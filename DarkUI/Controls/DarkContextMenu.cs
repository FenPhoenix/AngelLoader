using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DarkUI.Renderers;

namespace DarkUI.Controls
{
    public class DarkContextMenu : ContextMenuStrip
    {
        #region Constructors

        public DarkContextMenu() => SetUpTheme();


        #endregion

        private void SetUpTheme()
        {
            void SetMenuTheme(ToolStripDropDown menu)
            {
                menu.Renderer = new DarkMenuRenderer();

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
