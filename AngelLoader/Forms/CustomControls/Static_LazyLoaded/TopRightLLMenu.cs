using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.DataClasses;
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

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            #region Instantiation

            // @NET5: Force MS Sans Serif
            Menu = new ContextMenuStripCustom(components) { Font = ControlExtensions.LegacyMSSansSerif() };
            Menu.Items.AddRange(new ToolStripItem[]
            {
                StatsMenuItem = new ToolStripMenuItemCustom
                {
                    Checked = _checkedStates[(int)TopRightTab.Statistics],
                    CheckOnClick = true
                },
                EditFMMenuItem = new ToolStripMenuItemCustom
                {
                    Checked = _checkedStates[(int)TopRightTab.EditFM],
                    CheckOnClick = true
                },
                CommentMenuItem = new ToolStripMenuItemCustom
                {
                    Checked = _checkedStates[(int)TopRightTab.Comment],
                    CheckOnClick = true
                },
                TagsMenuItem = new ToolStripMenuItemCustom
                {
                    Checked = _checkedStates[(int)TopRightTab.Tags],
                    CheckOnClick = true
                },
                PatchMenuItem = new ToolStripMenuItemCustom
                {
                    Checked = _checkedStates[(int)TopRightTab.Patch],
                    CheckOnClick = true
                }
            });

            #endregion

            Menu.SetPreventCloseOnClickItems(Menu.Items.Cast<ToolStripMenuItemCustom>().ToArray());

            #region Event hookups

            StatsMenuItem.Click += form.TopRightMenu_MenuItems_Click;
            EditFMMenuItem.Click += form.TopRightMenu_MenuItems_Click;
            CommentMenuItem.Click += form.TopRightMenu_MenuItems_Click;
            TagsMenuItem.Click += form.TopRightMenu_MenuItems_Click;
            PatchMenuItem.Click += form.TopRightMenu_MenuItems_Click;

            #endregion

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
