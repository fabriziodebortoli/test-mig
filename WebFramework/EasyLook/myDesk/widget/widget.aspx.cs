using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace Microarea.Web.EasyLook.myDesk.widget
{
	public partial class widget : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			string path = HttpContext.Current.Request.PhysicalApplicationPath;
			StreamReader streamReader = new StreamReader(path + "\\myDesk\\widget\\widget.json");
			string json = streamReader.ReadToEnd();
			streamReader.Close();

			Response.Clear();
			Response.ContentType = "application/json; charset=utf-8";
			Response.Write(json);
			Response.End(); 

		}
	}
}