using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkListBox : ListBox, IDarkableScrollableNative
    {
        private BorderStyle? _origBorderStyle;

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                SetUpTheme();
            }
        }

        public DarkListBox()
        {
            // TODO: @DarkMode(DarkListBox): Set up scroll bars as usual.
            //VerticalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: true, passMouseWheel: true);
            //HorizontalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: false, passMouseWheel: true);
            //VisualScrollBarCorner = new ScrollBarVisualOnly_Corner(this);
        }

        private void SetUpTheme()
        {
            if (_darkModeEnabled)
            {
                SetStyle(
                    ControlStyles.Opaque |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, true);

                DrawMode = DrawMode.OwnerDrawFixed;
                //_origBorderStyle ??= BorderStyle;
                //BorderStyle = BorderStyle.None;
                BackColor = DarkColors.Fen_ControlBackground;
                ForeColor = DarkColors.Fen_DarkForeground;
            }
            else
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(
                    ControlStyles.Opaque |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, false);

                DrawMode = DrawMode.Normal;
                //if (_origBorderStyle != null) BorderStyle = (BorderStyle)_origBorderStyle;
                BackColor = SystemColors.Window;
                ForeColor = SystemColors.WindowText;
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

            var itemsToDraw = new List<DrawItemEventArgs>(Items.Count);

            if (Items.Count > 0)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var itemRect = GetItemRectangle(i);

                    if (itemRect.IntersectsWith(ClientRectangle))
                    {
                        DrawItemState state =
                            (this.SelectionMode == SelectionMode.One && SelectedIndex == i) ||
                            (this.SelectionMode != SelectionMode.None && SelectedIndices.Contains(i))
                                ? DrawItemState.Selected
                                : DrawItemState.Default;

                        itemsToDraw.Add(new DrawItemEventArgs(g, Font, itemRect, i, state, ForeColor, BackColor));
                    }
                }
            }

            // We have to draw the background in case there are no items or not enough items to fill out the size.
            // Very slightly wasteful because some of it may be painted over by the items later, but meh.
            g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, ClientRectangle);

            // TODO: @DarkMode(DarkListBox): Draw the border.
            // Set border style to none, then bump the item rectangles over to account for it, then draw the
            // border.

            //int realHeight = IntegralHeight ? ItemHeight * itemsToDraw.Count : Height;
            //var borderRect = new Rectangle(0, 0, Width - 1, realHeight - 1);
            //g.DrawRectangle(DarkColors.BlueHighlightPen, borderRect);

            for (int i = 0; i < itemsToDraw.Count; i++)
            {
                OnDrawItem(itemsToDraw[i]);
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (!_darkModeEnabled)
            {
                base.OnDrawItem(e);
                return;
            }

            if (e.Index == -1) return;

            e.Graphics.FillRectangle((e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? DarkColors.BlueSelectionBrush
                : DarkColors.Fen_ControlBackgroundBrush, e.Bounds);

            const TextFormatFlags textFormat =
                TextFormatFlags.Default |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoPadding |
                TextFormatFlags.NoClipping;

            var textRect = new Rectangle(e.Bounds.X + 2, e.Bounds.Y, e.Bounds.Width - 2, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, Items[e.Index].ToString(), e.Font, textRect, e.ForeColor, textFormat);
        }

        public bool Suspended { get; set; }
        public ScrollBarVisualOnly_Native VerticalVisualScrollBar { get; }
        public ScrollBarVisualOnly_Native HorizontalVisualScrollBar { get; }
        public ScrollBarVisualOnly_Corner VisualScrollBarCorner { get; }
        public Control ClosestAddableParent => Parent;
        public event EventHandler? DarkModeChanged;
        public event EventHandler? Scroll;
        public event EventHandler? VisibilityChanged;
    }
}
