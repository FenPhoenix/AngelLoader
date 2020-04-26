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
            // Internal only because of an assert for length and I don't wanna make a method just for that
            internal static readonly bool[] ColumnCheckedStates = { true, true, true, true, true, true, true, true, true, true, true, true, true };

            #endregion

#pragma warning disable 8618
            private static DataGridViewCustom Owner;
#pragma warning restore 8618

            private enum ColumnProperties { Visible, DisplayIndex, Width }

            private static IDisposable[]? ColumnHeaderMenuDisposables;

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
                for (int i = 0; i < Owner.Columns.Count; i++)
                {
                    DataGridViewColumn c = Owner.Columns[i];
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

                    Owner.SelectProperly();

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
                MakeColumnVisible(Owner.Columns[(int)s.Tag], s.Checked);
                Owner.SelectProperly();
            }

            #endregion

            #region Internal methods

            internal static ContextMenuStrip? GetContextMenu() => ColumnHeaderContextMenu;

            internal static void Construct(DataGridViewCustom owner)
            {
                if (_constructed) return;

                Owner = owner;

                #region Instantiation

                ColumnHeaderMenuDisposables = new IDisposable[]
                {
                    ColumnHeaderContextMenu = new ContextMenuStripCustom { Name = nameof(ColumnHeaderContextMenu) },
                    ResetColumnVisibilityMenuItem = new ToolStripMenuItem { Name = nameof(ResetColumnVisibilityMenuItem) },
                    ResetAllColumnWidthsMenuItem = new ToolStripMenuItem { Name = nameof(ResetAllColumnWidthsMenuItem) },
                    ResetColumnPositionsMenuItem = new ToolStripMenuItem { Name = nameof(ResetColumnPositionsMenuItem) },
                    ColumnHeaderContextMenuSep1 = new ToolStripSeparator { Name = nameof(ColumnHeaderContextMenuSep1) },
                    ShowGameMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowGameMenuItem),
                        Tag = Column.Game
                    },
                    ShowInstalledMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowInstalledMenuItem),
                        Tag = Column.Installed
                    },
                    ShowTitleMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowTitleMenuItem),
                        Tag = Column.Title
                    },
                    ShowArchiveMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowArchiveMenuItem),
                        Tag = Column.Archive
                    },
                    ShowAuthorMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowAuthorMenuItem),
                        Tag = Column.Author
                    },
                    ShowSizeMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowSizeMenuItem),
                        Tag = Column.Size
                    },
                    ShowRatingMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowRatingMenuItem),
                        Tag = Column.Rating
                    },
                    ShowFinishedMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowFinishedMenuItem),
                        Tag = Column.Finished
                    },
                    ShowReleaseDateMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowReleaseDateMenuItem),
                        Tag = Column.ReleaseDate
                    },
                    ShowLastPlayedMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowLastPlayedMenuItem),
                        Tag = Column.LastPlayed
                    },
                    ShowDateAddedMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowDateAddedMenuItem),
                        Tag = Column.DateAdded
                    },
                    ShowDisabledModsMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowDisabledModsMenuItem),
                        Tag = Column.DisabledMods
                    },
                    ShowCommentMenuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = nameof(ShowCommentMenuItem),
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
                    ColumnCheckedStates[index] = enabled;
                }
            }

            internal static void Dispose()
            {
                for (int i = 0; i < ColumnHeaderMenuDisposables?.Length; i++)
                {
                    ColumnHeaderMenuDisposables?[i]?.Dispose();
                }
            }

            #endregion
        }
    }
}
