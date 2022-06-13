using System.Windows.Forms;
using static AngelLoader.Misc;
using static AngelLoader.Forms.ControlUtils;

namespace AngelLoader.Forms
{
    internal sealed class Dialogs : IDialogs
    {
        // Auto-invoke everything in here for convenience. Any overhead introduced by this nonsense doesn't
        // matter for dialogs.

        #region Invoke nonsense

        private delegate void InvokeIfRequiredAction();
        private delegate object InvokeIfRequiredFunc();

        private static void InvokeIfViewExists(InvokeIfRequiredAction action)
        {
            if (FormsViewEnvironment.ViewCreated &&
                FormsViewEnvironment.ViewInternal.IsHandleCreated &&
                FormsViewEnvironment.ViewInternal.InvokeRequired)
            {
                FormsViewEnvironment.ViewInternal.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private static object InvokeIfViewExists(InvokeIfRequiredFunc func)
        {
            return FormsViewEnvironment.ViewCreated &&
                   FormsViewEnvironment.ViewInternal.IsHandleCreated &&
                   FormsViewEnvironment.ViewInternal.InvokeRequired
                ? FormsViewEnvironment.ViewInternal.Invoke(func)
                : func();
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
        public bool
        AskToContinue(
            string message,
            string title,
            bool noIcon = false,
            MBoxButton defaultButton = MBoxButton.Yes) =>
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
                        MBoxButton.Yes => MessageBoxDefaultButton.Button1,
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
        /// <param name="yes"></param>
        /// <param name="no"></param>
        /// <param name="cancel"></param>
        /// <param name="checkBoxText"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public (bool Cancel, bool Continue, bool CheckBoxChecked)
        AskToContinueWithCancelCustomStrings(
            string message,
            string title,
            MBoxIcon icon,
            string? yes,
            string? no,
            string? cancel,
            string? checkBoxText = null,
            MBoxButton defaultButton = MBoxButton.Yes) =>
            ((bool, bool, bool))InvokeIfViewExists(() =>
            {
                using var d = new DarkTaskDialog(
                    title: title,
                    message: message,
                    yesText: yes,
                    noText: no,
                    cancelText: cancel,
                    defaultButton: defaultButton,
                    checkBoxText: checkBoxText,
                    icon: GetIcon(icon));

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
        /// <param name="yes"></param>
        /// <param name="no"></param>
        /// <param name="yesIsDangerous"></param>
        /// <param name="checkBoxText"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public (bool Cancel, bool CheckBoxChecked)
        AskToContinueYesNoCustomStrings(
            string message,
            string title,
            MBoxIcon icon,
            string? yes,
            string? no,
            bool yesIsDangerous = false,
            string? checkBoxText = null,
            MBoxButton defaultButton = MBoxButton.Yes) =>
            ((bool, bool))InvokeIfViewExists(() =>
            {
                using var d = new DarkTaskDialog(
                    title: title,
                    message: message,
                    yesText: yes,
                    noText: no,
                    yesIsDangerous: yesIsDangerous,
                    defaultButton: defaultButton,
                    checkBoxText: checkBoxText,
                    icon: GetIcon(icon));

                DialogResult result = d.ShowDialogDark();

                bool cancel = result != DialogResult.Yes;
                bool dontAskAgain = d.IsVerificationChecked;
                return (cancel, dontAskAgain);
            });

        /// <summary>
        /// This method is auto-invoked if <see cref="Core.View"/> is able to be invoked to.
        /// </summary>
        /// <param name="message"></param>
        public void ShowError_ViewOwned(string message)
        {
            AssertR(FormsViewEnvironment.ViewCreated, nameof(FormsViewEnvironment) + "." + nameof(FormsViewEnvironment.ViewCreated) + " was false");
            InvokeIfViewExists(() => ShowError_Internal(message, FormsViewEnvironment.ViewInternal));
        }

        /// <summary>
        /// This method is auto-invoked if <see cref="Core.View"/> is able to be invoked to.
        /// </summary>
        /// <param name="message"></param>
        public void ShowError(string message) =>
            InvokeIfViewExists(() => ShowError_Internal(message, null));

        // Private method, not invoked because all calls are
        private static void ShowError_Internal(string message, IWin32Window? owner)
        {
            using var d = new DarkErrorDialog(message, Paths.LogFile);
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
        public void ShowAlert(
            string message,
            string title,
            MBoxIcon icon = MBoxIcon.Warning) => InvokeIfViewExists(() =>
            {
                if (Config.DarkMode)
                {
                    using var d = new DarkTaskDialog(
                        message: message,
                        title: title,
                        icon: GetIcon(icon),
                        yesText: LText.Global.OK,
                        defaultButton: MBoxButton.Yes);
                    d.ShowDialogDark();
                }
                else
                {
                    MessageBox.Show(message, title, MessageBoxButtons.OK, GetIcon(icon));
                }
            });

        public void ShowAlert_Stock(string message, string title, MBoxButtons buttons, MBoxIcon icon)
        {
            MessageBox.Show(message, title, GetButtons(buttons), GetIcon(icon));
        }
    }
}
