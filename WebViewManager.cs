using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using TidyHPC.Queues;

namespace WebApplication;

/// <summary>
/// WebView管理器
/// </summary>
public class WebViewManager
{
    public WebViewManager(TaskScheduler taskScheduler)
    {
        TaskScheduler = taskScheduler;
        TaskFactory = new TaskFactory(TaskScheduler);
    }

    public TaskScheduler TaskScheduler { get; }

    public TaskFactory TaskFactory { get; }

    private CoreWebView2Environment? WebView2Environment { get; set; }

    private WaitQueue<WebView2> WebView2Queue { get; set; } = new();

    public async Task Initialize(WebViewEnvironmentConfig config)
    {
        WebView2Environment = await CoreWebView2Environment.CreateAsync(
                    userDataFolder: config.UserDataDirectory);
        WebView2Queue.OnDequeueStart = async () =>
        {
            if(WebView2Queue.CurrentCount == 0)
            {
                WebView2? webView = null;
                TaskCompletionSource taskCompletionSource = new();
                _ = TaskFactory.StartNew(async () =>
                {
                    webView = new WebView2();
                    await webView.EnsureCoreWebView2Async(WebView2Environment);
                    taskCompletionSource.SetResult();
                });
                await taskCompletionSource.Task;
                if (webView != null)
                {
                    WebView2Queue.Enqueue(webView);
                }
            }
        };
    }

    public async Task<WebView2> GetWebView()
    {
        return await WebView2Queue.Dequeue();
    }

    public void ReturnWebView(WebView2 webView)
    {
        WebView2Queue.Enqueue(webView);
    }
}
