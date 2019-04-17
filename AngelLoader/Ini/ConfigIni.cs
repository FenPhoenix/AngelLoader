using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.Logger;

namespace AngelLoader.Ini
{
    // TODO: Maybe make this file have sections, cause it's getting pretty giant-blob-like

    internal static partial class Ini
    {
        // Not autogenerating these, because there's too many special cases, and adding stuff by hand is not that
        // big of a deal really.

        private static ColumnData ConvertStringToColumnData(string str)
        {
            str = str.Trim().Trim(',');

            // DisplayIndex,Width,Visible
            // 0,100,True
            var commas = str.CountChars(',');

            if (commas == 0) return null;

            var cProps = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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
            var tagsList =
                line.StartsWithFast_NoNullChecks(prefix + "FilterTagsAnd=") ? filter.Tags.AndTags :
                line.StartsWithFast_NoNullChecks(prefix + "FilterTagsOr=") ? filter.Tags.OrTags :
                line.StartsWithFast_NoNullChecks(prefix + "FilterTagsNot=") ? filter.Tags.NotTags :
                null;

            var val = line.Substring(line.IndexOf('=') + 1);

            if (tagsList == null || val.IsWhiteSpace()) return;

            var tagsArray = val.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in tagsArray)
            {
                string cat, tag;
                var colonCount = item.CountChars(':');
                if (colonCount > 1) continue;
                if (colonCount == 1)
                {
                    var index = item.IndexOf(':');
                    cat = item.Substring(0, index).Trim().ToLowerInvariant();
                    tag = item.Substring(index + 1).Trim();
                    if (cat.IsEmpty()) continue;
                }
                else
                {
                    cat = "misc";
                    tag = item.Trim();
                }

                CatAndTags match = null;
                for (int i = 0; i < tagsList.Count; i++)
                {
                    if (tagsList[i].Category == cat) match = tagsList[i];
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
            var list = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var finishedState in list)
            {
                switch (finishedState.Trim())
                {
                    case nameof(FinishedState.Finished):
                        filter.Finished.Add(FinishedState.Finished);
                        break;
                    case nameof(FinishedState.Unfinished):
                        filter.Finished.Add(FinishedState.Unfinished);
                        break;
                }
            }
        }

