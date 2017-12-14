using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;

namespace Microarea.Web.EasyLook
{
	/// <summary>
	/// Pagina che mostra in maniera dettagliata i documenti aperti sul server
	/// Nel caso di utente amministratore mostra anche i documenti aperti degli altri utenti 
	/// (puo' chiuderli o vederne uno snapshot)
	/// </summary>
	public partial class DocumentsDetailForm : System.Web.UI.Page
	{

		#region Data Member
		//stringhe usate per controllare quali elementi html hanno fatto il postback della pagina 
		const string CLOSE_BTN_ID			= "CloseImageButton";
		const string KILL_THREAD			= "kill";
		const string ALL_VALUE				= "All";
		const string CURRENTCOMPANY_VALUE	= "CurrentCompany";
		const string CURRENTUSER_VALUE		= "CurrentUser";
		const string SERVER_VALUE			= "Server";
		const string BROWSER_VALUE			= "Browser";
		const string staticTitle			= "EasyLook by Microarea s.p.a.: "; 

		ArrayList selectedDocs; //Lista dei documenti selezionati per la chiusura o per essere riagganciati dal browser
		UserInfo ui = null;
		Table documentsTable;
		Hashtable activeThreads = new Hashtable(); //thread attivi sul server (key:threadID, value:title(leggibile per utente) )
		ArrayList notClosableThreads; //Array con Id dei Thread che non possono essere chiusi
		Button SelectAllButton = new Button();
		Table tblDocuments = new Table();
		TableHeaderCell thCompany = null;

		#endregion

		#region Init e Load Function

		//--------------------------------------------------------------------------------------
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			
			SetCulture();

			if (!Page.IsPostBack)
			{
				LoadStaticCombo();
				SetToolTip();
			}

			//Carico la combo
			LoadComboThreadItem();

			documentsTable = CreateTable();
			documentsTable.CssClass = "tableDocuments";
			tableCellTable.Controls.Add(documentsTable);
			
			if (this.RefreshTimeDropDownList.SelectedItem != null && 
				this.RefreshTimeDropDownList.SelectedItem.Value != "0")
				InsertJSScript();

			BlockLayout();
			ApplyCustomSettings();
			SetLabelsText();
		}

		//---------------------------------------------------------------------------
		private void SetLabelsText()
		{
			LegendServer.Text = LabelStrings.OnlyServer;
			LegendBrowser.Text = LabelStrings.Attach;
			LabelGreen.Text = LabelStrings.CanBeClose;
			LabelRed.Text = LabelStrings.CanNotBeClose;
			this.Title = string.Concat(staticTitle, LabelStrings.Company, ": ", ui.Company, " - ", LabelStrings.Users, " ", ui.User);
		}

		//---------------------------------------------------------------------------
		private void InsertJSScript()
		{
			int millisecond = Convert.ToInt32(this.RefreshTimeDropDownList.SelectedItem.Value) * 1000;

			StringBuilder pingScript = new StringBuilder();
			pingScript.Append("<script type='text/javascript'>	function Ping() { \n");
			pingScript.Append("window.setTimeout('__doPostBack()', " + millisecond + "); }	</script>");
			ClientScript.RegisterClientScriptBlock(ClientScript.GetType(), "pingFunction", pingScript.ToString());
		}

		//---------------------------------------------------------------------------
		private void SetCulture()
		{
			//Setto la culture
			if (ui == null)
			{
				this.RedirectToLogin();
				return;
			}

			ui.SetCulture();
		}
		 
