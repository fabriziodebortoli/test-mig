using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	/// <summary>
	/// PseudoXPath is a class which aims to solve some bugging issues with case-sensitivity 
	/// caused by the look-up implemented by using XPath searches (attribute values representing 
	/// Windows FS paths). 
	/// PseudoXPath supports a minimal sub set of XPath grammar, but attributes values are 
	/// checked in a case-insensitive manner.
	/// The class provides a replacement for XmlDocument.SelectSingleNode and
	/// XmlElement.SelectSingleNode and by no means the class aim to be more performing than 
	/// the XPath searches permitted by such methods.
	/// </summary>
	public class PseudoXPath
	{
		//---------------------------------------------------------------------
		public static XmlElement SelectSingleElement(XmlDocument startingElement, string xPath)
		{
			XmlNode node = (XmlNode)startingElement;
			return SelectSingleElement(node, xPath);
		}
		//---------------------------------------------------------------------
		public static XmlElement SelectSingleElement(XmlElement startingElement, string xPath)
		{
			return SelectSingleElement((XmlNode)startingElement, xPath);
		}
		
		//---------------------------------------------------------------------
		private static XmlElement SelectSingleElement(XmlNode startingElement, string pseudoXPath)
		{
			Debug.Assert
				(startingElement.NodeType == XmlNodeType.Element
				|| startingElement.NodeType == XmlNodeType.Document);

			// boring checks
			if (pseudoXPath == null)
				throw new ArgumentNullException();
			if (pseudoXPath == null || pseudoXPath.Length == 0)
				throw new ArgumentException();
			if (pseudoXPath[0] == '/')
			{
				if (pseudoXPath.Length == 1)
					throw new ArgumentException();
				if (pseudoXPath[1] == '/')
				{
					Debug.WriteLine("Full XPath sintax is not supported by the method; search will be performed as case-sensitive.");
					return startingElement.SelectSingleNode(pseudoXPath) as XmlElement;
				}
				if (startingElement.NodeType != XmlNodeType.Document)
				{
					if (startingElement.OwnerDocument == null)
					{
						Debug.WriteLine("Full XPath sintax is not supported by the method; search will be performed as case-sensitive.");
						return startingElement.SelectSingleNode(pseudoXPath) as XmlElement;
					}
					startingElement = startingElement.OwnerDocument;
				}
				pseudoXPath = pseudoXPath.Substring(1);
			}

			// actual implementation starts here
			XmlElement xPathEl = startingElement.SelectSingleNode(pseudoXPath) as XmlElement;
			if (xPathEl != null)
				return xPathEl;
			string[] tokens = pseudoXPath.Split('/');
			XmlNode currEl = startingElement;
			foreach (string token in tokens)
			{
				int bracketPos = token.IndexOf('[');
				if (bracketPos == -1)
				{
					currEl = currEl.SelectSingleNode(token) as XmlElement;
					if (currEl == null)
						return null;
				}
				else
				{
					string tagName = token.Substring(0, bracketPos);
					
					string whereClause = token.Substring(bracketPos);
					string attrName;
					string attrValue;
					ParseSingleAttributeWhereClause(whereClause, out attrName, out attrValue);
					
					currEl = FindTagWithCaseInsensitiveAttribute((XmlElement)currEl, tagName, attrName, attrValue);
					if (currEl == null)
						return null;
				}
			}
			return currEl as XmlElement;
		}

		//---------------------------------------------------------------------
		private static void ParseSingleAttributeWhereClause
			(
			string whereClause, //[@name='value']
			out string attrName,
			out string attrValue
			)
		{
			int equalPos = whereClause.IndexOf('=');
			int endBracketPos = whereClause.IndexOf(']');
			Debug.Assert(equalPos != -1);
			Debug.Assert(endBracketPos != -1);
			Debug.Assert(whereClause.Length >= 6); //[@name='value']
			Debug.Assert(whereClause[1] == '@');
			Debug.Assert(whereClause[equalPos+1] == '\'');
			Debug.Assert(whereClause[whereClause.Length-2] == '\'');
			Debug.Assert(whereClause[whereClause.Length-1] == ']');
			
			if (equalPos == -1 
				|| endBracketPos == -1
				|| whereClause.Length < 6
				|| whereClause[1] != '@'
				|| whereClause[equalPos+1] != '\''
				|| whereClause[whereClause.Length-2] != '\''
				|| whereClause[whereClause.Length-1] != ']')
				throw new ArgumentException();

			attrName = whereClause.Substring(2, equalPos-2); //[@name='value'] ==> name
			attrValue = whereClause.Substring(equalPos+2, endBracketPos-equalPos-3); //[@name='value'] ==> value
		}

		//---------------------------------------------------------------------
		private static XmlElement FindTagWithCaseInsensitiveAttribute
			(
			XmlElement parent,
			string tagName,
			string attrName,
			string attrValue
			)
		{
			foreach (XmlElement el in parent.GetElementsByTagName(tagName))
			{
				string currAttr = el.GetAttribute(attrName);
				if (string.Compare(currAttr, attrValue, true, CultureInfo.InvariantCulture) == 0)
					return el;
			}
			return null;
		}

		//---------------------------------------------------------------------
	}
}
