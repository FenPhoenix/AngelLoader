﻿using System;
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

            #region Column header context menu fields

            private static ContextMenuStripCustom? ColumnHeaderContextMenu;

            private static ToolStripMenuItemCustom? ResetColumnVisibilityMenuItem;
            private static ToolStripMenuItemCustom? ResetAllColumnWidthsMenuItem;
            private static ToolStripMenuItemCustom? ResetColumnPositionsMenuItem;

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

            private static void CheckBoxMenuItems_Click(object sender, EventArgs e)
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

                ColumnHeaderContextMenu = new ContextMenuStripCustom(_owner._owner.GetComponents());

                #endregion

                #region Add items to menu and hookup events

                ColumnHeaderContextMenu.Items.AddRange(new ToolStripItem[]
                {
                    ResetColumnVisibilityMenuItem = new ToolStripMenuItemCustom(),
                    ResetAllColumnWidthsMenuItem = new ToolStripMenuItemCustom(),
                    ResetColumnPositionsMenuItem = new ToolStripMenuItemCustom(),
                    new ToolStripSeparator()
                });

                #region Fill ColumnHeaderCheckBoxMenuItems array

                ColumnHeaderCheckBoxMenuItems = new[]
                {
                    ShowGameMenuItem = new ToolStripMenuItemCustom(),
                    ShowInstalledMenuItem = new ToolStripMenuItemCustom(),
                    ShowTitleMenuItem = new ToolStripMenuItemCustom(),
                    ShowArchiveMenuItem = new ToolStripMenuItemCustom(),
                    ShowAuthorMenuItem = new ToolStripMenuItemCustom(),
                    ShowSizeMenuItem = new ToolStripMenuItemCustom(),
                    ShowRatingMenuItem = new ToolStripMenuItemCustom(),
                    ShowFinishedMenuItem = new ToolStripMenuItemCustom(),
                    ShowReleaseDateMenuItem = new ToolStripMenuItemCustom(),
                    ShowLastPlayedMenuItem = new ToolStripMenuItemCustom(),
                    ShowDateAddedMenuItem = new ToolStripMenuItemCustom(),
                    ShowDisabledModsMenuItem = new ToolStripMenuItemCustom(),
                    ShowCommentMenuItem = new ToolStripMenuItemCustom()
                };

                for (int i = 0; i < ColumnHeaderCheckBoxMenuItems.Length; i++)
                {
                    var item = ColumnHeaderCheckBoxMenuItems[i];
                    item.CheckOnClick = true;
                    item.Tag = (Column)i;
                    item.Checked = _columnCheckedStates[i];
                }

                #endregion

                foreach (var item in ColumnHeaderCheckBoxMenuItems)
                {
                    ColumnHeaderContextMenu.Items.Add(item);
                    item.Click += CheckBoxMenuItems_Click;
                }

                ResetColumnVisibilityMenuItem.Click += ResetColumnVisibilityMenuItem_Click;
                ResetAllColumnWidthsMenuItem.Click += ResetAllColumnWidthsMenuItem_Click;
                ResetColumnPositionsMenuItem.Click += ResetColumnPositionsMenuItem_Click;

                #endregion

                ColumnHeaderContextMenu.SetPreventCloseOnClickItems(ColumnHeaderCheckBoxMenuItems);

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

            #endregion
        }
    }
}
