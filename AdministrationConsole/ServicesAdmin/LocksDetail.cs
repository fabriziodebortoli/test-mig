using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using System.Xml;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.ServicesAdmin
{
	/// <summary>
	/// LocksDetail
	/// Visualizza il dettaglio del LockManager (un elenco di utenti applicativi
	/// connessi ad almeno una azienda, con la possibilità di eliminare i loro locks)
	/// </summary>
	//=========================================================================
	public partial class LocksDetail : System.Windows.Forms.Form
	{
		private const string LockMngDateTimeFormat = @"yyyy-MM-ddTHH\:mm\:ss";

		#region Eventi e delegati
		public delegate void SendDiagnostic         (object sender, Diagnostic diagnostic);
		public event         SendDiagnostic			OnSendDiagnostic;
		public delegate void EnableProgressBar		(object sender);
		public event		 EnableProgressBar		OnEnableProgressBar;
		public delegate void DisableProgressBar		(object sender);
		public event		 DisableProgressBar		OnDisableProgressBar;
		public delegate void SetProgressBarStep		(object sender, int step);
		public event		 SetProgressBarStep		OnSetProgressBarStep;
		public delegate void SetProgressBarValue	(object sender, int currentValue);
		public event		 SetProgressBarValue	OnSetProgressBarValue;
		public delegate void SetProgressBarText		(object sender, string message);
		public event		 SetProgressBarText		OnSetProgressBarText;

		// evento per chiedere alla Console l'authentication token
		public delegate string GetAuthenticationToken();
		public event GetAuthenticationToken OnGetAuthenticationToken;
		#endregion

		#region Variabili
        private Diagnostic diagnostic = new Diagnostic("LocksDetails");
		private LoginManager aLoginManager;
		private LockManager aLockManager;
		private XmlDocument xmlDocument = new XmlDocument();

		private static string nameSpacePlugInTreeNode = "Microarea.Console.Core.PlugIns.PlugInTreeNode";

		private const int minimumDataGridCompanyColumnWidth		= 50;
		private const int minimumDataGridApplicationColumnWidth = 100;
		private const int minimumStringsColumnWidth				= 50;
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		//---------------------------------------------------------------------
		public LocksDetail(LoginManager loginManager, LockManager lockManager)
		{
			InitializeComponent();
			aLoginManager	= loginManager;
			aLockManager	= lockManager;
		}
        
		#region ContextMenu
		/// <summary>
		/// BuildContextMenu
		/// Costruisce il context menu per gli utenti, le aziende e i data grid
		/// </summary>
		//---------------------------------------------------------------------
		private void BuildContextMenuTree()
		{
			contextMenuUser				= new ContextMenu();
			contextMenuCompany			= new ContextMenu();
			contextMenuUserDataGrid		= new ContextMenu();
			contextMenuCompanyDataGrid	= new ContextMenu();

			contextMenuUser.MenuItems.Add			(Strings.DeleteLocks, new System.EventHandler(DeleteLocks));
			contextMenuCompany.MenuItems.Add		(Strings.DeleteLocks, new System.EventHandler(DeleteLocks));
			contextMenuUserDataGrid.MenuItems.Add	(Strings.DeleteLocks, new System.EventHandler(DeleteUserLock));
			contextMenuCompanyDataGrid.MenuItems.Add(Strings.DeleteLocks, new System.EventHandler(DeleteCompanyLock));
		}

		/// <summary>
		/// BuildContextMenuRootTree - Context menu del nodo root dei trees
		/// </summary>
		//---------------------------------------------------------------------
		private void BuildContextMenuRootTree()
		{
			contextRootTree = new ContextMenu();
			contextRootTree.MenuItems.Add(Strings.Refresh, new System.EventHandler(OnRefresh));
		}
		#endregion

		#region OnRefresh - Refresh dei trees
		/// <summary>
		/// OnRefresh
		/// </summary>
		//---------------------------------------------------------------------
		private void OnRefresh(object sender, System.EventArgs e)
		{
			if (tabLockViewer.SelectedTab == tabPageUser) 
				LoadViewUsersLocks();
			else 
				if (tabLockViewer.SelectedTab == tabPageCompany) 
					LoadViewCompaniesLocks();
		}
		#endregion

		#region Cancellazione dei Locks
		/// <summary>
		/// DeleteLocks
		/// Cancellazione dei Locks per utente
		/// </summary>
		//---------------------------------------------------------------------
		private void DeleteLocks(object sender, System.EventArgs e)
		{
			bool result = false;

			PlugInTreeNode selectedNode =	(tabLockViewer.SelectedTab == tabPageCompany) 
											? (PlugInTreeNode)treeCompaniesLocks.SelectedNode 
											: (PlugInTreeNode)treeUsersLocks.SelectedNode;
			if (selectedNode == null) 
				return;
			
			switch (selectedNode.Type)
			{
				case "User" : 
				{
					result = UnlockForAllUser(selectedNode.Text); 
					if (result) 
						selectedNode.Remove(); 
					break;
				}
				case "CompanyContainer" : 
				{
					result = UnlockForAllUser(selectedNode.Parent.Text); 
					if (result) 
						selectedNode.Parent.Remove(); 
					break;
				}
				case "CompanyUser": 
				{
					result = UnLockForCompanyAndUser(selectedNode.Tag.ToString(), selectedNode.Text); 
					if (result) 
					{
						if (selectedNode.Parent.Nodes.Count == 1) 
							selectedNode.Parent.Parent.Remove(); 
						else 
							selectedNode.Remove();
					}
					break;
				}
				case "TableContainer": 
				{
					result = UnLockForAllCompany(selectedNode.Tag.ToString()); 
					if (result) 
						selectedNode.Parent.Remove(); 
					break;
				}
				case "Company": 
				{
					result = UnLockForAllCompany(selectedNode.Text); 
					if (result) 
						selectedNode.Remove(); 
					break;
				}
				case "Table": 
				{
					result = UnLockForCompanyAndTable(selectedNode.Tag.ToString(),selectedNode.Text);  
					if (result)
					{
						if (selectedNode.Parent.Nodes.Count == 1) 
							selectedNode.Parent.Parent.Remove(); 
						else 
							selectedNode.Remove();
					}
					break;
				}
			}
		}

		/// <summary>
		/// DeleteUserLock
		/// </summary>
		//---------------------------------------------------------------------
		private void DeleteUserLock(object sender, System.EventArgs e)
		{
			//cancellazione dal dg degli utenti
			PlugInTreeNode selectedUserNode = (PlugInTreeNode) treeUsersLocks.SelectedNode;
			if (selectedUserNode == null) 
				return;

			string companyDbName = selectedUserNode.Text;
			string userName  = selectedUserNode.Tag.ToString();

			DialogResult askIfContinue = MessageBox.Show
				(
				this, 
				string.Format(Strings.AskIfDeleteUserCompanyLocks, userName, companyDbName), 
				Strings.DeleteLocks, 
				MessageBoxButtons.YesNo, 
				MessageBoxIcon.Question
				);
			
			if (askIfContinue == DialogResult.No)
				return;
			
			//ha detto sì vado avanti
			//Abilito la progressBar
			SetConsoleProgressBarValue	(this, 1);
			SetConsoleProgressBarText	(this, Strings.Waiting);
			SetConsoleProgressBarStep	(this, 5);
			EnableConsoleProgressBar	(this);
			Cursor.Current = Cursors.WaitCursor; 
			Cursor.Show();
			
			try
			{
				if (ViewUsersLocksDataGrid.DataSource != null) 
				{
					DataTable dataGridTable = (DataTable)ViewUsersLocksDataGrid.DataSource;
					DataRow	currRow = dataGridTable.Rows[ViewUsersLocksDataGrid.CurrentRowIndex];
					DataColumn myColumn = dataGridTable.Columns["table"];
					string tableName = currRow[myColumn, DataRowVersion.Current].ToString();

					string token = string.Empty;
					if (OnGetAuthenticationToken != null)
						token = OnGetAuthenticationToken();
                
					if (token.Length == 0)
						throw new Exception(Strings.AuthenticationTokenNotValid);

					aLockManager.UnlockAllForCompanyDBNameAndTableAndUser(companyDbName, tableName, userName, token);
					currRow.Delete();
					
					if (dataGridTable.Rows.Count == 0)
					{
						ViewUsersLocksDataGrid.DataSource = null;
						CreateTableUserLockDetails();
						//devo anche cancellare i nodi
						PlugInTreeNode selectedCompany = selectedUserNode;
						PlugInTreeNode containerCompany = selectedCompany.Parent;

						if (containerCompany.Nodes.Count == 1)
							containerCompany.Parent.Remove();
						else
							selectedCompany.Remove();
					}
				}
			}
			catch(WebException webExc)
			{
				if (webExc.Response != null ) 
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());
			}
			catch(SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
			}
			catch(System.Exception exc)
			{
				diagnostic.Set(DiagnosticType.Error, exc.Message);
			}

			//disabilito la progress
			SetConsoleProgressBarText(this, string.Empty);
			DisableConsoleProgressBar(this);
		}

		/// <summary>
		/// DeleteCompanyLock
		/// </summary>
		//---------------------------------------------------------------------
		private void DeleteCompanyLock(object sender, System.EventArgs e)
		{
			//cancellazione dal dg delle company
			PlugInTreeNode selectedCompanyNode = (PlugInTreeNode)treeCompaniesLocks.SelectedNode;
			if (selectedCompanyNode == null) 
				return;

			string tableName	=  selectedCompanyNode.Text;
			string companyDbName= selectedCompanyNode.Tag.ToString();
			DialogResult askIfContinue = MessageBox.Show
				(
				this, 
				string.Format(Strings.AskIfDeleteCompanyTableLocks, companyDbName, tableName), 
				Strings.DeleteLocks, 
				MessageBoxButtons.YesNo, 
				MessageBoxIcon.Question
				);
			if (askIfContinue == DialogResult.No)
				return;

			//ha detto sì vado avanti
			//Abilito la progressBar
			SetConsoleProgressBarValue	(this, 1);
			SetConsoleProgressBarText	(this, Strings.Waiting);
			SetConsoleProgressBarStep	(this, 5);
			EnableConsoleProgressBar	(this);
			Cursor.Current = Cursors.WaitCursor; 
			Cursor.Show();

			try
			{
				if (ViewCompaniesLocksDataGrid.DataSource != null)
				{
					DataTable dataGridTable = (DataTable)ViewCompaniesLocksDataGrid.DataSource;
					DataRow	currRow = dataGridTable.Rows[ViewCompaniesLocksDataGrid.CurrentRowIndex];
					DataColumn myColumn = dataGridTable.Columns["user"];
					string userName = currRow[myColumn, DataRowVersion.Current].ToString();
					
					string token = string.Empty;
					if (OnGetAuthenticationToken != null)
						token = OnGetAuthenticationToken();
                
					if (token.Length == 0)
						throw new Exception(Strings.AuthenticationTokenNotValid);

					aLockManager.UnlockAllForCompanyDBNameAndTableAndUser(companyDbName, tableName, userName, token);
					currRow.Delete();
					
					if (dataGridTable.Rows.Count == 0)
					{
						ViewCompaniesLocksDataGrid.DataSource = null;
						this.CreateTableCompanyLockDetails();
						//devo anche cancellare i nodi
						PlugInTreeNode selectedTable = selectedCompanyNode;
						PlugInTreeNode containerTable = selectedTable.Parent;
						if (containerTable.Nodes.Count == 1)
							containerTable.Parent.Remove();
						else
							selectedTable.Remove();
					}
				}
			}
			catch(WebException webExc)
			{
				if (webExc.Response != null) 
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());
			}
			catch(SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
			}
			catch(System.Exception exc)
			{
				diagnostic.Set(DiagnosticType.Error, exc.Message);
			}

			//disabilito la progress
			SetConsoleProgressBarText(this, string.Empty);
			DisableConsoleProgressBar(this);
		}

		/// <summary>
		/// UnlockForAllUser
		/// </summary>
		//---------------------------------------------------------------------
		private bool UnlockForAllUser(string userName)
		{
			bool result = false;
			if (string.IsNullOrEmpty(userName)) 
				return result;

			DialogResult askIfContinue = MessageBox.Show
				(
				this, 
				string.Format(Strings.AskIfDeleteUserLocks, userName), 
				Strings.DeleteLocks, 
				MessageBoxButtons.YesNo, 
				MessageBoxIcon.Question
				);
			if (askIfContinue == DialogResult.No)
				return result;

			//ha detto sì vado avanti
			//Abilito la progressBar
			SetConsoleProgressBarValue	(this, 1);
			SetConsoleProgressBarText	(this, Strings.Waiting);
			SetConsoleProgressBarStep	(this, 5);
			EnableConsoleProgressBar	(this);
			Cursor.Current = Cursors.WaitCursor; 
			Cursor.Show();

			try
			{
				string token = string.Empty;
				if (OnGetAuthenticationToken != null)
					token = OnGetAuthenticationToken();
                
				if (token.Length == 0)
					throw new Exception(Strings.AuthenticationTokenNotValid);

				//eseguo la cancellazione
				result = aLockManager.UnlockAllForUser(userName, token);
			}
			catch(WebException webExc)
			{
				if (webExc.Response != null ) 
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());
				result = false;
			}
			catch(SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				result = false;
			}
			catch(Exception exc)
			{
				diagnostic.Set(DiagnosticType.Error, exc.Message);
			}

			//disabilito la progress
			SetConsoleProgressBarText (this, string.Empty);
			DisableConsoleProgressBar (this);
			return result;
		}

		/// <summary>
		/// UnLockForCompanyAndUser
		/// </summary>
		//---------------------------------------------------------------------
		private bool UnLockForCompanyAndUser(string userName, string companyName)
		{
			bool result = false;
			if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(companyName)) 
				return result;

			DialogResult askIfContinue = MessageBox.Show
				(
				this, 
				string.Format(Strings.AskIfDeleteCompanyUserLocks, companyName, userName), 
				Strings.DeleteLocks, 
				MessageBoxButtons.YesNo, 
				MessageBoxIcon.Question
				);
			if (askIfContinue == DialogResult.No)
				return result;
			
			//ha detto sì vado avanti
			//Abilito la progressBar
			SetConsoleProgressBarValue	(this, 1);
			SetConsoleProgressBarText	(this, Strings.Waiting);
			SetConsoleProgressBarStep	(this, 5);
			EnableConsoleProgressBar	(this);
			Cursor.Current = Cursors.WaitCursor; 
			Cursor.Show();

			try
			{
				string token = string.Empty;
				if (OnGetAuthenticationToken != null)
					token = OnGetAuthenticationToken();
                
				if (token.Length == 0)
					throw new Exception(Strings.AuthenticationTokenNotValid);

				//eseguo la cancellazione
				result = aLockManager.UnlockAllForCompanyDBNameAndUser(companyName, userName, token);
			}
			catch(WebException webExc)
			{
				if (webExc.Response != null ) 
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());
				result = false;
			}
			catch(SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				result = false;
			}
			catch(System.Exception exc)
			{
				diagnostic.Set(DiagnosticType.Error, exc.Message);
			}

			//disabilito la progress
			SetConsoleProgressBarText(this, string.Empty);
			DisableConsoleProgressBar(this);
			return result;
		}

		//---------------------------------------------------------------------
		private bool UnLockForAllCompany(string companyDbName)
		{
			bool result = false;
			if (string.IsNullOrEmpty(companyDbName)) 
				return result;

			DialogResult askIfContinue = MessageBox.Show
				(
				this, 
				string.Format(Strings.AskIfDeleteCompanyLocks, companyDbName), 
				Strings.DeleteLocks, 
				MessageBoxButtons.YesNo, 
				MessageBoxIcon.Question
				);
			if (askIfContinue == DialogResult.No)
				return result;

			//ha detto sì vado avanti
			//Abilito la progressBar
			SetConsoleProgressBarValue	(this, 1);
			SetConsoleProgressBarText	(this, Strings.Waiting);
			SetConsoleProgressBarStep	(this, 5);
			EnableConsoleProgressBar	(this);
			Cursor.Current = Cursors.WaitCursor; 
			Cursor.Show();

			try
			{
				string token = string.Empty;
				if (OnGetAuthenticationToken != null)
					token = OnGetAuthenticationToken();
                
				if (token.Length == 0)
					throw new Exception(Strings.AuthenticationTokenNotValid);

				//eseguo la cancellazione
				result = aLockManager.UnlockAllForCompanyDBName(companyDbName, token);
			}
			catch(WebException webExc)
			{
				if (webExc.Response != null) 
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());
				result = false;
			}
			catch(SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				result = false;
			}
			catch(System.Exception exc)
			{
				diagnostic.Set(DiagnosticType.Error, exc.Message);
			}

			//disabilito la progress
			SetConsoleProgressBarText(this, string.Empty);
			DisableConsoleProgressBar(this);
			return result;
		}
		
		//---------------------------------------------------------------------
		private bool UnLockForCompanyAndTable(string companyDbName, string tableName)
		{
			bool result = false;
			if (string.IsNullOrEmpty(companyDbName) || string.IsNullOrEmpty(tableName)) 
				return result;

			DialogResult askIfContinue = MessageBox.Show
				(
				this, 
				string.Format(Strings.AskIfDeleteCompanyTableLocks, companyDbName, tableName), 
				Strings.DeleteLocks, 
				MessageBoxButtons.YesNo, 
				MessageBoxIcon.Question
				);
			if (askIfContinue == DialogResult.No)
				return result;
			
			//ha detto sì vado avanti
			//Abilito la progressBar
			SetConsoleProgressBarValue	(this, 1);
			SetConsoleProgressBarText	(this, Strings.Waiting);
			SetConsoleProgressBarStep	(this, 5);
			EnableConsoleProgressBar	(this);
			Cursor.Current = Cursors.WaitCursor; 
			Cursor.Show();

			try
			{
				string token = string.Empty;
				if (OnGetAuthenticationToken != null)
					token = OnGetAuthenticationToken();
                
				if (token.Length == 0)
					throw new Exception(Strings.AuthenticationTokenNotValid);

				//eseguo la cancellazione
				result = aLockManager.UnlockAllForCompanyDBNameAndTable(companyDbName, tableName, token);
			}
			catch(WebException webExc)
			{
				if (webExc.Response != null) 
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());
				result = false;
			}
			catch(SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				result = false;
			}
			catch(System.Exception exc)
			{
				diagnostic.Set(DiagnosticType.Error, exc.Message);
			}

			//disabilito la progress
			SetConsoleProgressBarText(this, string.Empty);
			DisableConsoleProgressBar(this);
			return result;
		}
		#endregion

		#region LoadUsersWithLocks - Carico gli utenti che hanno locks
		/// <summary>
		/// LoadUsersWithLocks
		/// Carico gli utenti che sono stati associati a una azienda
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadUsersWithLocks(PlugInTreeNode root)
		{
			try
			{
				if (xmlDocument.DocumentElement == null) 
				{
					diagnostic.Set(DiagnosticType.Warning, Strings.NoLocksFounded);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
					else
						DiagnosticViewer.ShowDiagnostic(diagnostic);
					return;
				}

				//array delle section
				XmlNodeList users =  xmlDocument.DocumentElement.SelectNodes("//Companies/Company/Table/Lock");
				if (users == null || users.Count == 0)
					return;
				
				foreach(XmlNode currentNode in users)
				{
					int position			= 0;
					PlugInTreeNode user		= new PlugInTreeNode();
					user.Text				= currentNode.Attributes["userName"].Value;
					user.AssemblyName		= Assembly.GetExecutingAssembly().GetName().Name;
					user.AssemblyType		= typeof(ServicesAdmin);
					user.Type				= "User";
					user.ContextMenu    	= contextMenuUser;
					
					if (root.Nodes.Count == 0)
					{
						PlugInTreeNode companiesUser	= new PlugInTreeNode();
						companiesUser.Text				= Strings.CompanyContainers;
						companiesUser.Tag				= user.Text;
						companiesUser.AssemblyName		= Assembly.GetExecutingAssembly().GetName().Name;
						companiesUser.AssemblyType		= typeof(ServicesAdmin);
						companiesUser.Type				= "CompanyContainer";
						companiesUser.ContextMenu		= contextMenuUser;
						LoadCompaniesFromUser(user.Text, companiesUser);
						int pos							= user.Nodes.Add(companiesUser);
						user.Nodes[pos].ImageIndex		= user.Nodes[pos].SelectedImageIndex = PlugInTreeNode.GetCompaniesDefaultImageIndex;
						position = root.Nodes.Add(user);
                        root.Nodes[position].ImageIndex = root.Nodes[position].SelectedImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
					}
					else
					{
						bool toInsert = false;
						TreeNodeCollection myNodeCollection = root.Nodes;
						IEnumerator myEnumerator = myNodeCollection.GetEnumerator();
						while(myEnumerator.MoveNext())
						{
							if (((TreeNode)myEnumerator.Current).Text == user.Text)
							{
								toInsert = false;
								break;
							}
							toInsert = true;
						}
						if (toInsert)
						{
							PlugInTreeNode companiesUser	= new PlugInTreeNode();
							companiesUser.Text				= Strings.CompanyContainers;
							companiesUser.AssemblyName		= Assembly.GetExecutingAssembly().GetName().Name;
							companiesUser.AssemblyType		= typeof(ServicesAdmin);
							companiesUser.Type				= "CompanyContainer";
							companiesUser.ContextMenu		= contextMenuUser;
							LoadCompaniesFromUser(user.Text, companiesUser);
							int pos							= user.Nodes.Add(companiesUser);
                            user.Nodes[pos].ImageIndex = user.Nodes[pos].SelectedImageIndex = PlugInTreeNode.GetCompaniesDefaultImageIndex;
							position = root.Nodes.Add(user);
                            root.Nodes[position].ImageIndex = root.Nodes[position].SelectedImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
						}
					}
				}
			}
			catch(Exception exc)
			{
				Debug.Fail(exc.Message);
			}
		}
		#endregion

		#region LoadCompaniesLocks - Carico le aziende che hanno locks
		/// <summary>
		/// LoadCompaniesLocks
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadCompaniesLocks(PlugInTreeNode root)
		{
			if (xmlDocument.DocumentElement == null) 
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.NoLocksFounded);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				else
					DiagnosticViewer.ShowDiagnostic(diagnostic);
				return;
			}

			XmlElement companies = xmlDocument.DocumentElement;
			if (companies == null || companies.ChildNodes == null || companies.ChildNodes.Count == 0) 
				return;
			
			foreach (XmlElement company in companies.ChildNodes)
			{
				string companyName				= company.GetAttribute("companyDBName");
				PlugInTreeNode companyNode		= new PlugInTreeNode();
				companyNode.Type				= "Company";
				companyNode.Text				= companyName;
				companyNode.AssemblyName		= Assembly.GetExecutingAssembly().GetName().Name;
				companyNode.AssemblyType		= typeof(ServicesAdmin);
				companyNode.ContextMenu			= contextMenuCompany;
				if (company.ChildNodes.Count == 0) 
					continue;

				PlugInTreeNode containersTable	= new PlugInTreeNode();
				containersTable.Type			= "TableContainer";
				containersTable.Text			= Strings.TableContainers;
				containersTable.Tag				= companyName;
				containersTable.AssemblyName	= Assembly.GetExecutingAssembly().GetName().Name;
				containersTable.AssemblyType	= typeof(ServicesAdmin);
				containersTable.ContextMenu = contextMenuCompany;

				foreach( XmlElement table in company.ChildNodes)
				{
					PlugInTreeNode tableNode	= new PlugInTreeNode();
					tableNode.Type				= "Table";
					tableNode.Tag				= companyName;
					tableNode.Text				= table.GetAttribute("name");
					tableNode.AssemblyName		= Assembly.GetExecutingAssembly().GetName().Name;
					tableNode.AssemblyType		= typeof(ServicesAdmin);
					tableNode.ContextMenu		= contextMenuCompany;
					int posTable				= containersTable.Nodes.Add(tableNode);
                    containersTable.Nodes[posTable].ImageIndex = containersTable.Nodes[posTable].SelectedImageIndex = PlugInTreeNode.GetTableDefaultImageIndex;
				}

				int posTables = companyNode.Nodes.Add(containersTable);
                companyNode.Nodes[posTables].ImageIndex = companyNode.Nodes[posTables].SelectedImageIndex = PlugInTreeNode.GetDefaultImageIndex;

				int pos = root.Nodes.Add(companyNode);
                root.Nodes[pos].ImageIndex = root.Nodes[pos].SelectedImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
			}
		}
		#endregion

		#region LoadCompaniesFromUser - Carico le aziende dato l'utente
		/// <summary>
		/// LoadCompaniesFromUser
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadCompaniesFromUser(string userName, PlugInTreeNode companiesOfUser)
		{
			XmlNodeList companies = xmlDocument.SelectNodes("//Company[Table/Lock/@userName='" + userName + "']");
			if (companies == null || companies.Count == 0 )
				return;
			
			ArrayList companiesForUser = new ArrayList();
			foreach(XmlElement xmlElement in companies)
			{
				string companydbName = xmlElement.GetAttribute("companyDBName");
				if (!companiesForUser.Contains(companydbName))
					companiesForUser.Add(companydbName);
			}

			for (int i = 0; i < companiesForUser.Count; i++)
			{
				PlugInTreeNode companyNode	= new PlugInTreeNode();
				companyNode.Type			= "CompanyUser";
				companyNode.Text			= companiesForUser[i].ToString();
				companyNode.Tag				= userName;
				companyNode.AssemblyName	= Assembly.GetExecutingAssembly().GetName().Name;
				companyNode.AssemblyType	= typeof(ServicesAdmin);
				companyNode.ContextMenu     = contextMenuUser;
				int pos = companiesOfUser.Nodes.Add(companyNode);
                companiesOfUser.Nodes[pos].ImageIndex = companiesOfUser.Nodes[pos].SelectedImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
			}
		}
		#endregion

		#region LocksDetail_Load - Load della form e riempimento del tree
		/// <summary>
		/// LocksDetail_Load
		/// </summary>
		//---------------------------------------------------------------------
		private void LocksDetail_Load(object sender, System.EventArgs e)
		{
			tabLockViewer.SelectedTab = tabPageUser;
			
			BuildContextMenuTree();
			BuildContextMenuRootTree();
			treeCompaniesLocks.Nodes.Clear();

			InitializeUsersLocksGridStyle();
			CreateTableUserLockDetails();

			InitializeCompaniesLocksGridStyle();
			CreateTableCompanyLockDetails();

			LoadViewUsersLocks();
		} 
		#endregion

		#region Funzioni di Inizializzazione del Tree degli Utenti
		//---------------------------------------------------------------------
		private void InitializeUsersLocksGridStyle()
		{
			ViewUsersLocksDataGrid.TableStyles.Clear();

			DataGridTableStyle dataGridUsersLocksStyle = new DataGridTableStyle();
			dataGridUsersLocksStyle.DataGrid = ViewUsersLocksDataGrid;
			dataGridUsersLocksStyle.MappingName = "UserLockDetail";
			dataGridUsersLocksStyle.GridLineStyle = DataGridLineStyle.Solid;
			dataGridUsersLocksStyle.RowHeadersVisible = true;
			dataGridUsersLocksStyle.ColumnHeadersVisible = true;
			dataGridUsersLocksStyle.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			dataGridUsersLocksStyle.PreferredRowHeight = dataGridUsersLocksStyle.HeaderFont.Height;
			dataGridUsersLocksStyle.PreferredColumnWidth = 100;
			dataGridUsersLocksStyle.ReadOnly = true;

			//dataGridDateTextBox
			DataGridTextBoxColumn dataGridDateTextBox = new DataGridTextBoxColumn();
			dataGridDateTextBox.Alignment = HorizontalAlignment.Left;
			dataGridDateTextBox.Format = "";
			dataGridDateTextBox.FormatInfo = null;
			dataGridDateTextBox.MappingName = "date";
			dataGridDateTextBox.HeaderText = Strings.DataHeader;
			dataGridDateTextBox.NullText = String.Empty;
			dataGridDateTextBox.ReadOnly = true;
			dataGridDateTextBox.WidthChanged += new System.EventHandler(this.ViewUsersLocksDataGrid_SizeChanged);
			dataGridUsersLocksStyle.GridColumnStyles.Add(dataGridDateTextBox);

			//dataGridApplicationTextBox
			DataGridTextBoxColumn dataGridApplicationTextBox = new DataGridTextBoxColumn();
			dataGridApplicationTextBox.Alignment = HorizontalAlignment.Left;
			dataGridApplicationTextBox.Format = "";
			dataGridApplicationTextBox.FormatInfo = null;
			dataGridApplicationTextBox.MappingName = "application";
			dataGridApplicationTextBox.HeaderText = Strings.ApplicationHeader;
			dataGridApplicationTextBox.NullText = String.Empty;
			dataGridApplicationTextBox.ReadOnly = true;
			dataGridApplicationTextBox.WidthChanged += new System.EventHandler(this.ViewUsersLocksDataGrid_SizeChanged);
			dataGridUsersLocksStyle.GridColumnStyles.Add(dataGridApplicationTextBox);

			//dataGridTableTextBox
			DataGridTextBoxColumn dataGridTableTextBox = new DataGridTextBoxColumn();
			dataGridTableTextBox.Alignment = HorizontalAlignment.Left;
			dataGridTableTextBox.Format = "";
			dataGridTableTextBox.FormatInfo = null;
			dataGridTableTextBox.MappingName = "table";
			dataGridTableTextBox.HeaderText = Strings.TableHeader;
			dataGridTableTextBox.NullText = String.Empty;
			dataGridTableTextBox.ReadOnly = true;
			dataGridTableTextBox.WidthChanged += new System.EventHandler(this.ViewUsersLocksDataGrid_SizeChanged);
			dataGridUsersLocksStyle.GridColumnStyles.Add(dataGridTableTextBox);

			//dataGridKeyLockTextBox
			DataGridTextBoxColumn dataGridKeyLockTextBox = new DataGridTextBoxColumn();
			dataGridKeyLockTextBox.Alignment = HorizontalAlignment.Left;
			dataGridKeyLockTextBox.Format = "";
			dataGridKeyLockTextBox.FormatInfo = null;
			dataGridKeyLockTextBox.MappingName = "lockKey";
			dataGridKeyLockTextBox.HeaderText = Strings.KeyHeader;
			dataGridKeyLockTextBox.NullText = String.Empty;
			dataGridKeyLockTextBox.ReadOnly = true;
			dataGridKeyLockTextBox.WidthChanged += new System.EventHandler(this.ViewUsersLocksDataGrid_SizeChanged);
			dataGridUsersLocksStyle.GridColumnStyles.Add(dataGridKeyLockTextBox);

            //dataGridAddressTextBox
            DataGridTextBoxColumn dataGridAddressTextBox = new DataGridTextBoxColumn();
            dataGridAddressTextBox.Alignment = HorizontalAlignment.Left;
            dataGridAddressTextBox.Format = "";
            dataGridAddressTextBox.FormatInfo = null;
            dataGridAddressTextBox.MappingName = "address";
            dataGridAddressTextBox.HeaderText = Strings.AddressHeader;
            dataGridAddressTextBox.NullText = String.Empty;
            dataGridAddressTextBox.ReadOnly = true;
            dataGridAddressTextBox.WidthChanged += new System.EventHandler(this.ViewUsersLocksDataGrid_SizeChanged);
            dataGridUsersLocksStyle.GridColumnStyles.Add(dataGridAddressTextBox);

			ViewUsersLocksDataGrid.TableStyles.Add(dataGridUsersLocksStyle);
			AdjustLastUsersLocksDataGridColumnWidth();
		}

		//---------------------------------------------------------------------
		private void ViewUsersLocksDataGrid_SizeChanged(object sender, System.EventArgs e)
		{
			AdjustLastUsersLocksDataGridColumnWidth();
		}

		//---------------------------------------------------------------------
		private void AdjustLastUsersLocksDataGridColumnWidth()
		{
			if (ViewUsersLocksDataGrid.TableStyles == null || ViewUsersLocksDataGrid.TableStyles.Count == 0)
				return;

			// ScheduledTask.ScheduledTasksTableName is the MappingName of the DataGridTableStyle to retrieve. 
			DataGridTableStyle usersDataGridTableStyle = ViewUsersLocksDataGrid.TableStyles[0]; 

			if (usersDataGridTableStyle != null)
			{
				int colswidth = ViewUsersLocksDataGrid.RowHeaderWidth;
				for (int i = 0; i < usersDataGridTableStyle.GridColumnStyles.Count -1; i++)
					colswidth += usersDataGridTableStyle.GridColumnStyles[i].Width;

				usersDataGridTableStyle.GridColumnStyles[usersDataGridTableStyle.GridColumnStyles.Count -1].Width = 
					Math.Max(minimumStringsColumnWidth, ViewUsersLocksDataGrid.ClientSize.Width - colswidth);
				
				colswidth += usersDataGridTableStyle.GridColumnStyles[usersDataGridTableStyle.GridColumnStyles.Count -1].Width;

				ViewUsersLocksDataGrid.Parent.Refresh();
			}
		}

		//---------------------------------------------------------------------
		public void CreateTableUserLockDetails()
		{
			DataTable dataGridTable = new DataTable("UserLockDetail");

			//dateColum
			DataColumn dateColumn = new DataColumn();
			dateColumn.DataType = System.Type.GetType("System.DateTime");
			dateColumn.ColumnName = "date";
			dateColumn.ReadOnly = true;
			dateColumn.Unique = false;
			dataGridTable.Columns.Add(dateColumn);

			//applicationColumn
			DataColumn applicationColumn = new DataColumn();
			applicationColumn.DataType = System.Type.GetType("System.String");
			applicationColumn.ColumnName = "application";
			applicationColumn.ReadOnly = true;
			applicationColumn.Unique = false;
			dataGridTable.Columns.Add(applicationColumn);

			//tableColumn
			DataColumn tableColumn = new DataColumn();
			tableColumn.DataType = System.Type.GetType("System.String");
			tableColumn.ColumnName = "table";
			tableColumn.ReadOnly = true;
			tableColumn.Unique = false;
			dataGridTable.Columns.Add(tableColumn);

			//lockKey
			DataColumn keyColumn = new DataColumn();
			keyColumn.DataType = System.Type.GetType("System.String");
			keyColumn.ColumnName = "lockKey";
			keyColumn.ReadOnly = true;
			keyColumn.Unique = false;
			dataGridTable.Columns.Add(keyColumn);

            //address
            DataColumn addressColumn = new DataColumn();
            addressColumn.DataType = System.Type.GetType("System.String");
            addressColumn.ColumnName = "address";
            addressColumn.ReadOnly = true;
            addressColumn.Unique = false;
            dataGridTable.Columns.Add(addressColumn);
			
			ViewUsersLocksDataGrid.DataSource = dataGridTable;
		}
		#endregion

		#region Funzioni di Inizializzazione del Tree della Aziende
		//---------------------------------------------------------------------
		private void InitializeCompaniesLocksGridStyle()
		{
			ViewCompaniesLocksDataGrid.TableStyles.Clear();

			DataGridTableStyle dataGridCompaniesLocksStyle = new DataGridTableStyle();
			dataGridCompaniesLocksStyle.DataGrid = ViewCompaniesLocksDataGrid;
			dataGridCompaniesLocksStyle.MappingName = "CompanyLockDetail";
			dataGridCompaniesLocksStyle.GridLineStyle = DataGridLineStyle.Solid;
			dataGridCompaniesLocksStyle.RowHeadersVisible = true;
			dataGridCompaniesLocksStyle.ColumnHeadersVisible = true;
			dataGridCompaniesLocksStyle.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			dataGridCompaniesLocksStyle.PreferredRowHeight = dataGridCompaniesLocksStyle.HeaderFont.Height;
			dataGridCompaniesLocksStyle.PreferredColumnWidth = 100;
			dataGridCompaniesLocksStyle.ReadOnly = true;

			//dataGridCompanyTextBox
			DataGridTextBoxColumn dataGridDateTextBox = new DataGridTextBoxColumn();
			dataGridDateTextBox.Alignment = HorizontalAlignment.Left;
			dataGridDateTextBox.Format = "";
			dataGridDateTextBox.FormatInfo = null;
			dataGridDateTextBox.MappingName = "date";
			dataGridDateTextBox.HeaderText = Strings.DataHeader;
			dataGridDateTextBox.NullText = String.Empty;
			dataGridDateTextBox.ReadOnly = true;
			dataGridDateTextBox.WidthChanged += new System.EventHandler(this.ViewCompaniesLocksDataGrid_SizeChanged);
			dataGridCompaniesLocksStyle.GridColumnStyles.Add(dataGridDateTextBox);

			//dataGridUserTextBox
			DataGridTextBoxColumn dataGridUserTextBox = new DataGridTextBoxColumn();
			dataGridUserTextBox.Alignment = HorizontalAlignment.Left;
			dataGridUserTextBox.Format = "";
			dataGridUserTextBox.FormatInfo = null;
			dataGridUserTextBox.MappingName = "user";
			dataGridUserTextBox.HeaderText = Strings.UserHeader;
			dataGridUserTextBox.NullText = String.Empty;
			dataGridUserTextBox.ReadOnly = true;
			dataGridUserTextBox.WidthChanged += new System.EventHandler(this.ViewCompaniesLocksDataGrid_SizeChanged);
			dataGridCompaniesLocksStyle.GridColumnStyles.Add(dataGridUserTextBox);

			//dataGridApplicationTextBox
			DataGridTextBoxColumn dataGridApplicationTextBox = new DataGridTextBoxColumn();
			dataGridApplicationTextBox.Alignment = HorizontalAlignment.Left;
			dataGridApplicationTextBox.Format = "";
			dataGridApplicationTextBox.FormatInfo = null;
			dataGridApplicationTextBox.MappingName = "application";
			dataGridApplicationTextBox.HeaderText = Strings.ApplicationHeader;
			dataGridApplicationTextBox.NullText = String.Empty;
			dataGridApplicationTextBox.ReadOnly = true;
			dataGridApplicationTextBox.WidthChanged += new System.EventHandler(this.ViewCompaniesLocksDataGrid_SizeChanged);
			dataGridCompaniesLocksStyle.GridColumnStyles.Add(dataGridApplicationTextBox);

			//dataGridLockKeyTextBox
			DataGridTextBoxColumn dataGridLockKeyTextBox = new DataGridTextBoxColumn();
			dataGridLockKeyTextBox.Alignment = HorizontalAlignment.Left;
			dataGridLockKeyTextBox.Format = "";
			dataGridLockKeyTextBox.FormatInfo = null;
			dataGridLockKeyTextBox.MappingName = "lockKey";
			dataGridLockKeyTextBox.HeaderText = Strings.KeyHeader;
			dataGridLockKeyTextBox.NullText = String.Empty;
			dataGridLockKeyTextBox.WidthChanged += new System.EventHandler(this.ViewCompaniesLocksDataGrid_SizeChanged);
			dataGridCompaniesLocksStyle.GridColumnStyles.Add(dataGridLockKeyTextBox);

            //dataGridAddressTextBox
            DataGridTextBoxColumn dataGridAddressTextBox = new DataGridTextBoxColumn();
            dataGridAddressTextBox.Alignment = HorizontalAlignment.Left;
            dataGridAddressTextBox.Format = "";
            dataGridAddressTextBox.FormatInfo = null;
            dataGridAddressTextBox.MappingName = "address";
            dataGridAddressTextBox.HeaderText = Strings.AddressHeader;
            dataGridAddressTextBox.NullText = String.Empty;
            dataGridAddressTextBox.ReadOnly = true;
            dataGridAddressTextBox.WidthChanged += new System.EventHandler(this.ViewUsersLocksDataGrid_SizeChanged);
            dataGridCompaniesLocksStyle.GridColumnStyles.Add(dataGridAddressTextBox);

			ViewCompaniesLocksDataGrid.TableStyles.Add(dataGridCompaniesLocksStyle);
			AdjustLastCompaniesLocksDataGridColumnWidth();
		}

		//---------------------------------------------------------------------
		private void ViewCompaniesLocksDataGrid_SizeChanged(object sender, System.EventArgs e)
		{
			AdjustLastCompaniesLocksDataGridColumnWidth();
		}

		//---------------------------------------------------------------------
		private void AdjustLastCompaniesLocksDataGridColumnWidth()
		{
			if (ViewCompaniesLocksDataGrid.TableStyles == null || ViewCompaniesLocksDataGrid.TableStyles.Count == 0)
				return;
			// ScheduledTask.ScheduledTasksTableName is the MappingName of the DataGridTableStyle to retrieve. 
			DataGridTableStyle companiesDataGridTableStyle = ViewCompaniesLocksDataGrid.TableStyles[0]; 

			if (companiesDataGridTableStyle != null)
			{
				int colswidth = ViewCompaniesLocksDataGrid.RowHeaderWidth;
				for (int i = 0; i < companiesDataGridTableStyle.GridColumnStyles.Count -1; i++)
					colswidth += companiesDataGridTableStyle.GridColumnStyles[i].Width;

				companiesDataGridTableStyle.GridColumnStyles[companiesDataGridTableStyle.GridColumnStyles.Count -1].Width = 
					Math.Max(minimumStringsColumnWidth, ViewCompaniesLocksDataGrid.ClientSize.Width - colswidth);
				
				colswidth += companiesDataGridTableStyle.GridColumnStyles[companiesDataGridTableStyle.GridColumnStyles.Count -1].Width;

				ViewCompaniesLocksDataGrid.Parent.Refresh();
			}
		}

		//---------------------------------------------------------------------
		public void CreateTableCompanyLockDetails()
		{
			DataTable dataGridTable = new DataTable("CompanyLockDetail");

			//DataColum
			DataColumn dateColumn = new DataColumn();
			dateColumn.DataType = System.Type.GetType("System.DateTime");
			dateColumn.ColumnName = "date";
			dateColumn.ReadOnly = true;
			dateColumn.Unique = false;
			dataGridTable.Columns.Add(dateColumn);

			//userColumn
			DataColumn userColumn = new DataColumn();
			userColumn.DataType = System.Type.GetType("System.String");
			userColumn.ColumnName = "user";
			userColumn.ReadOnly = true;
			userColumn.Unique = false;
			dataGridTable.Columns.Add(userColumn);

			//applicationColumn
			DataColumn applicationColumn = new DataColumn();
			applicationColumn.DataType = System.Type.GetType("System.String");
			applicationColumn.ColumnName = "application";
			applicationColumn.ReadOnly = true;
			applicationColumn.Unique = false;
			dataGridTable.Columns.Add(applicationColumn);

			//lockKey
			DataColumn keyColumn = new DataColumn();
			keyColumn.DataType = System.Type.GetType("System.String");
			keyColumn.ColumnName = "lockKey";
			keyColumn.ReadOnly = true;
			keyColumn.Unique = false;
			dataGridTable.Columns.Add(keyColumn);

            //address
            DataColumn addressColumn = new DataColumn();
            addressColumn.DataType = System.Type.GetType("System.String");
            addressColumn.ColumnName = "address";
            addressColumn.ReadOnly = true;
            addressColumn.Unique = false;
            dataGridTable.Columns.Add(addressColumn);
			
			ViewCompaniesLocksDataGrid.DataSource = dataGridTable;
		}
		#endregion
		
		#region LoadViewUsersLocks - Visualizzazione dei Locks Per utente
		/// <summary>
		/// LoadViewUsersLocks
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadViewUsersLocks()
		{
			treeUsersLocks.Nodes.Clear();
			InitializeUsersLocksGridStyle();
			CreateTableUserLockDetails();
			SubtitleLabel.Text =Strings.SubTitleViewUsersLocks;

			try
			{
				string message = aLockManager.GetLockEntriesAtt();
				//message = "<Companies><Company companyDBName='MagoNET1'><Table name='PROVA'><Lock authenticationToken='111' address='sss' lockdate='06/07/2004' lockkey='223344a' userName='Nadia' processName='PIPPO'></Lock></Table></Company></Companies>";
				if (message.Length > 0)
					xmlDocument.LoadXml(message);

				PlugInTreeNode rootUsersNode = new PlugInTreeNode(Strings.Users);
				rootUsersNode.AssemblyName	 = Assembly.GetExecutingAssembly().GetName().Name;
				rootUsersNode.AssemblyType	 = typeof(ServicesAdmin);
				rootUsersNode.ContextMenu	 = contextRootTree;
				int pos						 = treeUsersLocks.Nodes.Add(rootUsersNode);
                treeUsersLocks.Nodes[pos].ImageIndex = treeUsersLocks.Nodes[pos].SelectedImageIndex = PlugInTreeNode.GetUsersDefaultImageIndex;
				LoadUsersWithLocks(rootUsersNode);
				if (rootUsersNode.Nodes.Count > 0)
				{
					treeUsersLocks.ExpandAll();
					ViewUsersLocksDataGrid.Enabled = true;
				}
				else
					ViewUsersLocksDataGrid.Enabled = false;
			}
			catch(WebException webExc)
			{
				if (webExc.Response != null ) 
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());
			}
			catch(Exception exc)
			{
				Debug.Fail(exc.Message);
			}
		}
		#endregion

		#region LoadViewCompaniesLocks - Visualizzazione dei Locks per azienda
		/// <summary>
		/// LoadViewCompaniesLocks
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadViewCompaniesLocks()
		{
			treeCompaniesLocks.Nodes.Clear();
			InitializeCompaniesLocksGridStyle();
			CreateTableCompanyLockDetails();
			SubtitleLabel.Text = Strings.SubTitleViewCompaniesLocks;
			//GetCompanyDBAndTableLocksList per il secondo tree e per la griglia GetLocksList
			try
			{
				aLockManager.GetCompanyDBAndTableLocksList();
				if (File.Exists(BasePathFinder.BasePathFinderInstance.GetLockLogFile()))
					xmlDocument.Load(BasePathFinder.BasePathFinderInstance.GetLockLogFile());
			
				PlugInTreeNode rootCompaniesNode	= new PlugInTreeNode(Strings.Company);
				rootCompaniesNode.AssemblyName		= Assembly.GetExecutingAssembly().GetName().Name;
				rootCompaniesNode.AssemblyType		= typeof(ServicesAdmin);
				rootCompaniesNode.ContextMenu	    = contextRootTree;
				int pos = treeCompaniesLocks.Nodes.Add(rootCompaniesNode);
                treeCompaniesLocks.Nodes[pos].ImageIndex = treeCompaniesLocks.Nodes[pos].SelectedImageIndex = PlugInTreeNode.GetCompaniesDefaultImageIndex;
				LoadCompaniesLocks(rootCompaniesNode);
				if (rootCompaniesNode.Nodes.Count > 0)
				{	
					treeCompaniesLocks.ExpandAll();
					ViewCompaniesLocksDataGrid.Enabled = true;
				}
				else
					ViewCompaniesLocksDataGrid.Enabled = false;
			}
			catch(WebException webExc)
			{
				if (webExc.Response != null ) 
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());
			}
			catch(System.Exception exc)
			{
				Debug.Fail(exc.Message);
			}
		}
		#endregion

		#region Eventi per Abilitare e Impostare la ProgressBar
		//---------------------------------------------------------------------
		private void EnableConsoleProgressBar(object sender)
		{
			if (OnEnableProgressBar != null)
				OnEnableProgressBar(sender);
		}

		//---------------------------------------------------------------------
		private void DisableConsoleProgressBar(object sender)
		{
			if (OnDisableProgressBar != null)
				OnDisableProgressBar(sender);
		}

		//---------------------------------------------------------------------
		private void SetConsoleProgressBarStep(object sender, int step)
		{
			if (OnSetProgressBarStep != null)
				OnSetProgressBarStep(sender, step);
		}

		//---------------------------------------------------------------------
		private void SetConsoleProgressBarValue(object sender, int currentValue)
		{
			if (OnSetProgressBarValue != null)
				OnSetProgressBarValue(sender, currentValue);
		}

		//---------------------------------------------------------------------
		private void SetConsoleProgressBarText(object sender, string message)
		{
			if (OnSetProgressBarText != null)
				OnSetProgressBarText(sender, message);
		}
		#endregion

		#region treeUsers_MouseDown - Evento per gestire l'attach del contextmenu sul nodo singolo utente del tree
		/// <summary>
		/// treeUsersLocks_MouseDown
		/// Evento per gestire l'attach del contextmenu sul nodo singolo utente del tree
		/// </summary>
		//---------------------------------------------------------------------
		private void treeUsersLocks_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			TreeView localTree = (TreeView)sender;
			PlugInTreeNode nodeToSelectIfNull = (PlugInTreeNode)localTree.Nodes[0].FirstNode == null 
												? (PlugInTreeNode)localTree.Nodes[0] 
												: (PlugInTreeNode)localTree.Nodes[0].FirstNode;

			//se ho cliccato in una zona fuori dal tree seleziono come nodo di destinazione
			//il primo nodo del tree
			localTree.SelectedNode = ((PlugInTreeNode)localTree.GetNodeAt(e.X, e.Y) != null) 
									? (PlugInTreeNode)localTree.GetNodeAt(e.X, e.Y) 
									: nodeToSelectIfNull;
			PlugInTreeNode selected	= (PlugInTreeNode)localTree.SelectedNode;

			if (selected != null)
			{
				Type nodeType = selected.GetType();
				if ((string.Compare(nodeType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0) ||
					(string.Compare(nodeType.BaseType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0))
				{
					switch (e.Button)
					{
						case MouseButtons.Left : 
							break;
						case MouseButtons.Right:
                            treeUsersLocks.ContextMenu = (selected.ContextMenu != null) ? selected.ContextMenu : null;
							break;
						case MouseButtons.Middle: break;
						case MouseButtons.None  : break;
						default                 : break;
					}
				}
			}
			else 
				treeUsersLocks.ContextMenu = null;
		}
		#endregion

		#region Selezione della Tab
		/// <summary>
		/// tabLockViewer_SelectedIndexChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void tabLockViewer_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			//ho selezionato la tab con vista utenti
			if (((TabControl)sender).SelectedTab == tabPageUser) 
				LoadViewUsersLocks();
			else //ho selezionato la tab con vista aziende
				if (((TabControl)sender).SelectedTab == tabPageCompany) 
					LoadViewCompaniesLocks();
		}

		/// <summary>
		/// tabLockViewer_TabIndexChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void tabLockViewer_TabIndexChanged(object sender, System.EventArgs e)
		{
			//ho selezionato la tab con vista utenti
			if (((TabControl)sender).SelectedTab == tabPageUser) 
				LoadViewUsersLocks();
			else //ho selezionato la tab con vista aziende 
				if (((TabControl)sender).SelectedTab == tabPageCompany) 
					LoadViewCompaniesLocks();
		}
		#endregion

		#region InsertDetailLocksUsersFromXml 
		/// <summary>
		/// InsertDetailLocksUsersFromXml
		/// </summary>
		//---------------------------------------------------------------------
		private void InsertDetailLocksUsersFromXml(string userName, string companyName)
		{
			XmlNodeList companies =  xmlDocument.SelectNodes("//Company[Table/Lock/@userName='" + userName + "']");
			if (companies == null || companies.Count == 0 )
				return;

			if (ViewUsersLocksDataGrid.DataSource == null)
				CreateTableUserLockDetails();
				
			DataTable dataGridTable = (DataTable)ViewUsersLocksDataGrid.DataSource;

			try
			{
				foreach(XmlElement companyElement in companies)
				{
					if (companyElement.ChildNodes == null) 
						continue;
					if (string.Compare(companyElement.GetAttribute("companyDBName"), companyName, true, CultureInfo.InvariantCulture) != 0) 
						continue;
					
					foreach(XmlElement tableElement in companyElement.ChildNodes)
					{
						if (tableElement.ChildNodes == null) 
							continue;
						
						foreach(XmlElement lockElement in tableElement.ChildNodes)
						{
							if (string.Compare(lockElement.GetAttribute("userName"), userName, true, CultureInfo.InvariantCulture) == 0)
							{
								DataRow newRow			= dataGridTable.NewRow();
								newRow["table"]			= tableElement.GetAttribute("name");
								newRow["date"]			= XmlConvert.ToDateTime(lockElement.GetAttribute("lockdate"), LockMngDateTimeFormat);
								newRow["application"]	= lockElement.GetAttribute("processName");
                                newRow["lockKey"]       = lockElement.GetAttribute("lockkey");
                                newRow["address"]       = lockElement.GetAttribute("address");
                                dataGridTable.Rows.Add(newRow);	
							}
						}
					}
				}
			}
			catch(Exception exc)
			{
				Debug.Fail(exc.Message);
			}
		}
		#endregion

		#region InsertDetailLocksCompaniesFromXml 
		/// <summary>
		/// InsertDetailLocksCompaniesFromXml
		/// </summary>
		//---------------------------------------------------------------------
		private void InsertDetailLocksCompaniesFromXml(string companyDBName, string tableName)
		{
			if (string.IsNullOrEmpty(companyDBName) || string.IsNullOrEmpty(tableName))
				return;

			try
			{
				aLockManager.GetLocksList(companyDBName, tableName);

				if (File.Exists(BasePathFinder.BasePathFinderInstance.GetLockLogFile()))
					xmlDocument.Load(BasePathFinder.BasePathFinderInstance.GetLockLogFile());

				XmlElement tables = xmlDocument.DocumentElement;
				if (tables == null || tables.ChildNodes.Count == 0) 
					return;

				DataTable dataGridTable = (DataTable)ViewCompaniesLocksDataGrid.DataSource;
				foreach (XmlElement lockElement in tables.ChildNodes)
				{
					DataRow newRow = dataGridTable.NewRow();
					newRow["date"]          = XmlConvert.ToDateTime(lockElement.GetAttribute("lockdate"), LockMngDateTimeFormat);
					newRow["user"]          = lockElement.GetAttribute("userName");
					newRow["application"]   = lockElement.GetAttribute("processName");
                    newRow["lockKey"]       = lockElement.GetAttribute("lockkey");
                    newRow["address"]       = lockElement.GetAttribute("address");
                    dataGridTable.Rows.Add(newRow);	
				}
			}
			catch(WebException webExc)
			{
				if (webExc.Response != null ) 
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());
			}
			catch(System.Exception exc)
			{
				Debug.Fail(exc.Message);
			}
		}
		#endregion

		#region Funzioni sul Tree della Aziende (seconda tab)
		/// <summary>
		/// treeCompaniesLocks_AfterSelect
		/// </summary>
		//---------------------------------------------------------------------
		private void treeCompaniesLocks_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if (e.Node != null)
			{
				//nodo selezionato
				PlugInTreeNode selectedNode = (PlugInTreeNode)e.Node;
				if (selectedNode == null)
					return;

				if (selectedNode.Type == "Table")
				{
					//nome azienda
					string companyName = selectedNode.Tag.ToString();
					//tabella
					string tableName = selectedNode.Text;
					if (ViewCompaniesLocksDataGrid.DataSource != null)
					{
						ViewCompaniesLocksDataGrid.DataSource = null;
						this.CreateTableCompanyLockDetails();
					}

					//popolare il grid
					InsertDetailLocksCompaniesFromXml(companyName, tableName);
				}
				else
				{
					if (ViewCompaniesLocksDataGrid.DataSource != null)
					{
						ViewCompaniesLocksDataGrid.DataSource = null;
						this.CreateTableCompanyLockDetails();
					}
				}
			}
		}

		/// <summary>
		/// treeCompaniesLocks_MouseDown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void treeCompaniesLocks_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			TreeView localTree	= (TreeView)sender;
			PlugInTreeNode nodeToSelectIfNull = (PlugInTreeNode)localTree.Nodes[0].FirstNode == null 
												? (PlugInTreeNode)localTree.Nodes[0] 
												: (PlugInTreeNode)localTree.Nodes[0].FirstNode;
			
			//se ho cliccato in una zona fuori dal tree seleziono come nodo di destinazione il primo nodo del tree
			localTree.SelectedNode	= ((PlugInTreeNode)localTree.GetNodeAt(e.X, e.Y) != null) 
									? (PlugInTreeNode)localTree.GetNodeAt(e.X, e.Y) 
									: nodeToSelectIfNull;
			PlugInTreeNode selected	= (PlugInTreeNode)localTree.SelectedNode;
			
			if (selected != null)
			{
				Type nodeType = selected.GetType();
				if ((string.Compare(nodeType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0) ||
					(string.Compare(nodeType.BaseType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0))
				{
					switch (e.Button)
					{
						case MouseButtons.Left : break;
						case MouseButtons.Right:
                            treeCompaniesLocks.ContextMenu = (selected.ContextMenu != null) ? selected.ContextMenu : null;
							break;
						case MouseButtons.Middle: break;
						case MouseButtons.None  : break;
						default                 : break;
					}
				}
			}
			else 
				treeCompaniesLocks.ContextMenu = null;
		}

		/// <summary>
		/// ViewCompaniesLocksDataGrid_MouseDown
		/// </summary>
		//---------------------------------------------------------------------
		private void ViewCompaniesLocksDataGrid_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (ViewCompaniesLocksDataGrid.DataSource == null) 
					return;

				for (int i = 0; i < ((DataTable)ViewCompaniesLocksDataGrid.DataSource).Rows.Count; i++)
					ViewCompaniesLocksDataGrid.UnSelect(i);
				
				DataGrid.HitTestInfo hit = ViewCompaniesLocksDataGrid.HitTest(e.X, e.Y);
				if (hit.Row < 0)
					return;
				
				if (ViewCompaniesLocksDataGrid.IsSelected(hit.Row))
					ViewCompaniesLocksDataGrid.UnSelect(hit.Row);
				else
					ViewCompaniesLocksDataGrid.Select(hit.Row);

				UpdateCompaniesLocksDataGridSelection();
			}
		}

		/// <summary>
		/// UpdateCompaniesLocksDataGridSelection
		/// </summary>
		//---------------------------------------------------------------------
		private void UpdateCompaniesLocksDataGridSelection()
		{
			if (ViewCompaniesLocksDataGrid.DataSource != null &&  ViewCompaniesLocksDataGrid.CurrentRowIndex >= 0)
			{
				Point ptMouse = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
				Point ptDataGridMouse =  ViewCompaniesLocksDataGrid.PointToClient(ptMouse);

				DataGrid.HitTestInfo hitTestinfo =  ViewCompaniesLocksDataGrid.HitTest(ptDataGridMouse);

				if (hitTestinfo.Type == DataGrid.HitTestType.Cell || hitTestinfo.Type == DataGrid.HitTestType.RowHeader)
				{
					ViewCompaniesLocksDataGrid.CurrentRowIndex = hitTestinfo.Row;
					ViewCompaniesLocksDataGrid.ContextMenu = contextMenuCompanyDataGrid;
					this.Refresh();
				}
			}
		}
		#endregion

		#region Funzioni sul tree della aziende
 		/// <summary>
		/// treeUsersLocks_AfterSelect
		/// </summary>
		//---------------------------------------------------------------------
		private void treeUsersLocks_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if (e.Node != null)
			{
				//nodo selezionato
				PlugInTreeNode selectedNode = (PlugInTreeNode)e.Node;
				if (selectedNode == null)
					return;

				if (selectedNode.Type == "CompanyUser")
				{
					if (selectedNode.Tag == null) 
						return;

					//utente
					string userName = selectedNode.Tag.ToString();
					//nome azienda
					string companyName = selectedNode.Text;
					if (ViewUsersLocksDataGrid.DataSource != null)
					{
						ViewUsersLocksDataGrid.DataSource = null;
						CreateTableUserLockDetails();
					}

					//popolare il grid
					InsertDetailLocksUsersFromXml(userName, companyName);
				}
				else
				{
					if (ViewUsersLocksDataGrid.DataSource != null)
					{
						ViewUsersLocksDataGrid.DataSource = null;
						CreateTableUserLockDetails();
					}
				}
			}
		}
		
		/// <summary>
		/// ViewUsersLocksDataGrid_MouseDown
		/// </summary>
		//---------------------------------------------------------------------
		private void ViewUsersLocksDataGrid_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				for (int i = 0; i < ((DataTable)ViewUsersLocksDataGrid.DataSource).Rows.Count; i++)
					ViewUsersLocksDataGrid.UnSelect(i);

				DataGrid.HitTestInfo hit = ViewUsersLocksDataGrid.HitTest(e.X, e.Y);
				if (hit.Row < 0)
					return;
				if (ViewUsersLocksDataGrid.IsSelected(hit.Row))
					ViewUsersLocksDataGrid.UnSelect(hit.Row);
				else
					ViewUsersLocksDataGrid.Select(hit.Row);
				UpdateUsersLocksDataGridSelection();
			}
		}

		/// <summary>
		/// ViewUsersLocksDataGrid_CurrentCellChanged
		/// </summary>
		//---------------------------------------------------------------------
		private void ViewUsersLocksDataGrid_CurrentCellChanged(object sender, System.EventArgs e)
		{
			UpdateUsersLocksDataGridSelection();
		}

		/// <summary>
		/// UpdateUsersLocksDataGridSelection
		/// </summary>
		//---------------------------------------------------------------------
		private void UpdateUsersLocksDataGridSelection()
		{
			if (ViewUsersLocksDataGrid.DataSource != null && ViewUsersLocksDataGrid.CurrentRowIndex >= 0)
			{
				Point ptMouse = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
				Point ptDataGridMouse = ViewUsersLocksDataGrid.PointToClient(ptMouse);

				DataGrid.HitTestInfo hitTestinfo = ViewUsersLocksDataGrid.HitTest(ptDataGridMouse);

				if (hitTestinfo.Type == DataGrid.HitTestType.Cell || hitTestinfo.Type == DataGrid.HitTestType.RowHeader)
				{
					ViewUsersLocksDataGrid.CurrentRowIndex = hitTestinfo.Row;
					ViewUsersLocksDataGrid.ContextMenu = contextMenuUserDataGrid;
					this.Refresh();
				}
			}
		}
		#endregion
	}
}