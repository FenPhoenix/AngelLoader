using System;
using System.Linq;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class GameFilterControlsLLMenu : IDarkable
{
    private bool _constructed;
    private readonly bool[] _checkedStates = InitializedArray(SupportedGameCount, true);

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

    internal GameFilterControlsLLMenu(MainForm owner) => _owner = owner;

    private void Construct()
    {
        if (_constructed) return;

        _menu = new DarkContextMenu(_owner);

        for (int i = 0; i < SupportedGameCount; i++)
        {
            ToolStripMenuItemCustom item = new()
            {
                CheckOnClick = true,
                Tag = i,
                Checked = _checkedStates[i],
            };
            item.Click += _owner.GameFilterControlsMenuItems_Click;

            _menu.Items.Add(item);
        }

        _menu.SetPreventCloseOnClickItems(_menu.Items.Cast<ToolStripMenuItemCustom>().ToArray());

        _menu.DarkModeEnabled = _darkModeEnabled;

        _constructed = true;

        Localize();
    }

    internal void Localize()
    {
        if (!_constructed) return;

        for (int i = 0; i < SupportedGameCount; i++)
        {
            _menu.Items[i].Text = GetLocalizedGameName((GameIndex)i);
        }
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
            Array.Copy(states, _checkedStates, SupportedGameCount);
        }
    }

    internal bool[] GetCheckedStates()
    {
        bool[] ret = new bool[SupportedGameCount];

        if (_constructed)
        {
            for (int i = 0; i < _menu.Items.Count; i++)
            {
                ret[i] = ((ToolStripMenuItemCustom)_menu.Items[i]).Checked;
            }
        }
        else
        {
            Array.Copy(_checkedStates, ret, SupportedGameCount);
        }

        return ret;
    }
}
