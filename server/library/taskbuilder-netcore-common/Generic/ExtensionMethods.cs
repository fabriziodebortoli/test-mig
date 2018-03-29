using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microarea.Common.Generic
{
	//================================================================================
	public static class StringExtensions
	{
        /// <summary>
        /// rimuove tutte le occorrenze di un carattere
        /// </summary>
        //--------------------------------------------------------------------------------
        public static string Remove(this string str1, char[] anyOf, int start = 0, int searchLength = 0)
        {
            int idx = -1;
            do
            {
                idx = (searchLength > 0 ? str1.IndexOfAny(anyOf, start, searchLength) : str1.IndexOfAny(anyOf, start));
                if (idx > -1)
                {
                    str1 = str1.Remove(idx, 1);
                }
            } while (idx > -1);
            return str1;
        }

        /// <summary>
        /// rimuove tutte le occorrenze di un carattere
        /// </summary>
        public static string Remove(this string str1, char c, int start = 0, int searchLength = 0)
        {
            return str1.Remove(new char[] { c }, start, searchLength);
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Effettua un compare con StringComparison.InvariantCultureIgnoreCase
        /// </summary>
        public static bool CompareNoCase(this string str1, string str2)
		{
			return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
		}

        public static bool CompareNoCase(this string str1, string[] str2)
        {
            foreach (string s in str2)
            {
                if (string.Compare(str1, s, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }
            return false;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Effettua una Replace no case
        /// </summary>
        public static string ReplaceNoCase(this string inputString, string stringToReplace, string replaceString)
		{
			return Regex.Replace(
				inputString,
				Regex.Escape(stringToReplace),
				replaceString,
				RegexOptions.IgnoreCase
				);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// IsNullOrEmpty sulla variabile e non statica di string
		/// </summary>
		public static bool IsNullOrEmpty(this string str1)
		{
			return string.IsNullOrEmpty(str1);
		}

		/// <summary>
		/// IsNullOrWhiteSpace sulla variabile e non statica di string
		/// </summary>
		public static bool IsNullOrWhiteSpace(this string str1)
		{
			return string.IsNullOrWhiteSpace(str1);
		}

        public static bool IsJsonEmpty(this string s)
        {
            return string.IsNullOrWhiteSpace(s) || s == "{}" || s == "[]" || s == "{[]}" || s == "\"\"";
        }

        //--------------------------------------------------------------------------------
        public static int IndexOfNoCase(this string source, string toCheck, int startIndex = 0)
		{
			return source.IndexOf(toCheck, startIndex, StringComparison.OrdinalIgnoreCase);
		}

		public static bool ContainsNoCase(this string source, string toCheck, int startIndex = 0)
		{
			return source.IndexOfNoCase(toCheck, startIndex) >= 0;
		}

        //--------------------------------------------------------------------------------
        public static int FindEndBlock(this string s, char chBegin, char chEnd, int startIndex = 0)
        {
            if (s[startIndex] != chBegin)
                return -1;

            int openBlock = 1; int i = startIndex + 1;
            for (; i < s.Length && openBlock > 0; i++)
            {
                if (s[i] == chEnd)
                    openBlock--;
                else if (s[i] == chBegin)
                    openBlock++;
            }
            return i >= s.Length ? -1 : i;
        }

        //--------------------------------------------------------------------------------
        public static int IndexOfWord(this string source, string toCheck, bool noCase = true, int startIndex = 0, bool skipInnerRound = false)
        {
            while ((startIndex + toCheck.Length) <= source.Length)
            {
                int idx = noCase ? source.IndexOfNoCase(toCheck, startIndex) : source.IndexOf(toCheck, startIndex);
                if (idx < 0)
                    break;

                if (skipInnerRound)
                {
                    int pos = source.IndexOf('(', startIndex);
                    if (pos > -1 && pos < idx)
                    {
                        startIndex = source.FindEndBlock('(', ')', startIndex);
                        if (startIndex < 0 || startIndex >= source.Length)
                            break;
                        if (startIndex > idx)
                            continue;
                    }
                }

                startIndex = idx + toCheck.Length;

                if (idx > 0 && char.IsLetterOrDigit(source[idx - 1]))
                    continue;

                if (startIndex < source.Length && char.IsLetterOrDigit(source[startIndex]))
                    continue;

                return idx;
            }
            return -1;
        }

        public static int LastIndexOfWord(this string source, string word, bool noCase = true, int startIndex = -1, bool skipInnerRound = false)
        {
            if (startIndex <= 0)
                startIndex = source.Length - 1;

            startIndex = source.Length - startIndex - 1;

            string src = source.Reverse();

            int index = src.IndexOfWord(word.Reverse(), noCase, startIndex, skipInnerRound);
            if (index == -1)
                return -1;

            return  source.Length - index - word.Length;
        }

        public static string ReplaceQualifier(this string source, string newQ = "")
        {
            for (int pos = source.IndexOf('.'); pos > 0; pos = source.IndexOf('.'))
            {
                if (pos < (source.Length -1) && char.IsLetter(source[pos + 1]))
                {
                    int j = pos - 1;
                    for (; j >= 0 && (char.IsLetterOrDigit(source[j]) || source[j] == '_'); j--);
                    if (j < (pos - 1))
                    {
                        source = source.Remove(j +1 , pos - j );
                        if (!newQ.IsNullOrEmpty())
                            source = source.InsertSub(newQ, j);
                    }
                }
            }

            return source;
        }

        /// <summary>
        /// Implement  c++ & VB LEFT syntax checking parameters boundary avoiding exception.
        /// returns the left count character
        /// </summary>
        /// <param name="count">number of character. If more than original string size return original string</param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        public static string Left(this string o, int count)
		{
			if (count <= 0)
				return string.Empty;

			return o == null || count > o.Length ? o : o.Substring(0, count);
		}

        //---------------------------------------------------------------------
        /// <summary>
        /// Implement  c++ & VB LEFT syntax checking parameters boundary avoiding exception.
        /// returns the left count character with 3 suspension points at the end, indicating the truncation
        /// </summary>
        /// <param name="o"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string LeftWithSuspensionPoints(this string o, int count)
		{
			if(count <= 0)
				return string.Empty;

			return o == null || count > o.Length ? o : o.Substring(0, count-3) + "..." ;
		}


        //---------------------------------------------------------------------
        /// <summary>
        /// Implement  c++ & VB RIGHT syntax checking parameters boundary avoiding exception.
        /// returns the rightmost count character
        /// </summary>
        /// <param name="count">number of character. If more than original string size return original string</param>
        /// <returns></returns>
        public static string Right(this string o, int count)
		{
			if (count <= 0)
				return string.Empty;

			return count >= o.Length ? o : o.Substring(o.Length - count);
		}

        //---------------------------------------------------------------------
        /// <summary>
        /// Implement  c++ & VB MID syntax checking parameters boundary avoiding exception.
        /// returns the innermost count character starting from start
        /// </summary>
        /// <param name="start">where to start</param>
        /// <param name="count">number of characters</param>
        /// <returns></returns>
        public static string Mid(this string o, int start, int count = -1)
		{
			if (count == 0 || start >= o.Length)
				return string.Empty;

			if (start < 0)
				start = 0;

			int remaining = o.Length - start;

			if (count < 0 || count > remaining)
				count = remaining;

			return o.Substring(start, count);
		}

        //---------------------------------------------------------------------
        /// <summary>
        /// Replace the char at given index with an other
        /// </summary>
        /// <param name="start">index of char to replace </param>
        /// <param name="count">the new char</param>
        /// <returns></returns>
        public static string ReplaceAt(this string o, int index, char c)
		{
			if (index <= 0 || index >= o.Length)
				return string.Empty;

			//s[index] = c;
			string s = o.Left(index) + c + o.Mid(index + 1);
			return s;
		}

        //---------------------------------------------------------------------
        /// <summary>
        /// Replace occurrences from a start index 
        /// </summary>
        /// <param name="s">the old string </param>
        /// <param name="n">the new string</param>
        /// <param name="startIndex">the start index</param>
        /// <returns></returns>
        public static string Replace(this string o, string s, string n, int startIndex)
		{
			string a = o.Left(startIndex) + o.Mid(startIndex).Replace(s, n);
			return a;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Remove the blanks that are internal to square brackets
		/// </summary>
		public static string StripBlankNearSquareBrackets(this string o)
		{
			string s = o;
			s = s.Replace("[ ", "[");
			s = s.Replace(" ]", "]");
			return s;
		}

		/// <summary>
		/// Insert a substring at given index
		/// </summary>
		/// <param name="sub">the sub string to add</param>
		/// <param name="index">index of insertion point </param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string InsertSub(this string s, string sub, int index)
		{
			return s.Substring(0, index) + sub + s.Substring(index);
		}

        //---------------------------------------------------------------------
        public static string ConcatWithSep(this string s, string app, char sep)
        {
            if (app.IsNullOrEmpty()) return s;
            if (s.IsNullOrEmpty()) return app;
            return s.TrimEnd(new char[] { sep }) + sep + app.TrimStart(new char[] { sep });
        }

        /// <summary>
        /// Conta le occorrenze di un carattere in una stringa
        /// </summary>
        /// <param name="c">carattere</param>
        //-------------------------------------------------------------------------
        public static int CountChars(this string s, char c, int startIndex = 0, int endIndex = -1)
		{
            if (endIndex == -1) endIndex = s.Length - 1;
            int start = startIndex;
            int count = 0; 

            int newl = 0; 
			while (newl >= 0)
			{
				newl = s.IndexOf(c, start);
				if (newl < 0) break;
				count++;
				start = newl + 1;
                if (start > endIndex) break;
			}
			return count;
		}

		/// <summary>
		/// IndexOf with Occurrence managment
		/// </summary>
		/// <param name="subs"></param>
		/// <param name="occurence"></param>
		/// <param name="startIndex"></param>
		//-------------------------------------------------------------------------
		public static int IndexOfOccurrence(this string s, string subs, int occurence = 1, int startIndex = 0)
		{
			if (startIndex >= 0)
				while (occurence > 0 && startIndex < s.Length)
				{
					int newStart = s.IndexOf(subs, startIndex);
					if (newStart >= 0)
					{
						if (occurence == 1)
							return newStart;
					}
					else
						return -1;
					occurence--;
					startIndex = newStart + 1;
				}
			return -1;
		}

		/// <summary>
		/// LastIndexOf with Occurence
		/// </summary>
		//-------------------------------------------------------------------------
		public static int LastIndexOfOccurrence(this string s, string subs, int occurence = 1, int startIndex = -1)
		{
            if (startIndex == -1)
                startIndex = s.Length - 1;
            else if (startIndex == s.Length)
				startIndex--;
	
			if (startIndex < s.Length)
				while (occurence > 0 && startIndex >= 0)
				{
					int newStart = s.LastIndexOf(subs, startIndex);
					if (newStart >= 0)
					{
						if (occurence == 1)
							return newStart;
					}
					else
						return -1;
					occurence--;
					startIndex = newStart - 1;
				}
			return -1;
		}

        /// <summary>
        /// reverse the string
        /// </summary>
        /// <returns>the reversed string</returns>
        //-------------------------------------------------------------------------
        public static string Reverse(this string s)
        {
            if (s == null || s.Length == 0)
                return s;

            char[] c = s.ToCharArray();
            Array.Reverse(c);
            return new string(c);
        }

        /// <summary>
        /// LastIndexOf with Occurence
        /// </summary>
        //-------------------------------------------------------------------------
        public static string RemoveExtension(this string s, string ext)
        {
            int pos = s.LastIndexOf('.');
            if (pos == -1) return s;

            string e = s.Mid(pos);
            if (e.CompareNoCase(ext))
                return s.Left(pos);
            return s;
        }

        /// <summary>
        /// LastIndexOf with Occurence
        /// </summary>
        //-------------------------------------------------------------------------
        public static string RemovePrefix(this string s, string pref)
        {
            int pos = s.IndexOf('.');
            if (pos == -1) return s;

            string p = s.Left(pos + 1);
            if (p.CompareNoCase(pref))
                return s.Mid(pos + 1);
            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        //-------------------------------------------------------------------------
        public static string AddPrefix(this string s, string prefix1, string prefix2 = null)
        {
            string p = s.Left(prefix1.Length);
            if (p.CompareNoCase(prefix1))
                return s;
            if (prefix2 != null)
            {
                p = s.Left(prefix2.Length);
                if (p.CompareNoCase(prefix2))
                    return s;
            }
            return prefix1 + s;
        }

        //-------------------------------------------------------------------------
        public static string Replicate(this string rep, int count)
        {
	        string s = string.Empty;
	        for (int i = 0; i < count; i++)
		        s += rep;
            return s;
        }

        /// <summary>
        /// WildcardMatch
        /// </summary>
        //-------------------------------------------------------------------------
        public static string WildcardToRegex(this string pattern)
        {             
            string result= Regex.Escape(pattern).
                Replace(@"\*", ".+?").
                Replace(@"\?", "."); 

            if (result.EndsWith(".+?"))
            {
                result = result.Remove(result.Length - 3, 3);
                result += ".*";
            }

            return result;
        }

        /// <summary>
        /// WildcardMatch
        /// </summary>
        //-------------------------------------------------------------------------
        public static bool WildcardMatch(this string s, string pattern, bool bIgnoreCase = true)
        {
            if (s == null || s.Length == 0)
                return false;

            Regex regex = bIgnoreCase ?
                                new Regex(pattern.WildcardToRegex(), RegexOptions.IgnoreCase)
                                :
                                new Regex(pattern.WildcardToRegex());

            return regex.IsMatch(s);
        }

        //---------------------------------------------------------------------
        public static bool SqlLike(this string str, string pattern)
        {
            bool isMatch = true,
                isWildCardOn = false,
                isCharWildCardOn = false,
                isCharSetOn = false,
                isNotCharSetOn = false,
                endOfPattern = false;
            int lastWildCard = -1;
            int patternIndex = 0;
            List<char> set = new List<char>();
            char p = '\0';

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                endOfPattern = (patternIndex >= pattern.Length);
                if (!endOfPattern)
                {
                    p = pattern[patternIndex];

                    if (!isWildCardOn && p == '%')
                    {
                        lastWildCard = patternIndex;
                        isWildCardOn = true;
                        while (patternIndex < pattern.Length &&
                            pattern[patternIndex] == '%')
                        {
                            patternIndex++;
                        }
                        if (patternIndex >= pattern.Length) p = '\0';
                        else p = pattern[patternIndex];
                    }
                    else if (p == '_')
                    {
                        isCharWildCardOn = true;
                        patternIndex++;
                    }
                    else if (p == '[')
                    {
                        if (pattern[++patternIndex] == '^')
                        {
                            isNotCharSetOn = true;
                            patternIndex++;
                        }
                        else isCharSetOn = true;

                        set.Clear();
                        if (pattern[patternIndex + 1] == '-' && pattern[patternIndex + 3] == ']')
                        {
                            char start = char.ToUpper(pattern[patternIndex]);
                            patternIndex += 2;
                            char end = char.ToUpper(pattern[patternIndex]);
                            if (start <= end)
                            {
                                for (char ci = start; ci <= end; ci++)
                                {
                                    set.Add(ci);
                                }
                            }
                            patternIndex++;
                        }

                        while (patternIndex < pattern.Length &&
                            pattern[patternIndex] != ']')
                        {
                            set.Add(pattern[patternIndex]);
                            patternIndex++;
                        }
                        patternIndex++;
                    }
                }

                if (isWildCardOn)
                {
                    if (char.ToUpper(c) == char.ToUpper(p))
                    {
                        isWildCardOn = false;
                        patternIndex++;
                    }
                }
                else if (isCharWildCardOn)
                {
                    isCharWildCardOn = false;
                }
                else if (isCharSetOn || isNotCharSetOn)
                {
                    bool charMatch = (set.Contains(char.ToUpper(c)));
                    if ((isNotCharSetOn && charMatch) || (isCharSetOn && !charMatch))
                    {
                        if (lastWildCard >= 0) patternIndex = lastWildCard;
                        else
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    isNotCharSetOn = isCharSetOn = false;
                }
                else
                {
                    if (char.ToUpper(c) == char.ToUpper(p))
                    {
                        patternIndex++;
                    }
                    else
                    {
                        if (lastWildCard >= 0) patternIndex = lastWildCard;
                        else
                        {
                            isMatch = false;
                            break;
                        }
                    }
                }
            }
            endOfPattern = (patternIndex >= pattern.Length);

            if (isMatch && !endOfPattern)
            {
                bool isOnlyWildCards = true;
                for (int i = patternIndex; i < pattern.Length; i++)
                {
                    if (pattern[i] != '%')
                    {
                        isOnlyWildCards = false;
                        break;
                    }
                }
                if (isOnlyWildCards) endOfPattern = true;
            }
            return isMatch && endOfPattern;
        }

        //---------------------------------------------------------------------
        //Damerau-Levenshtein alghoritm
        public static int FuzzyDistance(this string src, string dest)
        {
            int[,] d = new int[src.Length + 1, dest.Length + 1];
            int i, j, cost;
            char[] str1 = src.ToCharArray();
            char[] str2 = dest.ToCharArray();

            for (i = 0; i <= str1.Length; i++)
            {
                d[i, 0] = i;
            }
            for (j = 0; j <= str2.Length; j++)
            {
                d[0, j] = j;
            }
            for (i = 1; i <= str1.Length; i++)
            {
                for (j = 1; j <= str2.Length; j++)
                {

                    if (str1[i - 1] == str2[j - 1])
                        cost = 0;
                    else
                        cost = 1;

                    d[i, j] =
                        Math.Min(
                            d[i - 1, j] + 1,              // Deletion
                            Math.Min(
                                d[i, j - 1] + 1,          // Insertion
                                d[i - 1, j - 1] + cost)); // Substitution

                    if ((i > 1) && (j > 1) && (str1[i - 1] ==
                        str2[j - 2]) && (str1[i - 2] == str2[j - 1]))
                    {
                        d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
                    }
                }
            }

            return d[str1.Length, str2.Length];
        }

        public static bool FuzzyCompare(this string src, string dest, double fuzzyness, bool noCase = false)
        {
            if (noCase)
            {
                return FuzzyCompare(src.ToLower(), dest.ToLower(), fuzzyness, false);
            }

            double distance = src.FuzzyDistance(dest);
            int length = Math.Max(src.Length, dest.Length);
            double score = 1.0 - distance / length;

            // Match?
            return (score > fuzzyness);
        }

        //---------------------------------------------------------------------
        // Converte un carattere hex nel valore
        private static char[] baseHex = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		static byte HexDigit(char i)
		{
			return (byte)((i >= '0' && i <= '9') ? (i - '0') : (i - 'A' + 10));
		}

		public static string EncodeBase16(this string s)
		{
			string encode = string.Empty;
			byte[] byteArray = Encoding.Unicode.GetBytes(s);
			string ss = byteArray.ToString();
			for (int i = 0; i < byteArray.Length; i++)
			{
				byte a = (byte)(byteArray[i] & 0xF);
				byte b = (byte)((byteArray[i] >> 4) & 0xF);
				encode += string.Format("{0}{1}", baseHex[b], baseHex[a]);
			}
			return encode;
		}

		//--------------------------------------------------------------------------------
		public static string DecodeBase16(this string s)
		{
			if ((s.Length % 4) != 0)
				return string.Empty;

			string decode = string.Empty;
			for (int i = 0; i < s.Length; )
			{
				byte a = (byte)(HexDigit(s[i++]));
				byte b = (byte)(HexDigit(s[i++]));
				byte c = (byte)(HexDigit(s[i++]));
				byte d = (byte)(HexDigit(s[i++]));
				decode += (char)(b + (a << 4) + (d << 8) + (c << 12));
			}
			return decode;
		}

		//---------------------------------------------------------------------
		/*
		 * Fields are separated by commas.
		 * (In locales where the comma is used as a decimal separator, 
		 * the semicolon is used instead as a delimiter).
		*/
		//--------------------------------------------------------------------------------
		public static string ToCSV(this string o, char sep = ',')
		{
			string s = o.Trim();
			
			if (s.IndexOfAny(new char[] { sep, '"', '\n', ',', ';', '\t' }) >= 0)
				s = '"' + s.Replace("\"", "\"\"") + '"';
		
			return s;
		}

		/// <summary>
		/// Verifica che la password passata soddisfi almeno 3 di questi vincoli:
		///		caratteri
		///		numeri
		///		maiuscole/minuscole
		///		caratteri speciali (.;-...)
		/// </summary>
		/// <param name="minPwdLenght">lunghezza minima della pwd</param>
		/// <returns>true se soddisfa i prerequisiti</returns>
		//-------------------------------------------------------------------------
		public static bool IsStrongPassword(this string password, int minPwdLenght)
		{
			if (password.Length < minPwdLenght)
				return false;

			bool hasDigit = false;
			bool hasLower = false;
			bool hasUpper = false;
			//il nome l'ho scelto io ed è bellissimo.
			bool hasWeirdThings = false;
			int conditionsMet = 0;

			for (int i = 0; i < password.Length; i++)
			{
				char character = password[i];
				if (char.IsDigit(character))
				{
					if (!hasDigit)
					{
						hasDigit = true;
						conditionsMet++;
					}

					continue;
				}

				if (char.IsLower(character))
				{
					if (!hasLower)
					{
						hasLower = true;
						conditionsMet++;
					}

					continue;
				}

				if (char.IsUpper(character))
				{
					if (!hasUpper)
					{
						hasUpper = true;
						conditionsMet++;
					}

					continue;
				}

				if (!hasWeirdThings)
				{
					hasWeirdThings = true;
					conditionsMet++;
				}
			}
			return conditionsMet >= 3;
		}

		// Get the excel column letter by index
		//---------------------------------------------------------------------
		public static string ExcelColumnLetter(int intCol)
		{
			int intFirstLetter = ((intCol) / 676) + 64;
			int intSecondLetter = ((intCol % 676) / 26) + 64;
			int intThirdLetter = (intCol % 26) + 65;

			char FirstLetter = (intFirstLetter > 64) ? (char)intFirstLetter : ' ';
			char SecondLetter = (intSecondLetter > 64) ? (char)intSecondLetter : ' ';
			char ThirdLetter = (char)intThirdLetter;

			return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
		}
    }
    //================================================================================
    public static class TbJson
    {
        //---------------------------------------------------------------------
        /*
		 * Custom ToJson
		*/
        //--------------------------------------------------------------------------------
        public static string ToJson(this string o, string name = null, bool bracket = false, bool escape = false, bool quoted = true)
        {
            string s = o != null ? o.Trim() : string.Empty;

            if (escape)
            {
                s = s.Replace("\\", "\\\\");
                s = s.Replace("/", "\\/");

                s = s.Replace("\t", "\\t");
                s = s.Replace("\r", "\\r");
                s = s.Replace("\n", "\\n");

                s = s.Replace("\"", "\\\"");
            }

            if (quoted)
                s = '"' + s + '"';

            if (!name.IsNullOrEmpty())
                s = '"' + name + '"' + ':' + s;

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        public static string ToJson(this Guid g, string name = null, bool bracket = false)
        {
            return g.ToString().ToJson(name, bracket, false, true);
        }

        public static string ToJson(this bool b, string name = null, bool bracket = false)
        {
            return b.ToString(System.Globalization.CultureInfo.InvariantCulture).ToLower().ToJson(name, bracket, false, false);
        }
        public static string ToJson(this char c, string name = null, bool bracket = false, bool escape = false)
        {
            return c.ToString().ToJson(name, bracket, escape);
        }
        public static string ToJson(this int n, string name = null, bool bracket = false)
        {
            return n.ToString().ToJson(name, bracket, false, false);
        }
        public static string ToJson(this uint n, string name = null, bool bracket = false)
        {
            return n.ToString().ToJson(name, bracket, false, false);
        }
        public static string ToJson(this short n, string name = null, bool bracket = false)
        {
            return n.ToString().ToJson(name, bracket, false, false);
        }
        public static string ToJson(this ushort n, string name = null, bool bracket = false)
        {
            return n.ToString().ToJson(name, bracket, false, false);
        }
        public static string ToJson(this ushort n, string prefix, string name, bool bracket = false, string postfix = "")
        {
            return (prefix + n.ToString() + postfix).ToJson(name, bracket, false, true);
        }
        public static string ToJson(this long n, string name = null, bool bracket = false)
        {
            return n.ToString().ToJson(name, bracket, false, false);
        }
        public static string ToJson(this ulong n, string name = null, bool bracket = false)
        {
            return n.ToString().ToJson(name, bracket, false, false);
        }
        public static string ToJson(this byte n, string name = null, bool bracket = false)
        {
            return n.ToString().ToJson(name, bracket, false, false);
        }

        public static string ToJson(this double d, string name = null, bool bracket = false)
        {
            return d.ToString("F", System.Globalization.CultureInfo.InvariantCulture).ToJson(name, bracket, false, false);
        }

        public static string ToJson(this DateTime d, string name = null, bool bracket = false)
        {
            //       o: 2008-06-15T21:15:07.0000000   see https://msdn.microsoft.com/it-it/library/zdtaw1bw(v=vs.110).aspx
            return d.ToString("o").ToJson(name, bracket, false, true);
        }

        //----------------------------------------------------------------------------------
        public static string ToHtml_px(this int n, string name = null, bool bracket = false)
        {
            return n.ToJson(name, bracket);
        }

        public static string ToHtml_align(this AlignType a, bool bracket = false)
        {
            string textAlign = "left";
            if ((a & AlignType.DT_RIGHT) == AlignType.DT_RIGHT)
                textAlign = "right";
            else if ((a & AlignType.DT_CENTER) == AlignType.DT_CENTER)
                textAlign = "center";

            string verticalAlign = "top";
            if ((a & AlignType.DT_BOTTOM) == AlignType.DT_BOTTOM)
                verticalAlign = "bottom";
            else if ((a & AlignType.DT_VCENTER) == AlignType.DT_VCENTER)
                verticalAlign = "middle";

            string rotateBy = "0";
            if ((a & AlignType.DT_EX_90) == AlignType.DT_EX_90)
                rotateBy = "270";
            if ((a & AlignType.DT_EX_270) == AlignType.DT_EX_270)
                rotateBy = "90";

            string s = textAlign.ToJson("text_align") + ',' + verticalAlign.ToJson("vertical_align");

            if (rotateBy != "0")
                s = s + ',' + rotateBy.ToJson("rotateBy");

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        public static string ToJson(this Rectangle rect, string name = "rect", bool bracket = false)
        {
            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '"' + name + "\":";

            s += '{' +
                    rect.Left.ToHtml_px("left") + ',' +
                    rect.Right.ToHtml_px("right") + ',' +
                    rect.Top.ToHtml_px("top") + ',' +
                    rect.Bottom.ToHtml_px("bottom") +
                 '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        public static string ToHtml(this Color color)
        {
            return '#' + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        public static string ToJson(this Color color, string name = null, bool bracket = false)
        {
            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '"' + name + "\":";

            s += color.ToHtml().ToJson();

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        public static string ToJsonRGB(this Color color, string name = null, bool bracket = false)
        {
            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '"' + name + "\":";

            s += '{' +
                    //color.ToArgb().ToJson("color") +
                    color.R.ToJson("r") + ',' +
                    color.G.ToJson("g") + ',' +
                    color.B.ToJson("b") +
                 '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        public static string ToJson(this object o, string name = null, bool bracket = false)
        {
            try
            {
                if (o is string)
                {
                    string s = o as string;
                    return s.ToJson(name, bracket, true);
                }
                if (o is double)
                {
                    double d = (double)o;
                    return d.ToJson(name, bracket);
                }
                if (o is DateTime)
                {
                    DateTime dt = (DateTime)o;
                    return dt.ToJson(name, bracket);
                }
                if (o is int || o is Int32)
                {
                    int i = (int)o;
                    return i.ToJson(name, bracket);
                }
                if (o is Boolean)
                {
                    Boolean b = (Boolean)o;
                    return b.ToJson(name, bracket);
                }
                if (o is DataEnum)
                {
                    DataEnum de = (DataEnum)o;
                    uint ui = (uint)de;
                    return ui.ToJson(name, bracket);
                }
                if (o is uint || o is UInt32)
                {
                    uint i = (uint)o;
                    return i.ToJson(name, bracket);
                }
                if (o is ushort || o is UInt16)
                {
                    ushort i = (ushort)o;
                    return i.ToJson(name, bracket);
                }
                if (o is long || o is Int64)
                {
                    long i = (long)o;
                    return i.ToJson(name, bracket);
                }
                if (o is short || o is Int16)
                {
                    long i = (short)o;
                    return i.ToJson(name, bracket);
                }
                if (o is ulong || o is UInt64)
                {
                    ulong i = (ulong)o;
                    return i.ToJson(name, bracket);
                }
                if (o is Guid)
                {
                    Guid g = (Guid)o;
                    return g.ToJson(name, bracket);
                }
                if (o is Enum)
                {
                    //Enum e = (Enum)o;
                    int i = (int)o;
                    return i.ToJson(name, bracket);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
            Debug.Fail("ToJson(object..." + o.GetType().ToString());

            return o.ToString().ToJson(name, bracket, true);
        }
    }
    
    //================================================================================
    public static class ListStringExtensions
	{
		/// <summary>
		/// Effettua una contains No case
		/// </summary>
		//--------------------------------------------------------------------------------
		public static bool ContainsNoCase(this List<string> str1, String stringToSearch)
		{
			foreach (string item in str1)
			{
				if (item.CompareNoCase(stringToSearch))
					return true;
			}
			return false;
		}

		//--------------------------------------------------------------------------------
		public static bool ContainsNoCase(this StringCollection str1, String stringToSearch)
		{
			foreach (string item in str1)
			{
				if (item.CompareNoCase(stringToSearch))
					return true;
			}
			return false;
		}
	}

    //================================================================================
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Easter Sunday of dt year
        /// </summary>
        //--------------------------------------------------------------------------------
        public static DateTime EasterSunday(this DateTime dt)
        {
            int year = dt.Year;
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(day, month, year);
        }

        /// <summary>
        /// WeekStartDate
        /// </summary>
        //--------------------------------------------------------------------------------
        public static DateTime WeekStartDate(this DateTime dt, int week)
        {
            int dayofw = (int) dt.DayOfWeek;

            if (dayofw > 3)
                dt.AddDays(7 - dayofw);
            else
                dt.AddDays(- dayofw);

            dt.AddDays((week - 1) * 7);
 
            return dt;
         }
    }

    //================================================================================
    public static class StackExtensions
    {
        /// <summary>
        /// Clone of Stack<T>
        /// </summary>
        //--------------------------------------------------------------------------------
        public static Stack<T> Clone<T>(this Stack<T> stack)
        {
            return new Stack<T>(new Stack<T>(stack));
        }
    }

    //================================================================================
    public static class Extensions
    {
        /// <summary>
        /// Clone of List<T>
        /// </summary>
        //--------------------------------------------------------------------------------
        public static List<T> Clone<T>(this List<T> listToClone)
        {
            return new List<T>(listToClone);
        }
    }
    
}