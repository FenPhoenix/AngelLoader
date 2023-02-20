using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms;

internal static class Interfaces
{
    [PublicAPI]
    internal interface ISettingsPage
    {
        bool Visible { get; }
        void SetVScrollPos(int value);
        int GetVScrollPos();
        void Show();
        void Hide();
        DockStyle Dock { get; set; }
        void Dispose();
    }
}
