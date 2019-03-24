using System;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using FMScanner;

namespace AngelLoader.Forms
{
    public partial class ScanAllFMsForm : Form, ILocalizable
    {
        private readonly CheckBox[] CheckBoxes;

        internal ScanOptions ScanOptions = ScanOptions.FalseDefault();
        internal bool NoneSelected;

        public ScanAllFMsForm()
        {
            InitializeComponent();

            CheckBoxes = new[]
            {
                TitleCheckBox,
                AuthorCheckBox,
                GameCheckBox,
                CustomResourcesCheckBox,
                SizeCheckBox,
                ReleaseDateCheckBox,
                TagsCheckBox
            };

            SetUITextToLocalized();
        }

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            Text = LText.ScanAllFMsBox.TitleText;

            ScanAllFMsForLabel.Text = LText.ScanAllFMsBox.ScanAllFMsFor;
            TitleCheckBox.Text = LText.ScanAllFMsBox.Title;
            AuthorCheckBox.Text = LText.ScanAllFMsBox.Author;
            GameCheckBox.Text = LText.ScanAllFMsBox.Game;
            CustomResourcesCheckBox.Text = LText.ScanAllFMsBox.CustomResources;
            SizeCheckBox.Text = LText.ScanAllFMsBox.Size;
            ReleaseDateCheckBox.Text = LText.ScanAllFMsBox.ReleaseDate;
            TagsCheckBox.Text = LText.ScanAllFMsBox.Tags;

            SelectAllButton.SetTextAutoSize(LText.ScanAllFMsBox.SelectAll);
            SelectNoneButton.SetTextAutoSize(LText.ScanAllFMsBox.SelectNone);

            ScanButton.SetTextAutoSize(LText.ScanAllFMsBox.Scan);
            Cancel_Button.SetTextAutoSize(LText.Global.Cancel);
        }

        private void SelectAllButton_Click(object sender, EventArgs e) => SetCheckBoxValues(true);

        private void SelectNoneButton_Click(object sender, EventArgs e) => SetCheckBoxValues(false);

        private void SetCheckBoxValues(bool enabled)
        {
            foreach (var cb in CheckBoxes) cb.Checked = enabled;
        }

        private void ScanAllFMs_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK) return;

            if (CheckBoxes.All(x => !x.Checked))
            {
                NoneSelected = true;
            }
            else
            {
                ScanOptions.ScanTitle = TitleCheckBox.Checked;
                ScanOptions.ScanAuthor = AuthorCheckBox.Checked;
                ScanOptions.ScanGameType = GameCheckBox.Checked;
                ScanOptions.ScanCustomResources = CustomResourcesCheckBox.Checked;
                ScanOptions.ScanSize = SizeCheckBox.Checked;
                ScanOptions.ScanReleaseDate = ReleaseDateCheckBox.Checked;
                ScanOptions.ScanTags = TagsCheckBox.Checked;
            }
        }
    }
}
