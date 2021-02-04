using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Windows.Forms;
using DarkUI.Win32;

namespace DarkUI.Controls
{
    internal static class Extensions
    {
        internal static T Clamp<T>(this T value, T min, T max) where T : IComparable<T> =>
            value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
    }

    public sealed class ScrollBarVisualOnly : Control
    {

        private readonly Color _greySelection;
        private readonly SolidBrush _thumbNormalBrush;

        // We want them separate, not all pointing to the same reference
        private readonly Bitmap _upArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _downArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _leftArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _rightArrow = ScrollIcons.scrollbar_arrow_small_standard;

        private readonly Timer _timer = new Timer();

        private readonly Orientation _orientation;

        private readonly DarkContextMenu _menu;

        private ToolStripMenuItem _scrollHereMenuItem;

        private ToolStripMenuItem _minMenuItem;
        private ToolStripMenuItem _maxMenuItem;

        private ToolStripMenuItem _pageBackMenuItem;
        private ToolStripMenuItem _pageForwardMenuItem;

        private ToolStripMenuItem _scrollBackMenuItem;
        private ToolStripMenuItem _scrollForwardMenuItem;

        internal static double GetPercentFromValue(int current, int total) => (double)(100 * current) / total;
        //internal static int GetValueFromPercent(int percent, int total) => (percent / 100) * total;
        internal static int GetValueFromPercent(double percent, int total) =>
            (int)((percent / 100) * total);
        //(int)Math.Round((percent / 100) * total);

        private const int SB_CTL = 2;

        [DllImport("user32.dll")]
        public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        internal const int WM_USER = 0x0400;
        internal const int WM_REFLECT = 8192;
        private const int WM_HSCROLL = 0x0114;
        private const int WM_VSCROLL = 0x0115;

        private const int SB_LINEUP = 0;
        private const int SB_LINELEFT = 0;
        private const int SB_LINEDOWN = 1;
        private const int SB_LINERIGHT = 1;
        private const int SB_PAGEUP = 2;
        private const int SB_PAGELEFT = 2;
        private const int SB_PAGEDOWN = 3;
        private const int SB_PAGERIGHT = 3;
        private const int SB_THUMBPOSITION = 4;
        private const int SB_THUMBTRACK = 5;
        private const int SB_TOP = 6;
        private const int SB_LEFT = 6;
        private const int SB_BOTTOM = 7;
        private const int SB_RIGHT = 7;
        private const int SB_ENDSCROLL = 8;

        private const int SIF_RANGE = 0x0001;
        private const int SIF_PAGE = 0x0002;
        private const int SIF_POS = 0x0004;
        private const int SIF_DISABLENOSCROLL = 0x0008;
        private const int SIF_TRACKPOS = 0x0010;
        private const int SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS);

        [Serializable, StructLayout(LayoutKind.Sequential)]
        struct SCROLLINFO
        {
            public uint cbSize;
            public uint fMask;
            public int nMin;
            public int nMax;
            public uint nPage;
            public int nPos;
            public int nTrackPos;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

        [DllImport("user32.dll")]
        static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref SCROLLINFO lpsi, bool fRedraw);

