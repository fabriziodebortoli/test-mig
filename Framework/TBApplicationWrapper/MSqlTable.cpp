#include "StdAfx.h"

#include <TbParser\Lexan.h>
#include <TbGeneric/DataObj.h>
#include <TbOleDb\SqlRec.h>
#include <TbOleDb\SqlTable.h>

#include "MSqlTable.h"
#include "MSqlRecord.h"

using namespace Microarea::Framework::TBApplicationWrapper;

using namespace System;
/////////////////////////////////////////////////////////////////////////////
// 				class MSqlTable implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
CRecordDataFinder::CRecordDataFinder ()
{ 
}

// per ottimizzare vado direttamente a farmi ritornare il SqlRecordItem
// che tiene tutte le informazioni: dataobj, name, type e allocsize
//-----------------------------------------------------------------------------
SqlRecordItem* CRecordDataFinder::GetRecordItem (SqlRecord* pRecord, DataObj* pDataObj)
{
	SqlRecordItem* pItem;
	int nSize = pRecord->GetExtensions() ? pRecord->GetSizeEx() : pRecord->GetSize();
	for (int i=0; i < nSize; i++)
	{
		pItem = pRecord->GetAt(i);
		if (pItem && pItem->GetDataObj() == pDataObj)
			return pItem;
	}

	return NULL;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MSqlTable implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
MSqlTable::MSqlTable (System::IntPtr sqlTablePtr, IRecord^ record)
{
	hasCodeBehind = true;
	m_ppSqlTable = new TDisposablePtr<SqlTable>();
	*m_ppSqlTable = (SqlTable*) sqlTablePtr.ToInt64();
	this->Record = record;
	InitStatement();
}

//----------------------------------------------------------------------------
MSqlTable::MSqlTable (System::IntPtr sqlTablePtr)
{
	hasCodeBehind = true;
	m_ppSqlTable = new TDisposablePtr<SqlTable>();
	*m_ppSqlTable = (SqlTable*) sqlTablePtr.ToInt64();
	Record = gcnew MSqlRecord(GetSqlTable()->GetRecord());
	InitStatement();
}

//----------------------------------------------------------------------------
MSqlTable::MSqlTable (MSqlRecord^ record)
{
	hasCodeBehind = false;
	Record = record;
	m_ppSqlTable = new TDisposablePtr<SqlTable>();
	*m_ppSqlTable = (SqlTable*) new SqlTable(record->GetSqlRecord(), AfxGetDefaultSqlSession());
	InitStatement();
}

//----------------------------------------------------------------------------
MSqlTable::MSqlTable (MSqlRecord^ record, System::IntPtr sqlSessionPtr)
{
	hasCodeBehind = false;
	Record = record;
	m_ppSqlTable = new TDisposablePtr<SqlTable>();
	*m_ppSqlTable = (SqlTable*) new SqlTable(record->GetSqlRecord(), (SqlSession*)sqlSessionPtr.ToInt64());
	InitStatement();
}

//----------------------------------------------------------------------------
void MSqlTable::InitStatement ()
{
	if (!GetSqlTable())
	{
		//TODO Diagnostic
		return;
	}
	select	= gcnew SelectStatement		(GetSqlTable());
	from	= gcnew FromStatement		(GetSqlTable());
	where	= gcnew WhereStatement		(GetSqlTable());
	orderBy = gcnew OrderByStatement	(GetSqlTable());
	groupBy = gcnew GroupByStatement	(GetSqlTable());
	having	= gcnew HavingStatement		(GetSqlTable());

	DataFinder = new CRecordDataFinder();
}

//-----------------------------------------------------------------------------
MSqlTable::~MSqlTable()
{
	this->!MSqlTable();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MSqlTable::!MSqlTable()
{
	if (!hasCodeBehind)
		delete GetSqlTable();

	SAFE_DELETE(m_ppSqlTable);
	// si occupa di cancellare gli oggetti C++
	DataFinder = NULL;
}


//-----------------------------------------------------------------------------
IRecord^ MSqlTable::Record::get ()
{
	return record;
}

//-----------------------------------------------------------------------------
void MSqlTable::Record::set (IRecord^ record)
{
	this->record = record;
}


void	MSqlTable::Open ()
{
	Open (false, false, true);
}

//-----------------------------------------------------------------------------
void	MSqlTable::Open (bool updatable)
{
	Open (updatable, false, true);
}

//-----------------------------------------------------------------------------
void	MSqlTable::Open (bool updatable, bool scrollable)
{
	Open (updatable, scrollable, true);
}

//-----------------------------------------------------------------------------
void	MSqlTable::Open (bool updatable, bool scrollable, bool sensitivity)
{
	if (GetSqlTable())
	{
		if (GetSqlTable()->IsOpen())
			GetSqlTable()->Close();

		GetSqlTable()->Open(updatable, scrollable, sensitivity);
	}
}

//-----------------------------------------------------------------------------
void	MSqlTable::Open (bool updatable, CursorType eCursorType)
{
	if (GetSqlTable())
	{
		if (GetSqlTable()->IsOpen())
			GetSqlTable()->Close();

		GetSqlTable()->Open(updatable, (::CursorType) eCursorType);
	}
}

//-----------------------------------------------------------------------------
void	MSqlTable::Close()
{
	if (GetSqlTable() && GetSqlTable()->IsOpen())
	{
		GetSqlTable()->Close();
	}
}

//-----------------------------------------------------------------------------
void	MSqlTable::NextResult()
{
	if (GetSqlTable())
	{
		GetSqlTable()->MoveNext();
	}
}

//-----------------------------------------------------------------------------
void	MSqlTable::PrevResult()
{
	if (GetSqlTable())
	{
		GetSqlTable()->MovePrev();
	}
}
	
//-----------------------------------------------------------------------------
void	MSqlTable::FirstResult()
{
	if (GetSqlTable())
	{
		GetSqlTable()->MoveFirst();
	}
}

//-----------------------------------------------------------------------------
void	MSqlTable::LastResult()
{
	if (GetSqlTable())
	{
		GetSqlTable()->MoveLast();
	}
}

//-----------------------------------------------------------------------------
void	MSqlTable::ExecuteQuery()
{
	if (GetSqlTable())
	{
		GetSqlTable()->Query();
	}
}

//-----------------------------------------------------------------------------
bool	MSqlTable::IsEOF()
{
	if (GetSqlTable())
	{
		return GetSqlTable()->IsEOF() == TRUE;
	}
	return false;
}

//-----------------------------------------------------------------------------
bool	MSqlTable::IsBOF()
{
	if (GetSqlTable())
	{
		return GetSqlTable()->IsBOF() == TRUE;
	}
	return false;
}

//-----------------------------------------------------------------------------
bool	MSqlTable::IsEmpty	()
{
	if (GetSqlTable())
	{
		return GetSqlTable()->IsEmpty() == TRUE;
	}
	return true;
}

//-----------------------------------------------------------------------------
void	MSqlTable::AddNew ()
{
	if (GetSqlTable())
	{
		GetSqlTable()->AddNew();
	}
}

	
//-----------------------------------------------------------------------------
void	MSqlTable::Edit   ()
{
	if (GetSqlTable())
	{
		GetSqlTable()->Edit();
	}
}
	
//-----------------------------------------------------------------------------
int MSqlTable::Update ()
{
	if (GetSqlTable())
	{
		return GetSqlTable()->Update();
	}
	else
	{
		return UPDATE_FAILED;
	}
}

//-----------------------------------------------------------------------------
int MSqlTable::Update (MSqlRecord^ oldRecord)
{
	if (GetSqlTable())
	{
		return GetSqlTable()->Update(oldRecord->GetSqlRecord());
	}
	else
	{
		return UPDATE_FAILED;
	}
}

//-----------------------------------------------------------------------------
void	MSqlTable::Delete ()
{
	if (GetSqlTable())
	{
		GetSqlTable()->Delete();
	}
}

//-----------------------------------------------------------------------------
void	MSqlTable::Delete	(MSqlRecord^ oldRecord)
{
	if (GetSqlTable())
	{
		GetSqlTable()->Delete(oldRecord->GetSqlRecord());
	}
}

//-----------------------------------------------------------------------------
bool	MSqlTable::LockCurrent()
{
	if (GetSqlTable())
	{
		return GetSqlTable()->LockCurrent() == TRUE;
	}
	return true; //in assenza di sql table rischio di bloccare la transazione se non ritorno True
}
//-----------------------------------------------------------------------------		
bool	MSqlTable::LockCurrent		(bool useMessageBox)
{
	if (GetSqlTable())
	{
		return GetSqlTable()->LockCurrent(useMessageBox) == TRUE;
	}
	return true; //in assenza di sql table rischio di bloccare la transazione se non ritorno True
}
//-----------------------------------------------------------------------------
bool	MSqlTable::UnlockCurrent	()
{
	if (GetSqlTable())
	{
		return GetSqlTable()->UnlockCurrent() == TRUE;
	}
	return true; //in assenza di sql table rischio di bloccare la transazione se non ritorno True
}
//-----------------------------------------------------------------------------
bool	MSqlTable::UnlockAll		()
{
	if (GetSqlTable())
	{
		return GetSqlTable()->UnlockAll() == TRUE;
	}
	return true; //in assenza di sql table rischio di bloccare la transazione se non ritorno True
}

//-----------------------------------------------------------------------------
void MSqlTable::DataFinder::set (CRecordDataFinder* value) 
{  
	if (select->DataFinder)
		SAFE_DELETE(select->DataFinder);

	select->DataFinder = value;
	where->DataFinder = value;
	orderBy->DataFinder = value;
	groupBy->DataFinder = value;
	having->DataFinder = value;
}

/////////////////////////////////////////////////////////////////////////////
// 				class SelectStatement implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
void SelectStatement::AddColumn (MDataObj^ dataObj)
{
	if (m_pSqlTable && dataFinder)
	{		
		SqlRecordItem* pRecItem = dataFinder->GetRecordItem(m_pSqlTable->GetRecord(), dataObj->GetDataObj());
		if (pRecItem != nullptr)
			m_pSqlTable->Select(pRecItem->GetDataObj());
	} 
}

//-----------------------------------------------------------------------------
void SelectStatement::AddColumn (MSqlRecord^ record, MDataObj^ dataObj)
{
	if (m_pSqlTable)
		m_pSqlTable->Select(record->GetSqlRecord(), dataObj->GetDataObj());
}

//-----------------------------------------------------------------------------
void SelectStatement::AddColumn (System::String^ columnName, MDataObj^ dataObj, int allocSize, bool autoIncrement)
{
	if (m_pSqlTable)
		m_pSqlTable->Select(columnName, dataObj->GetDataObj(), allocSize, autoIncrement);
}

//-----------------------------------------------------------------------------
void SelectStatement::AddColumn (System::String^ columnName, MDataObj^ dataObj, int allocSize)
{
	if (m_pSqlTable)
		m_pSqlTable->Select(columnName, dataObj->GetDataObj(), allocSize, false);
}
 
//-----------------------------------------------------------------------------
void SelectStatement::AddColumn ( System::String^ columnName, MDataObj^ dataObj)
{
	if (m_pSqlTable)
		m_pSqlTable->Select(columnName, dataObj->GetDataObj(), 0, false);
}

//-----------------------------------------------------------------------------
void SelectStatement::AddFunction (System::String^ function, MDataObj^ resDataObj)
{
	AddFunction(function, resDataObj, 0);
}

//-----------------------------------------------------------------------------
void SelectStatement::AddFunction (System::String^ function, MDataObj^ resDataObj, int allocSize)
{
	if (m_pSqlTable)
		m_pSqlTable->SelectSqlFun(CString(function), resDataObj->GetDataObj(), allocSize);
}

//-----------------------------------------------------------------------------
void SelectStatement::AddFunction (System::String^ function, MDataObj^ paramDataObj, MDataObj^ resDataObj)
{
	AddFunction(function, paramDataObj, resDataObj, 0, nullptr);
}

//-----------------------------------------------------------------------------
void SelectStatement::AddFunction (System::String^ function, MDataObj^ paramDataObj, MDataObj^ resDataObj, int allocSize)
{
	AddFunction(function, paramDataObj, resDataObj, allocSize, nullptr);
}

//-----------------------------------------------------------------------------
void SelectStatement::AddFunction (System::String^ function, MDataObj^ paramDataObj, MDataObj^ resDataObj, int allocSize, MSqlRecord^ record)
{
	if (m_pSqlTable)
		m_pSqlTable->SelectSqlFun(CString(function), paramDataObj->GetDataObj(), resDataObj->GetDataObj(), allocSize, record == nullptr ? nullptr : record->GetSqlRecord());
}
	
//-----------------------------------------------------------------------------
void SelectStatement::All()
{
	if (m_pSqlTable)
		m_pSqlTable->SelectAll();
}

//-----------------------------------------------------------------------------
void SelectStatement::All(MSqlRecord^ record)
{
	if (m_pSqlTable)
		m_pSqlTable->SelectAll(record->GetSqlRecord());
}

//--------------------------------------------------------------------------
void SelectStatement::FromAllTables()
{
	if (m_pSqlTable)
		m_pSqlTable->SelectFromAllTable();
}

//-----------------------------------------------------------------------------
void SelectStatement::Top(SelectTopType aType, int topValue)
{
	if (m_pSqlTable)
		m_pSqlTable->AddSelectKeyword((SqlTable::SelectKeywordType)aType, topValue);
}

//-----------------------------------------------------------------------------
void SelectStatement::Distinct()
{
	if (m_pSqlTable)
		m_pSqlTable->AddSelectKeyword(SqlTable::DISTINCT);
}


/////////////////////////////////////////////////////////////////////////////
// 				class FromStatement implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
void	FromStatement::AddTable	(MSqlRecord^ record)
{
	if (m_pSqlTable)
	{
		m_pSqlTable->FromTable(record->GetSqlRecord());
	}
}

//-----------------------------------------------------------------------------
void	FromStatement::AddTable	(MSqlRecord^ record, System::String^ alias)
{
	if (m_pSqlTable)
	{
		CString strAlias = CString(alias);
		m_pSqlTable->FromTable(record->GetSqlRecord(), &strAlias);
	}
}

//-----------------------------------------------------------------------------
void	FromStatement::AddTable	(System::String^ tableName, System::String^ alias)
{
	if (m_pSqlTable)
	{
		m_pSqlTable->FromTable(CString(tableName), CString(alias));
	}
}
	
//-----------------------------------------------------------------------------
void	FromStatement::AddTable	(System::String^ tableName, System::String^ alias, MSqlRecord^ record)
{
	if (m_pSqlTable)
	{
		m_pSqlTable->FromTable(CString(tableName), CString(alias), record->GetSqlRecord());
	}
}



/////////////////////////////////////////////////////////////////////////////
// 				class WhereStatement implementation
/////////////////////////////////////////////////////////////////////////////



//-----------------------------------------------------------------------------
void	WhereStatement::AddColumn   (MDataObj^ dataObj)
{
	AddColumn(dataObj, System::String::Empty);
}

//-----------------------------------------------------------------------------
void	WhereStatement::AddColumn     (MSqlRecord^ record, MDataObj^ dataObj)
{
	AddColumn(record, dataObj, System::String::Empty);
}

//-----------------------------------------------------------------------------
void	WhereStatement::AddColumn    (System::String^ columnName)
{
	AddColumn(columnName, System::String::Empty);
}

//-----------------------------------------------------------------------------
void	WhereStatement::AddColumn (System::String^ columnName, System::String^ strOperator)
{
	if (m_pSqlTable)
		m_pSqlTable->AddFilterColumn(CString(columnName), CString(strOperator));
}

//-----------------------------------------------------------------------------
void	WhereStatement::AddColumn (MDataObj^ dataObj, System::String^ strOperator)
{
	if (m_pSqlTable && dataFinder)
	{		
		SqlRecordItem* pRecItem = dataFinder->GetRecordItem(m_pSqlTable->GetRecord(), dataObj->GetDataObj());
		if (pRecItem && pRecItem->GetColumnInfo())
			AddColumn(gcnew System::String(pRecItem->GetColumnInfo()->GetQualifiedColumnName()), strOperator);
	} 
}

//-----------------------------------------------------------------------------
void	WhereStatement::AddColumn (MSqlRecord^ record, MDataObj^ dataObj, System::String^ strOperator)
{
	if (m_pSqlTable)
		m_pSqlTable->AddFilterColumn(record->GetSqlRecord(), *dataObj->GetDataObj(), CString(strOperator));
}


//-----------------------------------------------------------------------------
void WhereStatement::AddParameter	(System::String^ paramName, MDataObj^ dataObj)
{
	if (m_pSqlTable)
		m_pSqlTable->AddParam(paramName, *dataObj->GetDataObj());
}

//-----------------------------------------------------------------------------
void   WhereStatement::AddParameter		(System::String^ paramName, MDataObj^ dataObj, System::String^ columnName)
{
	if (m_pSqlTable && dataObj->DataType == Microarea::TaskBuilderNet::Core::CoreTypes::DataType::Text)
		m_pSqlTable->AddDataTextParam(paramName, *dataObj->GetDataObj(), columnName);
}

//-----------------------------------------------------------------------------
void	WhereStatement::AddCompareColumn    (MDataObj^ dataObj, MSqlRecord^ record, MDataObj^ compareDataObj)
{
	AddCompareColumn(dataObj, record, compareDataObj, System::String::Empty);
}
	
//-----------------------------------------------------------------------------
void	WhereStatement::AddCompareColumn    (MSqlRecord^ record1, MDataObj^ compareDataObj1, MSqlRecord^ record2, MDataObj^ compareDataObj2)
{
	AddCompareColumn(record1, compareDataObj1, record2, compareDataObj2, System::String::Empty);
}

//-----------------------------------------------------------------------------
void	WhereStatement::AddCompareColumn (MDataObj^ dataObj, MSqlRecord^ record, MDataObj^ compareDataObj, System::String^ strOperator)
{
	if (m_pSqlTable && dataFinder)
	{		
		SqlRecordItem* pRecItem = dataFinder->GetRecordItem(m_pSqlTable->GetRecord(), dataObj->GetDataObj());
		if (pRecItem)
			m_pSqlTable->AddCompareColumn(*pRecItem->GetDataObj(), record->GetSqlRecord(), *compareDataObj->GetDataObj(), CString(strOperator));
	} 
}


//-----------------------------------------------------------------------------
void	WhereStatement::AddCompareColumn (MSqlRecord^ record1, MDataObj^ compareDataObj1, MSqlRecord^ record2, MDataObj^ compareDataObj2, System::String^ strOperator)
{
	if (m_pSqlTable)
		m_pSqlTable->AddCompareColumn(record1->GetSqlRecord(), *compareDataObj1->GetDataObj(), record2->GetSqlRecord(), *compareDataObj2->GetDataObj(), CString(strOperator));
}


//-----------------------------------------------------------------------------
void	WhereStatement::AddBetweenColumn (MDataObj^ dataObj)
{
	if (m_pSqlTable && dataFinder)
	{		
		SqlRecordItem* pRecItem = dataFinder->GetRecordItem(m_pSqlTable->GetRecord(), dataObj->GetDataObj());
		if (pRecItem && pRecItem->GetColumnInfo())
			AddBetweenColumn(gcnew System::String(pRecItem->GetColumnInfo()->GetQualifiedColumnName()));
	} 
}

//-----------------------------------------------------------------------------
void	WhereStatement::AddBetweenColumn (MSqlRecord^ record, MDataObj^ dataObj)
{
	if (m_pSqlTable)
		m_pSqlTable->AddBetweenColumn(record->GetSqlRecord(), *dataObj->GetDataObj());
}
	
//-----------------------------------------------------------------------------
void	WhereStatement::AddBetweenColumn (System::String^ columnName)
{
	if (m_pSqlTable)
		m_pSqlTable->AddBetweenColumn(CString(columnName));
}

//-----------------------------------------------------------------------------
void	WhereStatement::Parameter (System::String^ paramName, MDataObj^ value)
{
	if (m_pSqlTable)
		m_pSqlTable->SetParamValue(CString(paramName), *value->GetDataObj());
}

//-----------------------------------------------------------------------------
void	WhereStatement::ParameterLike (System::String^ paramName, MDataObj^ dataObj)
{
	if (m_pSqlTable)
		m_pSqlTable->SetParamLike(CString(paramName), *dataObj->GetDataObj());
}

//-----------------------------------------------------------------------------
void	WhereStatement::AddColumnLike (MDataObj^ dataObj)
{
	if (m_pSqlTable && dataFinder)
	{		
		SqlRecordItem* pRecItem = dataFinder->GetRecordItem(m_pSqlTable->GetRecord(), dataObj->GetDataObj());
		if (pRecItem && pRecItem->GetColumnInfo())
			AddColumnLike(gcnew System::String(pRecItem->GetColumnInfo()->GetQualifiedColumnName()));
	} 
}
	
//-----------------------------------------------------------------------------
void	WhereStatement::AddColumnLike (System::String^ columnName)
{
	if (m_pSqlTable)
		m_pSqlTable->AddFilterLike(CString(columnName));
}
	

/////////////////////////////////////////////////////////////////////////////
// 				class OrderByStatement implementation
/////////////////////////////////////////////////////////////////////////////


//-----------------------------------------------------------------------------
void	OrderByStatement::AddColumn (MDataObj^ dataObj)
{
	AddColumn(dataObj, false);
}
	   
//-----------------------------------------------------------------------------
void	OrderByStatement::AddColumn (MDataObj^ dataObj, bool descending)
{
	if (m_pSqlTable && dataFinder)
	{		
		SqlRecordItem* pRecItem = dataFinder->GetRecordItem(m_pSqlTable->GetRecord(), dataObj->GetDataObj());
		if (pRecItem && pRecItem->GetColumnInfo())
			AddColumn(gcnew System::String(pRecItem->GetColumnInfo()->GetQualifiedColumnName()), descending);
	} 
}
	
//-----------------------------------------------------------------------------
void	OrderByStatement::AddColumn (System::String^ columnName)
{
	AddColumn(columnName, false);
}
	
//-----------------------------------------------------------------------------
void	OrderByStatement::AddColumn (System::String^ columnName, bool descending)
{
	if (m_pSqlTable)
		m_pSqlTable->AddSortColumn(CString(columnName), descending);
}
    
//-----------------------------------------------------------------------------
void	OrderByStatement::AddColumn (MSqlRecord^ record, MDataObj^ dataObj)
{
	AddColumn(record, dataObj, false);
}
	
//-----------------------------------------------------------------------------
void	OrderByStatement::AddColumn (MSqlRecord^ record, MDataObj^ dataObj, bool descending)
{
	if (m_pSqlTable)
		m_pSqlTable->AddSortColumn(record->GetSqlRecord(), *dataObj->GetDataObj(), descending);
}

/////////////////////////////////////////////////////////////////////////////
// 				class GroupByStatement implementation
/////////////////////////////////////////////////////////////////////////////

	
//-----------------------------------------------------------------------------
void	GroupByStatement::AddColumn    (MDataObj^ dataObj)
{
	if (m_pSqlTable && dataFinder)
	{		
		SqlRecordItem* pRecItem = dataFinder->GetRecordItem(m_pSqlTable->GetRecord(), dataObj->GetDataObj());
		if (pRecItem && pRecItem->GetColumnInfo())
			AddColumn(gcnew System::String(pRecItem->GetColumnInfo()->GetQualifiedColumnName()));
	} 
}
	
//-----------------------------------------------------------------------------
void	GroupByStatement::AddColumn    (System::String^ columnName)
{
	if (m_pSqlTable)
		m_pSqlTable->AddGroupByColumn(columnName);
}
	
//-----------------------------------------------------------------------------
void	GroupByStatement::AddColumn    (MSqlRecord^ record, MDataObj^ dataObj)
{
	if (m_pSqlTable)
		m_pSqlTable->AddGroupByColumn(record->GetSqlRecord(), *dataObj->GetDataObj());
}


/////////////////////////////////////////////////////////////////////////////
// 				class HavingStatement implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
void	HavingStatement::AddColumn   (MDataObj^ dataObj)
{
	AddColumn(dataObj, System::String::Empty);
}

//-----------------------------------------------------------------------------
void	HavingStatement::AddColumn(MDataObj^ dataObj, System::String^ strOperator)
{
	if (m_pSqlTable && dataFinder)
	{		
		SqlRecordItem* pRecItem = dataFinder->GetRecordItem(m_pSqlTable->GetRecord(), dataObj->GetDataObj());
		if (pRecItem && pRecItem->GetColumnInfo())
			AddColumn(gcnew System::String(pRecItem->GetColumnInfo()->GetQualifiedColumnName()), strOperator);
	} 
}

//-----------------------------------------------------------------------------
void	HavingStatement::AddColumn (MSqlRecord^ record, MDataObj^ dataObj)
{
	AddColumn(record, dataObj, System::String::Empty);
}

//-----------------------------------------------------------------------------
void	HavingStatement::AddColumn (MSqlRecord^ record, MDataObj^ dataObj, System::String^ strOperator)
{
	if (m_pSqlTable)
		m_pSqlTable->AddHavingFilterColumn(record->GetSqlRecord(), *dataObj->GetDataObj(), CString(strOperator));
}

//-----------------------------------------------------------------------------
void	HavingStatement::AddColumn    (System::String^ columnName)
{
	AddColumn(columnName, System::String::Empty);
}

//-----------------------------------------------------------------------------
void	HavingStatement::AddColumn     (System::String^ columnName, System::String^ strOperator)
{
	if (m_pSqlTable)
		m_pSqlTable->AddHavingFilterColumn(CString(columnName), CString(strOperator));
}


//-----------------------------------------------------------------------------
void	HavingStatement::AddBetweenColumn		(MDataObj^ dataObj)
{
	if (m_pSqlTable && dataFinder)
	{		
		SqlRecordItem* pRecItem = dataFinder->GetRecordItem(m_pSqlTable->GetRecord(), dataObj->GetDataObj());
		if (pRecItem)
			AddBetweenColumn(gcnew System::String(pRecItem->GetColumnInfo()->GetQualifiedColumnName()));
	} 
}

//-----------------------------------------------------------------------------
void	HavingStatement::AddBetweenColumn		(MSqlRecord^ record, MDataObj^ dataObj)
{
	if (m_pSqlTable)
		m_pSqlTable->AddHavingBetweenColumn(record->GetSqlRecord(), *dataObj->GetDataObj());
}
	
//-----------------------------------------------------------------------------
void	HavingStatement::AddBetweenColumn	(System::String^ columnName)
{
	if (m_pSqlTable)
		m_pSqlTable->AddHavingBetweenColumn(CString(columnName));
}

//-----------------------------------------------------------------------------
void	HavingStatement::AddColumnLike (MDataObj^ dataObj)
{
	if (m_pSqlTable && dataFinder)
	{		
		SqlRecordItem* pRecItem = dataFinder->GetRecordItem(m_pSqlTable->GetRecord(), dataObj->GetDataObj());
		if (pRecItem && pRecItem->GetColumnInfo())
			m_pSqlTable->AddHavingFilterLike(gcnew System::String(pRecItem->GetColumnInfo()->GetQualifiedColumnName()));
	}
}
	
//-----------------------------------------------------------------------------
void	HavingStatement::AddColumnLike (System::String^ columnName)
{
	if (m_pSqlTable)
		m_pSqlTable->AddHavingFilterLike(CString(columnName));
}


//-----------------------------------------------------------------------------
void	HavingStatement::AddCompareColumn    (MDataObj^ dataObj, MSqlRecord^ record, MDataObj^ compareDataObj)
{
	AddCompareColumn(dataObj, record, compareDataObj, System::String::Empty);
}
	
//-----------------------------------------------------------------------------
void	HavingStatement::AddCompareColumn    (MDataObj^ dataObj, MSqlRecord^ record, MDataObj^ compareDataObj, System::String^ strOperator)
{
	if (m_pSqlTable && dataFinder)
	{		
		SqlRecordItem* pRecItem = dataFinder->GetRecordItem(m_pSqlTable->GetRecord(), dataObj->GetDataObj());
		if (pRecItem)
			m_pSqlTable->AddHavingCompareColumn(*pRecItem->GetDataObj(), record->GetSqlRecord(), *compareDataObj->GetDataObj(), CString(strOperator));
	}
}

//-----------------------------------------------------------------------------
void	HavingStatement::AddCompareColumn    (MSqlRecord^ record1, MDataObj^ compareDataObj1, MSqlRecord^ record2, MDataObj^ compareDataObj2)
{
	AddCompareColumn(record1, compareDataObj1, record2, compareDataObj2, System::String::Empty);
}
	
//-----------------------------------------------------------------------------
void	HavingStatement::AddCompareColumn    (MSqlRecord^ record1, MDataObj^ compareDataObj1, MSqlRecord^ record2, MDataObj^ compareDataObj2, System::String^ strOperator)
{
	if (m_pSqlTable)
		m_pSqlTable->AddHavingCompareColumn(record1->GetSqlRecord(), *compareDataObj1->GetDataObj(), record2->GetSqlRecord(), *compareDataObj2->GetDataObj(), CString(strOperator));
}
