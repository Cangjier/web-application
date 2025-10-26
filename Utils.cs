using Svg;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using TidyHPC.Common;
using TidyHPC.Extensions;
using TidyHPC.Loggers;

namespace WebApplication;
internal class Utils
{
    public static class DpiHelper
    {
        public static IntPtr Hwnd;

        private static float _DpiScaleFactor = 0;

        public static float DpiScaleFactor=> _DpiScaleFactor;

        public static float CurrentDpiScaleFactor { get; private set; } = 0;

        public static float Ratio
        {
            get
            {
                if (CurrentDpiScaleFactor == 0)
                {
                    return 1;
                }
                return CurrentDpiScaleFactor / _DpiScaleFactor;
            }
        }

        public static float GetDpiScaleFactor(Control control)
        {
            if(_DpiScaleFactor == 0)
            {
                using Graphics g = control.CreateGraphics();
                _DpiScaleFactor = g.DpiX / 96.0f; // 96 is the standard DPI
            }
            return _DpiScaleFactor;

        }

        public static void SetCurrentDpiScaleFactor()
        {
            //using Graphics g = Graphics.FromHwnd(Hwnd);
            CurrentDpiScaleFactor = GetDpiScaleFactor(Hwnd);
            Logger.Info($"Current DPI Scale Factor: {CurrentDpiScaleFactor}, Ratio: {Ratio}, Awareness:{GetAwareness()}");

        }

        [DllImport("user32.dll")]
        static extern uint GetDpiForWindow(IntPtr hWnd);

        static float GetDpiScaleFactor(IntPtr hwnd)
        {
            uint dpi = GetDpiForWindow(hwnd);
            return dpi / 96.0f;
        }

        [DllImport("shcore.dll")]
        private static extern int GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS value);

        private enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        public static string GetAwareness()
        {
             
            GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out var awareness);
            return awareness switch { 
                PROCESS_DPI_AWARENESS.Process_DPI_Unaware => "Process_DPI_Unaware",
                PROCESS_DPI_AWARENESS.Process_System_DPI_Aware => "Process_System_DPI_Aware",
                PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware => "Process_Per_Monitor_DPI_Aware",
                _ => "Unknown"
            };
        }
    }

    public static bool IsTcpPortInUse(int port)
    {
        IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] ipEndPoints = ipGlobalProperties.GetActiveTcpListeners();
        return ipEndPoints.Any(endPoint => endPoint.Port == port);
    }

    public static int GetAvailablePort()
    {
        TcpListener listener = new (IPAddress.Loopback, 0);
        try
        {
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    public static Stopwatch StartupWatch { get; } = new();

    public static LocalFavicon LocalFavicon { get; } = new();


}
