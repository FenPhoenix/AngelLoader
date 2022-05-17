using System;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    public sealed partial class MainForm
    {
        // Not great code really, but works.

        private ProgressPanel? ProgressBox;

        // Progress box not being null does not necessarily mean it's fully constructed.
        private bool _progressBoxConstructed;

        private bool _progressBoxDarkModeEnabled;
        private void SetProgressBoxDarkModeEnabled(bool value)
        {
            _progressBoxDarkModeEnabled = value;
            if (!_progressBoxConstructed) return;

            ProgressBox!.DarkModeEnabled = value;
        }

        private void ConstructProgressBox()
        {
            if (_progressBoxConstructed) return;

            ProgressBox = new ProgressPanel { Tag = LoadType.Lazy, Visible = false };
            Controls.Add(ProgressBox);
            ProgressBox.InjectOwner(this);
            ProgressBox.Anchor = AnchorStyles.None;
            ProgressBox.DarkModeEnabled = _progressBoxDarkModeEnabled;
            ProgressBox.SetSizeToDefault();

            _progressBoxConstructed = true;
        }

        internal void EnableEverything(bool enabled)
        {
            bool doFocus = !EverythingPanel.Enabled && enabled;

            EverythingPanel.Enabled = enabled;

            if (!doFocus) return;

            // The "mouse wheel scroll without needing to focus" thing stops working when no control is focused
            // (this happens when we disable and enable EverythingPanel). Therefore, we need to give focus to a
            // control here. One is as good as the next, but FMsDGV seems like a sensible choice.
            FMsDGV.Focus();
            FMsDGV.SelectProperly();
        }

        // Just always invoke these, because they're almost always called from another thread anyway. Keeps it
        // simple.

        // @vNext/@MEM(Progress report invoking): IMPORTANT: Test all of these!!! They can fail at runtime due to param counts and types etc.

        // Convenience methods for first show - they handle a few parameters for you
        #region Show methods

        #region ShowProgressBox_Single

        private static readonly Action<
                MainForm,
                bool?,
                string?,
                string?,
                ProgressType?,
                string?,
                Action<bool>?,
                string?,
                Action?>
            ShowProgressBox_Single_Action =
                (
                    view,
                    showCheckBox,
                    message1,
                    message2,
                    progressType,
                    checkBoxMessage,
                    checkBoxAction,
                    cancelMessage,
                    cancelAction
                ) =>
                {
                    view.ConstructProgressBox();
                    view.ProgressBox!.SetState(
                        visible: true,
                        size: showCheckBox == true ? ProgressSizeMode.SingleWithCheck : ProgressSizeMode.Single,
                        mainMessage1: message1 ?? "",
                        mainMessage2: message2 ?? "",
                        mainPercent: 0,
                        mainProgressBarType: progressType ?? ProgressPanel.DefaultProgressType,
                        subMessage: "",
                        subPercent: 0,
                        subProgressBarType: ProgressType.Determinate,
                        checkBoxMessage: checkBoxMessage ?? "",
                        checkChangedAction: checkBoxAction ?? NullBoolAction,
                        cancelButtonMessage: cancelMessage ?? ProgressPanel.DefaultCancelMessage,
                        cancelAction: cancelAction ?? NullAction);
                };

        public void ShowProgressBox_Single(
            bool? showCheckBox = null,
            string? message1 = null,
            string? message2 = null,
            ProgressType? progressType = null,
            string? checkBoxMessage = null,
            Action<bool>? checkBoxAction = null,
            string? cancelMessage = null,
            Action? cancelAction = null) => Invoke(
            ShowProgressBox_Single_Action,
            this,
            showCheckBox,
            message1,
            message2,
            progressType,
            checkBoxMessage,
            checkBoxAction,
            cancelMessage,
            cancelAction);

        #endregion

        #region ShowProgressBox_Double

        private static readonly Action<
                MainForm,
                bool?,
                string?,
                string?,
                ProgressType?,
                string?,
                ProgressType?,
                string?,
                Action<bool>?,
                string?,
                Action?>
            ShowProgressBox_Double_Action =
                (
                    view,
                    showCheckBox,
                    mainMessage1,
                    mainMessage2,
                    mainProgressType,
                    subMessage,
                    subProgressType,
                    checkBoxMessage,
                    checkBoxAction,
                    cancelMessage,
                    cancelAction
                ) =>
                {
                    view.ConstructProgressBox();
                    view.ProgressBox!.SetState(
                        visible: true,
                        size: showCheckBox == true ? ProgressSizeMode.DoubleWithCheck : ProgressSizeMode.Double,
                        mainMessage1: mainMessage1 ?? "",
                        mainMessage2: mainMessage2 ?? "",
                        mainPercent: 0,
                        mainProgressBarType: mainProgressType ?? ProgressPanel.DefaultProgressType,
                        subMessage: subMessage ?? "",
                        subPercent: 0,
                        subProgressBarType: subProgressType ?? ProgressPanel.DefaultProgressType,
                        checkBoxMessage: checkBoxMessage ?? "",
                        checkChangedAction: checkBoxAction ?? NullBoolAction,
                        cancelButtonMessage: cancelMessage ?? ProgressPanel.DefaultCancelMessage,
                        cancelAction: cancelAction ?? NullAction);
                };

        public void ShowProgressBox_Double(
            bool? showCheck = null,
            string? mainMessage1 = null,
            string? mainMessage2 = null,
            ProgressType? mainProgressType = null,
            string? subMessage = null,
            ProgressType? subProgressType = null,
            string? checkBoxMessage = null,
            Action<bool>? checkBoxAction = null,
            string? cancelMessage = null,
            Action? cancelAction = null) => Invoke(
            ShowProgressBox_Double_Action,
            this,
            showCheck,
            mainMessage1,
            mainMessage2,
            mainProgressType,
            subMessage,
            subProgressType,
            checkBoxMessage,
            checkBoxAction,
            cancelMessage,
            cancelAction);

        #endregion

        #endregion

        // Intended to be used after first show, for modifying the state
        #region Set methods

        // We cache the action that will be invoked, to avoid recreating it a zillion times while reporting
        // progress. Yes, they need to take the form they're on as a parameter, and yeah the code looks really
        // dumb, but hey, it gets rid of the mountain of allocations.

        #region SetProgressBoxState_Single

        private static readonly Action<
                MainForm,
                bool?,
                bool?,
                string?,
                string?,
                int?,
                ProgressType?,
                string?,
                Action<bool>?,
                string?,
                Action?>
            SetProgressBoxState_Single_Action =
                (
                    view,
                    visible,
                    showCheckBox,
                    message1,
                    message2,
                    percent,
                    progressType,
                    checkBoxMessage,
                    checkBoxAction,
                    cancelMessage,
                    cancelAction
                ) =>
                {
                    view.ConstructProgressBox();
                    view.ProgressBox!.SetState(
                        visible: visible,
                        size: showCheckBox == true ? ProgressSizeMode.SingleWithCheck : ProgressSizeMode.Single,
                        mainMessage1: message1,
                        mainMessage2: message2,
                        mainPercent: percent,
                        mainProgressBarType: progressType,
                        subMessage: "",
                        subPercent: 0,
                        subProgressBarType: ProgressType.Determinate,
                        checkBoxMessage: checkBoxMessage,
                        checkChangedAction: checkBoxAction,
                        cancelButtonMessage: cancelMessage,
                        cancelAction: cancelAction);
                };

        public void SetProgressBoxState_Single(
            bool? visible = null,
            bool? showCheckBox = null,
            string? message1 = null,
            string? message2 = null,
            int? percent = null,
            ProgressType? progressType = null,
            string? checkBoxMessage = null,
            Action<bool>? checkBoxAction = null,
            string? cancelMessage = null,
            Action? cancelAction = null) => Invoke(
            SetProgressBoxState_Single_Action,
            this,
            visible,
            showCheckBox,
            message1,
            message2,
            percent,
            progressType,
            checkBoxMessage,
            checkBoxAction,
            cancelMessage,
            cancelAction);

        #endregion

        #region SetProgressBoxState_Double

        private static readonly Action<
                MainForm,
                bool?,
                bool?,
                string?,
                string?,
                int?,
                ProgressType?,
                string?,
                int?,
                ProgressType?,
                string?,
                Action<bool>?,
                string?,
                Action?>
            SetProgressBoxState_Double_Action =
                (
                    view,
                    visible,
                    showCheckBox,
                    mainMessage1,
                    mainMessage2,
                    mainPercent,
                    mainProgressType,
                    subMessage,
                    subPercent,
                    subProgressType,
                    checkBoxMessage,
                    checkBoxAction,
                    cancelMessage,
                    cancelAction
                ) =>
                {
                    view.ConstructProgressBox();
                    view.ProgressBox!.SetState(
                        visible: visible,
                        size: showCheckBox == true ? ProgressSizeMode.DoubleWithCheck : ProgressSizeMode.Double,
                        mainMessage1: mainMessage1,
                        mainMessage2: mainMessage2,
                        mainPercent: mainPercent,
                        mainProgressBarType: mainProgressType,
                        subMessage: subMessage,
                        subPercent: subPercent,
                        subProgressBarType: subProgressType,
                        checkBoxMessage: checkBoxMessage,
                        checkChangedAction: checkBoxAction,
                        cancelButtonMessage: cancelMessage,
                        cancelAction: cancelAction);
                };

        public void SetProgressBoxState_Double(
            bool? visible = null,
            bool? showCheckBox = null,
            string? mainMessage1 = null,
            string? mainMessage2 = null,
            int? mainPercent = null,
            ProgressType? mainProgressType = null,
            string? subMessage = null,
            int? subPercent = null,
            ProgressType? subProgressType = null,
            string? checkBoxMessage = null,
            Action<bool>? checkBoxAction = null,
            string? cancelMessage = null,
            Action? cancelAction = null) => Invoke(
            SetProgressBoxState_Double_Action,
            this,
            visible,
            showCheckBox,
            mainMessage1,
            mainMessage2,
            mainPercent,
            mainProgressType,
            subMessage,
            subPercent,
            subProgressType,
            checkBoxMessage,
            checkBoxAction,
            cancelMessage,
            cancelAction);

        #endregion

        #region SetProgressPercent

        private static readonly Action<MainForm, int> SetProgressPercent_Action = (view, percent) =>
        {
            view.ConstructProgressBox();
            view.ProgressBox!.SetState(
                visible: null,
                size: null,
                mainMessage1: null,
                mainMessage2: null,
                mainPercent: percent,
                mainProgressBarType: null,
                subMessage: null,
                subPercent: null,
                subProgressBarType: null,
                checkBoxMessage: null,
                checkChangedAction: null,
                cancelButtonMessage: null,
                cancelAction: null);
        };

        public void SetProgressPercent(int percent) => Invoke(SetProgressPercent_Action, this, percent);

        #endregion

        #region SetProgressBoxState

        private static readonly Action<
                MainForm,
                bool?,
                ProgressSizeMode?,
                string?,
                string?,
                int?,
                ProgressType?,
                string?,
                int?,
                ProgressType?,
                string?,
                Action<bool>?,
                string?,
                Action?>
            SetProgressBoxState_Action =
                (
                    view,
                    visible,
                    size,
                    mainMessage1,
                    mainMessage2,
                    mainPercent,
                    mainProgressType,
                    subMessage,
                    subPercent,
                    subProgressType,
                    checkBoxMessage,
                    checkBoxAction,
                    cancelMessage,
                    cancelAction
                ) =>
                {
                    view.ConstructProgressBox();
                    view.ProgressBox!.SetState(
                        visible: visible,
                        size: size,
                        mainMessage1: mainMessage1,
                        mainMessage2: mainMessage2,
                        mainPercent: mainPercent,
                        mainProgressBarType: mainProgressType,
                        subMessage: subMessage,
                        subPercent: subPercent,
                        subProgressBarType: subProgressType,
                        checkBoxMessage: checkBoxMessage,
                        checkChangedAction: checkBoxAction,
                        cancelButtonMessage: cancelMessage,
                        cancelAction: cancelAction);
                };

        public void SetProgressBoxState(
            bool? visible = null,
            ProgressSizeMode? size = null,
            string? mainMessage1 = null,
            string? mainMessage2 = null,
            int? mainPercent = null,
            ProgressType? mainProgressType = null,
            string? subMessage = null,
            int? subPercent = null,
            ProgressType? subProgressType = null,
            string? checkBoxMessage = null,
            Action<bool>? checkBoxAction = null,
            string? cancelMessage = null,
            Action? cancelAction = null) => Invoke(
            SetProgressBoxState_Action,
            this,
            visible,
            size,
            mainMessage1,
            mainMessage2,
            mainPercent,
            mainProgressType,
            subMessage,
            subPercent,
            subProgressType,
            checkBoxMessage,
            checkBoxAction,
            cancelMessage,
            cancelAction
        );

        #endregion

        #endregion

        #region HideProgressBox

        private static readonly Action<MainForm> HideProgressBox_Action = view =>
        {
            view.ConstructProgressBox();
            view.ProgressBox!.HideThis();
        };

        public void HideProgressBox() => Invoke(HideProgressBox_Action, this);

        #endregion
    }
}
