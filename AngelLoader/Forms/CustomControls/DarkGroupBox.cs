using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkGroupBox : GroupBox, IDarkable
    {
        private Color _borderColor = DarkColors.LightBorder;

        [Category("Appearance")]
        [Description("Determines the color of the border.")]
        [PublicAPI]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                if (_darkModeEnabled) Invalidate();
            }
        }

        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
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

        private void SetUpTheme()
        {
            // Original:

            // ControlStyles.OptimizedDoubleBuffer == false
            // ControlStyles.ResizeRedraw == true
            // ControlStyles.UserPaint == true
            // ResizeRedraw == true
            // DoubleBuffered == false

            if (_darkModeEnabled)
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);

                ResizeRedraw = true;
                DoubleBuffered = true;
            }
            else
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
                SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
                ResizeRedraw = true;
                DoubleBuffered = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!_darkModeEnabled)
            {
                base.OnPaint(e);
                return;
            }

            var g = e.Graphics;
            var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
            var stringSize = g.MeasureString(Text, Font);

            g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, rect);

            using (var p = new Pen(BorderColor, 1))
            {
                var borderRect = new Rectangle(
                    0,
                    (int)stringSize.Height / 2,
                    rect.Width - 1,
                    rect.Height - ((int)stringSize.Height / 2) - 1);
                g.DrawRectangle(p, borderRect);
            }

            var textRect = new Rectangle(
                rect.Left + Consts.Padding,
                rect.Top,
                rect.Width - (Consts.Padding * 2),
                (int)stringSize.Height);

            var modRect = new Rectangle(textRect.Left, textRect.Top, Math.Min(textRect.Width, (int)stringSize.Width), textRect.Height);
            g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, modRect);

            // No TextAlign property, so leave constant
            const TextFormatFlags textFormatFlags =
                TextFormatFlags.Default |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoClipping |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.SingleLine;

            Color textColor = Enabled ? DarkColors.LightText : DarkColors.DisabledText;
            TextRenderer.DrawText(g, Text, Font, textRect, textColor, textFormatFlags);
        }
    }
}
