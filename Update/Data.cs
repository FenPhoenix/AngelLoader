using System;

namespace Update;

public enum VisualTheme
{
    Classic,
    Dark,
}

public interface IDarkable
{
    bool DarkModeEnabled { set; }
}

internal static class Data
{
    public static VisualTheme VisualTheme = VisualTheme.Classic;
    public static bool DarkMode => VisualTheme == VisualTheme.Dark;

    internal static readonly LocalizationData LText = new();

    /// <summary>
    /// When the update fails in a way that the current app version is either untouched or has been perfectly
    /// restored from backup.
    /// </summary>
    internal static string GenericUpdateFailedSafeMessage =>
        LText.Update.UpdateFailed + Environment.NewLine +
        LText.Update.RecommendManualUpdate;
}
