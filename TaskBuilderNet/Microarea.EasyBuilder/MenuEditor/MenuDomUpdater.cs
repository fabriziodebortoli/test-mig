using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Microarea.TaskBuilderNet.Core.MenuManagerLoader;

namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	internal class MenuDomUpdater : IMenuDomUpdater
	{
		private MenuModel currentMenuModel;
		private IDomUpdater domUpdater;

		//---------------------------------------------------------------------
		public MenuDomUpdater(MenuModel aMenuModel, XmlDocument domToBeUpdated, XmlDocument fullMenuXmlDom)
		{
			if (aMenuModel == null || domToBeUpdated == null)
				throw new ArgumentNullException("aMenuModel or domToBeUpdated are null");
				
			currentMenuModel = aMenuModel;
			domUpdater = new DomUpdater(domToBeUpdated, fullMenuXmlDom);

			aMenuModel.PropertyValueChanged += new EventHandler<MenuItemPropertyValueChangedEventArgs>(MenuModel_PropertyValueChanged);
			aMenuModel.MenuItemAdded += new EventHandler<MenuItemEventArgs>(MenuModel_MenuItemAdded);
			aMenuModel.MenuItemMoved += new EventHandler<MenuItemEventArgs>(MenuModel_MenuItemMoved);
			aMenuModel.MenuItemRemoved += new EventHandler<MenuItemEventArgs>(MenuModel_MenuItemRemoved);
			aMenuModel.MenuModelCleared += new EventHandler<EventArgs>(MenuModel_MenuModelCleared);
		}

		//---------------------------------------------------------------------
		private void MenuModel_PropertyValueChanged(object sender, MenuItemPropertyValueChangedEventArgs e)
		{
			if (e == null || e.ChangedItem == null || e.Property == null)
				return;

			ChangeMenuItemNodePropertyValue(e);
		}

		//---------------------------------------------------------------------
		private void MenuModel_MenuItemAdded(object sender, MenuItemEventArgs e)
		{
			if (e == null || e.MenuItem == null)
				return;

			AddMenuItemNode(e);
		}

		//---------------------------------------------------------------------
		private void MenuModel_MenuItemMoved(object sender, MenuItemEventArgs e)
		{
			if (e == null || e.MenuItem == null)
				return;

			MoveMenuItemNode(e);
		}

		//---------------------------------------------------------------------
		private void MenuModel_MenuItemRemoved(object sender, MenuItemEventArgs e)
		{
			if (e == null || e.MenuItem == null)
				return;

			RemoveMenuItemNode(e);
		}

		//---------------------------------------------------------------------
		private void MenuModel_MenuModelCleared(object sender, EventArgs e)
		{
			domUpdater.ClearDom();
		}

		//---------------------------------------------------------------------
		public void AddMenuItemNode(MenuItemEventArgs args)
		{
			if (args == null || args.MenuItem == null)
				return;

			string toBeAddedTag = GetMenuItemTagName(args); ;
			if (toBeAddedTag == null || toBeAddedTag.Length == 0)
			{
				Debug.Fail("Unable to add node because xml tag is empty.");
				return;
			}

			// se args.ParentItem allora sto aggiungendo un'Application
			string parentXPathQuery = args.ParentItem != null
				? MenuModel.GetModelFullPath(args.ParentItem as BaseMenuItem)
				: MenuXmlNode.XML_TAG_MENU_ROOT;

			string beforeItemXPathQuery = args.BeforeItem != null
				? MenuModel.GetModelFullPath(args.BeforeItem as BaseMenuItem)
				: string.Empty;

			XmlElement newNodeToBeAdded = GetMenuItemXmlElement(args.MenuItem);

			try
			{
				domUpdater.AddNode(parentXPathQuery, beforeItemXPathQuery, newNodeToBeAdded);
			}
			catch
			{}
		}

		//---------------------------------------------------------------------
		public void MoveMenuItemNode(MenuItemEventArgs args)
		{
			XmlNode aNode = RemoveMenuItemNode(args);
			if (aNode == null)
			{
				Debug.Fail("Unable to move node.");
				return;
			}

			AddMenuItemNode(args);
		}

		//---------------------------------------------------------------------
		public XmlNode RemoveMenuItemNode(MenuItemEventArgs args)
		{
			if (args == null || args.MenuItem == null)
				return null;

			string toBeRemovedTag = GetMenuItemTagName(args); ;
			if (toBeRemovedTag == null || toBeRemovedTag.Length == 0)
			{
				Debug.Fail("Unable to remove node because xml tag is empty.");
				return null;
			}

			// Costruisco la query xpath che identifica il nodo da rimuovere.
			StringBuilder xPathQueryBuilder = new StringBuilder();
			if (args.ParentItem == null)
				xPathQueryBuilder.Append(MenuXmlNode.XML_TAG_MENU_ROOT);
			else
			{
				object tempItem = args.PreviousParentItem;
				if (tempItem == null)
					tempItem = args.ParentItem;

				xPathQueryBuilder.Append(MenuModel.GetModelFullPath(tempItem as BaseMenuItem));
			}

			if (xPathQueryBuilder.Length == 0)
			{
				Debug.Fail("Unable to remove node because it is not contained in DOM.");
				return null;
			}

			BaseMenuItem aBaseMenuItem = args.MenuItem as BaseMenuItem;
			MenuCommand aMenuCommand = args.MenuItem as MenuCommand;

			if (aMenuCommand != null)
			{
				if (
					aMenuCommand.Title != null &&
					aMenuCommand.Title.Text != null &&
					aMenuCommand.Title.Text.Length > 0
					)
				{
					xPathQueryBuilder.AppendFormat(
						"/{0}[{1}='{2}']",
						toBeRemovedTag,
						MenuXmlNode.XML_TAG_TITLE,
						aMenuCommand.Title.Text
						);
				}
				else
				{
					Debug.Fail("Found MenuCommand with no title.");
					return null;
				}
			}
			else if (aBaseMenuItem != null)
			{
				if (aBaseMenuItem.Name != null && aBaseMenuItem.Name.Length > 0)
				{
					xPathQueryBuilder.AppendFormat(
						"/{0}[@{1}='{2}']",
						toBeRemovedTag,
						MenuXmlNode.XML_ATTRIBUTE_NAME,
						aBaseMenuItem.Name
						);
				}
				else if (
						aBaseMenuItem.Title != null &&
						aBaseMenuItem.Title.Text != null &&
						aBaseMenuItem.Title.Text.Length > 0
						)
				{
					xPathQueryBuilder.AppendFormat(
						"/{0}[{1}='{2}']",
						toBeRemovedTag,
						MenuXmlNode.XML_TAG_TITLE,
						aBaseMenuItem.Title.Text
						);
				}
				else
				{
					Debug.Fail("Found BaseItem with no name nor title.");
					return null;
				}
			}

			try
			{
				// Finalmente rimuovo il nodo.
				return domUpdater.RemoveNode(xPathQueryBuilder.ToString());
			}
			catch
			{
				return null;
			}
		}

		//---------------------------------------------------------------------
		public void ChangeMenuItemNodePropertyValue(MenuItemPropertyValueChangedEventArgs aPropertyValueChangedEventArgs)
		{
			if (
				aPropertyValueChangedEventArgs == null ||
				aPropertyValueChangedEventArgs.Property == null ||
				aPropertyValueChangedEventArgs.ChangedItem == null
				)
				return;

			object[] xmlNodeTypeAttributes = aPropertyValueChangedEventArgs.Property.GetCustomAttributes
				(
					typeof(MenuItemXmlNodeTypeAttribute),
					true
				);

			if (xmlNodeTypeAttributes == null || xmlNodeTypeAttributes.Length == 0)
			{
				Debug.Fail(
					"MenuItemXmlNodeTypeAttribute missing for the " +
					aPropertyValueChangedEventArgs.Property.Name +
					"property."
					);
				return;
			}

			XmlNodeType propertyXmlNodeType = ((MenuItemXmlNodeTypeAttribute)xmlNodeTypeAttributes[0]).XmlNodeType;
			if (propertyXmlNodeType == XmlNodeType.None)
			{
				Debug.Fail(
					"Invalid MenuItemXmlNodeTypeAttribute for the " +
					aPropertyValueChangedEventArgs.Property.Name +
					"property."
					);
				return;
			}

			object[] xmlNodeNameAttributes = aPropertyValueChangedEventArgs.Property.GetCustomAttributes
				(
					typeof(MenuItemXmlNodeNameAttribute),
					true
				);

			if (xmlNodeNameAttributes == null || xmlNodeNameAttributes.Length == 0)
			{
				Debug.Fail(
					"MenuItemXmlNodeNameAttribute missing for the " +
					aPropertyValueChangedEventArgs.Property.Name +
					"property."
					);
				return;
			}

			string propertyXmlNodeName = ((MenuItemXmlNodeNameAttribute)xmlNodeNameAttributes[0]).XmlNodeName;
			if (propertyXmlNodeName == null && propertyXmlNodeName.Length == 0)
			{
				Debug.Fail(
					"Invalid MenuItemXmlNodeNameAttribute for the " +
					aPropertyValueChangedEventArgs.Property.Name +
					" property."
					);
				return;
			}

			string xPathQuery = BuildXPathQuery(
				aPropertyValueChangedEventArgs,
				propertyXmlNodeType,
				propertyXmlNodeName
				);

			if (xPathQuery == null || xPathQuery.Length == 0)
			{
				Debug.Fail("XPath Expression construction failed.");
				return;
			}

			string propertyValue = aPropertyValueChangedEventArgs.Property.GetValue(
							aPropertyValueChangedEventArgs.ChangedItem,
							null
							).ToString();

			try
			{
				domUpdater.UpdateNodeProperty(
					propertyValue,
					propertyXmlNodeType,
					propertyXmlNodeName,
					xPathQuery
					);
			}
			catch
			{}
		}

		//---------------------------------------------------------------------
		private static string BuildXPathQuery(
			MenuItemPropertyValueChangedEventArgs aPropertyValueChangedEventArgs,
			XmlNodeType propertyXmlNodeType,
			string propertyXmlNodeName
			)
		{
			string xPathQuery =
				MenuModel.GetModelFullPath(aPropertyValueChangedEventArgs.ChangedItem as BaseMenuItem);
			if (xPathQuery == null || xPathQuery.Length == 0)
			{
				Debug.Fail("XPath Expression construction failed.");
				return string.Empty;
			}

			// Ci interessa l'ultima parte del percorso xPath
			// perchè è quella che descrive l'elemento che è cambiato.
			int lastSlashIdx = xPathQuery.LastIndexOf('/');
			StringBuilder xPathQueryLastToken = new StringBuilder((lastSlashIdx >= 0) ? xPathQuery.Substring(lastSlashIdx) : xPathQuery);

			// Costruisco il pattern per la regular expression alla
			// ricerca dell'elemento o della proprietà cambiata.
			// .*[@{propertyName}='(?<propertyValue>.*)'
			StringBuilder exprItemToSearch = new StringBuilder(".*\\[");
			if (propertyXmlNodeType == XmlNodeType.Attribute)
				exprItemToSearch.Append("@");

			exprItemToSearch.Append(propertyXmlNodeName);
			exprItemToSearch.Append("='(?<propertyValue>.*)'");

			Regex propertyRegex = new Regex(exprItemToSearch.ToString());
			Match propertyMatch = propertyRegex.Match(xPathQueryLastToken.ToString());
			if (propertyMatch != null && propertyMatch.Success)
			{
				try
				{
					// Sostituisco il nuovo valore con il vecchio per l'attributo o 
					// l'elemento in questione perchè il DOM in memoria non è ancora
					// allineato con il modello di menù per cui devo andare alla 
					// ricerca del vecchio valore.
					int index = propertyMatch.Groups["propertyValue"].Index;
					int len = propertyMatch.Groups["propertyValue"].Length;
					xPathQueryLastToken.Replace(
						propertyMatch.Groups["propertyValue"].Value,
						aPropertyValueChangedEventArgs.PreviousValue.ToString(),
						index,
						len
						);
				}
				catch (Exception exc)
				{
					Debug.Fail(exc.Message);
				}
				// Costruisco la query xPath per la ricerca del vecchio nel DOM in memoria.
				xPathQuery = String.Concat(xPathQuery.Substring(0, lastSlashIdx), xPathQueryLastToken.ToString());
			}
			return xPathQuery;
		}

		//---------------------------------------------------------------------
		private XmlElement GetMenuItemXmlElement(object menuItem)
		{
			XmlElement newElement = null;
			BaseMenuItem aBaseMenuItem = menuItem as BaseMenuItem;
			MenuCommand aMenuCommand = menuItem as MenuCommand;

			if (aBaseMenuItem != null)
				newElement = aBaseMenuItem.GetXmlElement(this.domUpdater.Dom);
			else if (aMenuCommand != null)
				newElement = aMenuCommand.GetXmlElement(this.domUpdater.Dom);

			return newElement;
		}

		//---------------------------------------------------------------------
		private static string GetMenuItemTagName(MenuItemEventArgs args)
		{
			string toBeRemovedTag = string.Empty;
			BaseMenuItem aBaseMenuItem = args.MenuItem as BaseMenuItem;
			MenuCommand aMenuCommand = args.MenuItem as MenuCommand;

			if (aBaseMenuItem != null)
				toBeRemovedTag = aBaseMenuItem.GetXmlTag();
			else if (aMenuCommand != null)
				toBeRemovedTag = aMenuCommand.GetXmlTag();

			return toBeRemovedTag;
		}
	}
}
