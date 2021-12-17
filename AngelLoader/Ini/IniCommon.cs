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
            string[] iniLines = File.ReadAllLines(fileName, Encoding.UTF8);

            if (fmsList.Count > 0) fmsList.Clear();

            bool fmsListIsEmpty = true;

            foreach (string line in iniLines)
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

        private static void ReadTags(FMCategoriesCollection tags, string val)
        {
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
                    cat = PresetTags.MiscCategory;
                    tag = item.Trim();
                }

                if (tags.TryGetValue(cat, out FMTagsCollection tagsList))
                {
                    if (!tag.IsEmpty()) tagsList.Add(tag);
                }
                else
                {
                    var newTagsList = new FMTagsCollection();
                    tags.Add(cat, newTagsList);
                    if (!tag.IsEmpty()) newTagsList.Add(tag);
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
            var intermediateTagsList = new List<string>();
            foreach (var catAndTags in tagsList)
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

            return string.Join(",", intermediateTagsList);
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
