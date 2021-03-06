﻿using System;
using System.Drawing;
using AngelLoader.Forms.CustomControls;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms.ThemeRenderers
{
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

            using var g = Graphics.FromHdc(hdc);

            var rect = Rectangle.FromLTRB(pRect.left, pRect.top, pRect.right, pRect.bottom);

            Misc.Direction direction = iStateId is Native.GLPS_CLOSED or Native.HGLPS_CLOSED
                ? Misc.Direction.Right
                : Misc.Direction.Down;

            Images.PaintArrow7x4(
                g,
                direction,
                rect,
                pen: Misc.Config.DarkMode ? DarkColors.LightTextPen : SystemPens.WindowText);

            return true;
        }
    }
}
