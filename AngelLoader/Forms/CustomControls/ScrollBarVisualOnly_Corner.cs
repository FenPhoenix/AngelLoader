using System.Drawing;
using System.Windows.Forms;
using DarkUI.Controls;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class ScrollBarVisualOnly_Corner : Control
    {
        private readonly IDarkableScrollableNative _owner;
        private bool _addedToControls;

        public ScrollBarVisualOnly_Corner(IDarkableScrollableNative owner)
        {
            Visible = false;
            _owner = owner;
            BackColor = DarkUI.Config.Colors.DarkBackground;
            // DON'T anchor it bottom-right, or we get visual glitches and chaos
        }

        public void AddToParent()
        {
            Control parent;
            if (!_addedToControls && (parent = _owner.ClosestAddableParent) != null)
            {
                parent.Controls.Add(this);
                Size = new Size(SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);
                BringToFront();
                _addedToControls = true;
            }
        }
    }
}
