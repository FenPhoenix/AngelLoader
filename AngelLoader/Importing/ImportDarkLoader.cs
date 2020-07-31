using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI;
using FMScanner;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader.Importing
{
    internal static class ImportDarkLoader
    {
        #region Private fields

        // TODO: Make these non-static

        private static readonly string[] _nonFMHeaders =
        {
            "[options]",
            "[window]",
            "[mission directories]",
            "[Thief 1]",
            "[Thief 2]",
            "[Thief2x]",
            "[SShock 2]"
        };

        // Not used - we scan for game types ourselves currently
        //private enum DLGame
        //{
        //    darkGameUnknown = 0, // <- if it hasn't been scanned, it will be this
        //    darkGameThief = 1,
        //    darkGameThief2 = 2,
        //    darkGameT2x = 3,
        //    darkGameSS2 = 4
        //}

        private static readonly Regex _darkLoaderFMRegex = new Regex(@"\.[0123456789]+]$", RegexOptions.Compiled);

        #endregion

        #region Helpers

        private static string RemoveDLArchiveBadChars(string archive)
        {
            foreach (string s in new[] { "]", "\u0009", "\u000A", "\u000D" }) archive = archive.Replace(s, "");
            return archive;
        }

        // Don't replace \r\n or \\ escapes because we use those in the exact same way so no conversion needed
        private static string DLUnescapeChars(string str) => str.Replace(@"\t", "\u0009").Replace(@"\""", "\"");

        #endregion

        internal static async Task<bool>
        Import(string iniFile, bool importFMData, bool importSaves, FieldsToImport fields)
        {
            Core.View.ShowProgressBox(ProgressTasks.ImportFromDarkLoader);
            try
            {
                var (error, fmsToScan) = await ImportInternal(iniFile, importFMData, importSaves, fields: fields);
                if (error != ImportError.None)
                {
                    Log("Import.Error: " + error, stackTrace: true);

                    if (error == ImportError.NoArchiveDirsFound)
                    {
                        Core.View.ShowAlert(LText.Importing.DarkLoader_NoArchiveDirsFound, LText.AlertMessages.Alert);
                        return false;
                    }

                    Core.View.ShowAlert(
                        "An error occurred with DarkLoader importing. See the log file for details. " +
                        "Aborting import operation.", LText.AlertMessages.Error);

                    return false;
                }

                await FMScan.ScanAndFind(fmsToScan,
                    ScanOptions.FalseDefault(scanGameType: true, scanCustomResources: true));
            }
            catch (Exception ex)
            {
                Log("Exception in DarkLoader import", ex);

                Core.View.ShowAlert(
                    "An error occurred with DarkLoader importing. See the log file for details. " +
                    "Aborting import operation.", LText.AlertMessages.Error);

                return false;
            }
            finally
            {
                Core.View.HideProgressBox();
            }

            return true;
        }

        private static async Task<(ImportError Error, List<FanMission> FMs)>
        ImportInternal(string iniFile, bool importFMData, bool importSaves, bool returnUnmergedFMsList = false,
            FieldsToImport? fields = null)
        {
            string[] lines = await Task.Run(() => File.ReadAllLines(iniFile));
            var fms = new List<FanMission>();

            var error = ImportError.None;

            if (importFMData)
            {
                bool missionDirsRead = false;
                var archiveDirs = new List<string>();

                error = await Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            string lineTS = line.TrimStart();
                            string lineTB = lineTS.TrimEnd();

                            #region Read archive directories

                            // We need to know the archive dirs before doing anything, because we may need to recreate
                            // some lossy names (if any bad chars have been removed by DarkLoader).
                            if (!missionDirsRead && lineTB == "[mission directories]")
                            {
                                while (i < lines.Length - 1)
                                {
                                    string lt = lines[i + 1].Trim();
                                    if (!lt.IsEmpty() && lt[0] != '[' && lt.EndsWith("=1"))
                                    {
                                        archiveDirs.Add(lt.Substring(0, lt.Length - 2));
                                    }
                                    else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                                    {
                                        break;
                                    }
                                    i++;
                                }

                                if (archiveDirs.Count == 0 || archiveDirs.All(x => x.IsWhiteSpace()))
                                {
                                    return ImportError.NoArchiveDirsFound;
                                }

                                // Restart from the beginning of the file, this time skipping anything that isn't an
                                // FM entry
                                i = -1;
                                missionDirsRead = true;
                                continue;
                            }

                            #endregion

                            #region Read FM entries

                            // MUST CHECK missionDirsRead OR IT ADDS EVERY FM TWICE!
                            if (missionDirsRead &&
                                !_nonFMHeaders.Contains(lineTB) && lineTB.Length > 0 && lineTB[0] == '[' &&
                                lineTB[lineTB.Length - 1] == ']' && lineTB.Contains('.') &&
                                _darkLoaderFMRegex.Match(lineTB).Success)
                            {
                                int lastIndexDot = lineTB.LastIndexOf('.');
                                string archive = lineTB.Substring(1, lastIndexDot - 1);
                                string size = lineTB.Substring(lastIndexDot + 1, lineTB.Length - lastIndexDot - 2);

                                foreach (string dir in archiveDirs)
                                {
                                    if (!Directory.Exists(dir)) continue;
                                    try
                                    {
                                        // DarkLoader only does zip format
                                        foreach (string f in FastIO.GetFilesTopOnly(dir, "*.zip"))
                                        {
                                            string fn = Path.GetFileNameWithoutExtension(f);
                                            if (RemoveDLArchiveBadChars(fn).EqualsI(archive))
                                            {
                                                archive = fn;
                                                goto breakout;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log("Exception in DarkLoader archive dir file enumeration", ex);
                                    }
                                }

                                breakout:

                                // Add .zip back on; required because everything expects it, and furthermore if there's
                                // a dot anywhere in the name then everything after it will be treated as the extension
                                // and is liable to be lopped off at any time
                                archive += ".zip";

                                ulong.TryParse(size, out ulong sizeBytes);
                                var fm = new FanMission
                                {
                                    Archive = archive,
                                    InstalledDir = archive.ToInstDirNameFMSel(),
                                    SizeBytes = sizeBytes
                                };

                                // We don't import game type, because DarkLoader by default gets it wrong for NewDark
                                // FMs (the user could have changed it manually in the ini file, and in fact it's
                                // somewhat likely they would have done so, but still, better to just scan for it
                                // ourselves later)

                                while (i < lines.Length - 1)
                                {
                                    string lts = lines[i + 1].TrimStart();
                                    string ltb = lts.TrimEnd();

                                    if (lts.StartsWith("comment=\""))
                                    {
                                        string comment = ltb.Substring(9);
                                        if (comment.Length >= 2 && comment[comment.Length - 1] == '\"')
                                        {
                                            comment = comment.Substring(0, comment.Length - 1);
                                            fm.Comment = DLUnescapeChars(comment);
                                        }
                                    }
                                    else if (lts.StartsWith("title=\""))
                                    {
                                        string title = ltb.Substring(7);
                                        if (title.Length >= 2 && title[title.Length - 1] == '\"')
                                        {
                                            title = title.Substring(0, title.Length - 1);
                                            fm.Title = DLUnescapeChars(title);
                                        }
                                    }
                                    else if (lts.StartsWith("misdate="))
                                    {
                                        ulong.TryParse(ltb.Substring(8), out ulong result);
                                        try
                                        {
                                            var date = new DateTime(1899, 12, 30).AddDays(result);
                                            fm.ReleaseDate.DateTime = date.Year > 1998 ? date : (DateTime?)null;
                                        }
                                        catch (ArgumentOutOfRangeException)
                                        {
                                            fm.ReleaseDate.DateTime = null;
                                        }
                                    }
                                    else if (lts.StartsWith("date="))
                                    {
                                        ulong.TryParse(ltb.Substring(5), out ulong result);
                                        try
                                        {
                                            var date = new DateTime(1899, 12, 30).AddDays(result);
                                            fm.LastPlayed.DateTime = date.Year > 1998 ? date : (DateTime?)null;
                                        }
                                        catch (ArgumentOutOfRangeException)
                                        {
                                            fm.LastPlayed.DateTime = null;
                                        }
                                    }
                                    else if (lts.StartsWith("finished="))
                                    {
                                        uint.TryParse(ltb.Substring(9), out uint result);
                                        // result will be 0 on fail, which is the empty value so it's fine
                                        fm.FinishedOn = result;
                                    }
                                    else if (!ltb.IsEmpty() && ltb[0] == '[' && ltb[ltb.Length - 1] == ']')
                                    {
                                        break;
                                    }
                                    i++;
                                }

                                fms.Add(fm);
                            }

                            #endregion
                        }
                        return ImportError.None;
                    }
                    catch (Exception ex)
                    {
                        Log("Exception in " + nameof(ImportDarkLoader) + "." + nameof(ImportInternal), ex);
                        return ImportError.Unknown;
                    }
                    finally
                    {
                        Core.View.InvokeSync(new Action(Core.View.HideProgressBox));
                    }
                });
            }

            if (error != ImportError.None) return (error, fms);

            if (importSaves)
            {
                bool success = await ImportSaves(lines);
            }

            var importedFMs = returnUnmergedFMsList
                ? fms
                : ImportCommon.MergeImportedFMData(ImportType.DarkLoader, fms, fields);

            return (ImportError.None, importedFMs);
        }

        private static async Task<bool>
        ImportSaves(string[] lines)
        {
            // We DON'T use game generalization here, because DarkLoader only supports T1/T2/SS2 and will never
            // change (it's not updated anymore). So it's okay that we code those games in manually here.

            string t1Dir = "";
            string t2Dir = "";
            string ss2Dir = "";
            bool t1DirRead = false;
            bool t2DirRead = false;
            bool ss2DirRead = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string lineTS = line.TrimStart();
                string lineTB = lineTS.TrimEnd();

                if (lineTB == "[options]")
                {
                    while (i < lines.Length - 1)
                    {
                        string lt = lines[i + 1].Trim();
                        if (lt.StartsWithI("thief1dir="))
                        {
                            t1Dir = lt.Substring(10).Trim();
                            t1DirRead = true;
                        }
                        else if (lt.StartsWithI("thief2dir="))
                        {
                            t2Dir = lt.Substring(10).Trim();
                            t2DirRead = true;
                        }
                        else if (lt.StartsWithI("shock2dir="))
                        {
                            ss2Dir = lt.Substring(10).Trim();
                            ss2DirRead = true;
                        }
                        else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }
                        if (t1DirRead && t2DirRead && ss2DirRead) goto breakout;
                        i++;
                    }
                }
            }

            breakout:

            if (t1Dir.IsWhiteSpace() && t2Dir.IsWhiteSpace() && ss2Dir.IsWhiteSpace()) return true;

            await Task.Run(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i == 0 && t1Dir.IsEmpty()) continue;
                    if (i == 1 && t2Dir.IsEmpty()) continue;
                    if (i == 2 && ss2Dir.IsEmpty()) continue;

                    string savesPath = Path.Combine(i switch { 0 => t1Dir, 1 => t2Dir, _ => ss2Dir }, "allsaves");
                    if (!Directory.Exists(savesPath)) continue;

                    string convertedPath = Path.Combine(Config.FMsBackupPath, Paths.DarkLoaderSaveBakDir);
                    Directory.CreateDirectory(convertedPath);

                    // Converting takes too long, so just copy them to our backup folder and they'll be handled
                    // appropriately next time the user installs an FM
                    foreach (string f in FastIO.GetFilesTopOnly(savesPath, "*.zip"))
                    {
                        string dest = Path.Combine(convertedPath, f.GetFileNameFast());
                        File.Copy(f, dest, overwrite: true);
                    }
                }
            });

            return true;
        }
    }
}
