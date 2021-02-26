using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using FMScanner;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class ScanAllFMsForm : Form
    {
        private readonly DarkCheckBox[] _checkBoxes;

        private readonly Dictionary<Control, (Color ForeColor, Color BackColor)> _controlColors = new();

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

            SetTheme(Config.VisualTheme);

            Localize();
        }

        private void SetTheme(VisualTheme theme)
        {
            bool darkMode = theme == VisualTheme.Dark;

            if (_controlColors.Count == 0) ControlUtils.FillControlDict(this, _controlColors);

            #region Automatic sets

            foreach (var item in _controlColors)
            {
                Control control = item.Key;

                // TODO: @DarkMode(SetTheme excludes): We need to exclude lazy-loaded controls also.
                // Figure out some way to just say "if a control is part of a lazy-loaded class" so we don't
                // have to write them out manually here again and keep both places in sync.
                // Excludes - we handle these manually
                if (control is ScrollBarVisualOnly || control is SplitterPanel)
                {
                    continue;
                }

                // Separate if because a control could be IDarkable AND be a ToolStrip
                if (control is ToolStrip ts)
                {
                    foreach (ToolStripItem tsItem in ts.Items)
                    {
                        if (tsItem is IDarkable darkableTSItem)
                        {
                            darkableTSItem.DarkModeEnabled = darkMode;
                        }
                    }
                }

                if (control is IDarkable darkableControl)
                {
                    darkableControl.DarkModeEnabled = darkMode;
                }
                else
                {
                    if (darkMode)
                    {
                        control.ForeColor = DarkColors.LightText;
                        control.BackColor = DarkColors.Fen_ControlBackground;
                    }
                    else
                    {
                        control.ForeColor = item.Value.ForeColor;
                        control.BackColor = item.Value.BackColor;
                    }
                }
            }

            #endregion
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

            SelectAllButton.Text = LText.Global.SelectAll;
            SelectNoneButton.Text = LText.Global.SelectNone;

            ScanButton.Text = LText.ScanAllFMsBox.Scan;
            Cancel_Button.Text = LText.Global.Cancel;
        }

        private void SelectAllButton_Click(object sender, EventArgs e) => SetCheckBoxValues(true);

        private void SelectNoneButton_Click(object sender, EventArgs e) => SetCheckBoxValues(false);

        private void SetCheckBoxValues(bool enabled)
        {
            foreach (DarkCheckBox cb in _checkBoxes) cb.Checked = enabled;
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

        private void ScanAllFMsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                Core.OpenHelpFile(HelpSections.ScanAllFMs);
            }
        }
    }
}
