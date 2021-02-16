using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Controls;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class TabPageCustom : TabPage, IDarkableScrollableNative
    {
        private readonly ScrollBarVisualOnly_Native VerticalVisualScrollBar;
        private readonly ScrollBarVisualOnly_Native HorizontalVisualScrollBar;

        private Color? _origBackColor;

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                if (_darkModeEnabled)
                {
                    _origBackColor ??= BackColor;
                    BackColor = DarkUI.Config.Colors.Fen_DarkBackground;
                }
                else
                {
                    if (_origBackColor != null) BackColor = (Color)_origBackColor;
                }
                DarkModeChanged?.Invoke(this, new DarkModeChangedEventArgs(_darkModeEnabled));
            }
        }

        public bool Suspended { get; set; }
        public new event EventHandler? Scroll;
        public event EventHandler<DarkModeChangedEventArgs>? DarkModeChanged;
        public event EventHandler? VisibilityChanged;
        public Control? ClosestAddableParent => Parent?.Parent;

        public TabPageCustom()
        {
            VerticalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: true, passMouseWheel: true);
            HorizontalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: false, passMouseWheel: true);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Scroll?.Invoke(this, EventArgs.Empty);
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
    }
}
