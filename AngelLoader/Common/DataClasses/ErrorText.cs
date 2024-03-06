using System.Diagnostics.CodeAnalysis;

namespace AngelLoader.DataClasses;

// @Localization(ErrorText): Keep this out of LText until we decide if we want it localizable or not
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
#pragma warning disable RCS1187 // Use constant instead of field.
internal static class ErrorText
{
    internal static readonly string Ex = "Exception ";
    internal static readonly string Un = "Unable to ";
    internal static readonly string FT = "Failed to ";
    internal static readonly string FTDel = FT + "delete ";
    internal static readonly string UnOpenLogFile = Un + "open log file.";
    internal static readonly string UnOpenFMDir = Un + "open FM folder.";
    internal static readonly string UnOpenHTMLReadme = Un + "open HTML readme ";
    internal static readonly string HTMLReadmeNotFound = "The HTML readme file could not be found.";
    internal static readonly string UnOpenLink = Un + "open link.";
    internal static readonly string FMGameU = "FM game type is unknown or unsupported.";
    internal static readonly string FMGameNotDark = "FM must be for a Dark Engine game.";
    internal static readonly string MPForNonT2 = "Multiplayer is not supported for games other than Thief 2.";
    internal static readonly string GamePathEmpty = "Game path is empty.";
    internal static readonly string ExRead = Ex + "reading ";
    internal static readonly string ExWrite = Ex + "writing ";
    internal static readonly string ExOpen = Ex + "opening ";
    internal static readonly string ExTry = Ex + "trying to ";
    internal static readonly string ExCreate = Ex + "creating ";
    internal static readonly string ExCopy = Ex + "copying ";
    //internal static readonly string ExExit = Ex + "exiting ";
    internal static readonly string ExGet = Ex + "getting ";
    private static readonly string Ret = "Returning ";
    internal static readonly string ExDetLangIn = ExTry + "detect language folders in ";
    //internal static readonly string RetT = Ret + "'true'.";
    internal static readonly string RetF = Ret + "'false'.";
    internal static readonly string OldDarkDependentFeaturesWillFail =
        "The following features/fixes will NOT be applied on this run:\r\n\r\n" +
        "-Old mantling, if the FM is OldDark and \"Use old mantling for OldDark FMs\" is enabled\r\n" +
        "-Palette fix, if the FM is OldDark and requires it";
    internal static readonly string LangDefault = "Language will be set to English and no other languages will be available.";
    internal static readonly string FoundRegKey = "Found the registry key but ";
    internal static readonly string RegKeyPath = "Registry key path was: ";
    internal static readonly string FMInstDirNF = "FM install directory not found.";
    internal static readonly string FMScreenshotsDirNF = "FM screenshot directory not found.";
    internal static readonly string ExInLWT = Ex + "in last write time compare ";
}
#pragma warning restore RCS1187 // Use constant instead of field.
