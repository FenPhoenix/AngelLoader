using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
    /*
    @TopLazy: Focus tab pages after construct, because a control in the page gets focus
    @TopLazy: ALL TABS: Use _constructed check instead of _page != null check, so _page doesn't need ! after it all the time
    @TopLazy: Test lazy-loaded lang error functionality
    @TopLazy: Mark any new business logic that's been moved back into the view (@VBL) for later review
    @TopLazy: Set tab indexes on all these when we're done
    @TopLazy: These tab pages may not need their explicit Size properties set in InitManual
    */

    public sealed class EditFMTabPage : Lazy_TabsBase
    {
        private Lazy_EditFMPage _page = null!;

        private DynamicItemsLLMenu AltTitlesLLMenu = null!;
        private Lazy_LangDetectError Lazy_LangDetectError = null!;

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

        public void Construct(MainForm owner)
        {
            if (_constructed) return;

            _owner = owner;
            _page = new Lazy_EditFMPage
            {
                Dock = DockStyle.Fill,
                Tag = LoadType.Lazy
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

                _page.EditFMRatingComboBox.SelectedIndexChanged += EditFMRatingComboBox_SelectedIndexChanged;

                _page.EditFMLanguageComboBox.SelectedIndexChanged += EditFMLanguageComboBox_SelectedIndexChanged;

                _page.EditFMFinishedOnButton.Click += EditFMFinishedOnButton_Click;

                _page.EditFMScanLanguagesButton.Click += EditFMScanLanguagesButton_Click;

                _page.EditFMScanTitleButton.PaintCustom += _owner.ScanIconButtons_Paint;
                _page.EditFMScanAuthorButton.PaintCustom += _owner.ScanIconButtons_Paint;
                _page.EditFMScanReleaseDateButton.PaintCustom += _owner.ScanIconButtons_Paint;
                _page.EditFMScanLanguagesButton.PaintCustom += _owner.ScanIconButtons_Paint;
                _page.EditFMScanForReadmesButton.PaintCustom += _owner.ScanIconButtons_Paint;

                _constructed = true;

                UpdateRatingStrings(Config.RatingDisplayStyle == RatingDisplayStyle.FMSel);

                UpdatePage();

                if (DarkModeEnabled) RefreshTheme();

            }

            Localize();
        }

        public void Localize()
        {
            if (!_constructed) return;

            _page.EditFMTitleLabel.Text = LText.EditFMTab.Title;
            _page.EditFMAuthorLabel.Text = LText.EditFMTab.Author;
            _page.EditFMReleaseDateCheckBox.Text = LText.EditFMTab.ReleaseDate;
            _page.EditFMLastPlayedCheckBox.Text = LText.EditFMTab.LastPlayed;
            _page.EditFMRatingLabel.Text = LText.EditFMTab.Rating;

            // For some reason this counts as a selected index change?!
            using (new DisableEvents(_owner))
            {
                _page.EditFMRatingComboBox.Items[0] = LText.Global.Unrated;
                if (_page.EditFMLanguageComboBox.Items.Count > 0 &&
                    _page.EditFMLanguageComboBox.BackingItems[0].EqualsI(FMLanguages.DefaultLangKey))
                {
                    _page.EditFMLanguageComboBox.Items[0] = LText.EditFMTab.DefaultLanguage;
                }
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

        public void UpdatePage()
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
                    if (_page.EditFMLanguageComboBox.Items.Count == 0)
                    {
                        AddLanguagesToList(new() { new(FMLanguages.DefaultLangKey, LText.EditFMTab.DefaultLanguage) });
                    }
                    SetSelectedLanguage(Language.Default);

                    SetLanguageEnabledState();
                }
                else
                {
                    SetLanguageEnabledState();
                    ScanAndFillLanguagesList(forceScan: false);
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

                UpdateRatingMenus(fm.Rating, disableEvents: false);
            }
            else
            {
                _page.EditFMRatingComboBox.SelectedIndex = 0;

                // Always enable the combobox when modifying its items, to prevent the white flicker.
                // We'll disable it again in the disable-all-controls loop.
                _page.EditFMLanguageComboBox.Enabled = true;

                _page.EditFMLanguageComboBox.ClearFullItems();
                _page.EditFMLanguageComboBox.AddFullItem(FMLanguages.DefaultLangKey, LText.EditFMTab.DefaultLanguage);
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

        #region Edit FM tab

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

            MainForm.ShowMenu(AltTitlesLLMenu.Menu, _page.EditFMAltTitlesArrowButton, MainForm.MenuPos.BottomLeft);
        }

        private void EditFMAltTitlesMenuItems_Click(object sender, EventArgs e)
        {
            _page.EditFMTitleTextBox.Text = ((ToolStripMenuItemWithBackingText)sender).BackingText;
            Ini.WriteFullFMDataIni();
        }

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

        private void EditFMRatingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            UpdateRatingForSelectedFMs(_page.EditFMRatingComboBox.SelectedIndex - 1);
        }

        internal void UpdateRatingForSelectedFMs(int rating, bool fromMenu = false)
        {
            // @TopLazy: Hack because we've mixed in control updating with fm updating, make this more elegant later
            if (fromMenu) Construct((MainForm)FindForm()!);

            FanMission fm = _owner.FMsDGV.GetMainSelectedFM();
            fm.Rating = rating;
            _owner.RefreshMainSelectedFMRow_Fast();

            UpdateRatingMenus(rating, disableEvents: true);

            FanMission[] sFMs = _owner.FMsDGV.GetSelectedFMs();
            if (sFMs.Length > 1)
            {
                foreach (FanMission sFM in sFMs)
                {
                    sFM.Rating = rating;
                }
                _owner.RefreshFMsListRowsOnlyKeepSelection();
            }
            Ini.WriteFullFMDataIni();
        }

        private void EditFMLanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_owner.EventsDisabled) return;
            UpdateFMSelectedLanguage();
        }

        private void EditFMFinishedOnButton_Click(object sender, EventArgs e)
        {
            MainForm.ShowMenu(_owner.FMsDGV_FM_LLMenu.GetFinishedOnMenu(), _page.EditFMFinishedOnButton, MainForm.MenuPos.BottomRight, unstickMenu: true);
        }

        private void EditFMScanLanguagesButton_Click(object sender, EventArgs e)
        {
            using (new DisableEvents(_owner))
            {
                ScanAndFillLanguagesList(forceScan: true);
            }
            Ini.WriteFullFMDataIni();
        }

        internal void AddLanguagesToList(List<KeyValuePair<string, string>> langPairs)
        {
            try
            {
                _page.EditFMLanguageComboBox.BeginUpdate();

                foreach (KeyValuePair<string, string> item in langPairs)
                {
                    _page.EditFMLanguageComboBox.AddFullItem(item.Key, item.Value);
                }
            }
            finally
            {
                _page.EditFMLanguageComboBox.EndUpdate();
            }
        }

        #endregion

        #region Languages

        public void ClearLanguagesList() => _page.EditFMLanguageComboBox.ClearFullItems();

        public Language GetMainSelectedLanguage()
        {
            if (_page.EditFMLanguageComboBox.SelectedIndex <= 0)
            {
                return Language.Default;
            }
            else
            {
                string backingItem = _page.EditFMLanguageComboBox.SelectedBackingItem();
                return LangStringsToEnums.TryGetValue(backingItem, out Language language) ? language : Language.Default;
            }
        }

        public Language SetSelectedLanguage(Language language)
        {
            if (_page.EditFMLanguageComboBox.Items.Count == 0)
            {
                return Language.Default;
            }
            else
            {
                _page.EditFMLanguageComboBox.SelectedIndex = !language.ConvertsToKnown(out LanguageIndex langIndex)
                    ? 0
                    : _page.EditFMLanguageComboBox
                        .BackingItems
                        .FindIndex(x => x.EqualsI(GetLanguageString(langIndex)))
                        .ClampToZero();

                return _page.EditFMLanguageComboBox.SelectedIndex > 0 &&
                       LangStringsToEnums.TryGetValue(_page.EditFMLanguageComboBox.SelectedBackingItem(), out Language returnLanguage)
                    ? returnLanguage
                    : Language.Default;
            }
        }

        internal void ScanAndFillLanguagesList(bool forceScan)
        {
            FanMission? fm = _owner.GetMainSelectedFMOrNull();
            if (fm == null) return;

            var langPairs = new List<KeyValuePair<string, string>>(SupportedLanguageCount + 1);

            ClearLanguagesList();

            langPairs.Add(new(FMLanguages.DefaultLangKey, LText.EditFMTab.DefaultLanguage));

            if (GameIsDark(fm.Game))
            {
                bool doScan = forceScan || !fm.LangsScanned;

                if (doScan)
                {
                    bool success = FMLanguages.FillFMSupportedLangs(fm);
                    ShowLanguageDetectError(!success);
                    Ini.WriteFullFMDataIni();
                }
                else
                {
                    ShowLanguageDetectError(false);
                }

                for (int i = 0; i < SupportedLanguageCount; i++)
                {
                    LanguageIndex index = (LanguageIndex)i;
                    Language language = LanguageIndexToLanguage(index);
                    if (fm.Langs.HasFlagFast(language))
                    {
                        string langStr = GetLanguageString(index);
                        langPairs.Add(new(langStr, GetTranslatedLanguageName(index)));
                    }
                }
            }
            else
            {
                ShowLanguageDetectError(false);
            }

            AddLanguagesToList(langPairs);

            fm.SelectedLang = SetSelectedLanguage(fm.SelectedLang);
        }

        internal void UpdateFMSelectedLanguage()
        {
            FanMission? fm = _owner.GetMainSelectedFMOrNull();
            if (fm == null) return;

            fm.SelectedLang = GetMainSelectedLanguage();
            Ini.WriteFullFMDataIni();
        }

        internal void UpdateRatingMenus(int rating, bool disableEvents = false)
        {
            if (!_constructed) return;

            using (disableEvents ? new DisableEvents(_owner) : null)
            {
                // @TopLazy: We need to do this regardless of construction! Pass _owner sooner than Construct()!
                _owner.FMsDGV_FM_LLMenu.SetRatingMenuItemChecked(rating);
                _page.EditFMRatingComboBox.SelectedIndex = rating + 1;
            }
        }

        public void ShowLanguageDetectError(bool enabled) => Lazy_LangDetectError.SetVisible(enabled);

        internal void UpdateRatingStrings(bool fmSelStyle)
        {
            if (!_constructed) return;

            // Just in case, since changing a ComboBox item's text counts as a selected index change maybe? Argh!
            using (new DisableEvents(_owner))
            {
                for (int i = 0; i <= 10; i++)
                {
                    _page.EditFMRatingComboBox.Items[i + 1] = (fmSelStyle ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture);
                }
            }
        }

        #endregion
    }
}
