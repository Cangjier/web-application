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
        Utils.DpiHelper.GetDpiScaleFactor(this);
        InitializeComponent();
        LoadLastIcon();
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
                if (await WebApplications.WebServices.IsStarted())
                {
                    Environment.Exit(0);
                }
                Hide();
                await WebViewManager.Initialize(new WebViewEnvironmentConfig
                {
                    UserDataDirectory = "UserData"
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
        SetWindowCenter(0.8f);
        //Hide();
    }
}
