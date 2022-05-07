using System.Collections.Generic;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkComboBoxWithBackingItems : DarkComboBox, IListControlWithBackingItems
    {
        // Cache visible state because calling Visible redoes the work even if the value is the same
        private bool _visibleCached = true;

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

        public new void Show()
        {
            if (_visibleCached) return;
            _visibleCached = true;
            base.Show();
        }

        public new void Hide()
        {
            if (!_visibleCached) return;
            _visibleCached = false;
            base.Hide();
        }

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
