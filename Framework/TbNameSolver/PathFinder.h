#pragma once

#include "TbNamespaces.h"
#include "MacroToRedifine.h"


//includere alla fine degli include del .H
#include "beginh.dex"

///////////////////////////////////////////////////////////////////////////////
// definisce il collegamento fra l'oggetto C++ passato al processore XSLT
// ed il corrispondente riferimento a tale oggetto nel file di trasformazione 
// XMLStringLoader.xsl; è importante che questa #define e la corrispondente
// stringa di namespace definita nel suddetto file rimangano allineati
#define PATH_FINDER_URN	_T("urn:TBPathFinder")
#define _PREDEFINED(s) _T(s)

// nomi di directory
TB_EXPORT extern const TCHAR szAllUserDirName[];
TB_EXPORT extern const TCHAR szStandard[];
TB_EXPORT extern const TCHAR szPredefined[];

// directory containers
TB_EXPORT extern const TCHAR szSettingsExt[];
TB_EXPORT extern const TCHAR szExtensionsApp[];
TB_EXPORT extern const TCHAR szXmlExt[];
TB_EXPORT extern const TCHAR szTaskBuilderApp[];
TB_EXPORT extern const TCHAR szTBLoader[];
TB_EXPORT extern const TCHAR szDbInfo[];
TB_EXPORT extern const TCHAR szSettingsConfigFile[];
TB_EXPORT extern const TCHAR szCandidateModulesSep[];
TB_EXPORT extern const TCHAR szModuleObjects[];

TB_EXPORT extern const TCHAR szWebMethods[];
TB_EXPORT extern const TCHAR szReportsXML[];
TB_EXPORT extern const TCHAR szBarcodeXML[];
TB_EXPORT extern const TCHAR szDocumentObjects[];
TB_EXPORT extern const TCHAR szJsonForms[];

class DataDate;
//////////////////////////////////////////////////////////////////////
//					CDictionaryPathFinderObj							//
//////////////////////////////////////////////////////////////////////
class TB_EXPORT CDictionaryPathFinderObj
{
	friend class CPathFinder;

protected:
	CPathFinder *m_pPathFinder;

public:

	CDictionaryPathFinderObj() : m_pPathFinder(NULL){}

	virtual void	GetDictionaryPathsFromString(LPCTSTR lpcszString, CStringArray &paths) = 0;
	virtual void	GetDictionaryPathsFromID(UINT nIDD, LPCTSTR lpszType, CStringArray &paths) = 0;
	virtual CString	GetDictionaryPathFromNamespace(const CTBNamespace& aNamespace, BOOL bStandard) = 0;
	virtual CString	GetDictionaryPathFromTableName(const CString& strTableName) = 0;
	virtual CString	GetDllNameFromNamespace(const CTBNamespace& aNamespace) = 0;
	virtual void GetDictionaryPathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &paths) = 0;
	virtual void GetModulePathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &paths) = 0;

};


#define QUERY_METADATA		0
#define FETCH_METADATA		1

///////////////////////////////////////////////////////////////////////////////
//								TBMetadataManager
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT TBFile : public CObject
{
public:
	long		m_FileID;
	long		m_ParentID;
	CString		m_strName;
	CString		m_strFileType;
	CString		m_strPathName;
	CString		m_strNamespace;
	CString		m_strAppName;
	CString		m_strModuleName;
	CString		m_ObjectType;
	long		m_FileSize;
	BYTE*		m_pFileContent;
	CString		m_strFileContent; //contenuto del file di tipo testo
	CString		m_strCompleteFileName;
	BOOL		m_bIsCustomPath;
	CTime		m_CreationTime;
	CTime		m_LastWriteTime;
	BOOL		m_IsReadOnly;
	BOOL		m_IsDirectory;

	//serve per la custom
	CString		m_strAccountName;

public:
	TBFile(const CString& strCompleteFileName);
	TBFile(const CString& strName, const CString& strPathName);
	~TBFile();

	CString GetContentAsString();
	BYTE*	GetContentAsBinary();
};

