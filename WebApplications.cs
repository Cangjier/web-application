using Microsoft.Web.WebView2.Core;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows.Forms;
using TidyHPC.LiteDB.BasicValues;
using TidyHPC.Loggers;

namespace WebApplication;

/// <summary>
/// 应用
/// </summary>
public class WebApplications
{
    /// <summary>
    /// 应用集合
    /// </summary>
    public ConcurrentDictionary<Guid, WebForm> Applications { get; } = new();

    /// <summary>
    /// 已知的应用
    /// </summary>
    public ConcurrentDictionary<string, Guid> KnownApplications { get; } = new();

    /// <summary>
    /// 任务栏菜单窗口
    /// </summary>
    public NotifyWindow NotifyWindow { get; }

    /// <summary>
    /// Webview管理器
    /// </summary>
    public WebViewManager WebViewManager { get; }

    /// <summary>
    /// Web服务
    /// </summary>
    public WebServices WebServices { get; }

    /// <summary>
    /// 应用配置
    /// </summary>
    public ApplicationConfig ApplicationConfig { get; } = new();

    /// <summary>
    /// 启动进程
    /// </summary>
    public List<Process> StarupProcesses { get; } = new();

    /// <summary>
    /// 本地图标缓存
    /// </summary>
    public LocalFavicon LocalFavicon { get; } = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="webViewManager"></param>
    public WebApplications(WebViewManager webViewManager)
    {
        WebViewManager = webViewManager;
        NotifyWindow = new NotifyWindow(webViewManager);
        WebServices = new WebServices(this);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    public async Task Initialize()
    {
        StartupProcesses();
        await NotifyWindow.Initialize();
        await WebServices.Start();
        if (ApplicationConfig.Router.TaskBarMenu.StartsWith("http"))
        {
            NotifyWindow.Navigate($"{ApplicationConfig.Router.TaskBarMenu}");
        }
        else
        {
            NotifyWindow.Navigate($"http://127.0.0.1:{WebServices.Port}{ApplicationConfig.Router.TaskBarMenu}");
        }
    }

    /// <summary>
    /// 启动进程
    /// </summary>
    /// <returns></returns>
    public void StartupProcesses()
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            foreach (var process in StarupProcesses)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
        };
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            foreach (var process in StarupProcesses)
            {
                if (!process.HasExited)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch(Exception subException)
                    {
                        Logger.Error(subException);
                    }
                }
            }
        };
        foreach (var environment in ApplicationConfig.Environments)
        {
            try
            {
                EnvironmentVariableTarget target = environment.Type switch
                {
                    "user" => EnvironmentVariableTarget.User,
                    "process" => EnvironmentVariableTarget.Process,
                    "machine" => EnvironmentVariableTarget.Machine,
                    _ => EnvironmentVariableTarget.Process
                };
                var value = environment.Value.Replace("{.}", Path.GetDirectoryName(Environment.ProcessPath));
                if (environment.Action == "add")
                {
                    var oldValue = Environment.GetEnvironmentVariable(environment.Key)?.ToString() ?? "";
                    var oldItems = oldValue.Split(';').Select(item => item.ToLower());
                    if (oldItems.Contains(value.ToLower()))
                    {
                        continue;
                    }
                    Environment.SetEnvironmentVariable(environment.Key, $"{value};{Environment.GetEnvironmentVariable(environment.Key)}", target);
                    if (target != EnvironmentVariableTarget.Process)
                    {
                        Environment.SetEnvironmentVariable(environment.Key, $"{value};{Environment.GetEnvironmentVariable(environment.Key)}", EnvironmentVariableTarget.Process);
                    }
                    Logger.Info($"Add {target} environment variable {environment.Key}={value};{Environment.GetEnvironmentVariable(environment.Key)}");
                }
                else if (environment.Action == "set")
                {
                    Environment.SetEnvironmentVariable(environment.Key, value, target);
                    if (target != EnvironmentVariableTarget.Process)
                    {
                        Environment.SetEnvironmentVariable(environment.Key, value, EnvironmentVariableTarget.Process);
                    }
                    Logger.Info($"Set {target} environment variable {environment.Key}={value}");
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
        }
        foreach (var startup in ApplicationConfig.Startups)
        {
            try
            {
                Logger.Info($"Start process {startup.Path} {string.Join(" ", startup.Args)}");
                var process = new Process();
                process.StartInfo.FileName = startup.Path;
                foreach (var arg in startup.Args)
                {
                    process.StartInfo.ArgumentList.Add(arg);
                }
                process.StartInfo.WorkingDirectory = startup.WorkingDirectory;
                process.StartInfo.CreateNoWindow = startup.CreateNoWindow;
                process.Start();
                StarupProcesses.Add(process);
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
        }
        
    }

    /// <summary>
    /// 启动应用
    /// </summary>
    /// <param name="url"></param>
    /// <param name="location"></param>
    /// <param name="resident"></param>
    /// <returns></returns>
    public async Task New(string url, LocationInterface location,bool resident)
    {
        var form = new WebForm(WebViewManager);
        if (resident)
        {
            // get url path without query
            var uri = new Uri(url);
            var path = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            await Register(path, form);
        }
        else
        {
            await Register(form);
        }

            form.Visible = false;
        form.WebView?.CoreWebView2.Navigate(url);
        form.SetLocation(location);
        //form.Show();
        //form.TopMost = !form.TopMost;
        //form.TopMost = !form.TopMost;
    }

    /// <summary>
    /// 注册窗口
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    public async Task Register(WebForm form)
    {
        await Register(null, form);
    }

    /// <summary>
    /// 注册窗口
    /// </summary>
    /// <param name="kownName"></param>
    /// <param name="form"></param>
    /// <returns></returns>
    public async Task Register(string? kownName, WebForm form)
    {
        var id = Guid.NewGuid();
        await form.Initialize();
        if (form.Mode == WindowMode.Normal)
        {
            form.FormClosing += (sender, e) => Applications.TryRemove(id, out _);
        }
        if (form.WebView != null)
        {
            var scriptID = await form.WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync($$"""
            window.webapplication = {
                "id": "{{id}}",
            }
            """);
            EventHandler<object> onTitleChanged = (sender, e) =>
            {
                var title = form.WebView?.CoreWebView2.DocumentTitle;
                if (form.Text != title && title != null)
                {
                    form.SetTitle(title);
                }
            };
            EventHandler<CoreWebView2NavigationCompletedEventArgs> onIconChanged = async (sender, e) =>
            {
                var url = form.WebView?.CoreWebView2.FaviconUri;
                if (url != null)
                {
                    var icon = await LocalFavicon.GetOrDownload(url);
                    if (icon != null && form.Icon != icon)
                    {
                        form.SetIcon(icon);
                    }
                }
            };
            form.WebView.CoreWebView2.DocumentTitleChanged += onTitleChanged;
            form.WebView.CoreWebView2.NavigationCompleted += onIconChanged;

            form.FormClosing += (sender, e) =>
            {
                if (form.WebView != null)
                {
                    form.WebView.CoreWebView2.DocumentTitleChanged -= onTitleChanged;
                    form.WebView.CoreWebView2.NavigationCompleted -= onIconChanged;
                    form.WebView.CoreWebView2.RemoveScriptToExecuteOnDocumentCreated(scriptID);
                }
            };
        }
        Applications.TryAdd(id, form);
        if (kownName != null)
        {
            KnownApplications.TryAdd(kownName, id);
        }
    }
}
