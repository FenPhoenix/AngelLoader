namespace AngelLoader.Forms.Import
{
    partial class ImportFromMultipleLoadersForm
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
            this.IniTabControl = new System.Windows.Forms.TabControl();
            this.DL_TabPage = new System.Windows.Forms.TabPage();
            this.FMSel_TabPage = new System.Windows.Forms.TabPage();
            this.NDL_TabPage = new System.Windows.Forms.TabPage();
            this.FieldSelectionTable = new System.Windows.Forms.TableLayoutPanel();
            this.FMDataToImportLabel = new System.Windows.Forms.Label();
            this.Prefer_DL_Label = new System.Windows.Forms.Label();
            this.Prefer_FMSel_Label = new System.Windows.Forms.Label();
            this.Prefer_NDL_Label = new System.Windows.Forms.Label();
            this.DL_Title_RadioButton = new System.Windows.Forms.RadioButton();
            this.DL_ReleaseDate_RadioButton = new System.Windows.Forms.RadioButton();
            this.DL_LastPlayed_RadioButton = new System.Windows.Forms.RadioButton();
            this.DL_Finished_RadioButton = new System.Windows.Forms.RadioButton();
            this.DL_Comment_RadioButton = new System.Windows.Forms.RadioButton();
            this.DL_Size_RadioButton = new System.Windows.Forms.RadioButton();
            this.FMSel_Title_RadioButton = new System.Windows.Forms.RadioButton();
            this.NDL_Title_RadioButton = new System.Windows.Forms.RadioButton();
            this.FMSel_ReleaseDate_RadioButton = new System.Windows.Forms.RadioButton();
            this.FMSel_LastPlayed_RadioButton = new System.Windows.Forms.RadioButton();
            this.FMSel_Finished_RadioButton = new System.Windows.Forms.RadioButton();
            this.FMSel_Comment_RadioButton = new System.Windows.Forms.RadioButton();
            this.FMSel_Rating_RadioButton = new System.Windows.Forms.RadioButton();
            this.FMSel_DisabledMods_RadioButton = new System.Windows.Forms.RadioButton();
            this.FMSel_Tags_RadioButton = new System.Windows.Forms.RadioButton();
            this.FMSel_SelectedReadme_RadioButton = new System.Windows.Forms.RadioButton();
            this.NDL_Size_RadioButton = new System.Windows.Forms.RadioButton();
            this.NDL_SelectedReadme_RadioButton = new System.Windows.Forms.RadioButton();
            this.NDL_Tags_RadioButton = new System.Windows.Forms.RadioButton();
            this.NDL_DisabledMods_RadioButton = new System.Windows.Forms.RadioButton();
            this.NDL_Comment_RadioButton = new System.Windows.Forms.RadioButton();
            this.NDL_Rating_RadioButton = new System.Windows.Forms.RadioButton();
            this.NDL_Finished_RadioButton = new System.Windows.Forms.RadioButton();
            this.NDL_LastPlayed_RadioButton = new System.Windows.Forms.RadioButton();
            this.NDL_ReleaseDate_RadioButton = new System.Windows.Forms.RadioButton();
            this.ImportTitleCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportReleaseDateCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportLastPlayedCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportFinishedOnCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportCommentCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportRatingCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportDisabledModsCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportTagsCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportSelectedReadmeCheckBox = new System.Windows.Forms.CheckBox();
            this.ImportSizeCheckBox = new System.Windows.Forms.CheckBox();
            this.OKCancelFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.DL_ImportControls = new AngelLoader.Forms.Import.User_DL_ImportControls();
            this.FMSel_ImportControls = new AngelLoader.Forms.Import.User_FMSel_NDL_ImportControls();
            this.NDL_ImportControls = new AngelLoader.Forms.Import.User_FMSel_NDL_ImportControls();
            this.IniTabControl.SuspendLayout();
            this.DL_TabPage.SuspendLayout();
            this.FMSel_TabPage.SuspendLayout();
            this.NDL_TabPage.SuspendLayout();
            this.FieldSelectionTable.SuspendLayout();
            this.OKCancelFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // IniTabControl
            // 
            this.IniTabControl.Controls.Add(this.DL_TabPage);
            this.IniTabControl.Controls.Add(this.FMSel_TabPage);
            this.IniTabControl.Controls.Add(this.NDL_TabPage);
            this.IniTabControl.Location = new System.Drawing.Point(8, 8);
            this.IniTabControl.Name = "IniTabControl";
            this.IniTabControl.SelectedIndex = 0;
            this.IniTabControl.Size = new System.Drawing.Size(576, 352);
            this.IniTabControl.TabIndex = 5;
            // 
            // DL_TabPage
            // 
            this.DL_TabPage.BackColor = System.Drawing.SystemColors.Control;
            this.DL_TabPage.Controls.Add(this.DL_ImportControls);
            this.DL_TabPage.Location = new System.Drawing.Point(4, 22);
            this.DL_TabPage.Name = "DL_TabPage";
            this.DL_TabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DL_TabPage.Size = new System.Drawing.Size(568, 326);
            this.DL_TabPage.TabIndex = 0;
            this.DL_TabPage.Text = "DarkLoader";
            // 
            // FMSel_TabPage
            // 
            this.FMSel_TabPage.BackColor = System.Drawing.SystemColors.Control;
            this.FMSel_TabPage.Controls.Add(this.FMSel_ImportControls);
            this.FMSel_TabPage.Location = new System.Drawing.Point(4, 22);
            this.FMSel_TabPage.Name = "FMSel_TabPage";
            this.FMSel_TabPage.Padding = new System.Windows.Forms.Padding(3);
            this.FMSel_TabPage.Size = new System.Drawing.Size(568, 326);
            this.FMSel_TabPage.TabIndex = 1;
            this.FMSel_TabPage.Text = "FMSel";
            // 
            // NDL_TabPage
            // 
            this.NDL_TabPage.BackColor = System.Drawing.SystemColors.Control;
            this.NDL_TabPage.Controls.Add(this.NDL_ImportControls);
            this.NDL_TabPage.Location = new System.Drawing.Point(4, 22);
            this.NDL_TabPage.Name = "NDL_TabPage";
            this.NDL_TabPage.Size = new System.Drawing.Size(568, 326);
            this.NDL_TabPage.TabIndex = 2;
            this.NDL_TabPage.Text = "NewDarkLoader";
            // 
            // FieldSelectionTable
            // 
            this.FieldSelectionTable.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.FieldSelectionTable.ColumnCount = 4;
            this.FieldSelectionTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 272F));
            this.FieldSelectionTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.FieldSelectionTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.FieldSelectionTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.FieldSelectionTable.Controls.Add(this.FMDataToImportLabel, 0, 0);
            this.FieldSelectionTable.Controls.Add(this.Prefer_DL_Label, 1, 0);
            this.FieldSelectionTable.Controls.Add(this.Prefer_FMSel_Label, 2, 0);
            this.FieldSelectionTable.Controls.Add(this.Prefer_NDL_Label, 3, 0);
            this.FieldSelectionTable.Controls.Add(this.DL_Title_RadioButton, 1, 1);
            this.FieldSelectionTable.Controls.Add(this.DL_ReleaseDate_RadioButton, 1, 2);
            this.FieldSelectionTable.Controls.Add(this.DL_LastPlayed_RadioButton, 1, 3);
            this.FieldSelectionTable.Controls.Add(this.DL_Finished_RadioButton, 1, 4);
            this.FieldSelectionTable.Controls.Add(this.DL_Comment_RadioButton, 1, 5);
            this.FieldSelectionTable.Controls.Add(this.DL_Size_RadioButton, 1, 10);
            this.FieldSelectionTable.Controls.Add(this.FMSel_Title_RadioButton, 2, 1);
            this.FieldSelectionTable.Controls.Add(this.NDL_Title_RadioButton, 3, 1);
            this.FieldSelectionTable.Controls.Add(this.FMSel_ReleaseDate_RadioButton, 2, 2);
            this.FieldSelectionTable.Controls.Add(this.FMSel_LastPlayed_RadioButton, 2, 3);
            this.FieldSelectionTable.Controls.Add(this.FMSel_Finished_RadioButton, 2, 4);
            this.FieldSelectionTable.Controls.Add(this.FMSel_Comment_RadioButton, 2, 5);
            this.FieldSelectionTable.Controls.Add(this.FMSel_Rating_RadioButton, 2, 6);
            this.FieldSelectionTable.Controls.Add(this.FMSel_DisabledMods_RadioButton, 2, 7);
            this.FieldSelectionTable.Controls.Add(this.FMSel_Tags_RadioButton, 2, 8);
            this.FieldSelectionTable.Controls.Add(this.FMSel_SelectedReadme_RadioButton, 2, 9);
            this.FieldSelectionTable.Controls.Add(this.NDL_Size_RadioButton, 3, 10);
            this.FieldSelectionTable.Controls.Add(this.NDL_SelectedReadme_RadioButton, 3, 9);
            this.FieldSelectionTable.Controls.Add(this.NDL_Tags_RadioButton, 3, 8);
            this.FieldSelectionTable.Controls.Add(this.NDL_DisabledMods_RadioButton, 3, 7);
            this.FieldSelectionTable.Controls.Add(this.NDL_Comment_RadioButton, 3, 5);
            this.FieldSelectionTable.Controls.Add(this.NDL_Rating_RadioButton, 3, 6);
            this.FieldSelectionTable.Controls.Add(this.NDL_Finished_RadioButton, 3, 4);
            this.FieldSelectionTable.Controls.Add(this.NDL_LastPlayed_RadioButton, 3, 3);
            this.FieldSelectionTable.Controls.Add(this.NDL_ReleaseDate_RadioButton, 3, 2);
            this.FieldSelectionTable.Controls.Add(this.ImportTitleCheckBox, 0, 1);
            this.FieldSelectionTable.Controls.Add(this.ImportReleaseDateCheckBox, 0, 2);
            this.FieldSelectionTable.Controls.Add(this.ImportLastPlayedCheckBox, 0, 3);
            this.FieldSelectionTable.Controls.Add(this.ImportFinishedOnCheckBox, 0, 4);
            this.FieldSelectionTable.Controls.Add(this.ImportCommentCheckBox, 0, 5);
            this.FieldSelectionTable.Controls.Add(this.ImportRatingCheckBox, 0, 6);
            this.FieldSelectionTable.Controls.Add(this.ImportDisabledModsCheckBox, 0, 7);
            this.FieldSelectionTable.Controls.Add(this.ImportTagsCheckBox, 0, 8);
            this.FieldSelectionTable.Controls.Add(this.ImportSelectedReadmeCheckBox, 0, 9);
            this.FieldSelectionTable.Controls.Add(this.ImportSizeCheckBox, 0, 10);
            this.FieldSelectionTable.Location = new System.Drawing.Point(8, 392);
            this.FieldSelectionTable.Name = "FieldSelectionTable";
            this.FieldSelectionTable.RowCount = 11;
            this.FieldSelectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.FieldSelectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.FieldSelectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.FieldSelectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.FieldSelectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.FieldSelectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.FieldSelectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.FieldSelectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.FieldSelectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.FieldSelectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.FieldSelectionTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.FieldSelectionTable.Size = new System.Drawing.Size(576, 272);
            this.FieldSelectionTable.TabIndex = 6;
            // 
            // FMDataToImportLabel
            // 
            this.FMDataToImportLabel.AutoSize = true;
            this.FMDataToImportLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FMDataToImportLabel.Location = new System.Drawing.Point(4, 1);
            this.FMDataToImportLabel.Name = "FMDataToImportLabel";
            this.FMDataToImportLabel.Size = new System.Drawing.Size(266, 20);
            this.FMDataToImportLabel.TabIndex = 0;
            this.FMDataToImportLabel.Text = "FM data to import";
            this.FMDataToImportLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Prefer_DL_Label
            // 
            this.Prefer_DL_Label.AutoSize = true;
            this.Prefer_DL_Label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Prefer_DL_Label.Location = new System.Drawing.Point(277, 1);
            this.Prefer_DL_Label.Name = "Prefer_DL_Label";
            this.Prefer_DL_Label.Size = new System.Drawing.Size(94, 20);
            this.Prefer_DL_Label.TabIndex = 0;
            this.Prefer_DL_Label.Text = "DarkLoader";
            this.Prefer_DL_Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Prefer_FMSel_Label
            // 
            this.Prefer_FMSel_Label.AutoSize = true;
            this.Prefer_FMSel_Label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Prefer_FMSel_Label.Location = new System.Drawing.Point(378, 1);
            this.Prefer_FMSel_Label.Name = "Prefer_FMSel_Label";
            this.Prefer_FMSel_Label.Size = new System.Drawing.Size(94, 20);
            this.Prefer_FMSel_Label.TabIndex = 0;
            this.Prefer_FMSel_Label.Text = "FMSel";
            this.Prefer_FMSel_Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Prefer_NDL_Label
            // 
            this.Prefer_NDL_Label.AutoSize = true;
            this.Prefer_NDL_Label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Prefer_NDL_Label.Location = new System.Drawing.Point(479, 1);
            this.Prefer_NDL_Label.Name = "Prefer_NDL_Label";
            this.Prefer_NDL_Label.Size = new System.Drawing.Size(94, 20);
            this.Prefer_NDL_Label.TabIndex = 0;
            this.Prefer_NDL_Label.Text = "NewDarkLoader";
            this.Prefer_NDL_Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DL_Title_RadioButton
            // 
            this.DL_Title_RadioButton.AutoSize = true;
            this.DL_Title_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DL_Title_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DL_Title_RadioButton.Enabled = false;
            this.DL_Title_RadioButton.Location = new System.Drawing.Point(277, 25);
            this.DL_Title_RadioButton.Name = "DL_Title_RadioButton";
            this.DL_Title_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.DL_Title_RadioButton.TabIndex = 1;
            this.DL_Title_RadioButton.UseVisualStyleBackColor = true;
            // 
            // DL_ReleaseDate_RadioButton
            // 
            this.DL_ReleaseDate_RadioButton.AutoSize = true;
            this.DL_ReleaseDate_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DL_ReleaseDate_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DL_ReleaseDate_RadioButton.Enabled = false;
            this.DL_ReleaseDate_RadioButton.Location = new System.Drawing.Point(277, 50);
            this.DL_ReleaseDate_RadioButton.Name = "DL_ReleaseDate_RadioButton";
            this.DL_ReleaseDate_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.DL_ReleaseDate_RadioButton.TabIndex = 1;
            this.DL_ReleaseDate_RadioButton.UseVisualStyleBackColor = true;
            // 
            // DL_LastPlayed_RadioButton
            // 
            this.DL_LastPlayed_RadioButton.AutoSize = true;
            this.DL_LastPlayed_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DL_LastPlayed_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DL_LastPlayed_RadioButton.Enabled = false;
            this.DL_LastPlayed_RadioButton.Location = new System.Drawing.Point(277, 75);
            this.DL_LastPlayed_RadioButton.Name = "DL_LastPlayed_RadioButton";
            this.DL_LastPlayed_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.DL_LastPlayed_RadioButton.TabIndex = 1;
            this.DL_LastPlayed_RadioButton.UseVisualStyleBackColor = true;
            // 
            // DL_Finished_RadioButton
            // 
            this.DL_Finished_RadioButton.AutoSize = true;
            this.DL_Finished_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DL_Finished_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DL_Finished_RadioButton.Enabled = false;
            this.DL_Finished_RadioButton.Location = new System.Drawing.Point(277, 100);
            this.DL_Finished_RadioButton.Name = "DL_Finished_RadioButton";
            this.DL_Finished_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.DL_Finished_RadioButton.TabIndex = 1;
            this.DL_Finished_RadioButton.UseVisualStyleBackColor = true;
            // 
            // DL_Comment_RadioButton
            // 
            this.DL_Comment_RadioButton.AutoSize = true;
            this.DL_Comment_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DL_Comment_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DL_Comment_RadioButton.Enabled = false;
            this.DL_Comment_RadioButton.Location = new System.Drawing.Point(277, 125);
            this.DL_Comment_RadioButton.Name = "DL_Comment_RadioButton";
            this.DL_Comment_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.DL_Comment_RadioButton.TabIndex = 1;
            this.DL_Comment_RadioButton.UseVisualStyleBackColor = true;
            // 
            // DL_Size_RadioButton
            // 
            this.DL_Size_RadioButton.AutoSize = true;
            this.DL_Size_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DL_Size_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DL_Size_RadioButton.Enabled = false;
            this.DL_Size_RadioButton.Location = new System.Drawing.Point(277, 250);
            this.DL_Size_RadioButton.Name = "DL_Size_RadioButton";
            this.DL_Size_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.DL_Size_RadioButton.TabIndex = 1;
            this.DL_Size_RadioButton.UseVisualStyleBackColor = true;
            // 
            // FMSel_Title_RadioButton
            // 
            this.FMSel_Title_RadioButton.AutoSize = true;
            this.FMSel_Title_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.FMSel_Title_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FMSel_Title_RadioButton.Enabled = false;
            this.FMSel_Title_RadioButton.Location = new System.Drawing.Point(378, 25);
            this.FMSel_Title_RadioButton.Name = "FMSel_Title_RadioButton";
            this.FMSel_Title_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.FMSel_Title_RadioButton.TabIndex = 1;
            this.FMSel_Title_RadioButton.UseVisualStyleBackColor = true;
            // 
            // NDL_Title_RadioButton
            // 
            this.NDL_Title_RadioButton.AutoSize = true;
            this.NDL_Title_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NDL_Title_RadioButton.Checked = true;
            this.NDL_Title_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NDL_Title_RadioButton.Enabled = false;
            this.NDL_Title_RadioButton.Location = new System.Drawing.Point(479, 25);
            this.NDL_Title_RadioButton.Name = "NDL_Title_RadioButton";
            this.NDL_Title_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.NDL_Title_RadioButton.TabIndex = 1;
            this.NDL_Title_RadioButton.TabStop = true;
            this.NDL_Title_RadioButton.UseVisualStyleBackColor = true;
            // 
            // FMSel_ReleaseDate_RadioButton
            // 
            this.FMSel_ReleaseDate_RadioButton.AutoSize = true;
            this.FMSel_ReleaseDate_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.FMSel_ReleaseDate_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FMSel_ReleaseDate_RadioButton.Enabled = false;
            this.FMSel_ReleaseDate_RadioButton.Location = new System.Drawing.Point(378, 50);
            this.FMSel_ReleaseDate_RadioButton.Name = "FMSel_ReleaseDate_RadioButton";
            this.FMSel_ReleaseDate_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.FMSel_ReleaseDate_RadioButton.TabIndex = 1;
            this.FMSel_ReleaseDate_RadioButton.UseVisualStyleBackColor = true;
            // 
            // FMSel_LastPlayed_RadioButton
            // 
            this.FMSel_LastPlayed_RadioButton.AutoSize = true;
            this.FMSel_LastPlayed_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.FMSel_LastPlayed_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FMSel_LastPlayed_RadioButton.Enabled = false;
            this.FMSel_LastPlayed_RadioButton.Location = new System.Drawing.Point(378, 75);
            this.FMSel_LastPlayed_RadioButton.Name = "FMSel_LastPlayed_RadioButton";
            this.FMSel_LastPlayed_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.FMSel_LastPlayed_RadioButton.TabIndex = 1;
            this.FMSel_LastPlayed_RadioButton.UseVisualStyleBackColor = true;
            // 
            // FMSel_Finished_RadioButton
            // 
            this.FMSel_Finished_RadioButton.AutoSize = true;
            this.FMSel_Finished_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.FMSel_Finished_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FMSel_Finished_RadioButton.Enabled = false;
            this.FMSel_Finished_RadioButton.Location = new System.Drawing.Point(378, 100);
            this.FMSel_Finished_RadioButton.Name = "FMSel_Finished_RadioButton";
            this.FMSel_Finished_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.FMSel_Finished_RadioButton.TabIndex = 1;
            this.FMSel_Finished_RadioButton.UseVisualStyleBackColor = true;
            // 
            // FMSel_Comment_RadioButton
            // 
            this.FMSel_Comment_RadioButton.AutoSize = true;
            this.FMSel_Comment_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.FMSel_Comment_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FMSel_Comment_RadioButton.Enabled = false;
            this.FMSel_Comment_RadioButton.Location = new System.Drawing.Point(378, 125);
            this.FMSel_Comment_RadioButton.Name = "FMSel_Comment_RadioButton";
            this.FMSel_Comment_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.FMSel_Comment_RadioButton.TabIndex = 1;
            this.FMSel_Comment_RadioButton.UseVisualStyleBackColor = true;
            // 
            // FMSel_Rating_RadioButton
            // 
            this.FMSel_Rating_RadioButton.AutoSize = true;
            this.FMSel_Rating_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.FMSel_Rating_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FMSel_Rating_RadioButton.Enabled = false;
            this.FMSel_Rating_RadioButton.Location = new System.Drawing.Point(378, 150);
            this.FMSel_Rating_RadioButton.Name = "FMSel_Rating_RadioButton";
            this.FMSel_Rating_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.FMSel_Rating_RadioButton.TabIndex = 1;
            this.FMSel_Rating_RadioButton.UseVisualStyleBackColor = true;
            // 
            // FMSel_DisabledMods_RadioButton
            // 
            this.FMSel_DisabledMods_RadioButton.AutoSize = true;
            this.FMSel_DisabledMods_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.FMSel_DisabledMods_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FMSel_DisabledMods_RadioButton.Enabled = false;
            this.FMSel_DisabledMods_RadioButton.Location = new System.Drawing.Point(378, 175);
            this.FMSel_DisabledMods_RadioButton.Name = "FMSel_DisabledMods_RadioButton";
            this.FMSel_DisabledMods_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.FMSel_DisabledMods_RadioButton.TabIndex = 1;
            this.FMSel_DisabledMods_RadioButton.UseVisualStyleBackColor = true;
            // 
            // FMSel_Tags_RadioButton
            // 
            this.FMSel_Tags_RadioButton.AutoSize = true;
            this.FMSel_Tags_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.FMSel_Tags_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FMSel_Tags_RadioButton.Enabled = false;
            this.FMSel_Tags_RadioButton.Location = new System.Drawing.Point(378, 200);
            this.FMSel_Tags_RadioButton.Name = "FMSel_Tags_RadioButton";
            this.FMSel_Tags_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.FMSel_Tags_RadioButton.TabIndex = 1;
            this.FMSel_Tags_RadioButton.UseVisualStyleBackColor = true;
            // 
            // FMSel_SelectedReadme_RadioButton
            // 
            this.FMSel_SelectedReadme_RadioButton.AutoSize = true;
            this.FMSel_SelectedReadme_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.FMSel_SelectedReadme_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FMSel_SelectedReadme_RadioButton.Enabled = false;
            this.FMSel_SelectedReadme_RadioButton.Location = new System.Drawing.Point(378, 225);
            this.FMSel_SelectedReadme_RadioButton.Name = "FMSel_SelectedReadme_RadioButton";
            this.FMSel_SelectedReadme_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.FMSel_SelectedReadme_RadioButton.TabIndex = 1;
            this.FMSel_SelectedReadme_RadioButton.UseVisualStyleBackColor = true;
            // 
            // NDL_Size_RadioButton
            // 
            this.NDL_Size_RadioButton.AutoSize = true;
            this.NDL_Size_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NDL_Size_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NDL_Size_RadioButton.Enabled = false;
            this.NDL_Size_RadioButton.Location = new System.Drawing.Point(479, 250);
            this.NDL_Size_RadioButton.Name = "NDL_Size_RadioButton";
            this.NDL_Size_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.NDL_Size_RadioButton.TabIndex = 1;
            this.NDL_Size_RadioButton.UseVisualStyleBackColor = true;
            // 
            // NDL_SelectedReadme_RadioButton
            // 
            this.NDL_SelectedReadme_RadioButton.AutoSize = true;
            this.NDL_SelectedReadme_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NDL_SelectedReadme_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NDL_SelectedReadme_RadioButton.Enabled = false;
            this.NDL_SelectedReadme_RadioButton.Location = new System.Drawing.Point(479, 225);
            this.NDL_SelectedReadme_RadioButton.Name = "NDL_SelectedReadme_RadioButton";
            this.NDL_SelectedReadme_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.NDL_SelectedReadme_RadioButton.TabIndex = 1;
            this.NDL_SelectedReadme_RadioButton.UseVisualStyleBackColor = true;
            // 
            // NDL_Tags_RadioButton
            // 
            this.NDL_Tags_RadioButton.AutoSize = true;
            this.NDL_Tags_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NDL_Tags_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NDL_Tags_RadioButton.Enabled = false;
            this.NDL_Tags_RadioButton.Location = new System.Drawing.Point(479, 200);
            this.NDL_Tags_RadioButton.Name = "NDL_Tags_RadioButton";
            this.NDL_Tags_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.NDL_Tags_RadioButton.TabIndex = 1;
            this.NDL_Tags_RadioButton.UseVisualStyleBackColor = true;
            // 
            // NDL_DisabledMods_RadioButton
            // 
            this.NDL_DisabledMods_RadioButton.AutoSize = true;
            this.NDL_DisabledMods_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NDL_DisabledMods_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NDL_DisabledMods_RadioButton.Enabled = false;
            this.NDL_DisabledMods_RadioButton.Location = new System.Drawing.Point(479, 175);
            this.NDL_DisabledMods_RadioButton.Name = "NDL_DisabledMods_RadioButton";
            this.NDL_DisabledMods_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.NDL_DisabledMods_RadioButton.TabIndex = 1;
            this.NDL_DisabledMods_RadioButton.UseVisualStyleBackColor = true;
            // 
            // NDL_Comment_RadioButton
            // 
            this.NDL_Comment_RadioButton.AutoSize = true;
            this.NDL_Comment_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NDL_Comment_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NDL_Comment_RadioButton.Enabled = false;
            this.NDL_Comment_RadioButton.Location = new System.Drawing.Point(479, 125);
            this.NDL_Comment_RadioButton.Name = "NDL_Comment_RadioButton";
            this.NDL_Comment_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.NDL_Comment_RadioButton.TabIndex = 1;
            this.NDL_Comment_RadioButton.UseVisualStyleBackColor = true;
            // 
            // NDL_Rating_RadioButton
            // 
            this.NDL_Rating_RadioButton.AutoSize = true;
            this.NDL_Rating_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NDL_Rating_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NDL_Rating_RadioButton.Enabled = false;
            this.NDL_Rating_RadioButton.Location = new System.Drawing.Point(479, 150);
            this.NDL_Rating_RadioButton.Name = "NDL_Rating_RadioButton";
            this.NDL_Rating_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.NDL_Rating_RadioButton.TabIndex = 1;
            this.NDL_Rating_RadioButton.UseVisualStyleBackColor = true;
            // 
            // NDL_Finished_RadioButton
            // 
            this.NDL_Finished_RadioButton.AutoSize = true;
            this.NDL_Finished_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NDL_Finished_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NDL_Finished_RadioButton.Enabled = false;
            this.NDL_Finished_RadioButton.Location = new System.Drawing.Point(479, 100);
            this.NDL_Finished_RadioButton.Name = "NDL_Finished_RadioButton";
            this.NDL_Finished_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.NDL_Finished_RadioButton.TabIndex = 1;
            this.NDL_Finished_RadioButton.UseVisualStyleBackColor = true;
            // 
            // NDL_LastPlayed_RadioButton
            // 
            this.NDL_LastPlayed_RadioButton.AutoSize = true;
            this.NDL_LastPlayed_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NDL_LastPlayed_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NDL_LastPlayed_RadioButton.Enabled = false;
            this.NDL_LastPlayed_RadioButton.Location = new System.Drawing.Point(479, 75);
            this.NDL_LastPlayed_RadioButton.Name = "NDL_LastPlayed_RadioButton";
            this.NDL_LastPlayed_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.NDL_LastPlayed_RadioButton.TabIndex = 1;
            this.NDL_LastPlayed_RadioButton.UseVisualStyleBackColor = true;
            // 
            // NDL_ReleaseDate_RadioButton
            // 
            this.NDL_ReleaseDate_RadioButton.AutoSize = true;
            this.NDL_ReleaseDate_RadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NDL_ReleaseDate_RadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NDL_ReleaseDate_RadioButton.Enabled = false;
            this.NDL_ReleaseDate_RadioButton.Location = new System.Drawing.Point(479, 50);
            this.NDL_ReleaseDate_RadioButton.Name = "NDL_ReleaseDate_RadioButton";
            this.NDL_ReleaseDate_RadioButton.Size = new System.Drawing.Size(94, 18);
            this.NDL_ReleaseDate_RadioButton.TabIndex = 1;
            this.NDL_ReleaseDate_RadioButton.UseVisualStyleBackColor = true;
            // 
            // ImportTitleCheckBox
            // 
            this.ImportTitleCheckBox.AutoSize = true;
            this.ImportTitleCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImportTitleCheckBox.Location = new System.Drawing.Point(4, 25);
            this.ImportTitleCheckBox.Name = "ImportTitleCheckBox";
            this.ImportTitleCheckBox.Size = new System.Drawing.Size(46, 18);
            this.ImportTitleCheckBox.TabIndex = 2;
            this.ImportTitleCheckBox.Text = "Title";
            this.ImportTitleCheckBox.UseVisualStyleBackColor = true;
            this.ImportTitleCheckBox.CheckedChanged += new System.EventHandler(this.ImportCheckBoxes_CheckedChanged);
            // 
            // ImportReleaseDateCheckBox
            // 
            this.ImportReleaseDateCheckBox.AutoSize = true;
            this.ImportReleaseDateCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImportReleaseDateCheckBox.Location = new System.Drawing.Point(4, 50);
            this.ImportReleaseDateCheckBox.Name = "ImportReleaseDateCheckBox";
            this.ImportReleaseDateCheckBox.Size = new System.Drawing.Size(89, 18);
            this.ImportReleaseDateCheckBox.TabIndex = 2;
            this.ImportReleaseDateCheckBox.Text = "Release date";
            this.ImportReleaseDateCheckBox.UseVisualStyleBackColor = true;
            this.ImportReleaseDateCheckBox.CheckedChanged += new System.EventHandler(this.ImportCheckBoxes_CheckedChanged);
            // 
            // ImportLastPlayedCheckBox
            // 
            this.ImportLastPlayedCheckBox.AutoSize = true;
            this.ImportLastPlayedCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImportLastPlayedCheckBox.Location = new System.Drawing.Point(4, 75);
            this.ImportLastPlayedCheckBox.Name = "ImportLastPlayedCheckBox";
            this.ImportLastPlayedCheckBox.Size = new System.Drawing.Size(80, 18);
            this.ImportLastPlayedCheckBox.TabIndex = 2;
            this.ImportLastPlayedCheckBox.Text = "Last played";
            this.ImportLastPlayedCheckBox.UseVisualStyleBackColor = true;
            this.ImportLastPlayedCheckBox.CheckedChanged += new System.EventHandler(this.ImportCheckBoxes_CheckedChanged);
            // 
            // ImportFinishedOnCheckBox
            // 
            this.ImportFinishedOnCheckBox.AutoSize = true;
            this.ImportFinishedOnCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImportFinishedOnCheckBox.Location = new System.Drawing.Point(4, 100);
            this.ImportFinishedOnCheckBox.Name = "ImportFinishedOnCheckBox";
            this.ImportFinishedOnCheckBox.Size = new System.Drawing.Size(65, 18);
            this.ImportFinishedOnCheckBox.TabIndex = 2;
            this.ImportFinishedOnCheckBox.Text = "Finished";
            this.ImportFinishedOnCheckBox.UseVisualStyleBackColor = true;
            this.ImportFinishedOnCheckBox.CheckedChanged += new System.EventHandler(this.ImportCheckBoxes_CheckedChanged);
            // 
            // ImportCommentCheckBox
            // 
            this.ImportCommentCheckBox.AutoSize = true;
            this.ImportCommentCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImportCommentCheckBox.Location = new System.Drawing.Point(4, 125);
            this.ImportCommentCheckBox.Name = "ImportCommentCheckBox";
            this.ImportCommentCheckBox.Size = new System.Drawing.Size(70, 18);
            this.ImportCommentCheckBox.TabIndex = 2;
            this.ImportCommentCheckBox.Text = "Comment";
            this.ImportCommentCheckBox.UseVisualStyleBackColor = true;
            this.ImportCommentCheckBox.CheckedChanged += new System.EventHandler(this.ImportCheckBoxes_CheckedChanged);
            // 
            // ImportRatingCheckBox
            // 
            this.ImportRatingCheckBox.AutoSize = true;
            this.ImportRatingCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImportRatingCheckBox.Location = new System.Drawing.Point(4, 150);
            this.ImportRatingCheckBox.Name = "ImportRatingCheckBox";
            this.ImportRatingCheckBox.Size = new System.Drawing.Size(57, 18);
            this.ImportRatingCheckBox.TabIndex = 2;
            this.ImportRatingCheckBox.Text = "Rating";
            this.ImportRatingCheckBox.UseVisualStyleBackColor = true;
            this.ImportRatingCheckBox.CheckedChanged += new System.EventHandler(this.ImportCheckBoxes_CheckedChanged);
            // 
            // ImportDisabledModsCheckBox
            // 
            this.ImportDisabledModsCheckBox.AutoSize = true;
            this.ImportDisabledModsCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImportDisabledModsCheckBox.Location = new System.Drawing.Point(4, 175);
            this.ImportDisabledModsCheckBox.Name = "ImportDisabledModsCheckBox";
            this.ImportDisabledModsCheckBox.Size = new System.Drawing.Size(95, 18);
            this.ImportDisabledModsCheckBox.TabIndex = 2;
            this.ImportDisabledModsCheckBox.Text = "Disabled mods";
            this.ImportDisabledModsCheckBox.UseVisualStyleBackColor = true;
            this.ImportDisabledModsCheckBox.CheckedChanged += new System.EventHandler(this.ImportCheckBoxes_CheckedChanged);
            // 
            // ImportTagsCheckBox
            // 
            this.ImportTagsCheckBox.AutoSize = true;
            this.ImportTagsCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImportTagsCheckBox.Location = new System.Drawing.Point(4, 200);
            this.ImportTagsCheckBox.Name = "ImportTagsCheckBox";
            this.ImportTagsCheckBox.Size = new System.Drawing.Size(50, 18);
            this.ImportTagsCheckBox.TabIndex = 2;
            this.ImportTagsCheckBox.Text = "Tags";
            this.ImportTagsCheckBox.UseVisualStyleBackColor = true;
            this.ImportTagsCheckBox.CheckedChanged += new System.EventHandler(this.ImportCheckBoxes_CheckedChanged);
            // 
            // ImportSelectedReadmeCheckBox
            // 
            this.ImportSelectedReadmeCheckBox.AutoSize = true;
            this.ImportSelectedReadmeCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImportSelectedReadmeCheckBox.Location = new System.Drawing.Point(4, 225);
            this.ImportSelectedReadmeCheckBox.Name = "ImportSelectedReadmeCheckBox";
            this.ImportSelectedReadmeCheckBox.Size = new System.Drawing.Size(106, 18);
            this.ImportSelectedReadmeCheckBox.TabIndex = 2;
            this.ImportSelectedReadmeCheckBox.Text = "Selected readme";
            this.ImportSelectedReadmeCheckBox.UseVisualStyleBackColor = true;
            this.ImportSelectedReadmeCheckBox.CheckedChanged += new System.EventHandler(this.ImportCheckBoxes_CheckedChanged);
            // 
            // ImportSizeCheckBox
            // 
            this.ImportSizeCheckBox.AutoSize = true;
            this.ImportSizeCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImportSizeCheckBox.Location = new System.Drawing.Point(4, 250);
            this.ImportSizeCheckBox.Name = "ImportSizeCheckBox";
            this.ImportSizeCheckBox.Size = new System.Drawing.Size(46, 18);
            this.ImportSizeCheckBox.TabIndex = 2;
            this.ImportSizeCheckBox.Text = "Size";
            this.ImportSizeCheckBox.UseVisualStyleBackColor = true;
            this.ImportSizeCheckBox.CheckedChanged += new System.EventHandler(this.ImportCheckBoxes_CheckedChanged);
            // 
            // OKCancelFlowLayoutPanel
            // 
            this.OKCancelFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKCancelFlowLayoutPanel.AutoSize = true;
            this.OKCancelFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OKCancelFlowLayoutPanel.Controls.Add(this.Cancel_Button);
            this.OKCancelFlowLayoutPanel.Controls.Add(this.OKButton);
            this.OKCancelFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.OKCancelFlowLayoutPanel.Location = new System.Drawing.Point(423, 717);
            this.OKCancelFlowLayoutPanel.Name = "OKCancelFlowLayoutPanel";
            this.OKCancelFlowLayoutPanel.Size = new System.Drawing.Size(162, 29);
            this.OKCancelFlowLayoutPanel.TabIndex = 10;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.AutoSize = true;
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(84, 3);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 0;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.AutoSize = true;
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(3, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // DL_ImportControls
            // 
            this.DL_ImportControls.Location = new System.Drawing.Point(8, 8);
            this.DL_ImportControls.Name = "DL_ImportControls";
            this.DL_ImportControls.Size = new System.Drawing.Size(540, 96);
            this.DL_ImportControls.TabIndex = 0;
            // 
            // FMSel_ImportControls
            // 
            this.FMSel_ImportControls.Location = new System.Drawing.Point(8, 8);
            this.FMSel_ImportControls.Name = "FMSel_ImportControls";
            this.FMSel_ImportControls.Size = new System.Drawing.Size(551, 312);
            this.FMSel_ImportControls.TabIndex = 1;
            // 
            // NDL_ImportControls
            // 
            this.NDL_ImportControls.Location = new System.Drawing.Point(8, 8);
            this.NDL_ImportControls.Name = "NDL_ImportControls";
            this.NDL_ImportControls.Size = new System.Drawing.Size(551, 312);
            this.NDL_ImportControls.TabIndex = 1;
            // 
            // ImportFromMultipleLoadersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 752);
            this.Controls.Add(this.OKCancelFlowLayoutPanel);
            this.Controls.Add(this.FieldSelectionTable);
            this.Controls.Add(this.IniTabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportFromMultipleLoadersForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import from multiple loaders";
            this.IniTabControl.ResumeLayout(false);
            this.DL_TabPage.ResumeLayout(false);
            this.FMSel_TabPage.ResumeLayout(false);
            this.NDL_TabPage.ResumeLayout(false);
            this.FieldSelectionTable.ResumeLayout(false);
            this.FieldSelectionTable.PerformLayout();
            this.OKCancelFlowLayoutPanel.ResumeLayout(false);
            this.OKCancelFlowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private User_DL_ImportControls DL_ImportControls;
        private User_FMSel_NDL_ImportControls FMSel_ImportControls;
        private User_FMSel_NDL_ImportControls NDL_ImportControls;
        private System.Windows.Forms.TabControl IniTabControl;
        private System.Windows.Forms.TabPage DL_TabPage;
        private System.Windows.Forms.TabPage FMSel_TabPage;
        private System.Windows.Forms.TabPage NDL_TabPage;
        private System.Windows.Forms.TableLayoutPanel FieldSelectionTable;
        private System.Windows.Forms.Label FMDataToImportLabel;
        private System.Windows.Forms.Label Prefer_DL_Label;
        private System.Windows.Forms.Label Prefer_FMSel_Label;
        private System.Windows.Forms.Label Prefer_NDL_Label;
        private System.Windows.Forms.RadioButton DL_Title_RadioButton;
        private System.Windows.Forms.RadioButton DL_ReleaseDate_RadioButton;
        private System.Windows.Forms.RadioButton DL_LastPlayed_RadioButton;
        private System.Windows.Forms.RadioButton DL_Finished_RadioButton;
        private System.Windows.Forms.RadioButton DL_Comment_RadioButton;
        private System.Windows.Forms.RadioButton DL_Size_RadioButton;
        private System.Windows.Forms.RadioButton FMSel_Title_RadioButton;
        private System.Windows.Forms.RadioButton NDL_Title_RadioButton;
        private System.Windows.Forms.RadioButton FMSel_ReleaseDate_RadioButton;
        private System.Windows.Forms.RadioButton FMSel_LastPlayed_RadioButton;
        private System.Windows.Forms.RadioButton FMSel_Finished_RadioButton;
        private System.Windows.Forms.RadioButton FMSel_Comment_RadioButton;
        private System.Windows.Forms.RadioButton FMSel_Rating_RadioButton;
        private System.Windows.Forms.RadioButton FMSel_DisabledMods_RadioButton;
        private System.Windows.Forms.RadioButton FMSel_Tags_RadioButton;
        private System.Windows.Forms.RadioButton FMSel_SelectedReadme_RadioButton;
        private System.Windows.Forms.RadioButton NDL_Size_RadioButton;
        private System.Windows.Forms.RadioButton NDL_SelectedReadme_RadioButton;
        private System.Windows.Forms.RadioButton NDL_Tags_RadioButton;
        private System.Windows.Forms.RadioButton NDL_DisabledMods_RadioButton;
        private System.Windows.Forms.RadioButton NDL_Comment_RadioButton;
        private System.Windows.Forms.RadioButton NDL_Rating_RadioButton;
        private System.Windows.Forms.RadioButton NDL_Finished_RadioButton;
        private System.Windows.Forms.RadioButton NDL_LastPlayed_RadioButton;
        private System.Windows.Forms.RadioButton NDL_ReleaseDate_RadioButton;
        private System.Windows.Forms.FlowLayoutPanel OKCancelFlowLayoutPanel;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.CheckBox ImportTitleCheckBox;
        private System.Windows.Forms.CheckBox ImportReleaseDateCheckBox;
        private System.Windows.Forms.CheckBox ImportLastPlayedCheckBox;
        private System.Windows.Forms.CheckBox ImportFinishedOnCheckBox;
        private System.Windows.Forms.CheckBox ImportCommentCheckBox;
        private System.Windows.Forms.CheckBox ImportRatingCheckBox;
        private System.Windows.Forms.CheckBox ImportDisabledModsCheckBox;
        private System.Windows.Forms.CheckBox ImportTagsCheckBox;
        private System.Windows.Forms.CheckBox ImportSelectedReadmeCheckBox;
        private System.Windows.Forms.CheckBox ImportSizeCheckBox;
    }
}