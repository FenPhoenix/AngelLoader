// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.
using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.Dialogs.WinForms
{
    /// <summary>
    /// Represents the state of the progress bar on the task dialog.
    /// </summary>
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
