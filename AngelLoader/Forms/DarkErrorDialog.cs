using System;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms;

[PublicAPI]
public sealed class DarkErrorDialog : DarkTaskDialog
{
    public DarkErrorDialog(
        string message,
        string? title = null,
        MessageBoxIcon icon = MessageBoxIcon.Error) :
        base(
            message: message,
            title: title ?? LText.AlertMessages.Error,
            icon: icon,
            yesText: LText.AlertMessages.Error_ViewLog,
            noText: LText.Global.OK,
            defaultButton: MBoxButton.Yes)
    {
        AcceptButton = NoButton;
        YesButton.DialogResult = DialogResult.None;
        NoButton.DialogResult = DialogResult.OK;

        YesButton.Click += YesButton_Click;
    }

    private void YesButton_Click(object? sender, EventArgs e) => Core.OpenLogFile();

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        YesButton.Focus();
    }
}
