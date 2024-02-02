using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using static Update.Data;
using static Update.Logger;

namespace Update;

/*
IMPORTANT: This app MUST NOT have any dependencies! It's going to copy an entire AL installation into its own directory.
The rename of its own exe should be all that is required to allow the entire copy to succeed (no files locked).

@Update: When update fails safe, rename Update.exe.bak back to Update.exe!
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

    internal static bool _testMode;

    private sealed class SingleInstanceManager : WindowsFormsApplicationBase
    {
        internal SingleInstanceManager() => IsSingleInstance = true;

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
            SetLogFile(LogFile);

            ReadLanguageIni();

            // @Update: Maybe we should name this something unappealing like "_update_internal.exe"
            if (eventArgs.CommandLine.Count == 1 &&
                eventArgs.CommandLine[0] == "-go")
            {
                Data.VisualTheme = Utils.ReadThemeFromConfigIni(ConfigIniPath);

                View = new MainForm();
                Application.Run(View);
            }
            // @Update: Dummy-out test stuff for final
            else if (eventArgs.CommandLine.Count == 1 &&
                      eventArgs.CommandLine[0] == "-test")
            {
                _testMode = true;

                Data.VisualTheme = Utils.ReadThemeFromConfigIni(ConfigIniPath);

                View = new MainForm();
                Application.Run(View);
            }
            else
            {
                // Stock message box intentionally here
                MessageBox.Show(
                    "This executable is not meant to be run on its own. Please update from within AngelLoader.",
                    "AngelLoader Updater");
            }
            return false;
        }
    }

    private static void ReadLanguageIni()
    {
        try
        {
            string langName = "";
            string[] lines = File.ReadAllLines(ConfigIniPath);
            foreach (string line in lines)
            {
                string lineT = line.Trim();
                // Don't break; AL behavior is to take the last one found
                if (lineT.StartsWithO("Language="))
                {
                    langName = lineT.Substring("Language=".Length).Trim();
                }
            }
            if (string.IsNullOrEmpty(langName)) return;
            string langFile = Path.Combine(Application.StartupPath, "Data", "Languages", langName + ".ini");
            Ini.ReadLocalizationIni(langFile, LText);
        }
        catch
        {
            // ignore
        }
    }

    private static MainForm View = null!;

    private static readonly string ConfigIniPath = Path.Combine(Application.StartupPath, "Data", "Config.ini");

    private static readonly string _baseTempPath = Path.Combine(Path.GetTempPath(), "AngelLoader");
    internal static readonly string UpdateTempPath = Path.Combine(_baseTempPath, "Update");
    internal static readonly string UpdateBakTempPath = Path.Combine(_baseTempPath, "UpdateBak");

    // @Update: Maybe we should rename all to-be-replaced files first, then delete after, just to avoid "in use" errors
    // @Update: Do we want to read the language inis for this app?
    internal static async Task DoCopy()
    {
        if (_testMode)
        {
            View.SetMessage(LText.Update.Copying);
            View.SetProgress(50);
            return;
        }

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
                if (files.Count == 0)
                {
                    Log("Update failed: No files in '" + UpdateTempPath + "'.");
                    Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
                    return;
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                Log("Update failed: Update temp directory not found: '" + UpdateTempPath + "'.", ex);
                Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
                return;
            }
            catch (Exception ex)
            {
                Log("Update failed: Error while trying to get the list of new app files in '" + UpdateTempPath + "'.", ex);
                Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
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
                    fileName.EqualsI("Config.ini") ||
                    fileName.EqualsI("AngelLoader_log.txt"))
                {
                    files.RemoveAt(i);
                    i--;
                }
            }

            string updateDirWithTrailingDirSep = UpdateTempPath.TrimEnd('\\', '/') + "\\";

            List<string> oldRelativeFileNames = new();

            try
            {
                Directory.CreateDirectory(UpdateBakTempPath);
            }
            catch (Exception ex)
            {
                Log("Update failed: Unable to create '" + UpdateBakTempPath + "' (current version backup path).", ex);
                Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
                return;
            }
            Utils.ClearUpdateBakTempPath();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                string relativeFileName = file.Substring(updateDirWithTrailingDirSep.Length);

                string appFileName = Path.Combine(startupPath, relativeFileName);

                if (File.Exists(appFileName))
                {
                    string finalBakFileName = Path.Combine(UpdateBakTempPath, relativeFileName);

                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(finalBakFileName)!);
                        File.Copy(appFileName, Path.Combine(UpdateBakTempPath, relativeFileName), overwrite: true);
                    }
                    catch (Exception ex)
                    {
                        Log("Update failed: Unable to complete the backup of current app files.", ex);
                        Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
                        Utils.ClearUpdateBakTempPath();
                        return;
                    }

                    oldRelativeFileNames.Add(relativeFileName);
                }
            }

            try
            {
                File.Move(exePath, exePath + ".bak");
            }
            catch (Exception ex)
            {
                Log("Update failed: Unable to rename '" + exePath + "' to '" + exePath + ".bak'.", ex);
                Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
                return;
            }

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                string fileName = file.Substring(updateDirWithTrailingDirSep.Length);

                View.SetMessage(LText.Update.Copying + Environment.NewLine + fileName);

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
                        // @Update: Localize this
                        using var d = new DarkTaskDialog(
                            message: "Couldn't copy '" + file + "' to '" + finalFileName + "'.\r\n\r\n" +
                                     "If AngelLoader is running, close it and try again.\r\n\r\nException: " +
                                     ex,
                            title: LText.AlertMessages.Error,
                            icon: MessageBoxIcon.Warning,
                            // @Update: Localize these
                            yesText: LText.Global.Retry,
                            noText: LText.Global.Cancel,
                            defaultButton: DialogResult.Yes);

                        DialogResult result = d.ShowDialog(View);

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
            Log("Unable to start AngelLoader after update copy.", ex);
            Utils.ShowAlert(View, LText.Update.UnableToStartAngelLoader);
            // ReSharper disable once RedundantJumpStatement
            return;
        }
    }

    private static void Rollback(string startupPath, List<string> oldRelativeFileNames)
    {
        try
        {
            for (int i = 0; i < oldRelativeFileNames.Count; i++)
            {
                string relativeFileName = oldRelativeFileNames[i];
                File.Copy(Path.Combine(UpdateBakTempPath, relativeFileName),
                    Path.Combine(startupPath, relativeFileName), overwrite: true);
            }
            // @Update: Test this
            Log("Update failed. Successfully rolled back (restored backed-up app files).");
            Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
        }
        catch (Exception ex)
        {
            // @Update: Test error and logging functionality
            Log("Update failed and the rollback failed as well.", ex);
            Utils.ShowError(View,
                LText.Update.RollbackFailed + Environment.NewLine +
                LText.Update.RecommendManualUpdate);
        }
    }

    private static readonly string LogFile = Path.Combine(Application.StartupPath, "AngelLoader_log.txt");

    internal static void OpenLogFile()
    {
        try
        {
            using (Process.Start(LogFile)) { }
        }
        catch
        {
            Utils.ShowAlert(View, "Unable to open log file." + "\r\n\r\n" + LogFile, LText.AlertMessages.Error);
        }
    }
}
