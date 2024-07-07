//#define TESTING

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AL_Common;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader;

public static class AppUpdate
{
    private sealed class UpdateInfoInternal
    {
        internal readonly Version Version;
        internal Uri? DownloadUrl;
        internal Uri? ChangelogUrl;

        internal UpdateInfoInternal(Version version) => Version = version;
    }

    public sealed class UpdateInfo(Version version, string changelogText, Uri downloadUri)
    {
        public readonly Version Version = version;
        public readonly string ChangelogText = changelogText;
        public readonly Uri DownloadUri = downloadUri;
    }

    private enum CheckUpdateResult
    {
        UpdateAvailable,
        NoUpdateAvailable,
        Error,
    }

    internal enum UpdateDetailsDownloadResult
    {
        Success,
        Error,
        NoUpdatesFound,
    }

#if TESTING
    private const string _updatesRepoDir = "updates_testing";
#else
    private const string _updatesRepoDir = "updates";
#endif

    private const string _bitnessRepoDir = "netmodern_x64";

    private static CancellationTokenSource _detailsDownloadCTS = new();
    private static CancellationTokenSource _updatingCTS = new();

    private const string _latestVersionFile = "https://fenphoenix.github.io/AngelLoaderUpdates/" + _updatesRepoDir + "/" + _bitnessRepoDir + "/latest_version.txt";
    private const string _versionsFile = "https://fenphoenix.github.io/AngelLoaderUpdates/" + _updatesRepoDir + "/" + _bitnessRepoDir + "/versions.ini";

    internal static void CancelDetailsDownload() => _detailsDownloadCTS.CancelIfNotDisposed();

    private static void CancelUpdate() => _updatingCTS.CancelIfNotDisposed();

