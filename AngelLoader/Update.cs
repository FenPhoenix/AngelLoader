//#define CHECK_UPDATES

#if CHECK_UPDATES
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AL_Common.Common;
using static AL_Common.Logger;

namespace AngelLoader;

internal static class CheckUpdates
{
    private static CancellationTokenSource CheckForUpdatesCTS = new CancellationTokenSource();

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

    private class UpdateFile
    {
        internal Version? Version;
        internal byte[]? Checksum;
        internal Uri? DownloadUrl;
        internal Uri? ChangelogUrl;
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
                if (line.StartsWithFast_NoNullChecks("Version="))
                {
                    if (Version.TryParse(val, out Version result))
                    {
                        ret.Version = result;
                    }
                }
                if (line.StartsWithFast_NoNullChecks("Checksum="))
                {
                    byte[] bytes = HexStringToBytes(val);
                    if (bytes.Length > 0)
                    {
                        ret.Checksum = bytes;
                    }
                }
                else if (line.StartsWithFast_NoNullChecks("DownloadUrl="))
                {
                    if (Uri.IsWellFormedUriString(val, UriKind.Absolute))
                    {
                        ret.DownloadUrl = new Uri(val);
                    }
                }
                else if (line.StartsWithFast_NoNullChecks("ChangelogUrl="))
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
