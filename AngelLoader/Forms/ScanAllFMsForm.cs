using System;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;
using FMScanner;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms;

public sealed partial class ScanAllFMsForm : DarkFormBase
{
    private readonly DarkCheckBox[] _checkBoxes;

    internal readonly ScanOptions ScanOptions = ScanOptions.FalseDefault();
    internal bool NoneSelected;

    public ScanAllFMsForm(bool selected)
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        _checkBoxes = new[]
        {
            TitleCheckBox,
            AuthorCheckBox,
            GameCheckBox,
            CustomResourcesCheckBox,
            SizeCheckBox,
            ReleaseDateCheckBox,
            TagsCheckBox,
            MissionCountCheckBox
        };

        if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

        Localize(selected);
    }

    public override void RespondToSystemThemeChange() => SetThemeBase(Config.VisualTheme);

    private void Localize(bool selected)
    {
        Text = selected ? LText.ScanAllFMsBox.TitleTextSelected : LText.ScanAllFMsBox.TitleText;

        ScanAllFMsForLabel.Text = selected ? LText.ScanAllFMsBox.ScanSelectedFMsFor : LText.ScanAllFMsBox.ScanAllFMsFor;
        TitleCheckBox.Text = LText.ScanAllFMsBox.Title;
        AuthorCheckBox.Text = LText.ScanAllFMsBox.Author;
        GameCheckBox.Text = LText.ScanAllFMsBox.Game;
        CustomResourcesCheckBox.Text = LText.ScanAllFMsBox.CustomResources;
        SizeCheckBox.Text = LText.ScanAllFMsBox.Size;
        ReleaseDateCheckBox.Text = LText.ScanAllFMsBox.ReleaseDate;
        TagsCheckBox.Text = LText.ScanAllFMsBox.Tags;
        MissionCountCheckBox.Text = LText.ScanAllFMsBox.MissionCount;

        SelectAllButton.Text = LText.Global.SelectAll;
        SelectNoneButton.Text = LText.Global.SelectNone;

        ScanButton.Text = LText.ScanAllFMsBox.Scan;
        Cancel_Button.Text = LText.Global.Cancel;
    }

    private void SelectAllButton_Click(object sender, EventArgs e) => SetCheckBoxValues(true);

    private void SelectNoneButton_Click(object sender, EventArgs e) => SetCheckBoxValues(false);

    private void SetCheckBoxValues(bool enabled)
    {
        foreach (DarkCheckBox cb in _checkBoxes)
        {
            cb.Checked = enabled;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            bool noneChecked = true;
            foreach (DarkCheckBox checkBox in _checkBoxes)
            {
                if (checkBox.Checked)
                {
                    noneChecked = false;
                    break;
                }
            }

            if (noneChecked)
            {
                NoneSelected = true;
            }
            else
            {
                ScanOptions.ScanTitle = TitleCheckBox.Checked;
                ScanOptions.ScanAuthor = AuthorCheckBox.Checked;
                ScanOptions.ScanGameType = GameCheckBox.Checked;
                ScanOptions.ScanCustomResources = CustomResourcesCheckBox.Checked;
                ScanOptions.ScanSize = SizeCheckBox.Checked;
                ScanOptions.ScanReleaseDate = ReleaseDateCheckBox.Checked;
                ScanOptions.ScanTags = TagsCheckBox.Checked;
                ScanOptions.ScanMissionCount = MissionCountCheckBox.Checked;
            }
        }

        base.OnFormClosing(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F1)
        {
            Core.OpenHelpFile(HelpSections.ScanAllFMs);
        }
        base.OnKeyDown(e);
    }
}
