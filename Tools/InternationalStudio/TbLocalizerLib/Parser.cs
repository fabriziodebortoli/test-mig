using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Woorm.WoormEngine;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;
using LexanParser = Microarea.TaskBuilderNet.Core.Lexan.Parser;
using Newtonsoft.Json;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Classe padre per tutti i parser dei file del progetto da localizzare.
	/// </summary>
	//=========================================================================
	public abstract class Parser
	{
		/// <summary>stringa che rappresenta la riga precedente</summary>
		protected string previousLine;
		/// <summary>stringa che rappresenta la riga attuale</summary>
		protected string currentLine;
		/// <summary>posizione dell'inizio del token attuale</summary>
		protected int startToken = 0;
		/// <summary>posizione della fine del token attuale</summary>
		protected int endToken = 0;
		/// <summary>lettore del file</summary>
		protected TextReader file;
		/// <summary>parser di lexan che riconosce i token</summary>
		internal LexanParser lex;
		/// <summary>elenco dei path per i file include</summary>
		protected string[] paths;

		protected Logger logWriter;

		/// <summary>Controlla la fine del file(readline ritorna null se eof). </summary>
		//--------------------------------------------------------------------------------
		protected bool EOF { get { return currentLine == null; } }

		//--------------------------------------------------------------------------------
		public Logger LogWriter { get { return logWriter; } }

		//---------------------------------------------------------------------
		public Parser(Logger logWriter)
		{
			this.logWriter = logWriter;

			lex = new LexanParser(LexanParser.SourceType.FromString);
			lex.PreprocessInclude = false;


			//Si aggiunge questa userKeyword per evitare conflitti
			lex.UserKeywords.Add("TEXT", Token.ID);
		}

		//--------------------------------------------------------------------------------
		public abstract DictionaryContainer DictionaryDocument { get; }

		//---------------------------------------------------------------------
		public abstract void Parse(string file);

		//---------------------------------------------------------------------
		public virtual void Save(DictionaryFileCollection document, string dictionaryRoot, string dictionaryFileName)
		{
			DictionaryDocument.Save(document[dictionaryFileName], dictionaryRoot, logWriter);
		}

		//---------------------------------------------------------------------
		internal static string ErrorParsingMessage(string file)
		{
			return String.Format(Strings.ParseError, file);
		}

		/// <summary>
		/// Apre il file e lo prepara per la lettura.
		/// </summary>
		/// <param name="fileName">path del file da aprire</param>
		//---------------------------------------------------------------------
		protected virtual bool Open(string fileName)
		{
			try
			{
				file = new StreamReader(fileName, Encoding.GetEncoding(0));
				Rewind();
				currentLine = null;
				previousLine = null;
				NextLine();
			}
			catch (Exception exc)
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, exc.Message, "Parser - Open");
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------
		protected void Close()
		{
			if (file != null)
				file.Close();
		}

		/// <summary>
		/// Porta end e start in posizione 0
		/// </summary>
		//---------------------------------------------------------------------
		private void Rewind()
		{
			startToken = endToken = 0;
		}

		/// <summary>
		/// Ottiene la prossima riga.
		/// </summary>
		//---------------------------------------------------------------------
		protected bool NextLine()
		{
			Rewind();
			if (file == null) return false;
			previousLine = currentLine;
			currentLine = ReadLine();
			//controllo le zone da non parsare, indicate dalla stringa BEGIN_TBLOCALIZER_SKIP
			if (currentLine != null && currentLine.IndexOf(AllStrings.beginSkipParse) != -1)
			{
				while (currentLine != null && currentLine.IndexOf(AllStrings.endSkipParse) == -1)
					NextLine();
			}
			return currentLine == null;
		}

		/// <summary>
		/// Legge la prossima riga.
		/// </summary>
		//---------------------------------------------------------------------
		private string ReadLine()
		{
			return file == null ? null : file.ReadLine();
		}

		/// <summary>
		/// Controlla che nella riga ci sia il token specificato.
		/// </summary>
		/// <param name="token">token di cui verificare l'esistenza</param>
		//---------------------------------------------------------------------
		protected bool LookAhead(Token token)
		{
			if (currentLine == null) return false;
			int start, end;
			if (FindToken(token, out start, out end))
			{
				startToken = start;
				endToken = end;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Isola la stringa identificata dal token TEXTSTRING
		/// </summary>
		//---------------------------------------------------------------------
		protected string FindString()
		{
			return FindString(false);
		}

		/// <summary>
		/// Isola la stringa (o l'id per include con parentesi angolari)
		/// </summary>
		/// <param name="orID">specifica se cercare, in alternativa alla stringa, un id</param>
		//---------------------------------------------------------------------
		protected string FindString(bool orID)
		{
			lex.Close();
			lex.Open(currentLine.Substring(endToken));
			string str = null;
			while (!lex.Eof)
			{
				if (lex.LookAhead() == Token.TEXTSTRING)
				{
					lex.ParseString(out str);
					break;
				}
				else if (orID && lex.LookAhead() == Token.ID)
				{
					str = currentLine.Substring(currentLine.IndexOf("<")).Trim();
					break;
				}
				else
					lex.SkipToken();
			}
			return str;

		}

		/// <summary>
		/// Isola la stringa rappresentante un id.
		/// </summary>
		//---------------------------------------------------------------------
		protected string FindID()
		{
			lex.Close();
			lex.Open(currentLine.Substring(endToken));
			string str = null;
			while (!lex.Eof)
			{
				if (lex.LookAhead() == Token.ID)
				{
					lex.ParseID(out str);
					break;
				}
				else
					lex.SkipToken();

			}
			return (str == null) ? String.Empty : str;
		}

		/// <summary>
		/// Trova nella riga il token specificato aggiornando i valori che ne indicano la posizione.
		/// </summary>
		/// <param name="token">token da trovare</param>
		/// <param name="start">posizione di inizio token</param>
		/// <param name="end">posizione di fine token</param>
		//---------------------------------------------------------------------
		private bool FindToken(Token token, out int start, out int end)
		{
			lex.Close();
			lex.Open(currentLine.Substring(endToken));
			bool found = false;
			start = end = 0;
			while (!lex.Eof && !found)
			{
				if (lex.LookAhead() == token)
				{
					start = lex.CurrentPos;
					lex.SkipToken();
					end = lex.CurrentPos;
					found = true;
				}
				else
					lex.SkipToken();
			}
			return found;
		}

		//---------------------------------------------------------------------
		internal static string BuildMessage(DiagnosticItem item)
		{
			if (item == null)
				return String.Empty;
			StringBuilder sb = new StringBuilder();
			sb.Append(item.FullExplain);
			if (item.ExtendedInfo.Count > 0)
			{
				sb.Append("\r\n");
				sb.Append(item.ExtendedInfo.Format(LineSeparator.Cr));
			}
			return sb.ToString();
		}
	}

	//=========================================================================
	/// <summary>
	/// Parsa i file rc.
	/// </summary>
	public class RCParser : Parser
	{
		/// <summary>conteggio delle stringtable parsate</summary>
		private int countStringTable = 0;
		/// <summary>documento che scrive il file relativo alle dialog</summary>
		protected DictionaryContainer dialogWriter = new DictionaryContainer();
		/// <summary>documento che scrive il file relativo ai menu</summary>
		protected DictionaryContainer menuWriter = new DictionaryContainer();
		/// <summary>documento che scrive il file relativo alle stringtable</summary>
		protected DictionaryContainer stringTableWriter = new DictionaryContainer();
		/// <summary>documento che scrive il file indice delle risorse</summary>
		protected ResourceIndexContainer resourceIndex = null;
		/// <summary>documento che scrive il file relativo alle dialog</summary>
		protected INCParser includeParser;
		/// <summary>documento che scrive il file relativo alle dialog</summary>
		private ArrayList allIncludes = new ArrayList();

		private string currentFile;

		//--------------------------------------------------------------------------------
		public DictionaryContainer DialogDictionaryDocument { get { return dialogWriter; } }
		//--------------------------------------------------------------------------------
		public DictionaryContainer MenuDictionaryDocument { get { return menuWriter; } }
		//--------------------------------------------------------------------------------
		public DictionaryContainer StringTableDictionaryDocument { get { return stringTableWriter; } }


		//--------------------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument
		{
			get
			{
				return null;
			}
		}



		/// <summary>Token da rilevare nel file per la localizzazione delle stringhe</summary>
		public enum RCToken
		{
			DIALOG = Token.USR00, DIALOGEX = Token.USR01, BEGIN = Token.USR02,
			END = Token.USR03, CAPTION = Token.USR04, CONTROL = Token.USR05,
			LTEXT = Token.USR06, RTEXT = Token.USR07, PUSHBUTTON = Token.USR08,
			LANGUAGE = Token.USR09, TEXTINCLUDE = Token.USR10, DESIGNINFO = Token.USR11,
			DEFPUSHBUTTON = Token.USR12, GROUPBOX = Token.USR13, EDITTEXT = Token.USR14,
			COMBOBOX = Token.USR15, LISTBOX = Token.USR16, STRINGTABLE = Token.USR17,
			AVI = Token.USR18, ICON = Token.USR19, BITMAP = Token.USR20,
			ACCELERATORS = Token.USR21, TOOLBAR = Token.USR22, MENU = Token.USR23,
			MENUITEM = Token.USR24, POPUP = Token.USR25, MENUEX = Token.USR26,
			CTEXT = Token.USR27,
			NULLTOKEN = Token.USR99
		}

		//array di RCToken per parsare i controlli della dialog
		private RCToken[] dialogTokens =
		{
			RCToken.LTEXT,          RCToken.RTEXT,
			RCToken.PUSHBUTTON,     RCToken.CONTROL,
			RCToken.GROUPBOX,       RCToken.DEFPUSHBUTTON,
			RCToken.CTEXT
		};

		//---------------------------------------------------------------------
		public RCParser(Logger logWriter, string[] includePaths, ResourceIndexContainer resourceIndex)
			: base(logWriter)
		{
			//Aggiungo tutti i miei token che considero interessanti al fine di 
			//riconoscere le stringhe da esternalizzare
			lex.UserKeywords.Add("Dialog", RCToken.DIALOG);
			lex.UserKeywords.Add("DialogEx", RCToken.DIALOGEX);
			lex.UserKeywords.Add("Caption", RCToken.CAPTION);
			lex.UserKeywords.Add("Begin", RCToken.BEGIN);
			lex.UserKeywords.Add("End", RCToken.END);
			lex.UserKeywords.Add("RText", RCToken.RTEXT);
			lex.UserKeywords.Add("LText", RCToken.LTEXT);
			lex.UserKeywords.Add("CText", RCToken.CTEXT);
			lex.UserKeywords.Add("Control", RCToken.CONTROL);
			lex.UserKeywords.Add("PushButton", RCToken.PUSHBUTTON);
			lex.UserKeywords.Add("DefPushButton", RCToken.DEFPUSHBUTTON);
			lex.UserKeywords.Add("", RCToken.NULLTOKEN);
			lex.UserKeywords.Add("Language", RCToken.LANGUAGE);
			lex.UserKeywords.Add("TextInclude", RCToken.TEXTINCLUDE);
			lex.UserKeywords.Add("DesignInfo", RCToken.DESIGNINFO);
			lex.UserKeywords.Add("ListBox", RCToken.LISTBOX);
			lex.UserKeywords.Add("StringTable", RCToken.STRINGTABLE);
			lex.UserKeywords.Add("Avi", RCToken.AVI);
			lex.UserKeywords.Add("Icon", RCToken.ICON);
			lex.UserKeywords.Add("Bitmap", RCToken.BITMAP);
			lex.UserKeywords.Add("Accellerators", RCToken.ACCELERATORS);
			lex.UserKeywords.Add("Toolbar", RCToken.TOOLBAR);
			lex.UserKeywords.Add("Menu", RCToken.MENU);
			lex.UserKeywords.Add("MenuEx", RCToken.MENUEX);
			lex.UserKeywords.Add("MenuItem", RCToken.MENUITEM);
			lex.UserKeywords.Add("Popup", RCToken.POPUP);
			lex.UserKeywords.Add("Groupbox", RCToken.GROUPBOX);

			this.includeParser = new INCParser(logWriter);
			this.paths = includePaths;
			this.resourceIndex = resourceIndex;

		}

		//---------------------------------------------------------------------
		public override void Save(DictionaryFileCollection document, string dictionaryRoot, string dictionaryFileName)
		{
			DialogDictionaryDocument.Save(document[dictionaryFileName], dictionaryRoot, logWriter);
			MenuDictionaryDocument.Save(document[dictionaryFileName], dictionaryRoot, logWriter);
			StringTableDictionaryDocument.Save(document[dictionaryFileName], dictionaryRoot, logWriter);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file">path del file da parsare</param>
		/// <param name="aPaths">elenco dei path dove trovare gli include</param>
		/// <param name="logW">scrittore del log</param>
		//---------------------------------------------------------------------
		public override void Parse(string currentFile)
		{
			this.currentFile = currentFile;

			dialogWriter.Id = Path.GetFileNameWithoutExtension(currentFile);
			menuWriter.Id = dialogWriter.Id;
			stringTableWriter.Id = dialogWriter.Id;

			try
			{
				Open(currentFile);

				Token thisToken;
				do
				{
					lex.Close();
					lex.Open(currentLine);
					while (!lex.Eof)
					{
						thisToken = lex.LookAhead();
						//setto la posizione
						startToken = lex.CurrentPos;
						lex.SkipToken();
						endToken = lex.CurrentPos;
						switch (thisToken)
						{
							case Token.INCLUDE:
								{
									FindInclude();
									continue;
								}
							case (Token)RCToken.STRINGTABLE:
								{
									ParseStringTable();
									continue;
								}

							case (Token)RCToken.MENU:
							case (Token)RCToken.MENUEX:
								{
									ParseMenu();
									continue;
								}
							case (Token)RCToken.TEXTINCLUDE:
							case (Token)RCToken.DESIGNINFO:
							case (Token)RCToken.TOOLBAR:
							case (Token)RCToken.ACCELERATORS:
								{
									JumpUseless();
									continue;
								}
							case (Token)RCToken.DIALOG:
							case (Token)RCToken.DIALOGEX:
								{
									ParseDialogEx();
									continue;
								}
						}// switch
					}//while riga
					NextLine();
				}//while file
				while (!EOF);
			}
			finally
			{
				Close();
			}
			return;
		}

		/// <summary>
		/// Trova gli include(recursive)per recuperare gli id necessari nel file di indice delle risorse.
		/// </summary>
		//---------------------------------------------------------------------
		private void FindInclude()
		{
			string include = FindString(true);
			if (include == String.Empty) return;
			//se tra parentesi angolari mi ritorna null e lo devo andare a cercare nelle directory predefinite
			if (include.IndexOf(">") != -1)
			{
				//cerco prima nella cartella corrente, poi provo ad abbinare 
				//i vari path se all define dice che il file non esiste.
				char[] c = { '<', '>' };
				include = include.Trim(c);
			}
			include = include.Trim().ToLower();
			if (Path.HasExtension(include))
			{
				string includeCOPY = (string)include.Clone();
				string curr = Path.GetDirectoryName(currentFile);
				string pathRC = Path.Combine(curr, include);
				bool checkFileFound = false;
				if (!allIncludes.Contains(pathRC.ToLower()))
				{
					allIncludes.Add(pathRC.ToLower());
					if (!includeParser.AllDefines(pathRC, paths, allIncludes))
					{
						foreach (string s in paths)
						{
							string pathInc = Path.Combine(s, include);
							if (allIncludes.Contains(pathInc.ToLower()))
							{
								checkFileFound = true;
								continue;
							}
							allIncludes.Add(pathInc.ToLower());
							if (includeParser.AllDefines(pathInc, paths, allIncludes))
							{
								checkFileFound = true;
								break;
							}
						}
						if (!checkFileFound)
							logWriter.WriteLog(String.Format(Strings.IncludeNotFound, include), TypeOfMessage.warning);
					}
				}
			}
		}

		/// <summary>
		/// Salto le parti che non interessano, contenenti un blocco begin-end.
		/// </summary>
		//---------------------------------------------------------------------
		private void JumpUseless()
		{
			int b = 0, e = 0;
			do
			{
				if (LookAhead((Token)RCToken.BEGIN)) b++;
				if (LookAhead((Token)RCToken.END)) e++;
				NextLine();
			}
			while (!EOF && (b != e || e == 0));
		}

		/// <summary>
		/// Parso la struttura della dialog.
		/// </summary>
		//---------------------------------------------------------------------
		private void ParseDialogEx()
		{
			string IDD = currentLine.Substring(0, startToken).Trim();
			if (IDD == String.Empty)
			{
				NextLine();
				return;
			}

			dialogWriter.WriteResource(AllStrings.dialog, IDD);

			WriteResIndex(IDD, AllStrings.dialog);
			do
			{
				if (LookAhead(((Token)RCToken.CAPTION)))
				{
					ParseStrings(dialogWriter);
					NextLine();
					continue;
				}
				NextLine();
			}
			while (!EOF && !LookAhead((Token)RCToken.BEGIN));

			do
			{
				ParseControls();
				NextLine();
			}
			while (!EOF && !LookAhead((Token)RCToken.END));
			dialogWriter.CloseResource();
			NextLine();
		}

		/// <summary>
		/// Parso la struttura del menu.
		/// </summary>
		//---------------------------------------------------------------------
		private void ParseMenu()
		{
			string IDD = currentLine.Substring(0, startToken).Trim();
			if (IDD == String.Empty)
			{
				NextLine();
				return;
			}

			menuWriter.WriteResource(AllStrings.menu, IDD);

			WriteResIndex(IDD, AllStrings.menu);
			do
				NextLine();
			while (!EOF && !LookAhead((Token)RCToken.POPUP));

			while (ParsePopup()) ;
			menuWriter.CloseResource();
		}

		/// <summary>
		/// Parsa la struttura del popup dei menu.
		/// </summary>
		/// <param name="b">numero di 'begin'</param>
		/// <param name="e">numero di 'end'</param>
		//---------------------------------------------------------------------
		private bool ParsePopup()
		{
			if (!LookAhead((Token)RCToken.POPUP))
				return false;

			ParseStrings(menuWriter);
			do
				NextLine();
			while (!EOF && !LookAhead((Token)RCToken.BEGIN));

			NextLine();

			do
			{
				if (LookAhead((Token)RCToken.POPUP))
				{
					ParsePopup();
					continue;
				}
				else if (LookAhead((Token)RCToken.MENUITEM))
				{
					ParseMenuItems();
					NextLine();
					continue;
				}
				else if (LookAhead((Token)RCToken.END))
				{
					NextLine();
					break;
				}
				else NextLine();
			}
			while (!EOF);

			return true;
		}

		/// <summary>
		/// Parsa i menuitems.
		/// </summary>
		//---------------------------------------------------------------------
		private void ParseMenuItems()
		{
			if (endToken == -1) NextLine();
			string aString = null;
			if (LookAhead((Token)RCToken.MENUITEM))
			{
				//per i menuitem non posso presupporre l'obbligatorietà di stringa presente 
				//caso MENUITEM SEPARATOR
				aString = FindString();
				if (aString != null && aString.Trim() != String.Empty)
					menuWriter.WriteString(aString);
			}
		}

		/// <summary>
		/// Parso la struttura della stringtable .
		/// </summary>
		//---------------------------------------------------------------------
		private void ParseStringTable()
		{
			if (countStringTable == 0)
			{
				string name = Path.GetFileNameWithoutExtension(currentFile).ToLower();
				stringTableWriter.WriteResource(AllStrings.stringtable, name);
				countStringTable++;
			}
			string id = null;
			while (!EOF && !LookAhead((Token)RCToken.BEGIN))
				NextLine();
			NextLine();
			do
			{
				id = FindID();
				if (id.Trim() != String.Empty)
				{
					WriteResIndex(id, AllStrings.stringtable);
					ParseStrings(stringTableWriter, id.Trim());
				}
				NextLine();
			}
			while (!EOF && !LookAhead((Token)RCToken.END));
			stringTableWriter.CloseResource();
			NextLine();
		}

		/// <summary>
		/// Parsa i controlli della dialog.
		/// </summary>
		//---------------------------------------------------------------------
		private void ParseControls()
		{
			if (endToken == -1) NextLine();
			if (DialogTokenFound() != RCToken.NULLTOKEN)
				ParseStrings(dialogWriter);
		}

		/// <summary>
		/// Controlla se si incontra un token relativo ai controlli della dialog.
		/// </summary>
		//---------------------------------------------------------------------
		private RCToken DialogTokenFound()
		{
			foreach (RCToken token in dialogTokens)
			{
				if (LookAhead((Token)token))
					return token;
			}
			return RCToken.NULLTOKEN;
		}

		/// <summary>
		///Scrive nel file di indice delle risorse.
		/// </summary>
		/// <param name="id">nome della risorsa</param>
		/// <param name="typeOfResource">tipologia di risorsa</param>
		//---------------------------------------------------------------------
		private void WriteResIndex(string id, string typeOfResource)
		{
			string fileName = Path.ChangeExtension(Path.GetFileName(currentFile), AllStrings.xmlExtension);
			double number;
			includeParser.lex.GetDefine(id, out number);
			//se id è zero scrivo sul log e non sul file indice
			if (number != 0)
				resourceIndex.AddResource(id, number, fileName, typeOfResource);
			else
				logWriter.WriteLog(String.Format(Strings.IdNotFound, id), TypeOfMessage.warning);
		}

		/// <summary>
		/// Parsa le stringhe. 
		/// </summary>
		/// <param name="thisWriter">datadocument sul quale si deve scrivere</param>
		//---------------------------------------------------------------------
		private void ParseStrings(DictionaryContainer thisWriter)
		{
			ParseStrings(thisWriter, null);
		}

		/// <summary>
		/// Parsa le stringhe.
		/// </summary>
		/// <param name="thisWriter">datadocument sul quale si deve scrivere</param>
		/// <param name="id">nome da dare alla stringa</param>
		//---------------------------------------------------------------------
		private void ParseStrings(DictionaryContainer thisWriter, string id)
		{
			if (currentLine == null)
				return;

			if (endToken == -1)
				NextLine();
			string aString = null;
			aString = FindString();
			if (aString == null)
			{
				NextLine();
				ParseStrings(thisWriter, id);
			}
			else if (aString.Trim() != String.Empty)
				thisWriter.WriteString(aString/*, id*/);
		}
	}

	/// <summary>
	/// Parsa i file cpp
	/// </summary>
	//=========================================================================
	public class CPPParser : Parser
	{
		protected static readonly string Comment1Pattern =
			"/\\*" +
			RegexHelper.ZeroOrMoreTimes(RegexHelper.NotFollowedBy("\\*/", RegexHelper.anyCharacter), false) +
			RegexHelper.anyCharacter +
			"\\*/";

		protected static readonly string Comment2Pattern =
			RegexHelper.zeroOrMoreSpaces +
			"//(.(?!^.))*";

		protected static Regex commentTrimmer = new Regex(RegexHelper.AlternativeTokens(Comment1Pattern, Comment2Pattern), RegexOptions.Compiled);

		/// <summary>nome del file che si sta parsando</summary>
		private string currentFile = null;
		/// <summary>documento che scrive il dizionario per i file sorgenti</summary>
		protected DictionaryContainer cppWriter = new DictionaryContainer();
		/// <summary>specifica se la risorsa è stata già scritta sul file di dizionario</summary>
		private bool resourceWrited = false;

		/// <summary>token di stato per il parse della stringa</summary>
		public enum CPPParsingState { WAIT_ROUND_OPEN, WAIT_STRING, STRING, ESCAPE }//NULL, TBBEGIN, STRINGBEGIN, STRING, SLASH, STRINGEND, TBEND, REFUSE}

		public enum CPPState { NULL, TBBEGIN, STRINGBEGIN, STRING, SLASH, STRINGEND, TBEND, REFUSE }

		//--------------------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument { get { return cppWriter; } }

		//---------------------------------------------------------------------
		public CPPParser(Logger logWriter)
			: base(logWriter)
		{
		}

		/// <summary>
		/// Parsa un file .cpp
		/// </summary>
		/// <param name="file">path del file da parsare</param>
		/// <param name="loadFile">specifica se caricare il file che è già stato creato</param>
		//---------------------------------------------------------------------
		public override void Parse(string file)
		{
			currentFile = file;
			cppWriter.Id = AllStrings.sources;

			try
			{
				Open(currentFile);

				do
				{
					string refuse = currentLine;
					do
					{
						int position = FindLocalizableStringPosition(refuse);
						if (position == -1)
							break;
						refuse = ParseStrings(position, refuse);
					}
					while (refuse != String.Empty);

					NextLine();
					continue;
				}
				while (!EOF);

				if (resourceWrited)
				{
					cppWriter.CloseResource();
					resourceWrited = false;
				}
			}
			finally
			{
				Close();
			}
		}

		//--------------------------------------------------------------------------------
		protected override bool Open(string fileName)
		{
			try
			{
				file = new StreamReader(fileName, Encoding.GetEncoding(0));
				currentLine = null;
				previousLine = null;
				RemoveComments();
				NextLine();
			}
			catch (Exception exc)
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, exc.Message, "CPPParser - Open");
				return false;
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		private void RemoveComments()
		{
			string s = file.ReadToEnd();
			file.Close();

			s = commentTrimmer.Replace(s, string.Empty);

			file = new StringReader(s);
		}

		/// <summary>
		/// Trova le stringhe localizzabili e le scrive nel dizionario
		/// </summary>
		//---------------------------------------------------------------------
		private int FindLocalizableStringPosition(string stringToAnalize)
		{
			string pattern = "(\\b" + AllStrings.stringToken1 + "\\b)|(\\b" + AllStrings.stringToken2 + "\\b)|(\\b" + AllStrings.stringToken3 + "\\b)";
			Match m = Regex.Match(stringToAnalize, pattern);
			return m.Success ? m.Index : -1;
		}

		//---------------------------------------------------------------------
		private string ParseStrings(int count, string stringToAnalize)
		{
			//nella currentline ho i token e la stringa 
			string work = String.Empty;
			work = stringToAnalize.Substring(count);
			CPPState state = CPPState.NULL;

			StringBuilder aString = new StringBuilder();

			foreach (char c in work.ToCharArray())
			{
				if (state == CPPState.REFUSE)
				{
					aString.Append(c);
					continue;
				}
				switch (c)
				{
					case ' ':
						if (state != CPPState.NULL && state != CPPState.STRINGEND && state != CPPState.TBBEGIN)
							aString.Append(c);
						break;
					case '(':
						if (state == CPPState.NULL)
							state = CPPState.TBBEGIN;
						else
							goto default;
						break;
					case ')':
						if (state == CPPState.STRINGEND)
							state = CPPState.TBEND;
						else
							goto default;
						break;
					case '\\':
						if (state == CPPState.SLASH)
						{
							state = CPPState.STRING;
							aString.Append(c);
						}
						else if (state == CPPState.STRING || state == CPPState.STRINGBEGIN)
						{
							state = CPPState.SLASH;
							aString.Append(c);
						}
						break;
					case '\"':
						if (state == CPPState.TBBEGIN)
							state = CPPState.STRINGBEGIN;
						else if (state == CPPState.SLASH)
						{
							state = CPPState.STRING;
							aString.Append(c);
						}
						else if (state == CPPState.STRING || state == CPPState.STRINGBEGIN)
							state = CPPState.STRINGEND;
						break;
					default:
						if (state == CPPState.STRING || state == CPPState.STRINGBEGIN || state == CPPState.SLASH)
						{
							state = CPPState.STRING;
							aString.Append(c);
						}
						break;
				}
				if (state == CPPState.TBEND && aString.ToString().Trim() != String.Empty)
				{
					if (!resourceWrited)
					{
						string name = Path.GetFileNameWithoutExtension(currentFile).ToLower();
						cppWriter.WriteResource(AllStrings.source, name);
						resourceWrited = true;
					}
					cppWriter.WriteString(aString.ToString());
					aString.Remove(0, aString.Length);
					state = CPPState.REFUSE;
					continue;
				}

			}
			//esci dal foreach: vuol dire che sono finiti i caratteri
			//controllo di essere almeno in tbend, altrimenti vuol
			//dire che la stringa finisce a capo e ciò sarebbe un errore sintattico,
			//quindi tralascio il mio stato e ricomincio da capo sulla nuova line
			//se lo stato è refuse allora tutta la stringa in avanzo 
			//devo rimandarla al parse

			if (state == CPPState.REFUSE)
				return aString.ToString().Trim();
			return String.Empty;
		}


		/// <summary>
		/// Parsa le stringhe cablate nel codice.
		/// </summary>
		//---------------------------------------------------------------------
		private string ParseStrings(string stringToAnalize)
		{
			//nella currentline ho i token e la stringa 
			char[] chars = stringToAnalize.ToCharArray();
			CPPParsingState state = CPPParsingState.WAIT_ROUND_OPEN;
			StringBuilder aString = new StringBuilder();

			char ch;
			for (int i = 0; i < chars.Length; i++)
			{
				ch = chars[i];
				switch (state)
				{
					// mi aspetto la parentesi '(' 
					// se trovo un carattere diverso mi arrabbio, a meno che non sia ' '
					case CPPParsingState.WAIT_ROUND_OPEN:
						if (ch == '(')
						{
							state = CPPParsingState.WAIT_STRING;
							continue;
						}
						if (ch == ' ') continue;
						return stringToAnalize.Substring(i);
					// mi aspetto l'inizio stringa '"'
					// se trovo un carattere diverso mi arrabbio, a meno che non sia ' '
					case CPPParsingState.WAIT_STRING:
						if (ch == '"')
						{
							state = CPPParsingState.STRING;
							continue;
						}
						if (ch == ' ') continue;
						return stringToAnalize.Substring(i);
					// il carattere precedente era un '\', sono in stato di escape e quindi ignoro 
					// l'eventuale chiusura di stringa (aggiungo il carattere ciecamente)
					case CPPParsingState.ESCAPE:
						aString.Append(ch);
						state = CPPParsingState.STRING;
						break;
					// se trovo il terminatore di stringa '"' aggiungo la stringa al dizionario ed esco
					// se trovo il carattere '\' mi metto in stato di escape (e aggiungo tale carattere alla stringa)
					case CPPParsingState.STRING:
						if (ch == '"')
						{
							string s = aString.ToString();
							if (s != string.Empty)
							{
								if (!resourceWrited) //perché lo memorizzo io e non il cppWriter?
								{
									string name = Path.GetFileNameWithoutExtension(currentFile).ToLower();
									cppWriter.WriteResource(AllStrings.source, name);
									resourceWrited = true;
								}
								cppWriter.WriteString(aString.ToString());
							}
							return stringToAnalize.Substring(i);
						}
						if (ch == '\\') state = CPPParsingState.ESCAPE;
						aString.Append(ch);
						break;
				}
			}

			return string.Empty;
		}
	}

	//=========================================================================
	/// <summary>
	/// Parsa gli enums.ini
	/// Gli enums.ini ora sono diventati xml, applichiamo una logica differente di parsing lasciando la logica precedente.
	/// </summary>
	public class EnumParser : Parser
	{

		#region DataMember Protetti

		protected DictionaryContainer enumWriter = new DictionaryContainer();
		protected string fileEnum = string.Empty;
		protected string fileSources = string.Empty;

		#endregion

		#region Proprietà

		//--------------------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument { get { return enumWriter; } }

		#endregion

		//---------------------------------------------------------------------
		public EnumParser(Logger logWriter)
			: base(logWriter)
		{
		}

		/// <summary>
		/// Parsa un file enum.ini.
		/// </summary>
		//---------------------------------------------------------------------
		public override void Parse(string fileEnum)
		{
			enumWriter.Id = Path.GetFileNameWithoutExtension(fileEnum);

			EnumTags et = new EnumTags();
			if (!et.LoadXml(fileEnum, null, false))
			{
				logWriter.WriteLog(Parser.ErrorParsingMessage(Path.GetFileName(fileEnum)), TypeOfMessage.error);
				return;
			}

			foreach (EnumTag tag in et)
			{
				string tagName = tag.Name;
				enumWriter.WriteResource(AllStrings.enumTag, tagName);
				enumWriter.WriteString(tagName);
				enumWriter.WriteString(tag.Description);

				foreach (EnumItem item in tag.EnumItems)
				{
					enumWriter.WriteString(item.Name);
					enumWriter.WriteString(item.Description);
				}
				enumWriter.CloseResource();
			}
		}

	}

	//=========================================================================
	public class FormatAndFontParser : Parser
	{
		string resourceSetName;
		string resourceName;
		Type type;
		Hashtable styles;

		DictionaryContainer writer = new DictionaryContainer();

		public enum Type { FONT, FORMAT }

		//--------------------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument { get { return writer; } }

		//---------------------------------------------------------------------
		public FormatAndFontParser(Logger logWriter, Type type)
			:
			this(logWriter, type, null)
		{
		}

		//---------------------------------------------------------------------
		public FormatAndFontParser(Logger logWriter, Type type, Hashtable styles)
			: base(logWriter)
		{
			this.type = type;
			this.styles = styles;

			if (type == Type.FONT)
			{
				resourceSetName = AllStrings.fonts;
				resourceName = AllStrings.font;
			}
			else
			{
				resourceSetName = AllStrings.formats;
				resourceName = AllStrings.format;
			}
		}

		//---------------------------------------------------------------------
		private bool Load(string filename)
		{
			if (type == Type.FONT)
			{
				styles = new FontStyles();
				if (
					!((FontStyles)styles).Load
					(
					filename,
					null,
					FontElement.FontSource.STANDARD
					)
					)
				{
					return false;
				}
			}
			else
			{
				styles = new FormatStyles(CommonFunctions.GetSession().ApplicationFormatStyles);
				if (
					!((FormatStyles)styles).Load
					(
					filename,
					null,
					Microarea.TaskBuilderNet.Core.Applications.Formatter.FormatSource.STANDARD
					)
					)
					return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		public void WriteString()
		{
			if (type == Type.FONT)
			{
				foreach (FontStylesGroup f in styles.Values)
					writer.WriteString(f.StyleName);
			}
			else
			{
				foreach (FormatStylesGroup f in styles.Values)
					writer.WriteString(f.StyleName);
			}
		}

		//---------------------------------------------------------------------
		public override void Parse(string filename)
		{
			writer.Id = resourceSetName;

			if (styles == null && !Load(filename))
			{
				logWriter.WriteLog(Parser.ErrorParsingMessage(Path.GetFileName(filename)), TypeOfMessage.error);
				return;
			}

			if (styles.Count == 0)
				return;

			writer.WriteResource(resourceName, resourceSetName);

			WriteString();

			writer.CloseResource();

		}

	}

	//=========================================================================
	/// <summary>
	/// Parsa gli include per trovare le define.
	/// </summary>
	public class INCParser : Parser
	{
		/// <summary>path del file corrente</summary> 
		private string currentFile = null;

		//---------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument
		{
			get
			{
				return null;
			}
		}

		//---------------------------------------------------------------------
		public INCParser(Logger logWriter)
			: base(logWriter)
		{
		}

		//--------------------------------------------------------------------------------
		public override void Parse(string file)
		{

		}

		/// <summary>
		/// Parsa tutto aggiungendo le define al suo array, se incontra include deve "annidarsi"
		/// </summary>
		/// <param name="file">path del file da leggere</param>
		//---------------------------------------------------------------------
		internal bool AllDefines(string file)
		{
			return AllDefines(file, null, null);
		}

		/// <summary>
		/// Parsa tutto aggiungendo le define al suo array, se incontra include deve "annidarsi", ritorna il buon fine
		/// </summary>
		/// <param name="file">path del file da leggere</param>
		/// <param name="apaths">lista dei path nei quali andare a cercare i file inclusi</param>
		/// <param name="allIncludes">lista degli include già letti per non ripetere la ricerca nello stesso file se incluso più volte.</param>
		//---------------------------------------------------------------------
		internal bool AllDefines(string file, string[] apaths, ArrayList allIncludes)
		{
			if (apaths != null)
				this.paths = apaths;
			if (!File.Exists(file))
				return false;
			currentFile = file;
			try
			{
				Open(currentFile);
				do
				{
					FindInclude(allIncludes);
					//in realtà non lo trova mai, trova EOF, ma carica tutto in defines(private)
					LookAhead(Token.DEFINE);
					NextLine();
					continue;
				}
				while (!EOF);
			}
			finally
			{
				Close();
			}
			return true;
		}

		/// <summary>
		/// Trova gli include - recursive.
		/// </summary>
		/// <param name="allIncludes">lista degli include già letti per non ripetere la ricerca nello stesso file se incluso più volte.</param>
		//---------------------------------------------------------------------
		private void FindInclude(ArrayList allIncludes)
		{
			if (LookAhead(Token.INCLUDE))
			{
				//FreezeState
				string tmpCurrentLine = currentLine;
				string tmpCurrentFile = currentFile;
				TextReader tmpFile = this.file;
				int tmpEnd = endToken;
				int tmpStart = startToken;

				string include = FindString(true);
				if (include == null || include == String.Empty) return;
				include = include.Trim().ToLower();
				char[] toTrim = { '<', '>' };
				include = include.Trim(toTrim);
				if (Path.HasExtension(include))
				{
					string curr = Path.GetDirectoryName(currentFile);
					string pathRC = Path.Combine(curr, include);
					bool checkFileFound = false;
					if (!allIncludes.Contains(pathRC.ToLower()))
					{
						allIncludes.Add(pathRC.ToLower());
						if (!AllDefines(pathRC, null, allIncludes))
						{
							foreach (string s in paths)
							{
								string pathInc = Path.Combine(s, include);
								if (allIncludes.Contains(pathInc.ToLower()))
								{
									checkFileFound = true;
									continue;
								}
								allIncludes.Add(pathInc.ToLower());
								if (AllDefines(pathInc, null, allIncludes))
								{
									checkFileFound = true;
									break;
								}
							}
							if (!checkFileFound)
								logWriter.WriteLog(String.Format(Strings.IncludeNotFound, include), TypeOfMessage.warning);
						}
					}
					//DeFreezeState
					currentFile = tmpCurrentFile;
					currentLine = tmpCurrentLine;
					this.file = tmpFile;
					endToken = tmpEnd;
					startToken = tmpStart;
				}
			}
		}
	}

	/// <summary>
	/// Parsa i file xml
	/// </summary>
	//=========================================================================
	public class XMLParser : Parser
	{
		private DictionaryContainer xmlWriter = new DictionaryContainer();

		//--------------------------------------------------------------------------------
		public XMLParser(Logger logWriter)
			: base(logWriter)
		{
		}

		//--------------------------------------------------------------------------------
		public virtual string ResourceType
		{
			get { return AllStrings.xml; }
		}

		//--------------------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument { get { return xmlWriter; } }

		/// <summary>
		///Parsa tutti i file xml(estensioni scelte manualmente)nella cartella del progetto
		/// </summary>
		//---------------------------------------------------------------------
		public override void Parse(string file)
		{
			xmlWriter.Id = Path.GetFileNameWithoutExtension(file);
			xmlWriter.WriteResource(ResourceType, Path.GetFileNameWithoutExtension(file).ToLower());
			foreach (XmlNode node in XmlStringsReader(file))
				xmlWriter.WriteString(node.Value);
			xmlWriter.CloseResource();

		}

		/// <summary>
		/// Restituisce un arrayList con le stringhe del file xml taggate 'localizable'
		/// e con i value degli attributi localize.
		/// </summary>nome del file da parsare</param>
		/// <param name="logWriter">scrittore del log</param>
		//---------------------------------------------------------------------
		public ArrayList XmlStringsReader(string file)
		{
			ArrayList stringsToLocalize = new ArrayList();
			LocalizerDocument reader = new LocalizerDocument();
			try
			{
				if (!File.Exists(file)) return stringsToLocalize;
				reader.Load(file);
			}
			catch (Exception)
			{
				logWriter.WriteLog(String.Format(Strings.NotConsistentXml, file), TypeOfMessage.warning);
				return stringsToLocalize;
			}
			try
			{
				AddStrings
					(
					reader,
					String.Concat
					(
					"//",
					AllStrings.nodeFunction,
					"[@",
					AllStrings.localizable,
					"='",
					AllStrings.trueTag,
					"']"
					),
					stringsToLocalize
					);

				AddStrings
					(
					reader,
					String.Concat
					(
					"//@",
					AllStrings.localize
					),
					stringsToLocalize
					);

			}
			catch (Exception)
			{
				string message = String.Format(Strings.NotConsistentXml, Path.GetFileName(file));
				logWriter.WriteLog(message, TypeOfMessage.warning);
				return stringsToLocalize;
			}
			return stringsToLocalize;
		}

		//---------------------------------------------------------------------
		private void AddStrings(LocalizerDocument doc, string selectString, ArrayList stringsToLocalize)
		{
			XmlNodeList listLocalizable = doc.SelectNodes(selectString);
			if (listLocalizable != null)
			{
				foreach (XmlNode node in listLocalizable)
				{
					XmlNodeList li = node.ChildNodes;
					foreach (XmlNode n in li)
					{
						if (n.NodeType == XmlNodeType.Text)
						{
							//scrivo la stringa identificandola col nome del nodo cui appartiene
							stringsToLocalize.Add(n);
							break;
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Parsa i file DBInfo (xml)
	/// </summary>
	//=========================================================================
	public class DBInfoParser : XMLParser
	{
		//--------------------------------------------------------------------------------
		public DBInfoParser(Logger logWriter)
			: base(logWriter)
		{
		}

		//--------------------------------------------------------------------------------
		public override string ResourceType
		{
			get { return AllStrings.dbinfo; }
		}

	}

	/// <summary>
	/// Parsa i file di woorm
	/// </summary>
	//=========================================================================
	public class WRMParser : Parser
	{
		DictionaryContainer wrmWriter = new DictionaryContainer();
		ArrayList additionalWriters = new ArrayList();

		//--------------------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument { get { return wrmWriter; } }

		//--------------------------------------------------------------------------------
		public WRMParser(Logger logWriter)
			: base(logWriter)
		{
		}

		/// <summary>
		/// Parsa i file wrm contenuti nella cartella report.
		/// </summary>
		/// <param name="wrmFiles">path del file da parsare</param>
		/// <param name="logWriter">scrittore del log</param>
		//---------------------------------------------------------------------
		public override void Parse(string file)
		{
			try
			{
				wrmWriter.Id = Path.GetFileNameWithoutExtension(file);
				ArrayList listOfStrings = new ArrayList();
				ArrayList listOfReferences = new ArrayList();
				ArrayList dialogs = new ArrayList();
				ArrayList localizableStrings = new ArrayList();

				FormatStyles formats;
				FontStyles fonts;
				LookWoormDocument(file, listOfStrings, localizableStrings, listOfReferences, out formats, out fonts);

				if (listOfStrings.Count > 0 || listOfReferences.Count > 0)
				{
					wrmWriter.WriteResource(AllStrings.report, AllStrings.reportIdentifier);

					foreach (string text in listOfStrings)
						wrmWriter.WriteString(text);

					foreach (string text in listOfReferences)
						if (text != null && text.Trim() != String.Empty)
							wrmWriter.WriteReferenceString(text);

					wrmWriter.CloseResource();
				}



				// estrae le stringhe dei font custom definiti nel report
				if (fonts != null && fonts.Count != 0)
				{
					FormatAndFontParser p = new FormatAndFontParser
						(
						logWriter,
						FormatAndFontParser.Type.FONT,
						fonts
						);
					p.Parse(null);
					if (p.DictionaryDocument.Count > 0)
						additionalWriters.Add(p.DictionaryDocument);
				}

				// estrae le stringhe dei formattatori custom definiti nel report
				if (formats != null && formats.Count != 0)
				{
					FormatAndFontParser p = new FormatAndFontParser
						(
						logWriter,
						FormatAndFontParser.Type.FORMAT,
						formats
						);
					p.Parse(null);
					if (p.DictionaryDocument.Count > 0)
						additionalWriters.Add(p.DictionaryDocument);
				}

				LookWoormEngine(file, dialogs, localizableStrings, logWriter);

				if (dialogs != null && dialogs.Count > 0)
				{
					foreach (AskDialog ad in dialogs)
						ParseAskDialog(ad, wrmWriter);
				}

				if (localizableStrings != null && localizableStrings.Count > 0)
					WriteLocalizableStrings(localizableStrings, wrmWriter);

			}
			catch (Exception ex)
			{
				logWriter.WriteLog(ex.Message, TypeOfMessage.error);
			}
		}


		//---------------------------------------------------------------------
		public override void Save(DictionaryFileCollection document, string dictionaryRoot, string dictionaryFileName)
		{
			DictionaryDocument.Save(document[dictionaryFileName], dictionaryRoot, logWriter);
			foreach (DictionaryContainer d in additionalWriters)
				d.Save(document[dictionaryFileName], dictionaryRoot, logWriter);

		}

		/// <summary>
		/// Parsa le askDialog dei report
		/// </summary>
		/// <param name="ad">dialog da parsare</param>
		/// <param name="wrmWriter">datadocument ce scrive il dizionario per i report</param>
		//---------------------------------------------------------------------
		private void ParseAskDialog(AskDialog ad, DictionaryContainer wrmWriter)
		{
			wrmWriter.WriteResource(AllStrings.report, ad.FormName.ToLower());
			wrmWriter.WriteString(ad.FormTitle);

			foreach (AskGroup ag in ad.Groups)
			{
				wrmWriter.WriteString(ag.Caption);
				foreach (AskEntry ae in ag.Entries)
					wrmWriter.WriteString(ae.Caption);
			}
			wrmWriter.CloseResource();
		}

		//---------------------------------------------------------------------
		private void WriteLocalizableStrings(ArrayList localizableStrings, DictionaryContainer wrmWriter)
		{
			wrmWriter.WriteResource(AllStrings.report, AllStrings.reportLocalizable);

			foreach (string s in localizableStrings)
			{
				wrmWriter.WriteString(s);
			}

			wrmWriter.CloseResource();
		}

		/// <summary>
		/// Crea un woormDocument e itera nei suoi oggetti.
		/// </summary>
		/// <param name="file">file da parsare</param>
		/// <param name="a">lista di stringhe per il layout</param>
		/// <param name="b">lista di path di file referenziati</param>
		//---------------------------------------------------------------------
		private void LookWoormDocument(
			string file,
			ArrayList graphicLocalizableStrings,
			ArrayList functionLocalizableStrings,
			ArrayList localizableReferences,
			out FormatStyles formats,
			out FontStyles fonts
			)
		{
			WoormDocument doc = new WoormDocument(file, CommonFunctions.GetSession(), null, null);
			doc.ForLocalizer = true; // inibisce il type checking e la valutazione delle espressioni di "hidden when"
			bool loaded = doc.LoadDocument() && doc.ParseDocument();
			if (!loaded)
			{
				IDiagnosticItems diser = doc.Lex.Diagnostic.AllMessages(DiagnosticType.Error);
				if (diser != null)
				{
					foreach (DiagnosticItem item in diser)
						logWriter.WriteLog(Parser.BuildMessage(item), TypeOfMessage.error);
				}

				fonts = null;
				formats = null;
				return;
			}
			if (doc.Lex.Diagnostic.Warning)
			{
				foreach (DiagnosticItem item in doc.Lex.Diagnostic.AllMessages(DiagnosticType.Warning))
					logWriter.WriteLog(Parser.BuildMessage(item), TypeOfMessage.warning);
			}
			doc.GetLocalizableStrings(graphicLocalizableStrings, functionLocalizableStrings, localizableReferences);

			fonts = doc.FontStyles;
			formats = doc.FormatStyles;
		}



		/// <summary>
		/// Parsa le dialog del report.
		/// </summary>
		/// <param name="file">file da parsare</param>
		/// <param name="a">lista di stringhe della dialog</param>
		//---------------------------------------------------------------------
		public void LookWoormEngine(string file, ArrayList rules, ArrayList strings, Logger logWriter)
		{
			Report report = new Report(file, CommonFunctions.GetSession(), "", "");
			report.ForLocalizer = true;//inibisce alcune operazioni, inutili ed impossibili senza connessione
			if (report.Compile())
			{
				rules.AddRange(report.AskingRules);
				strings.AddRange(report.GetLocalizableStrings());
				if (report.Lex.Diagnostic.Warning)
				{
					foreach (DiagnosticItem item in report.Lex.Diagnostic.AllMessages(DiagnosticType.Warning))
						logWriter.WriteLog(Parser.BuildMessage(item), TypeOfMessage.warning);
				}
			}
			else
			{
				logWriter.WriteLog(string.Format("Error parsing engine section for report '{0}'", file), TypeOfMessage.error);
				foreach (DiagnosticItem item in report.Lex.Diagnostic.AllMessages(DiagnosticType.Error))
					logWriter.WriteLog(Parser.BuildMessage(item), TypeOfMessage.error);
			}
		}
	}

	/// <summary>
	/// Parsa i file di script SQL
	/// </summary>
	//=========================================================================
	public class SQLParser : Parser
	{
		LexanParser parser;
		DictionaryContainer sqlWriter = new DictionaryContainer();

		enum SQLToken
		{
			TABLE = Token.USR00,
			CREATE = Token.USR01,
			DBO = Token.USR02,
			CONSTRAINT = Token.USR03,
			PRIMARY = Token.USR04,
			FOREIGN = Token.USR05,
			KEY = Token.USR06,
			REFERENCES = Token.USR07,
			OBJECT_ID = Token.USR08,
			ALTER = Token.USR09,
			GO = Token.USR10,
			ADD = Token.USR11,
			END = Token.USR12,
			DELETE = Token.USR13,
			INSERT = Token.USR14,
			INTO = Token.USR15,
			DOT = Token.USR16,
			INDEX = Token.USR17,
			VIEW = Token.USR18,
			AS = Token.USR19,
			SELECT = Token.USR20
		}

		public struct DataBaseObjectInfo
		{
			public string TableName;
			public string[] CloumnNames;
		}

		//---------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument
		{
			get
			{
				return sqlWriter;
			}
		}

		//---------------------------------------------------------------------
		public SQLParser(Logger logWriter)
			: base(logWriter)
		{

			parser = new LexanParser(LexanParser.SourceType.FromFile);
			parser.PreprocessorDisabled = true;
			parser.DoAudit = true;

			parser.UserKeywords.Add("TABLE", SQLToken.TABLE);
			parser.UserKeywords.Add("CREATE", SQLToken.CREATE);
			parser.UserKeywords.Add("dbo", SQLToken.DBO);
			parser.UserKeywords.Add("CONSTRAINT", SQLToken.CONSTRAINT);
			parser.UserKeywords.Add("PRIMARY", SQLToken.PRIMARY);
			parser.UserKeywords.Add("FOREIGN", SQLToken.FOREIGN);
			parser.UserKeywords.Add("KEY", SQLToken.KEY);
			parser.UserKeywords.Add("REFERENCES", SQLToken.REFERENCES);
			parser.UserKeywords.Add("object_id", SQLToken.OBJECT_ID);
			parser.UserKeywords.Add("ALTER", SQLToken.ALTER);
			parser.UserKeywords.Add("GO", SQLToken.GO);
			parser.UserKeywords.Add("ADD", SQLToken.ADD);
			parser.UserKeywords.Add("END", SQLToken.END);
			parser.UserKeywords.Add("DELETE", SQLToken.DELETE);
			parser.UserKeywords.Add("INSERT", SQLToken.INSERT);
			parser.UserKeywords.Add("INTO", SQLToken.INTO);
			parser.UserKeywords.Add(".", SQLToken.DOT);
			parser.UserKeywords.Add("INDEX", SQLToken.INDEX);
			parser.UserKeywords.Add("VIEW", SQLToken.VIEW);
			parser.UserKeywords.Add("AS", SQLToken.AS);
			parser.UserKeywords.Add("SELECT", SQLToken.SELECT);
		}

		/// <summary>
		///Parsa tutti i file Sql
		/// </summary>
		//---------------------------------------------------------------------
		public override void Parse(string file)
		{
			sqlWriter.Id = AllStrings.sqlScripts;

			DataBaseObjectInfo[] objs = ParseInfo(file);
			if (objs.Length == 0)
				return;

			foreach (DataBaseObjectInfo obj in objs)
			{
				sqlWriter.WriteResource(AllStrings.sqlScript, obj.TableName.ToLower());
				sqlWriter.WriteString(obj.TableName);
				foreach (string aString in obj.CloumnNames)
					sqlWriter.WriteString(aString);
				sqlWriter.CloseResource();
			}
		}

		//---------------------------------------------------------------------------------------------------
		public static bool IsOracleScript(string file)
		{
			if (file == null || file == String.Empty)
				return false;

			return (file.ToLower().Replace("/", "\\").IndexOf("\\oracle\\") != -1);
		}

		//---------------------------------------------------------------------------------------------------
		DataBaseObjectInfo[] ParseInfo(string file)
		{
			parser.Open(file);

			ArrayList objects = new ArrayList();

			while (!parser.Eof)
			{
				switch (parser.LookAhead())
				{
					case (Token)SQLToken.CREATE:
						parser.SkipToken();
						if (parser.LookAhead() == (Token)SQLToken.TABLE)
						{
							parser.SkipToken();
							DataBaseObjectInfo info;
							if (ParseCreateTable(out info))
								objects.Add(info);
							else
								foreach (DiagnosticItem item in parser.Diagnostic.AllMessages(DiagnosticType.Error))
									logWriter.WriteLog(Parser.BuildMessage(item), TypeOfMessage.error);
						}
						else if (parser.LookAhead() == (Token)SQLToken.VIEW)
						{
							parser.SkipToken();
							DataBaseObjectInfo info;
							if (ParseCreateView(out info))
								objects.Add(info);
							else
								foreach (DiagnosticItem item in parser.Diagnostic.AllMessages(DiagnosticType.Error))
									logWriter.WriteLog(Parser.BuildMessage(item), TypeOfMessage.error);
						}
						break;
					case (Token)SQLToken.ALTER:
						parser.SkipToken();
						if (parser.LookAhead() == (Token)SQLToken.TABLE)
						{
							parser.SkipToken();
							DataBaseObjectInfo info;
							if (ParseAlterTable(out info))
								objects.Add(info);
							else
								foreach (DiagnosticItem item in parser.Diagnostic.AllMessages(DiagnosticType.Error))
									logWriter.WriteLog(Parser.BuildMessage(item), TypeOfMessage.error);
						}
						break;
					default:
						parser.SkipToken();
						break;
				}
			}

			parser.Close();

			return (DataBaseObjectInfo[])objects.ToArray(typeof(DataBaseObjectInfo));
		}

		//---------------------------------------------------------------------------------------------------
		public bool ParseCreateTable(out DataBaseObjectInfo info)
		{
			string tableName = string.Empty, columnName;
			ArrayList columnNames = new ArrayList();
			try
			{
				if (!ParseTable(out tableName)) return false;

				if (!parser.ParseOpen()) return false;

				Token t;

				while ((t = parser.LookAhead()) != Token.ROUNDCLOSE && !parser.Eof)
				{
					parser.SkipToken();
					switch (t)
					{
						case (Token)SQLToken.CONSTRAINT:
							return true;

						case Token.SQUAREOPEN:
							if (!ParseColumn(tableName, out columnName))
								return false;
							columnNames.Add(columnName);
							break;
					}
				}
			}
			catch
			{
				return false;
			}
			finally
			{
				info = new DataBaseObjectInfo();
				info.CloumnNames = (string[])columnNames.ToArray(typeof(string));
				info.TableName = tableName;
			}

			return true;
		}

		//---------------------------------------------------------------------------------------------------
		public bool ParseAlterTable(out DataBaseObjectInfo info)
		{
			string tableName = string.Empty, columnName;
			ArrayList columnNames = new ArrayList();

			try
			{
				if (!ParseTable(out tableName)) return false;

				if (!parser.ParseTag((Token)SQLToken.ADD)) return true;

				Token t;
				bool skipName = false;
				while (
					(t = parser.LookAhead()) != (Token)SQLToken.GO &&
					t != (Token)SQLToken.END &&
					!parser.Eof
					)
				{
					parser.SkipToken();
					switch (t)
					{
						case (Token)SQLToken.CONSTRAINT:
							skipName = true;
							break;

						case Token.SQUAREOPEN:
							if (!skipName)
							{
								if (!ParseColumn(tableName, out columnName))
									return false;
								columnNames.Add(columnName);
							}
							skipName = false;
							break;
						default:
							skipName = false;
							break;
					}
				}
			}
			catch
			{
				return false;
			}
			finally
			{
				info = new DataBaseObjectInfo();
				info.CloumnNames = (string[])columnNames.ToArray(typeof(string));
				info.TableName = tableName;
			}

			return true;
		}

		//---------------------------------------------------------------------------------------------------
		bool ExistColumn(ArrayList columnNames, string column)
		{
			foreach (string s in columnNames)
				if (string.Compare(s, column, true) == 0) return true;

			return false;
		}

		//---------------------------------------------------------------------------------------------------
		public bool ParseCreateView(out DataBaseObjectInfo info)
		{
			string viewName = string.Empty, columnName = string.Empty, tmpToken;
			ArrayList columnNames = new ArrayList();
			try
			{
				if (!parser.ParseID(out viewName)) return false;

				if (parser.Parsed(Token.OPEN))
				{
					while (!parser.Eof && !parser.Parsed(Token.CLOSE))
					{
						if (!parser.ParseID(out columnName))
							return false;

						if (!ExistColumn(columnNames, columnName))
							columnNames.Add(columnName);
						parser.Parsed(Token.COMMA);
					}
				}

				if (
					!parser.ParseTag((Token)SQLToken.AS) ||
					!parser.ParseTag((Token)SQLToken.SELECT)
					) return false;

				Token t;

				while ((t = parser.LookAhead()) != Token.FROM && !parser.Eof)
				{
					if (t == (Token)SQLToken.AS)
					{
						parser.SkipToken();
						if (!parser.ParseID(out columnName))
							return false;
						if (!ExistColumn(columnNames, columnName))
							columnNames.Add(columnName);
						continue;
					}

					if (t != Token.ID)
					{
						parser.SkipToken();
						continue;
					}

					parser.ParseID(out tmpToken);
					int index = tmpToken.IndexOf(".");
					if (index != -1 && index < tmpToken.Length)
						columnName = tmpToken.Substring(index + 1);
					else
						columnName = tmpToken;

					t = parser.LookAhead();
					switch (t)
					{
						case Token.FROM:
						case Token.COMMA:
							if (columnName == string.Empty)
								return false;
							columnNames.Add(columnName);
							break;
					}
				}
			}
			catch
			{
				return false;
			}
			finally
			{
				info = new DataBaseObjectInfo();
				info.CloumnNames = (string[])columnNames.ToArray(typeof(string));
				info.TableName = viewName;
			}

			return true;
		}

		//---------------------------------------------------------------------------------------------------
		public bool ParseTable(out string tableName)
		{
			tableName = string.Empty;

			// mi aspetto i seguenti tokens: 
			// [dbo].[<nome_tabella>]
			return parser.ParseSquareOpen() &&
				parser.ParseTag((Token)SQLToken.DBO) &&
				parser.ParseSquareClose() &&
				parser.ParseTag((Token)SQLToken.DOT) &&
				parser.ParseSquareOpen() &&
				parser.ParseID(out tableName) &&
				parser.ParseSquareClose();
		}

		//---------------------------------------------------------------------------------------------------
		public bool ParseColumn(string tableName, out string columnName)
		{
			columnName = String.Empty;
			switch (parser.LookAhead())
			{
				case Token.ID:
					if (!parser.ParseID(out columnName)) return false;
					break;
				case Token.SQUARECLOSE:
					if (!parser.ParseSquareClose()) return false;
					break;
				default:
					//serve per evitare di andare in syntax error se una colonna si chiama come
					//uno dei nostri token (es: path, mail, note, Quantity,  etc.)
					columnName = parser.LookAhead().ToString();
					parser.SkipToken();
					break;
			}
			int roundToClose = 1;

			bool goOn = true;
			while (goOn && !parser.Eof)
			{
				switch (parser.LookAhead())
				{
					case (Token)SQLToken.GO: goOn = false; break;
					case (Token)SQLToken.CONSTRAINT: goto default;//goOn = false;					break;
					case (Token)SQLToken.END: goOn = false; break;
					case Token.COMMA: goOn = (roundToClose) > 1; goto default;
					case Token.ROUNDOPEN: roundToClose++; goto default;
					case Token.ROUNDCLOSE: goOn = (--roundToClose) > 0; goto default;
					default: parser.SkipToken(); break;
				};
			}

			return columnName != string.Empty;
		}
	}

	//================================================================================
	public struct OutputPath
	{
		public string Configuration { get; set; }
		public string Path { get; set; }
	}

	//================================================================================
	public class CSProjectParser : Parser
	{
		LocalizerDocument prjDocument;
		XmlNamespaceManager nsManager;
		ProjectDocument tblPrjWriter;
		string assemblyName;
		string nameSpace;
		int version = 2;
		string frameworkVersion = "v4.0";
		OutputPath[] outputPaths;

		//--------------------------------------------------------------------------------
		public OutputPath[] OutputPaths
		{
			get { return outputPaths; }
		}

		//--------------------------------------------------------------------------------
		public string AssemblyName
		{
			get { return assemblyName; }
		}

		//--------------------------------------------------------------------------------
		public string NameSpace
		{
			get { return nameSpace; }
		}
		//--------------------------------------------------------------------------------
		public string Version
		{
			get { return version.ToString(); }
		}

		//--------------------------------------------------------------------------------
		public string FrameworkVersion
		{
			get { return frameworkVersion; }
		}
		//--------------------------------------------------------------------------------
		public CSProjectParser(Logger logWriter, ProjectDocument tblPrjWriter)
			: base(logWriter)
		{
			this.tblPrjWriter = tblPrjWriter;
		}


		//--------------------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument
		{
			get
			{
				return null;
			}
		}

		//--------------------------------------------------------------------------------
		public override void Parse(string file)
		{
			prjDocument = new LocalizerDocument();
			prjDocument.Load(file);
			nsManager = new XmlNamespaceManager(prjDocument.NameTable);
			nsManager.AddNamespace("ms", "http://schemas.microsoft.com/developer/msbuild/2003");
			assemblyName = ReadAssemblyName();
			nameSpace = ReadNamespace();
			frameworkVersion = ReadFrameworkVersion();
			ReadOutputPaths();
		}

		//--------------------------------------------------------------------------------
		public override void Save(DictionaryFileCollection document, string dictionaryRoot, string dictionaryFileName)
		{
			Debug.Assert(tblPrjWriter != null);
			tblPrjWriter.WriteNode(AllStrings.namespaceTag, NameSpace, true);
			tblPrjWriter.WriteNode(AllStrings.assemblyTag, assemblyName, true);
			tblPrjWriter.WriteNode(AllStrings.versionTag, Version, true);
			tblPrjWriter.WriteNode(AllStrings.frameworkVersionTag, FrameworkVersion, true);
			tblPrjWriter.WriteObject(AllStrings.outputPaths, OutputPaths, true);
			tblPrjWriter.Save();
		}

		//--------------------------------------------------------------------------------
		private string ReadFrameworkVersion()
		{
			//2005-2008 version of file
			XmlNode namespaceNode = prjDocument.SelectSingleNode("ms:Project/ms:PropertyGroup/ms:TargetFrameworkVersion/text()", nsManager);
			if (namespaceNode != null)
			{
				version = 2;
				return namespaceNode.Value;
			}
			//.Net core version of file
			namespaceNode = prjDocument.SelectSingleNode("Project/PropertyGroup/TargetFramework/text()");
			if (namespaceNode != null)
			{
				version = 3;
				return namespaceNode.Value;
			}
			//2003 version of file
			version = 1;
			return "v1.1";
		}
		//--------------------------------------------------------------------------------
		private string ReadAssemblyName()
		{
			//2005-2008 version of file
			XmlNode assemblyNode = prjDocument.SelectSingleNode("ms:Project/ms:PropertyGroup/ms:AssemblyName/text()", nsManager);

			//.Net core version of file
			if (assemblyNode == null)
			{
				assemblyNode = prjDocument.SelectSingleNode("Project/PropertyGroup/AssemblyName/text()");
			}

			//2003 version of file
			if (assemblyNode == null)
			{
				assemblyNode = prjDocument.SelectSingleNode("//Build/Settings/@AssemblyName");
				version = 1;
			}

			return (assemblyNode == null) ? String.Empty : assemblyNode.Value;
		}

		//--------------------------------------------------------------------------------
		private string ReadNamespace()
		{
			//2005-2008 version of file
			XmlNode namespaceNode = prjDocument.SelectSingleNode("ms:Project/ms:PropertyGroup/ms:RootNamespace/text()", nsManager);

			//.Net core version of file
			if (namespaceNode == null)
			{
				namespaceNode = prjDocument.SelectSingleNode("Project/PropertyGroup/RootNamespace/text()");
			}
			//2003 version of file
			if (namespaceNode == null)
			{
				namespaceNode = prjDocument.SelectSingleNode("//Build/Settings/@RootNamespace");
				version = 1;
			}

			return (namespaceNode == null) ? String.Empty : namespaceNode.Value;
		}
		//--------------------------------------------------------------------------------
		private void ReadOutputPaths()
		{
			List<OutputPath> paths = new List<OutputPath>();
			XmlNodeList nodes = prjDocument.SelectNodes("ms:Project/ms:PropertyGroup/ms:OutputPath/text()", nsManager);

			//.Net core version of file
			if (nodes.Count == 0)
			{
				nodes = prjDocument.SelectNodes("Project/PropertyGroup/OutputPath/text()", nsManager);
			}
			//2003 version of file
			if (nodes.Count == 0)
			{
				nodes = prjDocument.SelectNodes("//Build/Settings/Config/@OutputPath");
				if (nodes.Count == 0)
				{
					//.Net core version of file with defaults
					OutputPath op = new OutputPath();

					op.Path = "bin\\Debug\\netcoreapp2.0\\";
					op.Configuration = "Debug";
					paths.Add(op);

					op = new OutputPath();

					op.Path = "bin\\Release\\netcoreapp2.0\\";
					op.Configuration = "Release";
					paths.Add(op);
				}
				else
				{
					version = 1;
					foreach (XmlNode node in nodes)
					{
						OutputPath op = new OutputPath();

						op.Path = node.Value;
						op.Configuration = ((XmlAttribute)node).OwnerElement.GetAttribute("Name");
						paths.Add(op);
					}
				}
			}
			else //2005-2008 version of file
			{
				foreach (XmlNode node in nodes)
				{
					string v = node.Value;
					string c = ((XmlElement)node.ParentNode.ParentNode).GetAttribute("Condition");
					Match m = Regex.Match(c, "==\\s*'((\\|)?(?<config>\\w+))+'");
					if (m.Success)
					{
						foreach (Capture cc in m.Groups["config"].Captures)
						{
							if (!cc.Value.CompareNoCase("AnyCPU"))
							{
								OutputPath op = new OutputPath();
								op.Configuration = cc.Value;
								op.Path = v;
								paths.Add(op);
							}
						}
					}
				}
			}

			outputPaths = paths.ToArray();
		}

		//--------------------------------------------------------------------------------
		internal string[] ReadReferences()
		{
			//2003 version of file
			XmlNodeList listOfReferences = prjDocument.SelectNodes
				(
				"//" +
				AllStrings.referencesCap +
				"/" +
				AllStrings.referenceCap +
				"/@" +
				AllStrings.nameCap
				);

			if (listOfReferences.Count == 0)
				//2005-2008 version of file
				listOfReferences = prjDocument.SelectNodes
						(
						"//ms:" +
						AllStrings.projectReference +
						"/ms:" +
						AllStrings.nameCap +
						"/text()",
						nsManager
						);
			if (listOfReferences.Count == 0)
				//2017 version of file
				listOfReferences = prjDocument.SelectNodes
						(
						"//" +
						AllStrings.projectReference +
						"/@" +
						AllStrings.include
						);
			List<string> list = new List<string>();
			foreach (XmlNode n in listOfReferences)
				list.Add(n.Value);

			return list.ToArray();
		}
	}

	//================================================================================
	public class ResXParser : Parser
	{
		private string filename;
		private string sourceRoot;
		ProjectDocument tblPrjWriter;

		//--------------------------------------------------------------------------------
		public ResXParser
			(
			Logger logWriter,
			ProjectDocument tblPrjWriter,
			string sourceRoot
			)
			: base(logWriter)
		{
			this.tblPrjWriter = tblPrjWriter;
			this.sourceRoot = sourceRoot;
		}

		//--------------------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument
		{
			get
			{
				return null;
			}
		}

		//--------------------------------------------------------------------------------
		public override void Parse(string file)
		{
			this.filename = file;
		}

		//--------------------------------------------------------------------------------
		public override void Save(DictionaryFileCollection document, string dictionaryRoot, string dictionaryFileName)
		{
			document[filename] = new ResxDictionaryFile(filename, sourceRoot, tblPrjWriter, logWriter);
		}


	}

	//================================================================================
	public class JSONParser : Parser
	{
		private DictionaryContainer jsonWriter = new DictionaryContainer();
		//--------------------------------------------------------------------------------
		public JSONParser
			(
			Logger logWriter
			)
			: base(logWriter)
		{
		}



		//--------------------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument { get { return jsonWriter; } }

		//--------------------------------------------------------------------------------
		public virtual string ResourceType { get { return AllStrings.jsonforms; } }

		enum ParseState { NONE, EXPECT_TEXT, EXPECT_CONTEXT }
		/// <summary>
		///Parsa tutti i file json nella cartella del progetto
		/// </summary>
		//---------------------------------------------------------------------
		public override void Parse(string file)
		{
			ParseState state = ParseState.NONE;
			string folder = Path.GetDirectoryName(file);
			string context = Path.GetFileName(folder).ToLower();
			if (context.Equals(AllStrings.jsonforms, StringComparison.InvariantCultureIgnoreCase))
			{
				folder = Path.GetDirectoryName(folder);
				context = Path.GetFileName(folder).ToLower();
			}
			jsonWriter.Id = context;
			List<String> strings = new List<string>();
			using (StreamReader sr = new StreamReader(file))
			{
				jsonWriter.WriteResource(ResourceType, Path.GetFileNameWithoutExtension(file));
				using (JsonTextReader reader = new JsonTextReader(sr))
				{
					while (reader.Read())
					{
						switch (reader.TokenType)
						{
							case JsonToken.PropertyName:
								if (reader.Value.Equals("text") || reader.Value.Equals("hint") || reader.Value.Equals("controlCaption"))
									state = ParseState.EXPECT_TEXT;
								else if (reader.Value.Equals("context"))
									state = ParseState.EXPECT_CONTEXT;
								break;
							case JsonToken.String:
								switch (state)
								{
									case ParseState.NONE:
										break;
									case ParseState.EXPECT_TEXT:
										strings.Add(reader.Value.ToString());
										//jsonWriter.WriteString(reader.Value.ToString());
										state = ParseState.NONE;
										break;
									case ParseState.EXPECT_CONTEXT:
										jsonWriter.Id = reader.Value.ToString();
										state = ParseState.NONE;
										break;
									default:
										break;
								}
								break;
						}

					}
					foreach (var s in strings)
					{
						if (s.StartsWith("@"))
							continue;//alias
						if (s.StartsWith("{{") && s.EndsWith("}}"))
							continue;//espressione
						jsonWriter.WriteString(s);
					}
					jsonWriter.CloseResource();
				}
			}
		}
	}


	/// <summary>
	/// Parsa i file cpp
	/// </summary>
	//=========================================================================
	public class WebContentParser : Parser
	{
		class StringInfos : List<string>
		{
			public string ComponentUrl = "";
			public string ClassName = "";
		}

		public enum WebToken
		{
			CLASS = Token.USR00,
			_TB = Token.USR01,
			COMPONENT = Token.USR02,
			TEMPLATEURL = Token.USR03,
			NULLTOKEN = Token.USR99
		}
		List<StringInfos> strings = new List<StringInfos>();

		/// <summary>documento che scrive il dizionario per i file sorgenti</summary>
		protected DictionaryContainer writer = new DictionaryContainer();

		//--------------------------------------------------------------------------------
		public override DictionaryContainer DictionaryDocument { get { return writer; } }

		//--------------------------------------------------------------------------------
		public virtual string ResourceType { get { return AllStrings.web; } }

		//---------------------------------------------------------------------
		public WebContentParser(Logger logWriter)
			: base(logWriter)
		{
			lex.UserKeywords.Add("class", WebToken.CLASS);
			lex.UserKeywords.Add("_TB", WebToken._TB);
			lex.UserKeywords.Add("this._TB", WebToken._TB);
			lex.UserKeywords.Add("@Component", WebToken.COMPONENT);
			lex.UserKeywords.Add("templateUrl", WebToken.TEMPLATEURL);
		}


		/// <summary>
		/// Parsa un file .ts
		/// </summary>
		/// <param name="file">path del file da parsare</param>
		/// <param name="loadFile">specifica se caricare il file che è già stato creato</param>
		//---------------------------------------------------------------------
		public override void Parse(string f)
		{
			writer.Id = AllStrings.web;

			ExtractTSStrings(f);
			foreach (var info in strings)
			{
				if (!string.IsNullOrEmpty(info.ComponentUrl))
				{
					string htmlFile = Path.Combine(Path.GetDirectoryName(f), info.ComponentUrl);
					if (File.Exists(htmlFile))
						ExtractHTMLStrings(htmlFile, info);
				}
				if (info.Count == 0)
					continue;
				if (string.IsNullOrEmpty(info.ClassName))
					throw new Exception(string.Format("Cannot parse typescript class name in file {}", f));

				writer.WriteResource(AllStrings.web, info.ClassName);

				foreach (var s in info)
				{
					writer.WriteString(s);
				}
				writer.CloseResource();
			}
			
		}
		StringInfos GetStringInfo(string className)
		{
			foreach (var item in strings)
			{
				if (item.ClassName == className)
					return item;
			}
			var infos = new StringInfos();
			infos.ClassName = className;
			strings.Add(infos);
			return infos;
		}

		private void ExtractTSStrings(string f)
		{
			try
			{
				string content = File.ReadAllText(f, Encoding.UTF8);
				lex.Open(content);
				while (!lex.Eof)
				{
					if (lex.Parsed((Token)WebToken.COMPONENT) && lex.Parsed(Token.ROUNDOPEN))
					{
						string templateUrl;
						if (ParseComponent(out templateUrl))
						{
							lex.SkipToToken((Token)WebToken.CLASS);
							StringInfos infos = ParseClass();
							if (!string.IsNullOrEmpty(templateUrl))
								infos.ComponentUrl = templateUrl;
						}
					}
					else if (lex.LookAhead((Token)WebToken.CLASS))
					{
						ParseClass();
					}
					else
					{
						lex.SkipToken();
					}
				}

			}
			finally
			{
				lex.Close();
			}
		}


		private void ExtractHTMLStrings(string f, StringInfos infos)
		{
			try
			{
				string content = File.ReadAllText(f, Encoding.UTF8);
				lex.Open(content);
				while (!lex.Eof)
				{
					if (lex.LookAhead((Token)WebToken._TB))
					{
						ParseString(infos);
						continue;
					}

					lex.SkipToken();
				}

			}
			finally
			{
				lex.Close();
			}
		}
		private bool ParseComponent(out string templateUrl)
		{
			templateUrl = "";
			bool componentOk = false;
			while (!lex.Eof)
			{
				if (lex.Parsed(Token.ROUNDCLOSE))
				{
					componentOk = true;
					break;
				}
				if (lex.Parsed((Token)WebToken.TEMPLATEURL))
				{
					if (lex.Parsed(Token.COLON))
						lex.ParseString(out templateUrl);
				}
				lex.SkipToken();
			}
			return componentOk;
		}

		private StringInfos ParseClass()
		{
			string className = "";
			StringInfos infos = null;
			if (!lex.Parsed((Token)WebToken.CLASS))
				return infos;

			if (lex.LookAhead() == Token.ID)
			{
				lex.ParseID(out className);

				infos = GetStringInfo(className);
				if (lex.SkipToToken(Token.BRACEOPEN, true, false))
				{
					int level = 1;
					while (level > 0 && !lex.Eof)
					{
						if (lex.LookAhead(Token.BRACEOPEN))
						{
							level++;
						}
						else if (lex.LookAhead(Token.BRACECLOSE))
						{
							level--;
						}
						else if (lex.LookAhead((Token)WebToken._TB))
						{
							ParseString(infos);
							continue;
						}

						lex.SkipToken();
					}
				}

			}
			else
			{
				lex.SkipToken();
			}
			return infos;
		}

		private void ParseString(StringInfos infos)
		{
			lex.SkipToken();

			if (lex.Parsed(Token.ROUNDOPEN))
			{
				string s;
				if (lex.ParseString(out s))
					infos.Add(s);
			}
		}
	}
	//================================================================================
	public class ResourceItem
	{
		string name;
		double id;
		string url;

		//--------------------------------------------------------------------------------
		public string Name { get { return name; } }
		//--------------------------------------------------------------------------------
		public double Id { get { return id; } }
		//--------------------------------------------------------------------------------
		public string Url { get { return url; } }

		//--------------------------------------------------------------------------------
		public ResourceItem(string name, double id, string url)
		{
			this.name = name;
			this.id = id;
			this.url = url;
		}

	}
	//================================================================================
	public class ResourceBlock : ArrayList
	{
		string type;

		//--------------------------------------------------------------------------------
		public string Type { get { return type; } }
		//--------------------------------------------------------------------------------
		public ResourceBlock(string type)
		{
			this.type = type;
		}

	}

	//================================================================================
	public class ResourceIndexContainer : ArrayList
	{
		//--------------------------------------------------------------------------------
		public void AddResource(string name, double id, string fileName, string typeOfResource)
		{
			ResourceBlock block = null;
			foreach (ResourceBlock rb in this)
			{
				if (string.Compare(rb.Type, typeOfResource, true) == 0)
				{
					block = rb;
					break;
				}
			}

			if (block == null)
			{
				block = new ResourceBlock(typeOfResource);
				Add(block);
			}

			block.Add(new ResourceItem(name, id, fileName));
		}

		//--------------------------------------------------------------------------------
		public void Save(string root, Logger logWriter)
		{
			ResourceIndexDocument document = new ResourceIndexDocument(root);
			if (!document.LoadIndex())
				document.InitDocument(AllStrings.resources);
			foreach (ResourceBlock block in this)
			{
				if (block.Count == 0)
					continue;

				foreach (ResourceItem item in block)
					document.AddResource(item.Name, item.Id, item.Url, block.Type, logWriter);
			}

			document.SaveAndLogError(logWriter);
		}
	}

	//================================================================================
	public class StringBlock : ArrayList
	{
		string name;
		string type;

		ArrayList references = null;

		//--------------------------------------------------------------------------------
		public string Name { get { return name; } }
		//--------------------------------------------------------------------------------
		public string Type { get { return type; } }
		//--------------------------------------------------------------------------------
		public ArrayList References { get { return references; } }

		//--------------------------------------------------------------------------------
		public StringBlock(string name, string type)
		{
			this.name = name;
			this.type = type;
		}

		//--------------------------------------------------------------------------------
		public void AddReference(string aString)
		{
			if (references == null)
				references = new ArrayList();
			aString = aString.ToLower();
			if (!references.Contains(aString))
				references.Add(aString);
		}

	}

	//================================================================================
	public class DictionaryContainer : ArrayList
	{
		StringBlock currentBlock;
		string id = "";

		//--------------------------------------------------------------------------------
		public void WriteString(string aString)
		{
			Debug.Assert(currentBlock != null, "Write resource method has not been called yet!");
			if (!currentBlock.Contains(aString))
				currentBlock.Add(aString);
		}

		//--------------------------------------------------------------------------------
		public void WriteReferenceString(string aString)
		{
			Debug.Assert(currentBlock != null, "Write resource method has not been called yet!");
			currentBlock.AddReference(aString);
		}

		//--------------------------------------------------------------------------------
		public string Id { get { return id; } set { id = value.ToLower(); } }

		//--------------------------------------------------------------------------------
		public void WriteResource(string resourceType, string resourceName)
		{
			currentBlock = new StringBlock(resourceName, resourceType);
		}

		//--------------------------------------------------------------------------------
		public void CloseResource()
		{
			Debug.Assert(currentBlock != null, "Write resource method has not been called yet!");
			if (currentBlock.Count > 0)
				Add(currentBlock);
		}

		//--------------------------------------------------------------------------------
		public void Save(DictionaryFile dictionaryFile, string root, Logger logWriter)
		{
			foreach (StringBlock block in this)
			{
				DictionaryDocument document = dictionaryFile.GetResource(Id, block.Type, root);
				document.WriteResource(block.Type, block.Name);
				foreach (string s in block)
					document.WriteString(s);

				if (block.References != null)
				{
					foreach (string reference in block.References)
					{
						try
						{

							string filePath = CommonFunctions.MapFile(reference);
							string targetString = Path.GetFileNameWithoutExtension(reference) + '.' + CommonFunctions.GetCulture(root) + Path.GetExtension(reference);
							//controlla: se file scrivi in target il nome del file
							string folderDest = Path.Combine(root, AllStrings.report);
							string fileDest = Path.Combine(folderDest, targetString);
							if (!File.Exists(fileDest))
							{
								if (!Directory.Exists(folderDest))
									Directory.CreateDirectory(folderDest);
								CommonUtilities.Functions.SafeCopyFile(filePath, fileDest, false);
							}

							XmlElement stringNode = document.WriteString(reference);
							if (stringNode != null)
							{
								stringNode.SetAttribute(AllStrings.target, targetString);
								stringNode.SetAttribute(AllStrings.file, AllStrings.trueTag);
							}
						}
						catch (Exception ex)
						{
							logWriter.WriteLog(string.Format(Strings.ErrorWritingReference, ex.Message), TypeOfMessage.error);
						}
					}
				}

				document.CloseResource();

			}
		}
	}
}
