
#include "stdafx.h"

#include <TbNameSolver\LoginContext.h>
#include <TbClientCore\ClientObjects.h>

#include <TbGeneric\ParametersSections.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\addonmng.h>
#include <TbGenlib\messages.h>
#include <TbGenlib\baseapp.h>

#include <TbWebServicesWrappers\LockManagerInterface.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGenlib\SettingsTableManager.h>

#include "oledbmng.h"
#include "sqlcatalog.h"
#include "sqlconnect.h"
#include "sqltable.h"

// resources
#include <TbGenlib\extres.hjson> //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

static TCHAR const szTraceSqlFile[] = _T("TraceSql");
static TCHAR const szTraceExt[] = _T(".log");
static TCHAR const szBackSlashSubstitute = _T('.');

#define TRUNCATE_SIZE		220


//////////////////////////////////////////////////////////////////////////////
//					CDefaultSqlSessions Implementation
//////////////////////////////////////////////////////////////////////////////
class ThreadSqlConnectionPool : public SqlConnectionPool
{
public:
	~ThreadSqlConnectionPool()
	{
		SqlConnection* pConnection = NULL;
		for (int i = 0; i < GetSize(); i++)
		{
			pConnection = GetAt(i);
			pConnection->Close();
		}
	}		
};


///////////////////////////////////////////////////////////////////////////////
//					class COleDbManager implementation
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(COleDbManager, SqlObject)

//DECLARE_THREAD_VARIABLE
//(
//	ThreadSqlConnectionPool,
//	m_arThreadSqlConnections
//);

//-----------------------------------------------------------------------------
COleDbManager::COleDbManager()
:
	m_bValid						(false),
	m_bUseOptimisticLock			(FALSE),
	m_bUseLockManager				(TRUE),
	m_DMSStatus						(DbInvalid)
{
	m_aConnectionPool.SetOwnConnection(false); //le connessioni sono di proprietà del thread
}

//-----------------------------------------------------------------------------
COleDbManager::~COleDbManager()
{
	//CloseAllConnections();
	m_aProviderInfoPool.RemoveAll();
}

//------------------------------------------------------------------------------
BOOL COleDbManager::CloseAllConnections()
{
	TB_LOCK_FOR_WRITE();
	TRY
	{
		m_aConnectionPool.CloseAll();
		return TRUE;
	}
	CATCH(SqlException,e )
	{
		m_pContext->ShowMessage(cwsprintf(_TB("Error closing connections: {0-%s}"), e->m_strError));
		return FALSE;
	}
	END_CATCH
}

//------------------------------------------------------------------------------
BOOL COleDbManager::CanCloseAllConnections()
{
	TB_LOCK_FOR_READ();
	return m_aConnectionPool.CanCloseAll();
}

//------------------------------------------------------------------------------
void COleDbManager::OnDSNChanged()
{	
	SqlConnection* pPrimaryConnection = GetPrimaryConnection();

	if (pPrimaryConnection)
	{
		if (!pPrimaryConnection->CanClose())
			return;
		pPrimaryConnection->Close();
	}

	if (MakePrimaryConnection())
	{
	}		
}

//------------------------------------------------------------------------------
SqlSession* COleDbManager::GetDefaultSqlSession()
{
	SqlConnection* pConnection = GetDefaultSqlConnection();
	
	return (pConnection) ? pConnection->GetDefaultSqlSession() : NULL;
}


//------------------------------------------------------------------------------
void COleDbManager::SetDefaultSqlConnection(SqlConnection* pSqlConnection)
{
	//per prima cosa inserisco la connessione nel connectionpool di thread dove agirà il garbage per chiudere le connessioni non più utilizzate	
	SqlConnectionPool* pSqlConnectionPool = (SqlConnectionPool*)AfxGetThreadContext()->GetSqlConnectionPool();
	if (!pSqlConnectionPool)
	{
		pSqlConnectionPool = new SqlConnectionPool();
		AfxGetThreadContext()->AttachSqlConnectionPool((SqlConnectionPoolObj*)pSqlConnectionPool);
	}
	pSqlConnectionPool->SetPrimaryConnection(pSqlConnection);

	//se non sono nel thread di documento ma dell'applicazione allora inserisco nell'array m_aConnectionPool la connessione
	//questo serve per poter effettuare la clonazione
	if (!AfxGetTBThread() || !AfxGetTBThread()->IsDocumentThread())
		m_aConnectionPool.SetPrimaryConnection(pSqlConnection);
}


