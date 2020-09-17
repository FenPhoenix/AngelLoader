using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class ImportFromLLMenu
    {
        private static bool _constructed;

        internal static ContextMenuStrip ImportFromMenu = null!;

        internal static ToolStripMenuItem ImportFromDarkLoaderMenuItem = null!;
        internal static ToolStripMenuItem ImportFromFMSelMenuItem = null!;
        [UsedImplicitly]
        // It's an implicit "else" case, but let's keep it non-private just for consistency
        internal static ToolStripMenuItem ImportFromNewDarkLoaderMenuItem = null!;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            ImportFromMenu = new ContextMenuStrip(components);

            // Not localized because they consist solely of proper names! Don't remove these!
            ImportFromDarkLoaderMenuItem = new ToolStripMenuItem { Text = "DarkLoader..." };
            ImportFromFMSelMenuItem = new ToolStripMenuItem { Text = "FMSel..." };
            ImportFromNewDarkLoaderMenuItem = new ToolStripMenuItem { Text = "NewDarkLoader..." };

            ImportFromMenu.Items.AddRange(new ToolStripItem[]
            {
                ImportFromDarkLoaderMenuItem,
                ImportFromFMSelMenuItem,
                ImportFromNewDarkLoaderMenuItem
            });

            ImportFromDarkLoaderMenuItem.Click += form.ImportMenuItems_Click;
            ImportFromFMSelMenuItem.Click += form.ImportMenuItems_Click;
            ImportFromNewDarkLoaderMenuItem.Click += form.ImportMenuItems_Click;

            _constructed = true;
        }
    }
}
