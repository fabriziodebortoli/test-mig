#include "StdAfx.h"

#include <TbGeneric/DataObj.h>
#include <TbGenlib/AddOnMng.h>

#include <TbOleDb\SqlRec.h>
#include <TbOleDb\Sqltable.h>
#include <TbOleDb\OledbMng.h>
#include <TbOleDb\SqlConnect.h>
#include <TbOleDb\SqlCatalog.h>
#include <TbOleDb\ATLSession.h>

#include "MSqlRecord.h"
#include "MSqlCatalog.h"

using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::Collections::Generic;

/////////////////////////////////////////////////////////////////////////////
// 				class MSqlCatalog Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
MSqlCatalog::MSqlCatalog () 
{  
	m_pConnection = AfxGetDefaultSqlConnection();
}

//-----------------------------------------------------------------------------
MSqlCatalog::MSqlCatalog (System::IntPtr connectionPtr) 
{  
	m_pConnection = (SqlConnection*) connectionPtr.ToInt64();
}

//-----------------------------------------------------------------------------
System::Collections::Generic::IList<IRecord^>^ MSqlCatalog::Items::get () 
{ 
	return GetItems(true, true, true);
}

//-----------------------------------------------------------------------------
System::Collections::Generic::IList<IRecord^>^ MSqlCatalog::Tables::get () 
{ 
	return GetItems(true, false, false);
}

//-----------------------------------------------------------------------------
System::Collections::Generic::IList<IRecord^>^ MSqlCatalog::Views::get () 
{ 
	return GetItems(false, true, false);
}

//-----------------------------------------------------------------------------
System::Collections::Generic::IList<IRecord^>^ MSqlCatalog::StoreProcedures::get () 
{ 
	return GetItems(false, false, true);
}

//-----------------------------------------------------------------------------
System::Collections::Generic::IList<IRecord^>^ MSqlCatalog::TablesAndViews::get () 
{ 
	return GetItems(true, true, false);
}

//-----------------------------------------------------------------------------
Microarea::TaskBuilderNet::Interfaces::DBMSType MSqlCatalog::DbmsType::get ()
{ 
	switch (m_pConnection->GetDBMSType())
	{
	case DBMS_SQLSERVER:
			return Microarea::TaskBuilderNet::Interfaces::DBMSType::SQLSERVER;
	case DBMS_ORACLE:
			return Microarea::TaskBuilderNet::Interfaces::DBMSType::ORACLE;
	}
	return Microarea::TaskBuilderNet::Interfaces::DBMSType::UNKNOWN;
}

//-----------------------------------------------------------------------------
System::Collections::Generic::IList<IRecord^>^ MSqlCatalog::GetItems (bool tables, bool views, bool storeProcedures) 
{ 
	SqlCatalog* pCatalog = AfxGetSqlCatalog(m_pConnection);
	if (!pCatalog)
		return nullptr;

	bool toFilter = !tables || !views || !storeProcedures;

	IList<IRecord^>^ objects = gcnew List<IRecord^>();

	POSITION pos;
	CString key;
	SqlCatalogEntry* pCatalogEntry;

	for (pos = pCatalog->GetStartPosition(); pos != NULL;)
	{
		pCatalog->GetNextAssoc(pos, key, (CObject*&) pCatalogEntry);
		
		if (
				toFilter &&
				(
					(pCatalogEntry->m_nType == TABLE_TYPE && !tables) ||
					(pCatalogEntry->m_nType == VIEW_TYPE && !views) ||
					(pCatalogEntry->m_nType == PROC_TYPE && !storeProcedures)
				)
			)
			continue;

		SqlRecord* pRecord = pCatalogEntry->CreateRecord();
		if (!pRecord)
			continue;
		
		if (!pRecord->IsValid())
		{
			delete pRecord;
			continue;
		}

		MSqlRecord^ rec = gcnew MSqlRecord(pRecord);
		rec->HasCodeBehind = false;
		objects->Add(rec);
	}
	return objects;  
}

//-----------------------------------------------------------------------------
IRecord^ MSqlCatalog::GetTable (System::String^ name)
{ 
	for each (IRecord^ record in Items)
		if (
				System::String::Compare(record->Name, name, true) == 0 && 
				(
					record->RecordType == DataModelEntityType::Table ||
					record->RecordType == DataModelEntityType::Virtual
				)
			)
			return record;
	
	return nullptr;
}

