#define FenGen_ConfigDest

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.GameSupport.GameIndex;
using static AngelLoader.Misc;

namespace AngelLoader
{
    // IMPORTANT: Possible BUG:
    // Autogenerate the config reader dict/action methods, no matter what it takes! With the flattened version,
    // it's way too easy to have a bug in one of the per-game things, or per-whatever things.

    // NOTE: This file should have had sections from the start, but now that it got released without, we can't
    // really change it without breaking compatibility. Oh well.

    // A note about floats:
    // When storing and reading floats, it's imperative that we specify InvariantInfo. Otherwise the decimal
    // separator is culture-dependent, and it could end up as ',' when we expect '.'. And then we could end up
    // with "5.001" being "5,001", and now we're in for a bad time.

    // TODO(Config ini reader) - Idea: chop the first 2/3 chars off keys and see if they're T1/T2/T3/SS2, then key on the non-prefixed version
    // Then we cut down massively on the repetition, and the setter can do another lookup by being passed the prefix

    internal static partial class Ini
    {
        #region Config reader setters

        #region Settings window state

        private static void Config_SettingsTab_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            var field = typeof(SettingsTab).GetField(valTrimmed, _bFlagsEnum);
            if (field != null)
            {
                config.SettingsTab = (SettingsTab)field.GetValue(null);
            }
        }

        private static void Config_SettingsWindowSize_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (!valTrimmed.Contains(',')) return;

            string[] values = valTrimmed.Split(CA_Comma);
            bool widthExists = Int_TryParseInv(values[0].Trim(), out int width);
            bool heightExists = Int_TryParseInv(values[1].Trim(), out int height);

