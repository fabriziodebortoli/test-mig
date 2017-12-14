using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;


namespace Microarea.TaskBuilderNet.Core.Lexan
{
	/// <summary>
	/// Summary description for Unparser.
	/// </summary>
	//==================================================================================
	public class Unparser : IDisposable
	{
		private int tabCounter = 0;
		private int DEFAULT_PEN_WIDTH = 1;

		private StreamWriter output = null;
		private StringWriter outstring = null;
		private bool fromFile = true;
		private bool mustTab = true;

		private bool saveAsWithCurrentLanguage;

		private string filename;
		FileStream fs = null;
		
		public int TabCounter { get { return tabCounter; } }
		public bool SaveAsWithCurrentLanguage { get { return saveAsWithCurrentLanguage; } set { saveAsWithCurrentLanguage = value; } }

		//------------------------------------------------------------------------------
		public Unparser()
		{ }

		//------------------------------------------------------------------------------
		void IDisposable.Dispose()
		{
			if (output != null)
				output.Close();
			output = null;

			if (outstring != null)
				outstring.Close();
			outstring = null;

			if (fs != null)
				fs.Close();
		}

		//------------------------------------------------------------------------------
		public bool Open(string afileName)
		{
			Debug.Assert(output == null);
			Debug.Assert(outstring == null);

			fromFile = true;
			filename = afileName;

			try
			{
				fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				output = new StreamWriter(fs);
			}
			catch { return false; }

			return true;
		}

		//------------------------------------------------------------------------------
		public bool Open()
		{
			Debug.Assert(output == null);
			Debug.Assert(outstring == null);
			fromFile = false;

			try
			{
				outstring = new StringWriter();
			}
			catch { return false; }

			return true;
		}

		//------------------------------------------------------------------------------
		public void Close()
		{
			if (output != null) output.Close();
			if (outstring != null) outstring.Close();

			output = null;
			outstring = null;

			// Ho rilasciato le cose pesanti e quindi posso inibire il finalize
			GC.SuppressFinalize(this);
		}

		//------------------------------------------------------------------------------
		~Unparser() { Close(); }

		//------------------------------------------------------------------------------
		public string GetResultString()
		{
			if (fromFile)
			{
				if (output != null)
					return LexanStrings.NoCloseError;

				StreamReader rdr = new StreamReader(filename, System.Text.Encoding.GetEncoding(0));
				string str = rdr.ReadToEnd();
				rdr.Close();
				return str;
			}

			if (outstring == null) return LexanStrings.AlreadyCloseError;
			return outstring.ToString();
		}

		//------------------------------------------------------------------------------
		internal void BaseWrite(string aString)
		{
			if (fromFile)
				output.Write(aString);
			else
				outstring.Write(aString);
		}

		//------------------------------------------------------------------------------
		internal void BaseWriteLine(string aString)
		{
			if (fromFile)
				output.WriteLine(aString);
			else
				outstring.WriteLine(aString);
		}

		//------------------------------------------------------------------------------
		internal void BaseWriteLine()
		{
			if (fromFile)
				output.WriteLine();
			else
				outstring.WriteLine();
		}

		//------------------------------------------------------------------------------
		public void WriteLine(string aString)
		{
			BaseWriteLine(aString);
			mustTab = true;
		}

		//------------------------------------------------------------------------------
		public void WriteLine()
		{
			BaseWriteLine();
			mustTab = true;
		}

		//------------------------------------------------------------------------------
		public void Write(string aString)
		{
			if (string.IsNullOrEmpty(aString))
				return;

			// first column tabulation
			if (mustTab && !string.IsNullOrEmpty(aString))
			{
				string str = Language.GetTokenString(Token.TAB);
				for (int i = 0; i < TabCounter; i++)
					BaseWrite(str);

				// inibisce la tabulazione fino al newline
				mustTab = false;
			}
		
			//cerco il cr nella stringa, se ce ne sono stampo il left della stringa (prima riga)
			//e chiamo ricorsivamente il write sul resto della string
			int index = aString.IndexOf(Environment.NewLine);
			if (index < 0 || index + 1 >= aString.Length)
			{
				BaseWrite(aString);
				return;
			}
			
			string firstLine = aString.Left(index);
			string restOfLine = aString.Substring(index + Environment.NewLine.Length);

			BaseWrite(firstLine);
			WriteLine();

			mustTab = true;
			Write(restOfLine);
		}

