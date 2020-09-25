namespace AngelLoader.Forms
{
    partial class GameInfoForm
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
            this.T1VersionLabel = new System.Windows.Forms.Label();
            this.T1VersionTextBox = new System.Windows.Forms.TextBox();
            this.T2VersionLabel = new System.Windows.Forms.Label();
            this.T2VersionTextBox = new System.Windows.Forms.TextBox();
            this.T3VersionLabel = new System.Windows.Forms.Label();
            this.T3VersionTextBox = new System.Windows.Forms.TextBox();
            this.SS2VersionLabel = new System.Windows.Forms.Label();
            this.SS2VersionTextBox = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // T1VersionLabel
            // 
            this.T1VersionLabel.AutoSize = true;
            this.T1VersionLabel.Location = new System.Drawing.Point(88, 67);
            this.T1VersionLabel.Name = "T1VersionLabel";
            this.T1VersionLabel.Size = new System.Drawing.Size(43, 13);
            this.T1VersionLabel.TabIndex = 1;
            this.T1VersionLabel.Text = "Thief 1:";
            // 
            // T1VersionTextBox
            // 
            this.T1VersionTextBox.Location = new System.Drawing.Point(224, 64);
            this.T1VersionTextBox.Name = "T1VersionTextBox";
            this.T1VersionTextBox.ReadOnly = true;
            this.T1VersionTextBox.Size = new System.Drawing.Size(224, 20);
            this.T1VersionTextBox.TabIndex = 2;
            // 
            // T2VersionLabel
            // 
            this.T2VersionLabel.AutoSize = true;
            this.T2VersionLabel.Location = new System.Drawing.Point(88, 91);
            this.T2VersionLabel.Name = "T2VersionLabel";
            this.T2VersionLabel.Size = new System.Drawing.Size(43, 13);
            this.T2VersionLabel.TabIndex = 3;
            this.T2VersionLabel.Text = "Thief 2:";
            // 
            // T2VersionTextBox
            // 
            this.T2VersionTextBox.Location = new System.Drawing.Point(224, 88);
            this.T2VersionTextBox.Name = "T2VersionTextBox";
            this.T2VersionTextBox.ReadOnly = true;
            this.T2VersionTextBox.Size = new System.Drawing.Size(224, 20);
            this.T2VersionTextBox.TabIndex = 4;
            // 
            // T3VersionLabel
            // 
            this.T3VersionLabel.AutoSize = true;
            this.T3VersionLabel.Location = new System.Drawing.Point(88, 115);
            this.T3VersionLabel.Name = "T3VersionLabel";
            this.T3VersionLabel.Size = new System.Drawing.Size(43, 13);
            this.T3VersionLabel.TabIndex = 5;
            this.T3VersionLabel.Text = "Thief 3:";
            // 
            // T3VersionTextBox
            // 
            this.T3VersionTextBox.Location = new System.Drawing.Point(224, 112);
            this.T3VersionTextBox.Name = "T3VersionTextBox";
            this.T3VersionTextBox.ReadOnly = true;
            this.T3VersionTextBox.Size = new System.Drawing.Size(224, 20);
            this.T3VersionTextBox.TabIndex = 6;
            // 
            // SS2VersionLabel
            // 
            this.SS2VersionLabel.AutoSize = true;
            this.SS2VersionLabel.Location = new System.Drawing.Point(88, 139);
            this.SS2VersionLabel.Name = "SS2VersionLabel";
            this.SS2VersionLabel.Size = new System.Drawing.Size(87, 13);
            this.SS2VersionLabel.TabIndex = 7;
            this.SS2VersionLabel.Text = "System Shock 2:";
            // 
            // SS2VersionTextBox
            // 
            this.SS2VersionTextBox.Location = new System.Drawing.Point(224, 136);
            this.SS2VersionTextBox.Name = "SS2VersionTextBox";
            this.SS2VersionTextBox.ReadOnly = true;
            this.SS2VersionTextBox.Size = new System.Drawing.Size(224, 20);
            this.SS2VersionTextBox.TabIndex = 8;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.OKButton.Location = new System.Drawing.Point(481, 337);
            this.OKButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // GameInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.OKButton;
            this.ClientSize = new System.Drawing.Size(564, 367);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.SS2VersionTextBox);
            this.Controls.Add(this.SS2VersionLabel);
            this.Controls.Add(this.T3VersionTextBox);
            this.Controls.Add(this.T3VersionLabel);
            this.Controls.Add(this.T2VersionTextBox);
            this.Controls.Add(this.T2VersionLabel);
            this.Controls.Add(this.T1VersionTextBox);
            this.Controls.Add(this.T1VersionLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameInfoForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Game info";
            this.Load += new System.EventHandler(this.GameInfoForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label T1VersionLabel;
        private System.Windows.Forms.TextBox T1VersionTextBox;
        private System.Windows.Forms.Label T2VersionLabel;
        private System.Windows.Forms.TextBox T2VersionTextBox;
        private System.Windows.Forms.Label T3VersionLabel;
        private System.Windows.Forms.TextBox T3VersionTextBox;
        private System.Windows.Forms.Label SS2VersionLabel;
        private System.Windows.Forms.TextBox SS2VersionTextBox;
        private System.Windows.Forms.Button OKButton;
    }
}