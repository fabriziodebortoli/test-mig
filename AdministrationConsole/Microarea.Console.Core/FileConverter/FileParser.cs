using System;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.FileConverter
{
	//================================================================================
	public abstract class FileParser
	{
		public		bool				modified = false;
		public		string				fileName;
		public		string				destinationFileName;
	
		public string OutputFileName { get { return destinationFileName; } }

		//---------------------------------------------------------------------------------------------------
		public FileParser(string fileName)
		{
			this.fileName = fileName;
			this.destinationFileName = fileName;
		}

		//--------------------------------------------------------------------------------
		public FileParser(string fileName, string destinationFileName)
		{
			this.fileName = fileName;
			this.destinationFileName = destinationFileName;
		}

		//--------------------------------------------------------------------------------
		protected ITableTranslator TableTranslator { get { return GlobalContext.TableTranslator; } }

		//---------------------------------------------------------------------------------------------------
		public abstract bool Parse();
		
		//---------------------------------------------------------------------------------------------------
		public string TranslateTable(string tableName, bool ifEmptyReturnSourceName)
		{
			if (tableName == null || tableName.Length == 0) return tableName;
#if TRANSLATE_DEBUG
			return "DBG_" + tableName + "_DBG";
#else
			string newName = TableTranslator.TranslateTable(tableName);
			if (newName == null || newName.Length == 0)
			{
				GlobalContext.LogManager.Message(String.Format(FileConverterStrings.InexistentTableTranslation, tableName), fileName, DiagnosticType.Warning, null);
				if (ifEmptyReturnSourceName) return tableName;
			}
			return newName.Trim();
#endif
		}

		//---------------------------------------------------------------------------------------------------
		public string TranslateColumn(string tableName, string columnName, bool ifEmptyReturnSourceName)
		{
			if (columnName == null || columnName.Length == 0) return columnName;
#if TRANSLATE_DEBUG
			return "DBG_" + columnName + "_DBG";
#else
			string newName = TableTranslator.TranslateColumn(tableName, columnName);
			if (newName == null || newName.Length == 0)
			{
				GlobalContext.LogManager.Message(String.Format(FileConverterStrings.InexistentColumnOfTableTranslation, columnName, tableName), fileName, DiagnosticType.Warning, null);
				if (ifEmptyReturnSourceName) return columnName;
			}
			if 
				(
					!ifEmptyReturnSourceName &&
					string.Compare(columnName, newName, true) == 0 &&
					!TableTranslator.ExistColumn(tableName, columnName)
				)
			{
				GlobalContext.LogManager.Message(String.Format("Incontrata colonna custom {0}.{1}", tableName, columnName), fileName, DiagnosticType.Warning, null);
			}
			return newName.Trim();
#endif
		}

		//---------------------------------------------------------------------------------------------------
		public bool ExistColumn(string tableName, string columnName)
		{
#if TRANSLATE_DEBUG
			return true;
#else
			return TableTranslator.ExistColumn(tableName, columnName);
#endif
		}

		//---------------------------------------------------------------------------------------------------
		public string TranslateQualifiedColumn(string qualifiedName)
		{
			return TranslateQualifiedColumn(qualifiedName, false);
		}

		//---------------------------------------------------------------------------------------------------
		public string TranslateQualifiedColumn(string qualifiedName, bool ifEmptyReturnSourceName)
		{
			string [] tokens = qualifiedName.Split('.');
			if (tokens.Length != 2) 
				return qualifiedName; //TODO potrebbe essere un casino !!!!

			string tableName = TranslateTable(tokens[0], false);
			string columnName = TranslateColumn(tokens[0],tokens[1], false);

			if (tableName == string.Empty)
				return qualifiedName; //la tabella è stata eliminata, NON do' errori sulle sue colonne

			if (columnName == string.Empty)
			{
				if (ifEmptyReturnSourceName)
					return qualifiedName;

				if (tokens[1] != string.Empty)
					return string.Empty; //colonna eliminata: il chiamante dovrà segnalare l'errore
			}

			return  tableName + '.' + columnName;			
		}

		//---------------------------------------------------------------------------------------------------
		protected void TryToCheckOut(string fileName)
		{
			if (GlobalContext.SSafe == null) return;

			GlobalContext.SSafe.CheckOutFile(fileName);
		}

		//---------------------------------------------------------------------------------------------------
		protected void TryToCheckOut()
		{
			TryToCheckOut(fileName);
		}

		//---------------------------------------------------------------------------------------------------
		public virtual string GetPosition()
		{
			return string.Empty;
		}

		//--------------------------------------------------------------------------------
		protected void SafeRemoveFile(bool backup)
		{
			if (string.Compare(fileName, destinationFileName, true) != 0)
			{
				if (backup)
				{
					string bkpFileName = Path.ChangeExtension(fileName, "bkp");
					File.Move(fileName, bkpFileName);
				}
				else
				{
					// si assicura che non sia read-only per cancellarlo
					FileInfo file = new FileInfo(fileName);
					file.Attributes = ~FileAttributes.ReadOnly;
					file.Delete();
				}
			}
		}

		//---------------------------------------------------------------------------------------------------
		protected string GetDbtMasterTableName(string aNS)
		{
			try
			{
				string DbtPath = GetStandardDocumentDescriptionPath(new NameSpace(aNS));
				DbtPath = Path.Combine(DbtPath, "Dbts.xml");
	            
				XmlDocument aDoc = new XmlDocument();
				aDoc.Load(DbtPath);
				string xPath;
				if (aNS.StartsWith("Dbt."))
				{
					string dbtNs = aNS.Substring(aNS.IndexOf(".") + 1);
					xPath = string.Format("//node()[@namespace='{0}']/Table[@namespace]", dbtNs);
				}
				else
					xPath = "DBTs/Master/Table[@namespace]";

				XmlNode aNode = aDoc.SelectSingleNode(xPath);
				if (aNode == null)
				{
					GlobalContext.LogManager.Message(string.Format(FileConverterStrings.CannotFindNodeOfQuery, xPath, DbtPath), string.Empty, DiagnosticType.Error, null);
					return null;
				}

				return aNode.FirstChild.Value;
			}
			catch(Exception ex)
			{
				GlobalContext.LogManager.Message(ex.Message, ex.Source, DiagnosticType.Information, null);
				throw new ApplicationException(string.Format(FileConverterStrings.CannotRecoverMasterTable, aNS), ex);
			}
		}
		
		//---------------------------------------------------------------------------------------------------
		protected string GetStandardDocumentDescriptionPath(INameSpace aNS)
		{
			return GlobalContext.PathFinder.GetStandardDocumentDescriptionPath(aNS);
		}

	}
}
