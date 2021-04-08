using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DataGridViewColumnHeaderCellCustom : DataGridViewColumnHeaderCell
    {
        private const BindingFlags _bfAll =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static;

        private static bool? _reflectionSupported;
        private static FieldInfo? selectionModeField;

        // Not strictly necessary: we work fine without it, but having it allows us to skip the reflection stuff
        // in dark mode.
        internal bool DarkModeEnabled;

        /// <summary>Paints the current <see cref="T:System.Windows.Forms.DataGridViewColumnHeaderCell" />.</summary>
        /// <param name="graphics">The <see cref="T:System.Drawing.Graphics" /> used to paint the cell.</param>
        /// <param name="clipBounds">A <see cref="T:System.Drawing.Rectangle" /> that represents the area of the <see cref="T:System.Windows.Forms.DataGridView" /> that needs to be repainted.</param>
        /// <param name="cellBounds">A <see cref="T:System.Drawing.Rectangle" /> that contains the bounds of the cell that is being painted.</param>
        /// <param name="rowIndex">The row index of the cell that is being painted.</param>
        /// <param name="dataGridViewElementState">A bitwise combination of <see cref="T:System.Windows.Forms.DataGridViewElementStates" /> values that specifies the state of the cell.</param>
        /// <param name="value">The data of the cell that is being painted.</param>
        /// <param name="formattedValue">The formatted data of the cell that is being painted.</param>
        /// <param name="errorText">An error message that is associated with the cell.</param>
        /// <param name="cellStyle">A <see cref="T:System.Windows.Forms.DataGridViewCellStyle" /> that contains formatting and style information about the cell.</param>
        /// <param name="advancedBorderStyle">A <see cref="T:System.Windows.Forms.DataGridViewAdvancedBorderStyle" /> that contains border styles for the cell that is being painted.</param>
        /// <param name="paintParts">A bitwise combination of the <see cref="T:System.Windows.Forms.DataGridViewPaintParts" /> values that specifies which parts of the cell need to be painted.</param>
        protected override void Paint(
            Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates dataGridViewElementState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            // Hack to force selected column headers NOT to be highlighted. We're in full-row select mode, so
            // highlighting makes no sense and is visually distracting. The DataGridView code is LUDICROUSLY
            // resilient against any attempt to make it stop the highlighting. It has an IsHighlighted() method
            // that it checks, which checks a bunch of properties (no fields directly!) and returns true in our
            // case and thus highlights the column header. One of the things it checks is "Accessibility
            // improvements level 2". We could turn those off with an app config XML property or whatever (can't
            // do it at runtime, of course!), but then that would turn off a bunch of OTHER accessibility stuff
            // that we want to leave on because we don't want to screw people over who may need them just so we
            // can turn off an irrelevant thing that's nothing to do with accessibility (it would be if we WEREN'T
            // full-row select, but as I just said...!)
            // So, instead, we just toggle the underlying field of one of the properties it checks (SelectionMode)
            // while we paint. Thus we force the accursed thing to not paint itself blue. FINALLY.
            // NOTE: In dark mode we don't have to do this because we're already custom drawing the headers then.
            if (rowIndex == -1 && !DarkModeEnabled)
            {
                // Do this here because DataGridView will still be null in the ctor. We only do it once app-wide
                // anyway (static) so it's fine.
                if (_reflectionSupported == null)
                {
                    try
                    {
                        selectionModeField = typeof(DataGridView).GetField("selectionMode", _bfAll);
                        if (selectionModeField == null ||
                            selectionModeField.GetValue(DataGridView) is not DataGridViewSelectionMode)
                        {
                            _reflectionSupported = false;
                            return;
                        }

                        _reflectionSupported = true;
                    }
                    catch
                    {
                        _reflectionSupported = false;
                    }
                }

                DataGridViewSelectionMode? oldSelectionMode = null;
                bool success = true;
                try
                {
                    oldSelectionMode = (DataGridViewSelectionMode)selectionModeField!.GetValue(DataGridView);
                    selectionModeField.SetValue(DataGridView, DataGridViewSelectionMode.CellSelect);
                }
                catch
                {
                    success = false;
                }

                try
                {
                    base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
                }
                finally
                {
                    if (success && oldSelectionMode != null)
                    {
                        try
                        {
                            selectionModeField!.SetValue(DataGridView, oldSelectionMode);
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
            }
            else
            {
                base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
            }
        }
    }
}
