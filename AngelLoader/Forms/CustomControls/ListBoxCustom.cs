using System.Collections.Generic;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed class ListBoxCustom : ListBox
    {
        #region Backing items

        // For when the backing items should be partial paths but the displayed items should only be filenames

        internal readonly List<string> BackingItems = new List<string>();

        internal void AddFullItem(string backingItem, string item)
        {
            BackingItems.Add(backingItem);
            Items.Add(item);
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
