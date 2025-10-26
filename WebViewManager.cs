using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using TidyHPC.Loggers;
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

    private async Task CreateWebView2()
    {
        try
        {
            WebView2? webView = null;
            TaskCompletionSource taskCompletionSource = new();
            _ = TaskFactory.StartNew(async () =>
            {
                try
                {
                    webView = new WebView2();
                    await webView.EnsureCoreWebView2Async(WebView2Environment);
                    webView.CoreWebView2.PermissionRequested += (sender, args) =>
                    {
                        args.State = CoreWebView2PermissionState.Allow;
                    };
                    webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

                    taskCompletionSource.SetResult();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            });
            await taskCompletionSource.Task;
            if (webView != null)
            {
                WebView2Queue.Enqueue(webView);
            }
        }
        catch(Exception e)
        {
            Logger.Error(e);
            throw;
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public async Task Initialize(WebViewEnvironmentConfig config)
    {
        try
        {
            if (Directory.Exists(config.UserDataDirectory) == false)
            {
                Directory.CreateDirectory(config.UserDataDirectory);
            }
            var localExecutableFolder = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "bin");
            WebView2Environment = await CoreWebView2Environment.CreateAsync(
                        browserExecutableFolder: Directory.Exists(localExecutableFolder) ? localExecutableFolder : null,
                        userDataFolder: config.UserDataDirectory,
                        options: new CoreWebView2EnvironmentOptions()
                        {
                            AdditionalBrowserArguments = "--disable-web-security --no-sandbox"
                        });
            WebView2Queue.OnDequeueStart = async () =>
            {
                if (WebView2Queue.CurrentCount == 0)
                {
                    await CreateWebView2();
                }
            };
        }
        catch(Exception e)
        {
            Logger.Error(e);
            throw;
        }
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
    /// 添加WebView
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public async Task AddWebView(int count)
    {
        for(int i = 0; i < count; i++)
        {
            await CreateWebView2();
        }
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
