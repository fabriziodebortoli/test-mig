
#include "stdafx.h"

#include <TbClientCore\ClientObjects.h>

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\TbNamespaces.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\LocalizableObjs.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbWebServicesWrappers\LockManagerInterface.h>

#include <TbGenlibUI\SettingsTableManager.h>
#include <TbGenlib\generic.h>

#include <TbGenlib\baseapp.h>

#include "Sqltable.h" 
#include "oledbmng.h"
#include "sqlobject.h"
#include "sqltable.h"
#include "sqlconnect.h"

#include "Sqllock.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szKeySep[]			= _T(":");

/////////////////////////////////////////////////////////////////////////////
//						class SqlLockCachedRecord
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class SqlCachedLock : public CObject
{
	friend class SqlLockMng;

private:
	CString			m_sLockContextKey;
	CStringArray	m_arLocks;
	CObject*		m_pAddress;

private:
	SqlCachedLock	(const CString& sLockContextKey, const CString& sLockKey, CObject* pAddress);

private:
	BOOL Add			(const CString& sLockKey);
	BOOL Remove			(const CString& sLockKey);
	BOOL IsLockCached	(const CString& sLockKey);

	const BOOL IsEmpty () { return m_arLocks.GetSize() == 0; }
};
	
//-----------------------------------------------------------------------------
SqlCachedLock::SqlCachedLock (const CString& sLockContextKey, const CString &sLockKey, CObject* pAddress)
{
	m_sLockContextKey	= sLockContextKey;
	m_pAddress			= pAddress;

	m_arLocks.Add (sLockKey);
}

//-----------------------------------------------------------------------------
BOOL SqlCachedLock::Add  (const CString& sLockKey)
{
	return m_arLocks.Add (sLockKey) > 0;
}

