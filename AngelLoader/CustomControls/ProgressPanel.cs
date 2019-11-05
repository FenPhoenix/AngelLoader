using System;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Forms;
using AngelLoader.WinAPI.Taskbar;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Misc;

namespace AngelLoader.CustomControls
{
    public partial class ProgressPanel : UserControl
    {
        // TODO: The way this works is no longer really tenable - rework it to be cleaner

        #region Fields etc.

        private MainForm? Owner;
        private ProgressTasks ProgressTask;

        #endregion

        public ProgressPanel() => InitializeComponent();

        internal void Inject(MainForm owner) => Owner = owner;

        #region Open/close

        internal void ShowProgressWindow(ProgressTasks progressTask, bool suppressShow = false)
        {
            ProgressTask = progressTask;

            this.CenterHV(Owner!, clientSize: true);

            ProgressMessageLabel.Text = progressTask switch
            {
                ProgressTasks.ScanAllFMs => LText.ProgressBox.Scanning,
                ProgressTasks.InstallFM => LText.ProgressBox.InstallingFM,
                ProgressTasks.UninstallFM => LText.ProgressBox.UninstallingFM,
                ProgressTasks.ConvertFiles => LText.ProgressBox.ConvertingFiles,
                ProgressTasks.ImportFromDarkLoader => LText.ProgressBox.ImportingFromDarkLoader,
                ProgressTasks.ImportFromNDL => LText.ProgressBox.ImportingFromNewDarkLoader,
                ProgressTasks.ImportFromFMSel => LText.ProgressBox.ImportingFromFMSel,
                ProgressTasks.CacheFM => LText.ProgressBox.CachingReadmeFiles,
                _ => ""
            };

            CurrentThingLabel.Text =
                progressTask == ProgressTasks.ScanAllFMs ? LText.ProgressBox.CheckingInstalledFMs
                : "";

            ProgressPercentLabel.Text = "";

            if (progressTask == ProgressTasks.UninstallFM ||
                progressTask == ProgressTasks.ConvertFiles ||
                progressTask == ProgressTasks.ImportFromDarkLoader ||
                progressTask == ProgressTasks.ImportFromNDL ||
                progressTask == ProgressTasks.ImportFromFMSel)
            {
                ProgressBar.Style = ProgressBarStyle.Marquee;
                if (Owner?.IsHandleCreated == true) TaskBarProgress.SetState(Owner.Handle, TaskbarStates.Indeterminate);
                ProgressCancelButton.Hide();
            }
            else
            {
                ProgressBar.Style = ProgressBarStyle.Blocks;
                ProgressCancelButton.Visible = progressTask != ProgressTasks.CacheFM;
                ProgressBar.SetValueInstant(0);
            }

            if (!suppressShow) ShowThis();
        }

        private void ShowThis()
        {
            Log(nameof(ShowThis) + " called", methodName: false);
            Owner!.EnableEverything(false);
            Enabled = true;

            BringToFront();
            Show();
        }

        internal void HideThis()
        {
            if (Owner?.IsHandleCreated == true) TaskBarProgress.SetState(Owner.Handle, TaskbarStates.NoProgress);

            Hide();

            ProgressMessageLabel.Text = "";
            CurrentThingLabel.Text = "";
            ProgressPercentLabel.Text = "";
            ProgressBar.SetValueInstant(0);

            ProgressBar.Style = ProgressBarStyle.Blocks;
            ProgressCancelButton.Show();

            Enabled = false;
            Owner!.EnableEverything(true);
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
            ProgressPercentLabel.Text = percent + @"%";

            if (Owner?.IsHandleCreated == true) TaskBarProgress.SetValue(Owner.Handle, percent, 100);
        }

        internal void ReportFMExtractProgress(int percent)
        {
            ProgressBar.SetValueInstant(percent.Clamp(0, 100));
            ProgressMessageLabel.Text = LText.ProgressBox.InstallingFM;
            ProgressPercentLabel.Text = percent + @"%";

            if (Owner?.IsHandleCreated == true) TaskBarProgress.SetValue(Owner.Handle, percent, 100);
        }

        internal void ReportCachingProgress(int percent)
        {
            ProgressBar.SetValueInstant(percent.Clamp(0, 100));
            ProgressPercentLabel.Text = percent + @"%";

            if (Visible)
            {
                if (Owner?.IsHandleCreated == true) TaskBarProgress.SetValue(Owner.Handle, percent, 100);
            }
            else
            {
                if (Owner?.IsHandleCreated == true) TaskBarProgress.SetState(Owner.Handle, TaskbarStates.NoProgress);
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
            ProgressCancelButton.SetTextAutoSize(LText.Global.Cancel, ProgressCancelButton.Width);
            ProgressCancelButton.CenterH(this);
        }

        private void ProgressCancelButton_Click(object sender, EventArgs e) => Cancel();

        private void Cancel()
        {
            switch (ProgressTask)
            {
                case ProgressTasks.ScanAllFMs:
                    FMScan.CancelScan();
                    break;
                case ProgressTasks.InstallFM:
                    FMInstallAndPlay.CancelInstallFM();
                    break;
            }
        }
    }
}
