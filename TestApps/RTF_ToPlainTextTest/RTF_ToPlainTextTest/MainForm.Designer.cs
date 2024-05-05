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
            ConvertAndWriteWithRichTextBoxButton = new System.Windows.Forms.Button();
            ConvertAndWriteWithCustomButton = new System.Windows.Forms.Button();
            Test1Button = new System.Windows.Forms.Button();
            ConvertAndWriteToDiskGroupBox = new System.Windows.Forms.GroupBox();
            NoImagesSet_WriteCheckBox = new System.Windows.Forms.CheckBox();
            ConvertOnlyGroupBox = new System.Windows.Forms.GroupBox();
            NoImagesSet_ConvertOnlyCheckBox = new System.Windows.Forms.CheckBox();
            ConvertOnlyWithRichTextBoxButton = new System.Windows.Forms.Button();
            ConvertOnlyWithCustomXButton = new System.Windows.Forms.Button();
            ConvertOnlyWithCustomButton = new System.Windows.Forms.Button();
            WriteOneButton = new System.Windows.Forms.Button();
            ConvertOneButton = new System.Windows.Forms.Button();
            ConvertAndWriteToDiskGroupBox.SuspendLayout();
            ConvertOnlyGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // ConvertAndWriteWithRichTextBoxButton
            // 
            ConvertAndWriteWithRichTextBoxButton.Location = new System.Drawing.Point(19, 28);
            ConvertAndWriteWithRichTextBoxButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConvertAndWriteWithRichTextBoxButton.Name = "ConvertAndWriteWithRichTextBoxButton";
            ConvertAndWriteWithRichTextBoxButton.Size = new System.Drawing.Size(93, 27);
            ConvertAndWriteWithRichTextBoxButton.TabIndex = 0;
            ConvertAndWriteWithRichTextBoxButton.Text = "RichTextBox";
            ConvertAndWriteWithRichTextBoxButton.UseVisualStyleBackColor = true;
            ConvertAndWriteWithRichTextBoxButton.Click += ConvertAndWriteWithRichTextBoxButton_Click;
            // 
            // ConvertAndWriteWithCustomButton
            // 
            ConvertAndWriteWithCustomButton.Location = new System.Drawing.Point(121, 28);
            ConvertAndWriteWithCustomButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConvertAndWriteWithCustomButton.Name = "ConvertAndWriteWithCustomButton";
            ConvertAndWriteWithCustomButton.Size = new System.Drawing.Size(93, 27);
            ConvertAndWriteWithCustomButton.TabIndex = 1;
            ConvertAndWriteWithCustomButton.Text = "Custom";
            ConvertAndWriteWithCustomButton.UseVisualStyleBackColor = true;
            ConvertAndWriteWithCustomButton.Click += ConvertAndWriteWithCustomButton_Click;
            // 
            // Test1Button
            // 
            Test1Button.Location = new System.Drawing.Point(261, 138);
            Test1Button.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Test1Button.Name = "Test1Button";
            Test1Button.Size = new System.Drawing.Size(88, 27);
            Test1Button.TabIndex = 2;
            Test1Button.Text = "Test";
            Test1Button.UseVisualStyleBackColor = true;
            Test1Button.Click += Test1Button_Click;
            // 
            // ConvertAndWriteToDiskGroupBox
            // 
            ConvertAndWriteToDiskGroupBox.Controls.Add(NoImagesSet_WriteCheckBox);
            ConvertAndWriteToDiskGroupBox.Controls.Add(ConvertAndWriteWithRichTextBoxButton);
            ConvertAndWriteToDiskGroupBox.Controls.Add(ConvertAndWriteWithCustomButton);
            ConvertAndWriteToDiskGroupBox.Location = new System.Drawing.Point(9, 9);
            ConvertAndWriteToDiskGroupBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConvertAndWriteToDiskGroupBox.Name = "ConvertAndWriteToDiskGroupBox";
            ConvertAndWriteToDiskGroupBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConvertAndWriteToDiskGroupBox.Size = new System.Drawing.Size(233, 92);
            ConvertAndWriteToDiskGroupBox.TabIndex = 1;
            ConvertAndWriteToDiskGroupBox.TabStop = false;
            ConvertAndWriteToDiskGroupBox.Text = "Write converted files to disk";
            // 
            // NoImagesSet_WriteCheckBox
            // 
            NoImagesSet_WriteCheckBox.AutoSize = true;
            NoImagesSet_WriteCheckBox.Location = new System.Drawing.Point(19, 65);
            NoImagesSet_WriteCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NoImagesSet_WriteCheckBox.Name = "NoImagesSet_WriteCheckBox";
            NoImagesSet_WriteCheckBox.Size = new System.Drawing.Size(138, 19);
            NoImagesSet_WriteCheckBox.TabIndex = 4;
            NoImagesSet_WriteCheckBox.Text = "Use no-images rtf set";
            NoImagesSet_WriteCheckBox.UseVisualStyleBackColor = true;
            // 
            // ConvertOnlyGroupBox
            // 
            ConvertOnlyGroupBox.Controls.Add(NoImagesSet_ConvertOnlyCheckBox);
            ConvertOnlyGroupBox.Controls.Add(ConvertOnlyWithRichTextBoxButton);
            ConvertOnlyGroupBox.Controls.Add(ConvertOnlyWithCustomXButton);
            ConvertOnlyGroupBox.Controls.Add(ConvertOnlyWithCustomButton);
            ConvertOnlyGroupBox.Location = new System.Drawing.Point(9, 129);
            ConvertOnlyGroupBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConvertOnlyGroupBox.Name = "ConvertOnlyGroupBox";
            ConvertOnlyGroupBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConvertOnlyGroupBox.Size = new System.Drawing.Size(233, 111);
            ConvertOnlyGroupBox.TabIndex = 0;
            ConvertOnlyGroupBox.TabStop = false;
            ConvertOnlyGroupBox.Text = "Convert only:";
            // 
            // NoImagesSet_ConvertOnlyCheckBox
            // 
            NoImagesSet_ConvertOnlyCheckBox.AutoSize = true;
            NoImagesSet_ConvertOnlyCheckBox.Location = new System.Drawing.Point(19, 88);
            NoImagesSet_ConvertOnlyCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NoImagesSet_ConvertOnlyCheckBox.Name = "NoImagesSet_ConvertOnlyCheckBox";
            NoImagesSet_ConvertOnlyCheckBox.Size = new System.Drawing.Size(138, 19);
            NoImagesSet_ConvertOnlyCheckBox.TabIndex = 4;
            NoImagesSet_ConvertOnlyCheckBox.Text = "Use no-images rtf set";
            NoImagesSet_ConvertOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // ConvertOnlyWithRichTextBoxButton
            // 
            ConvertOnlyWithRichTextBoxButton.Location = new System.Drawing.Point(19, 28);
            ConvertOnlyWithRichTextBoxButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConvertOnlyWithRichTextBoxButton.Name = "ConvertOnlyWithRichTextBoxButton";
            ConvertOnlyWithRichTextBoxButton.Size = new System.Drawing.Size(93, 27);
            ConvertOnlyWithRichTextBoxButton.TabIndex = 0;
            ConvertOnlyWithRichTextBoxButton.Text = "RichTextBox";
            ConvertOnlyWithRichTextBoxButton.UseVisualStyleBackColor = true;
            ConvertOnlyWithRichTextBoxButton.Click += ConvertOnlyWithRichTextBoxButton_Click;
            // 
            // ConvertOnlyWithCustomXButton
            // 
            ConvertOnlyWithCustomXButton.Location = new System.Drawing.Point(120, 56);
            ConvertOnlyWithCustomXButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConvertOnlyWithCustomXButton.Name = "ConvertOnlyWithCustomXButton";
            ConvertOnlyWithCustomXButton.Size = new System.Drawing.Size(93, 27);
            ConvertOnlyWithCustomXButton.TabIndex = 1;
            ConvertOnlyWithCustomXButton.Text = "Custom X";
            ConvertOnlyWithCustomXButton.UseVisualStyleBackColor = true;
            ConvertOnlyWithCustomXButton.Click += ConvertOnlyWithCustomXButton_Click;
            // 
            // ConvertOnlyWithCustomButton
            // 
            ConvertOnlyWithCustomButton.Location = new System.Drawing.Point(120, 28);
            ConvertOnlyWithCustomButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConvertOnlyWithCustomButton.Name = "ConvertOnlyWithCustomButton";
            ConvertOnlyWithCustomButton.Size = new System.Drawing.Size(93, 27);
            ConvertOnlyWithCustomButton.TabIndex = 1;
            ConvertOnlyWithCustomButton.Text = "Custom";
            ConvertOnlyWithCustomButton.UseVisualStyleBackColor = true;
            ConvertOnlyWithCustomButton.Click += ConvertOnlyWithCustomButton_Click;
            // 
            // WriteOneButton
            // 
            WriteOneButton.Location = new System.Drawing.Point(261, 46);
            WriteOneButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WriteOneButton.Name = "WriteOneButton";
            WriteOneButton.Size = new System.Drawing.Size(88, 27);
            WriteOneButton.TabIndex = 3;
            WriteOneButton.Text = "Write one";
            WriteOneButton.UseVisualStyleBackColor = true;
            WriteOneButton.Click += WriteOneButton_Click;
            // 
            // ConvertOneButton
            // 
            ConvertOneButton.Location = new System.Drawing.Point(261, 9);
            ConvertOneButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ConvertOneButton.Name = "ConvertOneButton";
            ConvertOneButton.Size = new System.Drawing.Size(88, 27);
            ConvertOneButton.TabIndex = 3;
            ConvertOneButton.Text = "Convert one";
            ConvertOneButton.UseVisualStyleBackColor = true;
            ConvertOneButton.Click += ConvertOneButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(358, 253);
            Controls.Add(ConvertOneButton);
            Controls.Add(WriteOneButton);
            Controls.Add(ConvertOnlyGroupBox);
            Controls.Add(ConvertAndWriteToDiskGroupBox);
            Controls.Add(Test1Button);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "MainForm";
            Text = "RTF Perf/Accuracy Tester";
            Shown += MainForm_Shown;
            ConvertAndWriteToDiskGroupBox.ResumeLayout(false);
            ConvertAndWriteToDiskGroupBox.PerformLayout();
            ConvertOnlyGroupBox.ResumeLayout(false);
            ConvertOnlyGroupBox.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.Button ConvertOnlyWithCustomXButton;
    }
}

