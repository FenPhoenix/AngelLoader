using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using AngelLoader.WinAPI.Dialogs;

namespace AngelLoader.Forms
{
    class ProgressBarRenderer
    {
        internal static IntPtr HTheme { get; private set; }

        internal static void Reload()
        {
            Native.CloseThemeData(HTheme);
            using var c = new Control();
            HTheme = Native.OpenThemeData(c.Handle, "Progress");
        }

        public bool RenderBackground(IntPtr hdc, int partId, int stateId, Native.RECT pRect)
        {
            using var g = Graphics.FromHdc(hdc);

            var rect = Rectangle.FromLTRB(pRect.left, pRect.top, pRect.right, pRect.bottom);

            switch (partId)
            {

            }

            return true;
        }
    }
}