//------------------------------------------------------------------------------
SqlConnection* COleDbManager::GetDefaultSqlConnection()
{
	//se non sono nel thread di documento
	if (AfxGetTBThread() && AfxGetTBThread()->IsDocumentThread())
	{
		SqlConnection* pThreadPrimaryConnection = NULL;	
		SqlConnectionPool* pSqlConnectionPool = (SqlConnectionPool*)AfxGetThreadContext()->GetSqlConnectionPool();
		if (!pSqlConnectionPool)
		{
			pSqlConnectionPool = new SqlConnectionPool();
			AfxGetThreadContext()->AttachSqlConnectionPool((SqlConnectionPoolObj*)pSqlConnectionPool);
		}
		pThreadPrimaryConnection = pSqlConnectionPool->GetPrimaryConnection();
		if (!pThreadPrimaryConnection)
		{
			SqlConnection* pPrimaryConnection = GetPrimaryConnection();
			if (pPrimaryConnection)
			{
				pThreadPrimaryConnection = pPrimaryConnection->Clone();
				pSqlConnectionPool->SetPrimaryConnection(pThreadPrimaryConnection);
			}
		}
		return pThreadPrimaryConnection;
	}
	return GetPrimaryConnection();
}

//------------------------------------------------------------------------------
SqlConnection* COleDbManager::GetSecondarySqlConnection(const CString& strAlias)
{
	if (AfxGetTBThread() && AfxGetTBThread()->IsDocumentThread())
	{
		SqlConnection* pThreadSqlConnection = NULL;
		SqlConnectionPool* pSqlConnectionPool = (SqlConnectionPool*)AfxGetThreadContext()->GetSqlConnectionPool();
		if (!pSqlConnectionPool)
		{
			pSqlConnectionPool = new SqlConnectionPool();
			AfxGetThreadContext()->AttachSqlConnectionPool((SqlConnectionPoolObj*)pSqlConnectionPool);
		}
		else
			pThreadSqlConnection = pSqlConnectionPool->GetSqlConnectionByAlias(strAlias);

		if (!pThreadSqlConnection)
		{
			SqlConnection* pSqlConnection = GetSqlConnectionByAlias(strAlias);
			if (pSqlConnection)
			{
				pThreadSqlConnection = pSqlConnection->Clone();
				pSqlConnectionPool->Add(pThreadSqlConnection);
			}
		}
		return pThreadSqlConnection;
	}
	return GetSqlConnectionByAlias(strAlias);
}

//------------------------------------------------------------------------------
void COleDbManager::SetSecondarySqlConnection(SqlConnection* pSqlConnection)
{
	SqlConnectionPool* pSqlConnectionPool = (SqlConnectionPool*)AfxGetThreadContext()->GetSqlConnectionPool();
	if (!pSqlConnectionPool)
	{
		pSqlConnectionPool = new SqlConnectionPool();
		AfxGetThreadContext()->AttachSqlConnectionPool((SqlConnectionPoolObj*)pSqlConnectionPool);
	}
	pSqlConnectionPool->Add(pSqlConnection);
	if (!m_aConnectionPool.GetSqlConnectionByAlias(pSqlConnection->m_strAlias))
		m_aConnectionPool.Add(pSqlConnection);
}

//-----------------------------------------------------------------------------
CString COleDbManager::GetDMSConnectionString()
{
	// the string to connect to document management database is empty, I must call it to LoginManager
	if (m_strDMSConnectionString.IsEmpty())
		m_strDMSConnectionString = AfxGetLoginManager()->GetDMSConnectionString();
	
	return m_strDMSConnectionString;
}

