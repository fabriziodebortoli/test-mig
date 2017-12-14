using System;
using System.Collections;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Summary description for EnvironmentVariableChooser.
	/// </summary>
	//================================================================================
	public class EnvironmentVariableChooser : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListView lstVariables;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ColumnHeader ColumnName;
		private System.Windows.Forms.ColumnHeader ColumnValue;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		public string SelectedItem
		{
			get
			{
				return lstVariables.SelectedItems[0].Text;
			}
		}

		//--------------------------------------------------------------------------------
		public EnvironmentVariableChooser()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			this.lstVariables = new System.Windows.Forms.ListView();
			this.ColumnName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ColumnValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lstVariables
			// 
			this.lstVariables.AllowColumnReorder = true;
			this.lstVariables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lstVariables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnName,
            this.ColumnValue});
			this.lstVariables.GridLines = true;
			this.lstVariables.Location = new System.Drawing.Point(24, 40);
			this.lstVariables.Name = "lstVariables";
			this.lstVariables.Size = new System.Drawing.Size(451, 213);
			this.lstVariables.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.lstVariables.TabIndex = 0;
			this.lstVariables.UseCompatibleStateImageBehavior = false;
			this.lstVariables.View = System.Windows.Forms.View.Details;
			this.lstVariables.DoubleClick += new System.EventHandler(this.lstVariables_DoubleClick);
			// 
			// ColumnName
			// 
			this.ColumnName.Text = "Name";
			this.ColumnName.Width = 200;
			// 
			// ColumnValue
			// 
			this.ColumnValue.Text = "Value";
			this.ColumnValue.Width = 200;
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnOK.Location = new System.Drawing.Point(134, 277);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 1;
			this.btnOK.Text = "&OK";
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnCancel.Location = new System.Drawing.Point(239, 277);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "&Cancel";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(328, 24);
			this.label1.TabIndex = 0;
			this.label1.Text = "Choose the environment variable:";
			// 
			// EnvironmentVariableChooser
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(7, 16);
			this.ClientSize = new System.Drawing.Size(491, 307);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.lstVariables);
			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "EnvironmentVariableChooser";
			this.Text = "Environment variables";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.EnvironmentVariableChooser_Closing);
			this.Load += new System.EventHandler(this.EnvironmentVariableChooser_Load);
			this.ResumeLayout(false);

		}
		#endregion

		//--------------------------------------------------------------------------------
		private void EnvironmentVariableChooser_Load(object sender, System.EventArgs e)
		{
			foreach (DictionaryEntry de in CommonFunctions.CurrentEnvironmentSettings.EnvironmentVariables)
			{
				string [] items = new string[2];
				items[0] = de.Key as String;
				items[1] = de.Value as String;
				ListViewItem item = new ListViewItem(items);
				lstVariables.Items.Add(item);
			}
			
		}

		//--------------------------------------------------------------------------------
		private void lstVariables_DoubleClick(object sender, System.EventArgs e)
		{
			if (lstVariables.SelectedItems.Count > 0)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		//--------------------------------------------------------------------------------
		private void EnvironmentVariableChooser_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
				e.Cancel = lstVariables.SelectedItems.Count <= 0;
		}
	}
}
