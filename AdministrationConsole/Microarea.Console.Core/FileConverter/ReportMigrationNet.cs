using System.Collections;
using System.IO;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;

namespace Microarea.Console.Core.FileConverter
{
    /// <summary>
    /// Summary description for ReportMigrationNet.
    /// </summary>
    //================================================================================
    public class ReportMigrationNet : WRMTableParser
	{
		public static bool BackupFile = false;
		private int stepProcess = 1;

		MigrationNetStep netStep = null;

		//--------------------------------------------------------------------------------
		public ReportMigrationNet(string fileName, string destinationFileName) : base(fileName, destinationFileName)
		{
			netStep = new MigrationNetStep(this);
			checkLogErrorsToValidateParsing = true;
		}

		//--------------------------------------------------------------------------------
		public override bool Parse()
		{
			if (BackupFile)
			{
				File.Copy(fileName, Path.ChangeExtension(fileName, ".bak"), true);
				GlobalContext.LogManager.Message(FileConverterStrings.FileBackup, string.Empty, DiagnosticType.Information, null);
			}
			
			// per non complicare il codice (e in alcuni casi perché il parser è forward-only), 
			// vengono effettuate piu' passate
			// la prima è inerente la rinominazione delle tabelle-colonne
			// la seconda è una serie di modifiche comuni alla nuova migrazione da MagoXp (enum, font e Formatter, linkform/linkreport)
			// la terza sono modifiche già apportate separatamente dalla precedente migrazione (hotlinks e webmethods)
			stepProcess = 1;
			encoding = System.Text.Encoding.UTF8;

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

			return successStep1 && successStep2 && successStep3 && successStep4; 
		}

		//--------------------------------------------------------------------------------
		protected override void ProcessBuffer()
		{
			if (stepProcess == 1)
			{
				GlobalContext.LogManager.Message(string.Format(FileConverterStrings.RunningStep, stepProcess, fileName), string.Empty, DiagnosticType.Information, null);
				base.ProcessBuffer(); //ereditato da WrmTableParser
			}
			else if (stepProcess == 2)
			{
				GlobalContext.LogManager.Message(string.Format(FileConverterStrings.RunningStep, stepProcess, fileName), string.Empty, DiagnosticType.Information, null);
				netStep.ProcessBufferCommon();
			}
			else if (stepProcess == 3)
			{
				GlobalContext.LogManager.Message(string.Format(FileConverterStrings.RunningStep, stepProcess, fileName), string.Empty, DiagnosticType.Information, null);
				netStep.ProcessBufferCommon2();
			}
			else if (stepProcess == 4)
			{
				GlobalContext.LogManager.Message(string.Format(FileConverterStrings.RunningStep, stepProcess, fileName), string.Empty, DiagnosticType.Information, null);
				netStep.ProcessBufferNet();
			}
		}

		//---------------------------------------------------------------------------------------------------
		override public string LookUpDocumentNamespace(ref string ns)
		{
			if (string.Compare("document.", 0 , ns, 0, 9, true) == 0)
				ns = ns.Substring(9);

			return GlobalContext.DocumentsTranslator.TranslateDocument(ns);
		}

	}
	//===========================================================================
	/// <summary>
	/// Summary description for MigrationNetStep.
	/// </summary>
	//================================================================================
	public class MigrationNetStep 
	{
		ObjectParser parent = null;

		private const int woormRelease = 7;	

		private Hashtable mapLocalFormatter = new Hashtable();
		private Hashtable mapLocalFont = new Hashtable();

		private Parser parser { get { return parent.parser; }}

		//--------------------------------------------------------------------------------
		public MigrationNetStep(ObjectParser p)
		{
			parent = p;
		}

		//--------------------------------------------------------------------------------
		protected IFontTranslator FontTranslator { get { return GlobalContext.FontTranslator; }}
		//--------------------------------------------------------------------------------
		protected IFormatterTranslator FormatterTranslator { get { return GlobalContext.FormatterTranslator; }}

