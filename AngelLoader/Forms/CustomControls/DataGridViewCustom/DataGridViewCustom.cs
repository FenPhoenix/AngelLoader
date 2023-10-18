using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls.LazyLoaded;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader.Forms.CustomControls;

public sealed class DataGridViewCustom : DataGridViewCustomBase
{
    #region Public fields

    internal readonly SelectedFM CurrentSelFM = new();

    internal readonly GameTabsState GameTabsState = new();

    #region Filter

    internal readonly Filter Filter = new();
    internal readonly List<int> FilterShownIndexList = new();

    #endregion

    #endregion

    #region Public methods

    #region Get FM / FM data

    /// <summary>
    /// Gets the FM at <paramref name="index"/>, taking the currently set filters into account.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    internal FanMission GetFMFromIndex(int index) => FMsViewList[FilterShownIndexList[index]];

    /// <summary>
    /// Gets the currently selected FM, taking the currently set filters into account.
    /// </summary>
    /// <returns></returns>
    internal FanMission GetMainSelectedFM()
    {
        AssertR(GetRowSelectedCount() > 0, nameof(GetMainSelectedFM) + ": no rows selected!");
        AssertR(MainSelectedRow != null, nameof(MainSelectedRow) + " is null when it shouldn't be");

        return GetFMFromIndex(MainSelectedRow!.Index);
    }

    /// <summary>
    /// Order is not guaranteed. Seems to be in reverse order currently but who knows. Use <see cref="GetSelectedFMs_InOrder"/>
    /// if you need them in visual order.
    /// </summary>
    /// <returns></returns>
    internal FanMission[] GetSelectedFMs()
    {
        var selRows = SelectedRows;
        var ret = new FanMission[selRows.Count];
        for (int i = 0; i < selRows.Count; i++)
        {
            ret[i] = GetFMFromIndex(selRows[i].Index);
        }

        return ret;
    }

    private DataGridViewRow[] GetOrderedRowsArray()
    {
        // Why. Why would Microsoft have all these infuriatingly stupid custom list types.
        // DataGridViewSelectedRowCollection?! Not even a row collection, but a _SELECTED_ row collection.
        // Jesus christ is that really necessary? And it has no sort methods either because it's custom, so
        // it's copy with Cast and then copy with OrderBy and then copy to Array and then copy that to another
        // array of actual FM objects. Hooray. We've got a garbage collector so who cares right?
        return SelectedRows.Cast<DataGridViewRow>().OrderBy(static x => x.Index).ToArray();
    }

    /// <summary>
    /// Use this if you need the FMs in visual order, but take a (probably minor-ish) perf/mem hit.
    /// </summary>
    /// <returns></returns>
    internal FanMission[] GetSelectedFMs_InOrder()
    {
        var selRows = GetOrderedRowsArray();

        var ret = new FanMission[selRows.Length];
        for (int i = 0; i < selRows.Length; i++)
        {
            ret[i] = GetFMFromIndex(selRows[i].Index);
        }

        return ret;
    }

    /// <summary>
    /// Use this if you need the FMs in visual order, but take a (probably minor-ish) perf/mem hit.
    /// </summary>
    /// <returns></returns>
    internal List<FanMission> GetSelectedFMs_InOrder_List()
    {
        DataGridViewRow[] selRows = GetOrderedRowsArray();

        var ret = new List<FanMission>(selRows.Length);
        for (int i = 0; i < selRows.Length; i++)
        {
            ret.Add(GetFMFromIndex(selRows[i].Index));
        }

        return ret;
    }

