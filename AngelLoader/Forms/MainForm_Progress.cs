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

        // @MEM(Progress report invoking): IMPORTANT: Test all of these!!! They can fail at runtime due to param counts and types etc.

        // Convenience methods for first show - they handle a few parameters for you
        #region Show methods

        #region ShowProgressBox_Single

        private static readonly Action<MainForm, string?, string?, ProgressType?, ProgressCancelType?, Action?>
            ShowProgressBox_Single_Action =
                (view, message1, message2, progressType, cancelType, cancelAction) =>
                {
                    view.ConstructProgressBox();
                    view.ProgressBox!.SetState(
                        visible: true,
                        size: ProgressSizeMode.Single,
                        mainMessage1: message1 ?? "",
                        mainMessage2: message2 ?? "",
                        mainPercent: 0,
                        mainProgressBarType: progressType ?? ProgressPanel.DefaultProgressType,
                        subMessage: "",
                        subPercent: 0,
                        subProgressBarType: ProgressType.Determinate,
                        cancelButtonType: cancelType ?? ProgressPanel.DefaultCancelType,
                        cancelAction: cancelAction ?? NullAction);
                };

        public void ShowProgressBox_Single(
            string? message1 = null,
            string? message2 = null,
            ProgressType? progressType = null,
            ProgressCancelType? cancelType = null,
            Action? cancelAction = null) => Invoke(
            ShowProgressBox_Single_Action,
            this,
            message1,
            message2,
            progressType,
            cancelType,
            cancelAction);

        #endregion

        #region ShowProgressBox_Double

        private static readonly Action<MainForm, string?, string?, ProgressType?, string?, ProgressType?,
                ProgressCancelType?, Action?>
            ShowProgressBox_Double_Action =
                (view, mainMessage1, mainMessage2, mainProgressType, subMessage, subProgressType, cancelType,
                    cancelAction) =>
                {
                    view.ConstructProgressBox();
                    view.ProgressBox!.SetState(
                        visible: true,
                        size: ProgressSizeMode.Double,
                        mainMessage1: mainMessage1 ?? "",
                        mainMessage2: mainMessage2 ?? "",
                        mainPercent: 0,
                        mainProgressBarType: mainProgressType ?? ProgressPanel.DefaultProgressType,
                        subMessage: subMessage ?? "",
                        subPercent: 0,
                        subProgressBarType: subProgressType ?? ProgressPanel.DefaultProgressType,
                        cancelButtonType: cancelType ?? ProgressPanel.DefaultCancelType,
                        cancelAction: cancelAction ?? NullAction);
                };

        public void ShowProgressBox_Double(
            string? mainMessage1 = null,
            string? mainMessage2 = null,
            ProgressType? mainProgressType = null,
            string? subMessage = null,
            ProgressType? subProgressType = null,
            ProgressCancelType? cancelType = null,
            Action? cancelAction = null) => Invoke(
            ShowProgressBox_Double_Action,
            this,
            mainMessage1,
            mainMessage2,
            mainProgressType,
            subMessage,
            subProgressType,
            cancelType,
            cancelAction);

        #endregion

        #endregion

        // Intended to be used after first show, for modifying the state
        #region Set methods

        // We cache the action that will be invoked, to avoid recreating it a zillion times while reporting
        // progress. Yes, they need to take the form they're on as a parameter, and yeah the code looks really
        // dumb, but hey, it gets rid of the mountain of allocations.

        #region SetProgressBoxState_Single

        private static readonly Action<MainForm, bool?, string?, string?, int?, ProgressType?,
                ProgressCancelType?, Action?>
            SetProgressBoxState_Single_Action = (view, visible, message1, message2, percent, progressType,
                cancelType, cancelAction) =>
            {
                view.ConstructProgressBox();
                view.ProgressBox!.SetState(
                    visible: visible,
                    size: ProgressSizeMode.Single,
                    mainMessage1: message1,
                    mainMessage2: message2,
                    mainPercent: percent,
                    mainProgressBarType: progressType,
                    subMessage: "",
                    subPercent: 0,
                    subProgressBarType: ProgressType.Determinate,
                    cancelButtonType: cancelType,
                    cancelAction: cancelAction);
            };

        public void SetProgressBoxState_Single(
            bool? visible = null,
            string? message1 = null,
            string? message2 = null,
            int? percent = null,
            ProgressType? progressType = null,
            ProgressCancelType? cancelType = null,
            Action? cancelAction = null) => Invoke(
            SetProgressBoxState_Single_Action,
            this,
            visible,
            message1,
            message2,
            percent,
            progressType,
            cancelType,
            cancelAction);

        #endregion

        #region SetProgressBoxState_Double

        private static readonly Action<MainForm, bool?, string?, string?, int?, ProgressType?, string?, int?,
                ProgressType?, ProgressCancelType?, Action?>
            SetProgressBoxState_Double_Action =
                (view, visible, mainMessage1, mainMessage2, mainPercent, mainProgressType, subMessage,
                    subPercent, subProgressType, cancelType, cancelAction) =>
                {
                    view.ConstructProgressBox();
                    view.ProgressBox!.SetState(
                        visible: visible,
                        size: ProgressSizeMode.Double,
                        mainMessage1: mainMessage1,
                        mainMessage2: mainMessage2,
                        mainPercent: mainPercent,
                        mainProgressBarType: mainProgressType,
                        subMessage: subMessage,
                        subPercent: subPercent,
                        subProgressBarType: subProgressType,
                        cancelButtonType: cancelType,
                        cancelAction: cancelAction);
                };

        public void SetProgressBoxState_Double(
            bool? visible = null,
            string? mainMessage1 = null,
            string? mainMessage2 = null,
            int? mainPercent = null,
            ProgressType? mainProgressType = null,
            string? subMessage = null,
            int? subPercent = null,
            ProgressType? subProgressType = null,
            ProgressCancelType? cancelType = null,
            Action? cancelAction = null) => Invoke(
            SetProgressBoxState_Double_Action,
            this,
            visible,
            mainMessage1,
            mainMessage2,
            mainPercent,
            mainProgressType,
            subMessage,
            subPercent,
            subProgressType,
            cancelType,
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
                cancelButtonType: null,
                cancelAction: null);
        };

        public void SetProgressPercent(int percent) => Invoke(SetProgressPercent_Action, this, percent);

        #endregion

        #region SetProgressBoxState

        private static readonly Action<MainForm, bool?, ProgressSizeMode?, string?, string?, int?, ProgressType?,
                string?, int?, ProgressType?, ProgressCancelType?, Action?>
            SetProgressBoxState_Action =
                (view, visible, size, mainMessage1, mainMessage2, mainPercent, mainProgressType, subMessage,
                    subPercent, subProgressType, cancelType, cancelAction) =>
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
                        cancelButtonType: cancelType,
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
            ProgressCancelType? cancelType = null,
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
            cancelType,
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
