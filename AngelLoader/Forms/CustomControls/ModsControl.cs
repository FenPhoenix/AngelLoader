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
    public sealed partial class ModsControl : UserControl, IEventDisabler
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EventsDisabled { get; set; }

#if DEBUG

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [PublicAPI]
        public new bool AutoScroll { get; set; }

#endif

        public ModsControl()
        {
#if DEBUG
            InitializeComponent();
#else
            InitSlim();
#endif
            CheckList.SetCautionVisiblePredicate(() => ShowUberCheckBox.Checked);
        }

        public void Localize(string headerText)
        {
            HeaderLabel.Text = headerText;
            ShowUberCheckBox.Text = LText.ModsTab.ShowImportantMods;
            EnableAllButton.Text = LText.ModsTab.EnableAll;
            DisableNonImportantButton.Text = LText.ModsTab.DisableAll;
            MainToolTip.SetToolTip(DisableNonImportantButton, LText.ModsTab.DisableAllToolTip);
            DisabledModsLabel.Text = LText.ModsTab.DisabledMods;
        }

        private void Commit()
        {
            string[] disabledMods = ModsDisabledModsTextBox.Text.Split(CA_Plus, StringSplitOptions.RemoveEmptyEntries);

            var modNames = new DictionaryI<int>(CheckList.CheckItems.Length);

            for (int i = 0; i < CheckList.CheckItems.Length; i++)
            {
                var checkItem = CheckList.CheckItems[i];
                modNames[checkItem.Text] = i;
            }

            bool[] checkedStates = InitializedArray(CheckList.CheckItems.Length, true);

            foreach (string mod in disabledMods)
            {
                if (modNames.TryGetValue(mod, out int index))
                {
                    checkedStates[index] = false;
                }
            }

            CheckList.SetItemCheckedStates(checkedStates);

            DisabledModsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public (bool Success, string DisabledMods, bool DisableAllMods)
        Set(Game game, string disabledMods, bool disableAllMods)
        {
            var fail = (false, "", false);

            try
            {
                CheckList.SuspendDrawing();

                ModsDisabledModsTextBox.Text = disabledMods;

                CheckList.ClearList();

                if (!GameSupportsMods(game)) return fail;

                (bool success, List<Mod> mods) = GameConfigFiles.GetGameMods(GameToGameIndex(game));

                if (!success) return fail;

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

                CheckList.FillList(checkItems, LText.ModsTab.ImportantModsCaution);

                return (true, disabledMods, disableAllMods);
            }
            finally
            {
                CheckList.ResumeDrawing();
            }
        }

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler? DisabledModsTextBoxTextChanged;

        [PublicAPI]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler? DisabledModsUpdated;

        private void EnableAllButton_Click(object sender, EventArgs e)
        {
            using (new DisableEvents(this))
            {
                foreach (Control control in CheckList.Controls)
                {
                    if (control is CheckBox checkBox)
                    {
                        checkBox.Checked = true;
                    }
                }
            }

            UpdateDisabledMods();
        }

        private void DisableNonImportantButton_Click(object sender, EventArgs e)
        {
            using (new DisableEvents(this))
            {
                foreach (Control control in CheckList.Controls)
                {
                    if (control is CheckBox checkBox && !DarkCheckList.IsControlCaution(checkBox))
                    {
                        checkBox.Checked = false;
                    }
                }
            }

            UpdateDisabledMods();
        }

        private void UpdateDisabledMods()
        {
            string disabledMods = "";

            foreach (DarkCheckList.CheckItem item in CheckList.CheckItems)
            {
                if (!item.Checked)
                {
                    if (!disabledMods.IsEmpty()) disabledMods += "+";
                    disabledMods += item.Text;
                }
            }

            using (new DisableEvents(this))
            {
                ModsDisabledModsTextBox.Text = disabledMods;
            }

            DisabledModsUpdated?.Invoke(CheckList, EventArgs.Empty);
        }

        private void ShowUberCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            CheckList.ShowCautionSection(ShowUberCheckBox.Checked);
        }

        private void DisabledModsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            DisabledModsTextBoxTextChanged?.Invoke(ModsDisabledModsTextBox, e);
        }

        private void DisabledModsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Commit();
            }
        }

        private void DisabledModsTextBox_Leave(object sender, EventArgs e)
        {
            Commit();
        }

        private void CheckList_ItemCheckedChanged(object sender, DarkCheckList.DarkCheckListEventArgs e)
        {
            if (EventsDisabled) return;
            UpdateDisabledMods();
        }
    }
}
