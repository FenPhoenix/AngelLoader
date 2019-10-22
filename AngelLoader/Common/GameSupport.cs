using System;
using AngelLoader.Common.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Common
{
    // @GENGAMES: The main location to add games
    [PublicAPI]
    internal static class GameSupport
    {
        // As much as possible, put all the game stuff in here, so when I add a new game I minimize the places in
        // the code that need updating.

        #region Enums

        // This is flags so we can combine its values for filtering by multiple games.
        [Flags]
        internal enum Game : uint
        {
            Null = 0,

            // Known/supported games
            Thief1 = 1,
            Thief2 = 2,
            Thief3 = 4,
            SS2 = 8,

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

        private static readonly string[] SteamAppIds =
        {
            "211600", // Thief Gold
            "211740", // Thief 2
            "6980",   // Thief 3
            "238210"  // SS2
        };

        #endregion

        internal static readonly int SupportedGameCount = Enum.GetValues(typeof(GameIndex)).Length;

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
            _ => Game.SS2,
        };

        #endregion

        #region Get game name from game type

        internal static string GetGameNameFromGameType(GameIndex gameIndex) => GetGameNameFromGameType(GameIndexToGame(gameIndex));

        internal static string GetGameNameFromGameType(Game game) => game switch
        {
            Game.Thief1 => LText.Global.Thief1,
            Game.Thief2 => LText.Global.Thief2,
            Game.Thief3 => LText.Global.Thief3,
            Game.SS2 => LText.Global.SystemShock2,
            _ => "[UnknownGameType]"
        };

        internal static string GetShortGameNameFromGameType(GameIndex gameIndex) => GetShortGameNameFromGameType(GameIndexToGame(gameIndex));

        internal static string GetShortGameNameFromGameType(Game game) => game switch
        {
            Game.Thief1 => LText.Global.Thief1_Short,
            Game.Thief2 => LText.Global.Thief2_Short,
            Game.Thief3 => LText.Global.Thief3_Short,
            Game.SS2 => LText.Global.SystemShock2_Short,
            _ => "[UnknownShortGameType]"
        };

        #endregion

        #region Game type checks

        internal static bool GameIsDark(Game game) => game == Game.Thief1 || game == Game.Thief2 || game == Game.SS2;
        internal static bool GameIsDark(GameIndex game) => game == GameIndex.Thief1 || game == GameIndex.Thief2 || game == GameIndex.SS2;
        internal static bool GameIsKnownAndSupported(Game game) => game != Game.Null && game != Game.Unsupported;

        #endregion
    }
}
