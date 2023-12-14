#define FMDATA_MINIMALMEM

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
using SpanExtensions;
using static AL_Common.Common;
using static AL_Common.LanguageSupport;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;

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
                lineT.StartsWithO(nameof(LText.Meta.TranslatedLanguageName) + "="))
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

            FileInfo lastWritten = fileInfos.OrderByDescending(static x => x.LastWriteTime).ToArray()[0];
            string lastWrittenFileNumStr = lastWritten.Name.Substring(Paths.FMDataBakBase.Length);

            int newNum = 1;
            if (Int_TryParseInv(lastWrittenFileNumStr, out int lastWrittenFileNum))
            {
                newNum = lastWrittenFileNum >= maxBackups ? 1 : lastWrittenFileNum + 1;
            }

            return Path.Combine(Paths.Data, Paths.FMDataBakBase + newNum.ToStrInv());
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
                        File.Copy(Paths.FMDataIni, file, overwrite: true);
                    }
                    catch (FileNotFoundException)
                    {
                        // ignore
                    }
                    catch (Exception ex)
                    {
                        Log(ErrorText.ExCopy + "'" + Paths.FMDataIni + "' to '" + file + "'", ex);
                    }
                }
                WriteFMDataIni(FMDataIniList, FMDataIniListTDM, Paths.FMDataIni);
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

    private sealed class MemoryStringComparer : IEqualityComparer<ReadOnlyMemory<char>>
    {
        public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) => x.Span.Equals(y.Span, StringComparison.Ordinal);

        public int GetHashCode(ReadOnlyMemory<char> obj) => string.GetHashCode(obj.Span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddFMIfNotNull(FanMission? fm)
    {
        if (fm != null)
        {
            List<FanMission> list = fm.Game == Game.TDM ? FMDataIniListTDM : FMDataIniList;
            list.Add(fm);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void PopulateFMFieldFromLine(FanMission fm, string line)
    {
        ReadOnlyMemory<char> lineTS = line.AsMemory().TrimStart();
        ReadOnlySpan<char> lineTSSpan = lineTS.Span;
        int eqIndex = lineTSSpan.IndexOf('=');
        if (eqIndex > -1 && lineTSSpan[0] != ';')
        {
            if (_actionDict_FMData.TryGetValue(lineTS[..eqIndex], out var result))
            {
                ReadOnlySpan<char> valRaw = lineTSSpan[(eqIndex + 1)..];
                result.Action(fm, valRaw);
            }
        }
    }

    internal static void ReadFMDataIni(string fileName, List<FanMission> fmsList, List<FanMission> fmsListTDM)
    {
        fmsList.Clear();
        fmsListTDM.Clear();

#if FMDATA_MINIMALMEM
        using var sr = File_OpenTextFast(fileName);

        FanMission? fm = null;
        while (sr.Reader.ReadLine() is { } line)
        {
            string lineT = line.Trim();
            if (lineT == "[FM]")
            {
                AddFMIfNotNull(fm);
                fm = new FanMission();
            }
            else if (fm != null)
            {
                PopulateFMFieldFromLine(fm, line);
            }
        }
        AddFMIfNotNull(fm);
#else
        var lines = File_ReadAllLines_List(fileName);

        int linesLength = lines.Count;
        for (int i = 0; i < linesLength; i++)
        {
            string lineT = lines[i].Trim();
            if (lineT == "[FM]")
            {
                FanMission fm = new();
                while (i < linesLength - 1)
                {
                    string lineTS = lines[i + 1].TrimStart();
                    int eqIndex = lineTS.IndexOf('=');
                    if (eqIndex > -1 && lineTS[0] != ';')
                    {
                        if (_actionDict_FMData.TryGetValue(lineTS, out var result))
                        {
                            // If the value is an arbitrary string or other unknowable type, then we need to split
                            // the string so the value part can go in the FM field. But if the value is a knowable
                            // type, then we don't need to split the string, we can just parse the value section.
                            // This slashes our allocation count WAY down.
                            result.Action(fm, lineTS, eqIndex);
                        }
                    }
                    else if (lineTS.Length > 0 && lineTS[0] == '[')
                    {
                        break;
                    }
                    i++;
                }
                AddFMIfNotNull(fm);
            }
        }
#endif
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

    #region FMData

    private static void AddReadmeEncoding(FanMission fm, ReadOnlySpan<char> line)
    {
        int lastIndexOfComma = line.LastIndexOf(',');

        if (lastIndexOfComma > -1 &&
            Int_TryParseInv(line[(lastIndexOfComma + 1)..], out int result) &&
            // 0 = default, we don't want to handle "default" as it's not a specific code page
            result > 0)
        {
            string readme = line[..lastIndexOfComma].ToString();
            if (!readme.IsEmpty())
            {
                fm.ReadmeCodePages[readme.ToBackSlashes()] = result;
            }
        }
    }

    private static void SetFMLanguages(FanMission fm, ReadOnlySpan<char> langsSpan)
    {
        // It's always supposed to be ascii lowercase, so only take the allocation if it's not
        if (!langsSpan.IsAsciiLower())
        {
            langsSpan = langsSpan.ToString().ToLowerInvariant().AsSpan();
        }

        fm.Langs = Language.Default;

        foreach (ReadOnlySpan<char> item in ReadOnlySpanExtensions.Split(langsSpan, ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Langs_TryGetValue(item, 0, item.Length, out Language result))
            {
                fm.Langs |= result;
            }
        }
    }

    private static void SetFMCustomResources(FanMission fm, ReadOnlySpan<char> fieldsSpan)
    {
        // Resources must be cleared here
        fm.Resources = CustomResources.None;

        bool first = true;
        foreach (ReadOnlySpan<char> item in ReadOnlySpanExtensions.Split(fieldsSpan, ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (first && item.SequenceEqual(nameof(CustomResources.None)))
            {
                return;
            }
            else
            {
                uint at = 1;
                for (int crI = 1; crI < CustomResourcesCount; crI++, at <<= 1)
                {
                    CustomResources cr = (CustomResources)at;
                    if (item.SequenceEqual(CustomResourcesNames[crI]))
                    {
                        fm.SetResource(cr, true);
                        break;
                    }
                }
            }
            first = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CommaCombineCustomResources(CustomResources resources, StreamWriter sw)
    {
        if (resources == CustomResources.None)
        {
            sw.WriteLine(nameof(CustomResources.None));
            return;
        }

        bool notEmpty = false;
        uint at = 1;
        for (int i = 1; i < CustomResourcesCount; i++, at <<= 1)
        {
            // Inline the check to be as fast as possible
            if ((resources & (CustomResources)at) != 0)
            {
                if (notEmpty) sw.Write(',');
                sw.Write(CustomResourcesNames[i]);
                notEmpty = true;
            }
        }

        sw.WriteLine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CommaCombineLanguageFlags(StreamWriter sw, Language languages)
    {
        bool notEmpty = false;
        uint at = 1;
        for (int i = 0; i < SupportedLanguageCount; i++, at <<= 1)
        {
            // Inline the check to be as fast as possible
            if ((languages & (Language)at) != 0)
            {
                if (notEmpty) sw.Write(',');
                sw.Write(SupportedLanguages[i]);
                notEmpty = true;
            }
        }

        sw.WriteLine();
    }

    #endregion

    #region Config

    private static bool TryParseIntPair(ReadOnlySpan<char> valTrimmed, out int first, out int second)
    {
        int commaIndex = valTrimmed.IndexOf(',');
        if (commaIndex == -1)
        {
            first = 0;
            second = 0;
            return false;
        }

        bool firstExists = Int_TryParseInv(valTrimmed[..commaIndex].Trim(), out first);
        bool secondExists = Int_TryParseInv(valTrimmed[(commaIndex + 1)..].Trim(), out second);

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

    private static void AddColumn(ConfigData config, ReadOnlySpan<char> valTrimmed, Column columnType)
    {
        valTrimmed = valTrimmed.Trim(',');

        ColumnData col = new() { Id = columnType };

        int i = 0;
        foreach (ReadOnlySpan<char> part in ReadOnlySpanExtensions.Split(valTrimmed, ','))
        {
            switch (i)
            {
                case 0 when Int_TryParseInv(part, out int di):
                    col.DisplayIndex = di;
                    break;
                case 1 when Int_TryParseInv(part, out int width):
                    col.Width = width;
                    break;
                case 2:
                    col.Visible = part.EqualsTrue();
                    break;
            }
            i++;
        }
        config.Columns[(int)col.Id] = col;
    }

    private static void ReadFilterTags(ReadOnlySpan<char> tagsToAdd, FMCategoriesCollection existingTags)
    {
        if (tagsToAdd.IsWhiteSpace()) return;

        // We need slightly different logic for this vs. the FM tag adder, to wit, we support tags of the
        // form "category:" (with no tags list). This is because we allow filtering by entire category,
        // whereas we don't allow FMs to have categories with no tags in them.

        foreach (ReadOnlySpan<char> part in ReadOnlySpanExtensions.SplitAny(tagsToAdd, CA_CommaSemicolon, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!FMTags.TryGetCatAndTag(part, out string cat, out string tag) ||
                cat.IsEmpty())
            {
                continue;
            }

            if (existingTags.TryGetValue(cat, out FMTagsCollection? tagsList))
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

    private static void ReadFinishedStates(Filter filter, ReadOnlySpan<char> val)
    {
        foreach (ReadOnlySpan<char> finishedState in ReadOnlySpanExtensions.Split(val, ','))
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

    private static void CommaCombineGameFlags(StreamWriter sw, Game games)
    {
        bool notEmpty = false;

        for (int i = 0; i < SupportedGameCount; i++)
        {
            Game game = GameIndexToGame((GameIndex)i);
            if (games.HasFlagFast(game))
            {
                if (notEmpty) sw.Write(',');
                sw.Write(SupportedGameNames[i]);
                notEmpty = true;
            }
        }

        sw.WriteLine();
    }

    private static void CommaCombineFinishedStates(StreamWriter sw, FinishedState finished)
    {
        bool notEmpty = false;

        if (finished.HasFlagFast(FinishedState.Finished))
        {
            sw.Write(nameof(FinishedState.Finished));
            notEmpty = true;
        }
        if (finished.HasFlagFast(FinishedState.Unfinished))
        {
            if (notEmpty) sw.Write(',');
            sw.Write(nameof(FinishedState.Unfinished));
        }

        sw.WriteLine();
    }

    private static string FilterDate(DateTime? dt) => dt == null
        ? ""
        : new DateTimeOffset((DateTime)dt).ToUnixTimeSeconds().ToString("X", CultureInfo.InvariantCulture);

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

        if (!ValidDateFormats.Contains(config.DateCustomFormat1, StringComparer.Ordinal) ||
            !ValidDateFormats.Contains(config.DateCustomFormat2, StringComparer.Ordinal) ||
            !ValidDateFormats.Contains(config.DateCustomFormat3, StringComparer.Ordinal) ||
            !ValidDateFormats.Contains(config.DateCustomFormat4, StringComparer.Ordinal))
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
