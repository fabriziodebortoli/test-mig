using System;
using System.Collections.Generic;

namespace Microarea.WebServices.LoginManager
{
	public partial class SitePrivateArea : System.Web.UI.Page
	{
		private static List<Guid> tokens = new List<Guid>();

		protected void Page_Load(object sender, EventArgs e)
		{
			string token = Request.QueryString["init"];
			if (!string.IsNullOrEmpty(token))
			{
				lock (tokens) 
				{
					tokens.Add(new Guid(token));
				}
				return;
			}

			//parametro della query string usato per far capire a LoginManager
			//che e' arrivata una richiesta per la pagina di amministrazione dell'area risevata del sito
			//passando da Mago ( vedere Generic\HelpManager.cs metodo GetUserProfileUrl() )
			string userProfileToken = Request.QueryString["u"];

			if (string.IsNullOrEmpty(userProfileToken))
				return;

			Guid tempGuid = new Guid(userProfileToken);

			try
			{
				lock (tokens)
				{
					if (!tokens.Contains(tempGuid))
						return;

					tokens.Remove(tempGuid);
				}
			}
			catch { }
			
			ActivationKey.Value = LoginManager.LoginApplication.LoginEngine.ActivationManager.ActivationKey;
		}
	}
}
