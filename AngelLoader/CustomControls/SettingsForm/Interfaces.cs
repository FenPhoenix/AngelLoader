namespace AngelLoader.CustomControls.SettingsForm
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