		//--------------------------------------------------------------------------------
		//modifiche condivise con la migrazione Xp->Net2
		public void ProcessBufferCommon()
		{
			parent.encoding = System.Text.Encoding.UTF8;
			int skipLinkFormParam = 0;
			
			while (!parser.Eof)
			{
				Token t = parser.LookAhead();
				switch (t)
				{
					case Token.RELEASE:
					{
						RenameRelease(); //da 6 a 7

						if (parser.Parsed(Token.COMMA)) parser.SkipToken();

						parser.Parsed(Token.MAXIMIZED);
						parser.Parsed(Token.MINIMIZED);
						if (parser.Parsed(Token.RECT))	parent.SkipRect();
						parser.Parsed(Token.ONLY_GRAPH);

						if (parser.LookAhead(Token.ID))
						{
							string name;
							parser.ParseID(out name);
							if (string.Compare(name, "Description", true) == 0)
							{
								parent.ReplaceWord(parser.CurrentPos, name, "ReportDescription", true);
							}
						}

						break;
					}
					case Token.PROPERTIES:
					{
						parser.SkipToken();
						parser.ParseTag(Token.BEGIN);
						parser.ParseTag(Token.TITLE);parser.SkipToken();
						parser.ParseTag(Token.SUBJECT);parser.SkipToken();
						parser.ParseTag(Token.AUTHOR);parser.SkipToken();
						if (!parser.LookAhead(Token.REPORTPRODUCER))
						{
							string name;
							parser.ParseID(out name);
							if (string.Compare(name, "Company", true) == 0)
							{
								parent.ReplaceWord(parser.CurrentPos, name, "ReportProducer", true);
							}
							parser.SkipToken();
						}
						if (parser.Parsed(Token.COMMENTS) && parser.LookAhead(Token.TEXTSTRING))
						{
							string name;
							if (!parser.ParseString(out name))
							{
								string s = "\r\n/*ATTENZIONE la proprietà 'Comments' non è stata convertita:\r\n se è suddivida su piu' righe, occorre quotare con i doppi apici\r\n separatamente il testo di tutte le righe del testo del commento*/\r\n";	
								parent.ReplaceWord(parser.CurrentPos, string.Empty, s, false, DiagnosticType.Error);
								break;
							}
							string newName = name.Replace("\r", "");
							newName = newName.Replace("\n", "\"\r\n\"");

							if (name != newName)
								parent.ReplaceWord(parser.CurrentPos, name+1, newName, true);

						}
						break;
					}

					//-----
					case Token.ID:
					{
						if (skipLinkFormParam != 2)
							RenameID();
						else
							parser.SkipToken();
						break;
					}
					case Token.LINKFORM:
					{
						parser.SkipToken();

						skipLinkFormParam = 1;
						break;
					}
					case Token.BEGIN:
					{
						parser.SkipToken();

						if (skipLinkFormParam == 1)
							skipLinkFormParam = 2;
						break;
					}
					case Token.END:
					{
						parser.SkipToken();

						if (skipLinkFormParam == 2)
							skipLinkFormParam = 0;
						break;
					}
					//-----

					case Token.FORMATSTYLES:
					{
						parser.SkipToken();
						if (parser.Parsed(Token.BEGIN))
						{
							for(;;) 
							{
								RenameLocalFormatStyle();
								if (parser.LookAhead(Token.EOF) || parser.Parsed(Token.END))
									break;
							}
						}
						else
						{
							RenameLocalFormatStyle();
						}
						break;
					}
					case Token.FONTSTYLES:
					{
						parser.SkipToken();
						if (parser.Parsed(Token.BEGIN))
						{
							for(;;) 
							{
								RenameLocalFontStyle();
								if (parser.LookAhead(Token.EOF) || parser.Parsed(Token.END))
									break;
							}
						}
						else
						{
							RenameLocalFontStyle();
						}
						break;
					}

					case Token.FONTSTYLE:
					{
						RenameFontStyle();
						break;
					}
					case Token.FORMATSTYLE:
					{
						RenameFormatStyle();
						break;
					}
					case Token.FORMAT:
					{
						RenameFormatFunction();
						break;
					}
					case Token.TYPED_BARCODE:
					{
						RenameTypedBarCodeFunction();
						break;
					}
					case Token.BARCODE:
					{
						RenameBarcodeFunction();
						break;
					}
					case Token.ISACTIVATED:
					{
						RenameIsActivatedFunction();
						break;
					}
					case Token.TEXT:
					{
						RenameSpecialFields();
						break;
					}

					case Token.SQUAREOPEN:
					{
						parser.SkipToken();
						if (!parser.LookAhead(Token.TEXTSTRING)) 
							break;

						string enumName = string.Empty;
						if (!parser.ParseString(out enumName))
							parent.ThrowException(FileConverterStrings.WaitingDatatype);

						string tagValue = GlobalContext.EnumTranslator.GetTagValue(enumName);
						if (tagValue != string.Empty)
							parent.ReplaceWord(parser.CurrentPos, "\"" + enumName + "\""  , tagValue, true);
						
						if (!parser.ParseTag(Token.SQUARECLOSE))
							parent.ThrowException(FileConverterStrings.WaitingDatatype);
						break;
					}

					case Token.BRACEOPEN:
					{
						parser.SkipToken();
						if (!parser.LookAhead(Token.TEXTSTRING)) 
							break;
						
						string enumName = string.Empty;
						if (!parser.ParseString(out enumName))
							parent.ThrowException(FileConverterStrings.WaitingValue);

						string tagValue = GlobalContext.EnumTranslator.GetTagValue(enumName);
						if (tagValue != string.Empty)
							parent.ReplaceWord(parser.CurrentPos , "\"" + enumName + "\""  , tagValue, true);

						if (!parser.ParseTag(Token.COLON))
							parent.ThrowException(FileConverterStrings.WaitingValue);
					
						string itemName = string.Empty;
						if (!parser.ParseString(out itemName))
							parent.ThrowException(FileConverterStrings.WaitingValue);

						string itemValue = GlobalContext.EnumTranslator.GetItemValue(enumName, itemName);
						if (itemValue != string.Empty)
							parent.ReplaceWord(parser.CurrentPos , "\"" + itemName + "\""  , itemValue, true);

						if (!parser.ParseTag(Token.BRACECLOSE))
							parent.ThrowException(FileConverterStrings.WaitingValue);
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
		//modifiche condivise con la migrazione Xp->Net2
		public void ProcessBufferCommon2()
		{
			parent.encoding = System.Text.Encoding.UTF8;
			
			while (!parser.Eof)
			{
				Token t = parser.LookAhead();
				switch (t)
				{
					case Token.ID:
					{
						string id;
						if (!parser.ParseID(out id))
							return;
						if (id.ToLower().EndsWith(".runreport"))
						{
							parent.ReplaceWord
								(
								parser.CurrentPos, 
								string.Empty, 
								"\r\n/*ATTENZIONE : occorre verificare il namespace del report e i nomi degli eventuali parametri*/\r\n", 
								false, DiagnosticType.Warning 
								);

							ParseRunReportFunction();
						}
						break;
					}
					case Token.RUNREPORT:
					{
						parent.ReplaceWord(
							parser.CurrentPos, string.Empty, 
"\r\n/*ATTENZIONE : occorre verificare il namespace del report e i nomi degli eventuali parametri*/\r\n" +
"Framework.TbWoormViewer.TbWoormViewer.", 
							true, DiagnosticType.Error );

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
		public void ProcessBufferNet()
		{
			parent.encoding = System.Text.Encoding.UTF8;
			
			while (!parser.Eof)
			{
				Token t = parser.LookAhead();
				switch (t)
				{
					case Token.ID:
					{
						RenameFunction(); 
						break;
					}

					case Token.HOTLINK:
					{
						parser.SkipToken(); 

						RenameHotlink();
						break;
					}

					case Token.LINKFORM:
					{
						parser.SkipToken();
						
						RenameLinkForm();
						break;
					}

					case Token.LINKREPORT:
					{
						parser.SkipToken();
						
						RenameLinkReport();
						break;
					}

					case Token.OPTIONS:
					{
						parser.SkipToken();
						parser.Parsed(Token.BEGIN); //opzionale
						if (parser.Parsed(Token.BITMAP))
						{
							RenameFilePath(false);
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
						RenameFileObject();
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
		protected void RenameLinkForm()
		{
			string name;
			if (!parser.ParseString(out name))
				return ;

			string newName = parent.LookUpDocumentNamespace(ref name);
			if (newName != name)
			{
				parent.ReplaceWord(parser.CurrentPos + 1, name, newName, true);
			}
		}

		//---------------------------------------------------------------------------------------------------
		protected void RenameLinkReport()
		{
			string name;
			if (!parser.ParseString(out name))
				parent.ThrowException(FileConverterStrings.RenameLinkReportException);

			string ns = name;
			if (string.Compare("report.", 0 , name, 0, 7, true) == 0)
				ns = ns.Substring(7);

			string newName = GlobalContext.ReportsTranslator.TranslateReport(name);
			if (newName != ns)
			{
				parent.ReplaceWord(parser.CurrentPos + 1, name, newName, true);
			}
			//NB: i parametri vengono considerati come ID generici
		}

		//--------------------------------------------------------------------------------
		protected void RenameID()
		{
			int pos = parser.CurrentPos;
			long line = parser.CurrentLine;

			string name;
			if (!parser.ParseID(out name)) //funzione;
				parent.ThrowException(FileConverterStrings.RenameIDException);
		
			string newName = name;
			if (!parser.LookAhead(Token.ROUNDOPEN))
			{
				newName = GlobalContext.WoormInfoVariablesTranslator.TranslateWoormInfoVariable(name);
			}
			if (newName != name)
				parent.ReplaceWord(line, pos, name, newName, true);		
		}

		//--------------------------------------------------------------------------------
		protected void RenameFunction()
		{
			int pos = parser.CurrentPos;
			long line = parser.CurrentLine;

			string name;
			if (!parser.ParseID(out name)) //funzione;
				parent.ThrowException(FileConverterStrings.RenameIDException);
		
			string newName = name;
			if (parser.LookAhead(Token.ROUNDOPEN))
			{
				newName = GlobalContext.FunctionTranslator.TranslateFunction(name, GlobalContext.ReportsData.Module);
			}
			if (newName != name)
				parent.ReplaceWord(line, pos, name, newName, true);		
		}

		//--------------------------------------------------------------------------------
		protected void RenameHotlink()
		{
			string name;
			if (!parser.ParseID(out name)) //funzione;
				throw new FileConverterException(FileConverterStrings.RenameIDException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
			
			string newName = GlobalContext.HotLinkTranslator.TranslateHotLink(name);
			if (newName != name)
			{
				parent.ReplaceWord(parser.CurrentPos, name, newName, true);
			}
		}

		//---------------------------------------------------------------------------------------------------
		protected void RenameLocalFormatStyle()
		{
			parser.SkipToken();//ParseDataType();

			string name;
			if (!parser.ParseString(out name))
				return;

			string newName = FormatterTranslator.TranslateFormatter(name);
			if (newName != name)
			{
				parent.ReplaceWord(parser.CurrentPos + 1, name, newName, true);
			}
			else 
				mapLocalFormatter.Add(name.ToLower(), name);

			parser.SkipToToken(Token.SEP, true, false);
		}

		//---------------------------------------------------------------------------------------------------
		protected void RenameLocalFontStyle()
		{
			//elimino l'ex token "Name"
			if (parser.LookAhead(Token.ID))
			{
				string tname;
				if (!parser.ParseID(out tname))
					return;
				if (string.Compare(tname, "name", true) == 0)
					parent.ReplaceWord(parser.CurrentPos, tname, string.Empty, true);

			}
			string name;
			if (!parser.ParseString(out name))
				return;
			
			string newName = FontTranslator.TranslateFont(name);
			if (newName != name)
			{
				parent.ReplaceWord(parser.CurrentPos + 1, name, newName, true);
			}
			else mapLocalFont.Add(name.ToLower(), name);
			
			parser.SkipToToken(Token.SEP, true, false);
		}

		//--------------------------------------------------------------------------------
		protected void RenameFormatStyle()
		{
			parser.SkipToken();	//FormatStyle

			string name;
			if (!parser.ParseString(out name))
				throw new FileConverterException(FileConverterStrings.RenameFormatStyleException, parser.Filename, parser.CurrentLine, parser.CurrentPos);

			string newName = name;
			if (mapLocalFormatter[name.ToLower()] == null)
				newName = FormatterTranslator.TranslateFormatter(name);
			if (newName != name)
				parent.ReplaceWord(parser.CurrentPos + 1 /*+1 per via dei doppi apici*/, name, newName, true); 
		}

		//--------------------------------------------------------------------------------
		protected void RenameFontStyle()
		{
			parser.SkipToken();	//FontStyle

			string name;
			if (!parser.ParseString(out name))
				throw new FileConverterException(FileConverterStrings.RenameFontStyleException, parser.Filename, parser.CurrentLine, parser.CurrentPos);

			string newName = name;
			if (mapLocalFont[name.ToLower()] == null)
				newName = FontTranslator.TranslateFont(name);
			if (newName != name)
				parent.ReplaceWord(parser.CurrentPos + 1 /*+1 per via dei doppi apici*/, name, newName, true); 
		}

		//--------------------------------------------------------------------------------
		protected bool SearchCommaAndRenameID()
		{
			int roundToClose = 1;
			while (true)
			{
				if (parser.LookAhead(Token.COMMA) && roundToClose == 1)
				{
					return true;
				}
				else if (parser.LookAhead(Token.ROUNDCLOSE))
				{
					roundToClose--;
					if (roundToClose == 0) 
						break;
				}
				else if (parser.LookAhead(Token.ROUNDOPEN))
				{
					roundToClose++;
				}
				else if (parser.LookAhead(Token.EOF))
					return false;

				if (parser.LookAhead(Token.ID))
					RenameID();
				else
					parser.SkipToken();
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		protected void RenameTypedBarCodeFunction()
		{
			parser.ParseTag(Token.TYPED_BARCODE);
			parser.ParseTag(Token.ROUNDOPEN);

			SearchCommaAndRenameID();

			if (parser.Parsed(Token.COMMA))
			{
				parent.ReplaceWord
					(
					parser.CurrentPos, 
					",", 
					", ERP.Items.Components.GetNativeBarCodeType(", 
					true
					); 			
				
				SearchCommaAndRenameID();
				
				if (parser.Parsed(Token.ROUNDCLOSE))
					parent.ReplaceWord
						(
						parser.CurrentPos, 
						string.Empty, 
						")", 
						true
						); 			
			}
		}
		//--------------------------------------------------------------------------------
		protected void RenameFormatFunction()
		{
			parser.Parsed(Token.FORMAT);
			parser.Parsed(Token.ROUNDOPEN);

			SearchCommaAndRenameID();
			
			if (parser.Parsed(Token.COMMA))
			{
				string name;
				if (!parser.ParseString(out name))
					throw new FileConverterException(FileConverterStrings.RenameFormatFunctionException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
				
				string newName = name;
				if (mapLocalFormatter[name.ToLower()] == null)
					newName = FormatterTranslator.TranslateFormatter(name);
				if (newName != name)
					parent.ReplaceWord(parser.CurrentPos + 1 /*+1 per via dei doppi apici*/, name, newName, true); 
			}
		}
		//--------------------------------------------------------------------------------
		protected void RenameBarcodeFunction()
		{
			parser.Parsed(Token.BARCODE);
			parser.Parsed(Token.ROUNDOPEN);

			string name;
			if (!parser.ParseString(out name))
				throw new FileConverterException(FileConverterStrings.RenameFormatFunctionException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
			
			string newName = name;
			if(name.ToLower().IndexOf("predefin") > -1) //case insensitive Predefinito|(predefinito)|<Predefined>
				newName = "Default";

			if (newName != name)
				parent.ReplaceWord(parser.CurrentPos + 1 /*+1 per via dei doppi apici*/, name, newName, true); 
		}

		//--------------------------------------------------------------------------------
		protected void ParseRunReportFunction()
		{
			//tipicamente la chiamata è del tipo:
			//... RunReport (GetReportModuleNamespace() + "child report | flag1=1|flag2=2", p1, p2)
			//... RunReport ("child report | flag1=1|flag2=2", p1, p2)
			parser.Parsed(Token.RUNREPORT);

			parser.Parsed(Token.ROUNDOPEN);

			if (!parser.LookAhead(Token.TEXTSTRING))
			{
				bool bOk = false;
				bOk = parser.SkipToToken(new Token [] { Token.TEXTSTRING, Token.COMMA, Token.ROUNDCLOSE }, false, true, true);
//				if (parser.LookAhead(Token.ID))
//				{
//					string id;
//					parser.ParseID(out id);
//					bOk = parser.Parsed(Token.ROUNDOPEN) && 
//						  parser.Parsed(Token.ROUNDCLOSE) && 
//						  parser.Parsed(Token.PLUS) && 
//						  parser.LookAhead(Token.TEXTSTRING);
//				}
//
				if (!bOk)
					return;
				if (!parser.LookAhead(Token.TEXTSTRING))
					return;
			}

			string ns, ns2, flags;
			if (!parser.ParseString(out ns))
				throw new FileConverterException(FileConverterStrings.RenameFormatFunctionException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
			ns2 = ns;
			flags = string.Empty;
			if (ns.ToLower().IndexOf("report.", 0) == 0)
				ns2 = ns.Substring(7);

			int pos = ns2.IndexOf("|", 0);
			if (pos > 0)
			{
				flags = ns2.Substring(pos);
				ns2 = ns2.Substring(0, pos);
			}
				
			string newName = GlobalContext.ReportsTranslator.TranslateReport(ns2);
			if (newName != ns2)
			{
				parent.ReplaceWord(parser.CurrentPos + 1, ns, newName + flags, true);
			}
			else
			{
				string [] arNs = ns2.Split(new char [] { '.' });
				if (arNs.GetUpperBound(0) > 1)
				{
					string s = arNs[0] + "." + arNs[1];
					string newNs = GlobalContext.LibrariesTranslator.TranslateModule(s) + "." + arNs[1];
					if (newNs != ns2)
						parent.ReplaceWord(parser.CurrentPos + 1, ns, newNs + flags, true);
				}
			}

			//RunReport ( "MagoNet.Partite.EstrattiContoClienti | WaitEnd=1|SendMail=6|CloseOnEndPrint=1|TemplateEmail=" + PathDocumento , ...
			if (!parser.SkipToToken(new Token [] {Token.COMMA, Token.ROUNDCLOSE}, false, true, true))
				return;

			//rinominazione dei nomi dei parametri
			string par;
			if (!parser.Parsed(Token.ROUNDCLOSE))
			{
				do 
				{
					if (parser.ParseTag(Token.COMMA))
					{
						if (!parser.ParseString(out par))
							return;

						string newPar = GlobalContext.WoormInfoVariablesTranslator.TranslateWoormInfoVariable(par);

						if (par != newPar)
							parent.ReplaceWord(parser.CurrentPos + 1, par, newPar,	true);

						if (!parser.ParseTag(Token.COMMA))
							return;
						//skip parametro attuale, puo' essere una espressione
						if (!parser.SkipToToken(new Token [] {Token.COMMA, Token.ROUNDCLOSE}, false, true, true))
							return;
					}
					else
						return;

				} while (!parser.Parsed(Token.ROUNDCLOSE) && !parser.LookAhead(Token.EOF));
			}

		}

		//--------------------------------------------------------------------------------
		protected void RenameIsActivatedFunction()
		{
			parser.Parsed(Token.ISACTIVATED);
			parser.Parsed(Token.ROUNDOPEN);

			string app;
			if (!parser.ParseString(out app))
				throw new FileConverterException(FileConverterStrings.RenameFormatFunctionException, parser.Filename, parser.CurrentLine, parser.CurrentPos);

			string newAppName = GlobalContext.ActivationsTranslator.TranslateActivation(app);
			if (newAppName != app)
				parent.ReplaceWord(parser.CurrentPos + 1 /*+1 per via dei doppi apici*/, app, newAppName, true); 

			parser.Parsed(Token.COMMA);

			string mod;
			if (!parser.ParseString(out mod))
				throw new FileConverterException(FileConverterStrings.RenameFormatFunctionException, parser.Filename, parser.CurrentLine, parser.CurrentPos);
			
			string newModName = GlobalContext.ActivationsTranslator.TranslateActivation(app, mod);
			if (newModName != mod)
				parent.ReplaceWord(parser.CurrentPos + 1 /*+1 per via dei doppi apici*/, mod, newModName, true); 
		}

		//--------------------------------------------------------------------------------
		protected void RenameFilePath(bool isText)
		{
			string name;
			parser.ParseString(out name);

			string [] arNs = name.Split(new char [] { '.' });
			if (arNs.GetUpperBound(0) < 4)
			{
				GlobalContext.LogManager.Message
					(
					string.Format(FileConverterStrings.RenameFilePathFailed, name),
					string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
					);
				return;
			}

			string s = arNs[0] + "." + arNs[1];
			string newNs = GlobalContext.LibrariesTranslator.TranslateModule(s);
			
			if (newNs != string.Empty) //&& newNs != name
			{
				string newName = newNs + name.Substring(s.Length);
				parent.ReplaceWord(parser.CurrentPos + 1, name, newName, true, DiagnosticType.Warning);
			}
			else
				GlobalContext.LogManager.Message
					(
					string.Format(FileConverterStrings.RenameFilePathFailed, name),
					string.Empty, DiagnosticType.Error, new ExtendedInfo(FileConverterStrings.Line, parser.CurrentLine)
					);
		}

		//--------------------------------------------------------------------------------
		protected void RenameFileObject()
		{
			parser.SkipToken();
			if (parser.LookAhead() != Token.ROUNDOPEN)
			{
				//potrebbe essere l'attributo File di un oggetto Field 
				//significa "mostra il contenuto del file il cui path/namespace è il corrente valore del Field(Rect)"
				return;
			}
			parent.SkipRect();
			parent.SkipRatio();

			RenameFilePath(true);
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
			parent.SkipRect();

			parser.Parsed(Token.PROPORTIONAL);
			if (parser.Parsed(Token.RECT))
				parent.SkipRect();

			RenameFilePath(false);
		}

		//--------------------------------------------------------------------------------
		protected void RenameSpecialFields()
		{
			bool ok =
				parser.ParseTag			(Token.TEXT) &&
				parser.ParseTag			(Token.ROUNDOPEN) &&
				parser.SkipToToken		(Token.ROUNDCLOSE, true, false);
			if (!ok)
				parent.ThrowException(FileConverterStrings.RenameSpecialFieldsException);
			
			parent.SkipRatio();

			if (parser.Parsed(Token.BEGIN))
			{
				for(;;) 
				{
					RenameSpecialFields2();
					if (parser.LookAhead(Token.EOF) || parser.Parsed(Token.END))
						break;
				}		
			}
			else
			{
				RenameSpecialFields2();
				return;
			}
		}

		//--------------------------------------------------------------------------------
		protected void RenameSpecialFields2()
		{
			string text = string.Empty;
			if (!parser.ParseString		(out text))
				parent.ThrowException(FileConverterStrings.RenameSpecialFieldsException);

			string newText = SpecialField.ConvertKeywords(text);
			if (newText != text)
			{
				text = text.Replace("'", "''");
				newText = newText.Replace("'", "''");

				text = text.Replace("\"", "\"\"");
				newText = newText.Replace("\"", "\"\"");

				parent.ReplaceWord(parser.CurrentPos + 1, text, newText, true);
			}
		}

		//--------------------------------------------------------------------------------
		protected void RenameRelease()
		{
			parser.SkipToken();//Release
			int release;

			if (!parser.ParseInt(out release))
				throw new FileConverterException(FileConverterStrings.RenameReleaseException, parser.Filename, parser.CurrentLine, parser.CurrentPos);

			if (release == 6 || release == 7)
				parent.ReplaceWord(parser.CurrentPos, release.ToString(), woormRelease.ToString(), true); 
			else
				throw new FileConverterException(FileConverterStrings.WrongReportRelease, parser.Filename, parser.CurrentLine, parser.CurrentPos);
		}
	}
}
