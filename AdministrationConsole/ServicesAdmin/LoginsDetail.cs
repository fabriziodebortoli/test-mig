using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Security;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

using Microarea.Console.Core.PlugIns;

namespace Microarea.Console.Plugin.ServicesAdmin
{
	/// <summary>
	/// LoginsDetail
	/// Elenco degli utente connessi all'applicazione, con la possibilità di uccidere il/i loro processi
	/// </summary>
	//=========================================================================
	public partial class LoginsDetail : System.Windows.Forms.Form
	{
		#region Variables
		//---------------------------------------------------------------------
		private Diagnostic diagnostic = new Diagnostic("LoginsDetail");
		
		private LoginManager loginManager;
		private TbServices tbServices;

		private ServicesLVItemComparer loginsLVComparer = new ServicesLVItemComparer();
		
		private bool	hiddenCompanyField	= false;
		private bool	hiddenUserField		= false;

		private bool	isEnterpriseEdition = false;
		private string	edition = string.Empty;
		private ArrayList applicationNames = new ArrayList();

		private DataTable CompaniesDataTable	= null;	// DataTable con l'elenco delle aziende (dal SysAdmin)
		private DataTable LoginsDataTable		= null;	// DataTable con l'elenco delle logins (dal SysAdmin)
		private DataTable CompanyUsersDataTable	= null;	// DataTable con l'elenco degli utenti di una azienda (dal SysAdmin)
		private DataTable OperationDataTable	= null;
		private DataTable TracedDataTable		= null;
		#endregion

		#region Events and Delegates
		// Il SysAdmin mi ritorna l'elenco delle aziende in MSD_Companies
		public delegate SqlDataReader AskAllCompaniesFromSysAdmin();
		public event AskAllCompaniesFromSysAdmin OnAskAllCompaniesFromSysAdmin;

		// Il SysAdmin mi ritorna l'elenco degli utenti presenti nella MSD_Logins
		public delegate SqlDataReader AskAllLoginsFromSysAdmin(bool localServer, string serverName);
		public event AskAllLoginsFromSysAdmin OnAskAllLoginsFromSysAdmin;

		// Il SysAdmin mi ritorna l'elenco degli utenti associati a una azienda
		public delegate SqlDataReader AskAllCompanyUsersFromSysAdmin(string companyId);
		public event AskAllCompanyUsersFromSysAdmin OnAskAllCompanyUsersFromSysAdmin;

		// Il SysAdmin mi ritorna i record tracciati
		public delegate SqlDataReader AskRecordsTracedFromSysAdmin(string companyId, string userId, TraceActionType operationType, DateTime fromDate, DateTime toDate);
		public event AskRecordsTracedFromSysAdmin OnAskRecordsTracedFromSysAdmin;

		public delegate bool DeleteRecordsTracedFromSysAdmin(DateTime ToDate);
		public event DeleteRecordsTracedFromSysAdmin OnDeleteRecordsTracedFromSysAdmin;

		// evento per chiedere alla Console l'authentication token
		public delegate string GetAuthenticationToken();
		public event GetAuthenticationToken OnGetAuthenticationToken;
		#endregion


        private int originalMsgColHeaderWidth = 0;
        private int nrEventsToShow = 300;
        private ServicesLVItemComparer listviewComparer = new ServicesLVItemComparer();

		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public LoginsDetail(LoginManager loginManager, TbServices tbServices, ImageList images)
		{
			InitializeComponent();
			this.loginManager	= loginManager;
			this.tbServices		= tbServices;
			edition	= loginManager.GetEdition();
			isEnterpriseEdition = (string.Compare(edition, NameSolverStrings.EnterpriseEdition, true, CultureInfo.InvariantCulture) == 0);
            LwLogs.SmallImageList = LwLogs.LargeImageList = images;
            originalMsgColHeaderWidth = MessageColHeader.Width;
            LwLogs.ListViewItemSorter = listviewComparer;
		}


        
		/// <summary>
		/// LoginsDetail_Load
		/// </summary>
		//---------------------------------------------------------------------
		private void LoginsDetail_Load(object sender, System.EventArgs e)
		{
			//mi posiziono sulla prima tab
			tabLogin.SelectedTab	= tabUsers;
			TitleLabel.Text			= Strings.TitleViewUsersConnected;
			SubtitleLabel.Text		= Strings.SubTitleViewUsersConnected;
            bool ent = (String.Compare(edition, Edition.Enterprise.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0);
            //se ent non mostro le cal named
            if (ent)
            {
                panelNoNamed.Location = panelNamed.Location;
                panelNamed.Visible = false;
                PanelWms.Location = new Point(PanelWms.Location.X - panelNamed.Width, PanelWms.Location.Y);
            }
			FillLogins();
		}

		/// <summary>
		/// tabLogin_SelectedIndexChanged
		/// Gestione delle Tab (Utenti Connessi, Trace di Operazioni e TbLoaders per EasyLook)
		/// </summary>
		//---------------------------------------------------------------------------
		private void tabLogin_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (((TabControl)sender).SelectedTab == tabTrace)
			{
				TitleLabel.Text		= Strings.TitleViewTrace;
				SubtitleLabel.Text	= Strings.SubTitleViewTrace;
				FillTrace();
			}
			else if (((TabControl)sender).SelectedTab == tabUsers)
			{
				TitleLabel.Text		= Strings.TitleViewUsersConnected;
				SubtitleLabel.Text	= Strings.SubTitleViewUsersConnected;
				FillLogins();
			}
            else if (((TabControl)sender).SelectedTab == tabLog)
            {
                TitleLabel.Text = Strings.TitleViewLogs;
                SubtitleLabel.Text = string.Format(Strings.OperationsTrace, NameSolverStrings.LoginManager, nrEventsToShow.ToString());
                LoadLogMessagesInListView();
            }
		}

      
		#region Funzioni per la tab delle Logins occupate
		///<summary>
		/// Leggo le logins occupate
		///</summary>
		//---------------------------------------------------------------------
		private void FillLogins()
		{
			listSlots.BeginUpdate();
			listSlots.Items.Clear();

            int namedCal = 0, gdiConcurrent = 0, unNamedCal = 0, officeCal = 0, tpCal = 0, wmscal = 0, manufacturingCal =0;
            int namedCalUsed = 0, concurrentUsed = 0, unNamedCalUsed = 0, officeCalUsed = 0, tpCalUsed = 0, wmsusedcal = 0;
            int mobileCalUsed = 0;
			try
			{
				
                namedCal = loginManager.GetCalNumber(out gdiConcurrent, out unNamedCal, out officeCal, out tpCal, out wmscal, out manufacturingCal);
			}
			catch (Exception)
			{
				return;
			}

			string loggedUserXML = string.Empty;

			try
			{
                string token = string.Empty;
                if (OnGetAuthenticationToken != null)
                    token = OnGetAuthenticationToken();

                loggedUserXML = loginManager.GetLoggedUsersAdvanced(token);
			}
			catch (Exception)
			{
				return;
			}

			if (string.IsNullOrEmpty(loggedUserXML))
				return;

			XmlDocument doc = null;

			try
			{
				doc = new XmlDocument();
				doc.LoadXml(loggedUserXML);
			}
			catch (XmlException)
			{
				return;
			}

			XmlNodeList smartClientNodes    = doc.SelectNodes("//AllUsers/SmartClient/User");
			XmlNodeList webNodes            = doc.SelectNodes("//AllUsers/Web/User");
			XmlNodeList officeNodes         = doc.SelectNodes("//AllUsers/Office/User");
			XmlNodeList tpNodes             = doc.SelectNodes("//AllUsers/ThirdParty/User");
			XmlNodeList concurrentNodes		= doc.SelectNodes("//AllUsers/Concurrent/User");
            XmlNodeList wmsNodes            = doc.SelectNodes("//AllUsers/WMS/User");
            XmlNodeList ebNodes             = doc.SelectNodes("//AllUsers/EB/User");
            XmlNodeList mobileNodes         = doc.SelectNodes("//AllUsers/Mobile/User");

			AddSlots(smartClientNodes,	Color.LightSteelBlue,	UserImages.SmartClientUser, out namedCalUsed);
			AddSlots(concurrentNodes,   Color.Khaki,            UserImages.SystemUser,      out concurrentUsed);
			AddSlots(webNodes,			Color.LightGreen,		UserImages.WebUser,			out unNamedCalUsed);
			AddSlots(officeNodes,		Color.LightSalmon,		UserImages.OfficeUser,		out officeCalUsed);
			AddSlots(tpNodes,			Color.Pink,				UserImages.ThirdPartUser,	out tpCalUsed);
           //cal mobile
            if (wmsNodes != null && wmsNodes.Count > 0) AddSlots(wmsNodes, Color.Beige, UserImages.NotWinUser, out wmsusedcal);
           //cal  invisibili dello slot skeduler ( ws mobile e scheduler) miglioria se no c'erano login fantasma non 'logoffabili'
            AddSlots(mobileNodes, Color.LightGray, UserImages.SystemUser, out mobileCalUsed);

			lblGDI.Text         = string.Format(" {0} / {1}", namedCalUsed, (namedCal == Int32.MaxValue) ? Strings.Infinite : namedCal.ToString());
			LblConc.Text        = string.Format(" {0} / {1}", concurrentUsed, (gdiConcurrent == Int32.MaxValue) ? Strings.Infinite : gdiConcurrent.ToString());
			lblWeb.Text         = string.Format(" {0} / {1}", unNamedCalUsed, (unNamedCal == Int32.MaxValue) ? Strings.Infinite : unNamedCal.ToString());
			lblOffice.Text      = string.Format(" {0} / {1}", officeCalUsed, (officeCal == Int32.MaxValue) ? Strings.Infinite : officeCal.ToString());
			lblThirdParty.Text  = string.Format(" {0} / {1}", tpCalUsed, (tpCal == Int32.MaxValue) ? Strings.Infinite : tpCal.ToString());

            if (wmsNodes != null && wmsNodes.Count > 0)
            {
                PanelWms.Visible = true;
                int wmsTotCal = wmscal + manufacturingCal;
                LblWmsCal.Text = string.Format(" {0} / {1}", wmsusedcal, (wmsTotCal == Int32.MaxValue) ? Strings.Infinite : wmsTotCal.ToString());
            }
            			
            listSlots.EndUpdate();
		}

