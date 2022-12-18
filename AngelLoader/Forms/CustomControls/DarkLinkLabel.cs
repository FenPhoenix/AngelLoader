using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkLinkLabel : LinkLabel, IDarkable
{
    private Color? _origForeColor;
    private Color? _origLinkColor;
    private Color? _origActiveLinkColor;
    private Color? _origVisitedLinkColor;
    private Color? _origDisabledLinkColor;

#if DEBUG

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseMnemonic { get => base.UseMnemonic; set => base.UseMnemonic = value; }

#endif

    public DarkLinkLabel() => UseMnemonic = false;

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            if (_darkModeEnabled)
            {
                _origForeColor ??= ForeColor;
                _origLinkColor ??= LinkColor;
                _origActiveLinkColor ??= ActiveLinkColor;
                _origVisitedLinkColor ??= VisitedLinkColor;
                _origDisabledLinkColor ??= DisabledLinkColor;

                ForeColor = DarkColors.LightText;
                LinkColor = DarkColors.Fen_Hyperlink;
                ActiveLinkColor = DarkColors.Fen_HyperlinkPressed;
                VisitedLinkColor = DarkColors.Fen_Hyperlink;
                DisabledLinkColor = DarkColors.DisabledText;
            }
            else
            {
                if (_origForeColor != null) ForeColor = (Color)_origForeColor;
                if (_origLinkColor != null) LinkColor = (Color)_origLinkColor;
                if (_origActiveLinkColor != null) ActiveLinkColor = (Color)_origActiveLinkColor;
                if (_origVisitedLinkColor != null) VisitedLinkColor = (Color)_origVisitedLinkColor;
                if (_origDisabledLinkColor != null) DisabledLinkColor = (Color)_origDisabledLinkColor;
            }
        }
    }
}