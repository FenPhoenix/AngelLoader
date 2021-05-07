namespace AngelLoader.Forms
{
    public sealed partial class GameVersionsForm
    {
        /// <summary>
        /// Custom generated component initializer with cruft removed.
        /// </summary>
        private void InitializeComponentSlim()
        {
            this.T1VersionLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.T1VersionTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.T2VersionLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.T2VersionTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.T3VersionLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.T3VersionTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.SS2VersionLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.SS2VersionTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.OKFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.OKFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // T1VersionLabel
            // 
            this.T1VersionLabel.AutoSize = true;
            this.T1VersionLabel.Location = new System.Drawing.Point(11, 11);
            this.T1VersionLabel.TabIndex = 1;
            // 
            // T1VersionTextBox
            // 
            this.T1VersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.T1VersionTextBox.Location = new System.Drawing.Point(205, 8);
            this.T1VersionTextBox.MaximumSize = new System.Drawing.Size(224, 32767);
            this.T1VersionTextBox.MinimumSize = new System.Drawing.Size(80, 4);
            this.T1VersionTextBox.ReadOnly = true;
            this.T1VersionTextBox.Size = new System.Drawing.Size(224, 20);
            this.T1VersionTextBox.TabIndex = 2;
            // 
            // T2VersionLabel
            // 
            this.T2VersionLabel.AutoSize = true;
            this.T2VersionLabel.Location = new System.Drawing.Point(11, 35);
            this.T2VersionLabel.TabIndex = 3;
            // 
            // T2VersionTextBox
            // 
            this.T2VersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.T2VersionTextBox.Location = new System.Drawing.Point(205, 32);
            this.T2VersionTextBox.MaximumSize = new System.Drawing.Size(224, 32767);
            this.T2VersionTextBox.MinimumSize = new System.Drawing.Size(80, 4);
            this.T2VersionTextBox.ReadOnly = true;
            this.T2VersionTextBox.Size = new System.Drawing.Size(224, 20);
            this.T2VersionTextBox.TabIndex = 4;
            // 
            // T3VersionLabel
            // 
            this.T3VersionLabel.AutoSize = true;
            this.T3VersionLabel.Location = new System.Drawing.Point(11, 59);
            this.T3VersionLabel.TabIndex = 5;
            // 
            // T3VersionTextBox
            // 
            this.T3VersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.T3VersionTextBox.Location = new System.Drawing.Point(205, 56);
            this.T3VersionTextBox.MaximumSize = new System.Drawing.Size(224, 32767);
            this.T3VersionTextBox.MinimumSize = new System.Drawing.Size(80, 4);
            this.T3VersionTextBox.ReadOnly = true;
            this.T3VersionTextBox.Size = new System.Drawing.Size(224, 20);
            this.T3VersionTextBox.TabIndex = 6;
            // 
            // SS2VersionLabel
            // 
            this.SS2VersionLabel.AutoSize = true;
            this.SS2VersionLabel.Location = new System.Drawing.Point(11, 83);
            this.SS2VersionLabel.TabIndex = 7;
            // 
            // SS2VersionTextBox
            // 
            this.SS2VersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SS2VersionTextBox.Location = new System.Drawing.Point(205, 80);
            this.SS2VersionTextBox.MaximumSize = new System.Drawing.Size(224, 32767);
            this.SS2VersionTextBox.MinimumSize = new System.Drawing.Size(80, 4);
            this.SS2VersionTextBox.ReadOnly = true;
            this.SS2VersionTextBox.Size = new System.Drawing.Size(224, 20);
            this.SS2VersionTextBox.TabIndex = 8;
            // 
            // OKButton
            // 
            this.OKButton.AutoSize = true;
            this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 8, 9, 3);
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // OKFlowLayoutPanel
            // 
            this.OKFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKFlowLayoutPanel.Controls.Add(this.OKButton);
            this.OKFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.OKFlowLayoutPanel.Location = new System.Drawing.Point(0, 106);
            this.OKFlowLayoutPanel.Size = new System.Drawing.Size(438, 40);
            this.OKFlowLayoutPanel.TabIndex = 0;
            // 
            // GameVersionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.OKButton;
            this.ClientSize = new System.Drawing.Size(438, 146);
            this.Controls.Add(this.OKFlowLayoutPanel);
            this.Controls.Add(this.SS2VersionTextBox);
            this.Controls.Add(this.SS2VersionLabel);
            this.Controls.Add(this.T3VersionTextBox);
            this.Controls.Add(this.T3VersionLabel);
            this.Controls.Add(this.T2VersionTextBox);
            this.Controls.Add(this.T2VersionLabel);
            this.Controls.Add(this.T1VersionTextBox);
            this.Controls.Add(this.T1VersionLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = AngelLoader.Forms.AL_Icon.AngelLoader;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " ";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GameVersionsForm_KeyDown);
            this.OKFlowLayoutPanel.ResumeLayout(false);
            this.OKFlowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
