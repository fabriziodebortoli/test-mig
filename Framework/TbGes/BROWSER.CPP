
#include "stdafx.h"

#include <TbGeneric\array.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\LocalizableObjs.h>

#include <TbGenlib\SettingsTableManager.h>

#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>	
#include <TbOledb\sqlconnect.h>	

#include "browser.h"
#include "dbt.h"
#include "extdoc.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// Display Error Message 
//-----------------------------------------------------------------------------	
void MessageBox(const CString & strMsg, SqlException* e = NULL)
{
    CString msg;
    
    if (e) msg += _T("\n")+ e->m_strError;
	AfxMessageBox (msg, MB_OK | MB_ICONSTOP);
}

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC (SqlBrowser, CObject)

//-----------------------------------------------------------------------------	
SqlBrowser::SqlBrowser(DBTMaster* pDBTMaster)
	:
	m_pDBTMaster	(pDBTMaster),
	m_pRecord		(NULL),
	m_pTable		(NULL),
	//m_pRecordKeySet	(NULL),
	m_bInvalidQuery	(TRUE),
	m_bFirstRecord	(FALSE),
	m_bLastRecord	(FALSE),
	m_bValid		(TRUE),
	m_bNoRecord		(TRUE)
{
	m_pRecord	= m_pDBTMaster->GetRecord()->Create();

	m_pSqlSession = m_pDBTMaster->GetDocument()->GetSqlConnection()->GetNewSqlSession();

	m_pTable	= new SqlTable(m_pRecord, m_pSqlSession);

	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szFormsSection, szDisplayBrowsingLimits, DataBool(m_bPingPong), szTbDefaultSettingFileName);
	m_bPingPong = pSetting ? *((DataBool*) pSetting) : FALSE;
	m_pTable->Open(FALSE, E_KEYSET_CURSOR); 
	m_pTable->EnablePaging(10); //al momento è cablato

	m_pDBTMaster->PrepareBrowser(m_pTable);
}

//-----------------------------------------------------------------------------	
SqlBrowser::~SqlBrowser()
{
	ASSERT(m_pRecord);
	ASSERT(m_pTable);

	if (m_pTable && m_pTable->IsOpen())
		m_pTable->Close();
			
	SAFE_DELETE (m_pTable);
	SAFE_DELETE (m_pRecord);
	
	SAFE_DELETE (m_pSqlSession);
}

