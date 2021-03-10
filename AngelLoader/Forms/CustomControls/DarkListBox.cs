using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkListBox : ListView, IDarkableScrollableNative
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Suspended { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Native VerticalVisualScrollBar { get; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Native HorizontalVisualScrollBar { get; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Corner VisualScrollBarCorner { get; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? Scroll;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? DarkModeChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? RefreshIfNeededForceCorner;

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
                DarkModeChanged?.Invoke(this, EventArgs.Empty);
                Refresh();
            }
        }

        private int _updatingItems;

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
                if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                // It's way too slow to do this for every single item added, so when we're in update mode, just
                // do it only once at the end of the update. This keeps us fast enough to get on with.
                AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
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

            VerticalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: true, passMouseWheel: true);
            HorizontalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: false, passMouseWheel: true);
            VisualScrollBarCorner = new ScrollBarVisualOnly_Corner(this);
        }

        #region Public methods

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

        #endregion

        #region Public properties

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

        public new void BringToFront()
        {
            base.BringToFront();
            VerticalVisualScrollBar.BringToFront();
            HorizontalVisualScrollBar.BringToFront();
            VisualScrollBarCorner.BringToFront();
        }

        #region Event overrides

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Refresh();
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
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

            if (itemSelected)
            {
                Brush brush = _darkModeEnabled ? DarkColors.BlueSelectionBrush : SystemBrushes.Highlight;
                e.Graphics.FillRectangle(brush, selRect);
            }
            else
            {
                Brush brush = _darkModeEnabled ? DarkColors.Fen_DarkBackgroundBrush : SystemBrushes.Window;
                e.Graphics.FillRectangle(brush, selRect);
            }

            Color textColor =
                _darkModeEnabled
                    ? itemSelected
                        ? DarkColors.LightText
                        : DarkColors.LightText
                    : itemSelected
                        ? SystemColors.HighlightText
                        : SystemColors.ControlText;

            Color textBackColor =
                _darkModeEnabled
                    ? itemSelected
                        ? DarkColors.BlueSelection
                        : DarkColors.Fen_DarkBackground
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

        #region Visible / Show / Hide overrides

        [PublicAPI]
        public new bool Visible
        {
            get => base.Visible;
            set
            {
                if (value)
                {
                    // Do this before setting the Visible value to avoid the classic-bar-flicker
                    VerticalVisualScrollBar?.ForceSetVisibleState(true);
                    HorizontalVisualScrollBar?.ForceSetVisibleState(true);
                    base.Visible = true;
                }
                else
                {
                    base.Visible = false;
                    VerticalVisualScrollBar?.ForceSetVisibleState(false);
                    HorizontalVisualScrollBar?.ForceSetVisibleState(false);
                }
            }
        }

        [PublicAPI]
        public new void Show()
        {
            VerticalVisualScrollBar?.ForceSetVisibleState(true);
            HorizontalVisualScrollBar?.ForceSetVisibleState(true);
            base.Show();
        }

        [PublicAPI]
        public new void Hide()
        {
            base.Hide();
            VerticalVisualScrollBar?.ForceSetVisibleState(false);
            HorizontalVisualScrollBar?.ForceSetVisibleState(false);
        }

        #endregion

        private void DrawBorder(IntPtr hWnd)
        {
            if (!_darkModeEnabled || BorderStyle == BorderStyle.None) return;

            using var dc = new Native.DeviceContext(hWnd);
            using Graphics g = Graphics.FromHdc(dc.DC);
            g.DrawRectangle(DarkColors.Fen_ControlBackgroundPen, new Rectangle(1, 1, Width - 3, Height - 3));
            g.DrawRectangle(DarkColors.LightBorderPen, new Rectangle(0, 0, Width - 1, Height - 1));
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
                        if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
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
                case Native.WM_LBUTTONUP:
                case Native.WM_LBUTTONDBLCLK:
                    m.LParam = Native.MAKELPARAM(2, Native.SignedHIWORD(m.LParam));
                    base.WndProc(ref m);
                    break;
                case Native.WM_MBUTTONDOWN:
                case Native.WM_MBUTTONUP:
                case Native.WM_MBUTTONDBLCLK:
                case Native.WM_RBUTTONDOWN:
                case Native.WM_RBUTTONUP:
                case Native.WM_RBUTTONDBLCLK:
                    break;
                case Native.WM_PAINT:
                case Native.WM_VSCROLL:
                case Native.WM_HSCROLL:
                    base.WndProc(ref m);
                    if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                    break;
                case Native.WM_CTLCOLORSCROLLBAR:
                case Native.WM_NCPAINT:
                    if (_darkModeEnabled)
                    {
                        if (m.Msg == Native.WM_NCPAINT)
                        {
                            DrawBorder(m.HWnd);
                        }
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
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
