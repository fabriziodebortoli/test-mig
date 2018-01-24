
#include "stdafx.h"

#include <TbGenlib\messages.h>

#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>				

// local declaration
#include "tblupdat.h"
#include "extdoc.h"


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//							classe TableUpdater
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC (TableUpdater, CObject)
                                     
//-----------------------------------------------------------------------------	
TableUpdater::TableUpdater 	
	(
			CRuntimeClass* 		pClass, 
			CBaseDocument* 		pDocument,	// = NULL
			CDiagnostic* 		pDiagnostic	// = NULL
	)
	:
	m_pDocument			(pDocument),
	m_pTable			(NULL),
	m_pSqlSession		(NULL),
	m_Status			(NONE),
	m_bForceAutocommit	(FALSE),
	m_bOnlyOneRecord	(TRUE),
	m_pDiagnostic		(pDiagnostic)
{
	// Mi creo un SqlRecord dove salvare il vecchio record
	// per passarlo all'update ed evitare errori nel  caso
	// di cambiamento di chiave primaria	
	m_pRecord	 = (SqlRecord*) pClass->CreateObject();
	m_pOldRecord = (SqlRecord*) pClass->CreateObject();	

	if (!m_pDiagnostic && (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc))))
		m_pDiagnostic = ((CAbstractFormDoc*)m_pDocument)->m_pMessages;
}

//-----------------------------------------------------------------------------	
TableUpdater::TableUpdater 	
	(
			const CString&	sTableName, 
			CBaseDocument* 	pDocument,	// = NULL
			CDiagnostic* 	pDiagnostic	// = NULL
	)
	:
	m_pDocument			(pDocument),
	m_pTable			(NULL),
	m_pSqlSession		(NULL),
	m_Status			(NONE),
	m_bForceAutocommit	(FALSE),
	m_bOnlyOneRecord	(TRUE),
	m_pDiagnostic		(pDiagnostic)
{
	// Mi creo un SqlRecord dove salvare il vecchio record
	// per passarlo all'update ed evitare errori nel  caso
	// di cambiamento di chiave primaria	
	m_pRecord	 = new SqlRecord (sTableName);
	m_pOldRecord = new SqlRecord (sTableName);

	if (!m_pDiagnostic && (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc))))
		m_pDiagnostic = ((CAbstractFormDoc*)m_pDocument)->m_pMessages;
}

//-----------------------------------------------------------------------------
void TableUpdater::Free()
{   
	if (m_pTable)
	{
		if (m_pTable->IsOpen()) 
			m_pTable->Close();

		delete m_pTable;
	}

	if (m_pRecord)
		delete m_pRecord;

	if (m_pOldRecord)
		delete m_pOldRecord;

	m_pRecord = NULL;
	m_pOldRecord = NULL;
	m_pTable = NULL;
}

//-----------------------------------------------------------------------------
BOOL TableUpdater::LockCurrent (BOOL bUseMessageBox /*=TRUE*/, LockRetriesMng* pRetriesMng /*NULL*/)
{ 
	BOOL bOk = m_pTable->LockCurrent(bUseMessageBox, pRetriesMng);
	
	if (!bOk)
		m_Status = LOCKED;
	
	return bOk;
}

//------------------------------------------------------------------------------
void TableUpdater::SetSqlSession(SqlSession* pSession)		
{ 
	m_pSqlSession = pSession; 
	if (!m_pDiagnostic)
		m_pDiagnostic = m_pSqlSession->m_pContext->GetDiagnostic();
}

//------------------------------------------------------------------------------
void TableUpdater::SetAutocommit()
{ 
	m_bForceAutocommit = TRUE; 
	if (m_pTable) m_pTable->SetAutocommit();
}

// Deve essere stato abilitato il lock connettendo il TableUpdater con il 
// documento altrimenti il lock avviene con documento NULL cioe' lock globale
// anche per chi lancia il lock
//------------------------------------------------------------------------------
TableUpdater::FindResult TableUpdater::FindRecord(BOOL bLock, LockRetriesMng* pRetriesMng /*NULL*/)
{
	m_Status = NONE;

    TRY
    {
	    // In caso di dato di find senza valore non effettua la find
		if	(IsEmptyQuery())
		{
			m_pRecord->Init();
			return m_Status;
		}
		// istanzio il cursore
		// se il programmatore non mi ha passata nessuna sessione di lavoro (mediante la SetSqlSession)
		// allora utilizzo la UpadatableSqlSession dell'eventuale documento 
		// altrimenti se non esiste il documento errore
		if (!TableCreated())
		{
			ASSERT(FALSE);
			return NONE;
		}

		if (!m_pTable->IsOpen())
		{
			m_pTable->Open(TRUE, E_FAST_FORWARD_ONLY);
			OnDefineQuery();
		}

		// valorizza i parametri per la query ad accesso diretto
		OnPrepareQuery();
		m_pTable->Query();

		// mi salvo di dati letti per poi riutilizzarne i criteri di estrazione 
		*m_pOldRecord = *m_pRecord;

		m_Status = m_pTable->IsEmpty() ? NOT_FOUND : FOUND;
	
		BOOL bUseMessageBox = (m_pDocument)
							? !m_pDocument->IsInUnattendedMode()
							: !m_pSqlSession->m_pContext->GetDiagnostic()->IsUnattendedMode();

		if (m_Status == FOUND && bLock)
		{
			if (pRetriesMng)
				pRetriesMng->m_bUseMessageBox = bUseMessageBox;

			if (!LockCurrent(bUseMessageBox, pRetriesMng) && !bUseMessageBox && m_pDiagnostic)
				m_pDiagnostic->Add(m_pTable->GetLockMessage());
		}		
	}
	CATCH(SqlException, e)	
	{
		if (m_pSqlSession)
			m_pSqlSession->ShowMessage(e->m_strError);

		if (m_pTable)
		{
			if (m_pTable->IsOpen()) 
				m_pTable->Close();

			delete m_pTable;
			m_pTable = NULL;
		}
		m_pRecord->Init();
		return NONE;
	}
	END_CATCH

	return m_Status;
}

