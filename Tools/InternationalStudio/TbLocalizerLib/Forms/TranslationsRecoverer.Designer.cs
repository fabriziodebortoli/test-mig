using System.Windows.Forms;
namespace Microarea.Tools.TBLocalizer.Forms
{
	partial class TranslationsRecoverer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TranslationsRecoverer));
			this.labelNewStrings = new System.Windows.Forms.Label();
			this.labelOldStrings = new System.Windows.Forms.Label();
			this.btnAddSource = new System.Windows.Forms.Button();
			this.panelArrows = new System.Windows.Forms.Panel();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.listViewOld = new Microarea.Tools.TBLocalizer.Forms.MyListView();
			this.columnHeaderOldString = new System.Windows.Forms.ColumnHeader();
			this.listViewNew = new Microarea.Tools.TBLocalizer.Forms.MyListView();
			this.columnHeaderNewString = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// labelNewStrings
			// 
			this.labelNewStrings.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.labelNewStrings.AutoSize = true;
			this.labelNewStrings.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.labelNewStrings.Location = new System.Drawing.Point(42, 52);
			this.labelNewStrings.Name = "labelNewStrings";
			this.labelNewStrings.Size = new System.Drawing.Size(97, 13);
			this.labelNewStrings.TabIndex = 0;
			this.labelNewStrings.Text = "Strings to translate:";
			// 
			// labelOldStrings
			// 
			this.labelOldStrings.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.labelOldStrings.AutoSize = true;
			this.labelOldStrings.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.labelOldStrings.Location = new System.Drawing.Point(440, 52);
			this.labelOldStrings.Name = "labelOldStrings";
			this.labelOldStrings.Size = new System.Drawing.Size(135, 13);
			this.labelOldStrings.TabIndex = 2;
			this.labelOldStrings.Text = "Existing invalid translations:";
			// 
			// btnAddSource
			// 
			this.btnAddSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAddSource.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnAddSource.Location = new System.Drawing.Point(663, 387);
			this.btnAddSource.Name = "btnAddSource";
			this.btnAddSource.Size = new System.Drawing.Size(83, 23);
			this.btnAddSource.TabIndex = 7;
			this.btnAddSource.Text = "&Add source...";
			this.btnAddSource.UseVisualStyleBackColor = true;
			this.btnAddSource.Click += new System.EventHandler(this.btnAddSource_Click);
			// 
			// panelArrows
			// 
			this.panelArrows.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.panelArrows.Location = new System.Drawing.Point(362, 71);
			this.panelArrows.Name = "panelArrows";
			this.panelArrows.Size = new System.Drawing.Size(72, 309);
			this.panelArrows.TabIndex = 4;
			this.panelArrows.Paint += new System.Windows.Forms.PaintEventHandler(this.panelArrows_Paint);
			// 
			// btnSave
			// 
			this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnSave.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnSave.Location = new System.Drawing.Point(298, 387);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(83, 23);
			this.btnSave.TabIndex = 5;
			this.btnSave.Text = "&Save";
			this.btnSave.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnCancel.Location = new System.Drawing.Point(411, 387);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(83, 23);
			this.btnCancel.TabIndex = 6;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "Report.bmp");
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(45, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(611, 13);
			this.label1.TabIndex = 8;
			this.label1.Text = "Drag and drop with mouse existing invalid translations onto strings to translate " +
				"(or use CTRL+c, CTRL+v) to perform associations.";
			// 
			// listViewOld
			// 
			this.listViewOld.AllowDrop = true;
			this.listViewOld.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.listViewOld.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderOldString});
			this.listViewOld.FullRowSelect = true;
			this.listViewOld.GridLines = true;
			this.listViewOld.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewOld.Location = new System.Drawing.Point(440, 71);
			this.listViewOld.MultiSelect = false;
			this.listViewOld.Name = "listViewOld";
			this.listViewOld.ShowItemToolTips = true;
			this.listViewOld.Size = new System.Drawing.Size(310, 309);
			this.listViewOld.SmallImageList = this.imageList1;
			this.listViewOld.TabIndex = 3;
			this.listViewOld.UseCompatibleStateImageBehavior = false;
			this.listViewOld.View = System.Windows.Forms.View.Details;
			this.listViewOld.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewOld_DragEnter);
			this.listViewOld.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewOld_DragDrop);
			this.listViewOld.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewOld_DragOver);
			this.listViewOld.SizeChanged += new System.EventHandler(this.listViewOld_SizeChanged);
			this.listViewOld.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listViewOld_KeyPress);
			this.listViewOld.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listViewOld_MouseDown);
			// 
			// columnHeaderOldString
			// 
			this.columnHeaderOldString.Text = "Old strings";
			this.columnHeaderOldString.Width = 306;
			// 
			// listViewNew
			// 
			this.listViewNew.AllowDrop = true;
			this.listViewNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.listViewNew.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderNewString});
			this.listViewNew.FullRowSelect = true;
			this.listViewNew.GridLines = true;
			this.listViewNew.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewNew.Location = new System.Drawing.Point(45, 71);
			this.listViewNew.MultiSelect = false;
			this.listViewNew.Name = "listViewNew";
			this.listViewNew.ShowItemToolTips = true;
			this.listViewNew.Size = new System.Drawing.Size(311, 309);
			this.listViewNew.SmallImageList = this.imageList1;
			this.listViewNew.TabIndex = 1;
			this.listViewNew.UseCompatibleStateImageBehavior = false;
			this.listViewNew.View = System.Windows.Forms.View.Details;
			this.listViewNew.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewNew_DragEnter);
			this.listViewNew.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewNew_DragDrop);
			this.listViewNew.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewNew_DragOver);
			this.listViewNew.SizeChanged += new System.EventHandler(this.listViewNew_SizeChanged);
			this.listViewNew.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listViewNew_KeyPress);
			// 
			// columnHeaderNewString
			// 
			this.columnHeaderNewString.Text = "New strings";
			this.columnHeaderNewString.Width = 307;
			// 
			// TranslationsRecoverer
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(792, 416);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.panelArrows);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.btnAddSource);
			this.Controls.Add(this.labelOldStrings);
			this.Controls.Add(this.labelNewStrings);
			this.Controls.Add(this.listViewOld);
			this.Controls.Add(this.listViewNew);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(800, 150);
			this.Name = "TranslationsRecoverer";
			this.RightToLeftLayout = true;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Translations Recoverer";
			this.SizeChanged += new System.EventHandler(this.TranslationsRecoverer_SizeChanged);
			this.DragOver += new System.Windows.Forms.DragEventHandler(this.TranslationsRecoverer_DragOver);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TranslationsRecoverer_FormClosing);
			this.Load += new System.EventHandler(this.TranslationsRecoverer_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MyListView listViewNew;
		private MyListView listViewOld;
		private System.Windows.Forms.Label labelNewStrings;
		private System.Windows.Forms.Label labelOldStrings;
		private System.Windows.Forms.Button btnAddSource;
		private ColumnHeader columnHeaderNewString;
		private Panel panelArrows;
		private ColumnHeader columnHeaderOldString;
		private Button btnSave;
		private Button btnCancel;
		private ImageList imageList1;
		private Label label1;
	}
}