//-----------------------------------------------------------------------------
void MSqlCatalog::RemoveObjectsOfRelease(int nRelease, INameSpace^ moduleNamespace)
{
	CTBNamespace ns(moduleNamespace->ToString());
	CStringArray arTables;
	CStringArray arFields;
	AfxGetWritableDatabaseObjectsTable()->ClearForRelease(nRelease, ns, arTables, arFields);
	SqlCatalog* pCatalog = AfxGetSqlCatalog(m_pConnection);
	
	for (int i = 0; i < arTables.GetCount(); i++)
		pCatalog->RemoveDynamicCatalogEntry(arTables[i]);
	for (int i = 0; i < arFields.GetCount(); i++)
	{
		CString fullName = arFields[i];//tabella.colonna
		int dotIdx = fullName.Find(_T('.'));
		CString strTable = fullName.Left(dotIdx);
		CString strColumn = fullName.Mid(dotIdx + 1);
		pCatalog->RemoveDynamicColumnInfo(strTable, strColumn);
	}
	
}
//-----------------------------------------------------------------------------
void MSqlCatalog::AddTable (IRecord^ table, bool bVirtual, bool masterTable)
{
	//Anticipato l'aggiunta nell'xml, perche per le tabelle create dinamicamente viene interrogato al momento di caricare il sqltableinfo 
	//(durante l'aggiunta al catalog)
	CDbObjectDescription* pNewDescription = new CDbObjectDescription (CTBNamespace::TABLE);
	pNewDescription->SetName(table->Name);
	pNewDescription->SetNotLocalizedTitle(table->Name);
	CTBNamespace ns(table->NameSpace->ToString());
	pNewDescription->SetNamespace(ns);
	pNewDescription->SetCreationRelease(table->CreationRelease);
	pNewDescription->SetDeclarationType(CDbObjectDescription::Dynamic);
	pNewDescription->SetMasterTable(masterTable);
	if (AfxGetWritableDatabaseObjectsTable()->AddObject(pNewDescription) < 0)
		delete pNewDescription;

	//Aggiunta al catalog
	SqlCatalog* pCatalog = AfxGetSqlCatalog(m_pConnection);
	pCatalog->AddDynamicCatalogEntry (m_pConnection, CTBNamespace (CString(table->NameSpace->ToString())), TABLE_TYPE, bVirtual, pNewDescription);
}

//-----------------------------------------------------------------------------
void MSqlCatalog::AddField (IRecord^ table, IRecordField^ field, INameSpace^ ownerModule, bool bVirtual)
{

	SqlCatalog* pCatalog = AfxGetSqlCatalog(m_pConnection);
	
	::DataType aDataType(field->DataObjType->Type, field->DataObjType->Tag);
	
	DataObj* pDataObj =  ::DataObj::DataObjCreate(aDataType);
	
	CString strNs (table->NameSpace->ToString());
	CTBNamespace aTableNs (strNs);
	CString aColName (field->Name);
	
	//carico le eventuali dll necessarie per registrare la tabella
	AfxGetTbCmdManager()->LoadNeededLibraries(aTableNs); 
	
	INameSpace^ tableNs = table->NameSpace;
	CTBNamespace ns (CTBNamespace::MODULE, ownerModule->Application + "." + ownerModule->Module);
	CDbFieldDescription *pField = new CDbFieldDescription(ns);
	const SqlColumnInfo* pColumnInfo = NULL;
	if (field->Type == DataModelEntityFieldType::Column)
	{
		pColumnInfo = pCatalog->AddDynamicColumnInfo (CString(table->Name), aColName, *pDataObj, field->Length, TRUE, bVirtual, field->IsSegmentKey);
		const_cast<SqlColumnInfo*>(pColumnInfo)->UpdateDataObjType(pDataObj);
		
		pField->SetColType (CDbFieldDescription::Column);
	}
	delete pDataObj;

	CDbObjectDescription* pDescri = AfxGetWritableDatabaseObjectsTable()->GetDescription(table->Name);
	pField->SetCreationRelease(field->CreationRelease);
	pField->SetName(field->Name);
	pField->SetDataType(aDataType);
	pField->SetValue(field->DefaultValue);
	//se l'owner module non corrisponde a quello della tabella, si tratta di un addon field
	pField->SetIsAddOn(table->NameSpace->Application != ownerModule->Application || table->NameSpace->Module != ownerModule->Module);
	pDescri->AddDynamicField(pField);
	
}



// devo trovare il campo compatibile per ogni campo del record passato
//-----------------------------------------------------------------------------
bool MSqlCatalog::HaveMasterSlaveRelationship (System::Collections::IList^ masterFields, System::Collections::IList^ slaveFields)
{
	for each (IRecordField^ masterField in masterFields)
	{
		if (MSqlRecord::GetCompatibleFieldsWith(masterField, slaveFields)->Count <= 0)
			return false;
	}

	return true;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MForeignKeyField Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
MForeignKeyField::MForeignKeyField (System::String^ pkFieldName, System::String^ fkFieldName, System::String^ pkTableName, System::String^ fkTableName)
{
	//ATTENZIONE! ci teniamo da parte i nomi e non i SqlRecordItem perché
	//nel caso si effettui un refresh del data model a seguito di una aggiunta di campo
	//i SqlREcordItem vvengono distrutti e ricreati e quindi si schianterebbe
	this->pkFieldName = pkFieldName;
	this->fkFieldName = fkFieldName;
	this->pkTableName = pkTableName;
	this->fkTableName = fkTableName;
}


//-----------------------------------------------------------------------------
System::String^ MForeignKeyField::PrimaryKey::get ()
{ 
	return pkFieldName;
}

//-----------------------------------------------------------------------------
void MForeignKeyField::PrimaryKey::set (System::String^ field)
{ 
	pkFieldName = field;
}

//-----------------------------------------------------------------------------
System::String^ MForeignKeyField::ForeignKey::get ()
{
	return fkFieldName;
}

//-----------------------------------------------------------------------------
void MForeignKeyField::ForeignKey::set (System::String^ field)
{ 
	fkFieldName = field;
}

//-----------------------------------------------------------------------------
bool MForeignKeyField::IsInForeignKey (System::String^ fieldName)
{
	return PrimaryKey == fieldName;
}