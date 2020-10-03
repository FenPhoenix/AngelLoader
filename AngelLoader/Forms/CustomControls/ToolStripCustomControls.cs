﻿using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed class ToolStripCustom : ToolStrip
    {
        /// <summary>
        /// Fiddle this around to get the right-side garbage line to disappear again when Padding is set to something.
        /// </summary>
        [Browsable(true)]
        [PublicAPI]
        public int PaddingDrawNudge { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Hack in order to be able to have ManagerRenderMode, but also get rid of any garbage around the
            // edges that may be drawn. In particular, there's an ugly visual-styled vertical line at the right
            // side if you don't do this.
            // Take margin into account to allow drawing past the left side of the first item or the right of the
            // last
            var rect1 = new Rectangle(0, 0, Items[0].Bounds.X - Items[0].Margin.Left, Height);
            var last = Items[Items.Count - 1];
            int rect2Start = last.Bounds.X + last.Bounds.Width + last.Margin.Right;
            var rect2 = new Rectangle(rect2Start - PaddingDrawNudge, 0, (Width - rect2Start) + PaddingDrawNudge, Height);

            e.Graphics.FillRectangle(SystemBrushes.Control, rect1);
            e.Graphics.FillRectangle(SystemBrushes.Control, rect2);
        }
    }

    /// <summary>
    /// Tool strip menu item but with automatic ampersand escaping, because it's hell to make sure they all get escaped otherwise.
    /// </summary>
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public class ToolStripMenuItemCustom : ToolStripMenuItem
    {
        public ToolStripMenuItemCustom() { }

        /// <summary>Set text with escaped ampersands.</summary>
        /// <param name="text">The text to display on the menu item, with escaped ampersands.</param>
        // Call it with bare text because the constructor will set Text, which will jump back up to setting it
        // here (which will escape ampersands), then jump back down to setting it in the base class. OOP IS THE
        // LITERAL DEFINITION OF SPAGHETTI CODE.
        // But we do it anyway to try and reduce our own errors of forgetting to call the ampersand escaper every
        // time we set text one way or the other...
        public ToolStripMenuItemCustom(string text) : base(text) { }

        /// <summary>
        /// Sets the text and escapes ampersands.
        /// </summary>
        public override string Text
        {
            get => base.Text;
            set => base.Text = value.EscapeAmpersands();
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public sealed class ToolStripMenuItemWithBackingText : ToolStripMenuItemCustom
    {
        private string _backingText;
        public string BackingText => _backingText;

        public ToolStripMenuItemWithBackingText(string text)
        {
            _backingText = text;
            base.Text = text;
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public sealed class ToolStripButtonCustom : ToolStripButton
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            // Use the mouseover BackColor when it's checked, for a more visible checked experience
            if (Checked) e.Graphics.FillRectangle(Brushes.LightSkyBlue, 0, 0, Width, Height);
            base.OnPaint(e);
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public sealed class ToolStripArrowButton : ToolStripButton
    {
        private Direction _arrowDirection;
        private readonly Point[] _arrowPolygon = new Point[3];

        // Public for the designer
        [Browsable(true)]
        [PublicAPI]
        public Direction ArrowDirection
        {
            get => _arrowDirection;
            set
            {
                _arrowDirection = value;
                Invalidate();
            }
        }

        // TODO: This is a straight-up duplicate of the regular Button version...
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int arrowX;
            int arrowY;

            switch (ArrowDirection)
            {
                case Direction.Left:
                    arrowX = (Width / 2) + 2;
                    arrowY = (Height / 2) - 4;

                    (_arrowPolygon[0].X, _arrowPolygon[0].Y) = (arrowX, arrowY - 1);
                    (_arrowPolygon[1].X, _arrowPolygon[1].Y) = (arrowX, arrowY + 7);
                    (_arrowPolygon[2].X, _arrowPolygon[2].Y) = (arrowX - 4, arrowY + 3);

                    break;
                case Direction.Right:
                    arrowX = (Width / 2) - 2;
                    arrowY = (Height / 2) - 4;

                    (_arrowPolygon[0].X, _arrowPolygon[0].Y) = (arrowX, arrowY - 1);
                    (_arrowPolygon[1].X, _arrowPolygon[1].Y) = (arrowX, arrowY + 7);
                    (_arrowPolygon[2].X, _arrowPolygon[2].Y) = (arrowX + 4, arrowY + 3);

                    break;
                case Direction.Up:
                    arrowX = (Width / 2) - 3;
                    arrowY = (Height / 2) + 1;

                    (_arrowPolygon[0].X, _arrowPolygon[0].Y) = (arrowX, arrowY);
                    (_arrowPolygon[1].X, _arrowPolygon[1].Y) = (arrowX + 7, arrowY);
                    (_arrowPolygon[2].X, _arrowPolygon[2].Y) = (arrowX + 3, arrowY - 4);

                    break;
                case Direction.Down:
                default:
                    arrowX = (Width / 2) - 3;
                    arrowY = (Height / 2) - 1;

                    (_arrowPolygon[0].X, _arrowPolygon[0].Y) = (arrowX, arrowY);
                    (_arrowPolygon[1].X, _arrowPolygon[1].Y) = (arrowX + 7, arrowY);
                    (_arrowPolygon[2].X, _arrowPolygon[2].Y) = (arrowX + 3, arrowY + 4);

                    break;
            }

            Brush brush = Enabled ? SystemBrushes.ControlText : SystemBrushes.ControlDark;
            e.Graphics.FillPolygon(brush, _arrowPolygon);
        }
    }
}
