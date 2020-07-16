// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.

using JetBrains.Annotations;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    /// <summary>
    /// Represents the state of the progress bar on the task dialog.
    /// </summary>
    [PublicAPI]
    public enum ProgressBarState
    {
        /// <summary>
        /// Normal state.
        /// </summary>
        Normal,
        /// <summary>
        /// Error state
        /// </summary>
        Error,
        /// <summary>
        /// Paused state
        /// </summary>
        Paused
    }
}
