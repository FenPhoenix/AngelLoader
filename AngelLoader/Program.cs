using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using static AngelLoader.Logger;

namespace AngelLoader
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
#if DEBUG || Release_Testing
            Forms.RTF_Visual_Test_Form.LoadIfCommandLineArgsArePresent();
#endif

            // Need to set these here, because the single-instance thing internally creates a window and message-
            // loop etc... that's also why we straight-up ditched our clever "init the ConfigurationManager in
            // the background" thing, because we're going to be creating a form as the very first thing we do now
            // anyway (even if we're the second instance), so such tricks won't help us. Oh well.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new SingleInstanceManager().Run(args);
        }

        private sealed class SingleInstanceManager : WindowsFormsApplicationBase
        {
            internal SingleInstanceManager() => IsSingleInstance = true;

            protected override bool OnStartup(StartupEventArgs eventArgs)
            {
                // We need to clear this because FMScanner doesn't have a startup version
                ClearLogFileStartup(Paths.ScannerLogFile);

                // We don't need to clear this log because LogStartup says append: false
                LogStartup(Application.ProductVersion + " Started session");

                // Do this after the startup log so we don't try to log something at the same time as the non-lock-
                // protected startup log
                AppDomain.CurrentDomain.FirstChanceException += (_, e) =>
                {
                    if (e.Exception.TargetSite.DeclaringType?.Assembly == Assembly.GetExecutingAssembly())
                    {
                        Log("Exception thrown", e.Exception);
                    }
                };

                #region SevenZipSharp init

                // Catching this early, because otherwise it just gets loaded whenever and could throw (or just fail)
                // at any time
                string sevenZipDllLocation = Path.Combine(Paths.Startup, "7z.dll");
                if (!File.Exists(sevenZipDllLocation))
                {
                    // NOTE: Not localizable because we don't want to do anything until we've checked this, and getting
                    // the right language would mean trying to read multiple different files and whatever junk, and
                    // we don't want to add the potential for even more errors here.
                    MessageBox.Show(
                        "Fatal error: 7z.dll was not found in the application startup directory.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }

                // NOTE: Calling this takes ~50ms, but fortunately if we don't call it then it just looks in the app
                // startup path. So we just make sure we copy 7z.dll to anywhere that could be an app startup path
                // (so that includes our bin\x86\whatever dirs).
                //SevenZip.SevenZipBase.SetLibraryPath(sevenZipDllLocation);

                #endregion

                Application.Run(new AppContext(eventArgs.CommandLine));

                return false;
            }

            protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
            {
                // The official Microsoft example puts the base call line first, so I guess I will too?
                // https://github.com/microsoft/wpf-samples/tree/main/Application%20Management/SingleInstanceDetection
                base.OnStartupNextInstance(eventArgs);
                Core.ActivateMainView();
#if false
                await Core.HandleCommandLineArgs(eventArgs.CommandLine);
#endif
            }
        }

        private sealed class AppContext : ApplicationContext
        {
            internal AppContext(ReadOnlyCollection<string> args) => Core.Init(args);
        }
    }
}
