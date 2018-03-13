#include "stdafx.h"
#include <atlenc.h>
#include "Chars.h"
#include "FileSystemFunctions.h"
#include "IFileSystemManager.h"
#include "PathFinder.h"
#include "LoginContext.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//------------------------------------------------------------------------------
// path di gestione 
static const TCHAR szCustom[] = _T("Custom");
static const TCHAR szConfiguration[] = _T("Configuration");
static const TCHAR szReferencedAssemblies[] = _T("ReferencedAssemblies");
static const TCHAR szSubscription[] = _T("Subscription");
static const TCHAR szCompanies[] = _T("Companies");
static const TCHAR szAllCompanies[] = _T("AllCompanies");
static const TCHAR szDictionary[] = _T("Dictionary");
static const TCHAR szDictionaryFile[] = _T("Dictionary.bin");
static const TCHAR szPreferences[] = _T("Preferences");
static const TCHAR szSettings[] = _T("Settings");
static const TCHAR szModuleObjects[] = _T("ModuleObjects");
static const TCHAR szJsonForms[] = _T("JsonForms");
static const TCHAR szData[] = _T("Data");
static const TCHAR szDataIO[] = _T("DataTransfer");
static const TCHAR szXTechDataIO[] = _T("XTech");
static const TCHAR szLogDataIO[] = _T("Log");
static const TCHAR szXml[] = _T("Xml");
static const TCHAR szDefault[] = _T("Default");
static const TCHAR szResourcesFolder[] = _T("Resources");
static const TCHAR szReport[] = _T("Report");
static const TCHAR szQuery[] = _T("Query");
static const TCHAR szRadar[] = _T("Radar");
static const TCHAR szFiles[] = _T("Files");
static const TCHAR szTexts[] = _T("Texts");
static const TCHAR szPdf[] = _T("Pdf");
static const TCHAR szRtf[] = _T("Rtf");
static const TCHAR szOdf[] = _T("Odf");
static const TCHAR szImages[] = _T("Images");
static const TCHAR szOthers[] = _T("Others");
static const TCHAR szHelp[] = _T("Help");
static const TCHAR szDescription[] = _T("Description");
static const TCHAR szUsers[] = _T("Users");
static const TCHAR szRoles[] = _T("Roles");
static const TCHAR szExportProfiles[] = _T("ExportProfiles");
static const TCHAR szAllUsers[] = _T("AllUsers");
static const TCHAR szSharedViews[] = _T("Shared");
static const TCHAR szSolutions[] = _T("Solutions");
static const TCHAR szThemes[] = _T("Themes");
static const TCHAR szFonts[] = _T("Fonts");
static const TCHAR szMainBrandFile[] = _T("Main.Brand.xml");
static const TCHAR szBrandExtension[] = _T(".Brand.xml");
static const TCHAR szThumbnails[] = _T("Thumbnails");
static const TCHAR szTempFolder[] = _T("TBTemp");

static const TCHAR szDataManager[] = _T("DataManager");
static const TCHAR szDataFile[] = _T("DataFile");
static const TCHAR szBin[] = _T("Apps");

static const TCHAR szEnumsViewer[] = _T("EnumsViewer");

static const TCHAR szWord[] = _T("Word");
static const TCHAR szExcel[] = _T("Excel");

// Files name
static const TCHAR szXSLTImport[] = _T("XSLImport.xsl");
static const TCHAR szXSLTExport[] = _T("XSLExport.xsl");
static const TCHAR szDBTS[] = _T("Dbts.xml");
static const TCHAR szActions[] = _T("Actions.xml");
static const TCHAR szDefaults[] = _T("Defaults.xml");
static const TCHAR szSelections[] = _T("Selections.xml");
static const TCHAR szDocument[] = _T("Document.xml");
static const TCHAR szExternalReference[] = _T("ExternalReferences.xml");
static const TCHAR szCodingRules[] = _T("CodingRules.xml");

static const TCHAR szDatabaseObjectsXml[] = _T("DatabaseObjects.xml");
static const TCHAR szDatabaseObjectsFolder[] = _T("DatabaseObjects");
static const TCHAR szAddOnDbObjects[] = _T("AddOnDatabaseObjects.xml");
static const TCHAR szDocumentObjects[] = _T("DocumentObjects.xml");
static const TCHAR szWebMethods[] = _T("WebMethods.xml");
static const TCHAR szResources[] = _T("Resources.xml");
static const TCHAR szReferenceObjects[] = _T("ReferenceObjects");
static const TCHAR szOutDateObjects[] = _T("OutDateObjects.xml");
static const TCHAR szEnvelopeObjects[] = _T("EnvelopeObjects.xml");
static const TCHAR szBehaviourObjects[] = _T("BehaviourObjects.xml");

static const TCHAR szItemSourcesObjects[] = _T("ItemSources.xml");
static const TCHAR szEventHandlersObjects[] = _T("EventHandlerObjects.xml");
static const TCHAR szClientDocumentObjects[] = _T("ClientDocumentObjects.xml");
static const TCHAR szNumberToLiteral[] = _T("NumberToLiteral.xml");
static const TCHAR szFileSystemCache[] = _T("FileSystemCache.xml");
static const TCHAR szDebugSymbols[] = _T("DebugSymbols");

static const TCHAR szActionSubscriptions[] = _T("ActionSubscriptions");

static const TCHAR szModuleConfigName[] = _T("Module.config");
static const TCHAR szAppConfigName[] = _T("Application.config");
static const TCHAR szLocalAppConfigName[] = _T("LocalizableApplication.config");

static const TCHAR szEnumsFileName[] = _T("Enums.xml");
static const TCHAR szFontsFileName[] = _T("Fonts.ini");
static const TCHAR szFormatsFileName[] = _T("Formats.ini");
static const TCHAR szReportsXML[] = _T("Reports.xml");
static const TCHAR szRadarsXML[] = _T("Radars.xml");
static const TCHAR szBarcodeXML[] = _T("Barcode.xml");
static const TCHAR szEventsXML[] = _T("Events.config");
static const TCHAR szDateRangesXML[] = _T("DateRanges.xml");

static const TCHAR szFormNsChangesXML[] = _T("FormNsChanges.xml");

static const TCHAR szStartupLog[] = _T("StartupLog-%d-%d-%d.xml");
static const TCHAR szExitLog[] = _T("ExitLog-%d-%d-%d.xml");

// extensions
static	const TCHAR szExe[]		= _T(".exe");
const TCHAR szXmlExt[]			= _T(".xml");
const TCHAR szThemeExt[]		= _T(".theme");
const TCHAR szCssExt[]			= _T(".css");
const TCHAR szCssOverrideExt[]	= _T(".override.css");
const TCHAR szFontExt[]			= _T(".ttf");

// lettura del clientconnection
static const TCHAR szServerConnectionConfig[] = _T("ServerConnection.config");
static const TCHAR szClientConnectionConfig[] = _T("ClientConnection.config");
static const TCHAR szTaskBuilderNet[] = _T("TaskBuilder.Net");

static const TCHAR szAll[] = _T("\\*.*");

static const TCHAR szBackSlashSubstitute = _T('.');
static const TCHAR szFileInvalidChars[] = _T("/?*\\<>:!|\"");

// application type strings
static const TCHAR szAppTypeStandardization[] = _T("EasyBuilderApplication");
static const TCHAR szAppTypeApplications[] = _T("TbApplication");
static const TCHAR szCustomization[] = _T("Customization");
static const TCHAR szAppTypeTb[] = _T("Tb");

// containers
static const TCHAR szContainerTaskBuilder[] = _T("TaskBuilder");
static const TCHAR szContainerApplications[] = _T("Applications");

// C++ Task Builder Platform
const TCHAR szTBLoader[] = _T("TBLoader");
const TCHAR szTaskBuilderApp[] = _T("Framework");
const TCHAR szExtensionsApp[] = _T("Extensions");

// nomi di directory
const TCHAR szAllUserDirName[] = _T("AllUsers");
const TCHAR szStandard[] = _T("Standard");

const TCHAR szSettingsExt[] = _T(".config");

const TCHAR szPredefined[] = _PREDEFINED("Default");
const TCHAR szDbInfo[] = _T("DbInfo");
const TCHAR szAppData[] = _T("App_Data");
const TCHAR szSettingsConfigFile[] = _T("Settings.config");
const TCHAR szCandidateModulesSep[] = _T(";");

static const TCHAR szEasyStudioHome[] = _T("ESHome");
//DataSynchronizer
static const TCHAR szSynchroProviders[] = _T("SynchroProviders");
enum EncodingType { ANSI, UTF8, UTF16_BE, UTF16_LE };	//UTF16_BE: Big Endian (swap sui byte); UTF16_LE: Little Endian 

static const TCHAR szTemplate[] = _T("Template");

//============================================================================
//	Static Objects & general functions
//============================================================================

//-------------------------------------------------------------
EncodingType GetEncodingType(BYTE* pBinaryContent, long nSize)
{
	EncodingType encodingType = ANSI;
	if (!pBinaryContent || nSize < 3)
		return encodingType;

	if (pBinaryContent[0] == 0xFF && pBinaryContent[1] == 0xFE)		//UTF-16 (LE)  little endian -- Unicode
		encodingType = UTF16_LE;
	else
	{
		if (pBinaryContent[0] == 0xFF && pBinaryContent[1] == 0xFF)	//UTF- 16 (BE) big endian
			encodingType = UTF16_BE;
		else
			if (pBinaryContent[0] == 0xEF && pBinaryContent[1] == 0xBB && pBinaryContent[2] == 0xBF)	//UTF8
				encodingType = UTF8;
	}

	return encodingType;

}
///////////////////////////////////////////////////////////////////////////////
//								TBFile
///////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
//----------------------------------------------------------------------------
TBFile::TBFile(const CString& strCompleteFileName)
	:
	m_strCompleteFileName(strCompleteFileName),
	m_bIsCustomPath(FALSE),
	m_pFileContent(NULL),
	m_FileSize(0)
{
	m_strName = ::GetName(strCompleteFileName);
	m_strPathName = ::GetPath(strCompleteFileName);
	m_strFileType = ::GetExtension(m_strName);
}
//----------------------------------------------------------------------------
TBFile::TBFile(const CString& strName, const CString& strPathName)
	:
	m_strName(strName),
	m_strPathName(strPathName),
	m_bIsCustomPath(FALSE),
	m_pFileContent(NULL),
	m_FileSize(0)
{
	m_strCompleteFileName = strPathName + SLASH_CHAR + strName;
	m_strFileType = ::GetExtension(m_strName);
}

//----------------------------------------------------------------------------
TBFile::~TBFile()
{
	if (m_pFileContent)
		delete m_pFileContent;
}

//----------------------------------------------------------------------------
CString TBFile::GetContentAsString()
{
	if (!m_strFileContent.IsEmpty())
		return m_strFileContent;

	if (!m_pFileContent || m_FileSize == 0)
		return _T("");

	//AfxGetPathFinder()->GetMetadataManager()->StartTimeOperation(CONVERT_METADATA);
	CString strContent;
	//devo convertire il binario nella giusta stringa a seconda del suo tipo
	EncodingType encodingType = GetEncodingType(m_pFileContent, m_FileSize);
	switch (encodingType)
	{
		case ANSI:
		{
			//devo mettere il carattere terminatore
			char* pANSIBuff = new char[m_FileSize+1];
			memcpy(pANSIBuff, m_pFileContent, m_FileSize);
			pANSIBuff[m_FileSize] = _T('\0');
			CString str = CString(pANSIBuff);
			delete[] pANSIBuff;
			return str;
		}

		case UTF16_BE:
		case UTF16_LE:
		{
			ASSERT(FALSE);
			break;
		}

		case UTF8:
		{
			DWORD dwLenght = m_FileSize - 3;
			wchar_t * pUnicodeBuff = new wchar_t[dwLenght + 1];
			dwLenght = MultiByteToWideChar(CP_UTF8, 0, (char*)(m_pFileContent + 3), dwLenght, pUnicodeBuff, dwLenght);
			pUnicodeBuff[dwLenght] = _T('\0');
			CString str = CString(pUnicodeBuff);
			delete[] pUnicodeBuff;
			return str;
		}
	}
	//AfxGetPathFinder()->GetMetadataManager()->StopTimeOperation(CONVERT_METADATA);

	return _T("");
}

//----------------------------------------------------------------------------
BYTE* TBFile::GetContentAsBinary()
{
	if (m_pFileContent)
		return m_pFileContent;

	if (m_strFileContent.IsEmpty())
		return NULL;
	
	int nBytesLength = AtlUnicodeToUTF8(m_strFileContent, m_strFileContent.GetLength(), NULL, 0); //calcolo la dimensione richiesta per il buffer
	BYTE* pResponseBytes = new BYTE[nBytesLength];
	AtlUnicodeToUTF8(m_strFileContent, m_strFileContent.GetLength(), (LPSTR)pResponseBytes, nBytesLength);

	return pResponseBytes;
}

//-----------------------------------------------------------------------------
TB_EXPORT CPathFinder* AFXAPI AfxGetPathFinder()
{
	return AfxGetApplicationContext()->GetPathFinder();
}

