using System;
using System.Collections;
using System.Drawing;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;

namespace Microarea.Web.EasyLook
{
	/// <summary>
	/// Summary description for ReportPage.
	/// </summary>
	public partial class ReportPage : System.Web.UI.Page
	{
		#region dichiarazione dataMember
		/// <summary>
		/// Path del nodo selezionato nel tree del men? 
		/// Padre dei report che andremo ad elencare
		/// </summary>
		protected string					selectedNodePath; 
		/// <summary>
		/// Titolo del men?selezionato
		/// </summary>
		protected string					menuTitle;
		/// <summary>
		/// Data Member d'appoggio per le ricorsive
		/// </summary>
		protected MenuXmlNode				node;
		/// <summary>
		/// Data Member che conterr?gli eventuali settaggi custom
		/// </summary>
		protected EasyLookCustomSettings	easyLookCustomSettings = null;

		public const string ReportParametersTableKey = "__reportParametersTable";

		#endregion

		#region dichiarazione dei controlli
		/// <summary>
		/// Icon del Report; sul click run del report selezionato
		/// </summary>
		protected System.Web.UI.WebControls.ImageButton		commandImage;
		/// <summary>
		/// HyperLink identificata dal nome del report selezionandola viene
		/// visualizzato il suo storico
		/// </summary>
		protected System.Web.UI.WebControls.HyperLink		commandHyperLink;
		/// <summary>
		/// Label contenente il titolo localizzato della pagina
		/// </summary>
		protected System.Web.UI.WebControls.Label			TitleLabel;
		/// <summary>
		/// Tabella contenente l'elenco dei report
		/// </summary>
		protected System.Web.UI.WebControls.Table			reportTable;
		/// <summary>
		/// Label contenente la data di creazione del File
		/// </summary>
		protected System.Web.UI.WebControls.Label			creationDateLabel;
		/// <summary>
		/// Label contenente la data di ultima modifica del File
		/// </summary>
		protected System.Web.UI.WebControls.Label			lastModifyDateLabel;

		/// <summary>
		/// Label contenente la descrizione del File
		/// </summary>
		protected System.Web.UI.WebControls.Label			reportDescriptionLabel;
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
				this.RedirectToLogin();
				return;
			}
			
			if (!this.IsPostBack)
			{
				//Metto a null la session con il nome del report da runnare
				if (Session[Microarea.TaskBuilderNet.Woorm.WoormWebControl.SessionKey.ReportPath] != null &&
					Session[Microarea.TaskBuilderNet.Woorm.WoormWebControl.SessionKey.ReportPath].ToString() != string.Empty)
					Session.Add(Microarea.TaskBuilderNet.Woorm.WoormWebControl.SessionKey.ReportPath, null);

			}
			else
			{
				//metto in session il valore dei flag
				Session.Add(SessionKey.ShowDate, DataCheckBox.Checked);
				Session.Add(SessionKey.ShowDescription, DescriptionCheckBox.Checked);
			}

			if (Session[SessionKey.ShowDate] != null)
				DataCheckBox.Checked = Convert.ToBoolean(Session[SessionKey.ShowDate]);

			if (Session[SessionKey.ShowDescription] != null)
				DescriptionCheckBox.Checked = Convert.ToBoolean(Session[SessionKey.ShowDescription]);

			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			//Setto la culture
			ui.SetCulture();
	
			//prendo i valori dalla QueryString
			string queryValue = HttpContext.Current.Request.QueryString["NameSpace"];
			selectedNodePath = string.Empty;
			
