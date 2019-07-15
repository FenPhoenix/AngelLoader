namespace AngelLoader.Forms
{
    partial class FilterTagsForm
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
            this.components = new System.ComponentModel.Container();
            this.OriginTreeView = new AngelLoader.CustomControls.TreeViewCustom();
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
            this.toolStripCustom1 = new AngelLoader.CustomControls.ToolStripCustom();
            this.toolStripSeparatorCustom1 = new AngelLoader.CustomControls.ToolStripSeparatorCustom();
            this.MoveButtonsPanel = new System.Windows.Forms.Panel();
            this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.FindTagTextBox = new System.Windows.Forms.TextBox();
            this.FilterLabelsPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.toolStripCustom1.SuspendLayout();
            this.MoveButtonsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // OriginTreeView
            // 
            this.OriginTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.OriginTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.OriginTreeView.HideSelection = false;
            this.OriginTreeView.Location = new System.Drawing.Point(8, 32);
            this.OriginTreeView.Name = "OriginTreeView";
            this.OriginTreeView.Size = new System.Drawing.Size(224, 624);
            this.OriginTreeView.TabIndex = 0;
            this.OriginTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OriginTreeView_AfterSelect);
            // 
            // AndTreeView
            // 
            this.AndTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.AndTreeView.HideSelection = false;
            this.AndTreeView.Location = new System.Drawing.Point(344, 24);
            this.AndTreeView.Name = "AndTreeView";
            this.AndTreeView.Size = new System.Drawing.Size(224, 632);
            this.AndTreeView.TabIndex = 3;
            // 
            // OrTreeView
            // 
            this.OrTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.OrTreeView.HideSelection = false;
            this.OrTreeView.Location = new System.Drawing.Point(576, 24);
            this.OrTreeView.Name = "OrTreeView";
            this.OrTreeView.Size = new System.Drawing.Size(224, 632);
            this.OrTreeView.TabIndex = 4;
            // 
            // NotTreeView
            // 
            this.NotTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.NotTreeView.HideSelection = false;
            this.NotTreeView.Location = new System.Drawing.Point(808, 24);
            this.NotTreeView.Name = "NotTreeView";
            this.NotTreeView.Size = new System.Drawing.Size(224, 632);
            this.NotTreeView.TabIndex = 5;
            // 
            // AndButton
            // 
            this.AndButton.AutoSize = true;
            this.AndButton.Image = global::AngelLoader.Forms.Images.ArrowRightSmall;
            this.AndButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AndButton.Location = new System.Drawing.Point(16, 0);
            this.AndButton.Name = "AndButton";
            this.AndButton.Size = new System.Drawing.Size(80, 23);
            this.AndButton.TabIndex = 0;
            this.AndButton.Text = "All";
            this.AndButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AndButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.AndButton.UseVisualStyleBackColor = true;
            this.AndButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
            // 
            // OrButton
            // 
            this.OrButton.AutoSize = true;
            this.OrButton.Image = global::AngelLoader.Forms.Images.ArrowRightSmall;
            this.OrButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.OrButton.Location = new System.Drawing.Point(16, 23);
            this.OrButton.Name = "OrButton";
            this.OrButton.Size = new System.Drawing.Size(80, 23);
            this.OrButton.TabIndex = 1;
            this.OrButton.Text = "Any";
            this.OrButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.OrButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.OrButton.UseVisualStyleBackColor = true;
            this.OrButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
            // 
            // NotButton
            // 
            this.NotButton.AutoSize = true;
            this.NotButton.Image = global::AngelLoader.Forms.Images.ArrowRightSmall;
            this.NotButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.NotButton.Location = new System.Drawing.Point(16, 46);
            this.NotButton.Name = "NotButton";
            this.NotButton.Size = new System.Drawing.Size(80, 23);
            this.NotButton.TabIndex = 2;
            this.NotButton.Text = "Exclude";
            this.NotButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.NotButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.NotButton.UseVisualStyleBackColor = true;
            this.NotButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
            // 
            // IncludeAllLabel
            // 
            this.IncludeAllLabel.AutoSize = true;
            this.IncludeAllLabel.Location = new System.Drawing.Point(0, 8);
            this.IncludeAllLabel.Name = "IncludeAllLabel";
            this.IncludeAllLabel.Size = new System.Drawing.Size(59, 13);
            this.IncludeAllLabel.TabIndex = 0;
            this.IncludeAllLabel.Text = "Include All:";
            // 
            // IncludeAnyLabel
            // 
            this.IncludeAnyLabel.AutoSize = true;
            this.IncludeAnyLabel.Location = new System.Drawing.Point(232, 8);
            this.IncludeAnyLabel.Name = "IncludeAnyLabel";
            this.IncludeAnyLabel.Size = new System.Drawing.Size(66, 13);
            this.IncludeAnyLabel.TabIndex = 3;
            this.IncludeAnyLabel.Text = "Include Any:";
            // 
            // ExcludeLabel
            // 
            this.ExcludeLabel.AutoSize = true;
            this.ExcludeLabel.Location = new System.Drawing.Point(464, 8);
            this.ExcludeLabel.Name = "ExcludeLabel";
            this.ExcludeLabel.Size = new System.Drawing.Size(48, 13);
            this.ExcludeLabel.TabIndex = 6;
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
            this.FilterLabelsPanel.Location = new System.Drawing.Point(344, 0);
            this.FilterLabelsPanel.Name = "FilterLabelsPanel";
            this.FilterLabelsPanel.Size = new System.Drawing.Size(688, 24);
            this.FilterLabelsPanel.TabIndex = 2;
            // 
            // RemoveAllNotButton
            // 
            this.RemoveAllNotButton.Location = new System.Drawing.Point(666, 0);
            this.RemoveAllNotButton.Name = "RemoveAllNotButton";
            this.RemoveAllNotButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveAllNotButton.TabIndex = 8;
            this.RemoveAllNotButton.UseVisualStyleBackColor = true;
            this.RemoveAllNotButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
            this.RemoveAllNotButton.Paint += new System.Windows.Forms.PaintEventHandler(this.RemoveAllButtons_Paint);
            // 
            // RemoveAllOrButton
            // 
            this.RemoveAllOrButton.Location = new System.Drawing.Point(434, 0);
            this.RemoveAllOrButton.Name = "RemoveAllOrButton";
            this.RemoveAllOrButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveAllOrButton.TabIndex = 5;
            this.RemoveAllOrButton.UseVisualStyleBackColor = true;
            this.RemoveAllOrButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
            this.RemoveAllOrButton.Paint += new System.Windows.Forms.PaintEventHandler(this.RemoveAllButtons_Paint);
            // 
            // RemoveAllAndButton
            // 
            this.RemoveAllAndButton.Location = new System.Drawing.Point(202, 0);
            this.RemoveAllAndButton.Name = "RemoveAllAndButton";
            this.RemoveAllAndButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveAllAndButton.TabIndex = 2;
            this.RemoveAllAndButton.UseVisualStyleBackColor = true;
            this.RemoveAllAndButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
            this.RemoveAllAndButton.Paint += new System.Windows.Forms.PaintEventHandler(this.RemoveAllButtons_Paint);
            // 
            // RemoveSelectedNotButton
            // 
            this.RemoveSelectedNotButton.Location = new System.Drawing.Point(643, 0);
            this.RemoveSelectedNotButton.Name = "RemoveSelectedNotButton";
            this.RemoveSelectedNotButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveSelectedNotButton.TabIndex = 7;
            this.RemoveSelectedNotButton.UseVisualStyleBackColor = true;
            this.RemoveSelectedNotButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
            this.RemoveSelectedNotButton.Paint += new System.Windows.Forms.PaintEventHandler(this.RemoveButtons_Paint);
            // 
            // RemoveSelectedOrButton
            // 
            this.RemoveSelectedOrButton.Location = new System.Drawing.Point(411, 0);
            this.RemoveSelectedOrButton.Name = "RemoveSelectedOrButton";
            this.RemoveSelectedOrButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveSelectedOrButton.TabIndex = 4;
            this.RemoveSelectedOrButton.UseVisualStyleBackColor = true;
            this.RemoveSelectedOrButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
            this.RemoveSelectedOrButton.Paint += new System.Windows.Forms.PaintEventHandler(this.RemoveButtons_Paint);
            // 
            // RemoveSelectedAndButton
            // 
            this.RemoveSelectedAndButton.Location = new System.Drawing.Point(179, 0);
            this.RemoveSelectedAndButton.Name = "RemoveSelectedAndButton";
            this.RemoveSelectedAndButton.Size = new System.Drawing.Size(23, 23);
            this.RemoveSelectedAndButton.TabIndex = 1;
            this.RemoveSelectedAndButton.UseVisualStyleBackColor = true;
            this.RemoveSelectedAndButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
            this.RemoveSelectedAndButton.Paint += new System.Windows.Forms.PaintEventHandler(this.RemoveButtons_Paint);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKButton.AutoSize = true;
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(881, 4);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(962, 4);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 3;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // ResetButton
            // 
            this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ResetButton.AutoSize = true;
            this.ResetButton.Location = new System.Drawing.Point(792, 4);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ResetButton.Size = new System.Drawing.Size(75, 23);
            this.ResetButton.TabIndex = 0;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.flowLayoutPanel1.Controls.Add(this.Cancel_Button);
            this.flowLayoutPanel1.Controls.Add(this.OKButton);
            this.flowLayoutPanel1.Controls.Add(this.toolStripCustom1);
            this.flowLayoutPanel1.Controls.Add(this.ResetButton);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 660);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1040, 32);
            this.flowLayoutPanel1.TabIndex = 6;
            // 
            // toolStripCustom1
            // 
            this.toolStripCustom1.GripMargin = new System.Windows.Forms.Padding(0);
            this.toolStripCustom1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripCustom1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripSeparatorCustom1});
            this.toolStripCustom1.Location = new System.Drawing.Point(870, 0);
            this.toolStripCustom1.Name = "toolStripCustom1";
            this.toolStripCustom1.Padding = new System.Windows.Forms.Padding(0);
            this.toolStripCustom1.PaddingDrawNudge = 0;
            this.toolStripCustom1.Size = new System.Drawing.Size(8, 30);
            this.toolStripCustom1.TabIndex = 7;
            this.toolStripCustom1.Text = "toolStripCustom1";
            // 
            // toolStripSeparatorCustom1
            // 
            this.toolStripSeparatorCustom1.AutoSize = false;
            this.toolStripSeparatorCustom1.Name = "toolStripSeparatorCustom1";
            this.toolStripSeparatorCustom1.Size = new System.Drawing.Size(6, 30);
            // 
            // MoveButtonsPanel
            // 
            this.MoveButtonsPanel.Controls.Add(this.AndButton);
            this.MoveButtonsPanel.Controls.Add(this.OrButton);
            this.MoveButtonsPanel.Controls.Add(this.NotButton);
            this.MoveButtonsPanel.Location = new System.Drawing.Point(232, 104);
            this.MoveButtonsPanel.Name = "MoveButtonsPanel";
            this.MoveButtonsPanel.Size = new System.Drawing.Size(112, 552);
            this.MoveButtonsPanel.TabIndex = 1;
            // 
            // FindTagTextBox
            // 
            this.FindTagTextBox.Location = new System.Drawing.Point(8, 8);
            this.FindTagTextBox.Name = "FindTagTextBox";
            this.FindTagTextBox.Size = new System.Drawing.Size(224, 20);
            this.FindTagTextBox.TabIndex = 7;
            this.FindTagTextBox.TextChanged += new System.EventHandler(this.FindTagTextBox_TextChanged);
            // 
            // FilterTagsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(1040, 692);
            this.Controls.Add(this.FindTagTextBox);
            this.Controls.Add(this.MoveButtonsPanel);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.FilterLabelsPanel);
            this.Controls.Add(this.NotTreeView);
            this.Controls.Add(this.OrTreeView);
            this.Controls.Add(this.AndTreeView);
            this.Controls.Add(this.OriginTreeView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = global::AngelLoader.Properties.Resources.AngelLoader;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1056, 32767);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1056, 242);
            this.Name = "FilterTagsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set Tags Filter";
            this.Load += new System.EventHandler(this.FilterTagsForm_Load);
            this.FilterLabelsPanel.ResumeLayout(false);
            this.FilterLabelsPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.toolStripCustom1.ResumeLayout(false);
            this.toolStripCustom1.PerformLayout();
            this.MoveButtonsPanel.ResumeLayout(false);
            this.MoveButtonsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AngelLoader.CustomControls.TreeViewCustom OriginTreeView;
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
        private CustomControls.ToolStripCustom toolStripCustom1;
        private CustomControls.ToolStripSeparatorCustom toolStripSeparatorCustom1;
        private System.Windows.Forms.Panel MoveButtonsPanel;
        private System.Windows.Forms.ToolTip MainToolTip;
        private System.Windows.Forms.TextBox FindTagTextBox;
    }
}