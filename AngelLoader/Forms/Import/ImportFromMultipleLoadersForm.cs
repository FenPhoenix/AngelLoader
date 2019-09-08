using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Importing;

namespace AngelLoader.Forms.Import
{
    public partial class ImportFromMultipleLoadersForm : Form
    {
        private readonly RadioButton[] TitleRBs;
        private readonly RadioButton[] ReleaseDateRBs;
        private readonly RadioButton[] LastPlayedRBs;
        private readonly RadioButton[] FinishedRBs;
        private readonly RadioButton[] CommentRBs;
        private readonly RadioButton[] RatingRBs;
        private readonly RadioButton[] DisabledModsRBs;
        private readonly RadioButton[] TagsRBs;
        private readonly RadioButton[] SelectedReadmeRBs;
        private readonly RadioButton[] SizeRBs;
        private readonly RadioButton[][] PriorityCheckSets;

        internal readonly ImportList ImportPriorities;

        internal string DL_IniFile;

        internal readonly List<string> FMSelIniFiles;
        internal readonly List<string> NDLIniFiles;

        internal bool DL_ImportSaves;

        public ImportFromMultipleLoadersForm()
        {
            InitializeComponent();

            ImportPriorities = new ImportList();
            FMSelIniFiles = new List<string>();
            NDLIniFiles = new List<string>();

            TitleRBs = new[] { DL_Title_RadioButton, FMSel_Title_RadioButton, NDL_Title_RadioButton };
            ReleaseDateRBs = new[] { DL_ReleaseDate_RadioButton, FMSel_ReleaseDate_RadioButton, NDL_ReleaseDate_RadioButton };
            LastPlayedRBs = new[] { DL_LastPlayed_RadioButton, FMSel_LastPlayed_RadioButton, NDL_LastPlayed_RadioButton };
            FinishedRBs = new[] { DL_Finished_RadioButton, FMSel_Finished_RadioButton, NDL_Finished_RadioButton };
            CommentRBs = new[] { DL_Comment_RadioButton, FMSel_Comment_RadioButton, NDL_Comment_RadioButton };
            RatingRBs = new[] { FMSel_Rating_RadioButton, NDL_Rating_RadioButton };
            DisabledModsRBs = new[] { FMSel_DisabledMods_RadioButton, NDL_DisabledMods_RadioButton };
            TagsRBs = new[] { FMSel_Tags_RadioButton, NDL_Tags_RadioButton };
            SelectedReadmeRBs = new[] { FMSel_SelectedReadme_RadioButton, NDL_SelectedReadme_RadioButton };
            SizeRBs = new[] { DL_Size_RadioButton, NDL_Size_RadioButton };

            PriorityCheckSets = new[]
            {
                TitleRBs, ReleaseDateRBs, LastPlayedRBs, FinishedRBs, CommentRBs, RatingRBs, DisabledModsRBs,
                TagsRBs, SelectedReadmeRBs, SizeRBs
            };

            Localize();
            DL_ImportControls.Localize();
            FMSel_ImportControls.Init(ImportType.FMSel);
            NDL_ImportControls.Init(ImportType.NewDarkLoader);
        }

        private void Localize()
        {
            Text = LText.Importing.ImportFromMultipleLoaders_TitleText;

            DL_ImportSavesCheckBox.Text = LText.Importing.DarkLoader_ImportSaves;

            FMDataToImportLabel.Text = LText.Importing.FMDataToImport;
            
            ImportTitleCheckBox.Text = LText.Importing.ImportData_Title;
            ImportReleaseDateCheckBox.Text = LText.Importing.ImportData_ReleaseDate;
            ImportLastPlayedCheckBox.Text = LText.Importing.ImportData_LastPlayed;
            ImportFinishedOnCheckBox.Text = LText.Importing.ImportData_Finished;
            ImportCommentCheckBox.Text = LText.Importing.ImportData_Comment;
            ImportRatingCheckBox.Text = LText.Importing.ImportData_Rating;
            ImportDisabledModsCheckBox.Text = LText.Importing.ImportData_DisabledMods;
            ImportTagsCheckBox.Text = LText.Importing.ImportData_Tags;
            ImportSelectedReadmeCheckBox.Text = LText.Importing.ImportData_SelectedReadme;
            ImportSizeCheckBox.Text = LText.Importing.ImportData_Size;

            OKButton.SetTextAutoSize(LText.Global.OK, OKButton.Width);
            Cancel_Button.SetTextAutoSize(LText.Global.Cancel, Cancel_Button.Width);
        }

        private void ImportCheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = ((CheckBox)sender).Checked;

            void SetEnabled(RadioButton[] set)
            {
                foreach (var item in set) item.Enabled = isChecked;
            }

            SetEnabled(
                sender == ImportTitleCheckBox ? TitleRBs :
                sender == ImportReleaseDateCheckBox ? ReleaseDateRBs :
                sender == ImportLastPlayedCheckBox ? LastPlayedRBs :
                sender == ImportFinishedOnCheckBox ? FinishedRBs :
                sender == ImportCommentCheckBox ? CommentRBs :
                sender == ImportRatingCheckBox ? RatingRBs :
                sender == ImportDisabledModsCheckBox ? DisabledModsRBs :
                sender == ImportTagsCheckBox ? TagsRBs :
                sender == ImportSelectedReadmeCheckBox ? SelectedReadmeRBs :
                SizeRBs);
        }

