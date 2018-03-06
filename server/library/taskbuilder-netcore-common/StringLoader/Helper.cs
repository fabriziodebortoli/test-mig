using Microarea.Common.NameSolver;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.StringLoader
{
    //================================================================================
    public class Helper
	{
		public const string stringConst	= "string";
		public const string baseConst	= "base";
		public const string targetConst	= "target";	
		public const string validConst	= "valid";	

		/// <summary>
		/// Restituisce la lingua corrente ("en", "it", ecc.)
		/// </summary>
		//---------------------------------------------------------------------
		public static string Culture
		{      
			get { return CultureInfo.CurrentUICulture.Name; }
		}

		//---------------------------------------------------------------------
		public static uint GetHashCode(string key)
		{
			uint nHash = 0;
			foreach (char ch in key)
				nHash = (nHash<<5) + nHash + ch;
			return nHash;
		}

		//--------------------------------------------------------------------------------
		internal static string GetSpecificDictionaryFilePath(string genericDictionaryPath)
		{
			return Path.Combine(Path.Combine(genericDictionaryPath, Culture), "Dictionary.bin");
		}
		//--------------------------------------------------------------------------------
		internal static string GetSpecificDictionaryFilePath(string application, string module, string fileName, PathFinder pf)
		{
			if (pf == null)
				return string.Empty;
		
			return pf.GetStandardModuleDictionaryFilePath (application, module, Culture); 
		}

		//---------------------------------------------------------------------
		public static bool ValidateDictionaryPath(ref string dictionaryPath)
		{
			// se il percorso esiste, tutto ok
			if (PathFinder.PathFinderInstance.ExistPath(dictionaryPath))
				return true;
			// se la culture non è specifica (es en-us) allora non ho alternative
			if (Culture.Length <= 2)
				return false;
			
			// provo con la culture generica
			dictionaryPath = Path.Combine(Path.GetDirectoryName(dictionaryPath), Culture.Substring(0, 2));

			if (!PathFinder.PathFinderInstance.ExistPath(dictionaryPath))
			{
				dictionaryPath = string.Empty;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Recupera la stringa in lingua target
		/// </summary>
		/// <param name="aNode">nodo in cui cercare la stringa</param>
		/// <param name="baseString">stringa in lingua base da cercare</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string FindString(DictionaryStringBlock dictionary, string baseString)
		{
			if (dictionary == null || dictionary.Count == 0)
				return baseString;

			string searchString = baseString;
			DictionaryStringItem targetItem = dictionary[searchString] as DictionaryStringItem;
			
			if (targetItem == null || targetItem.Target.Length == 0 )
			{
				searchString = searchString.Trim().Replace("\r\n", "\n");
				targetItem = dictionary[searchString] as DictionaryStringItem;
			}
			
			if (targetItem == null || targetItem.Target.Length == 0 )
				return baseString;
			
			return targetItem.Target;
		}

		//-----------------------------------------------------------------------------
		public static DictionaryStringBlock RetrieveStrings(XmlDocument doc)
		{
			return RetrieveStrings(doc, null);
		}

		//-----------------------------------------------------------------------------
		public static DictionaryStringBlock RetrieveStrings(XmlDocument doc, string resourceName)
		{
			if (doc == null)
			{
				Debug.Fail("XmlDocument reference can't be null");
				return null;
			}

			DictionaryStringBlock dictionary = new DictionaryStringBlock();
					
			//tutti i nodi che hanno questa nidificazione e hanno l'attributo name contengono stringhe
			string filter = "//node()[@name and string]";
			XmlNodeList resourceNodes = doc.SelectNodes(filter);
			
			foreach (XmlNode resource in resourceNodes)
			{
				XmlElement el = resource as XmlElement;

				if (resourceName != null && el != null)
				{
					string name = el.GetAttribute("name");
					if (string.Compare(name, resourceName, StringComparison.OrdinalIgnoreCase) != 0) 
						continue;
				}
				
				string baseString, targetString;
				foreach (XmlNode n in resource.ChildNodes)
				{
					if (!ParseStringElement(n, out baseString, out targetString))
						continue;
					DictionaryStringItem item = new DictionaryStringItem();
					item.Target = targetString;
					dictionary[baseString] = item;
				}
			}

			return dictionary;
		}
		
		//-----------------------------------------------------------------------------
		private static bool ParseStringElement(XmlNode n, out string baseString, out string targetString)
		{
			baseString = targetString = null;

			if (!(n is XmlElement))
				return false;
			
			XmlElement el = (XmlElement) n;
			if (string.Compare(el.GetAttribute(Helper.validConst), "false", StringComparison.OrdinalIgnoreCase) == 0)
				return false;

			if ((targetString = el.GetAttribute(Helper.targetConst)) == string.Empty)
				return false;

			baseString = el.GetAttribute(Helper.baseConst);

			return true;
		} 
	}
}
