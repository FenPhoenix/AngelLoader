using System;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkListBox : ListView, IDarkable
    {
        private bool _ctrlDown;
        private bool _shiftDown;
        private int _updatingItems;

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                if (_darkModeEnabled)
                {
                    BackColor = DarkColors.Fen_DarkBackground;
                }
                else
                {
                    BackColor = SystemColors.Window;
                }

                Refresh();
            }
        }

        public DarkListBox()
        {
            FullRowSelect = true;
            HeaderStyle = ColumnHeaderStyle.None;
            HideSelection = false;
            LabelWrap = false;
            ShowGroups = false;
            UseCompatibleStateImageBehavior = false;

            // We would like to just do View.List, but then we get our items in implicit columns going across,
            // instead of just one column going downwards. So we have to use View.Details and add an invisible-
            // headered column.
            View = View.Details;
            Columns.Add("");

            BorderStyle = BorderStyle.FixedSingle;

            OwnerDraw = true;
            base.DoubleBuffered = true;
        }

        #region Public methods

        public new void BeginUpdate()
        {
            base.BeginUpdate();
            _updatingItems++;
        }

        public new void EndUpdate()
        {
            base.EndUpdate();
            _updatingItems = (_updatingItems - 1).ClampToZero();
            if (_updatingItems == 0)
            {
                // It's way too slow to do this for every single item added, so when we're in update mode, just
                // do it only once at the end of the update. This keeps us fast enough to get on with.
                AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }

        internal void RemoveAndSelectNearest()
        {
            if (SelectedIndex == -1) return;

            int oldSelectedIndex = SelectedIndex;

            Items.RemoveAt(SelectedIndex);

            if (oldSelectedIndex < Items.Count && Items.Count > 1)
            {
                SelectedIndex = oldSelectedIndex;
            }
            else if (Items.Count > 1)
            {
                SelectedIndex = oldSelectedIndex - 1;
            }
            else if (Items.Count == 1)
            {
                SelectedIndex = 0;
            }
        }

        #endregion

        #region Public properties

        public string[] ItemsAsStrings
        {
            get
            {
                string[] ret = new string[Items.Count];
                for (int i = 0; i < Items.Count; i++)
                {
                    ret[i] = Items[i]?.Text ?? "";
                }
                return ret;
            }
        }

        public int SelectedIndex
        {
            get => SelectedIndices.Count == 0 ? -1 : SelectedIndices[0];
            set
            {
                SelectedIndices.Clear();
                if (value > -1) SelectedIndices.Add(value);
            }
        }

        public string SelectedItem => SelectedItems.Count == 0 ? "" : SelectedItems[0]?.Text ?? "";

        public string[] SelectedItemsAsStrings
        {
            get
            {
                string[] ret = new string[SelectedItems.Count];
                for (int i = 0; i < SelectedItems.Count; i++)
                {
                    ret[i] = SelectedItems[i]?.Text ?? "";
                }
                return ret;
            }
        }

        // TODO: @DarkMode(DarkListBox:ListView): Determine the actual item height (this is for the old DGV version)
        public int ItemHeight => Font.Height + 4;

        #endregion

        #region Event overrides

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Control) _ctrlDown = true;
            if (e.Shift) _shiftDown = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (!e.Control) _ctrlDown = false;
            if (!e.Shift) _shiftDown = false;
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            // Invalidate() for performance (Refresh() causes lag)
            Invalidate();
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            base.OnDrawItem(e);

            bool itemSelected = Items[e.ItemIndex].Selected;

            // Full-width item hack part deux: We can't tell it to make the actual selection area full-width, so
            // we just draw it full-width ourselves and handle the click interaction later (see WndProc).
            var selRect = new Rectangle(
                e.Bounds.X,
                e.Bounds.Y,
                ClientRectangle.Width - e.Bounds.X,
                e.Bounds.Height
            );

            Brush bgBrush =
                itemSelected
                    ? _darkModeEnabled
                        ? !Enabled
                            ? DarkColors.LightBackgroundBrush
                            : DarkColors.BlueSelectionBrush
                        : !Enabled
                            ? SystemBrushes.ControlLight
                            : SystemBrushes.Highlight
                    : _darkModeEnabled
                        ? !Enabled
                            ? DarkColors.Fen_ControlBackgroundBrush
                            : DarkColors.Fen_DarkBackgroundBrush
                        : !Enabled
                            ? SystemBrushes.Control
                            : SystemBrushes.Window;

            e.Graphics.FillRectangle(bgBrush, selRect);

            Color textColor =
                _darkModeEnabled
                    ? !Enabled
                        ? DarkColors.DisabledText
                        : itemSelected
                            ? DarkColors.Fen_HighlightText
                            : DarkColors.LightText
                    : !Enabled
                        ? SystemColors.GrayText
                        : itemSelected
                            ? SystemColors.HighlightText
                            : SystemColors.ControlText;

            Color textBackColor =
                _darkModeEnabled
                    ? !Enabled
                        ? itemSelected
                            ? DarkColors.LightBackground
                            : DarkColors.Fen_ControlBackground
                        : itemSelected
                            ? DarkColors.BlueSelection
                            : DarkColors.Fen_DarkBackground
                    : !Enabled
                        ? itemSelected
                            ? SystemColors.ControlLight
                            : SystemColors.Control
                        : itemSelected
                            ? SystemColors.Highlight
                            : SystemColors.Window;

            const TextFormatFlags textFormatFlags =
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoClipping |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.SingleLine;

            TextRenderer.DrawText(e.Graphics, e.Item.Text, e.Item.Font, e.Bounds, textColor, textBackColor, textFormatFlags);
        }

        private void DrawBorder(IntPtr hWnd)
        {
            if (!_darkModeEnabled || BorderStyle == BorderStyle.None) return;

            using var dc = new Native.DeviceContext(hWnd);
            using Graphics g = Graphics.FromHdc(dc.DC);
            g.DrawRectangle(DarkColors.Fen_ControlBackgroundPen, new Rectangle(1, 1, Width - 3, Height - 3));
            g.DrawRectangle(DarkColors.LightBorderPen, new Rectangle(0, 0, Width - 1, Height - 1));
        }

        // @DarkModeNote(DarkListBox/ListView): We could get super thorough and handle multi-select with shift properly...
        // But, meh... how it works is fine enough probably?
        private void HandleLButtonDown(ref Message m)
        {
            m.LParam = Native.MAKELPARAM(2, Native.SignedHIWORD(m.LParam));

            var modCursorPos = new Point(2, PointToClient(Cursor.Position).Y);

            ListViewHitTestInfo hitTest = HitTest(modCursorPos);

            var item = hitTest.Item;

            if (item != null)
            {
                if (item.Selected)
                {
                    if (MultiSelect)
                    {
                        if (_ctrlDown)
                        {
                            item.Selected = false;
                            return;
                        }
                        else if (!_shiftDown)
                        {
                            SelectedItems.Clear();
                            item.Selected = true;
                            return;
                        }
                    }
                }
                else
                {
                    if (MultiSelect && _ctrlDown)
                    {
                        item.Selected = true;
                        return;
                    }
                }
            }
            else
            {
                SelectedItems.Clear();
                return;
            }

            base.WndProc(ref m);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                // Because we have to have an explicit column and all that (see ctor), we have to tell it to
                // autosize to accomodate its content whenever the items list changes in a way that could change
                // the max width of it.
                case Native.LVM_SETITEMA:
                case Native.LVM_SETITEMW:
                case Native.LVM_INSERTITEMA:
                case Native.LVM_INSERTITEMW:
                case Native.LVM_DELETEITEM:
                case Native.LVM_DELETEALLITEMS:
                    if (_updatingItems == 0)
                    {
                        // This must come BEFORE the autosize or it won't work.
                        base.WndProc(ref m);
                        AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                    break;
                // Full-width item hack part trois: Even though we're drawing the selection full-width, the mouse
                // down will still do nothing if it's outside the actual column rectangle. So just set the X coord
                // to way over on the left all the time, and that makes it work like you'd expect. Repulsive, but
                // there you are.
                case Native.WM_LBUTTONDOWN:
                    HandleLButtonDown(ref m);
                    break;
                case Native.WM_MBUTTONDOWN:
                case Native.WM_MBUTTONUP:
                case Native.WM_MBUTTONDBLCLK:
                case Native.WM_RBUTTONDOWN:
                case Native.WM_RBUTTONUP:
                case Native.WM_RBUTTONDBLCLK:
                case Native.WM_LBUTTONUP:
                case Native.WM_LBUTTONDBLCLK:
                    break;
                case Native.WM_NCPAINT:
                    if (_darkModeEnabled) DrawBorder(m.HWnd);
                    base.WndProc(ref m);
                    break;
                case Native.WM_ENABLE:
                    if (_darkModeEnabled && m.WParam == IntPtr.Zero)
                    {
                        NativeHooks.SysColorOverride = NativeHooks.Override.Full;
                        base.WndProc(ref m);
                        NativeHooks.SysColorOverride = NativeHooks.Override.None;
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        #endregion
    }
}
