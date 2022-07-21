using System.Diagnostics.CodeAnalysis;

namespace AngelLoader.DataClasses
{
    // @Localization(ErrorText): Keep this out of LText until we decide if we want it localizable or not
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
#pragma warning disable RCS1187 // Use constant instead of field.
    internal static class ErrorText
    {
        internal static readonly string Ex = "Exception ";
        internal static readonly string Un = "Unable to ";
        internal static readonly string UnOpenLogFile = Un + "open log file.";
        internal static readonly string ScanErrors = "One or more errors occurred while scanning.";
        internal static readonly string UnStartExe = Un + "start executable.";
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
        private static readonly string Ret = "Returning ";
        //internal static readonly string RetT = Ret + "'true'.";
        internal static readonly string RetF = Ret + "'false'.";
    }
#pragma warning restore RCS1187 // Use constant instead of field.
}
