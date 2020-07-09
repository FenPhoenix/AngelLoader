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
        private static readonly bool[] _checkedStates = InitializedArray(TopRightTabsCount, true);

        private static ContextMenuStripCustom? _menu;
        internal static ContextMenuStripCustom Menu
        {
            get => _menu!;
            private set => _menu = value;
        }

        private static ToolStripMenuItem? StatsMenuItem;
        private static ToolStripMenuItem? EditFMMenuItem;
        private static ToolStripMenuItem? CommentMenuItem;
        private static ToolStripMenuItem? TagsMenuItem;
        private static ToolStripMenuItem? PatchMenuItem;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            #region Instantiation

            Menu = new ContextMenuStripCustom(components);
            Menu.Items.AddRange(new ToolStripItem[]
            {
                StatsMenuItem = new ToolStripMenuItem
                {
                    Checked = _checkedStates[(int)TopRightTab.Statistics],
                    CheckOnClick = true
                },
                EditFMMenuItem = new ToolStripMenuItem
                {
                    Checked = _checkedStates[(int)TopRightTab.EditFM],
                    CheckOnClick = true
                },
                CommentMenuItem = new ToolStripMenuItem
                {
                    Checked = _checkedStates[(int)TopRightTab.Comment],
                    CheckOnClick = true
                },
                TagsMenuItem = new ToolStripMenuItem
                {
                    Checked = _checkedStates[(int)TopRightTab.Tags],
                    CheckOnClick = true
                },
                PatchMenuItem = new ToolStripMenuItem
                {
                    Checked = _checkedStates[(int)TopRightTab.Patch],
                    CheckOnClick = true
                }
            });

            #endregion

            Menu.SetPreventCloseOnClickItems(Menu.Items.Cast<ToolStripMenuItem>().ToArray());

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
                ((ToolStripMenuItem)Menu.Items[index]).Checked = value;
            }
            else
            {
                _checkedStates[index] = value;
            }
        }

        internal static void Localize()
        {
            if (!_constructed) return;

            StatsMenuItem!.Text = LText.StatisticsTab.TabText.EscapeAmpersands();
            EditFMMenuItem!.Text = LText.EditFMTab.TabText.EscapeAmpersands();
            CommentMenuItem!.Text = LText.CommentTab.TabText.EscapeAmpersands();
            TagsMenuItem!.Text = LText.TagsTab.TabText.EscapeAmpersands();
            PatchMenuItem!.Text = LText.PatchTab.TabText.EscapeAmpersands();
        }

        internal static bool Focused => _constructed && Menu.Focused;
    }
}
