using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.Console.Core.PlugIns;
using System.Windows.Forms;
using Microarea.Console.Core.EventBuilder;
using System.Reflection;

namespace Microarea.Console.Plugin.HermesAdmin
{
	public class HermesAdminPlugIn : PlugIn
	{
		ConsoleGUIObjects consoleGUIObjects;
		ConsoleEnvironmentInfo consoleEnvironmentInfo;
		LicenceInfo licenceInfo;

		PlugInTreeNode rootPlugInNode;

		public override void Load
			(
			ConsoleGUIObjects consoleGUIObjects, 
			ConsoleEnvironmentInfo consoleEnvironmentInfo, 
			LicenceInfo licenceInfo
			)
		{
			this.consoleGUIObjects = consoleGUIObjects;
			this.consoleEnvironmentInfo = consoleEnvironmentInfo;
			this.licenceInfo = licenceInfo;
		}

		public override bool ShutDownFromPlugIn()
		{
			// TODO cleanup? quando arriva questo evento?
			return base.ShutDownFromPlugIn();
		}

		private void UpdateConsoleTree(TreeView treeConsole)
		{
			PlugInTreeNode rootTreeNode = GetRootTreeNode(treeConsole);

			if (this.rootPlugInNode == null)
			{
				Assembly myAsm = Assembly.GetExecutingAssembly();
				//Stream myStream = myAss.GetManifestResourceStream(myAss.GetName().Name + ".Images.AuditingPlugIn.bmp");

				//StreamReader myreader = new StreamReader(myStream);
				//int indexIcon = consoleTree.ImageList.Images.Add(Image.FromStream(myStream, true), Color.Magenta);

				this.rootPlugInNode = new PlugInTreeNode(Strings.PlugInTitle);
				rootPlugInNode.AssemblyName = myAsm.GetName().Name;
				rootPlugInNode.AssemblyType = this.GetType();
				//rootPlugInNode.ImageIndex = indexIcon;
				//rootPlugInNode.SelectedImageIndex = indexIcon;
				rootPlugInNode.Type = HermesAdminNodeType.HermesAdminRoot.ToString();
				rootPlugInNode.ToolTipText = Strings.PluginNoteToolTip;

				PlugInTreeNode nodeGeneral = CreateNode(HermesAdminNodeType.HermesAdminGeneral);
				AppendSubNode(rootPlugInNode, nodeGeneral);
				PlugInTreeNode nodeLog = CreateNode(HermesAdminNodeType.HermesAdminLog);
				AppendSubNode(rootPlugInNode, nodeLog);
				//PlugInTreeNode nodeServer = CreateNode(HermesAdminNodeType.HermesAdminMailServer);
				//AppendSubNode(rootPlugInNode, nodeServer);
			}

			rootTreeNode.Nodes.Add(this.rootPlugInNode);
			rootTreeNode.Expand();
		}

		private static PlugInTreeNode GetRootTreeNode(TreeView treeConsole)
		{
			// implementazione copiata da altro plug-in, ma Nodes se è vuota?
			return (PlugInTreeNode)treeConsole.Nodes[treeConsole.Nodes.Count - 1];
		}

		private enum HermesAdminNodeType
		{
			HermesAdminRoot,
			HermesAdminGeneral,
            HermesAdminLog,
			HermesAdminMailServer
		}
		private PlugInTreeNode CreateNode(HermesAdminNodeType nodeType)
		{
			PlugInTreeNode node = new PlugInTreeNode();
			node.Text = GetSubNodeText(nodeType);
			node.Type = nodeType.ToString();
			return node;
		}
		static private string GetSubNodeText(HermesAdminNodeType nodeType)
		{
			if (nodeType == HermesAdminNodeType.HermesAdminGeneral)
				return Strings.GeneralSettingsTitle;
			if (nodeType == HermesAdminNodeType.HermesAdminLog)
				return Strings.LogSettingsTitle;
			if (nodeType == HermesAdminNodeType.HermesAdminMailServer)
				return Strings.MailServerSettingsTitle;
			return nodeType.ToString();
		}
		static private void AppendSubNode(PlugInTreeNode root, PlugInTreeNode subNode)
		{
			root.Nodes.Add(subNode);
			subNode.AssemblyName = root.AssemblyName;
			subNode.AssemblyType = root.AssemblyType;
		}

		public void OnAfterSelectConsoleTree(object sender, System.EventArgs e)
		{
			TreeViewEventArgs treeEv = (TreeViewEventArgs)e;
			PlugInTreeNode pNode = treeEv.Node as PlugInTreeNode;
			if (pNode == null)
				return;

			//DisableConsoleToolBarBotton(sender, e);

			if (this.consoleGUIObjects != null && this.consoleGUIObjects.WkgAreaConsole != null)
			{
				Form form = pNode.Tag as Form;
				if (form == null)
				{
					form = CreateForm(pNode);
					pNode.Tag = form;
				}
				this.consoleGUIObjects.WkgAreaConsole.Controls.Clear(); // Serve!!! anche se form != null!
				if (form != null) // potrebbe essere solo un nodo raccoglitore
				{
					form.TopLevel = false;
					form.Dock = DockStyle.Fill;
					//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
					OnBeforeAddFormFromPlugIn(sender, form.ClientSize.Width, form.ClientSize.Height);
					this.consoleGUIObjects.WkgAreaConsole.Controls.Add(form);
					form.Enabled = true;
					form.Show();
				}
			}
		}

