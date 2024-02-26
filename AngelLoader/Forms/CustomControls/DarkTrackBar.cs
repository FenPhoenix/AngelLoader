using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

// @ScreenshotDisplay: TrackBar white flickering on game close
// TrackBars (even stock ones) seem to have this white-flicker problem on game close (I guess when a game window
// disappears and the app window is revealed?). See if we can figure out a fix or workaround.
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
}
