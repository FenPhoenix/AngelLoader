namespace AngelLoader.Forms
{
    partial class ImportFromMultipleInisForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ChooseIniFilesLabel = new System.Windows.Forms.Label();
            this.Thief1GroupBox = new System.Windows.Forms.GroupBox();
            this.Thief1IniBrowseButton = new System.Windows.Forms.Button();
            this.Thief1IniTextBox = new System.Windows.Forms.TextBox();
            this.Thief2GroupBox = new System.Windows.Forms.GroupBox();
            this.Thief2IniBrowseButton = new System.Windows.Forms.Button();
            this.Thief2IniTextBox = new System.Windows.Forms.TextBox();
            this.Thief3GroupBox = new System.Windows.Forms.GroupBox();
            this.Thief3IniBrowseButton = new System.Windows.Forms.Button();
            this.Thief3IniTextBox = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.OKCancelFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.Thief1GroupBox.SuspendLayout();
            this.Thief2GroupBox.SuspendLayout();
            this.Thief3GroupBox.SuspendLayout();
            this.OKCancelFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ChooseIniFilesLabel
            // 
            this.ChooseIniFilesLabel.AutoSize = true;
            this.ChooseIniFilesLabel.Location = new System.Drawing.Point(16, 8);
            this.ChooseIniFilesLabel.Name = "ChooseIniFilesLabel";
            this.ChooseIniFilesLabel.Size = new System.Drawing.Size(86, 13);
            this.ChooseIniFilesLabel.TabIndex = 1;
            this.ChooseIniFilesLabel.Text = "[Choose .ini files]";
            // 
            // Thief1GroupBox
            // 
            this.Thief1GroupBox.Controls.Add(this.Thief1IniBrowseButton);
            this.Thief1GroupBox.Controls.Add(this.Thief1IniTextBox);
            this.Thief1GroupBox.Location = new System.Drawing.Point(8, 32);
            this.Thief1GroupBox.Name = "Thief1GroupBox";
            this.Thief1GroupBox.Size = new System.Drawing.Size(536, 64);
            this.Thief1GroupBox.TabIndex = 2;
            this.Thief1GroupBox.TabStop = false;
            this.Thief1GroupBox.Text = "Thief 1";
            // 
            // Thief1IniBrowseButton
            // 
            this.Thief1IniBrowseButton.AutoSize = true;
            this.Thief1IniBrowseButton.Location = new System.Drawing.Point(448, 23);
            this.Thief1IniBrowseButton.Name = "Thief1IniBrowseButton";
            this.Thief1IniBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Thief1IniBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.Thief1IniBrowseButton.TabIndex = 1;
            this.Thief1IniBrowseButton.Text = "Browse...";
            this.Thief1IniBrowseButton.UseVisualStyleBackColor = true;
            this.Thief1IniBrowseButton.Click += new System.EventHandler(this.ThiefIniBrowseButtons_Click);
            // 
            // Thief1IniTextBox
            // 
            this.Thief1IniTextBox.Location = new System.Drawing.Point(16, 24);
            this.Thief1IniTextBox.Name = "Thief1IniTextBox";
            this.Thief1IniTextBox.Size = new System.Drawing.Size(432, 20);
            this.Thief1IniTextBox.TabIndex = 0;
            // 
            // Thief2GroupBox
            // 
            this.Thief2GroupBox.Controls.Add(this.Thief2IniBrowseButton);
            this.Thief2GroupBox.Controls.Add(this.Thief2IniTextBox);
            this.Thief2GroupBox.Location = new System.Drawing.Point(8, 104);
            this.Thief2GroupBox.Name = "Thief2GroupBox";
            this.Thief2GroupBox.Size = new System.Drawing.Size(536, 72);
            this.Thief2GroupBox.TabIndex = 3;
            this.Thief2GroupBox.TabStop = false;
            this.Thief2GroupBox.Text = "Thief 2";
            // 
            // Thief2IniBrowseButton
            // 
            this.Thief2IniBrowseButton.AutoSize = true;
            this.Thief2IniBrowseButton.Location = new System.Drawing.Point(447, 33);
            this.Thief2IniBrowseButton.Name = "Thief2IniBrowseButton";
            this.Thief2IniBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Thief2IniBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.Thief2IniBrowseButton.TabIndex = 1;
            this.Thief2IniBrowseButton.Text = "Browse...";
            this.Thief2IniBrowseButton.UseVisualStyleBackColor = true;
            this.Thief2IniBrowseButton.Click += new System.EventHandler(this.ThiefIniBrowseButtons_Click);
            // 
            // Thief2IniTextBox
            // 
            this.Thief2IniTextBox.Location = new System.Drawing.Point(15, 34);
            this.Thief2IniTextBox.Name = "Thief2IniTextBox";
            this.Thief2IniTextBox.Size = new System.Drawing.Size(432, 20);
            this.Thief2IniTextBox.TabIndex = 0;
            // 
            // Thief3GroupBox
            // 
            this.Thief3GroupBox.Controls.Add(this.Thief3IniBrowseButton);
            this.Thief3GroupBox.Controls.Add(this.Thief3IniTextBox);
            this.Thief3GroupBox.Location = new System.Drawing.Point(8, 184);
            this.Thief3GroupBox.Name = "Thief3GroupBox";
            this.Thief3GroupBox.Size = new System.Drawing.Size(536, 72);
            this.Thief3GroupBox.TabIndex = 4;
            this.Thief3GroupBox.TabStop = false;
            this.Thief3GroupBox.Text = "Thief 3";
            // 
            // Thief3IniBrowseButton
            // 
            this.Thief3IniBrowseButton.AutoSize = true;
            this.Thief3IniBrowseButton.Location = new System.Drawing.Point(447, 33);
            this.Thief3IniBrowseButton.Name = "Thief3IniBrowseButton";
            this.Thief3IniBrowseButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Thief3IniBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.Thief3IniBrowseButton.TabIndex = 1;
            this.Thief3IniBrowseButton.Text = "Browse...";
            this.Thief3IniBrowseButton.UseVisualStyleBackColor = true;
            this.Thief3IniBrowseButton.Click += new System.EventHandler(this.ThiefIniBrowseButtons_Click);
            // 
            // Thief3IniTextBox
            // 
            this.Thief3IniTextBox.Location = new System.Drawing.Point(15, 34);
            this.Thief3IniTextBox.Name = "Thief3IniTextBox";
            this.Thief3IniTextBox.Size = new System.Drawing.Size(432, 20);
            this.Thief3IniTextBox.TabIndex = 0;
            // 
            // OKButton
            // 
            this.OKButton.AutoSize = true;
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(3, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(84, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKCancelFlowLayoutPanel
            // 
            this.OKCancelFlowLayoutPanel.AutoSize = true;
            this.OKCancelFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKCancelFlowLayoutPanel.Controls.Add(this.Cancel_Button);
            this.OKCancelFlowLayoutPanel.Controls.Add(this.OKButton);
            this.OKCancelFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.OKCancelFlowLayoutPanel.Location = new System.Drawing.Point(384, 264);
            this.OKCancelFlowLayoutPanel.Name = "OKCancelFlowLayoutPanel";
            this.OKCancelFlowLayoutPanel.Size = new System.Drawing.Size(162, 29);
            this.OKCancelFlowLayoutPanel.TabIndex = 0;
            // 
            // ImportFromMultipleInisForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(550, 298);
            this.Controls.Add(this.OKCancelFlowLayoutPanel);
            this.Controls.Add(this.Thief3GroupBox);
            this.Controls.Add(this.Thief2GroupBox);
            this.Controls.Add(this.Thief1GroupBox);
            this.Controls.Add(this.ChooseIniFilesLabel);
            this.Icon = global::AngelLoader.Properties.Resources.AngelLoader;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportFromMultipleInisForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "[Import From Multiple]";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportFromMultipleInisForm_FormClosing);
            this.Thief1GroupBox.ResumeLayout(false);
            this.Thief1GroupBox.PerformLayout();
            this.Thief2GroupBox.ResumeLayout(false);
            this.Thief2GroupBox.PerformLayout();
            this.Thief3GroupBox.ResumeLayout(false);
            this.Thief3GroupBox.PerformLayout();
            this.OKCancelFlowLayoutPanel.ResumeLayout(false);
            this.OKCancelFlowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ChooseIniFilesLabel;
        private System.Windows.Forms.GroupBox Thief1GroupBox;
        private System.Windows.Forms.GroupBox Thief2GroupBox;
        private System.Windows.Forms.GroupBox Thief3GroupBox;
        private System.Windows.Forms.Button Thief1IniBrowseButton;
        private System.Windows.Forms.TextBox Thief1IniTextBox;
        private System.Windows.Forms.Button Thief2IniBrowseButton;
        private System.Windows.Forms.TextBox Thief2IniTextBox;
        private System.Windows.Forms.Button Thief3IniBrowseButton;
        private System.Windows.Forms.TextBox Thief3IniTextBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.FlowLayoutPanel OKCancelFlowLayoutPanel;
    }
}