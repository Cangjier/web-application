using TidyHPC.Loggers;

namespace WebApplication;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Logger.Level = LoggerFile.Levels.Develop;
        Utils.StartupWatch.Start();
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm()
        {
            //Visible = false,
            //Opacity = 0,
            //ShowInTaskbar=false,
            //FormBorderStyle = FormBorderStyle.None
        });
    }
}