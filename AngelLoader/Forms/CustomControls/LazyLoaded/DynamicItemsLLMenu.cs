using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class DynamicItemsLLMenu
    {
        private bool _constructed;

        private readonly MainForm _owner;

        private DarkContextMenu _menu = null!;
        internal DarkContextMenu Menu
        {
            get
            {
                Construct();
                return _menu;
            }
        }

        internal DynamicItemsLLMenu(MainForm owner) => _owner = owner;

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                _menu.DarkModeEnabled = _darkModeEnabled;
            }
        }

        private void Construct()
        {
            if (_constructed) return;

            _menu = new DarkContextMenu(_owner.GetComponents())
            {
                Tag = LoadType.Lazy,
                DarkModeEnabled = _darkModeEnabled
            };

            _constructed = true;

            // Just to keep things in a known state (clearing items also removes their event hookups, which is
            // convenient)
            _menu.Closed += (_, _) => Menu.Items.Clear();
        }

        internal void ClearAndFillMenu(ToolStripItem[] items)
        {
            Menu.Items.Clear();
            Menu.Items.AddRange(items);
            Menu.RefreshDarkModeState();
        }
    }
}
