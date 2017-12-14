
namespace Microarea.TaskBuilderNet.Data.DataManagerEngine
{
	/// <summary>
	/// DataManagerConsts
	/// Costanti stringa utilizzate dal DataManager
	/// </summary>
	//=========================================================================
	public class DataManagerConsts
	{
		// stringhe costanti
		public const string NamespaceDataManagerImg		= "Microarea.Console.Core.DataManager.Images";
		public const string NamespaceDBAdminPlugIn		= "Module.MicroareaConsole.ApplicationDBAdmin";

		// per HELP		
		public const string NamespaceDataManagerCommon	= "Microarea.Console.Core.DataManager.Common.";
		public const string NamespaceDataManagerDefault	= "Microarea.Console.Core.DataManager.Default.";
		public const string NamespaceDataManagerSample	= "Microarea.Console.Core.DataManager.Sample.";
		public const string NamespaceDataManagerImport	= "Microarea.Console.Core.DataManager.Import.";
		public const string NamespaceDataManagerExport	= "Microarea.Console.Core.DataManager.Export.";
		
		public const string ExportDataFileName		= "ExportData.xml";
		public const string Basic					= "Basic";
		public const string ManufacturingAdvanced	= "Manufacturing-Advanced";

		public const string ExportDateTime			= "yyyy-MM-dd-HH-mm-ss";
		public const string Append					= "Append";
		public const string Schema					= "xs:schema";
	
		 // tag utilizzati per il parsing dei file xml per l'import/export
		public const string DataTables				= "DataTables";
		public const string Optional				= "optional";

		// attributi per la gestione dei reference tra i file
		public const string HasReference			= "hasreference";
		public const string RefConfiguration		= "refconfiguration";
		public const string RefEdition				= "refedition";

		// tipi nodo nei controlli del wizard
		public const string ApplicationNode			= "Application";
		public const string ModuleNode				= "Module";
		public const string TableNode				= "Table";
		public const string NoExistTableNode		= "NoExistTable";
		public const string FileNode				= "File";
		public const string AppendFileNode			= "AppendFile";
		public const string ColumnNode				= "Column";
		public const string DirectoryNode			= "Directory";

		public const string SetParseOnlyOn			= "set parseonly on";
		public const string SetParseOnlyOff			= "set parseonly off";
	}

	/// <summary>
	/// Classe per gestire i nomi degli elementi e degli attributi dei file per pre-caricare le selezioni
	/// </summary>
	//=============================================================================        
	public class ConfigurationInfoXml
	{
		//================================================================================
		public class Element
		{
			public const string DefaultExportSelections	= "DefaultExportSelections";

			public const string Parameters			= "Parameters";
			public const string Configuration		= "Configuration";
			public const string CountryCode			= "CountryCode";
			public const string DBProvider			= "DBProvider";
			public const string ObjectsSelection	= "ObjectsSelection";
			public const string MandatoryColumns	= "MandatoryColumns";
			public const string Script				= "Script";

			public const string ExportedObjects		= "ExportedObjects";
			public const string Application			= "Application";
			public const string Module				= "Module";
			public const string Table				= "Table";
			public const string Column				= "Column";
		}

		//================================================================================
		public class Attribute
		{
			public const string Value			= "value";
			public const string Name			= "name";
			public const string AllTables		= "allTables";
			public const string AllColumns		= "allColumns";
			public const string WhereClause		= "whereClause";
			public const string ColTBCreated	= "colTBCreated";
			public const string ColTBModified	= "colTBModified";
			public const string Execute			= "execute";
		}
	}
}
