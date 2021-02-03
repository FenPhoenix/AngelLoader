using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DarkUI.Win32;

namespace DarkUI.Controls
{
    public sealed class ScrollBarVisualOnly : Control
    {

        private readonly Color _greySelection;
        private readonly SolidBrush _thumbNormalBrush;

        // We want them separate, not all pointing to the same reference
        private readonly Bitmap _upArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _downArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _leftArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _rightArrow = ScrollIcons.scrollbar_arrow_small_standard;

        public ScrollBarVisualOnly(ScrollBar owner)
        {
            _owner = owner;

            #region Set up scroll bar arrows

            _upArrow.RotateFlip(RotateFlipType.Rotate180FlipNone);
            _leftArrow.RotateFlip(RotateFlipType.Rotate90FlipNone);
            _rightArrow.RotateFlip(RotateFlipType.Rotate270FlipNone);

            #endregion

            BackColor = Config.Colors.Fen_DarkBackground;
            DoubleBuffered = true;

            _greySelection = Config.Colors.GreySelection;
            _thumbNormalBrush = new SolidBrush(_greySelection);

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.CacheText,
                true);
        }

        private readonly ScrollBar _owner;

        private const int WM_NCHITTEST = 0x0084;
        private const int WS_EX_NOACTIVATE = 134_217_728;
        private const int HTTRANSPARENT = -1;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.WM_LBUTTONDOWN || m.Msg == Native.WM_NCLBUTTONDOWN ||
                m.Msg == Native.WM_MBUTTONDOWN || m.Msg == Native.WM_NCMBUTTONDOWN ||
                m.Msg == Native.WM_RBUTTONDOWN || m.Msg == Native.WM_NCRBUTTONDOWN ||
                m.Msg == Native.WM_LBUTTONDBLCLK || m.Msg == Native.WM_NCLBUTTONDBLCLK ||
                m.Msg == Native.WM_MBUTTONDBLCLK || m.Msg == Native.WM_NCMBUTTONDBLCLK ||
                m.Msg == Native.WM_RBUTTONDBLCLK || m.Msg == Native.WM_NCRBUTTONDBLCLK ||
                m.Msg == Native.WM_LBUTTONUP || m.Msg == Native.WM_NCLBUTTONUP ||
                m.Msg == Native.WM_MBUTTONUP || m.Msg == Native.WM_NCMBUTTONUP ||
                m.Msg == Native.WM_RBUTTONUP || m.Msg == Native.WM_NCRBUTTONUP ||

                m.Msg == Native.WM_MOUSEMOVE || m.Msg == Native.WM_NCMOUSEMOVE //||

                // Don't handle mouse wheel or mouse wheel tilt for now - mousewheel at least breaks on FMsDGV
                //m.Msg == Native.WM_MOUSEWHEEL || m.Msg == Native.WM_MOUSEHWHEEL
                )
            {
                if (_owner.IsHandleCreated)
                {
                    Native.PostMessage(_owner.Handle, m.Msg, m.WParam, m.LParam);
                    m.Result = IntPtr.Zero;
                }
            }
            base.WndProc(ref m);
        }

        private int? _xyThumbTop;
        private int? _xyThumbBottom;

        public void RefreshIfNeeded()
        {
            // Refresh only if our thumb's size/position is stale. Otherwise, we get unacceptable lag.
            var psbi = GetCurrentScrollBarInfo();
            if (_xyThumbTop == null)
            {
                _xyThumbTop = psbi.xyThumbTop;
                _xyThumbBottom = psbi.xyThumbBottom;
                Refresh();
            }
            else
            {
                if (psbi.xyThumbTop != _xyThumbTop || psbi.xyThumbBottom != _xyThumbBottom)
                {
                    Refresh();
                }
            }
        }

        private Native.SCROLLBARINFO GetCurrentScrollBarInfo()
        {
            Native.SCROLLBARINFO psbi = new Native.SCROLLBARINFO();
            psbi.cbSize = Marshal.SizeOf(psbi);
            if (_owner.IsHandleCreated)
            {
                int result = Native.GetScrollBarInfo((IntPtr)_owner.Handle, Native.OBJID_CLIENT, ref psbi);
            }
            return psbi;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            bool isVertical = _owner is VScrollBar;

            var g = e.Graphics;

            #region Arrows

            int w = SystemInformation.VerticalScrollBarWidth;
            int h = SystemInformation.VerticalScrollBarArrowHeight;

            if (isVertical)
            {
                g.DrawImageUnscaled(
                    _upArrow,
                    (w / 2) - (_upArrow.Width / 2),
                    (h / 2) - (_upArrow.Height / 2));

                g.DrawImageUnscaled(
                    _downArrow,
                    (w / 2) - (_upArrow.Width / 2),
                    (Height - h) + ((h / 2) - (_upArrow.Height / 2)));
            }
            else
            {
                g.DrawImageUnscaled(
                    _leftArrow,
                    (w / 2) - (_leftArrow.Width / 2),
                    (h / 2) - (_leftArrow.Height / 2));

                g.DrawImageUnscaled(
                    _rightArrow,
                    Width - w + (w / 2) - (_leftArrow.Width / 2),
                    (h / 2) - (_leftArrow.Height / 2));
            }

            #endregion

            #region Thumb

            if (_owner.IsHandleCreated)
            {
                var psbi = GetCurrentScrollBarInfo();
                _xyThumbTop = psbi.xyThumbTop;
                _xyThumbBottom = psbi.xyThumbBottom;

                if (isVertical)
                {
                    g.FillRectangle(
                        _thumbNormalBrush,
                        1,
                        psbi.xyThumbTop,
                        Width - 2,
                        psbi.xyThumbBottom - psbi.xyThumbTop);
                }
                else
                {
                    g.FillRectangle(
                        _thumbNormalBrush,
                        psbi.xyThumbTop,
                        1,
                        psbi.xyThumbBottom - psbi.xyThumbTop,
                        Height - 2);
                }
            }

            #endregion

            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _upArrow.Dispose();
                _downArrow.Dispose();
                _leftArrow.Dispose();
                _rightArrow.Dispose();

                _thumbNormalBrush.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
