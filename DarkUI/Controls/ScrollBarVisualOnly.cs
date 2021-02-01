using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DarkUI.Win32;

namespace DarkUI.Controls
{
    public sealed class ScrollBarVisualOnly : Control
    {

        private Color _greySelection;
        private SolidBrush _paintBrush;

        public ScrollBarVisualOnly(ScrollBar owner)
        {
            _owner = owner;

            BackColor = Config.Colors.Fen_DarkBackground;
            DoubleBuffered = true;

            _greySelection = Config.Colors.GreySelection;
            _paintBrush = new SolidBrush(_greySelection);

            SetStyle(
                ControlStyles.UserPaint |
                //ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.CacheText,
                true);
            //SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer, false);
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

        //private Random _rnd = new Random();

        protected override void WndProc(ref Message m)
        {
            //if (m.Msg == WM_NCHITTEST)
            //{
            //    //m.Result = new IntPtr(HTTRANSPARENT);
            //    var parent = Parent;
            //    //if (parent != null && parent.IsHandleCreated)
            //    if (OwnerHandle != null)
            //    {
            //        Trace.WriteLine(_rnd.Next() + "hittest");
            //        Native.PostMessage((IntPtr)OwnerHandle, m.Msg, m.WParam, m.LParam);
            //        m.Result = IntPtr.Zero;
            //    }

            //}
            //if (m.Msg == Native.WM_MOUSEMOVE || m.Msg == Native.WM_NCMOUSEMOVE)
            //{
            //    Trace.WriteLine("Sdfdsfds f");
            //    Invalidate();
            //    m.Result = IntPtr.Zero;
            //}
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
                //var parent = Parent;
                //if (parent != null && parent.IsHandleCreated)
                if (_owner.IsHandleCreated)
                {
                    //Trace.WriteLine(_rnd.Next() + "hittest");
                    Native.PostMessage((IntPtr)_owner.Handle, m.Msg, m.WParam, m.LParam);
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
                Trace.WriteLine(Name);
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
            if (_owner.IsHandleCreated)
            {
                //e.Graphics.FillRectangle(Brushes.Red, 0, 0, Size.Width, Size.Height);

                //Trace.WriteLine(_rnd.Next() + " dgv vscrolled");
                Native.SCROLLBARINFO psbi = new Native.SCROLLBARINFO();
                psbi.cbSize = Marshal.SizeOf(psbi);
                int result = Native.GetScrollBarInfo((IntPtr)_owner.Handle, Native.OBJID_CLIENT, ref psbi);
                if (result == 0)
                {
                    //Trace.WriteLine("***ERROR: " + Marshal.GetLastWin32Error());
                }
                else
                {
                    _xyThumbTop = psbi.xyThumbTop;
                    _xyThumbBottom = psbi.xyThumbBottom;

                    //Trace.WriteLine(nameof(psbi.dxyLineButton) + ": " + psbi.dxyLineButton);
                    //Trace.WriteLine(nameof(psbi.xyThumbTop) + ": " + psbi.xyThumbTop);
                    //Trace.WriteLine(nameof(psbi.xyThumbBottom) + ": " + psbi.xyThumbBottom);

                    var g = e.Graphics;
                    {
                        if (_owner is VScrollBar)
                        {
                            g.FillRectangle(_paintBrush, 1, psbi.xyThumbTop, Width - 2, psbi.xyThumbBottom - psbi.xyThumbTop);
                        }
                        else
                        {
                            g.FillRectangle(_paintBrush, psbi.xyThumbTop, 1, psbi.xyThumbBottom - psbi.xyThumbTop, Height - 2);
                        }
                    }
                }
            }

            base.OnPaint(e);
        }
    }
}
