using System.Runtime.InteropServices;
using TidyHPC.Loggers;

namespace WebApplication;

/// <summary>
/// 
/// </summary>
public partial class MainForm : WebForm
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public MainForm():base(WindowMode.Singleton)
    {
        Location = new Point(-2000, -2000);
        Size = new Size(0,0);
        Utils.DpiHelper.GetDpiScaleFactor(this);
        Utils.DpiHelper.Hwnd = Handle;
        InitializeComponent();
        notifyIcon.Icon = Icon;
        notifyIcon.MouseUp += NotifyIcon_MouseUp;
        WebViewManager = new WebViewManager(TaskScheduler.FromCurrentSynchronizationContext());
        WebApplications = new WebApplications(WebViewManager);
        FormClosing += (sender, e) =>
        {
            e.Cancel = true;
            Hide();
        };
        WebViewManager.TaskFactory.StartNew(async () =>
        {
            try
            {
                Hide();
                if (await WebApplications.WebServices.IsStarted())
                {
                    Environment.Exit(0);
                }
                await WebViewManager.Initialize(new WebViewEnvironmentConfig
                {
                    UserDataDirectory = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "UserData")
                });
                await WebApplications.Initialize();
                await WebApplications.Register("home", this);
                if (WebApplications.ApplicationConfig.Router.Home.StartsWith("http"))
                {
                    WebView?.CoreWebView2.Navigate($"{WebApplications.ApplicationConfig.Router.Home}");
                }
                else
                {
                    WebView?.CoreWebView2.Navigate($"http://127.0.0.1:{WebApplications.WebServices.Port}{WebApplications.ApplicationConfig.Router.Home}");
                }
                _ = WebViewManager.TaskFactory.StartNew(async () => await WebViewManager.AddWebView(2));
            }
            catch(Exception e)
            {
                Logger.Error(e);
                throw;
            }
        });
    }

    /// <summary>
    /// 是否隐藏工具栏
    /// </summary>
    public bool IsHideToolbar { get; set; } = false;

    /// <summary>
    /// 图标路径
    /// </summary>
    public string FaviconPath => Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "favicon.ico");

    /// <summary>
    /// 加载上次图标
    /// </summary>
    public void LoadLastIcon()
    {
        try
        {
            if (File.Exists(FaviconPath))
            {
                using var stream = new FileStream(FaviconPath, FileMode.Open);
                SetIcon(new Icon(stream));
            }
        }
        catch
        {

        }
        
    }

    /// <summary>
    /// 设置图标
    /// </summary>
    /// <param name="icon"></param>
    public override void SetIcon(Icon icon)
    {
        base.SetIcon(icon);
        notifyIcon.Icon = icon;
        // 将Icon保存到程序同目录下的favicon.ico文件
        try
        {
            using var stream = new FileStream(FaviconPath, FileMode.OpenOrCreate);
            icon.Save(stream);
        }
        catch
        {

        }
    }

    /// <summary>
    /// 设置标题
    /// </summary>
    /// <param name="title"></param>
    public override void SetTitle(string title)
    {
        base.SetTitle(title);
        notifyIcon.Text = title;
    }

    private WebApplications WebApplications { get; }

    private void NotifyIcon_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right) // 右键点击
        {
            WebApplications.NotifyWindow.Show(Cursor.Position);
        }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        try
        {
            Logger.Info($"GetAwareness:{Utils.DpiHelper.GetAwareness()}");
        }
        catch(Exception ex)
        {
            Logger.Error("Failed to get DPI awareness. Ensure the application is DPI-aware.",ex);
        }
    }

    /// <summary>
    /// 处理窗口消息，特别是 DPI 改变事件
    /// </summary>
    /// <param name="m"></param>
    protected override void WndProc(ref Message m)
    {
        const int WM_DPICHANGED = 0x02E0;

        if (m.Msg == WM_DPICHANGED)
        {
            var suggestedRect = Marshal.PtrToStructure<RECT>(m.LParam);
            this.SetBounds(
                suggestedRect.Left,
                suggestedRect.Top,
                suggestedRect.Right - suggestedRect.Left,
                suggestedRect.Bottom - suggestedRect.Top
            );
        }

        base.WndProc(ref m);
    }

    /// <summary>
    /// 矩形结构体，用于处理窗口大小和位置
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        /// <summary>
        /// 左
        /// </summary>
        public int Left;
        /// <summary>
        /// 上
        /// </summary>
        public int Top;
        /// <summary>   
        /// 右
        /// </summary>
        public int Right;
        /// <summary>
        /// 下
        /// </summary>
        public int Bottom;
    }
}
