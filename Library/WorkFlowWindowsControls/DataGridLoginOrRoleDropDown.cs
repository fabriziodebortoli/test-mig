using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

using Microarea.Library.WorkFlowObjects;

namespace Microarea.Library.WorkFlowWindowsControls
{
	/// <summary>
	/// DataGridLoginOrRoleDropDown.
	/// </summary>
	/// =======================================================================
	public class DataGridLoginOrRoleDropDown : ComboBox
	{
		private DataTable usersOrRolesDataTable = new DataTable(WorkFlowUser.WorkFlowUserTableName);

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//---------------------------------------------------------------------
		public DataGridLoginOrRoleDropDown()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			this.DropDownStyle		= ComboBoxStyle.DropDownList;
			this.DrawMode			= System.Windows.Forms.DrawMode.Normal;
			this.BackColor			= System.Drawing.SystemColors.Window;
			this.ForeColor			= System.Drawing.SystemColors.WindowText;
			this.ItemHeight			= this.Font.Height + 2;
			this.IntegralHeight		= true;
			this.MaxDropDownItems	= 5;

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}

	/// =======================================================================
	public class DataGridLoginOrRoleDropDownColumn : DataGridColumnStyle
	{
		private DataGridLoginOrRoleDropDown  dataGridOwnerDropDown;

		private string               oldVal  = new string(string.Empty.ToCharArray());
		//private bool                 inEdit  = false;
		private const int dropdownDefaultWidth = 132;
		private int                  xMargin = 2;
		private int                  yMargin = 1;

		public delegate void ModifyColumnValueHandle(object sender, int rowNumber);
		public event ModifyColumnValueHandle OnModifyColumnValueHandle;

		//---------------------------------------------------------------------
		public DataGridLoginOrRoleDropDownColumn(System.Drawing.Font aFont, int companyId, int workflowId, SqlConnection currentConnection)
		{
			dataGridOwnerDropDown				= new DataGridLoginOrRoleDropDown();
			dataGridOwnerDropDown.Visible       = true;
			dataGridOwnerDropDown.DropDownWidth = dropdownDefaultWidth;


		}

		//---------------------------------------------------------------------
		protected override void Edit
			(
			CurrencyManager source,
			int rowNumber,
			Rectangle bounds, 
			bool readOnly,
			string instantText, 
			bool cellIsVisible
			)
		{
			Rectangle originalBounds = bounds;
			oldVal = dataGridOwnerDropDown.Text;
	
			if(cellIsVisible)
			{
				bounds.Offset(xMargin, yMargin);
				bounds.Width -= xMargin * 2;
				bounds.Height -= yMargin;
				dataGridOwnerDropDown.Bounds = bounds;
				dataGridOwnerDropDown.Visible = true;
				if (OnModifyColumnValueHandle != null)
					OnModifyColumnValueHandle(this, rowNumber);
				
			}
			else
			{
				dataGridOwnerDropDown.Bounds = originalBounds;
				dataGridOwnerDropDown.Visible = false;
			}
				
			

			if(dataGridOwnerDropDown.Visible)
				DataGridTableStyle.DataGrid.Invalidate(originalBounds);

			//inEdit = true;
		}

		//---------------------------------------------------------------------
		protected override bool Commit(CurrencyManager dataSource,int rowNumber)
		{
			return true;
		}

		//---------------------------------------------------------------------
		protected override void Abort(int rowNumber)
		{
		}

		//---------------------------------------------------------------------
		protected override int GetMinimumHeight()
		{
			return dataGridOwnerDropDown.PreferredHeight + yMargin;
		}

		//---------------------------------------------------------------------
		protected override int GetPreferredHeight(Graphics g ,object objectValue)
		{
			return dataGridOwnerDropDown.PreferredHeight + yMargin;
		}

		//---------------------------------------------------------------------
		protected override Size GetPreferredSize(Graphics g, object objectValue)
		{
			return new Size(dataGridOwnerDropDown.Width + xMargin, dataGridOwnerDropDown.Height + yMargin);
		}


		//---------------------------------------------------------------------
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNumber) 
		{
			Paint(g, bounds, source, rowNumber, this.Alignment == System.Windows.Forms.HorizontalAlignment.Right);
		}

		//---------------------------------------------------------------------
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNumber, bool alignToRight) 
		{
			SolidBrush backBrush = new SolidBrush(Color.White);
			g.FillRectangle(backBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);
			
		}

		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics g,
			Rectangle bounds,
			CurrencyManager source, 
			int rowNumber, 
			Brush backBrush,
			Brush foreBrush,
			bool alignToRight
			) 
		{
			g.FillRectangle(backBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);
			
		}

	}
}
