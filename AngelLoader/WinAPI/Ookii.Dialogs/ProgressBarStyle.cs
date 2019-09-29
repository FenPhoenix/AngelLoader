// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.

using JetBrains.Annotations;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    /// <summary>
    /// Indicates the type of progress on a task dialog.
    /// </summary>
    [PublicAPI]
    public enum ProgressBarStyle
    {
        /// <summary>
        /// No progress bar is displayed on the dialog.
        /// </summary>
        None,
        /// <summary>
        /// A regular progress bar is displayed on the dialog.
        /// </summary>
        ProgressBar,
        /// <summary>
        /// A marquee progress bar is displayed on the dialog. Use this value for operations
        /// that cannot report concrete progress information.
        /// </summary>
        MarqueeProgressBar
    }
}