		//------------------------------------------------------------------------------
		public void IncTab()
		{ 
			tabCounter++;
		}
		
		//------------------------------------------------------------------------------
		public void DecTab()
		{
			if (tabCounter > 0)
				tabCounter--;
		}

		//------------------------------------------------------------------------------
		public void WriteTab(bool newLine = true)
		{
			Write("\t");
			if (newLine)
				WriteLine();
		}

		//------------------------------------------------------------------------------
		public void WriteExpr(string expression, bool newLine = true)
		{
			Write(expression);

			// to manage the expression string with audited new-line at the end of string,
			// and to do the correct user defined tabulation
			if (expression[expression.Length - 1] == '\n' && newLine)
				mustTab = true;		// new-line already done by write
			else
				WriteBlank();
		}

		//------------------------------------------------------------------------------
		public void WriteTag(Token aToken, bool newLine = true) { Write(Language.GetTokenString(aToken)); WriteBlank(); if (newLine) WriteLine(); }
		public void WriteID(string aString, bool newLine = true) { Write(aString); WriteBlank(); if (newLine) WriteLine(); }

		//------------------------------------------------------------------------------
		public void Write(bool aBool, bool newLine = true) { Write(aBool.ToString()); if (newLine) WriteLine(); }
		public void Write(byte aByte, bool newLine = true) { Write(aByte.ToString()); if (newLine) WriteLine(); }
		public void Write(short aWord, bool newLine = true) { Write(aWord.ToString()); if (newLine) WriteLine(); }
		public void Write(int aInt, bool newLine = true) { Write(aInt.ToString()); if (newLine) WriteLine(); }
		public void Write(long aLong, bool newLine = true) { Write(aLong.ToString()); if (newLine) WriteLine(); }
		public void Write(float aFloat, bool newLine = true) { Write(aFloat.ToString()); if (newLine) WriteLine(); }
		public void Write(double aDouble, bool newLine = true) { Write(aDouble.ToString()); if (newLine) WriteLine(); }
		public void WriteString(string aString, bool newLine = true) { Write("\"" + aString + "\" ");  if (newLine) WriteLine(); }
		public void WriteSqlString(string aString, bool newLine = true) { Write("\'" + aString + "\' "); if (newLine) WriteLine(); }

		//------------------------------------------------------------------------------
		public void WriteBegin(bool newLine = true) { WriteTag(Token.BEGIN, newLine);}
		public void WriteEnd(bool newLine = true) { WriteTag(Token.END, newLine); }
		public void WriteBlank() { BaseWrite(" "); }
		public void WriteComma(bool newLine = true) { BaseWrite(Language.GetTokenString(Token.COMMA)); if (newLine) WriteLine(); }
		public void WriteColon(bool newLine = true) { BaseWrite(Language.GetTokenString(Token.COLON)); if (newLine) WriteLine(); }
		public void WriteSep(bool newLine = true) { BaseWrite(Language.GetTokenString(Token.SEP)); if (newLine) WriteLine(); }
		public void WriteSquareOpen(bool newLine = true) { BaseWrite(Language.GetTokenString(Token.SQUAREOPEN)); if (newLine) WriteLine(); }
		public void WriteSquareClose(bool newLine = true) { BaseWrite(Language.GetTokenString(Token.SQUARECLOSE)); WriteBlank(); if (newLine) WriteLine(); }
		public void WriteOpen(bool newLine = true) { BaseWrite(Language.GetTokenString(Token.ROUNDOPEN)); if (newLine) WriteLine(); }
		public void WriteClose(bool newLine = true) { BaseWrite(Language.GetTokenString(Token.ROUNDCLOSE)); if (newLine) WriteLine(); }

		//------------------------------------------------------------------------------
		public void WriteAlias(ushort id, bool newLine = false)
		{
			WriteTag(Token.ALIAS, false);
			Write(id, newLine);
			WriteBlank();
		}

		//------------------------------------------------------------------------------
		public void WriteItem(string itemName)
		{
			WriteOpen();
			Write(itemName);
			WriteClose();
		}

		//------------------------------------------------------------------------------
		public void WriteSubscr(int aVal, bool newLine = true)
		{
			WriteSquareOpen();
			Write(aVal);
			WriteSquareClose();

			if (newLine)
				WriteLine();
		}

		//------------------------------------------------------------------------------
		public void WriteSubscr(int aVal1, int aVal2, bool newLine = true)
		{
			WriteSquareOpen(false);
			Write(aVal1, false);
			if (aVal2 != 0)
			{
				WriteComma(false);
				Write(aVal2, false);
			}
			WriteSquareClose(false);

			if (newLine)
				WriteLine();
		}

