using System.Collections.Generic;
using System.Windows.Forms;
using AngelLoader.Common.Utility;

namespace AngelLoader.CustomControls
{
    internal class ListBoxCustom : ListBox
    {
        #region Backing items

        // For when the backing items should be partial paths but the displayed items should only be filenames

        internal readonly List<string> BackingItems = new List<string>();

        internal void AddFullItem(string item)
        {
            BackingItems.Add(item);
            Items.Add(item.Contains('\\') ? item.Substring(item.LastIndexOf('\\') + 1) : item);
        }

        internal void AddRangeFull(List<string> items)
        {
            for (int i = 0; i < items.Count; i++) AddFullItem(items[i]);
        }

        internal void ClearFullItems()
        {
            BackingItems.Clear();
            Items.Clear();
        }

        internal string SelectedBackingItem() => BackingItems[SelectedIndex];

        #endregion
    }
}
