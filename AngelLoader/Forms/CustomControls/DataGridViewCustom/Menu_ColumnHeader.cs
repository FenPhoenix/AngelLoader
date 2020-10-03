using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class DataGridViewCustom
    {
        private static class ColumnHeaderLLMenu
        {
            #region Control backing fields

            private static bool _constructed;
            private static readonly bool[] _columnCheckedStates = InitializedArray(ColumnsCount, true);

            #endregion

            private static DataGridViewCustom _owner = null!;

            private enum ColumnProperties { Visible, DisplayIndex, Width }

            private static IDisposable[]? _columnHeaderMenuDisposables;

            #region Column header context menu fields

            private static ContextMenuStripCustom? ColumnHeaderContextMenu;

            private static ToolStripMenuItemCustom? ResetColumnVisibilityMenuItem;
            private static ToolStripMenuItemCustom? ResetAllColumnWidthsMenuItem;
            private static ToolStripMenuItemCustom? ResetColumnPositionsMenuItem;

            private static ToolStripSeparator? ColumnHeaderContextMenuSep1;

            private static ToolStripMenuItemCustom[]? ColumnHeaderCheckBoxMenuItems;
            private static ToolStripMenuItemCustom? ShowGameMenuItem;
            private static ToolStripMenuItemCustom? ShowInstalledMenuItem;
            private static ToolStripMenuItemCustom? ShowTitleMenuItem;
            private static ToolStripMenuItemCustom? ShowArchiveMenuItem;
            private static ToolStripMenuItemCustom? ShowAuthorMenuItem;
            private static ToolStripMenuItemCustom? ShowSizeMenuItem;
            private static ToolStripMenuItemCustom? ShowRatingMenuItem;
            private static ToolStripMenuItemCustom? ShowFinishedMenuItem;
            private static ToolStripMenuItemCustom? ShowReleaseDateMenuItem;
            private static ToolStripMenuItemCustom? ShowLastPlayedMenuItem;
            private static ToolStripMenuItemCustom? ShowDateAddedMenuItem;
            private static ToolStripMenuItemCustom? ShowDisabledModsMenuItem;
            private static ToolStripMenuItemCustom? ShowCommentMenuItem;

            #endregion

            #region Private methods

            private static void ResetPropertyOnAllColumns(ColumnProperties property)
            {
                for (int i = 0; i < _owner.Columns.Count; i++)
                {
                    DataGridViewColumn c = _owner.Columns[i];
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

                    _owner.SelectProperly();

                    SetColumnChecked(c.Index, c.Visible);
                }
            }

            #endregion

            #region Private event handlers

            private static void ResetColumnVisibilityMenuItem_Click(object sender, EventArgs e) => ResetPropertyOnAllColumns(ColumnProperties.Visible);

            private static void ResetColumnPositionsMenuItem_Click(object sender, EventArgs e) => ResetPropertyOnAllColumns(ColumnProperties.DisplayIndex);

            private static void ResetAllColumnWidthsMenuItem_Click(object sender, EventArgs e) => ResetPropertyOnAllColumns(ColumnProperties.Width);

            private static void CheckBoxMenuItem_Click(object sender, EventArgs e)
            {
                var s = (ToolStripMenuItemCustom)sender;
                MakeColumnVisible(_owner.Columns[(int)s.Tag], s.Checked);
                _owner.SelectProperly();
            }

            #endregion

            #region API methods

            internal static ContextMenuStrip? GetContextMenu() => ColumnHeaderContextMenu;

            internal static bool Visible => _constructed && ColumnHeaderContextMenu!.Visible;

            internal static void Construct(DataGridViewCustom owner)
            {
                if (_constructed) return;

                _owner = owner;

                #region Instantiation

                _columnHeaderMenuDisposables = new IDisposable[]
                {
                    ColumnHeaderContextMenu = new ContextMenuStripCustom(),
                    ResetColumnVisibilityMenuItem = new ToolStripMenuItemCustom(),
                    ResetAllColumnWidthsMenuItem = new ToolStripMenuItemCustom(),
                    ResetColumnPositionsMenuItem = new ToolStripMenuItemCustom(),
                    ColumnHeaderContextMenuSep1 = new ToolStripSeparator(),
                    ShowGameMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.Game
                    },
                    ShowInstalledMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.Installed
                    },
                    ShowTitleMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.Title
                    },
                    ShowArchiveMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.Archive
                    },
                    ShowAuthorMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.Author
                    },
                    ShowSizeMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.Size
                    },
                    ShowRatingMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.Rating
                    },
                    ShowFinishedMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.Finished
                    },
                    ShowReleaseDateMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.ReleaseDate
                    },
                    ShowLastPlayedMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.LastPlayed
                    },
                    ShowDateAddedMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.DateAdded
                    },
                    ShowDisabledModsMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.DisabledMods
                    },
                    ShowCommentMenuItem = new ToolStripMenuItemCustom
                    {
                        CheckOnClick = true,
                        Tag = Column.Comment
                    }
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
                    ShowDateAddedMenuItem,
                    ShowDisabledModsMenuItem,
                    ShowCommentMenuItem
                };

                for (int i = 0; i < ColumnHeaderCheckBoxMenuItems.Length; i++)
                {
                    ColumnHeaderCheckBoxMenuItems[i].Checked = _columnCheckedStates[i];
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
                    ShowDateAddedMenuItem,
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
                ShowDateAddedMenuItem.Click += CheckBoxMenuItem_Click;
                ShowDisabledModsMenuItem.Click += CheckBoxMenuItem_Click;
                ShowCommentMenuItem.Click += CheckBoxMenuItem_Click;

                #endregion

                _constructed = true;

                SetMenuItemTextToLocalized();
            }

            internal static void SetMenuItemTextToLocalized()
            {
                if (!_constructed) return;

                ResetColumnVisibilityMenuItem!.Text = LText.FMsList.ColumnMenu_ResetAllColumnsToVisible;
                ResetAllColumnWidthsMenuItem!.Text = LText.FMsList.ColumnMenu_ResetAllColumnWidths;
                ResetColumnPositionsMenuItem!.Text = LText.FMsList.ColumnMenu_ResetAllColumnPositions;

                ShowGameMenuItem!.Text = LText.FMsList.GameColumn;
                ShowInstalledMenuItem!.Text = LText.FMsList.InstalledColumn;
                ShowTitleMenuItem!.Text = LText.FMsList.TitleColumn;
                ShowArchiveMenuItem!.Text = LText.FMsList.ArchiveColumn;
                ShowAuthorMenuItem!.Text = LText.FMsList.AuthorColumn;
                ShowSizeMenuItem!.Text = LText.FMsList.SizeColumn;
                ShowRatingMenuItem!.Text = LText.FMsList.RatingColumn;
                ShowFinishedMenuItem!.Text = LText.FMsList.FinishedColumn;
                ShowReleaseDateMenuItem!.Text = LText.FMsList.ReleaseDateColumn;
                ShowLastPlayedMenuItem!.Text = LText.FMsList.LastPlayedColumn;
                ShowDateAddedMenuItem!.Text = LText.FMsList.DateAddedColumn;
                ShowDisabledModsMenuItem!.Text = LText.FMsList.DisabledModsColumn;
                ShowCommentMenuItem!.Text = LText.FMsList.CommentColumn;
            }

            internal static void SetColumnChecked(int index, bool enabled)
            {
                if (_constructed)
                {
                    ColumnHeaderCheckBoxMenuItems![index].Checked = enabled;
                }
                else
                {
                    _columnCheckedStates[index] = enabled;
                }
            }

            internal static void Dispose()
            {
                for (int i = 0; i < _columnHeaderMenuDisposables?.Length; i++)
                {
                    _columnHeaderMenuDisposables?[i]?.Dispose();
                }
            }

            #endregion
        }
    }
}
