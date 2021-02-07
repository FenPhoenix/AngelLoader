using System;
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

    public interface IDarkableScrollableNative : IDarkable
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
    }
}
