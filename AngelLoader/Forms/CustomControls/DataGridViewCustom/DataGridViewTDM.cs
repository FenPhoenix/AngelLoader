using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.DataClasses;

namespace AngelLoader.Forms.CustomControls;

public sealed class DataGridViewTDM : DataGridViewCustomBase
{
    protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
    {
        base.OnCellPainting(e);

        if (!_darkModeEnabled) return;

        if (e.RowIndex == -1)
        {
            DrawColumnHeaders(e);
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
}
