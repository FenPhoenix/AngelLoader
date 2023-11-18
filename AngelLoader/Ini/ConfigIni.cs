#define FenGen_ConfigDest

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader;

// We still don't generate this, because complication yadda yadda.
// But we use a game prefix detector that brings back the per-game automation at runtime, but way faster
// than before. So we're only a little slower than full code generation, but way less error-prone than fully
// manually written per-game duplicated code.

// This file should have had sections from the start, but now that it got released without, we can't really
// change it without breaking compatibility. Oh well.

// A note about floats:
// When storing and reading floats, it's imperative that we specify InvariantInfo. Otherwise the decimal
// separator is culture-dependent, and it could end up as ',' when we expect '.'. And then we could end up
// with "5.001" being "5,001", and now we're in for a bad time.

internal static partial class Ini
{
    #region Config reader setters

    #region Settings window state

    private static void Config_SettingsTab_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(SettingsTab).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.SettingsTab = (SettingsTab)field.GetValue(null);
        }
    }

    private static void Config_SettingsWindowSize_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (TryParseIntPair(valTrimmed, out int width, out int height))
        {
            config.SettingsWindowSize = new Size(width, height);
        }
    }

    private static void Config_SettingsWindowSplitterDistance_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            config.SettingsWindowSplitterDistance = result;
        }
    }

    private static void Config_SettingsPathsVScrollPos_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            config.SetSettingsTabVScrollPos(SettingsTab.Paths, result);
        }
    }

    private static void Config_SettingsAppearanceVScrollPos_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            config.SetSettingsTabVScrollPos(SettingsTab.Appearance, result);
        }
    }

    private static void Config_SettingsOtherVScrollPos_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            config.SetSettingsTabVScrollPos(SettingsTab.Other, result);
        }
    }

    private static void Config_SettingsThiefBuddyVScrollPos_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            config.SetSettingsTabVScrollPos(SettingsTab.ThiefBuddy, result);
        }
    }

    #endregion

    private static void Config_LaunchGamesWithSteam_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.LaunchGamesWithSteam = valTrimmed.EqualsTrue();
    }

    private static void Config_SteamExe_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.SteamExe = valTrimmed.ToString();
    }

    private static void Config_RunThiefBuddyOnFMPlay_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(RunThiefBuddyOnFMPlay).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.RunThiefBuddyOnFMPlay = (RunThiefBuddyOnFMPlay)field.GetValue(null);
        }
    }

    private static void Config_FMsBackupPath_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FMsBackupPath = valTrimmed.ToString();
    }

    private static void Config_FMArchivePath_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FMArchivePaths.Add(valTrimmed.ToString());
    }

    private static void Config_FMArchivePathsIncludeSubfolders_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FMArchivePathsIncludeSubfolders = valTrimmed.EqualsTrue();
    }

    private static void Config_GameOrganization_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(GameOrganization).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.GameOrganization = (GameOrganization)field.GetValue(null);
        }
    }

    private static void Config_UseShortGameTabNames_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.UseShortGameTabNames = valTrimmed.EqualsTrue();
    }

    private static void Config_EnableArticles_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.EnableArticles = valTrimmed.EqualsTrue();
    }

    private static void Config_Articles_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        string[] articles = valTrimmed.ToString().Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);
        for (int a = 0; a < articles.Length; a++) articles[a] = articles[a].Trim();
        config.Articles.ClearAndAdd_Small(articles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static void Config_MoveArticlesToEnd_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.MoveArticlesToEnd = valTrimmed.EqualsTrue();
    }

    private static void Config_RatingDisplayStyle_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(RatingDisplayStyle).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.RatingDisplayStyle = (RatingDisplayStyle)field.GetValue(null);
        }
    }

    private static void Config_RatingUseStars_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.RatingUseStars = valTrimmed.EqualsTrue();
    }

    #region Date format

    private static void Config_DateFormat_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(DateFormat).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.DateFormat = (DateFormat)field.GetValue(null);
        }
    }

    private static void Config_DateCustomFormat1_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.DateCustomFormat1 = valRaw.ToString();
    }

    private static void Config_DateCustomSeparator1_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.DateCustomSeparator1 = valRaw.ToString();
    }

    private static void Config_DateCustomFormat2_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.DateCustomFormat2 = valRaw.ToString();
    }

    private static void Config_DateCustomSeparator2_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.DateCustomSeparator2 = valRaw.ToString();
    }

    private static void Config_DateCustomFormat3_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.DateCustomFormat3 = valRaw.ToString();
    }

    private static void Config_DateCustomSeparator3_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.DateCustomSeparator3 = valRaw.ToString();
    }

    private static void Config_DateCustomFormat4_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.DateCustomFormat4 = valRaw.ToString();
    }

    #endregion

    private static void Config_DaysRecent_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (UInt_TryParseInv(valTrimmed, out uint result))
        {
            config.DaysRecent = result;
        }
    }

    private static void Config_ConvertWAVsTo16BitOnInstall_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.ConvertWAVsTo16BitOnInstall = valTrimmed.EqualsTrue();
    }

    private static void Config_ConvertOGGsToWAVsOnInstall_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.ConvertOGGsToWAVsOnInstall = valTrimmed.EqualsTrue();
    }

    private static void Config_UseOldMantlingForOldDarkFMs_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.UseOldMantlingForOldDarkFMs = valTrimmed.EqualsTrue();
    }

    private static void Config_HideUninstallButton_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.HideUninstallButton = valTrimmed.EqualsTrue();
    }

    private static void Config_HideFMListZoomButtons_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.HideFMListZoomButtons = valTrimmed.EqualsTrue();
    }

    private static void Config_HideExitButton_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.HideExitButton = valTrimmed.EqualsTrue();
    }

    private static void Config_HideWebSearchButton_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.HideWebSearchButton = valTrimmed.EqualsTrue();
    }

    private static void Config_ConfirmInstall_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(ConfirmBeforeInstall).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.ConfirmBeforeInstall = (ConfirmBeforeInstall)field.GetValue(null);
        }
    }

    private static void Config_ConfirmUninstall_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.ConfirmUninstall = valTrimmed.EqualsTrue();
    }

    private static void Config_BackupFMData_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(BackupFMData).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.BackupFMData = (BackupFMData)field.GetValue(null);
        }
    }

    private static void Config_BackupAlwaysAsk_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.BackupAlwaysAsk = valTrimmed.EqualsTrue();
    }

    private static void Config_Language_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.Language = valTrimmed.ToString();
    }

    private static void Config_WebSearchUrl_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.SetWebSearchUrl(gameIndex, valRaw.ToString());
    }

    private static void Config_ConfirmPlayOnDCOrEnter_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.ConfirmPlayOnDCOrEnter = valTrimmed.EqualsTrue();
    }

    private static void Config_VisualTheme_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(VisualTheme).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.VisualTheme = (VisualTheme)field.GetValue(null);
        }
    }

    #region Filter visibilities

    private static void Config_FilterVisibleTitle_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FilterControlVisibilities[(int)HideableFilterControls.Title] = valTrimmed.EqualsTrue();
    }
    private static void Config_FilterVisibleAuthor_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FilterControlVisibilities[(int)HideableFilterControls.Author] = valTrimmed.EqualsTrue();
    }
    private static void Config_FilterVisibleReleaseDate_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FilterControlVisibilities[(int)HideableFilterControls.ReleaseDate] = valTrimmed.EqualsTrue();
    }
    private static void Config_FilterVisibleLastPlayed_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FilterControlVisibilities[(int)HideableFilterControls.LastPlayed] = valTrimmed.EqualsTrue();
    }
    private static void Config_FilterVisibleTags_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FilterControlVisibilities[(int)HideableFilterControls.Tags] = valTrimmed.EqualsTrue();
    }
    private static void Config_FilterVisibleFinishedState_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FilterControlVisibilities[(int)HideableFilterControls.FinishedState] = valTrimmed.EqualsTrue();
    }
    private static void Config_FilterVisibleRating_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FilterControlVisibilities[(int)HideableFilterControls.Rating] = valTrimmed.EqualsTrue();
    }
    private static void Config_FilterVisibleShowUnsupported_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FilterControlVisibilities[(int)HideableFilterControls.ShowUnsupported] = valTrimmed.EqualsTrue();
    }
    private static void Config_FilterVisibleShowUnavailable_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FilterControlVisibilities[(int)HideableFilterControls.ShowUnavailable] = valTrimmed.EqualsTrue();
    }
    private static void Config_FilterVisibleShowRecentAtTop_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.FilterControlVisibilities[(int)HideableFilterControls.ShowRecentAtTop] = valTrimmed.EqualsTrue();
    }

    #endregion

    #region Filter values

    private static void Config_FilterGames_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        string[] iniGames = valTrimmed.ToString().Split(CA_Comma, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < iniGames.Length; i++)
        {
            string iniGame = iniGames[i].Trim();
            FieldInfo? field = typeof(Game).GetField(iniGame, _bFlagsEnum);
            if (field != null)
            {
                config.Filter.Games |= (Game)field.GetValue(null);
            }
        }
    }

    private static void Config_FilterTitle_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        GetFilter(config, gameIndex, ignoreGameIndex).Title = valRaw.ToString();
    }

    private static void Config_FilterAuthor_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        GetFilter(config, gameIndex, ignoreGameIndex).Author = valRaw.ToString();
    }

    private static void Config_FilterReleaseDateFrom_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        GetFilter(config, gameIndex, ignoreGameIndex).ReleaseDateFrom = ConvertHexUnixDateToDateTime(valTrimmed);
    }

    private static void Config_FilterReleaseDateTo_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        GetFilter(config, gameIndex, ignoreGameIndex).ReleaseDateTo = ConvertHexUnixDateToDateTime(valTrimmed);
    }

    private static void Config_FilterLastPlayedFrom_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        GetFilter(config, gameIndex, ignoreGameIndex).LastPlayedFrom = ConvertHexUnixDateToDateTime(valTrimmed);
    }

    private static void Config_FilterLastPlayedTo_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        GetFilter(config, gameIndex, ignoreGameIndex).LastPlayedTo = ConvertHexUnixDateToDateTime(valTrimmed);
    }

    private static void Config_FilterFinishedStates_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        ReadFinishedStates(GetFilter(config, gameIndex, ignoreGameIndex), valTrimmed);
    }

    private static void Config_FilterRatingFrom_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            GetFilter(config, gameIndex, ignoreGameIndex).RatingFrom = result;
        }
    }

    private static void Config_FilterRatingTo_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            GetFilter(config, gameIndex, ignoreGameIndex).RatingTo = result;
        }
    }

    private static void Config_FilterTagsAnd_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        ReadFilterTags(valRaw, GetFilter(config, gameIndex, ignoreGameIndex).Tags.AndTags);
    }

    private static void Config_FilterTagsOr_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        ReadFilterTags(valRaw, GetFilter(config, gameIndex, ignoreGameIndex).Tags.OrTags);
    }

    private static void Config_FilterTagsNot_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        ReadFilterTags(valRaw, GetFilter(config, gameIndex, ignoreGameIndex).Tags.NotTags);
    }

    #endregion

    #region Per-game fields

    private static void Config_NewMantling_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.SetNewMantling(gameIndex, valTrimmed.EqualsTrue() ? true : valTrimmed.EqualsFalse() ? false : null);
    }

    private static void Config_DisabledMods_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.SetDisabledMods(gameIndex, valTrimmed.ToString());
    }

    private static void Config_Exe_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.SetGameExe(gameIndex, valTrimmed.ToString());
    }

    private static void Config_UseSteam_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.SetUseSteamSwitch(gameIndex, valTrimmed.EqualsTrue());
    }

    private static void Config_GameFilterVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.GameFilterControlVisibilities[(int)gameIndex] = valTrimmed.EqualsTrue();
    }

    #region Backward-compatible game filter visibility setters

    // We don't need to mark this with a GENGAMES tag or anything, because we never need to touch it again.
    // Even if we add new games, they don't need to go here, because they'll use the new key name format
    // (game prefixed instead of suffixed).

    private static void Config_GameFilterVisibleT1_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.GameFilterControlVisibilities[(int)GameIndex.Thief1] = valTrimmed.EqualsTrue();
    }
    private static void Config_GameFilterVisibleT2_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.GameFilterControlVisibilities[(int)GameIndex.Thief2] = valTrimmed.EqualsTrue();
    }
    private static void Config_GameFilterVisibleT3_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.GameFilterControlVisibilities[(int)GameIndex.Thief3] = valTrimmed.EqualsTrue();
    }
    private static void Config_GameFilterVisibleSS2_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.GameFilterControlVisibilities[(int)GameIndex.SS2] = valTrimmed.EqualsTrue();
    }

    #endregion

    #endregion

    private static void Config_ShowRecentAtTop_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.ShowRecentAtTop = valTrimmed.EqualsTrue();
    }
    private static void Config_ShowUnsupported_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.ShowUnsupported = valTrimmed.EqualsTrue();
    }
    private static void Config_ShowUnavailableFMs_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.ShowUnavailableFMs = valTrimmed.EqualsTrue();
    }
    private static void Config_FMsListFontSizeInPoints_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Float_TryParseInv(valTrimmed, out float result))
        {
            config.FMsListFontSizeInPoints = result;
        }
    }

    private static void Config_SortedColumn_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(Column).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.SortedColumn = (Column)field.GetValue(null);
        }
    }
    private static void Config_SortDirection_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(SortDirection).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.SortDirection = (SortDirection)field.GetValue(null);
        }
    }

    #region Columns

