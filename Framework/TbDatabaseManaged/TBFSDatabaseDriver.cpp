#include "stdafx.h"

#include <TbNameSolver\Chars.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ParametersSections.h>
#include "MSqlConnection.h"
#include "TBFSDatabaseDriver.h"

using namespace System;
using namespace System::Data;
using namespace System::Data::SqlClient;
using namespace System::Text;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

static const TCHAR szMPInstanceTBFS[] = _T("MP_InstanceTBFS");
static const TCHAR szTBCustomTBFS[] = _T("TB_CustomTBFS");

static const TCHAR szInstanceKey[] = _T("inst1"); //da mettere come variabile del TBFSDatabaseDriver

static const TCHAR szQueryDB[] = _T("QueryDB");
static const TCHAR szFetch[] = _T("Fetch");



//---------------------------------------------------------------------
CString GetType(const CString& fileFullName)
{
	CTBNamespace aTBNamespace = AfxGetPathFinder()->GetNamespaceFromPath(fileFullName);
	if (aTBNamespace.IsValid())
	{
		CString strType = aTBNamespace.GetTypeString();
		return strType.MakeUpper();
	}

	CString lowerFileName = fileFullName;
	lowerFileName.MakeLower();
	if (lowerFileName.Find(_T("application.config")) > 0)
		return _T("APPLICATION");

	if (lowerFileName.Find(_T("module.config")) > 0)
		return _T("MODULE");

	if (lowerFileName.Find(_T(".menu")) > 0)
		return _T("MENU");

	if (lowerFileName.Find(_T("description")) > 0)
		return _T("DESCRIPTION");

	if (lowerFileName.Find(_T("exportprofiles")) > 0)
		return _T("EXPORTPROFILES");

	if (lowerFileName.Find(_T("datamanager")) > 0)
		return _T("DATA");

	if (lowerFileName.Find(_T("brand")) > 0)
		return _T("BRAND");

	if (lowerFileName.Find(_T("themes")) > 0)
		return _T("THEMES");

	if (lowerFileName.Find(_T(".sql")) > 0)
		return _T("SQL");

	if (lowerFileName.Find(_T(".hjson"))  > 0 || lowerFileName.Find(_T(".tbjson")) > 0)
		return _T("JSONFORM");

	if (lowerFileName.Find(_T("settings.config"))  > 0 || lowerFileName.Find(_T("\\settings\\"))  > 0)
		return _T("SETTING");

	if (lowerFileName.Find(_T(".gif"))  > 0 || lowerFileName.Find(_T(".jpg"))  > 0 || lowerFileName.Find(_T(".png")) > 0)
		return _T("IMAGE");

	if (lowerFileName.Find(_T("\\moduleobjects\\")) > 0)
	{
		CString fileName = lowerFileName.Right(lowerFileName.ReverseFind('\\') + 1);
		fileName = fileName.Left(fileName.ReverseFind('.'));
		return fileName.MakeUpper();
	}
	return _T("");
}


//è possibile che le richieste vengano eseguite anche con della path che contengono URL_SLASH_CHAR
// mentre sul DB le path sono sempre salvate con SLASH_CHAR
CString GetTBFSFileCompleteName(const CString& strPathFileName)
{
	CString strFileName = strPathFileName;
	strFileName.Replace(URL_SLASH_CHAR, SLASH_CHAR);
	return strFileName;
}


//----------------------------------------------------------------------------
BOOL IsARootPath(const CString& strTBFSFolder)
{
	return (strTBFSFolder.CompareNoCase(AfxGetPathFinder()->GetStandardPath()) == 0 || 
			strTBFSFolder.CompareNoCase(AfxGetPathFinder()->GetCustomPath()) == 0 ||
			strTBFSFolder.CompareNoCase(AfxGetPathFinder()->GetCompaniesPath()) == 0);
}


///////////////////////////////////////////////////////////////////////////////
//								MetadataPerformanceManager
///////////////////////////////////////////////////////////////////////////////
//
MetadataPerformanceManager::MetadataPerformanceManager()
{
	AddCounter(szQueryDB);
	AddCounter(szFetch);

	InitCounters();
}

//----------------------------------------------------------------------------
void MetadataPerformanceManager::InitCounters()
{
	PerformanceManager::InitCounters();
	m_bStartTime = TRUE;
}

//----------------------------------------------------------------------------
CString MetadataPerformanceManager::GetFormattedQueryTime()
{
	return GetCounters().GetFormattedTimeAt(QUERY_METADATA);
}

//----------------------------------------------------------------------------
CString MetadataPerformanceManager::GetFormattedFetchTime()
{
	return GetCounters().GetFormattedTimeAt(FETCH_METADATA);
}

//toglie la parte assoluto della path c:\InstallationName\Standard o c:\InstallationName\Custom\Companies\Companyname
//----------------------------------------------------------------------------
CString GetRelativePath(const CString& strPathFileName, bool bCustom)
{
	CString strRelativePath = strPathFileName;
	strRelativePath.MakeUpper();

	CString strStartPath = (bCustom) ? AfxGetPathFinder()->GetCompanyPath() : AfxGetPathFinder()->GetStandardPath();
	strStartPath.MakeUpper();

	int nPos = strRelativePath.Find(strStartPath);
	int nEscape = (strStartPath.GetLength() + 1);
	//alla posizione devo aggiungere 1 per il backslash
	return (nPos >= 0) ? strRelativePath.Right(strPathFileName.GetLength() - nEscape) : strPathFileName;
}


//aggiunge la parte di c:\InstallationName\Standard o c:\InstallationName\Custom\Custom\Companies\Companyname per trasformare la path in assoluta
//--------------------------------------------------------------------------
CString GetAbsolutePath(const CString& strPathFileName, bool bCustom)
{
	CString initialPath = (bCustom) ? AfxGetPathFinder()->GetCompanyPath() : AfxGetPathFinder()->GetStandardPath();
	return initialPath + SLASH_CHAR + strPathFileName;
}

//se non esiste il parent lo inserisco, vado in ricorsione
//----------------------------------------------------------------------------------------------------------------
int InsertMetadataFolder(SqlConnection^ sqlConnection, String^ strFolder, String^ application, String^ module, bool bCustom, String^ accountName, bool toCreate)//, MetadataPerformanceManager* pMetadataPerformance)
{
	if (strFolder == String::Empty)
		return -1;

	SqlCommand^ sqlCommand = nullptr;
	SqlTransaction^ sqlTrans = nullptr;
	String^ tableName = (bCustom) ? gcnew String(szTBCustomTBFS) : gcnew String(szMPInstanceTBFS);
	int parentID = -1;
	//verifico se la path esiste
	String^ commandText = String::Format("SELECT FileID from {0} WHERE PathName = '{1}' AND IsDirectory = '1'", tableName, strFolder);
	if (!bCustom)
		commandText += String::Format(" AND InstanceKey = \'{0}\'", gcnew String(szInstanceKey));

	try
	{
		//if (pMetadataPerformance) pMetadataPerformance->MakeTimeOperation(QUERY_METADATA, START_TIME);
		sqlCommand = gcnew SqlCommand(commandText, sqlConnection);
		System::Object^ value = sqlCommand->ExecuteScalar();
		//if (pMetadataPerformance) pMetadataPerformance->MakeTimeOperation(QUERY_METADATA, STOP_TIME);

		//if (pMetadataPerformance) pMetadataPerformance->MakeTimeOperation(FETCH_METADATA, START_TIME);
		if (value != nullptr)
			parentID = (Int32)value;
		//if (pMetadataPerformance) pMetadataPerformance->MakeTimeOperation(FETCH_METADATA, STOP_TIME);

		//se la path non esiste vado in ricorsione sul parent
		int lastBackSlash = strFolder->LastIndexOf(SLASH_CHAR);
		String^ parent = (lastBackSlash > 0) ? strFolder->Substring(0, lastBackSlash) : strFolder;
		delete sqlCommand;
		if (parentID == -1 && lastBackSlash > 0 && toCreate)
			parentID = InsertMetadataFolder(sqlConnection, parent, application, module, bCustom, accountName, toCreate);// , pMetadataPerformance);
		else
			//la directory esiste già
			if (parentID > -1)
				return parentID;

		String^ strInsertCommandText = (bCustom)
			? String::Format("INSERT INTO {0} (ParentID, PathName, Application, Module, CompleteFileName, ObjectType, IsDirectory, TBCreatedID, TBModifiedID)  VALUES ( {1}, '{2}', '{3}', '{4}', '{2}\\', 'DIRECTORY', '1', '{5}', '{5}')", tableName, (parentID == -1) ? "NULL" : parentID.ToString(), strFolder, application, module, AfxGetWorkerId())
			: String::Format("INSERT INTO {0} (ParentID, PathName, Application, Module, CompleteFileName, ObjectType, IsDirectory, InstanceKey)  VALUES ( {1}, '{2}', '{3}', '{4}', '{2}\\', 'DIRECTORY', '1', {5})", tableName, (parentID == -1) ? "NULL" : parentID.ToString(), parent, application, module, gcnew String(szInstanceKey));
	
		//if (pMetadataPerformance) pMetadataPerformance->MakeTimeOperation(QUERY_METADATA, START_TIME);
		sqlTrans = sqlConnection->BeginTransaction();
		sqlCommand = gcnew SqlCommand(strInsertCommandText, sqlConnection, sqlTrans);
		
		sqlCommand->ExecuteNonQuery();
		sqlCommand->CommandText = "SELECT SCOPE_IDENTITY()";
		value = sqlCommand->ExecuteScalar();
		//if (pMetadataPerformance) pMetadataPerformance->MakeTimeOperation(QUERY_METADATA, STOP_TIME);
		
		//if (pMetadataPerformance) pMetadataPerformance->MakeTimeOperation(FETCH_METADATA, START_TIME);
		if (value != nullptr)
			parentID = Convert::ToInt32(value);
		sqlTrans->Commit();
		//if (pMetadataPerformance) pMetadataPerformance->MakeTimeOperation(FETCH_METADATA, STOP_TIME);


		delete sqlCommand;
		delete sqlTrans;
	}
	catch (SqlException^ e)
	{
		if (sqlCommand)
			delete sqlCommand;
		if (sqlTrans)
			delete sqlTrans;
		throw(e);
	}
	return parentID;
}

