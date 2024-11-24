using Microsoft.Web.WebView2.WinForms;
using System.Drawing.Drawing2D;

namespace WebApplication;
/// <summary>
/// Web窗口
/// </summary>
public partial class WebForm : Form
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="webViewManager"></param>
    public WebForm(WebViewManager webViewManager)
    {
        StartPosition = FormStartPosition.CenterScreen;
        InitializeComponent();
        WebViewManager = webViewManager;
        SetWindowMode(WindowMode.Normal);
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="webViewManager"></param>
    /// <param name="mode"></param>
    public WebForm(WebViewManager webViewManager,WindowMode mode)
    {
        StartPosition = FormStartPosition.CenterScreen;
        InitializeComponent();
        WebViewManager = webViewManager;
        SetWindowMode(mode);
    }

    /// <summary>
    /// 
    /// </summary>
    protected WebForm(WindowMode mode)
    {
        StartPosition = FormStartPosition.CenterScreen;
        InitializeComponent();
        SetWindowMode(mode);
    }

    private void SetWindowMode(WindowMode mode)
    {
        Mode = mode;
        if (mode == WindowMode.Normal)
        {
            FormClosing += (sender, e) =>
            {
                if (WebViewManager == null)
                {
                    throw new InvalidOperationException("WebViewManager is null");
                }
                if (WebView != null)
                {
                    Controls.Remove(WebView);
                    WebViewManager.ReturnWebView(WebView);
                    WebView.CoreWebView2.Navigate("about:blank");
                    WebView = null;
                }
            };
        }
        else if (mode == WindowMode.Singleton)
        {
            FormClosing += (sender, e) =>
            {
                e.Cancel = true;
                Hide();
            };
        }
    }

    private int cornerRadius = 10; // 默认圆角半径

    private Region? lastRegion = null;

    /// <summary>
    /// 窗口模式
    /// </summary>
    public WindowMode Mode { get; private set; } = WindowMode.Normal;

    /// <summary>
    /// 边框样式
    /// </summary>
    public WindowBorderStyle BorderStyle { get; set; } = WindowBorderStyle.Normal;

    private void UpdateRegion()
    {
        var size = Size;
        if (BorderStyle == WindowBorderStyle.Round)
        {
            using GraphicsPath path = new();
            path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(this.ClientSize.Width - cornerRadius - 1, 0, cornerRadius, cornerRadius, 270, 90); // 右上角
            path.AddArc(this.ClientSize.Width - cornerRadius - 1, this.ClientSize.Height - cornerRadius - 1, cornerRadius, cornerRadius, 0, 90); // 右下角
            path.AddArc(0, this.ClientSize.Height - cornerRadius - 1, cornerRadius, cornerRadius, 90, 90); // 左下角
            path.CloseFigure();
            var newRegion = new Region(path);
            this.Region = newRegion;
            lastRegion?.Dispose();
            lastRegion = newRegion;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRegion();
    }

    /// <summary>
    /// WebView管理器
    /// </summary>
    public WebViewManager? WebViewManager { get; protected set; }

    /// <summary>
    /// WebView
    /// </summary>
    public WebView2? WebView { get; set; }


    private bool _isInitialized = false;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    public async Task Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        _isInitialized = true;
        if (WebViewManager == null)
        {
            throw new InvalidOperationException("WebViewManager is null");
        }
        WebView = await WebViewManager.GetWebView();
        Controls.Add(WebView);
        WebView.Dock = DockStyle.Fill;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
    }
}