		#region metodo legge setting del PlugIn
		/// <summary>
		/// Metodo che applica i settaggi custom impostati per l'Utente attualmente
		/// logato all'applicazione
		/// </summary>
		//---------------------------------------------------------------------------
		private void ApplyCustomSettings()
		{
			if (Session[EasyLookCustomSettings.SessionKey] == null)
				return;

			EasyLookCustomSettings setting = (EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey];

			if (setting == null)
				return;

			if (documentsTable != null && (setting.FontFamily != string.Empty || setting.FontFamily != ""))
			{
				documentsTable.Font.Name = setting.FontFamily;
				documentsTable.Font.Size = FontUnit.Small;

				RefreshTimeLabel.Font.Name = setting.FontFamily;
				RefreshTimeLabel.Font.Size = FontUnit.Small;

				RefreshTimeDropDownList.Font.Name = setting.FontFamily;
				RefreshTimeLabel.Font.Size = FontUnit.Small;

				CompanyLabel.Font.Name = setting.FontFamily;
				CompanyLabel.Font.Size = FontUnit.Small;

				CompanyDropDownList.Font.Name = setting.FontFamily;
				CompanyDropDownList.Font.Size = FontUnit.Small;

				allUserLabel.Font.Name = setting.FontFamily;
				allUserLabel.Font.Size = FontUnit.Small;

				AllUserDropDownList.Font.Name = setting.FontFamily;
				AllUserDropDownList.Font.Size = FontUnit.Small;

				TypeLabel.Font.Name = setting.FontFamily;
				TypeLabel.Font.Size = FontUnit.Small;

				TypeFilterDropDownList.Font.Name = setting.FontFamily;
				TypeFilterDropDownList.Font.Size = FontUnit.Small;

				LegendServer.Font.Name = setting.FontFamily;
				LegendServer.Font.Size = FontUnit.Small;

				LegendBrowser.Font.Name = setting.FontFamily;
				LegendBrowser.Font.Size = FontUnit.Small;

				LabelGreen.Font.Name = setting.FontFamily;
				LabelGreen.Font.Size = FontUnit.Small;

				LabelRed.Font.Name = setting.FontFamily;
				LabelRed.Font.Size = FontUnit.Small;


			}
		}

		//---------------------------------------------------------------------
		#endregion

		//--------------------------------------------------------------------------------------
		protected override void OnPreInit(EventArgs e)
		{
			ui = UserInfo.FromSession();
			if (ui == null)
			{
				Page.ClientScript.RegisterStartupScript(GetType(), "CloseWindow", "window.close();", true);
				return;
			}

			//PostBack su azione di chiusura thread selezionati
			if (Request.Form["__EVENTTARGET"] == CLOSE_BTN_ID)
			{
				closeSelectedDocuments();
				//Se ci sono thread non chiudibili vuol dire che nell'operazione di chiusura non si e' riuscito a chiuderne qualcuno
				//si chiede all'utente se vuole killarli oppure annullare
				CreateAskToKillThreadScript();
			}
			//PostBack su azione di conferma kill dei documenti non precedentemente chiusi perche in elaborazione
			if (Request.Form["__EVENTTARGET"] == KILL_THREAD)
				killSelectedDocuments();

			CloseImageButton.Attributes["onclick"] = "return false"; //evita il postback
			CloseImageButton.OnClientClick = String.Format("__doPostBack('{0}', '')", CloseImageButton.ClientID);
		}

		#region Web Form Designer generated code
		/// <summary>
		/// Questa chiamata è richiesta da Progettazione Web Form ASP.NET.
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		/// Metodo necessario per il supporto della finestra di progettazione. Non modificare
		/// il contenuto del metodo con l'editor di codice.
		/// </summary>
		private void InitializeComponent()
		{

		}
		#endregion

		#endregion Init e Load Function
		
