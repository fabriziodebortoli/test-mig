using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;

namespace Microarea.Web.EasyLook
{
	public partial class OpenDocumentsPage : System.Web.UI.Page
	{
		#region Dichiarazione variabili di classe e costanti
		//stringhe usate per controllare quali elementi html hanno fatto il postback della pagina 
		ArrayList selectedDocs; //Lista dei documenti selezionati per la chiusura o per essere riagganciati dal browser
		Table documentsTable;
		string title = "";
		string documentNamespace = "";
		string message = "";
		StringBuilder initScript; //js iniettato nella pagina per inizializzare alcune colonne della tabella (servono informazioni che si hanno solo lato javascript)
		Hashtable activeThreads = new Hashtable(); //thread attivi sul server (key:threadID, value:title(leggibile per utente) )
		Button SelectAllButton = new Button();
		Table tblDocuments = new Table();
		#endregion

		#region metodi di uso generico


		/// <summary>
		/// Metodo che estrapola il nome del documento dal titolo della finestra.
		/// </summary>
		//---------------------------------------------------------------------
		string GetDocumentNameFromTitle(string windowTitle)
		{
			string docName = windowTitle.Substring(0, windowTitle.IndexOf('-') - 1);
			return String.IsNullOrEmpty(docName) ? windowTitle : docName;
		}

		#endregion

		#region metodi che creano dinamicamente script js da iniettare al browser


		///<summary>
		/// metodo che crea la chimata al metodo AttachtoDocuments creando e passandogli l'array dei thread selezionati 
		///</summary>
		//--------------------------------------------------------------------------------------
		string CreateAttachToDocumentsScript()
		{
			selectedDocs = (ArrayList)Session[SessionKey.DocumentsSelected];
			if (selectedDocs == null)
				return "";

			//Attenzione: passo un handle dummy (0) come primo paramentro perche nel caso di un solo documento seleziona
			//la chiamata js new Array(n) non crea una Array con un elemento ma un array con n elementi vuoti
			StringBuilder sbArrayParam = new StringBuilder("new Array(0, ");  
			for (int i = 0; i < selectedDocs.Count; i++)
			{
				if (i > 0)
					sbArrayParam.Append(",");

				int threadID = (int)selectedDocs[i];
				sbArrayParam.AppendFormat(" {0}", threadID);
			}
			sbArrayParam.Append(")");

			return string.Format("parent.attachToDocuments({0});", sbArrayParam.ToString());
		}

		
		// Crea lo script necessario a inizializzare i valori per un dato documento (una riga della tabella) 
		// con informazioni che si hanno solo da javascript (in particolare l'informazione se un documento e' aperto in una finestra del browser)
		//--------------------------------------------------------------------------------------
		private void CreateRowInitScript(int threadID, TableCell tcState)
		{
			initScript.AppendFormat("var cellState = document.getElementById('{0}'); \n", tcState.ClientID);
			initScript.AppendFormat("var bRunning = innerTextSupported() ? cellState.innerText == '{0}' : cellState.textContent == '{1}'; \n", LabelStrings.ThreadRunning, LabelStrings.ThreadRunning);
			initScript.AppendFormat("if (cellState != null && bRunning == false){{ \n");
			initScript.AppendFormat("	if(parent.getBrowserWindow({0}) != null) \n", threadID);
			initScript.AppendFormat("	{{ \n");
			initScript.AppendFormat("		   cellState.innerHTML = '<center><img align=\"middle\" src=\"{0}\"\" /></center>'; \n", Helper.GetImageUrl("computers-24x24.png"));
			initScript.AppendFormat("	}} \n");
			initScript.AppendFormat("	else \n");
			initScript.AppendFormat("	{{ \n");
			initScript.AppendFormat("		   cellState.innerHTML = '<center><img align=\"middle\" src=\"{0}\"\" /></center>'; \n", Helper.GetImageUrl("database-24x24.png"));
			initScript.AppendFormat("	}}\n");
			initScript.AppendFormat("}}\n");
		}
		#endregion

