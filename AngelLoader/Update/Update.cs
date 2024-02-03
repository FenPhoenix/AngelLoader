// @Update: Un-define this for final
#define TESTING

#define CHECK_UPDATES

#if CHECK_UPDATES
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using AL_Common;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader;

public static class CheckUpdates
{
    private sealed class UpdateFile
    {
        internal Version? Version;
        internal Uri? DownloadUrl;
        internal Uri? ChangelogUrl;
    }

    public sealed class UpdateInfo(Version version, string changelogText, Uri downloadUri)
    {
        public readonly Version Version = version;
        public readonly string ChangelogText = changelogText;
        public readonly Uri DownloadUri = downloadUri;
    }

    // @Update: Test all of this with internet disabled

#if TESTING
    private const string _updatesRepoDir = "updates_testing";
#else
    private const string _updatesRepoDir = "updates";
#endif

#if X64
    private const string _bitnessRepoDir = "framework_x64";
#else
    private const string _bitnessRepoDir = "framework_x86";
#endif

    private static CancellationTokenSource _checkForUpdatesCTS = new();

    internal static void CancelDetailsDownload() => _checkForUpdatesCTS.CancelIfNotDisposed();

    private const string _latestVersionFile = "https://fenphoenix.github.io/AngelLoaderUpdates/" + _updatesRepoDir + "/" + _bitnessRepoDir + "/latest_version.txt";
    private const string _versionsFile = "https://fenphoenix.github.io/AngelLoaderUpdates/" + _updatesRepoDir + "/" + _bitnessRepoDir + "/versions.ini";

