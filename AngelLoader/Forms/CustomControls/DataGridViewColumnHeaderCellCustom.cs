using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed class DataGridViewColumnHeaderCellCustom : DataGridViewColumnHeaderCell
{
    private const BindingFlags _bfAll =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.Static;

    private static bool? _reflectionSupported;
    private static FieldInfo? _selectionModeField;

    /// <inheritdoc cref="DataGridViewColumnHeaderCell.Paint"/>
    protected override void Paint(
        Graphics graphics,
        Rectangle clipBounds,
        Rectangle cellBounds,
        int rowIndex,
        DataGridViewElementStates dataGridViewElementState,
        object? value,
        object? formattedValue,
        string? errorText,
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
        the reflection stuff in dark mode.
        */
        if (rowIndex == -1 && !Global.Config.DarkMode && _reflectionSupported != false)
        {
            // Do this here because DataGridView will still be null in the ctor. We only do it once app-wide
            // anyway (static) so it's fine.
            if (_reflectionSupported == null)
            {
                try
                {
                    _selectionModeField = typeof(DataGridView).GetField(WinFormsReflection.DGV_SelectionModeBackingFieldName, _bfAll);
                    if (_selectionModeField == null ||
                        _selectionModeField.GetValue(DataGridView) is not DataGridViewSelectionMode)
                    {
                        CallBase(disableReflection: true);
                        return;
                    }
                    else
                    {
                        _reflectionSupported = true;
                    }
                }
                catch
                {
                    CallBase(disableReflection: true);
                    return;
                }
            }

            if (_selectionModeField == null)
            {
                CallBase(disableReflection: true);
                return;
            }

            DataGridViewSelectionMode oldSelectionMode;
            DataGridViewSelectionMode oldSelectionModeProperty = DataGridView.SelectionMode;
            try
            {
                oldSelectionMode = (DataGridViewSelectionMode)_selectionModeField.GetValue(DataGridView);
                _selectionModeField.SetValue(DataGridView, DataGridViewSelectionMode.CellSelect);
            }
            catch
            {
                // Force correct selection mode back in this case, because our temp set could have failed
                DataGridView.SelectionMode = oldSelectionModeProperty;
                CallBase(disableReflection: true);
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
                    _selectionModeField.SetValue(DataGridView, oldSelectionMode);
                }
                catch
                {
                    // Ditto
                    DataGridView.SelectionMode = oldSelectionModeProperty;
                    _reflectionSupported = false;
                }
            }
        }
        else
        {
            CallBase();
        }

        return;

        void CallBase(bool disableReflection = false)
        {
            if (disableReflection) _reflectionSupported = false;
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
