using System;
using System.Drawing;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;

namespace AngelLoader.Forms.ThemeRenderers;

internal sealed class ScrollBarRenderer : ThemeRenderer
{
    private protected override string CLSID { get; } = "Scrollbar";

    internal override bool Enabled => Global.Config.DarkMode;

    internal override bool TryDrawThemeBackground(
        IntPtr hTheme,
        IntPtr hdc,
        int iPartId,
        int iStateId,
        ref Native.RECT pRect,
        ref Native.RECT pClipRect)
    {
        using Graphics g = Graphics.FromHdc(hdc);

        #region Background

        Rectangle rect = pRect.ToRectangle();

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
                        rect = rect with { Y = rect.Y + 1, Height = rect.Height - 2 };
                        break;
                    case Native.SBP_THUMBBTNVERT:
                        g.DrawLine(DarkColors.DarkBackgroundPen, rect.X, rect.Y, rect.X, rect.Bottom);
                        g.DrawLine(DarkColors.DarkBackgroundPen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom);
                        rect = rect with { X = rect.X + 1, Width = rect.Width - 2 };
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
            Pen pen;
            switch (iStateId)
            {
                case Native.ABS_UPPRESSED:
                case Native.ABS_DOWNPRESSED:
                case Native.ABS_LEFTPRESSED:
                case Native.ABS_RIGHTPRESSED:
                    pen = DarkColors.ActiveControlPen;
                    break;
                case Native.ABS_UPDISABLED:
                case Native.ABS_DOWNDISABLED:
                case Native.ABS_LEFTDISABLED:
                case Native.ABS_RIGHTDISABLED:
                    pen = DarkColors.GreySelectionPen;
                    break;
                case Native.ABS_UPHOT:
                case Native.ABS_DOWNHOT:
                case Native.ABS_LEFTHOT:
                case Native.ABS_RIGHTHOT:
                    pen = DarkColors.GreyHighlightPen;
                    break;
#if false
                case Native.ABS_UPNORMAL:
                case Native.ABS_DOWNNORMAL:
                case Native.ABS_LEFTNORMAL:
                case Native.ABS_RIGHTNORMAL:
#endif
                default:
                    pen = DarkColors.GreySelectionPen;
                    break;
            }

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
#if false
                case Native.ABS_DOWNNORMAL:
                case Native.ABS_DOWNHOT:
                case Native.ABS_DOWNPRESSED:
                case Native.ABS_DOWNHOVER:
                case Native.ABS_DOWNDISABLED:
#endif
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
        // This is for scrollbar vert/horz corners on Win10 (and maybe Win8? Haven't tested it).
        // This is the ONLY way that works on those versions.
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