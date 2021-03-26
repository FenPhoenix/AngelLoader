using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AL_Common.CommonUtils;
using static AngelLoader.Forms.ControlUtils;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class FMsDGV_ColumnHeaderLLMenu
    {
        #region Control backing fields

        private static bool _constructed;
        private static readonly bool[] _columnCheckedStates = InitializedArray(ColumnsCount, true);

        #endregion

        private static MainForm _owner = null!;

        private enum ColumnProperties { Visible, DisplayIndex, Width }

        #region Column header context menu fields

        private static ContextMenuStripCustom? Menu;

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
            for (int i = 0; i < _owner.FMsDGV.Columns.Count; i++)
            {
                DataGridViewColumn c = _owner.FMsDGV.Columns[i];
                switch (property)
                {
                    case ColumnProperties.Visible:
                        _owner.FMsDGV.MakeColumnVisible(c, true);
                        break;
                    case ColumnProperties.DisplayIndex:
                        c.DisplayIndex = c.Index;
                        break;
                    case ColumnProperties.Width:
                        if (c.Resizable == DataGridViewTriState.True) c.Width = Defaults.ColumnWidth;
                        break;
                }

                _owner.FMsDGV.SelectProperly();

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
            _owner.FMsDGV.MakeColumnVisible(_owner.FMsDGV.Columns[(int)s.Tag], s.Checked);
            _owner.FMsDGV.SelectProperly();
        }

        #endregion

        private static bool _darkModeEnabled;
        public static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                Menu!.DarkModeEnabled = _darkModeEnabled;
            }
        }

        #region API methods

        internal static ContextMenuStrip? GetContextMenu() => Menu;

        internal static bool Visible => _constructed && Menu!.Visible;

        internal static void Construct(MainForm owner)
        {
            if (_constructed) return;

            _owner = owner;

            #region Instantiation

            Menu = new ContextMenuStripCustom(_darkModeEnabled, _owner.GetComponents()) { Tag = LazyLoaded.True };

            #endregion

            #region Add items to menu and hookup events

            Menu.Items.AddRange(new ToolStripItem[]
            {
                    ResetColumnVisibilityMenuItem = new ToolStripMenuItemCustom{ Tag = LazyLoaded.True },
                    ResetAllColumnWidthsMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ResetColumnPositionsMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    new ToolStripSeparator { Tag = LazyLoaded.True }
            });

            #region Fill ColumnHeaderCheckBoxMenuItems array

            ColumnHeaderCheckBoxMenuItems = new[]
            {
                    ShowGameMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowInstalledMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowTitleMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowArchiveMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowAuthorMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowSizeMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowRatingMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowFinishedMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowReleaseDateMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowLastPlayedMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowDateAddedMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowDisabledModsMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                    ShowCommentMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True }
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
                Menu.Items.Add(item);
                item.Click += CheckBoxMenuItems_Click;
            }

            ResetColumnVisibilityMenuItem.Click += ResetColumnVisibilityMenuItem_Click;
            ResetAllColumnWidthsMenuItem.Click += ResetAllColumnWidthsMenuItem_Click;
            ResetColumnPositionsMenuItem.Click += ResetColumnPositionsMenuItem_Click;

            #endregion

            Menu.SetPreventCloseOnClickItems(ColumnHeaderCheckBoxMenuItems);

            _constructed = true;

            Localize();
        }

        internal static void Localize()
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
