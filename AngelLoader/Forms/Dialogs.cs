using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    internal static class Dialogs
    {
        // Auto-invoke everything in here for convenience. Any overhead introduced by this nonsense doesn't
        // matter for dialogs.

        #region Invoke nonsense

        private delegate void InvokeIfRequiredAction();
        private delegate object InvokeIfRequiredFunc();

        private static void InvokeIfViewExists(InvokeIfRequiredAction action)
        {
            if (Core.View != null! && Core.View.IsHandleCreated && Core.View.InvokeRequired)
            {
                Core.View.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private static object InvokeIfViewExists(InvokeIfRequiredFunc func)
        {
            return Core.View != null! && Core.View.IsHandleCreated && Core.View.InvokeRequired ? Core.View.Invoke(func) : func();
        }

        #endregion

        /// <summary>
        /// This method is auto-invoked if <see cref="Core.View"/> is able to be invoked to.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="noIcon"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public static bool
        AskToContinue(
            string message,
            string title,
            bool noIcon = false,
            DarkTaskDialog.Button defaultButton = DarkTaskDialog.Button.Yes) =>
            (bool)InvokeIfViewExists(() =>
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
            });

        /// <summary>
        /// This method is auto-invoked if <see cref="Core.View"/> is able to be invoked to.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="icon"></param>
        /// <param name="showDontAskAgain"></param>
        /// <param name="yes"></param>
        /// <param name="no"></param>
        /// <param name="cancel"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public static (bool Cancel, bool Continue, bool DontAskAgain)
        AskToContinueWithCancelCustomStrings(
            string message,
            string title,
            MessageBoxIcon icon,
            bool showDontAskAgain,
            string yes,
            string no,
            string cancel,
            DarkTaskDialog.Button defaultButton = DarkTaskDialog.Button.Yes) =>
            ((bool, bool, bool))InvokeIfViewExists(() =>
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
            });

        /// <summary>
        /// This method is auto-invoked if <see cref="Core.View"/> is able to be invoked to.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="icon"></param>
        /// <param name="showDontAskAgain"></param>
        /// <param name="yes"></param>
        /// <param name="no"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public static (bool Cancel, bool DontAskAgain)
        AskToContinueYesNoCustomStrings(
            string message,
            string title,
            MessageBoxIcon icon,
            bool showDontAskAgain,
            string? yes,
            string? no,
            DarkTaskDialog.Button defaultButton = DarkTaskDialog.Button.Yes) =>
            ((bool, bool))InvokeIfViewExists(() =>
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
            });

        /// <summary>
        /// This method is auto-invoked if <see cref="Core.View"/> is able to be invoked to.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="owner"></param>
        /// <param name="showScannerLogFile"></param>
        public static void ShowError(string message, IWin32Window owner, bool showScannerLogFile = false) =>
            InvokeIfViewExists(() => ShowError_Internal(message, owner, showScannerLogFile));

        /// <summary>
        /// This method is auto-invoked if <see cref="Core.View"/> is able to be invoked to.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="showScannerLogFile"></param>
        public static void ShowError(string message, bool showScannerLogFile = false) =>
            InvokeIfViewExists(() => ShowError_Internal(message, null, showScannerLogFile));

        // Private method, not invoked because all calls are
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

        /// <summary>
        /// This method is auto-invoked if <see cref="Core.View"/> is able to be invoked to.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="icon"></param>
        public static void ShowAlert(
            string message,
            string title,
            MessageBoxIcon icon = MessageBoxIcon.Warning) => InvokeIfViewExists(() =>
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
            });
    }
}
