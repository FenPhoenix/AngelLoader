using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace AngelLoader.CustomControls
{
    internal sealed class ToolStripCustom : ToolStrip
    {
        /// <summary>
        /// Fiddle this around to get the right-side garbage line to disappear again when Padding is set to something.
        /// </summary>
        [Browsable(true)] public int PaddingDrawNudge { get; set; } = 0;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Hack in order to be able to have ManagerRenderMode, but also get rid of any garbage around the
            // edges that may be drawn. In particular, there's an ugly visual-styled vertical line at the right
            // side if you don't do this.
            var rect1 = new Rectangle(0, 0, Items[0].Bounds.X, Height);
            var last = Items[Items.Count - 1];
            var rect2Start = last.Bounds.X + last.Bounds.Width;
            var rect2 = new Rectangle(rect2Start - PaddingDrawNudge, 0, (Width - rect2Start) + PaddingDrawNudge, Height);

            e.Graphics.FillRectangle(SystemBrushes.Control, rect1);
            e.Graphics.FillRectangle(SystemBrushes.Control, rect2);
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
    public sealed class ToolStripSeparatorCustom : ToolStripSeparator
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            // These ToolStrip abominations just won't behave. They claim to have their color set to "Control",
            // but what they really mean is "almost Control but not quite". Same with any other hue. Have to set
            // it here just to get them to listen.
            e.Graphics.FillRectangle(SystemBrushes.Control, 0, 0, Width, Height);
            base.OnPaint(e);
        }
    }

    #region Dropdown working

    internal sealed class ToolStripDropDownCustom : ToolStripDropDown
    {
        internal ToolStripDropDownCustom() => AutoClose = false;

        protected override void OnVisibleChanged(EventArgs e)
        {
            // Message filters are apparently heavy, so only run it when we're visible
            if (Visible)
            {
                AutoClose = false;
            }
            else
            {
            }
            base.OnVisibleChanged(e);
        }

        internal void HideThis()
        {
            AutoClose = true;
            Hide();
        }

        private bool CursorOutsideDropDown()
        {
            var thisRect = new Rectangle(PointToClient(PointToScreen(new Point(0, 0))), ClientSize);
            Rectangle? cbDDRect = null;
            foreach (ToolStripItem tsItem in Items)
            {
                if (tsItem is ToolStripComboBox cb && cb.ComboBox != null && cb.ComboBox.DroppedDown)
                {
                    var cbBox = cb.ComboBox;

                    int width = 0;
                    using (var g = cbBox.CreateGraphics())
                    {
                        foreach (var cbItem in cbBox.Items)
                        {
                            if (!(cbItem is string cbItemStr)) continue;
                            int newWidth = (int)g.MeasureString(cbItemStr, cbBox.Font).Width;
                            if (width < newWidth) width = newWidth;
                        }
                    }
                    if (width < cbBox.DropDownWidth) width = cbBox.DropDownWidth;

                    cbDDRect = new Rectangle(
                        PointToClient(cbBox.PointToScreen(new Point(0, cb.Height))),
                        new Size(width, cbBox.ItemHeight * cbBox.Items.Count)
                    );

                    break;
                }

            }

            return !thisRect.Contains(PointToClient(Cursor.Position)) &&
                   (cbDDRect == null || !((Rectangle)cbDDRect).Contains(PointToClient(Cursor.Position)));
        }

        //public bool PreFilterMessage(ref Message m)
        //{
        //    const bool BlockMessage = true;
        //    const bool PassMessageOn = false;

        //    if (m.Msg == WM_KEYDOWN && (int)m.WParam == VK_ESCAPE)
        //    {
        //        ComboBox cbBox = null;
        //        foreach (ToolStripItem tsItem in Items)
        //        {
        //            if (tsItem is ToolStripComboBox cb && cb.ComboBox != null && cb.ComboBox.DroppedDown)
        //            {
        //                cbBox = cb.ComboBox;
        //                break;
        //            }
        //        }

        //        if (cbBox == null) HideThis();
        //    }
        //    else if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_MBUTTONDOWN || m.Msg == WM_RBUTTONDOWN ||
        //             m.Msg == WM_LBUTTONDBLCLK || m.Msg == WM_MBUTTONDBLCLK || m.Msg == WM_RBUTTONDBLCLK)
        //    {
        //        // I don't think this will even be running if we're not visible?
        //        if (!Visible) return PassMessageOn;

        //        // Make dropdown behavior less flighty and more solid when it comes to mouse clicks


        //        //if (!thisRect.Contains(PointToClient(Cursor.Position)) &&
        //        //    (cbDDRect == null || !((Rectangle)cbDDRect).Contains(PointToClient(Cursor.Position))))
        //        if (CursorOutsideDropDown())
        //        {
        //            HideThis();
        //            return BlockMessage;
        //        }
        //    }

        //    return PassMessageOn;
        //}
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public sealed class ToolStripCheckBoxCustom : ToolStripControlHost
    {
        public CheckBox CheckBox => Control as CheckBox;

        public ToolStripCheckBoxCustom() : base(new CheckBox()) { }
    }

    #endregion
}
