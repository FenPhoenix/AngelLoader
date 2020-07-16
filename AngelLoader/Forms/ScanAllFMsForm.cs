using System;
using System.Windows.Forms;
using FMScanner;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class ScanAllFMsForm : Form
    {
        // Explicit default widths because AutoSize is true and we don't set text in InitComponentManual(), which
        // would normally give them their default widths
        private const int _buttonDefaultWidth = 75;

        private readonly CheckBox[] _checkBoxes;

        internal readonly ScanOptions ScanOptions = ScanOptions.FalseDefault();
        internal bool NoneSelected;

        public ScanAllFMsForm()
        {
#if DEBUG
            InitializeComponent();
#else
            InitComponentManual();
#endif
            _checkBoxes = new[]
            {
                TitleCheckBox,
                AuthorCheckBox,
                GameCheckBox,
                CustomResourcesCheckBox,
                SizeCheckBox,
                ReleaseDateCheckBox,
                TagsCheckBox
            };

            Localize();
        }

        private void Localize()
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

            SelectAllButton.SetTextAutoSize(LText.Global.SelectAll);
            SelectNoneButton.SetTextAutoSize(LText.Global.SelectNone);

            ScanButton.SetTextAutoSize(LText.ScanAllFMsBox.Scan, _buttonDefaultWidth);
            Cancel_Button.SetTextAutoSize(LText.Global.Cancel, _buttonDefaultWidth);
        }

        private void SelectAllButton_Click(object sender, EventArgs e) => SetCheckBoxValues(true);

        private void SelectNoneButton_Click(object sender, EventArgs e) => SetCheckBoxValues(false);

        private void SetCheckBoxValues(bool enabled)
        {
            foreach (CheckBox cb in _checkBoxes) cb.Checked = enabled;
        }

        private void ScanAllFMs_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK) return;

            bool noneChecked = true;
            for (int i = 0; i < _checkBoxes.Length; i++)
            {
                if (_checkBoxes[i].Checked)
                {
                    noneChecked = false;
                    break;
                }
            }

            if (noneChecked)
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
