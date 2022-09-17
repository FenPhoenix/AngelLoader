namespace RTF_ToPlainTextTest
{
    sealed partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ConvertWithRichTextBoxButton = new System.Windows.Forms.Button();
            this.ConvertWithCustomButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Test1Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ConvertWithRichTextBoxButton
            // 
            this.ConvertWithRichTextBoxButton.Location = new System.Drawing.Point(216, 120);
            this.ConvertWithRichTextBoxButton.Name = "ConvertWithRichTextBoxButton";
            this.ConvertWithRichTextBoxButton.Size = new System.Drawing.Size(88, 23);
            this.ConvertWithRichTextBoxButton.TabIndex = 0;
            this.ConvertWithRichTextBoxButton.Text = "RichTextBox";
            this.ConvertWithRichTextBoxButton.UseVisualStyleBackColor = true;
            this.ConvertWithRichTextBoxButton.Click += new System.EventHandler(this.ConvertWithRichTextBoxButton_Click);
            // 
            // ConvertWithCustomButton
            // 
            this.ConvertWithCustomButton.Location = new System.Drawing.Point(312, 120);
            this.ConvertWithCustomButton.Name = "ConvertWithCustomButton";
            this.ConvertWithCustomButton.Size = new System.Drawing.Size(75, 23);
            this.ConvertWithCustomButton.TabIndex = 0;
            this.ConvertWithCustomButton.Text = "Custom";
            this.ConvertWithCustomButton.UseVisualStyleBackColor = true;
            this.ConvertWithCustomButton.Click += new System.EventHandler(this.ConvertWithCustomButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(216, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Convert RTFs to plaintext using:";
            // 
            // Test1Button
            // 
            this.Test1Button.Location = new System.Drawing.Point(264, 176);
            this.Test1Button.Name = "Test1Button";
            this.Test1Button.Size = new System.Drawing.Size(75, 23);
            this.Test1Button.TabIndex = 2;
            this.Test1Button.Text = "Test";
            this.Test1Button.UseVisualStyleBackColor = true;
            this.Test1Button.Click += new System.EventHandler(this.Test1Button_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Test1Button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ConvertWithCustomButton);
            this.Controls.Add(this.ConvertWithRichTextBoxButton);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ConvertWithRichTextBoxButton;
        private System.Windows.Forms.Button ConvertWithCustomButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Test1Button;
    }
}

