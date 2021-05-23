using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class FilterDateForm : DarkFormBase, IEventDisabler
    {
        private readonly List<KeyValuePair<Control, (Color ForeColor, Color BackColor)>> _controlColors = new();

        internal DateTime? DateFrom;
        internal DateTime? DateTo;

        private enum RDate { From, To }

        public bool EventsDisabled { get; set; }

        public FilterDateForm(string title, DateTime? from, DateTime? to)
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif

            if (Config.DarkMode) SetTheme(Config.VisualTheme);

            Localize();

            Text = title;

            using (new DisableEvents(this))
            {
                FromCheckBox.Checked = from != null;
                ToCheckBox.Checked = to != null;
            }

            ShowDate(RDate.From, from != null);
            ShowDate(RDate.To, to != null);

            if (from != null) FromDateTimePicker.Value = (DateTime)from;
            if (to != null) ToDateTimePicker.Value = (DateTime)to;
        }

        private void SetTheme(VisualTheme theme) => ControlUtils.ChangeFormThemeMode(theme, this, _controlColors);

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

        private void ShowDate(RDate rDate, bool shown)
        {
            var label = rDate == RDate.From ? NoMinLabel : NoMaxLabel;
            var dtp = rDate == RDate.From ? FromDateTimePicker : ToDateTimePicker;

            if (shown)
            {
                label.Hide();
                dtp.Show();
            }
            else
            {
                label.Location = dtp.Location;
                label.Size = dtp.Size;
                label.Show();
                dtp.Hide();
            }
        }

        private void CheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            var s = (CheckBox)sender;
            ShowDate(s == FromCheckBox ? RDate.From : RDate.To, s.Checked);
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            using (new DisableEvents(this))
            {
                FromCheckBox.Checked = false;
                ToCheckBox.Checked = false;
            }

            ShowDate(RDate.From, false);
            ShowDate(RDate.To, false);

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
