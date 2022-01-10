namespace AngelLoader.Forms
{
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
    }
}
