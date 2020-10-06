namespace AngelLoader.Forms
{
    sealed partial class GlobalFMStatsForm
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
            this.AvailableFMsLabel = new System.Windows.Forms.Label();
            this.T1Label = new System.Windows.Forms.Label();
            this.T2Label = new System.Windows.Forms.Label();
            this.T3Label = new System.Windows.Forms.Label();
            this.SS2Label = new System.Windows.Forms.Label();
            this.UnscannedLabel = new System.Windows.Forms.Label();
            this.UnsupportedLabel = new System.Windows.Forms.Label();
            this.AvailableFMsTextBox = new System.Windows.Forms.TextBox();
            this.T1TextBox = new System.Windows.Forms.TextBox();
            this.T2TextBox = new System.Windows.Forms.TextBox();
            this.T3TextBox = new System.Windows.Forms.TextBox();
            this.SS2TextBox = new System.Windows.Forms.TextBox();
            this.UnscannedTextBox = new System.Windows.Forms.TextBox();
            this.UnsupportedTextBox = new System.Windows.Forms.TextBox();
            this.FMsInDatabaseLabel = new System.Windows.Forms.Label();
            this.FMsInDatabaseTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.OKButton.Location = new System.Drawing.Point(712, 416);
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // AvailableFMsLabel
            // 
            this.AvailableFMsLabel.AutoSize = true;
            this.AvailableFMsLabel.Location = new System.Drawing.Point(64, 75);
            this.AvailableFMsLabel.Name = "AvailableFMsLabel";
            this.AvailableFMsLabel.Size = new System.Drawing.Size(76, 13);
            this.AvailableFMsLabel.TabIndex = 1;
            this.AvailableFMsLabel.Text = "Available FMs:";
            // 
            // T1Label
            // 
            this.T1Label.AutoSize = true;
            this.T1Label.Location = new System.Drawing.Point(64, 99);
            this.T1Label.Name = "T1Label";
            this.T1Label.Size = new System.Drawing.Size(43, 13);
            this.T1Label.TabIndex = 1;
            this.T1Label.Text = "Thief 1:";
            // 
            // T2Label
            // 
            this.T2Label.AutoSize = true;
            this.T2Label.Location = new System.Drawing.Point(64, 123);
            this.T2Label.Name = "T2Label";
            this.T2Label.Size = new System.Drawing.Size(43, 13);
            this.T2Label.TabIndex = 1;
            this.T2Label.Text = "Thief 2:";
            // 
            // T3Label
            // 
            this.T3Label.AutoSize = true;
            this.T3Label.Location = new System.Drawing.Point(64, 147);
            this.T3Label.Name = "T3Label";
            this.T3Label.Size = new System.Drawing.Size(43, 13);
            this.T3Label.TabIndex = 1;
            this.T3Label.Text = "Thief 3:";
            // 
            // SS2Label
            // 
            this.SS2Label.AutoSize = true;
            this.SS2Label.Location = new System.Drawing.Point(64, 171);
            this.SS2Label.Name = "SS2Label";
            this.SS2Label.Size = new System.Drawing.Size(87, 13);
            this.SS2Label.TabIndex = 1;
            this.SS2Label.Text = "System Shock 2:";
            // 
            // UnscannedLabel
            // 
            this.UnscannedLabel.AutoSize = true;
            this.UnscannedLabel.Location = new System.Drawing.Point(64, 195);
            this.UnscannedLabel.Name = "UnscannedLabel";
            this.UnscannedLabel.Size = new System.Drawing.Size(65, 13);
            this.UnscannedLabel.TabIndex = 1;
            this.UnscannedLabel.Text = "Unscanned:";
            // 
            // UnsupportedLabel
            // 
            this.UnsupportedLabel.AutoSize = true;
            this.UnsupportedLabel.Location = new System.Drawing.Point(64, 219);
            this.UnsupportedLabel.Name = "UnsupportedLabel";
            this.UnsupportedLabel.Size = new System.Drawing.Size(115, 13);
            this.UnsupportedLabel.TabIndex = 1;
            this.UnsupportedLabel.Text = "Invalid or unsupported:";
            // 
            // AvailableFMsTextBox
            // 
            this.AvailableFMsTextBox.Location = new System.Drawing.Point(200, 72);
            this.AvailableFMsTextBox.Name = "AvailableFMsTextBox";
            this.AvailableFMsTextBox.ReadOnly = true;
            this.AvailableFMsTextBox.Size = new System.Drawing.Size(100, 20);
            this.AvailableFMsTextBox.TabIndex = 2;
            // 
            // T1TextBox
            // 
            this.T1TextBox.Location = new System.Drawing.Point(200, 96);
            this.T1TextBox.Name = "T1TextBox";
            this.T1TextBox.ReadOnly = true;
            this.T1TextBox.Size = new System.Drawing.Size(100, 20);
            this.T1TextBox.TabIndex = 2;
            // 
            // T2TextBox
            // 
            this.T2TextBox.Location = new System.Drawing.Point(200, 120);
            this.T2TextBox.Name = "T2TextBox";
            this.T2TextBox.ReadOnly = true;
            this.T2TextBox.Size = new System.Drawing.Size(100, 20);
            this.T2TextBox.TabIndex = 2;
            // 
            // T3TextBox
            // 
            this.T3TextBox.Location = new System.Drawing.Point(200, 144);
            this.T3TextBox.Name = "T3TextBox";
            this.T3TextBox.ReadOnly = true;
            this.T3TextBox.Size = new System.Drawing.Size(100, 20);
            this.T3TextBox.TabIndex = 2;
            // 
            // SS2TextBox
            // 
            this.SS2TextBox.Location = new System.Drawing.Point(200, 168);
            this.SS2TextBox.Name = "SS2TextBox";
            this.SS2TextBox.ReadOnly = true;
            this.SS2TextBox.Size = new System.Drawing.Size(100, 20);
            this.SS2TextBox.TabIndex = 2;
            // 
            // UnscannedTextBox
            // 
            this.UnscannedTextBox.Location = new System.Drawing.Point(200, 192);
            this.UnscannedTextBox.Name = "UnscannedTextBox";
            this.UnscannedTextBox.ReadOnly = true;
            this.UnscannedTextBox.Size = new System.Drawing.Size(100, 20);
            this.UnscannedTextBox.TabIndex = 2;
            // 
            // UnsupportedTextBox
            // 
            this.UnsupportedTextBox.Location = new System.Drawing.Point(200, 216);
            this.UnsupportedTextBox.Name = "UnsupportedTextBox";
            this.UnsupportedTextBox.ReadOnly = true;
            this.UnsupportedTextBox.Size = new System.Drawing.Size(100, 20);
            this.UnsupportedTextBox.TabIndex = 2;
            // 
            // FMsInDatabaseLabel
            // 
            this.FMsInDatabaseLabel.AutoSize = true;
            this.FMsInDatabaseLabel.Location = new System.Drawing.Point(64, 51);
            this.FMsInDatabaseLabel.Name = "FMsInDatabaseLabel";
            this.FMsInDatabaseLabel.Size = new System.Drawing.Size(88, 13);
            this.FMsInDatabaseLabel.TabIndex = 1;
            this.FMsInDatabaseLabel.Text = "FMs in database:";
            // 
            // FMsInDatabaseTextBox
            // 
            this.FMsInDatabaseTextBox.Location = new System.Drawing.Point(200, 48);
            this.FMsInDatabaseTextBox.Name = "FMsInDatabaseTextBox";
            this.FMsInDatabaseTextBox.ReadOnly = true;
            this.FMsInDatabaseTextBox.Size = new System.Drawing.Size(100, 20);
            this.FMsInDatabaseTextBox.TabIndex = 2;
            // 
            // GlobalFMStatsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.OKButton;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.UnsupportedTextBox);
            this.Controls.Add(this.UnscannedTextBox);
            this.Controls.Add(this.T3TextBox);
            this.Controls.Add(this.SS2TextBox);
            this.Controls.Add(this.T1TextBox);
            this.Controls.Add(this.T2TextBox);
            this.Controls.Add(this.FMsInDatabaseTextBox);
            this.Controls.Add(this.AvailableFMsTextBox);
            this.Controls.Add(this.UnsupportedLabel);
            this.Controls.Add(this.UnscannedLabel);
            this.Controls.Add(this.SS2Label);
            this.Controls.Add(this.T3Label);
            this.Controls.Add(this.T2Label);
            this.Controls.Add(this.T1Label);
            this.Controls.Add(this.FMsInDatabaseLabel);
            this.Controls.Add(this.AvailableFMsLabel);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GlobalFMStatsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Global FM stats";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Label AvailableFMsLabel;
        private System.Windows.Forms.Label T1Label;
        private System.Windows.Forms.Label T2Label;
        private System.Windows.Forms.Label T3Label;
        private System.Windows.Forms.Label SS2Label;
        private System.Windows.Forms.Label UnscannedLabel;
        private System.Windows.Forms.Label UnsupportedLabel;
        private System.Windows.Forms.TextBox AvailableFMsTextBox;
        private System.Windows.Forms.TextBox T1TextBox;
        private System.Windows.Forms.TextBox T2TextBox;
        private System.Windows.Forms.TextBox T3TextBox;
        private System.Windows.Forms.TextBox SS2TextBox;
        private System.Windows.Forms.TextBox UnscannedTextBox;
        private System.Windows.Forms.TextBox UnsupportedTextBox;
        private System.Windows.Forms.Label FMsInDatabaseLabel;
        private System.Windows.Forms.TextBox FMsInDatabaseTextBox;
    }
}