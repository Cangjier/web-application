using System.Collections.Concurrent;
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
        Server.Register(Urls.Exit, Exit);
        Server.Register(Urls.Close, Close);
        Server.Register(Urls.Open, Open);
        Server.Register(Urls.OpenWithData, OpenWithData);
        Server.Register(Urls.GetDataByID, GetDataByID);
        Server.Register(Urls.Maximize, Maximize);
        Server.Register(Urls.Minimize, Minimize);
        Server.Register(Urls.Home, Home);
        Server.Register(Urls.MouseDownDrag, MouseDownDrag);
        
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
    /// <returns></returns>
    public async Task Open(string url,Json location)
    {
        await WebApplications.WebViewManager.TaskFactory.StartNew(async () => await WebApplications.New(url, location));
    }

    /// <summary>
    /// 打开指定窗口
    /// </summary>
    /// <param name="url"></param>
    /// <param name="location"></param>
    /// <param name="data"></param>
    /// <param name="dataID"></param>
    /// <returns></returns>
    public async Task OpenWithData(string url, Json location, Guid dataID, Json data)
    {
        DataCache[dataID] = data;
        await WebApplications.WebViewManager.TaskFactory.StartNew(async () => await WebApplications.New(url, location));
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
                    form.Show();
                    form.TopMost = !form.TopMost;
                    form.TopMost = !form.TopMost;
                });
            }
        }
    }
}
