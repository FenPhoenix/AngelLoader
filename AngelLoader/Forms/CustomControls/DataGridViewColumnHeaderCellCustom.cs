using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed class DataGridViewColumnHeaderCellCustom : DataGridViewColumnHeaderCell
{
    private static bool? _hackSupported;

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = WinFormsReflection.DGV_SelectionModeBackingFieldName)]
    private static extern ref DataGridViewSelectionMode GetSelectionMode(DataGridView dgv);

    /// <inheritdoc cref="DataGridViewColumnHeaderCell.Paint"/>
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
        if (DataGridView == null) return;

        /*
        Hack to force selected column headers NOT to be highlighted. We're in full-row select mode, so
        highlighting makes no sense and is visually distracting. The DataGridView code is LUDICROUSLY
        resilient against any attempt to make it stop the highlighting. It has an IsHighlighted() method
        that it checks, which checks a bunch of properties (no fields directly!) and returns true in our
        case and thus highlights the column header. One of the things it checks is "Accessibility
        improvements level 2". We could turn those off with an app config XML property or whatever (can't
        do it at runtime, of course!*), but then that would turn off a bunch of OTHER accessibility stuff
        that we want to leave on because we don't want to screw people over who may need them just so we
        can turn off an irrelevant thing that's nothing to do with accessibility (it would be if we WEREN'T
        full-row select, but as I just said...!)
        So, instead, we just toggle the underlying field of one of the properties it checks (SelectionMode)
        while we paint. Thus we force the accursed thing to not paint itself blue. FINALLY.
        In dark mode we don't have to do this because we're already custom drawing the headers then.

        *You can in fact set the accessibility level switches at runtime, but DataGridView caches the switch
        values internally, so setting the global ones does nothing in this case, it's just going to read
        from its own cache.

        The dark mode check is not strictly necessary: we work fine without it, but having it allows us to skip
        the private field access stuff in dark mode.
        */
        if (rowIndex == -1 && !Global.Config.DarkMode && _hackSupported != false)
        {
            // Do this here because DataGridView will still be null in the ctor. We only do it once app-wide
            // anyway (static) so it's fine.
            try
            {
                ref DataGridViewSelectionMode selectionMode = ref GetSelectionMode(DataGridView);

                DataGridViewSelectionMode oldSelectionMode;
                DataGridViewSelectionMode oldSelectionModeProperty = DataGridView.SelectionMode;
                try
                {
                    oldSelectionMode = selectionMode;
                    selectionMode = DataGridViewSelectionMode.CellSelect;
                }
                catch
                {
                    // Force correct selection mode back in this case, because our temp set could have failed
                    DataGridView.SelectionMode = oldSelectionModeProperty;
                    CallBase(disableHack: true);
                    return;
                }

                try
                {
                    CallBase();
                }
                finally
                {
                    try
                    {
                        selectionMode = oldSelectionMode;
                    }
                    catch
                    {
                        // Ditto
                        DataGridView.SelectionMode = oldSelectionModeProperty;
                        _hackSupported = false;
                    }
                }
            }
            catch
            {
                CallBase(disableHack: true);
                return;
            }
        }
        else
        {
            CallBase();
        }

        return;

        void CallBase(bool disableHack = false)
        {
            if (disableHack) _hackSupported = false;
            base.Paint(
                graphics,
                clipBounds,
                cellBounds,
                rowIndex,
                dataGridViewElementState,
                value,
                formattedValue,
                errorText,
                cellStyle,
                advancedBorderStyle,
                paintParts);
        }
    }
}
