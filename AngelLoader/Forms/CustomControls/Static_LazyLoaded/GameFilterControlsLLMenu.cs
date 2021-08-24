using System;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using static AL_Common.Utils;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class GameFilterControlsLLMenu
    {
        private static bool _constructed;
        private static readonly bool[] _checkedStates = InitializedArray(SupportedGameCount, true);

        private static MainForm _owner = null!;

        internal static DarkContextMenu Menu = null!;

        private static bool _darkModeEnabled;
        [PublicAPI]
        public static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                Menu.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            _owner = form;

            Menu = new DarkContextMenu(_darkModeEnabled, components) { Tag = LazyLoaded.True };

            for (int i = 0; i < SupportedGameCount; i++)
            {
                var item = new ToolStripMenuItemCustom
                {
                    CheckOnClick = true,
                    Tag = i,
                    Checked = _checkedStates[i],
                };
                item.Click += _owner.GameFilterControlsMenuItems_Click;

                Menu.Items.Add(item);
            }

            Menu.SetPreventCloseOnClickItems(Menu.Items.Cast<ToolStripMenuItemCustom>().ToArray());

            _constructed = true;

            Localize();
        }

        internal static void Localize()
        {
            if (!_constructed) return;

            for (int i = 0; i < SupportedGameCount; i++)
            {
                Menu.Items[i].Text = GetLocalizedGameName((GameIndex)i);
            }
        }

        internal static void SetCheckedStates(bool[] states)
        {
            if (_constructed)
            {
                for (int i = 0; i < Menu.Items.Count; i++)
                {
                    ((ToolStripMenuItemCustom)Menu.Items[i]).Checked = states[i];
                }
            }
            else
            {
                Array.Copy(states, _checkedStates, SupportedGameCount);
            }
        }

        internal static bool[] GetCheckedStates()
        {
            bool[] ret = new bool[SupportedGameCount];

            if (_constructed)
            {
                for (int i = 0; i < Menu.Items.Count; i++)
                {
                    ret[i] = ((ToolStripMenuItemCustom)Menu.Items[i]).Checked;
                }
            }
            else
            {
                Array.Copy(_checkedStates, ret, SupportedGameCount);
            }

            return ret;
        }
    }
}
