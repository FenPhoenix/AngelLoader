using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            // @Update: What to do if a user starts this exe manually
            // We should either quit out if we're not passed a go command, or make this app be the one that also
            // downloads the update.
            MainView = new MainForm();
            Application.Run(MainView);
            return false;
        }
    }

    private static MainForm MainView = null!;

    private static readonly string _baseTempPath = Path.Combine(Path.GetTempPath(), "AngelLoader");
    internal static readonly string UpdateTempPath = Path.Combine(_baseTempPath, "Update");

    internal static async Task DoCopy()
    {
        string startupPath = Application.StartupPath;
        string exePath = Application.ExecutablePath;

        await Task.Run(() =>
        {
            try
            {
                File.Delete(exePath + ".bak");
            }
            catch
            {
                // didn't exist or whatever
            }

            File.Move(exePath, exePath + ".bak");

            List<string> files = Directory.GetFiles(UpdateTempPath, "*", SearchOption.AllDirectories).ToList();

            for (int i = 0; i < files.Count; i++)
            {
                string fileName = Path.GetFileName(files[i]);

                if (fileName.EqualsI("FMData.ini") ||
                    fileName.StartsWithI("FMData.bak") ||
                    fileName.EqualsI("Config.ini"))
                {
                    files.RemoveAt(i);
                    i--;
                }
            }

            string updateDirWithTrailingDirSep = UpdateTempPath.TrimEnd('\\', '/') + "\\";

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                string fileName = file.Substring(updateDirWithTrailingDirSep.Length);

                MainView.SetMessage("Copying..." + Environment.NewLine + fileName);

                // @Update: Handle errors robustly
                string finalFileName = Path.Combine(startupPath, fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(finalFileName)!);
                File.Copy(file, finalFileName, overwrite: true);

                //for (int t = 0; t < 100; t++)
                //{
                //    Thread.Sleep(1);
                //    Application.DoEvents();
                //}

                int percent = Utils.GetPercentFromValue_Int(i + 1, files.Count);
                MainView.SetProgress(percent);
            }

            Utils.ClearUpdateTempPath();
        });

        // @Update: Handle errors robustly
        using (Process.Start(Path.Combine(startupPath, "AngelLoader.exe"))) { }
        Application.Exit();
    }
}