        //--------------------------------------------------------------------------			
        ///<summary>
        /// Aggiungo righe alla listview con le CAL che risultano occupate, differenziando per colore
        ///</summary>
        private void AddSlots(XmlNodeList nodes, Color color, int imageIdx, out int usedCal)
        {
            usedCal = 0;

            //per ogni used cal però ci sono diverse funzionalità e per il mobile bisogna esplodere il conteggio
            int tempusedcal = 0;

            string userName = string.Empty;

            foreach (XmlElement node in nodes)
            {
                if (string.Compare(node.GetAttribute("logged"), "True", true, CultureInfo.InvariantCulture) == 0)
                {
                    Color tempColor = color;

                    userName = node.GetAttribute("name");

                    ListViewItem lvi = new ListViewItem();
                    lvi.ImageIndex = lvi.StateImageIndex = imageIdx;

                    string s = node.GetAttribute("inactive");
                    if (String.Compare(s, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0)
                        lvi.ImageIndex = lvi.StateImageIndex = UserImages.PurgeableUser;

                    lvi.SubItems.Add(userName);
                    lvi.SubItems.Add(node.GetAttribute("company"));

                    string st = node.GetAttribute("slotType");
                    if (st.Length == 0)
                        st = "Magic Link";
                    if (string.Compare(st, "Gdi", true, CultureInfo.InvariantCulture) == 0)
                        st = Strings.aDefault;

                    usedCal++;

                    lvi.BackColor = tempColor;
                    lvi.SubItems.Add(st);
                    string processName = node.GetAttribute("process");
                    if (String.Compare(processName, ProcessType.InvisibleWMS, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                        String.Compare(processName, ProcessType.InvisibleMAN, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                        String.Compare(processName, ProcessType.InvisibleWARMAN, StringComparison.InvariantCultureIgnoreCase) == 0)
                        processName = "Internal Process";
                    lvi.SubItems.Add(processName);
                    if (!listSlots.Columns.ContainsKey("ID"))
                        listSlots.Columns.Add("ID", "ID", 40);


                    string id = node.GetAttribute("id");
                    lvi.SubItems.Add(id);


                    XmlNodeList appTokens = node.SelectNodes("ApplicationTokens/ApplicationToken");

                    foreach (XmlElement tokenEl in appTokens)
                    {
                        string app = tokenEl.GetAttribute("application");
                        string tok = tokenEl.GetAttribute("token");

                        if (!listSlots.Columns.ContainsKey(app))
                        {
                            applicationNames.Add(app);
                            listSlots.Columns.Add(app, app, 100);
                        }

                        //l'ordinamento non è fisso devo posizionare il subitem nella colonna corretta
                        int ind = listSlots.Columns.IndexOfKey(app);
                        ListViewItem.ListViewSubItem sub = new ListViewItem.ListViewSubItem(lvi, tok.Length > 0 ? "Logged" : string.Empty);
                        int i = lvi.SubItems.Count - 1;
                        //se la colonna esiste ma il sub item non è ancora attrezzato di tutti icampi li aggiungo vuoti
                        while (ind > i)
                        {
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, string.Empty));
                            i++;
                        }
                        sub.Tag = tok;
                        sub.Name = app;
                        sub.BackColor = Color.LightGreen;
                        lvi.SubItems[ind] = sub;
                    }

                    XmlNodeList funcCal = node.SelectNodes("FunctionsCAL/FunctionCAL");

                    foreach (XmlElement functionsEl in funcCal)
                    {
                        string function = functionsEl.GetAttribute("function");
                        string usedcal = functionsEl.GetAttribute("usedcal");
                        if (String.Compare(usedcal, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0) tempusedcal++;
                        if (!listSlots.Columns.ContainsKey(function))
                            listSlots.Columns.Add(function, function, 100);

                        //l'ordinamento non è fisso devo posizionare il subitem nella colonna corretta
                        int ind = listSlots.Columns.IndexOfKey(function);
                        int i = lvi.SubItems.Count - 1;
                        //se la colonna esiste ma il sub item non è ancora attrezzato di tutti icampi li aggiungo vuoti
                        while (ind > i)
                        {
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, string.Empty));
                            i++;
                        }
                        ListViewItem.ListViewSubItem sub = new ListViewItem.ListViewSubItem(lvi, usedcal);
                        lvi.SubItems[ind] = sub;
                    }

                    if (String.Compare(processName, ProcessType.WMS, StringComparison.InvariantCultureIgnoreCase) == 0)
                        usedCal = tempusedcal;

                    listSlots.Items.Add(lvi);
                }
            }
        }        //--------------------------------------------------------------------------			
        private void LogOut(ListViewItem lvi)
		{
			ArrayList tokens = GetTokens(lvi);
			foreach (string s in tokens)
				loginManager.LogOff(s);
		}

