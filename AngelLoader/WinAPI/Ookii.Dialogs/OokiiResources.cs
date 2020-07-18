using System.Diagnostics.CodeAnalysis;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    // static readonly instead of const, to avoid bloat and because these won't even be used unless we have an
    // exception, which we clearly try to not have in the first place
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    internal static class OokiiResources
    {
        internal static readonly string TaskDialogRunningError = "The task dialog is already being displayed.";
        internal static readonly string TaskDialogIllegalCrossThreadCallError = "Cross-thread operation not valid: Task dialog accessed from a thread other than the thread it was created on while it is visible.";
    }
}
