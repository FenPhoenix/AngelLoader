using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms
{
    [PublicAPI]
    public sealed class DarkErrorDialog : DarkTaskDialog
    {
        private readonly string _logFile;

        public DarkErrorDialog(
            string message,
            string logFile,
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
            _logFile = logFile;

            AcceptButton = NoButton;
            YesButton.DialogResult = DialogResult.None;
            NoButton.DialogResult = DialogResult.OK;

            YesButton.Click += YesButton_Click;
        }

        private void YesButton_Click(object sender, EventArgs e)
        {
            try
            {
                ProcessStart_UseShellExecute(_logFile);
            }
            catch
            {
                Core.Dialogs.ShowAlert(ErrorText.UnOpenLogFile + "\r\n\r\n" + _logFile, LText.AlertMessages.Error);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            YesButton.Focus();
        }
    }
}
