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
	/// Summary description for ActivityManager.
	/// </summary>
	public class TBViewActivity : System.Windows.Forms.UserControl
	{
		

		private bool			isConnectionOpen		= false;
		private SqlConnection	currentConnection		= null;
		private string			currentConnectionString = string.Empty;
		private int			    companyId = -1;
		private int             workFlowId = -1;
		private System.Windows.Forms.ToolTip TBWorkFlowActivityToolTip;
		private System.Windows.Forms.ContextMenu WorkFlowActivitiesContextMenu;
		private Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid WFActivityDataGrid;
		private System.ComponentModel.IContainer components;

		public bool			 IsConnectionOpen			{ get { return isConnectionOpen; } set { isConnectionOpen = value; }}
		public string		 CurrentConnectionString	{ set { currentConnectionString = value; }}
		public SqlConnection CurrentConnection			{ set { currentConnection		= value; }}
		public int			 CompanyId					{ set { companyId				= value; }}
		public int			 WorkFlowId					{ set { workFlowId				= value; }}
		//--------------------------------------------------------------------------------------------------------------------------------
		public DataRow CurrentWorkFlowActivityDataGridRow { get { return WFActivityDataGrid.CurrentRow; } }
		public DataGrid CurrentWorkFlowActivityDataGrid { get { return WFActivityDataGrid; }}

		//---------------------------------------------------------------------
		public TBViewActivity()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			WFActivityDataGrid.CaptionText		= WorkFlowActionsString.WorkFlowActionGridCaption;
			WFActivityDataGrid.VisibleChanged	+= new EventHandler(this.VertScrollBar_VisibleChanged);
			WFActivityDataGrid.ContextMenu		= WorkFlowActivitiesContextMenu;
			InitializeTableStyles();

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

		//---------------------------------------------------------------------
		public void InitializeTableStyles()
		{
			WFActivityDataGrid.TableStyles.Clear();

			
			DataGridTableStyle dataGridWorkFlowStyle	= new DataGridTableStyle();
			dataGridWorkFlowStyle.DataGrid				= WFActivityDataGrid;
			dataGridWorkFlowStyle.MappingName			= WorkFlowActivity.WorkFlowActionTableName;
			dataGridWorkFlowStyle.GridLineStyle			= System.Windows.Forms.DataGridLineStyle.Solid;
			dataGridWorkFlowStyle.RowHeadersVisible		= true;
			dataGridWorkFlowStyle.ColumnHeadersVisible	= true;
			dataGridWorkFlowStyle.HeaderFont			= new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			dataGridWorkFlowStyle.PreferredRowHeight	= dataGridWorkFlowStyle.HeaderFont.Height;
			dataGridWorkFlowStyle.PreferredColumnWidth	= 100;
			dataGridWorkFlowStyle.ReadOnly				= true;
			dataGridWorkFlowStyle.RowHeaderWidth		= 12;
			dataGridWorkFlowStyle.AlternatingBackColor	= WFActivityDataGrid.AlternatingBackColor;
			dataGridWorkFlowStyle.BackColor				= WFActivityDataGrid.BackColor;
			dataGridWorkFlowStyle.ForeColor				= WFActivityDataGrid.ForeColor;
			dataGridWorkFlowStyle.GridLineStyle			= WFActivityDataGrid.GridLineStyle;
			dataGridWorkFlowStyle.GridLineColor			= WFActivityDataGrid.GridLineColor;
			dataGridWorkFlowStyle.HeaderBackColor		= WFActivityDataGrid.HeaderBackColor;
			dataGridWorkFlowStyle.HeaderForeColor		= WFActivityDataGrid.HeaderForeColor;
			dataGridWorkFlowStyle.SelectionBackColor	= WFActivityDataGrid.SelectionBackColor;
			dataGridWorkFlowStyle.SelectionForeColor	= WFActivityDataGrid.SelectionForeColor;

			// 
			// dataGridActivityTextBox
			// 
			ActivityTextBoxDataGridColumnStyle dataGridActivityTextBox = new ActivityTextBoxDataGridColumnStyle();
			dataGridActivityTextBox.Alignment = System.Windows.Forms.HorizontalAlignment.Left;
			dataGridActivityTextBox.Format = "";
			dataGridActivityTextBox.FormatInfo = null;
			dataGridActivityTextBox.HeaderText = WorkFlowActionsString.DataGridActivityNameColumnHeaderText;
			dataGridActivityTextBox.MappingName = WorkFlowActivity.ActivityNameColumnName;
			dataGridActivityTextBox.NullText = string.Empty;
			dataGridActivityTextBox.ReadOnly = true;
			dataGridActivityTextBox.Width = WFActivityDataGrid.MinimumDataGridCodeColumnWidth;
			dataGridActivityTextBox.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);
			
			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridActivityTextBox);

			// 
			// dataGridDescriptionTextBox
			// 
			ActivityTextBoxDataGridColumnStyle dataGridActivityDescriptionTextBox = new ActivityTextBoxDataGridColumnStyle();
			dataGridActivityDescriptionTextBox.Alignment = System.Windows.Forms.HorizontalAlignment.Left;
			dataGridActivityDescriptionTextBox.Format = "";
			dataGridActivityDescriptionTextBox.FormatInfo = null;
			dataGridActivityDescriptionTextBox.HeaderText = WorkFlowActionsString.DataGridActivityDescColumnHeaderText;
			dataGridActivityDescriptionTextBox.MappingName = WorkFlowActivity.ActivityDescriptionColumnName;
			dataGridActivityDescriptionTextBox.NullText = string.Empty;
			dataGridActivityDescriptionTextBox.ReadOnly = true;
			dataGridActivityDescriptionTextBox.Width = WFActivityDataGrid.MinimumDataGridCodeColumnWidth;
			dataGridActivityDescriptionTextBox.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);

			dataGridWorkFlowStyle.GridColumnStyles.Add(dataGridActivityDescriptionTextBox);
			

			WFActivityDataGrid.TableStyles.Add(dataGridWorkFlowStyle);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void ClearWorkFlowActivityGrid()
		{
			WFActivityDataGrid.Clear();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void FillWorkFlowActivityGrid()
		{
			
			ClearWorkFlowActivityGrid();

			SqlDataAdapter selectAllSqlDataAdapter = null;

			if (!IsConnectionOpen)
				return;

			//if (this.workFlowId == -1 && this.companyId == -1)
			//	selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowActivity.GetSelectAllActivitiesOrderedByNameQuery(companyId, workFlowId), currentConnection);
			//else if (this.workFlowId == -1)
			//	selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowActivity.GetSelectAllActivitiesForCompanyOrderedByNameQuery(this.companyId), currentConnection);
			//else
				selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowActivity.GetSelectAllWorkFlowActivitiesOrderedByNameQuery(this.companyId, this.workFlowId), currentConnection);
			DataTable activitiesDataTable = new DataTable(WorkFlowActivity.WorkFlowActionTableName);
			selectAllSqlDataAdapter.Fill(activitiesDataTable);
			
			WFActivityDataGrid.DataSource = activitiesDataTable;

			
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void GridColumn_WidthChanged(object sender, System.EventArgs e)
		{
			AdjustLastColumnWidth();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AdjustLastColumnWidth()
		{
			if (WFActivityDataGrid.TableStyles == null || WFActivityDataGrid.TableStyles.Count == 0)
				return;

			// ScheduledTask.ScheduledTasksTableName is the MappingName of the DataGridTableStyle to retrieve. 
			DataGridTableStyle actionsDataGridTableStyle = WFActivityDataGrid.TableStyles[WorkFlowActivity.WorkFlowActionTableName]; 

			if (actionsDataGridTableStyle != null)
			{
				int colswidth = WFActivityDataGrid.RowHeaderWidth;
				for (int i = 0; i < actionsDataGridTableStyle.GridColumnStyles.Count -1; i++)
					colswidth += actionsDataGridTableStyle.GridColumnStyles[i].Width;

				int newColumnWidth = WFActivityDataGrid.DisplayRectangle.Width - colswidth;
				if (WFActivityDataGrid.CurrentVertScrollBar.Visible)
					newColumnWidth -= WFActivityDataGrid.CurrentVertScrollBar.Width;

				DataGridColumnStyle lastColumnStyle = actionsDataGridTableStyle.GridColumnStyles[actionsDataGridTableStyle.GridColumnStyles.Count -1];
				lastColumnStyle.Width = Math.Max
					(
					WFActivityDataGrid.MinimumDataGridStringColumnWidth, 
					newColumnWidth
					);
				
				this.Refresh();
			}
		}

		//--------------------------------------------------------------------------
		private void VertScrollBar_VisibleChanged(object sender, EventArgs e)
		{	
			if (sender != WFActivityDataGrid.CurrentVertScrollBar)
				return;
			
			AdjustLastColumnWidth();

			this.Refresh();
		}


		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TBViewActivity));
			this.TBWorkFlowActivityToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.WFActivityDataGrid = new Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid();
			this.WorkFlowActivitiesContextMenu = new System.Windows.Forms.ContextMenu();
			((System.ComponentModel.ISupportInitialize)(this.WFActivityDataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// WFActivityDataGrid
			// 
			this.WFActivityDataGrid.AccessibleDescription = resources.GetString("WFActivityDataGrid.AccessibleDescription");
			this.WFActivityDataGrid.AccessibleName = resources.GetString("WFActivityDataGrid.AccessibleName");
			this.WFActivityDataGrid.AlternatingBackColor = System.Drawing.Color.Lavender;
			this.WFActivityDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WFActivityDataGrid.Anchor")));
			this.WFActivityDataGrid.BackColor = System.Drawing.Color.Lavender;
			this.WFActivityDataGrid.BackgroundColor = System.Drawing.Color.Lavender;
			this.WFActivityDataGrid.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WFActivityDataGrid.BackgroundImage")));
			this.WFActivityDataGrid.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
			this.WFActivityDataGrid.CaptionFont = ((System.Drawing.Font)(resources.GetObject("WFActivityDataGrid.CaptionFont")));
			this.WFActivityDataGrid.CaptionForeColor = System.Drawing.Color.Navy;
			this.WFActivityDataGrid.CaptionText = resources.GetString("WFActivityDataGrid.CaptionText");
			this.WFActivityDataGrid.CurrentRow = null;
			this.WFActivityDataGrid.DataMember = "";
			this.WFActivityDataGrid.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WFActivityDataGrid.Dock")));
			this.WFActivityDataGrid.Enabled = ((bool)(resources.GetObject("WFActivityDataGrid.Enabled")));
			this.WFActivityDataGrid.Font = ((System.Drawing.Font)(resources.GetObject("WFActivityDataGrid.Font")));
			this.WFActivityDataGrid.ForeColor = System.Drawing.Color.Navy;
			this.WFActivityDataGrid.GridLineColor = System.Drawing.Color.LightSteelBlue;
			this.WFActivityDataGrid.HeaderBackColor = System.Drawing.Color.LightSteelBlue;
			this.WFActivityDataGrid.HeaderForeColor = System.Drawing.Color.DarkBlue;
			this.WFActivityDataGrid.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WFActivityDataGrid.ImeMode")));
			this.WFActivityDataGrid.Location = ((System.Drawing.Point)(resources.GetObject("WFActivityDataGrid.Location")));
			this.WFActivityDataGrid.Name = "WFActivityDataGrid";
			this.WFActivityDataGrid.ParentRowsBackColor = System.Drawing.Color.Lavender;
			this.WFActivityDataGrid.ParentRowsForeColor = System.Drawing.Color.Navy;
			this.WFActivityDataGrid.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WFActivityDataGrid.RightToLeft")));
			this.WFActivityDataGrid.SelectionBackColor = System.Drawing.Color.CornflowerBlue;
			this.WFActivityDataGrid.SelectionForeColor = System.Drawing.Color.AliceBlue;
			this.WFActivityDataGrid.Size = ((System.Drawing.Size)(resources.GetObject("WFActivityDataGrid.Size")));
			this.WFActivityDataGrid.TabIndex = ((int)(resources.GetObject("WFActivityDataGrid.TabIndex")));
			this.TBWorkFlowActivityToolTip.SetToolTip(this.WFActivityDataGrid, resources.GetString("WFActivityDataGrid.ToolTip"));
			this.WFActivityDataGrid.Visible = ((bool)(resources.GetObject("WFActivityDataGrid.Visible")));
			this.WFActivityDataGrid.Resize += new System.EventHandler(this.WFActivityDataGrid_Resize);
			this.WFActivityDataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WFActivityDataGrid_MouseDown);
			this.WFActivityDataGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WFActivityDataGrid_MouseMove);
			this.WFActivityDataGrid.CurrentCellChanged += new System.EventHandler(this.WFActivityDataGrid_CurrentCellChanged);
			// 
			// WorkFlowActivitiesContextMenu
			// 
			this.WorkFlowActivitiesContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkFlowActivitiesContextMenu.RightToLeft")));
			this.WorkFlowActivitiesContextMenu.Popup += new System.EventHandler(this.WorkFlowActivitiesContextMenu_Popup);
			// 
			// TBViewActivity
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.WFActivityDataGrid);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "TBViewActivity";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.TBWorkFlowActivityToolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.Load += new System.EventHandler(this.TBViewActivity_Load);
			((System.ComponentModel.ISupportInitialize)(this.WFActivityDataGrid)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		//--------------------------------------------------------------------------
		private void WFActivityDataGrid_Resize(object sender, System.EventArgs e)
		{
			AdjustLastColumnWidth();
		}

		//---------------------------------------------------------------------
		private void WFActivityDataGrid_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				UpdateWorkFlowActivityDataGridSelection();
		}

		//--------------------------------------------------------------------------
		private void WFActivityDataGrid_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (TBWorkFlowActivityToolTip == null)
				return;

			string currentToolTip = GetToolTipText(e);

			TBWorkFlowActivityToolTip.SetToolTip(WFActivityDataGrid, currentToolTip);
		}

		//--------------------------------------------------------------------------
		private void WFActivityDataGrid_CurrentCellChanged(object sender, System.EventArgs e)
		{
			UpdateWorkFlowActivityDataGridSelection();
		}

		//---------------------------------------------------------------------
		private void UpdateWorkFlowActivityDataGridSelection()
		{
			if (IsConnectionOpen && WFActivityDataGrid.DataSource != null && WFActivityDataGrid.CurrentRowIndex >= 0)
			{
				Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
				Point dataGridMousePosition = WFActivityDataGrid.PointToClient(mousePosition);

				DataGrid.HitTestInfo hitTestinfo = WFActivityDataGrid.HitTest(dataGridMousePosition);

				if (hitTestinfo.Type == DataGrid.HitTestType.Cell || hitTestinfo.Type == DataGrid.HitTestType.RowHeader)
				{
					WFActivityDataGrid.CurrentRowIndex = hitTestinfo.Row;
					UpdateCurrentWorkFlowActivityOperationStatus();
					this.Refresh();
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateCurrentWorkFlowActivityOperationStatus()
		{
		}

		//--------------------------------------------------------------------------
		private void WFActivityDataGrid_DoubleClick(object sender, System.EventArgs e)
		{
		
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetToolTipText(System.Windows.Forms.MouseEventArgs e)
		{
			DataGrid.HitTestInfo hitTestinfo = WFActivityDataGrid.HitTest(e.X, e.Y);

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
				int nameColIdx = GetDataGridTableStyleNameColumnIndex();
			
				if (hitTestinfo.Column == descriptionColIdx || hitTestinfo.Column == nameColIdx)
				{
					DataTable activitiesdataTable = (DataTable)this.WFActivityDataGrid.DataSource;
					if (activitiesdataTable != null && hitTestinfo.Row < activitiesdataTable.Rows.Count)
					{
						try
						{
							DataRow aRow = activitiesdataTable.Rows[hitTestinfo.Row];
							if (aRow != null)
								tooltipText = (string)aRow[(hitTestinfo.Column == descriptionColIdx) ? WorkFlowActivity.ActivityDescriptionColumnName : WorkFlowActivity.ActivityNameColumnName]; 
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
			return GetDataGridTableStyleColumnIndex(WorkFlowActivity.ActivityDescriptionColumnName);
		}

		//---------------------------------------------------------------------
		private int GetDataGridTableStyleNameColumnIndex()
		{
			return GetDataGridTableStyleColumnIndex(WorkFlowActivity.ActivityNameColumnName);
		}

		//---------------------------------------------------------------------
		private int GetDataGridTableStyleColumnIndex(string aColumnMappingName)
		{
			if (aColumnMappingName == null || aColumnMappingName == string.Empty || WFActivityDataGrid.TableStyles.Count == 0)
				return -1;

			DataGridTableStyle actitiesDataGridTableStyle = WFActivityDataGrid.TableStyles[WorkFlowActivity.WorkFlowActionTableName]; 
			if (actitiesDataGridTableStyle == null)
				return -1;
			
			for (int i = 0; i < actitiesDataGridTableStyle.GridColumnStyles.Count; i++)
			{
				if (String.Compare(actitiesDataGridTableStyle.GridColumnStyles[i].MappingName, aColumnMappingName) == 0)
					return i;
			}
			return -1;
		}

		//---------------------------------------------------------------------
		private void WorkFlowActivitiesContextMenu_Popup(object sender, System.EventArgs e)
		{
			WorkFlowActivitiesContextMenu.MenuItems.Clear();

			if (!IsConnectionOpen || WFActivityDataGrid.DataSource == null || WFActivityDataGrid.CurrentRowIndex < 0)
				return;

			Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
			Point dataGridMousePosition = WFActivityDataGrid.PointToClient(mousePosition);

			Rectangle rectCurrentCell = WFActivityDataGrid.GetCurrentCellBounds();
			if (dataGridMousePosition.Y < rectCurrentCell.Top || dataGridMousePosition.Y > rectCurrentCell.Bottom)
				return;

			WorkFlowActivity activity = GetWorkFlowActivity();

			if (activity == null)
				return;

			//Inserimento di una attività
			MenuItem addActivityMenuItem	= new MenuItem();
			addActivityMenuItem.Index		= WorkFlowActivitiesContextMenu.MenuItems.Count;
			addActivityMenuItem.Text		= "Aggiungi";
			addActivityMenuItem.Click		+= new System.EventHandler(this.AddActivityMenuItem_Click);
			WorkFlowActivitiesContextMenu.MenuItems.Add(addActivityMenuItem);

			// Modifica di una attività
			MenuItem modifyActivityMenuItem	= new MenuItem();
			modifyActivityMenuItem.Index		= WorkFlowActivitiesContextMenu.MenuItems.Count;
			modifyActivityMenuItem.Text			= "Modifica";
			modifyActivityMenuItem.Click		+= new System.EventHandler(this.ModifyActivityMenuItem_Click);
			WorkFlowActivitiesContextMenu.MenuItems.Add(modifyActivityMenuItem);

			// Cancellazione di una attività
			MenuItem deleteActivityMenuItem	= new MenuItem();
			deleteActivityMenuItem.Index		= WorkFlowActivitiesContextMenu.MenuItems.Count;
			deleteActivityMenuItem.Text			= "Cancella";
			deleteActivityMenuItem.Click		+= new System.EventHandler(this.DeleteActivityMenuItem_Click);
			WorkFlowActivitiesContextMenu.MenuItems.Add(deleteActivityMenuItem);
			

		}

		
		//---------------------------------------------------------------------
		private void AddActivityMenuItem_Click(object sender, System.EventArgs e)
		{
			//aggiungo una riga vuota nel datagrid per l'editing
			if (!IsConnectionOpen)
				return;
			WorkFlowActivity newActivity = new WorkFlowActivity(companyId, workFlowId);
			
		}

		//---------------------------------------------------------------------
		private void ModifyActivityMenuItem_Click(object sender, System.EventArgs e)
		{
			//editing riga corrente del datagrid
			if (!IsConnectionOpen || WFActivityDataGrid.DataSource == null || WFActivityDataGrid.CurrentRowIndex < 0)
				return;

			WorkFlowActivity currentActivity = GetWorkFlowActivity();
		}

		//---------------------------------------------------------------------
		private void DeleteActivityMenuItem_Click(object sender, System.EventArgs e)
		{
			//cancellazione riga corrente del datagrid
			if (!IsConnectionOpen || WFActivityDataGrid.DataSource == null || WFActivityDataGrid.CurrentRowIndex < 0)
				return;

			WorkFlowActivity currentActivity = GetWorkFlowActivity();

			if (MessageBox.Show(String.Format(WorkFlowControlsString.ConfirmWorkFlowActivityDeletionMsg, currentActivity.ActivityName), WorkFlowControlsString.ConfirmWorkFlowActivityDeletionCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question)== DialogResult.No)
				return;

			if (currentActivity.Delete(currentConnection))
			{
				WFActivityDataGrid.RemoveCurrentRow();
				
			}
		}

		//---------------------------------------------------------------------
		private WorkFlowActivity GetWorkFlowActivity()
		{
			if (CurrentWorkFlowActivityDataGridRow == null)
				return null;

			return new WorkFlowActivity(CurrentWorkFlowActivityDataGridRow, this.companyId, currentConnectionString);
			
		}

		private void TBViewActivity_Load(object sender, System.EventArgs e)
		{
		
		}
	}
}
