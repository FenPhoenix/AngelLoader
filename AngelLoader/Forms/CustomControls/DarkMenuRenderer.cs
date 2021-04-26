﻿using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkMenuRenderer : ToolStripRenderer
    {
        #region Initialisation Region

        protected override void Initialize(ToolStrip toolStrip)
        {
            base.Initialize(toolStrip);

            toolStrip.BackColor = DarkColors.Fen_ControlBackground;
            toolStrip.ForeColor = DarkColors.LightText;
        }

        protected override void InitializeItem(ToolStripItem item)
        {
            base.InitializeItem(item);

            item.BackColor = DarkColors.Fen_ControlBackground;
            item.ForeColor = DarkColors.LightText;

            if (item.GetType() == typeof(ToolStripSeparator))
            {
                item.Margin = new Padding(0, 0, 0, 1);
            }
        }

        #endregion

        #region Render Region

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            e.Graphics.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, e.AffectedBounds);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            var rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            e.Graphics.DrawRectangle(DarkColors.LightBorderPen, rect);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var g = e.Graphics;

            var rect = new Rectangle(
                e.ImageRectangle.Left - 2,
                e.ImageRectangle.Top - 2,
                e.ImageRectangle.Width + 4,
                e.ImageRectangle.Height + 4
            );

            g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, rect);

            var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
            g.DrawRectangle(DarkColors.BlueHighlightPen, modRect);

            if (e.Item.ImageIndex == -1 && string.IsNullOrEmpty(e.Item.ImageKey) && e.Item.Image == null)
            {
                // Match Win10 light mode checkmark shape exactly
                int left = e.ImageRectangle.Left;
                int top = e.ImageRectangle.Top;
                int x, y;
                for (x = left + 5, y = top + 7; x < left + 7; x++, y++)
                {
                    g.DrawLine(DarkColors.LightTextPen, x, y, x, y + 1);
                }
                for (; x < left + 12; x++, y--)
                {
                    g.DrawLine(DarkColors.LightTextPen, x, y, x, y + 1);
                }
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            var rect = new Rectangle(1, 3, e.Item.Width, 1);
            e.Graphics.FillRectangle(DarkColors.LightBorderBrush, rect);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = e.Item.Enabled ? DarkColors.LightText : DarkColors.DisabledText;
            e.ArrowRectangle = new Rectangle(new Point(e.ArrowRectangle.Left, e.ArrowRectangle.Top - 1), e.ArrowRectangle.Size);

            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            Color foreColor = e.Item.Enabled ? e.Item.Selected ? DarkColors.Fen_HighlightText : e.TextColor : DarkColors.DisabledText;
            TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont, e.TextRectangle, foreColor, e.TextFormat);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var g = e.Graphics;

            e.Item.ForeColor = e.Item.Enabled ? DarkColors.LightText : DarkColors.DisabledText;

            if (e.Item.Enabled)
            {
                // Normal item
                var rect = new Rectangle(2, 0, e.Item.Width - 3, e.Item.Height);

                if (!e.Item.Selected)
                {
                    using var b = new SolidBrush(e.Item.BackColor);
                    g.FillRectangle(b, rect);
                }
                else
                {
                    g.FillRectangle(DarkColors.Fen_DGVColumnHeaderPressedBrush, rect);
                }

                // @DarkModeNote: What is this anyway? "Header item on open menu"? This never gets hit as far as I can tell...
                // Header item on open menu
                if (e.Item is ToolStripMenuItem tsMenuItem &&
                   tsMenuItem.DropDown.Visible &&
                   !tsMenuItem.IsOnDropDown)
                {
                    g.FillRectangle(DarkColors.Fen_DGVColumnHeaderPressedBrush, rect);
                }
            }
        }

        #endregion
    }
}
