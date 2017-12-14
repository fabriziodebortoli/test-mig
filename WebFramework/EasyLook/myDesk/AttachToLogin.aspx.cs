using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;
using System.Web.Security;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;

namespace Microarea.Web.EasyLook.myDesk
{
	public partial class AttachToLogin : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			string token = Request.Params["authenticationtoken"];
			if (string.IsNullOrEmpty(token))
				return;

			UserInfo ui = new UserInfo();
			
			//se ho gia` visualizzato la richiesta di sovrascrivere la login, la password e' salvata in sessione
			
			string message = string.Empty;
			//effettuo la login, la prima volta senza sovrascrivere,e controllo integrita del database di sistema
			if (ui.Login(token))
			{
				UserInfo.ToSession(ui);
				EasyLookCustomSettings easyLookCustomSettings = new EasyLookCustomSettings(ui.PathFinder, ui.CompanyId, ui.LoginId);
				Session.Add(EasyLookCustomSettings.SessionKey, easyLookCustomSettings);
				Response.SetCookie(new HttpCookie("authtoken", ui.LoginManager.AuthenticationToken));
				// use the web menu in GDI version
				HttpContext.Current.Session["WebGdi"] = true;
				MenuXmlParser parserDomMenu = Helper.LoadMenuXmlParser();

				FormsAuthentication.RedirectFromLoginPage(ui.LoginManager.UserName, false);
				//Response.Redirect("myDesk/widget/widget.aspx");
			}
		}
	}
}