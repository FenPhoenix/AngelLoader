using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class FilterControlsLLMenu
    {
        private static bool _constructed;
        private static readonly bool[] _filterCheckedStates = InitializedArray(HideableFilterControlsCount, true);

        private static MainForm _owner = null!;

        internal static ContextMenuStripCustom Menu = null!;
        private static ToolStripMenuItem TitleMenuItem = null!;
        private static ToolStripMenuItem AuthorMenuItem = null!;
        private static ToolStripMenuItem ReleaseDateMenuItem = null!;
        private static ToolStripMenuItem LastPlayedMenuItem = null!;
        private static ToolStripMenuItem TagsMenuItem = null!;
        private static ToolStripMenuItem FinishedStateMenuItem = null!;
        private static ToolStripMenuItem RatingMenuItem = null!;
        private static ToolStripMenuItem ShowUnsupportedMenuItem = null!;
        private static ToolStripMenuItem ShowRecentAtTopMenuItem = null!;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            _owner = form;

            Menu = new ContextMenuStripCustom(components);
            Menu.Items.AddRange(new ToolStripItem[]
            {
                TitleMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.Title
                },
                AuthorMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.Author
                },
                ReleaseDateMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.ReleaseDate
                },
                LastPlayedMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.LastPlayed
                },
                TagsMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.Tags
                },
                FinishedStateMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.FinishedState
                },
                RatingMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.Rating
                },
                ShowUnsupportedMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.ShowUnsupported
                },
                ShowRecentAtTopMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.ShowRecentAtTop
                }
            });

            for (int i = 0; i < Menu.Items.Count; i++)
            {
                var item = (ToolStripMenuItem)Menu.Items[i];
                item.Checked = _filterCheckedStates[i];
                item.Click += _owner.FilterControlsMenuItems_Click;
            }

            Menu.SetPreventCloseOnClickItems(Menu.Items.Cast<ToolStripMenuItem>().ToArray());

            _constructed = true;

            Localize();
        }

        private static void Localize()
        {
            TitleMenuItem.Text = LText.FilterBar.ShowHideMenu_Title;
            AuthorMenuItem.Text = LText.FilterBar.ShowHideMenu_Author;
            ReleaseDateMenuItem.Text = LText.FilterBar.ShowHideMenu_ReleaseDate;
            LastPlayedMenuItem.Text = LText.FilterBar.ShowHideMenu_LastPlayed;
            TagsMenuItem.Text = LText.FilterBar.ShowHideMenu_Tags;
            FinishedStateMenuItem.Text = LText.FilterBar.ShowHideMenu_FinishedState;
            RatingMenuItem.Text = LText.FilterBar.ShowHideMenu_Rating;
            ShowUnsupportedMenuItem.Text = LText.FilterBar.ShowHideMenu_ShowUnsupported;
            ShowRecentAtTopMenuItem.Text = LText.FilterBar.ShowHideMenu_ShowRecentAtTop;
        }

        internal static void SetCheckedStates(bool[] states)
        {
            if (_constructed)
            {
                for (int i = 0; i < Menu!.Items.Count; i++)
                {
                    ((ToolStripMenuItem)Menu.Items[i]).Checked = states[i];
                }
            }
            else
            {
                Array.Copy(states, _filterCheckedStates, HideableFilterControlsCount);
            }
        }

        internal static bool[] GetCheckedStates()
        {
            bool[] ret = new bool[HideableFilterControlsCount];

            if (_constructed)
            {
                for (int i = 0; i < Menu!.Items.Count; i++)
                {
                    ret[i] = ((ToolStripMenuItem)Menu.Items[i]).Checked;
                }
            }
            else
            {
                Array.Copy(_filterCheckedStates, ret, HideableFilterControlsCount);
            }

            return ret;
        }
    }
}
