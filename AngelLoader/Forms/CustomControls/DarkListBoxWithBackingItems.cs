using System.Collections.Generic;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkListBoxWithBackingItems : DarkListBox, IListControlWithBackingItems
    {
        public readonly List<string> BackingItems = new List<string>();

        public void AddFullItem(string backingItem, string item)
        {
            BackingItems.Add(backingItem);
            Items.Add(item);
        }

        public void ClearFullItems()
        {
            BackingItems.Clear();
            Items.Clear();
        }

        public int BackingIndexOf(string item) => BackingItems.IndexOf(item);

        public string SelectedBackingItem() => BackingItems[SelectedIndex];

        public void SelectBackingIndexOf(string item) => SelectedIndex = BackingIndexOf(item);
    }
}
