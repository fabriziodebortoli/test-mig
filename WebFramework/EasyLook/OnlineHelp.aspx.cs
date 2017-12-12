using System;
using System.Collections.Generic;

namespace Microarea.WebServices.EasyLook
{
	public partial class OnlineHelp : System.Web.UI.Page
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

			//parametri della query string usato per far capire a LoginManager
			//che e' arrivata una richiesta per la pagina di help passando da Mago
			string authToken = Request.QueryString["q"];
			string nameSpace = Request.QueryString["n"];
			string language = Request.QueryString["lang"];

			if (string.IsNullOrEmpty(authToken))
				return;

			Guid tempGuid = new Guid(authToken);

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

			string serialNumber = Guid.NewGuid().ToString();

			Namespace.Value = nameSpace;
			SerialNumber.Value = serialNumber;
			Language.Value = language;
		}
	}
}
