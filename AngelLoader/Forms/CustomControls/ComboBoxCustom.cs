using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class ComboBoxCustom : DarkComboBox
    {
        private const uint WM_CTLCOLORLISTBOX = 308;
        private const int SWP_NOSIZE = 1;

        public ComboBoxCustom()
        {
            // Hack to make it autosize in dark mode
            DropDownHeight = int.MaxValue;
        }

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
                Point p = PointToScreen(new Point(0, Height));

                int screenWidth = Screen.FromControl(this).Bounds.Width;
                bool alignRight = p.X + DropDownWidth > screenWidth;

                int x = alignRight ? p.X - (DropDownWidth - Math.Min(Width, screenWidth - p.X)) : p.X;
                SetWindowPos(m.LParam, IntPtr.Zero, x, p.Y, 0, 0, SWP_NOSIZE);
            }
            // Needed to make the MouseLeave event fire when the mouse moves off the control directly onto another
            // window (other controls work like that automatically, ComboBox doesn't)
            else if (m.Msg == Native.WM_MOUSELEAVE) // 675 / 0x2A3
            {
                OnMouseLeave(EventArgs.Empty);
                m.Result = (IntPtr)1;
                // NOTE:
                // If we return here, the ComboBox remains highlighted even when the mouse leaves.
                // If we don't return here, the OnMouseLeave event gets fired twice. That's irritating, but in
                // this particular case it's fine, it just hides the readme controls twice. But remember in case
                // you want to do anything more complicated...
            }

            base.WndProc(ref m);
        }

        protected override void OnDropDown(EventArgs e)
        {
            // Autosize dropdown to accomodate the longest item
            int finalWidth = 0;
            foreach (object item in Items)
            {
                if (item is not string itemStr) continue;

                int currentItemWidth = TextRenderer.MeasureText(itemStr, Font, Size.Empty).Width;
                if (finalWidth < currentItemWidth) finalWidth = currentItemWidth;
            }
            DropDownWidth = Math.Max(finalWidth, Width);

            base.OnDropDown(e);
        }

        #endregion
    }
}
