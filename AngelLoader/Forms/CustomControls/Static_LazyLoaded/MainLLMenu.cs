using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class MainLLMenu
    {
        private static bool _constructed;

        internal static ContextMenuStrip Menu = null!;
        private static ToolStripMenuItem ViewGameInfoMenuItem = null!;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new ContextMenuStrip(components);
            Menu.Items.AddRange(new ToolStripItem[]
            {
                ViewGameInfoMenuItem = new ToolStripMenuItem()
            });

            ViewGameInfoMenuItem.Click += form.MainMenu_ViewGameInfoMenuItem_Click;

            _constructed = true;

            Localize();
        }

        internal static void Localize()
        {
            if (!_constructed) return;

            ViewGameInfoMenuItem.Text = LText.MainMenu.ViewGameInfo;
        }
    }
}
