using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.AuditingAdmin
{
	//=========================================================================
	public class AuditingAdmin: PlugIn
	{
		# region Data Member
        private MenuStrip			consoleMenu;
        private PlugInsTreeView     consoleTree;
		private Panel				workingAreaConsole;
		private Panel				workingAreaConsoleBottom;
		private ApplicationsTree	applicationsTree;
		
		private ContextInfo	contextInfo	= null;
		private CatalogInfo	catalogInfo = null;
		private BrandLoader	brandLoader = null;
		private LicenceInfo	licenceInfo = null;
		
		private StringCollection	companiesIdAdmitted = null;
		private Diagnostic			auditDiagnostic = new Diagnostic(AuditConstStrings.AuditingAdminPlugIn);

		private bool	isStandardEdition	= false;
		private string	sConn				= string.Empty;

		public	bool		isRunningFormServer	= true;
		public  string		Configuration		= string.Empty;
		public	ArrayList	AddOnAppList		= null;
		# endregion

		#region Eventi per i pulsanti della ToolBar del SysAdmin
		//Disabilito il pulsante Salva ----------------------------------------
		public event System.EventHandler OnDisableSaveToolBarButton;
		//Disabilito il pulsante New ------------------------------------------
		public event System.EventHandler OnDisableNewToolBarButton;
		//Disabilito il pulsante Open -----------------------------------------
		public event System.EventHandler OnDisableOpenToolBarButton;
		//Disabilito il pulsante Delete ---------------------------------------
		public event System.EventHandler OnDisableDeleteToolBarButton;		
		#endregion

		#region Eventi per i pulsanti della ToolBar della Security
		//Disabilita il bottone degli OtherObject Reference e DataBase ---------
		public event System.EventHandler OnDisableOtherObjectsToolBarButton;
		//Disabilito il pulsante xhe mostra il Tree con le icone di OSL -------
		public event System.EventHandler OnDisableShowSecurityIconsToolBarButton;
		//Disabilito il pulsante filtro di OSL --------------------------------
		public event System.EventHandler OnDisableApplySecurityFilterToolBarButton;

        public event System.EventHandler OnDisableFindSecurityObjectsToolBarButton;
        public event System.EventHandler OnDisableShowAllGrantsToolBarButtonPushed;
		#endregion

		public delegate void LoadMatViewsFormDelegate(string companyId, bool fromAuditingPlugIn);
		public event LoadMatViewsFormDelegate LoadMatViewsForm;

		//---------------------------------------------------------------------
		public AuditingAdmin() { }

		#region Funzioni di Inizializzazione
		//---------------------------------------------------------------------
		public  override void Load
			(
				ConsoleGUIObjects		consoleGUIObjects,
				ConsoleEnvironmentInfo	consoleEnvironmentInfo,
				LicenceInfo				licenceInfo
			)
		{
			consoleMenu					= consoleGUIObjects.MenuConsole; 
			consoleTree					= consoleGUIObjects.TreeConsole; 
			workingAreaConsole			= consoleGUIObjects.WkgAreaConsole; 
			workingAreaConsoleBottom	= consoleGUIObjects.BottomWkgAreaConsole; 
			isRunningFormServer			= consoleEnvironmentInfo.RunningFromServer;
			this.licenceInfo			= licenceInfo;
			isStandardEdition = 
				(string.Compare(licenceInfo.Edition, NameSolverStrings.StandardEdition, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		# region OnInitPathFinder - riceve il PathFinder inizializzato
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnInitPathFinder")]
		//---------------------------------------------------------------------
		public void OnAfterInitPathFinder(PathFinder pathFinder)
		{
			contextInfo = new ContextInfo(pathFinder, licenceInfo.DBNetworkType, licenceInfo.IsoState);
			
			contextInfo.OnAddUserAuthenticatedFromConsole += new ContextInfo.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			contextInfo.OnGetUserAuthenticatedPwdFromConsole += new ContextInfo.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			contextInfo.OnIsUserAuthenticatedFromConsole += new ContextInfo.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
		}
		# endregion

		# region OnInitBrandLoader - riceve il BrandLoader inizializzato
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnInitBrandLoader")]
		//-------------------------------------------------------------------
		public void OnInitBrandLoader(BrandLoader aBrandLoader)
		{
			brandLoader = aBrandLoader;
		}
		# endregion

		#endregion

		#region Eventi del SysAdmin
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterChangedCompanyAuditing")]
		//---------------------------------------------------------------------
		public void OnAfterChangedCompanyAuditingFlag(object sender, string companyId, bool disabled)
		{
			// Modifica del Flag x l'Auditing
			InsertTreeCompany();
		}
		
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterSavedCompany")]
		//---------------------------------------------------------------------
		public void OnAfterConsoleSaveNewCompany(object sender, string e)
		{
			// Salva Nuova /modifica azienda
			InsertTreeCompany();
		}

		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnDeleteCompanyFromSysAdmin")]
		//---------------------------------------------------------------------
		public void OnAfterConsoleDeleteCompany(object sender, string e)
		{
			PlugInTreeNode node = FindNodeTipology(consoleTree.Nodes, AuditConstStrings.AuditingAdminPlugIn, AuditConstStrings.CompanyContainer, AuditConstStrings.All);
			// eliminazione azienda
			PlugInTreeNode selNode;
			int position = -1;
			for (int i = 0; i < node.Nodes.Count; i++)
			{
				selNode = node.Nodes[i] as PlugInTreeNode;
				if (selNode != null && e == selNode.Id)
				{
					position = i;
					break;
				}
			}
			bool isExpanded = node.IsExpanded;
			if (node != null)
			{
				if (position != -1)
					node.Nodes.RemoveAt(position);
				if (isExpanded)
					node.Expand();
				this.consoleTree.Focus();
			}
		}

		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterClonedCompany")]
		//---------------------------------------------------------------------
		public void OnAfterConsoleClonedCompany(string companyId)
		{
			// clonazione
			InsertTreeCompany();
		}
		#endregion

		#region Eventi della Console

		#region Eventi di Connessione
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterLogOn")]
		//---------------------------------------------------------------------
		public void OnAfterLogOn(object sender, DynamicEventsArgs e)
		{
			// Valorizzo la struttura che conterrà i parametri che mi ha passato il SysAdmin per la connessione.
			contextInfo.SysDBConnectionInfo.DBName		= e.Get("DbDataSource").ToString();
			contextInfo.SysDBConnectionInfo.ServerName	= e.Get("DbServer").ToString();
			contextInfo.SysDBConnectionInfo.UserId		= e.Get("DbDefaultUser").ToString();
			contextInfo.SysDBConnectionInfo.Password	= e.Get("DbDefaultPassword").ToString();
			contextInfo.SysDBConnectionInfo.Instance	= e.Get("DbServerIstance").ToString();

			// se l'Edition è Standard, allora controllo quali aziende ha caricato il SysAdmin nel suo tree
			// ed carico nel tree dell'Auditing solo quelle...
			if (isStandardEdition)
				companiesIdAdmitted = ((StringCollection)(e.Get("CompaniesIdAdmitted")));

			UpdateConsoleTree(consoleTree);
		}

		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterLogOff")]
		//---------------------------------------------------------------------
		public void OnAfterLogOff(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			TreeNodeCollection nodeCollection = consoleTree.Nodes[0].Nodes;
            for (int i = 0; i < nodeCollection.Count; i++)
			{
                PlugInTreeNode aNode = nodeCollection[i] as PlugInTreeNode;
                if (aNode == null)
                    continue;
                if (aNode.AssemblyName == AuditConstStrings.AuditingAdminPlugIn)
				{
                    aNode.Remove();	
					i = i-1;
				}
			}
			workingAreaConsoleBottom.Visible = false;

			if (OnDisableSaveToolBarButton != null) OnDisableSaveToolBarButton(sender, e);
		}
		#endregion

		#region Eventi di Chiusura
		/// <summary>
		/// Aggiungere qui tutto ciò che il plugIn deve fare prima che la console venga chiusa
		/// </summary>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnShutDownConsole")]
		//---------------------------------------------------------------------
		public override bool ShutDownFromPlugIn()
		{
			// se non ho controls nella working area o il db e' uguale a SQL non procedo
			if (workingAreaConsole.Controls.Count == 0 || workingAreaConsole.Controls[0] == null || contextInfo.DbType == DBMSType.SQLSERVER)
				return base.ShutDownFromPlugIn();

			if (workingAreaConsole.Controls[0] is ApplicationsTree)
			{
				ApplicationsTree appTreeForm = (ApplicationsTree)workingAreaConsole.Controls[0];

				// prima di chiudere la Console controllo se sono state effettuate delle modifiche strutturali
 				// alle tabelle dell'Auditing e richiamo la form delle viste materializzate
				if (appTreeForm != null)
					AskUpdateMatViews();
			}

			return base.ShutDownFromPlugIn();
		}
		#endregion

		#endregion

		#region Creazione / Modifica del Tree della Console
		#region Creazione
		//---------------------------------------------------------------------
		public  void UpdateConsoleTree(TreeView treeConsole)
		{
			Assembly myAss	= Assembly.GetExecutingAssembly();
			Stream myStream = myAss.GetManifestResourceStream(myAss.GetName().Name + ".Images.AuditingPlugIn.bmp");
			
			StreamReader myreader = new StreamReader(myStream);
			int indexIcon = consoleTree.ImageList.Images.Add(Image.FromStream(myStream,true), Color.Magenta );
			
			//Root
            PlugInTreeNode lastNodeTree = (PlugInTreeNode)treeConsole.Nodes[treeConsole.Nodes.Count - 1];

			PlugInTreeNode rootPlugInNode = new PlugInTreeNode(Strings.PlugInTitle);
			rootPlugInNode.AssemblyName = AuditConstStrings.AuditingAdminPlugIn;
			rootPlugInNode.AssemblyType = typeof(AuditingAdmin);
			rootPlugInNode.ImageIndex = indexIcon; 
			rootPlugInNode.SelectedImageIndex = indexIcon; 
			rootPlugInNode.Type = AuditConstStrings.AuditingAdministratorType;
			rootPlugInNode.ToolTipText = Strings.ToolTipString;

			//Azienda
			PlugInTreeNode groupPlugInNode = new PlugInTreeNode(Strings.Companies);
			groupPlugInNode.AssemblyName = AuditConstStrings.AuditingAdminPlugIn;
			groupPlugInNode.AssemblyType = typeof(AuditingAdmin);
			groupPlugInNode.Type = AuditConstStrings.CompanyContainer;
			groupPlugInNode.Checked = false;
		    groupPlugInNode.ImageIndex = PlugInTreeNode.GetCompaniesDefaultImageIndex;
            groupPlugInNode.SelectedImageIndex = PlugInTreeNode.GetCompaniesDefaultImageIndex;
			lastNodeTree.Expand();
			
			lastNodeTree.Nodes.Add(rootPlugInNode);
			rootPlugInNode.Nodes.Add(groupPlugInNode);
			
			LoadCompanies(groupPlugInNode);
		}

		//---------------------------------------------------------------------
		public void LoadCompanies(PlugInTreeNode rootPlugInNode)
		{
			contextInfo.ComposeSystemDBConnectionString();
			if (string.IsNullOrEmpty(contextInfo.ConnectSysDB))
				return;

			// se sono nella Standard Edition allora devo caricare nel tree solo le aziende censite dal SysAdmin (le prime 2)
			string sqlSelect =
				(isStandardEdition)
				? "SELECT CompanyId, Company, IsValid FROM MSD_Companies WHERE UseAuditing = 1 AND Disabled = 0 AND CompanyId = @companyId"
				: "SELECT CompanyId, Company, IsValid FROM MSD_Companies WHERE UseAuditing = 1 AND Disabled = 0";

			TBConnection conn = null;
			TBCommand command = null;
			IDataReader reader = null;

			try
			{
				conn = new TBConnection(DBMSType.SQLSERVER);
				conn.ConnectionString = contextInfo.ConnectSysDB;
				conn.Open();

				command = new TBCommand(sqlSelect, conn);

				// Standard Edition
				if (isStandardEdition)
				{
					command.Parameters.Add("@companyId", ((TBType)SqlDbType.NVarChar));

					foreach (string coId in companiesIdAdmitted)
					{
						((IDbDataParameter)command.Parameters["@companyId"]).Value = coId;
						reader = command.ExecuteReader();
						AddCompanyNode(rootPlugInNode, reader);
					}
				}
				else
				{
					// Professional Edition (carico tutte le aziende)
					reader = command.ExecuteReader();
					AddCompanyNode(rootPlugInNode, reader);
				}
			}
			catch (TBException e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
			}
			finally
			{
				if (reader != null && !reader.IsClosed)
					reader.Close();
				if (command != null)
					command.Dispose();
				if (conn != null && conn.State != ConnectionState.Closed)
				{
					conn.Close();
					conn.Dispose();
				}
			}
		}

		/// <summary>
		/// Iterando sul DataReader passato come parametro aggiungo i nodi al tree delle aziende del PlugIn
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyNode(PlugInTreeNode rootPlugInNode, IDataReader reader)
		{
			while (reader.Read())
			{
				PlugInTreeNode currentNode = new PlugInTreeNode();
                currentNode.SelectedImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
                currentNode.ImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
				currentNode.Text = reader["Company"].ToString();
				currentNode.AssemblyName = AuditConstStrings.AuditingAdminPlugIn;
				currentNode.AssemblyType = typeof(AuditingAdmin);
				currentNode.Type = AuditConstStrings.Company;
				currentNode.Id = reader["CompanyId"].ToString();
				currentNode.IsValid	= Convert.ToBoolean(reader["IsValid"].ToString());
			
				int pos = rootPlugInNode.Nodes.Add(currentNode);

				// se l'azienda non è valida, aggiungo l'icona di stato dell'azienda da migrare
				if (!currentNode.IsValid)
				{
					currentNode.ForeColor = Color.Red;
                    rootPlugInNode.Nodes[pos].StateImageIndex = PlugInTreeNode.GetCompaniesToMigrateImageIndex;
				}
			}
		}
		#endregion

		#region Modifica
		//---------------------------------------------------------------------
		public void InsertTreeCompany()
		{
			PlugInTreeNode node;
			for (int i = 0; i < consoleTree.Nodes[0].Nodes.Count; i++)
			{
				node = (PlugInTreeNode)consoleTree.Nodes[0].Nodes[i];
				if (node.Type == AuditConstStrings.AuditingAdministratorType)
					node.Remove();
			}

			UpdateConsoleTree(consoleTree);
		}

		//---------------------------------------------------------------------
		private PlugInTreeNode FindNodeTipology(TreeNodeCollection nodes, string plugInName, string tipology, string companyId)
		{
			PlugInTreeNode nodeOfTypology = null;						
			for (int i = 0; nodeOfTypology == null; i++)
			{
				if (i >= nodes.Count)
					return nodeOfTypology;

                if ((((PlugInTreeNode)nodes[i]).Type == tipology) && (((PlugInTreeNode)nodes[i]).AssemblyName == plugInName))
				{
					nodeOfTypology = (PlugInTreeNode)nodes[i];
					
					if (companyId != AuditConstStrings.All)
					{
						if (nodeOfTypology.CompanyId == companyId)
							return nodeOfTypology;
						else
						{
							if (nodes[i].Parent.NextNode.Nodes.Count > 0)
							{
								PlugInTreeNode nextNode = (PlugInTreeNode)nodes[i].Parent.NextNode;
								nodeOfTypology = FindNodeTipology(((TreeNode)nextNode).Nodes, plugInName, tipology, companyId);
							}
						}
					}
					else
						return nodeOfTypology;
				}
				else
				{
					if (nodes[i].Nodes.Count > 0)
						nodeOfTypology = FindNodeTipology(nodes[i].Nodes, plugInName, tipology, companyId);	
				}
			}
			return nodeOfTypology;
		}
		#endregion
		#endregion

		#region Eventi sul Tree della Console
		
		// click su un nodo dell'albero
		//---------------------------------------------------------------------
		private void DisableConsoleToolBarBotton(object sender, System.EventArgs e)
		{
			if (OnDisableShowSecurityIconsToolBarButton != null) OnDisableShowSecurityIconsToolBarButton(sender, e);
			if (OnDisableApplySecurityFilterToolBarButton != null) OnDisableApplySecurityFilterToolBarButton(sender, e);
            if (OnDisableShowAllGrantsToolBarButtonPushed!= null) OnDisableShowAllGrantsToolBarButtonPushed(sender, e);
			if (OnDisableOtherObjectsToolBarButton != null) OnDisableOtherObjectsToolBarButton(sender, e);
            if (OnDisableFindSecurityObjectsToolBarButton!= null) OnDisableFindSecurityObjectsToolBarButton(sender, e);
			if (OnDisableDeleteToolBarButton != null) OnDisableDeleteToolBarButton(sender, e);
			if (OnDisableNewToolBarButton != null) OnDisableNewToolBarButton(sender, e);
			if (OnDisableOpenToolBarButton != null)	OnDisableOpenToolBarButton(sender, e);
			if (OnDisableSaveToolBarButton != null)	OnDisableSaveToolBarButton(sender, e);
			workingAreaConsoleBottom.Visible = false;
			workingAreaConsole.Controls.Clear();
		}

		// click su un nodo dell'albero
		//---------------------------------------------------------------------
		public void OnAfterSelectConsoleTree(object sender, System.EventArgs e)
		{
			DisableConsoleToolBarBotton(sender, e);
			
			if (workingAreaConsole != null)
			{
				AuditingInfo auditingInfo = new AuditingInfo();
				auditingInfo.TopLevel = false;
				auditingInfo.Dock = DockStyle.Fill;
				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, auditingInfo.ClientSize.Width, auditingInfo.ClientSize.Height);
				workingAreaConsole.Controls.Add(auditingInfo);
				auditingInfo.Enabled = true;
				auditingInfo.Show();
			}
		}
		
		// DoubleClick: se il nodo è un'azienda allora faccio vedere tutto il tree
		//---------------------------------------------------------------------------	
		public void OnAfterDoubleClickConsoleTree(object sender, System.EventArgs e)
		{
			DisableConsoleToolBarBotton(sender, e);
			EnableProgressBarFromPlugIn(sender);
			SetCyclicStepProgressBarFromPlugIn();
			SetProgressBarTextFromPlugIn(sender, Strings.LoadDatabaseInfo);

			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
			
			if (selectedNode.Type == AuditConstStrings.Company)
			{
				if (workingAreaConsole != null)
				{
					// se la company selezionata non è valida non procedo con il lancio del PlugIn e dò un messaggio
					if (!selectedNode.IsValid)
					{
						DiagnosticViewer.ShowInformation(Strings.NoValidCompany, Strings.Attention);
						SetProgressBarTextFromPlugIn(sender, string.Empty);
						DisableProgressBarFromPlugIn(sender);
						return; 
					}

					Cursor currentConsoleFormCursor = consoleTree.TopLevelControl.Cursor;
					consoleTree.TopLevelControl.Cursor = Cursors.WaitCursor;				

					if (!contextInfo.MakeCompanyConnection(selectedNode.Id.ToString()))
					{
						DiagnosticViewer.ShowDiagnostic(contextInfo.Diagnostic);
						consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
						SetProgressBarTextFromPlugIn(sender, string.Empty); 
						DisableProgressBarFromPlugIn(sender);
						return;
					}

					catalogInfo = new CatalogInfo();
					catalogInfo.Load(contextInfo.Connection, true);
				
					if (catalogInfo.GetTableEntry(AuditTableConsts.AuditTablesTableName) == null)
					{
						auditDiagnostic.SetWarning(Strings.NoTables);
						DiagnosticViewer.ShowDiagnostic(auditDiagnostic);
						consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
						SetProgressBarTextFromPlugIn(sender, string.Empty); 
						DisableProgressBarFromPlugIn(sender);
						return;
					}

					applicationsTree = new ApplicationsTree(contextInfo, catalogInfo, brandLoader);
					applicationsTree.LoadDatabaseInfoStarted += new LoadDatabaseInfoEventHandler(OnLoadDatabaseInfoStarted);
					applicationsTree.LoadDatabaseInfoModChanged	+= new LoadDatabaseInfoEventHandler(OnLoadDatabaseInfoModChanged);
					applicationsTree.LoadDatabaseInfoEnded += new LoadDatabaseInfoEventHandler(OnLoadDatabaseInfoEnded);
					applicationsTree.UpdateMatViews += new ApplicationsTree.UpdateMatViewsDelegate(AskUpdateMatViews);

					applicationsTree.InitDatabaseInfo();

					applicationsTree.TopLevel	= false;
					applicationsTree.Dock		= DockStyle.Fill;
					workingAreaConsole.Height	= 0;

					OnBeforeAddFormFromPlugIn(sender, applicationsTree.Width, applicationsTree.Height);
					workingAreaConsole.Controls.Add(applicationsTree);				
					applicationsTree.Show();

					workingAreaConsole.Visible = true;
					workingAreaConsole.Enabled = true;
					workingAreaConsoleBottom.Visible = false;
					workingAreaConsoleBottom.Enabled = false;				

					consoleTree.TopLevelControl.Cursor = currentConsoleFormCursor;
				}
			}

			SetProgressBarTextFromPlugIn(sender, string.Empty);
			DisableProgressBarFromPlugIn(sender);
		}
		#endregion
	
		#region Metodi per la gestione della progressbar in fase di caricamento
		//---------------------------------------------------------------------
		private void OnLoadDatabaseInfoStarted(object sender, int nCounter)
		{
			EnableProgressBarFromPlugIn(sender);
						
			SetProgressBarMinValueFromPlugIn(this, 0);
			SetProgressBarMaxValueFromPlugIn(this, nCounter);

			SetProgressBarTextFromPlugIn(this, Strings.LoadDatabaseInfo);

			SetProgressBarValueFromPlugIn (this, 0);

			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//---------------------------------------------------------------------
		private void OnLoadDatabaseInfoModChanged(object sender, int nCounter)
		{
			SetProgressBarValueFromPlugIn (this, nCounter);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//---------------------------------------------------------------------
		private void OnLoadDatabaseInfoEnded(object sender, int nCounter)
		{
			SetProgressBarTextFromPlugIn(this, String.Empty);

			DisableProgressBarFromPlugIn(this);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}
		#endregion		

		///<summary>
		/// Metodo richiamato in uscita dalla form principale del plugin Auditing
		/// Per aggiornare le viste materializzate per l'azienda Oracle
		///</summary>
		//---------------------------------------------------------------------
		private void AskUpdateMatViews()
		{
			if (
				applicationsTree.AuditStructureIsChanged && 
				contextInfo.DbType == DBMSType.ORACLE && 
				!string.IsNullOrWhiteSpace(contextInfo.CompanyId)
				)
			{
				// sparo un evento all'ApplicationDBAdmin, che apre la form MatViews se sono presenti delle viste materializzate
				if (LoadMatViewsForm != null)
					LoadMatViewsForm(contextInfo.CompanyId, true); 
			}
		}
	}
}