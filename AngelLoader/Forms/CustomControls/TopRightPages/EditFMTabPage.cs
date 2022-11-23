using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AngelLoader.Misc;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls.LazyLoaded;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class EditFMTabPage : Lazy_TabsBase
    {
        private Lazy_EditFMPage? _page;

        public void Construct(MainForm owner)
        {
            if (_page != null) return;

            _owner = owner;
            _page = new Lazy_EditFMPage
            {
                Dock = DockStyle.Fill,
                Tag = LoadType.Lazy
            };

            using (new DisableEvents(_owner))
            {
                Controls.Add(_page);

                //Sender_ScanCustomResources = new object();
                //_page.StatsScanCustomResourcesButton.Click += ScanCustomResourcesButton_Clicked;
                //_page.StatsScanCustomResourcesButton.PaintCustom += _owner.ScanIconButtons_Paint;

                //ScanCustomResourcesClick += _owner.Async_EventHandler_Main;

                _constructed = true;

                UpdatePage();

                if (DarkModeEnabled) RefreshTheme();

                Localize();
            }
        }

        public void Localize()
        {
            if (_page == null) return;

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
            _owner.Lazy_LangDetectError.Localize();

            _page.EditFMScanForReadmesButton.Text = LText.EditFMTab.RescanForReadmes;
        }

        public void UpdatePage()
        {
            if (_page == null) return;
            FanMission? fm = _owner.GetMainSelectedFMOrNull();

            if (fm != null)
            {

            }
            else
            {

            }
        }
    }
}
