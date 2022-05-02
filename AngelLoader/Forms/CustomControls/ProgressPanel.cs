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

        private ProgressBoxCancelButtonType _cancelButtonType = ProgressBoxCancelButtonType.Cancel;

        private Action _cancelAction = NullAction;

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

        private const int regularHeight = 128;
        private const int extendedHeight = 192;

        private void SetSizeMode(bool doubleSize)
        {
            Size = Size with { Height = doubleSize ? extendedHeight : regularHeight };
            CurrentSubThingLabel.Visible = doubleSize;
            SubProgressPercentLabel.Visible = doubleSize;
            SubProgressBar.Visible = doubleSize;

            this.CenterHV(_owner!, clientSize: true);
        }

        #region Open/close

        internal void ShowProgressWindow(ProgressTask progressTask, bool suppressShow)
        {
            _progressTask = progressTask;

            SetSizeMode(doubleSize: _progressTask == ProgressTask.InstallFMs);

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
                ProgressTask.CheckingFreeSpace => LText.ProgressBox.CheckingFreeSpace,
                ProgressTask.PreparingInstall => LText.ProgressBox.PreparingToInstall,
                ProgressTask.RestoringBackup => LText.ProgressBox.RestoringBackup,
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
                or ProgressTask.CheckingFreeSpace
                or ProgressTask.PreparingInstall
                or ProgressTask.RestoringBackup)
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

        private void SetProgressBarType(DarkProgressBar progressBar, ProgressBarType progressBarType, bool updateTaskbar)
        {
            if (progressBarType == ProgressBarType.Indeterminate)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                if (updateTaskbar)
                {
                    if (_owner?.IsHandleCreated == true) TaskBarProgress.SetState(_owner.Handle, TaskbarStates.Indeterminate);
                }
            }
            else
            {
                progressBar.Style = ProgressBarStyle.Blocks;
                if (updateTaskbar)
                {
                    if (_owner?.IsHandleCreated == true) TaskBarProgress.SetState(_owner.Handle, TaskbarStates.Indeterminate);
                }
            }
        }

        private void SetProgressBarValue(DarkProgressBar progressBar, int value, bool updateTaskbar)
        {
            progressBar.Value = value;
            if (updateTaskbar)
            {
                if (_owner?.IsHandleCreated == true) TaskBarProgress.SetValue(_owner.Handle, value, 100);
            }
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

            _cancelAction = NullAction;

            Enabled = false;
            _owner!.EnableEverything(true);
        }

        #endregion
        /// <summary>
        /// Sets the state of the progress box. A null parameter means no change.
        /// </summary>
        /// <param name="visible"></param>
        /// <param name="size"></param>
        /// <param name="mainMessage1"></param>
        /// <param name="mainMessage2"></param>
        /// <param name="mainPercent"></param>
        /// <param name="mainProgressBarType"></param>
        /// <param name="subMessage"></param>
        /// <param name="subPercent"></param>
        /// <param name="subProgressBarType"></param>
        /// <param name="cancelButtonType"></param>
        /// <param name="cancelAction">Pass <see cref="NullAction"/> to hide the cancel button.</param>
        internal void SetState(
            bool? visible,
            ProgressSize? size,
            string? mainMessage1,
            string? mainMessage2,
            int? mainPercent,
            ProgressBarType? mainProgressBarType,
            string? subMessage,
            int? subPercent,
            ProgressBarType? subProgressBarType,
            ProgressBoxCancelButtonType? cancelButtonType,
            Action? cancelAction)
        {
            if (visible != null)
            {
                if (visible == true)
                {
                    ShowThis();
                }
                else
                {
                    HideThis();
                }
            }
            if (size != null)
            {
                SetSizeMode(doubleSize: size == ProgressSize.Double);
            }
            if (mainMessage1 != null)
            {
                ProgressMessageLabel.Text = mainMessage1;
            }
            if (mainMessage2 != null)
            {
                CurrentThingLabel.Text = mainMessage2;
            }
            if (mainPercent != null)
            {
                ProgressPercentLabel.Text = mainPercent + "%";
                SetProgressBarValue(ProgressBar, (int)mainPercent, updateTaskbar: true);
            }
            if (mainProgressBarType != null)
            {
                SetProgressBarType(ProgressBar, (ProgressBarType)mainProgressBarType, updateTaskbar: true);
                if (mainProgressBarType == ProgressBarType.Indeterminate)
                {
                    ProgressPercentLabel.Text = "";
                }
            }
            if (subMessage != null)
            {
                CurrentSubThingLabel.Text = subMessage;
            }
            if (subPercent != null)
            {
                SubProgressPercentLabel.Text = subPercent + "%";
                SetProgressBarValue(SubProgressBar, (int)subPercent, updateTaskbar: false);
            }
            if (subProgressBarType != null)
            {
                SetProgressBarType(SubProgressBar, (ProgressBarType)subProgressBarType, updateTaskbar: false);
                if (subProgressBarType == ProgressBarType.Indeterminate)
                {
                    SubProgressPercentLabel.Text = "";
                }
            }
            if (cancelAction != null)
            {
                _cancelAction = cancelAction;
                ProgressCancelButton.Visible = cancelAction != NullAction;
            }
            if (cancelButtonType != null)
            {
                _cancelButtonType = (ProgressBoxCancelButtonType)cancelButtonType;
                Localize();
            }
        }

        internal void Localize()
        {
            ProgressCancelButton.Text = _cancelButtonType == ProgressBoxCancelButtonType.Stop
                ? LText.Global.Stop
                : LText.Global.Cancel;
            ProgressCancelButton.CenterH(this);
        }

        private void ProgressCancelButton_Click(object sender, EventArgs e) => Cancel();

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
        private void Cancel()
        {
            if (_cancelAction != NullAction)
            {
                _cancelAction.Invoke();
            }
            else
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
