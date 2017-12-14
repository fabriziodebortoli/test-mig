using System;
using System.Collections;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.FileConverter
{
	/// <summary>
	/// Summary description for CPPTableParser.
	/// </summary>
	//================================================================================
	public class CPPTableParser: ObjectParser
	{
		enum CPPToken 
		{
			GETSTATICNAME	= Token.USR01,
			RETURN			= Token.USR02,
			STRINGWRAPPER	= Token.USR03,
			BINDRECORD		= Token.USR04,
			BINDDATA		= Token.USR05,
			BINDADDONDATA	= Token.USR06,
			COLUMNWRAPPER	= Token.USR07,
			TABLEWRAPPER	= Token.USR08,
			FN				= Token.USR09
		}

		private Hashtable	tableList;
		private string		currentClass;

		//---------------------------------------------------------------------------------------------------
		public CPPTableParser(string fileName) : base(fileName)
		{
			this.checkLogErrorsToValidateParsing = false;
		}
		
		//---------------------------------------------------------------------------------------------------
		protected override void InitParser(Parser parser)
		{
			base.InitParser(parser);
			
			parser.UserKeywords.Add("GetStaticName",	CPPToken.GETSTATICNAME);
			parser.UserKeywords.Add("return",			CPPToken.RETURN);
			parser.UserKeywords.Add("_T",				CPPToken.STRINGWRAPPER);
			parser.UserKeywords.Add("_NS_FLD",			CPPToken.COLUMNWRAPPER);
			parser.UserKeywords.Add("_NS_TBL",			CPPToken.TABLEWRAPPER);
			parser.UserKeywords.Add("BindRecord",		CPPToken.BINDRECORD);
			parser.UserKeywords.Add("BIND_DATA",		CPPToken.BINDDATA);
			parser.UserKeywords.Add("BIND_ADDON_DATA",	CPPToken.BINDADDONDATA);
			parser.UserKeywords.Add("FN",				CPPToken.FN);
		}

		//---------------------------------------------------------------------------------------------------
		public override bool Parse()
		{
			PrepareTableList();
			
			return base.Parse();
		}

		//---------------------------------------------------------------------------------------------------
		protected override void ProcessBuffer()
		{
			string objectName = string.Empty, tableClass=string.Empty;

			while (!parser.Eof)
			{
				switch(parser.LookAhead())
				{
					case Token.ID:
						parser.ParseID(out tableClass);	
						break;
					case (Token)CPPToken.BINDRECORD:
						currentClass = tableClass;
						parser.SkipToken();
						break;
					case Token.COLON:
						parser.SkipToken();
						if (parser.Parsed(Token.COLON) &&
							parser.Parsed((Token)CPPToken.GETSTATICNAME))
						{
							if (!GetTableName(out objectName)) break;					
							ReplaceWord(parser.CurrentPos + 1, objectName, TranslateTable(objectName, true), true);
						}
						break;
					case (Token)CPPToken.BINDDATA:
					case (Token)CPPToken.BINDADDONDATA:
						if(!GetColumnName(out objectName))
							ThrowException(FileConverterStrings.ErrorReadColumnName);
						
						if (objectName.Length == 0) continue;

						string tableName = currentClass == null ? null : tableList[currentClass] as string;
						if (tableName == null)
							tableName = GetMasterTable();
						if (tableName == null)
							ThrowException(FileConverterStrings.ErrorReadTableName);
						ReplaceWord(parser.CurrentPos + 1, objectName, TranslateColumn(tableName, objectName, true), true);
						break;
					default:
						parser.SkipToken();
						break;
				}	
			}
		}

		//--------------------------------------------------------------------------------
		private string GetMasterTable()
		{
			try
			{
				string directory = Path.GetDirectoryName(fileName);
				string libraryToken = Path.GetFileName(directory);
				directory = Path.GetDirectoryName(directory);
			
				string moduleObjectsPath = Path.Combine(directory, "ModuleObjects\\AddOnDatabaseObjects.xml");
			
				string moduleToken = Path.GetFileName(directory);
				directory = Path.GetDirectoryName(directory);
				string appToken = Path.GetFileName(directory);

				if (!File.Exists(moduleObjectsPath)) return null;

				string ns = appToken + "." + moduleToken + "." + moduleToken + libraryToken;
				XmlDocument d = new XmlDocument();
				d.Load(moduleObjectsPath);
				XmlNodeList list = d.SelectNodes("//Table[@namespace]/node()[@namespace]");
				XmlElement foudEl = null;
				foreach (XmlElement el in list)
				{
					if (string.Compare(el.GetAttribute("namespace"), ns, true) == 0) 
					{
						foudEl = el;
						break;
					}
				}
			
				if (foudEl == null) return null;

				foudEl = foudEl.ParentNode as XmlElement;

				string tableNamespace = foudEl.GetAttribute("namespace");
				return tableNamespace.Substring(tableNamespace.LastIndexOf(".") + 1);
			}
			catch(Exception ex)
			{
				GlobalContext.LogManager.Message(ex.Message, DiagnosticType.Error);
				return null;
			}
		}

		//---------------------------------------------------------------------------------------------------
		private bool PrepareTableList()
		{
			tableList = new Hashtable();
			string tableClass = null, tableName;
			
			parser = new Parser(Parser.SourceType.FromFile);
			InitParser(parser);
			parser.Open(fileName);

			while (!parser.Eof)
			{
				switch(parser.LookAhead())
				{
					case Token.ID:
						parser.ParseID(out tableClass);	
						break;
					case Token.COLON:
						parser.SkipToken();
						if (parser.Parsed(Token.COLON) &&
							parser.Parsed((Token)CPPToken.GETSTATICNAME))
						{
							if (GetTableName(out tableName))
								tableList.Add(tableClass, tableName);
						}
						break;
					default:
						parser.SkipToken();
						break;
				}
			}

			parser.Close();
			parser = null;
			return tableList.Count > 0;
			
		}

		//---------------------------------------------------------------------------------------------------
		public bool GetTableName(out string tableName)
		{
			tableName = string.Empty;
			// mi aspetto i seguenti tokens: 
			// () { return _T("<nome_tabella>")

			if (!parser.ParseOpen() || 
				!parser.ParseClose() || 
				!parser.ParseBraceOpen() ||
				!parser.ParseTag((Token)CPPToken.RETURN))
				return false;
			
			if (!parser.Parsed((Token)CPPToken.STRINGWRAPPER) &&
				!parser.Parsed((Token)CPPToken.TABLEWRAPPER))
				return false;

			return
				parser.ParseOpen() && 
				parser.ParseString(out tableName);
		}

		//---------------------------------------------------------------------------------------------------
		public bool GetColumnName(out string columnName)
		{
			columnName = string.Empty;
			// mi aspetto i seguenti tokens: 
			// BIND_DATA	(_T("<nome_colonna>"), 
			// oppure
			// BIND_ADDON_DATA	(_T("<nome_colonna>"), 
			// al posto di _T posso trovare anche _NS_FLD

			parser.SkipToken();
			if (!parser.ParseOpen())
				return false;
 
			if (parser.Parsed((Token)CPPToken.FN))
				return true;

			if (!parser.Parsed((Token)CPPToken.STRINGWRAPPER) &&
				!parser.Parsed((Token)CPPToken.COLUMNWRAPPER))
				return false;
			
			return 
				parser.ParseOpen() && 
				parser.ParseString(out columnName);
		}
		
	}
}
