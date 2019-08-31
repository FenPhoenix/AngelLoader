using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.Importing;

namespace AngelLoader.Forms.Import
{
    public partial class ImportFromMultipleLoadersForm : Form
    {
        private RadioButton[] TitleRBs;
        private RadioButton[] ReleaseDateRBs;
        private RadioButton[] LastPlayedRBs;
        private RadioButton[] FinishedRBs;
        private RadioButton[] CommentRBs;
        private RadioButton[] RatingRBs;
        private RadioButton[] DisabledModsRBs;
        private RadioButton[] TagsRBs;
        private RadioButton[] SelectedReadmeRBs;
        private RadioButton[] SizeRBs;
        private RadioButton[][] PriorityCheckSets;

        public ImportFromMultipleLoadersForm()
        {
            InitializeComponent();

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

            FMSel_ImportControls.Init(ImportType.FMSel);
            NDL_ImportControls.Init(ImportType.NewDarkLoader);
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
    }
}
