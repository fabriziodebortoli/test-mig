using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Interfaces;
using System.Collections;
using Microarea.TaskBuilderNet.UI.WebControls;

// using Microarea.TaskBuilderNet.UI.WebControls;

namespace Microarea.Web.EasyLook.myDesk
{
	public partial class tbAppPage : System.Web.UI.Page
	{
		#region data member
			protected string firstGroupPath = "";
		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			
		}

		protected override void Render(HtmlTextWriter writer)
		{
			var sw = new System.IO.StringWriter();
			var tw = new HtmlTextWriter(sw);
			base.Render(tw);

			// Response.Write(LoadMenu());
			
			string rJson = "{\"menudata\": [" + LoadMenu() + "]}";
			Response.Clear();
			Response.ContentType = "application/json; charset=utf-8";
			Response.Write(rJson);
			Response.Flush();
			Response.End(); 
		}

		/// <summary>
		/// Funzione che setta MenuXmlParser al MenuApplicationsPanelBar
		/// </summary>
		private string LoadMenu()
		{
			string rJson = "";
			string rJsonGroup = "";

			GroupLinkLabel moduleGroup = null;
			ArrayList ar = null;
			MenuApplicationPanel collapsiblePanel = null;

			MenuXmlParser parserDomMenu = Helper.GetMenuXmlParser();
			if (parserDomMenu == null)
				return "Error";

			foreach (IMenuXmlNode applicationNode in parserDomMenu.Root.ApplicationsItems)
			{
				string applicationName = applicationNode.GetNameAttribute();
				collapsiblePanel = new MenuApplicationPanel(applicationName);
				string applicationTitle = applicationNode.Title;
				string appImageFile = parserDomMenu.GetApplicationImageFileName(applicationName);
				collapsiblePanel.ApplicationElement.ApplicationImageUrl = ImagesHelper.CopyImageFileIfNeeded(appImageFile);
				ar = applicationNode.GroupItems;
				if (ar == null)
					continue;

				if (rJson != "")
					rJson += ",";

				rJson += "{";
				rJson += "\"applicationTitle\" : " + "\"" + applicationTitle + "\",";
				rJson += "\"Image\" : " + "\"" + collapsiblePanel.ApplicationElement.ApplicationImageUrl + "\",";
				rJsonGroup = "";
				foreach (IMenuXmlNode groupNode in ar)
				{
					moduleGroup = collapsiblePanel.AddGroup(groupNode.Title, applicationNode.GetApplicationName());

					if (moduleGroup == null)
						continue;

					moduleGroup.NameSpace = groupNode.GetGroupName();
					string s = applicationName + @"\\" + moduleGroup.NameSpace;

					if (rJsonGroup != "")
						rJsonGroup += ",";

					rJsonGroup += "{";
					rJsonGroup += "\"appTitleMenuLabel\" : " + "\""+ s.EncodeBase16() + "\",";
					string[] token = moduleGroup.NameSpace.Split('.');
					if (token.Length >= 2 && string.Compare(token[1], "UserReportsGroup") == 0)
					{
						//moduleGroup.ImagePath = ImagesHelper.CreateImageAndGetUrl("DefaultUserReportsGroup.gif", typeof(Helper));
						moduleGroup.ImagePath = "./myDesk/images/DefaultUserReportsGroup.gif";
					}
					else
					{
						string imagePath = parserDomMenu.GetGroupImageFileName(applicationNode.GetApplicationName(), groupNode.GetGroupName());
						moduleGroup.ImagePath = ImagesHelper.CopyImageFileIfNeeded(imagePath);
						if (string.IsNullOrEmpty(moduleGroup.ImagePath))
							moduleGroup.ImagePath = "./myDesk/images/DefaultGroupImgSmall.gif";
							//moduleGroup.ImagePath = ImagesHelper.CreateImageAndGetUrl("DefaultGroupImgSmall.gif", typeof(Helper));

					}

					rJsonGroup += "\"Image\" : " + "\"" + moduleGroup.ImagePath + "\",";
					moduleGroup.ParentApplicationName = applicationNode.GetApplicationName();
					moduleGroup.LabelLinkFontInfo.Size = FontUnit.Point(9);
					rJsonGroup += "\"Url\" : " + "\""+ moduleGroup.LabelLinkText + "\"";
					rJsonGroup += "}";
				}
				rJson += "\"Items\" : " + "[" + rJsonGroup + "]";
				rJson += "}";

			}

			return rJson;
		}

	}
}