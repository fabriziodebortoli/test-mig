#include "stdafx.h"

#include <TbOleDb\SqlAccessor.h>
#include <TbOleDb\Sqltable.h>

#include "HotFilter.h"
#include "HotFilterDataPicker.h"
#include "UIHotFilterDataPicker.h"
#include "DataPickerRecordSet.h"

#ifdef _DEBUG
#undef THIS_FILE                                                        
static char THIS_FILE[] = __FILE__;     
#endif                                

//////////////////////////////////////////////////////////////////////////////
//       			class DataPickerRecordSet
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(DataPickerRecordSet, RecordArray)

//-----------------------------------------------------------------------------	
DataPickerRecordSet::DataPickerRecordSet
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	m_pDocument			(pDocument),
	m_nExtractedRows	(0),
	m_nLastBufferedRow	(-1),
	m_nCurrentCursorPos	(-1)
{
	m_pRecord = (SqlRecord*)pClass->CreateObject();
	m_pTable = new SqlTable(m_pRecord, m_pDocument->GetReadOnlySqlSession());
}

//-----------------------------------------------------------------------------	
DataPickerRecordSet::~DataPickerRecordSet()
{
	if (m_pTable->IsOpen())
		m_pTable->Close();

	SAFE_DELETE(m_pTable);
	SAFE_DELETE(m_pRecord);
}

//-----------------------------------------------------------------------------	
void DataPickerRecordSet::Init()
{
	m_pRecord->Init();
	RemoveAll();
	if (GetDocument()->m_pTBGridControl)
		GetDocument()->m_pTBGridControl->ClearRowsCount();
}

//-----------------------------------------------------------------------------	
void DataPickerRecordSet::ClearGrid()
{
	Init();
	m_SelectedItems.RemoveAll();
	m_UnselectedItems.RemoveAll();
}

//-----------------------------------------------------------------------------	
void DataPickerRecordSet::Execute(BOOL bPreselect)
{
	Init();
	Close();
	Open();
	FindFirstData();
	if (GetDocument()->m_pTBGridControl)
		GetDocument()->m_pTBGridControl->SetRowsCount(m_nExtractedRows);
}

//-----------------------------------------------------------------------------	
BOOL DataPickerRecordSet::Open()
{
	ASSERT(m_pTable);
	ASSERT_VALID(m_pTable);
	ASSERT(!m_pTable->IsOpen());

	if (m_pTable->IsOpen())
		return FALSE;

	BOOL bOk = FALSE;
	TRY
	{
		m_pTable->Open(FALSE, TRUE); // need a scrollable cursor

		OnDefineQuery();
	}
	CATCH(SqlException, e)
	{
		m_pDocument->GetMessages()->Add(e->m_strError);
		return FALSE;
	}
	END_CATCH
	
	return TRUE;
}

//-----------------------------------------------------------------------------	
void DataPickerRecordSet::Close()
{
	if (m_pTable && m_pTable->IsOpen())
		m_pTable->Close();
}

//---------------------------------------------------------------------------------------------------------
void DataPickerRecordSet::OnDefineQuery()
{
	GetDocument()->GetHotFilter()->OnDefineDataPickerParamsQuery(_T(""), m_pTable, m_pTable->GetRecord());
	if (!m_pTable->IsSelectEmpty())
		return;

	// by default, sort on linked column
	// @@ TODO permettere sort esterno
	if (GetDocument()->GetHotFilter()->m_nHKLLinkedColumnIdx != -1)
		m_pTable->AddSortColumn(m_pTable->GetRecord()->GetColumnName(GetDocument()->GetHotFilter()->m_nHKLLinkedColumnIdx));

	if (GetDocument()->m_pTBGridControl)
	{
		// provides a minimum select with all the bound columns belonging to the main record extracted
		for (int i = 0; i <= GetDocument()->m_pTBGridControl->GetColumnCount() - 1; i++)
		{
			int idx = m_pTable->GetRecord()->GetIndexFromDataObj(GetDocument()->m_pTBGridControl->GetColumnInfo(i)->GetDataObj());
			if (idx != -1 && !m_pTable->GetRecord()->IsVirtual(idx))
				m_pTable->Select(m_pTable->GetRecord()->GetDataObjAt(idx));
		}
	}
	else
		m_pTable->SelectAll();
}

//---------------------------------------------------------------------------------------------------------
void DataPickerRecordSet::OnPrepareQuery()
{
	GetDocument()->GetHotFilter()->OnPrepareDataPickerParamsQuery(_T(""), m_pTable);
}

