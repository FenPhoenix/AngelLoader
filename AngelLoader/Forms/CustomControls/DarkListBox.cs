using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

// BUG: DarkListBox: DPI (possible) can mess up auto-column-size
public class DarkListBox : ListView, IDarkable, IUpdateRegion
{
    private bool _ctrlDown;
    private bool _shiftDown;
    private int _updatingItems;

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            BackColor = _darkModeEnabled ? DarkColors.LightBackground : SystemColors.Window;

            Invalidate();

            // Hack to fix the background color not changing with modes if we're disabled
            if (!Enabled)
            {
                Native.PostMessageW(Handle, Native.WM_ENABLE, 1, IntPtr.Zero);
                Native.PostMessageW(Handle, Native.WM_ENABLE, 0, IntPtr.Zero);
            }
        }
    }

#if DEBUG

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new BorderStyle BorderStyle { get; set; }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool FullRowSelect { get; set; }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ColumnHeaderStyle HeaderStyle { get; set; }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool HideSelection { get; set; }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool LabelWrap { get; set; }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool OwnerDraw { get; set; }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool ShowGroups { get; set; }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool UseCompatibleStateImageBehavior { get; set; }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new View View { get; set; }

#endif

    public DarkListBox()
    {
        base.FullRowSelect = true;
        base.HeaderStyle = ColumnHeaderStyle.None;
        base.HideSelection = false;
        base.LabelWrap = false;
        base.ShowGroups = false;
        base.UseCompatibleStateImageBehavior = false;

        // We would like to just do View.List, but then we get our items in implicit columns going across,
        // instead of just one column going downwards. So we have to use View.Details and add an invisible-
        // headered column.
        base.View = View.Details;
        Columns.Add("");

        base.BorderStyle = BorderStyle.FixedSingle;

        base.OwnerDraw = true;
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

    [Browsable(false)]
    public string[] ItemsAsStrings
    {
        get
        {
            string[] ret = new string[Items.Count];
            for (int i = 0; i < Items.Count; i++)
            {
                ret[i] = Items[i].Text;
            }
            return ret;
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int SelectedIndex
    {
        get => SelectedIndices.Count == 0 ? -1 : SelectedIndices[0];
        set
        {
            SelectedIndices.Clear();
            if (value > -1) SelectedIndices.Add(value);
        }
    }

    [Browsable(false)]
    public string SelectedItem => SelectedItems.Count == 0 ? "" : SelectedItems[0].Text;

    [Browsable(false)]
    public string[] SelectedItemsAsStrings
    {
        get
        {
            string[] ret = new string[SelectedItems.Count];
            for (int i = 0; i < SelectedItems.Count; i++)
            {
                ret[i] = SelectedItems[i].Text;
            }
            return ret;
        }
    }

    // Tested, this height is correct for the current ListView version
    [Browsable(false)]
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

    protected override void OnSizeChanged(EventArgs e)
    {
        if (_darkModeEnabled)
        {
            Invalidate();
        }
        base.OnSizeChanged(e);
    }

    protected override void OnDrawItem(DrawListViewItemEventArgs e)
    {
        base.OnDrawItem(e);

        bool itemSelected = Items[e.ItemIndex].Selected;

        // Full-width item hack part deux: We can't tell it to make the actual selection area full-width, so
        // we just draw it full-width ourselves and handle the click interaction later (see WndProc).
        Rectangle selRect = e.Bounds with { Width = ClientRectangle.Width - e.Bounds.X };

        Brush bgBrush =
            itemSelected
                ? _darkModeEnabled
                    ? !Enabled
                        ? DarkColors.GetCachedSolidBrush(BackColor)
                        : DarkColors.BlueSelectionBrush
                    : !Enabled
                        ? SystemBrushes.ControlLight
                        : SystemBrushes.Highlight
                : _darkModeEnabled
                    ? !Enabled
                        ? DarkColors.Fen_ControlBackgroundBrush
                        : DarkColors.GetCachedSolidBrush(BackColor)
                    : !Enabled
                        ? SystemBrushes.Control
                        : DarkColors.GetCachedSolidBrush(BackColor);

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
                        ? BackColor
                        : DarkColors.Fen_ControlBackground
                    : itemSelected
                        ? DarkColors.BlueSelection
                        : BackColor
                : !Enabled
                    ? itemSelected
                        ? SystemColors.ControlLight
                        : SystemColors.Control
                    : itemSelected
                        ? SystemColors.Highlight
                        : BackColor;

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

        using Native.GraphicsContext gc = new(hWnd);
        gc.G.DrawRectangle(DarkColors.Fen_ControlBackgroundPen, new Rectangle(1, 1, Width - 3, Height - 3));
        gc.G.DrawRectangle(DarkColors.LightBorderPen, new Rectangle(0, 0, Width - 1, Height - 1));
    }

    // @DarkModeNote(DarkListBox/ListView): We could get super thorough and handle multi-select with shift properly...
    // But, meh... how it works is fine enough probably?
    private void HandleLButtonDown(ref Message m)
    {
        m.LParam = Native.MAKELPARAM(2, Native.SignedHIWORD(m.LParam));

        Point modCursorPos = new(2, this.ClientCursorPos().Y);

        ListViewHitTestInfo hitTest = HitTest(modCursorPos);

        ListViewItem? item = hitTest.Item;

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
            case Native.LVM_SETITEMTEXTA:
            case Native.LVM_SETITEMTEXTW:
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
            // Let MouseUp through for the tags dropdown!
            //case Native.WM_LBUTTONUP:
            case Native.WM_LBUTTONDBLCLK:
                break;
            case Native.WM_NCPAINT:
                if (_darkModeEnabled) DrawBorder(m.HWnd);
                base.WndProc(ref m);
                break;
            case Native.WM_ENABLE:
                if (_darkModeEnabled && m.WParam == IntPtr.Zero)
                {
                    using (new Win32ThemeHooks.OverrideSysColorScope(Win32ThemeHooks.Override.Full))
                    {
                        base.WndProc(ref m);
                    }
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
