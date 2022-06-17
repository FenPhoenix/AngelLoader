using System.Windows.Forms;
using static AngelLoader.Forms.ControlUtils;
using static AngelLoader.Misc;

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
        /// <param name="icon"></param>
        /// <param name="yes"></param>
        /// <param name="no"></param>
        /// <param name="cancel"></param>
        /// <param name="yesIsDangerous"></param>
        /// <param name="checkBoxText"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        public (MBoxButton ButtonPressed, bool CheckBoxChecked)
        ShowMultiChoiceDialog(
            string message,
            string title,
            MBoxIcon icon,
            string? yes,
            string? no,
            string? cancel = null,
            bool yesIsDangerous = false,
            string? checkBoxText = null,
            MBoxButton defaultButton = MBoxButton.Yes) =>
            ((MBoxButton, bool))InvokeIfViewExists(() =>
            {
                using var d = new DarkTaskDialog(
                    title: title,
                    message: message,
                    yesText: yes,
                    noText: no,
                    cancelText: cancel,
                    yesIsDangerous: yesIsDangerous,
                    defaultButton: defaultButton,
                    checkBoxText: checkBoxText,
                    icon: GetIcon(icon));

                return (DialogResultToMBoxButton(d.ShowDialogDark()), d.IsVerificationChecked);
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
