namespace AngelLoader.CustomControls.SettingsPages
{
    internal static class Interfaces
    {
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
