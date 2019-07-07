using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Properties;

namespace AngelLoader.Forms
{
    public partial class MainForm
    {
        private static class PlayOriginalGameMenuBacking
        {
            internal static bool Constructed;
        }

        private void ConstructPlayOriginalGameMenu()
        {
            if (PlayOriginalGameMenuBacking.Constructed) return;

            PlayOriginalThief1MenuItem = new ToolStripMenuItem { Image = Resources.Thief1_16 };
            PlayOriginalThief2MenuItem = new ToolStripMenuItem { Image = Resources.Thief2_16 };
            PlayOriginalThief2MPMenuItem = new ToolStripMenuItem { Image = Resources.Thief2_16 };
            PlayOriginalThief3MenuItem = new ToolStripMenuItem { Image = Resources.Thief3_16 };

            PlayOriginalThief1MenuItem.Click += PlayOriginalGameMenuItem_Click;
            PlayOriginalThief2MenuItem.Click += PlayOriginalGameMenuItem_Click;
            PlayOriginalThief2MPMenuItem.Click += PlayOriginalGameMenuItem_Click;
            PlayOriginalThief3MenuItem.Click += PlayOriginalGameMenuItem_Click;

            PlayOriginalGameMenu = new ContextMenuStrip(components);
            PlayOriginalGameMenu.Items.AddRange(new ToolStripItem[]
            {
                PlayOriginalThief1MenuItem,
                PlayOriginalThief2MenuItem,
                PlayOriginalThief2MPMenuItem,
                PlayOriginalThief3MenuItem
            });

            PlayOriginalGameMenuBacking.Constructed = true;

            LocalizePlayOriginalGameMenuItems();
        }

        private void LocalizePlayOriginalGameMenuItems()
        {
            if (!PlayOriginalGameMenuBacking.Constructed) return;

            PlayOriginalThief1MenuItem.Text = LText.PlayOriginalGameMenu.Thief1.EscapeAmpersands();
            PlayOriginalThief2MenuItem.Text = LText.PlayOriginalGameMenu.Thief2.EscapeAmpersands();
            PlayOriginalThief2MPMenuItem.Text = LText.PlayOriginalGameMenu.Thief2_Multiplayer.EscapeAmpersands();
            PlayOriginalThief3MenuItem.Text = LText.PlayOriginalGameMenu.Thief3.EscapeAmpersands();
        }
    }
}
