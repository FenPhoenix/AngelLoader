using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace Update;

internal static class Program
{
    private static readonly string _baseTempPath = Path.Combine(Path.GetTempPath(), "AngelLoader");
    internal static readonly string UpdateTempPath = Path.Combine(_baseTempPath, "Update");

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