		#region metodi di chiusura e kill dei threads
		///<summary>
		/// Metodo di chiusura dei documenti selezionati
		///</summary>
		//--------------------------------------------------------------------------------------
		void closeSelectedDocuments()
		{
			notClosableThreads = new ArrayList();
			ArrayList threadsClosed = new ArrayList();

			TbLoaderClientInterface tbInterface = CommonDocumentFunctions.GetTbInterface();
			selectedDocs = (ArrayList)Session[SessionKey.DocumentsSelected];
			if (selectedDocs == null || tbInterface == null)
				return;

			for (int i = selectedDocs.Count - 1; i >= 0; i--)
			{
				int threadID = (int)selectedDocs[i];

				if (tbInterface.CanStopThread(threadID))
				{
					if (tbInterface.StopThread(threadID))
					{
						selectedDocs.Remove(threadID);
						threadsClosed.Add(threadID);
					}
				}
				else
				{
					notClosableThreads.Add(threadID);
				}
			}

			if (threadsClosed.Count > 0) // aggiungo script per chiudere le window del browser associate ai thread chiusi (se presenti)
				CreateCloseBrowserWindowScript(threadsClosed);
		}
		///<summary>
		/// Crea lo script per chiudere le window del browser associate ai thread chiusi (se presenti)
		///</summary>
		//--------------------------------------------------------------------------------------
		private void CreateCloseBrowserWindowScript(ArrayList threadIds)
		{
			StringBuilder script = new StringBuilder();
			script.Append("<script type='text/javascript'>	function closeWindows() { \n");
			foreach (var threadId in threadIds)
			{
				script.AppendFormat("opener.closeBrowserWindow({0});", threadId);
			}
			script.Append("};	</script>");
			ClientScript.RegisterClientScriptBlock(ClientScript.GetType(), "closeWindow", script.ToString());
		}
		///<summary>
		/// Metodo per chiudere anche forzatamente i documenti selezionati
		///</summary>
		//--------------------------------------------------------------------------------------
		void killSelectedDocuments()
		{
			ArrayList threadsClosed = new ArrayList();

			TbLoaderClientInterface tbInterface = CommonDocumentFunctions.GetTbInterface();

			if (tbInterface == null)
				return;

			selectedDocs = (ArrayList)Session[SessionKey.DocumentsSelected];
			for (int i = selectedDocs.Count - 1; i >= 0; i--)
			{
				int threadID = (int)selectedDocs[i];

				if (!(tbInterface.CanStopThread(threadID) && tbInterface.StopThread(threadID)))
					tbInterface.KillThread(threadID);

				selectedDocs.Remove(threadID);
				threadsClosed.Add(threadID);
			}

			if (threadsClosed.Count > 0) // aggiungo script per chiudere le window del browser associate ai thread chiusi (se presenti)
				CreateCloseBrowserWindowScript(threadsClosed);
		}
		///<summary>
		/// Crea lo script per chiedere all'utente se vuole killare thread che non si e' riusciti a chiudere 
		///</summary>
		//--------------------------------------------------------------------------------------
		void CreateAskToKillThreadScript()
		{
			activeThreads = (Hashtable)Session[SessionKey.ActiveThreads];

			if (notClosableThreads != null && notClosableThreads.Count > 0)
			{
				StringBuilder confirmKillThreadScript = new StringBuilder();
				StringBuilder message = new StringBuilder();
				message.AppendFormat("'{0} \\n' +", LabelStrings.DocumentRunningOnServer);
				foreach (int threadId in notClosableThreads)
				{
					if (activeThreads != null)
						message.AppendFormat("'- {0} \\n' +", activeThreads[threadId]);
				}
				message.AppendFormat("'{0}'", LabelStrings.ConfirmClosing);
				confirmKillThreadScript.AppendFormat("<script>if (confirm({0})){{", message.ToString());
				confirmKillThreadScript.AppendFormat("			if (confirm('{0}')){{", LabelStrings.AreYouSure);
				confirmKillThreadScript.AppendFormat("				__doPostBack('{0}', '')", KILL_THREAD);
				confirmKillThreadScript.Append("	}");
				confirmKillThreadScript.Append("}");
				confirmKillThreadScript.Append("</script>");
				ClientScript.RegisterStartupScript(this.GetType(), "confirm", confirmKillThreadScript.ToString());
			}
		}
		#endregion

