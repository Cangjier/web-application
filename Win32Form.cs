using System.Runtime.InteropServices;

namespace WebApplication;

/// <summary>
/// Win32窗体
/// </summary>
public class Win32Form : Form
{
    /// <summary>
    /// 常量HTLEFT
    /// </summary>
    public const int HTLEFT = 10;
    /// <summary>
    /// 常量HTRIGHT
    /// </summary>
    public const int HTRIGHT = 11;
    /// <summary>
    /// 常量HTTOP
    /// </summary>
    public const int HTTOP = 12;
    /// <summary>
    /// 常量HTTOPLEFT
    /// </summary>
    public const int HTTOPLEFT = 13;
    /// <summary>
    /// 常量HTTOPRIGHT
    /// </summary>
    public const int HTTOPRIGHT = 14;
    /// <summary>
    /// 常量HTBOTTOM
    /// </summary>
    public const int HTBOTTOM = 15;
    /// <summary>
    /// 常量HTBOTTOMLEFT
    /// </summary>
    public const int HTBOTTOMLEFT = 16;
    /// <summary>
    /// 常量HTBOTTOMRIGHT
    /// </summary>
    public const int HTBOTTOMRIGHT = 17;
    /// <summary>
    /// 常量WM_SYSCOMMAND
    /// </summary>
    public const int WM_SYSCOMMAND = 0x0112;
    /// <summary>
    /// 常量SC_MINIMIZE
    /// </summary>
    public const int SC_MINIMIZE = 0xF020;
    /// <summary>
    /// 常量SC_RESTORE
    /// </summary>
    public const int SC_RESTORE = 0xF120;

    /// <summary>
    /// SetWindowLong
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="nIndex"></param>
    /// <param name="dwNewLong"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    /// <summary>
    /// GetWindowLong
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="nIndex"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    /// <summary>
    /// UpdateLayeredWindow
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="hdcDst"></param>
    /// <param name="pptDst"></param>
    /// <param name="psize"></param>
    /// <param name="hdcSrc"></param>
    /// <param name="pptSrc"></param>
    /// <param name="crKey"></param>
    /// <param name="pblend"></param>
    /// <param name="dwFlags"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UpdateLayeredWindow(IntPtr hWnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

    /// <summary>
    /// GetDC
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetDC(IntPtr hWnd);

