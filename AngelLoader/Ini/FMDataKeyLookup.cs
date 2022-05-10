//#define CONVERSION_ENABLED
using System.Runtime.CompilerServices;
using System.Text;
using AngelLoader.DataClasses;

namespace AngelLoader
{
    internal static partial class Ini
    {
        internal static class FMDataKeyLookup
        {
#if CONVERSION_ENABLED
            // @MEM: Make this more automated later! It's going to be a pain!
            public static void ConvertSemiAuto()
            {
                const string gperfFile = @"C:\temp2.txt";

                var lines = File.ReadAllLines(gperfFile);

                for (int i = 0; i < lines.Length; i++)
                {
                    string lineT = lines[i].Trim();
                    if (lineT.StartsWith("#line"))
                    {
                        string lineNumRaw = lineT.Substring(5, lineT.IndexOf('\"') - 5).Trim();
                        if (int.TryParse(lineNumRaw, out int result))
                        {
                            // -3 because the gperf in format is like
                            // (line 1) struct declaration
                            // (line 2) %%
                            // (line 3) 1st entry (but 3rd line)
                            lines[i] = "// Entry " + (result - 3);
                        }
                    }
                    else
                    {
                        lines[i] = lines[i].Replace("{\"\"}", "null");
                        lines[i] = lines[i].Trim();
                        Match m = Regex.Match(lineT, @"{\""(?<Value>[^\""]+)");
                        if (m.Success)
                        {
                            string value = m.Groups["Value"].Value;
                            lines[i] = "new(\"" + value + "\", &FMData_" + value + "_Set)"
                                       + (i < lines.Length - 1 ? "," : "");
                        }
                    }
                }

                File.WriteAllLines(@"C:\OUT_TEST.txt", lines);
            }
#endif
            /* ANSI-C code produced by gperf version 3.1 */
            /* Command-line: gperf --output-file='c:\\gperf_out.txt' -t -L ANSI-C -H Hash -G -W _dict 'c:\\gperf_in.txt'  */
            /* Computed positions: -k'1,4' */

            //private const int TOTAL_KEYWORDS = 38;
            private const int MIN_WORD_LENGTH = 4;
            private const int MAX_WORD_LENGTH = 17;
            //private const int MIN_HASH_VALUE = 6;
            private const int MAX_HASH_VALUE = 64;
            /* maximum key range = 59, duplicates = 0 */

            private static readonly byte[] asso_values =
            {
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 10, 65, 40, 15, 65,
                20, 55, 0, 35, 65, 65, 15, 0, 35, 40,
                10, 65, 0, 5, 20, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 5, 65, 65,
                45, 5, 65, 10, 10, 5, 65, 0, 20, 10,
                5, 65, 65, 65, 10, 10, 0, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65, 65, 65, 65, 65,
                65, 65, 65, 65, 65, 65
            };

            private sealed unsafe class NameAndAction
            {
                internal readonly string Name;
                internal readonly delegate*<FanMission, StringBuilder, int, void> Action;

                internal NameAndAction(string name, delegate*<FanMission, StringBuilder, int, void> action)
                {
                    Name = name;
                    Action = action;
                }
            }

            private static readonly unsafe NameAndAction?[] _actions =
            {
                null, null, null, null, null, null,
// Entry 28
                new("HasMap", &FMData_HasMap_Set),
                null, null,
// Entry 36
                new("HasMovies", &FMData_HasMovies_Set),
// Entry 35
                new("HasMotions", &FMData_HasMotions_Set),
// Entry 14
                new("Rating", &FMData_Rating_Set),
// Entry 23
                new("HasResources", &FMData_HasResources_Set),
// Entry 1
                new("MarkedScanned", &FMData_MarkedScanned_Set),
// Entry 32
                new("HasSounds", &FMData_HasSounds_Set),
// Entry 30
                new("HasScripts", &FMData_HasScripts_Set),
// Entry 15
                new("ReleaseDate", &FMData_ReleaseDate_Set),
// Entry 37
                new("HasSubtitles", &FMData_HasSubtitles_Set),
                null,
// Entry 13
                new("SizeBytes", &FMData_SizeBytes_Set),
// Entry 29
                new("HasAutomap", &FMData_HasAutomap_Set),
// Entry 2
                new("Pinned", &FMData_Pinned_Set),
// Entry 26
                new("SelectedLang", &FMData_SelectedLang_Set),
                null,
// Entry 11
                new("SelectedReadme", &FMData_SelectedReadme_Set),
// Entry 16
                new("LastPlayed", &FMData_LastPlayed_Set),
// Entry 7
                new("Author", &FMData_Author_Set),
// Entry 3
                new("Archive", &FMData_Archive_Set),
                null,
// Entry 17
                new("DateAdded", &FMData_DateAdded_Set),
// Entry 25
                new("Langs", &FMData_Langs_Set),
// Entry 31
                new("HasTextures", &FMData_HasTextures_Set),
// Entry 21
                new("DisabledMods", &FMData_DisabledMods_Set),
                null,
// Entry 22
                new("DisableAllMods", &FMData_DisableAllMods_Set),
// Entry 18
                new("FinishedOn", &FMData_FinishedOn_Set),
                null,
// Entry 24
                new("LangsScanned", &FMData_LangsScanned_Set),
                null,
// Entry 6
                new("AltTitles", &FMData_AltTitles_Set),
// Entry 27
                new("TagsString", &FMData_TagsString_Set),
                null,
// Entry 19
                new("FinishedOnUnknown", &FMData_FinishedOnUnknown_Set),
                null,
// Entry 9
                new("Installed", &FMData_Installed_Set),
// Entry 5
                new("Title", &FMData_Title_Set),
                null,
// Entry 4
                new("InstalledDir", &FMData_InstalledDir_Set),
                null,
// Entry 10
                new("NoReadmes", &FMData_NoReadmes_Set),
// Entry 33
                new("HasObjects", &FMData_HasObjects_Set),
                null,
// Entry 34
                new("HasCreatures", &FMData_HasCreatures_Set),
                null,
// Entry 0
                new("NoArchive", &FMData_NoArchive_Set),
                null, null,
// Entry 20
                new("Comment", &FMData_Comment_Set),
                null,
// Entry 12
                new("ReadmeEncoding", &FMData_ReadmeEncoding_Set),
                null, null, null, null,
// Entry 8
                new("Game", &FMData_Game_Set)
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static uint Hash(string str, int len)
            {
                return (uint)len + asso_values[str[3]] + asso_values[str[0]];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static uint Hash(StringBuilder str, int len)
            {
                return (uint)len + asso_values[str[3]] + asso_values[str[0]];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool SeqEqual(string seq1, int seq1Length, string seq2)
            {
                if (seq1Length != seq2.Length) return false;

                for (int ci = 0; ci < seq1Length; ci++)
                {
                    if (seq1[ci] != seq2[ci]) return false;
                }
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool SeqEqual(StringBuilder seq1, int seq1Length, string seq2)
            {
                if (seq1Length != seq2.Length) return false;

                for (int ci = 0; ci < seq1Length; ci++)
                {
                    if (seq1[ci] != seq2[ci]) return false;
                }
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe bool TryGetValue(StringBuilder str, int len, out delegate*<FanMission, StringBuilder, int, void> result)
            {
                if (len is <= MAX_WORD_LENGTH and >= MIN_WORD_LENGTH)
                {
                    uint key = Hash(str, len);

                    if (key <= MAX_HASH_VALUE)
                    {
                        NameAndAction? item = _actions[key];
                        if (item == null)
                        {
                            result = null;
                            return false;
                        }

                        if (SeqEqual(str, len, item.Name))
                        {
                            result = item.Action;
                            return true;
                        }
                    }
                }

                result = null;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe bool TryGetValue(string str, int len, out delegate*<FanMission, StringBuilder, int, void> result)
            {
                if (len is <= MAX_WORD_LENGTH and >= MIN_WORD_LENGTH)
                {
                    uint key = Hash(str, len);

                    if (key <= MAX_HASH_VALUE)
                    {
                        NameAndAction? item = _actions[key];
                        if (item == null)
                        {
                            result = null;
                            return false;
                        }

                        if (SeqEqual(str, len, item.Name))
                        {
                            result = item.Action;
                            return true;
                        }
                    }
                }

                result = null;
                return false;
            }
        }
    }
}
