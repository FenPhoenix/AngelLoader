namespace AngelLoader.Forms
{
    partial class ImportForm
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
            this.DarkLoaderButton = new System.Windows.Forms.Button();
            this.FMSelButton = new System.Windows.Forms.Button();
            this.NewDarkLoaderButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.DarkLoaderGroupBox = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.DarkLoaderTabPage = new System.Windows.Forms.TabPage();
            this.FMSelTabPage = new System.Windows.Forms.TabPage();
            this.NewDarkLoaderTabPage = new System.Windows.Forms.TabPage();
            this.FMSelGroupBox = new System.Windows.Forms.GroupBox();
            this.NewDarkLoaderGroupBox = new System.Windows.Forms.GroupBox();
            this.DarkLoaderIniBrowseButton = new System.Windows.Forms.Button();
            this.DarkLoaderIniTextBox = new System.Windows.Forms.TextBox();
            this.ChooseDarkLoaderIniLabel = new System.Windows.Forms.Label();
            this.DarkLoaderImportSavesCheckBox = new System.Windows.Forms.CheckBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.DarkLoaderImportFMDataCheckBox = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.DarkLoaderGroupBox.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // DarkLoaderButton
            // 
            this.DarkLoaderButton.Location = new System.Drawing.Point(704, 592);
            this.DarkLoaderButton.Name = "DarkLoaderButton";
            this.DarkLoaderButton.Size = new System.Drawing.Size(152, 23);
            this.DarkLoaderButton.TabIndex = 0;
            this.DarkLoaderButton.Text = "DarkLoader";
            this.DarkLoaderButton.UseVisualStyleBackColor = true;
            this.DarkLoaderButton.Click += new System.EventHandler(this.DarkLoaderButton_Click);
            // 
            // FMSelButton
            // 
            this.FMSelButton.Location = new System.Drawing.Point(704, 616);
            this.FMSelButton.Name = "FMSelButton";
            this.FMSelButton.Size = new System.Drawing.Size(152, 23);
            this.FMSelButton.TabIndex = 0;
            this.FMSelButton.Text = "FMSel";
            this.FMSelButton.UseVisualStyleBackColor = true;
            // 
            // NewDarkLoaderButton
            // 
            this.NewDarkLoaderButton.Location = new System.Drawing.Point(704, 640);
            this.NewDarkLoaderButton.Name = "NewDarkLoaderButton";
            this.NewDarkLoaderButton.Size = new System.Drawing.Size(152, 23);
            this.NewDarkLoaderButton.TabIndex = 0;
            this.NewDarkLoaderButton.Text = "NewDarkLoader";
            this.NewDarkLoaderButton.UseVisualStyleBackColor = true;
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(704, 680);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(152, 23);
            this.CloseButton.TabIndex = 1;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            // 
            // DarkLoaderGroupBox
            // 
            this.DarkLoaderGroupBox.Controls.Add(this.textBox1);
            this.DarkLoaderGroupBox.Controls.Add(this.radioButton3);
            this.DarkLoaderGroupBox.Controls.Add(this.radioButton2);
            this.DarkLoaderGroupBox.Controls.Add(this.radioButton1);
            this.DarkLoaderGroupBox.Controls.Add(this.DarkLoaderImportFMDataCheckBox);
            this.DarkLoaderGroupBox.Controls.Add(this.DarkLoaderImportSavesCheckBox);
            this.DarkLoaderGroupBox.Controls.Add(this.ChooseDarkLoaderIniLabel);
            this.DarkLoaderGroupBox.Controls.Add(this.DarkLoaderIniTextBox);
            this.DarkLoaderGroupBox.Controls.Add(this.DarkLoaderIniBrowseButton);
            this.DarkLoaderGroupBox.Location = new System.Drawing.Point(136, 24);
            this.DarkLoaderGroupBox.Name = "DarkLoaderGroupBox";
            this.DarkLoaderGroupBox.Size = new System.Drawing.Size(544, 216);
            this.DarkLoaderGroupBox.TabIndex = 2;
            this.DarkLoaderGroupBox.TabStop = false;
            this.DarkLoaderGroupBox.Text = "DarkLoader";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.DarkLoaderTabPage);
            this.tabControl1.Controls.Add(this.FMSelTabPage);
            this.tabControl1.Controls.Add(this.NewDarkLoaderTabPage);
            this.tabControl1.Location = new System.Drawing.Point(512, 608);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(160, 88);
            this.tabControl1.TabIndex = 3;
            // 
            // DarkLoaderTabPage
            // 
            this.DarkLoaderTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.DarkLoaderTabPage.Location = new System.Drawing.Point(4, 22);
            this.DarkLoaderTabPage.Name = "DarkLoaderTabPage";
            this.DarkLoaderTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DarkLoaderTabPage.Size = new System.Drawing.Size(672, 182);
            this.DarkLoaderTabPage.TabIndex = 0;
            this.DarkLoaderTabPage.Text = "DarkLoader";
            // 
            // FMSelTabPage
            // 
            this.FMSelTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.FMSelTabPage.Location = new System.Drawing.Point(4, 22);
            this.FMSelTabPage.Name = "FMSelTabPage";
            this.FMSelTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.FMSelTabPage.Size = new System.Drawing.Size(672, 182);
            this.FMSelTabPage.TabIndex = 1;
            this.FMSelTabPage.Text = "FMSel";
            // 
            // NewDarkLoaderTabPage
            // 
            this.NewDarkLoaderTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.NewDarkLoaderTabPage.Location = new System.Drawing.Point(4, 22);
            this.NewDarkLoaderTabPage.Name = "NewDarkLoaderTabPage";
            this.NewDarkLoaderTabPage.Size = new System.Drawing.Size(152, 62);
            this.NewDarkLoaderTabPage.TabIndex = 2;
            this.NewDarkLoaderTabPage.Text = "NewDarkLoader";
            // 
            // FMSelGroupBox
            // 
            this.FMSelGroupBox.Location = new System.Drawing.Point(8, 568);
            this.FMSelGroupBox.Name = "FMSelGroupBox";
            this.FMSelGroupBox.Size = new System.Drawing.Size(120, 72);
            this.FMSelGroupBox.TabIndex = 2;
            this.FMSelGroupBox.TabStop = false;
            this.FMSelGroupBox.Text = "FMSel";
            // 
            // NewDarkLoaderGroupBox
            // 
            this.NewDarkLoaderGroupBox.Location = new System.Drawing.Point(8, 648);
            this.NewDarkLoaderGroupBox.Name = "NewDarkLoaderGroupBox";
            this.NewDarkLoaderGroupBox.Size = new System.Drawing.Size(120, 64);
            this.NewDarkLoaderGroupBox.TabIndex = 2;
            this.NewDarkLoaderGroupBox.TabStop = false;
            this.NewDarkLoaderGroupBox.Text = "NewDarkLoader";
            // 
            // DarkLoaderIniBrowseButton
            // 
            this.DarkLoaderIniBrowseButton.Location = new System.Drawing.Point(456, 55);
            this.DarkLoaderIniBrowseButton.Name = "DarkLoaderIniBrowseButton";
            this.DarkLoaderIniBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.DarkLoaderIniBrowseButton.TabIndex = 0;
            this.DarkLoaderIniBrowseButton.Text = "Browse...";
            this.DarkLoaderIniBrowseButton.UseVisualStyleBackColor = true;
            // 
            // DarkLoaderIniTextBox
            // 
            this.DarkLoaderIniTextBox.Location = new System.Drawing.Point(16, 56);
            this.DarkLoaderIniTextBox.Name = "DarkLoaderIniTextBox";
            this.DarkLoaderIniTextBox.Size = new System.Drawing.Size(440, 20);
            this.DarkLoaderIniTextBox.TabIndex = 1;
            // 
            // ChooseDarkLoaderIniLabel
            // 
            this.ChooseDarkLoaderIniLabel.AutoSize = true;
            this.ChooseDarkLoaderIniLabel.Location = new System.Drawing.Point(16, 40);
            this.ChooseDarkLoaderIniLabel.Name = "ChooseDarkLoaderIniLabel";
            this.ChooseDarkLoaderIniLabel.Size = new System.Drawing.Size(118, 13);
            this.ChooseDarkLoaderIniLabel.TabIndex = 2;
            this.ChooseDarkLoaderIniLabel.Text = "Choose DarkLoader.ini:";
            // 
            // DarkLoaderImportSavesCheckBox
            // 
            this.DarkLoaderImportSavesCheckBox.AutoSize = true;
            this.DarkLoaderImportSavesCheckBox.Checked = true;
            this.DarkLoaderImportSavesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DarkLoaderImportSavesCheckBox.Location = new System.Drawing.Point(16, 96);
            this.DarkLoaderImportSavesCheckBox.Name = "DarkLoaderImportSavesCheckBox";
            this.DarkLoaderImportSavesCheckBox.Size = new System.Drawing.Size(86, 17);
            this.DarkLoaderImportSavesCheckBox.TabIndex = 3;
            this.DarkLoaderImportSavesCheckBox.Text = "Import saves";
            this.DarkLoaderImportSavesCheckBox.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(32, 112);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(192, 17);
            this.radioButton1.TabIndex = 4;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Detect saves folder(s) automatically";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(32, 128);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(132, 17);
            this.radioButton2.TabIndex = 4;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Choose saves folder(s)";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(448, 24);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(85, 17);
            this.radioButton3.TabIndex = 4;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "radioButton1";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // DarkLoaderImportFMDataCheckBox
            // 
            this.DarkLoaderImportFMDataCheckBox.AutoSize = true;
            this.DarkLoaderImportFMDataCheckBox.Checked = true;
            this.DarkLoaderImportFMDataCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DarkLoaderImportFMDataCheckBox.Location = new System.Drawing.Point(16, 80);
            this.DarkLoaderImportFMDataCheckBox.Name = "DarkLoaderImportFMDataCheckBox";
            this.DarkLoaderImportFMDataCheckBox.Size = new System.Drawing.Size(97, 17);
            this.DarkLoaderImportFMDataCheckBox.TabIndex = 3;
            this.DarkLoaderImportFMDataCheckBox.Text = "Import FM data";
            this.DarkLoaderImportFMDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(32, 152);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(240, 20);
            this.textBox1.TabIndex = 5;
            // 
            // ImportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(866, 733);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.NewDarkLoaderGroupBox);
            this.Controls.Add(this.FMSelGroupBox);
            this.Controls.Add(this.DarkLoaderGroupBox);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.NewDarkLoaderButton);
            this.Controls.Add(this.FMSelButton);
            this.Controls.Add(this.DarkLoaderButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import";
            this.DarkLoaderGroupBox.ResumeLayout(false);
            this.DarkLoaderGroupBox.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button DarkLoaderButton;
        private System.Windows.Forms.Button FMSelButton;
        private System.Windows.Forms.Button NewDarkLoaderButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.GroupBox DarkLoaderGroupBox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage DarkLoaderTabPage;
        private System.Windows.Forms.TabPage FMSelTabPage;
        private System.Windows.Forms.TabPage NewDarkLoaderTabPage;
        private System.Windows.Forms.GroupBox FMSelGroupBox;
        private System.Windows.Forms.GroupBox NewDarkLoaderGroupBox;
        private System.Windows.Forms.Label ChooseDarkLoaderIniLabel;
        private System.Windows.Forms.TextBox DarkLoaderIniTextBox;
        private System.Windows.Forms.Button DarkLoaderIniBrowseButton;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.CheckBox DarkLoaderImportFMDataCheckBox;
        private System.Windows.Forms.CheckBox DarkLoaderImportSavesCheckBox;
    }
}