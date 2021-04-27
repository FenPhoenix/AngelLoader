namespace AngelLoader.Forms
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

#if DEBUG
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.BottomFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
            this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ErrorLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.ErrorIconPictureBox = new System.Windows.Forms.PictureBox();
            this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.MainSplitContainer = new AngelLoader.Forms.CustomControls.SplitContainerCustom();
            this.OtherRadioButton = new AngelLoader.Forms.CustomControls.RadioButtonCustom();
            this.AppearanceRadioButton = new AngelLoader.Forms.CustomControls.RadioButtonCustom();
            this.PathsRadioButton = new AngelLoader.Forms.CustomControls.RadioButtonCustom();
            this.PagePanel = new System.Windows.Forms.Panel();
            this.BottomFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorIconPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomFlowLayoutPanel
            // 
            this.BottomFlowLayoutPanel.Controls.Add(this.Cancel_Button);
            this.BottomFlowLayoutPanel.Controls.Add(this.OKButton);
            this.BottomFlowLayoutPanel.Controls.Add(this.ErrorLabel);
            this.BottomFlowLayoutPanel.Controls.Add(this.ErrorIconPictureBox);
            this.BottomFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.BottomFlowLayoutPanel.Location = new System.Drawing.Point(0, 616);
            this.BottomFlowLayoutPanel.Name = "BottomFlowLayoutPanel";
            this.BottomFlowLayoutPanel.Size = new System.Drawing.Size(694, 40);
            this.BottomFlowLayoutPanel.TabIndex = 4;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(610, 8);
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.AutoSize = true;
            this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(529, 8);
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // ErrorLabel
            // 
            this.ErrorLabel.AutoSize = true;
            this.ErrorLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ErrorLabel.Location = new System.Drawing.Point(462, 12);
            this.ErrorLabel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
            this.ErrorLabel.Name = "ErrorLabel";
            this.ErrorLabel.Size = new System.Drawing.Size(61, 13);
            this.ErrorLabel.TabIndex = 4;
            this.ErrorLabel.Text = "[ErrorLabel]";
            this.ErrorLabel.Visible = false;
            // 
            // ErrorIconPictureBox
            // 
            this.ErrorIconPictureBox.Location = new System.Drawing.Point(445, 12);
            this.ErrorIconPictureBox.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.ErrorIconPictureBox.Name = "ErrorIconPictureBox";
            this.ErrorIconPictureBox.Size = new System.Drawing.Size(14, 14);
            this.ErrorIconPictureBox.TabIndex = 5;
            this.ErrorIconPictureBox.TabStop = false;
            this.ErrorIconPictureBox.Visible = false;
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainSplitContainer.BackColor = System.Drawing.SystemColors.ControlDark;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.MainSplitContainer.Panel1.Controls.Add(this.OtherRadioButton);
            this.MainSplitContainer.Panel1.Controls.Add(this.AppearanceRadioButton);
            this.MainSplitContainer.Panel1.Controls.Add(this.PathsRadioButton);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.MainSplitContainer.Panel2.Controls.Add(this.PagePanel);
            this.MainSplitContainer.Size = new System.Drawing.Size(694, 613);
            this.MainSplitContainer.SplitterDistance = 155;
            this.MainSplitContainer.TabIndex = 5;
            // 
            // OtherRadioButton
            // 
            this.OtherRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OtherRadioButton.BackColor = System.Drawing.SystemColors.Control;
            this.OtherRadioButton.Checked = false;
            this.OtherRadioButton.FlatAppearance.BorderSize = 0;
            this.OtherRadioButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Window;
            this.OtherRadioButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Window;
            this.OtherRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OtherRadioButton.Location = new System.Drawing.Point(8, 56);
            this.OtherRadioButton.Name = "OtherRadioButton";
            this.OtherRadioButton.Size = new System.Drawing.Size(136, 23);
            this.OtherRadioButton.TabIndex = 2;
            this.OtherRadioButton.Text = "Other";
            this.OtherRadioButton.UseVisualStyleBackColor = true;
            this.OtherRadioButton.CheckedChanged += new System.EventHandler(this.PathsRadioButton_CheckedChanged);
            this.OtherRadioButton.Click += new System.EventHandler(this.PageRadioButtons_Click);
            this.OtherRadioButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SectionButtons_MouseDown);
            // 
            // AppearanceRadioButton
            // 
            this.AppearanceRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AppearanceRadioButton.BackColor = System.Drawing.SystemColors.Control;
            this.AppearanceRadioButton.Checked = false;
            this.AppearanceRadioButton.FlatAppearance.BorderSize = 0;
            this.AppearanceRadioButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Window;
            this.AppearanceRadioButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Window;
            this.AppearanceRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AppearanceRadioButton.Location = new System.Drawing.Point(8, 32);
            this.AppearanceRadioButton.Name = "AppearanceRadioButton";
            this.AppearanceRadioButton.Size = new System.Drawing.Size(136, 23);
            this.AppearanceRadioButton.TabIndex = 1;
            this.AppearanceRadioButton.Text = "FM Display";
            this.AppearanceRadioButton.UseVisualStyleBackColor = true;
            this.AppearanceRadioButton.CheckedChanged += new System.EventHandler(this.PathsRadioButton_CheckedChanged);
            this.AppearanceRadioButton.Click += new System.EventHandler(this.PageRadioButtons_Click);
            this.AppearanceRadioButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SectionButtons_MouseDown);
            // 
            // PathsRadioButton
            // 
            this.PathsRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PathsRadioButton.BackColor = System.Drawing.SystemColors.Control;
            this.PathsRadioButton.Checked = false;
            this.PathsRadioButton.FlatAppearance.BorderSize = 0;
            this.PathsRadioButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Window;
            this.PathsRadioButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Window;
            this.PathsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PathsRadioButton.Location = new System.Drawing.Point(8, 8);
            this.PathsRadioButton.Name = "PathsRadioButton";
            this.PathsRadioButton.Size = new System.Drawing.Size(136, 23);
            this.PathsRadioButton.TabIndex = 0;
            this.PathsRadioButton.Text = "Paths";
            this.PathsRadioButton.UseVisualStyleBackColor = true;
            this.PathsRadioButton.CheckedChanged += new System.EventHandler(this.PathsRadioButton_CheckedChanged);
            this.PathsRadioButton.Click += new System.EventHandler(this.PageRadioButtons_Click);
            this.PathsRadioButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SectionButtons_MouseDown);
            // 
            // PagePanel
            // 
            this.PagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PagePanel.Location = new System.Drawing.Point(0, 0);
            this.PagePanel.Name = "PagePanel";
            this.PagePanel.Size = new System.Drawing.Size(535, 613);
            this.PagePanel.TabIndex = 2;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(694, 656);
            this.Controls.Add(this.MainSplitContainer);
            this.Controls.Add(this.BottomFlowLayoutPanel);
            this.DoubleBuffered = true;
            this.Icon = global::AngelLoader.Properties.Resources.AngelLoader;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(540, 320);
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SettingsForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.Shown += new System.EventHandler(this.SettingsForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SettingsForm_KeyDown);
            this.BottomFlowLayoutPanel.ResumeLayout(false);
            this.BottomFlowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorIconPictureBox)).EndInit();
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
#endif

        private System.Windows.Forms.Panel PagePanel;
        private System.Windows.Forms.FlowLayoutPanel BottomFlowLayoutPanel;
        private AngelLoader.Forms.CustomControls.DarkButton Cancel_Button;
        private AngelLoader.Forms.CustomControls.DarkButton OKButton;
        private System.Windows.Forms.ToolTip MainToolTip;
        private AngelLoader.Forms.CustomControls.DarkLabel ErrorLabel;
        private AngelLoader.Forms.CustomControls.SplitContainerCustom MainSplitContainer;
        private AngelLoader.Forms.CustomControls.RadioButtonCustom OtherRadioButton;
        private AngelLoader.Forms.CustomControls.RadioButtonCustom AppearanceRadioButton;
        private AngelLoader.Forms.CustomControls.RadioButtonCustom PathsRadioButton;
        private System.Windows.Forms.PictureBox ErrorIconPictureBox;
    }
}