		#region override dei metodi del ciclo di vita della pagina
		///<summary>
		/// Aggiunta dei controlli dinamici alla pagina
		///</summary>
		//--------------------------------------------------------------------------------------
		protected override void OnPreInit(EventArgs e)
		{
			//Setto la culture
			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				this.RedirectToLogin();
				return;
			}
			ui.SetCulture();

			//se ho aperto la pagina selezionando un documneto predispongo il link per il lancio di un nuovo documento
			CreateLinkToNewDocument();

			initScript = new StringBuilder();
			initScript.Append("<script type='text/javascript'>	function initTableRows() { \n");
			
			documentsTable = CreateTable();
			documentsTable.CssClass = "tableDocuments";
			documentsCell.Controls.Add(documentsTable);
			
			initScript.Append("};	</script>");
			ClientScript.RegisterClientScriptBlock(ClientScript.GetType(), "initTable", initScript.ToString());

		}

		//--------------------------------------------------------------------------------------
		protected void Page_Load(object sender, EventArgs e)
		{
			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			//se ho fatto reload della pagina pulisco array dei documenti selezionati
			if (!Page.IsPostBack)
				Session[SessionKey.DocumentsSelected] = null;

			LegendServer.Text = LabelStrings.OnlyServer;
			LegendBrowser.Text = LabelStrings.Attach;
			refreshBtn.ToolTip = LabelStrings.RefreshDocList;
			detailsBtn.ToolTip = LabelStrings.OpenDetails;
			attachSelectedBtn.ToolTip = LabelStrings.AttachDoc;

			SelectAllButton.Click += new EventHandler(SelectAllButton_Click);
	
			if (UserInfo.FromSession() == null)
				return;

			ApplyCustomSettings();
		}
		#endregion


		#region metodi per la creazione dinamica di controlli
		///<summary>
		/// Metodo che crea il link per un lancio di un documento se la pagina e' stata aperta da un link di documento
		///</summary>	
		private void CreateLinkToNewDocument()
		{
			//Prendo i valori dalla QueryString (per creare prima riga come link a nuovo documento)
			documentNamespace = HttpContext.Current.Request.QueryString["NameSpace"];
			title = HttpContext.Current.Request.QueryString["Title"];
			message = HttpContext.Current.Request.QueryString["Message"];
			if (documentNamespace != null)
				documentNamespace = HttpUtility.UrlDecode(documentNamespace);

			//Inserisco la prima riga che è diversa dalle altre perchè rappresenta il documento
			//stesso infatti ha solo il nome e il link fa partire una nuova esecuzione 
			//solo se ho aperto la pagina cliccando su un link di documento
			if (!string.IsNullOrEmpty(documentNamespace) && !string.IsNullOrEmpty(title))
			{
				string linkUrl =  string.Format("javascript:parent.OpenPopupDocument('{0}')",
												 HttpUtility.UrlEncode(documentNamespace)
												);

				ImageButton img = new ImageButton();
				img.ImageUrl = Helper.GetImageUrl("folder-full-32x32.png");
				img.ToolTip = LabelStrings.OpenSelDoc;
				newDocumentCellIcon.Controls.Add(img);
                title = title.DecodeBase16();
				HyperLink link = new HyperLink();
				link.ForeColor = Color.Blue;
				link.Font.Bold = true;
				link.Font.Name = "Verdana";
				link.Font.Size = 10;
				link.NavigateUrl = linkUrl;
				link.Text = String.Format("{0} {1}", LabelStrings.OpenNewDocument, title);
				img.OnClientClick = linkUrl;
				newDocumentCellLink.Controls.Add(link);
				
			}

		}