//enum StorageType { FileSystem, Database };
////////////////////////////////////////////////////////////////////////
////					TBMetadataManagerObj							//
////////////////////////////////////////////////////////////////////////
//class TB_EXPORT TBMetadataManagerObj
//{
//	friend class CPathFinder;
//
//protected:
//	CPathFinder* m_pPathFinder;
//	StorageType  m_eStorageType;
//
//public:
//	TBMetadataManagerObj(CPathFinder* pPathFinder)
//		:
//		m_pPathFinder(pPathFinder)
//	{
//	}
//
//public:
//	virtual BOOL ExistMetadataDirectory(const CString& strPath) const = 0;
//	virtual void CreateMetadataDirectory(const CString& strPathName) const = 0;	
//	
//	virtual void  GetMetadataNameInDirectory(const CString& strPathName, const CString& extensionsType, CStringArray* pFoundedFile) const = 0;
//	virtual void GetAllApplicationInfo(CObArray*)  = 0;
//	virtual void GetAllModuleInfo(TBFile*, CObArray*)  = 0;
//	
//	virtual BOOL ExistMetadataFile(const CString& strFileName)  const = 0;
//
//	virtual TBFile* GetMetadataFile(const CString& strPathFileName)  = 0;
//	virtual void RemoveMetadataFile(const CString& strPathFileName) = 0;
//	virtual void SaveMetadataTextFile(const CString& strPathFileName, const CString& fileTextContent) = 0;
//
//
//public:
//	virtual void StartTimeOperation(int) {}
//	virtual void StopTimeOperation(int) {}
//
//	virtual CString GetFormattedQueryTime() { return _T(""); }
//	virtual CString GetFormattedConvertTime() { return _T(""); }
//
//public:
//	StorageType GetStorageType() const { return m_eStorageType;	}
//};
//
//
////////////////////////////////////////////////////////////////////////
////					TBFileSystemManager
////////////////////////////////////////////////////////////////////////
//class TB_EXPORT TBFileSystemManager : public TBMetadataManagerObj
//{
//	friend class CPathFinder;
//
//public:
//	TBFileSystemManager(CPathFinder* pPathFinder)
//	:
//	TBMetadataManagerObj(pPathFinder)
//	{
//		m_eStorageType = StorageType::FileSystem;
//	}
//
//private:
//	void AddApplicationDirectories(const CString& sAppContainerPath, CObArray* pReturnArray) const;
//	void AddApplicationModules(const CString& sApplicationPath, CObArray* pReturnArray, bool isCustom ) const;
//
//public:
//	virtual BOOL ExistMetadataDirectory(const CString& strPathName) const;
//	virtual void CreateMetadataDirectory(const CString& strPathName) const;
//	virtual void GetMetadataNameInDirectory(const CString& strPathName, const CString& extensionsType, CStringArray* pFoundedFile) const;
//	virtual void GetAllApplicationInfo(CObArray*);
//	virtual void GetAllModuleInfo(TBFile*, CObArray*);
//	
//	virtual BOOL ExistMetadataFile(const CString& strFileName) const;
//	virtual TBFile* GetMetadataFile(const CString& strFileName);
//	virtual void RemoveMetadataFile(const CString& strPathFileName);
//	virtual void SaveMetadataTextFile(const CString& strPathFileName, const CString& fileTextContent) {};
//};


//==============================================================================
class TB_EXPORT CPathFinder : public CObject
{
	friend class CApplicationsLoader;
	friend class CDeveloperConfigManager;
	friend class CClientObjects;
	friend class TBFileSystemManager;

private:
	CString		m_sTbDllPath;
	CString		m_sServerName;
	CString		m_sStandardPath;
	CString		m_sCustomPath;
	CString		m_sInstallation;
	CString		m_sWebServiceInstallation;
	CString		m_sMasterSolution;
	BOOL		m_bIsStandAlone;

	CDictionaryPathFinderObj *m_pDictionaryPathFinder;
	//TBMetadataManagerObj*	  m_pMetadataManager;

