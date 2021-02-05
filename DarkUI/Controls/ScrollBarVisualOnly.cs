using System;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DarkUI.Win32;

namespace DarkUI.Controls
{

    // TODO: Get rid of this when we combine projects of course
    internal static class Extensions
    {
        internal static T Clamp<T>(this T value, T min, T max) where T : IComparable<T> =>
            value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
    }

    public sealed class ScrollBarVisualOnly : Control
    {
        #region Private fields

        private readonly ScrollBar _owner;

        private int? _xyThumbTop;
        private int? _xyThumbBottom;

        private readonly Color _greySelection;
        private readonly SolidBrush _thumbNormalBrush;

        // We want them separate, not all pointing to the same reference
        private readonly Bitmap _upArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _downArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _leftArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _rightArrow = ScrollIcons.scrollbar_arrow_small_standard;

        private readonly Timer _timer = new Timer();

        private readonly bool _isVertical;

        private readonly DarkContextMenu Menu;

        private readonly ToolStripMenuItem ScrollHereMenuItem;

        private readonly ToolStripMenuItem MinMenuItem;
        private readonly ToolStripMenuItem MaxMenuItem;

        private readonly ToolStripMenuItem PageBackMenuItem;
        private readonly ToolStripMenuItem PageForwardMenuItem;

        private readonly ToolStripMenuItem ScrollBackMenuItem;
        private readonly ToolStripMenuItem ScrollForwardMenuItem;

        private Point _storedCursorPosition = Point.Empty;

        private const int WM_REFLECT = 0x2000;
        private const int WM_HSCROLL = 0x0114;
        private const int WM_VSCROLL = 0x0115;

        private const int SB_THUMBTRACK = 5;

        private const int WS_EX_NOACTIVATE = 0x8000000;

        #endregion

        private static double GetPercentFromValue(int current, int total) => (double)(100 * current) / total;
        private static int GetValueFromPercent(double percent, int total) => (int)((percent / 100) * total);
        private void SetOwnerValue(int value)
        {
            _owner.Value = value.Clamp(_owner.Minimum, _owner.Maximum);

            // We have to explicitly send this message, or else the scroll bar's parent control doesn't
            // actually update itself in accordance with the scroll bar thumb position (at least with the
            // DataGridView anyway).
            uint msg = (uint)(_isVertical ? WM_VSCROLL : WM_HSCROLL);
            Native.SendMessage(_owner.Handle, WM_REFLECT + msg, (IntPtr)SB_THUMBTRACK, IntPtr.Zero);
        }

        #region Menu item event handlers

        private void ScrollHereMenuItem_Click(object sender, EventArgs e)
        {
            var sbi = GetCurrentScrollBarInfo();
            int thumbSize = sbi.xyThumbBottom - sbi.xyThumbTop;

            int arrowMargin = _isVertical
                ? SystemInformation.VerticalScrollBarArrowHeight
                : SystemInformation.HorizontalScrollBarArrowWidth;

            var rect = _isVertical
                ? new Rectangle(0, arrowMargin, Width, (Height - (arrowMargin * 2)) - thumbSize)
                : new Rectangle(arrowMargin, 0, (Width - (arrowMargin * 2)) - thumbSize, Height);

            int posAlong = _isVertical ? _storedCursorPosition.Y : +_storedCursorPosition.X;

            posAlong -= arrowMargin;

            posAlong -= thumbSize / 2;

            double posPercent = GetPercentFromValue(posAlong, _isVertical ? rect.Height : rect.Width).Clamp(0, 100);
            // Important that we use this formula (nMax - max(nPage -1, 0) or else our position is always
            // infuriatingly not-quite-right.
            int finalValue = GetValueFromPercent(posPercent, _owner.Maximum - Math.Max(_owner.LargeChange - 1, 0));

            SetOwnerValue(finalValue);
        }

        private void MinMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(0);

        private void MaxMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(_owner.Maximum);

        private void PageBackMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(_owner.Value - _owner.LargeChange.Clamp(_owner.Minimum, _owner.Maximum));

        private void PageForwardMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(_owner.Value + _owner.LargeChange.Clamp(_owner.Minimum, _owner.Maximum));

        private void ScrollBackMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(_owner.Value - _owner.SmallChange.Clamp(_owner.Minimum, _owner.Maximum));

        private void ScrollForwardMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(_owner.Value + _owner.SmallChange.Clamp(_owner.Minimum, _owner.Maximum));

        #endregion

        #region Constructor / init

