using System;
using System.Drawing;
using System.Globalization;
using AL_Common;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    // TODO(Tool window filter forms): SystemInformation.ToolWindowCaptionButtonSize (for auto width based on title text)
    public sealed partial class FilterRatingForm : DarkFormBase, IEventDisabler
    {
        private const int _minClientWidth = 170;

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

            int width = (OKButton.Width + Cancel_Button.Width + 24).Clamp(_minClientWidth, int.MaxValue);

            ClientSize = new Size(width, ClientSize.Height);

            Cancel_Button.Location = new Point(OKButton.Right + 8, Cancel_Button.Location.Y);

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