    /// <summary>
    /// ReleaseDC
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="hDC"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    /// <summary>
    /// CreateCompatibleDC
    /// </summary>
    /// <param name="hdc"></param>
    /// <returns></returns>
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    /// <summary>
    /// DeleteDC
    /// </summary>
    /// <param name="hdc"></param>
    /// <returns></returns>
    [DllImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteDC(IntPtr hdc);

    /// <summary>
    /// SelectObject
    /// </summary>
    /// <param name="hdc"></param>
    /// <param name="hgdiobj"></param>
    /// <returns></returns>
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    /// <summary>
    /// DeleteObject
    /// </summary>
    /// <param name="hObject"></param>
    /// <returns></returns>
    [DllImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject(IntPtr hObject);

    /// <summary>
    /// SetLayeredWindowAttributes
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="crKey"></param>
    /// <param name="bAlpha"></param>
    /// <param name="dwFlags"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    /// <summary>
    /// POINT
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        /// <summary>
        /// X坐标
        /// </summary>
        public int X;
        /// <summary>
        /// Y坐标
        /// </summary>
        public int Y;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    /// <summary>
    /// SIZE
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        /// <summary>
        /// 宽度
        /// </summary>
        public int cx;
        /// <summary>
        /// 高度
        /// </summary>
        public int cy;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        public SIZE(int cx, int cy)
        {
            this.cx = cx;
            this.cy = cy;
        }
    }
    /// <summary>
    /// BLENDFUNCTION
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BLENDFUNCTION
    {
        /// <summary>
        /// Alpha透明度
        /// </summary>
        public byte BlendOp;
        /// <summary>
        /// BlendFlags
        /// </summary>
        public byte BlendFlags;
        /// <summary>
        /// SourceConstantAlpha
        /// </summary>
        public byte SourceConstantAlpha;
        /// <summary>
        /// AlphaFormat
        /// </summary>
        public byte AlphaFormat;
    }

    /// <summary>
    /// ULW_ALPHA
    /// </summary>
    public const int ULW_ALPHA = 0x00000002;
    /// <summary>
    /// AC_SRC_OVER
    /// </summary>
    public const byte AC_SRC_OVER = 0x00;
    /// <summary>
    /// AC_SRC_ALPHA
    /// </summary>
    public const byte AC_SRC_ALPHA = 0x01;

    /// <summary>
    /// GWL_EXSTYLE
    /// </summary>
    public const int GWL_EXSTYLE = -20;
    /// <summary>
    /// WS_EX_LAYERED
    /// </summary>
    public const uint WS_EX_LAYERED = 0x80000;
    /// <summary>
    /// WM_EXITSIZEMOVE
    /// </summary>
    public const int WM_EXITSIZEMOVE = 0x0232;
    /// <summary>
    /// LWA_ALPHA
    /// </summary>
    public const int LWA_ALPHA = 0x2;
    /// <summary>
    /// LWA_COLORKEY
    /// </summary>
    public const uint LWA_COLORKEY = 0x00000001;

    //private const int WM_SYSCOMMAND = 0x112;
    private const int SC_SIZE = 0xF000;
    private const int WM_NCHITTEST = 0x84;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_MOUSEMOVE = 0x0200;
    private const int WS_EX_TRANSPARENT = 0x00000020;

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private struct Margins
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    public int PaddingValue { get; set; } = 3;

    /// <summary>
    /// Win32窗体
    /// </summary>
    public Win32Form()
    {
        FormBorderStyle = FormBorderStyle.None;
        DisableTopbar();
        this.Padding = new Padding(PaddingValue);
        this.BackColor = Color.White;
    }

    // 设置窗体的圆角区域
    private void SetFormRegion(int cornerRadius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        // 添加四个圆角
        path.AddArc(0, 0, cornerRadius*2, cornerRadius*2, 180, 90); // 左上角
        path.AddArc(Width - cornerRadius * 2, 0, cornerRadius*2, cornerRadius*2, 270, 90); // 右上角
        path.AddArc(Width - cornerRadius * 2, this.Height - cornerRadius * 2, cornerRadius*2, cornerRadius*2, 0, 90); // 右下角
        path.AddArc(0, this.Height - cornerRadius*2, cornerRadius*2, cornerRadius*2, 90, 90); // 左下角

        // 创建一个区域，表示窗体的透明区域
        path.CloseFigure();
        this.Region = new Region(path);
    }

    // 窗体重绘时重新设置区域，确保圆角有效
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        SetFormRegion(PaddingValue);
    }

    /// <summary>
    /// 启用调整大小
    /// </summary>
    public bool EnableResize { get; set; } = true;

    /// <summary>
    /// 禁用顶部栏
    /// </summary>
    public void DisableTopbar()
    {
        uint WS_SYSMENU = 0x00080000; // 系统菜单
        uint WS_MINIMIZEBOX = 0x20000; // 最大最小化按钮
        uint windowLong = (GetWindowLong(this.Handle, -16));
        SetWindowLong(this.Handle, -16, windowLong | WS_SYSMENU | WS_MINIMIZEBOX);
    }

    /// <summary>
    /// 窗体大小调整结束
    /// </summary>
    public virtual void OnResizeEnd()
    {

    }

    /// <summary>
    /// WndProc
    /// </summary>
    /// <param name="m"></param>
    protected override void WndProc(ref Message m)
    {
        const int WM_NCHITTEST = 0x84;
        const int RESIZE_HANDLE_SIZE = 10;

        if (EnableResize)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                var cursor = PointToClient(Cursor.Position);

                // Handle resizing logic
                if (cursor.X <= RESIZE_HANDLE_SIZE && cursor.Y <= RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTTOPLEFT;
                }
                else if (cursor.X >= this.Width - RESIZE_HANDLE_SIZE && cursor.Y <= RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTTOPRIGHT;
                }
                else if (cursor.X <= RESIZE_HANDLE_SIZE && cursor.Y >= this.Height - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTBOTTOMLEFT;
                }
                else if (cursor.X >= this.Width - RESIZE_HANDLE_SIZE && cursor.Y >= this.Height - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                }
                else if (cursor.X <= RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTLEFT;
                }
                else if (cursor.X >= this.Width - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTRIGHT;
                }
                else if (cursor.Y <= RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTTOP;
                }
                else if (cursor.Y >= this.Height - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HTBOTTOM;
                }
            }
            else if (m.Msg == WM_EXITSIZEMOVE)
            {
                OnResizeEnd();
            }
            else
            {
                base.WndProc(ref m);
            }
        }
        else
        {
            base.WndProc(ref m);
        }
    }
}

