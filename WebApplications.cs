using System.Collections.Concurrent;

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

    public NotifyWindow NotifyWindow { get; }

    public WebViewManager WebViewManager { get; }

    public WebApplications(WebViewManager webViewManager)
    {
        WebViewManager = webViewManager;
        NotifyWindow = new NotifyWindow(webViewManager);
    }

    public async Task Initialize()
    {
        await NotifyWindow.Initialize();
        NotifyWindow.Navigate("https://www.baidu.com");
    }

    public async Task StartApplication(string url)
    {
        var id = Guid.NewGuid();
        var form = new WebForm(WebViewManager);
        form.FormClosing += (sender, e) => Applications.TryRemove(id, out _);
        await form.Initialize();
        form.WebView?.CoreWebView2.Navigate(url);
        Applications.TryAdd(id, form);
        form.Show();
    }
}
