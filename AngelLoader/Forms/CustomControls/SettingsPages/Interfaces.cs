using JetBrains.Annotations;

namespace AngelLoader.CustomControls.SettingsPages
{
    internal static class Interfaces
    {
        [PublicAPI]
        internal interface ISettingsPage
        {
            bool IsVisible { get; }
            void SetVScrollPos(int value);
            int GetVScrollPos();
            void ShowPage();
            void HidePage();
        }
    }
}