			if (queryValue != string.Empty && queryValue != null)
			{			
				string		menuNodePathDelimStr		= MenuXmlNode.ActionMenuPathSeparator;
				char []		menuNodePathDelimiter		= menuNodePathDelimStr.ToCharArray();
				string []	menuNodePathSplitArguments	= queryValue.Split(menuNodePathDelimiter);
			
				for( int i=0; i< menuNodePathSplitArguments.Length; i++)
				{
                    selectedNodePath += menuNodePathSplitArguments[i].DecodeBase16();
					if ( i< menuNodePathSplitArguments.Length -1)
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
			
			if (selectedNodePath== null || selectedNodePath == string.Empty)
				return;

			//Carico i report
			MenuXmlParser parser = Helper.GetMenuXmlParser();
			if (parser == null)
				return;

			LoadReportXmlParser(parser);			
			
		}
		//---------------------------------------------------------------------
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

			string		menuNodePathDelimStr		= MenuXmlNode.ActionMenuPathSeparator;
			char []		menuNodePathDelimiter		= menuNodePathDelimStr.ToCharArray();
			string []	menuNodePathSplitArguments	= selectedNodePath.Split(menuNodePathDelimiter);

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

			if(!groupNode.HasMenuChildNodes())
			{
					node =  groupNode;
					return;
			}
			else
			{
				MenuXmlNode parentNode = groupNode;
				
				for (int i=2; i< menuNodePathSplitArguments.Length; i++)
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
			TableRow  reportInformationTableRow	= null; 
			TableRow  creationDataTableRow		= null;
			TableRow  lastModifyDataTableRow	= null;
			TableRow  descriptionTableRow		= null;

			TableCell imageTableCell			= null; 	
			TableCell labelTableCell			= null;
 
			TableCell creationDataTableCell		= null; 	
			TableCell lastModidyDataTableCell	= null; 

			TableCell blanckTableCell			= null; 	
			TableCell descriptionTableCell		= null; 
			TableCell blanckTableCell1	= null;
			TableCell blanckTableCell2 = null;
			
			if (node == null || node.CommandItems == null || node.CommandItems.Count== 0)
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

			int paramCount = 0;
			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				this.RedirectToLogin();
				return;
			}
			foreach (MenuXmlNode commandNode in node.CommandItems)
			{
				if (!commandNode.IsRunReport 
					&& !commandNode.IsRunBatch 
					&& !commandNode.IsRunDocument 
					&& !commandNode.IsRunFunction) 
					continue;
				
				//HyperLink
				string runlink = AddCommandHyperlink(commandNode);
				//Immagine
				AddCommandImage(parametersTable, commandNode, paramCount, runlink);

				if (ui.PathFinder != null)
				{
					MenuInfo.SetExternalDescription(ui.PathFinder, commandNode);
					MenuInfo.SetReportFileDateTimes(ui.PathFinder, commandNode);
				}

				//Cella x l'immagine
				imageTableCell	= new TableCell();
				imageTableCell.Width = Unit.Percentage (5);
				imageTableCell.Controls.Add(commandImage);
				
				//Cella per la label
				labelTableCell	= new TableCell();
				labelTableCell.Controls.Add(commandHyperLink);

				//Le aggiungo alla riga 
				reportInformationTableRow	= new TableRow();
				reportInformationTableRow.Cells.Add(imageTableCell);
				reportInformationTableRow.Cells.Add(labelTableCell);

				//Aggiungo la riga alla tabell
				reportTable.Rows.Add(reportInformationTableRow);

				//Descrizione
				reportDescriptionLabel		= new Label();
				reportDescriptionLabel.Text = commandNode.ExternalDescription;
				if(easyLookCustomSettings.FontFamily == string.Empty || easyLookCustomSettings.FontFamily == "")
					reportDescriptionLabel.CssClass	= "Command_Element";
				else
				{
					reportDescriptionLabel.Font.Name = easyLookCustomSettings.FontFamily;
				}

				descriptionTableRow			= new TableRow();
				blanckTableCell				= new TableCell();
				descriptionTableCell		= new TableCell();	

				blanckTableCell.ColumnSpan = 1;
				descriptionTableCell.ColumnSpan = 1;
				descriptionTableCell.Controls.Add(reportDescriptionLabel);
			
				descriptionTableRow.Cells.Add(blanckTableCell);
				descriptionTableRow.Cells.Add(descriptionTableCell);
				reportTable.Rows.Add(descriptionTableRow);
				descriptionTableRow.Visible = DescriptionCheckBox.Checked;
			

				//creationData
				creationDateLabel = new Label();
				creationDateLabel.Text = LabelStrings.CreationDate + commandNode.ReportFileCreationTime.ToString();
				if(easyLookCustomSettings.FontFamily == string.Empty || easyLookCustomSettings.FontFamily == "")
					creationDateLabel.CssClass	= "Command_Element";
				else
				{
					creationDateLabel.Font.Name = easyLookCustomSettings.FontFamily;
				}

				//lastModidyData
				lastModifyDateLabel = new Label();
				lastModifyDateLabel.Text = LabelStrings.LastModifyDate + commandNode.ReportFileLastWriteTime.ToString();
				if(easyLookCustomSettings.FontFamily == string.Empty || easyLookCustomSettings.FontFamily == "")
					lastModifyDateLabel.CssClass	= "Command_Element";
				else
				{
					lastModifyDateLabel.Font.Name = easyLookCustomSettings.FontFamily;
				}

				creationDataTableRow		= new TableRow();
				lastModifyDataTableRow		= new TableRow();

				creationDataTableCell		= new TableCell();
				lastModidyDataTableCell		= new TableCell();

				creationDataTableCell.Controls.Add(creationDateLabel);
				lastModidyDataTableCell.Controls.Add(lastModifyDateLabel);

				blanckTableCell1	= new TableCell();
				blanckTableCell1.ColumnSpan = 1;
				creationDataTableRow.Cells.Add(blanckTableCell1);
				creationDataTableRow.Cells.Add(creationDataTableCell);
				reportTable.Rows.Add(creationDataTableRow);

				blanckTableCell2	= new TableCell();
				blanckTableCell2.ColumnSpan = 1;
				lastModifyDataTableRow.Cells.Add(blanckTableCell2);	
				lastModifyDataTableRow.Cells.Add(lastModidyDataTableCell);
				lastModifyDataTableRow.Visible = DataCheckBox.Checked;			
				creationDataTableRow.Visible = DataCheckBox.Checked;
				reportTable.Rows.Add(lastModifyDataTableRow);
				
			}
		}

