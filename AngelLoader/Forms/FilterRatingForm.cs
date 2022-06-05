using System;
using System.Globalization;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    // TODO(Tool window filter forms): SystemInformation.ToolWindowCaptionButtonSize (for auto width based on title text)
    // We want to set width based on title bar text width, but ope, the %@#$ing measurement of the text is WRONG.
    // Visual measurements show the test long title to be 356px wide, but it measures as 328 with TextRenderer.MeasureText()
    // and 338.xxx with Graphics.MeasureString(). BOTH ARE #$@$%#$@!#ING WRONG AND USELESS. BOTH CAUSE CUT OFF
    // TEXT WHICH IS THE #$@$ING THING WE'RE TRYING TO PREVENT IN THE FIRST PLACE.
    public sealed partial class FilterRatingForm : DarkFormBase, IEventDisabler
    {
        public bool EventsDisabled { get; set; }

        internal int RatingFrom;
        internal int RatingTo;

        public FilterRatingForm(int ratingFrom, int ratingTo, bool outOfFive)
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif

            if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

            Localize();

            ControlUtils.DoFilterWindowButtonLayout(this, OKButton, Cancel_Button);

            FromComboBox.Items.Add(LText.Global.Unrated);
            for (int i = 0; i <= 10; i++) FromComboBox.Items.Add((outOfFive ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture));
            foreach (object item in FromComboBox.Items) ToComboBox.Items.Add(item);

            using (new DisableEvents(this))
            {
                FromComboBox.SelectedIndex = ratingFrom + 1;
                ToComboBox.SelectedIndex = ratingTo + 1;
            }
        }

        private void Localize()
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
                int fi = ToComboBox.SelectedIndex;
                int ti = FromComboBox.SelectedIndex;
                using (new DisableEvents(this))
                {
                    FromComboBox.SelectedIndex = fi;
                    ToComboBox.SelectedIndex = ti;
                }
            }
        }
    }
}
