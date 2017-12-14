using System;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

using Microarea.Library.WorkFlowObjects;

namespace Microarea.Library.WorkFlowWindowsControls
{
	/// <summary>
	/// DataGridActivityDropDown.
	/// </summary>
	/// =======================================================================
	public class DataGridActivityDropDown : ComboBox
	{
		private DataTable activitiesDataTable = new DataTable(WorkFlowActivity.WorkFlowActionTableName);

		//---------------------------------------------------------------------
		public DataGridActivityDropDown(int companyId, int workflowId, SqlConnection currentConnection)
		{
			
			this.DropDownStyle		= ComboBoxStyle.DropDownList;
			this.DrawMode			= System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.BackColor			= System.Drawing.SystemColors.Window;
			this.ForeColor			= System.Drawing.SystemColors.WindowText;
			this.ItemHeight			= this.Font.Height + 2;
			this.IntegralHeight		= true;
			this.MaxDropDownItems	= 5;
			LoadAllActivities(companyId, workflowId, currentConnection);			
			
		}

		//---------------------------------------------------------------------
		public string FindActivityName(int activityIdToFind)
		{
			string currentValueName = string.Empty;
			for ( int i = 0; i < activitiesDataTable.Rows.Count; i++)
			{
				int activityId = (int)activitiesDataTable.Rows[i][WorkFlowActivity.ActivityIdColumnName];
				if (activityId == activityIdToFind)
				{
					currentValueName = (string) activitiesDataTable.Rows[i][WorkFlowActivity.ActivityNameColumnName];
					break;
				}
			}
			return currentValueName;
		}

		//---------------------------------------------------------------------
		protected override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
		{
			if (e.Index == -1) 
				return;

			string currentValueName = string.Empty;
			object currentValueId = Items[e.Index];
			if (currentValueId != null)
				currentValueName = this.FindActivityName((int)currentValueId);
			
						
			SolidBrush drawStringBrush = new SolidBrush(Color.Black);
	
			e.Graphics.DrawString(currentValueName, this.Font, drawStringBrush, e.Bounds.X +  2, e.Bounds.Y);

			drawStringBrush.Dispose(); 
			
			base.OnDrawItem(e);
		}

		//---------------------------------------------------------------------
		public void LoadAllActivities(int companyId, int workflowId, SqlConnection currentConnection)
		{
			this.Items.Clear();

			activitiesDataTable.Rows.Clear();
			WorkFlowActivity activities = new WorkFlowActivity(companyId, workflowId);
			SqlDataAdapter selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowActivity.GetSelectAllActivitiesOrderedByNameQuery(companyId, workflowId), currentConnection);
			selectAllSqlDataAdapter.Fill(activitiesDataTable);

			for (int i = 0; i < activitiesDataTable.Rows.Count; i++)
				this.Items.Add(activitiesDataTable.Rows[i][WorkFlowActivity.ActivityIdColumnName]);

			//mi posiziono sul primo elementi
			this.SelectedIndex = 0;
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

		
	}

	/// =======================================================================
	public class DataGridActivityDropDownColumn : DataGridColumnStyle
	{
		//---------------------------------------------------------------------
		private DataGridActivityDropDown dataGridActivityDropDown	= null; //= new DataGridActivityDropDown();
		private SqlConnection		currentConnection	= null;
		private string               oldVal				= new string(string.Empty.ToCharArray());
		private bool                 inEdit				= false;
		private const int dropdownDefaultWidth			= 132;
		
		
		//---------------------------------------------------------------------
		public SqlConnection CurrentConnection { get { return currentConnection; } set { currentConnection = value; }}

