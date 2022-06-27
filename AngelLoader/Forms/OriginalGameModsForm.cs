using System;
using System.Windows.Forms;
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

            OrigGameModsControl.Set(GameIndexToGame(gameIndex), DisabledMods, false);
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

        private void OrigGameModsControl_ModsCheckListItemCheckedChanged(object sender, DarkCheckList.DarkCheckListEventArgs e)
        {
            if (EventsDisabled) return;
            UpdateDisabledMods();
        }

        private void OrigGameModsControl_ModsEnableAllButtonClick(object sender, EventArgs e)
        {
            using (new DisableEvents(this))
            {
                foreach (Control control in OrigGameModsControl.CheckList.Controls)
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
                foreach (Control control in OrigGameModsControl.CheckList.Controls)
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

            foreach (DarkCheckList.CheckItem item in OrigGameModsControl.CheckList.CheckItems)
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
