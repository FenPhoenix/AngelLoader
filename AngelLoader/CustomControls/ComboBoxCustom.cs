﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.CustomControls
{
    public class ComboBoxCustom : ComboBox
    {
        private const uint WM_CTLCOLORLISTBOX = 308;
        private const int SWP_NOSIZE = 1;

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

        #region Dropdown features

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        protected override void WndProc(ref Message m)
        {
            // If the dropdown is going to go off the right side of the screen, try to reposition it so it always
            // appears fully on-screen
            if (m.Msg == WM_CTLCOLORLISTBOX)
            {
                var p = PointToScreen(new Point(0, Height));

                var screenWidth = Screen.FromControl(this).Bounds.Width;
                bool alignRight = p.X + DropDownWidth > screenWidth;

                int x = alignRight ? p.X - (DropDownWidth - Math.Min(Width, screenWidth - p.X)) : p.X;
                SetWindowPos(m.LParam, IntPtr.Zero, x, p.Y, 0, 0, SWP_NOSIZE);
            }

            base.WndProc(ref m);
        }

        protected override void OnDropDown(EventArgs e)
        {
            // Autosize dropdown to accomodate the longest item
            int finalWidth = 0;
            foreach (var item in Items)
            {
                if (!(item is string itemStr)) continue;

                int currentItemWidth = TextRenderer.MeasureText(itemStr, Font, Size.Empty).Width;
                if (finalWidth < currentItemWidth) finalWidth = currentItemWidth;
            }
            DropDownWidth = Math.Max(finalWidth, Width);

            base.OnDropDown(e);
        }

        #endregion
    }
}
