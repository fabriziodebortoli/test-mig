using System;
using System.Drawing;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;
using Microarea.TaskBuilderNet.UI.WebControls;

namespace Microarea.Web.EasyLook
{
	/// <summary>
	/// Summary description for ApplicationPage.
	/// </summary>
	public partial class ApplicationPage : System.Web.UI.Page
	{

		#region data member
		protected string	firstGroupPath = "";
		#endregion

		#region load della pagina
		/// <summary>
		/// Funzione che fa il Load della pagina istanzia il Parser e carica 
		/// Applicazioni e moduli all'interno del MenuApplicationsPanelBar.
		/// Posiziona il Tree dei menù sul primo gruppo della prima applicazione 
		/// ed applica le aventuali personalizzazioni 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			if (!IsPostBack)
			{
				//mi setto la dimensione x lasciare il bordino blu
				menuApplicationsPanelBar.Width = Unit.Percentage(90);
				//Carico il parser
				LoadMenu();
				//Faccio apparire già il tree del menù aperto sul primo gruppo
				DisplayFirstGroup();
				//Metto i settaggi inseriti da PlugIn
				ApplyCustomSettings();
			}
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzione x aprire direttamente il tree dei menù sul primo gruppo
		/// <summary>
		/// Funzione che inserisce lo script per popolare automaticamente il Tree contenente i
		/// menù con quelli del primo modulo della prima applicazione
		/// </summary>
		private void DisplayFirstGroup()
		{
			string scriptValue	=
								@"<script>																
										function DisplayFirstGroup()
										{
											if (parent.topFrame.EasylookMenuArea == undefined)
												parent.MenuArea.location.href='MenuArea.aspx?NameSpace=" + firstGroupPath + @"';
										}																
								  </script>";

            ClientScript.RegisterClientScriptBlock(ClientScript.GetType(), "First", scriptValue);
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzioni x tirare su il parser e popolare il pannello delle applicazioni
		/// <summary>
		/// Funzione che setta MenuXmlParser al MenuApplicationsPanelBar
		/// </summary>
		private void LoadMenu()
		{
			MenuXmlParser parserDomMenu = Helper.GetMenuXmlParser();
			if (parserDomMenu == null)
				return;

			menuApplicationsPanelBar.SetMenuXmlParser(parserDomMenu);
			//Mi creo il Path del primo gruppo x poi aprire il tree dei menù
			//direttamente su di lui
			//Devo controllare che esista almeno un applicazione con un modulo da mostrare
			//perchè se no non so cosa far visualizzare al pannello di centro
			if (parserDomMenu == null || parserDomMenu.Root == null || parserDomMenu.Root.ApplicationsItems == null)
				return;

			MenuXmlNode applicationNode = ((MenuXmlNode)parserDomMenu.Root.ApplicationsItems[0]);
			firstGroupPath = "";
			if (applicationNode.GroupItems != null)
			{
				firstGroupPath = applicationNode.GetApplicationName() + MenuXmlNode.ActionMenuPathSeparator + ((MenuXmlNode)applicationNode.GroupItems[0]).GetNameAttribute() ;
                firstGroupPath = firstGroupPath.EncodeBase16();
			}
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzione che setta i settaggi custom del PlugIn
		/// <summary>
		/// Funzione che applica le eventuali personalizzazioni settate tramite il PlugIn della Console
		/// </summary>
		private void ApplyCustomSettings()
		{

			if (Session[EasyLookCustomSettings.SessionKey] == null)
				return;

			EasyLookCustomSettings	setting	= (EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey];

			if (setting == null)
				return;

			HtmlForm				form	= (HtmlForm)this.FindControl("applicationForm");

			//Colore di sfondo del pannello delle applicazioni
			Color appPanelBkgndColor	= Color.FromArgb(setting.AppPanelBkgndColor);
			string bkgndColorString		=  HtmlUtility.ToHtml(appPanelBkgndColor);

			menuApplicationsPanelBar.BackColor = appPanelBkgndColor;

			if (bkgndColorString.Length > 0)
				form.Style.Add("BACKGROUND-COLOR", bkgndColorString);


			foreach(MenuApplicationPanel panel in menuApplicationsPanelBar.CollapsiblePanelArray)
			{

				//Font Pannello Applicazioni e Gruppi
				if ( setting.FontFamily != string.Empty || setting.FontFamily != "")
				{
					panel.ApplicationElement.ApplicationTitleFontName	= setting.FontFamily;;
					panel.GroupsFontName								= setting.FontFamily;;
				}
				else
				{
					panel.ApplicationElement.ApplicationTitleClass	= "Application_Element";
					panel.GroupsCssClass							= "Application_Element";
				}
			}

			//TODO
			//Colore di sfondo dei gruppi
			Color groupPanelBkgndColor	 = Color.FromArgb(setting.GroupsPanelBkgndColor);
			string bkgndGroupColorString =  HtmlUtility.ToHtml(groupPanelBkgndColor);

			string bkgGroupImage = setting.GroupsPanelBkgndImageURL;
			
			foreach (MenuApplicationPanel panel in menuApplicationsPanelBar.CollapsiblePanelArray)
			{
				panel.ApplicationElement.ApplicationElementContainer.BackColor	= appPanelBkgndColor;
				
				//Setto il colore di sfondo dei gruppi diverso
				if (string.Compare(bkgndGroupColorString , "#E6E6FA") !=0)
					panel.GroupsBackGroundColor = groupPanelBkgndColor;
			
				if (bkgGroupImage != null && bkgGroupImage != "")
					panel.GroupsBackImageUrl = SetBkgImage(setting);

                if (string.Compare(bkgndGroupColorString, "#E6E6FA") == 0 && (bkgGroupImage != null || bkgGroupImage != ""))
                    panel.GroupsBackGroundColor = Color.Lavender;
			}
			
		}

		//---------------------------------------------------------------------
		private string SetBkgImage(EasyLookCustomSettings setting)
		{
			if (setting == null)
				return string.Empty;

			if (setting.GroupsPanelBkgndImageURL.Length != 0)
			{
				string imageUrl = Helper.GetImageUrl(string.Format("Companies\\{0}\\",  setting.GetCompanyName(setting.CompanyId)));

				if (setting.InheritedFromAllUsers)
					imageUrl = imageUrl + "AllUsers\\";
				else
					imageUrl = imageUrl + setting.GetUserName(setting.LoginId)+ "\\";

				return imageUrl + setting.GroupsPanelBkgndImageURL;
			}

			return string.Empty;
		}

		#endregion

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion
	}
}
