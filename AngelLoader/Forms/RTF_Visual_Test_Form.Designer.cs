
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
            this.RTFFileComboBox = new AngelLoader.Forms.CustomControls.DarkComboBox();
            this.RTFBox = new AngelLoader.Forms.CustomControls.RichTextBoxCustom();
            this.SuspendLayout();
            // 
            // RTFFileComboBox
            // 
            this.RTFFileComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RTFFileComboBox.FormattingEnabled = true;
            this.RTFFileComboBox.Location = new System.Drawing.Point(8, 40);
            this.RTFFileComboBox.Name = "RTFFileComboBox";
            this.RTFFileComboBox.Size = new System.Drawing.Size(968, 21);
            this.RTFFileComboBox.TabIndex = 1;
            this.RTFFileComboBox.SelectedIndexChanged += new System.EventHandler(this.RTFFileComboBox_SelectedIndexChanged);
            // 
            // RTFBox
            // 
            this.RTFBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RTFBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RTFBox.Location = new System.Drawing.Point(0, 112);
            this.RTFBox.Name = "RTFBox";
            this.RTFBox.ReadOnly = true;
            this.RTFBox.Size = new System.Drawing.Size(984, 536);
            this.RTFBox.TabIndex = 0;
            this.RTFBox.Text = "";
            this.RTFBox.VScroll += new System.EventHandler(this.RTFBox_VScroll);
            // 
            // RTF_Visual_Test_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 648);
            this.Controls.Add(this.RTFFileComboBox);
            this.Controls.Add(this.RTFBox);
            this.Name = "RTF_Visual_Test_Form";
            this.Text = "RTF_Visual_Test_Form";
            this.ResumeLayout(false);

        }

        #endregion

        private CustomControls.RichTextBoxCustom RTFBox;
        private CustomControls.DarkComboBox RTFFileComboBox;
    }
}