using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AL_Common.LanguageSupport;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader;

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
    internal static void AddLanguageFromFile(string file, string key, DictionaryI<string> langDict)
    {
        using var sr = new StreamReader(file, Encoding.UTF8);

        bool inMeta = false;
        while (sr.ReadLine() is { } line)
        {
            string lineT = line.Trim();
            if (inMeta &&
                lineT.StartsWithFast_NoNullChecks(nameof(LText.Meta.TranslatedLanguageName) + "="))
            {
                langDict[key] = line.TrimStart().Substring(nameof(LText.Meta.TranslatedLanguageName).Length + 1);
                return;
            }
            else if (lineT == "[" + nameof(LText.Meta) + "]")
            {
                inMeta = true;
            }
            else if (!lineT.IsEmpty() && lineT[0] == '[')
            {
                inMeta = false;
            }
        }
    }

    private static readonly object _writeFMDataIniLock = new();
    private static readonly object _writeConfigIniLock = new();

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

            FileInfo? lastWritten = fileInfos.OrderByDescending(static x => x.LastWriteTime).ToArray()[0];
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

#if DateAccTest
    private static readonly string _dateAccuracyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DateAccuracy.ini");

    internal static void ReadDateAccuracyFile()
    {
        if (!File.Exists(_dateAccuracyFile)) return;

        var dict = new DictionaryI<FanMission>(FMDataIniList.Count);
        foreach (FanMission fm in FMDataIniList)
        {
            dict[fm.InstalledDir] = fm;
        }

        var lines = File_ReadAllLines_List(_dateAccuracyFile);
        foreach (var line in lines)
        {
            string lineT = line.Trim();
            if (lineT.IsEmpty()) continue;

            int eqIndex = lineT.LastIndexOf('=');
            if (eqIndex == -1) continue;

            string instDir = lineT.Substring(0, eqIndex);
            string value = lineT.Substring(eqIndex + 1);

            if (dict.TryGetValue(instDir, out FanMission? fm))
            {
                fm.DateAccuracy = DateAccuracy_Deserialize(value);
            }
        }
    }

    private static void WriteDateAccuracyFile()
    {
        using var sw = new StreamWriter(_dateAccuracyFile);
        foreach (FanMission fm in FMDataIniList)
        {
            sw.WriteLine(fm.InstalledDir + "=" + DateAccuracy_Serialize(fm.DateAccuracy));
        }
    }
