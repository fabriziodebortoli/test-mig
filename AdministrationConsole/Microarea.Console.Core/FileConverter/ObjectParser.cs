using System;
using System.Collections;
using System.IO;
using System.Text;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.FileConverter
{
	//================================================================================
	public class ParserPosition
	{
		//--------------------------------------------------------------------------------
		public ParserPosition (int x, long y, string name)
		{
			this.X = x;
			this.Y = y;
			this.Name = name;
		}

		public int	X;
		public long Y;
		public string Name;
	}

	//================================================================================

	public class ParserInfo
	{
		//--------------------------------------------------------------------------------
		public ParserInfo (string line, string oldLine, DiagnosticType dt)
		{
			this.OldLine	= oldLine;
			this.Line		= line;
			this.Offset		= new Hashtable();
		}

		public string	OldLine;
		public string	Line;
		public Hashtable Offset;
	}

	/// <summary>
	/// Summary description for TableParser.
	/// </summary>
	//================================================================================
	public abstract class ObjectParser : FileParser
	{
		
		protected		string			buffer;
		protected		StringReader	readBuffer;
		protected		long			currentLineIndex;
		protected		Hashtable		lines = new Hashtable();
		public			Parser			parser;
		public			Encoding		encoding = Encoding.Default;
		protected		int				startTokenPosition;
		protected		int				endTokenPosition;
		
		//migration kit uses a log space for each file, while databaseobjectsmng uses a unique log space;
		//so when we are in the databaseobjectsmng context we cannot use the log space to determine if
		//a single parsing procedure has failed, because an error in parsing a single file would be 
		//propagated to all subsequent ones. Therefore, when wh are in the databaseobjectsmng context
		//checkLogErrorsToValidateParsing must be false, when we are in the migration kit context must be true
		protected		bool			checkLogErrorsToValidateParsing = true;		
		
		//---------------------------------------------------------------------------------------------------
		public override string GetPosition()
		{
			if (parser == null)	return string.Empty;
			return string.Format("line {0}, position {1}: {2}", parser.CurrentLine, parser.CurrentPos, parser.CurrentLexeme);
		}

		//---------------------------------------------------------------------------------------------------
		virtual public string LookUpDocumentNamespace(ref string ns)
		{	//override in ReportMigrationNet-WrmTableParser per lookup sui namespace inglesi (serve anche il ref)
			return ns;
		}

		//---------------------------------------------------------------------------------------------------
		protected ParserInfo CurrentInfo
		{
			get
			{
				try
				{
					if (lines.Count == 0)
						return new ParserInfo(null, null, DiagnosticType.None);

					return (ParserInfo)lines[currentLineIndex];
				}
				catch
				{
					return new ParserInfo(null, null, DiagnosticType.None);
				}

			}
			set
			{
				lines[currentLineIndex] = value;
			}
		}
		
		//---------------------------------------------------------------------------------------------------
		protected string CurrentLine
		{
			get
			{
				return CurrentInfo.Line;
			}
			set
			{
				CurrentInfo.Line = value;
			}
		}

		//---------------------------------------------------------------------------------------------------
		protected string OldLine
		{
			get
			{
				return CurrentInfo.OldLine;			
			}
			set
			{
				CurrentInfo.OldLine = value;
			}
		}

		//---------------------------------------------------------------------------------------------------
		protected Hashtable CurrentLineOffset
		{
			get
			{
				return CurrentInfo.Offset;			
			}
		}

		//--------------------------------------------------------------------------------
		protected int GetCurrentLineOffset(int columnIndex)
		{
			int totalOffset = 0;
			foreach (int key in CurrentLineOffset.Keys)
			{
				if (key <= columnIndex)
					totalOffset += (int) CurrentLineOffset[key];
			}

			return totalOffset;
		}

		//---------------------------------------------------------------------------------------------------
		public ObjectParser(string fileName) : base (fileName)
		{
		}

		//--------------------------------------------------------------------------------
		public ObjectParser(string fileName, string destinationFileName) : base(fileName, destinationFileName)
		{
		}
		
		//---------------------------------------------------------------------------------------------------
		protected virtual void InitParser(Parser parser)
		{
			parser.Progress += new EventHandler(ParseLineChanged);
			parser.PreprocessorDisabled = true;			
		}

		//---------------------------------------------------------------------------------------------------
		public override bool Parse()
		{
			StreamReader sr = null;			
			StreamWriter sw = null;
			bool failed = false;

			try
			{		
				sr = new StreamReader(fileName, encoding, true);
				parser = new Parser(Parser.SourceType.FromFile);

				buffer = sr.ReadToEnd();
				
				encoding = sr.CurrentEncoding;
				sr.Close();
				readBuffer = new StringReader(buffer);

				StringBuilder sb = new StringBuilder();
				
				InitParser(parser);
				parser.Open(fileName);
				
				ProcessBuffer();

				if (checkLogErrorsToValidateParsing)
					GlobalContext.LogManager.AddMessage(parser.Diagnostic);
				
				parser.Close();

				failed = checkLogErrorsToValidateParsing && GlobalContext.LogManager.LogWriter.Error();
				parser = null;
				readBuffer = null;
				
				TryToCheckOut();

				sw = new StreamWriter(destinationFileName, false, encoding);
				for (long line = 1; line <= lines.Keys.Count; line++)
				{
					string currLine = ((ParserInfo)lines[line]).Line;
					if (currLine!=null) 
					{
						if (line == lines.Keys.Count)
							sw.Write(currLine);
						else
							sw.WriteLine(currLine);
					}
				}
				sw.Close();
				
			}
			catch(Exception ex)
			{
				if (parser != null && checkLogErrorsToValidateParsing)
					GlobalContext.LogManager.AddMessage(parser.Diagnostic);
				
				GlobalContext.LogManager.Message(ex.Message, ex.Source, DiagnosticType.Error, parser != null ? new ExtendedInfo("", parser.CurrentLine) : null);
				return false;
			}
			finally
			{
				if (parser!=null) { parser.Close(); parser = null; }
				if (sr!=null) sr.Close();
				if (sw!=null) sw.Close();
			}

			return !failed;
		}
		
		//--------------------------------------------------------------------------------
		protected virtual void CurrentLineChanged ()
		{
			if (readBuffer != null)
			{
				string currentLine = readBuffer.ReadLine();
				if (currentLine != null)
				{
					currentLineIndex = parser.CurrentLine + 1;
					CurrentInfo = new ParserInfo(currentLine, currentLine, DiagnosticType.Information);
				}
			}
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseLineChanged(object sender, EventArgs args)
		{
			CurrentLineChanged();
		}
		
		//---------------------------------------------------------------------------------------------------
		public void ThrowException(string message)
		{
			throw new FileConverterException(message, parser.Filename, parser.CurrentLine, parser.CurrentPos);
		}

		//---------------------------------------------------------------------------------------------------
		protected void ThrowLastParserErrorException()
		{
			IDiagnosticItems items = parser.Diagnostic.AllMessages(DiagnosticType.Error);
			ThrowException(items[items.Count - 1].ToString());
		}

		//---------------------------------------------------------------------------------------------------
		protected abstract void ProcessBuffer();
		
		//---------------------------------------------------------------------------------------------------
		public void ReplaceWord(int currentPos, string origin, string destination)
		{
			ReplaceWord(currentPos, origin, destination, false);
		}

		//---------------------------------------------------------------------------------------------------
		public void ReplaceWord(int currentPos, string origin, string destination, bool currPosAtStartOfWord)
		{
			ReplaceWord(currentLineIndex, currentPos, origin, destination, currPosAtStartOfWord);
		}

		//---------------------------------------------------------------------------------------------------
		public void ReplaceWord(int currentPos, string origin, string destination, bool currPosAtStartOfWord, DiagnosticType dt)
		{
			ReplaceWord(currentLineIndex, currentPos, origin, destination, currPosAtStartOfWord, dt);
		}

		//---------------------------------------------------------------------------------------------------
		public void ReplaceWord(long currentLine, int currentPos, string origin, string destination)
		{
			ReplaceWord(currentLine, currentPos, origin, destination, false);
		}

		//---------------------------------------------------------------------------------------------------
		public void ReplaceWord(long currentLine, int originalCurrentPos, string origin, string destination, bool currentPosAtStartOfWord)
		{
			ReplaceWord(currentLine,  originalCurrentPos,  origin,  destination,  currentPosAtStartOfWord, DiagnosticType.Information);
		}

		/// <summary>
		/// Sostituisce la parola nella riga corrente
		/// </summary>
		/// <param name="currentLine">indice della riga corrente</param>
		/// <param name="Pos">posizione corrente del parser</param>
		/// <param name="origin">parola da modificare</param>
		/// <param name="destination">parola modificata</param>
		/// <param name="currentPosAtStartOfWord">indica se cutrrentPos è all'inizio o alla fine della parola da modificare</param>
		//---------------------------------------------------------------------------------------------------
		public void ReplaceWord(long currentLine, int originalCurrentPos, string origin, string destination, bool currentPosAtStartOfWord, DiagnosticType dt)
		{
			if (origin == destination) return;

			int currentPos = originalCurrentPos;

			long bkpLineIndex = currentLineIndex;
			currentLineIndex = currentLine;
			if (
				CurrentLine == null || 
				origin == null || 
				destination == null ||
				string.Compare(origin, destination, true) == 0
				)
			{
				currentLineIndex = bkpLineIndex;
				return;
			}
			
			try
			{
				currentPos = currentPos + GetCurrentLineOffset(originalCurrentPos);
				if (currentPosAtStartOfWord)
					currentPos += origin.Length;
				string newVal ;
				if (currentPos >= 0)
				{
					string scmp = CurrentLine.Substring(currentPos - origin.Length, origin.Length);
					if (string.Compare(scmp, origin, true) != 0) 
					{
						currentLineIndex = bkpLineIndex;
						throw new Exception
							(
							string.Format
							(
							FileConverterStrings.ErrorReplacingWord,
							origin,
							destination,
							CurrentLine
							)
							);
					}
				
					newVal = CurrentLine.Substring(0, currentPos - origin.Length);
					newVal += destination;
					newVal+= CurrentLine.Substring(currentPos);
				}
				else if (origin == string.Empty)
					newVal = CurrentLine + " " + destination;
				else
					newVal = destination;

				// sposto l'offset per riconciliare il disallineamento che si viene a creare
				// fra il mio buffer e quello del parser a seguito della sostituzione
				if (CurrentLineOffset[originalCurrentPos] == null)
					CurrentLineOffset[originalCurrentPos] = destination.Length - origin.Length;
				else
					CurrentLineOffset[originalCurrentPos] = (int)CurrentLineOffset[originalCurrentPos] + destination.Length - origin.Length;

				CurrentLine = newVal;
				modified = true;

				string s;
				if (origin != string.Empty && destination != string.Empty)
					s = string.Format("{0} replaced with: {1}", origin, destination);
				else if (origin == string.Empty)
					s = string.Format("Added: {0}", destination);
				else // if (destination == string.Empty)
					s = string.Format("Removed: {0}", origin);

				GlobalContext.LogManager.Message(s, string.Empty, dt, new ExtendedInfo("", bkpLineIndex));

			}
			catch(Exception ex)
			{
				GlobalContext.LogManager.Message(ex.Message, ex.Source, DiagnosticType.Error, new ExtendedInfo("", bkpLineIndex));
				throw ex;
			}
			currentLineIndex = bkpLineIndex;
		}

		//---------------------------------------------------------------------------------------------------
		protected bool HasToStop(Token[] tokens)
		{
			if (parser.Eof) return true;

			foreach (Token t in tokens) 
				if (parser.LookAhead() == t) return true;

			return false;
		}

		//------------------------------------------------------------------------------
		public bool SkipRect()
		{
			int top = 0;
			int left = 0;
			int right = 0;
			int bottom = 0;

			bool ok =
				parser.ParseOpen       () &&
				parser.ParseSignedInt  (out top) &&
				parser.ParseComma      () &&
				parser.ParseSignedInt  (out left) &&
				parser.ParseComma      () &&
				parser.ParseSignedInt  (out bottom) &&
				parser.ParseComma      () &&
				parser.ParseSignedInt  (out right) &&
				parser.ParseClose      ();

			return ok;
		}

		//------------------------------------------------------------------------------
		public bool SkipRatio()
		{
			if (parser.LookAhead() == Token.RATIO)
			{
				int hr = 0;
				int vr = 0;
	
				bool ok =
					parser.ParseTag			(Token.RATIO) &&
					parser.ParseOpen		() &&
					parser.ParseSignedInt	(out hr) &&
					parser.ParseComma		() &&
					parser.ParseSignedInt	(out vr) &&
					parser.ParseClose		();

				return ok;
			}
			return true;
		}

	}
}
