using System.ComponentModel;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class AddTagLLMenu
    {
        private static bool _constructed;

        internal static ContextMenuStrip Menu = null!;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            // @NET5: Force MS Sans Serif
            Menu = new ContextMenuStrip(components) { Font = ControlExtensions.LegacyMSSansSerif() };
            Menu.Closed += form.AddTagMenu_Closed;

            _constructed = true;
        }
    }
}