#if DateAccTest
    private static void Config_ColumnDateAccuracy_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.DateAccuracy);
    }
#endif

    private static void Config_ColumnGame_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.Game);
    }
    private static void Config_ColumnInstalled_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.Installed);
    }
    private static void Config_ColumnMisCount_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.MissionCount);
    }
    private static void Config_ColumnTitle_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.Title);
    }
    private static void Config_ColumnArchive_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.Archive);
    }
    private static void Config_ColumnAuthor_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.Author);
    }
    private static void Config_ColumnSize_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.Size);
    }
    private static void Config_ColumnRating_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.Rating);
    }
    private static void Config_ColumnFinished_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.Finished);
    }
    private static void Config_ColumnReleaseDate_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.ReleaseDate);
    }
    private static void Config_ColumnLastPlayed_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.LastPlayed);
    }
    private static void Config_ColumnDateAdded_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.DateAdded);
    }
    private static void Config_ColumnDisabledMods_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.DisabledMods);
    }
    private static void Config_ColumnComment_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.Comment);
    }

    #endregion

    private static void Config_SelFMInstDir_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        GetSelectedFM(config, gameIndex, ignoreGameIndex).InstalledName = valTrimmed.ToString();
    }
    private static void Config_SelFMIndexFromTop_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            GetSelectedFM(config, gameIndex, ignoreGameIndex).IndexFromTop = result;
        }
    }

    private static void Config_MainWindowState_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(WindowState).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.MainWindowState = (WindowState)field.GetValue(null);
        }
    }
    private static void Config_MainWindowSize_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (TryParseIntPair(valTrimmed, out int width, out int height))
        {
            config.MainWindowSize = new Size(width, height);
        }
    }
    private static void Config_MainWindowLocation_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (TryParseIntPair(valTrimmed, out int x, out int y))
        {
            config.MainWindowLocation = new Point(x, y);
        }
    }

    private static void Config_MainSplitterPercent_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Float_TryParseInv(valTrimmed, out float result))
        {
            config.MainSplitterPercent = result;
        }
    }
    private static void Config_TopSplitterPercent_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Float_TryParseInv(valTrimmed, out float result))
        {
            config.TopSplitterPercent = result;
        }
    }
    private static void Config_TopRightPanelCollapsed_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.TopRightPanelCollapsed = valTrimmed.EqualsTrue();
    }
    private static void Config_GameTab_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(GameIndex).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.GameTab = (GameIndex)field.GetValue(null);
        }
    }

    #region Top-right tabs

    private static void Config_TopRightTab_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(TopRightTab).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.TopRightTabsData.SelectedTab = (TopRightTab)field.GetValue(null);
        }
    }

    private static void Config_StatsTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.TopRightTabsData.GetTab(TopRightTab.Statistics).DisplayIndex = result;
    }
    private static void Config_StatsTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.TopRightTabsData.GetTab(TopRightTab.Statistics).Visible = valTrimmed.EqualsTrue();
    }

    private static void Config_EditFMTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.TopRightTabsData.GetTab(TopRightTab.EditFM).DisplayIndex = result;
    }
    private static void Config_EditFMTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.TopRightTabsData.GetTab(TopRightTab.EditFM).Visible = valTrimmed.EqualsTrue();
    }

    private static void Config_CommentTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.TopRightTabsData.GetTab(TopRightTab.Comment).DisplayIndex = result;
    }
    private static void Config_CommentTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.TopRightTabsData.GetTab(TopRightTab.Comment).Visible = valTrimmed.EqualsTrue();
    }

    private static void Config_TagsTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.TopRightTabsData.GetTab(TopRightTab.Tags).DisplayIndex = result;
    }
    private static void Config_TagsTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.TopRightTabsData.GetTab(TopRightTab.Tags).Visible = valTrimmed.EqualsTrue();
    }

    private static void Config_PatchTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.TopRightTabsData.GetTab(TopRightTab.Patch).DisplayIndex = result;
    }
    private static void Config_PatchTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.TopRightTabsData.GetTab(TopRightTab.Patch).Visible = valTrimmed.EqualsTrue();
    }

    private static void Config_ModsTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.TopRightTabsData.GetTab(TopRightTab.Mods).DisplayIndex = result;
    }
    private static void Config_ModsTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.TopRightTabsData.GetTab(TopRightTab.Mods).Visible = valTrimmed.EqualsTrue();
    }

    #endregion

    private static void Config_ReadmeZoomFactor_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Float_TryParseInv(valTrimmed, out float result))
        {
            config.ReadmeZoomFactor = result;
        }
    }
    private static void Config_ReadmeUseFixedWidthFont_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.ReadmeUseFixedWidthFont = valTrimmed.EqualsTrue();
    }
    private static void Config_EnableCharacterDetailFix_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.EnableCharacterDetailFix = valTrimmed.EqualsTrue();
    }

    private static void Config_PlayOriginalSeparateButtons_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.PlayOriginalSeparateButtons = valTrimmed.EqualsTrue();
    }

    private static void Config_AskedToScanForMisCounts_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.AskedToScanForMisCounts = valTrimmed.EqualsTrue();
    }

    private static void Config_EnableFuzzySearch_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.EnableFuzzySearch = valTrimmed.EqualsTrue();
    }

    #endregion

    private sealed unsafe class Config_DelegatePointerWrapper
    {
        internal readonly delegate*<ConfigData, ReadOnlySpan<char>, ReadOnlySpan<char>, GameIndex, bool, void> Action;

        internal Config_DelegatePointerWrapper(delegate*<ConfigData, ReadOnlySpan<char>, ReadOnlySpan<char>, GameIndex, bool, void> action)
        {
            Action = action;
        }
    }

    private static unsafe Dictionary<string, Config_DelegatePointerWrapper>
    CreateConfigDictionary() => new(new KeyComparer())
    {
        #region Settings window state

        { "SettingsTab", new Config_DelegatePointerWrapper(&Config_SettingsTab_Set) },
        { "SettingsWindowSize", new Config_DelegatePointerWrapper(&Config_SettingsWindowSize_Set) },
        { "SettingsWindowSplitterDistance", new Config_DelegatePointerWrapper(&Config_SettingsWindowSplitterDistance_Set) },
        { "SettingsPathsVScrollPos", new Config_DelegatePointerWrapper(&Config_SettingsPathsVScrollPos_Set) },
        { "SettingsAppearanceVScrollPos", new Config_DelegatePointerWrapper(&Config_SettingsAppearanceVScrollPos_Set) },
        { "SettingsOtherVScrollPos", new Config_DelegatePointerWrapper(&Config_SettingsOtherVScrollPos_Set) },
        { "SettingsThiefBuddyVScrollPos", new Config_DelegatePointerWrapper(&Config_SettingsThiefBuddyVScrollPos_Set) },

        #endregion

        { "LaunchGamesWithSteam", new Config_DelegatePointerWrapper(&Config_LaunchGamesWithSteam_Set) },
        { "SteamExe", new Config_DelegatePointerWrapper(&Config_SteamExe_Set) },
        { "RunThiefBuddyOnFMPlay", new Config_DelegatePointerWrapper(&Config_RunThiefBuddyOnFMPlay_Set) },
        { "FMsBackupPath", new Config_DelegatePointerWrapper(&Config_FMsBackupPath_Set) },
        { "FMArchivePath", new Config_DelegatePointerWrapper(&Config_FMArchivePath_Set) },
        { "FMArchivePathsIncludeSubfolders", new Config_DelegatePointerWrapper(&Config_FMArchivePathsIncludeSubfolders_Set) },
        { "GameOrganization", new Config_DelegatePointerWrapper(&Config_GameOrganization_Set) },
        { "UseShortGameTabNames", new Config_DelegatePointerWrapper(&Config_UseShortGameTabNames_Set) },
        { "EnableArticles", new Config_DelegatePointerWrapper(&Config_EnableArticles_Set) },
        { "Articles", new Config_DelegatePointerWrapper(&Config_Articles_Set) },
        { "MoveArticlesToEnd", new Config_DelegatePointerWrapper(&Config_MoveArticlesToEnd_Set) },
        { "RatingDisplayStyle", new Config_DelegatePointerWrapper(&Config_RatingDisplayStyle_Set) },
        { "RatingUseStars", new Config_DelegatePointerWrapper(&Config_RatingUseStars_Set) },

        #region Date format

        { "DateFormat", new Config_DelegatePointerWrapper(&Config_DateFormat_Set) },
        { "DateCustomFormat1", new Config_DelegatePointerWrapper(&Config_DateCustomFormat1_Set) },
        { "DateCustomSeparator1", new Config_DelegatePointerWrapper(&Config_DateCustomSeparator1_Set) },
        { "DateCustomFormat2", new Config_DelegatePointerWrapper(&Config_DateCustomFormat2_Set) },
        { "DateCustomSeparator2", new Config_DelegatePointerWrapper(&Config_DateCustomSeparator2_Set) },
        { "DateCustomFormat3", new Config_DelegatePointerWrapper(&Config_DateCustomFormat3_Set) },
        { "DateCustomSeparator3", new Config_DelegatePointerWrapper(&Config_DateCustomSeparator3_Set) },
        { "DateCustomFormat4", new Config_DelegatePointerWrapper(&Config_DateCustomFormat4_Set) },

        #endregion

        { "DaysRecent", new Config_DelegatePointerWrapper(&Config_DaysRecent_Set) },
        { "ConvertWAVsTo16BitOnInstall", new Config_DelegatePointerWrapper(&Config_ConvertWAVsTo16BitOnInstall_Set) },
        { "ConvertOGGsToWAVsOnInstall", new Config_DelegatePointerWrapper(&Config_ConvertOGGsToWAVsOnInstall_Set) },
        { "UseOldMantlingForOldDarkFMs", new Config_DelegatePointerWrapper(&Config_UseOldMantlingForOldDarkFMs_Set) },
        { "HideUninstallButton", new Config_DelegatePointerWrapper(&Config_HideUninstallButton_Set) },
        { "HideFMListZoomButtons", new Config_DelegatePointerWrapper(&Config_HideFMListZoomButtons_Set) },
        { "HideExitButton", new Config_DelegatePointerWrapper(&Config_HideExitButton_Set) },
        { "HideWebSearchButton", new Config_DelegatePointerWrapper(&Config_HideWebSearchButton_Set) },
        { "ConfirmBeforeInstall", new Config_DelegatePointerWrapper(&Config_ConfirmInstall_Set) },
        { "ConfirmUninstall", new Config_DelegatePointerWrapper(&Config_ConfirmUninstall_Set) },
        { "BackupFMData", new Config_DelegatePointerWrapper(&Config_BackupFMData_Set) },
        { "BackupAlwaysAsk", new Config_DelegatePointerWrapper(&Config_BackupAlwaysAsk_Set) },
        { "Language", new Config_DelegatePointerWrapper(&Config_Language_Set) },
        { "WebSearchUrl", new Config_DelegatePointerWrapper(&Config_WebSearchUrl_Set) },
        { "ConfirmPlayOnDCOrEnter", new Config_DelegatePointerWrapper(&Config_ConfirmPlayOnDCOrEnter_Set) },
        { "VisualTheme", new Config_DelegatePointerWrapper(&Config_VisualTheme_Set) },

        #region Filter visibilities

        { "FilterVisibleTitle", new Config_DelegatePointerWrapper(&Config_FilterVisibleTitle_Set) },
        { "FilterVisibleAuthor", new Config_DelegatePointerWrapper(&Config_FilterVisibleAuthor_Set) },
        { "FilterVisibleReleaseDate", new Config_DelegatePointerWrapper(&Config_FilterVisibleReleaseDate_Set) },
        { "FilterVisibleLastPlayed", new Config_DelegatePointerWrapper(&Config_FilterVisibleLastPlayed_Set) },
        { "FilterVisibleTags", new Config_DelegatePointerWrapper(&Config_FilterVisibleTags_Set) },
        { "FilterVisibleFinishedState", new Config_DelegatePointerWrapper(&Config_FilterVisibleFinishedState_Set) },
        { "FilterVisibleRating", new Config_DelegatePointerWrapper(&Config_FilterVisibleRating_Set) },
        { "FilterVisibleShowUnsupported", new Config_DelegatePointerWrapper(&Config_FilterVisibleShowUnsupported_Set) },
        { "FilterVisibleShowUnavailable", new Config_DelegatePointerWrapper(&Config_FilterVisibleShowUnavailable_Set) },
        { "FilterVisibleShowRecentAtTop", new Config_DelegatePointerWrapper(&Config_FilterVisibleShowRecentAtTop_Set) },

        #endregion

        #region Filter values

        { "FilterGames", new Config_DelegatePointerWrapper(&Config_FilterGames_Set) },
        { "FilterTitle", new Config_DelegatePointerWrapper(&Config_FilterTitle_Set) },
        { "FilterAuthor", new Config_DelegatePointerWrapper(&Config_FilterAuthor_Set) },
        { "FilterReleaseDateFrom", new Config_DelegatePointerWrapper(&Config_FilterReleaseDateFrom_Set) },
        { "FilterReleaseDateTo", new Config_DelegatePointerWrapper(&Config_FilterReleaseDateTo_Set) },
        { "FilterLastPlayedFrom", new Config_DelegatePointerWrapper(&Config_FilterLastPlayedFrom_Set) },
        { "FilterLastPlayedTo", new Config_DelegatePointerWrapper(&Config_FilterLastPlayedTo_Set) },
        { "FilterFinishedStates", new Config_DelegatePointerWrapper(&Config_FilterFinishedStates_Set) },
        { "FilterRatingFrom", new Config_DelegatePointerWrapper(&Config_FilterRatingFrom_Set) },
        { "FilterRatingTo", new Config_DelegatePointerWrapper(&Config_FilterRatingTo_Set) },
        { "FilterTagsAnd", new Config_DelegatePointerWrapper(&Config_FilterTagsAnd_Set) },
        { "FilterTagsOr", new Config_DelegatePointerWrapper(&Config_FilterTagsOr_Set) },
        { "FilterTagsNot", new Config_DelegatePointerWrapper(&Config_FilterTagsNot_Set) },

        #endregion

        #region Per-game fields

        { "NewMantling", new Config_DelegatePointerWrapper(&Config_NewMantling_Set) },

        { "DisabledMods", new Config_DelegatePointerWrapper(&Config_DisabledMods_Set) },

        { "Exe", new Config_DelegatePointerWrapper(&Config_Exe_Set) },

        { "UseSteam", new Config_DelegatePointerWrapper(&Config_UseSteam_Set) },

        { "GameFilterVisible", new Config_DelegatePointerWrapper(&Config_GameFilterVisible_Set) },

        { "SelFMInstDir", new Config_DelegatePointerWrapper(&Config_SelFMInstDir_Set) },
        { "SelFMIndexFromTop", new Config_DelegatePointerWrapper(&Config_SelFMIndexFromTop_Set) },

        #endregion

        { "ShowRecentAtTop", new Config_DelegatePointerWrapper(&Config_ShowRecentAtTop_Set) },
        { "ShowUnsupported", new Config_DelegatePointerWrapper(&Config_ShowUnsupported_Set) },
        { "ShowUnavailableFMs", new Config_DelegatePointerWrapper(&Config_ShowUnavailableFMs_Set) },
        { "FMsListFontSizeInPoints", new Config_DelegatePointerWrapper(&Config_FMsListFontSizeInPoints_Set) },

        { "SortedColumn", new Config_DelegatePointerWrapper(&Config_SortedColumn_Set) },
        { "SortDirection", new Config_DelegatePointerWrapper(&Config_SortDirection_Set) },

        #region Columns

#if DateAccTest
        { "ColumnDateAccuracy", new Config_DelegatePointerWrapper(&Config_ColumnDateAccuracy_Set) },
#endif
        { "ColumnGame", new Config_DelegatePointerWrapper(&Config_ColumnGame_Set) },
        { "ColumnInstalled", new Config_DelegatePointerWrapper(&Config_ColumnInstalled_Set) },
        { "ColumnMissionCount", new Config_DelegatePointerWrapper(&Config_ColumnMisCount_Set) },
        { "ColumnTitle", new Config_DelegatePointerWrapper(&Config_ColumnTitle_Set) },
        { "ColumnArchive", new Config_DelegatePointerWrapper(&Config_ColumnArchive_Set) },
        { "ColumnAuthor", new Config_DelegatePointerWrapper(&Config_ColumnAuthor_Set) },
        { "ColumnSize", new Config_DelegatePointerWrapper(&Config_ColumnSize_Set) },
        { "ColumnRating", new Config_DelegatePointerWrapper(&Config_ColumnRating_Set) },
        { "ColumnFinished", new Config_DelegatePointerWrapper(&Config_ColumnFinished_Set) },
        { "ColumnReleaseDate", new Config_DelegatePointerWrapper(&Config_ColumnReleaseDate_Set) },
        { "ColumnLastPlayed", new Config_DelegatePointerWrapper(&Config_ColumnLastPlayed_Set) },
        { "ColumnDateAdded", new Config_DelegatePointerWrapper(&Config_ColumnDateAdded_Set) },
        { "ColumnDisabledMods", new Config_DelegatePointerWrapper(&Config_ColumnDisabledMods_Set) },
        { "ColumnComment", new Config_DelegatePointerWrapper(&Config_ColumnComment_Set) },

        #endregion

        { "MainWindowState", new Config_DelegatePointerWrapper(&Config_MainWindowState_Set) },
        { "MainWindowSize", new Config_DelegatePointerWrapper(&Config_MainWindowSize_Set) },
        { "MainWindowLocation", new Config_DelegatePointerWrapper(&Config_MainWindowLocation_Set) },

        { "MainSplitterPercent", new Config_DelegatePointerWrapper(&Config_MainSplitterPercent_Set) },
        { "TopSplitterPercent", new Config_DelegatePointerWrapper(&Config_TopSplitterPercent_Set) },
        { "TopRightPanelCollapsed", new Config_DelegatePointerWrapper(&Config_TopRightPanelCollapsed_Set) },
        { "GameTab", new Config_DelegatePointerWrapper(&Config_GameTab_Set) },

        #region Top-right tabs

        { "TopRightTab", new Config_DelegatePointerWrapper(&Config_TopRightTab_Set) },

        { "StatsTabPosition", new Config_DelegatePointerWrapper(&Config_StatsTabPosition_Set) },
        { "StatsTabVisible", new Config_DelegatePointerWrapper(&Config_StatsTabVisible_Set) },

        { "EditFMTabPosition", new Config_DelegatePointerWrapper(&Config_EditFMTabPosition_Set) },
        { "EditFMTabVisible", new Config_DelegatePointerWrapper(&Config_EditFMTabVisible_Set) },

        { "CommentTabPosition", new Config_DelegatePointerWrapper(&Config_CommentTabPosition_Set) },
        { "CommentTabVisible", new Config_DelegatePointerWrapper(&Config_CommentTabVisible_Set) },

        { "TagsTabPosition", new Config_DelegatePointerWrapper(&Config_TagsTabPosition_Set) },
        { "TagsTabVisible", new Config_DelegatePointerWrapper(&Config_TagsTabVisible_Set) },

        { "PatchTabPosition", new Config_DelegatePointerWrapper(&Config_PatchTabPosition_Set) },
        { "PatchTabVisible", new Config_DelegatePointerWrapper(&Config_PatchTabVisible_Set) },

        { "ModsTabPosition", new Config_DelegatePointerWrapper(&Config_ModsTabPosition_Set) },
        { "ModsTabVisible", new Config_DelegatePointerWrapper(&Config_ModsTabVisible_Set) },

        #endregion

        { "ReadmeZoomFactor", new Config_DelegatePointerWrapper(&Config_ReadmeZoomFactor_Set) },
        { "ReadmeUseFixedWidthFont", new Config_DelegatePointerWrapper(&Config_ReadmeUseFixedWidthFont_Set) },
        { "EnableCharacterDetailFix", new Config_DelegatePointerWrapper(&Config_EnableCharacterDetailFix_Set) },

        { "PlayOriginalSeparateButtons", new Config_DelegatePointerWrapper(&Config_PlayOriginalSeparateButtons_Set) },

        { "AskedToScanForMisCounts", new Config_DelegatePointerWrapper(&Config_AskedToScanForMisCounts_Set) },

        { "EnableFuzzySearch", new Config_DelegatePointerWrapper(&Config_EnableFuzzySearch_Set) },

        #region Backward compatibility

        /*
        I put the game type as the suffix rather than the prefix on these for some reason, and then put
        them in the next public release. So now I have to support reading them suffixed. But let's just
        write them out prefixed from now on to nip this inconsistency in the bud. That way we don't have
        to add suffix support to our prefix detector and slow it down with a weird edge case.
        Note that even if we add more games, we don't have to - and shouldn't - add them here.
        Because the old app versions that need these won't support the new games anyway, and the new app
        versions that support the new games will also have the new prefixed format in the config.
        Or else they'll be reading from an old config that won't have data for the new game(s) anyway.
        */

        { "GameFilterVisibleT1", new Config_DelegatePointerWrapper(&Config_GameFilterVisibleT1_Set) },
        { "GameFilterVisibleT2", new Config_DelegatePointerWrapper(&Config_GameFilterVisibleT2_Set) },
        { "GameFilterVisibleT3", new Config_DelegatePointerWrapper(&Config_GameFilterVisibleT3_Set) },
        { "GameFilterVisibleSS2", new Config_DelegatePointerWrapper(&Config_GameFilterVisibleSS2_Set) }

        #endregion
    };

    // Read only the theme, hopefully from the top, for perf
    internal static VisualTheme ReadThemeFromConfigIni(string path)
    {
        try
        {
            using var sr = new StreamReader(path);
            while (sr.ReadLine() is { } line)
            {
                string lineT = line.Trim();
                if (lineT.StartsWithFast("VisualTheme="))
                {
                    return lineT.ValueEqualsIAscii("Dark", 12) ? VisualTheme.Dark : VisualTheme.Classic;
                }
            }

            return VisualTheme.Classic;
        }
        catch
        {
            return VisualTheme.Classic;
        }
    }

    internal static unsafe void ReadConfigIni(string path, ConfigData config)
    {
        List<string> iniLines = File_ReadAllLines_List(path);

        // We only read config once on startup, so GC the dictionary after we're done with it
        var configDict = CreateConfigDictionary();

        for (int li = 0; li < iniLines.Count; li++)
        {
            ReadOnlySpan<char> lineTS = iniLines[li].AsSpan().TrimStart();

            if (lineTS.Length == 0 || lineTS[0] == ';') continue;

            int eqIndex = lineTS.IndexOf('=');
            if (eqIndex > -1)
            {
                ReadOnlySpan<char> valRaw = lineTS[(eqIndex + 1)..];
                ReadOnlySpan<char> valTrimmed = valRaw.Trim();

                // @GENGAMES (ConfigIni prefix detector) - Begin

                // _Extremely_ stupid, but prevents having to run a starts-with check n times per line,
                // which is the kind of thing we were trying to get rid of in the first place.
                // Char checks so as to avoid the yet another allocation of substring-ing the prefix itself
                // and checking it that way.

                GameIndex gameIndex = GameIndex.Thief1;
                bool ignoreGameIndex = true;

                if (eqIndex >= 2)
                {
                    if (lineTS[0] == 'T')
                    {
                        switch (lineTS[1])
                        {
                            case '1':
                                gameIndex = GameIndex.Thief1;
                                ignoreGameIndex = false;
                                lineTS = lineTS[2..];
                                break;
                            case '2':
                                gameIndex = GameIndex.Thief2;
                                ignoreGameIndex = false;
                                lineTS = lineTS[2..];
                                break;
                            case '3':
                                gameIndex = GameIndex.Thief3;
                                ignoreGameIndex = false;
                                lineTS = lineTS[2..];
                                break;
                            case 'D':
                                if (eqIndex >= 3 && lineTS[2] == 'M')
                                {
                                    gameIndex = GameIndex.TDM;
                                    ignoreGameIndex = false;
                                    lineTS = lineTS[3..];
                                }
                                break;
                        }
                    }
                    else if (eqIndex >= 3 && lineTS[0] == 'S' && lineTS[1] == 'S' && lineTS[2] == '2')
                    {
                        gameIndex = GameIndex.SS2;
                        ignoreGameIndex = false;
                        lineTS = lineTS[3..];
                    }
                }

                // @GENGAMES (ConfigIni prefix detector) - End

                // @NET5: Fix this allocation later
                if (configDict.TryGetValue(lineTS.ToString(), out var result))
                {
                    result.Action(config, valTrimmed, valRaw, gameIndex, ignoreGameIndex);
                }
            }
        }

        // Vital, don't remove!
        FinalizeConfig(config);
    }

    private static void WriteConfigIniInternal(ConfigData config, string fileName)
    {
        // 2020-06-03: My config file is ~4000 bytes (OneList, Thief2 filter only). 6000 gives reasonable
        // headroom for avoiding reallocations.
        var sb = new StringBuilder(6000);

        // @NET5: Write current config version header (keep it off for testing old-to-new)
#if false
        sb.Append(ConfigVersionHeader).AppendLine(AppConfigVersion.ToString());
#endif

        // Put this one first so it can be read quickly so we can get the theme quickly so we can show the
        // themed splash screen quickly
        sb.Append("VisualTheme").Append('=').Append(config.VisualTheme).AppendLine();

        #region Settings window

        #region Settings window state

        sb.Append("SettingsTab").Append('=').Append(config.SettingsTab).AppendLine();
        sb.Append("SettingsWindowSize").Append('=').Append(config.SettingsWindowSize.Width).Append(',').Append(config.SettingsWindowSize.Height).AppendLine();
        sb.Append("SettingsWindowSplitterDistance").Append('=').Append(config.SettingsWindowSplitterDistance).AppendLine();

        sb.Append("SettingsPathsVScrollPos").Append('=').Append(config.GetSettingsTabVScrollPos(SettingsTab.Paths)).AppendLine();
        sb.Append("SettingsAppearanceVScrollPos").Append('=').Append(config.GetSettingsTabVScrollPos(SettingsTab.Appearance)).AppendLine();
        sb.Append("SettingsOtherVScrollPos").Append('=').Append(config.GetSettingsTabVScrollPos(SettingsTab.Other)).AppendLine();
        sb.Append("SettingsThiefBuddyVScrollPos").Append('=').Append(config.GetSettingsTabVScrollPos(SettingsTab.ThiefBuddy)).AppendLine();

        #endregion

        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            if (GameIsDark(gameIndex))
            {
                string val = config.GetNewMantling(gameIndex) switch
                {
                    true => bool.TrueString,
                    false => bool.FalseString,
                    _ => ""
                };
                sb.Append(GetGamePrefix(gameIndex)).Append("NewMantling").Append('=').AppendLine(val);
            }
        }

        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            if (GameSupportsMods(gameIndex))
            {
                sb.Append(GetGamePrefix(gameIndex)).Append("DisabledMods").Append('=').AppendLine(config.GetDisabledMods(gameIndex).Trim());
            }
        }

        #region Paths

        #region Game exes

        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            sb.Append(GetGamePrefix(gameIndex)).Append("Exe").Append('=').AppendLine(config.GetGameExe(gameIndex).Trim());
        }

        #endregion

        #region Steam

        sb.Append("LaunchGamesWithSteam").Append('=').Append(config.LaunchGamesWithSteam).AppendLine();

        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            if (GetGameSteamId(gameIndex).IsEmpty()) continue;
            sb.Append(GetGamePrefix(gameIndex)).Append("UseSteam").Append('=').Append(config.GetUseSteamSwitch(gameIndex)).AppendLine();
        }

        sb.Append("SteamExe").Append('=').AppendLine(config.SteamExe);

        #endregion

        sb.Append("FMsBackupPath").Append('=').AppendLine(config.FMsBackupPath.Trim());
        foreach (string path in config.FMArchivePaths)
        {
            sb.Append("FMArchivePath").Append('=').AppendLine(path.Trim());
        }
        sb.Append("FMArchivePathsIncludeSubfolders").Append('=').Append(config.FMArchivePathsIncludeSubfolders).AppendLine();

        #endregion

        sb.Append("GameOrganization").Append('=').Append(config.GameOrganization).AppendLine();
        sb.Append("UseShortGameTabNames").Append('=').Append(config.UseShortGameTabNames).AppendLine();

        sb.Append("EnableArticles").Append('=').Append(config.EnableArticles).AppendLine();
        sb.Append("Articles").Append('=').AppendLine(string.Join(",", config.Articles));
        sb.Append("MoveArticlesToEnd").Append('=').Append(config.MoveArticlesToEnd).AppendLine();

        sb.Append("RatingDisplayStyle").Append('=').Append(config.RatingDisplayStyle).AppendLine();
        sb.Append("RatingUseStars").Append('=').Append(config.RatingUseStars).AppendLine();

        sb.Append("DateFormat").Append('=').Append(config.DateFormat).AppendLine();
        sb.Append("DateCustomFormat1").Append('=').AppendLine(config.DateCustomFormat1);
        sb.Append("DateCustomSeparator1").Append('=').AppendLine(config.DateCustomSeparator1);
        sb.Append("DateCustomFormat2").Append('=').AppendLine(config.DateCustomFormat2);
        sb.Append("DateCustomSeparator2").Append('=').AppendLine(config.DateCustomSeparator2);
        sb.Append("DateCustomFormat3").Append('=').AppendLine(config.DateCustomFormat3);
        sb.Append("DateCustomSeparator3").Append('=').AppendLine(config.DateCustomSeparator3);
        sb.Append("DateCustomFormat4").Append('=').AppendLine(config.DateCustomFormat4);

        sb.Append("DaysRecent").Append('=').Append(config.DaysRecent).AppendLine();

        sb.Append("ConvertWAVsTo16BitOnInstall").Append('=').Append(config.ConvertWAVsTo16BitOnInstall).AppendLine();
        sb.Append("ConvertOGGsToWAVsOnInstall").Append('=').Append(config.ConvertOGGsToWAVsOnInstall).AppendLine();
        sb.Append("UseOldMantlingForOldDarkFMs").Append('=').Append(config.UseOldMantlingForOldDarkFMs).AppendLine();
        sb.Append("HideUninstallButton").Append('=').Append(config.HideUninstallButton).AppendLine();
        sb.Append("HideFMListZoomButtons").Append('=').Append(config.HideFMListZoomButtons).AppendLine();
        sb.Append("HideExitButton").Append('=').Append(config.HideExitButton).AppendLine();
        sb.Append("HideWebSearchButton").Append('=').Append(config.HideWebSearchButton).AppendLine();
        sb.Append("ConfirmBeforeInstall").Append('=').Append(config.ConfirmBeforeInstall).AppendLine();
        sb.Append("ConfirmUninstall").Append('=').Append(config.ConfirmUninstall).AppendLine();
        sb.Append("BackupFMData").Append('=').Append(config.BackupFMData).AppendLine();
        sb.Append("BackupAlwaysAsk").Append('=').Append(config.BackupAlwaysAsk).AppendLine();
        sb.Append("Language").Append('=').AppendLine(config.Language);
        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            sb.Append(GetGamePrefix(gameIndex)).Append("WebSearchUrl").Append('=').Append(config.GetWebSearchUrl(gameIndex)).AppendLine();
        }
        sb.Append("ConfirmPlayOnDCOrEnter").Append('=').Append(config.ConfirmPlayOnDCOrEnter).AppendLine();

        sb.Append("RunThiefBuddyOnFMPlay").Append('=').Append(config.RunThiefBuddyOnFMPlay).AppendLine();

        #endregion

        #region Filters

        for (int i = 0; i < SupportedGameCount; i++)
        {
            sb.Append(GetGamePrefix((GameIndex)i)).Append("GameFilterVisible").Append('=').AppendLine(config.GameFilterControlVisibilities[i].ToString());
        }

        for (int i = 0; i < HideableFilterControlsCount; i++)
        {
            sb.Append("FilterVisible").Append((HideableFilterControls)i).Append('=').AppendLine(config.FilterControlVisibilities[i].ToString());
        }

        StringBuilder tagsToStringSB = new(FMTags.TagsToStringSBInitialCapacity);

        for (int i = 0; i < SupportedGameCount + 1; i++)
        {
            Filter filter = i == 0 ? config.Filter : config.GameTabsState.GetFilter((GameIndex)(i - 1));
            string p = i == 0 ? "" : GetGamePrefix((GameIndex)(i - 1));

            if (i == 0)
            {
                sb.Append("FilterGames").Append('=');
                CommaCombineGameFlags(sb, config.Filter.Games);
            }

            sb.Append(p).Append("FilterTitle").Append('=').AppendLine(filter.Title);
            sb.Append(p).Append("FilterAuthor").Append('=').AppendLine(filter.Author);

            sb.Append(p).Append("FilterReleaseDateFrom").Append('=').AppendLine(FilterDate(filter.ReleaseDateFrom));
            sb.Append(p).Append("FilterReleaseDateTo").Append('=').AppendLine(FilterDate(filter.ReleaseDateTo));

            sb.Append(p).Append("FilterLastPlayedFrom").Append('=').AppendLine(FilterDate(filter.LastPlayedFrom));
            sb.Append(p).Append("FilterLastPlayedTo").Append('=').AppendLine(FilterDate(filter.LastPlayedTo));

            sb.Append(p).Append("FilterFinishedStates").Append('=');
            CommaCombineFinishedStates(sb, filter.Finished);

            sb.Append(p).Append("FilterRatingFrom").Append('=').Append(filter.RatingFrom).AppendLine();
            sb.Append(p).Append("FilterRatingTo").Append('=').Append(filter.RatingTo).AppendLine();

            sb.Append(p).Append("FilterTagsAnd").Append('=').AppendLine(TagsToString(filter.Tags.AndTags, tagsToStringSB));
            sb.Append(p).Append("FilterTagsOr").Append('=').AppendLine(TagsToString(filter.Tags.OrTags, tagsToStringSB));
            sb.Append(p).Append("FilterTagsNot").Append('=').AppendLine(TagsToString(filter.Tags.NotTags, tagsToStringSB));
        }

        #endregion

        #region Columns

        sb.Append("SortedColumn").Append('=').Append(config.SortedColumn).AppendLine();
        sb.Append("SortDirection").Append('=').Append(config.SortDirection).AppendLine();
        sb.Append("ShowRecentAtTop").Append('=').Append(config.ShowRecentAtTop).AppendLine();
        sb.Append("ShowUnsupported").Append('=').Append(config.ShowUnsupported).AppendLine();
        sb.Append("ShowUnavailableFMs").Append('=').Append(config.ShowUnavailableFMs).AppendLine();
        sb.Append("FMsListFontSizeInPoints").Append('=').AppendLine(config.FMsListFontSizeInPoints.ToString(NumberFormatInfo.InvariantInfo));

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

            sb.Append(p).Append("SelFMInstDir").Append('=').AppendLine(selFM.InstalledName);
            sb.Append(p).Append("SelFMIndexFromTop").Append('=').Append(selFM.IndexFromTop).AppendLine();
        }

        #endregion

        #region Main window state

        sb.Append("MainWindowState").Append('=').Append(config.MainWindowState).AppendLine();

        sb.Append("MainWindowSize").Append('=').Append(config.MainWindowSize.Width).Append(',').Append(config.MainWindowSize.Height).AppendLine();
        sb.Append("MainWindowLocation").Append('=').Append(config.MainWindowLocation.X).Append(',').Append(config.MainWindowLocation.Y).AppendLine();

        sb.Append("MainSplitterPercent").Append('=').AppendLine(config.MainSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
        sb.Append("TopSplitterPercent").Append('=').AppendLine(config.TopSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
        sb.Append("TopRightPanelCollapsed").Append('=').Append(config.TopRightPanelCollapsed).AppendLine();

        sb.Append("GameTab").Append('=').Append(config.GameTab).AppendLine();
        sb.Append("TopRightTab").Append('=').Append(config.TopRightTabsData.SelectedTab).AppendLine();

        sb.Append("StatsTabPosition").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.Statistics).DisplayIndex).AppendLine();
        sb.Append("EditFMTabPosition").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.EditFM).DisplayIndex).AppendLine();
        sb.Append("CommentTabPosition").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.Comment).DisplayIndex).AppendLine();
        sb.Append("TagsTabPosition").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.Tags).DisplayIndex).AppendLine();
        sb.Append("PatchTabPosition").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.Patch).DisplayIndex).AppendLine();
        sb.Append("ModsTabPosition").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.Mods).DisplayIndex).AppendLine();

        sb.Append("StatsTabVisible").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.Statistics).Visible).AppendLine();
        sb.Append("EditFMTabVisible").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.EditFM).Visible).AppendLine();
        sb.Append("CommentTabVisible").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.Comment).Visible).AppendLine();
        sb.Append("TagsTabVisible").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.Tags).Visible).AppendLine();
        sb.Append("PatchTabVisible").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.Patch).Visible).AppendLine();
        sb.Append("ModsTabVisible").Append('=').Append(config.TopRightTabsData.GetTab(TopRightTab.Mods).Visible).AppendLine();

        sb.Append("ReadmeZoomFactor").Append('=').AppendLine(config.ReadmeZoomFactor.ToString(NumberFormatInfo.InvariantInfo));
        sb.Append("ReadmeUseFixedWidthFont").Append('=').Append(config.ReadmeUseFixedWidthFont).AppendLine();

        #endregion

        sb.Append("EnableCharacterDetailFix").Append('=').Append(config.EnableCharacterDetailFix).AppendLine();
        sb.Append("PlayOriginalSeparateButtons").Append('=').Append(config.PlayOriginalSeparateButtons).AppendLine();

        sb.Append("AskedToScanForMisCounts").Append('=').Append(config.AskedToScanForMisCounts).AppendLine();

        sb.Append("EnableFuzzySearch").Append('=').Append(config.EnableFuzzySearch).AppendLine();

        using var sw = new StreamWriter(fileName, false, Encoding.UTF8);
        sw.Write(sb.ToString());
    }
}