    internal static async Task ShowUpdateAskDialog()
    {
        (bool accepted, bool noUpdatesFound, UpdateInfo? updateInfo) = Core.View.ShowUpdateAvailableDialog();
        if (noUpdatesFound)
        {
            Core.View.ShowUpdateNotification(false);
            return;
        }
        if (!accepted || updateInfo == null) return;

        _updatingCTS = _updatingCTS.Recreate();

        Core.View.ShowProgressBox_Single(
            message1: LText.Update.Updating,
            message2: LText.Update.DownloadingUpdate,
            progressType: ProgressType.Determinate,
            cancelMessage: LText.Global.Cancel,
            cancelAction: CancelUpdate
        );

        await Task.Run(async () =>
        {
            try
            {
                string localZipFile;
                var progress = new Progress<ProgressPercents>(ReportProgress);
                try
                {
                    using var request = await GlobalHttpClient.GetAsync(updateInfo.DownloadUri, _updatingCTS.Token);

                    if (!request.IsSuccessStatusCode)
                    {
                        Log("Error downloading the update. Status code: " + request.StatusCode);
                        Paths.CreateOrClearTempPath(TempPaths.Update);
                        Core.Dialogs.ShowError(LText.Update.ErrorDownloadingUpdate, LText.AlertMessages.Alert, MBoxIcon.Warning);
                        return;
                    }

                    _updatingCTS.Token.ThrowIfCancellationRequested();

                    // Just download the file once, so we know we won't read duplicate data or whatever

                    Paths.CreateOrClearTempPath(TempPaths.UpdateAppDownload);

                    _updatingCTS.Token.ThrowIfCancellationRequested();

                    localZipFile = Path.Combine(Paths.UpdateAppDownloadTemp,
                        Path.GetFileName(updateInfo.DownloadUri.OriginalString));

                    await using Stream zipStream = await request.Content.ReadAsStreamAsync();

                    _updatingCTS.Token.ThrowIfCancellationRequested();

                    await using var zipLocalStream = File.Create(localZipFile);

                    _updatingCTS.Token.ThrowIfCancellationRequested();

                    await UpdateZipStreamCopyAsync(zipStream, zipLocalStream, progress, _updatingCTS.Token);

                    _updatingCTS.Token.ThrowIfCancellationRequested();
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Log("Error downloading the update.", ex);
                    Paths.CreateOrClearTempPath(TempPaths.Update);
                    Core.Dialogs.ShowError(LText.Update.ErrorDownloadingUpdate, LText.AlertMessages.Alert, MBoxIcon.Warning);
                    return;
                }

                try
                {
                    Core.View.SetProgressBoxState_Single(
                        message2: LText.Update.UnpackingUpdate
                    );

                    await using var fs = File.OpenRead(localZipFile);

                    _updatingCTS.Token.ThrowIfCancellationRequested();

                    using var archive = new ZipArchive(fs, ZipArchiveMode.Read);

                    _updatingCTS.Token.ThrowIfCancellationRequested();

                    Paths.CreateOrClearTempPath(TempPaths.Update);

                    _updatingCTS.Token.ThrowIfCancellationRequested();

                    archive.Update_ExtractToDirectory_Fast(Paths.UpdateTemp, progress, _updatingCTS.Token);

                    _updatingCTS.Token.ThrowIfCancellationRequested();
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Log("Error unpacking the update.", ex);
                    Paths.CreateOrClearTempPath(TempPaths.Update);
                    Core.Dialogs.ShowError(LText.Update.ErrorUnpackingUpdate, LText.AlertMessages.Alert, MBoxIcon.Warning);
                    return;
                }

                // Save out the config BEFORE starting the update copier, so it can get the right theme/lang
                Ini.WriteConfigIni();

                _updatingCTS.Token.ThrowIfCancellationRequested();

                if (!File.Exists(Paths.UpdateExe))
                {
                    try
                    {
                        // Last-ditch attempt - extremely unlikely for this bak file to still exist and the
                        // normal exe not (maybe impossible currently?)
                        File.Move(Paths.UpdateExeBak, Paths.UpdateExe);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        Log("File not found: '" + Paths.UpdateExe + "'. Couldn't finish the update.");
                        Core.Dialogs.ShowError(LText.Update.UpdaterExeNotFound + $"{NL}{NL}" + Paths.UpdateExe);
                        Paths.CreateOrClearTempPath(TempPaths.Update);
                        return;
                    }
                }

                _updatingCTS.Token.ThrowIfCancellationRequested();

                try
                {
                    Utils.ProcessStart_UseShellExecute(new ProcessStartInfo(Paths.UpdateExe, "-go"));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Log("Unable to start '" + Paths.UpdateExe + "'. Couldn't finish the update.", ex);
                    Core.Dialogs.ShowError(LText.Update.UpdaterExeStartFailed + $"{NL}{NL}" + Paths.UpdateExe);
                    Paths.CreateOrClearTempPath(TempPaths.Update);
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                Paths.CreateOrClearTempPath(TempPaths.Update);
                return;
            }
            finally
            {
                _updatingCTS.Dispose();
                Paths.CreateOrClearTempPath(TempPaths.UpdateAppDownload);
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
            Core.View.SetProgressPercent(percents.MainPercent);
        }

        static async Task UpdateZipStreamCopyAsync(
            Stream source,
            Stream destination,
            IProgress<ProgressPercents> progress,
            CancellationToken cancellationToken)
        {
            ProgressPercents percents = new();

            byte[] buffer = ArrayPool<byte>.Shared.Rent(81920);
            try
            {
                int streamLength = (int)source.Length;
                int bytesRead;
                int totalBytesRead = 0;
                while ((bytesRead = await source.ReadAsync(buffer, cancellationToken)) != 0)
                {
                    await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    totalBytesRead += bytesRead;

                    percents.SubPercent = GetPercentFromValue_Int(totalBytesRead, streamLength);
                    percents.MainPercent = percents.SubPercent / 2;
                    progress.Report(percents);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
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
            if (await CheckIfUpdateAvailable(CancellationToken.None) == CheckUpdateResult.UpdateAvailable)
            {
                Core.View.Invoke(static () => Core.View.ShowUpdateNotification(true));
            }
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

    private static async Task<CheckUpdateResult> CheckIfUpdateAvailable(CancellationToken cancellationToken)
    {
        try
        {
            if (!Version.TryParse(Core.ViewEnv.ProductVersion, out Version? appVersion))
            {
                return CheckUpdateResult.NoUpdateAvailable;
            }

            using var request = await GlobalHttpClient.GetAsync(_latestVersionFile, cancellationToken);

            if (!request.IsSuccessStatusCode)
            {
                if (request.StatusCode == HttpStatusCode.NotFound)
                {
                    return CheckUpdateResult.NoUpdateAvailable;
                }
                else
                {
                    Log("Update check failed. Status code: " + request.StatusCode);
                    return CheckUpdateResult.Error;
                }
            }

            string versionString = await request.Content.ReadAsStringAsync(cancellationToken);

            return !versionString.IsEmpty() &&
                   Version.TryParse(versionString, out Version? version) &&
                   version > appVersion
                ? CheckUpdateResult.UpdateAvailable
                : CheckUpdateResult.NoUpdateAvailable;
        }
        catch (Exception ex)
        {
            Log("Update check failed.", ex);
            return CheckUpdateResult.Error;
        }
    }

    internal static async Task DoManualCheck()
    {
        CheckUpdateResult updateAvailable;
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

        switch (updateAvailable)
        {
            case CheckUpdateResult.UpdateAvailable:
                await ShowUpdateAskDialog();
                break;
            case CheckUpdateResult.NoUpdateAvailable:
                Core.Dialogs.ShowAlert(
                    LText.Update.NoUpdatesAvailable,
                    LText.Update.UpdateAlertBoxTitle,
                    MBoxIcon.Information);
                break;
            default:
                Core.Dialogs.ShowError(
                    LText.Update.CouldNotAccessUpdateServer,
                    LText.Update.UpdateAlertBoxTitle,
                    MBoxIcon.Warning);
                break;
        }
    }

    internal static async Task<(UpdateDetailsDownloadResult Result, List<UpdateInfo> UpdateInfos)>
    GetUpdateDetails(AutoResetEvent downloadARE) => await Task.Run(async () =>
    {
        try
        {
            _detailsDownloadCTS = _detailsDownloadCTS.Recreate();

            List<UpdateInfo> ret = new();

            List<UpdateInfoInternal> versions = new();

            if (!Version.TryParse(Core.ViewEnv.ProductVersion, out Version? appVersion))
            {
                return (UpdateDetailsDownloadResult.Error, ret);
            }

            UpdateInfoInternal? updateFile = null;

            using var request = await GlobalHttpClient.GetAsync(_versionsFile, _detailsDownloadCTS.Token);

            _detailsDownloadCTS.Token.ThrowIfCancellationRequested();

            if (!request.IsSuccessStatusCode) return (UpdateDetailsDownloadResult.Error, ret);

            Stream versionFileStream = await request.Content.ReadAsStreamAsync();

            _detailsDownloadCTS.Token.ThrowIfCancellationRequested();

            using (var sr = new StreamReader(versionFileStream))
            {
                _detailsDownloadCTS.Token.ThrowIfCancellationRequested();

                while (await sr.ReadLineAsync() is { } line)
                {
                    _detailsDownloadCTS.Token.ThrowIfCancellationRequested();

                    string lineT = line.Trim();

                    if (lineT.IsEmpty()) continue;

                    if (lineT[0] == '[' && lineT[^1] == ']' &&
                        Version.TryParse(lineT.AsSpan(1, lineT.Length - 2), out Version? version))
                    {
                        if (version <= appVersion) break;

                        if (updateFile != null) versions.Add(updateFile);
                        updateFile = new UpdateInfoInternal(version);
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

                _detailsDownloadCTS.Token.ThrowIfCancellationRequested();

                if (updateFile != null) versions.Add(updateFile);
            }

            if (versions.Count == 0) return (UpdateDetailsDownloadResult.NoUpdatesFound, ret);

            foreach (UpdateInfoInternal item in versions)
            {
                Uri? changelogUri = item.ChangelogUrl;
                Uri? downloadUri = item.DownloadUrl;

                if (changelogUri != null && downloadUri != null)
                {
                    using var changelogRequest = await GlobalHttpClient.GetAsync(changelogUri, _detailsDownloadCTS.Token);

                    _detailsDownloadCTS.Token.ThrowIfCancellationRequested();

                    string changelogText = await changelogRequest.Content.ReadAsStringAsync();

                    _detailsDownloadCTS.Token.ThrowIfCancellationRequested();

                    // Normalize linebreaks, because they'll normally be Unix-style on GitHub
                    changelogText = changelogText.NormalizeToCRLF().Trim();

                    if (request.IsSuccessStatusCode)
                    {
                        ret.Add(new UpdateInfo(
                            item.Version,
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
            _detailsDownloadCTS.Dispose();
            downloadARE.Set();
        }
    });

    /*
    Because the release packager might end up private, here's the spec for server-side changelogs.
    They're just plain text, no Markdown, except that list items are like:
    
    Some line before the list
     - An item
     - Another item
      - A sub-item
     - Yet another main item

    Lists are indented by one space to start with, and one is added for every sub-list. Sort of like Markdown but
    a bit modified to make it extremely easy to parse lists without carrying around Markdig or what have you. We
    use Markdig for the release packager so we don't have to use it here. We use a single space for indenting so
    that one indent == one char. So if we want to convert the space to a tab or whatever, it's trivial, and we
    can also trivially convert the dashes to Unicode bullets or whatever we want. Section headers can be detected
    by the simple heuristic of a line ending with : and not starting with a dash after 0 or more whitespaces.

    We don't use full Markdown for the release notes because we have to convert them to RTF, and I don't want to
    have to support the entirety of Markdown in the RTF converter. All we really want are section headers and
    lists.
    */
    internal static string[] GetFormattedPlainTextReleaseNotesLines(string text)
    {
        const string bullet = "\x2022";

        string[] lines = text.Split("\r\n");

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            int listCharIndex = ListCharIndex(line);
            if (listCharIndex > -1)
            {
                lines[i] = string.Concat(line.AsSpan(0, listCharIndex), bullet, line.AsSpan(listCharIndex + 1));
                line = lines[i];
            }

            if (line.StartsWithO(" "))
            {
                int nonSpaceIndex = NonSpaceCharIndex(line);
                if (listCharIndex > -1)
                {
                    lines[i] = new string(' ', nonSpaceIndex * 4) + line.TrimStart();
                }
            }
        }

        return lines;

        static int ListCharIndex(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (char.IsWhiteSpace(c)) continue;
                return c == '-' ? i : -1;
            }
            return -1;
        }

        static int NonSpaceCharIndex(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c != ' ') return i;
            }
            return -1;
        }
    }
}
