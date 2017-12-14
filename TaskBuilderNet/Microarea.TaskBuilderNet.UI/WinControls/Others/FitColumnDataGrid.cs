using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	//=========================================================================
	/// <summary>
	/// Summary description for FitColumnDataGrid.
	/// </summary>
	public partial class FitColumnDataGrid : System.Windows.Forms.DataGrid
	{	
		public delegate void RowOrColumnChangedEventHandler(object sender, int rowIndex);
		public event RowOrColumnChangedEventHandler CurrentRowChanged;

		private System.Windows.Forms.ToolTip toolTip = null;

		#region FitColumnDataGridTableStyle class

		//=========================================================================
		/// <summary>
		/// Summary description for FitColumnDataGridTableStyle.
		/// </summary>
		public class FitColumnDataGridTableStyle : System.Windows.Forms.DataGridTableStyle
		{
			private string fitColumnMappingName = String.Empty;
			private int minimumFitColumnWidth = 0;

			internal class ColumnToolTipInfo
			{
				private string columnMappingName = String.Empty;
				private string tootipText = String.Empty;

				internal string ColumnMappingName { get { return columnMappingName; } }
				internal string TootipText { get { return tootipText; } set { tootipText = value; } }

				internal ColumnToolTipInfo(string aMappingName, string aTootipText)
				{
					columnMappingName = aMappingName;
					tootipText = aTootipText;
				}
			}
			private ArrayList columnsToolTips = null;

			//---------------------------------------------------------------------------
			public FitColumnDataGridTableStyle()
			{
			}

			//---------------------------------------------------------------------------
			public FitColumnDataGridTableStyle(string aColumnMappingName, int aMinimumFitColumnWidth)
			{
				fitColumnMappingName = aColumnMappingName;
				minimumFitColumnWidth = aMinimumFitColumnWidth;
			}

			//---------------------------------------------------------------------------
			public FitColumnDataGridTableStyle(string aColumnMappingName):this(aColumnMappingName, 0)
			{
			}

            //---------------------------------------------------------------------------
			public string FitColumnMappingName { get { return fitColumnMappingName; } }
			//---------------------------------------------------------------------------
			public int MinimumFitColumnWidth { get { return minimumFitColumnWidth; } }

			//--------------------------------------------------------------------------------------------------------------------------------
			private void GridColumn_WidthChanged(object sender, System.EventArgs e)
			{
				if (this.DataGrid != null && (this.DataGrid is FitColumnDataGrid))
					((FitColumnDataGrid)this.DataGrid).AdjustColumnsWidth();
			}

			//---------------------------------------------------------------------------
			public int AddFitColumnStyle(System.Windows.Forms.DataGridColumnStyle aColumnStyle, string aToolTipText)
			{
				if (aColumnStyle == null)
					return -1;

				if 
					(
						fitColumnMappingName == null ||
						fitColumnMappingName.Length == 0 ||
						String.Compare(fitColumnMappingName, aColumnStyle.MappingName) != 0 
					)
					aColumnStyle.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);

				int addedStyleIndex = this.GridColumnStyles.Add(aColumnStyle); 
				if (addedStyleIndex >= 0 && aToolTipText != null && aToolTipText.Trim().Length > 0)
					SetColumnToolTip(aColumnStyle.MappingName, aToolTipText);

				return addedStyleIndex; 
			}
			
			//---------------------------------------------------------------------------
			public int AddFitColumnStyle(System.Windows.Forms.DataGridColumnStyle aColumnStyle)
			{
				return AddFitColumnStyle(aColumnStyle, null);
			}
			
			//---------------------------------------------------------------------------
			public void AddFitColumnStylesRange(System.Windows.Forms.DataGridColumnStyle[] aColumnStylesArray)
			{
				if (aColumnStylesArray == null || aColumnStylesArray.Length == 0)
					return;

				foreach(DataGridColumnStyle aColumnStyle in aColumnStylesArray)
				{
					if 
						(
						fitColumnMappingName == null ||
						fitColumnMappingName.Length == 0 ||
						String.Compare(fitColumnMappingName, aColumnStyle.MappingName) != 0 
						)
						aColumnStyle.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);
				}

				this.GridColumnStyles.AddRange(aColumnStylesArray);
			}
		
			//---------------------------------------------------------------------------
			public void SetColumnToolTip(string aColumnStyleMappingName, string aToolTipText)
			{
				if (aColumnStyleMappingName == null || aColumnStyleMappingName.Trim().Length == 0)
					return;

				aColumnStyleMappingName = aColumnStyleMappingName.Trim();

				if (columnsToolTips != null)
				{
					if (columnsToolTips.Count > 0)
					{
						foreach(ColumnToolTipInfo aToolTipInfo in columnsToolTips)
						{
							if (String.Compare(aToolTipInfo.ColumnMappingName, aColumnStyleMappingName, true) == 0)
							{
								aToolTipInfo.TootipText = aToolTipText;
								return;
							}
						}
					}
				}
				else
					columnsToolTips = new ArrayList();
				
				columnsToolTips.Add(new ColumnToolTipInfo(aColumnStyleMappingName, aToolTipText));
			}

			//---------------------------------------------------------------------------
			public void SetColumnToolTip(DataGridColumnStyle aColumnStyle, string aToolTipText)
			{
				if 
					(
					aColumnStyle == null ||
					aColumnStyle.MappingName == null || 
					aColumnStyle.MappingName.Trim().Length == 0
					)
					return;
				
				SetColumnToolTip(aColumnStyle.MappingName, aToolTipText);
			}
			
			//---------------------------------------------------------------------------
			public string GetColumnToolTipText(string aColumnStyleMappingName)
			{
				if (columnsToolTips == null || columnsToolTips.Count == 0)
					return null;

				foreach(ColumnToolTipInfo aToolTipInfo in columnsToolTips)
				{
					if (String.Compare(aToolTipInfo.ColumnMappingName, aColumnStyleMappingName, true) == 0)
					{
						return aToolTipInfo.TootipText;
					}
				}
				return null;
			}

			//---------------------------------------------------------------------------
			public string GetColumnToolTipText(int aColumnStyleIndex)
			{
				if 
					(
					columnsToolTips == null ||
					columnsToolTips.Count == 0 ||
					this.GridColumnStyles == null || 
					this.GridColumnStyles.Count == 0 ||
					aColumnStyleIndex < 0 ||
					aColumnStyleIndex >= this.GridColumnStyles.Count
					)
					return null;

				return GetColumnToolTipText(this.GridColumnStyles[aColumnStyleIndex].MappingName);
			}
		}
		
		#endregion // FitColumnDataGridTableStyle class

		//---------------------------------------------------------------------------
		public FitColumnDataGrid()
		{
            InitializeComponent();

			this.HorizScrollBar.VisibleChanged += new System.EventHandler(this.HorizScrollBar_VisibleChanged);
			this.VertScrollBar.VisibleChanged += new System.EventHandler(this.VertScrollBar_VisibleChanged);
		}


		#region FitColumnDataGrid protected virtual methods

		//--------------------------------------------------------------------------------------------------------------------------------
		protected virtual void OnInitializeTableStyles()
		{
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		protected virtual void OnAdjustingColumnsWidth(ref bool forceResizing)
		{
		}
		
		#endregion // FitColumnDataGrid protected virtual methods

		#region FitColumnDataGrid overridden protected methods

		//---------------------------------------------------------------------------
		protected override void OnCreateControl()
		{	
			// Invoke base class implementation
			base.OnCreateControl();

			this.TableStyles.Clear();
			
			if (!this.DesignMode)
				InitializeTableStyles();
		}

		//---------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{	
			// Invoke base class implementation
			base.OnResize(e);

			AdjustColumnsWidth(true);
		}

		//---------------------------------------------------------------------------
		// Override the OnMouseDown event to select the whole row
		// when the user clicks anywhere on a row.
		protected override void OnMouseDown(MouseEventArgs e) 
		{
			int previouslySelectedRowIndex = this.CurrentCell.RowNumber;

			// Get the HitTestInfo to return the row and pass
			// that value to the IsSelected property of the DataGrid.
			DataGrid.HitTestInfo hit = this.HitTest(e.X, e.Y);
	
			if (hit.Type == DataGrid.HitTestType.Cell && hit.Row >= 0)
			{
				try
				{
					if (!IsSelected(hit.Row))
					{
						if 
							(
							previouslySelectedRowIndex >= 0 && 
							previouslySelectedRowIndex < this.RowsCount &&
							previouslySelectedRowIndex != hit.Row && 
							IsSelected(previouslySelectedRowIndex)
							)
							this.UnSelect(previouslySelectedRowIndex);

						this.Select(hit.Row);
					
						// If you are viewing a parent table, or a table with no child relations, then
						// the CurrentRowIndex property returns the zero-based index of the current row.
						// If you are viewing a child table, incrementing the CurrentRowIndex will 
						// cause the System.Windows.Forms.DataGrid to display the next set of records
						// in the child table that are linked to the parent table.
						if (IsViewingParentTable())
							this.CurrentRowIndex = hit.Row;
						else if (this.ListManager != null)
							this.ListManager.Position = hit.Row;

						if (CurrentRowChanged != null)
							CurrentRowChanged(this, previouslySelectedRowIndex);

						return;
					}
				}						
				catch(IndexOutOfRangeException)
				{
				}

			}
			
			// Invoke base class implementation
			base.OnMouseDown(e);			
		
			if (CurrentRowChanged != null && previouslySelectedRowIndex != this.CurrentCell.RowNumber)
				CurrentRowChanged(this, previouslySelectedRowIndex);
		}
		
		//---------------------------------------------------------------------
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (!SystemInformation.MouseWheelPresent)
				return;
			
			int previouslySelectedRowIndex = this.CurrentCell.RowNumber;

			int rowsToScroll = e.Delta * SystemInformation.MouseWheelScrollLines / 120;

			if (rowsToScroll != 0)
			{
				if (previouslySelectedRowIndex >= 0)
				{
					int newRowIndexToSel = Math.Max(0, Math.Min(previouslySelectedRowIndex - rowsToScroll, this.RowsCount - 1));
					if (newRowIndexToSel != previouslySelectedRowIndex)
					{
						try
						{
							if (IsSelected(previouslySelectedRowIndex))
								this.UnSelect(previouslySelectedRowIndex);

							this.Select(newRowIndexToSel);
							// If you are viewing a parent table, or a table with no child relations, then
							// the CurrentRowIndex property returns the zero-based index of the current row.
							// If you are viewing a child table, incrementing the CurrentRowIndex will 
							// cause the System.Windows.Forms.DataGrid to display the next set of records
							// in the child table that are linked to the parent table.
							if (IsViewingParentTable())
								this.CurrentRowIndex = newRowIndexToSel;
							else if (this.ListManager != null)
								this.ListManager.Position = newRowIndexToSel;

							if (CurrentRowChanged != null)
								CurrentRowChanged(this, previouslySelectedRowIndex);

							return;
						}
						catch(IndexOutOfRangeException)
						{
						}
					}
				}
			}
			
			// Invoke base class implementation
			base.OnMouseWheel(e);
	
			if (CurrentRowChanged != null && previouslySelectedRowIndex != this.CurrentCell.RowNumber)
				CurrentRowChanged(this, previouslySelectedRowIndex);
		}

		//---------------------------------------------------------------------------
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			DataGridTableStyle dataGridTableStyle = GetCurrentTableStyle();
			if (dataGridTableStyle != null && (dataGridTableStyle is FitColumnDataGridTableStyle))
			{
				DataGrid.HitTestInfo hitTest = this.HitTest(e.X, e.Y);
				if (hitTest.Column >= this.FirstVisibleColumn && hitTest.Column < (this.FirstVisibleColumn + this.VisibleColumnCount))
				{		
					string toolTipText = ((FitColumnDataGridTableStyle)dataGridTableStyle).GetColumnToolTipText(hitTest.Column);
					if (toolTipText != null && toolTipText.Length > 0)
					{
						DataGridColumnStyle columnStyle = dataGridTableStyle.GridColumnStyles[hitTest.Column];
						if (columnStyle != null)
						{
							if (toolTip == null)
							{
								toolTip = new ToolTip();
								toolTip.InitialDelay = 500; 
							}

							toolTip.SetToolTip(this, toolTipText);

							return;
						}
					}
					
					if (toolTip != null)
						toolTip.SetToolTip(this, String.Empty);
				}
			}
		}

		//---------------------------------------------------------------------------
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)	
		{
			int previouslySelectedRowIndex = this.CurrentCell.RowNumber;

			int newRowIndexToSel = -1;
			if (previouslySelectedRowIndex >= 0)
			{
				if (keyData == Keys.Home) // Move to the first cell in the current row
				{
					newRowIndexToSel = 0;
				}
				else if (keyData == Keys.End) // Move to the last cell in the current row
				{
					newRowIndexToSel = this.RowsCount - 1;
				}
				else
				{
					int rowsToScroll = 0;
					switch(keyData)
					{
						case Keys.Home: // Move to the first cell in the current row
				
						case Keys.End: // Move to the last cell in the current row
				
						case Keys.Down:
							rowsToScroll = 1;
							break;

						case Keys.Up:
							rowsToScroll = -1;
							break;

						case Keys.PageDown:
							rowsToScroll = this.VisibleRowCount;
							break;

						case Keys.PageUp:
							rowsToScroll = -this.VisibleRowCount;
							break;

						case (Keys.Tab):
							// If focus is on the last child link, move to the first cell of the next row.
							if (this.CurrentCell.ColumnNumber == this.ColumnsCount -1)
								rowsToScroll = 1;
							break;

						case (Keys.Shift |Keys.Tab):
							// If focus is on the first child link, move to the last cell of the previous row.
							if (this.CurrentCell.ColumnNumber == 0)
								rowsToScroll = -1;
							break;
					}

					if (rowsToScroll != 0)
						newRowIndexToSel = Math.Max(0, Math.Min(previouslySelectedRowIndex + rowsToScroll, this.RowsCount - 1));
				}
			}

			if (newRowIndexToSel >= 0)
			{
				if (newRowIndexToSel != previouslySelectedRowIndex)
				{
					try
					{
						if (IsSelected(previouslySelectedRowIndex))
							this.UnSelect(previouslySelectedRowIndex);

						this.Select(newRowIndexToSel);

						// If you are viewing a parent table, or a table with no child relations, then
						// the CurrentRowIndex property returns the zero-based index of the current row.
						// If you are viewing a child table, incrementing the CurrentRowIndex will 
						// cause the System.Windows.Forms.DataGrid to display the next set of records
						// in the child table that are linked to the parent table.
						if (IsViewingParentTable())
							this.CurrentRowIndex = newRowIndexToSel;
						else if (this.ListManager != null)
							this.ListManager.Position = newRowIndexToSel;
				
						if (CurrentRowChanged != null)
							CurrentRowChanged(this, previouslySelectedRowIndex);
					}						
					catch(IndexOutOfRangeException)
					{
					}
				}
				
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		#endregion

		#region FitColumnDataGrid private methods

		//--------------------------------------------------------------------------------------------------------------------------------
		private void InitializeTableStyles()
		{
			this.TableStyles.Clear();

			if (this.DesignMode)
				return;

			OnInitializeTableStyles();

			AdjustColumnsWidth();
		}
		
		//---------------------------------------------------------------------------
		private bool IsViewingParentTable()
		{
			if (
				this.DataSource == null || 
				!(this.DataSource is DataTable)
				)
				return false;

			if (((DataTable)this.DataSource).ChildRelations == null || ((DataTable)this.DataSource).ChildRelations.Count == 0)
				return true;

			DataTable currentDataTable = this.CurrentDataTable;
			if (currentDataTable != null)
				return (String.Compare(currentDataTable.TableName, ((DataTable)this.DataSource).TableName) == 0);

			return false;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private DataGridTableStyle GetCurrentTableStyle()
		{
			if (this.TableStyles != null && this.TableStyles.Count > 0)
			{
				DataTable currentDataTable = this.CurrentDataTable;
				if (currentDataTable != null)
				{
					foreach(DataGridTableStyle aTableStyle in this.TableStyles)
					{
						if (aTableStyle != null && String.Compare(aTableStyle.MappingName, currentDataTable.TableName) == 0)
							return aTableStyle;
					}
				}
			}

			return null;
		}
		
		#endregion // FitColumnDataGrid private methods
		
		#region FitColumnDataGrid protected methods

		//--------------------------------------------------------------------------------------------------------------------------------
		protected void AdjustColumnsWidth(bool forceResizing)
		{
			OnAdjustingColumnsWidth(ref forceResizing);

			if (this.TableStyles == null || this.TableStyles.Count == 0)
				return;

			bool columnsResized = false;
			int borderWidth = (this.BorderStyle == BorderStyle.Fixed3D) ? SystemInformation.Border3DSize.Width : SystemInformation.BorderSize.Width;

			foreach(DataGridTableStyle aTableStyle in this.TableStyles)
			{
				if (aTableStyle == null || !(aTableStyle is FitColumnDataGridTableStyle))
					continue;

				string fitColumnMappingName = ((FitColumnDataGridTableStyle)aTableStyle).FitColumnMappingName;
				if (fitColumnMappingName == null || fitColumnMappingName.Length == 0)
					continue;

				int colswidth = 0;
				if (aTableStyle.RowHeadersVisible)
					colswidth += aTableStyle.RowHeaderWidth;
				
				DataGridColumnStyle fitColumnStyle = null;
				for (int i = 0; i < aTableStyle.GridColumnStyles.Count; i++)
				{
					if (String.Compare(aTableStyle.GridColumnStyles[i].MappingName, fitColumnMappingName) == 0)
						fitColumnStyle = aTableStyle.GridColumnStyles[i];
					else
						colswidth += aTableStyle.GridColumnStyles[i].Width;
				}
			
				if (fitColumnStyle == null)
					continue;

				int newFitColumnWidth = this.DisplayRectangle.Width - (2* borderWidth) - colswidth;

				if (this.VertScrollBar.Visible)
					newFitColumnWidth -= this.VertScrollBar.Width;

				int newFitColumnStyleWidth = Math.Max
					(
					((FitColumnDataGridTableStyle)aTableStyle).MinimumFitColumnWidth, 
					newFitColumnWidth
					);
			
				if (forceResizing || newFitColumnStyleWidth > fitColumnStyle.Width)
				{
					fitColumnStyle.Width = newFitColumnStyleWidth;

					columnsResized = true;
				}
			}
			if (columnsResized)
				this.PerformLayout();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected void AdjustColumnsWidth()
		{
			AdjustColumnsWidth(false);
		}
				
		//--------------------------------------------------------------------------
		protected void EnsureCurrentRowIsVisible()
		{	
			if (!this.VertScrollBar.Visible)
				return;

			Rectangle currentViewArea = new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height);
			
			if (this.VertScrollBar.Visible)
				currentViewArea.Width -= this.VertScrollBar.Width;
			if (this.HorizScrollBar.Visible)
				currentViewArea.Height -= this.HorizScrollBar.Height;
			
			try
			{
				Rectangle currentCellBounds = this.GetCurrentCellBounds();
				if (currentViewArea.Contains(currentCellBounds))
					return;

				if (currentCellBounds.Left < currentViewArea.Left || currentCellBounds.Right > currentViewArea.Right)
					ScrollToColumn(this.CurrentCell.ColumnNumber);

				if (currentCellBounds.Top < currentViewArea.Top || currentCellBounds.Bottom > currentViewArea.Bottom)
					ScrollToRow(this.CurrentCell.RowNumber);
			}
			catch(Exception)
			{
				// Ho aggiunto questo catch per schiantamenti su GetCurrentCellBounds...
			}
		}
	
		//--------------------------------------------------------------------------
		protected void ScrollToRow(int rowNumber)
		{
			if (!this.VertScrollBar.Visible)
				return;

			ScrollEventType sEventType;

			if (rowNumber <= 0)
			{
				sEventType = ScrollEventType.First;
			}
			else if (rowNumber >= this.RowsCount)
			{
				sEventType = ScrollEventType.Last;
			}
			else
			{
				int difference = this.VertScrollBar.Value - rowNumber;    
				// < 0 -- go down ; > 0	-- go up
				// It is not confirmed that what will the ScrollEventType do to the scrolling in this case.
				// But I find the right ScrollEventType anyway because it will break encapsulation otherwise.
				if (difference > 0)
				{
					if (difference < this.VertScrollBar.SmallChange)
						sEventType = ScrollEventType.SmallDecrement;
					else
						sEventType = ScrollEventType.LargeDecrement;
				}
				else
				{
					difference = - difference;
					if (difference < this.VertScrollBar.LargeChange)
						sEventType = ScrollEventType.SmallIncrement;
					else
						sEventType = ScrollEventType.LargeIncrement;
				}
			}
			
			GridVScrolled(this, new ScrollEventArgs(sEventType, rowNumber));
		}

		//--------------------------------------------------------------------------
		protected void ScrollToColumn(int columnNumber)
		{
			if (!this.HorizScrollBar.Visible)
				return;

			ScrollEventType sEventType;

			if (columnNumber <= 0)
			{
				sEventType = ScrollEventType.First;
			}
			else if (columnNumber >= this.ColumnsCount)
			{
				sEventType = ScrollEventType.Last;
			}
			else
			{
				int difference = this.HorizScrollBar.Value - columnNumber;    
				// < 0 -- go right ; > 0 -- go left
				// It is not confirmed that what will the ScrollEventType do to the scrolling in this case.
				// But I find the right ScrollEventType anyway because it will break encapsulation otherwise.
				if (difference > 0)
				{
					if (difference < this.HorizScrollBar.SmallChange)
						sEventType = ScrollEventType.SmallDecrement;
					else
						sEventType = ScrollEventType.LargeDecrement;
				}
				else
				{
					difference = - difference;
					if (difference < this.HorizScrollBar.LargeChange)
						sEventType = ScrollEventType.SmallIncrement;
					else
						sEventType = ScrollEventType.LargeIncrement;
				}
			}
			
			GridHScrolled(this, new ScrollEventArgs(sEventType, columnNumber));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected int GetDataGridTableStyleColumnIndex(string aColumnMappingName)
		{
			if (aColumnMappingName == null || aColumnMappingName == String.Empty)
				return -1;

			DataGridTableStyle dataGridTableStyle = GetCurrentTableStyle();
			if (dataGridTableStyle == null)
				return -1;
			
			for (int i = 0; i < dataGridTableStyle.GridColumnStyles.Count; i++)
			{
				if (String.Compare(dataGridTableStyle.GridColumnStyles[i].MappingName, aColumnMappingName) == 0)
					return i;
			}
			return -1;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected void FireCurrentRowChangedEvent(int previouslySelectedRowIndex)
		{
			if (CurrentRowChanged != null)
				CurrentRowChanged(this, previouslySelectedRowIndex);
		}
		
		#endregion // FitColumnDataGrid protected methods

		#region FitColumnDataGrid event handlers

		//--------------------------------------------------------------------------
		private void VertScrollBar_VisibleChanged(object sender, EventArgs e)
		{	
			if (sender != this.VertScrollBar)
				return;
			
			AdjustColumnsWidth(true);

			this.Refresh();
		}
		
		//--------------------------------------------------------------------------
		private void HorizScrollBar_VisibleChanged(object sender, EventArgs e)
		{	
			if (sender != this.HorizScrollBar)
				return;

			Application.DoEvents();

			EnsureCurrentRowIsVisible();
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

		#endregion

		#region FitColumnDataGrid public properties

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
							int previouslySelectedRowIndex = this.CurrentRowIndex;
							
							if (previouslySelectedRowIndex != rowIndex)
							{
								this.CurrentRowIndex = rowIndex;
				
								if (CurrentRowChanged != null)
									CurrentRowChanged(this, previouslySelectedRowIndex);
							}
							return;
						}
					}
				}
				if (this.DataSource != null && this.ListManager != null)
					this.CurrentRowIndex = -1;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public DataTable CurrentDataTable
		{
			get
			{
				try
				{
					object currentListManagerObject = (this.ListManager != null) ? this.ListManager.Current : null;
					if 
						(
						currentListManagerObject != null && 
						(currentListManagerObject is System.Data.DataRowView) &&
						((System.Data.DataRowView)currentListManagerObject).DataView != null
						)
						return ((System.Data.DataRowView)currentListManagerObject).DataView.Table;
				}
				catch(Exception)
				{
				}

				if (
					this.DataSource == null || 
					!(this.DataSource is DataTable)
					)
					return null;
				
				return (DataTable)this.DataSource;
			}
		}
			
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public int RowsCount
		{
			get
			{
				try
				{
					object currentListManagerObject = (this.ListManager != null) ? this.ListManager.Current : null;
					if 
						(
						currentListManagerObject != null && 
						(currentListManagerObject is System.Data.DataRowView) &&
						((System.Data.DataRowView)currentListManagerObject).DataView != null
						)
						return ((System.Data.DataRowView)currentListManagerObject).DataView.Count;
				}
				catch(Exception)
				{
				}
				
				DataTable currentDataTable = this.CurrentDataTable;
				if (currentDataTable == null)
					return 0;
			
				return currentDataTable.Rows.Count;
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public int ColumnsCount
		{
			get
			{
				DataGridTableStyle currentTableStyle = this.GetCurrentTableStyle();
				if (currentTableStyle != null)
					return currentTableStyle.GridColumnStyles.Count;
			
				DataTable currentDataTable = this.CurrentDataTable;
				if (currentDataTable == null)
					return 0;
				
				return currentDataTable.Rows.Count;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public new System.Windows.Forms.GridTableStylesCollection TableStyles
		{
			get { return base.TableStyles; }
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public new object DataSource
		{
			get
			{
				return base.DataSource;
			}
			
			set
			{
				if (this.DataSource == value)
					return;
				
				if (this.DataSource != null)
				{
					if (this.DataSource is DataTable)
						((DataTable)this.DataSource).Dispose();
					base.DataSource = null;
				}
				
				if (value == null)
				{
					base.DataSource = null;
					return;
				}

				if (!(value is DataTable))
					return;

				DataTable aDataTable = (DataTable)value;

				base.DataSource = aDataTable;

				aDataTable.DefaultView.ListChanged += new System.ComponentModel.ListChangedEventHandler(DataSource_DefaultViewListChanged);
					
				AdjustColumnsWidth();
		
				if (CurrentRowChanged != null && this.CurrentRowIndex >= 0)
					CurrentRowChanged(this, -1);
			}
		}
	
		//---------------------------------------------------------------------
		public new bool ReadOnly 
		{ 
			get { return base.ReadOnly; } 
			set 
			{ 
				base.ReadOnly = value;
						
				if (this.TableStyles != null && this.TableStyles.Count > 0)
				{
					foreach(DataGridTableStyle aTableStyle in this.TableStyles)
					{
						if (aTableStyle != null && aTableStyle.GridColumnStyles != null && aTableStyle.GridColumnStyles.Count > 0)
						{
							foreach (DataGridColumnStyle columnStyle in aTableStyle.GridColumnStyles)
							{
								columnStyle.ReadOnly = value;
							}
						}
					}
				}
			}
		}
		
		#endregion
		
		#region FitColumnDataGrid public methods

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveCurrentRow()
		{
			if (ReadOnly)
				return;

			DataTable currentDataTable = this.CurrentDataTable;
			if (currentDataTable == null || this.CurrentRow == null)
				return;

			int previouslySelectedRowIndex = this.CurrentRowIndex;

			currentDataTable.Rows.Remove(this.CurrentRow);
						
			((DataTable)this.DataSource).Rows.Remove(this.CurrentRow);
			
			this.CurrentRowIndex = -1;
	
			AdjustColumnsWidth();

			if (CurrentRowChanged != null && previouslySelectedRowIndex != this.CurrentRowIndex)
				CurrentRowChanged(this, previouslySelectedRowIndex);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public DataRow GetRowAt(int rowIndex)
		{
			if (this.DataSource == null || !(this.DataSource is DataTable))
				return null;

			if (rowIndex < 0 || rowIndex >= ((DataTable)this.DataSource).Rows.Count)
				throw new ArgumentOutOfRangeException();

			return ((DataTable)this.DataSource).Rows[rowIndex];
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			this.DataSource = null;

			AdjustColumnsWidth();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int GetListManagerPosition()
		{
			if (this.ListManager == null)
				return -1;

			return this.ListManager.Position;
		}

		#endregion
	}
}
