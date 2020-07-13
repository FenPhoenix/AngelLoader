using System.ComponentModel;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class ImportFromLLMenu
    {
        private static bool _constructed;

        private static ContextMenuStrip? _importFromMenu;
        internal static ContextMenuStrip ImportFromMenu
        {
            get => _importFromMenu!;
            private set => _importFromMenu = value;
        }

        private static ToolStripMenuItem? ImportFromDarkLoaderMenuItem;
        private static ToolStripMenuItem? ImportFromFMSelMenuItem;
        private static ToolStripMenuItem? ImportFromNewDarkLoaderMenuItem;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            ImportFromMenu = new ContextMenuStrip(components);

            // Not localized because they consist solely of proper names! Don't remove these!
            ImportFromDarkLoaderMenuItem = new ToolStripMenuItem { Text = @"DarkLoader..." };
            ImportFromFMSelMenuItem = new ToolStripMenuItem { Text = @"FMSel..." };
            ImportFromNewDarkLoaderMenuItem = new ToolStripMenuItem { Text = @"NewDarkLoader..." };

            ImportFromMenu.Items.AddRange(new ToolStripItem[]
            {
                ImportFromDarkLoaderMenuItem,
                ImportFromFMSelMenuItem,
                ImportFromNewDarkLoaderMenuItem
            });

            ImportFromDarkLoaderMenuItem.Click += form.ImportFromDarkLoaderMenuItem_Click;
            ImportFromFMSelMenuItem.Click += form.ImportFromFMSelMenuItem_Click;
            ImportFromNewDarkLoaderMenuItem.Click += form.ImportFromNewDarkLoaderMenuItem_Click;

            _constructed = true;
        }
    }
}