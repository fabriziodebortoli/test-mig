using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microarea.Web.EasyLook.WebUI
{
	public partial class ReportForm : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (this.IsPostBack)
				return;
			
			ClientScript.RegisterStartupScript(GetType(), "initialSessionData", string.Concat("window.sessionData={ namespace : '", Request.Params["namespace"], "' }"), true);
		}
	}
}