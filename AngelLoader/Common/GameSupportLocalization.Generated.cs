#define FenGen_LocalizedGameNameGetterDest

using static AngelLoader.FenGenAttributes;

namespace AngelLoader
{
    [FenGenLocalizedGameNameGetterDestClass]
    public static partial class GameSupport
    {
        #region Autogenerated per-game localized string getters

        internal static string GetLocalizedGameName(GameIndex gameIndex)
        {
            return
                gameIndex == GameIndex.Thief1 ? Misc.LText.Global.Thief1:
                gameIndex == GameIndex.Thief2 ? Misc.LText.Global.Thief2:
                gameIndex == GameIndex.Thief3 ? Misc.LText.Global.Thief3:
                Misc.LText.Global.SystemShock2;
        }

        internal static string GetShortLocalizedGameName(GameIndex gameIndex)
        {
            return
                gameIndex == GameIndex.Thief1 ? Misc.LText.Global.Thief1_Short:
                gameIndex == GameIndex.Thief2 ? Misc.LText.Global.Thief2_Short:
                gameIndex == GameIndex.Thief3 ? Misc.LText.Global.Thief3_Short:
                Misc.LText.Global.SystemShock2_Short;
        }

        internal static string GetLocalizedGameNameColon(GameIndex gameIndex)
        {
            return
                gameIndex == GameIndex.Thief1 ? Misc.LText.Global.Thief1_Colon:
                gameIndex == GameIndex.Thief2 ? Misc.LText.Global.Thief2_Colon:
                gameIndex == GameIndex.Thief3 ? Misc.LText.Global.Thief3_Colon:
                Misc.LText.Global.SystemShock2_Colon;
        }

        internal static string GetLocalizedGamePlayOriginalText(GameIndex gameIndex)
        {
            return
                gameIndex == GameIndex.Thief1 ? Misc.LText.PlayOriginalGameMenu.Thief1_PlayOriginal:
                gameIndex == GameIndex.Thief2 ? Misc.LText.PlayOriginalGameMenu.Thief2_PlayOriginal:
                gameIndex == GameIndex.Thief3 ? Misc.LText.PlayOriginalGameMenu.Thief3_PlayOriginal:
                Misc.LText.PlayOriginalGameMenu.SystemShock2_PlayOriginal;
        }

        internal static string GetLocalizedOriginalModHeaderText(GameIndex gameIndex)
        {
            return
                gameIndex == GameIndex.Thief1 ? Misc.LText.PlayOriginalGameMenu.Mods_EnableOrDisableModsForThief1:
                gameIndex == GameIndex.Thief2 ? Misc.LText.PlayOriginalGameMenu.Mods_EnableOrDisableModsForThief2:
                gameIndex == GameIndex.Thief3 ? Misc.LText.PlayOriginalGameMenu.Mods_EnableOrDisableModsForThief3:
                Misc.LText.PlayOriginalGameMenu.Mods_EnableOrDisableModsForSS2;
        }

        #endregion
    }
}
