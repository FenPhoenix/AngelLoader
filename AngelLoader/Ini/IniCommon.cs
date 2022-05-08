using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.LanguageSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static partial class Ini
    {
        #region BindingFlags

        private const BindingFlags _bFlagsEnum = BindingFlags.Instance |
                                                BindingFlags.Static |
                                                BindingFlags.Public |
                                                BindingFlags.NonPublic;

        #endregion

        // This kinda belongs in LanguageIni.cs, but it's separated to prevent it from being removed when that
        // file is re-generated. I could make it so it doesn't get removed, but meh.
        internal static void AddLanguageFromFile(string file, DictionaryI<string> langDict)
        {
            StreamReader? sr = null;
            try
            {
                sr = new StreamReader(file, Encoding.UTF8);

                bool inMeta = false;
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    string lineT = line.Trim();
                    if (inMeta &&
                        lineT.StartsWithFast_NoNullChecks(nameof(LText.Meta.TranslatedLanguageName) + "="))
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

        private static readonly ReaderWriterLockSlim _fmDataIniRWLock = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim _configIniRWLock = new ReaderWriterLockSlim();

        internal static void WriteFullFMDataIni()
        {
            try
            {
                _fmDataIniRWLock.EnterWriteLock();
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
                    _fmDataIniRWLock.ExitWriteLock();
                }
                catch (Exception ex)
                {
                    Log("Exception exiting " + nameof(_fmDataIniRWLock), ex);
                }
            }
        }

        internal static void ReadFMDataIni(string fileName, List<FanMission> fmsList)
        {
            fmsList.Clear();

            var iniLines = File_ReadAllLines_List(fileName, Encoding.UTF8);

            int fmCount = 0;
            for (int i = 0; i < iniLines.Count; i++)
            {
                if (iniLines[i] == "[FM]") fmCount++;
            }

            fmsList.Capacity = fmCount;

            bool fmsListIsEmpty = true;

            for (int i = 0; i < iniLines.Count; i++)
            {
                string lineTS = iniLines[i].TrimStart();

                if (lineTS.Length > 0 && lineTS[0] == '[')
                {
                    if (lineTS.Length >= 4 && lineTS[1] == 'F' && lineTS[2] == 'M' && lineTS[3] == ']')
                    {
                        fmsList.Add(new FanMission());
                        if (fmsListIsEmpty) fmsListIsEmpty = false;
                    }

                    continue;
                }

                if (fmsListIsEmpty) continue;

                if (lineTS.Length == 0 || lineTS[0] == ';') continue;

                int eqIndex = lineTS.IndexOf('=');
                if (eqIndex > -1)
                {
                    string key = lineTS.Substring(0, eqIndex);
                    string valRaw = lineTS.Substring(eqIndex + 1);
                    string valTrimmed = valRaw.Trim();
                    if (_actionDict_FMData.TryGetValue(key, out var action))
                    {
                        action.Invoke(fmsList[fmsList.Count - 1], valTrimmed, valRaw);
                    }
                }
            }
        }

        internal static void WriteConfigIni()
        {
            try
            {
                _configIniRWLock.EnterWriteLock();
                WriteConfigIniInternal(Config, Paths.ConfigIni);
            }
            catch (Exception ex)
            {
                Log("There was an error while writing to " + Paths.ConfigIni + ".", ex);
            }
            finally
            {
                try
                {
                    _configIniRWLock.ExitWriteLock();
                }
                catch (Exception ex)
                {
                    Log("Exception exiting " + nameof(_configIniRWLock), ex);
                }
            }
        }

        #region Helpers

        #region FMData

        // Doesn't handle whitespace around lang strings, but who cares, I'm so done with this.
        // We don't write out whitespace between them anyway.
        private static void SetFMLanguages(FanMission fm, string langsString)
        {
            fm.Langs = Language.Default;

            int len = langsString.Length;

            int curStart = 0;

            for (int i = 0; i < len; i++)
            {
                char c = langsString[i];

                if (c == ',' || i == len - 1)
                {
                    int end = i;

                    if (end == len - 1) end++;

                    if (end - curStart > 0 && Langs_TryGetValue(langsString, curStart, end, out Language result))
                    {
                        fm.Langs |= result;
                    }

                    curStart = i + 1;
                }
            }
        }

        private static bool SegmentEquals(this string first, int start, int end, string second)
        {
            for (; start < end; start++)
            {
                if (!char.IsWhiteSpace(first[start])) break;
            }

            for (; end >= start; end--)
            {
                if (!char.IsWhiteSpace(first[end])) break;
            }

            int secondLen = second.Length;
            if ((end - start) < secondLen - 1) return false;

            int i = start;
            int i2 = 0;
            while (i < end && i2 < secondLen - 1)
            {
                if (first[i] != second[i2]) return false;

                i++;
                i2++;
            }
            return true;
        }

        private static void FillFMHasXFields(FanMission fm, string fieldsString)
        {
            // Resources must be cleared here
            fm.Resources = CustomResources.None;

            int curStart = 0;

            int len = fieldsString.Length;

            for (int i = 0; i < len; i++)
            {
                char c = fieldsString[i];

                if (c == ',' || i == len - 1)
                {
                    if (curStart == 0 && fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.None)))
                    {
                        return;
                    }
                    else if (fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.Map)))
                    {
                        SetFMResource(fm, CustomResources.Map, true);
                    }
                    else if (fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.Automap)))
                    {
                        SetFMResource(fm, CustomResources.Automap, true);
                    }
                    else if (fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.Scripts)))
                    {
                        SetFMResource(fm, CustomResources.Scripts, true);
                    }
                    else if (fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.Textures)))
                    {
                        SetFMResource(fm, CustomResources.Textures, true);
                    }
                    else if (fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.Sounds)))
                    {
                        SetFMResource(fm, CustomResources.Sounds, true);
                    }
                    else if (fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.Objects)))
                    {
                        SetFMResource(fm, CustomResources.Objects, true);
                    }
                    else if (fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.Creatures)))
                    {
                        SetFMResource(fm, CustomResources.Creatures, true);
                    }
                    else if (fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.Motions)))
                    {
                        SetFMResource(fm, CustomResources.Motions, true);
                    }
                    else if (fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.Movies)))
                    {
                        SetFMResource(fm, CustomResources.Movies, true);
                    }
                    else if (fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.Subtitles)))
                    {
                        SetFMResource(fm, CustomResources.Subtitles, true);
                    }

                    curStart = i + 1;
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

        private static bool TryParseIntPair(string valTrimmed, out int first, out int second)
        {
            if (!valTrimmed.Contains(','))
            {
                first = 0;
                second = 0;
                return false;
            }

            string[] values = valTrimmed.Split(CA_Comma);
            bool firstExists = Int_TryParseInv(values[0].Trim(), out first);
            bool secondExists = Int_TryParseInv(values[1].Trim(), out second);

            return firstExists && secondExists;
        }

        private static SelectedFM GetSelectedFM(ConfigData config, GameIndex gameIndex, bool getGlobalSelectedFM)
        {
            return getGlobalSelectedFM ? config.SelFM : config.GameTabsState.GetSelectedFM(gameIndex);
        }

        private static Filter GetFilter(ConfigData config, GameIndex gameIndex, bool getGlobalFilter)
        {
            return getGlobalFilter ? config.Filter : config.GameTabsState.GetFilter(gameIndex);
        }

        private static void AddColumn(ConfigData config, string valTrimmed, Column columnType)
        {
            // DisplayIndex,Width,Visible
            // 0,100,True

            string value = valTrimmed.Trim(CA_Comma);
            string[] cProps;
            if (value.Contains(',') &&
                (cProps = value.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries)).Length > 0)
            {
                var col = new ColumnData { Id = columnType };
                for (int i = 0; i < cProps.Length; i++)
                {
                    switch (i)
                    {
                        case 0 when int.TryParse(cProps[i], out int di):
                            col.DisplayIndex = di;
                            break;
                        case 1 when int.TryParse(cProps[i], out int width):
                            col.Width = width;
                            break;
                        case 2:
                            col.Visible = cProps[i].EqualsTrue();
                            break;
                    }
                }
                config.Columns[(int)col.Id] = col;
            }
        }

        private static void ReadFilterTags(string tagsToAdd, FMCategoriesCollection existingTags)
        {
            if (tagsToAdd.IsWhiteSpace()) return;

            // We need slightly different logic for this vs. the FM tag adder, to wit, we support tags of the
            // form "category:" (with no tags list). This is because we allow filtering by entire category,
            // whereas we don't allow FMs to have categories with no tags in them.

            string[] tagsArray = tagsToAdd.Split(CA_CommaSemicolon, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tagsArray.Length; i++)
            {
                if (!FMTags.TryGetCatAndTag(tagsArray[i], out string cat, out string tag) ||
                    cat.IsEmpty())
                {
                    continue;
                }

                if (existingTags.TryGetValue(cat, out FMTagsCollection tagsList))
                {
                    if (!tag.IsEmpty()) tagsList.Add(tag);
                }
                else
                {
                    var newTagsList = new FMTagsCollection();
                    if (!tag.IsEmpty()) newTagsList.Add(tag);
                    existingTags.Add(cat, newTagsList);
                }
            }
        }

        private static void ReadFinishedStates(Filter filter, string val)
        {
            var list = val
                .Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

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

        private static void CommaCombineGameFlags(StringBuilder sb, Game games)
        {
            bool notEmpty = false;

            for (int i = 0; i < SupportedGameCount; i++)
            {
                Game game = GameIndexToGame((GameIndex)i);
                if (games.HasFlagFast(game))
                {
                    if (notEmpty) sb.Append(',');
                    sb.Append(game.ToString());
                    notEmpty = true;
                }
            }

            sb.AppendLine();
        }

        private static void CommaCombineLanguageFlags(StringBuilder sb, Language languages)
        {
            bool notEmpty = false;
            for (int i = 0; i < SupportedLanguageCount; i++)
            {
                LanguageIndex languageIndex = (LanguageIndex)i;
                Language language = LanguageIndexToLanguage(languageIndex);
                if (languages.HasFlagFast(language))
                {
                    if (notEmpty) sb.Append(',');
                    sb.Append(SupportedLanguages[i]);
                    notEmpty = true;
                }
            }

            sb.AppendLine();
        }

        private static void CommaCombineFinishedStates(StringBuilder sb, FinishedState finished)
        {
            bool notEmpty = false;

            if (finished.HasFlagFast(FinishedState.Finished))
            {
                sb.Append(nameof(FinishedState.Finished));
                notEmpty = true;
            }
            if (finished.HasFlagFast(FinishedState.Unfinished))
            {
                if (notEmpty) sb.Append(',');
                sb.Append(nameof(FinishedState.Unfinished));
            }

            sb.AppendLine();
        }

        private static string FilterDate(DateTime? dt) => dt == null
            ? ""
            : new DateTimeOffset((DateTime)dt).ToUnixTimeSeconds().ToString("X");

        private static string TagsToString(FMCategoriesCollection tagsList)
        {
            return FMTags.TagsToString(tagsList, writeEmptyCategories: true);
        }

        /// <summary>
        /// Sets some values that need everything in place first, and ensures validity of values that need it.
        /// </summary>
        /// <param name="config"></param>
        private static void FinalizeConfig(ConfigData config)
        {
            #region Ensure validity of game filter visibilities

            bool atLeastOneVisible = false;
            for (int i = 0; i < SupportedGameCount; i++)
            {
                if (config.GameFilterControlVisibilities[i])
                {
                    atLeastOneVisible = true;
                    break;
                }
            }

            if (!atLeastOneVisible)
            {
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    config.GameFilterControlVisibilities[i] = true;
                }
            }

            for (int i = 0; i < SupportedGameCount; i++)
            {
                Game game = GameIndexToGame((GameIndex)i);

                if (config.Filter.Games.HasFlagFast(game) &&
                    !config.GameFilterControlVisibilities[i])
                {
                    config.Filter.Games &= ~game;
                }
            }

            #endregion

            config.TopRightTabsData.EnsureValidity();

            #region Date format

            static string GetFormattedDateString(ConfigData config) =>
                config.DateCustomFormat1 +
                config.DateCustomSeparator1.EscapeAllChars() +
                config.DateCustomFormat2 +
                config.DateCustomSeparator2.EscapeAllChars() +
                config.DateCustomFormat3 +
                config.DateCustomSeparator3.EscapeAllChars() +
                config.DateCustomFormat4;

            static void ResetCustomDate(ConfigData config)
            {
                config.DateCustomFormat1 = Defaults.DateCustomFormat1;
                config.DateCustomSeparator1 = Defaults.DateCustomSeparator1;
                config.DateCustomFormat2 = Defaults.DateCustomFormat2;
                config.DateCustomSeparator2 = Defaults.DateCustomSeparator2;
                config.DateCustomFormat3 = Defaults.DateCustomFormat3;
                config.DateCustomSeparator3 = Defaults.DateCustomSeparator3;
                config.DateCustomFormat4 = Defaults.DateCustomFormat4;
                config.DateCustomFormatString = GetFormattedDateString(config);
            }

            if (!ValidDateFormatList.Contains(config.DateCustomFormat1) ||
                !ValidDateFormatList.Contains(config.DateCustomFormat2) ||
                !ValidDateFormatList.Contains(config.DateCustomFormat3) ||
                !ValidDateFormatList.Contains(config.DateCustomFormat4))
            {
                ResetCustomDate(config);
            }

            string formatString = GetFormattedDateString(config);

            try
            {
                // PERF: Passing an explicit DateTimeFormatInfo avoids a 5ms(!) hit that you take otherwise.
                // Man, DateTime and culture stuff is SLOW.
                _ = new DateTime(2000, 1, 1).ToString(formatString, DateTimeFormatInfo.InvariantInfo);
                config.DateCustomFormatString = formatString;
            }
            catch (FormatException)
            {
                ResetCustomDate(config);
            }
            catch (ArgumentOutOfRangeException)
            {
                ResetCustomDate(config);
            }

            #endregion
        }

        #endregion

        #endregion
    }
}
