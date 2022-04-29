using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.Forms.WinFormsNative.Taskbar;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class ProgressPanel : UserControl, IDarkable
    {
        // TODO(ProgressPanel): Make this more general and flexible

        #region Fields etc.

        private MainForm? _owner;
        private ProgressTask _progressTask;

        #endregion

        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                ProgressCancelButton.DarkModeEnabled = _darkModeEnabled;

                Color back, fore;

                if (_darkModeEnabled)
                {
                    // Use a lighter background to make it easy to see we're supposed to be in front and modal
                    back = DarkColors.LightBackground;
                    fore = DarkColors.LightText;
                }
                else
                {
                    back = SystemColors.Control;
                    fore = SystemColors.ControlText;
                }

                BackColor = back;
                ForeColor = fore;

                CurrentThingLabel.BackColor = back;
                CurrentThingLabel.ForeColor = fore;
                ProgressMessageLabel.ForeColor = fore;
                ProgressMessageLabel.BackColor = back;
                ProgressPercentLabel.ForeColor = fore;
                ProgressPercentLabel.BackColor = back;
                ProgressBar.DarkModeEnabled = _darkModeEnabled;

                CurrentSubThingLabel.BackColor = back;
                CurrentSubThingLabel.ForeColor = fore;
                SubProgressPercentLabel.BackColor = back;
                SubProgressPercentLabel.ForeColor = fore;
                SubProgressBar.DarkModeEnabled = _darkModeEnabled;
            }
        }

        public ProgressPanel()
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif
        }

        internal void InjectOwner(MainForm owner) => _owner = owner;

        #region Open/close

        internal void ShowProgressWindow(ProgressTask progressTask, bool suppressShow)
        {
            const int regularHeight = 128;
            const int extendedHeight = 192;

            _progressTask = progressTask;

            bool useExtendedHeight = _progressTask == ProgressTask.InstallFMs;

            Size = Size with { Height = useExtendedHeight ? extendedHeight : regularHeight };
            CurrentSubThingLabel.Visible = useExtendedHeight;
            SubProgressPercentLabel.Visible = useExtendedHeight;
            SubProgressBar.Visible = useExtendedHeight;

            this.CenterHV(_owner!, clientSize: true);

            ProgressBar.CenterH(this);
            SubProgressBar.CenterH(this);

            ProgressMessageLabel.Text = progressTask switch
            {
                ProgressTask.FMScan => LText.ProgressBox.Scanning,
                ProgressTask.InstallFM => LText.ProgressBox.InstallingFM,
                ProgressTask.InstallFMs => LText.ProgressBox.InstallingFMs,
                ProgressTask.UninstallFM => LText.ProgressBox.UninstallingFM,
                ProgressTask.UninstallFMs => LText.ProgressBox.UninstallingFMs,
                ProgressTask.ConvertFiles or
                ProgressTask.ConvertFilesManual => LText.ProgressBox.ConvertingFiles,
                ProgressTask.ImportFromDarkLoader => LText.ProgressBox.ImportingFromDarkLoader,
                ProgressTask.ImportFromNDL => LText.ProgressBox.ImportingFromNewDarkLoader,
                ProgressTask.ImportFromFMSel => LText.ProgressBox.ImportingFromFMSel,
                ProgressTask.CacheFM => LText.ProgressBox.CachingReadmeFiles,
                ProgressTask.DeleteFMArchive => LText.ProgressBox.DeletingFMArchive,
                ProgressTask.CheckingFreeSpace => LText.ProgressBox.CheckingFreeSpace,
                _ => ""
            };

            CurrentThingLabel.Text =
                progressTask == ProgressTask.FMScan ? LText.ProgressBox.PreparingToScanFMs
                : "";

            ProgressPercentLabel.Text = "";

            if (progressTask
                is ProgressTask.UninstallFM
                or ProgressTask.UninstallFMs
                or ProgressTask.ConvertFiles
                or ProgressTask.ConvertFilesManual
                or ProgressTask.ImportFromDarkLoader
                or ProgressTask.ImportFromNDL
                or ProgressTask.ImportFromFMSel
                or ProgressTask.DeleteFMArchive
                or ProgressTask.CheckingFreeSpace)
            {
                ProgressBar.Style = ProgressBarStyle.Marquee;
                if (_owner?.IsHandleCreated == true) TaskBarProgress.SetState(_owner.Handle, TaskbarStates.Indeterminate);
                if (progressTask != ProgressTask.ConvertFilesManual)
                {
                    ProgressCancelButton.Hide();
                }
            }
            else
            {
                ProgressBar.Style = ProgressBarStyle.Blocks;
                ProgressCancelButton.Visible = progressTask != ProgressTask.CacheFM;
                ProgressBar.Value = 0;
            }

            if (!suppressShow) ShowThis();
        }

        private void ShowThis()
        {
            _owner!.EnableEverything(false);
            Enabled = true;

            BringToFront();
            Show();
            ProgressCancelButton.Focus();

            Localize();
        }

        internal void HideThis()
        {
            if (_owner?.IsHandleCreated == true) TaskBarProgress.SetState(_owner.Handle, TaskbarStates.NoProgress);

            Hide();

            ProgressMessageLabel.Text = "";

            CurrentThingLabel.Text = "";
            ProgressPercentLabel.Text = "";
            ProgressBar.Value = 0;
            ProgressBar.Style = ProgressBarStyle.Blocks;

            CurrentSubThingLabel.Text = "";
            SubProgressPercentLabel.Text = "";
            SubProgressBar.Value = 0;
            SubProgressBar.Style = ProgressBarStyle.Blocks;

            ProgressCancelButton.Show();

            Enabled = false;
            _owner!.EnableEverything(true);
        }

        #endregion

        #region Reporting

        internal void SetCurrentThingMessage(string message)
        {
            CurrentThingLabel.Text = message;
        }

        internal void ReportScanProgress(int fmNumber, int fmsTotal, int percent, string fmName)
        {
            ProgressBar.Value = percent;
            string first = LText.ProgressBox.ReportScanningFirst;
            string between = LText.ProgressBox.ReportScanningBetweenNumAndTotal;
            string last = LText.ProgressBox.ReportScanningLast;
            ProgressMessageLabel.Text = first + fmNumber + between + fmsTotal + last;
            CurrentThingLabel.Text = fmName;
            ProgressPercentLabel.Text = percent + "%";

            if (_owner?.IsHandleCreated == true) TaskBarProgress.SetValue(_owner.Handle, percent, 100);
        }

        internal void ReportFMInstallProgress(int percent)
        {
            ProgressBar.Value = percent;
            ProgressMessageLabel.Text = LText.ProgressBox.InstallingFM;
            ProgressPercentLabel.Text = percent + "%";

            if (_owner?.IsHandleCreated == true) TaskBarProgress.SetValue(_owner.Handle, percent, 100);
        }

        /// <summary>
        /// For the percents, -1 means don't update the displayed values.
        /// </summary>
        /// <param name="mainPercent"></param>
        /// <param name="subPercent"></param>
        /// <param name="fmName"></param>
        /// <param name="subMessage"></param>
        internal void ReportMultiFMInstallProgress(int mainPercent, int subPercent, string fmName, string subMessage = "")
        {
            if (mainPercent > -1)
            {
                ProgressBar.Value = mainPercent;
                ProgressPercentLabel.Text = mainPercent + "%";
            }
            ProgressMessageLabel.Text = LText.ProgressBox.InstallingFMs;

            if (subPercent > -1)
            {
                SubProgressBar.Value = subPercent;
                SubProgressPercentLabel.Text = !subMessage.IsEmpty() ? subMessage : subPercent + "%";
            }
            else
            {
                if (!subMessage.IsEmpty()) SubProgressPercentLabel.Text = subMessage;
            }
            CurrentSubThingLabel.Text = fmName;

            if (_owner?.IsHandleCreated == true) TaskBarProgress.SetValue(_owner.Handle, mainPercent, 100);
        }

        internal void ReportCachingProgress(int percent)
        {
            ProgressBar.Value = percent;
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
            ProgressBar.Value = 0;
            ProgressPercentLabel.Text = "";
        }

        #endregion

        internal void Localize()
        {
            ProgressCancelButton.Text = _progressTask == ProgressTask.ConvertFilesManual
                ? LText.Global.Stop
                : LText.Global.Cancel;
            ProgressCancelButton.CenterH(this);
        }

        private void ProgressCancelButton_Click(object sender, EventArgs e) => Cancel();

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
        private void Cancel()
        {
            switch (_progressTask)
            {
                case ProgressTask.FMScan:
                    FMScan.CancelScan();
                    break;
                case ProgressTask.InstallFM:
                case ProgressTask.InstallFMs:
                    FMInstallAndPlay.CancelInstallFM();
                    break;
                case ProgressTask.ConvertFilesManual:
                    FMAudio.StopConversion();
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_darkModeEnabled && BorderStyle == BorderStyle.FixedSingle)
            {
                e.Graphics.DrawRectangle(DarkColors.GreySelectionPen, 0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            }
        }
    }
}
