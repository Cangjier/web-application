namespace WebApplication;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        ShowInTaskbar = false;
        Opacity = 0;
        notifyIcon.MouseUp += NotifyIcon_MouseUp;
        WebViewManager = new WebViewManager(TaskScheduler.FromCurrentSynchronizationContext());
        WebApplications = new WebApplications(WebViewManager);
        WebViewManager.TaskFactory.StartNew(async () =>
        {
            await WebViewManager.Initialize(new WebViewEnvironmentConfig
            {
                UserDataDirectory = "UserData"
            });
            await WebApplications.Initialize();
            await WebApplications.StartApplication("http://www.baidu.com");
        });
    }

    private WebApplications WebApplications { get; }

    private WebViewManager WebViewManager { get; }

    private void NotifyIcon_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right) // ÓÒ¼üµã»÷
        {
            WebApplications.NotifyWindow.Show(Cursor.Position);
        }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {

    }
}
