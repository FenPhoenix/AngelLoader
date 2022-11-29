﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls.LazyLoaded;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.LanguageSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class EditFMTabPage : Lazy_TabsBase
    {
        private Lazy_EditFMPage _page = null!;

        #region Lazy-loaded subcontrols

        private DynamicItemsLLMenu AltTitlesLLMenu = null!;
        private Lazy_LangDetectError Lazy_LangDetectError = null!;

        #endregion

        #region Event sending

        internal object? Sender_ScanForReadmes;
        internal object? Sender_ScanTitle;
        internal object? Sender_ScanAuthor;
        internal object? Sender_ScanReleaseDate;

        public event EventHandler? ScanForReadmesClick;
        public event EventHandler? ScanTitleClick;
        public event EventHandler? ScanAuthorClick;
        public event EventHandler? ScanReleaseDateClick;

        private void ScanForReadmesButton_Clicked(object sender, EventArgs e)
        {
            ScanForReadmesClick?.Invoke(Sender_ScanForReadmes, e);
        }

        private void ScanTitleButton_Clicked(object sender, EventArgs e)
        {
            ScanTitleClick?.Invoke(Sender_ScanTitle, e);
        }

        private void ScanAuthorButton_Clicked(object sender, EventArgs e)
        {
            ScanAuthorClick?.Invoke(Sender_ScanAuthor, e);
        }

        private void ScanReleaseDateButton_Clicked(object sender, EventArgs e)
        {
            ScanReleaseDateClick?.Invoke(Sender_ScanReleaseDate, e);
        }

        #endregion

        #region Theme

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool DarkModeEnabled
        {
            get => base.DarkModeEnabled;
            set
            {
                // Putting these up here just in case they also need to be for theming to work right
                if (_constructed)
                {
                    Lazy_LangDetectError.DarkModeEnabled = DarkModeEnabled;
                    AltTitlesLLMenu.DarkModeEnabled = DarkModeEnabled;
                }

                if (DarkModeEnabled == value) return;
                base.DarkModeEnabled = value;
            }
        }

        #endregion

        #region Public common

        public override void Construct()
        {
            if (_constructed) return;

            _page = new Lazy_EditFMPage
            {
                Dock = DockStyle.Fill,
                Tag = LoadType.Lazy,
                Visible = false
            };

            AltTitlesLLMenu = new DynamicItemsLLMenu(_owner);
            Lazy_LangDetectError = new Lazy_LangDetectError(_owner, _page);

            using (new DisableEvents(_owner))
            {
                Controls.Add(_page);

                Sender_ScanForReadmes = new object();
                _page.EditFMScanForReadmesButton.Click += ScanForReadmesButton_Clicked;
                ScanForReadmesClick += _owner.Async_EventHandler_Main;

                Sender_ScanTitle = new object();
                _page.EditFMScanTitleButton.Click += ScanTitleButton_Clicked;
                ScanTitleClick += _owner.Async_EventHandler_Main;

                Sender_ScanAuthor = new object();
                _page.EditFMScanAuthorButton.Click += ScanAuthorButton_Clicked;
                ScanAuthorClick += _owner.Async_EventHandler_Main;

                Sender_ScanReleaseDate = new object();
                _page.EditFMScanReleaseDateButton.Click += ScanReleaseDateButton_Clicked;
                ScanReleaseDateClick += _owner.Async_EventHandler_Main;

                _page.EditFMAltTitlesArrowButton.Click += EditFMAltTitlesArrowButton_Click;

                _page.EditFMTitleTextBox.TextChanged += EditFMTitleTextBox_TextChanged;
                _page.EditFMTitleTextBox.Leave += EditFMTitleTextBox_Leave;

                _page.EditFMAuthorTextBox.TextChanged += EditFMAuthorTextBox_TextChanged;
                _page.EditFMAuthorTextBox.Leave += EditFMAuthorTextBox_Leave;

                _page.EditFMReleaseDateCheckBox.CheckedChanged += EditFMReleaseDateCheckBox_CheckedChanged;
                _page.EditFMLastPlayedCheckBox.CheckedChanged += EditFMLastPlayedCheckBox_CheckedChanged;

                _page.EditFMReleaseDateDateTimePicker.ValueChanged += EditFMReleaseDateDateTimePicker_ValueChanged;
                _page.EditFMLastPlayedDateTimePicker.ValueChanged += EditFMLastPlayedDateTimePicker_ValueChanged;

                _page.EditFMRatingButton.Click += EditFMRatingButton_Click;

                _page.EditFMLanguageComboBox.AddFullItem(FMLanguages.DefaultLangKey, LText.EditFMTab.DefaultLanguage);
                _page.EditFMLanguageComboBox.SelectedIndex = 0;

                _page.EditFMLanguageComboBox.SelectedIndexChanged += EditFMLanguageComboBox_SelectedIndexChanged;

                _page.EditFMFinishedOnButton.Click += EditFMFinishedOnButton_Click;

                _page.EditFMScanLanguagesButton.Click += EditFMScanLanguagesButton_Click;

                _page.EditFMScanTitleButton.PaintCustom += _owner.ScanIconButtons_Paint;
                _page.EditFMScanAuthorButton.PaintCustom += _owner.ScanIconButtons_Paint;
                _page.EditFMScanReleaseDateButton.PaintCustom += _owner.ScanIconButtons_Paint;
                _page.EditFMScanLanguagesButton.PaintCustom += _owner.ScanIconButtons_Paint;
                _page.EditFMScanForReadmesButton.PaintCustom += _owner.ScanIconButtons_Paint;

                _constructed = true;

                UpdatePage();

                if (DarkModeEnabled) RefreshTheme();
            }

            Localize();

            _page.Show();
        }

        public override void Localize()
        {
            if (!_constructed) return;

            _page.EditFMTitleLabel.Text = LText.EditFMTab.Title;
            _page.EditFMAuthorLabel.Text = LText.EditFMTab.Author;
            _page.EditFMReleaseDateCheckBox.Text = LText.EditFMTab.ReleaseDate;
            _page.EditFMLastPlayedCheckBox.Text = LText.EditFMTab.LastPlayed;
            _page.EditFMRatingButton.Text = LText.EditFMTab.Rating;

            // For some reason this counts as a selected index change?!
            using (new DisableEvents(_owner))
            {
                _page.EditFMLanguageComboBox.Items[0] = LText.EditFMTab.DefaultLanguage;
            }

            _page.EditFMFinishedOnButton.Text = LText.EditFMTab.FinishedOn;

            _owner.MainToolTip.SetToolTip(_page.EditFMScanTitleButton, LText.EditFMTab.RescanTitleToolTip);
            _owner.MainToolTip.SetToolTip(_page.EditFMScanAuthorButton, LText.EditFMTab.RescanAuthorToolTip);
            _owner.MainToolTip.SetToolTip(_page.EditFMScanReleaseDateButton, LText.EditFMTab.RescanReleaseDateToolTip);
            _owner.MainToolTip.SetToolTip(_page.EditFMScanLanguagesButton, LText.EditFMTab.RescanLanguages);

            _page.EditFMLanguageLabel.Text = LText.EditFMTab.PlayFMInThisLanguage;
            Lazy_LangDetectError.Localize();

            _page.EditFMScanForReadmesButton.Text = LText.EditFMTab.RescanForReadmes;
        }

        public override void UpdatePage()
        {
            if (!_constructed) return;

            FanMission? fm = _owner.GetMainSelectedFMOrNull();

            if (fm != null)
            {
                bool fmIsT3 = fm.Game == Game.Thief3;

                void SetLanguageEnabledState()
                {
                    _page.EditFMLanguageLabel.Enabled = !fmIsT3;
                    _page.EditFMLanguageComboBox.Enabled = !fmIsT3;
                }

                // Adding/removing items from the combobox while disabled and in dark mode appears to be the
                // cause of the white flickering, so always make sure we're in an enabled state when setting
                // the items.
                if (fmIsT3)
                {
                    FillLanguagesListAndSelectLanguage();
                    SetLanguageEnabledState();
                }
                else
                {
                    SetLanguageEnabledState();
                    FillLanguagesListAndSelectLanguage();
                }

                foreach (Control c in _page.Controls)
                {
                    if (c != _page.EditFMLanguageLabel &&
                        c != _page.EditFMLanguageComboBox)
                    {
                        c.Enabled = true;
                    }
                }

                _page.EditFMScanTitleButton.Enabled = !fm.MarkedUnavailable;
                _page.EditFMScanAuthorButton.Enabled = !fm.MarkedUnavailable;
                _page.EditFMScanReleaseDateButton.Enabled = !fm.MarkedUnavailable;
                _page.EditFMScanLanguagesButton.Enabled = !fmIsT3 && !fm.MarkedUnavailable;
                _page.EditFMScanForReadmesButton.Enabled = !fm.MarkedUnavailable;

                _page.EditFMTitleTextBox.Text = fm.Title;

                // FM AltTitles is nominally always supposed to be non-empty (because the scan puts at least a
                // copy of the title in it), but it can be empty if all AltTitles lines for the FM have been
                // removed manually from the entry in the ini file. "Won't happen but could happen so we have to
                // handle it" scenario.
                _page.EditFMAltTitlesArrowButton.Enabled = fm.AltTitles.Count > 0;

                _page.EditFMAuthorTextBox.Text = fm.Author;

                _page.EditFMReleaseDateCheckBox.Checked = fm.ReleaseDate.DateTime != null;
                _page.EditFMReleaseDateDateTimePicker.Value = fm.ReleaseDate.DateTime ?? DateTime.Now;
                _page.EditFMReleaseDateDateTimePicker.Visible = fm.ReleaseDate.DateTime != null;

                _page.EditFMLastPlayedCheckBox.Checked = fm.LastPlayed.DateTime != null;
                _page.EditFMLastPlayedDateTimePicker.Value = fm.LastPlayed.DateTime ?? DateTime.Now;
                _page.EditFMLastPlayedDateTimePicker.Visible = fm.LastPlayed.DateTime != null;
            }
            else
            {
                // Always enable the combobox when modifying its items, to prevent the white flicker.
                // We'll disable it again in the disable-all-controls loop.
                _page.EditFMLanguageComboBox.Enabled = true;

                _page.EditFMLanguageComboBox.SelectedIndex = 0;

                Lazy_LangDetectError.SetVisible(false);

                foreach (Control c in _page.Controls)
                {
                    switch (c)
                    {
                        case TextBox tb:
                            tb.Text = "";
                            break;
                        case DateTimePicker dtp:
                            dtp.Value = DateTime.Now;
                            dtp.Hide();
                            break;
                        case CheckBox chk:
                            chk.Checked = false;
                            break;
                    }

                    c.Enabled = false;
                }

                _owner.FMsDGV_FM_LLMenu.ClearFinishedOnMenuItemChecks();
            }
        }

        #endregion

        #region Page

        #region Title

        private void EditFMTitleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            FanMission fm = _owner.FMsDGV.GetMainSelectedFM();
            fm.Title = _page.EditFMTitleTextBox.Text;
            _owner.RefreshMainSelectedFMRow_Fast();
        }

        private void EditFMTitleTextBox_Leave(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            Ini.WriteFullFMDataIni();
        }

        private void EditFMAltTitlesArrowButton_Click(object sender, EventArgs e)
        {
            List<string> fmAltTitles = _owner.FMsDGV.GetMainSelectedFM().AltTitles;
            if (fmAltTitles.Count == 0) return;

            var altTitlesMenuItems = new ToolStripItem[fmAltTitles.Count];
            for (int i = 0; i < fmAltTitles.Count; i++)
            {
                var item = new ToolStripMenuItemWithBackingText(fmAltTitles[i]);
                item.Click += EditFMAltTitlesMenuItems_Click;
                altTitlesMenuItems[i] = item;
            }

            AltTitlesLLMenu.ClearAndFillMenu(altTitlesMenuItems);

            ControlUtils.ShowMenu(
                AltTitlesLLMenu.Menu,
                _page.EditFMAltTitlesArrowButton,
                ControlUtils.MenuPos.BottomLeft);
        }

        private void EditFMAltTitlesMenuItems_Click(object sender, EventArgs e)
        {
            _page.EditFMTitleTextBox.Text = ((ToolStripMenuItemWithBackingText)sender).BackingText;
            Ini.WriteFullFMDataIni();
        }

        #endregion

        #region Author

        private void EditFMAuthorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            FanMission fm = _owner.FMsDGV.GetMainSelectedFM();
            fm.Author = _page.EditFMAuthorTextBox.Text;
            _owner.RefreshMainSelectedFMRow_Fast();
        }

        private void EditFMAuthorTextBox_Leave(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            Ini.WriteFullFMDataIni();
        }

        #endregion

        #region Release date

        private void EditFMReleaseDateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            _page.EditFMReleaseDateDateTimePicker.Visible = _page.EditFMReleaseDateCheckBox.Checked;

            FanMission fm = _owner.FMsDGV.GetMainSelectedFM();
            fm.ReleaseDate.DateTime = _page.EditFMReleaseDateCheckBox.Checked
                ? _page.EditFMReleaseDateDateTimePicker.Value
                : null;

            _owner.RefreshMainSelectedFMRow_Fast();
            Ini.WriteFullFMDataIni();
        }

        private void EditFMReleaseDateDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            FanMission fm = _owner.FMsDGV.GetMainSelectedFM();
            fm.ReleaseDate.DateTime = _page.EditFMReleaseDateDateTimePicker.Value;
            _owner.RefreshMainSelectedFMRow_Fast();
            Ini.WriteFullFMDataIni();
        }

        #endregion

        #region Last played

        private void EditFMLastPlayedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            _page.EditFMLastPlayedDateTimePicker.Visible = _page.EditFMLastPlayedCheckBox.Checked;

            FanMission fm = _owner.FMsDGV.GetMainSelectedFM();
            fm.LastPlayed.DateTime = _page.EditFMLastPlayedCheckBox.Checked
                ? _page.EditFMLastPlayedDateTimePicker.Value
                : null;

            _owner.RefreshMainSelectedFMRow_Fast();
            Ini.WriteFullFMDataIni();
        }

        private void EditFMLastPlayedDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            FanMission fm = _owner.FMsDGV.GetMainSelectedFM();
            fm.LastPlayed.DateTime = _page.EditFMLastPlayedDateTimePicker.Value;
            _owner.RefreshMainSelectedFMRow_Fast();
            Ini.WriteFullFMDataIni();
        }

        #endregion

        #region Rating

        private void EditFMRatingButton_Click(object sender, EventArgs e)
        {
            ControlUtils.ShowMenu(
                _owner.FMsDGV_FM_LLMenu.GetRatingMenu(),
                _page.EditFMRatingButton,
                ControlUtils.MenuPos.BottomRight,
                unstickMenu: true);
        }

        #endregion

        #region Finished on

        private void EditFMFinishedOnButton_Click(object sender, EventArgs e)
        {
            ControlUtils.ShowMenu(
                _owner.FMsDGV_FM_LLMenu.GetFinishedOnMenu(),
                _page.EditFMFinishedOnButton,
                ControlUtils.MenuPos.BottomRight,
                unstickMenu: true);
        }

        #endregion

        #region Languages

        private void EditFMLanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            FanMission? fm = _owner.GetMainSelectedFMOrNull();
            if (fm == null) return;

            fm.SelectedLang = GetMainSelectedLanguage();
            Ini.WriteFullFMDataIni();
        }

        private void EditFMScanLanguagesButton_Click(object sender, EventArgs e)
        {
            FanMission? fm = _owner.GetMainSelectedFMOrNull();
            if (fm == null) return;

            FMLanguages.FillFMSupportedLangs(fm);
            FillLanguagesListAndSelectLanguage();
            Ini.WriteFullFMDataIni();
        }

        #endregion

        #region Language methods

        private Language GetMainSelectedLanguage()
        {
            if (_page.EditFMLanguageComboBox.SelectedIndex == 0)
            {
                return Language.Default;
            }
            else
            {
                string backingItem = _page.EditFMLanguageComboBox.SelectedBackingItem();
                return LangStringsToEnums.TryGetValue(backingItem, out Language language) ? language : Language.Default;
            }
        }

        private void FillLanguagesListAndSelectLanguage()
        {
            using (new DisableEvents(_owner))
            {
                FanMission? fm = _owner.GetMainSelectedFMOrNull();
                if (fm == null) return;

                _page.EditFMLanguageComboBox.ClearAllBeyondFirstItem();

                if (GameIsDark(fm.Game))
                {
                    Lazy_LangDetectError.SetVisible(!fm.LangsScanned);

                    var langPairs = new List<(string InternalName, string TranslatedName)>(SupportedLanguageCount);

                    for (int i = 0; i < SupportedLanguageCount; i++)
                    {
                        LanguageIndex languageIndex = (LanguageIndex)i;
                        Language language = LanguageIndexToLanguage(languageIndex);
                        if (fm.Langs.HasFlagFast(language))
                        {
                            langPairs.Add((GetLanguageString(languageIndex), GetTranslatedLanguageName(languageIndex)));
                        }
                    }

                    try
                    {
                        _page.EditFMLanguageComboBox.BeginUpdate();

                        foreach (var (internalName, translatedName) in langPairs)
                        {
                            _page.EditFMLanguageComboBox.AddFullItem(internalName, translatedName);
                        }
                    }
                    finally
                    {
                        _page.EditFMLanguageComboBox.EndUpdate();
                    }

                    _page.EditFMLanguageComboBox.SelectedIndex = !fm.SelectedLang.ConvertsToKnown(out LanguageIndex langIndex)
                        ? 0
                        : _page.EditFMLanguageComboBox
                            .BackingItems
                            .FindIndex(x => x.EqualsI(GetLanguageString(langIndex)))
                            .ClampToZero();
                }
                else
                {
                    _page.EditFMLanguageComboBox.SelectedIndex = 0;
                    Lazy_LangDetectError.SetVisible(false);
                    fm.SelectedLang = Language.Default;
                }
            }
        }

        #endregion

        #endregion
    }
}
