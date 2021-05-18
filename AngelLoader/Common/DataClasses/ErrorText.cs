using System.Diagnostics.CodeAnalysis;

namespace AngelLoader.DataClasses
{
    // @Localization(ErrorText): Keep this out of LText until we decide if we want it localizable or not
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    internal static class ErrorText
    {
        internal static readonly string UnableToOpenLogFile = "Unable to open log file.";
        internal static readonly string ScanErrors = "One or more errors occurred while scanning.";
    }
}
