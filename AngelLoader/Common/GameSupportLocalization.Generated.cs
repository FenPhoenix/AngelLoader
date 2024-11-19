#define FenGen_LocalizedGameNameGetterDest

using static AL_Common.FenGenAttributes;
using static AngelLoader.Global;

namespace AngelLoader;

[FenGenLocalizedGameNameGetterDestClass]
public static partial class GameSupport
{
    #region Autogenerated per-game localized string getters

    internal static string GetLocalizedGameName(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief1 => LText.Global.Thief1,
        GameIndex.Thief2 => LText.Global.Thief2,
        GameIndex.Thief3 => LText.Global.Thief3,
        GameIndex.SS2 => LText.Global.SystemShock2,
        _ => LText.Global.TheDarkMod,
    };

    internal static string GetShortLocalizedGameName(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief1 => LText.Global.Thief1_Short,
        GameIndex.Thief2 => LText.Global.Thief2_Short,
        GameIndex.Thief3 => LText.Global.Thief3_Short,
        GameIndex.SS2 => LText.Global.SystemShock2_Short,
        _ => LText.Global.TheDarkMod_Short,
    };

    internal static string GetLocalizedGameNameColon(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief1 => LText.Global.Thief1_Colon,
        GameIndex.Thief2 => LText.Global.Thief2_Colon,
        GameIndex.Thief3 => LText.Global.Thief3_Colon,
        GameIndex.SS2 => LText.Global.SystemShock2_Colon,
        _ => LText.Global.TheDarkMod_Colon,
    };

    internal static string GetLocalizedCustomResourcesNotSupportedMessage(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief1 => LText.StatisticsTab.CustomResourcesNotSupportedForThief1,
        GameIndex.Thief2 => LText.StatisticsTab.CustomResourcesNotSupportedForThief2,
        GameIndex.Thief3 => LText.StatisticsTab.CustomResourcesNotSupportedForThief3,
        GameIndex.SS2 => LText.StatisticsTab.CustomResourcesNotSupportedForSS2,
        _ => LText.StatisticsTab.CustomResourcesNotSupportedForTDM,
    };

    internal static string GetLocalizedModsNotSupportedMessage(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief1 => LText.ModsTab.Thief1_ModsNotSupported,
        GameIndex.Thief2 => LText.ModsTab.Thief2_ModsNotSupported,
        GameIndex.Thief3 => LText.ModsTab.Thief3_ModsNotSupported,
        GameIndex.SS2 => LText.ModsTab.SS2_ModsNotSupported,
        _ => LText.ModsTab.TDM_ModsNotSupported,
    };

    internal static string GetLocalizedGamePlayOriginalText(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief1 => LText.PlayOriginalGameMenu.Thief1_PlayOriginal,
        GameIndex.Thief2 => LText.PlayOriginalGameMenu.Thief2_PlayOriginal,
        GameIndex.Thief3 => LText.PlayOriginalGameMenu.Thief3_PlayOriginal,
        GameIndex.SS2 => LText.PlayOriginalGameMenu.SystemShock2_PlayOriginal,
        _ => LText.PlayOriginalGameMenu.TheDarkMod_PlayOriginal,
    };

    internal static string GetLocalizedGameSettingsNotSupportedMessage(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief1 => LText.PlayOriginalGameMenu.Mods_Thief1NotSupported,
        GameIndex.Thief2 => LText.PlayOriginalGameMenu.Mods_Thief2NotSupported,
        GameIndex.Thief3 => LText.PlayOriginalGameMenu.Mods_Thief3NotSupported,
        GameIndex.SS2 => LText.PlayOriginalGameMenu.Mods_SS2NotSupported,
        _ => LText.PlayOriginalGameMenu.Mods_TDMNotSupported,
    };

    internal static string GetLocalizedOriginalModHeaderText(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief1 => LText.PlayOriginalGameMenu.Mods_EnableOrDisableModsForThief1,
        GameIndex.Thief2 => LText.PlayOriginalGameMenu.Mods_EnableOrDisableModsForThief2,
        GameIndex.Thief3 => LText.PlayOriginalGameMenu.Mods_EnableOrDisableModsForThief3,
        GameIndex.SS2 => LText.PlayOriginalGameMenu.Mods_EnableOrDisableModsForSS2,
        _ => LText.PlayOriginalGameMenu.Mods_EnableOrDisableModsForTDM,
    };

    internal static string GetLocalizedSelectGameExecutableMessage(GameIndex gameIndex) => gameIndex switch
    {
        GameIndex.Thief1 => LText.SettingsWindow.Paths_DialogTitle_SelectT1Exe,
        GameIndex.Thief2 => LText.SettingsWindow.Paths_DialogTitle_SelectT2Exe,
        GameIndex.Thief3 => LText.SettingsWindow.Paths_DialogTitle_SelectT3Exe,
        GameIndex.SS2 => LText.SettingsWindow.Paths_DialogTitle_SelectSS2Exe,
        _ => LText.SettingsWindow.Paths_DialogTitle_SelectTDMExe,
    };

    #endregion
}