	// contains the base candidate applications and modules that are passed to ApplicationLoader.
	// They are not the real loaded applications, as running applications are rappresented by AddOnMng
	// who has checked all the correct parameters and applications/modules descriptions.
	CMapStringToString	m_ApplicationContainerMap;
	CMapStringToString	m_ApplicationsModulesMap;

	/// Indica se il programma sta girando all'interno del percorso di installazione (Apps o Standard ad es. per i web services)
	BOOL				m_bIsRunningInsideInstallation;

public:
	enum PosType		{ STANDARD, CUSTOM, ALL_USERS, USERS, ROLES };
	enum Company		{ CURRENT, ALL_COMPANIES };
	enum ApplicationType{ UNDEFINED, TB_APPLICATION, TB, CUSTOMIZATION, STANDARDIZATION };

public:
	CPathFinder();
	~CPathFinder();

private:
	CString GetCompanyName() const;
	CString GetUserName() const;

	CString GetApplicationContainer(const CString strPath) const; 

public:
	void Init(const CString& sServer, const CString& sInstallationName, const CString& strMasterSolution);
	void SetWebServiceInstallation(const CString& sServiceInstallation) { m_sWebServiceInstallation = sServiceInstallation; }
	void AttachDictionaryPathFinder(CDictionaryPathFinderObj *pDictionaryPathFinder);

	void ClearApplicationsModulesMap()			{ m_ApplicationsModulesMap.RemoveAll(); }

	void GetCandidateApplications(CStringArray* pAppsArray);
	void GetCandidateModulesOfApp(const CString& sAppName, CStringArray* pAppsArray);


	//void AttachMetadataManager(TBMetadataManagerObj* pMetadataMng) { m_pMetadataManager = pMetadataMng;	}
	//TBMetadataManagerObj* GetMetadataManager() const { return m_pMetadataManager; }
	//StorageType GetMetadataStorageType() const { return m_pMetadataManager->GetStorageType(); }

	//BOOL ExistMetadataDirectory(const CString& strPath) const;
	//void CreateMetadataDirectory(const CString& strPath) const;
	//
	//BOOL			ExistMetadataFile(const CString& strPathFileName) const;
	//TBFile* GetMetadataFile(const CString& strPathFileName) const;
	//void			RemoveMetadataFile(const CString& strPathFileName);
	//void			SaveMetadataTextFile(const CString& strPathFileName, const CString& fileTextContent);
	//
	//void GetMetadataNameInDirectory(const CString& strPathName, const CString& extensionsType, CStringArray* pFoundedFile) const;


	// utility
	BOOL IsStandAlone() const;
	const CString&	GetServerName() const;
	//data una path mi restituisce l'applicazione ed il modulo di appartenenza
	void					GetApplicationModuleNameFromPath(const CString& sObjectFullPath, CString& strApplication, CString& strModule);
	CTBNamespace			GetNamespaceFromPath(const CString& sObjectFullPath);
	CString					GetUserNameFromPath(const CString& sObjectFullPath) const;
	CPathFinder::PosType	GetPosTypeFromPath(const CString& sObjectFullPath) const;
	
	CString			GetFileNameFromNamespace(const CTBNamespace& aNamespace, const CString& sUser, const CString& sCulture = _T("")) const;

	// estensioni di files
	const CString	GetObjectDefaultExtension(const CTBNamespace& aNamespace) const;
	void			GetObjectSearchExtensions(const CTBNamespace& aNamespace, CStringArray* pExtensions, BOOL bOnlyExt = FALSE) const;

	// ritorna il tipo di namespace sulla base dell'estensione
	CTBNamespace::NSObjectType	GetNamespaceTypeFromExtension(const CString& sFilename);


	// nomi di files
	const CString GetModuleConfigName() const;
	const CString GetAppConfigName() const;
	const CString GetLocalAppConfigName() const;

	// path names
	TB_OLD_METHOD const CString GetExePath() const { return GetTBDllPath(); }
	const CString GetTBDllPath() const;
	const CString GetDllNameFromNamespace(const CTBNamespace& aNamespace) const;
	const CString GetInstallationName() const;
	const CString GetInstallationPath() const;