		#region Layout
		//--------------------------------------------------------------------------------------
		private void BlockLayout()
		{
			if (!ui.Admin || CompanyDropDownList.Items.Count == 2)
			{
				CompanyDropDownList.SelectedIndex = 
							CompanyDropDownList.Items.IndexOf(new ListItem(LabelStrings.CurrentCompany, CURRENTCOMPANY_VALUE));
				CompanyDropDownList.Enabled = false;
			}

			if (!ui.Admin || AllUserDropDownList.Items.Count == 2)
			{
				AllUserDropDownList.SelectedIndex =
					AllUserDropDownList.Items.IndexOf(new ListItem(LabelStrings.CurrentUser, CURRENTUSER_VALUE));
				AllUserDropDownList.Enabled = false;
			}

			activeThreads = (Hashtable)Session[SessionKey.ActiveThreads];

			if (activeThreads.Count == 0)
				NotEnabledButtons();
		}
		//--------------------------------------------------------------------------------------
		private void NotEnabledButtons()
		{
			CloseImageButton.Enabled = false;
			SelectAllButton.Enabled = false;

		}
		//---------------------------------------------------------------------
		private void SetToolTip()
		{
			CloseImageButton.ToolTip = LabelStrings.Close;
			RefreshImageButton.ToolTip = LabelStrings.Refresh;
			allUserLabel.Text = LabelStrings.ShowItems;
			TypeLabel.Text = LabelStrings.FilterList;
			CompanyLabel.Text = LabelStrings.Company;
			RefreshTimeLabel.Text = LabelStrings.RefreshTimeInterval;
		}
		#endregion

