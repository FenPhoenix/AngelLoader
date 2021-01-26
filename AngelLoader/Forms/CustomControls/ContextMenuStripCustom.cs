using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class ContextMenuStripCustom : DarkUI.Controls.DarkContextMenu
    {
        private bool _preventClose;
        private ToolStripMenuItemCustom[]? _preventCloseItems;

        public ContextMenuStripCustom() { }

        public ContextMenuStripCustom(IContainer container) : base(container) { }

        public ContextMenuStripCustom(bool darkModeEnabled) => DarkModeEnabled = darkModeEnabled;

        public ContextMenuStripCustom(bool darkModeEnabled, IContainer container) : base(container) => DarkModeEnabled = darkModeEnabled;

        internal void SetPreventCloseOnClickItems(params ToolStripMenuItemCustom[] items) => _preventCloseItems = items;

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            _preventClose = _preventCloseItems!.Contains(e.ClickedItem) && ((ToolStripMenuItemCustom)e.ClickedItem).CheckOnClick;

            base.OnItemClicked(e);
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e)
        {
            if (_preventClose)
            {
                _preventClose = false;
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }
    }
}
