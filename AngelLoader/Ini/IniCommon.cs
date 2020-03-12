using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using AngelLoader.DataClasses;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static partial class Ini
    {
        internal static bool StartsWithFast_NoNullChecks(this string str, string value)
        {
            if (str.Length < value.Length) return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (str[i] != value[i]) return false;
            }

            return true;
        }

        #region BindingFlags

        private const BindingFlags BFlagsEnum = BindingFlags.Instance |
                                                BindingFlags.Static |
                                                BindingFlags.Public |
                                                BindingFlags.NonPublic;

        #endregion

        // This kinda belongs in LanguageIni.cs, but it's separated to prevent it from being removed when that
        // file is re-generated. I could make it so it doesn't get removed, but meh.
        internal static void ReadTranslatedLanguageName(string file)
        {
            StreamReader? sr = null;
            try
            {
                sr = new StreamReader(file, Encoding.UTF8);

                bool inMeta = false;
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string lineT = line.Trim();
                    if (inMeta && lineT.StartsWithFast_NoNullChecks(
                            nameof(LText.Meta.TranslatedLanguageName) + "="))
                    {
                        string key = file.GetFileNameFast().RemoveExtension();
                        string value = line.TrimStart().Substring(nameof(LText.Meta.TranslatedLanguageName).Length + 1);
                        Config.LanguageNames[key] = value;
                        return;
                    }
                    else if (lineT == "[" + nameof(LText.Meta) + "]")
                    {
                        inMeta = true;
                    }
                    else if (!lineT.IsEmpty() && lineT[0] == '[' && lineT[lineT.Length - 1] == ']')
                    {
                        inMeta = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("There was an error while reading " + file + ".", ex);
            }
            finally
            {
                sr?.Dispose();
            }
        }

        private static readonly ReaderWriterLockSlim FMDataIniRWLock = new ReaderWriterLockSlim();

        internal static void WriteFullFMDataIni()
        {
            try
            {
                FMDataIniRWLock.EnterWriteLock();
                WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
            }
            catch (Exception ex)
            {
                Log("Exception writing FM data ini", ex);
            }
            finally
            {
                try
                {
                    FMDataIniRWLock.ExitWriteLock();
                }
                catch (Exception ex)
                {
                    Log("Exception exiting " + nameof(FMDataIniRWLock) + " in " + nameof(WriteFullFMDataIni), ex);
                }
            }
        }

#if false
        internal static void WriteFMsListToCSV()
        {
            static string Escape(string value)
            {
                bool needsEscaping = false;
                value = value.Trim('\r', '\n');
                foreach (char c in value)
                {
                    if (c == '\"' || c == '\r' || c == '\n' || c == ',')
                    {
                        needsEscaping = true;
                        break;
                    }
                }

                return needsEscaping
                    ? "\"" + value.Replace("\"", "\"\"") + "\""
                    : value;
            }

            static string FinishedOn_(FanMission fm)
            {
                if (fm.FinishedOnUnknown) return "Unknown";
                FinishedOn diff = (FinishedOn)fm.FinishedOn;
                if (diff == FinishedOn.None) return "";

                string ret = "";
                if ((diff & FinishedOn.Normal) == FinishedOn.Normal)
                {
                    ret += (!ret.IsEmpty() ? "," : "") + "1";
                }
                if ((diff & FinishedOn.Hard) == FinishedOn.Hard)
                {
                    ret += (!ret.IsEmpty() ? "," : "") + "2";
                }
                if ((diff & FinishedOn.Expert) == FinishedOn.Expert)
                {
                    ret += (!ret.IsEmpty() ? "," : "") + "3";
                }
                if ((diff & FinishedOn.Extreme) == FinishedOn.Extreme)
                {
                    ret += (!ret.IsEmpty() ? "," : "") + "4";
                }

                return ret;
            }

            using (var sw = new StreamWriter(@"C:\AL_csv_test.csv", append: false, Encoding.UTF8))
            {
                const string header = nameof(Column.Game) + "," +
                                      nameof(Column.Installed) + "," +
                                      nameof(Column.Title) + "," +
                                      nameof(Column.Archive) + "," +
                                      nameof(Column.Author) + "," +
                                      nameof(Column.Size) + "," +
                                      nameof(Column.Rating) + "," +
                                      nameof(Column.Finished) + "," +
                                      nameof(Column.ReleaseDate) + "," +
                                      nameof(Column.LastPlayed) + "," +
                                      nameof(Column.DateAdded) + "," +
                                      nameof(Column.DisabledMods) + "," +
                                      nameof(Column.Comment);

                sw.WriteLine(header);
                foreach (var fm in FMsViewList)
                {
                    string line = fm.Game + "," +
                                  fm.Installed + "," +
                                  Escape(fm.Title) + "," +
                                  Escape(fm.Archive) + "," +
                                  Escape(fm.Author) + "," +
                                  fm.SizeBytes + "," +
                                  (fm.Rating > -1 ? fm.Rating.ToString() : "Unrated") + "," +
                                  Escape(FinishedOn_(fm)) + "," +
                                  fm.ReleaseDate.UnixDateString + "," +
                                  fm.LastPlayed.UnixDateString + "," +
                                  (fm.DateAdded != null
                                      ? new DateTimeOffset((DateTime)fm.DateAdded).ToUnixTimeSeconds()
                                          .ToString("X") : "") + "," +
                                  Escape(fm.DisabledMods) + "," +
                                  Escape(fm.Comment.FromRNEscapes()) + ",";

                    sw.WriteLine(line);
                }
            }
        }
#endif

        #region FM custom resource work

        private static void FillFMHasXFields(FanMission fm, string fieldsString)
        {
            string[] fields = fieldsString.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);

            // Resources must be cleared here
            fm.Resources = CustomResources.None;

            if (fields.Length > 0 && fields[0].EqualsI(nameof(CustomResources.None))) return;

            for (int i = 0; i < fields.Length; i++)
            {
                string field = fields[i];

                // Need this if block, because we're not iterating through all fields, so can't just have a flat
                // block of fm.HasX = field.EqualsI(X);
                if (field.EqualsI(nameof(CustomResources.Map)))
                {
                    SetFMResource(fm, CustomResources.Map, true);
                }
                else if (field.EqualsI(nameof(CustomResources.Automap)))
                {
                    SetFMResource(fm, CustomResources.Automap, true);
                }
                else if (field.EqualsI(nameof(CustomResources.Scripts)))
                {
                    SetFMResource(fm, CustomResources.Scripts, true);
                }
                else if (field.EqualsI(nameof(CustomResources.Textures)))
                {
                    SetFMResource(fm, CustomResources.Textures, true);
                }
                else if (field.EqualsI(nameof(CustomResources.Sounds)))
                {
                    SetFMResource(fm, CustomResources.Sounds, true);
                }
                else if (field.EqualsI(nameof(CustomResources.Objects)))
                {
                    SetFMResource(fm, CustomResources.Objects, true);
                }
                else if (field.EqualsI(nameof(CustomResources.Creatures)))
                {
                    SetFMResource(fm, CustomResources.Creatures, true);
                }
                else if (field.EqualsI(nameof(CustomResources.Motions)))
                {
                    SetFMResource(fm, CustomResources.Motions, true);
                }
                else if (field.EqualsI(nameof(CustomResources.Movies)))
                {
                    SetFMResource(fm, CustomResources.Movies, true);
                }
                else if (field.EqualsI(nameof(CustomResources.Subtitles)))
                {
                    SetFMResource(fm, CustomResources.Subtitles, true);
                }
            }
        }

        private static void CommaCombineHasXFields(FanMission fm, StringBuilder sb)
        {
            if (fm.Resources == CustomResources.None)
            {
                sb.AppendLine(nameof(CustomResources.None));
                return;
            }
            // Hmm... doesn't make for good code, but fast...
            bool notEmpty = false;
            if (FMHasResource(fm, CustomResources.Map))
            {
                sb.Append(nameof(CustomResources.Map));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Automap))
            {
                if (notEmpty) sb.Append(",");
                sb.Append(nameof(CustomResources.Automap));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Scripts))
            {
                if (notEmpty) sb.Append(",");
                sb.Append(nameof(CustomResources.Scripts));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Textures))
            {
                if (notEmpty) sb.Append(",");
                sb.Append(nameof(CustomResources.Textures));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Sounds))
            {
                if (notEmpty) sb.Append(",");
                sb.Append(nameof(CustomResources.Sounds));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Objects))
            {
                if (notEmpty) sb.Append(",");
                sb.Append(nameof(CustomResources.Objects));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Creatures))
            {
                if (notEmpty) sb.Append(",");
                sb.Append(nameof(CustomResources.Creatures));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Motions))
            {
                if (notEmpty) sb.Append(",");
                sb.Append(nameof(CustomResources.Motions));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Movies))
            {
                if (notEmpty) sb.Append(",");
                sb.Append(nameof(CustomResources.Movies));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Subtitles))
            {
                if (notEmpty) sb.Append(",");
                sb.Append(nameof(CustomResources.Subtitles));
            }

            sb.AppendLine();
        }

        #endregion
    }
}
