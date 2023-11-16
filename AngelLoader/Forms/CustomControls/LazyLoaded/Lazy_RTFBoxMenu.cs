using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_RTFBoxMenu : IDarkable
{
    private bool _constructed;

    private readonly MainForm _owner;

    internal Lazy_RTFBoxMenu(MainForm owner) => _owner = owner;

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
            SelectAllMenuItem = new ToolStripMenuItemCustom()
        });

        _menu.Opening += MenuOpening;

        CopyMenuItem.Click += (_, _) => _owner.ReadmeRichTextBox.Copy();
        SelectAllMenuItem.Click += (_, _) => _owner.ReadmeRichTextBox.SelectAll();

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
        if (!_owner.ReadmeRichTextBox.Visible)
        {
            e.Cancel = true;
            return;
        }

        CopyMenuItem.Enabled = _owner.ReadmeRichTextBox.SelectionLength > 0;
        SelectAllMenuItem.Enabled = !(_owner.ReadmeRichTextBox.SelectionStart == 0 &&
                                      _owner.ReadmeRichTextBox.SelectionLength ==
                                      _owner.ReadmeRichTextBox.TextLength);
    }
}
