using Cangjie.TypeSharp;
using Cangjie.TypeSharp.System;
using System.Collections.Concurrent;
using System.Diagnostics;
using TidyHPC.LiteJson;

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
    /// 启动
    /// </summary>
    /// <returns></returns>
    public async Task Start()
    {
        await UpdatePlugins();
        _ = Server.Start(new VizGroup.V1.ApplicationConfig()
        {
            EnablePlugins = true,
            ServerPorts = [Port],
            PluginsDirectory = PluginsDirectory,
            StaticResourcePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "build"),
            EnableDatabase = false
        });
        await Server.OnConfigCompleted.Task;
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
                var process = new Process();
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = "pull";
                process.StartInfo.WorkingDirectory = path;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                // 设置超时3秒取消
                using CancellationTokenSource cts = new();
                cts.CancelAfter(3000);
                await process.WaitForExitAsync(cts.Token);
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
                form.WindowState = FormWindowState.Maximized;
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
                    if (form.Visible == false)
                    {
                        form.Visible = true;
                    }
                    //form.Opacity = 1;
                    //form.HideTitleBar();
                    form.Show();
                    form.TopMost = !form.TopMost;
                    form.TopMost = !form.TopMost;
                });
            }
        }
    }
}
