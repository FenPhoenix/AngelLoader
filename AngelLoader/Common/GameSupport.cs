#define FenGen_GameSupportSource

using System;
using AngelLoader.DataClasses;
using static AL_Common.FenGenAttributes;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader;

// @GENGAMES (GameSupport): Begin
public static partial class GameSupport
{
    // As much as possible, put all the game stuff in here, so when I add a new game I minimize the places in
    // the code that need updating.

    #region Game enums

    // This is flags so we can combine its values for filtering by multiple games.
    [Flags, FenGenGameEnum(gameIndexEnumName: "GameIndex")]
    public enum Game : byte
    {
        [FenGenIgnore]
        Null = 0,

        // Known/supported games
        // IMPORTANT: Prefixes are used in Config.ini, so they must remain the same for compatibility.
        // Don't change the existing values, only add new ones!
        // Obviously the Steam ids must remain the same as well.

        [FenGenGame(
            prefix: "T1",
            steamId: "211600",
            editorName: "DromEd",
            isDarkEngine: true,
            supportsMods: true,
            supportsImport: true,
            supportsLanguages: true,
            supportsResourceDetection: true,
            requiresBackupPath: true)]
        Thief1 = 1,

        [FenGenGame(
            prefix: "T2",
            steamId: "211740",
            editorName: "DromEd",
            isDarkEngine: true,
            supportsMods: true,
            supportsImport: true,
            supportsLanguages: true,
            supportsResourceDetection: true,
            requiresBackupPath: true)]
        Thief2 = 2,

        [FenGenGame(
            prefix: "T3",
            steamId: "6980",
            editorName: "",
            isDarkEngine: false,
            supportsMods: false,
            supportsImport: true,
            supportsLanguages: false,
            supportsResourceDetection: false,
            requiresBackupPath: true)]
        Thief3 = 4,

        [FenGenGame(
            prefix: "SS2",
            steamId: "238210",
            editorName: "ShockEd",
            isDarkEngine: true,
            supportsMods: true,
            supportsImport: true,
            supportsLanguages: true,
            supportsResourceDetection: true,
            requiresBackupPath: true)]
        SS2 = 8,

        [FenGenGame(
            prefix: "TDM",
            steamId: "",
            editorName: "",
            isDarkEngine: false,
            supportsMods: false,
            supportsImport: false,
            supportsLanguages: false,
            supportsResourceDetection: false,
            requiresBackupPath: false)]
        TDM = 16,

        [FenGenIgnore]
        Unsupported = 32,
    }

    #endregion

    #region Conversion

    // Do a hard convert at the API boundary, even though these now match the ordering
    // NOTE: One is flags and the other isn't, so remember that if you ever want to array-ize this!
    internal static Game ScannerGameToGame(FMScanner.Game scannerGame) => scannerGame switch
    {
        FMScanner.Game.Unsupported => Game.Unsupported,
        FMScanner.Game.Thief1 => Game.Thief1,
        FMScanner.Game.Thief2 => Game.Thief2,
        FMScanner.Game.Thief3 => Game.Thief3,
        FMScanner.Game.SS2 => Game.SS2,
        FMScanner.Game.TDM => Game.TDM,
        _ => Game.Null,
    };

    internal static bool ConvertsToDarkThief(this Game game, out GameIndex gameIndex)
    {
        if (game is Game.Thief1 or Game.Thief2)
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = GameIndex.Thief1;
            return false;
        }
    }

    #endregion

    #region Get game-related localized strings

    internal static string GetLocalizedDifficultyName(Game game, Difficulty difficulty)
    {
        AssertR(difficulty != 0, nameof(difficulty) + " was out of range: " + difficulty);

        // More verbose but also more clear
        return game switch
        {
            Game.Thief3 => difficulty switch
            {
                Difficulty.Normal => LText.Difficulties.Easy,
                Difficulty.Hard => LText.Difficulties.Normal,
                Difficulty.Expert => LText.Difficulties.Hard,
                _ => LText.Difficulties.Expert,
            },
            Game.SS2 => difficulty switch
            {
                Difficulty.Normal => LText.Difficulties.Easy,
                Difficulty.Hard => LText.Difficulties.Normal,
                Difficulty.Expert => LText.Difficulties.Hard,
                _ => LText.Difficulties.Impossible,
            },
            _ => difficulty switch
            {
                Difficulty.Normal => LText.Difficulties.Normal,
                Difficulty.Hard => LText.Difficulties.Hard,
                Difficulty.Expert => LText.Difficulties.Expert,
                _ => LText.Difficulties.Extreme,
            },
        };
    }

    #endregion

    internal static bool GameDirNeedsWriteAccess(GameIndex gameIndex)
    {
        return gameIndex != GameIndex.Thief3 || Paths.SneakyUpgradeIsPortable();
    }

    internal static string GetLocalizedOpenInEditorMessage(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief1 => LText.FMsList.FMMenu_OpenInDromEd,
        GameIndex.Thief2 => LText.FMsList.FMMenu_OpenInDromEd,
        GameIndex.SS2 => LText.FMsList.FMMenu_OpenInShockEd,
        _ => "",
    };

    private static readonly Version _newDark128_Thief = new(1, 2, 8, 0);
    private static readonly Version _newDark128_SS2 = new(2, 4, 9, 0);

    /// <summary>
    /// Gets the version from the config object, not from disk. Use when you need to avoid a disk-hit delay.
    /// </summary>
    /// <param name="gameIndex"></param>
    /// <returns></returns>
    internal static bool ConfigStoredGameIsNewDark128OrAbove(GameIndex gameIndex)
    {
        switch (gameIndex)
        {
            case GameIndex.Thief1 or GameIndex.Thief2:
            {
                Version? version = Config.GetDarkGameVersion(gameIndex);
                return version != null && version >= _newDark128_Thief;
            }
            case GameIndex.SS2:
            {
                Version? version = Config.GetDarkGameVersion(gameIndex);
                return version != null && version >= _newDark128_SS2;
            }
            default:
                return false;
        }
    }
}
// @GENGAMES (GameSupport): End
