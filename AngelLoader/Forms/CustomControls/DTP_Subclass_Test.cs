using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

        private int saveCount = 0;

        public DTP_Subclass_Test()
        {
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Native.SetWindowTheme(Handle, "", "");
        }

        // TODO: @DarkMode(DateTimePicker): Paint over the Win98-looking unthemed parts (border, button, etc.)

        protected override void WndProc(ref Message m)
        {
            //if (m.Msg == Native.WM_PAINT)
            //{
            //    Trace.WriteLine("PAINT");
            //}
            //else if (m.Msg == Native.WM_NCPAINT)
            //{
            //    Trace.WriteLine("NC_PAINT");
            //}

            base.WndProc(ref m);
            return;

            if (m.Msg == Native.WM_PAINT)
            {
                Trace.WriteLine(_random.Next() + "DTP: WM_PAINT");
                using Native.DeviceContext dc = new Native.DeviceContext(Handle);
                using Graphics g = Graphics.FromHdc(dc.DC);

                //using Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
                using Bitmap bmp = new Bitmap(Width, Height, g);
                //using Graphics bg = Graphics.FromImage(bmp);

                //var rect = new Rectangle(2, 3, ClientRectangle.Width, ClientRectangle.Height - 3);

                DrawToBitmap(bmp, ClientRectangle);

                //bmp.Save(@"C:\_DTP_\save_" + saveCount + ".png", ImageFormat.Png);
                //saveCount++;
            }
            base.WndProc(ref m);
        }
    }
}
