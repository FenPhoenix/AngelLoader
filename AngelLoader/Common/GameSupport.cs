#define FenGen_GameSupportSource

using System;
using AngelLoader.DataClasses;
using static AngelLoader.FenGenAttributes;
using static AngelLoader.Misc;

namespace AngelLoader
{
    // @GENGAMES (GameSupport): Begin
    public static partial class GameSupport
    {
        internal static readonly int SupportedGameCount = Enum.GetValues(typeof(GameIndex)).Length;

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
            [FenGenGame(prefix: "T1", steamId: "211600")]
            Thief1 = 1,
            [FenGenGame(prefix: "T2", steamId: "211740")]
            Thief2 = 2,
            [FenGenGame(prefix: "T3", steamId: "6980")]
            Thief3 = 4,
            [FenGenGame(prefix: "SS2", steamId: "238210")]
            SS2 = 8,

            [FenGenIgnore]
            Unsupported = 16
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

        #endregion

        #region Game type checks

        internal static bool GameIsDark(Game game) => game is Game.Thief1 or Game.Thief2 or Game.SS2;
        internal static bool GameIsDark(GameIndex game) => game is GameIndex.Thief1 or GameIndex.Thief2 or GameIndex.SS2;
        internal static bool GameIsKnownAndSupported(Game game) => game is not Game.Null and not Game.Unsupported;

        #endregion
    }
    // @GENGAMES (GameSupport): End
}
