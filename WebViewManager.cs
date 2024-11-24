using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using TidyHPC.Queues;

namespace WebApplication;

/// <summary>
/// WebView管理器
/// </summary>
public class WebViewManager
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="taskScheduler"></param>
    public WebViewManager(TaskScheduler taskScheduler)
    {
        TaskScheduler = taskScheduler;
        TaskFactory = new TaskFactory(TaskScheduler);
    }

    /// <summary>
    /// UI线程调度器
    /// </summary>
    public TaskScheduler TaskScheduler { get; }

    /// <summary>
    /// UITask工厂
    /// </summary>
    public TaskFactory TaskFactory { get; }

    /// <summary>
    /// WebView2环境
    /// </summary>
    private CoreWebView2Environment? WebView2Environment { get; set; }

    /// <summary>
    /// WebView2队列
    /// </summary>
    private WaitQueue<WebView2> WebView2Queue { get; set; } = new();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 获取WebView
    /// </summary>
    /// <returns></returns>
    public async Task<WebView2> GetWebView()
    {
        return await WebView2Queue.Dequeue();
    }

    /// <summary>
    /// 归还WebView
    /// </summary>
    /// <param name="webView"></param>
    public void ReturnWebView(WebView2 webView)
    {
        WebView2Queue.Enqueue(webView);
    }
}
