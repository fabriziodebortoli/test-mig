using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.TBWizardProjects
{
    /// <summary>
	/// Summary description for TBWizardProjectParser.
	/// </summary>
	//=========================================================================
	public class TBWizardProjectParser
	{
		public const string ProjectFileExtension		= "tbwproj";
		public const string ProjectFileDialogFilter		= "TBWizard Project (*." + ProjectFileExtension + ")|*." + ProjectFileExtension;
		 
		public const string XML_TAG_APPLICATION							= "Application";
		public const string XML_TAG_REFERENCES							= "References";
		public const string XML_TAG_REFERENCED_APP						= "Reference";
		public const string XML_TAG_MODULES								= "Modules";
		public const string XML_TAG_MODULE								= "Module";
		public const string XML_TAG_LIBRARIES							= "Libraries";
		public const string XML_TAG_LIBRARY								= "Library";
		public const string XML_TAG_TABLES								= "Tables";
		public const string XML_TAG_TABLE								= "Table";
		public const string XML_TAG_COLUMNS								= "Columns";
		public const string XML_TAG_COLUMN								= "Column";
		public const string XML_TAG_FOREIGN_KEYS						= "ForeignKeys";
		public const string XML_TAG_FOREIGN_KEY							= "ForeignKey";
		public const string XML_TAG_FOREIGN_KEY_SEGMENT					= "Segment";
		public const string XML_TAG_DOCUMENTS							= "Documents";
		public const string XML_TAG_DOCUMENT							= "Document";
		public const string XML_TAG_LABELS								= "Labels";
		public const string XML_TAG_LABEL							    = "Label";
		public const string XML_TAG_DBTS								= "DBTs";
		public const string XML_TAG_DBT									= "DBT";
		public const string XML_TAG_DOC_TABBEDPANES						= "TabbedPanes";
		public const string XML_TAG_DOC_TABBEDPANE						= "TabbedPane";
		public const string XML_TAG_CLIENT_DOCUMENTS					= "ClientDocuments";
		public const string XML_TAG_CLIENT_DOCUMENT						= "ClientDocument";
		public const string XML_TAG_DEPENDENCIES						= "Dependencies";
		public const string XML_TAG_DEPENDENCY							= "Dependency";
		public const string XML_TAG_EXTRA_ADDED_COLUMNS					= "ExtraAddedColumns";
		public const string XML_TAG_EXTRA_ADDED_COLUMN					= "ExtraAddedColumn";
		public const string XML_TAG_ENUMS								= "Enums";
		public const string XML_TAG_ENUM								= "Enum";
		public const string XML_TAG_APP_TITLE							= "Title";
		public const string XML_TAG_APP_TYPE							= "Type";
		public const string XML_TAG_APP_PRODUCER						= "Producer";
		public const string XML_TAG_APP_DBSIGNATURE						= "DbSignature";
		public const string XML_TAG_APP_VERSION							= "Version";		
		public const string XML_TAG_APP_SHORTNAME						= "ShortName";
		public const string XML_TAG_APP_EDITION							= "Edition";
		public const string XML_TAG_APP_SOLUTION_TYPE					= "SolutionType";
		public const string XML_TAG_APP_CULTURE							= "Culture";
		public const string XML_TAG_APP_FONT							= "Font";
		public const string XML_TAG_APP_GUID							= "Guid";
		public const string XML_TAG_MOD_TITLE							= "Title";
		public const string XML_TAG_MOD_DBSIGNATURE						= "DbSignature";
		public const string XML_TAG_MOD_DBRELEASE						= "DbReleaseNumber";
        public const string XML_TAG_MOD_GUID                            = "Guid";
		public const string XML_TAG_LIB_SOURCEFOLDER					= "SourceFolder";
		public const string XML_TAG_LIB_MENUTITLE						= "MenuTitle";
		public const string XML_TAG_LIB_GUID							= "Guid";
		public const string XML_TAG_DOC_TITLE							= "Title";
		public const string XML_TAG_DBT_SLAVE_TAB_TITLE					= "TabTitle";
		public const string XML_TAG_DBTCOLUMN_TITLE						= "Title";
		public const string XML_TAG_ENUM_ITEMS							= "Items";
		public const string XML_TAG_ENUM_ITEM							= "Item";
		public const string XML_TAG_DOC_HOTLINK							= "HotLink";
		public const string XML_TAG_SERVER_DOCUMENT						= "ServerDocument";
		public const string XML_TAG_INCLUDE_FILES						= "IncludeFiles";
		public const string XML_TAG_INCLUDE_FILE						= "IncludeFile";
		public const string XML_TAG_TABLE_HISTORY						= "TableHistory";
		public const string XML_TAG_TABLE_HISTORY_STEP					= "HistoryStep";
		public const string XML_TAG_TABLE_HISTORY_EVENTS				= "Events";
		public const string XML_TAG_TABLE_HISTORY_COLUMN_EVENT			= "Event";
		public const string XML_TAG_TABLE_HISTORY_COLUMN				= "Column";
		public const string XML_TAG_TABLE_HISTORY_PREVIOUS				= "PreviousInfo";
		public const string XML_TAG_TABLE_HISTORY_INDEX_EVENT			= "IndexEvent";
		public const string XML_TAG_TABLE_HISTORY_INDEX_INFO			= "IndexInfo";
		public const string XML_TAG_TABLE_HISTORY_INDEX_PREVIOUS_INFO	= "PreviousIndexInfo";
		public const string XML_TAG_TABLE_HISTORY_INDEX_SEGMENT			= "Segment";
		public const string XML_TAG_TABLE_HISTORY_FOREIGN_KEY_EVENT		= "ForeignKeyEvent";
		public const string XML_TAG_TABLE_HISTORY_FOREIGN_KEY_INFO		= "ForeignKeyInfo";
		public const string XML_TAG_REFERENCED_HOTLINK					= "ReferencedHotLink";
		public const string XML_TAG_TABBED_PANE_TITLE					= "Title";
		public const string XML_TAG_INDEXES								= "Indexes";
		public const string XML_TAG_INDEX								= "Index";
		public const string XML_TAG_INDEX_SEGMENT						= "Segment";

		public const string XML_TAG_FIRST_RESOURCE_ID					= "FirstResourceId";
		public const string XML_TAG_FIRST_CONTROL_ID					= "FirstControlId";
		public const string XML_TAG_FIRST_COMMAND_ID					= "FirstCommandId";
		public const string XML_TAG_FIRST_SYMED_ID						= "FirstSymedId";

		// Label attribute
		public const string XML_TEXT_ATTRIBUTE   = "Text";
		public const string XML_TOP_ATTRIBUTE    = "Top";
		public const string XML_LEFT_ATTRIBUTE   = "Left";
		public const string XML_HEIGHT_ATTRIBUTE = "Height";
		public const string XML_WIDTH_ATTRIBUTE  = "Width";

		public const string XML_NAME_ATTRIBUTE								= "name";
		public const string XML_READONLY_ATTRIBUTE							= "read_only";
		public const string XML_CLASS_NAME_ATTRIBUTE						= "class_name";
		public const string XML_APPLICATION_NAME_ATTRIBUTE					= "application_name";
		public const string XML_MODULE_NAME_ATTRIBUTE						= "module_name";
		public const string XML_TABLE_NAMESPACE_ATTRIBUTE					= "table_namespace";
		public const string XML_DB_RELEASE_NUMBER_ATTRIBUTE					= "DbReleaseNumber";
		public const string XML_DB_REL_NUMBER_ATTRIBUTE						= "db_release_number";  //attributo tutto minuscolo,
		public const string XML_INDEX_SEG_COLUMNN_NAME_ATTRIBUTE			= "column_name";		//mantenuto il vecchio per poter leggere i file esistenti con vecchia sintassi
		public const string XML_APP_FONT_SIZEINPOINTS_ATTRIBUTE				= "size_in_points";
		public const string XML_APP_FONT_BOLD_ATTRIBUTE						= "bold";
		public const string XML_APP_FONT_ITALIC_ATTRIBUTE					= "italic";
		public const string XML_LIB_TRAP_DSNCHANGED_ATTRIBUTE				= "trap_dsnchanged";
		public const string XML_LIB_TRAP_APPDATECHANGED_ATTRIBUTE			= "trap_appdatechanged";
		public const string XML_LIBRARY_NAME_ATTRIBUTE						= "library_name";
		public const string XML_TABLE_NAME_ATTRIBUTE						= "table_name";
		public const string XML_TABLE_PRIMARY_KEY_CONSTRAINT_NAME_ATTRIBUTE	= "primary_key_constraint_name";
		public const string XML_TABLE_PRIMARY_KEY_CLUSTERED_ATTRIBUTE		= "clustered";
		public const string XML_TABLE_ADD_TBGUID_COLUMN_ATTRIBUTE			= "add_guid_column";
		public const string XML_TABLE_HKL_NAME_ATTRIBUTE					= "hot_key_link_name";
		public const string XML_TABLE_HKL_CLASS_NAME_ATTRIBUTE				= "hot_key_link_class_name";
		public const string XML_COLUMN_DATATYPE_ATTRIBUTE					= "data_type";
		public const string XML_COLUMN_DATA_ENUM_ATTRIBUTE					= "enum";
		public const string XML_COLUMN_UPPER_CASE_ATTRIBUTE					= "uppercase";
		public const string XML_COLUMN_LENGTH_ATTRIBUTE						= "data_length";
		public const string XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE				= "default_value";
		public const string XML_COLUMN_DEFAULT_EXPRESSION_VALUE_ATTRIBUTE	= "default_expression";
		public const string XML_COLUMN_PRIMARY_SEG_ATTRIBUTE				= "primary_key_segment";
		public const string XML_COLUMN_NULLABLE_VALUE_ATTRIBUTE				= "nullable";
		public const string XML_COLUMN_COLLATE_SENSITIVE_ATTRIBUTE			= "collate_sensitive";
		public const string XML_COLUMN_AUTO_INCREMENT_ATTRIBUTE				= "auto_increment";
		public const string XML_COLUMN_AUTO_INCREMENT_SEED_ATTRIBUTE		= "seed";
		public const string XML_COLUMN_AUTO_INCREMENT_INCREMENT_ATTRIBUTE	= "increment";
		public const string XML_COLUMN_DEFAULT_CONSTRAINT_NAME_ATTRIBUTE	= "default_constraint_name";
		public const string XML_COLUMN_HKL_CODE_ATTRIBUTE					= "hot_key_link_code";
		public const string XML_COLUMN_HKL_DESCR_ATTRIBUTE					= "hot_key_link_description";
		public const string XML_DBT_TYPE_ATTRIBUTE							= "type";
		public const string XML_DBT_SLAVEBUFFERED_CREATE_ROW_FORM_ATTRIBUTE	= "create_row_form";
		public const string XML_DBT_COLUMN_VISIBLE_ATTRIBUTE				= "visible";
		public const string XML_DBT_COLUMN_FINDABLE_ATTRIBUTE				= "findable";
		public const string XML_DBT_COLUMN_RELATEDCOL_ATTRIBUTE				= "related_column";
		public const string XML_DBT_COLUMN_HKL_CLASS_ATTRIBUTE				= "hot_key_link_class";
		public const string XML_DBT_COLUMN_HKL_SHOW_DESCRIPTION_ATTRIBUTE	= "hot_key_link_show_description";
		public const string XML_REL_DBTMASTER_NAME_ATTRIBUTE				= "related_master";
		public const string XML_ENUM_VALUE_ATTRIBUTE						= "value";
		public const string XML_ENUM_DEFAULT_ITEM_ATTRIBUTE					= "default_item";
		public const string XML_RESERVED_IDS_RANGE_ATTRIBUTE				= "ReservedIdsRange";
		public const string XML_DOC_DEFAULT_TYPE_ATTRIBUTE					= "default_type";
		public const string XML_DOC_HOTLINK_CODE_ATTRIBUTE					= "code_column";
		public const string XML_DOC_HOTLINK_DESCR_ATTRIBUTE					= "description_column";
		public const string XML_SERVER_DOC_FAMILY_ATTRIBUTE					= "attach_to_family";
		public const string XML_SERVER_DOC_EXCLUDE_BATCH_ATTRIBUTE			= "exclude_batch_documents";
		public const string XML_SERVER_DOC_NO_UNATTENDED_ATTRIBUTE			= "no_unattended_mode";
		public const string XML_CLIENT_DOC_SLAVEFORMVIEW_ATTRIBUTE			= "slave_form_view";
		public const string XML_CLIENT_DOC_ADDTABDIALOGS_ATTRIBUTE			= "add_tab_dialogs";
		public const string XML_ONLY_FOR_CLIENT_DOCUMENT_ATTRIBUTE			= "only_for_client_document";
		public const string XML_MASTER_TABLE_HEADER_ATTRIBUTE				= "master_table_header";
		public const string XML_REFERENCED_TABLE_HEADER_ATTRIBUTE			= "referenced_table_header";
		public const string XML_INDEX_NAME_ATTRIBUTE						= "index_name";
		public const string XML_INDEX_PRIMARY_ATTRIBUTE						= "primary";
		public const string XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE			= "event_type";
		public const string XML_COL_HISTORY_EVENT_ORDER_ATTRIBUTE			= "order";
		public const string XML_COL_HISTORY_EVENT_PREVIOUS_ORDER_ATTRIBUTE	= "previous_order";
		public const string XML_REFERENCED_HOTLINK_NAMESPACE_ATTRIBUTE		= "hotlink_namespace";
		public const string XML_REFERENCED_HOTLINK_INCLUDE_ATTRIBUTE		= "include_file";
		public const string XML_REFERENCED_TABLE_NAMESPACE_ATTRIBUTE		= "referenced_table_namespace";
		public const string XML_FOREIGN_KEY_CONSTRAINT_NAME_ATTRIBUTE		= "constraint_name";
		public const string XML_ON_DELETE_CASCADE_ATTRIBUTE					= "on_delete_cascade";
		public const string XML_ON_UPDATE_CASCADE_ATTRIBUTE					= "on_update_cascade";
		public const string XML_FOREIGN_KEY_SEG_COLUMN_ATTRIBUTE			= "column";
		public const string XML_FOREIGN_KEY_SEG_REFERENCED_COLUMN_ATTRIBUTE	= "referenced_column";
		public const string XML_TABBED_PANE_DBT_NAME_ATTRIBUTE				= "dbt_name";
		public const string XML_INDEX_UNIQUE_ATTRIBUTE						= "unique";
		public const string XML_INDEX_NON_CLUSTERED_ATTRIBUTE				= "non_clustered";
		public const string XML_LOCALIZE_ATTRIBUTE							= "localize";
		public const string XML_TB_NAMESPACE_ATTRIBUTE						= "tb_namespace"; // namespace oggetto
		public const string XML_LIBRARY_NAMESPACE_ATTRIBUTE					= "library_namespace"; // namespace library
		public const string XML_BASETYPE_ATTRIBUTE							= "basetype"; // nr. dell'enum (sintassi C++)
        public const string XML_CREATE_STEP_ATTRIBUTE                       = "createstep";
		// gestione nodi e attributi per gestione TableUpdate
		public const string XML_TAG_TABLES_UPDATE							= "TablesUpdate";
		public const string XML_TAG_TABLE_UPDATE							= "TableUpdate";
		public const string XML_TAG_WHERE_UPDATE							= "Where";
		public const string XML_SET_COLUMN_NAME_ATTRIBUTE					= "set_column_name";
		public const string XML_SET_SQL_VALUE_ATTRIBUTE						= "sql_set_value";
        public const string XML_SET_ORACLE_VALUE_ATTRIBUTE                  = "oracle_set_value";
		public const string XML_WHERE_TABLE_NAME_ATTRIBUTE					= "where_table_name";
		public const string XML_WHERE_COLUMN_NAME_ATTRIBUTE					= "where_column_name";
        public const string XML_WHERE_ORACLE_VALUE_ATTRIBUTE                = "oracle_where_value";
        public const string XML_WHERE_SQL_VALUE_ATTRIBUTE                   = "sql_where_value";

		// gestione nodi e attributi per gestione TBAfterScript\view\procedure
		public const string XML_TAG_TBAFTERSCRIPTS		= "TBAfterScripts";
		public const string XML_TAG_TBAFTERSCRIPT		= "TBAfterScript";
		public const string XML_STEP_ATTRIBUTE			= "step";
		public const string XML_TAG_SQLSCRIPT			= "SQLScript";
		public const string XML_TAG_ORACLESCRIPT		= "OracleScript";

		// gestione nodi e attributi per gestione View
		public const string XML_TAG_VIEWS	= "Views";
		public const string XML_TAG_VIEW	= "View";

		// gestione nodi e attributi per gestione Procedures
		public const string XML_TAG_PROCEDURES			= "Procedures";
		public const string XML_TAG_PROCEDURE			= "Procedure";
		public const string XML_TAG_PARAMETERS			= "Parameters";
		public const string XML_TAG_PARAMETER			= "Parameter";
		public const string XML_OUT_PARAMETER_ATTRIBUTE = "out_param";

		public static string TrueAttributeValue = true.ToString().ToLower();

		#region public events
		public event TBWizardEventHandler ProjectParsingStarted = null;
		public event TBWizardEventHandler ProjectReferencesParsed = null;
		public event TBWizardEventHandler ProjectParsingModule = null;
		public event TBWizardEventHandler ProjectParsingLibrary = null;
		public event TBWizardEventHandler ProjectParsingEnded = null;
		#endregion

		#region TBWizardProjectParser private data members

		protected string		projectFileName = String.Empty;
        protected XmlDocument   projectDocument = null;
		private WizardApplicationInfo			applicationInfo = null;
		private WizardApplicationInfoCollection referencedApplicationsInfo = null;
		private bool							isModified = false;

		private const string ProjectXmlVersion	= "1.0";
		private const string ProjectXmlEncoding	= "UTF-8";

		#endregion

		#region TBWizardProjectParser constructors

		//---------------------------------------------------------------------------
		public TBWizardProjectParser(string aProjectFileName)
		{
			ParseFile(aProjectFileName);
		}

		//---------------------------------------------------------------------------
		public TBWizardProjectParser() : this(null)
		{
		}

		#endregion

		#region TBWizardProjectParser private methods

		//---------------------------------------------------------------------------
		private void Clear()
		{
			projectDocument = null;
			projectFileName = String.Empty;
			if (applicationInfo != null)
			{
				applicationInfo.Dispose();
				applicationInfo = null;
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ParseFile(string aProjectFileName)
		{
			Clear();

			if (string.IsNullOrEmpty(aProjectFileName) || !File.Exists(aProjectFileName))
				return false;

			try
			{
				XmlDocument loadedDocument = new XmlDocument();

				loadedDocument.Load(aProjectFileName);

				if (loadedDocument.DocumentElement == null || !loadedDocument.DocumentElement.HasChildNodes)
					return true; // Progetto vuoto

                if (String.Compare(loadedDocument.DocumentElement.Name, RootTag) != 0)
					throw new TBWizardException(String.Format(TBWizardProjectsStrings.InvalidProjectFileErrMsg, aProjectFileName));

                if (!ParseApplicationInfo(loadedDocument.DocumentElement))
					throw new TBWizardException(String.Format(TBWizardProjectsStrings.InvalidApplicationInfoErrMsg, aProjectFileName));
				
				projectDocument = loadedDocument;
				projectFileName = aProjectFileName;
				return true;
			}
			catch(XmlException exception)
			{
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.LoadFromFileXmlExceptionErrMsg, aProjectFileName), exception);
			}
			catch(TBWizardException exception)
			{
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.LoadFromFileXmlExceptionErrMsg, aProjectFileName), exception);
			}
		}

		//---------------------------------------------------------------------------
		protected virtual bool ParseApplicationInfo(System.Xml.XmlElement aProjectRoot)
		{
			if (aProjectRoot == null || String.Compare(aProjectRoot.Name, RootTag) != 0)
				return false;

			try
			{
				XmlNode applicationInfoNode = aProjectRoot.SelectSingleNode("child::" + XML_TAG_APPLICATION);
				if (applicationInfoNode == null || !(applicationInfoNode is XmlElement))
					return false;

				string applicationName = ((XmlElement)applicationInfoNode).GetAttribute(XML_NAME_ATTRIBUTE);
				if (string.IsNullOrEmpty(applicationName))
					throw new TBWizardException(TBWizardProjectsStrings.EmptyApplicationNameErrMsg);

				bool isReadOnly = false;
				if (((XmlElement)applicationInfoNode).HasAttribute(XML_READONLY_ATTRIBUTE))
				{
					string readOnlyValue = ((XmlElement)applicationInfoNode).GetAttribute(XML_READONLY_ATTRIBUTE);
					if (!string.IsNullOrEmpty(readOnlyValue))
						isReadOnly = Boolean.Parse(readOnlyValue);
				}
	
				applicationInfo = new WizardApplicationInfo(applicationName, isReadOnly);

				if (ProjectParsingStarted != null)
					ProjectParsingStarted(this, new TBWizardEventArgs(applicationInfo, TBWizardEventArgs.ActionTaken.Parsing));

				XmlNode appTypeNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_APP_TYPE);
				if (appTypeNode != null && (appTypeNode is XmlElement))
				{
					string appType = appTypeNode.InnerText;
					if (!string.IsNullOrEmpty(appType))
					{
						try
						{
							applicationInfo.Type = (ApplicationType)Enum.Parse(typeof(ApplicationType),appType, true);
						}
						catch(ArgumentNullException exception)
						{
							Debug.Fail("ArgumentNullException raised during parsing of ApplicationType in TBWizardProjectParser.ParseApplicationInfo." + exception.Message);
							applicationInfo.Type = ApplicationType.TaskBuilderApplication;
							appTypeNode.InnerText = applicationInfo.Type.ToString();
							isModified = true;
						}
						catch(ArgumentException exception)
						{
							Debug.Fail("ArgumentException raised during parsing of ApplicationType in TBWizardProjectParser.ParseApplicationInfo." + exception.Message);
							applicationInfo.Type = ApplicationType.TaskBuilderApplication;
							appTypeNode.InnerText = applicationInfo.Type.ToString();
							isModified = true;
						}
					}
				}

				XmlNode titleNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_APP_TITLE);
				if (titleNode != null && (titleNode is XmlElement))
				{
					string title = titleNode.InnerText;
					if (!string.IsNullOrEmpty(title))
						applicationInfo.Title = title;
				}

				XmlNode versionNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_APP_VERSION);
				if (versionNode != null && (versionNode is XmlElement))
				{
					string version = versionNode.InnerText;
					if (!string.IsNullOrEmpty(version))
						applicationInfo.Version = version;
				}

				XmlNode appProducerNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_APP_PRODUCER);
				if (appProducerNode != null && (appProducerNode is XmlElement))
				{
					string producer = appProducerNode.InnerText;
					if (!string.IsNullOrEmpty(producer))
						applicationInfo.Producer = producer;
				}

				XmlNode appDbSignatureNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_APP_DBSIGNATURE);
				if (appDbSignatureNode != null && (appDbSignatureNode is XmlElement))
				{
					string dbSignature = appDbSignatureNode.InnerText;
					if (!string.IsNullOrEmpty(dbSignature))
						applicationInfo.DbSignature = dbSignature;
				}

				XmlNode shortNameNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_APP_SHORTNAME);
				if (shortNameNode != null && (shortNameNode is XmlElement))
				{
					string shortName = shortNameNode.InnerText;
					if (string.IsNullOrEmpty(shortName))
						throw new TBWizardException(TBWizardProjectsStrings.EmptyApplicationShortNameErrMsg);

					applicationInfo.ShortName = shortName;
				}
				else
					applicationInfo.ShortName = applicationName.Substring(0, Math.Min(WizardApplicationInfo.ShortNameLength, applicationName.Length));

				XmlNode editionNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_APP_EDITION);
				if (editionNode != null && (editionNode is XmlElement))
				{
					string editionText = editionNode.InnerText;
					if (!string.IsNullOrEmpty(editionText))
					{
						try
						{
							applicationInfo.Edition = (WizardApplicationInfo.SolutionEdition)Enum.Parse(typeof(WizardApplicationInfo.SolutionEdition), editionText);
						}
						catch(ArgumentNullException exception)
						{
							Debug.Fail("ArgumentNullException raised during parsing of application edition in TBWizardProjectParser.ParseApplicationInfo." + exception.Message);
						}
						catch(ArgumentException exception)
						{
							Debug.Fail("ArgumentException raised during parsing of application edition in TBWizardProjectParser.ParseApplicationInfo." + exception.Message);
						}
					}
				}
				if (applicationInfo.Edition == WizardApplicationInfo.SolutionEdition.Undefined)
					applicationInfo.Edition = WizardApplicationInfo.SolutionEdition.Standard;

				XmlNode solutionTypeNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_APP_SOLUTION_TYPE);
				if (solutionTypeNode != null && (solutionTypeNode is XmlElement))
				{
					string solutionTypeText = solutionTypeNode.InnerText;
					if (!string.IsNullOrEmpty(solutionTypeText))
					{
						try
						{
							applicationInfo.SolutionType = (WizardApplicationInfo.AppSolutionType)Enum.Parse(typeof(WizardApplicationInfo.AppSolutionType), solutionTypeText);
						}
						catch(ArgumentNullException exception)
						{
							Debug.Fail("ArgumentNullException raised during parsing of solution type in TBWizardProjectParser.ParseApplicationInfo." + exception.Message);
						}
						catch(ArgumentException exception)
						{
							Debug.Fail("ArgumentException raised during parsing of solution type in TBWizardProjectParser.ParseApplicationInfo." + exception.Message);
						}
					}
				}
				
				if (applicationInfo.SolutionType == WizardApplicationInfo.AppSolutionType.Undefined)
					applicationInfo.SolutionType = WizardApplicationInfo.AppSolutionType.AddOn;

				XmlNode cultureNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_APP_CULTURE);
				if (cultureNode != null && (cultureNode is XmlElement))
				{
					string cultureName = cultureNode.InnerText;
					if (!string.IsNullOrEmpty(cultureName))
						applicationInfo.CultureName = cultureName;
				}

				XmlNode fontNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_APP_FONT);
				if (fontNode != null && (fontNode is XmlElement))
				{
					string fontFamilyName = fontNode.InnerText;
					if (!string.IsNullOrEmpty(fontFamilyName))
					{
						try
						{
							float fontSize = 0;
							string sizeText = ((XmlElement)fontNode).GetAttribute(XML_APP_FONT_SIZEINPOINTS_ATTRIBUTE);
							if (!string.IsNullOrEmpty(sizeText))
								fontSize = Single.Parse(sizeText);
							
							bool isBold = false;
							if (((XmlElement)fontNode).HasAttribute(XML_APP_FONT_BOLD_ATTRIBUTE))
								isBold = Convert.ToBoolean(((XmlElement)fontNode).GetAttribute(XML_APP_FONT_BOLD_ATTRIBUTE));

							bool isItalic = false;
							if (((XmlElement)fontNode).HasAttribute(XML_APP_FONT_ITALIC_ATTRIBUTE))
								isItalic = Convert.ToBoolean(((XmlElement)fontNode).GetAttribute(XML_APP_FONT_ITALIC_ATTRIBUTE));
				
							applicationInfo.SetFont(fontFamilyName, fontSize, isBold, isItalic);
						}
						catch(FormatException)
						{
						}
						catch(OverflowException)
						{
						}
					}
				}

				XmlNode guidNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_APP_GUID);
				if (guidNode != null && (guidNode is XmlElement))
				{
					string guidText = guidNode.InnerText;
					if (!string.IsNullOrEmpty(guidText))
					{
						try
						{
							applicationInfo.Guid = new Guid(guidText);
						}
						catch(ArgumentNullException)
						{
						}
						catch(FormatException)
						{
						}
					}
				}
				if (applicationInfo.Guid.Equals(System.Guid.Empty))
					applicationInfo.Guid = System.Guid.NewGuid();

				ParseApplicationReferences((XmlElement)applicationInfoNode);

				// Una volta che sono state caricate tutte le informazioni globali, per
				// prima cosa, devo caricare tutti i dati, sparpagliati fra i vari moduli 
				// dell'applicazione, relativi alla definizione di tipi di dato enumerativi.
				// Lo devo ovviamente fare prima di caricare le informazioni riguardanti le
				// librerie e, quindi, le tabelle in esse contenute, visto che tali tabelle
				// possono utilizzarli.
				// Infatti, gli enumerativi vengono definiti a livello di modulo (ciascun
				// modulo dichiara i propri tipi enumerativi), ma essi hanno una valenza che
				// copre l'intera applicazione e possono venire utilizzati anche da librerie
				// che appartengono ad altri moduli.
				ParseAllModulesBasicInfo((XmlElement)applicationInfoNode);

				ParseLibrariesInfo((XmlElement)applicationInfoNode);

				// Quando ho caricato TUTTI i moduli posso andare a leggere le eventuali
				// dipendenze fra le librerie: prima (cioè in fase di caricamento dei singoli
				// moduli) non posso anora farlo, in quanto non tutte le informazioni di
				// tutte le librerie dell'applicazione sono ancora note
				ParseLibrariesDependencies((XmlElement)applicationInfoNode);

				// Dopo aver caricato tutte le tabelle e le dipendenze tra le varie librerie,
				// posso caricare eventuali colonne aggiuntive 
				ParseExtraAddedColumnsInfo((XmlElement)applicationInfoNode);
				
				// Nei DBT si possono gestire anche delle colonne aggiuntive (aggiunte dalla
				// libreria che contiene il DBT o anche da librerie da cui essa dipende
				ParseDBTsInfo((XmlElement)applicationInfoNode);
			
				ParseTablesForeignKeysInfo((XmlElement)applicationInfoNode);

				ParseDBTSlavesRelatedMasterInfo((XmlElement)applicationInfoNode);
				
				// I documenti vanno caricati dopo aver caricato TUTTI i DBT, in quanto un 
				// documento può fare riferimento a dei DBT che non stanno nella libreria che
				// lo contiene, bensì in librerie dalle quali essa dipende
				ParseDocumentsInfo((XmlElement)applicationInfoNode);
				ParseClientDocumentsInfo((XmlElement)applicationInfoNode);
				ParseDBTsHotLinks((XmlElement)applicationInfoNode);
				ParseTabbedPanesHotLinks((XmlElement)applicationInfoNode);

				if (ProjectParsingEnded != null)
					ProjectParsingEnded(this, new TBWizardEventArgs(applicationInfo, TBWizardEventArgs.ActionTaken.Parsing));

				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private void ParseUserInterface(XmlElement aApplicationNode)
		{ 
		}

		//---------------------------------------------------------------------------
		private void ParseApplicationReferences(System.Xml.XmlElement aApplicationNode)
		{
			if (applicationInfo == null || aApplicationNode == null || String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0)
				return;

			try
			{
				XmlNode referencesNode = aApplicationNode.SelectSingleNode("child::" + XML_TAG_REFERENCES);
				if (referencesNode == null || !(referencesNode is XmlElement) || !referencesNode.HasChildNodes)
					return;

				XmlNodeList referencesList = referencesNode.SelectNodes("child::" + XML_TAG_REFERENCED_APP);
				if (referencesList == null || referencesList.Count == 0)
					return;

                foreach (XmlNode referencedAppNode in referencesList)
                {
                    if (referencedAppNode == null || !(referencedAppNode is XmlElement))
                        continue;

                    string referencedApplicationName = referencedAppNode.InnerText;
                    if (BasePathFinder.BasePathFinderInstance != null && !string.IsNullOrEmpty(referencedApplicationName))
                    {
                        IBaseApplicationInfo refAppInfo = BasePathFinder.BasePathFinderInstance.GetApplicationInfoByName(referencedApplicationName);
                        if (refAppInfo != null)
                            referencedApplicationName = refAppInfo.Name;
                    }
                    applicationInfo.AddReference(referencedApplicationName);
                }
	
				if (ProjectReferencesParsed != null)
					ProjectReferencesParsed(this, new TBWizardEventArgs(applicationInfo, TBWizardEventArgs.ActionTaken.Parsing));
		
				CompleteReferencedHotLinksInfo(aApplicationNode);
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private void CompleteReferencedHotLinksInfo(System.Xml.XmlElement aApplicationNode)
		{
			if 
				(
				applicationInfo == null ||
				aApplicationNode == null ||
				String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0 ||
				referencedApplicationsInfo == null ||
				referencedApplicationsInfo.Count == 0
				)
				return;
			
			try
			{
				XmlNode referencesInfoNode = aApplicationNode.SelectSingleNode("child::" + XML_TAG_REFERENCES);
				if (referencesInfoNode == null || !(referencesInfoNode is XmlElement))
					return;

				XmlNodeList hotLinksList = referencesInfoNode.SelectNodes("child::" + XML_TAG_REFERENCED_HOTLINK);
				if (hotLinksList == null || hotLinksList.Count == 0)
					return;

				foreach(XmlNode hotLinkInfoNode in hotLinksList)
				{
					if 
						(
						hotLinkInfoNode == null || 
						!(hotLinkInfoNode is XmlElement) || 
						!((XmlElement)hotLinkInfoNode).HasAttribute(XML_CLASS_NAME_ATTRIBUTE) ||
						!((XmlElement)hotLinkInfoNode).HasAttribute(XML_REFERENCED_HOTLINK_NAMESPACE_ATTRIBUTE)
						)
						continue;

					string hotLinkClassName = ((XmlElement)hotLinkInfoNode).GetAttribute(XML_CLASS_NAME_ATTRIBUTE);
					if (hotLinkClassName == null)
						continue;
					hotLinkClassName = hotLinkClassName.Trim();
					if (hotLinkClassName.Length == 0)
						continue;

					string namespaceAttribute = ((XmlElement)hotLinkInfoNode).GetAttribute(XML_REFERENCED_HOTLINK_NAMESPACE_ATTRIBUTE);
					if (namespaceAttribute == null)
						continue;
					namespaceAttribute = namespaceAttribute.Trim();
					if (namespaceAttribute.Length == 0)
						continue;

					NameSpace hotlinkNamespace = new NameSpace(namespaceAttribute, NameSpaceObjectType.Hotlink);
		
					foreach(WizardApplicationInfo aLoadedApplication in referencedApplicationsInfo)
					{
						if (String.Compare(aLoadedApplication.Name, hotlinkNamespace.Application) == 0)
						{
							if (aLoadedApplication.ModulesCount > 0)
							{
								foreach(WizardModuleInfo aLoadedModule in aLoadedApplication.ModulesInfo)
								{
									if (String.Compare(aLoadedModule.Name, hotlinkNamespace.Module) == 0)
									{
										if (aLoadedModule.LibrariesCount > 0)
										{
											foreach(WizardLibraryInfo aLoadedLibrary in aLoadedModule.LibrariesInfo)
											{
												if (String.Compare(aLoadedLibrary.SourceFolder, hotlinkNamespace.Library) == 0)
												{
													WizardHotKeyLinkInfo hotlinkInfo = aLoadedLibrary.GetHotKeyLinkFromClassName(hotLinkClassName);
													if (hotlinkInfo != null && String.Compare(hotlinkInfo.Name, hotlinkNamespace.Hotlink) == 0)
													{
														if (((XmlElement)hotLinkInfoNode).HasAttribute(XML_REFERENCED_HOTLINK_INCLUDE_ATTRIBUTE))
														{
															string relativeFileName = ((XmlElement)hotLinkInfoNode).GetAttribute(XML_REFERENCED_HOTLINK_INCLUDE_ATTRIBUTE);
															if (relativeFileName != null && relativeFileName.Trim().Length > 0)
																hotlinkInfo.ExternalIncludeFile = Generics.BuildFullPath(WizardCodeGenerator.GetStandardTaskBuilderApplicationContainerPath(), relativeFileName);
														}
														break;
													}
												}
											}
										}
										break;
									}
								}
							}
							break;
						}
					}
				}
			}
			catch(Exception exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ParseAllModulesBasicInfo(System.Xml.XmlElement aApplicationNode)
		{
			if (applicationInfo == null || aApplicationNode == null || String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0)
				return false;
			
			try
			{
				XmlNode modulesInfoNode = aApplicationNode.SelectSingleNode("child::" + XML_TAG_MODULES);
				if (modulesInfoNode == null || !(modulesInfoNode is XmlElement) || !modulesInfoNode.HasChildNodes)
					return false;

				XmlNodeList modulesList = modulesInfoNode.SelectNodes("child::" + XML_TAG_MODULE);
				if (modulesList == null || modulesList.Count == 0)
					return false;

				foreach(XmlNode moduleInfoNode in modulesList)
				{
					if (moduleInfoNode == null || !(moduleInfoNode is XmlElement))
						continue;

					if (!ParseModuleEnumsInfoNode((XmlElement)moduleInfoNode))
						return false;
				}

				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ParseModuleEnumsInfoNode(System.Xml.XmlElement aModuleInfoNode)
		{
			if (aModuleInfoNode == null || String.Compare(aModuleInfoNode.Name, XML_TAG_MODULE) != 0)
				return false;

			string moduleName = aModuleInfoNode.GetAttribute(XML_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(moduleName))
				return false;
	
			bool isReadOnly = false;
			if (aModuleInfoNode.HasAttribute(XML_READONLY_ATTRIBUTE))
			{
				string readOnlyValue = aModuleInfoNode.GetAttribute(XML_READONLY_ATTRIBUTE);
				if (!string.IsNullOrEmpty(readOnlyValue))
					isReadOnly = Boolean.Parse(readOnlyValue);
			}

			WizardModuleInfo newModuleInfo = new WizardModuleInfo(moduleName, isReadOnly);

			XmlNode titleNode = aModuleInfoNode.SelectSingleNode("child::" + XML_TAG_MOD_TITLE);
			if (titleNode != null && (titleNode is XmlElement))
			{
				string title = titleNode.InnerText;
				if (!string.IsNullOrEmpty(title))
					newModuleInfo.Title = title;
			}
            
            XmlNode moduleDbSignatureNode = aModuleInfoNode.SelectSingleNode("child::" + XML_TAG_MOD_DBSIGNATURE);
            if (moduleDbSignatureNode != null && (moduleDbSignatureNode is XmlElement))
            {
                string dbSignature = moduleDbSignatureNode.InnerText;
				if (!string.IsNullOrEmpty(dbSignature))
                    newModuleInfo.DbSignature = dbSignature;
            }


			XmlNode dbReleaseNode = aModuleInfoNode.SelectSingleNode("child::" + XML_TAG_MOD_DBRELEASE);
			if (dbReleaseNode != null && (dbReleaseNode is XmlElement))
			{
				string dbRelease = dbReleaseNode.InnerText;
				if (!string.IsNullOrEmpty(dbRelease))
				{
					try
					{
						newModuleInfo.DbReleaseNumber = Convert.ToUInt32(dbRelease);
					}
					catch(FormatException)
					{
					}
					catch(OverflowException)
					{
					}
				}
			}

            XmlNode guidNode = aModuleInfoNode.SelectSingleNode("child::" + XML_TAG_MOD_GUID);
            if (guidNode != null && (guidNode is XmlElement))
            {
                string guidText = guidNode.InnerText;
				if (!string.IsNullOrEmpty(guidText))
                {
                    try
                    {
                        newModuleInfo.Guid = new Guid(guidText);
                    }
                    catch (ArgumentNullException)
                    {
                    }
                    catch (FormatException)
                    {
                    }
                }
            }

			ParseEnumsInfo(newModuleInfo, (XmlElement)aModuleInfoNode);
			
			return (applicationInfo.AddModuleInfo(newModuleInfo) != -1);
		}

		//---------------------------------------------------------------------------
		private bool ParseEnumsInfo(WizardModuleInfo aModuleInfo, System.Xml.XmlElement aModuleNode)
		{
			if (aModuleInfo == null || aModuleNode == null || String.Compare(aModuleNode.Name, XML_TAG_MODULE) != 0)
				return false;
			
			try
			{
				XmlNode enumsInfoNode = aModuleNode.SelectSingleNode("child::" + XML_TAG_ENUMS);
				if (enumsInfoNode == null || !(enumsInfoNode is XmlElement) || !enumsInfoNode.HasChildNodes)
					return false;

				XmlNodeList enumsList = enumsInfoNode.SelectNodes("child::" + XML_TAG_ENUM);
				if (enumsList == null || enumsList.Count == 0)
					return false;

				foreach(XmlNode enumInfoNode in enumsList)
				{
					if (enumInfoNode == null || !(enumInfoNode is XmlElement))
						continue;

					if (!ParseEnumInfoNode(aModuleInfo, (XmlElement)enumInfoNode))
						return false;
				}

				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ParseEnumInfoNode(WizardModuleInfo aModuleInfo, System.Xml.XmlElement aEnumInfoNode)
		{
			if (aModuleInfo == null || aEnumInfoNode == null || String.Compare(aEnumInfoNode.Name, XML_TAG_ENUM) != 0)
				return false;

			string enumName = aEnumInfoNode.GetAttribute(XML_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(enumName))
				return false;
			
			string enumValueText = aEnumInfoNode.GetAttribute(XML_ENUM_VALUE_ATTRIBUTE);
			if (string.IsNullOrEmpty(enumValueText))
				return false;

			ushort enumValue = 0;
			try
			{
				enumValue = Convert.ToUInt16(enumValueText);
			}
			catch(FormatException)
			{
				return false;
			}
			catch(OverflowException)
			{
				return false;
			}
			
			bool isReadOnly = false;
			if (aEnumInfoNode.HasAttribute(XML_READONLY_ATTRIBUTE))
			{
				string readOnlyValue = aEnumInfoNode.GetAttribute(XML_READONLY_ATTRIBUTE);
				if (!string.IsNullOrEmpty(readOnlyValue))
					isReadOnly = Boolean.Parse(readOnlyValue);
			}

			WizardEnumInfo newEnumInfo = new WizardEnumInfo(enumName, enumValue, isReadOnly);

			ParseEnumItemsInfo(newEnumInfo, aEnumInfoNode);

			return (aModuleInfo.AddEnumInfo(newEnumInfo) != -1);
		}
		
		//---------------------------------------------------------------------------
		private bool ParseEnumItemsInfo(WizardEnumInfo aEnumInfo, System.Xml.XmlElement aEnumInfoNode)
		{
			if (aEnumInfo == null || aEnumInfoNode == null || String.Compare(aEnumInfoNode.Name, XML_TAG_ENUM) != 0)
				return false;
			
			try
			{
				XmlNode itemsInfoNode = aEnumInfoNode.SelectSingleNode("child::" + XML_TAG_ENUM_ITEMS);
				if (itemsInfoNode == null || !(itemsInfoNode is XmlElement) || !itemsInfoNode.HasChildNodes)
					return false;

				XmlNodeList itemsList = itemsInfoNode.SelectNodes("child::" + XML_TAG_ENUM_ITEM);
				if (itemsList == null || itemsList.Count == 0)
					return false;

				foreach(XmlNode itemInfoNode in itemsList)
				{
					if (itemInfoNode == null || !(itemInfoNode is XmlElement))
						continue;

					if (!ParseEnumItemInfoNode(aEnumInfo, (XmlElement)itemInfoNode))
						return false;
				}

				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ParseEnumItemInfoNode(WizardEnumInfo aEnumInfo, System.Xml.XmlElement aItemInfoNode)
		{
			if (aEnumInfo == null || aItemInfoNode == null || String.Compare(aItemInfoNode.Name, XML_TAG_ENUM_ITEM) != 0)
				return false;

			string itemName = aItemInfoNode.GetAttribute(XML_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(itemName))
				return false;
			
			ushort itemValue = 0;
			string itemValueText = aItemInfoNode.GetAttribute(XML_ENUM_VALUE_ATTRIBUTE);
			if (!string.IsNullOrEmpty(itemValueText))
			{
				try
				{
					itemValue = Convert.ToUInt16(itemValueText);
				}
				catch(FormatException)
				{
					return false;
				}
				catch(OverflowException)
				{
					return false;
				}
			}
			else
				itemValue = aEnumInfo.GetNextValidItemValue();
					
			bool isReadOnly = false;
			if (aItemInfoNode.HasAttribute(XML_READONLY_ATTRIBUTE))
			{
				string readOnlyValue = aItemInfoNode.GetAttribute(XML_READONLY_ATTRIBUTE);
				if (!string.IsNullOrEmpty(readOnlyValue))
					isReadOnly = Boolean.Parse(readOnlyValue);
			}

			WizardEnumItemInfo newItemInfo = new WizardEnumItemInfo(itemName, itemValue, isReadOnly);

			string isDefaultItemText = aItemInfoNode.GetAttribute(XML_ENUM_DEFAULT_ITEM_ATTRIBUTE);
			if (!string.IsNullOrEmpty(isDefaultItemText))
			{
				try
				{
					newItemInfo.IsDefaultItem = Convert.ToBoolean(isDefaultItemText);
				}
				catch(FormatException)
				{
				}
				catch(OverflowException)
				{
				}
			}
			
			return (aEnumInfo.AddItemInfo(newItemInfo) != -1);
		}
		
		//---------------------------------------------------------------------------
		private bool ParseLibrariesInfo(System.Xml.XmlElement aApplicationNode)
		{
			if (applicationInfo == null || applicationInfo.ModulesCount == 0 || aApplicationNode == null ||
				String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0)
				return false;
			
			try
			{
				XmlNode modulesInfoNode = aApplicationNode.SelectSingleNode("child::" + XML_TAG_MODULES);
				if (modulesInfoNode == null || !(modulesInfoNode is XmlElement) || !modulesInfoNode.HasChildNodes)
					return false;

				// In applicationInfo sono già state inserite le informazioni di base
				// relative a tutti i moduli che compongono l'applicazione
				foreach(WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
				{
					XmlNode moduleInfoNode = modulesInfoNode.SelectSingleNode("child::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE +"='" + aModuleInfo.Name + "']");
				
					if (moduleInfoNode == null || !(moduleInfoNode is XmlElement))
						continue;

					if (ProjectParsingModule != null)
						ProjectParsingModule(this, new TBWizardEventArgs(aModuleInfo, TBWizardEventArgs.ActionTaken.Parsing));

					if (!ParseModuleLibrariesInfo(aModuleInfo, (XmlElement)moduleInfoNode))
						return false;
				}

				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ParseModuleLibrariesInfo(WizardModuleInfo aModuleInfo, System.Xml.XmlElement aModuleNode)
		{
			if (aModuleInfo == null || aModuleNode == null || String.Compare(aModuleNode.Name, XML_TAG_MODULE) != 0)
				return false;
			
			try
			{
				XmlNode librariesInfoNode = aModuleNode.SelectSingleNode("child::" + XML_TAG_LIBRARIES);
				if (librariesInfoNode == null || !(librariesInfoNode is XmlElement) || !librariesInfoNode.HasChildNodes)
					return false;

				XmlNodeList librariesList = librariesInfoNode.SelectNodes("child::" + XML_TAG_LIBRARY);
				if (librariesList == null || librariesList.Count == 0)
					return false;

				foreach(XmlNode libraryInfoNode in librariesList)
				{
					if (libraryInfoNode == null || !(libraryInfoNode is XmlElement))
						continue;

					if (!ParseLibraryInfoNode(aModuleInfo, (XmlElement)libraryInfoNode))
						return false;
				}

				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ParseLibraryInfoNode(WizardModuleInfo aModuleInfo, System.Xml.XmlElement aLibraryInfoNode)
		{
			if (aModuleInfo == null || aLibraryInfoNode == null || String.Compare(aLibraryInfoNode.Name, XML_TAG_LIBRARY) != 0)
				return false;

			string libraryName = aLibraryInfoNode.GetAttribute(XML_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(libraryName))
				return false;
		
			bool isReadOnly = false;
			if (aLibraryInfoNode.HasAttribute(XML_READONLY_ATTRIBUTE))
			{
				string readOnlyValue = aLibraryInfoNode.GetAttribute(XML_READONLY_ATTRIBUTE);
				if (!string.IsNullOrEmpty(readOnlyValue))
				{
					try
					{
						isReadOnly = Boolean.Parse(readOnlyValue);
					}
					catch(ArgumentNullException)
					{
					}
					catch(FormatException)
					{
					}
				}
			}

			WizardLibraryInfo newLibraryInfo = new WizardLibraryInfo(libraryName, isReadOnly);

			if (ProjectParsingLibrary != null)
				ProjectParsingLibrary(this, new TBWizardEventArgs(newLibraryInfo, TBWizardEventArgs.ActionTaken.Parsing));

			if (aLibraryInfoNode.HasAttribute(XML_LIB_TRAP_DSNCHANGED_ATTRIBUTE))
			{
				string trapDSNChangedEventValue = aLibraryInfoNode.GetAttribute(XML_LIB_TRAP_DSNCHANGED_ATTRIBUTE);
				if (!string.IsNullOrEmpty(trapDSNChangedEventValue))
				{
					try
					{
						newLibraryInfo.TrapDSNChangedEvent = Boolean.Parse(trapDSNChangedEventValue);
					}
					catch(ArgumentNullException)
					{
					}
					catch(FormatException)
					{
					}
				}
			}

			if (aLibraryInfoNode.HasAttribute(XML_LIB_TRAP_APPDATECHANGED_ATTRIBUTE))
			{
				string trapApplicationDateChangedEventValue = aLibraryInfoNode.GetAttribute(XML_LIB_TRAP_APPDATECHANGED_ATTRIBUTE);
				if (!string.IsNullOrEmpty(trapApplicationDateChangedEventValue))
				{
					try
					{
						newLibraryInfo.TrapApplicationDateChangedEvent = Boolean.Parse(trapApplicationDateChangedEventValue);
					}
					catch(ArgumentNullException)
					{
					}
					catch(FormatException)
					{
					}
				}
			}

			XmlNode sourceFolderNode = aLibraryInfoNode.SelectSingleNode("child::" + XML_TAG_LIB_SOURCEFOLDER);
			if (sourceFolderNode != null && (sourceFolderNode is XmlElement))
			{
				string sourceFolder = sourceFolderNode.InnerText;
				if (!string.IsNullOrEmpty(sourceFolder))
					newLibraryInfo.SourceFolder = sourceFolder;
			}
			
			XmlNode menuTitleNode = aLibraryInfoNode.SelectSingleNode("child::" + XML_TAG_LIB_MENUTITLE);
			if (menuTitleNode != null && (menuTitleNode is XmlElement))
			{
				string menuTitle = menuTitleNode.InnerText;
				if (!string.IsNullOrEmpty(menuTitle))
					newLibraryInfo.MenuTitle = menuTitle;
			}
			
			XmlNode guidNode = aLibraryInfoNode.SelectSingleNode("child::" + XML_TAG_LIB_GUID);
			if (guidNode != null && (guidNode is XmlElement))
			{
				string guidText = guidNode.InnerText;
				if (!string.IsNullOrEmpty(guidText))
				{
					try
					{
						newLibraryInfo.Guid = new Guid(guidText);
					}
					catch(ArgumentNullException)
					{
					}
					catch(FormatException)
					{
					}
				}
			}

			XmlNode firstResourceIdNode = aLibraryInfoNode.SelectSingleNode("child::" + XML_TAG_FIRST_RESOURCE_ID);
			if (firstResourceIdNode != null && (firstResourceIdNode is XmlElement))
			{
				string firstResourceIdText = firstResourceIdNode.InnerText;
				if (!string.IsNullOrEmpty(firstResourceIdText))
				{
					try
					{
						newLibraryInfo.FirstResourceId = Convert.ToUInt16(firstResourceIdText);

						string reservedRangeText = ((XmlElement)firstResourceIdNode).GetAttribute(XML_RESERVED_IDS_RANGE_ATTRIBUTE);
						if (!string.IsNullOrEmpty(reservedRangeText))
							newLibraryInfo.ReservedResourceIdsRange = Convert.ToUInt16(reservedRangeText);
					}
					catch(FormatException)
					{
					}
					catch(OverflowException)
					{
					}
				}			
			}

			XmlNode firstControlIdNode = aLibraryInfoNode.SelectSingleNode("child::" + XML_TAG_FIRST_CONTROL_ID);
			if (firstControlIdNode != null && (firstControlIdNode is XmlElement))
			{
				string firstControlIdText = firstControlIdNode.InnerText;
				if (!string.IsNullOrEmpty(firstControlIdText))
				{
					try
					{
						newLibraryInfo.FirstControlId = Convert.ToUInt16(firstControlIdText);

						string reservedRangeText = ((XmlElement)firstControlIdNode).GetAttribute(XML_RESERVED_IDS_RANGE_ATTRIBUTE);
						if (!string.IsNullOrEmpty(reservedRangeText))
							newLibraryInfo.ReservedControlIdsRange = Convert.ToUInt16(reservedRangeText);
					}
					catch(FormatException)
					{
					}
					catch(OverflowException)
					{
					}
				}			
			}

			XmlNode firstCommandIdNode = aLibraryInfoNode.SelectSingleNode("child::" + XML_TAG_FIRST_COMMAND_ID);
			if (firstCommandIdNode != null && (firstCommandIdNode is XmlElement))
			{
				string firstCommandIdText = firstCommandIdNode.InnerText;
				if (!string.IsNullOrEmpty(firstCommandIdText))
				{
					try
					{
						newLibraryInfo.FirstCommandId = Convert.ToUInt16(firstCommandIdText);
						
						string reservedRangeText = ((XmlElement)firstCommandIdNode).GetAttribute(XML_RESERVED_IDS_RANGE_ATTRIBUTE);
						if (!string.IsNullOrEmpty(reservedRangeText))
							newLibraryInfo.ReservedCommandIdsRange = Convert.ToUInt16(reservedRangeText);
					}
					catch(FormatException)
					{
					}
					catch(OverflowException)
					{
					}
				}			
			}

			XmlNode firstSymedIdNode = aLibraryInfoNode.SelectSingleNode("child::" + XML_TAG_FIRST_SYMED_ID);
			if (firstSymedIdNode != null && (firstSymedIdNode is XmlElement))
			{
				string firstSymedIdText = firstSymedIdNode.InnerText;
				if (!string.IsNullOrEmpty(firstSymedIdText))
				{
					try
					{
						newLibraryInfo.FirstSymedId = Convert.ToUInt16(firstSymedIdText);
						
						string reservedRangeText = ((XmlElement)firstSymedIdNode).GetAttribute(XML_RESERVED_IDS_RANGE_ATTRIBUTE);
						if (!string.IsNullOrEmpty(reservedRangeText))
							newLibraryInfo.ReservedSymedIdsRange = Convert.ToUInt16(reservedRangeText);
					}
					catch(FormatException)
					{
					}
					catch(OverflowException)
					{
					}
				}			
			}

			ParseTablesInfo(newLibraryInfo, (XmlElement)aLibraryInfoNode);
			
			if (aModuleInfo.AddLibraryInfo(newLibraryInfo) == -1)
				return false;
		
			// devo necessariamente leggere la release di DB relativa alla creazione
			// delle tabelle DOPO aver specificato la libreria ( e quindi il modulo)
			// di appartenenza!!!
			ParseLibraryTablesDBRelease(newLibraryInfo, (XmlElement)aLibraryInfoNode);

			return true;
		}
		
		//---------------------------------------------------------------------------
		private bool ParseTablesInfo(WizardLibraryInfo aLibraryInfo, System.Xml.XmlElement aLibraryNode)
		{
			if (aLibraryInfo == null || aLibraryNode == null || String.Compare(aLibraryNode.Name, XML_TAG_LIBRARY) != 0)
				return false;
			
			try
			{
				XmlNode tablesInfoNode = aLibraryNode.SelectSingleNode("child::" + XML_TAG_TABLES);
				if (tablesInfoNode == null || !(tablesInfoNode is XmlElement) || !tablesInfoNode.HasChildNodes)
					return false;

				XmlNodeList tablesList = tablesInfoNode.SelectNodes("child::" + XML_TAG_TABLE);
				if (tablesList == null || tablesList.Count == 0)
					return false;

				foreach(XmlNode tableInfoNode in tablesList)
				{
					if (tableInfoNode == null || !(tableInfoNode is XmlElement))
						continue;

					if (!ParseTableInfoNode(aLibraryInfo, (XmlElement)tableInfoNode))
						return false;
				}

				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo ParseTableInfoNode(XmlElement aTableInfoNode)
		{
			if (aTableInfoNode == null)
				return null;

			string tableName = aTableInfoNode.GetAttribute(XML_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(tableName))
				return null;

			bool isReadOnly = false;
			if (aTableInfoNode.HasAttribute(XML_READONLY_ATTRIBUTE))
			{
				string readOnlyValue = aTableInfoNode.GetAttribute(XML_READONLY_ATTRIBUTE);
				if (!string.IsNullOrEmpty(readOnlyValue))
					isReadOnly = Boolean.Parse(readOnlyValue);
			}
			
            WizardTableInfo newTableInfo = new WizardTableInfo(tableName, isReadOnly);

			newTableInfo.ClassName = aTableInfoNode.GetAttribute(XML_CLASS_NAME_ATTRIBUTE);

            if (aTableInfoNode.HasAttribute(XML_TB_NAMESPACE_ATTRIBUTE))
                newTableInfo.TbNameSpace = aTableInfoNode.GetAttribute(XML_TB_NAMESPACE_ATTRIBUTE);

			string primaryKeyConstraintName = ((XmlElement)aTableInfoNode).GetAttribute(XML_TABLE_PRIMARY_KEY_CONSTRAINT_NAME_ATTRIBUTE);
			if (!String.IsNullOrEmpty(primaryKeyConstraintName))
			{
				newTableInfo.PrimaryKeyConstraintName = primaryKeyConstraintName;
				string clusteredValue = ((XmlElement)aTableInfoNode).GetAttribute(XML_TABLE_PRIMARY_KEY_CLUSTERED_ATTRIBUTE);
				newTableInfo.PrimaryKeyClustered = String.Compare(clusteredValue, Boolean.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0;
			}

			if (aTableInfoNode.HasAttribute(XML_TABLE_ADD_TBGUID_COLUMN_ATTRIBUTE))
			{
				string addTBGuidColumnValue = aTableInfoNode.GetAttribute(XML_TABLE_ADD_TBGUID_COLUMN_ATTRIBUTE);
				if (!string.IsNullOrEmpty(addTBGuidColumnValue))
					newTableInfo.AddTBGuidColumn = Boolean.Parse(addTBGuidColumnValue);
			}

			GetDBReleaseNumber(aTableInfoNode, newTableInfo);

			ParseColumnsInfo(newTableInfo, aTableInfoNode);
			
			if (newTableInfo.IsHKLDefined)
			{
				if (aTableInfoNode.HasAttribute(XML_TABLE_HKL_NAME_ATTRIBUTE))
				{
					string hotLinkName = aTableInfoNode.GetAttribute(XML_TABLE_HKL_NAME_ATTRIBUTE);
					if (!string.IsNullOrEmpty(hotLinkName))
						newTableInfo.HKLName = hotLinkName;
				}
				if (aTableInfoNode.HasAttribute(XML_TABLE_HKL_CLASS_NAME_ATTRIBUTE))
				{
					string hotLinkClassName = aTableInfoNode.GetAttribute(XML_TABLE_HKL_CLASS_NAME_ATTRIBUTE);
					if (!string.IsNullOrEmpty(hotLinkClassName))
						newTableInfo.HKLClassName = hotLinkClassName;
				}
			}

			ParseTableHistoryInfo(newTableInfo, aTableInfoNode);

			return newTableInfo;
		}
		
		//---------------------------------------------------------------------------
		private bool ParseTableInfoNode(WizardLibraryInfo aLibraryInfo, System.Xml.XmlElement aTableInfoNode)
		{
			if (aLibraryInfo == null || aTableInfoNode == null || String.Compare(aTableInfoNode.Name, XML_TAG_TABLE) != 0)
				return false;

			return (aLibraryInfo.AddTableInfo(ParseTableInfoNode(aTableInfoNode)) != -1);
		}
		
		//---------------------------------------------------------------------------
		private bool ParseColumnsInfo(WizardTableInfo aTableInfo, System.Xml.XmlElement aTableNode)
		{
			if (aTableInfo == null || aTableNode == null || String.Compare(aTableNode.Name, XML_TAG_TABLE) != 0)
				return false;
			
			try
			{
				XmlNode columnsInfoNode = aTableNode.SelectSingleNode("child::" + XML_TAG_COLUMNS);
				if (columnsInfoNode == null || !(columnsInfoNode is XmlElement) || !columnsInfoNode.HasChildNodes)
					return false;

				XmlNodeList columnsList = columnsInfoNode.SelectNodes("child::" + XML_TAG_COLUMN);
				if (columnsList == null || columnsList.Count == 0)
					return false;

				foreach(XmlNode columnInfoNode in columnsList)
				{
					if (columnInfoNode == null || !(columnInfoNode is XmlElement))
						continue;

					if (!ParseColumnInfoNode(aTableInfo, (XmlElement)columnInfoNode))
						continue;// return false; se no si blocca sulla colonna sbagliata...
				}

				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ParseColumnInfoNode(WizardTableInfo aTableInfo, System.Xml.XmlElement aColumnInfoNode)
		{
			if (aTableInfo == null || aColumnInfoNode == null || String.Compare(aColumnInfoNode.Name, XML_TAG_COLUMN) != 0)
				return false;

			WizardTableColumnInfo newColumnInfo = ParseBaseColumnInfoNode(aTableInfo.Name, aColumnInfoNode);

			if (newColumnInfo == null || aTableInfo.AddColumnInfo(newColumnInfo) == -1)
				return false;

			bool setHKLCodeColumn = false;
			if (aColumnInfoNode.HasAttribute(XML_COLUMN_HKL_CODE_ATTRIBUTE))
			{
				string columnIsHKLCode = aColumnInfoNode.GetAttribute(XML_COLUMN_HKL_CODE_ATTRIBUTE);
				if (!string.IsNullOrEmpty(columnIsHKLCode))
				{
					try
					{
						setHKLCodeColumn = Convert.ToBoolean(columnIsHKLCode);
					}
					catch(FormatException)
					{
					}
					catch(OverflowException)
					{
					}
				}
			}

			bool setHKLDescriptionColumn = false;
			if (aColumnInfoNode.HasAttribute(XML_COLUMN_HKL_DESCR_ATTRIBUTE))
			{
				string columnIsHKLDescription = aColumnInfoNode.GetAttribute(XML_COLUMN_HKL_DESCR_ATTRIBUTE);
				if (!string.IsNullOrEmpty(columnIsHKLDescription))
				{
					try
					{
						setHKLDescriptionColumn = Convert.ToBoolean(columnIsHKLDescription);
					}
					catch(FormatException)
					{
					}
					catch(OverflowException)
					{
					}
				}
			}
			
			if (setHKLCodeColumn)
				aTableInfo.HKLCodeColumn = newColumnInfo;

			if (setHKLDescriptionColumn)
				aTableInfo.HKLDescriptionColumn = newColumnInfo;

			return true;
		}

		//---------------------------------------------------------------------------
		protected WizardTableColumnInfo ParseBaseColumnInfoNode(string aTableName, System.Xml.XmlElement aColumnInfoNode)
		{
			if (aColumnInfoNode == null || String.Compare(aColumnInfoNode.Name, XML_TAG_COLUMN) != 0)
				return null;

			string columnName = aColumnInfoNode.GetAttribute(XML_NAME_ATTRIBUTE);
			if (String.IsNullOrEmpty(columnName) || !Generics.IsValidTableColumnName(columnName))
			{
				//se la colonna è TBGuid, anche se riservata la faccio aggiungere
				//Se è impostato anche il flag AddTBGuidColumn verrà creata la colonna una volta sola
				//e la colonna generata qui verrà ignorata
				if (!Generics.IsTBGuidColumnName(columnName))
					return null;
			}
				
			bool isReadOnly = false;
			if (aColumnInfoNode.HasAttribute(XML_READONLY_ATTRIBUTE))
			{
				string readOnlyValue = aColumnInfoNode.GetAttribute(XML_READONLY_ATTRIBUTE);
				if (!string.IsNullOrEmpty(readOnlyValue))
					isReadOnly = Boolean.Parse(readOnlyValue);
			}

			WizardTableColumnInfo parsedColumnInfo = new WizardTableColumnInfo(columnName, isReadOnly);

			string columnDataType = aColumnInfoNode.GetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE);
			if (!string.IsNullOrEmpty(columnDataType))
				parsedColumnInfo.DataType = WizardTableColumnDataType.Parse(columnDataType);

			if (parsedColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum &&
				applicationInfo != null)
			{
				// gestione solo x il wizard
				string columnEnumType = aColumnInfoNode.GetAttribute(XML_COLUMN_DATA_ENUM_ATTRIBUTE);
				if (string.IsNullOrEmpty(columnEnumType))
					throw new TBWizardException(String.Format(TBWizardProjectsStrings.MissingEnumTypeErrorMsg, aTableName, columnName));

				WizardEnumInfo columnEnumInfo = applicationInfo.GetEnumInfoByName(columnEnumType);
				if (columnEnumInfo == null && referencedApplicationsInfo != null && referencedApplicationsInfo.Count > 0)
				{
					foreach (WizardApplicationInfo aLoadedApplication in referencedApplicationsInfo)
					{
						if (aLoadedApplication.HasEnums)
						{
							columnEnumInfo = aLoadedApplication.GetEnumInfoByName(columnEnumType);
							break;
						}
					}
				}
				parsedColumnInfo.EnumInfo = columnEnumInfo;
			}
			
			if (parsedColumnInfo.DataType.IsTextual)
			{
				try
				{
					if (parsedColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.String &&
						aColumnInfoNode.HasAttribute(XML_COLUMN_LENGTH_ATTRIBUTE))
					{
						string columnLength = aColumnInfoNode.GetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE);
						if (!string.IsNullOrEmpty(columnLength))
							parsedColumnInfo.DataLength = Convert.ToUInt32(columnLength);
					}

					if (aColumnInfoNode.HasAttribute(XML_COLUMN_UPPER_CASE_ATTRIBUTE))
					{
						string isUpperCase = aColumnInfoNode.GetAttribute(XML_COLUMN_UPPER_CASE_ATTRIBUTE);
						if (!string.IsNullOrEmpty(isUpperCase))
							parsedColumnInfo.IsUpperCaseDataString = Convert.ToBoolean(isUpperCase);
					}
				}
				catch(FormatException)
				{
				}
				catch(OverflowException)
				{
				}
			}

			// prima controlliamo se è stata definita un'espressione per il default
			// se c'è prevale su un'eventuale definizione di un valore di default che invece deve essere compatibile
			// con il tipo della colonna
			if (aColumnInfoNode.HasAttribute(XML_COLUMN_DEFAULT_EXPRESSION_VALUE_ATTRIBUTE))
			{
				string defaultExpressionText = aColumnInfoNode.GetAttribute(XML_COLUMN_DEFAULT_EXPRESSION_VALUE_ATTRIBUTE);
				parsedColumnInfo.DefaultExpressionValue = defaultExpressionText;
			}
			else if (aColumnInfoNode.HasAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE))
			{
				string defaultValueText = aColumnInfoNode.GetAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE);
				parsedColumnInfo.SetDefaultValueFromString(defaultValueText);
			}

			//isnullable default= true, false solo se indicato
			string nullableValueText = aColumnInfoNode.GetAttribute(XML_COLUMN_NULLABLE_VALUE_ATTRIBUTE);
			parsedColumnInfo.IsNullable = String.Compare(nullableValueText, Boolean.FalseString, true) != 0;

			//collatesensitive default= true, false solo se indicato
			string collateSensitiveValueText = aColumnInfoNode.GetAttribute(XML_COLUMN_COLLATE_SENSITIVE_ATTRIBUTE);
			parsedColumnInfo.IsCollateSensitive = String.Compare(collateSensitiveValueText, Boolean.FalseString, true) != 0;
			
			// colonna IDENTITY
			string autoIncrementValueText = aColumnInfoNode.GetAttribute(XML_COLUMN_AUTO_INCREMENT_ATTRIBUTE);
			parsedColumnInfo.IsAutoIncrement = String.Compare(autoIncrementValueText, Boolean.TrueString, true) == 0;
			
			if (parsedColumnInfo.IsAutoIncrement)
			{
				string seedValueText = aColumnInfoNode.GetAttribute(XML_COLUMN_AUTO_INCREMENT_SEED_ATTRIBUTE);
				parsedColumnInfo.Seed = Int32.Parse(seedValueText);

				string incrementValueText = aColumnInfoNode.GetAttribute(XML_COLUMN_AUTO_INCREMENT_INCREMENT_ATTRIBUTE);
				parsedColumnInfo.Increment = Int32.Parse(incrementValueText);
			}

			string columnIsPrimaryKeySegment = aColumnInfoNode.GetAttribute(XML_COLUMN_PRIMARY_SEG_ATTRIBUTE);
			if (!string.IsNullOrEmpty(columnIsPrimaryKeySegment))
			{
				try
				{
					parsedColumnInfo.IsPrimaryKeySegment = Convert.ToBoolean(columnIsPrimaryKeySegment);
				}
				catch(FormatException)
				{
				}
				catch(OverflowException)
				{
				}
			}
			
			string defaultConstraintName = aColumnInfoNode.GetAttribute(XML_COLUMN_DEFAULT_CONSTRAINT_NAME_ATTRIBUTE);
			if (!string.IsNullOrEmpty(defaultConstraintName))
				parsedColumnInfo.DefaultConstraintName = defaultConstraintName;

			if (aColumnInfoNode.HasAttribute(XML_CREATE_STEP_ATTRIBUTE))
			{
				int createStep;
				if (int.TryParse(aColumnInfoNode.GetAttribute(XML_CREATE_STEP_ATTRIBUTE), out createStep))
					parsedColumnInfo.CreateStep = createStep;
			}

			string dbReleaseNumberText = aColumnInfoNode.GetAttribute(XML_DB_RELEASE_NUMBER_ATTRIBUTE);
			if (string.IsNullOrEmpty(dbReleaseNumberText))
				dbReleaseNumberText = aColumnInfoNode.GetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE);

			uint dbReleaseNumber = 0;
			if (!string.IsNullOrEmpty(dbReleaseNumberText))
				UInt32.TryParse(dbReleaseNumberText, out dbReleaseNumber);
			parsedColumnInfo.CreationDbReleaseNumber = dbReleaseNumber;

			return parsedColumnInfo;
		}

		//---------------------------------------------------------------------------
		private void ParseLibraryTablesDBRelease(WizardLibraryInfo aLibraryInfo, System.Xml.XmlElement aLibraryNode)
		{
			if (aLibraryInfo == null || aLibraryNode == null || String.Compare(aLibraryNode.Name, XML_TAG_LIBRARY) != 0)
				return;

			XmlNode tablesInfoNode = aLibraryNode.SelectSingleNode("child::" + XML_TAG_TABLES);
			if (tablesInfoNode == null || !(tablesInfoNode is XmlElement) || !tablesInfoNode.HasChildNodes)
				return;

			XmlNodeList tablesList = tablesInfoNode.SelectNodes("child::" + XML_TAG_TABLE);
			if (tablesList == null || tablesList.Count == 0)
				return;

			foreach(XmlNode tableInfoNode in tablesList)
			{
				if (tableInfoNode == null || !(tableInfoNode is XmlElement))
					continue;

				string tableName = ((XmlElement)tableInfoNode).GetAttribute(XML_NAME_ATTRIBUTE);
				if (string.IsNullOrEmpty(tableName))
					continue;

				WizardTableInfo tableInfo = aLibraryInfo.GetTableInfoByName(tableName);
				if (tableInfo == null)
					continue;

				string dbReleaseNumberText = ((XmlElement)tableInfoNode).GetAttribute(XML_DB_RELEASE_NUMBER_ATTRIBUTE);
				if (string.IsNullOrEmpty(dbReleaseNumberText))
					dbReleaseNumberText = ((XmlElement)tableInfoNode).GetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE);

				uint dbReleaseNumber = 0;
				if (!string.IsNullOrEmpty(dbReleaseNumberText))
					UInt32.TryParse(dbReleaseNumberText, out dbReleaseNumber);

				tableInfo.SetCreationDbReleaseNumber((dbReleaseNumber > 0) ? dbReleaseNumber : 1);

				if (tableInfo.ColumnsCount == 0)
					continue;

				XmlNode columnsInfoNode = tableInfoNode.SelectSingleNode("child::" + XML_TAG_COLUMNS);
				if (columnsInfoNode == null || !(columnsInfoNode is XmlElement) || !columnsInfoNode.HasChildNodes)
					continue;

				XmlNodeList columnsList = columnsInfoNode.SelectNodes("child::" + XML_TAG_COLUMN);
				if (columnsList == null || columnsList.Count == 0)
					continue;

				foreach(XmlNode columnInfoNode in columnsList)
				{
					if (columnInfoNode == null || !(columnInfoNode is XmlElement))
						continue;

					string columnName = ((XmlElement)columnInfoNode).GetAttribute(XML_NAME_ATTRIBUTE);
					if (string.IsNullOrEmpty(columnName))
						continue;

					WizardTableColumnInfo columnInfo = tableInfo.GetColumnInfoByName(columnName);
					if (columnInfo != null)
					{
						string columnDbReleaseNumberText = ((XmlElement)tableInfoNode).GetAttribute(XML_DB_RELEASE_NUMBER_ATTRIBUTE);
						if (string.IsNullOrEmpty(columnDbReleaseNumberText))
							columnDbReleaseNumberText = ((XmlElement)tableInfoNode).GetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE);

						uint columnDbReleaseNumber = 0;
						if (!string.IsNullOrEmpty(columnDbReleaseNumberText))
							UInt32.TryParse(columnDbReleaseNumberText, out columnDbReleaseNumber);

						columnInfo.CreationDbReleaseNumber = (columnDbReleaseNumber > 0) ? columnDbReleaseNumber : 1;		
					}
				}
			}
		}
		
		// TODO: gestire tutti i tipi di colonna mancanti (IDENTITY, espressioni nei valori di DEFAULT, etc.)
		//---------------------------------------------------------------------------
		private bool ParseTableHistoryInfo(WizardTableInfo aTableInfo, System.Xml.XmlElement aTableNode)
		{
			if (aTableInfo == null || aTableNode == null || String.Compare(aTableNode.Name, XML_TAG_TABLE) != 0)
				return false;
			
			try
			{
				XmlNode historyInfoNode = aTableNode.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY);
				if (historyInfoNode == null || !(historyInfoNode is XmlElement) || !historyInfoNode.HasChildNodes)
					return false;

				XmlNodeList stepsList = historyInfoNode.SelectNodes("child::" + XML_TAG_TABLE_HISTORY_STEP);
				if (stepsList == null || stepsList.Count == 0)
					return false;

				foreach(XmlNode stepNode in stepsList)
				{
					if (stepNode == null || !(stepNode is XmlElement) || !stepNode.HasChildNodes)
						continue;

					string dbReleaseNumberText = ((XmlElement)stepNode).GetAttribute(XML_DB_RELEASE_NUMBER_ATTRIBUTE);
					if (string.IsNullOrEmpty(dbReleaseNumberText))
						dbReleaseNumberText = ((XmlElement)stepNode).GetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE);

					uint dbReleaseNumber = 0;
					if (!string.IsNullOrEmpty(dbReleaseNumberText))
						UInt32.TryParse(dbReleaseNumberText, out dbReleaseNumber);

					if (dbReleaseNumber <= 0)
						continue;

					XmlNode eventsInfoNode = stepNode.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY_EVENTS);
					if (eventsInfoNode == null || !(eventsInfoNode is XmlElement) || !eventsInfoNode.HasChildNodes)
						continue; // non ci sono eventi!
					
					XmlNodeList columnEventsList = eventsInfoNode.SelectNodes("child::" + XML_TAG_TABLE_HISTORY_COLUMN_EVENT);
					XmlNodeList indexEventsList = eventsInfoNode.SelectNodes("child::" + XML_TAG_TABLE_HISTORY_INDEX_EVENT);
					XmlNodeList foreignKeyEventsList = eventsInfoNode.SelectNodes("child::" + XML_TAG_TABLE_HISTORY_FOREIGN_KEY_EVENT);
					if 
						(
						(columnEventsList == null || columnEventsList.Count == 0) && 
						(indexEventsList == null || indexEventsList.Count == 0) &&
						(foreignKeyEventsList == null || foreignKeyEventsList.Count == 0)
						)
						continue; // non ci sono eventi!

					TableHistoryStep stepInfo = new TableHistoryStep(dbReleaseNumber);

					string primaryKeyConstraintName = ((XmlElement)stepNode).GetAttribute(XML_TABLE_PRIMARY_KEY_CONSTRAINT_NAME_ATTRIBUTE);
					if (!string.IsNullOrEmpty(primaryKeyConstraintName))
						stepInfo.PrimaryKeyConstraintName = primaryKeyConstraintName;

					// Caricamento delle informazioni relative alle modifiche sulle colonne
					if (columnEventsList != null && columnEventsList.Count > 0)
					{
						foreach(XmlNode columnEventNode in columnEventsList)
						{
							if 
								(
								columnEventNode == null || 
								!(columnEventNode is XmlElement) ||
								!columnEventNode.HasChildNodes ||
								!((XmlElement)columnEventNode).HasAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE)
								)
								continue;

							string eventTypeText = ((XmlElement)columnEventNode).GetAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE);
							if (string.IsNullOrEmpty(eventTypeText))
								continue;

							TableHistoryStep.EventType columnEventType = TableHistoryStep.EventType.Undefined;

							try
							{
								columnEventType = (TableHistoryStep.EventType)Enum.Parse(typeof(TableHistoryStep.EventType), eventTypeText, false); 
							}
							catch (ArgumentException)
							{
							}
						
							if (columnEventType == TableHistoryStep.EventType.Undefined)
								continue;

							int columnOrder = -1;
							if (((XmlElement)columnEventNode).HasAttribute(XML_COL_HISTORY_EVENT_ORDER_ATTRIBUTE))
							{
								string columnOrderText = ((XmlElement)columnEventNode).GetAttribute(XML_COL_HISTORY_EVENT_ORDER_ATTRIBUTE);
								if (!string.IsNullOrEmpty(columnOrderText))
								{
									try
									{
										columnOrder = Convert.ToInt32(columnOrderText);
									}
									catch(FormatException)
									{
									}
									catch(OverflowException)
									{
									}
								}
							}
					
							int previousColumnOrder = -1;
							if (((XmlElement)columnEventNode).HasAttribute(XML_COL_HISTORY_EVENT_PREVIOUS_ORDER_ATTRIBUTE))
							{
								string previousColumnOrderText = ((XmlElement)columnEventNode).GetAttribute(XML_COL_HISTORY_EVENT_PREVIOUS_ORDER_ATTRIBUTE);
								if (!string.IsNullOrEmpty(previousColumnOrderText))
								{
									try
									{
										previousColumnOrder = Convert.ToInt32(previousColumnOrderText);
									}
									catch(FormatException)
									{
									}
									catch(OverflowException)
									{
									}
								}
							}

							XmlNode columnInfoNode = columnEventNode.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY_COLUMN);
							if (columnInfoNode == null || !(columnInfoNode is XmlElement))
								continue; // non ci sono informazioni sulla colonna!

							WizardTableColumnInfo columnInfo = ParseHistoryColumnInfo((XmlElement)columnInfoNode);
							if (columnInfo == null)
								continue; // non ci sono informazioni sulla colonna!
						
							WizardTableColumnInfo previousColumnInfo = null;
							XmlNode previousColumnInfoNode = columnEventNode.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY_PREVIOUS);
							if (previousColumnInfoNode != null &&  previousColumnInfoNode is XmlElement)
							{
								if (columnEventType == TableHistoryStep.EventType.AlterColumnType ||
									columnEventType == TableHistoryStep.EventType.RenameColumn ||
									columnEventType == TableHistoryStep.EventType.ChangeColumnDefaultValue ||
									columnEventType == TableHistoryStep.EventType.ModifyPrimaryKey)
								{
									string previousColumnName = columnInfo.Name;
									if (columnEventType == TableHistoryStep.EventType.RenameColumn)
									{
										previousColumnName = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_NAME_ATTRIBUTE);
										if (string.IsNullOrEmpty(previousColumnName))
											continue;
									}

									previousColumnInfo = new WizardTableColumnInfo(previousColumnName);
							
									if (columnEventType == TableHistoryStep.EventType.AlterColumnType || columnEventType == TableHistoryStep.EventType.RenameColumn)
									{
										string previousColumnDataType = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE);
										if (!string.IsNullOrEmpty(previousColumnDataType))
											previousColumnInfo.DataType = WizardTableColumnDataType.Parse(previousColumnDataType);

										if (applicationInfo != null && previousColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum)
										{
											string columnEnumType = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_COLUMN_DATA_ENUM_ATTRIBUTE);
											if (!string.IsNullOrEmpty(columnEnumType))
											{
												WizardEnumInfo enumInfo = applicationInfo.GetEnumInfoByName(columnEnumType);
												if (enumInfo == null && referencedApplicationsInfo != null && referencedApplicationsInfo.Count > 0)
												{
													foreach(WizardApplicationInfo aLoadedApplication in referencedApplicationsInfo)
													{
														if (aLoadedApplication.HasEnums)
														{
															enumInfo = aLoadedApplication.GetEnumInfoByName(columnEnumType);
															break;
														}
													}
												}
												previousColumnInfo.EnumInfo = enumInfo;
											}
										}
			
										if (previousColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.String)
										{
											string previousColumnLength = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE);
											if (!string.IsNullOrEmpty(previousColumnLength))
											{
												try
												{
													previousColumnInfo.DataLength = Convert.ToUInt32(previousColumnLength);
												}
												catch(FormatException)
												{
												}
												catch(OverflowException)
												{
												}
											}
										}
									}
								
									if (columnEventType == TableHistoryStep.EventType.ChangeColumnDefaultValue)
									{
										previousColumnInfo.DataType = new WizardTableColumnDataType(columnInfo.DataType.Type);
										previousColumnInfo.DataLength = columnInfo.DataLength;

										if (((XmlElement)previousColumnInfoNode).HasAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE))
										{
											string previousColumnDefaultValueText = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE);

											if (!string.IsNullOrEmpty(previousColumnDefaultValueText))
											{
												if (columnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum)
												{
													previousColumnInfo.DataType = new WizardTableColumnDataType(WizardTableColumnDataType.DataType.Enum);

													if (applicationInfo != null)
													{
														try
														{
															uint defaultEnumItem = UInt32.Parse(previousColumnDefaultValueText);
															WizardEnumInfo enumInfo = applicationInfo.GetEnumInfoByItemStoredValue(defaultEnumItem);
															if (enumInfo == null && referencedApplicationsInfo != null && referencedApplicationsInfo.Count > 0)
															{
																foreach(WizardApplicationInfo aLoadedApplication in referencedApplicationsInfo)
																{
																	if (aLoadedApplication.HasEnums)
																	{
																		enumInfo = aLoadedApplication.GetEnumInfoByItemStoredValue(defaultEnumItem);
																		break;
																	}
																}
															}
															previousColumnInfo.EnumInfo = enumInfo;
														}
														catch(FormatException)
														{
														}
														catch(OverflowException)
														{
														}
													}
												}
										
												previousColumnInfo.SetDefaultValueFromString(previousColumnDefaultValueText);
											}
										}
									}

									string previousColumnIsPrimaryKeySegment = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_COLUMN_PRIMARY_SEG_ATTRIBUTE);
									if (!string.IsNullOrEmpty(previousColumnIsPrimaryKeySegment))
									{
										try
										{
											previousColumnInfo.IsPrimaryKeySegment = Convert.ToBoolean(previousColumnIsPrimaryKeySegment);
										}
										catch(FormatException)
										{
										}
										catch(OverflowException)
										{
										}
									}
								
									string previousDefaultConstraintName = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_COLUMN_DEFAULT_CONSTRAINT_NAME_ATTRIBUTE);
									if (!string.IsNullOrEmpty(previousDefaultConstraintName))
										previousColumnInfo.DefaultConstraintName = previousDefaultConstraintName;
								}					
							}
							stepInfo.AddColumnEvent(columnInfo, columnOrder, previousColumnInfo, previousColumnOrder, columnEventType);
						}	
					}

					// Caricamento delle informazioni relative alle modifiche sugli indici
					if (indexEventsList != null && indexEventsList.Count > 0)
					{
						foreach(XmlNode indexEventNode in indexEventsList)
						{
							if (indexEventNode == null || 
								!(indexEventNode is XmlElement) ||
								!indexEventNode.HasChildNodes ||
								!((XmlElement)indexEventNode).HasAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE))
								continue;

							string eventTypeText = ((XmlElement)indexEventNode).GetAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE);
							if (string.IsNullOrEmpty(eventTypeText))
								continue;

							TableHistoryStep.EventType indexEventType = TableHistoryStep.EventType.Undefined;

							try
							{
								indexEventType = (TableHistoryStep.EventType)Enum.Parse(typeof(TableHistoryStep.EventType), eventTypeText, false); 
							}
							catch (ArgumentException)
							{
							}
					
							if (indexEventType == TableHistoryStep.EventType.Undefined)
								continue;

							XmlNode indexInfoNode = indexEventNode.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY_INDEX_INFO);
							if (indexInfoNode == null || !(indexInfoNode is XmlElement))
								continue;

							string indexName = ((XmlElement)indexInfoNode).GetAttribute(XML_INDEX_NAME_ATTRIBUTE);
							if (string.IsNullOrEmpty(indexName))
								continue;

							bool isPrimaryIndex = false;
							if (((XmlElement)indexInfoNode).HasAttribute(XML_INDEX_PRIMARY_ATTRIBUTE))
							{
								string isPrimaryIndexText = ((XmlElement)indexInfoNode).GetAttribute(XML_INDEX_PRIMARY_ATTRIBUTE);
								if (!string.IsNullOrEmpty(isPrimaryIndexText))
								{
									try
									{
										isPrimaryIndex = Convert.ToBoolean(isPrimaryIndexText);
									}
									catch(FormatException)
									{
									}
									catch(OverflowException)
									{
									}
								}
							}

							WizardTableIndexInfo indexInfo = new WizardTableIndexInfo(indexName, isPrimaryIndex);

							XmlNodeList segmentsList = indexInfoNode.SelectNodes("child::" + XML_TAG_TABLE_HISTORY_INDEX_SEGMENT);
							if (segmentsList != null && segmentsList.Count > 0)
							{
								foreach(XmlNode segmentInfoNode in segmentsList)
								{
									if (segmentInfoNode == null || !(segmentInfoNode is XmlElement))
										continue;

									WizardTableColumnInfo segmentInfo = ParseHistoryColumnInfo((XmlElement)segmentInfoNode);
									if (segmentInfo != null)
										indexInfo.AddSegmentInfo(segmentInfo);
								}
							}

							WizardTableIndexInfo previousindexInfo = null;
							XmlNode previousIndexInfoNode = indexEventNode.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY_INDEX_PREVIOUS_INFO);
							if (previousIndexInfoNode != null && previousIndexInfoNode is XmlElement)
							{
								string previousIndexName = ((XmlElement)previousIndexInfoNode).GetAttribute(XML_INDEX_NAME_ATTRIBUTE);
								if (!string.IsNullOrEmpty(previousIndexName))
								{
									bool wasPrimaryIndex = false;
									if (((XmlElement)previousIndexInfoNode).HasAttribute(XML_INDEX_PRIMARY_ATTRIBUTE))
									{
										string isPrimaryIndexText = ((XmlElement)previousIndexInfoNode).GetAttribute(XML_INDEX_PRIMARY_ATTRIBUTE);
										if (!string.IsNullOrEmpty(isPrimaryIndexText))
										{
											try
											{
												wasPrimaryIndex = Convert.ToBoolean(isPrimaryIndexText);
											}
											catch(FormatException)
											{
											}
											catch(OverflowException)
											{
											}
										}
									}

									previousindexInfo = new WizardTableIndexInfo(previousIndexName, wasPrimaryIndex);
								
									XmlNodeList previousSegmentsList = previousIndexInfoNode.SelectNodes("child::" + XML_TAG_TABLE_HISTORY_INDEX_SEGMENT);
									if (previousSegmentsList != null && previousSegmentsList.Count > 0)
									{
										foreach(XmlNode segmentInfoNode in previousSegmentsList)
										{
											if (segmentInfoNode == null || !(segmentInfoNode is XmlElement))
												continue;

											WizardTableColumnInfo segmentInfo = ParseHistoryColumnInfo((XmlElement)segmentInfoNode);
											if (segmentInfo != null)
												indexInfo.AddSegmentInfo(segmentInfo);
										}
									}
								}
							}

							stepInfo.AddIndexEvent(indexInfo, previousindexInfo, indexEventType);
						}		
					}

					// Caricamento delle informazioni relative alle modifiche sui vincoli di foreign key
					if (foreignKeyEventsList != null && foreignKeyEventsList.Count > 0)
					{
						foreach(XmlNode foreignKeyEventNode in foreignKeyEventsList)
						{
							if (foreignKeyEventNode == null || 
								!(foreignKeyEventNode is XmlElement) ||
								!foreignKeyEventNode.HasChildNodes ||
								!((XmlElement)foreignKeyEventNode).HasAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE))
								continue;

							string eventTypeText = ((XmlElement)foreignKeyEventNode).GetAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE);
							if (string.IsNullOrEmpty(eventTypeText))
								continue;

							TableHistoryStep.EventType foreignKeyEventType = TableHistoryStep.EventType.Undefined;

							try
							{
								foreignKeyEventType = (TableHistoryStep.EventType)Enum.Parse(typeof(TableHistoryStep.EventType), eventTypeText, false); 
							}
							catch (ArgumentException)
							{
							}
					
							if (foreignKeyEventType == TableHistoryStep.EventType.Undefined)
								continue;

							XmlNode foreignKeyInfoNode = foreignKeyEventNode.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY_FOREIGN_KEY_INFO);
							if (foreignKeyInfoNode == null || !(foreignKeyInfoNode is XmlElement))
								continue;
							
							string referencedTableNameSpace = ((XmlElement)foreignKeyInfoNode).GetAttribute(XML_REFERENCED_TABLE_NAMESPACE_ATTRIBUTE);
							if (string.IsNullOrEmpty(referencedTableNameSpace))
								continue;
							
							string constraintName = ((XmlElement)foreignKeyInfoNode).GetAttribute(XML_FOREIGN_KEY_CONSTRAINT_NAME_ATTRIBUTE);
							if (foreignKeyEventType == TableHistoryStep.EventType.DropConstraint &&
								string.IsNullOrEmpty(constraintName))
								continue;
	
							WizardForeignKeyInfo foreignKeyInfo = new WizardForeignKeyInfo(referencedTableNameSpace); 
							foreignKeyInfo.ConstraintName = constraintName;

							if (foreignKeyEventType == TableHistoryStep.EventType.CreateConstraint)
							{
								XmlNodeList segmentsList = foreignKeyInfoNode.SelectNodes("child::" + XML_TAG_FOREIGN_KEY_SEGMENT);
								if (segmentsList == null || segmentsList.Count == 0)
									continue;
							
								foreach(XmlNode segmentInfoNode in segmentsList)
								{
									if (segmentInfoNode == null || 
										!(segmentInfoNode is XmlElement) ||
										!((XmlElement)segmentInfoNode).HasAttribute(XML_FOREIGN_KEY_SEG_COLUMN_ATTRIBUTE) ||
										!((XmlElement)segmentInfoNode).HasAttribute(XML_FOREIGN_KEY_SEG_REFERENCED_COLUMN_ATTRIBUTE))
										continue;

									string columnName = ((XmlElement)segmentInfoNode).GetAttribute(XML_FOREIGN_KEY_SEG_COLUMN_ATTRIBUTE);
									if (string.IsNullOrEmpty(columnName))
										continue;

									string referencedColumnName = ((XmlElement)segmentInfoNode).GetAttribute(XML_FOREIGN_KEY_SEG_REFERENCED_COLUMN_ATTRIBUTE);
									if (string.IsNullOrEmpty(referencedColumnName))
										continue;
					
									foreignKeyInfo.AddKeySegment(new WizardForeignKeyInfo.KeySegment(columnName, referencedColumnName));
								}
							}
							
							stepInfo.AddForeignKeyConstraintEvent(foreignKeyInfo, foreignKeyEventType);
						}
					}
					
					aTableInfo.AddHistoryStep(stepInfo);
				}

				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}

		// TODO: gestire tutti i tipi di colonna mancanti (IDENTITY, espressioni nei valori di DEFAULT, etc.)
		//---------------------------------------------------------------------------
		private WizardTableColumnInfo ParseHistoryColumnInfo(XmlElement aHistoryColumnElement)
		{
			if (aHistoryColumnElement == null || !aHistoryColumnElement.HasAttribute(XML_NAME_ATTRIBUTE))
				return null;
						
			string columnName = aHistoryColumnElement.GetAttribute(XML_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(columnName))
				return null;

			WizardTableColumnInfo columnInfo = new WizardTableColumnInfo(columnName);
						
			string columnDataType = aHistoryColumnElement.GetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE);
			if (!string.IsNullOrEmpty(columnDataType))
				columnInfo.DataType = WizardTableColumnDataType.Parse(columnDataType);

			if (applicationInfo != null && columnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum)
			{
				string columnEnumType = aHistoryColumnElement.GetAttribute(XML_COLUMN_DATA_ENUM_ATTRIBUTE);
				if (!string.IsNullOrEmpty(columnEnumType))
				{
					WizardEnumInfo enumInfo = applicationInfo.GetEnumInfoByName(columnEnumType);
					if (enumInfo == null && referencedApplicationsInfo != null && referencedApplicationsInfo.Count > 0)
					{
						foreach(WizardApplicationInfo aLoadedApplication in referencedApplicationsInfo)
						{
							if (aLoadedApplication.HasEnums)
							{
								enumInfo = aLoadedApplication.GetEnumInfoByName(columnEnumType);
								break;
							}
						}
					}
					columnInfo.EnumInfo = enumInfo;
				}
			}
			
			if (columnInfo.DataType.Type == WizardTableColumnDataType.DataType.String)
			{
				string columnLength = aHistoryColumnElement.GetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE);
				if (!string.IsNullOrEmpty(columnLength))
				{
					try
					{
						columnInfo.DataLength = Convert.ToUInt32(columnLength);
					}
					catch(FormatException)
					{
					}
					catch(OverflowException)
					{
					}
				}
			}
						
			if (aHistoryColumnElement.HasAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE))
			{
				string defaultValueText = aHistoryColumnElement.GetAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE);
				columnInfo.SetDefaultValueFromString(defaultValueText);
			}
						
			string columnIsPrimaryKeySegment = aHistoryColumnElement.GetAttribute(XML_COLUMN_PRIMARY_SEG_ATTRIBUTE);
			if (!string.IsNullOrEmpty(columnIsPrimaryKeySegment))
			{
				try
				{
					columnInfo.IsPrimaryKeySegment = Convert.ToBoolean(columnIsPrimaryKeySegment);
				}
				catch(FormatException)
				{
				}
				catch(OverflowException)
				{
				}
			}

			string defaultConstraintName = aHistoryColumnElement.GetAttribute(XML_COLUMN_DEFAULT_CONSTRAINT_NAME_ATTRIBUTE);
			if (!string.IsNullOrEmpty(defaultConstraintName))
				columnInfo.DefaultConstraintName = defaultConstraintName;

			return columnInfo;
		}
		
		//---------------------------------------------------------------------------
		private bool ParseDBTsInfo(System.Xml.XmlElement aApplicationNode)
		{
			if (applicationInfo == null || applicationInfo.ModulesCount == 0 || aApplicationNode == null ||
				String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0)
				return false;
			
			try
			{
				foreach (WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
				{
					if (aModuleInfo.LibrariesCount == 0)
						continue;
					
					XmlNode moduleInfoNode = aApplicationNode.SelectSingleNode("descendant::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE +"='" + aModuleInfo.Name + "']");
					if (moduleInfoNode == null || !(moduleInfoNode is XmlElement))
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						XmlNode libraryInfoNode = moduleInfoNode.SelectSingleNode("descendant::" + XML_TAG_LIBRARY + "[@" + XML_NAME_ATTRIBUTE +"='" + aLibraryInfo.Name + "']");
						if (libraryInfoNode == null || !(libraryInfoNode is XmlElement) || !libraryInfoNode.HasChildNodes)
							continue;

						XmlNode dbtsInfoNode = libraryInfoNode.SelectSingleNode("child::" + XML_TAG_DBTS);
						if (dbtsInfoNode == null || !(dbtsInfoNode is XmlElement) || !dbtsInfoNode.HasChildNodes)
							continue;

						XmlNodeList dbtsList = dbtsInfoNode.SelectNodes("child::" + XML_TAG_DBT);
						if (dbtsList == null || dbtsList.Count == 0)
							continue;

						foreach(XmlNode dbtInfoNode in dbtsList)
						{
							if (dbtInfoNode == null || !(dbtInfoNode is XmlElement))
								continue;

							ParseDBTInfoNode(aLibraryInfo, (XmlElement)dbtInfoNode);
						}
					}
				}
				
				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}

		//---------------------------------------------------------------------------
		private bool ParseDBTInfoNode(WizardLibraryInfo aLibraryInfo, System.Xml.XmlElement aDBTInfoNode)
		{
			if (aLibraryInfo == null || aDBTInfoNode == null || String.Compare(aDBTInfoNode.Name, XML_TAG_DBT) != 0)
				return false;

			string dbtName = aDBTInfoNode.GetAttribute(XML_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(dbtName))
				return false;
			
			string tableName = aDBTInfoNode.GetAttribute(XML_TABLE_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(tableName))
				return false;
			
			string dbtTypeText = aDBTInfoNode.GetAttribute(XML_DBT_TYPE_ATTRIBUTE);
			if (string.IsNullOrEmpty(dbtTypeText))
				return false;

			WizardDBTInfo.DBTType dbtType = WizardDBTInfo.DBTType.Undefined;
			try
			{
				dbtType = (WizardDBTInfo.DBTType)Enum.Parse(typeof(WizardDBTInfo.DBTType), dbtTypeText);
			}
			catch(ArgumentNullException exception)
			{
				Debug.Fail("ArgumentNullException raised during parsing of DBT type in TBWizardProjectParser.ParseDBTInfoNode." + exception.Message);
			}
			catch(ArgumentException exception)
			{
				Debug.Fail("ArgumentException raised during parsing of DBT type in TBWizardProjectParser.ParseDBTInfoNode." + exception.Message);
			}

			if (dbtType == WizardDBTInfo.DBTType.Undefined)
				return false;

			bool onlyForClientDoc = false;
			if (dbtType == WizardDBTInfo.DBTType.Slave || dbtType == WizardDBTInfo.DBTType.SlaveBuffered)
			{
				if (aDBTInfoNode.HasAttribute(XML_ONLY_FOR_CLIENT_DOCUMENT_ATTRIBUTE))
				{
					string onlyForClientDocValue = aDBTInfoNode.GetAttribute(XML_ONLY_FOR_CLIENT_DOCUMENT_ATTRIBUTE);
					if (!string.IsNullOrEmpty(onlyForClientDocValue))
						onlyForClientDoc = Boolean.Parse(onlyForClientDocValue);
				}
			}

			string slaveTabTitle = String.Empty;

			if (dbtType == WizardDBTInfo.DBTType.Slave || dbtType == WizardDBTInfo.DBTType.SlaveBuffered)
			{
				if (onlyForClientDoc)
				{
					// Controllo che esista la specifica del server document, altrimenti non considero il DBT.
					XmlNode serverDocumentNode = aDBTInfoNode.SelectSingleNode("child::" + XML_TAG_SERVER_DOCUMENT);
					if (serverDocumentNode == null || !(serverDocumentNode is XmlElement) ||
						serverDocumentNode.InnerText == null || serverDocumentNode.InnerText.Length == 0)
						return false;
				}
				else
				{
					// Controllo che esista l'attributo del Master correlato, altrimenti non considero il
					// DBT Slave. L'assegnazione del master verrà però fatta poi in seguito (quando avrò 
					// caricato TUTTI i DBT), in modo da poter verificare la consistenza del dato, cioè che
					// il nome qui specificato corrisponda effettivamente ad un DBTMaster definito nell'applicazione
					string relatedDBTMasterName = aDBTInfoNode.GetAttribute(XML_REL_DBTMASTER_NAME_ATTRIBUTE);
					if (string.IsNullOrEmpty(relatedDBTMasterName))
						return false;
				}

				XmlNode tabTitleNode = aDBTInfoNode.SelectSingleNode("child::" + XML_TAG_DBT_SLAVE_TAB_TITLE);
				if (tabTitleNode != null && (tabTitleNode is XmlElement))
					slaveTabTitle = tabTitleNode.InnerText;
			}

			bool isReadOnly = false;
			if (aDBTInfoNode.HasAttribute(XML_READONLY_ATTRIBUTE))
			{
				string readOnlyValue = aDBTInfoNode.GetAttribute(XML_READONLY_ATTRIBUTE);
				if (!string.IsNullOrEmpty(readOnlyValue))
					isReadOnly = Boolean.Parse(readOnlyValue);
			}

			WizardDBTInfo newDBTInfo = new WizardDBTInfo(dbtName, aLibraryInfo, tableName, dbtType, isReadOnly);

			newDBTInfo.ClassName = aDBTInfoNode.GetAttribute(XML_CLASS_NAME_ATTRIBUTE);
			newDBTInfo.SlaveTabTitle = slaveTabTitle;
			
			newDBTInfo.OnlyForClientDocumentAvailable = onlyForClientDoc;
			if (onlyForClientDoc && aDBTInfoNode.HasAttribute(XML_MASTER_TABLE_HEADER_ATTRIBUTE))
			{
				string relativeFileName = aDBTInfoNode.GetAttribute(XML_MASTER_TABLE_HEADER_ATTRIBUTE);
				if (relativeFileName != null && relativeFileName.Trim().Length > 0)
					newDBTInfo.MasterTableIncludeFile = Generics.BuildFullPath(WizardCodeGenerator.GetStandardLibraryPath(aLibraryInfo), relativeFileName);
			}
	
			if (dbtType == WizardDBTInfo.DBTType.SlaveBuffered && aDBTInfoNode.HasAttribute(XML_DBT_SLAVEBUFFERED_CREATE_ROW_FORM_ATTRIBUTE))
			{
				string createRowFormValue = aDBTInfoNode.GetAttribute(XML_DBT_SLAVEBUFFERED_CREATE_ROW_FORM_ATTRIBUTE);
				if (!string.IsNullOrEmpty(createRowFormValue))
					newDBTInfo.CreateRowForm = Boolean.Parse(createRowFormValue);
			}

			WizardTableInfo tableInfo = newDBTInfo.GetTableInfo();
			if (tableInfo != null && tableInfo.IsReferenced && aDBTInfoNode.HasAttribute(XML_REFERENCED_TABLE_HEADER_ATTRIBUTE))
			{
				string relativeFileName = aDBTInfoNode.GetAttribute(XML_REFERENCED_TABLE_HEADER_ATTRIBUTE);
				if (!string.IsNullOrEmpty(relativeFileName))
					newDBTInfo.ReferencedTableIncludeFile = Generics.BuildFullPath(WizardCodeGenerator.GetStandardLibraryPath(aLibraryInfo), relativeFileName);
			}

			ParseDBTColumnsInfo(newDBTInfo, aDBTInfoNode);

			return (aLibraryInfo.AddDBTInfo(newDBTInfo) != -1);
		}
		
		//---------------------------------------------------------------------------
		private bool ParseDBTColumnsInfo(WizardDBTInfo aDBTInfo, System.Xml.XmlElement aDBTNode)
		{
			if (aDBTInfo == null || aDBTNode == null || String.Compare(aDBTNode.Name, XML_TAG_DBT) != 0)
				return false;
			
			try
			{
				XmlNode columnsInfoNode = aDBTNode.SelectSingleNode("child::" + XML_TAG_COLUMNS);
				if (columnsInfoNode == null || !(columnsInfoNode is XmlElement) || !columnsInfoNode.HasChildNodes)
					return false;

				XmlNodeList columnsList = columnsInfoNode.SelectNodes("child::" + XML_TAG_COLUMN);
				if (columnsList == null || columnsList.Count == 0)
					return false;

				foreach(XmlNode columnInfoNode in columnsList)
				{
					if (columnInfoNode == null || !(columnInfoNode is XmlElement))
						continue;

					WizardDBTColumnInfo dbtColumnInfo = ParseDBTColumnInfoNode(aDBTInfo, (XmlElement)columnInfoNode);
					if (dbtColumnInfo == null)
						return false;

					aDBTInfo.SetColumnInfo(dbtColumnInfo);
				}

				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private WizardDBTColumnInfo ParseDBTColumnInfoNode(WizardDBTInfo aDBTInfo, System.Xml.XmlElement aColumnInfoNode)
		{
			if (aDBTInfo == null || aColumnInfoNode == null || String.Compare(aColumnInfoNode.Name, XML_TAG_COLUMN) != 0)
				return null;

			string columnName = aColumnInfoNode.GetAttribute(XML_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(columnName))
				return null;
			
			WizardTableColumnInfo tableColumnInfo = aDBTInfo.GetTableColumnInfoByName(columnName);
			if (tableColumnInfo == null)
				return null;

			bool isReadOnly = false;
			if (aColumnInfoNode.HasAttribute(XML_READONLY_ATTRIBUTE))
			{
				string readOnlyValue = aColumnInfoNode.GetAttribute(XML_READONLY_ATTRIBUTE);
				if (!string.IsNullOrEmpty(readOnlyValue))
					isReadOnly = Boolean.Parse(readOnlyValue);
			}

			WizardDBTColumnInfo newColumnInfo = new WizardDBTColumnInfo(tableColumnInfo, isReadOnly);

			XmlNode titleNode = aColumnInfoNode.SelectSingleNode("child::" + XML_TAG_DBTCOLUMN_TITLE);
			if (titleNode != null && (titleNode is XmlElement))
			{
				string title = titleNode.InnerText;
				if (!string.IsNullOrEmpty(title))
					newColumnInfo.Title = title;
			}

			string columnIsVisible = aColumnInfoNode.GetAttribute(XML_DBT_COLUMN_VISIBLE_ATTRIBUTE);
			if (!string.IsNullOrEmpty(columnIsVisible))
			{
				try
				{
					newColumnInfo.Visible = Convert.ToBoolean(columnIsVisible);
				}
				catch(FormatException)
				{
				}
				catch(OverflowException)
				{
				}
			}

			string columnIsFindable = aColumnInfoNode.GetAttribute(XML_DBT_COLUMN_FINDABLE_ATTRIBUTE);
			if (!string.IsNullOrEmpty(columnIsFindable))
			{
				try
				{
					newColumnInfo.Findable = Convert.ToBoolean(columnIsFindable);
				}
				catch(FormatException)
				{
				}
				catch(OverflowException)
				{
				}
			}

			string columnLeft = aColumnInfoNode.GetAttribute(XML_LEFT_ATTRIBUTE);
			string columnTop = aColumnInfoNode.GetAttribute(XML_TOP_ATTRIBUTE);
			string columnHeight = aColumnInfoNode.GetAttribute(XML_HEIGHT_ATTRIBUTE);
			string columnWidth = aColumnInfoNode.GetAttribute(XML_WIDTH_ATTRIBUTE);
			if (!string.IsNullOrEmpty(columnLeft) && !string.IsNullOrEmpty(columnTop) &&
				!string.IsNullOrEmpty(columnHeight) && !string.IsNullOrEmpty(columnWidth) 
				)
			{
				try
				{
					newColumnInfo.Position.Left = int.Parse(columnLeft);
					newColumnInfo.Position.Top = int.Parse(columnTop);
					newColumnInfo.Position.Width = int.Parse(columnWidth);
					newColumnInfo.Position.Height = int.Parse(columnHeight);
					newColumnInfo.LabelAdded = true;
				}
				catch (FormatException)
				{
				}
				catch (OverflowException)
				{
				}
			}

			return newColumnInfo;
		}

		//---------------------------------------------------------------------------
		protected IList<WizardForeignKeyInfo> ParseTableForeignKeysInfo(XmlElement aTableInfoNode)
		{
			if (aTableInfoNode == null)
				return null;

			string tableName = aTableInfoNode.GetAttribute(XML_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(tableName))
				return null;

			XmlNode foreignKeysNode = aTableInfoNode.SelectSingleNode("child::" + XML_TAG_FOREIGN_KEYS);

			if (foreignKeysNode == null || !(foreignKeysNode is XmlElement) || !foreignKeysNode.HasChildNodes)
				return null;
			
			XmlNodeList foreignKeysList = foreignKeysNode.SelectNodes("child::" + XML_TAG_FOREIGN_KEY);
			if (foreignKeysList == null || foreignKeysList.Count == 0)
				return null;

			IList<WizardForeignKeyInfo> list = new List<WizardForeignKeyInfo>();

			foreach (XmlNode foreignKeyNode in foreignKeysList)
			{
				if (foreignKeyNode == null || !((XmlElement)foreignKeyNode).HasAttribute(XML_REFERENCED_TABLE_NAMESPACE_ATTRIBUTE))
					continue;

				XmlNodeList segmentsList = foreignKeyNode.SelectNodes("child::" + XML_TAG_FOREIGN_KEY_SEGMENT);
				if (segmentsList == null || segmentsList.Count == 0)
					continue;

				string referencedTableNameSpace = ((XmlElement)foreignKeyNode).GetAttribute(XML_REFERENCED_TABLE_NAMESPACE_ATTRIBUTE);
				if (referencedTableNameSpace == null || referencedTableNameSpace.Trim().Length == 0)
					continue;

				referencedTableNameSpace = referencedTableNameSpace.Trim();

				WizardForeignKeyInfo foreignKeyInfo = new DBObjectsForeignKeyInfo(referencedTableNameSpace);

				foreach (XmlNode segmentInfoNode in segmentsList)
				{
					if (segmentInfoNode == null ||
						!(segmentInfoNode is XmlElement) ||
						!((XmlElement)segmentInfoNode).HasAttribute(XML_FOREIGN_KEY_SEG_COLUMN_ATTRIBUTE) ||
						!((XmlElement)segmentInfoNode).HasAttribute(XML_FOREIGN_KEY_SEG_REFERENCED_COLUMN_ATTRIBUTE))
						continue;

					string columnName = ((XmlElement)segmentInfoNode).GetAttribute(XML_FOREIGN_KEY_SEG_COLUMN_ATTRIBUTE);
					string referencedColumnName = ((XmlElement)segmentInfoNode).GetAttribute(XML_FOREIGN_KEY_SEG_REFERENCED_COLUMN_ATTRIBUTE);
					foreignKeyInfo.AddKeySegment(new WizardForeignKeyInfo.KeySegment(columnName, referencedColumnName));
				}

				foreignKeyInfo.ConstraintName = ((XmlElement)foreignKeyNode).GetAttribute(XML_FOREIGN_KEY_CONSTRAINT_NAME_ATTRIBUTE);
				
				string onDel = ((XmlElement)foreignKeyNode).GetAttribute(XML_ON_DELETE_CASCADE_ATTRIBUTE);
				string onUp = ((XmlElement)foreignKeyNode).GetAttribute(XML_ON_UPDATE_CASCADE_ATTRIBUTE);
				foreignKeyInfo.OnDeleteCascade = String.Compare(onDel, Boolean.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0;
				foreignKeyInfo.OnUpdateCascade = String.Compare(onUp, Boolean.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0;
				
				list.Add(foreignKeyInfo);
			}
			
			return list;
		}

		//---------------------------------------------------------------------------
		protected IList<WizardTableIndexInfo> ParseTableIndexInfo(XmlElement aTableInfoNode)
		{	/*
			 <Indexes>
				<IndexInfo name="" table="">
					<Segment name="" />
				</Index_info>
			 </Indexes>
			 */

			if (aTableInfoNode == null)
				return null;

			string tableName = aTableInfoNode.GetAttribute(XML_NAME_ATTRIBUTE);
			if (string.IsNullOrEmpty(tableName))
				return null;

			XmlNode indexesNode = aTableInfoNode.SelectSingleNode("child::" + XML_TAG_INDEXES);

			if (indexesNode == null || !(indexesNode is XmlElement) || !indexesNode.HasChildNodes)
				return null;

			XmlNodeList indexesList = indexesNode.SelectNodes("child::" + XML_TAG_INDEX);
			if (indexesList == null || indexesList.Count == 0)
				return null;

			IList<WizardTableIndexInfo> list = new List<WizardTableIndexInfo>();
			foreach (XmlNode indexNode in indexesList)
			{
				if (indexNode == null)
					continue;

				string name = ((XmlElement)indexNode).GetAttribute(XML_NAME_ATTRIBUTE);
				
				WizardTableIndexInfo index = new WizardTableIndexInfo(name, false);
				index.TableName = ((XmlElement)indexNode).GetAttribute(XML_TABLE_NAME_ATTRIBUTE);
				string uniqueValue = ((XmlElement)indexNode).GetAttribute(XML_INDEX_UNIQUE_ATTRIBUTE);
				index.Unique = String.Compare(uniqueValue, Boolean.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0;
				string nonClusteredValue = ((XmlElement)indexNode).GetAttribute(XML_INDEX_NON_CLUSTERED_ATTRIBUTE);
				index.NonClustered = String.Compare(nonClusteredValue, Boolean.FalseString, StringComparison.InvariantCultureIgnoreCase) != 0;
				
				XmlNodeList segmentList = indexNode.SelectNodes("child::" + XML_TAG_INDEX_SEGMENT);
				if (segmentList == null || segmentList.Count == 0)
					continue;
				
				foreach (XmlNode segNode in segmentList)
				{
					string columnName = ((XmlElement)segNode).GetAttribute(XML_NAME_ATTRIBUTE);
					WizardTableColumnInfo col = new WizardTableColumnInfo(columnName);
					index.AddSegmentInfo(col);
				}
				list.Add(index);
			}

			return list;
		}

		//---------------------------------------------------------------------------
		private bool ParseTablesForeignKeysInfo(System.Xml.XmlElement aApplicationNode)
		{
			if (applicationInfo == null || applicationInfo.ModulesCount == 0 || aApplicationNode == null ||
				String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0)
				return false;
	
			try
			{
				foreach (WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
				{
					if (aModuleInfo.LibrariesCount == 0)
						continue;
					
					XmlNode moduleInfoNode = aApplicationNode.SelectSingleNode("descendant::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE +"='" + aModuleInfo.Name + "']");
					if (moduleInfoNode == null || !(moduleInfoNode is XmlElement))
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo.TablesCount == 0)
							continue;

						XmlNode libraryInfoNode = moduleInfoNode.SelectSingleNode("descendant::" + XML_TAG_LIBRARY + "[@" + XML_NAME_ATTRIBUTE +"='" + aLibraryInfo.Name + "']");
						if (libraryInfoNode == null || !(libraryInfoNode is XmlElement) || !libraryInfoNode.HasChildNodes)
							continue;

						XmlNode tablesInfoNode = libraryInfoNode.SelectSingleNode("child::" + XML_TAG_TABLES);
						if (tablesInfoNode == null || !(tablesInfoNode is XmlElement) || !tablesInfoNode.HasChildNodes)
							continue;

						foreach (WizardTableInfo aTableInfo in aLibraryInfo.TablesInfo)
						{
							XmlNode tableNode = tablesInfoNode.SelectSingleNode("child::" + XML_TAG_TABLE + "[@" + XML_NAME_ATTRIBUTE +"='" + aTableInfo.Name + "']");
							if (tableNode == null || !(tableNode is XmlElement))
								continue;

							XmlNode foreignKeysNode = tableNode.SelectSingleNode("child::" + XML_TAG_FOREIGN_KEYS);
							if (foreignKeysNode == null || !(foreignKeysNode is XmlElement) || !foreignKeysNode.HasChildNodes)
								continue;
						
							XmlNodeList foreignKeysList = foreignKeysNode.SelectNodes("child::" + XML_TAG_FOREIGN_KEY);
							if (foreignKeysList == null || foreignKeysList.Count == 0)
								continue;
							
							foreach(XmlNode foreignKeyNode in foreignKeysList)
							{
								if (foreignKeyNode == null || 
									!((XmlElement)foreignKeyNode).HasAttribute(XML_REFERENCED_TABLE_NAMESPACE_ATTRIBUTE))
									continue;

								XmlNodeList segmentsList = foreignKeyNode.SelectNodes("child::" + XML_TAG_FOREIGN_KEY_SEGMENT);
								if (segmentsList == null || segmentsList.Count == 0)
									continue;

								string referencedTableNameSpace = ((XmlElement)foreignKeyNode).GetAttribute(XML_REFERENCED_TABLE_NAMESPACE_ATTRIBUTE);
								if (referencedTableNameSpace == null || referencedTableNameSpace.Trim().Length == 0)
									continue;

								referencedTableNameSpace = referencedTableNameSpace.Trim();

								NameSpace tmpNameSpace = new NameSpace(referencedTableNameSpace, NameSpaceObjectType.Table);
								if (!tmpNameSpace.IsValid())
									continue;

								WizardTableInfo referencedTableInfo = null;
								
								if (String.Compare(tmpNameSpace.Application, applicationInfo.Name) == 0)
									referencedTableInfo = applicationInfo.GetTableInfoByName(tmpNameSpace.Table);
								else if (referencedApplicationsInfo != null && referencedApplicationsInfo.Count > 0)
								{
									WizardApplicationInfo referencedTableApplication = referencedApplicationsInfo.GetApplicationInfoByName(tmpNameSpace.Application);
									if (referencedTableApplication == null)
										continue;

									referencedTableInfo = referencedTableApplication.GetTableInfoByName(tmpNameSpace.Table);
								}

								if (referencedTableInfo == null)
									continue;

								WizardForeignKeyInfo foreignKeyInfo = new WizardForeignKeyInfo(referencedTableNameSpace);

								foreach(XmlNode segmentInfoNode in segmentsList)
								{
									if (segmentInfoNode == null || !(segmentInfoNode is XmlElement) ||
										!((XmlElement)segmentInfoNode).HasAttribute(XML_FOREIGN_KEY_SEG_COLUMN_ATTRIBUTE) ||
										!((XmlElement)segmentInfoNode).HasAttribute(XML_FOREIGN_KEY_SEG_REFERENCED_COLUMN_ATTRIBUTE))
										continue;

									string columnName = ((XmlElement)segmentInfoNode).GetAttribute(XML_FOREIGN_KEY_SEG_COLUMN_ATTRIBUTE);
									if (string.IsNullOrEmpty(columnName) || aTableInfo.GetColumnInfoByName(columnName) == null)
										continue;

									string referencedColumnName = ((XmlElement)segmentInfoNode).GetAttribute(XML_FOREIGN_KEY_SEG_REFERENCED_COLUMN_ATTRIBUTE);
									if (string.IsNullOrEmpty(referencedColumnName) ||
										referencedTableInfo.GetColumnInfoByName(referencedColumnName) == null)
										continue;
						
									foreignKeyInfo.AddKeySegment(new WizardForeignKeyInfo.KeySegment(columnName, referencedColumnName));
								}

								if (foreignKeyInfo.SegmentsCount == 0)
									continue;

								if (((XmlElement)foreignKeyNode).HasAttribute(XML_FOREIGN_KEY_CONSTRAINT_NAME_ATTRIBUTE))
									foreignKeyInfo.ConstraintName = ((XmlElement)foreignKeyNode).GetAttribute(XML_FOREIGN_KEY_CONSTRAINT_NAME_ATTRIBUTE);

								aTableInfo.AddForeignKeyInfo(foreignKeyInfo);
							}
						}
					}
				}
				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}

		//---------------------------------------------------------------------------
		private bool ParseDBTSlavesRelatedMasterInfo(System.Xml.XmlElement aApplicationNode)
		{
			if (applicationInfo == null || applicationInfo.ModulesCount == 0 || aApplicationNode == null ||
				String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0)
				return false;

			try
			{
				foreach (WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
				{
					if (aModuleInfo.LibrariesCount == 0)
						continue;
					
					XmlNode moduleInfoNode = aApplicationNode.SelectSingleNode("descendant::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE +"='" + aModuleInfo.Name + "']");
					if (moduleInfoNode == null || !(moduleInfoNode is XmlElement))
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo == null || aLibraryInfo.DBTsCount == 0)
							continue;

						XmlNode libraryInfoNode = moduleInfoNode.SelectSingleNode("descendant::" + XML_TAG_LIBRARY + "[@" + XML_NAME_ATTRIBUTE +"='" + aLibraryInfo.Name + "']");
						if (libraryInfoNode == null || !(libraryInfoNode is XmlElement) || !libraryInfoNode.HasChildNodes)
							continue;

						XmlNode dbtsInfoNode = libraryInfoNode.SelectSingleNode("child::" + XML_TAG_DBTS);
						if (dbtsInfoNode == null || !(dbtsInfoNode is XmlElement) || !dbtsInfoNode.HasChildNodes)
							continue;

						foreach (WizardDBTInfo aDBTInfo in aLibraryInfo.DBTsInfo)
						{
							if (!aDBTInfo.IsSlave && !aDBTInfo.IsSlaveBuffered)
								continue;

							XmlNode dbtInfoNode = dbtsInfoNode.SelectSingleNode("child::" + XML_TAG_DBT + "[@" + XML_NAME_ATTRIBUTE +"='" + aDBTInfo.Name + "']");
							if (dbtInfoNode == null || !(dbtInfoNode is XmlElement))
								continue;

							if (aDBTInfo.OnlyForClientDocumentAvailable) // il DBT master va cercato tramite il Server Doc asssociato!
							{
								XmlNode serverDocumentNode = dbtInfoNode.SelectSingleNode("child::" + XML_TAG_SERVER_DOCUMENT);
								if (serverDocumentNode != null && (serverDocumentNode is XmlElement))
								{
									WizardDocumentInfo serverDocumentInfo = GetDocumentInfoFromNamespace(serverDocumentNode.InnerText);
									if (serverDocumentInfo != null)
										aDBTInfo.RelatedDBTMaster = serverDocumentInfo.DBTMaster;
								}
							}
							else
							{
								string relatedDBTMasterName = ((XmlElement)dbtInfoNode).GetAttribute(XML_REL_DBTMASTER_NAME_ATTRIBUTE);
								if (string.IsNullOrEmpty(relatedDBTMasterName))
									continue;

								aDBTInfo.RelatedDBTMaster = aLibraryInfo.GetDBTInfoByName(relatedDBTMasterName, true);
							}

							XmlNode columnsInfoNode = dbtInfoNode.SelectSingleNode("child::" + XML_TAG_COLUMNS);
							if (columnsInfoNode == null || !(columnsInfoNode is XmlElement) || !columnsInfoNode.HasChildNodes)
								continue;

							XmlNodeList columnsList = columnsInfoNode.SelectNodes("child::" + XML_TAG_COLUMN);
							if (columnsList == null || columnsList.Count == 0)
								continue;

							foreach(XmlNode columnInfoNode in columnsList)
							{
								if (columnInfoNode == null || !(columnInfoNode is XmlElement))
									continue;

								string columnName = ((XmlElement)columnInfoNode).GetAttribute(XML_NAME_ATTRIBUTE);
								string foreignKeyRelatedColumnName = ((XmlElement)columnInfoNode).GetAttribute(XML_DBT_COLUMN_RELATEDCOL_ATTRIBUTE);
								if (!string.IsNullOrEmpty(foreignKeyRelatedColumnName))
									aDBTInfo.SetForeignKeySegment(columnName, foreignKeyRelatedColumnName);
							}
						}
					}
				}
				
				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ParseLibrariesDependencies(System.Xml.XmlElement aApplicationNode)
		{
			if (applicationInfo == null || applicationInfo.ModulesCount == 0 || aApplicationNode == null ||
				String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0)
				return false;
			
			try
			{
				foreach (WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
				{
					if (aModuleInfo.LibrariesCount == 0)
						continue;
					
					XmlNode moduleInfoNode = aApplicationNode.SelectSingleNode("descendant::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE +"='" + aModuleInfo.Name + "']");
					if (moduleInfoNode == null || !(moduleInfoNode is XmlElement))
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						XmlNode libraryInfoNode = moduleInfoNode.SelectSingleNode("descendant::" + XML_TAG_LIBRARY + "[@" + XML_NAME_ATTRIBUTE +"='" + aLibraryInfo.Name + "']");
						if (libraryInfoNode == null || !(libraryInfoNode is XmlElement) || !libraryInfoNode.HasChildNodes)
							continue;

						XmlNode dependenciesInfoNode = libraryInfoNode.SelectSingleNode("child::" + XML_TAG_DEPENDENCIES);
						if (dependenciesInfoNode == null || !(dependenciesInfoNode is XmlElement) || !dependenciesInfoNode.HasChildNodes)
							continue;
		
						XmlNodeList dependenciesList = dependenciesInfoNode.SelectNodes("child::" + XML_TAG_DEPENDENCY);
						if (dependenciesList == null || dependenciesList.Count == 0)
							continue;

						foreach(XmlNode dependencyNode in dependenciesList)
						{
							if (dependencyNode == null || !(dependencyNode is XmlElement))
								continue;

							string moduleName = ((XmlElement)dependencyNode).GetAttribute(XML_MODULE_NAME_ATTRIBUTE);
							if (string.IsNullOrEmpty(moduleName))
								continue;

							WizardModuleInfo dependencyModuleInfo = null;
							string applicationName = String.Empty;
							if (((XmlElement)dependencyNode).HasAttribute(XML_APPLICATION_NAME_ATTRIBUTE))
								applicationName = ((XmlElement)dependencyNode).GetAttribute(XML_APPLICATION_NAME_ATTRIBUTE);
							
							if (!string.IsNullOrEmpty(applicationName) && String.Compare(applicationInfo.Name, applicationName) != 0)
							{
								if (referencedApplicationsInfo != null && referencedApplicationsInfo.Count > 0)
								{
									foreach(WizardApplicationInfo aLoadedApplication in referencedApplicationsInfo)
									{
										if (String.Compare(aLoadedApplication.Name, applicationName) == 0)
										{
											dependencyModuleInfo = aLoadedApplication.GetModuleInfoByName(moduleName);
											break;
										}
									}
								}
							}
							else
								dependencyModuleInfo = applicationInfo.GetModuleInfoByName(moduleName);
							
							if (dependencyModuleInfo == null)
								continue;

							string libraryName = ((XmlElement)dependencyNode).GetAttribute(XML_LIBRARY_NAME_ATTRIBUTE);
							if (string.IsNullOrEmpty(libraryName))
								continue;

							WizardLibraryInfo dependencyLibraryInfo = dependencyModuleInfo.GetLibraryInfoByName(libraryName);
							if (dependencyLibraryInfo == null)
								continue;

							aLibraryInfo.AddDependency(dependencyLibraryInfo);
						}
					}
				}
				
				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}

		//---------------------------------------------------------------------------
		private bool ParseDocumentsInfo(System.Xml.XmlElement aApplicationNode)
		{
			if (applicationInfo == null || applicationInfo.ModulesCount == 0 ||
				aApplicationNode == null || String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0)
				return false;
			
			try
			{
				foreach (WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
				{
					if (aModuleInfo.LibrariesCount == 0)
						continue;
					
					XmlNode moduleInfoNode = aApplicationNode.SelectSingleNode("descendant::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE +"='" + aModuleInfo.Name + "']");
					if (moduleInfoNode == null || !(moduleInfoNode is XmlElement))
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						XmlNode libraryInfoNode = moduleInfoNode.SelectSingleNode("descendant::" + XML_TAG_LIBRARY + "[@" + XML_NAME_ATTRIBUTE +"='" + aLibraryInfo.Name + "']");
						if (libraryInfoNode == null || !(libraryInfoNode is XmlElement) || !libraryInfoNode.HasChildNodes)
							continue;

						XmlNode documentsInfoNode = libraryInfoNode.SelectSingleNode("child::" + XML_TAG_DOCUMENTS);
						if (documentsInfoNode == null || !(documentsInfoNode is XmlElement) || !documentsInfoNode.HasChildNodes)
							continue;
		
						XmlNodeList documentsList = documentsInfoNode.SelectNodes("child::" + XML_TAG_DOCUMENT);
						if (documentsList == null || documentsList.Count == 0)
							continue;

						foreach(XmlNode documentNode in documentsList)
						{
							if (documentNode == null || !(documentNode is XmlElement))
								continue;

							string documentName = ((XmlElement)documentNode).GetAttribute(XML_NAME_ATTRIBUTE);
							if (string.IsNullOrEmpty(documentName))
								continue;

							bool isReadOnly = false;
							if (((XmlElement)documentNode).HasAttribute(XML_READONLY_ATTRIBUTE))
							{
								string readOnlyValue = ((XmlElement)documentNode).GetAttribute(XML_READONLY_ATTRIBUTE);
								if (!string.IsNullOrEmpty(readOnlyValue))
									isReadOnly = Boolean.Parse(readOnlyValue);
							}

							string strWidth = string.Empty;
							string strHeight = string.Empty;
							if (((XmlElement)documentNode).HasAttribute(XML_WIDTH_ATTRIBUTE) &&
								((XmlElement)documentNode).HasAttribute(XML_HEIGHT_ATTRIBUTE))
							{
								try
								{
									strWidth = ((XmlElement)documentNode).GetAttribute(XML_WIDTH_ATTRIBUTE);
									strHeight = ((XmlElement)documentNode).GetAttribute(XML_HEIGHT_ATTRIBUTE);
								}
								catch (FormatException)
								{
								}
								catch (OverflowException)
								{
								}
							}

							WizardDocumentInfo documentInfo = new WizardDocumentInfo(documentName, isReadOnly);
							if (documentInfo == null)
								continue;

							if (!string.IsNullOrEmpty(strWidth) &&
								!string.IsNullOrEmpty(strHeight)
								)
							{
								documentInfo.Width = int.Parse(strWidth);
								documentInfo.Height = int.Parse(strHeight);
							}

							if (((XmlElement)documentNode).HasAttribute(XML_DOC_DEFAULT_TYPE_ATTRIBUTE))
							{
								string defaultTypeValue = ((XmlElement)documentNode).GetAttribute(XML_DOC_DEFAULT_TYPE_ATTRIBUTE);
								if (!string.IsNullOrEmpty(defaultTypeValue))
								{
									try
									{
										documentInfo.DefaultType = (WizardDocumentInfo.DocumentType)Enum.Parse(typeof(WizardDocumentInfo.DocumentType), defaultTypeValue, true);
									}
									catch (ArgumentNullException exception)
									{
										Debug.Fail("ArgumentNullException raised during parsing of DefaultType in TBWizardProjectParser.ParseDocumentsInfo." + exception.Message);
									}
									catch (ArgumentException exception)
									{
										Debug.Fail("ArgumentException raised during parsing of DefaultType in TBWizardProjectParser.ParseDocumentsInfo." + exception.Message);
									}
								}
							}

							documentInfo.ClassName = ((XmlElement)documentNode).GetAttribute(XML_CLASS_NAME_ATTRIBUTE);

							// Prima aggiungo il documento alla libreria corrente, altrimenti quando 
							// scorro in seguito i suoi DBT non so dove controllarne l'esistenza e la
							// loro aggiunta al documento fallisce!
							aLibraryInfo.AddDocumentInfo(documentInfo);

							XmlNode titleNode = documentNode.SelectSingleNode("child::" + XML_TAG_DOC_TITLE);
							if (titleNode != null && (titleNode is XmlElement))
							{
								string title = titleNode.InnerText;
								if (!string.IsNullOrEmpty(title))
									documentInfo.Title = title;
							}

							// Labels
							ParseLabelInfo(documentNode, documentInfo.LabelInfoCollection);

							XmlNode dbtsInfoNode = documentNode.SelectSingleNode("child::" + XML_TAG_DBTS);
							if (dbtsInfoNode != null && (dbtsInfoNode is XmlElement) && dbtsInfoNode.HasChildNodes)
							{
								XmlNodeList dbtsList = dbtsInfoNode.SelectNodes("child::" + XML_TAG_DBT);
								if (dbtsList != null && dbtsList.Count > 0)
								{
									foreach (XmlNode dbtNode in dbtsList)
									{
										if (dbtNode == null || !(dbtNode is XmlElement))
											continue;

										string dbtName = ((XmlElement)dbtNode).GetAttribute(XML_NAME_ATTRIBUTE);
										if (!string.IsNullOrEmpty(dbtName))
										{
											WizardDBTInfo aDBTInfo = aLibraryInfo.GetDBTInfoByName(dbtName, true);
											if (aDBTInfo == null) // il DBT non è stato trovato fra quelli della libreria e nemmeno nelle dipendenze
												continue;

											documentInfo.AddDBTInfo(aDBTInfo);
										}
									}
								}
							}

							WizardDBTInfo masterInfo = documentInfo.DBTMaster;
							if (masterInfo != null)
							{
								XmlNode hotLinkNode = documentNode.SelectSingleNode("child::" + XML_TAG_DOC_HOTLINK);
								if (hotLinkNode != null && (hotLinkNode is XmlElement))
								{
									string codeColumnName = ((XmlElement)hotLinkNode).GetAttribute(XML_DOC_HOTLINK_CODE_ATTRIBUTE);
									if (!string.IsNullOrEmpty(codeColumnName))
									{
										WizardTableColumnInfo codeColumn = masterInfo.GetTableColumnInfoByName(codeColumnName);
										if (codeColumn != null)
										{
											documentInfo.HKLCodeColumn = codeColumn;
											string descriptionColumnName = ((XmlElement)hotLinkNode).GetAttribute(XML_DOC_HOTLINK_DESCR_ATTRIBUTE);
											if (!string.IsNullOrEmpty(descriptionColumnName))
											{
												WizardTableColumnInfo descriptionColumn = masterInfo.GetTableColumnInfoByName(descriptionColumnName);
												if (descriptionColumn != null)
													documentInfo.HKLDescriptionColumn = descriptionColumn;
											}

											documentInfo.HKLName = ((XmlElement)hotLinkNode).GetAttribute(XML_NAME_ATTRIBUTE);
											documentInfo.HKLClassName = ((XmlElement)hotLinkNode).GetAttribute(XML_CLASS_NAME_ATTRIBUTE);
										}
									}
								}
							}

							XmlNode tabbedPanesInfoNode = documentNode.SelectSingleNode("child::" + XML_TAG_DOC_TABBEDPANES);
							if (tabbedPanesInfoNode != null && (tabbedPanesInfoNode is XmlElement) && tabbedPanesInfoNode.HasChildNodes)
							{
								// Load tabber size	set
								string tabbedLeft = ((XmlElement)tabbedPanesInfoNode).GetAttribute(XML_LEFT_ATTRIBUTE);
								string tabbedTop = ((XmlElement)tabbedPanesInfoNode).GetAttribute(XML_TOP_ATTRIBUTE);
								string tabbedHeight = ((XmlElement)tabbedPanesInfoNode).GetAttribute(XML_HEIGHT_ATTRIBUTE);
								string tabbedWidth = ((XmlElement)tabbedPanesInfoNode).GetAttribute(XML_WIDTH_ATTRIBUTE);

								if (!string.IsNullOrEmpty(tabbedLeft) && !string.IsNullOrEmpty(tabbedTop) &&
									 !string.IsNullOrEmpty(tabbedHeight) && !string.IsNullOrEmpty(tabbedWidth))
								{
									try
									{
										documentInfo.TabberSize.SetPosition(int.Parse(tabbedLeft), int.Parse(tabbedTop),
											int.Parse(tabbedWidth), int.Parse(tabbedHeight));
									}
									catch (FormatException)
									{
									}
									catch (OverflowException)
									{
									}
								}

								XmlNodeList tabbedPanesList = tabbedPanesInfoNode.SelectNodes("child::" + XML_TAG_DOC_TABBEDPANE);
								if (tabbedPanesList != null && tabbedPanesList.Count > 0)
								{
									// Se esiste il nodo contenente la lista delle tabbed pane non devo
									// considerare le schede aggiunte di default all'atto dell'inserimento
									// dei DBT gestiti dal documento
									documentInfo.RemoveAllTabbedPanes();

									foreach (XmlNode tabbedPaneNode in tabbedPanesList)
									{
										if (tabbedPaneNode == null || !(tabbedPaneNode is XmlElement))
											continue;

										WizardDocumentTabbedPaneInfo tabbedPane = ParseTabbedPaneInfo(aLibraryInfo, (XmlElement)tabbedPaneNode);
										if (tabbedPane != null)
											documentInfo.AddTabbedPane(tabbedPane);

										// Labels
										ParseLabelInfo(tabbedPaneNode, tabbedPane.LabelInfoCollection);
									}
								}
							}
						}
					}
				}
				
				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}


		//---------------------------------------------------------------------------
		private void ParseLabelInfo(XmlNode documentNode, List<LabelInfo> labelInfoList)
		{
			XmlNode labelsNode = documentNode.SelectSingleNode("child::" + XML_TAG_LABELS);
			if (labelsNode != null && (labelsNode is XmlElement) && labelsNode.HasChildNodes)
			{
				XmlNodeList labelList = labelsNode.SelectNodes("child::" + XML_TAG_LABEL);
				if (labelList != null && labelList.Count > 0)
				{
					foreach (XmlNode labelNode in labelList)
					{
						if (labelNode == null || !(labelNode is XmlElement))
							continue;

						string labelText = ((XmlElement)labelNode).GetAttribute(XML_TEXT_ATTRIBUTE);
						string labelLeft = ((XmlElement)labelNode).GetAttribute(XML_LEFT_ATTRIBUTE);
						string labelTop = ((XmlElement)labelNode).GetAttribute(XML_TOP_ATTRIBUTE);
						string labelHeight = ((XmlElement)labelNode).GetAttribute(XML_HEIGHT_ATTRIBUTE);
						string labelWidth = ((XmlElement)labelNode).GetAttribute(XML_WIDTH_ATTRIBUTE);

						if (!string.IsNullOrEmpty(labelText) && !string.IsNullOrEmpty(labelLeft) &&
							!string.IsNullOrEmpty(labelTop) && !string.IsNullOrEmpty(labelWidth) && !string.IsNullOrEmpty(labelHeight))
						{
							try
							{
								LabelInfo labelInfo = new LabelInfo(labelText, int.Parse(labelLeft), int.Parse(labelTop), int.Parse(labelWidth), int.Parse(labelHeight));
								labelInfoList.Add(labelInfo);
							}
							catch (FormatException)
							{
							}
							catch (OverflowException)
							{
							}
						}
					}
				}
			}
		}

		//---------------------------------------------------------------------------
		private WizardDocumentTabbedPaneInfo ParseTabbedPaneInfo(WizardLibraryInfo aLibraryInfo, System.Xml.XmlElement aTabbedPaneElement)
		{
			if (aLibraryInfo == null || aTabbedPaneElement == null || 
				String.Compare(aTabbedPaneElement.Name, XML_TAG_DOC_TABBEDPANE) != 0)
				return null;

			try
			{										
				string dbtName = aTabbedPaneElement.GetAttribute(XML_TABBED_PANE_DBT_NAME_ATTRIBUTE);
				if (string.IsNullOrEmpty(dbtName))
					return null;

				WizardDBTInfo aDBTInfo = aLibraryInfo.GetDBTInfoByName(dbtName, true);
				if (aDBTInfo == null) // il DBT non è stato trovato fra quelli visti dalla libreria
					return null;
								
				bool isReadOnly = false;
				if (aTabbedPaneElement.HasAttribute(XML_READONLY_ATTRIBUTE))
				{
					string readOnlyValue = aTabbedPaneElement.GetAttribute(XML_READONLY_ATTRIBUTE);
					if (!string.IsNullOrEmpty(readOnlyValue))
						isReadOnly = Boolean.Parse(readOnlyValue);
				}
				
				WizardDocumentTabbedPaneInfo tabbedPane = new WizardDocumentTabbedPaneInfo(aDBTInfo, isReadOnly);

				XmlNode tabbedPaneTitleNode = aTabbedPaneElement.SelectSingleNode("child::" + XML_TAG_TABBED_PANE_TITLE);
				if (tabbedPaneTitleNode != null && (tabbedPaneTitleNode is XmlElement))
				{
					string title = tabbedPaneTitleNode.InnerText;
					if (!string.IsNullOrEmpty(title))
						tabbedPane.Title = title;
				}
								
				// Se il nodo riferito al tabbed pane non contiene la specifica
				// delle colonne da gestire vuol dire che è direttamente riferito
				// al DBT
				XmlNode columnsInfoNode = aTabbedPaneElement.SelectSingleNode("child::" + XML_TAG_COLUMNS);
				if (columnsInfoNode == null || !(columnsInfoNode is XmlElement) || !columnsInfoNode.HasChildNodes)
					return tabbedPane;

				XmlNodeList columnsList = columnsInfoNode.SelectNodes("child::" + XML_TAG_COLUMN);
				if (columnsList == null || columnsList.Count == 0)
					return tabbedPane;
			
				foreach(XmlNode columnInfoNode in columnsList)
				{
					if (columnInfoNode == null || !(columnInfoNode is XmlElement))
						continue;

					WizardDBTColumnInfo managedColumnInfo = ParseDBTColumnInfoNode(aDBTInfo, (XmlElement)columnInfoNode);
					if (managedColumnInfo != null)
						tabbedPane.AddManagedColumn(managedColumnInfo);
				}
				return tabbedPane;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
			catch(XmlException exception)
			{
 				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
	 	
		//---------------------------------------------------------------------------
		private bool ParseDBTsHotLinks(System.Xml.XmlElement aApplicationNode)
		{
			if (applicationInfo == null || applicationInfo.ModulesCount == 0 ||
				aApplicationNode == null || String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0)
				return false;
			
			try
			{
				foreach (WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
				{
					if (aModuleInfo.LibrariesCount == 0)
						continue;
					
					XmlNode moduleInfoNode = aApplicationNode.SelectSingleNode("descendant::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE +"='" + aModuleInfo.Name + "']");
					if (moduleInfoNode == null || !(moduleInfoNode is XmlElement))
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo == null || aLibraryInfo.DBTsCount == 0)
							continue;

						XmlNode libraryInfoNode = moduleInfoNode.SelectSingleNode("descendant::" + XML_TAG_LIBRARY + "[@" + XML_NAME_ATTRIBUTE +"='" + aLibraryInfo.Name + "']");
						if (libraryInfoNode == null || !(libraryInfoNode is XmlElement) || !libraryInfoNode.HasChildNodes)
							continue;

						XmlNode dbtsInfoNode = libraryInfoNode.SelectSingleNode("child::" + XML_TAG_DBTS);
						if (dbtsInfoNode == null || !(dbtsInfoNode is XmlElement) || !dbtsInfoNode.HasChildNodes)
							continue;

						foreach (WizardDBTInfo aDBTInfo in aLibraryInfo.DBTsInfo)
						{
							XmlNode dbtInfoNode = dbtsInfoNode.SelectSingleNode("child::" + XML_TAG_DBT + "[@" + XML_NAME_ATTRIBUTE +"='" + aDBTInfo.Name + "']");
							if (dbtInfoNode == null || !(dbtInfoNode is XmlElement))
								continue;

							XmlNode columnsInfoNode = dbtInfoNode.SelectSingleNode("child::" + XML_TAG_COLUMNS);
							if (columnsInfoNode == null || !(columnsInfoNode is XmlElement) || !columnsInfoNode.HasChildNodes)
								continue;

							XmlNodeList columnsList = columnsInfoNode.SelectNodes("child::" + XML_TAG_COLUMN);
							if (columnsList == null || columnsList.Count == 0)
								continue;

							foreach(XmlNode columnInfoNode in columnsList)
							{
								if (columnInfoNode == null || !(columnInfoNode is XmlElement))
									continue;

								string columnName = ((XmlElement)columnInfoNode).GetAttribute(XML_NAME_ATTRIBUTE);

								if (((XmlElement)columnInfoNode).HasAttribute(XML_DBT_COLUMN_HKL_CLASS_ATTRIBUTE))
								{
									string hotKeyLinkClassName = ((XmlElement)columnInfoNode).GetAttribute(XML_DBT_COLUMN_HKL_CLASS_ATTRIBUTE);
									if (!string.IsNullOrEmpty(hotKeyLinkClassName))
									{
										bool showHotKeyLinkDescription = false;
										if (((XmlElement)columnInfoNode).HasAttribute(XML_DBT_COLUMN_HKL_SHOW_DESCRIPTION_ATTRIBUTE))
										{
											string showHotKeyLinkDescriptionValue = ((XmlElement)columnInfoNode).GetAttribute(XML_DBT_COLUMN_HKL_SHOW_DESCRIPTION_ATTRIBUTE);
											if (!string.IsNullOrEmpty(showHotKeyLinkDescriptionValue))
												showHotKeyLinkDescription = Boolean.Parse(showHotKeyLinkDescriptionValue);
										}

										WizardHotKeyLinkInfo hotKeyLinkInfo = aLibraryInfo.GetHotKeyLinkFromClassName(hotKeyLinkClassName);
										if (hotKeyLinkInfo != null)
											aDBTInfo.SetHotKeyLink(columnName, hotKeyLinkInfo, showHotKeyLinkDescription);
									}
								}
							}
						}
					}
				}
				
				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ParseTabbedPanesHotLinks(System.Xml.XmlElement aApplicationNode)
		{
			if (applicationInfo == null || applicationInfo.ModulesCount == 0 ||
				aApplicationNode == null || String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0)
				return false;
			
			try
			{
				foreach (WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
				{
					if (aModuleInfo.LibrariesCount == 0)
						continue;
					
					XmlNode moduleInfoNode = aApplicationNode.SelectSingleNode("descendant::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE +"='" + aModuleInfo.Name + "']");
					if (moduleInfoNode == null || !(moduleInfoNode is XmlElement))
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo == null || aLibraryInfo.DocumentsCount == 0)
							continue;

						XmlNode libraryInfoNode = moduleInfoNode.SelectSingleNode("descendant::" + XML_TAG_LIBRARY + "[@" + XML_NAME_ATTRIBUTE +"='" + aLibraryInfo.Name + "']");
						if (libraryInfoNode == null || !(libraryInfoNode is XmlElement) || !libraryInfoNode.HasChildNodes)
							continue;

						XmlNode documentsInfoNode = libraryInfoNode.SelectSingleNode("child::" + XML_TAG_DOCUMENTS);
						if (documentsInfoNode == null || !(documentsInfoNode is XmlElement) || !documentsInfoNode.HasChildNodes)
							continue;

						foreach (WizardDocumentInfo aDocumentInfo in aLibraryInfo.DocumentsInfo)
						{
							if (aDocumentInfo == null || aDocumentInfo.TabbedPanesCount == 0)
								continue;

							XmlNode documentInfoNode = documentsInfoNode.SelectSingleNode("child::" + XML_TAG_DOCUMENT + "[@" + XML_NAME_ATTRIBUTE +"='" + aDocumentInfo.Name + "']");
							if (documentInfoNode == null || !(documentInfoNode is XmlElement))
								continue;

							XmlNode tabbedPanesInfoNode = documentInfoNode.SelectSingleNode("child::" + XML_TAG_DOC_TABBEDPANES);
							if (tabbedPanesInfoNode == null || !(tabbedPanesInfoNode is XmlElement) || !tabbedPanesInfoNode.HasChildNodes)
								continue;
							
							foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in aDocumentInfo.TabbedPanes)
							{
								XmlNode tabbedPaneNode = tabbedPanesInfoNode.SelectSingleNode("child::" + XML_TAG_DOC_TABBEDPANE + "[@" + XML_TABBED_PANE_DBT_NAME_ATTRIBUTE +"='" + aTabbedPaneInfo.DBTInfo.Name + "' and " + XML_TAG_TABBED_PANE_TITLE + "='" + aTabbedPaneInfo.Title + "']");
								if (tabbedPaneNode == null || !(tabbedPaneNode is XmlElement))
									continue;

								XmlNode columnsInfoNode = tabbedPaneNode.SelectSingleNode("child::" + XML_TAG_COLUMNS);
								if (columnsInfoNode == null || !(columnsInfoNode is XmlElement) || !columnsInfoNode.HasChildNodes)
									continue;
							
								XmlNodeList columnsList = columnsInfoNode.SelectNodes("child::" + XML_TAG_COLUMN);
								if (columnsList == null || columnsList.Count == 0)
									continue;

								foreach(XmlNode columnInfoNode in columnsList)
								{
									if (columnInfoNode == null || !(columnInfoNode is XmlElement))
										continue;

									string columnName = ((XmlElement)columnInfoNode).GetAttribute(XML_NAME_ATTRIBUTE);

									if (((XmlElement)columnInfoNode).HasAttribute(XML_DBT_COLUMN_HKL_CLASS_ATTRIBUTE))
									{
										string hotKeyLinkClassName = ((XmlElement)columnInfoNode).GetAttribute(XML_DBT_COLUMN_HKL_CLASS_ATTRIBUTE);
										if (!string.IsNullOrEmpty(hotKeyLinkClassName))
										{
											bool showHotKeyLinkDescription = false;
											if (((XmlElement)columnInfoNode).HasAttribute(XML_DBT_COLUMN_HKL_SHOW_DESCRIPTION_ATTRIBUTE))
											{
												string showHotKeyLinkDescriptionValue = ((XmlElement)columnInfoNode).GetAttribute(XML_DBT_COLUMN_HKL_SHOW_DESCRIPTION_ATTRIBUTE);
												if (!string.IsNullOrEmpty(showHotKeyLinkDescriptionValue))
													showHotKeyLinkDescription = Boolean.Parse(showHotKeyLinkDescriptionValue);
											}

											WizardHotKeyLinkInfo hotKeyLinkInfo = aLibraryInfo.GetHotKeyLinkFromClassName(hotKeyLinkClassName);
											if (hotKeyLinkInfo != null)
												aTabbedPaneInfo.SetHotKeyLink(columnName, hotKeyLinkInfo, showHotKeyLinkDescription);
										}
									}
								}
							}
						}
					}
				}
				
				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}

		//---------------------------------------------------------------------------
		private bool ParseClientDocumentsInfo(System.Xml.XmlElement aApplicationNode)
		{
			if 
				(
				applicationInfo == null ||
				applicationInfo.ModulesCount == 0 ||
				(
				!applicationInfo.HasDocuments && 
				(referencedApplicationsInfo == null || referencedApplicationsInfo.Count == 0)
				)||
				aApplicationNode == null ||
				String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0
				)
				return false;
			
			try
			{
				foreach (WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
				{
					if (aModuleInfo.LibrariesCount == 0)
						continue;
					
					XmlNode moduleInfoNode = aApplicationNode.SelectSingleNode("descendant::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE +"='" + aModuleInfo.Name + "']");
					if (moduleInfoNode == null || !(moduleInfoNode is XmlElement))
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						XmlNode libraryInfoNode = moduleInfoNode.SelectSingleNode("descendant::" + XML_TAG_LIBRARY + "[@" + XML_NAME_ATTRIBUTE +"='" + aLibraryInfo.Name + "']");
						if (libraryInfoNode == null || !(libraryInfoNode is XmlElement) || !libraryInfoNode.HasChildNodes)
							continue;

						XmlNode clientDocumentsNode = libraryInfoNode.SelectSingleNode("child::" + XML_TAG_CLIENT_DOCUMENTS);
						if (clientDocumentsNode == null || !(clientDocumentsNode is XmlElement) || !clientDocumentsNode.HasChildNodes)
							continue;

						XmlNodeList clientDocumentsList = clientDocumentsNode.SelectNodes("child::" + XML_TAG_CLIENT_DOCUMENT);
						if (clientDocumentsList == null || clientDocumentsList.Count == 0)
							continue;

						foreach(XmlNode clientDocumentNode in clientDocumentsList)
						{
							if (clientDocumentNode == null || !(clientDocumentNode is XmlElement))
								continue;

							XmlNode serverDocumentNode = clientDocumentNode.SelectSingleNode("child::" + XML_TAG_SERVER_DOCUMENT);
							if (serverDocumentNode == null || !(serverDocumentNode is XmlElement) || 
								serverDocumentNode.InnerText == null || serverDocumentNode.InnerText.Length == 0)
								continue;

							// Devo ritrovare, a partire dal suo namespace, il documento che fa da server fra quelli 
							// appartenenti alle applicazioni referenziate.
							WizardDocumentInfo serverDocumentInfo = GetDocumentInfoFromNamespace(serverDocumentNode.InnerText);
							if (serverDocumentInfo == null)
								continue;
							
							string clientDocumentName = ((XmlElement)clientDocumentNode).GetAttribute(XML_NAME_ATTRIBUTE);

							bool isReadOnly = false;
							if (((XmlElement)clientDocumentNode).HasAttribute(XML_READONLY_ATTRIBUTE))
							{
								string readOnlyValue = ((XmlElement)clientDocumentNode).GetAttribute(XML_READONLY_ATTRIBUTE);
								if (!string.IsNullOrEmpty(readOnlyValue))
									isReadOnly = Boolean.Parse(readOnlyValue);
							}

							WizardClientDocumentInfo clientDocumentInfo = new WizardClientDocumentInfo(clientDocumentName, serverDocumentInfo, isReadOnly);
							if (clientDocumentInfo == null)
								continue;

							clientDocumentInfo.ClassName = ((XmlElement)clientDocumentNode).GetAttribute(XML_CLASS_NAME_ATTRIBUTE);

							XmlNode titleNode = clientDocumentNode.SelectSingleNode("child::" + XML_TAG_DOC_TITLE);
							if (titleNode != null && (titleNode is XmlElement))
							{
								string title = titleNode.InnerText;
								if (!string.IsNullOrEmpty(title))
									clientDocumentInfo.Title = title;
							}

							if (((XmlElement)serverDocumentNode).HasAttribute(XML_SERVER_DOC_FAMILY_ATTRIBUTE))
							{
								string familyClass = ((XmlElement)serverDocumentNode).GetAttribute(XML_SERVER_DOC_FAMILY_ATTRIBUTE);
								if (!string.IsNullOrEmpty(familyClass))
								{
									clientDocumentInfo.FamilyToAttachClassName = familyClass;
									if (((XmlElement)serverDocumentNode).HasAttribute(XML_SERVER_DOC_EXCLUDE_BATCH_ATTRIBUTE))
									{
										string excludeBatchModeValue = ((XmlElement)serverDocumentNode).GetAttribute(XML_SERVER_DOC_EXCLUDE_BATCH_ATTRIBUTE);
										if (!string.IsNullOrEmpty(excludeBatchModeValue))
											clientDocumentInfo.ExcludeBatchMode = Boolean.Parse(excludeBatchModeValue);
									}
								}
							}
							
							if (((XmlElement)clientDocumentNode).HasAttribute(XML_CLIENT_DOC_SLAVEFORMVIEW_ATTRIBUTE))
							{
								string createSlaveFormViewValue = ((XmlElement)clientDocumentNode).GetAttribute(XML_CLIENT_DOC_SLAVEFORMVIEW_ATTRIBUTE);
								if (!string.IsNullOrEmpty(createSlaveFormViewValue))
									clientDocumentInfo.CreateSlaveFormView = Boolean.Parse(createSlaveFormViewValue);
							}
							
							if (((XmlElement)clientDocumentNode).HasAttribute(XML_CLIENT_DOC_ADDTABDIALOGS_ATTRIBUTE))
							{
								string addTabDialogsValue = ((XmlElement)clientDocumentNode).GetAttribute(XML_CLIENT_DOC_ADDTABDIALOGS_ATTRIBUTE);
								if (!string.IsNullOrEmpty(addTabDialogsValue))
									clientDocumentInfo.AddTabDialogs = Boolean.Parse(addTabDialogsValue);
							}
							
							if (((XmlElement)clientDocumentNode).HasAttribute(XML_SERVER_DOC_NO_UNATTENDED_ATTRIBUTE))
							{
								string noUnattendedModeValue = ((XmlElement)clientDocumentNode).GetAttribute(XML_SERVER_DOC_NO_UNATTENDED_ATTRIBUTE);
								if (!string.IsNullOrEmpty(noUnattendedModeValue))
									clientDocumentInfo.ExcludeUnattendedMode = Boolean.Parse(noUnattendedModeValue);
							}

							aLibraryInfo.AddClientDocumentInfo(clientDocumentInfo);
							
							XmlNode includeFilesNode = clientDocumentNode.SelectSingleNode("child::" + XML_TAG_INCLUDE_FILES);
							if (includeFilesNode != null && (includeFilesNode is XmlElement) && includeFilesNode.HasChildNodes)
							{
								XmlNodeList includeFilesList = includeFilesNode.SelectNodes("child::" + XML_TAG_INCLUDE_FILE);
								if (includeFilesList != null && includeFilesList.Count > 0)
								{
									foreach(XmlNode includeFileNode in includeFilesList)
									{
										if (includeFileNode == null || !(includeFileNode is XmlElement) || includeFileNode.InnerText == null)
											continue;
										
										string relativeFileName = includeFileNode.InnerText.Trim();
										if (string.IsNullOrEmpty(relativeFileName))
											continue;
										
										clientDocumentInfo.AddServerHeaderFile(Generics.BuildFullPath(WizardCodeGenerator.GetStandardLibraryPath(aLibraryInfo), relativeFileName));
									}
								}
							}
							
							XmlNode dbtsInfoNode = clientDocumentNode.SelectSingleNode("child::" + XML_TAG_DBTS);
							if (dbtsInfoNode != null && (dbtsInfoNode is XmlElement) && dbtsInfoNode.HasChildNodes)
							{							
								XmlNodeList dbtsList = dbtsInfoNode.SelectNodes("child::" + XML_TAG_DBT);
								if (dbtsList != null && dbtsList.Count > 0)
								{
									foreach(XmlNode dbtNode in dbtsList)
									{
										if (dbtNode == null || !(dbtNode is XmlElement))
											continue;
										
										string dbtName = ((XmlElement)dbtNode).GetAttribute(XML_NAME_ATTRIBUTE);
										if (!string.IsNullOrEmpty(dbtName))
										{
											WizardDBTInfo aDBTInfo = aLibraryInfo.GetDBTInfoByName(dbtName, true);
											if (aDBTInfo == null) // il DBT non è stato trovato fra quelli della libreria e nemmeno nelle dipendenze
												continue;
											
											clientDocumentInfo.AddDBTInfo(aDBTInfo);
										}
									}
								}
							}

							XmlNode tabbedPanesInfoNode = clientDocumentNode.SelectSingleNode("child::" + XML_TAG_DOC_TABBEDPANES);
							if (tabbedPanesInfoNode != null && (tabbedPanesInfoNode is XmlElement) && tabbedPanesInfoNode.HasChildNodes)
							{
								XmlNodeList tabbedPanesList = tabbedPanesInfoNode.SelectNodes("child::" + XML_TAG_DOC_TABBEDPANE);
								if (tabbedPanesList != null && tabbedPanesList.Count > 0)
								{
									// Se esiste il nodo contenente la lista delle tabbed pane non devo
									// considerare le schede aggiunte di default all'atto dell'inserimento
									// dei DBT gestiti dal client document
									clientDocumentInfo.RemoveAllTabbedPanes();

									foreach(XmlNode tabbedPaneNode in tabbedPanesList)
									{
										if (tabbedPaneNode == null || !(tabbedPaneNode is XmlElement))
											continue;

										WizardDocumentTabbedPaneInfo tabbedPane = ParseTabbedPaneInfo(aLibraryInfo, (XmlElement)tabbedPaneNode);
										if (tabbedPane != null)
											clientDocumentInfo.AddTabbedPane(tabbedPane);
									}
								}
							}

						}
					}
				}
				
				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ParseExtraAddedColumnsInfo(System.Xml.XmlElement aApplicationNode)
		{
			if 
				(
				applicationInfo == null ||
				applicationInfo.ModulesCount == 0 ||
				(
				!applicationInfo.HasTables && 
				(referencedApplicationsInfo == null || referencedApplicationsInfo.Count == 0)
				)||
				aApplicationNode == null ||
				String.Compare(aApplicationNode.Name, XML_TAG_APPLICATION) != 0
				)
				return false;
			
			try
			{
				foreach (WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
				{
					if (aModuleInfo.LibrariesCount == 0)
						continue;

					XmlNode moduleInfoNode = aApplicationNode.SelectSingleNode("descendant::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE + "='" + aModuleInfo.Name + "']");
					if (moduleInfoNode == null || !(moduleInfoNode is XmlElement))
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						XmlNode libraryInfoNode = moduleInfoNode.SelectSingleNode("descendant::" + XML_TAG_LIBRARY + "[@" + XML_NAME_ATTRIBUTE + "='" + aLibraryInfo.Name + "']");
						if (libraryInfoNode == null || !(libraryInfoNode is XmlElement) || !libraryInfoNode.HasChildNodes)
							continue;

						XmlNode extraAddedColumnsNode = libraryInfoNode.SelectSingleNode("child::" + XML_TAG_EXTRA_ADDED_COLUMNS);
						if (extraAddedColumnsNode == null || !(extraAddedColumnsNode is XmlElement) || !extraAddedColumnsNode.HasChildNodes)
							continue;

						XmlNodeList extraAddedColumnsList = extraAddedColumnsNode.SelectNodes("child::" + XML_TAG_EXTRA_ADDED_COLUMN);
						if (extraAddedColumnsList == null || extraAddedColumnsList.Count == 0)
							continue;

						foreach (XmlNode extraAddedColumnNode in extraAddedColumnsList)
						{
							WizardExtraAddedColumnsInfo extraAddedColumnInfo = ParseExtraAddedColumnInfo(libraryInfoNode);

							WizardExtraAddedColumnsInfo addedExtraAddedColumnsInfo = aLibraryInfo.AddExtraAddedColumnsInfo(extraAddedColumnInfo);
							if (addedExtraAddedColumnsInfo == null)
								continue;

							WizardTableInfo originalTableInfo = addedExtraAddedColumnsInfo.GetOriginalTableInfo();
							if (originalTableInfo != null && originalTableInfo.IsReferenced &&
								((XmlElement)extraAddedColumnNode).HasAttribute(XML_REFERENCED_TABLE_HEADER_ATTRIBUTE))
							{
								string relativeFileName = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_REFERENCED_TABLE_HEADER_ATTRIBUTE);
								if (relativeFileName != null && relativeFileName.Trim().Length > 0)
									addedExtraAddedColumnsInfo.ReferencedTableIncludeFile = Generics.BuildFullPath(WizardCodeGenerator.GetStandardLibraryPath(aLibraryInfo), relativeFileName);
							}
						}
					}
				}
				
				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}

		//---------------------------------------------------------------------------
		protected virtual WizardExtraAddedColumnsInfo ParseExtraAddedColumnInfo(System.Xml.XmlNode extraAddedColumnNode)
		{
			if (extraAddedColumnNode == null || !(extraAddedColumnNode is XmlElement))
				return null;

			string tableNameSpace = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_TABLE_NAMESPACE_ATTRIBUTE);
			if (string.IsNullOrEmpty(tableNameSpace))
				return null;

			XmlNode columnsInfoNode = extraAddedColumnNode.SelectSingleNode("child::" + XML_TAG_COLUMNS);
			if (columnsInfoNode == null || !(columnsInfoNode is XmlElement) || !columnsInfoNode.HasChildNodes)
				return null;

			XmlNodeList columnsList = columnsInfoNode.SelectNodes("child::" + XML_TAG_COLUMN);
			if (columnsList == null || columnsList.Count == 0)
				return null;

			bool isReadOnly = false;

			if (((XmlElement)extraAddedColumnNode).HasAttribute(XML_READONLY_ATTRIBUTE))
			{
				string readOnlyValue = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_READONLY_ATTRIBUTE);
				if (!string.IsNullOrEmpty(readOnlyValue))
					isReadOnly = Boolean.Parse(readOnlyValue);
			}

            WizardExtraAddedColumnsInfo extraAddedColumnInfo = new WizardExtraAddedColumnsInfo(tableNameSpace, isReadOnly);

			if (extraAddedColumnInfo == null)
				return null;

			extraAddedColumnInfo.ClassName = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_CLASS_NAME_ATTRIBUTE);

			foreach (XmlNode columnInfoNode in columnsList)
			{
				if (columnInfoNode == null || !(columnInfoNode is XmlElement))
					continue;

				WizardTableColumnInfo parsedColumnInfo = ParseBaseColumnInfoNode(extraAddedColumnInfo.TableName, (XmlElement)columnInfoNode);
				if (parsedColumnInfo == null)
					continue;

				extraAddedColumnInfo.AddColumnInfo(parsedColumnInfo);
			}

			string dbReleaseNumberText = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_DB_RELEASE_NUMBER_ATTRIBUTE);
			if (string.IsNullOrEmpty(dbReleaseNumberText))
				dbReleaseNumberText = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE);

			uint dbReleaseNumber = 0;
			if (!string.IsNullOrEmpty(dbReleaseNumberText))
				UInt32.TryParse(dbReleaseNumberText, out dbReleaseNumber);

			extraAddedColumnInfo.SetCreationDbReleaseNumber((dbReleaseNumber > 0) ? dbReleaseNumber : 1);

			ParseAdditionalColumnsHistoryInfo(extraAddedColumnInfo, (XmlElement)extraAddedColumnNode);

			return extraAddedColumnInfo;
		}

		// TODO: gestire tutti i tipi di colonna mancanti (IDENTITY, espressioni nei valori di DEFAULT, etc.)
		//---------------------------------------------------------------------------
		private bool ParseAdditionalColumnsHistoryInfo(WizardExtraAddedColumnsInfo aExtraAddedColumnInfo, System.Xml.XmlElement aExtraAddedColumnInfoNode)
		{
			if (aExtraAddedColumnInfo == null || aExtraAddedColumnInfoNode == null ||
				String.Compare(aExtraAddedColumnInfoNode.Name, XML_TAG_EXTRA_ADDED_COLUMN) != 0)
				return false;
			
			try
			{
				XmlNode historyInfoNode = aExtraAddedColumnInfoNode.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY);
				if (historyInfoNode == null || !(historyInfoNode is XmlElement) || !historyInfoNode.HasChildNodes)
					return false;

				XmlNodeList stepsList = historyInfoNode.SelectNodes("child::" + XML_TAG_TABLE_HISTORY_STEP);
				if (stepsList == null || stepsList.Count == 0)
					return false;

				foreach(XmlNode stepNode in stepsList)
				{
					if (stepNode == null || !(stepNode is XmlElement) || !stepNode.HasChildNodes)
						continue;

					string dbReleaseNumberText = ((XmlElement)stepNode).GetAttribute(XML_DB_RELEASE_NUMBER_ATTRIBUTE);
					if (string.IsNullOrEmpty(dbReleaseNumberText))
						dbReleaseNumberText = ((XmlElement)stepNode).GetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE);

					uint dbReleaseNumber = 0;
					if (!string.IsNullOrEmpty(dbReleaseNumberText))
						UInt32.TryParse(dbReleaseNumberText, out dbReleaseNumber);
					
					if (dbReleaseNumber <= 0)
						continue;

					XmlNode eventsInfoNode = stepNode.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY_EVENTS);
					if (eventsInfoNode == null || !(eventsInfoNode is XmlElement) || !eventsInfoNode.HasChildNodes)
						continue; // non ci sono eventi!
					
					XmlNodeList columnEventsList = eventsInfoNode.SelectNodes("child::" + XML_TAG_TABLE_HISTORY_COLUMN_EVENT);
					if (columnEventsList == null || columnEventsList.Count == 0)
						continue; // non ci sono eventi!

					TableHistoryStep stepInfo = new TableHistoryStep(dbReleaseNumber);

					// Caricamento delle informazioni relative alle modifiche sulle colonne
					if (columnEventsList != null && columnEventsList.Count > 0)
					{
						foreach(XmlNode columnEventNode in columnEventsList)
						{
							if (columnEventNode == null || !(columnEventNode is XmlElement) ||
								!columnEventNode.HasChildNodes || !((XmlElement)columnEventNode).HasAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE))
								continue;

							string eventTypeText = ((XmlElement)columnEventNode).GetAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE);
							if (string.IsNullOrEmpty(eventTypeText))
								continue;

							TableHistoryStep.EventType columnEventType = TableHistoryStep.EventType.Undefined;

							try
							{
								columnEventType = (TableHistoryStep.EventType)Enum.Parse(typeof(TableHistoryStep.EventType), eventTypeText, false); 
							}
							catch (ArgumentException)
							{
							}
						
							if (columnEventType == TableHistoryStep.EventType.Undefined)
								continue;

							int columnOrder = -1;
							if (((XmlElement)columnEventNode).HasAttribute(XML_COL_HISTORY_EVENT_ORDER_ATTRIBUTE))
							{
								string columnOrderText = ((XmlElement)columnEventNode).GetAttribute(XML_COL_HISTORY_EVENT_ORDER_ATTRIBUTE);
								if (!string.IsNullOrEmpty(columnOrderText))
								{
									try
									{
										columnOrder = Convert.ToInt32(columnOrderText);
									}
									catch(FormatException)
									{
									}
									catch(OverflowException)
									{
									}
								}
							}
					
							int previousColumnOrder = -1;
							if (((XmlElement)columnEventNode).HasAttribute(XML_COL_HISTORY_EVENT_PREVIOUS_ORDER_ATTRIBUTE))
							{
								string previousColumnOrderText = ((XmlElement)columnEventNode).GetAttribute(XML_COL_HISTORY_EVENT_PREVIOUS_ORDER_ATTRIBUTE);
								if (!string.IsNullOrEmpty(previousColumnOrderText))
								{
									try
									{
										previousColumnOrder = Convert.ToInt32(previousColumnOrderText);
									}
									catch(FormatException)
									{
									}
									catch(OverflowException)
									{
									}
								}
							}

							XmlNode columnInfoNode = columnEventNode.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY_COLUMN);
							if (columnInfoNode == null || !(columnInfoNode is XmlElement))
								continue; // non ci sono informazioni sulla colonna!

							WizardTableColumnInfo columnInfo = ParseHistoryColumnInfo((XmlElement)columnInfoNode);
							if (columnInfo == null)
								continue; // non ci sono informazioni sulla colonna!
						
							WizardTableColumnInfo previousColumnInfo = null;
							XmlNode previousColumnInfoNode = columnEventNode.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY_PREVIOUS);
							if (previousColumnInfoNode != null &&  previousColumnInfoNode is XmlElement)
							{
								if (columnEventType == TableHistoryStep.EventType.AlterColumnType ||
									columnEventType == TableHistoryStep.EventType.RenameColumn ||
									columnEventType == TableHistoryStep.EventType.ChangeColumnDefaultValue)
								{
									string previousColumnName = columnInfo.Name;
									if (columnEventType == TableHistoryStep.EventType.RenameColumn)
									{
										previousColumnName = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_NAME_ATTRIBUTE);
										if (string.IsNullOrEmpty(previousColumnName))
											continue;
									}

									previousColumnInfo = new WizardTableColumnInfo(previousColumnName);
							
									if (columnEventType == TableHistoryStep.EventType.AlterColumnType || columnEventType == TableHistoryStep.EventType.RenameColumn)
									{
										string previousColumnDataType = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE);
										if (!string.IsNullOrEmpty(previousColumnDataType))
											previousColumnInfo.DataType = WizardTableColumnDataType.Parse(previousColumnDataType);

										if (applicationInfo != null && previousColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum)
										{
											string columnEnumType = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_COLUMN_DATA_ENUM_ATTRIBUTE);
											if (!string.IsNullOrEmpty(columnEnumType))
											{
												WizardEnumInfo enumInfo = applicationInfo.GetEnumInfoByName(columnEnumType);
												if (enumInfo == null && referencedApplicationsInfo != null && referencedApplicationsInfo.Count > 0)
												{
													foreach(WizardApplicationInfo aLoadedApplication in referencedApplicationsInfo)
													{
														if (aLoadedApplication.HasEnums)
														{
															enumInfo = aLoadedApplication.GetEnumInfoByName(columnEnumType);
															break;
														}
													}
												}
												previousColumnInfo.EnumInfo = enumInfo;
											}
										}
			
										if (previousColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.String)
										{
											string previousColumnLength = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE);
											if (!string.IsNullOrEmpty(previousColumnLength))
											{
												try
												{
													previousColumnInfo.DataLength = Convert.ToUInt32(previousColumnLength);
												}
												catch(FormatException)
												{
												}
												catch(OverflowException)
												{
												}
											}
										}
									}
								
									if (columnEventType == TableHistoryStep.EventType.ChangeColumnDefaultValue)
									{
										previousColumnInfo.DataType = new WizardTableColumnDataType(columnInfo.DataType.Type);
										previousColumnInfo.DataLength = columnInfo.DataLength;

										if (((XmlElement)previousColumnInfoNode).HasAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE))
										{
											string previousColumnDefaultValueText = ((XmlElement)previousColumnInfoNode).GetAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE);
											if (!string.IsNullOrEmpty(previousColumnDefaultValueText))
											{
												if (columnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum)
												{
													previousColumnInfo.DataType = new WizardTableColumnDataType(WizardTableColumnDataType.DataType.Enum);

													if (applicationInfo != null)
													{
														try
														{
															uint defaultEnumItem = UInt32.Parse(previousColumnDefaultValueText);
															WizardEnumInfo enumInfo = applicationInfo.GetEnumInfoByItemStoredValue(defaultEnumItem);
															if (enumInfo == null && referencedApplicationsInfo != null && referencedApplicationsInfo.Count > 0)
															{
																foreach(WizardApplicationInfo aLoadedApplication in referencedApplicationsInfo)
																{
																	if (aLoadedApplication.HasEnums)
																	{
																		enumInfo = aLoadedApplication.GetEnumInfoByItemStoredValue(defaultEnumItem);
																		break;
																	}
																}
															}
															previousColumnInfo.EnumInfo = enumInfo;
														}
														catch(FormatException)
														{
														}
														catch(OverflowException)
														{
														}
													}
												}
										
												previousColumnInfo.SetDefaultValueFromString(previousColumnDefaultValueText);
											}
										}
									}
								}					
							}
							stepInfo.AddColumnEvent(columnInfo, columnOrder, previousColumnInfo, previousColumnOrder, columnEventType);
						}	
					}

					aExtraAddedColumnInfo.AddHistoryStep(stepInfo);
				}

				return true;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(TBWizardProjectsStrings.ExceptionRaisedDuringLoadErrMsg, exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private bool UnparseFile()
		{
			if (projectDocument == null || string.IsNullOrEmpty(projectFileName))
				return false;

			XmlTextWriter writer = null;
			try
			{
				// Save the document to a file and auto-indent the output.
				writer = new XmlTextWriter(projectFileName, Encoding.UTF8);
				writer.Formatting = Formatting.Indented;
				projectDocument.Save(writer);
				
				isModified = false;
				
				return true;
			}
			catch(XmlException exception)
			{
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.SaveFileXmlExceptionErrMsg, projectFileName), exception);
			}
			catch(Exception exception)
			{				
				string errorMessage = null;
				if (exception is IOException)
					errorMessage = TBWizardProjectsStrings.IOExceptionErrorMsg;
				else if (exception is UnauthorizedAccessException)
					errorMessage = TBWizardProjectsStrings.UnauthorizedAccessExceptionErrorMsg;
				else if (exception is DirectoryNotFoundException)
					errorMessage = TBWizardProjectsStrings.DirectoryNotFoundExceptionErrorMsg;
				else if (exception is ArgumentException)
					errorMessage = TBWizardProjectsStrings.InvalidPathStringErrorMsg;
				else
					errorMessage = exception.Message;
				
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.SaveFileXmlExceptionErrMsg, projectFileName) + "\n" + errorMessage);
			}
			finally
			{
				if (writer != null)
					writer.Close();
			}
		}

        //---------------------------------------------------------------------------
        protected void InitProject()
        {
            if (projectDocument == null)
            {
                projectDocument = new XmlDocument();
                XmlDeclaration projectDeclaration = projectDocument.CreateXmlDeclaration(ProjectXmlVersion, ProjectXmlEncoding, "yes");
                if (projectDeclaration != null)
                    projectDocument.AppendChild(projectDeclaration);
            }

            if (projectDocument.DocumentElement == null)
            {
                XmlElement newRoot = projectDocument.CreateElement(RootTag);
                projectDocument.AppendChild(newRoot);
            }
        }

		//---------------------------------------------------------------------------
		private XmlElement GetApplicationInfoNode()
		{
			try
			{
                InitProject();

				XmlNode applicationInfoNode = projectDocument.DocumentElement.SelectSingleNode("child::" + XML_TAG_APPLICATION);
				if (applicationInfoNode != null && (applicationInfoNode is XmlElement))
					return (XmlElement)applicationInfoNode;

				XmlElement newAppInfoElement = projectDocument.CreateElement(XML_TAG_APPLICATION);

				projectDocument.DocumentElement.AppendChild(newAppInfoElement);

				SetNodeApplicationInfo(newAppInfoElement);
				
				return newAppInfoElement;
			}
			catch(InvalidOperationException exception)
			{
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.GenericXmlExceptionErrMsg, "GetApplicationInfoNode"), exception);
			}
			catch(ArgumentException exception)
			{
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.GenericXmlExceptionErrMsg, "GetApplicationInfoNode"), exception);
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.GenericXmlExceptionErrMsg, "GetApplicationInfoNode"), exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private XmlElement GetModuleInfoNode(WizardModuleInfo aModuleInfo)
		{
			if (aModuleInfo == null || string.IsNullOrEmpty(aModuleInfo.Name))
				return null;

			XmlElement applicationInfoNode = GetApplicationInfoNode();
			if (applicationInfoNode == null)
				return null;

			try
			{
				XmlNode modulesInfoNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_MODULES);
				if (modulesInfoNode == null || !(modulesInfoNode is XmlElement))
				{
					XmlElement modulesElement = projectDocument.CreateElement(XML_TAG_MODULES);
					modulesInfoNode = applicationInfoNode.AppendChild(modulesElement);
				}
				else
				{
					XmlNode moduleInfoNode = modulesInfoNode.SelectSingleNode("child::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE +"='" + aModuleInfo.Name + "']");
					if (moduleInfoNode != null && (moduleInfoNode is XmlElement))
						return (XmlElement)moduleInfoNode;
				}
				
				return AddModuleInfoToModulesNode((XmlElement)modulesInfoNode, aModuleInfo);
			}
			catch(InvalidOperationException exception)
			{
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.GenericXmlExceptionErrMsg, "GetModuleInfoNode"), exception);
			}
			catch(ArgumentException exception)
			{
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.GenericXmlExceptionErrMsg, "GetModuleInfoNode"), exception);
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.GenericXmlExceptionErrMsg, "GetModuleInfoNode"), exception);
			}
		}
		
		//---------------------------------------------------------------------------
		private XmlElement GetLibraryInfoNode(WizardLibraryInfo aLibraryInfo)
		{
			if (aLibraryInfo == null || string.IsNullOrEmpty(aLibraryInfo.Name))
				return null;

			XmlElement moduleInfoNode = GetModuleInfoNode(aLibraryInfo.Module);
			if (moduleInfoNode == null)
				return null;

			try
			{
				XmlNode librariesInfoNode = moduleInfoNode.SelectSingleNode("child::" + XML_TAG_LIBRARIES);
				if (librariesInfoNode == null || !(librariesInfoNode is XmlElement))
					return null;
				
				XmlNode libraryInfoNode = librariesInfoNode.SelectSingleNode("child::" + XML_TAG_LIBRARY + "[@" + XML_NAME_ATTRIBUTE +"='" + aLibraryInfo.Name + "']");
				if (libraryInfoNode != null && (libraryInfoNode is XmlElement))
					return (XmlElement)libraryInfoNode;
				
				return null;
			}
			catch(XPathException exception)
			{
				throw new TBWizardException(String.Format(TBWizardProjectsStrings.GenericXmlExceptionErrMsg, "GetLibraryInfoNode"), exception);
			}
		}

		//---------------------------------------------------------------------------
		private XmlElement SetProjectNodeChildText(XmlElement aProjectElement, string childTag, string childInnerText)
		{
			if (projectDocument == null || aProjectElement == null)
				return null;

			XmlElement childElement = null;
			
			XmlNode childNode = aProjectElement.SelectSingleNode("child::" + childTag);
			
			if (childNode != null && (childNode is XmlElement))
				childElement = (XmlElement)childNode;

			if (!string.IsNullOrEmpty(childInnerText))
			{
				if (childElement == null)
				{
					childElement = projectDocument.CreateElement(childTag);
					aProjectElement.AppendChild(childElement);
				}
				childElement.InnerText = childInnerText;

				return childElement;
			}
			
			if (childElement != null)
				aProjectElement.RemoveChild(childElement);

			return null;
		}

		//---------------------------------------------------------------------------
		private XmlElement SetApplicationInfoChildText(XmlElement aApplicationInfoElement, string childTag, string childInnerText)
		{
			if (projectDocument == null || aApplicationInfoElement == null || 
				String.Compare(aApplicationInfoElement.Name, XML_TAG_APPLICATION) != 0 || 
				!IsApplicationInfoValidChildTag(childTag))
				return null;

			return SetProjectNodeChildText(aApplicationInfoElement, childTag, childInnerText);
		}
		
		//---------------------------------------------------------------------------
		private void SetNodeApplicationInfo(XmlElement aApplicationInfoElement)
		{
			if (projectDocument == null || aApplicationInfoElement == null || 
				String.Compare(aApplicationInfoElement.Name, XML_TAG_APPLICATION) != 0)
				return;

			aApplicationInfoElement.RemoveAllAttributes();

			aApplicationInfoElement.SetAttribute(XML_NAME_ATTRIBUTE, (applicationInfo != null) ? applicationInfo.Name : String.Empty);
			if (applicationInfo != null && applicationInfo.ReadOnly)
				WriteReadOnly(aApplicationInfoElement);

			SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_TYPE, (applicationInfo != null) ? applicationInfo.Type.ToString() : String.Empty);
			SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_TITLE, (applicationInfo != null) ? applicationInfo.Title : String.Empty);
			SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_VERSION, (applicationInfo != null) ? applicationInfo.Version : String.Empty);
			SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_PRODUCER, (applicationInfo != null) ? applicationInfo.Producer : String.Empty);
			SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_DBSIGNATURE, (applicationInfo != null) ? applicationInfo.DbSignature : String.Empty);
			SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_SHORTNAME, (applicationInfo != null) ? applicationInfo.ShortName : String.Empty);
			SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_EDITION, (applicationInfo != null) ? applicationInfo.Edition.ToString() : String.Empty);
			SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_SOLUTION_TYPE, (applicationInfo != null) ? applicationInfo.SolutionType.ToString() : String.Empty);
			SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_CULTURE, (applicationInfo != null) ? applicationInfo.CultureName : String.Empty);
			
			if (applicationInfo != null)
			{
				XmlElement fontNode = SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_FONT, applicationInfo.FontFamilyName);
				if (fontNode != null)
				{
					fontNode.SetAttribute(XML_APP_FONT_SIZEINPOINTS_ATTRIBUTE, applicationInfo.FontSizeInPoints.ToString(NumberFormatInfo.InvariantInfo));
					fontNode.SetAttribute(XML_APP_FONT_BOLD_ATTRIBUTE, applicationInfo.FontBold.ToString());
					fontNode.SetAttribute(XML_APP_FONT_ITALIC_ATTRIBUTE, applicationInfo.FontItalic.ToString());
				}
			}
			else
				SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_FONT, String.Empty);

			// Guid.ToString returns a String representation of the value of
			// the Guid instance, according to the provided format specifier.
			// If the value of format specifier is "B" the returned string 
			// appears as 32 hexadecimal digits separated by hyphens, enclosed 
			// in brackets: {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
			SetApplicationInfoChildText(aApplicationInfoElement, XML_TAG_APP_GUID, (applicationInfo != null) ? applicationInfo.Guid.ToString("B").ToUpper() : String.Empty);

			// Salvo gli eventuali riferimenti ad altre applicazioni		
			XmlNode referencesNode = aApplicationInfoElement.SelectSingleNode("child::" + XML_TAG_REFERENCES);
			string[] referencedApplications = (applicationInfo != null) ? applicationInfo.ReferencedApplications : null;

			if (referencedApplications != null && referencedApplications.Length > 0)
			{
				XmlElement referencesElement = null;
				if (referencesNode == null || !(referencesNode is XmlElement))
				{
					referencesElement = projectDocument.CreateElement(XML_TAG_REFERENCES);
					aApplicationInfoElement.AppendChild(referencesElement);
				}
				else 
					referencesElement = (XmlElement)referencesNode;
	
				referencesElement.RemoveAll();

				foreach(string aReferencedApplicationName in referencedApplications)
				{
					XmlElement referencedAppElement = projectDocument.CreateElement(XML_TAG_REFERENCED_APP);
					referencedAppElement.InnerText = aReferencedApplicationName;
					referencesElement.AppendChild(referencedAppElement);
				}
			}
			else
			{
				if (referencesNode != null && (referencesNode is XmlElement))
					aApplicationInfoElement.RemoveChild(referencesNode);
			}

			// Salvo le informazioni riguardanti i moduli
			XmlNode modulesInfoNode = aApplicationInfoElement.SelectSingleNode("child::" + XML_TAG_MODULES);
			if (applicationInfo == null || applicationInfo.ModulesCount == 0)
			{
				if (modulesInfoNode != null && (modulesInfoNode is XmlElement))
					aApplicationInfoElement.RemoveChild(modulesInfoNode);

				return;
			}

			XmlElement modulesElement = null;
			if (modulesInfoNode == null || !(modulesInfoNode is XmlElement))
			{
				modulesElement = projectDocument.CreateElement(XML_TAG_MODULES);
				aApplicationInfoElement.AppendChild(modulesElement);
			}
			else 
				modulesElement = (XmlElement)modulesInfoNode;

			modulesElement.RemoveAll();

			foreach(WizardModuleInfo aModuleInfo in applicationInfo.ModulesInfo)
				AddModuleInfoToModulesNode(modulesElement, aModuleInfo);
		}
		
		//---------------------------------------------------------------------------
		private void AddModuleInfoToApplicationNode(XmlElement aApplicationInfoElement, WizardModuleInfo aModuleInfo)
		{
			if (projectDocument == null || aModuleInfo == null || aApplicationInfoElement == null || 
				String.Compare(aApplicationInfoElement.Name, XML_TAG_APPLICATION) != 0)
				return;

			XmlElement modulesElement = null;
			XmlNode modulesInfoNode = aApplicationInfoElement.SelectSingleNode("child::" + XML_TAG_MODULES);
			if (modulesInfoNode == null || !(modulesInfoNode is XmlElement))
			{
				modulesElement = projectDocument.CreateElement(XML_TAG_MODULES);
				aApplicationInfoElement.AppendChild(modulesElement);
			}
			else 
				modulesElement = (XmlElement)modulesInfoNode;

			AddModuleInfoToModulesNode(modulesElement, aModuleInfo);
		}
		
		//---------------------------------------------------------------------------
		private void AddLibraryInfoToModuleNode(XmlElement aModuleInfoElement, WizardLibraryInfo aLibraryInfo)
		{
			if (projectDocument == null || aLibraryInfo == null || 
				aModuleInfoElement == null || String.Compare(aModuleInfoElement.Name, XML_TAG_MODULE) != 0)
				return;

			XmlElement librariesElement = null;
			XmlNode librariesInfoNode = aModuleInfoElement.SelectSingleNode("child::" + XML_TAG_LIBRARIES);
			if (librariesInfoNode == null || !(librariesInfoNode is XmlElement))
			{
				librariesElement = projectDocument.CreateElement(XML_TAG_LIBRARIES);
				aModuleInfoElement.AppendChild(librariesElement);
			}
			else 
				librariesElement = (XmlElement)librariesInfoNode;

			this.AddLibraryInfoToLibrariesNode(librariesElement, aLibraryInfo);
		}
		
		//---------------------------------------------------------------------------
		private XmlElement SetModuleInfoChildText(XmlElement aModuleInfoElement, string childTag, string childInnerText)
		{
			if (projectDocument == null || aModuleInfoElement == null || 
				String.Compare(aModuleInfoElement.Name, XML_TAG_MODULE) != 0 || !IsModuleInfoValidChildTag(childTag))
				return null;

			return SetProjectNodeChildText(aModuleInfoElement, childTag, childInnerText);
		}

		//---------------------------------------------------------------------------
		private XmlElement SetLibraryInfoChildText(XmlElement aLibraryInfoElement, string childTag, string childInnerText)
		{
			if (projectDocument == null || aLibraryInfoElement == null || 
				String.Compare(aLibraryInfoElement.Name, XML_TAG_LIBRARY) != 0 || !IsLibraryInfoValidChildTag(childTag))
				return null;

			return SetProjectNodeChildText(aLibraryInfoElement, childTag, childInnerText);
		}

		//---------------------------------------------------------------------------
		private XmlElement SetTableInfoChildText(XmlElement aTableInfoElement, string childTag, string childInnerText)
		{
			if (projectDocument == null || aTableInfoElement == null || 
				String.Compare(aTableInfoElement.Name, XML_TAG_TABLE) != 0 || !IsTableInfoValidChildTag(childTag))
				return null;

			return SetProjectNodeChildText(aTableInfoElement, childTag, childInnerText);
		}

		//---------------------------------------------------------------------------
		private XmlElement SetDocumentInfoChildText(XmlElement aDocumentInfoElement, string childTag, string childInnerText)
		{
			if (projectDocument == null || aDocumentInfoElement == null || 
				String.Compare(aDocumentInfoElement.Name, XML_TAG_DOCUMENT) != 0 ||	!IsDocumentInfoValidChildTag(childTag))
				return null;

			return SetProjectNodeChildText(aDocumentInfoElement, childTag, childInnerText);
		}

		//---------------------------------------------------------------------------
		private XmlElement SetClientDocumentInfoChildText(XmlElement aDocumentInfoElement, string childTag, string childInnerText)
		{
			if (projectDocument == null || aDocumentInfoElement == null || 
				String.Compare(aDocumentInfoElement.Name, XML_TAG_CLIENT_DOCUMENT) != 0 || !IsClientDocumentInfoValidChildTag(childTag))
				return null;

			return SetProjectNodeChildText(aDocumentInfoElement, childTag, childInnerText);
		}

		//---------------------------------------------------------------------------
		private XmlElement SetDBTColumnInfoChildText(XmlElement aDBTColumnInfoElement, string childTag, string childInnerText)
		{
			if (projectDocument == null || aDBTColumnInfoElement == null || 
				String.Compare(aDBTColumnInfoElement.Name, XML_TAG_COLUMN) != 0 ||
				!IsDBTColumnInfoValidChildTag(childTag))
				return null;

			return SetProjectNodeChildText(aDBTColumnInfoElement, childTag, childInnerText);
		}

		//---------------------------------------------------------------------------
		private XmlElement AddModuleInfoToModulesNode(XmlElement aModulesElement, WizardModuleInfo aModuleInfo)
		{
			if (projectDocument == null || aModuleInfo == null || aModulesElement == null || 
				String.Compare(aModulesElement.Name, XML_TAG_MODULES) != 0)
				return null;

			XmlElement moduleElement = null;
			XmlNode moduleNode = aModulesElement.SelectSingleNode("child::" + XML_TAG_MODULE + "[@" + XML_NAME_ATTRIBUTE +"='" + aModuleInfo.Name + "']");
			if (moduleNode == null || !(moduleNode is XmlElement))
			{
				moduleElement = projectDocument.CreateElement(XML_TAG_MODULE);
				aModulesElement.AppendChild(moduleElement);
			}
			else 
				moduleElement = (XmlElement)moduleNode;

			SetNodeModuleInfo(moduleElement, aModuleInfo);

			return moduleElement;
		}

		//---------------------------------------------------------------------------
		private void SetNodeModuleInfo(XmlElement aModuleElement, WizardModuleInfo aModuleInfo)
		{
			if (projectDocument == null || aModuleInfo == null || aModuleElement == null || 
				String.Compare(aModuleElement.Name, XML_TAG_MODULE) != 0)
				return;

			aModuleElement.RemoveAllAttributes();
			
			aModuleElement.SetAttribute(XML_NAME_ATTRIBUTE, aModuleInfo.Name);
			if (aModuleInfo.ReadOnly)
				WriteReadOnly(aModuleElement);

			SetModuleInfoChildText(aModuleElement, XML_TAG_MOD_TITLE, aModuleInfo.Title);
			SetModuleInfoChildText(aModuleElement, XML_TAG_MOD_DBSIGNATURE, aModuleInfo.DbSignature);
			SetModuleInfoChildText(aModuleElement, XML_TAG_MOD_DBRELEASE, aModuleInfo.DbReleaseNumber.ToString(NumberFormatInfo.InvariantInfo));

            // Guid.ToString returns a String representation of the value of
            // the Guid instance, according to the provided format specifier.
            // If the value of format specifier is "B" the returned string 
            // appears as 32 hexadecimal digits separated by hyphens, enclosed 
            // in brackets: {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
            SetModuleInfoChildText(aModuleElement, XML_TAG_MOD_GUID, aModuleInfo.Guid.ToString("B").ToUpper());

			// Salvo gli enumerativi
			SetModuleEnumsInfo(aModuleElement, aModuleInfo);

			// Salvo le informazioni riguardanti le librerie
			XmlNode librariesInfoNode = aModuleElement.SelectSingleNode("child::" + XML_TAG_LIBRARIES);
			if (aModuleInfo.LibrariesCount > 0)
			{
				XmlElement librariesElement = null;
				if (librariesInfoNode == null || !(librariesInfoNode is XmlElement))
				{
					librariesElement = projectDocument.CreateElement(XML_TAG_LIBRARIES);
					aModuleElement.AppendChild(librariesElement);
				}
				else 
					librariesElement = (XmlElement)librariesInfoNode;

				librariesElement.RemoveAll();

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					AddLibraryInfoToLibrariesNode(librariesElement, aLibraryInfo);
			}
			else
			{
				if (librariesInfoNode != null && (librariesInfoNode is XmlElement))
					aModuleElement.RemoveChild(librariesInfoNode);
			}
		}

		//---------------------------------------------------------------------------
		private void SetModuleEnumsInfo(XmlElement aModuleElement, WizardModuleInfo aModuleInfo)
		{
			if (projectDocument == null || aModuleInfo == null || aModuleElement == null || 
				String.Compare(aModuleElement.Name, XML_TAG_MODULE) != 0)
				return;

			XmlNode enumsInfoNode = aModuleElement.SelectSingleNode("child::" + XML_TAG_ENUMS);
			if (aModuleInfo.EnumsCount == 0)
			{
				if (enumsInfoNode != null && (enumsInfoNode is XmlElement))
					aModuleElement.RemoveChild(enumsInfoNode);
				
				return;
			}
			XmlElement enumsElement = null;
			if (enumsInfoNode == null || !(enumsInfoNode is XmlElement))
			{
				enumsElement = projectDocument.CreateElement(XML_TAG_ENUMS);
				aModuleElement.AppendChild(enumsElement);
			}
			else 
				enumsElement = (XmlElement)enumsInfoNode;

			enumsElement.RemoveAll();

			foreach(WizardEnumInfo aEnumInfo in aModuleInfo.EnumsInfo)
				AddEnumInfoToEnumsNode(enumsElement, aEnumInfo);
		}
		
		//---------------------------------------------------------------------------
		private void AddEnumInfoToEnumsNode(XmlElement aEnumsElement, WizardEnumInfo aEnumInfo)
		{
			if (projectDocument == null || aEnumInfo == null || aEnumsElement == null || 
				String.Compare(aEnumsElement.Name, XML_TAG_ENUMS) != 0)
				return;

			XmlElement enumElement = null;
			XmlNode enumNode = aEnumsElement.SelectSingleNode("child::" + XML_TAG_ENUM + "[@" + XML_NAME_ATTRIBUTE +"='" + Generics.SubstituteXMLReservedCharacters(aEnumInfo.Name) + "']");
			if (enumNode == null || !(enumNode is XmlElement))
			{
				enumElement = projectDocument.CreateElement(XML_TAG_ENUM);
				aEnumsElement.AppendChild(enumElement);
			}
			else 
				enumElement = (XmlElement)enumNode;

			SetNodeEnumInfo(enumElement, aEnumInfo);
		}
		
		//---------------------------------------------------------------------------
		private void SetNodeEnumInfo(XmlElement aEnumElement, WizardEnumInfo aEnumInfo)
		{
			if (projectDocument == null || aEnumInfo == null || aEnumElement == null || 
				String.Compare(aEnumElement.Name, XML_TAG_ENUM) != 0)
				return;

			aEnumElement.RemoveAllAttributes();
			
			aEnumElement.SetAttribute(XML_NAME_ATTRIBUTE, aEnumInfo.Name);
			if (aEnumInfo.ReadOnly)
				WriteReadOnly(aEnumElement);

			aEnumElement.SetAttribute(XML_ENUM_VALUE_ATTRIBUTE, aEnumInfo.Value.ToString(NumberFormatInfo.InvariantInfo));

			XmlNode itemsInfoNode = aEnumElement.SelectSingleNode("child::" + XML_TAG_ENUM_ITEMS);
			if (aEnumInfo.ItemsCount > 0)
			{
				XmlElement itemsElement = null;
				if (itemsInfoNode == null || !(itemsInfoNode is XmlElement))
				{
					itemsElement = projectDocument.CreateElement(XML_TAG_ENUM_ITEMS);
					aEnumElement.AppendChild(itemsElement);
				}
				else 
					itemsElement = (XmlElement)itemsInfoNode;

				itemsElement.RemoveAll();

				foreach(WizardEnumItemInfo aItemInfo in aEnumInfo.ItemsInfo)
					AddEnumItemInfoToEnumItemsNode(itemsElement, aItemInfo);
			}
			else
			{
				if (itemsInfoNode != null && (itemsInfoNode is XmlElement))
					aEnumElement.RemoveChild(itemsInfoNode);
			}
		}
		
		//---------------------------------------------------------------------------
		private void AddEnumItemInfoToEnumItemsNode(XmlElement aEnumItemsElement, WizardEnumItemInfo aEnumItemInfo)
		{
			if (projectDocument == null || aEnumItemInfo == null || aEnumItemsElement == null || 
				String.Compare(aEnumItemsElement.Name, XML_TAG_ENUM_ITEMS) != 0)
				return;

			XmlElement itemElement = null;
			XmlNode itemNode = aEnumItemsElement.SelectSingleNode("child::" + XML_TAG_ENUM_ITEM + "[@" + XML_NAME_ATTRIBUTE + "='" + Generics.SubstituteXMLReservedCharacters(aEnumItemInfo.Name) + "']");
			if (itemNode == null || !(itemNode is XmlElement))
			{
				itemElement = projectDocument.CreateElement(XML_TAG_ENUM_ITEM);
				aEnumItemsElement.AppendChild(itemElement);
			}
			else 
				itemElement = (XmlElement)itemNode;
	
			SetNodeEnumItemInfo(itemElement, aEnumItemInfo);
		}
		
		//---------------------------------------------------------------------------
		private void SetNodeEnumItemInfo(XmlElement aEnumItemElement, WizardEnumItemInfo aEnumItemInfo)
		{
			if (projectDocument == null || aEnumItemInfo == null || aEnumItemElement == null || 
				String.Compare(aEnumItemElement.Name, XML_TAG_ENUM_ITEM) != 0)
				return;

			aEnumItemElement.RemoveAllAttributes();

			aEnumItemElement.SetAttribute(XML_NAME_ATTRIBUTE, aEnumItemInfo.Name);
			if (aEnumItemInfo.ReadOnly)
				WriteReadOnly(aEnumItemElement);

			aEnumItemElement.SetAttribute(XML_ENUM_VALUE_ATTRIBUTE, aEnumItemInfo.Value.ToString(NumberFormatInfo.InvariantInfo));
			if (aEnumItemInfo.IsDefaultItem)
				aEnumItemElement.SetAttribute(XML_ENUM_DEFAULT_ITEM_ATTRIBUTE, TrueAttributeValue);
		}
		
		//---------------------------------------------------------------------------
		private void AddLibraryInfoToLibrariesNode(XmlElement aLibrariesElement, WizardLibraryInfo aLibraryInfo)
		{
			if (projectDocument == null || aLibraryInfo == null || aLibrariesElement == null || 
				String.Compare(aLibrariesElement.Name, XML_TAG_LIBRARIES) != 0)
				return;

			XmlElement libraryElement = null;
			XmlNode libraryNode = aLibrariesElement.SelectSingleNode("child::" + XML_TAG_LIBRARY + "[@" + XML_NAME_ATTRIBUTE +"='" + aLibraryInfo.Name + "']");
			if (libraryNode == null || !(libraryNode is XmlElement))
			{
				libraryElement = projectDocument.CreateElement(XML_TAG_LIBRARY);
				aLibrariesElement.AppendChild(libraryElement);
			}
			else 
				libraryElement = (XmlElement)libraryNode;

			SetNodeLibraryInfo(libraryElement, aLibraryInfo);
		}

		//---------------------------------------------------------------------------
		private void SetNodeLibraryInfo(XmlElement aLibraryElement, WizardLibraryInfo aLibraryInfo)
		{
			if (projectDocument == null || aLibraryInfo == null || aLibraryElement == null || 
				String.Compare(aLibraryElement.Name, XML_TAG_LIBRARY) != 0)
				return;

			aLibraryElement.RemoveAllAttributes();
			
			aLibraryElement.SetAttribute(XML_NAME_ATTRIBUTE, aLibraryInfo.Name);
			if (aLibraryInfo.ReadOnly)
				WriteReadOnly(aLibraryElement);

			if (aLibraryInfo.TrapDSNChangedEvent)
				aLibraryElement.SetAttribute(XML_LIB_TRAP_DSNCHANGED_ATTRIBUTE, TrueAttributeValue);

			if (aLibraryInfo.TrapApplicationDateChangedEvent)
				aLibraryElement.SetAttribute(XML_LIB_TRAP_APPDATECHANGED_ATTRIBUTE, TrueAttributeValue);

			SetLibraryInfoChildText(aLibraryElement, XML_TAG_LIB_SOURCEFOLDER, aLibraryInfo.SourceFolder);

			SetLibraryInfoChildText(aLibraryElement, XML_TAG_LIB_MENUTITLE, aLibraryInfo.MenuTitle);

			// Guid.ToString returns a String representation of the value of
			// the Guid instance, according to the provided format specifier.
			// If the value of format specifier is "B" the returned string 
			// appears as 32 hexadecimal digits separated by hyphens, enclosed 
			// in brackets: {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
			SetLibraryInfoChildText(aLibraryElement, XML_TAG_LIB_GUID, aLibraryInfo.Guid.ToString("B").ToUpper());

			XmlElement firstResourceIdNode = SetLibraryInfoChildText(aLibraryElement, XML_TAG_FIRST_RESOURCE_ID, aLibraryInfo.FirstResourceId.ToString(NumberFormatInfo.InvariantInfo));
			if (firstResourceIdNode != null)
				firstResourceIdNode.SetAttribute(XML_RESERVED_IDS_RANGE_ATTRIBUTE, aLibraryInfo.ReservedResourceIdsRange.ToString(NumberFormatInfo.InvariantInfo));

			XmlElement firstControlIdNode = SetLibraryInfoChildText(aLibraryElement, XML_TAG_FIRST_CONTROL_ID, aLibraryInfo.FirstControlId.ToString(NumberFormatInfo.InvariantInfo));
			if (firstControlIdNode != null)
				firstControlIdNode.SetAttribute(XML_RESERVED_IDS_RANGE_ATTRIBUTE, aLibraryInfo.ReservedControlIdsRange.ToString(NumberFormatInfo.InvariantInfo));

			XmlElement firstCommandIdNode = SetLibraryInfoChildText(aLibraryElement, XML_TAG_FIRST_COMMAND_ID, aLibraryInfo.FirstCommandId.ToString(NumberFormatInfo.InvariantInfo));
			if (firstCommandIdNode != null)
				firstCommandIdNode.SetAttribute(XML_RESERVED_IDS_RANGE_ATTRIBUTE, aLibraryInfo.ReservedCommandIdsRange.ToString(NumberFormatInfo.InvariantInfo));

			XmlElement firstSymedIdNode = SetLibraryInfoChildText(aLibraryElement, XML_TAG_FIRST_SYMED_ID, aLibraryInfo.FirstSymedId.ToString(NumberFormatInfo.InvariantInfo));
			if (firstSymedIdNode != null)
				firstSymedIdNode.SetAttribute(XML_RESERVED_IDS_RANGE_ATTRIBUTE, aLibraryInfo.ReservedSymedIdsRange.ToString(NumberFormatInfo.InvariantInfo));

			SetNodeLibraryTablesInfo(aLibraryElement, aLibraryInfo);

			SetNodeLibraryDBTsInfo(aLibraryElement, aLibraryInfo);

			SetNodeLibraryDocumentsInfo(aLibraryElement, aLibraryInfo);

			SetNodeLibraryClientDocumentsInfo(aLibraryElement, aLibraryInfo);
			
			SetNodeLibraryDependenciesInfo(aLibraryElement, aLibraryInfo);

			SetNodeLibraryExtraAddedColumnsInfo(aLibraryElement, aLibraryInfo);
		}
		
		//---------------------------------------------------------------------------
		private void SetNodeLibraryTablesInfo(XmlElement aLibraryElement, WizardLibraryInfo aLibraryInfo)
		{
			if (projectDocument == null || aLibraryInfo == null || aLibraryElement == null || 
				String.Compare(aLibraryElement.Name, XML_TAG_LIBRARY) != 0)
				return;

			XmlNode tablesInfoNode = aLibraryElement.SelectSingleNode("child::" + XML_TAG_TABLES);
			if (aLibraryInfo.TablesCount == 0)
			{
				if (tablesInfoNode != null && (tablesInfoNode is XmlElement))
					aLibraryElement.RemoveChild(tablesInfoNode);
				return;
			}

			XmlElement tablesElement = null;
			if (tablesInfoNode == null || !(tablesInfoNode is XmlElement))
			{
				tablesElement = projectDocument.CreateElement(XML_TAG_TABLES);
				aLibraryElement.AppendChild(tablesElement);
			}
			else 
				tablesElement = (XmlElement)tablesInfoNode;

			tablesElement.RemoveAll();

			foreach(WizardTableInfo aTableInfo in aLibraryInfo.TablesInfo)
			{
				if (aLibraryInfo.Module != null && aTableInfo.CreationDbReleaseNumber > aLibraryInfo.Module.DbReleaseNumber)
					continue;
				
				AddTableInfoToTablesNode(tablesElement, aTableInfo);
			}
		}
		
		//---------------------------------------------------------------------------
		private void SetNodeLibraryDBTsInfo(XmlElement aLibraryElement, WizardLibraryInfo aLibraryInfo)
		{
			if (projectDocument == null || aLibraryInfo == null || aLibraryElement == null || 
				String.Compare(aLibraryElement.Name, XML_TAG_LIBRARY) != 0)
				return;

			XmlNode dbtsInfoNode = aLibraryElement.SelectSingleNode("child::" + XML_TAG_DBTS);
			if (aLibraryInfo.DBTsCount == 0)
			{
				if (dbtsInfoNode != null && (dbtsInfoNode is XmlElement))
					aLibraryElement.RemoveChild(dbtsInfoNode);
				return;
			}
			
			XmlElement dbtsElement = null;
			if (dbtsInfoNode == null || !(dbtsInfoNode is XmlElement))
			{
				dbtsElement = projectDocument.CreateElement(XML_TAG_DBTS);
				aLibraryElement.AppendChild(dbtsElement);
			}
			else 
				dbtsElement = (XmlElement)dbtsInfoNode;

			dbtsElement.RemoveAll();

			foreach(WizardDBTInfo aDBTInfo in aLibraryInfo.DBTsInfo)
				AddDBTInfoToDBTsNode(dbtsElement, aDBTInfo);
		}
		
		//---------------------------------------------------------------------------
		private void SetNodeLibraryDocumentsInfo(XmlElement aLibraryElement, WizardLibraryInfo aLibraryInfo)
		{
			if (projectDocument == null || aLibraryInfo == null || aLibraryElement == null || 
				String.Compare(aLibraryElement.Name, XML_TAG_LIBRARY) != 0)
				return;

			XmlNode documentsInfoNode = aLibraryElement.SelectSingleNode("child::" + XML_TAG_DOCUMENTS);
			if (aLibraryInfo.DocumentsCount == 0)
			{
				if (documentsInfoNode != null && (documentsInfoNode is XmlElement))
					aLibraryElement.RemoveChild(documentsInfoNode);
				return;
			}
			
			XmlElement documentsElement = null;
			if (documentsInfoNode == null || !(documentsInfoNode is XmlElement))
			{
				documentsElement = projectDocument.CreateElement(XML_TAG_DOCUMENTS);
				aLibraryElement.AppendChild(documentsElement);
			}
			else 
				documentsElement = (XmlElement)documentsInfoNode;

			documentsElement.RemoveAll();

			foreach(WizardDocumentInfo aDocumentInfo in aLibraryInfo.DocumentsInfo)
				AddDocumentInfoToDocumentsNode(documentsElement, aDocumentInfo);
		}
		
		//---------------------------------------------------------------------------
		private void SetNodeLibraryClientDocumentsInfo(XmlElement aLibraryElement, WizardLibraryInfo aLibraryInfo)
		{
			if (projectDocument == null || aLibraryInfo == null || aLibraryElement == null || 
				String.Compare(aLibraryElement.Name, XML_TAG_LIBRARY) != 0)
				return;

			XmlNode clientDocsInfoNode = aLibraryElement.SelectSingleNode("child::" + XML_TAG_CLIENT_DOCUMENTS);
			if (aLibraryInfo.ClientDocumentsCount == 0)
			{
				if (clientDocsInfoNode != null && (clientDocsInfoNode is XmlElement))
					aLibraryElement.RemoveChild(clientDocsInfoNode);
				return;
			}
			
			XmlElement clientDocsElement = null;
			if (clientDocsInfoNode == null || !(clientDocsInfoNode is XmlElement))
			{
				clientDocsElement = projectDocument.CreateElement(XML_TAG_CLIENT_DOCUMENTS);
				aLibraryElement.AppendChild(clientDocsElement);
			}
			else 
				clientDocsElement = (XmlElement)clientDocsInfoNode;

			clientDocsElement.RemoveAll();

			foreach(WizardClientDocumentInfo aClientDocInfo in aLibraryInfo.ClientDocumentsInfo)
				AddClientDocumentInfoToDocumentsNode(clientDocsElement, aClientDocInfo);
		}

		//---------------------------------------------------------------------------
		private void SetNodeLibraryDependenciesInfo(XmlElement aLibraryElement, WizardLibraryInfo aLibraryInfo)
		{
			if (projectDocument == null || aLibraryInfo == null || aLibraryElement == null || 
				String.Compare(aLibraryElement.Name, XML_TAG_LIBRARY) != 0)
				return;

			XmlNode dependenciesInfoNode = aLibraryElement.SelectSingleNode("child::" + XML_TAG_DEPENDENCIES);
			if (aLibraryInfo.Dependencies == null || aLibraryInfo.Dependencies.Count == 0)
			{
				if (dependenciesInfoNode != null && (dependenciesInfoNode is XmlElement))
					aLibraryElement.RemoveChild(dependenciesInfoNode);
				return;
			}
			
			XmlElement dependenciesElement = null;
			if (dependenciesInfoNode == null || !(dependenciesInfoNode is XmlElement))
			{
				dependenciesElement = projectDocument.CreateElement(XML_TAG_DEPENDENCIES);
				aLibraryElement.AppendChild(dependenciesElement);
			}
			else 
				dependenciesElement = (XmlElement)dependenciesInfoNode;

			dependenciesElement.RemoveAll();

			foreach(WizardLibraryInfo aDependency in aLibraryInfo.Dependencies)
				AddDependencyToDependenciesNode(dependenciesElement, aDependency);
		}

		//---------------------------------------------------------------------------
		private void SetNodeLibraryExtraAddedColumnsInfo(XmlElement aLibraryElement, WizardLibraryInfo aLibraryInfo)
		{
			if (projectDocument == null || aLibraryInfo == null || aLibraryElement == null || 
				String.Compare(aLibraryElement.Name, XML_TAG_LIBRARY) != 0)
				return;

			XmlNode extraAddedColumnsInfoNode = aLibraryElement.SelectSingleNode("child::" + XML_TAG_EXTRA_ADDED_COLUMNS);
			if (aLibraryInfo.ExtraAddedColumnsCount == 0)
			{
				if (extraAddedColumnsInfoNode != null && (extraAddedColumnsInfoNode is XmlElement))
					aLibraryElement.RemoveChild(extraAddedColumnsInfoNode);
				return;
			}
			
			XmlElement extraAddedColumnsElement = null;
			if (extraAddedColumnsInfoNode == null || !(extraAddedColumnsInfoNode is XmlElement))
			{
				extraAddedColumnsElement = projectDocument.CreateElement(XML_TAG_EXTRA_ADDED_COLUMNS);
				aLibraryElement.AppendChild(extraAddedColumnsElement);
			}
			else 
				extraAddedColumnsElement = (XmlElement)extraAddedColumnsInfoNode;

			extraAddedColumnsElement.RemoveAll();

			foreach(WizardExtraAddedColumnsInfo aExtraAddedColumnInfo in aLibraryInfo.ExtraAddedColumnsInfo)
				AddExtraAddedColumnInfoToColumnsNode(extraAddedColumnsElement, aExtraAddedColumnInfo);
		}

		//---------------------------------------------------------------------------
		public void AddTableInfoToTablesNode(WizardTableInfo aTableInfo)
		{
			if (aTableInfo == null)
				return;

            InitProject();
			
			XmlElement aTablesElement = projectDocument.DocumentElement.SelectSingleNode("child::" + XML_TAG_TABLES) as XmlElement;
			if (aTablesElement == null)
			{
				aTablesElement = projectDocument.CreateElement(XML_TAG_TABLES);
				projectDocument.DocumentElement.AppendChild(aTablesElement);
			}
			
			XmlElement tableElement = aTablesElement.SelectSingleNode("child::" + XML_TAG_TABLE + "[@" + XML_NAME_ATTRIBUTE + "='" + aTableInfo.Name + "']") as XmlElement;
			if (tableElement == null)
			{
				tableElement = projectDocument.CreateElement(XML_TAG_TABLE);
				aTablesElement.AppendChild(tableElement);
			}
			
			SetNodeTableInfo(tableElement, aTableInfo);
			SetNodeIndexInfo(tableElement, aTableInfo);
		}
		
		//---------------------------------------------------------------------------
		private void AddTableInfoToTablesNode(XmlElement aTablesElement, WizardTableInfo aTableInfo)
		{
			if (projectDocument == null || aTableInfo == null || aTablesElement == null || 
				String.Compare(aTablesElement.Name, XML_TAG_TABLES) != 0)
				return;

			XmlElement tableElement = null;
			XmlNode tableNode = aTablesElement.SelectSingleNode("child::" + XML_TAG_TABLE + "[@" + XML_NAME_ATTRIBUTE + "='" + aTableInfo.Name + "']");
			if (tableNode == null || !(tableNode is XmlElement))
			{
				tableElement = projectDocument.CreateElement(XML_TAG_TABLE);
				aTablesElement.AppendChild(tableElement);
			}
			else 
				tableElement = (XmlElement)tableNode;

			SetNodeTableInfo(tableElement, aTableInfo);
		}

		//---------------------------------------------------------------------------
		private void SetNodeIndexInfo(XmlElement aTableElement, WizardTableInfo aTableInfo)
		{
			if (projectDocument == null ||
				aTableInfo == null ||
				aTableInfo.Indexes == null ||
				aTableInfo.Indexes.Count == 0 ||
				aTableElement == null ||
				String.Compare(aTableElement.Name, XML_TAG_TABLE) != 0)
				return;

			XmlNode indexesNode = aTableElement.SelectSingleNode(XML_TAG_INDEXES);
			if (indexesNode == null)
			{
				indexesNode = projectDocument.CreateElement(XML_TAG_INDEXES);
				aTableElement.AppendChild(indexesNode);
			}

			foreach (WizardTableIndexInfo index in aTableInfo.Indexes)
			{
				if (index == null)
					continue;

				XmlNode indexNode = indexesNode.SelectSingleNode(XML_TAG_INDEX + "[@" + XML_NAME_ATTRIBUTE + " = '" + index.Name + "']");
				if (indexNode != null)
					continue;//c'è già - non controllo altro per ora...

				indexNode = projectDocument.CreateElement(XML_TAG_INDEX);
				((XmlElement)indexNode).SetAttribute(XML_NAME_ATTRIBUTE, index.Name);
				((XmlElement)indexNode).SetAttribute(XML_TABLE_NAME_ATTRIBUTE, index.TableName);
				
				if (index.Unique)
					((XmlElement)indexNode).SetAttribute(XML_INDEX_UNIQUE_ATTRIBUTE, Boolean.TrueString);
				
				if (!index.NonClustered)
					((XmlElement)indexNode).SetAttribute(XML_INDEX_NON_CLUSTERED_ATTRIBUTE, Boolean.FalseString);

				if (index.Segments != null)
				{
					foreach (WizardTableColumnInfo col in index.Segments)
					{
						XmlElement segmentNode = projectDocument.CreateElement(XML_TAG_INDEX_SEGMENT);
						segmentNode.SetAttribute(XML_NAME_ATTRIBUTE, col.Name);
						indexNode.AppendChild(segmentNode);
					}
				}
				indexesNode.AppendChild(indexNode);
			}
		}

		//---------------------------------------------------------------------------
		private void SetNodeTableInfo(XmlElement aTableElement, WizardTableInfo aTableInfo)
		{
			if (projectDocument == null || aTableInfo == null || aTableElement == null || 
				String.Compare(aTableElement.Name, XML_TAG_TABLE) != 0)
				return;

			aTableElement.RemoveAllAttributes();

			aTableElement.SetAttribute(XML_NAME_ATTRIBUTE, aTableInfo.Name);
			aTableElement.SetAttribute(XML_LOCALIZE_ATTRIBUTE, aTableInfo.Name);
			
			if (aTableInfo.ReadOnly)
				WriteReadOnly(aTableElement);

			aTableElement.SetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE, aTableInfo.CreationDbReleaseNumber.ToString(NumberFormatInfo.InvariantInfo));
            aTableElement.SetAttribute(XML_TB_NAMESPACE_ATTRIBUTE, aTableInfo.TbNameSpace);

			if (!string.IsNullOrEmpty(aTableInfo.ClassName))
				WriteClassName(aTableElement, aTableInfo.ClassName);

			if (!String.IsNullOrEmpty(aTableInfo.PrimaryKeyConstraintName))
			{
				aTableElement.SetAttribute(XML_TABLE_PRIMARY_KEY_CONSTRAINT_NAME_ATTRIBUTE, aTableInfo.PrimaryKeyConstraintName);
				if (aTableInfo.PrimaryKeyClustered)
					aTableElement.SetAttribute(XML_TABLE_PRIMARY_KEY_CLUSTERED_ATTRIBUTE, Boolean.TrueString);
			}
			
			if (aTableInfo.AddTBGuidColumn)
				aTableElement.SetAttribute(XML_TABLE_ADD_TBGUID_COLUMN_ATTRIBUTE, TrueAttributeValue);

			if (aTableInfo.IsHKLDefined)
			{
				aTableElement.SetAttribute(XML_TABLE_HKL_NAME_ATTRIBUTE, aTableInfo.HKLName);
				aTableElement.SetAttribute(XML_TABLE_HKL_CLASS_NAME_ATTRIBUTE, aTableInfo.HKLClassName);
			}

			SetNodeTableColumnsInfo(aTableElement, aTableInfo);

			SetNodeTableForeignKeysInfo(aTableElement, aTableInfo);

			SetNodeTableHistoryInfo(aTableElement, aTableInfo);
		}

		//---------------------------------------------------------------------------
		private void SetNodeTableColumnsInfo(XmlElement aTableElement, WizardTableInfo aTableInfo)
		{
			if (projectDocument == null || aTableInfo == null || aTableElement == null || 
				String.Compare(aTableElement.Name, XML_TAG_TABLE) != 0)
				return;

			XmlNode columnsInfoNode = aTableElement.SelectSingleNode("child::" + XML_TAG_COLUMNS);
			if (aTableInfo.ColumnsCount == 0)
			{
				if (columnsInfoNode != null && (columnsInfoNode is XmlElement))
					aTableElement.RemoveChild(columnsInfoNode);
				return;
			}

			XmlElement columnsElement = null;
			if (columnsInfoNode == null || !(columnsInfoNode is XmlElement))
			{
				columnsElement = projectDocument.CreateElement(XML_TAG_COLUMNS);
				aTableElement.AppendChild(columnsElement);
			}
			else 
				columnsElement = (XmlElement)columnsInfoNode;

			columnsElement.RemoveAll();

			foreach(WizardTableColumnInfo aColumnInfo in aTableInfo.ColumnsInfo)
			{
				XmlElement addedColumnElement = AddTableColumnInfoToColumnsNode(columnsElement, aColumnInfo, aTableInfo.CreationDbReleaseNumber);
				if (addedColumnElement == null)
					continue;

				if (aTableInfo.HKLCodeColumn == aColumnInfo)
					addedColumnElement.SetAttribute(XML_COLUMN_HKL_CODE_ATTRIBUTE, TrueAttributeValue);
				
				if (aTableInfo.HKLDescriptionColumn == aColumnInfo)
					addedColumnElement.SetAttribute(XML_COLUMN_HKL_DESCR_ATTRIBUTE, TrueAttributeValue);
			}
		}
		
		//---------------------------------------------------------------------------
		private XmlElement AddTableColumnInfoToColumnsNode(XmlElement aColumnsElement, WizardTableColumnInfo aColumnInfo, uint tableCreationDbReleaseNumber)
		{
			if (projectDocument == null || aColumnInfo == null || aColumnsElement == null || 
				String.Compare(aColumnsElement.Name, XML_TAG_COLUMNS) != 0)
				return null;

			XmlElement columnElement = null;
			XmlNode columnNode = aColumnsElement.SelectSingleNode("child::" + XML_TAG_COLUMN + "[@" + XML_NAME_ATTRIBUTE +"='" + aColumnInfo.Name + "']");
			if (columnNode == null || !(columnNode is XmlElement))
			{
				columnElement = projectDocument.CreateElement(XML_TAG_COLUMN);
				aColumnsElement.AppendChild(columnElement);
			}
			else 
				columnElement = (XmlElement)columnNode;

			SetNodeTableColumnInfo(columnElement, aColumnInfo, tableCreationDbReleaseNumber);

			return columnElement;
		}

		//---------------------------------------------------------------------------
		protected void SetNodeTableColumnInfo(XmlElement aColumnElement, WizardTableColumnInfo aColumnInfo, uint tableCreationDbReleaseNumber)
		{
			if (projectDocument == null || aColumnInfo == null || aColumnElement == null || 
				String.Compare(aColumnElement.Name, XML_TAG_COLUMN) != 0)
				return;

			aColumnElement.RemoveAllAttributes();

			aColumnElement.SetAttribute(XML_NAME_ATTRIBUTE, aColumnInfo.Name);

			aColumnElement.SetAttribute(XML_LOCALIZE_ATTRIBUTE, aColumnInfo.Name);
			if (aColumnInfo.ReadOnly)
				WriteReadOnly(aColumnElement);
			if (tableCreationDbReleaseNumber < aColumnInfo.CreationDbReleaseNumber)
				aColumnElement.SetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE, aColumnInfo.CreationDbReleaseNumber.ToString(NumberFormatInfo.InvariantInfo));
            if (aColumnInfo.CreateStep > 0) 
                aColumnElement.SetAttribute(XML_CREATE_STEP_ATTRIBUTE, aColumnInfo.CreateStep.ToString());

			aColumnElement.SetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE, WizardTableColumnDataType.Unparse(aColumnInfo.DataType));

			if (aColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum)
			{
				// gestione del wizard
				if (!string.IsNullOrEmpty(aColumnInfo.EnumTypeName))
					aColumnElement.SetAttribute(XML_COLUMN_DATA_ENUM_ATTRIBUTE, aColumnInfo.EnumTypeName);
				else // gestione tool di migrazione 3.0 (scrive il numero del value dell'enumerativo)
					aColumnElement.SetAttribute(XML_BASETYPE_ATTRIBUTE, aColumnInfo.TbEnum.ToString());
			}

			if (aColumnInfo.IsUpperCaseDataString)
				aColumnElement.SetAttribute(XML_COLUMN_UPPER_CASE_ATTRIBUTE, TrueAttributeValue);
			
			if (aColumnInfo.DataLength > 0)
				aColumnElement.SetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE, aColumnInfo.DataLength.ToString(NumberFormatInfo.InvariantInfo));

			// prima controlliamo se è stata definita un'espressione per il default
			// se c'è prevale su un'eventuale definizione di un valore di default che invece deve essere compatibile
			// con il tipo della colonna
			if (!string.IsNullOrEmpty(aColumnInfo.DefaultExpressionValue))
				aColumnElement.SetAttribute(XML_COLUMN_DEFAULT_EXPRESSION_VALUE_ATTRIBUTE, aColumnInfo.DefaultExpressionValue);
			else 
				if (aColumnInfo.HasSpecificDefaultValue)
				{
					string defaultValueString = aColumnInfo.DefaultValueString;
					if (defaultValueString != null)
						aColumnElement.SetAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE, defaultValueString);
				}

			if (aColumnInfo.IsPrimaryKeySegment)
				aColumnElement.SetAttribute(XML_COLUMN_PRIMARY_SEG_ATTRIBUTE, TrueAttributeValue);

			if (!aColumnInfo.IsNullable)
				aColumnElement.SetAttribute(XML_COLUMN_NULLABLE_VALUE_ATTRIBUTE, Boolean.FalseString.ToLower());

			if (!aColumnInfo.IsCollateSensitive)
				aColumnElement.SetAttribute(XML_COLUMN_COLLATE_SENSITIVE_ATTRIBUTE, Boolean.FalseString.ToLower());

			if (aColumnInfo.IsAutoIncrement)
			{
				aColumnElement.SetAttribute(XML_COLUMN_AUTO_INCREMENT_ATTRIBUTE, Boolean.TrueString.ToLower());
				aColumnElement.SetAttribute(XML_COLUMN_AUTO_INCREMENT_SEED_ATTRIBUTE, aColumnInfo.Seed.ToString());
				aColumnElement.SetAttribute(XML_COLUMN_AUTO_INCREMENT_INCREMENT_ATTRIBUTE, aColumnInfo.Increment.ToString());
			}

			if (aColumnInfo.DefaultConstraintName != null && aColumnInfo.DefaultConstraintName.Length > 0)
				aColumnElement.SetAttribute(XML_COLUMN_DEFAULT_CONSTRAINT_NAME_ATTRIBUTE, aColumnInfo.DefaultConstraintName);
		}

		///<summary>
		/// Metodo vuoto reimplementato dal DBObjectsProjectParser
		/// Utilizzato per non riportare l'attributo classname nella gestione del database 3.0
		///</summary>
		//---------------------------------------------------------------------------
		protected virtual void WriteClassName(XmlElement aElement, string aValue)
		{
			aElement.SetAttribute(XML_CLASS_NAME_ATTRIBUTE, aValue);
		}

		///<summary>
		/// Metodo reimplementato dal DBObjectsProjectParser
		/// Utilizzato per non riportare l'attributo readonly nella gestione del database 3.0
		///</summary>
		//---------------------------------------------------------------------------
		protected virtual void WriteReadOnly(XmlElement aColumnElement)
		{
			aColumnElement.SetAttribute(XML_READONLY_ATTRIBUTE, TrueAttributeValue);
		}

		///<summary>
		/// Metodo vuoto reimplementato dal DBObjectsProjectParser
		/// Utilizzato per leggere la release della tabella (serve per la gestione del database 3.0)
		///</summary>
		//---------------------------------------------------------------------------
		protected virtual void GetDBReleaseNumber(XmlElement aTableElement, WizardTableInfo tableInfo)
		{
			// metodo vuoto perchè devo necessariamente leggere la release di DB 
			// relativa alla creazione
			// delle tabelle DOPO aver specificato la libreria ( e quindi il modulo)
			// di appartenenza!!!
			return;
		}

		//---------------------------------------------------------------------------
		private void SetNodeTableForeignKeysInfo(XmlElement aTableElement, WizardTableInfo aTableInfo)
		{
			if (projectDocument == null || aTableInfo == null || aTableElement == null || 
				String.Compare(aTableElement.Name, XML_TAG_TABLE) != 0)
				return;

			XmlNode foreignKeysNode = aTableElement.SelectSingleNode("child::" + XML_TAG_FOREIGN_KEYS);
			if (aTableInfo.ForeignKeysCount == 0)
			{
				if (foreignKeysNode != null && (foreignKeysNode is XmlElement))
					aTableElement.RemoveChild(foreignKeysNode);

				return;
			}

			XmlElement foreignKeysElement = null;
			if (foreignKeysNode == null || !(foreignKeysNode is XmlElement))
			{
				foreignKeysElement = projectDocument.CreateElement(XML_TAG_FOREIGN_KEYS);
				aTableElement.AppendChild(foreignKeysElement);
			}
			else 
				foreignKeysElement = (XmlElement)foreignKeysNode;

			foreignKeysElement.RemoveAll();

			foreach(WizardForeignKeyInfo aForeignKeyInfo in aTableInfo.ForeignKeys)
				AddTableForeignKeyInfoToForeignKeysNode(foreignKeysElement, aTableInfo, aForeignKeyInfo);
		}

		//---------------------------------------------------------------------------
		private XmlElement AddTableForeignKeyInfoToForeignKeysNode
			(
			XmlElement				aForeignKeysElement,
			WizardTableInfo			aTableInfo, 
			WizardForeignKeyInfo	aForeignKeyInfo
			)
		{
			if (projectDocument == null || 
				aForeignKeysElement == null || 
				String.Compare(aForeignKeysElement.Name, XML_TAG_FOREIGN_KEYS) != 0 ||
				aTableInfo == null || 
				aForeignKeyInfo == null ||
				string.IsNullOrEmpty(aForeignKeyInfo.ReferencedTableNameSpace) || 
				aForeignKeyInfo.SegmentsCount == 0)
				return null;

			XmlNodeList foreignKeysList = aForeignKeysElement.SelectNodes("child::" + XML_TAG_FOREIGN_KEY + "[@" + XML_REFERENCED_TABLE_NAMESPACE_ATTRIBUTE +"='" + aForeignKeyInfo.ReferencedTableNameSpace.Trim() + "']");
			if (foreignKeysList != null && foreignKeysList.Count > 0)
			{
				foreach(XmlNode foreignKeyNode in foreignKeysList)
				{
					if (foreignKeyNode == null || !(foreignKeyNode is XmlElement))
						continue;

					XmlNodeList segmentsList = foreignKeyNode.SelectNodes("child::" + XML_TAG_FOREIGN_KEY_SEGMENT);
					if (segmentsList == null || segmentsList.Count == 0)
						continue;

					WizardForeignKeyInfo.KeySegmentsCollection foreignKeySegments = new WizardForeignKeyInfo.KeySegmentsCollection();

					foreach(XmlNode segmentInfoNode in segmentsList)
					{
						if (
							segmentInfoNode == null || 
							!(segmentInfoNode is XmlElement) ||
							!((XmlElement)segmentInfoNode).HasAttribute(XML_FOREIGN_KEY_SEG_COLUMN_ATTRIBUTE) ||
							!((XmlElement)segmentInfoNode).HasAttribute(XML_FOREIGN_KEY_SEG_REFERENCED_COLUMN_ATTRIBUTE))
							continue;

						string columnName = ((XmlElement)segmentInfoNode).GetAttribute(XML_FOREIGN_KEY_SEG_COLUMN_ATTRIBUTE);
						if (string.IsNullOrEmpty(columnName) ||
							aTableInfo.GetColumnInfoByName(columnName) == null)
							continue;

						string referencedColumnName = ((XmlElement)segmentInfoNode).GetAttribute(XML_FOREIGN_KEY_SEG_REFERENCED_COLUMN_ATTRIBUTE);
						if (string.IsNullOrEmpty(referencedColumnName))
							continue;
						
						foreignKeySegments.Add(new WizardForeignKeyInfo.KeySegment(columnName, referencedColumnName));
					}

					if (aForeignKeyInfo.HasSameSegments(foreignKeySegments))
					{
						if (!string.IsNullOrEmpty(aForeignKeyInfo.ConstraintName))
							((XmlElement)foreignKeyNode).SetAttribute(XML_FOREIGN_KEY_CONSTRAINT_NAME_ATTRIBUTE, aForeignKeyInfo.ConstraintName);

						return (XmlElement)foreignKeyNode;
					}
				}
			}
			
			XmlElement foreignKeyElement = projectDocument.CreateElement(XML_TAG_FOREIGN_KEY);
			
			aForeignKeysElement.AppendChild(foreignKeyElement);

			SetNodeTableForeignKeyInfo(foreignKeyElement, aForeignKeyInfo);

			return foreignKeyElement;
		}
		
		//---------------------------------------------------------------------------
		private void SetNodeTableForeignKeyInfo(XmlElement aForeignKeyElement, WizardForeignKeyInfo aForeignKeyInfo)
		{
			if (projectDocument == null || aForeignKeyInfo == null || aForeignKeyElement == null || 
				String.Compare(aForeignKeyElement.Name, XML_TAG_FOREIGN_KEY) != 0)
				return;

			aForeignKeyElement.RemoveAll();

			aForeignKeyElement.SetAttribute(XML_REFERENCED_TABLE_NAMESPACE_ATTRIBUTE, aForeignKeyInfo.ReferencedTableNameSpace);

			if (!string.IsNullOrEmpty(aForeignKeyInfo.ConstraintName))
				aForeignKeyElement.SetAttribute(XML_FOREIGN_KEY_CONSTRAINT_NAME_ATTRIBUTE, aForeignKeyInfo.ConstraintName);

			if (aForeignKeyInfo.OnDeleteCascade)
				aForeignKeyElement.SetAttribute(XML_ON_DELETE_CASCADE_ATTRIBUTE, Boolean.TrueString.ToLower());

			if (aForeignKeyInfo.OnUpdateCascade)
				aForeignKeyElement.SetAttribute(XML_ON_UPDATE_CASCADE_ATTRIBUTE, Boolean.TrueString.ToLower());

			if (aForeignKeyInfo.SegmentsCount > 0)
			{
				foreach(WizardForeignKeyInfo.KeySegment aSegmentInfo in aForeignKeyInfo.Segments)
				{
					XmlElement segmentElement = projectDocument.CreateElement(XML_TAG_FOREIGN_KEY_SEGMENT);
				
					aForeignKeyElement.AppendChild(segmentElement);
					
					segmentElement.SetAttribute(XML_FOREIGN_KEY_SEG_COLUMN_ATTRIBUTE, aSegmentInfo.ColumnName); 
					segmentElement.SetAttribute(XML_FOREIGN_KEY_SEG_REFERENCED_COLUMN_ATTRIBUTE, aSegmentInfo.ReferencedColumnName); 
				}
			}
		}
		
		//---------------------------------------------------------------------------
		private void SetNodeTableHistoryInfo(XmlElement aTableElement, WizardTableInfo aTableInfo)
		{
			if (projectDocument == null || aTableInfo == null || aTableElement == null || 
				String.Compare(aTableElement.Name, XML_TAG_TABLE) != 0)
				return;
		
			XmlNode historyInfoNode = aTableElement.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY);

			if (aTableInfo.History == null || aTableInfo.History.StepsCount == 0)
			{
				if (historyInfoNode != null && (historyInfoNode is XmlElement))
					aTableElement.RemoveChild(historyInfoNode);

				return;
			}

			XmlElement historyElement = null;
			if (historyInfoNode == null || !(historyInfoNode is XmlElement))
			{
				historyElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY);
				aTableElement.AppendChild(historyElement);
			}
			else 
				historyElement = (XmlElement)historyInfoNode;

			historyElement.RemoveAll();

			foreach(TableHistoryStep aStep in aTableInfo.History.Steps)
			{
				if (aStep == null || aStep.DbReleaseNumber <= 0 || aStep.EventsCount == 0)
					continue;

				XmlElement stepElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_STEP);

				stepElement.SetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE, aStep.DbReleaseNumber.ToString(NumberFormatInfo.InvariantInfo));

				if (!string.IsNullOrEmpty(aStep.PrimaryKeyConstraintName))
					stepElement.SetAttribute(XML_TABLE_PRIMARY_KEY_CONSTRAINT_NAME_ATTRIBUTE, aStep.PrimaryKeyConstraintName);

				// Salvataggio delle informazioni relative alle singole modifiche
				XmlElement eventsElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_EVENTS);
				stepElement.AppendChild(eventsElement);

				if (aStep.ColumnsEventsCount > 0)
				{
					foreach(ColumnHistoryEvent aColumnEvent in aStep.ColumnsEvents)
					{
						if (aColumnEvent == null || aColumnEvent.ColumnInfo == null)
							continue;

						XmlElement columnEventElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_COLUMN_EVENT);
						eventsElement.AppendChild(columnEventElement);

						columnEventElement.SetAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE, aColumnEvent.Type.ToString());
					
						if (aColumnEvent.ColumnOrder >= 0)
							columnEventElement.SetAttribute(XML_COL_HISTORY_EVENT_ORDER_ATTRIBUTE, aColumnEvent.ColumnOrder.ToString(NumberFormatInfo.InvariantInfo));
						if (aColumnEvent.PreviousColumnOrder >= 0)
							columnEventElement.SetAttribute(XML_COL_HISTORY_EVENT_PREVIOUS_ORDER_ATTRIBUTE, aColumnEvent.PreviousColumnOrder.ToString(NumberFormatInfo.InvariantInfo));
			
						XmlElement columnElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_COLUMN);
						columnEventElement.AppendChild(columnElement);
				
						columnElement.SetAttribute(XML_NAME_ATTRIBUTE, aColumnEvent.ColumnInfo.Name);
			
						columnElement.SetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE, WizardTableColumnDataType.Unparse(aColumnEvent.ColumnInfo.DataType));

						if (aColumnEvent.ColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum)
							columnElement.SetAttribute(XML_COLUMN_DATA_ENUM_ATTRIBUTE, aColumnEvent.ColumnInfo.EnumTypeName);
			
						if (aColumnEvent.ColumnInfo.DataLength > 0)
							columnElement.SetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE, aColumnEvent.ColumnInfo.DataLength.ToString(NumberFormatInfo.InvariantInfo));

						// prima controlliamo se è stata definita un'espressione per il default
						// se c'è prevale su un'eventuale definizione di un valore di default che invece deve essere compatibile
						// con il tipo della colonna
						if (!string.IsNullOrEmpty(aColumnEvent.ColumnInfo.DefaultExpressionValue))
							columnElement.SetAttribute(XML_COLUMN_DEFAULT_EXPRESSION_VALUE_ATTRIBUTE, aColumnEvent.ColumnInfo.DefaultExpressionValue);
						else 
							if (aColumnEvent.ColumnInfo.HasSpecificDefaultValue)
							{
								string defaultValueString = aColumnEvent.ColumnInfo.DefaultValueString;
								if (defaultValueString != null)
									columnElement.SetAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE, defaultValueString);
							}

						if (aColumnEvent.ColumnInfo.IsPrimaryKeySegment)
							columnElement.SetAttribute(XML_COLUMN_PRIMARY_SEG_ATTRIBUTE, TrueAttributeValue);

						if (!aColumnEvent.ColumnInfo.IsNullable)
							columnElement.SetAttribute(XML_COLUMN_NULLABLE_VALUE_ATTRIBUTE, Boolean.FalseString.ToLower());

						if (!aColumnEvent.ColumnInfo.IsCollateSensitive)
							columnElement.SetAttribute(XML_COLUMN_COLLATE_SENSITIVE_ATTRIBUTE, Boolean.FalseString.ToLower());

						if (aColumnEvent.ColumnInfo.IsAutoIncrement)
						{
							columnElement.SetAttribute(XML_COLUMN_AUTO_INCREMENT_ATTRIBUTE, Boolean.TrueString.ToLower());
							columnElement.SetAttribute(XML_COLUMN_AUTO_INCREMENT_SEED_ATTRIBUTE, aColumnEvent.ColumnInfo.Seed.ToString());
							columnElement.SetAttribute(XML_COLUMN_AUTO_INCREMENT_INCREMENT_ATTRIBUTE, aColumnEvent.ColumnInfo.Increment.ToString());
						}

						if (aColumnEvent.ColumnInfo.DefaultConstraintName != null && aColumnEvent.ColumnInfo.DefaultConstraintName.Length > 0)
							columnElement.SetAttribute(XML_COLUMN_DEFAULT_CONSTRAINT_NAME_ATTRIBUTE, aColumnEvent.ColumnInfo.DefaultConstraintName);

						if (aColumnEvent.PreviousColumnInfo != null)
						{
							XmlElement previousInfoElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_PREVIOUS);
							columnEventElement.AppendChild(previousInfoElement);
						
							if (aColumnEvent.Type == TableHistoryStep.EventType.AlterColumnType || aColumnEvent.Type == TableHistoryStep.EventType.RenameColumn)
							{
								if (aColumnEvent.Type == TableHistoryStep.EventType.RenameColumn)
									previousInfoElement.SetAttribute(XML_NAME_ATTRIBUTE, aColumnEvent.PreviousColumnInfo.Name);
								
								previousInfoElement.SetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE, WizardTableColumnDataType.Unparse(aColumnEvent.PreviousColumnInfo.DataType));

								if (aColumnEvent.PreviousColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum)
									previousInfoElement.SetAttribute(XML_COLUMN_DATA_ENUM_ATTRIBUTE, aColumnEvent.PreviousColumnInfo.EnumTypeName);
			
								if (aColumnEvent.PreviousColumnInfo.DataLength > 0)
									previousInfoElement.SetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE, aColumnEvent.PreviousColumnInfo.DataLength.ToString(NumberFormatInfo.InvariantInfo));
							}
						
							if (aColumnEvent.Type == TableHistoryStep.EventType.ChangeColumnDefaultValue && aColumnEvent.PreviousColumnInfo.DefaultValue != null)
							{
								string previousDefaultValueString = aColumnEvent.PreviousColumnInfo.DefaultValueString;
								if (previousDefaultValueString != null)
									previousInfoElement.SetAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE, previousDefaultValueString);
							}
							
							if (aColumnEvent.PreviousColumnInfo.IsPrimaryKeySegment)
								previousInfoElement.SetAttribute(XML_COLUMN_PRIMARY_SEG_ATTRIBUTE, TrueAttributeValue);

							if (!string.IsNullOrEmpty(aColumnEvent.PreviousColumnInfo.DefaultConstraintName))
								previousInfoElement.SetAttribute(XML_COLUMN_DEFAULT_CONSTRAINT_NAME_ATTRIBUTE, aColumnEvent.PreviousColumnInfo.DefaultConstraintName);
						}
					}
				}

				if (aStep.IndexesEventsCount > 0)
				{
					foreach(IndexHistoryEvent aIndexEvent in aStep.IndexesEvents)
					{
						if (aIndexEvent == null || aIndexEvent.IndexInfo == null)
							continue;
				
						XmlElement indexEventElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_INDEX_EVENT);
						eventsElement.AppendChild(indexEventElement);

						indexEventElement.SetAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE, aIndexEvent.Type.ToString());

						XmlElement indexInfoElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_INDEX_INFO);
						indexEventElement.AppendChild(indexInfoElement);

						indexInfoElement.SetAttribute(XML_INDEX_NAME_ATTRIBUTE, aIndexEvent.IndexInfo.Name);
						
						if (aIndexEvent.IndexInfo.Primary)
							indexInfoElement.SetAttribute(XML_INDEX_PRIMARY_ATTRIBUTE, TrueAttributeValue);
						
						if (aIndexEvent.IndexInfo.SegmentsCount > 0)
						{
							foreach(WizardTableColumnInfo aSegment in aIndexEvent.IndexInfo.Segments)
							{
								XmlElement segmentElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_INDEX_SEGMENT);
								
								segmentElement.SetAttribute(XML_NAME_ATTRIBUTE, aSegment.Name);
								
								segmentElement.SetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE, WizardTableColumnDataType.Unparse(aSegment.DataType));

								if (aSegment.DataType.Type == WizardTableColumnDataType.DataType.Enum)
									segmentElement.SetAttribute(XML_COLUMN_DATA_ENUM_ATTRIBUTE, aSegment.EnumTypeName);
		
								if (aSegment.DataLength > 0)
									segmentElement.SetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE, aSegment.DataLength.ToString(NumberFormatInfo.InvariantInfo));
		
								if (aSegment.HasSpecificDefaultValue)
								{
									string defaultValueString = aSegment.DefaultValueString;
									if (defaultValueString != null)
										segmentElement.SetAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE, defaultValueString);
								}
				
								if (aSegment.IsPrimaryKeySegment)
									segmentElement.SetAttribute(XML_COLUMN_PRIMARY_SEG_ATTRIBUTE, TrueAttributeValue);

								if (!string.IsNullOrEmpty(aSegment.DefaultConstraintName))
									segmentElement.SetAttribute(XML_COLUMN_DEFAULT_CONSTRAINT_NAME_ATTRIBUTE, aSegment.DefaultConstraintName);
								
								indexInfoElement.AppendChild(segmentElement);
							}
						}
						
						if (aIndexEvent.PreviousIndexInfo != null)
						{
							XmlElement previousIndexInfoElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_INDEX_PREVIOUS_INFO);
							indexEventElement.AppendChild(previousIndexInfoElement);

							previousIndexInfoElement.SetAttribute(XML_INDEX_NAME_ATTRIBUTE, aIndexEvent.PreviousIndexInfo.Name);
						
							if (aIndexEvent.PreviousIndexInfo.Primary)
								previousIndexInfoElement.SetAttribute(XML_INDEX_PRIMARY_ATTRIBUTE, TrueAttributeValue);
						
							if (aIndexEvent.PreviousIndexInfo.SegmentsCount > 0)
							{
								foreach(WizardTableColumnInfo aSegment in aIndexEvent.PreviousIndexInfo.Segments)
								{
									XmlElement segmentElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_INDEX_SEGMENT);
								
									segmentElement.SetAttribute(XML_NAME_ATTRIBUTE, aSegment.Name);
								
									segmentElement.SetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE, WizardTableColumnDataType.Unparse(aSegment.DataType));

									if (aSegment.DataType.Type == WizardTableColumnDataType.DataType.Enum)
										segmentElement.SetAttribute(XML_COLUMN_DATA_ENUM_ATTRIBUTE, aSegment.EnumTypeName);
		
									if (aSegment.DataLength > 0)
										segmentElement.SetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE, aSegment.DataLength.ToString(NumberFormatInfo.InvariantInfo));
		
									if (aSegment.HasSpecificDefaultValue)
									{
										string defaultValueString = aSegment.DefaultValueString;
										if (defaultValueString != null)
											segmentElement.SetAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE, defaultValueString);
									}
				
									if (aSegment.IsPrimaryKeySegment)
										segmentElement.SetAttribute(XML_COLUMN_PRIMARY_SEG_ATTRIBUTE, TrueAttributeValue);

									if (!string.IsNullOrEmpty(aSegment.DefaultConstraintName))
										segmentElement.SetAttribute(XML_COLUMN_DEFAULT_CONSTRAINT_NAME_ATTRIBUTE, aSegment.DefaultConstraintName);
								
									previousIndexInfoElement.AppendChild(segmentElement);
								}
							}
						}
					}
				}
				
				if (aStep.ForeignKeyEventsCount > 0)
				{
					foreach(ForeignKeyHistoryEvent aForeignKeyEvent in aStep.ForeignKeyEvents)
					{
						if (aForeignKeyEvent == null || aForeignKeyEvent.ForeignKeyInfo == null)
							continue;
				
						XmlElement foreignKeyEventElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_FOREIGN_KEY_EVENT);
						eventsElement.AppendChild(foreignKeyEventElement);
		
						foreignKeyEventElement.SetAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE, aForeignKeyEvent.Type.ToString());

						XmlElement foreignKeyInfoElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_FOREIGN_KEY_INFO);
						foreignKeyEventElement.AppendChild(foreignKeyInfoElement);

						foreignKeyInfoElement.SetAttribute(XML_REFERENCED_TABLE_NAMESPACE_ATTRIBUTE, aForeignKeyEvent.ForeignKeyInfo.ReferencedTableNameSpace);
						foreignKeyInfoElement.SetAttribute(XML_FOREIGN_KEY_CONSTRAINT_NAME_ATTRIBUTE, aForeignKeyEvent.ForeignKeyInfo.ConstraintName);

						if (aForeignKeyEvent.Type == TableHistoryStep.EventType.CreateConstraint && aForeignKeyEvent.ForeignKeyInfo.SegmentsCount > 0)
						{
							foreach(WizardForeignKeyInfo.KeySegment aSegmentInfo in aForeignKeyEvent.ForeignKeyInfo.Segments)
							{
								XmlElement segmentElement = projectDocument.CreateElement(XML_TAG_FOREIGN_KEY_SEGMENT);
				
								foreignKeyInfoElement.AppendChild(segmentElement);
					
								segmentElement.SetAttribute(XML_FOREIGN_KEY_SEG_COLUMN_ATTRIBUTE, aSegmentInfo.ColumnName); 
								segmentElement.SetAttribute(XML_FOREIGN_KEY_SEG_REFERENCED_COLUMN_ATTRIBUTE, aSegmentInfo.ReferencedColumnName); 
							}						
						}
					}
				}
				
				historyElement.AppendChild(stepElement);
			}
		}
		
		//---------------------------------------------------------------------------
		private void AddDBTInfoToDBTsNode(XmlElement aDBTsElement, WizardDBTInfo aDBTInfo)
		{
			if (projectDocument == null || aDBTInfo == null || aDBTsElement == null || 
				String.Compare(aDBTsElement.Name, XML_TAG_DBTS) != 0)
				return;

			XmlElement dbtElement = null;
			XmlNode dbtNode = aDBTsElement.SelectSingleNode("child::" + XML_TAG_DBT + "[@" + XML_NAME_ATTRIBUTE +"='" + aDBTInfo.Name + "']");
			if (dbtNode == null || !(dbtNode is XmlElement))
			{
				dbtElement = projectDocument.CreateElement(XML_TAG_DBT);
				aDBTsElement.AppendChild(dbtElement);
			}
			else 
				dbtElement = (XmlElement)dbtNode;

			SetNodeDBTInfo(dbtElement, aDBTInfo);
		}

		//---------------------------------------------------------------------------
		private void SetNodeDBTInfo(XmlElement aDBTElement, WizardDBTInfo aDBTInfo)
		{
			if (projectDocument == null || aDBTInfo == null || aDBTElement == null || 
				String.Compare(aDBTElement.Name, XML_TAG_DBT) != 0)
				return;

			aDBTElement.RemoveAllAttributes();

			aDBTElement.SetAttribute(XML_NAME_ATTRIBUTE, aDBTInfo.Name);
	
			if (aDBTInfo.ReadOnly)
				WriteReadOnly(aDBTElement);

			WriteClassName(aDBTElement, aDBTInfo.ClassName);

			aDBTElement.SetAttribute(XML_TABLE_NAME_ATTRIBUTE, aDBTInfo.TableName);
		
			aDBTElement.SetAttribute(XML_DBT_TYPE_ATTRIBUTE, aDBTInfo.Type.ToString());
		
			if (aDBTInfo.IsSlave || aDBTInfo.IsSlaveBuffered)
			{
				if (aDBTInfo.OnlyForClientDocumentAvailable)
				{
					aDBTElement.SetAttribute(XML_ONLY_FOR_CLIENT_DOCUMENT_ATTRIBUTE, TrueAttributeValue);
					aDBTElement.SetAttribute(XML_MASTER_TABLE_HEADER_ATTRIBUTE, Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(aDBTInfo.Library), aDBTInfo.MasterTableIncludeFile));
					SetProjectNodeChildText(aDBTElement, XML_TAG_SERVER_DOCUMENT, aDBTInfo.ServerDocumentNamespace);
				}
				else if (aDBTInfo.RelatedDBTMaster != null)
					aDBTElement.SetAttribute(XML_REL_DBTMASTER_NAME_ATTRIBUTE, aDBTInfo.RelatedDBTMaster.Name);

				if (!string.IsNullOrEmpty(aDBTInfo.SlaveTabTitle))
					SetProjectNodeChildText(aDBTElement, XML_TAG_DBT_SLAVE_TAB_TITLE, aDBTInfo.SlaveTabTitle);
		
				if (aDBTInfo.IsSlaveBuffered && aDBTInfo.CreateRowForm)
					aDBTElement.SetAttribute(XML_DBT_SLAVEBUFFERED_CREATE_ROW_FORM_ATTRIBUTE, TrueAttributeValue);
			}

			WizardTableInfo tableInfo = aDBTInfo.GetTableInfo();
			if 
				(
				tableInfo != null && 
				tableInfo.IsReferenced &&
				aDBTInfo.ReferencedTableIncludeFile != null &&
				aDBTInfo.ReferencedTableIncludeFile.Trim().Length > 0
				)
				aDBTElement.SetAttribute(XML_REFERENCED_TABLE_HEADER_ATTRIBUTE, Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(aDBTInfo.Library), aDBTInfo.ReferencedTableIncludeFile));

			XmlNode columnsInfoNode = aDBTElement.SelectSingleNode("child::" + XML_TAG_COLUMNS);
			if (aDBTInfo.ColumnsCount == 0)
			{
				if (columnsInfoNode != null && (columnsInfoNode is XmlElement))
					aDBTElement.RemoveChild(columnsInfoNode);

				return;
			}

			XmlElement columnsElement = null;
			if (columnsInfoNode == null || !(columnsInfoNode is XmlElement))
			{
				columnsElement = projectDocument.CreateElement(XML_TAG_COLUMNS);
				aDBTElement.AppendChild(columnsElement);
			}
			else 
				columnsElement = (XmlElement)columnsInfoNode;

			columnsElement.RemoveAll();

			foreach(WizardDBTColumnInfo aColumnInfo in aDBTInfo.ColumnsInfo)
				AddDBTColumnInfoToColumnsNode(columnsElement, aColumnInfo);
		}
		
		//---------------------------------------------------------------------------
		private XmlElement AddDBTColumnInfoToColumnsNode(XmlElement aColumnsElement, WizardDBTColumnInfo aColumnInfo)
		{
			if (projectDocument == null || aColumnInfo == null || aColumnsElement == null || 
				String.Compare(aColumnsElement.Name, XML_TAG_COLUMNS) != 0)
				return null;

			XmlElement columnElement = null;
			XmlNode columnNode = aColumnsElement.SelectSingleNode("child::" + XML_TAG_COLUMN + "[@" + XML_NAME_ATTRIBUTE +"='" + aColumnInfo.ColumnName + "']");
			if (columnNode == null || !(columnNode is XmlElement))
			{
				columnElement = projectDocument.CreateElement(XML_TAG_COLUMN);
				aColumnsElement.AppendChild(columnElement);
			}
			else 
				columnElement = (XmlElement)columnNode;

			SetNodeDBTColumnInfo(columnElement, aColumnInfo);

			return columnElement;
		}

		//---------------------------------------------------------------------------
		private void SetNodeDBTColumnInfo(XmlElement aColumnElement, WizardDBTColumnInfo aColumnInfo)
		{
			if (projectDocument == null || aColumnInfo == null || aColumnElement == null || 
				String.Compare(aColumnElement.Name, XML_TAG_COLUMN) != 0)
				return;

			aColumnElement.RemoveAllAttributes();

			aColumnElement.SetAttribute(XML_NAME_ATTRIBUTE, aColumnInfo.ColumnName);
			
			if (aColumnInfo.ReadOnly)
				WriteReadOnly(aColumnElement);

			aColumnElement.SetAttribute(XML_DBT_COLUMN_VISIBLE_ATTRIBUTE, aColumnInfo.Visible.ToString());
			aColumnElement.SetAttribute(XML_DBT_COLUMN_FINDABLE_ATTRIBUTE, aColumnInfo.Findable.ToString());

			if (aColumnInfo.Position.isSet)
			{
				aColumnElement.SetAttribute(XML_LEFT_ATTRIBUTE, aColumnInfo.Position.Left.ToString());
				aColumnElement.SetAttribute(XML_TOP_ATTRIBUTE, aColumnInfo.Position.Top.ToString());
				aColumnElement.SetAttribute(XML_HEIGHT_ATTRIBUTE, aColumnInfo.Position.Height.ToString());
				aColumnElement.SetAttribute(XML_WIDTH_ATTRIBUTE, aColumnInfo.Position.Width.ToString());
			}

			if (aColumnInfo.ForeignKeySegment)
				aColumnElement.SetAttribute(XML_DBT_COLUMN_RELATEDCOL_ATTRIBUTE, aColumnInfo.ForeignKeyRelatedColumn);
			if (aColumnInfo.IsHKLDefined)
			{
				aColumnElement.SetAttribute(XML_DBT_COLUMN_HKL_CLASS_ATTRIBUTE, aColumnInfo.HotKeyLink.ClassName);
				if (aColumnInfo.ShowHotKeyLinkDescription)
					aColumnElement.SetAttribute(XML_DBT_COLUMN_HKL_SHOW_DESCRIPTION_ATTRIBUTE, TrueAttributeValue);
				if (aColumnInfo.HotKeyLink.IsReferenced)
					AddReferencedHotKeyLinkInfoNode(aColumnInfo.HotKeyLink);
			}

			SetDBTColumnInfoChildText(aColumnElement, XML_TAG_DBTCOLUMN_TITLE, aColumnInfo.Title);
		}

		//---------------------------------------------------------------------------
		private void AddDocumentInfoToDocumentsNode(XmlElement aDocumentsElement, WizardDocumentInfo aDocumentInfo)
		{
			if (projectDocument == null || aDocumentInfo == null || aDocumentsElement == null || 
				String.Compare(aDocumentsElement.Name, XML_TAG_DOCUMENTS) != 0)
				return;

			XmlElement documentElement = null;
			XmlNode documentNode = aDocumentsElement.SelectSingleNode("child::" + XML_TAG_DOCUMENT + "[@" + XML_NAME_ATTRIBUTE +"='" + aDocumentInfo.Name + "']");
			if (documentNode == null || !(documentNode is XmlElement))
			{
				documentElement = projectDocument.CreateElement(XML_TAG_DOCUMENT);
				aDocumentsElement.AppendChild(documentElement);
			}
			else 
				documentElement = (XmlElement)documentNode;

			SetNodeDocumentInfo(documentElement, aDocumentInfo);
		}

		//---------------------------------------------------------------------------
		private void SetNodeDocumentInfo(XmlElement aDocumentElement, WizardDocumentInfo aDocumentInfo)
		{
			if (projectDocument == null || aDocumentInfo == null || aDocumentElement == null || 
				String.Compare(aDocumentElement.Name, XML_TAG_DOCUMENT) != 0)
				return;

			aDocumentElement.RemoveAllAttributes();			
			aDocumentElement.SetAttribute(XML_NAME_ATTRIBUTE, aDocumentInfo.Name);
			aDocumentElement.SetAttribute(XML_HEIGHT_ATTRIBUTE, aDocumentInfo.Height.ToString());
			aDocumentElement.SetAttribute(XML_WIDTH_ATTRIBUTE, aDocumentInfo.Width.ToString());
			
			if (!aDocumentInfo.DefaultViewIsDataEntry)
				aDocumentElement.SetAttribute(XML_DOC_DEFAULT_TYPE_ATTRIBUTE, aDocumentInfo.DefaultType.ToString());

			if (aDocumentInfo.ReadOnly)
				WriteReadOnly(aDocumentElement);
			
			WriteClassName(aDocumentElement, aDocumentInfo.ClassName);
			
			SetDocumentInfoChildText(aDocumentElement, XML_TAG_DOC_TITLE, aDocumentInfo.Title);
			// Label
			AddLabelsNode(aDocumentElement, aDocumentInfo.LabelInfoCollection);
			
			// DBT
			XmlNode dbtsNode = aDocumentElement.SelectSingleNode("child::" + XML_TAG_DBTS);
			if (aDocumentInfo.DBTsCount > 0)
			{
				XmlElement dbtsElement = null;
				if (dbtsNode == null || !(dbtsNode is XmlElement))
				{
					dbtsElement = projectDocument.CreateElement(XML_TAG_DBTS);
					aDocumentElement.AppendChild(dbtsElement);
				}
				else 
					dbtsElement = (XmlElement)dbtsNode;

				dbtsElement.RemoveAll();

				foreach (WizardDBTInfo aDBTInfo in aDocumentInfo.DBTsInfo)
					AddDocumentDBTInfoToDBTsNode(dbtsElement, aDBTInfo);
			}
			else
			{
				if (dbtsNode != null && (dbtsNode is XmlElement))
					aDocumentElement.RemoveChild(dbtsNode);
			}
	
			if (aDocumentInfo.IsHKLDefined)
			{
				XmlElement hotLinkElement = projectDocument.CreateElement(XML_TAG_DOC_HOTLINK);
				hotLinkElement.SetAttribute(XML_NAME_ATTRIBUTE, aDocumentInfo.HKLName);
				WriteClassName(hotLinkElement, aDocumentInfo.ClassName);
				hotLinkElement.SetAttribute(XML_DOC_HOTLINK_CODE_ATTRIBUTE, aDocumentInfo.HKLCodeColumnName);
				hotLinkElement.SetAttribute(XML_DOC_HOTLINK_DESCR_ATTRIBUTE, aDocumentInfo.HKLDescriptionColumnName);
				aDocumentElement.AppendChild(hotLinkElement);
			}
		
			// Tabbed
			XmlNode tabbedPanesNode = aDocumentElement.SelectSingleNode("child::" + XML_TAG_DOC_TABBEDPANES);
			if (aDocumentInfo.TabbedPanesCount > 0)
			{
				XmlElement tabbedPanesElement = null;
				if (tabbedPanesNode == null || !(tabbedPanesNode is XmlElement))
				{
					tabbedPanesElement = projectDocument.CreateElement(XML_TAG_DOC_TABBEDPANES);
					aDocumentElement.AppendChild(tabbedPanesElement);
				}
				else 
					tabbedPanesElement = (XmlElement)tabbedPanesNode;

				tabbedPanesElement.RemoveAll();
				tabbedPanesElement.SetAttribute(XML_LEFT_ATTRIBUTE, aDocumentInfo.TabberSize.Left.ToString());
				tabbedPanesElement.SetAttribute(XML_TOP_ATTRIBUTE, aDocumentInfo.TabberSize.Top.ToString());
				tabbedPanesElement.SetAttribute(XML_HEIGHT_ATTRIBUTE, aDocumentInfo.TabberSize.Height.ToString());
				tabbedPanesElement.SetAttribute(XML_WIDTH_ATTRIBUTE, aDocumentInfo.TabberSize.Width.ToString());

				foreach(WizardDocumentTabbedPaneInfo aTabbedPaneInfo in aDocumentInfo.TabbedPanes)
					AddDocumentTabbedPaneInfoToTabbedPanesNode(tabbedPanesElement, aDocumentInfo, aTabbedPaneInfo);
			}
			else
			{
				if (tabbedPanesNode != null && (tabbedPanesNode is XmlElement))
					aDocumentElement.RemoveChild(tabbedPanesNode);
			}
		}

		//---------------------------------------------------------------------------
		private void AddLabelsNode(XmlElement aDocumentElement, List<LabelInfo> labelInfoCollection)
		{
			XmlNode labelsNode = aDocumentElement.SelectSingleNode("child::" + XML_TAG_LABELS);
			if (labelInfoCollection.Count > 0)
			{
				XmlElement LabelElement = null;
				if (labelsNode == null || !(labelsNode is XmlElement))
				{
					labelsNode = projectDocument.CreateElement(XML_TAG_LABELS);
					aDocumentElement.AppendChild(labelsNode);
				}
				LabelElement = (XmlElement)labelsNode;
				LabelElement.RemoveAll();
				labelInfoCollection.ForEach(l =>
				{
					AddDocumentLabelInfoToLabelsNode(LabelElement, l);
				});
			}
			else
			{
				if (labelsNode != null && (labelsNode is XmlElement))
					aDocumentElement.RemoveChild(labelsNode);
			}
		}

		//---------------------------------------------------------------------------
		private XmlElement AddDocumentLabelInfoToLabelsNode(XmlElement aLabelElement, LabelInfo aLabelInfo)
		{
			if (projectDocument == null || aLabelElement == null || aLabelInfo == null ||
				String.Compare(aLabelElement.Name, XML_TAG_LABELS ) != 0)
				return null;

			XmlElement labelElement = projectDocument.CreateElement(XML_TAG_LABEL);
			labelElement.SetAttribute(XML_TEXT_ATTRIBUTE, aLabelInfo.Label);
			labelElement.SetAttribute(XML_LEFT_ATTRIBUTE, aLabelInfo.Position.Left.ToString());
			labelElement.SetAttribute(XML_TOP_ATTRIBUTE, aLabelInfo.Position.Top.ToString());
			labelElement.SetAttribute(XML_HEIGHT_ATTRIBUTE, aLabelInfo.Position.Height.ToString());
			labelElement.SetAttribute(XML_WIDTH_ATTRIBUTE, aLabelInfo.Position.Width.ToString());
			aLabelElement.AppendChild(labelElement);
			return labelElement;
		}

		//---------------------------------------------------------------------------
		private XmlElement AddDocumentDBTInfoToDBTsNode(XmlElement aDBTsElement, WizardDBTInfo aDBTInfo)
		{
			if (projectDocument == null || aDBTInfo == null || aDBTsElement == null || 
				String.Compare(aDBTsElement.Name, XML_TAG_DBTS) != 0)
				return null;

			XmlElement dbtElement = null;
			XmlNode dbtNode = aDBTsElement.SelectSingleNode("child::" + XML_TAG_DBT + "[@" + XML_NAME_ATTRIBUTE +"='" + aDBTInfo.Name + "']");
			if (dbtNode == null || !(dbtNode is XmlElement))
			{
				dbtElement = projectDocument.CreateElement(XML_TAG_DBT);
				aDBTsElement.AppendChild(dbtElement);
			}
			else 
				dbtElement = (XmlElement)dbtNode;

			SetNodeDocumentDBTInfo(dbtElement, aDBTInfo);
			return dbtElement;
		}

		//---------------------------------------------------------------------------
		private void SetNodeDocumentDBTInfo(XmlElement aDBTElement, WizardDBTInfo aDBTInfo)
		{
			if (projectDocument == null || aDBTInfo == null || aDBTElement == null || 
				String.Compare(aDBTElement.Name, XML_TAG_DBT) != 0)
				return;

			aDBTElement.RemoveAllAttributes();
			aDBTElement.SetAttribute(XML_NAME_ATTRIBUTE, aDBTInfo.Name);

			if (aDBTInfo.BodyEditPosition.isSet)
			{
				aDBTElement.SetAttribute(XML_LEFT_ATTRIBUTE, aDBTInfo.BodyEditPosition.Left.ToString());
				aDBTElement.SetAttribute(XML_TOP_ATTRIBUTE, aDBTInfo.BodyEditPosition.Top.ToString());
				aDBTElement.SetAttribute(XML_HEIGHT_ATTRIBUTE, aDBTInfo.BodyEditPosition.Height.ToString());
				aDBTElement.SetAttribute(XML_WIDTH_ATTRIBUTE, aDBTInfo.BodyEditPosition.Width.ToString());
			}
		}
		
		//---------------------------------------------------------------------------
		private XmlElement AddDocumentTabbedPaneInfoToTabbedPanesNode(XmlElement aTabbedPanesElement, WizardDocumentInfo aDocumentInfo, WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if (projectDocument == null || aTabbedPaneInfo == null || 
				aTabbedPaneInfo.DBTInfo == null || string.IsNullOrEmpty(aTabbedPaneInfo.DBTInfo.Name) || 
				aTabbedPanesElement == null || String.Compare(aTabbedPanesElement.Name, XML_TAG_DOC_TABBEDPANES) != 0)
				return null;

			XmlNodeList dbtTabbedPanesList = aTabbedPanesElement.SelectNodes("child::" + XML_TAG_DOC_TABBEDPANE + "[@" + XML_TABBED_PANE_DBT_NAME_ATTRIBUTE +"='" + aTabbedPaneInfo.DBTInfo.Name + "']");
			if (dbtTabbedPanesList != null && dbtTabbedPanesList.Count > 0)
			{
				// Se c'è già un nodo che raccoglie le stesse informazioni non ne devo aggiungere
				// un altro...
				foreach(XmlNode sameDBTTabbedPaneNode in dbtTabbedPanesList)
				{
					if (sameDBTTabbedPaneNode == null || !(sameDBTTabbedPaneNode is XmlElement))
						continue;
					
					WizardDocumentTabbedPaneInfo tmpTabbedPane = ParseTabbedPaneInfo(aDocumentInfo.Library, (XmlElement)sameDBTTabbedPaneNode);

					if (tmpTabbedPane != null && tmpTabbedPane.Equals(aTabbedPaneInfo))
						return (XmlElement)sameDBTTabbedPaneNode;
				}
			}
			
			XmlElement tabbedPaneElement = projectDocument.CreateElement(XML_TAG_DOC_TABBEDPANE);
			aTabbedPanesElement.AppendChild(tabbedPaneElement);

			tabbedPaneElement.SetAttribute(XML_TABBED_PANE_DBT_NAME_ATTRIBUTE, aTabbedPaneInfo.DBTName);
			if (aTabbedPaneInfo.ReadOnly)
				WriteReadOnly(tabbedPaneElement);

			XmlElement titleElement = projectDocument.CreateElement(XML_TAG_TABBED_PANE_TITLE);
			titleElement.InnerText = aTabbedPaneInfo.Title;
			tabbedPaneElement.AppendChild(titleElement);

			if (aTabbedPaneInfo.ManagedColumnsCount > 0)
			{
				XmlElement columnsElement = projectDocument.CreateElement(XML_TAG_COLUMNS);
				tabbedPaneElement.AppendChild(columnsElement);
				
				foreach(WizardDBTColumnInfo aColumnInfo in aTabbedPaneInfo.ManagedColumns)
				{
					XmlElement columnElement = projectDocument.CreateElement(XML_TAG_COLUMN);
					columnsElement.AppendChild(columnElement);
	
					SetNodeDBTColumnInfo(columnElement, aColumnInfo);
				}
			}

			// Labels in pannel
			AddLabelsNode(tabbedPaneElement, aTabbedPaneInfo.LabelInfoCollection);

			return tabbedPaneElement;
		}

		//---------------------------------------------------------------------------
		private XmlElement AddClientDocumentTabbedPaneInfoToTabbedPanesNode(XmlElement aTabbedPanesElement, WizardClientDocumentInfo aClientDocInfo, WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if (projectDocument == null || aTabbedPaneInfo == null || 
				aTabbedPaneInfo.DBTInfo == null || string.IsNullOrEmpty(aTabbedPaneInfo.DBTInfo.Name) || 
				aTabbedPanesElement == null || String.Compare(aTabbedPanesElement.Name, XML_TAG_DOC_TABBEDPANES) != 0)
				return null;

			XmlNodeList dbtTabbedPanesList = aTabbedPanesElement.SelectNodes("child::" + XML_TAG_DOC_TABBEDPANE + "[@" + XML_TABBED_PANE_DBT_NAME_ATTRIBUTE +"='" + aTabbedPaneInfo.DBTInfo.Name + "']");
			if (dbtTabbedPanesList != null && dbtTabbedPanesList.Count > 0)
			{
				// Se c'è già un nodo che raccoglie le stesse informazioni non ne devo aggiungere
				// un altro...
				foreach(XmlNode sameDBTTabbedPaneNode in dbtTabbedPanesList)
				{
					if (sameDBTTabbedPaneNode == null || !(sameDBTTabbedPaneNode is XmlElement))
						continue;
					
					WizardDocumentTabbedPaneInfo tmpTabbedPane = ParseTabbedPaneInfo(aClientDocInfo.Library, (XmlElement)sameDBTTabbedPaneNode);

					if (tmpTabbedPane != null && tmpTabbedPane.Equals(aTabbedPaneInfo))
						return (XmlElement)sameDBTTabbedPaneNode;
				}
			}
			
			XmlElement tabbedPaneElement = projectDocument.CreateElement(XML_TAG_DOC_TABBEDPANE);
			aTabbedPanesElement.AppendChild(tabbedPaneElement);

			tabbedPaneElement.SetAttribute(XML_TABBED_PANE_DBT_NAME_ATTRIBUTE, aTabbedPaneInfo.DBTName);
			
			XmlElement titleElement = projectDocument.CreateElement(XML_TAG_TABBED_PANE_TITLE);
			titleElement.InnerText = aTabbedPaneInfo.Title;
			tabbedPaneElement.AppendChild(titleElement);

			if (aTabbedPaneInfo.ManagedColumnsCount > 0)
			{
				XmlElement columnsElement = projectDocument.CreateElement(XML_TAG_COLUMNS);
				tabbedPaneElement.AppendChild(columnsElement);
				
				foreach(WizardDBTColumnInfo aColumnInfo in aTabbedPaneInfo.ManagedColumns)
				{
					XmlElement columnElement = projectDocument.CreateElement(XML_TAG_COLUMN);
					columnsElement.AppendChild(columnElement);
	
					SetNodeDBTColumnInfo(columnElement, aColumnInfo);
				}
			}

			return tabbedPaneElement;
		}

		//---------------------------------------------------------------------------
		private void AddClientDocumentInfoToDocumentsNode(XmlElement aClientDocumentsElement, WizardClientDocumentInfo aClientDocInfo)
		{
			if 
				(
				projectDocument == null ||
				aClientDocInfo == null || 
				aClientDocInfo.ServerDocumentInfo == null ||
				aClientDocumentsElement == null || 
				String.Compare(aClientDocumentsElement.Name, XML_TAG_CLIENT_DOCUMENTS) != 0
				)
				return;

			XmlElement clientDocElement = null;
			XmlNode clientDocNode = aClientDocumentsElement.SelectSingleNode("child::" + XML_TAG_CLIENT_DOCUMENT + "[@" + XML_NAME_ATTRIBUTE +"='" + aClientDocInfo.Name + "']");
			if (clientDocNode == null || !(clientDocNode is XmlElement))
			{
				clientDocElement = projectDocument.CreateElement(XML_TAG_CLIENT_DOCUMENT);
				aClientDocumentsElement.AppendChild(clientDocElement);
			}
			else 
				clientDocElement = (XmlElement)clientDocNode;

			SetNodeClientDocumentInfo(clientDocElement, aClientDocInfo);
		}

		//---------------------------------------------------------------------------
		private void SetNodeClientDocumentInfo(XmlElement aClientDocumentElement, WizardClientDocumentInfo aClientDocInfo)
		{
			if 
				(
				projectDocument == null ||
				aClientDocInfo == null || 
				aClientDocInfo.ServerDocumentInfo == null ||
				aClientDocumentElement == null || 
				String.Compare(aClientDocumentElement.Name, XML_TAG_CLIENT_DOCUMENT) != 0
				)
				return;

			aClientDocumentElement.RemoveAllAttributes();
			
			aClientDocumentElement.SetAttribute(XML_NAME_ATTRIBUTE, aClientDocInfo.Name);

			if (aClientDocInfo.ReadOnly)
				WriteReadOnly(aClientDocumentElement);

			WriteClassName(aClientDocumentElement, aClientDocInfo.ClassName);
			
			SetClientDocumentInfoChildText(aClientDocumentElement, XML_TAG_DOC_TITLE, aClientDocInfo.Title);
			
			XmlElement serverDocElement = SetClientDocumentInfoChildText(aClientDocumentElement, XML_TAG_SERVER_DOCUMENT, aClientDocInfo.ServerDocumentInfo.GetNameSpace());
			if (serverDocElement != null)
			{
				serverDocElement.RemoveAllAttributes();
				
				string familyClass = aClientDocInfo.FamilyToAttachClassName;
				if (!string.IsNullOrEmpty(familyClass))
				{
					serverDocElement.SetAttribute(XML_SERVER_DOC_FAMILY_ATTRIBUTE, familyClass);
					if (aClientDocInfo.ExcludeBatchMode)
						serverDocElement.SetAttribute(XML_SERVER_DOC_EXCLUDE_BATCH_ATTRIBUTE, TrueAttributeValue);
				}
			}
							
			aClientDocumentElement.SetAttribute(XML_CLIENT_DOC_SLAVEFORMVIEW_ATTRIBUTE, aClientDocInfo.CreateSlaveFormView.ToString());
			aClientDocumentElement.SetAttribute(XML_CLIENT_DOC_ADDTABDIALOGS_ATTRIBUTE, aClientDocInfo.AddTabDialogs.ToString());

			if (aClientDocInfo.ExcludeUnattendedMode)
				aClientDocumentElement.SetAttribute(XML_SERVER_DOC_NO_UNATTENDED_ATTRIBUTE, TrueAttributeValue);
										
			XmlNode includeFilesNode = aClientDocumentElement.SelectSingleNode("child::" + XML_TAG_INCLUDE_FILES);
			if (aClientDocInfo.ServerHeaderFilesToincludeCount == 0)
			{
				if (includeFilesNode != null && (includeFilesNode is XmlElement))
					aClientDocumentElement.RemoveChild(includeFilesNode);
			}
			else
			{
				string libraryPath = WizardCodeGenerator.GetStandardLibraryPath(aClientDocInfo.Library);
				if (!string.IsNullOrEmpty(libraryPath))
				{
					XmlElement includeFilesElement = null;
					if (includeFilesNode == null || !(includeFilesNode is XmlElement))
					{
						includeFilesElement = projectDocument.CreateElement(XML_TAG_INCLUDE_FILES);
						aClientDocumentElement.AppendChild(includeFilesElement);
					}
					else 
						includeFilesElement = (XmlElement)includeFilesNode;

					includeFilesElement.RemoveAll();
			
					string[] headers = aClientDocInfo.ServerHeaderFilesToinclude;
					if (headers != null && headers.Length > 0)
					{
						foreach(string aHeaderFile in headers)
						{
							if (aHeaderFile == null || aHeaderFile.Trim().Length == 0)
								continue;

							XmlElement headerElement = projectDocument.CreateElement(XML_TAG_INCLUDE_FILE);
							headerElement.InnerText = Generics.MakeRelativeTo(libraryPath, aHeaderFile);
							includeFilesElement.AppendChild(headerElement);
						}
					}
				}
			}
			
			XmlNode dbtsNode = aClientDocumentElement.SelectSingleNode("child::" + XML_TAG_DBTS);
			if (aClientDocInfo.DBTsCount > 0)
			{
				XmlElement dbtsElement = null;
				if (dbtsNode == null || !(dbtsNode is XmlElement))
				{
					dbtsElement = projectDocument.CreateElement(XML_TAG_DBTS);
					aClientDocumentElement.AppendChild(dbtsElement);
				}
				else 
					dbtsElement = (XmlElement)dbtsNode;

				dbtsElement.RemoveAll();
								
				foreach(WizardDBTInfo aDBTInfo in aClientDocInfo.DBTsInfo)
					AddDocumentDBTInfoToDBTsNode(dbtsElement, aDBTInfo);
			}
			else
			{
				if (dbtsNode != null && (dbtsNode is XmlElement))
					aClientDocumentElement.RemoveChild(dbtsNode);
			}
	
			XmlNode tabbedPanesNode = aClientDocumentElement.SelectSingleNode("child::" + XML_TAG_DOC_TABBEDPANES);
			if (aClientDocInfo.TabbedPanesCount > 0)
			{
				XmlElement tabbedPanesElement = null;
				if (tabbedPanesNode == null || !(tabbedPanesNode is XmlElement))
				{
					tabbedPanesElement = projectDocument.CreateElement(XML_TAG_DOC_TABBEDPANES);
					aClientDocumentElement.AppendChild(tabbedPanesElement);
				}
				else 
					tabbedPanesElement = (XmlElement)tabbedPanesNode;

				tabbedPanesElement.RemoveAll();

				foreach(WizardDocumentTabbedPaneInfo aTabbedPaneInfo in aClientDocInfo.TabbedPanes)
					AddClientDocumentTabbedPaneInfoToTabbedPanesNode(tabbedPanesElement, aClientDocInfo, aTabbedPaneInfo);
			}
			else
			{
				if (tabbedPanesNode != null && (tabbedPanesNode is XmlElement))
					aClientDocumentElement.RemoveChild(tabbedPanesNode);
			}
		}
		
		//---------------------------------------------------------------------------
		private void AddDependencyToDependenciesNode(XmlElement aDependenciesElement, WizardLibraryInfo aDependency)
		{
			if (projectDocument == null ||
				aDependency == null || 
				aDependency.Module == null || 
				aDependenciesElement == null || 
				String.Compare(aDependenciesElement.Name, XML_TAG_DEPENDENCIES) != 0)
				return;

			XmlElement dependencyElement = null;
			XmlNode dependencyNode = aDependenciesElement.SelectSingleNode("child::" + XML_TAG_DEPENDENCY + "[@" + XML_MODULE_NAME_ATTRIBUTE +" = '" + aDependency.Module.Name + "' and @" + XML_LIBRARY_NAME_ATTRIBUTE +" = '" + aDependency.Name + "']");
			if (dependencyNode == null || !(dependencyNode is XmlElement))
			{
				dependencyElement = projectDocument.CreateElement(XML_TAG_DEPENDENCY);
				aDependenciesElement.AppendChild(dependencyElement);
			}
			else 
				dependencyElement = (XmlElement)dependencyNode;

			dependencyElement.RemoveAllAttributes();

			// Se la libreria che si vuole inserire nelle dipendenze non appartiene
			// all'applicazione definita nel progetto, bensì ad un'applicazione 
			// referenziata, devo specificarne l'origine
			if (applicationInfo != null &&
				aDependency.Application != null &&
				String.Compare(applicationInfo.Name, aDependency.Application.Name) != 0)
				dependencyElement.SetAttribute(XML_APPLICATION_NAME_ATTRIBUTE, aDependency.Application.Name);

			dependencyElement.SetAttribute(XML_MODULE_NAME_ATTRIBUTE, aDependency.Module.Name);
			dependencyElement.SetAttribute(XML_LIBRARY_NAME_ATTRIBUTE, aDependency.Name);
		}

		//---------------------------------------------------------------------------
		protected virtual void AddExtraAddedColumnInfoToColumnsNode(XmlElement aExtraAddedColumnsElement, WizardExtraAddedColumnsInfo aExtraAddedColumnInfo)
		{
			if (projectDocument == null || 
				aExtraAddedColumnInfo == null || 
				aExtraAddedColumnInfo.ColumnsCount == 0 || 
				string.IsNullOrEmpty(aExtraAddedColumnInfo.TableNameSpace) ||
				aExtraAddedColumnsElement == null || 
				String.Compare(aExtraAddedColumnsElement.Name, XML_TAG_EXTRA_ADDED_COLUMNS) != 0)
				return;

			XmlElement extraAddedColumnElement = null;
			
			XmlNodeList extraAddedColumnsList = aExtraAddedColumnsElement.SelectNodes("child::" + XML_TAG_EXTRA_ADDED_COLUMN);
			if (extraAddedColumnsList != null && extraAddedColumnsList.Count > 0)
			{
				foreach(XmlNode extraAddedColumnNode in extraAddedColumnsList)
				{
					if (extraAddedColumnNode == null || 
						!(extraAddedColumnNode is XmlElement) ||
						!((XmlElement)extraAddedColumnNode).HasAttribute(XML_TABLE_NAMESPACE_ATTRIBUTE))
						continue;
					
					string tableNameSpaceText = ((XmlElement)extraAddedColumnNode).GetAttribute(XML_TABLE_NAMESPACE_ATTRIBUTE);
					if (tableNameSpaceText == null)
						continue;

					tableNameSpaceText = tableNameSpaceText.Trim();
					if (tableNameSpaceText.Length == 0)
						continue;

					NameSpace tableNameSpace = new NameSpace(tableNameSpaceText, NameSpaceObjectType.Table);
					if (!tableNameSpace.IsValid())
						continue;

					if (String.Compare(tableNameSpaceText, aExtraAddedColumnInfo.TableNameSpace) == 0)
					{
						extraAddedColumnElement = (XmlElement)extraAddedColumnNode;
						break;
					}
				}
			}

			if (extraAddedColumnElement == null)
			{
				extraAddedColumnElement = projectDocument.CreateElement(XML_TAG_EXTRA_ADDED_COLUMN);
				aExtraAddedColumnsElement.AppendChild(extraAddedColumnElement);
			}
			
			SetNodeExtraAddedColumnsInfo(extraAddedColumnElement, aExtraAddedColumnInfo);

			SetNodeExtraAddedColumnsHistoryInfo(extraAddedColumnElement, aExtraAddedColumnInfo);
		}

		//---------------------------------------------------------------------------
		protected virtual void SetNodeExtraAddedColumnsInfo(XmlElement aExtraAddedColumnElement, WizardExtraAddedColumnsInfo aExtraAddedColumnInfo)
		{
			if (projectDocument == null || aExtraAddedColumnInfo == null ||
				string.IsNullOrEmpty(aExtraAddedColumnInfo.TableNameSpace) || aExtraAddedColumnInfo.ColumnsCount == 0 || 
				aExtraAddedColumnElement == null || String.Compare(aExtraAddedColumnElement.Name, XML_TAG_EXTRA_ADDED_COLUMN) != 0)
				return;

			aExtraAddedColumnElement.RemoveAllAttributes();
			
			aExtraAddedColumnElement.SetAttribute(XML_TABLE_NAMESPACE_ATTRIBUTE, aExtraAddedColumnInfo.TableNameSpace);

			aExtraAddedColumnElement.SetAttribute(XML_LOCALIZE_ATTRIBUTE, aExtraAddedColumnInfo.TableNameSpace);

			if (aExtraAddedColumnInfo.ReadOnly)
				WriteReadOnly(aExtraAddedColumnElement);

			if (!string.IsNullOrEmpty(aExtraAddedColumnInfo.ClassName))
				WriteClassName(aExtraAddedColumnElement, aExtraAddedColumnInfo.ClassName);

			aExtraAddedColumnElement.SetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE, aExtraAddedColumnInfo.CreationDbReleaseNumber.ToString(NumberFormatInfo.InvariantInfo));

			WizardTableInfo originalTableInfo = aExtraAddedColumnInfo.GetOriginalTableInfo();
			if (originalTableInfo != null && originalTableInfo.IsReferenced &&
				aExtraAddedColumnInfo.ReferencedTableIncludeFile != null &&
				aExtraAddedColumnInfo.ReferencedTableIncludeFile.Trim().Length > 0)
				aExtraAddedColumnElement.SetAttribute(XML_REFERENCED_TABLE_HEADER_ATTRIBUTE, Generics.MakeRelativeTo(WizardCodeGenerator.GetStandardLibraryPath(aExtraAddedColumnInfo.Library), aExtraAddedColumnInfo.ReferencedTableIncludeFile));

			XmlElement columnsElement = null;
			XmlNode columnsInfoNode = aExtraAddedColumnElement.SelectSingleNode("child::" + XML_TAG_COLUMNS);
			if (columnsInfoNode == null || !(columnsInfoNode is XmlElement))
			{
				columnsElement = projectDocument.CreateElement(XML_TAG_COLUMNS);
				aExtraAddedColumnElement.AppendChild(columnsElement);
			}
			else 
				columnsElement = (XmlElement)columnsInfoNode;

			columnsElement.RemoveAll();
			
			foreach(WizardTableColumnInfo aColumnInfo in aExtraAddedColumnInfo.ColumnsInfo)
			{
				XmlElement columnElement = null;
				XmlNode columnNode = columnsElement.SelectSingleNode("child::" + XML_TAG_COLUMN + "[@" + XML_NAME_ATTRIBUTE +"='" + aColumnInfo.Name + "']");
				if (columnNode == null || !(columnNode is XmlElement))
				{
					columnElement = projectDocument.CreateElement(XML_TAG_COLUMN);
					columnsElement.AppendChild(columnElement);
				}
				else 
					columnElement = (XmlElement)columnNode;

				SetNodeTableColumnInfo(columnElement, aColumnInfo, aExtraAddedColumnInfo.CreationDbReleaseNumber);
			}
		}
		
		//---------------------------------------------------------------------------
		private void SetNodeExtraAddedColumnsHistoryInfo(XmlElement aExtraAddedColumnElement, WizardExtraAddedColumnsInfo aExtraAddedColumnInfo)
		{
			if (projectDocument == null || aExtraAddedColumnInfo == null ||
				string.IsNullOrEmpty(aExtraAddedColumnInfo.TableNameSpace) ||
				aExtraAddedColumnInfo.ColumnsCount == 0 || aExtraAddedColumnElement == null || 
				String.Compare(aExtraAddedColumnElement.Name, XML_TAG_EXTRA_ADDED_COLUMN) != 0)
				return;
		
			XmlNode historyInfoNode = aExtraAddedColumnElement.SelectSingleNode("child::" + XML_TAG_TABLE_HISTORY);

			if (aExtraAddedColumnInfo.History == null || aExtraAddedColumnInfo.History.StepsCount == 0)
			{
				if (historyInfoNode != null && (historyInfoNode is XmlElement))
					aExtraAddedColumnElement.RemoveChild(historyInfoNode);

				return;
			}

			XmlElement historyElement = null;
			if (historyInfoNode == null || !(historyInfoNode is XmlElement))
			{
				historyElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY);
				aExtraAddedColumnElement.AppendChild(historyElement);
			}
			else 
				historyElement = (XmlElement)historyInfoNode;

			historyElement.RemoveAll();

			foreach(TableHistoryStep aStep in aExtraAddedColumnInfo.History.Steps)
			{
				if (aStep == null || aStep.DbReleaseNumber <= 0 || aStep.EventsCount == 0)
					continue;

				XmlElement stepElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_STEP);

				stepElement.SetAttribute(XML_DB_REL_NUMBER_ATTRIBUTE, aStep.DbReleaseNumber.ToString(NumberFormatInfo.InvariantInfo));
				
				// Salvataggio delle informazioni relative alle singole modifiche
				XmlElement eventsElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_EVENTS);
				stepElement.AppendChild(eventsElement);

				if (aStep.ColumnsEventsCount > 0)
				{
					foreach(ColumnHistoryEvent aColumnEvent in aStep.ColumnsEvents)
					{
						if (aColumnEvent == null || aColumnEvent.ColumnInfo == null)
							continue;

						XmlElement columnEventElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_COLUMN_EVENT);
						eventsElement.AppendChild(columnEventElement);

						columnEventElement.SetAttribute(XML_COL_HISTORY_EVENT_TYPE_ATTRIBUTE, aColumnEvent.Type.ToString());
					
						if (aColumnEvent.ColumnOrder >= 0)
							columnEventElement.SetAttribute(XML_COL_HISTORY_EVENT_ORDER_ATTRIBUTE, aColumnEvent.ColumnOrder.ToString(NumberFormatInfo.InvariantInfo));
						if (aColumnEvent.PreviousColumnOrder >= 0)
							columnEventElement.SetAttribute(XML_COL_HISTORY_EVENT_PREVIOUS_ORDER_ATTRIBUTE, aColumnEvent.PreviousColumnOrder.ToString(NumberFormatInfo.InvariantInfo));
			
						XmlElement columnElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_COLUMN);
						columnEventElement.AppendChild(columnElement);
				
						columnElement.SetAttribute(XML_NAME_ATTRIBUTE, aColumnEvent.ColumnInfo.Name);
			
						columnElement.SetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE, WizardTableColumnDataType.Unparse(aColumnEvent.ColumnInfo.DataType));

						if (aColumnEvent.ColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum)
							columnElement.SetAttribute(XML_COLUMN_DATA_ENUM_ATTRIBUTE, aColumnEvent.ColumnInfo.EnumTypeName);
			
						if (aColumnEvent.ColumnInfo.DataLength > 0)
							columnElement.SetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE, aColumnEvent.ColumnInfo.DataLength.ToString(NumberFormatInfo.InvariantInfo));

						if (!aColumnEvent.ColumnInfo.IsNullable)
							columnElement.SetAttribute(XML_COLUMN_NULLABLE_VALUE_ATTRIBUTE, Boolean.FalseString.ToLower());

						if (!aColumnEvent.ColumnInfo.IsCollateSensitive)
							columnElement.SetAttribute(XML_COLUMN_COLLATE_SENSITIVE_ATTRIBUTE, Boolean.FalseString.ToLower());

						if (aColumnEvent.ColumnInfo.IsAutoIncrement)
						{
							columnElement.SetAttribute(XML_COLUMN_AUTO_INCREMENT_ATTRIBUTE, Boolean.TrueString.ToLower());
							columnElement.SetAttribute(XML_COLUMN_AUTO_INCREMENT_SEED_ATTRIBUTE, aColumnEvent.ColumnInfo.Seed.ToString());
							columnElement.SetAttribute(XML_COLUMN_AUTO_INCREMENT_INCREMENT_ATTRIBUTE, aColumnEvent.ColumnInfo.Increment.ToString());
						}

						if (aColumnEvent.ColumnInfo.HasSpecificDefaultValue)
						{
							string defaultValueString = aColumnEvent.ColumnInfo.DefaultValueString;
							if (defaultValueString != null)
								columnElement.SetAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE, defaultValueString);
						}
					
						if (aColumnEvent.PreviousColumnInfo != null)
						{
							XmlElement previousInfoElement = projectDocument.CreateElement(XML_TAG_TABLE_HISTORY_PREVIOUS);
							columnEventElement.AppendChild(previousInfoElement);
						
							if (aColumnEvent.Type == TableHistoryStep.EventType.AlterColumnType || aColumnEvent.Type == TableHistoryStep.EventType.RenameColumn)
							{
								if (aColumnEvent.Type == TableHistoryStep.EventType.RenameColumn)
									previousInfoElement.SetAttribute(XML_NAME_ATTRIBUTE, aColumnEvent.PreviousColumnInfo.Name);
								
								previousInfoElement.SetAttribute(XML_COLUMN_DATATYPE_ATTRIBUTE, WizardTableColumnDataType.Unparse(aColumnEvent.PreviousColumnInfo.DataType));

								if (aColumnEvent.PreviousColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.Enum)
									previousInfoElement.SetAttribute(XML_COLUMN_DATA_ENUM_ATTRIBUTE, aColumnEvent.PreviousColumnInfo.EnumTypeName);
			
								if (aColumnEvent.PreviousColumnInfo.DataLength > 0)
									previousInfoElement.SetAttribute(XML_COLUMN_LENGTH_ATTRIBUTE, aColumnEvent.PreviousColumnInfo.DataLength.ToString(NumberFormatInfo.InvariantInfo));
							}
						
							if (aColumnEvent.Type == TableHistoryStep.EventType.ChangeColumnDefaultValue && aColumnEvent.PreviousColumnInfo.DefaultValue != null)
							{
								string previousDefaultValueString = aColumnEvent.PreviousColumnInfo.DefaultValueString;
								if (previousDefaultValueString != null)
									previousInfoElement.SetAttribute(XML_COLUMN_DEFAULT_VALUE_ATTRIBUTE, previousDefaultValueString);
							}
						}
					}
				}

				historyElement.AppendChild(stepElement);
			}
		}
		
		//---------------------------------------------------------------------------
		private void AddReferencedHotKeyLinkInfoNode(WizardHotKeyLinkInfo aHotLink)
		{
			if (projectDocument == null || aHotLink == null || !aHotLink.IsReferenced)
				return;
	
			try
			{
				XmlNode applicationInfoNode = projectDocument.DocumentElement.SelectSingleNode("child::" + XML_TAG_APPLICATION);
				if (applicationInfoNode == null || !(applicationInfoNode is XmlElement))
				{
					XmlElement applicationInfoElement = projectDocument.CreateElement(XML_TAG_APPLICATION);
					applicationInfoNode = projectDocument.DocumentElement.AppendChild(applicationInfoElement);
				}

				XmlNode referencesInfoNode = applicationInfoNode.SelectSingleNode("child::" + XML_TAG_REFERENCES);
				if (referencesInfoNode == null || !(referencesInfoNode is XmlElement))
				{
					XmlElement referenceHotLinksInfoElement = projectDocument.CreateElement(XML_TAG_REFERENCES);
					referencesInfoNode = applicationInfoNode.AppendChild(referenceHotLinksInfoElement);
				}

				XmlNode hotLinkInfoNode = referencesInfoNode.SelectSingleNode("child::" + XML_TAG_REFERENCED_HOTLINK + "[@" + XML_REFERENCED_HOTLINK_NAMESPACE_ATTRIBUTE + "='" + aHotLink.ReferencedNameSpace + "']");
				if (hotLinkInfoNode == null || !(hotLinkInfoNode is XmlElement))
					hotLinkInfoNode = referencesInfoNode.AppendChild(projectDocument.CreateElement(XML_TAG_REFERENCED_HOTLINK));
				
				XmlElement hotLinkElement = (XmlElement)hotLinkInfoNode;

				hotLinkElement.SetAttribute(XML_REFERENCED_HOTLINK_NAMESPACE_ATTRIBUTE, aHotLink.ReferencedNameSpace);
				hotLinkElement.SetAttribute(XML_NAME_ATTRIBUTE, aHotLink.Name);
				WriteClassName(hotLinkElement, aHotLink.ClassName);

				if (aHotLink.ExternalIncludeFile != null && aHotLink.ExternalIncludeFile.Trim().Length > 0)
				{
                    string applicationContainerPath = WizardCodeGenerator.GetStandardTaskBuilderApplicationContainerPath();
					
					string relativeIncludeFileName = Generics.MakeRelativeTo(applicationContainerPath, aHotLink.ExternalIncludeFile);
					hotLinkElement.SetAttribute(XML_REFERENCED_HOTLINK_INCLUDE_ATTRIBUTE, relativeIncludeFileName);
				}
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in TBWizardProjectParser.AddReferencedHotKeyLinkInfoNode." + exception.Message);
			}
		}

		//---------------------------------------------------------------------------
		private static bool IsApplicationInfoValidChildTag(string childTag)
		{
			return 
				(!string.IsNullOrEmpty(childTag) &&
				(
				String.Compare(childTag, XML_TAG_APP_TITLE) == 0 ||
				String.Compare(childTag, XML_TAG_APP_PRODUCER) == 0 ||
				String.Compare(childTag, XML_TAG_APP_DBSIGNATURE) == 0 ||
				String.Compare(childTag, XML_TAG_APP_VERSION) == 0 ||
				String.Compare(childTag, XML_TAG_APP_SHORTNAME) == 0 ||
				String.Compare(childTag, XML_TAG_APP_EDITION) == 0 ||
				String.Compare(childTag, XML_TAG_APP_SOLUTION_TYPE) == 0 ||
				String.Compare(childTag, XML_TAG_APP_CULTURE) == 0 ||
				String.Compare(childTag, XML_TAG_APP_FONT) == 0 ||
				String.Compare(childTag, XML_TAG_APP_GUID) == 0 ||
				String.Compare(childTag, XML_TAG_REFERENCES) == 0 ||
				String.Compare(childTag, XML_TAG_MODULES) == 0
				)
				);
		}
		
		//---------------------------------------------------------------------------
		private static bool IsModuleInfoValidChildTag(string childTag)
		{
			return
				(!string.IsNullOrEmpty(childTag) &&
				(
				String.Compare(childTag, XML_TAG_MOD_TITLE) == 0 ||
				String.Compare(childTag, XML_TAG_MOD_DBSIGNATURE) == 0 ||
				String.Compare(childTag, XML_TAG_MOD_DBRELEASE) == 0 ||
				String.Compare(childTag, XML_TAG_ENUMS) == 0 ||
				String.Compare(childTag, XML_TAG_LIBRARIES) == 0
				)
				);
		}
		
		//---------------------------------------------------------------------------
		private static bool IsLibraryInfoValidChildTag(string childTag)
		{
			return
				(!string.IsNullOrEmpty(childTag) &&
				(
				String.Compare(childTag, XML_TAG_LIB_SOURCEFOLDER) == 0 ||
				String.Compare(childTag, XML_TAG_LIB_MENUTITLE) == 0 ||
				String.Compare(childTag, XML_TAG_LIB_GUID) == 0 ||
				String.Compare(childTag, XML_TAG_FIRST_RESOURCE_ID) == 0 ||
				String.Compare(childTag, XML_TAG_FIRST_CONTROL_ID) == 0 ||
				String.Compare(childTag, XML_TAG_FIRST_COMMAND_ID) == 0 ||
				String.Compare(childTag, XML_TAG_FIRST_SYMED_ID) == 0 ||
				String.Compare(childTag, XML_TAG_TABLES) == 0 ||
				String.Compare(childTag, XML_TAG_DBTS) == 0 ||
				String.Compare(childTag, XML_TAG_DOCUMENTS) == 0 ||
				String.Compare(childTag, XML_TAG_CLIENT_DOCUMENTS) == 0 ||
				String.Compare(childTag, XML_TAG_DEPENDENCIES) == 0 ||
				String.Compare(childTag, XML_TAG_EXTRA_ADDED_COLUMNS) == 0
				)
				);
		}

		//---------------------------------------------------------------------------
		private static bool IsTableInfoValidChildTag(string childTag)
		{
			return
				(!string.IsNullOrEmpty(childTag) &&
				(
				String.Compare(childTag, XML_TAG_COLUMNS) == 0 ||
				String.Compare(childTag, XML_TAG_TABLE_HISTORY) == 0
				)
				);
		}

		//---------------------------------------------------------------------------
		private static bool IsDocumentInfoValidChildTag(string childTag)
		{
			return
				(!string.IsNullOrEmpty(childTag) &&
				(
				String.Compare(childTag, XML_TAG_DOC_TITLE) == 0 ||
				String.Compare(childTag, XML_TAG_FIRST_RESOURCE_ID) == 0 ||
				String.Compare(childTag, XML_TAG_FIRST_CONTROL_ID) == 0 ||
				String.Compare(childTag, XML_TAG_FIRST_COMMAND_ID) == 0 ||
				String.Compare(childTag, XML_TAG_FIRST_SYMED_ID) == 0 ||
				String.Compare(childTag, XML_TAG_DBTS) == 0 ||
				String.Compare(childTag, XML_TAG_DOC_HOTLINK) == 0
				)
				);
		}

		//---------------------------------------------------------------------------
		private static bool IsClientDocumentInfoValidChildTag(string childTag)
		{
			return
				(!string.IsNullOrEmpty(childTag) &&
				(
				String.Compare(childTag, XML_TAG_DOC_TITLE) == 0 ||
				String.Compare(childTag, XML_TAG_SERVER_DOCUMENT) == 0
				)
				);
		}

		//---------------------------------------------------------------------------
		private static bool IsDBTColumnInfoValidChildTag(string childTag)
		{
			return (!string.IsNullOrEmpty(childTag) && String.Compare(childTag, XML_TAG_DBTCOLUMN_TITLE) == 0);
		}

		//---------------------------------------------------------------------------
		private WizardDocumentInfo GetDocumentInfoFromNamespace(string aDocumentNamespaceText)
		{
			if (string.IsNullOrEmpty(aDocumentNamespaceText))
				return null;

			NameSpace documentNamespace = new NameSpace(aDocumentNamespaceText, NameSpaceObjectType.Document);

			string documentApplicationName = documentNamespace.Application;
			if (string.IsNullOrEmpty(documentApplicationName))
				return null;

			WizardApplicationInfo documentApplicationInfo = null;
			
			if (applicationInfo != null && String.Compare(applicationInfo.Name, documentApplicationName) == 0)
			{
				documentApplicationInfo = applicationInfo;
			}
			else if (referencedApplicationsInfo != null && referencedApplicationsInfo.Count > 0)
			{
				foreach(WizardApplicationInfo aReferencedApplication in referencedApplicationsInfo)
				{
					if (String.Compare(aReferencedApplication.Name, documentApplicationName) == 0)
					{
						documentApplicationInfo = aReferencedApplication;
						break;
					}
				}
			}
			if (documentApplicationInfo == null || documentApplicationInfo.ModulesCount == 0)
				return null;

			string documentModuleName = documentNamespace.Module;
			if (string.IsNullOrEmpty(documentModuleName))
				return null;

			string documentLibrarySourceFolder = documentNamespace.Library;
			if (string.IsNullOrEmpty(documentLibrarySourceFolder))
				return null;

			string documentName = documentNamespace.Document;
			if (string.IsNullOrEmpty(documentName))
				return null;

			WizardDocumentInfo documentInfo = null;
		
			foreach(WizardModuleInfo aModuleInfo in documentApplicationInfo.ModulesInfo)
			{
				if (String.Compare(aModuleInfo.Name, documentModuleName) == 0)
				{
					if (aModuleInfo.LibrariesCount > 0)
					{
						foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
						{
							if (String.Compare(aLibraryInfo.SourceFolder, documentLibrarySourceFolder) == 0)
							{
								documentInfo = aLibraryInfo.GetDocumentInfoByName(documentName);
								break;
							}
						}
					}
					break;
				}
			}

			return documentInfo;
		}
		
		#endregion
		
		#region TBWizardProjectParser public properties
        //---------------------------------------------------------------------------
        protected virtual string RootTag { get { return "TBWizardProject"; } }
		//---------------------------------------------------------------------------
		public string ProjectFileName { get { return projectFileName; } set { if( String.IsNullOrEmpty(projectFileName)) projectFileName = value;} }
		//---------------------------------------------------------------------------
		public WizardApplicationInfo ApplicationInfo { get { return applicationInfo; } }
		//---------------------------------------------------------------------------
		public WizardApplicationInfoCollection ReferencedApplicationsInfo { get { return referencedApplicationsInfo; } set { referencedApplicationsInfo = value; }}
		//---------------------------------------------------------------------------
		public bool IsModified { get { return isModified; } }

		#endregion

		#region TBWizardProjectParser public methods

		//---------------------------------------------------------------------------
		public void SetApplicationInfo(WizardApplicationInfo aApplicationInfo)
		{
			XmlElement applicationInfoNode = GetApplicationInfoNode();
			if (applicationInfoNode == null)
				return;

			applicationInfo = aApplicationInfo;
			
			SetNodeApplicationInfo(applicationInfoNode);
	
			isModified = true;
		}

		//---------------------------------------------------------------------------
		public bool AddModuleInfo(WizardModuleInfo aModuleInfo)
		{
			if (applicationInfo == null || aModuleInfo == null || string.IsNullOrEmpty(aModuleInfo.Name))
				return false;

			XmlElement applicationInfoNode = GetApplicationInfoNode();
			if (applicationInfoNode == null)
				return false;

			int addedIdx = applicationInfo.AddModuleInfo(aModuleInfo);
			if (addedIdx == -1)
				return false;
			
			AddModuleInfoToApplicationNode(applicationInfoNode, aModuleInfo);

			isModified = true;

			return true;
		}
		
		//---------------------------------------------------------------------------
		public void UpdateModuleInfo(WizardModuleInfo aModuleInfo)
		{
			if (applicationInfo == null || aModuleInfo == null || aModuleInfo.Application == null ||
				!WizardModuleInfo.Equals(applicationInfo, aModuleInfo.Application))
				return;

			XmlElement moduleInfoNode = GetModuleInfoNode(aModuleInfo);
			if (moduleInfoNode == null)
				return;

			SetNodeModuleInfo(moduleInfoNode, aModuleInfo);

			isModified = true;
		}
		
		//---------------------------------------------------------------------------
		public bool AddLibraryInfo(WizardModuleInfo aModuleInfo, WizardLibraryInfo aLibraryInfo)
		{
			if (applicationInfo == null || aModuleInfo == null || aLibraryInfo == null || string.IsNullOrEmpty(aLibraryInfo.Name))
				return false;

			XmlElement moduleInfoNode = GetModuleInfoNode(aModuleInfo);
			if (moduleInfoNode == null)
				return false;

			int addedIdx = aModuleInfo.AddLibraryInfo(aLibraryInfo);
			if (addedIdx == -1)
				return false;
			
			AddLibraryInfoToModuleNode(moduleInfoNode, aLibraryInfo);

			isModified = true;

			return true;
		}
		
		//---------------------------------------------------------------------------
		public void SetLibraryInfo(WizardLibraryInfo aLibraryInfo)
		{
			if (applicationInfo == null || aLibraryInfo == null)
				return;

			XmlElement libraryInfoNode = GetLibraryInfoNode(aLibraryInfo);
			if (libraryInfoNode == null)
				return;

			SetNodeLibraryInfo(libraryInfoNode, aLibraryInfo);

			isModified = true;
		}
		
		//---------------------------------------------------------------------------
		public void UpdateLibraryTableInfo(WizardTableInfo aTableInfo)
		{
			if (applicationInfo == null || aTableInfo == null || aTableInfo.Library == null)
				return;

			XmlElement libraryInfoNode = GetLibraryInfoNode(aTableInfo.Library);
			if (libraryInfoNode == null)
				return;

			SetNodeLibraryInfo(libraryInfoNode, aTableInfo.Library);

			isModified = true;
		}
		
		//---------------------------------------------------------------------------
		public void UpdateLibraryTablesInfo(WizardLibraryInfo aLibraryInfo)
		{
			if (applicationInfo == null || aLibraryInfo == null || aLibraryInfo.Module == null)
				return;

			XmlElement libraryInfoNode = GetLibraryInfoNode(aLibraryInfo);
			if (libraryInfoNode == null)
				return;

			SetNodeLibraryTablesInfo(libraryInfoNode, aLibraryInfo);

			isModified = true;
		}
		
		//---------------------------------------------------------------------------
		public void UpdateLibraryDBTsInfo(WizardLibraryInfo aLibraryInfo)
		{
			if (applicationInfo == null || aLibraryInfo == null || aLibraryInfo.Module == null)
				return;

			XmlElement libraryInfoNode = GetLibraryInfoNode(aLibraryInfo);
			if (libraryInfoNode == null)
				return;

			SetNodeLibraryDBTsInfo(libraryInfoNode, aLibraryInfo);

			isModified = true;
		}

		//---------------------------------------------------------------------------
		public void UpdateLibraryDBTInfo(WizardDBTInfo aDBTInfo)
		{
			if (applicationInfo == null || aDBTInfo == null || aDBTInfo.Library == null)
				return;

			XmlElement libraryInfoNode = GetLibraryInfoNode(aDBTInfo.Library);
			if (libraryInfoNode == null)
				return;

			SetNodeLibraryInfo(libraryInfoNode, aDBTInfo.Library);

			// Se si tratta di un DBTMaster devo anche prestare attenzione al fatto che
			// nel caso in cui esso possieda degli slave va modificato l'attributo del
			// nome del loro Master che potrebbe, infatti, essere cambiato
			if (aDBTInfo.IsMaster && applicationInfo.ExistsAttachedDBTSlaves(aDBTInfo))
			{
				WizardDBTInfoCollection attachedSlaves = applicationInfo.GetAttachedDBTSlaves(aDBTInfo);
				if (attachedSlaves != null && attachedSlaves.Count > 0)
				{
					foreach(WizardDBTInfo aSlaveInfo in attachedSlaves)
						UpdateLibraryDBTInfo(aSlaveInfo);
				}
			}

			isModified = true;
		}
		
		//---------------------------------------------------------------------------
		public void UpdateLibraryDocumentInfo(WizardDocumentInfo aDocumentInfo)
		{
			if (applicationInfo == null || aDocumentInfo == null || aDocumentInfo.Library == null)
				return;

			XmlElement libraryInfoNode = GetLibraryInfoNode(aDocumentInfo.Library);
			if (libraryInfoNode == null)
				return;

			SetNodeLibraryDocumentsInfo(libraryInfoNode, aDocumentInfo.Library);

			isModified = true;
		}
		
		//---------------------------------------------------------------------------
		public void UpdateLibraryClientDocumentsInfo(WizardLibraryInfo aLibraryInfo)
		{
			if (applicationInfo == null || aLibraryInfo == null || aLibraryInfo.Module == null)
				return;

			XmlElement libraryInfoNode = GetLibraryInfoNode(aLibraryInfo);
			if (libraryInfoNode == null)
				return;

			SetNodeLibraryClientDocumentsInfo(libraryInfoNode, aLibraryInfo);

			isModified = true;
		}

		//---------------------------------------------------------------------------
		public void UpdateLibraryClientDocumentInfo(WizardClientDocumentInfo aClientDocumentInfo)
		{
			if (applicationInfo == null || aClientDocumentInfo == null || aClientDocumentInfo.Library == null)
				return;

			UpdateLibraryClientDocumentsInfo(aClientDocumentInfo.Library);
		}
		
		//---------------------------------------------------------------------------
		public void UpdateModuleEnumsInfo(WizardModuleInfo aModuleInfo)
		{
			if (applicationInfo == null || aModuleInfo == null)
				return;

			XmlElement moduleInfoNode = GetModuleInfoNode(aModuleInfo);
			if (moduleInfoNode == null)
				return;

			SetModuleEnumsInfo(moduleInfoNode, aModuleInfo);

			isModified = true;
		}
		
		//---------------------------------------------------------------------------
		public void UpdateLibraryExtraColumnsInfo(WizardLibraryInfo aLibraryInfo)
		{
			if 
				(
				applicationInfo == null || 
				aLibraryInfo == null
				)
				return;

			XmlElement libraryInfoNode = GetLibraryInfoNode(aLibraryInfo);
			if (libraryInfoNode == null)
				return;

			SetNodeLibraryExtraAddedColumnsInfo(libraryInfoNode, aLibraryInfo);

			isModified = true;
		}

		//---------------------------------------------------------------------------
		public void UpdateDocumentUserInterface(WizardDocumentInfo aDocumentInfo)
		{
			if (applicationInfo == null || aDocumentInfo == null || aDocumentInfo.Library == null)
				return;

			XmlElement libraryInfoNode = GetLibraryInfoNode(aDocumentInfo.Library);
			if (libraryInfoNode == null)
				return;

			SetNodeLibraryDocumentsInfo(libraryInfoNode, aDocumentInfo.Library);

			isModified = true;
		}
	
		//---------------------------------------------------------------------------
		public bool Open(string aProjectFileName)
		{
			return ParseFile(aProjectFileName);
		}
		
		//---------------------------------------------------------------------------
		public bool Save()
		{
			return UnparseFile();
		}
		
		//---------------------------------------------------------------------------
		public bool SaveAs(string aProjectFileName)
		{
			projectFileName = aProjectFileName;

			return Save();
		}
		#endregion
	}
}
