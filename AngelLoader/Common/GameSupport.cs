#define FenGen_GameSupport

using System;
using AngelLoader.DataClasses;
using static AngelLoader.FenGenAttributes;
using static AngelLoader.Misc;

namespace AngelLoader
{
    // @GENGAMES (GameSupport): Begin
    public static class GameSupport
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
        public enum GameIndex : uint
        {
            Thief1,
            Thief2,
            Thief3,
            SS2
        }

        #endregion

        #region Per-game constants

        // IMPORTANT: These are used in Config.ini, so they must remain the same for compatibility.
        // Don't change the existing values, only add new ones!
        private static readonly string[]
        _gamePrefixes =
        {
            "T1",
            "T2",
            "T3",
            "SS2"
        };

        private static readonly string[]
        _steamAppIds =
        {
            "211600", // Thief Gold
            "211740", // Thief 2
            "6980",   // Thief 3
            "238210"  // SS2
        };

        #endregion

        internal static string GetGamePrefix(GameIndex index) => _gamePrefixes[(int)index];

        internal static string GetGameSteamId(GameIndex index) => _steamAppIds[(int)index];

        #region Conversion

        /// <summary>
        /// Converts a Game to a GameIndex. *Narrowing conversion, so make sure the game has been checked for convertibility first!
        /// </summary>
        /// <param name="game"></param>
        internal static GameIndex GameToGameIndex(Game game)
        {
            AssertR(game != Game.Null && game != Game.Unsupported, nameof(game) + " was out of range: " + game);

            return game switch
            {
                Game.Thief1 => GameIndex.Thief1,
                Game.Thief2 => GameIndex.Thief2,
                Game.Thief3 => GameIndex.Thief3,
                _ => GameIndex.SS2
            };
        }

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

        internal static string GetLocalizedGameName(GameIndex gameIndex) => GetLocalizedGameName(GameIndexToGame(gameIndex));

        internal static string GetLocalizedGameName(Game game)
        {
            AssertR(game != Game.Null && game != Game.Unsupported, nameof(game) + " was out of range: " + game);

            return game switch
            {
                Game.Thief1 => LText.Global.Thief1,
                Game.Thief2 => LText.Global.Thief2,
                Game.Thief3 => LText.Global.Thief3,
                _ => LText.Global.SystemShock2
            };
        }

        internal static string GetShortLocalizedGameName(GameIndex gameIndex) => gameIndex switch
        {
            GameIndex.Thief1 => LText.Global.Thief1_Short,
            GameIndex.Thief2 => LText.Global.Thief2_Short,
            GameIndex.Thief3 => LText.Global.Thief3_Short,
            _ => LText.Global.SystemShock2_Short
        };

        internal static string GetLocalizedGameNameColon(GameIndex gameIndex) => gameIndex switch
        {
            GameIndex.Thief1 => LText.Global.Thief1_Colon,
            GameIndex.Thief2 => LText.Global.Thief2_Colon,
            GameIndex.Thief3 => LText.Global.Thief3_Colon,
            _ => LText.Global.SystemShock2_Colon
        };

        #endregion

        #region Game type checks

        internal static bool GameIsDark(Game game) => game is Game.Thief1 or Game.Thief2 or Game.SS2;
        internal static bool GameIsDark(GameIndex game) => game is GameIndex.Thief1 or GameIndex.Thief2 or GameIndex.SS2;
        internal static bool GameIsKnownAndSupported(Game game) => game is not Game.Null and not Game.Unsupported;

        #endregion
    }
    // @GENGAMES (GameSupport): End
}
