using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Interfaces;
using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;
using Microarea.Console.Plugin.RowSecurityToolKit.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Console.Plugin.RowSecurityToolKit
{
	///<summary>
	/// PlugIn di amministrazione per il Row Data Security
	/// Si tratta di un toolkit a disposizione dei programmatori per creare/modificare
	/// i file di configurazione RowSecurityObjects.xml, utilizzati poi in Mago
	///</summary>
	//=========================================================================
	public class RowSecurityLayer : PlugIn
	{
		# region Private data-members
		private const string rowSecurityLayerAdministratorType = "RowSecurityLayerAdministrator";
		private static readonly string rowSecurityLayerAdminPlugIn = Assembly.GetExecutingAssembly().GetName().Name;

		private MenuStrip consoleMenu = null;
		private PlugInsTreeView consoleTreeView = null;
		private Panel workingAreaConsole;
		private Panel workingAreaConsoleBottom;

		//info di ambiente di console
		private ConsoleEnvironmentInfo consoleEnvironmentInfo;
		//info relative alla licenza dell'installazione
		private LicenceInfo licenceInfo;
		private BrandLoader brandLoader;

		private ContextInfo contextInfo = null;
		private PathFinder pathFinder = null;

		private string brandAppName = string.Empty;
		# endregion

		# region Events and Delegates
		// Il SysAdmin mi ritorna l'elenco delle aziende in MSD_Companies
		public delegate SqlDataReader GetCompanies();
		public event GetCompanies OnGetCompanies;

		// La console ritorna lo stato
		public delegate StatusType GetConsoleStatus();
		public event GetConsoleStatus OnGetConsoleStatus;

		// L'ApplicationDBAdmin ritorna lo stato del database dell'azienda prescelta
		public delegate DatabaseStatus CheckCompanyDBForRSLDelegate(string companyId);
		public event CheckCompanyDBForRSLDelegate CheckCompanyDBForRSL;
		# endregion
		
		///<summary>
		/// Constructor
		///</summary>
		//---------------------------------------------------------------------
		public RowSecurityLayer()
		{
		}

		# region Funzioni di inizializzazione
		/// <summary>
		/// Load ci deve essere sempre perchè la usa la console quando la tira su.
		/// Può anche essere vuota, l'importante è che valorizzi dentro a 3 variabili
		/// locali: il menù, il Tree e la Working Area in modo da poterle utilizzare
		/// anche in un secondo tempo
		/// </summary>
		//---------------------------------------------------------------------
		public override void Load(ConsoleGUIObjects consoleGUIObjects, ConsoleEnvironmentInfo consoleEnvironmentInfo, LicenceInfo licenceInfo)
		{
			consoleMenu = consoleGUIObjects.MenuConsole;
			consoleTreeView = consoleGUIObjects.TreeConsole;
			workingAreaConsole = consoleGUIObjects.WkgAreaConsole;
			workingAreaConsoleBottom = consoleGUIObjects.BottomWkgAreaConsole;

			this.consoleEnvironmentInfo = consoleEnvironmentInfo;
			this.licenceInfo = licenceInfo;
		}
		
		// Inizializzazione PathFinder della Console
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnInitPathFinder")]
		public void OnAfterInitPathFinder(PathFinder pathFinder)
		{
			this.pathFinder = pathFinder;

			contextInfo = new ContextInfo(pathFinder, licenceInfo.DBNetworkType, licenceInfo.IsoState);
			contextInfo.OnAddUserAuthenticatedFromConsole += new ContextInfo.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			contextInfo.OnGetUserAuthenticatedPwdFromConsole += new ContextInfo.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);
			contextInfo.OnIsUserAuthenticatedFromConsole += new ContextInfo.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
		}

		/// <summary>
		/// la Console mi passa il BrandLoader inizializzato correttamente
		/// </summary>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnInitBrandLoader")]
		//-------------------------------------------------------------------
		public void OnInitBrandLoader(BrandLoader aBrandLoader)
		{
			brandLoader = aBrandLoader;
		}
		# endregion

		# region OnAfterLogOn
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterLogOn")]
		public void OnAfterLogOn(object sender, DynamicEventsArgs e)
		{
			//utilizzo la classe di info ConsoleEnvironment
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName = e.Get("DbDefaultUser").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd = e.Get("DbDefaultPassword").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth = Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity"));
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName =
				string.IsNullOrEmpty(e.Get("DbServerIstance").ToString())
				? e.Get("DbServer").ToString()
				: Path.Combine(e.Get("DbServer").ToString(), e.Get("DbServerIstance").ToString());

			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName = e.Get("DbDataSource").ToString();

			if (OnGetConsoleStatus != null)
				this.consoleEnvironmentInfo.ConsoleStatus = OnGetConsoleStatus();

			//Valorizzo la struttura che conterrà i parametri che mi ha passato il SysAdmin per la connessione.
			contextInfo.SysDBConnectionInfo.DBName = e.Get("DbDataSource").ToString();
			contextInfo.SysDBConnectionInfo.ServerName = e.Get("DbServer").ToString();
			contextInfo.SysDBConnectionInfo.UserId = e.Get("DbDefaultUser").ToString();
			contextInfo.SysDBConnectionInfo.Password = e.Get("DbDefaultPassword").ToString();
			contextInfo.SysDBConnectionInfo.Instance = e.Get("DbServerIstance").ToString();

			UpdateConsoleTree(consoleTreeView);
		}
		# endregion

		# region OnAfterLogOff
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterLogOff")]
		public void OnAfterLogOff(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();

			TreeNodeCollection nodeCollection = consoleTreeView.Nodes[0].Nodes;
			for (int i = 0; i < nodeCollection.Count; i++)
			{
				if (((PlugInTreeNode)nodeCollection[i]).AssemblyName == rowSecurityLayerAdminPlugIn)
				{
					nodeCollection[i].Remove();
					i = i - 1;
				}
			}

			workingAreaConsoleBottom.Visible = false;

			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth = false;
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName = string.Empty;
		}
		# endregion

		#region ShutDownFromPlugIn
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnShutDownConsole")]
		public override bool ShutDownFromPlugIn()
		{
			//Aggiungere qui tutto ciò che il plugIn deve fare prima che la console venga chiusa
			return base.ShutDownFromPlugIn();
		}
		#endregion

		#region Creazione nodo del PlugIn Row Security Layer
		//---------------------------------------------------------------------
		public void UpdateConsoleTree(TreeView treeConsole)
		{
			Assembly myAss = Assembly.GetExecutingAssembly();

			Image rootImage = null;
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Plugin.RowSecurityToolKit.Images.Shield.png");
			if (imageStream != null)
				rootImage = Image.FromStream(imageStream);
			int indexIcon = (rootImage != null) ? consoleTreeView.ImageList.Images.Add(rootImage, Color.Magenta) : -1;

			// Root
			PlugInTreeNode lastNodeTree = (PlugInTreeNode)treeConsole.Nodes[treeConsole.Nodes.Count - 1];

			PlugInTreeNode rootPlugInNode = new PlugInTreeNode(Strings.PlugInTitle);
			rootPlugInNode.AssemblyName = rowSecurityLayerAdminPlugIn;
			rootPlugInNode.AssemblyType = typeof(RowSecurityLayer);
			rootPlugInNode.ImageIndex = indexIcon;
			rootPlugInNode.SelectedImageIndex = indexIcon;
			rootPlugInNode.Type = rowSecurityLayerAdministratorType;
			rootPlugInNode.ToolTipText = Strings.PlugInTitle;

			lastNodeTree.Nodes.Add(rootPlugInNode);
		}
		# endregion

		# region Eventi sul click del nodo
		/// <summary>
		/// Click sul nodo
		/// </summary>
		//---------------------------------------------------------------------
		public void OnAfterSelectConsoleTree(object sender, System.EventArgs e)
		{
			LoadRSStartForm(sender);
		}

		/// <summary>
		/// Double-click sul nodo
		/// </summary>
		//---------------------------------------------------------------------------	
		public void OnAfterDoubleClickConsoleTree(object sender, System.EventArgs e)
		{
			LoadRSStartForm(sender);
		}

		/// <summary>
		/// carico la StartForm con le opzioni per la gestione delle entita'
		/// </summary>
		//---------------------------------------------------------------------------	
		private void LoadRSStartForm(object sender)
		{
			if (workingAreaConsole == null)
				return;

			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTreeView.SelectedNode;

			if (selectedNode.Type == rowSecurityLayerAdministratorType)
			{
				workingAreaConsole.Controls.Clear();
				workingAreaConsoleBottom.Enabled = false;
				workingAreaConsoleBottom.Visible = false;

				RSStartForm erForm = new RSStartForm(pathFinder, brandLoader, contextInfo, licenceInfo, consoleTreeView.StateImageList, consoleTreeView.ImageList);
				erForm.OnGetCompanies += new RSStartForm.GetCompanies(rsl_GetCompanies);
				erForm.CheckCompanyDBForRSL += new RSStartForm.CheckCompanyDBForRSLDelegate(rsl_CheckCompanyDBForRSL);

				erForm.TopLevel = false;
				erForm.Dock = DockStyle.Fill;

				//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
				OnBeforeAddFormFromPlugIn(sender, erForm.ClientSize.Width, erForm.ClientSize.Height);

				workingAreaConsole.Controls.Add(erForm);
				erForm.Enabled = true;
				erForm.Show();
			}
		}
		# endregion

		# region Rimpallo eventi
		/// <summary>
		/// Evento al SysAdmin per avere l'elenco delle aziende amministrate nella MSD_Companies
		/// </summary>
		//---------------------------------------------------------------------------	
		public SqlDataReader rsl_GetCompanies()
		{
			if (OnGetCompanies != null)
				return OnGetCompanies();

			return null;
		}

		///<summary>
		/// Evento all'ApplicationDBAdmin per il check del database aziendale
		///</summary>
		//---------------------------------------------------------------------
		private DatabaseStatus rsl_CheckCompanyDBForRSL(string companyId)
		{
			DatabaseStatus dbStatus = DatabaseStatus.EMPTY;

			if (CheckCompanyDBForRSL != null)
				dbStatus = CheckCompanyDBForRSL(companyId);

			return dbStatus;
		}
		# endregion
	}
}
