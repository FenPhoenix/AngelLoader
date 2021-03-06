﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkGroupBox : GroupBox, IDarkable
    {
        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

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
        }

        private const int _padding = 10;

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

            var borderRect = new Rectangle(
                0,
                (int)stringSize.Height / 2,
                rect.Width - 1,
                rect.Height - ((int)stringSize.Height / 2) - 1);
            g.DrawRectangle(DarkColors.LighterBorderPen, borderRect);

            var textRect = new Rectangle(
                rect.Left + _padding,
                rect.Top,
                rect.Width - (_padding * 2),
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
