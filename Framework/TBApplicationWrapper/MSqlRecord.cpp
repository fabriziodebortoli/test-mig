#include "StdAfx.h"

#include <TbGeneric/DataObj.h>
#include <TbOleDb\SqlRec.h>
#include <TbGes\DBT.h>
#include <TbGes\ExtDoc.h>

#include "MSqlRecord.h"
#include "MDocument.h"
#include "BusinessObjectParams.h"
#include "MDBTObjects.h"
#include "MParsedControls.h"
#include "Attributes.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Collections::Generic;
using namespace System::CodeDom;
using namespace System::ComponentModel;
using namespace System::ComponentModel::Design::Serialization;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Interfaces::View;

using namespace ICSharpCode::NRefactory::CSharp;
using namespace ICSharpCode::NRefactory::PatternMatching;

/////////////////////////////////////////////////////////////////////////////
// 				class RecordFieldSerializer Implementation
/////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------	
Object^ RecordFieldSerializer::Serialize(IDesignerSerializationManager^ manager, Object^ object)
{
	System::Collections::Generic::List<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();

	if (object == nullptr || (!object->GetType()->IsSubclassOf(MSqlRecordItem::typeid) && object->GetType() != MSqlRecordItem::typeid))
		return newCollection;
	
	MSqlRecordItem^ sqlRecordItem = (MSqlRecordItem^) object;
	MDataObj^ dataObj = (MDataObj^)sqlRecordItem->DataObj;
	

	if (MLocalSqlRecordItem::typeid->IsInstanceOfType(sqlRecordItem))
	{
		MLocalSqlRecordItem^ localField = (MLocalSqlRecordItem^)sqlRecordItem;

		//AddLocalField("name", <DataType.Integer>);
		String^ sDataType = localField->DataObjType.DataTypeToString();
		Statement^ invoke = AstFacilities::GetInvocationStatement
			(
				gcnew ThisReferenceExpression(),
				AddLocalFieldMethodName,
				gcnew PrimitiveExpression(localField->Name),
				gcnew PrimitiveExpression(sDataType),
				gcnew PrimitiveExpression((Int32)localField->Length)
				);
		newCollection->Add(invoke);
	}
	String^ dataObjClass = sqlRecordItem->DataObj->GetType()->ToString();
	String^  variableName = sqlRecordItem->SerializedName;

	//this.GetFieldPtr("Description")
	InvocationExpression^ invoke = AstFacilities::GetInvocationExpression
		(
			gcnew ThisReferenceExpression(),
			GetFieldPtrMethodName,
			gcnew PrimitiveExpression(sqlRecordItem->Name)
			);

	//new Microarea.Framework.TBApplicationWrapper.MDataStr()
	ObjectCreateExpression^ codeCreateExpresson = AstFacilities::GetObjectCreationExpression(gcnew SimpleType(dataObjClass), invoke);

	array<Statement^>^ conditions = gcnew array<Statement^>(2);

	//<varName> = <codeCreateExpresson>;
	//_fld_Description = new Microarea.Framework.TBApplicationWrapper.MDataStr(this.GetFieldPtr("Description"));
	newCollection->Add(AstFacilities::GetAssignmentStatement(gcnew IdentifierExpression(variableName), codeCreateExpresson));
				
	//this.Add({varName});
	newCollection->Add(AstFacilities::GetInvocationStatement
		(
			gcnew ThisReferenceExpression(),
			AddMethodName,
			gcnew IdentifierExpression(variableName),
			gcnew PrimitiveExpression(dataObj->IsChanged)
			));

	// events
	System::Collections::Generic::IList<Statement^>^ events = SerializeEvents(manager, sqlRecordItem, sqlRecordItem->SerializedName);
	if (events != nullptr)
		newCollection->AddRange(events);

	return newCollection;

}

//----------------------------------------------------------------------------	
TypeDeclaration^ RecordFieldSerializer::SerializeClass (SyntaxTree^ syntaxTree, IComponent^ object)
{
	//Il record field non ha classe da serializzare.
	return nullptr;
}


/////////////////////////////////////////////////////////////////////////////
// 				class RecordSerializer Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Object^ RecordSerializer::Serialize(IDesignerSerializationManager^ manager, Object^ object)
{
	if (object == nullptr || (!object->GetType()->IsSubclassOf(MSqlRecord::typeid) && object->GetType() != MSqlRecord::typeid))
		return nullptr;

	System::Collections::Generic::List<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();
	
	MSqlRecord^ record = (MSqlRecord^)object;

	System::String^ className = record->SerializedType;

	System::String^ recVar = record->SerializedName;

	System::String^ recordPtrType = String::IsNullOrEmpty(record->InstanceName) ? GetRecordPtrMethodName : GetOldRecordPtrMethodName;

	//dichiara un this oppure <instanceName>.
	//this.GetRecordPtr()
	InvocationExpression^ invoke = gcnew InvocationExpression(
		gcnew MemberReferenceExpression(gcnew ThisReferenceExpression(), recordPtrType)
		);

	//new TDBTContactOrigin_MA_ContactOrigin(this.GetRecordPtr());
	ObjectCreateExpression^ codeCreateExpresson = AstFacilities::GetObjectCreationExpression(record->SerializedType, invoke);

	newCollection->Add(
		gcnew ExpressionStatement(gcnew AssignmentExpression(
			gcnew IdentifierExpression(recVar),
			AssignmentOperatorType::Assign,
			codeCreateExpresson
			))
		);

	//this.Add({varName});
	newCollection->Add(
	
		AstFacilities::GetInvocationStatement(
			gcnew ThisReferenceExpression(),
			AddMethodName,
			gcnew IdentifierExpression(recVar),
			gcnew PrimitiveExpression(record->IsChanged)
			)
		);

	// events
	System::Collections::Generic::IList<Statement^>^ events = SerializeEvents(manager, record, record->SerializedName);
	if (events != nullptr)
		newCollection->AddRange(events);

	return newCollection;
}

//----------------------------------------------------------------------------	
TypeDeclaration^ RecordSerializer::SerializeClass (SyntaxTree^ syntaxTree, IComponent^ object)
{
	if (object == nullptr || (!object->GetType()->IsSubclassOf(MSqlRecord::typeid) && object->GetType() != MSqlRecord::typeid))
		return nullptr;
	
	MSqlRecord^ record = (MSqlRecord^) object; 

	NamespaceDeclaration^ ns = EasyBuilderSerializer::GetNamespaceDeclaration(syntaxTree);
	
	String^ className = record->SerializedType;
	TypeDeclaration^ aClass = FindClass(syntaxTree, className);
	if (aClass != nullptr)
		aClass->Remove();

	aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MSqlRecord::typeid->FullName));

	// Constructor
	ConstructorDeclaration^ constr1 = gcnew ConstructorDeclaration();
	constr1->Modifiers = Modifiers::Public;
	constr1->Name = aClass->Name;
	aClass->Members->Add(constr1);

	constr1->Parameters->Add(gcnew ParameterDeclaration(gcnew SimpleType(IntPtr::typeid->FullName), EasyBuilderSerializer::WrappedObjectParamName, ICSharpCode::NRefactory::CSharp::ParameterModifier::None));

	constr1->Body = gcnew BlockStatement();
	constr1->Initializer = AstFacilities::GetConstructorInitializer(
		gcnew IdentifierExpression(EasyBuilderSerializer::WrappedObjectParamName)
		);

	constr1->Body = gcnew BlockStatement();

	// fields
	for each (MSqlRecordItem^ recordField in record->Fields)
	{
		//accessor
		//PropertyDeclaration^ fieldAccessor = GenerateFieldAccessor (recordField);
		//if (fieldAccessor == nullptr)
		//	continue;

		//aClass->Members->Add(fieldAccessor);

		// data member
		FieldDeclaration^ field = AstFacilities::GetFieldsDeclaration(
			recordField->SerializedType,
			recordField->SerializedName
			);
		field->Modifiers = Modifiers::Public;

		aClass->Members->Add(field);
	}

	return aClass;
}

