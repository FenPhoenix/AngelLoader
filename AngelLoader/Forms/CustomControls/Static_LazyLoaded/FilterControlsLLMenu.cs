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
        private static ToolStripMenuItemCustom TitleMenuItem = null!;
        private static ToolStripMenuItemCustom AuthorMenuItem = null!;
        private static ToolStripMenuItemCustom ReleaseDateMenuItem = null!;
        private static ToolStripMenuItemCustom LastPlayedMenuItem = null!;
        private static ToolStripMenuItemCustom TagsMenuItem = null!;
        private static ToolStripMenuItemCustom FinishedStateMenuItem = null!;
        private static ToolStripMenuItemCustom RatingMenuItem = null!;
        private static ToolStripMenuItemCustom ShowUnsupportedMenuItem = null!;
        private static ToolStripMenuItemCustom ShowRecentAtTopMenuItem = null!;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            _owner = form;

            // @NET5: Force MS Sans Serif
            Menu = new ContextMenuStripCustom(components) { Font = ControlExtensions.LegacyMSSansSerif() };
            Menu.Items.AddRange(new ToolStripItem[]
            {
                TitleMenuItem = new ToolStripMenuItemCustom
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.Title
                },
                AuthorMenuItem = new ToolStripMenuItemCustom
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.Author
                },
                ReleaseDateMenuItem = new ToolStripMenuItemCustom
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.ReleaseDate
                },
                LastPlayedMenuItem = new ToolStripMenuItemCustom
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.LastPlayed
                },
                TagsMenuItem = new ToolStripMenuItemCustom
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.Tags
                },
                FinishedStateMenuItem = new ToolStripMenuItemCustom
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.FinishedState
                },
                RatingMenuItem = new ToolStripMenuItemCustom
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.Rating
                },
                ShowUnsupportedMenuItem = new ToolStripMenuItemCustom
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.ShowUnsupported
                },
                ShowRecentAtTopMenuItem = new ToolStripMenuItemCustom
                {
                    CheckOnClick = true,
                    Tag = HideableFilterControls.ShowRecentAtTop
                }
            });

            for (int i = 0; i < Menu.Items.Count; i++)
            {
                var item = (ToolStripMenuItemCustom)Menu.Items[i];
                item.Checked = _filterCheckedStates[i];
                item.Click += _owner.FilterControlsMenuItems_Click;
            }

            Menu.SetPreventCloseOnClickItems(Menu.Items.Cast<ToolStripMenuItemCustom>().ToArray());

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
                    ((ToolStripMenuItemCustom)Menu.Items[i]).Checked = states[i];
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
                    ret[i] = ((ToolStripMenuItemCustom)Menu.Items[i]).Checked;
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
