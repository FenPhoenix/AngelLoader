using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.Forms;

namespace AngelLoader.CustomControls.Static_LazyLoaded
{
    internal static class AltTitlesLLMenu
    {
        private static bool _constructed;
        private static readonly List<string> _items = new List<string>();

        internal static ContextMenuStrip Menu;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;
            
            Menu = new ContextMenuStrip(components);
            for (int i = 0; i < _items.Count; i++) Menu.Items.Add(_items[i]);
            _items.Clear();
            
            _constructed = true;
        }

        internal static void AddRange(List<ToolStripItem> items)
        {
            if (_constructed)
            {
                Menu.Items.AddRange(items.ToArray());
            }
            else
            {
                for (int i = 0; i < items.Count; i++) _items.Add(items[i].ToString());
            }
        }

        internal static void ClearItems()
        {
            if (_constructed)
            {
                Menu.Items.Clear();
            }
            else
            {
                _items.Clear();
            }
        }
    }
}