		//------------------------------------------------------------------------------
		public void WriteGUID(Guid aGuid)
		{
			WriteTag(Token.UUID);
			WriteString(aGuid.ToString());
		}

		//------------------------------------------------------------------------------
		public void WriteColor(Token token, Color aColor, bool newLine = true)
		{
			WriteTag(token, false);
			WriteOpen(false);
			Write(aColor.R, false);
			WriteComma(false);
			Write(aColor.G, false);
			WriteComma(false);
			Write(aColor.B, false);
			WriteClose(false);

			if (newLine)
				WriteLine();
		}

		//------------------------------------------------------------------------------
		public void WriteAlign (int align, bool newline = true)
		{
			WriteTag(Token.ALIGN, false);
			Write(align, false);
			WriteBlank();
			WriteSep(newline);
		}

		//------------------------------------------------------------------------------
		public void WriteWidth(int width, bool newLine = true)
		{
			WriteTag(Token.WIDTH, false);
			Write(width, false);
			WriteBlank();
			
			if (newLine) 
				WriteLine();
		}

		//------------------------------------------------------------------------------
		public void UnparseSize(int size, bool newLine = true)
		{
			WriteTag(Token.SIZE, false);
			Write(size, false);
			WriteBlank();
			
			if (newLine)
				WriteLine();
		}

		//------------------------------------------------------------------------------
		public void WriteCEdit(string text, bool newLine, bool tDTStyle = false)
		{
			if (text.IsNullOrEmpty())
			{
				WriteString(text, newLine);
				return;
			}

			string newText = text;
			newText = newText.Replace("\r", "");

			int nStart = 0;
			int nEnd = 0;

			bool multiLine = false;

			for (int i = 0; i < newText.Length; i++)
			{
				if (newText[i] == '\n' || (nEnd - nStart) >= 8128)
				{
					if (!multiLine)
					{
						WriteLine();
						if (!tDTStyle)
							WriteBegin();

						multiLine = true;
					}

					WriteString(newText.Mid(nStart, nEnd - nStart), false);
					if (i < newText.Length - 1)
					{
						if (tDTStyle)
							WriteTag(Token.PLUS);

						WriteLine();
					}

					nStart = nEnd = i + 1;
					continue;
				}
				nEnd++;
			}

			// write remaining string in source string don't terminate with CR-LF pairs
			if (nEnd > nStart)
				WriteString(newText.Mid(nStart, nEnd - nStart), newLine);

			if (multiLine)
			{
				if (!newLine)
					WriteLine();

				if (!tDTStyle)
					WriteEnd();
			}
		}

		//------------------------------------------------------------------------------
		public void WriteTR (Token tag, int row, int col, bool newLine = true)
		{
			WriteTag(tag, false);
			WriteOpen (false);
			Write(row, false);
			WriteComma(false);
			Write(col, false);
			WriteClose(false);
			WriteBlank();
			
			if (newLine)
				WriteLine();
		}

		//------------------------------------------------------------------------------
		public void WriteTable(int row, int col, bool newLine = true)
		{
			WriteTR(Token.TABLE, row, col, newLine);
		}


		//------------------------------------------------------------------------------
		public void WriteRepeater(int row, int col, bool newLine = true)
		{
			WriteTR(Token.REPEATER, row, col, newLine);
		}

		//------------------------------------------------------------------------------
		public void UnparseDataType(string dataType, string arrayBaseType, ushort enumTag, bool newLine, bool indent)
		{
			if (dataType == "Array")
		    {
                UnparseDataType(dataType, 0, false, indent);
                UnparseDataType(arrayBaseType, enumTag, newLine, indent);
		    }
		    else
                UnparseDataType(dataType, enumTag, newLine, indent);
		}

		//------------------------------------------------------------------------------
        private void UnparseDataType(string dataType, ushort enumTag, bool newLine, bool indent)
		{
			if (enumTag != 0)	// && dataType == "Enum"
                dataType = string.Format("Enum[{0}]", enumTag);

			if (indent)
				WriteID(dataType.PadRight(Math.Max(dataType.Length, 12), ' '), newLine);
			else
				WriteID(dataType, newLine); 
		}

