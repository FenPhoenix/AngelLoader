using System;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    [PublicAPI]
    public sealed class DarkErrorDialog : DarkTaskDialog
    {
        private readonly string _logFile;

        public DarkErrorDialog(string message, string logFile) :
            base(
                message: message,
                title: LText.AlertMessages.Error,
                icon: MessageBoxIcon.Error,
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
                Dialogs.ShowAlert(ErrorText.UnableToOpenLogFile + "\r\n\r\n" + _logFile, LText.AlertMessages.Error);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            YesButton.Focus();
        }
    }
}
