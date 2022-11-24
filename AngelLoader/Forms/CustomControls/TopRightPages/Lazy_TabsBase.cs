using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public class Lazy_TabsBase : DarkTabPageCustom
    {
        private protected MainForm _owner = null!;

        private protected bool _constructed;

        private readonly List<KeyValuePair<Control, ControlUtils.ControlOriginalColors?>> _controlColors = new();

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool DarkModeEnabled
        {
            get => base.DarkModeEnabled;
            set
            {
                if (base.DarkModeEnabled == value) return;
                base.DarkModeEnabled = value;

                if (!_constructed) return;

                RefreshTheme();
            }
        }

        private protected void RefreshTheme()
        {
            ControlUtils.SetTheme(this, _controlColors, base.DarkModeEnabled ? VisualTheme.Dark : VisualTheme.Classic);
        }

        private protected bool OnStartupAndThisTabIsSelected()
        {
            return !_constructed && !_owner.Visible && _owner.TopRightTabControl.SelectedTab == this;
        }
    }
}
