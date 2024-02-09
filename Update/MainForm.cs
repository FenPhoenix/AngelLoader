//#define TESTING

using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Update;

public sealed partial class MainForm : DarkFormBase
{
    private bool _updateInProgress;
    private readonly AutoResetEvent _copyARE = new(false);

    public MainForm()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        Text = "AngelLoader Update";

        Message1Label.Text = "";
        Message2Label.Text = "";

        if (Data.VisualTheme != VisualTheme.Classic)
        {
            SetThemeBase(Data.VisualTheme);
        }

        CopyProgressBar.CenterHOnForm(this);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        try
        {
            _updateInProgress = true;
            try
            {
                await Program.DoCopy(_copyARE);
            }
            catch (OperationCanceledException)
            {
                _updateInProgress = false;
                Application.Exit();
                return;
            }
        }
        catch
        {
            // ignore - just in case
        }
        finally
        {
            _updateInProgress = false;
        }

#if TESTING
        if (!Program._testMode)
        {
            Application.Exit();
        }
        else
        {
#if true
            Utils.ShowDialogCustom(
                this,
                "Message dsf dsf dsf dsf sdf sd fsd",
                "Title",
                MessageBoxIcon.Warning,
                "Yes sda fdsaf dsaf dsa fdsa fsa",
                "No dsaf dsa fsa fdsa fasd",
                DialogResult.Yes);
#endif
            //using (System.Diagnostics.Process.Start(System.IO.Path.Combine(Application.StartupPath, "AngelLoader.exe"), "-after_update_cleanup")) { }
            //Application.Exit();
        }
#else
        Application.Exit();
#endif
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_updateInProgress)
        {
            e.Cancel = true;
            Program.CancelCopy();
        }

        base.OnClosing(e);
    }

    public void SetMessage1(string message) => Invoke(() =>
    {
        Message1Label.Text = message;
        Message1Label.CenterHOnForm(this);
    });

    public void SetMessage2(string message) => Invoke(() =>
    {
        Message2Label.Text = message;
        Message2Label.CenterHOnForm(this);
    });

    public void SetProgress(int percent) => Invoke(() => CopyProgressBar.Value = percent);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (Data.VisualTheme == VisualTheme.Dark)
        {
            Rectangle rect = new(
                CopyProgressBar.Location.X - 1,
                CopyProgressBar.Location.Y - 1,
                CopyProgressBar.Width + 1,
                CopyProgressBar.Height + 1);
            e.Graphics.DrawRectangle(DarkColors.GreySelectionPen, rect);
        }
    }
}