    internal int GetIndexFromInstalledName(string installedName, bool findNearest)
    {
        if (installedName.IsEmpty()) return 0;

        for (int i = 0; i < FilterShownIndexList.Count; i++)
        {
            if (GetFMFromIndex(i).InstalledDir.EqualsI(installedName)) return i;
        }

        if (findNearest)
        {
            for (int i = 0; i < FMsViewList.Count; i++)
            {
                if (FMsViewList[i].InstalledDir.EqualsI(installedName))
                {
                    for (int j = i; j < FMsViewList.Count; j++)
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

        ret.InstalledName = GetFMFromIndex(index).InstalledDir;
        ret.IndexFromTop = indexFromTop;
        return ret;
    }

    #endregion

    #region Get and set columns

    internal ColumnData[] GetColumnData()
    {
        var columns = new ColumnData[Columns.Count];

        for (int i = 0; i < Columns.Count; i++)
        {
            DataGridViewColumn col = Columns[i];
            columns[i] = new ColumnData
            {
                Id = (Column)col.Index,
                DisplayIndex = col.DisplayIndex,
                Visible = col.Visible,
                Width = col.Width
            };
        }

        return columns.OrderBy(static x => x.Id).ToArray();
    }

    internal void SetColumnData(FMsDGV_ColumnHeaderLLMenu menu, ColumnData[] columnData)
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

        ColumnData[] columnDataSorted = columnData.OrderBy(static x => x.DisplayIndex).ToArray();

        #endregion

        foreach (ColumnData colData in columnDataSorted)
        {
            DataGridViewColumn col = Columns[(int)colData.Id];

            col.DisplayIndex = colData.DisplayIndex;
            if (col.Resizable == DataGridViewTriState.True) col.Width = colData.Width;
            MakeColumnVisible(col, colData.Visible);

            menu.SetColumnChecked((int)colData.Id, colData.Visible);
        }
    }

    #endregion

    #endregion

    #region Event overrides

    #region Paint

    protected override void OnRowPrePaint(DataGridViewRowPrePaintEventArgs e)
    {
        base.OnRowPrePaint(e);

        if (_darkModeEnabled) return;

        // Coloring the recent rows here because if we do it in _CellValueNeeded, we get a brief flash of the
        // default white-background cell color before it changes.
        if (!_owner.CellValueNeededDisabled && FilterShownIndexList.Count > 0)
        {
            FanMission fm = GetFMFromIndex(e.RowIndex);

            Rows[e.RowIndex].DefaultCellStyle.BackColor =
                fm.MarkedUnavailable
                    ? DarkColors.DGV_UnavailableColorLight
                    : fm.Pinned
                        ? DarkColors.DGV_PinnedBackgroundLight
                        : fm.MarkedRecent
                            ? DarkColors.DGV_RecentHighlightColorLight
                            : DefaultRowBackColor;
        }
    }

    protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
    {
        base.OnCellPainting(e);

        if (!_darkModeEnabled) return;

        // This is for having different colored grid lines in recent-highlighted rows.
        // That way, we can get a good, visible separator color for all cases by just having two.

        if (e.RowIndex == -1)
        {
            DrawColumnHeaders(e);
        }
        else if (e.RowIndex > -1)
        {
            FanMission fm = GetFMFromIndex(e.RowIndex);

            bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;

            SolidBrush bgBrush = isSelected
                ? DarkColors.BlueSelectionBrush
                : fm.MarkedUnavailable
                    ? DarkColors.Fen_RedHighlightBrush
                    : fm.Pinned
                        ? DarkColors.DGV_PinnedBackgroundDarkBrush
                        : fm.MarkedRecent
                            ? DarkColors.Fen_DGVCellBordersBrush
                            : DarkColors.Fen_DarkBackgroundBrush;

            bool needsDarkBorder = fm.MarkedUnavailable || fm.MarkedRecent;

            Pen borderPen = needsDarkBorder
                ? DarkColors.Fen_DarkBackgroundPen
                : DarkColors.Fen_DGVCellBordersPen;

            FanMission nextFM;
            Pen bottomBorderPen =
                needsDarkBorder &&
                e.RowIndex < RowCount - 1 &&
                !(nextFM = GetFMFromIndex(e.RowIndex + 1)).MarkedUnavailable &&
                !nextFM.MarkedRecent
                    ? DarkColors.Fen_DGVCellBordersPen
                    : borderPen;

            DrawRows(e, isSelected, bgBrush, borderPen, bottomBorderPen);
        }
    }

    #endregion

    #endregion
}
