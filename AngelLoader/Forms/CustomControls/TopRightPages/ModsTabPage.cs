using System;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls;

public sealed class ModsTabPage : Lazy_TabsBase
{
    private Lazy_ModsPage _page = null!;

    #region Lazy-loaded subcontrols

    private DarkLabel ModsTabNotSupportedMessageLabel = null!;

    #endregion

    #region Public common

    public override void Construct()
    {
        if (_constructed) return;

        _page = ConstructPage<Lazy_ModsPage>();

        using (new DisableEvents(_owner))
        {
            Controls.Add(_page);

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

            FinishConstruct();
        }

        _page.Show();
    }

    public override void Localize()
    {
        if (!_constructed) return;

        _page.MainModsControl.Localize(LText.ModsTab.Header);
        _page.MainModsControl.RefreshCautionLabelText(LText.ModsTab.ImportantModsCaution);

        FanMission? fm = _owner.GetMainSelectedFMOrNull();
        UpdateNotSupportedMessage(fm);
    }

    private void UpdateNotSupportedMessage(FanMission? fm)
    {
        ModsTabNotSupportedMessageLabel.Text =
            fm?.Game == Game.TDM
                ? LText.ModsTab.TDM_ModsNotSupported
                : LText.ModsTab.Thief3_ModsNotSupported;
    }

    public override void UpdatePage()
    {
        if (!_constructed) return;

        FanMission? fm = _owner.GetMainSelectedFMOrNull();

        if (fm != null)
        {
            bool gameSupported = GameSupportsMods(fm.Game);

            _page.MainModsControl.Enabled = true;

            if (gameSupported)
            {
                _page.MainModsControl.Visible = true;
                ModsTabNotSupportedMessageLabel.Visible = false;

                _page.MainModsControl.Set(fm.Game, fm.DisabledMods);
            }
            else
            {
                _page.MainModsControl.Visible = false;
                ModsTabNotSupportedMessageLabel.Visible = true;

                _page.MainModsControl.SoftClearList();
            }
        }
        else
        {
            ModsTabNotSupportedMessageLabel.Visible = false;
            _page.MainModsControl.SoftClearList();
            _page.MainModsControl.Enabled = false;
            _page.MainModsControl.Visible = true;
        }

        UpdateNotSupportedMessage(fm);
    }

    #endregion

    #region Page

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
        if (_owner.EventsDisabled > 0 || !_owner.FMsDGV.RowSelected()) return;

        FanMission fm = _owner.FMsDGV.GetMainSelectedFM();
        fm.DisabledMods = _page.MainModsControl.DisabledModsTextBox.Text;
        _owner.RefreshSelectedRowCell(Column.DisabledMods);
        if (writeIni) Ini.WriteFullFMDataIni();
    }

    #endregion
}
