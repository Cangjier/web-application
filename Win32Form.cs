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
    const uint WS_CAPTION = 0x00C00000; // 标题栏样式
    const uint WS_SYSMENU = 0x00080000; // 系统菜单样式

    /// <summary>
    /// Hide the title bar of the form
    /// </summary>
    public void HideTitleBar()
    {
        // 获取当前窗体句柄
        IntPtr hwnd = this.Handle;

        //// 获取当前窗体的样式
        //uint style = (uint)GetWindowLong(hwnd, GWL_STYLE);

        //// 移除标题栏样式（WS_CAPTION），保留系统菜单和边框样式
        //style &= ~WS_CAPTION;

        //Logger.Info($"style={style}");


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

