using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class ModsControl : UserControl
    {
        public ModsControl()
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif
            ModsCheckList.SetCautionVisiblePredicate(() => ModsShowUberCheckBox.Checked);
        }

        public void Localize(string headerText)
        {
            ModsHeaderLabel.Text = headerText;
            ModsShowUberCheckBox.Text = LText.ModsTab.ShowImportantMods;
            ModsEnableAllButton.Text = LText.ModsTab.EnableAll;
            ModsDisableNonImportantButton.Text = LText.ModsTab.DisableAll;
            MainToolTip.SetToolTip(ModsDisableNonImportantButton, LText.ModsTab.DisableAllToolTip);
            ModsDisabledModsLabel.Text = LText.ModsTab.DisabledMods;
        }

        public (bool Success, string DisabledMods, bool DisableAllMods)
        Set(Game game, string disabledMods, bool disableAllMods)
        {
            var fail = (false, "", false);

            try
            {
                ModsCheckList.SuspendDrawing();

                ModsDisabledModsTextBox.Text = disabledMods;

                ModsCheckList.ClearList();

                if (!GameIsDark(game)) return fail;

                (Error error, List<Mod> mods) = GameConfigFiles.GetGameMods(GameToGameIndex(game));

                if (error != Error.None) return fail;

                var disabledModsList = disabledMods
                    .Split(CA_Plus, StringSplitOptions.RemoveEmptyEntries)
                    .ToHashSetI();

                bool allDisabled = disableAllMods;

                if (allDisabled) disabledMods = "";

                for (int i = 0; i < mods.Count; i++)
                {
                    Mod mod = mods[i];
                    if (mod.IsUber)
                    {
                        mods.RemoveAt(i);
                        mods.Add(mod);
                    }
                }

                var checkItems = new DarkCheckList.CheckItem[mods.Count];

                for (int i = 0; i < mods.Count; i++)
                {
                    Mod mod = mods[i];
                    checkItems[i] = new DarkCheckList.CheckItem(
                        @checked: allDisabled ? mod.IsUber : !disabledModsList.Contains(mod.InternalName),
                        text: mod.InternalName,
                        caution: mod.IsUber);

                    if (allDisabled && !mod.IsUber)
                    {
                        if (!disabledMods.IsEmpty()) disabledMods += "+";
                        disabledMods += mod.InternalName;
                    }
                }

                if (allDisabled)
                {
                    ModsDisabledModsTextBox.Text = disabledMods;
                    disableAllMods = false;
                }

                ModsCheckList.FillList(checkItems, LText.ModsTab.ImportantModsCaution);

                return (true, disabledMods, disableAllMods);
            }
            finally
            {
                ModsCheckList.ResumeDrawing();
            }
        }

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler? ModsEnableAllButtonClick;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler? ModsDisableNonImportantButtonClick;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler? ModsDisabledModsTextBoxTextChanged;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler<KeyEventArgs>? ModsDisabledModsTextBoxKeyDown;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler? ModsDisabledModsTextBoxLeave;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler<DarkCheckList.DarkCheckListEventArgs>? ModsCheckListItemCheckedChanged;

        private void ModsEnableAllButton_Click(object sender, EventArgs e)
        {
            ModsEnableAllButtonClick?.Invoke(ModsEnableAllButton, e);
        }

        private void ModsDisableNonImportantButton_Click(object sender, EventArgs e)
        {
            ModsDisableNonImportantButtonClick?.Invoke(ModsDisableNonImportantButton, e);
        }

        private void ModsShowUberCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ModsCheckList.ShowCautionSection(ModsShowUberCheckBox.Checked);
        }

        private void ModsDisabledModsTextBox_TextChanged(object sender, EventArgs e)
        {
            ModsDisabledModsTextBoxTextChanged?.Invoke(ModsDisabledModsTextBox, e);
        }

        private void ModsDisabledModsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            ModsDisabledModsTextBoxKeyDown?.Invoke(ModsDisabledModsTextBox, e);
        }

        private void ModsDisabledModsTextBox_Leave(object sender, EventArgs e)
        {
            ModsDisabledModsTextBoxLeave?.Invoke(ModsDisabledModsTextBox, e);
        }

        private void ModsCheckList_ItemCheckedChanged(object sender, DarkCheckList.DarkCheckListEventArgs e)
        {
            ModsCheckListItemCheckedChanged?.Invoke(ModsCheckList, e);
        }
    }
}
