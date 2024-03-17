using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class FMsDGV_ColumnHeaderLLMenu : IDarkable
{
    #region Control backing fields

    private bool _constructed;
    private readonly bool[] _columnCheckedStates = InitializedArray(ColumnCount, true);

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

    #endregion

    internal FMsDGV_ColumnHeaderLLMenu(MainForm owner) => _owner = owner;

    #region Private methods

    private void ResetPropertyOnAllColumns(ColumnProperties property)
    {
        try
        {
            _owner.TopSplitContainer.Panel1.SuspendDrawing();

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

                SetColumnChecked(c.Index, c.Visible);
            }
        }
        finally
        {
            _owner.TopSplitContainer.Panel1.ResumeDrawing();
            _owner.FMsDGV.SelectProperly();
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

        _menu = new DarkContextMenu(_owner);

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

        ColumnHeaderCheckBoxMenuItems = InitializedArray<ToolStripMenuItemCustom>(ColumnCount);

        for (int i = 0; i < ColumnHeaderCheckBoxMenuItems.Length; i++)
        {
            ToolStripMenuItemCustom item = ColumnHeaderCheckBoxMenuItems[i];
            item.CheckOnClick = true;
            item.Tag = (Column)i;
            item.Checked = _columnCheckedStates[i];
            item.Click += CheckBoxMenuItems_Click;
            _menu.Items.Add(item);
        }

        #endregion

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

        for (int i = 0; i < ColumnCount; i++)
        {
            ColumnHeaderCheckBoxMenuItems[i].Text = ColumnLocalizedStrings[i].Invoke();
        }
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
