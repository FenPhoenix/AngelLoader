﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public interface IDarkable
    {
        bool DarkModeEnabled { get; set; }
    }

    public interface IDarkableScrollable : IDarkable
    {
        ScrollBarVisualOnly? VerticalVisualScrollBar { get; }
        ScrollBarVisualOnly? HorizontalVisualScrollBar { get; }
        ScrollBar VerticalScrollBar { get; }
        ScrollBar HorizontalScrollBar { get; }
    }

    public interface IDarkableScrollableNative : IDarkable, ISuspendResumable
    {
        ScrollBarVisualOnly_Native? VerticalVisualScrollBar { get; }
        ScrollBarVisualOnly_Native? HorizontalVisualScrollBar { get; }
        ScrollBarVisualOnly_Corner? VisualScrollBarCorner { get; }
        new bool IsHandleCreated { get; }
        new IntPtr Handle { get; }
        event EventHandler? Scroll;
        Control? Parent { get; }
        Point Location { get; }
        Size ClientSize { get; }
        Size Size { get; }
        bool Enabled { get; }
        new bool Visible { get; }
        event EventHandler? DarkModeChanged;
        event EventHandler? RefreshIfNeededForceCorner;
    }

    public interface ISuspendResumable
    {
        bool IsHandleCreated { get; }
        bool Visible { get; }
        IntPtr Handle { get; }
        void Refresh();
        bool Suspended { get; set; }
    }
}
