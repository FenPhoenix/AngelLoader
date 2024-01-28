using System;
using System.Windows.Forms;

namespace Update;

/*
@Update: The plan:
-Ship this executable with AL distribution
-AL downloads the update and puts it in the temp folder, then calls this exe
-This exe then:
 -Waits for AL to close
 -Deletes its renamed exe if it exists
 -Renames its exe while running
 -Copies the update from the temp folder (including the updater exe from there, which will copy because we've
  renamed ourselves)
 -If successful, call AL, and close
 -AL will delete our renamed exe (possibly on next close, so it doesn't have to wait for us to close?)
 -If failed, rename our exe back to normal

@Update: Remove debug command line in properties!

@Update: Make close button safe - put up a warning, or revert on close?
*/

public sealed partial class MainForm : DarkFormBase
{
    public MainForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        Text = "AngelLoader Update";

        SetTheme(Data.VisualTheme);

        CopyProgressBarOutlinePanel.CenterHOnForm(this);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await Program.DoCopy();
        if (!Program._testMode)
        {
            Application.Exit();
        }
    }

    public void SetMessage(string message) => Invoke(() =>
    {
        CopyingLabel.Text = message;
        CopyingLabel.CenterHOnForm(this);
    });

    public void SetProgress(int percent) => Invoke(() => CopyingProgressBar.Value = percent);

    private void SetTheme(VisualTheme theme)
    {
        if (theme != VisualTheme.Dark)
        {
            ControlUtils.CreateAllControlsHandles(control: this);
            CopyProgressBarOutlinePanel.BorderStyle = BorderStyle.None;
        }
        else
        {
            SetThemeBase(theme: theme, createControlHandles: true);
            CopyProgressBarOutlinePanel.BorderStyle = BorderStyle.FixedSingle;
        }
    }
}