		private /*static */Form CreateForm(PlugInTreeNode pNode)
		{
			string strType = pNode.Type;
			if (string.IsNullOrEmpty(strType))
				return null;
			HermesAdminNodeType nType;
			if (false == Enum.TryParse(strType, out nType))
				return null;
			switch (nType)
			{
				case HermesAdminNodeType.HermesAdminRoot:
					return new MainPluginForm();
				case HermesAdminNodeType.HermesAdminGeneral:
					{
						GeneralSettingsForm form = new GeneralSettingsForm();
						form.GetCompanyList = this.GetAllComp;
						return form;
					}
				case HermesAdminNodeType.HermesAdminLog:
					return new LogSettingsForm();
				case HermesAdminNodeType.HermesAdminMailServer:
					return new MailServerSettingsForm();
				default:
					return null;
			}
		}

		// Eventi della Console

		// Eventi di Connessione
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterLogOn")]
		//---------------------------------------------------------------------
		public void OnAfterLogOn(object sender, DynamicEventsArgs e)
		{
			PlugInsTreeView consoleTree = this.consoleGUIObjects.TreeConsole;
			UpdateConsoleTree(consoleTree);
		}

		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterLogOff")]
		//---------------------------------------------------------------------
		public void OnAfterLogOff(object sender, System.EventArgs e)
		{
			CleanUp();
		}

		private void CleanUp()
		{
			PlugInTreeNode pNode = this.rootPlugInNode;
			Control.ControlCollection coll = this.consoleGUIObjects.WkgAreaConsole.Controls;
			ClearNodeForm(pNode, coll);
			foreach (TreeNode sNode in pNode.Nodes)
				ClearNodeForm(sNode, coll);

			// sembra essere a carico del plug-in fare pulizia, anche nel tree
			if (this.rootPlugInNode != null)
				this.rootPlugInNode.Remove();
		}

		private static void ClearNodeForm(TreeNode pNode, Control.ControlCollection coll)
		{
			Form form = pNode.Tag as Form;
			if (form != null && form.IsDisposed == false)
			{
				coll.Remove(form);
				form.Dispose();
				pNode.Tag = null;
			}
		}

		//---------------------------------------------------------------------------	
		// Si noti che sebbene si basi su un event, questo meccanismo non è costruito per una notifica
		// di evento, ma per poter invocare un metodo implementato in  un plugin su cui non si ha reference.
		//
		// La gestione degli eventi/metodi tra plugin della console è programmativamente un incubo:
		// Un plug-in dichiara tramite attributo un metodo
		// (in questo caso SysAdmin dichiara un metodo cablando la stringa "OnGetCompanies")
		// e lo implementa con stesso nome (?), poi MicroareaConsole.Core tramite la classe
		// EventBuilder via reflection crea degli eventi della console con tale signature
		// e costruisce un elenco di futuri sottoscriventi; quando un plugin è instanziato, 
		// se anch'esso implementa un evento con stessa signature e stesso nome, questo viene
		// usato non come evento classico, ma come metodo remoto del plugin dove la funzione
		// è implementata (in questo caso in SysAdmin).
		//
		// A questo si aggiunge che - oltre a usare eventi come fossero semplici delegate - 
		// tutte le naming e signature convention sono stravolte: la signature degli eventi non
		// è quella standard di .net perché sono usati come delegate, i nomi degli eventi sono
		// prefissati con OnXXXX come fossero metodi di event handler (quelli che nello standard
		// della programmazione .net invocano gli eventi veri e propri), i programmatori hanno
		// nel tempo copincollato implementando catene di registrazioni a eventi con sintasse
		// non standard laddove un delegate iniziale avrebbe risolto, etc...
		//
		// Inoltre a livello di design è tutto sbagliato: in questo caso non è ritornato un dato,
		// ma un SqlDataReader, aperto, con connessione che rimane aperta (non sarà mai riciclabile
		// dal connection pool); nulla è dato sapere della query e si è costretti a mischiare logica 
		// di accesso al dato nella UI...
		//
		public delegate System.Data.SqlClient.SqlDataReader GetCompaniesDelegate();
		public event GetCompaniesDelegate OnGetCompanies; // terribile nome non-standard, ma deve chiamarsi così perché cablato in magic-string in sysadmin

		private System.Data.SqlClient.SqlDataReader GetCompaniesDataReader()
		{
			// Il SysAdmin un datareader aperto sull'elenco delle aziende in MSD_Companies
			if (OnGetCompanies != null)
				return OnGetCompanies(); // invoca metodo remoto su plugin SysAdmin
			return null;
		}

		// Logica di accesso al dato. Non mi piace sia qui, ma dipendo da metodo remoto di sysadmin
		private List<string> GetAllComp()
		{
			List<string> list = new List<string>();
			using (System.Data.IDataReader reader = GetCompaniesDataReader())
			{
				while (reader.Read())
				{
					string comp = FetchField<string>(reader, CompanyFields.Company.ToString());
					list.Add(comp);
				}
			}
			return list;
		}
		private enum CompanyFields { Company } // se si vuole oggetto più complesso, estendere. Al momento non voglio entrare nel merito
		//---------------------------------------------------------------------
		private static T FetchField<T>(System.Data.IDataReader reader, string fieldName)
		{
			return FetchField<T>(reader, fieldName, default(T));
		}
		private static T FetchField<T>(System.Data.IDataReader reader, string fieldName, T defaultValue)
		{
			object obj = reader[fieldName];
			if (object.ReferenceEquals(obj, DBNull.Value))
				return defaultValue;
			return (T)obj;
		}
	}
}
