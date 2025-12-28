using System.Runtime.InteropServices;

namespace WebApplication;

/// <summary>
/// 事件转发器
/// </summary>
public class EventForwarder
{
    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HT_CAPTION = 0x2;
    private const int WM_SYSCOMMAND = 0x0112;
    private const int SC_MOVE = 0xF010;

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    readonly IntPtr target;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="target"></param>
    public EventForwarder(IntPtr target)
    {
        this.target = target;
    }

    /// <summary>
    /// 鼠标按下
    /// </summary>
    public void MouseDownDrag()
    {
        ReleaseCapture();
        SendMessage(target, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        //SendMessage(target, WM_SYSCOMMAND, SC_MOVE | HT_CAPTION, 0);
    }
}