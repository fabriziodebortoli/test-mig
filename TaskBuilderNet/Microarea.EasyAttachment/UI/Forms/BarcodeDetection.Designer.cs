namespace Microarea.EasyAttachment.UI.Forms
{
	partial class BarcodeDetection
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BarcodeDetection));
			this.LblMessage = new System.Windows.Forms.Label();
			this.BtnOK = new System.Windows.Forms.Button();
			this.ValDataGridView = new System.Windows.Forms.DataGridView();
			((System.ComponentModel.ISupportInitialize)(this.ValDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// LblMessage
			// 
			resources.ApplyResources(this.LblMessage, "LblMessage");
			this.LblMessage.Name = "LblMessage";
			// 
			// BtnOK
			// 
			resources.ApplyResources(this.BtnOK, "BtnOK");
			this.BtnOK.Name = "BtnOK";
			this.BtnOK.UseVisualStyleBackColor = true;
			this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// ValDataGridView
			// 
			this.ValDataGridView.AllowUserToAddRows = false;
			this.ValDataGridView.AllowUserToDeleteRows = false;
			this.ValDataGridView.BackgroundColor = System.Drawing.Color.Lavender;
			this.ValDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.ValDataGridView.GridColor = System.Drawing.Color.LightBlue;
			resources.ApplyResources(this.ValDataGridView, "ValDataGridView");
			this.ValDataGridView.MultiSelect = false;
			this.ValDataGridView.Name = "ValDataGridView";
			this.ValDataGridView.RowHeadersVisible = false;
			this.ValDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.ValDataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ValDataGridView_CellClick);
			this.ValDataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.ValDataGridView_DataError);
			// 
			// BarcodeDetection
			// 
			this.AcceptButton = this.BtnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.ValDataGridView);
			this.Controls.Add(this.BtnOK);
			this.Controls.Add(this.LblMessage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BarcodeDetection";
			this.ShowInTaskbar = false;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BarcodeDetection_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.ValDataGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label LblMessage;
		private System.Windows.Forms.Button BtnOK;
		private System.Windows.Forms.DataGridView ValDataGridView;
	}
}