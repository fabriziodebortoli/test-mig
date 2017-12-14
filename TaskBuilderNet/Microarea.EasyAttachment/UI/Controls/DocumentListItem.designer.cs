namespace Microarea.EasyAttachment.UI.Controls
{
    partial class DocumentListItem
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocumentListItem));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			this.documentName = new System.Windows.Forms.Label();
			this.keyDescription = new System.Windows.Forms.Label();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.expandBtn = new System.Windows.Forms.Button();
			this.dataGridViewImageColumn1 = new System.Windows.Forms.DataGridViewImageColumn();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.OpenMagoDocBtn = new System.Windows.Forms.Button();
			this.BtnToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.attachmentGridView = new Microarea.EasyAttachment.UI.Controls.ResultDataGridView();
			((System.ComponentModel.ISupportInitialize)(this.attachmentGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// documentName
			// 
			resources.ApplyResources(this.documentName, "documentName");
			this.documentName.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.documentName.Name = "documentName";
			this.documentName.DoubleClick += new System.EventHandler(this.documentName_DoubleClick);
			// 
			// keyDescription
			// 
			resources.ApplyResources(this.keyDescription, "keyDescription");
			this.keyDescription.Name = "keyDescription";
			this.keyDescription.DoubleClick += new System.EventHandler(this.keyDescription_DoubleClick);
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "Ext_BMP32x32.png");
			this.imageList.Images.SetKeyName(1, "Ext_DOC32x32.png");
			this.imageList.Images.SetKeyName(2, "Ext_GIF32x32.png");
			this.imageList.Images.SetKeyName(3, "Ext_GZIP32x32.png");
			this.imageList.Images.SetKeyName(4, "Ext_HTML32x32.png");
			this.imageList.Images.SetKeyName(5, "Ext_JPG32x32.png");
			this.imageList.Images.SetKeyName(6, "Ext_MAIL32x32.png");
			this.imageList.Images.SetKeyName(7, "Ext_PDF32x32.png");
			this.imageList.Images.SetKeyName(8, "Ext_PNG32x32.png");
			this.imageList.Images.SetKeyName(9, "Ext_PPT32x32.png");
			this.imageList.Images.SetKeyName(10, "Ext_RAR32x32.png");
			this.imageList.Images.SetKeyName(11, "Ext_TXT32x32.png");
			this.imageList.Images.SetKeyName(12, "Ext_XLS32x32.png");
			this.imageList.Images.SetKeyName(13, "Ext_ZIP32x32.png");
			this.imageList.Images.SetKeyName(14, "Ext_WMV32x32.png");
			this.imageList.Images.SetKeyName(15, "Ext_MPEG32x32.png");
			this.imageList.Images.SetKeyName(16, "Ext_AVI32x32.png");
			this.imageList.Images.SetKeyName(17, "Ext_WAV32x32.png");
			this.imageList.Images.SetKeyName(18, "Ext_MP332x32.png");
			this.imageList.Images.SetKeyName(19, "Ext_Default32x32.png");
			// 
			// expandBtn
			// 
			resources.ApplyResources(this.expandBtn, "expandBtn");
			this.expandBtn.Image = global::Microarea.EasyAttachment.Properties.Resources.arrowdown16x16;
			this.expandBtn.Name = "expandBtn";
			this.expandBtn.UseVisualStyleBackColor = true;
			this.expandBtn.Click += new System.EventHandler(this.expandBtn_Click);
			this.expandBtn.MouseMove += new System.Windows.Forms.MouseEventHandler(this.expandBtn_MouseMove);
			// 
			// dataGridViewImageColumn1
			// 
			this.dataGridViewImageColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.dataGridViewImageColumn1.DataPropertyName = "DocExtensionType";
			this.dataGridViewImageColumn1.Name = "dataGridViewImageColumn1";
			this.dataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			resources.ApplyResources(this.dataGridViewImageColumn1, "dataGridViewImageColumn1");
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.dataGridViewTextBoxColumn1.DataPropertyName = "Name";
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Verdana", 6.75F);
			this.dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle1;
			resources.ApplyResources(this.dataGridViewTextBoxColumn1, "dataGridViewTextBoxColumn1");
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.dataGridViewTextBoxColumn2.DataPropertyName = "AttachmentDate";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			resources.ApplyResources(this.dataGridViewTextBoxColumn2, "dataGridViewTextBoxColumn2");
			// 
			// OpenMagoDocBtn
			// 
			this.OpenMagoDocBtn.Image = global::Microarea.EasyAttachment.Properties.Resources.DataEntry32x32;
			resources.ApplyResources(this.OpenMagoDocBtn, "OpenMagoDocBtn");
			this.OpenMagoDocBtn.Name = "OpenMagoDocBtn";
			this.OpenMagoDocBtn.UseVisualStyleBackColor = true;
			this.OpenMagoDocBtn.Click += new System.EventHandler(this.OpenMagoDocBtn_Click);
			this.OpenMagoDocBtn.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OpenMagoDocBtn_MouseMove);
			// 
			// BtnToolTip
			// 
			this.BtnToolTip.AutoPopDelay = 3000;
			this.BtnToolTip.InitialDelay = 50;
			this.BtnToolTip.ReshowDelay = 100;
			// 
			// attachmentGridView
			// 
			this.attachmentGridView.AllowUserToAddRows = false;
			this.attachmentGridView.AllowUserToDeleteRows = false;
			this.attachmentGridView.AllowUserToOrderColumns = true;
			this.attachmentGridView.AllowUserToResizeRows = false;
			resources.ApplyResources(this.attachmentGridView, "attachmentGridView");
			this.attachmentGridView.BackgroundColor = System.Drawing.SystemColors.ActiveCaption;
			this.attachmentGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.attachmentGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.attachmentGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.attachmentGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.attachmentGridView.ColumnHeadersVisible = false;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ActiveCaption;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Verdana", 9F);
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.attachmentGridView.DefaultCellStyle = dataGridViewCellStyle2;
			this.attachmentGridView.GridColor = System.Drawing.SystemColors.ActiveCaption;
			this.attachmentGridView.MultiSelect = false;
			this.attachmentGridView.Name = "attachmentGridView";
			this.attachmentGridView.ReadOnly = true;
			this.attachmentGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.attachmentGridView.RowHeadersVisible = false;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.Azure;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
			this.attachmentGridView.RowsDefaultCellStyle = dataGridViewCellStyle3;
			this.attachmentGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.attachmentGridView.ShowEditingIcon = false;
			// 
			// DocumentListItem
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.OpenMagoDocBtn);
			this.Controls.Add(this.expandBtn);
			this.Controls.Add(this.attachmentGridView);
			this.Controls.Add(this.keyDescription);
			this.Controls.Add(this.documentName);
			resources.ApplyResources(this, "$this");
			this.Name = "DocumentListItem";
			((System.ComponentModel.ISupportInitialize)(this.attachmentGridView)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.Label documentName;
		private System.Windows.Forms.Label keyDescription;
		private System.Windows.Forms.ImageList imageList;
		private ResultDataGridView attachmentGridView;
		private System.Windows.Forms.Button expandBtn;
		private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.Button OpenMagoDocBtn;
		private System.Windows.Forms.ToolTip BtnToolTip;
    }
}