		//---------------------------------------------------------------------
		public DataGridActivityDropDownColumn(System.Drawing.Font aFont, int companyId, int workflowId, SqlConnection currentConnection): base()
		{
			dataGridActivityDropDown = new DataGridActivityDropDown(companyId, workflowId, currentConnection);
			dataGridActivityDropDown.Visible       = false;
			dataGridActivityDropDown.DropDownWidth = dropdownDefaultWidth;
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
			int currentActivityId = 0;
			object valueIntoCell = GetColumnValueAtRow(source, rowNumber);
			if (valueIntoCell != null && valueIntoCell != System.DBNull.Value)
				currentActivityId = (int)valueIntoCell;

			if (cellIsVisible)
			{
				dataGridActivityDropDown.Bounds = new Rectangle(bounds.X + 2, bounds.Y + 2, bounds.Width - 4, bounds.Height - 4);
				dataGridActivityDropDown.SelectedItem = currentActivityId;
				dataGridActivityDropDown.Visible = true;
				
			}
			else
			{
				dataGridActivityDropDown.SelectedItem = currentActivityId;
				dataGridActivityDropDown.Visible = false;

			}

			if (instantText != null)
				dataGridActivityDropDown.SelectedIndex = dataGridActivityDropDown.FindStringExact(instantText);
			else
				dataGridActivityDropDown.SelectedIndex = dataGridActivityDropDown.GetItemIndexFromObject(GetColumnValueAtRow(source, rowNumber));

			
			if (dataGridActivityDropDown.Visible)
				DataGridTableStyle.DataGrid.Invalidate(bounds);

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
				object Value = dataGridActivityDropDown.SelectedItem;
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
			if(dataGridActivityDropDown.Focused)
			{
				this.DataGridTableStyle.DataGrid.Focus();
			}
			dataGridActivityDropDown.Visible = false;
		}

		//---------------------------------------------------------------------
		private void RollBack()
		{
			dataGridActivityDropDown.Text = oldVal;
			if (oldVal == string.Empty)
				dataGridActivityDropDown.Text = "seleziona una attivita'";

		}

		//---------------------------------------------------------------------
		protected override int GetMinimumHeight()
		{
			return dataGridActivityDropDown.PreferredHeight + 4;
		}

		//---------------------------------------------------------------------
		protected override int GetPreferredHeight(Graphics g ,object objectValue)
		{
			return dataGridActivityDropDown.PreferredHeight + 4;
		}

		//---------------------------------------------------------------------
		protected override Size GetPreferredSize(Graphics g, object objectValue)
		{
			return new Size(dataGridActivityDropDown.Width + 4, dataGridActivityDropDown.Height + 4);
		}


		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum) 
		{
			Paint(g, bounds, source, rowNum, false);
		}


		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum,	bool alignToRight) 
		{
			Paint(g, bounds, source, rowNum, Brushes.Red, Brushes.Blue, alignToRight);
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

			object valueObject = GetColumnValueAtRow(source, rowNumber);
			Rectangle rect = bounds;
			g.FillRectangle(backBrush, rect);
			rect.Offset(0, 2);
			rect.Height -= 2;
			if ((valueObject == System.DBNull.Value) || (valueObject == null))
				return;
			int objectInt = (int)valueObject;
			string objectName = dataGridActivityDropDown.FindActivityName(objectInt);
			g.DrawString(objectName, dataGridActivityDropDown.Font, foreBrush, bounds.X + 2, bounds .Y);
			
		}

		

		//---------------------------------------------------------------------
		protected override void SetDataGridInColumn(DataGrid value) 
		{
			base.SetDataGridInColumn(value);
			if (dataGridActivityDropDown.Parent != null) 
			{
				dataGridActivityDropDown.Parent.Controls.Remove 
					(dataGridActivityDropDown);
			}
			if (value != null) 
			{
				value.Controls.Add(dataGridActivityDropDown);
			}
		}
	}

	//=========================================================================
	public class ActivityRow
	{
		private int			activityId			= -1;
		private string		activityName		= string.Empty;
		private string		activityDescription = string.Empty;

		//---------------------------------------------------------------------
		public ActivityRow(int aActivityId, string aActivityName, string aActivityDescription)
		{
			activityId			= aActivityId;
			activityName		= aActivityName;
			activityDescription = aActivityDescription;
		}

		//---------------------------------------------------------------------
		public ActivityRow(DataRow activityCurrentRow)
		{
			activityId			= (int)		activityCurrentRow[WorkFlowActivity.ActivityIdColumnName];
			activityName		= (string)	activityCurrentRow[WorkFlowActivity.ActivityNameColumnName];
			activityDescription = (string)	activityCurrentRow[WorkFlowActivity.ActivityDescriptionColumnName];
		}

		//---------------------------------------------------------------------
		public  int ActivityId				{ get { return activityId; } }
		//---------------------------------------------------------------------
		public string ActivityName			{ get { return activityName; } }
		//---------------------------------------------------------------------
		public string ActivityDescription	{ get { return activityDescription; } }
		
	}
}
