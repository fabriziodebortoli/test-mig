using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	//================================================================================
	public class RegexHelper
	{
		#region Regular expression tokens
		
		public const string backSlash = @"\\";
		public const string allButBackSlash = "[^\\\\]";
		public const string invertedCommas = "\"";
		public const string singleQuote = "'";
		
		public const string allButInvertedCommas = "[^\"]";
		public const string allButCloseRound = @"[^\)]";
		public const string allButSemicolon = @"[^;]";
		public const string allButSingleQuote = "[^']";

		public const string zeroOrMoreSpaces = @"\s*";
		public const string oneOrMoreSpaces = @"\s+";
		public const string nonSpaceCharacter = @"\S";
		public const string singleWord = @"\w+";
		public const string spaceDelimitedWord = @"\S+";
		public const string orConjunction = "|";
		public const string openRound = @"\(";
		public const string closeRound = @"\)";
		public const string anyCharacter = @"[\w\W]"; //\w for words, \W non word character like punctation, digits, etc
		public const string semiColon = @";";
		public const string anyDigit = @"\d";
		public const string oneOrMoreDigits = @"\d+";

		
		#endregion

		#region Regular expression composing methods

		//--------------------------------------------------------------------------------
		public static string AlternativeTokens(params string[] tokens)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("(");
			foreach (string t in tokens)
			{
				if (t != tokens[0])
					sb.Append(orConjunction);
				
				sb.Append("(");
				sb.Append(t);
				sb.Append(")");
			}
			sb.Append(")");
			return sb.ToString();
		}

		//--------------------------------------------------------------------------------
		public static string NotPrecededBy(string prefix, string s)
		{
			return string.Format("(?<!{0}){1}", prefix, s);
		}

		//--------------------------------------------------------------------------------
		public static string NotFollowedBy(string suffix, string s)
		{
			return string.Format("{0}(?!{1})", s, suffix);
		}

		//--------------------------------------------------------------------------------
		public static string FollowedBy(string suffix, string s)
		{
			return string.Format("{0}(?={1})", s, suffix);
		}

		//--------------------------------------------------------------------------------
		public static string ZeroOrMoreTimes(string s, bool capture)
		{
			return (capture ? "(" : "(?:") + s + ")*";
		}

		//--------------------------------------------------------------------------------
		public static string UpToNumberOfTimes(string s, bool capture, int times)
		{
			return (capture ? "(" : "(?:") + s + "){0," + times.ToString() + "}";
		}

		//--------------------------------------------------------------------------------
		public static string ZeroOrOneTimes(string s, bool capture)
		{
			return (capture ? "(" : "(?:") + s + ")?";
		}

		//--------------------------------------------------------------------------------
		public static string OneOrMoreTimes(string s, bool capture)
		{
			return (capture ? "(" : "(?:") + s + ")+";
		}
			
		//--------------------------------------------------------------------------------
		public static string Group(string s)
		{
			return Group (s, null);
		}	

		//--------------------------------------------------------------------------------
		public static string AtStartOfLine(string s)
		{
			return "^" + s;
		}

		//--------------------------------------------------------------------------------
		public static string Group(string s, string name)
		{
			if (name == null)
				return string.Format("({0})", s);
			else
				return string.Format("(?<{0}>{1})", name, s);
		}
		#endregion
	
		#region Regular expression common constructs	
		public const string stringGroupName = "string";
		public static readonly string stringPattern =		// = "(?<string>\"((\\\\\\\\)|([^\"])|([^\\\\]\\\\\"))*\")";
			Group
			(
			invertedCommas + 
			ZeroOrMoreTimes 
			( 
			AlternativeTokens
			(
			invertedCommas + invertedCommas,
			backSlash + backSlash, 
			allButInvertedCommas, 
			allButBackSlash + backSlash + invertedCommas
			), false ) + 
			invertedCommas,
			stringGroupName
			);
		#endregion
	}

	/// <summary>
	/// This class encapsulates logic for replacing text in a file using regular expressions;
	/// To use this class, create your own class inheriting from it and implement your own 
	/// regular expression overriding the Init method; then implement your translation logic
	/// overriding the Translate method 
	/// </summary>
	//================================================================================
	public abstract class RegexTextReplacer : RegexHelper
	{
		private bool initialized = false;
 
		private string fileName;
		protected bool modified = false;
		protected Regex regularExpression = null;
		protected Encoding outputEncoding;

		//--------------------------------------------------------------------------------
		public string FileName { get { return fileName; } set { fileName = value; } }
		
		public event EventHandler StartReplacing;
		public event EventHandler EndReplacing;

		//--------------------------------------------------------------------------------
		protected RegexTextReplacer(string fileName)
		{
			this.fileName = fileName;
		}

		//--------------------------------------------------------------------------------
		public void Replace()
		{
			if (!initialized)
			{
				Init();
				initialized = true;
			}
			
			StreamReader sr = new StreamReader(fileName, Encoding.GetEncoding(0), true);
			string s = sr.ReadToEnd();
			Encoding encoding = outputEncoding == null ? sr.CurrentEncoding : outputEncoding;
			sr.Close();

			if (regularExpression == null)
				throw new NullReferenceException(GenericStrings.RegexNotInited);
			
			s = regularExpression.Replace(s, new MatchEvaluator(InternalTranslate) );

			if (!modified) return;

			if (StartReplacing != null) StartReplacing(this, EventArgs.Empty);
			
			StreamWriter sw = new StreamWriter(fileName, false, encoding);
			sw.Write(s);
			sw.Close();

			if (EndReplacing != null) EndReplacing(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		private string InternalTranslate(Match m)
		{
			return Translate(m);
		}

		//--------------------------------------------------------------------------------
		protected abstract string Translate (Match m);

		//--------------------------------------------------------------------------------
		protected abstract void Init();
		
	}
}
