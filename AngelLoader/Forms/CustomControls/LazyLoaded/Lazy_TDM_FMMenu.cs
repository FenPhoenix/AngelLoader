using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AL_Common;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_TDM_FMMenu : IDarkable
{
    private bool _constructed;

    private readonly MainForm _owner;

    private DataGridViewTDM DGV => _owner.Lazy_TDMDataGridView.DGV;

    private DarkContextMenu _menu = null!;
    internal DarkContextMenu Menu
    {
        get
        {
            Construct();
            return _menu;
        }
    }

    private ToolStripMenuItemCustom DownloadMarkedMenuItem = null!;
    private ToolStripMenuItemCustom MarkForDownloadFMMenuItem = null!;

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

    internal Lazy_TDM_FMMenu(MainForm owner) => _owner = owner;

    private void Construct()
    {
        if (_constructed) return;

        _menu = new DarkContextMenu(_owner);

        _menu.Items.AddRange(new ToolStripItem[]
        {
            DownloadMarkedMenuItem = new ToolStripMenuItemCustom(),
            MarkForDownloadFMMenuItem = new ToolStripMenuItemCustom(),
        });

        _menu.Opening += MenuOpening;
        DownloadMarkedMenuItem.Click += AsyncMenuItems_Click;
        MarkForDownloadFMMenuItem.Click += AsyncMenuItems_Click;

        _menu.DarkModeEnabled = _darkModeEnabled;

        _constructed = true;

        Localize();
    }

    internal void Localize()
    {
        // @TDM: implement localization
        DownloadMarkedMenuItem.Text = "Download marked";
        MarkForDownloadFMMenuItem.Text = "Mark for download";
    }

    private async void AsyncMenuItems_Click(object sender, EventArgs e)
    {
        if (sender == DownloadMarkedMenuItem)
        {
            await DGV.DownloadMarked();
        }
        else if (sender == MarkForDownloadFMMenuItem)
        {
            // @TDM: Implement multiselect
            TDM_ServerFMData serverData = DGV.GetMainSelectedFM();
            serverData.MarkedForDownload = !serverData.MarkedForDownload;
            DGV.Refresh();
        }
    }

    private void MenuOpening(object sender, CancelEventArgs e)
    {
        if (DGV.RowCount == 0 || !DGV.RowSelected())
        {
            e.Cancel = true;
        }
    }
}
