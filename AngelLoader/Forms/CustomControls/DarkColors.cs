﻿using System.Drawing;

namespace AngelLoader.Forms.CustomControls
{
    public static class DarkColors
    {
        #region Fen

        // Scroll arrows:
        // Normal:   92,  92,  92 (GreySelection)
        // Hot:     122, 128, 132 (GreyHighlight)
        // Pressed: 159, 178, 196 (ActiveControl)

        public static readonly Color Fen_DarkBackground = Color.FromArgb(32, 32, 32);
        public static readonly Color Fen_DarkForeground = Color.FromArgb(200, 200, 200);

        public static readonly Color Fen_ControlBackground = Color.FromArgb(48, 48, 48);
        public static readonly Color Fen_DeselectedTabBackground = Color.FromArgb(44, 44, 44);
        public static readonly Color Fen_HotTabBackground = Color.FromArgb(62, 62, 62);

        public static readonly Color Fen_RedHighlight = Color.FromArgb(64, 24, 24);

        public static readonly Color Fen_DGVCellBorders = Color.FromArgb(64, 64, 72);

        public static readonly Color Fen_DGVColumnHeaderHighlight = Color.FromArgb(77, 90, 114);
        public static readonly Color Fen_DGVColumnHeaderPressed = Color.FromArgb(82, 103, 142);

        #endregion

        #region DarkUI

        public static readonly Color GreyBackground = Color.FromArgb(60, 63, 65);
        public static readonly Color HeaderBackground = Color.FromArgb(57, 60, 62);
        public static readonly Color BlueBackground = Color.FromArgb(66, 77, 95);
        public static readonly Color DarkBlueBackground = Color.FromArgb(52, 57, 66);
        public static readonly Color DarkBackground = Color.FromArgb(43, 43, 43);
        public static readonly Color MediumBackground = Color.FromArgb(49, 51, 53);
        public static readonly Color LightBackground = Color.FromArgb(69, 73, 74);
        public static readonly Color LighterBackground = Color.FromArgb(95, 101, 102);
        public static readonly Color LightestBackground = Color.FromArgb(178, 178, 178);
        public static readonly Color LightBorder = Color.FromArgb(81, 81, 81);
        public static readonly Color DarkBorder = Color.FromArgb(51, 51, 51);
        public static readonly Color LightText = Color.FromArgb(220, 220, 220);
        public static readonly Color DisabledText = Color.FromArgb(153, 153, 153);
        public static readonly Color BlueHighlight = Color.FromArgb(104, 151, 187);
        public static readonly Color BlueSelection = Color.FromArgb(75, 110, 175);
        public static readonly Color GreyHighlight = Color.FromArgb(122, 128, 132);
        public static readonly Color GreySelection = Color.FromArgb(92, 92, 92);
        public static readonly Color DarkGreySelection = Color.FromArgb(82, 82, 82);
        public static readonly Color DarkBlueBorder = Color.FromArgb(51, 61, 78);
        public static readonly Color LightBlueBorder = Color.FromArgb(86, 97, 114);
        public static readonly Color ActiveControl = Color.FromArgb(159, 178, 196);

        #endregion

        #region Pens

        public static readonly Pen Fen_DarkBackgroundPen = new Pen(Fen_DarkBackground);
        public static readonly Pen Fen_DarkForegroundPen = new Pen(Fen_DarkForeground);
        public static readonly Pen Fen_ControlBackgroundPen = new Pen(Fen_ControlBackground);
        public static readonly Pen Fen_DeselectedTabBackgroundPen = new Pen(Fen_DeselectedTabBackground);
        public static readonly Pen Fen_HotTabBackgroundPen = new Pen(Fen_HotTabBackground);
        public static readonly Pen Fen_RedHighlightPen = new Pen(Fen_RedHighlight);
        public static readonly Pen Fen_DGVCellBordersPen = new Pen(Fen_DGVCellBorders);
        public static readonly Pen Fen_DGVColumnHeaderHighlightPen = new Pen(Fen_DGVColumnHeaderHighlight);
        public static readonly Pen Fen_DGVColumnHeaderPressedPen = new Pen(Fen_DGVColumnHeaderPressed);

