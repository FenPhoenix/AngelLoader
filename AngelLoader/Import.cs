﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using FMScanner;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.SettingsWindowData;
using static AngelLoader.Utils;

namespace AngelLoader;

/*
@Import: Overhaul the import system. It needs:
-Quadratic searches changed to hash lookups
-NDL 1.7.0 AdditionalArchiveRoots support
-Thorough testing with full sets from each loader
-Notes addressed and bugs fixed
-Perf
*/

internal static class Import
{
    private sealed class FieldsToImport
    {
        internal bool Title;
        internal bool ReleaseDate;
        internal bool LastPlayed;
        internal bool FinishedOn;
        internal bool Comment;
        internal bool Rating;
        internal bool DisabledMods;
        internal bool Tags;
        internal bool SelectedReadme;
        internal bool Size;
    }

    #region Public methods

    internal static async Task ImportFrom(ImportType importType)
    {
        bool importFMData = false;
        bool importSaves = false;

        FieldsToImport fields;

        var iniFiles = new List<string>();

        if (importType == ImportType.DarkLoader)
        {
            reshow:
            (bool accepted,
                string iniFile,
                importFMData,
                bool importTitle,
                bool importSize,
                bool importComment,
                bool importReleaseDate,
                bool importLastPlayed,
                bool importFinishedOn,
                importSaves,
                bool backupPathSetRequested) = Core.View.ShowDarkLoaderImportWindow();

            if (backupPathSetRequested)
            {
                await Core.OpenSettings(SettingsWindowState.BackupPathSet);
                // Don't do the Settings manual refresh here, because we're still "in" the import window, and
                // we'll auto-refresh when that closes (that is to say "for real" closes).
                goto reshow;
            }

            if (!accepted) return;

            if (!importFMData && !importSaves)
            {
                Core.Dialogs.ShowAlert(LText.Importing.NothingWasImported, LText.AlertMessages.Alert);
                return;
            }

            iniFiles.Add(iniFile);

            fields = new FieldsToImport
            {
                Title = importTitle,
                ReleaseDate = importReleaseDate,
                LastPlayed = importLastPlayed,
                Size = importSize,
                Comment = importComment,
                FinishedOn = importFinishedOn
            };
        }
        else
        {
            (bool accepted,
                string[] returnedIniFiles,
                bool importTitle,
                bool importReleaseDate,
                bool importLastPlayed,
                bool importComment,
                bool importRating,
                bool importDisabledMods,
                bool importTags,
                bool importSelectedReadme,
                bool importFinishedOn,
                bool importSize) = Core.View.ShowImportFromMultipleInisWindow(importType);

            if (!accepted) return;
            foreach (string file in returnedIniFiles) iniFiles.Add(file);

            if (iniFiles.All(static x => x.IsWhiteSpace()))
            {
                Core.Dialogs.ShowAlert(LText.Importing.NothingWasImported, LText.AlertMessages.Alert);
                return;
            }

            fields = new FieldsToImport
            {
                Title = importTitle,
                ReleaseDate = importReleaseDate,
                LastPlayed = importLastPlayed,
                Comment = importComment,
                Rating = importRating,
                DisabledMods = importDisabledMods,
                Tags = importTags,
                SelectedReadme = importSelectedReadme,
                FinishedOn = importFinishedOn,
                Size = importSize
            };
        }

        // Do this every time we modify FMsViewList in realtime, to prevent FMsDGV from redrawing from the
        // list when it's in an indeterminate state, which can cause a selection change (bad) and/or a visible
        // change of the list (not really bad but unprofessional looking).
        Core.View.DisableFMsListDisplay(inert: false);

        // ReSharper disable once ConvertToConstant.Local
        string dlErrorMessage = "An error occurred with DarkLoader importing. See the log file for details. Aborting import operation.";

        InstDirNameContext instDirNameContext = new();

        // For DarkLoader this will be only one file, so it works out.
        // This is so we can keep just the one await call.
        foreach (string iniFile in iniFiles)
        {
            if (iniFile.IsWhiteSpace()) continue;

            // Shove it all in here to avoid extra awaits
            #region Import

            try
            {
                Core.View.ShowProgressBox_Single(
                    message1: importType switch
                    {
                        ImportType.DarkLoader => LText.ProgressBox.ImportingFromDarkLoader,
                        ImportType.FMSel => LText.ProgressBox.ImportingFromFMSel,
                        _ => LText.ProgressBox.ImportingFromNewDarkLoader
                    },
                    progressType: ProgressType.Indeterminate
                );

                var (error, fmsToScan) = await Task.Run(() => importType switch
                {
                    ImportType.DarkLoader => ImportDarkLoaderInternal(iniFile, importFMData, importSaves, fields, instDirNameContext),
                    ImportType.FMSel => ImportFMSelInternal(iniFile, fields),
                    _ => ImportNDLInternal(iniFile, fields, instDirNameContext)
                });

                if (error != ImportError.None)
                {
                    Log("ImportError: " + error, stackTrace: true);

                    if (importType == ImportType.DarkLoader)
                    {
                        (string message, string title) = error == ImportError.NoArchiveDirsFound
                            ? (LText.Importing.DarkLoader_NoArchiveDirsFound, LText.AlertMessages.Alert)
                            : (dlErrorMessage, LText.AlertMessages.Error);

                        Core.Dialogs.ShowAlert(message, title);
                    }
                }
                else // No error
                {
                    if (fmsToScan.Count > 0)
                    {
                        var scanOptions = importType == ImportType.FMSel
                            ? ScanOptions.FalseDefault(scanGameType: true, scanCustomResources: true, scanSize: true)
                            // NewDarkLoader and DarkLoader both take this one
                            : ScanOptions.FalseDefault(scanGameType: true, scanCustomResources: true);

                        await FMScan.ScanFMs(fmsToScan, scanOptions);
                        /*
                        Doing a find after a scan. I forgot exactly why. Reasons I thought of:
                        -I might be doing it to get rid of any duplicates or bad data that may have been imported?
                        -2020-02-14: I'm also doing this to properly update the tags. Without this the imported
                        tags wouldn't work because they're only in TagsString and blah blah blah.
                        -But couldn't I just call the tag list updater?
                        -This also updates the FM stats on the UI, and anything else that FindFMs() might update.
                         We could just do that explicitly here of course, but as long as we're calling this anyway
                         we may as well let it do the work.
                        */
                        FindFMs.Find();
                        TDM.UpdateTDMDataFromDisk(refresh: false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ErrorText.Ex + "in " + importType + " import", ex);

                if (importType == ImportType.DarkLoader)
                {
                    Core.Dialogs.ShowAlert(dlErrorMessage, LText.AlertMessages.Error);
                }
            }
            finally
            {
                Core.View.HideProgressBox();
            }

            #endregion

            // Just to be explicit
            if (importType == ImportType.DarkLoader) break;
        }

        // Do this no matter what; because we set the row count to 0 the list MUST be refreshed
        // @DISPLAYED_FM_SYNC(ImportFrom() SortAndSetFilter() call):
        // It is REQUIRED to force-display the FM, to ensure the main view's internal displayed FM field
        // is not referencing a stale FM object that no longer exists in the list!
        await Core.View.SortAndSetFilter(forceDisplayFM: true);
    }

    #endregion

    #region Private methods

    // @Import(DarkLoader): DL writes its ini file in non-UTF8
    // Notepad++ detects mine as ANSI (presumably 1252)
    // Check the DarkLoader source code for how exactly it decides what encoding to write the ini in

    private static (ImportError Error, List<FanMission> FMs)
    ImportDarkLoaderInternal(string iniFile, bool importFMData, bool importSaves, FieldsToImport fields, InstDirNameContext instDirNameContext)
    {
        #region Local functions

        static string RemoveDLArchiveBadChars(string archive, string[] badChars)
        {
            foreach (string s in badChars)
            {
                archive = archive.Replace(s, "");
            }
            return archive;
        }

        // Don't replace \r\n or \\ escapes because we use those in the exact same way so no conversion needed
        static string DLUnescapeChars(string str) => str.Replace(@"\t", "\u0009").Replace(@"\""", "\"");

        #endregion

        #region Data

        string[] badChars = { "]", "\u0009", "\u000A", "\u000D" };

        HashSetI nonFMHeaders =
        [
            "[options]",
            "[window]",
            "[mission directories]",
            "[Thief 1]",
            "[Thief 2]",
            "[Thief2x]",
            "[SShock 2]"
        ];

        // Not used - we scan for game types ourselves currently
        //private enum DLGame
        //{
        //    darkGameUnknown = 0, // <- if it hasn't been scanned, it will be this
        //    darkGameThief = 1,
        //    darkGameThief2 = 2,
        //    darkGameT2x = 3,
        //    darkGameSS2 = 4
        //}

        Regex darkLoaderFMRegex = new(@"\.[0123456789]+]$", RegexOptions.Compiled);

        const string missionDirsHeader = "[mission directories]";

        #endregion

        var fms = new List<FanMission>();

        var archives = new DictionaryI<string>();

        ImportError DoImport()
        {
            try
            {
                // It appears that DarkLoader uses the default legacy system codepage (437 for North America usually),
                // although I don't understand Delphi Pascal enough to 100% confirm it doesn't have some additional
                // behavior... but this is likely to result in better text than UTF8, so.
                List<string> lines = File_ReadAllLines_List(iniFile, GetOEMCodePageOrFallback(Encoding.UTF8), true);

                if (importFMData)
                {
                    #region Read archive directories

                    // We need to know the archive dirs before doing anything, because we may need to recreate
                    // some lossy names (if any bad chars have been removed by DarkLoader).

                    /*
                    @Import(DarkLoader): Test when multiple archives with the same name exist in different archive dirs
                    Does it use first or last encountered? Check this!

                    @Import(DarkLoader duplicate archives) research:
                    -If two FMs are same-named but different-sized, it puts them both in the list, which it can
                     do because the size is part of the id.
                     Possible solutions:
                     -Come up with a way to differentiate same-named archives in our own database
                     -Ask the user which one they want to take (displaying metadata), and take that one only
                    */
                    for (int i = 0; i < lines.Count; i++)
                    {
                        string lineT = lines[i].Trim();
                        if (lineT.EqualsI(missionDirsHeader))
                        {
                            HashSetI archivesHash = new();

                            while (i < lines.Count - 1)
                            {
                                string lt = lines[i + 1].Trim();
                                if (!lt.IsEmpty() && lt[0] != '[' && lt.EndsWithO("=1"))
                                {
                                    string dir = lt.Substring(0, lt.Length - 2);
                                    if (dir.IsWhiteSpace() || !Directory.Exists(dir)) continue;
                                    try
                                    {
                                        // DarkLoader only does zip format
                                        foreach (string f in FastIO.GetFilesTopOnly(dir, "*.zip"))
                                        {
                                            string fnNoExt = Path.GetFileNameWithoutExtension(f);
                                            if (fnNoExt.IsWhiteSpace()) continue;
                                            if (archivesHash.Add(fnNoExt))
                                            {
                                                archives[RemoveDLArchiveBadChars(fnNoExt, badChars)] = fnNoExt;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log(ErrorText.Ex + "in DarkLoader archive dir file enumeration", ex);
                                    }
                                }
                                else if (lt.IsIniHeader())
                                {
                                    break;
                                }
                                i++;
                            }
                            break;
                        }
                    }

                    if (archives.Count == 0)
                    {
                        return ImportError.NoArchiveDirsFound;
                    }

                    #endregion

                    #region Read FM entries

                    for (int i = 0; i < lines.Count; i++)
                    {
                        string lineT = lines[i].Trim();

                        if (!nonFMHeaders.Contains(lineT) &&
                            lineT.Length > 0 &&
                            lineT[0] == '[' &&
                            lineT[lineT.Length - 1] == ']' &&
                            lineT.Contains('.') &&
                            darkLoaderFMRegex.Match(lineT).Success)
                        {
                            int lastIndexDot = lineT.LastIndexOf('.');
                            string dlArchive = lineT.Substring(1, lastIndexDot - 1);
                            string size = lineT.Substring(lastIndexDot + 1, lineT.Length - lastIndexDot - 2);

                            if (!archives.TryGetValue(dlArchive, out string realArchive))
                            {
                                continue;
                            }

                            realArchive += ".zip";

                            ulong.TryParse(size, out ulong sizeBytes);
                            var fm = new FanMission
                            {
                                Archive = realArchive,
                                InstalledDir = realArchive.ToInstDirNameFMSel(instDirNameContext, true),
                                SizeBytes = sizeBytes
                            };

                            // We don't import game type, because DarkLoader by default gets it wrong for
                            // NewDark FMs (the user could have changed it manually in the ini file, and in
                            // fact it's somewhat likely they would have done so, but still, better to just
                            // scan for it ourselves later)

                            while (i < lines.Count - 1)
                            {
                                string lts = lines[i + 1].TrimStart();
                                string ltb = lts.TrimEnd();

                                if (lts.StartsWithO("comment=\""))
                                {
                                    string comment = ltb.Substring(9);
                                    if (comment.Length >= 2 && comment[comment.Length - 1] == '\"')
                                    {
                                        comment = comment.Substring(0, comment.Length - 1);
                                        fm.Comment = DLUnescapeChars(comment);
                                    }
                                }
                                else if (lts.StartsWithO("title=\""))
                                {
                                    string title = ltb.Substring(7);
                                    if (title.Length >= 2 && title[title.Length - 1] == '\"')
                                    {
                                        title = title.Substring(0, title.Length - 1);
                                        fm.Title = DLUnescapeChars(title);
                                    }
                                }
                                else if (lts.StartsWithO("misdate="))
                                {
                                    ulong.TryParse(ltb.Substring(8), out ulong result);
                                    try
                                    {
                                        var date = new DateTime(1899, 12, 30).AddDays(result);
                                        fm.ReleaseDate.DateTime = date.Year > 1998 ? date : null;
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        fm.ReleaseDate.DateTime = null;
                                    }
                                }
                                else if (lts.StartsWithO("date="))
                                {
                                    ulong.TryParse(ltb.Substring(5), out ulong result);
                                    try
                                    {
                                        var date = new DateTime(1899, 12, 30).AddDays(result);
                                        fm.LastPlayed.DateTime = date.Year > 1998 ? date : null;
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        fm.LastPlayed.DateTime = null;
                                    }
                                }
                                else if (lts.StartsWithO("finished="))
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
                    }

                    #endregion
                }

                if (importSaves)
                {
                    try
                    {
                        ImportDarkLoaderSaves(lines);
                    }
                    catch
                    {
                        // ignore - keeping same behavior
                    }
                }

                return ImportError.None;
            }
            catch (Exception ex)
            {
                Log(ex: ex);
                return ImportError.Unknown;
            }
            finally
            {
                // We don't really need this here; we already close later
                // This just lets us close before putting up a possible error dialog, but other than that is
                // unnecessary.
                Core.View.HideProgressBox();
            }
        }

        ImportError error = DoImport();

        if (error != ImportError.None) return (error, fms);

        List<FanMission> importedFMs = MergeImportedFMData(ImportType.DarkLoader, fms, fields);

        return (ImportError.None, importedFMs);
    }

    private static bool
    ImportDarkLoaderSaves(List<string> lines)
    {
        // We DON'T use game generalization here, because DarkLoader only supports T1/T2/SS2 and will never
        // change (it's not updated anymore). So it's okay that we code those games in manually here.

        string t1Dir = "";
        string t2Dir = "";
        string ss2Dir = "";
        bool t1DirRead = false;
        bool t2DirRead = false;
        bool ss2DirRead = false;

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            string lineTS = line.TrimStart();
            string lineTB = lineTS.TrimEnd();

            if (lineTB == "[options]")
            {
                while (i < lines.Count - 1)
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

        for (int i = 0; i < 3; i++)
        {
            if (i == 0 && t1Dir.IsEmpty()) continue;
            if (i == 1 && t2Dir.IsEmpty()) continue;
            if (i == 2 && ss2Dir.IsEmpty()) continue;

            string savesPath = Path.Combine(i switch { 0 => t1Dir, 1 => t2Dir, _ => ss2Dir }, "allsaves");
            if (!Directory.Exists(savesPath)) continue;

            Directory.CreateDirectory(Config.DarkLoaderBackupPath);

            // Converting takes too long, so just copy them to our backup folder and they'll be handled
            // appropriately next time the user installs an FM
            foreach (string f in FastIO.GetFilesTopOnly(savesPath, "*.zip"))
            {
                string dest = Path.Combine(Config.DarkLoaderBackupPath, f.GetFileNameFast());
                File.Copy(f, dest, overwrite: true);
            }
        }

        return true;
    }

    private static (ImportError Error, List<FanMission> FMs)
    ImportFMSelInternal(string iniFile, FieldsToImport fields)
    {
        string[] lines = File.ReadAllLines(iniFile);
        var fms = new List<FanMission>();

        static void DoImport(string[] lines, List<FanMission> fms)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Length >= 5 && line[0] == '[' && line[1] == 'F' && line[2] == 'M' && line[3] == '=')
                {
                    string instName = line.Substring(4, line.Length - 5);

                    var fm = new FanMission { InstalledDir = instName };

                    while (i < lines.Length - 1)
                    {
                        // @Import: FMSel: We're not trimming these lines at all. Is this to spec?
                        string lineFM = lines[i + 1];
                        if (lineFM.StartsWithFast("NiceName="))
                        {
                            fm.Title = lineFM.Substring(9);
                        }
                        /*
                        @Import/BUG: This field can have a leading subfolder!!! like "import_test\1999-06-04_PoorLordBafford.zip"
                        Argh! This completely wrecks everything! I think it searches subfolders always?
                        I thought only NDL had that functionality... Yikes... and FMSel even distinguishes
                        archives in different folders with a subdir prefix and a bracketed number after the
                        install dir name and all.
                        */
                        else if (lineFM.StartsWithFast("Archive="))
                        {
                            fm.Archive = lineFM.Substring(8);
                        }
                        else if (lineFM.StartsWithFast("ReleaseDate="))
                        {
                            fm.ReleaseDate.UnixDateString = lineFM.Substring(12);
                        }
                        else if (lineFM.StartsWithFast("LastStarted="))
                        {
                            fm.LastPlayed.UnixDateString = lineFM.Substring(12);
                        }
                        else if (lineFM.StartsWithFast("Completed="))
                        {
                            int.TryParse(lineFM.Substring(10), out int result);
                            // Unfortunately FMSel doesn't let you choose the difficulty you finished on, so
                            // we have to have this fallback value as a best-effort thing.
                            if (result > 0) fm.FinishedOnUnknown = true;
                        }
                        else if (lineFM.StartsWithFast("Rating="))
                        {
                            fm.Rating = int.TryParse(lineFM.Substring(7), out int result) ? result : -1;
                        }
                        else if (lineFM.StartsWithFast("Notes="))
                        {
                            fm.Comment = lineFM.Substring(6)
                                .Replace(@"\n", @"\r\n")
                                .Replace(@"\t", "\t")
                                .Replace(@"\""", "\"")
                                .Replace(@"\\", "\\");
                        }
                        else if (lineFM.StartsWithFast("ModExclude="))
                        {
                            fm.DisabledMods = lineFM.Substring(11);
                        }
                        else if (lineFM.StartsWithFast("Tags="))
                        {
                            fm.TagsString = lineFM.Substring(5);
                        }
                        else if (lineFM.StartsWithFast("InfoFile="))
                        {
                            fm.SelectedReadme = lineFM.Substring(9);
                        }
                        else if (!lineFM.IsEmpty() && lineFM[0] == '[' && lineFM[lineFM.Length - 1] == ']')
                        {
                            break;
                        }
                        i++;
                    }

                    fms.Add(fm);
                }
            }
        }

        DoImport(lines, fms);

        List<FanMission> importedFMs = MergeImportedFMData(ImportType.FMSel, fms, fields);

        return (ImportError.None, importedFMs);
    }

    private static (ImportError Error, List<FanMission> FMs)
    ImportNDLInternal(string iniFile, FieldsToImport fields, InstDirNameContext instDirNameContext)
    {
        // NewDarkLoader uses Encoding.Default for NewDarkLoader.ini, confirmed from source and testing 1.7.0
        List<string> lines = File_ReadAllLines_List(iniFile, Encoding.Default, true);
        var fms = new List<FanMission>();

        static void TryAddToArchivesHash(string dir, DictionaryI<FileInfo> archivesDict)
        {
            try
            {
                // NDL always searches subdirectories as well
                foreach (FileInfo fi in new DirectoryInfo(dir).GetFiles("*", SearchOption.AllDirectories))
                {
                    string f = fi.FullName;

                    // @DIRSEP: '/' conversion due to string.ContainsI()
                    if (!f.ToForwardSlashes_Net().ContainsI("/.fix/"))
                    {
                        string fn = Path.GetFileName(f);
                        if (!fn.IsWhiteSpace() &&
                            fn.ExtIsArchive() &&
                            !fn.ContainsI(Paths.FMSelBak))
                        {
                            archivesDict[fn] = fi;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ErrorText.Ex + "in NewDarkLoader archive dir file enumeration", ex);
            }
        }

        // @Import(NDL): Test!
        static ImportError DoImport(List<string> lines, List<FanMission> fms, InstDirNameContext instDirNameContext)
        {
            DictionaryI<FileInfo> archivesDict = new();

            #region Read archive directory

            // Unfortunately NDL doesn't store its archive names, so we have to do a file search
            // similar to DarkLoader

            for (int i = 0; i < lines.Count; i++)
            {
                // @Import: NDL: We're not trimming these lines at all. Is this to spec?
                string line = lines[i];

                if (line == "[Config]")
                {
                    string archiveRoot = "";

                    bool archiveRootFound = false;
                    bool additionalArchiveRootsFound = false;

                    while (i < lines.Count - 1)
                    {
                        string lc = lines[i + 1];
                        if (lc.StartsWithFast("ArchiveRoot="))
                        {
                            archiveRootFound = true;
                            string dir = lc.Substring(lc.IndexOf('=') + 1).Trim();
                            if (dir.IsWhiteSpace() || !Directory.Exists(dir)) continue;
                            archiveRoot = dir;
                            TryAddToArchivesHash(dir, archivesDict);
                        }
                        // @Import(NDL): Test additional archive roots
                        else if (lc.StartsWithFast("AdditionalArchiveRoots="))
                        {
                            additionalArchiveRootsFound = true;
                            string val = lc.Substring(lc.IndexOf('=') + 1).Trim();
                            string[] dirs = val.Split(CA_Semicolon, StringSplitOptions.RemoveEmptyEntries);
                            for (int dirI = 0; dirI < dirs.Length; dirI++)
                            {
                                string dir = dirs[dirI].Trim();
                                if (!archiveRoot.IsEmpty() && dir.PathEqualsI(archiveRoot))
                                {
                                    continue;
                                }
                                if (dir.IsWhiteSpace() || !Directory.Exists(dir))
                                {
                                    continue;
                                }
                                TryAddToArchivesHash(dir, archivesDict);
                            }
                        }
                        else if (!lc.IsEmpty() && lc[0] == '[' && lc[lc.Length - 1] == ']')
                        {
                            break;
                        }
                        if (archiveRootFound && additionalArchiveRootsFound)
                        {
                            break;
                        }
                        i++;
                    }
                }
            }

            #endregion

            if (archivesDict.Count == 0) return ImportError.NoArchiveDirsFound;

            List<FileInfo> archivesList = new(archivesDict.Count);
            foreach (var item in archivesDict)
            {
                archivesList.Add(item.Value);
            }
            DictionaryI<FanMission> fmsInstalledDirDict = new();

            #region Read FM entries (initial)

            for (int i = 0; i < lines.Count; i++)
            {
                // @Import: NDL: We're not trimming these lines at all. Is this to spec?
                string line = lines[i];

                if (line.Length >= 5 && line.StartsWithFast("[FM="))
                {
                    // @Import: There can be a problem like:
                    // installed name is CoolMission[1]
                    // it gets written like [FM=CoolMission[1]]
                    // it gets read and all [ and ] chars are removed
                    // it gets written back out like [FM=CoolMission1]
                    // Rare I guess, so just ignore?
                    string instName = line.Substring(4, line.Length - 5);

                    var fm = new FanMission { InstalledDir = instName };
                    fmsInstalledDirDict[instName] = fm;

                    while (i < lines.Count - 1)
                    {
                        // @Import: NDL: We're not trimming these lines at all. Is this to spec?
                        string lineFM = lines[i + 1];
                        if (lineFM.StartsWithFast("NiceName="))
                        {
                            fm.Title = lineFM.Substring(9);
                        }
                        else if (lineFM.StartsWithFast("ReleaseDate="))
                        {
                            fm.ReleaseDate.UnixDateString = lineFM.Substring(12);
                        }
                        else if (lineFM.StartsWithFast("LastCompleted="))
                        {
                            fm.LastPlayed.UnixDateString = lineFM.Substring(14);
                        }
                        else if (lineFM.StartsWithFast("Finished="))
                        {
                            uint.TryParse(lineFM.Substring(9), out uint result);
                            // result will be 0 on fail, which is the empty value so it's fine
                            fm.FinishedOn = result;
                        }
                        else if (lineFM.StartsWithFast("Rating="))
                        {
                            fm.Rating = int.TryParse(lineFM.Substring(7), out int result) ? result : -1;
                        }
                        else if (lineFM.StartsWithFast("Comment="))
                        {
                            fm.Comment = lineFM.Substring(8);
                        }
                        else if (lineFM.StartsWithFast("ModExclude="))
                        {
                            fm.DisabledMods = lineFM.Substring(11);
                        }
                        else if (lineFM.StartsWithFast("Tags="))
                        {
                            string val = lineFM.Substring(5);
                            if (!val.IsEmpty() && val != "[none]") fm.TagsString = val;
                        }
                        else if (lineFM.StartsWithFast("InfoFile="))
                        {
                            fm.SelectedReadme = lineFM.Substring(9);
                        }
                        else if (lineFM.StartsWithFast("FMSize="))
                        {
                            ulong.TryParse(lineFM.Substring(7), out ulong result);
                            fm.SizeBytes = result;
                        }
                        else if (!lineFM.IsEmpty() && lineFM[0] == '[' && lineFM[lineFM.Length - 1] == ']')
                        {
                            break;
                        }
                        i++;
                    }

                    fms.Add(fm);
                }
            }

            #endregion

            #region Set FM archive fields

            static bool SizesMatch(FileInfo archiveFI, FanMission fm)
            {
                return archiveFI.Length >= 0 && fm.SizeBytes == (ulong)archiveFI.Length;
            }

            static string RemoveBrackets(string str) => str.Replace("[", "").Replace("]", "");

            for (int i = 0; i < archivesList.Count; i++)
            {
                // @Import(NDL set archive fields): Try really hard to make sure we get an archive for each FM

                FileInfo archiveFI = archivesList[i];
                string archive = archiveFI.Name;

                // NewDarkLoader removes square brackets [] on ini read, but still writes them out, so it's possible
                // for them to be in there, so we need to check both cases.
                if ((fmsInstalledDirDict.TryGetValue(archive.ToInstDirNameNDL(instDirNameContext, truncate: true), out FanMission fm) && SizesMatch(archiveFI, fm)) ||
                    (fmsInstalledDirDict.TryGetValue(RemoveBrackets(archive.ToInstDirNameNDL(instDirNameContext, truncate: true)), out fm) && SizesMatch(archiveFI, fm)) ||

                    (fmsInstalledDirDict.TryGetValue(archive.ToInstDirNameNDL(instDirNameContext, truncate: false), out fm) && SizesMatch(archiveFI, fm)) ||
                    (fmsInstalledDirDict.TryGetValue(RemoveBrackets(archive.ToInstDirNameNDL(instDirNameContext, truncate: false)), out fm) && SizesMatch(archiveFI, fm)) ||

                    (fmsInstalledDirDict.TryGetValue(archive.ToInstDirNameFMSel(instDirNameContext, truncate: true), out fm) && SizesMatch(archiveFI, fm)) ||
                    (fmsInstalledDirDict.TryGetValue(RemoveBrackets(archive.ToInstDirNameFMSel(instDirNameContext, truncate: true)), out fm) && SizesMatch(archiveFI, fm)) ||

                    (fmsInstalledDirDict.TryGetValue(archive.ToInstDirNameFMSel(instDirNameContext, truncate: false), out fm) && SizesMatch(archiveFI, fm)) ||
                    (fmsInstalledDirDict.TryGetValue(RemoveBrackets(archive.ToInstDirNameFMSel(instDirNameContext, truncate: false)), out fm) && SizesMatch(archiveFI, fm)))
                {
                    fm.Archive = archive;
                }
            }

            #endregion

            return ImportError.None;
        }

        ImportError error = DoImport(lines, fms, instDirNameContext);

        if (error != ImportError.None) return (error, fms);

        List<FanMission> importedFMs = MergeImportedFMData_NDL(fms, fields);

        return (ImportError.None, importedFMs);
    }

    private static List<FanMission> MergeImportedFMData_NDL(List<FanMission> importedFMs, FieldsToImport fields)
    {
        var importedFMsInMainList = new List<FanMission>();

        DictionaryI<FanMission> archivesDict = new();
        DictionaryI<FanMission> instDirsDict = new();
        foreach (FanMission fm in FMDataIniList)
        {
            if (!fm.Archive.IsEmpty() && !archivesDict.ContainsKey(fm.Archive))
            {
                archivesDict[fm.Archive] = fm;
            }
            if (!instDirsDict.ContainsKey(fm.InstalledDir))
            {
                instDirsDict[fm.InstalledDir] = fm;
            }
        }

        foreach (FanMission importedFM in importedFMs)
        {
            if (archivesDict.TryGetValue(importedFM.Archive, out FanMission? mainFM) ||
                instDirsDict.TryGetValue(importedFM.InstalledDir, out mainFM))
            {
                if (fields.Title && !importedFM.Title.IsEmpty())
                {
                    mainFM.Title = importedFM.Title;
                }
                if (fields.ReleaseDate && importedFM.ReleaseDate.DateTime != null)
                {
                    mainFM.ReleaseDate.DateTime = importedFM.ReleaseDate.DateTime;
                }
                if (fields.LastPlayed)
                {
                    mainFM.LastPlayed.DateTime = importedFM.LastPlayed.DateTime;
                }
                if (fields.FinishedOn)
                {
                    mainFM.FinishedOn = importedFM.FinishedOn;
                    mainFM.FinishedOnUnknown = false;
                }
                if (fields.Comment)
                {
                    mainFM.Comment = importedFM.Comment;
                }
                if (fields.Rating)
                {
                    mainFM.Rating = importedFM.Rating;
                }
                if (fields.DisabledMods)
                {
                    mainFM.DisabledMods = importedFM.DisabledMods;
                    mainFM.DisableAllMods = mainFM.DisabledMods == "*";
                }
                if (fields.Tags)
                {
                    mainFM.TagsString = importedFM.TagsString;
                }
                if (fields.SelectedReadme)
                {
                    mainFM.SelectedReadme = importedFM.SelectedReadme;
                }
                if (fields.Size && mainFM.SizeBytes == 0)
                {
                    mainFM.SizeBytes = importedFM.SizeBytes;
                }

                mainFM.MarkedScanned = true;

                importedFMsInMainList.Add(mainFM);
            }
            else
            {
                var newFM = new FanMission
                {
                    Archive = importedFM.Archive,
                    InstalledDir = importedFM.InstalledDir
                };

                if (fields.Title)
                {
                    newFM.Title = !importedFM.Title.IsEmpty() ? importedFM.Title :
                        !importedFM.Archive.IsEmpty() ? importedFM.Archive.RemoveExtension() :
                        importedFM.InstalledDir;
                }
                if (fields.ReleaseDate)
                {
                    newFM.ReleaseDate.DateTime = importedFM.ReleaseDate.DateTime;
                }
                if (fields.LastPlayed)
                {
                    newFM.LastPlayed.DateTime = importedFM.LastPlayed.DateTime;
                }
                if (fields.Comment)
                {
                    newFM.Comment = importedFM.Comment;
                }
                if (fields.Rating)
                {
                    newFM.Rating = importedFM.Rating;
                }
                if (fields.DisabledMods)
                {
                    newFM.DisabledMods = importedFM.DisabledMods;
                    newFM.DisableAllMods = newFM.DisabledMods == "*";
                }
                if (fields.Tags)
                {
                    newFM.TagsString = importedFM.TagsString;
                }
                if (fields.SelectedReadme)
                {
                    newFM.SelectedReadme = importedFM.SelectedReadme;
                }
                if (fields.Size)
                {
                    newFM.SizeBytes = importedFM.SizeBytes;
                }
                if (fields.FinishedOn)
                {
                    newFM.FinishedOn = importedFM.FinishedOn;
                }

                newFM.MarkedScanned = true;

                FMDataIniList.Add(newFM);
                importedFMsInMainList.Add(newFM);
            }
        }

        return importedFMsInMainList;
    }

    private static List<FanMission>
    MergeImportedFMData(ImportType importType, List<FanMission> importedFMs, FieldsToImport fields)
    {
        // Perf
        int initCount = FMDataIniList.Count;
        bool[] checkedArray = new bool[initCount];

        // We can't just send back the list we got in, because we will have deep-copied them to the main list
        var importedFMsInMainList = new List<FanMission>();

        for (int impFMi = 0; impFMi < importedFMs.Count; impFMi++)
        {
            FanMission importedFM = importedFMs[impFMi];

            bool existingFound = false;
            for (int mainFMi = 0; mainFMi < initCount; mainFMi++)
            {
                FanMission mainFM = FMDataIniList[mainFMi];

                if (!checkedArray[mainFMi] &&
                    // @Import: Import match-up
                    // We should match installed dirs better! Match to FMSel/NDL _-replace style, truncated
                    // and not, etc... even bracket-numbered? D:
                    ((importType == ImportType.DarkLoader &&
                      mainFM.Archive.EqualsI(importedFM.Archive)) ||
                     (importType == ImportType.FMSel &&
                      ((!importedFM.Archive.IsEmpty() && importedFM.Archive.EqualsI(mainFM.Archive)) ||
                       importedFM.InstalledDir.EqualsI(mainFM.InstalledDir))) ||
                     (importType == ImportType.NewDarkLoader &&
                      ((!importedFM.Archive.IsEmpty() && importedFM.Archive.EqualsI(mainFM.Archive)) ||
                        importedFM.InstalledDir.EqualsI(mainFM.InstalledDir)
                      )
                     )
                    )
                   )
                {
                    if (fields.Title && !importedFM.Title.IsEmpty())
                    {
                        mainFM.Title = importedFM.Title;
                    }
                    if (fields.ReleaseDate && importedFM.ReleaseDate.DateTime != null)
                    {
                        mainFM.ReleaseDate.DateTime = importedFM.ReleaseDate.DateTime;
                    }
                    if (fields.LastPlayed)
                    {
                        mainFM.LastPlayed.DateTime = importedFM.LastPlayed.DateTime;
                    }
                    if (fields.FinishedOn)
                    {
                        mainFM.FinishedOn = importedFM.FinishedOn;
                        if (importType != ImportType.FMSel)
                        {
                            mainFM.FinishedOnUnknown = false;
                        }
                    }
                    if (fields.Comment)
                    {
                        mainFM.Comment = importedFM.Comment;
                    }

                    if (importType is ImportType.NewDarkLoader or ImportType.FMSel)
                    {
                        if (fields.Rating)
                        {
                            mainFM.Rating = importedFM.Rating;
                        }
                        if (fields.DisabledMods)
                        {
                            mainFM.DisabledMods = importedFM.DisabledMods;
                            mainFM.DisableAllMods = mainFM.DisabledMods == "*";
                        }
                        if (fields.Tags)
                        {
                            mainFM.TagsString = importedFM.TagsString;
                        }
                        if (fields.SelectedReadme)
                        {
                            mainFM.SelectedReadme = importedFM.SelectedReadme;
                        }
                    }
                    if (importType is ImportType.NewDarkLoader or ImportType.DarkLoader)
                    {
                        if (fields.Size && mainFM.SizeBytes == 0)
                        {
                            mainFM.SizeBytes = importedFM.SizeBytes;
                        }
                    }
                    else if (importType == ImportType.FMSel && mainFM.FinishedOn == 0 && !mainFM.FinishedOnUnknown)
                    {
                        if (fields.FinishedOn)
                        {
                            mainFM.FinishedOnUnknown = importedFM.FinishedOnUnknown;
                        }
                    }

                    mainFM.MarkedScanned = true;

                    checkedArray[mainFMi] = true;

                    importedFMsInMainList.Add(mainFM);

                    existingFound = true;
                    break;
                }
            }
            if (!existingFound)
            {
                var newFM = new FanMission
                {
                    Archive = importedFM.Archive,
                    InstalledDir = importedFM.InstalledDir
                };

                if (fields.Title)
                {
                    newFM.Title = !importedFM.Title.IsEmpty() ? importedFM.Title :
                        !importedFM.Archive.IsEmpty() ? importedFM.Archive.RemoveExtension() :
                        importedFM.InstalledDir;
                }
                if (fields.ReleaseDate)
                {
                    newFM.ReleaseDate.DateTime = importedFM.ReleaseDate.DateTime;
                }
                if (fields.LastPlayed)
                {
                    newFM.LastPlayed.DateTime = importedFM.LastPlayed.DateTime;
                }
                if (fields.Comment)
                {
                    newFM.Comment = importedFM.Comment;
                }

                if (importType is ImportType.NewDarkLoader or ImportType.FMSel)
                {
                    if (fields.Rating)
                    {
                        newFM.Rating = importedFM.Rating;
                    }
                    if (fields.DisabledMods)
                    {
                        newFM.DisabledMods = importedFM.DisabledMods;
                        newFM.DisableAllMods = newFM.DisabledMods == "*";
                    }
                    if (fields.Tags)
                    {
                        newFM.TagsString = importedFM.TagsString;
                    }
                    if (fields.SelectedReadme)
                    {
                        newFM.SelectedReadme = importedFM.SelectedReadme;
                    }
                }
                if (importType is ImportType.NewDarkLoader or ImportType.DarkLoader)
                {
                    if (fields.Size)
                    {
                        newFM.SizeBytes = importedFM.SizeBytes;
                    }
                    if (fields.FinishedOn)
                    {
                        newFM.FinishedOn = importedFM.FinishedOn;
                    }
                }
                else if (importType == ImportType.FMSel)
                {
                    if (fields.FinishedOn)
                    {
                        newFM.FinishedOnUnknown = importedFM.FinishedOnUnknown;
                    }
                }

                newFM.MarkedScanned = true;

                FMDataIniList.Add(newFM);
                importedFMsInMainList.Add(newFM);
            }
        }

        return importedFMsInMainList;
    }

    #endregion
}
