using Microsoft.Web.WebView2.WinForms;

namespace WebApplication;
public partial class WebForm : Form
{
    public WebForm(WebViewManager webViewManager)
    {
        InitializeComponent();
        WebViewManager = webViewManager;
        FormClosing += WebForm_FormClosing;
    }

    private void WebForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (WebView != null)
        {
            Controls.Remove(WebView);
            WebViewManager.ReturnWebView(WebView);
            WebView.CoreWebView2.Navigate("about:blank");
            WebView = null;
        }
    }

    public WebViewManager WebViewManager { get;}

    public WebView2? WebView { get; set; }

    public async Task Initialize()
    {
        WebView = await WebViewManager.GetWebView();
        WebView.Dock = DockStyle.Fill;
        Controls.Add(WebView);
    }
}
