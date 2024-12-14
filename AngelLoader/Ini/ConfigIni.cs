#define FenGen_ConfigDest

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using AngelLoader.DataClasses;
using SpanExtensions;
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
            config.SettingsTab = (SettingsTab)field.GetValue(null)!;
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

    private static void Config_SettingsUpdateVScrollPos_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            config.SetSettingsTabVScrollPos(SettingsTab.Update, result);
        }
    }

    private static void Config_SettingsIOThreadingVScrollPos_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            config.SetSettingsTabVScrollPos(SettingsTab.IOThreading, result);
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
            config.RunThiefBuddyOnFMPlay = (RunThiefBuddyOnFMPlay)field.GetValue(null)!;
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
            config.GameOrganization = (GameOrganization)field.GetValue(null)!;
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
        string[] articles = valTrimmed.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        config.Articles.ClearAndAdd(articles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
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
            config.RatingDisplayStyle = (RatingDisplayStyle)field.GetValue(null)!;
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
            config.DateFormat = (DateFormat)field.GetValue(null)!;
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
            config.ConfirmBeforeInstall = (ConfirmBeforeInstall)field.GetValue(null)!;
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
            config.BackupFMData = (BackupFMData)field.GetValue(null)!;
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
        if (valTrimmed is "FollowSystemTheme")
        {
            config.FollowSystemTheme = true;
            config.VisualTheme = Core.GetSystemTheme();
        }
        else
        {
            config.FollowSystemTheme = false;
            FieldInfo? field = typeof(VisualTheme).GetField(valTrimmed.ToString(), _bFlagsEnum);
            if (field != null)
            {
                config.VisualTheme = (VisualTheme)field.GetValue(null)!;
            }
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
        string[] iniGames = valTrimmed.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (string iniGame in iniGames)
        {
            FieldInfo? field = typeof(Game).GetField(iniGame.Trim(), _bFlagsEnum);
            if (field != null)
            {
                config.Filter.Games |= (Game)field.GetValue(null)!;
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
            config.SortedColumn = (Column)field.GetValue(null)!;
        }
    }
    private static void Config_SortDirection_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(SortDirection).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.SortDirection = (SortDirection)field.GetValue(null)!;
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
    private static void Config_ColumnPlayTime_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        AddColumn(config, valTrimmed, Column.PlayTime);
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
            config.MainWindowState = (WindowState)field.GetValue(null)!;
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

    private static void Config_BottomSplitterPercent_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Float_TryParseInv(valTrimmed, out float result))
        {
            config.BottomSplitterPercent = result;
        }
    }

    private static void Config_TopFMTabsPanelCollapsed_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.TopFMTabsPanelCollapsed = valTrimmed.EqualsTrue();
    }

    private static void Config_BottomFMTabsPanelCollapsed_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        config.BottomFMTabsPanelCollapsed = valTrimmed.EqualsTrue();
    }

    private static void Config_GameTab_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(GameIndex).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.GameTab = (GameIndex)field.GetValue(null)!;
        }
    }

    #region FM tabs

    private static void Config_FMTab_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(FMTab).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.FMTabsData.SelectedTab = (FMTab)field.GetValue(null)!;
        }
    }

    private static void Config_FMTab2_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(FMTab).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.FMTabsData.SelectedTab2 = (FMTab)field.GetValue(null)!;
        }
    }

    private static void SetFMTabVisibility(ConfigData config, FMTab fmTab, ReadOnlySpan<char> valTrimmed)
    {
        // Backward compatibility - match old behavior exactly
        if (valTrimmed.EqualsTrue())
        {
            config.FMTabsData.GetTab(fmTab).Visible = FMTabVisibleIn.Top;
        }
        else if (valTrimmed.EqualsFalse())
        {
            config.FMTabsData.GetTab(fmTab).Visible = FMTabVisibleIn.None;
        }
        else
        {
            FieldInfo? field = typeof(FMTabVisibleIn).GetField(valTrimmed.ToString(), _bFlagsEnum);
            config.FMTabsData.GetTab(fmTab).Visible = field != null
                ? (FMTabVisibleIn)field.GetValue(null)!
                : FMTabVisibleIn.None;
        }
    }

    private static void Config_StatsTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.FMTabsData.GetTab(FMTab.Statistics).DisplayIndex = result;
    }
    private static void Config_StatsTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        SetFMTabVisibility(config, FMTab.Statistics, valTrimmed);
    }

    private static void Config_EditFMTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.FMTabsData.GetTab(FMTab.EditFM).DisplayIndex = result;
    }
    private static void Config_EditFMTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        SetFMTabVisibility(config, FMTab.EditFM, valTrimmed);
    }

    private static void Config_CommentTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.FMTabsData.GetTab(FMTab.Comment).DisplayIndex = result;
    }
    private static void Config_CommentTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        SetFMTabVisibility(config, FMTab.Comment, valTrimmed);
    }

    private static void Config_TagsTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.FMTabsData.GetTab(FMTab.Tags).DisplayIndex = result;
    }
    private static void Config_TagsTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        SetFMTabVisibility(config, FMTab.Tags, valTrimmed);
    }

    private static void Config_PatchTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.FMTabsData.GetTab(FMTab.Patch).DisplayIndex = result;
    }
    private static void Config_PatchTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        SetFMTabVisibility(config, FMTab.Patch, valTrimmed);
    }

    private static void Config_ModsTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.FMTabsData.GetTab(FMTab.Mods).DisplayIndex = result;
    }
    private static void Config_ModsTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        SetFMTabVisibility(config, FMTab.Mods, valTrimmed);
    }

    private static void Config_ScreenshotsTabPosition_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        Int_TryParseInv(valTrimmed, out int result);
        config.FMTabsData.GetTab(FMTab.Screenshots).DisplayIndex = result;
    }
    private static void Config_ScreenshotsTabVisible_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        SetFMTabVisibility(config, FMTab.Screenshots, valTrimmed);
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

    private static void Config_CheckForUpdates_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(CheckForUpdates).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.CheckForUpdates = (CheckForUpdates)field.GetValue(null)!;
        }
    }

    private static void Config_ScreenshotGammaPercent_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            config.ScreenshotGammaPercent = result;
        }
    }

    private static void Config_IOThreadsMode_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        FieldInfo? field = typeof(IOThreadsMode).GetField(valTrimmed.ToString(), _bFlagsEnum);
        if (field != null)
        {
            config.IOThreadsMode = (IOThreadsMode)field.GetValue(null)!;
        }
    }

    private static void Config_CustomIOThreadCount_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        if (Int_TryParseInv(valTrimmed, out int result))
        {
            config.CustomIOThreadCount = result;
        }
    }

    private static void Config_DriveMultithreadingLevel_Set(ConfigData config, ReadOnlySpan<char> valTrimmed, ReadOnlySpan<char> valRaw, GameIndex gameIndex, bool ignoreGameIndex)
    {
        foreach (ReadOnlySpan<char> value in ReadOnlySpanExtensions.Split(valTrimmed, ',', StringSplitOptions.RemoveEmptyEntries))
        {
            char? letterChar = null;
            int i = 0;
            foreach (ReadOnlySpan<char> item in ReadOnlySpanExtensions.Split(value, ':', StringSplitOptions.RemoveEmptyEntries))
            {
                if (i == 0)
                {
                    ReadOnlySpan<char> letter = item.Trim();
                    if (letter.Length > 0)
                    {
                        letterChar = letter[0];
                    }
                }
                else if (i == 1)
                {
                    string threadabilityStr = item.Trim().ToString();
                    FieldInfo? field = typeof(DriveMultithreadingLevel).GetField(threadabilityStr, _bFlagsEnum);
                    if (field != null && letterChar is { } letterCharValid)
                    {
                        config.DriveLettersAndTypes[letterCharValid] = (DriveMultithreadingLevel)field.GetValue(null)!;
                    }
                    break;
                }
                i++;
            }
        }
    }

    #endregion

    [StructLayout(LayoutKind.Auto)]
    private readonly unsafe struct Config_DelegatePointerWrapper
    {
        internal readonly delegate*<ConfigData, ReadOnlySpan<char>, ReadOnlySpan<char>, GameIndex, bool, void> Action;

        internal Config_DelegatePointerWrapper(delegate*<ConfigData, ReadOnlySpan<char>, ReadOnlySpan<char>, GameIndex, bool, void> action)
        {
            Action = action;
        }
    }

    private static unsafe Dictionary<ReadOnlyMemory<char>, Config_DelegatePointerWrapper>
    CreateConfigDictionary() => new(new MemoryStringComparer())
    {
        #region Settings window state

        { "SettingsTab".AsMemory(), new Config_DelegatePointerWrapper(&Config_SettingsTab_Set) },
        { "SettingsWindowSize".AsMemory(), new Config_DelegatePointerWrapper(&Config_SettingsWindowSize_Set) },
        { "SettingsWindowSplitterDistance".AsMemory(), new Config_DelegatePointerWrapper(&Config_SettingsWindowSplitterDistance_Set) },
        { "SettingsPathsVScrollPos".AsMemory(), new Config_DelegatePointerWrapper(&Config_SettingsPathsVScrollPos_Set) },
        { "SettingsAppearanceVScrollPos".AsMemory(), new Config_DelegatePointerWrapper(&Config_SettingsAppearanceVScrollPos_Set) },
        { "SettingsOtherVScrollPos".AsMemory(), new Config_DelegatePointerWrapper(&Config_SettingsOtherVScrollPos_Set) },
        { "SettingsThiefBuddyVScrollPos".AsMemory(), new Config_DelegatePointerWrapper(&Config_SettingsThiefBuddyVScrollPos_Set) },
        { "SettingsUpdateVScrollPos".AsMemory(), new Config_DelegatePointerWrapper(&Config_SettingsUpdateVScrollPos_Set) },
        { "SettingsIOThreadingVScrollPos".AsMemory(), new Config_DelegatePointerWrapper(&Config_SettingsIOThreadingVScrollPos_Set) },

        #endregion

        { "LaunchGamesWithSteam".AsMemory(), new Config_DelegatePointerWrapper(&Config_LaunchGamesWithSteam_Set) },
        { "SteamExe".AsMemory(), new Config_DelegatePointerWrapper(&Config_SteamExe_Set) },
        { "RunThiefBuddyOnFMPlay".AsMemory(), new Config_DelegatePointerWrapper(&Config_RunThiefBuddyOnFMPlay_Set) },
        { "FMsBackupPath".AsMemory(), new Config_DelegatePointerWrapper(&Config_FMsBackupPath_Set) },
        { "FMArchivePath".AsMemory(), new Config_DelegatePointerWrapper(&Config_FMArchivePath_Set) },
        { "FMArchivePathsIncludeSubfolders".AsMemory(), new Config_DelegatePointerWrapper(&Config_FMArchivePathsIncludeSubfolders_Set) },
        { "GameOrganization".AsMemory(), new Config_DelegatePointerWrapper(&Config_GameOrganization_Set) },
        { "UseShortGameTabNames".AsMemory(), new Config_DelegatePointerWrapper(&Config_UseShortGameTabNames_Set) },
        { "EnableArticles".AsMemory(), new Config_DelegatePointerWrapper(&Config_EnableArticles_Set) },
        { "Articles".AsMemory(), new Config_DelegatePointerWrapper(&Config_Articles_Set) },
        { "MoveArticlesToEnd".AsMemory(), new Config_DelegatePointerWrapper(&Config_MoveArticlesToEnd_Set) },
        { "RatingDisplayStyle".AsMemory(), new Config_DelegatePointerWrapper(&Config_RatingDisplayStyle_Set) },
        { "RatingUseStars".AsMemory(), new Config_DelegatePointerWrapper(&Config_RatingUseStars_Set) },

        #region Date format

        { "DateFormat".AsMemory(), new Config_DelegatePointerWrapper(&Config_DateFormat_Set) },
        { "DateCustomFormat1".AsMemory(), new Config_DelegatePointerWrapper(&Config_DateCustomFormat1_Set) },
        { "DateCustomSeparator1".AsMemory(), new Config_DelegatePointerWrapper(&Config_DateCustomSeparator1_Set) },
        { "DateCustomFormat2".AsMemory(), new Config_DelegatePointerWrapper(&Config_DateCustomFormat2_Set) },
        { "DateCustomSeparator2".AsMemory(), new Config_DelegatePointerWrapper(&Config_DateCustomSeparator2_Set) },
        { "DateCustomFormat3".AsMemory(), new Config_DelegatePointerWrapper(&Config_DateCustomFormat3_Set) },
        { "DateCustomSeparator3".AsMemory(), new Config_DelegatePointerWrapper(&Config_DateCustomSeparator3_Set) },
        { "DateCustomFormat4".AsMemory(), new Config_DelegatePointerWrapper(&Config_DateCustomFormat4_Set) },

        #endregion

        { "DaysRecent".AsMemory(), new Config_DelegatePointerWrapper(&Config_DaysRecent_Set) },
        { "ConvertWAVsTo16BitOnInstall".AsMemory(), new Config_DelegatePointerWrapper(&Config_ConvertWAVsTo16BitOnInstall_Set) },
        { "ConvertOGGsToWAVsOnInstall".AsMemory(), new Config_DelegatePointerWrapper(&Config_ConvertOGGsToWAVsOnInstall_Set) },
        { "UseOldMantlingForOldDarkFMs".AsMemory(), new Config_DelegatePointerWrapper(&Config_UseOldMantlingForOldDarkFMs_Set) },
        { "HideUninstallButton".AsMemory(), new Config_DelegatePointerWrapper(&Config_HideUninstallButton_Set) },
        { "HideFMListZoomButtons".AsMemory(), new Config_DelegatePointerWrapper(&Config_HideFMListZoomButtons_Set) },
        { "HideExitButton".AsMemory(), new Config_DelegatePointerWrapper(&Config_HideExitButton_Set) },
        { "HideWebSearchButton".AsMemory(), new Config_DelegatePointerWrapper(&Config_HideWebSearchButton_Set) },
        { "ConfirmBeforeInstall".AsMemory(), new Config_DelegatePointerWrapper(&Config_ConfirmInstall_Set) },
        { "ConfirmUninstall".AsMemory(), new Config_DelegatePointerWrapper(&Config_ConfirmUninstall_Set) },
        { "BackupFMData".AsMemory(), new Config_DelegatePointerWrapper(&Config_BackupFMData_Set) },
        { "BackupAlwaysAsk".AsMemory(), new Config_DelegatePointerWrapper(&Config_BackupAlwaysAsk_Set) },
        { "Language".AsMemory(), new Config_DelegatePointerWrapper(&Config_Language_Set) },
        { "WebSearchUrl".AsMemory(), new Config_DelegatePointerWrapper(&Config_WebSearchUrl_Set) },
        { "ConfirmPlayOnDCOrEnter".AsMemory(), new Config_DelegatePointerWrapper(&Config_ConfirmPlayOnDCOrEnter_Set) },
        { "VisualTheme".AsMemory(), new Config_DelegatePointerWrapper(&Config_VisualTheme_Set) },

        #region Filter visibilities

        { "FilterVisibleTitle".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterVisibleTitle_Set) },
        { "FilterVisibleAuthor".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterVisibleAuthor_Set) },
        { "FilterVisibleReleaseDate".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterVisibleReleaseDate_Set) },
        { "FilterVisibleLastPlayed".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterVisibleLastPlayed_Set) },
        { "FilterVisibleTags".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterVisibleTags_Set) },
        { "FilterVisibleFinishedState".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterVisibleFinishedState_Set) },
        { "FilterVisibleRating".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterVisibleRating_Set) },
        { "FilterVisibleShowUnsupported".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterVisibleShowUnsupported_Set) },
        { "FilterVisibleShowUnavailable".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterVisibleShowUnavailable_Set) },
        { "FilterVisibleShowRecentAtTop".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterVisibleShowRecentAtTop_Set) },

        #endregion

        #region Filter values

        { "FilterGames".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterGames_Set) },
        { "FilterTitle".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterTitle_Set) },
        { "FilterAuthor".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterAuthor_Set) },
        { "FilterReleaseDateFrom".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterReleaseDateFrom_Set) },
        { "FilterReleaseDateTo".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterReleaseDateTo_Set) },
        { "FilterLastPlayedFrom".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterLastPlayedFrom_Set) },
        { "FilterLastPlayedTo".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterLastPlayedTo_Set) },
        { "FilterFinishedStates".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterFinishedStates_Set) },
        { "FilterRatingFrom".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterRatingFrom_Set) },
        { "FilterRatingTo".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterRatingTo_Set) },
        { "FilterTagsAnd".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterTagsAnd_Set) },
        { "FilterTagsOr".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterTagsOr_Set) },
        { "FilterTagsNot".AsMemory(), new Config_DelegatePointerWrapper(&Config_FilterTagsNot_Set) },

        #endregion

        #region Per-game fields

        { "NewMantling".AsMemory(), new Config_DelegatePointerWrapper(&Config_NewMantling_Set) },

        { "DisabledMods".AsMemory(), new Config_DelegatePointerWrapper(&Config_DisabledMods_Set) },

        { "Exe".AsMemory(), new Config_DelegatePointerWrapper(&Config_Exe_Set) },

        { "UseSteam".AsMemory(), new Config_DelegatePointerWrapper(&Config_UseSteam_Set) },

        { "GameFilterVisible".AsMemory(), new Config_DelegatePointerWrapper(&Config_GameFilterVisible_Set) },

        { "SelFMInstDir".AsMemory(), new Config_DelegatePointerWrapper(&Config_SelFMInstDir_Set) },
        { "SelFMIndexFromTop".AsMemory(), new Config_DelegatePointerWrapper(&Config_SelFMIndexFromTop_Set) },

        #endregion

        { "ShowRecentAtTop".AsMemory(), new Config_DelegatePointerWrapper(&Config_ShowRecentAtTop_Set) },
        { "ShowUnsupported".AsMemory(), new Config_DelegatePointerWrapper(&Config_ShowUnsupported_Set) },
        { "ShowUnavailableFMs".AsMemory(), new Config_DelegatePointerWrapper(&Config_ShowUnavailableFMs_Set) },
        { "FMsListFontSizeInPoints".AsMemory(), new Config_DelegatePointerWrapper(&Config_FMsListFontSizeInPoints_Set) },

        { "SortedColumn".AsMemory(), new Config_DelegatePointerWrapper(&Config_SortedColumn_Set) },
        { "SortDirection".AsMemory(), new Config_DelegatePointerWrapper(&Config_SortDirection_Set) },

        #region Columns

