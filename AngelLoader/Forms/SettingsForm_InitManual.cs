using System.Windows.Forms;

namespace AngelLoader.Forms
{
    internal partial class SettingsForm
    {
        private void InitComponentManual()
        {
            components = new System.ComponentModel.Container();
            BottomFlowLayoutPanel = new FlowLayoutPanel();
            Cancel_Button = new Button();
            OKButton = new Button();
            ErrorLabel = new Label();
            MainToolTip = new ToolTip(components);
            MainErrorProvider = new ErrorProvider(components);
            MainSplitContainer = new CustomControls.SplitContainerCustom();
            OtherRadioButton = new CustomControls.RadioButtonCustom();
            FMDisplayRadioButton = new CustomControls.RadioButtonCustom();
            PathsRadioButton = new CustomControls.RadioButtonCustom();
            PagePanel = new Panel();
            BottomFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MainErrorProvider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MainSplitContainer).BeginInit();
            MainSplitContainer.Panel1.SuspendLayout();
            MainSplitContainer.Panel2.SuspendLayout();
            MainSplitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // BottomFlowLayoutPanel
            // 
            BottomFlowLayoutPanel.Controls.Add(Cancel_Button);
            BottomFlowLayoutPanel.Controls.Add(OKButton);
            BottomFlowLayoutPanel.Controls.Add(ErrorLabel);
            BottomFlowLayoutPanel.Dock = DockStyle.Bottom;
            BottomFlowLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
            BottomFlowLayoutPanel.Location = new System.Drawing.Point(0, 616);
            BottomFlowLayoutPanel.Size = new System.Drawing.Size(694, 40);
            BottomFlowLayoutPanel.TabIndex = 4;
            // 
            // Cancel_Button
            // 
            Cancel_Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Cancel_Button.AutoSize = true;
            Cancel_Button.DialogResult = DialogResult.Cancel;
            Cancel_Button.Location = new System.Drawing.Point(610, 8);
            Cancel_Button.Margin = new Padding(3, 8, 9, 3);
            Cancel_Button.Padding = new Padding(6, 0, 6, 0);
            Cancel_Button.Size = new System.Drawing.Size(75, 23);
            Cancel_Button.TabIndex = 0;
            Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            OKButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            OKButton.AutoSize = true;
            OKButton.DialogResult = DialogResult.OK;
            OKButton.Location = new System.Drawing.Point(529, 8);
            OKButton.Margin = new Padding(3, 8, 3, 3);
            OKButton.Padding = new Padding(6, 0, 6, 0);
            OKButton.Size = new System.Drawing.Size(75, 23);
            OKButton.TabIndex = 1;
            OKButton.UseVisualStyleBackColor = true;
            // 
            // ErrorLabel
            // 
            ErrorLabel.AutoSize = true;
            ErrorLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            MainErrorProvider.SetIconAlignment(ErrorLabel, ErrorIconAlignment.MiddleLeft);
            ErrorLabel.Location = new System.Drawing.Point(462, 12);
            ErrorLabel.Margin = new Padding(3, 12, 3, 0);
            ErrorLabel.Size = new System.Drawing.Size(61, 13);
            ErrorLabel.TabIndex = 4;
            ErrorLabel.Visible = false;
            // 
            // MainErrorProvider
            // 
            MainErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            MainErrorProvider.ContainerControl = this;
            // 
            // MainSplitContainer
            // 
            MainSplitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                                                         | AnchorStyles.Left
                                                         | AnchorStyles.Right;
            MainSplitContainer.BackColor = System.Drawing.SystemColors.ControlDark;
            MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            // 
            // MainSplitContainer.Panel1
            // 
            MainSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
            MainSplitContainer.Panel1.Controls.Add(OtherRadioButton);
            MainSplitContainer.Panel1.Controls.Add(FMDisplayRadioButton);
            MainSplitContainer.Panel1.Controls.Add(PathsRadioButton);
            // 
            // MainSplitContainer.Panel2
            // 
            MainSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            MainSplitContainer.Panel2.Controls.Add(PagePanel);
            MainSplitContainer.Size = new System.Drawing.Size(694, 613);
            MainSplitContainer.SplitterDistance = 155;
            MainSplitContainer.TabIndex = 5;
            // 
            // OtherRadioButton
            // 
            OtherRadioButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OtherRadioButton.BackColor = System.Drawing.SystemColors.Control;
            OtherRadioButton.Checked = false;
            OtherRadioButton.FlatAppearance.BorderSize = 0;
            OtherRadioButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Window;
            OtherRadioButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Window;
            OtherRadioButton.FlatStyle = FlatStyle.Flat;
            OtherRadioButton.Location = new System.Drawing.Point(8, 56);
            OtherRadioButton.Size = new System.Drawing.Size(136, 23);
            OtherRadioButton.TabIndex = 2;
            OtherRadioButton.UseVisualStyleBackColor = true;
            OtherRadioButton.CheckedChanged += PathsRadioButton_CheckedChanged;
            OtherRadioButton.Click += PageRadioButtons_Click;
            OtherRadioButton.MouseDown += Paths_RadioButton_MouseDown;
            // 
            // FMDisplayRadioButton
            // 
            FMDisplayRadioButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FMDisplayRadioButton.BackColor = System.Drawing.SystemColors.Control;
            FMDisplayRadioButton.Checked = false;
            FMDisplayRadioButton.FlatAppearance.BorderSize = 0;
            FMDisplayRadioButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Window;
            FMDisplayRadioButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Window;
            FMDisplayRadioButton.FlatStyle = FlatStyle.Flat;
            FMDisplayRadioButton.Location = new System.Drawing.Point(8, 32);
            FMDisplayRadioButton.Size = new System.Drawing.Size(136, 23);
            FMDisplayRadioButton.TabIndex = 1;
            FMDisplayRadioButton.UseVisualStyleBackColor = true;
            FMDisplayRadioButton.CheckedChanged += PathsRadioButton_CheckedChanged;
            FMDisplayRadioButton.Click += PageRadioButtons_Click;
            FMDisplayRadioButton.MouseDown += Paths_RadioButton_MouseDown;
            // 
            // PathsRadioButton
            // 
            PathsRadioButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PathsRadioButton.BackColor = System.Drawing.SystemColors.Control;
            PathsRadioButton.Checked = false;
            PathsRadioButton.FlatAppearance.BorderSize = 0;
            PathsRadioButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Window;
            PathsRadioButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Window;
            PathsRadioButton.FlatStyle = FlatStyle.Flat;
            PathsRadioButton.Location = new System.Drawing.Point(8, 8);
            PathsRadioButton.Size = new System.Drawing.Size(136, 23);
            PathsRadioButton.TabIndex = 0;
            PathsRadioButton.UseVisualStyleBackColor = true;
            PathsRadioButton.CheckedChanged += PathsRadioButton_CheckedChanged;
            PathsRadioButton.Click += PageRadioButtons_Click;
            PathsRadioButton.MouseDown += Paths_RadioButton_MouseDown;
            // 
            // PagePanel
            // 
            PagePanel.Dock = DockStyle.Fill;
            PagePanel.Location = new System.Drawing.Point(0, 0);
            PagePanel.Size = new System.Drawing.Size(535, 613);
            PagePanel.TabIndex = 2;
            // 
            // SettingsForm
            // 
            AcceptButton = OKButton;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(694, 656);
            Controls.Add(MainSplitContainer);
            Controls.Add(BottomFlowLayoutPanel);
            DoubleBuffered = true;
            Icon = Images.AngelLoader;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(540, 320);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            // Un-obvious hack: If we DON'T set Text to something, anything, here, then first render (if paths tab
            // is the startup tab) is really slow. We just set a one-char blank space to prevent that(?!) Probably
            // something to do with this activating some kind of render routine beforehand... I guess... who knows...
            Text = @" ";
            FormClosing += SettingsForm_FormClosing;
            Load += SettingsForm_Load;
            Shown += SettingsForm_Shown;
            KeyDown += SettingsForm_KeyDown;
            BottomFlowLayoutPanel.ResumeLayout(false);
            BottomFlowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)MainErrorProvider).EndInit();
            MainSplitContainer.Panel1.ResumeLayout(false);
            MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)MainSplitContainer).EndInit();
            MainSplitContainer.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