//----------------------------------------------------------------------------	
PropertyDeclaration^ RecordSerializer::GenerateFieldAccessor(MSqlRecordItem^ recordField)
{
	String^ dataObjClass = recordField->DataObj->GetType()->ToString();
	String^  variableName = recordField->SerializedName;

	System::Collections::Generic::List<Statement^>^ getStatements = gcnew System::Collections::Generic::List<Statement^>();

	getStatements->Add(gcnew ReturnStatement(gcnew IdentifierExpression(variableName)));
	
	return GenerateProperty(dataObjClass, recordField->SerializedName, getStatements, false);
}


/////////////////////////////////////////////////////////////////////////////
// 				class MLocalSqlRecordItem Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
System::String^ MLocalSqlRecordItem::Name::get ()
{
	return __super::Name;
}
//-----------------------------------------------------------------------------
void MLocalSqlRecordItem::Name::set(System::String^ value) 
{
	m_pItem->SetColumnName(value);
}
//-----------------------------------------------------------------------------
Microarea::TaskBuilderNet::Core::CoreTypes::DataType MLocalSqlRecordItem::DataObjType::get ()
{
	return __super::DataObjType;
}
//-----------------------------------------------------------------------------
void MLocalSqlRecordItem::DataObjType::set(Microarea::TaskBuilderNet::Core::CoreTypes::DataType value)
{
	m_DataObjType = value;
	::DataType aDataType = ::DataType(value.Type, value.Tag);
	delete m_pItem->GetDataObj();
	::DataObj* pNewData = ::DataObj::DataObjCreate(aDataType);
	m_pItem->SetDataObj(pNewData);
	
	const_cast<SqlColumnInfo*>(m_pItem->GetColumnInfo())->ForceUpdateDataObjType(pNewData);
	DataObj = MDataObj::Create(pNewData);
}


//-----------------------------------------------------------------------------
int MLocalSqlRecordItem::Length::get ()
{
	return __super::Length;
}
//-----------------------------------------------------------------------------
void MLocalSqlRecordItem::Length::set(int value)
{
	const_cast<SqlColumnInfo*>(m_pItem->GetColumnInfo())->m_lLength = value;
}

//-----------------------------------------------------------------------------
bool MLocalSqlRecordItem::CanChangeProperty	(System::String^ propertyName)
{
	return ReferencesCount == 0 && 
		((MDataObj^)DataObj)->ReferencesCount == 0 &&
		(propertyName != "Length" || DataObjType == Microarea::TaskBuilderNet::Core::CoreTypes::DataType::String);
}
//-----------------------------------------------------------------------------
bool MLocalSqlRecordItem::CanBeDeleted::get ()
{
	return ReferencesCount == 0 && ((MDataObj^)DataObj)->ReferencesCount == 0;
}

/////////////////////////////////////////////////////////////////////////////
// 				class CRecordDisposing Implementation
/////////////////////////////////////////////////////////////////////////////
//classe ponte usata per chiamare l'evento di dispose del MSqlREcord
class CRecordDisposing : public TDisposablePtr<SqlRecord>
{
	gcroot<MSqlRecord^> record;
public:
	CRecordDisposing(MSqlRecord^ record) : record(record){}
protected:
	virtual void OnDisposing(){ if (record) record->FireUnmanagedObjectDisposing(); }
};

/////////////////////////////////////////////////////////////////////////////
// 				class MSqlRecord Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
MSqlRecord::MSqlRecord () 
{  
	m_ppRecord = new CRecordDisposing(this); 
	m_pLocals = NULL;
	HasCodeBehind = false;
	m_bComponentsCreated = false;
}
//-----------------------------------------------------------------------------
MSqlRecord::MSqlRecord (System::String^ tableName)
{
	if (System::String::IsNullOrWhiteSpace(tableName))
	{
		try
		{
			tableName = Microarea::Framework::TBApplicationWrapper::TableAttribute::GetTableName(this->GetType());
		}
		catch (InvalidOperationException^ exc)
		{
			Diagnostic->SetError(exc->ToString());
			return;
		}
		if (System::String::IsNullOrWhiteSpace(tableName))
		{
			Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("No table name found for: {0-%s}"), CString(this->GetType()->Name))));
			return;
		}
	}
	const SqlCatalogEntry* pEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(CString(tableName));
	if (!pEntry)
	{
		Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("Invalid table name: {0-%s}"), CString(tableName))));
		return;
	}
	m_ppRecord = new CRecordDisposing(this);
	*m_ppRecord = pEntry->CreateRecord();
	m_pLocals = NULL;
	HasCodeBehind = false;
	m_bComponentsCreated = false;
}
//-----------------------------------------------------------------------------
MSqlRecord::MSqlRecord (System::IntPtr sqlRecPtr)
{
	m_ppRecord = new CRecordDisposing(this);
	*m_ppRecord = (SqlRecord*)sqlRecPtr.ToInt64();
	
	m_pLocals = NULL;
	HasCodeBehind = true;
	m_bComponentsCreated = false;
}

//-----------------------------------------------------------------------------
MSqlRecord::MSqlRecord (SqlRecord* pRecord)
{ 
	m_ppRecord = new CRecordDisposing(this);
	*m_ppRecord = pRecord;
	
	m_pLocals = NULL;
	HasCodeBehind = true;
	m_bComponentsCreated = false;
}

