using System;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms
{
    internal static class ScrollBarPainter
    {
        internal static IntPtr HTheme { get; private set; }

        internal static void Reload()
        {
            Native.CloseThemeData(HTheme);
            using var c = new Control();
            HTheme = Native.OpenThemeData(c.Handle, "Scrollbar");
        }

        internal static bool Paint(IntPtr hdc, int partId, int stateId, Native.RECT pRect)
        {
            using var g = Graphics.FromHdc(hdc);

            #region Background

            Brush brush;
            switch (partId)
            {
                case Native.SBP_ARROWBTN:
                    brush = DarkColors.DarkBackgroundBrush;
                    break;
                case Native.SBP_THUMBBTNHORZ:
                case Native.SBP_THUMBBTNVERT:
                case Native.SBP_GRIPPERHORZ:
                case Native.SBP_GRIPPERVERT:
                    brush = stateId switch
                    {
                        Native.SCRBS_NORMAL => DarkColors.GreySelectionBrush,
                        Native.SCRBS_HOVER => DarkColors.Fen_ThumbScrollBarHoverBrush,
                        Native.SCRBS_HOT => DarkColors.GreyHighlightBrush,
                        Native.SCRBS_PRESSED => DarkColors.ActiveControlBrush,
                        _ => DarkColors.GreySelectionBrush
                    };
                    break;
                default:
                    brush = DarkColors.DarkBackgroundBrush;
                    break;
            }

            var rect = Rectangle.FromLTRB(pRect.left, pRect.top, pRect.right, pRect.bottom);

            g.FillRectangle(brush, rect);

            #endregion

            #region Arrow

            if (partId == Native.SBP_ARROWBTN)
            {
                Color foreColor;
                switch (stateId)
                {
                    case Native.ABS_UPPRESSED:
                    case Native.ABS_DOWNPRESSED:
                    case Native.ABS_LEFTPRESSED:
                    case Native.ABS_RIGHTPRESSED:
                        foreColor = DarkColors.ActiveControl;
                        break;
                    case Native.ABS_UPDISABLED:
                    case Native.ABS_DOWNDISABLED:
                    case Native.ABS_LEFTDISABLED:
                    case Native.ABS_RIGHTDISABLED:
                        foreColor = DarkColors.GreySelection;
                        break;
                    case Native.ABS_UPHOT:
                    case Native.ABS_DOWNHOT:
                    case Native.ABS_LEFTHOT:
                    case Native.ABS_RIGHTHOT:
                        foreColor = DarkColors.GreyHighlight;
                        break;
                    case Native.ABS_UPNORMAL:
                    case Native.ABS_DOWNNORMAL:
                    case Native.ABS_LEFTNORMAL:
                    case Native.ABS_RIGHTNORMAL:
                    default:
                        foreColor = DarkColors.GreySelection;
                        break;
                }

                using var pen = new Pen(foreColor);

                Misc.Direction direction;
                switch (stateId)
                {
                    case Native.ABS_LEFTNORMAL:
                    case Native.ABS_LEFTHOT:
                    case Native.ABS_LEFTPRESSED:
                    case Native.ABS_LEFTHOVER:
                    case Native.ABS_LEFTDISABLED:
                        direction = Misc.Direction.Left;
                        break;
                    case Native.ABS_RIGHTNORMAL:
                    case Native.ABS_RIGHTHOT:
                    case Native.ABS_RIGHTPRESSED:
                    case Native.ABS_RIGHTHOVER:
                    case Native.ABS_RIGHTDISABLED:
                        direction = Misc.Direction.Right;
                        break;
                    case Native.ABS_UPNORMAL:
                    case Native.ABS_UPHOT:
                    case Native.ABS_UPPRESSED:
                    case Native.ABS_UPHOVER:
                    case Native.ABS_UPDISABLED:
                        direction = Misc.Direction.Up;
                        break;
                    case Native.ABS_DOWNNORMAL:
                    case Native.ABS_DOWNHOT:
                    case Native.ABS_DOWNPRESSED:
                    case Native.ABS_DOWNHOVER:
                    case Native.ABS_DOWNDISABLED:
                    default:
                        direction = Misc.Direction.Down;
                        break;
                }

                ControlPainter.PaintArrow7x4(
                    g,
                    direction,
                    rect,
                    pen: pen
                );
            }

            #endregion

            return true;
        }

        internal static bool TryGetThemeColor(int iPartId, int iPropId, out int pColor)
        {
            if (iPartId == Native.SBP_CORNER && iPropId == Native.TMT_FILLCOLOR)
            {
                pColor = ColorTranslator.ToWin32(DarkColors.DarkBackground);
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
