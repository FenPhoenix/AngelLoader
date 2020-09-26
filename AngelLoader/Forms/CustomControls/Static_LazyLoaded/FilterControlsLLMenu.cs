using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
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
            TitleMenuItem.Text = "Title";
            AuthorMenuItem.Text = "Author";
            ReleaseDateMenuItem.Text = "Release date";
            LastPlayedMenuItem.Text = "Last played";
            TagsMenuItem.Text = "Tags";
            FinishedStateMenuItem.Text = "Finished state";
            RatingMenuItem.Text = "Rating";
            ShowUnsupportedMenuItem.Text = "Show unsupported";
            ShowRecentAtTopMenuItem.Text = "Show recent at top";
        }
    }
}
