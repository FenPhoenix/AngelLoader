using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms.CustomControls;

public sealed class DataGridViewTDM : DataGridViewCustomBase, IEventDisabler, IZeroSelectCodeDisabler
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int EventsDisabled { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int ZeroSelectCodeDisabled { get; set; }

    internal List<TDM_ServerFMData> _serverFMDataList = new();
    private CancellationTokenSource _serverFMDataCTS = new();
    private CancellationTokenSource _serverFMDetailsCTS = new();
    private CancellationTokenSource _screenshotCTS = new();

    protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
    {
        base.OnCellPainting(e);

        if (!_darkModeEnabled) return;

        if (e.RowIndex == -1)
        {
            DrawColumnHeaders(e, (int)CurrentSortedColumn);
        }
        else if (e.RowIndex > -1)
        {
            bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;

            SolidBrush bgBrush = isSelected
                ? DarkColors.BlueSelectionBrush
                : DarkColors.Fen_DarkBackgroundBrush;

            Pen borderPen = DarkColors.Fen_DGVCellBordersPen;
            DrawRows(e, isSelected, bgBrush, borderPen, borderPen);
        }
    }

    internal void SetColumnData(ColumnData<TDMColumn>[] columnData)
    {
        if (columnData.Length == 0) return;

        #region Important

        // IMPORTANT (SetColumnData):
        // Do not remove! DataGridViewColumn.DisplayIndex changes the DisplayIndex of other columns when you
        // set it. That means we can't step through columns in order and set their DisplayIndex to whatever
        // it should be; instead we have to step through DisplayIndexes in order and assign them to whatever
        // column they should be assigned to.

        // Wrong:
        // Column[0].DisplayIndex = 4; Column[1].DisplayIndex = 12; etc.

        // Right:
        // Column[10].DisplayIndex = 0; Column[3].DisplayIndex = 1; etc.

        ColumnData<TDMColumn>[] columnDataSorted = columnData.OrderBy(static x => x.DisplayIndex).ToArray();

        #endregion

        foreach (ColumnData<TDMColumn> colData in columnDataSorted)
        {
            DataGridViewColumn col = Columns[(int)colData.Id];

            col.DisplayIndex = colData.DisplayIndex;
            if (col.Resizable == DataGridViewTriState.True) col.Width = colData.Width;
        }
    }

    internal ColumnData<TDMColumn>[] GetColumnData()
    {
        var columns = new ColumnData<TDMColumn>[ColumnCount];

        for (int i = 0; i < ColumnCount; i++)
        {
            DataGridViewColumn col = Columns[i];
            columns[i] = new ColumnData<TDMColumn>((TDMColumn)col.Index, ColumnCount)
            {
                DisplayIndex = col.DisplayIndex,
                Visible = true,
                Width = col.Width
            };
        }

        return columns.OrderBy(static x => x.Id).ToArray();
    }

    internal TDMColumn CurrentSortedColumn;

    internal void SortDGV(TDMColumn column, SortDirection sortDirection)
    {
        CurrentSortedColumn = column;
        CurrentSortDirection = sortDirection;

        //Core.SortFMsViewList(column, sortDirection);
        // @TDM: Sort actual items here

        SetSortGlyph((int)column);
    }

    protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e)
    {
        base.OnColumnHeaderMouseClick(e);

        if (e.Button != MouseButtons.Left) return;

        SelectedFM? selFM = RowSelected() ? GetMainSelectedFMPosInfo() : null;

        SortDirection newSortDirection =
            e.ColumnIndex == (int)CurrentSortedColumn
                ? CurrentSortDirection == SortDirection.Ascending
                    ? SortDirection.Descending
                    : SortDirection.Ascending
                : ColumnDefaultSortDirections[e.ColumnIndex];

        SortDGV((TDMColumn)e.ColumnIndex, newSortDirection);

        //Core.SetFilter();
        if (RefreshFMsList(selFM, keepSelection: KeepSel.TrueNearest, fromColumnClick: true))
        {
            if (selFM != null && RowSelected() &&
                selFM.InstalledName != GetMainSelectedFM().InternalName)
            {
                //_displayedFM = await Core.DisplayFM();
                DisplayFMData(GetMainSelectedFM());
            }
        }
    }

    private void ClearShownData()
    {

    }

    private void DisplayFMData(TDM_ServerFMData data)
    {
        Trace.WriteLine("Basic data: " + data.Title + " etc.");
    }

    private bool RefreshFMsList(
        SelectedFM? selectedFM,
        bool startup = false,
        KeepSel keepSelection = KeepSel.False,
        bool fromColumnClick = false,
        TDM_ServerFMData[]? multiSelectedFMs = null)
    {
        using (new DisableEvents(this))
        {
            // A small but measurable perf increase from this. Also prevents flickering when switching game
            // tabs.
            if (!startup)
            {
                this.SuspendDrawing();
                // I think FMsDGV.SuspendDrawing() doesn't actually really work because it's a .NET control,
                // not a direct wrapper around a Win32 one. So that's why we need this. It's possible we
                // might not need this if we suspend/resume FMsDGV's parent control?
                CellValueNeededDisabled = true;
            }

            try
            {
                SuppressSelectionEvent = true;

                // Prevents:
                // -a glitched row from being drawn at the end in certain situations
                // -the subsequent row count set from being really slow
                Rows.Clear();

                RowCount = FilterShownIndexList.Count;
            }
            finally
            {
                SuppressSelectionEvent = false;
            }

            if (RowCount == 0)
            {
                if (!startup)
                {
                    CellValueNeededDisabled = false;
                    this.ResumeDrawing();
                }
                ClearShownData();
                return false;
            }
            else
            {
                int row;
                if (keepSelection == KeepSel.False)
                {
                    row = 0;
                    FirstDisplayedScrollingRowIndex = 0;
                }
                else
                {
                    SelectedFM selFM = selectedFM ?? new SelectedFM();
                    bool findNearest = keepSelection == KeepSel.TrueNearest && selectedFM != null;
                    row = GetIndexFromInstalledName(selFM.InstalledName, findNearest).ClampToZero();
                    try
                    {
                        if (fromColumnClick)
                        {
                            FirstDisplayedScrollingRowIndex =
                                CurrentSortDirection == SortDirection.Ascending
                                    ? row.ClampToZero()
                                    : (row - DisplayedRowCount(true)).ClampToZero();
                        }
                        else
                        {
                            FirstDisplayedScrollingRowIndex = (row - selFM.IndexFromTop).ClampToZero();
                        }
                    }
                    catch
                    {
                        // no room is available to display rows
                    }
                }

                bool selectDoneAtLeastOnce = false;
                void DoSelect()
                {
                    SelectSingle(row, suppressSelectionChangedEvent: !selectDoneAtLeastOnce);
                    SelectProperly(suspendResume: startup);
                    selectDoneAtLeastOnce = true;
                }

                // @SEL_SYNC_HACK
                // Stupid hack to attempt to prevent multiselect-set-popping-back-to-starting-at-list-top
                MultiSelect = false;
                DoSelect();
                MultiSelect = true;
                DoSelect();

                if (multiSelectedFMs != null)
                {
                    HashSet<TDM_ServerFMData> hash = multiSelectedFMs.ToHashSet();
                    DataGridViewRow? latestRow = null;
                    try
                    {
                        SuppressSelectionEvent = true;

                        for (int i = 0; i < FilterShownIndexList.Count; i++)
                        {
                            if (hash.Contains(GetFMFromIndex(i)))
                            {
                                latestRow = Rows[i];
                                latestRow.Selected = true;
                            }
                        }

                        // Match original behavior
                        if (latestRow != null) MainSelectedRow = latestRow;
                    }
                    finally
                    {
                        SuppressSelectionEvent = false;
                    }
                }

                // Resume drawing before loading the readme; that way the list will update instantly even
                // if the readme doesn't. The user will see delays in the "right place" (the readme box)
                // and understand why it takes a sec. Otherwise, it looks like merely changing tabs brings
                // a significant delay, and that's annoying because it doesn't seem like it should happen.
                if (!startup)
                {
                    CellValueNeededDisabled = false;
                    this.ResumeDrawing();
                }
            }
        }

        return true;
    }

    internal int GetIndexFromInstalledName(string installedName, bool findNearest)
    {
        if (installedName.IsEmpty()) return 0;

        for (int i = 0; i < FilterShownIndexList.Count; i++)
        {
            if (GetFMFromIndex(i).InternalName.EqualsI(installedName)) return i;
        }

        if (findNearest)
        {
            for (int i = 0; i < _serverFMDataList.Count; i++)
            {
                if (_serverFMDataList[i].InternalName.EqualsI(installedName))
                {
                    for (int j = i; j < _serverFMDataList.Count; j++)
                    {
                        int index = FilterShownIndexList.IndexOf(j);
                        if (index > -1) return index;
                    }
                    for (int j = i; j > 0; j--)
                    {
                        int index = FilterShownIndexList.IndexOf(j);
                        if (index > -1) return index;
                    }
                    break;
                }
            }
        }

        return 0;
    }

    /// <summary>
    /// Gets the currently selected FM, taking the currently set filters into account.
    /// </summary>
    /// <returns></returns>
    internal TDM_ServerFMData GetMainSelectedFM()
    {
        AssertR(GetRowSelectedCount() > 0, nameof(GetMainSelectedFM) + ": no rows selected!");
        AssertR(MainSelectedRow != null, nameof(MainSelectedRow) + " is null when it shouldn't be");

        return GetFMFromIndex(MainSelectedRow!.Index);
    }

    internal SelectedFM GetMainSelectedFMPosInfo() =>
        !RowSelected()
            ? new SelectedFM { InstalledName = "", IndexFromTop = 0 }
            : GetFMPosInfoFromIndex(index: MainSelectedRow!.Index);

    internal SelectedFM GetFMPosInfoFromIndex(int index)
    {
        var ret = new SelectedFM { InstalledName = "", IndexFromTop = 0 };

        // TODO/BUG: GetFMPosInfoFromIndex(): This is a leftover from when this just got the selection, instead of an explicit index.
        // I'm pretty sure this never gets hit in practice, but the Delete code ends up calling this and
        // I haven't checked that code thoroughly. All other callsites appear to not be able to hit this.
        // If we can determine this never gets hit, we should remove it because it's unintended logic (but
        // we just want to know we don't accidentally depend on it).
        // But we should replace it with a RowCount > 0 check, because this also implicitly guards against
        // that, and we do want that guard check.
        if (!RowSelected()) return ret;

        int firstDisplayed = FirstDisplayedScrollingRowIndex;
        int lastDisplayed = firstDisplayed + DisplayedRowCount(false);

        int indexFromTop = index >= firstDisplayed && index <= lastDisplayed
            ? index - firstDisplayed
            : DisplayedRowCount(true) / 2;

        ret.InstalledName = GetFMFromIndex(index).InternalName;
        ret.IndexFromTop = indexFromTop;
        return ret;
    }

    private TDM_ServerFMData GetFMFromIndex(int index) => _serverFMDataList[index];

    private bool CellValueNeededDisabled;

    internal void DGV_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
    {
        if (CellValueNeededDisabled) return;

        if (_serverFMDataList.Count == 0) return;

        TDM_ServerFMData data = _serverFMDataList[e.RowIndex];

        switch ((TDMColumn)e.ColumnIndex)
        {
            /*
            @TDM: Make images for these. Actually maybe just have one column
            I think the game notifies you of the updates in order, like if it's an update then it'll say *
            but then once you update if there's still a lang pack, then it'll say #. I did some testing but
            kind of forgot the exact details. Double-check this.
            */
            case TDMColumn.Update:
                e.Value = data.IsUpdate
                    ? Images.GreenCheckCircle
                    : Images.Blank;
                break;
            case TDMColumn.LanguagePack:
                e.Value = data.HasAvailableLanguagePack
                    ? Images.GreenCheckCircle
                    : Images.Blank;
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

    private void CancelServerFMBasicDataLoad() => _serverFMDataCTS.CancelIfNotDisposed();

    internal async Task LoadData()
    {
        try
        {
            _owner.ShowProgressBox_Single(
                message1: "Loading Dark Mod FMs...",
                progressType: ProgressType.Indeterminate,
                cancelMessage: LText.Global.Cancel,
                cancelAction: CancelServerFMBasicDataLoad
            );

            _serverFMDataCTS = _serverFMDataCTS.Recreate();
            (bool success, bool canceled, _, _serverFMDataList) =
                await TDM_Downloader.TryGetServerDataWithUpdateInfo(_serverFMDataCTS.Token);

            if (success)
            {
                var comparer = Comparers.TDMServerFMTitle;
                comparer.SortDirection = SortDirection.Ascending;

                _serverFMDataList.Sort(comparer);

                try
                {
                    SuppressSelectionEvent = true;
                    this.SuspendDrawing();
                    CellValueNeededDisabled = true;
                    Rows.Clear();
                    RowCount = _serverFMDataList.Count;
                }
                finally
                {
                    CellValueNeededDisabled = false;
                    SuppressSelectionEvent = false;
                    this.ResumeDrawing();
                }
            }
            else
            {
                Rows.Clear();
                if (canceled)
                {
                    // @TDM: implement canceled message
                }
                else
                {
                    // @TDM: Put this on an error label or whatever
                    Trace.WriteLine("Unable to fetch missions list from the server");
                }
            }
        }
        finally
        {
            _owner.HideProgressBox();
        }
    }

}