//-----------------------------------------------------------------------------
MSqlRecord::~MSqlRecord ()
{
	this->!MSqlRecord();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MSqlRecord::!MSqlRecord()
{
	if (m_pLocals)
	{
		//if (!HasCodeBehind) prima serviva perché il SqlRecord poteve essere morto, ma adesso ho lo smartpointer
		if (GetSqlRecord())
			GetSqlRecord()->RemoveExtension(m_pLocals);
		delete m_pLocals;
		m_pLocals = NULL;
	}
	
	if (!HasCodeBehind)
		delete GetSqlRecord();
	
	SAFE_DELETE(m_ppRecord);
}

//-----------------------------------------------------------------------------
bool MSqlRecord::IsAddonInstanceOfSharedSerializedClass::get()
{
	// le istanze ulteriori del record non si serializza come classe altrimenti overridano
	// le informazioni del record vero
	return (!String::IsNullOrEmpty(this->InstanceName));
}

//-----------------------------------------------------------------------------
void MSqlRecord::FirePropertyChanged(String^ propertyName)
{
	PropertyChanged(this, gcnew PropertyChangedEventArgs(propertyName));
}

//-----------------------------------------------------------------------------
void MSqlRecord::FirePropertyChanging(String^ propertyName)
{
	PropertyChanging(this, gcnew PropertyChangingEventArgs(propertyName));
}
//-----------------------------------------------------------------------------
String^ MSqlRecord::RecordDescription::get()
{
	return GetSqlRecord() ? gcnew String(GetSqlRecord()->GetRecordDescription()) : "";
}
//-----------------------------------------------------------------------------
void MSqlRecord::GetCompatibleDataTypes(System::String^ columnName, System::Collections::Generic::List<Microarea::TaskBuilderNet::Core::CoreTypes::DataType>^ dataTypes)
{
	if (!GetSqlRecord())
		return;

	const SqlColumnInfo* pSqlColumnInfo = GetSqlRecord()->GetColumnInfo(columnName);
	ASSERT (pSqlColumnInfo);
	ASSERT (!pSqlColumnInfo->m_bVirtual);
	CWordArray unmanagedDataTypes;
   	if (pSqlColumnInfo->GetDataObjTypes(unmanagedDataTypes))
	{
		for (int i = 0; i < unmanagedDataTypes.GetCount(); i++)
		{
			::DataType dt = unmanagedDataTypes[i];
			dataTypes->Add(Microarea::TaskBuilderNet::Core::CoreTypes::DataType(dt.m_wType, dt.m_wTag));
		}																		

		// aggiunto i tipi aggiuntivi (full date, time, elapsed time)
		if (pSqlColumnInfo->m_DataObjType == DATA_DATE_TYPE)
		{
			dataTypes->Add(Microarea::TaskBuilderNet::Core::CoreTypes::DataType(DATA_DATE_TYPE, DataObj::FULLDATE));
			dataTypes->Add(Microarea::TaskBuilderNet::Core::CoreTypes::DataType(DATA_DATE_TYPE, DataObj::FULLDATE | DataObj::TIME));
		}
		if (pSqlColumnInfo->m_DataObjType == DATA_LNG_TYPE)
			dataTypes->Add(Microarea::TaskBuilderNet::Core::CoreTypes::DataType(DATA_LNG_TYPE, DataObj::TIME));
	}
}

//-----------------------------------------------------------------------------
 ::SqlRecord* MSqlRecord:: GetSqlRecord () 
 { 
	 if (m_ppRecord->operator== (NULL))
		 return NULL;

	 return m_ppRecord ? m_ppRecord->operator->() : NULL; 
 }

//-----------------------------------------------------------------------------
System::String^ MSqlRecord::InstanceName::get()
{
	return instanceName;
}

//-----------------------------------------------------------------------------
void MSqlRecord::InstanceName::set(System::String^ value)
{
	instanceName = value;
}

//-----------------------------------------------------------------------------
System::String^ MSqlRecord::SerializedName::get ()
{ 
	return System::String::IsNullOrEmpty(instanceName) ? System::String::Concat("T", EasyBuilderSerializer::Escape(Name)) : EasyBuilderSerializer::Escape(instanceName);
}

//-----------------------------------------------------------------------------
System::String^ MSqlRecord::SerializedType::get ()
{ 
	return ParentComponent == nullptr ?
			System::String::Concat("T", EasyBuilderSerializer::Escape(Name)) :
			System::String::Concat("T", ((EasyBuilderComponent^) ParentComponent)->SerializedType, "_",  EasyBuilderSerializer::Escape(Name));
}

//-----------------------------------------------------------------------------
System::String^ MSqlRecord::Name::get()
{ 
	return GetSqlRecord() ? gcnew System::String (GetSqlRecord()->GetTableName()) : System::String::Empty;
}

//-----------------------------------------------------------------------------
bool MSqlRecord::IsStorable::get()
{ 
	return GetSqlRecord() ? GetSqlRecord()->IsStorable() == TRUE : false;
}

//-----------------------------------------------------------------------------
void MSqlRecord::IsStorable::set(bool value)
{ 
	if (GetSqlRecord())
		GetSqlRecord()->SetStorable(value == true);
}

//-----------------------------------------------------------------------------
DataModelEntityType	MSqlRecord::RecordType::get()
{ 
	if (!GetSqlRecord())
		return DataModelEntityType::Table;

	int nType = GetSqlRecord()->GetType();
	
	switch (nType)
	{
	case VIEW_TYPE:	return DataModelEntityType::View;
	case PROC_TYPE:	return DataModelEntityType::StoreProcedure;
	case VIRTUAL_TYPE:	return DataModelEntityType::Virtual;
	default:
		return DataModelEntityType::Table;
	}
}

//-----------------------------------------------------------------------------
Microarea::TaskBuilderNet::Core::Generic::NameSpace^ MSqlRecord::NameSpace::get  () 
{ 
	if (!GetSqlRecord() || !GetSqlRecord()->GetNamespace())
		return nullptr;

	return gcnew Microarea::TaskBuilderNet::Core::Generic::NameSpace(gcnew System::String (GetSqlRecord()->GetNamespace()->ToString()));
}

//-----------------------------------------------------------------------------
bool MSqlRecord::IsValid::get()
{ 
	return GetSqlRecord() && GetSqlRecord()->IsValid();
}
//-----------------------------------------------------------------------------
bool MSqlRecord::IsRegistered::get()
{ 
	return GetSqlRecord() && GetSqlRecord()->IsRegistered();
}

//-----------------------------------------------------------------------------
void MSqlRecord::AddRecItems(const ::SqlRecord* pRec)
{
	SqlRecordItem* pItem;
	DataModelEntityFieldType type;

	for (int i=0; i <= pRec->GetUpperBound(); i++)
	{
		pItem = pRec->GetAt(i);

		type = pRec->GetColumnInfo(i) && pRec->IsVirtual(i) ?
						DataModelEntityFieldType::Variable :
						DataModelEntityFieldType::Column;

		MSqlRecordItem^	field = gcnew MSqlRecordItem(this, pItem, type,  true);
		components->Add(field);
	}
}

//-----------------------------------------------------------------------------
System::Collections::IList^ MSqlRecord::Fields::get() 
{ 
	// potrebbe non avere il record
	::SqlRecord* pRec = GetSqlRecord();
	if (!pRec)
		return components;

	if (m_bComponentsCreated) 
		return components;

	AddRecItems(pRec);

	if (pRec->GetExtensions())
	{
		for (int j = 0; j < pRec->GetExtensions()->GetSize(); j++)
		{
			SqlRecord* pExt = pRec->GetExtensions()->GetAt(j);
			SqlRecordLocals* pLocals = dynamic_cast<SqlRecordLocals*>(pExt);
			if (pLocals)
			{
				// questa parte è usata dalla Add Local Field della property grid
				// dove va a completare il wrapping del SqlRecordItem in modo corretto.
				// Questo e' il motivo per cui ciclando sulle extensions, considera anche
				// la SqlRecordLocals che in teoria e' nostra
				SqlRecordItem* pItem;

				for (int i=0; i <= pExt->GetUpperBound(); i++)
				{
					pItem = pExt->GetAt(i);

					// lo aggiungo solo se non e' già presente con le 
					// caratteristiche giuste e identiche. 
					for (int c = components->Count - 1; c >= 0; c--)
					{
						IComponent^ component = components[c];
						MSqlRecordItem^ item = dynamic_cast<MSqlRecordItem^>(component);
						if (item != nullptr && String::Compare(gcnew String(pItem->GetColumnName()), item->Name) == 0)
						{
							// se e' lui non lo riaggiungo
							/*if (
									item->DataObjType.Type == pItem->GetDataType().m_wType && 
									item->Length == pItem->GetColumnLength()
								)
								continue;
							else*/
							{ 
								// se non e' lui rimuovo il vecchio e lo riaggiungo
								// con le nuove caratteristiche giuste
								components->Remove(component);
								break;
							}
						}
					}

					MSqlRecordItem^	field = gcnew MLocalSqlRecordItem(this, pItem, false);
					components->Add(field);
				}
			}
			else
			{
				AddRecItems(pExt);
			}
		}
	}

	m_bComponentsCreated = true;
	return components;
}

//-----------------------------------------------------------------------------
System::Collections::IList^ MSqlRecord::PrimaryKeyFields::get()
{
	List<IRecordField^>^ pkFields = gcnew List<IRecordField^>();
	for each (IRecordField^ field in Fields)
		if (field->IsSegmentKey)
			pkFields->Add (field);

	return pkFields;
}

//-----------------------------------------------------------------------------
void MSqlRecord::SetValue(System::String^ fieldName, Object^ value)
{
	IRecordField^ field = GetField(fieldName);

	if (field != nullptr)
		field->Value = value;
}

//-----------------------------------------------------------------------------
Object^ MSqlRecord::GetValue(System::String^ fieldName)
{
	IRecordField^ field = GetField(fieldName);
	
	return field == nullptr ? nullptr : field->Value;
}
	
//-----------------------------------------------------------------------------
IDataObj^ MSqlRecord::GetData (System::String^ fieldName)
{
	IRecordField^ field = GetField(fieldName);
	
	return field == nullptr ? nullptr : field->DataObj;
}
//-----------------------------------------------------------------------------
IDataObj^ MSqlRecord::GetData (::DataObj* pDataObj)
{
	for each (IRecordField^ field in Fields)
	{
		if (field != nullptr && ((MDataObj^)field->DataObj)->GetDataObj() == pDataObj)
			return field->DataObj;
	}
	return nullptr;
}

//-----------------------------------------------------------------------------
IRecordField^ MSqlRecord::Add (System::String^ name, DataModelEntityFieldType type, IDataType^ dataType, System::String^ localizableName, int length)
{
	if (!GetSqlRecord())
	{
		ASSERT(FALSE);
		return nullptr;
	}

	::DataType aDataType(dataType->Type, dataType->Tag);
	
	::DataObj* pDataObj =  ::DataObj::DataObjCreate(aDataType);
	if (!pDataObj)
		return nullptr;

	SqlCatalog* pCatalog = AfxGetSqlCatalog(GetSqlRecord()->GetConnection());
	if (!pCatalog)
	{
		delete pDataObj;
		return nullptr;
	}

	GetSqlRecord()->SetBindingDynamically(TRUE);
	CString aColName (name);
	switch (type)
	{
		case DataModelEntityFieldType::Column:
			GetSqlRecord()->BindDataObj(GetSqlRecord()->GetSize(), aColName, *pDataObj->Clone());
			break;
		case DataModelEntityFieldType::Variable:
			GetSqlRecord()->BindLocalDataObj(GetSqlRecord()->GetSize(), aColName, *pDataObj->Clone());
			break;
		case DataModelEntityFieldType::Parameter:
			// TODOBRUNA store procedure parameter
			break;
	}
	
	GetSqlRecord()->SetBindingDynamically(FALSE);
	delete pDataObj;

	SqlRecordItem* pRecItem = GetSqlRecord()->GetItemByColumnName (aColName);
	if (!pRecItem)
		return nullptr;

	MSqlRecordItem^ item = gcnew MSqlRecordItem (this, pRecItem, type, false, dataType);
	components->Add(item);
	
	return item; 
}

//-----------------------------------------------------------------------------
IRecordField^ MSqlRecord::GetField(System::String^ name)
{
	if (System::String::IsNullOrEmpty(name))
		return nullptr;

	array<System::String^>^ names = name->Split('.');
	System::String^ fieldName = names->Length > 1 ? names[1] : name;
	::SqlRecord* pRec = this->GetSqlRecord ();
	System::Diagnostics::Debug::Assert(pRec != nullptr, "MSqlRecord::GetField: return nullptr");
	System::String^ recQualifier = gcnew System::String(pRec->GetTableName());
	bool bNeedQualifier = names->Length > 1 && System::String::Compare(names[0], recQualifier, true) != 0;

	for each (IRecordField^ field in Fields)
	{
		if (field == nullptr) continue;

		if (bNeedQualifier)
		{
			if (System::String::Compare(field->QualifiedName, name, true) == 0)
				return field;
		}
		else
		{
			if (System::String::Compare(field->Name, fieldName, true) == 0)
				return field;
		}
	}
	return nullptr;
}

//-----------------------------------------------------------------------------
IRecordField^ MSqlRecord::GetField(IDataObj^ dataObj)
{
		for each (IRecordField^ field in Fields)
		if (field->DataObj == dataObj)
			return field;
	
	return nullptr;
}


//-----------------------------------------------------------------------------
IRecordField^ MSqlRecord::GetField(MDataObj^ dataObj)
{
		for each (IRecordField^ field in Fields)
		if (((MDataObj^)field->DataObj)->DataObjPtr == dataObj->DataObjPtr)
			return field;
	
	return nullptr;
}

//-----------------------------------------------------------------------------
System::IntPtr MSqlRecord::GetFieldPtr(System::String^ name)
{
	IRecordField^ field = GetField(name);
	if (field == nullptr || field->DataObj == nullptr)
		return System::IntPtr::Zero;
	
	return (System::IntPtr) ((MDataObj^) field->DataObj)->DataObjPtr;
}

//-----------------------------------------------------------------------------
void MSqlRecord::Add(IComponent^ component, System::String^ name)
{
	//Il metodo si chiama Add ma in realtà quello che fa è un re-bind dei dataobj
	//già nati per il sql record con i data obj presenti nell'object model generato dalla customizzaizone.
	if (component == nullptr)
		return;

	if (MDataObj::typeid->IsInstanceOfType(component))
	{
		MDataObj^ aMDataObj = nullptr;
		MDataObj^ toBeAdded = (MDataObj^)component;
		for each (IRecordField^ recordField in Fields)
		{
			aMDataObj = (MDataObj^)(recordField->DataObj);
			if (aMDataObj != nullptr && aMDataObj->DataObjPtr == toBeAdded->DataObjPtr)
			{
				((MSqlRecordItem^)recordField)->DataObj = toBeAdded;
				break;
			} 
		}
	}
	else if (MSqlRecordItem::typeid->IsInstanceOfType(component))
	{
		__super::Add(component, name);
	}

	if (component->GetType()->IsSubclassOf(EasyBuilderComponent::typeid) )
		((EasyBuilderComponent^) component)->ParentComponent = this;

	ITBComponentChangeService^ svc = nullptr; 
	
	if (Site != nullptr)
		svc = (ITBComponentChangeService^) Site->GetService(ITBComponentChangeService::typeid);
	
	if (svc != nullptr)
		svc->OnComponentAdded(this, component);
}

//-----------------------------------------------------------------------------
MLocalSqlRecordItem^ MSqlRecord::AddLocalField(System::String^ name, System::String^ dataType, int length)
{
	Microarea::TaskBuilderNet::Core::CoreTypes::DataType^ eDataType = Microarea::TaskBuilderNet::Core::CoreTypes::DataType::StringToDataType(dataType);
	return AddLocalField(name, eDataType, length);
}

//-----------------------------------------------------------------------------
MLocalSqlRecordItem^ MSqlRecord::AddLocalField(System::String^ name, Microarea::TaskBuilderNet::Core::CoreTypes::DataType^ dataType, int length)
{
	//serve per forzare la creazione della lista PRIMA che abbia aggiunto il local field ,
	//altrimenti lo aggiunge due volte (prima nella creazione dinamica in Fields, poi nella Add
	//di questo metodo; ne approfitto per fare un controllo sulle dimensioni,
	//evito anche che il compilatore mi tolga la riga se si accorge che è inutilizzata
	int cnt = Fields->Count;

	// okkio che a volte il SqlRecord che arriva wrappato contiene già la il SqlRecordLocals
	// perche' viene clonato dal C++ a basso livello. Quindi il puntatore non solo non e' m_pLocals
	// ma la sua istanziazione non appartiene a questa classe. Quindi per evitare di avere doppie
	// estensioni all'interno dell'array prima controllo se l'oggetto esiste già e se esiste già
	// mi limito a wrappare l'esistente senza aggiungere due volte il campo.
	SqlRecordLocals* pExisting = NULL;
	if (!m_pLocals)
	{
		pExisting = (SqlRecordLocals*)(GetSqlRecord()->GetExtension(RUNTIME_CLASS(SqlRecordLocals)));
		if (!pExisting)
		{
			m_pLocals = new SqlRecordLocals(GetSqlRecord()->GetTableName());
			GetSqlRecord()->AddExtension(m_pLocals);
			pExisting = m_pLocals;
		}
	}
		
	SqlRecordItem* pItem = pExisting ? pExisting->GetItemByColumnName(CString(name)) : NULL;

	if (!pItem && m_pLocals)
		pItem = m_pLocals->AddLocalField(::DataType(dataType->Type, dataType->Tag), name);

	if (!pItem)
	{
		ASSERT(FALSE);
		return nullptr;
	}

	// lo aggiungo solo ser non e' già aggiunto
	for each (IComponent^ component in Components)
	{
		MLocalSqlRecordItem^ item = dynamic_cast<MLocalSqlRecordItem^>(component);
		if (item != nullptr && String::Compare(item->Name, name) == 0 && item->DataObjType.Type == dataType->Type)
			continue;
	}

	MLocalSqlRecordItem^ mItem = gcnew MLocalSqlRecordItem(this, pItem, false);
	mItem->Length = length;
	Add(mItem);
	if (Fields->Count != cnt + 1)
	{
		ASSERT(FALSE);
		return nullptr;
	}

	return mItem;
}

//-----------------------------------------------------------------------------
void MSqlRecord::GetKeyInXMLFormat(System::String^% key, bool enumAsString)
{
	key = "";
	if (GetSqlRecord())
	{
		CString strKey;
		GetSqlRecord()->GetKeyInXMLFormat(strKey, enumAsString);
		key = gcnew System::String(strKey);
	}
}

//-----------------------------------------------------------------------------
System::String^ MSqlRecord::GetPrimaryKeyNameValue()
{
	if (GetSqlRecord())
	{
		CString strKey;
		strKey = GetSqlRecord()->GetPrimaryKeyNameValue();
		return gcnew System::String(strKey);
	}
	return gcnew System::String(_T(""));
}

//-----------------------------------------------------------------------------
void MSqlRecord::SetPrimaryKeyNameValue(System::String^ strKey)
{
	if (GetSqlRecord())
		GetSqlRecord()->SetPrimaryKeyNameValue(strKey);
}

//-----------------------------------------------------------------------------
void MSqlRecord::RefreshFields()
{
	if (GetSqlRecord())
	{
		GetSqlRecord()->RefreshDynamicFields();
		ClearComponents();//forzo la rilettura dei campi
		m_bComponentsCreated = false;
	}
}
//-----------------------------------------------------------------------------
/*static*/ System::Collections::IList^ MSqlRecord::GetCompatibleFieldsWith (IRecordField^ field, System::Collections::IList^ fields)
{
	List<IRecordField^>^ compatibleFields = gcnew List<IRecordField^>();
	
	for each (MSqlRecordItem^ pkField in fields)
		if (pkField->IsCompatibleWith(field))
			compatibleFields->Add(pkField);
	
	return compatibleFields;
}

//-----------------------------------------------------------------------------
void MSqlRecord::AttachSqlRecord (::SqlRecord* pRecord, bool hasCodeBehind)
{
	if (!m_ppRecord)
	{
		ASSERT(FALSE);
		return;
	}
	*m_ppRecord = pRecord;
	HasCodeBehind = hasCodeBehind;
}

//-----------------------------------------------------------------------------
int MSqlRecord::CreationRelease::get()
{
	if (!GetSqlRecord())
		return 0;

	CDbObjectDescription* pDescription = AfxGetDatabaseObjectsTable()->GetDescription(GetSqlRecord()->GetTableName());
	if (!pDescription)
	{
		//ASSERT(FALSE);
		return 0;
	}
	return pDescription->GetCreationRelease();
}

//-----------------------------------------------------------------------------
void MSqlRecord::OnAfterCreateComponents()
{
	// potrebbe non avere il record
	::SqlRecord* pRec = GetSqlRecord();
	if (!pRec)
		return;

	if (m_bComponentsCreated) 
		return;

	AddRecItems(pRec);

	if (pRec->GetExtensions())
	{
		for (int j = 0; j < pRec->GetExtensions()->GetSize(); j++)
		{
			SqlRecord* pExt = pRec->GetExtensions()->GetAt(j);
			SqlRecordLocals* pLocals = dynamic_cast<SqlRecordLocals*>(pExt);
			// qui le SqlRecordLocals non devono essere considerate perchè sono
			// apportate dalla customizzazione e MAI dal gestionale, tranne che in
			// caso di esposizione del BusinessObject dove servono x essere serializzate
			MDocument^ document = dynamic_cast<MDocument^>(Document);
			if (pLocals && document != nullptr)
			{
				CAbstractFormDoc* pDoc = document->GetDocument();
				CManagedDocComponentObj* pParams = pDoc->GetManagedParameters();
				CBusinessObjectInvocationInfo* pBOParams = dynamic_cast<CBusinessObjectInvocationInfo*>(pParams);
				if (pBOParams && pBOParams->IsExposing())
				{
					SqlRecordItem* pItem;

					for (int i = 0; i <= pExt->GetUpperBound(); i++)
					{
						pItem = pExt->GetAt(i);

						MSqlRecordItem^	field = gcnew MLocalSqlRecordItem(this, pItem, false);

						components->Add(field);
					}
				}
			}
			else
				AddRecItems(pExt);
		}
	}

	m_bComponentsCreated = true;
	return;
}

//-----------------------------------------------------------------------------
bool MSqlRecord::IsToDelete()
{
	// i record old sono considerati rimuovibili d'ufficio
	// mentre la decisione se il record e' buono viene presa
	// solo sulle caratteristiche del wrapping record primario
	if (!String::IsNullOrEmpty(this->InstanceName))
		return true;

	EasyBuilderComponent^ parentComponent = this->ParentComponent;
	return __super::IsToDelete() && (parentComponent != nullptr && parentComponent->IsToDelete());
}

//-----------------------------------------------------------------------------
void MSqlRecord::Assign(MSqlRecord^ record)
{
	if (GetSqlRecord() && record->GetSqlRecord())
	{
		*GetSqlRecord() = *record->GetSqlRecord();
	}
}

//-----------------------------------------------------------------------------
System::String^ MSqlRecord::Qualifier::get()
{
	return GetSqlRecord() ? gcnew System::String(GetSqlRecord()->GetQualifier()) : nullptr;
}

//-----------------------------------------------------------------------------
void MSqlRecord::Qualifier::set(System::String^ value)
{
	if (!GetSqlRecord())
		return;
	
	if (value == nullptr)
		GetSqlRecord()->DisableQualifier();
	else
		GetSqlRecord()->SetQualifier(CString(value));
}

//-----------------------------------------------------------------------------
bool MSqlRecord::Qualified::get()
{
	return !System::String::IsNullOrEmpty(Qualifier);
}

//-----------------------------------------------------------------------------
void MSqlRecord::Qualified::set(bool value)
{
	// gestione del default come tableName
	Qualifier = value ? Name : nullptr;
}

//-----------------------------------------------------------------------------
void MSqlRecord::Init ()
{
	if (GetSqlRecord())
		GetSqlRecord()->Init();
}

//-----------------------------------------------------------------------------
//bool MSqlRecord::Equals(System::Object^ obj)
//{
//	MSqlRecord^ rec = dynamic_cast<MSqlRecord^>(obj);
//	if (rec == nullptr)
//		return false;
//
//	return rec->GetSqlRecord() == GetSqlRecord();
//}

//-----------------------------------------------------------------------------
int MSqlRecord::GetHashCode()
{
	return __super::GetHashCode();
}

//-----------------------------------------------------------------------------
bool MSqlRecord::IsMasterTable::get()
{
	if (!GetSqlRecord())
		return false;
	const CDbObjectDescription* pDescription = AfxGetDbObjectDescription(CString(this->Name));
	return pDescription ? pDescription->IsMasterTable() == TRUE : false;

}


//-----------------------------------------------------------------------------
System::Collections::IList^ MSqlRecord::GetFieldsNoExtensions()
{
	System::Collections::Generic::List<System::ComponentModel::IComponent^>^	fieldList;
	fieldList = gcnew System::Collections::Generic::List<System::ComponentModel::IComponent^>();

	// potrebbe non avere il record
	::SqlRecord* pRec = GetSqlRecord();
	if (!pRec)
		return fieldList;


	SqlRecordItem* pItem;
	DataModelEntityFieldType type;

	for (int i = 0; i <= pRec->GetUpperBound(); i++)
	{
		pItem = pRec->GetAt(i);

		type = pRec->GetColumnInfo(i) && pRec->IsVirtual(i) ?
			DataModelEntityFieldType::Variable :
			DataModelEntityFieldType::Column;

		MSqlRecordItem^	field = gcnew MSqlRecordItem(this, pItem, type, true);
		fieldList->Add(field);
	}

	return fieldList;
}


/////////////////////////////////////////////////////////////////////////////
// 				class MSqlRecordItem Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MSqlRecordItem::MSqlRecordItem (
									MSqlRecord^ owner,
									SqlRecordItem* pItem, 
									DataModelEntityFieldType type,
									bool hasCodeBehind
								) 
	: 
	m_Owner			(owner),
	m_pItem			(pItem),
	m_type			(type)
{
	DataObj = MDataObj::Create(pItem->GetDataObj());
	HasCodeBehind	= hasCodeBehind;
	owner->IsChanged = !HasCodeBehind || pItem->IsDynamicallyBound();
	((MDataObj^)DataObj)->ParentComponent = owner;
} 

