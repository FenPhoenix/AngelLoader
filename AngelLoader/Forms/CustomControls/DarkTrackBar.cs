using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkTrackBar : TrackBar, IDarkable
{
    [PublicAPI]
    public Color DrawnBackColor = SystemColors.Control;

    [PublicAPI]
    public Color DarkModeDrawnBackColor = DarkColors.Fen_ControlBackground;

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        get => _darkModeEnabled;
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            BackColor = _darkModeEnabled ? DarkModeDrawnBackColor : DrawnBackColor;
        }
    }

    protected override void WndProc(ref Message m)
    {
        // Prevents white flicker when the main window redraws in certain cases (restore from minimize, game
        // window closing, etc.)
        if (m.Msg == Native.WM_ERASEBKGND) return;
        base.WndProc(ref m);
    }
}
