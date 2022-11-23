using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.Forms.DarkFormBase;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms.CustomControls
{
    /*
    @vNext: Dark mode doesn't work quite right with these lazy-loaded tab pages here.
    Non-IDarkable controls keep their dark colors if we do the non-affected tab, switch to affected tab, then
    switch to light mode thing. We need to modify our theming system to fix it.

    Solution: use IDarkable controls always. Like DrawnPanel instead of Panel. Meh.
    */
    public sealed class StatsTabPage : DarkTabPageCustom
    {
        private MainForm _owner = null!;
        private Lazy_StatsPage? _statsPage;

        internal object? Sender_ScanCustomResources;

        public event EventHandler? ScanCustomResourcesClick;

        private readonly List<KeyValuePair<Control, ControlOriginalColors?>> _controlColors = new();

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool DarkModeEnabled
        {
            get => base.DarkModeEnabled;
            set
            {
                if (base.DarkModeEnabled == value) return;
                base.DarkModeEnabled = value;

                if (_statsPage == null) return;

                RefreshTheme();
            }
        }

        private void RefreshTheme()
        {
            SetTheme(this, _controlColors, base.DarkModeEnabled ? VisualTheme.Dark : VisualTheme.Classic);
        }

        public void Construct(MainForm owner)
        {
            _owner = owner;
            _statsPage = new Lazy_StatsPage
            {
                Dock = DockStyle.Fill,
                Tag = LoadType.Lazy
            };

            Controls.Add(_statsPage);

            Sender_ScanCustomResources = new object();
            _statsPage.StatsScanCustomResourcesButton.Click += ScanCustomResourcesButton_Clicked;
            _statsPage.StatsScanCustomResourcesButton.PaintCustom += _owner.ScanIconButtons_Paint;

            ScanCustomResourcesClick += _owner.Async_EventHandler_Main;

            UpdatePage();

            if (DarkModeEnabled) RefreshTheme();

            Localize();
        }

        private void ScanCustomResourcesButton_Clicked(object sender, EventArgs e)
        {
            ScanCustomResourcesClick?.Invoke(Sender_ScanCustomResources, e);
        }

        public void Localize()
        {
            if (_statsPage == null) return;
            FanMission? selFM = _owner.GetMainSelectedFMOrNull();

            _statsPage.Stats_MisCountLabel.Text = selFM != null
                ? MainForm.CreateMisCountLabelText(selFM)
                : LText.StatisticsTab.NoFMSelected;

            _statsPage.CustomResourcesLabel.Text =
                selFM == null ? LText.StatisticsTab.CustomResources :
                selFM.Game == Game.Thief3 ? LText.StatisticsTab.CustomResourcesNotSupportedForThief3 :
                selFM.ResourcesScanned ? LText.StatisticsTab.CustomResources :
                LText.StatisticsTab.CustomResourcesNotScanned;

            _statsPage.CR_MapCheckBox.Text = LText.StatisticsTab.Map;
            _statsPage.CR_AutomapCheckBox.Text = LText.StatisticsTab.Automap;
            _statsPage.CR_TexturesCheckBox.Text = LText.StatisticsTab.Textures;
            _statsPage.CR_SoundsCheckBox.Text = LText.StatisticsTab.Sounds;
            _statsPage.CR_MoviesCheckBox.Text = LText.StatisticsTab.Movies;
            _statsPage.CR_ObjectsCheckBox.Text = LText.StatisticsTab.Objects;
            _statsPage.CR_CreaturesCheckBox.Text = LText.StatisticsTab.Creatures;
            _statsPage.CR_MotionsCheckBox.Text = LText.StatisticsTab.Motions;
            _statsPage.CR_ScriptsCheckBox.Text = LText.StatisticsTab.Scripts;
            _statsPage.CR_SubtitlesCheckBox.Text = LText.StatisticsTab.Subtitles;

            _statsPage.StatsScanCustomResourcesButton.Text = LText.StatisticsTab.RescanStatistics;
        }

        public void UpdatePage()
        {
            if (_statsPage == null) return;
            FanMission? fm = _owner.GetMainSelectedFMOrNull();

            if (fm != null)
            {
                bool fmIsT3 = fm.Game == Game.Thief3;

                EnableStatsPanelLabels(_statsPage, true);

                _statsPage.Stats_MisCountLabel.Text = MainForm.CreateMisCountLabelText(fm);

                _statsPage.StatsScanCustomResourcesButton.Enabled = !fm.MarkedUnavailable;

                if (fmIsT3)
                {
                    BlankStatsPanelWithMessage(_statsPage, LText.StatisticsTab.CustomResourcesNotSupportedForThief3);
                }
                else if (!fm.ResourcesScanned)
                {
                    BlankStatsPanelWithMessage(_statsPage, LText.StatisticsTab.CustomResourcesNotScanned);
                }
                else
                {
                    _statsPage.CustomResourcesLabel.Text = LText.StatisticsTab.CustomResources;

                    _statsPage.CR_MapCheckBox.Checked = FMHasResource(fm, CustomResources.Map);
                    _statsPage.CR_AutomapCheckBox.Checked = FMHasResource(fm, CustomResources.Automap);
                    _statsPage.CR_ScriptsCheckBox.Checked = FMHasResource(fm, CustomResources.Scripts);
                    _statsPage.CR_TexturesCheckBox.Checked = FMHasResource(fm, CustomResources.Textures);
                    _statsPage.CR_SoundsCheckBox.Checked = FMHasResource(fm, CustomResources.Sounds);
                    _statsPage.CR_ObjectsCheckBox.Checked = FMHasResource(fm, CustomResources.Objects);
                    _statsPage.CR_CreaturesCheckBox.Checked = FMHasResource(fm, CustomResources.Creatures);
                    _statsPage.CR_MotionsCheckBox.Checked = FMHasResource(fm, CustomResources.Motions);
                    _statsPage.CR_MoviesCheckBox.Checked = FMHasResource(fm, CustomResources.Movies);
                    _statsPage.CR_SubtitlesCheckBox.Checked = FMHasResource(fm, CustomResources.Subtitles);

                    _statsPage.StatsCheckBoxesPanel.Enabled = true;
                }
            }
            else
            {
                _statsPage.Stats_MisCountLabel.Text = LText.StatisticsTab.NoFMSelected;

                BlankStatsPanelWithMessage(_statsPage, LText.StatisticsTab.CustomResources);
                _statsPage.StatsScanCustomResourcesButton.Enabled = false;

                EnableStatsPanelLabels(_statsPage, false);
            }
        }

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
    }
}
