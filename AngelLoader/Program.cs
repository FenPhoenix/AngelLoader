using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Forms;

namespace AngelLoader
{
    internal class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Make this a single-instance application
            var mutex = new Mutex(true, "3053BA21-EB84-4660-8938-1B7329AA62E4.AngelLoader", out bool result);
            if (!result) return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var sevenZipDllLocation = Path.Combine(Paths.Startup, "7z.dll");
            if (!File.Exists(sevenZipDllLocation))
            {
                // NOTE: Not localizable because we don't want to do anything until we've checked this, and getting
                // the right language would mean trying to read multiple different files and whatever junk, and
                // we don't want to add the potential for even more errors here.
                MessageBox.Show("Fatal error: 7z.dll was not found in the application startup directory.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
            // NOTE: ~50ms to do this. If we modified SevenZipSharp, we could get that way down. Lowish priority,
            // but it's a fair chunk of the startup time.
            SevenZip.SevenZipBase.SetLibraryPath(sevenZipDllLocation);

            Application.Run(new AppContext());

            GC.KeepAlive(mutex);
        }
    }

    internal sealed class AppContext : ApplicationContext
    {
        internal AppContext() => Init();

        private static async void Init() => await new MainForm().Init();
    }
}