//-----------------------------------------------------------------------------
MSqlRecordItem::MSqlRecordItem (
									MSqlRecord^ owner,
									SqlRecordItem* pItem, 
									DataModelEntityFieldType type, 
									bool hasCodeBehind,
									IDataType^	dataObjType
								) 
	: 
	m_Owner				(owner),
	m_pItem				(pItem),
	m_type				(type),
	m_DataObjType		(dataObjType)
{
	DataObj = MDataObj::Create(pItem->GetDataObj());
	HasCodeBehind = hasCodeBehind;
	owner->IsChanged = !HasCodeBehind || pItem->IsDynamicallyBound();
	((MDataObj^)DataObj)->ParentComponent = owner;
}

//-----------------------------------------------------------------------------
MSqlRecordItem::~MSqlRecordItem()
{
	this->!MSqlRecordItem();
}

//-----------------------------------------------------------------------------
MSqlRecordItem::!MSqlRecordItem()
{
	
}

//-----------------------------------------------------------------------------
void MSqlRecordItem::OnValueChanged(Object^ sender, EasyBuilderEventArgs^ args)
{
	ValueChanged(sender, args);//rimpallo gli eventi del dataobj
	m_Owner->FirePropertyChanged("Name");
}
			
//-----------------------------------------------------------------------------
void MSqlRecordItem::OnValueChanging(Object^ sender, EasyBuilderEventArgs^ args)
{
	ValueChanging(sender, args);//rimpallo gli eventi del dataobj
	m_Owner->FirePropertyChanging("Name");
}
//-----------------------------------------------------------------------------
IRecord^ MSqlRecordItem::Record::get ()
{
	return m_Owner;
}
//-----------------------------------------------------------------------------
int MSqlRecordItem::CreationRelease::get()
{
	if (!m_pItem->GetColumnInfo())
		return 0;

	CDbObjectDescription* pDescription = AfxGetDatabaseObjectsTable()->GetDescription(m_pItem->GetColumnInfo()->GetTableName());
	if (!pDescription)
	{
		//ASSERT(FALSE);
		return 0;
	}
	CDbFieldDescription* pField = pDescription->GetDynamicFieldByName(m_pItem->GetColumnInfo()->GetColumnName());
	if (!pField)
		return pDescription->GetCreationRelease();

	return pField->GetCreationRelease();
}

