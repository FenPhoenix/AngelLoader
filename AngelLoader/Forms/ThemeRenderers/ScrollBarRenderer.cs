using System;
using System.Drawing;
using AngelLoader.Forms.CustomControls;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms.ThemeRenderers
{
    internal sealed class ScrollBarRenderer : ThemeRenderer
    {
        private protected override string CLSID { get; } = "Scrollbar";

        internal override bool TryDrawThemeBackground(IntPtr hTheme,
            IntPtr hdc,
            int iPartId,
            int iStateId,
            ref Native.RECT pRect,
            ref Native.RECT pClipRect)
        {
            using var g = Graphics.FromHdc(hdc);

            #region Background

            var rect = Rectangle.FromLTRB(pRect.left, pRect.top, pRect.right, pRect.bottom);

            Brush brush;
            switch (iPartId)
            {
                case Native.SBP_ARROWBTN:
                    brush = DarkColors.DarkBackgroundBrush;
                    break;
                case Native.SBP_GRIPPERHORZ:
                case Native.SBP_GRIPPERVERT:
                    // The "gripper" is a subset of the thumb, except sometimes it extends outside of it and
                    // causes problems with our thumb width correction, so just don't draw it
                    return true;
                case Native.SBP_THUMBBTNHORZ:
                case Native.SBP_THUMBBTNVERT:

                    #region Correct the thumb width

                    // Match Windows behavior - the thumb is 1px in from each side
                    // The "gripper" rect gives us the right width, but the wrong length
                    switch (iPartId)
                    {
                        case Native.SBP_THUMBBTNHORZ:
                            g.DrawLine(DarkColors.DarkBackgroundPen, rect.X, rect.Y, rect.Right, rect.Y);
                            g.DrawLine(DarkColors.DarkBackgroundPen, rect.X, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
                            rect = new Rectangle(rect.X, rect.Y + 1, rect.Width, rect.Height - 2);
                            break;
                        case Native.SBP_THUMBBTNVERT:
                            g.DrawLine(DarkColors.DarkBackgroundPen, rect.X, rect.Y, rect.X, rect.Bottom);
                            g.DrawLine(DarkColors.DarkBackgroundPen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom);
                            rect = new Rectangle(rect.X + 1, rect.Y, rect.Width - 2, rect.Height);
                            break;
                    }

                    #endregion

                    brush = iStateId switch
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

            g.FillRectangle(brush, rect);

            #endregion

            #region Arrow

            if (iPartId == Native.SBP_ARROWBTN)
            {
                Color foreColor;
                switch (iStateId)
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
                switch (iStateId)
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

                Images.PaintArrow7x4(
                    g,
                    direction,
                    rect,
                    pen: pen
                );
            }

            #endregion

            return true;
        }

        internal override bool TryGetThemeColor(
            IntPtr hTheme,
            int iPartId,
            int iStateId,
            int iPropId,
            out int pColor)
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
