using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DarkUI.Renderers;

namespace DarkUI.Controls
{
    public class DarkContextMenu : ContextMenuStrip
    {

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                SetUpTheme();
            }
        }

        #region Constructors

        // TODO: If we set dark mode after construct, we end up with our submenus not properly changed

        public DarkContextMenu() => SetUpTheme();

        public DarkContextMenu(IContainer container) : base(container) => SetUpTheme();

        public DarkContextMenu(bool darkModeEnabled) => DarkModeEnabled = darkModeEnabled;

        public DarkContextMenu(bool darkModeEnabled, IContainer container) : base(container) => DarkModeEnabled = darkModeEnabled;

        #endregion

        private void SetUpTheme()
        {
            if (_darkModeEnabled)
            {
                Renderer = new DarkMenuRenderer();
            }
            else
            {
                RenderMode = ToolStripRenderMode.ManagerRenderMode;

                // Prevents wrong back color on separators
                BackColor = SystemColors.Control;

                // Prevents wrong back/fore color on items
                foreach (ToolStripItem item in Items)
                {
                    item.BackColor = SystemColors.Control;
                    item.ForeColor = SystemColors.ControlText;
                }
            }
        }
    }
}
