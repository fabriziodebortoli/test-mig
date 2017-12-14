using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;

namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	internal class DomUpdater : IDomUpdater
	{
		private XmlDocument workingDom;
		private XmlDocument fullMenuXmlDom;

		//---------------------------------------------------------------------
		public DomUpdater(XmlDocument domToBeUpdated, XmlDocument fullMenuXmlDom = null)
		{
			if (domToBeUpdated == null)
				throw new ArgumentNullException("domToBeUpdated is null");

			this.workingDom = domToBeUpdated;
			this.fullMenuXmlDom = fullMenuXmlDom;
		}

		//---------------------------------------------------------------------
		public XmlDocument Dom { get { return this.workingDom; } }

		//---------------------------------------------------------------------
		public void ClearDom()
		{
			this.workingDom.DocumentElement.RemoveAll();
		}

		//---------------------------------------------------------------------
		public void AddNode(string parentXPathQuery, string beforeItemXPathQuery, XmlElement nodeToBeAdded)
		{
			if (parentXPathQuery == null || parentXPathQuery.Length == 0)
			{
				workingDom.AppendChild(nodeToBeAdded);
				return;
			}

			XmlNode parentNode = null;
			try
			{
				parentNode = workingDom.SelectSingleNode(parentXPathQuery);
			}
			catch (XPathException xPathExc)
			{
				Debug.Fail("Unable to add node to DOM: " + xPathExc.ToString());
				return;
			}

			if (parentNode == null)
			{
				XmlElement parentElement = CloneByOriginal(parentXPathQuery);

				AddNode(
					parentXPathQuery.Substring(0, parentXPathQuery.LastIndexOf('/')),
					"",
					parentElement
					);
				parentNode = workingDom.SelectSingleNode(parentXPathQuery);
			}

			bool isToBeInsertedInHead =	(beforeItemXPathQuery == null || beforeItemXPathQuery.Length == 0);

			XmlNode beforeItemNode = null;
			if (!isToBeInsertedInHead)
			{
				try
				{
					beforeItemNode = workingDom.SelectSingleNode(beforeItemXPathQuery);
				}
				catch (XPathException xPathExc)
				{
					Debug.Fail("Unable to add node from DOM: " + xPathExc.ToString());
					return;
				}
				if (beforeItemNode == null)
					isToBeInsertedInHead = true;
			}

			try
			{
				nodeToBeAdded = parentNode.OwnerDocument.ImportNode(nodeToBeAdded as XmlNode, true) as XmlElement;
			
				if (isToBeInsertedInHead)
					parentNode.InsertBefore(nodeToBeAdded, parentNode.FirstChild);
				else
					parentNode.InsertAfter(nodeToBeAdded, beforeItemNode);
			}
			catch (Exception exc)
			{
				Debug.Fail("Unable to add node to DOM: " + exc.Message);
				return;
			}
		}

		//---------------------------------------------------------------------
		private XmlElement CloneByOriginal(string xPathQuery)
		{
			if (fullMenuXmlDom == null)
				return CreateElement(xPathQuery);

			XmlNode parentNode = fullMenuXmlDom.SelectSingleNode(xPathQuery);

			if (parentNode == null)
				return CreateElement(xPathQuery);

			XmlNode clonedNode = parentNode.Clone();
			if (
					clonedNode.Name != MenuXmlNode.XML_TAG_APPLICATION &&
					clonedNode.Name != MenuXmlNode.XML_TAG_GROUP &&
					clonedNode.Name != MenuXmlNode.XML_TAG_MENU
				)
				return clonedNode as XmlElement;

			XmlAttribute imageFileAttr = clonedNode.Attributes[MenuXmlNode.XML_ATTRIBUTE_IMAGE_FILENAME];
			if (imageFileAttr != null)
				clonedNode.Attributes.Remove(imageFileAttr);

			for (int i = clonedNode.ChildNodes.Count - 1; i > -1; i--)
				if (clonedNode.ChildNodes[i].Name != MenuXmlNode.XML_TAG_TITLE)
					clonedNode.RemoveChild(clonedNode.ChildNodes[i]);

			return clonedNode as XmlElement;
		}

		//---------------------------------------------------------------------
		private XmlElement CreateElement(string xPathQuery)
		{
			String elemName = xPathQuery.Substring(xPathQuery.LastIndexOf('/') + 1);

			string regexElementNamePattern = "(?<ElementName>[a-zA-Z0-9]+)";
			Regex regex = new Regex(regexElementNamePattern);
			Match nodeMatch = regex.Match(elemName);
			if (nodeMatch == null || !nodeMatch.Success)
				return null;

			XmlElement elem = workingDom.CreateElement(nodeMatch.Groups["ElementName"].Value);

			regexElementNamePattern = "@(?<AttrName>[a-zA-Z0-9]+)=('|\")(?<AttrValue>[a-zA-Z0-9\\.,_]+)('|\")";
			regex = new Regex(regexElementNamePattern);
			Match attrMatch = regex.Match(elemName, nodeMatch.Index);
			do
			{
				if (attrMatch == null || !attrMatch.Success)
					break;

				elem.SetAttribute(attrMatch.Groups["AttrName"].Value, attrMatch.Groups["AttrValue"].Value);

			} while ((attrMatch = attrMatch.NextMatch()).Success);



			regexElementNamePattern = "\\[(?<ChildName>[a-zA-Z0-9]+)=('|\")(?<ChildValue>[a-zA-Z0-9\\.]+)('|\")";
			regex = new Regex(regexElementNamePattern);
			Match childMatch = null;
			do
			{
				childMatch = regex.Match(elemName, nodeMatch.Index);
				if (childMatch == null || !childMatch.Success)
					break;

				XmlElement childElem = workingDom.CreateElement(childMatch.Groups["ChildName"].Value);
				childElem.InnerText = childMatch.Groups["ChildValue"].Value;
				elem.AppendChild(childElem);

			} while ((childMatch = attrMatch.NextMatch()).Success);

			return elem;
		}

		//---------------------------------------------------------------------
		public XmlNode RemoveNode(string toBeRemovedNodeXPathQuery)
		{
			XmlNode toBeRemovedNode = null;
			try
			{
				toBeRemovedNode = workingDom.SelectSingleNode(toBeRemovedNodeXPathQuery);
			}
			catch (XPathException xPathExc)
			{
				Debug.Fail("Unable to remove node from DOM: " + xPathExc.ToString());
				return null;
			}

			if (toBeRemovedNode == null)
				return null;

			XmlNode aParentNode = toBeRemovedNode.ParentNode;

			if (aParentNode != null)
			{
				try
				{
					return aParentNode.RemoveChild(toBeRemovedNode);
				}
				catch (ArgumentException argExc)
				{
					Debug.Fail("Unable to remove node from DOM: " + argExc.ToString());
					return null;
				}
			}
			else
			{
				// Se toBeRemovedNode non ha parent
				// allora è la root per cui svuoto semplicemente il document. 
				workingDom.RemoveAll();
				return null;
			}
		}

		//---------------------------------------------------------------------
		public void UpdateNodeProperty(
			string propertyValue,
			XmlNodeType propertyXmlNodeType,
			string propertyXmlNodeName,
			string xPathQuery
			)
		{
			XmlElement aElement = null;
			try
			{
				aElement = workingDom.SelectSingleNode(xPathQuery) as XmlElement;
			}
			catch (XPathException xPathExc)
			{
				Debug.Fail("Unable to update node property from DOM: " + xPathExc.ToString());
				return;
			}

			if (aElement == null)
			{
				XmlElement parentElement = CloneByOriginal(xPathQuery);
				AddNode(
					xPathQuery.Substring(0, xPathQuery.LastIndexOf('/')),
					"",
					parentElement
					);
				aElement = workingDom.SelectSingleNode(xPathQuery) as XmlElement;
			}

			try
			{
				switch (propertyXmlNodeType)
				{
					case XmlNodeType.Attribute:
						aElement.SetAttribute(propertyXmlNodeName, propertyValue);
						break;
					case XmlNodeType.Element:
						XmlElement aPropertyNode = null;
						aPropertyNode = aElement.SelectSingleNode(String.Concat("child::", propertyXmlNodeName)) as XmlElement;
						if (aPropertyNode == null)
						{
							aPropertyNode = workingDom.CreateElement(propertyXmlNodeName);
							aElement.AppendChild(aPropertyNode);
						}
						
						if (aPropertyNode != null)
							aPropertyNode.InnerText = propertyValue;

						break;
					default: break;
				}
			}
			catch (XPathException xPathExc)
			{
				Debug.Fail("Unable to update node property from DOM: " + xPathExc.ToString());
				return;
			}
		}
	}
}
