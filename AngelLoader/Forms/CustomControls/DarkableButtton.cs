using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkableButton : Button
    {
        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                if (_darkModeEnabled)
                {
                    UseVisualStyleBackColor = false;
                    FlatStyle = FlatStyle.Flat;
                    SetDarkModeForeColor();
                    BackColor = DarkUI.Config.Colors.MediumBackground;
                    FlatAppearance.BorderColor = DarkUI.Config.Colors.GreyHighlight;
                    FlatAppearance.MouseOverBackColor = DarkUI.Config.Colors.BlueBackground;
                    FlatAppearance.MouseDownBackColor = DarkUI.Config.Colors.DarkBackground;
                }
                else
                {
                    //UseVisualStyleBackColor = true;
                    //FlatStyle = FlatStyle.Standard;
                    //FlatAppearance.BorderColor = SystemColors.ActiveBorder;
                    //FlatAppearance.MouseOverBackColor = Color.Empty;
                    //FlatAppearance.MouseDownBackColor = Color.Empty;
                    ForeColor = SystemColors.ControlText;
                    BackColor = SystemColors.Control;
                    UseVisualStyleBackColor = true;
                    FlatStyle = FlatStyle.Standard;
                }
            }
        }

        private void SetDarkModeForeColor()
        {
            if (_darkModeEnabled)
            {
                ForeColor = Enabled
                    ? DarkUI.Config.Colors.LightText
                    //: Color.Red;
                    : DarkUI.Config.Colors.DisabledText;
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            if (_darkModeEnabled)
            {
                FlatAppearance.BorderColor = DarkUI.Config.Colors.BlueHighlight;
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            if (_darkModeEnabled)
            {
                FlatAppearance.BorderColor = DarkUI.Config.Colors.GreyHighlight;
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            SetDarkModeForeColor();
            if (_darkModeEnabled)
            {
                BackColor = Enabled
                    ? DarkUI.Config.Colors.MediumBackground
                    : SystemColors.Window;
            }
            base.OnEnabledChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //SetDarkModeForeColor();
            base.OnPaint(e);
        }
    }
}