    internal static async Task ShowUpdateAskDialog()
    {
        (bool accepted, bool noUpdatesFound, UpdateInfo? updateInfo) = Core.View.ShowUpdateAvailableDialog();
        if (noUpdatesFound)
        {
            Core.View.ShowUpdateNotification(false);
            return;
        }
        if (!accepted || updateInfo == null) return;

        // @Update: To get fancy we could do a double-size progress box and show total/(download|unpack)
        Core.View.ShowProgressBox_Single(
            message1: LText.Update.DownloadingUpdate,
            progressType: ProgressType.Determinate
        );

        Core.View.ShowProgressBox_Double(
            mainMessage1: LText.Update.Updating,
            subMessage: LText.Update.DownloadingUpdate,
            mainProgressType: ProgressType.Determinate,
            subProgressType: ProgressType.Determinate
        );

        await Task.Run(async () =>
        {
            try
            {
                string localZipFile;
                var progress = new Progress<ProgressPercents>(ReportProgress);
                try
                {
                    // @Update: Implement cancellation token
                    using var request = await GlobalHttpClient.GetAsync(updateInfo.DownloadUri, CancellationToken.None);

                    if (!request.IsSuccessStatusCode)
                    {
                        Log("Error downloading the update. Status code: " + request.StatusCode);
                        Paths.CreateOrClearTempPath(Paths.UpdateTemp);
                        Core.Dialogs.ShowError(LText.Update.ErrorDownloadingUpdate, LText.AlertMessages.Alert, MBoxIcon.Warning);
                        return;
                    }

                    // Just download the file once, so we know we won't read duplicate data or whatever

                    Paths.CreateOrClearTempPath(Paths.UpdateAppDownloadTemp);

                    localZipFile = Path.Combine(Paths.UpdateAppDownloadTemp,
                        Path.GetFileName(updateInfo.DownloadUri.OriginalString));

                    using Stream zipStream = await request.Content.ReadAsStreamAsync();
                    using var zipLocalStream = File.Create(localZipFile);
                    // @Update: Implement cancellation token
                    await UpdateZipStreamCopyAsync(zipStream, zipLocalStream, new byte[StreamCopyBufferSize], progress);
                }
                catch (Exception ex)
                {
                    Log("Error downloading the update.", ex);
                    Paths.CreateOrClearTempPath(Paths.UpdateTemp);
                    Core.Dialogs.ShowError(LText.Update.ErrorDownloadingUpdate, LText.AlertMessages.Alert, MBoxIcon.Warning);
                    return;
                }

                try
                {
                    Core.View.SetProgressBoxState_Double(
                        subMessage: LText.Update.UnpackingUpdate,
                        subPercent: 0
                    );

                    using var fs = File.OpenRead(localZipFile);
                    using var archive = new ZipArchive(fs, ZipArchiveMode.Read);
                    Paths.CreateOrClearTempPath(Paths.UpdateTemp);

                    archive.Update_ExtractToDirectory_Fast(Paths.UpdateTemp, progress);
                }
                catch (Exception ex)
                {
                    Log("Error unpacking the update.", ex);
                    Paths.CreateOrClearTempPath(Paths.UpdateTemp);
                    Core.Dialogs.ShowError(LText.Update.ErrorUnpackingUpdate, LText.AlertMessages.Alert, MBoxIcon.Warning);
                    return;
                }

                // Save out the config BEFORE starting the update copier, so it can get the right theme/lang
                Ini.WriteConfigIni();

                if (!File.Exists(Paths.UpdateExe))
                {
                    try
                    {
                        // Last-ditch attempt - extremely unlikely for this bak file to still exist and the
                        // normal exe not (maybe impossible currently?)
                        File.Move(Paths.UpdateExeBak, Paths.UpdateExe);
                    }
                    catch
                    {
                        Log("File not found: '" + Paths.UpdateExe + "'. Couldn't finish the update.");
                        // @Update: Localize this
                        Core.Dialogs.ShowError("Update failed: Couldn't find the updater executable.");
                        Paths.CreateOrClearTempPath(Paths.UpdateTemp);
                        return;
                    }
                }

                try
                {
                    Utils.ProcessStart_UseShellExecute(new ProcessStartInfo(Paths.UpdateExe, "-go"));
                }
                catch (Exception ex)
                {
                    Log("Unable to start '" + Paths.UpdateExe + "'. Couldn't finish the update.", ex);
                    // @Update: Localize this
                    Core.Dialogs.ShowError("Update failed: Unable to start the updater executable.");
                    Paths.CreateOrClearTempPath(Paths.UpdateTemp);
                    return;
                }
            }
            finally
            {
                Paths.CreateOrClearTempPath(Paths.UpdateAppDownloadTemp);
                Core.View.HideProgressBox();
            }

            // Do this AFTER hiding the progress box, otherwise it'll throw up the "operation in progress"
            // message and cancel the app exit.
            // MUST invoke, because otherwise the view's event handlers may/will be called on a thread, and
            // then everything explodes due to cross-thread control access!
            Core.View.Invoke(static () => Core.View.Close());
        });

        return;

        static void ReportProgress(ProgressPercents percents)
        {
            Core.View.SetProgressBoxState(mainPercent: percents.MainPercent, subPercent: percents.SubPercent);
        }

        static async Task UpdateZipStreamCopyAsync(
            Stream source,
            Stream destination,
            byte[] buffer,
            IProgress<ProgressPercents> progress)
        {
            ProgressPercents percents = new();

            int streamLength = (int)source.Length;
            int bytesRead;
            int totalBytesRead = 0;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;

                percents.SubPercent = GetPercentFromValue_Int(totalBytesRead, streamLength);
                percents.MainPercent = percents.SubPercent / 2;
                progress.Report(percents);
            }
        }
    }

    /*
    Web data minimization:
    The update check can (may/probably will) happen every startup for many many different users, and we'll be
    hitting a github pages site, so cut the data transfer down to the absolute bare minimum: just the latest
    version, 5-8 bytes or so. The actual update will be a much less frequent occurrence, so we can afford to
    download more data there.
    */

    // We need to use a thread because we need to set it to "foreground" so it will be killed on app close.
    // Otherwise, we get the main window closing but the task keeping the app open forever, because tasks use
    // thread pool threads which are all background...
    internal static void StartCheckIfUpdateAvailableThread()
    {
        // ReSharper disable once AsyncVoidLambda
        var thread = new Thread(static async () =>
        {
            // Don't need to pass a cancellation token because this is an open-ended "finish whenever" thing
            // and it exits with the app if the app closes.
            if (!await CheckIfUpdateAvailable(CancellationToken.None)) return;
            Core.View.Invoke(static () => Core.View.ShowUpdateNotification(true));
        });

        try
        {
            thread.IsBackground = false;
            thread.Start();
        }
        catch
        {
            // ignore
        }
    }

    private static async Task<bool> CheckIfUpdateAvailable(CancellationToken cancellationToken)
    {
        try
        {
            // @Update: Updating the latest version file is the very last thing that should be done by the release packager
            // We want everything in place when the app finds a new version defined there.

            if (!Version.TryParse(Core.ViewEnv.ProductVersion, out Version appVersion))
            {
                return false;
            }

            using var request = await GlobalHttpClient.GetAsync(_latestVersionFile, cancellationToken);

            if (!request.IsSuccessStatusCode) return false;

            string versionString = await request.Content.ReadAsStringAsync();

            return !versionString.IsEmpty() &&
                   Version.TryParse(versionString, out Version version) &&
                   version > appVersion;
        }
        catch
        {
            return false;
        }
    }

    internal static async Task DoManualCheck()
    {
        bool updateAvailable;
        try
        {
            Core.View.SetWaitCursor(true);
            Core.View.Block(true);
            updateAvailable = await CheckIfUpdateAvailable(CancellationToken.None);
        }
        finally
        {
            Core.View.Block(false);
            Core.View.SetWaitCursor(false);
        }

        if (updateAvailable)
        {
            await ShowUpdateAskDialog();
        }
        else
        {
            Core.Dialogs.ShowAlert(
                LText.Update.NoUpdatesAvailable,
                LText.Update.UpdateAlertBoxTitle,
                MBoxIcon.Information);
        }
    }

    internal enum UpdateDetailsDownloadResult
    {
        Success,
        Error,
        NoUpdatesFound
    }

    internal static async Task<(UpdateDetailsDownloadResult Result, List<UpdateInfo> UpdateInfos)>
    GetUpdateDetails(AutoResetEvent downloadARE) => await Task.Run(async () =>
    {
        try
        {
            _checkForUpdatesCTS = _checkForUpdatesCTS.Recreate();

            List<UpdateInfo> ret = new();

            List<UpdateFile> versions = new();

            if (!Version.TryParse(Core.ViewEnv.ProductVersion, out Version appVersion))
            {
                return (UpdateDetailsDownloadResult.Error, ret);
            }

            UpdateFile? updateFile = null;

            using var request = await GlobalHttpClient.GetAsync(_versionsFile, _checkForUpdatesCTS.Token);

            _checkForUpdatesCTS.Token.ThrowIfCancellationRequested();

            if (!request.IsSuccessStatusCode) return (UpdateDetailsDownloadResult.Error, ret);

            Stream versionFileStream = await request.Content.ReadAsStreamAsync();

            _checkForUpdatesCTS.Token.ThrowIfCancellationRequested();

            using (var sr = new StreamReader(versionFileStream))
            {
                _checkForUpdatesCTS.Token.ThrowIfCancellationRequested();

                while (await sr.ReadLineAsync() is { } line)
                {
                    _checkForUpdatesCTS.Token.ThrowIfCancellationRequested();

                    string lineT = line.Trim();

                    if (lineT.IsEmpty()) continue;

                    if (lineT[0] == '[' && lineT[lineT.Length - 1] == ']' &&
                        Version.TryParse(lineT.Substring(1, lineT.Length - 2), out Version version))
                    {
                        if (version <= appVersion) break;

                        if (updateFile != null) versions.Add(updateFile);
                        updateFile = new UpdateFile { Version = version };
                    }
                    else if (updateFile != null)
                    {
                        if (lineT.StartsWithO("ChangelogUrl="))
                        {
                            updateFile.ChangelogUrl = new Uri(lineT.Substring("ChangelogUrl=".Length));
                        }
                        else if (lineT.StartsWithO("DownloadUrl="))
                        {
                            updateFile.DownloadUrl = new Uri(lineT.Substring("DownloadUrl=".Length));
                        }
                    }
                }

                _checkForUpdatesCTS.Token.ThrowIfCancellationRequested();

                if (updateFile != null) versions.Add(updateFile);
            }

            if (versions.Count == 0) return (UpdateDetailsDownloadResult.NoUpdatesFound, ret);

            foreach (UpdateFile item in versions)
            {
                Uri? changelogUri = item.ChangelogUrl;
                Uri? downloadUri = item.DownloadUrl;

                if (changelogUri != null && downloadUri != null)
                {
                    using var changelogRequest = await GlobalHttpClient.GetAsync(changelogUri, _checkForUpdatesCTS.Token);

                    _checkForUpdatesCTS.Token.ThrowIfCancellationRequested();

                    string changelogText = await changelogRequest.Content.ReadAsStringAsync();

                    _checkForUpdatesCTS.Token.ThrowIfCancellationRequested();

                    // Quick-n-dirty way to normalize linebreaks, because they'll normally be Unix-style on GitHub
                    if (!changelogText.Contains('\r'))
                    {
                        changelogText = changelogText.Replace("\n", "\r\n");
                    }
                    changelogText = changelogText.Trim();

                    if (request.IsSuccessStatusCode)
                    {
                        ret.Add(new UpdateInfo(
                            item.Version!,
                            changelogText,
                            downloadUri));
                    }
                }
            }

            UpdateDetailsDownloadResult result = ret.Count > 0
                ? UpdateDetailsDownloadResult.Success
                : UpdateDetailsDownloadResult.NoUpdatesFound;
            return (result, ret);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return (UpdateDetailsDownloadResult.Error, new List<UpdateInfo>());
        }
        finally
        {
            downloadARE.Set();
        }
    });
}
#endif
