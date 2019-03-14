using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using FMScanner;
using static AngelLoader.Common.Common;

namespace AngelLoader.Forms
{
    public partial class ImportFromDarkLoaderForm : Form
    {
        internal readonly List<FanMission> ImportedFMs = new List<FanMission>();

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

        private static readonly Regex DarkLoaderFMRegex = new Regex(@"\.[0123456789]+]$", RegexOptions.Compiled);

        private readonly BusinessLogic Model;

        internal ImportFromDarkLoaderForm(BusinessLogic model)
        {
            InitializeComponent();
            Model = model;
        }

        private static string RemoveDLArchiveBadChars(string archive)
        {
            foreach (string s in new[] { "]", "\u0009", "\u000A", "\u000D" }) archive = archive.Replace(s, "");
            return archive;
        }

        // Don't replace \r\n or \\ escapes because we use those in the exact same way so no conversion needed
        private static string DLUnescapeChars(string str) => str.Replace(@"\t", "\u0009").Replace(@"\""", "\"");

        // TODO: Consider importing DL's stats (textures, objects, etc.)
        // But then DL and AngelLoader each have a couple stats the other doesn't, so I'd have to allow unknowns
        // or else I'd have to set another bit that tells us to scan the mission for stats-only upon selection.
        // Or just scan the missions right here after importing, that way I can also get a guaranteed correct
        // game type, as noted below.
        private async Task<(bool Success, List<FanMission> FMs)>
        Import()
        {
            var file = DarkLoaderIniTextBox.Text;

            var ret = (false, new List<FanMission>());

            bool fileNameIsDLIni;
            try
            {
                fileNameIsDLIni = Path.GetFileName(file).EqualsI("DarkLoader.ini");
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Selected file is not a valid path.");
                return ret;
            }

            if (!fileNameIsDLIni)
            {
                // TODO: do something nicer here
                MessageBox.Show("Selected file is not DarkLoader.ini.");
                return ret;
            }

            var iniFileExists = await Task.Run(() => File.Exists(file));
            if (!iniFileExists)
            {
                MessageBox.Show("Selected DarkLoader.ini was not found.");
                return ret;
            }

            var lines = await Task.Run(() => File.ReadAllLines(file));

            var fms = new List<FanMission>();

            if (ImportFMDataCheckBox.Checked)
            {
                var archiveDirs = new List<string>();

                bool missionDirsRead = false;

                await Task.Run(() =>
                {
                    for (var i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        var lineTS = line.TrimStart();
                        var lineTB = lineTS.TrimEnd();

                        #region Read archive directories

                        // We need to know the archive dirs before doing anything, because we may need to recreate
                        // some lossy names (if any bad chars have been removed by DarkLoader).
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
                            // Restart from the beginning of the file, this time skipping anything that isn't an FM
                            // entry
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
                                    foreach (var f in Directory.EnumerateFiles(dir, "*.zip",
                                        SearchOption.TopDirectoryOnly))
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

                            // Add .zip back on; required because everything expects it, and furthermore if there's
                            // a dot anywhere in the name then everything after it will be treated as the extension
                            // and is liable to be lopped off at any time
                            archive += ".zip";

                            ulong.TryParse(size, out ulong sizeBytes);
                            var fm = new FanMission
                            {
                                Archive = archive,
                                InstalledDir = archive.ToInstalledFMDirNameFMSel(),
                                SizeBytes = sizeBytes
                            };

                            // We don't import game type, because DarkLoader by default gets it wrong for NewDark
                            // FMs (the user could have changed it manually in the ini file, and in fact it's
                            // somewhat likely they would have done so, but still, better to just scan for it
                            // ourselves later)

                            while (i < lines.Length - 1)
                            {
                                var lts = lines[i + 1].TrimStart();
                                var ltb = lts.TrimEnd();

                                if (lts.StartsWith("comment=\""))
                                {
                                    var comment = ltb.Substring(9);
                                    if (comment.Length >= 2 && comment[comment.Length - 1] == '\"')
                                    {
                                        comment = comment.Substring(0, comment.Length - 1);
                                        fm.Comment = DLUnescapeChars(comment);
                                    }
                                }
                                else if (lts.StartsWith("title=\""))
                                {
                                    var title = ltb.Substring(7);
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
                                        fm.ReleaseDate = date.Year > 1998 ? date : (DateTime?)null;
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        fm.ReleaseDate = null;
                                    }
                                }
                                else if (lts.StartsWith("date="))
                                {
                                    ulong.TryParse(ltb.Substring(5), out ulong result);
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
                                    int.TryParse(ltb.Substring(9), out int result);
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

                    Ini.Ini.WriteFMDataIni(fms, @"C:\DarkLoader_import_test.ini");
                });
            }

            if (ImportSavesCheckBox.Checked)
            {
                bool success = await ImportSaves(lines);
            }

            return (true, fms);
        }

        private static async Task<bool> ImportSaves(string[] lines)
        {
            var t1Dir = "";
            var t2Dir = "";
            var t1DirRead = false;
            var t2DirRead = false;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var lineTS = line.TrimStart();
                var lineTB = lineTS.TrimEnd();

                if (lineTB == "[options]")
                {
                    while (i < lines.Length - 1)
                    {
                        var lt = lines[i + 1].Trim();
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
                        else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }
                        if (t1DirRead && t2DirRead) goto breakout;
                        i++;
                    }
                }
            }

            breakout:

            if (t1Dir.IsWhiteSpace() && t2Dir.IsWhiteSpace()) return true;

            await Task.Run(() =>
            {
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0 && t1Dir.IsEmpty()) continue;
                    if (i == 1 && t2Dir.IsEmpty()) continue;

                    string savesPath = Path.Combine(i == 0 ? t1Dir : t2Dir, "allsaves");
                    if (!Directory.Exists(savesPath)) continue;

                    var convertedPath = Path.Combine(Config.FMsBackupPath, Paths.DarkLoaderSaveBakDir);
                    Directory.CreateDirectory(convertedPath);

                    // Converting takes too long, so just copy them to our backup folder and they'll be handled
                    // appropriately next time the user installs an FM
                    foreach (var f in Directory.EnumerateFiles(savesPath, "*.zip", SearchOption.TopDirectoryOnly))
                    {
                        var dest = Path.Combine(convertedPath, f.GetFileNameFast());
                        File.Copy(f, dest, overwrite: true);
                    }
                }
            });

            return true;
        }

        private async void ImportFromDarkLoaderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK) return;
            if (!ImportFMDataCheckBox.Checked && !ImportSavesCheckBox.Checked)
            {
                MessageBox.Show("Nothing was imported.", "Test alert yo");
                return;
            }

            // Whole thing is a stupid hack to make it not screw up by being async. Prolly get rid of this and
            // move the actual import to the caller
            e.Cancel = true;

            await Task.Yield();

            var (success, fms) = await Import();
            if (!success)
            {
                e.Cancel = true;
                return;
            }

            ImportedFMs.AddRange(fms);
            FormClosing -= ImportFromDarkLoaderForm_FormClosing;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void DarkLoaderIniBrowseButton_Click(object sender, EventArgs e)
        {
            using (var d = new OpenFileDialog())
            {
                d.Filter = LText.BrowseDialogs.IniFiles + @"|*.ini|" + LText.BrowseDialogs.AllFiles + @"|*.*";
                if (d.ShowDialog() != DialogResult.OK) return;

                DarkLoaderIniTextBox.Text = d.FileName;
            }
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
        A 4-bit flags field, exactly the same as AngelLoader uses, so no conversion needed at all.

        */
    }
}
