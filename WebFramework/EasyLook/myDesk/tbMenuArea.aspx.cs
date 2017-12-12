using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;

namespace Microarea.Web.EasyLook.myDesk
{
	public partial class tbMenuArea : System.Web.UI.Page
	{
		protected string menuNodePath = "";
		protected string menuNode = "";
		protected string rJson = "";
		protected MenuXmlParser parser = null;

		protected void Page_Load(object sender, EventArgs e)
		{

			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				ui = new UserInfo();
				if (ui.Login(Request.Cookies["authtoken"].Value))
				{
					UserInfo.ToSession(ui);
					EasyLookCustomSettings easyLookCustomSettings = new EasyLookCustomSettings(ui.PathFinder, ui.CompanyId, ui.LoginId);
					Session.Add(EasyLookCustomSettings.SessionKey, easyLookCustomSettings);
				}

			}

			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			menuNodePath = HttpContext.Current.Request.QueryString["NameSpace"];
			
			if (menuNodePath != null)
			{
				menuNodePath = menuNodePath.DecodeBase16();
			}

			if (string.IsNullOrEmpty(menuNodePath))
				return;

			if (!IsPostBack)
			{
				MenuXmlNode groupNode = null;
				string nodePath = string.Empty;
				parser = Helper.GetMenuXmlParser();

				//Gli setto la Root
				string rootName = GetRootText(ref groupNode, ref nodePath);

				if (groupNode != null)
					Recurse(groupNode, nodePath);
			}
		}

		//---------------------------------------------------------------------
		protected override void Render(HtmlTextWriter writer)
		{
			var sw = new System.IO.StringWriter();
			var tw = new HtmlTextWriter(sw);
			base.Render(tw);
			
			rJson = "{\"menudata\": [" + rJson + "]}";
			Response.Clear();
			Response.ContentType = "application/json; charset=utf-8";
			Response.Write(rJson);
			Response.Flush();
			Response.End();
		}

		//---------------------------------------------------------------------
		private void Recurse(MenuXmlNode aNode, string nodePath)
		{

			string nodeValue = nodePath.EncodeBase16();
			int j = 0;
			if (aNode.MenuItems != null)
			{
				foreach (MenuXmlNode node in aNode.MenuItems)
				{
					j ++;
					string titleHierarchy = node.GetMenuHierarchyTitlesString().EncodeBase16();
					string titleValue = node.Title.EncodeBase16();
					string NameSpace = nodeValue + MenuXmlNode.ActionMenuPathSeparator + titleHierarchy;

					rJson += "{";
					rJson += "\"NameSpace\" : " + "\"" + NameSpace + "\",";
					rJson += "\"TitleValue\" : " + "\"" + titleValue + "\",";
					rJson += "\"Title\" : " + "\"" + node.Title + "\",";

					if (node.HasMenuChildNodes())
					{
						rJson += "\"Child\" : " + "true,";
						rJson += "\"ChildNodes\" : " + "[";
						Recurse(node, nodePath);
						rJson += "]";
					}
					else
					{
						rJson += "\"Child\" : " + "false";
					}

					rJson += "}";
					if (aNode.MenuItems.Count != j)
						rJson += ",";
				}
			}
		}

		//---------------------------------------------------------------------
		private string GetRootText(ref MenuXmlNode node, ref string nodePath)
		{
			menuNodePath = menuNodePath.Replace(@"\\", @"\");

			string menuNodePathDelimStr = MenuXmlNode.ActionMenuPathSeparator;
			char[] menuNodePathDelimiter = menuNodePathDelimStr.ToCharArray();
			string[] menuNodePathSplitArguments = menuNodePath.Split(menuNodePathDelimiter);

			if (menuNodePathSplitArguments.Length < 2)
				return string.Empty;

			string applicationName = menuNodePathSplitArguments[0];
			if (applicationName == null || applicationName == String.Empty)
				return string.Empty;

			string groupName = menuNodePathSplitArguments[1];
			if (groupName == null || groupName == String.Empty)
				return string.Empty;

			MenuXmlNode applicationNode = parser != null ? parser.GetApplicationNodeByName(applicationName) : null;
			if (applicationNode == null)
				return string.Empty;

			nodePath = applicationName + @"\" + groupName;
			node = applicationNode.GetGroupNodeByName(groupName);

			if (node == null)
				return string.Empty;

			return node.Title;
		}

	}
}