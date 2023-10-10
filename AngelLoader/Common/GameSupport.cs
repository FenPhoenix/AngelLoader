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
    internal static readonly string[] SupportedGameNames = Enum.GetNames(typeof(GameIndex));
    internal static readonly int SupportedGameCount = SupportedGameNames.Length;

    // As much as possible, put all the game stuff in here, so when I add a new game I minimize the places in
    // the code that need updating.

    #region Game enums

    // This is flags so we can combine its values for filtering by multiple games.
    [Flags, FenGenGameEnum(gameIndexEnumName: "GameIndex")]
    public enum Game : uint
    {
        [FenGenIgnore]
        Null = 0,

        // Known/supported games
        // IMPORTANT: Prefixes are used in Config.ini, so they must remain the same for compatibility.
        // Don't change the existing values, only add new ones!
        // Obviously the steam ids must remain the same as well.
        [FenGenGame(prefix: "T1", steamId: "211600", editorName: "DromEd")]
        Thief1 = 1,
        [FenGenGame(prefix: "T2", steamId: "211740", editorName: "DromEd")]
        Thief2 = 2,
        [FenGenGame(prefix: "T3", steamId: "6980", editorName: "")]
        Thief3 = 4,
        [FenGenGame(prefix: "SS2", steamId: "238210", editorName: "ShockEd")]
        SS2 = 8,
        [FenGenGame(prefix: "TDM", steamId: "", editorName: "")]
        TDM = 16,

        [FenGenIgnore]
        Unsupported = 32
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
        _ => Game.Null
    };

    internal static bool ConvertsToKnownAndSupported(this Game game, out GameIndex gameIndex)
    {
        if (GameIsKnownAndSupported(game))
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
    internal static bool ConvertsToDark(this Game game, out GameIndex gameIndex)
    {
        if (GameIsDark(game))
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

    internal static bool ConvertsToModSupporting(this Game game, out GameIndex gameIndex)
    {
        if (GameSupportsMods(game))
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
                _ => LText.Difficulties.Expert
            },
            Game.SS2 => difficulty switch
            {
                Difficulty.Normal => LText.Difficulties.Easy,
                Difficulty.Hard => LText.Difficulties.Normal,
                Difficulty.Expert => LText.Difficulties.Hard,
                _ => LText.Difficulties.Impossible
            },
            _ => difficulty switch
            {
                Difficulty.Normal => LText.Difficulties.Normal,
                Difficulty.Hard => LText.Difficulties.Hard,
                Difficulty.Expert => LText.Difficulties.Expert,
                _ => LText.Difficulties.Extreme
            }
        };
    }

    #endregion

    #region Game type checks

    internal static bool GameIsDark(Game game) => game is Game.Thief1 or Game.Thief2 or Game.SS2;
    internal static bool GameIsDark(GameIndex game) => game is GameIndex.Thief1 or GameIndex.Thief2 or GameIndex.SS2;
    internal static bool GameIsKnownAndSupported(Game game) => game is not Game.Null and not Game.Unsupported;

    #endregion

    #region Mod support

    // @GENGAMES(T3 doesn't support mod management) - We could properly generalize this later
    // And have a "supports mods" field for each game etc.

    internal static bool GameSupportsMods(Game game) => GameIsDark(game);
    internal static bool GameSupportsMods(GameIndex game) => GameIsDark(game);

    internal static string GetLocalizedNoModSupportText(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief3 => LText.PlayOriginalGameMenu.Mods_Thief3NotSupported,
        GameIndex.TDM => LText.PlayOriginalGameMenu.Mods_TDMNotSupported,
        _ => ""
    };

    #endregion
}
// @GENGAMES (GameSupport): End