//---------------------------------------------------------------------------------------------------------
void DataPickerRecordSet::OnPrepareAuxColumns(SqlRecord* pRec)
{
	if (GetDocument()->GetSelected(pRec))
	{
		*(GetDocument()->GetSelected(pRec)) = GetDocument()->m_bSelectAll;

		DataObj* pLinked = pRec->GetDataObjAt(GetDocument()->m_pHotFilter->m_nHKLLinkedColumnIdx);
		if (m_SelectedItems.Find(pLinked) != -1)
			*GetDocument()->GetSelected(pRec) = TRUE;
		if (m_UnselectedItems.Find(pLinked) != -1)
			*GetDocument()->GetSelected(pRec) = FALSE;
	}
}

//-----------------------------------------------------------------------------	
BOOL DataPickerRecordSet::FindFirstData()
{
	ASSERT(m_pTable);

	OnPrepareQuery();

	BOOL bOk = FALSE;

	TRY
	{
		m_pTable->Query();

		m_nExtractedRows	= m_pTable->GetRowSetCount();
		m_nLastBufferedRow	= -1;
		m_nCurrentCursorPos = -1;

		// read ahead
		if (!m_pTable->IsEOF())
		{
			m_nLastBufferedRow	= 0;
			m_nCurrentCursorPos = 0;
			SetRecord(m_nCurrentCursorPos);
		}

		while	(
					!m_pTable->IsEOF() &&
					m_nLastBufferedRow < BUFFER_SIZE - 1
				)
		{
			m_pTable->MoveNext();
			m_nLastBufferedRow++;
			m_nCurrentCursorPos++;
			SetRecord(m_nCurrentCursorPos);
		}
		bOk = TRUE;
	}
	CATCH(SqlException, e)
	{
		GetDocument()->GetMessages()->Add(cwsprintf(_TB("DataPickerRecordSet: Query on table {0-%s} failed.\n{1-%s}"), m_pRecord->GetTableName(), e->m_strError));
	}
	END_CATCH

	return bOk && !m_pTable->IsEmpty();
}

//-----------------------------------------------------------------------------	
SqlRecord*	DataPickerRecordSet::GetVirtualRow(LONG nRow)
{
	return GetRow(nRow % BUFFER_SIZE);
}

//-----------------------------------------------------------------------------	
BOOL DataPickerRecordSet::FindData(LONG nRow)
{
	if	(
			nRow <= m_nLastBufferedRow &&
			nRow > m_nLastBufferedRow - BUFFER_SIZE 
		)
		return TRUE; // hit inside the buffer

	// shifts the buffer up
	if (nRow <= m_nLastBufferedRow - BUFFER_SIZE)
		return FindDataUp(nRow);

	// shifts the buffer down
	return FindDataDown(nRow);
}

//-----------------------------------------------------------------------------	
BOOL DataPickerRecordSet::FindDataDown(LONG nRow)
{
	TBTRACE0("cache DOWN");

	BOOL bOk = FALSE;
	TRY
	{
		// fast forward
		while	(
					!m_pTable->IsEOF() &&
					m_nCurrentCursorPos < m_nLastBufferedRow
				)
		{
			m_pTable->MoveNext();
			m_nCurrentCursorPos++;
		}

		while	(
					!m_pTable->IsEOF() &&
					m_nLastBufferedRow < nRow
				)
		{
			m_pTable->MoveNext();
			m_nLastBufferedRow++;
			m_nCurrentCursorPos++;
			SetRecord(m_nCurrentCursorPos % BUFFER_SIZE);
		}
		bOk = TRUE;
	}
	CATCH(SqlException, e)
	{
		GetDocument()->GetMessages()->Add(cwsprintf(_TB("DataPickerRecordSet: Query on table {0-%s} failed.\n{1-%s}"), m_pRecord->GetTableName(), e->m_strError));
	}
	END_CATCH

	return bOk;
}