        public ScrollBarVisualOnly(ScrollBar owner)
        {
            _owner = owner;

            _isVertical = owner is VScrollBar;

            DoubleBuffered = true;
            ResizeRedraw = true;

            #region Set up menu

            Menu = new DarkContextMenu(darkModeEnabled: true)
            {
                Items =
                {
                    (ScrollHereMenuItem = new ToolStripMenuItem()),
                    new ToolStripSeparator(),
                    (MinMenuItem = new ToolStripMenuItem()),
                    (MaxMenuItem = new ToolStripMenuItem()),
                    new ToolStripSeparator(),
                    (PageBackMenuItem = new ToolStripMenuItem()),
                    (PageForwardMenuItem = new ToolStripMenuItem()),
                    new ToolStripSeparator(),
                    (ScrollBackMenuItem = new ToolStripMenuItem()),
                    (ScrollForwardMenuItem = new ToolStripMenuItem())
                }
            };

            ScrollHereMenuItem.Click += ScrollHereMenuItem_Click;

            MinMenuItem.Click += MinMenuItem_Click;
            MaxMenuItem.Click += MaxMenuItem_Click;

            PageBackMenuItem.Click += PageBackMenuItem_Click;
            PageForwardMenuItem.Click += PageForwardMenuItem_Click;

            ScrollBackMenuItem.Click += ScrollBackMenuItem_Click;
            ScrollForwardMenuItem.Click += ScrollForwardMenuItem_Click;

            #endregion

            #region Set up refresh timer

            _timer.Interval = 1;
            _timer.Tick += (sender, e) => RefreshIfNeeded();

            #endregion

            #region Set up scroll bar arrows

            _upArrow.RotateFlip(RotateFlipType.Rotate180FlipNone);
            _leftArrow.RotateFlip(RotateFlipType.Rotate90FlipNone);
            _rightArrow.RotateFlip(RotateFlipType.Rotate270FlipNone);

            #endregion

            BackColor = Config.Colors.Fen_DarkBackground;

            _greySelection = Config.Colors.GreySelection;
            _thumbNormalBrush = new SolidBrush(_greySelection);

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.CacheText,
                true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }

        #endregion

        #region Private methods

        private Native.SCROLLBARINFO GetCurrentScrollBarInfo()
        {
            var sbi = new Native.SCROLLBARINFO();
            sbi.cbSize = Marshal.SizeOf(sbi);

            if (_owner.IsHandleCreated)
            {
                Native.GetScrollBarInfo(_owner.Handle, Native.OBJID_CLIENT, ref sbi);
            }

            return sbi;
        }

        private string[] GetMenuStrings()
        {
            const int count = 7;

            string[] ret = new string[count];
            try
            {
                // WinForms doesn't have these strings in it, because it just calls the Win32 menu and the strings
                // are in Windows itself. We don't want to mess with that, so we cheat and just grab them from WPF.
                // IMPORTANT: Because we get these from WPF, we need to reference PresentationFramework or we fail!
                var rm = new ResourceManager("ExceptionStringTable", typeof(System.Windows.Application).Assembly);

                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("fr-CA");
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("fr-CA");

                ret[0] = rm.GetString("ScrollBar_ContextMenu_ScrollHere", CultureInfo.CurrentCulture);

                if (_isVertical)
                {
                    ret[1] = rm.GetString("ScrollBar_ContextMenu_Top", CultureInfo.CurrentCulture);
                    ret[2] = rm.GetString("ScrollBar_ContextMenu_Bottom", CultureInfo.CurrentCulture);
                    ret[3] = rm.GetString("ScrollBar_ContextMenu_PageUp", CultureInfo.CurrentCulture);
                    ret[4] = rm.GetString("ScrollBar_ContextMenu_PageDown", CultureInfo.CurrentCulture);
                    ret[5] = rm.GetString("ScrollBar_ContextMenu_ScrollUp", CultureInfo.CurrentCulture);
                    ret[6] = rm.GetString("ScrollBar_ContextMenu_ScrollDown", CultureInfo.CurrentCulture);
                }
                else
                {
                    ret[1] = rm.GetString("ScrollBar_ContextMenu_LeftEdge", CultureInfo.CurrentCulture);
                    ret[2] = rm.GetString("ScrollBar_ContextMenu_RightEdge", CultureInfo.CurrentCulture);
                    ret[3] = rm.GetString("ScrollBar_ContextMenu_PageLeft", CultureInfo.CurrentCulture);
                    ret[4] = rm.GetString("ScrollBar_ContextMenu_PageRight", CultureInfo.CurrentCulture);
                    ret[5] = rm.GetString("ScrollBar_ContextMenu_ScrollLeft", CultureInfo.CurrentCulture);
                    ret[6] = rm.GetString("ScrollBar_ContextMenu_ScrollRight", CultureInfo.CurrentCulture);
                }

                for (int i = 0; i < count; i++)
                {
                    if (ret[i] == null || ret[i] == "")
                    {
                        return null;
                    }
                }

                return ret;
            }
            catch
            {
                // If we couldn't get the strings, we're just going to give up and tell the caller to show the
                // normal, un-dark-mode menu. Better than nothing.
                return null;
            }
        }

