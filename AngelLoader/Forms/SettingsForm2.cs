using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.CustomControls.SettingsForm;
using System.IO;
using System.Text.RegularExpressions;
using AngelLoader.Common.Utility;
using AngelLoader.Properties;
using AngelLoader.WinAPI.Dialogs;
using static AngelLoader.Common.Logger;

namespace AngelLoader.Forms
{
    internal sealed partial class SettingsForm2 : Form, IEventDisabler, ILocalizable
    {
        private readonly ILocalizable OwnerForm;

        private readonly bool Startup;

        private readonly ConfigData InConfig;
        internal readonly ConfigData OutConfig = new ConfigData();

        private readonly TextBox[] GameExePathTextBoxes;

        private enum PathError
        {
            True,
            False
        }

        private enum PageIndex
        {
            Paths,
            FMDisplay,
            Other
        }

        private readonly UserControl[] Pages;

        // August 4 is chosen more-or-less randomly, but both its name and its number are different short vs. long
        // (Aug vs. August; 8 vs. 08), and the same thing with 4 (4 vs. 04).
        private readonly DateTime exampleDate = new DateTime(DateTime.Now.Year, 8, 4);

        public bool EventsDisabled { get; set; }

        private readonly PathsPage PathsPage = new PathsPage { Visible = false };
        private readonly FMDisplayPage FMDisplayPage = new FMDisplayPage { Visible = false };
        private readonly OtherPage OtherPage = new OtherPage { Visible = false };