		//--------------------------------------------------------------------------			
		private ArrayList GetTokens(ListViewItem lvi)
		{
			ArrayList result = new ArrayList();

			if (lvi == null)
			{
				MessageBox.Show(this, "Nessuna riga selezionata", this.Text);
				return result;
			}

			ArrayList list = new ArrayList();
			foreach (string name in applicationNames)
			{
				if (lvi.SubItems[name] != null && lvi.SubItems[name].Text != null && lvi.SubItems[name].Text.Length > 0)
					list.Add(name);
			}

			if (list.Count == 0)
			{
//				MessageBox.Show(this, "Nessun utente loginato allo slot numero: " + (lvi.Index + 1).ToString(), this.Text);
				return result;
			}

			if (list.Count == 1)
			{
				string val = list[0] as string;
				string token = (string)lvi.SubItems[val].Tag;
				result.Add(token);
				return result;
			}

			LoginSelectApp selectAppForm = new LoginSelectApp(list);
			DialogResult res = selectAppForm.ShowDialog(this);
			if (res == DialogResult.Cancel)
				return result;
			
			ArrayList toks = new ArrayList();
			foreach (string s in selectAppForm.SelectedApplicationList)
				toks.Add((string)lvi.SubItems[s].Tag);

			return toks;
		}

		# region Eventi sui controls
		//---------------------------------------------------------------------
		private void aggiornaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FillLogins();
		}

		//---------------------------------------------------------------------
		private void cancellaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (listSlots.SelectedItems == null ||
				listSlots.SelectedItems.Count == 0 ||
				listSlots.SelectedItems[0] == null)
			{
				DisconnectAll();
				return;
			}

			foreach (ListViewItem lvi in listSlots.SelectedItems)
				LogOut(lvi);
			
