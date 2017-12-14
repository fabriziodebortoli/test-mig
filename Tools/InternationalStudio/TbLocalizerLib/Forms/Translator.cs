using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	///Mostra la tabella per la traduzione delle stringhe.
	/// </summary>
	//=========================================================================
	public class Translator : System.Windows.Forms.Form
	{
		private string selectedText = "";

		private string resourceType;
					
		/// <summary>Colonne del data grid *Non si riesce ad identificarle col nome?*</summary>
		public  enum Columns	{ NAME = 0, TYPE = 1, BASE = 2, SUPPORT = 3, TARGET	= 4, FILE = 5, TEMPORARY = 6 };
		/// <summary>File della solution </summary>
		private DataDocument		tblslnWriter;
		/// <summary>Codice della lingua target in uso</summary>
		private string				languageCode;
		/// <summary>Codice della lingua target in uso</summary>
		public string				SupportLanguage;
		/// <summary>Specifica se la visualizzazione deve essere support</summary>
		public bool					SupportView;
		/// <summary>Posizione del nodo da selezionare rispetto all'attuale</summary>
		private Direction			goTo			= Direction.NULL;
		/// <summary>Nodo del tree da tradurre </summary>
		private DictionaryTreeNode	treeNodeToTranslate;
		/// <summary>Abilitazione dei pulsanti Next e Prev</summary>	
		private EnabledButtons		toEnable		= new EnabledButtons(true,true);
		/// <summary>Visualizzazione solo dei nodi non completamente tradotti</summary>
		private bool				advancedNext;
		/// <summary>lista dei path dei Glossari esterni</summary>
		private string[]			externalGlossariesNames	= null;
		/// <summary>Elenco dei file modificati dopo l'applicazione del glossario</summary>
		private ModifyList			toModify			= new ModifyList(new NamedLocalizerDocument[]{});
		/// <summary>Specifica se chiusura immediata senza salvataggio</summary>
		private bool				closeWithoutSaving	= false;
		/// <summary>Nome della colonna espressione che indica con un simbolo lo stato della stringa</summary>
		private string				typeColumnName		= Columns.TYPE.ToString();
		private bool				modified			= false;
		private System.Windows.Forms.ToolBar TbAutoTranslate;
		private System.Windows.Forms.ToolBarButton BtnCopy;

		private DataGridRowHeightSetter rowHeightSetter;

		private int initialGridHeight, initialFormHeight;
		private System.Windows.Forms.ContextMenu ContextMenuAutoTrans;
		private System.Windows.Forms.MenuItem MiBaseAutoTrans;
		private System.Windows.Forms.MenuItem MiSupportAutoTrans;

		public	bool	IgnoreRowChangeEvents = false;

#region CONTROLS
		public Microarea.Tools.TBLocalizer.DefaultDataSet StringDataSet;
		private System.Data.DataView dataView;
		private IContainer		components;
		private DemoDialog		demoDialog;
		private ContextMenu		ContextMenuRowViewer;
		private MenuItem		MiViewRow;
		private MenuItem		MiAddGlossary;
		private MenuItem		MiApplyGlossary;
		private MenuItem		MiHint;
		private FontDialog		MyFontDialog;
		private ToolTip			ToolTipOnDG;
		public	DataGrid		DgTranslator;
		private ToolBar			TBItemView;
		private ToolBar			TBDialogControl;
		private ToolBar			TBStringsView;
		private ImageList		ImageListToolBar;
		private ToolBarButton	BtnMoveUp;
		private ToolBarButton	BtnMoveDown;
		private ToolBarButton	BtnMoveRight;
		private ToolBarButton	BtnMoveLeft;
		private ToolBarButton	BtnWidthPlus;
		private ToolBarButton	BtnWidthMinus;
		private ToolBarButton	BtnHeightPlus;
		private ToolBarButton	BtnHeightMinus;
		private ToolBarButton	BtrnFont;
		private ToolBarButton	BtnClear;
		private ToolBarButton	BtnPreviousTB;
		private ToolBarButton	BtnNextTB;
		private ToolBarButton	BtnSkipTB;
		private ToolBarButton	BtnFilter;
		private ToolBarButton	BtnPreview;
		private ContextMenu		ContextMenuFilter;
		private MenuItem		MiNoFilter;
		private MenuItem		MiFilterNotValid;
		private MenuItem		MiFilterValid;
		private ToolBarButton	BtnSupportLanguage;
		public	ContextMenu		ContextMenuSupport;
		private MenuItem		MiBase;
		private MenuItem		MiSupport;
		private ToolBar			TBBase;
		private ToolBarButton	BtnSavetb;
		private ToolBarButton	BtnClosetb;
		private ToolBar			TbTools;
		private ToolBarButton	BtnTools;
		private ContextMenu		ContextMenuTools;
		private ToolBarButton	BtnSearchTB;
		private DataGridTableStyle		StringTableStyle;
		private Microarea.Tools.TBLocalizer.Forms.TranslatorDataGridTextBoxColumn NameColumnStyle;
		private TranslatorDataGridTextBoxColumn		BaseColumnStyle;
		private TranslatorDataGridTextBoxColumn		SupportColumnStyle;
		private TranslatorDataGridTextBoxColumn		TargetColumnStyle;
		private TranslatorDataGridTextBoxColumn		FileColumnStyle;
		private TranslatorDataGridBoolColumn		TemporaryColumnStyle;
		private TranslatorDataGridTextBoxColumn		TypeColumnStyle;
		private System.Windows.Forms.MenuItem MiHintFromKnowledge;
#endregion
		private System.Windows.Forms.MenuItem MiPreviousAutoTrans;
		private System.Windows.Forms.ToolBarButton BtnOldStrings;



		//properties
		//--------------------------------------------------------------------------------
		internal	int					CurrentRowUnderMouse 
		{
			get 
			{ 
				Point pt = DgTranslator.PointToClient(MousePosition);
				DataGrid.HitTestInfo	myHitTest = DgTranslator.HitTest(pt.X, pt.Y);
				return myHitTest.Row;
			}
		}
		//properties
		//--------------------------------------------------------------------------------
		internal	int					CurrentColumnUnderMouse 
		{
			get 
			{ 
				Point pt = DgTranslator.PointToClient(MousePosition);
				DataGrid.HitTestInfo	myHitTest = DgTranslator.HitTest(pt.X, pt.Y);
				return myHitTest.Column;
			}
		}
		//--------------------------------------------------------------------------------
		internal	DictionaryTreeNode	TreeNodeToTranslate		{ get {return treeNodeToTranslate;}	set {treeNodeToTranslate = value;}}
		/// <summary>Posizione del nodo da selezionare rispetto all'attuale</summary>
		//--------------------------------------------------------------------------------
		internal	Direction	GoTo					{ get {return goTo;}				set {goTo = value;}}
		/// <summary>Abilitazione dei pulsanti Next e Prev</summary>	
		//--------------------------------------------------------------------------------
		internal	EnabledButtons	ToEnable			{ get {return toEnable;}			set {toEnable = value; EnabledDirectionButtons();}}
		/// <summary>Visualizzazione solo dei nodi non completamente tradotti</summary>
		//--------------------------------------------------------------------------------
		internal	 bool		AdvancedNext			{ get {return advancedNext;}		set {advancedNext = value;}}

		//--------------------------------------------------------------------------------
		private		int			BaseOrSupportColumn		{ get { return IsSupportViewing()? (int)Columns.SUPPORT : (int)Columns.BASE; }}
		
		//--------------------------------------------------------------------------------
		public		bool		HasChanges { get { return modified; } set { modified = true; } }
		
		//--------------------------------------------------------------------------------
		private		DataGridColumnStyle		BaseOrSupportColumnStyle { get { return IsSupportViewing()? SupportColumnStyle : BaseColumnStyle; }}
		//--------------------------------------------------------------------------------
		private		DefaultDataSet._stringRow CurrentRow 
		{
			get 
			{
				return ((DataRowView) DgTranslator.BindingContext[DgTranslator.DataSource, DgTranslator.DataMember].Current).Row as DefaultDataSet._stringRow; 
			}
		}
		//--------------------------------------------------------------------------------
		private		int CurrentRowIndex 
		{
			get 
			{
				return DgTranslator.BindingContext[DgTranslator.DataSource, DgTranslator.DataMember].Position;
			}
		}

		DictionaryCreator owner;
		//--------------------------------------------------------------------------------
		internal new DictionaryCreator  Owner			
		{
			get
			{
				return owner;
			}
			set
			{
				owner = value; 
				if (owner.Owner != null)
					base.Owner = owner.Owner;
				else
					ShowInTaskbar = true;
			}
		}


		//---------------------------------------------------------------------
		public	Translator(DataDocument tblslnWriter, ArrayList toolsInfo)
		{
			InitializeComponent();
			PostInitializeComponent(tblslnWriter, toolsInfo);
		}
	
		/// <summary>
		/// Inizializzazione.
		/// </summary>
		/// <param name="tblslnWriter">writer del file della solution</param>
		//---------------------------------------------------------------------
		private void PostInitializeComponent(DataDocument tblslnWriter, ArrayList toolsInfo)
		{
			this.SupportLanguage = SolutionDocument.LocalInfo.SupportLanguage;
			this.SupportView = SolutionDocument.LocalInfo.UseSupportDictionaryWhenAvailable;
			//la colonna base deve essere assolutamente readonly(lo è già la basecolumnstyle)
			//table.Columns[AllStrings.baseTag].ReadOnly = true;	
			StringDataSet._string.RowChanging  += new DataRowChangeEventHandler(RowChanging);
			StringDataSet._string.RowChanged  += new DataRowChangeEventHandler(RowChanged);
			
			Rectangle r = Screen.FromControl(this).WorkingArea;
			MaximumSize = new Size(r.Width, r.Height);

			initialFormHeight = Height;
			initialGridHeight = DgTranslator.Height;

			//setto i tag dei bottoni next e previous
			TBItemView.Buttons[ToolbarButtonIndexer.Previous].Tag	= Direction.PREVIOUS;
			TBItemView.Buttons[ToolbarButtonIndexer.Next].Tag		= Direction.NEXT;		
			//gestisco doppio click sulla colonna target
			TargetColumnStyle.TextBox.DoubleClick += new System.EventHandler(this.DgTranslator_DoubleClick);
			this.tblslnWriter	= tblslnWriter;
			//skip traslated item
			AdvancedNext		= TBItemView.Buttons[ToolbarButtonIndexer.Skip].Pushed;
			
			//Aggiungo gli item al menu dei tools
			TbTools.Enabled = (toolsInfo != null && toolsInfo.Count > 0);
			
			if (toolsInfo == null) return;

			foreach (ToolInfo ti in toolsInfo)
			{
				ToolMenuItem item = new ToolMenuItem(ti.Name, new EventHandler(MiTool_Click));
				item.UrlToFollow = ti.Url;
				item.Args		 = ti.Args;
				ContextMenuTools.MenuItems.Add(item);
			}

			BaseColumnStyle.TextBox.ContextMenu = ContextMenuRowViewer;

			DgTranslator.BindingContext[DgTranslator.DataSource, DgTranslator.DataMember].PositionChanged += new EventHandler(Translator_PositionChanged);
			
		}

		//---------------------------------------------------------------------
		private void MiTool_Click(object sender, System.EventArgs e)
		{
			ToolMenuItem item = sender as ToolMenuItem;
			if (item == null) return;
			try
			{
				System.Diagnostics.Process.Start(item.UrlToFollow, item.Args);
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, String.Format(Strings.AddressError, exc.Message), Strings.WarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Operazioni effettuate al load del Form.
		/// </summary>
		//---------------------------------------------------------------------
		private void Translator_Load(object sender, System.EventArgs e)
		{			
			this.Location = new Point(0,0);
			//DgTranslator.CurrentCell = new DataGridCell(0,0);
			RefreshControls();
			FillContextMenuFilter();			
		}

		//---------------------------------------------------------------------
		private bool SetCurrentRow(int row)
		{
			if (row < 0)
			{
				Debug.Fail("Numero di riga da impostare: " + row.ToString());
				return false;
			}

			if (row >= dataView.Count)
			{
				Debug.Fail("Numero di riga da impostare: " + row + " - numero effettivo di righe: " + dataView.Count);
				return false;
			}

			try
			{
				BindingManagerBase bmb = DgTranslator.BindingContext[DgTranslator.DataSource, DgTranslator.DataMember];
				if (bmb.Position != row)
					bmb.Position = row;
			}
			catch(Exception ex)
			{
				MessageBox.Show(this, ex.Message);
				return false;
			}

			return true;
		}
		//---------------------------------------------------------------------
		private bool SetCurrentCell(int row, int col)
		{
			if (row < 0)
			{
				Debug.Fail("Numero di riga da impostare per la cella corrente  = " + row.ToString());
				return false;
			}

			if (row >= dataView.Count)
			{
				Debug.Fail("Numero di riga da impostare per la cella corrente: " + row + " - numero effettivo di righe: " + dataView.Count);
				return false;
			}

			try
			{
				DgTranslator.CurrentCell = new DataGridCell(row, col);
				return true;
			}

			catch (Exception exc)
			{
				Debug.Fail("Eccezione durante l'impostazione della cella corrente : " + exc.Message);
				return false;
			}	

		}

		//---------------------------------------------------------------------
		public bool SetSelectedRow(FindAndReplaceInfos infos)
		{
			try
			{
				int index  = FindDataGridRow(infos);
				if (index < 0) return false;

				DgTranslator.Select(index);
				SetCurrentCell(index, BaseOrSupportColumn);
				if (infos.ReplaceString != null) // may be empty
				{
					DataRowView r = dataView[index];
					if (r != null)
					{
						string columnName = infos.ReplaceTarget ? AllStrings.target : AllStrings.baseTag;
						r[columnName] = infos.ReplaceString;
						modified = true;
					}
				}
				
			}
			catch(Exception ex)
			{
				throw new ApplicationException(ex.Message, ex);
			}

			return true;
		}

		//---------------------------------------------------------------------
		private int FindDataGridRow(FindAndReplaceInfos infos)
		{
			int numRows = DgTranslator.BindingContext[DgTranslator.DataSource, DgTranslator.DataMember].Count; 
			for (int i = 0; i < numRows; i++)
			{
				if (
					(infos.BaseString == null || string.Compare(DgTranslator[i, BaseOrSupportColumn] as string, infos.BaseString, true) == 0) 
					&&
					(infos.TargetString == null || infos.TargetString.Length  == 0 || string.Compare(DgTranslator[i, (int)Columns.TARGET] as string, infos.TargetString, true) == 0) 
					&&
					(
					DgTranslator[i, (int)Columns.NAME] == System.DBNull.Value  || string.Compare(DgTranslator[i, (int)Columns.NAME] as string, infos.Name, false) == 0)
					)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Specifica se è abilitata la visualizzazione della support e se 
		/// effettivaente si sta visualizzando la support, perchè potrebbe 
		/// capitare che nonostante tutto si visualizzi la base, perchè 
		/// manca il dizionario di supporto.
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool IsSupportViewing()
		{
			return (CommonFunctions.IsSupportEnabled(SupportLanguage) && SupportView);
		}

		//---------------------------------------------------------------------
		private string GetSelectedTextInCurrentColum(ref Columns columnSelected)
		{
			DataGridTextBoxColumn col = null;
			switch (DgTranslator.CurrentCell.ColumnNumber)
			{
				case (int)Columns.BASE:
					columnSelected = Columns.BASE;
					col = BaseColumnStyle;
					break;
				case (int)Columns.TARGET:
					columnSelected = Columns.TARGET;
					col = TargetColumnStyle;
					break;
				case (int)Columns.SUPPORT:
					columnSelected = Columns.SUPPORT;
					col = SupportColumnStyle;
					break;
				default:
					col = TargetColumnStyle;
					break;
			}
			if (col.TextBox != null)
				return col.TextBox.SelectedText;
			return String.Empty;
		}

		/// <summary>
		/// Operazioni in chiusura del form: salva glossary, chiudi preview, crea assembly...
		/// </summary>
		//---------------------------------------------------------------------
		private void Translator_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !closeWithoutSaving && !Save(true);
			if (e.Cancel) return;
			
			CloseDemoDialog();
			Owner.Focus();
		}

		//---------------------------------------------------------------------
		private void Translator_Closed(object sender, System.EventArgs e)
		{
			TranslatorCache.ActivateNextTranslator();
		}

		//---------------------------------------------------------------------
		internal bool LoadData()
		{
			return LoadData(resourceType, SupportLanguage, SupportView);
		}

		//---------------------------------------------------------------------
		internal bool LoadData(string resourceType)
		{
			return LoadData(resourceType, SupportLanguage, SupportView);
		}

		//---------------------------------------------------------------------
		internal bool LoadData(string resourceType, string supportLanguage, bool supportView)
		{
			DgTranslator.SuspendLayout();
			try
			{
				if (TreeNodeToTranslate.IsBaseLanguageNode)
					return false;

				this.resourceType = resourceType;
				this.Text			= Strings.TranslatorCaption;
				//manualmente faccio scattare l'evento di click per gestire la selezione
				if (SupportView)
					ContextMenuSupport_Click(MiSupport, EventArgs.Empty);
				else
					ContextMenuSupport_Click(MiBase, EventArgs.Empty);
				XmlElement xmlNodeToTranslate = TreeNodeToTranslate.GetResourceNode();

				if (xmlNodeToTranslate == null)
				{
					TBStringsView.Buttons[ToolbarButtonIndexer.LanguageSwitch].Enabled = false;
					return false;
				}

				LocalizerTreeNode languageNode = TreeNodeToTranslate.GetTypedParentNode(NodeType.LANGUAGE);
				languageCode = languageNode.Name;

				this.Text	+=	" >>> " + LanguageManager.GetDescriptionByCode(languageCode);
		
	
				bool supportOk = TreeNodeToTranslate.MergeWithSupport(SupportLanguage);
				if (!supportOk)
				{
					SupportLanguage = null;
					ContextMenuSupport_Click(MiBase, EventArgs.Empty);
				}
				FillGrid(true);
				SetWidths();
			}
			finally
			{
				DgTranslator.ResumeLayout();
				if (dataView.Count > 0)
					SetCurrentCell(0, (int)Columns.TARGET);
			}
			return true;
		}

		//---------------------------------------------------------------------
		public void CopySupportToTarget()
		{	
			DataTable t = StringDataSet.Tables[AllStrings.stringTag];
			bool iscs = GetPrjWriter(TreeNodeToTranslate.GetTypedParentNode(NodeType.PROJECT)).IsCsProject();
			XmlElement xmlNodeToTranslate = TreeNodeToTranslate.GetResourceNode();
			for (int i = 0; i < t.Rows.Count; i++)
			{

				try
				{
					DataRow r  = t.Rows[i];

					string supp = r[AllStrings.support] as string;
					if (supp == null || supp == String.Empty)
						continue;
					
					string target = r[AllStrings.target] as string;
					if (target != null && target.Length > 0)
						continue;

					string b = r[AllStrings.baseTag] as string;
					if (supp == b)
						continue;

					//temporary
					bool temp = false;
					string where = CommonFunctions.XPathWhereClause(true, AllStrings.baseTag, b, AllStrings.support, supp);
					XmlNode supportTempNode = xmlNodeToTranslate.SelectSingleNode(AllStrings.stringTag + where + "/@" + AllStrings.supportTemporary );
					if (supportTempNode != null && supportTempNode.Value != null && supportTempNode.Value.Length > 0)
						temp = String.Compare(supportTempNode.Value, bool.TrueString, true) == 0;
				
					r[AllStrings.target] = supp;
					if (temp)
						r[AllStrings.temporary] = true;
					
				}													
				catch (Exception exc)
				{
					MessageBox.Show(this, exc.Message);
					break;
				}
					
			}	
		}

		

		
		/// <summary>
		/// Setta la larghezza di form e datagrid in base all'area disponibile dello schermo.
		/// </summary>
		//---------------------------------------------------------------------
		private void SetWidths()
		{
			//larghezza form
			this.Width				= MaximumSize.Width;
			//larghezza del datagrid : 98% della finestra
			DgTranslator.Width				= (int)(this.Width * 0.98);
			//il 8% per la colonna temporary
			int widthSmall			= (int)( DgTranslator.Width * 0.08) ; 
			//il 3% per la colonna type
			int widthSmallType			= (int)( DgTranslator.Width * 0.03) ; 
			
			// il resto per base e target, considerando un po' di spazio per le line di separazione
			int widthBig = (int)((DgTranslator.Width - (widthSmall + widthSmallType)) / 2.07); 
			TargetColumnStyle.Width = widthBig;
			if (BaseColumnStyle.Width != 0)
				BaseColumnStyle.Width	= TargetColumnStyle.Width;
			TemporaryColumnStyle.Width	= widthSmall;
			TypeColumnStyle.Width		= widthSmallType;
		}

		/// <summary>
		/// Legge i dati e crea la view da visualizzare nel datagrid. 
		/// </summary>
		/// <param name="supportLanguage">codice della lingua di supporto</param>
		/// <param name="datasetChanged">specifica se il dateset è cambiato rispetto al precedente</param>
		//---------------------------------------------------------------------
		private void FillGrid(bool datasetChanged)
		{
			bool ignore = IgnoreRowChangeEvents; //save previous state
			try
			{
				IgnoreRowChangeEvents = true;
				//leggo l'xml del nodo da tradurre, secondo lo schema proprio.
				StringDataSet.ReadXml(new XmlNodeReader(TreeNodeToTranslate.GetResourceNode()));
				modified = false;
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, exc.Message, "Translator - FillGrid");
			}
			finally
			{
				IgnoreRowChangeEvents = ignore; //restore previous state
			}
			
			//se il dataset non è cambiato non è necessario ripetere le operazioni che seguono
			if (!datasetChanged) return;
		
			AddTypeColumn();
			TypeColumnStyle.MappingName = typeColumnName;

			//properties della colonna temporary
			TemporaryColumnStyle.FalseValue = DBNull.Value;
			TemporaryColumnStyle.TrueValue	= true;
			TemporaryColumnStyle.AllowNull  = false;
			TemporaryColumnStyle.NullValue	= false;

			SetFilter();

			string title = String.Format ( " ({0})", TreeNodeToTranslate.FullPath);

			this.Text += title;

			string path = CommonFunctions.GetPhysicalDictionaryPath(treeNodeToTranslate.FileSystemPath);
			if (File.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				this.Text += " [Read Only]";

			EnabledDirectionButtons();
			string supportIndication = "(" + SupportLanguage + ")";
			SupportColumnStyle.HeaderText = String.Format(SupportColumnStyle.HeaderText, supportIndication);
			TBStringsView.Buttons[ToolbarButtonIndexer.LanguageSwitch].Enabled = MiSupportAutoTrans.Enabled = CommonFunctions.IsSupportEnabled(SupportLanguage);
		}

		//---------------------------------------------------------------------
		private void EnabledDirectionButtons()
		{
			TBItemView.Buttons[ToolbarButtonIndexer.Previous].Enabled	= ToEnable.Previous;
			TBItemView.Buttons[ToolbarButtonIndexer.Next].Enabled		= ToEnable.Next;

		}

		/// <summary>
		/// Restituisce la rappresentazione in stringa dell'xml contenuto nel dataset, 
		/// eventualmente non considerando la colonna Type che è solo una colonna di visualizzazione
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private void UpdateXmlNodeToTranslate()
		{
			XmlElement xmlNodeToTranslate = TreeNodeToTranslate.GetResourceNode();
			if (xmlNodeToTranslate.ChildNodes.Count != StringDataSet._string.Count)
				return;

			for (int i = 0; i < xmlNodeToTranslate.ChildNodes.Count; i++)
			{
				XmlElement  el = xmlNodeToTranslate.ChildNodes[i] as XmlElement;
				if (el == null) 
					continue;

				DefaultDataSet._stringRow row = StringDataSet._string[i];
				foreach (DataColumn dc in StringDataSet._string.Columns)
				{
					if (dc.ColumnName == typeColumnName || dc.ColumnName == AllStrings.support)
						continue;

					object o = row[dc];
					if (o is DBNull)
					{
						el.RemoveAttribute(dc.ColumnName);
						continue;
					}

					string val = o.ToString();
#if DEBUG
					if (	(dc.ColumnName == AllStrings.baseTag && el.GetAttribute(AllStrings.baseTag) != val)
						|| (dc.ColumnName == AllStrings.name && el.GetAttribute(AllStrings.name) != val)
						|| (dc.ColumnName == AllStrings.id && el.GetAttribute(AllStrings.id) != val)
						)
					{
						Debug.Fail("Invalid row position!");
						throw new Exception("Invalid row position!");
					}
#endif
					if (o is Boolean)
						val = val.ToLower();

					el.SetAttribute(dc.ColumnName, val);
				}
			}
		}

		/// <summary>
		/// Setta il filtro da utilizzare per visualizzare il datagrid.
		/// </summary>
		//---------------------------------------------------------------------
		private void SetFilter()
		{
			string filter	 = null;
			int selection	 = -1;
			foreach (MenuItem mi in ContextMenuFilter.MenuItems)
				if (mi.Checked)
					selection = mi.Index;
			//[0]= no filter, show all strings
			switch (selection)
			{
				case 1://only valid strings
					filter = "isNull(valid, true)" ;
					break;
				case 2://only not valid strings
					filter = "valid = 'false'";
					break;
			}

			dataView.RowFilter	= filter;

			Preview();
		}

		//--------------------------------------------------------------------------------
		private bool PreviewAllowed 
		{
			get 
			{
				return  (resourceType == AllStrings.dialog)/* || (resourceType == AllStrings.report)*/; 
			} 
		} 
		/// <summary>
		/// Si occupa di settare correttamente l'abilitazione del pulsante di preview e toolbar dei settings
		/// </summary>
		//---------------------------------------------------------------------
		internal void RefreshControls()
		{	
			BtnPreview.Enabled	= PreviewAllowed;

			EnableSettingsToolbar();
			Refresh();			
		}

		/// <summary>
		/// Popola la combo dei filtri.
		/// </summary>
		//---------------------------------------------------------------------
		private void FillContextMenuFilter()
		{
			string[] items =  Strings.FilterComboItems.Split(AllStrings.separator);
		
			for (int i = 0; i < items.Length; i++)
			{
				ContextMenuFilter.MenuItems[i].Checked =(i == 0);
				ContextMenuFilter.MenuItems[i].Text = items[i];
			}
		}

		/// <summary>
		///  Cambio selezione della combo dei filtri.
		/// </summary>
		//---------------------------------------------------------------------
		private void CmbFilter_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			CloseDemoDialog();
			SetFilter();
		}
	
		/// <summary>
		/// Chiudi translator.
		/// </summary>
		//---------------------------------------------------------------------
		private void CloseTranslator()
		{
			try
			{
				GoTo = Direction.NULL;
				this.Close();
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, exc.Message, "Translator - CloseTranslator");
			}
		}		
		
		//---------------------------------------------------------------------
		private void Preview()
		{
			if (
				!BtnPreview.Pushed	|| 
				!BtnPreview.Enabled ||
				IgnoreRowChangeEvents
				) 
				return;
			
			if (demoDialog == null)
			{
					
				if (resourceType == AllStrings.dialog)
				{
					uint id= CommonFunctions.GetID(TreeNodeToTranslate);
					if (id == 0)
					{
						MessageBox.Show(this, Strings.ResourceNotFound, Strings.WarningCaption);
						BtnPreview.Pushed = false;
						PreviewManager();
						return;
					}

					string []mods = CommonFunctions.GetModules(TreeNodeToTranslate);
					if (mods == null || mods.Length == 0)
					{
						MessageBox.Show(this, Strings.ModuleNotFound, Strings.WarningCaption);
						BtnPreview.Pushed = false;
						PreviewManager();
						return;
					}
					demoDialog = new DemoDialog(mods, id, this);
				}
				else if (resourceType == AllStrings.report)
				{
					string nodeName = TreeNodeToTranslate.Name;
					if (nodeName == AllStrings.reportIdentifier)
						demoDialog = new DemoDialog(TreeNodeToTranslate.FileSystemPath, this);
					else if (nodeName != AllStrings.reportLocalizable)
						demoDialog = new DemoDialog(TreeNodeToTranslate.FileSystemPath, nodeName, this);
					else
						demoDialog = null;

					if (demoDialog != null)
					{
						string path = PathFunctions.GetReportPath(TreeNodeToTranslate);
						path = Path.Combine(path, TreeNodeToTranslate.GroupIdentifier);
						path = Path.ChangeExtension(path, AllStrings.wrmExtension);
						demoDialog.WoormFile = path;
					}
				}
			}
					
			ShowDemoDialog();
		}

		/// <summary>
		/// Setta l'espressione per la colonna che visualizza il tipo di stringa
		/// </summary>
		/// <param name="c">colonna su cui settare l'espressione, se null la cerca nella tabella</param>
		//---------------------------------------------------------------------
		private void AddTypeColumn()
		{
			DgTranslator.SuspendLayout();
			try
			{
				DataColumn c = StringDataSet._string.Columns[typeColumnName];
			
				if (c == null)
				{
					c = StringDataSet._string.Columns.Add(typeColumnName);
					c.Expression = string.Format(
						"IIF(valid = false, '{0}', IIF(ISNULL(matchtype, 0) > 0 AND temporary = true, '{1}', IIF((ISNULL(target, '') = '' OR temporary = true) , '{2}', '')))",
						AllStrings.NotValidRow,
						AllStrings.LooseMatchRow,
						AllStrings.NotTranslatedRow
						);
					return;
				}

			}
			finally
			{
				DgTranslator.ResumeLayout(true);
			}
		}

		//--------------------------------------------------------------------------------
		private bool RemoveTypeColumn()
		{
			DgTranslator.SuspendLayout();
			try
			{
				DataColumn c = StringDataSet._string.Columns[typeColumnName];
			
				if (c == null) return false;

				StringDataSet._string.Columns.Remove(typeColumnName);

				return true;
			}
			finally
			{
				DgTranslator.ResumeLayout(true);
			}
		}

		//---------------------------------------------------------------------
		private void CopyBaseToTarget()
		{
			
			int numRows = DgTranslator.BindingContext[DgTranslator.DataSource, DgTranslator.DataMember].Count; 
			for (int i = 0; i < numRows; i++)
			{
				try
				{
					string target = DgTranslator[i, (int)Columns.TARGET] as string;
					if (target == null || target == String.Empty)
						DgTranslator[i, (int)Columns.TARGET] = DgTranslator[i, (int)Columns.BASE];
				}
				catch (Exception exc)
				{
					MessageBox.Show(this, exc.Message);
					break;
				}
			}				
		}

		/// <summary>
		/// Salva le traduzioni ed eventualmente anche tutte quelle effettuate col glossary.
		/// </summary>
		//---------------------------------------------------------------------
		private void Save()
		{
			try
			{
				UpdateXmlNodeToTranslate();
				if (!TreeNodeToTranslate.SaveToFileSystem())
				{
					MessageBox.Show(this, string.Format(Strings.CannotSave, TreeNodeToTranslate.FileSystemPath), Strings.WarningCaption);
					return;
				}
				modified = false;

				//salvo le modifiche fatte col glossario, ma non devo risalvare il file corrente, 
				//rischierei di perdere le modifiche fatte dopo l'applicazione del glossario
				string[] exceptions = GlossaryFunctions.SaveGlossaryModification(toModify, TreeNodeToTranslate.FileSystemPath);
				GlossaryFunctions.ShowGlossaryExceptions(exceptions);
				toModify.Clear();
				
				if (DictionaryCreator.MainContext.ShowTranslationProgress)
					TreeNodeToTranslate.RefreshNodeAndAncestors	(true);

                KnowledgeManager.AddKnowledgeItems(languageCode, TreeNodeToTranslate.FullPath, TreeNodeToTranslate.GetStringNodes(), false);
			}
			catch (UnauthorizedAccessException exc)
			{
				MessageBox.Show(this, exc.Message, Strings.WarningCaption);
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, exc.Message, Strings.WarningCaption);
			}

		}

		/// <summary>
		/// Set del font del control.
		/// </summary>
		//---------------------------------------------------------------------
		private void ShowFontDialog()
		{
			if (DgTranslator.CurrentRowIndex == -1) return;
			string key = CurrentRow._base;
			DefaultDataSet._stringRow currRow = null, tmpRow;
			foreach (DataRow aRow in StringDataSet._string.Rows)
			{
				tmpRow = aRow as DefaultDataSet._stringRow;
				if (tmpRow != null 
					&& tmpRow._base == key
					&& (tmpRow.IsvalidNull() || (tmpRow.valid)))
				{
					currRow = tmpRow;
					break;                             
				}
			}
			if (currRow == null) 
				return;
			//cerco di aprire la dialog con i valori di default o preselezionati
			Font selectedFont = null;
			if (!currRow.IsfontnameNull())
			{
				FontFamily family	= new FontFamily(currRow.fontname);
				int style			= 0;
				style				= (currRow.bold) ? 1 : 2;
				style				= (currRow.italic) ? style : style + 2;
				FontStyle fs		= FontStyle.Regular;
				switch (style)
				{
					case 1:
						fs = FontStyle.Bold | FontStyle.Italic; break;
					case 2:
						fs = FontStyle.Italic;					break;
					case 3:
						fs = FontStyle.Bold ;					break;
				}
				selectedFont = new Font(family, currRow.fontsize, fs);
			}
			else
			{
				FontFamily defaultFamily = new FontFamily(AllStrings.fontDefault);
				selectedFont = new Font(defaultFamily, 14, FontStyle.Regular);
			}
			MyFontDialog.Font		= selectedFont;
			DialogResult result = (MyFontDialog.ShowDialog(this));
			if (result == DialogResult.OK)
			{
				Font myFont			= MyFontDialog.Font;
				currRow.fontname	= myFont.Name;
				currRow.fontsize	= (long)myFont.Size;
				currRow.bold		= myFont.Bold;
				currRow.italic		= myFont.Italic;		
				Preview();
			}
		}

		//--------------------------------------------------------------------------------
		private void SetDataGridDimension()
		{
			try
			{	
				//setto anchor prima di resettare la dimensione per evitare che venga visualizzato male
				DgTranslator.Anchor = AnchorStyles.Left| AnchorStyles.Right | AnchorStyles.Top;
				if (dataView == null) 
					return;
			 
				rowHeightSetter = new DataGridRowHeightSetter(DgTranslator);

				if (rowHeightSetter.RowCount == 0)
					return;

				// Sets g to a graphics object representing the drawing surface of the control or form g is a member of.
				// Setto l'altezzza della riga in modo che si veda una riga e mezzo, anche in base al dpi setting
				Graphics g = DgTranslator.CreateGraphics();
			
				double totalRowHeight = 0;
				double singleRowHeight = g.MeasureString("O", DgTranslator.Font).Height;
				for (int i = 0; i < rowHeightSetter.RowCount; i++)
				{
					string baseString = DgTranslator[i, BaseOrSupportColumn] as string;
					SizeF size = g.MeasureString(baseString, DgTranslator.Font, BaseOrSupportColumnStyle.Width);
				
					double baseHeight = size.Height;

					string targetString = DgTranslator[i, (int)Columns.TARGET] as string;
					size = g.MeasureString(targetString, DgTranslator.Font, TargetColumnStyle.Width);
				
					double targetHeight = size.Height;

					double max = Math.Max(baseHeight, targetHeight) + singleRowHeight;

					rowHeightSetter[i] = (int) Math.Round(max, 0);
					totalRowHeight += max;

				}
			
				DgTranslator.Height = initialGridHeight + (int) Math.Ceiling(totalRowHeight) + DgTranslator.PreferredRowHeight;
			
				int screenH = MaximumSize.Height;
				int min = Math.Min(DgTranslator.Height + initialFormHeight, screenH);
				if (min == screenH)
					DgTranslator.Height = screenH - initialFormHeight;
				this.Height = Math.Min(DgTranslator.Height + initialFormHeight, screenH);
		
				this.Width = Math.Min(Width, MaximumSize.Width);
			}
			catch(Exception ex)
			{
				Debug.Fail(ex.Message, ex.StackTrace);
			}
			finally
			{
				//resetto anchor ai valori corretti per visualizzare il DgTranslator in maniera corretta
				DgTranslator.Anchor = AnchorStyles.Bottom | AnchorStyles.Left| AnchorStyles.Top| AnchorStyles.Right;
			}
		}		

		/// <summary>
		/// Refresha il translator andando al nodo richiesto.
		/// </summary>
		/// <param name="direction">direzione richiesta per la visualizzazione del prossimo nodo</param>
		//---------------------------------------------------------------------
		private void DirectionButtonClick(Direction direction)
		{
			if (!Save(true)) return;
			GoTo = direction;
			bool ignore = IgnoreRowChangeEvents;
			IgnoreRowChangeEvents = true;
			StringDataSet.Clear();
			IgnoreRowChangeEvents = ignore;

			CloseDemoDialog();
			((DictionaryCreator)Owner).RefreshTranslator(this);
		}

		//---------------------------------------------------------------------
		public bool Save(bool askSaving)
		{
			//confronto xml di apertura con  l'attuale, 
			//controllo se ci sono state modifiche da glossario
			if ( !HasChanges ) return true;
		
			DialogResult result = askSaving 				
				? MessageBox.Show
				(
				this, 
				Strings.SaveTranslationsQuestion, 
				Strings.TranslatorCaption, 
				MessageBoxButtons.YesNoCancel, 
				MessageBoxIcon.Question, 
				MessageBoxDefaultButton.Button1
				)
				: DialogResult.Yes;
			//cancel: niente
			//no: cambio senza salvare
			//si: salvo e cambio
			if (result == DialogResult.Cancel) return false;
			if (result == DialogResult.Yes)
			{
				Save();
			}
			toModify.Clear();
			return true;	
		}

		/// <summary>
		/// Chiude demo dialog.
		/// </summary>
		//---------------------------------------------------------------------
		private void CloseDemoDialog()
		{
			if (demoDialog == null) return;
				
			demoDialog.Close();
			demoDialog = null;
		}

		/// <summary>
		///  Mostra la demo dialog aggiornata.
		/// </summary>
		//---------------------------------------------------------------------
		private bool ShowDemoDialog()
		{
			if (demoDialog != null)
			{
				UpdateXmlNodeToTranslate();
				SetCurrentAttribute(true);
				if (!demoDialog.Show(TreeNodeToTranslate.GetResourceNode().OuterXml))
				{
					demoDialog = null;
					MessageBox.Show(this, Strings.PreviewError, Strings.WarningCaption);
					BtnPreview.Pushed = false;
					PreviewManager();
					return false;
				}
				SetCurrentAttribute(false);
			}
			return true;
		}

		/// <summary>
		/// Setta l'attributo current al nodo corrente per visualizzarlo nella preview.
		/// </summary>
		/// <param name="put">specifica se l'attributo va messo(true) o tolto(false)</param>
		//---------------------------------------------------------------------
		private void SetCurrentAttribute(bool put)
		{
			if (DgTranslator.CurrentRowIndex == -1) return;
			XmlNodeList listOfString = TreeNodeToTranslate.GetResourceNode().SelectNodes(AllStrings.stringTag);
			foreach (XmlElement stringNode in listOfString)
			{
				if (!put && stringNode.Attributes[AllStrings.current] != null )
				{
					stringNode.RemoveAttribute(AllStrings.current);
					break;
				}
				XmlNode baseAttribute = stringNode.Attributes[AllStrings.baseTag];
				if (baseAttribute == null) continue;
				string baseString = DgTranslator[DgTranslator.CurrentRowIndex, (int)Columns.BASE] as string;
				if (put && DgTranslator.CurrentRowIndex != -1 && baseAttribute.Value == baseString)
				{
					// se si vorrà usare anche per i resx che non hanno doc, 
					//devo usare xmlnodetotranslate.ownerdocument
					stringNode.SetAttribute(AllStrings.current, AllStrings.trueTag);
					break;
				}
			}
		}

		/// <summary>
		/// Restituisce il datadocument relativo al file di progetto.
		/// </summary>
		/// <param name="path">path del file di dizionario</param>
		//---------------------------------------------------------------------
		public ProjectDocument GetPrjWriter(LocalizerTreeNode n)
		{
			try
			{
				return Owner.GetPrjWriter(n);
			}
			catch
			{
				return null;
			}			
		}


		/// <summary>
		/// Cambio cella attiva del datagrid. Utile per identificare i file inclusi nei report e
		/// per aggiornare con la nuova preview(con current modificato).
		/// </summary>
		//---------------------------------------------------------------------
		private void DgTranslator_CurrentCellChanged(object sender, System.EventArgs e)
		{
			int colN = DgTranslator.CurrentCell.ColumnNumber;
			//Se la currentCell è su una colonna invisibile eseguo un tab per avanzare fino ad una colonna visibile
			//Potrebbero essere visibili alternativamente le colonne support e base.
			if (colN != (int)Columns.TYPE && colN != BaseOrSupportColumn && colN != (int)Columns.TARGET && colN != (int)Columns.TEMPORARY)
			{
				SendKeys.Send("{Tab}");
				return;
			}

			//per i report vi è la possibilità di avere un file: target non modificabile
			if (resourceType == AllStrings.report)
			{
				if (!(DgTranslator[DgTranslator.CurrentRowIndex, (int)Columns.FILE] is DBNull))
				{
					if (((string)DgTranslator[DgTranslator.CurrentRowIndex, (int)Columns.FILE]) == AllStrings.trueTag)	
						dataView.AllowEdit = false;
				}
				else
				{
					dataView.AllowEdit = true;
				}
			}
		}

		private bool inhibitChange = false;
		//---------------------------------------------------------------------
		private void RowChanged(object sender,  DataRowChangeEventArgs e)
		{
			if (inhibitChange || IgnoreRowChangeEvents) return;
			inhibitChange = true;
			try
			{
				if (e.Row[AllStrings.temporary] == System.DBNull.Value || !(bool)e.Row[AllStrings.temporary])
					e.Row[AllStrings.matchType] = System.DBNull.Value;

				modified = true;
			}
			finally
			{
				inhibitChange = false;
			}

		}

		//--------------------------------------------------------------------------------
		private void Translator_PositionChanged(object sender, EventArgs e)
		{
			Preview();
		}

		/// <summary>
		/// Validazione della traduzione inserita nel rispetto delle espressioni.
		/// </summary>
		//---------------------------------------------------------------------
		private void RowChanging(object sender,  DataRowChangeEventArgs e)
		{
			if (IgnoreRowChangeEvents) return;

			string baseString	= e.Row[AllStrings.baseTag] as string;
			string targetString = e.Row[AllStrings.target]  as string;
			if (targetString == null || targetString == String.Empty) return;
			
			if (!CommonFunctions.CheckOutFileIfNeeded(CommonFunctions.GetPhysicalDictionaryPath(TreeNodeToTranslate.FileSystemPath)))
				throw new ApplicationException(string.Format(Strings.ReadOnlyFiles, CommonFunctions.GetPhysicalDictionaryPath(TreeNodeToTranslate.FileSystemPath)));
			
			CommonFunctions.ParametersMode pMode =  GetParametersMode();

			PlaceHolderValidity phv = CommonFunctions.IsPlaceHolderValid(baseString, targetString, pMode, false);
			if (!phv.TranslationValid || !phv.SequenceValid)
				throw new Exception(Strings.TranslationNotValid, null);
		}

		/// <summary>
		/// Setta il Tooltip relativo alla riga del datagrid, solo su colonna base e support.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void DgTranslator_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int rowUnderMouse = CurrentRowUnderMouse;
			int	 columnUnderMouse = CurrentColumnUnderMouse;
		
			if (rowUnderMouse == -1) return;

			if (columnUnderMouse == (int)Columns.BASE || columnUnderMouse == (int)Columns.SUPPORT)
			{
				int col = 
					(columnUnderMouse == (int)Columns.SUPPORT) ? 
					(int)Columns.BASE : 
					(int)Columns.SUPPORT;
				string stringToShow = DgTranslator[rowUnderMouse, col] as string;	
				if (stringToShow  == null) stringToShow  = String.Empty;
				ToolTipOnDG.SetToolTip(DgTranslator, stringToShow) ;
			}
			else
			{
				string stringToShow = DgTranslator[rowUnderMouse, (int)Columns.NAME ] as string;
				string tooltip = "";
				if (stringToShow != null && stringToShow.Length > 0)
				tooltip = string.Format(Strings.StringName, stringToShow);

				Brush b = null;
				string description = null;
			
				if (IsTemporary(dataView[rowUnderMouse].Row[AllStrings.temporary])&&
					GetBrushAndDescriptionFromMatchType(dataView[rowUnderMouse].Row[AllStrings.matchType], out b, out description))
				{
					if (tooltip.Length > 0)
						tooltip += Environment.NewLine;
					tooltip += string.Format(Strings.WarningBaseChanged, description);
				}
				if (tooltip.Length > 0)
					ToolTipOnDG.SetToolTip(DgTranslator, tooltip);
			}
		}

		//---------------------------------------------------------------------
		private static bool GetBrushAndDescriptionFromMatchType(object matchType, out Brush brush, out string description)
		{
			brush = null;
			description = null;

			if (matchType is System.DBNull)
				return false;
			description = matchType.ToString();
			switch ((DictionaryDocument.StringComparisonFlags) (int)(long)matchType)
			{
				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE:
				{
					brush = Brushes.LimeGreen;
					description = Strings.WarningIgnoreCase;
					return true;
				}				
				case DictionaryDocument.StringComparisonFlags.IGNORE_SPACES:
				{
					brush = Brushes.Yellow;
					description = Strings.WarningIgnoreSpaces;
					return true;
				}
				case DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					brush = Brushes.Red;
					description = Strings.WarningIgnorePunctuation;
					return true;
				}

				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_SPACES:
				{
					brush =  Brushes.Yellow;
					description = Strings.WarningIgnoreCase + Environment.NewLine + Strings.WarningIgnoreSpaces;
					return true;
				}
				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					brush = Brushes.Red;
					description = Strings.WarningIgnoreCase + Environment.NewLine + Strings.WarningIgnorePunctuation;
					return true;
				}	
				case DictionaryDocument.StringComparisonFlags.IGNORE_SPACES | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					brush = Brushes.Red;
					description = Strings.WarningIgnoreSpaces + Environment.NewLine + Strings.WarningIgnorePunctuation;
					return true;
				}	

				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_SPACES | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					brush = Brushes.Red;
					description = Strings.WarningIgnoreCase + Environment.NewLine + Strings.WarningIgnoreSpaces + Environment.NewLine + Strings.WarningIgnorePunctuation;
					return true;
				}	
			}
			return false;
		}

		
		/// <summary>
		/// Aggiunge voce al glossario.
		/// Al glossario vengono aggiunte solo voci tutte in minuscolo e senza &,
		/// in modo che il paragone venga poi effettuato NON case-sensitive e non '&-sensitive'
		/// </summary>
		//---------------------------------------------------------------------
		private void MiAddGlossary_Click(object sender, System.EventArgs e)
		{
			if (CurrentRow == null) return;

			string baseToAdd	= CurrentRow.Is_baseNull()? null : CurrentRow._base;
			string targetToAdd	= CurrentRow.IstargetNull()? null : CurrentRow.target;
			string supportToAdd = CurrentRow.IssupportNull()? null : CurrentRow.support;
			
			if (baseToAdd == null || targetToAdd == null)
			{
				MessageBox.Show(this, String.Format(Strings.NotAddedToGlossary, SupportView? supportToAdd : baseToAdd), Strings.GlossaryCaption);
				return;
			}
			//se target non valida non proseguo	
			PlaceHolderValidity phv = CommonFunctions.IsPlaceHolderValid(baseToAdd, targetToAdd, GetParametersMode(), false);
			if (!phv.TranslationValid)
			{
				ShowTraslationError(Strings.TranslationNotValid);
				return;
			}

			if (GlossaryFunctions.GlossaryEntryExist(baseToAdd, targetToAdd, GlossaryFunctions.GetGlossaryPath(languageCode)))
			{
				MessageBox.Show(this, String.Format(Strings.AlreadyInGlossary, SupportView? supportToAdd : baseToAdd), Strings.GlossaryCaption);
				return;
			}
			GlossaryFunctions.AddGlossaryEntry(baseToAdd, targetToAdd, languageCode, supportToAdd, SupportLanguage, true);
			//glossaryModified = true;
			MessageBox.Show(this, String.Format(Strings.AddedToGlossary, SupportView? supportToAdd : baseToAdd , targetToAdd ) , Strings.GlossaryCaption);
		}

		//---------------------------------------------------------------------
		internal CommonFunctions.ParametersMode GetParametersMode()
		{
			return Owner.GetParametersMode(TreeNodeToTranslate);
		}
		
		/// <summary>
		/// Apre la finestra per la scelta della profondità alla quale effettuare l 'operazione e procede con la traduzione.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiApplyGlossary_Click(object sender, System.EventArgs e)
		{
			if (CurrentRow == null) return;

			string baseString		 = CurrentRow.Is_baseNull() ? null : CurrentRow._base;
			string baseStringTreated = GlossaryFunctions.TreatForGlossary(baseString, true);
			bool result = false;
			string targetString = GlossaryFunctions.GetTarget(baseStringTreated, null, languageCode, false, ref result);
			if (!result || targetString == null || targetString == String.Empty)
			{
				MessageBox.Show(this, String.Format(Strings.GlossaryEntryNotFound, baseString), Strings.GlossaryCaption);
				return;
			}

			//applico il glossario
			ApplyGlossary ag = new ApplyGlossary();
			DialogResult res = ag.ShowDialog(this);

			if (res == DialogResult.OK && ag.SelectedType != NodeType.NULL)
			{
				Cursor = Cursors.WaitCursor;
				ApplyGlossaryIn(ag.SelectedType, baseStringTreated, targetString, ag.Overwrite, ag.NoTemporary, languageCode);
				Cursor = Cursors.Default;
				if (AreThereResults(ag.SelectedString) && CurrentRowIndex > -1)
				{
					try
					{
						//setto manualmente la checkBox a true e il nuovo target (con il setting della &)
						//altrimenti da solo non refresha il valore fino al salvataggio.
						CurrentRow.temporary = !ag.NoTemporary;
						CurrentRow.target = CommonFunctions.SetAmp(baseString, targetString);
						SetCurrentCell(CurrentRowIndex, (int)Columns.TEMPORARY);
					}
					catch(Exception ex)
					{
						MessageBox.Show(this, string.Format(Strings.ErrorApplyTranslation, ex.Message));
					}
				}
				
			}
			else toModify.Clear();
		}

		/// <summary>
		/// Applica il glossario andando a modificare in memoria tutti i file interessati.
		/// </summary>
		/// <param name="selectedIndex">indice dell'item selezionato dalla ApplyGlossary</param>
		/// <param name="selectedItem">testo dell'item selezionato dalla ApplyGlossary</param>
		/// <param name="baseString">stringa chiave del glossary da cercare nei file</param>
		/// <param name="targetString">specifica se andare a sovrascivere anche traduzioni precedentemente effettuate</param>
		//---------------------------------------------------------------------
		internal void ApplyGlossaryIn(NodeType selectedType, string baseString, string targetString, bool overwrite, bool noTemporary, string culture)
		{
			Cursor   = Cursors.WaitCursor;
			toModify.Clear();
			toModify = GlossaryFunctions.TranslateAll(TreeNodeToTranslate.GetTypedParentNode(selectedType), overwrite, noTemporary, culture, baseString, targetString);
		}

		/// <summary>
		///Mostra il numero di modifiche che verranno effettuate al momento del salvataggio.
		/// </summary>
		///<param name="depth">stringa dell'item selezionato dalla ApplyGlossary</param>
		//---------------------------------------------------------------------
		private bool AreThereResults(string depth)
		{
			int count = toModify.GetCount();
			if (count == 0 )
			{
				MessageBox.Show(this, Strings.NoFileToModify, Strings.GlossaryCaption);
				return false;
			}
			//{num} translations detected  in {depth} that will be fixed by saving.
			string message = String.Format(Strings.DetectedTranslations, count.ToString(), depth);
			MessageBox.Show(this, message, Strings.GlossaryCaption);
			return true;
		}

		/// <summary>
		///  Modifica della posizione  e dimensione del controllo.
		/// </summary>
		/// <param name="dPosition">delta della posizione del controllo</param>
		/// <param name="dSize">delta della dimensione del controllo</param>
		//---------------------------------------------------------------------
		private void ChangePosition(Size dPosition, Size dSize)
		{
			if (DgTranslator.CurrentRowIndex == -1) return;
			DefaultDataSet._stringRow currRow = null;
			try
			{
				DataView dv		= (DataView)((CurrencyManager)(DgTranslator.BindingContext[DgTranslator.DataSource, DgTranslator.DataMember])).List;
				DataRowView drv =  dv[DgTranslator.CurrentRowIndex];
				currRow			= drv.Row as DefaultDataSet._stringRow;
	
				if (currRow == null) return;
				
				Point currPos = 
					new Point(
					currRow.IsxNull()	? 0 : (int)currRow.x,
					currRow.IsyNull()	? 0 : (int)currRow.y
					);
            
				currPos += dPosition;

				Size currSize = 
					new Size(
					currRow.IswidthNull()	? 0 : (int)currRow.width,
					currRow.IsheightNull()	? 0 : (int)currRow.height
					);

				currSize += dSize;

				if (currPos.X == 0)
					currRow.SetxNull();
				else
					currRow.x		= currPos.X;

				if (currPos.Y == 0)
					currRow.SetyNull ();
				else
					currRow.y		= currPos.Y ;
			
				if (currSize.Width == 0)
					currRow.SetwidthNull();
				else
					currRow.width	= currSize.Width;
			
				if (currSize.Height == 0)
					currRow.SetheightNull();
				else
					currRow.height	= currSize.Height;
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, exc.Message, Strings.WarningCaption);
				return;
			}
			Preview();
		}

		/// <summary>
		/// Riporta tutti i valori dei control a null per dialog
		/// </summary>
		//---------------------------------------------------------------------
		private void ClearSettingValue()
		{
			if (DgTranslator.CurrentRowIndex == -1) return;

			DefaultDataSet._stringRow currRow = null;

			
			DataView dv		= (DataView)((CurrencyManager)(DgTranslator.BindingContext[DgTranslator.DataSource, DgTranslator.DataMember])).List;
			DataRowView drv =  dv[DgTranslator.CurrentRowIndex];
			currRow			= drv.Row as DefaultDataSet._stringRow;
	
			if (currRow == null) return;
			currRow.SetxNull();
			currRow.SetyNull();
			currRow.SetfontnameNull();
			currRow.SetfontsizeNull();
			currRow.SetwidthNull();
			currRow.SetheightNull();
			currRow.SetboldNull();
			currRow.SetitalicNull();
			Preview();
		}
			
		/// <summary>
		/// Al doppio click sulla riga si apre la visualizzazione facilitata.
		/// </summary>
		//---------------------------------------------------------------------
		private void DgTranslator_DoubleClick(object sender, System.EventArgs e)
		{
			System.Drawing.Point pt = DgTranslator.PointToClient(Cursor.Position); 
			DataGrid.HitTestInfo hti = DgTranslator.HitTest(pt); 
			int	 columnUnderMouse = hti.Column;
		
			if (hti.Type == DataGrid.HitTestType.ColumnHeader) 
				//non sulla cheCkbox, altrimenti non si riesce a cliccare dentro di essa.
				if (DgTranslator.CurrentRowIndex != -1	&& 
					columnUnderMouse != (int)Columns.TEMPORARY	&&
					hti.Type != DataGrid.HitTestType.ColumnHeader	&&
					columnUnderMouse != -1)
					ShowRowView();
		}

		//---------------------------------------------------------------------
		private void DgTranslator_Click(object sender, System.EventArgs e)
		{
			Point p = Control.MousePosition;
			if (DgTranslator.DataMember == null)
				return;
			Point pt = DgTranslator.PointToClient(p);
			DataGrid.HitTestInfo hti = DgTranslator.HitTest(pt);
			BindingManagerBase bmb = this.BindingContext[DgTranslator.DataSource, DgTranslator.DataMember];
			if (hti.Row == -1 )//non dentro le righe ma mi interessa in particolar modo sull'header.
				EndEditManager();
		}

		/// <summary>
		/// Visualizzazione facilitata della riga selezionata.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiViewRow_Click(object sender, System.EventArgs e)
		{
			if (DgTranslator.CurrentRowIndex != -1 )
				ShowRowView();
		}

		/// <summary>
		///Apre una finestra nella quale si può vedere la stringa selezionata 
		///in una finestra più grande. Per i file se ne edita il contenuto.
		/// </summary>
		//---------------------------------------------------------------------
		private void ShowRowView()
		{
			//devo fare in modo che la current cell non sia una 
			//colonna invisibile o disabile perchè altrimenti poi 
			//mi scatta il send del tab che però mi viene agiunto 
			//alla textbox della rowViewer!!!!!!
			SetCurrentCell(DgTranslator.CurrentRowIndex, (int)Columns.TARGET);
			RowViewer rowView = new RowViewer(this, Path.GetDirectoryName(TreeNodeToTranslate.FileSystemPath));
			rowView.ShowDialog(this);
			
		}

		/// <summary>
		/// Ferma l'esecuzione corrente durante l'esecuzione dell'editor esterno, 
		/// poi effettua il refresh e crea le assemblies.
		/// </summary>
		//---------------------------------------------------------------------
		private void RefreshTranslator(string nodePath)
		{
			LocalizerTreeNode node;
			if (Owner.GetNodeFromPath(nodePath, out node))
			{
				TreeNodeToTranslate = (DictionaryTreeNode) node;
				StringDataSet.Clear ();
				LoadData(); // effettua il refresh del translator
			}
		}

		//---------------------------------------------------------------------
		private void TBDialogControl_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (!EndEditManager()) return;
			switch( ((ToolBar)sender).Buttons.IndexOf(e.Button))
			{
				case ToolbarButtonIndexer.MoveUp:
					ChangePosition(new Size(0, -1), new Size(0, 0));
					break;
				case ToolbarButtonIndexer.MoveDown:
					ChangePosition(new Size(0, 1), new Size(0, 0));
					break;
				case ToolbarButtonIndexer.MoveRight:
					ChangePosition(new Size(1, 0), new Size(0, 0));
					break;
				case ToolbarButtonIndexer.MoveLeft:
					ChangePosition(new Size(-1, 0), new Size(0, 0));
					break;
				case ToolbarButtonIndexer.WidhtPlus:
					ChangePosition(new Size(0, 0), new Size(1, 0));
					break;
				case ToolbarButtonIndexer.WidhtMinus:
					ChangePosition(new Size(0, 0), new Size(-1, 0));
					break;
				case ToolbarButtonIndexer.HeigthPlus:
					ChangePosition(new Size(0, 0), new Size(0, 1));
					break;
				case ToolbarButtonIndexer.HeigthMinus:
					ChangePosition(new Size(0, 0), new Size(0, -1));
					break;
				case ToolbarButtonIndexer.Font:
					ShowFontDialog();
					break;
				case ToolbarButtonIndexer.Clear:
					AskClearSettingValue();
					break;
			}
			
		}

		//---------------------------------------------------------------------
		private void AskClearSettingValue()
		{
			DialogResult r = MessageBox.Show
				(
				this, 
				Strings.SetNullSettings,
				Strings.WarningCaption,
				MessageBoxButtons.YesNo
				);
			if (r == DialogResult.Yes)
				ClearSettingValue();
			
		}
		//---------------------------------------------------------------------
		private void TBItemView_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (!EndEditManager()) return;
			
			switch( ((ToolBar)sender).Buttons.IndexOf(e.Button))
			{
				case ToolbarButtonIndexer.Previous:	
				case ToolbarButtonIndexer.Next:		
					DirectionButtonClick((Direction)(e.Button.Tag));
					break;
				case ToolbarButtonIndexer.Skip:
					AdvancedNext = ((ToolBar)sender).Buttons[ToolbarButtonIndexer.Skip].Pushed;
					EnabledButtons enabilitation = ((DictionaryCreator)Owner).CheckButtonToDisable(AdvancedNext);
					TBItemView.Buttons[ToolbarButtonIndexer.Previous].Enabled = enabilitation.Previous;
					TBItemView.Buttons[ToolbarButtonIndexer.Next].Enabled	  = enabilitation.Next;
					break;
			}
		}

		/// <summary>
		/// Da chiamare quando clicco fuori dal datagrid e sui quei controlli, come la toolbar, 
		/// che non attivano l'endEdit
		/// </summary>
		//---------------------------------------------------------------------
		private bool EndEditManager()
		{			
			if (DgTranslator.CurrentRowIndex == -1) return true;
			//DgTranslator.CurrentCell = new DataGridCell(DgTranslator.CurrentRowIndex, (int)Columns.TARGET);
			DgTranslator.EndEdit(TargetColumnStyle, DgTranslator.CurrentRowIndex, false);
			try
			{
				BindingContext[dataView].EndCurrentEdit();
			}
			catch (Exception ex)
			{
				ShowTraslationError(ex.Message);
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------
		public void ShowTraslationError(string msg)
		{			
			MessageBox.Show(this, msg , Strings.WarningCaption);
		}

		//---------------------------------------------------------------------
		private delegate bool SearchWorkerDelegate
			(
			LocalizerTreeNode node, 
			string languageCode, 
			string toSearch, 
			Translator.Columns column
			);

		//---------------------------------------------------------------------
		private void SearchWorker()
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				string tosearch = String.Empty; 
				Columns columnBaseOrSupport = SupportView ? Columns.SUPPORT : Columns.BASE;
				if (DgTranslator.CurrentRowIndex != -1)
				{
					tosearch = GetSelectedTextInCurrentColum(ref columnBaseOrSupport);
					if (tosearch == null || tosearch == String.Empty)
						tosearch = DgTranslator[DgTranslator.CurrentCell.RowNumber, (int)columnBaseOrSupport] as string;
				}
			
				if ((DictionaryCreator)Owner != null)
				{
					BeginInvoke(
						new SearchWorkerDelegate(((DictionaryCreator)Owner).SearchWorker),
						new object[] {TreeNodeToTranslate,
										 languageCode, 
										 tosearch,
										 columnBaseOrSupport}

					
					);
				}
			}
			finally
			{
				Cursor		= Cursors.Default;
			}
		}

		/// <summary>
		/// Da chiamare ogni volta che viene modificata la proprietà pushed del pulsante preview
		/// </summary>
		//---------------------------------------------------------------------
		private void PreviewManager()
		{
			EnableSettingsToolbar();
			if (!BtnPreview.Pushed)
			{
				CloseDemoDialog();
				return;
			}
			else if (BtnPreview.Enabled) 	
				Preview();
		}

		//---------------------------------------------------------------------
		private void EnableSettingsToolbar()
		{
			TBDialogControl.Enabled = BtnPreview.Pushed && (resourceType == AllStrings.dialog);
		}

		//---------------------------------------------------------------------
		private void ContextMenuFilter_Click(object sender, System.EventArgs e)
		{
			EndEditManager();
			foreach (MenuItem mi in ContextMenuFilter.MenuItems)
				mi.Checked = (sender == mi)	;	
			CloseDemoDialog();
			SetFilter();			
		}

		//---------------------------------------------------------------------
		private void ContextMenuSupport_Click(object sender, System.EventArgs e)
		{
			EndEditManager();
			int selection = -1;
			foreach (MenuItem mi in ContextMenuSupport.MenuItems)
			{
				if (sender == mi)
				{
					mi.Checked = true;
					selection = mi.Index;
				}
				else mi.Checked = false;
			}
				 
			int width = 
				(BaseColumnStyle.Width == 0)? 
				SupportColumnStyle.Width : 
				BaseColumnStyle.Width;
			switch (selection)
			{
				case 0:
					BaseColumnStyle.Width	 = width;
					SupportColumnStyle.Width = 0;
					SupportView = false;
					break;
				case 1:
					BaseColumnStyle.Width	 = 0;
					SupportColumnStyle.Width = width;
					SupportView = true;
					break;
			}
		}

		/// <summary> 
		/// Restituisce un suggerimento alla traduzione, cercando di match-are 
		/// i singoli periodi, con i contenuti del glossary.
		/// </summary>
		/// <param name="baseString">stringa per la quale si chiede suggerimento</param>
		//---------------------------------------------------------------------
		private ArrayList SearchInPeriod(string baseString)
		{
			//splitto in frase(contenuta fra segni di interpunzione e/o inizio/fine stringa), 
			return SearchIn(CommonFunctions.GetClausesSplitter(), baseString);
		}

		/// <summary> 
		/// Restituisce un suggerimento alla traduzione, cercando di match-are 
		///  le singole parole, con i contenuti del glossary.
		/// </summary>
		/// <param name="baseString">stringa per la quale si chiede suggerimento</param>
		//---------------------------------------------------------------------
		private ArrayList SearchInWord(string baseString)
		{
			//splitto in parole
			return SearchIn(CommonFunctions.GetWordsSplitter(), baseString);
		}
		
		/// <summary>
		///Cerca nel glossario, anche senza considerare i carattere '&' le varie frasi
		/// o parole ottenute dallo splitting. 
		/// </summary>
		/// <param name="splitter">cartteri di interpunzione e simili</param>
		/// <param name="baseString">stringa per la quale si chiede suggerimento</param>
		//---------------------------------------------------------------------
		private ArrayList SearchIn(char[] splitter, string baseString)
		{
			//comparo case insensitive e senza considerare i segni di interpunzione usati nello splitting
			ArrayList suggestion = new ArrayList();
			string[] split	= baseString.Split(splitter);
			foreach (string piece in split)
				GlossaryFunctions.ProcessGlossaries(piece, suggestion, splitter, languageCode, externalGlossariesNames);

			return suggestion;
		}

		/// <summary>
		/// Richiesta di suggerimento sulla stringa della riga corrente.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiHintFromKnowledge_Click(object sender, System.EventArgs e)
		{
			if (CurrentRow == null) return;
			AddHintFromKnowledge(CurrentRow, false);
			
		}
		//---------------------------------------------------------------------
		private void AddHintFromKnowledge()
		{
			foreach (DefaultDataSet._stringRow row in dataView.Table.Rows)
			{
				if (!row.IstargetNull() && row.target != null && row.target.Length > 0)
					continue;
				if (!AddHintFromKnowledge(row, true))
					break;
			}
		}

		//---------------------------------------------------------------------
		private void ManageOldStrings()
		{
			if (!Save(true))
				return;
			
			string nodePath = TreeNodeToTranslate.FullPath;

			TranslationsRecoverer f = new TranslationsRecoverer(this.TreeNodeToTranslate);
			if (DialogResult.OK == f.ShowDialog(this))
				RefreshTranslator(nodePath);
		}

		//---------------------------------------------------------------------
		private bool AddHintFromKnowledge(DefaultDataSet._stringRow row, bool unattended)
		{
			string baseString;
			if (!unattended && selectedText.Length > 0)
				baseString = selectedText;
			else
				baseString = row.Is_baseNull() ? null : row._base;
			
			Cursor = Cursors.WaitCursor;
			try
			{
				HintItem[] suggestionList = KnowledgeManager.GetSuggestionsWithWaitingWindow(this, baseString, languageCode, unattended);
				//Se non ho suggerimenti, avverto
				if (suggestionList.Length == 0)
				{
					if (!unattended)
						MessageBox.Show(this, Strings.NoSuggestion, Strings.SuggestionCaption);
					return true;
				}
				
				string hint = null;
				if (unattended)
				{
					foreach (HintItem suggestion in suggestionList)
					{
						if (suggestion.Rating == 1f)
						{
							hint = suggestion.HintString;
							break;
						}
					}

					if (hint == null)
						return true;
				}
				else
				{
					//se ho suggerimenti, mostro una dialog di scelta
					ChooseHint ch  = new ChooseHint(suggestionList, true, baseString);
				
					//Concateno alla target i suggerimenti accettati 
					if (ch.ShowDialog(this) != DialogResult.OK)
						return true;

					hint = ch.HintAccepted;
				}
					
				
				if (!unattended && !row.IstargetNull() && row.target != null)
				{
					StringBuilder s = new StringBuilder();
					s.Append(row.target);
					s.Append("\t");
					s.Append(hint);
					row.target = s.ToString();
					row.temporary = true;
					SetCurrentCell(CurrentRowIndex, (int)Columns.TEMPORARY);
				}
				else 
				{
					row.target = hint;
				}
	
			}
			catch(Exception ex)
			{
				MessageBox.Show(this, string.Format(Strings.ErrorApplyTranslation, ex.Message));
				return false;
			}
			finally
			{
				Cursor = Cursors.Default;
			}


			return true;
		}
		/// <summary>
		/// Richiesta di suggerimento sulla stringa della riga corrente.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiHint_Click(object sender, System.EventArgs e)
		{
		
			string baseString	 = CurrentRow.Is_baseNull() ? null : CurrentRow._base;
			string targetString	 = CurrentRow.IstargetNull() ? null : CurrentRow.target;
			
			ArrayList suggestionList = new ArrayList();
			//Cerco
			suggestionList = GetAllGlossaryValues(GlossaryFunctions.TreatForGlossary(baseString, true));
			//Se è  null allora vado a scorporare prima in periodi poi in parole
			//per ottenere il maggior numero di suggerimenti utili
			if (suggestionList == null)
				suggestionList = SearchInPeriod(baseString);
			if (suggestionList.Count == 0)
				suggestionList = SearchInWord(baseString);
			//Se non ho suggerimenti, avverto
			if (suggestionList.Count == 0)
			{
				DialogResult res = MessageBox.Show(this, Strings.NoSuggestion, Strings.SuggestionCaption);
				return;

			}

			HintItem[] hints = new HintItem[suggestionList.Count];
			int i = 0;
			foreach (string s in suggestionList)
				hints[i++] = new HintItem(s);
			
			//se ho suggerimenti, mostro una dialog di scelta
			ChooseHint ch  = new ChooseHint(hints, false, baseString);
			
			//Concateno alla target i suggerimenti accettati 
			if (ch.ShowDialog(this) == DialogResult.OK)
			{
				try
				{
					CurrentRow.target = targetString + " " + ch.HintAccepted;
					CurrentRow.temporary = true;
					SetCurrentCell(CurrentRowIndex, (int)Columns.TEMPORARY);
				}
				catch(Exception ex)
				{
					MessageBox.Show(this, string.Format(Strings.ErrorApplyTranslation, ex.Message));
				}
			}
		}
		
		//---------------------------------------------------------------------
		private ArrayList GetAllGlossaryValues(string baseString)
		{
			ArrayList list = new ArrayList();
			if (baseString == null || baseString == String.Empty)
				return list;
			//Aggiungo i risultati del glossario interno
			//ArrayList internalResult = glossary[baseString];
			ArrayList internalResult = GlossaryFunctions.SearchGlossaryTargets(baseString, GlossaryFunctions.GetGlossaryPath(languageCode));
			if (internalResult != null)
			{//evito ripetizioni
				foreach (string s in internalResult)
					if (!list.Contains(s))
						list.Add(s);
			}
			//Aggiungo i risultati dei glossari esterni se ci sono
			if (externalGlossariesNames == null) 
				return list;
			ArrayList externalResult = new ArrayList();
			foreach (string name in externalGlossariesNames)
			{
				externalResult = GlossaryFunctions.SearchGlossaryTargets(baseString, name);//ght[baseString];
				if (externalResult != null)
				{//evito ripetizioni
					foreach (string s in externalResult)
						if (!list.Contains(s))
							list.Add(s);
				}
			}
			return list;
		}

		//---------------------------------------------------------------------
		public void Close(bool withoutSaving)
		{		
			closeWithoutSaving = withoutSaving;
			this.Close();
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

				TranslatorCache.RefreshCache(TreeNodeToTranslate, null);

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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Translator));
			this.BtnOldStrings = new System.Windows.Forms.ToolBarButton();
			this.DgTranslator = new System.Windows.Forms.DataGrid();
			this.ContextMenuRowViewer = new System.Windows.Forms.ContextMenu();
			this.MiViewRow = new System.Windows.Forms.MenuItem();
			this.MiAddGlossary = new System.Windows.Forms.MenuItem();
			this.MiApplyGlossary = new System.Windows.Forms.MenuItem();
			this.MiHint = new System.Windows.Forms.MenuItem();
			this.MiHintFromKnowledge = new System.Windows.Forms.MenuItem();
			this.dataView = new System.Data.DataView();
			this.StringDataSet = new Microarea.Tools.TBLocalizer.DefaultDataSet();
			this.StringTableStyle = new System.Windows.Forms.DataGridTableStyle();
			this.NameColumnStyle = new Microarea.Tools.TBLocalizer.Forms.TranslatorDataGridTextBoxColumn();
			this.TypeColumnStyle = new Microarea.Tools.TBLocalizer.Forms.TranslatorDataGridTextBoxColumn();
			this.BaseColumnStyle = new Microarea.Tools.TBLocalizer.Forms.TranslatorDataGridTextBoxColumn();
			this.SupportColumnStyle = new Microarea.Tools.TBLocalizer.Forms.TranslatorDataGridTextBoxColumn();
			this.TargetColumnStyle = new Microarea.Tools.TBLocalizer.Forms.TranslatorDataGridTextBoxColumn();
			this.FileColumnStyle = new Microarea.Tools.TBLocalizer.Forms.TranslatorDataGridTextBoxColumn();
			this.TemporaryColumnStyle = new Microarea.Tools.TBLocalizer.Forms.TranslatorDataGridBoolColumn();
			this.MyFontDialog = new System.Windows.Forms.FontDialog();
			this.ToolTipOnDG = new System.Windows.Forms.ToolTip(this.components);
			this.TBItemView = new System.Windows.Forms.ToolBar();
			this.BtnPreviousTB = new System.Windows.Forms.ToolBarButton();
			this.BtnNextTB = new System.Windows.Forms.ToolBarButton();
			this.BtnSkipTB = new System.Windows.Forms.ToolBarButton();
			this.ImageListToolBar = new System.Windows.Forms.ImageList(this.components);
			this.TBDialogControl = new System.Windows.Forms.ToolBar();
			this.BtnMoveUp = new System.Windows.Forms.ToolBarButton();
			this.BtnMoveDown = new System.Windows.Forms.ToolBarButton();
			this.BtnMoveRight = new System.Windows.Forms.ToolBarButton();
			this.BtnMoveLeft = new System.Windows.Forms.ToolBarButton();
			this.BtnWidthPlus = new System.Windows.Forms.ToolBarButton();
			this.BtnWidthMinus = new System.Windows.Forms.ToolBarButton();
			this.BtnHeightPlus = new System.Windows.Forms.ToolBarButton();
			this.BtnHeightMinus = new System.Windows.Forms.ToolBarButton();
			this.BtrnFont = new System.Windows.Forms.ToolBarButton();
			this.BtnClear = new System.Windows.Forms.ToolBarButton();
			this.TBStringsView = new System.Windows.Forms.ToolBar();
			this.BtnFilter = new System.Windows.Forms.ToolBarButton();
			this.ContextMenuFilter = new System.Windows.Forms.ContextMenu();
			this.MiNoFilter = new System.Windows.Forms.MenuItem();
			this.MiFilterValid = new System.Windows.Forms.MenuItem();
			this.MiFilterNotValid = new System.Windows.Forms.MenuItem();
			this.BtnSupportLanguage = new System.Windows.Forms.ToolBarButton();
			this.ContextMenuSupport = new System.Windows.Forms.ContextMenu();
			this.MiBase = new System.Windows.Forms.MenuItem();
			this.MiSupport = new System.Windows.Forms.MenuItem();
			this.BtnPreview = new System.Windows.Forms.ToolBarButton();
			this.BtnSearchTB = new System.Windows.Forms.ToolBarButton();
			this.TBBase = new System.Windows.Forms.ToolBar();
			this.BtnSavetb = new System.Windows.Forms.ToolBarButton();
			this.BtnClosetb = new System.Windows.Forms.ToolBarButton();
			this.TbTools = new System.Windows.Forms.ToolBar();
			this.BtnTools = new System.Windows.Forms.ToolBarButton();
			this.ContextMenuTools = new System.Windows.Forms.ContextMenu();
			this.TbAutoTranslate = new System.Windows.Forms.ToolBar();
			this.BtnCopy = new System.Windows.Forms.ToolBarButton();
			this.ContextMenuAutoTrans = new System.Windows.Forms.ContextMenu();
			this.MiPreviousAutoTrans = new System.Windows.Forms.MenuItem();
			this.MiBaseAutoTrans = new System.Windows.Forms.MenuItem();
			this.MiSupportAutoTrans = new System.Windows.Forms.MenuItem();
			((System.ComponentModel.ISupportInitialize)(this.DgTranslator)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StringDataSet)).BeginInit();
			this.SuspendLayout();
			// 
			// BtnOldStrings
			// 
			resources.ApplyResources(this.BtnOldStrings, "BtnOldStrings");
			this.BtnOldStrings.Name = "BtnOldStrings";
			// 
			// DgTranslator
			// 
			this.DgTranslator.AllowNavigation = false;
			this.DgTranslator.AlternatingBackColor = System.Drawing.Color.Lavender;
			resources.ApplyResources(this.DgTranslator, "DgTranslator");
			this.DgTranslator.BackgroundColor = System.Drawing.SystemColors.Window;
			this.DgTranslator.CaptionBackColor = System.Drawing.SystemColors.Highlight;
			this.DgTranslator.CaptionForeColor = System.Drawing.SystemColors.HighlightText;
			this.DgTranslator.CaptionVisible = false;
			this.DgTranslator.ContextMenu = this.ContextMenuRowViewer;
			this.DgTranslator.DataMember = "";
			this.DgTranslator.DataSource = this.dataView;
			this.DgTranslator.GridLineStyle = System.Windows.Forms.DataGridLineStyle.None;
			this.DgTranslator.HeaderBackColor = System.Drawing.SystemColors.ActiveCaption;
			this.DgTranslator.HeaderFont = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DgTranslator.HeaderForeColor = System.Drawing.Color.Lavender;
			this.DgTranslator.Name = "DgTranslator";
			this.DgTranslator.ParentRowsBackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.DgTranslator.PreferredColumnWidth = 100;
			this.DgTranslator.RowHeaderWidth = 20;
			this.DgTranslator.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			this.DgTranslator.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			this.DgTranslator.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
            this.StringTableStyle});
			this.DgTranslator.CurrentCellChanged += new System.EventHandler(this.DgTranslator_CurrentCellChanged);
			this.DgTranslator.Click += new System.EventHandler(this.DgTranslator_Click);
			this.DgTranslator.DoubleClick += new System.EventHandler(this.DgTranslator_DoubleClick);
			this.DgTranslator.Layout += new System.Windows.Forms.LayoutEventHandler(this.DgTranslator_Layout);
			this.DgTranslator.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DgTranslator_MouseMove);
			// 
			// ContextMenuRowViewer
			// 
			this.ContextMenuRowViewer.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MiViewRow,
            this.MiAddGlossary,
            this.MiApplyGlossary,
            this.MiHint,
            this.MiHintFromKnowledge});
			this.ContextMenuRowViewer.Popup += new System.EventHandler(this.ContextMenuRowViewer_Popup);
			// 
			// MiViewRow
			// 
			this.MiViewRow.Index = 0;
			resources.ApplyResources(this.MiViewRow, "MiViewRow");
			this.MiViewRow.Click += new System.EventHandler(this.MiViewRow_Click);
			// 
			// MiAddGlossary
			// 
			this.MiAddGlossary.Index = 1;
			resources.ApplyResources(this.MiAddGlossary, "MiAddGlossary");
			this.MiAddGlossary.Click += new System.EventHandler(this.MiAddGlossary_Click);
			// 
			// MiApplyGlossary
			// 
			this.MiApplyGlossary.Index = 2;
			resources.ApplyResources(this.MiApplyGlossary, "MiApplyGlossary");
			this.MiApplyGlossary.Click += new System.EventHandler(this.MiApplyGlossary_Click);
			// 
			// MiHint
			// 
			this.MiHint.Index = 3;
			resources.ApplyResources(this.MiHint, "MiHint");
			this.MiHint.Click += new System.EventHandler(this.MiHint_Click);
			// 
			// MiHintFromKnowledge
			// 
			this.MiHintFromKnowledge.Index = 4;
			resources.ApplyResources(this.MiHintFromKnowledge, "MiHintFromKnowledge");
			this.MiHintFromKnowledge.Click += new System.EventHandler(this.MiHintFromKnowledge_Click);
			// 
			// dataView
			// 
			this.dataView.AllowDelete = false;
			this.dataView.AllowNew = false;
			this.dataView.Table = this.StringDataSet._string;
			// 
			// StringDataSet
			// 
			this.StringDataSet.DataSetName = "DefaultDataSet";
			this.StringDataSet.EnforceConstraints = false;
			this.StringDataSet.Locale = new System.Globalization.CultureInfo("en-US");
			this.StringDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
			// 
			// StringTableStyle
			// 
			this.StringTableStyle.AlternatingBackColor = System.Drawing.Color.Lavender;
			this.StringTableStyle.DataGrid = this.DgTranslator;
			this.StringTableStyle.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
            this.NameColumnStyle,
            this.TypeColumnStyle,
            this.BaseColumnStyle,
            this.SupportColumnStyle,
            this.TargetColumnStyle,
            this.FileColumnStyle,
            this.TemporaryColumnStyle});
			this.StringTableStyle.HeaderBackColor = System.Drawing.Color.White;
			this.StringTableStyle.HeaderForeColor = System.Drawing.Color.RoyalBlue;
			this.StringTableStyle.MappingName = "string";
			resources.ApplyResources(this.StringTableStyle, "StringTableStyle");
			this.StringTableStyle.SelectionBackColor = System.Drawing.Color.RoyalBlue;
			this.StringTableStyle.SelectionForeColor = System.Drawing.Color.White;
			// 
			// NameColumnStyle
			// 
			this.NameColumnStyle.Format = "";
			this.NameColumnStyle.FormatInfo = null;
			resources.ApplyResources(this.NameColumnStyle, "NameColumnStyle");
			this.NameColumnStyle.ReadOnly = true;
			// 
			// TypeColumnStyle
			// 
			resources.ApplyResources(this.TypeColumnStyle, "TypeColumnStyle");
			this.TypeColumnStyle.Format = "";
			this.TypeColumnStyle.FormatInfo = null;
			this.TypeColumnStyle.ReadOnly = true;
			// 
			// BaseColumnStyle
			// 
			this.BaseColumnStyle.Format = "";
			this.BaseColumnStyle.FormatInfo = null;
			resources.ApplyResources(this.BaseColumnStyle, "BaseColumnStyle");
			this.BaseColumnStyle.ReadOnly = true;
			// 
			// SupportColumnStyle
			// 
			this.SupportColumnStyle.Format = "";
			this.SupportColumnStyle.FormatInfo = null;
			resources.ApplyResources(this.SupportColumnStyle, "SupportColumnStyle");
			this.SupportColumnStyle.ReadOnly = true;
			// 
			// TargetColumnStyle
			// 
			this.TargetColumnStyle.Format = "";
			this.TargetColumnStyle.FormatInfo = null;
			resources.ApplyResources(this.TargetColumnStyle, "TargetColumnStyle");
			// 
			// FileColumnStyle
			// 
			this.FileColumnStyle.Format = "";
			this.FileColumnStyle.FormatInfo = null;
			resources.ApplyResources(this.FileColumnStyle, "FileColumnStyle");
			this.FileColumnStyle.ReadOnly = true;
			// 
			// TemporaryColumnStyle
			// 
			resources.ApplyResources(this.TemporaryColumnStyle, "TemporaryColumnStyle");
			this.TemporaryColumnStyle.FalseValue = "";
			this.TemporaryColumnStyle.NullValue = "";
			this.TemporaryColumnStyle.TrueValue = "";
			// 
			// MyFontDialog
			// 
			this.MyFontDialog.AllowScriptChange = false;
			this.MyFontDialog.FontMustExist = true;
			this.MyFontDialog.ShowEffects = false;
			// 
			// ToolTipOnDG
			// 
			this.ToolTipOnDG.AutoPopDelay = 2000;
			this.ToolTipOnDG.InitialDelay = 500;
			this.ToolTipOnDG.ReshowDelay = 100;
			this.ToolTipOnDG.ShowAlways = true;
			// 
			// TBItemView
			// 
			this.TBItemView.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.BtnPreviousTB,
            this.BtnNextTB,
            this.BtnSkipTB});
			resources.ApplyResources(this.TBItemView, "TBItemView");
			this.TBItemView.Divider = false;
			this.TBItemView.ImageList = this.ImageListToolBar;
			this.TBItemView.Name = "TBItemView";
			this.TBItemView.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.TBItemView_ButtonClick);
			// 
			// BtnPreviousTB
			// 
			resources.ApplyResources(this.BtnPreviousTB, "BtnPreviousTB");
			this.BtnPreviousTB.Name = "BtnPreviousTB";
			// 
			// BtnNextTB
			// 
			resources.ApplyResources(this.BtnNextTB, "BtnNextTB");
			this.BtnNextTB.Name = "BtnNextTB";
			// 
			// BtnSkipTB
			// 
			resources.ApplyResources(this.BtnSkipTB, "BtnSkipTB");
			this.BtnSkipTB.Name = "BtnSkipTB";
			this.BtnSkipTB.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// ImageListToolBar
			// 
			this.ImageListToolBar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageListToolBar.ImageStream")));
			this.ImageListToolBar.TransparentColor = System.Drawing.Color.White;
			this.ImageListToolBar.Images.SetKeyName(0, "");
			this.ImageListToolBar.Images.SetKeyName(1, "");
			this.ImageListToolBar.Images.SetKeyName(2, "");
			this.ImageListToolBar.Images.SetKeyName(3, "");
			this.ImageListToolBar.Images.SetKeyName(4, "");
			this.ImageListToolBar.Images.SetKeyName(5, "");
			this.ImageListToolBar.Images.SetKeyName(6, "");
			this.ImageListToolBar.Images.SetKeyName(7, "");
			this.ImageListToolBar.Images.SetKeyName(8, "");
			this.ImageListToolBar.Images.SetKeyName(9, "");
			this.ImageListToolBar.Images.SetKeyName(10, "");
			this.ImageListToolBar.Images.SetKeyName(11, "");
			this.ImageListToolBar.Images.SetKeyName(12, "");
			this.ImageListToolBar.Images.SetKeyName(13, "");
			this.ImageListToolBar.Images.SetKeyName(14, "");
			this.ImageListToolBar.Images.SetKeyName(15, "");
			this.ImageListToolBar.Images.SetKeyName(16, "");
			this.ImageListToolBar.Images.SetKeyName(17, "");
			this.ImageListToolBar.Images.SetKeyName(18, "");
			this.ImageListToolBar.Images.SetKeyName(19, "");
			this.ImageListToolBar.Images.SetKeyName(20, "");
			this.ImageListToolBar.Images.SetKeyName(21, "");
			this.ImageListToolBar.Images.SetKeyName(22, "");
			this.ImageListToolBar.Images.SetKeyName(23, "");
			this.ImageListToolBar.Images.SetKeyName(24, "OldStrings.bmp");
			// 
			// TBDialogControl
			// 
			this.TBDialogControl.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.BtnMoveUp,
            this.BtnMoveDown,
            this.BtnMoveRight,
            this.BtnMoveLeft,
            this.BtnWidthPlus,
            this.BtnWidthMinus,
            this.BtnHeightPlus,
            this.BtnHeightMinus,
            this.BtrnFont,
            this.BtnClear});
			resources.ApplyResources(this.TBDialogControl, "TBDialogControl");
			this.TBDialogControl.Divider = false;
			this.TBDialogControl.ImageList = this.ImageListToolBar;
			this.TBDialogControl.Name = "TBDialogControl";
			this.TBDialogControl.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.TBDialogControl_ButtonClick);
			// 
			// BtnMoveUp
			// 
			resources.ApplyResources(this.BtnMoveUp, "BtnMoveUp");
			this.BtnMoveUp.Name = "BtnMoveUp";
			// 
			// BtnMoveDown
			// 
			resources.ApplyResources(this.BtnMoveDown, "BtnMoveDown");
			this.BtnMoveDown.Name = "BtnMoveDown";
			// 
			// BtnMoveRight
			// 
			resources.ApplyResources(this.BtnMoveRight, "BtnMoveRight");
			this.BtnMoveRight.Name = "BtnMoveRight";
			// 
			// BtnMoveLeft
			// 
			resources.ApplyResources(this.BtnMoveLeft, "BtnMoveLeft");
			this.BtnMoveLeft.Name = "BtnMoveLeft";
			// 
			// BtnWidthPlus
			// 
			resources.ApplyResources(this.BtnWidthPlus, "BtnWidthPlus");
			this.BtnWidthPlus.Name = "BtnWidthPlus";
			// 
			// BtnWidthMinus
			// 
			resources.ApplyResources(this.BtnWidthMinus, "BtnWidthMinus");
			this.BtnWidthMinus.Name = "BtnWidthMinus";
			// 
			// BtnHeightPlus
			// 
			resources.ApplyResources(this.BtnHeightPlus, "BtnHeightPlus");
			this.BtnHeightPlus.Name = "BtnHeightPlus";
			// 
			// BtnHeightMinus
			// 
			resources.ApplyResources(this.BtnHeightMinus, "BtnHeightMinus");
			this.BtnHeightMinus.Name = "BtnHeightMinus";
			// 
			// BtrnFont
			// 
			resources.ApplyResources(this.BtrnFont, "BtrnFont");
			this.BtrnFont.Name = "BtrnFont";
			// 
			// BtnClear
			// 
			resources.ApplyResources(this.BtnClear, "BtnClear");
			this.BtnClear.Name = "BtnClear";
			// 
			// TBStringsView
			// 
			this.TBStringsView.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.BtnFilter,
            this.BtnSupportLanguage,
            this.BtnPreview,
            this.BtnSearchTB});
			resources.ApplyResources(this.TBStringsView, "TBStringsView");
			this.TBStringsView.Divider = false;
			this.TBStringsView.ImageList = this.ImageListToolBar;
			this.TBStringsView.Name = "TBStringsView";
			this.TBStringsView.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.TBStringsView_ButtonClick);
			// 
			// BtnFilter
			// 
			this.BtnFilter.DropDownMenu = this.ContextMenuFilter;
			resources.ApplyResources(this.BtnFilter, "BtnFilter");
			this.BtnFilter.Name = "BtnFilter";
			this.BtnFilter.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			// 
			// ContextMenuFilter
			// 
			this.ContextMenuFilter.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MiNoFilter,
            this.MiFilterValid,
            this.MiFilterNotValid});
			// 
			// MiNoFilter
			// 
			this.MiNoFilter.Checked = true;
			this.MiNoFilter.Index = 0;
			this.MiNoFilter.RadioCheck = true;
			resources.ApplyResources(this.MiNoFilter, "MiNoFilter");
			this.MiNoFilter.Click += new System.EventHandler(this.ContextMenuFilter_Click);
			// 
			// MiFilterValid
			// 
			this.MiFilterValid.Index = 1;
			this.MiFilterValid.RadioCheck = true;
			resources.ApplyResources(this.MiFilterValid, "MiFilterValid");
			this.MiFilterValid.Click += new System.EventHandler(this.ContextMenuFilter_Click);
			// 
			// MiFilterNotValid
			// 
			this.MiFilterNotValid.Index = 2;
			this.MiFilterNotValid.RadioCheck = true;
			resources.ApplyResources(this.MiFilterNotValid, "MiFilterNotValid");
			this.MiFilterNotValid.Click += new System.EventHandler(this.ContextMenuFilter_Click);
			// 
			// BtnSupportLanguage
			// 
			this.BtnSupportLanguage.DropDownMenu = this.ContextMenuSupport;
			resources.ApplyResources(this.BtnSupportLanguage, "BtnSupportLanguage");
			this.BtnSupportLanguage.Name = "BtnSupportLanguage";
			this.BtnSupportLanguage.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			// 
			// ContextMenuSupport
			// 
			this.ContextMenuSupport.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MiBase,
            this.MiSupport});
			// 
			// MiBase
			// 
			this.MiBase.Checked = true;
			this.MiBase.Index = 0;
			this.MiBase.RadioCheck = true;
			resources.ApplyResources(this.MiBase, "MiBase");
			this.MiBase.Click += new System.EventHandler(this.ContextMenuSupport_Click);
			// 
			// MiSupport
			// 
			this.MiSupport.Index = 1;
			this.MiSupport.RadioCheck = true;
			resources.ApplyResources(this.MiSupport, "MiSupport");
			this.MiSupport.Click += new System.EventHandler(this.ContextMenuSupport_Click);
			// 
			// BtnPreview
			// 
			resources.ApplyResources(this.BtnPreview, "BtnPreview");
			this.BtnPreview.Name = "BtnPreview";
			this.BtnPreview.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			// 
			// BtnSearchTB
			// 
			resources.ApplyResources(this.BtnSearchTB, "BtnSearchTB");
			this.BtnSearchTB.Name = "BtnSearchTB";
			// 
			// TBBase
			// 
			this.TBBase.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.BtnSavetb,
            this.BtnClosetb});
			resources.ApplyResources(this.TBBase, "TBBase");
			this.TBBase.Divider = false;
			this.TBBase.ImageList = this.ImageListToolBar;
			this.TBBase.Name = "TBBase";
			this.TBBase.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.TBBase_ButtonClick);
			// 
			// BtnSavetb
			// 
			resources.ApplyResources(this.BtnSavetb, "BtnSavetb");
			this.BtnSavetb.Name = "BtnSavetb";
			// 
			// BtnClosetb
			// 
			resources.ApplyResources(this.BtnClosetb, "BtnClosetb");
			this.BtnClosetb.Name = "BtnClosetb";
			// 
			// TbTools
			// 
			this.TbTools.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.BtnTools});
			resources.ApplyResources(this.TbTools, "TbTools");
			this.TbTools.ImageList = this.ImageListToolBar;
			this.TbTools.Name = "TbTools";
			// 
			// BtnTools
			// 
			this.BtnTools.DropDownMenu = this.ContextMenuTools;
			resources.ApplyResources(this.BtnTools, "BtnTools");
			this.BtnTools.Name = "BtnTools";
			this.BtnTools.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			// 
			// TbAutoTranslate
			// 
			this.TbAutoTranslate.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.BtnCopy,
            this.BtnOldStrings});
			resources.ApplyResources(this.TbAutoTranslate, "TbAutoTranslate");
			this.TbAutoTranslate.ImageList = this.ImageListToolBar;
			this.TbAutoTranslate.Name = "TbAutoTranslate";
			this.TbAutoTranslate.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.TbAutoTranslate_ButtonClick);
			// 
			// BtnCopy
			// 
			this.BtnCopy.DropDownMenu = this.ContextMenuAutoTrans;
			resources.ApplyResources(this.BtnCopy, "BtnCopy");
			this.BtnCopy.Name = "BtnCopy";
			this.BtnCopy.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			// 
			// ContextMenuAutoTrans
			// 
			this.ContextMenuAutoTrans.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MiPreviousAutoTrans,
            this.MiBaseAutoTrans,
            this.MiSupportAutoTrans});
			// 
			// MiPreviousAutoTrans
			// 
			this.MiPreviousAutoTrans.Index = 0;
			resources.ApplyResources(this.MiPreviousAutoTrans, "MiPreviousAutoTrans");
			this.MiPreviousAutoTrans.Click += new System.EventHandler(this.MiPreviousAutoTrans_Click);
			// 
			// MiBaseAutoTrans
			// 
			this.MiBaseAutoTrans.Index = 1;
			resources.ApplyResources(this.MiBaseAutoTrans, "MiBaseAutoTrans");
			this.MiBaseAutoTrans.Click += new System.EventHandler(this.MiBaseAutoTrans_Click);
			// 
			// MiSupportAutoTrans
			// 
			this.MiSupportAutoTrans.Index = 2;
			resources.ApplyResources(this.MiSupportAutoTrans, "MiSupportAutoTrans");
			this.MiSupportAutoTrans.Click += new System.EventHandler(this.MiSupportAutoTrans_Click);
			// 
			// Translator
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.DgTranslator);
			this.Controls.Add(this.TbTools);
			this.Controls.Add(this.TBStringsView);
			this.Controls.Add(this.TBDialogControl);
			this.Controls.Add(this.TBItemView);
			this.Controls.Add(this.TBBase);
			this.Controls.Add(this.TbAutoTranslate);
			this.MaximizeBox = false;
			this.Name = "Translator";
			this.ShowInTaskbar = false;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Translator_Closing);
			this.Closed += new System.EventHandler(this.Translator_Closed);
			this.Load += new System.EventHandler(this.Translator_Load);
			((System.ComponentModel.ISupportInitialize)(this.DgTranslator)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StringDataSet)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		//---------------------------------------------------------------------
		private void TBBase_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (!EndEditManager()) return;
			switch( ((ToolBar)sender).Buttons.IndexOf(e.Button))
			{
				case ToolbarButtonIndexer.Save:
					Save();
					break;
				case ToolbarButtonIndexer.Close:
					CloseTranslator();
					break;
			}
		}

		//---------------------------------------------------------------------
		private void TbAutoTranslate_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (!EndEditManager()) return;
			switch (((ToolBar)sender).Buttons.IndexOf(e.Button))
			{
				case ToolbarButtonIndexer.AutoTranslate:
					AddHintFromKnowledge();
					break;
				case ToolbarButtonIndexer.OldStrigns:
					ManageOldStrings();
					break;
			}
		}

		//--------------------------------------------------------------------------------
		private void MiPreviousAutoTrans_Click(object sender, System.EventArgs e)
		{
			if (!EndEditManager()) return;
			AddHintFromKnowledge();
		}
		//--------------------------------------------------------------------------------
		private void MiBaseAutoTrans_Click(object sender, System.EventArgs e)
		{
			if (!EndEditManager()) return;
			CopyBaseToTarget();
		}

		//--------------------------------------------------------------------------------
		private void MiSupportAutoTrans_Click(object sender, System.EventArgs e)
		{
			if (!EndEditManager()) return;
			CopySupportToTarget();
		}

		//--------------------------------------------------------------------------------
		private void DgTranslator_Layout(object sender, System.Windows.Forms.LayoutEventArgs e)
		{
			SetDataGridDimension();
		}
		
		//--------------------------------------------------------------------------------
		public static bool IsTemporary(object o)
		{
			if (!(o is bool)) return false;
			return(bool) o;
		}

		//--------------------------------------------------------------------------------
		public static Brush GetLineBrush(Brush currentBrush, CurrencyManager source, int rowNum)
		{
			DataRowView current = ((DataRowView) source.Current).DataView[rowNum];
			
			Brush b = null;
			string description = null;
			if (IsTemporary(current.Row[AllStrings.temporary]) &&
				GetBrushAndDescriptionFromMatchType(current.Row[AllStrings.matchType], out b, out description))
				return b;
			return currentBrush;
		}

		//--------------------------------------------------------------------------------
		private void ContextMenuRowViewer_Popup(object sender, System.EventArgs e)
		{
			int rowUnderMouse = CurrentRowUnderMouse;
			
			if (rowUnderMouse == -1)
				return;
			SetCurrentRow(rowUnderMouse);
			MiApplyGlossary.Text = string.Format(Strings.ApplyGlossaryTo, CurrentRow._base);
			if (ContextMenuRowViewer.SourceControl is TextBox)
			{
				TextBox tb = (TextBox)ContextMenuRowViewer.SourceControl;
				selectedText = tb.SelectedText;
			}
			else
			{
				selectedText = "";
			}
		}

		//--------------------------------------------------------------------------------
		private void TBShowCRLF_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			TranslatorDataGridTextBoxColumn.ShowCrLf = !TranslatorDataGridTextBoxColumn.ShowCrLf;
			DgTranslator.Invalidate();
		}

		//---------------------------------------------------------------------
		private void TBStringsView_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{			
			if (!EndEditManager()) return;
			switch (((ToolBar)sender).Buttons.IndexOf(e.Button))
			{
					//questo caso corrisponde alla abilitazione/ disabilitazione
					//	della preview
				case ToolbarButtonIndexer.Preview:
					PreviewManager();
					break;
				case ToolbarButtonIndexer.Search:
					SearchWorker();
					break;
			}
			//gli altri casi sono gestiti dai menu di contesto
		}
	}


	//=========================================================================
	class TranslatorDataGridTextBoxColumn : DataGridTextBoxColumn
	{
		public static bool ShowCrLf = false;
	
		private static string crlf = string.Format(" {0}{1}\r\n", Convert.ToChar(0x25C4), Convert.ToChar(0x255D));
		private static string lf = string.Format(" {0}{1}\n", Convert.ToChar(0x25C4), Convert.ToChar(0x255D));
		 
		//--------------------------------------------------------------------------------
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			g.FillRectangle(Translator.GetLineBrush(backBrush, source, rowNum), bounds);
			DataRow row = ((DataRowView) source.List[rowNum]).Row;
			string text = row[MappingName] as string;
			
			if (text == null)
				text = string.Empty;
			else if (ShowCrLf)
				text = text.Replace("\n", lf);

			g.DrawString(text, this.DataGridTableStyle.DataGrid.Font, foreBrush, bounds);
		}
	}

	//=========================================================================
	class TranslatorDataGridBoolColumn : DataGridBoolColumn
	{
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			base.Paint (g, bounds, source, rowNum, Translator.GetLineBrush(backBrush, source, rowNum), foreBrush, alignToRight);
		}
	}

	/// <summary>
	/// Classe specifica per numerare i ToolbarButtons, ai quali si accede solo tramite l'index.
	/// </summary>
	//=========================================================================
	internal class ToolbarButtonIndexer
	{
		public const int Filter			= 0;
		public const int LanguageSwitch	= 1;
		public const int Preview		= 2;
		public const int Search			= 3;

		public const int Previous		= 0;
		public const int Next			= 1;
		public const int Skip			= 2;

		public const int MoveUp			= 0;
		public const int MoveDown		= 1;
		public const int MoveRight		= 2;
		public const int MoveLeft		= 3;
		public const int WidhtPlus		= 4;
		public const int WidhtMinus		= 5;
		public const int HeigthPlus		= 6;
		public const int HeigthMinus	= 7;
		public const int Font			= 8;
		public const int Clear			= 9;

		public const int WinRes			= 0;

		public const int ETranslator	= 0;

		public const int AutoTranslate	= 0;
		public const int OldStrigns		= 1;

		public const int Base			= 0;
		public const int Support		= 1;

		public const int Save			= 0;
		public const int Close			= 1;

	}

	/// <summary>
	///
	/// </summary>
	//=========================================================================
	public class GlossaryDepth
	{
		private		string		text;
		private		NodeType	aValue;
		//--------------------------------------------------------------------------------
		public		NodeType	ValueMember		{get {return aValue;}	set {aValue = value;}}
		//--------------------------------------------------------------------------------
		public		string		DisplayMember	{get {return text;}		set {text = value;}}

		//--------------------------------------------------------------------------------
		public GlossaryDepth (NodeType type, string text)
		{
			ValueMember		= type;
			DisplayMember	= text;
		}
	}

	//=========================================================================
	public class FinderInfo
	{
		public XmlNode NodeXml;
		public LocalizerTreeNode NodeTree;
		public ArrayList TranslationInfos;


		//--------------------------------------------------------------------------------
		public FinderInfo (XmlNode xmlNode, LocalizerTreeNode treeNode, ArrayList translationInfos)
		{
			NodeXml = xmlNode; 
			NodeTree = treeNode;
			TranslationInfos = translationInfos;
		}
	}

	//=========================================================================
	public struct TranslationInfo
	{
		public string BaseString;
		public string TargetString;
		public string SupportString;
		public string NameString;

		public TranslationInfo (string baseString, string targetString, string supportString, string nameString)
		{
			BaseString		= baseString;
			TargetString	= targetString;
			SupportString	= supportString;
			NameString		= nameString;
		}
	}

	//=========================================================================
	public class ToolMenuItem : MenuItem
	{
		public string UrlToFollow;
		public string Args;

		//--------------------------------------------------------------------------------
		public ToolMenuItem (string text, EventHandler onClick)
			:
			base(text, onClick)
		{}
	}

	//=========================================================================
	public class SupportInfo
	{
		public string SupportLanguage;
		public bool SupportView;

		//--------------------------------------------------------------------------------
		public SupportInfo (string supportLanguage, bool supportView)
		{
			SupportLanguage = supportLanguage;
			SupportView		= supportView;
		}

		//--------------------------------------------------------------------------------
		public bool IsNull ()
		{
			return (SupportLanguage == null && !SupportView);
		}
	}

	//=========================================================================
	/// <summary>Direzione per la selezione del nodo richiesto</summary>
	public  enum Direction	{ NEXT = 0, PREVIOUS = 1, NULL = 2 };
	
	//=========================================================================
	public struct EnabledButtons
	{
		public bool Next;
		public bool Previous;

		//=========================================================================
		public EnabledButtons(bool next, bool previous)
		{
			this.Next		= next;
			this.Previous	= previous;
		}
	}

	//=========================================================================
	public class FindAndReplaceInfos
	{
		public string Name;
		public string BaseString;
		public string TargetString;
		public string ReplaceString;
		public bool ReplaceTarget;
	}
}