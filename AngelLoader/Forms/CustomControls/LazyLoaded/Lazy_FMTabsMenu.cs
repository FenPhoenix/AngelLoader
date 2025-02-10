using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_FMTabsMenu : IDarkable
{
    private bool _constructed;
    private readonly bool[] _checkedStates = InitializedArray(FMTabCount, true);

    private readonly MainForm _owner;

    private DarkContextMenu _menu = null!;
    internal DarkContextMenu Menu
    {
        get
        {
            if (!_constructed)
            {
                #region Instantiation and hookup events

                _menu = new DarkContextMenu(_owner);

                ToolStripItem[] menuItems = new ToolStripItem[FMTabCount];
                for (int i = 0; i < menuItems.Length; i++)
                {
                    ToolStripMenuItemCustom item = new()
                    {
                        CheckOnClick = true,
                        Checked = _checkedStates[i],
                    };
                    item.Click += _owner.FMTabsMenu_MenuItems_Click;
                    menuItems[i] = item;
                }

                _menu.Items.AddRange(menuItems);

                #endregion

                _menu.SetPreventCloseOnClickItems(_menu.Items.Cast<ToolStripMenuItemCustom>().ToArray());

                _menu.DarkModeEnabled = _darkModeEnabled;

                _menu.Opening += _owner.FMTabsMenu_Opening;
                _menu.Closed += MenuClosed;

                _constructed = true;

                Localize();
            }

            return _menu;
        }
    }

    private void MenuClosed(object sender, ToolStripDropDownClosedEventArgs e)
    {
        /*
        Fix: OnPaint() has a bizarre bug where if it's drawn one tab with the hot color, then you right-click for
        a menu, then move the mouse onto another tab, then right-click for a menu again, now the first hot button
        is just ALWAYS drawn hot even if you move the mouse on and off the second hot button a trillion times -
        which causes THE BUTTON YOU'RE MOUSING OVER to change colors properly of course, but the first button you
        moused over (or all but the last in the chain if you did this multiple times) is now permanently drawn in
        hot colors, until you mouse on and off THAT particular button.

        Even though it always redraws all tabs whenever it redraws any.

        Even though Trace.WriteLine() debugging clearly shows that all values are correct for the tab to be drawn
        non-hot (and correct for all other tabs too for that matter), and it is using the correct non-hot brush.
                
        Even though the very next line after the brush gets decided, it unconditionally fills the frigging
        rectangle with the frigging non-hot brush.

        Clearly. Unambiguously. It. Is. Using. The. Non-hot. Brush.

        Except it isn't, somehow.
        
        I guess the framework doesn't let facts get in its way.

        Anyway. If we do this it fixes it, pointlessly clunkily.
        */
        if (Config.DarkMode)
        {
            _owner.TopFMTabControl.Invalidate(_owner.TopFMTabControl.GetTabBarRect());
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

    internal Lazy_FMTabsMenu(MainForm owner) => _owner = owner;

    internal void SetItemChecked(int index, bool value)
    {
        if (_constructed)
        {
            ((ToolStripMenuItemCustom)_menu.Items[index]).Checked = value;
        }
        else
        {
            _checkedStates[index] = value;
        }
    }

    internal void Localize()
    {
        if (!_constructed) return;

        for (int i = 0; i < FMTabCount; i++)
        {
            _menu.Items[i].Text = FMTabTextLocalizedStrings[i].Invoke();
        }
    }

    internal bool Focused => _constructed && _menu.Focused;
}
