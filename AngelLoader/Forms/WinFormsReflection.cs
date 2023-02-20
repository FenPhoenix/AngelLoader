namespace AngelLoader.Forms;

// @NET5(WinFormsReflection): Make sure these still work with whatever .NET version we're currently using
internal static class WinFormsReflection
{
    internal const string DGV_SelectionModeBackingFieldName =
#if NETFRAMEWORK
        "selectionMode";
#else
        "_selectionMode";
#endif

    internal const string DGV_TypeInternalBackingFieldName =
#if NETFRAMEWORK
        "typeInternal";
#else
        "_typeInternal";
#endif

    internal const string ToolTipNativeWindow_ToolTipFieldName =
#if NETFRAMEWORK
        "control";
#else
        "_toolTip";
#endif

    internal const string Form_RestoredWindowBounds =
#if NETFRAMEWORK
        "restoredWindowBounds";
#elif NET7_0_OR_GREATER
        "_restoredWindowBounds";
#else
        "restoredWindowBounds";
#endif

    internal const string Form_RestoredWindowBoundsSpecified =
#if NETFRAMEWORK
        "restoredWindowBoundsSpecified";
#elif NET7_0_OR_GREATER
        "_restoredWindowBoundsSpecified";
#else
        "restoredWindowBoundsSpecified";
#endif
}
