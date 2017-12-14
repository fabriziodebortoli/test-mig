using System;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.MenuManagerLoader
{
	public static class MenuJsonStrings
	{
		public const string targetAttributeName = "target";
		public const string objectTypeAttributeName = "objectType";
		public const string argumentsAttributeName = "arguments";
		public const string imageNamespacesAttributeName = "image_namespace";
		public const string imageFileAttributeName = "image_file";
	}

	public static class MenuTranslatorStrings
	{
		public const string translateTemplate = "translate(@{0}, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')=translate('{1}', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')";
	}

	//=========================================================================
	public static class MenuConverter
	{
		
		//---------------------------------------------------------------------
		public static XmlDocument ProcessMenu(this MenuLoader loader)
		{
			XmlDocument newDoc = new XmlDocument();
			XmlElement rootElement = newDoc.CreateElement("Root");
			newDoc.AppendChild(rootElement);

			if (loader.AppsMenuXmlParser == null)
				return null;

			MenuXmlNode appMenuXmlNode = loader.AppsMenuXmlParser.Root as MenuXmlNode;
			if (appMenuXmlNode != null && appMenuXmlNode.Node != null)
			{
				XmlElement applicationMenuElement = newDoc.CreateElement("ApplicationMenu");
				applicationMenuElement.AppendChild(newDoc.ImportNode(appMenuXmlNode.OwnerDocument.DocumentElement, true));
				rootElement.AppendChild(applicationMenuElement);
			}

			if (loader.EnvironmentXmlParser != null)
			{
				MenuXmlNode envMenuXmlNode = loader.EnvironmentXmlParser.Root as MenuXmlNode;
				if (envMenuXmlNode != null && envMenuXmlNode.Node != null)
				{
					XmlElement environemntMenuElement = newDoc.CreateElement("EnvironmentMenu");
					environemntMenuElement.AppendChild(newDoc.ImportNode(envMenuXmlNode.OwnerDocument.DocumentElement, true));
					rootElement.AppendChild(environemntMenuElement);
				}
			}
		
			PromoteChildNodeToAttribute(rootElement, MenuXmlNode.XML_TAG_TITLE);
			PromoteArgumentsToAttribute(rootElement);
			AdaptImageFilePath(rootElement);

			PromoteChildNodeToAttribute(rootElement, MenuXmlNode.XML_TAG_OBJECT, MenuJsonStrings.targetAttributeName, MenuXmlNode.XML_TAG_OBJECT);

			AddNeededAttributes(newDoc);

			return newDoc;
		}

		//---------------------------------------------------------------------
		private static void AddNeededAttributes(XmlDocument appMenu)
		{
			XmlNodeList objectsNodes = appMenu.SelectNodes("//Object");
			foreach (XmlNode current in objectsNodes)
			{
				XmlAttribute attribFav = current.Attributes["isFavorite"];
				if (attribFav != null)
					continue;

				//se non c'è lo creo e lo aggiungo
				attribFav = appMenu.CreateAttribute("isFavorite");
				attribFav.Value = "false";
				current.Attributes.Append(attribFav);

				XmlAttribute isMostUsedAttr = current.Attributes["isMostUsed"];
				if (isMostUsedAttr != null)
					continue;

				//se non c'è lo creo e lo aggiungo
				isMostUsedAttr = appMenu.CreateAttribute("isMostUsed");
				isMostUsedAttr.Value = "false";
				current.Attributes.Append(isMostUsedAttr);
			}

			XmlNodeList groups = appMenu.SelectNodes("//Group");
			foreach (XmlNode current in groups)
			{
				XmlAttribute attrib = current.Attributes["isOpen"];
				if (attrib != null)
					continue;

				//se non c'è lo creo e lo aggiungo
				attrib = appMenu.CreateAttribute("isOpen");
				attrib.Value = "false";
				current.Attributes.Append(attrib);
			}
		}

		//---------------------------------------------------------------------
		private static void AdaptImageFilePath(XmlNode appMenu)
		{
			string installationPath = BasePathFinder.BasePathFinderInstance.GetStandardPath() + Path.DirectorySeparatorChar;

			XmlNodeList imageFileNodes = appMenu.SelectNodes(string.Format(".//*/@{0}", MenuJsonStrings.imageFileAttributeName));
		
			foreach (XmlAttribute attrib in imageFileNodes)
			{
				attrib.Value = attrib.Value.ReplaceNoCase(installationPath, "");
			}

			ITheme theme = DefaultTheme.GetTheme();

			XmlNodeList imageNamespaceNodes = appMenu.SelectNodes(string.Format(".//*/@{0}", MenuJsonStrings.imageNamespacesAttributeName));
			foreach (XmlAttribute attrib in imageNamespaceNodes)
			{
				string imageFile = string.Empty;
				if (!theme.Name.IsNullOrEmpty())
				{
					imageFile = BasePathFinder.BasePathFinderInstance.GetGroupImagePathByTheme(new NameSpace(attrib.Value), theme.Name);

					if (imageFile.IsNullOrEmpty())
						imageFile = BasePathFinder.BasePathFinderInstance.GetGroupImagePath(new NameSpace(attrib.Value));
				}
				else
					imageFile = BasePathFinder.BasePathFinderInstance.GetGroupImagePath(new NameSpace(attrib.Value));

				string newValue = imageFile.ReplaceNoCase(installationPath, "");
				XmlAttribute newAttr = appMenu.OwnerDocument.CreateAttribute(MenuJsonStrings.imageFileAttributeName);
				newAttr.Value = newValue;
				
				attrib.OwnerElement.Attributes.Append(newAttr);

				attrib.OwnerElement.Attributes.Remove(attrib);
			}
		}

		//---------------------------------------------------------------------
		private static void PromoteArgumentsToAttribute(XmlNode appMenu)
		{
			XmlNodeList objectsNodes = appMenu.SelectNodes(".//Arguments");
			if (objectsNodes.Count == 0)
			{
				return;
			}

			XmlAttribute objectAttribute = null;
			foreach (XmlNode objectNode in objectsNodes)
			{
				objectAttribute = objectNode.OwnerDocument.CreateAttribute(MenuJsonStrings.argumentsAttributeName);
				objectAttribute.Value =  objectNode.OuterXml.Replace("\"", "'");

				objectNode.ParentNode.Attributes.Append(objectAttribute);
				objectNode.ParentNode.RemoveChild(objectNode);
			}
		}

		//---------------------------------------------------------------------
		private static void PromoteChildNodeToAttribute(
			XmlNode appMenu,
			string nodeName,
			string attributeName = null,
			string parentNodeNewName = null
			)
		{
			XmlNodeList objectsNodes = appMenu.SelectNodes(".//" + nodeName);

			if (objectsNodes.Count == 0)
			{
				return;
			}

			XmlAttribute objectAttribute = null;
			XmlNode parent = null;
			XmlNode newParent = null;
			foreach (XmlNode objectNode in objectsNodes)
			{
				objectAttribute =
					objectNode.OwnerDocument.CreateAttribute(
					String.IsNullOrWhiteSpace(attributeName) ? nodeName.ToLowerInvariant() : attributeName
					);

				objectAttribute.Value = objectNode.InnerText;

				objectNode.ParentNode.Attributes.Append(objectAttribute);

				if (!String.IsNullOrWhiteSpace(parentNodeNewName))
				{
					parent = objectNode.ParentNode;
					newParent = ProcessParentObjectNode(parent, parentNodeNewName);
					
					XmlNode newObjectNode = newParent.SelectSingleNode("./" + nodeName);
					if (newObjectNode != null)
						newObjectNode.ParentNode.RemoveChild(newObjectNode);

					parent.ParentNode.ReplaceChild(newParent, parent);

					//chiude i nodi empty
					XmlElement el = newParent as XmlElement;
					if (el != null && !el.IsEmpty && el.ChildNodes.Count == 0)
						el.IsEmpty = true;
				}
				else
				{
					objectNode.ParentNode.RemoveChild(objectNode);
				}
			}
		}

		//---------------------------------------------------------------------
		private static XmlNode ProcessParentObjectNode(XmlNode sourceNode, string parentNodeNewName)
		{
			XmlNode newNode = sourceNode.OwnerDocument.CreateElement(parentNodeNewName);
			newNode.InnerXml = sourceNode.InnerXml;

			foreach (XmlAttribute attr in sourceNode.Attributes)
			{
				newNode.Attributes.Append(attr.CloneNode(true) as XmlAttribute);
			}

			XmlAttribute objectTypeAttr = sourceNode.OwnerDocument.CreateAttribute(MenuJsonStrings.objectTypeAttributeName);
			objectTypeAttr.Value = sourceNode.Name;

			newNode.Attributes.Append(objectTypeAttr);

			return newNode;
		}
	}
}
