using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	public delegate void TaskBuilderSchedulerControlEventHandler(object sender, TaskBuilderSchedulerControlEventArgs e);
	public delegate void StartSchedulerAgentEventHandler(object sender, string connectionString);

	/// <summary>
	/// Summary description for TBSchedulerControl.
	/// </summary>
	//============================================================================
	public partial class TBSchedulerControl : System.Windows.Forms.UserControl
	{
		#region TasksDataGrid Class

		//=========================================================================
		public class TasksDataGrid : System.Windows.Forms.DataGrid
		{
			public event System.EventHandler DeleteSelectedTask = null;
			
			private const int minimumDataGridCodeColumnWidth = 80;
			private const int minimumDataGridStringColumnWidth = 220;
			private const int minimumDataGridBoolColumnWidth = 56;
		
			//---------------------------------------------------------------------------
			protected override void OnCreateControl()
			{	
				// Invoke base class implementation
				base.OnCreateControl();

				this.CaptionText = TaskSchedulerWindowsControlsStrings.TasksGridCaption;
				
				this.VertScrollBar.VisibleChanged += new System.EventHandler(this.VertScrollBar_VisibleChanged);
			}

			//---------------------------------------------------------------------------
			protected override void OnResize(EventArgs e)
			{	
				// Invoke base class implementation
				base.OnResize(e);

				AdjustLastColumnWidth();
			}

			//---------------------------------------------------------------------------
			// Override the OnMouseDown event to select the whole row
			// when the user clicks anywhere on a row.
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
				}
			}
			
			//---------------------------------------------------------------------------
			protected override void OnKeyUp(KeyEventArgs e)
			{
				// Invoke base class implementation
				base.OnKeyUp(e);

				if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.None)
				{
					if (DeleteSelectedTask != null)
						DeleteSelectedTask(this, new System.EventArgs());
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
			private void GridColumn_WidthChanged(object sender, System.EventArgs e)
			{
				AdjustLastColumnWidth();
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

			//--------------------------------------------------------------------------------------------------------------------------------
			private void AdjustLastColumnWidth()
			{
				if (this.TableStyles == null || this.TableStyles.Count == 0)
					return;

				// ScheduledTask.ScheduledTasksTableName is the MappingName of the DataGridTableStyle to retrieve. 
				DataGridTableStyle tasksDataGridTableStyle = this.TableStyles[WTEScheduledTask.ScheduledTasksTableName]; 

				if (tasksDataGridTableStyle != null)
				{
					int colswidth = this.RowHeaderWidth;
					for (int i = 0; i < tasksDataGridTableStyle.GridColumnStyles.Count -1; i++)
						colswidth += tasksDataGridTableStyle.GridColumnStyles[i].Width;

					int newColumnWidth = this.DisplayRectangle.Width - colswidth;
					if (this.VertScrollBar.Visible)
						newColumnWidth -= this.VertScrollBar.Width;

					DataGridColumnStyle lastColumnStyle = tasksDataGridTableStyle.GridColumnStyles[tasksDataGridTableStyle.GridColumnStyles.Count -1];
					lastColumnStyle.Width = Math.Max
						(
						minimumDataGridStringColumnWidth, 
						newColumnWidth
						);
				
					this.Refresh();
				}
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			private int GetDataGridTableStyleColumnIndex(string aColumnMappingName)
			{
				if (aColumnMappingName == null || aColumnMappingName == String.Empty || this.TableStyles.Count == 0)
					return -1;

				DataGridTableStyle tasksDataGridTableStyle = this.TableStyles[WTEScheduledTask.ScheduledTasksTableName]; 
				if (tasksDataGridTableStyle == null)
					return -1;
			
				for (int i = 0; i < tasksDataGridTableStyle.GridColumnStyles.Count; i++)
				{
					if (String.Compare(tasksDataGridTableStyle.GridColumnStyles[i].MappingName, aColumnMappingName) == 0)
						return i;
				}
				return -1;
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			private int GetDataGridTableStyleDescriptionColumnIndex()
			{
				return GetDataGridTableStyleColumnIndex(WTEScheduledTask.DescriptionColumnName);
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			private int GetDataGridTableStyleCommandColumnIndex()
			{
				return GetDataGridTableStyleColumnIndex(WTEScheduledTask.CommandColumnName);
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public void InitializeTableStyles()
			{
				this.TableStyles.Clear();

				System.Windows.Forms.DataGridTableStyle dataGridScheduledTasksStyle = new System.Windows.Forms.DataGridTableStyle();
				dataGridScheduledTasksStyle.DataGrid = this;
				dataGridScheduledTasksStyle.MappingName = WTEScheduledTask.ScheduledTasksTableName;
				dataGridScheduledTasksStyle.GridLineStyle = System.Windows.Forms.DataGridLineStyle.Solid;
				dataGridScheduledTasksStyle.RowHeadersVisible = true;
				dataGridScheduledTasksStyle.ColumnHeadersVisible = true;
				dataGridScheduledTasksStyle.HeaderFont = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				dataGridScheduledTasksStyle.PreferredRowHeight = dataGridScheduledTasksStyle.HeaderFont.Height;
				dataGridScheduledTasksStyle.PreferredColumnWidth = 100;
				dataGridScheduledTasksStyle.ReadOnly = true;
				dataGridScheduledTasksStyle.RowHeaderWidth = 12;
				dataGridScheduledTasksStyle.AlternatingBackColor = this.AlternatingBackColor;
				dataGridScheduledTasksStyle.BackColor = this.BackColor;
				dataGridScheduledTasksStyle.ForeColor = this.ForeColor;
				dataGridScheduledTasksStyle.GridLineStyle = this.GridLineStyle;
				dataGridScheduledTasksStyle.GridLineColor = this.GridLineColor;
				dataGridScheduledTasksStyle.HeaderBackColor = this.HeaderBackColor;
				dataGridScheduledTasksStyle.HeaderForeColor = this.HeaderForeColor;
				dataGridScheduledTasksStyle.SelectionBackColor = this.SelectionBackColor;
				dataGridScheduledTasksStyle.SelectionForeColor = this.SelectionForeColor;

				// Aggiungo una colonna con l'immagine che distinque una sequenza da un task semplice!!!
				TaskTypeImageDataGridColumnStyle imageColumn = new TaskTypeImageDataGridColumnStyle();
				imageColumn.MappingName = WTEScheduledTask.TypeColumnName;
				//imageColumn.HeaderText = TaskSchedulerWindowsControlsStrings.DataGridTypeColumnHeaderText;
				imageColumn.HeaderText = String.Empty;
				imageColumn.NullText = String.Empty;
				imageColumn.ReadOnly = true;
				imageColumn.Width = 38;
				imageColumn.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);
			
				dataGridScheduledTasksStyle.GridColumnStyles.Add(imageColumn);

				TaskCompletitionLevelImageDataGridColumnStyle completitionLevelImageColumn = new TaskCompletitionLevelImageDataGridColumnStyle();
				completitionLevelImageColumn.MappingName = WTEScheduledTask.LastRunCompletitionLevelColumnName;
				completitionLevelImageColumn.HeaderText = String.Empty;
				completitionLevelImageColumn.NullText = String.Empty;
				completitionLevelImageColumn.ReadOnly = true;
				completitionLevelImageColumn.Width = 38;
				completitionLevelImageColumn.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);
			
				dataGridScheduledTasksStyle.GridColumnStyles.Add(completitionLevelImageColumn);
			
				// 
				// dataGridCodeTextBox
				// 
				TaskTextBoxDataGridColumnStyle dataGridCodeTextBox = new TaskTextBoxDataGridColumnStyle();

				dataGridCodeTextBox.Alignment = System.Windows.Forms.HorizontalAlignment.Left;
				dataGridCodeTextBox.Format = "";
				dataGridCodeTextBox.FormatInfo = null;
				dataGridCodeTextBox.HeaderText = TaskSchedulerWindowsControlsStrings.DataGridCodeColumnHeaderText;
				dataGridCodeTextBox.MappingName = WTEScheduledTask.CodeColumnName;
				dataGridCodeTextBox.NullText = String.Empty;
				dataGridCodeTextBox.ReadOnly = true;
				dataGridCodeTextBox.Width = minimumDataGridCodeColumnWidth;
				dataGridCodeTextBox.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);
			
				dataGridScheduledTasksStyle.GridColumnStyles.Add(dataGridCodeTextBox);

				// 
				// dataGridDescriptionTextBox
				// 
				TaskTextBoxDataGridColumnStyle dataGridDescriptionTextBox = new TaskTextBoxDataGridColumnStyle();

				dataGridDescriptionTextBox.Alignment = System.Windows.Forms.HorizontalAlignment.Left;
				dataGridDescriptionTextBox.Format = "";
				dataGridDescriptionTextBox.FormatInfo = null;
				dataGridDescriptionTextBox.HeaderText = TaskSchedulerWindowsControlsStrings.DataGridDescriptionColumnHeaderText;
				dataGridDescriptionTextBox.MappingName = WTEScheduledTask.DescriptionColumnName;
				dataGridDescriptionTextBox.NullText = String.Empty;
				dataGridDescriptionTextBox.ReadOnly = true;
				dataGridDescriptionTextBox.Width = minimumDataGridStringColumnWidth;
				dataGridDescriptionTextBox.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);
			
				dataGridScheduledTasksStyle.GridColumnStyles.Add(dataGridDescriptionTextBox);

				// 
				// dataGridCommandTextBox
				// 
				TaskTextBoxDataGridColumnStyle dataGridCommandTextBox = new TaskTextBoxDataGridColumnStyle();

				dataGridCommandTextBox.Alignment = System.Windows.Forms.HorizontalAlignment.Left;
				dataGridCommandTextBox.Format = "";
				dataGridCommandTextBox.FormatInfo = null;
				dataGridCommandTextBox.HeaderText = TaskSchedulerWindowsControlsStrings.DataGridCommandColumnHeaderText;
				dataGridCommandTextBox.MappingName = WTEScheduledTask.CommandColumnName;
				dataGridCommandTextBox.NullText = String.Empty;
				dataGridCommandTextBox.ReadOnly = true;
				dataGridCommandTextBox.Width = minimumDataGridStringColumnWidth;
				dataGridCommandTextBox.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);
		
				dataGridScheduledTasksStyle.GridColumnStyles.Add(dataGridCommandTextBox);

				this.TableStyles.Add(dataGridScheduledTasksStyle);
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public void SetDataRowFieldsFromTask(WTEScheduledTaskObj aTask, ref System.Data.DataRow aTaskTableRow)
			{
				aTask.SetDataRowFields(ref aTaskTableRow);
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public System.Data.DataRow GetDataRowFromTask(WTEScheduledTaskObj aTaskToSearch)
			{
				if (aTaskToSearch == null || this.DataSource == null)
					return null;

				DataTable dataGridTable = (DataTable)this.DataSource;
				foreach(System.Data.DataRow dataRow in dataGridTable.Rows)
				{
					System.Guid guid = new Guid(dataRow[WTEScheduledTask.IdColumnName].ToString());
					if (guid.Equals(aTaskToSearch.Id))
						return dataRow;
				}

				return null;
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public System.Data.DataRow AddTaskRow(WTEScheduledTaskObj aTaskToAdd)
			{
				if (aTaskToAdd == null || this.DataSource == null)
					return null;

				DataTable dataGridTable = (DataTable)this.DataSource;

				System.Data.DataRow newRow = dataGridTable.NewRow();

				SetDataRowFieldsFromTask(aTaskToAdd, ref newRow);

				dataGridTable.Rows.Add(newRow);
			
				AdjustLastColumnWidth();

				return newRow;
			}

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
		
			//--------------------------------------------------------------------------------------------------------------------------------
			public void UpdateCurrentRow(WTEScheduledTaskObj aTask)
			{
				DataRow currentRow = this.CurrentRow;
				SetDataRowFieldsFromTask(aTask, ref currentRow);
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public string GetToolTipText(System.Windows.Forms.MouseEventArgs e)
			{
				System.Windows.Forms.DataGrid.HitTestInfo hitTestinfo = this.HitTest(e.X, e.Y);

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
					int commandColIdx = GetDataGridTableStyleCommandColumnIndex();
			
					if (hitTestinfo.Column == descriptionColIdx || hitTestinfo.Column == commandColIdx)
					{
						DataTable tasksdataTable = (DataTable)this.DataSource;
						if (tasksdataTable != null && hitTestinfo.Row < tasksdataTable.Rows.Count)
						{
							DataRow aRow = tasksdataTable.Rows[hitTestinfo.Row];
							if (aRow != null)
								tooltipText = (string)aRow[(hitTestinfo.Column == descriptionColIdx) ? WTEScheduledTask.DescriptionColumnName : WTEScheduledTask.CommandColumnName];
						}
					}
				}
				return tooltipText;
			}
		
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
		}
		
		#endregion

		#region SchedulerEventLogEntriesListView Class

		//=========================================================================
		public class SchedulerEventLogEntriesListView : System.Windows.Forms.ListView
		{
			private const int minimumEventLogMsgTextColumnWidth = 120;

			private System.Windows.Forms.ImageList EventLogEntriesImageList;
			private System.Windows.Forms.ContextMenu EventLogEntriesListViewContextMenu;
			private System.Windows.Forms.ToolTip EventLogEntriesListViewToolTip;

			private System.Windows.Forms.ColumnHeader EventLogMsgTypeColumnHeader = null;
			private System.Windows.Forms.ColumnHeader EventLogOriginalMsgTextColumnHeader = null;
			private System.Windows.Forms.ColumnHeader EventLogMsgTextColumnHeader = null;
			private System.Windows.Forms.ColumnHeader EventLogMsgDateColumnHeader = null;
			private System.Windows.Forms.ColumnHeader EventLogMsgTimeColumnHeader = null;
			//---------------------------------------------------------------------------
			[DllImport("advapi32.dll", CharSet=CharSet.Auto)]
			public static extern IntPtr OpenEventLog(String serverName, String sourceName); 
		
			[DllImport("advapi32.dll", CharSet=CharSet.Auto)]
			public static extern bool BackupEventLog(IntPtr eventLogHandle, String backupFileName); 
		
			[DllImport("advapi32.dll", CharSet=CharSet.Auto)]
			public static extern bool CloseEventLog(IntPtr eventLogHandle); 
		
			[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
			public static extern int GetLastError(); 

			[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
			public static extern int FormatMessage(int flags, ref IntPtr source, int messageId, int languageId, ref String buffer, int size, IntPtr arguments); 
		
			//---------------------------------------------------------------------------
			public SchedulerEventLogEntriesListView()
			{
				// 
				// EventLogEntriesImageList
				// 
				this.EventLogEntriesImageList = new System.Windows.Forms.ImageList();
				this.EventLogEntriesImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
				this.EventLogEntriesImageList.ImageSize = new System.Drawing.Size(16,16);
				this.EventLogEntriesImageList.TransparentColor = System.Drawing.Color.Magenta;

				Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps.InformationLogMessage.bmp");
				if (imageStream != null)
				EventLogEntriesImageList.Images.Add(Image.FromStream(imageStream));

				imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps.ErrorLogMessage.bmp");
				if (imageStream != null)
				EventLogEntriesImageList.Images.Add(Image.FromStream(imageStream));

				imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps.WarningLogMessage.bmp");
				if (imageStream != null)
				EventLogEntriesImageList.Images.Add(Image.FromStream(imageStream));

				this.SmallImageList = this.EventLogEntriesImageList;
	
				// 
				// EventLogEntriesListViewContextMenu
				// 
				this.EventLogEntriesListViewContextMenu = new System.Windows.Forms.ContextMenu();
				this.EventLogEntriesListViewContextMenu.RightToLeft = System.Windows.Forms.RightToLeft.No;
				this.EventLogEntriesListViewContextMenu.Popup += new System.EventHandler(this.EventLogEntriesListViewContextMenu_Popup);

				this.ContextMenu = EventLogEntriesListViewContextMenu;
	
				this.EventLogEntriesListViewToolTip = new System.Windows.Forms.ToolTip();
			}

			//---------------------------------------------------------------------------
			protected override void OnResize(EventArgs e)
			{	
				// Invoke base class implementation
				base.OnResize(e);

				AdjustColumnsWidth();
			}

			//---------------------------------------------------------------------------
			protected override void OnKeyUp(KeyEventArgs e)
			{
				// Invoke base class implementation
				base.OnKeyUp(e);

				if (this.Visible && e.KeyCode == Keys.F5 && e.Modifiers == Keys.None)
					ShowEntries();
			}
		
			//--------------------------------------------------------------------------------------------------------------------------------
			protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
			{
				// Invoke base class implementation
				base.OnMouseMove(e);

				EventLogEntriesListViewToolTip.SetToolTip(this, GetToolTipText(e));
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public void InitializeColumnHeaders()
			{
				this.Columns.Clear();

				// 
				// EventLogMsgTypeColumnHeader
				// 
				EventLogMsgTypeColumnHeader = new System.Windows.Forms.ColumnHeader();
				EventLogMsgTypeColumnHeader.Text = TaskSchedulerWindowsControlsStrings.EventLogMsgTypeColumnHeaderText;
				EventLogMsgTypeColumnHeader.Width = 120;

				this.Columns.Add(EventLogMsgTypeColumnHeader);

				// 
				// EventLogOriginalMsgTextColumnHeader
				// 
				EventLogOriginalMsgTextColumnHeader = new System.Windows.Forms.ColumnHeader();
				EventLogOriginalMsgTextColumnHeader.Text = String.Empty;
				EventLogOriginalMsgTextColumnHeader.Width = 0;

				this.Columns.Add(EventLogOriginalMsgTextColumnHeader);
			
				// 
				// EventLogMsgTextColumnHeader
				// 
				EventLogMsgTextColumnHeader = new System.Windows.Forms.ColumnHeader();
				EventLogMsgTextColumnHeader.Text = TaskSchedulerWindowsControlsStrings.EventLogMsgTextColumnHeaderText;
				EventLogMsgTextColumnHeader.Width = 200;

				this.Columns.Add(EventLogMsgTextColumnHeader);
			
				// 
				// EventLogMsgDateColumnHeader
				// 
				EventLogMsgDateColumnHeader = new System.Windows.Forms.ColumnHeader();
				EventLogMsgDateColumnHeader.Text = TaskSchedulerWindowsControlsStrings.EventLogMsgDateColumnHeaderText;
				EventLogMsgDateColumnHeader.Width = 100;
			
				this.Columns.Add(EventLogMsgDateColumnHeader);
			
				// 
				// EventLogMsgTimeColumnHeader
				// 
				EventLogMsgTimeColumnHeader = new System.Windows.Forms.ColumnHeader();
				EventLogMsgTimeColumnHeader.Text = TaskSchedulerWindowsControlsStrings.EventLogMsgTimeColumnHeaderText;
				EventLogMsgTimeColumnHeader.Width = 60;

				this.Columns.Add(EventLogMsgTimeColumnHeader);
			}
		
			//--------------------------------------------------------------------------------------------------------------------------------
			public bool ShowEntries()
			{
				this.Items.Clear();

				EventLog scheduledTaskEventLog = WTEScheduledTaskObj.GetSchedulerAgentEventLog();
			
				if (scheduledTaskEventLog == null || !WTEScheduledTaskObj.SchedulerAgentEventLogExists())
				{
					MessageBox.Show(TaskSchedulerWindowsControlsStrings.NoScheduledTasksEventLog);
					return false;
				}
			
				if (scheduledTaskEventLog.Entries == null || scheduledTaskEventLog.Entries.Count == 0)
				{
					MessageBox.Show(TaskSchedulerWindowsControlsStrings.NoEntriesInScheduledTasksEventLog);
					return true;
				}
			
				foreach (EventLogEntry entry in scheduledTaskEventLog.Entries)
				{
					// skippo gli eventi diversi da SchedulerAgent (ora l'EventViewer e' unico)
					if (string.Compare(entry.Source, WTEScheduledTask.SchedulerAgentEventLogSourceName, StringComparison.InvariantCultureIgnoreCase) != 0)
						continue;

					ListViewItem entryItem = new ListViewItem(entry.EntryType.ToString());

					switch (entry.EntryType)
					{
						case EventLogEntryType.Information:
							entryItem.ImageIndex = 0;
							break;
						case EventLogEntryType.Error:
							entryItem.ImageIndex = 1;
							break;
						case EventLogEntryType.Warning:
							entryItem.ImageIndex = 2;
							break;
					}

					entryItem.SubItems.Add(entry.Message);
					entryItem.SubItems.Add(ReplaceInvalidMessageCharacters(entry.Message));
					entryItem.SubItems.Add(entry.TimeGenerated.ToShortDateString());
					entryItem.SubItems.Add(entry.TimeGenerated.ToShortTimeString());

					// Nella collection scheduledTaskEventLog.Entries i messaggi risultano ordinati
					// cronologicamente dal più vecchio al più recente e, quindi, per averli nella lista
					// in ordine inverso (di modo che in cima ci sia il più recente), non uso il metodo
					// Add ma l'Insert.
					this.Items.Insert(0, entryItem);
				}
			
				AdjustColumnsWidth();

				return true;
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public void ClearEntries()
			{
				if (!WTEScheduledTaskObj.SchedulerAgentEventLogExists())
					return;

				try
				{
					EventLog scheduledTaskEventLog = WTEScheduledTaskObj.GetSchedulerAgentEventLog();
					if (scheduledTaskEventLog == null)
					{
						MessageBox.Show(TaskSchedulerWindowsControlsStrings.NoScheduledTasksEventLog);
						return;
					}

					if (scheduledTaskEventLog.Entries != null && scheduledTaskEventLog.Entries.Count > 0)
					{
						DialogResult dlgresult = MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.SaveScheduledTasksEventLogQuestion, scheduledTaskEventLog.LogDisplayName), TaskSchedulerWindowsControlsStrings.SaveScheduledTasksEventLogCaption, MessageBoxButtons.YesNoCancel);
						if (dlgresult == DialogResult.Cancel)
							return;
						if (dlgresult == DialogResult.Yes)
						{
							SaveFileDialog saveDlg = new SaveFileDialog();
							saveDlg.CheckPathExists = true;
							saveDlg.Title = TaskSchedulerWindowsControlsStrings.SaveScheduledTasksEventLogCaption;
							saveDlg.DefaultExt = "*.evt";
							saveDlg.Filter = TaskSchedulerWindowsControlsStrings.SaveScheduledTasksEventLogFilter;

							dlgresult = saveDlg.ShowDialog(this);
							if (dlgresult == DialogResult.Cancel)
								return;

							if (dlgresult == DialogResult.OK &&	saveDlg.FileName.Length > 0)
							{
								if (File.Exists(saveDlg.FileName))
									File.Delete(saveDlg.FileName);

								string serverName = null;
								string eventLogName = scheduledTaskEventLog.Log;
								string backupFilename = saveDlg.FileName;

								IntPtr eventLogHandle = OpenEventLog(serverName, eventLogName);
								if (eventLogHandle == IntPtr.Zero || !BackupEventLog(eventLogHandle, backupFilename))
								{
									int lastError = GetLastError();
									IntPtr ptrSource = IntPtr.Zero;
									IntPtr ptrArguments = IntPtr.Zero;
									string buffer = String.Empty;

									FormatMessage
										(
										0x00001100, // FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM
										ref ptrSource,
										lastError,
										0x0400, // MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT)
										ref buffer,
										0,
										ptrArguments
										);
									MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.BackupEventLogErrorMsg, eventLogName, backupFilename, buffer));

								}
								if (eventLogHandle != IntPtr.Zero)
									CloseEventLog(eventLogHandle);
							}
						}
					}
					
					scheduledTaskEventLog.Clear();
				
					this.Items.Clear();

					AdjustColumnsWidth();
				}
				catch(Exception exception)
				{
					MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.ClearEventLogErrorMsg, exception.Message));
				}
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public string GetToolTipText(System.Windows.Forms.MouseEventArgs e)
			{
				if (e.Clicks == 0 && this.SelectedItems != null && this.SelectedItems.Count == 1)
			{	
					if (this.ClientRectangle.IntersectsWith(this.SelectedItems[0].Bounds))
					{
						Point ptListViewMousePosition = new Point(e.X, e.Y);

						Rectangle visibleItemRect = this.ClientRectangle;
						visibleItemRect.Intersect(this.SelectedItems[0].Bounds);
						if (visibleItemRect.Contains(ptListViewMousePosition))
							return this.SelectedItems[0].SubItems[1].Text;
					}
				}
				return String.Empty;
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			private void AdjustColumnsWidth()
			{
				if (this.Columns == null || this.Columns.Count == 0 || EventLogMsgTextColumnHeader == null)
					return;

				int colswidth = 0;
				for (int i = 0; i < this.Columns.Count; i++)
					colswidth += this.Columns[i].Width;
			
				if (colswidth == this.DisplayRectangle.Width)
					return;

				int newMsgColumnWidth = EventLogMsgTextColumnHeader.Width + this.DisplayRectangle.Width - colswidth;
		
				newMsgColumnWidth = Math.Max(minimumEventLogMsgTextColumnWidth, newMsgColumnWidth);
		
				if (newMsgColumnWidth != EventLogMsgTextColumnHeader.Width)
				{
					EventLogMsgTextColumnHeader.Width = newMsgColumnWidth;

					this.PerformLayout();
				}
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			private static string ReplaceInvalidMessageCharacters(string aText)
			{
				if (aText == null || aText == String.Empty)
					return String.Empty;
			
				string newText = aText.Trim();
				newText = newText.Replace("\r\n", " ");
				newText = newText.Replace('\n', ' ');
				newText = newText.Replace('\t', ' ');
				newText = newText.Replace('\v', ' ');

				return newText;
			}
		
			#region EventLogEntriesListViewContextMenu construction and menu items Click Handlers
		
			//--------------------------------------------------------------------------------------------------------------------------------
			private void EventLogEntriesListViewContextMenu_Popup(object sender, System.EventArgs e)
			{
				EventLogEntriesListViewContextMenu.MenuItems.Clear();
				if 
					(
					!this.Visible || 
					!WTEScheduledTaskObj.SchedulerAgentEventLogExists()
					)
					return;
			
				EventLog scheduledTaskEventLog = WTEScheduledTaskObj.GetSchedulerAgentEventLog();
				if (scheduledTaskEventLog == null)
					return;

				// commentata la possibilità di eliminare gli eventi dello Scheduler:
				// avendo unificato in MA Server tutti gli eventi e NON essendo possibile eliminare una singola entry di log
				// a questo punto non deve essere possibile cancellare tutto, altrimenti vengono cancellati anche quelli di LoginManager, etc!
/*				MenuItem clearEventLogMenuItem = new System.Windows.Forms.MenuItem(); 
				clearEventLogMenuItem.Index = 0;
				clearEventLogMenuItem.Text = TaskSchedulerWindowsControlsStrings.ClearEventLogMenuItemText;
				clearEventLogMenuItem.Enabled = scheduledTaskEventLog.Entries.Count > 0;
				clearEventLogMenuItem.Click += new System.EventHandler(this.ClearEventLogMenuItem_Click);
				EventLogEntriesListViewContextMenu.MenuItems.Add(clearEventLogMenuItem);
				// Aggiungo un separatore
				EventLogEntriesListViewContextMenu.MenuItems.Add("-");
*/			
				MenuItem refreshEventLogMenuItem = new System.Windows.Forms.MenuItem(); 
				refreshEventLogMenuItem.Index = 1;
				refreshEventLogMenuItem.Text = TaskSchedulerWindowsControlsStrings.RefreshEventLogMenuItemText;
				refreshEventLogMenuItem.Click += new System.EventHandler(this.RefreshEventLogMenuItem_Click);
				EventLogEntriesListViewContextMenu.MenuItems.Add(refreshEventLogMenuItem);
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			private void ClearEventLogMenuItem_Click(object sender, System.EventArgs e)
			{
				ClearEntries();
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			private void RefreshEventLogMenuItem_Click(object sender, System.EventArgs e)
			{
				ShowEntries();
			}

			#endregion
		}

		#endregion

		private SqlConnection		currentConnection = null;
		private string				currentConnectionString = String.Empty;
		private MenuLoader			menuLoader = null;
		private bool				isTaskschedulerAgentRunning = false;
		private BrandLoader			brandLoader = null;

		private int		currentLoginId = -1;
		private string	currentUser = String.Empty;
		private int		currentCompanyId = -1;
		private string	currentCompany = String.Empty;

		private bool	userInteractionDisabled = false;
		
		public event TaskBuilderSchedulerControlEventHandler GetMenuLoaderInstance;
		public event StartSchedulerAgentEventHandler  OnStartSchedulerAgent;
		public event System.EventHandler  OnStopSchedulerAgent;
		
		//--------------------------------------------------------------------------------------
		public TBSchedulerControl(BrandLoader aBrandLoader)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			brandLoader = aBrandLoader;

			this.TasksMngPanel.Title = TaskSchedulerWindowsControlsStrings.TasksMngPanelTitle;

			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps.TaskMng.bmp");
			if (imageStream != null)
				TasksMngPanel.TitleImage = Image.FromStream(imageStream);
			
			SetPictureBoxBitmap(NewTaskPictureBox, "NewScheduledTask.bmp");
			this.NewTaskLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.NewTaskLinkLabelToolTipText;

			SetPictureBoxBitmap(NewSequencePictureBox, "NewScheduledSequence.bmp");
			this.NewTasksSequenceLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.NewTasksSequenceLinkLabelToolTipText;

			SetPictureBoxBitmap(DeleteCurrentTaskPictureBox, "DeleteCurrentTask.bmp");
			this.DeleteCurrentTaskLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.DeleteCurrentTaskLinkLabelToolTipText;

			SetPictureBoxBitmap(CloneCurrentTaskPictureBox, "CloneCurrentTask.bmp");
			this.CloneCurrentTaskLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.CloneCurrentTaskLinkLabelToolTipText;

			SetPictureBoxBitmap(ShowCurrentTaskSchedulingDetailsPictureBox, "CurrentTaskSchedulingDetails.bmp");
			this.ShowCurrentTaskSchedulingDetailsLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.ShowCurrTaskDetailsLinkLabelToolTipText;

			SetPictureBoxBitmap(CurrentTaskPropertiesPictureBox, "CurrentTaskProperties.bmp");
			this.CurrentTaskPropertiesLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.CurrTaskPropertiesLinkLabelToolTipText;

			ScheduledTasksDataGrid.InitializeTableStyles();

			this.RunTasksMngPanel.Title = TaskSchedulerWindowsControlsStrings.RunTasksMngPanelTitle;

			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps.RunTasksServiceMng.bmp");
			if (imageStream != null)
				RunTasksMngPanel.TitleImage = Image.FromStream(imageStream);
			
			//RunTasksMngPanel.Collapse();

			SetPictureBoxBitmap(RunCurrentTaskPictureBox, "RunCurrentTask.bmp");
			this.RunCurrentTaskLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.RunCurrentTaskLinkLabelToolTipText;

			SetPictureBoxBitmap(StartStopSchedulerAgentPictureBox, "SchedulerAgentServiceMng.bmp");
			this.StartStopSchedulerAgentLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.StartStopSchedulerAgentLToolTipText;

			SetPictureBoxBitmap(ShowEventLogEntriesPictureBox, "ShowLogMessage.bmp");
			this.ShowEventLogEntriesLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.ShowEventLogEntriesLinkLabelToolTipText;

			SetPictureBoxBitmap(AdjustTasksNextRunDatePictureBox, "AdjustNextRunDate.bmp");
			this.AdjustTasksNextRunDateLinkLabel.ToolTipText = TaskSchedulerWindowsControlsStrings.AdjustTasksNextRunDateToolTipText;

			EventLogEntriesListView.InitializeColumnHeaders();

			ShowEventLogEntriesListView(false);
		}

		//--------------------------------------------------------------------------------------
		public TBSchedulerControl() : this(null)
		{
		}

		#region TBSchedulerControl public properties

		//--------------------------------------------------------------------------------------
		public bool IsLiteConsole { get; set; }
		
		//--------------------------------------------------------------------------------------
		public MenuLoader MenuLoader { get { return menuLoader; } set { menuLoader = value; } }
	
		//--------------------------------------------------------------------------------------------------------------------------------
		// Devo necessariamente salvare in un campo la stringa di connessione originale: se si salvasse solo
		// la connessione poi, nel caso in cui sia prevista una password, avrei dei problemi ad ottenere da
		// essa la stringa di connessione corretta e, quindi, a riutilizzare tale stringa per effettuare nuove
		// connessioni. Infatti, la proprietà ConnectionString di SqlConnection "taglia" questa informazione
		// (a meno che la stringa usata per connettersi contenga anche "persist security info=True")
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public string ConnectionString 
		{
			get{ return currentConnectionString; } 
			set
			{
				try
				{
					CloseConnection();

					currentConnectionString = value;
					if (currentConnectionString == null || currentConnectionString == String.Empty)
						return;

					currentConnection = new SqlConnection(currentConnectionString);
					
					// The Open method uses the information in the ConnectionString
					// property to contact the data source and establish an open connection
					currentConnection.Open();
				}
				catch (SqlException e)
				{
					MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.ConnectionErrorMsgFmt, e.Message));

					currentConnection = null;
					currentConnectionString = String.Empty;
				}
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsConnectionOpen { get{ return (currentConnection != null) && ((currentConnection.State & ConnectionState.Open) == ConnectionState.Open); } }

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public BrandLoader BrandLoader { get { return brandLoader; } set { brandLoader = value; }}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public bool UserInteractionDisabled { get { return userInteractionDisabled; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsXEngineExtensionLicensed
		{
			get
			{
				if (menuLoader == null || menuLoader.LoginManager == null)
					return false;

				return menuLoader.LoginManager.IsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.XEngine);
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsMailConnectorLicensed
		{
			get
			{
				if (menuLoader == null || menuLoader.LoginManager == null)
					return false;

				return menuLoader.LoginManager.IsActivated(NameSolverStrings.Extensions, DatabaseLayerConsts.MailConnector);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsEasyLookLicensed
		{
			get
			{
				if (menuLoader == null || menuLoader.LoginManager == null)
					return false;

				return menuLoader.LoginManager.IsActivated(DatabaseLayerConsts.WebFramework, DatabaseLayerConsts.EasyLook);
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsSQLServerDB
		{
			get
			{
				if (menuLoader == null || menuLoader.LoginManager == null)
					return false;

				string dbType = menuLoader.LoginManager.GetProviderNameFromCompanyId(this.currentCompanyId);
               
				return
                    (String.Compare(dbType, NameSolverDatabaseStrings.SQLOLEDBProvider, true, CultureInfo.InvariantCulture) == 0 || String.Compare(dbType, NameSolverDatabaseStrings.SQLODBCProvider, true, CultureInfo.InvariantCulture) == 0);
			}
		}

		#endregion	

		#region TBSchedulerControl private properties

		//--------------------------------------------------------------------------------------------------------------------------------
		private DataRow CurrentScheduledTasksDataGridRow { get { return ScheduledTasksDataGrid.CurrentRow; } }
		
		#endregion	

		#region TBSchedulerControl public methods

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool SetCurrentAuthentication(int loginId, int companyId)
		{
			currentLoginId = -1;
			currentUser = String.Empty;
			currentCompanyId = -1;
			currentCompany = String.Empty;
			
			if (!IsConnectionOpen || !WTEScheduledTaskObj.GetLoginDataFromIds(currentConnection, companyId, loginId, out currentCompany, out currentUser))
				return false;

			currentLoginId = loginId;
			currentCompanyId = companyId;

			FillScheduledTasksGrid();
			
			return true;
		}
		
		//--------------------------------------------------------------------------------------------------------
		public void CloseConnection()
		{
			if (currentConnection != null)
			{
				if (IsConnectionOpen)
					currentConnection.Close();
			
				currentConnection.Dispose();
			}

			currentConnection = null;
			currentConnectionString = String.Empty;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public System.Data.DataRow AddTaskRowToScheduledTasksDataGrid(WTEScheduledTaskObj aTaskToAdd)
		{
			return ScheduledTasksDataGrid.AddTaskRow(aTaskToAdd);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void UpdateStartStopSchedulerAgentStatus(bool runningAgent)
		{
			isTaskschedulerAgentRunning = runningAgent;
			StartStopSchedulerAgentLinkLabel.Text = isTaskschedulerAgentRunning ? TaskSchedulerWindowsControlsStrings.StopRunTasksLabelText : TaskSchedulerWindowsControlsStrings.StartRunTasksLabelText;
		}

		#endregion	

		#region TBSchedulerControl private methods

		//--------------------------------------------------------------------------------------
		private void CheckMenuLoader()
		{
			IMessageFilter aFilter = null;
			try
			{
				aFilter = DisableUserInteraction();
			
				if (menuLoader == null && GetMenuLoaderInstance != null)
					GetMenuLoaderInstance(this, new TaskBuilderSchedulerControlEventArgs(currentUser, currentCompany));
			}
			catch(Exception exception)
			{
				// ATTENZIONE !!!
				// Qui occorre necessariamente catturare possibili eccezioni, predisporre
				// cioè un blocco try-catch- finally, altrimenti continuerebbe a restare
				// disabilitata qualunque interazione da parte del'utente...
				
				Debug.Fail("Exception thrown in TBSchedulerControl.CheckMenuLoader: " + exception.Message);
			}
			finally
			{
				RestoreUserInteraction(aFilter);
			}
		}

		//--------------------------------------------------------------------------------------
		private void SetPictureBoxBitmap(System.Windows.Forms.PictureBox pictureBox, string bitmapResourceName)
		{
			Stream bitmapStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps." + bitmapResourceName);
			if (bitmapStream != null)
			{
				System.Drawing.Bitmap bitmap = new Bitmap(bitmapStream);
				if (bitmap != null)
				{
					bitmap.MakeTransparent(Color.Magenta);
					pictureBox.Image = bitmap;
				}
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void ClearScheduledTasksGrid()
		{
			ScheduledTasksDataGrid.Clear();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void FillScheduledTasksGrid()
		{
			ClearScheduledTasksGrid();

			if (!IsConnectionOpen)
				return;

			SqlDataAdapter selectAllSqlDataAdapter = new SqlDataAdapter(WTEScheduledTask.GetSelectAllTasksOrderedByCodeQuery(currentConnection, currentCompanyId, currentLoginId));
					
			DataTable tasksDataTable = new DataTable(WTEScheduledTask.ScheduledTasksTableName);

			selectAllSqlDataAdapter.Fill(tasksDataTable);
			
			ScheduledTasksDataGrid.DataSource = tasksDataTable;

			UpdateCurrentTaskOperationsStatus();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private WTEScheduledTaskObj GetSelectedTask()
		{
			if (CurrentScheduledTasksDataGridRow == null)
				return null;

			return new WTEScheduledTaskObj(CurrentScheduledTasksDataGridRow, currentConnectionString);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool RunCurrentTaskOnDemand()
		{
			if (!IsConnectionOpen || ScheduledTasksDataGrid.DataSource == null || ScheduledTasksDataGrid.CurrentRowIndex < 0)
				return false;

			WTEScheduledTaskObj task = GetSelectedTask();

			if (task == null || !task.ToRunOnDemand)
				return false;

			try
			{
				WTETaskExecutionEngine.RunTaskThread runningThread = WTETaskExecutionEngine.StartRunTaskThread(task, currentConnection, currentConnectionString);

				if (runningThread == null)
					return false;

				runningThread.OnTaskExecutionEnded += new  System.EventHandler(this.TaskOnDemandExecutionEnded);

				if (runningThread.IsAlive)
				{
					// Ricarico dal database i dati relativi al task in modo che risultino aggiornati (flag di stato)
					task.RefreshData(currentConnectionString, true);
				
					ScheduledTasksDataGrid.UpdateCurrentRow(task);

					UpdateCurrentTaskOperationsStatus();

					this.Refresh();
				}

				return true;
			}
			catch(ScheduledTaskException exception)
			{
				MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.RunCurrentTaskOnDemandFailedErrMsgFmt, exception.ExtendedMessage));
				return false;
			}
		}
		
		public event WTETaskExecutionEngine.TaskExecutionEndedEventHandler TaskExecutionEnded = null;
        delegate void TaskOnDemandExecutionEndedCallback(object sender, System.EventArgs e);
        //--------------------------------------------------------------------------------------------------------------------------------
		private void TaskOnDemandExecutionEnded(object sender, System.EventArgs e)
		{
            // InvokeRequired required compares the thread ID of the calling thread to the 
            // thread ID of the creating thread. If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                TaskOnDemandExecutionEndedCallback d = new TaskOnDemandExecutionEndedCallback(TaskOnDemandExecutionEnded);
                this.Invoke(d, new object[] { sender , e});
            }
            else
            {
                if (sender == null || !(sender is WTETaskExecutionEngine.RunTaskThread))
                    return;

                if (((WTETaskExecutionEngine.RunTaskThread)sender).Task != null)
                {
                    System.Data.DataRow taskDataRow = ScheduledTasksDataGrid.GetDataRowFromTask(((WTETaskExecutionEngine.RunTaskThread)sender).Task);
                    if (taskDataRow != null)
                    {
                        ScheduledTasksDataGrid.SetDataRowFieldsFromTask(((WTETaskExecutionEngine.RunTaskThread)sender).Task, ref taskDataRow);

                        UpdateCurrentTaskOperationsStatus();

                        this.Refresh();
                    }
                }

                if (TaskExecutionEnded != null)
                    TaskExecutionEnded(this, new WTETaskExecutionEngine.TaskExecutionEndedEventArgs((WTETaskExecutionEngine.RunTaskThread)sender));
            }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RemoveCurrentScheduledTask()
		{
			if (userInteractionDisabled || !IsConnectionOpen || ScheduledTasksDataGrid.DataSource == null || ScheduledTasksDataGrid.CurrentRowIndex < 0)
				return;
			
			WTEScheduledTaskObj task = GetSelectedTask();
			if (task == null)
				return;

			if (MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.ConfirmTaskDeletionMsg, task.Code), TaskSchedulerWindowsControlsStrings.ConfirmTaskDeletionCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				return;


			if (ScheduledSequencesEngine.IsInSequenceInvolved(currentConnectionString, task.Id))
			{
				MessageBox.Show(TaskSchedulerWindowsControlsStrings.CannotDeleteTaskInSequenceWarningMsg, TaskSchedulerWindowsControlsStrings.CannotDeleteTaskCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}


			if (WTEScheduledTask.Delete(currentConnectionString, task.Id, task.IsSequence))
			{
                task.DeleteRelatedFiles(currentConnection);
                ScheduledTasksDataGrid.RemoveCurrentRow();
				UpdateCurrentTaskOperationsStatus();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CloneCurrentScheduledTask()
		{
			if (userInteractionDisabled || !IsConnectionOpen || ScheduledTasksDataGrid.DataSource == null || ScheduledTasksDataGrid.CurrentRowIndex < 0)
				return;

            WTEScheduledTaskObj task = GetSelectedTask();

			if (task == null)
				return;

			SetNewTaskCodeDlg setNewTaskCodeDlg = new SetNewTaskCodeDlg(currentCompanyId, currentLoginId, currentConnectionString);
			setNewTaskCodeDlg.ShowDialog(this);

			if (setNewTaskCodeDlg.DialogResult != DialogResult.OK || setNewTaskCodeDlg.NewCode == null || setNewTaskCodeDlg.NewCode == String.Empty)
				return;

            WTEScheduledTaskObj newTask = new WTEScheduledTaskObj(task, false);
			newTask.SetCode(setNewTaskCodeDlg.NewCode, currentConnection);
			newTask.SetNewId(); // cambio l'id del nuovo task clonato in modo che risulti differente dall'originale

			try
			{
				if (newTask.Insert(currentConnection, newTask))
				{
					System.Data.DataRow addedRow = AddTaskRowToScheduledTasksDataGrid(newTask);
					if (addedRow != null)
					{
						ScheduledTasksDataGrid.CurrentRow = addedRow;
						UpdateCurrentTaskOperationsStatus();
					}
				}
			}
			catch(ScheduledTaskException exception)
			{
				MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.InsertTaskFailedErrorMsgFmt, exception.Message));
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void ClearLastRunCompletitionLevel()
		{
			if
				(
				userInteractionDisabled ||
				!IsConnectionOpen ||
				ScheduledTasksDataGrid.DataSource == null ||
				ScheduledTasksDataGrid.CurrentRowIndex < 0
				)
				return;

            WTEScheduledTaskObj task = GetSelectedTask();
			if (task == null)
				return;
            if (task.IsRunning)
            {
                if (MessageBox.Show(
                    TaskSchedulerWindowsControlsStrings.TaskIsRunning,
                    TaskSchedulerWindowsControlsStrings.ChangeTaskState,
                    MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;
            }

			task.SaveSuccesStatus(currentConnection);
			ScheduledTasksDataGrid.UpdateCurrentRow(task);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ShowCurrentScheduledTaskProperties()
		{
			if 
				(
				userInteractionDisabled ||
				!IsConnectionOpen ||
				ScheduledTasksDataGrid.DataSource == null || 
				ScheduledTasksDataGrid.CurrentRowIndex < 0
				)
				return;

            WTEScheduledTaskObj task = GetSelectedTask();
			if (task == null)
				return;

			CheckMenuLoader();

			if (task.IsSequence)
			{
				TasksSequencesPropertiesForm currentSequencePropertiesForm = new TasksSequencesPropertiesForm(ref task, this);
				currentSequencePropertiesForm.Text = TaskSchedulerWindowsControlsStrings.ExistingTasksSequencePropertiesDlgTitle;
				
				currentSequencePropertiesForm.ShowDialog(this);

				if (currentSequencePropertiesForm.DialogResult != DialogResult.OK)
					return;
			}
			else
			{
				TaskPropertiesForm currentTaskPropertiesForm = new TaskPropertiesForm(ref task, this);
				currentTaskPropertiesForm.Text = TaskSchedulerWindowsControlsStrings.ExistingTaskPropertiesDlgTitle;

				currentTaskPropertiesForm.ShowDialog(this);

				if (currentTaskPropertiesForm.DialogResult != DialogResult.OK)
					return;
			}
			if (task.Update(currentConnection, true))
			{
				ScheduledTasksDataGrid.UpdateCurrentRow(task);
				UpdateCurrentTaskOperationsStatus();
			}

            try
            {
                task.SaveCommandParametersToFile(currentConnection);
            }
            catch (ScheduledTaskException exception)
            {
                throw exception;
            }

            if (task.IsSequence && !task.SaveTasksInSequence(currentConnection))
                return;

            TaskNotificationRecipientEngine.SaveMailNotificationsSettings(currentConnection,task.Id, task.notificationRecipients);

        }

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ShowCurrentScheduledTaskDetails()
		{
			if (!IsConnectionOpen || ScheduledTasksDataGrid.DataSource == null || ScheduledTasksDataGrid.CurrentRowIndex < 0)
				return;

            WTEScheduledTaskObj task = GetSelectedTask();
			if (task == null)
				return;

			// Ricarico dal database i dati relativi al task in modo che risultino aggiornati (flag di stato)
			task.RefreshData(currentConnectionString, true);

			TaskDetailsForm currentTaskDetailsForm = new TaskDetailsForm(task);
			currentTaskDetailsForm.ShowDialog(this);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ShowEventLogEntriesListView(bool show)
		{
			if (EventLogEntriesListView.Visible == show)
				return;

			if (!show)
			{
				EventLogEntriesListView.Visible = false;
				ScheduledTasksDataGrid.Dock = DockStyle.Fill;
				ScheduledTasksDataGrid.Height = TBSchedulerPanel.Height - RightHorizontalSplitter.Height - RightHorizontalSplitter.MinSize;
			}
			else
			{
				ScheduledTasksDataGrid.Dock = DockStyle.Top;
				EventLogEntriesListView.Visible = true;
			}
			ScheduledTasksDataGrid.Invalidate(ScheduledTasksDataGrid.ClientRectangle, true);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool ShowScheduledTasksEventLogEntries()
		{
			if (EventLogEntriesListView == null)
				return false;

			return EventLogEntriesListView.ShowEntries();
		}

        delegate void UpdateCurrentTaskOperationsStatusCallback();
        //--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateCurrentTaskOperationsStatus()
		{
            // InvokeRequired required compares the thread ID of the calling thread to the 
            // thread ID of the creating thread. If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                UpdateCurrentTaskOperationsStatusCallback d = new UpdateCurrentTaskOperationsStatusCallback(UpdateCurrentTaskOperationsStatus);
                this.Invoke(d, null);
            }
            else
            {
                WTEScheduledTaskObj currentTask = GetSelectedTask();
                if (currentTask != null)
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
            }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateScheduledTasksDataGridSelection()
		{
			if (IsConnectionOpen && ScheduledTasksDataGrid.DataSource != null && ScheduledTasksDataGrid.CurrentRowIndex >= 0)
			{
				Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
				Point dataGridMousePosition = ScheduledTasksDataGrid.PointToClient(mousePosition);

				System.Windows.Forms.DataGrid.HitTestInfo hitTestinfo = ScheduledTasksDataGrid.HitTest(dataGridMousePosition);

				if (hitTestinfo.Type == DataGrid.HitTestType.Cell || hitTestinfo.Type == DataGrid.HitTestType.RowHeader)
				{
					ScheduledTasksDataGrid.CurrentRowIndex = hitTestinfo.Row;
					UpdateCurrentTaskOperationsStatus();
					this.Refresh();
				}
			}
		}
		
		#endregion
		
		#region TBSchedulerControl protected overridden methods

		//---------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{	
			// Invoke base class implementation
			base.OnVisibleChanged(e);

			ShowEventLogEntriesLinkLabel.Enabled = WTEScheduledTaskObj.SchedulerAgentEventLogExists();		
		}

		#endregion

		#region ScheduledTasksContextMenu construction and menu items Click Handlers
		//--------------------------------------------------------------------------------------------------------------------------------
		private void ScheduledTasksContextMenu_Popup(object sender, System.EventArgs e)
		{
			ScheduledTasksContextMenu.MenuItems.Clear();

			if (!IsConnectionOpen || ScheduledTasksDataGrid.DataSource == null || ScheduledTasksDataGrid.CurrentRowIndex < 0)
				return;
			
			Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
			Point dataGridMousePosition = ScheduledTasksDataGrid.PointToClient(mousePosition);

			Rectangle rectCurrentCell = ScheduledTasksDataGrid.GetCurrentCellBounds();
			if (dataGridMousePosition.Y < rectCurrentCell.Top || dataGridMousePosition.Y > rectCurrentCell.Bottom)
				return;

            WTEScheduledTaskObj task = GetSelectedTask();

			if (task == null)
				return;

			if (task.Enabled && task.ToRunOnDemand)
			{
				System.Windows.Forms.MenuItem runTaskOnDemandMenuItem = new System.Windows.Forms.MenuItem(); 
				runTaskOnDemandMenuItem.Index = 0;

				runTaskOnDemandMenuItem.Text = TaskSchedulerWindowsControlsStrings.RunTaskMenuItemText;
				runTaskOnDemandMenuItem.Enabled = !task.IsRunning;

				runTaskOnDemandMenuItem.Click += new System.EventHandler(this.RunTaskOnDemandMenuItem_Click);

				ScheduledTasksContextMenu.MenuItems.Add(runTaskOnDemandMenuItem);

				// Aggiungo un separatore
				ScheduledTasksContextMenu.MenuItems.Add("-");
			}

			System.Windows.Forms.MenuItem removeTaskMenuItem = new System.Windows.Forms.MenuItem(); 
			removeTaskMenuItem.Index = ScheduledTasksContextMenu.MenuItems.Count;

			removeTaskMenuItem.Text = TaskSchedulerWindowsControlsStrings.RemoveTaskMenuItemText;

			removeTaskMenuItem.Click += new System.EventHandler(this.RemoveTaskMenuItem_Click);

			ScheduledTasksContextMenu.MenuItems.Add(removeTaskMenuItem);

			System.Windows.Forms.MenuItem cloneTaskMenuItem = new System.Windows.Forms.MenuItem(); 
			cloneTaskMenuItem.Index = ScheduledTasksContextMenu.MenuItems.Count;

			cloneTaskMenuItem.Text = TaskSchedulerWindowsControlsStrings.CloneTaskMenuItemText;

			cloneTaskMenuItem.Click += new System.EventHandler(this.CloneTaskMenuItem_Click);

			ScheduledTasksContextMenu.MenuItems.Add(cloneTaskMenuItem);

			// Aggiungo un separatore
			ScheduledTasksContextMenu.MenuItems.Add("-");

			// Aggiungo una voce di menù per la visualizzazione dei vari dettagli riguardanti la sua
			// schedulazione, quali la data dell'ultima esecuzione, se essa ha avuto o meno successo, ecc.
			System.Windows.Forms.MenuItem taskDetailsMenuItem = new System.Windows.Forms.MenuItem(); 
			taskDetailsMenuItem.Index = ScheduledTasksContextMenu.MenuItems.Count;

			taskDetailsMenuItem.Text = TaskSchedulerWindowsControlsStrings.TaskDetailsMenuItemText;

			taskDetailsMenuItem.Click += new System.EventHandler(this.TaskDetailsMenuItem_Click);

			ScheduledTasksContextMenu.MenuItems.Add(taskDetailsMenuItem);

			System.Windows.Forms.MenuItem clearLastRunCompletitionLevel = new System.Windows.Forms.MenuItem();
			clearLastRunCompletitionLevel.Index = ScheduledTasksContextMenu.MenuItems.Count;

			clearLastRunCompletitionLevel.Text = TaskSchedulerWindowsControlsStrings.ClearLastRunCompletitionLevelText;
			clearLastRunCompletitionLevel.Enabled = (CompletitionLevelEnum)(Int16)task.LastRunCompletitionLevel != CompletitionLevelEnum.Success;

			clearLastRunCompletitionLevel.Click += new System.EventHandler(this.ClearLastRunCompletitionLevel_Click);

			ScheduledTasksContextMenu.MenuItems.Add(clearLastRunCompletitionLevel);

			// Aggiungo una voce di menù per la visualizzazione delle proprietà del task dando
			// anche modo all'utente di modificarle
			System.Windows.Forms.MenuItem taskPropertiesMenuItem = new System.Windows.Forms.MenuItem(); 
			taskPropertiesMenuItem.Index = ScheduledTasksContextMenu.MenuItems.Count;

			taskPropertiesMenuItem.Text = TaskSchedulerWindowsControlsStrings.TaskPropertiesMenuItemText;

			taskPropertiesMenuItem.Click += new System.EventHandler(this.TaskPropertiesMenuItem_Click);

			ScheduledTasksContextMenu.MenuItems.Add(taskPropertiesMenuItem);
		}
		
		#region ScheduledTasksContextMenu Click Event Handlers

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RunTaskOnDemandMenuItem_Click(object sender, System.EventArgs e)
		{
			RunCurrentTaskOnDemand();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RemoveTaskMenuItem_Click(object sender, System.EventArgs e)
		{
			RemoveCurrentScheduledTask();
		}

		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void CloneTaskMenuItem_Click(object sender, System.EventArgs e)
		{
			CloneCurrentScheduledTask();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void TaskPropertiesMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowCurrentScheduledTaskProperties();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void ClearLastRunCompletitionLevel_Click(object sender, System.EventArgs e)
		{
			ClearLastRunCompletitionLevel();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void TaskDetailsMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowCurrentScheduledTaskDetails();
		}
		#endregion

		#endregion

		#region Tasks Management LinkLabels Click Event Handlers
		//--------------------------------------------------------------------------------------------------------------------------------
		private void NewTaskLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (userInteractionDisabled || !IsConnectionOpen)
				return;

			CheckMenuLoader();

            WTEScheduledTaskObj newTask = new WTEScheduledTaskObj(currentCompanyId, currentLoginId);
			TaskPropertiesForm newTaskForm = new TaskPropertiesForm(ref newTask, this);

			newTaskForm.Text = TaskSchedulerWindowsControlsStrings.NewTaskDlgTitle;
			
			newTaskForm.ShowDialog(this);

			if (newTaskForm.DialogResult != DialogResult.OK)
				return;
			
			try
			{
				if (WTEScheduledTask.Insert(currentConnection, newTask))
				{
					System.Data.DataRow addedRow = AddTaskRowToScheduledTasksDataGrid(newTask);
					if (addedRow != null)
					{
						ScheduledTasksDataGrid.CurrentRow = addedRow;
						UpdateCurrentTaskOperationsStatus();
					}
				}
			}
			catch(ScheduledTaskException exception)
			{
				MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.InsertTaskFailedErrorMsgFmt, exception.Message));
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void NewTasksSequenceLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (userInteractionDisabled || !IsConnectionOpen)
				return;

			CheckMenuLoader();

            WTEScheduledTaskObj newSequence = new WTEScheduledTaskObj(currentCompanyId, currentLoginId);
			newSequence.IsSequence = true;
			TasksSequencesPropertiesForm newSequenceForm = new TasksSequencesPropertiesForm(ref newSequence, this);
			newSequenceForm.Text = TaskSchedulerWindowsControlsStrings.NewTasksSequenceDlgTitle;
			newSequenceForm.ShowDialog(this);

			if (newSequenceForm.DialogResult != DialogResult.OK)
				return;
			
			try
			{
				if (WTEScheduledTask.Insert(currentConnection, newSequence))
				{
					System.Data.DataRow addedRow = AddTaskRowToScheduledTasksDataGrid(newSequence);
					if (addedRow != null)
					{
						ScheduledTasksDataGrid.CurrentRow = addedRow;
						UpdateCurrentTaskOperationsStatus();
					}
				}
			}
			catch(ScheduledTaskException exception)
			{
				MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.InsertTasksSequenceFailedErrorMsgFmt, exception.Message));
			}
		}

		//---------------------------------------------------------------------------
		private void DeleteCurrentTaskLinkLabel_Click(object sender, System.EventArgs e)
		{
			RemoveCurrentScheduledTask();
		}

		//---------------------------------------------------------------------------
		private void CloneCurrentTaskLinkLabel_Click(object sender, System.EventArgs e)
		{
			CloneCurrentScheduledTask();
		}

		//---------------------------------------------------------------------------
		private void ShowCurrentTaskSchedulingDetailsLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ShowCurrentScheduledTaskDetails();
		}

		//---------------------------------------------------------------------------
		private void CurrentTaskPropertiesLinkLabel_Click(object sender, System.EventArgs e)
		{
			ShowCurrentScheduledTaskProperties();
		}
	
		#endregion

		#region ScheduledTasksDataGrid Event Handlers

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ScheduledTasksDataGrid_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (TBSchedulerControlToolTip == null)
				return;

			ScheduledTasksDataGrid.GetToolTipText(e);

			TBSchedulerControlToolTip.SetToolTip(ScheduledTasksDataGrid, ScheduledTasksDataGrid.GetToolTipText(e));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ScheduledTasksDataGrid_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				UpdateScheduledTasksDataGridSelection();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ScheduledTasksDataGrid_DeleteSelectedTask(object sender, System.EventArgs e)
		{
			RemoveCurrentScheduledTask();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ScheduledTasksDataGrid_CurrentCellChanged(object sender, System.EventArgs e)
		{
			UpdateScheduledTasksDataGridSelection();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ScheduledTasksDataGrid_DoubleClick(object sender, System.EventArgs e)
		{
			if (!IsConnectionOpen || ScheduledTasksDataGrid.DataSource == null || ScheduledTasksDataGrid.CurrentRowIndex < 0)
				return;

			Point mousePosition = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
			Point dataGridMousePosition = ScheduledTasksDataGrid.PointToClient(mousePosition);

			System.Windows.Forms.DataGrid.HitTestInfo hitTestinfo = ScheduledTasksDataGrid.HitTest(dataGridMousePosition);

			if (hitTestinfo.Type == DataGrid.HitTestType.None || hitTestinfo.Row != ScheduledTasksDataGrid.CurrentRowIndex)
				return;

			ShowCurrentScheduledTaskProperties();
		}

		#endregion

		#region Tasks Execution LinkLabels Click Event Handlers

		//---------------------------------------------------------------------------
		private void RunCurrentTaskLinkLabel_Click(object sender, System.EventArgs e)
		{
			RunCurrentTaskOnDemand();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void StartStopSchedulerAgentLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (isTaskschedulerAgentRunning)
			{
				if (OnStopSchedulerAgent != null)
					OnStopSchedulerAgent(this, null);
			}
			else if (OnStartSchedulerAgent != null)
				OnStartSchedulerAgent(this, currentConnectionString);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ShowEventLogEntriesLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{					
			ShowEventLogEntriesListView(!EventLogEntriesListView.Visible);

			if (EventLogEntriesListView.Visible)
			{
				if (!ShowScheduledTasksEventLogEntries())
				{
					ShowEventLogEntriesListView(false);
					ShowEventLogEntriesLinkLabel.Enabled = false;
					return;
				}
				ShowEventLogEntriesLinkLabel.Text = TaskSchedulerWindowsControlsStrings.HideEventLogEntriesLinkLabelText;
			}
			else
				ShowEventLogEntriesLinkLabel.Text = TaskSchedulerWindowsControlsStrings.ShowEventLogEntriesLinkLabelText;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AdjustTasksNextRunDateLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (MessageBox.Show(TaskSchedulerWindowsControlsStrings.AdjustTasksNextRunDateMsg, TaskSchedulerWindowsControlsStrings.AdjustTasksNextRunDateCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				return;

			try
			{
                WTEScheduledTask.AdjustTasksNextRunDateIfNecessary(currentConnectionString);
			}
			catch(ScheduledTaskException exception)
			{
				MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.AdjustTasksNextRunDateFailedErrMsgFmt, exception.ExtendedMessage));
			}
		}

		#endregion	

		#region TBSchedulerControl static methods


		#endregion
		
		#region UserInputMessageFilter class and disabling user input 
		//--------------------------------------------------------------------------------------------------------------------------------
		// In certi casi si rende assolutamente necessario inibire qualsiasi possibile interazione 
		// con l'applicazione da parte dell'utente.
		// Ad esempio, il metodo che gestisce l'evento GetMenuLoaderInstance scatena a sua volta  
		// degli eventi (v. nel caso della PlugIn di MicroareaConsole la visualizzazione della 
		// ProgressBar). Se, mentre si stanno caricando le informazioni relative al menù, l'utente
		// preme un tasto o clicca su un control presente nella finestra della console, viene 
		// lanciato il comando corrispondente e vengono di conseguenza scatenati gli eventi relativi.
		// Purtroppo, dalle prove fatte la disabiltazione della form principale della console non
		// risulta essere sufficiente: i comandi vengono inviati comunque!
		// Pertanto, si rende necessario l'utilizzo di un filtro sui messaggi che impedisce
		// l'arrivo di messaggi all'applicazione causati da una qualunque interazione con essa
		// da parte dell'utente.
		// Questo filtro va altresì applicato nel caso di apertura "modale" di TBLoader come
		// nel caso dell'impostazione dei parametri di lancio di una batch o di un report.
		//============================================================================
		private class UserInputMessageFilter : IMessageFilter 
		{
			private System.Windows.Forms.Cursor currentConsoleFormCursor = null;
			
			//--------------------------------------------------------------------------------------------------------------------------------
			public UserInputMessageFilter(System.Windows.Forms.Cursor aCursor)
			{
				currentConsoleFormCursor = aCursor;
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public System.Windows.Forms.Cursor CurrentConsoleFormCursor { get { return currentConsoleFormCursor; } }
			
			//--------------------------------------------------------------------------------------------------------------------------------
			public bool PreFilterMessage(ref Message message) 
			{
				return	
					message.Msg == 273 || // Blocks all the WM_COMMAND messages.
					message.Msg == 256 || // Blocks all the WM_KEYDOWN messages.
					message.Msg == 257 || // Blocks all the WM_KEYUP messages.
					(message.Msg >= 160 && message.Msg <= 173) || // Blocks all the messages relating to the mouse buttons within the nonclient area (WM_NC...).
					(message.Msg >= 512 && message.Msg <= 525);// Blocks all the messages relating to the mouse buttons.
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public IMessageFilter DisableUserInteraction()
		{
			if (userInteractionDisabled)
				return null;

			userInteractionDisabled = true;

			// Ricavo il corrente cursore della form della console e lo salvo
			// per poterlo poi riassegnare in seguito, una volta terminata l'elaborazione
			Cursor currentConsoleFormCursor = this.TopLevelControl.Cursor;

			UserInputMessageFilter aFilter = new UserInputMessageFilter(currentConsoleFormCursor);
			
			this.TopLevelControl.Cursor = Cursors.WaitCursor;

			Cursor.Current = Cursors.WaitCursor;

			Application.AddMessageFilter(aFilter); // Adds a message filter to monitor Windows messages as they are routed to their destinations.

			return aFilter;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RestoreUserInteraction(IMessageFilter aFilter)
		{
			if (aFilter == null || !userInteractionDisabled)
				return;
				
			Application.RemoveMessageFilter(aFilter); // Removes a message filter from the message pump of the application.

			// Se non si "ripulisce" la coda dai messaggi arrivati in fase di esecuzione del metodo che 
			// gestisce l'evento GetMenuLoaderInstance, si possono riscontrare degli effetto indesiderati. 
			// Ad esempio, se durante il caricamento delle informazioni relative al menù l'utente clicca 
			// sulla finestra della console e poi subito dopo si visualizza una dialog box, quest'ultima 
			// non è modale.
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			// Chiamando Application.DoEvents prima di reimpostare il cursore a Cursors.Default, l'applicazione
			// si rimetterà all'ascolto degli eventi del mouse e visualizzerà il cursore appropriato per ciascun
			// controllo.
			Application.DoEvents();

			if ((aFilter is UserInputMessageFilter) && ((UserInputMessageFilter)aFilter).CurrentConsoleFormCursor != null)
				this.TopLevelControl.Cursor = ((UserInputMessageFilter)aFilter).CurrentConsoleFormCursor;

			// Set Cursor.Current to Cursors.Default to display the appropriate cursor for each control
			Cursor.Current = Cursors.Default;
	
			userInteractionDisabled = false;
		}

		#endregion

	}

	//============================================================================
	public class TaskBuilderSchedulerControlEventArgs : EventArgs
	{
		public string User = String.Empty;
		public string Company = String.Empty;

		//---------------------------------------------------------------------------
		public TaskBuilderSchedulerControlEventArgs(string aUser, string aCompany)
		{
			User = aUser;
			Company = aCompany;
		}

		//---------------------------------------------------------------------------
		public TaskBuilderSchedulerControlEventArgs(): this(String.Empty, String.Empty)
		{
		}
	}

	//=========================================================================
  	public class TaskTypeImageDataGridColumnStyle : DataGridColumnStyle 
	{
		private System.Windows.Forms.ImageList taskTypesImageList = null;
		
		//---------------------------------------------------------------------
		public TaskTypeImageDataGridColumnStyle() 
		{
			taskTypesImageList = new System.Windows.Forms.ImageList();
			taskTypesImageList.TransparentColor = Color.Magenta;
			taskTypesImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			taskTypesImageList.ImageSize = new Size(24, 24);

			AddImageToTaskTypesImageList("TaskGridColumnValue.bmp");		// 0
			AddImageToTaskTypesImageList("SequenceGridColumnValue.bmp");	// 1
			AddImageToTaskTypesImageList("ScheduledBatch.bmp");				// 2
			AddImageToTaskTypesImageList("ScheduledReport.bmp");			// 3
			AddImageToTaskTypesImageList("ScheduledExecutable.bmp");		// 4
			AddImageToTaskTypesImageList("ScheduledFunction.bmp");			// 5
			AddImageToTaskTypesImageList("ScheduledMail.bmp");				// 6
			AddImageToTaskTypesImageList("ScheduledWebPage.bmp");			// 7
		}
		
		//---------------------------------------------------------------------
		private bool AddImageToTaskTypesImageList(string bitmapFile) 
		{
			Stream bitmapStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps." + bitmapFile);
			if (bitmapStream == null)
				return false;
				
			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(bitmapStream);

			bitmap.MakeTransparent(Color.Magenta);

			taskTypesImageList.Images.Add(bitmap);

			return true;
		}

		//---------------------------------------------------------------------
		private System.Drawing.Image GetTypeImage(CurrencyManager source,int rowNumber) 
		{
			TaskTypeEnum taskType = (TaskTypeEnum)GetColumnValueAtRow(source, rowNumber);

			if (taskType == TaskTypeEnum.Sequence) 
				return taskTypesImageList.Images[1];
			
			if (taskType == TaskTypeEnum.Batch) 
				return taskTypesImageList.Images[2];

			if (taskType == TaskTypeEnum.Report) 
				return taskTypesImageList.Images[3];
			
			if (taskType == TaskTypeEnum.Executable) 
				return taskTypesImageList.Images[4];

			if (taskType == TaskTypeEnum.Function) 
				return taskTypesImageList.Images[5];

			if (taskType == TaskTypeEnum.Mail) 
				return taskTypesImageList.Images[6];

			if (taskType == TaskTypeEnum.WebPage) 
				return taskTypesImageList.Images[7];

			return taskTypesImageList.Images[0];
		}

		//---------------------------------------------------------------------
		protected override void Abort(int rowNumber) 
		{
		}

		//---------------------------------------------------------------------
		protected override bool Commit(CurrencyManager source,int rowNumber) 
		{
			return true;
		}

		//---------------------------------------------------------------------
		protected override void Edit
			(
			CurrencyManager source,
			int				rowNumber,
			Rectangle		bounds, 
			bool			readOnly,
			string			instantText, 
			bool			cellIsVisible
			) 
		{
		}

		//---------------------------------------------------------------------
		protected override int GetMinimumHeight() 
		{
			return 16;
		}

		//---------------------------------------------------------------------
		protected override int GetPreferredHeight(Graphics graphics, object objectValue) 
		{
			return 16;
		}

		//---------------------------------------------------------------------
		protected override Size GetPreferredSize(Graphics graphics, object objectValue) 
		{
			return new Size(16,16);
		}

		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics		graphics,
			Rectangle		bounds,
			CurrencyManager	source,
			int				rowNumber
			) 
		{
			Paint(graphics, bounds, source, rowNumber, new SolidBrush(SystemColors.Window), null, false);
		}

		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics		graphics,
			Rectangle		bounds,
			CurrencyManager	source,
			int				rowNumber,
			bool			alignToRight
			) 
		{
			Paint(graphics, bounds, source, rowNumber, new SolidBrush(SystemColors.Window), null, alignToRight);
		}

		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics		graphics,
			Rectangle		bounds,
			CurrencyManager	source,
			int				rowNumber,
			Brush			backBrush,
			Brush			foreBrush,
			bool			alignToRight
			) 
		{
			graphics.FillRectangle(backBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);

			System.Drawing.Image image = GetTypeImage(source, rowNumber);
			if (image == null)
				return;

			System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
			destRect.Inflate(1,1);

			if (image.Width > destRect.Width || image.Height > destRect.Height)
			{
				int destImageWidth = (image.Width * destRect.Height)/image.Height;
				if (destImageWidth < destRect.Width)
				{
					destRect.Offset((destRect.Width - destImageWidth)/2, 0);
					destRect.Width = destImageWidth;
				}
			}
			else
			{
				destRect.Offset((destRect.Width - image.Width)/2, (destRect.Height - image.Height)/2);
				destRect.Width = image.Width;
				destRect.Height = image.Height;
			}
			
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel);
		}
	
	}
	
	//=========================================================================
	public class TaskCompletitionLevelImageDataGridColumnStyle : DataGridColumnStyle 
	{
		private System.Windows.Forms.ImageList taskCompletitionLevelsImageList = null;
		
		//---------------------------------------------------------------------
		public TaskCompletitionLevelImageDataGridColumnStyle() 
		{
			taskCompletitionLevelsImageList = new System.Windows.Forms.ImageList();
			taskCompletitionLevelsImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			taskCompletitionLevelsImageList.ImageSize = new Size(24, 24);
			taskCompletitionLevelsImageList.TransparentColor = Color.Magenta;

			AddImageTotaskCompletitionLevelsImageList("Success.bmp");					// 0
			AddImageTotaskCompletitionLevelsImageList("Failure.bmp");					// 1
			AddImageTotaskCompletitionLevelsImageList("Running.bmp");					// 2
			AddImageTotaskCompletitionLevelsImageList("WaitForNextRetryAttempt.bmp");	// 3
			AddImageTotaskCompletitionLevelsImageList("Aborted.bmp");					// 4
			AddImageTotaskCompletitionLevelsImageList("SequenceInterrupted.bmp");		// 5
			AddImageTotaskCompletitionLevelsImageList("SequencePartialSuccess.bmp");	// 6
			AddImageTotaskCompletitionLevelsImageList("ClosedProcess.bmp");				// 7
		}
		
		//---------------------------------------------------------------------
		private bool AddImageTotaskCompletitionLevelsImageList(string imageFile) 
		{
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps." + imageFile);
			if (imageStream == null)
				return false;
				
			System.Drawing.Bitmap image = System.Drawing.Bitmap.FromStream(imageStream) as System.Drawing.Bitmap;

			if (image == null)
				return false;

			taskCompletitionLevelsImageList.Images.Add(image);

			return true;
		}

		//---------------------------------------------------------------------
		private System.Drawing.Image GetCompletitionLevelImage(CurrencyManager source,int rowNumber) 
		{
			object columnValue = GetColumnValueAtRow(source, rowNumber);

			if (columnValue == null || !(columnValue is Int16))
				return null;
			
			CompletitionLevelEnum taskCompletitionLevel = (CompletitionLevelEnum)(Int16)columnValue;

			if 
				(
				this.DataGridTableStyle != null &&
				this.DataGridTableStyle.DataGrid != null && 
				this.DataGridTableStyle.DataGrid.DataSource != null && 
				this.DataGridTableStyle.DataGrid.DataSource is DataTable
				)
			{
				DataTable tasksdataTable = (DataTable)this.DataGridTableStyle.DataGrid.DataSource;
			
				if (rowNumber >= 0 && rowNumber < tasksdataTable.Rows.Count)
				{
					DataRow currentTaskRow = tasksdataTable.Rows[rowNumber];

					if 
						(
						currentTaskRow != null && 
						currentTaskRow.RowState != DataRowState.Deleted &&
						(TaskTypeEnum)currentTaskRow[WTEScheduledTask.TypeColumnName] == TaskTypeEnum.Sequence
						)
					{
						if (taskCompletitionLevel == CompletitionLevelEnum.SequenceInterrupted) 
							return taskCompletitionLevelsImageList.Images[5];
			
						if (taskCompletitionLevel == CompletitionLevelEnum.SequencePartialSuccess) 
							return taskCompletitionLevelsImageList.Images[6];
					}
				}
			}

			if (taskCompletitionLevel == CompletitionLevelEnum.Success) 
				return taskCompletitionLevelsImageList.Images[0];
			
			if (taskCompletitionLevel == CompletitionLevelEnum.Failure) 
				return taskCompletitionLevelsImageList.Images[1];

			if (taskCompletitionLevel == CompletitionLevelEnum.Running) 
				return taskCompletitionLevelsImageList.Images[2];

			if (taskCompletitionLevel == CompletitionLevelEnum.WaitForNextRetryAttempt) 
				return taskCompletitionLevelsImageList.Images[3];

			if (taskCompletitionLevel == CompletitionLevelEnum.Aborted) 
				return taskCompletitionLevelsImageList.Images[4];

			if (taskCompletitionLevel == CompletitionLevelEnum.SequenceInterrupted) 
				return taskCompletitionLevelsImageList.Images[5];
			
			if (taskCompletitionLevel == CompletitionLevelEnum.SequencePartialSuccess) 
				return taskCompletitionLevelsImageList.Images[6];
			
			if (taskCompletitionLevel == CompletitionLevelEnum.ClosedProcess) 
				return taskCompletitionLevelsImageList.Images[7];

			return null;
		}

		//---------------------------------------------------------------------
		protected override void Abort(int rowNumber) 
		{
		}

		//---------------------------------------------------------------------
		protected override bool Commit(CurrencyManager source,int rowNumber) 
		{
			return true;
		}

		//---------------------------------------------------------------------
		protected override void Edit
			(
			CurrencyManager source,
			int				rowNumber,
			Rectangle		bounds, 
			bool			readOnly,
			string			instantText, 
			bool			cellIsVisible
			) 
		{
		}

		//---------------------------------------------------------------------
		protected override int GetMinimumHeight() 
		{
			return 16;
		}

		//---------------------------------------------------------------------
		protected override int GetPreferredHeight(Graphics graphics, object objectValue) 
		{
			return 16;
		}

		//---------------------------------------------------------------------
		protected override Size GetPreferredSize(Graphics graphics, object objectValue) 
		{
			return new Size(16,16);
		}

		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics		graphics,
			Rectangle		bounds,
			CurrencyManager	source,
			int				rowNumber
			) 
		{
			Paint(graphics, bounds, source, rowNumber, new SolidBrush(SystemColors.Window), null, false);
		}

		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics		graphics,
			Rectangle		bounds,
			CurrencyManager	source,
			int				rowNumber,
			bool			alignToRight
			) 
		{
			Paint(graphics, bounds, source, rowNumber, new SolidBrush(SystemColors.Window), null, alignToRight);
		}

		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics		graphics,
			Rectangle		bounds,
			CurrencyManager	source,
			int				rowNumber,
			Brush			backBrush,
			Brush			foreBrush,
			bool			alignToRight
			) 
		{
			graphics.FillRectangle(backBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);

			System.Drawing.Image image = GetCompletitionLevelImage(source, rowNumber);
			if (image == null)
				return;

			System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
			destRect.Inflate(1,1);

			if (image.Width > destRect.Width || image.Height > destRect.Height)
			{
				int destImageWidth = (image.Width * destRect.Height)/image.Height;
				if (destImageWidth < destRect.Width)
				{
					destRect.Offset((destRect.Width - destImageWidth)/2, 0);
					destRect.Width = destImageWidth;
				}
			}
			else
			{
				destRect.Offset((destRect.Width - image.Width)/2, (destRect.Height - image.Height)/2);
				destRect.Width = image.Width;
				destRect.Height = image.Height;
			}
			
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel);
		}
	
	}

	//=========================================================================
	public class TaskTextBoxDataGridColumnStyle  : DataGridTextBoxColumn 
	{
		//---------------------------------------------------------------------
		protected override void Paint
			(
			Graphics graphics,
			Rectangle bounds,
			CurrencyManager source,
			int rowNumber,
			Brush backBrush,
			Brush foreBrush,
			bool alignToRight
			)
		{
			
			if
				(
				this.DataGridTableStyle != null &&
				this.DataGridTableStyle.DataGrid != null &&
				this.DataGridTableStyle.DataGrid.DataSource != null &&
				this.DataGridTableStyle.DataGrid.DataSource is DataTable
				)
			{
				DataTable tasksdataTable = (DataTable)this.DataGridTableStyle.DataGrid.DataSource;

				if (rowNumber >= 0 && rowNumber < tasksdataTable.Rows.Count)
				{
					DataRow currentTaskRow = ((DataRowView)source.List[rowNumber]).Row;

					if
						(
						currentTaskRow != null &&
						currentTaskRow.RowState != DataRowState.Deleted &&
						!(bool)currentTaskRow[WTEScheduledTask.EnabledColumnName]
						)
					{
						using (Brush b = new System.Drawing.SolidBrush(System.Drawing.SystemColors.GrayText))
							base.Paint(graphics, bounds, source, rowNumber, backBrush, b, alignToRight);
						return;
					}
			
				}
			}

			base.Paint(graphics, bounds, source, rowNumber, backBrush, foreBrush, alignToRight);
		}
	}	
}
