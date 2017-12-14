namespace Microarea.EasyBuilder.UI
{
	partial class DBTEditor
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DBTEditor));
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lblSlaveFK = new System.Windows.Forms.Label();
			this.grdRelationShip = new System.Windows.Forms.DataGridView();
			this.rbnSlave11 = new System.Windows.Forms.RadioButton();
			this.rbnSlave1n = new System.Windows.Forms.RadioButton();
			this.lblMasterName = new System.Windows.Forms.Label();
			this.lblMasterTableName = new System.Windows.Forms.Label();
			this.pnlRelation = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.dataManagerControl = new Microarea.EasyBuilder.UI.TableSourceControl();
			((System.ComponentModel.ISupportInitialize)(this.grdRelationShip)).BeginInit();
			this.pnlRelation.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.Name = "btnOk";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// lblSlaveFK
			// 
			resources.ApplyResources(this.lblSlaveFK, "lblSlaveFK");
			this.lblSlaveFK.Name = "lblSlaveFK";
			// 
			// grdRelationShip
			// 
			this.grdRelationShip.AllowUserToAddRows = false;
			this.grdRelationShip.AllowUserToDeleteRows = false;
			this.grdRelationShip.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.grdRelationShip.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.grdRelationShip.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
			this.grdRelationShip.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			this.grdRelationShip.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.grdRelationShip.GridColor = System.Drawing.SystemColors.ControlDarkDark;
			resources.ApplyResources(this.grdRelationShip, "grdRelationShip");
			this.grdRelationShip.MultiSelect = false;
			this.grdRelationShip.Name = "grdRelationShip";
			this.grdRelationShip.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			this.grdRelationShip.RowHeadersVisible = false;
			this.grdRelationShip.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.grdRelationShip.ShowCellErrors = false;
			this.grdRelationShip.ShowCellToolTips = false;
			this.grdRelationShip.ShowEditingIcon = false;
			// 
			// rbnSlave11
			// 
			resources.ApplyResources(this.rbnSlave11, "rbnSlave11");
			this.rbnSlave11.Name = "rbnSlave11";
			this.rbnSlave11.TabStop = true;
			this.rbnSlave11.UseVisualStyleBackColor = true;
			// 
			// rbnSlave1n
			// 
			resources.ApplyResources(this.rbnSlave1n, "rbnSlave1n");
			this.rbnSlave1n.Name = "rbnSlave1n";
			this.rbnSlave1n.TabStop = true;
			this.rbnSlave1n.UseVisualStyleBackColor = true;
			// 
			// lblMasterName
			// 
			resources.ApplyResources(this.lblMasterName, "lblMasterName");
			this.lblMasterName.BackColor = System.Drawing.Color.Transparent;
			this.lblMasterName.Name = "lblMasterName";
			// 
			// lblMasterTableName
			// 
			this.lblMasterTableName.BackColor = System.Drawing.Color.White;
			this.lblMasterTableName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			resources.ApplyResources(this.lblMasterTableName, "lblMasterTableName");
			this.lblMasterTableName.Name = "lblMasterTableName";
			// 
			// pnlRelation
			// 
			this.pnlRelation.BackColor = System.Drawing.Color.Transparent;
			this.pnlRelation.BackgroundImage = global::Microarea.EasyBuilder.Properties.Resources.DBTSlaveRelation;
			resources.ApplyResources(this.pnlRelation, "pnlRelation");
			this.pnlRelation.Controls.Add(this.label2);
			this.pnlRelation.Controls.Add(this.pictureBox2);
			this.pnlRelation.Controls.Add(this.pictureBox1);
			this.pnlRelation.Controls.Add(this.lblSlaveFK);
			this.pnlRelation.Controls.Add(this.lblMasterTableName);
			this.pnlRelation.Controls.Add(this.lblMasterName);
			this.pnlRelation.Controls.Add(this.rbnSlave1n);
			this.pnlRelation.Controls.Add(this.rbnSlave11);
			this.pnlRelation.Controls.Add(this.grdRelationShip);
			this.pnlRelation.Name = "pnlRelation";
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Name = "label2";
			// 
			// pictureBox2
			// 
			this.pictureBox2.Image = global::Microarea.EasyBuilder.Properties.Resources.DbtKeys;
			resources.ApplyResources(this.pictureBox2, "pictureBox2");
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.TabStop = false;
			// 
			// pictureBox1
			// 
			resources.ApplyResources(this.pictureBox1, "pictureBox1");
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.TabStop = false;
			// 
			// dataManagerControl
			// 
			resources.ApplyResources(this.dataManagerControl, "dataManagerControl");
			this.dataManagerControl.Name = "dataManagerControl";
			this.dataManagerControl.ObjectName = "";
			this.dataManagerControl.TableChangeable = true;
			this.dataManagerControl.NoSelection += new System.EventHandler(this.dataManagerControl_NoSelection);
			this.dataManagerControl.SelectedTableChanged += new System.EventHandler(this.CbxTableName_SelectedTableChanged);
			// 
			// DBTEditor
			// 
			this.AcceptButton = this.btnOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ControlBox = false;
			this.Controls.Add(this.dataManagerControl);
			this.Controls.Add(this.pnlRelation);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DBTEditor";
			((System.ComponentModel.ISupportInitialize)(this.grdRelationShip)).EndInit();
			this.pnlRelation.ResumeLayout(false);
			this.pnlRelation.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		
		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.DataGridView grdRelationShip;
		private System.Windows.Forms.RadioButton rbnSlave11;
		private System.Windows.Forms.RadioButton rbnSlave1n;
		private System.Windows.Forms.Label lblMasterName;
		private System.Windows.Forms.Label lblSlaveFK;
		private System.Windows.Forms.Panel pnlRelation;
		private System.Windows.Forms.Label lblMasterTableName;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.Label label2;
		private TableSourceControl dataManagerControl;
	}
}