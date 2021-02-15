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
        ScrollBarVisualOnly_Native VerticalVisualScrollBar { get; }
        ScrollBarVisualOnly_Native HorizontalVisualScrollBar { get; }
        bool IsHandleCreated { get; }
        IntPtr Handle { get; }
        event EventHandler Scroll;
        event PaintEventHandler Paint;
        bool VScrollVisible { get; }
        bool HScrollVisible { get; }
        void AddToControls(ScrollBarVisualOnly_Native visualScrollBar);
        event EventHandler VScroll;
        event EventHandler HScroll;
        Point VScrollLocation { get; }
        Point HScrollLocation { get; }
        Size VScrollSize { get; }
        Size HScrollSize { get; }
        Point PointToClient(Point p);
        Point PointToScreen(Point p);
        Control Parent { get; }
        Point Location { get; set; }
        Size Size { get; set; }
        bool Visible { get; set; }
        event EventHandler ClientSizeChanged;
        event EventHandler<DarkModeChangedEventArgs> DarkModeChanged;
        event EventHandler VisibilityChanged;
    }

    public interface ISuspendResumable
    {
        Control[] ConceptualChildControls { get; }
        bool IsHandleCreated { get; }
        bool Visible { get; set; }
        IntPtr Handle { get; }
        void Refresh();
    }
}
