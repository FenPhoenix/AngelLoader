// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    public sealed class TaskDialogButton
    {
        internal readonly string Text = "";
        internal readonly ButtonType ButtonType;

        /// <summary>
        /// This is not meant to be set manually. If you set it before calling the <see cref="TaskDialog"/>
        /// constructor, it will be overwritten. If you set it after, you'll screw everything up colossally.
        /// Don't do it!
        /// </summary>
        // Okay, now I see why dude did the whole dependency injection backwards-seeming thing... It's so the
        // button could set its own id and not expose it externally. I think that's probably it anyway. That's
        // a fair enough reason for a public API I guess.
        internal int Id;

        public TaskDialogButton(ButtonType type) => ButtonType = type;

        public TaskDialogButton(string text) => Text = text;

        internal NativeMethods.TaskDialogCommonButtonFlags ButtonFlag => ButtonType switch
        {
            ButtonType.Ok => NativeMethods.TaskDialogCommonButtonFlags.OkButton,
            ButtonType.Yes => NativeMethods.TaskDialogCommonButtonFlags.YesButton,
            ButtonType.No => NativeMethods.TaskDialogCommonButtonFlags.NoButton,
            ButtonType.Cancel => NativeMethods.TaskDialogCommonButtonFlags.CancelButton,
            ButtonType.Retry => NativeMethods.TaskDialogCommonButtonFlags.RetryButton,
            ButtonType.Close => NativeMethods.TaskDialogCommonButtonFlags.CloseButton,
            _ => 0
        };
    }
}
