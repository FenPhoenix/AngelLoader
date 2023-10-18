using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_TDMDataGridView : IDarkable
{
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

    private DataGridViewTDM _dgv = null!;
    internal DataGridViewTDM DGV
    {
        get
        {
            Construct();
            return _dgv;
        }
    }

    private readonly List<TDM_ServerFMData> _tdmServerDataList = new();

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
        _dgv.MultiSelect = true;
        _dgv.ReadOnly = true;
        _dgv.RowHeadersVisible = false;
        _dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _dgv.Size = _owner.FMsDGV.Size;
        _dgv.StandardTab = true;
        _dgv.TabIndex = 0;
        _dgv.VirtualMode = true;
        // @TDM: Implement these commented-out ones
        //_dgv.CellDoubleClick += _dgv_CellDoubleClick;
        _dgv.CellValueNeeded += CellValueNeeded;
        /*
        _dgv.ColumnHeaderMouseClick += _dgv_ColumnHeaderMouseClick;
        _dgv.SelectionChanged += _dgv_SelectionChanged;
        _dgv.KeyDown += _dgv_KeyDown;
        _dgv.MainSelectedRowChanged += _dgv_MainSelectedRowChanged;
        _dgv.MouseDown += _dgv_MouseDown;
        */

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
    }

    internal void Localize()
    {
        if (!_constructed) return;

        // @TDM(DGV Localize): implement
    }

    private void CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
    {
        if (_tdmServerDataList.Count == 0) return;

        TDM_ServerFMData data = _tdmServerDataList[e.RowIndex];

        switch ((TDMColumn)e.ColumnIndex)
        {
            /*
            @TDM: Make images for these. Actually maybe just have one column
            I think the game notifies you of the updates in order, like if it's an update then it'll say *
            but then once you update if there's still a lang pack, then it'll say #. I did some testing but
            kind of forgot the exact details. Double-check this.
            */
            case TDMColumn.Update:
                e.Value = Images.Blank;
                break;
            case TDMColumn.LanguagePack:
                e.Value = Images.Blank;
                break;

            case TDMColumn.Version:
                e.Value = data.Version;
                break;
            case TDMColumn.Title:
                e.Value = data.Title;
                break;
            case TDMColumn.Author:
                e.Value = data.Author;
                break;
            case TDMColumn.Size:
                e.Value = data.Size + " " + LText.Global.MegabyteShort;
                break;
            case TDMColumn.ReleaseDate:
                DateTime? releaseDate = data.ReleaseDateDT;
                e.Value = releaseDate != null ? FormatDate((DateTime)releaseDate) : data.ReleaseDate;
                break;
        }
    }
}
