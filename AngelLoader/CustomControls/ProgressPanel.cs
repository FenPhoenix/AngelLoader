using System;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.Utility;
using AngelLoader.Forms;
using AngelLoader.WinAPI.Taskbar;

namespace AngelLoader.CustomControls
{
    public partial class ProgressPanel : UserControl
    {
        #region Fields etc.

        private enum ProgressTask
        {
            ScanAllFMs,
            InstallFM,
            UninstallFM,
            ConvertFiles,
            ImportFromDarkLoader
        }

        private MainForm Owner;
        private ProgressTask _progressTask;

        #endregion

        public ProgressPanel()
        {
            InitializeComponent();
            ProgressCancelButton.CenterH(this);
        }

        internal void Inject(MainForm owner) => Owner = owner;

        #region Show methods

        internal void ShowImportDarkLoader()
        {
            _progressTask = ProgressTask.ImportFromDarkLoader;
            ShowProgressWindow(_progressTask);
        }

        internal void ShowScanningAllFMs()
        {
            _progressTask = ProgressTask.ScanAllFMs;
            ShowProgressWindow(_progressTask);
        }

        internal void ShowInstallingFM()
        {
            _progressTask = ProgressTask.InstallFM;
            ShowProgressWindow(_progressTask);
        }

        internal void ShowUninstallingFM()
        {
            _progressTask = ProgressTask.UninstallFM;
            ShowProgressWindow(_progressTask);
        }

        internal void ShowConvertingFiles()
        {
            _progressTask = ProgressTask.ConvertFiles;
            ShowProgressWindow(_progressTask);
        }

        #endregion

        #region Open/close

        private void ShowProgressWindow(ProgressTask progressTask)
        {
            Center();

            ProgressMessageLabel.Text =
                progressTask == ProgressTask.ScanAllFMs ? LText.ProgressBox.Scanning :
                progressTask == ProgressTask.InstallFM ? LText.ProgressBox.InstallingFM :
                progressTask == ProgressTask.UninstallFM ? LText.ProgressBox.UninstallingFM :
                progressTask == ProgressTask.ConvertFiles ? LText.ProgressBox.ConvertingFiles :
                progressTask == ProgressTask.ImportFromDarkLoader ? LText.ProgressBox.ImportingFromDarkLoader :
                "";

            CurrentThingLabel.Text = progressTask == ProgressTask.ScanAllFMs
                ? LText.ProgressBox.CheckingInstalledFMs
                : "";

            if (progressTask == ProgressTask.UninstallFM ||
                progressTask == ProgressTask.ConvertFiles ||
                progressTask == ProgressTask.ImportFromDarkLoader)
            {
                ProgressBar.Hide();
                TaskBarProgress.SetState(Owner.Handle, TaskbarStates.Indeterminate);
                ProgressCancelButton.Hide();
            }
            else
            {
                ProgressCancelButton.Show();
                ProgressPercentLabel.Text = "";
            }

            ProgressBar.SetValueInstant(0);

            Owner.EnableEverything(false);
            Enabled = true;

            BringToFront();

            Show();
        }

        internal new void Hide()
        {
            TaskBarProgress.SetState(Owner.Handle, TaskbarStates.NoProgress);

            ((Control)this).Hide();

            ProgressMessageLabel.Text = "";
            CurrentThingLabel.Text = "";
            ProgressPercentLabel.Text = "";
            ProgressBar.SetValueInstant(0);

            // We're not actually showing these right here because their parent control is hidden, but we're just
            // turning their visibility back on so we won't forget later
            ProgressBar.Show();
            ProgressCancelButton.Show();

            Enabled = false;
            Owner.EnableEverything(true);
        }

        #endregion

        #region Reporting

        internal void ReportScanProgress(int fmNumber, int fmsTotal, int percent, string fmName)
        {
            ProgressBar.SetValueInstant(percent.Clamp(0, 100));
            var first = LText.ProgressBox.ReportScanningFirst;
            var between = LText.ProgressBox.ReportScanningBetweenNumAndTotal;
            var last = LText.ProgressBox.ReportScanningLast;
            ProgressMessageLabel.Text = first + fmNumber + between + fmsTotal + last;
            CurrentThingLabel.Text = fmName;
            ProgressPercentLabel.Text = percent + "%";

            TaskBarProgress.SetValue(Owner.Handle, percent, 100);
        }

        internal void ReportFMExtractProgress(int percent)
        {
            ProgressBar.SetValueInstant(percent.Clamp(0, 100));
            ProgressMessageLabel.Text = LText.ProgressBox.InstallingFM;
            ProgressPercentLabel.Text = percent + "%";

            TaskBarProgress.SetValue(Owner.Handle, percent, 100);
        }

        internal void SetCancelingFMInstall()
        {
            ProgressMessageLabel.Text = LText.ProgressBox.CancelingInstall;
            ProgressBar.SetValueInstant(0);
            ProgressPercentLabel.Text = "";
        }

        #endregion

        internal void SetUITextToLocalized()
        {
            ProgressCancelButton.SetL10nText(LText.Global.Cancel, ProgressCancelButton.Width);
            ProgressCancelButton.CenterH(this);
        }

        internal void Center() => this.CenterHV(Owner, clientSize: true);

        private void ProgressCancelButton_Click(object sender, EventArgs e)
        {
            switch (_progressTask)
            {
                case ProgressTask.ScanAllFMs:
                    Owner.CancelScan();
                    break;
                case ProgressTask.InstallFM:
                    Owner.CancelInstallFM();
                    break;
            }
        }
    }
}