///////////////////////////////////////////////////////////////////////////////
//								TBFSDatabaseDriver
///////////////////////////////////////////////////////////////////////////////
//
TBFSDatabaseDriver::TBFSDatabaseDriver(const CString& strStandardConnectionString, const CString& sTestCustomConnectionString)
	:
	IFileSystemDriver(),
	m_StandardConnectionString(strStandardConnectionString),
	m_TestCustomConnectionString(sTestCustomConnectionString),
	m_pCachedTBFile(NULL),
	m_pMetadataPerformance(NULL)
{
	//m_pMetadataPerformance = new MetadataPerformanceManager();
}

//----------------------------------------------------------------------------
TBFSDatabaseDriver::~TBFSDatabaseDriver()
{
	if (m_pCachedTBFile)
		delete m_pCachedTBFile;

	if (m_pMetadataPerformance)
		delete m_pMetadataPerformance;
}

//----------------------------------------------------------------------------
CString	TBFSDatabaseDriver::GetServerConnectionConfig()
{
	return GetTextFile(AfxGetPathFinder()->GetServerConnectionConfigFullName());
}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::IsAManagedObject(const CString& sFileName) const 
{ 
	if (AfxGetPathFinder()->IsStandardPath(sFileName))
		return TRUE;

	if (AfxGetPathFinder()->IsEasyStudioPath(sFileName) && GetCustomConnectionString().IsEmpty())
		return FALSE;

	return	AfxGetPathFinder()->IsCustomPath(sFileName); 
}

//----------------------------------------------------------------------------
void TBFSDatabaseDriver::GetAllApplicationInfo(CStringArray* pArray)
{
	if (!pArray)
		pArray = new CStringArray();

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;
	SqlDataReader^ dr = nullptr;

	String^ commandText;
	try
	{
		commandText = String::Format("Select PathName FROM {0} WHERE ObjectType = \'APPLICATION\' AND InstanceKey = \'{1}\' ORDER BY FileID", gcnew String (szMPInstanceTBFS), gcnew String(szInstanceKey));
				
		sqlConnection = gcnew SqlConnection(gcnew String(m_StandardConnectionString));
		sqlConnection->Open();
		sqlCommand = gcnew SqlCommand(commandText, sqlConnection);
		dr = sqlCommand->ExecuteReader();
		while (dr->Read())
			pArray->Add(GetAbsolutePath((String^)dr["PathName"], false));

		if (dr)
		{
			dr->Close();
			delete dr;
		}
		delete sqlCommand;
		sqlConnection->Close();
		delete sqlConnection;
	}
	catch (SqlException^ e)
	{
		if (dr)
		{
			dr->Close();
			delete dr;
		}
		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
		AfxGetDiagnostic()->Add(e->Message);
	}

	if (AfxGetPathFinder()->GetEasyStudioCustomizationsPosType() == CPathFinder::CUSTOM)
		::GetSubFolders(AfxGetPathFinder()->GetEasyStudioCustomizationsPath(), pArray);

}

//----------------------------------------------------------------------------
void TBFSDatabaseDriver::GetAllModuleInfo(const CString& strAppName, CStringArray* pModulesPath)
{
	if (strAppName.IsEmpty())
		return;
	
	if (!pModulesPath)
		pModulesPath = new CStringArray();

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;
	SqlDataReader^ dr = nullptr;
	String^ commandText;
	try
	{
		//MakeTimeOperation(QUERY_METADATA, START_TIME);
		commandText = String::Format("Select PathName FROM {0} WHERE ObjectType = \'MODULE\' AND Application = \'{1}\' AND InstanceKey = \'{2}\' ORDER BY FileID", gcnew String(szMPInstanceTBFS), gcnew String(strAppName), gcnew String(szInstanceKey));
		sqlConnection = gcnew SqlConnection(gcnew String(m_StandardConnectionString));
		sqlConnection->Open();
		sqlCommand = gcnew SqlCommand(commandText, sqlConnection);
		dr = sqlCommand->ExecuteReader();
		MakeTimeOperation(QUERY_METADATA, STOP_TIME);
		while (dr->Read())
		{
			MakeTimeOperation(FETCH_METADATA, START_TIME);
			pModulesPath->Add(GetAbsolutePath((String^)dr["PathName"], false));
			MakeTimeOperation(FETCH_METADATA, STOP_TIME);
		}
		if (dr)
		{
			dr->Close();
			delete dr;
		}
		delete sqlCommand;
		sqlConnection->Close();
		delete sqlConnection;
	}
	catch (SqlException^ e)
	{
		if (dr)
		{
			dr->Close();
			delete dr;
		}
		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
		AfxGetDiagnostic()->Add(e->Message);
	}
}


//----------------------------------------------------------------------------
CString TBFSDatabaseDriver::GetCustomConnectionString() const
{
	return	(
				AfxGetLoginInfos() ? 
				AfxGetLoginInfos()->m_strNonProviderCompanyConnectionString :
				m_TestCustomConnectionString
			);
}


//----------------------------------------------------------------------------
int TBFSDatabaseDriver::GetFolder(const CString& strPathName, const BOOL& bCreate)
{
	if (strPathName.IsEmpty())
		return -1;

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;
	SqlDataReader^ dr = nullptr;

	CString strRelativePath;
	CString strCommandText;
	CString strApplication, strModule, accountName;
	String^ connectionString;
	bool isCustom = false;
	int fileID = -1;
	try
	{
		//effettuo un select nella tabella MSD_StandardMetadati oppure TB_CustomMetadati 
		if (AfxGetPathFinder()->IsCustomPath(strPathName))
		{
			if (GetCustomConnectionString().IsEmpty())
				return FALSE;			
			isCustom = true;
		}
		connectionString = (isCustom) ? gcnew String(GetCustomConnectionString()) : gcnew String(m_StandardConnectionString);
		strRelativePath = GetRelativePath(strPathName, isCustom);

		AfxGetPathFinder()->GetApplicationModuleNameFromPath(strPathName, strApplication, strModule);
		sqlConnection = gcnew SqlConnection(connectionString);
		sqlConnection->Open();
		fileID = InsertMetadataFolder(sqlConnection, gcnew String(strRelativePath), gcnew String(strApplication), gcnew String(strModule), isCustom, gcnew String(accountName), bCreate == TRUE);//, m_pMetadataPerformance);

		sqlConnection->Close();
		delete sqlConnection;
	}
	catch (SqlException^ e)
	{
		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
		AfxGetDiagnostic()->Add(e->Message);
		return -1;
	}

	return fileID;
}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::CreateFolder(const CString& strPathName, const BOOL& /*bRecursive*/)
{
	return GetFolder(strPathName, TRUE) != -1;
}

