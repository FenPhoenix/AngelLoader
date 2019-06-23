using System.Windows.Forms;
using AngelLoader.Common.DataClasses;

namespace AngelLoader.CustomControls
{
    public sealed partial class DataGridViewCustom
    {
        private void InitializeComponent()
        {
            #region Init column header context menu

            FMColumnHeaderRightClickMenu.Name = "FMColumnHeaderRightClickMenu";

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

            FMColumnHeaderRightClickMenu.SetPreventCloseOnClickItems(ColumnHeaderCheckBoxMenuItems);

            ShowGameMenuItem.Tag = Column.Game;
            ShowInstalledMenuItem.Tag = Column.Installed;
            ShowTitleMenuItem.Tag = Column.Title;
            ShowArchiveMenuItem.Tag = Column.Archive;
            ShowAuthorMenuItem.Tag = Column.Author;
            ShowSizeMenuItem.Tag = Column.Size;
            ShowRatingMenuItem.Tag = Column.Rating;
            ShowFinishedMenuItem.Tag = Column.Finished;
            ShowReleaseDateMenuItem.Tag = Column.ReleaseDate;
            ShowLastPlayedMenuItem.Tag = Column.LastPlayed;
            ShowDisabledModsMenuItem.Tag = Column.DisabledMods;
            ShowCommentMenuItem.Tag = Column.Comment;

            #endregion
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

        #region Fields

        private ToolStripMenuItem[] ColumnHeaderCheckBoxMenuItems;
        internal ContextMenuStripCustom FMColumnHeaderRightClickMenu = new ContextMenuStripCustom();
        private readonly ToolStripMenuItem ResetColumnVisibilityMenuItem = new ToolStripMenuItem
        {
            Name = "ResetColumnVisibilityMenuItem"
        };
        private readonly ToolStripMenuItem ResetAllColumnWidthsMenuItem = new ToolStripMenuItem
        {
            Name = "ResetAllColumnWidthsMenuItem"
        };
        private readonly ToolStripMenuItem ResetColumnPositionsMenuItem = new ToolStripMenuItem
        {
            Name = "ResetColumnPositionsMenuItem"
        };
        private readonly ToolStripSeparator ColumnHeaderRightClickMenuSeparator1 = new ToolStripSeparator
        {
            Name = "ColumnHeaderRightClickMenuSeparator1"
        };
        private readonly ToolStripMenuItem ShowGameMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowGameMenuItem"
        };
        private readonly ToolStripMenuItem ShowInstalledMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowInstalledMenuItem"
        };
        private readonly ToolStripMenuItem ShowTitleMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowTitleMenuItem"
        };
        private readonly ToolStripMenuItem ShowArchiveMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowArchiveMenuItem"
        };
        private readonly ToolStripMenuItem ShowAuthorMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowAuthorMenuItem"
        };
        private readonly ToolStripMenuItem ShowSizeMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowSizeMenuItem"
        };
        private readonly ToolStripMenuItem ShowRatingMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowRatingMenuItem"
        };
        private readonly ToolStripMenuItem ShowFinishedMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowFinishedMenuItem"
        };
        private readonly ToolStripMenuItem ShowReleaseDateMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowReleaseDateMenuItem"
        };
        private readonly ToolStripMenuItem ShowLastPlayedMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowLastPlayedMenuItem"
        };
        private readonly ToolStripMenuItem ShowDisabledModsMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowDisabledModsMenuItem"
        };
        private readonly ToolStripMenuItem ShowCommentMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowCommentMenuItem"
        };

        #endregion
    }
}
