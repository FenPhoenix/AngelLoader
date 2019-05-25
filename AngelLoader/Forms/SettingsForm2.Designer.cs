namespace AngelLoader.Forms
{
    partial class SettingsForm2
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
            this.SectionsListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // SectionsListBox
            // 
            this.SectionsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.SectionsListBox.FormattingEnabled = true;
            this.SectionsListBox.IntegralHeight = false;
            this.SectionsListBox.Items.AddRange(new object[] {
            "Paths",
            "FM Display",
            "Other"});
            this.SectionsListBox.Location = new System.Drawing.Point(0, 0);
            this.SectionsListBox.Name = "SectionsListBox";
            this.SectionsListBox.Size = new System.Drawing.Size(232, 550);
            this.SectionsListBox.TabIndex = 1;
            // 
            // SettingsForm2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 550);
            this.Controls.Add(this.SectionsListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(540, 320);
            this.Name = "SettingsForm2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SettingsForm2";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox SectionsListBox;
    }
}