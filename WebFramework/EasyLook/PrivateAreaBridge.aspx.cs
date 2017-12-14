using System;
using System.Web;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.Web.EasyLook
{
	/// <summary>
    /// Summary description for PrivateAreaBridge.
    /// Page which connect current user (if granted in AdministrationConsole) with private area on producer website
	/// </summary>
	public partial class PrivateAreaBridge : System.Web.UI.Page
	{
        //---------------------------------------------------------------------
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				this.RedirectToLogin();
				return;
			}

            string url = HelpManager.GetProducerSitePrivateAreaLinkWithToken(ui.LoginManager.AuthenticationToken);
            Response.Redirect(url);
		}

        //---------------------------------------------------------------------
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
        //--------------------------------------------------------------------- 
		private void InitializeComponent()
		{    
		}
	}
}