		#region CreateTable Functions
		//--------------------------------------------------------------------------------------
		TableRow CreateNoDocumentsRow()
		{
			TableRow tr = new TableRow();
			TableCell tc = new TableCell();
			if (CompanyDropDownList.Items.Count == 2 || !ui.Admin)
				tc.ColumnSpan = 5;
			else
				tc.ColumnSpan = 6;  //ATTENZIONE, deve essere uguale al numero di colonne della tabella, vedere metodo CreateHeader()
			tc.Text = LabelStrings.NoDocumentOpen;
			tc.Font.Bold = true;
			tc.Style.Add(HtmlTextWriterStyle.TextAlign, "center");
			tc.Style.Add(HtmlTextWriterStyle.Padding, "3px");
			tr.Cells.Add(tc);
			return tr;
		}
		//--------------------------------------------------------------------------------------
		TableHeaderRow CreateHeader()
		{
			TableHeaderRow thr = new TableHeaderRow();
			thr.BackColor = Color.Blue;
			thr.ForeColor = Color.White;

			TableHeaderCell thc = new TableHeaderCell();
			thc.CssClass = "cellHeader";
			thc.Text = LabelStrings.Session;
			thr.Cells.Add(thc);
			
			thc = new TableHeaderCell();
			thc.CssClass = "cellHeader";
			thc.Text = LabelStrings.Document;
			thr.Cells.Add(thc);
			
			thc = new TableHeaderCell();
			thc.CssClass = "cellHeader";
			thc.Text = LabelStrings.UserLabel;
			thr.Cells.Add(thc);

			thCompany = new TableHeaderCell();
			thCompany.CssClass = "cellHeader";
			thCompany.Text = LabelStrings.Company;
			thr.Cells.Add(thCompany);

			thc = new TableHeaderCell();
			thc.CssClass = "cellHeader";
			thc.Text = LabelStrings.OperationDate;
			thr.Cells.Add(thc);
			thc = new TableHeaderCell();
			thc.CssClass = "cellHeader";
			thc.Text = LabelStrings.CloseStatus;
			thr.Cells.Add(thc);
			thc = new TableHeaderCell();
			thc.CssClass = "cellHeader";
			thc.Text = LabelStrings.InactivityTime;
			thr.Cells.Add(thc);
			return thr;
		}
		//--------------------------------------------------------------------------------------
		private string GetActiveThreads(bool all)
		{ 
			if (!all)
				return CommonDocumentFunctions.GetTbInterface().GetLoginActiveThreads();

			return CommonDocumentFunctions.GetTbInterface().GetActiveThreads();
		}
		//--------------------------------------------------------------------------------------
		private Table CreateTable()
		{
			ListItem item = null;
			//pulizia hashtable dei thread attivi, viene ripopolato in fase di creazione della tabella
			activeThreads.Clear();
			tableCellTable.Controls.Clear();
			tblDocuments.Rows.Clear();
			tblDocuments.Rows.Add(CreateHeader());

			//inserisco tutti i documenti attivi sul server se sono admin e voglio vederli tutti
			string xmlActiveThreads = GetActiveThreads(ui.Admin && this.AllUserDropDownList.SelectedItem != null && this.AllUserDropDownList.SelectedItem.Value == ALL_VALUE);
			if (!String.IsNullOrEmpty(xmlActiveThreads))
			{
				XmlDocument xmlDoc = new XmlDocument();
				try
				{
					xmlDoc.LoadXml(xmlActiveThreads);
				}
				catch
				{
				}
				string companyName = string.Empty;
				string operationDate = string.Empty;
				string userName = string.Empty;

				XmlNodeList documentThreads = xmlDoc.SelectNodes("Threads/Thread");
				if (documentThreads != null && documentThreads.Count != 0)
				{
					foreach (XmlElement loginThread in documentThreads)
					{
						companyName = loginThread.Attributes["company"].Value;
						operationDate = loginThread.Attributes["operationDate"].Value;
						userName = loginThread.Attributes["user"].Value;

						item = new ListItem(loginThread.GetAttribute("user"), loginThread.GetAttribute("user"));
						if (AllUserDropDownList.Items != null && !AllUserDropDownList.Items.Contains(item) && 
								loginThread.GetAttribute("user") != ui.User)
							AllUserDropDownList.Items.Add(item);

						item = new ListItem(loginThread.GetAttribute("company"), loginThread.GetAttribute("company"));
						if (CompanyDropDownList.Items != null && !CompanyDropDownList.Items.Contains(item) && 
								loginThread.GetAttribute("company") != ui.Company)
							CompanyDropDownList.Items.Add(item);

						XmlNodeList documentThreadList = loginThread.SelectNodes("Threads/Thread");
						if (documentThreadList == null || documentThreadList.Count == 0)
							continue;

						foreach (XmlElement docThread in documentThreadList)
						{
							if (this.CompanyDropDownList.SelectedItem != null && 
								this.CompanyDropDownList.SelectedItem.Value != ALL_VALUE &&
								this.CompanyDropDownList.SelectedItem.Value != companyName &&
								this.CompanyDropDownList.SelectedItem.Value != CURRENTCOMPANY_VALUE)
								continue;

							if (TypeFilterDropDownList.SelectedItem != null)
							{
								if ((TypeFilterDropDownList.SelectedItem.Value == BROWSER_VALUE && 
									docThread.GetAttribute("remoteinterfaceattached") == "false") ||
									(TypeFilterDropDownList.SelectedItem.Value == SERVER_VALUE && 
									docThread.GetAttribute("remoteinterfaceattached") == "true"))

									continue;
							}

							TableRow myTR = AddThreadRow(docThread, companyName, operationDate, userName);

							if (myTR != null)
								tblDocuments.Rows.Add(myTR);
						}
					}
					tblDocuments.Rows.Add(CreateEmptyRow());
				}
				else //non ci sono thread attivi inserisco una riga con il messaggio
				{
					tblDocuments.Rows.Add(CreateNoDocumentsRow());
					NotEnabledButtons();
				}
			}

			Session[SessionKey.ActiveThreads] = activeThreads;

			if (CompanyDropDownList.Items.Count == 2 || !ui.Admin)
				thCompany.Visible = false;

			return tblDocuments;
		}
		//--------------------------------------------------------------------------------------
		private TableRow CreateEmptyRow()
		{
			TableRow trEmpy = new TableRow();
			TableCell tcButton = new TableCell();

			if (CompanyDropDownList.Items.Count == 2 || !ui.Admin)
				tcButton.ColumnSpan = 6;
			else
				tcButton.ColumnSpan = 7;

			tcButton.HorizontalAlign = HorizontalAlign.Right;
			SelectAllButton.Click += new EventHandler(SelectAllButton_Click);
			SelectAllButton.Text = LabelStrings.SelectAll;
			tcButton.Controls.Add(SelectAllButton);
			trEmpy.Cells.Add(tcButton);

			return trEmpy;
		
		}
		//--------------------------------------------------------------------------------------
		private TableRow  AddThreadRow(XmlElement docThread, string companyName, string operationDate, string userName)
		{
			int threadID = 0;
			Int32.TryParse(docThread.GetAttribute("id"), out threadID);
			string threadTitle = docThread.GetAttribute("title");
			int inactivitySeconds = 0;
			Int32.TryParse(docThread.GetAttribute("inactivitytime"), out inactivitySeconds);
			TableRow tr = new TableRow();

			TableCell tcState = new TableCell();  //state
			tcState.CssClass = "cellDocuments";

			System.Web.UI.WebControls.Image imgstate = new System.Web.UI.WebControls.Image();
			imgstate.ImageUrl = Helper.GetImageUrl("computers-24x24.png");
				if (docThread.GetAttribute("remoteinterfaceattached") == "false")
				imgstate.ImageUrl = Helper.GetImageUrl("database-24x24.png");
			tcState.HorizontalAlign = HorizontalAlign.Center;
			tcState.Controls.Add(imgstate);

			tr.Cells.Add(tcState);

			TableCell tc = new TableCell(); //document name
			tc.CssClass = "cellDocuments";
			HyperLink link = new HyperLink();
			link.Text = threadTitle;
			
			//se il documento e' di questa login, permetto un attach al documento, altrimenti mostro uno snapshot del documento stesso
			if (ui.User == userName)
			{
				link.ToolTip = threadTitle;
				link.NavigateUrl = string.Format("javascript:opener.attachToDocument({0})", threadID);	
			}				
			else
			{	link.ToolTip = LabelStrings.ViewSnapshotOf + threadTitle;
				link.NavigateUrl = string.Format("javascript:opener.showDocumentSnapshot({0})", threadID);
			}
			tc.Controls.Add(link);
			tr.Cells.Add(tc);

			tc = new TableCell(); //user
			tc.CssClass = "cellDocuments";
			tc.Text = userName;
			tr.Cells.Add(tc);

			tc = new TableCell(); //Company
			tc.CssClass = "cellDocuments";
			tc.Text = companyName;
			tc.HorizontalAlign = HorizontalAlign.Right;

			if (CompanyDropDownList.Items.Count == 2 || !ui.Admin)
				tc.Visible = false;

			tr.Cells.Add(tc);

			tc = new TableCell(); //Operation Date
			tc.CssClass = "cellDocuments";
			tc.HorizontalAlign = HorizontalAlign.Right;
			tc.Text = operationDate;
			tr.Cells.Add(tc);

			tc = new TableCell();
			tc.CssClass = "cellDocuments";
			CheckBox selectCheck = new CheckBox();
			System.Web.UI.WebControls.Image img = new  System.Web.UI.WebControls.Image();
			img.ImageUrl = Helper.GetImageUrl("greenBall.png");
			if (docThread.GetAttribute("canbestopped")!= "true")
			{
				img.ImageUrl = Helper.GetImageUrl("redBall.png");
			}
			tc.HorizontalAlign = HorizontalAlign.Center;
			 
			
			selectCheck.AutoPostBack = true;
			selectCheck.ID = threadID.ToString();
			selectCheck.CheckedChanged += new EventHandler(selectCheck_CheckedChanged);
			
			tc.Controls.Add(selectCheck);
			tc.Controls.Add(new LiteralControl("&nbsp;"));
			tc.Controls.Add(img);

			tr.Cells.Add(tc);
			tc = new TableCell(); //Inactivity Time
			tc.CssClass = "cellDocuments";
			tc.Text = TimeSpan.FromSeconds(inactivitySeconds).ToString();
			tc.HorizontalAlign = HorizontalAlign.Right;
			tr.Cells.Add(tc);

			activeThreads.Add(threadID, threadTitle);
			return tr;
		}

