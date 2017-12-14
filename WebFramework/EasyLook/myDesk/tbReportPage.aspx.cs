using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;

namespace Microarea.Web.EasyLook.myDesk
{
	public partial class tbReportPage : System.Web.UI.Page
	{
		#region dichiarazione dataMember
		/// <summary>
		/// Path del nodo selezionato nel tree del men? 
		/// Padre dei report che andremo ad elencare
		/// </summary>
		protected string selectedNodePath;
		/// <summary>
		/// Titolo del men?selezionato
		/// </summary>
		protected string menuTitle;
		/// <summary>
		/// Data Member d'appoggio per le ricorsive
		/// </summary>
		protected MenuXmlNode node;
		/// <summary>
		/// Data Member che conterr?gli eventuali settaggi custom
		/// </summary>
		protected EasyLookCustomSettings easyLookCustomSettings = null;

		public const string ReportParametersTableKey = "__reportParametersTable";

		#endregion

		#region dichiarazione dei controlli
		/// <summary>
		/// Icon del Report; sul click run del report selezionato
		/// </summary>
		protected System.Web.UI.WebControls.ImageButton commandImage;
		/// <summary>
		/// HyperLink identificata dal nome del report selezionandola viene
		/// visualizzato il suo storico
		/// </summary>
		protected System.Web.UI.WebControls.HyperLink commandHyperLink;
		/// <summary>
		/// Label contenente il titolo localizzato della pagina
		/// </summary>
		protected System.Web.UI.WebControls.Label TitleLabel;
		/// <summary>
		/// Tabella contenente l'elenco dei report
		/// </summary>
		protected System.Web.UI.WebControls.Table reportTable;
		/// <summary>
		/// Label contenente la data di creazione del File
		/// </summary>
		protected System.Web.UI.WebControls.Label creationDateLabel;
		/// <summary>
		/// Label contenente la data di ultima modifica del File
		/// </summary>
		protected System.Web.UI.WebControls.Label lastModifyDateLabel;

		/// <summary>
		/// Label contenente la descrizione del File
		/// </summary>
		protected System.Web.UI.WebControls.Label reportDescriptionLabel;

		/// <summary>
		/// return string html
		/// </summary>
		protected string rJson = "";

		#endregion

		#region load della pagina
		/// <summary>
		/// Evento di Load della pagina; setta la CaChe a NoCache Applica le eventuali
		/// personalizzazioni effettuate tramite il PlugIn della Console visualizza un 
		/// elenco dei report presenti nel men?selezionato
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				// this.RedirectToLogin();
				return;
			}

			
			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			//Setto la culture
			ui.SetCulture();

			//prendo i valori dalla QueryString
			string queryValue = HttpContext.Current.Request.QueryString["NameSpace"];
			selectedNodePath = string.Empty;

			if (queryValue != string.Empty && queryValue != null)
			{
				string menuNodePathDelimStr = MenuXmlNode.ActionMenuPathSeparator;
				char[] menuNodePathDelimiter = menuNodePathDelimStr.ToCharArray();
				string[] menuNodePathSplitArguments = queryValue.Split(menuNodePathDelimiter);

				for (int i = 0; i < menuNodePathSplitArguments.Length; i++)
				{
					selectedNodePath += menuNodePathSplitArguments[i].DecodeBase16();
					if (i < menuNodePathSplitArguments.Length - 1)
						selectedNodePath += @"\";
				}
			}
			selectedNodePath = selectedNodePath.Replace(@"\\", @"\");
			menuTitle = HttpContext.Current.Request.QueryString["Title"];
			if (menuTitle != string.Empty && menuTitle != null)
				menuTitle = menuTitle.DecodeBase16();

			//Localizzo le stringhe 
			SetLocalizedLabel();
			//Applco gli eventuali settaggi custom del PlugIn
			ApplyCustomSetting();

			if (selectedNodePath == null || selectedNodePath == string.Empty)
				return;

			//Carico i report
			MenuXmlParser parser = Helper.GetMenuXmlParser();
			if (parser == null)
				return;

			LoadReportXmlParser(parser);

		}
		//---------------------------------------------------------------------
		#endregion

		//---------------------------------------------------------------------
		#region load della pagina
		protected override void Render(HtmlTextWriter writer)
		{
			var sw = new System.IO.StringWriter();
			var tw = new HtmlTextWriter(sw);
			base.Render(tw);
			
			rJson = "{\"menudata\": [" + rJson + "]}";
			Response.Clear();
			Response.ContentType = "application/json; charset=utf-8";
			Response.Write(rJson);
			Response.Flush();
			Response.End(); 

		}
		#endregion

