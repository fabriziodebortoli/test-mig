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
	/// DataGridStateDropDown.
	/// </summary>
	/// =======================================================================
	public class DataGridStateDropDown  : ComboBox
	{

		private DataTable statesDataTable = new DataTable(WorkFlowState.WorkFlowStateTableName);

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//---------------------------------------------------------------------
		public DataGridStateDropDown(int companyId, int workflowId, SqlConnection currentConnection)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.DropDownStyle		= ComboBoxStyle.DropDownList;
			this.DrawMode			= System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.BackColor			= System.Drawing.SystemColors.Window;
			this.ForeColor			= System.Drawing.SystemColors.WindowText;
			this.ItemHeight			= this.Font.Height + 2;
			this.IntegralHeight		= true;
			this.MaxDropDownItems	= 5;
			LoadAllStates(companyId, workflowId, currentConnection);	

		}

		//---------------------------------------------------------------------
		protected override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
		{
			if (e.Index == -1) 
				return;

			object currentValueId = Items[e.Index];
			string currentValueName = string.Empty;
			if (currentValueId != null)
				currentValueName = this.FindStateName((int)currentValueId);
			
						
			SolidBrush drawStringBrush = new SolidBrush(Color.Black);
	
			e.Graphics.DrawString(currentValueName, this.Font, drawStringBrush, e.Bounds.X +  2, e.Bounds.Y);

			drawStringBrush.Dispose(); 
			
			base.OnDrawItem(e);
		}

		//---------------------------------------------------------------------
		public void LoadAllStates(int companyId, int workflowId, SqlConnection currentConnection)
		{
			this.Items.Clear();

			statesDataTable.Rows.Clear();
			WorkFlowState states = new WorkFlowState(companyId, workflowId);
			SqlDataAdapter selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowState.GetSelectAllStatesOrderedByNameQuery(companyId, workflowId), currentConnection);
			selectAllSqlDataAdapter.Fill(statesDataTable);

			for (int i = 0; i < statesDataTable.Rows.Count; i++)
				this.Items.Add(statesDataTable.Rows[i][WorkFlowState.StateIdColumnName]);

			//mi posiziono sul primo elementi
			this.SelectedIndex = 0;
		}

		//---------------------------------------------------------------------
		public string FindStateName(int stateIdToFind)
		{
			string currentValueName = string.Empty;
			for ( int i = 0; i < statesDataTable.Rows.Count; i++)
			{
				int stateId = (int)statesDataTable.Rows[i][WorkFlowState.StateIdColumnName];
				if (stateId == stateIdToFind)
				{
					currentValueName = (string) statesDataTable.Rows[i][WorkFlowState.StateNameColumnName];
					break;
				}

			}
			return currentValueName;
		}

		//---------------------------------------------------------------------
		public int GetItemIndexFromObject(object objectValue)
		{
			if(objectValue == null || objectValue == System.DBNull.Value)
			{
				return -1;
			}

			return this.FindStringExact(objectValue.ToString());
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
	public class DataGridStateDropDownColumn : DataGridColumnStyle
	{
		private DataGridStateDropDown  dataGridStateDropDown = null;

		private string               oldVal  = new string(string.Empty.ToCharArray());
		private bool                 inEdit  = false;
		private const int dropdownDefaultWidth = 132;
		private const int yMargin = 4;
		private const int xMargin = 4;

		

		public delegate void ModifyColumnValueHandle(object sender, int rowNumber);
		public event ModifyColumnValueHandle OnModifyColumnValueHandle;

		//---------------------------------------------------------------------
		public DataGridStateDropDownColumn(System.Drawing.Font aFont, int companyId, int workflowId, SqlConnection currentConnection)
		{
			dataGridStateDropDown				= new DataGridStateDropDown(companyId, workflowId, currentConnection);
			dataGridStateDropDown.Visible       = false;
			//dataGridStateDropDown.Location      = new System.Drawing.Point(8, 0);
			//dataGridStateDropDown.Size          = new System.Drawing.Size(dropdownDefaultWidth, aFont.Height + 4);
			//dataGridStateDropDown.TabIndex      = 0;
			dataGridStateDropDown.DropDownWidth = dropdownDefaultWidth;
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
			oldVal = dataGridStateDropDown.Text;

			int currentStateId = 0;
			object valueIntoCell = GetColumnValueAtRow(source, rowNumber);
			if (valueIntoCell != null && valueIntoCell != System.DBNull.Value)
				currentStateId = (int)valueIntoCell;

	
			if(cellIsVisible)
			{
				//bounds.Offset(xMargin, yMargin);
				//bounds.Width -= xMargin * 2;
				//bounds.Height -= yMargin;
				dataGridStateDropDown.Bounds = bounds;
				dataGridStateDropDown.Visible = true;

				dataGridStateDropDown.SelectedItem = currentStateId;
				
				if (OnModifyColumnValueHandle != null)
					OnModifyColumnValueHandle(this, rowNumber);
				
			}
			else
			{
				dataGridStateDropDown.SelectedItem = currentStateId;
				dataGridStateDropDown.Bounds = originalBounds;
				dataGridStateDropDown.Visible = false;
			}
				
			if (instantText != null)
				dataGridStateDropDown.SelectedIndex = dataGridStateDropDown.FindStringExact(instantText);
			else
				dataGridStateDropDown.SelectedIndex = dataGridStateDropDown.GetItemIndexFromObject(GetColumnValueAtRow(source, rowNumber));

			if(dataGridStateDropDown.Visible)
				DataGridTableStyle.DataGrid.Invalidate(originalBounds);

			inEdit = true;
		}

		//---------------------------------------------------------------------
		protected override bool Commit(CurrencyManager dataSource,int rowNumber)
		{
			HideComboBox();
			
			if(!inEdit)
			{
				return true;
			}
			try
			{
				object Value = dataGridStateDropDown.SelectedItem;
				if (Value == null) return false;
				if(NullText.Equals(Value))
				{
					Value = System.Convert.DBNull; 
				}
				SetColumnValueAtRow(dataSource, rowNumber, Value);
			}
			catch
			{
				RollBack();
				return false;	
			}
			
			this.EndEdit();
			return true;
		}

		//---------------------------------------------------------------------
		protected override void Abort(int rowNumber)
		{
			RollBack();
			HideComboBox();
			EndEdit();
		}

		//---------------------------------------------------------------------
		public void EndEdit()
		{
			inEdit = false;
			Invalidate();
		}

		//---------------------------------------------------------------------
		private void HideComboBox()
		{
			if(dataGridStateDropDown.Focused)
			{
				this.DataGridTableStyle.DataGrid.Focus();
			}
			dataGridStateDropDown.Visible = false;
		}

		//---------------------------------------------------------------------
		private void RollBack()
		{
			dataGridStateDropDown.Text = oldVal;
			if (oldVal == string.Empty)
				dataGridStateDropDown.Text = "seleziona una stato";

		}

		//---------------------------------------------------------------------
		protected override void SetDataGridInColumn(DataGrid value) 
		{
			base.SetDataGridInColumn(value);
			if (dataGridStateDropDown.Parent != null) 
			{
				dataGridStateDropDown.Parent.Controls.Remove 
					(dataGridStateDropDown);
			}
			if (value != null) 
			{
				value.Controls.Add(dataGridStateDropDown);
			}
		}

		//---------------------------------------------------------------------
		protected override int GetMinimumHeight()
		{
			return dataGridStateDropDown.PreferredHeight + yMargin;
		}

		//---------------------------------------------------------------------
		protected override int GetPreferredHeight(Graphics g ,object objectValue)
		{
			return dataGridStateDropDown.PreferredHeight + yMargin;
		}

		//---------------------------------------------------------------------
		protected override Size GetPreferredSize(Graphics g, object objectValue)
		{
			return new Size(dataGridStateDropDown.Width + xMargin, dataGridStateDropDown.Height + yMargin);
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
			//g.FillRectangle(backBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);

			object valueObject = GetColumnValueAtRow(source, rowNumber);
			Rectangle rect = bounds;
			g.FillRectangle(backBrush, rect);
			rect.Offset(0, 2);
			rect.Height -= 2;
			if ((valueObject == System.DBNull.Value) || (valueObject == null))
				return;
			string objectName = dataGridStateDropDown.FindStateName((int)valueObject);
			
			g.DrawString(objectName, dataGridStateDropDown.Font, foreBrush, bounds.X + 2, bounds .Y);
			
		}

	}
}
