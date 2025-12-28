using Cangjie.TypeSharp;
using Cangjie.TypeSharp.System;
using System.Collections.Concurrent;
using System.Diagnostics;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;

namespace WebApplication;

/// <summary>
/// Web服务
/// </summary>
public class WebServices
{
    /// <summary>
    /// Web服务
    /// </summary>
    /// <param name="webApplications"></param>
    public WebServices(WebApplications webApplications)
    {
        WebApplications = webApplications;
        Server = new VizGroup.V1.Application();
        Server.ServiceScope.TaskService.ProgramCollection.CreateProgramByScriptContent = (filePath, content) =>
        {
            return new TSProgram(filePath, content);
        };
        Server.ServiceScope.TaskService.ProgramCollection.RunProgramByFilePathAndArgs = async (program, filePath, args) =>
        {
            if (program is not TSProgram programInstance)
            {
                throw new ArgumentException();
            }
            using var context = new Context();
            context.script_path = filePath;
            context.args = args;
            await programInstance.RunAsync(context);
            await context.Logger.QueueLogger.WaitForEmpty();
        };
        Server.ServiceScope.TaskService.ProgramCollection.RunProgramByFilePathAndContext = async (program, filePath, context) =>
        {
            if (program is not TSProgram programInstance)
            {
                throw new ArgumentException($"program is not a TSProgram");
            }
            var asContext = context as Context ?? throw new ArgumentException($"context is not a Context");
            asContext.script_path = filePath;
            asContext.args = [];
            return await programInstance.RunWithoutDisposeAsync(asContext);
        };
        Server.Register(Urls.Exit, Exit);
        Server.Register(Urls.Close, Close);
        Server.Register(Urls.Open, Open);
        Server.Register(Urls.OpenWithData, OpenWithData);
        Server.Register(Urls.GetDataByID, GetDataByID);
        Server.Register(Urls.Maximize, Maximize);
        Server.Register(Urls.Minimize, Minimize);
        Server.Register(Urls.Home, Home);
        Server.Register(Urls.MouseDownDrag, MouseDownDrag);
        Server.Register(Urls.Show, Show);
        Server.Register(Urls.Copy, Copy);
        Server.Register(Urls.Broadcast, Broadcast);
    }

    /// <summary>
    /// Web应用
    /// </summary>
    public WebApplications WebApplications { get; }

    /// <summary>
    /// Web服务
    /// </summary>
    public VizGroup.V1.Application Server { get; }

    /// <summary>
    /// 数据缓存
    /// </summary>
    public ConcurrentDictionary<Guid, Json> DataCache { get; } = new();

    /// <summary>
    /// Web服务端口
    /// </summary>
    public int Port { get; set; } = 12332;

