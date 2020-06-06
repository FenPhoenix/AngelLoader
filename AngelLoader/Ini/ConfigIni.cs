#define FenGen_ConfigDest

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.GameSupport.GameIndex;
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

        // I tried removing the reflection in this one and it measured no faster, so leaving it as is.
        internal static void ReadConfigIni(string path, ConfigData config)
        {
            string[] iniLines = File.ReadAllLines(path);

            for (int li = 0; li < iniLines.Length; li++)
            {
                // PERF: It's okay to do a TrimStart() right off the bat since blank lines are the rare case.
                // And having only one (trimmed) line string prevents us from accidentally using the un-trimmed
                // one by accident like we previously did in here.
                string lineTS = iniLines[li].TrimStart();
                if (!lineTS.Contains('=')) continue;

                if (lineTS.Length > 0 && (lineTS[0] == ';' || lineTS[0] == '[')) continue;

                string val = lineTS.Substring(lineTS.IndexOf('=') + 1);

                if (lineTS.StartsWithFast_NoNullChecks("Column") && lineTS[6] != '=')
                {
                    string colName = lineTS.Substring(6, lineTS.IndexOf('=') - 6);

                    var field = typeof(Column).GetField(colName, BFlagsEnum);
                    if (field == null) continue;

                    ColumnData? col = ConvertStringToColumnData(val);
                    if (col == null) continue;

                    col.Id = (Column)field.GetValue(null);

                    if (!ContainsColWithId(config, col)) config.Columns.Add(col);
                }
                #region Filter
                // @GENGAMES (Config reader - Filter): Begin
                else if (lineTS.StartsWithFast_NoNullChecks("FilterTitle="))
                {
                    config.Filter.Title = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1FilterTitle="))
                {
                    config.GameTabsState.GetFilter(Thief1).Title = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2FilterTitle="))
                {
                    config.GameTabsState.GetFilter(Thief2).Title = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3FilterTitle="))
                {
                    config.GameTabsState.GetFilter(Thief3).Title = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2FilterTitle="))
                {
                    config.GameTabsState.GetFilter(SS2).Title = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterAuthor="))
                {
                    config.Filter.Author = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1FilterAuthor="))
                {
                    config.GameTabsState.GetFilter(Thief1).Author = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2FilterAuthor="))
                {
                    config.GameTabsState.GetFilter(Thief2).Author = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3FilterAuthor="))
                {
                    config.GameTabsState.GetFilter(Thief3).Author = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2FilterAuthor="))
                {
                    config.GameTabsState.GetFilter(SS2).Author = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterReleaseDateFrom="))
                {
                    config.Filter.ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1FilterReleaseDateFrom="))
                {
                    config.GameTabsState.GetFilter(Thief1).ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2FilterReleaseDateFrom="))
                {
                    config.GameTabsState.GetFilter(Thief2).ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3FilterReleaseDateFrom="))
                {
                    config.GameTabsState.GetFilter(Thief3).ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2FilterReleaseDateFrom="))
                {
                    config.GameTabsState.GetFilter(SS2).ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterReleaseDateTo="))
                {
                    config.Filter.ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1FilterReleaseDateTo="))
                {
                    config.GameTabsState.GetFilter(Thief1).ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2FilterReleaseDateTo="))
                {
                    config.GameTabsState.GetFilter(Thief2).ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3FilterReleaseDateTo="))
                {
                    config.GameTabsState.GetFilter(Thief3).ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2FilterReleaseDateTo="))
                {
                    config.GameTabsState.GetFilter(SS2).ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterLastPlayedFrom="))
                {
                    config.Filter.LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1FilterLastPlayedFrom="))
                {
                    config.GameTabsState.GetFilter(Thief1).LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2FilterLastPlayedFrom="))
                {
                    config.GameTabsState.GetFilter(Thief2).LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3FilterLastPlayedFrom="))
                {
                    config.GameTabsState.GetFilter(Thief3).LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2FilterLastPlayedFrom="))
                {
                    config.GameTabsState.GetFilter(SS2).LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterLastPlayedTo="))
                {
                    config.Filter.LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1FilterLastPlayedTo="))
                {
                    config.GameTabsState.GetFilter(Thief1).LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2FilterLastPlayedTo="))
                {
                    config.GameTabsState.GetFilter(Thief2).LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3FilterLastPlayedTo="))
                {
                    config.GameTabsState.GetFilter(Thief3).LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2FilterLastPlayedTo="))
                {
                    config.GameTabsState.GetFilter(SS2).LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                // Note: These lines can't index past the end, because we won't get here unless the line contains
                // '=' and since there are no '=' chars in the checked strings, we know the length must be at least
                // checked string length + 1
                // TODO: This is downright dangerous, having not one but two int literals per. Be EXTREMELY careful if modifying these!
                else if (lineTS.StartsWithFast_NoNullChecks("FilterTags") && lineTS[10] != '=')
                {
                    ReadTags(lineTS.Substring(10), config.Filter);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1FilterTags") && lineTS[12] != '=')
                {
                    ReadTags(lineTS.Substring(12), config.GameTabsState.GetFilter(Thief1));
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2FilterTags") && lineTS[12] != '=')
                {
                    ReadTags(lineTS.Substring(12), config.GameTabsState.GetFilter(Thief2));
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3FilterTags") && lineTS[12] != '=')
                {
                    ReadTags(lineTS.Substring(12), config.GameTabsState.GetFilter(Thief3));
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2FilterTags") && lineTS[13] != '=')
                {
                    ReadTags(lineTS.Substring(13), config.GameTabsState.GetFilter(SS2));
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterGames="))
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
                else if (lineTS.StartsWithFast_NoNullChecks("FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.Filter.RatingFrom = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief1).RatingFrom = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief2).RatingFrom = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief3).RatingFrom = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(SS2).RatingFrom = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.Filter.RatingTo = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief1).RatingTo = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief2).RatingTo = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(Thief3).RatingTo = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.GetFilter(SS2).RatingTo = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.Filter);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.GetFilter(Thief1));
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.GetFilter(Thief2));
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.GetFilter(Thief3));
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.GetFilter(SS2));
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterShowJunk="))
                {
                    config.Filter.ShowUnsupported = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1FilterShowJunk="))
                {
                    config.GameTabsState.GetFilter(Thief1).ShowUnsupported = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2FilterShowJunk="))
                {
                    config.GameTabsState.GetFilter(Thief2).ShowUnsupported = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3FilterShowJunk="))
                {
                    config.GameTabsState.GetFilter(Thief3).ShowUnsupported = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2FilterShowJunk="))
                {
                    config.GameTabsState.GetFilter(SS2).ShowUnsupported = val.EqualsTrue();
                }
                // @GENGAMES (Config reader - Filter): End
                #endregion
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.EnableArticles) + "="))
                {
                    config.EnableArticles = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.Articles) + "="))
                {
                    string[] articles = val.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);
                    for (int a = 0; a < articles.Length; a++) articles[a] = articles[a].Trim();
                    config.Articles.ClearAndAdd(articles.Distinct(StringComparer.OrdinalIgnoreCase));
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.MoveArticlesToEnd) + "="))
                {
                    config.MoveArticlesToEnd = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.SortDirection) + "="))
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
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.SortedColumn) + "="))
                {
                    var field = typeof(Column).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.SortedColumn = (Column)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.ShowRecentAtTop) + "="))
                {
                    config.ShowRecentAtTop = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.FMsListFontSizeInPoints) + "="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.FMsListFontSizeInPoints = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.RatingDisplayStyle) + "="))
                {
                    var field = typeof(RatingDisplayStyle).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.RatingDisplayStyle = (RatingDisplayStyle)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.RatingUseStars) + "="))
                {
                    config.RatingUseStars = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("TopRightTab="))
                {
                    var field = typeof(TopRightTab).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.TopRightTabsData.SelectedTab = (TopRightTab)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.SettingsTab) + "="))
                {
                    var field = typeof(SettingsTab).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.SettingsTab = (SettingsTab)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.SettingsWindowSize) + "="))
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
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.SettingsWindowSplitterDistance) + "="))
                {
                    if (int.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int result))
                    {
                        config.SettingsWindowSplitterDistance = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.SettingsPathsVScrollPos) + "="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SettingsPathsVScrollPos = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.SettingsFMDisplayVScrollPos) + "="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SettingsFMDisplayVScrollPos = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.SettingsOtherVScrollPos) + "="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SettingsOtherVScrollPos = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FMArchivePath="))
                {
                    config.FMArchivePaths.Add(val.Trim());
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.FMArchivePathsIncludeSubfolders) + "="))
                {
                    config.FMArchivePathsIncludeSubfolders = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.FMsBackupPath) + "="))
                {
                    config.FMsBackupPath = val.Trim();
                }
                #region Steam
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.LaunchGamesWithSteam) + "="))
                {
                    config.LaunchGamesWithSteam = val.EqualsTrue();
                }
                // @GENGAMES (Config reader - Steam): Begin
                else if (lineTS.StartsWithFast_NoNullChecks("T1UseSteam="))
                {
                    config.SetUseSteamSwitch(Thief1, val.EqualsTrue());
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2UseSteam="))
                {
                    config.SetUseSteamSwitch(Thief2, val.EqualsTrue());
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3UseSteam="))
                {
                    config.SetUseSteamSwitch(Thief3, val.EqualsTrue());
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2UseSteam="))
                {
                    config.SetUseSteamSwitch(SS2, val.EqualsTrue());
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.SteamExe) + "="))
                {
                    config.SteamExe = val.Trim();
                }
                // @GENGAMES (Config reader - Steam): End
                #endregion
                // @GENGAMES (Config reader - Exes): Begin
                else if (lineTS.StartsWithFast_NoNullChecks("T1Exe="))
                {
                    config.SetGameExe(Thief1, val.Trim());
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2Exe="))
                {
                    config.SetGameExe(Thief2, val.Trim());
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3Exe="))
                {
                    config.SetGameExe(Thief3, val.Trim());
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2Exe="))
                {
                    config.SetGameExe(SS2, val.Trim());
                }
                // @GENGAMES (Config reader - Exes): End
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.GameOrganization) + "="))
                {
                    var field = typeof(GameOrganization).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.GameOrganization = (GameOrganization)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.UseShortGameTabNames) + "="))
                {
                    config.UseShortGameTabNames = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.GameTab) + "="))
                {
                    config.GameTab = val switch
                    {
                        nameof(Thief2) => Thief2,
                        nameof(Thief3) => Thief3,
                        nameof(SS2) => SS2,
                        _ => Thief1
                    };
                }
                // @GENGAMES (Config reader - Selected FM pos infos): Begin
                else if (lineTS.StartsWithFast_NoNullChecks("T1SelFMInstDir="))
                {
                    config.GameTabsState.GetSelectedFM(Thief1).InstalledName = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2SelFMInstDir="))
                {
                    config.GameTabsState.GetSelectedFM(Thief2).InstalledName = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3SelFMInstDir="))
                {
                    config.GameTabsState.GetSelectedFM(Thief3).InstalledName = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2SelFMInstDir="))
                {
                    config.GameTabsState.GetSelectedFM(SS2).InstalledName = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T1SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.GetSelectedFM(Thief1).IndexFromTop = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T2SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.GetSelectedFM(Thief2).IndexFromTop = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("T3SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.GetSelectedFM(Thief3).IndexFromTop = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SS2SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.GetSelectedFM(SS2).IndexFromTop = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SelFMInstDir="))
                {
                    config.SelFM.InstalledName = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SelFM.IndexFromTop = result;
                    }
                }
                // @GENGAMES (Config reader - Selected FM pos infos): End
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.DateFormat) + "="))
                {
                    var field = typeof(DateFormat).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.DateFormat = (DateFormat)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.DateCustomFormat1) + "="))
                {
                    config.DateCustomFormat1 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.DateCustomSeparator1) + "="))
                {
                    config.DateCustomSeparator1 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.DateCustomFormat2) + "="))
                {
                    config.DateCustomFormat2 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.DateCustomSeparator2) + "="))
                {
                    config.DateCustomSeparator2 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.DateCustomFormat3) + "="))
                {
                    config.DateCustomFormat3 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.DateCustomSeparator3) + "="))
                {
                    config.DateCustomSeparator3 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.DateCustomFormat4) + "="))
                {
                    config.DateCustomFormat4 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.DaysRecent) + "="))
                {
                    if (uint.TryParse(val, out uint result)) config.DaysRecent = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.ReadmeZoomFactor) + "="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.ReadmeZoomFactor = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.ReadmeUseFixedWidthFont) + "="))
                {
                    config.ReadmeUseFixedWidthFont = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.MainWindowState) + "="))
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
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.MainWindowSize) + "="))
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
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.MainWindowLocation) + "="))
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
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.MainSplitterPercent) + "="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.MainSplitterPercent = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.TopSplitterPercent) + "="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.TopSplitterPercent = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.TopRightPanelCollapsed) + "="))
                {
                    config.TopRightPanelCollapsed = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.ConvertWAVsTo16BitOnInstall) + "="))
                {
                    config.ConvertWAVsTo16BitOnInstall = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.ConvertOGGsToWAVsOnInstall) + "="))
                {
                    config.ConvertOGGsToWAVsOnInstall = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.HideUninstallButton) + "="))
                {
                    config.HideUninstallButton = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.HideFMListZoomButtons) + "="))
                {
                    config.HideFMListZoomButtons = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.ConfirmUninstall) + "="))
                {
                    config.ConfirmUninstall = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.BackupFMData) + "="))
                {
                    var field = typeof(BackupFMData).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.BackupFMData = (BackupFMData)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.BackupAlwaysAsk) + "="))
                {
                    config.BackupAlwaysAsk = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.Language) + "="))
                {
                    config.Language = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.WebSearchUrl) + "="))
                {
                    config.WebSearchUrl = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks(nameof(config.ConfirmPlayOnDCOrEnter) + "="))
                {
                    config.ConfirmPlayOnDCOrEnter = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("StatsTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.StatsTab.Position = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("EditFMTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.EditFMTab.Position = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("CommentTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.CommentTab.Position = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("TagsTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.TagsTab.Position = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("PatchTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.PatchTab.Position = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("StatsTabVisible="))
                {
                    config.TopRightTabsData.StatsTab.Visible = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("EditFMTabVisible="))
                {
                    config.TopRightTabsData.EditFMTab.Visible = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("CommentTabVisible="))
                {
                    config.TopRightTabsData.CommentTab.Visible = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("TagsTabVisible="))
                {
                    config.TopRightTabsData.TagsTab.Visible = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("PatchTabVisible="))
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
        internal static void WriteConfigIni(ConfigData config, string fileName)
        {
            // 2020-06-03: My config file is ~4000 bytes (OneList, Thief2 filter only). 6000 gives reasonable
            // headroom for avoiding reallocations.
            var sb = new StringBuilder(6000);

            #region Settings window

            #region Settings window state

            sb.AppendLine(nameof(config.SettingsTab) + "=" + config.SettingsTab);
            sb.AppendLine(nameof(config.SettingsWindowSize) + "=" + config.SettingsWindowSize.Width + "," + config.SettingsWindowSize.Height);
            sb.AppendLine(nameof(config.SettingsWindowSplitterDistance) + "=" + config.SettingsWindowSplitterDistance);

            sb.AppendLine(nameof(config.SettingsPathsVScrollPos) + "=" + config.SettingsPathsVScrollPos);
            sb.AppendLine(nameof(config.SettingsFMDisplayVScrollPos) + "=" + config.SettingsFMDisplayVScrollPos);
            sb.AppendLine(nameof(config.SettingsOtherVScrollPos) + "=" + config.SettingsOtherVScrollPos);

            #endregion

            #region Paths

            #region Game exes

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                sb.AppendLine(GetGamePrefix(gameIndex) + "Exe=" + config.GetGameExe(gameIndex).Trim());
            }

            #endregion

            #region Steam

            sb.AppendLine(nameof(config.LaunchGamesWithSteam) + "=" + config.LaunchGamesWithSteam);

            // @GENGAMES (Config writer - Steam): Begin
            // So far all games are on Steam. If we have one that isn't, we can just add an internal per-game
            // read-only "IsOnSteam" bool and check it before writing/reading this
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                sb.AppendLine(GetGamePrefix(gameIndex) + "UseSteam=" + config.GetUseSteamSwitch(gameIndex));
            }
            // @GENGAMES (Config writer - Steam): End

            sb.AppendLine(nameof(config.SteamExe) + "=" + config.SteamExe);

            #endregion

            sb.AppendLine(nameof(config.FMsBackupPath) + "=" + config.FMsBackupPath.Trim());
            foreach (string path in config.FMArchivePaths) sb.AppendLine("FMArchivePath=" + path.Trim());
            sb.AppendLine(nameof(config.FMArchivePathsIncludeSubfolders) + "=" + config.FMArchivePathsIncludeSubfolders);

            #endregion

            sb.AppendLine(nameof(config.GameOrganization) + "=" + config.GameOrganization);
            sb.AppendLine(nameof(config.UseShortGameTabNames) + "=" + config.UseShortGameTabNames);

            sb.AppendLine(nameof(config.EnableArticles) + "=" + config.EnableArticles);
            sb.AppendLine(nameof(config.Articles) + "=" + CommaCombine(config.Articles));
            sb.AppendLine(nameof(config.MoveArticlesToEnd) + "=" + config.MoveArticlesToEnd);

            sb.AppendLine(nameof(config.RatingDisplayStyle) + "=" + config.RatingDisplayStyle);
            sb.AppendLine(nameof(config.RatingUseStars) + "=" + config.RatingUseStars);

            sb.AppendLine(nameof(config.DateFormat) + "=" + config.DateFormat);
            sb.AppendLine(nameof(config.DateCustomFormat1) + "=" + config.DateCustomFormat1);
            sb.AppendLine(nameof(config.DateCustomSeparator1) + "=" + config.DateCustomSeparator1);
            sb.AppendLine(nameof(config.DateCustomFormat2) + "=" + config.DateCustomFormat2);
            sb.AppendLine(nameof(config.DateCustomSeparator2) + "=" + config.DateCustomSeparator2);
            sb.AppendLine(nameof(config.DateCustomFormat3) + "=" + config.DateCustomFormat3);
            sb.AppendLine(nameof(config.DateCustomSeparator3) + "=" + config.DateCustomSeparator3);
            sb.AppendLine(nameof(config.DateCustomFormat4) + "=" + config.DateCustomFormat4);

            sb.AppendLine(nameof(config.DaysRecent) + "=" + config.DaysRecent);

            sb.AppendLine(nameof(config.ConvertWAVsTo16BitOnInstall) + "=" + config.ConvertWAVsTo16BitOnInstall);
            sb.AppendLine(nameof(config.ConvertOGGsToWAVsOnInstall) + "=" + config.ConvertOGGsToWAVsOnInstall);
            sb.AppendLine(nameof(config.HideUninstallButton) + "=" + config.HideUninstallButton);
            sb.AppendLine(nameof(config.HideFMListZoomButtons) + "=" + config.HideFMListZoomButtons);
            sb.AppendLine(nameof(config.ConfirmUninstall) + "=" + config.ConfirmUninstall);
            sb.AppendLine(nameof(config.BackupFMData) + "=" + config.BackupFMData);
            sb.AppendLine(nameof(config.BackupAlwaysAsk) + "=" + config.BackupAlwaysAsk);
            sb.AppendLine(nameof(config.Language) + "=" + config.Language);
            sb.AppendLine(nameof(config.WebSearchUrl) + "=" + config.WebSearchUrl);
            sb.AppendLine(nameof(config.ConfirmPlayOnDCOrEnter) + "=" + config.ConfirmPlayOnDCOrEnter);

            #endregion

            #region Filters

            for (int i = 0; i < SupportedGameCount + 1; i++)
            {
                Filter filter = i == 0 ? config.Filter : config.GameTabsState.GetFilter((GameIndex)(i - 1));
                string p = i == 0 ? "" : GetGamePrefix((GameIndex)(i - 1));

                if (i == 0) sb.AppendLine("FilterGames=" + CommaCombineGameFlags(config.Filter.Games));

                sb.AppendLine(p + "FilterTitle=" + filter.Title);
                sb.AppendLine(p + "FilterAuthor=" + filter.Author);

                sb.AppendLine(p + "FilterReleaseDateFrom=" + FilterDate(filter.ReleaseDateFrom));
                sb.AppendLine(p + "FilterReleaseDateTo=" + FilterDate(filter.ReleaseDateTo));

                sb.AppendLine(p + "FilterLastPlayedFrom=" + FilterDate(filter.LastPlayedFrom));
                sb.AppendLine(p + "FilterLastPlayedTo=" + FilterDate(filter.LastPlayedTo));

                sb.AppendLine(p + "FilterFinishedStates=" + CommaCombineFinishedStates(filter.Finished));

                sb.AppendLine(p + "FilterRatingFrom=" + filter.RatingFrom);
                sb.AppendLine(p + "FilterRatingTo=" + filter.RatingTo);

                sb.AppendLine(p + "FilterShowJunk=" + filter.ShowUnsupported);

                sb.AppendLine(p + "FilterTagsAnd=" + TagsToString(filter.Tags.AndTags));
                sb.AppendLine(p + "FilterTagsOr=" + TagsToString(filter.Tags.OrTags));
                sb.AppendLine(p + "FilterTagsNot=" + TagsToString(filter.Tags.NotTags));
            }

            #endregion

            #region Columns

            sb.AppendLine(nameof(config.SortedColumn) + "=" + config.SortedColumn);
            sb.AppendLine(nameof(config.SortDirection) + "=" + config.SortDirection);
            sb.AppendLine(nameof(config.ShowRecentAtTop) + "=" + config.ShowRecentAtTop);
            sb.AppendLine(nameof(config.FMsListFontSizeInPoints) + "=" + config.FMsListFontSizeInPoints.ToString(NumberFormatInfo.InvariantInfo));

            foreach (ColumnData col in config.Columns)
            {
                sb.AppendLine("Column" + col.Id + "=" + col.DisplayIndex + "," + col.Width + "," + col.Visible);
            }

            #endregion

            #region Selected FM

            for (int i = 0; i < SupportedGameCount + 1; i++)
            {
                SelectedFM selFM = i == 0 ? config.SelFM : config.GameTabsState.GetSelectedFM((GameIndex)(i - 1));
                string p = i == 0 ? "" : GetGamePrefix((GameIndex)(i - 1));

                sb.AppendLine(p + "SelFMInstDir=" + selFM.InstalledName);
                sb.AppendLine(p + "SelFMIndexFromTop=" + selFM.IndexFromTop);
            }

            #endregion

            #region Main window state

            sb.AppendLine(nameof(config.MainWindowState) + "=" +
                         (config.MainWindowState == FormWindowState.Minimized
                             ? FormWindowState.Maximized
                             : config.MainWindowState));

            sb.AppendLine(nameof(config.MainWindowSize) + "=" + config.MainWindowSize.Width + "," + config.MainWindowSize.Height);
            sb.AppendLine(nameof(config.MainWindowLocation) + "=" + config.MainWindowLocation.X + "," + config.MainWindowLocation.Y);

            sb.AppendLine(nameof(config.MainSplitterPercent) + "=" + config.MainSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
            sb.AppendLine(nameof(config.TopSplitterPercent) + "=" + config.TopSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
            sb.AppendLine(nameof(config.TopRightPanelCollapsed) + "=" + config.TopRightPanelCollapsed);

            sb.AppendLine(nameof(config.GameTab) + "=" + config.GameTab);
            sb.AppendLine("TopRightTab=" + config.TopRightTabsData.SelectedTab);

            sb.AppendLine("StatsTabPosition=" + config.TopRightTabsData.StatsTab.Position);
            sb.AppendLine("EditFMTabPosition=" + config.TopRightTabsData.EditFMTab.Position);
            sb.AppendLine("CommentTabPosition=" + config.TopRightTabsData.CommentTab.Position);
            sb.AppendLine("TagsTabPosition=" + config.TopRightTabsData.TagsTab.Position);
            sb.AppendLine("PatchTabPosition=" + config.TopRightTabsData.PatchTab.Position);

            sb.AppendLine("StatsTabVisible=" + config.TopRightTabsData.StatsTab.Visible);
            sb.AppendLine("EditFMTabVisible=" + config.TopRightTabsData.EditFMTab.Visible);
            sb.AppendLine("CommentTabVisible=" + config.TopRightTabsData.CommentTab.Visible);
            sb.AppendLine("TagsTabVisible=" + config.TopRightTabsData.TagsTab.Visible);
            sb.AppendLine("PatchTabVisible=" + config.TopRightTabsData.PatchTab.Visible);

            sb.AppendLine(nameof(config.ReadmeZoomFactor) + "=" + config.ReadmeZoomFactor.ToString(NumberFormatInfo.InvariantInfo));
            sb.AppendLine(nameof(config.ReadmeUseFixedWidthFont) + "=" + config.ReadmeUseFixedWidthFont);

            #endregion

            using var sw = new StreamWriter(fileName, false, Encoding.UTF8);
            sw.Write(sb.ToString());
        }
    }
}
