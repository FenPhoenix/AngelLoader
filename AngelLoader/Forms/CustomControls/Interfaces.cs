using System;
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

    public interface ISuspendResumable
    {
        bool IsHandleCreated { get; }
        bool Visible { get; }
        IntPtr Handle { get; }
        void Refresh();
        bool Suspended { get; set; }
    }
}
