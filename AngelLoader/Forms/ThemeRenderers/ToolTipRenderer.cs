using System;
using System.Drawing;
using AngelLoader.Forms.CustomControls;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms.ThemeRenderers
{
    internal sealed class ToolTipRenderer : ThemeRenderer
    {
        private protected override string CLSID { get; } = "ToolTip";

        internal override bool Enabled => Misc.Config.DarkMode && ControlUtils.ToolTipsReflectable;

        internal override bool TryDrawThemeBackground(
            IntPtr hTheme,
            IntPtr hdc,
            int iPartId,
            int iStateId,
            ref Native.RECT pRect,
            ref Native.RECT pClipRect)
        {
            using var g = Graphics.FromHdc(hdc);
            if (iPartId is Native.TTP_STANDARD or Native.TTP_STANDARDTITLE)
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

        internal override bool TryGetThemeColor(
            IntPtr hTheme,
            int iPartId,
            int iStateId,
            int iPropId,
            out int pColor)
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
