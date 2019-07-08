using System.Windows.Forms;

namespace AngelLoader.Forms
{
    public partial class MainForm
    {
        private static class ImportFromMenuBacking
        {
            internal static bool Constructed;
        }

        private void ConstructImportFromMenu()
        {
            if (ImportFromMenuBacking.Constructed) return;
            // Not localized because they consist solely of proper names! Don't remove these!
            ImportFromDarkLoaderMenuItem = new ToolStripMenuItem { Text = @"DarkLoader" };
            ImportFromFMSelMenuItem = new ToolStripMenuItem { Text = @"FMSel" };
            ImportFromNewDarkLoaderMenuItem = new ToolStripMenuItem { Text = @"NewDarkLoader" };

            ImportFromDarkLoaderMenuItem.Click += ImportFromDarkLoaderMenuItem_Click;
            ImportFromFMSelMenuItem.Click += ImportFromFMSelMenuItem_Click;
            ImportFromNewDarkLoaderMenuItem.Click += ImportFromNewDarkLoaderMenuItem_Click;

            ImportFromMenu = new ContextMenuStrip(components);
            ImportFromMenu.Items.AddRange(new ToolStripItem[]
            {
                ImportFromDarkLoaderMenuItem,
                ImportFromFMSelMenuItem,
                ImportFromNewDarkLoaderMenuItem
            });

            ImportFromMenuBacking.Constructed = true;
        }
    }
}
