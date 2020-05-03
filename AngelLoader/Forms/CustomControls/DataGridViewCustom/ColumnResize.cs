using System.Reflection;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class DataGridViewCustom
    {
        #region Private fields

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private enum DataGridViewHitTestTypeInternal
        {
            None,
            Cell,
            ColumnHeader,
            RowHeader,
            ColumnResizeLeft,
            ColumnResizeRight,
            RowResizeTop,
            RowResizeBottom,
            FirstColumnHeaderLeft,
            TopLeftHeader,
            TopLeftHeaderResizeLeft,
            TopLeftHeaderResizeRight,
            TopLeftHeaderResizeTop,
            TopLeftHeaderResizeBottom,
            ColumnHeadersResizeBottom,
            ColumnHeadersResizeTop,
            RowHeadersResizeRight,
            RowHeadersResizeLeft,
            ColumnHeaderLeft,
            ColumnHeaderRight
        }

        #region Resize data fields

        private bool ColumnResizeInProgress;
        private int ColumnToResize;
        private int ColumnToResizeOriginalMouseX;
        private int ColumnToResizeOriginalWidth;

        #endregion

        #endregion

        internal void CancelColumnResize()
        {
            if (!ColumnResizeInProgress) return;

            ColumnResizeInProgress = false;
            // Prevents the context menu from popping up if the user right-clicked to cancel. The menu will be
            // set back to what it should be when the user right-clicks while a resize is not progress.
            SetContextMenuToNone();
            Columns[ColumnToResize].Width = ColumnToResizeOriginalWidth;
        }

        #region Private methods

        /// <summary>
        /// Returns true if base.OnMouseDown(e) should be called afterwards, false if it shouldn't.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool StartColumnResize(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && ColumnResizeInProgress)
            {
                CancelColumnResize();
                return false;
            }

            // Manual implementation of real-time column width resizing (the column changes size as you drag)
            // TODO: If you mousedown while a context menu is up, the cursor isn't a size cursor. Fix it for
            // TODO: "the dev thought of everything" points.
            if (e.Button == MouseButtons.Left && Cursor.Current == Cursors.SizeWE)
            {
                var ht = HitTest(e.X, e.Y);
                var typeInternal = ht.GetType().GetField("typeInternal", BindingFlags.NonPublic | BindingFlags.Instance);

                #region Reflection error check and fallback

                // If something has changed internally, fall back to the crappy resize-without-indication default.
                // Always have a fallback in place when using reflection on someone else's classes.

                if (typeInternal == null)
                {
                    ColumnResizeInProgress = false;
                    return true;
                }

                DataGridViewHitTestTypeInternal hitTestType;
                try
                {
                    hitTestType = (DataGridViewHitTestTypeInternal)typeInternal.GetValue(ht);
                }
                catch
                {
                    ColumnResizeInProgress = false;
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

                // When we're dragging a column divider, we always want to resize the column to the left of it.
                // But the hit test will report the right-side column if our mouse is to the right of the divider
                // when we start dragging, so in that case we need to use the column that's one to the left of
                // the reported one.
                // NOTE: I think ColumnResizeLeft means the resizable divider on the left side of the current
                //       column. But if we're thinking of the divider itself, we're on the right side of it.
                //       Just so I don't get confused again if I look at this in a few months.
                ColumnToResize = hitTestType == DataGridViewHitTestTypeInternal.ColumnResizeLeft
                    ? FindColumnIndexByDisplayIndex(Columns[ht.ColumnIndex].DisplayIndex - 1)
                    : ht.ColumnIndex;

                ColumnToResizeOriginalMouseX = e.X;
                ColumnToResizeOriginalWidth = Columns[ColumnToResize].Width;

                ColumnResizeInProgress = true;

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
            if (e.Button == MouseButtons.Left && ColumnResizeInProgress)
            {
                // The move is complete
                ColumnResizeInProgress = false;
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
            if (ColumnResizeInProgress)
            {
                Columns[ColumnToResize].Width = e.X > ColumnToResizeOriginalMouseX
                    ? ColumnToResizeOriginalWidth + (e.X - ColumnToResizeOriginalMouseX)
                    : ColumnToResizeOriginalWidth - (ColumnToResizeOriginalMouseX - e.X);
                return false;
            }

            return true;
        }

        #endregion
    }
}
