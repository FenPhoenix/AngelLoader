using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class OriginalGameModsForm : DarkFormBase, IEventDisabler
    {
        public bool EventsDisabled { get; set; }

        public string DisabledMods;

        public OriginalGameModsForm(GameIndex gameIndex, string inDisabledMods)
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif

            DisabledMods = inDisabledMods;

            OrigGameModsControl.ModsDisabledModsTextBox.Text = DisabledMods;

            if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

            Localize(gameIndex);

            try
            {
                OrigGameModsControl.ModsCheckList.SuspendDrawing();

                OrigGameModsControl.ModsCheckList.ClearList();

                (Error error, List<Mod> mods) = GameConfigFiles.GetGameMods(gameIndex);

                if (error == Error.None)
                {
                    var disabledModsList = DisabledMods
                        .Split(CA_Plus, StringSplitOptions.RemoveEmptyEntries)
                        .ToHashSetI();

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
                            @checked: !disabledModsList.Contains(mod.InternalName),
                            text: mod.InternalName,
                            caution: mod.IsUber);
                    }

                    OrigGameModsControl.ModsCheckList.FillList(checkItems, LText.ModsTab.ImportantModsCaution);
                }
            }
            finally
            {
                OrigGameModsControl.ModsCheckList.ResumeDrawing();
            }
        }

        private void Localize(GameIndex gameIndex)
        {
            Text = GetLocalizedGameName(gameIndex);
            OrigGameModsControl.Localize(GetLocalizedOriginalModHeaderText(gameIndex));
            OKButton.Text = LText.Global.OK;
            Cancel_Button.Text = LText.Global.Cancel;
        }

        private void OriginalGameMods_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK) return;
            DisabledMods = OrigGameModsControl.ModsDisabledModsTextBox.Text;
        }

        // ---

        private void OrigGameModsControl_ModsDisabledModsTextBoxTextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            DisabledMods = OrigGameModsControl.ModsDisabledModsTextBox.Text;
        }

        private void OrigGameModsControl_ModsDisabledModsTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ModsDisabledModsTextBoxCommit();
                e.SuppressKeyPress = true;
            }
        }

        private void OrigGameModsControl_ModsDisabledModsTextBoxLeave(object sender, EventArgs e)
        {
            ModsDisabledModsTextBoxCommit();
        }

        private void ModsDisabledModsTextBoxCommit()
        {
            string[] disabledMods = DisabledMods.Split(CA_Plus, StringSplitOptions.RemoveEmptyEntries);

            var modNames = new DictionaryI<int>(OrigGameModsControl.ModsCheckList.CheckItems.Length);

            for (int i = 0; i < OrigGameModsControl.ModsCheckList.CheckItems.Length; i++)
            {
                var checkItem = OrigGameModsControl.ModsCheckList.CheckItems[i];
                modNames[checkItem.Text] = i;
            }

            bool[] checkedStates = InitializedArray(OrigGameModsControl.ModsCheckList.CheckItems.Length, true);

            foreach (string mod in disabledMods)
            {
                if (modNames.TryGetValue(mod, out int index))
                {
                    checkedStates[index] = false;
                }
            }

            OrigGameModsControl.ModsCheckList.SetItemCheckedStates(checkedStates);
        }

        private void OrigGameModsControl_ModsCheckListItemCheckedChanged(object sender, DarkCheckList.DarkCheckListEventArgs e)
        {
            if (EventsDisabled) return;
            UpdateDisabledMods();
        }

        private void OrigGameModsControl_ModsEnableAllButtonClick(object sender, EventArgs e)
        {
            using (new DisableEvents(this))
            {
                foreach (Control control in OrigGameModsControl.ModsCheckList.Controls)
                {
                    if (control is CheckBox checkBox)
                    {
                        checkBox.Checked = true;
                    }
                }

                DisabledMods = "";
                OrigGameModsControl.ModsDisabledModsTextBox.Text = "";
            }
        }

        private void OrigGameModsControl_ModsDisableNonImportantButtonClick(object sender, EventArgs e)
        {
            using (new DisableEvents(this))
            {
                foreach (Control control in OrigGameModsControl.ModsCheckList.Controls)
                {
                    if (control is CheckBox checkBox && !DarkCheckList.IsControlCaution(checkBox))
                    {
                        checkBox.Checked = false;
                    }
                }

                UpdateDisabledMods();
            }
        }

        private void UpdateDisabledMods()
        {
            DisabledMods = "";

            foreach (DarkCheckList.CheckItem item in OrigGameModsControl.ModsCheckList.CheckItems)
            {
                if (!item.Checked)
                {
                    if (!DisabledMods.IsEmpty()) DisabledMods += "+";
                    DisabledMods += item.Text;
                }
            }

            using (new DisableEvents(this))
            {
                OrigGameModsControl.ModsDisabledModsTextBox.Text = DisabledMods;
            }

            DisabledMods = OrigGameModsControl.ModsDisabledModsTextBox.Text;
        }
    }
}
