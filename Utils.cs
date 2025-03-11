using System.Net;
using System.Net.NetworkInformation;

namespace WebApplication;
internal class Utils
{
    public static class DpiHelper
    {
        private static float _DpiScaleFactor = 0;

        public static float DpiScaleFactor=> _DpiScaleFactor;

        public static float GetDpiScaleFactor(Control control)
        {
            if(_DpiScaleFactor == 0)
            {
                using Graphics g = control.CreateGraphics();
                _DpiScaleFactor = g.DpiX / 96.0f; // 96 is the standard DPI
            }
            return _DpiScaleFactor;

        }


    }

    public static bool IsTcpPortInUse(int port)
    {
        IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] ipEndPoints = ipGlobalProperties.GetActiveTcpListeners();
        return ipEndPoints.Any(endPoint => endPoint.Port == port);
    }
}
