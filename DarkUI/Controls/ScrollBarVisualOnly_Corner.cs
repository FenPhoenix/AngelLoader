﻿using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.Controls
{
    public sealed class ScrollBarVisualOnly_Corner : Control
    {
        private readonly IDarkableScrollableNative _owner;
        private bool _addedToControls;

        public ScrollBarVisualOnly_Corner(IDarkableScrollableNative owner)
        {
            Visible = false;
            _owner = owner;
            BackColor = Config.Colors.DarkBackground;
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        }

        public void AddToParent()
        {
            Control parent;
            if (!_addedToControls && (parent = _owner.ClosestAddableParent) != null)
            {
                parent.Controls.Add(this);
                Size = new Size(SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);
                _addedToControls = true;
            }
        }
    }
}
