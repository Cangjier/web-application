using System.Collections.Concurrent;
using System.Windows.Forms;

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
    public ConcurrentDictionary<string,Guid> KnownApplications { get; } = new();    

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
        await NotifyWindow.Initialize();
        _ = Task.Run(WebServices.Start);
        NotifyWindow.Navigate($"http://127.0.0.1:{WebServices.Port}{ApplicationConfig.Router.TaskBarMenu}");
    }

    /// <summary>
    /// 启动应用
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task New(string url)
    {
        var form = new WebForm(WebViewManager);
        await Register(form);
        form.WebView?.CoreWebView2.Navigate(url);
        form.Show();
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
    public async Task Register(string? kownName,WebForm form)
    {
        var id = Guid.NewGuid();
        await form.Initialize();
        if (form.Mode == WindowMode.Normal)
        {
            form.FormClosing += (sender, e) => Applications.TryRemove(id, out _);
        }
        if (form.WebView != null)
        {
            await form.WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync($$"""
            window.webapplication = {
                "id": "{{id}}",
            }
            """);
            EventHandler<object> onTitleChanged = (sender, e) =>
            {
                if (form.Text != form.WebView?.CoreWebView2.DocumentTitle)
                {
                    form.Text = form.WebView?.CoreWebView2.DocumentTitle;
                }
            };
            form.WebView.CoreWebView2.DocumentTitleChanged += onTitleChanged;
            form.FormClosing += (sender, e) =>
            {
                if (form.WebView != null)
                {
                    form.WebView.CoreWebView2.DocumentTitleChanged -= onTitleChanged;
                }
            };
        }
        Applications.TryAdd(id, form);
        if(kownName != null)
        {
            KnownApplications.TryAdd(kownName, id);
        }
    }


}