		//--------------------------------------------------------------------------------------
		TableHeaderRow CreateHeader()
		{
			TableHeaderRow thr = new TableHeaderRow();
			thr.BackColor = Color.Blue;
			thr.ForeColor = Color.White;

			TableHeaderCell  thc = new TableHeaderCell();
			thc.CssClass = "cellHeader";
			thc.Text = LabelStrings.Session;
			thr.Cells.Add(thc);

			thc = new TableHeaderCell();
			thc.CssClass = "cellHeader";
			thc.Text = LabelStrings.Document;
			thr.Cells.Add(thc);

			thc = new TableHeaderCell();
			thc.CssClass = "cellHeader";
			thc.Text = LabelStrings.Company;
			thr.Cells.Add(thc);

			thc = new TableHeaderCell();
			thc.CssClass = "cellHeader";
			thc.Text = LabelStrings.OperationDate;
			thr.Cells.Add(thc);

			thc = new TableHeaderCell();
			thc.CssClass = "cellHeader";
			thc.Text = LabelStrings.atDoc;
			thr.Cells.Add(thc);
			return thr;
		}

		//--------------------------------------------------------------------------------------
		TableRow CreateNoDocumentsRow()
		{
			TableRow tr = new TableRow();
			TableCell tc = new TableCell();
			tc.ColumnSpan = 5;  //ATTENZIONE, deve essere uguale al numero di colonne della tabella, vedere metodo CreateHeader()
			tc.Text = LabelStrings.NoDocumentOpen;
			tc.Font.Bold = true;
			tc.Style.Add(HtmlTextWriterStyle.TextAlign, "center");
			tc.Style.Add(HtmlTextWriterStyle.Padding, "3px");
			tr.Cells.Add(tc);
			return tr;
		}

		//--------------------------------------------------------------------------------------
		Table CreateTable()
		{
			bool documentsInserted = false;
			//pulizia hashtable dei thread attivi, viene ripopolato in fase di creazione della tabella
			activeThreads.Clear();
			tblDocuments.Rows.Clear();
			tblDocuments.Rows.Add(CreateHeader());

			//se nn ho un tbloader non ha senso chiedere se ci cosno documenti aperti
			if (CommonDocumentFunctions.IsTbLoaderLoaded())
			{
				TbLoaderClientInterface tbLoaderClientInterface = CommonDocumentFunctions.GetTbInterface();
				if (tbLoaderClientInterface != null && tbLoaderClientInterface.IsLoginValid())
					documentsInserted = CreateBodyTable(tbLoaderClientInterface);
			}
			if (!documentsInserted) //non ci sono threads attivi inserisco una riga con il messaggio e nascondo i bottoni che fanno operazioni sui documenti
			{
				tblDocuments.Rows.Add(CreateNoDocumentsRow());
				HideButtons();
			}
			Session[SessionKey.ActiveThreads] = activeThreads;
			
			return tblDocuments;
		}

		///<summary>
		///metodo che popola il corpo della tabella inserendo una riga per ogni documento attivo
		///<param name="tbLoaderClientInterface">Interfaccia del TbLoader</param>
		/// <returns>true se sono stati trovati documenti (e quindi inserite righe nella tabella), false altrimenti</returns>
		///</summary>
		//--------------------------------------------------------------------------------------
		private bool CreateBodyTable(TbLoaderClientInterface tbLoaderClientInterface)
		{
			bool documentsInserted = false;
			
			if (tbLoaderClientInterface == null)
				return false;

			//inserisco tutti i documenti attivi sul server
			string xmlActiveThreads = tbLoaderClientInterface.GetLoginActiveThreads();
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
					companyName = documentThreads[0].Attributes["company"].Value;
					operationDate = documentThreads[0].Attributes["operationDate"].Value;
					userName = documentThreads[0].Attributes["user"].Value;
				}
				XmlNodeList documentThreadList = xmlDoc.SelectNodes("Threads/Thread/Threads/Thread");

