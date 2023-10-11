using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls;

public sealed class StatsTabPage : Lazy_TabsBase
{
    private Lazy_StatsPage _page = null!;

    #region Event sending

    private void ScanCustomResourcesButton_Clicked(object sender, EventArgs e)
    {
        _owner.Async_EventHandler_Main(ScanSender.CustomResources, e);
    }

    #endregion

    #region Public common

    public override void Construct()
    {
        if (_constructed) return;

        _page = ConstructPage<Lazy_StatsPage>();

        using (new DisableEvents(_owner))
        {
            Controls.Add(_page);

            _page.StatsScanCustomResourcesButton.PaintCustom += _owner.ScanIconButtons_Paint;

            _page.StatsScanCustomResourcesButton.Click += ScanCustomResourcesButton_Clicked;

            FinishConstruct();
        }

        _page.Show();
    }

    public override void Localize()
    {
        if (!_constructed) return;
        FanMission? selFM = _owner.GetMainSelectedFMOrNull();

        _page.Stats_MisCountLabel.Text = selFM != null
            ? Core.CreateMisCountMessageText(selFM.MisCount)
            : LText.StatisticsTab.NoFMSelected;

        _page.CustomResourcesLabel.Text =
            selFM == null ? LText.StatisticsTab.CustomResources :
            selFM.Game == Game.Thief3 ? LText.StatisticsTab.CustomResourcesNotSupportedForThief3 :
            selFM.Game == Game.TDM ? LText.StatisticsTab.CustomResourcesNotSupportedForTDM :
            selFM.ResourcesScanned ? LText.StatisticsTab.CustomResources :
            LText.StatisticsTab.CustomResourcesNotScanned;

        // IMPORTANT! Fragile numeric-indexed stuff, DO NOT change the order!
        _page._checkBoxes[0].Text = LText.StatisticsTab.Map;
        _page._checkBoxes[1].Text = LText.StatisticsTab.Automap;
        _page._checkBoxes[2].Text = LText.StatisticsTab.Scripts;
        _page._checkBoxes[3].Text = LText.StatisticsTab.Textures;
        _page._checkBoxes[4].Text = LText.StatisticsTab.Sounds;
        _page._checkBoxes[5].Text = LText.StatisticsTab.Objects;
        _page._checkBoxes[6].Text = LText.StatisticsTab.Creatures;
        _page._checkBoxes[7].Text = LText.StatisticsTab.Motions;
        _page._checkBoxes[8].Text = LText.StatisticsTab.Movies;
        _page._checkBoxes[9].Text = LText.StatisticsTab.Subtitles;

        _page.StatsScanCustomResourcesButton.Text = LText.StatisticsTab.RescanStatistics;
    }

    public override void UpdatePage()
    {
        if (!_constructed) return;
        FanMission? fm = _owner.GetMainSelectedFMOrNull();

        if (fm != null)
        {
            bool gameSupported = GameSupportsResourceDetection(fm.Game);

            EnableStatsPanelLabels(_page, true);

            _page.Stats_MisCountLabel.Text = Core.CreateMisCountMessageText(fm.MisCount);

            _page.StatsScanCustomResourcesButton.Enabled = !fm.MarkedUnavailable;

            if (!gameSupported)
            {
                BlankStatsPanelWithMessage(
                    _page,
                    fm.Game == Game.TDM
                        ? LText.StatisticsTab.CustomResourcesNotSupportedForTDM
                        : LText.StatisticsTab.CustomResourcesNotSupportedForThief3);
            }
            else if (!fm.ResourcesScanned)
            {
                BlankStatsPanelWithMessage(_page, LText.StatisticsTab.CustomResourcesNotScanned);
            }
            else
            {
                _page.CustomResourcesLabel.Text = LText.StatisticsTab.CustomResources;

                for (int i = 0, at = 1; i < Misc.CustomResourcesCount - 1; i++, at <<= 1)
                {
                    _page._checkBoxes[i].Checked = fm.HasResource((CustomResources)at);
                }

                _page.StatsCheckBoxesPanel.Enabled = true;
            }
        }
        else
        {
            _page.Stats_MisCountLabel.Text = LText.StatisticsTab.NoFMSelected;

            BlankStatsPanelWithMessage(_page, LText.StatisticsTab.CustomResources);
            _page.StatsScanCustomResourcesButton.Enabled = false;

            EnableStatsPanelLabels(_page, false);
        }
    }

    #endregion

    #region Page

    private static void BlankStatsPanelWithMessage(Lazy_StatsPage statsPage, string message)
    {
        statsPage.CustomResourcesLabel.Text = message;
        foreach (CheckBox cb in statsPage.StatsCheckBoxesPanel.Controls)
        {
            cb.Checked = false;
        }
        statsPage.StatsCheckBoxesPanel.Enabled = false;
    }

    private static void EnableStatsPanelLabels(Lazy_StatsPage statsPage, bool enabled)
    {
        foreach (Control control in statsPage.Controls)
        {
            if (control is DarkLabel label)
            {
                label.Enabled = enabled;
            }
        }
    }

    #endregion
}