            if (widthExists && heightExists)
            {
                config.SettingsWindowSize = new Size(width, height);
            }
        }

        private static void Config_SettingsWindowSplitterDistance_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.SettingsWindowSplitterDistance = result;
            }
        }

        private static void Config_SettingsPathsVScrollPos_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.SettingsPathsVScrollPos = result;
            }
        }

        private static void Config_SettingsAppearanceVScrollPos_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.SettingsAppearanceVScrollPos = result;
            }
        }

        private static void Config_SettingsOtherVScrollPos_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.SettingsOtherVScrollPos = result;
            }
        }

        #endregion

        private static void Config_LaunchGamesWithSteam_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.LaunchGamesWithSteam = valTrimmed.EqualsTrue();
        }

        private static void Config_SteamExe_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.SteamExe = valTrimmed;
        }

        private static void Config_FMsBackupPath_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FMsBackupPath = valTrimmed;
        }

        private static void Config_FMArchivePath_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FMArchivePaths.Add(valTrimmed);
        }

        private static void Config_FMArchivePathsIncludeSubfolders_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FMArchivePathsIncludeSubfolders = valTrimmed.EqualsTrue();
        }

        private static void Config_GameOrganization_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            var field = typeof(GameOrganization).GetField(valTrimmed, _bFlagsEnum);
            if (field != null)
            {
                config.GameOrganization = (GameOrganization)field.GetValue(null);
            }
        }

        private static void Config_UseShortGameTabNames_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.UseShortGameTabNames = valTrimmed.EqualsTrue();
        }

        private static void Config_EnableArticles_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.EnableArticles = valTrimmed.EqualsTrue();
        }

        private static void Config_Articles_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            string[] articles = valTrimmed.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);
            for (int a = 0; a < articles.Length; a++) articles[a] = articles[a].Trim();
            config.Articles.ClearAndAdd(articles.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static void Config_MoveArticlesToEnd_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.MoveArticlesToEnd = valTrimmed.EqualsTrue();
        }

        private static void Config_RatingDisplayStyle_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            var field = typeof(RatingDisplayStyle).GetField(valTrimmed, _bFlagsEnum);
            if (field != null)
            {
                config.RatingDisplayStyle = (RatingDisplayStyle)field.GetValue(null);
            }
        }

        private static void Config_RatingUseStars_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.RatingUseStars = valTrimmed.EqualsTrue();
        }

        #region Date format

        private static void Config_DateFormat_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            var field = typeof(DateFormat).GetField(valTrimmed, _bFlagsEnum);
            if (field != null)
            {
                config.DateFormat = (DateFormat)field.GetValue(null);
            }
        }

        private static void Config_DateCustomFormat1_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.DateCustomFormat1 = valRaw;
        }

        private static void Config_DateCustomSeparator1_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.DateCustomSeparator1 = valRaw;
        }

        private static void Config_DateCustomFormat2_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.DateCustomFormat2 = valRaw;
        }

        private static void Config_DateCustomSeparator2_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.DateCustomSeparator2 = valRaw;
        }

        private static void Config_DateCustomFormat3_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.DateCustomFormat3 = valRaw;
        }

        private static void Config_DateCustomSeparator3_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.DateCustomSeparator3 = valRaw;
        }

        private static void Config_DateCustomFormat4_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.DateCustomFormat4 = valRaw;
        }

        #endregion

        private static void Config_DaysRecent_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (UInt_TryParseInv(valTrimmed, out uint result))
            {
                config.DaysRecent = result;
            }
        }

        private static void Config_ConvertWAVsTo16BitOnInstall_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.ConvertWAVsTo16BitOnInstall = valTrimmed.EqualsTrue();
        }

        private static void Config_ConvertOGGsToWAVsOnInstall_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.ConvertOGGsToWAVsOnInstall = valTrimmed.EqualsTrue();
        }

        private static void Config_HideUninstallButton_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.HideUninstallButton = valTrimmed.EqualsTrue();
        }

        private static void Config_HideFMListZoomButtons_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.HideFMListZoomButtons = valTrimmed.EqualsTrue();
        }

        private static void Config_HideExitButton_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.HideExitButton = valTrimmed.EqualsTrue();
        }

        private static void Config_ConfirmUninstall_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.ConfirmUninstall = valTrimmed.EqualsTrue();
        }

        private static void Config_BackupFMData_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            var field = typeof(BackupFMData).GetField(valTrimmed, _bFlagsEnum);
            if (field != null)
            {
                config.BackupFMData = (BackupFMData)field.GetValue(null);
            }
        }

        private static void Config_BackupAlwaysAsk_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.BackupAlwaysAsk = valTrimmed.EqualsTrue();
        }

        private static void Config_Language_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.Language = valTrimmed;
        }

        private static void Config_WebSearchUrl_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.WebSearchUrl = valRaw;
        }

        private static void Config_ConfirmPlayOnDCOrEnter_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.ConfirmPlayOnDCOrEnter = valTrimmed.EqualsTrue();
        }

        private static void Config_VisualTheme_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            var field = typeof(VisualTheme).GetField(valTrimmed, _bFlagsEnum);
            if (field != null)
            {
                config.VisualTheme = (VisualTheme)field.GetValue(null);
            }
        }

        #region Filter visibilities

        private static void Config_FilterVisibleTitle_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FilterControlVisibilities[(int)HideableFilterControls.Title] = valTrimmed.EqualsTrue();
        }
        private static void Config_FilterVisibleAuthor_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FilterControlVisibilities[(int)HideableFilterControls.Author] = valTrimmed.EqualsTrue();
        }
        private static void Config_FilterVisibleReleaseDate_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FilterControlVisibilities[(int)HideableFilterControls.ReleaseDate] = valTrimmed.EqualsTrue();
        }
        private static void Config_FilterVisibleLastPlayed_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FilterControlVisibilities[(int)HideableFilterControls.LastPlayed] = valTrimmed.EqualsTrue();
        }
        private static void Config_FilterVisibleTags_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FilterControlVisibilities[(int)HideableFilterControls.Tags] = valTrimmed.EqualsTrue();
        }
        private static void Config_FilterVisibleFinishedState_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FilterControlVisibilities[(int)HideableFilterControls.FinishedState] = valTrimmed.EqualsTrue();
        }
        private static void Config_FilterVisibleRating_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FilterControlVisibilities[(int)HideableFilterControls.Rating] = valTrimmed.EqualsTrue();
        }
        private static void Config_FilterVisibleShowUnsupported_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FilterControlVisibilities[(int)HideableFilterControls.ShowUnsupported] = valTrimmed.EqualsTrue();
        }
        private static void Config_FilterVisibleShowUnavailable_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FilterControlVisibilities[(int)HideableFilterControls.ShowUnavailable] = valTrimmed.EqualsTrue();
        }
        private static void Config_FilterVisibleShowRecentAtTop_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.FilterControlVisibilities[(int)HideableFilterControls.ShowRecentAtTop] = valTrimmed.EqualsTrue();
        }

        #endregion

        #region Global filter values

        private static void Config_FilterGames_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            string[] iniGames = valTrimmed.Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < iniGames.Length; i++)
            {
                iniGames[i] = iniGames[i].Trim();
            }
            iniGames = iniGames.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

            string?[] gameNames = new string?[SupportedGameCount];

            // @BigO (but doesn't matter except for OCD)
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

        private static void Config_FilterTitle_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.Filter.Title = valRaw;
        }

        private static void Config_FilterAuthor_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.Filter.Author = valRaw;
        }

        private static void Config_FilterReleaseDateFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.Filter.ReleaseDateFrom = ConvertHexUnixDateToDateTime(valTrimmed);
        }

        private static void Config_FilterReleaseDateTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.Filter.ReleaseDateTo = ConvertHexUnixDateToDateTime(valTrimmed);
        }

        private static void Config_FilterLastPlayedFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.Filter.LastPlayedFrom = ConvertHexUnixDateToDateTime(valTrimmed);
        }

        private static void Config_FilterLastPlayedTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.Filter.LastPlayedTo = ConvertHexUnixDateToDateTime(valTrimmed);
        }

        private static void Config_FilterFinishedStates_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadFinishedStates(config.Filter, valTrimmed);
        }

        private static void Config_FilterRatingFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.Filter.RatingFrom = result;
            }
        }

        private static void Config_FilterRatingTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.Filter.RatingTo = result;
            }
        }

        private static void Config_FilterTagsAnd_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.Filter.Tags.AndTags, valRaw);
        }

        private static void Config_FilterTagsOr_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.Filter.Tags.OrTags, valRaw);
        }

        private static void Config_FilterTagsNot_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.Filter.Tags.NotTags, valRaw);
        }

        #endregion

        #region Per-game fields

        // @GENGAMES (ConfigIni field setters) - Begin

        private static void Config_T1Exe_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.SetGameExe(Thief1, valTrimmed);
        }
        private static void Config_T2Exe_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.SetGameExe(Thief2, valTrimmed);
        }
        private static void Config_T3Exe_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.SetGameExe(Thief3, valTrimmed);
        }
        private static void Config_SS2Exe_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.SetGameExe(SS2, valTrimmed);
        }

        private static void Config_T1UseSteam_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.SetUseSteamSwitch(Thief1, valTrimmed.EqualsTrue());
        }
        private static void Config_T2UseSteam_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.SetUseSteamSwitch(Thief2, valTrimmed.EqualsTrue());
        }
        private static void Config_T3UseSteam_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.SetUseSteamSwitch(Thief3, valTrimmed.EqualsTrue());
        }
        private static void Config_SS2UseSteam_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.SetUseSteamSwitch(SS2, valTrimmed.EqualsTrue());
        }

        private static void Config_GameFilterVisibleT1_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameFilterControlVisibilities[(int)Thief1] = valTrimmed.EqualsTrue();
        }
        private static void Config_GameFilterVisibleT2_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameFilterControlVisibilities[(int)Thief2] = valTrimmed.EqualsTrue();
        }
        private static void Config_GameFilterVisibleT3_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameFilterControlVisibilities[(int)Thief3] = valTrimmed.EqualsTrue();
        }
        private static void Config_GameFilterVisibleSS2_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameFilterControlVisibilities[(int)SS2] = valTrimmed.EqualsTrue();
        }

        private static void Config_T1FilterTitle_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief1).Title = valRaw;
        }
        private static void Config_T2FilterTitle_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief2).Title = valRaw;
        }
        private static void Config_T3FilterTitle_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief3).Title = valRaw;
        }
        private static void Config_SS2FilterTitle_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(SS2).Title = valRaw;
        }

        private static void Config_T1FilterAuthor_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief1).Author = valRaw;
        }
        private static void Config_T2FilterAuthor_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief2).Author = valRaw;
        }
        private static void Config_T3FilterAuthor_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief3).Author = valRaw;
        }
        private static void Config_SS2FilterAuthor_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(SS2).Author = valRaw;
        }

        private static void Config_T1FilterReleaseDateFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief1).ReleaseDateFrom = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_T2FilterReleaseDateFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief2).ReleaseDateFrom = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_T3FilterReleaseDateFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief3).ReleaseDateFrom = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_SS2FilterReleaseDateFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(SS2).ReleaseDateFrom = ConvertHexUnixDateToDateTime(valTrimmed);
        }

        private static void Config_T1FilterReleaseDateTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief1).ReleaseDateTo = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_T2FilterReleaseDateTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief2).ReleaseDateTo = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_T3FilterReleaseDateTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief3).ReleaseDateTo = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_SS2FilterReleaseDateTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(SS2).ReleaseDateTo = ConvertHexUnixDateToDateTime(valTrimmed);
        }

        private static void Config_T1FilterLastPlayedFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief1).LastPlayedFrom = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_T2FilterLastPlayedFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief2).LastPlayedFrom = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_T3FilterLastPlayedFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief3).LastPlayedFrom = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_SS2FilterLastPlayedFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(SS2).LastPlayedFrom = ConvertHexUnixDateToDateTime(valTrimmed);
        }

        private static void Config_T1FilterLastPlayedTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief1).LastPlayedTo = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_T2FilterLastPlayedTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief2).LastPlayedTo = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_T3FilterLastPlayedTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(Thief3).LastPlayedTo = ConvertHexUnixDateToDateTime(valTrimmed);
        }
        private static void Config_SS2FilterLastPlayedTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetFilter(SS2).LastPlayedTo = ConvertHexUnixDateToDateTime(valTrimmed);
        }

        private static void Config_T1FilterFinishedStates_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadFinishedStates(config.GameTabsState.GetFilter(Thief1), valTrimmed);
        }
        private static void Config_T2FilterFinishedStates_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadFinishedStates(config.GameTabsState.GetFilter(Thief2), valTrimmed);
        }
        private static void Config_T3FilterFinishedStates_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadFinishedStates(config.GameTabsState.GetFilter(Thief3), valTrimmed);
        }
        private static void Config_SS2FilterFinishedStates_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadFinishedStates(config.GameTabsState.GetFilter(SS2), valTrimmed);
        }

        private static void Config_T1FilterRatingFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetFilter(Thief1).RatingFrom = result;
            }
        }
        private static void Config_T2FilterRatingFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetFilter(Thief2).RatingFrom = result;
            }
        }
        private static void Config_T3FilterRatingFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetFilter(Thief3).RatingFrom = result;
            }
        }
        private static void Config_SS2FilterRatingFrom_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetFilter(SS2).RatingFrom = result;
            }
        }

        private static void Config_T1FilterRatingTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetFilter(Thief1).RatingTo = result;
            }
        }
        private static void Config_T2FilterRatingTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetFilter(Thief2).RatingTo = result;
            }
        }
        private static void Config_T3FilterRatingTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetFilter(Thief3).RatingTo = result;
            }
        }
        private static void Config_SS2FilterRatingTo_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetFilter(SS2).RatingTo = result;
            }
        }

        private static void Config_T1FilterTagsAnd_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(Thief1).Tags.AndTags, valRaw);
        }
        private static void Config_T2FilterTagsAnd_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(Thief2).Tags.AndTags, valRaw);
        }
        private static void Config_T3FilterTagsAnd_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(Thief3).Tags.AndTags, valRaw);
        }
        private static void Config_SS2FilterTagsAnd_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(SS2).Tags.AndTags, valRaw);
        }

        private static void Config_T1FilterTagsOr_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(Thief1).Tags.OrTags, valRaw);
        }
        private static void Config_T2FilterTagsOr_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(Thief2).Tags.OrTags, valRaw);
        }
        private static void Config_T3FilterTagsOr_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(Thief3).Tags.OrTags, valRaw);
        }
        private static void Config_SS2FilterTagsOr_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(SS2).Tags.OrTags, valRaw);
        }

        private static void Config_T1FilterTagsNot_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(Thief1).Tags.NotTags, valRaw);
        }
        private static void Config_T2FilterTagsNot_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(Thief2).Tags.NotTags, valRaw);
        }
        private static void Config_T3FilterTagsNot_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(Thief3).Tags.NotTags, valRaw);
        }
        private static void Config_SS2FilterTagsNot_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            ReadTags(config.GameTabsState.GetFilter(SS2).Tags.NotTags, valRaw);
        }

        private static void Config_T1SelFMInstDir_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetSelectedFM(Thief1).InstalledName = valRaw;
        }
        private static void Config_T2SelFMInstDir_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetSelectedFM(Thief2).InstalledName = valRaw;
        }
        private static void Config_T3SelFMInstDir_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetSelectedFM(Thief3).InstalledName = valRaw;
        }
        private static void Config_SS2SelFMInstDir_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.GameTabsState.GetSelectedFM(SS2).InstalledName = valRaw;
        }

        private static void Config_T1SelFMIndexFromTop_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetSelectedFM(Thief1).IndexFromTop = result;
            }
        }
        private static void Config_T2SelFMIndexFromTop_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetSelectedFM(Thief2).IndexFromTop = result;
            }
        }
        private static void Config_T3SelFMIndexFromTop_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetSelectedFM(Thief3).IndexFromTop = result;
            }
        }
        private static void Config_SS2SelFMIndexFromTop_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.GameTabsState.GetSelectedFM(SS2).IndexFromTop = result;
            }
        }

        // @GENGAMES (ConfigIni field setters) - End

        #endregion

        private static void Config_ShowRecentAtTop_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.ShowRecentAtTop = valTrimmed.EqualsTrue();
        }
        private static void Config_ShowUnsupported_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.ShowUnsupported = valTrimmed.EqualsTrue();
        }
        private static void Config_ShowUnavailableFMs_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.ShowUnavailableFMs = valTrimmed.EqualsTrue();
        }
        private static void Config_FMsListFontSizeInPoints_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Float_TryParseInv(valTrimmed, out float result))
            {
                config.FMsListFontSizeInPoints = result;
            }
        }

        private static void Config_SortedColumn_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            var field = typeof(Column).GetField(valTrimmed, _bFlagsEnum);
            if (field != null)
            {
                config.SortedColumn = (Column)field.GetValue(null);
            }
        }
        private static void Config_SortDirection_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            var field = typeof(SortDirection).GetField(valTrimmed, _bFlagsEnum);
            if (field != null)
            {
                config.SortDirection = (SortDirection)field.GetValue(null);
            }
        }

        #region Columns

        private static void Config_ColumnGame_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.Game);
        }
        private static void Config_ColumnInstalled_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.Installed);
        }
        private static void Config_ColumnTitle_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.Title);
        }
        private static void Config_ColumnArchive_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.Archive);
        }
        private static void Config_ColumnAuthor_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.Author);
        }
        private static void Config_ColumnSize_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.Size);
        }
        private static void Config_ColumnRating_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.Rating);
        }
        private static void Config_ColumnFinished_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.Finished);
        }
        private static void Config_ColumnReleaseDate_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.ReleaseDate);
        }
        private static void Config_ColumnLastPlayed_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.LastPlayed);
        }
        private static void Config_ColumnDateAdded_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.DateAdded);
        }
        private static void Config_ColumnDisabledMods_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.DisabledMods);
        }
        private static void Config_ColumnComment_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            AddColumn(config, valTrimmed, Column.Comment);
        }

        #endregion

        private static void Config_SelFMInstDir_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.SelFM.InstalledName = valTrimmed;
        }
        private static void Config_SelFMIndexFromTop_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Int_TryParseInv(valTrimmed, out int result))
            {
                config.SelFM.IndexFromTop = result;
            }
        }

        private static void Config_MainWindowState_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            var field = typeof(WindowState).GetField(valTrimmed, _bFlagsEnum);
            if (field != null)
            {
                var windowState = (WindowState)field.GetValue(null);
                if (windowState != WindowState.Minimized)
                {
                    config.MainWindowState = windowState;
                }
            }
        }
        private static void Config_MainWindowSize_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (!valTrimmed.Contains(',')) return;

            string[] values = valTrimmed.Split(CA_Comma);
            bool widthExists = Int_TryParseInv(values[0].Trim(), out int width);
            bool heightExists = Int_TryParseInv(values[1].Trim(), out int height);

            if (widthExists && heightExists)
            {
                config.MainWindowSize = new Size(width, height);
            }
        }
        private static void Config_MainWindowLocation_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (!valTrimmed.Contains(',')) return;

            string[] values = valTrimmed.Split(CA_Comma);
            bool xExists = Int_TryParseInv(values[0].Trim(), out int x);
            bool yExists = Int_TryParseInv(values[1].Trim(), out int y);

            if (xExists && yExists)
            {
                config.MainWindowLocation = new Point(x, y);
            }
        }

        private static void Config_MainSplitterPercent_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Float_TryParseInv(valTrimmed, out float result))
            {
                config.MainSplitterPercent = result;
            }
        }
        private static void Config_TopSplitterPercent_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Float_TryParseInv(valTrimmed, out float result))
            {
                config.TopSplitterPercent = result;
            }
        }
        private static void Config_TopRightPanelCollapsed_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.TopRightPanelCollapsed = valTrimmed.EqualsTrue();
        }
        private static void Config_GameTab_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            bool found = false;
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                if (valTrimmed == gameIndex.ToString())
                {
                    config.GameTab = gameIndex;
                    found = true;
                    break;
                }
            }
            // matching previous behavior
            if (!found) config.GameTab = Thief1;
        }

        #region Top-right tabs

        private static void Config_TopRightTab_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            var field = typeof(TopRightTab).GetField(valTrimmed, _bFlagsEnum);
            if (field != null)
            {
                config.TopRightTabsData.SelectedTab = (TopRightTab)field.GetValue(null);
            }
        }

        private static void Config_StatsTabPosition_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            Int_TryParseInv(valTrimmed, out int result);
            config.TopRightTabsData.StatsTab.DisplayIndex = result;
        }
        private static void Config_StatsTabVisible_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.TopRightTabsData.StatsTab.Visible = valTrimmed.EqualsTrue();
        }

        private static void Config_EditFMTabPosition_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            Int_TryParseInv(valTrimmed, out int result);
            config.TopRightTabsData.EditFMTab.DisplayIndex = result;
        }
        private static void Config_EditFMTabVisible_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.TopRightTabsData.EditFMTab.Visible = valTrimmed.EqualsTrue();
        }

        private static void Config_CommentTabPosition_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            Int_TryParseInv(valTrimmed, out int result);
            config.TopRightTabsData.CommentTab.DisplayIndex = result;
        }
        private static void Config_CommentTabVisible_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.TopRightTabsData.CommentTab.Visible = valTrimmed.EqualsTrue();
        }

        private static void Config_TagsTabPosition_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            Int_TryParseInv(valTrimmed, out int result);
            config.TopRightTabsData.TagsTab.DisplayIndex = result;
        }
        private static void Config_TagsTabVisible_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.TopRightTabsData.TagsTab.Visible = valTrimmed.EqualsTrue();
        }

        private static void Config_PatchTabPosition_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            Int_TryParseInv(valTrimmed, out int result);
            config.TopRightTabsData.PatchTab.DisplayIndex = result;
        }
        private static void Config_PatchTabVisible_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.TopRightTabsData.PatchTab.Visible = valTrimmed.EqualsTrue();
        }

        private static void Config_ModsTabPosition_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            Int_TryParseInv(valTrimmed, out int result);
            config.TopRightTabsData.ModsTab.DisplayIndex = result;
        }
        private static void Config_ModsTabVisible_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.TopRightTabsData.ModsTab.Visible = valTrimmed.EqualsTrue();
        }

        #endregion

        private static void Config_ReadmeZoomFactor_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            if (Float_TryParseInv(valTrimmed, out float result))
            {
                config.ReadmeZoomFactor = result;
            }
        }
        private static void Config_ReadmeUseFixedWidthFont_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.ReadmeUseFixedWidthFont = valTrimmed.EqualsTrue();
        }
        private static void Config_EnableCharacterDetailFix_Set(ConfigData config, string valTrimmed, string valRaw)
        {
            config.EnableCharacterDetailFix = valTrimmed.EqualsTrue();
        }

        #endregion

        private static readonly Dictionary<string, Action<ConfigData, string, string>> _actionDict_Config = new()
        {
            #region Settings window state

            { "SettingsTab", Config_SettingsTab_Set },
            { "SettingsWindowSize", Config_SettingsWindowSize_Set },
            { "SettingsWindowSplitterDistance", Config_SettingsWindowSplitterDistance_Set },
            { "SettingsPathsVScrollPos", Config_SettingsPathsVScrollPos_Set },
            { "SettingsAppearanceVScrollPos", Config_SettingsAppearanceVScrollPos_Set },
            { "SettingsOtherVScrollPos", Config_SettingsOtherVScrollPos_Set },

            #endregion

            { "LaunchGamesWithSteam", Config_LaunchGamesWithSteam_Set },
            { "SteamExe", Config_SteamExe_Set },
            { "FMsBackupPath", Config_FMsBackupPath_Set },
            { "FMArchivePath", Config_FMArchivePath_Set },
            { "FMArchivePathsIncludeSubfolders", Config_FMArchivePathsIncludeSubfolders_Set },
            { "GameOrganization", Config_GameOrganization_Set },
            { "UseShortGameTabNames", Config_UseShortGameTabNames_Set },
            { "EnableArticles", Config_EnableArticles_Set },
            { "Articles", Config_Articles_Set },
            { "MoveArticlesToEnd", Config_MoveArticlesToEnd_Set },
            { "RatingDisplayStyle", Config_RatingDisplayStyle_Set },
            { "RatingUseStars", Config_RatingUseStars_Set },

            #region Date format

            { "DateFormat", Config_DateFormat_Set },
            { "DateCustomFormat1", Config_DateCustomFormat1_Set },
            { "DateCustomSeparator1", Config_DateCustomSeparator1_Set },
            { "DateCustomFormat2", Config_DateCustomFormat2_Set },
            { "DateCustomSeparator2", Config_DateCustomSeparator2_Set },
            { "DateCustomFormat3", Config_DateCustomFormat3_Set },
            { "DateCustomSeparator3", Config_DateCustomSeparator3_Set },
            { "DateCustomFormat4", Config_DateCustomFormat4_Set },

            #endregion

            { "DaysRecent", Config_DaysRecent_Set },
            { "ConvertWAVsTo16BitOnInstall", Config_ConvertWAVsTo16BitOnInstall_Set },
            { "ConvertOGGsToWAVsOnInstall", Config_ConvertOGGsToWAVsOnInstall_Set },
            { "HideUninstallButton", Config_HideUninstallButton_Set },
            { "HideFMListZoomButtons", Config_HideFMListZoomButtons_Set },
            { "HideExitButton", Config_HideExitButton_Set },
            { "ConfirmUninstall", Config_ConfirmUninstall_Set },
            { "BackupFMData", Config_BackupFMData_Set },
            { "BackupAlwaysAsk", Config_BackupAlwaysAsk_Set },
            { "Language", Config_Language_Set },
            { "WebSearchUrl", Config_WebSearchUrl_Set },
            { "ConfirmPlayOnDCOrEnter", Config_ConfirmPlayOnDCOrEnter_Set },
            { "VisualTheme", Config_VisualTheme_Set },

            #region Filter visibilities

            { "FilterVisibleTitle", Config_FilterVisibleTitle_Set },
            { "FilterVisibleAuthor", Config_FilterVisibleAuthor_Set },
            { "FilterVisibleReleaseDate", Config_FilterVisibleReleaseDate_Set },
            { "FilterVisibleLastPlayed", Config_FilterVisibleLastPlayed_Set },
            { "FilterVisibleTags", Config_FilterVisibleTags_Set },
            { "FilterVisibleFinishedState", Config_FilterVisibleFinishedState_Set },
            { "FilterVisibleRating", Config_FilterVisibleRating_Set },
            { "FilterVisibleShowUnsupported", Config_FilterVisibleShowUnsupported_Set },
            { "FilterVisibleShowUnavailable", Config_FilterVisibleShowUnavailable_Set },
            { "FilterVisibleShowRecentAtTop", Config_FilterVisibleShowRecentAtTop_Set },

            #endregion

            #region Global filter values

            { "FilterGames", Config_FilterGames_Set },
            { "FilterTitle", Config_FilterTitle_Set },
            { "FilterAuthor", Config_FilterAuthor_Set },
            { "FilterReleaseDateFrom", Config_FilterReleaseDateFrom_Set },
            { "FilterReleaseDateTo", Config_FilterReleaseDateTo_Set },
            { "FilterLastPlayedFrom", Config_FilterLastPlayedFrom_Set },
            { "FilterLastPlayedTo", Config_FilterLastPlayedTo_Set },
            { "FilterFinishedStates", Config_FilterFinishedStates_Set },
            { "FilterRatingFrom", Config_FilterRatingFrom_Set },
            { "FilterRatingTo", Config_FilterRatingTo_Set },
            { "FilterTagsAnd", Config_FilterTagsAnd_Set },
            { "FilterTagsOr", Config_FilterTagsOr_Set },
            { "FilterTagsNot", Config_FilterTagsNot_Set },

            #endregion

            #region Per-game fields

            // @GENGAMES (ConfigIni key dictionary) - Begin

            { "T1Exe", Config_T1Exe_Set },
            { "T2Exe", Config_T2Exe_Set },
            { "T3Exe", Config_T3Exe_Set },
            { "SS2Exe", Config_SS2Exe_Set },

            { "T1UseSteam", Config_T1UseSteam_Set },
            { "T2UseSteam", Config_T2UseSteam_Set },
            { "T3UseSteam", Config_T3UseSteam_Set },
            { "SS2UseSteam", Config_SS2UseSteam_Set },

            { "GameFilterVisibleT1", Config_GameFilterVisibleT1_Set },
            { "GameFilterVisibleT2", Config_GameFilterVisibleT2_Set },
            { "GameFilterVisibleT3", Config_GameFilterVisibleT3_Set },
            { "GameFilterVisibleSS2", Config_GameFilterVisibleSS2_Set },

            { "T1FilterTitle", Config_T1FilterTitle_Set },
            { "T2FilterTitle", Config_T2FilterTitle_Set },
            { "T3FilterTitle", Config_T3FilterTitle_Set },
            { "SS2FilterTitle", Config_SS2FilterTitle_Set },

            { "T1FilterAuthor", Config_T1FilterAuthor_Set },
            { "T2FilterAuthor", Config_T2FilterAuthor_Set },
            { "T3FilterAuthor", Config_T3FilterAuthor_Set },
            { "SS2FilterAuthor", Config_SS2FilterAuthor_Set },

            { "T1FilterReleaseDateFrom", Config_T1FilterReleaseDateFrom_Set },
            { "T2FilterReleaseDateFrom", Config_T2FilterReleaseDateFrom_Set },
            { "T3FilterReleaseDateFrom", Config_T3FilterReleaseDateFrom_Set },
            { "SS2FilterReleaseDateFrom", Config_SS2FilterReleaseDateFrom_Set },

            { "T1FilterReleaseDateTo", Config_T1FilterReleaseDateTo_Set },
            { "T2FilterReleaseDateTo", Config_T2FilterReleaseDateTo_Set },
            { "T3FilterReleaseDateTo", Config_T3FilterReleaseDateTo_Set },
            { "SS2FilterReleaseDateTo", Config_SS2FilterReleaseDateTo_Set },

            { "T1FilterLastPlayedFrom", Config_T1FilterLastPlayedFrom_Set },
            { "T2FilterLastPlayedFrom", Config_T2FilterLastPlayedFrom_Set },
            { "T3FilterLastPlayedFrom", Config_T3FilterLastPlayedFrom_Set },
            { "SS2FilterLastPlayedFrom", Config_SS2FilterLastPlayedFrom_Set },

            { "T1FilterLastPlayedTo", Config_T1FilterLastPlayedTo_Set },
            { "T2FilterLastPlayedTo", Config_T2FilterLastPlayedTo_Set },
            { "T3FilterLastPlayedTo", Config_T3FilterLastPlayedTo_Set },
            { "SS2FilterLastPlayedTo", Config_SS2FilterLastPlayedTo_Set },

            { "T1FilterFinishedStates", Config_T1FilterFinishedStates_Set },
            { "T2FilterFinishedStates", Config_T2FilterFinishedStates_Set },
            { "T3FilterFinishedStates", Config_T3FilterFinishedStates_Set },
            { "SS2FilterFinishedStates", Config_SS2FilterFinishedStates_Set },

            { "T1FilterRatingFrom", Config_T1FilterRatingFrom_Set },
            { "T2FilterRatingFrom", Config_T2FilterRatingFrom_Set },
            { "T3FilterRatingFrom", Config_T3FilterRatingFrom_Set },
            { "SS2FilterRatingFrom", Config_SS2FilterRatingFrom_Set },

            { "T1FilterRatingTo", Config_T1FilterRatingTo_Set },
            { "T2FilterRatingTo", Config_T2FilterRatingTo_Set },
            { "T3FilterRatingTo", Config_T3FilterRatingTo_Set },
            { "SS2FilterRatingTo", Config_SS2FilterRatingTo_Set },

            { "T1FilterTagsAnd", Config_T1FilterTagsAnd_Set },
            { "T2FilterTagsAnd", Config_T2FilterTagsAnd_Set },
            { "T3FilterTagsAnd", Config_T3FilterTagsAnd_Set },
            { "SS2FilterTagsAnd", Config_SS2FilterTagsAnd_Set },

            { "T1FilterTagsOr", Config_T1FilterTagsOr_Set },
            { "T2FilterTagsOr", Config_T2FilterTagsOr_Set },
            { "T3FilterTagsOr", Config_T3FilterTagsOr_Set },
            { "SS2FilterTagsOr", Config_SS2FilterTagsOr_Set },

            { "T1FilterTagsNot", Config_T1FilterTagsNot_Set },
            { "T2FilterTagsNot", Config_T2FilterTagsNot_Set },
            { "T3FilterTagsNot", Config_T3FilterTagsNot_Set },
            { "SS2FilterTagsNot", Config_SS2FilterTagsNot_Set },

            { "T1SelFMInstDir", Config_T1SelFMInstDir_Set },
            { "T2SelFMInstDir", Config_T2SelFMInstDir_Set },
            { "T3SelFMInstDir", Config_T3SelFMInstDir_Set },
            { "SS2SelFMInstDir", Config_SS2SelFMInstDir_Set },

            { "T1SelFMIndexFromTop", Config_T1SelFMIndexFromTop_Set },
            { "T2SelFMIndexFromTop", Config_T2SelFMIndexFromTop_Set },
            { "T3SelFMIndexFromTop", Config_T3SelFMIndexFromTop_Set },
            { "SS2SelFMIndexFromTop", Config_SS2SelFMIndexFromTop_Set },

            // @GENGAMES (ConfigIni key dictionary) - End

            #endregion

            { "ShowRecentAtTop", Config_ShowRecentAtTop_Set },
            { "ShowUnsupported", Config_ShowUnsupported_Set },
            { "ShowUnavailableFMs", Config_ShowUnavailableFMs_Set },
            { "FMsListFontSizeInPoints", Config_FMsListFontSizeInPoints_Set },

            { "SortedColumn", Config_SortedColumn_Set },
            { "SortDirection", Config_SortDirection_Set },

            #region Columns

            { "ColumnGame", Config_ColumnGame_Set },
            { "ColumnInstalled", Config_ColumnInstalled_Set },
            { "ColumnTitle", Config_ColumnTitle_Set },
            { "ColumnArchive", Config_ColumnArchive_Set },
            { "ColumnAuthor", Config_ColumnAuthor_Set },
            { "ColumnSize", Config_ColumnSize_Set },
            { "ColumnRating", Config_ColumnRating_Set },
            { "ColumnFinished", Config_ColumnFinished_Set },
            { "ColumnReleaseDate", Config_ColumnReleaseDate_Set },
            { "ColumnLastPlayed", Config_ColumnLastPlayed_Set },
            { "ColumnDateAdded", Config_ColumnDateAdded_Set },
            { "ColumnDisabledMods", Config_ColumnDisabledMods_Set },
            { "ColumnComment", Config_ColumnComment_Set },

            #endregion

            { "SelFMInstDir", Config_SelFMInstDir_Set },
            { "SelFMIndexFromTop", Config_SelFMIndexFromTop_Set },

            { "MainWindowState", Config_MainWindowState_Set },
            { "MainWindowSize", Config_MainWindowSize_Set },
            { "MainWindowLocation", Config_MainWindowLocation_Set },

            { "MainSplitterPercent", Config_MainSplitterPercent_Set },
            { "TopSplitterPercent", Config_TopSplitterPercent_Set },
            { "TopRightPanelCollapsed", Config_TopRightPanelCollapsed_Set },
            { "GameTab", Config_GameTab_Set },

            #region Top-right tabs

            { "TopRightTab", Config_TopRightTab_Set },

            { "StatsTabPosition", Config_StatsTabPosition_Set },
            { "StatsTabVisible", Config_StatsTabVisible_Set },

            { "EditFMTabPosition", Config_EditFMTabPosition_Set },
            { "EditFMTabVisible", Config_EditFMTabVisible_Set },

            { "CommentTabPosition", Config_CommentTabPosition_Set },
            { "CommentTabVisible", Config_CommentTabVisible_Set },

            { "TagsTabPosition", Config_TagsTabPosition_Set },
            { "TagsTabVisible", Config_TagsTabVisible_Set },

            { "PatchTabPosition", Config_PatchTabPosition_Set },
            { "PatchTabVisible", Config_PatchTabVisible_Set },

            { "ModsTabPosition", Config_ModsTabPosition_Set },
            { "ModsTabVisible", Config_ModsTabVisible_Set },

            #endregion

            { "ReadmeZoomFactor", Config_ReadmeZoomFactor_Set },
            { "ReadmeUseFixedWidthFont", Config_ReadmeUseFixedWidthFont_Set },
            { "EnableCharacterDetailFix", Config_EnableCharacterDetailFix_Set },
        };

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

            #endregion

            #region Filters

            for (int i = 0; i < SupportedGameCount; i++)
            {
                sb.Append("GameFilterVisible").Append(GetGamePrefix((GameIndex)i)).Append('=').AppendLine(config.GameFilterControlVisibilities[i].ToString());
            }

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
            sb.Append("ModsTabPosition=").Append(config.TopRightTabsData.ModsTab.DisplayIndex).AppendLine();

            sb.Append("StatsTabVisible=").Append(config.TopRightTabsData.StatsTab.Visible).AppendLine();
            sb.Append("EditFMTabVisible=").Append(config.TopRightTabsData.EditFMTab.Visible).AppendLine();
            sb.Append("CommentTabVisible=").Append(config.TopRightTabsData.CommentTab.Visible).AppendLine();
            sb.Append("TagsTabVisible=").Append(config.TopRightTabsData.TagsTab.Visible).AppendLine();
            sb.Append("PatchTabVisible=").Append(config.TopRightTabsData.PatchTab.Visible).AppendLine();
            sb.Append("ModsTabVisible=").Append(config.TopRightTabsData.ModsTab.Visible).AppendLine();

            sb.Append("ReadmeZoomFactor=").AppendLine(config.ReadmeZoomFactor.ToString(NumberFormatInfo.InvariantInfo));
            sb.Append("ReadmeUseFixedWidthFont=").Append(config.ReadmeUseFixedWidthFont).AppendLine();

            #endregion

            sb.Append("EnableCharacterDetailFix=").Append(config.EnableCharacterDetailFix).AppendLine();

            using var sw = new StreamWriter(fileName, false, Encoding.UTF8);
            sw.Write(sb.ToString());
        }
    }
}
