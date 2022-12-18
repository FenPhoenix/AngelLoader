using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace FenGen.Forms;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

#pragma warning disable 1998
    private async void GenerateButton_Click(object sender, EventArgs e)
#pragma warning restore 1998
    {
        Core.ReadArgsAndDoTasks();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Environment.Exit(0);
    }
}