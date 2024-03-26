#define FenGen_GameSupportMainGenDest

using System;
using static AL_Common.FenGenAttributes;

namespace AngelLoader;

[FenGenGameSupportMainGenDestClass]
public static partial class GameSupport
{
    #region Autogenerated game support code

    public const int SupportedGameCount = 5;
    public const int DarkGameCount = 3;
    public const int ModSupportingGameCount = 3;
    public const int ImportSupportingGameCount = 4;
    public const int LanguageSupportingGameCount = 3;
    public const int ResourceDetectionSupportingGameCount = 3;
    public const int BackupRequiringGameCount = 4;

    public static readonly string[] SupportedGameNames =
    {
        "Thief1",
        "Thief2",
        "Thief3",
        "SS2",
        "TDM"
    };

    public enum GameIndex : byte
    {
        Thief1,
        Thief2,
        Thief3,
        SS2,
        TDM
    }

    #region General

    /// <summary>
    /// Converts a Game to a GameIndex. *Narrowing conversion, so make sure the game has been checked for convertibility first!
    /// </summary>
    /// <param name="game"></param>
    private static GameIndex GameToGameIndex(Game game) => game switch
    {
        Game.Thief1 => GameIndex.Thief1,
        Game.Thief2 => GameIndex.Thief2,
        Game.Thief3 => GameIndex.Thief3,
        Game.SS2 => GameIndex.SS2,
        _ => GameIndex.TDM
    };

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

    public static bool GameIsKnownAndSupported(Game game) =>
        game
            is not Game.Null
            and not Game.Unsupported;

    public static bool ConvertsToKnownAndSupported(this Game game, out GameIndex gameIndex)
    {
        if (GameIsKnownAndSupported(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    #endregion

    #region GetGamePrefix

    private static readonly String[] _gamePrefixes =
    {
        "T1",
        "T2",
        "T3",
        "SS2",
        "TDM"
    };

    public static String GetGamePrefix(GameIndex index) => _gamePrefixes[(byte)index];

    #endregion

    #region GetGameSteamId

    private static readonly String[] _steamAppIds =
    {
        "211600",
        "211740",
        "6980",
        "238210",
        ""
    };

    public static String GetGameSteamId(GameIndex index) => _steamAppIds[(byte)index];

    #endregion

    #region GetGameEditorName

    private static readonly String[] _gameEditorNames =
    {
        "DromEd",
        "DromEd",
        "",
        "ShockEd",
        ""
    };

    public static String GetGameEditorName(GameIndex index) => _gameEditorNames[(byte)index];

    #endregion

    #region GameIsDark

    private static readonly Boolean[] _isDark =
    {
        true,
        true,
        false,
        true,
        false
    };

    public static Boolean GameIsDark(GameIndex index) => _isDark[(byte)index];

    public static bool GameIsDark(Game game) =>
        game.ConvertsToKnownAndSupported(out GameIndex gameIndex) &&
        GameIsDark(gameIndex);

    public static bool ConvertsToDark(this Game game, out GameIndex gameIndex)
    {
        if (GameIsDark(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    public static bool ConvertsToKnownButNotDark(this Game game, out GameIndex gameIndex)
    {
        if (GameIsKnownAndSupported(game) && !GameIsDark(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    #endregion

    #region GameSupportsMods

    private static readonly Boolean[] _supportsMods =
    {
        true,
        true,
        false,
        true,
        false
    };

    public static Boolean GameSupportsMods(GameIndex index) => _supportsMods[(byte)index];

    public static bool GameSupportsMods(Game game) =>
        game.ConvertsToKnownAndSupported(out GameIndex gameIndex) &&
        GameSupportsMods(gameIndex);

    public static bool ConvertsToModSupporting(this Game game, out GameIndex gameIndex)
    {
        if (GameSupportsMods(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    public static bool ConvertsToKnownButNotModSupporting(this Game game, out GameIndex gameIndex)
    {
        if (GameIsKnownAndSupported(game) && !GameSupportsMods(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    #endregion

    #region GameSupportsImport

    private static readonly Boolean[] _supportsImport =
    {
        true,
        true,
        true,
        true,
        false
    };

    public static Boolean GameSupportsImport(GameIndex index) => _supportsImport[(byte)index];

    public static bool GameSupportsImport(Game game) =>
        game.ConvertsToKnownAndSupported(out GameIndex gameIndex) &&
        GameSupportsImport(gameIndex);

    public static bool ConvertsToImportSupporting(this Game game, out GameIndex gameIndex)
    {
        if (GameSupportsImport(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    public static bool ConvertsToKnownButNotImportSupporting(this Game game, out GameIndex gameIndex)
    {
        if (GameIsKnownAndSupported(game) && !GameSupportsImport(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    #endregion

    #region GameSupportsLanguages

    private static readonly Boolean[] _supportsLanguages =
    {
        true,
        true,
        false,
        true,
        false
    };

    public static Boolean GameSupportsLanguages(GameIndex index) => _supportsLanguages[(byte)index];

    public static bool GameSupportsLanguages(Game game) =>
        game.ConvertsToKnownAndSupported(out GameIndex gameIndex) &&
        GameSupportsLanguages(gameIndex);

    public static bool ConvertsToLanguageSupporting(this Game game, out GameIndex gameIndex)
    {
        if (GameSupportsLanguages(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    public static bool ConvertsToKnownButNotLanguageSupporting(this Game game, out GameIndex gameIndex)
    {
        if (GameIsKnownAndSupported(game) && !GameSupportsLanguages(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    #endregion

    #region GameSupportsResourceDetection

    private static readonly Boolean[] _supportsResourceDetection =
    {
        true,
        true,
        false,
        true,
        false
    };

    public static Boolean GameSupportsResourceDetection(GameIndex index) => _supportsResourceDetection[(byte)index];

    public static bool GameSupportsResourceDetection(Game game) =>
        game.ConvertsToKnownAndSupported(out GameIndex gameIndex) &&
        GameSupportsResourceDetection(gameIndex);

    public static bool ConvertsToResourceDetectionSupporting(this Game game, out GameIndex gameIndex)
    {
        if (GameSupportsResourceDetection(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    public static bool ConvertsToKnownButNotResourceDetectionSupporting(this Game game, out GameIndex gameIndex)
    {
        if (GameIsKnownAndSupported(game) && !GameSupportsResourceDetection(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    #endregion

    #region GameRequiresBackupPath

    private static readonly Boolean[] _requiresBackupPath =
    {
        true,
        true,
        true,
        true,
        false
    };

    public static Boolean GameRequiresBackupPath(GameIndex index) => _requiresBackupPath[(byte)index];

    public static bool GameRequiresBackupPath(Game game) =>
        game.ConvertsToKnownAndSupported(out GameIndex gameIndex) &&
        GameRequiresBackupPath(gameIndex);

    public static bool ConvertsToBackupPathRequiring(this Game game, out GameIndex gameIndex)
    {
        if (GameRequiresBackupPath(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    public static bool ConvertsToKnownButNotBackupPathRequiring(this Game game, out GameIndex gameIndex)
    {
        if (GameIsKnownAndSupported(game) && !GameRequiresBackupPath(game))
        {
            gameIndex = GameToGameIndex(game);
            return true;
        }
        else
        {
            gameIndex = default;
            return false;
        }
    }

    #endregion

    #endregion
}
