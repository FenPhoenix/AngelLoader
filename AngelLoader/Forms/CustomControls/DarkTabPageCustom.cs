using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public class DarkTabPageCustom : TabPage, IDarkable
{
    private Color? _origBackColor;

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual bool DarkModeEnabled
    {
        get => _darkModeEnabled;
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (_darkModeEnabled)
            {
                _origBackColor ??= BackColor;
                BackColor = DarkColors.Fen_ControlBackground;
            }
            else
            {
                if (_origBackColor != null) BackColor = (Color)_origBackColor;
            }
        }
    }

    /// <summary>
    /// If this component represents a game in some way, you can set its <see cref="GameSupport.GameIndex"/> here.
    /// </summary>
    [PublicAPI]
    public GameSupport.GameIndex GameIndex = GameSupport.GameIndex.Thief1;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == Native.WM_NCPAINT)
        {
            base.WndProc(ref m);
            ControlUtils.Wine_DrawScrollBarCorner(this);
            return;
        }

        base.WndProc(ref m);
    }
}
