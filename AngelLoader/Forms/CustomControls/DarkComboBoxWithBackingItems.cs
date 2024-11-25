using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkComboBoxWithBackingItems : DarkComboBox, IListControlWithBackingItems
{
    // Cache visible state because calling Visible redoes the work even if the value is the same
    private bool _visibleCached = true;

    [PublicAPI]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool Visible
    {
        get => base.Visible;
        set
        {
            if (value == _visibleCached) return;
            _visibleCached = value;
            base.Visible = value;
        }
    }

    [PublicAPI]
    public new void Show()
    {
        if (_visibleCached) return;
        _visibleCached = true;
        base.Show();
    }

    [PublicAPI]
    public new void Hide()
    {
        if (!_visibleCached) return;
        _visibleCached = false;
        base.Hide();
    }

    public readonly List<string> BackingItems = new();

    public void AddFullItem(string backingItem, string item)
    {
        BackingItems.Add(backingItem);
        Items.Add(item);
    }

    public void ClearFullItems()
    {
        if (Items.Count > 0 && BackingItems.Count > 0)
        {
            BackingItems.Clear();
            Items.Clear();
        }
    }

    public void ClearAllBeyondFirstItem()
    {
        if (Items.Count > 1 && BackingItems.Count > 1)
        {
            for (int i = Items.Count - 1; i >= 1; i--)
            {
                Items.RemoveAt(i);
                BackingItems.RemoveAt(i);
            }
        }
    }

    public int BackingIndexOf(string item) => BackingItems.IndexOf(item);

    public string SelectedBackingItem() => BackingItems[SelectedIndex];

    public void SelectBackingIndexOf(string item) => SelectedIndex = BackingIndexOf(item);
}
