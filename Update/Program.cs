using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using static Update.Data;
using static Update.Logger;

namespace Update;

/*
IMPORTANT: This app MUST NOT have any dependencies! It's going to copy an entire AL installation into its own directory.
The rename of its own exe should be all that is required to allow the entire copy to succeed (no files locked).
*/

internal static class Program
{
    internal static bool _testMode;

    private static MainForm View = null!;

    private static CancellationTokenSource _copyCTS = new();

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
            SetLogFile(Paths.LogFile);

            // It's safe to read the AL config file before waiting for AL to close, because it will have written
            // out the config explicitly BEFORE calling us. This also lets us pop up our window immediately to
            // make it feel like things are moving along quickly, without a big delay with no window.
            // Also AL doesn't write to its language files so it's always safe to read those.

            ReadLanguageIni();

            // @Update: Maybe we should name this something unappealing like "_update_internal.exe"
            if (eventArgs.CommandLine.Count == 1 &&
                eventArgs.CommandLine[0] == "-go")
            {
                Data.VisualTheme = Utils.ReadThemeFromConfigIni(Paths.ConfigIni);

                View = new MainForm();
                Application.Run(View);
            }
            // @Update: Dummy-out test stuff for final
            else if (eventArgs.CommandLine.Count == 1 &&
                      eventArgs.CommandLine[0] == "-test")
            {
                _testMode = true;

                Data.VisualTheme = Utils.ReadThemeFromConfigIni(Paths.ConfigIni);

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
            string[] lines = File.ReadAllLines(Paths.ConfigIni);
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

    internal static void CancelCopy()
    {
        try { _copyCTS.Cancel(); } catch (ObjectDisposedException) { }
    }

    internal static async Task DoCopy(AutoResetEvent autoResetEvent)
    {
        try
        {
            await DoCopyInternal();
        }
        catch (OperationCanceledException)
        {
            Utils.ClearUpdateTempPath();
            Utils.ClearUpdateBakTempPath();
            throw;
        }
        finally
        {
            autoResetEvent.Set();
        }
    }

    // Repulsive hack cause I'm lazy
    private static bool _startAngelLoader = true;

    private static async Task DoCopyInternal()
    {
        _copyCTS.Dispose();
        _copyCTS = new CancellationTokenSource();

        if (_testMode)
        {
            View.SetMessage1(LText.Update.CopyingFiles);
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

        List<string> oldRelativeFileNames = new();

        View.SetMessage1(LText.Update.PreparingToUpdate);

        await Utils.WaitForAngelLoaderToClose(_copyCTS.Token);

        await Task.Run(async () =>
        {
            List<string> files;
            try
            {
                files = Directory.GetFiles(Paths.UpdateTemp, "*", SearchOption.AllDirectories).ToList();

                CleanupAndThrowIfCancellationRequested();

                if (files.Count == 0)
                {
                    Log("Update failed: No files in '" + Paths.UpdateTemp + "'.");
                    Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
                    return;
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                Log("Update failed: Update temp directory not found: '" + Paths.UpdateTemp + "'.", ex);
                Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log("Update failed: Error while trying to get the list of new app files in '" + Paths.UpdateTemp + "'.", ex);
                Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
                return;
            }

            try
            {
                File.Delete(exePath + ".bak");
            }
            catch
            {
                // ignore
            }

            CleanupAndThrowIfCancellationRequested();

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

            string updateDirWithTrailingDirSep = Paths.UpdateTemp.TrimEnd('\\', '/') + "\\";

            try
            {
                Directory.CreateDirectory(Paths.UpdateBakTemp);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log("Update failed: Unable to create '" + Paths.UpdateBakTemp + "' (current version backup path).", ex);
                Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
                return;
            }

            CleanupAndThrowIfCancellationRequested();

            Utils.ClearUpdateBakTempPath();

            CleanupAndThrowIfCancellationRequested();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                string relativeFileName = file.Substring(updateDirWithTrailingDirSep.Length);

                string appFileName = Path.Combine(startupPath, relativeFileName);

                if (File.Exists(appFileName))
                {
                    string finalBakFileName = Path.Combine(Paths.UpdateBakTemp, relativeFileName);

                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(finalBakFileName)!);
                        File.Copy(appFileName, Path.Combine(Paths.UpdateBakTemp, relativeFileName), overwrite: true);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        Log("Update failed: Unable to complete the backup of current app files.", ex);
                        Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
                        Utils.ClearUpdateBakTempPath();
                        return;
                    }

                    oldRelativeFileNames.Add(relativeFileName);
                }

                CleanupAndThrowIfCancellationRequested();
            }

            // If anything goes wrong, the rollback will restore our original exe, so we don't have to rename it
            // back as long as the bak rename comes right before the loop
            try
            {
                File.Move(exePath, exePath + ".bak");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log("Update failed: Unable to rename '" + exePath + "' to '" + exePath + ".bak'.", ex);
                Utils.ShowAlert(View, GenericUpdateFailedSafeMessage);
                return;
            }

            CleanupAndThrowIfCancellationRequested(doRollback: true);

            View.SetMessage1(LText.Update.CopyingFiles);

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                string fileName = file.Substring(updateDirWithTrailingDirSep.Length);

                View.SetMessage2(fileName);

                int retryCount = 0;
                retry:
                string finalFileName = "";
                try
                {
                    finalFileName = Path.Combine(startupPath, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(finalFileName)!);

                    CleanupAndThrowIfCancellationRequested(doRollback: true);

                    //if (i == files.Count - 1)
                    //{
                    //    throw new Exception("TEST");
                    //}
                    File.Copy(file, finalFileName, overwrite: true);

                    CleanupAndThrowIfCancellationRequested(doRollback: true);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (retryCount > 10)
                    {
                        Log("Couldn't copy '" + file + "' to '" + finalFileName + "'.", ex);
                        DialogResult result = Utils.ShowDialogCustom(View,
                            message: LText.Update.FileCopy_CouldNotCopyFile + "\r\n\r\n" +
                                     LText.Update.FileCopy_Source + " " + file + "\r\n" +
                                     LText.Update.FileCopy_Destination + " " + finalFileName + "\r\n\r\n" +
                                     LText.Update.FileCopy_CloseAngelLoader,
                            title: LText.AlertMessages.Error,
                            icon: MessageBoxIcon.Warning,
                            yesText: LText.Global.Retry,
                            noText: LText.Global.Cancel,
                            defaultButton: DialogResult.Yes);

                        if (result == DialogResult.Retry)
                        {
                            retryCount = 0;
                            goto retry;
                        }
                        else
                        {
                            Rollback(startupPath, oldRelativeFileNames);

                            CleanupAndThrowIfCancellationRequested();

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

            CleanupAndThrowIfCancellationRequested(doRollback: true);

            Utils.ClearUpdateTempPath();

            CleanupAndThrowIfCancellationRequested(doRollback: true);

            Utils.ClearUpdateBakTempPath();
        });

        if (!_startAngelLoader) return;

        try
        {
            using (Process.Start(Path.Combine(startupPath, "AngelLoader.exe"), "-after_update_cleanup")) { }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log("Unable to start AngelLoader after update copy.", ex);
            Utils.ShowAlert(View, LText.Update.UnableToStartAngelLoader);
            // ReSharper disable once RedundantJumpStatement
            return;
        }

        return;

        void CleanupAndThrowIfCancellationRequested(bool doRollback = false)
        {
            if (_copyCTS.Token.IsCancellationRequested)
            {
                if (doRollback)
                {
                    Rollback(startupPath, oldRelativeFileNames, canceled: true);
                }

                Utils.ClearUpdateTempPath();
                Utils.ClearUpdateBakTempPath();

                _copyCTS.Token.ThrowIfCancellationRequested();
            }
        }
    }

    private static void Rollback(string startupPath, List<string> oldRelativeFileNames, bool canceled = false)
    {
        View.SetMessage1(LText.Update.RestoringOldFiles);
        try
        {
            for (int i = 0; i < oldRelativeFileNames.Count; i++)
            {
                string relativeFileName = oldRelativeFileNames[i];
                File.Copy(Path.Combine(Paths.UpdateBakTemp, relativeFileName),
                    Path.Combine(startupPath, relativeFileName), overwrite: true);
            }
            string reason = canceled ? "Update canceled." : "Update failed.";
            Log(reason + " Successfully rolled back (restored backed-up app files).");
            string message = canceled
                ? LText.Update.UpdateCanceled
                : GenericUpdateFailedSafeMessage;
            Utils.ShowAlert(View, message);
        }
        catch (Exception ex)
        {
            _startAngelLoader = false;
            string message = canceled ? "Update canceled but the rollback failed." : "Update failed and the rollback failed as well.";
            Log(message, ex);
            Utils.ShowError(View,
                (canceled ? LText.Update.CanceledAndRollbackFailed : LText.Update.RollbackFailed) + Environment.NewLine +
                LText.Update.RecommendManualUpdate);
        }
    }

    internal static void OpenLogFile()
    {
        try
        {
            using (Process.Start(Paths.LogFile)) { }
        }
        catch
        {
            Utils.ShowAlert(View, "Unable to open log file." + "\r\n\r\n" + Paths.LogFile, LText.AlertMessages.Error);
        }
    }
}
