using System;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.WebServices.LoginManager
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

			//string serialNumber = LoginManager.LoginApplication.LoginEngine.GetMainSerialNumber();
			string serialNumber = Guid.NewGuid().ToString();

			//tappullaccio per help center 4.0 // continuiamo coi tapulli dopo ok di Germano (e in presenza di Luca B. in data 26/05/2016)
			Namespace.Value = nameSpace
				.ReplaceNoCase(".ERP.", ".M4.") 
				.ReplaceNoCase(".Framework.", ".TBF.")
				.ReplaceNoCase(".Extensions.", ".TBF.")
				.ReplaceNoCase(".MDC.", ".M4.");
			

			SerialNumber.Value = serialNumber;
			Language.Value = language;

			string helpCenterUrl = InstallationData.BrandLoader.GetBrandedStringBySourceString("HelpCenterUrl");
			if (string.IsNullOrEmpty(helpCenterUrl) || helpCenterUrl.CompareNoCase("HelpCenterUrl"))
				helpCenterUrl = "http://www.microarea.it/MicroareaHelpCenter/Login.aspx";

			HelpForm.Action = helpCenterUrl;
		}
	}
}