        public ScrollBarVisualOnly(ScrollBar owner)
        {
            _owner = owner;

            _orientation = owner is VScrollBar ? Orientation.Vertical : Orientation.Horizontal;

            _menu = new DarkContextMenu(darkModeEnabled: true)
            {
                Items =
                {
                    (_scrollHereMenuItem = new ToolStripMenuItem()),
                    new ToolStripSeparator(),
                    (_minMenuItem = new ToolStripMenuItem()),
                    (_maxMenuItem = new ToolStripMenuItem()),
                    new ToolStripSeparator(),
                    (_pageBackMenuItem = new ToolStripMenuItem()),
                    (_pageForwardMenuItem = new ToolStripMenuItem()),
                    new ToolStripSeparator(),
                    (_scrollBackMenuItem = new ToolStripMenuItem()),
                    (_scrollForwardMenuItem = new ToolStripMenuItem())
                }
            };

            //_owner.ValueChanged += (sender, e) =>
            //{
            //    Trace.WriteLine("value: " + _owner.Value);
            //};

            Trace.WriteLine(_owner.Maximum);

            _scrollHereMenuItem.Click += (sender, e) =>
            {
                Trace.WriteLine("----------------");

                Trace.WriteLine("_owner.Value: " + _owner.Value);
                Trace.WriteLine("_owner.Maximum: " + _owner.Maximum);

                var si = GetCurrentScrollBarInfo();
                int thumbSize = si.xyThumbBottom - si.xyThumbTop;

                Trace.WriteLine(si.dxyLineButton);

                Trace.WriteLine(si.rcScrollBar.left);
                Trace.WriteLine(si.rcScrollBar.top);
                Trace.WriteLine(si.rcScrollBar.right);
                Trace.WriteLine(si.rcScrollBar.bottom);


                var si2 = new SCROLLINFO();
                si2.cbSize = (uint)Marshal.SizeOf(si2);
                si2.fMask = SIF_PAGE;
                GetScrollInfo(_owner.Handle, SB_CTL, ref si2);

                Trace.WriteLine(si2.nPage);

                //si.xyThumbTop = 0;
                //si.xyThumbBottom = 100;
                //SetScrollBarInfo(_owner.Handle, SB_CTL,ref si, true);

                int arrowMargin = _orientation == Orientation.Vertical
                    ? SystemInformation.VerticalScrollBarArrowHeight
                    : SystemInformation.HorizontalScrollBarArrowWidth;

                Trace.WriteLine(nameof(arrowMargin) + ": " + arrowMargin);

                var rect = new Rectangle(0, arrowMargin, Width, (Height - (arrowMargin * 2)) - thumbSize);

                double divisor = (double)_owner.Maximum / rect.Height;
                int thumbSizeInValueUnits = (int)Math.Round(thumbSize * divisor, MidpointRounding.AwayFromZero);
                //int thumbSizeInValueUnits = 2592;

                //int posY = PointToClient(Cursor.Position).Y;

                int posY = _storedCursorPosition.Y;

                Trace.WriteLine("initial posY: " + posY);

                posY -= arrowMargin;

                //posY = posY.Clamp(0, (Height - (arrowMargin * 2)) - thumbSize);

                //posY = (((Height - (arrowMargin * 2))-thumbSize)/2);
                Trace.WriteLine("pos.Y: " + posY);

                //Trace.WriteLine(Height - arrowMargin);

                Trace.WriteLine(nameof(thumbSize) + ": " + thumbSize);

                //Point menuLoc = _menu.PointToClient(_menu.PointToScreen(_menu.Location));
                //Point menuLoc = _menu.PointToScreen(_menu.Location);
                Point menuLoc = _menu.Location;

                Trace.WriteLine("menu pos: " + menuLoc);

                Trace.WriteLine(rect);

                Trace.WriteLine("Height " + Height);
                Trace.WriteLine("Adjusted height: " + ((Height - (arrowMargin * 2)) - thumbSize).ToString());

                //posY += thumbSize / 2;

                //posY = 141;

                //posY = rect.Height / 2;

                //double posPercent = GetPercentFromValue(posY, (Height - (arrowMargin * 2)) - (thumbSize));
                double posPercent = GetPercentFromValue(posY, rect.Height).Clamp(0, 100);
                int finalValue = GetValueFromPercent(posPercent, (int)(_owner.Maximum - Math.Max(si2.nPage - 1, 0)));
                //finalValue -= thumbSizeInValueUnits / 2;

                //System.Windows.Controls.Primitives.ScrollBar

                //finalValue -= thumbSize / 2;

                Trace.WriteLine(nameof(posPercent) + ": " + posPercent);

                Trace.WriteLine(nameof(finalValue) + ": " + finalValue);

                //finalValue = (int)(35772 * .75f);

                _owner.Value = finalValue.Clamp(_owner.Minimum, _owner.Maximum);

                //SetScrollPos(_owner.Handle, SB_CTL, finalValue, true);

                //SetScrollPos(_owner.Handle, SB_CTL, 35772/2, true);

                Trace.WriteLine("_owner.Value: " + _owner.Value);

                uint msg = (uint)(_orientation == Orientation.Vertical ? WM_VSCROLL : WM_HSCROLL);
                Native.SendMessage(_owner.Handle, WM_REFLECT + msg, (IntPtr)SB_THUMBTRACK, IntPtr.Zero);
            };

            _minMenuItem.Click += (sender, e) => { };
            _maxMenuItem.Click += (sender, e) => { };

            _pageBackMenuItem.Click += (sender, e) => { };
            _pageForwardMenuItem.Click += (sender, e) => { };

            _scrollBackMenuItem.Click += (sender, e) => { };
            _scrollForwardMenuItem.Click += (sender, e) => { };

            _timer.Interval = 1;
            _timer.Tick += (sender, e) => RefreshIfNeeded();

            ResizeRedraw = true;

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

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var cp = base.CreateParams;
        //        cp.ExStyle |= WS_EX_NOACTIVATE;
        //        return cp;
        //    }
        //}

        private Random _rnd = new Random();

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

        private bool SafeToShowDarkMenu()
        {
            string[] temp = GetMenuStrings();
            if (temp == null) return false;

            return true;
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

                ret[0] = rm.GetString("ScrollBar_ContextMenu_ScrollHere", CultureInfo.CurrentCulture);

                if (_orientation == Orientation.Vertical)
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

        private Point _storedCursorPosition = Point.Empty;

        private bool ShowDarkMenu()
        {
            string[] items = GetMenuStrings();
            if (items == null) return false;

            _scrollHereMenuItem.Text = items[0];
            _minMenuItem.Text = items[1];
            _maxMenuItem.Text = items[2];
            _pageBackMenuItem.Text = items[3];
            _pageForwardMenuItem.Text = items[4];
            _scrollBackMenuItem.Text = items[5];
            _scrollForwardMenuItem.Text = items[6];

            _storedCursorPosition = PointToClient(Cursor.Position);
            _menu.Show(this, _storedCursorPosition);

            return true;
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
                int result = Native.GetScrollBarInfo(_owner.Handle, Native.OBJID_CLIENT, ref psbi);
            }
            return psbi;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            bool isVertical = _orientation == Orientation.Vertical;

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

        protected override void OnVisibleChanged(EventArgs e)
        {
            _timer.Enabled = Visible;
            base.OnVisibleChanged(e);
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

                _menu.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
