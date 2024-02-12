using System.Collections.Generic;
using System.Windows.Forms;
using static AngelLoader.Forms.ControlUtils;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader.Forms;

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
                message: message,
                title: title,
                icon: GetIcon(icon),
                yesText: yes,
                noText: no,
                cancelText: cancel,
                yesIsDangerous: yesIsDangerous,
                checkBoxText: checkBoxText,
                defaultButton: defaultButton);

            DialogResult result = FormsViewEnvironment.ViewCreated
                ? d.ShowDialogDark(FormsViewEnvironment.ViewInternal)
                : d.ShowDialogDark();

            return (DialogResultToMBoxButton(result), d.IsVerificationChecked);
        });

    public (bool Accepted, List<string> SelectedItems)
    ShowListDialog(
        string messageTop,
        string messageBottom,
        string title,
        MBoxIcon icon,
        string okText,
        string cancelText,
        bool okIsDangerous,
        string[] choiceStrings,
        bool multiSelectionAllowed) =>
        ((bool, List<string>))InvokeIfViewExists(() =>
        {
            using var d = new MessageBoxCustomForm(
                messageTop: messageTop,
                messageBottom: messageBottom,
                title: title,
                icon: GetIcon(icon),
                okText: okText,
                cancelText: cancelText,
                okIsDangerous: okIsDangerous,
                choiceStrings: choiceStrings,
                multiSelectionAllowed: multiSelectionAllowed
            );

            /*
            Just always show with us as the owner, because we sometimes hard require it

            From the archive add method:

            "We need to show with explicit owner because otherwise we get in a "halfway" state where
            the dialog is modal, but it can be made to be underneath the main window and then you
            can never get back to it again and have to kill the app through Task Manager."
            */
            DialogResult result = FormsViewEnvironment.ViewCreated
                ? d.ShowDialogDark(FormsViewEnvironment.ViewInternal)
                : d.ShowDialogDark();

            return (result == DialogResult.OK, d.SelectedItems);
        });

#if !X64
    /// <summary>
    /// This method is auto-invoked if <see cref="Core.View"/> is able to be invoked to.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="icon"></param>
    public void ShowError_ViewOwned(string message, string? title = null, MBoxIcon icon = MBoxIcon.Error)
    {
        AssertR(FormsViewEnvironment.ViewCreated, nameof(FormsViewEnvironment) + "." + nameof(FormsViewEnvironment.ViewCreated) + " was false");
        InvokeIfViewExists(() => ShowError_Internal(message, FormsViewEnvironment.ViewInternal, title, icon));
    }
#endif

    /// <summary>
    /// This method is auto-invoked if <see cref="Core.View"/> is able to be invoked to.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="icon"></param>
    public void ShowError(string message, string? title = null, MBoxIcon icon = MBoxIcon.Error) =>
        InvokeIfViewExists(() => ShowError_Internal(message, null, title, icon));

    // Private method, not invoked because all calls are
    private static void ShowError_Internal(string message, IWin32Window? owner, string? title, MBoxIcon icon)
    {
        using var d = new DarkErrorDialog(message, title, GetIcon(icon));
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
        using var d = new DarkTaskDialog(
            message: message,
            title: title,
            icon: GetIcon(icon),
            yesText: LText.Global.OK,
            defaultButton: MBoxButton.Yes);
        d.ShowDialogDark();
    });

    public void ShowAlert_Stock(string message, string title, MBoxButtons buttons, MBoxIcon icon)
    {
        MessageBox.Show(message, title, GetButtons(buttons), GetIcon(icon));
    }
}
