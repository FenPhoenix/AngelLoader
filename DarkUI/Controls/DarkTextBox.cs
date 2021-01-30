using System.Drawing;
using System.Windows.Forms;
using DarkUI.Config;

namespace DarkUI.Controls
{
    public class DarkTextBox : TextBox, IDarkable
    {
        private bool _origValuesStored;
        private Color _origForeColor;
        private Color _origBackColor;
        private Padding _origPadding;
        private BorderStyle _origBorderStyle;

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                if (_darkModeEnabled)
                {
                    if (!_origValuesStored)
                    {
                        _origForeColor = ForeColor;
                        _origBackColor = BackColor;
                        _origPadding = Padding;
                        _origBorderStyle = BorderStyle;
                        _origValuesStored = true;
                    }

                    BackColor = Colors.LightBackground;
                    ForeColor = Colors.LightText;
                    Padding = new Padding(2, 2, 2, 2);
                    BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    if (_origValuesStored)
                    {
                        BackColor = _origBackColor;
                        ForeColor = _origForeColor;
                        Padding = _origPadding;
                        BorderStyle = _origBorderStyle;
                    }
                }
            }
        }
    }
}
