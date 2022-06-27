namespace AngelLoader.Forms
{
    partial class OriginalGameModsForm
    {
        /// <summary>
        /// Custom generated component initializer with cruft removed.
        /// </summary>
        private void InitializeComponentSlim()
        {
            this.OrigGameModsControl = new AngelLoader.Forms.CustomControls.ModsControl();
            this.OKButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.Cancel_Button = new AngelLoader.Forms.CustomControls.DarkButton();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // OrigGameModsControl
            // 
            this.OrigGameModsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OrigGameModsControl.AutoScroll = true;
            this.OrigGameModsControl.Size = new System.Drawing.Size(527, 468);
            this.OrigGameModsControl.TabIndex = 0;
            this.OrigGameModsControl.AllEnabled += new System.EventHandler(this.OrigGameModsControl_AllEnabled);
            this.OrigGameModsControl.DisableNonImportantButtonClick += new System.EventHandler(this.OrigGameModsControl_DisableNonImportantButtonClick);
            this.OrigGameModsControl.DisabledModsTextBoxTextChanged += new System.EventHandler(this.OrigGameModsControl_DisabledModsTextBoxTextChanged);
            this.OrigGameModsControl.CheckListItemCheckedChanged += new System.EventHandler<AngelLoader.Forms.CustomControls.DarkCheckList.DarkCheckListEventArgs>(this.OrigGameModsControl_CheckListItemCheckedChanged);
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Margin = new System.Windows.Forms.Padding(0);
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.Cancel_Button);
            this.flowLayoutPanel1.Controls.Add(this.OKButton);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(8, 469);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(512, 23);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Margin = new System.Windows.Forms.Padding(0);
            this.Cancel_Button.MinimumSize = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 1;
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OriginalGameModsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(527, 500);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.OrigGameModsControl);
            this.KeyPreview = true;
            this.Icon = AngelLoader.Forms.AL_Icon.AngelLoader;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(200, 200);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            // Hack to prevent slow first render on some forms if Text is blank
            this.Text = " ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OriginalGameMods_FormClosing);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
