#if DEBUG || Release_Testing
namespace AngelLoader.Forms
{
    sealed partial class RTF_Visual_Test_Form
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
            this.NotesTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
            this.SaveButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.RTFBox = new AngelLoader.Forms.CustomControls.RichTextBoxCustom();
            this.RTFFileComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.SuspendLayout();
            // 
            // NotesTextBox
            // 
            this.NotesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NotesTextBox.Location = new System.Drawing.Point(8, 32);
            this.NotesTextBox.Multiline = true;
            this.NotesTextBox.Name = "NotesTextBox";
            this.NotesTextBox.Size = new System.Drawing.Size(888, 56);
            this.NotesTextBox.TabIndex = 3;
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveButton.Location = new System.Drawing.Point(902, 32);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 2;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // RTFBox
            // 
            this.RTFBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RTFBox.BackColor = System.Drawing.SystemColors.Window;
            this.RTFBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RTFBox.Location = new System.Drawing.Point(0, 112);
            this.RTFBox.Name = "RTFBox";
            this.RTFBox.ReadOnly = true;
            this.RTFBox.Size = new System.Drawing.Size(984, 536);
            this.RTFBox.TabIndex = 0;
            this.RTFBox.Text = "";
            this.RTFBox.VScroll += new System.EventHandler(this.RTFBox_VScroll);
            // 
            // RTFFileComboBox
            // 
            this.RTFFileComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RTFFileComboBox.FormattingEnabled = true;
            this.RTFFileComboBox.Location = new System.Drawing.Point(8, 8);
            this.RTFFileComboBox.Name = "RTFFileComboBox";
            this.RTFFileComboBox.Size = new System.Drawing.Size(968, 21);
            this.RTFFileComboBox.TabIndex = 1;
            this.RTFFileComboBox.SelectedIndexChanged += new System.EventHandler(this.RTFFileComboBox_SelectedIndexChanged);
            // 
            // RTF_Visual_Test_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 648);
            this.Controls.Add(this.NotesTextBox);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.RTFBox);
            this.Controls.Add(this.RTFFileComboBox);
            this.Name = "RTF_Visual_Test_Form";
            this.Text = "RTF_Visual_Test_Form";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RTF_Visual_Test_Form_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CustomControls.RichTextBoxCustom RTFBox;
        private CustomControls.DarkComboBox RTFFileComboBox;
        private CustomControls.DarkButton SaveButton;
        private CustomControls.DarkTextBox NotesTextBox;
    }
}
#endif
