namespace AngelLoader.Forms.CustomControls
{
    sealed partial class ProgressPanel
    {
        /// <summary>
        /// Custom generated component initializer with cruft removed.
        /// </summary>
        private void InitializeComponentSlim()
        {
            this.ProgressCancelButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ProgressPercentLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.ProgressMessageLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.CurrentThingLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.ProgressBar = new AngelLoader.Forms.CustomControls.DarkProgressBar();
            this.SubProgressBar = new AngelLoader.Forms.CustomControls.DarkProgressBar();
            this.CurrentSubThingLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.SubProgressPercentLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.SuspendLayout();
            // 
            // ProgressCancelButton
            // 
            this.ProgressCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ProgressCancelButton.AutoSize = true;
            this.ProgressCancelButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ProgressCancelButton.Location = new System.Drawing.Point(168, 152);
            this.ProgressCancelButton.MinimumSize = new System.Drawing.Size(88, 23);
            this.ProgressCancelButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ProgressCancelButton.Size = new System.Drawing.Size(88, 23);
            this.ProgressCancelButton.TabIndex = 4;
            this.ProgressCancelButton.UseVisualStyleBackColor = true;
            this.ProgressCancelButton.Click += new System.EventHandler(this.ProgressCancelButton_Click);
            // 
            // ProgressPercentLabel
            // 
            this.ProgressPercentLabel.Location = new System.Drawing.Point(2, 40);
            this.ProgressPercentLabel.Size = new System.Drawing.Size(418, 13);
            this.ProgressPercentLabel.TabIndex = 2;
            this.ProgressPercentLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ProgressMessageLabel
            // 
            this.ProgressMessageLabel.Location = new System.Drawing.Point(2, 8);
            this.ProgressMessageLabel.Size = new System.Drawing.Size(418, 13);
            this.ProgressMessageLabel.TabIndex = 0;
            this.ProgressMessageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // CurrentThingLabel
            // 
            this.CurrentThingLabel.Location = new System.Drawing.Point(2, 24);
            this.CurrentThingLabel.Size = new System.Drawing.Size(418, 13);
            this.CurrentThingLabel.TabIndex = 1;
            this.CurrentThingLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ProgressBar
            // 
            this.ProgressBar.Location = new System.Drawing.Point(8, 56);
            this.ProgressBar.Size = new System.Drawing.Size(408, 23);
            this.ProgressBar.TabIndex = 3;
            // 
            // SubProgressBar
            // 
            this.SubProgressBar.Location = new System.Drawing.Point(8, 120);
            this.SubProgressBar.Size = new System.Drawing.Size(408, 16);
            this.SubProgressBar.TabIndex = 5;
            // 
            // CurrentSubThingLabel
            // 
            this.CurrentSubThingLabel.Location = new System.Drawing.Point(0, 88);
            this.CurrentSubThingLabel.Size = new System.Drawing.Size(418, 13);
            this.CurrentSubThingLabel.TabIndex = 1;
            this.CurrentSubThingLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // SubProgressPercentLabel
            // 
            this.SubProgressPercentLabel.Location = new System.Drawing.Point(0, 104);
            this.SubProgressPercentLabel.Size = new System.Drawing.Size(418, 13);
            this.SubProgressPercentLabel.TabIndex = 2;
            this.SubProgressPercentLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ProgressPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.SubProgressBar);
            this.Controls.Add(this.ProgressCancelButton);
            this.Controls.Add(this.SubProgressPercentLabel);
            this.Controls.Add(this.ProgressPercentLabel);
            this.Controls.Add(this.ProgressMessageLabel);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.CurrentSubThingLabel);
            this.Controls.Add(this.CurrentThingLabel);
            this.Size = new System.Drawing.Size(424, 192);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
