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
        Server.Register(Urls.Exit, Exit);
        Server.Register(Urls.Close, Close);
        Server.Register(Urls.Open, Open);
        Server.Register(Urls.Maximize, Maximize);
        Server.Register(Urls.Minimize, Minimize);
        Server.Register(Urls.Home, Home);
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
    /// Web服务端口
    /// </summary>
    public int Port { get; set; } = 12332;

    /// <summary>
    /// 启动
    /// </summary>
    /// <returns></returns>
    public async Task Start()
    {
        await Server.Start(new VizGroup.V1.ApplicationConfig()
        {
            EnablePlugins = true,
            ServerPorts = [Port],
            StaticResourcePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "build")
        });
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
        public const string Close = "/api/v1/app/close/(?<id>.*)";

        /// <summary>
        /// 打开Url
        /// </summary>
        public const string Open = "/api/v1/app/open/(?<url>.*)";

        /// <summary>
        /// 最大化Url
        /// </summary>
        public const string Maximize = "/api/v1/app/maximize/(?<id>.*)";

        /// <summary>
        /// 最小化Url
        /// </summary>
        public const string Minimize = "/api/v1/app/minimize/(?<id>.*)";

        /// <summary>
        /// 显示home的Url
        /// </summary>
        public const string Home = "/api/v1/app/home";
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
    /// <returns></returns>
    public async Task Open(string url)
    {
        await WebApplications.WebViewManager.TaskFactory.StartNew(async () => await WebApplications.New(url));
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
    /// 显示主页
    /// </summary>
    /// <returns></returns>
    public async Task Home()
    {
        if (WebApplications.KnownApplications.TryGetValue("home", out var id))
        {
            if (WebApplications.Applications.TryGetValue(id, out var form))
            {
                await WebApplications.WebViewManager.TaskFactory.StartNew(form.Show);
            }
        }
    }
}
