using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class AltTitlesLLMenu
    {
        internal static bool Constructed { get; private set; }

        internal static DarkContextMenu Menu = null!;

        private static bool _darkModeEnabled;
        public static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!Constructed) return;

                Menu!.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal static void Construct(IContainer components)
        {
            if (Constructed) return;

            Menu = new DarkContextMenu(_darkModeEnabled, components) { Tag = LazyLoaded.True };

            Constructed = true;
        }

        internal static void AddRange(List<ToolStripItem> items)
        {
            if (!Constructed) return;

            ToolStripItem[] itemsArray = items.ToArray();

            for (int i = 0; i < itemsArray.Length; i++)
            {
                itemsArray[i].Tag = LazyLoaded.True;
            }

            Menu.Items.AddRange(itemsArray);
        }

        internal static void ClearItems()
        {
            if (!Constructed) return;
            Menu.Items.Clear();
        }
    }
}
