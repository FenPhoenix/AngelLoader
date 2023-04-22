using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader.Forms.CustomControls;

public sealed class StatsTabPage : Lazy_TabsBase
{
    private Lazy_StatsPage _page = null!;

    #region Event sending

    internal object? Sender_ScanCustomResources;

    public event EventHandler? ScanCustomResourcesClick;

    private void ScanCustomResourcesButton_Clicked(object sender, EventArgs e)
    {
        ScanCustomResourcesClick?.Invoke(Sender_ScanCustomResources, e);
    }

    #endregion

    #region Public common

    public override void Construct()
    {
        if (_constructed) return;

        _page = new Lazy_StatsPage
        {
            Dock = DockStyle.Fill,
            Tag = LoadType.Lazy,
            Visible = false
        };

        using (new DisableEvents(_owner))
        {
            Controls.Add(_page);

            _page.StatsScanCustomResourcesButton.PaintCustom += _owner.ScanIconButtons_Paint;

            Sender_ScanCustomResources = new object();
            _page.StatsScanCustomResourcesButton.Click += ScanCustomResourcesButton_Clicked;
            ScanCustomResourcesClick += _owner.Async_EventHandler_Main;

            _constructed = true;

            UpdatePage();

            if (DarkModeEnabled) RefreshTheme();

            Localize();
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
            selFM.ResourcesScanned ? LText.StatisticsTab.CustomResources :
            LText.StatisticsTab.CustomResourcesNotScanned;

        _page.CR_MapCheckBox.Text = LText.StatisticsTab.Map;
        _page.CR_AutomapCheckBox.Text = LText.StatisticsTab.Automap;
        _page.CR_TexturesCheckBox.Text = LText.StatisticsTab.Textures;
        _page.CR_SoundsCheckBox.Text = LText.StatisticsTab.Sounds;
        _page.CR_MoviesCheckBox.Text = LText.StatisticsTab.Movies;
        _page.CR_ObjectsCheckBox.Text = LText.StatisticsTab.Objects;
        _page.CR_CreaturesCheckBox.Text = LText.StatisticsTab.Creatures;
        _page.CR_MotionsCheckBox.Text = LText.StatisticsTab.Motions;
        _page.CR_ScriptsCheckBox.Text = LText.StatisticsTab.Scripts;
        _page.CR_SubtitlesCheckBox.Text = LText.StatisticsTab.Subtitles;

        _page.StatsScanCustomResourcesButton.Text = LText.StatisticsTab.RescanStatistics;
    }

    public override void UpdatePage()
    {
        if (!_constructed) return;
        FanMission? fm = _owner.GetMainSelectedFMOrNull();

        if (fm != null)
        {
            bool fmIsT3 = fm.Game == Game.Thief3;

            EnableStatsPanelLabels(_page, true);

            _page.Stats_MisCountLabel.Text = Core.CreateMisCountMessageText(fm.MisCount);

            _page.StatsScanCustomResourcesButton.Enabled = !fm.MarkedUnavailable;

            if (fmIsT3)
            {
                BlankStatsPanelWithMessage(_page, LText.StatisticsTab.CustomResourcesNotSupportedForThief3);
            }
            else if (!fm.ResourcesScanned)
            {
                BlankStatsPanelWithMessage(_page, LText.StatisticsTab.CustomResourcesNotScanned);
            }
            else
            {
                _page.CustomResourcesLabel.Text = LText.StatisticsTab.CustomResources;

                _page.CR_MapCheckBox.Checked = FMHasResource(fm, CustomResources.Map);
                _page.CR_AutomapCheckBox.Checked = FMHasResource(fm, CustomResources.Automap);
                _page.CR_ScriptsCheckBox.Checked = FMHasResource(fm, CustomResources.Scripts);
                _page.CR_TexturesCheckBox.Checked = FMHasResource(fm, CustomResources.Textures);
                _page.CR_SoundsCheckBox.Checked = FMHasResource(fm, CustomResources.Sounds);
                _page.CR_ObjectsCheckBox.Checked = FMHasResource(fm, CustomResources.Objects);
                _page.CR_CreaturesCheckBox.Checked = FMHasResource(fm, CustomResources.Creatures);
                _page.CR_MotionsCheckBox.Checked = FMHasResource(fm, CustomResources.Motions);
                _page.CR_MoviesCheckBox.Checked = FMHasResource(fm, CustomResources.Movies);
                _page.CR_SubtitlesCheckBox.Checked = FMHasResource(fm, CustomResources.Subtitles);

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
