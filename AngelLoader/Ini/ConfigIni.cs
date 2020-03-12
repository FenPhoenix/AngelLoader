#define FenGen_ConfigDest

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.GameSupport.GameIndex;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    // TODO: Maybe make this file have sections, cause it's getting pretty giant-blob-like

    // A note about floats:
    // When storing and reading floats, it's imperative that we specify InvariantInfo. Otherwise the decimal
    // separator is culture-dependent, and it could end up as ',' when we expect '.'. And then we could end up
    // with "5.001" being "5,001", and now we're in for a bad time.

    internal static partial class Ini
    {
        // Not autogenerating these, because there's too many special cases, and adding stuff by hand is not that
        // big of a deal really.

        private static ColumnData? ConvertStringToColumnData(string str)
        {
            str = str.Trim().Trim(CA_Comma);

            // DisplayIndex,Width,Visible
            // 0,100,True

            if (str.CountChars(',') == 0) return null;

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

        private static void ReadTags(string line, Filter filter, string prefix)
        {
            CatAndTagsList? tagsList =
                line.StartsWithFast_NoNullChecks(prefix + "FilterTagsAnd=") ? filter.Tags.AndTags :
                line.StartsWithFast_NoNullChecks(prefix + "FilterTagsOr=") ? filter.Tags.OrTags :
                line.StartsWithFast_NoNullChecks(prefix + "FilterTagsNot=") ? filter.Tags.NotTags :
                null;

            string val = line.Substring(line.IndexOf('=') + 1);

            if (tagsList == null || val.IsWhiteSpace()) return;

            string[] tagsArray = val.Split(CA_CommaSemicolon, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in tagsArray)
            {
                string cat, tag;
                int colonCount = item.CountChars(':');
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

        // I tried removing the reflection in this one and it measured no faster, so leaving it as is.
        internal static void ReadConfigIni(string path, ConfigData config)
        {
            string[] iniLines = File.ReadAllLines(path);

            foreach (string line in iniLines)
            {
                if (!line.Contains('=')) continue;

                string lineT = line.TrimStart();

                if (lineT.Length > 0 && (lineT[0] == ';' || lineT[0] == '[')) continue;

                string val = lineT.Substring(lineT.IndexOf('=') + 1);

                if (lineT.StartsWithFast_NoNullChecks("Column") && line[6] != '=')
                {
                    string colName = lineT.Substring(6, lineT.IndexOf('=') - 6);

                    var field = typeof(Column).GetField(colName, BFlagsEnum);
                    if (field == null) continue;

                    ColumnData? col = ConvertStringToColumnData(val);
                    if (col == null) continue;

                    col.Id = (Column)field.GetValue(null);

                    static bool ContainsColWithId(ConfigData _config, ColumnData _col)
                    {
                        foreach (ColumnData x in _config.Columns) if (x.Id == _col.Id) return true;
                        return false;
                    }

                    if (!ContainsColWithId(config, col)) config.Columns.Add(col);
                }
                #region Filter
                // @GENGAMES
                else if (lineT.StartsWithFast_NoNullChecks("FilterTitle="))
                {
                    config.Filter.Title = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterTitle="))
                {
                    config.GameTabsState.GetFilter(Thief1).Title = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterTitle="))
                {
                    config.GameTabsState.GetFilter(Thief2).Title = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterTitle="))
                {
                    config.GameTabsState.GetFilter(Thief3).Title = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2FilterTitle="))
                {
                    config.GameTabsState.GetFilter(SS2).Title = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterAuthor="))
                {
                    config.Filter.Author = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterAuthor="))
                {
                    config.GameTabsState.GetFilter(Thief1).Author = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterAuthor="))
                {
                    config.GameTabsState.GetFilter(Thief2).Author = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterAuthor="))
                {
                    config.GameTabsState.GetFilter(Thief3).Author = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2FilterAuthor="))
                {
                    config.GameTabsState.GetFilter(SS2).Author = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterReleaseDateFrom="))
                {
                    config.Filter.ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterReleaseDateFrom="))
                {
                    config.GameTabsState.GetFilter(Thief1).ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterReleaseDateFrom="))
                {
                    config.GameTabsState.GetFilter(Thief2).ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterReleaseDateFrom="))
                {
                    config.GameTabsState.GetFilter(Thief3).ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2FilterReleaseDateFrom="))
                {
                    config.GameTabsState.GetFilter(SS2).ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterReleaseDateTo="))
                {
                    config.Filter.ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterReleaseDateTo="))
                {
                    config.GameTabsState.GetFilter(Thief1).ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterReleaseDateTo="))
                {
                    config.GameTabsState.GetFilter(Thief2).ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterReleaseDateTo="))
                {
                    config.GameTabsState.GetFilter(Thief3).ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2FilterReleaseDateTo="))
                {
                    config.GameTabsState.GetFilter(SS2).ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterLastPlayedFrom="))
                {
                    config.Filter.LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterLastPlayedFrom="))
                {
                    config.GameTabsState.GetFilter(Thief1).LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterLastPlayedFrom="))
                {
                    config.GameTabsState.GetFilter(Thief2).LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterLastPlayedFrom="))
                {
                    config.GameTabsState.GetFilter(Thief3).LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2FilterLastPlayedFrom="))
                {
                    config.GameTabsState.GetFilter(SS2).LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterLastPlayedTo="))
                {
                    config.Filter.LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterLastPlayedTo="))
                {
                    config.GameTabsState.GetFilter(Thief1).LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterLastPlayedTo="))
                {
                    config.GameTabsState.GetFilter(Thief2).LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterLastPlayedTo="))
                {
                    config.GameTabsState.GetFilter(Thief3).LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2FilterLastPlayedTo="))
                {
                    config.GameTabsState.GetFilter(SS2).LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                // Note: These lines can't index past the end, because we won't get here unless the line contains
                // '=' and since there are no '=' chars in the checked strings, we know the length must be at least
                // checked string length + 1
                else if (lineT.StartsWithFast_NoNullChecks("FilterTags") && line[10] != '=')
                {
                    ReadTags(lineT, config.Filter, "");
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterTags") && line[12] != '=')
                {
                    ReadTags(lineT, config.GameTabsState.GetFilter(Thief1), "T1");
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterTags") && line[12] != '=')
                {
                    ReadTags(lineT, config.GameTabsState.GetFilter(Thief2), "T2");
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterTags") && line[12] != '=')
                {
                    ReadTags(lineT, config.GameTabsState.GetFilter(Thief3), "T3");
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2FilterTags") && line[13] != '=')
                {
                    ReadTags(lineT, config.GameTabsState.GetFilter(SS2), "SS2");
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterGames="))
                {
                    var list = val.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries)
                        .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                    foreach (string game in list)
                    {
                        // TODO: @GENGAMES: Faster to do it manually
                        switch (game.Trim())
                        {
                            case nameof(Game.Thief1):
                                config.Filter.Games |= Game.Thief1;
                                break;
                            case nameof(Game.Thief2):
                                config.Filter.Games |= Game.Thief2;
                                break;
                            case nameof(Game.Thief3):
                                config.Filter.Games |= Game.Thief3;
                                break;
                            case nameof(Game.SS2):
                                config.Filter.Games |= Game.SS2;
                                break;
                        }
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.Filter.RatingFrom = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief1).RatingFrom = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief2).RatingFrom = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief3).RatingFrom = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(SS2).RatingFrom = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.Filter.RatingTo = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief1).RatingTo = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief2).RatingTo = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief3).RatingTo = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(SS2).RatingTo = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.Filter);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.GetFilter(Thief1));
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.GetFilter(Thief2));
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.GetFilter(Thief3));
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.GetFilter(SS2));
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterShowJunk="))
                {
                    config.Filter.ShowUnsupported = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterShowJunk="))
                {
                    config.GameTabsState.GetFilter(Thief1).ShowUnsupported = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterShowJunk="))
                {
                    config.GameTabsState.GetFilter(Thief2).ShowUnsupported = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterShowJunk="))
                {
                    config.GameTabsState.GetFilter(Thief3).ShowUnsupported = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2FilterShowJunk="))
                {
                    config.GameTabsState.GetFilter(SS2).ShowUnsupported = val.EqualsTrue();
                }

                #endregion
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.EnableArticles) + "="))
                {
                    config.EnableArticles = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.Articles) + "="))
                {
                    string[] articles = val.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);
                    for (int a = 0; a < articles.Length; a++) articles[a] = articles[a].Trim();
                    config.Articles.ClearAndAdd(articles.Distinct(StringComparer.OrdinalIgnoreCase));
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.MoveArticlesToEnd) + "="))
                {
                    config.MoveArticlesToEnd = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.SortDirection) + "="))
                {
                    if (val.EqualsI("Ascending"))
                    {
                        config.SortDirection = SortOrder.Ascending;
                    }
                    else if (val.EqualsI("Descending"))
                    {
                        config.SortDirection = SortOrder.Descending;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.SortedColumn) + "="))
                {
                    var field = typeof(Column).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.SortedColumn = (Column)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.ShowRecentAtTop) + "="))
                {
                    config.ShowRecentAtTop = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.FMsListFontSizeInPoints) + "="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.FMsListFontSizeInPoints = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.RatingDisplayStyle) + "="))
                {
                    var field = typeof(RatingDisplayStyle).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.RatingDisplayStyle = (RatingDisplayStyle)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.RatingUseStars) + "="))
                {
                    config.RatingUseStars = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("TopRightTab="))
                {
                    var field = typeof(TopRightTab).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.TopRightTabsData.SelectedTab = (TopRightTab)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.SettingsTab) + "="))
                {
                    var field = typeof(SettingsTab).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.SettingsTab = (SettingsTab)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.SettingsWindowSize) + "="))
                {
                    if (!val.Contains(',')) continue;

                    string[] values = val.Split(CA_Comma);
                    bool widthExists = int.TryParse(values[0].Trim(), out int width);
                    bool heightExists = int.TryParse(values[1].Trim(), out int height);

                    if (widthExists && heightExists)
                    {
                        config.SettingsWindowSize = new Size(width, height);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.SettingsWindowSplitterDistance) + "="))
                {
                    if (int.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int result))
                    {
                        config.SettingsWindowSplitterDistance = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.SettingsPathsVScrollPos) + "="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SettingsPathsVScrollPos = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.SettingsFMDisplayVScrollPos) + "="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SettingsFMDisplayVScrollPos = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.SettingsOtherVScrollPos) + "="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SettingsOtherVScrollPos = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("FMArchivePath="))
                {
                    config.FMArchivePaths.Add(val.ToSystemDirSeps().Trim());
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.FMArchivePathsIncludeSubfolders) + "="))
                {
                    config.FMArchivePathsIncludeSubfolders = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.FMsBackupPath) + "="))
                {
                    config.FMsBackupPath = val.Trim();
                }
                #region Steam
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.LaunchGamesWithSteam) + "="))
                {
                    config.LaunchGamesWithSteam = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1UseSteam="))
                {
                    config.SetUseSteamSwitch(Thief1, val.EqualsTrue());
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2UseSteam="))
                {
                    config.SetUseSteamSwitch(Thief2, val.EqualsTrue());
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3UseSteam="))
                {
                    config.SetUseSteamSwitch(Thief3, val.EqualsTrue());
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2UseSteam="))
                {
                    config.SetUseSteamSwitch(SS2, val.EqualsTrue());
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.SteamExe) + "="))
                {
                    config.SteamExe = val.Trim();
                }
                #endregion
                else if (lineT.StartsWithFast_NoNullChecks("T1Exe="))
                {
                    config.SetGameExe(Thief1, val.Trim());
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2Exe="))
                {
                    config.SetGameExe(Thief2, val.Trim());
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3Exe="))
                {
                    config.SetGameExe(Thief3, val.Trim());
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2Exe="))
                {
                    config.SetGameExe(SS2, val.Trim());
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.GameOrganization) + "="))
                {
                    var field = typeof(GameOrganization).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.GameOrganization = (GameOrganization)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.UseShortGameTabNames) + "="))
                {
                    config.UseShortGameTabNames = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.GameTab) + "="))
                {
                    config.GameTab = val switch
                    {
                        nameof(Thief2) => Thief2,
                        nameof(Thief3) => Thief3,
                        nameof(SS2) => SS2,
                        _ => Thief1
                    };
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1SelFMInstDir="))
                {
                    config.GameTabsState.GetSelectedFM(Thief1).InstalledName = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2SelFMInstDir="))
                {
                    config.GameTabsState.GetSelectedFM(Thief2).InstalledName = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3SelFMInstDir="))
                {
                    config.GameTabsState.GetSelectedFM(Thief3).InstalledName = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2SelFMInstDir="))
                {
                    config.GameTabsState.GetSelectedFM(SS2).InstalledName = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.GetSelectedFM(Thief1).IndexFromTop = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.GetSelectedFM(Thief2).IndexFromTop = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.GetSelectedFM(Thief3).IndexFromTop = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("SS2SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.GetSelectedFM(SS2).IndexFromTop = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("SelFMInstDir="))
                {
                    config.SelFM.InstalledName = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SelFM.IndexFromTop = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.DateFormat) + "="))
                {
                    var field = typeof(DateFormat).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.DateFormat = (DateFormat)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.DateCustomFormat1) + "="))
                {
                    config.DateCustomFormat1 = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.DateCustomSeparator1) + "="))
                {
                    config.DateCustomSeparator1 = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.DateCustomFormat2) + "="))
                {
                    config.DateCustomFormat2 = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.DateCustomSeparator2) + "="))
                {
                    config.DateCustomSeparator2 = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.DateCustomFormat3) + "="))
                {
                    config.DateCustomFormat3 = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.DateCustomSeparator3) + "="))
                {
                    config.DateCustomSeparator3 = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.DateCustomFormat4) + "="))
                {
                    config.DateCustomFormat4 = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.DaysRecent) + "="))
                {
                    if (uint.TryParse(val, out uint result)) config.DaysRecent = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.ReadmeZoomFactor) + "="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.ReadmeZoomFactor = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.ReadmeUseFixedWidthFont) + "="))
                {
                    config.ReadmeUseFixedWidthFont = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.MainWindowState) + "="))
                {
                    var field = typeof(FormWindowState).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        var windowState = (FormWindowState)field.GetValue(null);
                        if (windowState != FormWindowState.Minimized)
                        {
                            config.MainWindowState = windowState;
                        }
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.MainWindowSize) + "="))
                {
                    if (!val.Contains(',')) continue;

                    string[] values = val.Split(CA_Comma);
                    bool widthExists = int.TryParse(values[0].Trim(), out int width);
                    bool heightExists = int.TryParse(values[1].Trim(), out int height);

                    if (widthExists && heightExists)
                    {
                        config.MainWindowSize = new Size(width, height);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.MainWindowLocation) + "="))
                {
                    if (!val.Contains(',')) continue;

                    string[] values = val.Split(CA_Comma);
                    bool xExists = int.TryParse(values[0].Trim(), out int x);
                    bool yExists = int.TryParse(values[1].Trim(), out int y);

                    if (xExists && yExists)
                    {
                        config.MainWindowLocation = new Point(x, y);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.MainSplitterPercent) + "="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.MainSplitterPercent = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.TopSplitterPercent) + "="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.TopSplitterPercent = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.TopRightPanelCollapsed) + "="))
                {
                    config.TopRightPanelCollapsed = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.ConvertWAVsTo16BitOnInstall) + "="))
                {
                    config.ConvertWAVsTo16BitOnInstall = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.ConvertOGGsToWAVsOnInstall) + "="))
                {
                    config.ConvertOGGsToWAVsOnInstall = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.HideUninstallButton) + "="))
                {
                    config.HideUninstallButton = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.HideFMListZoomButtons) + "="))
                {
                    config.HideFMListZoomButtons = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.ConfirmUninstall) + "="))
                {
                    config.ConfirmUninstall = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.BackupFMData) + "="))
                {
                    var field = typeof(BackupFMData).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.BackupFMData = (BackupFMData)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.BackupAlwaysAsk) + "="))
                {
                    config.BackupAlwaysAsk = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.Language) + "="))
                {
                    config.Language = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.WebSearchUrl) + "="))
                {
                    config.WebSearchUrl = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.ConfirmPlayOnDCOrEnter) + "="))
                {
                    config.ConfirmPlayOnDCOrEnter = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("StatsTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.StatsTab.Position = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("EditFMTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.EditFMTab.Position = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("CommentTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.CommentTab.Position = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("TagsTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.TagsTab.Position = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("PatchTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.PatchTab.Position = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("StatsTabVisible="))
                {
                    config.TopRightTabsData.StatsTab.Visible = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("EditFMTabVisible="))
                {
                    config.TopRightTabsData.EditFMTab.Visible = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("CommentTabVisible="))
                {
                    config.TopRightTabsData.CommentTab.Visible = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("TagsTabVisible="))
                {
                    config.TopRightTabsData.TagsTab.Visible = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("PatchTabVisible="))
                {
                    config.TopRightTabsData.PatchTab.Visible = val.EqualsTrue();
                }
            }

            // Vital, don't remove
            config.TopRightTabsData.EnsureValidity();

            string sep1 = config.DateCustomSeparator1.EscapeAllChars();
            string sep2 = config.DateCustomSeparator2.EscapeAllChars();
            string sep3 = config.DateCustomSeparator3.EscapeAllChars();

            string formatString = config.DateCustomFormat1 +
                               sep1 +
                               config.DateCustomFormat2 +
                               sep2 +
                               config.DateCustomFormat3 +
                               sep3 +
                               config.DateCustomFormat4;

            try
            {
                // PERF: Passing an explicit DateTimeFormatInfo avoids a 5ms(!) hit that you take otherwise.
                // Man, DateTime and culture stuff is SLOW.
                _ = new DateTime(2000, 1, 1).ToString(formatString, DateTimeFormatInfo.InvariantInfo);
                config.DateCustomFormatString = formatString;
            }
            catch (FormatException)
            {
                config.DateFormat = DateFormat.CurrentCultureShort;
            }
            catch (ArgumentOutOfRangeException)
            {
                config.DateFormat = DateFormat.CurrentCultureShort;
            }
        }

        // This is faster with reflection removed.
        // @GENGAMES
        internal static void WriteConfigIni(ConfigData config, string fileName)
        {
            static string commaCombine<T>(List<T> list) where T : notnull
            {
                string ret = "";
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) ret += ",";
                    ret += list[i].ToString();
                }

                return ret;
            }

            #region Enum-specific commaCombine() methods

            // TODO: Figure out a better way to be fast without this dopey manual code. Code generation?

            static string commaCombineGameFlags(Game games)
            {
                string ret = "";

                // Hmm... doesn't make for good code, but fast...
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

                return ret;
            }

            static string commaCombineFinishedStates(FinishedState finished)
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

            #endregion

            StreamWriter? sw = null;
            try
            {
                sw = new StreamWriter(fileName, false, Encoding.UTF8);

                #region Settings window

                #region Settings window state

                sw.WriteLine(nameof(config.SettingsTab) + "=" + config.SettingsTab);
                sw.WriteLine(nameof(config.SettingsWindowSize) + "=" + config.SettingsWindowSize.Width + "," + config.SettingsWindowSize.Height);
                sw.WriteLine(nameof(config.SettingsWindowSplitterDistance) + "=" + config.SettingsWindowSplitterDistance);

                sw.WriteLine(nameof(config.SettingsPathsVScrollPos) + "=" + config.SettingsPathsVScrollPos);
                sw.WriteLine(nameof(config.SettingsFMDisplayVScrollPos) + "=" + config.SettingsFMDisplayVScrollPos);
                sw.WriteLine(nameof(config.SettingsOtherVScrollPos) + "=" + config.SettingsOtherVScrollPos);

                #endregion

                #region Paths

                sw.WriteLine("T1Exe=" + config.GetGameExe(Thief1).Trim());
                sw.WriteLine("T2Exe=" + config.GetGameExe(Thief2).Trim());
                sw.WriteLine("T3Exe=" + config.GetGameExe(Thief3).Trim());
                sw.WriteLine("SS2Exe=" + config.GetGameExe(SS2).Trim());

                sw.WriteLine(nameof(config.LaunchGamesWithSteam) + "=" + config.LaunchGamesWithSteam);
                sw.WriteLine("T1UseSteam=" + config.GetUseSteamSwitch(Thief1));
                sw.WriteLine("T2UseSteam=" + config.GetUseSteamSwitch(Thief2));
                sw.WriteLine("T3UseSteam=" + config.GetUseSteamSwitch(Thief3));
                sw.WriteLine("SS2UseSteam=" + config.GetUseSteamSwitch(SS2));
                sw.WriteLine(nameof(config.SteamExe) + "=" + config.SteamExe);

                sw.WriteLine(nameof(config.FMsBackupPath) + "=" + config.FMsBackupPath.Trim());
                foreach (string path in config.FMArchivePaths) sw.WriteLine("FMArchivePath=" + path.Trim());
                sw.WriteLine(nameof(config.FMArchivePathsIncludeSubfolders) + "=" + config.FMArchivePathsIncludeSubfolders);

                #endregion

                sw.WriteLine(nameof(config.GameOrganization) + "=" + config.GameOrganization);
                sw.WriteLine(nameof(config.UseShortGameTabNames) + "=" + config.UseShortGameTabNames);

                sw.WriteLine(nameof(config.EnableArticles) + "=" + config.EnableArticles);
                sw.WriteLine(nameof(config.Articles) + "=" + commaCombine(config.Articles));
                sw.WriteLine(nameof(config.MoveArticlesToEnd) + "=" + config.MoveArticlesToEnd);

                sw.WriteLine(nameof(config.RatingDisplayStyle) + "=" + config.RatingDisplayStyle);
                sw.WriteLine(nameof(config.RatingUseStars) + "=" + config.RatingUseStars);

                sw.WriteLine(nameof(config.DateFormat) + "=" + config.DateFormat);
                sw.WriteLine(nameof(config.DateCustomFormat1) + "=" + config.DateCustomFormat1);
                sw.WriteLine(nameof(config.DateCustomSeparator1) + "=" + config.DateCustomSeparator1);
                sw.WriteLine(nameof(config.DateCustomFormat2) + "=" + config.DateCustomFormat2);
                sw.WriteLine(nameof(config.DateCustomSeparator2) + "=" + config.DateCustomSeparator2);
                sw.WriteLine(nameof(config.DateCustomFormat3) + "=" + config.DateCustomFormat3);
                sw.WriteLine(nameof(config.DateCustomSeparator3) + "=" + config.DateCustomSeparator3);
                sw.WriteLine(nameof(config.DateCustomFormat4) + "=" + config.DateCustomFormat4);

                sw.WriteLine(nameof(config.DaysRecent) + "=" + config.DaysRecent);

                sw.WriteLine(nameof(config.ConvertWAVsTo16BitOnInstall) + "=" + config.ConvertWAVsTo16BitOnInstall);
                sw.WriteLine(nameof(config.ConvertOGGsToWAVsOnInstall) + "=" + config.ConvertOGGsToWAVsOnInstall);
                sw.WriteLine(nameof(config.HideUninstallButton) + "=" + config.HideUninstallButton);
                sw.WriteLine(nameof(config.HideFMListZoomButtons) + "=" + config.HideFMListZoomButtons);
                sw.WriteLine(nameof(config.ConfirmUninstall) + "=" + config.ConfirmUninstall);
                sw.WriteLine(nameof(config.BackupFMData) + "=" + config.BackupFMData);
                sw.WriteLine(nameof(config.BackupAlwaysAsk) + "=" + config.BackupAlwaysAsk);
                sw.WriteLine(nameof(config.Language) + "=" + config.Language);
                sw.WriteLine(nameof(config.WebSearchUrl) + "=" + config.WebSearchUrl);
                sw.WriteLine(nameof(config.ConfirmPlayOnDCOrEnter) + "=" + config.ConfirmPlayOnDCOrEnter);

                #endregion

                #region Filters

                static string FilterDate(DateTime? dt) => dt == null
                    ? ""
                    : new DateTimeOffset((DateTime)dt).ToUnixTimeSeconds().ToString("X");

                for (int fi = 0; fi < 5; fi++)
                {
                    #region Set i-dependent values

                    // @GENGAMES: Manual because we need a 0th option for just the global filter
                    Filter filter = fi switch
                    {
                        0 => config.Filter,
                        1 => config.GameTabsState.GetFilter(Thief1),
                        2 => config.GameTabsState.GetFilter(Thief2),
                        3 => config.GameTabsState.GetFilter(Thief3),
                        _ => config.GameTabsState.GetFilter(SS2)
                    };

                    string p = fi switch { 0 => "", 1 => "T1", 2 => "T2", 3 => "T3", _ => "SS2" };

                    #endregion

                    if (fi == 0) sw.WriteLine("FilterGames=" + commaCombineGameFlags(config.Filter.Games));

                    sw.WriteLine(p + "FilterTitle=" + filter.Title);
                    sw.WriteLine(p + "FilterAuthor=" + filter.Author);

                    sw.WriteLine(p + "FilterReleaseDateFrom=" + FilterDate(filter.ReleaseDateFrom));
                    sw.WriteLine(p + "FilterReleaseDateTo=" + FilterDate(filter.ReleaseDateTo));

                    sw.WriteLine(p + "FilterLastPlayedFrom=" + FilterDate(filter.LastPlayedFrom));
                    sw.WriteLine(p + "FilterLastPlayedTo=" + FilterDate(filter.LastPlayedTo));

                    sw.WriteLine(p + "FilterFinishedStates=" + commaCombineFinishedStates(filter.Finished));

                    sw.WriteLine(p + "FilterRatingFrom=" + filter.RatingFrom);
                    sw.WriteLine(p + "FilterRatingTo=" + filter.RatingTo);

                    sw.WriteLine(p + "FilterShowJunk=" + filter.ShowUnsupported);

                    #region Tags

                    static string TagsToString(CatAndTagsList tagsList)
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

                    sw.WriteLine(p + "FilterTagsAnd=" + TagsToString(filter.Tags.AndTags));
                    sw.WriteLine(p + "FilterTagsOr=" + TagsToString(filter.Tags.OrTags));
                    sw.WriteLine(p + "FilterTagsNot=" + TagsToString(filter.Tags.NotTags));
                }

                #endregion

                #endregion

                #region Columns

                sw.WriteLine(nameof(config.SortedColumn) + "=" + config.SortedColumn);
                sw.WriteLine(nameof(config.SortDirection) + "=" + config.SortDirection);
                sw.WriteLine(nameof(config.ShowRecentAtTop) + "=" + config.ShowRecentAtTop);
                sw.WriteLine(nameof(config.FMsListFontSizeInPoints) + "=" + config.FMsListFontSizeInPoints.ToString(NumberFormatInfo.InvariantInfo));

                foreach (ColumnData col in config.Columns)
                {
                    sw.WriteLine("Column" + col.Id + "=" + col.DisplayIndex + "," + col.Width + "," + col.Visible);
                }

                #endregion

                #region Selected FM

                sw.WriteLine("SelFMInstDir=" + config.SelFM.InstalledName);
                sw.WriteLine("SelFMIndexFromTop=" + config.SelFM.IndexFromTop);
                sw.WriteLine("T1SelFMInstDir=" + config.GameTabsState.GetSelectedFM(Thief1).InstalledName);
                sw.WriteLine("T1SelFMIndexFromTop=" + config.GameTabsState.GetSelectedFM(Thief1).IndexFromTop);
                sw.WriteLine("T2SelFMInstDir=" + config.GameTabsState.GetSelectedFM(Thief2).InstalledName);
                sw.WriteLine("T2SelFMIndexFromTop=" + config.GameTabsState.GetSelectedFM(Thief2).IndexFromTop);
                sw.WriteLine("T3SelFMInstDir=" + config.GameTabsState.GetSelectedFM(Thief3).InstalledName);
                sw.WriteLine("T3SelFMIndexFromTop=" + config.GameTabsState.GetSelectedFM(Thief3).IndexFromTop);
                sw.WriteLine("SS2SelFMInstDir=" + config.GameTabsState.GetSelectedFM(SS2).InstalledName);
                sw.WriteLine("SS2SelFMIndexFromTop=" + config.GameTabsState.GetSelectedFM(SS2).IndexFromTop);

                #endregion

                #region Main window state

                sw.WriteLine(nameof(config.MainWindowState) + "=" +
                             (config.MainWindowState == FormWindowState.Minimized
                                 ? FormWindowState.Maximized
                                 : config.MainWindowState));

                sw.WriteLine(nameof(config.MainWindowSize) + "=" + config.MainWindowSize.Width + "," + config.MainWindowSize.Height);
                sw.WriteLine(nameof(config.MainWindowLocation) + "=" + config.MainWindowLocation.X + "," + config.MainWindowLocation.Y);

                sw.WriteLine(nameof(config.MainSplitterPercent) + "=" + config.MainSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
                sw.WriteLine(nameof(config.TopSplitterPercent) + "=" + config.TopSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
                sw.WriteLine(nameof(config.TopRightPanelCollapsed) + "=" + config.TopRightPanelCollapsed);

                sw.WriteLine(nameof(config.GameTab) + "=" + config.GameTab);
                sw.WriteLine("TopRightTab=" + config.TopRightTabsData.SelectedTab);

                sw.WriteLine("StatsTabPosition=" + config.TopRightTabsData.StatsTab.Position);
                sw.WriteLine("EditFMTabPosition=" + config.TopRightTabsData.EditFMTab.Position);
                sw.WriteLine("CommentTabPosition=" + config.TopRightTabsData.CommentTab.Position);
                sw.WriteLine("TagsTabPosition=" + config.TopRightTabsData.TagsTab.Position);
                sw.WriteLine("PatchTabPosition=" + config.TopRightTabsData.PatchTab.Position);

                sw.WriteLine("StatsTabVisible=" + config.TopRightTabsData.StatsTab.Visible);
                sw.WriteLine("EditFMTabVisible=" + config.TopRightTabsData.EditFMTab.Visible);
                sw.WriteLine("CommentTabVisible=" + config.TopRightTabsData.CommentTab.Visible);
                sw.WriteLine("TagsTabVisible=" + config.TopRightTabsData.TagsTab.Visible);
                sw.WriteLine("PatchTabVisible=" + config.TopRightTabsData.PatchTab.Visible);

                sw.WriteLine(nameof(config.ReadmeZoomFactor) + "=" + config.ReadmeZoomFactor.ToString(NumberFormatInfo.InvariantInfo));
                sw.WriteLine(nameof(config.ReadmeUseFixedWidthFont) + "=" + config.ReadmeUseFixedWidthFont);

                #endregion
            }
            catch (Exception ex)
            {
                Log("There was an error while writing to " + Paths.ConfigIni + ".", ex);
            }
            finally
            {
                sw?.Dispose();
            }
        }
    }
}
