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
        Point PointToScreen(Point p);
        Control Parent { get; }
        Point Location { get; set; }
        Size Size { get; set; }
        new bool Visible { get; set; }
        event EventHandler<DarkModeChangedEventArgs> DarkModeChanged;
        event EventHandler VisibilityChanged;
    }

    public interface ISuspendResumable
    {
        bool IsHandleCreated { get; }
        bool Visible { get; set; }
        IntPtr Handle { get; }
        void Refresh();
        bool Suspended { get; set; }
    }
}
