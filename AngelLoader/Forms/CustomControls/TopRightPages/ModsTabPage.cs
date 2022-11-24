using System;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class ModsTabPage : Lazy_TabsBase
    {
        private Lazy_ModsPage _page = null!;

        // @TopLazy: Test this with text and theme
        private DarkLabel ModsTabNotSupportedMessageLabel = null!;

        public void SetOwner(MainForm owner) => _owner = owner;

        public void Construct()
        {
            if (_constructed) return;

            _page = new Lazy_ModsPage
            {
                Dock = DockStyle.Fill,
                Tag = LoadType.Lazy
            };

            using (new DisableEvents(_owner))
            {
                Controls.Add(_page);

                _page.MainModsControl.SetErrorTextGetter(static () => LText.Global.ErrorReadingMods);

                ModsTabNotSupportedMessageLabel = new DarkLabel
                {
                    AutoSize = false,
                    DarkModeBackColor = DarkColors.Fen_ControlBackground,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Visible = false
                };

                _page.Controls.Add(ModsTabNotSupportedMessageLabel);
                ModsTabNotSupportedMessageLabel.BringToFront();

                _page.MainModsControl.DisabledModsTextBoxTextChanged += ModsDisabledModsTextBox_TextChanged;
                _page.MainModsControl.DisabledModsUpdated += Mods_DisabledModsUpdated;

                _constructed = true;

                UpdatePage();

                if (DarkModeEnabled) RefreshTheme();

                Localize();
            }
        }

        public void Localize()
        {
            if (!_constructed) return;

            _page.MainModsControl.Localize(LText.ModsTab.Header);
            _page.MainModsControl.CheckList.RefreshCautionLabelText(LText.ModsTab.ImportantModsCaution);

            ModsTabNotSupportedMessageLabel.Text = LText.ModsTab.Thief3_ModsNotSupported;
        }

        public void UpdatePage()
        {
            FanMission? fm = _owner.GetMainSelectedFMOrNull();

            if (fm != null)
            {
                bool fmIsT3 = fm.Game == Game.Thief3;

                if (_constructed)
                {
                    _page.MainModsControl.Enabled = true;

                    if (fmIsT3)
                    {
                        _page.MainModsControl.Visible = false;
                        ModsTabNotSupportedMessageLabel.Visible = true;

                        _page.MainModsControl.CheckList.ClearList();
                    }
                    else
                    {
                        _page.MainModsControl.Visible = true;
                        ModsTabNotSupportedMessageLabel.Visible = false;

                        (bool success, string disabledMods, bool disableAllMods) =
                            _page.MainModsControl.Set(fm.Game, fm.DisabledMods, fm.DisableAllMods);
                        if (success)
                        {
                            fm.DisabledMods = disabledMods;
                            fm.DisableAllMods = disableAllMods;
                        }
                    }
                }
                else
                {
                    // @TopLazy Another thing that runs duplicate if we're actually selected on startup
                    if (!fmIsT3)
                    {
                        (bool success, string disabledMods, bool disableAllMods) =
                            Core.CanonicalizeFMDisabledMods(fm.Game, fm.DisabledMods, fm.DisableAllMods);
                        if (success)
                        {
                            fm.DisabledMods = disabledMods;
                            fm.DisableAllMods = disableAllMods;
                        }
                    }
                }
            }
            else
            {
                if (!_constructed) return;

                ModsTabNotSupportedMessageLabel.Visible = false;
                _page.MainModsControl.CheckList.ClearList();
                _page.MainModsControl.Enabled = false;
                _page.MainModsControl.Visible = true;
            }
        }

        private void ModsDisabledModsTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateFMDisabledMods(writeIni: false);
        }

        private void Mods_DisabledModsUpdated(object sender, EventArgs e)
        {
            UpdateFMDisabledMods(writeIni: true);
        }

        private void UpdateFMDisabledMods(bool writeIni)
        {
            if (_owner.EventsDisabled || !_owner.FMsDGV.RowSelected()) return;

            FanMission fm = _owner.FMsDGV.GetMainSelectedFM();
            fm.DisabledMods = _page.MainModsControl.DisabledModsTextBox.Text;
            _owner.RefreshMainSelectedFMRow_Fast();
            if (writeIni) Ini.WriteFullFMDataIni();
        }
    }
}
