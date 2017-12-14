using System;
using System.Web;

using Microarea.TaskBuilderNet.Core.Applications;

namespace Microarea.Web.EasyLook
{
	/// <summary>
	/// Summary description for Footer.
	/// </summary>
	public partial class Footer : System.Web.UI.Page
	{

	
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
				
			ui.SetCulture();

			lblVersionStatic.Text	= LabelStrings.Installation;
			lblVersion.Text			= ui.PathFinder.Installation;
			lblCompanyStatic.Text	= LabelStrings.Company;
			lblCompany.Text			= ui.PathFinder.Company;

			lblUserStatic.Text	= LabelStrings.Users;
			lblUser.Text		= ui.LoginManager.UserName;
		}

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
