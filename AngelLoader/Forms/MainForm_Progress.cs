using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;
using JetBrains.Annotations;
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
        [PublicAPI]
        private bool ProgressBoxDarkModeEnabled
        {
            get => _progressBoxDarkModeEnabled;
            set
            {
                _progressBoxDarkModeEnabled = value;
                if (!_progressBoxConstructed) return;

                ProgressBox!.DarkModeEnabled = value;
            }
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

            _progressBoxConstructed = true;
        }

        private void LocalizeProgressBox()
        {
            if (!_progressBoxConstructed) return;
            ProgressBox!.Localize();
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

        public void ShowProgressBox(ProgressTask progressTask, bool suppressShow = false)
        {
            ConstructProgressBox();
            ProgressBox!.ShowProgressWindow(progressTask, suppressShow);
        }

        public void SetProgressBoxSizeMode(bool doubleSize)
        {
            ConstructProgressBox();
            ProgressBox!.SetSizeMode(doubleSize);
        }

        public void SetProgressBoxFirstMessage(string message)
        {
            ConstructProgressBox();
            ProgressBox!.SetMainMessage(message);
        }

        public void SetProgressBoxSecondMessage(string message)
        {
            ConstructProgressBox();
            ProgressBox!.SetCurrentThingMessage(message);
        }

        public void ReportScanProgress(int fmNumber, int fmsTotal, int percent, string fmName)
        {
            ConstructProgressBox();
            ProgressBox!.ReportScanProgress(fmNumber, fmsTotal, percent, fmName);
        }

        public void ReportFMInstallProgress(int percent)
        {
            ConstructProgressBox();
            ProgressBox!.ReportFMInstallProgress(percent);
        }

        public void ReportFMInstallRollbackProgress(int percent, string message)
        {
            ConstructProgressBox();
            ProgressBox!.ReportFMInstallRollbackProgress(percent, message);
        }

        /// <summary>
        /// For the percents, -1 means don't update the displayed values.
        /// </summary>
        /// <param name="mainPercent"></param>
        /// <param name="subPercent"></param>
        /// <param name="fmName"></param>
        public void ReportMultiFMInstallProgress(int mainPercent, int subPercent, string fmName)
        {
            ConstructProgressBox();
            ProgressBox!.ReportMultiFMInstallProgress(mainPercent, subPercent, fmName);
        }

        /// <summary>
        /// For the percents, -1 means don't update the displayed values.
        /// </summary>
        /// <param name="mainPercent"></param>
        /// <param name="subPercent"></param>
        /// <param name="subMessage"></param>
        /// <param name="fmName"></param>
        public void ReportMultiFMInstallProgress(int mainPercent, int subPercent, string subMessage, string fmName)
        {
            ConstructProgressBox();
            ProgressBox!.ReportMultiFMInstallProgress(mainPercent, subPercent, fmName, subMessage: subMessage);
        }

        public void ReportCachingProgress(int percent)
        {
            ConstructProgressBox();
            ProgressBox!.ReportCachingProgress(percent);
        }

        public void SetCancelingFMInstall()
        {
            ConstructProgressBox();
            ProgressBox!.SetCancelingFMInstall();
        }

        public void HideProgressBox()
        {
            ConstructProgressBox();
            ProgressBox!.HideThis();
        }
    }
}
