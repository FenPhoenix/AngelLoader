using static AngelLoader.Global;

namespace AngelLoader.Forms;

public sealed partial class UpdateForm : DarkFormBase
{
    public UpdateForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        if (Config.DarkMode) SetThemeBase(Config.VisualTheme);

        Localize();
    }

    public override void RespondToSystemThemeChange() => SetThemeBase(Config.VisualTheme);

    // @Update: Localize this
    private void Localize()
    {
        Text = "Update available";

        UpdateButton.Text = "Update and restart";
        Cancel_Button.Text = LText.Global.Cancel;
    }

    public void SetReleaseNotes(string releaseNotes) => ReleaseNotesTextBox.Text = releaseNotes;
}
