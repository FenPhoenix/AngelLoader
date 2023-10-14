#define FenGen_GameSupportMainGenDest

using static AL_Common.FenGenAttributes;
using static AngelLoader.Utils;

namespace AngelLoader;

[FenGenGameSupportMainGenDestClass]
public static partial class GameSupport
{
    #region Autogenerated game support code

    internal const int SupportedGameCount = 5;

    public enum GameIndex : uint
    {
        Thief1,
        Thief2,
        Thief3,
        SS2,
        TDM
    }

    #region Per-game constants

    private static readonly string[] _gamePrefixes =
    {
        "T1",
        "T2",
        "T3",
        "SS2",
        "TDM"
    };

    public static string GetGamePrefix(GameIndex index) => _gamePrefixes[(int)index];

    private static readonly string[] _steamAppIds =
    {
        "211600",
        "211740",
        "6980",
        "238210",
        ""
    };

    public static string GetGameSteamId(GameIndex index) => _steamAppIds[(int)index];

    private static readonly string[] _gameEditorNames =
    {
        "DromEd",
        "DromEd",
        "",
        "ShockEd",
        ""
    };

    public static string GetGameEditorName(GameIndex index) => _gameEditorNames[(int)index];

    #endregion

    /// <summary>
    /// Converts a Game to a GameIndex. *Narrowing conversion, so make sure the game has been checked for convertibility first!
    /// </summary>
    /// <param name="game"></param>
    public static GameIndex GameToGameIndex(Game game)
    {
        AssertR(GameIsKnownAndSupported(game), nameof(game) + " was out of range: " + game);

        return game switch
        {
            Game.Thief1 => GameIndex.Thief1,
            Game.Thief2 => GameIndex.Thief2,
            Game.Thief3 => GameIndex.Thief3,
            Game.SS2 => GameIndex.SS2,
            _ => GameIndex.TDM
        };
    }

    /// <summary>
    /// Converts a GameIndex to a Game. Widening conversion, so it will always succeed.
    /// </summary>
    /// <param name="gameIndex"></param>
    public static Game GameIndexToGame(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief1 => Game.Thief1,
        GameIndex.Thief2 => Game.Thief2,
        GameIndex.Thief3 => Game.Thief3,
        GameIndex.SS2 => Game.SS2,
        _ => Game.TDM
    };

    #endregion
}
