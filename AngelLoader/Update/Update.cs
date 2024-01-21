#define CHECK_UPDATES

#if CHECK_UPDATES
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// @Update: Get rid of WinForms reference here when we're done
using System.Windows.Forms;
using AL_Common;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.Global;

namespace AngelLoader;

internal static class CheckUpdates
{
    private sealed class UpdateFile
    {
        internal Version? Version;
        internal byte[]? Checksum;
        internal Uri? DownloadUrl;
        internal Uri? ChangelogUrl;
    }

    internal sealed class UpdateInfo
    {
        internal readonly Version Version;
        internal readonly string ChangelogText;
        internal readonly Uri DownloadUri;

        internal UpdateInfo(Version version, string changelogText, Uri downloadUri)
        {
            Version = version;
            ChangelogText = changelogText;
            DownloadUri = downloadUri;
        }
    }

    internal static async Task ShowUpdateAskDialog(List<UpdateInfo> updateInfos)
    {
        // @Update: Test with multiple versions/changelogs
        string changelogFullText = "";
        for (int i = 0; i < updateInfos.Count; i++)
        {
            if (i > 0) changelogFullText += "\r\n\r\n\r\n";
            UpdateInfo? item = updateInfos[i];
            changelogFullText += item.Version + ":\r\n" + item.ChangelogText;
        }

        bool accepted = Core.View.ShowUpdateAvailableDialog(changelogFullText);

        if (accepted)
        {
            UpdateInfo? latest = updateInfos[0];
            using var fs = File.OpenRead(latest.DownloadUri.LocalPath);
            using var archive = new ZipArchive(fs, ZipArchiveMode.Read);
            Paths.CreateOrClearTempPath(Paths.UpdateTemp);
            archive.ExtractToDirectory(Paths.UpdateTemp);
            // @Update: Put a UI-disabling progress meter or whatnot here
            //Core.Dialogs.ShowAlert("Extract done!", "Test");

            Config.UpdateInfosTempCache.Clear();

            // @Update: Call the update copier and exit main AL app here.
            Utils.ProcessStart_UseShellExecute(new ProcessStartInfo(Path.Combine(Paths.Startup, "Update.exe"), "-go"));
            Application.Exit();
        }
    }

    internal static async Task<(bool Success, List<UpdateInfo> UpdateInfos)> Check2024()
    {
        return await Task.Run(static () =>
        {
            List<UpdateInfo> ret = new();

#if X64
            const string versionsFile = @"G:\AngelLoader_Public_Zips\update_local\framework_x64\versions.ini";
#else
            const string versionsFile = @"G:\AngelLoader_Public_Zips\update_local\framework_x86\versions.ini";
#endif

            List<UpdateFile> versions = new();

            if (!Version.TryParse(Application.ProductVersion, out Version appVersion))
            {
                return (false, ret);
            }

            // @Update: Remove all Trace calls for final
            Trace.WriteLine(versionsFile);
            //var uri = new Uri(versionsFile);
            //using Stream latestVersionStream = await GlobalHttpClient.GetStreamAsync(uri);
            UpdateFile? updateFile = null;

            using Stream versionFileStream = File.OpenRead(versionsFile);
            using (var sr = new StreamReader(versionFileStream))
            {
                while (sr.ReadLine() is { } line)
                {
                    string lineT = line.Trim();

                    if (lineT.IsEmpty()) continue;

                    if (lineT[0] == '[' && lineT[lineT.Length - 1] == ']' &&
                        Version.TryParse(lineT.Substring(1, lineT.Length - 2), out Version version))
                    {
                        if (version <= appVersion) break;

                        Trace.WriteLine("Header: Version " + version);
                        if (updateFile != null) versions.Add(updateFile);
                        updateFile = new UpdateFile { Version = version };
                    }
                    else if (updateFile != null)
                    {
                        if (lineT.StartsWithO("ChangelogUrl="))
                        {
                            updateFile.ChangelogUrl = new Uri(lineT.Substring("ChangelogUrl=".Length));
                            Trace.WriteLine("Changelog location: " + updateFile.ChangelogUrl);
                        }
                        else if (lineT.StartsWithO("DownloadUrl="))
                        {
                            updateFile.DownloadUrl = new Uri(lineT.Substring("DownloadUrl=".Length));
                            Trace.WriteLine("Download location: " + updateFile.DownloadUrl);
                        }
                    }
                }
                if (updateFile != null) versions.Add(updateFile);
            }

            if (versions.Count == 0) return (false, ret);

            foreach (UpdateFile item in versions)
            {
                Uri? changelogUri = item.ChangelogUrl;
                Uri? downloadUri = item.DownloadUrl;

                if (changelogUri != null && downloadUri != null)
                {
                    ret.Add(new UpdateInfo(
                        item.Version!,
                        File.ReadAllText(changelogUri.LocalPath).Trim(),
                        downloadUri));
                }
            }

            return (ret.Count > 0, ret);
        });
    }

    private static CancellationTokenSource CheckForUpdatesCTS = new();

    internal static void Cancel() => CheckForUpdatesCTS.CancelIfNotDisposed();

