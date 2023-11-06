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
            this.ConvertAndWriteWithRichTextBoxButton = new System.Windows.Forms.Button();
            this.ConvertAndWriteWithCustomButton = new System.Windows.Forms.Button();
            this.Test1Button = new System.Windows.Forms.Button();
            this.ConvertAndWriteToDiskGroupBox = new System.Windows.Forms.GroupBox();
            this.NoImagesSet_WriteCheckBox = new System.Windows.Forms.CheckBox();
            this.ConvertOnlyGroupBox = new System.Windows.Forms.GroupBox();
            this.NoImagesSet_ConvertOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.ConvertOnlyWithRichTextBoxButton = new System.Windows.Forms.Button();
            this.ConvertOnlyWithCustomButton = new System.Windows.Forms.Button();
            this.WriteOneButton = new System.Windows.Forms.Button();
            this.ConvertOneButton = new System.Windows.Forms.Button();
            this.ConvertAndWriteToDiskGroupBox.SuspendLayout();
            this.ConvertOnlyGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConvertAndWriteWithRichTextBoxButton
            // 
            this.ConvertAndWriteWithRichTextBoxButton.Location = new System.Drawing.Point(16, 24);
            this.ConvertAndWriteWithRichTextBoxButton.Name = "ConvertAndWriteWithRichTextBoxButton";
            this.ConvertAndWriteWithRichTextBoxButton.Size = new System.Drawing.Size(80, 23);
            this.ConvertAndWriteWithRichTextBoxButton.TabIndex = 0;
            this.ConvertAndWriteWithRichTextBoxButton.Text = "RichTextBox";
            this.ConvertAndWriteWithRichTextBoxButton.UseVisualStyleBackColor = true;
            this.ConvertAndWriteWithRichTextBoxButton.Click += new System.EventHandler(this.ConvertAndWriteWithRichTextBoxButton_Click);
            // 
            // ConvertAndWriteWithCustomButton
            // 
            this.ConvertAndWriteWithCustomButton.Location = new System.Drawing.Point(104, 24);
            this.ConvertAndWriteWithCustomButton.Name = "ConvertAndWriteWithCustomButton";
            this.ConvertAndWriteWithCustomButton.Size = new System.Drawing.Size(80, 23);
            this.ConvertAndWriteWithCustomButton.TabIndex = 1;
            this.ConvertAndWriteWithCustomButton.Text = "Custom";
            this.ConvertAndWriteWithCustomButton.UseVisualStyleBackColor = true;
            this.ConvertAndWriteWithCustomButton.Click += new System.EventHandler(this.ConvertAndWriteWithCustomButton_Click);
            // 
            // Test1Button
            // 
            this.Test1Button.Location = new System.Drawing.Point(224, 120);
            this.Test1Button.Name = "Test1Button";
            this.Test1Button.Size = new System.Drawing.Size(75, 23);
            this.Test1Button.TabIndex = 2;
            this.Test1Button.Text = "Test";
            this.Test1Button.UseVisualStyleBackColor = true;
            this.Test1Button.Click += new System.EventHandler(this.Test1Button_Click);
            // 
            // ConvertAndWriteToDiskGroupBox
            // 
            this.ConvertAndWriteToDiskGroupBox.Controls.Add(this.NoImagesSet_WriteCheckBox);
            this.ConvertAndWriteToDiskGroupBox.Controls.Add(this.ConvertAndWriteWithRichTextBoxButton);
            this.ConvertAndWriteToDiskGroupBox.Controls.Add(this.ConvertAndWriteWithCustomButton);
            this.ConvertAndWriteToDiskGroupBox.Location = new System.Drawing.Point(8, 8);
            this.ConvertAndWriteToDiskGroupBox.Name = "ConvertAndWriteToDiskGroupBox";
            this.ConvertAndWriteToDiskGroupBox.Size = new System.Drawing.Size(200, 80);
            this.ConvertAndWriteToDiskGroupBox.TabIndex = 1;
            this.ConvertAndWriteToDiskGroupBox.TabStop = false;
            this.ConvertAndWriteToDiskGroupBox.Text = "Write converted files to disk";
            // 
            // NoImagesSet_WriteCheckBox
            // 
            this.NoImagesSet_WriteCheckBox.AutoSize = true;
            this.NoImagesSet_WriteCheckBox.Location = new System.Drawing.Point(16, 56);
            this.NoImagesSet_WriteCheckBox.Name = "NoImagesSet_WriteCheckBox";
            this.NoImagesSet_WriteCheckBox.Size = new System.Drawing.Size(125, 17);
            this.NoImagesSet_WriteCheckBox.TabIndex = 4;
            this.NoImagesSet_WriteCheckBox.Text = "Use no-images rtf set";
            this.NoImagesSet_WriteCheckBox.UseVisualStyleBackColor = true;
            // 
            // ConvertOnlyGroupBox
            // 
            this.ConvertOnlyGroupBox.Controls.Add(this.NoImagesSet_ConvertOnlyCheckBox);
            this.ConvertOnlyGroupBox.Controls.Add(this.ConvertOnlyWithRichTextBoxButton);
            this.ConvertOnlyGroupBox.Controls.Add(this.ConvertOnlyWithCustomButton);
            this.ConvertOnlyGroupBox.Location = new System.Drawing.Point(8, 112);
            this.ConvertOnlyGroupBox.Name = "ConvertOnlyGroupBox";
            this.ConvertOnlyGroupBox.Size = new System.Drawing.Size(200, 80);
            this.ConvertOnlyGroupBox.TabIndex = 0;
            this.ConvertOnlyGroupBox.TabStop = false;
            this.ConvertOnlyGroupBox.Text = "Convert only:";
            // 
            // NoImagesSet_ConvertOnlyCheckBox
            // 
            this.NoImagesSet_ConvertOnlyCheckBox.AutoSize = true;
            this.NoImagesSet_ConvertOnlyCheckBox.Location = new System.Drawing.Point(16, 56);
            this.NoImagesSet_ConvertOnlyCheckBox.Name = "NoImagesSet_ConvertOnlyCheckBox";
            this.NoImagesSet_ConvertOnlyCheckBox.Size = new System.Drawing.Size(125, 17);
            this.NoImagesSet_ConvertOnlyCheckBox.TabIndex = 4;
            this.NoImagesSet_ConvertOnlyCheckBox.Text = "Use no-images rtf set";
            this.NoImagesSet_ConvertOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // ConvertOnlyWithRichTextBoxButton
            // 
            this.ConvertOnlyWithRichTextBoxButton.Location = new System.Drawing.Point(16, 24);
            this.ConvertOnlyWithRichTextBoxButton.Name = "ConvertOnlyWithRichTextBoxButton";
            this.ConvertOnlyWithRichTextBoxButton.Size = new System.Drawing.Size(80, 23);
            this.ConvertOnlyWithRichTextBoxButton.TabIndex = 0;
            this.ConvertOnlyWithRichTextBoxButton.Text = "RichTextBox";
            this.ConvertOnlyWithRichTextBoxButton.UseVisualStyleBackColor = true;
            this.ConvertOnlyWithRichTextBoxButton.Click += new System.EventHandler(this.ConvertOnlyWithRichTextBoxButton_Click);
            // 
            // ConvertOnlyWithCustomButton
            // 
            this.ConvertOnlyWithCustomButton.Location = new System.Drawing.Point(104, 24);
            this.ConvertOnlyWithCustomButton.Name = "ConvertOnlyWithCustomButton";
            this.ConvertOnlyWithCustomButton.Size = new System.Drawing.Size(80, 23);
            this.ConvertOnlyWithCustomButton.TabIndex = 1;
            this.ConvertOnlyWithCustomButton.Text = "Custom";
            this.ConvertOnlyWithCustomButton.UseVisualStyleBackColor = true;
            this.ConvertOnlyWithCustomButton.Click += new System.EventHandler(this.ConvertOnlyWithCustomButton_Click);
            // 
            // WriteOneButton
            // 
            this.WriteOneButton.Location = new System.Drawing.Point(224, 40);
            this.WriteOneButton.Name = "WriteOneButton";
            this.WriteOneButton.Size = new System.Drawing.Size(75, 23);
            this.WriteOneButton.TabIndex = 3;
            this.WriteOneButton.Text = "Write one";
            this.WriteOneButton.UseVisualStyleBackColor = true;
            this.WriteOneButton.Click += new System.EventHandler(this.WriteOneButton_Click);
            // 
            // ConvertOneButton
            // 
            this.ConvertOneButton.Location = new System.Drawing.Point(224, 8);
            this.ConvertOneButton.Name = "ConvertOneButton";
            this.ConvertOneButton.Size = new System.Drawing.Size(75, 23);
            this.ConvertOneButton.TabIndex = 3;
            this.ConvertOneButton.Text = "Convert one";
            this.ConvertOneButton.UseVisualStyleBackColor = true;
            this.ConvertOneButton.Click += new System.EventHandler(this.ConvertOneButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 219);
            this.Controls.Add(this.ConvertOneButton);
            this.Controls.Add(this.WriteOneButton);
            this.Controls.Add(this.ConvertOnlyGroupBox);
            this.Controls.Add(this.ConvertAndWriteToDiskGroupBox);
            this.Controls.Add(this.Test1Button);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "RTF Perf/Accuracy Tester";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ConvertAndWriteToDiskGroupBox.ResumeLayout(false);
            this.ConvertAndWriteToDiskGroupBox.PerformLayout();
            this.ConvertOnlyGroupBox.ResumeLayout(false);
            this.ConvertOnlyGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ConvertAndWriteWithRichTextBoxButton;
        private System.Windows.Forms.Button ConvertAndWriteWithCustomButton;
        private System.Windows.Forms.Button Test1Button;
        private System.Windows.Forms.GroupBox ConvertAndWriteToDiskGroupBox;
        private System.Windows.Forms.GroupBox ConvertOnlyGroupBox;
        private System.Windows.Forms.Button ConvertOnlyWithRichTextBoxButton;
        private System.Windows.Forms.Button ConvertOnlyWithCustomButton;
        private System.Windows.Forms.Button WriteOneButton;
        private System.Windows.Forms.Button ConvertOneButton;
        private System.Windows.Forms.CheckBox NoImagesSet_WriteCheckBox;
        private System.Windows.Forms.CheckBox NoImagesSet_ConvertOnlyCheckBox;
    }
}

