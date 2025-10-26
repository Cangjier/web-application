using TidyHPC.Loggers;
namespace WebApplication;

/// <summary>
/// 通知窗口，用于任务栏菜单
/// </summary>
public class NotifyWindow
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="webViewManager"></param>
    /// <param name="webApplications"></param>
    public NotifyWindow(WebViewManager webViewManager, WebApplications webApplications)
    {
        NotifyWebform = new WebForm(webViewManager);
        NotifyWebform.Deactivate += (sender, e) => NotifyWebform.Hide();
        NotifyWebform.ShowInTaskbar = false;
        NotifyWebform.FormBorderStyle = FormBorderStyle.None;
        NotifyWebform.BorderStyle = WindowBorderStyle.Round;
        NotifyWebform.StartPosition = FormStartPosition.Manual;
        NotifyWebform.Size = new Size(100, 150);
        NotifyWebform.Hide();
        NotifyWebform.FormClosing += (sender, e) =>
        {
            e.Cancel = true;
            NotifyWebform.Hide();
        };
        GlobalMouseHook = new GlobalMouseOutsideHook(NotifyWebform);
        GlobalMouseHook.MouseClickOutside += (pos) =>
        {
            if (NotifyWebform.Visible)
            {
                Logger.Info($"GlobalMouseHook.MouseClickOutside:{pos.X},{pos.Y}, Bounds={NotifyWebform.Bounds}");
            }
            NotifyWebform.Hide();
        };
        WebApplications = webApplications;
    }

    private GlobalMouseOutsideHook GlobalMouseHook;

    private WebApplications WebApplications { get; }

    /// <summary>
    /// 提醒用的窗口
    /// </summary>
    public WebForm NotifyWebform { get; }

    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="position"></param>
    public void Show(Point position)
    {
        var size = NotifyWebform.Size;
        NotifyWebform.TopMost = true;
        // 获取屏幕的工作区域
        Rectangle screenBounds = Screen.FromPoint(position).WorkingArea;

        // 窗口的默认大小（可根据实际情况调整）
        int windowWidth = NotifyWebform.Width;
        int windowHeight = NotifyWebform.Height;

        // 初始化弹窗位置
        int x = position.X;
        int y = position.Y;

        // 检查上下边界
        if (position.Y < screenBounds.Top + windowHeight) // 靠近顶部
        {
            y = position.Y; // 显示在鼠标点击下方
        }
        else if (position.Y + windowHeight > screenBounds.Bottom) // 靠近底部
        {
            y = position.Y - windowHeight; // 显示在鼠标点击上方
        }

        // 检查左右边界
        if (position.X + windowWidth > screenBounds.Right) // 靠近右侧
        {
            x = position.X - windowWidth; // 显示在鼠标点击左侧
        }
        else if (position.X < screenBounds.Left + windowWidth) // 靠近左侧
        {
            x = position.X; // 显示在鼠标点击右侧
        }

        // 设置窗口位置
        
        NotifyWebform.Location = new Point(x, y);
        // 显示窗口
        NotifyWebform.Show();
        NotifyWebform.Size = size;
       
    }

    /// <summary>
    /// 导航到指定的地址
    /// </summary>
    /// <param name="url"></param>
    public void Navigate(string url)
    {
        NotifyWebform.WebView?.CoreWebView2.Navigate(url);
        //NotifyWebform.WebView?.CoreWebView2.OpenDevToolsWindow();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    public async Task Initialize()
    {
        //await NotifyWebform.Initialize();
        await WebApplications.Register("notify", NotifyWebform);
        if (NotifyWebform.WebView != null)
        {
            var webview2 = NotifyWebform.WebView;
            webview2.CoreWebView2.Settings.IsStatusBarEnabled = false;
        }

        //if (Path.GetFileNameWithoutExtension(Environment.ProcessPath) == "WebApplication")
        //{
        //    NotifyWebform.WebView?.CoreWebView2.OpenDevToolsWindow();
        //}
    }
}
