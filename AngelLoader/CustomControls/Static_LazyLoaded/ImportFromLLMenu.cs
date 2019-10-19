using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses; // for LText in disabled multi loaders menu item
using AngelLoader.Forms;

namespace AngelLoader.CustomControls.Static_LazyLoaded
{
    internal static class ImportFromLLMenu
    {
        private static bool _constructed;

        internal static ContextMenuStrip ImportFromMenu;
        private static ToolStripMenuItem ImportFromDarkLoaderMenuItem;
        private static ToolStripMenuItem ImportFromFMSelMenuItem;
        private static ToolStripMenuItem ImportFromNewDarkLoaderMenuItem;
        //private static ToolStripSeparator Sep1;
        //private static ToolStripMenuItem ImportFromMultipleLoadersMenuItem;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            ImportFromMenu = new ContextMenuStrip(components);

            // Not localized because they consist solely of proper names! Don't remove these!
            ImportFromDarkLoaderMenuItem = new ToolStripMenuItem { Text = @"DarkLoader..." };
            ImportFromFMSelMenuItem = new ToolStripMenuItem { Text = @"FMSel..." };
            ImportFromNewDarkLoaderMenuItem = new ToolStripMenuItem { Text = @"NewDarkLoader..." };
            //Sep1 = new ToolStripSeparator();
            //ImportFromMultipleLoadersMenuItem = new ToolStripMenuItem();

            ImportFromMenu.Items.AddRange(new ToolStripItem[]
            {
                ImportFromDarkLoaderMenuItem,
                ImportFromFMSelMenuItem,
                ImportFromNewDarkLoaderMenuItem,
                //Sep1,
                //ImportFromMultipleLoadersMenuItem
            });

            ImportFromDarkLoaderMenuItem.Click += form.ImportFromDarkLoaderMenuItem_Click;
            ImportFromFMSelMenuItem.Click += form.ImportFromFMSelMenuItem_Click;
            ImportFromNewDarkLoaderMenuItem.Click += form.ImportFromNewDarkLoaderMenuItem_Click;
            //ImportFromMultipleLoadersMenuItem.Click += form.ImportFromMultipleLoadersMenuItem_Click;

            _constructed = true;

            Localize();
        }

        internal static void Localize()
        {
            if (!_constructed) return;
            //ImportFromMultipleLoadersMenuItem.Text = LText.Importing.ImportFromMultipleLoaders;
        }
    }
}