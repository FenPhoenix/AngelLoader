using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
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

        private readonly Point[] _arrowPoints = new Point[3];

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
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

            var textColorBrush = Enabled ? DarkColors.LightTextBrush : DarkColors.DisabledTextBrush;
            var borderPen = DarkColors.GreySelectionPen;
            var fillColorBrush = DarkColors.LightBackgroundBrush;

            if (Focused && TabStop) borderPen = DarkColors.BlueHighlightPen;

            g.FillRectangle(fillColorBrush, rect);

            {
                var borderRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                g.DrawRectangle(borderPen, borderRect);
            }

            const int arrowWidth = 9;
            const int arrowHeight = 4;

            int x = rect.Width - arrowWidth - (Consts.Padding / 2);
            int y = (rect.Height / 2) - (arrowHeight / 2);

            _arrowPoints[0] = new Point(x, y);
            _arrowPoints[1] = new Point(x + 4, y + 5);
            _arrowPoints[2] = new Point(x + 9, y);

            g.FillPolygon(DarkColors.GreyHighlightBrush, _arrowPoints);

            var text = SelectedItem != null ? SelectedItem.ToString() : Text;

            const int padding = 2;

            var textRect = new Rectangle(rect.Left + padding,
                rect.Top + padding,
                rect.Width - arrowWidth - (Consts.Padding / 2) - (padding * 2),
                rect.Height - (padding * 2));

            // Explicitly set the fill color so that the antialiasing/ClearType looks right
            TextRenderer.DrawText(g, text, Font, textRect, textColorBrush.Color, fillColorBrush.Color, _textFormat);
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

            var textColorBrush = DarkColors.LightTextBrush;
            var fillColorBrush = DarkColors.LightBackgroundBrush;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected ||
                (e.State & DrawItemState.Focus) == DrawItemState.Focus ||
                (e.State & DrawItemState.NoFocusRect) != DrawItemState.NoFocusRect)
            {
                fillColorBrush = DarkColors.BlueSelectionBrush;
            }

            g.FillRectangle(fillColorBrush, rect);

            if (e.Index >= 0 && e.Index < Items.Count)
            {
                var text = Items[e.Index].ToString();

                const int padding = 2;

                var textRect = new Rectangle(rect.Left + padding,
                    rect.Top + padding,
                    rect.Width - (padding * 2),
                    rect.Height - (padding * 2));

                // Explicitly set the fill color so that the antialiasing/ClearType looks right
                TextRenderer.DrawText(g, text, Font, textRect, textColorBrush.Color, fillColorBrush.Color, _textFormat);
            }
        }
    }
}
