using System.Collections;


namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	/// <summary>
	/// Corrispettivo del file Module.config che decrive un modulo di un 'applicazione
	/// per TaskBuilder.
	/// </summary>
	public interface IBaseModuleInfo
	{
		IClientDocumentsObjectInfo ClientDocumentsObjectInfo { get; }
		string DictionaryFilePath { get; }
		IDocumentsObjectInfo DocumentObjectsInfo { get; }
		IList Documents { get; }
		string GetActionsInfoFile();
		string GetAddOnDatabaseObjectsPath();
		string GetClientDocumentObjectsPath();
		string GetClientRelativeOutputPath(string build);
		string GetCustomCompanyReportPath(string companyName);
		string GetCustomPath(string companyName);
		string GetCustomReportPath(string companyName, string userName);
		string GetDatabaseObjectsPath();
		string GetDatabaseScriptPath();
		string GetDataMigrationLogPath();
		string GetDBInfoPath();
		string GetDictionaryPath();
		IDocumentInfo GetDocumentInfoByNameSpace(string nameSpace);
		IDocumentInfo GetDocumentInfoByTitle(string documentTitle);
		string GetDocumentObjectsPath();
		string GetDocumentPath(string documentName);
		string GetEnumsIniPath();
		string GetEnumsPath();
		string GetEventHandlerObjectsPath();
		string GetFontsFullFilename();
		string GetFormatsFullFilename();
		ILibraryInfo GetLibraryInfoByName(string libraryName);
		ILibraryInfo GetLibraryInfoByPath(string aLibraryPath);
		IList GetLibraryInfosBySourceFolderName(string sourceFolderName);
		string GetModuleObjectPath();
		string GetCustomModuleObjectPath();
		string GetOutDateObjectsPath();
		string GetProviderCreateScriptPath(string provider);
		string GetReferenceObjectFileName(INameSpace ns);
		string GetReferenceObjectsPath();
		string GetServerOutPutPath(string build);
		string[] GetSettingsFiles(string path);
		string GetStandardDatabaseScriptPath();
		string GetStandardDocumentSchemaFilesPath(string documentName);
		string GetStandardDocumentSchemaFullFilename(string documentName, string schemaName);
		string GetStandardExcelDocumentFullFilename(string document, string language);
		string GetStandardExcelFilesPath();
		string GetStandardExcelFilesPath(string language);
		string GetStandardExcelTemplateFullFilename(string template, string language);
		string GetStandardHelpPath();
		string GetStandardImageFullFilename(string image);
		string GetStandardImagePath();
		string GetStandardReportFullFilename(string reportName);
		string GetStandardReportPath();
		string GetStandardReportSchemaFilesPath();
		string GetStandardReportSchemaFullFilename(string schemaName);
		string GetStandardSettingsFullFilename(string settings);
		string GetStandardSettingsPath();
		string GetStandardTextFullFilename(string text);
		string GetStandardTextPath();
		string GetStandardWordDocumentFullFilename(string document, string language);
		string GetStandardWordFilesPath();
		string GetStandardWordFilesPath(string language);
		string GetStandardWordTemplateFullFilename(string template, string language);
		string GetWebMethodsPath();
		IList Libraries { get; }
		IModuleConfigInfo ModuleConfigInfo { get; }
		string Name { get; set; }
		IBaseApplicationInfo ParentApplicationInfo { get; }
		string ParentApplicationName { get; }
		string Path { get; }
		IBasePathFinder PathFinder { get; }
		string Title { get; }
		INameSpace NameSpace { get; }
		IList WebMethods { get; }
        IFunctionPrototype GetFunctionPrototipeByNameSpace(string nameSpace);

		string[] XTechDocuments { get; }

		IDatabaseObjectsInfo DatabaseObjectsInfo { get; } 
		IAddOnDatabaseObjectsInfo AddOnDatabaseObjectsInfo { get; } 
		IDBObjects DBObjects { get; }
		int CurrentDBRelease { get; set; }
	}

	//----------------------------------------------------------------------------
	public enum ActionWhen
	{
		ClientUpdate,
		Uknown
	}
}