//-----------------------------------------------------------------------------
bool MSqlRecordItem::NeedsQualification()
{
	// l'elemento non e' valido o non ha possibilità di qualifica
	if (!m_pItem ||  !m_pItem->GetColumnInfo() || m_pItem->GetColumnInfo()->m_bVirtual || m_pItem->GetColumnInfo()->GetTableName().IsEmpty() || m_Owner == nullptr)
		return false;


	return m_Owner->GetSqlRecord()->GetTableName().CompareNoCase(m_pItem->GetColumnInfo()->GetTableName()) != 0;
}

//-----------------------------------------------------------------------------
System::String^ MSqlRecordItem::Name::get () 
{ 
	return gcnew System::String(NeedsQualification()  ? m_pItem->GetColumnInfo()->GetQualifiedColumnName() : m_pItem->GetColumnName());
}

//-----------------------------------------------------------------------------
System::ComponentModel::IComponent^ MSqlRecordItem::EventSourceComponent::get()
{
	System::ComponentModel::IComponent^ data = dynamic_cast<System::ComponentModel::IComponent^>(m_DataObj);
	return data;
}

//-----------------------------------------------------------------------------
System::String^ MSqlRecordItem::DefaultValue::get()
{
	if (!m_pItem->GetColumnInfo())
		return "";

	CDbObjectDescription* pDescription = AfxGetDatabaseObjectsTable()->GetDescription(m_pItem->GetColumnInfo()->GetTableName());
	if (!pDescription)
	{
		//ASSERT(FALSE);
		return "";
	}
	CDbFieldDescription* pField = pDescription->GetDynamicFieldByName(m_pItem->GetColumnInfo()->GetColumnName());
	if (!pField)
		return "";
	return gcnew System::String(pField->GetValue()->FormatDataForXML());
}

