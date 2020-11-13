using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class ImportFromLLMenu
    {
        private static bool _constructed;

        internal static ContextMenuStrip ImportFromMenu = null!;

        internal static ToolStripMenuItemCustom ImportFromDarkLoaderMenuItem = null!;
        internal static ToolStripMenuItemCustom ImportFromFMSelMenuItem = null!;
        [UsedImplicitly]
        // It's an implicit "else" case, but let's keep it non-private just for consistency
        internal static ToolStripMenuItemCustom ImportFromNewDarkLoaderMenuItem = null!;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            ImportFromMenu = new ContextMenuStrip(components);

            // Not localized because they consist solely of proper names! Don't remove these!
            ImportFromDarkLoaderMenuItem = new ToolStripMenuItemCustom("DarkLoader...");
            ImportFromFMSelMenuItem = new ToolStripMenuItemCustom("FMSel...");
            ImportFromNewDarkLoaderMenuItem = new ToolStripMenuItemCustom("NewDarkLoader...");

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
