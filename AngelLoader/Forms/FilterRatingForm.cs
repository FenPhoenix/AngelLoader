using System;
using System.Globalization;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;

namespace AngelLoader.Forms
{
    public partial class FilterRatingForm : Form, IEventDisabler, ILocalizable
    {
        internal int RatingFrom;
        internal int RatingTo;

        public bool EventsDisabled { get; set; }

        public FilterRatingForm(int ratingFrom, int ratingTo, bool outOfFive)
        {
            InitializeComponent();

            SetUITextToLocalized();

            FromComboBox.Items.Add(LText.Global.Unrated);
            for (int i = 0; i <= 10; i++) FromComboBox.Items.Add((outOfFive ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture));
            foreach (var item in FromComboBox.Items) ToComboBox.Items.Add(item);

            using (new DisableEvents(this))
            {
                FromComboBox.SelectedIndex = ratingFrom + 1;
                ToComboBox.SelectedIndex = ratingTo + 1;
            }
        }

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            Text = LText.RatingFilterBox.TitleText;
            FromLabel.Text = LText.RatingFilterBox.From;
            ToLabel.Text = LText.RatingFilterBox.To;

            ResetButton.Text = LText.Global.Reset;
            OKButton.Text = LText.Global.OK;
            Cancel_Button.Text = LText.Global.Cancel;
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            using (new DisableEvents(this))
            {
                FromComboBox.SelectedIndex = 0;
                ToComboBox.SelectedIndex = ToComboBox.Items.Count - 1;
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            RatingFrom = FromComboBox.SelectedIndex - 1;
            RatingTo = ToComboBox.SelectedIndex - 1;
        }

        private void ComboBoxes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            if (ToComboBox.SelectedIndex < FromComboBox.SelectedIndex)
            {
                var fi = ToComboBox.SelectedIndex;
                var ti = FromComboBox.SelectedIndex;
                using (new DisableEvents(this))
                {
                    FromComboBox.SelectedIndex = fi;
                    ToComboBox.SelectedIndex = ti;
                }
            }
        }
    }
}