#if DateAccTest
        { "ColumnDateAccuracy".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnDateAccuracy_Set) },
#endif
        { "ColumnGame".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnGame_Set) },
        { "ColumnInstalled".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnInstalled_Set) },
        { "ColumnMissionCount".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnMisCount_Set) },
        { "ColumnTitle".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnTitle_Set) },
        { "ColumnArchive".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnArchive_Set) },
        { "ColumnAuthor".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnAuthor_Set) },
        { "ColumnSize".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnSize_Set) },
        { "ColumnRating".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnRating_Set) },
        { "ColumnFinished".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnFinished_Set) },
        { "ColumnReleaseDate".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnReleaseDate_Set) },
        { "ColumnLastPlayed".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnLastPlayed_Set) },
        { "ColumnDateAdded".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnDateAdded_Set) },
        { "ColumnPlayTime".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnPlayTime_Set) },
        { "ColumnDisabledMods".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnDisabledMods_Set) },
        { "ColumnComment".AsMemory(), new Config_DelegatePointerWrapper(&Config_ColumnComment_Set) },

        #endregion

        { "MainWindowState".AsMemory(), new Config_DelegatePointerWrapper(&Config_MainWindowState_Set) },
        { "MainWindowSize".AsMemory(), new Config_DelegatePointerWrapper(&Config_MainWindowSize_Set) },
        { "MainWindowLocation".AsMemory(), new Config_DelegatePointerWrapper(&Config_MainWindowLocation_Set) },

        { "MainSplitterPercent".AsMemory(), new Config_DelegatePointerWrapper(&Config_MainSplitterPercent_Set) },
        { "TopSplitterPercent".AsMemory(), new Config_DelegatePointerWrapper(&Config_TopSplitterPercent_Set) },
        { "BottomSplitterPercent".AsMemory(), new Config_DelegatePointerWrapper(&Config_BottomSplitterPercent_Set) },
        { "TopRightPanelCollapsed".AsMemory(), new Config_DelegatePointerWrapper(&Config_TopFMTabsPanelCollapsed_Set) },
        { "BottomRightPanelCollapsed".AsMemory(), new Config_DelegatePointerWrapper(&Config_BottomFMTabsPanelCollapsed_Set) },
        { "GameTab".AsMemory(), new Config_DelegatePointerWrapper(&Config_GameTab_Set) },

        #region FM tabs

        { "SelectedTab1".AsMemory(), new Config_DelegatePointerWrapper(&Config_FMTab_Set) },
        { "SelectedTab2".AsMemory(), new Config_DelegatePointerWrapper(&Config_FMTab2_Set) },

        { "StatsTabPosition".AsMemory(), new Config_DelegatePointerWrapper(&Config_StatsTabPosition_Set) },
        { "StatsTabVisible".AsMemory(), new Config_DelegatePointerWrapper(&Config_StatsTabVisible_Set) },

        { "EditFMTabPosition".AsMemory(), new Config_DelegatePointerWrapper(&Config_EditFMTabPosition_Set) },
        { "EditFMTabVisible".AsMemory(), new Config_DelegatePointerWrapper(&Config_EditFMTabVisible_Set) },

        { "CommentTabPosition".AsMemory(), new Config_DelegatePointerWrapper(&Config_CommentTabPosition_Set) },
        { "CommentTabVisible".AsMemory(), new Config_DelegatePointerWrapper(&Config_CommentTabVisible_Set) },

        { "TagsTabPosition".AsMemory(), new Config_DelegatePointerWrapper(&Config_TagsTabPosition_Set) },
        { "TagsTabVisible".AsMemory(), new Config_DelegatePointerWrapper(&Config_TagsTabVisible_Set) },

        { "PatchTabPosition".AsMemory(), new Config_DelegatePointerWrapper(&Config_PatchTabPosition_Set) },
        { "PatchTabVisible".AsMemory(), new Config_DelegatePointerWrapper(&Config_PatchTabVisible_Set) },

        { "ModsTabPosition".AsMemory(), new Config_DelegatePointerWrapper(&Config_ModsTabPosition_Set) },
        { "ModsTabVisible".AsMemory(), new Config_DelegatePointerWrapper(&Config_ModsTabVisible_Set) },

        { "ScreenshotsTabPosition".AsMemory(), new Config_DelegatePointerWrapper(&Config_ScreenshotsTabPosition_Set) },
        { "ScreenshotsTabVisible".AsMemory(), new Config_DelegatePointerWrapper(&Config_ScreenshotsTabVisible_Set) },

        #endregion

        { "ReadmeZoomFactor".AsMemory(), new Config_DelegatePointerWrapper(&Config_ReadmeZoomFactor_Set) },
        { "ReadmeUseFixedWidthFont".AsMemory(), new Config_DelegatePointerWrapper(&Config_ReadmeUseFixedWidthFont_Set) },
        { "EnableCharacterDetailFix".AsMemory(), new Config_DelegatePointerWrapper(&Config_EnableCharacterDetailFix_Set) },

        { "PlayOriginalSeparateButtons".AsMemory(), new Config_DelegatePointerWrapper(&Config_PlayOriginalSeparateButtons_Set) },
        { "AskedToScanForMisCounts".AsMemory(), new Config_DelegatePointerWrapper(&Config_AskedToScanForMisCounts_Set) },
        { "EnableFuzzySearch".AsMemory(), new Config_DelegatePointerWrapper(&Config_EnableFuzzySearch_Set) },
        { "CheckForUpdates".AsMemory(), new Config_DelegatePointerWrapper(&Config_CheckForUpdates_Set) },
        { "ScreenshotGammaPercent".AsMemory(), new Config_DelegatePointerWrapper(&Config_ScreenshotGammaPercent_Set) },
        { "IOThreadsMode".AsMemory(), new Config_DelegatePointerWrapper(&Config_IOThreadsMode_Set) },
        { "CustomIOThreadCount".AsMemory(), new Config_DelegatePointerWrapper(&Config_CustomIOThreadCount_Set) },
        { "DriveMultithreadingLevel".AsMemory(), new Config_DelegatePointerWrapper(&Config_DriveMultithreadingLevel_Set) },

        #region Backward compatibility

        { "TopRightTab".AsMemory(), new Config_DelegatePointerWrapper(&Config_FMTab_Set) },

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

        { "GameFilterVisibleT1".AsMemory(), new Config_DelegatePointerWrapper(&Config_GameFilterVisibleT1_Set) },
        { "GameFilterVisibleT2".AsMemory(), new Config_DelegatePointerWrapper(&Config_GameFilterVisibleT2_Set) },
        { "GameFilterVisibleT3".AsMemory(), new Config_DelegatePointerWrapper(&Config_GameFilterVisibleT3_Set) },
        { "GameFilterVisibleSS2".AsMemory(), new Config_DelegatePointerWrapper(&Config_GameFilterVisibleSS2_Set) },

        #endregion
    };

    // Read only the theme, hopefully from the top, for perf
    internal static (VisualTheme Theme, bool FollowSystemTheme)
    ReadThemeFromConfigIni(string path)
    {
        try
        {
            using var sr = new StreamReader(path);
            while (sr.ReadLine() is { } line)
            {
                string lineT = line.Trim();
                if (lineT.StartsWithO("VisualTheme="))
                {
                    if (lineT.ValueEqualsIAscii("FollowSystemTheme", 12))
                    {
                        return (Core.GetSystemTheme(), true);
                    }
                    else if (lineT.ValueEqualsIAscii("Dark", 12))
                    {
                        return (VisualTheme.Dark, false);
                    }
                    else
                    {
                        return (VisualTheme.Classic, false);
                    }
                }
            }

            return (Core.GetSystemTheme(), true);
        }
        catch
        {
            return (Core.GetSystemTheme(), true);
        }
    }

    internal static unsafe void ReadConfigIni(string path, ConfigData config)
    {
        List<string> iniLines = File_ReadAllLines_List(path);

        // We only read config once on startup, so GC the dictionary after we're done with it
        var configDict = CreateConfigDictionary();

        for (int li = 0; li < iniLines.Count; li++)
        {
            ReadOnlyMemory<char> lineTS = iniLines[li].AsMemory().TrimStart();
            var lineTSInitialSpan = lineTS.Span;

            if (lineTSInitialSpan.Length == 0 || lineTSInitialSpan[0] == ';') continue;

            int eqIndex = lineTSInitialSpan.IndexOf('=');
            if (eqIndex > -1)
            {
                ReadOnlySpan<char> valRaw = lineTSInitialSpan[(eqIndex + 1)..];
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
                    if (lineTSInitialSpan[0] == 'T')
                    {
                        switch (lineTSInitialSpan[1])
                        {
                            case '1':
                                gameIndex = GameIndex.Thief1;
                                ignoreGameIndex = false;
                                lineTS = lineTS[2..];
                                eqIndex -= 2;
                                break;
                            case '2':
                                gameIndex = GameIndex.Thief2;
                                ignoreGameIndex = false;
                                lineTS = lineTS[2..];
                                eqIndex -= 2;
                                break;
                            case '3':
                                gameIndex = GameIndex.Thief3;
                                ignoreGameIndex = false;
                                lineTS = lineTS[2..];
                                eqIndex -= 2;
                                break;
                            case 'D':
                                if (eqIndex >= 3 && lineTSInitialSpan[2] == 'M')
                                {
                                    gameIndex = GameIndex.TDM;
                                    ignoreGameIndex = false;
                                    lineTS = lineTS[3..];
                                    eqIndex -= 3;
                                }
                                break;
                        }
                    }
                    else if (eqIndex >= 3 && lineTSInitialSpan[0] == 'S' && lineTSInitialSpan[1] == 'S' && lineTSInitialSpan[2] == '2')
                    {
                        gameIndex = GameIndex.SS2;
                        ignoreGameIndex = false;
                        lineTS = lineTS[3..];
                        eqIndex -= 3;
                    }
                }

                // @GENGAMES (ConfigIni prefix detector) - End

                if (configDict.TryGetValue(lineTS[..eqIndex], out var result))
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
        using StreamWriter sw = new(fileName, false, Encoding.UTF8);

        // @NET5: Write current config version header (keep it off for testing old-to-new)
#if false
        sw.Append(ConfigVersionHeader).AppendLine(AppConfigVersion.ToString());
#endif

        /*
        Doing like 'sw.Append("Value").Append('=').AppendLine(config.Value)' makes us smaller because the "Value"
        string can be deduplicated with the one in the reader dictionary, whereas adding a '=' directly inline
        makes it a separate string, bloating up the file a bit. But, appending a '=' is inefficient, so whatever.
        */

        // Put this one first so it can be read quickly so we can get the theme quickly so we can show the
        // themed splash screen quickly
        sw.Append("VisualTheme=");
        if (config.FollowSystemTheme)
        {
            sw.Append("FollowSystemTheme").AppendLine();
        }
        else
        {
            sw.Append(config.VisualTheme).AppendLine();
        }

        #region Settings window

        #region Settings window state

        sw.Append("SettingsTab=").AppendLine(config.SettingsTab);
        sw.Append("SettingsWindowSize=").Append(config.SettingsWindowSize.Width).Append(',').AppendLine(config.SettingsWindowSize.Height);
        sw.Append("SettingsWindowSplitterDistance=").AppendLine(config.SettingsWindowSplitterDistance);

        sw.Append("SettingsPathsVScrollPos=").AppendLine(config.GetSettingsTabVScrollPos(SettingsTab.Paths));
        sw.Append("SettingsAppearanceVScrollPos=").AppendLine(config.GetSettingsTabVScrollPos(SettingsTab.Appearance));
        sw.Append("SettingsOtherVScrollPos=").AppendLine(config.GetSettingsTabVScrollPos(SettingsTab.Other));
        sw.Append("SettingsThiefBuddyVScrollPos=").AppendLine(config.GetSettingsTabVScrollPos(SettingsTab.ThiefBuddy));
        sw.Append("SettingsUpdateVScrollPos=").AppendLine(config.GetSettingsTabVScrollPos(SettingsTab.Update));
        sw.Append("SettingsIOThreadingVScrollPos=").AppendLine(config.GetSettingsTabVScrollPos(SettingsTab.IOThreading));

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
                    _ => "",
                };
                sw.Append(GetGamePrefix(gameIndex)).Append("NewMantling=").AppendLine(val);
            }
        }

        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            if (GameSupportsMods(gameIndex))
            {
                sw.Append(GetGamePrefix(gameIndex)).Append("DisabledMods=").AppendLine(config.GetDisabledMods(gameIndex).Trim());
            }
        }

        #region Paths

        #region Game exes

        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            sw.Append(GetGamePrefix(gameIndex)).Append("Exe=").AppendLine(config.GetGameExe(gameIndex).Trim());
        }

        #endregion

        #region Steam

        sw.Append("LaunchGamesWithSteam=").AppendLine(config.LaunchGamesWithSteam);

        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            if (GetGameSteamId(gameIndex).IsEmpty()) continue;
            sw.Append(GetGamePrefix(gameIndex)).Append("UseSteam=").AppendLine(config.GetUseSteamSwitch(gameIndex));
        }

        sw.Append("SteamExe=").AppendLine(config.SteamExe);

        #endregion

        sw.Append("FMsBackupPath=").AppendLine(config.FMsBackupPath.Trim());
        foreach (string path in config.FMArchivePaths)
        {
            sw.Append("FMArchivePath=").AppendLine(path.Trim());
        }
        sw.Append("FMArchivePathsIncludeSubfolders=").AppendLine(config.FMArchivePathsIncludeSubfolders);

        #endregion

        sw.Append("GameOrganization=").AppendLine(config.GameOrganization);
        sw.Append("UseShortGameTabNames=").AppendLine(config.UseShortGameTabNames);

        sw.Append("EnableArticles=").AppendLine(config.EnableArticles);
        sw.Append("Articles=").AppendLine(string.Join(",", config.Articles));
        sw.Append("MoveArticlesToEnd=").AppendLine(config.MoveArticlesToEnd);

        sw.Append("RatingDisplayStyle=").AppendLine(config.RatingDisplayStyle);
        sw.Append("RatingUseStars=").AppendLine(config.RatingUseStars);

        sw.Append("DateFormat=").AppendLine(config.DateFormat);
        sw.Append("DateCustomFormat1=").AppendLine(config.DateCustomFormat1);
        sw.Append("DateCustomSeparator1=").AppendLine(config.DateCustomSeparator1);
        sw.Append("DateCustomFormat2=").AppendLine(config.DateCustomFormat2);
        sw.Append("DateCustomSeparator2=").AppendLine(config.DateCustomSeparator2);
        sw.Append("DateCustomFormat3=").AppendLine(config.DateCustomFormat3);
        sw.Append("DateCustomSeparator3=").AppendLine(config.DateCustomSeparator3);
        sw.Append("DateCustomFormat4=").AppendLine(config.DateCustomFormat4);

        sw.Append("DaysRecent=").AppendLine(config.DaysRecent);

        sw.Append("ConvertWAVsTo16BitOnInstall=").AppendLine(config.ConvertWAVsTo16BitOnInstall);
        sw.Append("ConvertOGGsToWAVsOnInstall=").AppendLine(config.ConvertOGGsToWAVsOnInstall);
        sw.Append("UseOldMantlingForOldDarkFMs=").AppendLine(config.UseOldMantlingForOldDarkFMs);
        sw.Append("HideUninstallButton=").AppendLine(config.HideUninstallButton);
        sw.Append("HideFMListZoomButtons=").AppendLine(config.HideFMListZoomButtons);
        sw.Append("HideExitButton=").AppendLine(config.HideExitButton);
        sw.Append("HideWebSearchButton=").AppendLine(config.HideWebSearchButton);
        sw.Append("ConfirmBeforeInstall=").AppendLine(config.ConfirmBeforeInstall);
        sw.Append("ConfirmUninstall=").AppendLine(config.ConfirmUninstall);
        sw.Append("BackupFMData=").AppendLine(config.BackupFMData);
        sw.Append("BackupAlwaysAsk=").AppendLine(config.BackupAlwaysAsk);
        sw.Append("Language=").AppendLine(config.Language);
        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            sw.Append(GetGamePrefix(gameIndex)).Append("WebSearchUrl=").AppendLine(config.GetWebSearchUrl(gameIndex));
        }
        sw.Append("ConfirmPlayOnDCOrEnter=").AppendLine(config.ConfirmPlayOnDCOrEnter);

        sw.Append("RunThiefBuddyOnFMPlay=").AppendLine(config.RunThiefBuddyOnFMPlay);

        #endregion

        #region Filters

        for (int i = 0; i < SupportedGameCount; i++)
        {
            sw.Append(GetGamePrefix((GameIndex)i)).Append("GameFilterVisible=").AppendLine(config.GameFilterControlVisibilities[i].ToString());
        }

        for (int i = 0; i < HideableFilterControlsCount; i++)
        {
            sw.Append("FilterVisible").Append((HideableFilterControls)i).Append('=').AppendLine(config.FilterControlVisibilities[i].ToString());
        }

        StringBuilder tagsToStringSB = new(FMTags.TagsToStringSBInitialCapacity);

        for (int i = 0; i < SupportedGameCount + 1; i++)
        {
            Filter filter = i == 0 ? config.Filter : config.GameTabsState.GetFilter((GameIndex)(i - 1));
            string p = i == 0 ? "" : GetGamePrefix((GameIndex)(i - 1));

            if (i == 0)
            {
                sw.Append("FilterGames=");
                CommaCombineGameFlags(sw, config.Filter.Games);
            }

            sw.Append(p).Append("FilterTitle=").AppendLine(filter.Title);
            sw.Append(p).Append("FilterAuthor=").AppendLine(filter.Author);

            sw.Append(p).Append("FilterReleaseDateFrom=").AppendLine(FilterDate(filter.ReleaseDateFrom));
            sw.Append(p).Append("FilterReleaseDateTo=").AppendLine(FilterDate(filter.ReleaseDateTo));

            sw.Append(p).Append("FilterLastPlayedFrom=").AppendLine(FilterDate(filter.LastPlayedFrom));
            sw.Append(p).Append("FilterLastPlayedTo=").AppendLine(FilterDate(filter.LastPlayedTo));

            sw.Append(p).Append("FilterFinishedStates=");
            CommaCombineFinishedStates(sw, filter.Finished);

            sw.Append(p).Append("FilterRatingFrom=").AppendLine(filter.RatingFrom);
            sw.Append(p).Append("FilterRatingTo=").AppendLine(filter.RatingTo);

            sw.Append(p).Append("FilterTagsAnd=").AppendLine(TagsToString(filter.Tags.AndTags, tagsToStringSB));
            sw.Append(p).Append("FilterTagsOr=").AppendLine(TagsToString(filter.Tags.OrTags, tagsToStringSB));
            sw.Append(p).Append("FilterTagsNot=").AppendLine(TagsToString(filter.Tags.NotTags, tagsToStringSB));
        }

        #endregion

        #region Columns

        sw.Append("SortedColumn=").AppendLine(config.SortedColumn);
        sw.Append("SortDirection=").AppendLine(config.SortDirection);
        sw.Append("ShowRecentAtTop=").AppendLine(config.ShowRecentAtTop);
        sw.Append("ShowUnsupported=").AppendLine(config.ShowUnsupported);
        sw.Append("ShowUnavailableFMs=").AppendLine(config.ShowUnavailableFMs);
        sw.Append("FMsListFontSizeInPoints=").AppendLine(config.FMsListFontSizeInPoints.ToString(NumberFormatInfo.InvariantInfo));

        foreach (ColumnData col in config.Columns)
        {
            sw.Append("Column").Append(col.Id).Append('=').Append(col.DisplayIndex).Append(',').Append(col.Width).Append(',').AppendLine(col.Visible);
        }

        #endregion

        #region Selected FM

        for (int i = 0; i < SupportedGameCount + 1; i++)
        {
            SelectedFM selFM = i == 0 ? config.SelFM : config.GameTabsState.GetSelectedFM((GameIndex)(i - 1));
            string p = i == 0 ? "" : GetGamePrefix((GameIndex)(i - 1));

            sw.Append(p).Append("SelFMInstDir=").AppendLine(selFM.InstalledName);
            sw.Append(p).Append("SelFMIndexFromTop=").AppendLine(selFM.IndexFromTop);
        }

        #endregion

        #region Main window state

        sw.Append("MainWindowState=").AppendLine(config.MainWindowState);

        sw.Append("MainWindowSize=").Append(config.MainWindowSize.Width).Append(',').AppendLine(config.MainWindowSize.Height);
        sw.Append("MainWindowLocation=").Append(config.MainWindowLocation.X).Append(',').AppendLine(config.MainWindowLocation.Y);

        sw.Append("MainSplitterPercent=").AppendLine(config.MainSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
        sw.Append("TopSplitterPercent=").AppendLine(config.TopSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
        sw.Append("BottomSplitterPercent=").AppendLine(config.BottomSplitterPercent.ToString(NumberFormatInfo.InvariantInfo));
        sw.Append("TopRightPanelCollapsed=").AppendLine(config.TopFMTabsPanelCollapsed);
        sw.Append("BottomRightPanelCollapsed=").AppendLine(config.BottomFMTabsPanelCollapsed);

        sw.Append("GameTab=").AppendLine(config.GameTab);
        sw.Append("SelectedTab1=").AppendLine(config.FMTabsData.SelectedTab);
        sw.Append("SelectedTab2=").AppendLine(config.FMTabsData.SelectedTab2);

        sw.Append("StatsTabPosition=").AppendLine(config.FMTabsData.GetTab(FMTab.Statistics).DisplayIndex);
        sw.Append("EditFMTabPosition=").AppendLine(config.FMTabsData.GetTab(FMTab.EditFM).DisplayIndex);
        sw.Append("CommentTabPosition=").AppendLine(config.FMTabsData.GetTab(FMTab.Comment).DisplayIndex);
        sw.Append("TagsTabPosition=").AppendLine(config.FMTabsData.GetTab(FMTab.Tags).DisplayIndex);
        sw.Append("PatchTabPosition=").AppendLine(config.FMTabsData.GetTab(FMTab.Patch).DisplayIndex);
        sw.Append("ModsTabPosition=").AppendLine(config.FMTabsData.GetTab(FMTab.Mods).DisplayIndex);
        sw.Append("ScreenshotsTabPosition=").AppendLine(config.FMTabsData.GetTab(FMTab.Screenshots).DisplayIndex);

        sw.Append("StatsTabVisible=").AppendLine(config.FMTabsData.GetTab(FMTab.Statistics).Visible);
        sw.Append("EditFMTabVisible=").AppendLine(config.FMTabsData.GetTab(FMTab.EditFM).Visible);
        sw.Append("CommentTabVisible=").AppendLine(config.FMTabsData.GetTab(FMTab.Comment).Visible);
        sw.Append("TagsTabVisible=").AppendLine(config.FMTabsData.GetTab(FMTab.Tags).Visible);
        sw.Append("PatchTabVisible=").AppendLine(config.FMTabsData.GetTab(FMTab.Patch).Visible);
        sw.Append("ModsTabVisible=").AppendLine(config.FMTabsData.GetTab(FMTab.Mods).Visible);
        sw.Append("ScreenshotsTabVisible=").AppendLine(config.FMTabsData.GetTab(FMTab.Screenshots).Visible);

        sw.Append("ReadmeZoomFactor=").AppendLine(config.ReadmeZoomFactor.ToString(NumberFormatInfo.InvariantInfo));
        sw.Append("ReadmeUseFixedWidthFont=").AppendLine(config.ReadmeUseFixedWidthFont);

        #endregion

        sw.Append("EnableCharacterDetailFix=").AppendLine(config.EnableCharacterDetailFix);
        sw.Append("PlayOriginalSeparateButtons=").AppendLine(config.PlayOriginalSeparateButtons);
        sw.Append("AskedToScanForMisCounts=").AppendLine(config.AskedToScanForMisCounts);
        sw.Append("EnableFuzzySearch=").AppendLine(config.EnableFuzzySearch);
        sw.Append("CheckForUpdates=").AppendLine(config.CheckForUpdates);
        sw.Append("ScreenshotGammaPercent=").AppendLine(config.ScreenshotGammaPercent);

        sw.Append("IOThreadsMode=").AppendLine(config.IOThreadsMode);
        sw.Append("CustomIOThreadCount=").AppendLine(config.CustomIOThreadCount);
        sw.Append("DriveMultithreadingLevel=");
        var driveLettersAndTypes = config.DriveLettersAndTypes.OrderBy(static x => x.Key).ToArray();
        for (int i = 0; i < driveLettersAndTypes.Length; i++)
        {
            var item = driveLettersAndTypes[i];
            if (i > 0) sw.Append(',');
            sw.Append(item.Key).Append(':').Append(item.Value);
        }
        sw.AppendLine();
    }
}
