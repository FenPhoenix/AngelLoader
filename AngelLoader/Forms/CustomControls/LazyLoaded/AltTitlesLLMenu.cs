using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal sealed class AltTitlesLLMenu
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

        internal AltTitlesLLMenu(MainForm owner) => _owner = owner;

        private bool _darkModeEnabled;
        [PublicAPI]
        internal bool DarkModeEnabled
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

            _menu = new DarkContextMenu(_darkModeEnabled, _owner.GetComponents()) { Tag = LoadType.Lazy };

            _constructed = true;
        }

        internal void AddRange(ToolStripItem[] items)
        {
            Construct();

            _menu.AddRange(items);
        }

        internal void ClearItems()
        {
            if (!_constructed) return;
            _menu.Items.Clear();
        }
    }
}
