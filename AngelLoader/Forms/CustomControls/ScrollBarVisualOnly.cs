using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class ScrollBarVisualOnly : Control
    {
        public ScrollBarVisualOnly()
        {
            BackColor = Color.Aqua;
        }

        public IntPtr? OwnerHandle;

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

        private Random _rnd = new Random();

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
            //        InteropMisc.PostMessage((IntPtr)OwnerHandle, m.Msg, m.WParam, m.LParam);
            //        m.Result = IntPtr.Zero;
            //    }

            //}
            if (m.Msg == InteropMisc.WM_LBUTTONDOWN || m.Msg == InteropMisc.WM_NCLBUTTONDOWN ||
                m.Msg == InteropMisc.WM_MBUTTONDOWN || m.Msg == InteropMisc.WM_NCMBUTTONDOWN ||
                m.Msg == InteropMisc.WM_RBUTTONDOWN || m.Msg == InteropMisc.WM_NCRBUTTONDOWN ||
                m.Msg == InteropMisc.WM_LBUTTONDBLCLK || m.Msg == InteropMisc.WM_NCLBUTTONDBLCLK ||
                m.Msg == InteropMisc.WM_MBUTTONDBLCLK || m.Msg == InteropMisc.WM_NCMBUTTONDBLCLK ||
                m.Msg == InteropMisc.WM_RBUTTONDBLCLK || m.Msg == InteropMisc.WM_NCRBUTTONDBLCLK ||
                m.Msg == InteropMisc.WM_LBUTTONUP || m.Msg == InteropMisc.WM_NCLBUTTONUP ||
                m.Msg == InteropMisc.WM_MBUTTONUP || m.Msg == InteropMisc.WM_NCMBUTTONUP ||
                m.Msg == InteropMisc.WM_RBUTTONUP || m.Msg == InteropMisc.WM_NCRBUTTONUP ||

                m.Msg == InteropMisc.WM_MOUSEMOVE || m.Msg == InteropMisc.WM_NCMOUSEMOVE //||

                // Don't handle mouse wheel or mouse wheel tilt for now - mousewheel at least breaks on FMsDGV
                //m.Msg == InteropMisc.WM_MOUSEWHEEL || m.Msg == InteropMisc.WM_MOUSEHWHEEL
                )
            {
                //var parent = Parent;
                //if (parent != null && parent.IsHandleCreated)
                if (OwnerHandle != null)
                {
                    //Trace.WriteLine(_rnd.Next() + "hittest");
                    InteropMisc.PostMessage((IntPtr)OwnerHandle, m.Msg, m.WParam, m.LParam);
                    m.Result = IntPtr.Zero;
                }
            }
            base.WndProc(ref m);
        }
    }
}