///////////////////////////////////////////////////////////////////////////////
//				class CPathFinder implementation
///////////////////////////////////////////////////////////////////////////////
//
extern "C" int APIENTRY DllMain(HINSTANCE, DWORD, LPVOID);

#define	GET_DLL_HINSTANCE(hInstance) \
	{\
		MEMORY_BASIC_INFORMATION mbi; \
		VirtualQuery(DllMain, &mbi, sizeof(mbi)); \
		hInstance = (HINSTANCE)mbi.AllocationBase; \
	}

//-----------------------------------------------------------------------------
CPathFinder::CPathFinder()
	:
	m_bIsStandAlone(FALSE),
	m_pDictionaryPathFinder(NULL),
	m_bIsRunningInsideInstallation(FALSE),
	m_eESAppPosType(PosType::CUSTOM)
{
	HINSTANCE hInstance;
	GET_DLL_HINSTANCE(hInstance);
	TCHAR fname[_MAX_FNAME + 1];
	::GetModuleFileName(hInstance, fname, _MAX_FNAME + 1);

	m_sTbDllPath = GetPath(fname);
	m_sESHome = szEasyStudioHome;
}

//-----------------------------------------------------------------------------
CPathFinder::~CPathFinder()
{
	delete m_pDictionaryPathFinder;
}

//-----------------------------------------------------------------------------
void CPathFinder::Init(const CString& sServer, const CString& sInstallationName, const CString& sMasterSolution)
{
	m_sServerName = sServer;
	m_sMasterSolution = sMasterSolution;
	m_sInstallation = sInstallationName;

	ASSERT(!m_sInstallation.IsEmpty());
	m_bIsStandAlone = _tcsicmp(sServer, GetComputerName(FALSE)) == 0;	
	//mi � stato detto (mediante il file FileSystemManager.config o il ClickOnce) di andare a pescare i dati da un'altra parte	
	if (!m_bIsStandAlone)
	{
		m_sStandardPath = CString(SLASH_CHAR) + SLASH_CHAR + sServer + SLASH_CHAR + sInstallationName + _T("_") + szStandard;
		m_sCustomPath = CString(SLASH_CHAR) + SLASH_CHAR + sServer + SLASH_CHAR + sInstallationName + _T("_") + szCustom;
	}
	else
	{

		CString sPath = GetTBDllPath();
		TCHAR szFolder[MAX_PATH];
		SHGetSpecialFolderPath(NULL, szFolder, CSIDL_PROFILE, FALSE);
		BOOL isClickOnce = sPath.Find(szFolder) != -1;
		int appsIdx = isClickOnce ? -1 : sPath.Find(_T("\\Apps\\"));
		//se nel path dove sto girando e` presente Apps, allora sto girando nell'installazione e non ho bisogno dello share
		if (appsIdx > -1)
		{
			CString sInstallationPath = sPath.Mid(0, appsIdx);
			m_sStandardPath = sInstallationPath + SLASH_CHAR + szStandard;
			m_sCustomPath = sInstallationPath + SLASH_CHAR + szCustom;
			m_bIsRunningInsideInstallation = TRUE;
		}
	}
}

//-----------------------------------------------------------------------------
void CPathFinder::AttachDictionaryPathFinder(CDictionaryPathFinderObj *pDictionaryPathFinder)
{
	ASSERT(!m_pDictionaryPathFinder);
	m_pDictionaryPathFinder = pDictionaryPathFinder;
	m_pDictionaryPathFinder->m_pPathFinder = this;
}



//-----------------------------------------------------------------------------
const CString& CPathFinder::GetServerName() const
{
	return m_sServerName;
}

//-----------------------------------------------------------------------------
CString CPathFinder::GetCompanyName() const
{
	// verifico che non siano presenti caratteri 
	// non consentiti nel nome di directory

	const CLoginInfos* pInfos = AfxGetLoginInfos();
	CString sRet = pInfos ? pInfos->m_strCompanyName : _T("");
	sRet.Replace(szFileInvalidChars, _T(" "));
	sRet.Trim();
	return sRet;
}

//-----------------------------------------------------------------------------
CString CPathFinder::GetUserName() const
{
	// verifico che non siano presenti caratteri 
	// non consentiti nel nome di directory

	const CLoginInfos* pInfos = AfxGetLoginInfos();
	CString sRet = pInfos ? pInfos->m_strUserName : _T("");
	sRet.Replace(szFileInvalidChars, _T(" "));
	sRet.Trim();
	return sRet;
}
//-----------------------------------------------------------------------------
BOOL CPathFinder::IsStandAlone() const
{
	return m_bIsStandAlone;
}

//-----------------------------------------------------------------------------
CString CPathFinder::TransformInRemotePath(const CString& strPath) const
{
	if (!m_bIsStandAlone)
		return strPath;

	CString strRemotePath;
	int nPos = strPath.Find(m_sInstallation);
	if (nPos >= 0)
	{
		int nNextSlash = strPath.Find(SLASH_CHAR, nPos);
		strRemotePath = strPath.Right(strPath.GetLength() - (nNextSlash + 1));
		strRemotePath = CString(SLASH_CHAR) + SLASH_CHAR + GetComputerName(FALSE) + SLASH_CHAR + m_sInstallation + _T("_") + strRemotePath;
	}

	return (strRemotePath.IsEmpty()) ? strPath : strRemotePath;
}


//-----------------------------------------------------------------------------
CString CPathFinder::GetCustomDebugSymbolsPath() const
{
	return GetCustomPath() + SLASH_CHAR + szDebugSymbols;
}

//---------------------------------------------------------------------------------
CString CPathFinder::GetCustomUserApplicationDataPath(BOOL bCreateDir /*= TRUE*/) const
{
	CString sPath = GetCompanyPath(bCreateDir) +
		SLASH_CHAR +
		szAppData;
	if (bCreateDir && !ExistPath(sPath))
		CreateDirectory(sPath);

	sPath = sPath +
		SLASH_CHAR +
		GetUserName();
	if (bCreateDir && !ExistPath(sPath))
		CreateDirectory(sPath);
	return sPath;
}

//-------------------------------------------------------------------------------------
CString CPathFinder::GetApplicationContainer(const CString strPath) const
{
	CString strPathLower = strPath;
	
	strPathLower.MakeLower();
	CString strAppContainerPath = GetContainerPath(CPathFinder::TB);
	strAppContainerPath.MakeLower();
	if (strPathLower.Find(strAppContainerPath) >= 0)
		return GetName(strAppContainerPath);

	strAppContainerPath = GetContainerPath(CPathFinder::TB_APPLICATION);
	strAppContainerPath.MakeLower();
	if (strPathLower.Find(strAppContainerPath) >= 0)
		return GetName(strAppContainerPath);

	strAppContainerPath = GetEasyStudioCustomizationsPath();
	strAppContainerPath.MakeLower();
	if (strPathLower.Find(strAppContainerPath) >= 0)
		return GetName(strAppContainerPath);

	ASSERT(FALSE);
	return _T("");
}

//-------------------------------------------------------------------------------------
void CPathFinder::GetCandidateApplications(CStringArray* pAppsArray)
	{
	if (!pAppsArray)
		return;
	
	CString strApplicationPath;
	CString strApplicationName;
	CStringArray applicationPathArray ;
	AfxGetFileSystemManager()->GetAllApplicationInfo(&applicationPathArray);
	if (m_ApplicationContainerMap.IsEmpty())
	{
		for (int i = 0; i < applicationPathArray.GetSize(); i++)
		{
			strApplicationPath = applicationPathArray.GetAt(i);
			strApplicationName = GetName(strApplicationPath);
			strApplicationName.MakeLower();
			m_ApplicationContainerMap[strApplicationName] = GetApplicationContainer(strApplicationPath);
			pAppsArray->Add(strApplicationName);
		}
	}
}

//-------------------------------------------------------------------------------------
void CPathFinder::GetCandidateModulesOfApp(const CString& sAppName, CStringArray* pModsArray)
{
	if (!pModsArray)
		return;

	pModsArray->RemoveAll();

	CString strApplicationName = sAppName;
	CString strModuleName;
	CString strValue;
	strApplicationName.MakeLower();
	CStringArray modulePathArray;

	AfxGetFileSystemManager()->GetAllModuleInfo(sAppName, &modulePathArray);
	int nModules = 0;

	for (int i = 0; i < modulePathArray.GetSize(); i++)
	{
		strModuleName = GetName(modulePathArray.GetAt(i));
		if (!strModuleName.IsEmpty())
		{
			strValue += strModuleName + szCandidateModulesSep;
			nModules++;
		}
		pModsArray->Add(strModuleName);
		
		if (nModules > 0)
			m_ApplicationsModulesMap[strApplicationName] = strValue;
	}
}


//-------------------------------------------------------------------------------------
const BOOL CPathFinder::IsStandardPath(CString sPath) const
{
	CString aStd = GetStandardPath();

	aStd.MakeUpper();
	sPath.MakeUpper();

	return sPath.Find(aStd) >= 0;
}

//-------------------------------------------------------------------------------------
const BOOL CPathFinder::IsAllUsersPath(CString sPath) const
{
	CString a = (CString(szAllUserDirName)).MakeUpper();
	sPath.MakeUpper();

	return sPath.Find(a) >= 0;
}

//-------------------------------------------------------------------------------------
const BOOL CPathFinder::IsCustomPath(CString sPath) const
{
	CString aCst = GetCustomPath();

	aCst.MakeUpper();
	sPath.MakeUpper();

	return sPath.Find(aCst) >= 0;
}

//-------------------------------------------------------------------------------------
const CString CPathFinder::GetContainerPath(const ApplicationType aType) const
{
	return m_sStandardPath + SLASH_CHAR +
		(aType == CPathFinder::TB_APPLICATION ? szContainerApplications : szContainerTaskBuilder);
}

// si occupa di definire la locazione della specifica AddOnApplication sapendo che in
// fase di caricamento vengono eliminati i duplicati di nome
//-------------------------------------------------------------------------------------
const CString CPathFinder::GetAppContainerName(const CString& sAppName) const
{
	CString strValue, strKey;
	strKey = sAppName;
	strKey.MakeLower();
	if (m_ApplicationContainerMap.Lookup(strKey, strValue))
		return strValue;

	return szContainerApplications;
}

