﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms.WinFormsNative.Taskbar;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class ProgressPanel : UserControl, IDarkable
    {
        #region Consts

        private const ProgressSizeMode _defaultSizeMode = ProgressSizeMode.Single;
        internal static string DefaultCancelMessage => LText.Global.Cancel;
        internal const ProgressType DefaultProgressType = ProgressType.Determinate;

        private const int regularHeight = 128;
        private const int regularHeightWithCheck = regularHeight + 32;
        private const int extendedHeight = 192;
        private const int extendedHeightWithCheck = extendedHeight + 32;

        #endregion

        #region Fields

        private MainForm? _owner;

        private ProgressSizeMode _sizeModeMode = _defaultSizeMode;

        private Action _cancelAction = NullAction;
        private Action<bool> _checkChangedAction = NullBoolAction;

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

                Cancel_Button.DarkModeEnabled = _darkModeEnabled;

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

                MainMessage1Label.ForeColor = fore;
                MainMessage1Label.BackColor = back;
                MainMessage2Label.BackColor = back;
                MainMessage2Label.ForeColor = fore;
                MainPercentLabel.ForeColor = fore;
                MainPercentLabel.BackColor = back;
                MainProgressBar.DarkModeEnabled = _darkModeEnabled;

                SubMessageLabel.BackColor = back;
                SubMessageLabel.ForeColor = fore;
                SubPercentLabel.BackColor = back;
                SubPercentLabel.ForeColor = fore;
                SubProgressBar.DarkModeEnabled = _darkModeEnabled;

                MainCheckBox.DarkModeEnabled = _darkModeEnabled;
            }
        }

        #region Init

        public ProgressPanel()
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif
        }

        internal void InjectOwner(MainForm owner) => _owner = owner;

        internal void SetSizeToDefault() => SetSizeMode(_defaultSizeMode, forceChange: true);

        #endregion

        #region Private methods

        private void SetSizeMode(ProgressSizeMode sizeMode, bool forceChange = false)
        {
            if (!forceChange && sizeMode == _sizeModeMode) return;

            bool doubleSize = sizeMode == ProgressSizeMode.Double;
            bool checkBoxShown = sizeMode is ProgressSizeMode.SingleWithCheck or ProgressSizeMode.DoubleWithCheck;

            Size = Size with
            {
                Height = sizeMode switch
                {
                    ProgressSizeMode.Single => regularHeight,
                    ProgressSizeMode.SingleWithCheck => regularHeightWithCheck,
                    ProgressSizeMode.Double => extendedHeight,
                    _ => extendedHeightWithCheck
                }
            };
            SubMessageLabel.Visible = doubleSize;
            SubPercentLabel.Visible = doubleSize;
            SubProgressBar.Visible = doubleSize;

            MainCheckBox.Visible = checkBoxShown;
            if (!checkBoxShown) _checkChangedAction = NullBoolAction;

            this.CenterHV(_owner!, clientSize: true);

            _sizeModeMode = sizeMode;

            // Necessary otherwise our drawn border doesn't update its size
            Invalidate();
        }

        private void SetProgressBarType(DarkProgressBar progressBar, ProgressType progressType, bool updateTaskbar)
        {
            if (progressType == ProgressType.Indeterminate)
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

        #endregion

        #region Internal methods

        internal void HideThis()
        {
            if (_owner?.IsHandleCreated == true) TaskBarProgress.SetState(_owner.Handle, TaskbarStates.NoProgress);

            Hide();

            MainMessage1Label.Text = "";
            MainMessage2Label.Text = "";
            MainPercentLabel.Text = "";
            MainProgressBar.Value = 0;
            MainProgressBar.Style = ProgressBarStyle.Blocks;

            SubMessageLabel.Text = "";
            SubPercentLabel.Text = "";
            SubProgressBar.Value = 0;
            SubProgressBar.Style = ProgressBarStyle.Blocks;

            MainCheckBox.Hide();
            _checkChangedAction = NullBoolAction;
            // Only set the checked state AFTER setting the null action, otherwise we'll trigger the action!
            MainCheckBox.Checked = false;

            Cancel_Button.Hide();
            _cancelAction = NullAction;

            SetSizeMode(_defaultSizeMode);

            Enabled = false;
            _owner!.EnableEverything(true);
        }

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
        /// <param name="checkChangedAction"></param>
        /// <param name="checkBoxMessage"></param>
        /// <param name="cancelButtonMessage"></param>
        /// <param name="cancelAction">Pass <see cref="T:NullAction"/> to hide the cancel button.</param>
        internal void SetState(
            bool? visible,
            ProgressSizeMode? size,
            string? mainMessage1,
            string? mainMessage2,
            int? mainPercent,
            ProgressType? mainProgressBarType,
            string? subMessage,
            int? subPercent,
            ProgressType? subProgressBarType,
            Action<bool>? checkChangedAction,
            string? checkBoxMessage,
            string? cancelButtonMessage,
            Action? cancelAction)
        {
            if (size != null)
            {
                SetSizeMode((ProgressSizeMode)size);
            }
            if (mainMessage1 != null)
            {
                MainMessage1Label.Text = mainMessage1;
            }
            if (mainMessage2 != null)
            {
                MainMessage2Label.Text = mainMessage2;
            }
            if (mainPercent != null)
            {
                MainPercentLabel.Text = mainPercent + "%";
                SetProgressBarValue(MainProgressBar, (int)mainPercent, updateTaskbar: true);
            }
            if (mainProgressBarType != null)
            {
                SetProgressBarType(MainProgressBar, (ProgressType)mainProgressBarType, updateTaskbar: true);
                if (mainProgressBarType == ProgressType.Indeterminate)
                {
                    MainPercentLabel.Text = "";
                }
            }
            if (subMessage != null)
            {
                SubMessageLabel.Text = subMessage;
            }
            if (subPercent != null)
            {
                SubPercentLabel.Text = subPercent + "%";
                SetProgressBarValue(SubProgressBar, (int)subPercent, updateTaskbar: false);
            }
            if (subProgressBarType != null)
            {
                SetProgressBarType(SubProgressBar, (ProgressType)subProgressBarType, updateTaskbar: false);
                if (subProgressBarType == ProgressType.Indeterminate)
                {
                    SubPercentLabel.Text = "";
                }
            }
            if (checkBoxMessage != null)
            {
                MainCheckBox.Text = checkBoxMessage;
                MainCheckBox.CenterH(this);
            }
            if (checkChangedAction != null)
            {
                _checkChangedAction = checkChangedAction;
                MainCheckBox.Visible = checkChangedAction != NullBoolAction;
            }
            if (cancelButtonMessage != null)
            {
                Cancel_Button.Text = cancelButtonMessage;
                Cancel_Button.CenterH(this);
            }
            if (cancelAction != null)
            {
                _cancelAction = cancelAction;
                Cancel_Button.Visible = cancelAction != NullAction;
            }

            // Put this last so the localization and whatever else can be right
            if (visible != null)
            {
                if (visible == true)
                {
                    _owner!.EnableEverything(false);
                    Enabled = true;

                    BringToFront();
                    Show();
                    Cancel_Button.Focus();
                }
                else
                {
                    HideThis();
                }
            }
        }

        #endregion

        private void MainCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_checkChangedAction != NullBoolAction) _checkChangedAction.Invoke(MainCheckBox.Checked);
        }

        private void ProgressCancelButton_Click(object sender, EventArgs e)
        {
            if (_cancelAction != NullAction) _cancelAction.Invoke();
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
