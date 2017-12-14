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
	/// Summary description for TBViewWorkFlow.
	/// </summary>
	public class TBViewWorkFlow : System.Windows.Forms.UserControl
	{
		private Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid WorkFlowsDataGrid;
		private System.ComponentModel.IContainer components;
		private bool isConnectionOpen = false;
		private SqlConnection		currentConnection = null;
		private string				currentConnectionString = string.Empty;
		private System.Windows.Forms.ToolTip TBWorkFlowControlToolTip;
		private System.Windows.Forms.ContextMenu WorkFlowContextMenu;
		private int			        companyId = -1;

		public bool IsConnectionOpen { get { return isConnectionOpen; } set { isConnectionOpen = value; }}
		public string CurrentConnectionString { set { currentConnectionString = value; }}
		public SqlConnection CurrentConnection { set { currentConnection = value; }}
		public int CompanyId { set { companyId = value; }}
		public TBWorkFlowDataGrid WorkFlowGrid { get { return WorkFlowsDataGrid; }}
		public DataRow CurrentWorkFlowDataGridRow { get { return WorkFlowsDataGrid.CurrentRow; } }

		public delegate void AfterSelectedRow(object sender, int companyId, int workFlowId, int templateId);
		public event		 AfterSelectedRow OnViewSelectedRow;

		public delegate void DeleteSelectedRow(object sender, DataRow currentRow);
		public event		 DeleteSelectedRow OnDeleteSelectedRow;


		public TBViewWorkFlow()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			WorkFlowsDataGrid.CaptionText		= WorkFlowString.WorkFlowGridCaption;
			WorkFlowsDataGrid.VisibleChanged	+= new EventHandler(this.VertScrollBar_VisibleChanged);
			WorkFlowsDataGrid.ContextMenu		=  WorkFlowContextMenu;
			InitializeTableStyles();

		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void InitializeTableStyles()
		{
			WorkFlowsDataGrid.TableStyles.Clear();

			DataGridTableStyle dataGridWorkFlowStyle	= new DataGridTableStyle();
			dataGridWorkFlowStyle.DataGrid				= WorkFlowsDataGrid;
			dataGridWorkFlowStyle.MappingName			= WorkFlow.WorkFlowTableName;
			dataGridWorkFlowStyle.GridLineStyle			= DataGridLineStyle.Solid;
			dataGridWorkFlowStyle.RowHeadersVisible		= true;
			dataGridWorkFlowStyle.ColumnHeadersVisible	= true;
			dataGridWorkFlowStyle.HeaderFont			= new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			dataGridWorkFlowStyle.PreferredRowHeight	= dataGridWorkFlowStyle.HeaderFont.Height;
			dataGridWorkFlowStyle.PreferredColumnWidth	= 100;
			dataGridWorkFlowStyle.ReadOnly				= true;
			dataGridWorkFlowStyle.RowHeaderWidth		= 12;
			dataGridWorkFlowStyle.AlternatingBackColor	= WorkFlowsDataGrid.AlternatingBackColor;
			dataGridWorkFlowStyle.BackColor				= WorkFlowsDataGrid.BackColor;
			dataGridWorkFlowStyle.ForeColor				= WorkFlowsDataGrid.ForeColor;
			dataGridWorkFlowStyle.GridLineStyle			= WorkFlowsDataGrid.GridLineStyle;
			dataGridWorkFlowStyle.GridLineColor			= WorkFlowsDataGrid.GridLineColor;
			dataGridWorkFlowStyle.HeaderBackColor		= WorkFlowsDataGrid.HeaderBackColor;
			dataGridWorkFlowStyle.HeaderForeColor		= WorkFlowsDataGrid.HeaderForeColor;
			dataGridWorkFlowStyle.SelectionBackColor	= WorkFlowsDataGrid.SelectionBackColor;
			dataGridWorkFlowStyle.SelectionForeColor	= WorkFlowsDataGrid.SelectionForeColor;

			// 
			// dataGridWorkFlowNameTextBox
			// 
			WorkFlowTextBoxDataGridColumnStyle dataGridWorkFlowNameTextBox = new WorkFlowTextBoxDataGridColumnStyle();
			dataGridWorkFlowNameTextBox.Alignment = HorizontalAlignment.Left;
			dataGridWorkFlowNameTextBox.Format = "";
			dataGridWorkFlowNameTextBox.FormatInfo = null;
			dataGridWorkFlowNameTextBox.HeaderText = WorkFlowString.DataGridWorkFlowNameColumnHeaderText;
			dataGridWorkFlowNameTextBox.MappingName = WorkFlow.WorkFlowNameColumnName;
			dataGridWorkFlowNameTextBox.NullText = string.Empty;
			dataGridWorkFlowNameTextBox.ReadOnly = true;
			dataGridWorkFlowNameTextBox.Width = WorkFlowsDataGrid.MinimumDataGridCodeColumnWidth;
			dataGridWorkFlowNameTextBox.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);
			
			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridWorkFlowNameTextBox);

			// 
			// dataGridDescriptionTextBox
			// 
			WorkFlowTextBoxDataGridColumnStyle dataGridWorkFlowDescriptionTextBox = new WorkFlowTextBoxDataGridColumnStyle();
			dataGridWorkFlowDescriptionTextBox.Alignment = HorizontalAlignment.Left;
			dataGridWorkFlowDescriptionTextBox.Format = "";
			dataGridWorkFlowDescriptionTextBox.FormatInfo = null;
			dataGridWorkFlowDescriptionTextBox.HeaderText = WorkFlowString.DataGridWorkFlowDescColumnHeaderText;
			dataGridWorkFlowDescriptionTextBox.MappingName = WorkFlow.WorkFlowDescColumnName;
			dataGridWorkFlowDescriptionTextBox.NullText = string.Empty;
			dataGridWorkFlowDescriptionTextBox.ReadOnly = true;
			dataGridWorkFlowDescriptionTextBox.Width = WorkFlowsDataGrid.MinimumDataGridCodeColumnWidth;
			dataGridWorkFlowDescriptionTextBox.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);

			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridWorkFlowDescriptionTextBox);
			

			WorkFlowsDataGrid.TableStyles.Add(dataGridWorkFlowStyle);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void GridColumn_WidthChanged(object sender, System.EventArgs e)
		{
			AdjustLastColumnWidth();
		}


		//--------------------------------------------------------------------------------------------------------------------------------
		private void AdjustLastColumnWidth()
		{
			if (WorkFlowsDataGrid.TableStyles == null || WorkFlowsDataGrid.TableStyles.Count == 0)
				return;

			// ScheduledTask.ScheduledTasksTableName is the MappingName of the DataGridTableStyle to retrieve. 
			DataGridTableStyle workFlowsDataGridTableStyle = WorkFlowsDataGrid.TableStyles[WorkFlow.WorkFlowTableName]; 

			if (workFlowsDataGridTableStyle != null)
			{
				int colswidth = WorkFlowsDataGrid.RowHeaderWidth;
				for (int i = 0; i < workFlowsDataGridTableStyle.GridColumnStyles.Count -1; i++)
					colswidth += workFlowsDataGridTableStyle.GridColumnStyles[i].Width;

				int newColumnWidth = WorkFlowsDataGrid.DisplayRectangle.Width - colswidth;
				if (WorkFlowsDataGrid.CurrentVertScrollBar.Visible)
					newColumnWidth -= WorkFlowsDataGrid.CurrentVertScrollBar.Width;

				DataGridColumnStyle lastColumnStyle = workFlowsDataGridTableStyle.GridColumnStyles[workFlowsDataGridTableStyle.GridColumnStyles.Count -1];
				lastColumnStyle.Width = Math.Max
					(
					WorkFlowsDataGrid.MinimumDataGridStringColumnWidth, 
					newColumnWidth
					);
				
				this.Refresh();
			}
		}

		//--------------------------------------------------------------------------
		private void VertScrollBar_VisibleChanged(object sender, EventArgs e)
		{	
			if (sender != WorkFlowsDataGrid.CurrentVertScrollBar)
				return;
			
			AdjustLastColumnWidth();

			this.Refresh();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void FillWorkFlowGrid()
		{
			SqlDataAdapter selectAllSqlDataAdapter	= null;

			ClearWorkFlowsGrid();

			if (!IsConnectionOpen)
				return;

			if (companyId == -1)
				selectAllSqlDataAdapter	= new SqlDataAdapter(WorkFlow.GetSelectAllWorkFlowsOrderedByNameQuery(), currentConnection);
			else
				selectAllSqlDataAdapter	= new SqlDataAdapter(WorkFlow.GetSelectAllCompanyWorkFlowsOrderedByNameQuery(companyId), currentConnection);
			DataTable workFlowsDataTable			= new DataTable(WorkFlow.WorkFlowTableName);
			selectAllSqlDataAdapter.Fill(workFlowsDataTable);
			
			WorkFlowsDataGrid.DataSource = workFlowsDataTable;
		}

		//---------------------------------------------------------------------
		private void WorkFlowsDataGrid_Resize(object sender, System.EventArgs e)
		{
			AdjustLastColumnWidth();
		}

		//---------------------------------------------------------------------
		public void ClearWorkFlowsGrid()
		{
			WorkFlowsDataGrid.Clear();
		}

		//---------------------------------------------------------------------
		private void WorkFlowsDataGrid_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				UpdateWorkFlowDataGridSelection();
			
		}

		//---------------------------------------------------------------------
		private void WorkFlowsDataGrid_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (TBWorkFlowControlToolTip == null)
				return;

			string currentToolTip = GetToolTipText(e);

			TBWorkFlowControlToolTip.SetToolTip(WorkFlowsDataGrid, currentToolTip);
		}

		//---------------------------------------------------------------------
		private void WorkFlowsDataGrid_CurrentCellChanged(object sender, System.EventArgs e)
		{
			UpdateWorkFlowDataGridSelection();
		}

		//---------------------------------------------------------------------
		private void WorkFlowsDataGrid_DoubleClick(object sender, System.EventArgs e)
		{
			if (!IsConnectionOpen || WorkFlowsDataGrid.DataSource == null || WorkFlowsDataGrid.CurrentRowIndex < 0)
				return;

			Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
			Point dataGridMousePosition = WorkFlowsDataGrid.PointToClient(mousePosition);

			System.Windows.Forms.DataGrid.HitTestInfo hitTestinfo = WorkFlowsDataGrid.HitTest(dataGridMousePosition);

			if (hitTestinfo.Type == DataGrid.HitTestType.None || hitTestinfo.Row != WorkFlowsDataGrid.CurrentRowIndex)
				return;

			ViewSelectedWorkFlow(sender, e);
		}

		//---------------------------------------------------------------------
		private void WorkFlowsDataGrid_VisibleChanged(object sender, System.EventArgs e)
		{
			AdjustLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void UpdateWorkFlowDataGridSelection()
		{
			if (IsConnectionOpen && WorkFlowsDataGrid.DataSource != null && WorkFlowsDataGrid.CurrentRowIndex >= 0)
			{
				Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
				Point dataGridMousePosition = WorkFlowsDataGrid.PointToClient(mousePosition);

				System.Windows.Forms.DataGrid.HitTestInfo hitTestinfo = WorkFlowsDataGrid.HitTest(dataGridMousePosition);

				if (hitTestinfo.Type == DataGrid.HitTestType.Cell || hitTestinfo.Type == DataGrid.HitTestType.RowHeader)
				{
					WorkFlowsDataGrid.CurrentRowIndex = hitTestinfo.Row;
					UpdateCurrentWorkFlowOperationStatus();
					this.Refresh();
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateCurrentWorkFlowOperationStatus()
		{
			/*
			WorkFlow currentWorkFlow = GetSelectedWorkFlow(false);
			if (currentWorkFlow != null)
			{
				DeleteCurrentTaskLinkLabel.Enabled = !currentTask.IsRunning;
				CloneCurrentTaskLinkLabel.Enabled = true;
				ShowCurrentTaskSchedulingDetailsLinkLabel.Enabled = true;
				CurrentTaskPropertiesLinkLabel.Enabled = !currentTask.IsRunning;

				TasksMngPanel.Size = new Size(TasksMngPanel.Width, CurrentTaskPropertiesLinkLabel.Bottom + 8); 

				RunCurrentTaskLinkLabel.Enabled = currentTask.Enabled && currentTask.ToRunOnDemand && !currentTask.IsRunning;
			}
			else
			{
				DeleteCurrentTaskLinkLabel.Enabled = false;
				CloneCurrentTaskLinkLabel.Enabled = false;
				ShowCurrentTaskSchedulingDetailsLinkLabel.Enabled = false;
				CurrentTaskPropertiesLinkLabel.Enabled = false;

				TasksMngPanel.Size = new Size(TasksMngPanel.Width, DeleteCurrentTaskLinkLabel.Location.Y - 8); 
				
				RunCurrentTaskLinkLabel.Enabled = false;
			}

			RunTasksMngPanel.Location = new Point(RunTasksMngPanel.Location.X, TasksMngPanel.Location.Y + TasksMngPanel.Height + 8);
			*/

		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetToolTipText(System.Windows.Forms.MouseEventArgs e)
		{
			DataGrid.HitTestInfo hitTestinfo = this.WorkFlowsDataGrid.HitTest(e.X, e.Y);

			string tooltipText = String.Empty;
			if 
				(
				e.Clicks == 0 &&
				hitTestinfo.Type == DataGrid.HitTestType.Cell &&
				hitTestinfo.Row >= 0 &&
				hitTestinfo.Column >= 0
				)
			{
				int descriptionColIdx = GetDataGridTableStyleDescriptionColumnIndex();
				int commandColIdx = GetDataGridTableStyleNameColumnIndex();
			
				if (hitTestinfo.Column == descriptionColIdx || hitTestinfo.Column == commandColIdx)
				{
					DataTable workFlowsdataTable = (DataTable)this.WorkFlowsDataGrid.DataSource;
					if (workFlowsdataTable != null && hitTestinfo.Row < workFlowsdataTable.Rows.Count)
					{
						try
						{
							DataRow aRow = workFlowsdataTable.Rows[hitTestinfo.Row];
							if (aRow != null)
								tooltipText = (string)aRow[(hitTestinfo.Column == descriptionColIdx) ? WorkFlow.WorkFlowDescColumnName : WorkFlow.WorkFlowNameColumnName]; 
						}
						catch(Exception){}
					}
				}
			}
			return tooltipText;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetDataGridTableStyleDescriptionColumnIndex()
		{
			return GetDataGridTableStyleColumnIndex(WorkFlow.WorkFlowDescColumnName);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetDataGridTableStyleNameColumnIndex()
		{
			return GetDataGridTableStyleColumnIndex(WorkFlow.WorkFlowNameColumnName);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetDataGridTableStyleColumnIndex(string aColumnMappingName)
		{
			if (aColumnMappingName == null || aColumnMappingName == string.Empty || WorkFlowsDataGrid.TableStyles.Count == 0)
				return -1;

			DataGridTableStyle workFlowsDataGridTableStyle = WorkFlowsDataGrid.TableStyles[WorkFlow.WorkFlowTableName]; 
			if (workFlowsDataGridTableStyle == null)
				return -1;
			
			for (int i = 0; i < workFlowsDataGridTableStyle.GridColumnStyles.Count; i++)
			{
				if (String.Compare(workFlowsDataGridTableStyle.GridColumnStyles[i].MappingName, aColumnMappingName) == 0)
					return i;
			}
			return -1;
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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TBViewWorkFlow));
			this.WorkFlowsDataGrid = new Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid();
			this.TBWorkFlowControlToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.WorkFlowContextMenu = new System.Windows.Forms.ContextMenu();
			((System.ComponentModel.ISupportInitialize)(this.WorkFlowsDataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// WorkFlowsDataGrid
			// 
			this.WorkFlowsDataGrid.AccessibleDescription = resources.GetString("WorkFlowsDataGrid.AccessibleDescription");
			this.WorkFlowsDataGrid.AccessibleName = resources.GetString("WorkFlowsDataGrid.AccessibleName");
			this.WorkFlowsDataGrid.AlternatingBackColor = System.Drawing.Color.Lavender;
			this.WorkFlowsDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WorkFlowsDataGrid.Anchor")));
			this.WorkFlowsDataGrid.BackColor = System.Drawing.Color.Lavender;
			this.WorkFlowsDataGrid.BackgroundColor = System.Drawing.Color.Lavender;
			this.WorkFlowsDataGrid.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WorkFlowsDataGrid.BackgroundImage")));
			this.WorkFlowsDataGrid.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
			this.WorkFlowsDataGrid.CaptionFont = ((System.Drawing.Font)(resources.GetObject("WorkFlowsDataGrid.CaptionFont")));
			this.WorkFlowsDataGrid.CaptionForeColor = System.Drawing.Color.Navy;
			this.WorkFlowsDataGrid.CaptionText = resources.GetString("WorkFlowsDataGrid.CaptionText");
			this.WorkFlowsDataGrid.CurrentRow = null;
			this.WorkFlowsDataGrid.DataMember = "";
			this.WorkFlowsDataGrid.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WorkFlowsDataGrid.Dock")));
			this.WorkFlowsDataGrid.Enabled = ((bool)(resources.GetObject("WorkFlowsDataGrid.Enabled")));
			this.WorkFlowsDataGrid.Font = ((System.Drawing.Font)(resources.GetObject("WorkFlowsDataGrid.Font")));
			this.WorkFlowsDataGrid.ForeColor = System.Drawing.Color.Navy;
			this.WorkFlowsDataGrid.GridLineColor = System.Drawing.Color.LightSteelBlue;
			this.WorkFlowsDataGrid.HeaderBackColor = System.Drawing.Color.LightSteelBlue;
			this.WorkFlowsDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.WorkFlowsDataGrid.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WorkFlowsDataGrid.ImeMode")));
			this.WorkFlowsDataGrid.Location = ((System.Drawing.Point)(resources.GetObject("WorkFlowsDataGrid.Location")));
			this.WorkFlowsDataGrid.Name = "WorkFlowsDataGrid";
			this.WorkFlowsDataGrid.ParentRowsBackColor = System.Drawing.Color.Lavender;
			this.WorkFlowsDataGrid.ParentRowsForeColor = System.Drawing.Color.Navy;
			this.WorkFlowsDataGrid.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkFlowsDataGrid.RightToLeft")));
			this.WorkFlowsDataGrid.SelectionBackColor = System.Drawing.Color.CornflowerBlue;
			this.WorkFlowsDataGrid.SelectionForeColor = System.Drawing.Color.AliceBlue;
			this.WorkFlowsDataGrid.Size = ((System.Drawing.Size)(resources.GetObject("WorkFlowsDataGrid.Size")));
			this.WorkFlowsDataGrid.TabIndex = ((int)(resources.GetObject("WorkFlowsDataGrid.TabIndex")));
			this.TBWorkFlowControlToolTip.SetToolTip(this.WorkFlowsDataGrid, resources.GetString("WorkFlowsDataGrid.ToolTip"));
			this.WorkFlowsDataGrid.Visible = ((bool)(resources.GetObject("WorkFlowsDataGrid.Visible")));
			this.WorkFlowsDataGrid.Resize += new System.EventHandler(this.WorkFlowsDataGrid_Resize);
			this.WorkFlowsDataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WorkFlowsDataGrid_MouseDown);
			this.WorkFlowsDataGrid.DoubleClick += new System.EventHandler(this.WorkFlowsDataGrid_DoubleClick);
			this.WorkFlowsDataGrid.VisibleChanged += new System.EventHandler(this.WorkFlowsDataGrid_VisibleChanged);
			this.WorkFlowsDataGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WorkFlowsDataGrid_MouseMove);
			this.WorkFlowsDataGrid.CurrentCellChanged += new System.EventHandler(this.WorkFlowsDataGrid_CurrentCellChanged);
			// 
			// WorkFlowContextMenu
			// 
			this.WorkFlowContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkFlowContextMenu.RightToLeft")));
			this.WorkFlowContextMenu.Popup += new System.EventHandler(this.WorkFlowContextMenu_Popup);
			// 
			// TBViewWorkFlow
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.WorkFlowsDataGrid);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "TBViewWorkFlow";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.TBWorkFlowControlToolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
			((System.ComponentModel.ISupportInitialize)(this.WorkFlowsDataGrid)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion


		

		//---------------------------------------------------------------------
		private void WorkFlowContextMenu_Popup(object sender, System.EventArgs e)
		{
			WorkFlowContextMenu.MenuItems.Clear();

			if (!IsConnectionOpen || WorkFlowsDataGrid.DataSource == null || WorkFlowsDataGrid.CurrentRowIndex < 0)
				return;

			Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
			Point dataGridMousePosition = WorkFlowsDataGrid.PointToClient(mousePosition);

			Rectangle rectCurrentCell = WorkFlowsDataGrid.GetCurrentCellBounds();
			if (dataGridMousePosition.Y < rectCurrentCell.Top || dataGridMousePosition.Y > rectCurrentCell.Bottom)
				return;

			// Visualizza (ed eventualmente modifica) un template di workflow
			MenuItem modifyMenuItem	= new MenuItem();
			modifyMenuItem.Index		= WorkFlowContextMenu.MenuItems.Count;
			modifyMenuItem.Text			= ContextMenusString.View;
			modifyMenuItem.Click		+= new System.EventHandler(this.ViewSelectedWorkFlow);
			WorkFlowContextMenu.MenuItems.Add(modifyMenuItem);


			MenuItem deleteMenuItem	= new MenuItem();
			deleteMenuItem.Index		= WorkFlowContextMenu.MenuItems.Count;
			deleteMenuItem.Text			= ContextMenusString.Delete;
			deleteMenuItem.Click		+= new System.EventHandler(this.DeleteSelectedWorkFlow);
			WorkFlowContextMenu.MenuItems.Add(deleteMenuItem);
		}

		//---------------------------------------------------------------------
		private void ViewSelectedWorkFlow(object sender, System.EventArgs e)
		{
			DataRow currentSelectedRow = WorkFlowsDataGrid.CurrentRow;
			if (OnViewSelectedRow != null)
				OnViewSelectedRow(sender, (int)currentSelectedRow[WorkFlow.CompanyIdColumnName], (int)currentSelectedRow[WorkFlow.WorkFlowIdColumnName], (int)currentSelectedRow[WorkFlow.TemplateIdColumnName]);
		}

		//---------------------------------------------------------------------
		private void DeleteSelectedWorkFlow(object sender, System.EventArgs e)
		{
			DataRow currentSelectedRow = WorkFlowsDataGrid.CurrentRow;
			if (OnDeleteSelectedRow != null)
				OnDeleteSelectedRow(sender, currentSelectedRow);
		}

		
		

		
	}
}
