using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class FilterRatingForm : DarkFormBase, IEventDisabler
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int EventsDisabledCount { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool EventsDisabled => EventsDisabledCount > 0;

    internal int RatingFrom;
    internal int RatingTo;

    public FilterRatingForm(int ratingFrom, int ratingTo, bool outOfFive)
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

        Localize();

        ControlUtils.AutoSizeFilterWindow(this, OKButton, Cancel_Button);

        try
        {
            FromComboBox.BeginUpdate();
            ToComboBox.BeginUpdate();

            FromComboBox.Items.Add(LText.Global.Unrated);
            ToComboBox.Items.Add(LText.Global.Unrated);
            for (int i = 0; i <= 10; i++)
            {
                string item = (outOfFive ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture);
                FromComboBox.Items.Add(item);
                ToComboBox.Items.Add(item);
            }
        }
        finally
        {
            ToComboBox.EndUpdate();
            FromComboBox.EndUpdate();
        }

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

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            RatingFrom = FromComboBox.SelectedIndex - 1;
            RatingTo = ToComboBox.SelectedIndex - 1;
        }
        base.OnFormClosing(e);
    }
}
