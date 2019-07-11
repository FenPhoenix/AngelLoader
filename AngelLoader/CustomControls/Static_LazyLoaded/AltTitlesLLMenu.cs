using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace AngelLoader.CustomControls.Static_LazyLoaded
{
    internal static class AltTitlesLLMenu
    {
        internal static bool Constructed { get; private set; }

        internal static ContextMenuStrip Menu;

        internal static void Construct(IContainer components)
        {
            if (Constructed) return;

            Menu = new ContextMenuStrip(components);

            Constructed = true;
        }

        internal static void AddRange(List<ToolStripItem> items)
        {
            if (!Constructed) return;
            Menu.Items.AddRange(items.ToArray());
        }

        internal static void ClearItems()
        {
            if (!Constructed) return;
            Menu.Items.Clear();
        }
    }
}
