using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microarea.Common.Generic;
using Microarea.Common.SecurityLayer.SecurityObjects;

namespace Microarea.Common.MenuLoader
{
	//usata anche nel SecurityAdminPlugIn

	/// <summary>
	/// Summary description for.
	/// </summary>
	public class MenuSecurityFilter : Security
	{
		//---------------------------------------------------------------------
		public MenuSecurityFilter(string sysDBConnectionString, string company, string user, bool securityLicensed)
			: base (sysDBConnectionString, company, user, securityLicensed)
		{
		}

		//---------------------------------------------------------------------
		public MenuSecurityFilter(SqlConnection aSqlConnection, int aCompanyId, int aUserId, bool securityLicensed, bool isRoleId) 
			: base(aSqlConnection, aCompanyId, aUserId, securityLicensed, isRoleId)
		{
		}

		//---------------------------------------------------------------------
		public void Filter (MenuXmlParser aParser)
		{
			if 
				(
				!IsSecurityLicensed ||
				IsAdmin ||
				aParser == null ||
				aParser.Root == null ||
				aParser.Root.ApplicationsItems == null ||
				aParser.Root.ApplicationsItems.Count == 0
				) 
				return;

			foreach(MenuXmlNode applicationMenuXmlNode in aParser.Root.ApplicationsItems)
			{
				FilterXMLObject(applicationMenuXmlNode, aParser);
			}
		}

