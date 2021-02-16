using System;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public interface IDarkable
    {
        bool DarkModeEnabled { get; set; }
    }

    public interface IDarkableScrollable : IDarkable
    {
        ScrollBarVisualOnly VerticalVisualScrollBar { get; }
        ScrollBarVisualOnly HorizontalVisualScrollBar { get; }
        ScrollBar VerticalScrollBar { get; }
        ScrollBar HorizontalScrollBar { get; }
    }

    public interface IDarkableScrollableNative : IDarkable, ISuspendResumable
    {
        new bool IsHandleCreated { get; }
        new IntPtr Handle { get; }
        event EventHandler Scroll;
        Control Parent { get; }
        Control ClosestAddableParent { get; }
        Point Location { get; }
        Size Size { get; }
        new bool Visible { get; }
        event EventHandler<DarkModeChangedEventArgs> DarkModeChanged;
        event EventHandler VisibilityChanged;
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
