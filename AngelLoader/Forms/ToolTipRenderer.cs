using System;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms
{
    internal static class ToolTipRenderer
    {
        internal static IntPtr HTheme { get; private set; }

        internal static void Reload()
        {
            Native.CloseThemeData(HTheme);
            using var c = new Control();
            HTheme = Native.OpenThemeData(c.Handle, "ToolTip");
        }

        internal static bool Paint(IntPtr hdc, int partId, Native.RECT pRect)
        {
            using var g = Graphics.FromHdc(hdc);
            if (partId == Native.TTP_STANDARD || partId == Native.TTP_STANDARDTITLE)
            {
                var rect = Rectangle.FromLTRB(pRect.left, pRect.top, pRect.right, pRect.bottom);

                g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, rect);
                g.DrawRectangle(
                    DarkColors.GreySelectionPen,
                    new Rectangle(
                        rect.X,
                        rect.Y,
                        rect.Width - 1,
                        rect.Height - 1
                    ));
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static bool TryGetThemeColor(int iPropId, out int pColor)
        {
            if (iPropId == Native.TMT_TEXTCOLOR)
            {
                pColor = ColorTranslator.ToWin32(DarkColors.LightText);
                return true;
            }
            else
            {
                pColor = 0;
                return false;
            }
        }
    }
}
