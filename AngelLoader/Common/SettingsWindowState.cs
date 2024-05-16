namespace AngelLoader;

public static class SettingsWindowData
{
    public enum SettingsWindowState
    {
        Normal,
        Startup,
        StartupClean,
        BackupPathSet,
    }

    public static bool IsStartup(this SettingsWindowState settingsWindowState) => settingsWindowState is SettingsWindowState.Startup or SettingsWindowState.StartupClean;
}
