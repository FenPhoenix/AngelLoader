namespace Update;

public enum VisualTheme
{
    Classic,
    Dark
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
}
