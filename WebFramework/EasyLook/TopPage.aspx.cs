using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;
using System.Web.Security;


namespace Microarea.Web.EasyLook
{
	#region Enumerativi
	public enum SelectionReportType
	{
		CurrentUser	= 0,
		AllUSers	= 1,
		All			= 2
	}

	public enum ReloadType
	{
		Delete		= 0,
		History		= 1,
		ChangeUser	=2
	}
	#endregion

	public partial class TopPage : System.Web.UI.Page
	{
		#region data member protetti
		private string loginImage			= string.Empty;
		private string changeDataImage		= string.Empty;
		private string runReportImage		= string.Empty;
		private string findReportsImage		= string.Empty;
		#endregion

		#region Load della Pagina
		//---------------------------------------------------------------------
		/// <summary>
		/// Evento di Load della pagina; setto la Cache a NoCache, imposto la culture
		/// dell'Utente corrente localizzando, di conseguenza, le stringhe presenti
		/// nella pagina, ed applico le eventuali personalizzazioni impostate tramite 
		/// il PlugIn della Console
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			Microarea.TaskBuilderNet.UI.WebControls.Helper.RegisterLinkDocumentFunction(Page);

			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				this.RedirectToLogin();
				return;
			}

			string msg = ui.RegistrationMessage;
			if (!IsPostBack && !String.IsNullOrEmpty(msg) && !ClientScript.IsClientScriptBlockRegistered("DemoAlert"))
			{
				string script = string.Format(@"demoAlert('{0}');", msg);
				ClientScript.RegisterStartupScript(GetType(), "DemoAlert", script, true);
			}
			
			ui.SetCulture();
			