//-----------------------------------------------------------------------------
BOOL SqlCachedLock::Remove (const CString& sLockKey)
{
	for (int i= m_arLocks.GetUpperBound(); i >= 0; i--)
	{
		if (m_arLocks.GetAt(i).CompareNoCase(sLockKey) != 0)
			continue;

		m_arLocks.RemoveAt(i);
		return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL SqlCachedLock::IsLockCached (const CString& sLockKey)
{
	for (int i= m_arLocks.GetUpperBound(); i >= 0; i--)
		if (m_arLocks.GetAt(i).CompareNoCase(sLockKey) == 0)
			return TRUE;

	return FALSE;
}

/////////////////////////////////////////////////////////////////////////////
//						class LockRetriesMng
/////////////////////////////////////////////////////////////////////////////
//

//-----------------------------------------------------------------------------
void EnableDocumentFrame(CBaseDocument* pDocument, BOOL bEnable)
{
	if (!pDocument || !AfxIsValidAddress(pDocument, sizeof(CBaseDocument)))
		return;
	
	POSITION pos = pDocument->GetFirstViewPosition();
	ASSERT (pos);

	CView* pView = pDocument->GetNextView(pos);
	ASSERT (pView);	if (!pView)	return;

	CFrameWnd* pFrame = pView->GetParentFrame();
	ASSERT (pFrame); if (!pFrame) return;

	pFrame->EnableWindow(bEnable);
}

//-----------------------------------------------------------------------------
LockRetriesMng::LockRetriesMng (BOOL bUseMessageBox /*FALSE*/, BOOL bUseCustomSettings /*FALSE*/) 
	:
	m_pContext (NULL)
{ 
	m_bUseMessageBox		= bUseMessageBox; 
	m_bUseCustomSettings	= bUseCustomSettings;
}

//-----------------------------------------------------------------------------
BOOL LockRetriesMng::AttachContext (CBaseContext* pContext)
{
	m_pContext = pContext; 

	ASSERT(m_pContext);
	
	if (!m_pContext)
	{
		ASSERT_TRACE(FALSE, _T("Missing context for LockRetriesMng object!"));
		return FALSE;
	}
	
	if (!m_pContext->GetDocument() || !AfxIsValidAddress(m_pContext->GetDocument(), sizeof(CBaseDocument)))
		return FALSE;

	// settings has been chagned by developer
	if (m_bUseCustomSettings)
		return TRUE;

	// la temporizzazione per i documenti batch e` diversa
	if (m_pContext->GetDocument()->GetType() == VMT_BATCH || !m_bUseMessageBox)
	{	
		m_nLockRetry = 0;
		DataObj* pSetting = AfxGetSettingValue(snsTbOleDb, szLockManager, szDisableBatchLockRetry, DataBool(m_bDisabled), szTbDefaultSettingFileName);
		m_bDisabled	 = pSetting ? *((DataBool*) pSetting) : FALSE;

		pSetting = AfxGetSettingValue(snsTbOleDb, szLockManager, szDisableBatchBeep, DataBool(m_bBeepDisabled), szTbDefaultSettingFileName);
		m_bBeepDisabled	=  pSetting ? *((DataBool*) pSetting) : FALSE;
		
		pSetting = AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxBatchLockRetry, DataInt(m_nMaxLockRetry), szTbDefaultSettingFileName);
		m_nMaxLockRetry	= pSetting ? *((DataInt*) pSetting) : 8;

		pSetting = AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxBatchLockTime, DataInt(m_nMaxLockTime), szTbDefaultSettingFileName);
		m_nMaxLockTime	= pSetting ? *((DataInt*) pSetting) : 3000;
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void LockRetriesMng::EnableDocumentFrame(BOOL bEnable)
{
	if (m_pContext && AfxIsValidAddress(m_pContext, sizeof(CBaseContext)))
		::EnableDocumentFrame(m_pContext->GetDocument(), bEnable);
}

/////////////////////////////////////////////////////////////////////////////
//						class SqlLockMng
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
//-----------------------------------------------------------------------------
SqlLockMng::SqlLockMng(const CString &strDBName)
:
	m_strDBName			(strDBName),
	m_pCurrentTable		(NULL),
	m_bLocksCacheEnabled(FALSE)
{	
	m_pLockManagerInterface		= AfxGetLockManager();
	m_pLoginManagerInterface	= AfxGetLoginManager();
}

//-----------------------------------------------------------------------------
SqlLockMng::~SqlLockMng()
{
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::RemoveAllLocks(CBaseContext* pContext)
{
	if (UnlockAll(pContext))
	{
		// alllocks removal will invalidate all cache
		ClearLocksCache();
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void SqlLockMng::SetKey(const DataObj* pkey, CString& strLockKey)
{
	int nKeyLen = strLockKey.GetLength();
	
	CString str;

	if (!pkey)
	{
		str = "X";
	} 
	else
	{
		switch (pkey->GetDataType().m_wType)
		{
			case DATA_STR_TYPE	:	str = pkey->Str();	break;
			case DATA_INT_TYPE	:   str = cwsprintf(_T("%0d"),	(int)(*((const DataInt*)pkey)));		break;
			case DATA_LNG_TYPE	:   str = cwsprintf(_T("%0ld"),	(long)(*((const DataLng*)pkey)));		break;
			case DATA_DATE_TYPE	:   str = cwsprintf(_T("%s"),	(LPCTSTR) pkey->Str());					break;
			case DATA_BOOL_TYPE	:   str = cwsprintf(_T("%s"),	(LPCTSTR) pkey->Str().Left(1));			break;
			case DATA_ENUM_TYPE	:	str = cwsprintf(_T("%0u"),	((const DataEnum*)pkey)->GetValue());	break;
			case DATA_GUID_TYPE	:   str = cwsprintf(_T("%s"), 	(LPCTSTR) pkey->Str());					break;
		}
	} 
	
	if (nKeyLen) 
		strLockKey += szKeySep;

	strLockKey +=  str;
}

//-----------------------------------------------------------------------------
void SqlLockMng::SetKey(const SqlRecord* pRec, CString& strLockKey)
{
	for (int i=0; i < pRec->GetNumberSpecialColumns(); i++)
	{
		SetKey(pRec->GetSpecialColumn(i)->GetDataObj(), strLockKey);
	}
}

//-----------------------------------------------------------------------------
CString SqlLockMng::InitLockKey(SqlTable* pTable)
{	
	CString strLockKey;

	if (!pTable || !AfxIsValidAddress(pTable, sizeof(SqlTable)))
		return strLockKey;

	
	// determina dalle special columns la composizione della chiave da
	// usare per fare il lock della risorsa. Si basa sui dati correntemente
	// presenti nel record della tabella
	//
	if (pTable->m_pRecord && AfxIsValidAddress(pTable->m_pRecord, sizeof(SqlRecord)))
	{
		for (int i = 0; i <= pTable->m_pRecord->GetUpperBound(); i++)
		{
			SqlRecordItem* pItem = pTable->m_pRecord->GetAt(i);
			if (pItem->m_pColumnInfo->m_bSpecial)
				SetKey(pItem->m_pDataObj, strLockKey);
		}
	}
	return strLockKey;
}

//-----------------------------------------------------------------------------
CString SqlLockMng::GetLockKeyDescription(SqlTable* pTable)
{	
	CString strLockKey;

	if (!pTable || !AfxIsValidAddress(pTable, sizeof(SqlTable)) || !pTable->GetRecord())
		return _T("");

	return _T("\r\n") + pTable->GetRecord()->GetPrimaryKeyDescription();  
	
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::GetLockEntry(SqlTable* pTable)
{
	if (	
			m_strCurrentKey.IsEmpty()		||
			!pTable							||
			m_strTableName.IsEmpty()||
			!pTable->m_pContext				
		)
	{
		ASSERT(FALSE);
		TRACE("SqlLockMng::GetLockEntry: no lock is possible!\n");
		return FALSE;
	}

	CString strLockKeyDescription = GetLockKeyDescription(pTable);
	if (strLockKeyDescription.IsEmpty())
		strLockKeyDescription = m_strCurrentKey;

	BOOL bOk = m_pLockManagerInterface->LockCurrent(m_strDBName, m_strTableName, m_strCurrentKey, cwsprintf(_T("%lp"), pTable->m_pContext), m_strLockMsg, strLockKeyDescription);

	TRACE_SQL(cwsprintf ( (bOk) ? _T("Lock LockCurrent %s %s %s %lp successfully done!") : _T("Lock: LockCurrent %s %s %s %lp failed!"), m_strDBName, m_strTableName, m_strCurrentKey, pTable->m_pContext), pTable);
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::LockCurrent(SqlTable* pTable, BOOL bUseMessageBox /*=TRUE*/, LockRetriesMng* pRetriesMng /*NULL*/)
{
	LockRetriesMng* pMng = pRetriesMng ? pRetriesMng : new LockRetriesMng(bUseMessageBox);
	BOOL bLocked = LockCurrent
			(
				pTable->GetTableName(), 
				pTable, 
				pMng
			);

	if (!pRetriesMng)
		delete pMng;
	
	return bLocked;
}


//-----------------------------------------------------------------------------
BOOL SqlLockMng::ExecuteLock(LockRetriesMng* pRetriesMng)
{
	BOOL bLock = TRUE;

	if (!m_pCurrentTable || m_strCurrentKey.IsEmpty())
	{
		TRACE_SQL(cwsprintf(_T("Lock LockRecord record key empty. Table {0-%s) No lock available!"),  m_strCurrentKey), m_pCurrentTable);		
		return FALSE;
	}

	int nPhantomLockRetry = 0;
	CBaseContext* pContext = m_pCurrentTable->m_pContext;
	LockRetriesMng* pLockRetriesMng = pRetriesMng ? pRetriesMng : new LockRetriesMng(!pContext->GetDiagnostic()->IsUnattendedMode());
	BOOL bRetryInit = pLockRetriesMng->AttachContext(pContext);

	while (TRUE)
	{
		m_strLockMsg.Empty();
		// bLock = TRUE se sono riuscita ad effettuare il lock oppure é un mio lock
		// FALSE altrimenti
		bLock = GetLockEntry(m_pCurrentTable);
	
		if (bLock)
		{
			if (pContext && AfxIsValidAddress(pContext, sizeof(CBaseContext)))
			{
				pContext->m_bLocked = TRUE;
				if (pContext->IsValidDocument())
					pContext->m_pDocument->m_bRetryingLock = FALSE;
				}
			else 
				bLock = FALSE;
			break;
		}
		else
		{
			if (
					pContext && 
					AfxIsValidAddress(pContext, sizeof(CBaseContext)) &&
					pContext->IsValidDocument()
				)
				pContext->m_pDocument->m_bRetryingLock = TRUE;

			if (!bRetryInit) break;
			
			// custom lock message
			if (!pLockRetriesMng->m_bUseCustomSettings || pLockRetriesMng->m_strMsg.IsEmpty())
				pLockRetriesMng->m_strMsg =  m_strLockMsg + _T("\n");

			// prova a fare una dormitina se posso
			if (!pLockRetriesMng->Wait(pLockRetriesMng->m_bUseMessageBox && !pContext->GetDiagnostic()->IsUnattendedMode()))
			{
				// event viewer trace has been disabled
				DataObj* pSetting = AfxGetSettingValue	(
															snsTbOleDb,
															szConnectionSection, 
															szEnableEventViewerLog, 
															DataBool(FALSE), 
															szTbDefaultSettingFileName
														);
				if (pSetting && pSetting->GetDataType() == DATA_BOOL_TYPE && *((DataBool*) pSetting))
					WriteEventViewerMessage(FormatAbortedLockLogMessage(pLockRetriesMng->m_strMsg), EVENTLOG_WARNING_TYPE);
				break;	
			}
			
			// l'utente mi ha detto di continuare	
			TRACE_SQL(cwsprintf(_T("Lock LockRecord {0-%s} of {1-%s} table retry lock!"), m_strCurrentKey, m_strTableName), m_pCurrentTable);	
			continue;
		}

		// il lock e` misteriosamente scomparso (rilasciato dopo la mia query)
		// allora ci riprova per un numero massimo di volte
		if (nPhantomLockRetry++ > 100)
		{
			TraceError(cwsprintf(_TB("Unable to reserve data in the table {0-%s}"), m_strTableName));
			break;
		}
	}
	
	if	(
			pContext && 
			AfxIsValidAddress(pContext, sizeof(CBaseContext)) &&
			pContext->IsValidDocument()
		)
		pContext->m_pDocument->m_bRetryingLock = FALSE;

	

	if (!pRetriesMng)
		delete pLockRetriesMng;
	
	return bLock;
}


//-----------------------------------------------------------------------------
BOOL SqlLockMng::LockCurrent(const CString& sLockContextKey, SqlTable* pTable, LockRetriesMng* pRetriesMng)
{
	BOOL bLock = TRUE;
	if (!pTable || !pTable->m_pContext || pTable->GetTableName().IsEmpty())
	{
		ASSERT(FALSE);
		TRACE("SqlLockMng::LockCurrent: table without context. No lock available.\n");
		return FALSE;
	}

	if (IsCachedTableKeyLocked(sLockContextKey, pTable, pTable->GetRecord()))
	{
		TRACE_SQL(cwsprintf(_T("Lock IsCachedTableKeyLocked. LockContextkey {0-%s} Table {1-%s)"),  sLockContextKey, pTable->GetTableName()), pTable);		
		return TRUE;
	}

	m_strCurrentKey.Empty();
	m_strCurrentKey = InitLockKey(pTable);
	m_pCurrentTable = pTable;
	m_strTableName = pTable->GetTableName();

	bLock = ExecuteLock(pRetriesMng);
	// bool flag avoid to insert keys in cache. All the other
	// operations can be performed as cache is emtpy
	if (bLock && m_bLocksCacheEnabled)
		InsertInLocksCache(sLockContextKey, m_strCurrentKey, m_pCurrentTable->m_pContext);

	return bLock;	
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::UnlockCurrent(SqlTable* pTable)
{

	return UnlockCurrent (pTable->GetTableName(), pTable);
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::UnlockCurrent(const CString& sLockContextKey, SqlTable* pTable)
{
	if (	
			!pTable							||
			pTable->GetTableName().IsEmpty()||
			!pTable->m_pContext
		)
		return TRUE;
	
	CString strLockKey = (m_pCurrentTable != pTable || m_strCurrentKey.IsEmpty()) 
						? InitLockKey(pTable) 
						: m_strCurrentKey;

	if (strLockKey.IsEmpty())
		return TRUE;

	BOOL bOk = m_pLockManagerInterface->UnlockCurrent(m_strDBName, pTable->GetTableName(), strLockKey, cwsprintf(_T("%lp"), pTable->m_pContext));

	if (bOk)
	{
		RemoveFromLocksCache(sLockContextKey, strLockKey);
	}

	TRACE_SQL(cwsprintf( (bOk) ? _T("Lock UnlockCurrent %s %s %s %lp sucessfully done!") : _T("Lock: UnlockCurrent %s %s %s %lp failed!"),  m_strDBName, pTable->GetTableName(), m_strCurrentKey, pTable->m_pContext), pTable);

	return bOk;
}

	//table lock
//-----------------------------------------------------------------------------
BOOL SqlLockMng::LockTable(SqlTable* pTable, const CString& tableName, BOOL bUseMessageBox, LockRetriesMng* pRetriesMng)
{
	BOOL bLock = TRUE;
	if (!pTable || !pTable->m_pContext || tableName.IsEmpty())
	{
		ASSERT(FALSE);
		TRACE("SqlLockMng::LockCurrent: table without context. No lock available.\n");
		return FALSE;
	}

	m_strTableName = tableName;
	m_strCurrentKey = tableName;
	m_pCurrentTable = pTable;

	LockRetriesMng* pMng = pRetriesMng ? pRetriesMng : new LockRetriesMng(bUseMessageBox);
	bLock = ExecuteLock(pMng);

	if (!pRetriesMng)
		delete pMng;

	return bLock;
}


//-----------------------------------------------------------------------------
BOOL SqlLockMng::UnlockTable(SqlTable* pTable, const CString& tableName)
{
	if (	
			!pTable							||
			tableName.IsEmpty()||
			!pTable->m_pContext
		)
		return TRUE;
	
	CString strLockKey = (m_pCurrentTable != pTable || m_strCurrentKey.IsEmpty()) 
						? tableName 
						: m_strCurrentKey;

	if (strLockKey.IsEmpty())
		return TRUE;

	BOOL bOk = m_pLockManagerInterface->UnlockCurrent(m_strDBName, tableName, strLockKey, cwsprintf(_T("%lp"), pTable->m_pContext));
	
	TRACE_SQL(cwsprintf( (bOk) ? _T("Lock UnlockCurrent %s %s %s %lp sucessfully done!") : _T("Lock: UnlockCurrent %s %s %s %lp failed!"),  m_strDBName, tableName, m_strCurrentKey, pTable->m_pContext), pTable);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::IsCurrentLocked(SqlTable* pTable)
{
	if (	
			!pTable							||
			pTable->GetTableName().IsEmpty()||
			!pTable->m_pContext
		)
	{
		ASSERT(FALSE);
		TRACE("SqlLockMng::IsCurrentLocked: no lock possible. Invalid information\n");
		return FALSE;
	}

	CString strLockKey = InitLockKey(pTable);
	if (strLockKey.IsEmpty())
	{
		TRACE("SqlLockMng::IsCurrentLocked: no lock possible. The record key has empty value\n");
		return FALSE;
	}

	return m_pLockManagerInterface->IsCurrentLocked(
		m_strDBName,
		pTable->GetTableName(), 
		strLockKey, 
		cwsprintf(_T("%lp"), pTable->m_pContext));
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::IsMyLock(SqlTable* pTable)
{
	if (	
			!pTable							||
			pTable->GetTableName().IsEmpty()||
			!pTable->m_pContext
		)
	{
		ASSERT(FALSE);
		TRACE("SqlLockMng::IsMyLock: no lock possible. Invalid information\n");
		return FALSE;
	}

	CString strLockKey = InitLockKey(pTable);
	if (strLockKey.IsEmpty())
	{
		TRACE("SqlLockMng::IsMyLock: no lock possible. The record key has empty value\n");
		return FALSE;
	}

	return m_pLockManagerInterface->IsMyLock(
		m_strDBName,
		pTable->GetTableName(), 
		strLockKey, 
		cwsprintf(_T("%lp"), pTable->m_pContext));
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::UnlockAll(CBaseContext* pContext)
{
	if (!pContext || !AfxIsValidAddress(pContext, sizeof(CBaseContext)))
	{
		ASSERT(FALSE);
		TRACE("SqlLockMng::UnlockAll: no lock possible. Invalid information\n");
		return FALSE;
	}
	// solo il proprietario del contesto puó effettuare un lockall
	if (!pContext->m_bOwnedContext)
		return TRUE;
	
	if (m_pLockManagerInterface->UnlockAllContext(m_strDBName, cwsprintf(_T("%lp"), pContext)))
	{
		ClearLocksCache();
		pContext->m_bLocked = FALSE;
		//UnlockAll viene chiamato anche nel distruttore di COleDbManager (che deriva da SqlObject)
		//peccato che TRACE_SQL chiami al suo interno pOleDbManager->DebugTraceSQL
		//non si tromba ma viene dato ASSERT
		//TRACE_SQL(cwsprintf(_T("Unlock all %s %lp"),m_strDBName, pContext), NULL);
	}

	return !pContext->m_bLocked;
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::UnlockAll(CBaseContext* pContext, LPCTSTR szTableName)
{
	if (!pContext || !AfxIsValidAddress(pContext, sizeof(CBaseContext)))
	{
		ASSERT(FALSE);
		TRACE("SqlLockMng::UnlockAll: no lock possible. Invalid information\n");
		return FALSE;
	}
			
	if (m_pLockManagerInterface->UnlockAll(m_strDBName, cwsprintf(_T("%lp"), pContext), szTableName))
	{
		ClearLocksCache();
		TRACE_SQL(cwsprintf(_T("Lock Unlock all %s %lp %s"),m_strDBName, pContext, szTableName), NULL);
		return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::LockDocument(CBaseContext* pBaseContext)
{
	m_strLockMsg.Empty();
		
	if (!pBaseContext || !pBaseContext->IsValidDocument())
	{
		ASSERT(FALSE);
		TRACE("SqlLockMng::LockDocument: document not valid. No lock available.\n");
		return FALSE;
	}

	BOOL bOk =  m_pLockManagerInterface->LockDocument
		(
			m_strDBName, 
			pBaseContext->GetDocument()->GetNamespace().ToString(), 
			cwsprintf(_T("%lp"), pBaseContext), 
			m_strLockMsg
		);

	if (!bOk)
		pBaseContext->AddMessage(m_strLockMsg);
	
	return bOk;

}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::UnlockDocument(CBaseContext* pBaseContext)
{
	if (!pBaseContext || !pBaseContext->IsValidDocument())
	{
		ASSERT(FALSE);
		TRACE("SqlLockMng::UnlockDocument: document not valid. No lock available.\n");
		return FALSE;
	}


	return m_pLockManagerInterface->UnlockDocument
		(
			m_strDBName, 
			pBaseContext->GetDocument()->GetNamespace().ToString(), 
			cwsprintf(_T("%lp"),pBaseContext)
		);
}



//-----------------------------------------------------------------------------
CString SqlLockMng::GetLockMessage(SqlTable* pTable)
{
	if (!pTable || pTable != m_pCurrentTable)
		return _T(""); 
	
	return m_strLockMsg;
}

//-----------------------------------------------------------------------------
void SqlLockMng::EnableLocksCache (const BOOL bValue /*TRUE*/)
{
	m_bLocksCacheEnabled = bValue;
}

//-----------------------------------------------------------------------------
void SqlLockMng::InsertInLocksCache (const CString& sLockContextKey, const CString& sLockKey, CObject* pAddress)
{
	SqlCachedLock* pCachedLock;
	if (!m_LocksCache.Lookup(sLockContextKey, (CObject*&) pCachedLock) || !pCachedLock)
		m_LocksCache [sLockContextKey] = new SqlCachedLock (sLockContextKey, sLockKey, pAddress);
	else
		pCachedLock->Add(sLockKey);
}

//-----------------------------------------------------------------------------
void SqlLockMng::RemoveFromLocksCache (const CString& sLockContextKey, const CString& sLockKey)
{
	SqlCachedLock* pCachedLock;
	if (!m_LocksCache.Lookup(sLockContextKey, (CObject*&) pCachedLock) || !pCachedLock)
		return;

	pCachedLock->Remove(sLockKey);
	if (pCachedLock->IsEmpty())
	{
		delete pCachedLock;
		m_LocksCache.RemoveKey(sLockContextKey);
	}
}

//-----------------------------------------------------------------------------
void SqlLockMng::ClearLocksCache(const CString sLockContextKey /*_T("")*/)
{
	POSITION		pos;
	CString			strKey;
	SqlCachedLock*	pElem;

	for (pos = m_LocksCache.GetStartPosition(); pos != NULL;)
	{
		m_LocksCache.GetNextAssoc(pos, strKey, (CObject*&)pElem);

		if (pElem &&
				(
					sLockContextKey.IsEmpty() || 
					pElem->m_sLockContextKey.CompareNoCase(sLockContextKey) == 0
				)
			)
		{
			m_LocksCache.RemoveKey(pElem->m_sLockContextKey);
			delete pElem;
		}
	}
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::UnlockTableKey (SqlTable* pTable, SqlRecord* pRec)
{
	return UnlockTableKey(pTable->GetTableName(), pTable, pRec);
}

// Unlock all the keys related to a single context key
//-----------------------------------------------------------------------------
BOOL SqlLockMng::UnlockAllLockContextKeys (const CString& sLockContextKey)
{
	SqlCachedLock*	pCachedLock;
	if (!m_LocksCache.Lookup(sLockContextKey, (CObject*&) pCachedLock) || !pCachedLock)
		return FALSE;

	BOOL bAllUnlocked = TRUE;
	for (int i=pCachedLock->m_arLocks.GetUpperBound(); i >= 0; i--)
	{
		if (m_pLockManagerInterface->UnlockCurrent(m_strDBName, sLockContextKey, pCachedLock->m_arLocks.GetAt(i), cwsprintf(_T("%lp"), pCachedLock->m_pAddress)))
			pCachedLock->Remove(pCachedLock->m_arLocks.GetAt(i));
		else
			bAllUnlocked = FALSE;
	}

	if (bAllUnlocked && pCachedLock->IsEmpty())
	{
		delete pCachedLock;
		m_LocksCache.RemoveKey(sLockContextKey);
	}
	
	return bAllUnlocked;
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::UnlockTableKey (const CString& sLockContextKey, SqlTable* pTable, SqlRecord* pRec)
{
	SqlRecord* pSaveRecord = pTable->GetRecord();
	CString sCurrentKey = m_strCurrentKey;

	pTable->m_pRecord = pRec;
	m_strCurrentKey.Empty();

	BOOL bOk = UnlockCurrent (sLockContextKey, pTable);

	pTable->m_pRecord = pSaveRecord;
	m_strCurrentKey = sCurrentKey;
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::LockTableKey (SqlTable* pTable, SqlRecord* pRec, LockRetriesMng* pRetriesMng /*NULL*/)
{
	return LockTableKey(pTable->GetTableName(), pTable, pRec, pRetriesMng);
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::LockTableKey (const CString& sLockContextKey, SqlTable* pTable, SqlRecord* pRec, LockRetriesMng* pRetriesMng /*NULL*/)
{
	SqlRecord* pSaveRecord = pTable->GetRecord();
	CString sCurrentKey = m_strCurrentKey;

	pTable->m_pRecord = pRec;
	m_strCurrentKey.Empty();

	BOOL bOk = LockCurrent (sLockContextKey, pTable, pRetriesMng);

	pTable->m_pRecord = pSaveRecord;
	m_strCurrentKey = sCurrentKey;
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::IsTableKeyLocked (const CString& sLockContextKey, SqlTable* pTable, SqlRecord* pRec)
{
	BOOL bLocked = IsCachedTableKeyLocked(sLockContextKey, pTable, pRec);
	if (bLocked)
		return TRUE;

	SqlRecord* pSaveRecord = pTable->GetRecord();
	CString sCurrentKey = m_strCurrentKey;

	pTable->m_pRecord = pRec;
	m_strCurrentKey.Empty();

	BOOL bOk = FALSE;
	InitLockKey(pTable);

	bLocked = IsCurrentLocked(pTable);
	pTable->m_pRecord = pSaveRecord;
	m_strCurrentKey = sCurrentKey;
	return bLocked;
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::IsCachedTableKeyLocked (const CString& sLockContextKey, SqlTable* pTable, SqlRecord* pRec)
{
	// in caso di cache disabilitata demanda il controllo
	// al metodo del LockManager
	if (!m_bLocksCacheEnabled)
		return FALSE;

	SqlRecord* pSaveRecord = pTable->GetRecord();
	CString sCurrentKey = m_strCurrentKey;

	pTable->m_pRecord = pRec;
	m_strCurrentKey.Empty();

	BOOL bOk = FALSE;
	InitLockKey(pTable);

	SqlCachedLock* pCachedLock;
	if (m_LocksCache.Lookup(sLockContextKey, (CObject*&) pCachedLock) && pCachedLock && pCachedLock->IsLockCached(m_strCurrentKey))
		bOk = TRUE;

	pTable->m_pRecord = pSaveRecord;
	m_strCurrentKey = sCurrentKey;
	
	return bOk;
}

//-----------------------------------------------------------------------------
CString SqlLockMng::FormatAbortedLockLogMessage (const CString& sCurrentLockMessage)
{
	return cwsprintf
		(
				_TB("\r\nTransaction has been aborted by the user in %s document due to a record lock.\r\nThe user connected at abort time was %s on the client %s. \r\nThe current lock message thrown by the application was:\r\n\"%s\""), 
				m_pCurrentTable  && m_pCurrentTable->m_pContext && m_pCurrentTable->m_pContext->m_pDocument ? 
				m_pCurrentTable->m_pContext->m_pDocument->GetNamespace().ToUnparsedString() :
				_TB("Unknown"),
			AfxGetLoginInfos()->m_strUserName,
			GetComputerName(TRUE),
			sCurrentLockMessage
		);
}

//-----------------------------------------------------------------------------
BOOL SqlLockMng::HasRestarted () const
{
	return m_pLockManagerInterface ? m_pLockManagerInterface->HasRestarted() : FALSE;
}