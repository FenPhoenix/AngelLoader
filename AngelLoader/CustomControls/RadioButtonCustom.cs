using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.CustomControls
{
    public class RadioButtonCustom : Button
    {
        private bool _checked;

        public event EventHandler CheckedChanged;

        [Browsable(true)]
        public bool Checked
        {
            get => _checked;
            set
            {
                _checked = value;
                // TODO: Only do the stuff if the checked state has actually changed
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
            var rect = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
            e.Graphics.DrawRectangle(new Pen(Color.Black, 1.0f), rect);
        }
    }
}
