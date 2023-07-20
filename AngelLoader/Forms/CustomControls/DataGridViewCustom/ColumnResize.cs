using System.Reflection;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed partial class DataGridViewCustom
{
    #region Private fields

    private const int ColumnResizeLeft = 4;

    /*
    private enum DataGridViewHitTestTypeInternal
    {
        None = 0,
        Cell = 1,
        ColumnHeader = 2,
        RowHeader = 3,
        ColumnResizeLeft = 4,
        ColumnResizeRight = 5,
        RowResizeTop = 6,
        RowResizeBottom = 7,
        FirstColumnHeaderLeft = 8,
        TopLeftHeader = 9,
        TopLeftHeaderResizeLeft = 10,
        TopLeftHeaderResizeRight = 11,
        TopLeftHeaderResizeTop = 12,
        TopLeftHeaderResizeBottom = 13,
        ColumnHeadersResizeBottom = 14,
        ColumnHeadersResizeTop = 15,
        RowHeadersResizeRight = 16,
        RowHeadersResizeLeft = 17,
        ColumnHeaderLeft = 18,
        ColumnHeaderRight = 19
    }
    */

    #region Resize data fields

    private bool _columnResizeInProgress;
    private int _columnToResize;
    private int _columnToResizeOriginalMouseX;
    private int _columnToResizeOriginalWidth;

    #endregion

    #endregion

    internal void CancelColumnResize()
    {
        if (!_columnResizeInProgress) return;

        _columnResizeInProgress = false;
        // Prevents the context menu from popping up if the user right-clicked to cancel. The menu will be
        // set back to what it should be when the user right-clicks while a resize is not in progress.
        ContextMenuStrip = null;
        Columns[_columnToResize].Width = _columnToResizeOriginalWidth;
    }

    #region Private methods

    /// <summary>
    /// Returns true if base.OnMouseDown(e) should be called afterwards, false if it shouldn't.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private bool StartColumnResize(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right && _columnResizeInProgress)
        {
            CancelColumnResize();
            return false;
        }

        // Manual implementation of real-time column width resizing (the column changes size as you drag)
        // TODO: If you mousedown while a context menu is up, the cursor isn't a size cursor.
        // Fix it for "the dev thought of everything" points.
        if (e.Button == MouseButtons.Left && Cursor.Current == Cursors.SizeWE)
        {
            HitTestInfo ht = HitTest(e.X, e.Y);
            FieldInfo? typeInternal = typeof(HitTestInfo)
                .GetField(
                    WinFormsReflection.DGV_TypeInternalBackingFieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance);

            #region Reflection error check and fallback

            if (typeInternal == null)
            {
                _columnResizeInProgress = false;
                return true;
            }

            int hitTestType;
            try
            {
                hitTestType = (int)typeInternal.GetValue(ht);
            }
            catch
            {
                _columnResizeInProgress = false;
                return true;
            }

            #endregion

            #region Set column resize to "in progress"

            int FindColumnIndexByDisplayIndex(int displayIndex)
            {
                for (int i = 0; i < Columns.Count; i++)
                {
                    if (Columns[i].DisplayIndex == displayIndex) return Columns[i].Index;
                }

                return -1;
            }

            /*
            When we're dragging a column divider, we always want to resize the column to the left of it.
            But the hit test will report the right-side column if our mouse is to the right of the divider
            when we start dragging, so in that case we need to use the column that's one to the left of the
            the reported one.

            I think ColumnResizeLeft means the resizable divider on the left side of the current column. But
            if we're thinking of the divider itself, we're on the right side of it. Just so I don't get
            confused again if I look at this in a few months.
            */
            _columnToResize = hitTestType == ColumnResizeLeft
                ? FindColumnIndexByDisplayIndex(Columns[ht.ColumnIndex].DisplayIndex - 1)
                : ht.ColumnIndex;

            _columnToResizeOriginalMouseX = e.X;
            _columnToResizeOriginalWidth = Columns[_columnToResize].Width;

            _columnResizeInProgress = true;

            #endregion

            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if base.OnMouseDown(e) should be called afterwards, false if it shouldn't.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private bool EndColumnResize(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && _columnResizeInProgress)
        {
            _columnResizeInProgress = false;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if base.OnMouseDown(e) should be called afterwards, false if it shouldn't.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private bool DoColumnResize(MouseEventArgs e)
    {
        if (_columnResizeInProgress)
        {
            Columns[_columnToResize].Width = e.X > _columnToResizeOriginalMouseX
                ? _columnToResizeOriginalWidth + (e.X - _columnToResizeOriginalMouseX)
                : _columnToResizeOriginalWidth - (_columnToResizeOriginalMouseX - e.X);
            return false;
        }

        return true;
    }

    #endregion
}
