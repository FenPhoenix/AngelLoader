#define FenGen_GameSupport

using System;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.Attributes;
using static AngelLoader.Misc;

namespace AngelLoader
{
    // @GENGAMES (GameSupport): Begin
    [PublicAPI]
    internal static class GameSupport
    {
        internal static readonly int SupportedGameCount = Enum.GetValues(typeof(GameIndex)).Length;

        // As much as possible, put all the game stuff in here, so when I add a new game I minimize the places in
        // the code that need updating.

        #region Game enums

        // This is flags so we can combine its values for filtering by multiple games.
        [Flags, FenGenGameEnum]
        internal enum Game : uint
        {
            [FenGenNotAGameType]
            Null = 0,

            // Known/supported games
            Thief1 = 1,
            Thief2 = 2,
            Thief3 = 4,
            SS2 = 8,

            [FenGenNotAGameType]
            Unsupported = 16
        }

        // This is sequential so we can use it as an indexer into any same-ordered array. That way, we can avoid
        // having to specify games individually everywhere throughout the code, and instead just do a loop and
        // have it all done implicitly wherever it needs to be done.
        internal enum GameIndex : uint
        {
            Thief1,
            Thief2,
            Thief3,
            SS2
        }

        #endregion

        #region Per-game constants

        // IMPORTANT: These are used in Config.ini, so they must remain the same for compatibility. Don't change
        // the existing values, only add new ones!
        private static readonly string[]
        GamePrefixes =
        {
            "T1",
            "T2",
            "T3",
            "SS2"
        };

        private static readonly string[]
        SteamAppIds =
        {
            "211600", // Thief Gold
            "211740", // Thief 2
            "6980",   // Thief 3
            "238210"  // SS2
        };

        #endregion

        internal static string GetGamePrefix(GameIndex index) => GamePrefixes[(int)index];

        internal static string GetGameSteamId(GameIndex index) => SteamAppIds[(int)index];

        #region Conversion

        /// <summary>
        /// Converts a Game to a GameIndex. *Narrowing conversion, so make sure the game has been checked for convertibility first!
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        internal static GameIndex GameToGameIndex(Game game) => game switch
        {
            Game.Thief1 => GameIndex.Thief1,
            Game.Thief2 => GameIndex.Thief2,
            Game.Thief3 => GameIndex.Thief3,
            Game.SS2 => GameIndex.SS2,
            Game.Null => throw new IndexOutOfRangeException(nameof(game) + " was " + nameof(Game.Null) + ", which is not convertible to GameIndex."),
            Game.Unsupported => throw new IndexOutOfRangeException(nameof(game) + " was " + nameof(Game.Unsupported) + ", which is not convertible to GameIndex."),
            _ => throw new IndexOutOfRangeException(nameof(game) + " was an out-of-range value which is not convertible to GameIndex.")
        };

        /// <summary>
        /// Converts a GameIndex to a Game. Widening conversion, so it will always succeed.
        /// </summary>
        /// <param name="gameIndex"></param>
        /// <returns></returns>
        internal static Game GameIndexToGame(GameIndex gameIndex) => gameIndex switch
        {
            GameIndex.Thief1 => Game.Thief1,
            GameIndex.Thief2 => Game.Thief2,
            GameIndex.Thief3 => Game.Thief3,
            _ => Game.SS2
        };

        // Do a hard convert at the API boundary, even though these now match the ordering
        // NOTE: One is flags and the other isn't, so remember that if you ever want to array-ize this!
        internal static Game ScannerGameToGame(FMScanner.Game scannerGame) => scannerGame switch
        {
            FMScanner.Game.Unsupported => Game.Unsupported,
            FMScanner.Game.Thief1 => Game.Thief1,
            FMScanner.Game.Thief2 => Game.Thief2,
            FMScanner.Game.Thief3 => Game.Thief3,
            FMScanner.Game.SS2 => Game.SS2,
            _ => Game.Null
        };

        #endregion

        #region Get game-related localized strings

        internal static string GetLocalizedDifficultyName(Game game, Difficulty difficulty)
        {
            // More verbose but also more clear
            return game == Game.Thief3 ? difficulty switch
            {
                Difficulty.Normal => LText.Difficulties.Easy,
                Difficulty.Hard => LText.Difficulties.Normal,
                Difficulty.Expert => LText.Difficulties.Hard,
                Difficulty.Extreme => LText.Difficulties.Expert,
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty) + " is not a valid value")
            }
            : game == Game.SS2 ? difficulty switch
            {
                Difficulty.Normal => LText.Difficulties.Easy,
                Difficulty.Hard => LText.Difficulties.Normal,
                Difficulty.Expert => LText.Difficulties.Hard,
                Difficulty.Extreme => LText.Difficulties.Impossible,
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty) + " is not a valid value")
            }
            : difficulty switch
            {
                Difficulty.Normal => LText.Difficulties.Normal,
                Difficulty.Hard => LText.Difficulties.Hard,
                Difficulty.Expert => LText.Difficulties.Expert,
                Difficulty.Extreme => LText.Difficulties.Extreme,
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty) + " is not a valid value")
            };
        }

        internal static string GetLocalizedGameName(GameIndex gameIndex) => GetLocalizedGameName(GameIndexToGame(gameIndex));

        internal static string GetLocalizedGameName(Game game) => game switch
        {
            Game.Thief1 => LText.Global.Thief1,
            Game.Thief2 => LText.Global.Thief2,
            Game.Thief3 => LText.Global.Thief3,
            Game.SS2 => LText.Global.SystemShock2,
            _ => throw new ArgumentOutOfRangeException(nameof(game), game, nameof(GetLocalizedGameName) + @": Game not in range")
        };

        internal static string GetShortLocalizedGameName(GameIndex gameIndex) => GetShortLocalizedGameName(GameIndexToGame(gameIndex));

        internal static string GetShortLocalizedGameName(Game game) => game switch
        {
            Game.Thief1 => LText.Global.Thief1_Short,
            Game.Thief2 => LText.Global.Thief2_Short,
            Game.Thief3 => LText.Global.Thief3_Short,
            Game.SS2 => LText.Global.SystemShock2_Short,
            _ => throw new ArgumentOutOfRangeException(nameof(game), game, nameof(GetShortLocalizedGameName) + @": Game not in range")
        };

        internal static string GetLocalizedGameNameColon(GameIndex gameIndex) => GetLocalizedGameNameColon(GameIndexToGame(gameIndex));

        internal static string GetLocalizedGameNameColon(Game game) => game switch
        {
            Game.Thief1 => LText.Global.Thief1_Colon,
            Game.Thief2 => LText.Global.Thief2_Colon,
            Game.Thief3 => LText.Global.Thief3_Colon,
            Game.SS2 => LText.Global.SystemShock2_Colon,
            _ => throw new ArgumentOutOfRangeException(nameof(game), game, nameof(GetLocalizedGameNameColon) + @": Game not in range")
        };

        #endregion

        #region Game type checks

        internal static bool GameIsDark(Game game) => game == Game.Thief1 || game == Game.Thief2 || game == Game.SS2;
        internal static bool GameIsDark(GameIndex game) => game == GameIndex.Thief1 || game == GameIndex.Thief2 || game == GameIndex.SS2;
        internal static bool GameIsKnownAndSupported(Game game) => game != Game.Null && game != Game.Unsupported;

        #endregion
    }
    // @GENGAMES (GameSupport): End
}