        internal SettingsForm2(ILocalizable ownerForm, ConfigData config)
        {
            InitializeComponent();

            Pages = new UserControl[] { PathsPage, FMDisplayPage, OtherPage };

            OwnerForm = ownerForm;

            InConfig = config;

            Text = LText.SettingsWindow.TitleText;

            PagesListBox.SelectedIndex = (int)(
                InConfig.SettingsTab == SettingsTab.FMDisplay ? PageIndex.FMDisplay :
                InConfig.SettingsTab == SettingsTab.Other ? PageIndex.Other :
                PageIndex.Paths);

            // Language can change while the form is open, so store original sizes for later use as minimums
            OKButton.Tag = OKButton.Size;
            Cancel_Button.Tag = Cancel_Button.Size;

            SectionPanel.Controls.Add(PathsPage);
            SectionPanel.Controls.Add(FMDisplayPage);
            SectionPanel.Controls.Add(OtherPage);

            PathsPage.Dock = DockStyle.Fill;
            FMDisplayPage.Dock = DockStyle.Fill;
            OtherPage.Dock = DockStyle.Fill;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            #region Paths page

            PathsPage.Thief1ExePathTextBox.Text = InConfig.T1Exe;
            PathsPage.Thief2ExePathTextBox.Text = InConfig.T2Exe;
            PathsPage.Thief3ExePathTextBox.Text = InConfig.T3Exe;

            PathsPage.BackupPathTextBox.Text = InConfig.FMsBackupPath;

            PathsPage.FMArchivePathsListBox.Items.Clear();
            foreach (var path in InConfig.FMArchivePaths) PathsPage.FMArchivePathsListBox.Items.Add(path);

            PathsPage.IncludeSubfoldersCheckBox.Checked = InConfig.FMArchivePathsIncludeSubfolders;

            #endregion

            #region FM Display page

            #region Game organization

            switch (InConfig.GameOrganization)
            {
                case GameOrganization.ByTab:
                    FMDisplayPage.OrganizeGamesByTabRadioButton.Checked = true;
                    break;
                case GameOrganization.OneList:
                    FMDisplayPage.SortGamesInOneListRadioButton.Checked = true;
                    break;
            }

            #region Articles

            FMDisplayPage.EnableIgnoreArticlesCheckBox.Checked = InConfig.EnableArticles;

            for (var i = 0; i < InConfig.Articles.Count; i++)
            {
                var article = InConfig.Articles[i];
                if (i > 0) FMDisplayPage.ArticlesTextBox.Text += @", ";
                FMDisplayPage.ArticlesTextBox.Text += article;
            }

            FMDisplayPage.MoveArticlesToEndCheckBox.Checked = InConfig.MoveArticlesToEnd;

            #region Date format

            // NOTE: This section actually depends on the events in order to work. Also it appears to depend on
            // none of the date-related checkboxes being checked by default. Absolutely don't make any of them
            // checked by default!

            switch (InConfig.DateFormat)
            {
                case DateFormat.CurrentCultureShort:
                    FMDisplayPage.DateCurrentCultureShortRadioButton.Checked = true;
                    break;
                case DateFormat.CurrentCultureLong:
                    FMDisplayPage.DateCurrentCultureLongRadioButton.Checked = true;
                    break;
                case DateFormat.Custom:
                    FMDisplayPage.DateCustomRadioButton.Checked = true;
                    break;
            }

            object[] dateFormatList = { "", "d", "dd", "ddd", "dddd", "M", "MM", "MMM", "MMMM", "yy", "yyyy" };
            FMDisplayPage.Date1ComboBox.Items.AddRange(dateFormatList);
            FMDisplayPage.Date2ComboBox.Items.AddRange(dateFormatList);
            FMDisplayPage.Date3ComboBox.Items.AddRange(dateFormatList);
            FMDisplayPage.Date4ComboBox.Items.AddRange(dateFormatList);

            var d1 = InConfig.DateCustomFormat1;
            var s1 = InConfig.DateCustomSeparator1;
            var d2 = InConfig.DateCustomFormat2;
            var s2 = InConfig.DateCustomSeparator2;
            var d3 = InConfig.DateCustomFormat3;
            var s3 = InConfig.DateCustomSeparator3;
            var d4 = InConfig.DateCustomFormat4;

            FMDisplayPage.Date1ComboBox.SelectedItem = !d1.IsEmpty() && FMDisplayPage.Date1ComboBox.Items.Contains(d1) ? d1 : "dd";
            FMDisplayPage.DateSeparator1TextBox.Text = !s1.IsEmpty() ? s1 : "/";
            FMDisplayPage.Date2ComboBox.SelectedItem = !d2.IsEmpty() && FMDisplayPage.Date2ComboBox.Items.Contains(d2) ? d2 : "MM";
            FMDisplayPage.DateSeparator2TextBox.Text = !s2.IsEmpty() ? s2 : "/";
            FMDisplayPage.Date3ComboBox.SelectedItem = !d3.IsEmpty() && FMDisplayPage.Date3ComboBox.Items.Contains(d3) ? d3 : "yyyy";
            FMDisplayPage.DateSeparator3TextBox.Text = !s3.IsEmpty() ? s3 : "";
            FMDisplayPage.Date4ComboBox.SelectedItem = !d4.IsEmpty() && FMDisplayPage.Date4ComboBox.Items.Contains(d4) ? d4 : "";

            #endregion

            #region Rating display style

            using (new DisableEvents(this))
            {
                switch (InConfig.RatingDisplayStyle)
                {
                    case RatingDisplayStyle.NewDarkLoader:
                        FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked = true;
                        break;
                    case RatingDisplayStyle.FMSel:
                        FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Checked = true;
                        break;
                }

                FMDisplayPage.RatingUseStarsCheckBox.Checked = InConfig.RatingUseStars;

                FMDisplayPage.RatingExamplePictureBox.Image = FMDisplayPage.RatingNDLDisplayStyleRadioButton.Checked
                    ? Resources.RatingExample_NDL
                    : FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Checked && FMDisplayPage.RatingUseStarsCheckBox.Checked
                    ? Resources.RatingExample_FMSel_Stars
                    : Resources.RatingExample_FMSel_Number;

                FMDisplayPage.RatingUseStarsCheckBox.Enabled = FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Checked;
            }

            #endregion

            #endregion

            #endregion

            #region File conversion

            OtherPage.ConvertWAVsTo16BitOnInstallCheckBox.Checked = InConfig.ConvertWAVsTo16BitOnInstall;
            OtherPage.ConvertOGGsToWAVsOnInstallCheckBox.Checked = InConfig.ConvertOGGsToWAVsOnInstall;

            #endregion

            #endregion

            #region Other page

            #region Uninstalling FMs

            OtherPage.ConfirmUninstallCheckBox.Checked = InConfig.ConfirmUninstall;

            switch (InConfig.BackupFMData)
            {
                case BackupFMData.SavesAndScreensOnly:
                    OtherPage.BackupSavesAndScreensOnlyRadioButton.Checked = true;
                    break;
                case BackupFMData.AllChangedFiles:
                    OtherPage.BackupAllChangedDataRadioButton.Checked = true;
                    break;
            }

            OtherPage.BackupAlwaysAskCheckBox.Checked = InConfig.BackupAlwaysAsk;

            #endregion

            #region Languages

            using (new DisableEvents(this))
            {
                foreach (var item in InConfig.LanguageNames)
                {
                    OtherPage.LanguageComboBox.BackingItems.Add(item.Key);
                    OtherPage.LanguageComboBox.Items.Add(item.Value);
                }

                const string engLang = "English";

                if (OtherPage.LanguageComboBox.BackingItems.ContainsI(engLang))
                {
                    OtherPage.LanguageComboBox.BackingItems.Remove(engLang);
                    OtherPage.LanguageComboBox.BackingItems.Insert(0, engLang);
                    OtherPage.LanguageComboBox.Items.Remove(engLang);
                    OtherPage.LanguageComboBox.Items.Insert(0, engLang);
                }
                else
                {
                    OtherPage.LanguageComboBox.BackingItems.Insert(0, engLang);
                    OtherPage.LanguageComboBox.Items.Insert(0, engLang);
                }

                OtherPage.LanguageComboBox.SelectBackingIndexOf(OtherPage.LanguageComboBox.BackingItems.Contains(InConfig.Language)
                    ? InConfig.Language
                    : engLang);
            }

            #endregion

            OtherPage.WebSearchUrlTextBox.Text = InConfig.WebSearchUrl;

            OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Checked = InConfig.ConfirmPlayOnDCOrEnter;

            #region Show/hide UI elements

            OtherPage.HideUninstallButtonCheckBox.Checked = InConfig.HideUninstallButton;
            OtherPage.HideFMListZoomButtonsCheckBox.Checked = InConfig.HideFMListZoomButtons;

            #endregion

            #endregion

            SetUITextToLocalized();
        }

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            if (suspendResume) this.SuspendDrawing();
            try
            {
                OKButton.SetTextAutoSize(LText.Global.OK, ((Size)OKButton.Tag).Width);
                Cancel_Button.SetTextAutoSize(LText.Global.Cancel, ((Size)Cancel_Button.Tag).Width);

                #region Paths tab

                PagesListBox.Items[(int)PageIndex.Paths] = Startup
                    ? LText.SettingsWindow.InitialSettings_TabText
                    : LText.SettingsWindow.Paths_TabText;

                PathsPage.PathsToGameExesGroupBox.Text = LText.SettingsWindow.Paths_PathsToGameExes;
                PathsPage.Thief1ExePathLabel.Text = LText.SettingsWindow.Paths_Thief1;
                PathsPage.Thief2ExePathLabel.Text = LText.SettingsWindow.Paths_Thief2;
                PathsPage.Thief3ExePathLabel.Text = LText.SettingsWindow.Paths_Thief3;

                PathsPage.OtherGroupBox.Text = LText.SettingsWindow.Paths_Other;
                PathsPage.BackupPathLabel.Text = LText.SettingsWindow.Paths_BackupPath;

                // Manual "flow layout" for textbox/browse button combos
                for (int i = 0; i < 4; i++)
                {
                    var button =
                        i == 0 ? PathsPage.Thief1ExePathBrowseButton :
                        i == 1 ? PathsPage.Thief2ExePathBrowseButton :
                        i == 2 ? PathsPage.Thief3ExePathBrowseButton :
                        PathsPage.BackupPathBrowseButton;

                    var textBox =
                        i == 0 ? PathsPage.Thief1ExePathTextBox :
                        i == 1 ? PathsPage.Thief2ExePathTextBox :
                        i == 2 ? PathsPage.Thief3ExePathTextBox :
                        PathsPage.BackupPathTextBox;

                    button.SetTextAutoSize(textBox, LText.Global.BrowseEllipses);
                }

                PathsPage.GameRequirementsLabel.Text =
                    LText.SettingsWindow.Paths_Thief1AndThief2RequireNewDark + "\r\n" +
                    LText.SettingsWindow.Paths_Thief3RequiresSneakyUpgrade;

                PathsPage.FMArchivePathsGroupBox.Text = LText.SettingsWindow.Paths_FMArchivePaths;
                PathsPage.IncludeSubfoldersCheckBox.Text = LText.SettingsWindow.Paths_IncludeSubfolders;
                MainToolTip.SetToolTip(PathsPage.AddFMArchivePathButton, LText.SettingsWindow.Paths_AddArchivePathToolTip);
                MainToolTip.SetToolTip(PathsPage.RemoveFMArchivePathButton, LText.SettingsWindow.Paths_RemoveArchivePathToolTip);

                #endregion

                #region FM Display tab

                PagesListBox.Items[(int)PageIndex.FMDisplay] = LText.SettingsWindow.FMDisplay_TabText;

                FMDisplayPage.GameOrganizationGroupBox.Text = LText.SettingsWindow.FMDisplay_GameOrganization;
                FMDisplayPage.OrganizeGamesByTabRadioButton.Text = LText.SettingsWindow.FMDisplay_GameOrganizationByTab;
                FMDisplayPage.SortGamesInOneListRadioButton.Text = LText.SettingsWindow.FMDisplay_GameOrganizationOneList;

                FMDisplayPage.SortingGroupBox.Text = LText.SettingsWindow.FMDisplay_Sorting;
                FMDisplayPage.EnableIgnoreArticlesCheckBox.Text = LText.SettingsWindow.FMDisplay_IgnoreArticles;
                FMDisplayPage.MoveArticlesToEndCheckBox.Text = LText.SettingsWindow.FMDisplay_MoveArticlesToEnd;

                FMDisplayPage.RatingDisplayStyleGroupBox.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyle;
                FMDisplayPage.RatingNDLDisplayStyleRadioButton.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleNDL;
                FMDisplayPage.RatingFMSelDisplayStyleRadioButton.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleFMSel;
                FMDisplayPage.RatingUseStarsCheckBox.Text = LText.SettingsWindow.FMDisplay_RatingDisplayStyleUseStars;

                FMDisplayPage.DateFormatGroupBox.Text = LText.SettingsWindow.FMDisplay_DateFormat;
                FMDisplayPage.DateCurrentCultureShortRadioButton.Text = LText.SettingsWindow.FMDisplay_CurrentCultureShort;
                FMDisplayPage.DateCurrentCultureLongRadioButton.Text = LText.SettingsWindow.FMDisplay_CurrentCultureLong;
                FMDisplayPage.DateCustomRadioButton.Text = LText.SettingsWindow.FMDisplay_Custom;

                #endregion

                #region Other tab

                PagesListBox.Items[(int)PageIndex.Other] = LText.SettingsWindow.Other_TabText;

                OtherPage.FMFileConversionGroupBox.Text = LText.SettingsWindow.Other_FMFileConversion;
                OtherPage.ConvertWAVsTo16BitOnInstallCheckBox.Text = LText.SettingsWindow.Other_ConvertWAVsTo16BitOnInstall;
                OtherPage.ConvertOGGsToWAVsOnInstallCheckBox.Text = LText.SettingsWindow.Other_ConvertOGGsToWAVsOnInstall;

                OtherPage.UninstallingFMsGroupBox.Text = LText.SettingsWindow.Other_UninstallingFMs;
                OtherPage.ConfirmUninstallCheckBox.Text = LText.SettingsWindow.Other_ConfirmBeforeUninstalling;
                OtherPage.WhatToBackUpLabel.Text = LText.SettingsWindow.Other_WhenUninstallingBackUp;
                OtherPage.BackupSavesAndScreensOnlyRadioButton.Text = LText.SettingsWindow.Other_BackUpSavesAndScreenshotsOnly;
                OtherPage.BackupAllChangedDataRadioButton.Text = LText.SettingsWindow.Other_BackUpAllChangedFiles;
                OtherPage.BackupAlwaysAskCheckBox.Text = LText.SettingsWindow.Other_BackUpAlwaysAsk;

                OtherPage.LanguageGroupBox.Text = LText.SettingsWindow.Other_Language;

                OtherPage.WebSearchGroupBox.Text = LText.SettingsWindow.Other_WebSearch;
                OtherPage.WebSearchUrlLabel.Text = LText.SettingsWindow.Other_WebSearchURL;
                OtherPage.WebSearchTitleExplanationLabel.Text = LText.SettingsWindow.Other_WebSearchTitleVar;
                MainToolTip.SetToolTip(OtherPage.WebSearchUrlResetButton, LText.SettingsWindow.Other_WebSearchResetToolTip);

                OtherPage.PlayFMOnDCOrEnterGroupBox.Text = LText.SettingsWindow.Other_ConfirmPlayOnDCOrEnter;
                OtherPage.ConfirmPlayOnDCOrEnterCheckBox.Text = LText.SettingsWindow.Other_ConfirmPlayOnDCOrEnter_Ask;

                OtherPage.ShowOrHideUIElementsGroupBox.Text = LText.SettingsWindow.Other_ShowOrHideInterfaceElements;
                OtherPage.HideUninstallButtonCheckBox.Text = LText.SettingsWindow.Other_HideUninstallButton;
                OtherPage.HideFMListZoomButtonsCheckBox.Text = LText.SettingsWindow.Other_HideFMListZoomButtons;

                #endregion
            }
            finally
            {
                if (suspendResume) this.ResumeDrawing();
            }
        }

        private void PagesListBox_SelectedIndexChanged(object sender, EventArgs e) => SelectPage(PagesListBox.SelectedIndex);

        // This is to allow for selection to change immediately on mousedown. In that case the event will fire
        // again when you let up the mouse, but that's okay because a re-select is a visual no-op and the work
        // is basically nothing.
        private void PagesListBox_MouseDown(object sender, MouseEventArgs e) => SelectPage(PagesListBox.IndexFromPoint(e.Location));

        private void SelectPage(int index)
        {
            int pagesLength = Pages.Length;
            if (index < 0 || index > pagesLength - 1) return;

            Pages[index].Show();
            for (int i = 0; i < pagesLength; i++) if (i != index) Pages[i].Hide();
        }
    }
}
