namespace AngelLoader.CustomControls.SettingsForm
{
    internal class Interfaces
    {
        internal interface ISettingsPage
        {
            void SetVScrollPos(int value);
            int GetVScrollPos();
            void ShowPage();
            void HidePage();
        }
    }
}
