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

        private void OrigGameModsControl_DisabledModsTextBoxTextChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            DisabledMods = OrigGameModsControl.ModsDisabledModsTextBox.Text;
        }

        private void OrigGameModsControl_CheckListItemCheckedChanged(object sender, DarkCheckList.DarkCheckListEventArgs e)
        {
            if (EventsDisabled) return;
            UpdateDisabledMods();
        }

        private void OrigGameModsControl_AllEnabled(object sender, EventArgs e)
        {
            DisabledMods = "";
        }

        private void OrigGameModsControl_AllDisabled(object sender, EventArgs e)
        {
            UpdateDisabledMods();
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
