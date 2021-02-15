using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using DarkUI.Controls;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed class ToolStripCustom : ToolStrip, IDarkable
    {
        private readonly SolidBrush _darkBackColorBrush = new SolidBrush(DarkUI.Config.Colors.Fen_ControlBackground);
        private readonly Pen _darkBackColorPen = new Pen(DarkUI.Config.Colors.Fen_ControlBackground);

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                BackColor = _darkModeEnabled ? DarkUI.Config.Colors.Fen_ControlBackground : SystemColors.Control;
                Refresh();
            }
        }

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
            var rectLeft = new Rectangle(0, 0, Items[0].Bounds.X - Items[0].Margin.Left, Height);
            var last = Items[Items.Count - 1];
            int rect2Start = last.Bounds.X + last.Bounds.Width + last.Margin.Right;
            var rectRight = new Rectangle(rect2Start - PaddingDrawNudge, 0, (Width - rect2Start) + PaddingDrawNudge, Height);
            var rectBottom = new Rectangle(0, Height - 1, Width, 1);

            var brush = _darkModeEnabled ? _darkBackColorBrush : SystemBrushes.Control;
            var pen = _darkModeEnabled ? _darkBackColorPen : SystemPens.Control;

            e.Graphics.FillRectangle(brush, rectLeft);
            e.Graphics.FillRectangle(brush, rectRight);
            e.Graphics.FillRectangle(brush, rectBottom);
            e.Graphics.DrawLine(pen, Width - 2, Height - 2, Width - 1, Height - 2);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _darkBackColorBrush.Dispose();
                _darkBackColorPen.Dispose();
            }
            base.Dispose(disposing);
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

    /// <summary>
    /// Because the text will be displayed as "One &amp; Two" but will still be stored as "One &amp;&amp; Two"
    /// </summary>
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public sealed class ToolStripMenuItemWithBackingText : ToolStripMenuItemCustom
    {
        public string BackingText { get; }

        public ToolStripMenuItemWithBackingText(string text)
        {
            BackingText = text;
            Text = text;
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public sealed class ToolStripButtonCustom : ToolStripButton, IDarkable
    {
        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                BackColor = _darkModeEnabled ? DarkUI.Config.Colors.Fen_DarkBackground : SystemColors.Control;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Use the mouseover BackColor when it's checked, for a more visible checked experience
            if (Checked) e.Graphics.FillRectangle(Brushes.LightSkyBlue, 0, 0, Width, Height);
            base.OnPaint(e);
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public sealed class ToolStripArrowButton : ToolStripButton, IDarkable
    {
        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                BackColor = _darkModeEnabled ? DarkUI.Config.Colors.Fen_DarkBackground : SystemColors.Control;
            }
        }

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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ControlPainter.PaintArrow(e, _arrowPolygon, _arrowDirection, Width, Height, Enabled);
        }
    }
}
