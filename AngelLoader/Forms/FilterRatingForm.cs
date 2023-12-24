using System;
using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class FilterRatingForm : DarkFormBase, IEventDisabler
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int EventsDisabled { get; set; }

    internal int RatingFrom;
    internal int RatingTo;

    public FilterRatingForm(int ratingFrom, int ratingTo, RatingDisplayStyle style)
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

        Localize();

        ControlUtils.AutoSizeFilterWindow(this, OKButton, Cancel_Button);

        using (new UpdateRegion(FromComboBox))
        using (new UpdateRegion(ToComboBox))
        {
            FromComboBox.Items.Add(LText.Global.Unrated);
            ToComboBox.Items.Add(LText.Global.Unrated);
            for (int i = 0; i <= 10; i++)
            {
                string item = ControlUtils.GetRatingString(i, style);
                FromComboBox.Items.Add(item);
                ToComboBox.Items.Add(item);
            }
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

    public override void RespondToSystemThemeChange() => SetThemeBase(Config.VisualTheme);

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
        if (EventsDisabled > 0) return;
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
