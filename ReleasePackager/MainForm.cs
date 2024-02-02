using System;
using System.Windows.Forms;

namespace ReleasePackager;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // Re-enable this stuff for final
#if false
        Package(Bitness.X64);
        Application.Exit();
#endif
    }

    private void ReleaseNotesTextBox_TextChanged(object sender, EventArgs e) => Program.UpdateTexts();

    internal string GetReleaseNotesText() => ReleaseNotesTextBox.Text;

    internal void SetMarkdownText(string text) => ReleaseNotesMarkdownRawTextBox.Text = text;

    internal void SetTTLGText(string text) => ReleaseNotesTTLGTextBox.Text = text;
}
