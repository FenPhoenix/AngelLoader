using System.Windows.Forms;
using AngelLoader.Common.DataClasses;

namespace AngelLoader.CustomControls
{
    public sealed partial class DataGridViewCustom
    {
        private void InitColumnHeaderContextMenu()
        {
            #region Instantiation

            FMColumnHeaderRightClickMenu = new ContextMenuStripCustom
            {
                Name = "FMColumnHeaderRightClickMenu"
            };
            ResetColumnVisibilityMenuItem = new ToolStripMenuItem
            {
                Name = "ResetColumnVisibilityMenuItem"
            };
            ResetAllColumnWidthsMenuItem = new ToolStripMenuItem
            {
                Name = "ResetAllColumnWidthsMenuItem"
            };
            ResetColumnPositionsMenuItem = new ToolStripMenuItem
            {
                Name = "ResetColumnPositionsMenuItem"
            };
            ColumnHeaderRightClickMenuSeparator1 = new ToolStripSeparator
            {
                Name = "ColumnHeaderRightClickMenuSeparator1"
            };
            ShowGameMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowGameMenuItem",
                Tag = Column.Game
            };
            ShowInstalledMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowInstalledMenuItem",
                Tag = Column.Installed
            };
            ShowTitleMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowTitleMenuItem",
                Tag = Column.Title
            };
            ShowArchiveMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowArchiveMenuItem",
                Tag = Column.Archive
            };
            ShowAuthorMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowAuthorMenuItem",
                Tag = Column.Author
            };
            ShowSizeMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowSizeMenuItem",
                Tag = Column.Size
            };
            ShowRatingMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowRatingMenuItem",
                Tag = Column.Rating
            };
            ShowFinishedMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowFinishedMenuItem",
                Tag = Column.Finished
            };
            ShowReleaseDateMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowReleaseDateMenuItem",
                Tag = Column.ReleaseDate
            };
            ShowLastPlayedMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowLastPlayedMenuItem",
                Tag = Column.LastPlayed
            };
            ShowDisabledModsMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowDisabledModsMenuItem",
                Tag = Column.DisabledMods
            };
            ShowCommentMenuItem = new ToolStripMenuItem
            {
                Checked = true,
                CheckOnClick = true,
                Name = "ShowCommentMenuItem",
                Tag = Column.Comment
            };

            #endregion

            #region Event hookups

            ResetColumnVisibilityMenuItem.Click += ResetColumnVisibilityMenuItem_Click;
            ResetAllColumnWidthsMenuItem.Click += ResetAllColumnWidthsMenuItem_Click;
            ResetColumnPositionsMenuItem.Click += ResetColumnPositionsMenuItem_Click;

            ShowGameMenuItem.Click += CheckBoxMenuItem_Click;
            ShowInstalledMenuItem.Click += CheckBoxMenuItem_Click;
            ShowTitleMenuItem.Click += CheckBoxMenuItem_Click;
            ShowArchiveMenuItem.Click += CheckBoxMenuItem_Click;
            ShowAuthorMenuItem.Click += CheckBoxMenuItem_Click;
            ShowSizeMenuItem.Click += CheckBoxMenuItem_Click;
            ShowRatingMenuItem.Click += CheckBoxMenuItem_Click;
            ShowFinishedMenuItem.Click += CheckBoxMenuItem_Click;
            ShowReleaseDateMenuItem.Click += CheckBoxMenuItem_Click;
            ShowLastPlayedMenuItem.Click += CheckBoxMenuItem_Click;
            ShowDisabledModsMenuItem.Click += CheckBoxMenuItem_Click;
            ShowCommentMenuItem.Click += CheckBoxMenuItem_Click;

            #endregion

            #region Add items to menu

            FMColumnHeaderRightClickMenu.Items.AddRange(new ToolStripItem[]
            {
                ResetColumnVisibilityMenuItem,
                ResetAllColumnWidthsMenuItem,
                ResetColumnPositionsMenuItem,
                ColumnHeaderRightClickMenuSeparator1,
                ShowGameMenuItem,
                ShowInstalledMenuItem,
                ShowTitleMenuItem,
                ShowArchiveMenuItem,
                ShowAuthorMenuItem,
                ShowSizeMenuItem,
                ShowRatingMenuItem,
                ShowFinishedMenuItem,
                ShowReleaseDateMenuItem,
                ShowLastPlayedMenuItem,
                ShowDisabledModsMenuItem,
                ShowCommentMenuItem
            });

            #endregion

            #region Fill ColumnHeaderCheckBoxMenuItems array

            ColumnHeaderCheckBoxMenuItems = new[]
            {
                ShowGameMenuItem,
                ShowInstalledMenuItem,
                ShowTitleMenuItem,
                ShowArchiveMenuItem,
                ShowAuthorMenuItem,
                ShowSizeMenuItem,
                ShowRatingMenuItem,
                ShowFinishedMenuItem,
                ShowReleaseDateMenuItem,
                ShowLastPlayedMenuItem,
                ShowDisabledModsMenuItem,
                ShowCommentMenuItem
            };

            #endregion

            FMColumnHeaderRightClickMenu.SetPreventCloseOnClickItems(ColumnHeaderCheckBoxMenuItems);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OriginalRightClickMenu?.Dispose();
                FMColumnHeaderRightClickMenu?.Dispose();
                ShowTitleMenuItem?.Dispose();
                ShowArchiveMenuItem?.Dispose();
                ShowSizeMenuItem?.Dispose();
                ShowRatingMenuItem?.Dispose();
                ShowReleaseDateMenuItem?.Dispose();
                ShowLastPlayedMenuItem?.Dispose();
                ShowCommentMenuItem?.Dispose();
                ShowDisabledModsMenuItem?.Dispose();
                ShowInstalledMenuItem?.Dispose();
                ShowGameMenuItem?.Dispose();
                ShowAuthorMenuItem?.Dispose();
                ResetColumnVisibilityMenuItem?.Dispose();
                ResetColumnPositionsMenuItem?.Dispose();
                ResetAllColumnWidthsMenuItem?.Dispose();
                ColumnHeaderRightClickMenuSeparator1?.Dispose();
                ShowFinishedMenuItem?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Column header context menu fields

        internal ContextMenuStripCustom FMColumnHeaderRightClickMenu;

        private ToolStripMenuItem[] ColumnHeaderCheckBoxMenuItems;

        private ToolStripMenuItem ResetColumnVisibilityMenuItem;
        private ToolStripMenuItem ResetAllColumnWidthsMenuItem;
        private ToolStripMenuItem ResetColumnPositionsMenuItem;

        private ToolStripSeparator ColumnHeaderRightClickMenuSeparator1;

        private ToolStripMenuItem ShowGameMenuItem;
        private ToolStripMenuItem ShowInstalledMenuItem;
        private ToolStripMenuItem ShowTitleMenuItem;
        private ToolStripMenuItem ShowArchiveMenuItem;
        private ToolStripMenuItem ShowAuthorMenuItem;
        private ToolStripMenuItem ShowSizeMenuItem;
        private ToolStripMenuItem ShowRatingMenuItem;
        private ToolStripMenuItem ShowFinishedMenuItem;
        private ToolStripMenuItem ShowReleaseDateMenuItem;
        private ToolStripMenuItem ShowLastPlayedMenuItem;
        private ToolStripMenuItem ShowDisabledModsMenuItem;
        private ToolStripMenuItem ShowCommentMenuItem;

        #endregion
    }
}
