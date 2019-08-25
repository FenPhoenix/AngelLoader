namespace AngelLoader.Forms.Import
{
    partial class ImportFromDarkLoaderForm
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
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.OKCancelFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.user_DL_ImportControls1 = new AngelLoader.Forms.Import.User_DL_ImportControls();
            this.OKCancelFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
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
            this.OKCancelFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKCancelFlowLayoutPanel.AutoSize = true;
            this.OKCancelFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKCancelFlowLayoutPanel.Controls.Add(this.Cancel_Button);
            this.OKCancelFlowLayoutPanel.Controls.Add(this.OKButton);
            this.OKCancelFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.OKCancelFlowLayoutPanel.Location = new System.Drawing.Point(384, 144);
            this.OKCancelFlowLayoutPanel.Name = "OKCancelFlowLayoutPanel";
            this.OKCancelFlowLayoutPanel.Size = new System.Drawing.Size(162, 29);
            this.OKCancelFlowLayoutPanel.TabIndex = 9;
            // 
            // user_DL_ImportControls1
            // 
            this.user_DL_ImportControls1.Location = new System.Drawing.Point(0, 0);
            this.user_DL_ImportControls1.Name = "user_DL_ImportControls1";
            this.user_DL_ImportControls1.Size = new System.Drawing.Size(545, 128);
            this.user_DL_ImportControls1.TabIndex = 10;
            // 
            // ImportFromDarkLoaderForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(547, 175);
            this.Controls.Add(this.user_DL_ImportControls1);
            this.Controls.Add(this.OKCancelFlowLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::AngelLoader.Properties.Resources.AngelLoader;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportFromDarkLoaderForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import from DarkLoader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportFromDarkLoaderForm_FormClosing);
            this.Load += new System.EventHandler(this.ImportFromDarkLoaderForm_Load);
            this.OKCancelFlowLayoutPanel.ResumeLayout(false);
            this.OKCancelFlowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.FlowLayoutPanel OKCancelFlowLayoutPanel;
        private User_DL_ImportControls user_DL_ImportControls1;
    }
}