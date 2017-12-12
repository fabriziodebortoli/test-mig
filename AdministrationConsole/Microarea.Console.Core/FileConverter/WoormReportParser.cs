using System.Collections;
using System.IO;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.FileConverter
{
	/// <summary>
	/// Summary description for WoormReportParser.
	/// </summary>
	//================================================================================
	public class WoormReportParser : WRMReportTableParser
	{
		public static bool BackupFile = false;
		private int stepProcess = 1;

		private const int woormRelease = 6;	//NB: i primi step portano la release da 3 a 6, l'ultimo step da 6 a 7
		private bool invalid = false;
		private bool noWeb = false;

		private bool pageLayOutStarted = false;
		private bool pageLayOutClosed = false;
		private bool procedureStarted = false;
		private bool procedureClosed = false;
		private bool linkStarted = false;
		private bool linkClosed = false;
		private bool existEvents = false;
		private bool missingFinalizeEvent = true;
		private int lastEndPos = -1;
		private long releaseLineIndex = -1;	//mi serve per inserire a posteriori il tag NoWeb

		private int maxAlias = 0;

		//gestione dei contesti
		ArrayList contextNamespaceCol = new ArrayList(); // collection dei namespace delle funzioni di contesto di Create/Dispose
		ArrayList contextHandleNameCol = new ArrayList(); // collection dei nomi di handle di contesto da aggiungere come variabili
		ArrayList contextMissingDisposeCol = new ArrayList(); // collection per aggiungere le Dispose a quei contesti che non avevano una analogo metodo di pulizia

		//gestione variabili valorizzate tramite woorminfo
		//spostata nella classe base		Hashtable mapId = new Hashtable();	//variabili del report
		ArrayList newVariables = new ArrayList(); //variabili mancanti al report (quelle derivate dalla valorizzazione del woorminfo)
		ArrayList newVariableTypes = new ArrayList(); //tipo delle variabili mancanti al report (quelle derivate dalla valorizzazione del woorminfo)

		MigrationNetStep netStep = null;

		protected bool AddWoormInfoVariables(string v, string t)
		{
			if (v != string.Empty)
				for (int i=0; i < newVariables.Count; i++)
					if (string.Compare(newVariables[i] as string, v, true) == 0)
						return false;
			newVariables.Add(v);
			newVariableTypes.Add(t);
			return true;
		}

		//--------------------------------------------------------------------------------
		public WoormReportParser(string fileName, string destinationFileName) : base(fileName, destinationFileName)
		{
			netStep = new MigrationNetStep(this);
			checkLogErrorsToValidateParsing = true;

			FunctionTranslator ft = (FunctionTranslator) GlobalContext.FunctionTranslator;
			ft.translateVar = new FunctionTranslator.DTranslateString(GlobalContext.WoormInfoVariablesTranslator.TranslateWoormInfoVariable);
		}

		//--------------------------------------------------------------------------------
		protected IFontTranslator FontTranslator { get { return GlobalContext.FontTranslator; }}
		//--------------------------------------------------------------------------------
		protected IFormatterTranslator FormatterTranslator { get { return GlobalContext.FormatterTranslator; }}

		//--------------------------------------------------------------------------------
		public override bool Parse()
		{
			if (BackupFile)
			{
				File.Copy(fileName, Path.ChangeExtension(fileName, ".bak"), true);
				GlobalContext.LogManager.Message(FileConverterStrings.FileBackup, string.Empty, DiagnosticType.Information, null);
			}
			
			// per non complicare il codice (e in alcuni casi perché il parser è forward-only), faccio due passate al report;
			// prima processo gli elementi più strutturati, facilmente prevedibili
			// poi faccio una seconda passata per gli elementi meno strutturati,
			// ad esempio le funzioni esterne (per le quali non so a priori dove possano trovarsi)
			stepProcess = 1;
			encoding = System.Text.Encoding.Default;
			bool successStep1 = base.Parse();
				
			stepProcess++;
			modified = false;
			
			// la seconda passata deve aprire il file di destinazione
			fileName = destinationFileName;

			bool successStep2 = base.Parse();

			stepProcess++;
			modified = false;

			bool successStep3 = base.Parse();

			stepProcess++;
			modified = false;

			bool successStep4 = base.Parse();

			stepProcess++;
			modified = false;

			bool successStep5 = base.Parse();

			stepProcess++;
			modified = false;

			bool successStep6 = base.Parse();

			return successStep1 && successStep2 && successStep3 && successStep4 && successStep5 && successStep6; 
		}

		//--------------------------------------------------------------------------------
		protected override void ProcessBuffer()
		{
			if (stepProcess == 1)
			{
				GlobalContext.LogManager.Message(string.Format(FileConverterStrings.RunningStep, stepProcess, fileName), string.Empty, DiagnosticType.Information, null);
				ProcessBuffer1();
			}
			else if (stepProcess == 2)
			{
				GlobalContext.LogManager.Message(string.Format(FileConverterStrings.RunningStep, stepProcess, fileName), string.Empty, DiagnosticType.Information, null);
				ProcessBuffer2();
			}
			else if (stepProcess == 3)
			{
				GlobalContext.LogManager.Message(string.Format(FileConverterStrings.RunningStep, stepProcess, fileName), string.Empty, DiagnosticType.Information, null);
				ProcessBuffer3();
			}
			else if (stepProcess == 4)
			{
				GlobalContext.LogManager.Message(string.Format(FileConverterStrings.RunningStep, stepProcess, fileName), string.Empty, DiagnosticType.Information, null);
				ProcessBuffer4();
			}
			else  if (stepProcess == 5)
			{
				GlobalContext.LogManager.Message(string.Format(FileConverterStrings.RunningStep, stepProcess, fileName), string.Empty, DiagnosticType.Information, null);
				netStep.ProcessBufferCommon();
			}
			else if (stepProcess == 6)
			{
				GlobalContext.LogManager.Message(string.Format(FileConverterStrings.RunningStep, stepProcess, fileName), string.Empty, DiagnosticType.Information, null);
				netStep.ProcessBufferCommon2();
			}
		}

		//--------------------------------------------------------------------------------
		// modifica gli elementi più strutturati
		private void ProcessBuffer1()
		{
			encoding = System.Text.Encoding.UTF8;
			
			while (!parser.Eof)
			{
				Token t = parser.LookAhead();
				CheckForPageLayout(t);
				switch (t)
				{
					case Token.RELEASE:
					{
						ParseRelease();
						break;
					}
					case Token.UUID:
					{
						ParseUUID();
						break;
					}

					case Token.FORMATSTYLES:
					{
						parser.SkipToken();
						if (parser.LookAhead() == Token.BEGIN)
							parser.SkipToToken(Token.END, false,false);
						else
							parser.SkipToToken(Token.SEP, false,false);
						break;
					}

					case Token.RULES:
					{
						ParseRules();
						break;
					}

					case Token.EVENTS:
					{
						parser.SkipToken();

						existEvents = true;
						break;
					}
			
					case Token.DIALOG:
					{
						if (procedureStarted && !procedureClosed)
						{
							ReplaceWord(parser.CurrentPos, string.Empty, "End/*Procedures*/\r\n");
							procedureClosed = true;
						}

						ParseAskDialog(true);
						break;
					}

					case Token.PROCEDURE:
					{
						parser.SkipToken();

						if (!procedureStarted)
						{
							ReplaceWord(parser.CurrentPos, string.Empty, "Procedures\r\nBegin\r\n");
							procedureStarted = true;
						}
						break;
					}

					case Token.SELECT:
					{
						if (!pageLayOutStarted)
						{
							ReplaceWord(parser.CurrentPos, string.Empty, "PageLayout \"Default\"\r\nBegin\r\nEnd\r\n");
							pageLayOutStarted = true;
							pageLayOutClosed = true;
						}
						else if (!pageLayOutClosed)
						{
							ReplaceWord(parser.CurrentPos, string.Empty, "End/*PageLayout*/\r\n");
							pageLayOutClosed = true;
						}

						parser.SkipToken();

						if (!linkStarted)
						{
							ReplaceWord(parser.CurrentPos, "Select", "Links\r\nBegin\r\n", true);
							linkStarted = true;
						}
						else
							ReplaceWord(parser.CurrentPos, "Select", " ", true);

						if (parser.LookAhead() == Token.LINKFORM )
							ParseLink(true, false);
						else if (parser.LookAhead() == Token.LINKREPORT )
							ParseLink(false, false);
						else
						{
							ReplaceWord(parser.CurrentPos, string.Empty, "LinkRadar", true);
							ParseLink(true, true);
						}

						break;
					}

					case Token.REPORT:
					{
						if (!pageLayOutStarted)
						{
							ReplaceWord(parser.CurrentPos, string.Empty, "PageLayout \"Default\"\r\nBegin\r\nEnd\r\n");
							pageLayOutStarted = true;
							pageLayOutClosed = true;
						}
						else  if (!pageLayOutClosed)
						{
							ReplaceWord(parser.CurrentPos, string.Empty, "End/*PageLayout*/\r\n");
							pageLayOutClosed = true;
						}

						parser.SkipToken();

						if (linkStarted && !linkClosed)
						{
							ReplaceWord(parser.CurrentPos, string.Empty, "End/*Links*/\r\n");
							linkClosed = true;
						}

						break;
					}
			
					case Token.OPTIONS:
					{
						parser.SkipToken();
						parser.Parsed(Token.BEGIN); //opzionale
						if (parser.Parsed(Token.BITMAP))
						{
							ParseFilePath(false);
						}
						break;
					}
					case Token.BITMAP:
					{
						ParseGraphObject();
						break;
					}
					case Token.FILE:
					{
						ParseFileObject();
						break;
					}

					case Token.ID:
					{
						//ricerca funzioni esterne con contesto per inserimento di Create e Dispose allo step 3
						CheckID(); 
						break;
					}
					case Token.ALIAS:
					{
						//determino il massimo valore di Alias utilizzato nel report
						CheckMaxAlias(); 
						break;
					}
					case Token.END:
					{
						//determino il massimo valore di Alias utilizzato nel report
						long nLastEnd = this.currentLineIndex;
						parser.SkipToken(); 
						if (parser.LookAhead() == Token.EOF)
						{
							if (procedureStarted && !procedureClosed)
							{
								ReplaceWord(nLastEnd, 0, string.Empty, "End/*Procedures*/\r\n");
								procedureClosed = true;
							}
						}
						break;
					}

					default:
					{
						parser.SkipToken();
						break;
					}
				}
			}
		}

		//---------------------------------------------------------------------------------------------------
		override protected void SkipRule()
		{
			do 
			{ 
				if (parser.LookAhead() == Token.EOF) 
					break;
				
				if (parser.LookAhead() == Token.ID)
				{
					CheckID();
					continue;
				}

				if (parser.LookAhead() != Token.SEP)
					parser.SkipToken();	
			}
			while (!parser.Parsed(Token.SEP));
		}

		//--------------------------------------------------------------------------------
		// modifica gli elementi meno strutturati
		private void ProcessBuffer2()
		{
			while (!parser.Eof)
			{
				switch (parser.LookAhead())
				{
					case Token.ASK:
					{
						ParseAsk();
						break;
					}

					case Token.MESSAGE:
					{
						ParseMessage();
						break;
					}

					case Token.ID:
					{
						ParseID(); //possibile funzione esterna
						break;
					}

					case Token.FROM:
					{
						parser.SkipToken();
						TranslateSplittedTables();
						break;
					}

					case Token.SQUAREOPEN:
					{
						parser.SkipToken();
						if (!parser.LookAhead(Token.TEXTSTRING)) break;

						string enumName = string.Empty;
						if (!parser.ParseString(out enumName)) break;
						string enumNewName = GlobalContext.EnumTranslator.TranslateEnumType(enumName);
						if (enumNewName == string.Empty)
						{
							string msg = string.Format("Tipo enumerativo '{0}' non più supportato.", enumName);
							GlobalContext.LogManager.Message
								(
								msg,
								string.Empty, DiagnosticType.Error, new ExtendedInfo("Linea:", parser.CurrentLine)
								);
							ReplaceWord(parser.CurrentPos, string.Empty, string.Format("/*ATTENZIONE {0}*/\r\n", msg), true);

							InsertInvalid();
						}
						else if (enumNewName != enumName) 
						{
							ReplaceWord(parser.CurrentPos + 1, enumName, enumNewName, true);
						}
						
						parser.ParseTag(Token.SQUARECLOSE);
						break;
					}

					case Token.BRACEOPEN:
					{
						parser.SkipToken();
						if (!parser.LookAhead(Token.TEXTSTRING)) break;
						
						string enumName = string.Empty;
						string enumValue = string.Empty;
						if (!parser.ParseString(out enumName)) break;
						string enumNewName = GlobalContext.EnumTranslator.TranslateEnumType(enumName);
						if (enumNewName == string.Empty)
						{
							string msg = string.Format("Tipo enumerativo '{0}' non più supportato.", enumName);
							GlobalContext.LogManager.Message
								(
								msg,
								string.Empty, DiagnosticType.Error, new ExtendedInfo("Linea:", parser.CurrentLine)
								);
							ReplaceWord(parser.CurrentPos, string.Empty, string.Format("/*ATTENZIONE {0}*/\r\n", msg), true);
	
							parser.ParseTag(Token.COLON);
							parser.ParseString(out enumValue);
							parser.ParseTag(Token.BRACECLOSE);

							InsertInvalid();
							break;
						}						
						else if (enumNewName != enumName)
						{
							ReplaceWord(parser.CurrentPos + 1, enumName, enumNewName, true);
						}
	
						if (!parser.ParseTag(Token.COLON)) break;
						
						if (!parser.ParseString(out enumValue)) break;
						string enumNewValueName = GlobalContext.EnumTranslator.TranslateEnumValue(enumName, enumValue);
						if (enumNewValueName == string.Empty)
						{
							string msg = string.Format("Valore enumerativo '{{\"{0}\":\"{1}\"}}' non più supportato.", enumName, enumValue);
							GlobalContext.LogManager.Message
								(
								msg,
								string.Empty, DiagnosticType.Error, new ExtendedInfo("Linea:", parser.CurrentLine)
								);
							ReplaceWord(parser.CurrentPos, string.Empty, string.Format("\r\n/*ATTENZIONE {0}*/\r\n", msg), true);
							
							InsertInvalid();
						}
						else if (enumNewValueName != enumValue)
						{
							ReplaceWord(parser.CurrentPos + 1, enumValue, enumNewValueName, true);
						}

						parser.ParseTag(Token.BRACECLOSE);
						break;
					}

					default:
					{
						parser.SkipToken();
						break;
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		// modifica gli elementi più strutturati
		private void ProcessBuffer3()
		{
			while (!parser.Eof)
			{
				Token t = parser.LookAhead();
				switch (t)
				{
					case Token.VAR:
					{	
						//append variabili handle di contesto con init = ..._Create()
						ParseVariables(); 
						break;
					}
					case Token.EVENTS:
					{	
						//inserimento evento finalize per dispose dei contesti
						parser.SkipToken();

						if (parser.LookAhead(Token.FORMFEED))
						{	
							ParseGroupEvents();
						}

						if (parser.LookAhead(Token.REPORT)) 
						{
							ParseReportEvents(); 
						}
						else
						{
							//devo aggiungere il gruppo report
							InsertReportEventFinalize(true, false);
						}
						break;
					}

					case Token.DIALOGS:
					{
						parser.SkipToken();

						if (!existEvents)
						{
							InsertReportEventFinalize(true, true);
							existEvents = true;
						}
						break;
					}
					case Token.PROCEDURES:
					{
						parser.SkipToken();

						if (!existEvents)
						{
							InsertReportEventFinalize(true, true);
							existEvents = true;
						}
						break;
					}
					case Token.END:
					{
						lastEndPos = parser.CurrentPos;
						parser.SkipToken();

						if (parser.LookAhead(Token.EOF) && !existEvents)
						{
							InsertReportEventFinalize(true, true);
						}
						break;
					}

					default:
					{
						parser.SkipToken();
						break;
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void ProcessBuffer4()
		{
			while (!parser.Eof)
			{
				Token t = parser.LookAhead();
				switch (t)
				{
					case Token.ID:
					{	
						ParseWoormInfoMultiParamFunctionID();
						break;
					}

					case Token.RUNREPORT:
					{
						ParseRunReportFunction();
						break;
					}

					default:
					{
						parser.SkipToken();
						break;
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void CheckForPageLayout(Token t)
		{
			if (pageLayOutClosed) return;

			switch(t)
			{
				case Token.TABLE    : 
				case Token.RNDRECT  : 
				case Token.SQRRECT  : 
				case Token.TEXT     : 
				case Token.BITMAP    : 
				case Token.METAFILE : 
				case Token.FILE     : 
				case Token.FIELD    : 
				{
					if (!pageLayOutStarted)
					{
						ReplaceWord(parser.CurrentPos, string.Empty, "PageLayout \"Default\"\r\nBegin\r\n");
						pageLayOutStarted = true;
					}
					break;
				}
				case Token.EOF     : 
				{
					if (pageLayOutStarted && !pageLayOutClosed)
					{
						ReplaceWord(parser.CurrentPos, string.Empty, "\r\nEnd/*PageLayout*/\r\n");
						pageLayOutClosed = true;
					}
					break;
				}
			}
		}


		//--------------------------------------------------------------------------------
		protected void ParseFilePath(bool isText)
		{
			string name;
			parser.ParseString(out name);
			string newName = GlobalContext.FileObjectTranslator.TranslateFile(name, isText, this.fileName,	GlobalContext.ReportExternalPaths, GlobalContext.ReportsData);

			if (newName != string.Empty)
				ReplaceWord(parser.CurrentPos + 1, name, newName, true);
			else
				GlobalContext.LogManager.Message
					(
					string.Format(FileConverterStrings.RenameFilePathFailed, name),
					string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
					);
		}

		//--------------------------------------------------------------------------------
		protected void ParseFileObject()
		{
			parser.SkipToken();
			if (parser.LookAhead() != Token.ROUNDOPEN)
			{
				//potrebbe essere l'attributo File di un oggetto Field 
				//significa "mostra il contenuto del file il cui path/namespace è il corrente valore del Field(Rect)"
				return;
			}
			SkipRect();
			SkipRatio();

			ParseFilePath(true);
		}

		//--------------------------------------------------------------------------------
		protected void ParseGraphObject()
		{
			parser.SkipToken();
			if (parser.LookAhead() != Token.ROUNDOPEN)
			{
				//potrebbe essere l'attributo Bitmap di un oggetto Field 
				//significa "mostra l'immagine il cui path/namespace è il corrente valore del Field(Rect)"
				return;
			}
			SkipRect();

			parser.Parsed(Token.PROPORTIONAL);
			if (parser.Parsed(Token.RECT))
				SkipRect();

			ParseFilePath(false);
		}


		//--------------------------------------------------------------------------------
		protected void ParseAsk()
		{
			parser.SkipToken(); //ask;

			string name;
			if (!parser.ParseID(out name)) //nome ask dialog
				throw new FileConverterException(FileConverterStrings.ReadingAsk, parser.Filename, parser.CurrentLine, parser.CurrentPos);
		}

		//--------------------------------------------------------------------------------
		protected void ParseRunReportFunction()
		{
			InsertInvalid();

			parser.ParseTag(Token.RUNREPORT);

//			ReplaceWord
//				(
//				parser.CurrentPos, 
//				string.Empty, 
//				"/*ATTENZIONE : occorre aggiornare il namespace del report e i nomi degli eventuali parametri*/\r\n", 
//				false, DiagnosticType.Error 
//				);

			parser.ParseTag(Token.ROUNDOPEN);

			//tipicamente la chiamata è del tipo:
			//... RunReport (GetReportPath() + "child-report | flag1=1|flag2=2", p1, p2)
			//... RunReport ("path/child-report | flag1=1|flag2=2", p1, p2)

			//mi posiziono sulla virgola che precede il primo parametro se esiste
			if (!parser.SkipToToken(new Token[] { Token.COMMA, Token.ROUNDCLOSE }, false, true, true))
			{
				return; 
			}

			//aggiungo i segnaposto per i nomi dei parametri
			if (!parser.Parsed(Token.ROUNDCLOSE))
			{
				do 
				{
					if (parser.Parsed(Token.COMMA))
					{
						ReplaceWord(parser.CurrentPos, 
							",", 
							",\r\n" +
							"/*ATTENZIONE*/\"<Sostituire con il nome del campo del report chiamato da valorizzare con il valore del parametro successivo>\", ",
							true);
					}
					else
						parser.SkipToken();
				} while (!parser.Parsed(Token.ROUNDCLOSE) && !parser.LookAhead(Token.EOF));
			}
		}

		//--------------------------------------------------------------------------------
		protected void CheckID()
		{
			int functionPosition = parser.CurrentPos;
			long functionLine = parser.CurrentLine;

			string name;
			if (!parser.ParseID(out name)) //funzione;
				throw new FileConverterException(FileConverterStrings.RenameIDException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
		
			// se non è seguito da (, non si tratta di funzione
			if (!parser.Parsed(Token.ROUNDOPEN))
			{
				if (!mapId.Contains(name.ToLower())) 
					mapId.Add(name.ToLower(), null); // 1
				return;
			}

			if (GlobalContext.FunctionTranslator.IsWoorminfoFunction(name))
			{
				string [] vars; string [] types;
				string retvar; string rettype;

				if (GlobalContext.FunctionTranslator.RetrieveSubstituteReturnValue(name, out retvar, out rettype))
				{
					AddWoormInfoVariables(retvar, rettype);
				}
				if (GlobalContext.FunctionTranslator.RetrieveSubstituteVariables(name, out vars, out types))
				{
					if (!GlobalContext.FunctionTranslator.IsWoorminfoReturnValueFunction(name))
						AddWoormInfoVariables("AskDialog", "Bool");
					for (int i= 0; i < vars.GetLength(0); i++)
					{
						AddWoormInfoVariables(vars[i], types[i]);
					}
				}
				return;
			}
//			else if (GlobalContext.FunctionTranslator.IsDocumentFunction(name))
//			{
//				return;
//			}
			else if (GlobalContext.FunctionTranslator.IsFunctionWithContext(name))
			{
				//si tratta di funzione con contesto, devo aggiungere un handle di contesto
				string contextNamespace, contextHandleName, functionName;
				if (SplitContextNamespace(GlobalContext.FunctionTranslator.TranslateFunction(name, GlobalContext.ReportsData.Module), out contextNamespace, out contextHandleName, out functionName))
				{
					for (int i=0; i < contextHandleNameCol.Count; i++)
					{
						if (string.Compare(contextHandleName, contextHandleNameCol[i] as string, true) == 0)
						{
							if (string.Compare(functionName, "Dispose", true) == 0)
								contextMissingDisposeCol[i] = false;
							return;
						}
					}
					contextHandleNameCol.Add(contextHandleName);
					contextNamespaceCol.Add(contextNamespace);
					contextMissingDisposeCol.Add(string.Compare(functionName, "Dispose", true) != 0);
				}
			}
		}

		//--------------------------------------------------------------------------------
		protected bool SplitContextNamespace(string contextFunction, out string contextNamespace, out string contextHandleName, out string functionName)
		{
			contextNamespace = string.Empty;
			contextHandleName = string.Empty;
			functionName  = string.Empty;
			if (contextFunction == string.Empty) 
				return false;

			string handleName = string.Empty;
			int np = contextFunction.LastIndexOf(".");
			if (np < -1)
				return false;
			int n_ = contextFunction.LastIndexOf("_");
			if (n_ < -1)
				return false;
			if (n_ < np)
				return false;

			functionName = contextFunction.Substring(n_ + 1);;
			contextNamespace = contextFunction.Substring(0, n_);
			contextHandleName = "h_" + contextFunction.Substring(np + 1, n_ - np -1);
			return true;
		}

		//--------------------------------------------------------------------------------
		protected void ParseID()
		{
			int functionPosition = parser.CurrentPos;
			long functionLine = parser.CurrentLine;

			string name;
			if (!parser.ParseID(out name)) //funzione;
				throw new FileConverterException(FileConverterStrings.RenameIDException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
		
			if (this.ParseSoundCommand(name))
				return;
	
			// se non è seguito da ( non si tratta di funzione
			if (!parser.Parsed(Token.ROUNDOPEN))
				return;
			int nPosRoundOpen = parser.CurrentPos;

			if (IsNoWebFunction(name))
				InsertNoWeb();
	
			bool isUselessFunction = GlobalContext.FunctionTranslator.IsUnsupportedFunction(name);
			bool isFunctionWithContext = GlobalContext.FunctionTranslator.IsFunctionWithContext(name);

			if (isUselessFunction && !isFunctionWithContext)
			{
				// la funzione è cambiata o è stata rimossa
				string newName = GlobalContext.FunctionTranslator.TranslateFunction(name, GlobalContext.ReportsData.Module);
				string comment;
				
				if (string.Compare(name, "RunReport", true) == 0)
					comment = string.Format
						(
						"/*ATTENZIONE la funzione {0} necessita di un intervento manuale.\n {1}*/\n{2}", 
						name, 
						GlobalContext.FunctionTranslator.GetDescription(name),
						newName
						);
				else if (GlobalContext.FunctionTranslator.IsWoorminfoReturnValueFunction(name)) 
					comment = string.Format
						(
						"/*ATTENZIONE funzione {0} non più supportata.\n {1}*/\n{2}", 
						name, 
						GlobalContext.FunctionTranslator.GetDescription(name), 
						GlobalContext.FunctionTranslator.GetReturnValueName(name)
						);
				else 
					comment = string.Format
						(
						"/*ATTENZIONE funzione {0} non più supportata.\n {1}*/\n{2}", 
						name, 
						GlobalContext.FunctionTranslator.GetDescription(name), 
						newName != string.Empty ? newName : name
						)
					;

				ReplaceWord(functionLine, functionPosition, name, comment, true, DiagnosticType.Warning);

				if (GlobalContext.FunctionTranslator.IsWoorminfoReturnValueFunction(name))
				{
					ReplaceWord(parser.CurrentPos, "(", string.Empty, true);
					if (!parser.Parsed(Token.ROUNDCLOSE))
						return;
					ReplaceWord(parser.CurrentPos, ")", string.Empty, true);
				}
				else
				{
					if (GlobalContext.FunctionTranslator.IsWoorminfoFunction(name))
					{
						return;
					}

					GlobalContext.LogManager.Message
						(
						string.Format(FileConverterStrings.UnsupportedFunction, name, GlobalContext.FunctionTranslator.GetDescription(name)),
						string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
						);
					InsertInvalid();

					if (GlobalContext.FunctionTranslator.IsDocumentFunction(name))
						ReplaceWord(parser.CurrentPos, "(", "(OwnerID, ", true);
				}
			}
			else if (isUselessFunction && isFunctionWithContext)
			{
				// la funzione è cambiata o è stata rimossa
				string newName = GlobalContext.FunctionTranslator.TranslateFunction(name, GlobalContext.ReportsData.Module);
				string comment = string.Format
					(
					"/*ATTENZIONE funzione {0} non più supportata. {1}*/{2}", 
					name, 
					GlobalContext.FunctionTranslator.GetDescription(name), 
					newName
					);

				if (newName != string.Empty)
				{
					//funzione con contesto con numero di parametri differente
					string contextNamespace, contextHandleName, fname;
					if (SplitContextNamespace(newName, out contextNamespace, out contextHandleName, out fname))
					{
						if (string.Compare(fname, "Create", true) != 0)
						{
							GlobalContext.LogManager.Message
								(
								string.Format(FileConverterStrings.UnsupportedFunction, name, GlobalContext.FunctionTranslator.GetDescription(name)),
								string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
								);

							ReplaceWord(functionLine, functionPosition, name, comment, true, DiagnosticType.Warning);

							if (parser.LookAhead() != Token.ROUNDCLOSE)
								ReplaceWord(nPosRoundOpen, "(", "(" + contextHandleName + ", ", true);
							else
								ReplaceWord(nPosRoundOpen, "(", "(" + contextHandleName + " ", true);

							InsertInvalid();
						}
						else
						{
							comment = string.Format
								(
								"/*ATTENZIONE funzione {0} non più supportata. {1} Viene sostituita con la {2} nella sezione 'Variables'*/", 
								name, 
								GlobalContext.FunctionTranslator.GetDescription(name), 
								newName
								);
							ReplaceWord
								(
									functionLine, functionPosition, 
								name, comment, true, 
								GlobalContext.FunctionTranslator.NeedOtherParameters(name) ? DiagnosticType.Error : DiagnosticType.Warning
								);
							if (GlobalContext.FunctionTranslator.NeedOtherParameters(name))
								InsertInvalid();

							ReplaceWord(nPosRoundOpen, "(", "/*(" , true);
							bool bRet = parser.SkipToToken(Token.ROUNDCLOSE, true, false);
							ReplaceWord(parser.CurrentPos, ")", ")*/ TRUE", true);
						}
					}
				}
				else
				{
					GlobalContext.LogManager.Message
						(
						string.Format(FileConverterStrings.UnsupportedFunction, name, GlobalContext.FunctionTranslator.GetDescription(name)),
						string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
						);

					ReplaceWord(functionLine, functionPosition, name, comment, true, DiagnosticType.Warning);
					
					InsertInvalid();
				}
			}
			else if (GlobalContext.FunctionTranslator.IsDocumentFunction(name))
			{
				InsertNoWeb();

				string comment = string.Empty;
				if (GlobalContext.FunctionTranslator.IsOverloadedDocumentFunction(name))
					comment = string.Format
						(
						"/*ATTENZIONE Esistono differenti versioni della funzione {0}: occore indicare il namespace del documento chiamante.\n{1}\n */\n", 
						name, 
						GlobalContext.FunctionTranslator.GetDescription(name)
						);
				string newName = GlobalContext.FunctionTranslator.TranslateFunction(name, GlobalContext.ReportsData.Module);
				if (newName != string.Empty)
				{
					if (comment != string.Empty)
						comment += string.Format("/*E' stato scelto il namespace in base al modulo {0} in cui si sta importando il report*/\n", GlobalContext.ReportsData.Module);

					ReplaceWord(functionLine, functionPosition, name, comment + newName, 
						true,
						comment != string.Empty ? DiagnosticType.Warning : DiagnosticType.Information
						);
				}
				else
				{
					ReplaceWord(functionLine, functionPosition, name, comment + name + "\n", true, DiagnosticType.Error);
				}
				//si tratta di funzione di documento, devo aggiungere OwnerId ai parametri
				ReplaceWord(parser.CurrentPos, "(", "(OwnerID, ", true);
			}
			else if (GlobalContext.FunctionTranslator.IsFunctionWithContext(name))
			{
				//si tratta di funzione con contesto, devo aggiungere un handle di contesto come primo parametro
				string newName = GlobalContext.FunctionTranslator.TranslateFunction(name, GlobalContext.ReportsData.Module);
				if (newName != string.Empty)
					ReplaceWord(functionLine, functionPosition, name, newName, true);
				else
					GlobalContext.LogManager.Message
						(
						string.Format(FileConverterStrings.FirstUnsupportedFunctionWithContext, name, GlobalContext.FunctionTranslator.GetDescription(name)),
						string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
						);

				string contextNamespace, contextHandleName, fname;
				if (SplitContextNamespace(GlobalContext.FunctionTranslator.TranslateFunction(name, GlobalContext.ReportsData.Module), out contextNamespace, out contextHandleName, out fname))
				{
					if (string.Compare(fname, "Create", true) == 0)
					{
						GlobalContext.LogManager.Message
							(
							string.Format(FileConverterStrings.SplitContextNamespace, name, GlobalContext.FunctionTranslator.GetDescription(name)),
							string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
							);
					}
					else if (parser.LookAhead() == Token.ROUNDCLOSE)
						ReplaceWord(parser.CurrentPos, string.Empty, contextHandleName + " ", true); //ha un solo parametro
					else 
						ReplaceWord(parser.CurrentPos, string.Empty, contextHandleName + ", ", true);
				}
				else
					GlobalContext.LogManager.Message
						(
						string.Format(FileConverterStrings.SecondUnsupportedFunctionWithContext, name, GlobalContext.FunctionTranslator.GetDescription(name)),
						string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
						);
			}
			else
			{
				//è una normale funzione che ha cambiato nome
				string newName = GlobalContext.FunctionTranslator.TranslateFunction(name, GlobalContext.ReportsData.Module);
				if (newName.EndsWith(")"))
				{
					ReplaceWord(
						nPosRoundOpen, 
						"(", 
						"/*(",
						true
						);

					ReplaceWord(
						functionLine, functionPosition, 
						name, 
						newName,
						true
						);

					if (parser.SkipToToken(new Token [] {Token.ROUNDCLOSE}, false, true, true))
					{
						ReplaceWord(
							parser.CurrentPos, 
							")", 
							")*/",
							true
							);
					}
				}
				else
				{
					ReplaceWord(
						functionLine, functionPosition, 
						name, 
						newName,
						true
						);
				}
			}
		}

		//--------------------------------------------------------------------------------
		protected void ParseWoormInfoMultiParamFunctionID()
		{
			int functionPosition = parser.CurrentPos;
			long functionLine = parser.CurrentLine;

			string name;
			if (!parser.ParseID(out name)) //funzione;
				throw new FileConverterException(FileConverterStrings.RenameIDException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
			
			// se non è seguito da (, non si tratta di funzione
			if (!parser.Parsed(Token.ROUNDOPEN))
				return;
			int nPosRoundOpen = parser.CurrentPos;
			if (!GlobalContext.FunctionTranslator.IsWoorminfoFunction(name))
				return;

			if (GlobalContext.FunctionTranslator.IsWoorminfoReturnValueFunction(name))
			{
				string newName = GlobalContext.FunctionTranslator.GetReturnValueName(name);
				if (newName == string.Empty)
					GlobalContext.LogManager.Message
						(
						string.Format("La funzione {0} è descritta male nei file di migrazione, manca il nome del campo che viene valorizzato in automatico al posto della sua chiamata", name),
						string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
						);

				ReplaceWord(functionLine, functionPosition, name, newName, true);
				return;
			}

			if 
				(
					GlobalContext.FunctionTranslator.IsWoorminfoFunction(name) &&
					!GlobalContext.FunctionTranslator.IsWoorminfoReturnValueFunction(name)
				)
			{
				if (RemoveFunctionCall(name, functionPosition, functionLine))
					return;

				GlobalContext.LogManager.Message
					(
					string.Format("Occorre un intervento manuale per la conversione della chiamata alla funzione {0}", name),
					string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
					);

				InsertInvalid();
			}
		}

		//--------------------------------------------------------------------------------
		/*
		 * Euristiche:
		 * If funzione (p1, p2, p3, ...) Is Null Then (Begin ... End | <cmd> ; ) [ Else (Begin ... End | <cmd> ; ) ]
		 * -> If |* ...*| |*(p1, p2, p3, ...)*| AskDialog == FALSE Then ... Else Begin p1 = v1; p2 = v2; p3 = v3; ... End ;
		 * */
		private bool RemoveFunctionCall(string name, int functionPosition, long functionLine)
		{
			ReplaceWord(functionLine, functionPosition, name, "/*" + name, true);

			string newName = GlobalContext.FunctionTranslator.GetReturnValueName(name);
			if (newName == string.Empty)
				newName = "TRUE";

			ArrayList actParams = new ArrayList();
			string actPar;

//			ReplaceWord(parser.CurrentPos, "(", "/*(", true);

			do 
			{
				if (parser.LookAhead(Token.ID))
				{
					if (!parser.ParseID(out actPar))
						throw new FileConverterException(FileConverterStrings.RemoveFunctionCallException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
				}
				else
				{
					parser.SkipToken();
					actPar = string.Empty;
				}
				actParams.Add(actPar);

				parser.Parsed(Token.COMMA);
			} 
			while (!parser.Parsed(Token.ROUNDCLOSE) && !parser.LookAhead(Token.EOF));

			ReplaceWord(parser.CurrentPos, ")", ")*/", true);

			string [] vars; string [] types;
			if (!GlobalContext.FunctionTranslator.RetrieveSubstituteVariables(name, out vars, out types))
				return false;
			if (vars.GetLength(0) != actParams.Count)
				return false;

			string assign = string.Empty;

			if (parser.Parsed(Token.SEP))
			{
				string assign2 = string.Empty;
				assign = newName + ";\r\n/*ATTENZIONE sono necessarie le istruzioni che seguono, verificare che il contesto le permetta:\r\nse la chiamata è in una clausola di inizializzazione di un campo (nelle sezione Variables ... Init = f ( a, b, ...)\r\n occorre posizionare le istruzioni nell'evento before del report;\r\n se invece siamo in una sezione programmativa del report (Eventi, Procedure, Rule) basterà decommentarle\r\n con l'accortezza di aggiungere eventualmente una coppia di Begin-End se la chiamata era in una ramo di una istruzione If o era l'unica istruzione di un gestore di eventi.\r\n";

				for (int i = 0; i < actParams.Count; i ++)
				{
					string p = actParams[i] as string;
					p = GlobalContext.WoormInfoVariablesTranslator.TranslateWoormInfoVariable(p);
					string v = vars[i];
					v = GlobalContext.WoormInfoVariablesTranslator.TranslateWoormInfoVariable(v);
					if (string.Compare(p,v,true) != 0)
						assign2 += string.Format("{0} = {1} ;\r\n", p, v);
				}
				assign += assign2 + "*/\r\n";

				if (assign2.Length == 0)
				{
					ReplaceWord(parser.CurrentPos, ";", newName + ";", true);
					return true;
				}

				ReplaceWord(parser.CurrentPos, ";", assign, true, DiagnosticType.Error);

				GlobalContext.LogManager.Message
					(
					string.Format(FileConverterStrings.ReplaceWord, name),
					string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
					);

				return true;
			}

			if (!parser.Parsed(Token.IS)) 
				return false;
			ReplaceWord(parser.CurrentPos, "Is", "AskDialog", true);

			if (!parser.Parsed(Token.NULL)) 
				return false;
			ReplaceWord(parser.CurrentPos, "Null", "== True", true);

			if (!parser.SkipToToken(Token.THEN, true, false)) 
				return false;

			if (parser.LookAhead(Token.BEGIN)) 
			{
				parser.SkipBlock();
				if (parser.Parsed(Token.SEP))
					ReplaceWord(parser.CurrentPos, ";", string.Empty, true);
			}
			else
				parser.SkipToToken(Token.SEP, true, true);

			bool missingEnd = true;
			bool missingElse = false;

			if (!parser.LookAhead(Token.ELSE))
			{
				assign = "\nElse Begin\n";
				missingElse = true;
			}
			else if (parser.Parsed(Token.ELSE) && !parser.Parsed(Token.BEGIN))
				assign = "\nBegin\n";
			else 
				missingEnd = false;

			for (int i=0; i < actParams.Count; i ++)
			{
				if (vars[i] != string.Empty && string.Compare(vars[i], actParams[i] as string, true) != 0)
					assign += string.Format("{0} = {1} ;\n", actParams[i] as string, vars[i]);
			}

			if (missingElse)
			{
				assign += "End;\n";
				missingEnd = false;
			}
			else	
				parser.SkipToken();

			ReplaceWord(parser.CurrentPos, string.Empty, assign, true);
			
			if (missingEnd)
			{
				parser.SkipToToken(Token.SEP, true, true);

				ReplaceWord(parser.CurrentPos, ";", ";\nEnd;", true);
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		private bool ParseSoundCommand(string name)
		{
			if 
				(
				string.Compare(name, "Beep", true) == 0 ||
				string.Compare(name, "Play", true) == 0 || 
				string.Compare(name, "PlayAsync", true) == 0
				)
			{
				ReplaceWord(parser.CurrentPos, name, "Message \"L'istruzione Beep non è piu' supportata", true);

				if (string.Compare(name, "Beep", true) == 0)
				{
					int timeBeep;
					if (!parser.ParseSignedInt(out timeBeep))
						throw new FileConverterException(FileConverterStrings.InternalFunctionException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
				}
				else
				{
					string file;
					if (!parser.ParseString(out file))
						throw new FileConverterException(FileConverterStrings.InternalFunctionException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
					ReplaceWord(parser.CurrentPos, "\"" + file + "\"", string.Empty, true);
				}
				
				if (!parser.ParseSep())
					throw new FileConverterException(FileConverterStrings.InternalFunctionException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
				
				ReplaceWord(parser.CurrentPos, ";", "\";", true, DiagnosticType.Warning);
				return true;
			}

			return false;
		}

		//--------------------------------------------------------------------------------
		private bool IsNoWebFunction(string name)
		{
            //serviva per la migrazione da MagoXp
			string[] noWebFun = new string[] 
										{
											"RunReport",	
											"RunProgram",	

											"MailSetFrom",
											"MailSetSubject",
											"MailSetBody",
											"MailSetMapiProfile",
											"MailSetBodyTemplateFileName",
											"MailSetTo",
											"MailSetCc",
											"MailSetBcc",
											"MailSetAttachment",
											"MailSetBodyParameter",
											"MailSend"
										};

			foreach (string f in noWebFun)
				if(string.Compare(name, f, true) == 0)
					return true;
			return false;
		}

		//--------------------------------------------------------------------------------
		private void InsertNoWeb()
		{
			if (!noWeb)
			{
				ReplaceWord(releaseLineIndex, 0, string.Empty, 
"NoWeb /*ATTENZIONE il report contiene dei costrutti che ne impediscono l'utilizzo tramite 'Easy Look'*/\r\n", 
					false,
					DiagnosticType.Warning);
				noWeb = true;
			}
		}

		//--------------------------------------------------------------------------------
		protected override void InsertInvalid()
		{
			if (!invalid)
			{
				ReplaceWord(releaseLineIndex, 0, string.Empty, "Invalid /*ATTENZIONE la conversione del report necessita di un intervento manuale*/\r\n");
				invalid = true;
			}
		}

		//--------------------------------------------------------------------------------
		private void InsertContextVariables(ref string vars)
		{
			if (contextHandleNameCol.Count == 0) 
				return;
	
			vars = string.Format("\r\nBool __DisposeContextHandleOk [10] Alias {0} Hidden ;\r\n", ++maxAlias);
			for (int i=0; i < contextHandleNameCol.Count; i++)
			{
				vars += string.Format("Long {0} [10] Alias {1} Hidden Init = {2}_Create();\r\n", contextHandleNameCol[i], ++maxAlias, contextNamespaceCol[i]);
			}
		}

		//--------------------------------------------------------------------------------
		private void InsertWoormInfoVariables(ref string vars)
		{
			if (newVariables.Count == 0) 
				return;

			for (int i = 0; i < newVariables.Count; i++)
			{
				string nv = newVariables[i] as string;
				if (nv == string.Empty || mapId.Contains(nv.ToLower())) 
					continue;
				if (string.Compare(nv, "AskDialog", true) != 0)
					vars += string.Format("{0} {1} [10] Alias {2} Hidden Input;\r\n", newVariableTypes[i] as string, nv, ++maxAlias);
				else
					vars += string.Format("{0} {1} [10] Alias {2} Hidden Input Init = TRUE;\r\n", newVariableTypes[i] as string, nv, ++maxAlias);
			}
		}
	
		//--------------------------------------------------------------------------------
		private void InsertNewVariables()
		{
			string vars = string.Empty;

			InsertContextVariables(ref vars);

			InsertWoormInfoVariables(ref vars);
			
			if (vars != string.Empty)
				ReplaceWord(parser.CurrentPos, string.Empty, vars, true);
		}

		//--------------------------------------------------------------------------------
		private void InsertReportEventFinalize(bool addReportGroup, bool AddEvents)
		{
			if (!missingFinalizeEvent) 
				return;
			missingFinalizeEvent = false;

			if (contextHandleNameCol.Count == 0) 
				return;

			string eventFinalize = string.Empty;
			for (int i=0; i < contextHandleNameCol.Count; i++)
			{
				if ((bool)contextMissingDisposeCol[i])
				{
					if (eventFinalize == string.Empty)
						eventFinalize = 
							(AddEvents ? "\r\nEvents\r\n" : string.Empty) +
							(addReportGroup ? "\r\nReport : Do" : string.Empty) +
							"\r\nFinalize Begin\r\n";

					eventFinalize += string.Format("__DisposeContextHandleOk = {0}_Dispose({1});\r\n", contextNamespaceCol[i], contextHandleNameCol[i]);
				}
			}
			if (eventFinalize != string.Empty)
			{
				eventFinalize += "End\r\n" + (AddEvents ? "End\r\n" : string.Empty);
				ReplaceWord(parser.CurrentPos >= 0 ? parser.CurrentPos : lastEndPos, string.Empty, eventFinalize, true);
			}
		}

		//--------------------------------------------------------------------------------
		protected void ParseMessage()
		{
			int messagePos = parser.CurrentPos;
			long messageLine = parser.CurrentLine;

			parser.SkipToken(); //message;
			
			if (parser.LookAhead() == Token.BYTE)
			{
				int messageType;
				if (!parser.ParseInt(out messageType))
					throw new FileConverterException(FileConverterStrings.ReadingMessageStatement, parser.Filename, parser.CurrentLine, parser.CurrentPos);
			
				if (
					messageType == 16			//MB_OK			| MB_ICONSTOP: in questo caso viene interrotta l'elaborazione
					|| messageType == 65		//MB_OKCANCEL	| MB_ICONINFORMATION: in questo caso si puo` interrompere l'elaborazione
					|| messageType == 49 		//MB_OKCANCEL	| MB_ICONEXCLAMATION: in questo caso si puo` interrompere l'elaborazione
					)
					ReplaceWord(messageLine, messagePos, "Message", "Abort", true);
				
				ReplaceWord(parser.CurrentPos, messageType.ToString(), string.Empty, true);
	
				if (!parser.ParseComma())
					throw new FileConverterException(FileConverterStrings.ReadingMessageStatement, parser.Filename, parser.CurrentLine, parser.CurrentPos);

				ReplaceWord(parser.CurrentPos, ",", string.Empty, true);
			}          
		}

		//--------------------------------------------------------------------------------
		protected void ParseVariables()
		{
			parser.SkipToken(); //Variables
			parser.SkipToken(); 

			InsertNewVariables(); //inserisce fra Variables ed il simbolo skippato sopra
		}

		//--------------------------------------------------------------------------------
		protected void ParseGroupEvents()
		{
			parser.SkipToken();
			if (
				!parser.ParseTag(Token.COLON)  ||
				!parser.ParseTag(Token.DO)
				) return;

			if (parser.Parsed(Token.ALWAYS))
			{
				if (parser.LookAhead() != Token.BEGIN)
				{
					if (!parser.SkipToToken(Token.SEP, true, true)) return;
				} 
				else	
					if (!parser.SkipBlock()) return;
			}
			if (parser.Parsed(Token.BEFORE))
			{
				if (parser.LookAhead() != Token.BEGIN)
				{
					if (!parser.SkipToToken(Token.SEP, true, true)) return;
				} 
				else	
					if (!parser.SkipBlock()) return;
			}
			if (parser.Parsed(Token.AFTER))
			{
				if (parser.LookAhead() != Token.BEGIN)
				{
					if (!parser.SkipToToken(Token.SEP, true, true)) return;
				} 
				else	
					if (!parser.SkipBlock()) return;
			}
		}

		//--------------------------------------------------------------------------------
		protected void ParseReportEvents()
		{
			if (!parser.LookAhead(Token.REPORT))
			{
				return;
			}

			ParseGroupEvents();
	
			parser.SkipToken();//si sposta un token in avanti, ma inserirà prima 

			InsertReportEventFinalize(false, false);
		}

		//--------------------------------------------------------------------------------
		protected void CheckMaxAlias()
		{
			// cerco il valore massimo di Alias utilizzato
			parser.SkipToken();//Alias
			int alias;
			parser.ParseInt(out alias);
			if (maxAlias < alias) maxAlias = alias;
		}

		//--------------------------------------------------------------------------------
		protected void ParseRelease()
		{
			// tengo da parte la linea del tag release, la riutilizzerò successivamente per 
			// inserire un eventuale tag NoWeb
			releaseLineIndex = currentLineIndex;
			
			parser.SkipToken();//Release
			int release;

			if (!parser.ParseInt(out release))
				throw new FileConverterException(FileConverterStrings.RenameReleaseException, parser.Filename, parser.CurrentLine, parser.CurrentPos);

			if (release == 3)
				ReplaceWord(parser.CurrentPos, release.ToString(), woormRelease.ToString(), true); 
			else
				throw new FileConverterException(FileConverterStrings.ParseReleaseThree, parser.Filename, parser.CurrentLine, parser.CurrentPos);

		}

		//--------------------------------------------------------------------------------
		protected void ParseUUID()
		{
			parser.SkipToken(); //UUID
				
			ReplaceWord(parser.CurrentPos, "Uuid", string.Empty, true);
			string guid;
			if (!parser.ParseString(out guid))
				throw new FileConverterException(FileConverterStrings.ReadingGuidReport, parser.Filename, parser.CurrentLine, parser.CurrentPos);

			ReplaceWord(parser.CurrentPos, "\"" + guid + "\"", string.Empty, true);
		}

		//--------------------------------------------------------------------------------
		protected bool ParseAskDialog(bool isFirst)
		{
			parser.Parsed(Token.DIALOG); 
			
			// se è la prima chiamata, inserisco il token di raggruppamento
			if (isFirst)
				ReplaceWord
					(
					parser.CurrentPos, 
					string.Empty, 
					"Dialogs\r\nBegin\r\n", 
					true
					);
			
			if (parser.Parsed(Token.ON))
			{
				if (!parser.Parsed(Token.ASK))
					throw new FileConverterException(FileConverterStrings.ReadingReportRole, parser.Filename, parser.CurrentLine, parser.CurrentPos);
			}

			if (!ParseAskDialogHeader())
				throw new FileConverterException(FileConverterStrings.ReadingReportRole, parser.Filename, parser.CurrentLine, parser.CurrentPos);

			//DIALOG BEGIN
			ReplaceWord
				(
				parser.CurrentPos, 
				string.Empty, 
				"Begin\r\n", 
				true
				);
	
			//CONTROLS BEGIN
			ReplaceWord
				(
				parser.CurrentPos, 
				string.Empty, 
				"Controls\r\nBegin\r\n", 
				true
				);

			// leggo tutti i gruppi
			while (parser.Parsed(Token.BEGIN))
			{
				while (!parser.Parsed(Token.END))
				{
					if (parser.Parsed(Token.HOTLINK))
						ParseHotLink(false);					// se in coda all'hot link c'è un datasource, lo rimuove
					else if (parser.Parsed(Token.DATASOURCE))
						ParseDataSource();				// se passo di qui, significa che prima non c'era hotlink
					else
						parser.SkipToken();
					
					if (parser.Eof)
						throw new FileConverterException(FileConverterStrings.ReadingReportRole, parser.Filename, parser.CurrentLine, parser.CurrentPos);
				}
			}

			//(CONTROLS) END
			ReplaceWord
				(
				parser.CurrentPos, 
				string.Empty, 
				"End\r\n", 
				true
				);

			//(DIALOG) END
			ReplaceWord
				(
				parser.CurrentPos, 
				string.Empty, 
				"End\r\n", 
				true
				);

			if (parser.LookAhead(Token.DIALOG))
				ParseAskDialog(false);


			// se è la prima chiamata, inserisco il token di chiusura (tutte le ask dialog sono state processate ricorsivamente)
			if (isFirst)
				ReplaceWord
					(
					parser.CurrentPos, 
					string.Empty, 
					"\r\nEnd/*Dialogs*/\r\n", 
					true
					);
		
			return true;
		}

		//---------------------------------------------------------------------------
		private void ParseHotLink(bool isDataSource)
		{
			string name;
			if (!parser.ParseTag(Token.ASSIGN))
				throw new FileConverterException(FileConverterStrings.ReadingHotLink, parser.Filename, parser.CurrentLine, parser.CurrentPos);

			// rimuovo l'assegnazione
			ReplaceWord(parser.CurrentPos, "=", string.Empty, true);
			
			if (!parser.ParseID(out name))
				throw new FileConverterException(FileConverterStrings.ReadingHotLink, parser.Filename, parser.CurrentLine, parser.CurrentPos);


			// sostituisco il nome dell'hot link con il namespace
			string newName = GlobalContext.HotLinkTranslator.TranslateHotLink(name);
			if (newName != null && newName != string.Empty && name != newName)
			{
				ReplaceWord(parser.CurrentPos, name, newName, true);
			}
			else
			{
				bool finded = false;

				if (isDataSource && name.StartsWith("DS"))
				{
					newName = GlobalContext.HotLinkTranslator.TranslateHotLink("HKL" + name.Substring(2));
					if (newName != null && newName != string.Empty)
					{
						ReplaceWord(parser.CurrentPos, name, 
							newName + "\r\n/*ATTENZIONE la traduzione è avvenuta trasformando il DataSource nel corrispondente Hotlink: verificare la conformità degli eventuali parametri*/\r\n", 
							true, DiagnosticType.Warning);
						finded = true;
					}
				}
				if (!finded)
				{
					if (isDataSource)
						GlobalContext.LogManager.Message(string.Format(FileConverterStrings.DataSourceUnsupported, name), "", DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine));
					else
						GlobalContext.LogManager.Message(string.Format(FileConverterStrings.HotLinkUnsupported, name, GlobalContext.HotLinkTranslator.GetDescription(name)), "", DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine));
				}
			}

			if (parser.Parsed(Token.SQUAREOPEN))
			{
				ReplaceWord(parser.CurrentPos, "[", "(", true);
				
				while (!parser.Parsed(Token.SQUARECLOSE))
				{
					if (parser.Parsed(Token.SEP))
					{
						ReplaceWord(parser.CurrentPos, ";", ",", true);	
					}
					else
						parser.SkipToken();
					if (parser.Eof)
						throw new FileConverterException(FileConverterStrings.ReadingHotLink, parser.Filename, parser.CurrentLine, parser.CurrentPos);
				}
				
				ReplaceWord(parser.CurrentPos, "]", ")", true);	

				string description = GlobalContext.HotLinkTranslator.GetDescription(name);
				if (description != string.Empty && description != null)
					ReplaceWord(parser.CurrentPos, string.Empty, "/* " + description + " */", true);		
			}
			else
			{
				// aggiungo le parentesi tonde
				ReplaceWord(parser.CurrentPos, string.Empty, "()", true);
			}

			if (parser.Parsed(Token.DATASOURCE))
				RemoveDataSource();
		}

		//---------------------------------------------------------------------------
		private void RemoveDataSource()
		{
			// rimuovo il tag datasource
			ReplaceWord(parser.CurrentPos, "DataSource", string.Empty, true);
			if (!parser.ParseTag(Token.ASSIGN))
				throw new FileConverterException(FileConverterStrings.ReadingHotLink, parser.Filename, parser.CurrentLine, parser.CurrentPos);

			// rimuovo l'assegnazione
			ReplaceWord(parser.CurrentPos, "=", string.Empty, true);
			
			string name;
			if (!parser.ParseID(out name))
				throw new FileConverterException(FileConverterStrings.ReadingHotLink, parser.Filename, parser.CurrentLine, parser.CurrentPos);

			GlobalContext.LogManager.Message
				(
				string.Format(FileConverterStrings.DataSourceJoinedHotLinkUnsupported, name), 
				"", 
				DiagnosticType.Information,
				new ExtendedInfo(FileConverterStrings.Line,
				parser.CurrentLine)
				);

			// rimuovo il nome del datasource
			ReplaceWord(parser.CurrentPos, name, string.Empty, true);
			
			if (parser.Parsed(Token.SQUAREOPEN))
			{
				// rimuovo [ 
				ReplaceWord(parser.CurrentPos, "[", string.Empty, true);
				
				while (!parser.Parsed(Token.SQUARECLOSE))
				{
					parser.SkipToken();
					if (parser.Eof)
						throw new FileConverterException(FileConverterStrings.ReadingHotLink, parser.Filename, parser.CurrentLine, parser.CurrentPos);
					ReplaceWord(parser.CurrentPos, parser.CurrentLexeme, string.Empty, true);
				}
				// rimuovo ]
				ReplaceWord(parser.CurrentPos, "]", string.Empty, true);	
			}
		}

		//---------------------------------------------------------------------------
		private void ParseDataSource()
		{
			// converto hotlink in datasource
			ReplaceWord(parser.CurrentPos, "DataSource", "HotLink", true);
			// quindi faccio la conversione come se fosse un normale hotlink
			ParseHotLink(true);
		}

		//---------------------------------------------------------------------------
		private bool ParseAskDialogHeader()
		{
			string dummy;
			// nome
			if (!parser.ParseID(out dummy))
				return false;

			// titolo
			if (parser.LookAhead() == Token.TEXTSTRING && !parser.ParseString(out dummy))
				return false;

			// eventuale help
			if (parser.LookAhead(Token.HELP))
			{
				ReplaceWord(parser.CurrentPos, "Help", "/*Help", true);
				parser.Parsed(Token.HELP);
				if (!parser.ParseTag(Token.ASSIGN))
					return false;
				
				long helpData;
				parser.ParseLong(out helpData);

				parser.LookAhead();
				ReplaceWord(parser.CurrentPos, string.Empty, "*/\r\n");
			}

			// default position
			switch (parser.LookAhead())
			{
				case Token.LEFT	:
				case Token.TOP	:
					parser.SkipToken();
					if (!parser.ParseTag(Token.PROMPT)) return false;
					break;
			}
			return true;
		}
	}
}
