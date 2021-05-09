using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkContextMenu : ContextMenuStrip
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
                RefreshDarkModeState();
            }
        }

        #region Constructors

        public DarkContextMenu() => RefreshDarkModeState();

        public DarkContextMenu(IContainer container) : base(container) => RefreshDarkModeState();

        public DarkContextMenu(bool darkModeEnabled) => DarkModeEnabled = darkModeEnabled;

        public DarkContextMenu(bool darkModeEnabled, IContainer container) : base(container) => DarkModeEnabled = darkModeEnabled;

        #endregion

        public void AddRange(ToolStripItem[] toolStripItems)
        {
            for (int i = 0; i < toolStripItems.Length; i++)
            {
                toolStripItems[i].Tag = LazyLoaded.True;
            }

            Items.AddRange(toolStripItems);
            RefreshDarkModeState();
        }

        /*
        // Disabled until needed
        public void AddRange(ToolStripItemCollection toolStripItems)
        {
            for (int i = 0; i < toolStripItems.Count; i++)
            {
                toolStripItems[i].Tag = LazyLoaded.True;
            }

            Items.AddRange(toolStripItems);
            RefreshDarkModeState();
        }
        */

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
