using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;


namespace Microarea.Web.EasyLook
{


	//=========================================================================
	public partial class HistoryPage : System.Web.UI.Page
	{

		#region DataMember protetti
		/// <summary>
		/// ArrayList che conterrà l'elenco dei report storicizzati
		/// </summary>
		protected ArrayList reports = null;
		/// <summary>
		/// Strign che conterrà il tipo di ordinamento effettuato
		/// </summary>
		protected string currentSort;
		#endregion

		#region load della pagina
		/// <summary>
		/// Evento di Load della pagina setta la Cache a NoCache, imposta la culture 
		/// dell'Utente che si è connesso, e visualizza lo storico del report selezionato
		/// popolando una griglia.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			string reload = string.Empty;

			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			SetCulture();

			if (Page.IsPostBack && Request.Form["__EVENTTARGET"] != "refreshBtn")
				return;

			if (UserInfo.FromSession() == null) 
				return;

			//Prendo i valori dalla QueryString
			string reportNodePath	= HttpContext.Current.Request.QueryString["NameSpace"];
			string reportTitle		= HttpContext.Current.Request.QueryString["Title"];
			string reportParametersID = HttpContext.Current.Request.QueryString["ParametersID"];
			
			if (reportTitle != null )
                reportTitle = reportTitle.DecodeBase16();
				
			string reportParameters = string.Empty;
			Hashtable parametersTable = Session[ReportPage.ReportParametersTableKey] as Hashtable;

			if (parametersTable != null && reportParametersID != null)
				reportParameters = parametersTable[reportParametersID] as string;
			if (reportParameters == null) reportParameters = "";
			

			//Valorizzo il parametro ReloadHistory prendendolo dalla session che mi dice cosa devo fare
			if (Session[SessionKey.ReloadHistory] != null && Session[SessionKey.ReloadHistory].ToString() != string.Empty )
				reload = Session[SessionKey.ReloadHistory].ToString();

			//Default del refresh dei pannelli
			if (string.IsNullOrEmpty(reportNodePath) && string.IsNullOrEmpty(reload))
				return;

			//Se non mi hanno passato il Titolo del report nella query string me lo vado a prendere 
			//dalla session perchè vuol dire che è un reload della pagina a seguito di un'operazione
			// ES. cancellazione, run, ecc...
			if (string.IsNullOrEmpty(reportTitle))
			{
				if (Session[SessionKey.ReportTitle] != null)
					reportTitle = Session[SessionKey.ReportTitle].ToString();
			}
			else
				Session.Add(SessionKey.ReportTitle, reportTitle);


			ToolBarTable.Visible = true;
			//Sono nella cancellazione
			if (Session[SessionKey.ReportsToDeleting] != null &&   reload == ReloadType.Delete.ToString()) 
			{	
				if(((ArrayList)Session[SessionKey.ReportsToDeleting]).Count != 0 )
					DeleteReport();
			}
			
			//Se il reportNodePath non mi è arrivato dalla Query string me lo prendo dalla
			//session perchè sono nel reload della pagina
			if (reload != string.Empty && (reportNodePath== string.Empty ||reportNodePath == null))
			{
				if (Session[Microarea.TaskBuilderNet.Woorm.WoormWebControl.SessionKey.ReportPath] != null)
					reportNodePath = Session[Microarea.TaskBuilderNet.Woorm.WoormWebControl.SessionKey.ReportPath].ToString();
				else
					return;
			}

			ArrayList reportArrayList = GetHistory(reportNodePath);

			//Setto le stringhe localizzate
			SetLocalizableLable();

			//Mi preparo il DataGrid Layout e Dati
			SetDataGrid(reportNodePath, reportParameters);

			if (historyDataGrid.DataSource != null)
				deleteBtn.Visible  = true;
			else
				deleteBtn.Visible = false;

			//Applico i setting custom
			ApplyCustomSettings();

			CreateLinkToNewReport(reportNodePath, reportParameters);
			

		}

		///<summary>
		/// Metodo che crea il link per un lancio di un report se la pagina e' stata aperta da un link di report
		///</summary>	
		private void CreateLinkToNewReport(string reportNodePath, string reportParameters)
		{
			string linkUrl = string.Format(
											"javascript:parent.OpenPopUpNewReport('{0}', '{1}')",
											HttpUtility.UrlEncode(reportNodePath),
											reportParameters
										  );
			runReportBtn.NavigateUrl = linkUrl;
				
			HyperLink link = new HyperLink();
			link.ForeColor = Color.Blue;
			link.Font.Bold = true;
			link.Font.Name = "Verdana";
			link.Font.Size = 10;
			link.NavigateUrl = linkUrl;
			link.Text = String.Format("{0} {1}", LabelStrings.OpenNewReport, Session[SessionKey.ReportTitle].ToString());
			runReportCellLink.Controls.Add(link);
		}

		//--------------------------------------------------------------------------------------
		protected override void OnPreInit(EventArgs e)
		{
			refreshBtn.OnClientClick = String.Format("__doPostBack('{0}', '')", refreshBtn.ClientID);
		}

