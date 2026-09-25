using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_RTFBoxMenu : IDarkable
{
    private bool _constructed;

    private readonly RichTextBoxCustom _owner;

    internal Lazy_RTFBoxMenu(RichTextBoxCustom owner) => _owner = owner;

    private DarkContextMenu _menu = null!;
    internal DarkContextMenu Menu
    {
        get
        {
            Construct();
            return _menu;
        }
    }

    private ToolStripMenuItemCustom CopyMenuItem = null!;
    private ToolStripMenuItemCustom SelectAllMenuItem = null!;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!_constructed) return;

            _menu.DarkModeEnabled = _darkModeEnabled;
        }
    }

    private void Construct()
    {
        if (_constructed) return;

        _menu = new DarkContextMenu(_owner);
        _menu.Items.AddRange(new ToolStripItem[]
        {
            CopyMenuItem = new ToolStripMenuItemCustom(),
            new ToolStripSeparator(),
            SelectAllMenuItem = new ToolStripMenuItemCustom(),
        });

        _menu.Opening += MenuOpening;

        CopyMenuItem.Click += (_, _) => _owner.Copy();
        SelectAllMenuItem.Click += (_, _) => _owner.SelectAll();

        _menu.DarkModeEnabled = _darkModeEnabled;

        _constructed = true;

        Localize();
    }

    internal void Localize()
    {
        if (!_constructed) return;

        CopyMenuItem.Text = LText.Global.Copy;
        SelectAllMenuItem.Text = LText.Global.SelectAll;
    }

    private void MenuOpening(object? sender, CancelEventArgs e)
    {
        if (!_owner.Visible)
        {
            e.Cancel = true;
            return;
        }

        CopyMenuItem.Enabled = _owner.SelectionLength > 0;
        SelectAllMenuItem.Enabled = !(_owner.SelectionStart == 0 &&
                                      _owner.SelectionLength ==
                                      _owner.TextLength);
    }
}
