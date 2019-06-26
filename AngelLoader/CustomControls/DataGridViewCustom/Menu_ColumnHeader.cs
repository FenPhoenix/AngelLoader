using System;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.CustomControls
{
    public sealed partial class DataGridViewCustom
    {
        #region Backing fields

        private bool _columnHeaderMenuCreated;
        private readonly bool[] ColumnCheckedStates = { true, true, true, true, true, true, true, true, true, true, true, true };

        #endregion

        private enum ColumnProperties { Visible, DisplayIndex, Width }

        private IDisposable[] ColumnHeaderMenuDisposables;

        #region Column header context menu fields

        internal ContextMenuStripCustom ColumnHeaderContextMenu;

        private ToolStripMenuItem ResetColumnVisibilityMenuItem;
        private ToolStripMenuItem ResetAllColumnWidthsMenuItem;
        private ToolStripMenuItem ResetColumnPositionsMenuItem;

        private ToolStripSeparator ColumnHeaderContextMenuSep1;

        private ToolStripMenuItem[] ColumnHeaderCheckBoxMenuItems;
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

        #region Private methods

        private void InitColumnHeaderContextMenu()
        {
            if (_columnHeaderMenuCreated) return;

            #region Instantiation

            ColumnHeaderMenuDisposables = new IDisposable[]
            {
                (ColumnHeaderContextMenu = new ContextMenuStripCustom { Name = nameof(ColumnHeaderContextMenu) }),
                (ResetColumnVisibilityMenuItem = new ToolStripMenuItem { Name = nameof(ResetColumnVisibilityMenuItem) }),
                (ResetAllColumnWidthsMenuItem = new ToolStripMenuItem { Name = nameof(ResetAllColumnWidthsMenuItem) }),
                (ResetColumnPositionsMenuItem = new ToolStripMenuItem { Name = nameof(ResetColumnPositionsMenuItem) }),
                (ColumnHeaderContextMenuSep1 = new ToolStripSeparator { Name = nameof(ColumnHeaderContextMenuSep1) }),
                (ShowGameMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowGameMenuItem),
                    Tag = Column.Game
                }),
                (ShowInstalledMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowInstalledMenuItem),
                    Tag = Column.Installed
                }),
                (ShowTitleMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowTitleMenuItem),
                    Tag = Column.Title
                }),
                (ShowArchiveMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowArchiveMenuItem),
                    Tag = Column.Archive
                }),
                (ShowAuthorMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowAuthorMenuItem),
                    Tag = Column.Author
                }),
                (ShowSizeMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowSizeMenuItem),
                    Tag = Column.Size
                }),
                (ShowRatingMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowRatingMenuItem),
                    Tag = Column.Rating
                }),
                (ShowFinishedMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowFinishedMenuItem),
                    Tag = Column.Finished
                }),
                (ShowReleaseDateMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowReleaseDateMenuItem),
                    Tag = Column.ReleaseDate
                }),
                (ShowLastPlayedMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowLastPlayedMenuItem),
                    Tag = Column.LastPlayed
                }),
                (ShowDisabledModsMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowDisabledModsMenuItem),
                    Tag = Column.DisabledMods
                }),
                (ShowCommentMenuItem = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Name = nameof(ShowCommentMenuItem),
                    Tag = Column.Comment
                })
            };

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

            for (int i = 0; i < ColumnHeaderCheckBoxMenuItems.Length; i++)
            {
                ColumnHeaderCheckBoxMenuItems[i].Checked = ColumnCheckedStates[i];
            }

            #endregion

            #region Add items to menu

            ColumnHeaderContextMenu.Items.AddRange(new ToolStripItem[]
            {
                ResetColumnVisibilityMenuItem,
                ResetAllColumnWidthsMenuItem,
                ResetColumnPositionsMenuItem,
                ColumnHeaderContextMenuSep1,
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

            ColumnHeaderContextMenu.SetPreventCloseOnClickItems(ColumnHeaderCheckBoxMenuItems);

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

            _columnHeaderMenuCreated = true;

            SetColumnHeaderMenuItemTextToLocalized();
        }

        private void SetColumnHeaderMenuItemTextToLocalized()
        {
            if (!_columnHeaderMenuCreated) return;

            ResetColumnVisibilityMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnsToVisible.EscapeAmpersands();
            ResetAllColumnWidthsMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnWidths.EscapeAmpersands();
            ResetColumnPositionsMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnPositions.EscapeAmpersands();

            ShowGameMenuItem.Text = LText.FMsList.GameColumn.EscapeAmpersands();
            ShowInstalledMenuItem.Text = LText.FMsList.InstalledColumn.EscapeAmpersands();
            ShowTitleMenuItem.Text = LText.FMsList.TitleColumn.EscapeAmpersands();
            ShowArchiveMenuItem.Text = LText.FMsList.ArchiveColumn.EscapeAmpersands();
            ShowAuthorMenuItem.Text = LText.FMsList.AuthorColumn.EscapeAmpersands();
            ShowSizeMenuItem.Text = LText.FMsList.SizeColumn.EscapeAmpersands();
            ShowRatingMenuItem.Text = LText.FMsList.RatingColumn.EscapeAmpersands();
            ShowFinishedMenuItem.Text = LText.FMsList.FinishedColumn.EscapeAmpersands();
            ShowReleaseDateMenuItem.Text = LText.FMsList.ReleaseDateColumn.EscapeAmpersands();
            ShowLastPlayedMenuItem.Text = LText.FMsList.LastPlayedColumn.EscapeAmpersands();
            ShowDisabledModsMenuItem.Text = LText.FMsList.DisabledModsColumn.EscapeAmpersands();
            ShowCommentMenuItem.Text = LText.FMsList.CommentColumn.EscapeAmpersands();
        }
        
        private void ResetPropertyOnAllColumns(ColumnProperties property)
        {
            for (var i = 0; i < Columns.Count; i++)
            {
                DataGridViewColumn c = Columns[i];
                switch (property)
                {
                    case ColumnProperties.Visible:
                        MakeColumnVisible(c, true);
                        break;
                    case ColumnProperties.DisplayIndex:
                        c.DisplayIndex = c.Index;
                        break;
                    case ColumnProperties.Width:
                        if (c.Resizable == DataGridViewTriState.True) c.Width = Defaults.ColumnWidth;
                        break;
                }

                SetColumnChecked(c.Index, c.Visible);
            }
        }

        private void SetColumnChecked(int index, bool enabled)
        {
            if (_columnHeaderMenuCreated)
            {
                ColumnHeaderCheckBoxMenuItems[index].Checked = enabled;
            }
            else
            {
                ColumnCheckedStates[index] = enabled;
            }
        }

        #endregion

        #region Event handlers

        private void ResetColumnVisibilityMenuItem_Click(object sender, EventArgs e) => ResetPropertyOnAllColumns(ColumnProperties.Visible);

        private void ResetColumnPositionsMenuItem_Click(object sender, EventArgs e) => ResetPropertyOnAllColumns(ColumnProperties.DisplayIndex);

        private void ResetAllColumnWidthsMenuItem_Click(object sender, EventArgs e) => ResetPropertyOnAllColumns(ColumnProperties.Width);

        private void CheckBoxMenuItem_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItem)sender;
            MakeColumnVisible(Columns[(int)s.Tag], s.Checked);
        }

        #endregion

        private void DisposeColumnHeaderMenu()
        {
            for (int i = 0; i < ColumnHeaderMenuDisposables?.Length; i++)
            {
                ColumnHeaderMenuDisposables[i]?.Dispose();
            }
        }
    }
}