#endif

    internal static void WriteFullFMDataIni(bool makeBackup = false)
    {
        lock (_writeFMDataIniLock)
        {
            try
            {
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
                        Log(ErrorText.ExCopy + "'" + Paths.FMDataIni + "' to '" + file + "'", ex);
                    }
                }
                WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
#if DateAccTest
            WriteDateAccuracyFile();
#endif
            }
            catch (Exception ex)
            {
                Log(ErrorText.ExWrite + Paths.FMDataIni, ex);
            }
        }
    }

    private sealed class KeyComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == y) return true;

            // Intended: x == key in dict (no '='), y == incoming full line (with '=')
            // But assume we have no guarantee on which param is which, so swap them if they're wrong.

            int index = y.IndexOf('=');
            if (index == -1)
            {
                (y, x) = (x, y);
                index = y.IndexOf('=');
            }

            if (index != x.Length) return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i]) return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateLeft(uint value, int offset) => (value << offset) | (value >> (32 - offset));

        public unsafe int GetHashCode(string obj)
        {
            // From .NET 7 (but tweaked to stop at '=') - no separate 32/64 paths, and doesn't stop at nulls
            fixed (char* src = obj)
            {
                uint hash1 = (5381 << 16) + 5381;
                uint hash2 = hash1;

                uint* ptr = (uint*)src;

                int length = obj.IndexOf('=');
                length = length == -1 ? obj.Length : length + 1;
                int originalLength = length;

                while (length > 2 && *ptr < originalLength)
                {
                    length -= 4;
                    // Where length is 4n-1 (e.g. 3,7,11,15,19) this additionally consumes the null terminator
                    hash1 = (RotateLeft(hash1, 5) + hash1) ^ ptr[0];
                    hash2 = (RotateLeft(hash2, 5) + hash2) ^ ptr[1];
                    ptr += 2;
                }

                if (length > 0)
                {
                    // Where length is 4n-3 (e.g. 1,5,9,13,17) this additionally consumes the null terminator
                    hash2 = (RotateLeft(hash2, 5) + hash2) ^ ptr[0];
                }

                return (int)(hash1 + (hash2 * 1566083941));
            }
        }
    }

    internal static unsafe void ReadFMDataIni(string fileName, List<FanMission> fmsList)
    {
        fmsList.Clear();

        using var sr = new StreamReaderCustom.SRC_Wrapper(File_OpenReadFast(fileName), new StreamReaderCustom());

        bool fmsListIsEmpty = true;

        while (sr.Reader.ReadLine() is { } line)
        {
            string lineTS = line.TrimStart();

            if (lineTS.Length > 0 && lineTS[0] == '[')
            {
                if (lineTS.Length >= 4 && lineTS[1] == 'F' && lineTS[2] == 'M' && lineTS[3] == ']')
                {
                    fmsList.Add(new FanMission());
                    fmsListIsEmpty = false;
                }

                continue;
            }

            if (fmsListIsEmpty) continue;

            if (lineTS.Length == 0 || lineTS[0] == ';') continue;

            int eqIndex = lineTS.IndexOf('=');
            if (eqIndex > -1)
            {
                if (_actionDict_FMData.TryGetValue(lineTS, out var result))
                {
                    // If the value is an arbitrary string or other unknowable type, then we need to split
                    // the string so the value part can go in the FM field. But if the value is a knowable
                    // type, then we don't need to split the string, we can just parse the value section.
                    // This slashes our allocation count WAY down.
                    result.Action(fmsList[fmsList.Count - 1], lineTS, eqIndex);
                }
            }
        }
    }

    internal static void WriteConfigIni()
    {
        lock (_writeConfigIniLock)
        {
            try
            {
                WriteConfigIniInternal(Config, Paths.ConfigIni);
            }
            catch (Exception ex)
            {
                Log(ErrorText.ExWrite + Paths.ConfigIni + ".", ex);
            }
        }
    }

    #region Helpers

    #region Positive integral value parsers

    // Parses from the "value" section of the string - no substring allocation needed

    [PublicAPI]
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

    [PublicAPI]
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

    [PublicAPI]
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

    #region FMData

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

    // Doesn't handle whitespace around lang strings, but who cares, I'm so done with this.
    // We don't write out whitespace between them anyway.
    private static void SetFMLanguages(FanMission fm, string langsString, int start)
    {
        // It's always supposed to be ascii lowercase, so only take the allocation if it's not
        if (!langsString.IsAsciiLower(start))
        {
            // This will lowercase the key portion of the string too, but we only deal with the value so we
            // don't care
            langsString = langsString.ToLowerInvariant();
        }

        fm.Langs = Language.Default;

        int len = langsString.Length;

        int curStart = start;

        for (int i = start; i < len; i++)
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

    private static void FillFMHasXFields(FanMission fm, string fieldsString, int start)
    {
        // Resources must be cleared here
        fm.Resources = CustomResources.None;

        int curStart = start;

        int len = fieldsString.Length;

        for (int i = start; i < len; i++)
        {
            char c = fieldsString[i];

            if (c == ',' || i == len - 1)
            {
                if (curStart == start && fieldsString.SegmentEquals(curStart, i, nameof(CustomResources.None)))
                {
                    return;
                }
                else
                {
                    uint at = 1;
                    for (int crI = 1; crI < CustomResourcesCount; crI++, at <<= 1)
                    {
                        CustomResources cr = (CustomResources)at;
                        if (fieldsString.SegmentEquals(curStart, i, CustomResourcesNames[crI]))
                        {
                            SetFMResource(fm, cr, true);
                            break;
                        }
                    }
                }

                curStart = i + 1;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CommaCombineHasXFields(CustomResources resources, StringBuilder sb)
    {
        if (resources == CustomResources.None)
        {
            sb.AppendLine(nameof(CustomResources.None));
            return;
        }

        bool notEmpty = false;
        uint at = 1;
        for (int i = 1; i < CustomResourcesCount; i++, at <<= 1)
        {
            // Inline the check to be as fast as possible
            if ((resources & (CustomResources)at) != 0)
            {
                if (notEmpty) sb.Append(',');
                sb.Append(CustomResourcesNames[i]);
                notEmpty = true;
            }
        }

        sb.AppendLine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CommaCombineLanguageFlags(StringBuilder sb, Language languages)
    {
        bool notEmpty = false;
        uint at = 1;
        for (int i = 0; i < SupportedLanguageCount; i++, at <<= 1)
        {
            // Inline the check to be as fast as possible
            if ((languages & (Language)at) != 0)
            {
                if (notEmpty) sb.Append(',');
                sb.Append(SupportedLanguages[i]);
                notEmpty = true;
            }
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
        foreach (string finishedState in val.Split(CA_Comma))
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
                sb.Append(SupportedGameNames[i]);
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

    private static string TagsToString(FMCategoriesCollection tagsList, StringBuilder sb)
    {
        return FMTags.TagsToString(tagsList, writeEmptyCategories: true, sb);
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

        // TODO: Make it insert new columns at their default index (currently their position is semi-undefined)
        // Because they end up being subject to that weird-ass behavior of DisplayIndex setting where one
        // will affect the others etc...
        // If we ever add new columns, which is rarely, but still...

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

        if (!ValidDateFormats.Contains(config.DateCustomFormat1) ||
            !ValidDateFormats.Contains(config.DateCustomFormat2) ||
            !ValidDateFormats.Contains(config.DateCustomFormat3) ||
            !ValidDateFormats.Contains(config.DateCustomFormat4))
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

        // IMPORTANT: ToArray() is necessary otherwise it doesn't work (IEnumerable behavior I guess)
        // TODO: This should really be like an "Ordered HashSet" but there's no such thing I don't think
        // So we should make it a custom type like the cat and tags classes
        // Because we want it to self-dedupe, but also to keep its ordering
        config.FMArchivePaths.ClearAndAdd(config.FMArchivePaths.Distinct(new PathComparer()).ToArray());
    }

    #endregion

    #endregion
}
