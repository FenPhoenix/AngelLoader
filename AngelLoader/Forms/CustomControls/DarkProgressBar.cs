using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkProgressBar : ProgressBar, IDarkable
    {
        private bool _darkModeEnabled;

        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

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
        }
    }
}
