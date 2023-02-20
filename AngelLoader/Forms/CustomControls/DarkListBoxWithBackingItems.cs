using System.Collections.Generic;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkListBoxWithBackingItems : DarkListBox, IListControlWithBackingItems
{
    public readonly List<string> BackingItems = new();

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

    public string SelectedBackingItem() => BackingItems[SelectedIndex];

    #region Disabled until needed

#if false
    public int BackingIndexOf(string item) => BackingItems.IndexOf(item);

    public void SelectBackingIndexOf(string item) => SelectedIndex = BackingIndexOf(item);
#endif

    #endregion
}
