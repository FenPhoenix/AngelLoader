﻿using System.Windows.Forms;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class OriginalGameModsForm : DarkFormBase
    {
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
    }
}
