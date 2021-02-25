using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkContextMenu : ContextMenuStrip
    {

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                SetUpTheme();
            }
        }

        #region Constructors

        public DarkContextMenu() => SetUpTheme();

        public DarkContextMenu(IContainer container) : base(container) => SetUpTheme();

        public DarkContextMenu(bool darkModeEnabled) => DarkModeEnabled = darkModeEnabled;

        public DarkContextMenu(bool darkModeEnabled, IContainer container) : base(container) => DarkModeEnabled = darkModeEnabled;

        #endregion

        private void SetUpTheme()
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
                    menu.BackColor = SystemColors.Control;

                    // Prevents wrong back/fore color on items
                    foreach (ToolStripItem item in menu.Items)
                    {
                        item.BackColor = SystemColors.Control;
                        item.ForeColor = SystemColors.ControlText;
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
