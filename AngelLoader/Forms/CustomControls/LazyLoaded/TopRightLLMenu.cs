using System.Linq;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class TopRightLLMenu : IDarkable
    {
        private bool _constructed;
        private readonly bool[] _checkedStates = InitializedArray(TopRightTabsData.Count, true);

        private readonly MainForm _owner;

        private DarkContextMenu _menu = null!;
        internal DarkContextMenu Menu
        {
            get
            {
                if (!_constructed)
                {
                    #region Instantiation and hookup events

                    _menu = new DarkContextMenu(_owner) { Tag = LoadType.Lazy };

                    // Can't use InitializedArray() because the menu wants the array to be of a base type even though the
                    // items will be of a derived type, to avoid the stupid covariance warning
                    var menuItems = new ToolStripItem[TopRightTabsData.Count];
                    for (int i = 0; i < menuItems.Length; i++)
                    {
                        menuItems[i] = new ToolStripMenuItemCustom();
                    }

                    _menu.Items.AddRange(menuItems);

                    AssertR(_menu.Items.Count == TopRightTabsData.Count, "top-right tabs menu item count is different than enum length");

                    for (int i = 0; i < _menu.Items.Count; i++)
                    {
                        var item = (ToolStripMenuItemCustom)_menu.Items[i];
                        item.CheckOnClick = true;
                        item.Checked = _checkedStates[i];
                        item.Click += _owner.TopRightMenu_MenuItems_Click;
                    }

                    #endregion

                    _menu.SetPreventCloseOnClickItems(_menu.Items.Cast<ToolStripMenuItemCustom>().ToArray());

                    _menu.DarkModeEnabled = _darkModeEnabled;

                    _constructed = true;

                    Localize();
                }

                return _menu;
            }
        }

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                _menu.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal TopRightLLMenu(MainForm owner) => _owner = owner;

        internal void SetItemChecked(int index, bool value)
        {
            if (_constructed)
            {
                ((ToolStripMenuItemCustom)_menu.Items[index]).Checked = value;
            }
            else
            {
                _checkedStates[index] = value;
            }
        }

        internal void Localize()
        {
            if (!_constructed) return;

            _menu.Items[(int)TopRightTab.Statistics].Text = LText.StatisticsTab.TabText;
            _menu.Items[(int)TopRightTab.EditFM].Text = LText.EditFMTab.TabText;
            _menu.Items[(int)TopRightTab.Comment].Text = LText.CommentTab.TabText;
            _menu.Items[(int)TopRightTab.Tags].Text = LText.TagsTab.TabText;
            _menu.Items[(int)TopRightTab.Patch].Text = LText.PatchTab.TabText;
            _menu.Items[(int)TopRightTab.Mods].Text = LText.ModsTab.TabText;
        }

        internal bool Focused => _constructed && _menu.Focused;
    }
}
