using System.Diagnostics.CodeAnalysis;

namespace AngelLoader.DataClasses
{
    // @Localization(ErrorText): Keep this out of LText until we decide if we want it localizable or not
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    internal static class ErrorText
    {
        internal static readonly string UnableToOpenLogFile = "Unable to open log file.";
        internal static readonly string ScanErrors = "One or more errors occurred while scanning.";
        internal static readonly string UnableToStartExecutable = "Unable to start executable.";
        internal static readonly string UnableToOpenFMFolder = "Unable to open FM folder.";
        internal static readonly string UnableToOpenHTMLReadme = "Unable to open HTML readme.";
        internal static readonly string HTMLReadmeNotFound = "The HTML readme file could not be found.";
        internal static readonly string UnableToOpenLink = "Unable to open link.";
    }
}
