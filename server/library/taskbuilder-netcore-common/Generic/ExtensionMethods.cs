using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace Microarea.Common.Generic
{
	//================================================================================
	public static class StringExtensions
	{
		/// <summary>
		/// Effettua un compare con StringComparison.InvariantCultureIgnoreCase
		/// </summary>
		//--------------------------------------------------------------------------------
		public static bool CompareNoCase(this String str1, String str2)
		{
			return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
		}

		/// <summary>
		/// Effettua una Replace no case
		/// </summary>
		//--------------------------------------------------------------------------------
		public static string ReplaceNoCase(this string inputString, string stringToReplace, string replaceString)
		{
			return Regex.Replace(
				inputString,
				Regex.Escape(stringToReplace),
				replaceString,
				RegexOptions.IgnoreCase
				);
		}

		/// <summary>
		/// IsNullOrEmpty sulla variabile e non statica di string
		/// </summary>
		//--------------------------------------------------------------------------------
		public static bool IsNullOrEmpty(this string str1)
		{
			return string.IsNullOrEmpty(str1);
		}

		/// <summary>
		/// IsNullOrWhiteSpace sulla variabile e non statica di string
		/// </summary>
		//--------------------------------------------------------------------------------
		public static bool IsNullOrWhiteSpace(this string str1)
		{
			return string.IsNullOrWhiteSpace(str1);
		}

		//--------------------------------------------------------------------------------
		public static int IndexOfNoCase(this string source, string toCheck)
		{
			return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase);
		}

		//--------------------------------------------------------------------------------
		public static bool ContainsNoCase(this string source, string toCheck)
		{
			return source.IndexOfNoCase(toCheck) >= 0;
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


		/// <summary>
		/// Implement  c++ & VB RIGHT syntax checking parameters boundary avoiding exception.
		/// returns the rightmost count character
		/// </summary>
		/// <param name="count">number of character. If more than original string size return original string</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string Right(this string o, int count)
		{
			if (count <= 0)
				return string.Empty;

			return count >= o.Length ? o : o.Substring(o.Length - count);
		}

		/// <summary>
		/// Implement  c++ & VB MID syntax checking parameters boundary avoiding exception.
		/// returns the innermost count character starting from start
		/// </summary>
		/// <param name="start">where to start</param>
		/// <param name="count">number of characters</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
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

		/// <summary>
		/// Replace the char at given index with an other
		/// </summary>
		/// <param name="start">index of char to replace </param>
		/// <param name="count">the new char</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string ReplaceAt(this string o, int index, char c)
		{
			if (index <= 0 || index >= o.Length)
				return string.Empty;

			//s[index] = c;
			string s = o.Left(index) + c + o.Mid(index + 1);
			return s;
		}

		/// <summary>
		/// Replace occurrences from a start index 
		/// </summary>
		/// <param name="s">the old string </param>
		/// <param name="n">the new string</param>
		/// <param name="startIndex">the start index</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string Replace(this string o, string s, string n, int startIndex)
		{
			string a = o.Left(startIndex) + o.Mid(startIndex).Replace(s, n);
			return a;
		}

		/// <summary>
		/// Remove the blanks that are internal to square brackets
		/// </summary>
		//---------------------------------------------------------------------
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
		/// <summary>
		/// Conta le occorrenze di un carattere in una stringa
		/// </summary>
		/// <param name="c">carattere</param>
		//-------------------------------------------------------------------------
		public static int CountChar(this string s, char c)
		{
			int start = 0; int newl = 0;
			int nl = 0;
			while (newl >= 0)
			{
				newl = s.IndexOf(c, start);
				if (newl < 0) return nl;
				nl++;
				start = newl + 1;
			}
			return nl;
		}

		/// <summary>
		/// IndexOf with Occurrence managment
		/// </summary>
		/// <param name="subs"></param>
		/// <param name="occurence"></param>
		/// <param name="startIndex"></param>
		//-------------------------------------------------------------------------
		public static int IndexOfOccurrence(this string s, string subs, int occurence, int startIndex)
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
		public static int LastIndexOfOccurrence(this string s, string subs, int occurence, int startIndex)
		{
			if (startIndex == s.Length)
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

		//---------------------------------------------------------------------
		// Converte un carattere hex nel valore
		private static char[] baseHex = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		//--------------------------------------------------------------------------------
		static byte HexDigit(char i)
		{
			return (byte)((i >= '0' && i <= '9') ? (i - '0') : (i - 'A' + 10));
		}

		//--------------------------------------------------------------------------------
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

        //---------------------------------------------------------------------
        /*
		 * Fields are separated by commas.
		 * (In locales where the comma is used as a decimal separator, 
		 * the semicolon is used instead as a delimiter).
		*/
        //--------------------------------------------------------------------------------
        public static string ToJson(this string o)
        {
            string s = o.Trim();

           // if (s.IndexOfAny(new char[] { sep, '"', '\n', ',', ';', '\t' }) >= 0)

            s = s.Replace("\\", "\\\\");
            s = s.Replace("/", "\\/");
 
            s = s.Replace("\t", "\\t");
            s = s.Replace("\r", "\\r");
            s = s.Replace("\n", "\\n");

            s = s.Replace("\"", "\\\"");

            s = '"' + s + '"';

            return s;
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
}