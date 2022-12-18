namespace AngelLoader.Forms;

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

    // @NET5: These ones seem to be the same currently (as of .NET 6.0.x)
    internal const string Form_RestoredWindowBounds =
#if NETFRAMEWORK
        "restoredWindowBounds";
#else
            "restoredWindowBounds";
#endif

    internal const string Form_RestoredWindowBoundsSpecified =
#if NETFRAMEWORK
        "restoredWindowBoundsSpecified";
#else
            "restoredWindowBoundsSpecified";
#endif
}