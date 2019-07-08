namespace AngelLoader.CustomControls.SettingsPages
{
    internal class Interfaces
    {
        internal interface ISettingsPage
        {
            bool IsVisible { get; set; }
            void SetVScrollPos(int value);
            int GetVScrollPos();
            void ShowPage();
            void HidePage();
        }
    }
}