		//---------------------------------------------------------------------
		private string AddCommandHyperlink(MenuXmlNode commandNode)
		{
			commandHyperLink = new HyperLink();
			commandHyperLink.Text = commandNode.Title;

			if (easyLookCustomSettings.FontFamily == string.Empty || easyLookCustomSettings.FontFamily == "")
				commandHyperLink.CssClass = "Command_Element";
			else
			{
				commandHyperLink.Font.Name = easyLookCustomSettings.FontFamily;
			}

			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				this.RedirectToLogin();
				return String.Empty;
			}
			commandHyperLink.ForeColor = AdjustForeColor(commandNode, ui);

			if (commandNode.IsRunReport)
			{
				commandHyperLink.ToolTip = LabelStrings.RunReportToolTip;
				if (!commandNode.GetRunNativeAttribute())
				{
                    commandHyperLink.NavigateUrl = string.Format("javascript:parent.OpenPopUpNewReport('{0}', '{1}')",
                            HttpUtility.UrlEncode(commandNode.ItemObject),
                            Microarea.TaskBuilderNet.Woorm.WoormWebControl.Helper.FormatParametersForRequest(commandNode.ArgumentsOuterXml)
                            );
                    commandHyperLink.NavigateUrl = string.Format("javascript:parent.OpenPopupApiReport('{0}', '{1}')",
                                HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.Report, commandNode.ItemObject)),
                                ""  //todo parametri
                                );

                }
                else
				{
                    //commandHyperLink.NavigateUrl = string.Format("javascript:parent.OpenPopupDocument('{0}')",
                    //			HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.Report, commandNode.ItemObject))
                    //			);
                    commandHyperLink.NavigateUrl = string.Format("javascript:parent.OpenPopupApiReport('{0}', '{1}')",
                                 HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.Report, commandNode.ItemObject)),
                                 ""  //todo parametri
                                 );

                }
            }
			else if (commandNode.IsRunFunction)
			{
				commandHyperLink.ToolTip = LabelStrings.RunFunctionToolTip;
				commandHyperLink.NavigateUrl = string.Format("javascript:parent.OpenPopupDocument('{0}')",
							HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.Function, commandNode.ItemObject))
							);
			}
			else
			{
				commandHyperLink.ToolTip = LabelStrings.RunDocumentToolTip;
				commandHyperLink.NavigateUrl = string.Format("javascript:parent.OpenPopupDocument('{0}')",
							HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.Document, commandNode.ItemObject))
							);
			}

			return commandHyperLink.NavigateUrl;
		}

		//---------------------------------------------------------------------
		private void AddCommandImage(Hashtable parametersTable, MenuXmlNode commandNode, int paramCount, string link)
		{
			commandImage = new ImageButton();

			link = link.Replace("'", "\\'");

			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				this.RedirectToLogin();
				return;
			}
			commandImage.ImageUrl = AdjustImagePath(commandNode, ui);
            string title = commandNode.Title.EncodeBase16();

			if (commandNode.IsRunReport)
			{
				string paramID = "";
				if (commandNode.ArgumentsOuterXml.Length > 0)
				{
					paramID = (paramCount++).ToString();
					parametersTable[paramID] = Microarea.TaskBuilderNet.Woorm.WoormWebControl.Helper.FormatParametersForRequest(commandNode.ArgumentsOuterXml);
				}
				commandImage.ToolTip = LabelStrings.ShowHistoryToolTip;
				commandImage.Attributes["onClick"] = string.Format(
					"javascript:doLink('HistoryPage.aspx?NameSpace={0}&Title={1}&ParametersID={2}', '" + link + "' );",
					commandNode.ItemObject,
					title,
					paramID
					);
			} 
