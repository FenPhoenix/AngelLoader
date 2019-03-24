namespace AngelLoader.Forms
{
    partial class FilterTagsForm2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterTagsForm2));
            this.OriginTreeView = new System.Windows.Forms.TreeView();
            this.AndTreeView = new System.Windows.Forms.TreeView();
            this.OrTreeView = new System.Windows.Forms.TreeView();
            this.NotTreeView = new System.Windows.Forms.TreeView();
            this.AndButton = new System.Windows.Forms.Button();
            this.OrButton = new System.Windows.Forms.Button();
            this.NotButton = new System.Windows.Forms.Button();
            this.IncludeAllLabel = new System.Windows.Forms.Label();
            this.IncludeAnyLabel = new System.Windows.Forms.Label();
            this.ExcludeLabel = new System.Windows.Forms.Label();
            this.FilterLabelsPanel = new System.Windows.Forms.Panel();
            this.RemoveAllNotButton = new System.Windows.Forms.Button();
            this.RemoveAllOrButton = new System.Windows.Forms.Button();
            this.RemoveAllAndButton = new System.Windows.Forms.Button();
            this.RemoveSelectedNotButton = new System.Windows.Forms.Button();
            this.RemoveSelectedOrButton = new System.Windows.Forms.Button();
            this.RemoveSelectedAndButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.FilterLabelsPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // OriginTreeView
            // 
            this.OriginTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.OriginTreeView.Location = new System.Drawing.Point(8, 8);
            this.OriginTreeView.Name = "OriginTreeView";
            this.OriginTreeView.Size = new System.Drawing.Size(224, 648);
            this.OriginTreeView.TabIndex = 0;
            this.OriginTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OriginTreeView_AfterSelect);
            // 
            // AndTreeView
            // 
            this.AndTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.AndTreeView.Location = new System.Drawing.Point(328, 24);
            this.AndTreeView.Name = "AndTreeView";
            this.AndTreeView.Size = new System.Drawing.Size(224, 632);
            this.AndTreeView.TabIndex = 0;
            // 
            // OrTreeView
            // 
            this.OrTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.OrTreeView.Location = new System.Drawing.Point(560, 24);
            this.OrTreeView.Name = "OrTreeView";
            this.OrTreeView.Size = new System.Drawing.Size(224, 632);
            this.OrTreeView.TabIndex = 0;
            // 
            // NotTreeView
            // 
            this.NotTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.NotTreeView.Location = new System.Drawing.Point(792, 24);
            this.NotTreeView.Name = "NotTreeView";
            this.NotTreeView.Size = new System.Drawing.Size(224, 632);
            this.NotTreeView.TabIndex = 0;
            // 
            // AndButton
            // 
            this.AndButton.Location = new System.Drawing.Point(240, 48);
            this.AndButton.Name = "AndButton";
            this.AndButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.AndButton.Size = new System.Drawing.Size(80, 24);
            this.AndButton.TabIndex = 1;
            this.AndButton.Text = "-> All";
            this.AndButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AndButton.UseVisualStyleBackColor = true;
            this.AndButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
            // 
            // OrButton
            // 
            this.OrButton.Location = new System.Drawing.Point(240, 72);
            this.OrButton.Name = "OrButton";
            this.OrButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OrButton.Size = new System.Drawing.Size(80, 24);
            this.OrButton.TabIndex = 1;
            this.OrButton.Text = "-> Any";
            this.OrButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.OrButton.UseVisualStyleBackColor = true;
            this.OrButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
            // 
            // NotButton
            // 
            this.NotButton.Location = new System.Drawing.Point(240, 96);
            this.NotButton.Name = "NotButton";
            this.NotButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.NotButton.Size = new System.Drawing.Size(80, 24);
            this.NotButton.TabIndex = 1;
            this.NotButton.Text = "-> Exclude";
            this.NotButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.NotButton.UseVisualStyleBackColor = true;
            this.NotButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
            // 
            // IncludeAllLabel
            // 
            this.IncludeAllLabel.AutoSize = true;
            this.IncludeAllLabel.Location = new System.Drawing.Point(0, 8);
            this.IncludeAllLabel.Name = "IncludeAllLabel";
            this.IncludeAllLabel.Size = new System.Drawing.Size(59, 13);
            this.IncludeAllLabel.TabIndex = 2;
            this.IncludeAllLabel.Text = "Include All:";
            // 
            // IncludeAnyLabel
            // 
            this.IncludeAnyLabel.AutoSize = true;
            this.IncludeAnyLabel.Location = new System.Drawing.Point(232, 8);
            this.IncludeAnyLabel.Name = "IncludeAnyLabel";
            this.IncludeAnyLabel.Size = new System.Drawing.Size(66, 13);
            this.IncludeAnyLabel.TabIndex = 2;
            this.IncludeAnyLabel.Text = "Include Any:";
            // 
            // ExcludeLabel
            // 
            this.ExcludeLabel.AutoSize = true;
            this.ExcludeLabel.Location = new System.Drawing.Point(464, 8);
            this.ExcludeLabel.Name = "ExcludeLabel";
            this.ExcludeLabel.Size = new System.Drawing.Size(48, 13);
            this.ExcludeLabel.TabIndex = 2;
            this.ExcludeLabel.Text = "Exclude:";
            // 
            // FilterLabelsPanel
            // 
            this.FilterLabelsPanel.Controls.Add(this.RemoveAllNotButton);
            this.FilterLabelsPanel.Controls.Add(this.RemoveAllOrButton);
            this.FilterLabelsPanel.Controls.Add(this.RemoveAllAndButton);
            this.FilterLabelsPanel.Controls.Add(this.IncludeAllLabel);
            this.FilterLabelsPanel.Controls.Add(this.ExcludeLabel);
            this.FilterLabelsPanel.Controls.Add(this.RemoveSelectedNotButton);
            this.FilterLabelsPanel.Controls.Add(this.RemoveSelectedOrButton);
            this.FilterLabelsPanel.Controls.Add(this.RemoveSelectedAndButton);
            this.FilterLabelsPanel.Controls.Add(this.IncludeAnyLabel);
            this.FilterLabelsPanel.Location = new System.Drawing.Point(328, 0);
            this.FilterLabelsPanel.Name = "FilterLabelsPanel";
            this.FilterLabelsPanel.Size = new System.Drawing.Size(688, 24);
            this.FilterLabelsPanel.TabIndex = 4;
            // 
            // RemoveAllNotButton
            // 
            this.RemoveAllNotButton.Location = new System.Drawing.Point(663, 0);
            this.RemoveAllNotButton.Name = "RemoveAllNotButton";
            this.RemoveAllNotButton.Size = new System.Drawing.Size(26, 23);
            this.RemoveAllNotButton.TabIndex = 6;
            this.RemoveAllNotButton.Text = "-A";
            this.RemoveAllNotButton.UseVisualStyleBackColor = true;
            this.RemoveAllNotButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
            // 
            // RemoveAllOrButton
            // 
            this.RemoveAllOrButton.Location = new System.Drawing.Point(431, 0);
            this.RemoveAllOrButton.Name = "RemoveAllOrButton";
            this.RemoveAllOrButton.Size = new System.Drawing.Size(26, 23);
            this.RemoveAllOrButton.TabIndex = 6;
            this.RemoveAllOrButton.Text = "-A";
            this.RemoveAllOrButton.UseVisualStyleBackColor = true;
            this.RemoveAllOrButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
            // 
            // RemoveAllAndButton
            // 
            this.RemoveAllAndButton.Location = new System.Drawing.Point(199, 0);
            this.RemoveAllAndButton.Name = "RemoveAllAndButton";
            this.RemoveAllAndButton.Size = new System.Drawing.Size(26, 23);
            this.RemoveAllAndButton.TabIndex = 6;
            this.RemoveAllAndButton.Text = "-A";
            this.RemoveAllAndButton.UseVisualStyleBackColor = true;
            this.RemoveAllAndButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
            // 
            // RemoveSelectedNotButton
            // 
            this.RemoveSelectedNotButton.Location = new System.Drawing.Point(636, 0);
            this.RemoveSelectedNotButton.Name = "RemoveSelectedNotButton";
            this.RemoveSelectedNotButton.Size = new System.Drawing.Size(28, 23);
            this.RemoveSelectedNotButton.TabIndex = 6;
            this.RemoveSelectedNotButton.Text = "-";
            this.RemoveSelectedNotButton.UseVisualStyleBackColor = true;
            this.RemoveSelectedNotButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
            // 
            // RemoveSelectedOrButton
            // 
            this.RemoveSelectedOrButton.Location = new System.Drawing.Point(404, 0);
            this.RemoveSelectedOrButton.Name = "RemoveSelectedOrButton";
            this.RemoveSelectedOrButton.Size = new System.Drawing.Size(28, 23);
            this.RemoveSelectedOrButton.TabIndex = 6;
            this.RemoveSelectedOrButton.Text = "-";
            this.RemoveSelectedOrButton.UseVisualStyleBackColor = true;
            this.RemoveSelectedOrButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
            // 
            // RemoveSelectedAndButton
            // 
            this.RemoveSelectedAndButton.Location = new System.Drawing.Point(172, 0);
            this.RemoveSelectedAndButton.Name = "RemoveSelectedAndButton";
            this.RemoveSelectedAndButton.Size = new System.Drawing.Size(28, 23);
            this.RemoveSelectedAndButton.TabIndex = 6;
            this.RemoveSelectedAndButton.Text = "-";
            this.RemoveSelectedAndButton.UseVisualStyleBackColor = true;
            this.RemoveSelectedAndButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(861, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 5;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(942, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 5;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(240, 128);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ResetButton.Size = new System.Drawing.Size(80, 24);
            this.ResetButton.TabIndex = 6;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.flowLayoutPanel1.Controls.Add(this.Cancel_Button);
            this.flowLayoutPanel1.Controls.Add(this.OKButton);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 660);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1020, 32);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // FilterTagsForm2
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(1024, 692);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.FilterLabelsPanel);
            this.Controls.Add(this.NotButton);
            this.Controls.Add(this.OrButton);
            this.Controls.Add(this.AndButton);
            this.Controls.Add(this.NotTreeView);
            this.Controls.Add(this.OrTreeView);
            this.Controls.Add(this.AndTreeView);
            this.Controls.Add(this.OriginTreeView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1040, 32767);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1040, 160);
            this.Name = "FilterTagsForm2";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set Tags Filter";
            this.Load += new System.EventHandler(this.FilterTagsForm2_Load);
            this.FilterLabelsPanel.ResumeLayout(false);
            this.FilterLabelsPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView OriginTreeView;
        private System.Windows.Forms.TreeView AndTreeView;
        private System.Windows.Forms.TreeView OrTreeView;
        private System.Windows.Forms.TreeView NotTreeView;
        private System.Windows.Forms.Button AndButton;
        private System.Windows.Forms.Button OrButton;
        private System.Windows.Forms.Button NotButton;
        private System.Windows.Forms.Label IncludeAllLabel;
        private System.Windows.Forms.Label IncludeAnyLabel;
        private System.Windows.Forms.Label ExcludeLabel;
        private System.Windows.Forms.Panel FilterLabelsPanel;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button RemoveSelectedAndButton;
        private System.Windows.Forms.Button RemoveAllAndButton;
        private System.Windows.Forms.Button RemoveAllNotButton;
        private System.Windows.Forms.Button RemoveAllOrButton;
        private System.Windows.Forms.Button RemoveSelectedNotButton;
        private System.Windows.Forms.Button RemoveSelectedOrButton;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}