				if (documentThreadList != null && documentThreadList.Count != 0)
				{
					foreach (XmlElement docThread in documentThreadList)
					{
						int threadID = 0;
						Int32.TryParse(docThread.GetAttribute("id"), out threadID);
						string threadTitle = docThread.GetAttribute("title");

						TableRow tr = new TableRow();


						TableCell tcState = new TableCell();  //state
						tcState.CssClass = "cellDocuments";
						tcState.ID = string.Format("state{0}", threadID);
						
						tr.Cells.Add(tcState);
						TableCell tc = new TableCell(); //document name
						tc.CssClass = "cellDocuments";
						HyperLink link = new HyperLink();
						link.Text = threadTitle;
						link.ToolTip = threadTitle;
						link.NavigateUrl = string.Format(
												"javascript:parent.attachToDocument({0})",
												threadID
												);
						tc.Controls.Add(link);
						tr.Cells.Add(tc);
						tc = new TableCell(); //Company
						tc.CssClass = "cellDocuments";
						tc.Text = companyName;
						tr.Cells.Add(tc);

						tc = new TableCell(); //DATA
						tc.CssClass = "cellDocuments";
						tc.Text = operationDate;
						tr.Cells.Add(tc);
						tc = new TableCell();
						tc.CssClass = "cellDocuments";
						tc.HorizontalAlign = HorizontalAlign.Center;
						CheckBox selectCheck = new CheckBox();
						selectCheck.AutoPostBack = true;
						selectCheck.ID = threadID.ToString();
						selectCheck.CheckedChanged += new EventHandler(selectCheck_CheckedChanged);
						tc.Controls.Add(selectCheck);
						tr.Cells.Add(tc);

						CreateRowInitScript(threadID, tcState);
						tblDocuments.Rows.Add(tr);

						activeThreads.Add(threadID, threadTitle);
					}
					documentsInserted = true;
				}
			}
			TableRow trEmpy = new TableRow();
			TableCell tcButton = new TableCell();

			tcButton.ColumnSpan = 5;
			tcButton.HorizontalAlign = HorizontalAlign.Right;
			SelectAllButton.Text = LabelStrings.SelectAll;
			tcButton.Controls.Add(SelectAllButton);
			trEmpy.Cells.Add(tcButton);
			tblDocuments.Rows.Add(trEmpy);

			return documentsInserted;
		}

		///<summary>
		/// Nasconde i bottoni nel caso caso non ci siano documenti selezionati e quindi nessuna azioen da fare su di loro
		///</summary>
		/////--------------------------------------------------------------------------------------
		private void HideButtons()
		{
			attachSelectedBtn.Visible = false;
			SelectAllButton.Visible = false;
		}		

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
			attachSelectedBtn.OnClientClick = CreateAttachToDocumentsScript();
		}

		//-------------------------------------------------------------------------------------
		void SelectAllButton_Click(object sender, EventArgs e)
		{
			
			if (SelectAllButton.Text == LabelStrings.SelectAll)
			{
				SelectAll();
				SelectAllButton.Text = LabelStrings.UnselectAll;
			}
			else
			{
				SelectAllButton.Text = LabelStrings.SelectAll;
				UnselectAll();
			}
		}

		//-------------------------------------------------------------------------------------
		private void UnselectAll()
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
							chk.Checked = false;
							Int32.TryParse(chk.ID, out threadID);
							if (selectedDocs.Contains(threadID))
								selectedDocs.Remove(threadID);
						}
					}
				}
			}
			Session[SessionKey.DocumentsSelected] = selectedDocs;
		}

		//-------------------------------------------------------------------------------------
		private void SelectAll()
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
							chk.Checked = true;
							Int32.TryParse(chk.ID, out threadID);
							if (!selectedDocs.Contains(threadID))
								selectedDocs.Add(threadID);

							Session[SessionKey.DocumentsSelected] = selectedDocs;
							attachSelectedBtn.OnClientClick = CreateAttachToDocumentsScript();
						}
					}
				}
			}
		}
		#endregion

		#region metodo legge setting del PlugIn
		/// <summary>
		/// Metodo che applica i settaggi custom impostati per l'Utente attualmente
		/// logato all'applicazione
		/// </summary>
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

				LegendServer.Font.Name = setting.FontFamily;
				LegendServer.Font.Size = FontUnit.Small;

				LegendBrowser.Font.Name = setting.FontFamily;
				LegendBrowser.Font.Size = FontUnit.Small;
				
			}
		}

		//---------------------------------------------------------------------
		#endregion
	}
}
