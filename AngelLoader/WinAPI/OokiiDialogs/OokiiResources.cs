using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelLoader.WinAPI.OokiiDialogs
{
    internal static class OokiiResources
    {
        internal const string TaskDialogRunningError = "The task dialog is already being displayed.";
        internal const string TaskDialogNotRunningError = "The task dialog is not currently displayed.";
        internal const string TaskDialogsNotSupportedError = "The operating system does not support task dialogs.";
        internal const string NonCustomTaskDialogButtonIdError = "Cannot change the id for a standard button.";
        internal const string TaskDialogNoButtonsError = "The task dialog must have buttons.";
        internal const string InvalidTaskDialogItemIdError = "The id of a task dialog item must be higher than 0.";
        internal const string TaskDialogEmptyButtonLabelError = "A custom button or radio button cannot have an empty label.";
        internal const string TaskDialogIllegalCrossThreadCallError = "Cross-thread operation not valid: Task dialog accessed from a thread other than the thread it was created on while it is visible.";
        internal const string DuplicateButtonTypeError = "The task dialog already has a non-custom button with the same type.";
        internal const string DuplicateItemIdError = "The task dialog already has an item with the same id.";
        internal const string Preview = "Preview";
        internal const string NoAssociatedTaskDialogError = "The item is not associated with a task dialog.";
        internal const string TaskDialogItemHasOwnerError = "The task dialog item already belongs to another task dialog.";
    }
}