			if (!Page.IsPostBack)
			{
				LogoffButton.ImageUrl = Helper.GetImageUrl("LoginButton.gif");
				changeDataButton.ImageUrl = Helper.GetImageUrl("ChangeApplicationDate.gif");
				runReportButton.ImageUrl = Helper.GetImageUrl("RunNewReport.gif");
				SearchCommandButton.ImageUrl = Helper.GetImageUrl("Search.gif");
           
                LinkSitePrivateAreaButton.ImageUrl = Helper.GetImageUrl("GoToProducerSitePrivateArea.gif");
				//Setto i tooltip dei pulsanti
				SetToolTip();
				//Carico la combo
				LoadComboItem();
					
                Session.Add(SessionKey.ReportType, SelectionReportType.CurrentUser);

                BrandLoader brandLoader = ui.Brand;
				IBrandInfo brandInfo = brandLoader.GetMainBrandInfo();
/* TODO RSWEB
				if (brandInfo.IsMagoNet())
					logoImage.ImageUrl = Helper.GetImageUrl("logoMagoNetEasylook.png");
*/
				if (brandLoader.GetCompanyName() == NameSolverStrings.Microarea)
				{
					System.Web.UI.WebControls.Image logoCompanyImage = new System.Web.UI.WebControls.Image();
					logoCompanyImage.ImageUrl = Helper.GetImageUrl("LogoMicroarea.png");
					logoCompanyImage.ImageAlign = ImageAlign.Top;
					cellCompanyLogo.Controls.Add(logoCompanyImage);
				}
                //Applico i setting inseriti dal PlugIn
				ApplyCustomSetting();
			
				Page.Title = CommonFunctions.GetBrandedTitle();
			}
		}
		
		#endregion

	

		#region SelectedIndexChanged Combo
		/// <summary>
		/// Evento di SelectedIndexChanged sulla combo contenete le tipologie 
		/// di filtraggio dello storico dei report
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		public void reportTypeCombo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Session.Add(SessionKey.ReportType, this.reportTypeCombo.SelectedItem.Value);
			Session.Add(SessionKey.ReloadHistory, "true"); 
			Session[SessionKey.ReportsToDeleting] = null;
		}

		#endregion

		#region pulsante di run report
		/// <summary>
		/// Funzione che ritorna il NameSpace del Report
		/// </summary>
		/// <returns></returns>
		public  string GetReportNameSpace()
		{
			return Session[Microarea.TaskBuilderNet.Woorm.WoormWebControl.SessionKey.ReportPath].ToString();
		}

		//---------------------------------------------------------------------
		#endregion

		#region pulsante di login

		/// <summary>
		/// Evento di Click sul pulsante Login; 
		/// Inserisce lo script per ridirezionare la pagina sulla login
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void LogoffButton_Click (object sender, System.Web.UI.ImageClickEventArgs e)
		{
			// inizializza la ReportSession di applicazione TaskBuilder
			UserInfo ui = UserInfo.FromSession();
			if (ui != null)
				ui.LogOff();

            FormsAuthentication.SignOut();
            Session.Abandon();
			this.RedirectToLogin();
		}

		//---------------------------------------------------------------------
		#endregion

		#region funzione che setta i tool Tip dei pulsanti localizzati
		/// <summary>
		/// Funzione che localizza secondo la culture dell'Utente connesso i ToolTip
		/// dei pulsanti presenti nella ToolBar
		/// </summary>
		private void SetToolTip()
		{
			this.LogoffButton.ToolTip =  LabelStrings.LoginButtonTooolTip ;
			this.runReportButton.ToolTip =  LabelStrings.RunReportButtonToolTip ;
			this.changeDataButton.ToolTip =  LabelStrings.ChangeDateButtonToolTip ;
            this.LinkSitePrivateAreaButton.ToolTip = LabelStrings.LinkSitePrivateAreaButtonToolTip;
			this.SearchCommandButton.ToolTip = LabelStrings.SearchButtonToolTip ;
			reportTypeLabel.Text = LabelStrings.ComboLabel;
		}
		#endregion

		#region funzione che carica la combo localizzata
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che inserisce i tipi di filtraggio nella combo dopo averli 
		/// localizzati
		/// </summary>
		private void LoadComboItem()
		{
			ListItem itemCurrentUser = new ListItem(LabelStrings.CurrentUser, "CurrentUser");
			itemCurrentUser.Selected = true;
			this.reportTypeCombo.Items.Add(itemCurrentUser);
			ListItem itemAll = new ListItem(LabelStrings.All, "All");
			this.reportTypeCombo.Items.Add(itemAll);
									
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzione che applica i settaggi immessi dal PlugIn
		/// <summary>
		/// Funzione che applica gli eventuali settaggi custom impostati dall'Utente
		/// tramite il PlugIn della Console
		/// </summary>
		private void ApplyCustomSetting()
		{
			//Mi recupero gli eventuali settaggi custom
			string logoUrl = ""; 
            string physicalPath = "";

			EasyLookCustomSettings setting = ((EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey]);

			if (setting == null)
				return;

			if (setting.LogoImageURL.Length != 0)
			{
                physicalPath = Helper.GetImagePhysicalPath(string.Format(@"Companies\{0}\", setting.GetCompanyName(setting.CompanyId)));
                logoUrl = Helper.GetImageUrl(string.Format(@"Companies/{0}/", setting.GetCompanyName(setting.CompanyId)));


                if (setting.InheritedFromAllUsers)
                {
                    physicalPath = physicalPath + @"AllUsers\";
                    logoUrl = logoUrl + @"AllUsers/";
                }
                else
                {
                    physicalPath = physicalPath + setting.GetUserName(setting.LoginId) + @"\";
                    logoUrl = logoUrl + setting.GetUserName(setting.LoginId) + @"/";
                }

				logoUrl = logoUrl + setting.LogoImageURL;
                physicalPath = physicalPath + setting.LogoImageURL;

				try
				{
					logoImage.ImageUrl = logoUrl;
                    
                    IBaseModuleInfo mod = BasePathFinder.BasePathFinderInstance.GetModuleInfoByName("WebFramework", "EasyLook");
                    if (mod == null)
                        return;
					
					string destinationPath = mod.Path;
                    destinationPath = Path.Combine(destinationPath, physicalPath);
                    System.Drawing.Image image = System.Drawing.Image.FromFile(destinationPath);

					if (image.Size.Width > 430)
						this.logoImage.Width = 430;

					if (image.Size.Height > 100)
						this.logoImage.Height = 100;
				}
				catch(Exception ex)
				{
					string a = ex.Message; 
				}
			}

			//Font della label e della combo
			this.reportTypeLabel.Font.Name = ((EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey]).FontFamily;
			this.reportTypeCombo.Font.Name = ((EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey]).FontFamily;
		}
		//---------------------------------------------------------------------
		#endregion
		override protected void OnInit(EventArgs e)
		{

			base.OnInit(e);
		}
	
	}
}
