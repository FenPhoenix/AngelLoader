using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class ImportFromLLMenu
    {
        private static bool _constructed;

        internal static ContextMenuStrip Menu = null!;

        internal static ToolStripMenuItemCustom ImportFromDarkLoaderMenuItem = null!;
        internal static ToolStripMenuItemCustom ImportFromFMSelMenuItem = null!;
        [UsedImplicitly]
        // It's an implicit "else" case, but let's keep it non-private just for consistency
        internal static ToolStripMenuItemCustom ImportFromNewDarkLoaderMenuItem = null!;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new ContextMenuStrip(components);

            Menu.Items.AddRange(new ToolStripItem[]
            {
                // Not localized because they consist solely of proper names! Don't remove these!
                ImportFromDarkLoaderMenuItem = new ToolStripMenuItemCustom("DarkLoader..."),
                ImportFromFMSelMenuItem = new ToolStripMenuItemCustom("FMSel..."),
                ImportFromNewDarkLoaderMenuItem = new ToolStripMenuItemCustom("NewDarkLoader...")
            });

            foreach (ToolStripMenuItemCustom item in Menu.Items)
            {
                item.Click += form.ImportMenuItems_Click;
            }

            _constructed = true;
        }
    }
}