        public static readonly Pen GreyBackgroundPen = new Pen(GreyBackground);
        public static readonly Pen HeaderBackgroundPen = new Pen(HeaderBackground);
        public static readonly Pen BlueBackgroundPen = new Pen(BlueBackground);
        public static readonly Pen DarkBlueBackgroundPen = new Pen(DarkBlueBackground);
        public static readonly Pen DarkBackgroundPen = new Pen(DarkBackground);
        public static readonly Pen MediumBackgroundPen = new Pen(MediumBackground);
        public static readonly Pen LightBackgroundPen = new Pen(LightBackground);
        public static readonly Pen LighterBackgroundPen = new Pen(LighterBackground);
        public static readonly Pen LightestBackgroundPen = new Pen(LightestBackground);
        public static readonly Pen LightBorderPen = new Pen(LightBorder);
        public static readonly Pen DarkBorderPen = new Pen(DarkBorder);
        public static readonly Pen LightTextPen = new Pen(LightText);
        public static readonly Pen DisabledTextPen = new Pen(DisabledText);
        public static readonly Pen BlueHighlightPen = new Pen(BlueHighlight);
        public static readonly Pen BlueSelectionPen = new Pen(BlueSelection);
        public static readonly Pen GreyHighlightPen = new Pen(GreyHighlight);
        public static readonly Pen GreySelectionPen = new Pen(GreySelection);
        public static readonly Pen DarkGreySelectionPen = new Pen(DarkGreySelection);
        public static readonly Pen DarkBlueBorderPen = new Pen(DarkBlueBorder);
        public static readonly Pen LightBlueBorderPen = new Pen(LightBlueBorder);
        public static readonly Pen ActiveControlPen = new Pen(ActiveControl);

        #endregion

        #region Brushes

        public static readonly SolidBrush Fen_DarkBackgroundBrush = new SolidBrush(Fen_DarkBackground);
        public static readonly SolidBrush Fen_DarkForegroundBrush = new SolidBrush(Fen_DarkForeground);
        public static readonly SolidBrush Fen_ControlBackgroundBrush = new SolidBrush(Fen_ControlBackground);
        public static readonly SolidBrush Fen_DeselectedTabBackgroundBrush = new SolidBrush(Fen_DeselectedTabBackground);
        public static readonly SolidBrush Fen_HotTabBackgroundBrush = new SolidBrush(Fen_HotTabBackground);
        public static readonly SolidBrush Fen_RedHighlightBrush = new SolidBrush(Fen_RedHighlight);
        public static readonly SolidBrush Fen_DGVCellBordersBrush = new SolidBrush(Fen_DGVCellBorders);
        public static readonly SolidBrush Fen_DGVColumnHeaderHighlightBrush = new SolidBrush(Fen_DGVColumnHeaderHighlight);
        public static readonly SolidBrush Fen_DGVColumnHeaderPressedBrush = new SolidBrush(Fen_DGVColumnHeaderPressed);

        public static readonly SolidBrush GreyBackgroundBrush = new SolidBrush(GreyBackground);
        public static readonly SolidBrush HeaderBackgroundBrush = new SolidBrush(HeaderBackground);
        public static readonly SolidBrush BlueBackgroundBrush = new SolidBrush(BlueBackground);
        public static readonly SolidBrush DarkBlueBackgroundBrush = new SolidBrush(DarkBlueBackground);
        public static readonly SolidBrush DarkBackgroundBrush = new SolidBrush(DarkBackground);
        public static readonly SolidBrush MediumBackgroundBrush = new SolidBrush(MediumBackground);
        public static readonly SolidBrush LightBackgroundBrush = new SolidBrush(LightBackground);
        public static readonly SolidBrush LighterBackgroundBrush = new SolidBrush(LighterBackground);
        public static readonly SolidBrush LightestBackgroundBrush = new SolidBrush(LightestBackground);
        public static readonly SolidBrush LightBorderBrush = new SolidBrush(LightBorder);
        public static readonly SolidBrush DarkBorderBrush = new SolidBrush(DarkBorder);
        public static readonly SolidBrush LightTextBrush = new SolidBrush(LightText);
        public static readonly SolidBrush DisabledTextBrush = new SolidBrush(DisabledText);
        public static readonly SolidBrush BlueHighlightBrush = new SolidBrush(BlueHighlight);
        public static readonly SolidBrush BlueSelectionBrush = new SolidBrush(BlueSelection);
        public static readonly SolidBrush GreyHighlightBrush = new SolidBrush(GreyHighlight);
        public static readonly SolidBrush GreySelectionBrush = new SolidBrush(GreySelection);
        public static readonly SolidBrush DarkGreySelectionBrush = new SolidBrush(DarkGreySelection);
        public static readonly SolidBrush DarkBlueBorderBrush = new SolidBrush(DarkBlueBorder);
        public static readonly SolidBrush LightBlueBorderBrush = new SolidBrush(LightBlueBorder);
        public static readonly SolidBrush ActiveControlBrush = new SolidBrush(ActiveControl);

        #endregion
    }
}
