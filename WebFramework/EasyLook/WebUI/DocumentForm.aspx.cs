using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using Microarea.TaskBuilderNet.Core.Applications;

namespace Microarea.Web.EasyLook.WebUI
{
	public partial class DocumentForm : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (this.IsPostBack)
				return;
			string sessionGuid = Guid.NewGuid().ToString();

            string outSt = string.Concat("window.sessionData={ \"sessionGuid\" : \"", sessionGuid, "\", \"objectNamespace\": \"",  Request.Params["ObjectNamespace"], "\"}");

            ClientScript.RegisterStartupScript(GetType(), "initialSessionData", outSt, true);
		}
	}
}