		//---------------------------------------------------------------------
		private void FilterXMLObject (MenuXmlNode currNode, MenuXmlParser aParser)
		{
			if (currNode == null) 
				return;

			if (currNode.IsApplication) 
			{
				List<MenuXmlNode> groupItems = currNode.GroupItems;
				if (groupItems != null && groupItems.Count > 0) 
				{
					foreach (MenuXmlNode groupNode in groupItems)
						FilterXMLObject(groupNode, aParser);
				}	
			}

			if (currNode.IsGroup || currNode.IsMenu)
			{
                List<MenuXmlNode> menuItems = currNode.MenuItems;
				if (menuItems != null  && menuItems.Count > 0) 
				{
					foreach (MenuXmlNode menuNode in menuItems)
					{
						FilterXMLObject(menuNode, aParser);
						//if (!ExistOSLGrants(menuNode.ItemObject, GetType(menuNode) ))
						if (menuNode.MenuItems == null && !menuNode.HasCommandChildNodes())
							aParser.RemoveNode(menuNode);
					}	
				
					if (currNode.MenuItems == null && !currNode.HasCommandChildNodes())
						aParser.RemoveNode(currNode);
				}
			}
			
			if (currNode.IsMenu || currNode.IsCommand)
			{
                List<MenuXmlNode> commandItems = currNode.CommandItems;
				if (commandItems != null && commandItems.Count > 0)
				{
					foreach (MenuXmlNode commandNode in commandItems)
					{
						FilterXMLObject(commandNode, aParser);
						if (!ExistExecuteGrant(commandNode.ItemObject, GetType(commandNode)))
							aParser.RemoveNode(commandNode);
					}	
				}
				else
				{
					if (!ExistExecuteGrant(currNode.ItemObject, GetType(currNode)))
						aParser.RemoveNode(currNode);
				}
			}
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// usata anche nel SecurityAdminPlugIn
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static int GetType(MenuXmlNode node)
		{
			if (node == null)
				return -1;

			if (node.Type.IsRunDocument)
				return (int)SecurityType.DataEntry;
				
			if (node.Type.IsRunReport)
				return (int)SecurityType.Report;

			if (node.Type.IsRunBatch)
				return (int) SecurityType.Batch;

			if (node.Type.IsRunFunction)
				return (int) SecurityType.Function; 

            //if (node.Type.IsExternalItem)
            //    return (int)Enum.Parse(typeof(SecurityType), node.ExternalItemType);

			if (node.Type.IsOfficeItem)
			{
                if (node.IsWordDocument || node.IsWordDocument2007)
					return (int) SecurityType.WordDocument;

                if (node.IsExcelDocument || node.IsExcelDocument2007)
					return (int) SecurityType.ExcelDocument;

                if (node.IsWordTemplate || node.IsWordTemplate2007)
					return (int) SecurityType.WordTemplate;

                if (node.IsExcelTemplate || node.IsExcelTemplate2007)
					return (int) SecurityType.ExcelTemplate;
			}

            if (node.Type.IsExternalItem)
            {
                if(node.ExternalItemType.CompareNoCase(SecurityType.HotLink.ToString()))
                    return (int)SecurityType.HotLink;
                if (node.ExternalItemType.CompareNoCase(SecurityType.View.ToString()))
                    return (int)SecurityType.View;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Table.ToString()))
                    return (int)SecurityType.Table;
                if (node.ExternalItemType.CompareNoCase(SecurityType.DataEntry.ToString()))
                    return (int)SecurityType.DataEntry;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Report.ToString()))
                    return (int)SecurityType.Report;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Tab.ToString()))
                    return (int)SecurityType.Tab;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Tabber.ToString()))
                    return (int)SecurityType.Tabber;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Tile.ToString()))
                    return (int)SecurityType.Tile;
                if (node.ExternalItemType.CompareNoCase(SecurityType.TileManager.ToString()))
                    return (int)SecurityType.TileManager;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Toolbar.ToString()))
                    return (int)SecurityType.Toolbar;
                if (node.ExternalItemType.CompareNoCase(SecurityType.ToolbarButton.ToString()))
                    return (int)SecurityType.ToolbarButton;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Grid.ToString()))
                    return (int)SecurityType.Grid;
                if (node.ExternalItemType.CompareNoCase(SecurityType.GridColumn.ToString()))
                    return (int)SecurityType.GridColumn;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Control.ToString()))
                    return (int)SecurityType.Control;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Function.ToString()))
                    return (int)SecurityType.Function;
                if (node.ExternalItemType.CompareNoCase(SecurityType.RowView.ToString()))
                    return (int)SecurityType.RowView;
                if (node.ExternalItemType.CompareNoCase(SecurityType.ChildForm.ToString()))
                    return (int)SecurityType.ChildForm;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Tabber.ToString()))
                    return (int)SecurityType.Tabber;
                if (node.ExternalItemType.CompareNoCase(SecurityType.TileManager.ToString()))
                    return (int)SecurityType.TileManager;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Tile.ToString()))
                    return (int)SecurityType.Tile;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Toolbar.ToString()))
                    return (int)SecurityType.Toolbar;
                if (node.ExternalItemType.CompareNoCase(SecurityType.ToolbarButton.ToString()))
                    return (int)SecurityType.ToolbarButton;
                if (node.ExternalItemType.CompareNoCase(SecurityType.EmbeddedView.ToString()))
                    return (int)SecurityType.EmbeddedView;
                if (node.ExternalItemType.CompareNoCase(SecurityType.TilePanelTab.ToString()))
                    return (int)SecurityType.TilePanelTab;
                if (node.ExternalItemType.CompareNoCase(SecurityType.PropertyGrid.ToString()))
                    return (int)SecurityType.PropertyGrid;
                if (node.ExternalItemType.CompareNoCase(SecurityType.Finder.ToString()))
                    return (int)SecurityType.Finder;
            }

			return -1;
		}

		//---------------------------------------------------------------------
		private void FilterEasylook (MenuXmlNode currNode, MenuXmlParser aParser, bool showDocuments)
		{
			if (currNode == null) 
				return;

            List<MenuXmlNode> menuItems = currNode.MenuItems;
			if (menuItems != null ) 
			{
				foreach ( MenuXmlNode menuNode in menuItems)
				{
                    FilterEasylook(menuNode, aParser, showDocuments);
				}	
			}

            List<MenuXmlNode> commandItems = currNode.CommandItems;
			if ( commandItems != null )
			{
				foreach (MenuXmlNode commandNode in commandItems)
				{
                    if (commandNode.IsRunReport)
                        continue;

                    if
                        (
							 !commandNode.NoWeb && showDocuments &&
                            (commandNode.IsRunDocument || commandNode.IsRunBatch || commandNode.IsDocumentShortcut || commandNode.IsRunFunction)
                        )
                        continue;

                    aParser.RemoveNode(commandNode);
                }	
			}
			if (!currNode.HasMenuChildNodes() && !currNode.HasCommandChildNodes())
				aParser.RemoveNode(currNode);
		}

		//---------------------------------------------------------------------
		public void FilterEasylook (MenuXmlParser aParser, bool showDocuments)
		{
 			foreach(MenuXmlNode applicationMenuXmlNode in aParser.Root.ApplicationsItems)
			{
				if (applicationMenuXmlNode.GroupItems == null || applicationMenuXmlNode.GroupItems.Count == 0)
				{
					aParser.RemoveNode(applicationMenuXmlNode);
					continue;
				}
				foreach(MenuXmlNode groupMenuXmlNode in applicationMenuXmlNode.GroupItems)
				{
                    FilterEasylook(groupMenuXmlNode, aParser, showDocuments);
				}
				if (applicationMenuXmlNode.GroupItems == null || applicationMenuXmlNode.GroupItems.Count == 0)
					aParser.RemoveNode(applicationMenuXmlNode);
			}
		}
	}
}
