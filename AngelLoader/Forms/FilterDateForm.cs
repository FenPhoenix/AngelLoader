using System;
using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class FilterDateForm : DarkFormBase, IEventDisabler
{
    private enum DateType { From, To }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int EventsDisabled { get; set; }

    internal DateTime? DateFrom;
    internal DateTime? DateTo;

    public FilterDateForm(string title, DateTime? from, DateTime? to)
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
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

        ControlUtils.AutoSizeFilterWindow(this, OKButton, Cancel_Button);

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

    public override void RespondToSystemThemeChange() => SetThemeBase(Config.VisualTheme);

    private void ShowDate(DateType dateType, bool shown)
    {
        DarkTextBox label = dateType == DateType.From ? NoMinLabel : NoMaxLabel;
        DarkDateTimePicker dtp = dateType == DateType.From ? FromDateTimePicker : ToDateTimePicker;

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
        if (EventsDisabled > 0) return;
        CheckBox s = (CheckBox)sender;
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

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            DateFrom = FromDateTimePicker.Visible ? FromDateTimePicker.Value : null;
            DateTo = ToDateTimePicker.Visible ? ToDateTimePicker.Value : null;
        }
        base.OnFormClosing(e);
    }
}
