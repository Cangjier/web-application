using Microsoft.Web.WebView2.WinForms;
using System.Drawing.Drawing2D;

namespace WebApplication;
/// <summary>
/// Web窗口
/// </summary>
public partial class WebForm : Win32Form
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="webViewManager"></param>
    public WebForm(WebViewManager webViewManager)
    {
        InitializeComponent();
        WebViewManager = webViewManager;
        SetWindowMode(WindowMode.Normal);
        HideTitleBar();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="webViewManager"></param>
    /// <param name="mode"></param>
    public WebForm(WebViewManager webViewManager,WindowMode mode)
    {
        InitializeComponent();
        WebViewManager = webViewManager;
        SetWindowMode(mode);
        HideTitleBar();
    }

    /// <summary>
    /// 
    /// </summary>
    protected WebForm(WindowMode mode)
    {
        InitializeComponent();
        SetWindowMode(mode);
        HideTitleBar();
    }

    /// <summary>
    /// 设置位置
    /// </summary>
    /// <param name="location"></param>
    public void SetLocation(LocationInterface location)
    {
        // 先设置大小，再设置位置
        var width = location.Width;
        var height = location.Height;
        var x = location.X;
        var y = location.Y;
        if (width.IsNumber)
        {
            Width = width.ToInt32;
        }
        else if (width.IsString)
        {
            var widthString = width.AsString;
            if (widthString.EndsWith("%"))
            {
                var ratio = float.Parse(widthString.Substring(0, widthString.Length - 1)) / 100;
                var currentScreen = Screen.FromControl(this);
                var workingArea = currentScreen.WorkingArea;
                Width = (int)(workingArea.Width * ratio * Utils.DpiHelper.DpiScaleFactor);
            }
        }
        if (height.IsNumber)
        {
            Height = height.ToInt32;
        }
        else if (height.IsString)
        {
            var heightString = height.AsString;
            if (heightString.EndsWith("%"))
            {
                var ratio = float.Parse(heightString.Substring(0, heightString.Length - 1)) / 100;
                var currentScreen = Screen.FromControl(this);
                var workingArea = currentScreen.WorkingArea;
                Height = (int)(workingArea.Height * ratio * Utils.DpiHelper.DpiScaleFactor);
            }
        }

        if (x.IsNumber)
        {
            Left = x.ToInt32;
        }
        else if (x.IsString)
        {
            var xString = x.AsString;
            if (xString.EndsWith("%"))
            {
                var ratio = float.Parse(xString.Substring(0, xString.Length - 1)) / 100;
                var currentScreen = Screen.FromControl(this);
                var workingArea = currentScreen.WorkingArea;
                Left = workingArea.Left + (int)(workingArea.Width * ratio * Utils.DpiHelper.DpiScaleFactor);
            }
            else if (xString == "left")
            {
                Left = 0;
            }
            else if (xString == "right")
            {
                Left = Screen.FromControl(this).WorkingArea.Right - Width;
            }
            else if (xString == "center")
            {
                Left = (Screen.FromControl(this).WorkingArea.Width - Width) / 2;
            }
        }

        if (y.IsNumber)
        {
            Top = y.ToInt32;
        }
        else if (y.IsString)
        {
            var yString = y.AsString;
            if (yString.EndsWith("%"))
            {
                var ratio = float.Parse(yString.Substring(0, yString.Length - 1)) / 100;
                var currentScreen = Screen.FromControl(this);
                var workingArea = currentScreen.WorkingArea;
                Top = workingArea.Top + (int)(workingArea.Height * ratio * Utils.DpiHelper.DpiScaleFactor);
            }
            else if (yString == "top")
            {
                Top = 0;
            }
            else if (yString == "bottom")
            {
                Top = Screen.FromControl(this).WorkingArea.Bottom - Height;
            }
            else if (yString == "center")
            {
                Top = (Screen.FromControl(this).WorkingArea.Height - Height) / 2;
            }
        }
    }

    /// <summary>
    /// 设置居中
    /// </summary>
    /// <param name="ratio"></param>
    public void SetWindowCenter(float ratio)
    {
        var currentScreen = Screen.FromControl(this);
        var workingArea = currentScreen.WorkingArea;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(workingArea.Left + (int)(workingArea.Width * (1- ratio) / 2 * Utils.DpiHelper.DpiScaleFactor), workingArea.Top + (int)(workingArea.Height * (1- ratio) / 2 * Utils.DpiHelper.DpiScaleFactor));
        Size = new Size((int)(workingArea.Width * ratio * Utils.DpiHelper.DpiScaleFactor), (int)(workingArea.Height * ratio * Utils.DpiHelper.DpiScaleFactor));
    }

    /// <summary>
    /// 设置居中
    /// </summary>
    /// <param name="xRatio"></param>
    /// <param name="yRatio"></param>
    public void SetWindowCenterByRatio(float xRatio,float yRatio)
    {
        var currentScreen = Screen.FromControl(this);
        var workingArea = currentScreen.WorkingArea;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(workingArea.Left + (int)(workingArea.Width * (1 - xRatio) / 2 * Utils.DpiHelper.DpiScaleFactor), workingArea.Top + (int)(workingArea.Height * (1 - yRatio) / 2 * Utils.DpiHelper.DpiScaleFactor));
        Size = new Size((int)(workingArea.Width * xRatio * Utils.DpiHelper.DpiScaleFactor), (int)(workingArea.Height * yRatio * Utils.DpiHelper.DpiScaleFactor));
    }

    /// <summary>
    /// 设置居中
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetWindowCenterSize(int width, int height)
    {
        var currentScreen = Screen.FromControl(this);
        var workingArea = currentScreen.WorkingArea;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(workingArea.Left + (workingArea.Width - width) / 2, workingArea.Top + (workingArea.Height - height) / 2);
        Size = new Size(width, height);
    }

    /// <summary>
    /// 设置窗口右下角位置
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetWindowBottomRightSize(int width, int height)
    {
        var currentScreen = Screen.FromControl(this);
        var workingArea = currentScreen.WorkingArea;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(workingArea.Right - width, workingArea.Bottom - height);
        Size = new Size(width, height);
    }

    /// <summary>
    /// 设置窗口左下角位置
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetWindowBottomLeftSize(int width, int height)
    {
        var currentScreen = Screen.FromControl(this);
        var workingArea = currentScreen.WorkingArea;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(workingArea.Left, workingArea.Bottom - height);
        Size = new Size(width, height);
    }

    /// <summary>
    /// 设置窗口右上角位置
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetWindowTopRightSize(int width, int height)
    {
        var currentScreen = Screen.FromControl(this);
        var workingArea = currentScreen.WorkingArea;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(workingArea.Right - width, workingArea.Top);
        Size = new Size(width, height);
    }

    /// <summary>
    /// 设置窗口左上角位置
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetWindowTopLeftSize(int width, int height)
    {
        var currentScreen = Screen.FromControl(this);
        var workingArea = currentScreen.WorkingArea;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(workingArea.Left, workingArea.Top);
        Size = new Size(width, height);
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

    /// <summary>
    /// 设置图标
    /// </summary>
    /// <param name="icon"></param>
    public virtual void SetIcon(Icon icon)
    {
        this.Icon = icon;
    }

    /// <summary>
    /// 设置标题
    /// </summary>
    /// <param name="title"></param>
    public virtual void SetTitle(string title)
    {
        this.Text = title;
    }

    /// <summary>
    /// 设置描述
    /// </summary>
    /// <param name="description"></param>
    public virtual void SetDescription(string description)
    {

    }
}
