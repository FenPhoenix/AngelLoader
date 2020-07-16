using System.Diagnostics.CodeAnalysis;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    // static readonly instead of const, to avoid bloat and because these won't even be used unless we have an
    // exception, which we clearly try to not have in the first place
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    internal static class OokiiResources
    {
        internal static readonly string TaskDialogRunningError = "The task dialog is already being displayed.";
        internal static readonly string TaskDialogNotRunningError = "The task dialog is not currently displayed.";
        internal static readonly string TaskDialogsNotSupportedError = "The operating system does not support task dialogs.";
        internal static readonly string NonCustomTaskDialogButtonIdError = "Cannot change the id for a standard button.";
        internal static readonly string TaskDialogNoButtonsError = "The task dialog must have buttons.";
        internal static readonly string InvalidTaskDialogItemIdError = "The id of a task dialog item must be higher than 0.";
        internal static readonly string TaskDialogEmptyButtonLabelError = "A custom button or radio button cannot have an empty label.";
        internal static readonly string TaskDialogIllegalCrossThreadCallError = "Cross-thread operation not valid: Task dialog accessed from a thread other than the thread it was created on while it is visible.";
        internal static readonly string DuplicateButtonTypeError = "The task dialog already has a non-custom button with the same type.";
        internal static readonly string DuplicateItemIdError = "The task dialog already has an item with the same id.";
        internal static readonly string Preview = "Preview";
        internal static readonly string NoAssociatedTaskDialogError = "The item is not associated with a task dialog.";
        internal static readonly string TaskDialogItemHasOwnerError = "The task dialog item already belongs to another task dialog.";
    }
}