		#region funzioni per caricare la lista dei reports
		/// <summary>
		/// Funzione che dato un MenuXmlParser trova il nodo di men?
		/// selezionato e richiama una funzione che creer?la lista
		/// dei report presenti nel men?stesso
		/// </summary>
		/// <param name="parser"></param>
		private void LoadReportXmlParser(MenuXmlParser parser)
		{
			if (selectedNodePath == null || selectedNodePath == String.Empty)
				return;

			string menuNodePathDelimStr = MenuXmlNode.ActionMenuPathSeparator;
			char[] menuNodePathDelimiter = menuNodePathDelimStr.ToCharArray();
			string[] menuNodePathSplitArguments = selectedNodePath.Split(menuNodePathDelimiter);

			if (menuNodePathSplitArguments.Length < 3)
				return;

			string applicationName = menuNodePathSplitArguments[0];
			if (applicationName == null || applicationName == String.Empty)
				return;

			string groupName = menuNodePathSplitArguments[1];
			if (groupName == null || groupName == String.Empty)
				return;

			MenuXmlNode applicationNode = parser.GetApplicationNodeByName(applicationName);
			if (applicationNode == null)
				return;

			MenuXmlNode groupNode = applicationNode.GetGroupNodeByName(menuNodePathSplitArguments[1]);
			if (groupNode == null)
				return;

			if (!groupNode.HasMenuChildNodes())
			{
				node = groupNode;
				return;
			}
			else
			{
				MenuXmlNode parentNode = groupNode;

				for (int i = 2; i < menuNodePathSplitArguments.Length; i++)
				{
					MenuXmlNode childMenuNode = parentNode.GetMenuNodeByTitle(menuNodePathSplitArguments[i]);
					if (childMenuNode == null)
						break;
					parentNode = childMenuNode;
				}
				node = parentNode;
			}

			LoadReportList();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che crea lalista dei report presenti nel men?
		/// </summary>
		private void LoadReportList()
		{
			if (node == null || node.CommandItems == null || node.CommandItems.Count == 0)
			{
				CreateNoReportLayout();
				return;
			}

			// in this table I store the parameters (I can't pass them in QueryString because they
			// are too large)
			Hashtable parametersTable = Session[ReportParametersTableKey] as Hashtable;
			if (parametersTable == null)
			{
				parametersTable = new Hashtable();
				Session[ReportParametersTableKey] = parametersTable;
			}
			else
			{
				parametersTable.Clear();
			}

			// int paramCount = 0;
			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				this.RedirectToLogin();
				return;
			}
			foreach (MenuXmlNode commandNode in node.CommandItems)
			{

				if (rJson != "")
					rJson += ",";

				if (commandNode.IsRunReport)
				{
					rJson += "{";
					rJson += "\"NameSpaceType\" : " + "\"report\",";
					rJson += "\"Title\" : " + "\"" + commandNode.Title + "\",";
					rJson += "\"Url\" : " + "\"" + HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.Report, commandNode.ItemObject)) + "\",";
					rJson += "\"arguments\" : " + "\"" + Microarea.TaskBuilderNet.Woorm.WoormWebControl.Helper.FormatParametersForRequest(commandNode.ArgumentsOuterXml) + "\"";
					rJson += "}";
				}
				else if (commandNode.IsRunFunction)
				{
					rJson += "{";
					rJson += "\"NameSpaceType\" : " + "\"fuction\",";
					rJson += "\"Title\" : " + "\"" + commandNode.Title + "\",";
					rJson += "\"Url\" : " + "\"" + HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.Function, commandNode.ItemObject)) + "\"";
					rJson += "}";
				}
				else if (commandNode.IsRunDocument || commandNode.IsRunBatch)
				{
					rJson += "{";
					if (commandNode.IsRunDocument)
						rJson += "\"NameSpaceType\" : " + "\"document\",";
					else
						rJson += "\"NameSpaceType\" : " + "\"runbatch\",";

					rJson += "\"Title\" : " + "\"" + commandNode.Title  + "\",";
					rJson += "\"Url\" : " + "\"" + HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.Document, commandNode.ItemObject)) + "\"";
					rJson += "}";
				}
				else if (commandNode.IsRunExecutable)
				{
					rJson += "{";
					rJson += "\"NameSpaceType\" : " + "\"application\",";
					rJson += "\"Title\" : " + "\"" + commandNode.Title + "\",";
					rJson += "\"Url\" : " + "\"" + HttpUtility.UrlEncode(string.Format("{0}", NameSpaceSegment.Application)) + "\",";
					rJson += "\"arguments\" : " + "\"" + HttpUtility.UrlEncode(string.Format("{0}", commandNode.ItemObject)) + "\"";
					rJson += "}";
				}
				else if (commandNode.IsRunText)
				{
					rJson += "{";
					rJson += "\"NameSpaceType\" : " + "\"text\",";
					rJson += "\"Title\" : " + "\"" + commandNode.Title + "\",";
					rJson += "\"Url\" : " + "\"" + HttpUtility.UrlEncode(string.Format("{0}", NameSpaceSegment.Text)) + "\",";
					rJson += "\"arguments\" : " + "\"" + HttpUtility.UrlEncode(string.Format("{0}", commandNode.ItemObject)) + "\"";
					rJson += "}";
				}
				else if (commandNode.IsOfficeItem)
				{
					
					rJson += "{";
					rJson += "\"Title\" : " + "\"" + commandNode.Title + "\",";

					if (commandNode.IsOfficeDocument)
					{
						if (commandNode.IsWordItem)
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.WordDocument + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.WordDocument, commandNode.ItemObject)),
													"\",");
						}
						else if (commandNode.IsExcelItem)
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.ExcelDocument + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.ExcelDocument, commandNode.ItemObject)),
													"\",");
						}
						else
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.NotValid + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.NotValid, commandNode.ItemObject)),
													"\",");
						}
					}
					else if (commandNode.IsOfficeDocument2007)
					{
						if (commandNode.IsWordItem)
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.WordDocument2007 + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.WordDocument2007, commandNode.ItemObject)),
													"\",");
						}
						else if (commandNode.IsExcelItem)
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.ExcelDocument2007 + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.ExcelDocument2007, commandNode.ItemObject)),
													"\",");
						}
						else
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.NotValid + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.NotValid, commandNode.ItemObject)),
													"\",");
						}

						
					}
					else if (commandNode.IsOfficeTemplate)
					{
						if (commandNode.IsWordItem)
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.WordTemplate + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.WordTemplate, commandNode.ItemObject)),
													"\",");
						}
						else if (commandNode.IsExcelItem)
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.ExcelTemplate + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.ExcelTemplate, commandNode.ItemObject)),
													"\",");
						}
						else
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.NotValid + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.NotValid, commandNode.ItemObject)),
													"\",");
						}
					}
					else if (commandNode.IsOfficeTemplate2007)
					{
						if (commandNode.IsWordItem)
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.WordTemplate2007 + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.WordTemplate2007, commandNode.ItemObject)),
													"\",");
						}
						else if (commandNode.IsExcelItem)
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.ExcelTemplate2007 + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.ExcelTemplate2007, commandNode.ItemObject)),
													"\",");
						}
						else
						{
							rJson += "\"NameSpaceType\" : " + "\"" + NameSpaceSegment.NotValid + "\",";
							rJson += string.Concat("\"Url\" : \"",
													HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.NotValid, commandNode.ItemObject)),
													"\",");
						}
					}
					rJson += "\"arguments\" : " + "\"\"";
					rJson += "}";
				}
				else
				{
					rJson += "{";
					rJson += "\"NameSpaceType\" : " + "\"TODO\",";
					rJson += "\"Title\" : " + "\"TODO:" + commandNode.Title + "\",";
					rJson += "\"Url\" : " + "\"TODO\"";
					rJson += "}";

					// tutte le atre continuare Todo:dp !!!
				}
				
			}

		}

		//---------------------------------------------------------------------
		private void CreateNoReportLayout()
		{
			TableRow reportInformationTableRow = new TableRow();

			commandHyperLink = new HyperLink();
			commandHyperLink.Text = LabelStrings.NoMenuItems;

			TableCell labelTableCell = new TableCell();
			labelTableCell.Controls.Add(commandHyperLink);
			reportInformationTableRow.Cells.Add(labelTableCell);
			labelTableCell.HorizontalAlign = HorizontalAlign.Center;

			if (reportTable == null)
				return;

			reportTable.Rows.Add(reportInformationTableRow);

			if (easyLookCustomSettings.FontFamily == string.Empty || easyLookCustomSettings.FontFamily == "")
			{
				commandHyperLink.CssClass = "Command_Element";
				commandHyperLink.Font.Bold = true;
			}
			else
			{
				commandHyperLink.Font.Name = easyLookCustomSettings.FontFamily;
				commandHyperLink.Font.Bold = true;
			}

		}

		//---------------------------------------------------------------------
		public static string AdjustImagePath(MenuXmlNode commandNode, UserInfo ui)
		{
			if (commandNode != null && commandNode.IsCommand)
			{
				if (commandNode.IsRunReport)
				{
					IPathFinder menuPathFinder = ui.PathFinder;
					if (menuPathFinder != null)
						MenuInfo.SetMenuCommandOrigin(menuPathFinder, commandNode, ui.LoginManager.PreferredLanguage);

					if (commandNode.CommandOrigin == CommandOrigin.CustomCurrentUser)
						return PathImageStrings.CurrentUserReportImagePath;
					else if (commandNode.CommandOrigin == CommandOrigin.CustomAllUsers)
						return PathImageStrings.AllUsersReportImagePath;
					else
						return PathImageStrings.ReportImagePath;
				}
				else if (commandNode.IsRunBatch)
					return PathImageStrings.BatchImagePath;
				else if (commandNode.IsRunDocument)
					return PathImageStrings.DocumentImagePath;
				else if (commandNode.IsRunFunction)
					return PathImageStrings.FunctionImagePath;

			}

			return string.Empty;
		}
		//---------------------------------------------------------------------
		/*
		public static Color AdjustForeColor(MenuXmlNode reportNode, UserInfo ui)
		{
			if (reportNode != null && reportNode.IsCommand && ui != null)
			{
				IPathFinder menuPathFinder = ui.PathFinder;
				if (menuPathFinder != null)
					MenuInfo.SetMenuCommandOrigin(menuPathFinder, reportNode, ui.LoginManager.PreferredLanguage);

				if (reportNode.CommandOrigin == CommandOrigin.CustomCurrentUser)
					return DefaultSettings.currentUserCommandForeColor;
				else if (reportNode.CommandOrigin == CommandOrigin.CustomAllUsers)
					return DefaultSettings.allUsersCommandForeColor;

			}

			return DefaultSettings.standardCommandForeColor;

		}
		*/
		#endregion

		#region funzione x localizzare le stringhe
		/// <summary>
		/// Funzione che setta le stringhe presenti nella pagina secondo la culture 
		/// dell'Utente corrente
		/// </summary>
		private void SetLocalizedLabel()
		{
			/*
			TitleLabel.Text = LabelStrings.ItemsList + ": " + menuTitle;
			DescriptionCheckBox.Text = LabelStrings.DescriptionCheck;
			DataCheckBox.Text = LabelStrings.DataCheck;
			*/

		}
		//---------------------------------------------------------------------
		#endregion

		#region funzione per applicare i setting custom
		/// <summary>
		/// Funzione che applica gli eventuali settaggi custom impostati dall'Utente
		/// tramite il PlugIn della Console
		/// </summary>
		private void ApplyCustomSetting()
		{
			/*
			easyLookCustomSettings = (EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey];


			if (easyLookCustomSettings == null)
				return;

			DefaultSettings.currentUserCommandForeColor = Color.FromArgb(easyLookCustomSettings.CurrentUserReportTitleColor);
			DefaultSettings.allUsersCommandForeColor = Color.FromArgb(easyLookCustomSettings.AllUsersReportTitleColor);

			if (easyLookCustomSettings.FontFamily != string.Empty || easyLookCustomSettings.FontFamily != "")
			{
				this.TitleLabel.Font.Name = easyLookCustomSettings.FontFamily;
				DataCheckBox.Font.Name = easyLookCustomSettings.FontFamily;
				DescriptionCheckBox.Font.Name = easyLookCustomSettings.FontFamily;
			}

			int commandBackGround = ((EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey]).CommandListBkgndColor;

			if (commandBackGround == -1)
				return;

			HtmlForm form = (HtmlForm)this.FindControl("reportForm");
			Color appPanelBkgndColor = Color.FromArgb(commandBackGround);

			string bkgndColorString = HtmlUtility.ToHtml(appPanelBkgndColor);
			form.Style.Add("BACKGROUND-COLOR", bkgndColorString);
			*/
		}

		//---------------------------------------------------------------------
		#endregion

		#region Web Form Designer generated code
		/// <summary>
		/// Questa chiamata ?richiesta da Progettazione Web Form ASP.NET.
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

	}
}