namespace AngelLoader.Forms;

sealed partial class FilterTagsForm
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.components = new System.ComponentModel.Container();
        this.OriginTreeView = new AngelLoader.Forms.CustomControls.DarkTreeView();
        this.AndTreeView = new AngelLoader.Forms.CustomControls.DarkTreeView();
        this.OrTreeView = new AngelLoader.Forms.CustomControls.DarkTreeView();
        this.NotTreeView = new AngelLoader.Forms.CustomControls.DarkTreeView();
        this.AndButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.OrButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.NotButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.IncludeAllLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.IncludeAnyLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.ExcludeLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.FilterLabelsPanel = new System.Windows.Forms.Panel();
        this.RemoveAllNotButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.RemoveAllOrButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.RemoveAllAndButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.RemoveSelectedNotButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.RemoveSelectedOrButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.RemoveSelectedAndButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.OKButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.Cancel_Button = new AngelLoader.Forms.CustomControls.StandardButton();
        this.ResetButton = new AngelLoader.Forms.CustomControls.StandardButton();
        this.BottomFLP = new System.Windows.Forms.FlowLayoutPanel();
        this.MoveButtonsPanel = new System.Windows.Forms.Panel();
        this.MainToolTip = new System.Windows.Forms.ToolTip(this.components);
        this.FindTagTextBox = new AngelLoader.Forms.CustomControls.DarkTextBox();
        this.FilterLabelsPanel.SuspendLayout();
        this.BottomFLP.SuspendLayout();
        this.MoveButtonsPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // OriginTreeView
        // 
        this.OriginTreeView.AlwaysDrawNodesFocused = true;
        this.OriginTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.OriginTreeView.HideSelection = false;
        this.OriginTreeView.Location = new System.Drawing.Point(8, 32);
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
        this.AndTreeView.Size = new System.Drawing.Size(224, 632);
        this.AndTreeView.TabIndex = 3;
        // 
        // OrTreeView
        // 
        this.OrTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.OrTreeView.HideSelection = false;
        this.OrTreeView.Location = new System.Drawing.Point(576, 24);
        this.OrTreeView.Size = new System.Drawing.Size(224, 632);
        this.OrTreeView.TabIndex = 4;
        // 
        // NotTreeView
        // 
        this.NotTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.NotTreeView.HideSelection = false;
        this.NotTreeView.Location = new System.Drawing.Point(808, 24);
        this.NotTreeView.Size = new System.Drawing.Size(224, 632);
        this.NotTreeView.TabIndex = 5;
        // 
        // AndButton
        // 
        this.AndButton.AutoSize = true;
        this.AndButton.Location = new System.Drawing.Point(16, 0);
        this.AndButton.MinimumSize = new System.Drawing.Size(80, 23);
        this.AndButton.Padding = new System.Windows.Forms.Padding(7, 0, 0, 0);
        this.AndButton.TabIndex = 0;
        this.AndButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.AndButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.ArrowButtons_Paint);
        this.AndButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
        // 
        // OrButton
        // 
        this.OrButton.AutoSize = true;
        this.OrButton.Location = new System.Drawing.Point(16, 23);
        this.OrButton.MinimumSize = new System.Drawing.Size(80, 23);
        this.OrButton.Padding = new System.Windows.Forms.Padding(7, 0, 0, 0);
        this.OrButton.TabIndex = 1;
        this.OrButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.OrButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.ArrowButtons_Paint);
        this.OrButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
        // 
        // NotButton
        // 
        this.NotButton.AutoSize = true;
        this.NotButton.Location = new System.Drawing.Point(16, 46);
        this.NotButton.MinimumSize = new System.Drawing.Size(80, 23);
        this.NotButton.Padding = new System.Windows.Forms.Padding(7, 0, 0, 0);
        this.NotButton.TabIndex = 2;
        this.NotButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.NotButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.ArrowButtons_Paint);
        this.NotButton.Click += new System.EventHandler(this.AddTagsButtons_Click);
        // 
        // IncludeAllLabel
        // 
        this.IncludeAllLabel.AutoSize = true;
        this.IncludeAllLabel.Location = new System.Drawing.Point(0, 8);
        // 
        // IncludeAnyLabel
        // 
        this.IncludeAnyLabel.AutoSize = true;
        this.IncludeAnyLabel.Location = new System.Drawing.Point(232, 8);
        // 
        // ExcludeLabel
        // 
        this.ExcludeLabel.AutoSize = true;
        this.ExcludeLabel.Location = new System.Drawing.Point(464, 8);
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
        this.FilterLabelsPanel.Size = new System.Drawing.Size(688, 24);
        this.FilterLabelsPanel.TabIndex = 2;
        // 
        // RemoveAllNotButton
        // 
        this.RemoveAllNotButton.Location = new System.Drawing.Point(666, 0);
        this.RemoveAllNotButton.Size = new System.Drawing.Size(23, 23);
        this.RemoveAllNotButton.TabIndex = 8;
        this.RemoveAllNotButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveAllButtons_Paint);
        this.RemoveAllNotButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
        // 
        // RemoveAllOrButton
        // 
        this.RemoveAllOrButton.Location = new System.Drawing.Point(434, 0);
        this.RemoveAllOrButton.Size = new System.Drawing.Size(23, 23);
        this.RemoveAllOrButton.TabIndex = 5;
        this.RemoveAllOrButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveAllButtons_Paint);
        this.RemoveAllOrButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
        // 
        // RemoveAllAndButton
        // 
        this.RemoveAllAndButton.Location = new System.Drawing.Point(202, 0);
        this.RemoveAllAndButton.Size = new System.Drawing.Size(23, 23);
        this.RemoveAllAndButton.TabIndex = 2;
        this.RemoveAllAndButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveAllButtons_Paint);
        this.RemoveAllAndButton.Click += new System.EventHandler(this.RemoveAllButtons_Click);
        // 
        // RemoveSelectedNotButton
        // 
        this.RemoveSelectedNotButton.Location = new System.Drawing.Point(643, 0);
        this.RemoveSelectedNotButton.Size = new System.Drawing.Size(23, 23);
        this.RemoveSelectedNotButton.TabIndex = 7;
        this.RemoveSelectedNotButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveButtons_Paint);
        this.RemoveSelectedNotButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
        // 
        // RemoveSelectedOrButton
        // 
        this.RemoveSelectedOrButton.Location = new System.Drawing.Point(411, 0);
        this.RemoveSelectedOrButton.Size = new System.Drawing.Size(23, 23);
        this.RemoveSelectedOrButton.TabIndex = 4;
        this.RemoveSelectedOrButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveButtons_Paint);
        this.RemoveSelectedOrButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
        // 
        // RemoveSelectedAndButton
        // 
        this.RemoveSelectedAndButton.Location = new System.Drawing.Point(179, 0);
        this.RemoveSelectedAndButton.Size = new System.Drawing.Size(23, 23);
        this.RemoveSelectedAndButton.TabIndex = 1;
        this.RemoveSelectedAndButton.PaintCustom += new System.Windows.Forms.PaintEventHandler(this.RemoveButtons_Paint);
        this.RemoveSelectedAndButton.Click += new System.EventHandler(this.RemoveSelectedButtons_Click);
        // 
        // OKButton
        // 
        this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.OKButton.Margin = new System.Windows.Forms.Padding(13, 3, 3, 3);
        this.OKButton.TabIndex = 2;
        // 
        // Cancel_Button
        // 
        this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.Cancel_Button.Margin = new System.Windows.Forms.Padding(3, 3, 7, 3);
        this.Cancel_Button.TabIndex = 3;
        // 
        // ResetButton
        // 
        this.ResetButton.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
        this.ResetButton.TabIndex = 0;
        this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
        // 
        // BottomFLP
        // 
        this.BottomFLP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.BottomFLP.Controls.Add(this.Cancel_Button);
        this.BottomFLP.Controls.Add(this.OKButton);
        this.BottomFLP.Controls.Add(this.ResetButton);
        this.BottomFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        this.BottomFLP.Location = new System.Drawing.Point(0, 660);
        this.BottomFLP.Size = new System.Drawing.Size(1040, 32);
        this.BottomFLP.TabIndex = 6;
        this.BottomFLP.Paint += new System.Windows.Forms.PaintEventHandler(this.BottomFLP_Paint);
        // 
        // MoveButtonsPanel
        // 
        this.MoveButtonsPanel.Controls.Add(this.AndButton);
        this.MoveButtonsPanel.Controls.Add(this.OrButton);
        this.MoveButtonsPanel.Controls.Add(this.NotButton);
        this.MoveButtonsPanel.Location = new System.Drawing.Point(232, 104);
        this.MoveButtonsPanel.Size = new System.Drawing.Size(112, 552);
        this.MoveButtonsPanel.TabIndex = 1;
        // 
        // FindTagTextBox
        // 
        this.FindTagTextBox.Location = new System.Drawing.Point(8, 8);
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
        this.Controls.Add(this.BottomFLP);
        this.Controls.Add(this.FilterLabelsPanel);
        this.Controls.Add(this.NotTreeView);
        this.Controls.Add(this.OrTreeView);
        this.Controls.Add(this.AndTreeView);
        this.Controls.Add(this.OriginTreeView);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
        this.KeyPreview = true;
        this.MaximizeBox = false;
        this.MaximumSize = new System.Drawing.Size(1056, 32767);
        this.MinimizeBox = false;
        this.MinimumSize = new System.Drawing.Size(1056, 242);
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        // Hack to prevent slow first render on some forms if Text is blank
        this.Text = " ";
        this.FilterLabelsPanel.ResumeLayout(false);
        this.FilterLabelsPanel.PerformLayout();
        this.BottomFLP.ResumeLayout(false);
        this.BottomFLP.PerformLayout();
        this.MoveButtonsPanel.ResumeLayout(false);
        this.MoveButtonsPanel.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
