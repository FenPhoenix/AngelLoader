using System;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace AL_UpdateCopy;

internal static class Program
{
    internal static string DestDir = "";

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        new SingleInstanceManager().Run(args);
    }

    private sealed class SingleInstanceManager : WindowsFormsApplicationBase
    {
        internal SingleInstanceManager() => IsSingleInstance = true;

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
            if (eventArgs.CommandLine.Count == 1)
            {
                DestDir = eventArgs.CommandLine[0];
                if (!string.IsNullOrEmpty(DestDir))
                {
                    Application.Run(new MainForm());
                }
            }
            return false;
        }
    }
}
