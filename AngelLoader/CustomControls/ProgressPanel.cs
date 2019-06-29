using System;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Forms;
using AngelLoader.WinAPI.Taskbar;
using static AngelLoader.Common.Logger;

namespace AngelLoader.CustomControls
{
    public partial class ProgressPanel : UserControl, ILocalizable
    {
        // TODO: The way this works is no longer really tenable - rework it to be cleaner

        #region Fields etc.

        // PERF_TODO: This bool prevents layout until show which is good, but...
        // ...a better idea would be to make this whole thing lazy-loaded. It's the perfect candidate for it.
        private bool _shownOnce;

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
        internal ProgressTasks ProgressTask;

        #endregion

        public ProgressPanel() => InitializeComponent();

        internal void Inject(MainForm owner) => Owner = owner;

        #region Show methods

        internal void ShowImportDarkLoader()
        {
            Log("ProgressBox: " + nameof(ShowImportDarkLoader), methodName: false);
            ProgressTask = ProgressTasks.ImportFromDarkLoader;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowImportNDL()
        {
            Log("ProgressBox: " + nameof(ShowImportNDL), methodName: false);
            ProgressTask = ProgressTasks.ImportFromNDL;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowImportFMSel()
        {
            Log("ProgressBox: " + nameof(ShowImportFMSel), methodName: false);
            ProgressTask = ProgressTasks.ImportFromFMSel;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowScanningAllFMs()
        {
            Log("ProgressBox: " + nameof(ShowScanningAllFMs), methodName: false);
            ProgressTask = ProgressTasks.ScanAllFMs;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowInstallingFM()
        {
            Log("ProgressBox: " + nameof(ShowInstallingFM), methodName: false);
            ProgressTask = ProgressTasks.InstallFM;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowUninstallingFM()
        {
            Log("ProgressBox: " + nameof(ShowUninstallingFM), methodName: false);
            ProgressTask = ProgressTasks.UninstallFM;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowConvertingFiles()
        {
            Log("ProgressBox: " + nameof(ShowConvertingFiles), methodName: false);
            ProgressTask = ProgressTasks.ConvertFiles;
            ShowProgressWindow(ProgressTask);
        }

        internal void ShowCachingFM()
        {
            Log("ProgressBox: " + nameof(ShowCachingFM), methodName: false);
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
                if (Owner != null && Owner.IsHandleCreated) TaskBarProgress.SetState(Owner.Handle, TaskbarStates.Indeterminate);
                ProgressCancelButton.Hide();
            }
            else
            {
                ProgressBar.Style = ProgressBarStyle.Blocks;
                ProgressCancelButton.Visible = progressTask != ProgressTasks.CacheFM;
                ProgressPercentLabel.Text = "";
                ProgressBar.SetValueInstant(0);
            }

            if (!suppressShow) ShowThis();
        }

        internal void ShowThis()
        {
            if (!_shownOnce)
            {
                _shownOnce = true;
                SetUITextToLocalized();
                ProgressCancelButton.CenterH(this);
            }

            Log(nameof(ShowThis) + " called", methodName: false);
            Owner.EnableEverything(false);
            Enabled = true;

            BringToFront();
            Show();
        }

        internal void HideThis()
        {
            if (Owner != null && Owner.IsHandleCreated) TaskBarProgress.SetState(Owner.Handle, TaskbarStates.NoProgress);

            Hide();

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
            ProgressPercentLabel.Text = percent + @"%";

            if (Owner != null && Owner.IsHandleCreated) TaskBarProgress.SetValue(Owner.Handle, percent, 100);
        }

        internal void ReportFMExtractProgress(int percent)
        {
            ProgressBar.SetValueInstant(percent.Clamp(0, 100));
            ProgressMessageLabel.Text = LText.ProgressBox.InstallingFM;
            ProgressPercentLabel.Text = percent + @"%";

            if (Owner != null && Owner.IsHandleCreated) TaskBarProgress.SetValue(Owner.Handle, percent, 100);
        }

        internal void ReportCachingProgress(int percent)
        {
            ProgressBar.SetValueInstant(percent.Clamp(0, 100));
            ProgressPercentLabel.Text = percent + @"%";

            if (Visible)
            {
                if (Owner != null && Owner.IsHandleCreated) TaskBarProgress.SetValue(Owner.Handle, percent, 100);
            }
            else
            {
                if (Owner != null && Owner.IsHandleCreated) TaskBarProgress.SetState(Owner.Handle, TaskbarStates.NoProgress);
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
            if (!_shownOnce) return;

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
