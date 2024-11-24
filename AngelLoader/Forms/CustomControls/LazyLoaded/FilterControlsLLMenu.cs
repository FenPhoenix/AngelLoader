using System;
using System.Linq;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class FilterControlsLLMenu : IDarkable
{
    private bool _constructed;
    private readonly bool[] _filterCheckedStates = InitializedArray(HideableFilterControlsCount, true);

    private readonly MainForm _owner;

    private DarkContextMenu _menu = null!;
    internal DarkContextMenu Menu
    {
        get
        {
            Construct();
            return _menu;
        }
    }

    private ToolStripMenuItemCustom TitleMenuItem = null!;
    private ToolStripMenuItemCustom AuthorMenuItem = null!;
    private ToolStripMenuItemCustom ReleaseDateMenuItem = null!;
    private ToolStripMenuItemCustom LastPlayedMenuItem = null!;
    private ToolStripMenuItemCustom TagsMenuItem = null!;
    private ToolStripMenuItemCustom FinishedStateMenuItem = null!;
    private ToolStripMenuItemCustom RatingMenuItem = null!;
    private ToolStripMenuItemCustom ShowUnsupportedMenuItem = null!;
    private ToolStripMenuItemCustom ShowUnavailableFMsMenuItem = null!;
    private ToolStripMenuItemCustom ShowRecentAtTopMenuItem = null!;

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

    internal FilterControlsLLMenu(MainForm owner) => _owner = owner;

    private void Construct()
    {
        if (_constructed) return;

        _menu = new DarkContextMenu(_owner);
        _menu.Items.AddRange(
            TitleMenuItem = new ToolStripMenuItemCustom { Tag = HideableFilterControls.Title },
            AuthorMenuItem = new ToolStripMenuItemCustom { Tag = HideableFilterControls.Author },
            ReleaseDateMenuItem = new ToolStripMenuItemCustom { Tag = HideableFilterControls.ReleaseDate },
            LastPlayedMenuItem = new ToolStripMenuItemCustom { Tag = HideableFilterControls.LastPlayed },
            TagsMenuItem = new ToolStripMenuItemCustom { Tag = HideableFilterControls.Tags },
            FinishedStateMenuItem = new ToolStripMenuItemCustom { Tag = HideableFilterControls.FinishedState },
            RatingMenuItem = new ToolStripMenuItemCustom { Tag = HideableFilterControls.Rating },
            ShowUnsupportedMenuItem = new ToolStripMenuItemCustom { Tag = HideableFilterControls.ShowUnsupported },
            ShowUnavailableFMsMenuItem = new ToolStripMenuItemCustom { Tag = HideableFilterControls.ShowUnavailable },
            ShowRecentAtTopMenuItem = new ToolStripMenuItemCustom { Tag = HideableFilterControls.ShowRecentAtTop }
        );

        for (int i = 0; i < _menu.Items.Count; i++)
        {
            var item = (ToolStripMenuItemCustom)_menu.Items[i];
            item.CheckOnClick = true;
            item.Checked = _filterCheckedStates[i];
            item.Click += _owner.FilterControlsMenuItems_Click;
        }

        _menu.SetPreventCloseOnClickItems(_menu.Items.Cast<ToolStripMenuItemCustom>().ToArray());

        _menu.DarkModeEnabled = _darkModeEnabled;

        _constructed = true;

        Localize();
    }

    internal void Localize()
    {
        if (!_constructed) return;

        TitleMenuItem.Text = LText.FilterBar.ShowHideMenu_Title;
        AuthorMenuItem.Text = LText.FilterBar.ShowHideMenu_Author;
        ReleaseDateMenuItem.Text = LText.FilterBar.ShowHideMenu_ReleaseDate;
        LastPlayedMenuItem.Text = LText.FilterBar.ShowHideMenu_LastPlayed;
        TagsMenuItem.Text = LText.FilterBar.ShowHideMenu_Tags;
        FinishedStateMenuItem.Text = LText.FilterBar.ShowHideMenu_FinishedState;
        RatingMenuItem.Text = LText.FilterBar.ShowHideMenu_Rating;
        ShowUnsupportedMenuItem.Text = LText.FilterBar.ShowHideMenu_ShowUnsupported;
        ShowUnavailableFMsMenuItem.Text = LText.FilterBar.ShowHideMenu_ShowUnavailable;
        ShowRecentAtTopMenuItem.Text = LText.FilterBar.ShowHideMenu_ShowRecentAtTop;
    }

    internal void SetCheckedStates(bool[] states)
    {
        if (_constructed)
        {
            for (int i = 0; i < _menu.Items.Count; i++)
            {
                ((ToolStripMenuItemCustom)_menu.Items[i]).Checked = states[i];
            }
        }
        else
        {
            Array.Copy(states, _filterCheckedStates, HideableFilterControlsCount);
        }
    }

    internal bool[] GetCheckedStates()
    {
        bool[] ret = new bool[HideableFilterControlsCount];

        if (_constructed)
        {
            for (int i = 0; i < _menu.Items.Count; i++)
            {
                ret[i] = ((ToolStripMenuItemCustom)_menu.Items[i]).Checked;
            }
        }
        else
        {
            Array.Copy(_filterCheckedStates, ret, HideableFilterControlsCount);
        }

        return ret;
    }
}
