using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.CustomControls
{
    public class RadioButtonCustom : Button
    {
        private bool _checked;

        // @R#_FALSE_POSITIVE?: It doesn't make sense to call event handlers "nullable" does it?
        public event EventHandler? CheckedChanged;

        [Browsable(true)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;

                _checked = value;
                BackColor = _checked ? SystemColors.Window : SystemColors.Control;
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
            e.Graphics.DrawRectangle(new Pen(Color.Black, 1.0f), rect);
        }
    }
}
