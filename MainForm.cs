namespace WebApplication;

public partial class MainForm : WebForm
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public MainForm():base(WindowMode.Singleton)
    {
        InitializeComponent();
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
            await WebViewManager.Initialize(new WebViewEnvironmentConfig
            {
                UserDataDirectory = "UserData"
            });
            await WebApplications.Initialize();
            await WebApplications.Register("home", this);
            WebView?.CoreWebView2.Navigate($"http://127.0.0.1:{WebApplications.WebServices.Port}{WebApplications.ApplicationConfig.Router.Home}");
        });
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

    }
}
