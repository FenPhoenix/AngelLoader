namespace AngelLoader.Forms.CustomControls;

sealed partial class ProgressPanel
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.SubProgressBar = new AngelLoader.Forms.CustomControls.DarkProgressBar();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
        this.SubPercentLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.MainPercentLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.MainMessage1Label = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.MainProgressBar = new AngelLoader.Forms.CustomControls.DarkProgressBar();
        this.SubMessageLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.MainMessage2Label = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.SuspendLayout();
        // 
        // SubProgressBar
        // 
        this.SubProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.SubProgressBar.Location = new System.Drawing.Point(8, 120);
        this.SubProgressBar.Size = new System.Drawing.Size(406, 16);
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
        this.Cancel_Button.AutoSize = true;
        this.Cancel_Button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.Cancel_Button.Location = new System.Drawing.Point(168, 152);
        this.Cancel_Button.MinimumSize = new System.Drawing.Size(88, 23);
        this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.Cancel_Button.TabIndex = 7;
        this.Cancel_Button.Click += new System.EventHandler(this.ProgressCancelButton_Click);
        // 
        // SubPercentLabel
        // 
        this.SubPercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.SubPercentLabel.Location = new System.Drawing.Point(4, 104);
        this.SubPercentLabel.Size = new System.Drawing.Size(416, 13);
        this.SubPercentLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // MainPercentLabel
        // 
        this.MainPercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.MainPercentLabel.Location = new System.Drawing.Point(4, 40);
        this.MainPercentLabel.Size = new System.Drawing.Size(416, 13);
        this.MainPercentLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // MainMessage1Label
        // 
        this.MainMessage1Label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.MainMessage1Label.Location = new System.Drawing.Point(4, 8);
        this.MainMessage1Label.Size = new System.Drawing.Size(416, 13);
        this.MainMessage1Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // MainProgressBar
        // 
        this.MainProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.MainProgressBar.Location = new System.Drawing.Point(8, 56);
        this.MainProgressBar.Size = new System.Drawing.Size(406, 23);
        // 
        // SubMessageLabel
        // 
        this.SubMessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.SubMessageLabel.Location = new System.Drawing.Point(4, 88);
        this.SubMessageLabel.Size = new System.Drawing.Size(416, 13);
        this.SubMessageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // MainMessage2Label
        // 
        this.MainMessage2Label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.MainMessage2Label.Location = new System.Drawing.Point(4, 24);
        this.MainMessage2Label.Size = new System.Drawing.Size(416, 13);
        this.MainMessage2Label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // ProgressPanel
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.Controls.Add(this.SubProgressBar);
        this.Controls.Add(this.Cancel_Button);
        this.Controls.Add(this.SubPercentLabel);
        this.Controls.Add(this.MainPercentLabel);
        this.Controls.Add(this.MainMessage1Label);
        this.Controls.Add(this.MainProgressBar);
        this.Controls.Add(this.SubMessageLabel);
        this.Controls.Add(this.MainMessage2Label);
        this.Size = new System.Drawing.Size(424, 192);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
