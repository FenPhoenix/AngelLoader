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

            ProgressBox = new ProgressPanel();
            Controls.Add(ProgressBox);
            ProgressBox.Inject(this);
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

        public void ReportScanProgress(int fmNumber, int fmsTotal, int percent, string fmName)
        {
            ConstructProgressBox();
            ProgressBox!.ReportScanProgress(fmNumber, fmsTotal, percent, fmName);
        }

        public void ReportFMExtractProgress(int percent)
        {
            ConstructProgressBox();
            ProgressBox!.ReportFMExtractProgress(percent);
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
