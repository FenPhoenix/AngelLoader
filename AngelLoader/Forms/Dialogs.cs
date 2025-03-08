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

    public (MBoxButton ButtonPressed, bool CheckBoxChecked)
    ShowMultiChoiceDialog(string message,
        string title,
        MBoxIcon icon,
        string? yes,
        string? no,
        string? cancel = null,
        bool yesIsDangerous = false,
        bool noIsDangerous = false,
        bool cancelIsDangerous = false,
        string? checkBoxText = null,
        MBoxButton defaultButton = MBoxButton.Yes,
        bool viewLogButtonVisible = false) =>
        ((MBoxButton, bool))InvokeIfViewExists(() =>
        {
            using DarkTaskDialog d = new(
                message: message,
                title: title,
                icon: GetIcon(icon),
                yesText: yes,
                noText: no,
                cancelText: cancel,
                yesIsDangerous: yesIsDangerous,
                noIsDangerous: noIsDangerous,
                cancelIsDangerous: cancelIsDangerous,
                checkBoxText: checkBoxText,
                defaultButton: defaultButton,
                viewLogButtonVisible: viewLogButtonVisible);

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
            using MessageBoxCustomForm d = new(
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

    public void ShowError(
        string message,
        string? title = null,
        MBoxIcon icon = MBoxIcon.Error) => InvokeIfViewExists(() =>
    {
        IWin32Window? view = FormsViewEnvironment.ViewCreated && !FormsViewEnvironment.ViewInternal.Disposing
            ? FormsViewEnvironment.ViewInternal
            : null;
        ShowError_Internal(message, view, title, icon);
    });

    // Private method, not invoked because all calls are
    private static void ShowError_Internal(string message, IWin32Window? owner, string? title, MBoxIcon icon)
    {
        using DarkErrorDialog d = new(message, title, GetIcon(icon));
        if (owner != null)
        {
            d.ShowDialogDark(owner);
        }
        else
        {
            d.ShowDialogDark();
        }
    }

    public void ShowAlert(
        string message,
        string title,
        MBoxIcon icon = MBoxIcon.Warning) => InvokeIfViewExists(() =>
    {
        using DarkTaskDialog d = new(
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
