#include "StdAfx.h"

#include <TbGes\EXTDOC.H>
#include <TbOleDb\wclause.h>

#include "MDocument.h"
#include "MSqlRecord.h"
#include "MGenericDataManager.h"
#include "QueryController.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::ComponentModel;

/////////////////////////////////////////////////////////////////////////////
// 				class MGenericDataManager Implementation
/////////////////////////////////////////////////////////////////////////////
MGenericDataManager::MGenericDataManager(void) : whereClause(NULL), sqlTableArray(NULL)
{
}

//-----------------------------------------------------------------------------
MGenericDataManager::~MGenericDataManager()
{
	this->!MGenericDataManager();
	System::GC::SuppressFinalize(this);
}
//-----------------------------------------------------------------------------
MGenericDataManager::!MGenericDataManager()
{
	SAFE_DELETE(whereClause);
	SAFE_DELETE(sqlTableArray);
}

//-----------------------------------------------------------------------------
SymTable* MGenericDataManager::GetSymTable()
{
	return ((MDocument^)Document)->GetSymTable();
}

//-----------------------------------------------------------------------------
void MGenericDataManager::OnAfterCreateComponents()
{
	MSqlRecord^ rec = (MSqlRecord^)Record;
	Add(rec);
}

//-----------------------------------------------------------------------------
SqlTableInfoArray* MGenericDataManager::GetTableInfoArray()	
{
	if (!sqlTableArray)
	{
		::SqlRecord *pRecord = ((MSqlRecord^)Record)->GetSqlRecord();
		sqlTableArray = new SqlTableInfoArray (pRecord->GetTableInfo());
	}
	return sqlTableArray; 
}
//-----------------------------------------------------------------------------
void MGenericDataManager::AssignInternalState(MGenericDataManager^ source)
{
	filterQuery = source->filterQuery;
	Querying += source->delegateQuerying;
	Queried += source->delegateQueried;
	DefiningQuery += source->delegateDefiningQuery;
	PreparingQuery += source->delegatePreparingQuery;
}

//-----------------------------------------------------------------------------
WClause* MGenericDataManager::CreateValidWhereClause(SqlTable* pTable, System::String^ whereClause, CString& strError)
{
	CQueryController controller (pTable, GetSymTable(), GetTableInfoArray());
	return controller.CreateValidWhereClause(whereClause, strError);
}
//-----------------------------------------------------------------------------
void MGenericDataManager::DefineQuery (MSqlTable^ mSqlTable)
{
	DefiningQuery(this, gcnew DataManagerEventArgs(mSqlTable));
}
//-----------------------------------------------------------------------------
void MGenericDataManager::PrepareQuery (MSqlTable^ mSqlTable)
{
	this->OnQuerying ();

	if (!System::String::IsNullOrEmpty(filterQuery))
	{
		BOOL bAppend = FALSE;
		if (!whereClause)
		{
			CString strError;
			whereClause = CreateValidWhereClause(mSqlTable->GetSqlTable(), filterQuery, strError);
			if (!whereClause)
				Diagnostic->SetError(gcnew System::String(strError));
			bAppend = TRUE;
		}

		if (whereClause)
			whereClause->PrepareQuery(bAppend);
	}
	PreparingQuery(this, gcnew DataManagerEventArgs(mSqlTable));
}

//-----------------------------------------------------------------------------
void MGenericDataManager::OnQuerying	()
{
	this->Querying (this, nullptr);
}

//-----------------------------------------------------------------------------
void MGenericDataManager::OnQueried ()
{
	this->Queried (this, nullptr);
}


//-----------------------------------------------------------------------------
System::String^	MGenericDataManager::FilterQuery::get ()
{
	return filterQuery;
}

//-----------------------------------------------------------------------------
void MGenericDataManager::FilterQuery::set (System::String^ value)
{
	//controllo skippato per motivi di performance e di coerenza dell'oggetto: se sono
	//nella createcomponents potrei non avere terminato l'inizializzazione e quindi non avere informazioni
	//sufficienti per il controllo (comunque un controllo di validità viene fatto in fase di editing)
	/*if (filterQuery != value 
		&& !System::String::IsNullOrEmpty(value) 
		&& GetTable())
	{
		CString strError;
		WClause * pClause = CreateValidWhereClause(GetTable(), value, strError);
		if (!pClause)
		{
			Diagnostic->SetError(gcnew System::String(strError));
			return;
		}
		delete pClause;
	}*/
	filterQuery = value;
}

//-----------------------------------------------------------------------------
void MGenericDataManager::Add(IComponent^ component, System::String^ name)
{
	//IComponent^ existing = nullptr;
	//int nIndex = components->IndexOf(component);
	//if (nIndex >= 0)
	//	existing = components[nIndex];

	//// a seconda dei tempi di creazione del SqlRecord possiamo ricevere il 
	//// tipizzato successivamente al generico. Se arriva il tipizzato allora lo
	//// sostituiamo a quello generico, altrimenti ci sono due wrapper sullo stesso record
	//if	(
	//		existing != nullptr && existing->GetType() == MSqlRecord::typeid && 
	//		component->GetType()->IsAssignableFrom(MSqlRecord::typeid) && component->GetType() != MSqlRecord::typeid
	//	)
	//{
	//	components->Remove(existing);
	//}
	__super::Add(component, name);
}