        // We want to have all radio buttons in a row be part of the same group but those in different columns be
        // in different groups, but the default behavior is to have every one be part of the same group. So do
        // this manual stuff to achieve what we want instead.
        private void Priority_RadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            var s = (RadioButton)sender;

            if (!s.Checked) return;

            foreach (var set in PriorityCheckSets)
            {
                if (set.Contains(s))
                {
                    foreach (var rb in set) if (rb != s) rb.Checked = false;
                }
            }
        }

        private void ImportFromMultipleLoadersForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK) return;

            #region Set DL ini files

            DL_IniFile = DL_ImportControls.DarkLoaderIniText;
            DL_ImportSaves = DL_ImportSavesCheckBox.Checked;

            #endregion

            #region Set FMSel ini files

            if (!FMSel_ImportControls.Thief1IniFile.IsWhiteSpace())
            {
                FMSelIniFiles.Add(FMSel_ImportControls.Thief1IniFile);
            }
            if (!FMSel_ImportControls.Thief2IniFile.IsWhiteSpace())
            {
                FMSelIniFiles.Add(FMSel_ImportControls.Thief2IniFile);
            }
            if (!FMSel_ImportControls.Thief3IniFile.IsWhiteSpace())
            {
                FMSelIniFiles.Add(FMSel_ImportControls.Thief3IniFile);
            }

            #endregion

            #region Set NDL ini files

            if (!NDL_ImportControls.Thief1IniFile.IsWhiteSpace())
            {
                NDLIniFiles.Add(NDL_ImportControls.Thief1IniFile);
            }
            if (!NDL_ImportControls.Thief2IniFile.IsWhiteSpace())
            {
                NDLIniFiles.Add(NDL_ImportControls.Thief2IniFile);
            }
            if (!NDL_ImportControls.Thief3IniFile.IsWhiteSpace())
            {
                NDLIniFiles.Add(NDL_ImportControls.Thief3IniFile);
            }

            #endregion

            #region Set import priorities

            ImportPriorities.Title =
                !ImportTitleCheckBox.Checked ? ImportPriority.NoImport :
                DL_Title_RadioButton.Checked ? ImportPriority.DarkLoader :
                FMSel_Title_RadioButton.Checked ? ImportPriority.FMSel :
                ImportPriority.NewDarkLoader;
            ImportPriorities.ReleaseDate =
                !ImportReleaseDateCheckBox.Checked ? ImportPriority.NoImport :
                DL_ReleaseDate_RadioButton.Checked ? ImportPriority.DarkLoader :
                FMSel_ReleaseDate_RadioButton.Checked ? ImportPriority.FMSel :
                ImportPriority.NewDarkLoader;
            ImportPriorities.LastPlayed =
                !ImportLastPlayedCheckBox.Checked ? ImportPriority.NoImport :
                DL_LastPlayed_RadioButton.Checked ? ImportPriority.DarkLoader :
                FMSel_LastPlayed_RadioButton.Checked ? ImportPriority.FMSel :
                ImportPriority.NewDarkLoader;
            ImportPriorities.FinishedOn =
                !ImportFinishedOnCheckBox.Checked ? ImportPriority.NoImport :
                DL_Finished_RadioButton.Checked ? ImportPriority.DarkLoader :
                FMSel_Finished_RadioButton.Checked ? ImportPriority.FMSel :
                ImportPriority.NewDarkLoader;
            ImportPriorities.Comment =
                !ImportCommentCheckBox.Checked ? ImportPriority.NoImport :
                DL_Comment_RadioButton.Checked ? ImportPriority.DarkLoader :
                FMSel_Comment_RadioButton.Checked ? ImportPriority.FMSel :
                ImportPriority.NewDarkLoader;
            ImportPriorities.Rating =
                !ImportRatingCheckBox.Checked ? ImportPriority.NoImport :
                FMSel_Rating_RadioButton.Checked ? ImportPriority.FMSel :
                ImportPriority.NewDarkLoader;
            ImportPriorities.DisabledMods =
                !ImportDisabledModsCheckBox.Checked ? ImportPriority.NoImport :
                FMSel_DisabledMods_RadioButton.Checked ? ImportPriority.FMSel :
                ImportPriority.NewDarkLoader;
            ImportPriorities.Tags =
                !ImportTagsCheckBox.Checked ? ImportPriority.NoImport :
                FMSel_Tags_RadioButton.Checked ? ImportPriority.FMSel :
                ImportPriority.NewDarkLoader;
            ImportPriorities.SelectedReadme =
                !ImportSelectedReadmeCheckBox.Checked ? ImportPriority.NoImport :
                FMSel_SelectedReadme_RadioButton.Checked ? ImportPriority.FMSel :
                ImportPriority.NewDarkLoader;
            ImportPriorities.Size =
                !ImportSizeCheckBox.Checked ? ImportPriority.NoImport :
                DL_Size_RadioButton.Checked ? ImportPriority.DarkLoader :
                ImportPriority.NewDarkLoader;

            #endregion
        }
    }
}
