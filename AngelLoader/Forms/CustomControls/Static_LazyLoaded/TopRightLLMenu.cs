using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class TopRightLLMenu
    {
        private static bool _constructed;
        private static readonly bool[] _checkedStates = InitializedArray(TopRightTabsData.Count, true);

        internal static DarkContextMenu Menu = null!;

        private static bool _darkModeEnabled;
        [PublicAPI]
        internal static bool DarkModeEnabled
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

            #region Instantiation and hookup events

            Menu = new DarkContextMenu(_darkModeEnabled, components) { Tag = LazyLoaded.True };

            // Can't use InitializedArray() because them neu wants the array to be of a base type even though the
            // items will be of a derived type, to avoid the stupid covariance warning
            var menuItems = new ToolStripItem[TopRightTabsData.Count];
            for (int i = 0; i < menuItems.Length; i++)
            {
                menuItems[i] = new ToolStripMenuItemCustom { Tag = LazyLoaded.True };
            }

            Menu.Items.AddRange(menuItems);

            AssertR(Menu.Items.Count == TopRightTabsData.Count, "top-right tabs menu item count is different than enum length");

            for (int i = 0; i < Menu.Items.Count; i++)
            {
                var item = (ToolStripMenuItemCustom)Menu.Items[i];
                item.CheckOnClick = true;
                item.Checked = _checkedStates[i];
                item.Click += form.TopRightMenu_MenuItems_Click;
            }

            #endregion

            Menu.SetPreventCloseOnClickItems(Menu.Items.Cast<ToolStripMenuItemCustom>().ToArray());

            _constructed = true;
            Localize();
        }

        internal static void SetItemChecked(int index, bool value)
        {
            if (_constructed)
            {
                ((ToolStripMenuItemCustom)Menu.Items[index]).Checked = value;
            }
            else
            {
                _checkedStates[index] = value;
            }
        }

        internal static void Localize()
        {
            if (!_constructed) return;

            Menu.Items[(int)TopRightTab.Statistics].Text = LText.StatisticsTab.TabText;
            Menu.Items[(int)TopRightTab.EditFM].Text = LText.EditFMTab.TabText;
            Menu.Items[(int)TopRightTab.Comment].Text = LText.CommentTab.TabText;
            Menu.Items[(int)TopRightTab.Tags].Text = LText.TagsTab.TabText;
            Menu.Items[(int)TopRightTab.Patch].Text = LText.PatchTab.TabText;
            Menu.Items[(int)TopRightTab.Mods].Text = LText.ModsTab.TabText;
        }

        internal static bool Focused => _constructed && Menu.Focused;
    }
}
