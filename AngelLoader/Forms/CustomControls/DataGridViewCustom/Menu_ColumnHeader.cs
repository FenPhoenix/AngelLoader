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

            private static ToolStripMenuItem? ResetColumnVisibilityMenuItem;
            private static ToolStripMenuItem? ResetAllColumnWidthsMenuItem;
            private static ToolStripMenuItem? ResetColumnPositionsMenuItem;

            private static ToolStripSeparator? ColumnHeaderContextMenuSep1;

            private static ToolStripMenuItem[]? ColumnHeaderCheckBoxMenuItems;
            private static ToolStripMenuItem? ShowGameMenuItem;
            private static ToolStripMenuItem? ShowInstalledMenuItem;
            private static ToolStripMenuItem? ShowTitleMenuItem;
            private static ToolStripMenuItem? ShowArchiveMenuItem;
            private static ToolStripMenuItem? ShowAuthorMenuItem;
            private static ToolStripMenuItem? ShowSizeMenuItem;
            private static ToolStripMenuItem? ShowRatingMenuItem;
            private static ToolStripMenuItem? ShowFinishedMenuItem;
            private static ToolStripMenuItem? ShowReleaseDateMenuItem;
            private static ToolStripMenuItem? ShowLastPlayedMenuItem;
            private static ToolStripMenuItem? ShowDateAddedMenuItem;
            private static ToolStripMenuItem? ShowDisabledModsMenuItem;
            private static ToolStripMenuItem? ShowCommentMenuItem;

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
                var s = (ToolStripMenuItem)sender;
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
                    ResetColumnVisibilityMenuItem = new ToolStripMenuItem(),
                    ResetAllColumnWidthsMenuItem = new ToolStripMenuItem(),
                    ResetColumnPositionsMenuItem = new ToolStripMenuItem(),
                    ColumnHeaderContextMenuSep1 = new ToolStripSeparator(),
                    ShowGameMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.Game
                    },
                    ShowInstalledMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.Installed
                    },
                    ShowTitleMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.Title
                    },
                    ShowArchiveMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.Archive
                    },
                    ShowAuthorMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.Author
                    },
                    ShowSizeMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.Size
                    },
                    ShowRatingMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.Rating
                    },
                    ShowFinishedMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.Finished
                    },
                    ShowReleaseDateMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.ReleaseDate
                    },
                    ShowLastPlayedMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.LastPlayed
                    },
                    ShowDateAddedMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.DateAdded
                    },
                    ShowDisabledModsMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Tag = Column.DisabledMods
                    },
                    ShowCommentMenuItem = new ToolStripMenuItem
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

                ResetColumnVisibilityMenuItem!.Text = LText.FMsList.ColumnMenu_ResetAllColumnsToVisible.EscapeAmpersands();
                ResetAllColumnWidthsMenuItem!.Text = LText.FMsList.ColumnMenu_ResetAllColumnWidths.EscapeAmpersands();
                ResetColumnPositionsMenuItem!.Text = LText.FMsList.ColumnMenu_ResetAllColumnPositions.EscapeAmpersands();

                ShowGameMenuItem!.Text = LText.FMsList.GameColumn.EscapeAmpersands();
                ShowInstalledMenuItem!.Text = LText.FMsList.InstalledColumn.EscapeAmpersands();
                ShowTitleMenuItem!.Text = LText.FMsList.TitleColumn.EscapeAmpersands();
                ShowArchiveMenuItem!.Text = LText.FMsList.ArchiveColumn.EscapeAmpersands();
                ShowAuthorMenuItem!.Text = LText.FMsList.AuthorColumn.EscapeAmpersands();
                ShowSizeMenuItem!.Text = LText.FMsList.SizeColumn.EscapeAmpersands();
                ShowRatingMenuItem!.Text = LText.FMsList.RatingColumn.EscapeAmpersands();
                ShowFinishedMenuItem!.Text = LText.FMsList.FinishedColumn.EscapeAmpersands();
                ShowReleaseDateMenuItem!.Text = LText.FMsList.ReleaseDateColumn.EscapeAmpersands();
                ShowLastPlayedMenuItem!.Text = LText.FMsList.LastPlayedColumn.EscapeAmpersands();
                ShowDateAddedMenuItem!.Text = LText.FMsList.DateAddedColumn.EscapeAmpersands();
                ShowDisabledModsMenuItem!.Text = LText.FMsList.DisabledModsColumn.EscapeAmpersands();
                ShowCommentMenuItem!.Text = LText.FMsList.CommentColumn.EscapeAmpersands();
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
