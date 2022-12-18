using System;
using System.Drawing;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;

namespace AngelLoader.Forms.ThemeRenderers;

internal sealed class TreeViewRenderer : ThemeRenderer
{
    private protected override string CLSID { get; } = "TreeView";

    internal override bool Enabled { get; } = true;

    internal override bool TryDrawThemeBackground(
        IntPtr hTheme,
        IntPtr hdc,
        int iPartId,
        int iStateId,
        ref Native.RECT pRect,
        ref Native.RECT pClipRect)
    {
        if (iPartId is not Native.TVP_GLYPH and not Native.TVP_HOTGLYPH) return false;

        using Graphics g = Graphics.FromHdc(hdc);

        Rectangle rect = pRect.ToRectangle();

        Misc.Direction direction = iStateId is Native.GLPS_CLOSED or Native.HGLPS_CLOSED
            ? Misc.Direction.Right
            : Misc.Direction.Down;

        Images.PaintArrow7x4(
            g,
            direction,
            rect,
            pen: Global.Config.DarkMode ? DarkColors.LightTextPen : SystemPens.WindowText);

        return true;
    }
}