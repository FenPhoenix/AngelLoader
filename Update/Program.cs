using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace Update;

/*
IMPORTANT: This app MUST NOT have any dependencies! It's going to copy an entire AL installation into its own directory.
The rename of its own exe should be all that is required to allow the entire copy to succeed (no files locked).
*/

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
            // @Update: Maybe we should name this something unappealing like "_update_internal.exe"
            if (eventArgs.CommandLine.Count == 1 &&
                eventArgs.CommandLine[0] == "-go")
            {
                View = new MainForm();
                Application.Run(View);
            }
            else
            {
                MessageBox.Show(
                    "This executable is not meant to be run on its own. Please update from within AngelLoader.",
                    "AngelLoader Updater");
            }
            return false;
        }
    }

    private static MainForm View = null!;

    private static readonly string _baseTempPath = Path.Combine(Path.GetTempPath(), "AngelLoader");
    internal static readonly string UpdateTempPath = Path.Combine(_baseTempPath, "Update");
    internal static readonly string UpdateBakTempPath = Path.Combine(_baseTempPath, "UpdateBak");

    // @Update: Maybe we should rename all to-be-replaced files first, then delete after, just to avoid "in use" errors
    internal static async Task DoCopy()
    {
#if false
        string startupPath = @"C:\AngelLoader";
        string exePath = @"C:\AngelLoader\Update.exe";
#else
        string startupPath = Application.StartupPath;
        string exePath = Application.ExecutablePath;
#endif

        await Utils.WaitForAngelLoaderToClose();

        await Task.Run(async () =>
        {
            List<string> files;
            try
            {
                files = Directory.GetFiles(UpdateTempPath, "*", SearchOption.AllDirectories).ToList();
                if (files.Count == 0) return;
            }
            catch (DirectoryNotFoundException)
            {
                return;
            }
            catch (Exception)
            {
                // @Update: Handle other exception cases here
                return;
            }

            try
            {
                File.Delete(exePath + ".bak");
            }
            catch
            {
                // didn't exist or whatever
            }

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

            List<string> oldRelativeFileNames = new();

            Directory.CreateDirectory(UpdateBakTempPath);
            Utils.ClearUpdateBakTempPath();
            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                string relativeFileName = file.Substring(updateDirWithTrailingDirSep.Length);

                string appFileName = Path.Combine(startupPath, relativeFileName);

                if (File.Exists(appFileName))
                {
                    string finalBakFileName = Path.Combine(UpdateBakTempPath, relativeFileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(finalBakFileName)!);
                    File.Copy(appFileName, Path.Combine(UpdateBakTempPath, relativeFileName), overwrite: true);

                    oldRelativeFileNames.Add(relativeFileName);
                }
            }

            try
            {
                File.Move(exePath, exePath + ".bak");
            }
            catch (Exception ex)
            {
                MessageBox.Show(View,
                    "Update failed: Unable to rename '" + exePath + "' to '" + exePath + ".bak'.\r\n\r\n" +
                    "Exception:\r\n\r\n" +
                    ex);
                return;
            }

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                string fileName = file.Substring(updateDirWithTrailingDirSep.Length);

                View.SetMessage("Copying..." + Environment.NewLine + fileName);

                int retryCount = 0;
                retry:
                string finalFileName = "";
                try
                {
                    finalFileName = Path.Combine(startupPath, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(finalFileName)!);
                    //if (i == files.Count - 1)
                    //{
                    //    throw new Exception("TEST");
                    //}
                    File.Copy(file, finalFileName, overwrite: true);
                }
                catch (Exception ex)
                {
                    if (retryCount > 10)
                    {
                        DialogResult result = MessageBox.Show(View,
                            "Couldn't copy '" + file + "' to '" + finalFileName + "'.\r\n\r\n" +
                            "If AngelLoader is running, close it and try again.\r\n\r\nException: " + ex,
                            "Error",
                            MessageBoxButtons.RetryCancel,
                            MessageBoxIcon.Warning);
                        if (result == DialogResult.Retry)
                        {
                            retryCount = 0;
                            goto retry;
                        }
                        else
                        {
                            Rollback(startupPath, oldRelativeFileNames);
                            return;
                        }
                    }
                    else
                    {
                        retryCount++;
                        await Task.Delay(1000);
                        goto retry;
                    }
                }

                int percent = Utils.GetPercentFromValue_Int(i + 1, files.Count);
                View.SetProgress(percent);
            }

            Utils.ClearUpdateTempPath();
            Utils.ClearUpdateBakTempPath();
        });

        try
        {
            using (Process.Start(Path.Combine(startupPath, "AngelLoader.exe"), "-after_update_cleanup")) { }
        }
        catch (Exception ex)
        {
            MessageBox.Show(View,
                "Unable to start AngelLoader. You'll need to start it manually.\r\n\r\n" +
                "Exception:\r\n\r\n" +
                ex);
            return;
        }
    }

    private static void Rollback(string startupPath, List<string> oldRelativeFileNames)
    {
        View.SetMessage("Could not complete copy; rolling back to old version...");
        try
        {
            for (int i = 0; i < oldRelativeFileNames.Count; i++)
            {
                string relativeFileName = oldRelativeFileNames[i];
                File.Copy(Path.Combine(UpdateBakTempPath, relativeFileName),
                    Path.Combine(startupPath, relativeFileName), overwrite: true);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(View,
                "The update failed and we tried to restore the old version, but that failed too. " +
                "It's recommended to download the latest version of AngelLoader and re-install it manually.\r\n\r\n" +
                "Exception:\r\n\r\n" +
                ex);
        }
    }
}