// ritorna il tipo di namespace sulla base dell'estensione
//-------------------------------------------------------------------------------------
CTBNamespace::NSObjectType CPathFinder::GetNamespaceTypeFromExtension(const CString& sFilename)
{
	CTBNamespace::NSObjectType aType = CTBNamespace::FILE;

	CString sExtension = GetExtension(sFilename); sExtension.MakeLower();
	CString	sFile = szFiles;

	CString sLower = sFilename; sLower.MakeLower();

	CString s = sFile + SLASH_CHAR + szImages + SLASH_CHAR;;
	s.MakeLower();
	int idx = sLower.Find(s);
	if (idx > 0)
	{
		aType = CTBNamespace::IMAGE;
		goto l_step2;
	}

	s = sFile + SLASH_CHAR + szTexts + SLASH_CHAR;
	s.MakeLower();
	idx = sLower.Find(s);
	if (idx > 0)
	{
		aType = CTBNamespace::TEXT;
		goto l_step2;
	}

	s = sFile + SLASH_CHAR + szPdf + SLASH_CHAR;
	s.MakeLower();
	idx = sLower.Find(s);
	if (idx > 0)
	{
		aType = CTBNamespace::PDF;
		goto l_step2;
	}

	s = sFile + SLASH_CHAR + szRtf + SLASH_CHAR;
	s.MakeLower();
	idx = sLower.Find(s);
	if (idx > 0)
	{
		aType = CTBNamespace::RTF;
		goto l_step2;
	}

	//s = sFile + SLASH_CHAR + szOdf + SLASH_CHAR;
	//s.MakeLower();
	//idx = sLower.Find(s);
	//if (idx > 0)
	//{
	//	aType = CTBNamespace::ODF;
	//	goto l_step2;
	//}

	//--------
l_step2:
	if (aType == CTBNamespace::TEXT)
	{
		CTBNamespace aNs(CTBNamespace::TEXT);
		CStringArray arExts;
		GetObjectSearchExtensions(aNs, &arExts, TRUE);
		for (int i = 0; i <= arExts.GetUpperBound(); i++)
			if (_tcsicmp(arExts.GetAt(i), sExtension) == 0)
				return CTBNamespace::TEXT;
	}
	else if (aType == CTBNamespace::IMAGE)
	{
		CTBNamespace aNs(CTBNamespace::IMAGE);
		CStringArray arExts;
		GetObjectSearchExtensions(aNs, &arExts, TRUE);
		for (int i = 0; i <= arExts.GetUpperBound(); i++)
			if (_tcsicmp(arExts.GetAt(i), sExtension) == 0)
				return CTBNamespace::IMAGE;
	}
	else if (aType == CTBNamespace::PDF)
	{
		CTBNamespace aNs(CTBNamespace::PDF);
		CStringArray arExts;
		GetObjectSearchExtensions(aNs, &arExts, TRUE);
		for (int i = 0; i <= arExts.GetUpperBound(); i++)
			if (_tcsicmp(arExts.GetAt(i), sExtension) == 0)
				return CTBNamespace::PDF;
	}
	else if (aType == CTBNamespace::RTF)
	{
		CTBNamespace aNs(CTBNamespace::RTF);
		CStringArray arExts;
		GetObjectSearchExtensions(aNs, &arExts, TRUE);
		for (int i = 0; i <= arExts.GetUpperBound(); i++)
			if (_tcsicmp(arExts.GetAt(i), sExtension) == 0)
				return CTBNamespace::RTF;
	}
	//else if (aType == CTBNamespace::ODF)
	//{
	//	CTBNamespace aNs(CTBNamespace::ODF);
	//	CStringArray arExts;
	//	GetObjectSearchExtensions(aNs, &arExts, TRUE);
	//	for (int i=0; i <= arExts.GetUpperBound(); i++)
	//		if (_tcsicmp(arExts.GetAt(i), sExtension) == 0)
	//			return CTBNamespace::ODF;
	//}

	return CTBNamespace::FILE;
}
//-------------------------------------------------------------------------------------
void CPathFinder::GetApplicationModuleNameFromPath(const CString& sObjectFullPath, CString& strApplication, CString& strModule)
{	
	int nPathToken = 0;
	int nPos = 0;
	strApplication.Empty();
	strApplication.Empty();

	while (nPos >= 0)
	{
		nPos = sObjectFullPath.Find(SLASH_CHAR, nPos + 1);
		if (nPos >= 0)
			nPathToken++;
	}

	// il minimo che posso rappresentare � applicazione e modulo, quindi
	// con il tipo devo avere almeno tre segmenti di path per poterlo fare 
	if (nPathToken <= 3)
		return;

	BOOL bStandard = IsStandardPath(sObjectFullPath);

	CString sStdCstPath;
	CString sTemp = sObjectFullPath;
	sTemp.Replace(SLASH_CHAR, URL_SLASH_CHAR);
	sTemp.MakeLower();

	// le replace sono Case Sensitive!
	if (bStandard)
		sStdCstPath = GetStandardPath();
	else
	{
		sStdCstPath = GetCompanyPath();
		// bug 13.914 point at the end of company name is not allowed
		if (sStdCstPath.Right(1).Compare(_T(".")) == 0)
			sStdCstPath = sStdCstPath.Left(sStdCstPath.GetLength() - 1);
	}

	sStdCstPath.Replace(SLASH_CHAR, URL_SLASH_CHAR);
	sStdCstPath.MakeLower();

	if (sTemp.Find(sStdCstPath) != 0)
	{
		TRACE2("GetApplicationModuleNameFromPath: Invalid application path: %s\n%s\n", (LPCTSTR)sStdCstPath, (LPCTSTR)sObjectFullPath);
		return;
	}
	sTemp = sTemp.Mid(sStdCstPath.GetLength() + 1);
	//sTemp.Replace(sStdCstPath + URL_SLASH_CHAR, _T(""));

	// la container la salto
	int nPosDir = sTemp.Find(URL_SLASH_CHAR);
	if (nPosDir > 0)
		sTemp = sTemp.Mid(nPosDir + 1);

	// l' application e module fanno parte del namespace
	nPosDir = sTemp.Find(URL_SLASH_CHAR);
	if (nPosDir > 0)
	{
		strApplication = sTemp.Left(nPosDir);
		sTemp = sTemp.Mid(nPosDir + 1);
	}

	nPosDir = sTemp.Find(URL_SLASH_CHAR);
	if (nPosDir > 0)
	{
		strModule = sTemp.Left(nPosDir);
		sTemp = sTemp.Mid(nPosDir + 1);
	}	
}

// Questo metodo ricostruisce il namespace dell'oggetto a partire dal Path. Di default
// ricostruisce quello di modulo e SOLO per gli oggetti che hanno una sottodirectory
// dedicata, riesce a ricostruire il namespace di tipo completo.
//-------------------------------------------------------------------------------------
CTBNamespace CPathFinder::GetNamespaceFromPath(const CString& sObjectFullPath)
{
	// prima controllo le cose di base
	if (sObjectFullPath.IsEmpty() || GetName(sObjectFullPath).IsEmpty() || GetPath(sObjectFullPath).IsEmpty())
		return CTBNamespace();
	CString sModule, sApplication, sType;
	// poi la consistenza dei token di path
	int nPathToken = 0;
	int nPos = 0;
	while (nPos >= 0)
	{
		nPos = sObjectFullPath.Find(SLASH_CHAR, nPos + 1);
		if (nPos >= 0)
			nPathToken++;
	}

	// il minimo che posso rappresentare � applicazione e modulo, quindi
	// con il tipo devo avere almeno tre segmenti di path per poterlo fare 
	if (nPathToken <= 3)
		return CTBNamespace();

	BOOL bStandard = IsStandardPath(sObjectFullPath);


	CString sStdCstPath;
	CString sTemp = sObjectFullPath;
	sTemp.Replace(SLASH_CHAR, URL_SLASH_CHAR);
	sTemp.MakeLower();

	// le replace sono Case Sensitive!
	if (bStandard)
		sStdCstPath = GetStandardPath();
	else
	{
		sStdCstPath = GetCompanyPath();
		// bug 13.914 point at the end of company name is not allowed
		if (sStdCstPath.Right(1).Compare(_T(".")) == 0)
			sStdCstPath = sStdCstPath.Left(sStdCstPath.GetLength() - 1);
	}

	sStdCstPath.Replace(SLASH_CHAR, URL_SLASH_CHAR);
	sStdCstPath.MakeLower();

	if (sTemp.Find(sStdCstPath) != 0)
	{
		TRACE2("GetNamespaceFromPath: Invalid application path: %s\n%s\n", (LPCTSTR)sStdCstPath, (LPCTSTR)sObjectFullPath);
		return CTBNamespace();
	}
	sTemp = sTemp.Mid(sStdCstPath.GetLength() + 1);
	//sTemp.Replace(sStdCstPath + URL_SLASH_CHAR, _T(""));

	// la container la salto
	int nPosDir = sTemp.Find(URL_SLASH_CHAR);
	if (nPosDir > 0)
		sTemp = sTemp.Mid(nPosDir + 1);

	// l' application e module fanno parte del namespace
	nPosDir = sTemp.Find(URL_SLASH_CHAR);
	if (nPosDir > 0)
	{
		sApplication = sTemp.Left(nPosDir);
		sTemp = sTemp.Mid(nPosDir + 1);
	}

	nPosDir = sTemp.Find(URL_SLASH_CHAR);
	if (nPosDir > 0)
	{
		sModule = sTemp.Left(nPosDir);
		sTemp = sTemp.Mid(nPosDir + 1);
	}

	CTBNamespace aNamespace(CTBNamespace::MODULE, sApplication + CTBNamespace::GetSeparator() + sModule);

	// il passo successivo � la sottidirectory di tipo
	nPosDir = sTemp.Find(URL_SLASH_CHAR);
	if (nPosDir > 0)
	{
		sType = sTemp.Left(nPosDir);
		sTemp = sTemp.Mid(nPosDir + 1);
	}

	// se ho un tipo coerente allora rappresento l'oggetto completo
	CTBNamespace::NSObjectType aType;
	if (_tcsicmp(sType, szJsonForms) == 0)
		aType = CTBNamespace::JSON;
	else if (_tcsicmp(sType, szReport) == 0)
		aType = CTBNamespace::REPORT;
	else if (_tcsicmp(sType, szFiles) == 0)
		aType = CTBNamespace::FILE;
	//Bugfix 23472
	else if (_tcsicmp(sType, szDataManager) == 0 ||_tcsicmp(sType, szDataFile) == 0)
		aType = CTBNamespace::DATAFILE;
	else
	{
		TRACE2("GetNamespaceFromPath: Invalid/unmanaged file type: %s\n%s\n", (LPCTSTR)sType, (LPCTSTR)sObjectFullPath);
		return CTBNamespace();
	}

	// l'oggetto file rappresenta anche il ramo dei suoi subfolders
	if (aType == CTBNamespace::FILE)
	{
		aType = GetNamespaceTypeFromExtension(sObjectFullPath);		//non serve molto....
		aNamespace.SetType(aType);

		// elimino il nome con l'estensione
		CString sName = GetNameWithExtension(sObjectFullPath);
		sName.MakeLower();
		sTemp.Replace(sName, _T(""));

		// dalla struttura custom devo togliere anche la directory utente
		if (!bStandard)
		{
			// tolgo AllUsers oppure Users/user-name
			CString s = CString(URL_SLASH_CHAR) + szAllUsers;
			s.MakeLower();
			if (sTemp.Replace((LPCTSTR)s, _T("")) == 0)
			{
				s = CString(URL_SLASH_CHAR) + szUsers;
				s.MakeLower();
				if (sTemp.Replace((LPCTSTR)s, _T("")))
				{
					// tolgo directory dell'utente....
					nPosDir = sTemp.Find(URL_SLASH_CHAR);
					if (nPosDir > 0)
						sTemp = sTemp.Mid(0, nPosDir + 1);
				}
				else
				{
					ASSERT(FALSE);
				}
			}
		}

		if (sTemp.Right(1) == URL_SLASH_CHAR)
			sTemp = sTemp.Left(sTemp.GetLength() - 1);

		if (sTemp.Left(1) == URL_SLASH_CHAR)
			sTemp = sTemp.Mid(1);

		sTemp.Replace(CString(URL_SLASH_CHAR), CTBNamespace::GetSeparator());
		aNamespace.SetPathInside(sTemp);
	}

	aNamespace.SetObjectName(aType, GetNameWithExtension(sObjectFullPath), TRUE);

	if (!aNamespace.IsValid())
		return CTBNamespace();

	return aNamespace;
}

//-----------------------------------------------------------------------------
CString	CPathFinder::GetUserNameFromPath(const CString& sObjectFullPath) const
{
	if (IsStandardPath(sObjectFullPath))
		return _T("");

	CString sTemp = GetPath(sObjectFullPath);
	int nPos = sTemp.ReverseFind(SLASH_CHAR);
	CString strUser = sTemp.Right(sTemp.GetLength() - (nPos + 1));

	if (strUser.IsEmpty())
		return _T("");

	// se non ha di estensione la GetNameWithExtension mi ritorna il . in fondo
	if (_tcsicmp(strUser.Right(1), _T(".")) == 0)
		strUser = strUser.Left(strUser.GetLength() - 1);

	// se sono in AllUsers ritorno vuoto, cio� non utente
	if (_tcsicmp(strUser, szAllUserDirName) == 0)
		return _T("");

	return ToUserName(strUser);
}

//-----------------------------------------------------------------------------
CPathFinder::PosType CPathFinder::GetPosTypeFromPath(const CString& sObjectFullPath) const
{
	CString sTemp = GetUserNameFromPath(sObjectFullPath);

	if (!sTemp.IsEmpty())
		return CPathFinder::USERS;

	if (sTemp.IsEmpty() && IsStandardPath(sObjectFullPath))
		return  CPathFinder::STANDARD;

	CString sPath = GetName(GetPath(sObjectFullPath));
	if (
		sTemp.IsEmpty() && IsCustomPath(sObjectFullPath) &&
		_tcsicmp(sPath, szAllUserDirName) == 0
		)
		return CPathFinder::ALL_USERS;


	return  CPathFinder::CUSTOM;
}

//-----------------------------------------------------------------------------
CPathFinder::PosType CPathFinder::GetDefaultPosTypeFor(BOOL bCustomization) const
{
	return bCustomization ? m_eESAppPosType : PosType::STANDARD;
}

