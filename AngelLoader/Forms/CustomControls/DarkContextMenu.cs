using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkContextMenu : ContextMenuStrip
{
    private bool _preventClose;
    private ToolStripMenuItemCustom[]? _preventCloseItems;

    /// <summary>
    /// Since event driven architecture is Good Architecture(tm), of course that means you can't simply just pass
    /// data from a menu open call to a menu item click handler. So set this data and it will be nulled out on close.
    /// </summary>
    public object? Data;

    private readonly IDarkContextMenuOwner _owner;

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
            RefreshDarkModeState();
        }
    }

    #region Constructors

    public DarkContextMenu(IDarkContextMenuOwner owner) : base(owner.GetComponents())
    {
        _owner = owner;
        Tag = LoadType.Lazy;
    }

    protected override void OnOpening(CancelEventArgs e)
    {
        if (_owner.ViewBlocked) e.Cancel = true;
        base.OnOpening(e);
    }

    #endregion

    internal void SetPreventCloseOnClickItems(params ToolStripMenuItemCustom[] items) => _preventCloseItems = items;

    protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
    {
        if (_preventCloseItems?.Length > 0)
        {
            _preventClose = _preventCloseItems.Contains(e.ClickedItem) && ((ToolStripMenuItemCustom)e.ClickedItem).CheckOnClick;
        }

        base.OnItemClicked(e);
    }

    protected override void OnClosing(ToolStripDropDownClosingEventArgs e)
    {
        if (_preventCloseItems?.Length > 0 && _preventClose)
        {
            _preventClose = false;
            e.Cancel = true;
            return;
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
    {
        Data = null;
        base.OnClosed(e);
    }

    internal void RefreshDarkModeState()
    {
        void SetMenuTheme(ToolStripDropDown menu)
        {
            if (_darkModeEnabled)
            {
                // We can't cache this, because it stops working on a second dark mode set if we do
                menu.Renderer = new DarkMenuRenderer();
            }
            else
            {
                menu.RenderMode = ToolStripRenderMode.ManagerRenderMode;

                // Prevents wrong back color on separators
                menu.ResetBackColor();
                menu.ResetForeColor();

                // Prevents wrong back/fore color on items
                foreach (ToolStripItem item in menu.Items)
                {
                    item.ResetBackColor();
                    item.ResetForeColor();
                }
            }

            foreach (ToolStripItem item in menu.Items)
            {
                if (item is ToolStripMenuItem { DropDown: not null } menuItem)
                {
                    SetMenuTheme(menuItem.DropDown);
                }
            }
        }

        SetMenuTheme(this);
    }
}
