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

        OrigGameModsControl.SetErrorTextGetter(static () => LText.Global.ErrorReadingMods);

        NewMantling = Config.GetNewMantling(gameIndex);
        DisabledMods = Config.GetDisabledMods(gameIndex);

        NewMantleCheckBox.SetFromNullableBool(NewMantling);
        OrigGameModsControl.DisabledModsTextBox.Text = DisabledMods;

        if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

        Localize(gameIndex);

        OrigGameModsControl.Set(gameIndex, DisabledMods);
    }

    private void Localize(GameIndex gameIndex)
    {
        Text = GetLocalizedGameName(gameIndex);

        NewMantleCheckBox.Text = LText.PatchTab.NewMantle;
        MainToolTip.SetToolTip(
            NewMantleCheckBox,
            LText.PatchTab.NewMantle_ToolTip_Checked + "\r\n" +
            LText.PatchTab.NewMantle_ToolTip_Unchecked + "\r\n" +
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

    private void OriginalGameModsForm_Paint(object sender, PaintEventArgs e)
    {
        Images.DrawHorizDiv(e.Graphics, 7, 44, ClientSize.Width - 9);
    }
}
