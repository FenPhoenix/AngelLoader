﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.LanguageSupport;
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

        private static readonly ReaderWriterLockSlim _fmDataIniRWLock = new();
        private static readonly ReaderWriterLockSlim _configIniRWLock = new();

        private static string GetBackupFileName()
        {
            const int maxBackups = 10;

            try
            {
                var fileInfos = new DirectoryInfo(Paths.Data)
                    .GetFiles(Paths.FMDataBakBase + "*", SearchOption.TopDirectoryOnly)
                    .ToList();

                for (int i = 0; i < fileInfos.Count; i++)
                {
                    string fileName = fileInfos[i].Name;
                    if (!Regex.Match(fileName, Paths.FMDataBakNumberedRegexString).Success)
                    {
                        fileInfos.RemoveAt(i);
                        i--;
                    }
                }

                if (fileInfos.Count == 0)
                {
                    return Path.Combine(Paths.Data, Paths.FMDataBakBase + "1");
                }

                FileInfo? lastWritten = fileInfos.OrderByDescending(x => x.LastWriteTime).ToArray()[0];
                string lastWrittenFileNumStr = lastWritten.Name.Substring(Paths.FMDataBakBase.Length);

                int newNum = 1;
                if (int.TryParse(lastWrittenFileNumStr, out int lastWrittenFileNum))
                {
                    newNum = lastWrittenFileNum >= maxBackups ? 1 : lastWrittenFileNum + 1;
                }

                return Path.Combine(Paths.Data, Paths.FMDataBakBase + newNum);
            }
            catch
            {
                return Path.Combine(Paths.Data, Paths.FMDataBakBase + "1");
            }
        }

        internal static void WriteFullFMDataIni(bool makeBackup = false)
        {
            try
            {
                _fmDataIniRWLock.EnterWriteLock();
                if (makeBackup)
                {
                    string file = GetBackupFileName();
                    try
                    {
                        if (File.Exists(Paths.FMDataIni))
                        {
                            File.Copy(Paths.FMDataIni, file, overwrite: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Error attempting to copy '" + Paths.FMDataIni + "' to '" + file + "'", ex);
                    }
                }
                WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
            }
            catch (Exception ex)
            {
                Log(ErrorText.ExWrite + Paths.FMDataIni, ex);
            }
            finally
            {
                try
                {
                    _fmDataIniRWLock.ExitWriteLock();
                }
                catch (Exception ex)
                {
                    Log(ErrorText.Ex + "exiting " + nameof(_fmDataIniRWLock), ex);
                }
            }
        }

        internal static unsafe void ReadFMDataIni(string fileName, List<FanMission> fmsList)
        {
            fmsList.Clear();

            using var fs = File.OpenRead(fileName);
            using var sr = new StreamReader(fs, Encoding.UTF8);

            bool fmsListIsEmpty = true;

            while (sr.ReadLine() is { } line)
            {
                string lineTS = line.TrimStart();

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
                    // @MEM(FMData read): Knowable values left:
                    // -Game
                    // -Resources
                    // -Langs (we do an alloc-free parse on the value itself, but we still substring the value)
                    // -SelectedLang
                    if (FMDataKeyLookup.TryGetValue(lineTS, eqIndex, out var action))
                    {
                        // If the value is an arbitrary string or other unknowable type, then we need to split
                        // the string so the value part can go in the FM field. But if the value is a knowable
                        // type, then we don't need to split the string, we can just parse the value section.
                        // This slashes our allocation count WAY down.
                        action(fmsList[fmsList.Count - 1], lineTS, eqIndex);
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
                    Log(ErrorText.Ex + "exiting " + nameof(_configIniRWLock), ex);
                }
            }
        }

        #region Helpers

        #region Positive integral value parsers

        // Parses from the "value" section of the string - no substring allocation needed

        private static bool TryParseIntFromEnd(string str, int indexPastSeparator, int maxDigits, out int result)
        {
            const int intMaxDigits = 10;

            result = 0;

            int strLen = str.Length;

            if (indexPastSeparator >= strLen ||
                strLen - indexPastSeparator > intMaxDigits ||
                strLen > indexPastSeparator + maxDigits)
            {
                return false;
            }

            try
            {
                int end = Math.Min(strLen, indexPastSeparator + maxDigits);
                for (int i = indexPastSeparator; i < end; i++)
                {
                    char c = str[i];
                    if (c.IsAsciiNumeric())
                    {
                        checked
                        {
                            result *= 10;
                            result += c - '0';
                        }
                    }
                    else
                    {
                        result = 0;
                        return false;
                    }
                }
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        private static bool TryParseULongFromEnd(string str, int indexPastSeparator, int maxDigits, out ulong result)
        {
            const int ulongMaxDigits = 20;

            result = 0;

            int strLen = str.Length;

            if (indexPastSeparator >= strLen ||
                strLen - indexPastSeparator > ulongMaxDigits ||
                strLen > indexPastSeparator + maxDigits)
            {
                return false;
            }

            try
            {
                int end = Math.Min(strLen, indexPastSeparator + maxDigits);
                for (int i = indexPastSeparator; i < end; i++)
                {
                    char c = str[i];
                    if (c.IsAsciiNumeric())
                    {
                        checked
                        {
                            result *= 10;
                            result += (ulong)(c - '0');
                        }
                    }
                    else
                    {
                        result = 0;
                        return false;
                    }
                }
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        private static bool TryParseUIntFromEnd(string str, int indexPastSeparator, int maxDigits, out uint result)
        {
            const int uintMaxDigits = 10;

            result = 0;

            int strLen = str.Length;

            if (indexPastSeparator >= strLen ||
                strLen - indexPastSeparator > uintMaxDigits ||
                strLen > indexPastSeparator + maxDigits)
            {
                return false;
            }

            try
            {
                int end = Math.Min(strLen, indexPastSeparator + maxDigits);
                for (int i = indexPastSeparator; i < end; i++)
                {
                    char c = str[i];
                    if (c.IsAsciiNumeric())
                    {
                        checked
                        {
                            result *= 10;
                            result += (uint)(c - '0');
                        }
                    }
                    else
                    {
                        result = 0;
                        return false;
                    }
                }
            }
            catch
            {
                result = 0;
                return false;
            }

            return true;
        }

        #endregion

        private static void AddReadmeEncoding(FanMission fm, string line, int indexAfterEq)
        {
            int lastIndexOfComma = line.LastIndexOf(',');

            if (lastIndexOfComma > -1 &&
                TryParseIntFromEnd(line, lastIndexOfComma + 1, 10, out int result) &&
                // 0 = default, we don't want to handle "default" as it's not a specific code page
                result > 0)
            {
                string readme = line.Substring(indexAfterEq, lastIndexOfComma - indexAfterEq);
                if (!readme.IsEmpty())
                {
                    fm.ReadmeCodePages[readme.ToBackSlashes()] = result;
                }
            }
        }

        #region FMData

        // Doesn't handle whitespace around lang strings, but who cares, I'm so done with this.
        // We don't write out whitespace between them anyway.
        private static void SetFMLanguages(FanMission fm, string langsString)
        {
            // It's always supposed to be ascii lowercase, so only take the allocation if it's not
            if (!langsString.IsAsciiLower()) langsString = langsString.ToLowerInvariant();

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
            var list = val.Split(CA_Comma);

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
        internal static void FinalizeConfig(ConfigData config)
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
