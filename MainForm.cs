using TidyHPC.Loggers;

namespace WebApplication;

/// <summary>
/// 
/// </summary>
public partial class MainForm : WebForm
{
    /// <summary>
    /// ���캯��
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
    /// ͼ��·��
    /// </summary>
    public string FaviconPath => Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "favicon.ico");

    /// <summary>
    /// �����ϴ�ͼ��
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
    /// ����ͼ��
    /// </summary>
    /// <param name="icon"></param>
    public override void SetIcon(Icon icon)
    {
        base.SetIcon(icon);
        notifyIcon.Icon = icon;
        // ��Icon���浽����ͬĿ¼�µ�favicon.ico�ļ�
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
    /// ���ñ���
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
        if (e.Button == MouseButtons.Right) // �Ҽ����
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
