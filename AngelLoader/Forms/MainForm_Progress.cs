using System.Windows.Forms;
using AngelLoader.CustomControls;
using static AngelLoader.Common.Logger;
using static AngelLoader.CustomControls.ProgressPanel;

namespace AngelLoader.Forms
{
    public partial class MainForm
    {
        // Not great code really, but works.

        private ProgressPanel ProgressBox;

        private void ConstructProgressBox()
        {
            if (ProgressBox == null)
            {
                ProgressBox = new ProgressPanel();
                Controls.Add(ProgressBox);
                ProgressBox.Inject(this);
                ProgressBox.SetUITextToLocalized();
                ProgressBox.Anchor = AnchorStyles.None;
            }
        }

        private void LocalizeProgressBox() => ProgressBox?.SetUITextToLocalized();

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

        public void ShowProgressBox(ProgressTasks progressTask, bool suppressShow = false)
        {
            ConstructProgressBox();

            Log(nameof(ShowProgressBox) + ": " + progressTask, methodName: false);
            ProgressBox.ShowProgressWindow(progressTask, suppressShow);
        }

        public void ReportScanProgress(int fmNumber, int fmsTotal, int percent, string fmName)
        {
            ConstructProgressBox();
            ProgressBox.ReportScanProgress(fmNumber, fmsTotal, percent, fmName);
        }

        public void ReportFMExtractProgress(int percent)
        {
            ConstructProgressBox();
            ProgressBox.ReportFMExtractProgress(percent);
        }

        public void ReportCachingProgress(int percent)
        {
            ConstructProgressBox();
            ProgressBox.ReportCachingProgress(percent);
        }

        public void SetCancelingFMInstall()
        {
            ConstructProgressBox();
            ProgressBox.SetCancelingFMInstall();
        }

        public void HideProgressBox()
        {
            ConstructProgressBox();
            ProgressBox.HideThis();
        }
    }
}
