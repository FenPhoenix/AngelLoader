using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DTP_Subclass_Test : DateTimePicker
    {
        private readonly Random _random = new Random();

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.WM_PAINT)
            {
                Trace.WriteLine(_random.Next() + "DTP: WM_PAINT");
            }
            base.WndProc(ref m);
        }
    }
}