// controlla di non essere sul primo record
//-----------------------------------------------------------------------------
void SqlBrowser::Initialize(MoveType moveType, BOOL bCopyBuffer /* = TRUE*/)
{
	TRY
	{
		if (!m_bValid)
			return;

		//m_pTable->MoveLast();
		
		m_bFirstRecord	= (moveType == E_MOVE_NEXT) ? FALSE : !m_pTable->IsBOF();
		m_bLastRecord	= (moveType == E_MOVE_NEXT) ? !m_pTable->IsEOF() : FALSE;
		m_bNoRecord		= (moveType == E_MOVE_NEXT) ? !m_bLastRecord : !m_bFirstRecord;

		// cerca di capire se c'e un solo record		
		if (!m_bNoRecord)
			(moveType == E_MOVE_NEXT) ? CheckFirstRecord() : CheckLastRecord();
	
		// se la tabella e` vuota non assegna il record permettendo
		// un nuovo inserimento.
		if (bCopyBuffer && !m_bNoRecord)
			*(m_pDBTMaster->GetRecord()) = *m_pRecord;
	}
	CATCH(SqlException, e)
	{
		MessageBox(_TB("Browser: Last failed."), e);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlBrowser::ReInit (BOOL bCopyBuffer/*=TRUE*/)
{
	if (m_pTable && m_pTable->IsOpen())
		m_pTable->Close();

	SAFE_DELETE (m_pTable);
	
	m_bInvalidQuery	= TRUE;
	m_bFirstRecord	= FALSE;
	m_bLastRecord	= FALSE;
	m_bValid		= TRUE;
	m_bNoRecord		= TRUE;

	m_pTable	= new SqlTable(m_pRecord, m_pSqlSession);

	m_pTable->Open(FALSE, E_KEYSET_CURSOR); 

	m_pDBTMaster->PrepareBrowser(m_pTable);

	Initialize(E_MOVE_LAST, bCopyBuffer);
}

// controlla di non essere sul primo record
//-----------------------------------------------------------------------------
void SqlBrowser::CheckFirstRecord()
{
	if (!m_bPingPong)
		return;

	TRY
	{
		if (!m_bValid)
			return;

		m_pTable->MovePrev();                  
		if (m_pTable->IsBOF())
		{
			m_bFirstRecord = TRUE;
			m_pTable->MoveFirst();
			return;
		}
	
		m_bFirstRecord = FALSE;
		m_pTable->MoveNext();
	}
	CATCH(SqlException, e)
	{
		MessageBox(_TB("Browser: Prev failed."), e);
	}
	END_CATCH
}

// controlla di non essere sull'ultimo record
//-----------------------------------------------------------------------------
void SqlBrowser::CheckLastRecord()
{
	if (!m_bPingPong)
		return;

	TRY
	{
		if (!m_bValid)
			return;

		m_pTable->MoveNext();                  
		if (m_pTable->IsEOF())
		{
			m_bLastRecord = TRUE;
			m_pTable->MoveLast();
			return;
		}
	
		m_bLastRecord = FALSE;
		m_pTable->MovePrev();
	}
	CATCH(SqlException, e)
	{
		MessageBox(_TB("Browser: Next failed."), e);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
BOOL SqlBrowser::CanDoFirstRecord() const
{
	return CanDoPrevRecord();
}

//-----------------------------------------------------------------------------
BOOL SqlBrowser::CanDoPrevRecord() const
{
	return 
		m_bValid &&
		(!m_bPingPong ||
		m_bInvalidQuery || 
		(!m_bInvalidQuery && !m_bNoRecord && !m_bFirstRecord));
}

// enable if opened and >= 1 records present
//-----------------------------------------------------------------------------
BOOL SqlBrowser::CanDoNextRecord() const
{
	return 
		m_bValid &&
		(!m_bPingPong ||
		m_bInvalidQuery || 
		(!m_bInvalidQuery && !m_bNoRecord && !m_bLastRecord));
}

//-----------------------------------------------------------------------------
BOOL SqlBrowser::CanDoLastRecord() const
{
	return CanDoNextRecord();
}

//-----------------------------------------------------------------------------	
void SqlBrowser::SetNoRecord ()
{
	m_bFirstRecord	= FALSE;
	m_bLastRecord	= FALSE;
	m_bNoRecord		= TRUE;
}

//-----------------------------------------------------------------------------	
BOOL SqlBrowser::FirstRecord ()
{
		
	TRY
	{	
		if (m_bInvalidQuery)
		{
			if (!Query(E_MOVE_FIRST))
				return FALSE;
		}
		else
			m_pTable->MoveFirst();

		m_bFirstRecord = TRUE;

		if (m_pTable->IsEmpty())
		{
			SetNoRecord();
			return FALSE;
		}
		m_bNoRecord = FALSE;
		CheckLastRecord();		
	}
	CATCH(SqlException, e)
	{
		MessageBox(_TB("Browser: First failed."), e);
		return FALSE;
	}
	END_CATCH

	ASSERT(
		m_pDBTMaster->m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pRecord->GetRuntimeClass())
		||
		m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pDBTMaster->m_pRecord->GetRuntimeClass())
		);

	*(m_pDBTMaster->GetRecord()) = *m_pRecord;
	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL SqlBrowser::PrevRecord	() 
{
	if (m_bInvalidQuery && !Query(E_MOVE_LAST))
		return FALSE;

	TRY
	{
		m_pTable->MovePrev();
		if (m_pTable->IsEmpty())
		{
			SetNoRecord();
			return FALSE;
		}

		// If we moved on top file, then move to first record
		if (m_pTable->IsBOF())
		{
			m_pTable->MoveFirst();
			m_bFirstRecord	= TRUE;
	    }
	    
		CheckFirstRecord();
		CheckLastRecord();
	}
	CATCH(SqlException, e)
	{
		MessageBox(_TB("Browser: Prev failed."), e);
		return FALSE;
	}
	END_CATCH
	
	ASSERT(
		m_pDBTMaster->m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pRecord->GetRuntimeClass())
		||
		m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pDBTMaster->m_pRecord->GetRuntimeClass())
		);

	*(m_pDBTMaster->GetRecord()) = *m_pRecord;
	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL SqlBrowser::NextRecord ()
{
	TRY
	{

		if (m_bInvalidQuery)
		{
			if (!Query(E_MOVE_LAST))
				return FALSE;
		}
		else
			m_pTable->MoveNext();

		if (m_pTable->IsEmpty())
		{
			SetNoRecord();
			return FALSE;
		}

		// If we moved off the end of file, then move back to last record
		if (m_pTable->IsEOF())
		{
			m_pTable->MoveLast();
			if (m_pTable->IsEmpty())
				Query();
			else
				m_bLastRecord = TRUE;
        }
	    
		CheckLastRecord();
		CheckFirstRecord();
	}
	CATCH(SqlException, e)
	{
		MessageBox(_TB("Browser: Next failed."), e);
		return FALSE;
	}
	END_CATCH

	ASSERT(
		m_pDBTMaster->m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pRecord->GetRuntimeClass())
		||
		m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pDBTMaster->m_pRecord->GetRuntimeClass())
		);

	ASSERT(
		m_pDBTMaster->m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pRecord->GetRuntimeClass())
		||
		m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pDBTMaster->m_pRecord->GetRuntimeClass())
		);

	*(m_pDBTMaster->GetRecord()) = *m_pRecord;
	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL SqlBrowser::LastRecord ()
{
	TRY
	{	
		if (m_bInvalidQuery)
		{
			if (!Query(E_MOVE_LAST))
				return FALSE;
		}
		else
			m_pTable->MoveLast();

		m_bLastRecord = TRUE;
		if (m_pTable->IsEmpty())
		{
			SetNoRecord();
			return FALSE;
		}
		m_bNoRecord = FALSE;	
		CheckFirstRecord();
	}
	CATCH(SqlException, e)
	{
		MessageBox(_TB("Browser: Last failed."), e);
		return FALSE;
	}
	END_CATCH
	
	ASSERT(
		m_pDBTMaster->m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pRecord->GetRuntimeClass())
		||
		m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pDBTMaster->m_pRecord->GetRuntimeClass())
		);

	*(m_pDBTMaster->GetRecord()) = *m_pRecord;
	return TRUE;
}

