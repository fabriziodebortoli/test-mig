using System;
using System.Web;
using System.Web.UI;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;

namespace Microarea.Web.EasyLook
{
	///<summary>
	/// Pagina alternativa per la visualizzazione del Menu in semplice HTML
	/// da dare in pasto all'ambiente IAF
	///</summary>
	//================================================================================
	public partial class InfinityMenu : System.Web.UI.Page
	{
		//--------------------------------------------------------------------------------
		protected void Page_Load(object sender, EventArgs e)
		{
			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			// Imposta il titolo della pagina
			if (this.IsPostBack)
				return;

			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				this.RedirectToLogin();
				return;
			}
			
			Page.Title = CommonFunctions.GetBrandedTitle();

			// istanzio il menuloader e me lo faccio tornare in formato HTML
			InfinityMenuLoader menuLoader = new InfinityMenuLoader();
			string html = menuLoader.GetCompleteHtmlMenu(ui.Company, ui.User, ui.PathFinder, ui.LoginManager);

			Response.Write(html);
			Response.End();
		}
	}
}