    /// <summary>
    /// 插件目录
    /// </summary>
    public static string PluginsDirectory { get; } = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "plugins");

    /// <summary>
    /// 广播映射
    /// </summary>
    public ConcurrentDictionary<string, IWebsocketResponse> BroadcastMap { get; } = [];

    /// <summary>
    /// 启动
    /// </summary>
    /// <returns></returns>
    public async Task Start()
    {
        await UpdatePlugins();
        _ = Task.Run(async () =>
        {
            await Server.Start(new VizGroup.V1.ApplicationConfig()
            {
                EnablePlugins = true,
                ServerPorts = [Port],
                PluginsDirectory = PluginsDirectory,
                StaticResourcePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "build"),
                EnableDatabase = false
            });
        });
        await Server.OnConfigCompleted.Task;
        _ = Task.Run(async () =>
        {
            var context = new Context();
            context.Logger = Logger.LoggerFile;
            var scriptContext = context.context;
            scriptContext["server"] = new Json(new Server(Server));
            var disposes = await Server.ServiceScope.TaskService.PluginCollection.RunTypeSharpService(context);
            // 不对资源进行释放，保持长驻内存
            await Task.Delay(Timeout.Infinite);
            disposes.Dispose();
        });
    }

    /// <summary>
    /// 更新插件
    /// </summary>
    /// <returns></returns>
    public async Task UpdatePlugins()
    {
        var directories = Directory.GetDirectories(PluginsDirectory, ".git", SearchOption.AllDirectories);
        List<Task> tasks = [];
        foreach (var item in directories)
        {
            var path = Path.GetDirectoryName(item);
            var pullTask = Task.Run(async () =>
            {
                try
                {
                    var process = new Process();
                    process.StartInfo.FileName = "git";
                    process.StartInfo.Arguments = "pull";
                    process.StartInfo.WorkingDirectory = path;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    await process.WaitForExitAsync();
                }
                catch(Exception e)
                {
                    Logger.Error(e);
                }
            });
            tasks.Add(pullTask);
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 判断当前端口是否被占用，如果被占用则调用home接口，然后退出
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsStarted()
    {
        if(WebApplications.ApplicationConfig.Mode == ApplicationConfig.ApplicationMode.Multiple)
        {
            Port = Utils.GetAvailablePort();
            return false;
        }
        using HttpClient httpClient = new();
        try
        {
            if (Utils.IsTcpPortInUse(Port))
            {
                await httpClient.GetAsync($"http://127.0.0.1:{Port}/api/v1/app/home");
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// URL地址
    /// </summary>
    public class Urls
    {
        /// <summary>
        /// 退出Url
        /// </summary>
        public const string Exit = "/api/v1/app/exit";

        /// <summary>
        /// 关闭Url
        /// </summary>
        public const string Close = "/api/v1/app/close";

        /// <summary>
        /// 打开Url
        /// </summary>
        public const string Open = "/api/v1/app/open";

        /// <summary>
        /// 打开Url
        /// </summary>
        public const string OpenWithData = "/api/v1/app/openwithdata";

        /// <summary>
        /// 根据ID获取数据
        /// </summary>
        public const string GetDataByID = "/api/v1/app/getdatabyid";

        /// <summary>
        /// 最大化Url
        /// </summary>
        public const string Maximize = "/api/v1/app/maximize";

        /// <summary>
        /// 最小化Url
        /// </summary>
        public const string Minimize = "/api/v1/app/minimize";

        /// <summary>
        /// 显示home的Url
        /// </summary>
        public const string Home = "/api/v1/app/home";

        /// <summary>
        /// 显示指定窗口的Url
        /// </summary>
        public const string Show = "/api/v1/app/show";

        /// <summary>
        /// 显示home的Url
        /// </summary>
        public const string MouseDownDrag = "/api/v1/app/mousedowndrag";

        /// <summary>
        /// 复制数据
        /// </summary>
        public const string Copy = "/api/v1/app/copy";

        /// <summary>
        /// 广播消息到其他Web Page
        /// </summary>
        public const string Broadcast = "/api/v1/app/broadcast";
    }

    /// <summary>
    /// 退出
    /// </summary>
    /// <returns></returns>
    public async Task Exit()
    {
        Environment.Exit(0);
        await Task.CompletedTask;
    }

    /// <summary>
    /// 关闭指定窗口
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task Close(Guid id)
    {
        if (WebApplications.Applications.TryGetValue(id, out var form))
        {
            await WebApplications.WebViewManager.TaskFactory.StartNew(form.Close);
        }
    }

    /// <summary>
    /// 打开指定窗口
    /// </summary>
    /// <param name="url"></param>
    /// <param name="location"></param>
    /// <param name="resident"></param>
    /// <returns></returns>
    public async Task Open(string url,Json location,bool resident=false)
    {
        await WebApplications.WebViewManager.TaskFactory.StartNew(async () => await WebApplications.New(url, location, resident));
    }

    /// <summary>
    /// 显示指定窗口
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task Show(Guid id)
    {
        if (WebApplications.Applications.TryGetValue(id, out var form))
        {
            await WebApplications.WebViewManager.TaskFactory.StartNew(() =>
            {
                form.Show();
                form.TopMost = !form.TopMost;
                form.TopMost = !form.TopMost;
            });
        }
    }

    /// <summary>
    /// 打开指定窗口
    /// </summary>
    /// <param name="url"></param>
    /// <param name="location"></param>
    /// <param name="data"></param>
    /// <param name="dataID"></param>
    /// <param name="resident"></param>
    /// <returns></returns>
    public async Task OpenWithData(string url, Json location, Guid dataID, Json data,bool resident = false)
    {
        DataCache[dataID] = data;
        await WebApplications.WebViewManager.TaskFactory.StartNew(async () => await WebApplications.New(url, location, resident));
    }

    /// <summary>
    /// 根据ID获取数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Json GetDataByID(Guid id)
    {
        if(DataCache.TryRemove(id, out var data))
        {
            return data;
        }
        return Json.Null;
    }

    /// <summary>
    /// 最大化指定窗口
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task Maximize(Guid id)
    {
        if (WebApplications.Applications.TryGetValue(id, out var form))
        {
            await WebApplications.WebViewManager.TaskFactory.StartNew(() =>
            {
                if(form.GetWindowState()== FormWindowState.Maximized)
                {
                    form.RestoreWindow();
                }
                else
                {
                    form.MaximizeToCurrentScreen();
                }
                //form.WindowState = FormWindowState.Maximized;
            });
        }
    }

    /// <summary>
    /// 最小化指定窗口
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task Minimize(Guid id)
    {
        if (WebApplications.Applications.TryGetValue(id, out var form))
        {
            await WebApplications.WebViewManager.TaskFactory.StartNew(() =>
            {
                form.WindowState = FormWindowState.Minimized;
            });
        }
    }

    /// <summary>
    /// 复制数据到剪贴板
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task Copy(string text)
    {
        await WebApplications.WebViewManager.TaskFactory.StartNew(() =>
        {
            Clipboard.SetText(text);
        });
    }

    private DateTime LastMouseDownDragDateTime { get; set; } = DateTime.MinValue;

    /// <summary>
    /// 鼠标按下拖拽
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task MouseDownDrag(Guid id)
    {
        if (WebApplications.Applications.TryGetValue(id, out var form))
        {
            await WebApplications.WebViewManager.TaskFactory.StartNew(() =>
            {
                form.EventForwarder.MouseDownDrag();
                if(LastMouseDownDragDateTime.AddSeconds(0.5) > DateTime.Now)
                {
                    if(form.WindowState == FormWindowState.Normal)
                    {
                        form.WindowState = FormWindowState.Maximized;
                    }
                    else
                    {
                        form.WindowState = FormWindowState.Normal;
                    }
                }
                LastMouseDownDragDateTime = DateTime.Now;
            });
        }
    }

    /// <summary>
    /// 显示主页
    /// </summary>
    /// <returns></returns>
    public async Task Home()
    {
        if (WebApplications.KnownApplications.TryGetValue("home", out var id))
        {
            if (WebApplications.Applications.TryGetValue(id, out var form))
            {
                await WebApplications.WebViewManager.TaskFactory.StartNew(() =>
                {
                    //if (form.Visible == false)
                    //{
                    //    form.Visible = true;
                    //}

                    var mainform = form as MainForm;
                    if (mainform?.IsHideToolbar==false)
                    {
                        //var location = form.Location;
                        //form.Opacity = 1;
                        //form.ShowInTaskbar = true;
                        //form.FormBorderStyle = FormBorderStyle.Sizable;
                        //form.HideTitleBar();
                        //form.Location = location;
                        form.SetLocation(new LocationInterface()
                        {
                            X="center",
                            Y="center",
                            Width = "80%",
                            Height = "80%"
                        });
                    }
                    
                    form.Show();
                    
                    form.TopMost = !form.TopMost;
                    form.TopMost = !form.TopMost;
                    if (Utils.StartupWatch.IsRunning)
                    {
                        Utils.StartupWatch.Stop();
                        Logger.Info($"Startup time: {Utils.StartupWatch.ElapsedMilliseconds}ms");
                    }
                });
            }
        }
    }

    /// <summary>
    /// 广播消息到其他Web Page，不包括发送消息的页面
    /// <para>仅支持websocket</para>
    /// </summary>
    /// <returns></returns>
    public async Task Broadcast(Session session)
    {
        if (session.IsWebSocket==false)
        {
            return;
        }
        var message = await session.Cache.GetRequstBodyJson();
        var action = message.Read("action", string.Empty);
        var appId = message.Read("app_id", string.Empty);
        if (action == "register")
        {
            Logger.Debug($"Register broadcast client: {appId}");
            if (string.IsNullOrEmpty(appId) == false)
            {
                BroadcastMap[appId] = session.WebSocketResponse ?? throw new NullReferenceException();
            }
            else
            {
                throw new ArgumentException("app_id is null or empty");
            }
        }
        else if(action == "broadcast")
        {
            var others = BroadcastMap.Where(x => x.Key != appId).ToArray();
            Logger.Debug($"Broadcast to {others.Length} clients");
            foreach (var item in others)
            {
                var websocketResponse = item.Value;
                var itemKey = item.Key;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await websocketResponse.SendMessage(message.Get("data").ToString());
                    }
                    catch (Exception e)
                    {
                        BroadcastMap.TryRemove(itemKey, out var _);
                        Logger.Error("Broadcast failed",e);
                    }
                });
            }
        }
        else
        {
            Logger.Error($"Broadcast Unknown action: {action}");
        }
    }
}
