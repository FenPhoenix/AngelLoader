using System.Diagnostics.CodeAnalysis;

namespace AngelLoader.DataClasses
{
    // @Localization(ErrorText): Keep this out of LText until we decide if we want it localizable or not
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    internal static class ErrorText
    {
        internal static readonly string Ex = "Exception ";
        internal static readonly string UnableToOpenLogFile = "Unable to open log file.";
        internal static readonly string ScanErrors = "One or more errors occurred while scanning.";
        internal static readonly string UnableToStartExecutable = "Unable to start executable.";
        internal static readonly string UnableToOpenFMFolder = "Unable to open FM folder.";
        internal static readonly string UnableToOpenHTMLReadme = "Unable to open HTML readme.";
        internal static readonly string HTMLReadmeNotFound = "The HTML readme file could not be found.";
        internal static readonly string UnableToOpenLink = "Unable to open link.";
        internal static readonly string FMGameU = "FM game type is unknown or unsupported.";
        internal static readonly string FMGameNotDark = "FM must be for a Dark Engine game.";
        internal static readonly string MPForNonT2 = "Multiplayer is not supported for games other than Thief 2.";
        internal static readonly string GamePathEmpty = "Game directory is empty (a path to the game executable has not been specified).";
        internal static readonly string ExRead = Ex + "reading ";
        internal static readonly string ExWrite = Ex + "writing ";
    }
}
