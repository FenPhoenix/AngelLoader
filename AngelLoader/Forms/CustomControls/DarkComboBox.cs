using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Properties;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkComboBox : ComboBox, IDarkable
    {
        private const TextFormatFlags _textFormat =
            TextFormatFlags.Default |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.SingleLine |
            TextFormatFlags.NoClipping;

        private Bitmap? _buffer;

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                SetUpTheme();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [PublicAPI]
        public new Color ForeColor { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [PublicAPI]
        public new Color BackColor { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [PublicAPI]
        public new FlatStyle FlatStyle { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [PublicAPI]
        public new ComboBoxStyle DropDownStyle { get; set; }

        public DarkComboBox() => SetUpTheme();

        private void SetUpTheme()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, _darkModeEnabled);

            if (_darkModeEnabled)
            {
                DrawMode = DrawMode.OwnerDrawVariable;
                base.FlatStyle = FlatStyle.Flat;
            }
            else
            {
                DrawMode = DrawMode.Normal;
                base.FlatStyle = FlatStyle.Standard;
            }

            base.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void InvalidateIfDark()
        {
            if (_darkModeEnabled) Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _buffer = null;

            base.Dispose(disposing);
        }

        protected override void OnTabStopChanged(EventArgs e)
        {
            base.OnTabStopChanged(e);
            InvalidateIfDark();
        }

        protected override void OnTabIndexChanged(EventArgs e)
        {
            base.OnTabIndexChanged(e);
            InvalidateIfDark();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            InvalidateIfDark();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            InvalidateIfDark();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            InvalidateIfDark();
        }

        protected override void OnTextUpdate(EventArgs e)
        {
            base.OnTextUpdate(e);
            InvalidateIfDark();
        }

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            base.OnSelectedValueChanged(e);
            InvalidateIfDark();
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);
            PaintCombobox();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _buffer = null;
            InvalidateIfDark();
        }

        private void PaintCombobox()
        {
            if (!_darkModeEnabled) return;

            _buffer ??= new Bitmap(ClientRectangle.Width, ClientRectangle.Height);

            using var g = Graphics.FromImage(_buffer);
            var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

            var textColor = Enabled ? DarkModeColors.LightText : DarkModeColors.DisabledText;
            var borderColor = DarkModeColors.GreySelection;
            var fillColor = DarkModeColors.LightBackground;

            if (Focused && TabStop) borderColor = DarkModeColors.BlueHighlight;

            using (var b = new SolidBrush(fillColor))
            {
                g.FillRectangle(b, rect);
            }

            using (var p = new Pen(borderColor, 1))
            {
                var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                g.DrawRectangle(p, modRect);
            }

            var icon = Resources.DarkUI_scrollbar_arrow_hot;
            g.DrawImageUnscaled(icon,
                rect.Right - icon.Width - (Consts.Padding / 2),
                (rect.Height / 2) - (icon.Height / 2));

            var text = SelectedItem != null ? SelectedItem.ToString() : Text;

            using (var b = new SolidBrush(textColor))
            {
                const int padding = 2;

                var modRect = new Rectangle(rect.Left + padding,
                    rect.Top + padding,
                    rect.Width - icon.Width - (Consts.Padding / 2) - (padding * 2),
                    rect.Height - (padding * 2));

                // Explicitly set the fill color so that the antialiasing/ClearType looks right
                TextRenderer.DrawText(g, text, Font, modRect, b.Color, fillColor, _textFormat);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!_darkModeEnabled)
            {
                base.OnPaint(e);
                return;
            }

            //if (_buffer == null) PaintCombobox();
            // Just paint it always, because we already check for a null buffer and initialize it in PaintComboBox()
            // and while we exit before then if !_darkModeEnabled, we already will have exited this method anyway
            // if that's the case, so we know dark mode is enabled by the time we get here.
            // Fixes the glitchy drawing bug.
            PaintCombobox();

            e.Graphics.DrawImageUnscaled(_buffer!, Point.Empty);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (!_darkModeEnabled)
            {
                base.OnDrawItem(e);
                return;
            }

            // Otherwise, we draw the text that's supposed to be in the drop-down overtop of the non-dropped-down
            // item, briefly overwriting the text already there but we're bumped over slightly.
            if (!DroppedDown) return;

            var g = e.Graphics;
            var rect = e.Bounds;

            var textColor = DarkModeColors.LightText;
            var fillColor = DarkModeColors.LightBackground;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected ||
                (e.State & DrawItemState.Focus) == DrawItemState.Focus ||
                (e.State & DrawItemState.NoFocusRect) != DrawItemState.NoFocusRect)
            {
                fillColor = DarkModeColors.BlueSelection;
            }

            using (var b = new SolidBrush(fillColor))
            {
                g.FillRectangle(b, rect);
            }

            if (e.Index >= 0 && e.Index < Items.Count)
            {
                var text = Items[e.Index].ToString();

                using var b = new SolidBrush(textColor);
                const int padding = 2;

                var modRect = new Rectangle(rect.Left + padding,
                    rect.Top + padding,
                    rect.Width - (padding * 2),
                    rect.Height - (padding * 2));

                // Explicitly set the fill color so that the antialiasing/ClearType looks right
                TextRenderer.DrawText(g, text, Font, modRect, b.Color, fillColor, _textFormat);
            }
        }
    }
}