//----------------------------------------------------------------------------
CString	TBFSDatabaseDriver::GetTextFile(const CString& strPathFileName)
{
	if (strPathFileName.IsEmpty())
		return _T("");
	
	TBFile* pTBFile = GetTBFile(strPathFileName);
	CString strTextContent = (pTBFile) ? pTBFile->GetContentAsString() : _T("");
	ASSERT(pTBFile);
	if (pTBFile)
		delete pTBFile;
	ASSERT(!strTextContent.IsEmpty());
	return strTextContent;
}

//----------------------------------------------------------------------------
BYTE* TBFSDatabaseDriver::GetBinaryFile(const CString& strPathFileName, int& nLen)
{
	if (strPathFileName.IsEmpty())
		return NULL;

	TBFile* pTBFile = GetTBFile(strPathFileName);
	nLen = 0;
	BYTE* pBinaryContent = (pTBFile) ? pTBFile->GetContentAsBinary() : NULL;
	ASSERT(pTBFile);
	if (pTBFile)
	{
		nLen = (pBinaryContent) ? pTBFile->m_FileSize : 0;
		//lo metto a NULL così il distrutture del TBFile non va a cancellare l'area di memoria di m_pFileContent che è stata assegnata a pBinaryContent
		pTBFile->m_pFileContent = NULL;
		delete pTBFile;
	}
	ASSERT(pBinaryContent);
	return pBinaryContent;
}


//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::SaveBinaryFile(const CString& strPathFileName, BYTE* pBinaryContent, int nLen)
{
	if (strPathFileName.IsEmpty())
		return FALSE;
	CString strTBFSFileName = GetTBFSFileCompleteName(strPathFileName);
	TBFile* pTBFile = new TBFile(strTBFSFileName);
	pTBFile->m_pFileContent = pBinaryContent;
	pTBFile->m_ObjectType = GetType(strTBFSFileName);
	pTBFile->m_IsDirectory = false;
	pTBFile->m_IsReadOnly = false;
	pTBFile->m_FileSize = nLen;
	AfxGetPathFinder()->GetApplicationModuleNameFromPath(strTBFSFileName, pTBFile->m_strAppName, pTBFile->m_strModuleName);
	if (pTBFile->m_bIsCustomPath = AfxGetPathFinder()->IsCustomPath(strTBFSFileName))
		pTBFile->m_strAccountName = AfxGetPathFinder()->GetUserNameFromPath(strTBFSFileName);

	BOOL bResult = SaveTBFile(pTBFile, TRUE);
	delete pTBFile;

	return bResult;
}