//-----------------------------------------------------------------------------
Microarea::TaskBuilderNet::Core::Generic::NameSpace^ MSqlRecordItem::OwnerModule::get () 
{ 
	if (!m_pItem->GetColumnInfo())
		return nullptr;

	CDbObjectDescription* pDescription = AfxGetDatabaseObjectsTable()->GetDescription(m_pItem->GetColumnInfo()->GetTableName());
	if (!pDescription)
	{
		//ASSERT(FALSE);
		return nullptr;
	}
	CDbFieldDescription* pField = pDescription->GetDynamicFieldByName(m_pItem->GetColumnInfo()->GetColumnName());
	const CTBNamespace ns = pField 
		? pField->GetOwnerModule() 
		: CTBNamespace(CTBNamespace::MODULE, pDescription->GetNamespace().GetApplicationName() + _T('.') + pDescription->GetNamespace().GetModuleName());
	return gcnew Microarea::TaskBuilderNet::Core::Generic::NameSpace(gcnew System::String (ns.ToString()));
}

//-----------------------------------------------------------------------------
System::String^ MSqlRecordItem::LocalizableName::get () 
{ 
	if (m_pItem->GetColumnInfo())
		return gcnew System::String(m_pItem->GetColumnInfo()->GetColumnTitle());
	
	return System::String::Empty;
}

