using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace Update;

public sealed class DarkProgressBar : ProgressBar, IDarkable
{
    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            RefreshDarkModeState();
        }
    }

    private void RefreshDarkModeState()
    {
        if (_darkModeEnabled)
        {
            Native.SetWindowTheme(Handle, "", "");
            BackColor = DarkColors.Fen_ControlBackground;
            ForeColor = DarkColors.BlueHighlight;
        }
        else
        {
            // I can't get SetWindowTheme() to work for resetting the theme back to normal, but recreating
            // the handle does the job.
            RecreateHandle();
        }

        Invalidate();
    }

    [PublicAPI]
    public new ProgressBarStyle Style
    {
        get => base.Style;
        set
        {
            base.Style = value;
            // Changing style reverts us to classic mode, so reset us back to dark if necessary
            if (_darkModeEnabled) RefreshDarkModeState();
        }
    }

    /// <summary>
    /// Sets the progress bar's value instantly. Avoids the la-dee-dah catch-up-when-I-feel-like-it nature of
    /// the progress bar that makes it look annoying and unprofessional.
    /// </summary>
    [PublicAPI]
    public new int Value
    {
        get => base.Value;
        set
        {
            value = value.Clamp(Minimum, Maximum);

            if (_darkModeEnabled)
            {
                // In dark mode we don't animate, so we don't have to fudge around the value to make it instant
                base.Value = value;
            }
            else
            {
                // ... but in classic mode, we do and we do
                if (value == Maximum)
                {
                    base.Value = Maximum;
                }
                else
                {
                    base.Value = (value + 1).Clamp(Minimum, Maximum);
                    base.Value = value;
                }
            }
        }
    }
}