		//-------------------------------------------------------------------------------------
		void SelectAllButton_Click(object sender, EventArgs e)
		{
			if (SelectAllButton.Text == LabelStrings.SelectAll)
			{
				SelectAll(true);
				SelectAllButton.Text = LabelStrings.UnselectAll;
			}
			else
			{
				SelectAllButton.Text = LabelStrings.SelectAll;
				SelectAll(false);
			}
		}

		//-------------------------------------------------------------------------------------
		private void SelectAll(bool select)
		{
			selectedDocs = new ArrayList();
			int threadID = 0;
			CheckBox chk = null;

			foreach (TableRow tr in tblDocuments.Rows)
			{
				foreach (TableCell tc in tr.Cells)
				{
					foreach (Control control in tc.Controls)
					{
						if (control is CheckBox)
						{
							chk = (CheckBox)control;
							chk.Checked = select;
							Int32.TryParse(chk.ID, out threadID);

							if (selectedDocs.Contains(threadID) && !select)
								selectedDocs.Remove(threadID);
							else if (!selectedDocs.Contains(threadID) && select)
								selectedDocs.Add(threadID);
						}
					}
				}
			}

			Session[SessionKey.DocumentsSelected] = selectedDocs;
		}

		#endregion CreateTable Functions

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che inserisce i tipi di filtraggio nella combo dopo averli 
		/// localizzati
		/// </summary>
		private void LoadComboThreadItem()
		{
			AllUserDropDownList.Items.Clear();
			CompanyDropDownList.Items.Clear();

			ListItem itemAll = new ListItem(LabelStrings.All, ALL_VALUE);
			AllUserDropDownList.Items.Add(itemAll);
			ListItem itemCurrentUser = new ListItem(LabelStrings.CurrentUser, CURRENTUSER_VALUE);
			AllUserDropDownList.Items.Add(itemCurrentUser);

			CompanyDropDownList.Items.Add(new ListItem(LabelStrings.All, ALL_VALUE));
			CompanyDropDownList.Items.Add(new ListItem(LabelStrings.CurrentCompany, CURRENTCOMPANY_VALUE));
		}


