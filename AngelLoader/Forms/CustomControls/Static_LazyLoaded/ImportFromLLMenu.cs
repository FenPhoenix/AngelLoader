using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class ImportFromLLMenu
    {
        private static bool _constructed;

        internal static DarkContextMenu Menu = null!;

        internal static ToolStripMenuItemCustom ImportFromDarkLoaderMenuItem = null!;
        internal static ToolStripMenuItemCustom ImportFromFMSelMenuItem = null!;
        [UsedImplicitly]
        // It's an implicit "else" case, but let's keep it non-private just for consistency
        internal static ToolStripMenuItemCustom ImportFromNewDarkLoaderMenuItem = null!;

        private static bool _darkModeEnabled;
        public static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                Menu!.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new DarkContextMenu(_darkModeEnabled, components) { Tag = LazyLoaded.True };

            Menu.Items.AddRange(new ToolStripItem[]
            {
                // Not localized because they consist solely of proper names! Don't remove these!
                ImportFromDarkLoaderMenuItem = new ToolStripMenuItemCustom("DarkLoader...") { Tag = LazyLoaded.True },
                ImportFromFMSelMenuItem = new ToolStripMenuItemCustom("FMSel...") { Tag = LazyLoaded.True },
                ImportFromNewDarkLoaderMenuItem = new ToolStripMenuItemCustom("NewDarkLoader...") { Tag = LazyLoaded.True }
            });

            foreach (ToolStripMenuItemCustom item in Menu.Items)
            {
                item.Click += form.ImportMenuItems_Click;
            }

            _constructed = true;
        }
    }
}
