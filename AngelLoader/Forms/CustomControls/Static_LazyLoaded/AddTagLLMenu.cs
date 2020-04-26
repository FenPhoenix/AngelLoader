using System.ComponentModel;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class AddTagLLMenu
    {
        private static bool _constructed;

        private static ContextMenuStrip? _menu;
        internal static ContextMenuStrip Menu
        {
            get => _menu!;
            private set => _menu = value;
        }

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new ContextMenuStrip(components);
            Menu.Closed += form.AddTagMenu_Closed;

            _constructed = true;
        }
    }
}
