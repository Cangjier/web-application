namespace WebApplication;
public class NotifyWindow
{
    public NotifyWindow(WebViewManager webViewManager)
    {
        NotifyWebform = new WebForm(webViewManager);
        NotifyWebform.Opacity = 0;
        NotifyWebform.Deactivate += (sender, e) => Hide();
        NotifyWebform.FormBorderStyle = FormBorderStyle.None;
        NotifyWebform.ShowInTaskbar = false;
        NotifyWebform.Size = new Size(200, 400);
        NotifyWebform.Show();
        NotifyWebform.FormClosing += (sender, e) =>
        {
            e.Cancel = true;
            Hide();
        };
        GlobalMouseHook = new GlobalMouseHook(NotifyWebform);
        GlobalMouseHook.MouseClickOutside += (pos) => Hide();
    }

    private GlobalMouseHook GlobalMouseHook;

    public WebForm NotifyWebform { get; }

    public void Show(Point position)
    {
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
        NotifyWebform.StartPosition = FormStartPosition.Manual;
        NotifyWebform.Location = new Point(x, y);

        // 显示窗口
        NotifyWebform.Opacity = 1;
    }

    public void Hide()
    {
        NotifyWebform.Opacity = 0;
    }

    public void Navigate(string url)
    {
        NotifyWebform.WebView?.CoreWebView2.Navigate(url);
    }

    public async Task Initialize()
    {
        await NotifyWebform.Initialize();
        if (NotifyWebform.WebView != null)
        {
            var webview2 = NotifyWebform.WebView;
            webview2.CoreWebView2.Settings.IsStatusBarEnabled = false;
        }
    }
}
