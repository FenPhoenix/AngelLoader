using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    internal static class Dialogs
    {
        public static bool AskToContinue(
            string message,
            string title,
            bool noIcon = false,
            DarkTaskDialog.Button defaultButton = DarkTaskDialog.Button.Yes)
        {
            if (Config.DarkMode)
            {
                using var d = new DarkTaskDialog(
                    message: message,
                    title: title,
                    icon: noIcon ? MessageBoxIcon.None : MessageBoxIcon.Warning,
                    yesText: LText.Global.Yes,
                    noText: LText.Global.No,
                    defaultButton: defaultButton);
                return d.ShowDialogDark() == DialogResult.Yes;
            }
            else
            {
                var mbDefaultButton = defaultButton switch
                {
                    DarkTaskDialog.Button.Yes => MessageBoxDefaultButton.Button1,
                    _ => MessageBoxDefaultButton.Button2
                };

                return MessageBox.Show(
                    message,
                    title,
                    MessageBoxButtons.YesNo,
                    noIcon ? MessageBoxIcon.None : MessageBoxIcon.Warning,
                    mbDefaultButton) == DialogResult.Yes;
            }
        }

        public static (bool Cancel, bool Continue, bool DontAskAgain)
        AskToContinueWithCancelCustomStrings(
            string message,
            string title,
            MessageBoxIcon icon,
            bool showDontAskAgain,
            string yes,
            string no,
            string cancel,
            DarkTaskDialog.Button defaultButton = DarkTaskDialog.Button.Yes)
        {
            using var d = new DarkTaskDialog(
                title: title,
                message: message,
                yesText: yes,
                noText: no,
                cancelText: cancel,
                defaultButton: defaultButton,
                checkBoxText: showDontAskAgain ? LText.AlertMessages.DontAskAgain : null,
                icon: icon);

            DialogResult result = d.ShowDialogDark();

            bool canceled = result == DialogResult.Cancel;
            bool cont = result == DialogResult.Yes;
            bool dontAskAgain = d.IsVerificationChecked;
            return (canceled, cont, dontAskAgain);
        }

        public static (bool Cancel, bool DontAskAgain)
        AskToContinueYesNoCustomStrings(
            string message,
            string title,
            MessageBoxIcon icon,
            bool showDontAskAgain,
            string? yes,
            string? no,
            DarkTaskDialog.Button defaultButton = DarkTaskDialog.Button.Yes)
        {
            using var d = new DarkTaskDialog(
                title: title,
                message: message,
                yesText: yes,
                noText: no,
                defaultButton: defaultButton,
                checkBoxText: showDontAskAgain ? LText.AlertMessages.DontAskAgain : null,
                icon: icon);

            DialogResult result = d.ShowDialogDark();

            bool cancel = result != DialogResult.Yes;
            bool dontAskAgain = d.IsVerificationChecked;
            return (cancel, dontAskAgain);
        }

        public static void ShowError(string message, IWin32Window owner, bool showScannerLogFile = false)
        {
            ShowError_Internal(message, owner, showScannerLogFile);
        }

        public static void ShowError(string message, bool showScannerLogFile = false)
        {
            ShowError_Internal(message, null, showScannerLogFile);
        }

        private static void ShowError_Internal(string message, IWin32Window? owner, bool showScannerLogFile)
        {
            string logFile = showScannerLogFile ? Paths.ScannerLogFile : Paths.LogFile;

            using var d = new DarkErrorDialog(message, logFile);
            if (owner != null)
            {
                d.ShowDialogDark(owner);
            }
            else
            {
                d.ShowDialogDark();
            }
        }

        public static void ShowAlert(string message, string title, MessageBoxIcon icon = MessageBoxIcon.Warning)
        {
            if (Config.DarkMode)
            {
                using var d = new DarkTaskDialog(
                    message: message,
                    title: title,
                    icon: icon,
                    yesText: LText.Global.OK,
                    defaultButton: DarkTaskDialog.Button.Yes);
                d.ShowDialogDark();
            }
            else
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
            }
        }
    }
}
