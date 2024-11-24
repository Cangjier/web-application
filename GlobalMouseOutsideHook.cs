using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WebApplication
{
    /// <summary>
    /// 全局鼠标钩子
    /// </summary>
    public class GlobalMouseOutsideHook : IDisposable
    {
        private IntPtr _hookId = IntPtr.Zero;
        private readonly LowLevelMouseProc _proc;

        /// <summary>
        /// 当鼠标点击在目标窗口外时触发
        /// </summary>
        public event Action<Point>? MouseClickOutside;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="targetForm"></param>
        public GlobalMouseOutsideHook(Form targetForm)
        {
            _proc = HookCallback;
            _hookId = SetHook(_proc);
            TargetForm = targetForm;
        }

        /// <summary>
        /// 目标窗口
        /// </summary>
        public Form TargetForm { get; }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            if (curModule != null)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
            }
            return IntPtr.Zero;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN))
            {
                MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                Point cursorPos = new Point(hookStruct.pt.x, hookStruct.pt.y);

                // 如果点击不在目标窗口内，触发事件
                if (!TargetForm.Bounds.Contains(cursorPos))
                {
                    MouseClickOutside?.Invoke(cursorPos);
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            UnhookWindowsHookEx(_hookId);
        }

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}
