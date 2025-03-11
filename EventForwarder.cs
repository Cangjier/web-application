using System.Runtime.InteropServices;

namespace WebApplication;

/// <summary>
/// 事件转发器
/// </summary>
public class EventForwarder
{
    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HT_CAPTION = 0x2;

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
    }
}