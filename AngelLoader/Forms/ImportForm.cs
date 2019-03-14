using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.Forms
{
    public partial class ImportForm : Form
    {
        private readonly string[] NonFMHeaders =
        {
            "[options]",
            "[window]",
            "[mission directories]",
            "[Thief 1]",
            "[Thief 2]",
            "[Thief2x]",
            "[SShock 2]"
        };

        private enum DLGame
        {
            darkGameUnknown = 0, // <- if it hasn't been scanned, it will be this
            darkGameThief = 1,
            darkGameThief2 = 2,
            darkGameT2x = 3,
            darkGameSS2 = 4
        }

        //private static readonly Regex DarkLoaderFMRegex = new Regex(@"\.\d+]$", RegexOptions.Compiled);
        private static readonly Regex DarkLoaderFMRegex = new Regex(@"\.[0123456789]+]$", RegexOptions.Compiled);

        public ImportForm()
        {
            InitializeComponent();
        }

        private static string RemoveDLArchiveBadChars(string archive)
        {
            foreach (string s in new[] { "]", "\u0009", "\u000A", "\u000D" }) archive = archive.Replace(s, "");
            return archive;
        }

        // Don't replace \r\n or \\ escapes because we use those in the exact same way so no conversion needed
        private static string DLUnescapeChars(string str) => str.Replace(@"\t", "\u0009").Replace(@"\""", "\"");

        // TODO: Also import DarkLoader's saves
        // TODO: Consider importing DL's stats (textures, objects, etc.)
        // But then DL and AngelLoader each have a couple stats the other doesn't, so I'd have to allow unknowns
        // or else I'd have to set another bit that tells us to scan the mission for stats-only upon selection.
        // Or just scan the missions right here after importing, that way I can also get a guaranteed correct
        // game type, as noted below.
        private void DarkLoaderButton_Click(object sender, EventArgs e)
        {
            string file;
            using (var d = new OpenFileDialog())
            {
                d.Filter = LText.BrowseDialogs.IniFiles + @"|*.ini|" + LText.BrowseDialogs.AllFiles + @"|*.*";
                if (d.ShowDialog() != DialogResult.OK) return;

                file = d.FileName;
            }

            if (!Path.GetFileName(file).EqualsI("DarkLoader.ini"))
            {
                // TODO: do something nicer here
                MessageBox.Show("Selected file is not DarkLoader.ini.");
                return;
            }

            var lines = File.ReadAllLines(file);

            var archiveDirs = new List<string>();
            var fms = new List<FanMission>();

            bool missionDirsRead = false;
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var lineTS = line.TrimStart();
                var lineTB = lineTS.TrimEnd();

                #region Read archive directories

                // We need to know the archive dirs before doing anything, because we may need to recreate some
                // lossy names (if any bad chars have been removed by DarkLoader).
                if (!missionDirsRead)
                {
                    if (lineTB != "[mission directories]") continue;

                    while (i < lines.Length - 1)
                    {
                        var lt = lines[i + 1].Trim();
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
                    // Restart from the beginning of the file, this time skipping anything that isn't an FM entry
                    i = -1;
                    missionDirsRead = true;
                    continue;
                }

                #endregion

                #region Read FM entries

                if (!NonFMHeaders.Contains(lineTB) && lineTB.Length > 0 && lineTB[0] == '[' &&
                         lineTB[lineTB.Length - 1] == ']' && lineTB.Contains('.') &&
                         DarkLoaderFMRegex.Match(lineTB).Success)
                {
                    var lastIndexDot = lineTB.LastIndexOf('.');
                    var archive = lineTB.Substring(1, lastIndexDot - 1);
                    var size = lineTB.Substring(lastIndexDot + 1, lineTB.Length - lastIndexDot - 2);

                    foreach (var dir in archiveDirs)
                    {
                        if (!Directory.Exists(dir)) continue;
                        try
                        {
                            // DarkLoader only does zip format
                            foreach (var f in Directory.EnumerateFiles(dir, "*.zip", SearchOption.TopDirectoryOnly))
                            {
                                var fn = Path.GetFileNameWithoutExtension(f);
                                if (RemoveDLArchiveBadChars(fn).EqualsI(archive))
                                {
                                    archive = fn;
                                    goto breakout;
                                }
                            }
                        }
                        catch
                        {
                            // log it here
                        }
                    }

                    breakout:

                    ulong.TryParse(size, out ulong sizeBytes);
                    var fm = new FanMission
                    {
                        Archive = archive,
                        InstalledDir = archive.ToInstalledFMDirNameFMSel(),
                        SizeBytes = sizeBytes
                    };

                    while (i < lines.Length - 1)
                    {
                        var lts = lines[i + 1].TrimStart();
                        var ltb = lts.TrimEnd();

                        if (lts.StartsWith("type="))
                        {
                            int.TryParse(lts.Substring(lts.IndexOf('=') + 1), out int dlGame);
                            fm.Game =
                                dlGame == (int)DLGame.darkGameUnknown ? (Game?)null :
                                dlGame == (int)DLGame.darkGameThief ? Game.Thief1 :
                                dlGame == (int)DLGame.darkGameThief2 ? Game.Thief2 :
                                Game.Unsupported;
                        }
                        else if (lts.StartsWith("comment=\""))
                        {
                            var comment = ltb.Substring(ltb.IndexOf('=') + 1);
                            if (comment.Length > 1 && comment[0] == '\"' && comment[comment.Length - 1] == '\"')
                            {
                                comment = comment.Substring(1, comment.Length - 2);
                                fm.Comment = DLUnescapeChars(comment);
                            }
                        }
                        else if (lts.StartsWith("title=\""))
                        {
                            var title = ltb.Substring(ltb.IndexOf('=') + 1);
                            if (title.Length > 1 && title[0] == '\"' && title[title.Length - 1] == '\"')
                            {
                                title = title.Substring(1, title.Length - 2);
                                fm.Title = DLUnescapeChars(title);
                            }
                        }
                        else if (lts.StartsWith("misdate="))
                        {
                            ulong.TryParse(ltb.Substring(ltb.IndexOf('=') + 1), out ulong result);
                            try
                            {
                                var date = new DateTime(1899, 12, 30).AddDays(result);
                                fm.ReleaseDate = date.Year > 1998 ? date : (DateTime?)null;
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                fm.ReleaseDate = null;
                            }
                        }
                        else if (lts.StartsWith("date="))
                        {
                            ulong.TryParse(ltb.Substring(ltb.IndexOf('=') + 1), out ulong result);
                            try
                            {
                                var date = new DateTime(1899, 12, 30).AddDays(result);
                                fm.LastPlayed = date.Year > 1998 ? date : (DateTime?)null;
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                fm.LastPlayed = null;
                            }
                        }
                        else if (lts.StartsWith("finished="))
                        {
                            var success = int.TryParse(ltb.Substring(ltb.IndexOf('=') + 1), out int result);
                            if (success) fm.FinishedOn = result;
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

            Ini.Ini.WriteFMDataIni(fms, @"C:\DarkLoader_import_test.ini");
        }

        /* DarkLoader:

        Saves:
        -When you uninstall a mission, it puts the saves in [GameExePath]\allsaves
            ex. C:\Thief2\allsaves
        -Saves are put into a zip, in its base directory (no "saves" folder within)
        -The zip is named [archive]_saves.zip
            ex. 2002-02-19_Justforshow_saves.zip
        -This name is NOT run through the badchar remover, but it does appear to have whitespace trimmed off both
         ends.

        ---

        Non-FM headers:
        [options]
        [window]
        [mission directories]
        [Thief 1]
        [Thief 2]
        [Thief2x]
        [SShock 2]
        (or anything that doesn't have a .number at the end)
         
        FM headers look like this:
        
        [1999-06-11_DeceptiveSceptre,The.538256]
        
        First an opening bracket ('[') then the archive name minus the extension (which is always '.zip'), in
        full (not truncated), with the following characters removed:
        ], Chr(9) (TAB), Chr(10) (LF), Chr(13) (CR)
         
        or, put another way:
        badchars=[']',#9,#10,#13];

        Then comes a dot (.) followed by the size, in bytes, of the archive, then a closing bracket (']').

        FM key-value pairs:
        type=(int)
        
        Will be one of these values (numeric, not named):
        darkGameUnknown = 0; <- if it hasn't been scanned, it will be this
        darkGameThief   = 1;
        darkGameThief2  = 2;
        darkGameT2x     = 3;
        darkGameSS2     = 4;

        but we should ignore this and scan for the type ourselves, because:
        a) DarkLoader gets the type wrong with NewDark (marks Thief1 as Thief2), and
        b) we don't want to pollute our own list with archive types we don't support (T2x, SS2)

        comment=(string)
        Looks like this:
        comment="This is a comment"
        The string is always surrounded with double-quotes (").
        Escapes are handled like this:
        #9  -> \t
        #10 -> \n
        #13 -> \r
        "   -> \"
        \   -> \\

        title=(string)
        Handled the exact same way as comment= above.

        misdate=(int)
        Mission release date in number of days since December 30, 1899.

        date=(int)
        Last played date in number of days since December 30, 1899.

        finished=(int)
        A 4-bit flags field, exactly the same as AngelLoader uses, so no conversion needed at all

        */

        /*
        NewDarkLoader:

        -Allow the user to select multiple NDL installs (because one is needed for each game)
        -We can take the ArchiveRoot path (below) and add it to our archive paths

        -NOTE: Make sure to handle NDL's subfolder functionality - look into how it works!

        -Unused keys:
        -------------
        const string kGame_type = "Type";
        const string kArchive_name = "Archive";
        const string kWindowPos = "WindowPos";
        -------------

        Config section:
        ---------------
        const string secOptions = "Config";
        
        -Supported archive extensions. Default zip,7z,rar. Ignore this, we have our own internal list.
        const string kExtensions = "ValidExtensions";

        -Ignore, we don't use this
        const string kUseRelativePaths = "UseRelativePaths";

        -Archive path (will be only one per install)
        const string kArchive_root = "ArchiveRoot";

        const string kLanguage = "Language";

        -int with two possible values
         1 = dd/MM/yyyy
         2 = MM/dd/yyyy
        const string kDate_format = "DateFormat";
        
        -Don't ask for confirmation to play when you double-click an FM
        const string kAlwaysPlay = "DbClDontAsk";

        -The value "Ask" or "Always"
        const string kBackup_type = "BackupType";

        -One of these will be written out. This is "run NDL after game/editor" so we don't need it anyway.
        const string kReturn_type = "DebriefFM";
        const string kReturn_type_ed = "DebriefFMEd";

        -We use internal 7z stuff only, so ignore these
        const string k7zipG = "sevenZipG";
        const string kUse7zNoWin = "Use7zNoWin";

        -This is what goes after google's "site:" bit, it'll be eg. "ttlg.com"
        -UI says enter 0 to disable, check if that gets written out literally to the file
        "WebSearchSite"

        -Space separated - "the a an"
        -UI says enter 0 to disable, check if that gets written out literally to the file
        "ArticleWords"
        "SortIgnoreArticles"

        const string kSplitDist = "SplitDistance";
        const string kCWidths = "ColumnWidths";
        const string kWindowState = "WindowState";

        -Whether the top-right section is expanded or collapsed
        const string kShowTags = "ShowTags";

        const string kSortCol = "SortColumn";
        const string kSortOrder = "SortOrder";
        
        -Last played (not last selected) FM. Ignore it I guess.
        const string kLast_fm = "LastFM";

        -Ignore these, it's easy enough for the user to set them back again
        const string kNameFilter = "FilterName";
        const string kUnfinishedFilter = "FilterUnfinished";
        const string kStartDateFilter = "FilterStart";
        const string kEndDateFilter = "FilterEnd";
        const string kIncTagsFilter = "FilterTagsOR";
        const string kExcTagsFilter = "FilterTagsNOT";
        ---------------

        -FM section:
        ------------
        const string kFm_title = "NiceName";
        
        -Both written out in Unix time hex just like us, so no conversion needed
        -But scan for dates ourselves, and then only replace dates that are invalid (<1999)
        const string kRelease_date = "ReleaseDate";

        -This actually does mean "last played", not "last completed"
        const string kLast_played = "LastCompleted";

        -Same as always, just an int 0-15
        const string kFinished = "Finished";

        -If empty or no key, then None (unrated), otherwise 0-10
        const string kRating = "Rating";

        -Single line. Just read it verbatim.
        const string kComment = "Comment";

        -Disabled mods string. Read it verbatim, but if it's "*" then blank it and set DisableAllMods to true
        const string kNo_mods = "ModExclude";

        -"[none]" means none. Otherwise, a string with the same format as we use.
        const string kTags = "Tags";

        -Selected readme.
        const string kInfoFile = "InfoFile";

        -Size in bytes. Same as our SizeBytes key.
        -Appears to work just like ours; it's the archive size if at all possible, otherwise the folder size.
        const string kSizeBytes = "FMSize";
        ------------
        */
    }
}
