using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class AddTagLLMenu
    {
        private static bool _constructed;

        internal static DarkContextMenu Menu = null!;

        private static bool _darkModeEnabled;
        [PublicAPI]
        public static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                Menu.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new DarkContextMenu(_darkModeEnabled, components) { Tag = LazyLoaded.True };
            Menu.Closed += form.AddTagMenu_Closed;

            _constructed = true;
        }

        internal static void AddRange(ToolStripItem[] items)
        {
            if (!_constructed) return;

            Menu.AddRange(items);
        }
    }
}
