using System.Drawing;
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

}