		//--------------------------------------------------------------------------------------
		private ArrayList GetHistory(string reportNodePath)
		{
			//Cerco i report storicizzati
			RunnedReport[] report = GetRunnedReport(reportNodePath);
			//Me li ordino per data Decrescente
			ArrayList reportArrayList = GetReportByDate(report);
			//Me lo metto in session
			Session.Add(SessionKey.Reports, reportArrayList);
			//e per sicurezza ci metto anche il reportNodePath nel caso fosse != da 
			//quello che c'è già
			Session[Microarea.TaskBuilderNet.Woorm.WoormWebControl.SessionKey.ReportPath] = reportNodePath;

			return reportArrayList;
		}

		//--------------------------------------------------------------------------------------
		private void SetCulture()
		{
			//Setto la culture
			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				this.RedirectToLogin();
				return;
			}

			ui.SetCulture();

		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Funzione che ordina un array di RunnedReport per data decrescente
		/// </summary>
		/// <param name="report"></param>
		/// <returns></returns>
		private ArrayList GetReportByDate(RunnedReport[] report)
		{
			ArrayList reportArrayList = new ArrayList(report);
			reportArrayList.Sort(new RunnedReportComparer());
			return reportArrayList;
		}
		#endregion
		
		#region funzione che leggi setting del PlugIn
		/// <summary>
		/// Funzione che applica i settaggi custom impostati per l'Utente attualmente
		/// logato all'applicazione
		/// </summary>
		private void ApplyCustomSettings()
		{
			

			if (Session[EasyLookCustomSettings.SessionKey] == null)
				return;

			EasyLookCustomSettings	setting	= (EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey];

			if (setting == null)
				return;

			if (setting.FontFamily != string.Empty || setting.FontFamily != "")
			{
				historyDataGrid.Font.Name		= setting.FontFamily;
				historyDataGrid.Font.Size		= FontUnit.XSmall;
			}
			else
			{
				historyDataGrid.CssClass	= "Command_Element";
			}
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzioni che settano il DataSource del DataGrid e il suo Layout
		/// <summary>
		/// Funzione che setta la proprietà DataSource del DataGrid contenete i 
		/// l'elenco dei report storicizzati, e setta il layout del DataGrid
		/// </summary>
		/// <param name="report"></param>
		/// <param name="reportNodePath"></param>
		private void SetDataGrid(string reportNodePath, string reportArguments)
		{
			ArrayList reportArrayList = GetHistory(reportNodePath);
			//Creo il DataSource x il dataGrid
			historyDataGrid.DataSource = CreateDataSource(reportArrayList, reportNodePath, reportArguments);
			if (historyDataGrid.DataSource == null)
				return;

			historyDataGrid.DataBind();
		}
		//---------------------------------------------------------------------
		
		/// <summary>
		/// Funzione che crea una dataView contenente l'elenco dei report storicizzati.
		/// Diventerà il DataSource del DataGrid.
		/// </summary>
		/// <param name="report"></param>
		/// <param name="reportNameSpaceString"></param>
		/// <returns></returns>
		private DataView CreateDataSource(ArrayList report, string reportNameSpaceString, string reportParameters)
		{
			if (reportNameSpaceString == null || reportNameSpaceString == string.Empty )
				return null;

			DataTable	dataTable	= new DataTable();
			DataRow		dataRow		= null;
			
			//Struttura del DataSource
			dataTable.Columns.Add(new DataColumn("NomeReport",	typeof(string)));
			dataTable.Columns.Add(new DataColumn("User",		typeof(string)));
			dataTable.Columns.Add(new DataColumn("Data",		typeof(DateTime)));
			dataTable.Columns.Add(new DataColumn("Link",		typeof(string)));
			dataTable.Columns.Add(new DataColumn("NomeFile",	typeof(string)));

			//Inserisco tutti i report storicizzati
			foreach(RunnedReport runnedReport in report)
			{
				dataRow = dataTable.NewRow();

				if( runnedReport.Description != string.Empty )
					dataRow["NomeReport"]	= runnedReport.Description; 
				else
					dataRow["NomeReport"]	= Session[SessionKey.ReportTitle].ToString();

				dataRow["User"]			= runnedReport.User;
				dataRow["Data"]			= runnedReport.TimeStamp;
				dataRow["Link"]			= string.Format(
											"javascript:parent.OpenPopUpNewReportFromFilename('{0}')", 
											HttpUtility.UrlEncode(runnedReport.FilePath.Replace("\\", "/"))
											);
				dataRow["NomeFile"]		= runnedReport.FilePath;
				dataTable.Rows.Add(dataRow);
			}
			DataView dv			= new DataView(dataTable);
			dv.Sort = currentSort;
			return dv;
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzione che ritorna i report storicizzati
		/// <summary>
		/// Funzione che ritorna l'elenco dei report storicizzati per tutti gli Utenti, o per 
		/// l'Utente corrente
		/// </summary>
		/// <param name="reportNodePath"></param>
		/// <returns></returns>
		private RunnedReport[]  GetRunnedReport(string reportNodePath)
		{
			if (UserInfo.FromSession() == null)
				return null;

			IPathFinder pathFinder			= UserInfo.FromSession().PathFinder;
			RunnedReportMng runnedReportMng = new RunnedReportMng();
			NameSpace reportNameSpace		= new NameSpace(reportNodePath, NameSpaceObjectType.Report);

			if (Session[SessionKey.ReportType].ToString() == "CurrentUser")
				return runnedReportMng.GetRunnedReports(reportNameSpace, pathFinder.Company, pathFinder.User);
			
			ArrayList array = new ArrayList();
			RunnedReport[] report = runnedReportMng.GetRunnedReports(reportNameSpace, pathFinder.Company, pathFinder.User);
			array.AddRange(report);
			report = runnedReportMng.GetRunnedReports(reportNameSpace, pathFinder.Company, "AllUsers");
			array.AddRange(report);
			
			return (RunnedReport[])array.ToArray(typeof(RunnedReport));
		}
		//---------------------------------------------------------------------
		#endregion


		#region cambio dei report selezionati
		/// <summary>
		/// Evento di selezione e/o deselezione di un Report nel DataGrid dei report
		/// storicizzati 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void DataGrid_ChekChanged(object sender,  System.EventArgs e)
		{
			CheckBox	checkBox		= (CheckBox)sender;
			TableCell	fileNameCell	= (TableCell)checkBox.Parent.Parent.Controls[4];
			
			if (checkBox.Checked == true)
			{
				if (Session[SessionKey.ReportsToDeleting] == null)
					reports = new ArrayList();
				else
					reports = (ArrayList) Session[SessionKey.ReportsToDeleting];

				reports.Add(fileNameCell.Text);
			}
			else
			{
				reports = (ArrayList) Session[SessionKey.ReportsToDeleting];
				if (reports == null)
				{
					reports = new ArrayList();
					
				}

				foreach (string reportName in reports)
				{
					if (string.Compare(reportName, fileNameCell.Text)==0)
					{
						reports.Remove(reportName);
						break;
					}
				}
				
			}
			Session.Add(SessionKey.ReportsToDeleting, reports);
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzioni che cancellano i report
		/// <summary>
		/// Funzione che cancella fisicamente i report selezionati all'interno del 
		/// DataGrid dei report storicizzati
		/// </summary>
		private void DeleteReport()
		{
			RunnedReport report = null;
			int i = ((ArrayList)Session[SessionKey.ReportsToDeleting]).Count;
			for(i=0; i< ((ArrayList)Session[SessionKey.ReportsToDeleting]).Count; i++)
			{
				string filename = ((ArrayList)Session[SessionKey.ReportsToDeleting])[i].ToString();
				report = GetRunnedReportForDeleting(filename);
				if (report == null) 
					continue;
				report.Delete();
			}
			Session[SessionKey.ReloadHistory]		= string.Empty;
			Session[SessionKey.ReportsToDeleting]	= null;
		}
		//---------------------------------------------------------------------
		
		private RunnedReport GetRunnedReportForDeleting(string filename)
		{
			foreach (RunnedReport report in (ArrayList)Session[SessionKey.Reports])
			{
				if (string.Compare(report.FilePath, filename)==0)
					return report;
			}
			return null;
		}
		//---------------------------------------------------------------------

		#endregion

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

		#region funzione pulsante cancella lancia il JavaScript
		/// <summary>
		/// Evento di Click sul pulsante "Cancella Selezionati"
		/// Inserisce lo script di cancellazione
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void deleteReportButton_Click(object sender, System.EventArgs e)
		{
			SetLocalizableLable();
			string scriptValue = "";
			
			if (Session[SessionKey.ReportsToDeleting] == null || ((ArrayList)Session[SessionKey.ReportsToDeleting]).Count == 0)
			{
				scriptValue	=  @"<script>																
									function Delete()
									 {
										alert('" + LabelStrings.NoSelectedReports + @"');

									 }																
									</script>";	
			}
			else
			{
				scriptValue	=  @"<script>																
									function Delete()
									 {
										window.location='HistoryPage.aspx';

									 }																
									</script>";

				Session.Add(SessionKey.ReloadHistory, "Delete"); 
			}
            ClientScript.RegisterClientScriptBlock(ClientScript.GetType(), "del", scriptValue);
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzioni per localizzare le stringhe della pagina
		/// <summary>
		/// Funzione che localizza le stringhe presenti nella pagina
		/// </summary>
		private void SetLocalizableLable()
		{
			this.refreshBtn.ToolTip = LabelStrings.RefreshReportList;
			this.runReportBtn.ToolTip = LabelStrings.RunReportToolTip;
			this.deleteBtn.ToolTip = LabelStrings.CancelSelectedReports;

			this.historyDataGrid.Columns[0].HeaderText = "&nbsp;" + LabelStrings.ReportLabel + "&nbsp;";
			this.historyDataGrid.Columns[1].HeaderText = "&nbsp;" + LabelStrings.UserLabel + "&nbsp;";
			this.historyDataGrid.Columns[2].HeaderText = "&nbsp;" + LabelStrings.DataLabel + "&nbsp;";
		}
		//---------------------------------------------------------------------
		#endregion
	}
}