//-----------------------------------------------------------------------------
BOOL COleDbManager::EasyAttachmentEnable()
{
	return (
				AfxIsActivated(TBEXT_APP, EASYATTACHMENT_ACT) &&
				!GetDMSConnectionString().IsEmpty() &&
				m_DMSStatus == Valid
			);
}

//-----------------------------------------------------------------------------
BOOL COleDbManager::EasyAttachmentFullEnable()
{
	return (
				AfxIsActivated(TBEXT_APP, EASYATTACHMENT_FULL_ACT) &&
				!GetDMSConnectionString().IsEmpty() &&
				m_DMSStatus == Valid
			);
}

//-----------------------------------------------------------------------------
BOOL COleDbManager::DMSMassiveEnable()
{
	return (
				AfxIsActivated(TBEXT_APP, DMS_MASSIVE_ACT) &&
				!GetDMSConnectionString().IsEmpty() &&
				m_DMSStatus == Valid
			);
}

//-----------------------------------------------------------------------------
BOOL COleDbManager::DMSSOSEnable()
{
	return (
		AfxIsActivated(TBEXT_APP, DMS_SOS_ACT) &&
		!GetDMSConnectionString().IsEmpty() &&
		m_DMSStatus == Valid
		);
}

//-----------------------------------------------------------------------------
SqlConnection* COleDbManager::GetNewConnection
									(
										LPCWSTR	szConnectionString, 
										BOOL	bCheckRegisterTable /*=FALSE*/,
										CBaseContext* pContext /*=NULL*/,
										const CString& strAlias /*= _T("")*/
									)
{
	//	diagnostica di ausilio
	TRACE1("COleDbManager::GetNewConnection: %s\r\n", szConnectionString);
	TB_LOCK_FOR_WRITE();
	SqlConnection* pSqlConnection = new SqlConnection(szConnectionString, bCheckRegisterTable, pContext, strAlias);
	
	bool bCompanyDB = (_tcscmp(szConnectionString, T2W((LPTSTR)((LPCTSTR)AfxGetLoginInfos()->m_strProviderCompanyConnectionString))) == 0);
	if (bCompanyDB)
		pSqlConnection->SetUseUnicode(GetPrimaryConnection() != NULL && GetPrimaryConnection()->UseUnicode());

	if (_tcscmp(szConnectionString, T2W((LPTSTR)((LPCTSTR)AfxGetLoginManager()->GetSystemDBConnectionString()))) == 0)
		pSqlConnection->SetUseUnicode(AfxGetLoginInfos()->m_bUseUnicode);

	TRY
	{
		if (pSqlConnection->MakeConnection())
		{
			SetSecondarySqlConnection(pSqlConnection);
			//m_aConnectionPool.Add(pSqlConnection);
			/*if (bCompanyDB)
				CheckLockManager(pSqlConnection);*/
			return pSqlConnection;
		}
		else
		{
			delete pSqlConnection;
			return NULL;
		}
	}
	CATCH(SqlException, e)
	{
		delete pSqlConnection;
		THROW_LAST();
		return NULL;
    }
	END_CATCH
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL COleDbManager::MakePrimaryConnection() 
{
	//no lock is needed: this method is called only at login time
	m_bValid = false;
	
	m_strTraceSqlActions = *(DataStr*)AfxGetSettingValue(snsTbOleDb, szConnectionSection, szDebugSqlTraceActions, DataStr(_T("")), szTbDefaultSettingFileName);
	CString strTraceSqlTables = *(DataStr*)AfxGetSettingValue(snsTbOleDb, szConnectionSection, szDebugSqlTraceTables, DataStr(_T("")), szTbDefaultSettingFileName);
	
	// 03/11/2010: change DebugSqlTrace file log creation. Now I create a file for each day without remove the traced information.
	// Each days there is one file for each connected user that contains all selected traced operation for one or more connections done during the day. 
	if (IsTraceSQLEnabled())
	{
		SYSTEMTIME dateTime;
		::GetSystemTime(&dateTime);
		CString time;
		time.Format(_T("%u-%.2u-%.2u"),
					dateTime.wYear, dateTime.wMonth, dateTime.wDay);
		CString sTmp = AfxGetLoginInfos()->m_strUserName;	
		sTmp.Replace(SLASH_CHAR, szBackSlashSubstitute);
		m_strTraceSqlFile = AfxGetPathFinder()->GetLogDataIOPath(TRUE) + SLASH_CHAR +
							sTmp + _T("_") + szTraceSqlFile + _T("_") + time + szTraceExt;

		if (!m_strTraceSqlActions.IsEmpty())
		{
			int curPos= 0;
			CString strToken= m_strTraceSqlActions.Tokenize(_T(","),curPos);
			while (strToken != "")
			{
				m_TracedSqlActions.SetAt(strToken,int(0));
				strToken= m_strTraceSqlActions.Tokenize(_T(","),curPos);
			}
		}
		 
		//The framework allows the user to trace only some tables. This is possible 
		if (!strTraceSqlTables.IsEmpty())
		{
			int curPos= 0;
			CString strToken= strTraceSqlTables.Tokenize(_T(","),curPos);
			while (!strToken.IsEmpty())
			{
				strToken.Remove(_T(' '));
				m_TracedSqlTables.Add(strToken);
				strToken= strTraceSqlTables.Tokenize(_T(","),curPos);
			}
		}	
	}

	AfxSetStatusBarText(cwsprintf(_TB("Connecting to company {0-%s}..."), AfxGetLoginInfos()->m_strCompanyName));
	
	//SqlConnection* pSqlConnection = new SqlConnection(AfxGetLoginInfos()->m_strNonProviderCompanyConnectionString, TRUE, m_pContext);
	SqlConnection* pSqlConnection = new SqlConnection(AfxGetLoginInfos()->m_strProviderCompanyConnectionString, TRUE, m_pContext, _T("PRIMARY"));
	pSqlConnection->SetProviderId(AfxGetLoginInfos()->m_nProviderId);
	pSqlConnection->SetUseUnicode(AfxGetLoginInfos()->m_bUseUnicode);

	TRY
	{
		// il check del database lo devo effettuare esternamente xché prima
		// devo settare la connessione di default per poter istanziare correttamente
		// sqlmark
		SetDefaultSqlConnection(pSqlConnection);
		if (!pSqlConnection->MakeConnection())
		{
			m_aConnectionPool.RemovePrimaryConnection();
			m_bValid = false;
		}
		else
		{
			m_bValid = (pSqlConnection->m_bValid == TRUE);
			//CheckLockManager(pSqlConnection);
		}
	}
		
	return m_bValid;

	CATCH(SqlException, e)
	{
		m_pContext->AddMessage(e->m_strError);
		m_bValid = false;
		return m_bValid;
    }
	AND_CATCH_ALL(e)
	{
		THROW_LAST();
		return FALSE;
	}
	END_CATCH_ALL
	
}

//------------------------------------------------------------------------------
BOOL COleDbManager::CloseConnection(SqlConnection* pSqlConnection)
{
	TB_LOCK_FOR_WRITE();
	TRY
	{
		return m_aConnectionPool.RemoveConnection(pSqlConnection);
	}

	CATCH(SqlException,e )
	{
		m_pContext->ShowMessage(cwsprintf(_TB("Error closing connections: {0-%s}"), e->m_strError));
		return FALSE;
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
BOOL COleDbManager::RegisterAddOnTable(AddOnLibrary* pAddOnLib)
{
	// devo ciclare su tutte le connessioni attive che prevedono la registrazione 
	// delle tabelle dell'applicativo
	TB_LOCK_FOR_WRITE();
	BOOL bOk = TRUE;
	for (int i = 0; i < m_aConnectionPool.GetSize(); i++)
	{
		SqlConnection* pConnection = m_aConnectionPool.GetAt(i);
		if (pConnection->m_bCheckRegisterTable)
			bOk = pConnection->RegisterAddOnLibrayTables(pAddOnLib) && bOk;
	}	
	return bOk;
}

////-----------------------------------------------------------------------------
//SqlConnection* COleDbManager::GetFirstActiveConnection
//								(
//									const CString& strServerName,
//									const CString& strDBName
//								)
//{
//	TB_LOCK_FOR_READ();
//	return (m_aConnectionPool.GetFirstActiveConnection(strServerName, strDBName));
//}

//-----------------------------------------------------------------------------
SqlProviderInfo* COleDbManager::GetProviderInfo(LPCTSTR pszProviderName, 
											   LPCTSTR pszProviderVersion,
											   SqlConnection* pConnection)
{		
	TB_LOCK_FOR_WRITE();
	SqlProviderInfo* pInfo = m_aProviderInfoPool.GetProviderInfo(pszProviderName, pConnection->UseUnicode());
	
	if (pInfo)
		return pInfo;
	
	pInfo = new SqlProviderInfo(pszProviderName, pszProviderVersion);
	TRY
	{
		if (pInfo->LoadProviderInfo(pConnection))
			m_aProviderInfoPool.Add(pInfo);
		else
		{
			delete pInfo;
			pInfo = NULL;
		}
	}
	return pInfo;

	CATCH(SqlException, e)
	{
		pConnection->ShowMessage(e->m_strError);
		delete pInfo;
		return NULL;
    }
	END_CATCH
}


// gestione dell'accesso limitato ad un solo utente
//-----------------------------------------------------------------------------
BOOL COleDbManager::EnableMultiUserMode(BOOL bEnable)
{ 
	return /*m_pLockTable ? m_pLockTable->EnableMultiUserMode (bEnable) :*/ TRUE; 
}

//-----------------------------------------------------------------------------
BOOL COleDbManager::MultiUserModeEnabled() const			
{ 
	return /*m_pLockTable ? m_pLockTable->MultiUserModeEnabled() : */TRUE; 
}


//-----------------------------------------------------------------------------
void COleDbManager::DebugTraceSQL(LPCTSTR szTrace, SqlObject* pSqlObject )
{
	TB_LOCK_FOR_WRITE();
	UINT nFlags = 
		CFile::modeCreate | CFile::modeWrite | 
		CFile::shareDenyWrite | CFile::typeText |  CFile::modeNoTruncate;
	//// alla connessione tronca il file e quindi lo ripulisce
	//if (_tcsicmp(_T("Connect"), szTrace) || ExistFile(m_strTraceSqlFile))
	//	nFlags |= CFile::modeNoTruncate;

	int nBlank, nCount;
	CString strAction(szTrace);
	if ((nBlank = strAction.FindOneOf(_T(" "))) != -1)
		strAction = strAction.Left(nBlank);

	if (m_TracedSqlActions.Lookup(strAction, nCount))
		m_TracedSqlActions.SetAt(strAction, ++nCount);
	else
		// if empty filter string, trace all
		if (m_strTraceSqlActions.IsEmpty())
			m_TracedSqlActions.SetAt(strAction, int(1));
		else
			// skip action if not in filter string, but for "connect" action
			if (strAction.CompareNoCase(_T("Connect")) != 0)
				return;

	if (m_TracedSqlTables.GetCount() > 0)
	{
		BOOL bFound = FALSE;
		
		if (pSqlObject)
		{
			ASSERT_VALID(pSqlObject);
			//se è di tipo SqlRowSet controllo se la tabella è tra quelle da controllare
			if (pSqlObject->IsKindOf(RUNTIME_CLASS(SqlRowSet)))
			{
				SqlRowSet* pSqlRowSet = (SqlRowSet*)pSqlObject;
				CString strTable;
				CString strTracedTable;
				int j = 0;
				if (pSqlRowSet->m_strCurrentTables.GetCount() > 0)
					strTable = pSqlRowSet->m_strCurrentTables.GetAt(0);
			/* TODO 
				Si schianta in uscita di alcune procedure batch che  deletano un DBTSlaveBuffered NON attacciato
				durante la delete di m_pTbContext nel distruttore di CAbstractFormDoc.
				["il record potrebbe essere stato cancellato da fuori ma il puntatore non essere ancora NULL"]
				c'e' un commento simile sopra il metodo "SqlRowSet::Close()"
			*/
				//else if (pSqlRowSet->IsKindOf(RUNTIME_CLASS(SqlTable)))
				//{
				//	SqlTable* pSqlTable = (SqlTable*)pSqlRowSet;
				//	ASSERT_VALID(pSqlTable->GetRecord());	// ==> CRASH
				//	if (!pSqlTable->GetTableName().IsEmpty())
				//		strTable = pSqlTable->GetTableName();
				//}
			/* ---- */

				while (!bFound && !strTable.IsEmpty())
				{
					for (int i = 0; i < m_TracedSqlTables.GetCount(); i++)

					{
						strTracedTable = m_TracedSqlTables.GetAt(i);
						if (strTracedTable.CompareNoCase(strTable) == 0)
						{
							bFound = TRUE;
							break;
						}
					}
					if (j++ > pSqlRowSet->m_strCurrentTables.GetCount())
						strTable = pSqlRowSet->m_strCurrentTables.GetAt(j);
					else
						strTable.Empty();
				}
				if (!bFound)
					return;
			}
		}

		//se è di tipo SqlSession controllo se il documento di appartenenza è tra quelle da controllare
		if (pSqlObject && pSqlObject->IsKindOf(RUNTIME_CLASS(SqlSession)))
		{
			SqlSession* pSqlSession = (SqlSession*)pSqlObject;
			if (pSqlSession->m_pContext && pSqlSession->m_pContext->GetDocument())
			{
				CString strDocumentRTC(pSqlSession->m_pContext->GetDocument()->GetRuntimeClass()->m_lpszClassName);
				for (int i = 0; i < m_TracedSqlTables.GetCount(); i++)
				{
					if (strDocumentRTC.CompareNoCase(m_TracedSqlTables.GetAt(i)) == 0)
					{
						bFound = TRUE;
						break;
					}
				}
				if (!bFound)
					return;
			}			
		}
	}

	CLineFile oFile;
	CFileException e;

	if (!oFile.Open(m_strTraceSqlFile, nFlags, &e))
		return;

	SYSTEMTIME dateTime;
	::GetSystemTime(&dateTime);
	CString time;
	time.Format(_T("%u-%.2u-%.2u-%.2u:%.2u,%.2u"),
				dateTime.wYear, dateTime.wMonth, dateTime.wDay,
				dateTime.wHour, dateTime.wMinute, dateTime.wSecond);


	CString strTrace (_T("   "));
	oFile.SeekToEnd();
	//if (_tcsicmp(_T("Connect"), szTrace) == 0)
	//{
	//	SqlSession* pSqlSession = (SqlSession*)pSqlObject;

	//	strTrace += cwsprintf(_T("%s %p %s %s"), time, pSqlObject, szTrace, pSqlSession->m_pSqlConnection->m_strDbmsName);

	//	CString sTitle;
	//	sTitle.Format(
	//		_T("\r\n\r\n")
	//		_T("************************************************\r\n")
	//		_T("Connect to database %s\r\n")
	//		_T("%s %p %s\r\n\t\r\n"),
	//		pSqlSession->m_pSqlConnection->m_strDbmsName,
	//		time, pSqlObject, szTrace);
	//	strTrace += sTitle;

	//}
	//else
	//if (_tcsicmp(_T("Disconnect"), szTrace) == 0)
	//{
	//	SqlSession* pSqlSession = (SqlSession*)pSqlObject;

	//	strTrace += cwsprintf(_T("%s %p %s %s"), time, pSqlObject, szTrace, pSqlSession->m_pSqlConnection->m_strDbmsName);

	//	CString sTitle;
	//	sTitle.Format(
	//		_T("\r\n\r\n")
	//		_T("************************************************\r\n")
	//		_T("Disconnect to database %s\r\n")
	//		_T("%s %p %s\r\n\t\r\n"),
	//		pSqlSession->m_pSqlConnection->m_strDbmsName,
	//		time, pSqlObject, szTrace);
	//	strTrace += sTitle;

	//}
	//else
		strTrace += cwsprintf(_T("%s %p %s"), time, pSqlObject, szTrace);

	// Display 255 chars/line
	while (strTrace.GetLength() > TRUNCATE_SIZE)
	{
		oFile.WriteString(strTrace.Left(TRUNCATE_SIZE) + _T("\n"));
		strTrace = _T("            ") + strTrace.Right(strTrace.GetLength() - TRUNCATE_SIZE);
	}
	oFile.WriteString(strTrace + _T("\n"));
	oFile.Close();
}
//-----------------------------------------------------------------------------
void COleDbManager::ResetSQLActionCounters(const CString& strMess)
{	
	if (!IsTraceSQLEnabled()) //written only at startup, no lock needed
		return;

	TB_LOCK_FOR_WRITE();
	
	POSITION aPos = m_TracedSqlActions.GetStartPosition();
	CString strKey;
	int		nVal;
	while (aPos != NULL)
	{
		m_TracedSqlActions.GetNextAssoc(aPos,strKey,nVal);
		m_TracedSqlActions.SetAt(strKey,int(0));
	}

	CLineFile oFile;
	CFileException e;
	UINT nFlags = 
		CFile::modeCreate | CFile::modeWrite | 
		CFile::shareDenyWrite | CFile::typeText | CFile::modeNoTruncate;
	if (!oFile.Open(m_strTraceSqlFile, nFlags, &e))
		return;
	CString strTrace('#',69 - strMess.GetLength());
	oFile.SeekToEnd();
	oFile.WriteString(CString(_T("\n")) + _T("### RESET  ") + strTrace + strMess + _T("\n\n"));
	oFile.Close();
}

//-----------------------------------------------------------------------------
void COleDbManager::CheckLockManager(SqlConnection* pSqlConnection)
{
	SqlConnection* pConn;
	for (int i=0; i <= m_aConnectionPool.GetUpperBound(); i++)
	{
		pConn = m_aConnectionPool.GetAt(i);
		if (pSqlConnection != pConn && pSqlConnection->m_strDBName.CompareNoCase(pConn->m_strDBName) == 0)
			return;
	}

	// if it is the first connection on database name, I init lockmanager
	AfxGetLockManager()->Init(pSqlConnection->GetDatabaseName());
}

//-----------------------------------------------------------------------------
void COleDbManager::TraceSQLSeparator(const CString& strMess)
{
	if (!IsTraceSQLEnabled()) //written only at startup, no lock needed
		return;
	
	TB_LOCK_FOR_WRITE();

	CLineFile oFile;
	CFileException e;
	UINT nFlags = 
		CFile::modeCreate | CFile::modeWrite | 
		CFile::shareDenyWrite | CFile::typeText | CFile::modeNoTruncate;
	if (!oFile.Open(m_strTraceSqlFile, nFlags, &e))
		return;
	CString strTrace('-',80 - strMess.GetLength());
	oFile.SeekToEnd();
	oFile.WriteString(_T("\n") + strTrace + strMess + _T("\n\n"));
	oFile.Close();
}

//-----------------------------------------------------------------------------
CString COleDbManager::GetSQLActionCounters(const CString& strMess)
{
	if (!IsTraceSQLEnabled()) //written only at startup, no lock needed
		return _T(" ");

	TB_LOCK_FOR_WRITE();

	POSITION aPos = m_TracedSqlActions.GetStartPosition();
	CString strKey;
	CString strResult;
	int		nVal;
	while (aPos != NULL)
	{
		m_TracedSqlActions.GetNextAssoc(aPos,strKey,nVal);
		strKey.Insert(strKey.GetLength()+1,CString(' ',max(25 - strKey.GetLength(), 1)));
		strResult += cwsprintf(_T("%s %5d\n"),(LPCTSTR)strKey,nVal);
	}

	CLineFile oFile;
	CFileException e;
	UINT nFlags = 
		CFile::modeCreate | CFile::modeWrite | 
		CFile::shareDenyWrite | CFile::typeText | CFile::modeNoTruncate;
	if (!oFile.Open(m_strTraceSqlFile, nFlags, &e))
		return strResult;
	CString strTrace('=',80 - strMess.GetLength());
	oFile.SeekToEnd();
	oFile.WriteString(_T("\n") + strTrace + strMess + _T("\n") + strResult + strTrace + strMess + _T("\n\n"));
	oFile.Close();

	return strResult;
}

//------------------------------------------------------------------------------
void  COleDbManager::CacheParameters ()
{
	TB_LOCK_FOR_WRITE();

	// parameters caching
	DataObj* pDataObj = AfxGetSettingValue
	(
		snsTbOleDb,
		szLockManager,
		szUseOptimisticLock,
		DataBool(FALSE),
		szTbDefaultSettingFileName
	);
	m_bUseOptimisticLock = pDataObj && *((DataBool*)pDataObj);

	pDataObj = AfxGetSettingValue 
				(
					snsTbOleDb, 
					szLockManager, 
					szUseLockManager, 
					DataBool(TRUE),
					szTbDefaultSettingFileName
				);
	m_bUseLockManager = pDataObj && *((DataBool*) pDataObj);


	pDataObj = AfxGetSettingValue
	(
		snsTbOleDb,
		szLockManager,
		szUseNewSqlLockManager,
		DataBool(TRUE),
		szTbDefaultSettingFileName
	);

	m_bUseNewSqlLockManager = pDataObj && *((DataBool*)pDataObj);
}

//------------------------------------------------------------------------------
COleDbManager* AfxGetOleDbMng()
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (!pContext)
		return NULL;
	return pContext->GetObject<COleDbManager>(&CLoginContext::GetOleDbMng);
}

//------------------------------------------------------------------------------
SqlConnection* AfxGetDefaultSqlConnection()
{ 
	COleDbManager *pOleDB = AfxGetOleDbMng();
	return pOleDB ? pOleDB->GetDefaultSqlConnection() : NULL; 
}     

TB_EXPORT void AfxSetDefaultSqlConnection(SqlConnection* pSqlConnection)
{
	COleDbManager *pOleDB = AfxGetOleDbMng();
	return pOleDB ? pOleDB->SetDefaultSqlConnection(pSqlConnection) : NULL;
}

//------------------------------------------------------------------------------
TB_EXPORT SqlConnection* AfxGetSecondarySqlConnection(const CString& strAlias)
{
	COleDbManager *pOleDB = AfxGetOleDbMng();
	return pOleDB ? pOleDB->GetSecondarySqlConnection(strAlias) : NULL;
}

//------------------------------------------------------------------------------
///<summary>
/// Get default sql connection
///</summary>
//[TBWebMethod(name=GetConnection)]
SqlSession* AfxGetDefaultSqlSession()
{ 
	COleDbManager *pOleDB = AfxGetOleDbMng();
	return pOleDB ? pOleDB->GetDefaultSqlSession() : NULL; 
}   

//------------------------------------------------------------------------------
///<summary>
/// Create new sql connection
///</summary>
//[TBWebMethod(name=OpenConnection)]
SqlSession* AfxOpenSqlSession(DataStr connectionString)
{ 
	COleDbManager *pOleDB = AfxGetOleDbMng();
	LPCWSTR szConnectionString = T2W((LPTSTR) ((LPCTSTR)(connectionString.GetString())));
	SqlConnection* pConn = pOleDB->GetNewConnection(szConnectionString);
	return pConn ? pConn->GetDefaultSqlSession() : NULL;
}   

//------------------------------------------------------------------------------
///<summary>
/// Create record by table name
///</summary>
//[TBWebMethod(name=CreateRecord)]
SqlRecord* AfxCreateRecord(DataStr tableName)
{ 
	const SqlCatalogEntry* pSqlCatalogEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(tableName);
	if (!pSqlCatalogEntry)
	{
			//ASSERT(FALSE);
			return NULL;
	}
	SqlRecord* pRec = pSqlCatalogEntry->CreateRecord();
	ASSERT_VALID(pRec);
	return pRec;
}   



//----------------------------------------------------------------------------
CLockManagerInterface* AFXAPI AfxGetLockManager()
{
	return (CLockManagerInterface*)AfxGetDefaultSqlConnection()->GetLockManagerInterface();
}

//----------------------------------------------------------------------------
CLockManagerInterface* AFXAPI AfxCreateLockManager()
{
	return AfxGetLockManager();
}