		//------------------------------------------------------------------------------
		public void WriteHeader()
		{
			WriteTag(Token.COMMENT_SEP);
			WriteTag(Token.COPYRIGHT);
			WriteTag(Token.COMMENT_SEP);
			WriteLine();
		}

		//------------------------------------------------------------------------------
		public void WriteRect(Token tag, Rectangle rect, bool newLine = true)
		{
			WriteTag(tag, false);
			WriteOpen(false);
			Write(rect.Top, false);
			WriteComma(false);
			Write(rect.Left, false);
			WriteComma(false);
			Write(rect.Bottom, false);
			WriteComma(false);
			Write(rect.Right, false);
			WriteClose(false);

			WriteBlank();

			if (newLine)
				WriteLine();
		}

		//------------------------------------------------------------------------------
		public void WriteRect(Rectangle RectCutted, bool newLine = true)
		{
			WriteRect(Token.RECT, RectCutted, newLine);
		}

		//------------------------------------------------------------------------------
		public void WriteRatio(int hRatio, int vRatio, bool bNewline = false)
		{
			if (hRatio != 0 || vRatio != 0)
			{
				WriteTag(Token.RATIO, false);
				WriteOpen();
				Write(hRatio, false);
				WriteComma();
				Write(vRatio, false);
				WriteClose(false);
			}

			if (bNewline)
				WriteLine();
		}

		//------------------------------------------------------------------------------
		public static string EscapeString(string aString)
		{
			// trasforma tutte le " in ""
			int pos = 0;
			while (true)
			{
				pos = aString.IndexOf("\"", pos);
				if (pos < 0) break;

				aString = aString.Insert(pos, "\"");
				pos += 2;
			}

			// trasforma tutte gli ' in ''
			pos = 0;
			while (true)
			{
				pos = aString.IndexOf("\'", pos);
				if (pos < 0) break;

				aString = aString.Insert(pos, "\'");
				pos += 2;
			}
			return aString;
		}

		//------------------------------------------------------------------------------
		public void WriteEscapedString(string aString)
		{
			Write(EscapeString(aString));
		}

		//------------------------------------------------------------------------------
		public void WriteDateTime(DateTime dt)
		{
			string str = string.Format
				(
				@"""{0:D2}/{1:D2}/{2:D4} {3:D2}:{4:D2}:{5:D2}""",
				dt.Day, dt.Month, dt.Year,
				dt.Hour, dt.Minute, dt.Second
				);
			Write("{dt " + str + "}");
		}

		//------------------------------------------------------------------------------
		public void WriteDate(DateTime dt)
		{
			string str = string.Format
				(
				@"""{0:D2}/{1:D2}/{2:D4}""",
				dt.Day, dt.Month, dt.Year
				);
			Write("{dt " + str + "}");
		}

		//------------------------------------------------------------------------------
		public void WriteTime(DateTime dt)
		{
			string str = string.Format
				(
				@"""{0:D2}:{1:D2}:{2:D2}""",
				dt.Hour, dt.Minute, dt.Second
				);
			Write("{t " + str + "}");
		}

		//------------------------------------------------------------------------------
		public void WriteElapsedTime(TimeSpan et)
		{
			string str = string.Format
				(
				@"""{0:D2}:{1:D2}:{2:D2}:{3:D}""",
				et.Days, et.Hours, et.Minutes, et.Seconds
				);
			Write("{et " + str + "}");
		}

		//------------------------------------------------------------------------------
		public void WriteLineTag(Token aToken) { WriteTag(aToken); WriteLine(); }
		public void WriteLineID(string aString) { WriteID(aString); WriteLine(); }

		//------------------------------------------------------------------------------
		public void WriteLine(bool aBool) { Write(aBool); WriteLine(); }
		public void WriteLine(byte aByte) { Write(aByte); WriteLine(); }
		public void WriteLine(short aWord) { Write(aWord); WriteLine(); }
		public void WriteLine(int aInt) { Write(aInt); WriteLine(); }
		public void WriteLine(long aLong) { Write(aLong); WriteLine(); }
		public void WriteLine(float aFloat) { Write(aFloat); WriteLine(); }
		public void WriteLine(double aDouble) { Write(aDouble); WriteLine(); }

		//------------------------------------------------------------------------------
		public void WriteLineString(string aString) { WriteString(aString); WriteLine(); }
		public void WriteLineSqlString(string aString) { WriteSqlString(aString); WriteLine(); }

