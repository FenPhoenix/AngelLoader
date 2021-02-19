﻿using System;
using System.Drawing;
using System.Windows.Forms;
using DarkUI.Config;

namespace DarkUI.Controls
{
    public class DarkTextBox : TextBox, IDarkableScrollableNative
    {
        private bool _origValuesStored;
        private Color _origForeColor;
        private Color _origBackColor;
        private Padding _origPadding;
        private BorderStyle _origBorderStyle;

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                if (_darkModeEnabled)
                {
                    if (!_origValuesStored)
                    {
                        _origForeColor = ForeColor;
                        _origBackColor = BackColor;
                        _origPadding = Padding;
                        _origBorderStyle = BorderStyle;
                        _origValuesStored = true;
                    }

                    BackColor = Colors.LightBackground;
                    ForeColor = Colors.LightText;
                    Padding = new Padding(2, 2, 2, 2);
                    BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    if (_origValuesStored)
                    {
                        BackColor = _origBackColor;
                        ForeColor = _origForeColor;
                        Padding = _origPadding;
                        BorderStyle = _origBorderStyle;
                    }
                }
                DarkModeChanged?.Invoke(this, new DarkModeChangedEventArgs(_darkModeEnabled));
            }
        }

        public new bool Multiline
        {
            get => base.Multiline;
            set
            {
                base.Multiline = value;
                if (value && VerticalVisualScrollBar == null && HorizontalVisualScrollBar == null && VisualScrollBarCorner == null)
                {
                    VerticalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: true, passMouseWheel: true);
                    HorizontalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: false, passMouseWheel: true);
                    VisualScrollBarCorner = new ScrollBarVisualOnly_Corner(this);
                }
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Visible / Show / Hide overrides

        public new bool Visible
        {
            get => base.Visible;
            set
            {
                if (value)
                {
                    // Do this before setting the Visible value to avoid the classic-bar-flicker
                    VerticalVisualScrollBar.ForceSetVisibleState(true);
                    base.Visible = value;
                }
                else
                {
                    base.Visible = value;
                    VerticalVisualScrollBar.ForceSetVisibleState(false);
                }
            }
        }

        public new void Show()
        {
            VerticalVisualScrollBar.ForceSetVisibleState(true);
            base.Show();
        }

        public new void Hide()
        {
            base.Hide();
            VerticalVisualScrollBar.ForceSetVisibleState(false);
        }

        #endregion
        public bool Suspended { get; set; }
        public ScrollBarVisualOnly_Native VerticalVisualScrollBar { get; private set; }
        public ScrollBarVisualOnly_Native HorizontalVisualScrollBar { get; private set; }
        public ScrollBarVisualOnly_Corner VisualScrollBarCorner { get; private set; }
        public event EventHandler Scroll;
        public Control ClosestAddableParent => Parent;
        public event EventHandler<DarkModeChangedEventArgs> DarkModeChanged;
        public event EventHandler VisibilityChanged;
    }
}