        // I tried removing the reflection in this one and it measured no faster, so leaving it as is.
        internal static void ReadConfigIni(string path, ConfigData config)
        {
            var iniLines = File.ReadAllLines(path);

            foreach (var line in iniLines)
            {
                if (!line.Contains('=')) continue;

                var lineT = line.TrimStart();

                if (lineT.Length > 0 && (lineT[0] == ';' || lineT[0] == '[')) continue;

                var val = lineT.Substring(lineT.IndexOf('=') + 1);

                if (lineT.StartsWithFast_NoNullChecks("Column") && line[6] != '=')
                {
                    var colName = lineT.Substring(6, lineT.IndexOf('=') - 6);

                    var field = typeof(Column).GetField(colName, BFlagsEnum);
                    if (field == null) continue;

                    var col = ConvertStringToColumnData(val);
                    if (col == null) continue;

                    col.Id = (Column)field.GetValue(null);
                    if (config.Columns.Any(x => x.Id == col.Id)) continue;

                    config.Columns.Add(col);
                }
                #region Filter

                else if (lineT.StartsWithFast_NoNullChecks("FilterTitle="))
                {
                    config.Filter.Title = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterTitle="))
                {
                    config.GameTabsState.T1Filter.Title = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterTitle="))
                {
                    config.GameTabsState.T2Filter.Title = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterTitle="))
                {
                    config.GameTabsState.T3Filter.Title = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterAuthor="))
                {
                    config.Filter.Author = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterAuthor="))
                {
                    config.GameTabsState.T1Filter.Author = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterAuthor="))
                {
                    config.GameTabsState.T2Filter.Author = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterAuthor="))
                {
                    config.GameTabsState.T3Filter.Author = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterReleaseDateFrom="))
                {
                    config.Filter.ReleaseDateFrom = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterReleaseDateFrom="))
                {
                    config.GameTabsState.T1Filter.ReleaseDateFrom = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterReleaseDateFrom="))
                {
                    config.GameTabsState.T2Filter.ReleaseDateFrom = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterReleaseDateFrom="))
                {
                    config.GameTabsState.T3Filter.ReleaseDateFrom = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterReleaseDateTo="))
                {
                    config.Filter.ReleaseDateTo = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterReleaseDateTo="))
                {
                    config.GameTabsState.T1Filter.ReleaseDateTo = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterReleaseDateTo="))
                {
                    config.GameTabsState.T2Filter.ReleaseDateTo = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterReleaseDateTo="))
                {
                    config.GameTabsState.T3Filter.ReleaseDateTo = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterLastPlayedFrom="))
                {
                    config.Filter.LastPlayedFrom = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterLastPlayedFrom="))
                {
                    config.GameTabsState.T1Filter.LastPlayedFrom = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterLastPlayedFrom="))
                {
                    config.GameTabsState.T2Filter.LastPlayedFrom = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterLastPlayedFrom="))
                {
                    config.GameTabsState.T3Filter.LastPlayedFrom = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterLastPlayedTo="))
                {
                    config.Filter.LastPlayedTo = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterLastPlayedTo="))
                {
                    config.GameTabsState.T1Filter.LastPlayedTo = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterLastPlayedTo="))
                {
                    config.GameTabsState.T2Filter.LastPlayedTo = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterLastPlayedTo="))
                {
                    config.GameTabsState.T3Filter.LastPlayedTo = ReadNullableHexDate(val);
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterTags") && line[10] != '=')
                {
                    ReadTags(lineT, config.Filter, "");
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterTags") && line[12] != '=')
                {
                    ReadTags(lineT, config.GameTabsState.T1Filter, "T1");
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterTags") && line[12] != '=')
                {
                    ReadTags(lineT, config.GameTabsState.T2Filter, "T2");
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterTags") && line[12] != '=')
                {
                    ReadTags(lineT, config.GameTabsState.T3Filter, "T3");
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterGames="))
                {
                    var list = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                    foreach (var game in list)
                    {
                        switch (game.Trim())
                        {
                            case nameof(Game.Thief1):
                                config.Filter.Games.Add(Game.Thief1);
                                break;
                            case nameof(Game.Thief2):
                                config.Filter.Games.Add(Game.Thief2);
                                break;
                            case nameof(Game.Thief3):
                                config.Filter.Games.Add(Game.Thief3);
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
                    if (int.TryParse(val, out int result)) config.GameTabsState.T1Filter.RatingFrom = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.T2Filter.RatingFrom = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterRatingFrom="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.T3Filter.RatingFrom = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.Filter.RatingTo = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.T1Filter.RatingTo = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.T2Filter.RatingTo = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterRatingTo="))
                {
                    if (int.TryParse(val, out int result)) config.GameTabsState.T3Filter.RatingTo = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.Filter);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.T1Filter);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.T2Filter);
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterFinishedStates="))
                {
                    ReadFinishedStates(val, config.GameTabsState.T3Filter);
                }
                else if (lineT.StartsWithFast_NoNullChecks("FilterShowJunk="))
                {
                    config.Filter.ShowJunk = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1FilterShowJunk="))
                {
                    config.GameTabsState.T1Filter.ShowJunk = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2FilterShowJunk="))
                {
                    config.GameTabsState.T2Filter.ShowJunk = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3FilterShowJunk="))
                {
                    config.GameTabsState.T3Filter.ShowJunk = val.EqualsTrue();
                }

                #endregion
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.EnableArticles) + "="))
                {
                    config.EnableArticles = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.Articles) + "="))
                {
                    var articles = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (var a = 0; a < articles.Length; a++) articles[a] = articles[a].Trim();
                    config.Articles.Clear();
                    config.Articles.AddRange(articles.Distinct(StringComparer.OrdinalIgnoreCase));
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
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.RatingDisplayStyle) + "="))
                {
                    var field = typeof(RatingDisplayStyle).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.RatingDisplayStyle = (RatingDisplayStyle)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.RatingDisplayStyle) + "="))
                {
                    config.RatingUseStars = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.TopRightTab) + "="))
                {
                    var field = typeof(TopRightTab).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.TopRightTab = (TopRightTab)field.GetValue(null);
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
                else if (lineT.StartsWithFast_NoNullChecks("FMArchivePath="))
                {
                    config.FMArchivePaths.Add(val.Trim());
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.FMArchivePathsIncludeSubfolders) + "="))
                {
                    config.FMArchivePathsIncludeSubfolders = val.EqualsTrue();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.FMsBackupPath) + "="))
                {
                    config.FMsBackupPath = val.Trim();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.T1Exe) + "="))
                {
                    config.T1Exe = val.Trim();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.T2Exe) + "="))
                {
                    config.T2Exe = val.Trim();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.T3Exe) + "="))
                {
                    config.T3Exe = val.Trim();
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.GameOrganization) + "="))
                {
                    var field = typeof(GameOrganization).GetField(val, BFlagsEnum);
                    if (field != null)
                    {
                        config.GameOrganization = (GameOrganization)field.GetValue(null);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.GameTab) + "="))
                {
                    switch (val)
                    {
                        case nameof(Game.Thief2):
                            config.GameTab = Game.Thief2;
                            break;
                        case nameof(Game.Thief3):
                            config.GameTab = Game.Thief3;
                            break;
                        default:
                            config.GameTab = Game.Thief1;
                            break;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1SelFMInstDir="))
                {
                    config.GameTabsState.T1SelFM.InstalledName = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2SelFMInstDir="))
                {
                    config.GameTabsState.T2SelFM.InstalledName = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3SelFMInstDir="))
                {
                    config.GameTabsState.T3SelFM.InstalledName = val;
                }
                else if (lineT.StartsWithFast_NoNullChecks("T1SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.T1SelFM.IndexFromTop = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("T2SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.T2SelFM.IndexFromTop = result;
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks("T3SelFMIndexFromTop="))
                {
                    if (int.TryParse(val, out int result))
                    {
                        config.GameTabsState.T3SelFM.IndexFromTop = result;
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
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.ReadmeZoomFactor) + "="))
                {
                    if (float.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result))
                    {
                        config.ReadmeZoomFactor = result;
                    }
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

                    var values = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var widthExists = int.TryParse(values[0].Trim(), out var width);
                    var heightExists = int.TryParse(values[1].Trim(), out var height);

                    if (widthExists && heightExists)
                    {
                        config.MainWindowSize = new Size(width, height);
                    }
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.MainWindowLocation) + "="))
                {
                    if (!val.Contains(',')) continue;

                    var values = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var xExists = int.TryParse(values[0].Trim(), out var x);
                    var yExists = int.TryParse(values[1].Trim(), out var y);

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
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.TopRightTabOrder.StatsTabPosition) + "="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabOrder.StatsTabPosition = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.TopRightTabOrder.EditFMTabPosition) + "="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabOrder.EditFMTabPosition = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.TopRightTabOrder.CommentTabPosition) + "="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabOrder.CommentTabPosition = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.TopRightTabOrder.TagsTabPosition) + "="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabOrder.TagsTabPosition = result;
                }
                else if (lineT.StartsWithFast_NoNullChecks(nameof(config.TopRightTabOrder.PatchTabPosition) + "="))
                {
                    int.TryParse(val, out int result);
                    config.TopRightTabOrder.PatchTabPosition = result;
                }
            }

            var sep1 = config.DateCustomSeparator1.EscapeAllChars();
            var sep2 = config.DateCustomSeparator2.EscapeAllChars();
            var sep3 = config.DateCustomSeparator3.EscapeAllChars();

            var formatString = config.DateCustomFormat1 +
                               sep1 +
                               config.DateCustomFormat2 +
                               sep2 +
                               config.DateCustomFormat3 +
                               sep3 +
                               config.DateCustomFormat4;

            try
            {
                var temp = new DateTime(2000, 1, 1).ToString(formatString);
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
            string commaCombine<T>(List<T> list)
            {
                var ret = "";
                for (var i = 0; i < list.Count; i++)
                {
                    if (i > 0) ret += ",";
                    ret += list[i].ToString();
                }

                return ret;
            }

            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(fileName, false, Encoding.UTF8);

                #region Settings window

                sw.WriteLine(nameof(config.SettingsTab) + "=" + config.SettingsTab);

                #region Paths

                sw.WriteLine(nameof(config.T1Exe) + "=" + config.T1Exe.Trim());
                sw.WriteLine(nameof(config.T2Exe) + "=" + config.T2Exe.Trim());
                sw.WriteLine(nameof(config.T3Exe) + "=" + config.T3Exe.Trim());
                sw.WriteLine(nameof(config.FMsBackupPath) + "=" + config.FMsBackupPath.Trim());
                foreach (string path in config.FMArchivePaths) sw.WriteLine("FMArchivePath=" + path.Trim());
                sw.WriteLine(nameof(config.FMArchivePathsIncludeSubfolders) + "=" + config.FMArchivePathsIncludeSubfolders);

                #endregion

                sw.WriteLine(nameof(config.GameOrganization) + "=" + config.GameOrganization);

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

                sw.WriteLine(nameof(config.ConvertWAVsTo16BitOnInstall) + "=" + config.ConvertWAVsTo16BitOnInstall);
                sw.WriteLine(nameof(config.ConvertOGGsToWAVsOnInstall) + "=" + config.ConvertOGGsToWAVsOnInstall);
                sw.WriteLine(nameof(config.ConfirmUninstall) + "=" + config.ConfirmUninstall);
                sw.WriteLine(nameof(config.BackupFMData) + "=" + config.BackupFMData);
                sw.WriteLine(nameof(config.BackupAlwaysAsk) + "=" + config.BackupAlwaysAsk);
                sw.WriteLine(nameof(config.Language) + "=" + config.Language);
                sw.WriteLine(nameof(config.WebSearchUrl) + "=" + config.WebSearchUrl);
                sw.WriteLine(nameof(config.ConfirmPlayOnDCOrEnter) + "=" + config.ConfirmPlayOnDCOrEnter);

                #endregion

                #region Filters

                string FilterDate(DateTime? dt) => dt == null
                    ? ""
                    : new DateTimeOffset((DateTime)dt).ToUnixTimeSeconds().ToString("X");

                for (int fi = 0; fi < 4; fi++)
                {
                    var filter =
                        fi == 0 ? config.Filter :
                        fi == 1 ? config.GameTabsState.T1Filter :
                        fi == 2 ? config.GameTabsState.T2Filter :
                        config.GameTabsState.T3Filter;
                    var p = fi == 0 ? "" : fi == 1 ? "T1" : fi == 2 ? "T2" : "T3";

                    if (fi == 0) sw.WriteLine("FilterGames=" + commaCombine(config.Filter.Games));

                    sw.WriteLine(p + "FilterTitle=" + filter.Title);
                    sw.WriteLine(p + "FilterAuthor=" + filter.Author);

                    sw.WriteLine(p + "FilterReleaseDateFrom=" + FilterDate(filter.ReleaseDateFrom));
                    sw.WriteLine(p + "FilterReleaseDateTo=" + FilterDate(filter.ReleaseDateTo));

                    sw.WriteLine(p + "FilterLastPlayedFrom=" + FilterDate(filter.LastPlayedFrom));
                    sw.WriteLine(p + "FilterLastPlayedTo=" + FilterDate(filter.LastPlayedTo));

                    sw.WriteLine(p + "FilterFinishedStates=" + commaCombine(filter.Finished));

                    sw.WriteLine(p + "FilterRatingFrom=" + filter.RatingFrom);
                    sw.WriteLine(p + "FilterRatingTo=" + filter.RatingTo);

                    sw.WriteLine(p + "FilterShowJunk=" + filter.ShowJunk);

                    #region Tags

                    string TagsToString(List<CatAndTags> tagsList)
                    {
                        var intermediateTagsList = new List<string>();
                        foreach (var catAndTag in tagsList)
                        {
                            if (catAndTag.Tags.Count == 0)
                            {
                                intermediateTagsList.Add(catAndTag.Category + ":");
                            }
                            else
                            {
                                foreach (var tag in catAndTag.Tags)
                                {
                                    intermediateTagsList.Add(catAndTag.Category + ":" + tag);
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

                foreach (var col in config.Columns)
                {
                    sw.WriteLine("Column" + col.Id + "=" + col.DisplayIndex + "," + col.Width + "," + col.Visible);
                }

                #endregion

                #region Selected FM

                sw.WriteLine("SelFMInstDir=" + config.SelFM.InstalledName);
                sw.WriteLine("SelFMIndexFromTop=" + config.SelFM.IndexFromTop);
                sw.WriteLine("T1SelFMInstDir=" + config.GameTabsState.T1SelFM.InstalledName);
                sw.WriteLine("T1SelFMIndexFromTop=" + config.GameTabsState.T1SelFM.IndexFromTop);
                sw.WriteLine("T2SelFMInstDir=" + config.GameTabsState.T2SelFM.InstalledName);
                sw.WriteLine("T2SelFMIndexFromTop=" + config.GameTabsState.T2SelFM.IndexFromTop);
                sw.WriteLine("T3SelFMInstDir=" + config.GameTabsState.T3SelFM.InstalledName);
                sw.WriteLine("T3SelFMIndexFromTop=" + config.GameTabsState.T3SelFM.IndexFromTop);

                #endregion

                #region Window state

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
                sw.WriteLine(nameof(config.TopRightTab) + "=" + config.TopRightTab);

                sw.WriteLine(nameof(config.TopRightTabOrder.StatsTabPosition) + "=" + config.TopRightTabOrder.StatsTabPosition);
                sw.WriteLine(nameof(config.TopRightTabOrder.EditFMTabPosition) + "=" + config.TopRightTabOrder.EditFMTabPosition);
                sw.WriteLine(nameof(config.TopRightTabOrder.CommentTabPosition) + "=" + config.TopRightTabOrder.CommentTabPosition);
                sw.WriteLine(nameof(config.TopRightTabOrder.TagsTabPosition) + "=" + config.TopRightTabOrder.TagsTabPosition);
                sw.WriteLine(nameof(config.TopRightTabOrder.PatchTabPosition) + "=" + config.TopRightTabOrder.PatchTabPosition);

                sw.WriteLine(nameof(config.ReadmeZoomFactor) + "=" + config.ReadmeZoomFactor.ToString(NumberFormatInfo.InvariantInfo));

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
