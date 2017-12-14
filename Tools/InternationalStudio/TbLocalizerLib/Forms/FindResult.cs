using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Summary description for FindResult.
	/// </summary>
	//=========================================================================
	public class FindResult : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;
		private MenuItem		MiOpenTranslator;
		private ContextMenu		ContextMenuAction;
		public	string			Separator;
		private ArrayList itemList = new ArrayList();
		private System.Windows.Forms.DataGrid ListViewResult;
		private System.Windows.Forms.DataGridTableStyle findResultTableStyle;
		private MarkTextColumnStyle ColumnBase;
		private MarkTextColumnStyle ColumnTarget;
		private System.Data.DataSet findDataSet;
		private System.Data.DataTable findDataTable;
		private System.Data.DataView findDataView;
		private System.Data.DataColumn dataColumnBase;
		private System.Data.DataColumn dataColumnTarget;
		private System.Windows.Forms.DataGridTextBoxColumn ColumnName;
		private System.Data.DataColumn dataColumnName;
		private System.Data.DataColumn dataColumnData;
		private System.Data.DataColumn dataColumnReplaced;
	
		private System.Windows.Forms.DataGridBoolColumn ColumnReplaceAccepted;
		private System.Data.DataColumn dataColumnReplaceAccepted;
		private System.Data.DataColumn dataColumnOriginal;
				
		bool canResizeDataGrid = false;
			
		private bool translatorOpen = false;

		private bool supportSearch;
		private bool targetSearch;
		private bool matchCase;
		private bool matchWord;
		private bool useRegex;
		private string searchString;
		private string replaceString;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.CheckBox ckbKeepOpen;
		private DictionaryCreator				owner;
		private System.Windows.Forms.MenuItem MiOriginalValues;
		private System.Windows.Forms.MenuItem MiNewValues;
		private Regex searchExpression, newValueExpression;
		private System.Data.DataColumn dataColumnResourceName;

		//--------------------------------------------------------------------------------
		public ArrayList ItemList {get {return itemList;} set {itemList = value;}}
		//--------------------------------------------------------------------------------
		public string DataGridReplaceColumn 
		{
			get { return TargetModified ? dataColumnTarget.ColumnName : dataColumnBase.ColumnName; }
		}

		//--------------------------------------------------------------------------------
		public bool FindOnlyMode
		{
			get { return replaceString == null; }
		}

		//--------------------------------------------------------------------------------
		public bool TargetModified
		{
			get { return targetSearch || FindOnlyMode; }
		}

		//--------------------------------------------------------------------------------
		public Regex SearchExpression 
		{
			get
			{
				if (searchExpression != null) return searchExpression;
				
				searchExpression = CommonFunctions.GetSplitExpression(searchString, matchCase, matchWord, useRegex);
				return searchExpression;
			}
		}
		
		//--------------------------------------------------------------------------------
		public Regex NewValueExpression 
		{
			get
			{
				if (newValueExpression != null) return newValueExpression;
				
				newValueExpression = CommonFunctions.GetSplitExpression(replaceString, matchCase, matchWord, useRegex);
				return newValueExpression;
			}
		}

		//--------------------------------------------------------------------------------
		public new DictionaryCreator Owner  { get { return owner; } set { this.owner = value; base.Owner = this.owner.Owner; }}
		
		//---------------------------------------------------------------------
		public FindResult
			(
			ArrayList list,
			string separator,
			bool supportSearch,
			bool targetSearch,
			string searchString,
			string replaceString,
			bool matchCase,
			bool matchWord, 
			bool useRegex
			)
		{		
			InitializeComponent();
			
			ItemList			= list;
			Separator			= separator;
			this.supportSearch	= supportSearch;
			this.targetSearch	= targetSearch;
			this.searchString	= searchString;
			this.replaceString	= replaceString;
			this.matchCase		= matchCase;
			this.matchWord		= matchWord;
			this.useRegex		= useRegex;

			PostInitializeComponent();
		}

		//---------------------------------------------------------------------
		public void PostInitializeComponent()
		{	
			MarkTextColumnStyle	modifyColumn = targetSearch ? ColumnTarget : ColumnBase;

			if (FindOnlyMode)			// executed when operation is "find only"
			{
				ColumnReplaceAccepted.Width = 0;
				modifyColumn.NewValueExpression = SearchExpression;
				modifyColumn.SearchExpression = SearchExpression;
				ColumnTarget.ReadOnly = false;
				btnSave.Enabled = false;
			}
			else								// executed when operation is "find and replace"
			{
				modifyColumn.ReadOnly = false;
				modifyColumn.NewValueExpression = NewValueExpression;
				modifyColumn.SearchExpression = SearchExpression;
			}
			
			ColumnBase.HeaderText = supportSearch ? Strings.SupportLanguageCaption : Strings.BaseLanguageCaption;
			ColumnTarget.HeaderText = Strings.TargetLanguageCaption;

			MiOpenTranslator.Visible = (ItemList != null && ItemList.Count > 0);

			foreach (FinderInfo info in ItemList)
			{
				for (int i = 0; i < info.TranslationInfos.Count; i++)
				{
					TranslationInfo translation = (TranslationInfo) info.TranslationInfos[i];
				
					DataRow r = findDataTable.NewRow();
					r[dataColumnName.ColumnName] = info.NodeTree.FullPath;
					r[dataColumnBase.ColumnName] = supportSearch ? translation.SupportString : translation.BaseString;
					r[dataColumnTarget.ColumnName] = translation.TargetString;
					r[dataColumnResourceName.ColumnName] = translation.NameString;
					r[dataColumnData.ColumnName] = info.NodeTree;
					r[dataColumnOriginal.ColumnName] = TargetModified 
						? translation.TargetString 
						: supportSearch 
						? translation.SupportString : translation.BaseString;
					
					r[dataColumnReplaced.ColumnName] = Replace(((string)r[dataColumnOriginal.ColumnName]));
					r[dataColumnReplaceAccepted.ColumnName] = !FindOnlyMode;

					if (!FindOnlyMode) 
						ToggleTranslation(r, true);

					findDataTable.Rows.Add(r); 
			
				}
			}	

			ToggleEvent(true);

			canResizeDataGrid = true;
		}

		//--------------------------------------------------------------------------------
		private void SetDataGridDimensionProcedure()
		{
			if (!canResizeDataGrid) return;
		
			try
			{	
				canResizeDataGrid = false;
				//setto anchor prima di resettare la dimensione per evitare che venga visualizzato male
				ListViewResult.Anchor = AnchorStyles.Left| AnchorStyles.Right | AnchorStyles.Top;
				
				DataGridRowHeightSetter rowHeightSetter = new DataGridRowHeightSetter(ListViewResult);

				// Sets g to a graphics object representing the drawing surface of the control or form g is a member of.
				// Setto l'altezzza della riga in modo che si veda una riga e mezzo, anche in base al dpi setting
				Graphics g = ListViewResult.CreateGraphics();
			
				double totalRowHeight = 0;
				FindAndReplaceInfos fri = new FindAndReplaceInfos();

				for (int i = 0; i < findDataTable.Rows.Count; i++)
				{
					SetStandardFindAndReplaceInfos(findDataTable.Rows[i], fri);
					
					SizeF size = g.MeasureString(fri.BaseString, ListViewResult.Font, ColumnBase.Width);
				
					double baseHeight = size.Height;

					size = g.MeasureString(fri.TargetString, ListViewResult.Font, ColumnTarget.Width);
				
					double targetHeight = size.Height;

					double max = Math.Max(baseHeight, targetHeight);

					rowHeightSetter[i] = (int) Math.Round(max, 0);
					totalRowHeight += max;

				}
			
			}
			catch(Exception ex)
			{
				Debug.Fail(ex.Message, ex.StackTrace);
			}
			finally
			{
				//resetto anchor ai valori corretti per visualizzare il DgTranslator in maniera corretta
				ListViewResult.Anchor = AnchorStyles.Bottom | AnchorStyles.Left| AnchorStyles.Top| AnchorStyles.Right;
				canResizeDataGrid = true;
			}
		}		

		//--------------------------------------------------------------------------------
		private void ToggleEvent(bool setEvent)
		{	
			if (!FindOnlyMode) return;

			if (setEvent)
				findDataTable.RowChanged += new DataRowChangeEventHandler(FindDataTable_RowChanged);			
			else
				findDataTable.RowChanged -= new DataRowChangeEventHandler(FindDataTable_RowChanged);			

		}

		//--------------------------------------------------------------------------------
		private string Replace(string originalValue)
		{
			if (originalValue == null || replaceString == null) return originalValue;

			return SearchExpression.Replace(originalValue, replaceString);
		}

		//---------------------------------------------------------------------
		private void MiOpenTranslator_Click(object sender, System.EventArgs e)
		{
			OpenTranslator();
		}

		//---------------------------------------------------------------------
		private void ListViewResult_DoubleClick(object sender, System.EventArgs e)
		{
			OpenTranslator();
		}

		//---------------------------------------------------------------------
		private void OpenTranslator()
		{
			if (ListViewResult.CurrentRowIndex  == -1) return;
			DataRowView r = findDataView[ListViewResult.CurrentRowIndex];
			TreeNode nodeToShow = (TreeNode)r[dataColumnData.ColumnName];
			if (nodeToShow == null) return;
			
			Owner.ProjectsTree.SelectedNode = nodeToShow; 
			
			FindAndReplaceInfos infos = new FindAndReplaceInfos();
			SetStandardFindAndReplaceInfos(r.Row, infos);

			Translator t = Owner.ShowTranslator(infos);
			if (t != null)
				PositionNearTranslator(t);
			
		}

		//--------------------------------------------------------------------------------
		private void PositionNearTranslator(Translator t)
		{
			t.Top = t.Left = 0;
			if (t.Bottom + Height > Screen.FromControl(t).WorkingArea.Height)
			{
				int h = t.MaximumSize.Height - Height;
				if (h > 0)
				{
					Size s = new Size(t.MaximumSize.Width, h);
					t.MaximumSize = s;
					t.PerformLayout();
				}
				
			}
			
			Top = t.Bottom;
			Left = t.Left;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if (components != null)
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FindResult));
			this.ContextMenuAction = new System.Windows.Forms.ContextMenu();
			this.MiOpenTranslator = new System.Windows.Forms.MenuItem();
			this.MiOriginalValues = new System.Windows.Forms.MenuItem();
			this.MiNewValues = new System.Windows.Forms.MenuItem();
			this.ListViewResult = new System.Windows.Forms.DataGrid();
			this.findDataView = new System.Data.DataView();
			this.findDataTable = new System.Data.DataTable();
			this.dataColumnBase = new System.Data.DataColumn();
			this.dataColumnTarget = new System.Data.DataColumn();
			this.dataColumnName = new System.Data.DataColumn();
			this.dataColumnData = new System.Data.DataColumn();
			this.dataColumnReplaced = new System.Data.DataColumn();
			this.dataColumnReplaceAccepted = new System.Data.DataColumn();
			this.dataColumnOriginal = new System.Data.DataColumn();
			this.dataColumnResourceName = new System.Data.DataColumn();
			this.findResultTableStyle = new System.Windows.Forms.DataGridTableStyle();
			this.ColumnName = new System.Windows.Forms.DataGridTextBoxColumn();
			this.ColumnReplaceAccepted = new System.Windows.Forms.DataGridBoolColumn();
			this.ColumnBase = new Microarea.Tools.TBLocalizer.Forms.MarkTextColumnStyle();
			this.ColumnTarget = new Microarea.Tools.TBLocalizer.Forms.MarkTextColumnStyle();
			this.findDataSet = new System.Data.DataSet();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.ckbKeepOpen = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.ListViewResult)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.findDataView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.findDataTable)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.findDataSet)).BeginInit();
			this.SuspendLayout();
			// 
			// ContextMenuAction
			// 
			this.ContextMenuAction.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							  this.MiOpenTranslator,
																							  this.MiOriginalValues,
																							  this.MiNewValues});
			this.ContextMenuAction.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ContextMenuAction.RightToLeft")));
			// 
			// MiOpenTranslator
			// 
			this.MiOpenTranslator.Enabled = ((bool)(resources.GetObject("MiOpenTranslator.Enabled")));
			this.MiOpenTranslator.Index = 0;
			this.MiOpenTranslator.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("MiOpenTranslator.Shortcut")));
			this.MiOpenTranslator.ShowShortcut = ((bool)(resources.GetObject("MiOpenTranslator.ShowShortcut")));
			this.MiOpenTranslator.Text = resources.GetString("MiOpenTranslator.Text");
			this.MiOpenTranslator.Visible = ((bool)(resources.GetObject("MiOpenTranslator.Visible")));
			this.MiOpenTranslator.Click += new System.EventHandler(this.MiOpenTranslator_Click);
			// 
			// MiOriginalValues
			// 
			this.MiOriginalValues.Enabled = ((bool)(resources.GetObject("MiOriginalValues.Enabled")));
			this.MiOriginalValues.Index = 1;
			this.MiOriginalValues.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("MiOriginalValues.Shortcut")));
			this.MiOriginalValues.ShowShortcut = ((bool)(resources.GetObject("MiOriginalValues.ShowShortcut")));
			this.MiOriginalValues.Text = resources.GetString("MiOriginalValues.Text");
			this.MiOriginalValues.Visible = ((bool)(resources.GetObject("MiOriginalValues.Visible")));
			this.MiOriginalValues.Click += new System.EventHandler(this.MiOriginalValues_Click);
			// 
			// MiNewValues
			// 
			this.MiNewValues.Enabled = ((bool)(resources.GetObject("MiNewValues.Enabled")));
			this.MiNewValues.Index = 2;
			this.MiNewValues.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("MiNewValues.Shortcut")));
			this.MiNewValues.ShowShortcut = ((bool)(resources.GetObject("MiNewValues.ShowShortcut")));
			this.MiNewValues.Text = resources.GetString("MiNewValues.Text");
			this.MiNewValues.Visible = ((bool)(resources.GetObject("MiNewValues.Visible")));
			this.MiNewValues.Click += new System.EventHandler(this.MiNewValues_Click);
			// 
			// ListViewResult
			// 
			this.ListViewResult.AccessibleDescription = resources.GetString("ListViewResult.AccessibleDescription");
			this.ListViewResult.AccessibleName = resources.GetString("ListViewResult.AccessibleName");
			this.ListViewResult.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ListViewResult.Anchor")));
			this.ListViewResult.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ListViewResult.BackgroundImage")));
			this.ListViewResult.CaptionFont = ((System.Drawing.Font)(resources.GetObject("ListViewResult.CaptionFont")));
			this.ListViewResult.CaptionText = resources.GetString("ListViewResult.CaptionText");
			this.ListViewResult.ContextMenu = this.ContextMenuAction;
			this.ListViewResult.DataMember = "";
			this.ListViewResult.DataSource = this.findDataView;
			this.ListViewResult.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ListViewResult.Dock")));
			this.ListViewResult.Enabled = ((bool)(resources.GetObject("ListViewResult.Enabled")));
			this.ListViewResult.Font = ((System.Drawing.Font)(resources.GetObject("ListViewResult.Font")));
			this.ListViewResult.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ListViewResult.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ListViewResult.ImeMode")));
			this.ListViewResult.Location = ((System.Drawing.Point)(resources.GetObject("ListViewResult.Location")));
			this.ListViewResult.Name = "ListViewResult";
			this.ListViewResult.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ListViewResult.RightToLeft")));
			this.ListViewResult.Size = ((System.Drawing.Size)(resources.GetObject("ListViewResult.Size")));
			this.ListViewResult.TabIndex = ((int)(resources.GetObject("ListViewResult.TabIndex")));
			this.ListViewResult.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
																									   this.findResultTableStyle});
			this.ListViewResult.Visible = ((bool)(resources.GetObject("ListViewResult.Visible")));
			this.ListViewResult.Click += new System.EventHandler(this.ListViewResult_Click);
			this.ListViewResult.DoubleClick += new System.EventHandler(this.ListViewResult_DoubleClick);
			this.ListViewResult.Layout += new System.Windows.Forms.LayoutEventHandler(this.ListViewResult_Layout);
			// 
			// findDataView
			// 
			this.findDataView.AllowDelete = false;
			this.findDataView.AllowNew = false;
			this.findDataView.Table = this.findDataTable;
			// 
			// findDataTable
			// 
			this.findDataTable.Columns.AddRange(new System.Data.DataColumn[] {
																				 this.dataColumnBase,
																				 this.dataColumnTarget,
																				 this.dataColumnName,
																				 this.dataColumnData,
																				 this.dataColumnReplaced,
																				 this.dataColumnReplaceAccepted,
																				 this.dataColumnOriginal,
																				 this.dataColumnResourceName});
			this.findDataTable.TableName = "FindDataTable";
			// 
			// dataColumnBase
			// 
			this.dataColumnBase.ColumnName = "BaseColumn";
			// 
			// dataColumnTarget
			// 
			this.dataColumnTarget.ColumnName = "TargetColumn";
			// 
			// dataColumnName
			// 
			this.dataColumnName.ColumnName = "NameColumn";
			// 
			// dataColumnData
			// 
			this.dataColumnData.ColumnName = "DataColumn";
			this.dataColumnData.DataType = typeof(object);
			// 
			// dataColumnReplaced
			// 
			this.dataColumnReplaced.ColumnName = "ReplacedColumn";
			// 
			// dataColumnReplaceAccepted
			// 
			this.dataColumnReplaceAccepted.ColumnName = "ReplaceAcceptedColumn";
			this.dataColumnReplaceAccepted.DataType = typeof(bool);
			// 
			// dataColumnOriginal
			// 
			this.dataColumnOriginal.ColumnName = "OriginalColumn";
			// 
			// dataColumnResourceName
			// 
			this.dataColumnResourceName.ColumnName = "ResourceName";
			// 
			// findResultTableStyle
			// 
			this.findResultTableStyle.DataGrid = this.ListViewResult;
			this.findResultTableStyle.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
																												   this.ColumnName,
																												   this.ColumnReplaceAccepted,
																												   this.ColumnBase,
																												   this.ColumnTarget});
			this.findResultTableStyle.HeaderFont = ((System.Drawing.Font)(resources.GetObject("findResultTableStyle.HeaderFont")));
			this.findResultTableStyle.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.findResultTableStyle.MappingName = "FindDataTable";
			this.findResultTableStyle.PreferredColumnWidth = ((int)(resources.GetObject("findResultTableStyle.PreferredColumnWidth")));
			this.findResultTableStyle.PreferredRowHeight = ((int)(resources.GetObject("findResultTableStyle.PreferredRowHeight")));
			this.findResultTableStyle.RowHeaderWidth = ((int)(resources.GetObject("findResultTableStyle.RowHeaderWidth")));
			// 
			// ColumnName
			// 
			this.ColumnName.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ColumnName.Alignment")));
			this.ColumnName.Format = "";
			this.ColumnName.FormatInfo = null;
			this.ColumnName.HeaderText = resources.GetString("ColumnName.HeaderText");
			this.ColumnName.MappingName = resources.GetString("ColumnName.MappingName");
			this.ColumnName.NullText = resources.GetString("ColumnName.NullText");
			this.ColumnName.ReadOnly = true;
			this.ColumnName.Width = ((int)(resources.GetObject("ColumnName.Width")));
			// 
			// ColumnReplaceAccepted
			// 
			this.ColumnReplaceAccepted.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ColumnReplaceAccepted.Alignment")));
			this.ColumnReplaceAccepted.AllowNull = false;
			this.ColumnReplaceAccepted.FalseValue = false;
			this.ColumnReplaceAccepted.HeaderText = resources.GetString("ColumnReplaceAccepted.HeaderText");
			this.ColumnReplaceAccepted.MappingName = resources.GetString("ColumnReplaceAccepted.MappingName");
			this.ColumnReplaceAccepted.NullText = resources.GetString("ColumnReplaceAccepted.NullText");
			this.ColumnReplaceAccepted.NullValue = ((object)(resources.GetObject("ColumnReplaceAccepted.NullValue")));
			this.ColumnReplaceAccepted.TrueValue = true;
			this.ColumnReplaceAccepted.Width = ((int)(resources.GetObject("ColumnReplaceAccepted.Width")));
			// 
			// ColumnBase
			// 
			this.ColumnBase.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ColumnBase.Alignment")));
			this.ColumnBase.Format = "";
			this.ColumnBase.FormatInfo = null;
			this.ColumnBase.HeaderText = resources.GetString("ColumnBase.HeaderText");
			this.ColumnBase.MappingName = resources.GetString("ColumnBase.MappingName");
			this.ColumnBase.NullText = resources.GetString("ColumnBase.NullText");
			this.ColumnBase.ReadOnly = true;
			this.ColumnBase.Width = ((int)(resources.GetObject("ColumnBase.Width")));
			// 
			// ColumnTarget
			// 
			this.ColumnTarget.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ColumnTarget.Alignment")));
			this.ColumnTarget.Format = "";
			this.ColumnTarget.FormatInfo = null;
			this.ColumnTarget.HeaderText = resources.GetString("ColumnTarget.HeaderText");
			this.ColumnTarget.MappingName = resources.GetString("ColumnTarget.MappingName");
			this.ColumnTarget.NullText = resources.GetString("ColumnTarget.NullText");
			this.ColumnTarget.ReadOnly = true;
			this.ColumnTarget.Width = ((int)(resources.GetObject("ColumnTarget.Width")));
			// 
			// findDataSet
			// 
			this.findDataSet.DataSetName = "FindDataSet";
			this.findDataSet.Locale = new System.Globalization.CultureInfo("it-IT");
			this.findDataSet.Tables.AddRange(new System.Data.DataTable[] {
																			 this.findDataTable});
			// 
			// btnSave
			// 
			this.btnSave.AccessibleDescription = resources.GetString("btnSave.AccessibleDescription");
			this.btnSave.AccessibleName = resources.GetString("btnSave.AccessibleName");
			this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSave.Anchor")));
			this.btnSave.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSave.BackgroundImage")));
			this.btnSave.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSave.Dock")));
			this.btnSave.Enabled = ((bool)(resources.GetObject("btnSave.Enabled")));
			this.btnSave.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSave.FlatStyle")));
			this.btnSave.Font = ((System.Drawing.Font)(resources.GetObject("btnSave.Font")));
			this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
			this.btnSave.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSave.ImageAlign")));
			this.btnSave.ImageIndex = ((int)(resources.GetObject("btnSave.ImageIndex")));
			this.btnSave.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSave.ImeMode")));
			this.btnSave.Location = ((System.Drawing.Point)(resources.GetObject("btnSave.Location")));
			this.btnSave.Name = "btnSave";
			this.btnSave.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSave.RightToLeft")));
			this.btnSave.Size = ((System.Drawing.Size)(resources.GetObject("btnSave.Size")));
			this.btnSave.TabIndex = ((int)(resources.GetObject("btnSave.TabIndex")));
			this.btnSave.Text = resources.GetString("btnSave.Text");
			this.btnSave.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSave.TextAlign")));
			this.btnSave.Visible = ((bool)(resources.GetObject("btnSave.Visible")));
			this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
			// 
			// btnClose
			// 
			this.btnClose.AccessibleDescription = resources.GetString("btnClose.AccessibleDescription");
			this.btnClose.AccessibleName = resources.GetString("btnClose.AccessibleName");
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnClose.Anchor")));
			this.btnClose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnClose.BackgroundImage")));
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnClose.Dock")));
			this.btnClose.Enabled = ((bool)(resources.GetObject("btnClose.Enabled")));
			this.btnClose.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnClose.FlatStyle")));
			this.btnClose.Font = ((System.Drawing.Font)(resources.GetObject("btnClose.Font")));
			this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
			this.btnClose.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnClose.ImageAlign")));
			this.btnClose.ImageIndex = ((int)(resources.GetObject("btnClose.ImageIndex")));
			this.btnClose.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnClose.ImeMode")));
			this.btnClose.Location = ((System.Drawing.Point)(resources.GetObject("btnClose.Location")));
			this.btnClose.Name = "btnClose";
			this.btnClose.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnClose.RightToLeft")));
			this.btnClose.Size = ((System.Drawing.Size)(resources.GetObject("btnClose.Size")));
			this.btnClose.TabIndex = ((int)(resources.GetObject("btnClose.TabIndex")));
			this.btnClose.Text = resources.GetString("btnClose.Text");
			this.btnClose.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnClose.TextAlign")));
			this.btnClose.Visible = ((bool)(resources.GetObject("btnClose.Visible")));
			this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
			// 
			// ckbKeepOpen
			// 
			this.ckbKeepOpen.AccessibleDescription = resources.GetString("ckbKeepOpen.AccessibleDescription");
			this.ckbKeepOpen.AccessibleName = resources.GetString("ckbKeepOpen.AccessibleName");
			this.ckbKeepOpen.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ckbKeepOpen.Anchor")));
			this.ckbKeepOpen.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("ckbKeepOpen.Appearance")));
			this.ckbKeepOpen.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ckbKeepOpen.BackgroundImage")));
			this.ckbKeepOpen.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ckbKeepOpen.CheckAlign")));
			this.ckbKeepOpen.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ckbKeepOpen.Dock")));
			this.ckbKeepOpen.Enabled = ((bool)(resources.GetObject("ckbKeepOpen.Enabled")));
			this.ckbKeepOpen.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("ckbKeepOpen.FlatStyle")));
			this.ckbKeepOpen.Font = ((System.Drawing.Font)(resources.GetObject("ckbKeepOpen.Font")));
			this.ckbKeepOpen.Image = ((System.Drawing.Image)(resources.GetObject("ckbKeepOpen.Image")));
			this.ckbKeepOpen.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ckbKeepOpen.ImageAlign")));
			this.ckbKeepOpen.ImageIndex = ((int)(resources.GetObject("ckbKeepOpen.ImageIndex")));
			this.ckbKeepOpen.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ckbKeepOpen.ImeMode")));
			this.ckbKeepOpen.Location = ((System.Drawing.Point)(resources.GetObject("ckbKeepOpen.Location")));
			this.ckbKeepOpen.Name = "ckbKeepOpen";
			this.ckbKeepOpen.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ckbKeepOpen.RightToLeft")));
			this.ckbKeepOpen.Size = ((System.Drawing.Size)(resources.GetObject("ckbKeepOpen.Size")));
			this.ckbKeepOpen.TabIndex = ((int)(resources.GetObject("ckbKeepOpen.TabIndex")));
			this.ckbKeepOpen.Text = resources.GetString("ckbKeepOpen.Text");
			this.ckbKeepOpen.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ckbKeepOpen.TextAlign")));
			this.ckbKeepOpen.Visible = ((bool)(resources.GetObject("ckbKeepOpen.Visible")));
			// 
			// FindResult
			// 
			this.AcceptButton = this.btnSave;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnClose;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.ckbKeepOpen);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.ListViewResult);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "FindResult";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.ListViewResult)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.findDataView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.findDataTable)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.findDataSet)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		//--------------------------------------------------------------------------------
		private void ListViewResult_Click(object sender, System.EventArgs e)
		{
			DataGridColumnStyle s = findResultTableStyle.GridColumnStyles[ListViewResult.CurrentCell.ColumnNumber];
			if (s.MappingName != dataColumnReplaceAccepted.ColumnName) return;
			
			// change current selection
			bool translationAccepted = !(bool) ListViewResult[ListViewResult.CurrentCell];
			
			ListViewResult[ListViewResult.CurrentCell] = translationAccepted;

			DataRow r = findDataTable.Rows[ListViewResult.CurrentCell.RowNumber];
			ToggleTranslation(r, translationAccepted);

			r.AcceptChanges();
		}

		//--------------------------------------------------------------------------------
		private void ToggleTranslation(DataRow r, bool translationAccepted)
		{
			r[DataGridReplaceColumn] = translationAccepted ? r[dataColumnReplaced.ColumnName] : r[dataColumnOriginal.ColumnName];
		}

		//--------------------------------------------------------------------------------
		private void EndEdit()
		{
			DataGridCell cell = ListViewResult.CurrentCell;
			if (cell.ColumnNumber == -1 || cell.RowNumber == -1)
				return;
			ListViewResult.EndEdit(ListViewResult.TableStyles[0].GridColumnStyles[cell.ColumnNumber], cell.RowNumber, false);
			
		}

		//--------------------------------------------------------------------------------
		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			if (!ckbKeepOpen.Checked && 
				MessageBox.Show(
				this,
				Strings.ReplaceAll,
				Strings.MainFormCaption,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning
				) != DialogResult.Yes)
				return;
			
			Visible = false;
			
			try
			{
				//forces datagrid to update data with user changes
				EndEdit();
				TreeNode latestNode = null;
				Translator t = null;

				foreach (DataRow r in findDataTable.Rows)
				{
					if (!(bool)r[dataColumnReplaceAccepted.ColumnName]) continue;
				
					TreeNode nodeToShow = (TreeNode)r[dataColumnData.ColumnName];
					if (nodeToShow == null) continue;
			
					Owner.ProjectsTree.SelectedNode = nodeToShow; 
				
					FindAndReplaceInfos infos = new FindAndReplaceInfos();
					SetStandardFindAndReplaceInfos(r, infos);

					infos.ReplaceString = (string) r[DataGridReplaceColumn];
					infos.ReplaceTarget = TargetModified;

					// breaking of node: i have to save translator
					// in order to avoid lost update problems
					if (latestNode != null && latestNode != nodeToShow)
						SaveTranslator(t);

					t = Owner.ShowTranslator(infos);
					if (t == null) return;
						
					latestNode = nodeToShow;
				}

				SaveTranslator(t);
			}
			finally
			{
				Visible = true;
			}
			
			DialogResult = DialogResult.OK;
			
			Close();

		}

		//--------------------------------------------------------------------------------
		private void SetStandardFindAndReplaceInfos(DataRow r, FindAndReplaceInfos infos)
		{
			infos.BaseString = TargetModified 
				? (string) r[dataColumnBase.ColumnName]
				: (string) r[dataColumnOriginal.ColumnName];
					
			infos.TargetString = TargetModified 
				? (string) r[dataColumnOriginal.ColumnName]
				: (string) r[dataColumnTarget.ColumnName];

			infos.Name = (string) r[dataColumnResourceName.ColumnName];
					
		}

		//--------------------------------------------------------------------------------
		private void SaveTranslator(Translator t)
		{
			if (t == null) return;

			if (ckbKeepOpen.Checked)
			{
				translatorOpen = true;
				t.Closed += new EventHandler(Translator_Closed);
				while (translatorOpen)
					Application.DoEvents();
			}
			else
			{
				t.Save(false);
				t.Close(true);
			}
			
		}

		//--------------------------------------------------------------------------------
		private void Translator_Closed(object sender, EventArgs e)
		{
			translatorOpen = false;
		}

		//--------------------------------------------------------------------------------
		private void BtnClose_Click(object sender, System.EventArgs e)
		{
			DialogResult r = DialogResult.No;
			if (btnSave.Enabled)
			{
				r = MessageBox.Show
					(
					this,
					Strings.SaveTranslationsQuestion,
					Strings.TranslatorCaption, 
					MessageBoxButtons.YesNoCancel, 
					MessageBoxIcon.Question, 
					MessageBoxDefaultButton.Button3
					);
			}

			switch (r)
			{
				case DialogResult.Yes :		BtnSave_Click(sender, e); break;
				case DialogResult.Cancel:	break;
				case DialogResult.No:		Close();	break;
			}
			
		}

		//--------------------------------------------------------------------------------
		private void MiOriginalValues_Click(object sender, System.EventArgs e)
		{
			foreach (DataRow r in findDataTable.Rows)
			{
				r[dataColumnReplaceAccepted.ColumnName] = false;
				ToggleTranslation(r, false);
			}
			findDataSet.AcceptChanges();
		}

		//--------------------------------------------------------------------------------
		private void MiNewValues_Click(object sender, System.EventArgs e)
		{
			foreach (DataRow r in findDataTable.Rows)
			{
				r[dataColumnReplaceAccepted.ColumnName] = true;
				ToggleTranslation(r, true);
			}
			findDataSet.AcceptChanges();
		}

		//--------------------------------------------------------------------------------
		private void FindDataTable_RowChanged(object sender, DataRowChangeEventArgs e)
		{
			btnSave.Enabled = true;
			ToggleEvent(false); // to avoid loop
			e.Row[dataColumnReplaceAccepted.ColumnName] = true;
			ToggleEvent(true);
		}

		//--------------------------------------------------------------------------------
		private void ListViewResult_Layout(object sender, System.Windows.Forms.LayoutEventArgs e)
		{
			SetDataGridDimensionProcedure();
		}
	}

	//================================================================================
	class MarkTextColumnStyle : DataGridTextBoxColumn
	{
		static Brush searchHighLightBrush = new SolidBrush(Color.Red);
		static Brush newValueHighLightBrush = new SolidBrush(Color.Yellow);
		static StringFormat stringFormat = (StringFormat) StringFormat.GenericDefault.Clone();
		
		public Regex SearchExpression;
		public Regex NewValueExpression;

		//--------------------------------------------------------------------------------
		public MarkTextColumnStyle()
		{
		}

		//--------------------------------------------------------------------------------
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			try
			{
				g.FillRectangle(backBrush, bounds);

				DataGrid.HitTestInfo hi;
				DataGrid grid = this.DataGridTableStyle.DataGrid;
				hi = grid.HitTest(bounds.Left, bounds.Top);
				if (hi.Row == -1 || hi.Column == -1) return;

				string text = grid[hi.Row, hi.Column] as string;


				DataGridColumnStyle style = DataGridTableStyle.GridColumnStyles["ReplaceAcceptedColumn"];
				int index = DataGridTableStyle.GridColumnStyles.IndexOf(style);
				bool changeAccepted = ((bool)grid[hi.Row, index]);
				
				Regex exp = changeAccepted ? NewValueExpression : SearchExpression;
				Brush brush = changeAccepted ? newValueHighLightBrush : searchHighLightBrush;
				if ( exp != null)
				{
					ArrayList ranges = new ArrayList();
					MatchCollection mc = exp.Matches(text);
					foreach(Match m in mc)
						ranges.Add(new CharacterRange(m.Index, m.Length));

					CharacterRange[] characterRanges = (CharacterRange[]) ranges.ToArray(typeof(CharacterRange));
					
					try
					{
						stringFormat.SetMeasurableCharacterRanges(characterRanges);
					}
					catch
					{
						//does nothing
					}

					
					Region[] regions = g.MeasureCharacterRanges(text, TextBox.Font, bounds, stringFormat);
					foreach (Region r in regions)
					{
						RectangleF rect = r.GetBounds(g);
						g.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
					}
				}

				g.DrawString(text, grid.Font, foreBrush, bounds, stringFormat);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Fail(ex.Message);
			}
		}
	}
}
