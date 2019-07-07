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
        private static class TopRightMenuBacking
        {
            internal static bool Constructed;
            internal static readonly bool[] CheckedStates = { true, true, true, true, true };
        }

        private void ConstructTopRightMenu()
        {
            if (TopRightMenuBacking.Constructed) return;

            #region Instantiation

            TopRightMenu = new ContextMenuStripCustom(components) { Name = nameof(TopRightMenu) };
            TopRightMenu.Items.AddRange(new ToolStripItem[]
            {
                (TRM_StatsMenuItem = new ToolStripMenuItem
                {
                    Name = nameof(TRM_StatsMenuItem),
                    Checked = TopRightMenuBacking.CheckedStates[(int)TopRightTab.Statistics],
                    CheckOnClick = true
                }),
                (TRM_EditFMMenuItem = new ToolStripMenuItem
                {
                    Name = nameof(TRM_EditFMMenuItem),
                    Checked = TopRightMenuBacking.CheckedStates[(int)TopRightTab.EditFM],
                    CheckOnClick = true
                }),
                (TRM_CommentMenuItem = new ToolStripMenuItem
                {
                    Name = nameof(TRM_CommentMenuItem),
                    Checked = TopRightMenuBacking.CheckedStates[(int)TopRightTab.Comment],
                    CheckOnClick = true
                }),
                (TRM_TagsMenuItem = new ToolStripMenuItem
                {
                    Name = nameof(TRM_TagsMenuItem),
                    Checked = TopRightMenuBacking.CheckedStates[(int)TopRightTab.Tags],
                    CheckOnClick = true
                }),
                (TRM_PatchMenuItem = new ToolStripMenuItem
                {
                    Name = nameof(TRM_PatchMenuItem),
                    Checked = TopRightMenuBacking.CheckedStates[(int)TopRightTab.Patch],
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

            TopRightMenuBacking.Constructed = true;
            LocalizeTopRightMenu();
        }

        private void SetTopRightMenuItemChecked(int index, bool value)
        {
            if (TopRightMenuBacking.Constructed)
            {
                ((ToolStripMenuItem)TopRightMenu.Items[index]).Checked = value;
            }
            else
            {
                TopRightMenuBacking.CheckedStates[index] = value;
            }
        }

        internal void LocalizeTopRightMenu()
        {
            Debug.Assert(TopRightMenuBacking.CheckedStates.Length == TopRightTabEnumStatic.TopRightTabsCount,
                nameof(TopRightMenuBacking.CheckedStates) + ".Length != " + nameof(TopRightTabEnumStatic.TopRightTabsCount) + ".Length");

            if (!TopRightMenuBacking.Constructed) return;

            TRM_StatsMenuItem.Text = LText.StatisticsTab.TabText.EscapeAmpersands();
            TRM_EditFMMenuItem.Text = LText.EditFMTab.TabText.EscapeAmpersands();
            TRM_CommentMenuItem.Text = LText.CommentTab.TabText.EscapeAmpersands();
            TRM_TagsMenuItem.Text = LText.TagsTab.TabText.EscapeAmpersands();
            TRM_PatchMenuItem.Text = LText.PatchTab.TabText.EscapeAmpersands();
        }
    }
}
