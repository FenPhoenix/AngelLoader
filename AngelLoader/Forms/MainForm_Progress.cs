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

        // We don't cache the actions anymore, because we still ended up recreating the params object[] array
        // every time, which was in almost all cases _larger_ than the 32 bytes of an action. Also, it made our
        // parameters un-statically-checkable for correct number and types, since they were a variable-length
        // array of plain objects. So... yeah, not worth it.

        // Convenience methods for first show - they handle a few parameters for you
        #region Show methods

        public void ShowProgressBox_Single(
            bool? showCheckBox = null,
            string? message1 = null,
            string? message2 = null,
            ProgressType? progressType = null,
            string? checkBoxMessage = null,
            Action<bool>? checkBoxAction = null,
            string? cancelMessage = null,
            Action? cancelAction = null) => Invoke(() =>
        {
            ConstructProgressBox();
            ProgressBox!.SetState(
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
        });

        public void ShowProgressBox_Double(
            bool? showCheckBox = null,
            string? mainMessage1 = null,
            string? mainMessage2 = null,
            ProgressType? mainProgressType = null,
            string? subMessage = null,
            ProgressType? subProgressType = null,
            string? checkBoxMessage = null,
            Action<bool>? checkBoxAction = null,
            string? cancelMessage = null,
            Action? cancelAction = null) => Invoke(() =>
        {
            ConstructProgressBox();
            ProgressBox!.SetState(
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
        });

        #endregion

        // Intended to be used after first show, for modifying the state
        #region Set methods

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
            Action? cancelAction = null) => Invoke(() =>
        {
            ConstructProgressBox();
            ProgressBox!.SetState(
                visible: visible,
                size: showCheckBox switch
                {
                    true => ProgressSizeMode.SingleWithCheck,
                    false => ProgressSizeMode.Single,
                    _ => null
                },
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
        });

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
            Action? cancelAction = null) => Invoke(() =>
        {
            ConstructProgressBox();
            ProgressBox!.SetState(
                visible: visible,
                size: showCheckBox switch
                {
                    true => ProgressSizeMode.DoubleWithCheck,
                    false => ProgressSizeMode.Double,
                    _ => null
                },
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
        });

        public void SetProgressPercent(int percent) => Invoke(() =>
        {
            ConstructProgressBox();
            ProgressBox!.SetState(
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
        });

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
            Action? cancelAction = null) =>
            Invoke(() =>
            {
                ConstructProgressBox();
                ProgressBox!.SetState(
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
            });

        #endregion

        public void HideProgressBox() => Invoke(() =>
        {
            ConstructProgressBox();
            ProgressBox!.HideThis();
        });
    }
}
