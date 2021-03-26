using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AL_Common.CommonUtils;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class TopRightLLMenu
    {
        private static bool _constructed;
        private static readonly bool[] _checkedStates = InitializedArray(TopRightTabsData.Count, true);

        internal static ContextMenuStripCustom Menu = null!;

        private static ToolStripMenuItemCustom StatsMenuItem = null!;
        private static ToolStripMenuItemCustom EditFMMenuItem = null!;
        private static ToolStripMenuItemCustom CommentMenuItem = null!;
        private static ToolStripMenuItemCustom TagsMenuItem = null!;
        private static ToolStripMenuItemCustom PatchMenuItem = null!;

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

            #region Instantiation and hookup events

            Menu = new ContextMenuStripCustom(_darkModeEnabled, components) { Tag = LazyLoaded.True };
            Menu.Items.AddRange(new ToolStripItem[]
            {
                StatsMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                EditFMMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                CommentMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                TagsMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                PatchMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True }
            });
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

            StatsMenuItem.Text = LText.StatisticsTab.TabText;
            EditFMMenuItem.Text = LText.EditFMTab.TabText;
            CommentMenuItem.Text = LText.CommentTab.TabText;
            TagsMenuItem.Text = LText.TagsTab.TabText;
            PatchMenuItem.Text = LText.PatchTab.TabText;
        }

        internal static bool Focused => _constructed && Menu.Focused;
    }
}