//----------------------------------------------------------------------------
int TBFSDatabaseDriver::GetFile(const CString& strPathFileName)
{
	if (strPathFileName.IsEmpty())
		return -1;
	CString strTBFSFileName = GetTBFSFileCompleteName(strPathFileName);

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;

	CString strRelativePath;
	String^ commandText;
	String^ connectionString;
	String^ tableName;
	bool isCustom = false;
	try
	{
		//effettuo un select nella tabella MSD_StandardMetadati oppure TB_CustomMetadati 
		if (AfxGetPathFinder()->IsCustomPath(strTBFSFileName))
		{
			if (GetCustomConnectionString().IsEmpty())
				return FALSE;
			isCustom = true;
		}
		connectionString = (isCustom) ? gcnew String(GetCustomConnectionString()) : gcnew String(m_StandardConnectionString);
		strRelativePath = GetRelativePath(strTBFSFileName, isCustom);
		tableName = (isCustom) ? gcnew String(szTBCustomTBFS) : gcnew String(szMPInstanceTBFS);
		
		MakeTimeOperation(QUERY_METADATA, START_TIME);
		sqlConnection = gcnew SqlConnection(connectionString);
		sqlConnection->Open();
		commandText = String::Format("Select FileID from {0} where CompleteFileName = '{1}'", tableName, gcnew String(strRelativePath));
		if (!isCustom)
			commandText += String::Format(" AND InstanceKey = \'{0}\'", gcnew String(szInstanceKey));

		sqlCommand = gcnew SqlCommand(commandText, sqlConnection);
		System::Object^ value = sqlCommand->ExecuteScalar();
		MakeTimeOperation(QUERY_METADATA, STOP_TIME);

		delete sqlCommand;
		sqlConnection->Close();
		delete sqlConnection;

		int nResult;
		MakeTimeOperation(FETCH_METADATA, START_TIME);
		nResult = (value != nullptr) ? (Int32)value : -1;
		MakeTimeOperation(FETCH_METADATA, STOP_TIME);
		delete value;
		return nResult;
	}
	catch (SqlException^)
	{

		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
		ASSERT(FALSE);
		return -1;
	}

	return -1;
}
//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::SaveTextFile(const CString& strPathFileName, const CString& fileTextContent)
{
	if (strPathFileName.IsEmpty())
		return FALSE;
	CString strTBFSFileName = GetTBFSFileCompleteName(strPathFileName);

	TBFile* pTBFile = new TBFile(strTBFSFileName);
	pTBFile->m_strFileContent = fileTextContent;
	pTBFile->m_ObjectType = GetType(strTBFSFileName);
	pTBFile->m_IsDirectory = false;
	pTBFile->m_IsReadOnly = false;
	pTBFile->m_FileSize = fileTextContent.GetLength();
	AfxGetPathFinder()->GetApplicationModuleNameFromPath(strTBFSFileName, pTBFile->m_strAppName, pTBFile->m_strModuleName);
	if (pTBFile->m_bIsCustomPath = AfxGetPathFinder()->IsCustomPath(strTBFSFileName))
		pTBFile->m_strAccountName = AfxGetPathFinder()->GetUserNameFromPath(strTBFSFileName);
	
	BOOL bResult = SaveTBFile(pTBFile, TRUE);
	delete pTBFile;

	return bResult;
}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::ExistFile(const CString& strPathFileName)
{
	//return GetFile(strPathFileName) > 0;
	if (m_pCachedTBFile)
	{
		delete m_pCachedTBFile;
		m_pCachedTBFile = NULL;
	}
	m_pCachedTBFile = GetTBFile(strPathFileName);

	return m_pCachedTBFile ? m_pCachedTBFile->m_FileID > 0 : FALSE; //GetFile(strPathFileName) > 0;
}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::RemoveFile(const CString& strPathFileName)
{
	if (strPathFileName.IsEmpty())
		return FALSE;
	CString strTBFSFileName = GetTBFSFileCompleteName(strPathFileName);

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;

	CString strRelativePath;
	String^ commandText;
	String^ connectionString;
	String^ tableName;
	bool isCustom = false;
	try
	{
		if (AfxGetPathFinder()->IsCustomPath(strTBFSFileName))
		{
			if (GetCustomConnectionString().IsEmpty())
				return FALSE;
			isCustom = true;
		}
		connectionString = (isCustom) ? gcnew String(GetCustomConnectionString()) : gcnew String(m_StandardConnectionString);
		strRelativePath = GetRelativePath(strTBFSFileName, isCustom);
		tableName = (isCustom) ? gcnew String(szTBCustomTBFS) : gcnew String(szMPInstanceTBFS);

		sqlConnection = gcnew SqlConnection(connectionString);
		sqlConnection->Open();
		MakeTimeOperation(QUERY_METADATA, START_TIME);		
		commandText = String::Format("DELETE {0} WHERE CompleteFileName = '{1}'", tableName, gcnew String(strRelativePath));
		sqlCommand = gcnew SqlCommand(commandText, sqlConnection);
		int nResult = sqlCommand->ExecuteNonQuery();
		MakeTimeOperation(QUERY_METADATA, STOP_TIME);

		delete sqlCommand;
		sqlConnection->Close();
		delete sqlConnection;
		return nResult == 1;
	}
	catch (SqlException^)
	{

		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
		ASSERT(FALSE);
		return FALSE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::RenameFile(const CString& strOldFileName, const CString& strNewFileName)
{
	if (strOldFileName.IsEmpty() || strNewFileName.IsEmpty())
		return FALSE;

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;

	CString strOldRelativePath, strNewRelativePath;
	String^ commandText;
	String^ connectionString;
	String^ tableName;
	bool isCustom = false;
	strOldRelativePath = GetTBFSFileCompleteName(strOldFileName);
	strNewRelativePath = GetTBFSFileCompleteName(strNewFileName);
	try
	{


		///effettuo un select nella tabella MSD_StandardMetadati oppure TB_CustomMetadati 
		if (AfxGetPathFinder()->IsCustomPath(strOldRelativePath))
		{
			if (GetCustomConnectionString().IsEmpty())
				return FALSE;
			isCustom = true;
		}
		connectionString = (isCustom) ? gcnew String(GetCustomConnectionString()) : gcnew String(m_StandardConnectionString);
		strOldRelativePath = GetRelativePath(strOldRelativePath, isCustom);
		strNewRelativePath = GetRelativePath(strNewRelativePath, isCustom);
		tableName = (isCustom) ? gcnew String(szTBCustomTBFS) : gcnew String(szMPInstanceTBFS);
			
		MakeTimeOperation(QUERY_METADATA, START_TIME);
		sqlConnection = gcnew SqlConnection(connectionString);
		sqlConnection->Open();
		FileInfo file(gcnew String(strNewFileName));
		
		commandText = String::Format("UPDATE {0} SET  FileName = {1}, FileType = {2}, CompleteFileName = {3} WHERE CompleteFileName = '{4}'", tableName, file.Name, file.Extension, gcnew String(strNewRelativePath), gcnew String(strOldRelativePath));
		if (!isCustom)
			commandText += String::Format(" AND InstanceKey = \'{0}\'", gcnew String(szInstanceKey));

		sqlCommand = gcnew SqlCommand(commandText, sqlConnection);
		int nResult = sqlCommand->ExecuteNonQuery();
		MakeTimeOperation(QUERY_METADATA, STOP_TIME);
		delete sqlCommand;
		sqlConnection->Close();
		delete sqlConnection;
		return nResult == 1;
	}
	catch (SqlException^)
	{

		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
		ASSERT(FALSE);
		return FALSE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::GetFileStatus(const CString& strPathFileName, CFileStatus& fs)
{
	if (strPathFileName.IsEmpty())
		return FALSE;

	CString strTBFSFileName = GetTBFSFileCompleteName(strPathFileName);

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;
	SqlDataReader^ dr = nullptr;

	CString strRelativePath;
	String^ commandText;
	String^ connectionString;
	String^ tableName;
	
	bool isCustom = false;
	try
	{
		//effettuo un select nella tabella MSD_StandardMetadati oppure TB_CustomMetadati 
		if (AfxGetPathFinder()->IsCustomPath(strTBFSFileName))
		{
			if (GetCustomConnectionString().IsEmpty())
				return FALSE;
			isCustom = true;
		}
		connectionString = (isCustom) ? gcnew String(GetCustomConnectionString()) : gcnew String(m_StandardConnectionString);
		strRelativePath = GetRelativePath(strTBFSFileName, isCustom);
		tableName = (isCustom) ? gcnew String(szTBCustomTBFS) : gcnew String(szMPInstanceTBFS);

		MakeTimeOperation(QUERY_METADATA, START_TIME);
		sqlConnection = gcnew SqlConnection(connectionString);
		sqlConnection->Open();
		commandText = String::Format("Select FileSize, CreationTime, LastWriteTime, IsDirectory, IsReadOnly from {0} where CompleteFileName = '{1}'", tableName, gcnew String(strRelativePath));
		if (!isCustom)
			commandText += String::Format(" AND InstanceKey = \'{0}\'", gcnew String(szInstanceKey));

		sqlCommand = gcnew SqlCommand(commandText, sqlConnection);

		dr = sqlCommand->ExecuteReader();
		MakeTimeOperation(QUERY_METADATA, STOP_TIME);

		MakeTimeOperation(FETCH_METADATA, START_TIME);
		if (dr->Read())
		{
			DateTime aDT = (DateTime)dr["CreationTime"];
			fs.m_ctime = CTime(aDT.Year, aDT.Month, aDT.Day, aDT.Hour, aDT.Minute, aDT.Second);
			aDT = (DateTime)dr["LastWriteTime"];
			fs.m_atime = fs.m_mtime = CTime(aDT.Year, aDT.Month, aDT.Day, aDT.Hour, aDT.Minute, aDT.Second);
			fs.m_size = (Int32)dr["FileSize"];
			lstrcpyn(fs.m_szFullName, strTBFSFileName, strTBFSFileName.GetLength());

			if ((String^)dr["IsDirectory"] == "1")
				fs.m_attribute |= 0x10; // Attribute::directory;
			if ((String^)dr["IsReadOnly"] == "1")
				fs.m_attribute |= 0x01;
		}
		MakeTimeOperation(FETCH_METADATA, STOP_TIME);
		if (dr)
		{
			dr->Close();
			delete dr;
		}

		delete sqlCommand;
		sqlConnection->Close();
		delete sqlConnection;
		
		return TRUE;
	}
	catch (SqlException^)
	{

		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
		ASSERT(FALSE);
		return -1;
	}


	return TRUE;
}

//----------------------------------------------------------------------------
DWORD TBFSDatabaseDriver::GetFileAttributes(const CString& sFileName)
{
	CFileStatus fs;
	if (GetFileStatus(sFileName, fs))
		return fs.m_attribute;
	
	return 0x00;
}


//----------------------------------------------------------------------------
void GetTBFilesInfo(const CString& strConnectionString, const CString& strCommandText, TBMetadataArray* pArray, bool bFromCustom, MetadataPerformanceManager* pPerformanceManager)
{
	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;
	SqlDataReader^ dr = nullptr;
	TBFile* pMetadataFile;
	try
	{
		if (pPerformanceManager) pPerformanceManager->MakeTimeOperation(QUERY_METADATA, START_TIME);
		sqlConnection = gcnew SqlConnection(gcnew String(strConnectionString));
		sqlConnection->Open();
		sqlCommand = gcnew SqlCommand(gcnew String(strCommandText), sqlConnection);

		dr = sqlCommand->ExecuteReader();
		if (pPerformanceManager) pPerformanceManager->MakeTimeOperation(QUERY_METADATA, STOP_TIME);

		if (pPerformanceManager) pPerformanceManager->MakeTimeOperation(FETCH_METADATA, START_TIME);
		while (dr->Read())
		{
			pMetadataFile = new TBFile((String^)dr["FileName"], GetAbsolutePath((String^)dr["PathName"], bFromCustom));
			pMetadataFile->m_FileID = (Int32)dr["FileID"];
			pMetadataFile->m_ParentID = (Int32)dr["ParentID"];
			pMetadataFile->m_strNamespace = (String^)dr["Namespace"];
			pMetadataFile->m_strAppName = (String^)dr["Application"];
			pMetadataFile->m_strModuleName = (String^)dr["Module"];
			pMetadataFile->m_FileSize = (Int32)dr["FileSize"];	
			pMetadataFile->m_IsReadOnly = ((String^)dr["IsReadOnly"] == "1");
			pMetadataFile->m_IsDirectory = ((String^)dr["IsDirectory"] == "1");
			DateTime aDT = (DateTime)dr["CreationTime"];
			pMetadataFile->m_CreationTime = CTime(aDT.Year, aDT.Month, aDT.Day, aDT.Hour, aDT.Minute, aDT.Second);
			aDT = (DateTime)dr["LastWriteTime"];
			pMetadataFile->m_LastWriteTime = CTime(aDT.Year, aDT.Month, aDT.Day, aDT.Hour, aDT.Minute, aDT.Second);
			//se sono nella custom devo valorizzare anche i seguenti campi
			if (bFromCustom)
				pMetadataFile->m_strAccountName = (String^)dr["AccountName"];

			pMetadataFile->m_bIsCustomPath = bFromCustom;
			if (dr["FileContent"] == System::DBNull::Value || pMetadataFile->m_FileSize <= 0)
				pMetadataFile->m_pFileContent = NULL;
			else
			{
				array<Byte>^ byteData = ((array<Byte>^)dr["FileContent"]);
				pin_ptr<Byte> data = &byteData[0];
				pMetadataFile->m_pFileContent = new BYTE[pMetadataFile->m_FileSize];
				memcpy(pMetadataFile->m_pFileContent, data, pMetadataFile->m_FileSize);
			}

			if (dr["FileTextContent"] != System::DBNull::Value)
				pMetadataFile->m_strFileContent = (String^)dr["FileTextContent"];
			
			pArray->Add(pMetadataFile);
		}
		if (pPerformanceManager) pPerformanceManager->MakeTimeOperation(FETCH_METADATA, STOP_TIME);

		if (dr)
		{
			dr->Close();
			delete dr;
		}
		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
	}
	catch (SqlException^ e)
	{
		if (dr)
		{
			dr->Close();
			delete dr;
		}
		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}

		throw(e);
	}
}


//----------------------------------------------------------------------------
void TBFSDatabaseDriver::GetStandardTBFileInfo(const CString& whereClause, TBMetadataArray* pArray)
{
	if (!pArray || whereClause.IsEmpty())
		return;
	try
	{
		CString strCommandText = cwsprintf(_T("Select * from %s where %s"), szMPInstanceTBFS, whereClause);
		GetTBFilesInfo(m_StandardConnectionString, strCommandText, pArray, false, m_pMetadataPerformance);
	}
	catch (SqlException^ e)
	{
		throw(e);
	}
}

//----------------------------------------------------------------------------
void TBFSDatabaseDriver::GetCustomTBFileInfo(const CString& whereClause, TBMetadataArray* pArray)
{
	CString strCustConnectionString = GetCustomConnectionString();

	if (!pArray || whereClause.IsEmpty() || strCustConnectionString.IsEmpty())
		return;
	try
	{
		CString strCommandText = cwsprintf(_T("Select * from %s where %s"), szTBCustomTBFS, whereClause);
		GetTBFilesInfo(strCustConnectionString, strCommandText, pArray, true , m_pMetadataPerformance);
	}
	catch (SqlException^ e)
	{
		throw(e);
	}
}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::SaveTBFile(TBFile* pTBFile, const BOOL& bOverWrite)
{
	if (pTBFile->m_strCompleteFileName.IsEmpty())
		return FALSE;

		//esiste allora lo devo solo modificare FileContent, LastWriteTime, FileSize, TBModified e TBModifiedID

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;

	String^ connectionString;
	String^ tableName;	
	CString strRelativePath;
	CString strApplication, strModule, strAccountName;
	bool isCustom = false;
	int parentID = -1;

	//verifico se il file esiste
	int fileID = GetFile(pTBFile->m_strCompleteFileName);
	if (fileID != -1 && !bOverWrite)
		return TRUE;


	String^ strCommandText;
	try
	{
		if (AfxGetPathFinder()->IsCustomPath(pTBFile->m_strCompleteFileName))
		{
			if (GetCustomConnectionString().IsEmpty())
				return FALSE;
			isCustom = true;
		}
		connectionString = (isCustom) ? gcnew String(GetCustomConnectionString()) : gcnew String(m_StandardConnectionString);
		strRelativePath = GetRelativePath(pTBFile->m_strCompleteFileName, isCustom);
		tableName = (isCustom) ? gcnew String(szTBCustomTBFS) : gcnew String(szMPInstanceTBFS);
		

		strCommandText = (fileID > -1)
			? String::Format("UPDATE {0} SET LastWriteTime = GetDate(), FileSize = @FileSize, FileContent = @BinaryContent, FileTextContent = @FileTextContent, TBModified = GetDate(), TBModifiedID = @TBModifiedID WHERE FileID = @FileID", tableName)
			: (isCustom)
			? String::Format("INSERT INTO {0} (ParentID, PathName, FileName, CompleteFileName, FileType, FileSize, Application, Module, ObjectType,IsDirectory,IsReadOnly,FileContent,FileTextContent, AccountName, TbCreatedID, TBModifiedID )  VALUES ( @ParentID, @PathName, @FileName, @CompleteFileName, @FileType, @FileSize, @Application, @Module, @ObjectType, @IsDirectory, @IsReadOnly, @BinaryContent, @FileTextContent, @AccountName, @TbCreatedID, @TBModifiedID)", tableName)
			: String::Format("INSERT INTO {0} (ParentID, PathName, FileName, CompleteFileName, FileType, FileSize, Application, Module, ObjectType,IsDirectory,IsReadOnly,FileContent,FileTextContent, InstanceKey, TbCreatedID, TBModifiedID )  VALUES ( @ParentID, @PathName, @FileName, @CompleteFileName, @FileType, @FileSize, @Application, @Module, @ObjectType, @IsDirectory, @IsReadOnly ,@BinaryContent,  @FileTextContent, @InstanceKey @TbCreatedID, @TBModifiedID)", tableName);

		//se non ho ancora inserito il file mi devo far dare il parentID e se non esiste inserirlo
		if (fileID == -1)
		{
			if (IsARootPath(pTBFile->m_strCompleteFileName))
				parentID = -1;
			parentID = GetFolder(pTBFile->m_strCompleteFileName, TRUE);
			AfxGetPathFinder()->GetApplicationModuleNameFromPath(pTBFile->m_strCompleteFileName, strApplication, strModule);
			strAccountName = AfxGetPathFinder()->GetUserNameFromPath(pTBFile->m_strCompleteFileName);
		}
		MakeTimeOperation(QUERY_METADATA, START_TIME);
		sqlConnection = gcnew SqlConnection(connectionString);
		sqlConnection->Open();
		sqlCommand = gcnew SqlCommand(strCommandText, sqlConnection);
		//aggiungo i parametri
		//prima quelli comuni tra update ed insert
		sqlCommand->Parameters->AddWithValue("@FileSize", (Int32)pTBFile->m_FileSize);
		sqlCommand->Parameters->AddWithValue("@FileTextContent", gcnew String(pTBFile->m_strFileContent));
		SqlParameter^ binaryContentParam = gcnew SqlParameter("@BinaryContent", SqlDbType::VarBinary);
		if (pTBFile->m_pFileContent)
		{
			array<Byte>^ arr = gcnew array<Byte>(pTBFile->m_FileSize);
			Marshal::Copy((IntPtr)pTBFile->m_pFileContent, arr, 0, pTBFile->m_FileSize);
			binaryContentParam->Value = arr;
		}
		else
			binaryContentParam->Value = System::DBNull::Value;

		sqlCommand->Parameters->Add(binaryContentParam);
		sqlCommand->Parameters->AddWithValue("@TBModifiedID", (Int32)AfxGetWorkerId());

		//se sto aggiungendo una nuova riga allora devo fare il bind anche degli altri parametri
		if (fileID <= 0)
		{
			String^ relativePath = gcnew String(strRelativePath);
			SqlParameter^ parentParam = gcnew SqlParameter("@ParentID", SqlDbType::Int);
			if (parentID > -1)
				parentParam->Value = parentID;
			else
				parentParam->Value = System::DBNull::Value;
			sqlCommand->Parameters->Add(parentParam);

			sqlCommand->Parameters->AddWithValue("@PathName", relativePath);
			sqlCommand->Parameters->AddWithValue("@FileName", gcnew String(pTBFile->m_strName));
			sqlCommand->Parameters->AddWithValue("@CompleteFileName", relativePath);
			sqlCommand->Parameters->AddWithValue("@FileType", gcnew String(pTBFile->m_strFileType));
			sqlCommand->Parameters->AddWithValue("@Application", gcnew String(strApplication));
			sqlCommand->Parameters->AddWithValue("@Module", gcnew String(strModule));
			sqlCommand->Parameters->AddWithValue("@ObjectType", gcnew String(pTBFile->m_ObjectType));
			sqlCommand->Parameters->AddWithValue("@IsDirectory", gcnew String((pTBFile->m_IsDirectory) ? "1" : "0"));
			sqlCommand->Parameters->AddWithValue("@IsReadOnly", gcnew String((pTBFile->m_IsReadOnly) ? "1" : "0"));
			sqlCommand->Parameters->AddWithValue("@TbCreatedID", (Int32)AfxGetWorkerId());
			if (isCustom)
				sqlCommand->Parameters->AddWithValue("@AccountName", gcnew String(strAccountName));
			else
				sqlCommand->Parameters->AddWithValue("@InstanceKey", gcnew String(szInstanceKey));
		}
		else //parametro della where clause nel caso di update
			sqlCommand->Parameters->AddWithValue("@FileID", (Int32)fileID);

		sqlCommand->ExecuteNonQuery();
		MakeTimeOperation(QUERY_METADATA, STOP_TIME);
		delete sqlCommand;
	}

	catch (SqlException^ e)
	{
		if (sqlCommand)
			delete sqlCommand;
		if (sqlConnection)
		{
			sqlConnection->Close();
			delete 	sqlConnection;
		}
		AfxGetDiagnostic()->Add(e->Message);
		return FALSE;
	}

	return TRUE;

}

//----------------------------------------------------------------------------
TBFile* TBFSDatabaseDriver::GetTBFile(const CString& strPathFileName)
{
	if (strPathFileName.IsEmpty())
		return NULL;
	
	CString strTBFSFileName = GetTBFSFileCompleteName(strPathFileName);
	if (m_pCachedTBFile && m_pCachedTBFile->m_strCompleteFileName.CompareNoCase(strPathFileName) == 0)
	{
		TBFile* pTBFile = m_pCachedTBFile;
		m_pCachedTBFile = NULL;
		return pTBFile;
	}

	TBMetadataArray aMetadataArray;
	aMetadataArray.SetOwns(FALSE);
	CString strRelativePath;
	try
	{
		if (AfxGetPathFinder()->IsStandardPath(strTBFSFileName))
		{
			strRelativePath = GetRelativePath(strTBFSFileName, false);			
			GetStandardTBFileInfo(cwsprintf(_T(" CompleteFileName = \'%s\' AND InstanceKey = \'%s\'"), strRelativePath, szInstanceKey), &aMetadataArray);
		}
		else
		{
			strRelativePath = GetRelativePath(strTBFSFileName, true);
			GetCustomTBFileInfo(cwsprintf(_T(" CompleteFileName = \'%s\'"), strRelativePath), &aMetadataArray);
		}		
	}
	catch (SqlException^ e)
	{
		AfxGetDiagnostic()->Add(e->Message);
		return NULL;
	}	

	return (aMetadataArray.GetSize() > 0) ? aMetadataArray.GetAt(0) : NULL;
}



//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::CopyTBFile(TBFile* pTBOldFileInfo, const CString& strNewName, const BOOL& bOverWrite)
{
	if (!pTBOldFileInfo || strNewName.IsEmpty())
		return FALSE;

	if (pTBOldFileInfo->m_strCompleteFileName.CompareNoCase(strNewName) == 0)
		return TRUE;
	

	//modifico il nome e se serve path,application, module	
	CString strNewPath = ::GetPath(strNewName);
	//le path sono diverse
	if (strNewPath.CompareNoCase(pTBOldFileInfo->m_strPathName) != 0)
	{
		AfxGetPathFinder()->GetApplicationModuleNameFromPath(strNewPath, pTBOldFileInfo->m_strAppName, pTBOldFileInfo->m_strModuleName);
		if (pTBOldFileInfo->m_bIsCustomPath = AfxGetPathFinder()->IsCustomPath(strNewPath))
			pTBOldFileInfo->m_strAccountName = AfxGetPathFinder()->GetUserNameFromPath(strNewPath);
	}
	pTBOldFileInfo->m_strCompleteFileName = GetTBFSFileCompleteName(strNewName);
	pTBOldFileInfo->m_strName = ::GetName(strNewPath);
	pTBOldFileInfo->m_strFileType = ::GetExtension(strNewPath);

	return SaveTBFile(pTBOldFileInfo, bOverWrite);

}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::CopySingleFile(const CString& strOldFileName, const CString& strNewName, const BOOL& bOverWrite)
{
	if (strOldFileName.CompareNoCase(strNewName) == 0)
		return TRUE;
	if (strOldFileName.IsEmpty() || strNewName.IsEmpty())
		return FALSE;

	TBFile* pTBOldFileInfo = GetTBFile(strOldFileName);
	if (!pTBOldFileInfo)
		return FALSE;

	BOOL bOK = CopyTBFile(pTBOldFileInfo, strNewName, bOverWrite);
	delete pTBOldFileInfo;
	return bOK;
}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::ExistPath(const CString& strPathName)
{
	if (IsARootPath(strPathName))
		return TRUE;

	return GetFolder(strPathName, FALSE) != 0;
}

//----------------------------------------------------------------------------
bool RemoveParentFolders(SqlConnection^ sqlConnection, String^ tableName, Int32 fileID, MetadataPerformanceManager* pMetadataPerformance)
{
	SqlCommand^ sqlCommand = nullptr;
	try
	{
		if (pMetadataPerformance) pMetadataPerformance->MakeTimeOperation(QUERY_METADATA, START_TIME);
		// verifico se il folder è vuoto o meno
		sqlCommand = gcnew SqlCommand(String::Format("SELECT FileID FROM {0} WHERE ParentID = {2}", tableName, fileID.ToString()), sqlConnection);
		System::Object^ value = sqlCommand->ExecuteScalar();
		//ci sono dei file o altri subfolder. Non la posso cancellare
		if (value != nullptr && (Int32)value > 0)
		{
			delete sqlCommand;
			return true;
		}
		sqlCommand->CommandText = String::Format("SELECT ParentID FROM {0} WHERE FileID = {1}", tableName, fileID.ToString());
		value = sqlCommand->ExecuteScalar();		
		//cancello il folder e poi vado in ricorsione sul parent se esiste			
		//se non ho parent vuol dire che sono arrivata alla root
		sqlCommand->CommandText = String::Format("DELETE FROM {0} WHERE FileID = {1}", tableName, fileID.ToString());
		sqlCommand->ExecuteNonQuery();
		if (pMetadataPerformance) pMetadataPerformance->MakeTimeOperation(QUERY_METADATA, START_TIME);
		delete sqlCommand;
		if (value != nullptr && (Int32)value > 0)
			return RemoveParentFolders(sqlConnection, tableName, (Int32)value , pMetadataPerformance);

		return true;
	}
	catch (SqlException^ e)
	{
		if (sqlCommand)
			delete sqlCommand;

		throw(e);
	}

	return true;
}

//----------------------------------------------------------------------------
bool RemoveRecursiveFolder(SqlConnection^ sqlConnection, String^ tableName, Int32 parentID)
{
	SqlCommand^ sqlCommand = nullptr;
	SqlDataAdapter^ ad = nullptr;
	try
	{	
		ad = gcnew SqlDataAdapter(String::Format("SELECT FileID FROM {0} WHERE ParentID = {1} and IsDirectory = '1'", tableName, parentID.ToString()), sqlConnection);
		DataTable^ filesDT = gcnew DataTable();
		ad->Fill(filesDT);
		for each (DataRow^ row in filesDT->Rows)
		{
			int nFolderID = (Int32)row[0];
			for (int i = 0; i < filesDT->Rows->Count; i++)
			{
				RemoveRecursiveFolder(sqlConnection, tableName, nFolderID);
				sqlCommand = gcnew SqlCommand(String::Format("DELETE FROM {0} WHERE ParentID = {1}", tableName, parentID.ToString()), sqlConnection);
				sqlCommand->ExecuteNonQuery();
				delete sqlCommand;
			}
		}
		return true;
	}
	catch (SqlException^ e)
	{
		if (ad)
			delete ad;
		if (sqlCommand)
			delete sqlCommand;

		throw(e);
	}

	return true;
}


//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::RemoveFolder(const CString& strPathName, const BOOL& bRecursive, const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents /*= FALSE*/)
{
	if (strPathName.IsEmpty())
		return false;

	CString strTBFSFolder = GetTBFSFileCompleteName(strPathName);
	if (IsARootPath(strTBFSFolder))
		return true;

	bool bOk = true;

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;
	System::Object^ value = nullptr;

	CString strRelativePath;
	CString strCommandText;
	String^ connectionString;
	bool isCustom = false;
	int fileID = 0;
	int parentID = -1;
	try
	{
		
		//effettuo un select nella tabella MSD_StandardMetadati oppure TB_CustomMetadati 
		if (AfxGetPathFinder()->IsCustomPath(strTBFSFolder))
		{
			if (GetCustomConnectionString().IsEmpty())
				return 0;
			isCustom = true;
		}

		connectionString = (isCustom) ? gcnew String(GetCustomConnectionString()) : gcnew String(m_StandardConnectionString);
		strRelativePath = GetRelativePath(strTBFSFolder, isCustom);
		String^ tableName = (isCustom) ? gcnew String(szTBCustomTBFS) : gcnew String(szMPInstanceTBFS);


		sqlConnection = gcnew SqlConnection(connectionString);
		sqlConnection->Open();
		int fileID = GetFolder(strTBFSFolder, FALSE);
		if (bRecursive)
			bOk = RemoveRecursiveFolder(sqlConnection, tableName, fileID);

		int parentID = 0;
		if (bOk && bAndEmptyParents)
		{
			sqlCommand = gcnew SqlCommand(String::Format("SELECT ParentID FROM {0} WHERE FileID = {1}", tableName, fileID.ToString()), sqlConnection);

			MakeTimeOperation(QUERY_METADATA, START_TIME);
			value = sqlCommand->ExecuteScalar();
			MakeTimeOperation(QUERY_METADATA, STOP_TIME);

			MakeTimeOperation(FETCH_METADATA, START_TIME);
			parentID = (value != nullptr) ? (Int32)value : -1;
			MakeTimeOperation(FETCH_METADATA, STOP_TIME);
		}
		
		if (bOk && bRemoveRoot)
		{
			if (!sqlCommand)
				gcnew SqlCommand("",sqlConnection);
			MakeTimeOperation(QUERY_METADATA, START_TIME);
			sqlCommand->CommandText = String::Format("DELETE FROM {0} WHERE fileID = {1}", tableName, fileID.ToString());
			sqlCommand->ExecuteNonQuery();			
			MakeTimeOperation(QUERY_METADATA, STOP_TIME);
		}

		if (sqlCommand)
			delete sqlCommand;

		if (bOk && bRemoveRoot && bAndEmptyParents)
			bOk = RemoveParentFolders(sqlConnection, tableName, fileID, m_pMetadataPerformance);

		sqlConnection->Close();
		delete sqlConnection;
	}
	catch (SqlException^ e)
	{
		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
		AfxGetDiagnostic()->Add(e->Message);
		return FALSE;
	}

	return (BOOL)bOk;
}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::CopyFolder(const CString& sOldPathName, const CString& sNewPathName, const BOOL& bOverwrite, const BOOL& bRecursive)
{
	CString strSourcePath, strTargetPath;
	strSourcePath = GetTBFSFileCompleteName(sOldPathName);
	strTargetPath = GetTBFSFileCompleteName(sNewPathName);
	if (IsDirSeparator(sOldPathName.Right(1)))
		strSourcePath = sOldPathName.Left(sOldPathName.GetLength() - 1);
	else
		strSourcePath = sOldPathName;

	if (IsDirSeparator(sNewPathName.Right(1)))
		strTargetPath = sNewPathName.Left(sNewPathName.GetLength() - 1);
	else
		strTargetPath = sNewPathName;

	try
	{

		int sourceFolderID = GetFolder(strSourcePath, FALSE);
		int targetFolderID = GetFolder(strTargetPath, FALSE);
		//se esiste il folder sorgente 
		if (sourceFolderID == -1)
			return FALSE;

		//se esiste il folder di destinazione e non posso sovvrascriverlo
		if (!bOverwrite && targetFolderID != -1)
			return FALSE;

		if (targetFolderID == -1)
		{
			//creo il foldel di destinazione
			targetFolderID = GetFolder(strTargetPath, TRUE);
			if (targetFolderID == -1)
				return FALSE;
		}

		//vado in ricorsione sul contenuto 
		TBMetadataArray* pTBFiles = new TBMetadataArray();
		TBFile* pTBFile = NULL;
		CString strFolderName;
		GetTBFolderContent(strSourcePath, pTBFiles, TRUE, TRUE, _T(""));
		for (int i = 0; i < pTBFiles->GetCount(); i++)
		{
			pTBFile = pTBFiles->GetAt(i);

			if (pTBFile->m_IsDirectory)
			{
				int nPos = pTBFile->m_strPathName.ReverseFind(SLASH_CHAR);
				strFolderName = pTBFile->m_strPathName.Right(pTBFile->m_strPathName.GetLength() - (nPos + 1));

				if (!CopyFolder(pTBFile->m_strPathName, strTargetPath + SLASH_CHAR + strFolderName, bOverwrite, bRecursive))
					return FALSE;
			}
			else
			{
				if (!CopyTBFile(pTBFile, strTargetPath + SLASH_CHAR + pTBFile->m_strName, bOverwrite))
					return FALSE;
			}
		}
	}
	catch (SqlException^ e)
	{
		AfxGetDiagnostic()->Add(e->Message);
		return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::GetSubFolders(const CString& strPathName, CStringArray* pSubFolders)
{
	if (strPathName.IsEmpty())
		return TRUE;

	CString strTBFSFolder = GetTBFSFileCompleteName(strPathName);

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;
	SqlDataReader^ dr = nullptr;

	String^ relativePath;
	String^ connectionString;
	String^ commandText;
	String^ tableName;
	bool isCustom = false;
	try
	{
		if (AfxGetPathFinder()->IsCustomPath(strTBFSFolder))
		{
			if (GetCustomConnectionString().IsEmpty())
				return FALSE;
			isCustom = true;
		}
		connectionString = (isCustom) ? gcnew String(GetCustomConnectionString()) : gcnew String(m_StandardConnectionString);
		relativePath = gcnew String(GetRelativePath(strTBFSFolder, isCustom));
		tableName = (isCustom) ? gcnew String(szTBCustomTBFS) : gcnew String(szMPInstanceTBFS);

		commandText = String::Format("Select X.PathName from {0} X,  {0} Y WHERE X.ParentID = Y.FileID AND Y.PathName =  \'{1}\' AND X.IsDirectory = \'1\'", tableName, relativePath);
		//devo aggiungere il filtro per l'instance name
		if (!isCustom)
			commandText += String::Format(" AND X.InstanceKey = \'{0}\'", gcnew String(szInstanceKey));
		MakeTimeOperation(QUERY_METADATA, START_TIME);
		sqlConnection = gcnew SqlConnection(connectionString);
		sqlConnection->Open();
		sqlCommand = gcnew SqlCommand(commandText, sqlConnection);
		dr = sqlCommand->ExecuteReader();
		MakeTimeOperation(QUERY_METADATA, STOP_TIME);
		MakeTimeOperation(FETCH_METADATA, START_TIME);
		while (dr->Read())
			pSubFolders->Add(GetAbsolutePath((String^)dr["PathName"], isCustom));
		MakeTimeOperation(FETCH_METADATA, STOP_TIME);
		if (dr)
		{
			dr->Close();
			delete dr;
		}
		delete sqlCommand;
		sqlConnection->Close();
		delete sqlConnection;
	}
	catch (SqlException^ e)
	{
		if (dr)
		{
			dr->Close();
			delete dr;
		}
		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
		AfxGetDiagnostic()->Add(e->Message);
		return FALSE;
	}
	return TRUE;
}



//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::GetTBFolderContent(const CString& strPathName, TBMetadataArray* pFolderContent, BOOL bFolders, BOOL bFiles, const CString& strFileExt)
{
	if (strPathName.IsEmpty())
		return TRUE;
	CString strTBFSFolder = GetTBFSFileCompleteName(strPathName);

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;
	SqlDataReader^ dr = nullptr;

	CString strConnectionString;
	CString strCommandText;
	CString strTableName;
	CString strRelativePath;
	bool isCustom = false;
	try
	{
		if (AfxGetPathFinder()->IsCustomPath(strTBFSFolder))
		{
			if (GetCustomConnectionString().IsEmpty())
				return FALSE;
			isCustom = true;
		}
		strConnectionString = (isCustom) ? GetCustomConnectionString() : m_StandardConnectionString;
		strRelativePath = GetRelativePath(strTBFSFolder, isCustom);
		strTableName = (isCustom) ? szTBCustomTBFS : szMPInstanceTBFS;

		strCommandText = cwsprintf(_T("Select X.* FROM {%s} X,  {%s} Y WHERE X.ParentID = Y.FileID AND Y.PathName =  \'{%s}\''"), strTableName, strRelativePath);

		if (bFiles && (!strFileExt.IsEmpty() || strFileExt.CompareNoCase(_T("*.*")) != 0))
		{
			CString fileType = (strFileExt.Find(_T('*')) == 0) ? strFileExt.Right(strFileExt.GetLength() - 1) : strFileExt;
			strCommandText += (bFolders)
				? cwsprintf(_T(" AND (X.IsDirectory = \'0\' OR (X.IsDirectory = \'1\' AND X.FileType = \'{0}\'))"), fileType)
				: cwsprintf(_T(" AND (X.IsDirectory = \'1\' AND X.FileType = \'{0}\')"), fileType);
		}
		else
			if (bFolders || bFiles)
				strCommandText += cwsprintf(_T(" AND X.IsDirectory = \'%s\'"), (bFolders) ? '0' : '1');

		if (!isCustom)
			strCommandText += cwsprintf(_T(" AND Y.InstanceKey = \'%s\'"), szInstanceKey);

		GetTBFilesInfo(strConnectionString, strCommandText, pFolderContent, isCustom, m_pMetadataPerformance);
	}
	catch (SqlException^ e)
	{
		AfxGetDiagnostic()->Add(e->Message);
		return FALSE;
	}	
	return TRUE;
}


//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::GetPathContent(const CString& strPathName, BOOL bFolders, CStringArray* pSubFolders, BOOL bFiles, const CString& strFileExt, CStringArray* pFiles)
{
	if (strPathName.IsEmpty())
		return TRUE;
	CString strTBFSFolder = GetTBFSFileCompleteName(strPathName);

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;
	SqlDataReader^ dr = nullptr;

	String^ relativePath;
	String^ connectionString;
	String^ commandText;
	String^ tableName;
	bool isCustom = false;
	try
	{
		if (AfxGetPathFinder()->IsCustomPath(strTBFSFolder))
		{
			if (GetCustomConnectionString().IsEmpty())
				return FALSE;
			isCustom = true;
		}
		connectionString = (isCustom) ? gcnew String(GetCustomConnectionString()) : gcnew String(m_StandardConnectionString);
		relativePath = gcnew String(GetRelativePath(strTBFSFolder, isCustom));
		tableName = (isCustom) ? gcnew String(szTBCustomTBFS) : gcnew String(szMPInstanceTBFS);

		commandText = String::Format("Select X.CompleteFileName, X.PathName, X.IsDirectory FROM {0} X,  {0} Y WHERE X.ParentID = Y.FileID AND Y.PathName =  \'{1}\''", tableName, relativePath);

		if (bFiles && (!strFileExt.IsEmpty() || strFileExt.CompareNoCase(_T("*.*")) != 0))
		{
			CString fileType = (strFileExt.Find(_T('*')) == 0) ? strFileExt.Right(strFileExt.GetLength() - 1) : strFileExt;
			commandText += (bFolders)
				? String::Format(" AND (X.IsDirectory = \'0\' OR (X.IsDirectory = \'1\' AND X.FileType = \'{0}\'))", gcnew String(fileType))
				: String::Format(" AND (X.IsDirectory = \'1\' AND X.FileType = \'{0}\'))", gcnew String(fileType));
		}
		else
			if (bFolders || bFiles)
				commandText += String::Format(" AND X.IsDirectory = \'{0}\'))", (bFolders) ? '0' : '1');
		
		if (!isCustom)
			commandText += String::Format(" AND Y.InstanceKey =\'{0}\'", gcnew String(szInstanceKey));
		
		MakeTimeOperation(QUERY_METADATA, START_TIME);
		sqlConnection = gcnew SqlConnection(connectionString);
		sqlConnection->Open();
		sqlCommand = gcnew SqlCommand(commandText, sqlConnection);
		dr = sqlCommand->ExecuteReader();
		MakeTimeOperation(QUERY_METADATA, STOP_TIME);
		MakeTimeOperation(FETCH_METADATA, START_TIME);
		while (dr->Read())
		{
			if ((String^)dr["IsDirectory"] == "1")
				pSubFolders->Add(GetAbsolutePath((String^)dr["PathName"], isCustom));
			else
				pFiles->Add(GetAbsolutePath((String^)dr["CompleteFileName"], isCustom));
		}
		MakeTimeOperation(FETCH_METADATA, STOP_TIME);
		if (dr)
		{
			dr->Close();
			delete dr;
		}
		delete sqlCommand;
		sqlConnection->Close();
		delete sqlConnection;
	}
	catch (SqlException^ e)
	{
		if (dr)
		{
			dr->Close();
			delete dr;
		}
		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
		AfxGetDiagnostic()->Add(e->Message);
		return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL TBFSDatabaseDriver::GetFiles(const CString& strPathName, const CString& strFileExt, CStringArray* pFiles)
{
	if (!pFiles || strPathName.IsEmpty())
		return TRUE;
	CString strTBFSFolder = GetTBFSFileCompleteName(strPathName);

	SqlConnection^ sqlConnection = nullptr;
	SqlCommand^	   sqlCommand = nullptr;
	SqlDataReader^ dr = nullptr;

	String^ relativePath;
	String^ connectionString;
	String^ commandText;
	String^ tableName;
	bool isCustom = false;
	try
	{
		if (AfxGetPathFinder()->IsCustomPath(strTBFSFolder))
		{
			if (GetCustomConnectionString().IsEmpty())
				return FALSE;
			isCustom = true;
		}
		connectionString = (isCustom) ? gcnew String(GetCustomConnectionString()) : gcnew String(m_StandardConnectionString);
		relativePath = gcnew String(GetRelativePath(strTBFSFolder, isCustom));
		tableName = (isCustom) ? gcnew String(szTBCustomTBFS) : gcnew String(szMPInstanceTBFS);

		commandText = String::Format("Select X.CompleteFileName from {0} X,  {0} Y WHERE X.ParentID = Y.FileID AND Y.PathName =  \'{1}\' AND X.IsDirectory = \'0\'", tableName, relativePath);

		if (!strFileExt.IsEmpty() || strFileExt.CompareNoCase(_T("*.*")) != 0)
		{
			CString fileType = (strFileExt.Find(_T('*')) == 0) ? strFileExt.Right(strFileExt.GetLength() - 1) : strFileExt;			
			commandText += String::Format(" AND X.FileType = \'{0}\'", gcnew String(fileType));
		}

		if (!isCustom)
			commandText += String::Format(" AND Y.InstanceKey =\'{0}\'", gcnew String(szInstanceKey));

		MakeTimeOperation(QUERY_METADATA, START_TIME);
		sqlConnection = gcnew SqlConnection(connectionString);
		sqlConnection->Open();
		sqlCommand = gcnew SqlCommand(commandText, sqlConnection);
		dr = sqlCommand->ExecuteReader();
		MakeTimeOperation(QUERY_METADATA, STOP_TIME);

		MakeTimeOperation(FETCH_METADATA, START_TIME);
		while (dr->Read())
			pFiles->Add(GetAbsolutePath((String^)dr["CompleteFileName"], isCustom));
		MakeTimeOperation(FETCH_METADATA, STOP_TIME);

		if (dr)
		{
			dr->Close();
			delete dr;
		}
		delete sqlCommand;
		sqlConnection->Close();
		delete sqlConnection;
	}
	catch (SqlException^ e)
	{
		if (dr)
		{
			dr->Close();
			delete dr;
		}
		if (sqlCommand)
			delete sqlCommand;

		if (sqlConnection)
		{
			sqlConnection->Close();
			delete sqlConnection;
		}
		AfxGetDiagnostic()->Add(e->Message);
		return FALSE;
	}
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void TBFSDatabaseDriver::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "TBFSDatabaseDriver\n");
}

void TBFSDatabaseDriver::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG