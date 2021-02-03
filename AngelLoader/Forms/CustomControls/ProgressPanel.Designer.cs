namespace AngelLoader.Forms.CustomControls
{
    sealed partial class ProgressPanel
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
#if DEBUG
        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ProgressCancelButton = new DarkUI.Controls.DarkButton();
            this.ProgressPercentLabel = new System.Windows.Forms.Label();
            this.ProgressMessageLabel = new System.Windows.Forms.Label();
            this.CurrentThingLabel = new System.Windows.Forms.Label();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // ProgressCancelButton
            // 
            this.ProgressCancelButton.AutoSize = true;
            this.ProgressCancelButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ProgressCancelButton.BackColor = System.Drawing.SystemColors.Control;
            this.ProgressCancelButton.DarkModeEnabled = false;
            this.ProgressCancelButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ProgressCancelButton.Location = new System.Drawing.Point(168, 88);
            this.ProgressCancelButton.MinimumSize = new System.Drawing.Size(88, 23);
            this.ProgressCancelButton.Name = "ProgressCancelButton";
            this.ProgressCancelButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ProgressCancelButton.Size = new System.Drawing.Size(88, 23);
            this.ProgressCancelButton.TabIndex = 4;
            this.ProgressCancelButton.Text = "Cancel";
            this.ProgressCancelButton.UseMnemonic = false;
            this.ProgressCancelButton.UseVisualStyleBackColor = true;
            this.ProgressCancelButton.Click += new System.EventHandler(this.ProgressCancelButton_Click);
            // 
            // ProgressPercentLabel
            // 
            this.ProgressPercentLabel.Location = new System.Drawing.Point(2, 40);
            this.ProgressPercentLabel.Name = "ProgressPercentLabel";
            this.ProgressPercentLabel.Size = new System.Drawing.Size(418, 13);
            this.ProgressPercentLabel.TabIndex = 2;
            this.ProgressPercentLabel.Text = "%";
            this.ProgressPercentLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ProgressMessageLabel
            // 
            this.ProgressMessageLabel.Location = new System.Drawing.Point(2, 8);
            this.ProgressMessageLabel.Name = "ProgressMessageLabel";
            this.ProgressMessageLabel.Size = new System.Drawing.Size(418, 13);
            this.ProgressMessageLabel.TabIndex = 0;
            this.ProgressMessageLabel.Text = "Message";
            this.ProgressMessageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // CurrentThingLabel
            // 
            this.CurrentThingLabel.Location = new System.Drawing.Point(2, 24);
            this.CurrentThingLabel.Name = "CurrentThingLabel";
            this.CurrentThingLabel.Size = new System.Drawing.Size(418, 13);
            this.CurrentThingLabel.TabIndex = 1;
            this.CurrentThingLabel.Text = "Current thing";
            this.CurrentThingLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ProgressBar
            // 
            this.ProgressBar.Location = new System.Drawing.Point(8, 56);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(408, 23);
            this.ProgressBar.TabIndex = 3;
            // 
            // ProgressPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.ProgressCancelButton);
            this.Controls.Add(this.ProgressPercentLabel);
            this.Controls.Add(this.ProgressMessageLabel);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.CurrentThingLabel);
            this.Name = "ProgressPanel";
            this.Size = new System.Drawing.Size(424, 128);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
#endif
        private DarkUI.Controls.DarkButton ProgressCancelButton;
        private System.Windows.Forms.Label ProgressPercentLabel;
        private System.Windows.Forms.Label ProgressMessageLabel;
        private System.Windows.Forms.Label CurrentThingLabel;
        private System.Windows.Forms.ProgressBar ProgressBar;
    }
}
