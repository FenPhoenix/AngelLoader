﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class TabPageCustom : TabPage, IDarkableScrollableNative
    {
        private Color? _origBackColor;

        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (_darkModeEnabled)
                {
                    _origBackColor ??= BackColor;
                    BackColor = DarkColors.Fen_ControlBackground;
                }
                else
                {
                    if (_origBackColor != null) BackColor = (Color)_origBackColor;
                }
                DarkModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Browsable(false)]
        public ScrollBarVisualOnly_Native VerticalVisualScrollBar { get; }
        [Browsable(false)]
        public ScrollBarVisualOnly_Native HorizontalVisualScrollBar { get; }
        [Browsable(false)]
        public ScrollBarVisualOnly_Corner VisualScrollBarCorner { get; }
        [Browsable(false)]
        public bool Suspended { get; set; }
        public new event EventHandler? Scroll;
        [Browsable(false)]
        public event EventHandler? DarkModeChanged;
        [Browsable(false)]
        public event EventHandler? RefreshIfNeededForceCorner;
        [Browsable(false)]
        public Control? ClosestAddableParent => Parent.Parent;

        public TabPageCustom()
        {
            VerticalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: true, passMouseWheel: true);
            HorizontalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: false, passMouseWheel: true);
            VisualScrollBarCorner = new ScrollBarVisualOnly_Corner(this);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            if (_darkModeEnabled) Scroll?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Native.WM_NCPAINT:
                    if (_darkModeEnabled)
                    {
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                    break;
                case Native.WM_PAINT:
                    if (_darkModeEnabled)
                    {
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                    }
                    base.WndProc(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}
