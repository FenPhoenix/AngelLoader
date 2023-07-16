using System;
using System.Collections;
using System.Windows.Forms;

namespace FMInfoGen;

internal static class Comparers
{
    internal sealed class ItemComparer : IComparer
    {
        internal int Column { get; set; }
        internal SortOrder Order { get; set; }

        internal ItemComparer(int colIndex)
        {
            Column = colIndex;
            Order = SortOrder.None;
        }

        public int Compare(object? x, object? y)
        {
            var itemA = x as ListViewItem;
            var itemB = y as ListViewItem;

            if (itemA == itemB) return 0;
            if (itemA == null) return -1;
            if (itemB == null) return 1;

            var accuracySort = string.CompareOrdinal(
                (string)itemA.SubItems[Column].Tag,
                (string)itemB.SubItems[Column].Tag);

            // Sort by accuracy, then by name
            int result = accuracySort != 0
                ? accuracySort
                : string.Compare(
                    itemA.SubItems[Column].Text,
                    itemB.SubItems[Column].Text, StringComparison.OrdinalIgnoreCase);

            if (Order == SortOrder.Descending) result *= -1;

            return result;
        }
    }
}
