using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkToolTip : ToolTip, IDarkable
    {
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

                // Need to do this or else the text color doesn't update
                ControlUtils.InvokeToolTipRecreateHandle(this);
            }
        }

        public DarkToolTip() { }

        public DarkToolTip(IContainer cont) : base(cont) { }
    }
}
