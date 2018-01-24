
#include "stdafx.h"

#include <TbGeneric\DataObj.h>
#include <TbGenlib\SettingsTableManager.h>
#include <TbGeneric\ParametersSections.h>

#include <TbOledb\oledbmng.h>
#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>				
#include <TbOledb\sqlconnect.h>	

#include "tblread.h"
#include "extdoc.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif


//=============================================================================
//					Class TableReader implementation
//=============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TableReader, CObject)

//-----------------------------------------------------------------------------
TableReader::TableReader (CRuntimeClass* pClass, CBaseDocument* pDocument)
	:
	m_pDocument	  (pDocument),
	m_pTable	  (NULL),
	m_pSqlSession (NULL),
	m_PrevResult  (NONE),
	m_bForceQuery (FALSE),
	m_bOnlyOneRecord (TRUE)
{
	m_pRecord = (SqlRecord*) pClass->CreateObject();	
}

//-----------------------------------------------------------------------------
TableReader::TableReader (const CString& sTableName, CBaseDocument* pDocument)
	:
	m_pDocument	  (pDocument),
	m_pTable	  (NULL),
	m_pSqlSession (NULL),
	m_PrevResult  (NONE),
	m_bForceQuery (FALSE),
	m_bOnlyOneRecord(TRUE)
{
	const CDbObjectDescription* pDescri = AfxGetDbObjectDescription(sTableName);
	if (!pDescri)
	{
		ASSERT(FALSE);
		TRACE1 ("Cannot find XML description for table %s. Cannot construct dynamic SqlRecord!!", sTableName);
	}

	m_pRecord = new SqlRecord(sTableName, NULL, pDescri->GetSqlRecType());	
}
	
//-----------------------------------------------------------------------------
TableReader::TableReader (CRuntimeClass* pClass, SqlSession* pSqlSession)
	:
	m_pDocument	  (NULL),
	m_pTable	  (NULL),
	m_pSqlSession (pSqlSession),
	m_PrevResult  (NONE),
	m_bForceQuery (FALSE),
	m_bOnlyOneRecord(TRUE)
{
	m_pRecord = (SqlRecord*) pClass->CreateObject();	
}

//-----------------------------------------------------------------------------
void TableReader::Free()
{   
	if (m_pTable)
	{
		ASSERT_VALID(m_pTable);
		// potrebbe non essere mai stata aperta, oppure chiusa d'ufficio
		// dalla tabella LRU
		if (m_pTable->IsOpen()) 
			m_pTable->Close();
	
		// Se e' ancora in LRU la rimuove
		delete m_pTable;
		m_pTable = NULL;
	}

	if (m_pRecord)
	{
		ASSERT_VALID(m_pRecord);
		delete m_pRecord;
		m_pRecord = NULL;
	}
}

// il controllo di non cercare dati nulli va fatto nella FindRecord
// della specifica implementazione, che conosce le condizioni di "dato nullo"
//------------------------------------------------------------------------------
TableReader::FindResult TableReader::FindRecord()
{
	FindResult aResult = NONE;
	
    TRY
    {
		// In caso di dato di find senza valore non effettua la find
		if	(IsEmptyQuery())
		{
			// devo rifare il settaggio dei parametri per gestire il confronto di query
			if (m_pTable && m_pTable->IsOpen()) 
				OnPrepareQuery();
				
			m_pRecord->Init();
			m_PrevResult = NONE;
			return m_PrevResult;
		}
		// ritardo la creazione del cursore e la sua apertura alla prima find
		if (!m_pTable)
		{
			if (m_pDocument && !m_pSqlSession)
				m_pSqlSession = m_pDocument->GetReadOnlySqlSession();

			if (!m_pSqlSession)
				m_pSqlSession = AfxGetDefaultSqlSession();

			m_pTable = new SqlTable(m_pRecord, m_pSqlSession, m_pDocument);					
			m_pTable->SetOnlyOneRecordExpected(m_bOnlyOneRecord);
		}

		if (!m_pTable->IsOpen())
		{
			m_pTable->Open(FALSE);
			OnDefineQuery();
			OnPrepareQuery();
		}
		else
		{
			OnPrepareQuery();
			// if some parameters are implicitly bound to context objects, need to refresh them
			// in order to let SameQuery work properly	
			//m_pTable->ValorizeContextBagParameters();
			if	(m_pTable->SameQuery() && !m_bForceQuery)
				return m_PrevResult;
		}
				
		// valorizza i parametri per la query ad accesso diretto
		//m_pTable->SetForceQuery(m_bForceQuery); // visto che sto utilizzando un'area di caching
												// se devo rieseguire la query, la rieseguo sulla tabella
		m_pTable->Query();
		m_PrevResult = aResult = m_pTable->IsEmpty() ? NOT_FOUND : FOUND;
	}
	CATCH(SqlException, e)	
	{
		if (m_pSqlSession)
			m_pSqlSession->ShowMessage(e->m_strError);

		if (m_pTable)
		{
			// potrebbe non essere mai stata aperta, oppure chiusa d'ufficio
			if (m_pTable->IsOpen()) 
				m_pTable->Close();
		}
		m_pRecord->Init();
		return NONE;
	}
	END_CATCH

	return aResult;
}

//=============================================================================
//					Class RowsetReader implementation
//=============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(RowsetReader, TableReader)

//=============================================================================
//					Class RecordReader implementation
//=============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(RecordReader, RowsetReader)

void RecordReader::OnDefineQuery()
{
	const CUIntArray& arKeys = m_pParamRec->GetPrimaryKeyIndexes ();
	int nk = arKeys.GetSize();
	if (nk == 0)
	{
		ASSERT(FALSE);
		m_bMissingKeys = TRUE;
		return;
	}

	ASSERT(m_pTable);

	for (int i = 0; i < nk; i++)
	{
		SqlRecordItem* pItem = m_pParamRec->GetAt(arKeys.GetAt(i));

		m_pTable->AddFilterColumn(pItem->GetColumnName());
		CString sParName = cwsprintf(_T("_key_seg%d"), i);
		DataObj* pKey = pItem->GetDataObj();
		if (!pKey || pKey->IsEmpty())
		{
			m_bMissingKeys = TRUE;
			ASSERT(m_pTable);
			if (!pKey) continue;
		}
		m_pTable->AddParam(sParName, *pKey);
		m_pTable->SetParamValue(sParName, *pKey);
	}

	m_pTable->SelectAll();
}
///////////////////////////////////////////////////////////////////////////////