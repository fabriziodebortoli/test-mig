using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Microarea.Library.WorkFlowWindowsControls
{
	/// <summary>
	/// Summary description for TBWorkFlowDataGrid.
	/// </summary>
	public class TBWorkFlowDataGrid : System.Windows.Forms.DataGrid
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public int MinimumDataGridCodeColumnWidth		= 150;
		public int MinimumDataGridStringColumnWidth		= 300;
		public int MinimumDataGridBoolColumnWidth		= 56;
		public int MinimumDropdownDefaultWidth			= 100;
		
		public ScrollBar CurrentVertScrollBar { get { return this.VertScrollBar; }}


		public TBWorkFlowDataGrid()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			

		}

		//---------------------------------------------------------------------------
		protected override void OnCreateControl()
		{	
			// Invoke base class implementation
			base.OnCreateControl();
			
			this.VertScrollBar.VisibleChanged += new System.EventHandler(this.VertScrollBar_VisibleChanged);
		}

		//---------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{	
			// Invoke base class implementation
			base.OnResize(e);

			AdjustLastColumnWidth();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AdjustLastColumnWidth()
		{
			this.Refresh();
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e) 
		{
			int previouslySelectedRowIndex = this.CurrentRowIndex;

			// Get the HitTestInfo to return the row and pass
			// that value to the IsSelected property of the DataGrid.
			DataGrid.HitTestInfo hit = this.HitTest(e.X, e.Y);
			if (hit.Type != DataGrid.HitTestType.Cell || hit.Row < 0)
			{
				// Invoke base class implementation
				base.OnMouseDown(e);
				return;
			}
				
			if (!IsSelected(hit.Row))
			{
				if 
					(
					previouslySelectedRowIndex >= 0 && 
					previouslySelectedRowIndex != hit.Row && 
					IsSelected(previouslySelectedRowIndex)
					)
					this.UnSelect(previouslySelectedRowIndex);

				this.Select(hit.Row);
				this.CurrentRowIndex = hit.Row;
				base.OnMouseDown(e);
				return;
			}
		}
			
		//--------------------------------------------------------------------------
		private void VertScrollBar_VisibleChanged(object sender, EventArgs e)
		{	
			if (sender != this.VertScrollBar)
				return;
			
			AdjustLastColumnWidth();

			this.Refresh();
		}
		
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void DataSource_DefaultViewListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
		{
			if (
				this.DataSource == null || 
				!(this.DataSource is DataTable) || 
				((DataTable)this.DataSource).DefaultView == null ||
				sender != ((DataTable)this.DataSource).DefaultView
				)
				return;
			
			if (e.ListChangedType == ListChangedType.Reset)
			{
				if 
					(
					this.CurrentRowIndex >= 0 &&
					IsSelected(this.CurrentRowIndex)
					)
					this.UnSelect(this.CurrentRowIndex);
				return;
			}

			if (
				e.ListChangedType == ListChangedType.ItemMoved && 
				e.OldIndex == this.CurrentRowIndex
				)
				this.CurrentRowIndex = e.NewIndex;
		}

		
		
		
		

		
		
		/*
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetDataRowFieldsFromTask(WorkFlowIstance aWorkFlow, ref System.Data.DataRow aTaskTableRow)
		{
			aTask.SetDataRowFields(ref aTaskTableRow);
		}

		
		//--------------------------------------------------------------------------------------------------------------------------------
		public System.Data.DataRow GetDataRowFromTask(WorkFlowIstance aWorkFlowToSearch)
		{
			if (aTaskToSearch == null || this.DataSource == null)
				return null;

			DataTable dataGridTable = (DataTable)this.DataSource;
			foreach(System.Data.DataRow dataRow in dataGridTable.Rows)
			{
				System.Guid guid = new Guid(dataRow[ScheduledTask.IdColumnName].ToString());
				if (guid.Equals(aTaskToSearch.Id))
					return dataRow;
			}

			return null;
		}*/

		/*
		//--------------------------------------------------------------------------------------------------------------------------------
		public System.Data.DataRow AddTaskRow(ScheduledTask aTaskToAdd)
		{
			if (aTaskToAdd == null || this.DataSource == null)
				return null;

			DataTable dataGridTable = (DataTable)this.DataSource;

			System.Data.DataRow newRow = dataGridTable.NewRow();

			SetDataRowFieldsFromTask(aTaskToAdd, ref newRow);

			dataGridTable.Rows.Add(newRow);
			
			AdjustLastColumnWidth();

			return newRow;
		}*/

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveCurrentRow()
		{
			if (this.DataSource == null || !(this.DataSource is DataTable) || this.CurrentRow == null)
				return;

			((DataTable)this.DataSource).Rows.Remove(this.CurrentRow);
			
			AdjustLastColumnWidth();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			this.DataSource = null;

			AdjustLastColumnWidth();
		}
		
		/*
		//--------------------------------------------------------------------------------------------------------------------------------
		public void UpdateCurrentRow(ScheduledTask aTask)
		{
			DataRow currentRow = this.CurrentRow;
			SetDataRowFieldsFromTask(aTask, ref currentRow);
		}*/

		
		
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public System.Data.DataRow CurrentRow
		{
			get
			{
				if (
					this.DataSource == null || 
					!(this.DataSource is DataTable) || 
					((DataTable)this.DataSource).DefaultView == null || 
					this.CurrentRowIndex < 0 ||
					this.CurrentRowIndex >= ((DataTable)this.DataSource).DefaultView.Count
					)
					return null;
			
				return ((DataTable)this.DataSource).DefaultView[this.CurrentRowIndex].Row;
			}
				
			set
			{
				if (
					this.DataSource == null || 
					!(this.DataSource is DataTable) || 
					((DataTable)this.DataSource).DefaultView == null
					)
					return;
			
				if (value != null)
				{
					for (int rowIndex = 0; rowIndex < ((DataTable)this.DataSource).DefaultView.Count; rowIndex++)
					{
						if (((DataTable)this.DataSource).DefaultView[rowIndex].Row == value)
						{
							this.CurrentRowIndex = rowIndex;
							return;
						}
					}
				}
			
				if (this.DataSource != null)
					this.CurrentRowIndex = -1;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public new object DataSource
		{
			get
			{
				return base.DataSource;
			}
			
			set
			{
				if (value == null)
				{
					base.DataSource = null;
					return;
				}
				if (!(value is DataTable))
					return;

				DataTable aDataTable = (DataTable)value;

				base.DataSource = aDataTable;

				aDataTable.DefaultView.AllowNew = false;
				aDataTable.DefaultView.AllowEdit = false;
				aDataTable.DefaultView.AllowDelete = false;

				aDataTable.DefaultView.ListChanged += new System.ComponentModel.ListChangedEventHandler(DataSource_DefaultViewListChanged);
					
				AdjustLastColumnWidth();
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
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
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			// 
			// TBWorkFlowDataGrid
			// 
			this.AlternatingBackColor = System.Drawing.Color.Lavender;
			this.BackColor = System.Drawing.Color.Lavender;
			this.BackgroundColor = System.Drawing.Color.Lavender;
			this.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
			this.CaptionFont = new System.Drawing.Font("Verdana", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
			this.CaptionForeColor = System.Drawing.Color.Navy;
			this.Font = new System.Drawing.Font("Verdana", 8.25F);
			this.ForeColor = System.Drawing.Color.Navy;
			this.GridLineColor = System.Drawing.Color.LightSteelBlue;
			this.HeaderBackColor = System.Drawing.Color.LightSteelBlue;
			this.HeaderForeColor = System.Drawing.Color.DarkBlue;
			this.ParentRowsBackColor = System.Drawing.Color.Lavender;
			this.ParentRowsForeColor = System.Drawing.Color.Navy;
			this.SelectionBackColor = System.Drawing.Color.CornflowerBlue;
			this.SelectionForeColor = System.Drawing.Color.AliceBlue;
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();

		}
		#endregion

		
	}
}
