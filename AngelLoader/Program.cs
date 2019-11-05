using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Make this a single-instance application
            var mutex = new Mutex(true, AppGuid, out bool firstInstance);
            if (!firstInstance)
            {
                // Tell first instance to show itself
                InteropMisc.SendMessage((IntPtr)InteropMisc.HWND_BROADCAST, InteropMisc.WM_SHOWFIRSTINSTANCE, IntPtr.Zero, IntPtr.Zero);
                // If it fails, oh well, then it's just the old behavior where the window doesn't activate but
                // it's still a single instance. Good enough.
                return;
            }

            // Form.ctor initializes the config manager if it isn't already (specifically so it can get the DPI
            // awareness value). The config manager, like a bloated-ass five-hundred-foot-tall blubber-laden pig,
            // takes 32ms to initialize(!). Rather than letting Form.ctor do it serially, we're going to do it in
            // the background while other stuff runs, thus chopping off even more startup time.
            // AppSettings is just a dummy value whose retrieval will cause the config manager to initialize.
            Task configTask = Task.Run(() => System.Configuration.ConfigurationManager.AppSettings);

            // We need to clear this because FMScanner doesn't have a startup version
            ClearLogFileStartup(Paths.ScannerLogFile);

            // We don't need to clear this log because LogStartup says append: false
            LogStartup(Application.ProductVersion + " Started session");

            // Do this after the startup log so we don't try to log something at the same time as the non-lock-
            // protected startup log
            AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
            {
                if (e.Exception.TargetSite.DeclaringType?.Assembly == Assembly.GetExecutingAssembly())
                {
                    Log("Exception thrown", e.Exception);
                }
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            #region SevenZipSharp init

            // Catching this early, because otherwise it just gets loaded whenever and could throw (or just fail)
            // at any time
            var sevenZipDllLocation = Path.Combine(Paths.Startup, "7z.dll");
            if (!File.Exists(sevenZipDllLocation))
            {
                // NOTE: Not localizable because we don't want to do anything until we've checked this, and getting
                // the right language would mean trying to read multiple different files and whatever junk, and
                // we don't want to add the potential for even more errors here.
                MessageBox.Show(@"Fatal error: 7z.dll was not found in the application startup directory.", @"Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }

            // NOTE: Calling this takes ~50ms, but fortunately if we don't call it then it just looks in the app
            // startup path. So we just make sure we copy 7z.dll to anywhere that could be an app startup path
            // (so that includes our bin\x86\whatever dirs).
            //SevenZip.SevenZipBase.SetLibraryPath(sevenZipDllLocation);

            #endregion

            Application.Run(new AppContext(configTask));

            GC.KeepAlive(mutex);
        }
    }

    internal sealed class AppContext : ApplicationContext
    {
        internal AppContext(Task configTask) => Core.Init(configTask);
    }
}