#if TBWEB && DEBUG
			commandImage.Attributes["onClick"] = string.Format("javascript:parent.OpenPopupDocument('{0}', true)",
							HttpUtility.UrlEncode(string.Format("{0}.{1}", NameSpaceSegment.Document, commandNode.ItemObject))
							);
#else
			else if (commandNode.IsRunFunction)
			{
				commandImage.ToolTip = LabelStrings.ShowOpenDocumentsTooltip;
				commandImage.Attributes["onClick"] = string.Format("javascript:doLink('OpenDocumentsPage.aspx?Title={0}&NameSpace={1}', '" + link + "');",
															title,
															HttpUtility.UrlEncode(commandNode.ItemObject)
															);
			}
			else
			{
				commandImage.ToolTip = LabelStrings.ShowOpenDocumentsTooltip;
				commandImage.Attributes["onClick"] = string.Format("javascript:doLink('OpenDocumentsPage.aspx?Title={0}&NameSpace={1}', '" + link + "');",
															title,
															HttpUtility.UrlEncode(commandNode.ItemObject)
															);
			}
#endif
		}

		

		//---------------------------------------------------------------------
		private void CreateNoReportLayout()
		{
			TableRow reportInformationTableRow = new TableRow();

			commandHyperLink			= new HyperLink();
			commandHyperLink.Text	= LabelStrings.NoMenuItems;
				
			TableCell labelTableCell = new TableCell();
			labelTableCell.Controls.Add(commandHyperLink);
			reportInformationTableRow.Cells.Add(labelTableCell);
			labelTableCell.HorizontalAlign = HorizontalAlign.Center;

			reportTable.Rows.Add(reportInformationTableRow);

			if(easyLookCustomSettings.FontFamily == string.Empty || easyLookCustomSettings.FontFamily == "")
			{
				commandHyperLink.CssClass	= "Command_Element";
				commandHyperLink.Font.Bold	= true;
			}
			else
			{
				commandHyperLink.Font.Name	= easyLookCustomSettings.FontFamily;
				commandHyperLink.Font.Bold	= true;
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
		#endregion

		#region funzione x localizzare le stringhe
		/// <summary>
		/// Funzione che setta le stringhe presenti nella pagina secondo la culture 
		/// dell'Utente corrente
		/// </summary>
		private void SetLocalizedLabel()
		{
			TitleLabel.Text = LabelStrings.ItemsList + ": " + menuTitle;
			DescriptionCheckBox.Text = LabelStrings.DescriptionCheck;
			DataCheckBox.Text = LabelStrings.DataCheck;

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
			easyLookCustomSettings = (EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey];
			

			if (easyLookCustomSettings == null)
				return;

			DefaultSettings.currentUserCommandForeColor = Color.FromArgb(easyLookCustomSettings.CurrentUserReportTitleColor);
			DefaultSettings.allUsersCommandForeColor	= Color.FromArgb(easyLookCustomSettings.AllUsersReportTitleColor);

			if (easyLookCustomSettings.FontFamily != string.Empty || easyLookCustomSettings.FontFamily != "")	
			{
				this.TitleLabel.Font.Name = easyLookCustomSettings.FontFamily;
				DataCheckBox.Font.Name = easyLookCustomSettings.FontFamily;
				DescriptionCheckBox.Font.Name = easyLookCustomSettings.FontFamily;
			}

			int commandBackGround	 = ((EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey]).CommandListBkgndColor;
			
			if (commandBackGround == -1)
				return;

			HtmlForm form = (HtmlForm)this.FindControl("reportForm");
			Color appPanelBkgndColor	= Color.FromArgb(commandBackGround);
			
			string bkgndColorString		= HtmlUtility.ToHtml(appPanelBkgndColor);
			form.Style.Add("BACKGROUND-COLOR", bkgndColorString);
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
