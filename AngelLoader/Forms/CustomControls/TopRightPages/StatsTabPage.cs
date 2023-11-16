using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls;

public sealed class StatsTabPage : Lazy_TabsBase
{
    private Lazy_StatsPage _page = null!;

    #region Event sending

    private void ScanCustomResourcesButton_Clicked(object? sender, EventArgs e)
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

        UpdateCustomResourcesLabel(selFM);

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

    private void UpdateCustomResourcesLabel(FanMission? fm)
    {
        // @GENGAMES(Stats tab/UpdateCustomResourcesLabel())
        _page.CustomResourcesLabel.Text =
            fm == null ? LText.StatisticsTab.CustomResources :
            fm.Game == Game.Thief3 ? LText.StatisticsTab.CustomResourcesNotSupportedForThief3 :
            fm.Game == Game.TDM ? LText.StatisticsTab.CustomResourcesNotSupportedForTDM :
            fm.ResourcesScanned ? LText.StatisticsTab.CustomResources :
            LText.StatisticsTab.CustomResourcesNotScanned;
    }

    public override void UpdatePage()
    {
        if (!_constructed) return;
        FanMission? fm = _owner.GetMainSelectedFMOrNull();

        UpdateCustomResourcesLabel(fm);

        if (fm != null)
        {
            EnableStatsPanelLabels(_page, true);

            _page.Stats_MisCountLabel.Text = Core.CreateMisCountMessageText(fm.MisCount);

            _page.StatsScanCustomResourcesButton.Enabled = !fm.MarkedUnavailable;

            if (!GameSupportsResourceDetection(fm.Game) || !fm.ResourcesScanned)
            {
                BlankStatsPanel();
            }
            else
            {
                for (int i = 0, at = 1; i < CustomResourcesCount - 1; i++, at <<= 1)
                {
                    _page._checkBoxes[i].Checked = fm.HasResource((CustomResources)at);
                }

                _page.StatsCheckBoxesPanel.Enabled = true;
            }
        }
        else
        {
            _page.Stats_MisCountLabel.Text = LText.StatisticsTab.NoFMSelected;

            BlankStatsPanel();
            _page.StatsScanCustomResourcesButton.Enabled = false;

            EnableStatsPanelLabels(_page, false);
        }
    }

    #endregion

    #region Page

    private void BlankStatsPanel()
    {
        foreach (CheckBox cb in _page.StatsCheckBoxesPanel.Controls)
        {
            cb.Checked = false;
        }
        _page.StatsCheckBoxesPanel.Enabled = false;
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
