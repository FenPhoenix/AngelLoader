using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
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
        internal static void AddLanguageFromFile(string file, Dictionary<string, string> langDict)
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
                        langDict[key] = value;
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
        private static readonly ReaderWriterLockSlim ConfigIniRWLock = new ReaderWriterLockSlim();

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

        internal static void WriteFullConfigIni()
        {
            try
            {
                ConfigIniRWLock.EnterWriteLock();
                WriteConfigIni(Config, Paths.ConfigIni);
            }
            catch (Exception ex)
            {
                Log("There was an error while writing to " + Paths.ConfigIni + ".", ex);
            }
            finally
            {
                try
                {
                    ConfigIniRWLock.ExitWriteLock();
                }
                catch (Exception ex)
                {
                    Log("Exception exiting " + nameof(ConfigIniRWLock) + " in " + nameof(WriteFullConfigIni), ex);
                }
            }
        }

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
                if (notEmpty) sb.Append(',');
                sb.Append(nameof(CustomResources.Automap));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Scripts))
            {
                if (notEmpty) sb.Append(',');
                sb.Append(nameof(CustomResources.Scripts));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Textures))
            {
                if (notEmpty) sb.Append(',');
                sb.Append(nameof(CustomResources.Textures));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Sounds))
            {
                if (notEmpty) sb.Append(',');
                sb.Append(nameof(CustomResources.Sounds));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Objects))
            {
                if (notEmpty) sb.Append(',');
                sb.Append(nameof(CustomResources.Objects));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Creatures))
            {
                if (notEmpty) sb.Append(',');
                sb.Append(nameof(CustomResources.Creatures));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Motions))
            {
                if (notEmpty) sb.Append(',');
                sb.Append(nameof(CustomResources.Motions));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Movies))
            {
                if (notEmpty) sb.Append(',');
                sb.Append(nameof(CustomResources.Movies));
                notEmpty = true;
            }
            if (FMHasResource(fm, CustomResources.Subtitles))
            {
                if (notEmpty) sb.Append(',');
                sb.Append(nameof(CustomResources.Subtitles));
            }

            sb.AppendLine();
        }

        #endregion

        #region Config

        private static bool ContainsColWithId(ConfigData _config, ColumnData _col)
        {
            foreach (ColumnData x in _config.Columns) if (x.Id == _col.Id) return true;
            return false;
        }

        private static ColumnData? ConvertStringToColumnData(string str)
        {
            str = str.Trim().Trim(CA_Comma);

            // DisplayIndex,Width,Visible
            // 0,100,True

            if (!str.Contains(',')) return null;

            string[] cProps = str.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);
            if (cProps.Length == 0) return null;

            var ret = new ColumnData();
            for (int i = 0; i < cProps.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        if (int.TryParse(cProps[i], out int di))
                        {
                            ret.DisplayIndex = di;
                        }
                        break;
                    case 1:
                        if (int.TryParse(cProps[i], out int width))
                        {
                            ret.Width = width > Defaults.MinColumnWidth ? width : Defaults.MinColumnWidth;
                        }
                        break;
                    case 2:
                        ret.Visible = cProps[i].EqualsTrue();
                        break;
                }
            }

            return ret;
        }

        private static void ReadTags(string line, Game game)
        {
            Filter filter = GameIsKnownAndSupported(game)
                ? Config.GameTabsState.GetFilter(GameToGameIndex(game))
                : Config.Filter;

            // TODO: This line-passing-and-reading business is still a little janky
            CatAndTagsList? tagsList =
                line.StartsWithFast_NoNullChecks("And=") ? filter.Tags.AndTags :
                line.StartsWithFast_NoNullChecks("Or=") ? filter.Tags.OrTags :
                line.StartsWithFast_NoNullChecks("Not=") ? filter.Tags.NotTags :
                null;

            if (tagsList == null) return;

            string val = line.Substring(line.IndexOf('=') + 1);

            if (val.IsWhiteSpace()) return;

            string[] tagsArray = val.Split(CA_CommaSemicolon, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in tagsArray)
            {
                string cat, tag;
                int colonCount = item.CountCharsUpToAmount(':', 2);
                if (colonCount > 1) continue;
                if (colonCount == 1)
                {
                    int index = item.IndexOf(':');
                    cat = item.Substring(0, index).Trim().ToLowerInvariant();
                    tag = item.Substring(index + 1).Trim();
                    if (cat.IsEmpty()) continue;
                }
                else
                {
                    cat = "misc";
                    tag = item.Trim();
                }

                CatAndTags? match = null;
                for (int i = 0; i < tagsList.Count; i++)
                {
                    if (tagsList[i].Category == cat)
                    {
                        match = tagsList[i];
                        break;
                    }
                }
                if (match == null)
                {
                    tagsList.Add(new CatAndTags { Category = cat });
                    if (!tag.IsEmpty()) tagsList[tagsList.Count - 1].Tags.Add(tag);
                }
                else
                {
                    if (!tag.IsEmpty() && !match.Tags.ContainsI(tag)) match.Tags.Add(tag);
                }
            }
        }

        private static void ReadFinishedStates(string val, Filter filter)
        {
            var list = val.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            foreach (string finishedState in list)
            {
                switch (finishedState.Trim())
                {
                    case nameof(FinishedState.Finished):
                        filter.Finished |= FinishedState.Finished;
                        break;
                    case nameof(FinishedState.Unfinished):
                        filter.Finished |= FinishedState.Unfinished;
                        break;
                }
            }
        }

        private static string CommaCombine<T>(List<T> list) where T : notnull
        {
            string ret = "";
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += list[i].ToString();
            }

            return ret;
        }

        // TODO: Figure out a better way to be fast without this dopey manual code. Code generation?
        private static string CommaCombineGameFlags(Game games)
        {
            string ret = "";

            // Hmm... doesn't make for good code, but fast...
            // @GENGAMES (Config writer - Comma combine game flags): Begin
            bool notEmpty = false;

            if ((games & Game.Thief1) == Game.Thief1)
            {
                ret += nameof(Game.Thief1);
                notEmpty = true;
            }
            if ((games & Game.Thief2) == Game.Thief2)
            {
                if (notEmpty) ret += ",";
                ret += nameof(Game.Thief2);
                notEmpty = true;
            }
            if ((games & Game.Thief3) == Game.Thief3)
            {
                if (notEmpty) ret += ",";
                ret += nameof(Game.Thief3);
                notEmpty = true;
            }
            if ((games & Game.SS2) == Game.SS2)
            {
                if (notEmpty) ret += ",";
                ret += nameof(Game.SS2);
            }
            // @GENGAMES (Config writer - Comma combine game flags): End

            return ret;
        }

        private static string CommaCombineFinishedStates(FinishedState finished)
        {
            string ret = "";

            bool notEmpty = false;

            if ((finished & FinishedState.Finished) == FinishedState.Finished)
            {
                ret += nameof(FinishedState.Finished);
                notEmpty = true;
            }
            if ((finished & FinishedState.Unfinished) == FinishedState.Unfinished)
            {
                if (notEmpty) ret += ",";
                ret += nameof(FinishedState.Unfinished);
            }

            return ret;
        }

        private static string FilterDate(DateTime? dt) => dt == null
            ? ""
            : new DateTimeOffset((DateTime)dt).ToUnixTimeSeconds().ToString("X");

        private static string TagsToString(CatAndTagsList tagsList)
        {
            var intermediateTagsList = new List<string>();
            foreach (CatAndTags catAndTags in tagsList)
            {
                if (catAndTags.Tags.Count == 0)
                {
                    intermediateTagsList.Add(catAndTags.Category + ":");
                }
                else
                {
                    string catC = catAndTags.Category + ":";
                    foreach (string tag in catAndTags.Tags)
                    {
                        intermediateTagsList.Add(catC + tag);
                    }
                }
            }

            string filterTagsString = "";
            for (int ti = 0; ti < intermediateTagsList.Count; ti++)
            {
                if (ti > 0) filterTagsString += ",";
                filterTagsString += intermediateTagsList[ti];
            }

            return filterTagsString;
        }

        #endregion
    }
}
