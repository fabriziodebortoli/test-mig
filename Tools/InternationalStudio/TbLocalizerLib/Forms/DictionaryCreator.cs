using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WinControls;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.SourceBinding;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Microarea.Tools.TBLocalizer.Forms
{

	/// <summary>
	/// Consente di creare dizionari ed accedere alla loro traduzione.
	/// </summary>
	//=========================================================================
	public class DictionaryCreator : System.Windows.Forms.UserControl
	{
		public delegate void CloseSolutionEventHandler(object sender);

		#region funzioni per interagire con il componente ospitante

		private static Form owner = null;

		public event EventHandler MenuStateChanged;

		//--------------------------------------------------------------------------------
		internal void OnMenuStateChanged()
		{
			if (MenuStateChanged != null)
				MenuStateChanged(this, EventArgs.Empty);
		}
		//--------------------------------------------------------------------------------
		public static System.Windows.Forms.Form ActiveForm { get { return Form.ActiveForm; } }

		//--------------------------------------------------------------------------------
		public System.Windows.Forms.Form Owner { get { return owner; } }

		//--------------------------------------------------------------------------------
		public bool ShowInTaskbar
		{
			get
			{
				return Owner == null ? false : Owner.ShowInTaskbar;
			}
			set
			{
				if (Owner != null)
					Owner.ShowInTaskbar = value;
			}
		}

		//--------------------------------------------------------------------------------
		public void Close() { if (Owner != null) Owner.Close(); }

		private event CancelEventHandler Closing;

		//--------------------------------------------------------------------------------
		public void InvokeClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (Closing != null)
				Closing(sender, e);
		}

		//--------------------------------------------------------------------------------
		public Icon ApplicationIcon
		{
			get
			{
				Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Tools.TBLocalizer.img.App.ico");
				return new Icon(s);

			}
		}

		#endregion


		#region  private data members

		#region Controls
		private Microarea.Tools.TBLocalizer.CommonUtilities.LocalizerTreeView ProjectsTreeView;

		private IContainer components;
		private System.Windows.Forms.MenuItem MiSepOpt2;
		private System.Windows.Forms.MenuItem MiSepOpt1;
		private System.Windows.Forms.MenuItem MiChooseLanguage;
		private System.Windows.Forms.MenuItem MiAllLanguage;
		private System.Windows.Forms.MenuItem MiZipDictionary;
		private System.Windows.Forms.MenuItem MiUnzipDictionary;
		private System.Windows.Forms.MenuItem MiImportDictionary;
		private System.Windows.Forms.MenuItem MiTranslateFromKnowledge;
		private System.Windows.Forms.MenuItem MiTranslateJson;
		private System.Windows.Forms.MenuItem MiDictionaryViewer;
		private System.Windows.Forms.MenuItem MiTranslateFromLanguage;
		private System.Windows.Forms.MenuItem MiCustomCreate;
		private System.Windows.Forms.Splitter HorizontalSplitter;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.PictureBox PbStart;
		private System.Windows.Forms.Panel PanelButtons;
		private System.Windows.Forms.MenuItem MiUpdateReferences;
		private System.Windows.Forms.ContextMenu ResultsContextMenu;
		private System.Windows.Forms.MenuItem MenuItemClear;
		private System.Windows.Forms.MenuItem MenuItemCopy;
		private System.Windows.Forms.MenuItem MenuItemSave;
		private ImageList ProjectsTreeViewImageList;
		private ContextMenu MainContextMenu;
		private ContextMenu MyContextMenu;
		private StatusBar MyStatusBar;
		private Panel PanelStartPage;
		private Panel PanelTreeView;
		private LinkLabel LnkNew;
		private LinkLabel LnkOpen;
		private DataGrid DgStart;
		private ProgressBar MyProgressBar;
		public RichTextBox TxtOutput;
		private ImageList ImageListButton;
		private Panel PanelOutput;
		private Button BtnStopProcedure;

		private ToolTip ToolTipPrj;
		private DataGridTableStyle MyTableStyle;
		private DataGridTextBoxColumn HeaderColumnStyle;
		private DataGridTextBoxColumn HiddenColumnStyle;

		#region MenuItems

		// MainContextMenu: File Menu ("Solution")
		private System.Windows.Forms.MenuItem MiFile;
		private System.Windows.Forms.MenuItem MiNew;
		private System.Windows.Forms.MenuItem MiOpen;
		private System.Windows.Forms.MenuItem MiCloseSol;
		private System.Windows.Forms.MenuItem MiFileSeparator1;
		private System.Windows.Forms.MenuItem MiDir;
		private System.Windows.Forms.MenuItem MiXml;
		private System.Windows.Forms.MenuItem MiFileSeparator2;
		private System.Windows.Forms.MenuItem MiSupport;
		private System.Windows.Forms.MenuItem MiGlossaries;
		private System.Windows.Forms.MenuItem MiTools;
		private System.Windows.Forms.MenuItem MiFilterDictionaries;
		private System.Windows.Forms.MenuItem MiRefresh;
		private System.Windows.Forms.MenuItem MiCollapse;
		private System.Windows.Forms.MenuItem MiFileSeparator3;
		private System.Windows.Forms.MenuItem MiSave;
		private System.Windows.Forms.MenuItem MiSaveAs;
		private System.Windows.Forms.MenuItem MiFileSeparator4;
		private System.Windows.Forms.MenuItem MiClose;
		// MainContextMenu: File Menu ("Solution") - Submenu Support ("Support dictionary")
		private System.Windows.Forms.MenuItem MiSupportChoose;
		private System.Windows.Forms.MenuItem MiResetSupport;

		// MainContextMenu: Projects Menu ("Projects")
		private System.Windows.Forms.MenuItem MiProjects;
		private System.Windows.Forms.MenuItem MiManageProjects;
		private System.Windows.Forms.MenuItem MiRemoveProjects;
		private System.Windows.Forms.MenuItem MiSelectAll;
		private System.Windows.Forms.MenuItem MiProjectsSeparator1;
		private System.Windows.Forms.MenuItem MiDictionary;
		private System.Windows.Forms.MenuItem MiProjectsSeparator2;
		private System.Windows.Forms.MenuItem MiBuildSelectedProjects;
		private System.Windows.Forms.MenuItem MiBuildSolution;
		private System.Windows.Forms.MenuItem MiProjectsSeparator3;
		private System.Windows.Forms.MenuItem MiCreate;
		private System.Windows.Forms.MenuItem MiAddDictionary;

		// MainContextMenu: Options Menu ("Options")
		private System.Windows.Forms.MenuItem MiOptions;
		private System.Windows.Forms.MenuItem MiProgress;
		private System.Windows.Forms.MenuItem MiGlossaryOptions;
		private System.Windows.Forms.MenuItem MiFont;
		private System.Windows.Forms.MenuItem MiRapidCreation;
		private System.Windows.Forms.MenuItem MiTempAsTrans;
		private System.Windows.Forms.MenuItem MiEnvironmentSettings;
		// MainContextMenu: Options Menu ("Options") - Submenu Glossary ("Glossary")
		private System.Windows.Forms.MenuItem MiViewGlossary;
		private System.Windows.Forms.MenuItem MiSetGlossaryFolder;

		// MainContextMenu: Tools Menu ("Tools")
		private System.Windows.Forms.MenuItem MiLocalizerTools;
		private System.Windows.Forms.MenuItem MiTbLoader;

		// MainContextMenu: SourceControl Menu ("Source Control")
		private System.Windows.Forms.MenuItem MiSourceControl;
		private System.Windows.Forms.MenuItem MiEnableSourceControl;

		// MainContextMenu: Question Menu ("?")
		private System.Windows.Forms.MenuItem MiQuestion;
		private System.Windows.Forms.MenuItem MiHelp;
		private System.Windows.Forms.MenuItem MiAbout;

		// MyContextMenu
		private System.Windows.Forms.MenuItem MiTranslate;
		private System.Windows.Forms.MenuItem MiFind;
		private System.Windows.Forms.MenuItem MiCount;
		private System.Windows.Forms.MenuItem MiSetReference;
		private System.Windows.Forms.MenuItem MiRemove;
		private System.Windows.Forms.MenuItem MiGlossary;
		private System.Windows.Forms.MenuItem MiProgressSpec;
		private System.Windows.Forms.MenuItem MiCheckDialogs;
		private System.Windows.Forms.MenuItem MiAmpersand;
		private System.Windows.Forms.MenuItem MiPlaceholder;
		private System.Windows.Forms.MenuItem MiTranslation;
		private System.Windows.Forms.MenuItem MiAutoTranslate;
		// MyContextMenu: Translation Menu ("Export translation table...")
		private System.Windows.Forms.MenuItem MiAllStrings;
		private System.Windows.Forms.MenuItem MiNotTranslatedStrings;
		// MyContextMenu: Translation Menu ("Translate empty target as...")
		private System.Windows.Forms.MenuItem MiAutoTranslateBase;

		#endregion // MenuItems

		#endregion // Controls

		/// <summary>Elenco dei progetti selezionati (con icona manina)</summary>
		private ArrayList selectedNodeList;
		/// <summary>Elenco dei file di progetto(key = percorso cartella del file, value = writer)</summary>
		private ProjectDocumentTable tblPrjFiles;
		/// <summary>Elenco dei dizionari in comune ai prj selezionati</summary>
		private ArrayList commonDictionaries;

		private WindowPointCapturer windowPointCapturer;
		/// <summary>File della solution</summary>
		private SolutionDocument tblslnWriter;
		/// <summary>File di inizializzazione,contiene le ultime solution</summary>
		private IniDocument iniWriter;
		/// <summary>Contatore dei progetti della solution</summary>
		private int countPrj = 0;
		/// <summary>Contatore per nominare le nuove solution con un numero progressivo</summary>
		private int docCounter = 0;
		/// <summary>Indica l'abilitazione dei pulsanti next e previous del translator</summary>
		private EnabledButtons enabilitation;

		/// <summary>Lista tabellare dei glossari esterni</summary>
		private Point clickPosition;

		private bool working = false;
		/// <summary>cartella scelta dalla openfolderdialog  per le solutions</summary>
		private string solutionsFolder = String.Empty;

		private Logger batchLogger = null;
		private Logger globalLogger = null;

		private bool batchBuild = false;

		private int oldNodeIndex = -1;

		private ControlFreezer ProjectsTreeViewFreezer;
		private SourceControlManager sControlMng = null;

		//private int freezeTimes = 0;
		private DataGrid.HitTestInfo dgStartHit;
		private WordCounter globalWordCounter;
		private CaseInsensitiveStringCollection dictionaries = new CaseInsensitiveStringCollection();

		public static Thread UpdatingThread = null;
		public static Thread SCCRefreshingThread = null;

		#endregion // private data members

		#region internal data members

		internal Hashtable ExternalGlossaries = null;

		internal ArrayList ToolsList;

		#endregion // internal data members

		#region public data members
		public int SocketPort = 1971;
		public static DictionaryCreator MainContext = null;
		private MenuItem MiRecover;
		private MenuItem MiTranslatedStrings;
		private MenuItem MiTeamSystem;
		private MenuItem MiTFSSourceBinding;
		private ImageList StateImageList;
		private MenuItem menuItem1;
		private MenuItem MiCheckIn;
		private MenuItem MiCheckOut;
		private MenuItem MiGetLatest;
		private MenuItem MiUndoCheckOut;
		private MenuItem menuItem2;
		private MenuItem MiXmlAllStrings;
		private MenuItem MiXmlNotTranslatedStrings;
		private MenuItem MiXmlTranslatedStrings;
		private MenuItem MiImportXml;
		private MenuItem menuItem3;
		private MenuItem MiImportHTML;
		private MenuItem menuItem4;
		private MenuItem MiCSVAllStrings;
		private MenuItem MiCSVNotTranslatedStrings;
		private MenuItem MiCSVTranslatedStrings;
		private MenuItem MIPurge;
		private Microarea.Tools.TBLocalizer.CommonUtilities.LocalizerWaitingControl WaitingControl;
		#endregion // public data members

		internal Logger GlobalLogger { get { return globalLogger; } }
		//--------------------------------------------------------------------------------
		private bool ProjectMenuItemsEnabled { get { return selectedNodeList.Count > 0; } }

		//--------------------------------------------------------------------------------
		private bool IsExistingSolution { get { return File.Exists(Solution); } }

		internal string SupportLanguage
		{
			get { return SolutionDocument.LocalInfo.SupportLanguage; }
			set { SolutionDocument.LocalInfo.SupportLanguage = value; }
		}
		internal bool SupportView
		{
			get { return SolutionDocument.LocalInfo.UseSupportDictionaryWhenAvailable; }
			set { SolutionDocument.LocalInfo.UseSupportDictionaryWhenAvailable = value; }
		}
		//---------------------------------------------------------------------
		private bool IsHiddenCulture(string culture)
		{
			return
				(string.Compare(CommonUtilities.LocalizerTreeNode.BaseLanguage, culture, true) != 0)
				&&
				(SolutionDocument.LocalInfo.HiddenDictionaries.Contains(culture.ToLower()));
		}

		#region public properties

		//---------------------------------------------------------------------
		public System.Windows.Forms.TreeView ProjectsTree { get { return this.ProjectsTreeView; } }

		//---------------------------------------------------------------------
		public System.Windows.Forms.ImageList ProjectsTreeImageList { get { return this.ProjectsTreeViewImageList; } }

		//---------------------------------------------------------------------
		public System.Windows.Forms.ContextMenu MainMenu { get { return this.MainContextMenu; } }

		//---------------------------------------------------------------------
		public MenuItem SolutionMenuItem { get { return this.MiFile; } }
		//--------------------------------------------------------------------------------
		public MenuItem SolutionNewMenuItem { get { return this.MiNew; } }
		//--------------------------------------------------------------------------------
		public MenuItem SolutionOpenMenuItem { get { return this.MiOpen; } }
		//--------------------------------------------------------------------------------
		public MenuItem SolutionCloseMenuItem { get { return this.MiCloseSol; } }
		//--------------------------------------------------------------------------------
		public MenuItem SolutionIncludeDirsMenuItem { get { return this.MiDir; } }
		//--------------------------------------------------------------------------------
		public MenuItem SolutionToolsMenuItem { get { return this.MiTools; } }
		//--------------------------------------------------------------------------------
		public MenuItem SolutionCollapseMenuItem { get { return this.MiCollapse; } }
		//--------------------------------------------------------------------------------
		public MenuItem SolutionSaveMenuItem { get { return this.MiSave; } }
		//--------------------------------------------------------------------------------
		public MenuItem SolutionSaveAsMenuItem { get { return this.MiSaveAs; } }
		//--------------------------------------------------------------------------------
		public MenuItem SolutionExitAsMenuItem { get { return this.MiClose; } }

		//--------------------------------------------------------------------------------
		public MenuItem ProjectsMenuItem { get { return this.MiProjects; } }
		//--------------------------------------------------------------------------------
		public MenuItem ManageProjectsMenuItem { get { return this.MiManageProjects; } }
		//--------------------------------------------------------------------------------
		public MenuItem RemoveProjectsMenuItem { get { return this.MiRemoveProjects; } }
		//--------------------------------------------------------------------------------
		public MenuItem BuildSelProjectsMenuItem { get { return this.MiBuildSelectedProjects; } }
		//--------------------------------------------------------------------------------
		public MenuItem BuildSolutionMenuItem { get { return this.MiBuildSolution; } }
		//--------------------------------------------------------------------------------
		public MenuItem DictionaryExNovoMenuItem { get { return this.MiCreate; } }
		//--------------------------------------------------------------------------------
		public MenuItem DictionaryUpdateMenuItem { get { return this.MiAddDictionary; } }

		//--------------------------------------------------------------------------------
		public MenuItem OptionsMenuItem { get { return this.MiOptions; } }
		//--------------------------------------------------------------------------------
		public MenuItem OptionRapidCreationMenuItem { get { return this.MiRapidCreation; } }
		//--------------------------------------------------------------------------------
		public MenuItem OptionTempAsTransMenuItem { get { return this.MiTempAsTrans; } }
		//--------------------------------------------------------------------------------
		public MenuItem OptionEnvSettingsMenuItem { get { return this.MiEnvironmentSettings; } }
		//--------------------------------------------------------------------------------
		public MenuItem OptionsViewGlossaryMenuItem { get { return this.MiViewGlossary; } }


		//--------------------------------------------------------------------------------
		public MenuItem ToolsMenuItem { get { return this.MiLocalizerTools; } }
		//--------------------------------------------------------------------------------
		public MenuItem SourceControlMenuItem { get { return this.MiSourceControl; } }
		//--------------------------------------------------------------------------------
		public MenuItem QuestionMenuItem { get { return this.MiQuestion; } }

		//--------------------------------------------------------------------------------
		public MenuItem TranslateMenuItem { get { return this.MiTranslate; } }
		//--------------------------------------------------------------------------------
		public MenuItem FindMenuItem { get { return this.MiFind; } }
		//--------------------------------------------------------------------------------
		public MenuItem GlossaryMenuItem { get { return this.MiGlossary; } }
		//--------------------------------------------------------------------------------
		public MenuItem ShowProgressMenuItem { get { return this.MiProgressSpec; } }

		//---------------------------------------------------------------------
		public bool Working
		{
			get
			{
				Application.DoEvents();
				return working;
			}
		}

		//---------------------------------------------------------------------
		public SourceControlManager SourceControlManager
		{
			get
			{
				return sControlMng;
			}
			set
			{
				if (sControlMng != null)
					sControlMng.Dispose();

				sControlMng = value;
				sControlMng.OnSourceTreeRefreshNeeded += new EventHandler(SourceSafeManager_OnSourceTreeRefreshNeeded);
				sControlMng.CompareExecutablePathChanged += new EventHandler(SourceSafeManager_CompareExecutablePathChanged);
			}
		}

		//---------------------------------------------------------------------
		public bool BatchBuild { get { return batchBuild; } }

		//---------------------------------------------------------------------
		public string Solution { get { return tblslnWriter.FileName; } set { tblslnWriter.FileName = value; } }
		//---------------------------------------------------------------------
		public SolutionDocument SolutionDocument { get { return tblslnWriter; } }
		//---------------------------------------------------------------------
		public AssemblyGenerator.ConfigurationType Configuration
		{
			get
			{
				return SolutionDocument.LocalInfo.Configuration;
			}
			set
			{
				SolutionDocument.LocalInfo.Configuration = value;
			}
		}

		//---------------------------------------------------------------------
		public bool CountTemporaryAsTranslated
		{
			get
			{
				return SolutionDocument.LocalInfo.CountTemporaryAsTranslated;
			}
			set
			{
				SolutionDocument.LocalInfo.CountTemporaryAsTranslated = value;
			}
		}

		//---------------------------------------------------------------------
		public bool RapidCreation
		{
			get
			{
				return SolutionDocument.LocalInfo.RapidCreation;
			}
			set
			{
				SolutionDocument.LocalInfo.RapidCreation = value;
			}
		}

		//---------------------------------------------------------------------
		public bool ShowTranslationProgress
		{
			get
			{
				return SolutionDocument.LocalInfo.ShowTranslationProgress;
			}
			set
			{
				SolutionDocument.LocalInfo.ShowTranslationProgress = value;
			}
		}

		//---------------------------------------------------------------------
		public LocalizerTreeNode SolutionNode { get { return ProjectsTreeView.Nodes[0] as LocalizerTreeNode; } }
		//---------------------------------------------------------------------
		public LocalizerTreeNode SelectedNode
		{
			get { return ProjectsTreeView.SelectedNode as LocalizerTreeNode; }
			set { ProjectsTreeView.SelectedNode = value; }
		}

		#endregion

		/// <summary>
		///Struct che contiene la risposta a askUpdate.
		/// </summary>
		//---------------------------------------------------------------------
		private struct DictionaryMode
		{
			public bool applyAll;
			public DialogResult result;

			/// <summary>
			/// Costruttore
			/// </summary>
			/// <param name="applyAll">applica a tutti i progetti</param>
			/// <param name="result">dialogResult della AskDialog</param
			//---------------------------------------------------------------------
			public DictionaryMode(bool applyAll, DialogResult result)
			{
				this.applyAll = applyAll;
				this.result = result;
			}
		}

		/// <summary>
		/// Classe derivata da HashTable in maniera che restituisca già il cast anzichè l'oggetto.
		/// </summary>
		//=========================================================================
		public class ProjectDocumentTable : Hashtable
		{
			//---------------------------------------------------------------------
			public ProjectDocumentTable()
				: base(StringComparer.InvariantCultureIgnoreCase)
			{

			}
			/// <summary>
			/// Implementazione del nostro indexer che sfrutta il base a restituisce il cast
			/// </summary>
			//---------------------------------------------------------------------
			public ProjectDocument this[string key]
			{
				get { return base[key] as ProjectDocument; }
				set { base[key] = value; }
			}
		}

		//---------------------------------------------------------------------
		public DictionaryCreator()
			: this(null)
		{
		}

		//---------------------------------------------------------------------
		public DictionaryCreator(Form owner)
		{
			Debug.Assert(MainContext == null, "Only one DictionaryCreator object is allowed");
			MainContext = this;

			DictionaryCreator.owner = owner;
			Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

			InitializeComponent();
			if (!DesignMode)
				PostInitializeComponent();
		}

		/// <summary>
		/// Popola il datagrid della startpage e aggiunge l'evento al closing.
		/// </summary>
		//---------------------------------------------------------------------
		private void PostInitializeComponent()
		{
			Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
			globalWordCounter = new WordCounter(this);
			selectedNodeList = new ArrayList();
			tblPrjFiles = new ProjectDocumentTable();
			commonDictionaries = new ArrayList();
			tblslnWriter = new SolutionDocument(this.batchBuild, this.batchLogger);
			iniWriter = new IniDocument();
			enabilitation = new EnabledButtons(true, true);

			//AddSourceControlImages();

			// initializes referenced assemblies delegates
			SourceBinding.CommonFunctions.LogicalPathToPhysicalPathFunction = new SourceBinding.CommonFunctions.LogicalPathToPhysicalPathDelegate(CommonFunctions.LogicalPathToPhysicalPath);
			SourceBinding.CommonFunctions.GetEnvironmentVariableFunction = new SourceBinding.CommonFunctions.GetEnvironmentVariableDelegate(CommonFunctions.GetEnvironmentVariable);
			CommonUtilities.Functions.CalculateChildNodesFunction = new CommonUtilities.Functions.CalculateChildNodesDelegate(CalculateChildNodes);
			CommonUtilities.Functions.GetWordInfoStringFunction = new Microarea.Tools.TBLocalizer.CommonUtilities.Functions.GetWordInfoStringDelegate(globalWordCounter.GetWordInfoString);
			CommonUtilities.Functions.GetFiltersFunction = new Microarea.Tools.TBLocalizer.CommonUtilities.Functions.GetFiltersDelegate(globalWordCounter.GetFilters);
			CommonUtilities.Functions.AvailableFiltersFunction = new Microarea.Tools.TBLocalizer.CommonUtilities.Functions.GetAvailableFiltersDelegate(globalWordCounter.AvailableFilters);
			CommonUtilities.Functions.IsUsingFiltersFunction = new Microarea.Tools.TBLocalizer.CommonUtilities.Functions.IsUsingFiltersDelegate(globalWordCounter.IsUsingFilters);

			SourceControlManager = new SourceControlManager("", this, "", new Logger(MyProgressBar, MyStatusBar, TxtOutput));

			tblslnWriter.FileNameChanged += new FileNameChangedEventHandler(tblslnWriter_FileNameChanged);
			ReadAndFillGrid();
			ProjectsTreeViewFreezer = new ControlFreezer(ProjectsTreeView);

			Closing += new CancelEventHandler(DictionaryCreator_Closing);
			LocalizerTreeNode root = new LocalizerTreeNode("", "", NodeType.SOLUTION);
			ProjectsTreeView.Nodes.Add(root);
			//hides all context menu items
			AdjustContextMenu(null);
			globalLogger = new Logger(MyProgressBar, MyStatusBar, TxtOutput);

			this.HelpRequested += new HelpEventHandler(Microarea.TaskBuilderNet.Core.Generic.HelpManager.HelpRequested);
		}

		//--------------------------------------------------------------------------------
		private void MergeImage(Bitmap newImage, string mergeImageNs)
		{
			Image img = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(mergeImageNs));
			using (Graphics g = Graphics.FromImage(newImage))
			{
				g.DrawImageUnscaled(img, 0, 0);
			}
		}

		//--------------------------------------------------------------------------------
		public void Stop()
		{
			working = false;
		}

		//---------------------------------------------------------------------
		/* public void StartListening()
		 {
			 //se sono in produzione
			 string workingPath = Path.GetDirectoryName(Application.ExecutablePath);
			 string path = Path.Combine(workingPath, "Aspx");
			 if (!Directory.Exists(path))
			 {
				 //se sono in sviluppo
				 for (int i = 0; i < 4; i++) path = Path.GetDirectoryName(path);
				 path = Path.Combine(path, "WoormViewerAspx");

				 if (!Directory.Exists(path))
					 path = Path.Combine(Path.GetPathRoot(System.Environment.SystemDirectory), "Inetpub\\wwwroot\\WoormViewerAspx");
			 }

			 Debug.Assert(Directory.Exists(path), string.Format("Path not found: {0}", path));
			 woormServer = new WoormViewerAspx.Server("/TBLocalizer", path, SocketPort);
			 SocketPort = woormServer.Port;
		 }*/

		//---------------------------------------------------------------------
		/* public void StopListening()
		 {
			 if (woormServer != null)
			 {
				 woormServer.Dispose();
				 woormServer = null;
			 }
		 }*/

		/// <summary>
		/// Restituisce la lista di nodi progetto che sono i children della root.
		/// </summary>
		//---------------------------------------------------------------------
		public TreeNodeCollection GetProjectNodeCollection()
		{
			return SolutionNode.Nodes;
		}

		/// <summary>
		/// Disegna il datagrid della startpage accedendo al file tblocalizer.tbl.
		/// </summary>
		//---------------------------------------------------------------------
		private void ReadAndFillGrid()
		{
			iniWriter.InitDocument(AllStrings.solutions, null, AllStrings.INI);
			iniWriter.SaveAndShowError("DictionaryCreator - FillGrid", AllStrings.INI, true);
			FillGrid();
		}
		private void FillGrid()
		{
			//queste stringhe possono NON ESSERE inserite nelle AllStrings
			const string header = "header";
			const string hidden = "hidden";
			const string tableName = "table";
			DisableMenuItems();
			DataTable table = new DataTable(tableName);
			//ora che ho dato il nome alla table setto il mapping name della table-style
			MyTableStyle.MappingName = table.TableName;
			XmlNodeList allPath = iniWriter.GetAllSolutionPaths();
			string captionText = (allPath.Count == 0) ? Strings.EmptyDataGridTitle : Strings.DataGridTitle;
			table.Columns.Add(header);
			table.Columns.Add(hidden);
			//prima riga vuota
			DataRow drFirst = table.NewRow();
			drFirst[header] = drFirst[hidden] = String.Empty;
			table.Rows.Add(drFirst);
			//leggo dal file e popolo la table
			foreach (XmlNode e in allPath)
			{
				DataRow dr = table.NewRow();
				dr[header] = GetSolutionNameFromPath(e.Value);
				dr[hidden] = e.Value;
				table.Rows.Add(dr);
			}
			DgStart.CaptionText = captionText;
			DgStart.DataSource = table;

			//Sposto il focus su un altro control per non
			//visualizzare il rettangolo grigio di selezione del DgStart
			LnkNew.Select();
		}

		/// <summary>
		/// Disabilita / abilita i menu items quando start page attivata.
		/// </summary>
		//---------------------------------------------------------------------
		private void DisableMenuItems()
		{
			EnableMenuItemsTo(false);

			MiClose.Enabled =
				MiNew.Enabled =
				MiOpen.Enabled = true;

			EnableProjectMenuItems();
			OnMenuStateChanged();
		}

		/// <summary>
		/// Gestisce l'abilitazione/disabilitazione delle voci di menu legate ai progetti selezionati
		/// </summary>
		//---------------------------------------------------------------------
		private void EnableProjectMenuItems()
		{
			MiRemoveProjects.Enabled =
				MiDictionary.Enabled =
				MiZipDictionary.Enabled =
				MiBuildSelectedProjects.Enabled = ProjectMenuItemsEnabled;

			OnMenuStateChanged();
		}

		/// <summary>
		/// Scrive la root del tree.
		/// </summary>
		/// <param name="clear">specifica se azzerare la solution</param>
		/// <param name="pathSln">path del file della solution</param>
		//---------------------------------------------------------------------
		private void WriteFirstNode(bool clear, string pathSln)
		{
			if (clear)
			{
				docCounter++;
				countPrj = 0;
				Solution = AllStrings.default_root + docCounter.ToString();
			}

			string solutionName = GetSolutionNameFromPath(Solution);

			string prj = (countPrj == 1) ? AllStrings.project : AllStrings.projects;

			NodeTag aTag = new NodeTag(pathSln, solutionName, NodeType.SOLUTION);
			aTag.Details = " (" + countPrj.ToString() + " " + prj + ")";

			SolutionNode.Tag = aTag;
		}

		//---------------------------------------------------------------------
		private string GetSolutionNameFromPath(string path)
		{
			if (path == null || path == String.Empty)
				return String.Empty;
			return Path.GetFileNameWithoutExtension(path);
		}

		//---------------------------------------------------------------------
		private void MiStop_Click(object sender, System.EventArgs e)
		{
			if (Working)
			{
				if (MessageBox.Show(this, Strings.ConfirmAbort, Strings.MainFormCaption, MessageBoxButtons.YesNo) == DialogResult.No) return;
				Stop();
			}
		}

		//---------------------------------------------------------------------
		private void BuildDictionaries()
		{
			if (Configuration == AssemblyGenerator.ConfigurationType.CFG_NONE) return;

			ArrayList projectList = MakeProjectList(false, !RapidCreation);

			int errors = 0, oks = 0;
			foreach (LocalizerTreeNode projNode in projectList)
			{
				if (!Working) return;

				ProjectDocument aTblPrjWriter = GetPrjWriter(projNode);

				if (aTblPrjWriter == null) continue;

				foreach (LocalizerTreeNode child in projNode.Nodes)
				{
					if (!Working) return;

					if (child.Type != NodeType.LANGUAGE || child.IsBaseLanguageNode)
						continue;

					batchLogger.WriteLog(string.Format(Strings.BuildingDictionary, child.FullPath));

					ArrayList resxFiles = new ArrayList();
					ArrayList xmlFiles = new ArrayList();
					string file;
					foreach (DictionaryTreeNode fileNode in child.GetTypedChildNodes(NodeType.LASTCHILD, true))
					{
						file = fileNode.FileSystemPath;
						if (CommonFunctions.IsResx(file))
							resxFiles.Add(file);
						else if (CommonFunctions.IsXML(file) && !xmlFiles.Contains(file))
							xmlFiles.Add(file);
					}

					bool result = true;

					result = result && (
						(resxFiles.Count == 0 && projNode.ReferencedNodes.Length == 0)
						||
						AssemblyGenerator.CreateDictionaryForResx
						(
						resxFiles,
						(DictionaryTreeNode)child,
						projNode.ReferencedNodes,
						aTblPrjWriter,
						batchLogger,
						Configuration
						));
                    if (aTblPrjWriter.GetFileType() == ProjectDocument.ProjectType.NG)
                    {
                        result = result && (xmlFiles.Count == 0 || DataDocumentFunctions.CreateDictionaryForAngular
                            (
                            xmlFiles,
                            (DictionaryTreeNode)child,
                            batchLogger,
                            Path.Combine(aTblPrjWriter.SourceFolder, AllStrings.dictionary, child.Name)
                            )
                            );
                    }
                    else
                    {
                        result = result && (xmlFiles.Count == 0 || DataDocumentFunctions.CreateDictionaryForXml
                            (
                            xmlFiles,
                            (DictionaryTreeNode)child,
                            Configuration,
                            batchLogger
                            ));

                    }
					if (result)
						oks++;
					else
						errors++;

				}
			}

			if (batchLogger != null && (oks + errors) > 0)
				batchLogger.WriteLog
					(
					string.Format
					(
					Strings.SatelliteProcedure,
					oks,
					errors
					),
					TypeOfMessage.info
					);
		}

		//---------------------------------------------------------------------
		public LocalizerTreeNode GetProjectNodeFromSourcePath(string sourcePath)
		{
			string modifiedPath = sourcePath.ToLower();
			foreach (LocalizerTreeNode projNode in GetProjectNodeCollection())
			{
				if (modifiedPath.StartsWith(projNode.SourcesPath.ToLower()))
					return projNode;
			}

			return null;
		}

		//---------------------------------------------------------------------
		private void MiBuildSolution_Click(object sender, System.EventArgs e)
		{
			BuildSolution();
		}

		//---------------------------------------------------------------------
		private void MiBuildSelectedProjects_Click(object sender, System.EventArgs e)
		{
			BuildProjects(false);
		}

		//---------------------------------------------------------------------
		private void MiBuildSelectedProjectsHelp_Click(object sender, System.EventArgs e)
		{
			BuildProjects(false);

		}

		//---------------------------------------------------------------------
		private void MiBuildSolutionHelp_Click(object sender, System.EventArgs e)
		{

		}

		//--------------------------------------------------------------------------------
		public void DoBatchBuild(string solution)
		{
			batchBuild = true;
			OpenSolution(solution);

			try
			{
				BuildSolution();
			}
			finally
			{
				batchBuild = false;
			}
		}

		//--------------------------------------------------------------------------------
		public void BuildSolution()
		{
			BuildProjects(true);
		}


		//---------------------------------------------------------------------
		public void BuildProjects(bool selectAll)
		{
			if (
				(!SolutionDocument.LocalInfo.EnvironmentSettings.BuildDictionary)
				)
			{
				if (!BatchBuild)
					MessageBox.Show(this, Strings.BuildNotEnabled);
				return;
			}

			if (!BatchBuild && MessageBox.Show
				(
				this,
				Strings.BuildSolution,
				"TbLocalizer",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question
				) == DialogResult.No)
				return;

			if (selectAll)
				SelectAll(true);

			EnableControl(false);
			batchLogger = new Logger(MyProgressBar, MyStatusBar, BatchBuild ? null : TxtOutput);

			Cursor = Cursors.WaitCursor;
			bool tmpEnable = SourceControlManager.Enabled;
			bool made = false;
			try
			{
				BuildDictionaries();
				made = true;
			}
			catch (Exception ex)
			{
				batchLogger.WriteLog(ex.Message, TypeOfMessage.error);
			}
			finally
			{
				SourceControlManager.Enabled = tmpEnable;
				Cursor = Cursors.Default;
				batchLogger.WriteLog(made ? Strings.ProcedureEnded : Strings.ProcedureEndedWithErrors, TypeOfMessage.info);
				EnableControl(true);
				batchLogger.SaveLog();
			}
		}

		//---------------------------------------------------------------------
		private void MiCloseSol_Click(object sender, System.EventArgs e)
		{
			CloseSolution(true);
		}

		//---------------------------------------------------------------------
		private void MiSelectAll_Click(object sender, System.EventArgs e)
		{
			SelectAll(true);
		}

		//---------------------------------------------------------------------
		private void MiTFSSourceBinding_Click(object sender, EventArgs e)
		{
			SolutionDocument.LocalInfo.SourceControlEnabled = SourceControlManager.SetTFSDatabaseConnection();
			RefreshSourceControlStatusAsync();
		}

		//---------------------------------------------------------------------
		public void SelectAll(bool select)
		{
			TreeNode root = SolutionNode;
			if (root == null) return;
			foreach (TreeNode child in root.Nodes)
				SelectProject(child, true);
		}

		//---------------------------------------------------------------------
		private void ShowTree(bool show)
		{
			if (show)
			{
				PanelStartPage.SendToBack();
				PanelStartPage.Visible = false;

				PanelTreeView.BringToFront();
				PanelTreeView.Visible = true;
			}
			else
			{
				PanelTreeView.SendToBack();
				PanelTreeView.Visible = false;

				PanelStartPage.BringToFront();
				PanelStartPage.Visible = true;
			}
		}

		/// <summary>
		/// Ripresenta la startPage.
		/// </summary>
		//---------------------------------------------------------------------
		private bool CloseSolution(bool askSaving)
		{
			if (!TranslatorCache.CloseAllTranslators()) return false;

			if (askSaving && !AskSaving()) return false;

			StopUpdatingThread();
			StopSCCRefreshingThread();
			SolutionDocument.LocalInfo.Save();

			ClearAll();
			ShowTree(false);

			//hides all context menu items
			AdjustContextMenu(null);

			ReadAndFillGrid();

			return true;
		}

		/// <summary>
		/// Apre nuova solution da menu.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiNew_Click(object sender, System.EventArgs e)
		{
			NewSolution();
		}

		/// <summary>
		/// Apre nuova solution da link.
		/// </summary>
		//---------------------------------------------------------------------
		private void LnkNew_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			NewSolution();
		}

		/// <summary>
		///Apre nuova solution.
		/// </summary>
		//---------------------------------------------------------------------
		public void NewSolution()
		{
			if (!CloseSolution(true)) return;

			ShowTree(true);
			EnableMenuItemsTo(true);
			OnMenuStateChanged();

			TxtOutput.Clear();
			WriteFirstNode(true, null);
			tblslnWriter.InitDocument(AllStrings.solution, true);

			EnableProjectMenuItems();

			//hides all context menu items
			AdjustContextMenu(null);
		}

		//---------------------------------------------------------------------
		private void EnableMenuItemsTo(bool enabled)
		{
			MiManageProjects.Enabled =
			MiSave.Enabled =
			MiSaveAs.Enabled =
			MiUnzipDictionary.Enabled =
			MiCloseSol.Enabled =
			MiDir.Enabled =
			MiXml.Enabled =
			MiSelectAll.Enabled =
			MiRefresh.Enabled =
			MiCollapse.Enabled =
			MiBuildSolution.Enabled =
			MiSourceControl.Enabled = enabled;
		}

		/// <summary>
		///Pulisce il file di indice e il tree l'elenco dei 
		///progetti selezionati e l'insieme dei file tblprj. 
		/// </summary>
		/// <param name="isNew">specifica è una solution nuova(quindi mai salvata)</param>
		//---------------------------------------------------------------------
		private void ClearAll()
		{
			tblslnWriter.ClearDocument();
			selectedNodeList.Clear();
			tblPrjFiles.Clear();
			LocalizerDocument.RemoveAllDictionariesFromCache();
			ClearTreeView();
		}

		/// <summary>
		/// Svuota il treeview.
		/// </summary>
		//---------------------------------------------------------------------
		private void ClearTreeView()
		{
			GetProjectNodeCollection().Clear();
		}

		/// <summary>
		///Richiesta help.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiHelp_Click(object sender, System.EventArgs e)
		{
			Microarea.TaskBuilderNet.Core.Generic.HelpManager.HelpRequested(this, null);
		}

		/// <summary>
		///Richiesta di aggiunta progetto alla solution.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiManageProjects_Click(object sender, System.EventArgs e)
		{
			ManageProjects();
		}

		/// <summary>
		///Apre la OpenFileDialog per la scelta del file.
		/// </summary>
		//---------------------------------------------------------------------
		private void ManageProjects()
		{
			SolutionEditor f = new SolutionEditor(tblslnWriter);
			if (f.ShowDialog(this) != DialogResult.OK) return;

			DeleteAllProjects();
			foreach (string prj in f.SelectedProjects)
				AddProject(prj);
			RefreshTree();
		}

		/// <summary>
		/// Aggiunge al tree il file di progetto selezionato.
		/// </summary>
		//---------------------------------------------------------------------
		public void AddProject(string logicalSourceFilePath)
		{
			string tblPrjPath = tblslnWriter.DictionaryPathFinder.GetTblprjPath(logicalSourceFilePath);
			string sourcesPath = tblslnWriter.DictionaryPathFinder.GetSourcesPath(logicalSourceFilePath); ;

			string projectName = CommonFunctions.GetProjectName(tblPrjPath);

			TreeNode treeNode = new LocalizerTreeNode(tblPrjPath, projectName, NodeType.PROJECT);

			treeNode.ImageIndex = treeNode.SelectedImageIndex = (int)Images.PROJECT;
			TreeNodeCollection list = GetProjectNodeCollection();

			list.Add(treeNode);
			countPrj++;
			WriteFirstNode(false, Solution);

			tblslnWriter.WriteProject(tblPrjPath);

			if (tblPrjFiles.Contains(tblPrjPath))
				return;

			ProjectDocument.ProjectType extensionType;
			ArrayList projectPaths = DataDocumentFunctions.GetProjectFiles(sourcesPath, out extensionType);

			ProjectDocument aTblprjWriter = new ProjectDocument();
			aTblprjWriter.InitializeTblPrj(sourcesPath, tblPrjPath, extensionType);
			tblPrjFiles[tblPrjPath] = aTblprjWriter;

			if (extensionType == ProjectDocument.ProjectType.NONE)
				extensionType = aTblprjWriter.GetFileType();
			try
			{
				if (extensionType == ProjectDocument.ProjectType.CS)
				{
					aTblprjWriter.SetReferencesCouple(DataDocumentFunctions.ReadRealReferences(projectPaths[0] as string, globalLogger), list);
					tblslnWriter.modified = true;
				}

				aTblprjWriter.SaveAndShowError(Strings.WarningCaption, true);
			}
			catch (Exception)
			{
				MessageBox.Show(this, String.Format(Strings.ReferencesProblem, tblPrjPath), "DictionaryCreator.FillDirectoryTree");
			}

			ManageReferences(true, tblPrjPath);
		}

		//---------------------------------------------------------------------
		private void MiDictionary_Select(object sender, System.EventArgs e)
		{
			if (!MiDictionary.Enabled)
				return;

			if (selectedNodeList.Count == 0)
			{
				MiCreate.Enabled = false;
				MiCustomCreate.Enabled = false;
				MiAddDictionary.Enabled = false;
				OnMenuStateChanged();
			}
			else
			{
				MiCreate.Enabled = true;
				MiCustomCreate.Enabled = true;
				MiAddDictionary.Enabled = true;
				OnMenuStateChanged();
			}

		}

		//---------------------------------------------------------------------
		public ArrayList GetAvailableDictionaries()
		{
			return GetAvailableDictionaries(null, false);
		}

		//---------------------------------------------------------------------
		public ArrayList GetAvailableDictionaries(LocalizerTreeNode projectNode, bool excludeBaseLanguage)
		{
			Debug.Assert(projectNode == null ||
				projectNode.Type == NodeType.PROJECT ||
				projectNode.Type == NodeType.SOLUTION,
				"Invalid node type"
				);

			ArrayList nodes = null;

			if (projectNode == null)
				nodes = GetDictionaryNodes();
			else
			{
				nodes = new ArrayList();
				nodes.AddRange(projectNode.GetTypedChildNodes(NodeType.LANGUAGE, false));
			}

			CaseInsensitiveStringCollection dictionaries = new CaseInsensitiveStringCollection();
			foreach (LocalizerTreeNode n in nodes)
			{
				if (excludeBaseLanguage && n.IsBaseLanguageNode)
					continue;

				if (!dictionaries.Contains(n.Name))
					dictionaries.Add(n.Name);
			}

			return dictionaries;
		}

		//---------------------------------------------------------------------
		public ArrayList GetDictionaryNodes()
		{
			return SolutionNode.GetTypedChildNodes(NodeType.LANGUAGE, false);
		}

		//---------------------------------------------------------------------
		public ArrayList GetDictionaryNodes(string culture)
		{
			return SolutionNode.GetTypedChildNodes(NodeType.LANGUAGE, false, culture, true);
		}

		/// <summary>
		/// Se ci sono e Quali sono i dizionari comuni ai progetti selezionati.
		/// </summary>
		//---------------------------------------------------------------------
		private bool ExistCommonDictionary()
		{
			commonDictionaries.Clear();
			if (selectedNodeList.Count == 0)
				return false;

			int totalDictionary = selectedNodeList.Count;
			ArrayList allDictionaries = new ArrayList();
			//per ogni progetto selezionato popolo un string[]
			//con i dizionari esistenti
			for (int j = 0; j < totalDictionary; j++)
			{
				TreeNode node = (TreeNode)selectedNodeList[j];
				string[] dictionaryList = new string[node.GetNodeCount(false)];
				for (int k = 0; k < node.GetNodeCount(false); k++)
				{
					string language = ((LocalizerTreeNode)node.Nodes[k]).Name;
					dictionaryList.SetValue(language, k);
				}
				//aggiungo i vari string[] ad un comune arraylist(like a jagged array)
				allDictionaries.Add(dictionaryList);
			}
			int i;
			bool make = false;
			//verifico che gli elementi del primo string[] siano o meno presenti negli altri
			//non è necessario proseguire con gli altri o 
			//continuare se un elemento è mancante in uno degli altri string[]
			foreach (string firstelement in ((string[])allDictionaries[0]))
			{
				bool thereIs = false;
				for (i = 1; i < totalDictionary; i++)
				{
					foreach (string s in ((string[])allDictionaries[i]))
					{
						if (String.Compare(s, firstelement, true) != 0)
							continue;
						else
						{
							thereIs = true;
							break;
						}
					}
					if (!thereIs)
						break;
					else
					{
						if (i == totalDictionary - 1)
							//solo se sono al numero di presenze richiesto aggiungo il dictionary
							commonDictionaries.Add(firstelement);
						else
							thereIs = false;
					}
				}

				if (totalDictionary == 1 && !make)
				{
					commonDictionaries.AddRange(((string[])allDictionaries[0]));
					make = true;
				}
			}
			//l'eventuale cartella fantasma delle reference non deve essere considerata
			if (commonDictionaries.Contains(AllStrings.referencesCap))
				commonDictionaries.Remove((AllStrings.referencesCap));
			//poi il menu item verrà abilitato solo se ci sono dictionary comuni a tutti i prj selezionati
			return (commonDictionaries.Count > 0);
		}

		//---------------------------------------------------------------------
		private void MiCreate_Click(object sender, System.EventArgs e)
		{
			CreateDictionaries(false);
		}

		//---------------------------------------------------------------------
		private void MiCustomCreate_Click(object sender, System.EventArgs e)
		{
			CreateDictionaries(true);
		}

		//--------------------------------------------------------------------------------
		private void MiAddDictionary_Click(object sender, System.EventArgs e)
		{
			AddDictionary();
		}

		//---------------------------------------------------------------------
		private void MiImportDictionary_Click(object sender, System.EventArgs e)
		{
			ChooseOldRootName f = new ChooseOldRootName();
			if (f.ShowDialog(this) != DialogResult.OK)
				return;

			LocalizerDocument.RemoveAllDictionariesFromCache();
			ArrayList projectList = MakeProjectList(false, false);
			Logger logWriter = new Logger(MyProgressBar, MyStatusBar, TxtOutput);
			DictionaryConverter converter = new DictionaryConverter(logWriter, f.RootName);
			EnableControl(false);
			try
			{
				converter.ImportDictionaries(projectList);
				logWriter.SaveLog();
			}
			finally
			{
				EnableControl(true);
			}
		}

		//---------------------------------------------------------------------
		public void AddDictionary()
		{
			ChooseLanguage languagesDialog = new ChooseLanguage();
			languagesDialog.FillCombo(String.Empty);
			languagesDialog.FormTitle = Strings.LanguageCaption;
			if (languagesDialog.ShowDialog(this) == DialogResult.OK)
			{
				ArrayList projectList = MakeProjectList(false, !RapidCreation);
				foreach (LocalizerTreeNode project in projectList)
				{
					if (project.GetTypedChildNodes(NodeType.LANGUAGE, false, languagesDialog.ChoosedLanguage.Name, true).Count > 0)
					{
						MessageBox.Show(this, string.Format(Strings.DictionaryAlreadyExisting, languagesDialog.ChoosedLanguage.Name, project.Name));
						continue;
					}

					string path = tblslnWriter.DictionaryPathFinder.GetDictionaryFolder(project, languagesDialog.ChoosedLanguage.Name, true);

					foreach (DictionaryTreeNode cultureNode in project.GetTypedChildNodes(NodeType.LANGUAGE, false, DictionaryTreeNode.BaseLanguage, true))
					{
						ArrayList paths = new ArrayList();
						foreach (DictionaryTreeNode leaf in cultureNode.GetTypedChildNodes(NodeType.LASTCHILD, true))
						{
							string fileName = Path.GetFileName(leaf.FileSystemPath);
							if (CommonFunctions.IsResx(fileName))
								fileName = DictionaryFile.ResxDictionaryFileName;
							else if (!CommonFunctions.IsXML(fileName))
								continue;
							if (!paths.Contains(fileName))
							{
								paths.Add(fileName);
								DictionaryFile dFile = new DictionaryFile(fileName);
								dFile.InitDocument(path);
								dFile.Save(path);
							}
						}
					}

				}
				RefreshTree();
			}
		}

		//---------------------------------------------------------------------
		private void EnableControl(bool enable)
		{
			//per mandare la chiamata al thread giusto
			/*Begin*/
			Invoke((ThreadStart)delegate //Sincrono per evitare che la variabile "working" che pilota la compilazione dei dizionari venga impostata troppo in ritardo e così la compilazione dei dizionari non parta neanche.
			{
					ProjectsTreeView.Enabled = enable;

					foreach (MenuItem item in MainContextMenu.MenuItems)
						item.Enabled = enable;

					BtnStopProcedure.Visible = !enable;
					MyProgressBar.Visible = !enable;
					working = !enable;

					OnMenuStateChanged();
				});
		}

		//---------------------------------------------------------------------
		public bool CreateDictionaries(bool askForGenerationSettings)
		{
			EnableControl(false);
			FreezeTree();
			try
			{
				LocalizerDocument.RemoveAllDictionariesFromCache();

				StopSCCRefreshingThread();
				StopUpdatingThread();

				string prj = String.Empty;
				ArrayList projectList = MakeProjectList(!RapidCreation, !RapidCreation);

				if (projectList.Count == 0)
				{
					MessageBox.Show(this, Strings.NoProjectSelected, Strings.WarningCaption,
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}

				SetContext(true);

				Logger logWriter = new Logger(MyProgressBar, MyStatusBar, TxtOutput);

				//salvo solution
				if (!SaveSolution(false, IsExistingSolution))
					return false;

				GenerationSettings settings = null;
				if (askForGenerationSettings)
				{
					DictionaryGenerationSettings f = new DictionaryGenerationSettings();
					if (DialogResult.OK != f.ShowDialog(this))
						return false;
					settings = f.Settings;
				}

				CreatorManager(projectList, settings, logWriter);

				// salvo i progetti, perché la procedura di creazione del dizionario potrebbe 
				// averli modificati
				SaveAllTblPrj(false);

				logWriter.SaveLog();
				object[] args = { projectList };
				RefreshTree();
			}
			finally
			{
				LocalizerDocument.RemoveAllDictionariesFromCache();
				DefreezeTree();
				SetContext(false);
				EnableControl(true);
			}
			return true;
		}

		/// <summary>
		/// Imposta il contesto grafico per la preparazione del dizionario.
		/// </summary>
		/// <param name="begin">indica se la procedura sta iniziando(true) o è finita(false)</param>
		//---------------------------------------------------------------------
		private void SetContext(bool begin)
		{
			//progressbar e clessidra per mostrare che c'è un processo in atto
			//visualizzo anche nome del file in parsing nella statusbar
			MyProgressBar.Visible = begin;
			if (begin)
			{
				Cursor = Cursors.WaitCursor;
				TxtOutput.Visible = true;
				TxtOutput.Clear();
				MyProgressBar.Value = 0;
			}
			else
			{
				Cursor = Cursors.Default;
				MyStatusBar.Text = String.Empty;
			}
		}

		/// <summary>
		/// Se necessario controlla le references e chiede l'update, 
		/// poi procede con il metodo opportuno per la tipologia di procedura scelta.
		/// </summary>
		/// <param name="lang">lingua scelta per il dizionario(codice e parola)</param>
		/// <param name="template">codice lingua template</param>
		/// <param name="allPrj">lista di tutti i progetti coinvolti</param>
		//---------------------------------------------------------------------
		private void CreatorManager(ArrayList allPrj, GenerationSettings settings, Logger logWriter)
		{
			string dictionaryPath = String.Empty;

			DictionaryMode dictionaryMode = new DictionaryMode(false, DialogResult.Yes);

			SourceFileParser parser = new SourceFileParser
				(
				logWriter,
				tblslnWriter.ReadXmlExtension(),
				tblslnWriter.ReadPhysicalIncludesPath(),
				string.Empty,
				null,
				settings
				);
			LocalizerDocument.RemoveAllDictionariesFromCache();
			//devo usare tutti i progetti, anche quelli referenziati,li ho tutti in allPrj
			foreach (LocalizerTreeNode n in allPrj)
			{
				string message = String.Empty;
				string prjName = n.Name;
				DictionaryFileCollection files = null;
				try
				{
					if (!Working) return;

					ProjectDocument aTblPrjWriter = GetPrjWriter(n);
					parser.ProjectDocument = aTblPrjWriter;

					ProjectDocument.ProjectType extension = aTblPrjWriter.GetFileType();

					dictionaryPath = tblslnWriter.DictionaryPathFinder.GetDictionaryFolder(n, LocalizerTreeNode.BaseLanguage, false);
					if (!AskRegeneration(n, dictionaryPath, ref dictionaryMode))
						continue;

					if (!DataDocumentFunctions.DeleteIndex(dictionaryPath))
						logWriter.WriteLog(Strings.ReadOnlyIndex, TypeOfMessage.error);

					files = tblslnWriter.CreateDictionary
						(
						logWriter,
						n.SourcesPath,
						dictionaryPath,
						aTblPrjWriter.FileName,
						parser
						);

					//se il file è saltato per qualche motivo lo aggiungo alla lista.
					string key = n.FullPath;
					if (tblPrjFiles.Contains(key))
						tblPrjFiles.Remove(key);
					tblPrjFiles.Add(key, aTblPrjWriter);
				}
				catch (Exception exc)
				{
					MessageBox.Show(this, exc.Message + "\n" + exc.StackTrace, "DictionaryCreator - CreatorManager");
					continue;
				}
				try
				{
					if (settings == null || !(settings.Include || settings.Exclude))
						DataDocumentFunctions.AdjustDictionary(dictionaryPath, files);
				}
				catch (UnauthorizedAccessException)
				{
					//non ripeto il messaggio per questa eccezione, è già stato intercettato nella fase precedente.
				}
				catch (Exception exc)
				{
					logWriter.WriteLog(exc.Message, TypeOfMessage.error);
				}
			}
		}

		/// <summary>
		/// Restituisce la lista di tutti i dizionari coinvolti nella creazione del dizionario(selezionati e loro referenze)
		/// </summary>
		//---------------------------------------------------------------------
		private ArrayList MakeProjectList(bool askForMissingReferences, bool listAlsoReferences)
		{
			ArrayList list = new ArrayList();

			// se non devo chiedere per le eventuali referenze mancanti, 
			// imposto da subito a true il booleano che indica di 
			// processare il progetto (toAdd) e indico di applicare
			// tale setting a tutti i progetti (toAll)
			bool toAll = !askForMissingReferences;
			bool toAdd = true;
			foreach (LocalizerTreeNode n in selectedNodeList)
			{
				ProjectDocument aTblPrjWriter = GetPrjWriter(n);
				if (aTblPrjWriter == null) continue;
				ProjectDocument.ProjectType type = aTblPrjWriter.GetFileType();
				StringBuilder message = new StringBuilder();
				ArrayList partialList = new ArrayList();
				if (listAlsoReferences && type == ProjectDocument.ProjectType.CS)
				{
					//per ogni progetto  C#  coinvolto nella creazione dei dizionari, 
					//devo andare a cercare i relativi references, leggendoli dal file tblprj
					ArrayList messageList = new ArrayList();
					AddReferences(n, partialList, aTblPrjWriter, ref messageList);
					foreach (MissingReference mr in messageList)
						message.Append(mr.ToString());
					if (!toAll && message.Length != 0)
					{
						MissingReferences mr = new MissingReferences
							(
							n.Name,
							message.ToString()
							);
						DialogResult r = mr.ShowDialog(this);
						toAdd = (r == DialogResult.Yes);
						toAll = mr.ToAll;
					}
				}
				if (toAdd || message.Length == 0)
				{
					foreach (TreeNode partialNode in partialList)
					{
						if (!list.Contains(partialNode))
							list.Add(partialNode);
					}
					if (!list.Contains(n)) list.Add(n);
				}
			}
			return list;
		}

		/// <summary>
		/// Aggiunge le references del relativo progetto alla lista di progetti coinvolti nella crezione del dizionario
		/// </summary>
		/// <param name="n">nodo del tree da trattare</param>
		/// <param name="list">lista dei progetti coinvolti nella creazione del dizionario</param>
		/// <param name="aTblPrjWriter">datadocument relativo al progetto</param>
		//---------------------------------------------------------------------
		private void AddReferences(LocalizerTreeNode n, ArrayList list, ProjectDocument aTblPrjWriter, ref ArrayList messageList)
		{
			XmlNodeList emptyList = aTblPrjWriter.GetEmptyReferences();
			if (emptyList != null)
			{
				foreach (XmlNode node in emptyList)
				{
					MissingReference mr = new MissingReference(n.Name, node.Attributes[AllStrings.name].Value);
					if (!messageList.Contains(mr))
						messageList.Add(mr);
				}
			}
			ArrayList referencesList = aTblPrjWriter.GetReferences();

			foreach (DictionaryReference reference in referencesList)
			{
				if (reference.Project == String.Empty) continue;

				LocalizerTreeNode nodeToAdd = CheckSolution(reference.Project);

				if (nodeToAdd == null || nodeToAdd == n) continue;//non esiste nella solution


				n.AddReferencedNode(nodeToAdd);

				ProjectDocument tempTblPrjWriter = GetPrjWriter(nodeToAdd);
				if (tempTblPrjWriter != null)
					AddReferences(nodeToAdd, list, tempTblPrjWriter, ref messageList);

				if (!list.Contains(nodeToAdd))
					list.Add(nodeToAdd);
			}
		}

		/// <summary>
		/// Restituisce il nodo del tree relativo al progetto in questione, se appartenente alla solution.
		/// </summary>
		/// <param name="reference">nome del progetto da verificare</param>
		//---------------------------------------------------------------------
		private LocalizerTreeNode CheckSolution(string referenceProject)
		{
			TreeNodeCollection collection = GetProjectNodeCollection();
			foreach (LocalizerTreeNode n in collection)
			{
				//posso paragonare solo il nome (NodeName) perchè 
				//non possono esserci 2 prj con lo stesso nome
				string nameToCompare = n.Name;
				if (String.Compare(nameToCompare, referenceProject, true) == 0)
					return n;
			}
			return null;
		}

		/// <summary>
		/// Verifica se il nodo selezionato è un file tblprj o meno.
		/// </summary>
		/// <param name="node">nodo da verificare</param>
		//---------------------------------------------------------------------
		private bool IsTblprj(LocalizerTreeNode node)
		{
			return node.Type == NodeType.PROJECT && CommonFunctions.IsTblprj(node.FileSystemPath);
		}


		/// <summary>
		/// Chiede cosa fare se il dizionario per la lingua selezionata esiste già. Se ritorna false il progetto viene saltato.
		/// </summary>
		/// <param name="path">path del dizionario</param>
		/// <param name="prj">nome del progetto</param>
		/// <param name="updating">verrà settato a true se sarà richiesto un update</param>
		//---------------------------------------------------------------------
		private bool AskRegeneration(LocalizerTreeNode projectNode, string dictionaryPath, ref DictionaryMode dictionaryMode)
		{
			//se ritorna false il progetto in questione viene saltato
			if (Directory.Exists(dictionaryPath)) //se esiste chiedo cosa si vuol fare
			{
				if (!dictionaryMode.applyAll)
				{
					AskUpdate askUpdateDialog = new AskUpdate(projectNode.FullPath);
					dictionaryMode.result = askUpdateDialog.ShowDialog(this);
					dictionaryMode.applyAll = askUpdateDialog.ApplyAll;
				}

				if (dictionaryMode.result == DialogResult.Cancel)
					return false;
				if (dictionaryMode.result == DialogResult.Yes)
					return true;

				try
				{
					//altrimenti se rigenerazione, cancello e ricreo.
					CommonUtilities.Functions.SafeDeleteFolder(dictionaryPath);
				}
				catch (UnauthorizedAccessException exc)
				{
					MessageBox.Show(this, exc.Message, "DictionaryCreator - AskRegeneration (Unauthorized)");
					return false;
				}
				catch (IOException exc)
				{
					MessageBox.Show(this, exc.Message, "DictionaryCreator - AskRegeneration (IOException)");
					return true;
				}
				catch (Exception exc)
				{
					MessageBox.Show(this, exc.Message, "DictionaryCreator - AskRegeneration");
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Fill del tree leggendo da filesystem
		/// includendo i dizionari esistenti ed eventualmente 
		/// aggiungendo cartelle fantasma(references C#)
		/// </summary>
		//---------------------------------------------------------------------
		private bool TreeViewReviewer()
		{
			//performance trick
			FreezeTree();
			try
			{
				StopUpdatingThread();
				StopSCCRefreshingThread();

				tblslnWriter.Cache.Init(EnvironmentSettings.Key, SolutionDocument.LocalInfo.EnvironmentSettings);
				LocalizerDocument.RemoveAllDictionariesFromCache();
				dictionaries.Clear();

				//supponendo che il secondo livello di nodi siano i progetti
				foreach (LocalizerTreeNode node in GetProjectNodeCollection())
				{
					if (node != null)
						AddCultureNodes(node);
				}

				CalculateChildNodes(SolutionNode, true);

				Functions.OrderNodes(GetProjectNodeCollection());
			}
			finally
			{
				SolutionNode.Expand();
				DefreezeTree();
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		private void AddCultureNodes(LocalizerTreeNode projectNode)
		{
			try
			{
				//salta la cartella vuota e continua con le altre.
				foreach (string folder in tblslnWriter.DictionaryPathFinder.GetDictionaryFolders(projectNode))
				{
					if (!Directory.Exists(folder))
						Directory.CreateDirectory(folder);

					string culture = CommonFunctions.GetCulture(folder);
					if (culture == null)
						continue;

					if (!dictionaries.Contains(culture))
						dictionaries.Add(culture);

					if (IsHiddenCulture(culture))
						continue;

					DictionaryTreeNode newNode = new DictionaryTreeNode(folder, culture, NodeType.LANGUAGE);
					newNode.ImageIndex = newNode.SelectedImageIndex = (int)Images.LANGUAGE;
					bool toAdd = true;
					foreach (DictionaryTreeNode n in projectNode.Nodes)
						if (string.Compare(n.FileSystemPath, newNode.FileSystemPath, true) == 0)
						{
							toAdd = false;
							break;
						}

					if (toAdd)
					{
						projectNode.Nodes.Add(newNode);

						//se non si tradda di un dizionario di lingua base, aggiungo dinamicamente il file di dizionario
						if (!newNode.IsBaseLanguageNode)
						{
							ProjectDocument tblPrjWriterTmp = GetPrjWriter(projectNode);
							string fileName = "";

							if (tblPrjWriterTmp.IsVcProject())
								fileName = DictionaryFile.DictionaryFileName;
							else if (tblPrjWriterTmp.IsCsProject())
								fileName = DictionaryFile.ResxDictionaryFileName;

							//controllo se esiste il file, se non esiste lo aggiungo
							if (!string.IsNullOrEmpty(fileName) && !File.Exists(Path.Combine(folder, fileName)))
							{
								DictionaryFile dFile = new DictionaryFile(fileName);
								dFile.InitDocument(folder);
								dFile.Save(folder);
							}
						}
					}
				}
			}
			catch (DirectoryNotFoundException)
			{
				DialogResult result = MessageBox.Show
					(
					this,
					Strings.DamagedFile,
					Strings.WarningCaption,
					MessageBoxButtons.OK,
					MessageBoxIcon.Stop,
					MessageBoxDefaultButton.Button1
					);
				ClearAll();
				MiManageProjects.Enabled = false;
				MiSelectAll.Enabled = false;
				OnMenuStateChanged();

				return;
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, exc.Message, "DictionaryCreator - TreeViewReviewer");
				return;
			}
		}

		//--------------------------------------------------------------------------------
		private void ProjectsTreeView_AfterCollapse(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if (e.Node.ImageIndex == (int)Images.FOLDEROPENED)
			{
				e.Node.ImageIndex = (int)Images.FOLDERCLOSED;
				e.Node.SelectedImageIndex = (int)Images.FOLDERCLOSED;
			}
		}

		//--------------------------------------------------------------------------------
		private void ProjectsTreeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			LocalizerTreeNode n = e.Node as LocalizerTreeNode;
			if (n == null)
				return;
			MyStatusBar.Text = n.FileSystemPath;
		}

		/// <summary>
		/// Aggiunge la cartella fantasma delle references per i progetti c#.
		/// </summary>
		/// <param name="node">nodo al quale appendere la cartella</param>
		//---------------------------------------------------------------------
		private void AddReferenceFolder(LocalizerTreeNode node)
		{
			ProjectDocument tblPrjWriterTmp = GetPrjWriter(node);
			if (tblPrjWriterTmp != null && tblPrjWriterTmp.GetFileType() == ProjectDocument.ProjectType.CS)
			{
				int indexToRemove = -1;
				for (int i = 0; i < node.Nodes.Count; i++)
				{
					if (((LocalizerTreeNode)node.Nodes[i]).Name == AllStrings.referencesCap)
					{
						indexToRemove = i;
						break;
					}
				}
				if (indexToRemove > -1)
					node.Nodes.RemoveAt(indexToRemove);
				TreeNode ghostNode = PrepareGhostReferenceFolder(tblPrjWriterTmp);
				if (ghostNode != null) node.Nodes.Add(ghostNode);

			}

		}

		/// <summary>
		/// Restituisce il nodo preparato delle references da appendere al nodo del progetto.
		/// </summary>
		/// <param name="tblPrjWriterTmp">datadocument relativo al progetto</param>
		//---------------------------------------------------------------------
		private TreeNode PrepareGhostReferenceFolder(ProjectDocument tblPrjWriterTmp)
		{
			ArrayList list = tblPrjWriterTmp.GetReferences();
			if (list.Count == 0) return null;

			TreeNode[] children = new TreeNode[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				DictionaryReference aReference = (DictionaryReference)list[i];
				string name = aReference.Name;
				string project = aReference.Project;

				int imageIndex = (int)Images.REFERENCE;
				if (project == String.Empty)
					imageIndex = (int)Images.REFERENCEEMPTY;

				TreeNode n = new LocalizerTreeNode(name, name, NodeType.REFERENCE);
				n.ImageIndex = n.SelectedImageIndex = imageIndex;
				children.SetValue(n, i);
			}
			TreeNode ghostNode = new LocalizerTreeNode(AllStrings.referencesCap, AllStrings.referencesCap, NodeType.REFERENCEBLOCK);
			ghostNode.Nodes.AddRange(children);
			ghostNode.ImageIndex = (int)Images.REFERENCES;
			ghostNode.SelectedImageIndex = (int)Images.REFERENCESOPEN;
			return ghostNode;
		}

		//---------------------------------------------------------------------
		internal static ArrayList GetMainDictionaries(string prjName, ArrayList dictionaries)
		{
			ArrayList mainDictionary = new ArrayList();
			foreach (string dictionary in dictionaries)
			{
				string foldeName = Directory.GetParent(dictionary).Name;
				if (String.Compare(foldeName, prjName, true) == 0)
					mainDictionary.Add(dictionary);
			}
			return mainDictionary;
		}

		/// <summary>
		/// Restituisce la lista dei dizionari per la determinata lingua.
		/// </summary>
		/// <param name="language">codice della lingua per cui bisogna cercare i dizionari</param>
		/// <param name="mainDictionary">lista di dizionari frai i quali cercare</param>
		//---------------------------------------------------------------------
		internal static ArrayList GetDictionaryOfLanguage(string language, ArrayList mainDictionaries)
		{
			ArrayList languageDictionaries = new ArrayList();
			foreach (string dictionary in mainDictionaries)
			{
				string[] work = Directory.GetDirectories(dictionary, language);
				if (work != null && work.Length != 0)
					languageDictionaries.AddRange(work);
			}
			return languageDictionaries;
		}

		//---------------------------------------------------------------------
		private void CloneDictionaryContent(DictionaryTreeNode dictionaryNode, DictionaryTreeNode sourceNode, string culture)
		{
			foreach (DictionaryTreeNode n in sourceNode.Nodes)
			{
				string path = (n.FileSystemPath.Length == 0)
					? string.Empty
					: CommonFunctions.GetCorrespondingLanguagePath(n.FileSystemPath, culture);

				DictionaryTreeNode clone = new DictionaryTreeNode
					(
					path,
					n.Name,
					n.Type
					);
				clone.GroupIdentifier = n.GroupIdentifier;
				clone.ResourceType = n.ResourceType;
				clone.ImageIndex = n.ImageIndex;
				clone.SelectedImageIndex = n.SelectedImageIndex;

				dictionaryNode.Nodes.Add(clone);
				CloneDictionaryContent(clone, n, culture);
			}
		}

		//--------------------------------------------------------------------------------
		public void BeginCopyDictionariesAndOpenNewSolution(string newFolder)
		{
			BeginInvoke(new CopyDictionariesAndOpenNewSolutionDelegate(CopyDictionariesAndOpenNewSolution), new object[] { newFolder });
		}
		//--------------------------------------------------------------------------------
		public delegate void CopyDictionariesAndOpenNewSolutionDelegate(string newFolder);
		public void CopyDictionariesAndOpenNewSolution(string newFolder)
		{
			Logger logWriter = new Logger(MyProgressBar, MyStatusBar, TxtOutput);
			DictionaryPathFinder newPf = newFolder.Length == 0 ? new DictionaryPathFinder() : new ExternalDictionaryPathFinder(newFolder);
			tblslnWriter.DeleteAllProjects();
			foreach (LocalizerTreeNode prjNode in GetProjectNodeCollection())
			{
				logWriter.WriteLog(string.Format(Strings.CopyingProject, prjNode.FullPath), TypeOfMessage.info);
				string newProject = newPf.GetTblprjPath(Path.Combine(prjNode.SourcesPath, "dummy"));

				if (File.Exists(newProject))
				{
					DialogResult res = Functions.RepeatableMessage(this, true, Strings.ProjectAlreadyExists, newProject);
					if (res == DialogResult.Cancel)
					{
						logWriter.WriteLog(Strings.ProcedureAborted);
						OpenExistingSolution(Solution, false);
						return;
					}
					else if (res == DialogResult.No)
					{
						logWriter.WriteLog(string.Format(Strings.ProjectAlreadyExists, newProject), TypeOfMessage.warning);
						continue;
					}
				}
				tblslnWriter.WriteProject(newProject);


				if (!Functions.SafeCopyFile(prjNode.FileSystemPath, newProject, true))
				{
					logWriter.WriteLog(string.Format(Strings.ErrorCopyingProject, prjNode.FullPath), TypeOfMessage.warning);
					continue;
				}

				ProjectDocument p = new ProjectDocument();
				p.Load(newProject);
				p.SourceFolder = prjNode.SourcesPath;
				p.SaveAndLogError(logWriter);

				foreach (DictionaryTreeNode dictNode in prjNode.GetTypedChildNodes(NodeType.LANGUAGE, false))
				{
					Application.DoEvents();
					logWriter.WriteLog(string.Format(Strings.CopyingDictionary, dictNode.FullPath), TypeOfMessage.info);
					string targetFolder = newPf.GetDictionaryFolderFromTblPrjPath(newProject, dictNode.Culture, false);
					if (Directory.Exists(targetFolder))
					{
						DialogResult res = Functions.RepeatableMessage(this, true, Strings.DictionaryAlreadyExists, targetFolder);
						if (res == DialogResult.Cancel)
						{
							logWriter.WriteLog(Strings.ProcedureAborted);
							OpenExistingSolution(Solution, false);
							return;
						}
						else if (res == DialogResult.No)
						{
							logWriter.WriteLog(string.Format(Strings.DictionaryAlreadyExists, targetFolder), TypeOfMessage.warning);
							continue;
						}
					}
					if (!Functions.SafeCopyFolder(dictNode.FileSystemPath, targetFolder, true))
					{
						logWriter.WriteLog(string.Format(Strings.ErrorCopyingDictionary, dictNode.FullPath), TypeOfMessage.warning);
						continue;
					}
				}
			}

			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.Description = Strings.SelectNewSolutionPath;
			dlg.SelectedPath = Path.Combine(newFolder, LocalizerTreeNode.BaseLanguage);
			while (dlg.ShowDialog(this) != DialogResult.OK) ;

			string newSolution = Path.Combine(dlg.SelectedPath, Path.GetFileName(Solution));

			logWriter.WriteLog(string.Format(Strings.CreatingSolution, newSolution), TypeOfMessage.info);
			tblslnWriter.DictionaryRootPath = newFolder;
			tblslnWriter.SaveAndLogError(logWriter, newSolution);

			logWriter.WriteLog(string.Format(Strings.OpeningSolution, newSolution), TypeOfMessage.info);
			OpenExistingSolution(newSolution, false);

			logWriter.WriteLog(Strings.Finished, TypeOfMessage.info);
		}

		//---------------------------------------------------------------------
		private void ReadDictionaryContent(DictionaryTreeNode dictionaryNode)
		{
			//se il conto è zero non ho mai aperto queto nodo, 
			//altrimenti ho già le info che mi servono nel treenode
			if (dictionaryNode.Nodes.Count != 0) return;

			if (dictionaryNode.PrevNode != null)
			{
				CloneDictionaryContent(dictionaryNode, dictionaryNode.PrevNode as DictionaryTreeNode, dictionaryNode.Culture);
				return;
			}

			string dictionaryPath = CommonFunctions.GetCorrespondingBaseLanguagePath(dictionaryNode.FileSystemPath);

			if (!Directory.Exists(dictionaryPath))
			{
				CommonUtilities.Functions.RepeatableMessage(DictionaryCreator.MainContext, Strings.BaseLanguagePathNotFound, dictionaryPath, SolutionDocument.BaseLanguageDisplayName);
				return;
			}

			try
			{
				foreach (string file in Directory.GetFiles(dictionaryPath, "*.xml"))
				{
					if (string.Compare(Path.GetFileName(file), AllStrings.resourceIndex, true) == 0)
						continue;

					DictionaryFile f = new DictionaryFile("");
					f.Parse(dictionaryNode, file);
				}

				//ho un array di DictionaryPositionInfo che contengono path ed eventuale company per le custom
				foreach (string directory in Directory.GetDirectories(dictionaryPath))
				{
					string modifiedPath = CommonFunctions.GetCorrespondingLanguagePath(directory, dictionaryNode.Name);

					DictionaryTreeNode tempNode = dictionaryNode.AddUnique(modifiedPath, Path.GetFileName(directory), NodeType.RESOURCE);

					foreach (string file in Directory.GetFiles(directory))
					{
						if (!IsFileAsLastChild(file))
							continue;

						string extension = Path.GetExtension(file).ToLower();

						//scarto i file che non sono dentro alle cartelle
						if (
							string.Compare(extension, AllStrings.resxExtension, true) != 0 &&
							string.Compare(extension, AllStrings.htmExtension, true) != 0
							)
							continue;

						bool isResx = CommonFunctions.IsResx(file);

						string nodeName = Path.GetFileNameWithoutExtension(file);
						modifiedPath = CommonFunctions.GetCorrespondingLanguagePath(file, dictionaryNode.Name);

						DictionaryTreeNode lastNode = tempNode.AddUnique(modifiedPath, nodeName, NodeType.LASTCHILD);

						string resourceType = Path.GetFileName(directory);

						lastNode.ImageIndex = lastNode.SelectedImageIndex =
							CommonFunctions.GetImageIndexFromName(resourceType, null);
						lastNode.ResourceType = resourceType;
						lastNode.GroupIdentifier = nodeName;


					}//foreach file

					if (tempNode.Nodes.Count == 0)
						dictionaryNode.Nodes.Remove(tempNode);

				}//foreach directory

			}
			catch (UnauthorizedAccessException)
			{
				//se qualche cartella non é accessibile la salto, risulterà vuota
				//ometto l'avvertimento che intanto è inutile.
			}
			catch (Exception exc)
			{
				Functions.RepeatableMessage(this, Functions.ExtractMessages(exc));
			}
		}

		//---------------------------------------------------------------------
		public LocalizerTreeNode GetCultureNodeFromFileSystemPath(string path)
		{
			string culture = CommonFunctions.GetCulture(path);
			foreach (LocalizerTreeNode node in SolutionNode.GetTypedChildNodes(NodeType.LASTCHILD, true, null, true, culture))
			{
				if (string.Compare(node.FileSystemPath, path, true) == 0)
					return node.GetTypedParentNode(NodeType.LANGUAGE);
			}
			return null;
		}

		//---------------------------------------------------------------------
		public bool GetNodeFromPath(string fullPath, out LocalizerTreeNode currentNode)
		{
			string[] tokens = fullPath.Split('\\');
			currentNode = SolutionNode;
			int i;
			for (i = 1; i < tokens.Length; i++)
			{
				string token = tokens[i];
				LocalizerTreeNode foundNode = null;
				foreach (LocalizerTreeNode node in currentNode.Nodes)
					if (string.Compare(node.Name, token, true) == 0)
					{
						foundNode = node;
						break;
					}

				if (foundNode == null)
					break;
				else
					currentNode = foundNode;
			}

			return i == tokens.Length;
		}

		/// <summary>
		/// Refresha il tree 
		/// </summary>
		//---------------------------------------------------------------------
		public void RefreshTree()
		{
			try
			{
				try
				{
					StopUpdatingThread();
					StopSCCRefreshingThread();
					WriteFirstNode(false, Solution);

					Cursor = Cursors.WaitCursor;
					LocalizerTreeNode selected = SelectedNode;
					string fullPath = (selected != null) ? selected.FullPath : "";

					foreach (TreeNode n in GetProjectNodeCollection())
						n.Nodes.Clear();

					TreeViewReviewer();

					LocalizerTreeNode currentNode;
					GetNodeFromPath(fullPath, out currentNode);

					SelectedNode = currentNode;
				}
				finally
				{
					UpdateDetailsAsync();
					RefreshSourceControlStatusAsync();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, string.Format(Strings.ErrorRefreshingTree, ex.Message));
			}

		}


		/// <summary>
		/// Cambia icona e sfondo ai progetti selezionati, visualizza eventuali contextMenu.
		/// </summary>
		//---------------------------------------------------------------------
		private void ProjectsTreeView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			clickPosition = new Point(e.X, e.Y);

			LocalizerTreeNode tempNode = ProjectsTreeView.GetNodeAt(clickPosition) as LocalizerTreeNode;
			if (tempNode == null) return;

			bool rightClick = (e.Button == MouseButtons.Right);
			bool leftClick = (e.Button == MouseButtons.Left);

			NodeType nodeType = tempNode.Type;

			if (leftClick)
			{
				bool controlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
				if (!controlPressed)
				{
					//creao un array copia per non avere errori di tipo "è cambiata la collection..." 
					TreeNode[] tempList = (TreeNode[])selectedNodeList.ToArray(typeof(TreeNode));
					foreach (TreeNode n in tempList)
						ToggleProject(n);
				}

				//Gestione del click per selezione singola, Click+control per selezione multipla
				if (nodeType == NodeType.PROJECT)
					ToggleProject(tempNode);
			}

			SelectedNode = tempNode;

			// hides or shows context menu items depending on the selected node
			AdjustContextMenu(tempNode);
		}



		//---------------------------------------------------------------------
		private void AdjustContextMenu(LocalizerTreeNode aNode)
		{

			NodeType nodeType = aNode != null ? aNode.Type : NodeType.NULL;

			MiTranslate.Visible = (nodeType == NodeType.LASTCHILD);
			MiRecover.Visible = (nodeType == NodeType.LASTCHILD);
			MiSetReference.Visible = (nodeType == NodeType.REFERENCE);
			MiUpdateReferences.Visible = true;


			MIPurge.Visible =
				nodeType == NodeType.SOLUTION
				||
				nodeType == NodeType.PROJECT
				|| (nodeType == NodeType.LANGUAGE && aNode.IsBaseLanguageNode);

			MiCount.Visible = (nodeType != NodeType.REFERENCE && nodeType != NodeType.REFERENCEBLOCK && nodeType != NodeType.NULL);
			MiRemove.Visible = (nodeType == NodeType.LANGUAGE) || (nodeType == NodeType.LASTCHILD && aNode.IsBaseLanguageNode);
			MiCheckDialogs.Visible = (nodeType != NodeType.REFERENCE && nodeType != NodeType.REFERENCEBLOCK && nodeType != NodeType.NULL);
			MiProgressSpec.Visible =
				MiPlaceholder.Visible =
				MiAmpersand.Visible =
				MiTranslation.Visible =
				(nodeType == NodeType.SOLUTION || nodeType == NodeType.PROJECT || nodeType == NodeType.LANGUAGE || nodeType == NodeType.RESOURCE || nodeType == NodeType.LASTCHILD);
			MiGlossary.Visible =
				MiFind.Visible =
				(nodeType == NodeType.SOLUTION || nodeType == NodeType.PROJECT || nodeType == NodeType.LANGUAGE || nodeType == NodeType.RESOURCE);

			MiAutoTranslate.Visible =
				MiTranslateFromLanguage.Visible =
				(nodeType == NodeType.SOLUTION || nodeType == NodeType.PROJECT || nodeType == NodeType.LANGUAGE || nodeType == NodeType.RESOURCE || nodeType == NodeType.LASTCHILD);

			MiTranslateFromKnowledge.Visible = true;
			MiTranslateJson.Visible = true;

			MiGetLatest.Enabled = MiCheckIn.Enabled = MiCheckOut.Enabled = MiUndoCheckOut.Enabled = SourceControlManager.Enabled;
			OnMenuStateChanged();
		}

		//---------------------------------------------------------------------
		private void ToggleProject(TreeNode aNode)
		{
			SelectProject(aNode, aNode.ImageIndex != (int)Images.SELECTED);
		}

		//---------------------------------------------------------------------
		private void SelectProject(TreeNode aNode, bool select)
		{
			if (select)
			{
				if (selectedNodeList.Contains(aNode)) return;
				aNode.ImageIndex = aNode.SelectedImageIndex = (int)Images.SELECTED;
				selectedNodeList.Add(aNode);

				CommonFunctions.SetNodeColor(aNode, CommonFunctions.NodeColors.SELECTED);
			}
			else
			{
				aNode.ImageIndex = aNode.SelectedImageIndex = (int)Images.PROJECT;
				selectedNodeList.Remove(aNode);

				CommonFunctions.SetNodeColor(aNode, CommonFunctions.NodeColors.DEFAULT);
			}

			EnableProjectMenuItems();
		}

		/// <summary>
		/// Richiesta di apertura solution esistente da menu.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiOpen_Click(object sender, System.EventArgs e)
		{
			OpenSolution();
		}

		/// <summary>
		/// Richiesta di apertura solution esistente da link.
		/// </summary>
		//---------------------------------------------------------------------
		private void LnkOpen_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			OpenSolution();
		}

		/// <summary>
		/// Apertura solution esistente, visualizza openfiledialog.
		/// </summary>
		//---------------------------------------------------------------------
		private void OpenSolution()
		{
			OpenFileDialog openDialog = new OpenFileDialog();
			openDialog.InitialDirectory =
				solutionsFolder == string.Empty ?
				AllStrings.MainAssemblyPath :
				solutionsFolder;
			openDialog.Filter = AllStrings.FILTERSLN;
			openDialog.Title = Strings.OpenProjectCaption;

			if (openDialog.ShowDialog(this) == DialogResult.OK)
			{
				if (!CloseSolution(true))
					return;

				solutionsFolder = openDialog.FileName;
				OpenExistingSolution(solutionsFolder, true);
			}
		}

		//---------------------------------------------------------------------
		private void ResetStatus(string pathsln)
		{
			iniWriter.INIModifier(pathsln, null);
			MessageBox.Show(this, Strings.UnreachSolution, Strings.WarningCaption);
			CloseSolution(true);
			LnkNew.Select();
		}

		//---------------------------------------------------------------------
		public bool OpenSolution(string pathsln)
		{
			if (!CloseSolution(!BatchBuild)) return false;

			OpenExistingSolution(pathsln, !BatchBuild);
			return true;
		}

		//---------------------------------------------------------------------
		public void CreateKnowledge(string culture)
		{
			string currentKnowledgeCulture = null;
			CreateKnowledge(ref currentKnowledgeCulture, culture, SolutionNode);
		}

		//---------------------------------------------------------------------
		private void CreateKnowledge(ref string currentKnowledgeCulture, string culture, TreeNode node)
		{
			foreach (LocalizerTreeNode n in node.Nodes)
			{
				switch (n.Type)
				{
					case NodeType.LANGUAGE:
						{
							string currentCulture = n.Name;
							if (culture != null && string.Compare(culture, currentCulture, true) != 0)
								continue;
							currentKnowledgeCulture = currentCulture;
							break;
						}
					case NodeType.LASTCHILD:
						{
							KnowledgeManager.AddKnowledgeItems(currentKnowledgeCulture, n.FullPath, ((DictionaryTreeNode)n).GetStringNodes(), true);
							continue;
						}
				}

				CreateKnowledge(ref currentKnowledgeCulture, culture, n);
			}
		}

		/// <summary>
		/// Apre solution esistente.
		/// </summary>
		/// <param name="pathsln">path del file tblsln</param>
		//---------------------------------------------------------------------
		private void OpenExistingSolution(string pathsln, bool askSaving)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				FreezeTree();

				if (!CloseSolution(askSaving)) return;

				ShowTree(true);

				if (!tblslnWriter.Load(pathsln))
				{
					ResetStatus(pathsln);
					return;
				}

				string pattern = @"(\\|/)Standard(\\|/)";   //nuova struttura di cartelle
				Match m = Regex.Match(pathsln, pattern, RegexOptions.IgnoreCase);
				if (m.Success)
				{
					string installationPath = pathsln.Substring(0, m.Index);
					if (!askSaving)
					{
						SolutionDocument.LocalInfo.EnvironmentSettings.InstallationPath = installationPath;
						SolutionDocument.InitPathFinder();
					}
					else
					{
						string currentInst = SolutionDocument.LocalInfo.EnvironmentSettings.InstallationPath;
						if (string.Compare(installationPath, currentInst, true) != 0)
						{
							if (DialogResult.Yes ==
								MessageBox.Show
								(
								this,
								string.Format(
								@"'InstallationPath' environment variable (current value: '{0}') is not consistent with the one inferred by solution path;
    Do you want to change it to '{1}'?", currentInst, installationPath),
								Strings.WarningCaption,
								MessageBoxButtons.YesNo,
								MessageBoxIcon.Warning
								))
							{
								SolutionDocument.LocalInfo.EnvironmentSettings.InstallationPath = installationPath;
								SolutionDocument.InitPathFinder();
							}
						}
					}
				}
				string rootPath = SolutionDocument.DictionaryRootPath;
				if (!String.IsNullOrEmpty(rootPath))
				{
					if (IsOldStructured(rootPath))
					{
						rootPath = RemoveOldStructureTokens(rootPath);
						if (Directory.Exists(rootPath))
							SolutionDocument.DictionaryRootPath = rootPath;
					}
					else if (!Directory.Exists(rootPath))
					{ //se non esiste la cartella, provo e metterci il path di solution decurtato della cartella di lingua
						SolutionDocument.DictionaryRootPath = Path.GetDirectoryName(Path.GetDirectoryName(pathsln));
					}
				}
				CommonUtilities.LocalizerTreeNode.BaseLanguage = tblslnWriter.BaseLanguage;

				string[] projectList = tblslnWriter.ReadProjects();
				CommonFunctions.LogicalPathToPhysicalPath(projectList);

				bool mod = false;
				for (int i = 0; i < projectList.Length; i++)
				{
					string p = projectList[i];
					if (IsOldStructured(p))
					{
						//sono nella nuova struttura di cartelle della 3.0?
						p = RemoveOldStructureTokens(p);
						if (File.Exists(p))
						{
							projectList[i] = p; //sono nella nuova struttura di cartelle della 3.0!
							mod = true;
						}
					}
					else if (!File.Exists(p))
					{
						//se non esiste provo a cercarlo relativamente al path di solution
						string searchPath = Path.GetDirectoryName(pathsln);
						string[] files = Directory.GetFiles(searchPath, Path.GetFileName(p), SearchOption.AllDirectories);

						if (files.Length == 1)
						{
							projectList[i] = files[0]; //sono nella nuova struttura di cartelle della 3.0!
							mod = true;
						}
					}
				}

				if (mod)
					tblslnWriter.WriteProjects((string[])projectList.Clone());

				bool modified = false;
				string prjToUpdate = Environment.NewLine;
				foreach (string prjPath in projectList)
				{
					string folder;

					try
					{
						folder = tblslnWriter.DictionaryPathFinder.GetSourcesPath(prjPath);
					}
					catch (Exception)
					{
						if (BatchBuild)
						{
							throw;
						}
						else
						{
							continue;
						}
					}

					if (String.IsNullOrEmpty(folder))
					{
						if (BatchBuild)
						{
							throw new ApplicationException(String.Format("This project does not exist: {0}. Program terminated.", prjPath));
						}
						else
						{
							continue;
						}
					}

					if (IsOldStructured(folder))
					{
						string newFolder = RemoveOldStructureTokens(folder);
						if (Directory.Exists(newFolder))
						{
							folder = newFolder;
							ProjectDocument prj = DictionaryCreator.MainContext.GetPrjWriter(prjPath);
							prj.SourceFolder = folder;
						}
					}
				}
				if (modified && !BatchBuild)//nel batchLog lo scrive alla fine
					TxtOutput.AppendText(String.Format(Strings.ModifiedPrj, GetSolutionNameFromPath(pathsln), prjToUpdate));

				//se la soluzione non si riesce ad aprire annullo operazione
				if (!TreeViewDesigner(pathsln, projectList)) return;
				//altrimenti procedo nella creazione del tree
				iniWriter.WriteINI(Solution);

				EnableMenuItemsTo(true);
				OnMenuStateChanged();

				//hides all context menu items
				AdjustContextMenu(null);

				EnableProjectMenuItems();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, AllStrings.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
				iniWriter.RemoveSolution(pathsln);
				iniWriter.SaveAndShowError(AllStrings.error, true);
				ShowTree(false);
				FillGrid();
			}
			finally
			{
				Cursor = Cursors.Default;
				DefreezeTree();
			}
		}

		//---------------------------------------------------------------------
		private static string RemoveOldStructureTokens(string folderOrFile)
		{
			return Regex.Replace(folderOrFile, "(microareaserver\\\\)|(running\\\\)", "", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}

		//---------------------------------------------------------------------
		private static bool IsOldStructured(string folderOrFile)
		{
			return folderOrFile.IndexOf("microareaserver", StringComparison.InvariantCultureIgnoreCase) != -1 &&
				!Directory.Exists(folderOrFile) &&
				!File.Exists(folderOrFile);
		}

		/// <summary>
		/// Apre solution esistente da start page, sensibile al cambio di cella attiva del DgStart.
		/// </summary>
		//---------------------------------------------------------------------
		private void DgStart_CurrentCellChanged(object sender, System.EventArgs e)
		{
			//colonna 1 = hidden col percorso completo
			string name = (string)DgStart[DgStart.CurrentRowIndex, 1];
			if (name != null && name != String.Empty)
				OpenExistingSolution(name, true);
		}

		/// <summary>
		/// Legge xml e popola il treeview. Ritorna un bool che indica il buon fine.
		/// </summary>
		/// <param name="pathSln">path del file tblsln</param>
		/// <param name="list">lista dei nodi contenenti i progetti apparteneti alla solution</param>
		//---------------------------------------------------------------------
		private bool TreeViewDesigner(string pathSln, string[] list)
		{
			//se list è null, c'è qualche problema nell'apertura della solution
			// e mostro la start page. 

			if (list == null)
			{
				ResetStatus(pathSln);
				return false;
			}
			foreach (string prjPath in list)
			{

				// converto il nome del file di progetto a *.tblprj
				// (modifica necessaria per compatibiità pregressa con i vecchi files di solution)
				string tblPrjFullPath = tblslnWriter.DictionaryPathFinder.GetTblprjPath(prjPath);

				TreeNode treeNode = new LocalizerTreeNode(tblPrjFullPath, CommonFunctions.GetProjectName(tblPrjFullPath), NodeType.PROJECT);
				treeNode.ImageIndex = treeNode.SelectedImageIndex = (int)Images.PROJECT;
				GetProjectNodeCollection().Add(treeNode);
			}
			bool ok = TreeViewReviewer();
			countPrj = SolutionNode.Nodes.Count;
			Solution = Path.ChangeExtension(pathSln, AllStrings.slnExtension);

			WriteFirstNode(!ok, pathSln);

			UpdateDetailsAsync();
			RefreshSourceControlStatusAsync();

			return true;
		}

		/// <summary>
		/// Richiesta di salvataggio del file della solution.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiSave_Click(object sender, System.EventArgs e)
		{
			SaveSolution(true, IsExistingSolution);
		}

		/// <summary>
		/// Richiesta di salvataggio con nome del file della solution.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiSaveAs_Click(object sender, System.EventArgs e)
		{
			tblslnWriter.modified = true;
			SaveSolution(true, false);
		}

		/// <summary>
		/// Propone il salvataggio se il file non è mai stato salvato.
		/// </summary>
		//---------------------------------------------------------------------
		public bool SaveSolution(bool includeProjects, bool existing)
		{
			if (includeProjects && !SaveAllTblPrj(!existing))
				return false;

			if (existing)
			{
				tblslnWriter.SaveAndShowError("DictionaryCreator - CloseAndSaveXML", true);
				return true;
			}

			// non apre la FileDialog se il file è già salvato
			SaveFileDialog saveDialog = new SaveFileDialog();
			saveDialog.Filter = AllStrings.FILTERSLN;
			saveDialog.InitialDirectory = solutionsFolder;
			saveDialog.FileName = Solution;
			saveDialog.Title = Strings.SaveSolutionCaption;

			if (saveDialog.ShowDialog(this) == DialogResult.OK)
			{
				Solution = saveDialog.FileName;
				WriteFirstNode(false, Solution);
				tblslnWriter.SaveAndShowError("DictionaryCreator - SaveSolution", Solution, true);
				iniWriter.WriteINI(Solution);

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Cancella progetti selezionati dalla solution.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiRemoveProjects_Click(object sender, System.EventArgs e)
		{
			//cancello da file e da treeview e da selectedNodeList e 
			//da hashtablee svuoto tutte le sue references se ci sono
			if (selectedNodeList.Count < 1)
			{
				MessageBox.Show(this, Strings.NoProjectSelected, Strings.WarningCaption);
				return;
			}
			string prj = String.Empty;
			foreach (LocalizerTreeNode n in selectedNodeList)
				prj += " " + n.Name + ",";
			DialogResult result = MessageBox.Show
				(
				this,
				String.Format(Strings.RemoveProjectQuestion, prj.Remove(prj.LastIndexOf(","), 1)),
				Strings.WarningCaption,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Information,
				MessageBoxDefaultButton.Button1
				);
			if (result == DialogResult.No) return;

			DeleteProjects(selectedNodeList);
		}

		//--------------------------------------------------------------------------------
		private void DeleteProjects(ArrayList prjNodes)
		{
			ArrayList prjNames = new ArrayList();

			foreach (LocalizerTreeNode tn in prjNodes)
			{
				prjNames.Add(tn.FileSystemPath);

				GetProjectNodeCollection().Remove(tn);
				countPrj--;
				//cancello anche dalla lista dei tblprj
				tblPrjFiles.Remove(tn.FileSystemPath);
			}

			tblslnWriter.DeleteProjects(prjNames);

			ManageReferences(false, null);
			prjNodes.Clear();
			WriteFirstNode(false, Solution);
			EnableProjectMenuItems();
		}

		//--------------------------------------------------------------------------------
		private void DeleteAllProjects()
		{
			ArrayList prjNodes = new ArrayList();
			prjNodes.AddRange(GetProjectNodeCollection());
			DeleteProjects(prjNodes);
		}

		/// <summary>
		/// Gestisce le reference, se si aggiunge o si elimina un progetto dalla solution, va a controllare tutte le references ad esso
		/// </summary>
		/// <param name="add">se true il progetto è stato aggiunto, se false è stato eliminato</param>
		/// <param name="prjPath">path del file di progetto</param>
		//---------------------------------------------------------------------
		private void ManageReferences(bool add, string prjPath)
		{
			foreach (ProjectDocument p in tblPrjFiles.Values)
			{
				if (!p.IsCsProject())
					continue;

				if (!add)
				{
					foreach (LocalizerTreeNode n in selectedNodeList)
					{
						string prj = n.Name;
						p.DeleteReference(prj);
					}
				}
				else
					p.AddReference(prjPath);
			}
			RefreshReferencesFolder();
		}

		/// <summary>
		/// Per ogni progetto nel treeView va ad aggiornare la cartella delle reference.
		/// </summary>
		//---------------------------------------------------------------------
		private void RefreshReferencesFolder()
		{
			foreach (LocalizerTreeNode n in GetProjectNodeCollection())
				AddReferenceFolder(n);
		}

		/// <summary>
		/// Close del form.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiClose_Click(object sender, System.EventArgs e)
		{
			try
			{
				this.Close();
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, exc.Message, "DictionaryCreator - MiClose_Click");
			}
		}

		/// <summary>
		/// Chiede salvataggio se la solution o i file dei progetti relativi sono stati modificati.
		/// </summary>
		//---------------------------------------------------------------------
		private bool AskSaving()
		{
			bool solutionSavingOk = true;
			SaveAllTblPrj(true);
			if (tblslnWriter.modified)
			{
				if (BatchBuild)
				{
					if (batchLogger != null)
						batchLogger.WriteLog(Strings.SolutionModified, TypeOfMessage.warning);
					return true;
				}
				DialogResult result = MessageBox.Show
					(
					this,
					String.Format(Strings.SaveSolutionQuestion, Solution),
					Strings.MainFormCaption,
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1
					);
				//cancel: niente
				//no: chiudo la solution corrente senza salvare
				//si: salvo e chiudo la solution corrente
				if (result == DialogResult.Cancel) solutionSavingOk = false;
				if (result == DialogResult.Yes) solutionSavingOk = SaveSolution(true, IsExistingSolution);
				if (result == DialogResult.No) solutionSavingOk = true;
			}

			return solutionSavingOk;
		}

		/// <summary>
		/// Salva la lista di tblprj, finora rimasta in memoria.
		/// </summary>
		//---------------------------------------------------------------------
		private bool SaveAllTblPrj(bool ask)
		{
			StringBuilder errorMessages = new StringBuilder();
			StringBuilder prjModifiedList = new StringBuilder();
			ArrayList list = new ArrayList();
			bool atLeastOne = false;
			foreach (ProjectDocument p in tblPrjFiles.Values)
			{
				if (!p.modified) continue;

				prjModifiedList.Append(p.FileName);
				prjModifiedList.Append(Environment.NewLine);
				atLeastOne = true;
			}
			// Non ci sono progetti modificati
			if (!atLeastOne) return true;

			if (ask)
			{
				if (BatchBuild)
				{
					//Se ci sono progetti modificati e sono in batch consiglio di controllare sul log, non salvo.
					if (batchLogger != null)
					{
						string message = String.Format(Strings.PrjNotLinedUp, prjModifiedList.ToString());
						batchLogger.WriteLog(message, TypeOfMessage.warning);
					}
					return true;
				}
				else
				{
					//Se ci sono progetti modificati e non sono in batch chiedo il salvataggio
					DialogResult result = MessageBox.Show
						(
						this,
						String.Format(Strings.SavingPrj, prjModifiedList.ToString()),
						Strings.WarningCaption,
						MessageBoxButtons.YesNoCancel
						);
					if (result == DialogResult.No) return true;
					if (result == DialogResult.Cancel) return false;
				}
			}

			foreach (ProjectDocument p in tblPrjFiles.Values)
				p.Save(errorMessages);

			string msg = errorMessages.ToString().Trim();
			if (msg != null && msg != String.Empty && !BatchBuild)
				MessageBox.Show(this, errorMessages.ToString(), Strings.WarningCaption);
			return true;
		}



		//---------------------------------------------------------------------
		private void DgStart_Enter(object sender, System.EventArgs e)
		{
			LnkNew.Select();
		}

		//--------------------------------------------------------------------------------
		private void DgStart_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Cursor = Cursors.Hand;
			dgStartHit = DgStart.HitTest(new Point(e.X, e.Y));
			//tooltip
			if (dgStartHit.Row == -1) return;
			string path = (string)DgStart[dgStartHit.Row, 1];
			ToolTipPrj.SetToolTip(DgStart, path);
			ToolTipPrj.Active = true;

		}

		/// <summary>
		/// Cambio cursore(Default).
		/// </summary>
		//---------------------------------------------------------------------
		private void DgStart_MouseLeave(object sender, System.EventArgs e)
		{
			Cursor = Cursors.Default;
		}

		/// <summary>
		/// Rinomina soluzione.
		/// </summary>
		//---------------------------------------------------------------------
		private void RenameSolution(string label, TreeNode solNode)
		{
			//cambiare nome nel tree, nome del file e nome nell'tbl
			string newName = label + AllStrings.slnExtension;
			string dest = Path.Combine(Path.GetDirectoryName(Solution), newName);
			try
			{
				if (File.Exists(Solution))
				{
					File.Move(Solution, dest);
					tblslnWriter.modified = true;
					iniWriter.INIModifier(Solution, dest);
					OpenExistingSolution(dest, true);
				}
				else //rinomino solo nel tree, perche non ho ancora salvato il file della solution
				{
					Solution = dest;
					WriteFirstNode(false, Solution);
					tblslnWriter.modified = true;
				}
			}
			catch (Exception exc)
			{
				//messaggio di soluzione non rinominata
				MessageBox.Show(this, exc.Message, "DictionaryCreator - MiRenameSolution_Click");
			}
		}

		/// <summary>
		/// Mostra il traduttore per il nodo selezionato, da context menu.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiTranslate_Click(object sender, System.EventArgs e)
		{
			ShowTranslator();
		}

		//---------------------------------------------------------------------
		private void MiRecover_Click(object sender, EventArgs e)
		{
			TranslationsRecoverer f = new TranslationsRecoverer((DictionaryTreeNode)SelectedNode);
			f.ShowDialog(this);
		}

		//---------------------------------------------------------------------
		private void MIPurge_Click(object sender, EventArgs e)
		{
			Logger logWriter = new Logger(MyProgressBar, MyStatusBar, TxtOutput);
			LocalizerTreeNode selectedNode = SelectedNode;

			if (selectedNode == null)
			{
				Logger.WriteLog(logWriter, DictionaryUpdaterStrings.NoNodeInvolved, TypeOfMessage.error);
				return;
			}

			if (DialogResult.Yes != MessageBox.Show(
				this,
				string.Format(Strings.ConfirmPurge1, selectedNode.Name),
				Strings.MainFormCaption,
				MessageBoxButtons.YesNo
				)
				||
				DialogResult.No != MessageBox.Show(
				this,
				Strings.ConfirmPurge2,
				Strings.MainFormCaption,
				MessageBoxButtons.YesNo
				))
				return;


			Cursor = Cursors.WaitCursor;
			try
			{
				EnableControl(false);
				NodeType nt = selectedNode.Type;
				string nodeName = selectedNode.Name;


				foreach (DictionaryTreeNode n in selectedNode.GetTypedChildNodes(NodeType.LASTCHILD, true, null, true, DictionaryTreeNode.BaseLanguage))
				{
					if (!Working)
						return;

					string file = n.FileSystemPath;
					logWriter.WriteLog(string.Format(Strings.Purging, file), TypeOfMessage.info);
					if (CommonFunctions.IsResx(file))
					{
						string oldRex = CommonFunctions.GetOldResxDictionaryFile(file);
						if (File.Exists(oldRex))
						{
							logWriter.WriteLog(string.Format(Strings.Purging, oldRex), TypeOfMessage.info);
							CommonFunctions.TryToRemoveFromSourceControl(oldRex);
							File.Delete(oldRex);
						}
						LocalizerDocument doc = LocalizerDocument.GetStandardXmlDocument(file, false);
						if (doc.SelectNodes("//" + AllStrings.stringTag).Count == 0)
						{
							CommonFunctions.TryToRemoveFromSourceControl(file);
							File.Delete(file);
						}
					}
					else
					{
						LocalizerDocument doc = new LocalizerDocument();
						doc.Load(file);

						bool modified = false;
						XmlNodeList list = doc.SelectNodes("//" + AllStrings.stringTag);
						for (int i = list.Count; i >= 0; i--)
						{
							XmlElement el = list[i] as XmlElement;
							if (el != null && el.GetAttribute(AllStrings.valid) == AllStrings.falseTag)
							{
								Functions.RemoveNodeAndEmptyAncestors(el);
								modified = true;
							}
						}
						if (modified)
						{
							if (doc.SelectNodes("//" + AllStrings.stringTag).Count > 0)
							{
								CommonFunctions.TryToCheckOut(file);
								doc.Save(file);
							}
							else
							{
								CommonFunctions.TryToRemoveFromSourceControl(file);
								File.Delete(file);
							}
						}
					}
				}

			}
			catch (Exception ex)
			{
				logWriter.WriteLog(ex.Message, TypeOfMessage.error);
			}
			finally
			{
				Cursor = Cursors.Default;
				RefreshTree();
				EnableControl(true);
				MessageBox.Show(this, Strings.OperationCompleted, "TBLocalizer");
			}
		}
		//comportamenti diversi, click e double click, perchè?
		//---------------------------------------------------------------------
		private void ProjectsTreeView_DoubleClick(object sender, System.EventArgs e)
		{
			DictionaryTreeNode nodeToShow = SelectedNode as DictionaryTreeNode;
			if (nodeToShow == null || nodeToShow.Type != NodeType.LASTCHILD)
				return;

			ShowTranslator();
		}

		//---------------------------------------------------------------------
		public string[] CurrentXmlExtensions()
		{
			if (tblslnWriter == null) return new string[0];
			return tblslnWriter.ReadXmlExtension();
		}

		//funzione utilizzata dall'addin di Visual Studio
		//---------------------------------------------------------------------
		public bool UpdateDictionaryFromFile(string projectPath, string file, out LocalizerTreeNode projectNode)
		{
			projectNode = null;
			working = true;

			if (Solution == string.Empty)
			{
				MessageBox.Show(this, Strings.InvalidSolution);
				return false;
			}

			string testFile = file.ToLower();

			if (                                                //già tolower()
				!testFile.EndsWith(AllStrings.cppExtension) &&
				!testFile.EndsWith(AllStrings.hExtension) &&
				!testFile.EndsWith(AllStrings.cExtension) &&
				!testFile.EndsWith(AllStrings.rcExtension) &&
				!testFile.EndsWith(AllStrings.resxExtension)
				)
			{
				MessageBox.Show(this, Strings.FileTypeNotSupported);
				return false;
			}

			bool result = false;
			Logger logWriter = new Logger(MyProgressBar, MyStatusBar, TxtOutput);

			LocalizerDocument.RemoveAllDictionariesFromCache();

			// la ricerca dovrebbe trovare un solo nodo
			ArrayList projectNodes = SolutionNode.GetTypedChildNodes
				(
				NodeType.PROJECT,
				false,
				Path.GetFileName(projectPath),
				true
				);

			if (projectNodes.Count == 1)
				projectNode = projectNodes[0] as LocalizerTreeNode;

			if (projectNode != null)
			{
				ArrayList dictionaryNodes = projectNode.GetTypedChildNodes
					(
					NodeType.LANGUAGE,
					false,
					LocalizerTreeNode.BaseLanguage,
					true
					);

				ProjectDocument tblProject = GetPrjWriter(projectNode);
				if (tblProject == null)
					return false;

				foreach (DictionaryTreeNode dictionaryNode in dictionaryNodes)
				{
					if (
						!tblslnWriter.UpdateDictionayFromFile
						(
						CommonUtilities.Functions.CalculateRelativePath(file, projectPath, true),
						dictionaryNode.Tag.ToString(),
						projectPath,
						tblProject,
						logWriter
						)
						)
						return false;

					result = true;
				}
			}


			RefreshTree();
			return result;
		}

		//funzione utilizzata dall'addin di Visual Studio
		//---------------------------------------------------------------------
		public void ShowTranslatorOnFile(string projectPath, string name, string type, LocalizerTreeNode projectNode)
		{
			if (Solution == string.Empty)
			{
				MessageBox.Show(this, Strings.InvalidSolution);
				return;
			}

			if (projectNode == null)
			{
				// la ricerca dovrebbe trovare un solo nodo
				ArrayList projectNodes = SolutionNode.GetTypedChildNodes
					(
					NodeType.PROJECT,
					false,
					Path.GetFileName(projectPath),
					true
					);

				if (projectNodes.Count == 1)
					projectNode = projectNodes[0] as LocalizerTreeNode;
			}

			Translator lastTranslator = null;
			if (projectNode != null)
			{
				ArrayList resourceNodes = projectNode.GetTypedChildNodes
					(
					NodeType.RESOURCE,
					false,
					type,
					true
					);

				foreach (LocalizerTreeNode resourceNode in resourceNodes)
				{
					if (resourceNode.IsBaseLanguageNode)
						continue;

					ArrayList childNodes = resourceNode.GetTypedChildNodes
						(
						NodeType.LASTCHILD,
						false,
						name,
						true
						);

					foreach (LocalizerTreeNode childNode in childNodes)
					{

						SelectedNode = childNode;
						Translator t = ShowTranslator();
						if (lastTranslator != null)
							t.Location = lastTranslator.Location + new Size(20, 20);
						lastTranslator = t;
					}
				}
			}

			if (lastTranslator == null)
				MessageBox.Show(this, Strings.ItemNotFound);
		}

		//---------------------------------------------------------------------
		public ArrayList LanguageChoose(ArrayList dictionaries)
		{
			ArrayList list = new ArrayList();
			foreach (string s in dictionaries)
				list.Add(s.ToLower());
			ChooseMultipleLanguage cml = new ChooseMultipleLanguage(list);
			DialogResult res = cml.ShowDialog();
			if (res == DialogResult.Cancel) return null;
			return cml.CheckedItems;
		}

		//---------------------------------------------------------------------
		public Translator LoadTranslator(DictionaryTreeNode nodeToLoad, bool giveFeedback)
		{
			Translator t = null;

			bool created = TranslatorCache.GetTranslator(tblslnWriter, ToolsList, nodeToLoad, out t);

			if (created)
			{
				t.Closing += new CancelEventHandler(Translator_Closing);
				t.Owner = this;

				if (!t.LoadData(nodeToLoad.ResourceType, SupportLanguage, SupportView))
				{
					// tolgo il translator dalla cache perché non lo visualizzo
					TranslatorCache.RefreshCache(nodeToLoad, null);
					if (giveFeedback)
					{
						MessageBox.Show(
							this,
							Strings.BaseStringsNotFound,
							Strings.WarningCaption,
							MessageBoxButtons.OK
							);
					}
					return null;
				}
			}
			return t;
		}

		//---------------------------------------------------------------------
		public Translator ShowTranslatorOnNode(LocalizerTreeNode node)
		{
			SelectedNode = node;
			return ShowTranslator();
		}

		/// <summary>
		/// Mostro il translator settando tutte le properties.
		/// </summary>
		/// <param name="firsTime">specifica se sto aprendo il traduttore per la prima volta, altrimenti è un refresh</param>
		//---------------------------------------------------------------------
		public Translator ShowTranslator()
		{
			return ShowTranslator(null);
		}

		//---------------------------------------------------------------------
		public Translator ShowTranslator(FindAndReplaceInfos infos)
		{
			Translator translator = null;
			DictionaryTreeNode nodeToShow = SelectedNode as DictionaryTreeNode;

			try
			{
				if (nodeToShow == null)
				{
					MessageBox.Show(this, Strings.NoProjectSelected, Strings.WarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return null;
				}

				if (nodeToShow.IsBaseLanguageNode)
				{
					MessageBox.Show(this, Strings.CantEditBaseDictionary, Strings.WarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return null;

				}

				if (nodeToShow.Type != NodeType.LASTCHILD)
					return null;

				FreezeTree();

				enabilitation = CheckButtonToDisable(false);

				translator = LoadTranslator(nodeToShow, true);
				if (translator == null)
					return null;

				translator.ToEnable = enabilitation;
				translator.Show();
				translator.Activate();

				if (infos != null)
					translator.SetSelectedRow(infos);
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, exc.Message);
				if (translator != null)
					translator.Close();
				// tolgo il translator dalla cache perché non lo visualizzo
				TranslatorCache.RefreshCache(nodeToShow, null);
				return null;

			}
			finally
			{
				DefreezeTree();
				if (nodeToShow != null && nodeToShow.Parent != null)
				{
					nodeToShow.EnsureVisible();
					SelectedNode = nodeToShow;
				}
			}

			return translator;
		}

		//---------------------------------------------------------------------
		private void Translator_Closing(object sender, CancelEventArgs e)
		{
			Translator t = sender as Translator;
			if (t == null)
				return;
			SupportView = t.SupportView;
		}

		/// <summary>
		/// Refresh del translator passandogli il nuovo nodo da tradurre.
		/// </summary>
		/// <param name="translator">translator che ha richiesto il refresh</param>
		//---------------------------------------------------------------------
		internal void RefreshTranslator(Translator translator)
		{
			if (translator == null || translator.GoTo == Direction.NULL)
				return;

			DictionaryTreeNode followingNode = (DictionaryTreeNode)GetFollowingNode(translator.TreeNodeToTranslate, translator.GoTo, translator.AdvancedNext);
			if (followingNode == null || followingNode.Type != NodeType.LASTCHILD)
			{
				//provoca il close immediato senza saving
				translator.Close(true);
				return;
			}
			//Ci sono casi che non riesce a settare la selezione corretta, 
			//magari perchè il tree è collassato
			//allora uso tmp  per sicurezza
			SelectedNode = followingNode;
			enabilitation = CheckButtonToDisable(followingNode, translator.AdvancedNext);

			// se esiste già un altro translator sul file associato al nodo, impedisco 
			// lo spostamento ed attivo il secondo translator
			if (TranslatorCache.RefreshCache(translator.TreeNodeToTranslate, followingNode))
			{
				translator.TreeNodeToTranslate = followingNode;
				translator.ToEnable = enabilitation;
			}
			else
			{
				MessageBox.Show(this, Strings.TranslatorAlreadyOpen);
			}

			if (!translator.LoadData(followingNode.ResourceType))
				return;

			translator.RefreshControls();

			// qui attivo il translator voluto dall'utente
			Translator t = TranslatorCache.GetExistingTranslator(followingNode);
			if (t != null)
			{
				t.Location = translator.Location;
				t.Focus();
			}

		}

		//---------------------------------------------------------------------
		private bool TranslateFromKnowledge(DictionaryTreeNode treeNode, XmlElement stringNode, string culture, params object[] args)
		{

			string targetString = stringNode.GetAttribute(AllStrings.target);
			if (targetString == null || targetString.Length == 0)
			{
				string baseString = stringNode.GetAttribute(AllStrings.baseTag);

				HintItem[] suggestions = KnowledgeManager.GetSuggestionsWithWaitingWindow(this, baseString, culture, true);
				if (suggestions.Length == 0)
					return false;
				List<HintItem> candidates = new List<HintItem>();
				foreach (HintItem suggestion in suggestions)
				{
					if (suggestion.Rating == 1f)
					{
						candidates.Add(suggestion);

					}
				}
				if (candidates.Count > 0)
				{
					if (candidates.Count > 1)
						candidates.Sort(new HintComparer(treeNode.FullPath));
					HintItem suggestion = candidates[0];
					stringNode.SetAttribute(AllStrings.target, suggestion.HintString);
					if (args.Length == 1 && (bool)args[0])
						stringNode.SetAttribute(AllStrings.temporary, AllStrings.trueTag);

					return true;
				}
			}

			return false;
		}

		//---------------------------------------------------------------------
		private bool TranslateJson(DictionaryTreeNode treeNode, XmlElement stringNode, string culture, params object[] args)
		{
			if (treeNode.ResourceType != AllStrings.jsonforms)
				return false;
			bool overWriteExisting = (args.Length == 1 && (bool)args[0]);
			string targetString = stringNode.GetAttribute(AllStrings.target);
			if (!overWriteExisting && !string.IsNullOrEmpty(targetString))
				return false;
			string baseString = stringNode.GetAttribute(AllStrings.baseTag);

			//il parent è il nodo del file
			LocalizerTreeNode fileNode = (LocalizerTreeNode)treeNode.Parent;

			//recupero il nodo di culture comune
			LocalizerTreeNode languageNode = treeNode.GetTypedParentNode(NodeType.LANGUAGE);

			//ridiscendo sulla risorsa dialog
			ArrayList correspondingDialogNodes = languageNode.GetTypedChildNodes(NodeType.RESOURCE, false, AllStrings.dialog, false);
			if (correspondingDialogNodes.Count != 1)
				return false;

			//ridiscendo sulla risorsa avente lo stesso nome
			ArrayList correspondingResourceNodes = ((LocalizerTreeNode)correspondingDialogNodes[0]).GetTypedChildNodes(NodeType.RESOURCE, true, fileNode.Name, true);
			if (correspondingResourceNodes.Count != 1)
				return false;

			//ridiscendo al nodo che contiene le traduzioni
			ArrayList correspondingNodes = ((LocalizerTreeNode)correspondingResourceNodes[0]).GetTypedChildNodes(NodeType.LASTCHILD, true, treeNode.Name, false);
			if (correspondingNodes.Count != 1)
				return false;

			DictionaryTreeNode correspondingDialogNode = (DictionaryTreeNode)correspondingNodes[0];
			foreach (XmlElement el in correspondingDialogNode.GetStringNodes())
			{
				if (baseString.Equals(el.GetAttribute(AllStrings.baseTag)))
				{
					string suggestion = el.GetAttribute(AllStrings.target);
					if (string.IsNullOrEmpty(suggestion))
						return false;
					stringNode.SetAttribute(AllStrings.target, suggestion);
					if (stringNode.HasAttribute(AllStrings.temporary))
						stringNode.RemoveAttribute(AllStrings.temporary);
					return true;
				}
			}
			//se non l'ho trovato lì, cerco nelle stringtable (la caption delle dialog è lì)
			ArrayList correspondingStringTableNodes = languageNode.GetTypedChildNodes(NodeType.RESOURCE, false, AllStrings.stringtable, false);
			if (correspondingStringTableNodes.Count != 1)
				return false;

			//ridiscendo al nodo che contiene le traduzioni
			correspondingNodes = ((LocalizerTreeNode)correspondingStringTableNodes[0]).GetTypedChildNodes(NodeType.LASTCHILD, true, fileNode.Name, true);
			if (correspondingNodes.Count != 1)
				return false;

			DictionaryTreeNode correspondingStringTableNode = (DictionaryTreeNode)correspondingNodes[0];
			if (correspondingStringTableNode != null)
			{
				foreach (XmlElement el in correspondingStringTableNode.GetStringNodes())
				{
					if (baseString.Equals(el.GetAttribute(AllStrings.baseTag)))
					{
						string suggestion = el.GetAttribute(AllStrings.target);
						if (string.IsNullOrEmpty(suggestion))
							return false;
						stringNode.SetAttribute(AllStrings.target, suggestion);
						if (stringNode.HasAttribute(AllStrings.temporary))
							stringNode.RemoveAttribute(AllStrings.temporary);

						return true;
					}
				}
			}

			return false;
		}

		//---------------------------------------------------------------------
		private bool CopyBaseToTarget(DictionaryTreeNode treeNode, XmlElement stringNode, string culture, params object[] args)
		{
			string targetString = stringNode.GetAttribute(AllStrings.target);
			if (targetString == null || targetString.Length == 0)
			{
				string baseString = stringNode.GetAttribute(AllStrings.baseTag);
				stringNode.SetAttribute(AllStrings.target, baseString);
				stringNode.SetAttribute(AllStrings.temporary, AllStrings.trueTag);
				return true;
			}

			return false;
		}

		//---------------------------------------------------------------------
		private bool CopyLanguageToTarget(DictionaryTreeNode treeNode, XmlElement element, string culture, params object[] parameters)
		{
			if (element == null || element.IsReadOnly)
				return false;
			if (parameters == null || parameters.Length != 2)
				return false;

			string languageToCopy = parameters[0] as string;
			Logger logWriter = parameters[1] as Logger;

			string targetString = element.GetAttribute(AllStrings.target);
			string baseString = element.GetAttribute(AllStrings.baseTag);

			if (targetString == null || targetString == String.Empty)
			{
				bool temporaryToCopy;
				string toInsert = GetCorrespondingValue(baseString, languageToCopy, treeNode, logWriter, out temporaryToCopy);
				if (toInsert == null || toInsert.Length <= 0)
					return false;
				element.SetAttribute(AllStrings.target, toInsert);
				if (temporaryToCopy)
					element.SetAttribute(AllStrings.temporary, bool.TrueString.ToLower());
				return true;
			}
			return false;

		}
		//---------------------------------------------------------------------
		private string GetCorrespondingValue(string baseToCompare, string languageToCopy, DictionaryTreeNode treeNode, Logger logWriter, out bool temporaryToCopy)
		{
			temporaryToCopy = false;
			if (baseToCompare == null || baseToCompare.Length <= 0 ||
				languageToCopy == null || languageToCopy.Length <= 0 ||
				treeNode == null
				)
				return null;

			LocalizerTreeNode correspondingNode;

			string correspondingPath = treeNode.GetCorrespondingNodePath(languageToCopy);
			if (correspondingPath == null || !GetNodeFromPath(correspondingPath, out correspondingNode))
				return null;

			XmlNodeList list = ((DictionaryTreeNode)correspondingNode).GetStringNodes();
			foreach (XmlElement el in list)
			{
				string targetString = el.GetAttribute(AllStrings.target);
				string baseString = el.GetAttribute(AllStrings.baseTag);
				if (
					String.Compare(baseString, baseToCompare, true) == 0 &&
					targetString != null &&
					targetString.Length > 0
					)
				{
					string temp = el.GetAttribute(AllStrings.temporary);
					temporaryToCopy = String.Compare(temp, bool.TrueString, true) == 0;
					return targetString;
				}
			}
			return null;
		}

		private delegate bool PerformActionOnString(DictionaryTreeNode treeNode, XmlElement stringNode, string culture, params object[] args);
		//---------------------------------------------------------------------
		private void PerformActionOnStrings(string language, LocalizerTreeNode selectedNode, Logger logWriter, PerformActionOnString action, params object[] args)
		{
			if (selectedNode == null)
			{
				Logger.WriteLog(logWriter, DictionaryUpdaterStrings.NoNodeInvolved, TypeOfMessage.error);
				return;
			}

			Cursor = Cursors.WaitCursor;
			ArrayList cultureNodes = null;
			try
			{
				EnableControl(false);
				NodeType nt = selectedNode.Type;
				string nodeName = selectedNode.Name;

				//se è un nodo che può riferirsi a più lingue si chiede la lingua di riferimento
				if (nt == NodeType.SOLUTION || nt == NodeType.PROJECT)
				{
					if (language == null || language == String.Empty)
					{
						ArrayList languageNodes = selectedNode.GetTypedChildNodes(NodeType.LANGUAGE, true);
						ChooseLanguage languageDialog = new ChooseLanguage();
						languageDialog.FormTitle = Strings.ChooseLanguage;
						languageDialog.FillCombo((LocalizerTreeNode[])languageNodes.ToArray(typeof(LocalizerTreeNode)));
						if (languageDialog.ShowDialog(this) != DialogResult.OK) return;
						language = languageDialog.ChoosedLanguage.Name;
					}

					cultureNodes = selectedNode.GetTypedChildNodes(NodeType.LANGUAGE, false, language, true);

				}
				else
				{
					language = ((DictionaryTreeNode)selectedNode).Culture;
					cultureNodes = new ArrayList();
					cultureNodes.Add(selectedNode);
				}

				string latestFile = null;
				LocalizerDocument latestDocument = null;
				foreach (DictionaryTreeNode cultureNode in cultureNodes)
				{
					foreach (DictionaryTreeNode n in cultureNode.GetTypedChildNodes(NodeType.LASTCHILD, true))
					{
						if (n.IsBaseLanguageNode)
							continue;

						if (!Working)
							return;

						XmlNodeList list = n.GetStringNodes();
						bool modified = false;
						foreach (XmlElement el in list)
							modified = action(n, el, language, args) || modified;

						if (modified)
						{
							if (DictionaryCreator.MainContext.ShowTranslationProgress)
								n.RefreshNodeAndAncestors(true);

							if (latestFile != n.FileSystemPath)
							{
								if (latestFile != null && latestDocument != null)
									LocalizerDocument.SaveStandardXmlDocument(latestFile, latestDocument);

								latestFile = n.FileSystemPath;
								latestDocument = n.Document;
							}
						}
					}

					if (latestFile != null && latestDocument != null)
					{
						logWriter.WriteLog(string.Format(Strings.UpdatedFile, latestFile));
						LocalizerDocument.SaveStandardXmlDocument(latestFile, latestDocument);
					}
				}
			}
			finally
			{
				Cursor = Cursors.Default;
				EnableControl(true);
				MessageBox.Show(this, Strings.OperationCompleted, "TBLocalizer");
			}
		}

		/// <summary>
		/// Restituisce il nodo più vicino, nella direzione specificata, 
		/// controllando eventualmente se è già tradotto.
		/// </summary>
		/// <param name="nodeToCheck">nodo di partenza</param>
		/// <param name="direction">direzione di spostamento</param>
		/// <param name="advancedNext">specifica se devo skippare traduzioni già effettuate</param>
		//---------------------------------------------------------------------
		private DictionaryTreeNode GetFollowingNode(DictionaryTreeNode nodeToCheck, Direction direction, bool checkCompleteTranslation)
		{
			do
				nodeToCheck = nodeToCheck.GetAdjacentLeafNode(direction);
			while (nodeToCheck != null && !IsValidItem(nodeToCheck, checkCompleteTranslation));

			return nodeToCheck;
		}

		/// <summary>
		/// Verifica che il dizionario in questione sia tradotto e validato completamente.
		/// </summary>
		/// <param name="nodeToCheck">nodo da verificare</param>
		//---------------------------------------------------------------------
		private bool IsValidItem(DictionaryTreeNode nodeToCheck, bool checkCompleteTranslation)
		{
			if (nodeToCheck == null || nodeToCheck.IsBaseLanguageNode) return false;

			bool result = true;
			XmlNode xmlNode = nodeToCheck.GetResourceNode();
			result = result && (xmlNode != null);
			result = result && (!checkCompleteTranslation || !IsNodeTranslated(xmlNode));
			return result;

		}

		/// <summary>
		/// Restituisce il prev o next node considerando che si potrebbe anche 
		/// salire o scendere di un parent se è primo o ultimo child,
		/// agisco quindi a tutto il livello di nodo language. Salta cartelle vuote.
		/// Non considera se tradotto o meno.
		/// </summary>
		/// <param name="nodeToCheck">nodo di partenza</param>
		/// <param name="direction">direzione di spostamento)</param>
		//---------------------------------------------------------------------

		/// <summary>
		/// Verifica quali e se sono i pulsanti da disabilitare nel translator tra next e prev
		/// </summary>
		/// <param name="advancedNext">specifica se skippare traduzioni già effettuate</param>
		//---------------------------------------------------------------------
		internal EnabledButtons CheckButtonToDisable(DictionaryTreeNode node, bool advancedNext)
		{
			if (node == null)
				node = SelectedNode as DictionaryTreeNode;

			return new EnabledButtons(
				(GetFollowingNode(node, Direction.NEXT, advancedNext) != null),
				(GetFollowingNode(node, Direction.PREVIOUS, advancedNext) != null)
				);
		}

		//---------------------------------------------------------------------
		internal EnabledButtons CheckButtonToDisable(bool advancedNext)
		{
			return CheckButtonToDisable(null, advancedNext);
		}

		/// <summary>
		/// Scelta directories per gli include.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiDir_Click(object sender, System.EventArgs e)
		{
			DirectoriesSpecifier directoriesDialog = new DirectoriesSpecifier();
			directoriesDialog.Dirs = tblslnWriter.ReadLogicalIncludesPath();
			directoriesDialog.Fill();
			if (directoriesDialog.ShowDialog(this) == DialogResult.OK)
				//Salva nel file indice in questo ordine
				tblslnWriter.WriteLogicalIncludePath(directoriesDialog.Dirs);
		}

		/// <summary>
		/// Scelta del dizionario support .
		/// </summary>
		//---------------------------------------------------------------------
		private void MiSupport_Click(object sender, System.EventArgs e)
		{
			ChooseLanguage languagesDialog = new ChooseLanguage();
			if (SupportLanguage != null)
				languagesDialog.ChoosedLanguage = new CultureInfo(SupportLanguage);
			languagesDialog.FillCombo((LocalizerTreeNode[])GetDictionaryNodes().ToArray(typeof(LocalizerTreeNode)));
			languagesDialog.FormTitle = Strings.SupportLanguageCaption;
			if (languagesDialog.ShowDialog(this) == DialogResult.OK)
			{
				SupportLanguage = languagesDialog.ChoosedLanguage.Name;
				SupportView = true;
			}
		}

		//---------------------------------------------------------------------
		private void MiResetSupport_Click(object sender, System.EventArgs e)
		{
			SupportLanguage = null;
			SupportView = false;
		}

		/// <summary>
		///Scelta delle estensioni xml da parsare.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiXml_Click(object sender, System.EventArgs e)
		{
			XmlSpecifier xmlDialog = new XmlSpecifier();
			xmlDialog.Extension = tblslnWriter.ReadXmlExtension();
			xmlDialog.ShowDialog(this);
			if (xmlDialog.DialogResult == DialogResult.OK)
			{
				tblslnWriter.WriteXmlExtensions(xmlDialog.Extension);
			}
		}

		/// <summary>
		/// Calcola la percentuale di stringhe tradotte sul nodo selezionato
		/// </summary>
		//---------------------------------------------------------------------
		private void MiProgressSpec_Click(object sender, System.EventArgs e)
		{
			ShowProgressPercentage();
		}

		//---------------------------------------------------------------------
		public void ShowProgressPercentage()
		{
			WordCounter wc = new WordCounter(this);
			wc.ShowProgressPercentage(SelectedNode);
		}

		/// <summary>
		/// Visualizza/nasconde i dettagli di progetto
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void MiProgress_Click(object sender, System.EventArgs e)
		{
			try
			{
				Cursor = Cursors.WaitCursor;
				ShowTranslationProgress = !ShowTranslationProgress;
				if (
						ShowTranslationProgress &&
						!globalWordCounter.SelectFilters(SolutionNode)
					)
					ShowTranslationProgress = false;

				UpdateDetailsAsync();
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		//---------------------------------------------------------------------
		private void StopSCCRefreshingThread()
		{
			if (SCCRefreshingThread != null && SCCRefreshingThread.IsAlive)
			{
				System.Diagnostics.Debug.WriteLine("Trying to stop updating thread...");
				LocalizerTreeNode.StopSCCRefresh = true;
				SCCRefreshingThread.Join();
				System.Diagnostics.Debug.WriteLine("Updating thread stopped");
				SCCRefreshingThread = null;
			}
			LocalizerTreeNode.StopSCCRefresh = false;

		}
		//--------------------------------------------------------------------------------
		public void RefreshSourceControlStatusAsync()
		{
			StopSCCRefreshingThread();
			SCCRefreshingThread = new Thread(new ThreadStart(delegate { SolutionNode.RefreshSourceControlStatus(true); }));
			SCCRefreshingThread.Name = "RefreshSourceControlStatus";
			SCCRefreshingThread.Start();

		}
		//---------------------------------------------------------------------
		public void UpdateDetailsAsync()
		{
			StopUpdatingThread();

			SolutionNode.CleanWordTable(true);
			UpdatingThread = new Thread(new ThreadStart(UpdateDetails));
			UpdatingThread.Priority = ThreadPriority.BelowNormal;
			UpdatingThread.Name = "UpdateTreeNodeDetails";
			UpdatingThread.SetApartmentState(ApartmentState.STA);
			UpdatingThread.Start();
		}

		//---------------------------------------------------------------------
		private void StopUpdatingThread()
		{
			if (UpdatingThread != null && UpdatingThread.IsAlive)
			{
				System.Diagnostics.Debug.WriteLine("Trying to stop updating thread...");
				LocalizerTreeNode.StopUpdating = true;
				UpdatingThread.Join();
				System.Diagnostics.Debug.WriteLine("Updating thread stopped");
				UpdatingThread = null;
			}
			LocalizerTreeNode.StopUpdating = false;

		}

		private delegate void EnableWaitingControlFunction(bool enable);
		//---------------------------------------------------------------------
		private void EnableWaitingControl(bool enable)
		{
			WaitingControl.Visible = enable;
		}

		//---------------------------------------------------------------------
		public void UpdateDetails()
		{
			System.Diagnostics.Debug.WriteLine("Started updating thread...");
			try
			{
				if (ShowTranslationProgress)
				{
					WaitingControl.Message = Strings.UpdateMessage;
					BeginInvoke(new EnableWaitingControlFunction(EnableWaitingControl), new object[] { true });
				}
				SolutionNode.UpdateDetails(ShowTranslationProgress, true);
			}
			finally
			{
				BeginInvoke(new EnableWaitingControlFunction(EnableWaitingControl), new object[] { false });
				System.Diagnostics.Debug.WriteLine("Finished updating thread...");
			}
		}

		//---------------------------------------------------------------------
		private void MiFont_Click(object sender, System.EventArgs e)
		{
			DemoDialog dlg = new DemoDialog();
			dlg.SetFont();
		}


		//---------------------------------------------------------------------
		private void MiUpdateReferences_Click(object sender, System.EventArgs e)
		{
			List<LocalizerTreeNode> projectNodes = new List<LocalizerTreeNode>();

			NodeType nt = SelectedNode.Type;
			switch (nt)
			{
				case NodeType.SOLUTION:
					{
						foreach (LocalizerTreeNode n in SelectedNode.GetTypedChildNodes(NodeType.PROJECT, false))
							projectNodes.Add(n);
						break;
					}
				case NodeType.PROJECT:
					{
						projectNodes.Add(SelectedNode);
						break;
					}
				default:
					{
						projectNodes.Add(SelectedNode.GetTypedParentNode(NodeType.PROJECT));
						break;
					}
			}

			foreach (LocalizerTreeNode projNode in projectNodes)
			{
				ProjectDocument tblProject = GetPrjWriter(projNode);
				if (tblProject == null || !tblProject.IsCsProject()) continue;

				ProjectDocument.ProjectType extensionType;
				ArrayList projectPaths = DataDocumentFunctions.GetProjectFiles(tblProject.SourceFolder, out extensionType);
				if (extensionType == ProjectDocument.ProjectType.CS)
				{
					tblProject.SetReferencesCouple(DataDocumentFunctions.ReadRealReferences((string)projectPaths[0], globalLogger), GetProjectNodeCollection());
					AddReferenceFolder(projNode);
				}
				else
				{
					MessageBox.Show(this, string.Format(Strings.SourcesNotFound, tblProject.SourceFolder));
				}
			}
		}

		//---------------------------------------------------------------------
		private void MiSetReference_Click(object sender, System.EventArgs e)
		{
			ArrayList listPrj = new ArrayList();

			ProjectDocument tblPrjWriterTmp;
			string[] projectList = tblslnWriter.ReadProjects();
			CommonFunctions.LogicalPathToPhysicalPath(projectList);
			foreach (string prj in projectList)
			{
				tblPrjWriterTmp = GetPrjWriter(prj);

				if (tblPrjWriterTmp.IsCsProject())
					listPrj.Add(CommonFunctions.GetProjectName(prj));
			}

			LocalizerTreeNode projectParent = SelectedNode.GetTypedParentNode(NodeType.PROJECT);
			string reference = SelectedNode.Name;
			tblPrjWriterTmp = GetPrjWriter(projectParent);
			string project = tblPrjWriterTmp.GetProjectReferenced(reference);
			SetReferences sr = new SetReferences
				(
				listPrj,
				reference,
				project,
				projectParent.Name
				);

			if (sr.ShowDialog(this) == DialogResult.OK)
			{
				tblPrjWriterTmp.SetReference(reference, sr.project);
				int imageindex = (sr.project == String.Empty || sr.project == null) ? (int)Images.REFERENCEEMPTY : (int)Images.REFERENCE;
				SelectedNode.SelectedImageIndex =
					SelectedNode.ImageIndex = imageindex;
			}
		}

		//---------------------------------------------------------------------
		public string SearchWorker()
		{
			LocalizerTreeNode selectedNode = SelectedNode;
			if (selectedNode == null)
				return Strings.NoSelectedNode;

			Cursor = Cursors.WaitCursor;
			FreezeTree();
			bool success = false;
			string languageCode = "";
			try
			{
				LocalizerTreeNode languageNode = selectedNode.GetTypedParentNode(NodeType.LANGUAGE);
				if (languageNode == null)
				{
					ArrayList languageNodes = selectedNode.GetTypedChildNodes(NodeType.LANGUAGE, false);
					if (languageNodes.Count == 0)
						return Strings.NoLanguages;

					if (languageNodes.Count == 1)
						languageCode = (languageNodes[0] as LocalizerTreeNode).Name;
					else
					{
						ChooseLanguage languagesDialog = new ChooseLanguage();
						languagesDialog.FillCombo((LocalizerTreeNode[])languageNodes.ToArray(typeof(LocalizerTreeNode)));
						languagesDialog.FormTitle = Strings.LanguageCaption;
						if (languagesDialog.ShowDialog(this) != DialogResult.OK) return string.Empty;
						//VISUALIZZA LA SCELTA DEI DIZIONARI.
						languageCode = languagesDialog.ChoosedLanguage.Name;
					}
				}
				else
					languageCode = languageNode.Name;

				Cursor = Cursors.WaitCursor;

				success = SearchWorker
					(
					selectedNode,
					languageCode,
					String.Empty,
					Translator.Columns.BASE
					);
			}
			finally
			{
				if (!success)
				{
					DefreezeTree();
					SelectedNode = selectedNode;
					selectedNode.Collapse();
				}
				Cursor = Cursors.Default;
			}

			return string.Empty;
		}

		//---------------------------------------------------------------------
		public bool SearchWorker
			(
			LocalizerTreeNode node,
			string languageCode,
			string toSearch,
			Translator.Columns column
			)
		{
			NodeType nodeType = node.Type;

			SolutionCacheObject obj = Functions.CurrentSolutionCache
				[
				"Finder",
				nodeType,
				CommonFunctions.IsSupportEnabled(SupportLanguage),
				toSearch
				];

			if (obj.Object == null)
				obj.Object = new Finder
					(
					nodeType,
					column,
					CommonFunctions.IsSupportEnabled(SupportLanguage),
					toSearch
					);

			Finder finder = obj.Object as Finder;
			if (finder.ShowDialog(this) != DialogResult.OK)
				return false;

			toSearch = finder.ToSearch;

			bool onlyTranslated = finder.OnlyTranslated;

			Finder.LanguageType searchingLanguage = finder.SearchingLanguage;

			string searchingTag;
			if (searchingLanguage == Finder.LanguageType.Target)
				searchingTag = AllStrings.target;
			else if (searchingLanguage == Finder.LanguageType.Support)
				searchingTag = AllStrings.support;
			else
				searchingTag = AllStrings.baseTag;

			TextSearcher ts = new TextSearcher(
				finder,
				node,
				searchingTag,
				onlyTranslated,
				toSearch,
				languageCode,
				SupportLanguage,
				finder.MatchWord,
				finder.MatchCase,
				finder.UseRegex,
				finder.ApplyFilter,
				finder.Filters,
				MyStatusBar);

			EnableControl(false);
			ts.SearchInTreeNode();
			StopSearch(ts);
			return true;
		}

		//--------------------------------------------------------------------------------
		private void StopSearch(TextSearcher ts)
		{
			try
			{
				//se cerco in base		visualizzo base anche se sono in supportview, 
				//se cerco in support	visualizzo support anche se non sono in supportview, 
				//se cerco in target	visualizzo in funzione di supportview
				bool supportsearch = false;
				if (ts.Finder.SearchingLanguage == Finder.LanguageType.Target)
					supportsearch = SupportView;
				else if (ts.Finder.SearchingLanguage == Finder.LanguageType.Support)
					supportsearch = true;

				if (ts.FinderInfos == null || ts.FinderInfos.Count == 0)
				{
					MessageBox.Show(this, String.Format(Strings.NoFindResult, ts.Finder.ToSearch), "Find result");
					return;
				}

				FindResult resView = new FindResult
					(
					ts.FinderInfos,
					ProjectsTreeView.PathSeparator,
					supportsearch,
					ts.Finder.SearchingLanguage == Finder.LanguageType.Target,
					ts.Finder.ToSearch,
					ts.Finder.ReplaceText,
					ts.Finder.MatchCase,
					ts.Finder.MatchWord,
					ts.Finder.UseRegex
					);
				resView.Owner = this;
				resView.ShowDialog(this); //must wait end of dialog to avoid sync problems
			}
			finally
			{
				Cursor = Cursors.Default;
				DefreezeTree();
				SelectedNode = ts.Node;
				EnableControl(true);
			}
		}

		//---------------------------------------------------------------------
		private bool IsNodeTranslated(XmlNode node)
		{
			try
			{
				if (node == null) return true;

				foreach (XmlNode n in node.ChildNodes)
				{
					if (n.NodeType != XmlNodeType.Element)
						continue;

					XmlAttribute target = n.Attributes[AllStrings.target];
					XmlAttribute temporaryAttribute = n.Attributes[AllStrings.temporary];

					bool temporary = false;
					if (temporaryAttribute != null)
						temporary = string.Compare(temporaryAttribute.Value, AllStrings.trueTag, true) == 0;

					if (target == null || target.Value == String.Empty || temporary)
						return false;
				}
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message, ex.StackTrace);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Apre il file log con IE.
		/// </summary>
		//---------------------------------------------------------------------
		private void TxtOutput_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
			try
			{
				string completepath = AllStrings.LOGPATH + e.LinkText.Substring(e.LinkText.LastIndexOf("\\"));
				if (
					MessageBox.Show
					(Strings.LogView, Strings.LogViewTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
					DialogResult.Yes
					)
					completepath = ElaburateLog(completepath);

				if (File.Exists(completepath))
					System.Diagnostics.Process.Start(completepath);

			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
				TxtOutput.AppendText(Environment.NewLine);
				TxtOutput.AppendText(String.Format(Strings.Error, exc.Message));
			}

		}



		//---------------------------------------------------------------------
		private string ElaburateLog(string path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			XmlDocument newDoc = new XmlDocument();

			XmlDeclaration dec = newDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
			newDoc.AppendChild(dec);

			XmlProcessingInstruction newPI = newDoc.CreateProcessingInstruction("xml-stylesheet", "type='text/xsl' href='LogManager.xsl'");
			newDoc.AppendChild(newPI);

			XmlElement el = newDoc.CreateElement("messages");
			XmlNodeList list = doc.SelectNodes("//messages/message[@type='error' or @type='warning']");
			foreach (XmlNode n in list)
			{
				el.AppendChild(newDoc.ImportNode(n, true));
			}
			newDoc.AppendChild(el);
			string newpath = Path.GetFileNameWithoutExtension(path);
			newpath += "_FILTERED";
			newpath = path.Replace(Path.GetFileNameWithoutExtension(path), newpath);
			string newpath2 = newpath.Replace("file:\\\\\\", "");
			newDoc.Save(newpath2);
			return newpath;
		}





		//---------------------------------------------------------------------
		private void MiFilterDictionaries_Click(object sender, System.EventArgs e)
		{
			try
			{
				FreezeTree();
				Cursor = Cursors.WaitCursor;

				ChooseFilterDictionary dictionariesDialog = new ChooseFilterDictionary(dictionaries);
				dictionariesDialog.ChoosedDictionaries = SolutionDocument.LocalInfo.HiddenDictionaries;

				if (dictionariesDialog.ShowDialog(this) == DialogResult.OK)
				{
					SolutionDocument.LocalInfo.HiddenDictionaries = dictionariesDialog.ChoosedDictionaries;

					// ricalcolo il tree nascondendo i dizionari selezionati
					RefreshTree();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				DefreezeTree();
				Cursor = Cursors.Default;
			}
		}

		/// <summary>
		/// Visualizza conteggio parole del nodo.
		/// </summary>
		//---------------------------------------------------------------------
		private void MiCount_Click(object sender, System.EventArgs e)
		{
			WordCounter wc = new WordCounter(this);
			wc.Count(SelectedNode);
		}

		//---------------------------------------------------------------------
		private void DictionaryCreator_Load(object sender, System.EventArgs e)
		{
			LocalState localState = new LocalState();
			if (!localState.LoadFromConfiguration())
				MessageBox.Show(this, Strings.ErrorReadingConfigFile, Strings.WarningCaption);
			else
			{
				this.ToolsList = localState.ToolsList;
				this.ExternalGlossaries = localState.GlossariesList;
			}


			this.ToolTipPrj = new System.Windows.Forms.ToolTip();
			this.ToolTipPrj.InitialDelay = 300; //half a second delay 
			this.ToolTipPrj.ReshowDelay = 0;
		}

		//---------------------------------------------------------------------
		private void DictionaryCreator_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//
			if (Working || !CloseSolution(true))
			{
				e.Cancel = true;
				return;
			}
			if (batchLogger != null)
			{
				batchLogger.SaveLog();
			}

			PerformClosingOperations();
		}

		//---------------------------------------------------------------------
		public void PerformClosingOperations()
		{
			//StopListening();

			LocalizerDocument.StopSavingContext();

			StopUpdatingThread();
			StopSCCRefreshingThread();

			// Salvo lo stato del menù per poterlo ripristinare al prossimo lancio
			LocalState localState = new LocalState(this);
			localState.SaveToConfiguration();
		}

		//---------------------------------------------------------------------
		private TreeNode SearchChild(Stack aStack, TreeNode aNode)
		{
			LocalizerTreeNode childNode = aNode as LocalizerTreeNode;
			foreach (LocalizerTreeNode child in aStack)
			{
				LocalizerTreeNode potentialNode = null;
				foreach (LocalizerTreeNode potential in childNode.Nodes)
				{
					if (CommonUtilities.Functions.SameTreeNodes(potential, child))
					{
						potentialNode = potential;
						break;
					}
				}

				if (potentialNode != null)
					childNode = potentialNode;
				else
					return null;
			}

			return childNode;
		}

		//---------------------------------------------------------------------
		private void MiRemove_Click(object sender, System.EventArgs e)
		{
			LocalizerTreeNode aNode = SelectedNode;
			if (aNode == null) return;

			switch (aNode.Type)
			{
				case NodeType.LANGUAGE:
					{
						DeleteDictionary((DictionaryTreeNode)aNode);
						return;
					}
				case NodeType.LASTCHILD:
					{
						if (aNode.IsBaseLanguageNode)
							DeleteBaseLanguageNode(aNode as DictionaryTreeNode);
						return;
					}
			}
		}

		//---------------------------------------------------------------------
		private XmlNode SearchResourceNodeByName(LocalizerDocument aDoc, string name)
		{
			return aDoc.SelectSingleNode(
				AllStrings.nodeFunction +
				"/" +
				AllStrings.nodeFunction +
				CommonFunctions.XPathWhereClause(AllStrings.name, name)
				);
		}

		//---------------------------------------------------------------------
		private bool DeleteBaseLanguageNode(DictionaryTreeNode aNode)
		{
			try
			{
				Cursor = Cursors.WaitCursor;

				if (aNode.Type != NodeType.LASTCHILD)
					return false;

				if (
					MessageBox.Show(
					this,
					string.Format(Strings.ConfirmDelete, aNode.FullPath),
					Strings.MainFormCaption,
					MessageBoxButtons.YesNo
					) != DialogResult.Yes
					)
					return false;

				if (aNode.PrevNode != null)
					SelectedNode = aNode.PrevNode as LocalizerTreeNode;
				else if (aNode.NextNode != null)
					SelectedNode = aNode.NextNode as LocalizerTreeNode;
				else
					SelectedNode = aNode.Parent as LocalizerTreeNode;

				ArrayList nodesToClean = new ArrayList();
				foreach (DictionaryTreeNode node in SolutionNode.GetTypedChildNodes(NodeType.LASTCHILD, true, null, true, DictionaryTreeNode.BaseLanguage))
				{
					if (node.References(aNode))
						nodesToClean.Add(node);
				}
				if (nodesToClean.Count > 0)
				{
					string nodes = string.Empty;
					foreach (DictionaryTreeNode node in nodesToClean)
						nodes += (node.FullPath + Environment.NewLine);

					if (
						MessageBox.Show(
						this,
						string.Format(Strings.ConfirmDeleteReferences, aNode.FullPath, nodes),
						Strings.MainFormCaption,
						MessageBoxButtons.YesNo
						) != DialogResult.Yes
						)
						return false;
				}

				string nodePath = aNode.FullPath;
				foreach (DictionaryTreeNode node in nodesToClean)
					LocalizerDocument.RemoveReferenceInBaseLanguageFile(node, nodePath);

				string file = aNode.FileSystemPath;
				if (CommonFunctions.IsXML(file))
				{
					XmlElement el = aNode.GetResourceNode();
					if (el == null)
						return false;

					LocalizerDocument doc = el.OwnerDocument as LocalizerDocument;
					Functions.RemoveNodeAndEmptyAncestors(el);

					if (!LocalizerDocument.SaveStandardXmlDocument(file, doc))
						return false;
				}
				else
				{
					Functions.SafeDeleteFile(file);
					string folder = Path.GetDirectoryName(file);
					if (Directory.GetFileSystemEntries(folder, "*.*").Length == 0)
						Directory.Delete(folder, false);

				}

				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message);
				return false;
			}
			finally
			{
				RefreshTree();

				Cursor = Cursors.Default;

			}
		}

		//---------------------------------------------------------------------
		private bool DeleteDictionary(DictionaryTreeNode aNode)
		{
			if (DialogResult.Yes != MessageBox.Show(
				this,
				string.Format(Strings.ConfirmDeleteDictionary1, aNode.Name),
				Strings.MainFormCaption,
				MessageBoxButtons.YesNo
				)
				||
				DialogResult.No != MessageBox.Show(
				this,
				Strings.ConfirmDeleteDictionary2,
				Strings.MainFormCaption,
				MessageBoxButtons.YesNo
				))
				return false;


			if (aNode.IsBaseLanguageNode)
			{
				if (DialogResult.Yes != MessageBox.Show(
					this,
					string.Format(Strings.ConfirmDeleteDictionary3, aNode.Name),
					Strings.MainFormCaption,
					MessageBoxButtons.YesNo
					))
					return false;

				ArrayList nodes = new ArrayList();
				nodes.AddRange(aNode.Parent.Nodes);
				foreach (LocalizerTreeNode n in nodes)
				{
					if (n.Type == NodeType.LANGUAGE)
					{
						CommonUtilities.Functions.SafeDeleteFolder(n.FileSystemPath);
						n.Remove();
					}
				}

			}
			else
			{
				CommonUtilities.Functions.SafeDeleteFolder(aNode.FileSystemPath);
				aNode.Remove();
			}

			return true;
		}

		//---------------------------------------------------------------------
		private void MiPlaceholder_Click(object sender, System.EventArgs e)
		{
			StringBuilder message = new StringBuilder();
			try
			{
				Cursor = Cursors.WaitCursor;
				LocalizerTreeNode selectedNode = SelectedNode;


				//messaggio di spiegazione delle tipologie di errore
				message.Append(Strings.PlaceHoldersExplication);
				message.Append(Environment.NewLine);
				message.Append(Environment.NewLine);
				ArrayList placeHolderInfos = new ArrayList();
				foreach (DictionaryTreeNode node in selectedNode.GetTypedChildNodes(NodeType.LASTCHILD, true))
				{
					XmlNodeList list = node.GetStringNodes();
					if (list == null)
						continue;

					//tipologia di placeHolders
					CommonFunctions.ParametersMode mode = GetParametersMode(node);
					if (mode == CommonFunctions.ParametersMode.NONE) continue;

					foreach (XmlElement el in list)
					{
						string baseString = el.GetAttribute(AllStrings.baseTag);
						if (baseString.IndexOf("{") == -1)
							continue;

						string targetString = el.GetAttribute(AllStrings.target);
						if (targetString == String.Empty) continue;
						PlaceHolderValidity phv = CommonFunctions.IsPlaceHolderValid(baseString, targetString, mode, true);
						if (!phv.TranslationValid || !phv.SequenceValid)
						{
							string resName = String.Empty;
							XmlElement parent = el.ParentNode as XmlElement;
							if (parent != null)
								resName = parent.GetAttribute(AllStrings.name);
							PlaceHolderInfo phi = new PlaceHolderInfo(node.FullPath, resName);

							if (!placeHolderInfos.Contains(phi))
							{
								phi.AddError(phv);
								placeHolderInfos.Add(phi);
							}
							else
							{
								PlaceHolderInfo test = (PlaceHolderInfo)placeHolderInfos[placeHolderInfos.IndexOf(phi)];
								test.AddError(phv);
							}
						}
					}
				}
				if (placeHolderInfos != null && placeHolderInfos.Count > 0)
				{
					foreach (PlaceHolderInfo phi in placeHolderInfos)
						message.Append(phi.ToString());
				}
				else
					message.Append(Strings.PlaceHoldersNoError);
			}
			finally
			{
				Cursor = Cursors.Default;
			}

			InfoViewer dialog = new InfoViewer(message.ToString());
			dialog.Show();
		}

		//---------------------------------------------------------------------
		internal CommonFunctions.ParametersMode GetParametersMode(DictionaryTreeNode node)
		{
			CommonFunctions.ParametersMode mode = CommonFunctions.ParametersMode.NONE;

			//modalità di controllo parametri:
			if (String.Compare(node.ResourceType, AllStrings.report, true) == 0)
				mode = CommonFunctions.ParametersMode.REPORT;
			else
			{
				LocalizerTreeNode projNode = node.GetTypedParentNode(NodeType.PROJECT);
				ProjectDocument aTblPrj = GetPrjWriter(projNode);
				if (aTblPrj == null)
					return CommonFunctions.ParametersMode.NONE;

				if (aTblPrj.IsVcProject())
					mode = CommonFunctions.ParametersMode.CPP;
				else if (aTblPrj.IsCsProject())
					mode = CommonFunctions.ParametersMode.CS;
			}
			return mode;
		}

		//---------------------------------------------------------------------
		private void MiAmpersand_Click(object sender, System.EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			StringBuilder messages = new StringBuilder();
			try
			{
				LocalizerTreeNode selectedNode = SelectedNode;

				ArrayList repeated = null;

				foreach (DictionaryTreeNode node in selectedNode.GetTypedChildNodes(NodeType.LASTCHILD, true))
				{

					XmlNodeList list = node.GetStringNodes();
					if (list == null)
						continue;

					ArrayList accelerator = new ArrayList();

					foreach (XmlElement n in list)
					{
						XmlElement el = n.ParentNode as XmlElement;
						string target = n.GetAttribute(AllStrings.target);
						int index = target.IndexOf("&");

						if (index == -1)
							continue;

						string name = el.GetAttribute(AllStrings.name);
						string post = null;
						try
						{
							if (index < target.Length && index > -1)
								post = ((char)target[index + 1]).ToString().ToLower();
						}
						catch { }

						if (post != null && post != String.Empty)
						{
							AmpersandInfo ai = new AmpersandInfo(node.FullPath, name, post);

							if (!accelerator.Contains(ai))
							{
								ai.AddRepetition(target);
								accelerator.Add(ai);
							}
							else
							{
								if (repeated == null)
									repeated = new ArrayList();
								if (repeated.Contains(ai))
								{
									AmpersandInfo test = (AmpersandInfo)repeated[repeated.IndexOf(ai)];
									test.Counter += 1;
									test.AddRepetition(target);
								}
								else
								{
									AmpersandInfo existing = (AmpersandInfo)accelerator[accelerator.IndexOf(ai)];
									if (existing != null)
									{
										string pre = existing.RepetitionList[0] as string;
										ai.AddRepetition(pre);
										ai.AddRepetition(target);
										repeated.Add(ai);
									}
								}
							}
						}
					}
				}
				if (repeated != null)
				{
					foreach (AmpersandInfo ai in repeated)
						messages.Append(ai.ToString());
				}
				else
					messages.Append(Strings.AmpersandNoRepetition);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				Cursor = Cursors.Default;
			}

			InfoViewer ampersandDialog = new InfoViewer(messages.ToString());
			ampersandDialog.Show();
		}

		//---------------------------------------------------------------------
		private void MiGlossary_Click(object sender, System.EventArgs e)
		{
			ApplyGlossary();
		}

		//---------------------------------------------------------------------
		private void MiFind_Click(object sender, System.EventArgs e)
		{
			SearchWorker();
		}


		//---------------------------------------------------------------------
		public string ApplyGlossary()
		{
			bool overwrite, noTemporary;

			if (IsSureToApplyAutomaticTranslation(out overwrite, out noTemporary))
				return ApplyGlossary(overwrite, noTemporary);

			return string.Empty;
		}

		//---------------------------------------------------------------------
		private string ApplyGlossary(bool overwrite, bool noTemporary)
		{
			if (SelectedNode == null)
				return string.Empty;

			Cursor = Cursors.WaitCursor;
			try
			{
				GlossaryFunctions.ApplyGlossary(SelectedNode, overwrite, noTemporary);

				MessageBox.Show(this, Strings.OperationCompleted, "TBLocalizer");
			}
			finally
			{
				Cursor = Cursors.Default;
			}
			return string.Empty;
		}

		//---------------------------------------------------------------------
		private bool IsSureToApplyAutomaticTranslation(out bool overwrite, out bool noTemporary)
		{
			overwrite = false;
			noTemporary = false;

			GlossaryApplicationAsk ask = new GlossaryApplicationAsk();
			DialogResult result = ask.ShowDialog(this);
			if (result != DialogResult.OK)
				return false;

			overwrite = ask.Overwrite;
			noTemporary = ask.NoTemporary;
			return true;
		}

		//---------------------------------------------------------------------
		private void MiGlossaries_Click(object sender, System.EventArgs e)
		{
			GlossariesManager glossaryManager = new GlossariesManager(ExternalGlossaries);
			DialogResult result = glossaryManager.ShowDialog(this);
			if (result == DialogResult.OK)
				ExternalGlossaries = glossaryManager.GlossaryList;
		}

		//---------------------------------------------------------------------
		private void MiCheckDialogs_Click(object sender, System.EventArgs e)
		{
			DialogChecker.Check(SelectedNode, this);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				/* if (woormServer != null)
				 {
					 woormServer.Dispose();
				 }*/

			}
			base.Dispose(disposing);
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DictionaryCreator));
			this.MyContextMenu = new System.Windows.Forms.ContextMenu();
			this.MiTranslate = new System.Windows.Forms.MenuItem();
			this.MiFind = new System.Windows.Forms.MenuItem();
			this.MiCount = new System.Windows.Forms.MenuItem();
			this.MiSetReference = new System.Windows.Forms.MenuItem();
			this.MiUpdateReferences = new System.Windows.Forms.MenuItem();
			this.MiRemove = new System.Windows.Forms.MenuItem();
			this.MiGlossary = new System.Windows.Forms.MenuItem();
			this.MiProgressSpec = new System.Windows.Forms.MenuItem();
			this.MiCheckDialogs = new System.Windows.Forms.MenuItem();
			this.MiAmpersand = new System.Windows.Forms.MenuItem();
			this.MiPlaceholder = new System.Windows.Forms.MenuItem();
			this.MiTranslation = new System.Windows.Forms.MenuItem();
			this.MiAllStrings = new System.Windows.Forms.MenuItem();
			this.MiNotTranslatedStrings = new System.Windows.Forms.MenuItem();
			this.MiTranslatedStrings = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.MiXmlAllStrings = new System.Windows.Forms.MenuItem();
			this.MiXmlNotTranslatedStrings = new System.Windows.Forms.MenuItem();
			this.MiXmlTranslatedStrings = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.MiCSVAllStrings = new System.Windows.Forms.MenuItem();
			this.MiCSVNotTranslatedStrings = new System.Windows.Forms.MenuItem();
			this.MiCSVTranslatedStrings = new System.Windows.Forms.MenuItem();
			this.MiAutoTranslate = new System.Windows.Forms.MenuItem();
			this.MiAutoTranslateBase = new System.Windows.Forms.MenuItem();
			this.MiTranslateFromKnowledge = new System.Windows.Forms.MenuItem();
			this.MiTranslateJson = new System.Windows.Forms.MenuItem();
			this.MiTranslateFromLanguage = new System.Windows.Forms.MenuItem();
			this.MiRecover = new System.Windows.Forms.MenuItem();
			this.MIPurge = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.MiCheckIn = new System.Windows.Forms.MenuItem();
			this.MiCheckOut = new System.Windows.Forms.MenuItem();
			this.MiGetLatest = new System.Windows.Forms.MenuItem();
			this.MiUndoCheckOut = new System.Windows.Forms.MenuItem();
			this.ProjectsTreeViewImageList = new System.Windows.Forms.ImageList(this.components);
			this.StateImageList = new System.Windows.Forms.ImageList(this.components);
			this.MyStatusBar = new System.Windows.Forms.StatusBar();
			this.MainContextMenu = new System.Windows.Forms.ContextMenu();
			this.MiFile = new System.Windows.Forms.MenuItem();
			this.MiNew = new System.Windows.Forms.MenuItem();
			this.MiOpen = new System.Windows.Forms.MenuItem();
			this.MiCloseSol = new System.Windows.Forms.MenuItem();
			this.MiFileSeparator1 = new System.Windows.Forms.MenuItem();
			this.MiDir = new System.Windows.Forms.MenuItem();
			this.MiXml = new System.Windows.Forms.MenuItem();
			this.MiFileSeparator2 = new System.Windows.Forms.MenuItem();
			this.MiSupport = new System.Windows.Forms.MenuItem();
			this.MiSupportChoose = new System.Windows.Forms.MenuItem();
			this.MiResetSupport = new System.Windows.Forms.MenuItem();
			this.MiGlossaries = new System.Windows.Forms.MenuItem();
			this.MiTools = new System.Windows.Forms.MenuItem();
			this.MiFilterDictionaries = new System.Windows.Forms.MenuItem();
			this.MiRefresh = new System.Windows.Forms.MenuItem();
			this.MiCollapse = new System.Windows.Forms.MenuItem();
			this.MiFileSeparator3 = new System.Windows.Forms.MenuItem();
			this.MiImportXml = new System.Windows.Forms.MenuItem();
			this.MiImportHTML = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.MiSave = new System.Windows.Forms.MenuItem();
			this.MiSaveAs = new System.Windows.Forms.MenuItem();
			this.MiFileSeparator4 = new System.Windows.Forms.MenuItem();
			this.MiClose = new System.Windows.Forms.MenuItem();
			this.MiProjects = new System.Windows.Forms.MenuItem();
			this.MiManageProjects = new System.Windows.Forms.MenuItem();
			this.MiRemoveProjects = new System.Windows.Forms.MenuItem();
			this.MiSelectAll = new System.Windows.Forms.MenuItem();
			this.MiProjectsSeparator1 = new System.Windows.Forms.MenuItem();
			this.MiDictionary = new System.Windows.Forms.MenuItem();
			this.MiCreate = new System.Windows.Forms.MenuItem();
			this.MiCustomCreate = new System.Windows.Forms.MenuItem();
			this.MiAddDictionary = new System.Windows.Forms.MenuItem();
			this.MiImportDictionary = new System.Windows.Forms.MenuItem();
			this.MiProjectsSeparator2 = new System.Windows.Forms.MenuItem();
			this.MiBuildSelectedProjects = new System.Windows.Forms.MenuItem();
			this.MiBuildSolution = new System.Windows.Forms.MenuItem();
			this.MiProjectsSeparator3 = new System.Windows.Forms.MenuItem();
			this.MiZipDictionary = new System.Windows.Forms.MenuItem();
			this.MiChooseLanguage = new System.Windows.Forms.MenuItem();
			this.MiAllLanguage = new System.Windows.Forms.MenuItem();
			this.MiUnzipDictionary = new System.Windows.Forms.MenuItem();
			this.MiOptions = new System.Windows.Forms.MenuItem();
			this.MiProgress = new System.Windows.Forms.MenuItem();
			this.MiGlossaryOptions = new System.Windows.Forms.MenuItem();
			this.MiViewGlossary = new System.Windows.Forms.MenuItem();
			this.MiSetGlossaryFolder = new System.Windows.Forms.MenuItem();
			this.MiRapidCreation = new System.Windows.Forms.MenuItem();
			this.MiTempAsTrans = new System.Windows.Forms.MenuItem();
			this.MiSepOpt1 = new System.Windows.Forms.MenuItem();
			this.MiEnvironmentSettings = new System.Windows.Forms.MenuItem();
			this.MiSepOpt2 = new System.Windows.Forms.MenuItem();
			this.MiFont = new System.Windows.Forms.MenuItem();
			this.MiLocalizerTools = new System.Windows.Forms.MenuItem();
			this.MiTbLoader = new System.Windows.Forms.MenuItem();
			this.MiDictionaryViewer = new System.Windows.Forms.MenuItem();
			this.MiSourceControl = new System.Windows.Forms.MenuItem();
			this.MiTeamSystem = new System.Windows.Forms.MenuItem();
			this.MiTFSSourceBinding = new System.Windows.Forms.MenuItem();
			this.MiEnableSourceControl = new System.Windows.Forms.MenuItem();
			this.MiQuestion = new System.Windows.Forms.MenuItem();
			this.MiHelp = new System.Windows.Forms.MenuItem();
			this.MiAbout = new System.Windows.Forms.MenuItem();
			this.PanelStartPage = new System.Windows.Forms.Panel();
			this.PanelButtons = new System.Windows.Forms.Panel();
			this.LnkNew = new System.Windows.Forms.LinkLabel();
			this.LnkOpen = new System.Windows.Forms.LinkLabel();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.DgStart = new System.Windows.Forms.DataGrid();
			this.MyTableStyle = new System.Windows.Forms.DataGridTableStyle();
			this.HeaderColumnStyle = new System.Windows.Forms.DataGridTextBoxColumn();
			this.HiddenColumnStyle = new System.Windows.Forms.DataGridTextBoxColumn();
			this.PbStart = new System.Windows.Forms.PictureBox();
			this.PanelTreeView = new System.Windows.Forms.Panel();
			this.ProjectsTreeView = new Microarea.Tools.TBLocalizer.CommonUtilities.LocalizerTreeView();
			this.WaitingControl = new Microarea.Tools.TBLocalizer.CommonUtilities.LocalizerWaitingControl();
			this.HorizontalSplitter = new System.Windows.Forms.Splitter();
			this.PanelOutput = new System.Windows.Forms.Panel();
			this.BtnStopProcedure = new System.Windows.Forms.Button();
			this.TxtOutput = new System.Windows.Forms.RichTextBox();
			this.ResultsContextMenu = new System.Windows.Forms.ContextMenu();
			this.MenuItemClear = new System.Windows.Forms.MenuItem();
			this.MenuItemCopy = new System.Windows.Forms.MenuItem();
			this.MenuItemSave = new System.Windows.Forms.MenuItem();
			this.MyProgressBar = new System.Windows.Forms.ProgressBar();
			this.ImageListButton = new System.Windows.Forms.ImageList(this.components);
			this.PanelStartPage.SuspendLayout();
			this.PanelButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DgStart)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PbStart)).BeginInit();
			this.PanelTreeView.SuspendLayout();
			this.ProjectsTreeView.SuspendLayout();
			this.PanelOutput.SuspendLayout();
			this.SuspendLayout();
			// 
			// MyContextMenu
			// 
			this.MyContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiTranslate,
			this.MiFind,
			this.MiCount,
			this.MiSetReference,
			this.MiUpdateReferences,
			this.MiRemove,
			this.MiGlossary,
			this.MiProgressSpec,
			this.MiCheckDialogs,
			this.MiAmpersand,
			this.MiPlaceholder,
			this.MiTranslation,
			this.MiAutoTranslate,
			this.MiRecover,
			this.MIPurge,
			this.menuItem1,
			this.MiCheckIn,
			this.MiCheckOut,
			this.MiGetLatest,
			this.MiUndoCheckOut});
			// 
			// MiTranslate
			// 
			this.MiTranslate.Index = 0;
			resources.ApplyResources(this.MiTranslate, "MiTranslate");
			this.MiTranslate.Click += new System.EventHandler(this.MiTranslate_Click);
			// 
			// MiFind
			// 
			this.MiFind.Index = 1;
			resources.ApplyResources(this.MiFind, "MiFind");
			this.MiFind.Click += new System.EventHandler(this.MiFind_Click);
			// 
			// MiCount
			// 
			this.MiCount.Index = 2;
			resources.ApplyResources(this.MiCount, "MiCount");
			this.MiCount.Click += new System.EventHandler(this.MiCount_Click);
			// 
			// MiSetReference
			// 
			this.MiSetReference.Index = 3;
			resources.ApplyResources(this.MiSetReference, "MiSetReference");
			this.MiSetReference.Click += new System.EventHandler(this.MiSetReference_Click);
			// 
			// MiUpdateReferences
			// 
			this.MiUpdateReferences.Index = 4;
			resources.ApplyResources(this.MiUpdateReferences, "MiUpdateReferences");
			this.MiUpdateReferences.Click += new System.EventHandler(this.MiUpdateReferences_Click);
			// 
			// MiRemove
			// 
			this.MiRemove.Index = 5;
			resources.ApplyResources(this.MiRemove, "MiRemove");
			this.MiRemove.Click += new System.EventHandler(this.MiRemove_Click);
			// 
			// MiGlossary
			// 
			this.MiGlossary.Index = 6;
			resources.ApplyResources(this.MiGlossary, "MiGlossary");
			this.MiGlossary.Click += new System.EventHandler(this.MiGlossary_Click);
			// 
			// MiProgressSpec
			// 
			this.MiProgressSpec.Index = 7;
			resources.ApplyResources(this.MiProgressSpec, "MiProgressSpec");
			this.MiProgressSpec.Click += new System.EventHandler(this.MiProgressSpec_Click);
			// 
			// MiCheckDialogs
			// 
			this.MiCheckDialogs.Index = 8;
			resources.ApplyResources(this.MiCheckDialogs, "MiCheckDialogs");
			this.MiCheckDialogs.Click += new System.EventHandler(this.MiCheckDialogs_Click);
			// 
			// MiAmpersand
			// 
			this.MiAmpersand.Index = 9;
			resources.ApplyResources(this.MiAmpersand, "MiAmpersand");
			this.MiAmpersand.Click += new System.EventHandler(this.MiAmpersand_Click);
			// 
			// MiPlaceholder
			// 
			this.MiPlaceholder.Index = 10;
			resources.ApplyResources(this.MiPlaceholder, "MiPlaceholder");
			this.MiPlaceholder.Click += new System.EventHandler(this.MiPlaceholder_Click);
			// 
			// MiTranslation
			// 
			this.MiTranslation.Index = 11;
			this.MiTranslation.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiAllStrings,
			this.MiNotTranslatedStrings,
			this.MiTranslatedStrings,
			this.menuItem2,
			this.MiXmlAllStrings,
			this.MiXmlNotTranslatedStrings,
			this.MiXmlTranslatedStrings,
			this.menuItem4,
			this.MiCSVAllStrings,
			this.MiCSVNotTranslatedStrings,
			this.MiCSVTranslatedStrings});
			resources.ApplyResources(this.MiTranslation, "MiTranslation");
			// 
			// MiAllStrings
			// 
			this.MiAllStrings.Index = 0;
			resources.ApplyResources(this.MiAllStrings, "MiAllStrings");
			this.MiAllStrings.Click += new System.EventHandler(this.MiExportTranslation_Click);
			// 
			// MiNotTranslatedStrings
			// 
			this.MiNotTranslatedStrings.Index = 1;
			resources.ApplyResources(this.MiNotTranslatedStrings, "MiNotTranslatedStrings");
			this.MiNotTranslatedStrings.Click += new System.EventHandler(this.MiExportTranslation_Click);
			// 
			// MiTranslatedStrings
			// 
			this.MiTranslatedStrings.Index = 2;
			resources.ApplyResources(this.MiTranslatedStrings, "MiTranslatedStrings");
			this.MiTranslatedStrings.Click += new System.EventHandler(this.MiExportTranslation_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 3;
			resources.ApplyResources(this.menuItem2, "menuItem2");
			// 
			// MiXmlAllStrings
			// 
			this.MiXmlAllStrings.Index = 4;
			resources.ApplyResources(this.MiXmlAllStrings, "MiXmlAllStrings");
			this.MiXmlAllStrings.Click += new System.EventHandler(this.MiExportXmlTranslation_Click);
			// 
			// MiXmlNotTranslatedStrings
			// 
			this.MiXmlNotTranslatedStrings.Index = 5;
			resources.ApplyResources(this.MiXmlNotTranslatedStrings, "MiXmlNotTranslatedStrings");
			this.MiXmlNotTranslatedStrings.Click += new System.EventHandler(this.MiExportXmlTranslation_Click);
			// 
			// MiXmlTranslatedStrings
			// 
			this.MiXmlTranslatedStrings.Index = 6;
			resources.ApplyResources(this.MiXmlTranslatedStrings, "MiXmlTranslatedStrings");
			this.MiXmlTranslatedStrings.Click += new System.EventHandler(this.MiExportXmlTranslation_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 7;
			resources.ApplyResources(this.menuItem4, "menuItem4");
			// 
			// MiCSVAllStrings
			// 
			this.MiCSVAllStrings.Index = 8;
			resources.ApplyResources(this.MiCSVAllStrings, "MiCSVAllStrings");
			this.MiCSVAllStrings.Click += new System.EventHandler(this.MiExportCSVTranslation_Click);
			// 
			// MiCSVNotTranslatedStrings
			// 
			this.MiCSVNotTranslatedStrings.Index = 9;
			resources.ApplyResources(this.MiCSVNotTranslatedStrings, "MiCSVNotTranslatedStrings");
			this.MiCSVNotTranslatedStrings.Click += new System.EventHandler(this.MiExportCSVTranslation_Click);
			// 
			// MiCSVTranslatedStrings
			// 
			this.MiCSVTranslatedStrings.Index = 10;
			resources.ApplyResources(this.MiCSVTranslatedStrings, "MiCSVTranslatedStrings");
			this.MiCSVTranslatedStrings.Click += new System.EventHandler(this.MiExportCSVTranslation_Click);
			// 
			// MiAutoTranslate
			// 
			this.MiAutoTranslate.Index = 12;
			this.MiAutoTranslate.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiAutoTranslateBase,
			this.MiTranslateFromKnowledge,
			this.MiTranslateFromLanguage,
			this.MiTranslateJson});
			resources.ApplyResources(this.MiAutoTranslate, "MiAutoTranslate");
			// 
			// MiAutoTranslateBase
			// 
			this.MiAutoTranslateBase.Index = 0;
			resources.ApplyResources(this.MiAutoTranslateBase, "MiAutoTranslateBase");
			this.MiAutoTranslateBase.Click += new System.EventHandler(this.MiAutoTranslate_Click);
			// 
			// MiTranslateFromKnowledge
			// 
			this.MiTranslateFromKnowledge.Index = 1;
			resources.ApplyResources(this.MiTranslateFromKnowledge, "MiTranslateFromKnowledge");
			this.MiTranslateFromKnowledge.Click += new System.EventHandler(this.MiTranslateFromKnowledge_Click);
			// 
			// MiTranslateJson
			// 
			this.MiTranslateJson.Index = 3;
			resources.ApplyResources(this.MiTranslateJson, "MiTranslateJson");
			this.MiTranslateJson.Click += new System.EventHandler(this.MiTranslateJson_Click);
			// 
			// MiTranslateFromLanguage
			// 
			this.MiTranslateFromLanguage.Index = 2;
			resources.ApplyResources(this.MiTranslateFromLanguage, "MiTranslateFromLanguage");
			this.MiTranslateFromLanguage.Click += new System.EventHandler(this.MiTranslateFromLanguage_Click);
			// 
			// MiRecover
			// 
			this.MiRecover.Index = 13;
			resources.ApplyResources(this.MiRecover, "MiRecover");
			this.MiRecover.Click += new System.EventHandler(this.MiRecover_Click);
			// 
			// MIPurge
			// 
			this.MIPurge.Index = 14;
			resources.ApplyResources(this.MIPurge, "MIPurge");
			this.MIPurge.Click += new System.EventHandler(this.MIPurge_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 15;
			resources.ApplyResources(this.menuItem1, "menuItem1");
			// 
			// MiCheckIn
			// 
			this.MiCheckIn.Index = 16;
			resources.ApplyResources(this.MiCheckIn, "MiCheckIn");
			this.MiCheckIn.Click += new System.EventHandler(this.MiCheckIn_Click);
			// 
			// MiCheckOut
			// 
			this.MiCheckOut.Index = 17;
			resources.ApplyResources(this.MiCheckOut, "MiCheckOut");
			this.MiCheckOut.Click += new System.EventHandler(this.MiCheckOut_Click);
			// 
			// MiGetLatest
			// 
			this.MiGetLatest.Index = 18;
			resources.ApplyResources(this.MiGetLatest, "MiGetLatest");
			this.MiGetLatest.Click += new System.EventHandler(this.MiGetLatest_Click);
			// 
			// MiUndoCheckOut
			// 
			this.MiUndoCheckOut.Index = 19;
			resources.ApplyResources(this.MiUndoCheckOut, "MiUndoCheckOut");
			this.MiUndoCheckOut.Click += new System.EventHandler(this.MiUndoCheckOut_Click);
			// 
			// ProjectsTreeViewImageList
			// 
			this.ProjectsTreeViewImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ProjectsTreeViewImageList.ImageStream")));
			this.ProjectsTreeViewImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.ProjectsTreeViewImageList.Images.SetKeyName(0, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(1, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(2, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(3, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(4, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(5, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(6, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(7, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(8, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(9, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(10, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(11, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(12, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(13, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(14, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(15, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(16, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(17, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(18, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(19, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(20, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(21, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(22, "");
			this.ProjectsTreeViewImageList.Images.SetKeyName(23, "");
			// 
			// StateImageList
			// 
			this.StateImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("StateImageList.ImageStream")));
			this.StateImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.StateImageList.Images.SetKeyName(0, "Merge-Readonly.bmp");
			this.StateImageList.Images.SetKeyName(1, "Merge-CheckOut.bmp");
			// 
			// MyStatusBar
			// 
			resources.ApplyResources(this.MyStatusBar, "MyStatusBar");
			this.MyStatusBar.Name = "MyStatusBar";
			// 
			// MainContextMenu
			// 
			this.MainContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiFile,
			this.MiProjects,
			this.MiOptions,
			this.MiLocalizerTools,
			this.MiSourceControl,
			this.MiQuestion});
			// 
			// MiFile
			// 
			this.MiFile.Index = 0;
			this.MiFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiNew,
			this.MiOpen,
			this.MiCloseSol,
			this.MiFileSeparator1,
			this.MiDir,
			this.MiXml,
			this.MiFileSeparator2,
			this.MiSupport,
			this.MiGlossaries,
			this.MiTools,
			this.MiFilterDictionaries,
			this.MiRefresh,
			this.MiCollapse,
			this.MiFileSeparator3,
			this.MiImportXml,
			this.MiImportHTML,
			this.menuItem3,
			this.MiSave,
			this.MiSaveAs,
			this.MiFileSeparator4,
			this.MiClose});
			resources.ApplyResources(this.MiFile, "MiFile");
			// 
			// MiNew
			// 
			this.MiNew.Index = 0;
			resources.ApplyResources(this.MiNew, "MiNew");
			this.MiNew.Click += new System.EventHandler(this.MiNew_Click);
			// 
			// MiOpen
			// 
			this.MiOpen.Index = 1;
			resources.ApplyResources(this.MiOpen, "MiOpen");
			this.MiOpen.Click += new System.EventHandler(this.MiOpen_Click);
			// 
			// MiCloseSol
			// 
			resources.ApplyResources(this.MiCloseSol, "MiCloseSol");
			this.MiCloseSol.Index = 2;
			this.MiCloseSol.Click += new System.EventHandler(this.MiCloseSol_Click);
			// 
			// MiFileSeparator1
			// 
			this.MiFileSeparator1.Index = 3;
			resources.ApplyResources(this.MiFileSeparator1, "MiFileSeparator1");
			// 
			// MiDir
			// 
			resources.ApplyResources(this.MiDir, "MiDir");
			this.MiDir.Index = 4;
			this.MiDir.Click += new System.EventHandler(this.MiDir_Click);
			// 
			// MiXml
			// 
			resources.ApplyResources(this.MiXml, "MiXml");
			this.MiXml.Index = 5;
			this.MiXml.Click += new System.EventHandler(this.MiXml_Click);
			// 
			// MiFileSeparator2
			// 
			this.MiFileSeparator2.Index = 6;
			resources.ApplyResources(this.MiFileSeparator2, "MiFileSeparator2");
			// 
			// MiSupport
			// 
			this.MiSupport.Index = 7;
			this.MiSupport.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiSupportChoose,
			this.MiResetSupport});
			resources.ApplyResources(this.MiSupport, "MiSupport");
			// 
			// MiSupportChoose
			// 
			this.MiSupportChoose.Index = 0;
			resources.ApplyResources(this.MiSupportChoose, "MiSupportChoose");
			this.MiSupportChoose.Click += new System.EventHandler(this.MiSupport_Click);
			// 
			// MiResetSupport
			// 
			this.MiResetSupport.Index = 1;
			resources.ApplyResources(this.MiResetSupport, "MiResetSupport");
			this.MiResetSupport.Click += new System.EventHandler(this.MiResetSupport_Click);
			// 
			// MiGlossaries
			// 
			this.MiGlossaries.Index = 8;
			resources.ApplyResources(this.MiGlossaries, "MiGlossaries");
			this.MiGlossaries.Click += new System.EventHandler(this.MiGlossaries_Click);
			// 
			// MiTools
			// 
			this.MiTools.Index = 9;
			resources.ApplyResources(this.MiTools, "MiTools");
			this.MiTools.Click += new System.EventHandler(this.MiTools_Click);
			// 
			// MiFilterDictionaries
			// 
			this.MiFilterDictionaries.Index = 10;
			resources.ApplyResources(this.MiFilterDictionaries, "MiFilterDictionaries");
			this.MiFilterDictionaries.Click += new System.EventHandler(this.MiFilterDictionaries_Click);
			// 
			// MiRefresh
			// 
			this.MiRefresh.Index = 11;
			resources.ApplyResources(this.MiRefresh, "MiRefresh");
			this.MiRefresh.Click += new System.EventHandler(this.MiRefresh_Click);
			// 
			// MiCollapse
			// 
			this.MiCollapse.Index = 12;
			resources.ApplyResources(this.MiCollapse, "MiCollapse");
			this.MiCollapse.Click += new System.EventHandler(this.MiCollapse_Click);
			// 
			// MiFileSeparator3
			// 
			this.MiFileSeparator3.Index = 13;
			resources.ApplyResources(this.MiFileSeparator3, "MiFileSeparator3");
			// 
			// MiImportXml
			// 
			this.MiImportXml.Index = 14;
			resources.ApplyResources(this.MiImportXml, "MiImportXml");
			this.MiImportXml.Click += new System.EventHandler(this.MiImportXmlTranslation_Click);
			// 
			// MiImportHTML
			// 
			this.MiImportHTML.Index = 15;
			resources.ApplyResources(this.MiImportHTML, "MiImportHTML");
			this.MiImportHTML.Click += new System.EventHandler(this.MiImportHTML_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 16;
			resources.ApplyResources(this.menuItem3, "menuItem3");
			// 
			// MiSave
			// 
			resources.ApplyResources(this.MiSave, "MiSave");
			this.MiSave.Index = 17;
			this.MiSave.Click += new System.EventHandler(this.MiSave_Click);
			// 
			// MiSaveAs
			// 
			resources.ApplyResources(this.MiSaveAs, "MiSaveAs");
			this.MiSaveAs.Index = 18;
			this.MiSaveAs.Click += new System.EventHandler(this.MiSaveAs_Click);
			// 
			// MiFileSeparator4
			// 
			this.MiFileSeparator4.Index = 19;
			resources.ApplyResources(this.MiFileSeparator4, "MiFileSeparator4");
			// 
			// MiClose
			// 
			this.MiClose.Index = 20;
			resources.ApplyResources(this.MiClose, "MiClose");
			this.MiClose.Click += new System.EventHandler(this.MiClose_Click);
			// 
			// MiProjects
			// 
			this.MiProjects.Index = 1;
			this.MiProjects.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiManageProjects,
			this.MiRemoveProjects,
			this.MiSelectAll,
			this.MiProjectsSeparator1,
			this.MiDictionary,
			this.MiProjectsSeparator2,
			this.MiBuildSelectedProjects,
			this.MiBuildSolution,
			this.MiProjectsSeparator3,
			this.MiZipDictionary,
			this.MiUnzipDictionary});
			resources.ApplyResources(this.MiProjects, "MiProjects");
			// 
			// MiManageProjects
			// 
			resources.ApplyResources(this.MiManageProjects, "MiManageProjects");
			this.MiManageProjects.Index = 0;
			this.MiManageProjects.Click += new System.EventHandler(this.MiManageProjects_Click);
			// 
			// MiRemoveProjects
			// 
			resources.ApplyResources(this.MiRemoveProjects, "MiRemoveProjects");
			this.MiRemoveProjects.Index = 1;
			this.MiRemoveProjects.Click += new System.EventHandler(this.MiRemoveProjects_Click);
			// 
			// MiSelectAll
			// 
			resources.ApplyResources(this.MiSelectAll, "MiSelectAll");
			this.MiSelectAll.Index = 2;
			this.MiSelectAll.Click += new System.EventHandler(this.MiSelectAll_Click);
			// 
			// MiProjectsSeparator1
			// 
			this.MiProjectsSeparator1.Index = 3;
			resources.ApplyResources(this.MiProjectsSeparator1, "MiProjectsSeparator1");
			// 
			// MiDictionary
			// 
			resources.ApplyResources(this.MiDictionary, "MiDictionary");
			this.MiDictionary.Index = 4;
			this.MiDictionary.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiCreate,
			this.MiCustomCreate,
			this.MiAddDictionary,
			this.MiImportDictionary});
			this.MiDictionary.Select += new System.EventHandler(this.MiDictionary_Select);
			// 
			// MiCreate
			// 
			resources.ApplyResources(this.MiCreate, "MiCreate");
			this.MiCreate.Index = 0;
			this.MiCreate.Click += new System.EventHandler(this.MiCreate_Click);
			// 
			// MiCustomCreate
			// 
			resources.ApplyResources(this.MiCustomCreate, "MiCustomCreate");
			this.MiCustomCreate.Index = 1;
			this.MiCustomCreate.Click += new System.EventHandler(this.MiCustomCreate_Click);
			// 
			// MiAddDictionary
			// 
			resources.ApplyResources(this.MiAddDictionary, "MiAddDictionary");
			this.MiAddDictionary.Index = 2;
			this.MiAddDictionary.Click += new System.EventHandler(this.MiAddDictionary_Click);
			// 
			// MiImportDictionary
			// 
			this.MiImportDictionary.Index = 3;
			resources.ApplyResources(this.MiImportDictionary, "MiImportDictionary");
			this.MiImportDictionary.Click += new System.EventHandler(this.MiImportDictionary_Click);
			// 
			// MiProjectsSeparator2
			// 
			this.MiProjectsSeparator2.Index = 5;
			resources.ApplyResources(this.MiProjectsSeparator2, "MiProjectsSeparator2");
			// 
			// MiBuildSelectedProjects
			// 
			resources.ApplyResources(this.MiBuildSelectedProjects, "MiBuildSelectedProjects");
			this.MiBuildSelectedProjects.Index = 6;
			this.MiBuildSelectedProjects.Click += new System.EventHandler(this.MiBuildSelectedProjects_Click);
			// 
			// MiBuildSolution
			// 
			resources.ApplyResources(this.MiBuildSolution, "MiBuildSolution");
			this.MiBuildSolution.Index = 7;
			this.MiBuildSolution.Click += new System.EventHandler(this.MiBuildSolution_Click);
			// 
			// MiProjectsSeparator3
			// 
			this.MiProjectsSeparator3.Index = 8;
			resources.ApplyResources(this.MiProjectsSeparator3, "MiProjectsSeparator3");
			// 
			// MiZipDictionary
			// 
			resources.ApplyResources(this.MiZipDictionary, "MiZipDictionary");
			this.MiZipDictionary.Index = 9;
			this.MiZipDictionary.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiChooseLanguage,
			this.MiAllLanguage});
			// 
			// MiChooseLanguage
			// 
			this.MiChooseLanguage.Index = 0;
			resources.ApplyResources(this.MiChooseLanguage, "MiChooseLanguage");
			this.MiChooseLanguage.Click += new System.EventHandler(this.MiChooseLanguage_Click);
			// 
			// MiAllLanguage
			// 
			this.MiAllLanguage.Index = 1;
			resources.ApplyResources(this.MiAllLanguage, "MiAllLanguage");
			this.MiAllLanguage.Click += new System.EventHandler(this.MiAllLanguage_Click);
			// 
			// MiUnzipDictionary
			// 
			this.MiUnzipDictionary.Index = 10;
			resources.ApplyResources(this.MiUnzipDictionary, "MiUnzipDictionary");
			this.MiUnzipDictionary.Click += new System.EventHandler(this.MiUnzipDictionary_Click);
			// 
			// MiOptions
			// 
			this.MiOptions.Index = 2;
			this.MiOptions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiProgress,
			this.MiGlossaryOptions,
			this.MiRapidCreation,
			this.MiTempAsTrans,
			this.MiSepOpt1,
			this.MiEnvironmentSettings,
			this.MiSepOpt2,
			this.MiFont});
			resources.ApplyResources(this.MiOptions, "MiOptions");
			this.MiOptions.Select += new System.EventHandler(this.MiOptions_Select);
			// 
			// MiProgress
			// 
			this.MiProgress.Index = 0;
			resources.ApplyResources(this.MiProgress, "MiProgress");
			this.MiProgress.Click += new System.EventHandler(this.MiProgress_Click);
			// 
			// MiGlossaryOptions
			// 
			this.MiGlossaryOptions.Index = 1;
			this.MiGlossaryOptions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiViewGlossary,
			this.MiSetGlossaryFolder});
			resources.ApplyResources(this.MiGlossaryOptions, "MiGlossaryOptions");
			this.MiGlossaryOptions.Click += new System.EventHandler(this.MiViewGlossary_Click);
			// 
			// MiViewGlossary
			// 
			this.MiViewGlossary.Index = 0;
			resources.ApplyResources(this.MiViewGlossary, "MiViewGlossary");
			this.MiViewGlossary.Click += new System.EventHandler(this.MiViewGlossary_Click);
			// 
			// MiSetGlossaryFolder
			// 
			this.MiSetGlossaryFolder.Index = 1;
			resources.ApplyResources(this.MiSetGlossaryFolder, "MiSetGlossaryFolder");
			this.MiSetGlossaryFolder.Click += new System.EventHandler(this.MiSetGlossaryFolder_Click);
			// 
			// MiRapidCreation
			// 
			this.MiRapidCreation.Index = 2;
			resources.ApplyResources(this.MiRapidCreation, "MiRapidCreation");
			this.MiRapidCreation.Click += new System.EventHandler(this.MiRapidCreation_Click);
			// 
			// MiTempAsTrans
			// 
			this.MiTempAsTrans.Index = 3;
			resources.ApplyResources(this.MiTempAsTrans, "MiTempAsTrans");
			this.MiTempAsTrans.Click += new System.EventHandler(this.MiTempAsTrans_Click);
			// 
			// MiSepOpt1
			// 
			this.MiSepOpt1.Index = 4;
			resources.ApplyResources(this.MiSepOpt1, "MiSepOpt1");
			// 
			// MiEnvironmentSettings
			// 
			this.MiEnvironmentSettings.Index = 5;
			resources.ApplyResources(this.MiEnvironmentSettings, "MiEnvironmentSettings");
			this.MiEnvironmentSettings.Click += new System.EventHandler(this.MiEnvironmentSettings_Click);
			// 
			// MiSepOpt2
			// 
			this.MiSepOpt2.Index = 6;
			resources.ApplyResources(this.MiSepOpt2, "MiSepOpt2");
			// 
			// MiFont
			// 
			this.MiFont.Index = 7;
			resources.ApplyResources(this.MiFont, "MiFont");
			this.MiFont.Click += new System.EventHandler(this.MiFont_Click);
			// 
			// MiLocalizerTools
			// 
			this.MiLocalizerTools.Index = 3;
			this.MiLocalizerTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiTbLoader,
			this.MiDictionaryViewer});
			resources.ApplyResources(this.MiLocalizerTools, "MiLocalizerTools");
			// 
			// MiTbLoader
			// 
			this.MiTbLoader.Index = 0;
			resources.ApplyResources(this.MiTbLoader, "MiTbLoader");
			this.MiTbLoader.Click += new System.EventHandler(this.MiTbLoader_Click);
			// 
			// MiDictionaryViewer
			// 
			this.MiDictionaryViewer.Index = 1;
			resources.ApplyResources(this.MiDictionaryViewer, "MiDictionaryViewer");
			this.MiDictionaryViewer.Click += new System.EventHandler(this.MiDictionaryViewer_Click);
			// 
			// MiSourceControl
			// 
			this.MiSourceControl.Index = 4;
			this.MiSourceControl.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiTeamSystem,
			this.MiEnableSourceControl});
			resources.ApplyResources(this.MiSourceControl, "MiSourceControl");
			this.MiSourceControl.Select += new System.EventHandler(this.MiSourceControl_Select);
			// 
			// MiTeamSystem
			// 
			this.MiTeamSystem.Index = 0;
			this.MiTeamSystem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiTFSSourceBinding});
			resources.ApplyResources(this.MiTeamSystem, "MiTeamSystem");
			// 
			// MiTFSSourceBinding
			// 
			this.MiTFSSourceBinding.Index = 0;
			resources.ApplyResources(this.MiTFSSourceBinding, "MiTFSSourceBinding");
			this.MiTFSSourceBinding.Click += new System.EventHandler(this.MiTFSSourceBinding_Click);
			// 
			// MiEnableSourceControl
			// 
			this.MiEnableSourceControl.Index = 1;
			resources.ApplyResources(this.MiEnableSourceControl, "MiEnableSourceControl");
			this.MiEnableSourceControl.Click += new System.EventHandler(this.MiEnableSourceControl_Click);
			// 
			// MiQuestion
			// 
			this.MiQuestion.Index = 5;
			this.MiQuestion.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MiHelp,
			this.MiAbout});
			resources.ApplyResources(this.MiQuestion, "MiQuestion");
			// 
			// MiHelp
			// 
			this.MiHelp.Index = 0;
			resources.ApplyResources(this.MiHelp, "MiHelp");
			this.MiHelp.Click += new System.EventHandler(this.MiHelp_Click);
			// 
			// MiAbout
			// 
			this.MiAbout.Index = 1;
			resources.ApplyResources(this.MiAbout, "MiAbout");
			this.MiAbout.Click += new System.EventHandler(this.MiAbout_Click);
			// 
			// PanelStartPage
			// 
			resources.ApplyResources(this.PanelStartPage, "PanelStartPage");
			this.PanelStartPage.BackColor = System.Drawing.Color.White;
			this.PanelStartPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.PanelStartPage.Controls.Add(this.PanelButtons);
			this.PanelStartPage.Controls.Add(this.DgStart);
			this.PanelStartPage.Controls.Add(this.PbStart);
			this.PanelStartPage.ForeColor = System.Drawing.SystemColors.ControlText;
			this.PanelStartPage.Name = "PanelStartPage";
			this.PanelStartPage.SizeChanged += new System.EventHandler(this.PanelStartPage_SizeChanged);
			this.PanelStartPage.Enter += new System.EventHandler(this.PanelStartPage_Enter);
			// 
			// PanelButtons
			// 
			this.PanelButtons.Controls.Add(this.LnkNew);
			this.PanelButtons.Controls.Add(this.LnkOpen);
			this.PanelButtons.Controls.Add(this.pictureBox2);
			this.PanelButtons.Controls.Add(this.pictureBox1);
			resources.ApplyResources(this.PanelButtons, "PanelButtons");
			this.PanelButtons.Name = "PanelButtons";
			// 
			// LnkNew
			// 
			resources.ApplyResources(this.LnkNew, "LnkNew");
			this.LnkNew.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.LnkNew.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.LnkNew.LinkColor = System.Drawing.Color.Blue;
			this.LnkNew.Name = "LnkNew";
			this.LnkNew.TabStop = true;
			this.LnkNew.UseCompatibleTextRendering = true;
			this.LnkNew.VisitedLinkColor = System.Drawing.SystemColors.ActiveCaption;
			this.LnkNew.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkNew_LinkClicked);
			// 
			// LnkOpen
			// 
			resources.ApplyResources(this.LnkOpen, "LnkOpen");
			this.LnkOpen.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.LnkOpen.LinkColor = System.Drawing.Color.Blue;
			this.LnkOpen.Name = "LnkOpen";
			this.LnkOpen.TabStop = true;
			this.LnkOpen.UseCompatibleTextRendering = true;
			this.LnkOpen.VisitedLinkColor = System.Drawing.SystemColors.ActiveCaption;
			this.LnkOpen.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkOpen_LinkClicked);
			// 
			// pictureBox2
			// 
			resources.ApplyResources(this.pictureBox2, "pictureBox2");
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.TabStop = false;
			// 
			// pictureBox1
			// 
			resources.ApplyResources(this.pictureBox1, "pictureBox1");
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.TabStop = false;
			// 
			// DgStart
			// 
			this.DgStart.AllowNavigation = false;
			this.DgStart.AllowSorting = false;
			this.DgStart.AlternatingBackColor = System.Drawing.Color.Lavender;
			resources.ApplyResources(this.DgStart, "DgStart");
			this.DgStart.BackColor = System.Drawing.Color.White;
			this.DgStart.BackgroundColor = System.Drawing.Color.White;
			this.DgStart.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.DgStart.CaptionBackColor = System.Drawing.Color.White;
			this.DgStart.CaptionForeColor = System.Drawing.Color.RoyalBlue;
			this.DgStart.ColumnHeadersVisible = false;
			this.DgStart.DataMember = "";
			this.DgStart.FlatMode = true;
			this.DgStart.ForeColor = System.Drawing.Color.RoyalBlue;
			this.DgStart.GridLineColor = System.Drawing.Color.White;
			this.DgStart.GridLineStyle = System.Windows.Forms.DataGridLineStyle.None;
			this.DgStart.HeaderBackColor = System.Drawing.Color.White;
			this.DgStart.HeaderFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DgStart.HeaderForeColor = System.Drawing.Color.RoyalBlue;
			this.DgStart.LinkColor = System.Drawing.Color.White;
			this.DgStart.Name = "DgStart";
			this.DgStart.ParentRowsBackColor = System.Drawing.Color.White;
			this.DgStart.ParentRowsForeColor = System.Drawing.Color.White;
			this.DgStart.ParentRowsVisible = false;
			this.DgStart.PreferredColumnWidth = 300;
			this.DgStart.ReadOnly = true;
			this.DgStart.RowHeadersVisible = false;
			this.DgStart.SelectionBackColor = System.Drawing.Color.White;
			this.DgStart.SelectionForeColor = System.Drawing.Color.White;
			this.DgStart.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
			this.MyTableStyle});
			this.DgStart.TabStop = false;
			this.DgStart.CurrentCellChanged += new System.EventHandler(this.DgStart_CurrentCellChanged);
			this.DgStart.Enter += new System.EventHandler(this.DgStart_Enter);
			this.DgStart.MouseLeave += new System.EventHandler(this.DgStart_MouseLeave);
			this.DgStart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DgStart_MouseMove);
			// 
			// MyTableStyle
			// 
			this.MyTableStyle.AlternatingBackColor = System.Drawing.Color.Lavender;
			this.MyTableStyle.BackColor = System.Drawing.Color.White;
			this.MyTableStyle.ColumnHeadersVisible = false;
			this.MyTableStyle.DataGrid = this.DgStart;
			this.MyTableStyle.ForeColor = System.Drawing.Color.RoyalBlue;
			this.MyTableStyle.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
			this.HeaderColumnStyle,
			this.HiddenColumnStyle});
			this.MyTableStyle.GridLineStyle = System.Windows.Forms.DataGridLineStyle.None;
			this.MyTableStyle.HeaderBackColor = System.Drawing.Color.White;
			this.MyTableStyle.HeaderForeColor = System.Drawing.Color.RoyalBlue;
			this.MyTableStyle.ReadOnly = true;
			this.MyTableStyle.RowHeadersVisible = false;
			this.MyTableStyle.SelectionBackColor = System.Drawing.Color.White;
			// 
			// HeaderColumnStyle
			// 
			this.HeaderColumnStyle.Format = "";
			this.HeaderColumnStyle.FormatInfo = null;
			resources.ApplyResources(this.HeaderColumnStyle, "HeaderColumnStyle");
			this.HeaderColumnStyle.ReadOnly = true;
			// 
			// HiddenColumnStyle
			// 
			this.HiddenColumnStyle.Format = "";
			this.HiddenColumnStyle.FormatInfo = null;
			resources.ApplyResources(this.HiddenColumnStyle, "HiddenColumnStyle");
			this.HiddenColumnStyle.ReadOnly = true;
			// 
			// PbStart
			// 
			this.PbStart.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.PbStart, "PbStart");
			this.PbStart.Name = "PbStart";
			this.PbStart.TabStop = false;
			// 
			// PanelTreeView
			// 
			this.PanelTreeView.BackColor = System.Drawing.Color.White;
			this.PanelTreeView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.PanelTreeView.Controls.Add(this.ProjectsTreeView);
			this.PanelTreeView.Controls.Add(this.HorizontalSplitter);
			this.PanelTreeView.Controls.Add(this.PanelOutput);
			resources.ApplyResources(this.PanelTreeView, "PanelTreeView");
			this.PanelTreeView.ForeColor = System.Drawing.SystemColors.ControlText;
			this.PanelTreeView.Name = "PanelTreeView";
			// 
			// ProjectsTreeView
			// 
			this.ProjectsTreeView.AllowDrop = true;
			this.ProjectsTreeView.BackColor = System.Drawing.Color.White;
			this.ProjectsTreeView.CausesValidation = false;
			this.ProjectsTreeView.ContextMenu = this.MyContextMenu;
			this.ProjectsTreeView.Controls.Add(this.WaitingControl);
			this.ProjectsTreeView.Cursor = System.Windows.Forms.Cursors.Default;
			resources.ApplyResources(this.ProjectsTreeView, "ProjectsTreeView");
			this.ProjectsTreeView.HideSelection = false;
			this.ProjectsTreeView.ImageList = this.ProjectsTreeViewImageList;
			this.ProjectsTreeView.ItemHeight = 20;
			this.ProjectsTreeView.Name = "ProjectsTreeView";
			this.ProjectsTreeView.StateImageList = this.StateImageList;
			this.ProjectsTreeView.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.ProjectsTreeView_AfterCollapse);
			this.ProjectsTreeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.ProjectsTreeView_AfterExpand);
			this.ProjectsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ProjectsTreeView_AfterSelect);
			this.ProjectsTreeView.DoubleClick += new System.EventHandler(this.ProjectsTreeView_DoubleClick);
			this.ProjectsTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ProjectsTreeView_MouseDown);
			this.ProjectsTreeView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ProjectsTreeView_MouseMove);
			// 
			// WaitingControl
			// 
			resources.ApplyResources(this.WaitingControl, "WaitingControl");
			this.WaitingControl.BackColor = System.Drawing.Color.White;
			this.WaitingControl.Cursor = System.Windows.Forms.Cursors.WaitCursor;
			this.WaitingControl.Message = "";
			this.WaitingControl.Name = "WaitingControl";
			// 
			// HorizontalSplitter
			// 
			this.HorizontalSplitter.BackColor = System.Drawing.SystemColors.ControlDark;
			this.HorizontalSplitter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.HorizontalSplitter, "HorizontalSplitter");
			this.HorizontalSplitter.Name = "HorizontalSplitter";
			this.HorizontalSplitter.TabStop = false;
			// 
			// PanelOutput
			// 
			this.PanelOutput.BackColor = System.Drawing.SystemColors.ControlLight;
			this.PanelOutput.Controls.Add(this.BtnStopProcedure);
			this.PanelOutput.Controls.Add(this.TxtOutput);
			this.PanelOutput.Controls.Add(this.MyProgressBar);
			resources.ApplyResources(this.PanelOutput, "PanelOutput");
			this.PanelOutput.Name = "PanelOutput";
			// 
			// BtnStopProcedure
			// 
			resources.ApplyResources(this.BtnStopProcedure, "BtnStopProcedure");
			this.BtnStopProcedure.Name = "BtnStopProcedure";
			this.BtnStopProcedure.Click += new System.EventHandler(this.MiStop_Click);
			// 
			// TxtOutput
			// 
			resources.ApplyResources(this.TxtOutput, "TxtOutput");
			this.TxtOutput.AutoWordSelection = true;
			this.TxtOutput.BackColor = System.Drawing.SystemColors.Window;
			this.TxtOutput.ContextMenu = this.ResultsContextMenu;
			this.TxtOutput.Name = "TxtOutput";
			this.TxtOutput.ReadOnly = true;
			this.TxtOutput.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.TxtOutput_LinkClicked);
			// 
			// ResultsContextMenu
			// 
			this.ResultsContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.MenuItemClear,
			this.MenuItemCopy,
			this.MenuItemSave});
			// 
			// MenuItemClear
			// 
			this.MenuItemClear.Index = 0;
			resources.ApplyResources(this.MenuItemClear, "MenuItemClear");
			this.MenuItemClear.Click += new System.EventHandler(this.OnClearClick);
			// 
			// MenuItemCopy
			// 
			this.MenuItemCopy.Index = 1;
			resources.ApplyResources(this.MenuItemCopy, "MenuItemCopy");
			this.MenuItemCopy.Click += new System.EventHandler(this.OnCopyClick);
			// 
			// MenuItemSave
			// 
			this.MenuItemSave.Index = 2;
			resources.ApplyResources(this.MenuItemSave, "MenuItemSave");
			this.MenuItemSave.Click += new System.EventHandler(this.OnSaveClick);
			// 
			// MyProgressBar
			// 
			resources.ApplyResources(this.MyProgressBar, "MyProgressBar");
			this.MyProgressBar.Name = "MyProgressBar";
			this.MyProgressBar.Step = 1;
			// 
			// ImageListButton
			// 
			this.ImageListButton.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageListButton.ImageStream")));
			this.ImageListButton.TransparentColor = System.Drawing.Color.Transparent;
			this.ImageListButton.Images.SetKeyName(0, "");
			this.ImageListButton.Images.SetKeyName(1, "");
			// 
			// DictionaryCreator
			// 
			this.AllowDrop = true;
			this.ContextMenu = this.MainContextMenu;
			this.Controls.Add(this.MyStatusBar);
			this.Controls.Add(this.PanelTreeView);
			this.Controls.Add(this.PanelStartPage);
			resources.ApplyResources(this, "$this");
			this.MinimumSize = new System.Drawing.Size(563, 456);
			this.Name = "DictionaryCreator";
			this.Load += new System.EventHandler(this.DictionaryCreator_Load);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.DictionaryCreator_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.DictionaryCreator_DragEnter);
			this.PanelStartPage.ResumeLayout(false);
			this.PanelStartPage.PerformLayout();
			this.PanelButtons.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DgStart)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PbStart)).EndInit();
			this.PanelTreeView.ResumeLayout(false);
			this.ProjectsTreeView.ResumeLayout(false);
			this.PanelOutput.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		//---------------------------------------------------------------------
		private void MiAbout_Click(object sender, System.EventArgs e)
		{
			DateTime date = new DateTime(2000, 1, 1);
			date = date.AddDays((double)new Version(Application.ProductVersion).Build);

			string about =
				Application.ProductName +
				" v. " + Application.ProductVersion +
				" (" + date.ToShortDateString() + ").";

			MessageBox.Show
				(
				this,
				about,
				"About " + Application.ProductName,
				MessageBoxButtons.OK,
				MessageBoxIcon.Information
				);
		}

		//---------------------------------------------------------------------
		private ArrayList FilterPerIncomplete(ArrayList fileList)
		{
			ArrayList incompleteList = new ArrayList();
			foreach (string file in fileList)
			{
				LocalizerDocument doc = new LocalizerDocument();
				try
				{
					doc = LocalizerDocument.GetStandardXmlDocument(file, false);
					if (doc == null)
						throw new Exception("Documento null: " + file);
					if (!IsDocumentComplete(doc))
						incompleteList.Add(file);
				}
				catch (Exception exc)
				{
					Debug.Fail(string.Format("Failed reading document; {0}", exc.Message));
					continue;
				}
			}

			return incompleteList;
		}

		//---------------------------------------------------------------------
		private bool IsDocumentComplete(LocalizerDocument doc)
		{
			/*stringa di ricerca che seleziona i nodi che NON hanno l'attributo target 
                o che l'hanno contenente una stringa vuota 
                o che hanno temporary a true( se non bisogna contare i temporary come tardotti)*/
			/*"//string[not(@target) or @target='' or @temporary='true']"*/
			string research = null;
			if (CountTemporaryAsTranslated)
				research = String.Concat
					(
					"//",
					AllStrings.stringTag,
					"[not(@",
					AllStrings.target,
					") or @",
					AllStrings.target,
					"='']"
					);
			else
				research = String.Concat
					(
					"//",
					AllStrings.stringTag,
					"[not(@",
					AllStrings.target,
					") or @",
					AllStrings.target,
					"='' or @",
					AllStrings.temporary,
					"='",
					AllStrings.trueTag,
					"']"
					);
			XmlNodeList incompleteNodeList = doc.SelectNodes(research);
			return (incompleteNodeList == null || incompleteNodeList.Count == 0);
		}

		//---------------------------------------------------------------------
		private string GetOneLanguageFromSelectedNode()
		{
			if (SelectedNode.Type == NodeType.SOLUTION || SelectedNode.Type == NodeType.PROJECT)
			{
				ArrayList languageNodes = SelectedNode.GetTypedChildNodes(NodeType.LANGUAGE, true);
				ChooseLanguage languageDialog = new ChooseLanguage();
				languageDialog.FormTitle = Strings.ChooseLanguage;
				languageDialog.FillCombo((LocalizerTreeNode[])languageNodes.ToArray(typeof(LocalizerTreeNode)));
				if (languageDialog.ShowDialog(this) != DialogResult.OK)
					return null;
				return languageDialog.ChoosedLanguage.Name;
			}
			if (SelectedNode is DictionaryTreeNode)
				return ((DictionaryTreeNode)SelectedNode).Culture;

			return null;
		}
		//---------------------------------------------------------------------
		private void MiExportTranslation_Click(object sender, System.EventArgs e)
		{
			string culture = GetOneLanguageFromSelectedNode();
			if (culture == null)
				return;

			bool notTranslatedOnly = (MenuItem)sender == MiNotTranslatedStrings;
			bool translatedOnly = (MenuItem)sender == MiTranslatedStrings;

			ImportExport impexp = new ImportExport(new Logger(MyProgressBar, MyStatusBar, TxtOutput));
			impexp.ExportToHtml(
				culture,
				translatedOnly || !notTranslatedOnly,
				notTranslatedOnly || !translatedOnly
				);
		}

		//---------------------------------------------------------------------
		private void MiImportXmlTranslation_Click(object sender, EventArgs e)
		{
			bool notTranslatedOnly = sender == MiXmlNotTranslatedStrings;
			bool translatedOnly = sender == MiXmlTranslatedStrings;

			ImportExport impexp = new ImportExport(new Logger(MyProgressBar, MyStatusBar, TxtOutput));
			impexp.ImportXml();
		}
		//---------------------------------------------------------------------
		private void MiExportXmlTranslation_Click(object sender, EventArgs e)
		{
			string culture = GetOneLanguageFromSelectedNode();
			if (culture == null)
				return;

			bool notTranslatedOnly = (MenuItem)sender == MiXmlNotTranslatedStrings;
			bool translatedOnly = (MenuItem)sender == MiXmlTranslatedStrings;

			ImportExport impexp = new ImportExport(new Logger(MyProgressBar, MyStatusBar, TxtOutput));
			impexp.ExportToXml(
				culture,
				translatedOnly || !notTranslatedOnly,
				notTranslatedOnly || !translatedOnly
				);
		}

		//---------------------------------------------------------------------
		public ArrayList GetSelectedProjects()
		{
			return selectedNodeList;
		}

		//---------------------------------------------------------------------
		public ArrayList GetCommonDictionariesInSelectedProjects()
		{
			ArrayList commonDictionaries = new ArrayList();
			bool firstTime = true;
			foreach (LocalizerTreeNode t in GetSelectedProjects())
			{
				if (firstTime)
				{
					foreach (LocalizerTreeNode cultureNode in t.GetTypedChildNodes(NodeType.LANGUAGE, false))
						commonDictionaries.Add(cultureNode.Name);
					firstTime = false;
				}
				else
				{
					ArrayList toRemove = new ArrayList();
					foreach (string culture in commonDictionaries)
					{
						bool found = false;
						foreach (LocalizerTreeNode cultureNode in t.GetTypedChildNodes(NodeType.LANGUAGE, false))
						{
							if (string.Compare(cultureNode.Name, culture, true) == 0)
							{
								found = true;
								break;
							}
						}
						if (!found)
							toRemove.Add(culture);
					}

					foreach (string culture in toRemove)
						commonDictionaries.Remove(culture);
				}
			}

			return commonDictionaries;
		}

		//---------------------------------------------------------------------
		private void MiTools_Click(object sender, System.EventArgs e)
		{
			ToolsSpecifier toolsView = new ToolsSpecifier(ToolsList);
			if (toolsView.ShowDialog(this) == DialogResult.OK)
			{
				ToolsList = toolsView.ToolsInfo;
			}
		}

		//-----------------------------------------------------------------------------
		private void MiRapidCreation_Click(object sender, System.EventArgs e)
		{
			RapidCreation = !RapidCreation;
		}

		//---------------------------------------------------------------------
		private void ProjectsTreeView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			LocalizerTreeNode tn = ProjectsTreeView.GetNodeAt(e.X, e.Y) as LocalizerTreeNode;
			if (tn != null)
			{
				int currentNodeIndex = tn.Index;
				if (currentNodeIndex != oldNodeIndex)
				{
					oldNodeIndex = currentNodeIndex;
					if (this.ToolTipPrj != null && this.ToolTipPrj.Active)
						this.ToolTipPrj.Active = false; //turn it off 
					if (tn.Type == NodeType.PROJECT)
					{
						this.ToolTipPrj.SetToolTip(ProjectsTreeView, tn.FileSystemPath);
						this.ToolTipPrj.Active = true; //make it active so it can show 
					}
				}
			}
		}

		//-----------------------------------------------------------------------------
		private void MiRefresh_Click(object sender, System.EventArgs e)
		{
			RefreshTree();
		}

		//-----------------------------------------------------------------------------
		public void FreezeTree()
		{
			//			if (!ProjectsTreeViewFreezer.Created)
			//				return;
			//
			//			// if 'freezeTimes' is greater than zero, 'FreezeTree' has already been invoked
			//			// so I do nothing
			//			if (freezeTimes == 0)
			//				ProjectsTreeViewFreezer.BeginInvoke(new ControlFreezer.FreezerDelegate(ProjectsTreeViewFreezer.Freeze));
			//			freezeTimes ++;
		}

		//-----------------------------------------------------------------------------
		public void DefreezeTree()
		{
			//			if ( freezeTimes == 0 || !ProjectsTreeViewFreezer.Created)
			//				return;
			//			// if i call the freeze function multiple times, i have to call
			//			// the correspondent defreeze the same number of times before 
			//			// performing the actual defreeze action
			//			freezeTimes --;
			//			if ( freezeTimes == 0 )
			//				ProjectsTreeViewFreezer.BeginInvoke(new ControlFreezer.FreezerDelegate(ProjectsTreeViewFreezer.Defreeze));
		}

		public new void BeginInvoke(System.Delegate method, object[] args)
		{
			if (IsHandleCreated)
				base.BeginInvoke(method, args);
		}

		//---------------------------------------------------------------------
		public ProjectDocument GetPrjWriter(LocalizerTreeNode node)
		{
			LocalizerTreeNode projNode = node.GetTypedParentNode(NodeType.PROJECT);
			if (projNode == null)
				return null;

			return GetPrjWriter(projNode.FileSystemPath);
		}

		//---------------------------------------------------------------------
		public ProjectDocument GetPrjWriter(string tblPrjPath)
		{
			ProjectDocument prj = tblPrjFiles[tblPrjPath];

			if (prj != null)
				return prj;

			prj = new ProjectDocument();
			if (prj.Load(tblPrjPath))
			{
				//se non esiste il percorso, provo ad aggiustarlo usando il path finder corrente
				if (prj.SourceFolder != null && !Directory.Exists(prj.SourceFolder))
				{
					string standard = CommonFunctions.GetPathFinder().GetStandardPath();

					//rimappo la vecchia standard sulla nuova
					int standardIdx = prj.SourceFolder.IndexOf(NameSolverStrings.Standard);
					if (standardIdx > -1)
					{
						standardIdx += NameSolverStrings.Standard.Length;

						string path = standard + prj.SourceFolder.Substring(standardIdx);

						if (Directory.Exists(path))
							prj.SourceFolder = path;
					}
				}
				tblPrjFiles.Add(tblPrjPath, prj);
				return prj;
			}
			return null;
		}

		//---------------------------------------------------------------------
		private void MiSourceControl_Select(object sender, System.EventArgs e)
		{
			RefreshSourceControlMenu();
		}

		//--------------------------------------------------------------------------------
		private void RefreshSourceControlMenu()
		{
			MiEnableSourceControl.Checked =
			SourceControlManager.Enabled;

			OnMenuStateChanged();
		}

		//---------------------------------------------------------------------
		private void MiEnableSourceControl_Click(object sender, System.EventArgs e)
		{
			ToggleSourceControlEnabilitation();
			RefreshSourceControlStatusAsync();

		}

		//--------------------------------------------------------------------------------
		private void ToggleSourceControlEnabilitation()
		{
			if (tblslnWriter != null)
			{
				bool enable = !SourceControlManager.Enabled;
				if (enable && !SourceControlManager.TestDatabaseConnection())
					return;

				SourceControlManager.Enabled = enable;
				SolutionDocument.LocalInfo.SourceControlEnabled = SourceControlManager.Enabled;
			}
		}

		//---------------------------------------------------------------------
		private void tblslnWriter_FileNameChanged(object sender, FileNameChangedEventArgs args)
		{
			SourceControlManager = new SourceControlManager
				(
				args.NewName,
				this,
				SolutionDocument.LocalInfo.EnvironmentSettings.CompareExecutablePath,
				new Logger(MyProgressBar, MyStatusBar, TxtOutput)
				);

			if (tblslnWriter != null)
				SourceControlManager.Enabled = SolutionDocument.LocalInfo.SourceControlEnabled;
		}

		//--------------------------------------------------------------------------------
		private void MiOptions_Select(object sender, System.EventArgs e)
		{
			MiRapidCreation.Checked = RapidCreation;
			MiProgress.Checked = ShowTranslationProgress;
			MiTempAsTrans.Checked = CountTemporaryAsTranslated;
		}

		//--------------------------------------------------------------------------------
		private void SourceSafeManager_OnSourceTreeRefreshNeeded(object sender, EventArgs e)
		{
			RefreshTree();
		}

		//---------------------------------------------------------------------
		private void SourceSafeManager_CompareExecutablePathChanged(object sender, EventArgs e)
		{
			SolutionDocument.LocalInfo.EnvironmentSettings.CompareExecutablePath = SourceControlManager.CompareExecutablePath;
		}

		//---------------------------------------------------------------------
		private void MiTranslateFromKnowledge_Click(object sender, System.EventArgs e)
		{
			DialogResult res = MessageBox.Show(this, Strings.AutomaticTranslation, Strings.WarningCaption, MessageBoxButtons.YesNoCancel);
			if (res == DialogResult.Cancel)
				return;

			Logger logWriter = new Logger(MyProgressBar, MyStatusBar, TxtOutput);
			PerformActionOnStrings(string.Empty, SelectedNode, logWriter, new PerformActionOnString(TranslateFromKnowledge), res == DialogResult.Yes);
		}

		//---------------------------------------------------------------------
		private void MiTranslateJson_Click(object sender, System.EventArgs e)
		{
			DialogResult res = MessageBox.Show(this, Strings.AutomaticJsonTranslation, Strings.WarningCaption, MessageBoxButtons.YesNoCancel);
			if (res == DialogResult.Cancel)
				return;

			Logger logWriter = new Logger(MyProgressBar, MyStatusBar, TxtOutput);
			PerformActionOnStrings(string.Empty, SelectedNode, logWriter, new PerformActionOnString(TranslateJson), res == DialogResult.Yes);
		}
		//--------------------------------------------------------------------------------
		private void MiAutoTranslate_Click(object sender, System.EventArgs e)
		{
			DialogResult res = MessageBox.Show(this, Strings.CopyTranslation, Strings.WarningCaption, MessageBoxButtons.YesNo);
			if (res == DialogResult.No)
				return;
			Logger logWriter = new Logger(MyProgressBar, MyStatusBar, TxtOutput);
			PerformActionOnStrings(string.Empty, SelectedNode, logWriter, new PerformActionOnString(CopyBaseToTarget));
		}

		//--------------------------------------------------------------------------------
		private void MiTranslateFromLanguage_Click(object sender, System.EventArgs e)
		{
			ChooseLanguage languageDialog = new ChooseLanguage();
			languageDialog.FormTitle = Strings.ChooseLanguage;

			languageDialog.FillCombo((LocalizerTreeNode[])GetDictionaryNodes().ToArray(typeof(LocalizerTreeNode)));
			if (languageDialog.ShowDialog(this) != DialogResult.OK)
				return;

			DialogResult res = MessageBox.Show(this, Strings.CopyTranslation, Strings.WarningCaption, MessageBoxButtons.YesNo);
			if (res == DialogResult.No)
				return;

			Cursor = Cursors.WaitCursor;
			string languagetocopy = languageDialog.ChoosedLanguage.Name;

			Logger logWriter = new Logger(MyProgressBar, MyStatusBar, TxtOutput);
			PerformActionOnStrings(string.Empty, SelectedNode, logWriter, new PerformActionOnString(CopyLanguageToTarget), languagetocopy, logWriter);

		}

		//--------------------------------------------------------------------------------
		private void MiViewGlossary_Click(object sender, System.EventArgs e)
		{
			ViewGlossay();

		}

		//--------------------------------------------------------------------------------
		public void ViewGlossay()
		{
			ChooseLanguage languagesDialog = new ChooseLanguage();
			languagesDialog.FillCombo(String.Empty);
			languagesDialog.FormTitle = Strings.LanguageCaption;
			if (languagesDialog.ShowDialog(this) != DialogResult.OK)
				return;
			Glossary g = new Glossary(languagesDialog.ChoosedLanguage.Name, new SupportInfo(SupportLanguage, SupportView));
			g.ShowDialog();
		}

		//--------------------------------------------------------------------------------
		private void MiCollapse_Click(object sender, System.EventArgs e)
		{
			foreach (TreeNode n1 in ProjectsTreeView.Nodes)
				foreach (TreeNode n2 in n1.Nodes)
					n2.Collapse();
		}

		/// <summary>
		/// Scrive il messaggio di errore e salva il file di log.
		/// </summary>
		/// <param name="message">messaggio </param>
		/// <param name="dictionaryPath">path del dizionario in creazione</param>
		//-----------------------------------------------------------------
		internal static void WriteErrorAndSaveLog(string message, string dictionaryPath, string dictionaryName, Logger logWriter)
		{
			logWriter.WriteLog(message, TypeOfMessage.error);
			WriteFinalLog(dictionaryPath, logWriter);
			logWriter.SaveLog();
		}

		/// <summary>
		/// Scrive il messaggio finale.
		/// </summary>
		/// <param name="dictionaryPath">path del dizionario in creazione</param>
		//-----------------------------------------------------------------
		internal static void WriteFinalLog(string dictionaryPath, Logger logWriter)
		{
			string dictionaryName = CommonFunctions.GetCulture(dictionaryPath);
			string message = (logWriter.HasOnlyInfo) ? Strings.Successfully : Strings.WithError;
			string outputMessage =
				(Directory.Exists(dictionaryPath))
				?
				String.Format(Strings.CreationCompleted, dictionaryName, message)
				:
				String.Format(Strings.StringNotFound, dictionaryName);

			outputMessage += "\r\n\r\n";
			logWriter.WriteLog(outputMessage, TypeOfMessage.state);
		}

		//---------------------------------------------------------------------
		private void MiTempAsTrans_Click(object sender, System.EventArgs e)
		{
			CountTemporaryAsTranslated = !CountTemporaryAsTranslated;
			UpdateDetailsAsync();
		}

		//--------------------------------------------------------------------------------
		private void MiEnvironmentSettings_Click(object sender, System.EventArgs e)
		{
			if (tblslnWriter == null) return;
			string oldInstallation = SolutionDocument.LocalInfo.EnvironmentSettings.InstallationPath;

			EnvironmentSettingsForm f =
				new EnvironmentSettingsForm(SolutionDocument.LocalInfo.EnvironmentSettings, dictionaries);

			if (f.ShowDialog() != DialogResult.OK)
				return;

			if (tblslnWriter.FileName == null || tblslnWriter.FileName.Length == 0)
			{
				SolutionDocument.LocalInfo.Save();
				return;
			}

			SourceControlManager.CompareExecutablePath = SolutionDocument.LocalInfo.EnvironmentSettings.CompareExecutablePath;

			bool modified =
				(oldInstallation != SolutionDocument.LocalInfo.EnvironmentSettings.InstallationPath);

			if (!modified || MessageBox.Show(this, Strings.ReopenSolution, Strings.MainFormCaption, MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;

			OpenExistingSolution(tblslnWriter.FileName, true);

		}

		//--------------------------------------------------------------------------------
		private void MiSetGlossaryFolder_Click(object sender, System.EventArgs e)
		{
			string path = SolutionDocument.LocalInfo.GlossariesFolder;
			FolderBrowserDialog folderDialog = new FolderBrowserDialog();
			folderDialog.Description = String.Format(Strings.ChooseFolderCaption, path);
			folderDialog.ShowNewFolderButton = false;
			folderDialog.SelectedPath = path;
			DialogResult result = folderDialog.ShowDialog(this);
			if (result != DialogResult.OK)
				return;
			SolutionDocument.LocalInfo.GlossariesFolder = folderDialog.SelectedPath;
		}

		/// <summary>
		///Aggiunge le sottocartelle al treeview.
		/// </summary>
		//---------------------------------------------------------------------
		private void ProjectsTreeView_AfterExpand(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if (e.Node.ImageIndex == (int)Images.FOLDERCLOSED || e.Node.ImageIndex == -1)
			{
				e.Node.ImageIndex = (int)Images.FOLDEROPENED;
				e.Node.SelectedImageIndex = (int)Images.FOLDEROPENED;
			}
		}

		//--------------------------------------------------------------------------------
		private void CalculateChildNodes(LocalizerTreeNode node)
		{
			try
			{
				Cursor = Cursors.WaitCursor;
				CalculateChildNodes(node, true);
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		//--------------------------------------------------------------------------------
		public void CalculateChildNodes(LocalizerTreeNode aNode, bool recursive)
		{
			if (aNode == null) return;

			if (aNode.Type == NodeType.PROJECT)
				AddReferenceFolder(aNode);
			else if (aNode.Type == NodeType.LANGUAGE)
				ReadDictionaryContent((DictionaryTreeNode)aNode);

			if (recursive)
				foreach (LocalizerTreeNode child in aNode.Nodes)
					CalculateChildNodes(child, true);
		}

		//--------------------------------------------------------------------------------
		private void MiTbLoader_Click(object sender, System.EventArgs e)
		{
			try
			{
				Cursor = Cursors.WaitCursor;

				if (windowPointCapturer != null)
				{
					windowPointCapturer.Activate();
					return;
				}

				Point startLocation = (windowPointCapturer == null)
					? new Point(Width / 2, Height / 2)
					: windowPointCapturer.Location;

				windowPointCapturer = new WindowPointCapturer(this);
				windowPointCapturer.Location = startLocation;
				windowPointCapturer.Closed += new EventHandler(WindowPointCapturer_Closed);
				windowPointCapturer.Show();

				if (Parent != null)
					Parent.SendToBack();

			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}



		//--------------------------------------------------------------------------------
		private void WindowPointCapturer_Closed(object sender, EventArgs e)
		{
			windowPointCapturer = null;
			if (Parent != null)
				Parent.BringToFront();

		}

		//--------------------------------------------------------------------------------
		private void PanelStartPage_Enter(object sender, System.EventArgs e)
		{
			LnkNew.Select();
		}

		//--------------------------------------------------------------------------------
		private void DictionaryCreator_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			// Assign the file names to a string array, in 
			// case the user has selected multiple files.
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			try
			{
				OpenSolution(files[0]);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return;
			}
		}

		//--------------------------------------------------------------------------------
		private void DictionaryCreator_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (Path.GetExtension(files[0]) == AllStrings.slnExtension)
					e.Effect = DragDropEffects.Copy;
				else
					e.Effect = DragDropEffects.None;
			}
			else
				e.Effect = DragDropEffects.None;
		}

		//---------------------------------------------------------------------
		//per queste tipologie il file contiene una sola risorsa
		public bool IsFileAsLastChild(string filePath)
		{
			return !CommonFunctions.IsXML(filePath);
		}

		//--------------------------------------------------------------------------------
		private void MiChooseLanguage_Click(object sender, System.EventArgs e)
		{
			ChooseLanguage languagesDialog = new ChooseLanguage();
			languagesDialog.FillCombo((LocalizerTreeNode[])GetDictionaryNodes().ToArray(typeof(LocalizerTreeNode)));
			languagesDialog.FormTitle = Strings.LanguageCaption;
			if (languagesDialog.ShowDialog(this) == DialogResult.OK)
				ExportDictionaries(languagesDialog.ChoosedLanguage.Name);
		}

		//--------------------------------------------------------------------------------
		private void MiAllLanguage_Click(object sender, System.EventArgs e)
		{
			ExportDictionaries(null);
		}

		//---------------------------------------------------------------------
		private void ExportDictionaries(string language)
		{
			ArrayList projectList = MakeProjectList(false, false);
			ArrayList dirs = new ArrayList();
			bool ok = true;
			bool zipped = false;
			string zipfullname = String.Empty;
			string commonPath = null;
			foreach (LocalizerTreeNode n in projectList)
			{
				string[] dicPaths = null;
				if (language != null && language.Length > 0)
				{
					dicPaths = new string[] { tblslnWriter.DictionaryPathFinder.GetDictionaryFolder(n, language, false) };
				}
				else
				{
					dicPaths = tblslnWriter.DictionaryPathFinder.GetDictionaryFolders(n);
				}
				foreach (string dicPath in dicPaths)
				{
					if (!Directory.Exists(dicPath))
						continue;

					string lowerDicPath = dicPath.ToLower();//per evitare problemi di case
					commonPath = PathFunctions.GetCommonPath(commonPath, lowerDicPath);
					dirs.Add(lowerDicPath);
				}
			}
			if (dirs.Count > 0)
			{
				zipped = true;
				string filename = Path.GetFileNameWithoutExtension(Solution);
				string id = AllStrings.ZippedDictionaryStandard +
					((language != null && language.Length > 0)
					? ("_" + language)
					: String.Empty);
				filename = filename + id + AllStrings.zipExtension;
				zipfullname = Path.Combine(Path.GetDirectoryName(Solution), filename);
				using (AsyncZipDictionaryManager zm = new AsyncZipDictionaryManager(zipfullname, new Logger(MyProgressBar, MyStatusBar, TxtOutput), true))
				{
					bool wasEnabled = Enabled;
					try
					{
						Enabled = false;
						zm.AddRootInfoToZip(commonPath);
						zm.ZipDictionary((string[])dirs.ToArray(typeof(string)), true, commonPath);
						ok = zm.WaitResult() && ok;
					}
					finally
					{
						Enabled = wasEnabled;
					}
				}
			}

			if (zipped && ok)
				MessageBox.Show(Strings.ProcedureSuccessfully + Environment.NewLine + String.Format(Strings.ZipLocation, Path.GetDirectoryName(zipfullname)));
			else
				MessageBox.Show(Strings.Failed);
		}

		//--------------------------------------------------------------------------------
		private void MiUnzipDictionary_Click(object sender, System.EventArgs e)
		{
			SelectZip();

		}

		//---------------------------------------------------------------------
		private void SelectZip()
		{
			OpenFileDialog openDialog = new OpenFileDialog();
			openDialog.InitialDirectory = Path.GetDirectoryName(tblslnWriter.FileName);
			openDialog.Filter = AllStrings.FILTERZIP;
			openDialog.Title = Strings.SelectZip;
			if (openDialog.ShowDialog(this) == DialogResult.OK)
				UnzipDictionary(openDialog.FileName);
		}

		//---------------------------------------------------------------------
		private void UnzipDictionary(string zipFile)
		{
			using (AsyncZipDictionaryManager zm = new AsyncZipDictionaryManager(zipFile, new Logger(MyProgressBar, MyStatusBar, TxtOutput), false))
			{
				string baseDir = zm.GetRootInfoFromZip();

				if (baseDir == null)
				{
					baseDir = Path.GetDirectoryName(zipFile);

					DialogResult res = MessageBox.Show(String.Format(Strings.UnzippingLocationConfirm, baseDir), Strings.Unzipping, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (res == DialogResult.No)
					{
						FolderBrowserDialog folderDialog = new FolderBrowserDialog();
						folderDialog.Description = Strings.UnzippingLocation;
						folderDialog.ShowNewFolderButton = false;
						folderDialog.SelectedPath = baseDir;
						DialogResult result = folderDialog.ShowDialog(this);
						if (result != DialogResult.OK)
							return;
						baseDir = folderDialog.SelectedPath;
					}
					if (res == DialogResult.Cancel)
						return;
				}

				bool wasEnabled = Enabled;
				bool ok = false;
				try
				{
					Enabled = false;
					zm.UnzipFile(baseDir);
					ok = zm.WaitResult();
				}
				finally
				{
					Enabled = wasEnabled;
				}

				RefreshTree();

				if (ok)
					MessageBox.Show(Strings.ProcedureSuccessfully, Strings.Unzipping);
				else
					MessageBox.Show(Strings.Failed, Strings.Unzipping);
			}

		}

		//---------------------------------------------------------------------
		private void Application_ApplicationExit(object sender, EventArgs e)
		{
			DemoDialog.FreeStringLoaderResources();
		}

		//---------------------------------------------------------------------
		private void MiDictionaryViewer_Click(object sender, System.EventArgs e)
		{
			DictionaryViewer f = new DictionaryViewer();
			f.Owner = this.Owner;
			f.Show();
		}

		//---------------------------------------------------------------------
		private void PanelStartPage_SizeChanged(object sender, System.EventArgs e)
		{
			DgStart.Top = 60;
			DgStart.Left = (Width - DgStart.Width) / 2;

			PbStart.Left = DgStart.Left;
			PanelButtons.Left = DgStart.Right - PanelButtons.Width;
		}

		//---------------------------------------------------------------------
		private void OnClearClick(object sender, System.EventArgs e)
		{
			TxtOutput.Text = string.Empty;
		}

		//---------------------------------------------------------------------
		private void OnCopyClick(object sender, System.EventArgs e)
		{
			Clipboard.SetDataObject(TxtOutput.Text, true);
		}

		//---------------------------------------------------------------------
		private void OnSaveClick(object sender, System.EventArgs e)
		{
			SaveFileDialog saveFile = new SaveFileDialog();
			saveFile.Filter = "Text Documents (*.txt)|*.txt|All files|*.*";
			saveFile.RestoreDirectory = true;

			if (saveFile.ShowDialog(this) == DialogResult.OK)
			{
				try
				{
					Stream saveStream;

					if ((saveStream = saveFile.OpenFile()) != null)
					{
						StringReader sr = new StringReader(TxtOutput.Text);
						BinaryWriter bw = new BinaryWriter(saveStream);

						char[] buffer = new char[TxtOutput.Text.Length];
						sr.Read(buffer, 0, buffer.Length);
						foreach (char c in buffer)
							bw.Write(Convert.ToByte(c));

						bw.Close();
						sr.Close();
						saveStream.Close();
					}
				}
				catch (Exception exc)
				{
					Debug.Fail(exc.Message);
				}
			}
		}

		//---------------------------------------------------------------------
		private void MiGetLatest_Click(object sender, EventArgs e)
		{
			SourceControlManager.GetLatestVersion(SelectedNode);
			SourceControlManager.WriteReadyMessage();
		}

		//---------------------------------------------------------------------
		private void MiUndoCheckOut_Click(object sender, EventArgs e)
		{
			string[] files = SelectedNode.GetNodeFiles();
			SourceControlManager.UndoCheckOut(files);
			foreach (string file in files)
				CommonFunctions.RefreshSourceControlStatus(file);
			SourceControlManager.WriteReadyMessage();
		}

		//---------------------------------------------------------------------
		private void MiCheckOut_Click(object sender, EventArgs e)
		{
			string[] files = SelectedNode.GetNodeFiles();
			SourceControlManager.CheckOut(files);
			foreach (string file in files)
				CommonFunctions.RefreshSourceControlStatus(file);
			SourceControlManager.WriteReadyMessage();
		}

		//---------------------------------------------------------------------
		private void MiCheckIn_Click(object sender, EventArgs e)
		{
			string[] files = SelectedNode.GetNodeFiles();
			SourceControlManager.CheckIn(files);
			foreach (string file in files)
				CommonFunctions.RefreshSourceControlStatus(file);
			SourceControlManager.WriteReadyMessage();
		}

		private void MiImportHTML_Click(object sender, EventArgs e)
		{
			ImportExport impexp = new ImportExport(new Logger(MyProgressBar, MyStatusBar, TxtOutput));
			impexp.ImportHTML();

		}

		private void MiExportCSVTranslation_Click(object sender, EventArgs e)
		{
			string culture = GetOneLanguageFromSelectedNode();
			if (culture == null)
				return;

			bool notTranslatedOnly = (MenuItem)sender == MiCSVNotTranslatedStrings;
			bool translatedOnly = (MenuItem)sender == MiCSVTranslatedStrings;

			ImportExport impexp = new ImportExport(new Logger(MyProgressBar, MyStatusBar, TxtOutput));
			impexp.ExportToCSV(
				culture,
				translatedOnly || !notTranslatedOnly,
				notTranslatedOnly || !translatedOnly
				);

		}


	}
}
