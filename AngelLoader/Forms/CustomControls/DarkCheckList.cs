using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkCheckList : Panel, IDarkable
    {
        private bool _origValuesStored;
        private Color? _origBackColor;
        private Color? _origForeColor;

        [PublicAPI]
        public new Color BackColor { get; set; } = SystemColors.Control;
        [PublicAPI]
        public new Color ForeColor { get; set; } = SystemColors.ControlText;

        [PublicAPI]
        public Color DarkModeBackColor { get; set; } = DarkColors.Fen_ControlBackground;
        [PublicAPI]
        public Color DarkModeForeColor { get; set; } = DarkColors.LightText;

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

                RefreshDarkMode();
            }
        }

        internal void RefreshDarkMode()
        {
            if (_darkModeEnabled)
            {
                if (!_origValuesStored)
                {
                    _origBackColor = BackColor;
                    _origForeColor = ForeColor;
                    _origValuesStored = true;
                }

                BackColor = DarkModeBackColor;
                ForeColor = DarkModeForeColor;
            }
            else
            {
                if (_origValuesStored)
                {
                    BackColor = (Color)_origBackColor!;
                    ForeColor = (Color)_origForeColor!;
                }
            }

            foreach (Control control in Controls)
            {
                if (control is DarkCheckBox and IDarkable darkableControl)
                {
                    darkableControl.DarkModeEnabled = _darkModeEnabled;
                }
            }
        }
    }
}
