using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class FMsDGV_ColumnHeaderLLMenu
    {
        #region Control backing fields

        private bool _constructed;
        private readonly bool[] _columnCheckedStates = InitializedArray(ColumnsCount, true);

        #endregion

        private readonly MainForm _owner;

        private enum ColumnProperties { Visible, DisplayIndex, Width }

        #region Menu item fields

        private DarkContextMenu _menu = null!;
        internal DarkContextMenu Menu
        {
            get
            {
                Construct();
                return _menu;
            }
        }

        private ToolStripMenuItemCustom ResetColumnVisibilityMenuItem = null!;
        private ToolStripMenuItemCustom ResetAllColumnWidthsMenuItem = null!;
        private ToolStripMenuItemCustom ResetColumnPositionsMenuItem = null!;

        private ToolStripMenuItemCustom[] ColumnHeaderCheckBoxMenuItems = null!;
        private ToolStripMenuItemCustom ShowGameMenuItem = null!;
        private ToolStripMenuItemCustom ShowInstalledMenuItem = null!;
        private ToolStripMenuItemCustom ShowTitleMenuItem = null!;
        private ToolStripMenuItemCustom ShowArchiveMenuItem = null!;
        private ToolStripMenuItemCustom ShowAuthorMenuItem = null!;
        private ToolStripMenuItemCustom ShowSizeMenuItem = null!;
        private ToolStripMenuItemCustom ShowRatingMenuItem = null!;
        private ToolStripMenuItemCustom ShowFinishedMenuItem = null!;
        private ToolStripMenuItemCustom ShowReleaseDateMenuItem = null!;
        private ToolStripMenuItemCustom ShowLastPlayedMenuItem = null!;
        private ToolStripMenuItemCustom ShowDateAddedMenuItem = null!;
        private ToolStripMenuItemCustom ShowDisabledModsMenuItem = null!;
        private ToolStripMenuItemCustom ShowCommentMenuItem = null!;

        #endregion

        internal FMsDGV_ColumnHeaderLLMenu(MainForm owner) => _owner = owner;

        #region Private methods

        private void ResetPropertyOnAllColumns(ColumnProperties property)
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

        private void ResetColumnVisibilityMenuItem_Click(object sender, EventArgs e) => ResetPropertyOnAllColumns(ColumnProperties.Visible);

        private void ResetColumnPositionsMenuItem_Click(object sender, EventArgs e) => ResetPropertyOnAllColumns(ColumnProperties.DisplayIndex);

        private void ResetAllColumnWidthsMenuItem_Click(object sender, EventArgs e) => ResetPropertyOnAllColumns(ColumnProperties.Width);

        private void CheckBoxMenuItems_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItemCustom)sender;
            _owner.FMsDGV.MakeColumnVisible(_owner.FMsDGV.Columns[(int)s.Tag], s.Checked);
            _owner.FMsDGV.SelectProperly();
        }

        #endregion

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                _menu.DarkModeEnabled = _darkModeEnabled;
            }
        }

        #region Public methods

        internal bool Visible => _constructed && _menu.Visible;

        private void Construct()
        {
            if (_constructed) return;

            #region Instantiation

            _menu = new DarkContextMenu(_owner.GetComponents()) { Tag = LoadType.Lazy };

            #endregion

            #region Add items to menu and hookup events

            _menu.Items.AddRange(new ToolStripItem[]
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
                _menu.Items.Add(item);
                item.Click += CheckBoxMenuItems_Click;
            }

            ResetColumnVisibilityMenuItem.Click += ResetColumnVisibilityMenuItem_Click;
            ResetAllColumnWidthsMenuItem.Click += ResetAllColumnWidthsMenuItem_Click;
            ResetColumnPositionsMenuItem.Click += ResetColumnPositionsMenuItem_Click;

            #endregion

            _menu.SetPreventCloseOnClickItems(ColumnHeaderCheckBoxMenuItems);

            _menu.DarkModeEnabled = _darkModeEnabled;

            _constructed = true;

            Localize();
        }

        internal void Localize()
        {
            if (!_constructed) return;

            ResetColumnVisibilityMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnsToVisible;
            ResetAllColumnWidthsMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnWidths;
            ResetColumnPositionsMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnPositions;

            ShowGameMenuItem.Text = LText.FMsList.GameColumn;
            ShowInstalledMenuItem.Text = LText.FMsList.InstalledColumn;
            ShowTitleMenuItem.Text = LText.FMsList.TitleColumn;
            ShowArchiveMenuItem.Text = LText.FMsList.ArchiveColumn;
            ShowAuthorMenuItem.Text = LText.FMsList.AuthorColumn;
            ShowSizeMenuItem.Text = LText.FMsList.SizeColumn;
            ShowRatingMenuItem.Text = LText.FMsList.RatingColumn;
            ShowFinishedMenuItem.Text = LText.FMsList.FinishedColumn;
            ShowReleaseDateMenuItem.Text = LText.FMsList.ReleaseDateColumn;
            ShowLastPlayedMenuItem.Text = LText.FMsList.LastPlayedColumn;
            ShowDateAddedMenuItem.Text = LText.FMsList.DateAddedColumn;
            ShowDisabledModsMenuItem.Text = LText.FMsList.DisabledModsColumn;
            ShowCommentMenuItem.Text = LText.FMsList.CommentColumn;
        }

        internal void SetColumnChecked(int index, bool enabled)
        {
            if (_constructed)
            {
                ColumnHeaderCheckBoxMenuItems[index].Checked = enabled;
            }
            else
            {
                _columnCheckedStates[index] = enabled;
            }
        }

        #endregion
    }
}
