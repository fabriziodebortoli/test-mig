#include "stdafx.h"

#include "TBGridControl.h"
#include "TBGridControlDataSource.h"
#include "Dbt.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

////////////////////////////////////////////////////////////////////////////////
//				class TBGridControlDataSource implementation
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
TBGridControlDataSource::TBGridControlDataSource(CTBGridControl* pTBGridControl)
	:
	m_pTBGridControl(pTBGridControl)
{
	ASSERT(m_pTBGridControl);
}

////////////////////////////////////////////////////////////////////////////////
//				class DBTSlaveBufferedGridDataSource implementation
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
DBTSlaveBufferedGridDataSource::DBTSlaveBufferedGridDataSource(DBTSlaveBuffered* pDBT, CTBGridControl* pTBGridControl)
	:
	TBGridControlDataSource	(pTBGridControl),
	m_pDBT					(pDBT)
{
	ASSERT(pDBT);
}

//-----------------------------------------------------------------------------
SqlRecord* DBTSlaveBufferedGridDataSource::GetPrototypeRecord()
{
	return m_pDBT->GetRecord();
}

//-----------------------------------------------------------------------------
void DBTSlaveBufferedGridDataSource::Reload()
{
	if (!m_pDBT->IsModified())
		return;

	for (int nRow = 0; nRow < m_pDBT->GetRowCount(); nRow++)
		m_pTBGridControl->AddRow(m_pDBT->GetRecords()->GetAt(nRow));
}

////////////////////////////////////////////////////////////////////////////////
//				class SqlTableGridDataSource implementation
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
SqlTableGridDataSource::SqlTableGridDataSource(SqlTable* pSqlTable, CTBGridControl* pTBGridControl)
	:
	m_pSqlTable				(pSqlTable),
	TBGridControlDataSource	(pTBGridControl)
{
	ASSERT(m_pSqlTable);
}

//-----------------------------------------------------------------------------
SqlRecord* SqlTableGridDataSource::GetPrototypeRecord()
{
	return m_pSqlTable->GetRecord();
}

//-----------------------------------------------------------------------------
void SqlTableGridDataSource::Reload()
{
	bool mementoClose = false;

	if (!m_pSqlTable->IsOpen())
	{
		m_pSqlTable->Open();
		mementoClose = TRUE;
	}

	if (m_pSqlTable->IsPreQueryState())
	{
		m_pSqlTable->Query();
	}
	
	while (!m_pSqlTable->IsEOF())
	{
		m_pTBGridControl->AddRow(m_pSqlTable->GetRecord());

		m_pSqlTable->MoveNext();
	}

	if (mementoClose)
	{
		m_pSqlTable->Close();
	}
}

////////////////////////////////////////////////////////////////////////////////
//				class RecordArrayGridDataSource implementation
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
RecordArrayGridDataSource::RecordArrayGridDataSource(RecordArray* pRecordArray, CTBGridControl* pTBGridControl)
	:
	m_pRecordArray			(pRecordArray),
	TBGridControlDataSource	(pTBGridControl)
{
	ASSERT(m_pRecordArray);
	ASSERT(m_pRecordArray->GetPrototype() != NULL);
}

//-----------------------------------------------------------------------------
SqlRecord* RecordArrayGridDataSource::GetPrototypeRecord()
{
	return m_pRecordArray->GetPrototype();
}

//-----------------------------------------------------------------------------
SqlRecord* RecordArrayGridDataSource::GetRecordAt(int nRow)	
{ 
	if (nRow < 0|| nRow >  m_pRecordArray->GetUpperBound())
	return NULL; 

	return m_pRecordArray->GetAt(nRow);
}


//-----------------------------------------------------------------------------
void RecordArrayGridDataSource::Reload()
{
	if (!m_pTBGridControl->IsVirtualMode())
		for (int i = 0; i < m_pRecordArray->GetCount(); i++)
			m_pTBGridControl->AddRow(m_pRecordArray->GetAt(i));
}