using System.Collections.Generic;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkComboBoxWithBackingItems : DarkComboBox
    {
        #region Backing items

        // For when the backing items should be different from the displayed items
        // (ex. if backing items are full paths but displayed items should only be filenames)

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

        [PublicAPI]
        internal int BackingIndexOf(string item) => BackingItems.IndexOf(item);

        internal string SelectedBackingItem() => BackingItems[SelectedIndex];

        internal void SelectBackingIndexOf(string item) => SelectedIndex = BackingIndexOf(item);

        #endregion
    }
}