		//------------------------------------------------------------------------------
		public void WriteLineBegin() { WriteBegin(); WriteLine(); }
		public void WriteLineEnd() { WriteEnd(); WriteLine(); }
		public void WriteLineComma() { WriteComma(); WriteLine(); }
		public void WriteLineColon() { WriteColon(); WriteLine(); }
		public void WriteLineSep() { WriteSep(); WriteLine(); }
		public void WriteLineSquareOpen() { WriteSquareOpen(); WriteLine(); }
		public void WriteLineSquareClose() { WriteSquareClose(); WriteLine(); }
		public void WriteLineOpen() { WriteOpen(); WriteLine(); }
		public void WriteLineClose() { WriteClose(); WriteLine(); }

		//------------------------------------------------------------------------------
		public void WriteLineAlias(ushort id) { WriteAlias(id); WriteLine(); }
		public void WriteLineItem(string itemName) { WriteItem(itemName); WriteLine(); }
		public void WriteLineSubscr(int aVal) { WriteSubscr(aVal); WriteLine(); }
		public void WriteLineSubscr(int aVal1, int aVal2) { WriteSubscr(aVal1, aVal2); WriteLine(); }
		public void WriteLineGUID(Guid aGuid) { WriteGUID(aGuid); WriteLine(); }
		public void WriteLineColor(Token token, Color aColor) { WriteColor(token, aColor); WriteLine(); }
		public void WriteLineEscapedString(string aString) { WriteEscapedString(aString); WriteLine(); }

		public void WriteLineDate(DateTime dt) { WriteDate(dt); WriteLine(); }
		public void WriteLineDateTime(DateTime dt) { WriteDateTime(dt); WriteLine(); }
		public void WriteLineTime(DateTime dt) { WriteTime(dt); WriteLine(); }
		public void WriteLineElapsedTime(TimeSpan et) { WriteElapsedTime(et); WriteLine(); }

		//------------------------------------------------------------------------------
		public void WritePen(BorderPen pen, bool newline = true)
		{
			WriteColor(Token.PEN, pen.Color, false);

			if (pen.Width != DEFAULT_PEN_WIDTH)
			{
				WriteTag(Token.SIZE, false);
				Write(pen.Width, false);
			}

			WriteSep();

			if (newline)
				WriteLine();
		}

		//------------------------------------------------------------------------------
		public void WriteBorders(Borders borders, bool newline = true)
		{
			WriteTag(Token.BORDERS, false);
			WriteOpen(false);
			Write(borders.Top ? 1 : 0, false); 
			WriteComma(false);
			Write(borders.Left ? 1 : 0, false);
			WriteComma(false);
			Write(borders.Bottom ? 1 : 0, false);
			WriteComma(false);
			Write(borders.Right ? 1 : 0, false);
			WriteClose(false);
			WriteSep(newline);
		}

		//------------------------------------------------------------------------------
		public void WriteOrigin(Point point, bool newLine = true)
		{
			WriteTag(Token.ORIGIN, false);
			WriteOpen(false);
			Write(point.X, false);
			WriteComma(false);
			Write(point.Y, false);
			WriteClose(false);
			WriteBlank();

			if (newLine)
				WriteLine();
		}

		//------------------------------------------------------------------------------
		public void WriteHeights(int title, int columnTitle, int cell, int total, bool newline = true)
		{
			WriteTag(Token.HEIGHTS, false);
			WriteOpen(false);
			Write(title, false);
			WriteComma(false);
			Write(columnTitle, false);
			WriteComma(false);
			Write(cell, false);
			WriteComma(false);
			Write(total, false);
			WriteClose(newline);
		}

		//------------------------------------------------------------------------------
		public void WriteFormat(string formatName, bool newLine = true)
		{
		    WriteTag(Token.FORMATSTYLE, false);
			WriteString(formatName, newLine);
			WriteBlank();
		}

		//------------------------------------------------------------------------------
		public bool IsLocalizableTextInCurrentLanguage()
		{
			return saveAsWithCurrentLanguage;
		}

		//------------------------------------------------------------------------------
		public string LoadReportString(string text)
		{
			if (!saveAsWithCurrentLanguage)
				return text;

			return text;
			//return AfxLoadReportString(sText, m_strOriginFileName, m_strOriginDictionaryPath);
		}

		//------------------------------------------------------------------------------
		public void WriteFont(string fontStyleName, bool newLine = true)
		{
			WriteTag(Token.FONTSTYLE, false);
			WriteString(fontStyleName, false);
			WriteSep(newLine);
		}
	}
}
