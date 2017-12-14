using System;
using System.Drawing;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;

namespace Microarea.Web.EasyLook
{
	public partial class MenuArea : System.Web.UI.Page
	{
		
		#region datamember protetti

        protected string menuNode = "";
		/// <summary>
		/// Path del nodo dal quale si inizierà a popolare il tree
		/// </summary>
		protected string	menuNodePath	= "";
		/// <summary>
		/// Nome del Font da applicarte alla pagina
		/// </summary>
		protected string	    fontFamily	= "";
        protected MenuXmlParser parser      = null;

		#endregion
	
		#region load della pagina
		/// <summary>
		/// Evento di Load della Pagina setta la CaChe a NoCache Applica le eventuali
		/// personalizzazioni effettuate tramite il PlugIn della Console, setta 
		/// il MenuXmlParser del Tree
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
			{
				this.RedirectToLogin();
				return;
			}

			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			
			ApplyCustomSettings();

			menuNodePath = HttpContext.Current.Request.QueryString["NameSpace"];

			if (menuNodePath != null )
			{
                menuNodePath = menuNodePath.DecodeBase16();
			}
			Session[Microarea.TaskBuilderNet.Woorm.WoormWebControl.SessionKey.ReportPath] = string.Empty;

			if (string.IsNullOrEmpty(menuNodePath))
				return; 

			if (!IsPostBack) 
			{
                menuNode = Helper.GetImageUrl("Menu.GIF");
				parser = Helper.GetMenuXmlParser();
                MenuTreeView.Font.Name = fontFamily;
                MenuXmlNode groupNode = null;
                string nodePath = string.Empty;
                //Gli setto la Root
				string imgRootPath = Helper.GetImageUrl("Group.GIF");
				TreeNode root = new TreeNode(GetRootText(ref groupNode, ref nodePath), null, imgRootPath, null, null);
                MenuTreeView.Nodes.Add(root);
                if (groupNode != null)
					Recurse(groupNode, MenuTreeView.Nodes[0], nodePath);
			}
		}

        private void Recurse(MenuXmlNode aNode, TreeNode treeNode, string nodePath)
        {

            string nodeValue = nodePath.EncodeBase16();

            if (aNode.MenuItems != null)
            {
                foreach (MenuXmlNode node in aNode.MenuItems)
                {
                    string titleHierarchy = node.GetMenuHierarchyTitlesString().EncodeBase16();
                    string titleValue = node.Title.EncodeBase16();

                    string aHref = "MenuItemsPage.aspx?NameSpace=" + nodeValue + MenuXmlNode.ActionMenuPathSeparator + titleHierarchy + "&Title=" + titleValue;
                    string link = "javascript:doLink('" + aHref + "');";
                    TreeNode newNode = new TreeNode(node.Title, node.ItemObject, menuNode,  link,  null);
					treeNode.ChildNodes.Add(newNode);

                    if (node.HasMenuChildNodes())
                      Recurse(node, newNode, nodePath);
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
		//---------------------------------------------------------------------
		#endregion

		#region Web Form Designer generated code
		/// <summary>
		/// Questa chiamata è richiesta da Progettazione Web Form ASP.NET.
		/// </summary>
		/// <param name="e"></param>
		
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Metodo necessario per il supporto della finestra di progettazione. Non modificare
		/// il contenuto del metodo con l'editor di codice.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		#region applico i settaggi custom
		/// <summary>
		/// Funzione che applica gli eventuali settaggi custom impostati dall'Utente
		/// tramite il PlugIn della Console
		/// </summary>
		private void ApplyCustomSettings()
		{
			if (Session[EasyLookCustomSettings.SessionKey] == null)
				return;

			EasyLookCustomSettings setting = (EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey];
			fontFamily			 = setting.FontFamily;
			int menuBackGround	 = setting.MenuTreeBkgndColor;
			
			if (menuBackGround == -1)
				return;

			HtmlForm	form				= (HtmlForm)this.FindControl("menuForm");
			Color		appPanelBkgndColor	= Color.FromArgb(menuBackGround);
			string		bkgndColorString	= HtmlUtility.ToHtml(appPanelBkgndColor);
			form.Style.Add("BACKGROUND-COLOR", bkgndColorString);

		}

		//---------------------------------------------------------------------
		#endregion

	}
}
