using System.Drawing;

namespace AngelLoader.Forms.CustomControls
{
    public static class DarkColors
    {
        #region Fen

        // Scroll arrows:
        // Normal:   92,  92,  92 (GreySelection)
        // Hot:     122, 128, 132 (GreyHighlight)
        // Pressed: 159, 178, 196 (ActiveControl)

        public static Color Fen_DarkBackground = Color.FromArgb(32, 32, 32);
        public static Color Fen_DarkForeground = Color.FromArgb(200, 200, 200);

        public static Color Fen_ControlBackground = Color.FromArgb(48, 48, 48);
        public static Color Fen_DeselectedTabBackground = Color.FromArgb(44, 44, 44);

        public static Color Fen_RedHighlight = Color.FromArgb(64, 24, 24);

        #endregion

        #region DarkUI

        public static Color GreyBackground = Color.FromArgb(60, 63, 65);

        public static Color HeaderBackground = Color.FromArgb(57, 60, 62);

        public static Color BlueBackground = Color.FromArgb(66, 77, 95);

        public static Color DarkBlueBackground = Color.FromArgb(52, 57, 66);

        public static Color DarkBackground = Color.FromArgb(43, 43, 43);

        public static Color MediumBackground = Color.FromArgb(49, 51, 53);

        public static Color LightBackground = Color.FromArgb(69, 73, 74);

        public static Color LighterBackground = Color.FromArgb(95, 101, 102);

        public static Color LightestBackground = Color.FromArgb(178, 178, 178);

        public static Color LightBorder = Color.FromArgb(81, 81, 81);

        public static Color DarkBorder = Color.FromArgb(51, 51, 51);

        public static Color LightText = Color.FromArgb(220, 220, 220);

        public static Color DisabledText = Color.FromArgb(153, 153, 153);

        public static Color BlueHighlight = Color.FromArgb(104, 151, 187);

        public static Color BlueSelection = Color.FromArgb(75, 110, 175);

        public static Color GreyHighlight = Color.FromArgb(122, 128, 132);

        public static Color GreySelection = Color.FromArgb(92, 92, 92);

        public static Color DarkGreySelection = Color.FromArgb(82, 82, 82);

        public static Color DarkBlueBorder = Color.FromArgb(51, 61, 78);

        public static Color LightBlueBorder = Color.FromArgb(86, 97, 114);

        public static Color ActiveControl = Color.FromArgb(159, 178, 196);

        #endregion

        #region Pens

        public static readonly Pen Fen_DarkBackgroundPen = new Pen(Fen_DarkBackground);
        public static readonly Pen Fen_DarkForegroundPen = new Pen(Fen_DarkForeground);
        public static readonly Pen Fen_ControlBackgroundPen = new Pen(Fen_ControlBackground);
        public static readonly Pen Fen_DeselectedTabBackgroundPen = new Pen(Fen_DeselectedTabBackground);
        public static readonly Pen Fen_RedHighlightPen = new Pen(Fen_RedHighlight);

        public static Pen GreyBackgroundPen = new Pen(GreyBackground);
        public static Pen HeaderBackgroundPen = new Pen(HeaderBackground);
        public static Pen BlueBackgroundPen = new Pen(BlueBackground);
        public static Pen DarkBlueBackgroundPen = new Pen(DarkBlueBackground);
        public static Pen DarkBackgroundPen = new Pen(DarkBackground);
        public static Pen MediumBackgroundPen = new Pen(MediumBackground);
        public static Pen LightBackgroundPen = new Pen(LightBackground);
        public static Pen LighterBackgroundPen = new Pen(LighterBackground);
        public static Pen LightestBackgroundPen = new Pen(LightestBackground);
        public static Pen LightBorderPen = new Pen(LightBorder);
        public static Pen DarkBorderPen = new Pen(DarkBorder);
        public static Pen LightTextPen = new Pen(LightText);
        public static Pen DisabledTextPen = new Pen(DisabledText);
        public static Pen BlueHighlightPen = new Pen(BlueHighlight);
        public static Pen BlueSelectionPen = new Pen(BlueSelection);
        public static Pen GreyHighlightPen = new Pen(GreyHighlight);
        public static Pen GreySelectionPen = new Pen(GreySelection);
        public static Pen DarkGreySelectionPen = new Pen(DarkGreySelection);
        public static Pen DarkBlueBorderPen = new Pen(DarkBlueBorder);
        public static Pen LightBlueBorderPen = new Pen(LightBlueBorder);
        public static Pen ActiveControlPen = new Pen(ActiveControl);

        #endregion

        #region Brushes

        public static readonly SolidBrush Fen_DarkBackgroundBrush = new SolidBrush(Fen_DarkBackground);
        public static readonly SolidBrush Fen_DarkForegroundBrush = new SolidBrush(Fen_DarkForeground);
        public static readonly SolidBrush Fen_ControlBackgroundBrush = new SolidBrush(Fen_ControlBackground);
        public static readonly SolidBrush Fen_DeselectedTabBackgroundBrush = new SolidBrush(Fen_DeselectedTabBackground);
        public static readonly SolidBrush Fen_RedHighlightBrush = new SolidBrush(Fen_RedHighlight);

        public static SolidBrush GreyBackgroundBrush = new SolidBrush(GreyBackground);
        public static SolidBrush HeaderBackgroundBrush = new SolidBrush(HeaderBackground);
        public static SolidBrush BlueBackgroundBrush = new SolidBrush(BlueBackground);
        public static SolidBrush DarkBlueBackgroundBrush = new SolidBrush(DarkBlueBackground);
        public static SolidBrush DarkBackgroundBrush = new SolidBrush(DarkBackground);
        public static SolidBrush MediumBackgroundBrush = new SolidBrush(MediumBackground);
        public static SolidBrush LightBackgroundBrush = new SolidBrush(LightBackground);
        public static SolidBrush LighterBackgroundBrush = new SolidBrush(LighterBackground);
        public static SolidBrush LightestBackgroundBrush = new SolidBrush(LightestBackground);
        public static SolidBrush LightBorderBrush = new SolidBrush(LightBorder);
        public static SolidBrush DarkBorderBrush = new SolidBrush(DarkBorder);
        public static SolidBrush LightTextBrush = new SolidBrush(LightText);
        public static SolidBrush DisabledTextBrush = new SolidBrush(DisabledText);
        public static SolidBrush BlueHighlightBrush = new SolidBrush(BlueHighlight);
        public static SolidBrush BlueSelectionBrush = new SolidBrush(BlueSelection);
        public static SolidBrush GreyHighlightBrush = new SolidBrush(GreyHighlight);
        public static SolidBrush GreySelectionBrush = new SolidBrush(GreySelection);
        public static SolidBrush DarkGreySelectionBrush = new SolidBrush(DarkGreySelection);
        public static SolidBrush DarkBlueBorderBrush = new SolidBrush(DarkBlueBorder);
        public static SolidBrush LightBlueBorderBrush = new SolidBrush(LightBlueBorder);
        public static SolidBrush ActiveControlBrush = new SolidBrush(ActiveControl);

        #endregion
    }
}
