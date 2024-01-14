using System;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace Update;

internal static class Program
{
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
            Application.Run(new MainForm());
            return false;
        }
    }
}