// ritorna il nome del file da aprire secondo le politiche di custom 
//-----------------------------------------------------------------------------
CString	CPathFinder::GetFileNameFromNamespace(const CTBNamespace& aNamespace, const CString& sUser, const CString& sCulture/* = _T("")*/) const
{
	if (!aNamespace.IsValid())
		return _T("");

	CString sExtension = GetObjectDefaultExtension(aNamespace);
	CString sFileName = SLASH_CHAR + aNamespace.GetObjectName();
	if (!sExtension.IsEmpty() && ::GetExtension(sFileName).IsEmpty())
		sFileName += sExtension;

	CString sFullFileName;
	switch (aNamespace.GetType())
	{
		case (CTBNamespace::REPORT):
			sFullFileName = GetModuleReportPath(aNamespace, CPathFinder::USERS, sUser) + sFileName;
			if (ExistFile(sFullFileName))
				return sFullFileName;

			sFullFileName = GetModuleReportPath(aNamespace, CPathFinder::ALL_USERS) + sFileName;
			if (ExistFile(sFullFileName))
				return sFullFileName;

			return GetModuleReportPath(aNamespace, CPathFinder::STANDARD) + sFileName;

		case (CTBNamespace::IMAGE):
		case (CTBNamespace::TEXT):
		case (CTBNamespace::PDF):
		case (CTBNamespace::RTF):
			//case (CTBNamespace::ODF) :
		case (CTBNamespace::FILE):
		{
			sFullFileName = GetModuleFilesPath(aNamespace, CPathFinder::USERS, sUser) + sFileName;
			if (ExistFile(sFullFileName))
				return sFullFileName;

			sFullFileName = GetModuleFilesPath(aNamespace, CPathFinder::ALL_USERS) + sFileName;
			if (ExistFile(sFullFileName))
				return sFullFileName;

			return GetModuleFilesPath(aNamespace, CPathFinder::STANDARD) + sFileName;
		}
		case (CTBNamespace::DATAFILE):
		{
			ASSERT(!sCulture.IsEmpty());
			sFileName += szXmlExt;

			sFullFileName = GetModuleDataFilePath(aNamespace, CPathFinder::USERS, sUser) + SLASH_CHAR + sCulture + sFileName;
			if (ExistFile(sFullFileName))
				return sFullFileName;

			sFullFileName = GetModuleDataFilePath(aNamespace, CPathFinder::ALL_USERS) + SLASH_CHAR + sCulture + sFileName;
			if (ExistFile(sFullFileName))
				return sFullFileName;

			return GetModuleDataFilePath(aNamespace, CPathFinder::STANDARD) + SLASH_CHAR + sCulture + sFileName;
		}

		case (CTBNamespace::PROFILE):
		{
			CTBNamespace nsDocument =
				sFullFileName = GetDocumentExportProfilesPath(aNamespace, CPathFinder::USERS, sUser) + sFileName;
			if (ExistFile(sFullFileName + SLASH_CHAR + szDocument))
				return sFullFileName;

			sFullFileName = GetDocumentExportProfilesPath(aNamespace, CPathFinder::ALL_USERS) + sFileName;
			if (ExistFile(sFullFileName + SLASH_CHAR + szDocument))
				return sFullFileName;

			return GetDocumentExportProfilesPath(aNamespace, CPathFinder::STANDARD) + sFileName;
		}

		case CTBNamespace::WORDDOCUMENT:
		case CTBNamespace::WORDTEMPLATE:
		case CTBNamespace::ODT:
		{
			sFullFileName = GetModuleWordDocPath(aNamespace, CPathFinder::USERS, sUser) + sFileName;
			if (ExistFile(sFullFileName))
				return sFullFileName;

			sFullFileName = GetModuleWordDocPath(aNamespace, CPathFinder::ALL_USERS) + sFileName;
			if (ExistFile(sFullFileName))
				return sFullFileName;

			return GetModuleWordDocPath(aNamespace, CPathFinder::STANDARD) + sFileName;
		}

		case CTBNamespace::EXCELDOCUMENT:
		case CTBNamespace::EXCELTEMPLATE:
		case CTBNamespace::ODS:
		{
			sFullFileName = GetModuleExcelDocPath(aNamespace, CPathFinder::USERS, sUser) + sFileName;
			if (ExistFile(sFullFileName))
				return sFullFileName;

			sFullFileName = GetModuleExcelDocPath(aNamespace, CPathFinder::ALL_USERS) + sFileName;
			if (ExistFile(sFullFileName))
				return sFullFileName;

			return GetModuleExcelDocPath(aNamespace, CPathFinder::STANDARD) + sFileName;
		}

	}

	return sFullFileName;
}

//-------------------------------------------------------------------------------------
const CString CPathFinder::GetModuleConfigName() const
{
	return szModuleConfigName;
}

//-------------------------------------------------------------------------------------
const CString CPathFinder::GetAppConfigName() const
{
	return szAppConfigName;
}

//-------------------------------------------------------------------------------------
const CString CPathFinder::GetLocalAppConfigName() const
{
	return szLocalAppConfigName;
}




//-------------------------------------------------------------------------------------
const CString CPathFinder::GetTBDllPath() const
{
	return m_sTbDllPath;
}

//-------------------------------------------------------------------------------------
const CString CPathFinder::GetDllNameFromNamespace(const CTBNamespace& aNamespace) const
{
	ASSERT(m_pDictionaryPathFinder);
	return m_pDictionaryPathFinder->GetDllNameFromNamespace(aNamespace);
}