		//---------------------------------------------------------------------
		private void LoadStaticCombo()
		{
			ListItem itemAll = new ListItem(LabelStrings.All, ALL_VALUE);
			TypeFilterDropDownList.Items.Add(itemAll);
			TypeFilterDropDownList.Items.Add(new ListItem(LabelStrings.Server, SERVER_VALUE));
			TypeFilterDropDownList.Items.Add(new ListItem(LabelStrings.Browser, BROWSER_VALUE));
			itemAll.Selected = true;
			RefreshTimeDropDownList.Items.Add(new ListItem(LabelStrings.Never, "0"));
			RefreshTimeDropDownList.Items.Add(new ListItem("1'", "60"));
			RefreshTimeDropDownList.Items.Add(new ListItem("5'", "300"));
			RefreshTimeDropDownList.Items.Add(new ListItem("10'", "600"));
		}

		#region Events
		// Metodo che sulla selezione dei documenti aggiorna l'array dei documenti
		//--------------------------------------------------------------------------------------
		void selectCheck_CheckedChanged(object sender, EventArgs e)
		{
			selectedDocs = (ArrayList)Session[SessionKey.DocumentsSelected];
			if (selectedDocs == null)
				selectedDocs = new ArrayList();

			CheckBox chk = (CheckBox)sender;
			int threadID = 0;
			Int32.TryParse(chk.ID, out threadID);

			if (chk.Checked && !selectedDocs.Contains(threadID))
				selectedDocs.Add(threadID);
			else
				selectedDocs.Remove(threadID);

			Session[SessionKey.DocumentsSelected] = selectedDocs;
		}
		#endregion Events
	}
}