//-----------------------------------------------------------------------------
System::String^ MSqlRecordItem::ContextName::get () 
{ 
	CDbObjectDescription* pDescription = AfxGetDatabaseObjectsTable()->GetDescription(m_pItem->GetColumnInfo()->GetTableName());
	if (!pDescription)
		return nullptr;
	
	CDbFieldDescription* pField = NULL;
	if (m_pItem->GetColumnInfo())
		pField = pDescription->GetDynamicFieldByName(m_pItem->GetColumnInfo()->GetColumnName());
	
	return pField ? gcnew String(pField->GetContextName()) : System::String::Empty;
}


//-----------------------------------------------------------------------------
int MSqlRecordItem::Length::get () 
{ 
	if (m_pItem->GetColumnInfo())
		return m_pItem->GetColumnInfo()->GetColumnLength();

	return 0;
}

//-----------------------------------------------------------------------------
IDataType^ MSqlRecordItem::DataObjTypeIRecordField::get () 
{
	return DataObjType;
}

//-----------------------------------------------------------------------------
Microarea::TaskBuilderNet::Core::CoreTypes::DataType MSqlRecordItem::DataObjType::get () 
{ 
	if (m_pItem->GetColumnInfo())
		return Microarea::TaskBuilderNet::Core::CoreTypes::DataType 
						(
							m_pItem->GetColumnInfo()->GetDataObjType().m_wType, 
							m_pItem->GetColumnInfo()->GetDataObjType().m_wTag
						);

	return (Microarea::TaskBuilderNet::Core::CoreTypes::DataType) m_DataObjType;
}

//-----------------------------------------------------------------------------
System::String^	 MSqlRecordItem::QualifiedName::get ()
{
	if (!m_pItem  || !m_pItem->GetColumnInfo())
		return System::String::Empty;

	return gcnew System::String
				(	cwsprintf
					(	_T("%s.%s"), 
						m_pItem->GetColumnInfo()->GetTableName(), 
						m_pItem->GetColumnInfo()->GetColumnName()
					)
				);
}

//-----------------------------------------------------------------------------
System::String^ MSqlRecordItem::SerializedName::get ()
{ 
	String^ name = EasyBuilderSerializer::Escape(Name);
	name = name->Replace(".", "_");
	return System::String::Concat("fld_", name);
}

//-----------------------------------------------------------------------------
System::String^ MSqlRecordItem::SerializedType::get ()
{ 
	return DataObj->GetType()->Name;
}

//-----------------------------------------------------------------------------
DataModelEntityFieldType MSqlRecordItem::Type::get () 
{ 
	return m_type;
}

//-----------------------------------------------------------------------------
bool MSqlRecordItem::IsSegmentKey::get () 
{ 
	return m_pItem && m_pItem->IsSpecial();
}

//-----------------------------------------------------------------------------
void MSqlRecordItem::SetColumnInfo(System::IntPtr pSqlColumnInfo)
{
	const SqlColumnInfo* pColInfo = (const SqlColumnInfo*) pSqlColumnInfo.ToInt64();
	if (pColInfo && m_pItem)
		m_pItem->SetColumnInfo(pColInfo);
}

//-----------------------------------------------------------------------------
void MSqlRecordItem::ReplaceDataObj(System::IntPtr pDataObj, bool deletePrev)
{
	ASSERT_VALID(m_pItem);

	((MDataObj^)m_DataObj)->ReplaceDataObj(pDataObj, FALSE);

	if (m_pItem)
		m_pItem->ReplaceDataObj((::DataObj*) pDataObj.ToInt64(), deletePrev);
}

//-----------------------------------------------------------------------------
Object^ MSqlRecordItem::Value::get () 
{ 
	return DataObj->Value;
}

//-----------------------------------------------------------------------------
void  MSqlRecordItem::Value::set (Object^ value)
{
	DataObj->Value = value;
}

//-----------------------------------------------------------------------------
IDataObj^ MSqlRecordItem::DataObj::get ()
{
	return m_DataObj;
}