//-------------------------------------------------------------------------------------
const CString CPathFinder::GetInstallationName() const
{
	return m_sInstallation;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetClientFileSystemCacheName() const
{
	return GetTBDllPath() + SLASH_CHAR + szFileSystemCache;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetServerFileSystemCacheName() const
{
	return GetCustomPath() + SLASH_CHAR + szFileSystemCache;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetStandardPath() const
{
	return m_sStandardPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetInstallationPath() const
{
	if (m_bIsStandAlone)
		return GetPath(m_sStandardPath) + SLASH_CHAR;

	int nPos = m_sStandardPath.Find(szStandard);
	return nPos >= 0 ? m_sStandardPath.Left(nPos) : _T("");
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetCustomPath(BOOL bCreateDir) const
{
	if (bCreateDir)
		CreateDirectory(m_sCustomPath);

	return m_sCustomPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetCompaniesPath(BOOL bCreateDir) const
{
	//@@BAUZI: rename temporaneo così in sviluppo non si perdono le custom
	// poi sarà il processo di migrazione che si preoccuperà di fare il rename e di portare i file nella tabella TB_CustomData
	CString sCustomPath = GetCustomPath(bCreateDir);
	
	//CString sCompanyPath = sCustomPath + SLASH_CHAR + szCompanies;
	CString sSubscriptionPath = sCustomPath + SLASH_CHAR + szSubscription;
	
	/*if (::ExistPath(sCompanyPath))
		::RenameFile(sCompanyPath, sSubscriptionPath);
	else*/
	if (bCreateDir)
		CreateDirectory(sSubscriptionPath);
	return sSubscriptionPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetApplicationPath(const CString& sAppName, PosType pos, BOOL bCreateDir, Company aCompany) const
{
	if (pos == STANDARD)
		return m_sStandardPath + SLASH_CHAR + GetAppContainerName(sAppName) + SLASH_CHAR + sAppName;

	// container
	CString sPath = GetCompanyPath(bCreateDir) + SLASH_CHAR + GetAppContainerName(sAppName);

	// Personalizzazioni di EasyStudio: se sono su file system sono sulla home
	// mentre se sono nel database sono nella current company
	if	(aCompany == CPathFinder::EASYSTUDIO && (!AfxGetFileSystemManager()->IsManagedByAlternativeDriver(sPath)))
		sPath = GetEasyStudioCustomizationsPath();

	if (bCreateDir)
		CreateDirectory(sPath);

	// applicazione
	sPath += SLASH_CHAR + sAppName;
	if (bCreateDir)
		CreateDirectory(sPath);

	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetConfigurationPath() const
{
	return m_sCustomPath + SLASH_CHAR + szConfiguration;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetEasyStudioReferencedAssembliesPath() const
{
	if (m_eESAppPosType == CPathFinder::CUSTOM)
		return GetEasyStudioHomePath() + SLASH_CHAR + szReferencedAssemblies;
	
	return GetStandardPath() + SLASH_CHAR + szReferencedAssemblies;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetEasyStudioEnumsAssemblyName() const
{
	return GetEasyStudioReferencedAssembliesPath() + SLASH_CHAR + _T("Microarea.EasyBuilder.Enums.dll");
}

//-----------------------------------------------------------------------------
BOOL CPathFinder::IsEasyStudioPath(const CString& strFileName)
{
	CString sESHome = GetEasyStudioHomePath();
	CString sPath = strFileName;
	sESHome = sESHome.MakeLower();
	sPath.MakeLower();
	return sPath.FindOneOf(sESHome) > 0;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetCompanyPath(BOOL bCreateDir) const
{
	CString sPath, sCompanyName = GetCompanyName();
	if (sCompanyName.IsEmpty())
		sPath = GetAllCompaniesPath(bCreateDir);
	else
		sPath = GetCompaniesPath(bCreateDir) + SLASH_CHAR + sCompanyName;

	if (bCreateDir)
		CreateDirectory(sPath);

	return sPath;
}
//-----------------------------------------------------------------------------
const CString CPathFinder::GetTempPath(BOOL bCreateDir) const
{
	CString sPath = GetCustomPath() + SLASH_CHAR + _T("Temp");

	if (bCreateDir)
		CreateDirectory(sPath);

	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetAppDataPath(BOOL bCreateDir) const
{
	//prima provo nella local App Data, se girassi in IIS allora questa potrebbe essere vuota,
	//allora uso la Common App Data
	CString sPath;
	TCHAR* sz = sPath.GetBuffer(MAX_PATH);
	//su alcuni server (2008, 2003) potrebbero non esserci i diritti di scrittura sulle cartelle usate nel commento
	/*SHGetSpecialFolderPath(NULL, sz, CSIDL_LOCAL_APPDATA, bCreateDir);
	int len = _tcslen(sz);
	if (len ==  0)
	SHGetSpecialFolderPath(NULL, sz, CSIDL_COMMON_APPDATA, bCreateDir);*/
	::GetTempPath(MAX_PATH, sz);
	int len = _tcslen(sz);
	sz += len;

	_stprintf_s(sz, MAX_PATH - len, _T("%s\\%s\\%s\\%s"),
		szTempFolder,
		(LPCTSTR)GetServerName(),
		(LPCTSTR)GetInstallationName(),
		(LPCTSTR)(::GetName(::GetProcessFileName())));
	sPath.ReleaseBuffer();


	if (bCreateDir && !ExistPath(sPath))
		CreateDirectory(sPath);

	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetWebProxyImagesPath(BOOL bCreateDir) const
{
	CString sPath = GetTempPath(bCreateDir) + SLASH_CHAR + _T("WebProxyImages");

	if (bCreateDir)
		CreateDirectory(sPath);

	return sPath;
}

//Path dove vengono salvati i file di cui si sta facendo il download prima di essere inviati al browser
//-----------------------------------------------------------------------------
const CString CPathFinder::GetWebProxyFilesPath(BOOL bCreateDir) const
{
	CString sPath = GetTempPath(bCreateDir) + SLASH_CHAR + _T("WebProxyFiles");

	if (bCreateDir)
		CreateDirectory(sPath);

	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetAllCompaniesPath(BOOL bCreateDir) const
{
	CString sPath = GetCompaniesPath(bCreateDir) + SLASH_CHAR + szAllCompanies;
	if (bCreateDir)
		CreateDirectory(sPath);
	return sPath;
}

//-----------------------------------------------------------------------------
void CPathFinder::SetEasyStudioParams(PosType posType, CString strHomeName) 
{ 
	m_eESAppPosType = posType; 
	// lascio il default se serve
	if (!m_sESHome.IsEmpty())
		m_sESHome = strHomeName;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetEasyStudioHomePath(BOOL bCreateDir /*FALSE*/) const
{
	CString sPath = m_eESAppPosType == CPathFinder::CUSTOM ?
		GetCompaniesPath(bCreateDir) + SLASH_CHAR + m_sESHome :
		GetStandardPath();

	if (bCreateDir)
		CreateDirectory(sPath);
	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetEasyStudioCustomizationsPath(BOOL bCreateDir /*FALSE*/) const
{
	CString sPath = GetEasyStudioHomePath(bCreateDir) + SLASH_CHAR + szContainerApplications;
	if (bCreateDir)
		CreateDirectory(sPath);
	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetModulePath(const CString& sAppName, const CString& sModuleName, PosType pos, BOOL bCreateDir, Company aCompany) const
{
	CString sPath = GetApplicationPath(sAppName, pos, bCreateDir, aCompany) + SLASH_CHAR + sModuleName;
	if (bCreateDir)
		CreateDirectory(sPath);

	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetModulePath(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir, Company aCompany) const
{
	//se sono un namespace dinamico, pesco dalla custom e non dalla standard
	if (pos == STANDARD && aNamespace.HasAFakeLibrary())
		pos = CUSTOM;

	CString sPath = GetApplicationPath(aNamespace.GetApplicationName(), pos, bCreateDir, aCompany)
		+ SLASH_CHAR + aNamespace.GetObjectName(CTBNamespace::MODULE);
	if (bCreateDir)
		CreateDirectory(sPath);

	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetModuleSettingsPath(const CTBNamespace& aNamespace, PosType pos, const CString sUserRole, BOOL bCreateDir, Company aCompany) const
{
	CString sPath = GetModulePath(aNamespace, pos, bCreateDir, aCompany) + SLASH_CHAR + szSettings;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetModuleObjectsPath(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir, Company aCompany) const
{
	CString sPath = GetModulePath(aNamespace, pos, bCreateDir, aCompany) + SLASH_CHAR + szModuleObjects;
	if (bCreateDir)
		CreateDirectory(sPath);

	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetJsonFormsPath(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir, Company aCompany, const CString& sUserRole) const
{
	CString sPath = GetModulePath(aNamespace, pos, bCreateDir, aCompany) + SLASH_CHAR + szJsonForms;
	if (bCreateDir)
		CreateDirectory(sPath);
	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);;
}
//-----------------------------------------------------------------------------
const CString CPathFinder::GetModuleDictionaryFilePath(const CTBNamespace& aNamespace, BOOL bFromStandard, const CString& strCulture) const
{
	if (bFromStandard)
		return GetModulePath(aNamespace, CPathFinder::STANDARD, FALSE) + SLASH_CHAR + szDictionary + SLASH_CHAR + strCulture + SLASH_CHAR + szDictionaryFile;

	CString sPath;
	_stprintf_s(sPath.GetBuffer(MAX_PATH), MAX_PATH, _T("%s\\%s\\%s.%s.%s"),
		(LPCTSTR)GetTBDllPath(),
		(LPCTSTR)strCulture,
		(LPCTSTR)aNamespace.GetApplicationName(),
		(LPCTSTR)aNamespace.GetModuleName(),
		(LPCTSTR)szDictionaryFile);
	sPath.ReleaseBuffer();

	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetServerConnectionConfigFullName() const
{
	return  GetCustomPath() + SLASH_CHAR + szServerConnectionConfig;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetDataPath(PosType pos) const
{
	if (pos == STANDARD)
		return GetStandardPath() + SLASH_CHAR + szData;

	return GetCustomPath() + SLASH_CHAR + szData;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetDataDefaultPath(PosType pos) const
{
	return GetDataPath(pos) + SLASH_CHAR + szDefault;
}

//-----------------------------------------------------------------------------
void CPathFinder::GetAllModulePath(
	CStringArray&		aAllModulePath,
	CTBNamespace&		aNamespace,
	const CStringArray& aUser,
	BOOL				bStd/*= TRUE*/,
	BOOL				bAllUsers/*= TRUE*/,
	BOOL				bSearchStd/*= TRUE*/,
	BOOL				bIsFromApp /*= TRUE*/,
	const CString		strModuleName /*= _T("")*/
	) const
{
	CString			strModulePath;

	aNamespace.SetObjectName(CTBNamespace::MODULE, strModuleName);

	if (bIsFromApp)
	{
		switch (aNamespace.GetType())
		{
		case (CTBNamespace::REPORT) :
		{
			for (int n = 0; n <= aUser.GetUpperBound(); n++)
			{
				//	if (aUser.GetAt(n).CompareNoCase(szStandard) == 0 && bSearchStd)
				//		continue;

				strModulePath = GetModuleReportPath(aNamespace, CPathFinder::USERS, aUser.GetAt(n));
				aAllModulePath.Add(strModulePath);
			}

			if (bAllUsers)//(bSearchStd)
			{
				strModulePath = GetModuleReportPath(aNamespace, CPathFinder::ALL_USERS);
				aAllModulePath.Add(strModulePath);
			}

			if (bStd)//(bSearchStd)
			{
				strModulePath = GetModuleReportPath(aNamespace, CPathFinder::STANDARD);
				aAllModulePath.Add(strModulePath);
			}

			break;
		}
		case (CTBNamespace::IMAGE) :
		case (CTBNamespace::TEXT) :
		case (CTBNamespace::PDF) :
		case (CTBNamespace::RTF) :
								 //case (CTBNamespace::ODF):
		case (CTBNamespace::FILE) :
		{
			for (int n = 0; n <= aUser.GetUpperBound(); n++)
			{
				//if (aUser.GetAt(n).CompareNoCase(szStandard) == 0 && bSearchStd)
				//	continue;

				strModulePath = GetModuleFilesPath(aNamespace, CPathFinder::USERS, aUser.GetAt(n));
				aAllModulePath.Add(strModulePath);
			}

			if (bAllUsers)
			{
				strModulePath = GetModuleFilesPath(aNamespace, CPathFinder::ALL_USERS);
				aAllModulePath.Add(strModulePath);
			}

			if (bStd)//(bSearchStd)
			{
				strModulePath = GetModuleFilesPath(aNamespace, CPathFinder::STANDARD);
				aAllModulePath.Add(strModulePath);
			}

			break;
		}
		}
	}
	else
	{
		switch (aNamespace.GetType())
		{
		case (CTBNamespace::REPORT) :
		{
			for (int n = 0; n <= aUser.GetUpperBound(); n++)
			{
				//if (aUser.GetAt(n).CompareNoCase(szStandard) == 0 && bSearchStd)
				//	continue;

				strModulePath = GetModuleReportPath(aNamespace, CPathFinder::USERS, aUser.GetAt(n));
				aAllModulePath.Add(strModulePath);
			}

			if (bStd) //(bSearchStd)
			{
				strModulePath = GetModuleReportPath(aNamespace, CPathFinder::STANDARD);
				aAllModulePath.Add(strModulePath);
			}

			if (bAllUsers)
			{
				strModulePath = GetModuleReportPath(aNamespace, CPathFinder::ALL_USERS);
				aAllModulePath.Add(strModulePath);
			}
			break;
		}
		case (CTBNamespace::IMAGE) :
		case (CTBNamespace::TEXT) :
		case (CTBNamespace::PDF) :
		case (CTBNamespace::RTF) :
								 //case (CTBNamespace::ODF):
		case (CTBNamespace::FILE) :
		{
			for (int n = 0; n <= aUser.GetUpperBound(); n++)
			{
				//if (aUser.GetAt(n).CompareNoCase(szStandard) == 0 && bSearchStd)
				//continue;

				strModulePath = GetModuleFilesPath(aNamespace, CPathFinder::USERS, aUser.GetAt(n));
				aAllModulePath.Add(strModulePath);
			}

			if (bStd)//(bSearchStd)
			{
				strModulePath = GetModuleFilesPath(aNamespace, CPathFinder::STANDARD);
				aAllModulePath.Add(strModulePath);
			}

			if (bAllUsers)
			{
				strModulePath = GetModuleFilesPath(aNamespace, CPathFinder::ALL_USERS);
				aAllModulePath.Add(strModulePath);
			}
			break;
		}
		}
	}
}

//-----------------------------------------------------------------------------
void CPathFinder::GetAllObjInFolder(CStringArray& aAllObjInFolder, CTBNamespace& aNamespace, const CStringArray& aAllModulesPath, const CString strNameSearch /*= _T("*")*/, const CString strExtSearch /*= _T("")*/) const
{
	CStringArray 	aStrExt;
	CString			strExt = _T("");
	CString			strFound = _T("");
	CString			strExtNew = _T("");
	CString			strExtOld = _T("");
	CString			strSearchFile = _T("");

	if (strExtSearch.IsEmpty())
		GetObjectSearchExtensions(aNamespace, &aStrExt, FALSE);
	else
	{
		CStringArray aExtPossible;
		GetObjectSearchExtensions(aNamespace, &aExtPossible, FALSE);
		BOOL bExist = FALSE;
		for (int s = 0; s <= aExtPossible.GetUpperBound(); s++)
		{
			CString	sExt = aExtPossible.GetAt(s);
			if (_tcsicmp(sExt, strExtSearch) == 0)
			{
				bExist = TRUE;
				break;
			}
		}

		if (!bExist)
		{
			ASSERT(FALSE);	// estensione strExtSearch non compatibile con il tipo di NS
			return;
		}

		if (strExtSearch.Find('.') < 0)
			aStrExt.Add(_T(".") + strExtSearch);
		else
			aStrExt.Add(strExtSearch);
	}

	for (int n = 0; n <= aStrExt.GetUpperBound(); n++)
	{
		strExt = aStrExt.GetAt(n);
		if (_tcsicmp(strExt, _T(".*")) == 0)
			strExt = _T("");
		for (int i = 0; i <= aAllModulesPath.GetUpperBound(); i++)
		{
			strSearchFile = aAllModulesPath.GetAt(i);

			CStringArray arFiles;
			AfxGetFileSystemManager()->GetFiles(strSearchFile, strExt, &arFiles);

			for (int f = 0; f <= arFiles.GetUpperBound(); f++)
			{
				CString strFileName = arFiles.GetAt(f);
				if ((strFileName != ".") && (strFileName != ".."))
				{
					strExtNew = _T("*") + GetExtension(strFileName);
					strExtOld = aStrExt.GetAt(n);

					if (_tcsicmp(strExtOld, _T("*.*")) != 0)
						// pb estensioni: per evitare che x es. se cerco *.htm mi venga restituito come valido sia x.htm che x.html
						if (_tcsicmp(strExtNew, strExtOld) != 0)
							continue;

					strFound = strFileName;
					aAllObjInFolder.Add(strFound);
				}
			}
		}
	}
}


//-----------------------------------------------------------------------------
const CString CPathFinder::GetModuleReportPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir) const
{
	CString sPath = GetModulePath(aNamespace, pos, bCreateDir) + SLASH_CHAR + szReport;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetModuleWordDocPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir) const
{
	CString sPath = GetModulePath(aNamespace, pos, bCreateDir) + SLASH_CHAR + szWord;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetModuleExcelDocPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir) const
{
	CString sPath = GetModulePath(aNamespace, pos, bCreateDir) + SLASH_CHAR + szExcel;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetDocumentPath(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir, Company aCompany, const CString& sUserRole /*= _T("")*/) const
{
	if (aNamespace.GetObjectName(CTBNamespace::DOCUMENT).IsEmpty())
	{
		TRACE1("The namespace parameter is not a Document namespace but %s. Document namespace is needed.", aNamespace.ToString());
		ASSERT(FALSE);

	}

	CString sPath = GetModuleObjectsPath(aNamespace, pos, bCreateDir, aCompany) + SLASH_CHAR + aNamespace.GetObjectName(CTBNamespace::DOCUMENT);
	if (bCreateDir)
		CreateDirectory(sPath);

	if (!sUserRole.IsEmpty() && (IsCustomPath(sPath) || aCompany == CPathFinder::EASYSTUDIO))
	{
		sPath = sPath + SLASH_CHAR + sUserRole;
		if (bCreateDir)
			CreateDirectory(sPath);
	}

	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetDocumentRadarPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir) const
{
	CString sPath = GetDocumentPath(aNamespace, pos, bCreateDir) + SLASH_CHAR + szRadar;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetDocumentQueryPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir) const
{
	CString sPath = GetDocumentPath(aNamespace, pos, bCreateDir) + SLASH_CHAR + szQuery;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetDocumentDescriptionPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir /*= FALSE*/, Company aCompany) const
{
	CString sPath = GetDocumentPath(aNamespace, pos, bCreateDir, aCompany) + SLASH_CHAR + szDescription;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}
//-----------------------------------------------------------------------------
const CString CPathFinder::GetDocumentSchemaPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir /*= FALSE*/, Company aCompany) const
{
	CString sPath = GetDocumentPath(aNamespace, pos, bCreateDir, aCompany) + SLASH_CHAR + szExportProfiles;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetDocumentReportsFile(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir /*= FALSE*/) const
{
	CString sPath = GetDocumentDescriptionPath(aNamespace, pos, sUserRole, bCreateDir);

	return sPath + SLASH_CHAR + szReportsXML;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetDocumentEventsFile(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir /*= FALSE*/) const
{
	CString sPath = GetDocumentDescriptionPath(aNamespace, pos, sUserRole, bCreateDir);

	return sPath + SLASH_CHAR + szEventsXML;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetDocumentFormNSChangesFile(const CTBNamespace& aNamespace) const
{
	CString sPath = GetDocumentDescriptionPath(aNamespace, CPathFinder::STANDARD);

	return sPath + SLASH_CHAR + szFormNsChangesXML;
}

//-----------------------------------------------------------------------------
CString CPathFinder::GetXmlParametersDirectory() 
{
	CString xmlParametersDirectory = GetCustomPath() + SLASH_CHAR;
	if (xmlParametersDirectory == _T(""))
		return _T("");

	//Companies
	xmlParametersDirectory = xmlParametersDirectory + _T("Companies");
	xmlParametersDirectory = xmlParametersDirectory + SLASH_CHAR;

	// GetCustomCompanyPath();
	xmlParametersDirectory = xmlParametersDirectory + AfxGetLoginInfos()->m_strCompanyName;

	// <CustomCompanyPath>\ScheduledTasks\Parameters\<user>
	xmlParametersDirectory = xmlParametersDirectory + SLASH_CHAR;
	xmlParametersDirectory = xmlParametersDirectory + _T("ScheduledTasks");
	xmlParametersDirectory = xmlParametersDirectory + SLASH_CHAR;
	xmlParametersDirectory = xmlParametersDirectory + _T("Parameters");
	xmlParametersDirectory = xmlParametersDirectory + SLASH_CHAR;
	xmlParametersDirectory = xmlParametersDirectory + AfxGetLoginInfos()->m_strUserName;

	return xmlParametersDirectory;

}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetDocumentRadarsFile(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir /*= FALSE*/) const
{
	CString sPath = GetDocumentDescriptionPath(aNamespace, pos, sUserRole, bCreateDir);

	return sPath + SLASH_CHAR + szRadarsXML;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetDocumentBarcodeFile(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir /*= FALSE*/) const
{
	CString sPath = GetDocumentDescriptionPath(aNamespace, pos, sUserRole, bCreateDir);

	return sPath + SLASH_CHAR + szBarcodeXML;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentDefaultsFile(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir /*= FALSE*/) const
{
	CString sPath = GetDocumentDescriptionPath(aNamespace, pos, sUserRole, bCreateDir);

	return sPath + SLASH_CHAR + szDefaults;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetAppDataIOPath(BOOL bCreateDir, Company aCompany) const
{
	CString sPath = GetCompanyPath(bCreateDir) + SLASH_CHAR + szDataIO;

	if (bCreateDir)
		CreateDirectory(sPath);

	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetAppXTechDataIOPath(BOOL bCreateDir, Company aCompany) const
{
	CString sPath = GetAppDataIOPath(bCreateDir) + SLASH_CHAR + szXTechDataIO;
	if (bCreateDir)
		CreateDirectory(sPath);

	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetLogDataIOPath(BOOL bCreateDir, Company aCompany) const
{
	CString sPath = GetCompanyPath(bCreateDir) + SLASH_CHAR + szLogDataIO;
	if (bCreateDir)
		CreateDirectory(sPath);

	return sPath;
}

//-----------------------------------------------------------------------------
void  _AddSubFolder(CString& sSubPath, const CString& tk)
{
	if (sSubPath.IsEmpty())
	{
		sSubPath = tk;
		return;
	}

	if (!tk.IsEmpty())
	{
		CString s(sSubPath); s.MakeLower();
		CString sub(tk); sub.MakeLower();
		if (s.Find(sub) != 0)
			sSubPath = tk + SLASH_CHAR + sSubPath;
	}

	// devo sostituire eventuali separatori del namespace con SLASH_CHAR 
	sSubPath.Replace(CTBNamespace::GetSeparator(), CString(SLASH_CHAR));
}

int _FindNoCase(CString s, CString sub)	//per dipendenza inversa non posso usare qella in TbGeneric/GeneralFunctions
{
	s.MakeLower();
	sub.MakeLower();
	return s.Find(sub);
}

const CString CPathFinder::GetModuleFilesPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir /*FALSE*/, Company aCompany /*= CURRENT*/) const
{
	// mi faccio ritornare i subfolder del namespace
	CString sSubPath = aNamespace.GetPathInside();

	// aggiungo i subfolder impliciti del namespace (se non ci sono gi�)
	switch (aNamespace.GetType())
	{
		case CTBNamespace::IMAGE:	::_AddSubFolder(sSubPath, szImages);	break;
		case CTBNamespace::TEXT:	::_AddSubFolder(sSubPath, szTexts);	break;
		case CTBNamespace::PDF:		::_AddSubFolder(sSubPath, szPdf);		break;
		case CTBNamespace::RTF:		::_AddSubFolder(sSubPath, szRtf);		break;
		default: // CTBNamespace::FILE			
		{	
			if (
					::_FindNoCase(sSubPath, L"files.others") == 0 ||
					::_FindNoCase(sSubPath, L"files.images") == 0 ||
					::_FindNoCase(sSubPath, L"files.texts") == 0 ||
					::_FindNoCase(sSubPath, L"files.pdf") == 0 ||
					::_FindNoCase(sSubPath, L"files.rtf") == 0
				)
			{
				sSubPath = sSubPath.Mid(6);
				sSubPath.Replace(CTBNamespace::GetSeparator(), CString(SLASH_CHAR));
			}
			else if (
					::_FindNoCase(sSubPath, szOthers) == 0 ||
					::_FindNoCase(sSubPath, szImages) == 0 ||
					::_FindNoCase(sSubPath, szTexts) == 0 ||
					::_FindNoCase(sSubPath, szPdf) == 0 ||
					::_FindNoCase(sSubPath, szRtf) == 0
					)
			{
				sSubPath.Replace(CTBNamespace::GetSeparator(), CString(SLASH_CHAR));
			}
			else
				_AddSubFolder(sSubPath, szOthers);	break;
		}
	}

	CString sPath = GetModulePath(aNamespace, pos, bCreateDir, aCompany) + SLASH_CHAR + szFiles;
	if (bCreateDir)
		CreateDirectory(sPath);
	sPath += SLASH_CHAR + sSubPath;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetModuleXmlPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir) const
{
	CString sPath = GetModulePath(aNamespace, pos, bCreateDir);
	if (sPath.ReverseFind('\\') != (sPath.GetLength() - 1))
		sPath += SLASH_CHAR;
	sPath += szXml;

	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetModuleXmlPathCulture(const CTBNamespace& aNamespace, PosType pos, const CString& sCulture, const CString& sUserRole, BOOL bCreateDir) const
{
	CString sPath = GetModuleXmlPath(aNamespace, pos, sUserRole, bCreateDir) + SLASH_CHAR + sCulture;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetModuleHelpPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir/*FALSE*/) const
{
	CString sPath = GetModulePath(aNamespace, pos, bCreateDir) + SLASH_CHAR + szHelp;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetModuleDataFilePath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir) const
{
	CString sPath = GetModulePath(aNamespace, pos, bCreateDir) + SLASH_CHAR + szDataManager + SLASH_CHAR + szDataFile;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetFontsFullName(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir /*FALSE*/) const
{
	return GetModuleObjectsPath(aNamespace, pos, bCreateDir) + SLASH_CHAR + szFontsFileName;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetFormatsFullName(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir /*FALSE*/) const
{
	return GetModuleObjectsPath(aNamespace, pos, bCreateDir) + SLASH_CHAR + szFormatsFileName;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentXSLTImportFullName(const CTBNamespace& aNamespace) const
{
	return GetDocumentPath(aNamespace, CPathFinder::STANDARD) + SLASH_CHAR + szXSLTImport;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentXSLTExportFullName(const CTBNamespace& aNamespace) const
{
	return GetDocumentPath(aNamespace, CPathFinder::STANDARD) + SLASH_CHAR + szXSLTExport;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetEnumsFullName(const CTBNamespace& aNamespace, PosType pos) const
{
	return GetModuleObjectsPath(aNamespace, pos, FALSE) + CString(SLASH_CHAR) + szEnumsFileName;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentDbtsFullName(const CTBNamespace& aNamespace, PosType pos) const
{
	return GetDocumentDescriptionPath(aNamespace, pos, FALSE) + SLASH_CHAR + szDBTS;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentDbtsFullName(const CTBNamespace& aNamespace) const
{
	return GetDocumentDbtsFullName(aNamespace, CPathFinder::STANDARD);
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentActionsFullName(const CTBNamespace& aNamespace) const
{
	return GetDocumentActionsFullName(aNamespace, CPathFinder::STANDARD);
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentActionsFullName(const CTBNamespace& aNamespace, PosType pos) const
{
	return GetDocumentDescriptionPath(aNamespace, pos) + SLASH_CHAR + szActions;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentSelectionsFullName(const CTBNamespace& aNamespace, PosType pos) const
{
	return GetDocumentDescriptionPath(aNamespace, pos) + SLASH_CHAR + szSelections;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentDocumentFullName(const CTBNamespace& aNamespace, PosType pos) const
{
	return GetDocumentDescriptionPath(aNamespace, pos) + SLASH_CHAR + szDocument;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentDocumentFullName(const CTBNamespace& aNamespace) const
{
	return GetDocumentDocumentFullName(aNamespace, CPathFinder::STANDARD);
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentXRefFullName(const CTBNamespace& aNamespace, PosType pos) const
{
	return GetDocumentDescriptionPath(aNamespace, pos) + SLASH_CHAR + szExternalReference;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentXRefFullName(const CTBNamespace& aNamespace) const
{
	return GetDocumentXRefFullName(aNamespace, CPathFinder::STANDARD);
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentCodingRulesFullName(const CTBNamespace& aNamespace, PosType pos) const
{
	CString strDocPath = GetDocumentDescriptionPath(aNamespace, pos);
	if (strDocPath.IsEmpty())
		return NULL;

	return strDocPath + SLASH_CHAR + szCodingRules;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetSemaphoreFilePath() const
{
	return m_sStandardPath + SLASH_CHAR + szContainerTaskBuilder + SLASH_CHAR + szTaskBuilderApp + SLASH_CHAR + szAppConfigName;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDateRangesFilePath() const
{
	return m_sStandardPath + SLASH_CHAR + szContainerTaskBuilder + SLASH_CHAR + szTaskBuilderApp + SLASH_CHAR + szDateRangesXML;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetTaskBuilderXmlPath() const
{
	return GetApplicationPath(szTaskBuilderApp, CPathFinder::STANDARD) + SLASH_CHAR + szXml;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetReportFullName(const CTBNamespace& aNamespace, const CString& sUser) const
{
	if (aNamespace.GetType() != CTBNamespace::REPORT)
	{
		TRACE1("The namespace parameter is not a Report namespace but %s. Report namespace is needed.", aNamespace.ToString());
		ASSERT(FALSE);
	}

	return GetFileNameFromNamespace(aNamespace, sUser);
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetReportFullNameIn(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole) const
{
	if (aNamespace.GetType() != CTBNamespace::REPORT)
	{
		TRACE1("The namespace parameter is not a Report namespace but %s. Report namespace is needed.", aNamespace.ToString());
		ASSERT(FALSE);
	}

	CString sExtension = GetObjectDefaultExtension(aNamespace);
	CString sFileName = SLASH_CHAR + aNamespace.GetObjectName();
	if (::GetExtension(sFileName).IsEmpty())
		sFileName += sExtension;

	return GetModuleReportPath(aNamespace, pos, sUserRole) + sFileName;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDatabaseObjectsFullName(const CTBNamespace& aNamespace, PosType pos) const
{
	return GetModuleObjectsPath(aNamespace, pos, FALSE) + SLASH_CHAR + szDatabaseObjectsXml;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetAddOnDbObjectsFullName(const CTBNamespace& aNamespace, PosType pos) const
{
	return GetModuleObjectsPath(aNamespace, pos, FALSE) + SLASH_CHAR + szAddOnDbObjects;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDocumentObjectsFullName(const CTBNamespace& aNamespace, PosType pos, Company aCompany /*CURRENT*/) const
{
	return GetModuleObjectsPath(aNamespace, pos, FALSE, aCompany) + SLASH_CHAR + szDocumentObjects;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetBehaviourObjectsFullName(const CTBNamespace& aNamespace, PosType pos) const
{
	return GetModuleObjectsPath(aNamespace, pos, FALSE) + SLASH_CHAR + szBehaviourObjects;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetClientDocumentObjectsFullName(const CTBNamespace& aNamespace, PosType pos /*= CPathFinder::STANDARD*/, Company aCompany /*= CPathFinder::CURRENT*/) const
{
	return GetModuleObjectsPath(aNamespace, pos, FALSE, aCompany) + SLASH_CHAR + szClientDocumentObjects;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetResourcesFullFileName(const CTBNamespace& aNamespace) const
{
	return	GetModuleObjectsPath(aNamespace, CPathFinder::STANDARD) + SLASH_CHAR +
		szSharedViews + SLASH_CHAR +
		CString(szResourcesFolder) + SLASH_CHAR +
		szResources;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetWebMethodsFullName(const CTBNamespace& aNamespace) const
{
	return GetModuleObjectsPath(aNamespace, CPathFinder::STANDARD) + SLASH_CHAR + szWebMethods;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetEventHandlerObjectsFullName(const CTBNamespace& aNamespace) const
{
	return GetModuleObjectsPath(aNamespace, CPathFinder::STANDARD) + SLASH_CHAR + szEventHandlersObjects;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetItemSourceObjectsFullName(const CTBNamespace& aNamespace) const
{
	return GetModuleObjectsPath(aNamespace, CPathFinder::STANDARD) + SLASH_CHAR + szItemSourcesObjects;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetModuleReferenceObjectsPath(const CTBNamespace& aNamespace, PosType pos, const CString& sUserRole, BOOL bCreateDir, Company aCompany) const
{
	CString sPath = GetModulePath(aNamespace, pos, bCreateDir, aCompany) + SLASH_CHAR + szReferenceObjects;
	if (bCreateDir)
		CreateDirectory(sPath);

	return ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

// Pu� ricevere un namespace di un documento o di un profilo
//-----------------------------------------------------------------------------
const CString CPathFinder::GetDocumentExportProfilesPath(const CTBNamespace& aNamespace, PosType pos, const CString& strUserRole /*= _T("")*/, BOOL bCreateDir /*= FALSE*/, Company aCompany /*= CURRENT*/) const
{
	if (aNamespace.GetType() != CTBNamespace::DOCUMENT && aNamespace.GetType() != CTBNamespace::PROFILE)
	{
		TRACE1("The namespace parameter is not a Document or Profile namespace but %s. Document or Profile namespace is needed.", aNamespace.ToString());
		ASSERT(FALSE);
	}

	CString strPath = GetModuleObjectsPath(aNamespace, pos, bCreateDir, aCompany) + SLASH_CHAR + aNamespace.GetObjectName(CTBNamespace::DOCUMENT);
	if (bCreateDir)
		CreateDirectory(strPath);
	strPath += SLASH_CHAR;
	strPath += szExportProfiles;
	return ToPosDirectory(strPath, pos, strUserRole, bCreateDir);
}

//dato il namespace di un profilo restituisce la path di memorizzazione in base al PosType passato ed all'eventuale user
//-----------------------------------------------------------------------------
const CString CPathFinder::GetExportProfilePath(const CTBNamespace& nsDocument, const CString& strProfileName, PosType posType, const CString& strUserRole /*= _T("")*/, BOOL bCreateDir /*= FALSE*/, Company aCompany /*= CURRENT*/) const
{
	if (strProfileName.IsEmpty())
		return AfxGetPathFinder()->GetDocumentDescriptionPath(nsDocument, posType, strUserRole, bCreateDir, aCompany);

	CString strPath = GetDocumentExportProfilesPath(nsDocument, posType, strUserRole, bCreateDir, aCompany);
	if (!strPath.IsEmpty())
	{
		strPath += SLASH_CHAR;
		strPath += strProfileName;
		if (bCreateDir)
			CreateDirectory(strPath);
	}
	return strPath;
}


//-----------------------------------------------------------------------------
const CString CPathFinder::GetExportProfilePath(const CTBNamespace& nsProfile, PosType pos, const CString& strUserRole /*= _T("")*/, BOOL bCreateDir /*= FALSE*/, Company aCompany /*= CURRENT*/) const
{
	if (nsProfile.GetType() != CTBNamespace::PROFILE)
	{
		TRACE1("The namespace parameter is not a Profile namespace but %s. Profile namespace is needed.", nsProfile);
		ASSERT(FALSE);
	}

	CString strProfileName = nsProfile.GetObjectName(CTBNamespace::PROFILE);
	CTBNamespace nsDocument(nsProfile);
	nsDocument.SetObjectName(_T(""));
	nsDocument.SetType(CTBNamespace::DOCUMENT);

	return GetExportProfilePath(nsDocument, strProfileName, pos, strUserRole, bCreateDir, aCompany);
}

//dato il namespace di un profilo restituisce la path di memorizzazione (in base al PosType passato ed all'eventuale user) da utilizzare dai clientdoc
//-----------------------------------------------------------------------------
const CString CPathFinder::GetPartialProfilePathForClientDoc(const CTBNamespace& aProfileNamespace, PosType pos, const CString& strUserRole /*= _T("")*/, BOOL bCreateDir /*= FALSE*/, Company aCompany /*= CURRENT*/) const
{
	if (aProfileNamespace.GetType() != CTBNamespace::PROFILE)
	{
		TRACE1("The namespace parameter is not a Profile namespace but %s. Profile namespace is needed.", aProfileNamespace.ToString());
		ASSERT(FALSE);
	}

	CString strProfileName = aProfileNamespace.GetObjectName(CTBNamespace::PROFILE);
	CString strDocumentName = aProfileNamespace.GetObjectName(CTBNamespace::DOCUMENT);

	if (strProfileName.IsEmpty() || strDocumentName.IsEmpty())
	{
		TRACE0("Profile name or document name is empty.");
		ASSERT(FALSE);
	}

	CString strPath = szExportProfiles;
	strPath += SLASH_CHAR;
	if (pos == CPathFinder::STANDARD)
		strPath += strDocumentName + SLASH_CHAR + strProfileName;
	else
	{
		if (pos == CPathFinder::ALL_USERS)
		{
			strPath += szAllUserDirName;
			strPath += SLASH_CHAR + strDocumentName + SLASH_CHAR + strProfileName;
		}
		else
		{
			if (pos == CPathFinder::USERS)
			{
				strPath += szUsers;
				strPath += SLASH_CHAR + ToUserDirectory(strUserRole) + SLASH_CHAR + strDocumentName + SLASH_CHAR + strProfileName;
			}
		}
	}

	if (bCreateDir)
		CreateDirectory(strPath);

	return strPath;
}


//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetModuleReferenceObjectsSearch() const
{
	return _T("*.xml");
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetOutDateObjectsFullName(const CTBNamespace& aNamespace) const
{
	return GetModuleObjectsPath(aNamespace, CPathFinder::STANDARD) + SLASH_CHAR + szOutDateObjects;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetEnvelopeObjectsFullName(const CTBNamespace& aNamespace) const
{
	return GetModuleObjectsPath(aNamespace, CPathFinder::STANDARD) + SLASH_CHAR + szEnvelopeObjects;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetModuleConfigFullName(const CString& sAppName, const CString& sModuleName) const
{
	CString sModulePath = GetModulePath(sAppName, sModuleName, CPathFinder::STANDARD);
	if (!ExistPath(sModulePath))
		sModulePath = GetModulePath(sAppName, sModuleName, CPathFinder::CUSTOM, FALSE, CPathFinder::EASYSTUDIO);

	return sModulePath + SLASH_CHAR + szModuleConfigName;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetXRefFileFromThisPath(const CString& strPath)
{
	if (strPath.IsEmpty())
	{
		TRACE0("The strPath parameter is empty.");
		ASSERT(FALSE);
		return _T("");
	}

	CString strTempPath = strPath;
	if (!IsDirSeparator(strTempPath.Right(1)))
		strTempPath += SLASH_CHAR;

	return  strTempPath + szExternalReference;
}
//-----------------------------------------------------------------------------
void CPathFinder::GetProfilesFromPath(const CString& strProfilesPath, CStringArray& pProfilesList, BOOL bAppend /*=TRUE*/)
{
	if (strProfilesPath.IsEmpty())
		return;

	BOOL bFound;

	CStringArray arProfiles;
	CString strPath = strProfilesPath;
	AfxGetFileSystemManager()->GetSubFolders(strProfilesPath, &arProfiles);

	for (int i = 0; i <= arProfiles.GetUpperBound(); i++)
	{
		CString strFileName = CString(arProfiles.GetAt(i));
		// per essere un profilo da caricare deve avere il file document.xml.
		// questo perch� si utilizza la custom di un profilo anche per il salvataggio dei soli file ExpCriteriaVars.xml
		if (strFileName == "." || strFileName == ".." || !ExistFile(strProfilesPath + SLASH_CHAR + strFileName + SLASH_CHAR + szDocument))
			continue;

		//se non � stato ancora caricato lo inserisco
		bFound = FALSE;
		for (int nIdx = 0; nIdx <= pProfilesList.GetUpperBound(); nIdx++)
		{
			CString strExistName = ::GetName(pProfilesList.GetAt(nIdx));
			if (_tcsicmp(strFileName, strExistName) == 0 && !bAppend)
			{
				bFound = TRUE;
				break;
			}
		}
		if (!bFound)
			pProfilesList.Add(strProfilesPath + SLASH_CHAR + strFileName);
	}
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetObjectDefaultExtension(const CTBNamespace& aNamespace) const
{
	return aNamespace.GetType() == CTBNamespace::REPORT ? szReportExtension : _T("");
}

//----------------------------------------------------------------------------------------------
void CPathFinder::GetObjectSearchExtensions(const CTBNamespace& aNamespace, CStringArray* pExtensions, BOOL bOnlyExt /*= FALSE*/) const
{
	if (!pExtensions)
	{
		TRACE0("pExtensions parameter is empty.");
		ASSERT(FALSE);
		return;
	}

	CString strFile(bOnlyExt ? "" : "*");
	CString sDefault = GetObjectDefaultExtension(aNamespace);

	if (!sDefault.IsEmpty())
		pExtensions->Add(strFile + sDefault);

	switch (aNamespace.GetType())
	{
	case CTBNamespace::REPORT:
		pExtensions->Add(strFile + _T(".rde"));
		pExtensions->Add(strFile + _T(".wrmt"));

	case CTBNamespace::IMAGE:
		pExtensions->Add(strFile + _T(".bmp"));
		pExtensions->Add(strFile + _T(".jpg"));
		pExtensions->Add(strFile + _T(".jpeg"));
		pExtensions->Add(strFile + _T(".png"));
		pExtensions->Add(strFile + _T(".wmf"));
		pExtensions->Add(strFile + _T(".emf"));
		pExtensions->Add(strFile + _T(".eps"));
		pExtensions->Add(strFile + _T(".psd"));
		pExtensions->Add(strFile + _T(".pcd"));
		pExtensions->Add(strFile + _T(".ico"));
		pExtensions->Add(strFile + _T(".cur"));
		pExtensions->Add(strFile + _T(".wpg"));
		pExtensions->Add(strFile + _T(".cmp"));
		pExtensions->Add(strFile + _T(".img"));
		pExtensions->Add(strFile + _T(".msp"));
		pExtensions->Add(strFile + _T(".pct"));
		//if (CPicture::UseGdiPlus())
		{
			pExtensions->Add(strFile + _T(".gif"));
			pExtensions->Add(strFile + _T(".tif"));
			pExtensions->Add(strFile + _T(".tiff"));
			pExtensions->Add(strFile + _T(".exif"));
		}
		break;
	case CTBNamespace::TEXT:
		pExtensions->Add(strFile + _T(".txt"));
		break;
	case CTBNamespace::PDF:
		pExtensions->Add(strFile + _T(".pdf"));
		break;
	case CTBNamespace::RTF:
		pExtensions->Add(strFile + _T(".rtf"));
		break;
	case CTBNamespace::ODT:
		pExtensions->Add(strFile + _T(".odt"));
		pExtensions->Add(strFile + _T(".ott"));
		break;
	case CTBNamespace::ODS:
		pExtensions->Add(strFile + _T(".ods"));
		pExtensions->Add(strFile + _T(".ots"));
		//pExtensions->Add (strFile + _T(".sxw"));
		//pExtensions->Add (strFile + _T(".stw"));
		break;
	case CTBNamespace::FILE:
		pExtensions->Add(strFile + _T(".*"));
		break;
	}
}

//----------------------------------------------------------------------------------------------
void CPathFinder::GetAvailableThemeFonts(CStringArray& fonts)
{
	CString sSearchExt("*");
	sSearchExt += szFontExt;

	fonts.RemoveAll();
	CString strThemesPath = GetMasterApplicationPath() + SLASH_CHAR + szThemes + SLASH_CHAR + szFonts;
	AfxGetFileSystemManager()->GetFiles(strThemesPath, sSearchExt, &fonts);

	strThemesPath = GetApplicationPath(szTaskBuilderApp, CPathFinder::STANDARD) + SLASH_CHAR + szThemes + SLASH_CHAR + szFonts;
	AfxGetFileSystemManager()->GetFiles(strThemesPath, sSearchExt, &fonts);
}

//----------------------------------------------------------------------------------------------
void CPathFinder::GetAvailableThemesFullNames(CStringArray& arThemes)
{
	CString sSearchExt("*");
	sSearchExt += szThemeExt;
	
	arThemes.RemoveAll();
	CString strThemesPath = GetMasterApplicationPath() + SLASH_CHAR + szThemes;
	AfxGetFileSystemManager()->GetFiles(strThemesPath, sSearchExt, &arThemes);

	strThemesPath = GetApplicationPath(szTaskBuilderApp, CPathFinder::STANDARD) + SLASH_CHAR + szThemes;
	AfxGetFileSystemManager()->GetFiles(strThemesPath, sSearchExt, &arThemes);
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetThemeElementFullName(CString fileName) const
{
	// prima prova la directory della master application	
	CString strThemeFullFileName = GetApplicationThemeFullName(fileName);
	// se non esiste cerca su tb
	if (!ExistFile(strThemeFullFileName))
		strThemeFullFileName = GetApplicationThemeFullName(fileName, TRUE);

	return strThemeFullFileName;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetThemeCssFullNameFromThemeName(CString strThemeName)
{
	if (strThemeName.IsEmpty())
		return NULL;

	CString cssName = PathFindFileName(strThemeName);
	if (cssName.Find(szThemeExt >= 0))
		cssName.Replace(szThemeExt, szCssExt);

	return GetThemeCssFullName(cssName);
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetThemeCssFullName(CString strThemeName) const
{
	// prima prova la directory della master application	
	CString strThemeFullFileName = GetApplicationThemeCssFullName(strThemeName);
	// se non esiste cerca su tb
	if (!ExistFile(strThemeFullFileName))
		strThemeFullFileName = GetApplicationThemeCssFullName(strThemeName, TRUE);

	return strThemeFullFileName;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetApplicationThemeFullName(CString strThemeName, BOOL bTB) const
{
	CString strMasterAppPath = bTB ? GetApplicationPath(szTaskBuilderApp, CPathFinder::STANDARD) : 
									 GetMasterApplicationPath();
	CString strFullName = strMasterAppPath + SLASH_CHAR + szThemes + SLASH_CHAR + strThemeName;

	if (GetExtension(strThemeName).IsEmpty())
		strFullName += szThemeExt;

	return strFullName;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetApplicationThemeCssFullName(CString strThemeName, BOOL bTB) const
{
	CString strMasterAppPath = bTB ? GetApplicationPath(szTaskBuilderApp, CPathFinder::STANDARD) :
		GetMasterApplicationPath();
	CString strFullName = strMasterAppPath + SLASH_CHAR + szThemes + SLASH_CHAR + strThemeName;

	if (GetExtension(strThemeName).IsEmpty())
		strFullName += szCssExt;

	return strFullName;
}


//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetMasterBrandFile(const CString& sAppName) const
{
	CString strAppFolder = GetApplicationPath(sAppName, CPathFinder::STANDARD);
	return strAppFolder + SLASH_CHAR + szSolutions + SLASH_CHAR + m_sMasterSolution + szBrandExtension;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetMasterApplicationPath() const
{
	CStringArray arFolders;
	CString applicationsFolder = GetContainerPath(CPathFinder::TB_APPLICATION);
	
	//faccio il ciclo tra le applicazione caricate (� inutile che vada su file system)
	POSITION	pos;
	CString		strValue;
	CString		strApplication;
	CString		strAppFolder;
	CString		strMasterBrandFile;	
	for (pos = m_ApplicationContainerMap.GetStartPosition(); pos != NULL;)
	{
		m_ApplicationContainerMap.GetNextAssoc(pos, strApplication, strValue);
		strAppFolder = GetApplicationPath(strApplication, CPathFinder::STANDARD);
		strMasterBrandFile = strAppFolder + SLASH_CHAR + szSolutions + SLASH_CHAR + m_sMasterSolution + szBrandExtension;
		if (ExistFile(strMasterBrandFile))
			return strAppFolder;
	}	
	return _T("");	
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetApplicationConfigFullName(const CString& sAppName) const
{
	if (sAppName.IsEmpty())
		return NULL;
	CString sPath = GetApplicationPath(sAppName, CPathFinder::STANDARD);
	if (!ExistPath(sPath))
		sPath = GetApplicationPath(sAppName, CPathFinder::CUSTOM, FALSE, CPathFinder::EASYSTUDIO);
	return GetApplicationConfigFullNameFromPath(sPath);
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetApplicationConfigFullNameFromPath(const CString& sAppPath) const
{
	if (sAppPath.IsEmpty())
		return NULL;

	return sAppPath + SLASH_CHAR + GetAppConfigName();	
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetLocalizApplicationConfigFullName(const CString& sAppName, const CString& sModName) const
{
	return GetModulePath(sAppName, sModName, CPathFinder::STANDARD) + SLASH_CHAR + GetLocalAppConfigName();
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetStartupLogFullName(const CString& sUser, BOOL bCreateDir /*FALSE*/) const
{
	SYSTEMTIME now;
	::GetLocalTime(&now);

	TCHAR szFile[MAX_FNAME_LEN];
	wsprintf(szFile, szStartupLog, now.wYear, now.wMonth, now.wDay);

	CString sName(szFile);

	//Se ho il nome dell'utente vado in custom altrimenti nella temp
	return (sUser.IsEmpty()) ? GetAppDataPath(bCreateDir) + SLASH_CHAR + sName + szTBJsonFileExt : ToPosDirectory(GetLogDataIOPath(bCreateDir), (sUser.IsEmpty() ? CPathFinder::ALL_USERS : CPathFinder::USERS), sUser, bCreateDir) + SLASH_CHAR + sName;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetUserLogPath(const CString& sUser, const CString& sCompany, BOOL bCreateDir /*FALSE*/, BOOL bOnClient /*FALSE*/) const
{
	CString sPath;
	if (bOnClient)
	{
		sPath = GetTBDllPath() + SLASH_CHAR + (sCompany.IsEmpty() ? szAllCompanies : sCompany);

		if (bCreateDir)
			CreateDirectory(sPath);
	}
	else
		sPath = GetLogDataIOPath(bCreateDir);

	return ToPosDirectory(sPath, CPathFinder::USERS, sUser, bCreateDir);
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetExitLogFullName(const CString& sUser, BOOL bCreateDir /*FALSE*/, BOOL bOnClient /*FALSE*/) const
{
	CString sCompanyName = GetCompanyName();

	CString sPath = bOnClient
		? GetTBDllPath()
		+ SLASH_CHAR
		+ (sCompanyName.IsEmpty()
		? szAllCompanies
		: sCompanyName)
		+ SLASH_CHAR
		: GetLogDataIOPath(bCreateDir);

	if (bCreateDir)
		CreateDirectory(sPath);

	SYSTEMTIME now;
	::GetLocalTime(&now);

	TCHAR szFile[MAX_FNAME_LEN];
	wsprintf(szFile, szExitLog, now.wYear, now.wMonth, now.wDay);

	CString sName(szFile);

	// exit without login
	CString sFullName;
	sFullName = ToPosDirectory(sPath, (sUser.IsEmpty() ? CPathFinder::ALL_USERS : CPathFinder::USERS), sUser, bCreateDir) + SLASH_CHAR;
	sFullName += sName;

	return sFullName;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetNumberToLiteralXmlFullName(const CTBNamespace& aNamespace, const CString& sCulture) const
{
	return GetModuleXmlPathCulture(aNamespace, CPathFinder::STANDARD, sCulture) + SLASH_CHAR + szNumberToLiteral;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetLoginServiceName() const
{
	if (m_sWebServiceInstallation.IsEmpty())
	{
		ASSERT(FALSE);
		return _T("");
	}

	return  m_sWebServiceInstallation + _T("/LoginManager/LoginManager.asmx");
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetTbServicesName() const
{
	if (m_sWebServiceInstallation.IsEmpty())
		return _T("");

	return  m_sWebServiceInstallation + _T("/TbServices/TbServices.asmx");
}

//---------------------------------------------------------------------------------------------
const CString CPathFinder::GetLockServiceName() const
{
	if (m_sWebServiceInstallation.IsEmpty())
		return _T("");

	return  m_sWebServiceInstallation + _T("/LockManager/LockManager.asmx");
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetActionSubscriptionsFolderPath(Company aCompany) const
{
	return GetCompanyPath() + SLASH_CHAR + szActionSubscriptions;
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetSynchroProvidersPath(const CTBNamespace& aNamespace) const
{
	return GetModulePath(aNamespace, CPathFinder::STANDARD) + SLASH_CHAR + szSynchroProviders;
}

// Si occupa di calcolare la directory in cui va posizionato il path sulla base di Pos.
//----------------------------------------------------------------------------------------------
CString	CPathFinder::ToPosDirectory(const CString& sPath, PosType pos, const CString& sName, BOOL bCreateDir) const
{
	CString sTmp = sPath;
	CString sUserRole = ToUserDirectory(sName);
	switch (pos)
	{
	case STANDARD:
	case CUSTOM:
		return sPath;

	case ALL_USERS:
		sTmp = sTmp + SLASH_CHAR + szAllUserDirName;
		break;

	case USERS:
		sTmp = sTmp + SLASH_CHAR + szUsers;
		if (bCreateDir)	CreateDirectory(sTmp);
		sTmp += SLASH_CHAR + sUserRole;
		break;

	case ROLES:
		sTmp = sTmp + SLASH_CHAR + szRoles;
		if (bCreateDir)	CreateDirectory(sTmp);
		sTmp += SLASH_CHAR + sUserRole;
	}

	if (bCreateDir)	CreateDirectory(sTmp);

	return sTmp;
}

// si occupa di rimuovere dallo username i caratteri non consentiti nel nome di directory/file
//			/?*\\<>:!\"			sono in caratteri non consentiti nel nome di un folder
//			/?*\\<>:!\"[],;@+=" sono in caratteri non consentiti nel nome di un utente windows
// Sfrutto uno dei caratteri consentiti da file system per eliminare il separatore \ che
// identifica il dominio in caso di sicurezza integrata. Sostituito con . su richiesta di Enrico
//----------------------------------------------------------------------------------------------
CString	CPathFinder::ToUserName(const CString& sUserDir) const
{
	CString sTmp = sUserDir;

	sTmp.Replace(szBackSlashSubstitute, SLASH_CHAR);

	return sTmp;
}

//----------------------------------------------------------------------------------------------
CString	CPathFinder::ToUserDirectory(const CString& sUserName) const
{
	CString sUserDir = sUserName;

	sUserDir.Replace(SLASH_CHAR, szBackSlashSubstitute);

	return sUserDir;
}

//----------------------------------------------------------------------------------------------
CPathFinder::ApplicationType CPathFinder::StringToApplicationType(const CString& sType)
{
	if (_tcsicmp(sType, szAppTypeApplications) == 0)
		return CPathFinder::TB_APPLICATION;

	if (_tcsicmp(sType, szAppTypeTb) == 0)
		return CPathFinder::TB;

	if (_tcsicmp(sType, szCustomization) == 0)
		return CPathFinder::CUSTOMIZATION;

	return CPathFinder::UNDEFINED;
}

//----------------------------------------------------------------------------------------------
BOOL CPathFinder::IsASystemApplication(const CString& sAppName) const
{
	ApplicationType aType = GetContainerApplicationType(GetAppContainerName(sAppName));
	return aType == CPathFinder::TB;
}

//----------------------------------------------------------------------------------------------
CPathFinder::ApplicationType CPathFinder::GetContainerApplicationType(const CString& sContainerName) const
{
	if (_tcsicmp(sContainerName, szContainerApplications) == 0)
		return CPathFinder::TB_APPLICATION;

	if (_tcsicmp(sContainerName, szContainerTaskBuilder) == 0)
		return CPathFinder::TB;

	return CPathFinder::UNDEFINED;
}


//----------------------------------------------------------------------------------------------
const void CPathFinder::GetDictionaryPathsFromString(LPCTSTR lpcszString, CStringArray &paths)
{
	if (m_pDictionaryPathFinder)
		m_pDictionaryPathFinder->GetDictionaryPathsFromString(lpcszString, paths);
}

//----------------------------------------------------------------------------------------------
const void CPathFinder::GetDictionaryPathsFromID(UINT nIDD, LPCTSTR lpszType, CStringArray &paths)
{
	if (m_pDictionaryPathFinder)
		m_pDictionaryPathFinder->GetDictionaryPathsFromID(nIDD, lpszType, paths);
}

//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDictionaryPathFromNamespace(const CTBNamespace& aNamespace, BOOL bStandard)
{
	return m_pDictionaryPathFinder
		? m_pDictionaryPathFinder->GetDictionaryPathFromNamespace(aNamespace, bStandard)
		: _T("");
}
//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDictionaryPathFromTableName(const CString& strTableName)
{
	return m_pDictionaryPathFinder
		? m_pDictionaryPathFinder->GetDictionaryPathFromTableName(strTableName)
		: _T("");
}
//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetDictionaryPathFromFileName(const CString& strFileName)
{
	CTBNamespace ns = GetNamespaceFromPath(strFileName);
	return GetDictionaryPathFromNamespace(ns, FALSE);
}
//----------------------------------------------------------------------------------------------
void CPathFinder::GetDictionaryPathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &paths)
{
	if (m_pDictionaryPathFinder)
		m_pDictionaryPathFinder->GetDictionaryPathsFormDllInstance(hDllInstance, paths);
}
//----------------------------------------------------------------------------------------------
void CPathFinder::GetJsonFormsPathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &arPaths)
{
	if (m_pDictionaryPathFinder)
		m_pDictionaryPathFinder->GetModulePathsFormDllInstance(hDllInstance, arPaths);
	for (int i = 0; i < arPaths.GetCount(); i++)
		arPaths[i] = arPaths[i] + SLASH_CHAR + szJsonForms;
}
//----------------------------------------------------------------------------------------------
const CString CPathFinder::GetJsonFormPath(const CTBNamespace& ns, PosType pos, BOOL bCreateDir /*FALSE*/, CString strSubPath /*_T("")*/)
{
	if (ns.GetType() == CTBNamespace::DOCUMENT)
	{
		CString strPath = GetDocumentPath(ns, pos, bCreateDir, pos == CPathFinder::CUSTOM ? CPathFinder::EASYSTUDIO : CPathFinder::CURRENT) + SLASH_CHAR + szJsonForms;
		if (bCreateDir && !ExistPath(strPath))
			CreateDirectory(strPath);

		if (!strSubPath.IsEmpty())
		{
			strPath += SLASH_CHAR + strSubPath;
			if (bCreateDir && !ExistPath(strPath))
				CreateDirectory(strPath);
		}

		return strPath;
	}

	if (ns.GetType() == CTBNamespace::MODULE)
		return GetModulePath(ns, CPathFinder::STANDARD) + SLASH_CHAR + szJsonForms;
	return _T("");
}

//------------------------------------------------------------------------------
const CString CPathFinder::FromNs2Path(const CString& sName, CTBNamespace::NSObjectType t1, CTBNamespace::NSObjectType t2)
{
	if (sName.IsEmpty() || IsFileName(sName))
		return sName;

	CTBNamespace aFileNs;
	CTBNamespace::NSObjectType aType = CTBNamespace(sName).GetType();

	//if (aType == CTBNamespace::NSObjectType::FILE && t1 == CTBNamespace::NSObjectType::IMAGE && t2 == CTBNamespace::NSObjectType::FILE)
	//{
	//	if (::Find)
	//}

	if (aType == t1 || aType == t2)
		aFileNs.SetNamespace(sName);
	else
		aFileNs.AutoCompleteNamespace(t1, sName, aFileNs);

	if (aFileNs.IsValid())
	{
		CString strFileName = GetFileNameFromNamespace(aFileNs, AfxGetLoginInfos()->m_strUserName);
		if (IsFileName(strFileName))
			return strFileName;
	}

	return sName;
}

//-----------------------------------------------------------------------------
CString CPathFinder::GetMenuThumbnailsFolderPath()
{
	CString sPath = GetAppDataPath(TRUE) + SLASH_CHAR + szThumbnails;
	CreateDirectory(sPath);
	return sPath;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetJsonFormsFullFileName(const CTBNamespace& aNamespace, CString sId, PosType pos, BOOL bCreateDir, Company aCompany, const CString& sUserRole) const
{
	CString sPath = AfxGetPathFinder()->GetJsonFormsPath(aNamespace, pos, bCreateDir, aCompany, sUserRole);
	return sPath + SLASH_CHAR + sId + szTBJsonFileExt;
}

//-----------------------------------------------------------------------------
const CString CPathFinder::GetTemplatesPath(const CTBNamespace& aNamespace, PosType pos, BOOL bCreateDir /*FALSE*/, Company aCompany /*CURRENT*/) const
{
	CString sPath = GetModulePath(aNamespace, pos, bCreateDir, aCompany);
	sPath = sPath + SLASH_CHAR + CString(szTemplate);
	
	if (!ExistPath(sPath))
		RecursiveCreateFolders(sPath);

	return sPath;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CPathFinder::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CAbstractFormDoc\n");
}

void CPathFinder::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG


