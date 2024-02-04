using System;
using System.Windows.Forms;

namespace ReleasePackager;

public sealed partial class MainForm : Form
{
    public MainForm() => InitializeComponent();

    private void ReleaseNotesTextBox_TextChanged(object sender, EventArgs e) => Program.UpdateTexts();

    internal string GetRawReleaseNotesText() => ReleaseNotesTextBox.Text;

    internal string GetMarkdownReleaseNotes() => ReleaseNotesMarkdownRawTextBox.Text;

    internal void SetMarkdownText(string text) => ReleaseNotesMarkdownRawTextBox.Text = text;

    internal string GetTTLGReleaseNotes() => ReleaseNotesTTLGTextBox.Text;

    internal void SetTTLGText(string text) => ReleaseNotesTTLGTextBox.Text = text;

    private async void CreateReleaseButton_Click(object sender, EventArgs e) => await Program.CreateRelease();
}
