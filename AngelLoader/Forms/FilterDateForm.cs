using System;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class FilterDateForm : DarkFormBase, IEventDisabler
    {
        private const int _minClientWidth = 170;

        private enum DateType { From, To }

        public bool EventsDisabled { get; set; }

        internal DateTime? DateFrom;
        internal DateTime? DateTo;

        public FilterDateForm(string title, DateTime? from, DateTime? to)
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif

            NoMinLabel.Location = FromDateTimePicker.Location;
            NoMinLabel.Size = FromDateTimePicker.Size;
            NoMinLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            NoMaxLabel.Location = ToDateTimePicker.Location;
            NoMaxLabel.Size = ToDateTimePicker.Size;
            NoMaxLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

            Localize();

            Text = title;

            int width = (OKButton.Width + Cancel_Button.Width + 24).Clamp(_minClientWidth, int.MaxValue);

            ClientSize = ClientSize with { Width = width };

            Cancel_Button.Location = Cancel_Button.Location with { X = OKButton.Right + 8 };

            using (new DisableEvents(this))
            {
                FromCheckBox.Checked = from != null;
                ToCheckBox.Checked = to != null;
            }

            ShowDate(DateType.From, from != null);
            ShowDate(DateType.To, to != null);

            if (from != null) FromDateTimePicker.Value = (DateTime)from;
            if (to != null) ToDateTimePicker.Value = (DateTime)to;
        }

        private void Localize()
        {
            FromLabel.Text = LText.DateFilterBox.From;
            ToLabel.Text = LText.DateFilterBox.To;
            NoMinLabel.Text = LText.DateFilterBox.NoMinimum;
            NoMaxLabel.Text = LText.DateFilterBox.NoMaximum;

            ResetButton.Text = LText.Global.Reset;
            OKButton.Text = LText.Global.OK;
            Cancel_Button.Text = LText.Global.Cancel;
        }

        private void ShowDate(DateType dateType, bool shown)
        {
            var label = dateType == DateType.From ? NoMinLabel : NoMaxLabel;
            var dtp = dateType == DateType.From ? FromDateTimePicker : ToDateTimePicker;

            if (shown)
            {
                label.Hide();
                dtp.Show();
            }
            else
            {
                label.Show();
                dtp.Hide();
            }
        }

        private void CheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var s = (CheckBox)sender;
            ShowDate(s == FromCheckBox ? DateType.From : DateType.To, s.Checked);
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            using (new DisableEvents(this))
            {
                FromCheckBox.Checked = false;
                ToCheckBox.Checked = false;
            }

            ShowDate(DateType.From, false);
            ShowDate(DateType.To, false);

            FromDateTimePicker.Value = DateTime.Now;
            ToDateTimePicker.Value = DateTime.Now;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            DateFrom = FromDateTimePicker.Visible ? FromDateTimePicker.Value : null;
            DateTo = ToDateTimePicker.Visible ? ToDateTimePicker.Value : null;
        }
    }
}
