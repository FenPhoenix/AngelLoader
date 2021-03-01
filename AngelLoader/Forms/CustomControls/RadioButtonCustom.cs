using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class RadioButtonCustom : DarkButton
    {
        private bool _checked;

        public event EventHandler? CheckedChanged;

        public override bool DarkModeEnabled
        {
            get => base.DarkModeEnabled;
            set
            {
                base.DarkModeEnabled = value;
                SetCheckedVisualState();
            }
        }

        private void SetCheckedVisualState()
        {
            DarkModeBackColor =
                _checked
                    ? DarkColors.Fen_DarkBackground
                    : DarkColors.Fen_ControlBackground;

            if (!DarkModeEnabled)
            {
                BackColor = _checked
                    ? SystemColors.Window
                    : SystemColors.Control;
            }
            // Needed to prevent background color sticking when unchecked sometimes
            Refresh();
        }

        [Browsable(true)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;

                _checked = value;
                SetCheckedVisualState();

                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = true;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var rect = new Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            e.Graphics.DrawRectangle(DarkModeEnabled ? DarkColors.LightTextPen : SystemPens.ControlText, rect);
        }
    }
}
