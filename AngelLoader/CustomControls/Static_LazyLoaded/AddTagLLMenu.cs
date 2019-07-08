using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.Forms;

namespace AngelLoader.CustomControls.Static_LazyLoaded
{
    internal static class AddTagLLMenu
    {
        private static bool _constructed;
        internal static ContextMenuStrip Menu;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new ContextMenuStrip(components);
            Menu.Closed += form.AddTagMenu_Closed;

            _constructed = true;
        }
    }
}
