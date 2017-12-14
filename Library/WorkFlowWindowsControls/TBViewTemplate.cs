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
	/// Summary description for TBViewTemplate.
	/// </summary>
	public class TBViewTemplate : System.Windows.Forms.UserControl
	{
		private Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid WFTemplateDataGrid;

		private bool			isConnectionOpen		= false;
		private SqlConnection	currentConnection		= null;
		private string			currentConnectionString = string.Empty;
		private int			    templateId = -1;
		private System.Windows.Forms.ContextMenu TemplateWorkflowContextMenu;
		private System.Windows.Forms.ToolTip TemplateWorkFlowToolTip;
		private System.ComponentModel.IContainer components;

		public bool			 IsConnectionOpen			{ get { return isConnectionOpen; } set { isConnectionOpen = value; }}
		public string		 CurrentConnectionString	{ set { currentConnectionString = value; }}
		public SqlConnection CurrentConnection			{ set { currentConnection		= value; }}
		public int			 TemplateId					{ set { templateId				= value; }}
		//--------------------------------------------------------------------------------------------------------------------------------
		public DataRow CurrentWorkFlowTemplateDataGridRow { get { return WFTemplateDataGrid.CurrentRow; } }
		public DataGrid CurrentWorkFlowTemplateDataGrid { get { return WFTemplateDataGrid; }}

		public delegate void AfterSelectedRow(object sender, int templateId);
		public event		 AfterSelectedRow OnViewSelectedRow;

		public delegate void DeleteSelectedRow(object sender, DataRow currentRow);
		public event		 DeleteSelectedRow OnDeleteSelectedRow;

		//---------------------------------------------------------------------
		public TBViewTemplate()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

			WFTemplateDataGrid.CaptionText		= WorkFlowTemplatesString.WorkFlowTemplateGridCaption;
			WFTemplateDataGrid.VisibleChanged	+= new EventHandler(this.VertScrollBar_VisibleChanged);
			WFTemplateDataGrid.ContextMenu		= TemplateWorkflowContextMenu;
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TBViewTemplate));
			this.WFTemplateDataGrid = new Microarea.Library.WorkFlowWindowsControls.TBWorkFlowDataGrid();
			this.TemplateWorkflowContextMenu = new System.Windows.Forms.ContextMenu();
			this.TemplateWorkFlowToolTip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.WFTemplateDataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// WFTemplateDataGrid
			// 
			this.WFTemplateDataGrid.AccessibleDescription = resources.GetString("WFTemplateDataGrid.AccessibleDescription");
			this.WFTemplateDataGrid.AccessibleName = resources.GetString("WFTemplateDataGrid.AccessibleName");
			this.WFTemplateDataGrid.AlternatingBackColor = System.Drawing.Color.Lavender;
			this.WFTemplateDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WFTemplateDataGrid.Anchor")));
			this.WFTemplateDataGrid.BackColor = System.Drawing.Color.Lavender;
			this.WFTemplateDataGrid.BackgroundColor = System.Drawing.Color.Lavender;
			this.WFTemplateDataGrid.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WFTemplateDataGrid.BackgroundImage")));
			this.WFTemplateDataGrid.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
			this.WFTemplateDataGrid.CaptionFont = ((System.Drawing.Font)(resources.GetObject("WFTemplateDataGrid.CaptionFont")));
			this.WFTemplateDataGrid.CaptionForeColor = System.Drawing.Color.Navy;
			this.WFTemplateDataGrid.CaptionText = resources.GetString("WFTemplateDataGrid.CaptionText");
			this.WFTemplateDataGrid.CurrentRow = null;
			this.WFTemplateDataGrid.DataMember = "";
			this.WFTemplateDataGrid.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WFTemplateDataGrid.Dock")));
			this.WFTemplateDataGrid.Enabled = ((bool)(resources.GetObject("WFTemplateDataGrid.Enabled")));
			this.WFTemplateDataGrid.Font = ((System.Drawing.Font)(resources.GetObject("WFTemplateDataGrid.Font")));
			this.WFTemplateDataGrid.ForeColor = System.Drawing.Color.Navy;
			this.WFTemplateDataGrid.GridLineColor = System.Drawing.Color.LightSteelBlue;
			this.WFTemplateDataGrid.HeaderBackColor = System.Drawing.Color.LightSteelBlue;
			this.WFTemplateDataGrid.HeaderForeColor = System.Drawing.Color.DarkBlue;
			this.WFTemplateDataGrid.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WFTemplateDataGrid.ImeMode")));
			this.WFTemplateDataGrid.Location = ((System.Drawing.Point)(resources.GetObject("WFTemplateDataGrid.Location")));
			this.WFTemplateDataGrid.Name = "WFTemplateDataGrid";
			this.WFTemplateDataGrid.ParentRowsBackColor = System.Drawing.Color.Lavender;
			this.WFTemplateDataGrid.ParentRowsForeColor = System.Drawing.Color.Navy;
			this.WFTemplateDataGrid.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WFTemplateDataGrid.RightToLeft")));
			this.WFTemplateDataGrid.SelectionBackColor = System.Drawing.Color.CornflowerBlue;
			this.WFTemplateDataGrid.SelectionForeColor = System.Drawing.Color.AliceBlue;
			this.WFTemplateDataGrid.Size = ((System.Drawing.Size)(resources.GetObject("WFTemplateDataGrid.Size")));
			this.WFTemplateDataGrid.TabIndex = ((int)(resources.GetObject("WFTemplateDataGrid.TabIndex")));
			this.TemplateWorkFlowToolTip.SetToolTip(this.WFTemplateDataGrid, resources.GetString("WFTemplateDataGrid.ToolTip"));
			this.WFTemplateDataGrid.Visible = ((bool)(resources.GetObject("WFTemplateDataGrid.Visible")));
			this.WFTemplateDataGrid.Resize += new System.EventHandler(this.WFTemplateDataGrid_Resize);
			this.WFTemplateDataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WFTemplateDataGrid_MouseDown);
			this.WFTemplateDataGrid.DoubleClick += new System.EventHandler(this.WFTemplateDataGrid_DoubleClick);
			this.WFTemplateDataGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WFTemplateDataGrid_MouseMove);
			// 
			// TemplateWorkflowContextMenu
			// 
			this.TemplateWorkflowContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TemplateWorkflowContextMenu.RightToLeft")));
			this.TemplateWorkflowContextMenu.Popup += new System.EventHandler(this.TemplateWorkflowContextMenu_Popup);
			// 
			// TBViewTemplate
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.WFTemplateDataGrid);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "TBViewTemplate";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.TemplateWorkFlowToolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
			((System.ComponentModel.ISupportInitialize)(this.WFTemplateDataGrid)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		//---------------------------------------------------------------------
		public void InitializeTableStyles()
		{
			WFTemplateDataGrid.TableStyles.Clear();

			
			DataGridTableStyle dataGridTemplateStyle	= new DataGridTableStyle();
			dataGridTemplateStyle.DataGrid				= WFTemplateDataGrid;
			dataGridTemplateStyle.MappingName			= WorkFlowTemplate.WorkFlowTemplateTableName;
			dataGridTemplateStyle.GridLineStyle			= DataGridLineStyle.Solid;
			dataGridTemplateStyle.RowHeadersVisible		= true;
			dataGridTemplateStyle.ColumnHeadersVisible	= true;
			dataGridTemplateStyle.HeaderFont			= new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			dataGridTemplateStyle.PreferredRowHeight	= dataGridTemplateStyle.HeaderFont.Height;
			dataGridTemplateStyle.PreferredColumnWidth	= 100;
			dataGridTemplateStyle.ReadOnly				= true;
			dataGridTemplateStyle.RowHeaderWidth		= 12;
			dataGridTemplateStyle.AlternatingBackColor	= WFTemplateDataGrid.AlternatingBackColor;
			dataGridTemplateStyle.BackColor				= WFTemplateDataGrid.BackColor;
			dataGridTemplateStyle.ForeColor				= WFTemplateDataGrid.ForeColor;
			dataGridTemplateStyle.GridLineStyle			= WFTemplateDataGrid.GridLineStyle;
			dataGridTemplateStyle.GridLineColor			= WFTemplateDataGrid.GridLineColor;
			dataGridTemplateStyle.HeaderBackColor		= WFTemplateDataGrid.HeaderBackColor;
			dataGridTemplateStyle.HeaderForeColor		= WFTemplateDataGrid.HeaderForeColor;
			dataGridTemplateStyle.SelectionBackColor	= WFTemplateDataGrid.SelectionBackColor;
			dataGridTemplateStyle.SelectionForeColor	= WFTemplateDataGrid.SelectionForeColor;

			// 
			// dataGridActivityTextBox
			// 
			TemplateTextBoxDataGridColumnStyle dataGridTemplateTextBox = new TemplateTextBoxDataGridColumnStyle();
			dataGridTemplateTextBox.Alignment = System.Windows.Forms.HorizontalAlignment.Left;
			dataGridTemplateTextBox.Format = "";
			dataGridTemplateTextBox.FormatInfo = null;
			dataGridTemplateTextBox.HeaderText = WorkFlowTemplatesString.DataGridTemplateNameColumnHeaderText;
			dataGridTemplateTextBox.MappingName = WorkFlowTemplate.TemplateNameColumnName;
			dataGridTemplateTextBox.NullText = string.Empty;
			dataGridTemplateTextBox.ReadOnly = true;
			dataGridTemplateTextBox.Width = WFTemplateDataGrid.MinimumDataGridCodeColumnWidth;
			dataGridTemplateTextBox.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);
			
			dataGridTemplateStyle.GridColumnStyles.Add(dataGridTemplateTextBox);

			// 
			// dataGridDescriptionTextBox
			// 
			TemplateTextBoxDataGridColumnStyle dataGridTemplateDescriptionTextBox = new TemplateTextBoxDataGridColumnStyle();
			dataGridTemplateDescriptionTextBox.Alignment = System.Windows.Forms.HorizontalAlignment.Left;
			dataGridTemplateDescriptionTextBox.Format = "";
			dataGridTemplateDescriptionTextBox.FormatInfo = null;
			dataGridTemplateDescriptionTextBox.HeaderText = WorkFlowTemplatesString.DataGridTemplateDescColumnHeaderText;
			dataGridTemplateDescriptionTextBox.MappingName = WorkFlowTemplate.TemplateDescriptionColumnName;
			dataGridTemplateDescriptionTextBox.NullText = string.Empty;
			dataGridTemplateDescriptionTextBox.ReadOnly = true;
			dataGridTemplateDescriptionTextBox.Width = WFTemplateDataGrid.MinimumDataGridCodeColumnWidth;
			dataGridTemplateDescriptionTextBox.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);

			dataGridTemplateStyle.GridColumnStyles.Add(dataGridTemplateDescriptionTextBox);
			

			WFTemplateDataGrid.TableStyles.Add(dataGridTemplateStyle);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void ClearWorkFlowTemplateGrid()
		{
			WFTemplateDataGrid.Clear();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void FillWorkFlowTemplateGrid()
		{
			
			ClearWorkFlowTemplateGrid();

			SqlDataAdapter selectAllSqlDataAdapter = null;

			if (!IsConnectionOpen)
				return;

			selectAllSqlDataAdapter = new SqlDataAdapter(WorkFlowTemplate.GetSelectAllWorkFlowTemplateOrderedByNameQuery(), currentConnection);
			DataTable templatesDataTable = new DataTable(WorkFlowTemplate.WorkFlowTemplateTableName);
			selectAllSqlDataAdapter.Fill(templatesDataTable);
			
			WFTemplateDataGrid.DataSource = templatesDataTable;

			
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void GridColumn_WidthChanged(object sender, System.EventArgs e)
		{
			AdjustLastColumnWidth();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AdjustLastColumnWidth()
		{
			if (WFTemplateDataGrid.TableStyles == null || WFTemplateDataGrid.TableStyles.Count == 0)
				return;

			// ScheduledTask.ScheduledTasksTableName is the MappingName of the DataGridTableStyle to retrieve. 
			DataGridTableStyle templatesDataGridTableStyle = WFTemplateDataGrid.TableStyles[WorkFlowTemplate.WorkFlowTemplateTableName]; 

			if (templatesDataGridTableStyle != null)
			{
				int colswidth = WFTemplateDataGrid.RowHeaderWidth;
				for (int i = 0; i < templatesDataGridTableStyle.GridColumnStyles.Count -1; i++)
					colswidth += templatesDataGridTableStyle.GridColumnStyles[i].Width;

				int newColumnWidth = WFTemplateDataGrid.DisplayRectangle.Width - colswidth;
				if (WFTemplateDataGrid.CurrentVertScrollBar.Visible)
					newColumnWidth -= WFTemplateDataGrid.CurrentVertScrollBar.Width;

				DataGridColumnStyle lastColumnStyle = templatesDataGridTableStyle.GridColumnStyles[templatesDataGridTableStyle.GridColumnStyles.Count -1];
				lastColumnStyle.Width = Math.Max
					(
					WFTemplateDataGrid.MinimumDataGridStringColumnWidth, 
					newColumnWidth
					);
				
				this.Refresh();
			}
		}

		//--------------------------------------------------------------------------
		private void VertScrollBar_VisibleChanged(object sender, EventArgs e)
		{	
			if (sender != WFTemplateDataGrid.CurrentVertScrollBar)
				return;
			
			AdjustLastColumnWidth();

			this.Refresh();
		}

		//--------------------------------------------------------------------------
		private void WFTemplateDataGrid_Resize(object sender, System.EventArgs e)
		{
			AdjustLastColumnWidth();
		}

		//--------------------------------------------------------------------------
		private void WFTemplateDataGrid_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			

			
		}

		//---------------------------------------------------------------------
		private void ViewSelectedTemplate(object sender, System.EventArgs e)
		{
			DataRow currentSelectedRow = WFTemplateDataGrid.CurrentRow;
			if (OnViewSelectedRow != null)
				OnViewSelectedRow(sender,(int)currentSelectedRow[WorkFlowTemplate.TemplateIdColumnName]);
		}

		//---------------------------------------------------------------------
		private void DeleteSelectedTemplate(object sender, System.EventArgs e)
		{
			DataRow currentSelectedRow = WFTemplateDataGrid.CurrentRow;
			if (OnDeleteSelectedRow != null)
				OnDeleteSelectedRow(sender, currentSelectedRow);
		}


		//--------------------------------------------------------------------------
		private void TemplateWorkflowContextMenu_Popup(object sender, System.EventArgs e)
		{
			TemplateWorkflowContextMenu.MenuItems.Clear();

			if (!IsConnectionOpen || WFTemplateDataGrid.DataSource == null || WFTemplateDataGrid.CurrentRowIndex < 0)
				return;

			Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
			Point dataGridMousePosition = WFTemplateDataGrid.PointToClient(mousePosition);

			Rectangle rectCurrentCell = WFTemplateDataGrid.GetCurrentCellBounds();
			if (dataGridMousePosition.Y < rectCurrentCell.Top || dataGridMousePosition.Y > rectCurrentCell.Bottom)
				return;

			// Visualizza (ed eventualmente modifica) un template di workflow
			MenuItem modifyTemplateMenuItem	= new MenuItem();
			modifyTemplateMenuItem.Index		= TemplateWorkflowContextMenu.MenuItems.Count;
			modifyTemplateMenuItem.Text			= ContextMenusString.View;
			modifyTemplateMenuItem.Click		+= new System.EventHandler(this.ViewSelectedTemplate);
			TemplateWorkflowContextMenu.MenuItems.Add(modifyTemplateMenuItem);


			MenuItem deleteTemplateMenuItem	= new MenuItem();
			deleteTemplateMenuItem.Index		= TemplateWorkflowContextMenu.MenuItems.Count;
			deleteTemplateMenuItem.Text			= ContextMenusString.Delete;
			deleteTemplateMenuItem.Click		+= new System.EventHandler(this.DeleteSelectedTemplate);
			TemplateWorkflowContextMenu.MenuItems.Add(deleteTemplateMenuItem);

		}

		/// <summary>
		/// WFTemplateDataGrid_DoubleClick
		/// Apre il dettaglio del modello del workflow selezionato
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------------
		private void WFTemplateDataGrid_DoubleClick(object sender, System.EventArgs e)
		{
			if (!IsConnectionOpen || WFTemplateDataGrid.DataSource == null || WFTemplateDataGrid.CurrentRowIndex < 0)
				return;

			Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
			Point dataGridMousePosition = WFTemplateDataGrid.PointToClient(mousePosition);

			System.Windows.Forms.DataGrid.HitTestInfo hitTestinfo = WFTemplateDataGrid.HitTest(dataGridMousePosition);

			if (hitTestinfo.Type == DataGrid.HitTestType.None || hitTestinfo.Row != WFTemplateDataGrid.CurrentRowIndex)
				return;

			ViewSelectedTemplate(sender, e);
		}

		//--------------------------------------------------------------------------
		private void WFTemplateDataGrid_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (TemplateWorkFlowToolTip == null)
				return;

			GetToolTipText(e);

			TemplateWorkFlowToolTip.SetToolTip(WFTemplateDataGrid, GetToolTipText(e));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetToolTipText(System.Windows.Forms.MouseEventArgs e)
		{
			System.Windows.Forms.DataGrid.HitTestInfo hitTestinfo = WFTemplateDataGrid.HitTest(e.X, e.Y);

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
					DataTable templatesDataTable = (DataTable)WFTemplateDataGrid.DataSource;
					if (templatesDataTable != null && hitTestinfo.Row < templatesDataTable.Rows.Count)
					{
						DataRow aRow = templatesDataTable.Rows[hitTestinfo.Row];
						if (aRow != null)
							tooltipText = (string)aRow[(hitTestinfo.Column == descriptionColIdx) ? WorkFlowTemplate.TemplateDescriptionColumnName : WorkFlowTemplate.TemplateNameColumnName];
					}
				}
			}
			return tooltipText;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetDataGridTableStyleDescriptionColumnIndex()
		{
			return GetDataGridTableStyleColumnIndex(WorkFlowTemplate.TemplateDescriptionColumnName);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetDataGridTableStyleNameColumnIndex()
		{
			return GetDataGridTableStyleColumnIndex(WorkFlowTemplate.TemplateNameColumnName);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetDataGridTableStyleColumnIndex(string aColumnMappingName)
		{
			if (aColumnMappingName == null || aColumnMappingName == String.Empty || WFTemplateDataGrid.TableStyles.Count == 0)
				return -1;

			DataGridTableStyle templateDataGridTableStyle = WFTemplateDataGrid.TableStyles[WorkFlowTemplate.WorkFlowTemplateTableName]; 
			if (templateDataGridTableStyle == null)
				return -1;
			
			for (int i = 0; i < templateDataGridTableStyle.GridColumnStyles.Count; i++)
			{
				if (string.Compare(templateDataGridTableStyle.GridColumnStyles[i].MappingName, aColumnMappingName) == 0)
					return i;
			}
			return -1;
		}

	}
}
