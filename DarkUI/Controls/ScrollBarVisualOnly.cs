using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DarkUI.Win32;

namespace DarkUI.Controls
{
    public sealed class ScrollBarVisualOnly : ScrollBarVisualOnly_Base
    {
        #region Private fields

        private readonly ScrollBar _owner;

        #endregion

        #region Constructor / init

        public ScrollBarVisualOnly(ScrollBar owner, bool passMouseWheel = false)
            : base(owner is VScrollBar, passMouseWheel)
        {
            SetUpSelf();

            #region Setup involving owner

            _owner = owner;

            if (_owner.Parent != null)
            {
                _owner.Parent.Controls.Add(this);
                BringToFront();
                if (owner.Parent is IDarkableScrollable ids)
                {
                    _owner.Parent.Paint += (sender, e) => PaintDarkScrollBars(ids, e);
                }
            }

            _owner.VisibleChanged += (sender, e) => { if (_owner.Visible) BringToFront(); };
            _owner.Scroll += (sender, e) => { if (_owner.Visible || Visible) RefreshIfNeeded(); };

            #endregion

            SetUpAfterOwner();
        }

        #endregion

        #region Private methods

        private protected override Native.SCROLLBARINFO GetCurrentScrollBarInfo()
        {
            var sbi = new Native.SCROLLBARINFO();
            sbi.cbSize = Marshal.SizeOf(sbi);

            if (_owner.IsHandleCreated)
            {
                Native.GetScrollBarInfo(_owner.Handle, Native.OBJID_CLIENT, ref sbi);
            }

            return sbi;
        }

        private protected override void RefreshIfNeeded()
        {
            // Refresh only if our thumb's size/position is stale. Otherwise, we get unacceptable lag.
            var sbi = GetCurrentScrollBarInfo();
            if (_xyThumbTop == null)
            {
                _xyThumbTop = sbi.xyThumbTop;
                _xyThumbBottom = sbi.xyThumbBottom;
                Refresh();
            }
            else
            {
                if (sbi.xyThumbTop != _xyThumbTop || sbi.xyThumbBottom != _xyThumbBottom)
                {
                    Refresh();
                }
            }
        }

        private static void PaintDarkScrollBars(IDarkableScrollable control, PaintEventArgs e)
        {
            if (control.DarkModeEnabled)
            {
                for (int i = 0; i < 2; i++)
                {
                    var realScrollBar = i == 0 ? control.VerticalScrollBar : control.HorizontalScrollBar;
                    var visualScrollBar = i == 0 ? control.VerticalVisualScrollBar : control.HorizontalVisualScrollBar;

                    // PERF: Check if we need to set expensive properties before setting expensive properties
                    if (realScrollBar.Visible)
                    {
                        visualScrollBar.Location = realScrollBar.Location;
                        visualScrollBar.Size = realScrollBar.Size;
                        visualScrollBar.Anchor = realScrollBar.Anchor;
                    }

                    if (realScrollBar.Visible != visualScrollBar.Visible)
                    {
                        visualScrollBar.Visible = realScrollBar.Visible;
                    }
                }

                if (control.VerticalScrollBar.Visible && control.HorizontalScrollBar.Visible)
                {
                    // Draw the corner in between the two scroll bars
                    // TODO: @DarkMode: Also cache this brush
                    using (var b = new SolidBrush(control.VerticalVisualScrollBar.BackColor))
                    {
                        e.Graphics.FillRectangle(b, new Rectangle(
                            control.VerticalScrollBar.Location.X,
                            control.HorizontalScrollBar.Location.Y,
                            control.VerticalScrollBar.Width,
                            control.HorizontalScrollBar.Height));
                    }
                }
            }
            else
            {
                control.VerticalVisualScrollBar.Hide();
                control.HorizontalVisualScrollBar.Hide();
            }
        }

        #endregion

        #region Event overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            PaintArrows(g);

            #region Thumb

            if (_owner.IsHandleCreated)
            {
                var sbi = GetCurrentScrollBarInfo();

                _xyThumbTop = sbi.xyThumbTop;
                _xyThumbBottom = sbi.xyThumbBottom;

                g.FillRectangle(CurrentThumbBrush, GetVisualThumbRect(ref sbi));
            }

            #endregion

            base.OnPaint(e);
        }

        protected override void WndProc(ref Message m)
        {
            void SendToOwner(ref Message _m)
            {
                if (_owner.IsHandleCreated)
                {
                    Native.PostMessage(_owner.Handle, _m.Msg, _m.WParam, _m.LParam);
                }
            }

            if (ShouldSendToOwner(m.Msg))
            {
                SendToOwner(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        #endregion
    }
}
