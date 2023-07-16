using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FMInfoGen.Misc;

namespace FMInfoGen;

internal static class FMExtract
{
    private static CancellationTokenSource? _extractCTS;

    #region Extract FM archives

    internal static void ExtractFMArchive()
    {
        string selectedFM = Core.View.GetSelectedFM();
        string archive = Path.Combine(Config.FMsPath, selectedFM);
        string extractDir = Path.Combine(Config.TempPath, selectedFM.FN_NoExt().Trim());

        if (Directory.Exists(extractDir)) Directory.Delete(extractDir, recursive: true);

        Directory.CreateDirectory(extractDir);

        try
        {
            ZipFile.ExtractToDirectory(archive, extractDir);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Exception extracting archive:\r\n" +
                            archive + "\r\n\r\n" +
                            ex.Message);
        }

        MessageBox.Show("FM zip file extracted to temp folder.");

        Core.UpdateExtractedFMsListBox();
    }

    internal static async Task ExtractAllFMArchives()
    {
        bool exceptionsCaught = false;
        bool canceled = false;

        try
        {
            _extractCTS = new CancellationTokenSource();

            Core.View.BeginExtractArchiveMode();

            var fmArchives = Core.View.GetFMArchives().ToArray();

            for (int i = 0; i < fmArchives.Length; i++)
            {
                string fmArchive = fmArchives[i];

                string archive = Path.Combine(Config.FMsPath, fmArchive);
                string extractDir = Path.Combine(Config.TempPath, fmArchive.FN_NoExt().Trim());

                bool overwriteFolders = Core.View.GetOverwriteFoldersChecked();

                try
                {
                    if (Directory.Exists(extractDir) && overwriteFolders)
                    {
                        Directory.Delete(extractDir, recursive: true);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    throw;
                }

                if (overwriteFolders || !Directory.Exists(extractDir))
                {
                    // CreateDirectory already skips dirs if they exist, but not sure if it still walks down
                    // the tree and creates subdirectories if they don't exist? Gonna be explicit here.
                    if (!Directory.Exists(extractDir)) Directory.CreateDirectory(extractDir);

                    bool success = await Task.Run(() =>
                    {
                        try
                        {
                            ZipFile.ExtractToDirectory(archive, extractDir);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Exception extracting archive:\n" +
                                            archive + "\n\n" +
                                            ex.Message);
                            return false;
                        }
                    });

                    if (!success) exceptionsCaught = true;
                }

                if (_extractCTS.IsCancellationRequested)
                {
                    canceled = true;
                    break;
                }

                Core.View.SetExtractProgressBarValue((100 * i) / fmArchives.Length);

                await Task.Delay(1);
            }

            if (exceptionsCaught)
            {
                MessageBox.Show("Exception(s) extracting archive. Look at the console log man!");
            }
        }
        finally
        {
            _extractCTS?.Dispose();

            Core.View.EndExtractArchiveMode();

            MessageBox.Show(canceled
                ? "Extract all cancelled."
                : "All FM zip files extracted to temp folder.");

            Core.UpdateExtractedFMsListBox();
        }
    }

    internal static void Cancel() => _extractCTS?.CancelIfNotDisposed();

    #endregion
}
