// NULL_TODO
#nullable disable

// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.
using System;
using JetBrains.Annotations;

namespace AngelLoader.WinAPI.Ookii.Dialogs
{
    /// <summary>
    /// Class that provides data for the <see cref="TaskDialog.HyperlinkClicked"/> event.
    /// </summary>
    /// <threadsafety instance="false" static="true" />
    [PublicAPI]
    public class HyperlinkClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the <see cref="HyperlinkClickedEventArgs"/> class with the specified URL.
        /// </summary>
        /// <param name="href">The URL of the hyperlink.</param>
        public HyperlinkClickedEventArgs(string href) => Href = href;

        /// <summary>
        /// Gets the URL of the hyperlink that was clicked.
        /// </summary>
        /// <value>
        /// The value of the href attribute of the hyperlink.
        /// </value>
        public string Href { get; }
    }
}
