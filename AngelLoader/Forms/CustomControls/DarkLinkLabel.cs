﻿using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkLinkLabel : LinkLabel, IDarkable
    {
        private Color? _origForeColor;
        private Color? _origLinkColor;
        private Color? _origActiveLinkColor;
        private Color? _origVisitedLinkColor;
        private Color? _origDisabledLinkColor;

        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
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
}
