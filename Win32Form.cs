using System.Runtime.InteropServices;
using TidyHPC.Loggers;

namespace WebApplication;

/// <summary>
/// Win32窗体
/// </summary>
public class Win32Form : Form
{
    // 引用Windows API，修改窗体样式
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);


    // 获取窗口样式
    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    // 常量定义
    const int GWL_STYLE = -16;

    private FormWindowState lastWindowState = FormWindowState.Normal;
    private Rectangle normalBounds; // 保存正常状态下的窗口位置和大小

    /// <summary>
    /// 获取当前窗口状态
    /// </summary>
    /// <returns></returns>
    public FormWindowState GetWindowState()
    {
        return lastWindowState;
    }

    /// <summary>
    /// 重写窗体消息处理
    /// </summary>
    /// <param name="m"></param>
    protected override void WndProc(ref Message m)
    {
        const int WM_SYSCOMMAND = 0x112;
        const int SC_MAXIMIZE = 0xF030;
        const int SC_RESTORE = 0xF120;
        if (m.Msg == WM_SYSCOMMAND)
        {
            int command = m.WParam.ToInt32() & 0xFFF0;

            if (command == SC_MAXIMIZE)
            {
                // 捕获最大化事件，自定义处理
                MaximizeToCurrentScreen();
                return; // 阻止默认最大化
            }
            else if (command == SC_RESTORE)
            {
                // 捕获还原事件
                RestoreWindow();
                return;
            }
        }

        base.WndProc(ref m);
    }

    /// <summary>
    /// 自定义最大化方法
    /// </summary>
    public void MaximizeToCurrentScreen()
    {
        // 保存正常状态的位置和大小
        normalBounds = this.Bounds;

        // 获取当前屏幕的工作区域（不包含任务栏）
        Screen currentScreen = Screen.FromHandle(this.Handle);
        Rectangle workingArea = currentScreen.WorkingArea;
        // 设置窗口为无边框并最大化到工作区域
        this.WindowState = FormWindowState.Normal; // 先设置为Normal，避免冲突
        this.Bounds = workingArea;

        // 更新状态
        lastWindowState = FormWindowState.Maximized;
    }

    /// <summary>
    /// 还原窗口
    /// </summary>
    public void RestoreWindow()
    {
        if (normalBounds != Rectangle.Empty)
        {
            this.Bounds = normalBounds;
            this.WindowState = FormWindowState.Normal;
            lastWindowState = FormWindowState.Normal;
        }
    }

    /// <summary>
    /// 窗口状态改变事件（可选，用于处理其他状态变化）
    /// </summary>
    /// <param name="e"></param>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        //// 如果通过其他方式改变了窗口状态，同步状态变量
        //if (this.WindowState != lastWindowState)
        //{
        //    lastWindowState = this.WindowState;
        //}
    }


    /// <summary>
    /// Hide the title bar of the form
    /// </summary>
    public void HideTitleBar()
    {
        // 获取当前窗体句柄
        IntPtr hwnd = this.Handle;


        // 更新窗体样式
        SetWindowLong(hwnd, GWL_STYLE, 101646336);

        // 更新窗口位置和大小，确保可以缩放
        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, this.Width, this.Height, 0);
    }

    /// <summary>
    /// 事件转发
    /// </summary>
    public EventForwarder EventForwarder { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public Win32Form()
    {
        EventForwarder = new EventForwarder(this.Handle);
    }
}

