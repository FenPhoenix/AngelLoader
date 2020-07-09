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
    // NOTE: This file should have had sections from the start, but now that it got released without, we can't
    // really change it without breaking compatibility. Oh well.

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

                int indexOfEq = lineTS.IndexOf('=');
                if (indexOfEq == -1) continue;

                if (lineTS.Length > 0 && (lineTS[0] == ';' || lineTS[0] == '[')) continue;

                string val = lineTS.Substring(indexOfEq + 1);

                if (lineTS.StartsWithFast_NoNullChecks("Column") && lineTS[6] != '=')
                {
                    string colName = lineTS.Substring(6, indexOfEq - 6);

                    var field = typeof(Column).GetField(colName, _bFlagsEnum);
                    if (field == null) continue;

                    ColumnData? col = ConvertStringToColumnData(val);
                    if (col == null) continue;

                    col.Id = (Column)field.GetValue(null);

                    if (!ContainsColWithId(config, col)) config.Columns.Add(col);
                }

                else if (lineTS.StartsWithFast_NoNullChecks("GameOrganization="))
                {
                    var field = typeof(GameOrganization).GetField(val, _bFlagsEnum);
                    if (field != null)
                    {
                        config.GameOrganization = (GameOrganization)field.GetValue(null);
                    }
                }

                #region Filter

                else if (lineTS.StartsWithFast_NoNullChecks("FilterGames="))
                {
                    string[] iniGames = val
                        .Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries)
                        .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

                    for (int i = 0; i < iniGames.Length; i++)
                    {
                        iniGames[i] = iniGames[i].Trim();
                    }

                    string[] gameNames = new string[SupportedGameCount];

                    for (int i = 0; i < SupportedGameCount; i++)
                    {
                        GameIndex gameIndex = (GameIndex)i;
                        for (int j = 0; j < iniGames.Length; j++)
                        {
                            string game = iniGames[j];
                            // Stupid micro-optimization
                            if (game == (gameNames[i] ??= gameIndex.ToString()))
                            {
                                config.Filter.Games |= GameIndexToGame(gameIndex);
                                break;
                            }
                        }
                    }
                }

                // One list
                else if (lineTS.StartsWithFast_NoNullChecks("FilterTitle="))
                {
                    config.Filter.Title = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterAuthor="))
                {
                    config.Filter.Author = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterReleaseDateFrom="))
                {
                    config.Filter.ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterReleaseDateTo="))
                {
                    config.Filter.ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterLastPlayedFrom="))
                {
                    config.Filter.LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterLastPlayedTo="))
                {
                    config.Filter.LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterFinishedStates="))
                {
                    ReadFinishedStates(config.Filter, val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.Filter.RatingFrom = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.Filter.RatingTo = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterShowJunk="))
                {
                    config.Filter.ShowUnsupported = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterTagsAnd="))
                {
                    ReadTags(config.Filter.Tags.AndTags, val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterTagsOr="))
                {
                    ReadTags(config.Filter.Tags.OrTags, val);
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FilterTagsNot="))
                {
                    ReadTags(config.Filter.Tags.NotTags, val);
                }

                // By tab
                else if (IsGamePrefixedLine(lineTS, "FilterTitle=", out GameIndex _gameIndex))
                {
                    config.GameTabsState.GetFilter(_gameIndex).Title = val;
                }
                else if (IsGamePrefixedLine(lineTS, "FilterAuthor=", out _gameIndex))
                {
                    config.GameTabsState.GetFilter(_gameIndex).Author = val;
                }
                else if (IsGamePrefixedLine(lineTS, "FilterReleaseDateFrom=", out _gameIndex))
                {
                    config.GameTabsState.GetFilter(_gameIndex).ReleaseDateFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (IsGamePrefixedLine(lineTS, "FilterReleaseDateTo=", out _gameIndex))
                {
                    config.GameTabsState.GetFilter(_gameIndex).ReleaseDateTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (IsGamePrefixedLine(lineTS, "FilterLastPlayedFrom=", out _gameIndex))
                {
                    config.GameTabsState.GetFilter(_gameIndex).LastPlayedFrom = ConvertHexUnixDateToDateTime(val);
                }
                else if (IsGamePrefixedLine(lineTS, "FilterLastPlayedTo=", out _gameIndex))
                {
                    config.GameTabsState.GetFilter(_gameIndex).LastPlayedTo = ConvertHexUnixDateToDateTime(val);
                }
                else if (IsGamePrefixedLine(lineTS, "FilterFinishedStates=", out _gameIndex))
                {
                    ReadFinishedStates(config.GameTabsState.GetFilter(_gameIndex), val);
                }
                else if (IsGamePrefixedLine(lineTS, "FilterRatingFrom=", out _gameIndex))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.GetFilter(_gameIndex).RatingFrom = result;
                    }
                }
                else if (IsGamePrefixedLine(lineTS, "FilterRatingTo=", out _gameIndex))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.GetFilter(_gameIndex).RatingTo = result;
                    }
                }
                else if (IsGamePrefixedLine(lineTS, "FilterShowJunk=", out _gameIndex))
                {
                    config.GameTabsState.GetFilter(_gameIndex).ShowUnsupported = val.EqualsTrue();
                }
                else if (IsGamePrefixedLine(lineTS, "FilterTagsAnd=", out _gameIndex))
                {
                    ReadTags(config.GameTabsState.GetFilter(_gameIndex).Tags.AndTags, val);
                }
                else if (IsGamePrefixedLine(lineTS, "FilterTagsOr=", out _gameIndex))
                {
                    ReadTags(config.GameTabsState.GetFilter(_gameIndex).Tags.OrTags, val);
                }
                else if (IsGamePrefixedLine(lineTS, "FilterTagsNot=", out _gameIndex))
                {
                    ReadTags(config.GameTabsState.GetFilter(_gameIndex).Tags.NotTags, val);
                }

                #endregion

                #region Articles

                else if (lineTS.StartsWithFast_NoNullChecks("EnableArticles="))
                {
                    config.EnableArticles = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("Articles="))
                {
                    string[] articles = val.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);
                    for (int a = 0; a < articles.Length; a++) articles[a] = articles[a].Trim();
                    config.Articles.ClearAndAdd(articles.Distinct(StringComparer.OrdinalIgnoreCase));
                }
                else if (lineTS.StartsWithFast_NoNullChecks("MoveArticlesToEnd="))
                {
                    config.MoveArticlesToEnd = val.EqualsTrue();
                }

                #endregion

                #region Sorting

                else if (lineTS.StartsWithFast_NoNullChecks("SortDirection="))
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
                else if (lineTS.StartsWithFast_NoNullChecks("SortedColumn="))
                {
                    var field = typeof(Column).GetField(val, _bFlagsEnum);
                    if (field != null)
                    {
                        config.SortedColumn = (Column)field.GetValue(null);
                    }
                }

                #endregion

                #region Rating display style

                else if (lineTS.StartsWithFast_NoNullChecks("RatingDisplayStyle="))
                {
                    var field = typeof(RatingDisplayStyle).GetField(val, _bFlagsEnum);
                    if (field != null)
                    {
                        config.RatingDisplayStyle = (RatingDisplayStyle)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("RatingUseStars="))
                {
                    config.RatingUseStars = val.EqualsTrue();
                }

                #endregion

                #region Settings window state

                else if (lineTS.StartsWithFast_NoNullChecks("SettingsTab="))
                {
                    var field = typeof(SettingsTab).GetField(val, _bFlagsEnum);
                    if (field != null)
                    {
                        config.SettingsTab = (SettingsTab)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SettingsWindowSize="))
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
                else if (lineTS.StartsWithFast_NoNullChecks("SettingsWindowSplitterDistance="))
                {
                    if (int.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int result))
                    {
                        config.SettingsWindowSplitterDistance = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SettingsPathsVScrollPos="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SettingsPathsVScrollPos = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SettingsFMDisplayVScrollPos="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SettingsFMDisplayVScrollPos = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SettingsOtherVScrollPos="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SettingsOtherVScrollPos = result;
                    }
                }

                #endregion

                else if (IsGamePrefixedLine(lineTS, "Exe=", out _gameIndex))
                {
                    config.SetGameExe(_gameIndex, val.Trim());
                }

                #region Steam
                else if (lineTS.StartsWithFast_NoNullChecks("LaunchGamesWithSteam="))
                {
                    config.LaunchGamesWithSteam = val.EqualsTrue();
                }
                else if (IsGamePrefixedLine(lineTS, "UseSteam=", out _gameIndex))
                {
                    config.SetUseSteamSwitch(_gameIndex, val.EqualsTrue());
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SteamExe="))
                {
                    config.SteamExe = val.Trim();
                }
                #endregion

                #region FM paths

                else if (lineTS.StartsWithFast_NoNullChecks("FMsBackupPath="))
                {
                    config.FMsBackupPath = val.Trim();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FMArchivePath="))
                {
                    config.FMArchivePaths.Add(val.Trim());
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FMArchivePathsIncludeSubfolders="))
                {
                    config.FMArchivePathsIncludeSubfolders = val.EqualsTrue();
                }

                #endregion

                else if (lineTS.StartsWithFast_NoNullChecks("UseShortGameTabNames="))
                {
                    config.UseShortGameTabNames = val.EqualsTrue();
                }

                else if (lineTS.StartsWithFast_NoNullChecks("GameTab="))
                {
                    bool found = false;
                    for (int i = 0; i < SupportedGameCount; i++)
                    {
                        GameIndex gameIndex = (GameIndex)i;
                        if (val == gameIndex.ToString())
                        {
                            config.GameTab = gameIndex;
                            found = true;
                            break;
                        }
                    }
                    // matching previous behavior
                    if (!found) config.GameTab = Thief1;
                }

                #region Selected FM info

                // One list
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

                // By tab
                else if (IsGamePrefixedLine(lineTS, "SelFMInstDir=", out _gameIndex))
                {
                    config.GameTabsState.GetSelectedFM(_gameIndex).InstalledName = val;
                }
                else if (IsGamePrefixedLine(lineTS, "SelFMIndexFromTop=", out _gameIndex))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.GetSelectedFM(_gameIndex).IndexFromTop = result;
                    }
                }

                #endregion

                #region Date format

                else if (lineTS.StartsWithFast_NoNullChecks("DateFormat="))
                {
                    var field = typeof(DateFormat).GetField(val, _bFlagsEnum);
                    if (field != null)
                    {
                        config.DateFormat = (DateFormat)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("DateCustomFormat1="))
                {
                    config.DateCustomFormat1 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("DateCustomSeparator1="))
                {
                    config.DateCustomSeparator1 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("DateCustomFormat2="))
                {
                    config.DateCustomFormat2 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("DateCustomSeparator2="))
                {
                    config.DateCustomSeparator2 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("DateCustomFormat3="))
                {
                    config.DateCustomFormat3 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("DateCustomSeparator3="))
                {
                    config.DateCustomSeparator3 = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("DateCustomFormat4="))
                {
                    config.DateCustomFormat4 = val;
                }

                #endregion

                #region Readme

                else if (lineTS.StartsWithFast_NoNullChecks("ReadmeZoomFactor="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.ReadmeZoomFactor = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("ReadmeUseFixedWidthFont="))
                {
                    config.ReadmeUseFixedWidthFont = val.EqualsTrue();
                }

                #endregion

                #region Main window state

                else if (lineTS.StartsWithFast_NoNullChecks("MainWindowState="))
                {
                    var field = typeof(FormWindowState).GetField(val, _bFlagsEnum);
                    if (field != null)
                    {
                        var windowState = (FormWindowState)field.GetValue(null);
                        if (windowState != FormWindowState.Minimized)
                        {
                            config.MainWindowState = windowState;
                        }
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("MainWindowSize="))
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
                else if (lineTS.StartsWithFast_NoNullChecks("MainWindowLocation="))
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
                else if (lineTS.StartsWithFast_NoNullChecks("MainSplitterPercent="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.MainSplitterPercent = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("TopSplitterPercent="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.TopSplitterPercent = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("TopRightPanelCollapsed="))
                {
                    config.TopRightPanelCollapsed = val.EqualsTrue();
                }

                #endregion

                #region Top-right tabs

                else if (lineTS.StartsWithFast_NoNullChecks("TopRightTab="))
                {
                    var field = typeof(TopRightTab).GetField(val, _bFlagsEnum);
                    if (field != null)
                    {
                        config.TopRightTabsData.SelectedTab = (TopRightTab)field.GetValue(null);
                    }
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

                #endregion

                #region Audio conversion

                else if (lineTS.StartsWithFast_NoNullChecks("ConvertWAVsTo16BitOnInstall="))
                {
                    config.ConvertWAVsTo16BitOnInstall = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("ConvertOGGsToWAVsOnInstall="))
                {
                    config.ConvertOGGsToWAVsOnInstall = val.EqualsTrue();
                }

                #endregion

                #region Hide UI elements

                else if (lineTS.StartsWithFast_NoNullChecks("HideUninstallButton="))
                {
                    config.HideUninstallButton = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("HideFMListZoomButtons="))
                {
                    config.HideFMListZoomButtons = val.EqualsTrue();
                }

                #endregion

                #region Uninstall / backup

                else if (lineTS.StartsWithFast_NoNullChecks("ConfirmUninstall="))
                {
                    config.ConfirmUninstall = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("BackupFMData="))
                {
                    var field = typeof(BackupFMData).GetField(val, _bFlagsEnum);
                    if (field != null)
                    {
                        config.BackupFMData = (BackupFMData)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("BackupAlwaysAsk="))
                {
                    config.BackupAlwaysAsk = val.EqualsTrue();
                }

                #endregion

                else if (lineTS.StartsWithFast_NoNullChecks("DaysRecent="))
                {
                    if (uint.TryParse(val, out uint result))
                    {
                        config.DaysRecent = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("Language="))
                {
                    config.Language = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("WebSearchUrl="))
                {
                    config.WebSearchUrl = val;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("ConfirmPlayOnDCOrEnter="))
                {
                    config.ConfirmPlayOnDCOrEnter = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("ShowRecentAtTop="))
                {
                    config.ShowRecentAtTop = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FMsListFontSizeInPoints="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.FMsListFontSizeInPoints = result;
                    }
                }
            }

            // Vital, don't remove!
            FinalizeConfig(config);
        }

        // This is faster with reflection removed.
        private static void WriteConfigIniInternal(ConfigData config, string fileName)
        {
            // 2020-06-03: My config file is ~4000 bytes (OneList, Thief2 filter only). 6000 gives reasonable
            // headroom for avoiding reallocations.
            var sb = new StringBuilder(6000);

            #region Settings window

            #region Settings window state

            sb.AppendLine("SettingsTab=" + config.SettingsTab);
            sb.AppendLine("SettingsWindowSize=" + config.SettingsWindowSize.Width + "," + config.SettingsWindowSize.Height);
            sb.AppendLine("SettingsWindowSplitterDistance=" + config.SettingsWindowSplitterDistance);

            sb.AppendLine("SettingsPathsVScrollPos=" + config.SettingsPathsVScrollPos);
            sb.AppendLine("SettingsFMDisplayVScrollPos=" + config.SettingsFMDisplayVScrollPos);
            sb.AppendLine("SettingsOtherVScrollPos=" + config.SettingsOtherVScrollPos);

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

            sb.AppendLine("LaunchGamesWithSteam=" + config.LaunchGamesWithSteam);

            // @GENGAMES (Config writer - Steam): Begin
            // So far all games are on Steam. If we have one that isn't, we can just add an internal per-game
            // read-only "IsOnSteam" bool and check it before writing/reading this
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                sb.AppendLine(GetGamePrefix(gameIndex) + "UseSteam=" + config.GetUseSteamSwitch(gameIndex));
            }
            // @GENGAMES (Config writer - Steam): End

            sb.AppendLine("SteamExe=" + config.SteamExe);

            #endregion

            sb.AppendLine("FMsBackupPath=" + config.FMsBackupPath.Trim());
            foreach (string path in config.FMArchivePaths) sb.AppendLine("FMArchivePath=" + path.Trim());
            sb.AppendLine("FMArchivePathsIncludeSubfolders=" + config.FMArchivePathsIncludeSubfolders);

            #endregion

            sb.AppendLine("GameOrganization=" + config.GameOrganization);
            sb.AppendLine("UseShortGameTabNames=" + config.UseShortGameTabNames);

            sb.AppendLine("EnableArticles=" + config.EnableArticles);
            sb.AppendLine("Articles=" + CommaCombine(config.Articles));
            sb.AppendLine("MoveArticlesToEnd=" + config.MoveArticlesToEnd);

            sb.AppendLine("RatingDisplayStyle=" + config.RatingDisplayStyle);
            sb.AppendLine("RatingUseStars=" + config.RatingUseStars);

            sb.AppendLine("DateFormat=" + config.DateFormat);
            sb.AppendLine("DateCustomFormat1=" + config.DateCustomFormat1);
            sb.AppendLine("DateCustomSeparator1=" + config.DateCustomSeparator1);
            sb.AppendLine("DateCustomFormat2=" + config.DateCustomFormat2);
            sb.AppendLine("DateCustomSeparator2=" + config.DateCustomSeparator2);
            sb.AppendLine("DateCustomFormat3=" + config.DateCustomFormat3);
            sb.AppendLine("DateCustomSeparator3=" + config.DateCustomSeparator3);
            sb.AppendLine("DateCustomFormat4=" + config.DateCustomFormat4);

            sb.AppendLine("DaysRecent=" + config.DaysRecent);

            sb.AppendLine("ConvertWAVsTo16BitOnInstall=" + config.ConvertWAVsTo16BitOnInstall);
            sb.AppendLine("ConvertOGGsToWAVsOnInstall=" + config.ConvertOGGsToWAVsOnInstall);
            sb.AppendLine("HideUninstallButton=" + config.HideUninstallButton);
            sb.AppendLine("HideFMListZoomButtons=" + config.HideFMListZoomButtons);
            sb.AppendLine("ConfirmUninstall=" + config.ConfirmUninstall);
            sb.AppendLine("BackupFMData=" + config.BackupFMData);
            sb.AppendLine("BackupAlwaysAsk=" + config.BackupAlwaysAsk);
            sb.AppendLine("Language=" + config.Language);
            sb.AppendLine("WebSearchUrl=" + config.WebSearchUrl);
            sb.AppendLine("ConfirmPlayOnDCOrEnter=" + config.ConfirmPlayOnDCOrEnter);

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

            sb.AppendLine("SortedColumn=" + config.SortedColumn);
            sb.AppendLine("SortDirection=" + config.SortDirection);
            sb.AppendLine("ShowRecentAtTop=" + config.ShowRecentAtTop);
            sb.AppendLine("FMsListFontSizeInPoints=" + config.FMsListFontSizeInPoints.ToString(NumberFormatInfo.InvariantInfo));

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

            sb.AppendLine("MainWindowState=" + config.MainWindowState);

            sb.AppendLine("MainWindowSize=" + config.MainWindowSize.Width + "," + config.MainWindowSize.Height);
            sb.AppendLine("MainWindowLocation=" + config.MainWindowLocation.X + "," + config.MainWindowLocation.Y);

            sb.AppendLine("MainSplitterPercent=" + config.MainSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
            sb.AppendLine("TopSplitterPercent=" + config.TopSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
            sb.AppendLine("TopRightPanelCollapsed=" + config.TopRightPanelCollapsed);

            sb.AppendLine("GameTab=" + config.GameTab);
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

            sb.AppendLine("ReadmeZoomFactor=" + config.ReadmeZoomFactor.ToString(NumberFormatInfo.InvariantInfo));
            sb.AppendLine("ReadmeUseFixedWidthFont=" + config.ReadmeUseFixedWidthFont);

            #endregion

            using var sw = new StreamWriter(fileName, false, Encoding.UTF8);
            sw.Write(sb.ToString());
        }
    }
}
