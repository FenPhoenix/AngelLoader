using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.CustomControls;

namespace AngelLoader.Forms
{
    public partial class MainForm
    {
        #region Menu and items

        private ContextMenuStripCustom TopRightMenu;
        private ToolStripMenuItem TRM_StatsMenuItem;
        private ToolStripMenuItem TRM_EditFMMenuItem;
        private ToolStripMenuItem TRM_CommentMenuItem;
        private ToolStripMenuItem TRM_TagsMenuItem;
        private ToolStripMenuItem TRM_PatchMenuItem;

        #endregion

        private bool _constructed;
        private readonly bool[] TopRightMenuCheckedStates = { true, true, true, true, true };

        private void ConstructTopRightMenu()
        {
            if (_constructed) return;

            #region Instantiation

            TopRightMenu = new ContextMenuStripCustom(components) { Name = nameof(TopRightMenu) };
            TopRightMenu.Items.AddRange(new ToolStripItem[]
            {
                (TRM_StatsMenuItem = new ToolStripMenuItem
                {
                    Name = nameof(TRM_StatsMenuItem),
                    Checked = TopRightMenuCheckedStates[(int)TopRightTab.Statistics],
                    CheckOnClick = true
                }),
                (TRM_EditFMMenuItem = new ToolStripMenuItem
                {
                    Name = nameof(TRM_EditFMMenuItem),
                    Checked = TopRightMenuCheckedStates[(int)TopRightTab.EditFM],
                    CheckOnClick = true
                }),
                (TRM_CommentMenuItem = new ToolStripMenuItem
                {
                    Name = nameof(TRM_CommentMenuItem),
                    Checked = TopRightMenuCheckedStates[(int)TopRightTab.Comment],
                    CheckOnClick = true
                }),
                (TRM_TagsMenuItem = new ToolStripMenuItem
                {
                    Name = nameof(TRM_TagsMenuItem),
                    Checked = TopRightMenuCheckedStates[(int)TopRightTab.Tags],
                    CheckOnClick = true
                }),
                (TRM_PatchMenuItem = new ToolStripMenuItem
                {
                    Name = nameof(TRM_PatchMenuItem),
                    Checked = TopRightMenuCheckedStates[(int)TopRightTab.Patch],
                    CheckOnClick = true
                })
            });

            #endregion

            TopRightMenu.SetPreventCloseOnClickItems(TopRightMenu.Items.Cast<ToolStripMenuItem>().ToArray());

            #region Event hookups

            TRM_StatsMenuItem.Click += TopRightMenu_MenuItems_Click;
            TRM_EditFMMenuItem.Click += TopRightMenu_MenuItems_Click;
            TRM_CommentMenuItem.Click += TopRightMenu_MenuItems_Click;
            TRM_TagsMenuItem.Click += TopRightMenu_MenuItems_Click;
            TRM_PatchMenuItem.Click += TopRightMenu_MenuItems_Click;

            #endregion

            _constructed = true;
            LocalizeTopRightMenu();
        }

        private void SetTopRightMenuItemChecked(int index, bool value)
        {
            if (_constructed)
            {
                ((ToolStripMenuItem)TopRightMenu.Items[index]).Checked = value;
            }
            else
            {
                TopRightMenuCheckedStates[index] = value;
            }
        }

        internal void LocalizeTopRightMenu()
        {
            Debug.Assert(TopRightMenuCheckedStates.Length == TopRightTabEnumStatic.TopRightTabsCount,
                nameof(TopRightMenuCheckedStates) + ".Length != " + nameof(TopRightTabEnumStatic.TopRightTabsCount) + ".Length");

            if (!_constructed) return;

            TRM_StatsMenuItem.Text = LText.StatisticsTab.TabText.EscapeAmpersands();
            TRM_EditFMMenuItem.Text = LText.EditFMTab.TabText.EscapeAmpersands();
            TRM_CommentMenuItem.Text = LText.CommentTab.TabText.EscapeAmpersands();
            TRM_TagsMenuItem.Text = LText.TagsTab.TabText.EscapeAmpersands();
            TRM_PatchMenuItem.Text = LText.PatchTab.TabText.EscapeAmpersands();
        }
    }
}
