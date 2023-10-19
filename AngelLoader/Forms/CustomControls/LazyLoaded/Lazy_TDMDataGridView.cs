using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_TDMDataGridView : IDarkable
{
    // @TDM(DGV): Add all relevant FMsDGV features (match zoom to FMsDGV, multisel+key hacks, etc.)

    #region Backing fields

    private bool _constructed;

    #endregion

    internal DataGridViewImageColumn UpdateColumn = null!;
    internal DataGridViewImageColumn LanguagePackColumn = null!;
    internal DataGridViewTextBoxColumn VersionColumn = null!;
    internal DataGridViewTextBoxColumn TitleColumn = null!;
    internal DataGridViewTextBoxColumn AuthorColumn = null!;
    internal DataGridViewTextBoxColumn SizeColumn = null!;
    internal DataGridViewTextBoxColumn ReleaseDateColumn = null!;

    private readonly ColumnData<TDMColumn>[] _columnData = new ColumnData<TDMColumn>[TDMColumnCount];

    private DataGridViewTDM _dgv = null!;
    internal DataGridViewTDM DGV
    {
        get
        {
            Construct();
            return _dgv;
        }
    }

    private readonly MainForm _owner;

    internal Lazy_TDMDataGridView(MainForm owner) => _owner = owner;

    private bool _darkModeEnabled;

    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!_constructed) return;

            _dgv.DarkModeEnabled = value;
        }
    }

    private void Construct()
    {
        if (_constructed) return;

        UpdateColumn = new DataGridViewImageColumn();
        LanguagePackColumn = new DataGridViewImageColumn();
        VersionColumn = new DataGridViewTextBoxColumn();
        TitleColumn = new DataGridViewTextBoxColumn();
        AuthorColumn = new DataGridViewTextBoxColumn();
        SizeColumn = new DataGridViewTextBoxColumn();
        ReleaseDateColumn = new DataGridViewTextBoxColumn();

        _dgv = new DataGridViewTDM();
        _dgv.AllowUserToAddRows = false;
        _dgv.AllowUserToDeleteRows = false;
        _dgv.AllowUserToOrderColumns = true;
        _dgv.AllowUserToResizeRows = false;
        _dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        _dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _dgv.Columns.AddRange(
            UpdateColumn,
            LanguagePackColumn,
            VersionColumn,
            TitleColumn,
            AuthorColumn,
            SizeColumn,
            ReleaseDateColumn);
        _dgv.BackgroundColor = SystemColors.ControlDark;
        _dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        _dgv.Location = new Point(1, 26);
        // @TDM: Enable this again when we can support it fully
        //_dgv.MultiSelect = true;
        _dgv.ReadOnly = true;
        _dgv.RowHeadersVisible = false;
        _dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _dgv.Size = _owner.FMsDGV.Size;
        _dgv.StandardTab = true;
        _dgv.TabIndex = 0;
        _dgv.VirtualMode = true;
        // @TDM: Implement these commented-out ones
        //_dgv.CellDoubleClick += _dgv_CellDoubleClick;
        _dgv.CellValueNeeded += _dgv.DGV_CellValueNeeded;
        /*
        _dgv.ColumnHeaderMouseClick += _dgv_ColumnHeaderMouseClick;
        _dgv.SelectionChanged += _dgv_SelectionChanged;
        _dgv.KeyDown += _dgv_KeyDown;
        _dgv.MainSelectedRowChanged += _dgv_MainSelectedRowChanged;
        _dgv.MouseDown += _dgv_MouseDown;
        */

        _dgv.SetOwner(_owner);

        SizeColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        foreach (DataGridViewColumn col in _dgv.Columns)
        {
            if (col is DataGridViewImageColumn imgCol)
            {
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
            }

            col.MinimumWidth = Defaults.MinColumnWidth;
            col.ReadOnly = true;
            col.SortMode = DataGridViewColumnSortMode.Programmatic;
        }

        _constructed = true;

        Localize();

        _dgv.SetColumnData(_columnData);
        _owner.TopSplitContainer.Panel1.Controls.Add(_dgv);
        _dgv.BringToFront();

        _dgv.DarkModeEnabled = _darkModeEnabled;

        _dgv.SortDGV(Config.TDMSortedColumn, Config.TDMSortDirection);
    }

    internal void Localize()
    {
        if (!_constructed) return;

        UpdateColumn.HeaderText = "Update";
        LanguagePackColumn.HeaderText = "Language Pack";
        VersionColumn.HeaderText = "Version";
        TitleColumn.HeaderText = "Title";
        AuthorColumn.HeaderText = "Author";
        SizeColumn.HeaderText = "Size";
        ReleaseDateColumn.HeaderText = "Release Date";

        // @TDM(DGV Localize): implement
    }

    internal bool Visible => _constructed && _dgv.Visible;

    internal async Task Show(bool value)
    {
        if (value)
        {
            DGV.Show();
            await _dgv.LoadData();
        }
        else
        {
            if (_constructed)
            {
                _dgv.Hide();
            }
        }
    }

    private TDMColumn _currentSortedColumn;
    internal TDMColumn CurrentSortedColumn
    {
        get => _constructed ? _dgv.CurrentSortedColumn : _currentSortedColumn;
        set
        {
            if (_constructed)
            {
                _dgv.CurrentSortedColumn = value;
            }
            else
            {
                _currentSortedColumn = value;
            }
        }
    }

    private SortDirection _currentSortDirection;
    internal SortDirection CurrentSortDirection
    {
        get => _constructed ? _dgv.CurrentSortDirection : _currentSortDirection;
        set
        {
            if (_constructed)
            {
                _dgv.CurrentSortDirection = value;
            }
            else
            {
                _currentSortDirection = value;
            }
        }
    }

    internal ColumnData<TDMColumn>[] GetColumnData()
    {
        if (_constructed)
        {
            return _dgv.GetColumnData();
        }
        else
        {
            return _columnData;
        }
    }

    internal void SetColumnData(ColumnData<TDMColumn>[] columnData)
    {
        if (_constructed)
        {
            _dgv.SetColumnData(columnData);
        }
        else
        {
            Array.Copy(columnData, _columnData, TDMColumnCount);
        }
    }
}
