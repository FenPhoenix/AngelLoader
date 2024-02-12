using System.Drawing;

namespace Update;

public static class DarkColors
{
    #region Fen

    public static readonly Color Fen_DarkBackground = Color.FromArgb(32, 32, 32);
    public static readonly Color Fen_ControlBackground = Color.FromArgb(48, 48, 48);

    #endregion

    #region DarkUI

    public static readonly Color BlueBackground = Color.FromArgb(66, 77, 95);
    public static readonly Color DarkBackground = Color.FromArgb(43, 43, 43);
    public static readonly Color LightBackground = Color.FromArgb(69, 73, 74);
    public static readonly Color LightText = Color.FromArgb(220, 220, 220);
    public static readonly Color DisabledText = Color.FromArgb(153, 153, 153);
    public static readonly Color BlueHighlight = Color.FromArgb(104, 151, 187);
    public static readonly Color GreySelection = Color.FromArgb(92, 92, 92);
    public static readonly Color DarkGreySelection = Color.FromArgb(82, 82, 82);

    #endregion

    #region Pens

    public static readonly Pen BlueHighlightPen = new Pen(BlueHighlight);
    public static readonly Pen GreySelectionPen = new Pen(GreySelection);

    #endregion

    #region Brushes

    public static readonly SolidBrush Fen_ControlBackgroundBrush = new SolidBrush(Fen_ControlBackground);
    public static readonly SolidBrush LightBackgroundBrush = new SolidBrush(LightBackground);
    public static readonly SolidBrush LightTextBrush = new SolidBrush(LightText);
    public static readonly SolidBrush DisabledTextBrush = new SolidBrush(DisabledText);

    #endregion
}
