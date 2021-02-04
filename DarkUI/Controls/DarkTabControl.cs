using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public class DarkTabControl : TabControl, IDarkable
    {
        public DarkTabControl() { }

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                SetUpTheme();
            }
        }

        protected void SetUpTheme()
        {
            SetStyle(ControlStyles.UserPaint, _darkModeEnabled);

            if (_darkModeEnabled)
            {

            }
            else
            {

            }
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_darkModeEnabled)
            {
                // Test/proof of concept
                using (var b = new SolidBrush(Config.Colors.Fen_DarkBackground))
                {
                    e.Graphics.FillRectangle(b, ClientRectangle);
                }

                Brush[] brushes =
                {
                    Brushes.Red,
                    Brushes.Blue,
                    Brushes.White,
                    Brushes.Orange,
                    Brushes.Purple
                };

                for (int i = 0; i < TabPages.Count; i++)
                {
                    TabPage tabPage = TabPages[i];
                    Rectangle tabRect = GetTabRect(i);

                    e.Graphics.FillRectangle(brushes[i], tabRect);
                }
            }

            base.OnPaint(e);
        }
    }
}
