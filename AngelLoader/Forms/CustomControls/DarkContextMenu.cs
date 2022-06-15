using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkContextMenu : ContextMenuStrip
    {
        private bool _preventClose;
        private ToolStripMenuItemCustom[]? _preventCloseItems;

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
                RefreshDarkModeState();
            }
        }

        #region Constructors

        public DarkContextMenu() => RefreshDarkModeState();

        public DarkContextMenu(IContainer container) : base(container) => RefreshDarkModeState();

        public DarkContextMenu(bool darkModeEnabled) => DarkModeEnabled = darkModeEnabled;

        public DarkContextMenu(bool darkModeEnabled, IContainer container) : base(container) => DarkModeEnabled = darkModeEnabled;

        #endregion

        internal void SetPreventCloseOnClickItems(params ToolStripMenuItemCustom[] items) => _preventCloseItems = items;

        public void AddRange(ToolStripItem[] toolStripItems)
        {
            Items.AddRange(toolStripItems);
            RefreshDarkModeState();
        }

        /*
        // Disabled until needed
        public void AddRange(ToolStripItemCollection toolStripItems)
        {
            for (int i = 0; i < toolStripItems.Count; i++)
            {
                toolStripItems[i].Tag = LoadType.Lazy;
            }

            Items.AddRange(toolStripItems);
            RefreshDarkModeState();
        }
        */

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (_preventCloseItems?.Length > 0)
            {
                _preventClose = _preventCloseItems.Contains(e.ClickedItem) && ((ToolStripMenuItemCustom)e.ClickedItem).CheckOnClick;
            }

            base.OnItemClicked(e);
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e)
        {
            if (_preventCloseItems?.Length > 0 && _preventClose)
            {
                _preventClose = false;
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }

        private void RefreshDarkModeState()
        {
            void SetMenuTheme(ToolStripDropDown menu)
            {
                if (_darkModeEnabled)
                {
                    menu.Renderer = new DarkMenuRenderer();
                }
                else
                {
                    menu.RenderMode = ToolStripRenderMode.ManagerRenderMode;

                    // Prevents wrong back color on separators
                    menu.ResetBackColor();
                    menu.ResetForeColor();

                    // Prevents wrong back/fore color on items
                    foreach (ToolStripItem item in menu.Items)
                    {
                        item.ResetBackColor();
                        item.ResetForeColor();
                    }
                }

                foreach (ToolStripItem item in menu.Items)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem.DropDown != null)
                    {
                        SetMenuTheme(menuItem.DropDown);
                    }
                }
            }

            SetMenuTheme(this);
        }
    }
}