//-----------------------------------------------------------------------------	
BOOL TableUpdater::UpdateRecord(BOOL bForceTBModified /*=FALSE*/)
{
	BOOL bResult = FALSE;

    TRY
    {
		if (!m_pTable || !m_pTable->IsOpen())
		{
			ASSERT(FALSE);
			TRACE0("TableUpdater::UpdateRecord(): used with no FindRecord call\n");
			return bResult;
		}

		// Il record e' locked non si puo' aggiornare
		if (m_Status == LOCKED)
		{
			ASSERT(FALSE);
			TRACE0("TableUpdater::UpdateRecord(): the record is locked\n");
			return bResult;
		}
	
    	// inibisce l'inizializzazione del record in caso di AddNew per non fumare
    	// i dati modificati dal programmataore dipo la FindRecord.
		if (m_pTable->IsEmpty()) m_pTable->AddNew(FALSE); else m_pTable->Edit();

		// l'old deve  arrivare
		// a questo punto buono
		ASSERT(m_pOldRecord);

		m_pTable->Update(m_pOldRecord, bForceTBModified);
		
	/*	if (m_bOnlyOneRecord)
			m_pTable->Disconnect();*/

	    bResult = TRUE;
	}
	CATCH(SqlException, e)	
	{
		if	(m_pDiagnostic)
		{                       
			CString strMsg = cwsprintf(_TB("Error updating table {0-%s}."), (LPCTSTR)m_pRecord->GetTableName());
			m_pDiagnostic->Add(strMsg);
			m_pDiagnostic->Add(e->m_strError);
		}
		else
			AfxMessageBox (e->m_strError);
		return FALSE;
	}
	END_CATCH
	
    return bResult;
}

//-----------------------------------------------------------------------------	
BOOL TableUpdater::DeleteRecord()
{
	BOOL bResult = FALSE;

    TRY
    {
		if (!m_pTable || !m_pTable->IsOpen())
		{
			ASSERT(FALSE);
			TRACE0("TableUpdater::DeleteRecord() used with no FindRecord call\n");
			return bResult;
		}

		// Il record e' locked non si puo' aggiornare
		if (m_Status == LOCKED)
		{
			ASSERT(FALSE);
			TRACE0("TableUpdater::UpdateRecord(): the record is locked\n");
			return bResult;
		}
	
		ASSERT(!m_pTable->IsEmpty());

		// l'old deve  arrivare
		// a questo punto buono
		ASSERT(m_pOldRecord);

		m_pTable->Delete(m_pOldRecord);
		
		/*if (m_bOnlyOneRecord)
			m_pTable->Disconnect();
	*/    
	    bResult = TRUE;
	}
	CATCH(SqlException, e)	
	{
		CString strMsg = cwsprintf(_TB("Error deleting a record in table {0-%s}."), (LPCTSTR)m_pRecord->GetTableName());
			
		if	(m_pDiagnostic)
		{                       
			m_pDiagnostic->Add(strMsg);
			m_pDiagnostic->Add(e->m_strError);
		}
		else
			AfxMessageBox (e->m_strError);
		return FALSE;
	}
	END_CATCH
	
    return bResult;
}

//------------------------------------------------------------------------------------
BOOL TableUpdater::TableCreated()
{
	if (m_pTable)
		return TRUE;

	if (m_pDocument && !m_pSqlSession)
		m_pSqlSession =	m_pDocument->GetUpdatableSqlSession();
	else
	{
		if (!m_pSqlSession)
		{
			ASSERT(FALSE);
			return FALSE;
		}
	}

	m_pTable = new SqlTable(m_pRecord, m_pSqlSession);
	m_pTable->SetOnlyOneRecordExpected(m_bOnlyOneRecord);
	m_pOldRecord->SetConnection(m_pRecord->GetConnection());
	if (m_bForceAutocommit)
		m_pTable->SetAutocommit();

	return TRUE;
}

//------------------------------------------------------------------------------------
BOOL TableUpdater::LockTableKey (SqlRecord* pRec, LockRetriesMng* pRetriesMng /*NULL*/)
{
	if (!TableCreated ())
		return FALSE;

	return m_pTable->m_pContext && m_pTable->m_pContext->LockTableKey(m_pTable, pRec, m_pTable->GetTableName(), pRetriesMng); 
}

/////////////////////////////////////////////////////////////////////////////
//							classe RowsetUpdater
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC (RowsetUpdater, TableUpdater)
