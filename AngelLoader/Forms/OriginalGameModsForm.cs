using System.Windows.Forms;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class OriginalGameModsForm : DarkFormBase
{
    public string DisabledMods;
    public bool? NewMantling;

    public OriginalGameModsForm(GameIndex gameIndex)
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        NewMantling = Config.GetNewMantling(gameIndex);
        DisabledMods = Config.GetDisabledMods(gameIndex);

        NewMantleCheckBox.SetFromNullableBool(NewMantling);
        OrigGameModsControl.DisabledModsTextBox.Text = DisabledMods;

        if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

        Localize(gameIndex);

        OrigGameModsControl.SetAndRecreateList(gameIndex, DisabledMods);
    }

    public override void RespondToSystemThemeChange() => SetThemeBase(Config.VisualTheme);

    private void Localize(GameIndex gameIndex)
    {
        Text = GetLocalizedGameName(gameIndex);

        NewMantleCheckBox.Text = LText.PatchTab.NewMantle;
        MainToolTip.SetToolTip(
            NewMantleCheckBox,
            LText.PatchTab.NewMantle_ToolTip_Checked + $"{NL}" +
            LText.PatchTab.NewMantle_ToolTip_Unchecked + $"{NL}" +
            LText.PatchTab.NewMantle_ToolTip_NotSet
        );

        OrigGameModsControl.Localize(GetLocalizedOriginalModHeaderText(gameIndex));
        OKButton.Text = LText.Global.OK;
        Cancel_Button.Text = LText.Global.Cancel;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            DisabledMods = OrigGameModsControl.DisabledModsTextBox.Text;
            NewMantling = NewMantleCheckBox.ToNullableBool();
        }
        base.OnFormClosing(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Images.DrawHorizDiv(e.Graphics, 7, OrigGameModsControl.Top - 20, ClientSize.Width - 9);
    }
}
