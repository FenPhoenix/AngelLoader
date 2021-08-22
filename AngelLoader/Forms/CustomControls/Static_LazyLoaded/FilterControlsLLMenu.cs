using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Utils;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class FilterControlsLLMenu
    {
        private static bool _constructed;
        private static readonly bool[] _filterCheckedStates = InitializedArray(HideableFilterControlsCount, true);

        private static MainForm _owner = null!;

        internal static DarkContextMenu Menu = null!;
        private static ToolStripMenuItemCustom TitleMenuItem = null!;
        private static ToolStripMenuItemCustom AuthorMenuItem = null!;
        private static ToolStripMenuItemCustom ReleaseDateMenuItem = null!;
        private static ToolStripMenuItemCustom LastPlayedMenuItem = null!;
        private static ToolStripMenuItemCustom TagsMenuItem = null!;
        private static ToolStripMenuItemCustom FinishedStateMenuItem = null!;
        private static ToolStripMenuItemCustom RatingMenuItem = null!;
        private static ToolStripMenuItemCustom ShowUnsupportedMenuItem = null!;
        private static ToolStripMenuItemCustom ShowUnavailableFMsMenuItem = null!;
        private static ToolStripMenuItemCustom ShowRecentAtTopMenuItem = null!;

        private static bool _darkModeEnabled;
        [PublicAPI]
        public static bool DarkModeEnabled
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

            _owner = form;

            // TODO: Component LazyLoaded tags are ignored because only Controls are checked in the dictionary filler.
            // Also, they get stomped on below anyway with the indexes.

            Menu = new DarkContextMenu(_darkModeEnabled, components) { Tag = LazyLoaded.True };
            Menu.Items.AddRange(new ToolStripItem[]
            {
                TitleMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                AuthorMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                ReleaseDateMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                LastPlayedMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                TagsMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                FinishedStateMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                RatingMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                ShowUnsupportedMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                ShowUnavailableFMsMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                ShowRecentAtTopMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True }
            });

            for (int i = 0; i < Menu.Items.Count; i++)
            {
                var item = (ToolStripMenuItemCustom)Menu.Items[i];
                item.CheckOnClick = true;
                item.Tag = (HideableFilterControls)i;
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
            ShowUnavailableFMsMenuItem.Text = LText.FilterBar.ShowHideMenu_ShowUnavailable;
            ShowRecentAtTopMenuItem.Text = LText.FilterBar.ShowHideMenu_ShowRecentAtTop;
        }

        internal static void SetCheckedStates(bool[] states)
        {
            if (_constructed)
            {
                for (int i = 0; i < Menu.Items.Count; i++)
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
                for (int i = 0; i < Menu.Items.Count; i++)
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