//-----------------------------------------------------------------------------
void MSqlRecordItem::DataObj::set (IDataObj^ value)
{
	if (m_DataObj)
	{
		delete m_DataObj;
		((MDataObj^)m_DataObj)->ValueChanged -= gcnew EventHandler<EasyBuilderEventArgs^>(this, &MLocalSqlRecordItem::OnValueChanged);
		((MDataObj^)m_DataObj)->ValueChanging -= gcnew EventHandler<EasyBuilderEventArgs^>(this, &MLocalSqlRecordItem::OnValueChanging);
	}
	m_DataObj = value;
	if (value == nullptr)
	{
		m_DataObjType = nullptr;
		return;
	}

	((MDataObj^)m_DataObj)->ValueChanged += gcnew EventHandler<EasyBuilderEventArgs^>(this, &MLocalSqlRecordItem::OnValueChanged);
	((MDataObj^)m_DataObj)->ValueChanging += gcnew EventHandler<EasyBuilderEventArgs^>(this, &MLocalSqlRecordItem::OnValueChanging);
	m_DataObjType = m_DataObj->DataType;
}

//----------------------------------------------------------------------------
bool MSqlRecordItem::IsCompatibleWith (IRecordField^ field)
{
	return 	DataObjType.Tag == field->DataObjType->Tag &&
			DataObjType.Type == field->DataObjType->Type && 
			Length == field->Length;
}

//----------------------------------------------------------------------------
IDocumentDataManager^ MSqlRecordItem::Document::get ()
{
	return DataObj == nullptr ? nullptr : ((MDataObj^) DataObj)->Document;
}

/////////////////////////////////////////////////////////////////////////////
// 				class FieldDataBinding Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
FieldDataBinding::FieldDataBinding (MDataObj^ dataObj, IDataManager^ parent)
	:
	mDataObj	(dataObj)
{
	InitializeParent();
}

//----------------------------------------------------------------------------
FieldDataBinding::FieldDataBinding (MDataObj^ dataObj)
	:
	mDataObj	(dataObj)
{
	InitializeParent();
}

//----------------------------------------------------------------------------
void FieldDataBinding::InitializeParent()
{
	if (mDataObj == nullptr)
		return;

	if (mDataObj->ParentComponent == nullptr)
		this->parent = nullptr;
	else
	{
		IDataManager^ dataManager = (IDataManager^) mDataObj->ParentComponent->ParentComponent;
		this->parent = dataManager;
	}
}

//----------------------------------------------------------------------------
System::String^ FieldDataBinding::ToString()
{
	if	(mDataObj == nullptr || parent == nullptr || parent->Record == nullptr)
		return System::String::Empty;

	return ReflectionUtils::GetComponentFullPath(mDataObj);
}

//----------------------------------------------------------------------------
bool FieldDataBinding::IsEmpty::get()
{
	return !mDataObj && !parent;
}

//----------------------------------------------------------------------------
Object^ FieldDataBinding::Data::get()
{
	return mDataObj;
}

//----------------------------------------------------------------------------
System::String^ FieldDataBinding::Name::get()
{
	return mDataObj->Name;
}

//----------------------------------------------------------------------------
IDocumentDataManager^ FieldDataBinding::Document::get()
{
	EasyBuilderComponent^ dataManager = (EasyBuilderComponent^) Parent;
	if (!dataManager || !dataManager->Document)
		return nullptr;
	
	return dataManager->Document;
}

//----------------------------------------------------------------------------
IDataType^ FieldDataBinding::DataType::get()
{
	return mDataObj->DataType;
}

//----------------------------------------------------------------------------
IRecord^ FieldDataBinding::Record::get()
{
	return parent ? parent->Record : nullptr;
}

//----------------------------------------------------------------------------
Object^ FieldDataBinding::Clone()
{
	return gcnew FieldDataBinding (mDataObj, parent);
}

//----------------------------------------------------------------------------
bool FieldDataBinding::IsDataReadOnly::get () 
{ 
	return parent == nullptr ? true : !parent->IsUpdatable; 
}

//----------------------------------------------------------------------------
bool FieldDataBinding::DataVisible::get()
{
	return mDataObj == nullptr ? true : mDataObj->Visible;
}

//----------------------------------------------------------------------------
void FieldDataBinding::DataVisible::set(bool value)
{
	if (mDataObj != nullptr)
	{
		mDataObj->Visible = value;
	}
}

//----------------------------------------------------------------------------
bool FieldDataBinding::Modified::get()
{
	return mDataObj->Modified;
}

//----------------------------------------------------------------------------
void FieldDataBinding::Modified::set(bool value)
{
	mDataObj->Modified = value;
}


/////////////////////////////////////////////////////////////////////////////
// 				class DBTDataBinding Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
DBTDataBinding::DBTDataBinding (MDBTObject^ dbt)
	:
	mDbt(dbt)	
{
}

//----------------------------------------------------------------------------
System::String^ DBTDataBinding::ToString()
{
	return mDbt == nullptr ? System::String::Empty : mDbt->GetType()->Name;
}

//----------------------------------------------------------------------------
bool DBTDataBinding::IsEmpty::get()
{
	return !mDbt;
}

//----------------------------------------------------------------------------
Object^ DBTDataBinding::Data::get()
{
	return mDbt;
}

//----------------------------------------------------------------------------
void DBTDataBinding::Data::set(Object^ data)
{ 
	mDbt = (MDBTObject^) data;
}

//----------------------------------------------------------------------------
System::String^ DBTDataBinding::Name::get()
{
	return mDbt->Name;
}

//----------------------------------------------------------------------------
IDocumentDataManager^ DBTDataBinding::Document::get()
{
	return mDbt->Document;
}

//----------------------------------------------------------------------------
IDataType^ DBTDataBinding::DataType::get()
{
	return Microarea::TaskBuilderNet::Core::CoreTypes::DataType::Array;
}

//----------------------------------------------------------------------------
Object^ DBTDataBinding::Clone()
{
	return gcnew DBTDataBinding (mDbt);
}

//----------------------------------------------------------------------------
bool DBTDataBinding::IsDataReadOnly::get () 
{ 
	return mDbt == nullptr ? true : !mDbt->IsUpdatable; 
}

//----------------------------------------------------------------------------
bool DBTDataBinding::DataVisible::get()
{
	return true;
	//Il dbt non supporta la logica di visible/hidden, ne consegue che le griglie non possono essere nascoste con EasyStudio.
}

//----------------------------------------------------------------------------
void DBTDataBinding::DataVisible::set(bool value)
{
	//Il dbt non supporta la logica di visible/hidden, ne consegue che le griglie non possono essere nascoste con EasyStudio.
}

//----------------------------------------------------------------------------
bool DBTDataBinding::Modified::get()
{
	if (this->Parent == nullptr)
		return false;

	IDocumentSlaveBufferedDataManager^ slaveBuffered =
		dynamic_cast<IDocumentSlaveBufferedDataManager^>(this->Parent);

	return slaveBuffered != nullptr && slaveBuffered->Modified;
}

//----------------------------------------------------------------------------
void DBTDataBinding::Modified::set(bool value)
{
	if (this->Parent == nullptr)
		return;

	IDocumentSlaveBufferedDataManager^ slaveBuffered =
		dynamic_cast<IDocumentSlaveBufferedDataManager^>(this->Parent);

	if (slaveBuffered != nullptr)
		slaveBuffered->Modified = true;
}
