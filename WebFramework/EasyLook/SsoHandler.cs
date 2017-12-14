using System;
using System.Web;
using System.Web.SessionState;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;
using System.Web.Security;

namespace Microarea.Web.EasyLook
{
	//=========================================================================
	public class SsoHandler : IHttpHandler, IReadOnlySessionState
	{
		/// <summary>
		/// You will need to configure this handler in the web.config file of your 
		/// web and register it with IIS before being able to use it. For more information
		/// see the following link: http://go.microsoft.com/?linkid=8101007
		/// </summary>
		#region IHttpHandler Members

		//---------------------------------------------------------------------
		public bool IsReusable
		{
			// Return false in case your Managed Handler cannot be reused for another request.
			// Usually this would be false in case you have some state information preserved per request.
			get { return true; }
		}

		//---------------------------------------------------------------------
		public void ProcessRequest(HttpContext context)
		{
			if (
				context == null ||
				context.Request == null ||
				context.Request.QueryString == null
				)
				return;

			string ssoToken = context.Request.QueryString["ssoToken"];

			if (String.IsNullOrWhiteSpace(ssoToken))
				return;

			UserInfo ui = new UserInfo();
            string userName = ui.SsoLoggedUser(ssoToken);
			if (String.IsNullOrWhiteSpace(userName))
				return;

			UserInfo.ToSession(ui);
			EasyLookCustomSettings easyLookCustomSettings = new EasyLookCustomSettings(ui.PathFinder, ui.CompanyId, ui.LoginId);
			context.Session.Add(EasyLookCustomSettings.SessionKey, easyLookCustomSettings);

			FormsAuthentication.RedirectFromLoginPage(userName, false); // serve per mettere il cookie di sessione nel browser!!

			context.Application.AddUserInfoToApplication(ui);
		}

		#endregion
	}
}
