using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Microarea.Library.PEFileAnalyzer;
using Microarea.Library.SqlScriptUtility;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.TBWizardProjects
{
	//=================================================================================
	public class ReverseEngineer
	{
		#region public events
		public event TBWizardEventHandler StartApplicationImport = null;
		public event TBWizardEventHandler StartModuleImport = null;
		public event TBWizardEventHandler StartLibraryImport = null;
		public event TBWizardEventHandler ParseTableCreationScript = null;
		public event TBWizardEventHandler StartLibraryObjectsImport = null;
		public event TBWizardEventHandler EndApplicationImport = null;
	
		public delegate void ImportFailedEventHandler(object sender, string aApplicationName);
		public event ImportFailedEventHandler ApplicationImportFailed = null;

		public event InitProgressBarEventHandler InitProgressBar = null;
		public event PerformProgressBarStepEventHandler PerformProgressBarStep = null;
		#endregion

		private IBasePathFinder pathFinder = null;
		private WizardApplicationInfo applicationInfo = null;
		private bool referencedApplication = true;
		private readonly string[] hotLinkDefaultComboColumnNames = null;
		private SqlRecordStructuresInfo recordStructuresInfo = null;

		#region ReverseEngineer private constants for parsing of Dbts.xml
		private const string XML_DOCUMENT_DECRIPTION_DBTS_TAG = "DBTs";
		private const string XML_DOCUMENT_DECRIPTION_DBT_MASTER_TAG = "Master";
		private const string XML_DOCUMENT_DECRIPTION_SLAVES_TAG = "Slaves";
		private const string XML_DOCUMENT_DECRIPTION_DBT_SLAVE_TAG = "Slave";
		private const string XML_DOCUMENT_DECRIPTION_DBT_SLAVEBUFFERED_TAG = "SlaveBuffered";
		private const string XML_DOCUMENT_DECRIPTION_DBT_TITLE_TAG = "Title";
		private const string XML_DOCUMENT_DECRIPTION_DBT_TABLE_TAG = "Table";
		private const string XML_DOCUMENT_DECRIPTION_NAMESPACE_ATTRIBUTE = "namespace";
		#endregion

		[DllImport("TBWizardUtilities.dll", EntryPoint="GetPEImports", CharSet=CharSet.Unicode)]
		private static extern bool GetPEImports([MarshalAs(UnmanagedType.LPWStr)] string aPEFileName, [MarshalAs(UnmanagedType.SafeArray)] out object[] array);

		# region Constructors
		//---------------------------------------------------------------------------
		public ReverseEngineer(string aApplicationName, IBasePathFinder aPathFinder, bool isReferencedApplication)
		{
            pathFinder = aPathFinder;
            if (pathFinder == null)
                pathFinder = BasePathFinder.BasePathFinderInstance;

            hotLinkDefaultComboColumnNames = Generics.GetHotLinkComboDefaultColumnNames(pathFinder);

			referencedApplication = isReferencedApplication;
			ApplicationName = aApplicationName;
		}

		//---------------------------------------------------------------------------
		public ReverseEngineer(IBasePathFinder aPathFinder, bool isReferencedApplication) 
			: this(String.Empty, aPathFinder, isReferencedApplication)
		{
		}

		//---------------------------------------------------------------------------
		public ReverseEngineer(IBasePathFinder aPathFinder) 
			: this(aPathFinder, false)
		{
		}
        //---------------------------------------------------------------------------
        public ReverseEngineer(SqlRecordStructuresInfo recordStructuresInfo)
            : this(null, false)
        {
            this.recordStructuresInfo = recordStructuresInfo;
        }
		# endregion

		#region ReverseEngineer private methods
		//---------------------------------------------------------------------------
		private void InitApplicationInfo(string aApplicationName)
		{
			applicationInfo = null;

			if (aApplicationName == null || aApplicationName.Trim().Length == 0)
				return;

			IBaseApplicationInfo baseAppInfo = pathFinder.GetApplicationInfoByName(aApplicationName);
			if (baseAppInfo == null)
			{
				if (ApplicationImportFailed != null)
					ApplicationImportFailed(this, aApplicationName);
				
				return;
			}

			applicationInfo = new WizardApplicationInfo(aApplicationName, true, referencedApplication);

			if (StartApplicationImport != null)
				StartApplicationImport(this, new TBWizardEventArgs(applicationInfo, TBWizardEventArgs.ActionTaken.ImportingApplication));

			applicationInfo.Type = baseAppInfo.ApplicationType;

			if (baseAppInfo.ApplicationConfigInfo != null)
			{
				applicationInfo.Version = baseAppInfo.ApplicationConfigInfo.Version;
				applicationInfo.DbSignature = baseAppInfo.ApplicationConfigInfo.DbSignature;
			}

			if (baseAppInfo.Modules != null)
			{
				if (InitProgressBar != null)
					InitProgressBar(this, baseAppInfo.Modules.Count);
				
				foreach(IBaseModuleInfo baseModuleInfo in baseAppInfo.Modules)
				{
					AddModuleInfo(baseModuleInfo);
					
					if (PerformProgressBarStep != null)
						PerformProgressBarStep(this);
				}

				// Una volta che ho caricato tutti i moduli e le librerie in esse contenute (inserendo in questa
				// prima passata anche tutte le informazioni relative agli enumerativi ed alle tabelle), posso 
				// passare al caricamento dei documenti e dei loro DBT.
				if (applicationInfo.ModulesCount > 0)
				{
					if (InitProgressBar != null)
						InitProgressBar(this, applicationInfo.ModulesCount);

					foreach(WizardModuleInfo aWizardModuleInfo in applicationInfo.ModulesInfo)
					{
						AdjustModuleTablesInfo(aWizardModuleInfo);
						
						LoadModuleLibrariesObjects(aWizardModuleInfo, baseAppInfo);

						LoadModuleExtraAddedColumns(aWizardModuleInfo);
						
						// Adesso vedo anche quali HotLink trovo definiti nel modulo
						LoadModuleHotLinks(aWizardModuleInfo, baseAppInfo);
						
						if (PerformProgressBarStep != null)
							PerformProgressBarStep(this);
					}
				}
			}

			if (EndApplicationImport != null)
				EndApplicationImport(this, new TBWizardEventArgs(applicationInfo, TBWizardEventArgs.ActionTaken.ImportingApplication));
		}

		//---------------------------------------------------------------------------
		private int AddModuleInfo(IBaseModuleInfo aBaseModuleInfo)
		{
			if (applicationInfo == null || aBaseModuleInfo == null)
				return -1;

            WizardModuleInfo aModuleInfo = new WizardModuleInfo(aBaseModuleInfo.ModuleConfigInfo.ModuleName, true, referencedApplication);

			aModuleInfo.Title = aBaseModuleInfo.Title;

			if (aBaseModuleInfo.DatabaseObjectsInfo != null)
			{
				aModuleInfo.DbSignature = aBaseModuleInfo.DatabaseObjectsInfo.Signature;
				aModuleInfo.DbReleaseNumber = (uint)aBaseModuleInfo.DatabaseObjectsInfo.Release;
			}

			int moduleIndex = applicationInfo.AddModuleInfo(aModuleInfo, false);
			if (moduleIndex >= 0)
			{
				if (StartModuleImport != null)
					StartModuleImport(this, new TBWizardEventArgs(aModuleInfo, TBWizardEventArgs.ActionTaken.ImportingModule));
			
				// Carico le informazioni relative agli enumerativi del modulo
				AddModuleEnums(aModuleInfo, aBaseModuleInfo);

				// Carico le informazioni relative alle librerie del modulo
				if (aBaseModuleInfo.Libraries != null)
				{
					foreach(ILibraryInfo libraryInfo in aBaseModuleInfo.Libraries)
						AddLibraryInfo(aModuleInfo, libraryInfo);
				}
			}

			return moduleIndex;
		}
		
		//---------------------------------------------------------------------------
		private void AddModuleEnums(WizardModuleInfo aModuleInfo, IBaseModuleInfo aBaseModuleInfo)
		{
			if (pathFinder == null || aModuleInfo == null)
				return;

			string moduleObjectsPath = WizardCodeGenerator.GetStandardModuleObjectsPath(pathFinder, aModuleInfo);
			if (moduleObjectsPath == null || moduleObjectsPath.Length == 0)
				return;

			string moduleEnumsFilename = moduleObjectsPath + 
				Path.DirectorySeparatorChar	+
				NameSolverStrings.EnumsXml;

			if (!File.Exists(moduleEnumsFilename))
				return;

			Enums aEnumsParser = new Enums();

			aEnumsParser.LoadXml(moduleEnumsFilename, aBaseModuleInfo, false);

			if (aEnumsParser.Tags == null || aEnumsParser.Tags.Count == 0)
				return;

			foreach (EnumTag aEnumTag in aEnumsParser.Tags)
			{
				WizardEnumInfo enumToAdd = new WizardEnumInfo(aEnumTag.Name, aEnumTag.Value, true, referencedApplication);
				ushort enumDefaultItemValue = aEnumTag.DefaultValue;

				if (aEnumTag.EnumItems != null && aEnumTag.EnumItems.Count > 0)
				{
					foreach (EnumItem aItem in aEnumTag.EnumItems)
					{
						WizardEnumItemInfo itemToAdd = new WizardEnumItemInfo(aItem.Name, aItem.Value, true);
						
						if (enumDefaultItemValue == aItem.Value)
							itemToAdd.IsDefaultItem = true;

						enumToAdd.AddItemInfo(itemToAdd);
					}
				}
				aModuleInfo.AddEnumInfo(enumToAdd);
			}
		}
		
		//---------------------------------------------------------------------------
		private int AddLibraryInfo(WizardModuleInfo aModuleInfo, ILibraryInfo aLibraryInfo)
		{
			if (pathFinder == null || aModuleInfo == null || aLibraryInfo == null)
				return -1;

			WizardLibraryInfo aWizardLibraryInfo = new WizardLibraryInfo(aLibraryInfo.Name, true, referencedApplication);

            aWizardLibraryInfo.AggregateName = aLibraryInfo.AggregateName;
			aWizardLibraryInfo.SourceFolder = aLibraryInfo.Path;

			aWizardLibraryInfo.FirstResourceId = Generics.TBReservedResourceIdUpperLimit;
			aWizardLibraryInfo.ReservedResourceIdsRange = 0;
			aWizardLibraryInfo.FirstControlId = Generics.TBReservedControlIdUpperLimit;
			aWizardLibraryInfo.ReservedControlIdsRange = 0;
			aWizardLibraryInfo.FirstCommandId = Generics.TBReservedCommandIdUpperLimit;
			aWizardLibraryInfo.ReservedCommandIdsRange = 0;
			aWizardLibraryInfo.FirstSymedId = Generics.TBReservedResourceIdUpperLimit;
			aWizardLibraryInfo.ReservedSymedIdsRange = 0;

			int libraryIndex = aModuleInfo.AddLibraryInfo(aWizardLibraryInfo, false);
			if (libraryIndex >= 0)
			{
				if (StartLibraryImport != null)
					StartLibraryImport(this, new TBWizardEventArgs(aWizardLibraryInfo, TBWizardEventArgs.ActionTaken.ImportingLibrary));
				
				InitLibraryTables(aWizardLibraryInfo);
			}

			return libraryIndex;
		}
	
		//---------------------------------------------------------------------------
		private void InitLibraryTables(WizardLibraryInfo aWizardLibraryInfo)
		{
			if 
				(
				pathFinder == null ||
				aWizardLibraryInfo == null || 
				aWizardLibraryInfo.Name == null || 
				aWizardLibraryInfo.Name.Length == 0 ||
				aWizardLibraryInfo.Module == null
				)
				return;

			string moduleObjectsPath = WizardCodeGenerator.GetStandardModuleObjectsPath(pathFinder, aWizardLibraryInfo.Module);
			if (moduleObjectsPath == null || moduleObjectsPath.Length == 0)
				return;

			string databaseObjectsFilename = moduleObjectsPath + 
				Path.DirectorySeparatorChar	+
				NameSolverStrings.DatabaseObjectsXml;

			if (!File.Exists(databaseObjectsFilename))
				return;

			try
			{
				XmlDocument databaseObjectsDocument = new XmlDocument();
				databaseObjectsDocument.Load(databaseObjectsFilename);

				if (databaseObjectsDocument.DocumentElement == null || 
					String.Compare(databaseObjectsDocument.DocumentElement.Name,  DataBaseObjectsXML.Element.DatabaseObjects) != 0)
					return;

				XmlNode tablesNode = databaseObjectsDocument.DocumentElement.SelectSingleNode("child::" + DataBaseObjectsXML.Element.Tables);
				if (tablesNode == null || !tablesNode.HasChildNodes)
					return;

				XmlNodeList tableNodesList = tablesNode.SelectNodes("child::" + DataBaseObjectsXML.Element.Table);
				if (tableNodesList == null || tableNodesList.Count == 0)
					return;

				string databaseCreateInfoFileName = String.Empty;
				string databaseSQLScriptsPath = WizardCodeGenerator.GetStandardDatabaseScriptsPath(pathFinder, aWizardLibraryInfo.Module);
				if (databaseSQLScriptsPath == null || databaseSQLScriptsPath.Length == 0)
					return;

				databaseSQLScriptsPath += Path.DirectorySeparatorChar +
					Generics.CreateScriptsSubFolderName	+ 
					Path.DirectorySeparatorChar;

				databaseCreateInfoFileName = databaseSQLScriptsPath + NameSolverStrings.CreateInfoXml;
				
				databaseSQLScriptsPath += Generics.SQLServerScriptsSubFolderName + Path.DirectorySeparatorChar;

				XmlElement databaseCreateModuleLevel1Element = null;
				if (databaseCreateInfoFileName != null && databaseCreateInfoFileName.Length > 0 && File.Exists(databaseCreateInfoFileName))
				{
					XmlDocument databaseCreateInfoDocument = new XmlDocument();
					databaseCreateInfoDocument.Load(databaseCreateInfoFileName);

					if 
						(
						databaseCreateInfoDocument.DocumentElement != null && 
						String.Compare(databaseCreateInfoDocument.DocumentElement.Name, Generics.XmlTagDbCreateInfo) == 0
						)
					{
						// Le informazioni di creazione delle tabelle stanno SEMPRE sotto level1
						XmlNode level1Node = databaseCreateInfoDocument.DocumentElement.SelectSingleNode("child::" + Generics.XmlTagDbInfoLevel1);
						if (level1Node != null && (level1Node is XmlElement))
							databaseCreateModuleLevel1Element = (XmlElement)level1Node;
					}
				}
				
				foreach(XmlNode tableNode in tableNodesList)
				{
					if (tableNode == null || !(tableNode is XmlElement) || !((XmlElement)tableNode).HasAttribute(DataBaseObjectsXML.Attribute.Namespace))
						continue;

					string tablefullNamespaceText = ((XmlElement)tableNode).GetAttribute(DataBaseObjectsXML.Attribute.Namespace);
					if (tablefullNamespaceText == null || tablefullNamespaceText.Length == 0)
						continue;

					int tableCreateStep = 0;
					uint tableCreationRelease = 0;
					XmlNode tableCreateNode = tableNode.SelectSingleNode("child::" + DataBaseObjectsXML.Element.Create);
					if (tableCreateNode != null && (tableCreateNode is XmlElement))
					{
						if (((XmlElement)tableCreateNode).HasAttribute(DataBaseObjectsXML.Attribute.Createstep))
						{
							string createStepText = ((XmlElement)tableCreateNode).GetAttribute(DataBaseObjectsXML.Attribute.Createstep);
							if (createStepText != null && createStepText.Length > 0)
								tableCreateStep = int.Parse(createStepText);
						}
						if (((XmlElement)tableCreateNode).HasAttribute(DataBaseObjectsXML.Attribute.Release))
						{
							string releaseText = ((XmlElement)tableCreateNode).GetAttribute(DataBaseObjectsXML.Attribute.Release);
							if (releaseText != null && releaseText.Length > 0)
								tableCreationRelease = uint.Parse(releaseText);
						}
					}
					NameSpace tableNamespace = new NameSpace(tablefullNamespaceText, NameSpaceObjectType.Table);

					string tableName = tableNamespace.Table;
					// Considero le sole tabelle che appartengono alla libreria in questione
					if 
						(
                        // Internamente, il nome del modulo è letto da file system e, quindi, bisogna fare
                        // confronti case insensitive
						String.Compare(tableNamespace.Module, aWizardLibraryInfo.Module.Name, true) == 0 &&
						// ATTENZIONE:
						// Prima l'elemento "Library" di un namespace indicava, come è ovvio e, soprattutto,
						// corretto filosoficamente (perchè coerente con l'analisi), il nome della libreria.
						// Pertanto, il controllo da fare era il seguente:
						//		String.Compare(tableNamespace.Library, aWizardLibraryInfo.Name) == 0 &&
						// Adesso si è voluto, invece, cambiare questa convenzione e usare nel namespace
						// il nome di cartella relativo alla library... sicuramente si abbreviano in generale i
						// namespace e si evitano ripetizioni (ad es. ora si ha "ERP.Accounting.Dbl" al posto di 
						// "ERP.Accounting.AccountingDbl"), ma un namespace non contiene più l'informazione del
						// nome della libreria. Analizzando un namespace si può risalire a tale nome solo dal 
						// nome della cartella, andando a vedere nel file Module.config a quale libreria corrisponde.
						String.Compare(tableNamespace.Library, aWizardLibraryInfo.SourceFolder) == 0 &&
						tableName != null &&
						tableName.Length > 0 
						)
					{
						WizardTableInfo tableInfoToAdd = new WizardTableInfo(tableName, true, referencedApplication);
						//tableInfoToAdd.NameSpace = tablefullNamespaceText;

						// Carico la struttura della tabella analizzando il suo script di creazione
						string scriptName = String.Empty;
						if (databaseCreateModuleLevel1Element != null)
						{
							XmlNode stepNode = databaseCreateModuleLevel1Element.SelectSingleNode("child::" + Generics.XmlTagDbInfoStep + "[@" + Generics.XmlAttributeDbInfoStepNumstep + " = '" + tableCreateStep.ToString() +"']");
							if (stepNode != null && (stepNode is XmlElement) && ((XmlElement)stepNode).HasAttribute(Generics.XmlAttributeDbInfoStepScript))
								scriptName = ((XmlElement)stepNode).GetAttribute(Generics.XmlAttributeDbInfoStepScript);
						}
						if (scriptName == null || scriptName.Length == 0)
							scriptName += tableName + Generics.DatabaseScriptExtension;

						if (
							InitTableStructureFromCreateScript(tableInfoToAdd, databaseSQLScriptsPath + scriptName) &&
							aWizardLibraryInfo.AddTableInfo(tableInfoToAdd, true, false) >= 0
							)
						{
							if (tableCreationRelease > 0)
								tableInfoToAdd.SetCreationDbReleaseNumber(tableCreationRelease, true);
						}
					}
				}
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in ReverseEngineer.InitLibraryTables.", exception.Message);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool InitTableStructureFromCreateScript(WizardTableInfo aWizardTableInfo, string tableSQLScriptFileName)
		{
			if
				(
				aWizardTableInfo == null ||
				aWizardTableInfo.Name == null ||
				aWizardTableInfo.Name.Length == 0 ||
				tableSQLScriptFileName == null ||
				tableSQLScriptFileName.Length == 0 ||
				!File.Exists(tableSQLScriptFileName)
				)
				return false;

			try
			{
				SqlParser scriptParser = new SqlParser();

				if (ParseTableCreationScript != null)
					ParseTableCreationScript(this, new TBWizardEventArgs(aWizardTableInfo, TBWizardEventArgs.ActionTaken.ParseTableScript));

				if (!scriptParser.ParseWithLexan(tableSQLScriptFileName))
					return false;

				return InitTableStructureFromCreateScript(aWizardTableInfo, scriptParser);
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception thrown in ReverseEngineer.InitTableStructureFromCreateScript.", exception.Message);
				return false;
			}
		}


		//---------------------------------------------------------------------------
		private bool InitTableStructureFromCreateScript(WizardTableInfo aWizardTableInfo, SqlParser scriptParser)
		{
			if 
				(
				scriptParser == null ||
				scriptParser.Tables == null ||
				scriptParser.Tables.Count == 0 
				)
				return false;

			try
			{
				SqlTable sqlTableInfo = scriptParser.GetTableByName(aWizardTableInfo.Name);
				if (sqlTableInfo == null)
					return false;

				TableConstraint primaryKeyConstraint = sqlTableInfo.GetPrimaryKeyConstraint();
				if (primaryKeyConstraint != null)
				{
					aWizardTableInfo.PrimaryKeyConstraintName = primaryKeyConstraint.Name;
					aWizardTableInfo.PrimaryKeyClustered = primaryKeyConstraint.Clustered;
				}
				
				if (sqlTableInfo.Columns != null && sqlTableInfo.Columns.Count > 0)
				{
					foreach (TableColumn aParsedColum in sqlTableInfo.Columns)
					{
						WizardTableColumnInfo aColumnToAdd = GetTableColumnInfo(aParsedColum, false, aWizardTableInfo.Name);

						if (aColumnToAdd != null)
						{
							if (primaryKeyConstraint != null)
								aColumnToAdd.IsPrimaryKeySegment = primaryKeyConstraint.IsColumnInvolved(aParsedColum.Name);
							
							aWizardTableInfo.AddColumnInfo(aColumnToAdd);
						}
					}
				}

				if (sqlTableInfo.Constraints != null && sqlTableInfo.Constraints.Count > 0)
				{
					foreach (TableConstraint constraint in sqlTableInfo.Constraints)
					{
						if (constraint.IsPrimaryKeyConstraint) 
							continue;//solo FK
						
						WizardForeignKeyInfo fkInfo = new DBObjectsForeignKeyInfo(constraint.ReferenceTableName);
						fkInfo.ConstraintName = constraint.Name;
						fkInfo.OnUpdateCascade = constraint.OnUpdateCascade;
						fkInfo.OnDeleteCascade = constraint.OnDeleteCascade;

						for (int i = 0; i < constraint.Columns.Count; i++)
						{
							TableColumn c = constraint.Columns[i] as TableColumn;
							if (c == null) 
								continue;
							fkInfo.AddKeySegment(new WizardForeignKeyInfo.KeySegment(c.Name, constraint.ReferenceColumns[i] as string));
						}
						
						aWizardTableInfo.AddForeignKeyInfo(fkInfo);
					}
				}

				IList<WizardTableIndexInfo> indexList = new List<WizardTableIndexInfo>();
				
				foreach (TableIndex i in sqlTableInfo.Indexes)
				{
					WizardTableIndexInfo indexToAdd = new WizardTableIndexInfo(i.Name, false);
					indexToAdd.TableName = i.Table;
					indexToAdd.Unique = i.Unique;
					indexToAdd.NonClustered = i.NonClustered;
					
					foreach (TableColumn c in i.Columns)
						indexToAdd.AddSegmentInfo(new WizardTableColumnInfo(c.Name));
					indexList.Add(indexToAdd);
				}

				aWizardTableInfo.Indexes = indexList;
				return true;
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception thrown in ReverseEngineer.InitTableStructureFromCreateScript.", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------------
		private WizardTableColumnInfo GetTableColumnInfo(TableColumn aParsedColum, bool isExtraAdded)
		{
			return GetTableColumnInfo(aParsedColum, isExtraAdded, string.Empty);
		}

		///<summary>
		/// GetTableColumnInfo
		/// Conversione da TableColumn a WizardTableColumnInfo (il tableName viene valorizzato solo per il tool
		/// di migrazione alla 3.0)
		///</summary>
		//---------------------------------------------------------------------------
		private WizardTableColumnInfo GetTableColumnInfo(TableColumn aParsedColum, bool isExtraAdded, string tableName)
		{
			if (aParsedColum == null)
				return null;

			WizardTableColumnInfo aColumnToAdd = new WizardTableColumnInfo(aParsedColum.Name, true, isExtraAdded);
			
			// ritorna il datatype associato alla colonna
			WizardTableColumnDataType columnDataType = 
				new WizardTableColumnDataType(WizardTableColumnDataType.GetFromSQLServerDataType(aParsedColum.DataType));

            // Metto a posto i booleani...
            // se il tipo letto dal file sql è char e la sua lunghezza è pari a 1 significa che abbiamo incontrato
            // unu booleano (e non controlliamo il suo DEFAULT, xchè potrebbe non essere espresso)
            if (
                (string.Compare(aParsedColum.DataType, "char", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(aParsedColum.DataType, "nchar", StringComparison.InvariantCultureIgnoreCase) == 0) &&
                aParsedColum.DataLength == 1
                )
                columnDataType = new WizardTableColumnDataType(WizardTableColumnDataType.DataType.Boolean);

			aColumnToAdd.DataType               = columnDataType;
			aColumnToAdd.DataLength             = (uint)aParsedColum.DataLength;
			aColumnToAdd.IsNullable             = aParsedColum.IsNullable;
			aColumnToAdd.IsCollateSensitive     = aParsedColum.IsCollateSensitive;
			aColumnToAdd.IsAutoIncrement        = aParsedColum.IsAutoIncrement;
			aColumnToAdd.Seed                   = aParsedColum.Seed;
			aColumnToAdd.Increment              = aParsedColum.Increment;
			aColumnToAdd.DefaultExpressionValue = aParsedColum.DefaultExpressionValue;
            
            if (aParsedColum.DefaultValue != null || !String.IsNullOrEmpty(aParsedColum.DefaultExpressionValue))
                aColumnToAdd.DefaultConstraintName = aParsedColum.DefaultConstraintName;

			if (recordStructuresInfo != null && !string.IsNullOrEmpty(tableName))
			{

				ColumnInfo ci = recordStructuresInfo.GetColumnInfo(tableName, aColumnToAdd.Name);
				if (ci != null)
				{ 
					columnDataType = new WizardTableColumnDataType(WizardTableColumnDataType.GetDataTypeFromTBXmlValue(ci.TbType));
					aColumnToAdd.DataType = columnDataType;

					if (string.Compare(ci.TbType, WizardTableColumnDataType.TB_XML_DATATYPE_ENUM_VALUE, StringComparison.InvariantCultureIgnoreCase) == 0)
						aColumnToAdd.TbEnum = ci.TbEnum;
				
				}
			}

            if (aParsedColum.DefaultValue != null)// && aParsedColum.DefaultValue.Length > 0)//tolto perchè il def può essere stringa vuota.
            {
                // Metto a posto gli enumerativi...		
                if (applicationInfo != null &&
                    columnDataType.Type == WizardTableColumnDataType.DataType.Long)
                {
                    string columnDefaultValue = aParsedColum.DefaultValue as string;
                    UInt32 enumDefaultValue;
                    if (columnDefaultValue != null && (UInt32.TryParse(columnDefaultValue, out enumDefaultValue)))
                    {
                        if (enumDefaultValue >= 0x00010000)
                        {
                            //Cerco l'enumerativo corrispondente
                            UInt16 enumTag = (UInt16)(enumDefaultValue / 65536);

                            if (Generics.IsValidEnumValue(enumTag))
                            {
                                WizardEnumInfo enumInfo = applicationInfo.GetEnumInfoByValue(enumTag);
                                if (enumInfo != null)
                                {
                                    aColumnToAdd.DataType = new WizardTableColumnDataType(WizardTableColumnDataType.DataType.Enum);
                                    aColumnToAdd.EnumInfo = enumInfo;
                                    columnDefaultValue = enumDefaultValue.ToString();
                                }
                            }
                        }
                    }
                    aColumnToAdd.DefaultValue = columnDefaultValue;
                    return aColumnToAdd;
                }
                aColumnToAdd.DefaultValue = aParsedColum.DefaultValue;
            }
			return aColumnToAdd;
		}

		//---------------------------------------------------------------------------
		private void AdjustModuleTablesInfo(WizardModuleInfo aWizardModuleInfo)
		{
			if (aWizardModuleInfo == null || !aWizardModuleInfo.HasTables)
				return;
		
			// Per ottenere il nome della classe derivata da SqlRecord che nel codice dell'applicazione
			// gestisce il corrispondente tracciato record vado a leggere in DBInfo.
			// Posso anche "mettere a posto" i tipi di dato di ciascuna colonna. Ad esempio, i campi di
			// tipo booleano sul database vengono creati come char di lunghezza 1 e non come tipi logici.
			// Così posso anche aggiustare eventuali dati enumerativi.
			string dbInfoPath = WizardCodeGenerator.GetStandardModuleObjectsPath(pathFinder, aWizardModuleInfo);
			if (dbInfoPath == null || dbInfoPath.Length == 0 || !Directory.Exists(dbInfoPath))
				return;

			foreach(WizardLibraryInfo aLibraryInfo in aWizardModuleInfo.LibrariesInfo)
			{
				if (aLibraryInfo == null || aLibraryInfo.TablesCount == 0)
					continue;

				foreach(WizardTableInfo aTableInfo in aLibraryInfo.TablesInfo)
				{
					string dbInfoFullFileName = dbInfoPath + Path.DirectorySeparatorChar +
						NameSolverStrings.DBInfo +
						Path.DirectorySeparatorChar	+
						aTableInfo.Name + 
						NameSolverStrings.XmlExtension;

					DataParser.AdjustTableInfo(aTableInfo, dbInfoFullFileName);
				}
			}
		}
	
		//---------------------------------------------------------------------------
		private void LoadModuleLibrariesObjects(WizardModuleInfo aModuleInfo, IBaseApplicationInfo baseAppInfo)
		{
			if (pathFinder == null || baseAppInfo == null || aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
				return;

			IBaseModuleInfo baseModuleInfo = baseAppInfo.GetModuleInfoByName(aModuleInfo.Name);
			if (baseModuleInfo == null)
				return;

            StringCollection PEAnalyzed = new StringCollection();
            foreach(WizardLibraryInfo aWizardLibraryInfo in aModuleInfo.LibrariesInfo)
                LoadLibraryObjects(aWizardLibraryInfo, baseModuleInfo.GetLibraryInfoByName(aWizardLibraryInfo.Name), pathFinder.GetTBLoaderPath(), PEAnalyzed);
		}

		//---------------------------------------------------------------------------
		private unsafe void LoadLibraryObjects
			(
				    WizardLibraryInfo	    aWizardLibraryInfo, 
				    ILibraryInfo			aLibraryInfo, 
				    string			        moduleClientOutputPath,
                    StringCollection        PEAnalyzed
			)
		{
			if 
				(
				pathFinder == null || 
				applicationInfo == null ||
				aWizardLibraryInfo == null || 
				aLibraryInfo == null
				)
				return;
	
			if (StartLibraryObjectsImport != null)
				StartLibraryObjectsImport(this, new TBWizardEventArgs(aWizardLibraryInfo, TBWizardEventArgs.ActionTaken.ImportingLibrary));
			
			// Carico le informazioni relative ai documenti definiti nella libreria
			InitLibraryDocuments(aWizardLibraryInfo, aLibraryInfo);

            // se la libreria aggregata è già stata aperta per esaminare le risorse esportate, non lo fa nuovamente
            if (PEAnalyzed.Contains(aLibraryInfo.AggregateName))
                return;
		
			// Adesso, se trovo la corrispondente dll compilata, ricavo dal file PE le
			// librerie importate per vedere se devo ancora aggiungere delle dipendenze
			// e vedo quali identificatori di risorse vengono utilizzati
			if 
				(
				moduleClientOutputPath != null &&
				moduleClientOutputPath.Length > 0 &&
				Directory.Exists(moduleClientOutputPath)
				)
			{
				string libraryFullPath = Path.Combine(moduleClientOutputPath, aLibraryInfo.AggregateName + ".dll");
                
                // segna che la libreria aggregata è già stata esaminata, per evitare di aprirla molte volte
                PEAnalyzed.Add(aLibraryInfo.AggregateName);
		
				if (File.Exists(libraryFullPath))
				{
					PEFileAnalyzer.PEFileAnalyzer libraryAnalyzer = new PEFileAnalyzer.PEFileAnalyzer(libraryFullPath);
					
					string[] libraryImports = libraryAnalyzer.GetImports();
					if (libraryImports != null && libraryImports.Length > 0)
					{
						foreach(object aImportedLibrary in libraryImports)
						{
							if (aImportedLibrary == null || !(aImportedLibrary is string) || ((string)aImportedLibrary).Length == 0)
								continue;

							string importedLibraryName = (string)aImportedLibrary;

							if (importedLibraryName.ToLower().EndsWith(".dll"))
								importedLibraryName = importedLibraryName.Substring(0, importedLibraryName.Length - 4);

							WizardLibraryInfo aDependencyToAdd = applicationInfo.GetLibraryInfoByName(importedLibraryName);
							if (aDependencyToAdd != null)
								aWizardLibraryInfo.AddDependency(aDependencyToAdd);
						}
					}

					//ExportedFunction[] exps = libraryAnalyzer.GetExports();
					EmbeddedResource[] libraryResources = libraryAnalyzer.GetResources();
					if (libraryResources != null && libraryResources.Length > 0)
					{
						ushort minResourceValue = Generics.MaximumResourceId;
						ushort maxResourceValue = Generics.TBReservedResourceIdUpperLimit;
						ushort minControlValue = Generics.MaximumControlId;
						ushort maxControlValue = Generics.TBReservedControlIdUpperLimit;
						ushort minCommandValue = Generics.MaximumCommandId;
						ushort maxCommandValue = Generics.TBReservedCommandIdUpperLimit;
						ushort minSymedValue = Generics.MaximumSymedId;
						ushort maxSymedValue = Generics.TBReservedSymedIdUpperLimit;
						
						foreach(EmbeddedResource aResource in libraryResources)
						{
							if (aResource.Type == EmbeddedResource.ResType.StringTable)
							{
								EmbeddedString[] tableStrings = libraryAnalyzer.GetStringTableStrings(aResource);
								if (tableStrings != null && tableStrings.Length > 0)
								{
									foreach(EmbeddedString aString in tableStrings)
									{
										if (minResourceValue > aString.Id)
											minResourceValue = (ushort)aString.Id;
										if (maxResourceValue < aString.Id)
											maxResourceValue = (ushort)aString.Id;
									}
								}
							}
							else if 
								(
								aResource.Type == EmbeddedResource.ResType.Menu ||
								aResource.Type == EmbeddedResource.ResType.Accelerator
								)
							{
								if (minResourceValue > aResource.Id)
									minResourceValue = (ushort)aResource.Id;
								if (maxResourceValue < aResource.Id)
									maxResourceValue = (ushort)aResource.Id;
							}
							else if (aResource.Type == EmbeddedResource.ResType.Dialog)
							{
								DialogInfo dlgInfo = libraryAnalyzer.GetDialogInfo(aResource);

								if (minResourceValue > dlgInfo.Id)
									minResourceValue = (ushort)dlgInfo.Id;
								if (maxResourceValue < dlgInfo.Id)
									maxResourceValue = (ushort)dlgInfo.Id;

								if (dlgInfo.ControlsCount > 0)
								{
									foreach(DialogControlInfo aControlInfo in dlgInfo.Controls)
									{
										if (aControlInfo.Id < Generics.TBReservedControlIdUpperLimit)
											continue;

										if (minControlValue > aControlInfo.Id)
											minControlValue = (ushort)aControlInfo.Id;
										if (maxControlValue < aControlInfo.Id)
											maxControlValue = (ushort)aControlInfo.Id;
									}
								}
							}
							else if (
								aResource.Type == EmbeddedResource.ResType.Bitmap ||
								aResource.Type == EmbeddedResource.ResType.Cursor ||
								aResource.Type == EmbeddedResource.ResType.Icon ||
								aResource.Type == EmbeddedResource.ResType.Anicursor ||
								aResource.Type == EmbeddedResource.ResType.Aniicon
								)
							{
								if (minSymedValue > aResource.Id)
									minSymedValue = (ushort)aResource.Id;
								if (maxSymedValue < aResource.Id)
									maxSymedValue = (ushort)aResource.Id;
							}							
						}
						
						if (minResourceValue < maxResourceValue)
						{
							aWizardLibraryInfo.FirstResourceId = minResourceValue;
							aWizardLibraryInfo.ReservedResourceIdsRange = (ushort)(maxResourceValue - minResourceValue);
						}

						if (minControlValue < maxControlValue)
						{
							aWizardLibraryInfo.FirstControlId = minControlValue;
							aWizardLibraryInfo.ReservedControlIdsRange = (ushort)(maxControlValue - minControlValue);
						}

						if (minCommandValue < maxCommandValue)
						{
							aWizardLibraryInfo.FirstCommandId = minCommandValue;
							aWizardLibraryInfo.ReservedCommandIdsRange = (ushort)(maxCommandValue - minCommandValue);
						}
					
						if (minSymedValue < maxSymedValue)
						{
							aWizardLibraryInfo.FirstSymedId = minSymedValue;
							aWizardLibraryInfo.ReservedSymedIdsRange = (ushort)(maxSymedValue - minSymedValue);
						}
					}
				
					libraryAnalyzer.Dispose();
				}
			}
		}

		//---------------------------------------------------------------------------
		private void InitLibraryDocuments(WizardLibraryInfo aWizardLibraryInfo, ILibraryInfo aLibraryInfo)
		{
			if 
				(
				pathFinder == null || 
				applicationInfo == null ||
				aWizardLibraryInfo == null || 
				aLibraryInfo == null || 
				aLibraryInfo.Documents == null ||
				aLibraryInfo.Documents.Count == 0
				)
				return;
		
			foreach(DocumentInfo aDocumentInfo in aLibraryInfo.Documents)
			{
				WizardDocumentInfo aWizardDocumentInfo = new WizardDocumentInfo(aDocumentInfo.Name, true, referencedApplication);

				// Nel valore dell'attributo "classhierarchy" viene descritta la gerarchia 
				// della classe del documento a partire da CAbstractFormDoc. 
				// La stringa riporta la catena di discendenza della classe a partire dalla 
				// prima che deriva direttamente da CAbstractFormDoc fino ad arrivare alla 
				// classe stessa del documento, separando fra loro i nomi delle varie classi 
				// con un punto.
				string documentClassHierarchy = aDocumentInfo.Classhierarchy;
				int lastPointIdx = documentClassHierarchy.LastIndexOf('.');
				if (lastPointIdx >= 0 && lastPointIdx < (documentClassHierarchy.Length-1))
				{
					aWizardDocumentInfo.ClassHierarchy = documentClassHierarchy.Substring(0, lastPointIdx);
					aWizardDocumentInfo.ClassName = documentClassHierarchy.Substring(lastPointIdx + 1);
				}
				else
					aWizardDocumentInfo.ClassName = documentClassHierarchy;

				if (aDocumentInfo.IsBatch)
					aWizardDocumentInfo.DefaultViewIsBatch = true;
				else if (aDocumentInfo.IsFinder)
					aWizardDocumentInfo.DefaultViewIsDataFinder = true;

				aWizardDocumentInfo.Title = aDocumentInfo.Title;

				int documentIndex = aWizardLibraryInfo.AddDocumentInfo(aWizardDocumentInfo);
				if (documentIndex >= 0)
				{
					// Vedo per ciascun documento se trovo informazioni sui suoi DBT
					string dbtsDescriptionFileName = pathFinder.GetDbtsPath(aDocumentInfo.NameSpace);
					if (!File.Exists(dbtsDescriptionFileName))
						continue;

					try
					{
						XmlDocument dbtsDescriptionDocument = new XmlDocument();
						dbtsDescriptionDocument.Load(dbtsDescriptionFileName);

						if 
							(
							dbtsDescriptionDocument.DocumentElement == null ||
							String.Compare(dbtsDescriptionDocument.DocumentElement.Name, XML_DOCUMENT_DECRIPTION_DBTS_TAG) != 0
							)
							continue;

						XmlNode masterNode = dbtsDescriptionDocument.DocumentElement.SelectSingleNode("child::" + XML_DOCUMENT_DECRIPTION_DBT_MASTER_TAG);
						if (masterNode == null || !(masterNode is XmlElement) || !((XmlElement)masterNode).HasAttribute(XML_DOCUMENT_DECRIPTION_NAMESPACE_ATTRIBUTE))
							continue;
						
						NameSpace masterNameSpace = new NameSpace(((XmlElement)masterNode).GetAttribute(XML_DOCUMENT_DECRIPTION_NAMESPACE_ATTRIBUTE), NameSpaceObjectType.Dbt);
						if (masterNameSpace.Dbt == null || masterNameSpace.Dbt.Length == 0)
							continue;

						XmlNode masterTableNode = ((XmlElement)masterNode).SelectSingleNode("child::" + XML_DOCUMENT_DECRIPTION_DBT_TABLE_TAG);
						if (masterTableNode == null || !(masterTableNode is XmlElement))
							continue;
						string masterTableName = masterTableNode.InnerText;
						if (masterTableName == null || masterTableName.Length == 0)
							continue;

                        // Devo ricavare dal namespace la corretta libreria di appartenenza del DBT
						WizardLibraryInfo masterLibrary = applicationInfo.GetLibraryInfoByNameSpace(masterNameSpace);
						if (masterLibrary == null)
							continue;

						WizardDBTInfo masterInfo = masterLibrary.GetDBTInfoByName(masterNameSpace.Dbt);
						if (masterInfo == null)
						{
							masterInfo = new WizardDBTInfo(masterNameSpace.Dbt, masterTableName, WizardDBTInfo.DBTType.Master, true, referencedApplication);
							masterLibrary.AddDBTInfo(masterInfo, true);
						}

						// Se il DBT sta in un'altra libreria devo aggiungerla nelle dipendenze
						if (!masterLibrary.Equals(aWizardLibraryInfo))
							aWizardLibraryInfo.AddDependency(masterLibrary);

						aWizardDocumentInfo.AddDBTInfo(masterInfo);

						XmlNode slavesNode = masterNode.SelectSingleNode("child::" + XML_DOCUMENT_DECRIPTION_SLAVES_TAG);
						if (slavesNode != null && (slavesNode is XmlElement) && slavesNode.HasChildNodes)
						{
							foreach (XmlNode aSlaveNode in slavesNode.ChildNodes)
							{
								if 
									(
									aSlaveNode == null || 
									!(aSlaveNode is XmlElement) ||
									(
									String.Compare(aSlaveNode.Name, XML_DOCUMENT_DECRIPTION_DBT_SLAVE_TAG) != 0 &&
									String.Compare(aSlaveNode.Name, XML_DOCUMENT_DECRIPTION_DBT_SLAVEBUFFERED_TAG) != 0
									)
									)
									continue;
								
								NameSpace slaveNameSpace = new NameSpace(((XmlElement)aSlaveNode).GetAttribute(XML_DOCUMENT_DECRIPTION_NAMESPACE_ATTRIBUTE), NameSpaceObjectType.Dbt);
								if (slaveNameSpace.Dbt == null || slaveNameSpace.Dbt.Length == 0)
									continue;

								XmlNode slaveTableNode = ((XmlElement)aSlaveNode).SelectSingleNode("child::" + XML_DOCUMENT_DECRIPTION_DBT_TABLE_TAG);
								if (slaveTableNode == null || !(slaveTableNode is XmlElement))
									continue;
								string slaveTableName = slaveTableNode.InnerText;
								if (slaveTableName == null || slaveTableName.Length == 0)
									continue;
								
								// Devo ricavare dal namespace la corretta libreria di appartenenza del DBT
								WizardLibraryInfo slaveLibrary = applicationInfo.GetLibraryInfoByNameSpace(slaveNameSpace);
								if (slaveLibrary == null)
									continue;

								WizardDBTInfo slaveInfo = slaveLibrary.GetDBTInfoByName(slaveNameSpace.Dbt);
								if (slaveInfo == null)
								{
									WizardDBTInfo.DBTType slaveType = (String.Compare(aSlaveNode.Name, XML_DOCUMENT_DECRIPTION_DBT_SLAVE_TAG) == 0) ? WizardDBTInfo.DBTType.Slave : WizardDBTInfo.DBTType.SlaveBuffered;
									slaveInfo = new WizardDBTInfo(slaveNameSpace.Dbt, slaveTableName, slaveType, true, referencedApplication);
									slaveLibrary.AddDBTInfo(slaveInfo, true);
								}
								
								// Se il DBT sta in un'altra libreria devo aggiungerla nelle dipendenze
								if (!slaveLibrary.Equals(aWizardLibraryInfo))
									aWizardLibraryInfo.AddDependency(slaveLibrary);
								
								slaveInfo.RelatedDBTMaster = masterInfo;

								XmlNode slaveTitleNode = ((XmlElement)aSlaveNode).SelectSingleNode("child::" + XML_DOCUMENT_DECRIPTION_DBT_TITLE_TAG);
								if (slaveTitleNode == null || !(slaveTitleNode is XmlElement))
									slaveInfo.SlaveTabTitle = slaveTitleNode.InnerText;
						
								aWizardDocumentInfo.AddDBTInfo(slaveInfo);
							}
						}
					}
					catch(XmlException exception)
					{
						Debug.Fail("Exception thrown in ReverseEngineer.InitLibraryDocuments.", exception.Message);
					}
				}
			}
		}

		//---------------------------------------------------------------------------
		private void LoadModuleHotLinks(WizardModuleInfo aModuleInfo, IBaseApplicationInfo baseAppInfo)
		{
			if (pathFinder == null || aModuleInfo == null)
				return;

			string referenceObjectsPath = WizardCodeGenerator.GetStandardModulePath(pathFinder, aModuleInfo);
			if (referenceObjectsPath == null || referenceObjectsPath.Length == 0)
				return;

			referenceObjectsPath += Path.DirectorySeparatorChar;
			referenceObjectsPath += Generics.ReferenceObjectsFolderName;

			if (!Directory.Exists(referenceObjectsPath))
				return;

			// Parso tutti i file di HotLink che trovo nella cartella
			DirectoryInfo referenceObjectsDirInfo = new DirectoryInfo(referenceObjectsPath);
			FileInfo[] referenceFiles = referenceObjectsDirInfo.GetFiles("*" + NameSolverStrings.XmlExtension);
			if (referenceFiles == null || referenceFiles.Length == 0)
				return;

			IBaseModuleInfo baseModuleInfo = (baseAppInfo != null) ? baseAppInfo.GetModuleInfoByName(aModuleInfo.Name) : null;

			foreach(FileInfo referenceFile in referenceFiles)
			{
				try
				{
					XmlDocument referenceDocument = new XmlDocument();

					referenceDocument.Load(referenceFile.FullName);

					if 
						(
						referenceDocument.DocumentElement == null ||
						String.Compare(referenceDocument.DocumentElement.Name, ReferenceObjectsXML.Element.HotKeyLink) != 0
						)
						continue;
					
					XmlNode dbFieldNode = referenceDocument.DocumentElement.SelectSingleNode("child::" + ReferenceObjectsXML.Element.DbField);
					if 
						(
						dbFieldNode == null || 
						!(dbFieldNode is XmlElement) ||
						!((XmlElement)dbFieldNode).HasAttribute(ReferenceObjectsXML.Attribute.Name)
						)
						continue;

					string qualifiedColumnName = ((XmlElement)dbFieldNode).GetAttribute(ReferenceObjectsXML.Attribute.Name);
					if (qualifiedColumnName == null || qualifiedColumnName.Length == 0)
						continue;

					qualifiedColumnName = qualifiedColumnName.Trim();
					int dotPos = qualifiedColumnName.IndexOf('.');
					if (dotPos < 0 || dotPos >= (qualifiedColumnName.Length - 1))
						continue;

					string tableName = qualifiedColumnName.Substring(0, dotPos);
					string codeColumnName = qualifiedColumnName.Substring(dotPos + 1);

					WizardTableInfo hotLinkTable = aModuleInfo.GetTableInfoByName(tableName);
					if (hotLinkTable == null)
					{
						if (aModuleInfo.Application != null)
							hotLinkTable = aModuleInfo.Application.GetTableInfoByName(tableName);
											
						if (hotLinkTable == null)
							continue;
					}

					WizardTableColumnInfo codeColumn = hotLinkTable.GetColumnInfoByName(codeColumnName);
					if (codeColumn == null)
						continue;

					XmlNode functionNode = referenceDocument.DocumentElement.SelectSingleNode("child::" + ReferenceObjectsXML.Element.Function);
					if 
						(
						functionNode == null || 
						!(functionNode is XmlElement) ||
						!((XmlElement)functionNode).HasAttribute(ReferenceObjectsXML.Attribute.Namespace)
						)
						continue;

					string namespaceAttribute = ((XmlElement)functionNode).GetAttribute(ReferenceObjectsXML.Attribute.Namespace);
					if (namespaceAttribute == null)
						continue;
					namespaceAttribute = namespaceAttribute.Trim();
					if (namespaceAttribute.Length == 0)
						continue;
	
					NameSpace hotlinkNamespace = new NameSpace(namespaceAttribute, NameSpaceObjectType.Hotlink);

					// Cerco la libreria in cui dover aggiungere l'hotlink
					string hotlinkModuleName = hotlinkNamespace.Module.Trim();
                   	if (String.Compare(hotlinkModuleName, aModuleInfo.Name) != 0)
						continue;
 					
					WizardLibraryInfo libraryInfo = aModuleInfo.GetLibraryInfoBySourceFolder(hotlinkNamespace.Library);

					if (libraryInfo == null || !libraryInfo.IsTableAvailable(hotLinkTable.Name))
						continue;

					// Dovrei anche controllare che l'hotlink sia disponibile ovvero esportato...
					// Per farlo mi occorre il nome della sua classe

					WizardHotKeyLinkInfo hotLinkInfo = new WizardHotKeyLinkInfo(hotLinkTable, true, referencedApplication);

					hotLinkInfo.Name = hotlinkNamespace.Hotlink.Trim();
					if (((XmlElement)functionNode).HasAttribute(ReferenceObjectsXML.Attribute.Localize))
						hotLinkInfo.Title = ((XmlElement)functionNode).GetAttribute(ReferenceObjectsXML.Attribute.Localize);

					hotLinkInfo.CodeColumn = codeColumn;

					XmlNode classNameNode = referenceDocument.DocumentElement.SelectSingleNode("child::" + WizardCodeGenerator.XML_HOTLINK_CLASS_NAME_TAG);
					if  (
						    classNameNode == null || 
						    !(classNameNode is XmlElement)
					    )
						continue;

					// Adesso controllo che la classe venga effettivamente esportata dalla libreria
                    if(Directory.Exists(pathFinder.GetTBLoaderPath()))
                    {
                        string libraryFullPath = Path.Combine(pathFinder.GetTBLoaderPath(), libraryInfo.AggregateName + ".dll");

                        if (File.Exists(libraryFullPath) && !PEFileAnalyzer.PEFileAnalyzer.IsClassConstructorExported(libraryFullPath, classNameNode.InnerText))
                            continue;
                    }

					hotLinkInfo.ClassName = classNameNode.InnerText;
					hotLinkInfo.ReferencedNameSpace = namespaceAttribute;

					// Adesso controllo anche se trovo il nome della colonna descrittiva.
					// Nell'array di stringhe hotLinkDefaultComboColumnNames ci sono i nomi
					// dei campi che compongono l'item di default della combo di un HotLink.
					// Infatti, gli item della combo legata all'HotLink (datasource) vengono
					// composti nella maniera seguente:
					// Se nel file descrittivo dell'HotLink (nella cartella ReferenceObjects
					// del modulo di appartenenza) si ha l'elemento <ComboBox> (come sotto-nodo
					// di <HotKeyLink>) significa che esso può venire rappresentato graficamente
					// mediante una combobox, altrimenti occorre usare una semplice textbox.
					// Se il nodo <ComboBox> esiste, ma non contiene alcun sotto-nodo, vuol dire
					// che vanno presi come campi di decodifica quelli riportati nel file di 
					// Settings di TB.
					// Se il nodo <ComboBox> esiste e ha dei sotto-nodi di tipo <Column>, questi 
					// sono preposti a fornire la struttura della stringa relativa all'item della 
					// combo, elencando i campi che la compongono, eventuali stringhe localizzabili 
					// di congiunzione da inserire tra di essi, nonchè possibili condizioni da 
                    // soddisfare affinchè un campo debba o meno far parte dell'item.
					// Esempio:
					//		<ComboBox>
					// 			<Column source="MA_Banks.Bank" />
					// 			<Column source="MA_Banks.Description" />
					// 			<Column localize="of" source="MA_Banks.City" />
					// 			<Column localize="Ag." source="MA_Banks.Agency" when="MA_Banks.Agency != ''" />
					// 			<Column localize="Bank Code:" source="MA_Banks.ABI" />
					// 			<Column localize="Branch Code:" source="MA_Banks.CAB" />
					//		</ComboBox>
					XmlNode comboBoxNode = referenceDocument.DocumentElement.SelectSingleNode("child::" + ReferenceObjectsXML.Element.ComboBox);
					if (comboBoxNode != null && comboBoxNode is XmlElement)
					{
						if (hotLinkDefaultComboColumnNames != null && hotLinkDefaultComboColumnNames.Length > 0)
						{
							string descriptionColumnName = String.Empty;
							foreach(string aFieldName in hotLinkDefaultComboColumnNames)
							{
								if 
									(
									String.Compare(aFieldName, "@dbfield", true) == 0 ||
									String.Compare(aFieldName, codeColumnName, true) == 0
									)
									continue;

								WizardTableColumnInfo descriptionColumn = hotLinkTable.GetColumnInfoByName(aFieldName);
								if (descriptionColumn != null && descriptionColumn.DataType.IsTextual)
								{
									descriptionColumnName = aFieldName;
									break;
								}
							}

							if (descriptionColumnName.Length > 0)
								hotLinkInfo.DescriptionColumnName = descriptionColumnName;
						}
					}
					else
						hotLinkInfo.ShowCombo = false;

					libraryInfo.AddExtraHotLinkInfo(hotLinkInfo);

				}
				catch(XmlException exception)
				{
					Debug.Fail("Exception thrown in ReverseEngineer.LoadModuleHotLinks (File: " + referenceFile.FullName + "):", exception.Message);
				}
			}
		}

		//---------------------------------------------------------------------------
		private void LoadModuleExtraAddedColumns(WizardModuleInfo aModuleInfo)
		{
			if (pathFinder == null || aModuleInfo == null)
				return;

			string addOnDatabaseObjectsPath = WizardCodeGenerator.GetStandardModuleObjectsPath(pathFinder, aModuleInfo);
			if (addOnDatabaseObjectsPath == null || addOnDatabaseObjectsPath.Length == 0)
				return;

			// Per prima cosa leggo il file ModuleObjects\AddOnDatabaseObjects.xml che ha il seguente
			// formato:
			//	<?xml version="1.0" encoding="utf-8" ?> 
			//	<AddOnDatabaseObjects>
			//		<AdditionalColumns>
			//			<Table namespace="namespace della tabella alla quale vengono aggiunte le colonne">
			//				<AlterTable namespace="namespace della libreria nella quale sono implementate le colonne aggiuntive" release="n" createstep="m" />
			//			</Table> 
			//		</AdditionalColumns>
			//	</AddOnDatabaseObjects>
			// Per ciascuna tabella modificata devo prendere il valore dell'attributo createstep e cercare
			// nel file DatabaseScript\Create\CreateInfo.xml il nome dello script SQL corrispondente.

			addOnDatabaseObjectsPath += Path.DirectorySeparatorChar;
			addOnDatabaseObjectsPath += NameSolverStrings.AddOnDatabaseObjectsXml;

			if (!File.Exists(addOnDatabaseObjectsPath))
				return;

			string databaseSQLScriptsPath = WizardCodeGenerator.GetStandardDatabaseScriptsPath(pathFinder, aModuleInfo);
			if (string.IsNullOrEmpty(databaseSQLScriptsPath))
				return;

			databaseSQLScriptsPath +=	Path.DirectorySeparatorChar +
										Generics.CreateScriptsSubFolderName +
										Path.DirectorySeparatorChar;

			if (!Directory.Exists(databaseSQLScriptsPath))
				return;

			string databaseCreateInfoFileName = databaseSQLScriptsPath + NameSolverStrings.CreateInfoXml;

			if (!File.Exists(databaseCreateInfoFileName))
				return;

			databaseSQLScriptsPath += Generics.SQLServerScriptsSubFolderName + Path.DirectorySeparatorChar;

			try
			{
				XmlDocument databaseObjectsDocument = new XmlDocument();
				databaseObjectsDocument.Load(addOnDatabaseObjectsPath);

				if (databaseObjectsDocument.DocumentElement == null ||
					String.Compare(databaseObjectsDocument.DocumentElement.Name, Generics.AddOnDatabaseObjectsRootTag) != 0)
					return;

				XmlDocument databaseCreateInfoDocument = new XmlDocument();
				databaseCreateInfoDocument.Load(databaseCreateInfoFileName);
				if (databaseCreateInfoDocument.DocumentElement == null ||
					String.Compare(databaseCreateInfoDocument.DocumentElement.Name, Generics.XmlTagDbCreateInfo) != 0)
					return;

				// Le informazioni di creazione delle tabelle stanno SEMPRE sotto level1
				XmlNode level1Node = databaseCreateInfoDocument.DocumentElement.SelectSingleNode("child::" + Generics.XmlTagDbInfoLevel1);
				if (level1Node == null || !(level1Node is XmlElement))
					return;

				XmlNode additionalColumnsNode = databaseObjectsDocument.DocumentElement.SelectSingleNode("child::" + AddOnDatabaseObjectsXML.Element.AdditionalColumns);
				if (additionalColumnsNode == null || !(additionalColumnsNode is XmlElement) || !additionalColumnsNode.HasChildNodes)
					return;

				XmlNodeList tableNodesList = additionalColumnsNode.SelectNodes("child::" + AddOnDatabaseObjectsXML.Element.Table);
				if (tableNodesList == null || tableNodesList.Count == 0)
					return;

				foreach (XmlNode tableNode in tableNodesList)
				{
					if
						(
						tableNode == null ||
						!(tableNode is XmlElement) ||
						!((XmlElement)tableNode).HasAttribute(AddOnDatabaseObjectsXML.Attribute.NameSpace)
						)
						continue;

					XmlNodeList alterTableNodesList = tableNode.SelectNodes("child::" + AddOnDatabaseObjectsXML.Element.AlterTable);
					if (alterTableNodesList == null || alterTableNodesList.Count == 0)
						continue;

					string alteredTableNameSpace = ((XmlElement)tableNode).GetAttribute(AddOnDatabaseObjectsXML.Attribute.NameSpace);
					if (alteredTableNameSpace == null || alteredTableNameSpace.Trim().Length == 0)
						continue;

					NameSpace tableNameSpace = new NameSpace(alteredTableNameSpace.Trim(), NameSpaceObjectType.Table);
					if (!tableNameSpace.IsValid())
						continue;

					foreach (XmlNode alterTableNode in alterTableNodesList)
					{
						if
							(
							alterTableNode == null ||
							!(alterTableNode is XmlElement) ||
							!((XmlElement)alterTableNode).HasAttribute(AddOnDatabaseObjectsXML.Attribute.NameSpace)
							)
							continue;

						string libraryNameSpaceString = ((XmlElement)alterTableNode).GetAttribute(AddOnDatabaseObjectsXML.Attribute.NameSpace);
						if (libraryNameSpaceString == null || libraryNameSpaceString.Trim().Length == 0)
							continue;

						NameSpace libraryNameSpace = new NameSpace(libraryNameSpaceString.Trim(), NameSpaceObjectType.Library);
						if (!libraryNameSpace.IsValid())
							continue;

						if (String.Compare(aModuleInfo.Name, libraryNameSpace.Module) != 0)
							continue;

						WizardLibraryInfo libraryInfo = aModuleInfo.GetLibraryInfoBySourceFolder(libraryNameSpace.Library);
						if (libraryInfo == null)
							continue;

						int addedColumnsCreateStep = 0;
						if (((XmlElement)alterTableNode).HasAttribute(AddOnDatabaseObjectsXML.Attribute.Createstep))
						{
							string createStepText = ((XmlElement)alterTableNode).GetAttribute(AddOnDatabaseObjectsXML.Attribute.Createstep);
							if (createStepText != null && createStepText.Length > 0)
								addedColumnsCreateStep = int.Parse(createStepText);
						}

						XmlNode stepNode = level1Node.SelectSingleNode("child::" + Generics.XmlTagDbInfoStep + "[@" + Generics.XmlAttributeDbInfoStepNumstep + " = '" + addedColumnsCreateStep.ToString() + "']");
						if
							(
							stepNode == null ||
							!(stepNode is XmlElement) ||
							!((XmlElement)stepNode).HasAttribute(Generics.XmlAttributeDbInfoStepScript)
							)
							continue;

						string scriptName = databaseSQLScriptsPath + ((XmlElement)stepNode).GetAttribute(Generics.XmlAttributeDbInfoStepScript);
						/*	if (!File.Exists(scriptName))
								continue;

							SqlParser scriptParser = new SqlParser();

							TableColumnList addedColumns = null;
							if 
								(
								!scriptParser.ParseAlterTableAddedColumns(scriptName, tableNameSpace.Table, out addedColumns) ||
								addedColumns == null || 
								addedColumns.Count == 0
								)
								continue;

							if (addedColumns == null || addedColumns.Count == 0)
								continue;

							WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo = new WizardExtraAddedColumnsInfo(alteredTableNameSpace.Trim(), true, true);
					
							if (((XmlElement)alterTableNode).HasAttribute(AddOnDatabaseObjectsXML.Attribute.Release))
							{
								string releaseText = ((XmlElement)alterTableNode).GetAttribute(AddOnDatabaseObjectsXML.Attribute.Release);
								if (releaseText != null && releaseText.Length > 0)
									aExtraAddedColumnsInfo.CreationDbReleaseNumber = uint.Parse(releaseText);
							}

							foreach(TableColumn aParsedColum in addedColumns)
							{
								WizardTableColumnInfo aColumnToAdd = GetTableColumnInfo(aParsedColum, true);
								aExtraAddedColumnsInfo.AddColumnInfo(aColumnToAdd);
							}
							*/
						WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo = LoadExtraAddedColumns(scriptName, tableNameSpace.Table, alterTableNode, alteredTableNameSpace);
						if (aExtraAddedColumnsInfo == null) continue;
						libraryInfo.AddExtraAddedColumnsInfo(aExtraAddedColumnsInfo);
					}
				}
			}
			catch (XmlException exception)
			{
				Debug.Fail("Exception thrown in ReverseEngineer.LoadModuleExtraAddedColumns.", exception.Message);
			}
		}
		#endregion // ReverseEngineer private methods

		# region Metodi pubblici per il parse SQL
		///<summary>
		/// LoadExtraAddedColumns
		/// Utilizzata per ritornare la struttura delle alter table e update appena parsate
		/// cambiamo il nomeeeeeeeee!!!
		///</summary>
		//---------------------------------------------------------------------------
        public void LoadExtraAddedColumns
			(
			string scriptName, 
			out IList<DBObjectsExtraAddedColumnsInfo> addedColumns, 
			out IList<TableUpdate> updateColumns
			)
		{
			updateColumns = null;// elenco delle operazioni di UPDATE
            addedColumns = new List<DBObjectsExtraAddedColumnsInfo>();// elenco delle operazioni di ALTER

			if (!File.Exists(scriptName))
				return;

			SqlParser scriptParser = new SqlParser();

			Hashtable addedColumnsForTables = null; // elenco delle AddOnColumns aggiunte tramite ALTER TABLE

			if (!scriptParser.ParseAlterTableAddedColumns(scriptName, out addedColumnsForTables, out updateColumns) ||
				(addedColumnsForTables == null || addedColumnsForTables.Count == 0) &&
				(updateColumns == null || updateColumns.Count == 0))
				return;

			foreach (DictionaryEntry e in addedColumnsForTables)
			{
				// il primo parametro dovrebbe essere il namespace, ma nel file sql non ce l'ho
				// verrà poi scritto leggendo dal file AddOnDatabaseObjects.xml
                WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo = new DBObjectsExtraAddedColumnsInfo(string.Empty, e.Key as string);
				foreach (TableColumn aParsedColum in e.Value as TableColumnList)
				{
					WizardTableColumnInfo aColumnToAdd = GetTableColumnInfo(aParsedColum, true, e.Key as string);
					aExtraAddedColumnsInfo.AddColumnInfo(aColumnToAdd);
				}
                addedColumns.Add(aExtraAddedColumnsInfo as DBObjectsExtraAddedColumnsInfo);
			}

			return;
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo LoadExtraAddedColumns
			(
			string scriptName, 
			string tableName, 
			XmlNode alterTableNode, 
			string alteredTableNameSpace
			)
		{
			if (!File.Exists(scriptName))
				return null;

			SqlParser scriptParser = new SqlParser();

			TableColumnList addedColumns = null;
			if (!scriptParser.ParseAlterTableAddedColumns(scriptName, tableName, out addedColumns) ||
				addedColumns == null || addedColumns.Count == 0)
				return null;

			WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo = new WizardExtraAddedColumnsInfo(alteredTableNameSpace.Trim(), true, true);

			string releaseText = ((XmlElement)alterTableNode).GetAttribute(AddOnDatabaseObjectsXML.Attribute.Release);
			if (releaseText != null && releaseText.Length > 0)
				aExtraAddedColumnsInfo.CreationDbReleaseNumber = uint.Parse(releaseText);

			foreach (TableColumn aParsedColum in addedColumns)
			{
				WizardTableColumnInfo aColumnToAdd = GetTableColumnInfo(aParsedColum, true);
				aExtraAddedColumnsInfo.AddColumnInfo(aColumnToAdd);
			}

			return aExtraAddedColumnsInfo;
		}

		///<summary>
		/// InitTableStructureFromCreateScript
		/// Entry-point per il parse dei file SQL (solo per gli script di CREATE)
		/// Accetta il path di un file sql
		///</summary>
		//---------------------------------------------------------------------------
        public IList<WizardTableInfo> InitTableStructureFromCreateScript(string tableSQLScriptFileName, out StringCollection viewNames, out StringCollection procNames)
		{
            viewNames = procNames = null;
			IList<WizardTableInfo> tables = new List<WizardTableInfo>();
			if (string.IsNullOrEmpty(tableSQLScriptFileName) ||	!File.Exists(tableSQLScriptFileName))
				return tables;

			try
			{
				// richiama il sqlparser (che usa il lexan)
				SqlParser scriptParser = new SqlParser();

				if (!scriptParser.ParseWithNoCommentLexanSqlParser(tableSQLScriptFileName))
					return tables;

                viewNames = scriptParser.ViewNames;
                procNames = scriptParser.ProcedureNames;

				foreach (SqlTable table in scriptParser.Tables)
				{
					WizardTableInfo aWizardTableInfo = new WizardTableInfo(table.ExtendedName);
					InitTableStructureFromCreateScript(aWizardTableInfo, scriptParser);
					tables.Add(aWizardTableInfo);
				}
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception thrown in ReverseEngineer.InitTableStructureFromCreateScript.", exception.Message);
				return tables;
			}

			return tables;
		}

		///<summary>
		/// LoadViewsFromDatabase
		/// - Apre una connessione con la stringa di connessione passata come parametro 
		/// - Estrapola tutte le view dalla INFORMATION_SCHEMA.VIEWS (in realtà dovrà caricare le info
		/// solo per le view del DatabaseObjects.xml
		/// - Carica le informazioni di ogni view
		///</summary>
		//---------------------------------------------------------------------------
		public SqlViewList LoadViewsFromDatabase(string dbConnectionString)
		{
			SqlViewList viewsList = new SqlViewList();
			
			if (string.IsNullOrEmpty(dbConnectionString))
				return viewsList;

			TBConnection myConnection = new TBConnection(dbConnectionString, DBMSType.SQLSERVER);
			TBCommand myCommand;
			IDataReader myReader = null;
			TBDatabaseSchema ds;
			List<string> myViews = new List<string>();
			
			string vObjects = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS";

			try
			{
				myConnection.Open();
				myCommand = new TBCommand(vObjects, myConnection);
				myReader = myCommand.ExecuteReader();
				
				if (myReader != null)
				{
					while (myReader.Read())
						myViews.Add(myReader["TABLE_NAME"].ToString());

					myReader.Close();
					myReader.Dispose();
				}
				
				if (myViews.Count == 0)
					return viewsList;

				ds = new TBDatabaseSchema(myConnection);

				foreach (string view in myViews)
				{
					// per ogni view estratta dal db vado a leggermi le sue info
					SqlView sv = LoadViewInfo(view, ds);
					viewsList.Add(sv);
				}
			}
			catch (TBException exception)
			{
				Debug.Fail("Exception thrown in ReverseEngineer.LoadViewsFromDatabase.", exception.Message);
				return viewsList;
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
				{
					myReader.Close();
					myReader.Dispose();
				}

				if (myConnection != null && myConnection.State != ConnectionState.Closed)
				{
					myConnection.Close();
					myConnection.Dispose();
				}
			}

			return viewsList;
		}

		///<summary>
		/// LoadViewInfo
		/// Riempie un oggetto di tipo SqlView
		///</summary>
		//---------------------------------------------------------------------------
		private SqlView LoadViewInfo(string view, TBDatabaseSchema ds)
		{
			string definition = string.Empty;

			// carica le informazioni dalle tabelle di sistema su quella view
			DataTable dtView = ds.LoadViewOrProcedureInfo(view, DBObjectTypes.VIEW, out definition);
			
			SqlView myView = new SqlView(view);
			myView.SqlDefinition = definition;

			if (dtView == null)
				return myView;

			string colName = string.Empty;
			string colType = string.Empty;
			uint colLength = 0;
			bool isCollateSensitive = true;
			uint colTbEnum = 0;

			foreach (DataRow dr in dtView.Rows)
			{
				colName = dr["Name"].ToString();
				colType = dr["Type"].ToString();
                colLength = Convert.ToUInt32(dr["Length"].ToString());
				// ritorna il datatype associato alla colonna
				WizardTableColumnDataType columnDataType =
					new WizardTableColumnDataType(WizardTableColumnDataType.GetFromSQLServerDataType(colType));

				// associo prima il tipo e poi lo riassegno per i casi anomali (enum/double)
				colType = WizardTableColumnDataType.Unparse(columnDataType);

				if (columnDataType.Type == WizardTableColumnDataType.DataType.Long)
				{
					if (recordStructuresInfo != null)
					{
						ColumnInfo ci = recordStructuresInfo.GetColumnInfo(view, colName);

						if (ci != null && string.Compare(ci.TbType, WizardTableColumnDataType.TB_XML_DATATYPE_ENUM_VALUE, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							colType = WizardTableColumnDataType.DataType.Enum.ToString();
							colTbEnum = ci.TbEnum;
						}
					}
				}

				if (columnDataType.Type == WizardTableColumnDataType.DataType.Double)
				{
					if (recordStructuresInfo != null)
					{
						ColumnInfo ci = recordStructuresInfo.GetColumnInfo(view, colName);

						if (ci != null)
						{
							if (string.Compare(ci.TbType, WizardTableColumnDataType.TB_XML_DATATYPE_QUANTITY_VALUE, StringComparison.InvariantCultureIgnoreCase) == 0)
								colType = WizardTableColumnDataType.DataType.Quantity.ToString();

							if (string.Compare(ci.TbType, WizardTableColumnDataType.TB_XML_DATATYPE_PERC_VALUE, StringComparison.InvariantCultureIgnoreCase) == 0)
								colType = WizardTableColumnDataType.DataType.Percent.ToString();

							if (string.Compare(ci.TbType, WizardTableColumnDataType.TB_XML_DATATYPE_MONEY_VALUE, StringComparison.InvariantCultureIgnoreCase) == 0)
								colType = WizardTableColumnDataType.DataType.Monetary.ToString();
						}
					}
				}

                if (columnDataType.Type == WizardTableColumnDataType.DataType.String && colLength == 1)
                {
                    if (recordStructuresInfo != null)
                    {
                        ColumnInfo ci = recordStructuresInfo.GetColumnInfo(view, colName);

                        if (ci != null)
                        {
                            if (string.Compare(ci.TbType, WizardTableColumnDataType.TB_XML_DATATYPE_BOOLEAN_VALUE, StringComparison.InvariantCultureIgnoreCase) == 0)
                                colType = WizardTableColumnDataType.DataType.Boolean.ToString();
                        }
                    }
                }
				
				myView.AddColumn(colName, colType, colLength, isCollateSensitive, colTbEnum);
			}

			return myView;
		}

		///<summary>
		/// LoadProceduresFromDatabase
		/// - Apre una connessione con la stringa di connessione passata come parametro 
		/// - Estrapola tutte le view dalla INFORMATION_SCHEMA.VIEWS (in realtà dovrà caricare le info
		/// solo per le view del DatabaseObjects.xml
		/// - Carica le informazioni di ogni view
		///</summary>
		//---------------------------------------------------------------------------
		public SqlProcedureList LoadProceduresFromDatabase(string dbConnectionString)
		{
			SqlProcedureList proceduresList = new SqlProcedureList();

			if (string.IsNullOrEmpty(dbConnectionString))
				return proceduresList;

			TBConnection myConnection = new TBConnection(dbConnectionString, DBMSType.SQLSERVER);
			TBCommand myCommand;
			IDataReader myReader = null;
			TBDatabaseSchema ds;
			List<string> myProcedures = new List<string>();

			string vObjects = "SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES";

			try
			{
				myConnection.Open();
				myCommand = new TBCommand(vObjects, myConnection);
				myReader = myCommand.ExecuteReader();

				if (myReader != null)
				{
					while (myReader.Read())
						myProcedures.Add(myReader["ROUTINE_NAME"].ToString());

					myReader.Close();
					myReader.Dispose();
				}

				if (myProcedures.Count == 0)
					return proceduresList;

				ds = new TBDatabaseSchema(myConnection);

				foreach (string proc in myProcedures)
				{
					// per ogni stored procedure estratta dal db vado a leggermi le sue info
					SqlProcedure sp = LoadProcedureInfo(proc, ds);
					proceduresList.Add(sp);
				}
			}
			catch (TBException exception)
			{
				Debug.Fail("Exception thrown in ReverseEngineer.LoadProceduresFromDatabase.", exception.Message);
				return proceduresList;
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
				{
					myReader.Close();
					myReader.Dispose();
				}

				if (myConnection != null && myConnection.State != ConnectionState.Closed)
				{
					myConnection.Close();
					myConnection.Dispose();
				}
			}

			return proceduresList;
		}

		///<summary>
		/// LoadProcedureInfo
		/// Riempie un oggetto di tipo SqlProcedure
		///</summary>
		//---------------------------------------------------------------------------
		private SqlProcedure LoadProcedureInfo(string procedure, TBDatabaseSchema ds)
		{
			string definition = string.Empty;

			// carica le informazioni dalle tabelle di sistema su quella view
			DataTable dtProc = ds.LoadViewOrProcedureInfo(procedure, DBObjectTypes.ROUTINE, out definition);

			SqlProcedure myProcedure = new SqlProcedure(procedure);
			myProcedure.SqlDefinition = definition;

			if (dtProc == null)
				return myProcedure;

			string colName = string.Empty;
			string colType = string.Empty;
			uint colLength = 0;
			bool isOutParam = false;
			bool isCollateSensitive = true;
			uint colTbEnum = 0;

			// devo aggiungere d'ufficio il parametro @RETURN_VALUE che serve a TB ma che non ritorna il database
			myProcedure.AddParameter("@RETURN_VALUE", "Long", 0, true, isCollateSensitive, colTbEnum);

			foreach (DataRow dr in dtProc.Rows)
			{
				colName = dr["Name"].ToString();
				colType = dr["Type"].ToString();
                colLength = Convert.ToUInt32(dr["Length"].ToString());
                isOutParam = Convert.ToBoolean(dr["OutParam"]);

				// ritorna il datatype associato alla colonna
				WizardTableColumnDataType columnDataType =
					new WizardTableColumnDataType(WizardTableColumnDataType.GetFromSQLServerDataType(colType));

				// associo prima il tipo e poi lo riassegno per i casi anomali (enum/double)
				colType = WizardTableColumnDataType.Unparse(columnDataType);

				if (columnDataType.Type == WizardTableColumnDataType.DataType.Long)
				{
					if (recordStructuresInfo != null)
					{
						ColumnInfo ci = recordStructuresInfo.GetColumnInfo(procedure, colName);

						if (ci != null && string.Compare(ci.TbType, WizardTableColumnDataType.TB_XML_DATATYPE_ENUM_VALUE, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							colType = "Enum";
							colTbEnum = ci.TbEnum;
						}
					}
				}

				if (columnDataType.Type == WizardTableColumnDataType.DataType.Double)
				{
					if (recordStructuresInfo != null)
					{
						ColumnInfo ci = recordStructuresInfo.GetColumnInfo(procedure, colName);

						if (ci != null)
						{
							if (string.Compare(ci.TbType, WizardTableColumnDataType.TB_XML_DATATYPE_QUANTITY_VALUE, StringComparison.InvariantCultureIgnoreCase) == 0)
								colType = WizardTableColumnDataType.DataType.Quantity.ToString();

							if (string.Compare(ci.TbType, WizardTableColumnDataType.TB_XML_DATATYPE_PERC_VALUE, StringComparison.InvariantCultureIgnoreCase) == 0)
								colType = WizardTableColumnDataType.DataType.Percent.ToString();

							if (string.Compare(ci.TbType, WizardTableColumnDataType.TB_XML_DATATYPE_MONEY_VALUE, StringComparison.InvariantCultureIgnoreCase) == 0)
								colType = WizardTableColumnDataType.DataType.Monetary.ToString();
						}
					}
				}
                if (columnDataType.Type == WizardTableColumnDataType.DataType.String && colLength == 1)
                {
                    if (recordStructuresInfo != null)
                    {
                        ColumnInfo ci = recordStructuresInfo.GetColumnInfo(procedure, colName);

                        if (ci != null)
                        {
                            if (string.Compare(ci.TbType, WizardTableColumnDataType.TB_XML_DATATYPE_BOOLEAN_VALUE, StringComparison.InvariantCultureIgnoreCase) == 0)
                                colType = WizardTableColumnDataType.DataType.Boolean.ToString();
                        }
                    }
                }
				
				myProcedure.AddParameter(colName, colType, colLength, isOutParam, isCollateSensitive, colTbEnum);
			}

			return myProcedure;
		}
		# endregion

		#region ReverseEngineer public properties

		//---------------------------------------------------------------------------
		public WizardApplicationInfo ApplicationInfo { get { return applicationInfo; } }

		//---------------------------------------------------------------------------
		public string ApplicationName 
		{
			get { return (applicationInfo != null) ? applicationInfo.Name : String.Empty; } 

			set
			{
				InitApplicationInfo(value);
			}
		}

		#endregion

		#region ReverseEngineer public methods

		//---------------------------------------------------------------------------
		public static string[] GetAvailableApplicationNames(IBasePathFinder aPathFinder, ApplicationType aApplicationType)
		{
            if (aPathFinder == null)
                aPathFinder = BasePathFinder.BasePathFinderInstance;

            if (aPathFinder.ApplicationInfos == null || aPathFinder.ApplicationInfos.Count == 0)
				return null;

			ArrayList applicationNames = new ArrayList();

            foreach (IBaseApplicationInfo baseApplicationInfo in aPathFinder.ApplicationInfos)
			{
				if 
					(
					aApplicationType != ApplicationType.All && 
					aApplicationType != ApplicationType.Undefined && 
					baseApplicationInfo.ApplicationType != aApplicationType
					)
					continue;

				applicationNames.Add(baseApplicationInfo.Name);
			}

			return (applicationNames.Count > 0) ? (string[])applicationNames.ToArray(typeof(string)) : null;
		}

		//---------------------------------------------------------------------------
		public static string[] GetAllAvailableApplicationNames(IBasePathFinder aPathFinder)
		{
			return GetAvailableApplicationNames(aPathFinder, ApplicationType.All);
		}
		
		//---------------------------------------------------------------------------
		public static string[] GetAvailableTaskBuilderApplicationNames(IBasePathFinder aPathFinder)
		{
			return GetAvailableApplicationNames(aPathFinder, ApplicationType.TaskBuilderApplication);
		}
		
		#endregion

	}
}
