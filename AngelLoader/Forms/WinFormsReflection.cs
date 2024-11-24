namespace AngelLoader.Forms;

// @NET5(WinFormsReflection): Make sure these still work with whatever .NET version we're currently using
// Tested working for .NET 9
internal static class WinFormsReflection
{
    internal const string DGV_SelectionModeBackingFieldName =
        "_selectionMode";

    internal const string DGV_TypeInternalBackingFieldName =
        "_typeInternal";

    internal const string ToolTipNativeWindow_ToolTipFieldName =
        "_toolTip";

    internal const string Form_RestoredWindowBounds =
        "_restoredWindowBounds";

    internal const string Form_RestoredWindowBoundsSpecified =
        "_restoredWindowBoundsSpecified";
}
