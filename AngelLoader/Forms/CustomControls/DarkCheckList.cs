using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkCheckList : Panel, IDarkable
{
#if DEBUG
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Color BackColor
    {
        get => base.BackColor;
        set => base.BackColor = value;
    }
#endif

    public DarkCheckList() => BackColor = SystemColors.Window;

    #region Public fields and properties

    internal bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            RefreshDarkMode();
        }
    }

    #endregion

    #region Private methods

    internal void RefreshDarkMode()
    {
        if (_darkModeEnabled)
        {
            BackColor = DarkColors.Fen_ControlBackground;
            ForeColor = DarkColors.LightText;
        }
        else
        {
            BackColor = SystemColors.Window;
            ForeColor = SystemColors.WindowText;
        }

        foreach (Control control in Controls)
        {
            if (control is IDarkable darkableControl)
            {
                darkableControl.DarkModeEnabled = _darkModeEnabled;
            }
        }
    }

    #endregion
}
