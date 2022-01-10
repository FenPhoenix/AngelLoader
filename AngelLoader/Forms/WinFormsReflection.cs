namespace AngelLoader.Forms
{
    internal static class WinFormsReflection
    {
        internal const string DGV_SelectionModeBackingFieldName =
#if NETFRAMEWORK
            "selectionMode";
#else
            "_selectionMode"
#endif

        internal const string DGV_TypeInternalBackingFieldName =
#if NETFRAMEWORK
            "typeInternal";
#else
            "_typeInternal"
#endif
    }
}
