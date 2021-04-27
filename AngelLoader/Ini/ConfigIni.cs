#define FenGen_ConfigDest

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AL_Common.CommonUtils;
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

            for (int i = 0; i < iniLines.Length; i++)
            {
                string lt = iniLines[i].Trim();
                if (lt.StartsWithFast_NoNullChecks(ConfigVersionHeader))
                {
                    if (int.TryParse(lt.Substring(ConfigVersionHeader.Length), out int result))
                    {
                        config.Version = result;
                        break;
                    }
                }
            }

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

                else if (lineTS.StartsWithFast_NoNullChecks("FilterVisible") && lineTS[13] != '=')
                {
                    string filterName = lineTS.Substring(13, indexOfEq - 13);

                    var field = typeof(HideableFilterControls).GetField(filterName, _bFlagsEnum);
                    if (field == null) continue;

                    config.FilterControlVisibilities[(int)field.GetValue(null)] = val.EqualsTrue();
                }

                else if (lineTS.StartsWithFast_NoNullChecks("FilterGames="))
                {
                    string[] iniGames = val
                        .Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries)
                        .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

                    for (int i = 0; i < iniGames.Length; i++)
                    {
                        iniGames[i] = iniGames[i].Trim();
                    }

                    string?[] gameNames = new string?[SupportedGameCount];

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
                else if (lineTS.StartsWithFast_NoNullChecks("SettingsAppearanceVScrollPos="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SettingsAppearanceVScrollPos = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("SettingsFMsListVScrollPos="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.SettingsFMsListVScrollPos = result;
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
                    config.TopRightTabsData.StatsTab.DisplayIndex = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("EditFMTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.EditFMTab.DisplayIndex = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("CommentTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.CommentTab.DisplayIndex = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("TagsTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.TagsTab.DisplayIndex = result;
                }
                else if (lineTS.StartsWithFast_NoNullChecks("PatchTabPosition="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabsData.PatchTab.DisplayIndex = result;
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
                else if (lineTS.StartsWithFast_NoNullChecks("HideExitButton="))
                {
                    config.HideExitButton = val.EqualsTrue();
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
                else if (lineTS.StartsWithFast_NoNullChecks("ShowUnsupported="))
                {
                    config.ShowUnsupported = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("ShowUnavailableFMs="))
                {
                    config.ShowUnavailableFMs = val.EqualsTrue();
                }
                else if (lineTS.StartsWithFast_NoNullChecks("FMsListFontSizeInPoints="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.FMsListFontSizeInPoints = result;
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("VisualTheme="))
                {
                    var field = typeof(VisualTheme).GetField(val, _bFlagsEnum);
                    if (field != null)
                    {
                        config.VisualTheme = (VisualTheme)field.GetValue(null);
                    }
                }
                else if (lineTS.StartsWithFast_NoNullChecks("RTFThemedColorStyle="))
                {
                    var field = typeof(RTFColorStyle).GetField(val, _bFlagsEnum);
                    if (field != null)
                    {
                        config.RTFThemedColorStyle = (RTFColorStyle)field.GetValue(null);
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

            // @NET5: Write current config version header (keep it off for testing old-to-new)
#if false
            sb.Append(ConfigVersionHeader).AppendLine(AppConfigVersion.ToString());
#endif

            #region Settings window

            #region Settings window state

            sb.Append("SettingsTab=").Append(config.SettingsTab).AppendLine();
            sb.Append("SettingsWindowSize=").Append(config.SettingsWindowSize.Width).Append(',').Append(config.SettingsWindowSize.Height).AppendLine();
            sb.Append("SettingsWindowSplitterDistance=").Append(config.SettingsWindowSplitterDistance).AppendLine();

            sb.Append("SettingsPathsVScrollPos=").Append(config.SettingsPathsVScrollPos).AppendLine();
            sb.Append("SettingsAppearanceVScrollPos=").Append(config.SettingsAppearanceVScrollPos).AppendLine();
            sb.Append("SettingsFMsListVScrollPos=").Append(config.SettingsFMsListVScrollPos).AppendLine();
            sb.Append("SettingsOtherVScrollPos=").Append(config.SettingsOtherVScrollPos).AppendLine();

            #endregion

            #region Paths

            #region Game exes

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                sb.Append(GetGamePrefix(gameIndex)).Append("Exe=").AppendLine(config.GetGameExe(gameIndex).Trim());
            }

            #endregion

            #region Steam

            sb.Append("LaunchGamesWithSteam=").Append(config.LaunchGamesWithSteam).AppendLine();

            // @GENGAMES (Config writer - Steam): Begin
            // So far all games are on Steam. If we have one that isn't, we can just add an internal per-game
            // read-only "IsOnSteam" bool and check it before writing/reading this
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                sb.Append(GetGamePrefix(gameIndex)).Append("UseSteam=").Append(config.GetUseSteamSwitch(gameIndex)).AppendLine();
            }
            // @GENGAMES (Config writer - Steam): End

            sb.Append("SteamExe=").AppendLine(config.SteamExe);

            #endregion

            sb.Append("FMsBackupPath=").AppendLine(config.FMsBackupPath.Trim());
            foreach (string path in config.FMArchivePaths) sb.Append("FMArchivePath=").AppendLine(path.Trim());
            sb.Append("FMArchivePathsIncludeSubfolders=").Append(config.FMArchivePathsIncludeSubfolders).AppendLine();

            #endregion

            sb.Append("GameOrganization=").Append(config.GameOrganization).AppendLine();
            sb.Append("UseShortGameTabNames=").Append(config.UseShortGameTabNames).AppendLine();

            sb.Append("EnableArticles=").Append(config.EnableArticles).AppendLine();
            sb.Append("Articles=").AppendLine(string.Join(",", config.Articles));
            sb.Append("MoveArticlesToEnd=").Append(config.MoveArticlesToEnd).AppendLine();

            sb.Append("RatingDisplayStyle=").Append(config.RatingDisplayStyle).AppendLine();
            sb.Append("RatingUseStars=").Append(config.RatingUseStars).AppendLine();

            sb.Append("DateFormat=").Append(config.DateFormat).AppendLine();
            sb.Append("DateCustomFormat1=").AppendLine(config.DateCustomFormat1);
            sb.Append("DateCustomSeparator1=").AppendLine(config.DateCustomSeparator1);
            sb.Append("DateCustomFormat2=").AppendLine(config.DateCustomFormat2);
            sb.Append("DateCustomSeparator2=").AppendLine(config.DateCustomSeparator2);
            sb.Append("DateCustomFormat3=").AppendLine(config.DateCustomFormat3);
            sb.Append("DateCustomSeparator3=").AppendLine(config.DateCustomSeparator3);
            sb.Append("DateCustomFormat4=").AppendLine(config.DateCustomFormat4);

            sb.Append("DaysRecent=").Append(config.DaysRecent).AppendLine();

            sb.Append("ConvertWAVsTo16BitOnInstall=").Append(config.ConvertWAVsTo16BitOnInstall).AppendLine();
            sb.Append("ConvertOGGsToWAVsOnInstall=").Append(config.ConvertOGGsToWAVsOnInstall).AppendLine();
            sb.Append("HideUninstallButton=").Append(config.HideUninstallButton).AppendLine();
            sb.Append("HideFMListZoomButtons=").Append(config.HideFMListZoomButtons).AppendLine();
            sb.Append("HideExitButton=").Append(config.HideExitButton).AppendLine();
            sb.Append("ConfirmUninstall=").Append(config.ConfirmUninstall).AppendLine();
            sb.Append("BackupFMData=").Append(config.BackupFMData).AppendLine();
            sb.Append("BackupAlwaysAsk=").Append(config.BackupAlwaysAsk).AppendLine();
            sb.Append("Language=").AppendLine(config.Language);
            sb.Append("WebSearchUrl=").AppendLine(config.WebSearchUrl);
            sb.Append("ConfirmPlayOnDCOrEnter=").Append(config.ConfirmPlayOnDCOrEnter).AppendLine();

            sb.Append("VisualTheme=").Append(config.VisualTheme).AppendLine();
            sb.Append("RTFThemedColorStyle=").Append(config.RTFThemedColorStyle).AppendLine();

            #endregion

            #region Filters

            for (int i = 0; i < HideableFilterControlsCount; i++)
            {
                sb.Append("FilterVisible").Append((HideableFilterControls)i).Append('=').AppendLine(config.FilterControlVisibilities[i].ToString());
            }

            for (int i = 0; i < SupportedGameCount + 1; i++)
            {
                Filter filter = i == 0 ? config.Filter : config.GameTabsState.GetFilter((GameIndex)(i - 1));
                string p = i == 0 ? "" : GetGamePrefix((GameIndex)(i - 1));

                if (i == 0)
                {
                    sb.Append("FilterGames=");
                    CommaCombineGameFlags(sb, config.Filter.Games);
                }

                sb.Append(p).Append("FilterTitle=").AppendLine(filter.Title);
                sb.Append(p).Append("FilterAuthor=").AppendLine(filter.Author);

                sb.Append(p).Append("FilterReleaseDateFrom=").AppendLine(FilterDate(filter.ReleaseDateFrom));
                sb.Append(p).Append("FilterReleaseDateTo=").AppendLine(FilterDate(filter.ReleaseDateTo));

                sb.Append(p).Append("FilterLastPlayedFrom=").AppendLine(FilterDate(filter.LastPlayedFrom));
                sb.Append(p).Append("FilterLastPlayedTo=").AppendLine(FilterDate(filter.LastPlayedTo));

                sb.Append(p).Append("FilterFinishedStates=");
                CommaCombineFinishedStates(sb, filter.Finished);

                sb.Append(p).Append("FilterRatingFrom=").Append(filter.RatingFrom).AppendLine();
                sb.Append(p).Append("FilterRatingTo=").Append(filter.RatingTo).AppendLine();

                sb.Append(p).Append("FilterTagsAnd=").AppendLine(TagsToString(filter.Tags.AndTags));
                sb.Append(p).Append("FilterTagsOr=").AppendLine(TagsToString(filter.Tags.OrTags));
                sb.Append(p).Append("FilterTagsNot=").AppendLine(TagsToString(filter.Tags.NotTags));
            }

            #endregion

            #region Columns

            sb.Append("SortedColumn=").Append(config.SortedColumn).AppendLine();
            sb.Append("SortDirection=").Append(config.SortDirection).AppendLine();
            sb.Append("ShowRecentAtTop=").Append(config.ShowRecentAtTop).AppendLine();
            sb.Append("ShowUnsupported=").Append(config.ShowUnsupported).AppendLine();
            sb.Append("ShowUnavailableFMs=").Append(config.ShowUnavailableFMs).AppendLine();
            sb.Append("FMsListFontSizeInPoints=").AppendLine(config.FMsListFontSizeInPoints.ToString(NumberFormatInfo.InvariantInfo));

            foreach (ColumnData col in config.Columns)
            {
                sb.Append("Column").Append(col.Id).Append('=').Append(col.DisplayIndex).Append(',').Append(col.Width).Append(',').Append(col.Visible).AppendLine();
            }

            #endregion

            #region Selected FM

            for (int i = 0; i < SupportedGameCount + 1; i++)
            {
                SelectedFM selFM = i == 0 ? config.SelFM : config.GameTabsState.GetSelectedFM((GameIndex)(i - 1));
                string p = i == 0 ? "" : GetGamePrefix((GameIndex)(i - 1));

                sb.Append(p).Append("SelFMInstDir=").AppendLine(selFM.InstalledName);
                sb.Append(p).Append("SelFMIndexFromTop=").Append(selFM.IndexFromTop).AppendLine();
            }

            #endregion

            #region Main window state

            sb.Append("MainWindowState=").Append(config.MainWindowState).AppendLine();

            sb.Append("MainWindowSize=").Append(config.MainWindowSize.Width).Append(',').Append(config.MainWindowSize.Height).AppendLine();
            sb.Append("MainWindowLocation=").Append(config.MainWindowLocation.X).Append(',').Append(config.MainWindowLocation.Y).AppendLine();

            sb.Append("MainSplitterPercent=").AppendLine(config.MainSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
            sb.Append("TopSplitterPercent=").AppendLine(config.TopSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
            sb.Append("TopRightPanelCollapsed=").Append(config.TopRightPanelCollapsed).AppendLine();

            sb.Append("GameTab=").Append(config.GameTab).AppendLine();
            sb.Append("TopRightTab=").Append(config.TopRightTabsData.SelectedTab).AppendLine();

            sb.Append("StatsTabPosition=").Append(config.TopRightTabsData.StatsTab.DisplayIndex).AppendLine();
            sb.Append("EditFMTabPosition=").Append(config.TopRightTabsData.EditFMTab.DisplayIndex).AppendLine();
            sb.Append("CommentTabPosition=").Append(config.TopRightTabsData.CommentTab.DisplayIndex).AppendLine();
            sb.Append("TagsTabPosition=").Append(config.TopRightTabsData.TagsTab.DisplayIndex).AppendLine();
            sb.Append("PatchTabPosition=").Append(config.TopRightTabsData.PatchTab.DisplayIndex).AppendLine();

            sb.Append("StatsTabVisible=").Append(config.TopRightTabsData.StatsTab.Visible).AppendLine();
            sb.Append("EditFMTabVisible=").Append(config.TopRightTabsData.EditFMTab.Visible).AppendLine();
            sb.Append("CommentTabVisible=").Append(config.TopRightTabsData.CommentTab.Visible).AppendLine();
            sb.Append("TagsTabVisible=").Append(config.TopRightTabsData.TagsTab.Visible).AppendLine();
            sb.Append("PatchTabVisible=").Append(config.TopRightTabsData.PatchTab.Visible).AppendLine();

            sb.Append("ReadmeZoomFactor=").AppendLine(config.ReadmeZoomFactor.ToString(NumberFormatInfo.InvariantInfo));
            sb.Append("ReadmeUseFixedWidthFont=").Append(config.ReadmeUseFixedWidthFont).AppendLine();

            #endregion

            using var sw = new StreamWriter(fileName, false, Encoding.UTF8);
            sw.Write(sb.ToString());
        }
    }
}
