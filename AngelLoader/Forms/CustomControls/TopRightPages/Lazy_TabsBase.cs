using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    // @VBL: Lots of it in the lazy-loaded top-right tabs now.
    /*
    @TopLazy(hide the entire tab control when top-right is collapsed):
    We could just hide the tab control and then say don't construct the selected tab if the tab control is not
    visible. Then on tab control Visible changed to true, we then construct the selected tab.
    */
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

        public virtual void SetOwner(MainForm owner) { }

        public virtual void Construct() { }

        public virtual void Localize() { }

        public virtual void UpdatePage() { }


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
