namespace Microarea.EasyAttachment.UI.Forms
{
	partial class BBTrayletForm
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
			if(disposing && (components != null))
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BBTrayletForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.FormsDGV = new System.Windows.Forms.DataGridView();
            this.DateReceivedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TitleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DescriptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RequesterColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DateProcessedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProcessedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.FormInstanceIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AttachmentIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TopComboBox = new System.Windows.Forms.ComboBox();
            this.RefreshBtnTop = new System.Windows.Forms.Button();
            this.ShowFormBtn = new System.Windows.Forms.Button();
            this.ShowDocumentBtnTop = new System.Windows.Forms.Button();
            this.MyRequestsDGV = new System.Windows.Forms.DataGridView();
            this.LoadingBoxTop = new System.Windows.Forms.PictureBox();
            this.LoadingBoxBottom = new System.Windows.Forms.PictureBox();
            this.RefreshBtnBottom = new System.Windows.Forms.Button();
            this.ShowDocumentBtnBottom = new System.Windows.Forms.Button();
            this.MasterSplitContainer = new System.Windows.Forms.SplitContainer();
            this.TopPanel1 = new System.Windows.Forms.Panel();
            this.TopFilterLbl = new System.Windows.Forms.Label();
            this.Toplbl = new System.Windows.Forms.Label();
            this.DelFormBtn = new System.Windows.Forms.Button();
            this.TopDGVFilterTxt = new System.Windows.Forms.TextBox();
            this.BottomPanel1 = new System.Windows.Forms.Panel();
            this.TopPanel2 = new System.Windows.Forms.Panel();
            this.BottomComboBox = new System.Windows.Forms.ComboBox();
            this.BottomLbl = new System.Windows.Forms.Label();
            this.BottomDGVFilterTxt = new System.Windows.Forms.TextBox();
            this.BottomFilterLbl = new System.Windows.Forms.Label();
            this.BottomPanel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.FormsDGV)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MyRequestsDGV)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingBoxTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingBoxBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MasterSplitContainer)).BeginInit();
            this.MasterSplitContainer.Panel1.SuspendLayout();
            this.MasterSplitContainer.Panel2.SuspendLayout();
            this.MasterSplitContainer.SuspendLayout();
            this.TopPanel1.SuspendLayout();
            this.BottomPanel1.SuspendLayout();
            this.TopPanel2.SuspendLayout();
            this.BottomPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // FormsDGV
            // 
            this.FormsDGV.AllowUserToAddRows = false;
            this.FormsDGV.AllowUserToDeleteRows = false;
            this.FormsDGV.AllowUserToResizeRows = false;
            this.FormsDGV.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.FormsDGV.BackgroundColor = System.Drawing.Color.White;
            this.FormsDGV.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FormsDGV.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Sunken;
            this.FormsDGV.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Sunken;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.FormsDGV.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.FormsDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.FormsDGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DateReceivedColumn,
            this.TitleColumn,
            this.DescriptionColumn,
            this.RequesterColumn,
            this.DateProcessedColumn,
            this.ProcessedColumn,
            this.FormInstanceIdColumn,
            this.AttachmentIdColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Red;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.FormsDGV.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.FormsDGV, "FormsDGV");
            this.FormsDGV.EnableHeadersVisualStyles = false;
            this.FormsDGV.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            this.FormsDGV.Name = "FormsDGV";
            this.FormsDGV.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.FormsDGV.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.FormsDGV.RowHeadersVisible = false;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.FormsDGV.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.FormsDGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.FormsDGV.StandardTab = true;
            // 
            // DateReceivedColumn
            // 
            resources.ApplyResources(this.DateReceivedColumn, "DateReceivedColumn");
            this.DateReceivedColumn.Name = "DateReceivedColumn";
            this.DateReceivedColumn.ReadOnly = true;
            this.DateReceivedColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // TitleColumn
            // 
            resources.ApplyResources(this.TitleColumn, "TitleColumn");
            this.TitleColumn.Name = "TitleColumn";
            this.TitleColumn.ReadOnly = true;
            // 
            // DescriptionColumn
            // 
            resources.ApplyResources(this.DescriptionColumn, "DescriptionColumn");
            this.DescriptionColumn.Name = "DescriptionColumn";
            this.DescriptionColumn.ReadOnly = true;
            // 
            // RequesterColumn
            // 
            resources.ApplyResources(this.RequesterColumn, "RequesterColumn");
            this.RequesterColumn.Name = "RequesterColumn";
            this.RequesterColumn.ReadOnly = true;
            // 
            // DateProcessedColumn
            // 
            resources.ApplyResources(this.DateProcessedColumn, "DateProcessedColumn");
            this.DateProcessedColumn.Name = "DateProcessedColumn";
            this.DateProcessedColumn.ReadOnly = true;
            // 
            // ProcessedColumn
            // 
            resources.ApplyResources(this.ProcessedColumn, "ProcessedColumn");
            this.ProcessedColumn.Name = "ProcessedColumn";
            this.ProcessedColumn.ReadOnly = true;
            this.ProcessedColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ProcessedColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // FormInstanceIdColumn
            // 
            resources.ApplyResources(this.FormInstanceIdColumn, "FormInstanceIdColumn");
            this.FormInstanceIdColumn.Name = "FormInstanceIdColumn";
            this.FormInstanceIdColumn.ReadOnly = true;
            // 
            // AttachmentIdColumn
            // 
            resources.ApplyResources(this.AttachmentIdColumn, "AttachmentIdColumn");
            this.AttachmentIdColumn.Name = "AttachmentIdColumn";
            this.AttachmentIdColumn.ReadOnly = true;
            // 
            // TopComboBox
            // 
            resources.ApplyResources(this.TopComboBox, "TopComboBox");
            this.TopComboBox.FormattingEnabled = true;
            this.TopComboBox.Name = "TopComboBox";
            this.TopComboBox.SelectedIndexChanged += new System.EventHandler(this.TopComboBox_SelectedIndexChanged);
            // 
            // RefreshBtnTop
            // 
            resources.ApplyResources(this.RefreshBtnTop, "RefreshBtnTop");
            this.RefreshBtnTop.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.RefreshBtnTop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue;
            this.RefreshBtnTop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.RoyalBlue;
            this.RefreshBtnTop.Image = global::Microarea.EasyAttachment.Properties.Resources.Refresh;
            this.RefreshBtnTop.Name = "RefreshBtnTop";
            this.RefreshBtnTop.UseVisualStyleBackColor = true;
            this.RefreshBtnTop.Click += new System.EventHandler(this.RefreshBtnTop_Click);
            // 
            // ShowFormBtn
            // 
            resources.ApplyResources(this.ShowFormBtn, "ShowFormBtn");
            this.ShowFormBtn.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.ShowFormBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue;
            this.ShowFormBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.RoyalBlue;
            this.ShowFormBtn.Image = global::Microarea.EasyAttachment.Properties.Resources.Form;
            this.ShowFormBtn.Name = "ShowFormBtn";
            this.ShowFormBtn.UseVisualStyleBackColor = true;
            this.ShowFormBtn.Click += new System.EventHandler(this.ShowFormBtn_Click);
            // 
            // ShowDocumentBtnTop
            // 
            resources.ApplyResources(this.ShowDocumentBtnTop, "ShowDocumentBtnTop");
            this.ShowDocumentBtnTop.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.ShowDocumentBtnTop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue;
            this.ShowDocumentBtnTop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.RoyalBlue;
            this.ShowDocumentBtnTop.Image = global::Microarea.EasyAttachment.Properties.Resources.Document1;
            this.ShowDocumentBtnTop.Name = "ShowDocumentBtnTop";
            this.ShowDocumentBtnTop.UseVisualStyleBackColor = true;
            this.ShowDocumentBtnTop.Click += new System.EventHandler(this.ShowDocumentBtnTop_Click);
            // 
            // MyRequestsDGV
            // 
            this.MyRequestsDGV.AllowUserToAddRows = false;
            this.MyRequestsDGV.AllowUserToDeleteRows = false;
            this.MyRequestsDGV.AllowUserToResizeRows = false;
            this.MyRequestsDGV.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.MyRequestsDGV.BackgroundColor = System.Drawing.Color.White;
            this.MyRequestsDGV.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MyRequestsDGV.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Sunken;
            this.MyRequestsDGV.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Sunken;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.MyRequestsDGV.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.MyRequestsDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.MyRequestsDGV.DefaultCellStyle = dataGridViewCellStyle6;
            resources.ApplyResources(this.MyRequestsDGV, "MyRequestsDGV");
            this.MyRequestsDGV.EnableHeadersVisualStyles = false;
            this.MyRequestsDGV.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            this.MyRequestsDGV.MultiSelect = false;
            this.MyRequestsDGV.Name = "MyRequestsDGV";
            this.MyRequestsDGV.ReadOnly = true;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.MyRequestsDGV.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.MyRequestsDGV.RowHeadersVisible = false;
            this.MyRequestsDGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.MyRequestsDGV.StandardTab = true;
            // 
            // LoadingBoxTop
            // 
            this.LoadingBoxTop.Image = global::Microarea.EasyAttachment.Properties.Resources.loading;
            resources.ApplyResources(this.LoadingBoxTop, "LoadingBoxTop");
            this.LoadingBoxTop.Name = "LoadingBoxTop";
            this.LoadingBoxTop.TabStop = false;
            // 
            // LoadingBoxBottom
            // 
            this.LoadingBoxBottom.Image = global::Microarea.EasyAttachment.Properties.Resources.loading;
            resources.ApplyResources(this.LoadingBoxBottom, "LoadingBoxBottom");
            this.LoadingBoxBottom.Name = "LoadingBoxBottom";
            this.LoadingBoxBottom.TabStop = false;
            // 
            // RefreshBtnBottom
            // 
            resources.ApplyResources(this.RefreshBtnBottom, "RefreshBtnBottom");
            this.RefreshBtnBottom.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.RefreshBtnBottom.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue;
            this.RefreshBtnBottom.FlatAppearance.MouseOverBackColor = System.Drawing.Color.RoyalBlue;
            this.RefreshBtnBottom.Image = global::Microarea.EasyAttachment.Properties.Resources.Refresh;
            this.RefreshBtnBottom.Name = "RefreshBtnBottom";
            this.RefreshBtnBottom.UseVisualStyleBackColor = true;
            this.RefreshBtnBottom.Click += new System.EventHandler(this.RefreshBtnBottom_Click);
            // 
            // ShowDocumentBtnBottom
            // 
            resources.ApplyResources(this.ShowDocumentBtnBottom, "ShowDocumentBtnBottom");
            this.ShowDocumentBtnBottom.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.ShowDocumentBtnBottom.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue;
            this.ShowDocumentBtnBottom.FlatAppearance.MouseOverBackColor = System.Drawing.Color.RoyalBlue;
            this.ShowDocumentBtnBottom.Image = global::Microarea.EasyAttachment.Properties.Resources.Document1;
            this.ShowDocumentBtnBottom.Name = "ShowDocumentBtnBottom";
            this.ShowDocumentBtnBottom.UseVisualStyleBackColor = true;
            this.ShowDocumentBtnBottom.Click += new System.EventHandler(this.ShowDocumentBtnBottom_Click);
            // 
            // MasterSplitContainer
            // 
            this.MasterSplitContainer.AllowDrop = true;
            this.MasterSplitContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            resources.ApplyResources(this.MasterSplitContainer, "MasterSplitContainer");
            this.MasterSplitContainer.Name = "MasterSplitContainer";
            // 
            // MasterSplitContainer.Panel1
            // 
            this.MasterSplitContainer.Panel1.BackColor = System.Drawing.Color.White;
            this.MasterSplitContainer.Panel1.Controls.Add(this.TopPanel1);
            this.MasterSplitContainer.Panel1.Controls.Add(this.BottomPanel1);
            // 
            // MasterSplitContainer.Panel2
            // 
            this.MasterSplitContainer.Panel2.BackColor = System.Drawing.Color.White;
            this.MasterSplitContainer.Panel2.Controls.Add(this.TopPanel2);
            this.MasterSplitContainer.Panel2.Controls.Add(this.BottomPanel2);
            // 
            // TopPanel1
            // 
            this.TopPanel1.Controls.Add(this.TopFilterLbl);
            this.TopPanel1.Controls.Add(this.Toplbl);
            this.TopPanel1.Controls.Add(this.DelFormBtn);
            this.TopPanel1.Controls.Add(this.ShowFormBtn);
            this.TopPanel1.Controls.Add(this.TopComboBox);
            this.TopPanel1.Controls.Add(this.RefreshBtnTop);
            this.TopPanel1.Controls.Add(this.ShowDocumentBtnTop);
            this.TopPanel1.Controls.Add(this.TopDGVFilterTxt);
            resources.ApplyResources(this.TopPanel1, "TopPanel1");
            this.TopPanel1.Name = "TopPanel1";
            // 
            // TopFilterLbl
            // 
            resources.ApplyResources(this.TopFilterLbl, "TopFilterLbl");
            this.TopFilterLbl.Name = "TopFilterLbl";
            // 
            // Toplbl
            // 
            resources.ApplyResources(this.Toplbl, "Toplbl");
            this.Toplbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            this.Toplbl.Name = "Toplbl";
            // 
            // DelFormBtn
            // 
            resources.ApplyResources(this.DelFormBtn, "DelFormBtn");
            this.DelFormBtn.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.DelFormBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.RoyalBlue;
            this.DelFormBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.RoyalBlue;
            this.DelFormBtn.Image = global::Microarea.EasyAttachment.Properties.Resources.Delete;
            this.DelFormBtn.Name = "DelFormBtn";
            this.DelFormBtn.UseVisualStyleBackColor = true;
            this.DelFormBtn.Click += new System.EventHandler(this.DelFormBtn_Click);
            // 
            // TopDGVFilterTxt
            // 
            resources.ApplyResources(this.TopDGVFilterTxt, "TopDGVFilterTxt");
            this.TopDGVFilterTxt.Name = "TopDGVFilterTxt";
            this.TopDGVFilterTxt.TextChanged += new System.EventHandler(this.TopDGVFilterTxt_TextChanged);
            // 
            // BottomPanel1
            // 
            resources.ApplyResources(this.BottomPanel1, "BottomPanel1");
            this.BottomPanel1.Controls.Add(this.LoadingBoxTop);
            this.BottomPanel1.Controls.Add(this.FormsDGV);
            this.BottomPanel1.Name = "BottomPanel1";
            // 
            // TopPanel2
            // 
            this.TopPanel2.Controls.Add(this.ShowDocumentBtnBottom);
            this.TopPanel2.Controls.Add(this.RefreshBtnBottom);
            this.TopPanel2.Controls.Add(this.BottomComboBox);
            this.TopPanel2.Controls.Add(this.BottomLbl);
            this.TopPanel2.Controls.Add(this.BottomDGVFilterTxt);
            this.TopPanel2.Controls.Add(this.BottomFilterLbl);
            resources.ApplyResources(this.TopPanel2, "TopPanel2");
            this.TopPanel2.Name = "TopPanel2";
            // 
            // BottomComboBox
            // 
            resources.ApplyResources(this.BottomComboBox, "BottomComboBox");
            this.BottomComboBox.FormattingEnabled = true;
            this.BottomComboBox.Name = "BottomComboBox";
            this.BottomComboBox.SelectedIndexChanged += new System.EventHandler(this.BottomComboBox_SelectedIndexChanged);
            // 
            // BottomLbl
            // 
            resources.ApplyResources(this.BottomLbl, "BottomLbl");
            this.BottomLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(120)))), ((int)(((byte)(228)))));
            this.BottomLbl.Name = "BottomLbl";
            // 
            // BottomDGVFilterTxt
            // 
            resources.ApplyResources(this.BottomDGVFilterTxt, "BottomDGVFilterTxt");
            this.BottomDGVFilterTxt.Name = "BottomDGVFilterTxt";
            this.BottomDGVFilterTxt.TextChanged += new System.EventHandler(this.BottomDGVFilterTxt_TextChanged);
            // 
            // BottomFilterLbl
            // 
            resources.ApplyResources(this.BottomFilterLbl, "BottomFilterLbl");
            this.BottomFilterLbl.Name = "BottomFilterLbl";
            // 
            // BottomPanel2
            // 
            resources.ApplyResources(this.BottomPanel2, "BottomPanel2");
            this.BottomPanel2.Controls.Add(this.LoadingBoxBottom);
            this.BottomPanel2.Controls.Add(this.MyRequestsDGV);
            this.BottomPanel2.Name = "BottomPanel2";
            // 
            // BBTrayletForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.Controls.Add(this.MasterSplitContainer);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BBTrayletForm";
            ((System.ComponentModel.ISupportInitialize)(this.FormsDGV)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MyRequestsDGV)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingBoxTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingBoxBottom)).EndInit();
            this.MasterSplitContainer.Panel1.ResumeLayout(false);
            this.MasterSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MasterSplitContainer)).EndInit();
            this.MasterSplitContainer.ResumeLayout(false);
            this.TopPanel1.ResumeLayout(false);
            this.TopPanel1.PerformLayout();
            this.BottomPanel1.ResumeLayout(false);
            this.TopPanel2.ResumeLayout(false);
            this.TopPanel2.PerformLayout();
            this.BottomPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView FormsDGV;
		private System.Windows.Forms.ComboBox TopComboBox;
		private System.Windows.Forms.Button RefreshBtnTop;
		private System.Windows.Forms.Button ShowFormBtn;
		private System.Windows.Forms.Button ShowDocumentBtnTop;
		private System.Windows.Forms.DataGridView MyRequestsDGV;
		private System.Windows.Forms.PictureBox LoadingBoxTop;
		private System.Windows.Forms.PictureBox LoadingBoxBottom;
		private System.Windows.Forms.Button RefreshBtnBottom;
		private System.Windows.Forms.Button ShowDocumentBtnBottom;
		private System.Windows.Forms.SplitContainer MasterSplitContainer;
		private System.Windows.Forms.ComboBox BottomComboBox;
		private System.Windows.Forms.Label Toplbl;
		private System.Windows.Forms.Label BottomLbl;
		private System.Windows.Forms.TextBox BottomDGVFilterTxt;
		private System.Windows.Forms.TextBox TopDGVFilterTxt;
		private System.Windows.Forms.Label TopFilterLbl;
		private System.Windows.Forms.Label BottomFilterLbl;
		private System.Windows.Forms.Button DelFormBtn;
		private System.Windows.Forms.Panel TopPanel1;
		private System.Windows.Forms.Panel BottomPanel1;
		private System.Windows.Forms.Panel TopPanel2;
		private System.Windows.Forms.Panel BottomPanel2;
		private System.Windows.Forms.DataGridViewTextBoxColumn DateReceivedColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn TitleColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn DescriptionColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn RequesterColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn DateProcessedColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ProcessedColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn FormInstanceIdColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn AttachmentIdColumn;

	}
}