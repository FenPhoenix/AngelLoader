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
            ProgressBox.Localize();
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

        // Convenience methods for first show - they handle a few parameters for you
        #region Show methods

        public void ShowProgressBox_Single(
            string? message1 = null,
            string? message2 = null,
            ProgressType? progressType = null,
            ProgressBoxCancelType? cancelType = null,
            Action? cancelAction = null)
        {
            ConstructProgressBox();
            ProgressBox!.SetState(
                visible: true,
                size: ProgressSize.Single,
                mainMessage1: message1 ?? "",
                mainMessage2: message2 ?? "",
                mainPercent: 0,
                mainProgressBarType: progressType ?? ProgressPanel.ProgressTypeDefault,
                subMessage: "",
                subPercent: 0,
                subProgressBarType: ProgressType.Determinate,
                cancelButtonType: cancelType ?? ProgressPanel.CancelTypeDefault,
                cancelAction: cancelAction ?? NullAction);
        }

        public void ShowProgressBox_Double(
            string? mainMessage1 = null,
            string? mainMessage2 = null,
            ProgressType? mainProgressType = null,
            string? subMessage = null,
            ProgressType? subProgressType = null,
            ProgressBoxCancelType? cancelType = null,
            Action? cancelAction = null)
        {
            ConstructProgressBox();
            ProgressBox!.SetState(
                visible: true,
                size: ProgressSize.Double,
                mainMessage1: mainMessage1 ?? "",
                mainMessage2: mainMessage2 ?? "",
                mainPercent: 0,
                mainProgressBarType: mainProgressType ?? ProgressPanel.ProgressTypeDefault,
                subMessage: subMessage ?? "",
                subPercent: 0,
                subProgressBarType: subProgressType ?? ProgressPanel.ProgressTypeDefault,
                cancelButtonType: cancelType ?? ProgressPanel.CancelTypeDefault,
                cancelAction: cancelAction ?? NullAction);
        }

        #endregion

        // Intended to be used after first show, for modifying the state
        #region Set methods

        public void SetProgressBoxState_Single(
            bool? visible = null,
            string? message1 = null,
            string? message2 = null,
            int? percent = null,
            ProgressType? progressType = null,
            ProgressBoxCancelType? cancelType = null,
            Action? cancelAction = null)
        {
            ConstructProgressBox();
            ProgressBox!.SetState(
                visible: visible,
                size: ProgressSize.Single,
                mainMessage1: message1,
                mainMessage2: message2,
                mainPercent: percent,
                mainProgressBarType: progressType,
                subMessage: "",
                subPercent: 0,
                subProgressBarType: ProgressType.Determinate,
                cancelButtonType: cancelType,
                cancelAction: cancelAction);
        }

        public void SetProgressBoxState_Double(
            bool? visible = null,
            string? mainMessage1 = null,
            string? mainMessage2 = null,
            int? mainPercent = null,
            ProgressType? mainProgressType = null,
            string? subMessage = null,
            int? subPercent = null,
            ProgressType? subProgressType = null,
            ProgressBoxCancelType? cancelType = null,
            Action? cancelAction = null)
        {
            ConstructProgressBox();
            ProgressBox!.SetState(
                visible: visible,
                size: ProgressSize.Double,
                mainMessage1: mainMessage1,
                mainMessage2: mainMessage2,
                mainPercent: mainPercent,
                mainProgressBarType: mainProgressType,
                subMessage: subMessage,
                subPercent: subPercent,
                subProgressBarType: subProgressType,
                cancelButtonType: cancelType,
                cancelAction: cancelAction);
        }

        public void SetProgressPercent(int percent)
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
                cancelButtonType: null,
                cancelAction: null);
        }

        public void SetProgressBoxState(
            bool? visible = null,
            ProgressSize? size = null,
            string? mainMessage1 = null,
            string? mainMessage2 = null,
            int? mainPercent = null,
            ProgressType? mainProgressType = null,
            string? subMessage = null,
            int? subPercent = null,
            ProgressType? subProgressType = null,
            ProgressBoxCancelType? cancelType = null,
            Action? cancelAction = null)
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
                cancelButtonType: cancelType,
                cancelAction: cancelAction);
        }

        #endregion

        public void HideProgressBox()
        {
            ConstructProgressBox();
            ProgressBox!.HideThis();
        }
    }
}
