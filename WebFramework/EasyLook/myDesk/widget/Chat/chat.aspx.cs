using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace Microarea.Web.EasyLook.myDesk.widget.Chat
{
	public partial class chat : System.Web.UI.Page
	{
		

		protected void Page_Load(object sender, EventArgs e)
		{	
			string path = HttpContext.Current.Request.PhysicalApplicationPath + "\\myDesk\\widget\\chat.txt";
			
			// This text is added only once to the file. 
			var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

			StreamReader sr = new StreamReader(fs);
			string line;
			int nLine = 0;
			string rLine = "";
			string jLine = "";

			
			while ((line = sr.ReadLine()) != null)
			{
				nLine++;
				if (nLine > Convert.ToInt32(Request.Form["nChat"]) && Convert.ToInt32(Request.Form["nChat"]) != -1)
				{
					string[] spLine = line.Split(',');

					rLine += "[" + spLine[0] + "] " + spLine[1] + "<br>";
				}
			}
			

			if (Request.Form["data"] != "")
			{
				string newLine = DateTime.Now.ToString("yyyyMMdd-HH:mm:ss") + "," + Request.Form["data"];
				string[] spLine = newLine.Split(',');
				rLine += "[" + spLine[0] + "] " + spLine[1] + "<br>";
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.WriteLine(newLine);
				}
			}

			jLine = "{";
			jLine += "\"nLine\" : " + nLine.ToString() + ",";
			jLine += "\"message\" : \"" + rLine + "\"";
			jLine += '}';

			fs.Close();
			Response.Clear();
			Response.ContentType = "text/plain; charset=utf-8";
			Response.Write(jLine);
			Response.End(); 
			
		}
	}
}