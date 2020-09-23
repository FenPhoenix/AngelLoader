using System;
using System.Windows.Forms;
using AngelLoader.WinAPI.Taskbar;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class ProgressPanel : UserControl
    {
        // TODO: The way this works is no longer really tenable - rework it to be cleaner

        #region Fields etc.

        private MainForm? _owner;
        private ProgressTask _progressTask;

        #endregion

        public ProgressPanel()
        {
#if DEBUG
            InitializeComponent();
#else
            InitComponentManual();
#endif
        }

        internal void Inject(MainForm owner) => _owner = owner;

        #region Open/close

        internal void ShowProgressWindow(ProgressTask progressTask, bool suppressShow = false)
        {
            _progressTask = progressTask;

            this.CenterHV(_owner!, clientSize: true);

            ProgressMessageLabel.Text = progressTask switch
            {
                ProgressTask.ScanAllFMs => LText.ProgressBox.Scanning,
                ProgressTask.InstallFM => LText.ProgressBox.InstallingFM,
                ProgressTask.UninstallFM => LText.ProgressBox.UninstallingFM,
                ProgressTask.ConvertFiles => LText.ProgressBox.ConvertingFiles,
                ProgressTask.ImportFromDarkLoader => LText.ProgressBox.ImportingFromDarkLoader,
                ProgressTask.ImportFromNDL => LText.ProgressBox.ImportingFromNewDarkLoader,
                ProgressTask.ImportFromFMSel => LText.ProgressBox.ImportingFromFMSel,
                ProgressTask.CacheFM => LText.ProgressBox.CachingReadmeFiles,
                ProgressTask.DeleteFMArchive => LText.ProgressBox.DeletingFMArchive,
                _ => ""
            };

            CurrentThingLabel.Text =
                progressTask == ProgressTask.ScanAllFMs ? LText.ProgressBox.CheckingInstalledFMs
                : "";

            ProgressPercentLabel.Text = "";

            if (progressTask == ProgressTask.UninstallFM ||
                progressTask == ProgressTask.ConvertFiles ||
                progressTask == ProgressTask.ImportFromDarkLoader ||
                progressTask == ProgressTask.ImportFromNDL ||
                progressTask == ProgressTask.ImportFromFMSel ||
                progressTask == ProgressTask.DeleteFMArchive)
            {
                ProgressBar.Style = ProgressBarStyle.Marquee;
                if (_owner?.IsHandleCreated == true) TaskBarProgress.SetState(_owner.Handle, TaskbarStates.Indeterminate);
                ProgressCancelButton.Hide();
            }
            else
            {
                ProgressBar.Style = ProgressBarStyle.Blocks;
                ProgressCancelButton.Visible = progressTask != ProgressTask.CacheFM;
                ProgressBar.SetValueInstant(0);
            }

            if (!suppressShow) ShowThis();
        }

        private void ShowThis()
        {
            _owner!.EnableEverything(false);
            Enabled = true;

            BringToFront();
            Show();
        }

        internal void HideThis()
        {
            if (_owner?.IsHandleCreated == true) TaskBarProgress.SetState(_owner.Handle, TaskbarStates.NoProgress);

            Hide();

            ProgressMessageLabel.Text = "";
            CurrentThingLabel.Text = "";
            ProgressPercentLabel.Text = "";
            ProgressBar.SetValueInstant(0);

            ProgressBar.Style = ProgressBarStyle.Blocks;
            ProgressCancelButton.Show();

            Enabled = false;
            _owner!.EnableEverything(true);
        }

        #endregion

        #region Reporting

        internal void ReportScanProgress(int fmNumber, int fmsTotal, int percent, string fmName)
        {
            ProgressBar.SetValueInstant(percent.Clamp(0, 100));
            string first = LText.ProgressBox.ReportScanningFirst;
            string between = LText.ProgressBox.ReportScanningBetweenNumAndTotal;
            string last = LText.ProgressBox.ReportScanningLast;
            ProgressMessageLabel.Text = first + fmNumber + between + fmsTotal + last;
            CurrentThingLabel.Text = fmName;
            ProgressPercentLabel.Text = percent + "%";

            if (_owner?.IsHandleCreated == true) TaskBarProgress.SetValue(_owner.Handle, percent, 100);
        }

        internal void ReportFMExtractProgress(int percent)
        {
            ProgressBar.SetValueInstant(percent.Clamp(0, 100));
            ProgressMessageLabel.Text = LText.ProgressBox.InstallingFM;
            ProgressPercentLabel.Text = percent + "%";

            if (_owner?.IsHandleCreated == true) TaskBarProgress.SetValue(_owner.Handle, percent, 100);
        }

        internal void ReportCachingProgress(int percent)
        {
            ProgressBar.SetValueInstant(percent.Clamp(0, 100));
            ProgressPercentLabel.Text = percent + "%";

            if (Visible)
            {
                if (_owner?.IsHandleCreated == true) TaskBarProgress.SetValue(_owner.Handle, percent, 100);
            }
            else
            {
                if (_owner?.IsHandleCreated == true) TaskBarProgress.SetState(_owner.Handle, TaskbarStates.NoProgress);
            }
        }

        internal void SetCancelingFMInstall()
        {
            ProgressCancelButton.Hide();
            ProgressBar.Style = ProgressBarStyle.Marquee;
            ProgressMessageLabel.Text = LText.ProgressBox.CancelingInstall;
            ProgressBar.SetValueInstant(0);
            ProgressPercentLabel.Text = "";
        }

        #endregion

        internal void Localize()
        {
            ProgressCancelButton.Text = LText.Global.Cancel;
            ProgressCancelButton.CenterH(this);
        }

        private void ProgressCancelButton_Click(object sender, EventArgs e) => Cancel();

        private void Cancel()
        {
            switch (_progressTask)
            {
                case ProgressTask.ScanAllFMs:
                    FMScan.CancelScan();
                    break;
                case ProgressTask.InstallFM:
                    FMInstallAndPlay.CancelInstallFM();
                    break;
            }
        }
    }
}