//-----------------------------------------------------------------------------	
/*void SqlBrowser::LoadRecordKeySet()
{
	if (m_pRecordKeySet)
		m_pRecordKeySet->RemoveAll();
	else
		m_pRecordKeySet = new CRecordKeySet;
	
	DataObjArray* pKeys = NULL;
	while (!m_pTable->IsEOF())
	{
		pKeys = DataObjArray;
		m_pRecord->GetKeyStream(pKeys);
		m_pRecordKeySet->Add(pKeys);
	}
}*/

//-----------------------------------------------------------------------------	
BOOL SqlBrowser::Query(MoveType moveType /*= SqlTable::E_MOVE_NEXT*/)
{
	TRY
	{
		if (!m_bValid)
			return FALSE;

		m_pSqlSession->Open();

		m_pTable->Query(moveType);
		Initialize(moveType);
		m_bInvalidQuery = FALSE;

		//dopo la query mi disconnetto
		m_pSqlSession->Close();

		// se lavoro disconnesso allora mi creo l'array delle chiavi
		//if (m_pTable->m_pSqlConnection->m_bWorkOffline) 
		//	LoadRecordKeySet();

		return TRUE;
	}
	CATCH(SqlException, e)	
	{
		MessageBox(_TB("Browser: Requery failed."), e);
		m_bValid = FALSE;
		return FALSE;
	}
	END_CATCH
}

//-----------------------------------------------------------------------------	
void SqlBrowser::SetInvalidQuery (const BOOL bValidQuery)
{
	m_bInvalidQuery = bValidQuery;
	
	// forza la Query perche` potrebbe non essere toccato alcun bottone
	// di browse e allora lo stato non sarebbe piu` coerente
	/*if (m_bInvalidQuery && m_pTable->IsEmpty())
		Query(E_MOVE_LAST);*/
}

//-----------------------------------------------------------------------------	
BOOL SqlBrowser::IsEmpty() const
{
	return m_pTable->IsEOF() && m_pTable->IsBOF();
}

//-----------------------------------------------------------------------------	
BOOL SqlBrowser::NoCurrent() const 
{
	return 
		!m_bValid ||
		m_bNoRecord || m_pTable->IsEOF() || 
		m_pTable->IsBOF() || m_pTable->IsDeleted();
}