			FillLogins();
		}

		//---------------------------------------------------------------------
		private void disconnettiTuttiToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DisconnectAll();
		}

		//--------------------------------------------------------------------------			
		private void DisconnectAll()
		{
			foreach (ListViewItem lvi in listSlots.Items)
				LogOut(lvi);

			FillLogins();
		}

		//---------------------------------------------------------------------
		private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
			ContextMenuStrip cms = (ContextMenuStrip)sender;
			cms.Items[1].Visible = (listSlots.SelectedItems.Count > 0);
		}
		#endregion
	
		#endregion

		#region Funzioni per la tab di Trace
		/// <summary>
		/// FillTrace
		/// Crea e popola la tab
		/// </summary>
		//---------------------------------------------------------------------------
		private void FillTrace()
		{
			LoadAllCompanies();
			LoadAllLogins();
			LoadAllOperations();
			BuildTraceListView();
			FillTraceListView();
		}

		/// <summary>
		/// CreateCompaniesDataTable
		/// Crea la dataTable con cui verrà popolata la combo delle azienda
		/// </summary>
		//---------------------------------------------------------------------------
		private void CreateCompaniesDataTable()
		{
			CompaniesDataTable = new DataTable(ConstString.companiesTable);
			CompaniesDataTable.Columns.Add(new DataColumn(ConstString.companyId,Type.GetType("System.String")));
			CompaniesDataTable.Columns.Add(new DataColumn(ConstString.company,	Type.GetType("System.String")));
		}

		/// <summary>
		/// CreateLoginsDataTable
		/// Crea la dataTable con cui verrà popolata la combo degli utenti applicativi
		/// </summary>
		//---------------------------------------------------------------------------
		private void CreateLoginsDataTable()
		{
			LoginsDataTable = new DataTable(ConstString.loginsTable);
			LoginsDataTable.Columns.Add(new DataColumn(ConstString.loginId,	Type.GetType("System.String")));
			LoginsDataTable.Columns.Add(new DataColumn(ConstString.login,	Type.GetType("System.String")));
		}

		/// <summary>
		/// CreateCompanyUsersDataTable
		/// Crea la dataTable con cui verrà popolata la combo degli utenti associati
		/// </summary>
		//---------------------------------------------------------------------------
		private void CreateCompanyUsersDataTable()
		{
			CompanyUsersDataTable = new DataTable(ConstString.companyUsersTable);
			CompanyUsersDataTable.Columns.Add(new DataColumn(ConstString.loginId,	Type.GetType("System.String")));
			CompanyUsersDataTable.Columns.Add(new DataColumn(ConstString.login,		Type.GetType("System.String")));
		}

		/// <summary>
		/// CreateOperationsDataTable
		/// Crea la dataTable con cui verrà popolata la combo della tipologie di applicazioni
		/// </summary>
		//---------------------------------------------------------------------------
		private void CreateOperationsDataTable()
		{
			OperationDataTable = new DataTable(ConstString.operationTable);
			OperationDataTable.Columns.Add(new DataColumn(ConstString.operationDescription, Type.GetType("System.String")));
			OperationDataTable.Columns.Add(new DataColumn(ConstString.operationId, Enum.GetUnderlyingType(typeof(TraceActionType))));
		}
		
		/// <summary>
		/// BuildTraceListView
		/// Costruisco la listView in cui far vedere i dati di tracciatura estratti
		/// </summary>
		//---------------------------------------------------------------------------
		private void BuildTraceListView()
		{
			TraceListView.Columns.Clear();
			TraceListView.Columns.Add(Strings.User,				-2,HorizontalAlignment.Left);
			TraceListView.Columns.Add(Strings.Company,			-2,HorizontalAlignment.Left);
			TraceListView.Columns.Add(Strings.DateAndTime,		-2,HorizontalAlignment.Left);
			TraceListView.Columns.Add(Strings.ApplicationHeader,-2,HorizontalAlignment.Left);
			TraceListView.Columns.Add(Strings.Operation,		-2,HorizontalAlignment.Left);
			TraceListView.Columns.Add(Strings.WinUser,			-2,HorizontalAlignment.Left);
			TraceListView.Columns.Add(Strings.ComputerName,		-2,HorizontalAlignment.Left);
			TraceListView.TabStop				= false;
			TraceListView.View					= View.Details;
			TraceListView.Dock					= DockStyle.Fill;
			TraceListView.AllowColumnReorder	= true;
			TraceListView.LargeImageList		= TraceListView.SmallImageList = this.imageUsers;

			TraceListView.ListViewItemSorter = loginsLVComparer;
		}

		/// <summary>
		/// LoadAllCompanies
		/// Carico le aziende presenti nella combo (le chiedo al SysAdmin)
		/// </summary>
		//---------------------------------------------------------------------------
		private void LoadAllCompanies()
		{
			ComboBoxCompanies.DataSource = null;
			ComboBoxCompanies.Items.Clear();
			
			CreateCompaniesDataTable();
			DataRow row	= CompaniesDataTable.NewRow();
			row[ConstString.companyId]	= "0";
			row[ConstString.company]	= Strings.AllElements;
			CompaniesDataTable.Rows.Add(row);
			CompaniesDataTable.AcceptChanges();

			if (OnAskAllCompaniesFromSysAdmin != null)
			{
				SqlDataReader reader = OnAskAllCompaniesFromSysAdmin();
				if (reader != null)
				{
					while (reader.Read())
					{
						row	= CompaniesDataTable.NewRow();
						row[ConstString.companyId]	= reader[ConstString.companyId].ToString();
						row[ConstString.company]	= reader[ConstString.company].ToString();
						CompaniesDataTable.Rows.Add(row);
					}
					CompaniesDataTable.AcceptChanges();
					reader.Close(); 
				}
			}

			ComboBoxCompanies.SelectedIndexChanged -= new EventHandler(ComboBoxCompanies_SelectedIndexChanged);

			ComboBoxCompanies.DataSource	= CompaniesDataTable;
			ComboBoxCompanies.DisplayMember	= ConstString.company;
			ComboBoxCompanies.ValueMember	= ConstString.companyId;

			ComboBoxCompanies.SelectedIndexChanged += new EventHandler(ComboBoxCompanies_SelectedIndexChanged);
			ComboBoxCompanies.SelectedIndex = 0;
		}

		/// <summary>
		/// LoadAllLogins
		/// Carico gli utenti applicativi nella combo (li chiedo al Sysadmin)
		/// </summary>
		//---------------------------------------------------------------------------
		private void LoadAllLogins()
		{
			ComboBoxLogins.DataSource = null;
			ComboBoxLogins.Items.Clear();

			CreateLoginsDataTable();
			DataRow row	= LoginsDataTable.NewRow();
			row[ConstString.loginId]= "0";
			row[ConstString.login]	= Strings.AllElements;
			LoginsDataTable.Rows.Add(row);
			LoginsDataTable.AcceptChanges();

			if (OnAskAllLoginsFromSysAdmin != null)
			{
				//stabilisco se si tratta di un server remoto o locale
				bool isLocalServer = BasePathFinder.BasePathFinderInstance.RunAtServer;

				SqlDataReader reader = OnAskAllLoginsFromSysAdmin(isLocalServer, BasePathFinder.BasePathFinderInstance.RemoteFileServer);
				if (reader != null)
				{
					while (reader.Read())
					{
						row	= LoginsDataTable.NewRow();
						row[ConstString.loginId]	= reader[ConstString.loginId].ToString();
						row[ConstString.login]		= reader[ConstString.login].ToString();
						LoginsDataTable.Rows.Add(row);
					}
					LoginsDataTable.AcceptChanges();
					reader.Close(); 
				}
			}

			ComboBoxLogins.SelectedIndexChanged -= new EventHandler(ComboBoxLogins_SelectedIndexChanged);

			ComboBoxLogins.DataSource		= LoginsDataTable;
			ComboBoxLogins.DisplayMember	= ConstString.login;
			ComboBoxLogins.ValueMember		= ConstString.loginId;
			ComboBoxLogins.SelectedIndex	= 0;

			ComboBoxLogins.SelectedIndexChanged += new EventHandler(ComboBoxLogins_SelectedIndexChanged);
		}

		/// <summary>
		/// LoadAllCompanyLogins
		/// Carico gli utenti associati all'azienda specificata dal companyId (li chiedo al Sysadmin)
		/// </summary>
		//---------------------------------------------------------------------------
		private void LoadAllCompanyLogins(string companyId)
		{
			ComboBoxLogins.DataSource = null;
			ComboBoxLogins.Items.Clear();
			
			CreateCompanyUsersDataTable();
			DataRow row	= CompanyUsersDataTable.NewRow();
			row[ConstString.loginId]= "0";
			row[ConstString.login]	= Strings.AllElements;
			CompanyUsersDataTable.Rows.Add(row);
			CompanyUsersDataTable.AcceptChanges();
			
			if (this.OnAskAllCompanyUsersFromSysAdmin != null)
			{
				SqlDataReader reader = OnAskAllCompanyUsersFromSysAdmin(companyId);
				if (reader != null)
				{
					while (reader.Read())
					{
						row	= CompanyUsersDataTable.NewRow();
						row[ConstString.loginId]= reader[ConstString.loginId].ToString();
						row[ConstString.login]	= reader[ConstString.login].ToString();
						CompanyUsersDataTable.Rows.Add(row);
					}
					CompanyUsersDataTable.AcceptChanges();
					reader.Close(); 
				}
			}

			ComboBoxLogins.SelectedIndexChanged -= new EventHandler(ComboBoxLogins_SelectedIndexChanged);

			ComboBoxLogins.DataSource		= CompanyUsersDataTable;
			ComboBoxLogins.DisplayMember	= ConstString.login;
			ComboBoxLogins.ValueMember		= ConstString.loginId;
			ComboBoxLogins.SelectedIndex	= 0;

			ComboBoxLogins.SelectedIndexChanged += new EventHandler(ComboBoxLogins_SelectedIndexChanged);

			FillTraceListView();
		}

		/// <summary>
		/// LoadAllOperations
		/// Carico tutti i tipi di operazione che tracciamo (enumerativi in Generic)
		/// </summary>
		//---------------------------------------------------------------------------
		private void LoadAllOperations()
		{
			ComboBoxOperations.DataSource = null;
			int selectedIndex	= -1;
			ComboBoxOperations.Items.Clear();

			CreateOperationsDataTable();
			
			foreach (int i in Enum.GetValues(typeof(TraceActionType)))
			{
				DataRow row	= OperationDataTable.NewRow();
				row[ConstString.operationId]			= i;
				row[ConstString.operationDescription]	= TraceActionName.GetTraceVersionName( (TraceActionType) (Enum.GetValues(typeof(TraceActionType)).GetValue(i)) );
				if (Enum.GetName(typeof(TraceActionType), i) == TraceActionType.All.ToString())
					selectedIndex = i;
				OperationDataTable.Rows.Add(row);
				OperationDataTable.AcceptChanges();
			}

			ComboBoxOperations.SelectedIndexChanged -= new EventHandler(ComboBoxOperations_SelectedIndexChanged);
			
			ComboBoxOperations.DataSource		= OperationDataTable;
			ComboBoxOperations.DisplayMember	= ConstString.operationDescription;
			ComboBoxOperations.ValueMember		= ConstString.operationId;
			ComboBoxOperations.SelectedIndex	= selectedIndex;

			ComboBoxOperations.SelectedIndexChanged += new EventHandler(ComboBoxOperations_SelectedIndexChanged);
		}
		
		/// <summary>
		/// FillTraceListView
		/// Popola la listView con i dati di tracciatura estratti in base ai criteri
		/// di ricerca impostati (chiedo i dati al sysadmin)
		/// </summary>
		//---------------------------------------------------------------------------
		private void FillTraceListView()
		{
			hiddenCompanyField	= false;
			hiddenUserField		= false;
			string fullDateTimePattern = Thread.CurrentThread.CurrentCulture.DateTimeFormat.FullDateTimePattern;
			
			if (ComboBoxCompanies.SelectedItem	== null || 
				ComboBoxLogins.SelectedItem		== null || 
				ComboBoxOperations.SelectedItem == null)
			{
				EnableButtons(false);
				return;
			}

			SqlDataReader reader = null;

			try
			{
				TraceListView.BeginUpdate();
				TraceListView.Items.Clear();

				//criteri di ricerca
				string companySelected	= ((DataRowView)ComboBoxCompanies.SelectedItem).Row[ConstString.company].ToString();
				string userSelected		= ((DataRowView)ComboBoxLogins.SelectedItem).Row[ConstString.login].ToString();
				long l = long.Parse(((DataRowView)ComboBoxOperations.SelectedItem).Row[ConstString.operationId].ToString());
				
				TraceActionType operationType = (TraceActionType) ( Enum.GetValues(typeof(TraceActionType)).GetValue(l));
				
				DateTime fromDateSelected	= FromDate.Value;
				DateTime toDateSelected		= ToDate.Value;

				if (OnAskRecordsTracedFromSysAdmin != null)
				{
					reader = OnAskRecordsTracedFromSysAdmin
						(
						companySelected, 
						userSelected, 
						operationType, 
						fromDateSelected, 
						toDateSelected
						);

					if (reader != null)
					{
						while (reader.Read())
						{
							ListViewItem currentRowView = new ListViewItem();
							currentRowView.Text = reader[ConstString.login].ToString();
							currentRowView.SubItems.Add(reader[ConstString.company].ToString());
							currentRowView.SubItems.Add(XmlConvert.ToString((DateTime)reader[ConstString.operationDate], fullDateTimePattern));
							currentRowView.SubItems.Add(reader[ConstString.applicationName].ToString());
							currentRowView.SubItems.Add(TraceActionName.GetTraceVersionName( (TraceActionType) ( Enum.GetValues(typeof(TraceActionType)).GetValue( Convert.ToUInt16(reader[ConstString.operationType].ToString())))));
							currentRowView.SubItems.Add(reader[ConstString.WinUser].ToString());
							currentRowView.SubItems.Add(reader[ConstString.Location].ToString());
							//imposto immagine
							if (reader[ConstString.login].ToString().IndexOf(System.IO.Path.DirectorySeparatorChar) == -1)
								currentRowView.ImageIndex = currentRowView.StateImageIndex = UserImages.NotWinUser;
							else
								currentRowView.ImageIndex = currentRowView.StateImageIndex = UserImages.WinUser;
							TraceListView.Items.Add(currentRowView);
						}
						reader.Close();
					}
				}

				if (operationType == TraceActionType.ChangePassword ||
					operationType == TraceActionType.ChangePasswordFailed ||
					operationType == TraceActionType.DeleteUser)
					hiddenCompanyField = true;

				if (operationType == TraceActionType.DeleteCompany)
					hiddenUserField = true;
	
				EnableButtons(TraceListView.Items.Count != 0);
				TraceListView_Resize(this, new EventArgs());

				TraceListView.EndUpdate();
			}
			catch(System.InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message, invalidExc.InnerException.Message);
				diagnostic.Set(DiagnosticType.Error, invalidExc.Message);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				diagnostic.Clear();
				if (reader != null)
					reader.Close();
			}
			catch(Exception exc)
			{
				Debug.Fail(exc.Message, exc.InnerException.Message);
				diagnostic.Set(DiagnosticType.Error, exc.Message);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				diagnostic.Clear();
				if (reader != null)
					reader.Close();
			}
		}

		/// <summary>
		/// CreateTracedDataTable
		/// Crea la dataTable per l'esportazione in XML dei dati in MSD_Trace
		/// </summary>
		//---------------------------------------------------------------------------
		private void CreateTracedDataTable()
		{
			TracedDataTable = new DataTable(ConstString.traceTable);
			TracedDataTable.Columns.Add(new DataColumn(ConstString.company, Type.GetType("System.String")));
			TracedDataTable.Columns.Add(new DataColumn(ConstString.login, Type.GetType("System.String")));
			TracedDataTable.Columns.Add(new DataColumn(ConstString.operationDate, Type.GetType("System.DateTime")));
			TracedDataTable.Columns.Add(new DataColumn(ConstString.operationType, Type.GetType("System.UInt16")));
			TracedDataTable.Columns.Add(new DataColumn(ConstString.operationDescription, Type.GetType("System.String")));
			TracedDataTable.Columns.Add(new DataColumn(ConstString.applicationName, Type.GetType("System.String")));
			TracedDataTable.Columns.Add(new DataColumn(ConstString.WinUser, Type.GetType("System.String")));
			TracedDataTable.Columns.Add(new DataColumn(ConstString.Location, Type.GetType("System.String")));
		}

		/// <summary>
		/// FillTracedDataTable
		/// Popolo la tabella di tracciatura per l'esportazione XML
		/// </summary>
		//---------------------------------------------------------------------------
		private void FillTracedDataTable()
		{
			string fullDateTimePattern = Thread.CurrentThread.CurrentCulture.DateTimeFormat.FullDateTimePattern;
			string companySelected = ((DataRowView)ComboBoxCompanies.SelectedItem).Row[ConstString.company].ToString();
			string userSelected	= ((DataRowView)ComboBoxLogins.SelectedItem).Row[ConstString.login].ToString();
			long l = long.Parse(((DataRowView)ComboBoxOperations.SelectedItem).Row[ConstString.operationId].ToString());

			TraceActionType operationType = (TraceActionType)(Enum.GetValues(typeof(TraceActionType)).GetValue(l));
			DateTime fromDateSelected	= FromDate.Value;
			DateTime toDateSelected		= ToDate.Value;

			if (OnAskRecordsTracedFromSysAdmin != null)
			{
				SqlDataReader reader = OnAskRecordsTracedFromSysAdmin
					(
					companySelected,
					userSelected,
					operationType,
					fromDateSelected,
					toDateSelected
					);

				while (reader != null && reader.Read())
				{
					DataRow row								= TracedDataTable.NewRow();
					row[ConstString.company]				= reader[ConstString.company].ToString();
					row[ConstString.login]					= reader[ConstString.login].ToString();
					row[ConstString.operationDate]			= (DateTime)reader[ConstString.operationDate];
					string typeOperation					= reader[ConstString.operationType].ToString();
					row[ConstString.operationType]			= Convert.ToUInt16(typeOperation);
					row[ConstString.operationDescription]	= TraceActionName.GetTraceVersionName((TraceActionType)(Enum.GetValues(typeof(TraceActionType)).GetValue(Convert.ToUInt16(typeOperation))));
					row[ConstString.WinUser]				= reader[ConstString.WinUser].ToString();
					row[ConstString.Location]				= reader[ConstString.Location].ToString();
					TracedDataTable.Rows.Add(row);
				}
				reader.Close();
				TracedDataTable.AcceptChanges();
			}
		}

		/// <summary>
		/// WriteXmlTraceFile
		/// Scrittura su file dei dati di tracciatura in formato XML
		/// </summary>
		//---------------------------------------------------------------------------
		private bool WriteXmlTraceFile(string filePath)
		{
			bool result	= true;

			DataSet tempDataSet	= new DataSet("TraceRecords");
			StreamWriter sWriter= new StreamWriter(filePath);

			try
			{
				CreateTracedDataTable();
				FillTracedDataTable();
				if (TracedDataTable.Rows.Count > 0)
				{
					tempDataSet.Tables.Add(TracedDataTable);
					tempDataSet.WriteXml(sWriter);
				}
				else
					diagnostic.Set(DiagnosticType.Warning, Strings.NoTracedRecords);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				diagnostic.Set(DiagnosticType.Error, exc.Message);
				result = false;
			}
			finally
			{
				sWriter.Close();
				tempDataSet.Dispose();
			}
			return result;
		}

		/// <summary>
		/// EnableButtons
		/// Abilito o disabilito i bottoni di esportazione e cancellazione
		/// </summary>
		//---------------------------------------------------------------------------
		private void EnableButtons(bool enable)
		{
			BtnXmlExport.Enabled = enable;
			BtnDeleteAll.Enabled = enable;
		}

		# region Eventi sui controls
		/// <summary>
		/// TraceListView_Resize
		/// Ricalcolo la larghezza della listView (ma non dei campi che ho deciso di nascondere e che hanno la width=0)
		/// </summary>
		//---------------------------------------------------------------------------
		private void TraceListView_Resize(object sender, System.EventArgs e)
		{
			if (TraceListView.Columns.Count > 0)
			{
				for (int i = 0; i < TraceListView.Columns.Count; i++)
				{
					if (((string.Compare(TraceListView.Columns[i].Text, Strings.User, true, CultureInfo.InvariantCulture) == 0) 
						&& hiddenUserField) ||
						((string.Compare(TraceListView.Columns[i].Text, Strings.Company, true, CultureInfo.InvariantCulture) == 0) 
						&& hiddenCompanyField))
						TraceListView.Columns[i].Width = 0;
					else
						TraceListView.Columns[i].Width = -2;
				}
			}
		}

		/// <summary>
		/// BtnDeleteAll_Click
		/// Cancellazione dei dati di tracciatura
		/// </summary>
		//---------------------------------------------------------------------------
		private void BtnDeleteAll_Click(object sender, System.EventArgs e)
		{
			DeleteTraceRecords deleteTraceRecors = new DeleteTraceRecords();
			DialogResult result = deleteTraceRecors.ShowDialog();
			if (result == DialogResult.Cancel)
				return;

			//cancellazione 
			if (OnDeleteRecordsTracedFromSysAdmin != null)
			{
				OnDeleteRecordsTracedFromSysAdmin(deleteTraceRecors.SelectedToDate);	
				//ricarico i dati
				FillTraceListView();
			}
		}

		/// <summary>
		/// BtnXmlExport_Click
		/// Esportazione dei dati di tracciatura in formato XML
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------------
		private void BtnXmlExport_Click(object sender, System.EventArgs e)
		{
			diagnostic.Clear();
			DialogResult result = MessageBox.Show
				(
				Strings.AskIfSaveTraceXmlFormat,
				Strings.TitleXmlTraceExport,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1
				);

			if (result == DialogResult.Yes)
			{
				SaveFileDialog saveXmlTraceFile		= new SaveFileDialog();
				saveXmlTraceFile.CheckPathExists	= true;
				saveXmlTraceFile.Title				= Strings.TitleXmlTraceExport;
				saveXmlTraceFile.DefaultExt			= "*" + NameSolverStrings.XmlExtension;
				saveXmlTraceFile.Filter				= "XML File|*" + NameSolverStrings.XmlExtension;
				saveXmlTraceFile.OverwritePrompt	= true;

				if (saveXmlTraceFile.ShowDialog() == DialogResult.OK &&	saveXmlTraceFile.FileName.Length > 0)
				{
					bool saved = WriteXmlTraceFile(saveXmlTraceFile.FileName);
					if (saved)
						diagnostic.Set(DiagnosticType.Information, Strings.SuccessXmlSaved);
					else
						diagnostic.Set(DiagnosticType.Error, Strings.SavingTracedData);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
				}
			}
		}

		/// <summary>
		/// ComboBoxCompanies_SelectedIndexChanged
		/// Se seleziono una azienda, le logins visualizzate saranno quelle degli utenti
		/// associati a tale azienda: se seleziono (tutti), le logins saranno quelle degli
		/// utenti applicativi
		/// </summary>
		//---------------------------------------------------------------------------
		private void ComboBoxCompanies_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (ComboBoxCompanies.SelectedItem != null)
			{
				TraceListView.BeginUpdate();
				ComboBoxCompanies.BeginUpdate();
				ComboBoxLogins.BeginUpdate();
				string companyId = ((DataRowView)ComboBoxCompanies.SelectedItem).Row[ConstString.companyId].ToString();
				string company   = ((DataRowView)ComboBoxCompanies.SelectedItem).Row[ConstString.company].ToString();

				if (string.Compare(company, Strings.AllElements, true, CultureInfo.InvariantCulture) == 0)
				{
					LoadAllLogins();
					FillTraceListView();
				}
				else
					LoadAllCompanyLogins(companyId);

				ComboBoxLogins.EndUpdate();
				ComboBoxCompanies.EndUpdate();
				TraceListView.EndUpdate();
			}
		}

		/// <summary>
		/// ComboBoxLogins_SelectedIndexChanged
		/// Ho selezionato un utente, ricarico i dati nella listView
		/// </summary>
		//---------------------------------------------------------------------------
		private void ComboBoxLogins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (ComboBoxLogins.SelectedItem != null)
				FillTraceListView();
		}

		/// <summary>
		/// ComboBoxOperations_SelectedIndexChanged
		/// Ho selezionato una operazione, ricarico i dati nella listview
		/// </summary>
		//---------------------------------------------------------------------------
		private void ComboBoxOperations_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (ComboBoxOperations.SelectedItem != null)
				FillTraceListView();
		}

		/// <summary>
		/// FromDate_ValueChanged
		/// Check sul range della data
		/// </summary>
		//---------------------------------------------------------------------------
		private void FromDate_ValueChanged(object sender, System.EventArgs e)
		{
			ToDate.MinDate = ((DateTimePicker)sender).Value;
		}

		/// <summary>
		/// ToDate_ValueChanged
		/// Check sul range della data
		/// </summary>
		//---------------------------------------------------------------------------
		private void ToDate_ValueChanged(object sender, System.EventArgs e)
		{
			FromDate.MaxDate = ((DateTimePicker)sender).Value;
		}

		/// <summary>
		/// FromDate_CloseUp
		/// Sulla chiusura del controllo ricarico i dati nella list-view
		/// </summary>
		//---------------------------------------------------------------------------
		private void FromDate_CloseUp(object sender, System.EventArgs e)
		{
			FillTraceListView();
		}

		/// <summary>
		/// ToDate_CloseUp
		/// Sulla chiusura del controllo ricarico i dati nella list-view
		/// </summary>
		//---------------------------------------------------------------------------
		private void ToDate_CloseUp(object sender, System.EventArgs e)
		{
			FillTraceListView();
		}

		/// <summary>
		/// TraceListView_ColumnClick
		/// Utilizzato per il sort di ogni singola colonna
		/// </summary>
		//---------------------------------------------------------------------------
		private void TraceListView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			// Determine if clicked column is already the column that is being sorted.
			if (e.Column == loginsLVComparer.SortColumn)
			{
				// Reverse the current sort direction for this column.
				if (loginsLVComparer.Order == System.Windows.Forms.SortOrder.Ascending)
					loginsLVComparer.Order = System.Windows.Forms.SortOrder.Descending;
				else
					loginsLVComparer.Order = System.Windows.Forms.SortOrder.Ascending;
			}
			else
			{
				// Set the column number that is to be sorted; default to ascending.
				loginsLVComparer.SortColumn = e.Column;
				loginsLVComparer.Order = System.Windows.Forms.SortOrder.Ascending;
			}

			// Perform the sort with these new sort options.
			TraceListView.Sort();
		}
		# endregion
		#endregion

        ///<summary>
        /// Caricamento info dall'EventLog di sistema
        ///</summary>
        //--------------------------------------------------------------------------------
        private void LoadLogMessagesInListView()
        {
            LwLogs.Items.Clear();

            // prima controllo che l'EventLog esista
            if (!CheckLoginManagerEventLog())
            {
                ListViewItem emptyItem = new ListViewItem(EventLogEntryType.Warning.ToString());
                emptyItem.ImageIndex = PlugInTreeNode.GetWarningStateImageIndex;
                emptyItem.SubItems.Add(Strings.EventLogUnavailable);
                LwLogs.Items.Add(emptyItem);
                return;
            }

            EventLog theEventLog = new EventLog();
            theEventLog.Log = Diagnostic.EventLogName;
            theEventLog.Source = NameSolverStrings.LoginManager;

            // se non ci sono entries non procedo
            if (theEventLog.Entries == null || theEventLog.Entries.Count == 0)
            {
                ListViewItem emptyItem = new ListViewItem(EventLogEntryType.Warning.ToString());
                emptyItem.ImageIndex = PlugInTreeNode.GetWarningStateImageIndex;
                emptyItem.SubItems.Add(String.Format(Strings.NoMsgAvailable, NameSolverStrings.LoginManager));
                LwLogs.Items.Add(emptyItem);
                return;
            }

            LwLogs.BeginUpdate();

            int count = 0;

            Diagnostic currDiagnostic = null;
            DiagnosticType currDiagnosticType = DiagnosticType.Information;
            string explain = string.Empty;

            // gli eventi risultano ordinati cronologicamente dal più vecchio al più recente, quindi li scorro dal fondo
            for (int i = theEventLog.Entries.Count - 1; i >= 0; i--)
            {
                EventLogEntry entry = theEventLog.Entries[i];

                // se il testo NON inizia con l'istanza corrente di installazione continuo (altrimenti vedo tutti i log insieme)
                if (entry == null ||
                    !entry.Message.StartsWith(InstallationData.InstallationName, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                // considero solo gli eventi di LoginManager/ activation object
                if (string.Compare(entry.Source, NameSolverStrings.LoginManager, StringComparison.InvariantCultureIgnoreCase) == 0
                    ||
                    string.Compare(entry.Source, NameSolverStrings.ActivationObject, StringComparison.InvariantCultureIgnoreCase) == 0
                   )
                {
                    count++;
                    if (count > nrEventsToShow) // visualizzo solo i primi n messaggi
                        break;

                    ListViewItem entryItem = new ListViewItem(entry.EntryType.ToString());

                    // assegno un'immagine a seconda del tipo messaggio
                    switch (entry.EntryType)
                    {
                        case EventLogEntryType.Information:
                            entryItem.ImageIndex = PlugInTreeNode.GetInformationStateImageIndex;
                            currDiagnosticType = DiagnosticType.Information;
                            break;
                        case EventLogEntryType.Error:
                            entryItem.ImageIndex = PlugInTreeNode.GetErrorStateImageIndex;
                            currDiagnosticType = DiagnosticType.Error;
                            break;
                        case EventLogEntryType.Warning:
                            entryItem.ImageIndex = PlugInTreeNode.GetWarningStateImageIndex;
                            currDiagnosticType = DiagnosticType.Warning;
                            break;
                    }

                    explain = ReplaceInvalidMessageCharacters(entry.Message);
                    entryItem.SubItems.Add(explain);
                    entryItem.SubItems.Add(entry.TimeGenerated.ToShortDateString());
                    entryItem.SubItems.Add(entry.TimeGenerated.ToShortTimeString());

                    // creo un Diagnostico da assegnare al Tag del corrente ListViewItem
                    currDiagnostic = new Diagnostic(NameSolverStrings.LoginManager);
                    currDiagnostic.Set(currDiagnosticType, entry.TimeGenerated, explain);
                    entryItem.Tag = currDiagnostic;

                    LwLogs.Items.Add(entryItem);
                }
            }

            AdjustColumnsWidth();

            LwLogs.EndUpdate();
        }

        ///<summary>
        /// Sostituisce i caratteri non validi nel testo del messaggio
        ///</summary>
        //-------------------------------------------------------------------------------------------
        private string ReplaceInvalidMessageCharacters(string aText)
        {
            if (string.IsNullOrWhiteSpace(aText))
                return String.Empty;

            string newText = aText.Trim();
            newText = newText.Replace("\r\n", " ");
            newText = newText.Replace('\n', ' ');
            newText = newText.Replace('\t', ' ');
            newText = newText.Replace('\v', ' ');

            return newText;
        }

        ///<summary>
        /// Effettua un aggiustamento della larghezza delle colonne sulla base dell'area disponibile
        ///</summary>
        //-------------------------------------------------------------------------------------------
        private void AdjustColumnsWidth()
        {
            if (LwLogs.Columns == null || LwLogs.Columns.Count == 0 || MessageColHeader == null)
                return;

            int colswidth = 0;
            for (int i = 0; i < LwLogs.Columns.Count; i++)
                colswidth += LwLogs.Columns[i].Width;

            if (colswidth == LwLogs.DisplayRectangle.Width)
                return;

            int newMsgColumnWidth = MessageColHeader.Width + LwLogs.DisplayRectangle.Width - colswidth;
            newMsgColumnWidth = Math.Max(originalMsgColHeaderWidth, newMsgColumnWidth);

            if (newMsgColumnWidth != MessageColHeader.Width)
            {
                MessageColHeader.Width = newMsgColumnWidth;
                LwLogs.PerformLayout();
            }
        }

        //-------------------------------------------------------------------------------------------
        private bool CheckLoginManagerEventLog()
        {
            try
            {
                // The Source can be any random string, but the name must be distinct from other
                // sources on the computer. It is common for the source to be the name of the 
                // application or another identifying string. An attempt to create a duplicated 
                // Source value throws an exception. However, a single event log can be associated
                // with multiple sources.
                if (EventLog.SourceExists(NameSolverStrings.LoginManager))
                {
                    string existingLogName = EventLog.LogNameFromSourceName(NameSolverStrings.LoginManager, ".");
                    if (String.Compare(Diagnostic.EventLogName, existingLogName, StringComparison.InvariantCultureIgnoreCase) == 0)
                        return true;

                    EventLog.DeleteEventSource(NameSolverStrings.LoginManager);
                }
                EventLog.CreateEventSource(NameSolverStrings.LoginManager, Diagnostic.EventLogName);

                if (!EventLog.Exists(Diagnostic.EventLogName))
                    return false;

                return true;
            }
            catch (SecurityException se)
            {
                Debug.Fail("EventSource creation failed in CheckLoginManagerEventLog: " + se.Message);
                return false;
            }
            catch (Exception exception)
            {
                Debug.Fail("EventSource creation failed in CheckLoginManagerEventLog: " + exception.Message);
                return false;
            }
        }

        ///<summary>
        /// Sul doppio click di una riga della listview mostro l'eventuale Diagnostic memorizzato nel Tag
        ///</summary>
        //---------------------------------------------------------------------------
        private void LoginManagerLogListView_DoubleClick(object sender, EventArgs e)
        {
            ListView list = (ListView)sender;

            if (list.SelectedItems == null ||
                list.SelectedItems.Count == 0
                || list.SelectedItems.Count > 1)
                return;

            ListViewItem item = list.SelectedItems[0];
            ShowDiagnosticOnListViewItem(item);
        }

        /// <summary>
        /// mostro un Diagnostico relativo all'item selezionato nella list view
        /// </summary>
        //---------------------------------------------------------------------------
        private void ShowDiagnosticOnListViewItem(ListViewItem currItem)
        {
            if (currItem != null && currItem.Tag != null)
                DiagnosticViewer.ShowDiagnostic(((Diagnostic)(currItem.Tag)));
        }

        ///<summary>
        /// Dopo il resize della listview e ri-aggiusto la larghezza delle colonne
        ///</summary>
        //--------------------------------------------------------------------------------
        private void LoginManagerLogListView_Resize(object sender, EventArgs e)
        {
            AdjustColumnsWidth();
        }

        ///<summary>
        /// Intercetto l'F5 sulla listview e ricarico le righe
        ///</summary>
        //--------------------------------------------------------------------------------
        private void LoginManagerLogListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5 && e.Modifiers == Keys.None)
                LoadLogMessagesInListView();
        }

        //--------------------------------------------------------------------------------
        private void LoginManagerLogListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == listviewComparer.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (listviewComparer.Order == System.Windows.Forms.SortOrder.Ascending)
                    listviewComparer.Order = System.Windows.Forms.SortOrder.Descending;
                else
                    listviewComparer.Order = System.Windows.Forms.SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                listviewComparer.SortColumn = e.Column;
                listviewComparer.Order = System.Windows.Forms.SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            LwLogs.Sort();

            // consente di visualizzare la freccina nel columnheader
            //LwLogs.SetSortIcon(e.Column, listviewComparer.Order);
        }

        ///<summary>
        /// Clicco sul menu di contesto per eseguire il ricaricamento dei messaggi dell'EventViewer
        ///</summary>
        //--------------------------------------------------------------------------------
        private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadLogMessagesInListView();
        }

        //--------------------------------------------------------------------------------
        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // prima controllo che l'EventLog esista
            if (!CheckLoginManagerEventLog())
                return;

            EventLog theEventLog = new EventLog();
            theEventLog.Log = Diagnostic.EventLogName;
            theEventLog.Source = NameSolverStrings.LoginManager;
           
            // carico gli ultimi messaggi
            LoadLogMessagesInListView();

            // se non ci sono entries non procedo
            if (theEventLog.Entries == null || theEventLog.Entries.Count == 0)
                return;

            if (MessageBox.Show(Strings.WarnClearEntriesInEventLog, string.Empty, MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            try
            {
                theEventLog.Clear();//CANCELLA TUTTI gli entries MASERVER...
            }
            catch
            { }

            LwLogs.BeginUpdate();
            LwLogs.Items.Clear();
            LwLogs.EndUpdate();
        }
	}

	/// <summary>
	/// UserImages
	/// Elenco di immagini per la listView nella tab di Trace e nella tab degli Utenti Connessi
	/// </summary>
	//=========================================================================
	public class UserImages
	{
		public const int WinUser		= 0;
		public const int NotWinUser		= 1;
		public const int SmartClientUser= 2; // cal GDI
		public const int WebUser		= 3; // cal EasyLook
		public const int OfficeUser		= 4; // cal di MagicDocuments
		public const int ThirdPartUser	= 5; // cal per processi di terze parti
		public const int SystemUser		= 6; // cal per processi di tipo System
        public const int PurgeableUser = 7; // cal purgabili
    }
}