        private bool ShowDarkMenu()
        {
            string[] items = GetMenuStrings();
            if (items == null) return false;

            ScrollHereMenuItem.Text = items[0];
            MinMenuItem.Text = items[1];
            MaxMenuItem.Text = items[2];
            PageBackMenuItem.Text = items[3];
            PageForwardMenuItem.Text = items[4];
            ScrollBackMenuItem.Text = items[5];
            ScrollForwardMenuItem.Text = items[6];

            // Store the cursor position because the menu just does not want to give us its control-relative
            // position, returning its screen-relative position every time. Meh.
            _storedCursorPosition = PointToClient(Cursor.Position);
            Menu.Show(this, _storedCursorPosition);

            return true;
        }

        #endregion

        #region Public methods

        public void RefreshIfNeeded()
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

        #endregion

        #region Event overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            #region Arrows

            int w = SystemInformation.VerticalScrollBarWidth;
            int h = SystemInformation.VerticalScrollBarArrowHeight;

            if (_isVertical)
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
                var sbi = GetCurrentScrollBarInfo();
                _xyThumbTop = sbi.xyThumbTop;
                _xyThumbBottom = sbi.xyThumbBottom;

                if (_isVertical)
                {
                    g.FillRectangle(
                        _thumbNormalBrush,
                        1,
                        sbi.xyThumbTop,
                        Width - 2,
                        sbi.xyThumbBottom - sbi.xyThumbTop);
                }
                else
                {
                    g.FillRectangle(
                        _thumbNormalBrush,
                        sbi.xyThumbTop,
                        1,
                        sbi.xyThumbBottom - sbi.xyThumbTop,
                        Height - 2);
                }
            }

            #endregion

            base.OnPaint(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            _timer.Enabled = Visible;
            base.OnVisibleChanged(e);
        }

        #endregion

        protected override void WndProc(ref Message m)
        {
            void SendToOwner(ref Message _m)
            {
                if (_owner.IsHandleCreated)
                {
                    Native.PostMessage(_owner.Handle, _m.Msg, _m.WParam, _m.LParam);
                    _m.Result = IntPtr.Zero;
                }
            }

            if (m.Msg == Native.WM_LBUTTONDOWN || m.Msg == Native.WM_NCLBUTTONDOWN
                || m.Msg == Native.WM_MBUTTONDOWN || m.Msg == Native.WM_NCMBUTTONDOWN
                || m.Msg == Native.WM_LBUTTONDBLCLK || m.Msg == Native.WM_NCLBUTTONDBLCLK
                || m.Msg == Native.WM_MBUTTONDBLCLK || m.Msg == Native.WM_NCMBUTTONDBLCLK
                || m.Msg == Native.WM_LBUTTONUP || m.Msg == Native.WM_NCLBUTTONUP
                || m.Msg == Native.WM_MBUTTONUP || m.Msg == Native.WM_NCMBUTTONUP

                || m.Msg == Native.WM_MOUSEMOVE || m.Msg == Native.WM_NCMOUSEMOVE

                // Don't handle mouse wheel or mouse wheel tilt for now - mousewheel at least breaks on FMsDGV
                //|| m.Msg == Native.WM_MOUSEWHEEL || m.Msg == Native.WM_MOUSEHWHEEL
                // TODO: @DarkMode: Test wheel tilt with this system!
                // (do I still have that spare Logitech mouse that works?)
                )
            {
                SendToOwner(ref m);
            }
            else if (m.Msg == Native.WM_RBUTTONDOWN || m.Msg == Native.WM_NCRBUTTONDOWN ||
                     m.Msg == Native.WM_RBUTTONDBLCLK || m.Msg == Native.WM_NCRBUTTONDBLCLK)
            {
                if (GetMenuStrings() == null) SendToOwner(ref m);
            }
            else if (m.Msg == Native.WM_RBUTTONUP || m.Msg == Native.WM_NCRBUTTONUP)
            {
                if (!ShowDarkMenu()) SendToOwner(ref m);
            }
            else
            {
                // Intentional - we want to block the mouse messages up there because we're already sending them
                // to owner if necessary, and if we don't block them we get incorrect behavior with menu showing
                // (showing the FM context menu instead of our menu etc.)
                base.WndProc(ref m);
            }
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

                _timer.Dispose();

                Menu.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
