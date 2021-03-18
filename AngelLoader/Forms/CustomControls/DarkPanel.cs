using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkPanel : Panel, IDarkable
    {
        private Color? _origForeColor;
        private Color? _origBackColor;

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

                if (_darkModeEnabled)
                {
                    _origForeColor ??= SystemColors.ControlText;
                    _origBackColor ??= SystemColors.Control;

                    ForeColor = DarkColors.LightText;
                    BackColor = DarkColors.Fen_ControlBackground;
                }
                else
                {
                    if (_origForeColor != null) ForeColor = (Color)_origForeColor;
                    if (_origBackColor != null) BackColor = (Color)_origBackColor;
                }
            }
        }
    }
}