//-----------------------------------------------------------------------------	
BOOL DataPickerRecordSet::FindDataUp(LONG nRow)
{
	TBTRACE0("cache UP");

	BOOL bOk = FALSE;
	TRY
	{
		// rewind
		while	(
					!m_pTable->IsBOF() &&
					m_nCurrentCursorPos > m_nLastBufferedRow - BUFFER_SIZE + 1
				)
		{
			m_pTable->MovePrev();
			m_nCurrentCursorPos--;
		}

		// fetch
		while	(
					!m_pTable->IsBOF() &&
					nRow < m_nCurrentCursorPos  
				)
		{
			m_pTable->MovePrev();
			m_nLastBufferedRow--;
			m_nCurrentCursorPos--;
			SetRecord(m_nCurrentCursorPos % BUFFER_SIZE);
		}

		bOk = TRUE;
	}
	CATCH(SqlException, e)
	{
		GetDocument()->GetMessages()->Add(cwsprintf(_TB("DataPickerRecordSet: Query on table {0-%s} failed.\n{1-%s}"), m_pRecord->GetTableName(), e->m_strError));
	}
	END_CATCH

	return bOk;
}

//-----------------------------------------------------------------------------	
void DataPickerRecordSet::SetRecord(int nPos)
{
	SqlRecord* pRec = NULL;
	if (nPos > GetUpperBound())
	{
		pRec = m_pRecord->Create();
		pRec->SetConnection(m_pRecord->GetConnection());
		Add(pRec);
	}
	else pRec = GetAt(nPos);

	*pRec = *m_pRecord;

	pRec->CopyAttribute(m_pRecord);

	OnPrepareAuxColumns(pRec);
}

//-----------------------------------------------------------------------------
BOOL DataPickerRecordSet::SelectDeselectAllLines()
{
	if (m_nExtractedRows == 0)
		return FALSE;

	GetDocument()->m_bSelectAll = !GetDocument()->m_bSelectAll;

	GetDocument()->m_nSelectedRows = GetDocument()->m_bSelectAll ? m_nExtractedRows : 0;

	m_SelectedItems.RemoveAll();
	m_UnselectedItems.RemoveAll();

	return GetDocument()->m_bSelectAll;
}	

//-----------------------------------------------------------------------------
void DataPickerRecordSet::GetSelectedItems(DataObjArray& selectedItems, DataObjArray& unselectedItems)
{
	selectedItems.RemoveAll();
	unselectedItems.RemoveAll();

	for (int i = 0; i <= m_SelectedItems.GetUpperBound(); i++)
		selectedItems.Add(m_SelectedItems.GetAt(i)->Clone());
	for (int i = 0; i <= m_UnselectedItems.GetUpperBound(); i++)
		unselectedItems.Add(m_UnselectedItems.GetAt(i)->Clone());
}

//----------------------------------------------------------------------------------
void DataPickerRecordSet::ChangeStatus(int nRow, BOOL bSelected /*= FALSE*/)
{
	ASSERT_TRACE(nRow >  m_nLastBufferedRow - BUFFER_SIZE && nRow <= m_nLastBufferedRow, "Cannot change status of an unbuffered row");
	if (nRow <=  m_nLastBufferedRow - BUFFER_SIZE || nRow > m_nLastBufferedRow)
		return;

	DataObj* pDataObj = GetVirtualRow(nRow)->GetDataObjAt(GetDocument()->GetHotFilter()->m_nHKLLinkedColumnIdx)->Clone();

	if (GetDocument()->m_bSelectAll)
	{
		if (!bSelected)
			m_UnselectedItems.Add(pDataObj);
		else
		{
			int pos = m_UnselectedItems.Find(pDataObj);
			if (pos != -1)
				m_UnselectedItems.RemoveAt(pos);
			delete pDataObj;
		}
	}
	else
	{
		if (bSelected)
			m_SelectedItems.Add(pDataObj);
		else
		{
			int pos = m_SelectedItems.Find(pDataObj);
			if (pos != -1)
				m_SelectedItems.RemoveAt(pos);
			delete pDataObj;
		}
	}

	GetDocument()->m_nSelectedRows += bSelected? +1 : -1;
	*GetDocument()->GetSelected(GetVirtualRow(nRow)) = bSelected;
}

//-----------------------------------------------------------------------------
void DataPickerRecordSet::SetPreselectedItems(DataObjArray& selectedItems, DataObjArray& unselectedItems)
{
	m_SelectedItems.RemoveAll();
	m_UnselectedItems.RemoveAll();
	for (int i = 0; i <= selectedItems.GetUpperBound(); i++)
		m_SelectedItems.Add(selectedItems.GetAt(i)->Clone());
	for (int i = 0; i <= unselectedItems.GetUpperBound(); i++)
		m_UnselectedItems.Add(unselectedItems.GetAt(i)->Clone());
}
