using System.Collections;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microarea.Tools.TBLocalizer
{
	public class DictionaryDocumentFunctions
	{
		public const string punctuationPattern = "[^a-zA-Z_0-9\\s]";
				
		//---------------------------------------------------------------------
		public static void OrderStringNodes(XmlElement[] stringNodes, DictionaryDocument.StringComparisonFlags[] comparisonTypes)
		{
			for (int i = 0; i < stringNodes.Length; i ++)
			{
				for (int j = i + 1; j < stringNodes.Length; j++)
				{
					if (comparisonTypes[i] < comparisonTypes[j])
						continue;
					
					if (comparisonTypes[i] == comparisonTypes[j])
					{
						string iMatch = stringNodes[i].GetAttribute(AllStrings.matchType);
						if (iMatch.Length == 0) iMatch = "0";
						string jMatch = stringNodes[j].GetAttribute(AllStrings.matchType);
						if (jMatch.Length == 0) jMatch = "0";
						
						if (int.Parse(iMatch) < int.Parse(jMatch))
							continue;
					}

					DictionaryDocument.StringComparisonFlags tmp = comparisonTypes[i];
					comparisonTypes[i] = comparisonTypes[j];
					comparisonTypes[j] = tmp;

					XmlElement tmpEl = stringNodes[i];
					stringNodes[i] = stringNodes[j];
					stringNodes[j] = tmpEl;
				}
			}
		}

		//---------------------------------------------------------------------
		public static bool MatchString(
			string aString,
			string baseString,
			DictionaryDocument.StringComparisonFlags allowedComparisons, 
			out DictionaryDocument.StringComparisonFlags actualComparisonCase
			)
		{
			return MatchString(aString, baseString, allowedComparisons, out actualComparisonCase, new ArrayList());
		}

		//---------------------------------------------------------------------
		public static bool MatchString(
			string aString,
			string baseString,
			DictionaryDocument.StringComparisonFlags allowedComparisons, 
			out DictionaryDocument.StringComparisonFlags actualComparisonCase,
			ArrayList doneMatches
			)
		{
			
			actualComparisonCase = DictionaryDocument.StringComparisonFlags.PERFECT_MATCH;
			if (doneMatches.Contains(allowedComparisons))
				return false; //this kind of comparison has already been done (test to improve performance)
			
			doneMatches.Add(allowedComparisons);
			
			//perfect match
			if (aString == baseString) return true; 
				
			
			switch (allowedComparisons)
			{
				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE:
				{
					actualComparisonCase = allowedComparisons;
					return string.Compare(aString, baseString, true) == 0;
				}				
				case DictionaryDocument.StringComparisonFlags.IGNORE_SPACES:
				{
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, "\\s", ""), Regex.Replace(baseString, "\\s", ""), false) == 0;
				}
				case DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, punctuationPattern, ""), Regex.Replace(baseString, punctuationPattern, ""), false) == 0;
				}

				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_SPACES:
				{
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_CASE, out actualComparisonCase, doneMatches))
						return true;
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_SPACES, out actualComparisonCase, doneMatches))
						return true;
					
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, "\\s", ""), Regex.Replace(baseString, "\\s", ""), true) == 0;
				}
				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_CASE, out actualComparisonCase, doneMatches))
						return true;
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION, out actualComparisonCase, doneMatches))
						return true;
					
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, punctuationPattern, ""), Regex.Replace(baseString, punctuationPattern, ""), true) == 0;
				}	
				case DictionaryDocument.StringComparisonFlags.IGNORE_SPACES | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_SPACES, out actualComparisonCase, doneMatches))
						return true;
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION, out actualComparisonCase, doneMatches))
						return true;
					
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, "\\W", ""), Regex.Replace(baseString, "\\W", ""), false) == 0;
				}	


				case DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_SPACES | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION:
				{
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_SPACES, out actualComparisonCase, doneMatches))
						return true;
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_CASE | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION, out actualComparisonCase, doneMatches))
						return true;
					if (MatchString(aString, baseString, DictionaryDocument.StringComparisonFlags.IGNORE_SPACES | DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION, out actualComparisonCase, doneMatches))
						return true;
					
					actualComparisonCase = allowedComparisons;
					return string.Compare(Regex.Replace(aString, "\\W", ""), Regex.Replace(baseString, "\\W", ""), true) == 0;
				}				
			}

			return false;
		}
	}
}
