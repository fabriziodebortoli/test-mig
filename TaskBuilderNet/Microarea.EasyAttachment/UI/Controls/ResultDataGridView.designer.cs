namespace Microarea.EasyAttachment.UI.Controls
{
	partial class ResultDataGridView
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
            f.Dispose();
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResultDataGridView));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			this.SuspendLayout();
			// 
			// ResultDataGridView
			// 
			this.AllowUserToAddRows = false;
			this.AllowUserToOrderColumns = true;
			this.AllowUserToResizeRows = false;
			resources.ApplyResources(this, "$this");
			this.BackgroundColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.ColumnHeadersVisible = false;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.DefaultCellStyle = dataGridViewCellStyle1;
			this.GridColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.MultiSelect = false;
			this.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.RowHeadersVisible = false;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.ActiveCaption;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
			this.RowsDefaultCellStyle = dataGridViewCellStyle2;
			this.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.ShowEditingIcon = false;
			this.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.ResultDataGridView_RowsAdded);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ResultDataGridView_MouseClick);
			this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ResultDataGridView_MouseDoubleClick);
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion
	}
}