	//per compatibilita' pregressa
	const CString GetRunningPath() const { return GetInstallationPath(); }

	const CString GetStandardPath() const;
	const CString GetCustomPath(BOOL bCreateDir = FALSE) const;

	const BOOL	  IsStandardPath(CString sPath) const;
	const BOOL	  IsCustomPath(CString sPath) const;
	const BOOL	  IsAllUsersPath(CString sPath) const;

	const CString GetClientFileSystemCacheName() const;
	const CString GetServerFileSystemCacheName() const;

	const CString GetEBReferencedAssembliesPath	() const;
	const CString GetConfigurationPath			() const;
	const CString GetCompaniesPath				(BOOL bCreateDir = FALSE) const;
	const CString GetAllCompaniesPath			(BOOL bCreateDir = FALSE) const;
	const CString GetCompanyPath				(BOOL bCreateDir = FALSE) const;
	const CString GetTempPath					(BOOL bCreateDir = FALSE) const;
	const CString GetAppDataPath				(BOOL bCreateDir = FALSE) const;
	const CString GetWebProxyImagesPath			(BOOL bCreateDir = FALSE) const;
	const CString GetWebProxyFilesPath			(BOOL bCreateDir = FALSE) const;
	
	const CString GetApplicationPath(const CString& sAppName, PosType pos, BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetCustomApplicationsPath() const;
	const CString GetModulePath(const CString& sAppName, const CString& sModuleName, PosType pos, BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetModulePath(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;

	const CString GetSemaphoreFilePath() const;
	const CString GetDateRangesFilePath() const;

	const CString GetTaskBuilderXmlPath() const;
	const CString GetModuleObjectsPath(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetCustomAllCompaniesModuleObjectsPath(const CTBNamespace& ownerModule) const;
	const CString GetJsonFormsPath(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir = FALSE, Company aCompany = CURRENT, const CString& sUserRole = _T("")) const;
	const CString GetModuleSettingsPath(const CTBNamespace& aNamespace, PosType pos, const CString sUserRole = _T(""), BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetModuleDictionaryFilePath(const CTBNamespace& aNamespace, BOOL bFromStandard, const CString& strCulture) const;
	const CString GetModuleReportPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	const CString GetModuleFilesPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetModuleXmlPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	const CString GetModuleXmlPathCulture(const CTBNamespace& aNamespace, PosType pos, const CString& sCulture = _T(""), const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	const CString GetModuleHelpPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	const CString GetDocumentPath(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetDocumentRadarPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	const CString GetDocumentQueryPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	const CString GetDocumentDescriptionPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetDocumentSchemaPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	
	const CString GetDocumentReportsFile(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	const CString GetDocumentRadarsFile(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	const CString GetDocumentBarcodeFile(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	const CString GetDocumentDefaultsFile(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	
	const CString GetModuleDataFilePath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	const CString GetModuleReferenceObjectsPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetDocumentExportProfilesPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetExportProfilePath(const CTBNamespace& nsProfile, PosType pos, const CString& strUserRole = _T(""), BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetExportProfilePath(const CTBNamespace& nsDocument, const CString& strProfileName, PosType pos, const CString& strUserRole = _T(""), BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const; const CString GetPartialProfilePathForClientDoc(const CTBNamespace& aProfileNamespace, PosType pos, const CString& strUserRole = _T(""), BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	
	const CString GetDocumentFormNSChangesFile(const CTBNamespace& aNamespace) const;	
	const CString GetDocumentEventsFile(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;

	const CString GetAppDataIOPath(BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetAppXTechDataIOPath(BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;
	const CString GetLogDataIOPath(BOOL bCreateDir = FALSE, Company aCompany = CURRENT) const;

	const CString GetDataPath(PosType pos) const;
	const CString GetDataDefaultPath(PosType pos) const;

	const CString GetModuleWordDocPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;
	const CString GetModuleExcelDocPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;

	// path objects names
	void GetAllModulePath(CStringArray& aAllModulePath, CTBNamespace& aNamespace, const CStringArray& aUser, BOOL bStd = TRUE, BOOL bAllUsrs = TRUE, BOOL bSearchStd = TRUE, BOOL bIsFromApp = TRUE, const CString strModuleName = _T("")) const;	// restituisce tutte le path del modulo (custom indicato e std) di un dato tipo di namespace
	void GetAllObjInFolder(CStringArray& aAllObjInFolder, CTBNamespace& aNamespace, const CStringArray& aAllModulesPath, const CString strNameSearch = _T("*"), const CString strExtSearch = _T("")) const;					// restituisce path di tutti gli oggetti custom e std dei moduli passati

	// seach strings
	const CString GetModuleReferenceObjectsSearch() const;

	// file names
	const CString GetServerConnectionConfigFullName() const;

	//const CString GetFontsFullName(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir = FALSE) const;
	const CString GetFontsFullName(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir = FALSE) const;



	const CString GetFormatsFullName(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir = FALSE) const;
	const CString GetEnumsFullName(const CTBNamespace& aNamespace, PosType pos) const;
	const CString GetDocumentDbtsFullName(const CTBNamespace& aNamespace, PosType pos) const;
	const CString GetDocumentDbtsFullName(const CTBNamespace& aNamespace) const;
	const CString GetReportFullName(const CTBNamespace& aNamespace, const CString& sUser) const;
	const CString GetReportFullNameIn(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole = _T("")) const;
	const CString GetDatabaseObjectsFullName(const CTBNamespace& aNamespace, PosType pos) const;
	const CString GetAddOnDbObjectsFullName(const CTBNamespace& aNamespace, PosType pos) const;
	const CString GetDocumentObjectsFullName(const CTBNamespace& aNamespace, PosType pos) const;
	const CString GetBehaviourObjectsFullName(const CTBNamespace& aNamespace, PosType pos) const;
	const CString GetClientDocumentObjectsFullName(const CTBNamespace& aNamespace) const;
	const CString GetResourcesFullFileName(const CTBNamespace& aNamespace) const;
	const CString GetJsonFormsFullFileName(const CTBNamespace& aNamespace, CString sId, PosType pos, BOOL bCreateDir = FALSE, Company aCompany = CURRENT, const CString& sUserRole = _T("")) const;

	const CString GetApplicationConfigFullName(const CString& sAppName) const;
	
	const CString GetMasterApplicationPath() const;

	const CString GetThemeElementFullName(CString strThemeName) const;
	const CString GetThemeCssFullName(CString strThemeName) const;
	const CString GetThemeCssFullNameFromThemeName(CString strThemeName);
	const CString GetApplicationConfigFullNameFromPath(const CString& sAppPath) const;
	const CString GetWebMethodsFullName(const CTBNamespace& aNamespace) const;
	const CString GetEventHandlerObjectsFullName(const CTBNamespace& aNamespace) const;
	const CString GetOutDateObjectsFullName(const CTBNamespace& aNamespace) const;
	const CString GetEnvelopeObjectsFullName(const CTBNamespace& aNamespace) const;
	const CString GetItemSourceObjectsFullName(const CTBNamespace& aNamespace) const;
	const CString GetLocalizApplicationConfigFullName(const CString& sAppName, const CString& sModName) const;
	
	const CString GetStartupLogFullName(const CString& sUser, BOOL bCreateDir = FALSE) const;
	const CString GetExitLogFullName(const CString& sUser, BOOL bCreateDir = FALSE, BOOL bOnClient = FALSE) const;
	const CString GetUserLogPath(const CString& sUser, const CString& sCompany, BOOL bCreateDir = FALSE, BOOL bOnClient = FALSE) const;

	const void	GetDictionaryPathsFromString(LPCTSTR lpcszString, CStringArray &paths);
	const void GetDictionaryPathsFromID(UINT nIDD, LPCTSTR lpszType, CStringArray &paths);
	const CString GetDictionaryPathFromNamespace(const CTBNamespace& aNamespace, BOOL bStandard);
	const CString GetDictionaryPathFromTableName(const CString& strTableName);
	const CString GetDictionaryPathFromFileName(const CString& strFileName);
	
	void GetDictionaryPathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &paths);
	void GetJsonFormsPathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &paths);
	const CString GetJsonFormPath(const CTBNamespace& ns);

	const CString GetNumberToLiteralXmlFullName(const CTBNamespace& aNamespace, const CString& sCulture = _T("")) const;

	const CString GetActionSubscriptionsFolderPath(Company aCompany = CURRENT) const;

	// filesname per tecnologia Xtech
	const CString GetDocumentXSLTImportFullName(const CTBNamespace& aNamespace) const;
	const CString GetDocumentXSLTExportFullName(const CTBNamespace& aNamespace) const;
	const CString GetDocumentActionsFullName(const CTBNamespace& aNamespace) const;
	const CString GetDocumentActionsFullName(const CTBNamespace& aNamespace, PosType pos) const;
	const CString GetDocumentSelectionsFullName(const CTBNamespace& aNamespace, PosType pos) const;
	const CString GetDocumentDocumentFullName(const CTBNamespace& aNamespace, PosType pos) const;
	const CString GetDocumentDocumentFullName(const CTBNamespace& aNamespace) const;
	const CString GetDocumentXRefFullName(const CTBNamespace& aNamespace, PosType pos) const;
	const CString GetDocumentXRefFullName(const CTBNamespace& aNamespace) const;
	const CString GetDocumentCodingRulesFullName(const CTBNamespace& aNamespace, PosType pos) const;
	const CString GetModuleConfigFullName(const CString& sAppName, const CString& sModuleName) const;

	CString GetMenuThumbnailsFolderPath();

	const CString GetXRefFileFromThisPath(const CString&);

	//bAppend = FALSE se si ha l'esigenza di inserire i profili nell'array seguendo le regole della personalizzazione
	void GetProfilesFromPath(const CString& strProfilesPath, CStringArray& pProfilesList, BOOL bAppend = TRUE);
	void GetAvailableThemesFullNames(CStringArray& arThemes);
	void GetAvailableThemeFonts(CStringArray& fonts);


	const CString GetLoginServiceName() const;
	const CString GetTbServicesName() const;
	const CString GetLockServiceName() const;

	//DataSynchronizer
	const CString GetSynchroProvidersPath(const CTBNamespace& aNamespace) const;

	CString	ToPosDirectory(const CString& sPath, PosType pos, const CString& sName, BOOL bCreateDir = FALSE) const;
	CString	ToUserName(const CString& sUserDir) const;
	CString	ToUserDirectory(const CString& sUserName) const;


	// container and application type
	static ApplicationType	StringToApplicationType(const CString& sType);

	ApplicationType	GetContainerApplicationType(const CString& sContainerName) const;
	const CString	GetContainerPath(const ApplicationType aType) const;
	const CString	GetAppContainerName(const CString& sAppName) const;
	BOOL			IsASystemApplication(const CString& sAppName) const;

	CString			TransformInRemotePath(const CString& strPath) const;
	CString			GetCustomDebugSymbolsPath() const;
	CString			GetCustomUserApplicationDataPath(BOOL bCreateDir = TRUE) const;

	const CString FromNs2Path(const CString& sName, CTBNamespace::NSObjectType t1, CTBNamespace::NSObjectType t2);

	CString CPathFinder::GetXmlParametersDirectory();

private:
	int	 AddApplicationModules(const CString& sApplicationPath);

	const CString GetApplicationThemeFullName(CString strThemeName, BOOL bTB = FALSE) const;
	const CString GetApplicationThemeCssFullName(CString strThemeName, BOOL bTB = FALSE) const;

#ifdef _DEBUG
	void Dump(CDumpContext& dc) const;
	void AssertValid() const;
#endif //_DEBUG

};

// General Functions
//-----------------------------------------------------------------------------
TB_EXPORT CPathFinder*	AFXAPI AfxGetPathFinder();

//=============================================================================        

#include "endh.dex"
