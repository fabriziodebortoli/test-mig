using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

namespace Microarea.TaskBuilderNet.Interfaces
{
	/// <summary>
	/// Wrapper per i metodi che restituiscono solo path della lib namesolver.
	/// </summary>
	//=========================================================================
	public interface IBasePathFinder
	{
		int TbLoaderSOAPPort { get; }
		int TbLoaderTCPPort { get; }

		string	Build				{ get; }
		IList	ApplicationInfos	{ get; }
		string	LoginManagerUrl		{ get; }
		string	LockManagerUrl		{ get; }
		string	TbServicesUrl		{ get; }
		string	EasyLookServiceUrl	{ get; }
		string	Installation		{ get; }
		string	RemoteFileServer	{ get; }
		string	RemoteWebServer		{ get; }
		string	ServerConnectionFile { get; }
        string  ProductVersion      { get; }
        DateTime  ProductDate         { get; }
			
		string		FindSplashFile						(string fileName);
		string		GetApplicationModulePath				(string applicationName, string moduleName);
		string		GetStandardModulePath				(INameSpace aNameSpace);
		string		GetStandardModuleTextPath			(INameSpace aNameSpace);
		string		GetCustomModulePath					(string companyName, string applicationName, string moduleName);
		string		GetCustomUserReportFile				(string companyName, string userName, INameSpace ns, bool recursive);
		ICollection	GetModulesList						(string applicationName);

		INameSpace	GetNamespaceFromPath				(string sObjectFullPath);

		bool		IsCustomPath						(string aPath);
		bool		IsStandardPath						(string aPath);
		
		FileInfo[]	GetBrandFiles						();

		string		GetDbtsPath							(INameSpace aNameSpace);

		string		GetCustomPath						();
		string		GetCustomCompaniesPath				();
		string GetCustomCompanyPath(string companyName);
		string		GetCustomCompanyLogPath				(string companyName);
		string		GetModuleConfigFullName(string appName, string moduleName);
		IBaseModuleInfo GetModuleInfo					(INameSpace aNameSpace);
		string		GetApplicationConfigFullName		(string appName);
		bool		GetApplicationsList					(ApplicationType applicationType, out StringCollection applicationList);

		string		GetStandardApplicationPath			(string applicationName);
		string		GetStandardTaskBuilderXmlPath		();
		string		GetStandardModuleDictionaryFilePath (string appName, string moduleName, string culture);
		string		GetStandardDatabaseScriptPath		(string application, string module);
		string		GetStandardUpgradeInfoXML			(string application, string module);
		string		GetStandardCreateInfoXML			(string application, string module);
		string		GetStandardScriptPath				(string path, string nameScript, string provider, bool create, int rel);
		string		GetInstallationPath						();
		string		GetStandardPath							();
		string		GetStandardDocumentPath					(INameSpace aNameSpace);
		string		GetStandardDocumentDescriptionPath		(INameSpace aNameSpace);
		string		GetStandardDocumentExportprofilesPath	(INameSpace aNameSpace);
		string		GetStandardApplicationContainerPath		(ApplicationType aApplicationType);
		string		GetStandardDataManagerPath				(string application, string module);
		string		GetStandardDataManagerDefaultPath		(string application, string module, string language, string edition);
		
		//abbiamo diverse tipologie di esempio: Standard, Manufacturing e libero x l'utente
		string		GetStandardDataManagerSamplePath		(string application, string module, string language, string edition);
		string		GetApplicationModuleObjectsPath			(string applicationName, string moduleName);

		string		GetStandardUIControllerFilePath		(string applicationName, string module, string migrationFileName);
		string		GetStandardUIControllerFilePath		(INameSpace aNameSpace, string migrationFileName);
		
		string		GetStandardUIControllerPath			(string applicationName, string module);
		string		GetStandardUIControllerPath			(INameSpace aNameSpace);

		string		GetTBLoaderPath();
		string		GetTempPath							();
		string		GetWebProxyFilesPath				();
		string		GetWebProxyImagesPath				();
		
		string		GetDictionaryFilePathFromWoormFile	(string woormFilePath);
		string		GetDictionaryFilePathFromTableName	(string tableName);

        IDocumentInfo       GetDocumentInfo				(INameSpace aNameSpace);

		string		GetUserInfoFile						();
		FileInfo[]	GetLicensedFiles					();
		string		GetLicensedFile						(string productName);

		string		GetMessagesQueuePath				();

		string		GetCustomDocumentPath				(string companyName, string application, string module, string document);
		string		GetCustomReportPathFromNamespace	(INameSpace ns, string companyName, string userName);
		string		GetCustomReportPathFromWoormFile	(string woormFilePath, string companyName, string userName);
		string		GetLogManAppDataPath				();

		IBaseApplicationInfo GetApplicationInfoByName	(string applicationName);
		IBaseModuleInfo GetModuleInfoByName				(string applicationName, string moduleName);

		FileInfo[]	GetSolutionFiles					();
		string		GetSolutionFile						(string productName);
		string		GetAppInfoVersionFromSolutionName(string productName);
		string		GetLoginMngSessionFile				();
		string		GetDatabaseObjectsBinPath			();
		string		GetSolutionsModulesPath(string productName);
		string		GetProxiesFilePath();

		bool IsApplicationDirectory(string appPath);
		bool IsModuleDirectory(string modulePath);
		string GetApplicationContainerName(ApplicationType applicationType);

		string GetCustomModuleTextPath(string companyName, string userName, string appName, string modName);
		string GetCustomApplicationPath(string companyName, string applicationName);

		string SearchImageInAppFolder(string imageFile);

        void LoadPrototypes();
        IFunctions WebFunctions { get; }

        string GetGenericBrandFilePath();
    }

	/// <summary>
	/// Tipo di applicazione che PathFinder deve cercare.
	/// </summary>
	//=========================================================================
	[Flags]
	public enum ApplicationType
	{
		Undefined = 0,
		TaskBuilderNet = 1,
		TaskBuilderApplication = 2,
		TaskBuilder = 4,
		Customization = 8 ,
		Standardization = 16,
		All = TaskBuilder | TaskBuilderApplication | TaskBuilderNet | Customization | Standardization
	}

	//---------------------------------------------------------------------------
	public enum OfficeType { Word, Excel, All, None };

}