    internal static async Task Check()
    {
        CheckForUpdatesCTS = CheckForUpdatesCTS.Recreate();

        //const string uri = "https://fenphoenix.github.io/AngelLoader/al_update/al_update.ini";
        const string uri = "https://fenphoenix.github.io/AngelLoader/al_update_testing/al_update.ini";
        const string localFile = @"C:\al_update_ini_test.ini";

        using var wc = new WebClient();

        try
        {
            var req = WebRequest.Create(uri);

            WebResponse response;
            using (CheckForUpdatesCTS.Token.Register(() => req.Abort()))
            {
                try
                {
                    response = await req.GetResponseAsync();
                }
                catch (WebException)
                {
                    if (CheckForUpdatesCTS.IsCancellationRequested)
                    {
                        return;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            req.Method = WebRequestMethods.Http.Head;

            long fileSize = response.ContentLength;
            if (fileSize < 0)
            {
                MessageBox.Show(@"Couldn't get the file size; quitting. Check the log.");
                Log("Error getting the file size: it was -1");
                return;
            }
            else if (fileSize == 0)
            {
                MessageBox.Show(@"File is empty; quitting.", @"Alert");
                return;
            }
            else if (fileSize > ByteSize.KB * 10)
            {
                MessageBox.Show(@"File size is over 10 KB(" +
                                fileSize.ToString(CultureInfo.InvariantCulture.NumberFormat) +
                                @" bytes). That's too large; quitting.", @"Alert");
                return;
            }
            else
            {
                MessageBox.Show(@"File size is OK: " +
                                fileSize.ToString(CultureInfo.CurrentCulture.NumberFormat) +
                                @" bytes.", @"Notice");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(@"Couldn't get the file size; quitting. Check the log.");
            Log("Exception getting the file size.", ex);
            return;
        }

        try
        {
            await wc.DownloadFileTaskAsync(uri, localFile);

            if (CheckForUpdatesCTS.IsCancellationRequested) return;

            var (success, updateFile) = ReadUpdateFile(localFile);
            if (!success)
            {
                MessageBox.Show(@"Exception while reading update ini file; quitting.", @"Alert");
                return;
            }

            Version assemblyVersion;
            if (updateFile.Version != null &&
                (assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version) != null &&
                updateFile.Version.CompareTo(assemblyVersion) > 0)
            {
                MessageBox.Show(@"Downloaded version number is higher - test successful", @"Notice");
            }
            else
            {
                MessageBox.Show(@"No new version.", @"Notice");
                return;
            }
        }
        catch (WebException ex)
        {
            MessageBox.Show(@"Exception downloading file. Check the log.", @"Alert");
            Log("Exception downloading update ini file:", ex);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(@"File is in use; quitting.", @"Alert");
            Log("File is in use: " + localFile, ex);
        }
        finally
        {
            try
            {
                File.Delete(localFile);
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show(@"Directory not found: " + localFile.GetDirNameFast(), @"Alert");
                Log("Directory not found: " + localFile.GetDirNameFast(), ex);
            }
            catch (PathTooLongException ex)
            {
                MessageBox.Show(@"The path is too long: " + localFile, @"Alert");
                Log("The path is too long: " + localFile, ex);
            }
            catch (IOException ex)
            {
                MessageBox.Show(@"The file is in use: " + localFile, @"Alert");
                Log("The file is in use: " + localFile, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Unauthorized access exception. Message:\r\n" + ex.Message, @"Alert");
                Log("Unauthorized access exception. Message:\r\n" + ex.Message, ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Exception deleting temp version file. Check the log.", @"Alert");
                Log("Exception deleting temp version file.", ex);
            }
        }
    }

    private static byte[] HexStringToBytes(string value)
    {
        if (value.IsEmpty() || value.Length < 2 || value.Length % 2 != 0) return Array.Empty<byte>();

        var bytes = new byte[value.Length / 2];
        for (int bi = 0; bi < value.Length; bi += 2)
        {
            string s = value.Substring(bi, 2);
            if (byte.TryParse(s, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out byte result))
            {
                bytes[bi / 2] = result;
            }
        }

        return bytes;
    }

    private static (bool Success, UpdateFile UpdateFile)
    ReadUpdateFile(string file)
    {
        var ret = new UpdateFile();

        try
        {
            var lines = File.ReadAllLines(file, Encoding.UTF8);

            for (int i = 0; i < Math.Min(3, lines.Length); i++)
            {
                string line = lines[i];
                if (!line.Contains('=')) continue;

                string val = line.Substring(line.IndexOf('=') + 1);
                if (line.StartsWithFast("Version="))
                {
                    if (Version.TryParse(val, out Version result))
                    {
                        ret.Version = result;
                    }
                }
                if (line.StartsWithFast("Checksum="))
                {
                    byte[] bytes = HexStringToBytes(val);
                    if (bytes.Length > 0)
                    {
                        ret.Checksum = bytes;
                    }
                }
                else if (line.StartsWithFast("DownloadUrl="))
                {
                    if (Uri.IsWellFormedUriString(val, UriKind.Absolute))
                    {
                        ret.DownloadUrl = new Uri(val);
                    }
                }
                else if (line.StartsWithFast("ChangelogUrl="))
                {
                    if (Uri.IsWellFormedUriString(val, UriKind.Absolute))
                    {
                        ret.ChangelogUrl = new Uri(val);
                    }
                }
            }
        }
        catch (Exception)
        {
            return (false, ret);
        }

        return (true, ret);
    }
}
#endif
