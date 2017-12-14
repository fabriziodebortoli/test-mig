using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microarea.Tools.TBLocalizer
{
	//================================================================================
	public class DictionaryDocument : DataDocument
	{
		[Flags]
			public enum StringComparisonFlags 
		{		
			PERFECT_MATCH		= 0x00000000,
			IGNORE_CASE			= 0x00000001,
			IGNORE_SPACES		= 0x00000002,
			IGNORE_PUNCTUATION 	= 0x00000004,
			IGNORE_ALL			= IGNORE_CASE | IGNORE_PUNCTUATION | IGNORE_SPACES 
		}
		

		//--------------------------------------------------------------------------------
		public DictionaryDocument()
		{
			
		}

		//--------------------------------------------------------------------------------
		public DictionaryDocument(LocalizerDocument doc, XmlElement rootNode) 
			: base (doc, rootNode)
		{
		}

		/// <summary>
		/// (dic/resx)Restituisce il nodo da tradurre con quel name estraendolo dal dom in memoria.
		/// </summary>
		/// <param name="idd">valore dell'attributo name da cercare</param>
		//---------------------------------------------------------------------
		internal XmlNode GetNodeToTranslate(string idd)
		{
			if (idd == null) return null;
			return currentDocument.SelectSingleNode
				(
				"//node()" + 
				CommonFunctions.XPathWhereClause(AllStrings.name, idd) + 
				"[" +
				AllStrings.stringTag +
				"]"
				);
		}
		
		//---------------------------------------------------------------------
		private XmlNodeList StringNodes { get { return resource.ChildNodes; } }

		//---------------------------------------------------------------------
		private bool GetExistingStringNodes
			(
			string aString, 
			out XmlElement[] stringNodes, 
			out DictionaryDocument.StringComparisonFlags[] comparisonTypes,
			out long targetID
			)
		{
			ArrayList nodeList = new ArrayList();
			ArrayList typeList = new ArrayList();
			StringComparisonFlags flags = CommonFunctions.CurrentEnvironmentSettings.StringComparisonFlags;
			targetID = 0; 
			foreach (XmlElement el in StringNodes)
			{
				string idAttr = el.GetAttribute(AllStrings.id);
				if (idAttr.Length > 0)
				{
					try 
					{
						targetID = Math.Max(long.Parse(idAttr), targetID);
					} 
					catch 
					{
					}
				}
				StringComparisonFlags actualFlag;			
				if (DictionaryDocumentFunctions.MatchString(aString, el.GetAttribute(AllStrings.baseTag), flags, out actualFlag))
				{
					nodeList.Add(el);
					typeList.Add(actualFlag);
				}
			}
			targetID ++;
			stringNodes = (XmlElement[]) nodeList.ToArray(typeof(XmlElement));
			comparisonTypes = (StringComparisonFlags[]) typeList.ToArray(typeof(StringComparisonFlags));

			DictionaryDocumentFunctions.OrderStringNodes(stringNodes, comparisonTypes);

			return nodeList.Count > 0;
		}

		//---------------------------------------------------------------------
		internal XmlElement WriteString(string aString)
		{
			return WriteString(aString, null);
		}

		//---------------------------------------------------------------------
		internal XmlElement WriteString(string aString, string idValue)
		{
			if (aString == null || aString.Trim() == string.Empty) return null;

			aString = CommonFunctions.UnescapeString(aString).Trim(' ');
			
			long temporaryId = 0;
			long targetID = 0;
			StringComparisonFlags temporaryFlag = StringComparisonFlags.PERFECT_MATCH;
			
			XmlElement[] stringNodes;
			StringComparisonFlags[] comparisonflags; 
			if (GetExistingStringNodes(aString, out stringNodes, out comparisonflags, out targetID))
			{
				for (int i = 0; i < stringNodes.Length; i++)
				{
					XmlElement existingNode = stringNodes[i];
					if (existingNode.Attributes[AllStrings.name] == null || 
						existingNode.Attributes[AllStrings.name].Value == idValue)
					{
						if (comparisonflags[i] == StringComparisonFlags.PERFECT_MATCH)
						{
							existingNode.SetAttribute(AllStrings.valid, AllStrings.trueTag);
							modified = true;
							return existingNode;
						}
						else
						{
							temporaryId = long.Parse(existingNode.GetAttribute(AllStrings.id));
							temporaryFlag = comparisonflags[i];
							break;
						}
					}
				}	
			}
				
			XmlElement singleString = currentDocument.CreateElement(AllStrings.stringTag);

			//name = id 
			if (idValue != null) 
			{
				XmlNodeList listSameName = resource.SelectNodes (AllStrings.stringTag + CommonFunctions.XPathWhereClause(AllStrings.name, idValue));
				
				foreach (XmlElement n in listSameName)
					n.SetAttribute(AllStrings.valid, AllStrings.falseTag);
		
				singleString.SetAttribute (AllStrings.name, idValue);
			}

			singleString.SetAttribute (AllStrings.baseTag, aString.Trim(' '));
			singleString.SetAttribute(AllStrings.id, targetID.ToString());
			singleString.SetAttribute(AllStrings.valid, AllStrings.trueTag);
							
			if (temporaryId != 0)
			{
				singleString.SetAttribute(AllStrings.temporaryId, temporaryId.ToString());
				singleString.SetAttribute(AllStrings.matchType, ((int)temporaryFlag).ToString());
			}
						
			resource.AppendChild(singleString);

			modified = true;

			return singleString;

		}

		//---------------------------------------------------------------------
		internal void WriteStringNode(XmlElement aNode)
		{
			if (aNode == null) return;

			string aString	= aNode.GetAttribute(AllStrings.baseTag);
			string target	= aNode.GetAttribute(AllStrings.target);
	
			resource.AppendChild(currentDocument.ImportNode(aNode, true));

			modified = true;
		}

		//--------------------------------------------------------------------------------
		public string GetString(string lookupString, bool searchTargetString, bool ignoreWhiteSpaces)
		{
			lookupString = CommonFunctions.UnescapeString(lookupString);

			string retVal = SearchString(lookupString, searchTargetString, ignoreWhiteSpaces);
			if (retVal != null) return retVal;

			lookupString = lookupString.Trim(' ');

			retVal = SearchString(lookupString, searchTargetString, ignoreWhiteSpaces);
			if (retVal != null) return retVal;

			lookupString = lookupString.Replace("\r\n", "\n");

			retVal = SearchString(lookupString, searchTargetString, ignoreWhiteSpaces);
			if (retVal != null) return retVal;
			
			lookupString = lookupString.Replace("''", "'");

			retVal = SearchString(lookupString, searchTargetString, ignoreWhiteSpaces);
			if (retVal != null) return retVal;
			
			lookupString = lookupString.Trim();

			return SearchString(lookupString, searchTargetString, ignoreWhiteSpaces);
		}

		//--------------------------------------------------------------------------------
		private string SearchString(string lookupString, bool searchTargetString, bool ignoreWhiteSpaces)
		{
			string whereClause = CommonFunctions.XPathWhereClause ( searchTargetString ? AllStrings.baseTag : AllStrings.target, lookupString );

			//non permetto duplicazione di stessa base e stessa target
			XmlNodeList list = currentDocument.SelectNodes
				(
				"//" + 
				AllStrings.stringTag 
				);

			if (list == null) return null;

			foreach (XmlElement el in list)
			{
				string matchValue = el.GetAttribute(searchTargetString ? AllStrings.baseTag : AllStrings.target);
				if (Convert(matchValue, ignoreWhiteSpaces) == Convert(lookupString, ignoreWhiteSpaces)) 
					return el.GetAttribute(searchTargetString ? AllStrings.target : AllStrings.baseTag);
			}

			return null;
		}
		
		//--------------------------------------------------------------------------------
		private string Convert (string sourceString, bool ignoreWhiteSpaces)
		{
			return ignoreWhiteSpaces 
				? Regex.Replace(sourceString, "\\s", "")
				: sourceString;
		}
	}
}
