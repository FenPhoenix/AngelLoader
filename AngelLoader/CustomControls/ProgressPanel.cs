using System;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Forms;
using AngelLoader.WinAPI.Taskbar;

namespace AngelLoader.CustomControls
{
    public partial class ProgressPanel : UserControl, ILocalizable
    {
        // TODO: The way this works is no longer really tenable - rework it to be cleaner

        #region Fields etc.

        internal enum ProgressTasks
        {
            ScanAllFMs,
            InstallFM,
            UninstallFM,
            ConvertFiles,
            ImportFromDarkLoader,
            ImportFromNDL,
            ImportFromFMSel,
            CacheFM
        }

        private MainForm Owner;
        internal ProgressTasks ProgressTask { get; set; }

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
            ProgressTask = ProgressTasks.ImportFromDarkLoader;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowImportNDL()
        {
            ProgressTask = ProgressTasks.ImportFromNDL;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowImportFMSel()
        {
            ProgressTask = ProgressTasks.ImportFromFMSel;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowScanningAllFMs()
        {
            ProgressTask = ProgressTasks.ScanAllFMs;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowInstallingFM()
        {
            ProgressTask = ProgressTasks.InstallFM;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowUninstallingFM()
        {
            ProgressTask = ProgressTasks.UninstallFM;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowConvertingFiles()
        {
            ProgressTask = ProgressTasks.ConvertFiles;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowCachingFM()
        {
            ProgressTask = ProgressTasks.CacheFM;
            ShowProgressWindow(ProgressTask);
        }

        #endregion

        #region Open/close

        internal void ShowProgressWindow(ProgressTasks progressTask, bool suppressShow = false)
        {
            Center();

            ProgressMessageLabel.Text =
                progressTask == ProgressTasks.ScanAllFMs ? LText.ProgressBox.Scanning :
                progressTask == ProgressTasks.InstallFM ? LText.ProgressBox.InstallingFM :
                progressTask == ProgressTasks.UninstallFM ? LText.ProgressBox.UninstallingFM :
                progressTask == ProgressTasks.ConvertFiles ? LText.ProgressBox.ConvertingFiles :
                progressTask == ProgressTasks.ImportFromDarkLoader ? LText.ProgressBox.ImportingFromDarkLoader :
                progressTask == ProgressTasks.ImportFromNDL ? LText.ProgressBox.ImportingFromNewDarkLoader :
                progressTask == ProgressTasks.ImportFromFMSel ? LText.ProgressBox.ImportingFromFMSel :
                progressTask == ProgressTasks.CacheFM ? LText.ProgressBox.CachingReadmeFiles :
                "";

            CurrentThingLabel.Text =
                progressTask == ProgressTasks.ScanAllFMs ? LText.ProgressBox.CheckingInstalledFMs
                : "";

            if (progressTask == ProgressTasks.UninstallFM ||
                progressTask == ProgressTasks.ConvertFiles ||
                progressTask == ProgressTasks.ImportFromDarkLoader ||
                progressTask == ProgressTasks.ImportFromNDL ||
                progressTask == ProgressTasks.ImportFromFMSel)
            {
                ProgressBar.Style = ProgressBarStyle.Marquee;
                TaskBarProgress.SetState(Owner.Handle, TaskbarStates.Indeterminate);
                ProgressCancelButton.Hide();
            }
            else
            {
                ProgressBar.Style = ProgressBarStyle.Blocks;
                ProgressCancelButton.Visible = progressTask != ProgressTasks.CacheFM;
                ProgressPercentLabel.Text = "";
                ProgressBar.SetValueInstant(0);
            }

            if (!suppressShow)
            {
                ShowThis();
            }
        }

        internal void ShowThis()
        {
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

            ProgressBar.Style = ProgressBarStyle.Blocks;
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

        internal void ReportCachingProgress(int percent)
        {
            ProgressBar.SetValueInstant(percent.Clamp(0, 100));
            ProgressPercentLabel.Text = percent + "%";

            if (Visible)
            {
                TaskBarProgress.SetValue(Owner.Handle, percent, 100);
            }
            else
            {
                TaskBarProgress.SetState(Owner.Handle, TaskbarStates.NoProgress);
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

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            ProgressCancelButton.SetTextAutoSize(LText.Global.Cancel, ProgressCancelButton.Width);
            ProgressCancelButton.CenterH(this);
        }

        internal void Center() => this.CenterHV(Owner, clientSize: true);

        private void ProgressCancelButton_Click(object sender, EventArgs e) => Cancel();

        internal void Cancel()
        {
            switch (ProgressTask)
            {
                case ProgressTasks.ScanAllFMs:
                    Owner.CancelScan();
                    break;
                case ProgressTasks.InstallFM:
                    Owner.CancelInstallFM();
                    break;
            }
        }
    }
}
