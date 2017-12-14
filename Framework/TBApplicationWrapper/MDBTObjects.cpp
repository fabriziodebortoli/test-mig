#include "StdAfx.h"
#include <TbGes\DynDBT.h>
#include <TbGes\EXTDOC.H>
#include <TbGenlib\NumbererInfo.h>
#include <TbGes\BROWSER.H>
#include <TbOleDb\sqlaccessor.h>
#include <TbOleDb\wclause.h>
#include <TbWoormEngine\RepTable.h>

#include "MDocument.h"
#include "MDBTObjects.h"
#include "MParsedControls.h"
#include "QueryController.h"

using namespace System;
using namespace System::CodeDom;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;
using namespace System::ComponentModel::Design::Serialization;
using namespace System::ComponentModel;

using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::TaskBuilderNet::Interfaces::View;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder::Refactor;
using namespace Microarea:: Framework::TBApplicationWrapper;

typedef ICSharpCode::NRefactory::CSharp::Attribute AstAttribute;
typedef ICSharpCode::NRefactory::CSharp::Expression AstExpression;

/////////////////////////////////////////////////////////////////////////////
// 				class MXMLSearchBookmark Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
MXMLSearchBookmark::MXMLSearchBookmark (System::IntPtr fieldPtr)
	:
	m_pSearchBookmark ((CXMLSearchBookmark*) fieldPtr.ToInt64())
{ 
}

//-----------------------------------------------------------------------------
System::String^ MXMLSearchBookmark::FieldName::get()
{ 
	return m_pSearchBookmark ? gcnew System::String(m_pSearchBookmark->GetName()) : System::String::Empty;
}

//-----------------------------------------------------------------------------
bool MXMLSearchBookmark::ShowAsDescription::get()		
{  
	return (m_pSearchBookmark && m_pSearchBookmark->ShowAsDescription());
}


//-----------------------------------------------------------------------------
System::String^ MXMLSearchBookmark::HKLName::get()
{ 
	return m_pSearchBookmark ? gcnew System::String(m_pSearchBookmark->GetHKLName()) : System::String::Empty;
}

//-----------------------------------------------------------------------------
System::String^ MXMLSearchBookmark::KeyCode::get()
{ 
	return m_pSearchBookmark ? gcnew  System::String(m_pSearchBookmark->GetKeyCode()) : System::String::Empty;
}




/////////////////////////////////////////////////////////////////////////////
// 				class DBTSerializer Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
System::Object^ DBTSerializer::Serialize(IDesignerSerializationManager^ manager, System::Object^ current)
{
	System::Collections::Generic::List<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();

	MDBTObject^ dbt = (MDBTObject^) current;
	
	System::String^ className	= dbt->SerializedType;
	System::String^ varName		= dbt->SerializedName;

	//TODO MATTEO Gestire commenti
	//// comment
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	//newCollection->Add(gcnew CodeCommentStatement(String::Concat(" ", className)));
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	
	// construction
	IdentifierExpression^ varExpression	= gcnew IdentifierExpression(varName);
	newCollection->Add
		(
			AstFacilities::GetAssignmentStatement(
				varExpression,
				AstFacilities::GetObjectCreationExpression(
					className
					)
				)
		);
	SetExpression(manager, dbt, varExpression, true);

	// attach process
	varExpression = gcnew IdentifierExpression(varName);
	Statement^ attachStatement = dbt->Relation == DataRelationType::Master ?
		AstFacilities::GetInvocationStatement	//this.AttachMaster(DBTCompany);
		(
			gcnew ThisReferenceExpression(),
			AttachMasterMethodName,
			gcnew MemberReferenceExpression(gcnew ThisReferenceExpression(), varName)
		)
		:
		AstFacilities::GetInvocationStatement	//this.AttachSlave(_DBTPeople);
		(
			gcnew ThisReferenceExpression(),
			AttachSlaveMethodName,
			varExpression
		);
	newCollection->Add(attachStatement);

	// foreign key relation
	if (!dbt->HasCodeBehind && dbt->Relation != DataRelationType::Master)
	{
		MDBTSlave^ dbtSlave = (MDBTSlave^) dbt;
		if (dbtSlave->MasterForeignKey != nullptr && dbtSlave->MasterForeignKey->Count > 0)
		{
			System::Collections::Generic::IList<Statement^>^ fks = SerializeForeignKey(dbtSlave, varName);
			if (fks != nullptr)
				newCollection->AddRange(fks);
		}
	}
	// properties
	System::Collections::Generic::IList<Statement^>^ props = SerializeProperties(manager, dbt, varName);
	if (props != nullptr)
		newCollection->AddRange(props);

	// events
	System::Collections::Generic::IList<Statement^>^ events = SerializeEvents(manager, dbt, dbt->SerializedName);
	if (events != nullptr)
		newCollection->AddRange(events);

	return newCollection;
}

//----------------------------------------------------------------------------	
void DBTSerializer::SerializeOldAccessors(SyntaxTree^ syntaxTree, TypeDeclaration^ aClass, MSqlRecord^ record)
{
	// record accessors
	PropertyDeclaration^ oldAccessor = GenerateRecordAccessor(record, RecordSerializer::GetOldRecordPtrMethodName, OldRecordPropertyName);
	if (oldAccessor != nullptr)
		aClass->Members->Add(oldAccessor);
	
	//PropertyDeclaration^ typedAccessor = GenerateTypedRecordAccessor(record, OldRecordPropertyName);
	//if (typedAccessor != nullptr)
	//	aClass->Members->Add(typedAccessor);

	FieldDeclaration^ field = AstFacilities::GetFieldsDeclaration(record->SerializedType, record->SerializedName);
	field->Modifiers = Modifiers::Public;
	aClass->Members->Add(field);
}

//----------------------------------------------------------------------------	
TypeDeclaration^ DBTSerializer::SerializeClass (SyntaxTree^ syntaxTree, IComponent^ object)
{
	if (object == nullptr || !object->GetType()->IsSubclassOf(MDBTObject::typeid))
		return nullptr;
	
	MDBTObject^ dbt = (MDBTObject^) object; 

	NamespaceDeclaration^ ns = EasyBuilderSerializer::GetNamespaceDeclaration(syntaxTree);
	
	String^ className = dbt->SerializedType;
	RemoveClass(syntaxTree, className);

	Type^ baseType = nullptr;
	switch (dbt->Relation)
	{
	case DataRelationType::Master:		baseType = MDBTMaster::typeid;			break;
	case DataRelationType::OneToMany:	baseType = MDBTSlaveBuffered::typeid;	break;
	default:							baseType = MDBTSlave::typeid;			break;
	}

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(baseType->Name));

	// Costruttore per la creazione di un nuovo DBT nativo
	ConstructorDeclaration^ constr = gcnew ConstructorDeclaration();
	constr->Modifiers = Modifiers::Public;
	constr->Name = aClass->Name;
	aClass->Members->Add(constr);

	constr->Initializer = gcnew ConstructorInitializer();
	constr->Initializer->ConstructorInitializerType = ConstructorInitializerType::Base;
	
	constr->Initializer->Arguments->Add(gcnew PrimitiveExpression(dbt->Record->Name));
	constr->Initializer->Arguments->Add(gcnew PrimitiveExpression(dbt->Name));
	constr->Initializer->Arguments->Add(
		gcnew MemberReferenceExpression(
			gcnew IdentifierExpression(StaticControllerVariableName),
			EasyBuilderControlSerializer::DocumentPropertyName
			)
		);
	constr->Initializer->Arguments->Add(gcnew PrimitiveExpression(dbt->HasCodeBehind));

	constr->Body = gcnew BlockStatement();
	
	// Costruttore per il wrapping di un dbt nativo esistente
	constr = gcnew ConstructorDeclaration();
	constr->Modifiers = Modifiers::Public;
	constr->Name = aClass->Name;
	aClass->Members->Add(constr);

	String ^ dbtVar = "dbtPtr";

	constr->Parameters->Add(
		gcnew ParameterDeclaration(gcnew SimpleType(IntPtr::typeid->FullName), dbtVar, ParameterModifier::None)
		);	

	constr->Initializer = gcnew ConstructorInitializer();
	constr->Initializer->ConstructorInitializerType = ConstructorInitializerType::Base;

	constr->Initializer->Arguments->Add(gcnew IdentifierExpression(dbtVar));
	
	constr->Body = gcnew BlockStatement();
	//this.controller = controller;

	SerializeRecordAccessor(syntaxTree, aClass, (MSqlRecord^)dbt->Record);

	SerializeOldAccessors(syntaxTree, aClass, (MSqlRecord^)dbt->OldRecord);

	//TMyRecord CastToMyRecord(IRecord record) { return (TMyRecord) record; }
	MethodDeclaration^ castToMyRecordMethod = gcnew MethodDeclaration();
	castToMyRecordMethod->Name = CastToMyRecordMethodName;
	castToMyRecordMethod->Modifiers = Modifiers::Public | Modifiers::Virtual;
	castToMyRecordMethod->ReturnType = gcnew SimpleType (((MSqlRecord^)dbt->Record)->SerializedType);
	castToMyRecordMethod->Parameters->Add (gcnew ParameterDeclaration(gcnew SimpleType(IRecord::typeid->FullName), RecordParamName, ParameterModifier::None));
	
	castToMyRecordMethod->Body = gcnew BlockStatement();
	castToMyRecordMethod->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				gcnew CastExpression
				(
					gcnew SimpleType(((MSqlRecord^)dbt->Record)->SerializedType),
					gcnew IdentifierExpression (RecordParamName)
				)
			)
		);
	aClass->Members->Add(castToMyRecordMethod);

	//override MDBTObject^ CreateAndAttach(IntPtr dbtPtr) { return new DBTMyDBT(dbtPtr); }
	MethodDeclaration^ createDBTMethod = gcnew MethodDeclaration();
	createDBTMethod->Name = "CreateAndAttach";
	createDBTMethod->Modifiers = Modifiers::Public | Modifiers::Override;
	createDBTMethod->ReturnType = gcnew SimpleType (MDBTObject::typeid->FullName);
	createDBTMethod->Parameters->Add (gcnew ParameterDeclaration(gcnew SimpleType (IntPtr::typeid->FullName), dbtVar, ParameterModifier::None));
	
	createDBTMethod->Body = gcnew BlockStatement();
	createDBTMethod->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				AstFacilities::GetObjectCreationExpression(
					dbt->SerializedType,
					gcnew IdentifierExpression (dbtVar)
					)
			)
		);
	aClass->Members->Add(createDBTMethod);

	// serialize typed row method
	if (dbt->Relation == DataRelationType::OneToMany)
		SerializeRowMethods (syntaxTree, aClass, (MSqlRecord^) dbt->Record);

	return aClass;
}

//----------------------------------------------------------------------------	
void DBTSerializer::SerializeRowMethods (SyntaxTree^ syntaxTree, TypeDeclaration^ dbtClass, MSqlRecord^ record)
{
	MethodDeclaration^ currentRowMethod = gcnew MethodDeclaration();
	currentRowMethod->Name = GetCurrentRowMethodName;
	currentRowMethod->Modifiers = Modifiers::Public | Modifiers::Virtual;
	currentRowMethod->ReturnType = gcnew SimpleType (record->SerializedType);

	currentRowMethod->Body = gcnew BlockStatement();
	currentRowMethod->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				gcnew CastExpression
				(
					gcnew SimpleType(record->SerializedType),
					AstFacilities::GetInvocationExpression(
						gcnew ThisReferenceExpression(),
						GetCurrentRecordMethodName
					)
				)
			)
		);

	dbtClass->Members->Add(currentRowMethod);

	MethodDeclaration^ addRowMethod = gcnew MethodDeclaration();
	addRowMethod->Name = AddRowMethodName;
	addRowMethod->Modifiers = Modifiers::Public | Modifiers::Virtual;
	addRowMethod->ReturnType = gcnew SimpleType (record->SerializedType);

	addRowMethod->Body = gcnew BlockStatement();
	addRowMethod->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				gcnew CastExpression
				(
					gcnew SimpleType(record->SerializedType),
					AstFacilities::GetInvocationExpression
					(
						gcnew ThisReferenceExpression(),
						AddRecordMethodName
					)
				)
			)
		);

	dbtClass->Members->Add(addRowMethod);

	MethodDeclaration^ insertRowMethod = gcnew MethodDeclaration();
	insertRowMethod->Name = InsertRowMethodName;
	insertRowMethod->Modifiers = Modifiers::Public | Modifiers::Virtual;
	insertRowMethod->ReturnType = gcnew SimpleType (record->SerializedType);
	insertRowMethod->Parameters->Add (gcnew ParameterDeclaration(gcnew PrimitiveType("int"), RowNrParamName, ParameterModifier::None));

	insertRowMethod->Body = gcnew BlockStatement();
	insertRowMethod->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				gcnew CastExpression
				(
					gcnew SimpleType(record->SerializedType),
					AstFacilities::GetInvocationExpression
					(
						gcnew ThisReferenceExpression(),
						InsertRecordMethodName,
						gcnew IdentifierExpression(RowNrParamName)
					)
				)
			)
		);

	dbtClass->Members->Add(insertRowMethod);

	MethodDeclaration^ getRecordMethod = gcnew MethodDeclaration();
	getRecordMethod->Name = GetRowMethodName;
	getRecordMethod->Modifiers = Modifiers::Public | Modifiers::Virtual;
	getRecordMethod->ReturnType = gcnew SimpleType (record->SerializedType);
	getRecordMethod->Parameters->Add (gcnew ParameterDeclaration(gcnew PrimitiveType("int"), RowNrParamName, ParameterModifier::None));

	getRecordMethod->Body = gcnew BlockStatement();
	getRecordMethod->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				gcnew CastExpression
				(
					gcnew SimpleType(record->SerializedType),
					AstFacilities::GetInvocationExpression
					(
						gcnew ThisReferenceExpression(),
						GetRecordMethodName,
						gcnew IdentifierExpression(RowNrParamName)
					)
				)
			)
		);

	dbtClass->Members->Add(getRecordMethod);

	MethodDeclaration^ getOldRecordMethod = gcnew MethodDeclaration();
	getOldRecordMethod->Name = GetOldRowMethodName;
	getOldRecordMethod->Modifiers = Modifiers::Public | Modifiers::Virtual;
	getOldRecordMethod->ReturnType = gcnew SimpleType (record->SerializedType);
	getOldRecordMethod->Parameters->Add (gcnew ParameterDeclaration(gcnew PrimitiveType("int"), RowNrParamName, ParameterModifier::None));

	getOldRecordMethod->Body = gcnew BlockStatement();
	getOldRecordMethod->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				gcnew CastExpression
				(
					gcnew SimpleType(record->SerializedType),
					AstFacilities::GetInvocationExpression
					(
						gcnew ThisReferenceExpression(),
						GetOldRecordMethodName,
						gcnew IdentifierExpression(RowNrParamName)
					)
				)
			)
		);

	dbtClass->Members->Add(getOldRecordMethod);
}

//----------------------------------------------------------------------------	
System::Collections::Generic::IList<Statement^>^ DBTSerializer::SerializeForeignKey (IDataManager^ dbt, String^ varName)
{
	MDBTSlave^ dbtSlave = (MDBTSlave^) dbt;
	
	System::Collections::Generic::IList<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();
	String^ recordName = varName + "." + ((MSqlRecord^)dbt->Record)->SerializedName;
	for each (MForeignKeyField^ fkField in dbtSlave->MasterForeignKey)
	{
		MSqlRecordItem^ myField = (MSqlRecordItem^) dbtSlave->Record->GetField(fkField->PrimaryKey);
		if (myField == nullptr)
		{
			System::Diagnostics::Debug::Fail("Campo non trovato");
			continue;
		}
		newCollection->Add 
		(
			AstFacilities::GetInvocationStatement
			(
				gcnew IdentifierExpression(varName),
				AddMasterForeignKeyMethodName,
				gcnew MemberReferenceExpression
				(
					gcnew IdentifierExpression(recordName),
					myField->SerializedName
				),
				gcnew PrimitiveExpression(fkField->ForeignKey)
			)				
		);				
	}
	
	return newCollection->Count > 0 ? newCollection : nullptr;
}


/////////////////////////////////////////////////////////////////////////////
// 				class MDBTObject Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDBTObject::MDBTObject ()
	:
	record(nullptr), 
	table(nullptr)
{ 
	m_ppDBTObject = new TDisposablePtr<DBTObject>();
	HasCodeBehind = false;
}

//-----------------------------------------------------------------------------
MDBTObject::MDBTObject (System::IntPtr dbtPtr)
	:
	table(nullptr)
{ 
	m_ppDBTObject = new TDisposablePtr<DBTObject>();
	*m_ppDBTObject = (DBTObject*) dbtPtr.ToInt64();
	HasCodeBehind = true;
	AttachDefaultEvents();
}

//-----------------------------------------------------------------------------
MDBTObject::~MDBTObject()
{
	this->!MDBTObject();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MDBTObject::!MDBTObject()
{
	ClearComponents ();
	
	for (int i = 0; i < Records->Count; i++)
		delete Records[i];

	if (m_ppDBTObject)
		*m_ppDBTObject = NULL;

	if (onCheckPrimaryKeyHandle.IsAllocated)
		onCheckPrimaryKeyHandle.Free();
	SAFE_DELETE(m_ppDBTObject);
}


//-----------------------------------------------------------------------------
void MDBTObject::AttachDefaultEvents()
{
	if (!GetDBTObject())
		return;

	if (onCheckPrimaryKeyHandle.IsAllocated)
		onCheckPrimaryKeyHandle.Free();

	onCheckPrimaryKeyCallBack = gcnew OnCheckPrimaryKeyCallBack(this, &MDBTObject::OnCheckPrimaryKey);
	onCheckPrimaryKeyHandle = GCHandle::Alloc(onCheckPrimaryKeyCallBack);
	IntPtr funPtr = Marshal::GetFunctionPointerForDelegate(onCheckPrimaryKeyCallBack);
	GetDBTObject()->SetOnCheckPrimaryKeyFunPtr(static_cast<DATAOBJ_ROW_FUNC>(funPtr.ToPointer()));
}
//-----------------------------------------------------------------------------
System::Collections::IList^ MDBTObject::Records::get()
{ 
	if (records == nullptr)
		records = gcnew List<MSqlRecord^>();
	
	return records; 
} 


//-----------------------------------------------------------------------------
void MDBTObject::AssignInternalState(MGenericDataManager^ source)
{
	__super::AssignInternalState(source);
	MDBTObject^ o = (MDBTObject^)source;
	CheckPrimaryKey += o->delegateCheckPrimaryKey;	
}

//-----------------------------------------------------------------------------
bool MDBTObject::Valid::get ()
{
	return GetDBTObject() && GetDBTObject()->GetRecord()->IsValid();
}

//-----------------------------------------------------------------------------
void MDBTObject::DefineQuery()
{
	DefineQuery(Table);
}

//-----------------------------------------------------------------------------
void MDBTObject::PrepareQuery()
{
	PrepareQuery(Table);
}

//-----------------------------------------------------------------------------
System::IntPtr	MDBTObject::GetRecordPtr ()
{
	return GetDBTObject() 
		? (System::IntPtr) GetDBTObject()->GetRecord() 
		: System::IntPtr::Zero;
}

//-----------------------------------------------------------------------------
System::IntPtr	MDBTObject::GetOldRecordPtr ()
{
	return GetDBTObject() 
		? (System::IntPtr) GetDBTObject()->GetOldRecord() 
		: System::IntPtr::Zero;
}

//-----------------------------------------------------------------------------
System::String^ MDBTObject::SerializedType::get ()
{ 
	return System::String::Concat("DBT", EasyBuilderSerializer::Escape(Name));
}

//-----------------------------------------------------------------------------
System::String^ MDBTObject::Name::get ()
{ 
	return GetDBTObject() 
		? gcnew System::String(GetDBTObject()->GetNamespace().GetObjectName()) 
		: System::String::Empty;
}

//-----------------------------------------------------------------------------
System::String^ MDBTObject::Title::get ()
{ 
	return GetDBTObject() 
		? gcnew System::String(GetDBTObject()->GetTitle()) 
		: System::String::Empty;
}
//-----------------------------------------------------------------------------
INameSpace^ MDBTObject::Namespace::get ()
{ 
	return gcnew NameSpace(GetDBTObject() ? gcnew System::String(GetDBTObject()->GetNamespace().ToString()) : System::String::Empty);
}

//-----------------------------------------------------------------------------
System::String^ MDBTObject::TableName::get () 
{ 
	return GetDBTObject() ? gcnew System::String(GetDBTObject()->GetRecord()->GetTableName()) : System::String::Empty;
}

//-----------------------------------------------------------------------------
bool MDBTObject::IsDeleteOwner::get ()
{
	return GetDBTObject() ? GetDBTObject()->GetNoDelete() == FALSE: true;
}

//-----------------------------------------------------------------------------
void MDBTObject::IsDeleteOwner::set (bool value)
{
	if (GetDBTObject())
		GetDBTObject()->SetNoDelete(!value);
}

//-----------------------------------------------------------------------------
bool MDBTObject::IsAssociatedToAView::get ()
{
	return GetDBTObject() ? GetDBTObject()->IsDBTOnView() == TRUE: false;
}

//-----------------------------------------------------------------------------
IRecord^ MDBTObject::Record::get ()
{
	return record;
}

//-----------------------------------------------------------------------------
IRecord^ MDBTObject::OldRecord::get ()
{
	return oldRecord;
}

//-----------------------------------------------------------------------------
void MDBTObject::OnAfterCreateComponents()
{
	if (record == nullptr)
	{
		MSqlRecord^ rec = GetDBTObject() ? GetRecord(GetDBTObject()->GetRecord(), false) : nullptr;
		Add(rec);
	}

	if (oldRecord == nullptr)
	{
		MSqlRecord^ old = GetDBTObject() ? GetRecord(GetDBTObject()->GetOldRecord(), false) : nullptr;
		Add(old);
	}
}

//-----------------------------------------------------------------------------
void MDBTObject::Add(IComponent^ component, System::String^ name)
{
	if (component == nullptr)
		return;

	bool isRecord = component->GetType()->IsSubclassOf(MSqlRecord::typeid)  || component->GetType() == MSqlRecord::typeid;
	
	if (isRecord)
	{
		MSqlRecord^ rec = (MSqlRecord^) component;
		if (GetDBTObject() && rec->GetSqlRecord() == GetDBTObject()->GetOldRecord())
		{
			oldRecord = rec;
			if (oldRecord != nullptr)
				rec->InstanceName = System::String::Concat("Old", rec->SerializedName);
		}
		else if (GetDBTObject() && rec->GetSqlRecord() == GetDBTObject()->GetRecord())
		{
			record = rec;
		}
		Records->Add(rec);
	}
	
	__super::Add(component, name);

	// la CreateComponent può funzionare solo dopo che la Add ha finito il suo processo 
	// e ha collegato tutte le parentele degli oggetti
	//	if (isRecord)	
	//		((MSqlRecord^) record)->CallCreateComponents();
	// tolta: così facendo, la CallCreateComponetns viene invocata due volte, inoltre adesso è troppo presto
	// e si crea casino nell'inizializzazione del local fields

}

//-----------------------------------------------------------------------------
::DBTObject* MDBTObject::GetDBTObject() 
{ 
	if (m_ppDBTObject == NULL)
		return NULL;

	if (m_ppDBTObject->operator== (NULL))
		return NULL;

	return m_ppDBTObject ? m_ppDBTObject->operator->() : NULL; 
}

//-----------------------------------------------------------------------------
bool MDBTObject::Equals(Object^ obj)
{
	if (obj == nullptr)
		return false;

	if (!obj->GetType()->IsSubclassOf(MDBTObject::typeid))
		return false;

	return GetDBTObject() == ((MDBTObject^)obj)->GetDBTObject();
}

//-----------------------------------------------------------------------------
MSqlRecord^	MDBTObject::GetRecord (SqlRecord* pRecord)
{
	return GetRecord(pRecord, false);
}

//-----------------------------------------------------------------------------
MSqlRecord^	MDBTObject::GetRecord (SqlRecord* pRecord, bool invokeLazyRecord)
{
	if (!pRecord)
		return nullptr;

	for each (MSqlRecord^ record in Records)
		if (record->GetSqlRecord() == pRecord)
			return record;

	MSqlRecord^ mRecord = nullptr;
	// si occupa di far scattare l'inizializzazione lazy delle
	// classi tipizzate, altrimenti non scatterebbe mai a meno di
	// un intervento esterno nella grammatica
	if (record == nullptr && invokeLazyRecord)
		IRecord^ rec = Record;

	// il record appena nato può essere tipizzato o meno, ma deve entrare 
	// nella catena di parentele per poter navigare bene la propria struttura
	if (record == nullptr)
		mRecord = gcnew MSqlRecord(pRecord);
	else
		mRecord =  (MSqlRecord^) Activator::CreateInstance(record->GetType(), (System::IntPtr) pRecord);
	
	mRecord->ParentComponent = this;
	
	mRecord->CallCreateComponents();

	Records->Add(mRecord);
	mRecord->UnmanagedObjectDisposing += gcnew EventHandler<EventArgs^>(this, &MDBTObject::OnUnmanagedRecordDisposing);
	return mRecord;
}

//-----------------------------------------------------------------------------
void MDBTObject::OnUnmanagedRecordDisposing (Object^ sender, EventArgs^ args)
{
	MSqlRecord^ mRecord = (MSqlRecord^)sender;
	if (mRecord == nullptr)
		return;

	/*System::Console::WriteLine("======================================================================================");
	System::Console::WriteLine("======================================================================================");

	for each (MSqlRecord^ aRecord in Records)
	{
		System::Console::WriteLine("----------------------------------------------------------------------------------");
		System::Console::WriteLine(aRecord->Name);
		System::Console::WriteLine(((System::IntPtr)aRecord->GetSqlRecord()).ToString());
		System::Console::WriteLine("----------------------------------------------------------------------------------");
	}

	System::Console::WriteLine("======================================================================================");
	System::Console::WriteLine("======================================================================================");*/

	Records->Remove(mRecord);
	delete mRecord;
}


//-----------------------------------------------------------------------------
MSqlTable^ MDBTObject::Table::get ()
{ 
	if (!GetDBTObject())
		return nullptr;
	
	if (table == nullptr)
		table = gcnew MSqlTable((System::IntPtr) GetDBTObject()->GetTable(), Record);
	
	return table;
}

//-----------------------------------------------------------------------------
::DataObj* MDBTObject::OnCheckPrimaryKey (SqlRecord* pRecord, int nRow)
{
	MSqlRecord^ record = GetRecord(pRecord);

	BadDataRowEventArgs^ eventArgs = gcnew BadDataRowEventArgs(nRow, record);
	CheckPrimaryKey (this, eventArgs);

	return eventArgs->BadData == nullptr ? NULL : eventArgs->BadData->GetDataObj(); 
}

//-----------------------------------------------------------------------------
void MDBTObject::BindAutoincrement(MDataObj^ data, String^ entity)
{
	if (GetDBTObject())
		GetDBTObject()->BindAutoincrement(data->GetDataObj(), CString(entity));
}

//-----------------------------------------------------------------------------
void MDBTObject::BindAutonumber (MDataObj^ data, String^ entity)
{
	if (GetDBTObject())
		GetDBTObject()->BindAutonumber(data->GetDataObj(), CString(entity));
}

//-----------------------------------------------------------------------------
void MDBTObject::BindAutonumber (MDataObj^ data, String^ entity, MDataDate^ date)
{
	if (GetDBTObject())
		GetDBTObject()->BindAutonumber(data->GetDataObj(), CString(entity), (::DataDate*) date->GetDataObj());
}

//-----------------------------------------------------------------------------
bool MDBTObject::IsToDelete()
{
	EasyBuilderComponent^ record = (EasyBuilderComponent^) this->Record;
	return __super::IsToDelete() && !record->IsChanged;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MDBTSlave Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
MDBTSlave::MDBTSlave ()
	:
	master (nullptr)
{ 
}

//-----------------------------------------------------------------------------
MDBTSlave::MDBTSlave (System::IntPtr dbtPtr)
	:
	master		(nullptr),
	MDBTObject	(dbtPtr)
{ 

}

//-----------------------------------------------------------------------------
MDBTSlave::~MDBTSlave ()
{ 
	this->!MDBTSlave();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MDBTSlave::!MDBTSlave ()
{ 
	delete table;
	if (!HasCodeBehind && GetDBTObject() && GetDBTObject()->GetDocument())
	{
		DBTMaster* pDBTMaster = GetDBTObject()->GetDocument()->m_pDBTMaster;
		if (!pDBTMaster)
			return;

		GetDBTObject()->Close();
		DBTArray* pDBTSlaves = pDBTMaster->GetDBTSlaves();
		pDBTSlaves->RemoveDBT((DBTSlave*) GetDBTObject());
		if (m_ppDBTObject)
			*m_ppDBTObject = NULL;
	}

	Master = nullptr;
}

//-----------------------------------------------------------------------------
MDBTSlave::MDBTSlave (System::String^ tableName, System::String^ dbtName, IDocumentDataManager^ document, bool hasCodeBehind)
{
	CAbstractFormDoc* pDoc = document == nullptr ? NULL : ((MDocument^) document)->GetDocument();
	if (!pDoc || !pDoc->GetMaster())
	{
		ASSERT(FALSE);
		return;
	}

	DBTObject* pDBTObject = pDoc->GetDBTByName(CString(dbtName));

	this->HasCodeBehind = hasCodeBehind;

	if (hasCodeBehind)
	{
		*m_ppDBTObject = pDBTObject;
		CTBNamespace aFullNs(pDoc->GetNamespace().ToString());
		aFullNs.SetChildNamespace(CTBNamespace::DBT, CString(dbtName), aFullNs);

		// da questa richiesta passa sia quando è non ancora istanziato,sia quando viene wrappato in edit dalla perosnalizzazione in memoria
		DeleteChangeRequest^ request = gcnew DeleteChangeRequest(Refactor::ChangeSubject::Class, gcnew NameSpace(gcnew String(aFullNs.ToString())), document->Namespace, this->Version);
		BOOL bInvalidDBT = BaseCustomizationContext::ApplicationChanges->IsDeletedObject(request);
		delete request;

		if (bInvalidDBT)
			((MDocument^)document)->AddInvalidDBT(aFullNs.ToString());

		if (!GetDBTObject())
		{
			if (bInvalidDBT)
				*m_ppDBTObject = DynDBTSlave::Create(tableName, pDoc, CString(dbtName), ALLOW_EMPTY_BODY);
			else
			{
				ASSERT(FALSE);
				Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("DBT with the name %s does not exists into the document!"), CString(dbtName))));
			}
		}
	}
	else
	{
		if (pDBTObject)
			Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("Trying to create duplicate DBTSlave! DBT %s already exists !"), CString(dbtName))));

		
		*m_ppDBTObject = DynDBTSlave::Create(tableName, pDoc, CString(dbtName), ALLOW_EMPTY_BODY);
	}

	AttachDefaultEvents();
}
//-----------------------------------------------------------------------------
MDBTObject^ MDBTSlave::CreateAndAttach(System::IntPtr dbtPtr)
{
	return gcnew MDBTSlave(dbtPtr);
}

//-----------------------------------------------------------------------------
DataRelationType MDBTSlave::Relation::get () 
{ 
	return DataRelationType::OneToOne ;
} 

//-----------------------------------------------------------------------------
List<MForeignKeyField^>^ MDBTSlave::MasterForeignKey::get ()
{ 
	if (this->masterForeignKey == nullptr)
		this->masterForeignKey = gcnew List<MForeignKeyField^>();
	
	return this->masterForeignKey;
} 

//-----------------------------------------------------------------------------
void MDBTSlave::MasterForeignKey::set (List<MForeignKeyField^>^ masterForeignKey)
{ 
	this->masterForeignKey = masterForeignKey;
} 

//-----------------------------------------------------------------------------
int MDBTSlave::PreloadStep::get ()
{
	return GetDBTObject() ? ((DBTSlave*) GetDBTObject())->GetPreloadStep() == TRUE: 0;
}

//-----------------------------------------------------------------------------
void MDBTSlave::PreloadStep::set (int value)
{
	if (GetDBTObject())
		((DBTSlave*) GetDBTObject())->SetPreloadStep(value);
}

//-----------------------------------------------------------------------------
bool MDBTSlave::AllowEmpty::get ()
{
	return GetDBTObject() ? ((DBTSlave*) GetDBTObject())->GetAllowEmpty() == TRUE: false;
}

//-----------------------------------------------------------------------------
void MDBTSlave::AllowEmpty::set (bool value)
{
	if (GetDBTObject())
		((DBTSlave*) GetDBTObject())->SetAllowEmpty(value);
}

//-----------------------------------------------------------------------------
bool MDBTSlave::OnlyDeleteAction::get ()
{
	return GetDBTObject() ? ((DBTSlave*) GetDBTObject())->GetOnlyDelete() == TRUE: false;
}

//-----------------------------------------------------------------------------
void MDBTSlave::OnlyDeleteAction::set (bool value)
{
	if (GetDBTObject())
		((DBTSlave*) GetDBTObject())->SetOnlyDelete(value);
}

//-----------------------------------------------------------------------------
DelayReadType MDBTSlave::ReadBehaviour::get ()
{
	if (!GetDBTObject())
		return DelayReadType::Immediate;

	DBTSlave::ReadType type = ((DBTSlave*) GetDBTObject())->GetDelayedReadType();
	
	return (DelayReadType)(int) type;
}

//-----------------------------------------------------------------------------
void MDBTSlave::ReadBehaviour::set (DelayReadType value)
{
	if (!GetDBTObject())
	{
		ASSERT(FALSE);
		return;
	}
	
	((DBTSlave*) GetDBTObject())->SetDelayedRead((DBTSlave::ReadType) (int) value);
}

//-----------------------------------------------------------------------------
IDocumentMasterDataManager^ MDBTSlave::Master::get ()
{
	return master;
}

//-----------------------------------------------------------------------------
void MDBTSlave::Master::set (IDocumentMasterDataManager^ value)
{
	master = value;
}

//-----------------------------------------------------------------------------
void MDBTSlave::DefineQuery (MSqlTable^ mSqlTable)
{
	//no mor code needed here: moved to DynDBT
	__super::DefineQuery(mSqlTable);
}

//-----------------------------------------------------------------------------
void MDBTSlave::PrepareQuery (MSqlTable^ mSqlTable)
{
	//no mor code needed here: moved to DynDBT
	
	__super::PrepareQuery(Table);
}

//-----------------------------------------------------------------------------
void MDBTSlave::AddMasterForeignKey(MDataObj^ dataObj, MDataObj^ masterdataObj)
{
	IRecordField^ field = record->GetField(dataObj);
	IRecordField^ masterField = Master->Record->GetField(masterdataObj);

	AddMasterForeignKey(field, masterField);
}

//-----------------------------------------------------------------------------
void MDBTSlave::AddMasterForeignKey (MDataObj^ dataObj, System::String^ masterFieldName)
{
	IRecordField^ field			= record->GetField(dataObj);
	IRecordField^ masterField	= Master->Record->GetField(masterFieldName);

	AddMasterForeignKey(field, masterField);
}

//-----------------------------------------------------------------------------
void MDBTSlave::AddMasterForeignKey (IRecordField^ field, IRecordField^ masterField)
{
	if (field == nullptr || masterField == nullptr)
	{
		System::String^ errorInKey = gcnew System::String(_TB("Error declaring {0} foreign key. Foreign Key field {1} is not declared in {2} record!"));
	
		if (field == nullptr)
			Diagnostic->SetError(System::String::Format(errorInKey, this->SerializedName, "", record->Name));
		else
			Diagnostic->SetError(System::String::Format(errorInKey, this->SerializedName, "", Master->Record));

		return;
	}

	MasterForeignKey->Add(gcnew MForeignKeyField(field->Name, masterField->Name, record->Name, Master->Record->Name));

	//add information to underlying native dbt if dynamic
	AddMasterForeignKey(field->Name, masterField->Name);
}
//-----------------------------------------------------------------------------
void MDBTSlave::AddMasterForeignKey(CString sPrimary, CString sForeign)
{
	DBTObject* pDBT = GetDBTObject();
	if (pDBT && pDBT->IsKindOf(RUNTIME_CLASS(DynDBTSlave)))
	{
		((DynDBTSlave*)pDBT)->GetQuery()->AddForeignKey(sPrimary, sForeign);
	}
}
//-----------------------------------------------------------------------------
bool MDBTSlave::ContainsMasterForeignKey(System::String^ fieldName)
{
	// default primary key algorithm
	for each (MForeignKeyField^ fkfield in MasterForeignKey)
	{
		if (System::String::Compare(fkfield->PrimaryKey, fieldName, StringComparison::OrdinalIgnoreCase) == 0)
			return true;
	}
	return false;
}

//-----------------------------------------------------------------------------
void MDBTSlave::ReceivePreparePrimaryKey (int /*nRow*/, System::IntPtr /*recordPtr*/)
{
	PerformDefaultPreparePrimaryKey ((MSqlRecord^) record);
	this->PrimaryKeyPrepared (this, EasyBuilderEventArgs::Empty);
}

//-----------------------------------------------------------------------------
void MDBTSlave::PerformDefaultPreparePrimaryKey (MSqlRecord^ record)
{
	if (!GetDBTObject() || HasCodeBehind || MasterForeignKey->Count == 0 || record == nullptr)
		return;

	// default primary key algorithm
	for each (MForeignKeyField^ fkfield in MasterForeignKey)
	{
		// cerco il dataObj da assegnare
		IRecordField^ field =  record->GetField(fkfield->PrimaryKey);
		if (field == nullptr)
			return;

		MDataObj^ mDataObj = (MDataObj^) field->DataObj;
		if (mDataObj == nullptr || mDataObj->DataObjPtr == System::IntPtr::Zero)
			continue;
			
		::DataObj* pDataObj = (::DataObj*) mDataObj->DataObjPtr.ToInt64();
		
		*mDataObj->GetDataObj() = *((DBTSlave*)GetDBTObject())->GetMasterRecord()->GetDataObjFromColumnName(CString(fkfield->ForeignKey));

	}
}

//-----------------------------------------------------------------------------
bool MDBTSlave::IsInForeignKey (System::String^ fieldName)
{
	for each (MForeignKeyField^ field in MasterForeignKey)
		if (field->IsInForeignKey(fieldName))
			return true;

	return false;
}

//-----------------------------------------------------------------------------
void MDBTSlave::AssignInternalState(MGenericDataManager^ source)
{
	__super::AssignInternalState(source);
	MDBTSlave^ sl =(MDBTSlave^)source;
	HasCodeBehind = sl->HasCodeBehind;
	for each (MForeignKeyField^ fkfield in sl->MasterForeignKey)
		MasterForeignKey->Add(fkfield);

	PrimaryKeyPrepared += sl->delegatePrimaryKeyPrepared;
}

//-----------------------------------------------------------------------------
System::Collections::Generic::IList<IRecordField^>^ MDBTSlave::GetNonForeignKeyFields()
{
	System::Collections::Generic::IList<IRecordField^>^ records =
		gcnew System::Collections::Generic::List<IRecordField^>();

	for each (IRecordField^ field in Record->Fields)
	{
		if (!field->IsSegmentKey || !ContainsMasterForeignKey(field->Name))
		{
			records->Add(field);
		}
	}

	return records;
}

//-----------------------------------------------------------------------------
void MDBTSlave::InitializeForeignKeys()
{

}

/////////////////////////////////////////////////////////////////////////////
// 				Event args Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
RowEventArgs::RowEventArgs(Int16 nRow, IRecord^ record)
{
	this->nRow = nRow;
	this->record = record;
	this->cancel = false;
}

//-----------------------------------------------------------------------------
Int16	RowEventArgs::RowNumber::get ()
{
	return nRow;
}

//-----------------------------------------------------------------------------
void RowEventArgs::RowNumber::set (Int16 nRow)
{
	this->nRow = nRow;
}

//-----------------------------------------------------------------------------
IRecord^ RowEventArgs::Record::get ()
{
	return record;
}

//-----------------------------------------------------------------------------
void RowEventArgs::Record::set (IRecord^ record)
{
	this->record = record;
}

//-----------------------------------------------------------------------------
bool RowEventArgs::Cancel::get ()
{
	return cancel;
}

//-----------------------------------------------------------------------------
void RowEventArgs::Cancel::set (bool returnValue)
{
	this->cancel = returnValue;
}

//-----------------------------------------------------------------------------
BadDataRowEventArgs::BadDataRowEventArgs(Int16 nRow, IRecord^ record) 
	: 
	RowEventArgs(nRow, record), 
	badData		(nullptr) 
{
}

//-----------------------------------------------------------------------------
MDataObj^ BadDataRowEventArgs::BadData::get ()
{
	return badData;
}

//-----------------------------------------------------------------------------
void BadDataRowEventArgs::BadData::set (MDataObj^ value)
{
	this->badData = value;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MDBTSlaveBuffered Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDBTSlaveBuffered::MDBTSlaveBuffered (System::IntPtr dbtPtr) 	
	:
	MDBTSlave				(dbtPtr),
	isDuplicateKeyCallBack	(nullptr)
{ 
	prototypes = gcnew List<IDocumentSlaveDataManager^>();
}

//-----------------------------------------------------------------------------
MDBTSlaveBuffered::MDBTSlaveBuffered (System::String^ tableName, System::String^ dbtName, IDocumentDataManager^ document, bool hasCodeBehind)
	:
	isDuplicateKeyCallBack	(nullptr)
{
	prototypes = gcnew List<IDocumentSlaveDataManager^>();
	CAbstractFormDoc* pDoc = document == nullptr ? NULL : ((MDocument^) document)->GetDocument();
	if (!pDoc || !pDoc->GetMaster())
	{
		ASSERT(FALSE);
		return;
	}

	DBTObject* pDBTObject = pDoc->GetDBTByName (CString(dbtName));

	if (dbtName == "CustomerListsDetails")
		int i = 0;

	this->HasCodeBehind = hasCodeBehind;
	if (hasCodeBehind)
	{
		*m_ppDBTObject = pDBTObject;
		CTBNamespace aFullNs(pDoc->GetNamespace().ToString());
		aFullNs.SetChildNamespace(CTBNamespace::DBT, CString(dbtName), aFullNs);

		// da questa richiesta passa sia quando è non ancora istanziato,sia quando viene wrappato in edit dalla perosnalizzazione in memoria
		DeleteChangeRequest^ request = gcnew DeleteChangeRequest(Refactor::ChangeSubject::Class, gcnew NameSpace(gcnew String(aFullNs.ToString())), document->Namespace, this->Version);
		BOOL bInvalidDBT = BaseCustomizationContext::ApplicationChanges->IsDeletedObject(request);
		delete request;

		if (bInvalidDBT)
			((MDocument^)document)->AddInvalidDBT(aFullNs.ToString());
		
		if (!GetDBTObject())
		{
			if (bInvalidDBT)
				*m_ppDBTObject = DynDBTSlaveBuffered::Create(tableName, pDoc, CString(dbtName), ALLOW_EMPTY_BODY, CHECK_DUPLICATE_KEY);
			else
			{
				ASSERT(FALSE);
				Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("DBT with the name %s does not exists into the document!"), CString(dbtName))));
			}
		}
	}
	else
	{
		if (pDBTObject)
			Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("Trying to create duplicate DBTSlaveBuffered! DBT %s already exists !"), CString(dbtName))));

		*m_ppDBTObject = DynDBTSlaveBuffered::Create(tableName, pDoc, CString(dbtName), ALLOW_EMPTY_BODY, CHECK_DUPLICATE_KEY);
	}

	AttachDefaultEvents();
}

//-----------------------------------------------------------------------------
MDBTSlaveBuffered::~MDBTSlaveBuffered	()
{
	this->!MDBTSlaveBuffered ();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MDBTSlaveBuffered::!MDBTSlaveBuffered	()
{
	if (isDuplicateKeyHandle.IsAllocated)
		isDuplicateKeyHandle.Free();

	if (slaves != nullptr)
		for each (MDBTSlave^ sl in slaves) 
			delete sl;
}

//-----------------------------------------------------------------------------
MDBTObject^ MDBTSlaveBuffered::CreateAndAttach(System::IntPtr dbtPtr)
{
	return gcnew MDBTSlaveBuffered(dbtPtr);
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::AttachDefaultEvents()
{
	if (!GetDBTObject())
		return;

	__super::AttachDefaultEvents();

	DBTSlaveBuffered* pBuffered = (DBTSlaveBuffered*)GetDBTObject();

	if (isDuplicateKeyHandle.IsAllocated)
		isDuplicateKeyHandle.Free();

	isDuplicateKeyCallBack = gcnew IsDuplicateKeyCallBack(this, &MDBTSlaveBuffered::OnDefaultIsDuplicateKey);
	isDuplicateKeyHandle = GCHandle::Alloc(isDuplicateKeyCallBack);
	IntPtr funPtrKey = Marshal::GetFunctionPointerForDelegate(isDuplicateKeyCallBack);
	pBuffered->SetDuplicateKeyFunPtr(static_cast<DATAOBJ_ROW_FUNC>(funPtrKey.ToPointer()));
}

//-----------------------------------------------------------------------------
bool MDBTSlaveBuffered::IsValid ()
{ 
	return record != nullptr && record->IsValid; 
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::SetCurrentRowForValueChanged(int nRow)
{
	DBTSlaveBuffered* pDBTSlaveBuffered = (DBTSlaveBuffered*)GetDBTObject();
	if (pDBTSlaveBuffered)
	{
		pDBTSlaveBuffered->SetCurrentRowForValueChanged(nRow);
	}
}

//-----------------------------------------------------------------------------
bool MDBTSlaveBuffered::Modified::get ()
{
	if (!GetDBTObject())	
		return false;	
	
	return ((DBTSlaveBuffered*) GetDBTObject())->IsModified() == TRUE; 
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::Modified::set (bool value)
{
	if (GetDBTObject())
	{
		((DBTSlaveBuffered*) GetDBTObject())->SetModified(value == TRUE); 
		if (GetDBTObject()->GetDocument())
			GetDBTObject()->GetDocument()->SetModifiedFlag(value == TRUE);
	}
}

//-----------------------------------------------------------------------------
int MDBTSlaveBuffered::CurrentRow::get ()
{
	return GetDBTObject() ? ((DBTSlaveBuffered*) GetDBTObject())->GetCurrentRowIdx() : -1;

}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::CurrentRow::set (int value)
{
	if (GetDBTObject())
	{
		((DBTSlaveBuffered*) GetDBTObject())->SetCurrentRow(value);
	}
}


//-----------------------------------------------------------------------------
bool MDBTSlaveBuffered::ReadOnly::get ()
{
	return GetDBTObject() ? ((DBTSlaveBuffered*) GetDBTObject())->IsReadOnly() == TRUE: false;
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::ReadOnly::set (bool value)
{
	if (GetDBTObject())
	{
		((DBTSlaveBuffered*) GetDBTObject())->SetReadOnly(value);
		ReadOnlyChanged(this, EasyBuilderEventArgs::Empty);
	}
}

//-----------------------------------------------------------------------------
bool MDBTSlaveBuffered::CheckDuplicateKey::get ()
{
	return GetDBTObject() ? ((DBTSlaveBuffered*) GetDBTObject())->IsCheckingDuplicateKey() == TRUE: false;
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::CheckDuplicateKey::set (bool value)
{
	if (GetDBTObject())
		((DBTSlaveBuffered*) GetDBTObject())->SetCheckDuplicateKey(value);
}

//-----------------------------------------------------------------------------
DataRelationType MDBTSlaveBuffered::Relation::get () 
{ 
	return DataRelationType::OneToMany;
} 

//-----------------------------------------------------------------------------
bool MDBTSlaveBuffered::DeleteRow (int nRow)
{  
	if (!GetDBTObject())	
		return false;	
	
	return ((DBTSlaveBuffered*) GetDBTObject())->DeleteRecord(nRow) == TRUE; 
}


//-----------------------------------------------------------------------------
MSqlRecord^ MDBTSlaveBuffered::AddRecord ()
{  
	if (!GetDBTObject())	
		return nullptr;		
	
	SqlRecord* pRecord = ((DBTSlaveBuffered*) GetDBTObject())->AddRecord();
	pRecord->SetStorable(TRUE);
	
	CurrentRow = RowsCount - 1;
	
	return GetRecord(pRecord); 
}

//-----------------------------------------------------------------------------
MSqlRecord^	 MDBTSlaveBuffered::InsertRecord (int nRow)
{
	if (!GetDBTObject())	
		return nullptr;		
	
	SqlRecord* pRecord = ((DBTSlaveBuffered*) GetDBTObject())->InsertRecord(nRow); 
	pRecord->SetStorable(TRUE);

	CurrentRow = nRow;

	return GetRecord(pRecord); 
}

//-----------------------------------------------------------------------------
MSqlRecord^	 MDBTSlaveBuffered::GetRecord (int nRow)
{
	SqlRecord* pRecord = ((DBTSlaveBuffered*) GetDBTObject())->GetRow(nRow); 
	return GetRecord(pRecord); 
}

//-----------------------------------------------------------------------------
MSqlRecord^	 MDBTSlaveBuffered::GetOldRecord (int nRow)
{
	SqlRecord* pRecord = ((DBTSlaveBuffered*) GetDBTObject())->GetOldRow(nRow); 
	return GetRecord(pRecord); 
}

//-----------------------------------------------------------------------------
int MDBTSlaveBuffered::RowsCount::get ()
{
	 return GetDBTObject() ? ((DBTSlaveBuffered*) GetDBTObject())->GetSize() : 0; 
}

//-----------------------------------------------------------------------------
int	 MDBTSlaveBuffered::OldRowsCount::get ()
{
	return GetDBTObject() ? ((DBTSlaveBuffered*) GetDBTObject())->GetOldSize() : 0; 
}

//-----------------------------------------------------------------------------
MSqlRecord^ MDBTSlaveBuffered::GetCurrentRecord ()
{
	SqlRecord* pRecord = ((DBTSlaveBuffered*) GetDBTObject())->GetCurrentRow(); 
	return GetRecord(pRecord); 
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::AttachSlave(IDocumentSlaveDataManager^ dbtSlave)
{
	MDBTSlave^ dbt = (MDBTSlave^) dbtSlave;
	if (!dbt->GetDBTObject())
		return;

	if (!dbtSlave->HasCodeBehind)
		((DBTSlaveBuffered*)GetDBTObject())->Attach((DBTSlave*) dbt->GetDBTObject());

	this->prototypes->Add(dbtSlave);

	Remove(dbt);
	Add(dbt);

	dbtSlave->Master = this;

	int foreignKeysCount = dbt->MasterForeignKey->Count;
	dbt->InitializeForeignKeys();
	AddReferencedBy(dbt->SerializedName);
}

//-----------------------------------------------------------------------------
MDBTObject^ MDBTSlaveBuffered::GetDBT(System::String^ name)
{
	for each(IDocumentSlaveDataManager^ slave in prototypes)
	{
		if (System::String::Compare(slave->Name, name, true) == 0)
			return (MDBTObject^) slave;
		if (MDBTSlaveBuffered::typeid->IsInstanceOfType(slave))
		{
			MDBTObject^ p = ((MDBTSlaveBuffered^) slave)->GetDBT(name);
			if (p != nullptr)
				return p;
		}
	}
	return nullptr;
}
//-----------------------------------------------------------------------------
MDBTObject^ MDBTSlaveBuffered::GetDBT(INameSpace^ nameSpace)
{
	for each(IDocumentSlaveDataManager^ slave in prototypes)
	{
		if (System::String::Compare(slave->Namespace->FullNameSpace, nameSpace->FullNameSpace, true) == 0)
			return (MDBTObject^) slave;
		if (MDBTSlaveBuffered::typeid->IsInstanceOfType(slave))
		{
			MDBTObject^ p = ((MDBTSlaveBuffered^) slave)->GetDBT(nameSpace);
			if (p != nullptr)
				return p;
		}
	}
	return nullptr;
}


//-----------------------------------------------------------------------------
MDBTSlave^ MDBTSlaveBuffered::GetDBTSlave(System::String^ name, int idx)
{
	DBTSlaveBuffered* pBuffered = (DBTSlaveBuffered*) GetDBTObject();
	if (!pBuffered)
		return nullptr;

	DBTSlave* pDBTSlave = pBuffered->GetDBTSlave(CString(name), idx, TRUE);
	if (!pDBTSlave)
		return nullptr;

	MDBTObject^ dbt = GetDBT((IntPtr) pDBTSlave);
	
	return (MDBTSlave^) dbt;
}
//-----------------------------------------------------------------------------
MDBTSlave^ MDBTSlaveBuffered::GetCurrentSlave(System::String^ name)
{
	DBTSlaveBuffered* pBuffered = (DBTSlaveBuffered*) GetDBTObject();
	if (!pBuffered)
		return nullptr;

	DBTSlave* pDBTSlave = pBuffered->GetCurrentDBTSlave(CString(name));

	MDBTObject^ dbt = GetDBT((IntPtr) pDBTSlave);
	
	return (MDBTSlave^) dbt;
}


//-----------------------------------------------------------------------------
MDBTSlave^ MDBTSlaveBuffered::GetCurrentSlave()
{
	DBTSlaveBuffered* pBuffered = (DBTSlaveBuffered*) GetDBTObject();
	if (!pBuffered)
		return nullptr;

	DBTSlave* pDBTSlave = pBuffered->GetCurrentDBTSlave(_T(""));

	MDBTObject^ dbt = GetDBT((IntPtr) pDBTSlave);
	
	return (MDBTSlave^) dbt;
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::AssignInternalState(MGenericDataManager^ source)
{
	__super::AssignInternalState(source);
	MDBTSlaveBuffered^ sl =(MDBTSlaveBuffered^)source;
	PrimaryKeyPrepared += sl->delegatePrimaryKeyPrepared;
	RowPrepared += sl->delegateRowPrepared;
	AddingRow += sl->delegateAddingRow;
	RowAdded += sl->delegateRowAdded;
	RowInserted += sl->delegateRowInserted;
	DeletingRow += sl->delegateDeletingRow;
	RowDeleted += sl->delegateRowDeleted;
	CurrentRowChanged += sl->delegateCurrentRowChanged;
	AuxColumnsPrepared += sl->delegateAuxColumnsPrepared;
}

//-----------------------------------------------------------------------------
MDBTObject^ MDBTSlaveBuffered::GetDBT(System::IntPtr dbtPtr)
{
	DBTObject* p = (DBTObject*)dbtPtr.ToInt64();
	for each(MDBTObject^ slave in prototypes)
	{
		
		if (((MDBTObject^)slave)->GetDBTObject()->GetNamespace() == p->GetNamespace())
		{
			if (slave->GetDBTObject() == p)
				return slave;

			if (slaves == nullptr)
				slaves = gcnew List<MDBTSlave^>();

			for (int i = slaves->Count - 1; i >= 0; i-- )
			{
				MDBTSlave^ sl = slaves[i]; 
				if (sl->GetDBTObject() == p)
					return sl;
				if (sl->GetDBTObject() == NULL)
				{
					slaves->RemoveAt(i);
					Remove((IComponent^) sl);
			
					delete sl;
				}
			}
			MDBTSlave^ newSlave = (MDBTSlave^)((MDBTObject^)slave)->CreateAndAttach(dbtPtr);
			newSlave->AssignInternalState((MDBTObject^)slave);
			slaves->Add(newSlave);
			Add((IComponent^) newSlave);
			newSlave->Master = this;

			return newSlave;
		}
		if (MDBTSlaveBuffered::typeid->IsInstanceOfType(slave))
		{
			MDBTObject^ p = ((MDBTSlaveBuffered^) slave)->GetDBT(dbtPtr);
			if (p != nullptr)
				return p;
		}
	}
	return nullptr;
}
//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::ReceivePreparePrimaryKey (int nRow, System::IntPtr recordPtr)
{
	if (recordPtr == System::IntPtr::Zero)
		return;

	SqlRecord* pRecord = (SqlRecord*) recordPtr.ToInt64();
	
	MSqlRecord^ record = GetRecord(pRecord);
	PerformDefaultPreparePrimaryKey (record);
	this->PrimaryKeyPrepared (this, gcnew RowEventArgs(nRow, record));
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::ReceivePrepareRow (int nRow, System::IntPtr recordPtr)
{
	if (recordPtr == System::IntPtr::Zero)
		return;

	SqlRecord* pRecord = (SqlRecord*) recordPtr.ToInt64();
	this->RowPrepared (this, gcnew RowEventArgs(nRow, GetRecord(pRecord)));
}

//-----------------------------------------------------------------------------
bool MDBTSlaveBuffered::ReceiveBeforeAddRow (int nRow)
{
	RowEventArgs^ eventArgs = gcnew RowEventArgs(nRow, nullptr);
	this->AddingRow(this, eventArgs);
	return !eventArgs->Cancel;
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::ReceiveAfterAddRow (int nRow, System::IntPtr recordPtr)
{
	if (recordPtr == System::IntPtr::Zero)
		return;

	SqlRecord* pRecord = (SqlRecord*) recordPtr.ToInt64();
	this->RowAdded (this, gcnew RowEventArgs(nRow, GetRecord(pRecord)));
}


//-----------------------------------------------------------------------------
bool MDBTSlaveBuffered::ReceiveBeforeInsertRow (int nRow)
{
	RowEventArgs^ eventArgs = gcnew RowEventArgs(nRow, nullptr);
	this->InsertingRow (this, eventArgs);
	return !eventArgs->Cancel;
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::ReceiveAfterInsertRow (int nRow, System::IntPtr recordPtr)
{
	if (recordPtr == System::IntPtr::Zero)
		return;

	SqlRecord* pRecord = (SqlRecord*) recordPtr.ToInt64();
	this->RowInserted (this, gcnew RowEventArgs(nRow, GetRecord(pRecord)));
}

//-----------------------------------------------------------------------------
bool MDBTSlaveBuffered::ReceiveBeforeDeleteRow (int nRow, System::IntPtr recordPtr)
{
	if (recordPtr == System::IntPtr::Zero)
		return true;

	SqlRecord* pRecord = (SqlRecord*) recordPtr.ToInt64();
	RowEventArgs^ eventArgs = gcnew RowEventArgs(nRow, GetRecord(pRecord));
	this->DeletingRow (this, eventArgs);

	return !eventArgs->Cancel;
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::ReceiveAfterDeleteRow (int nRow)
{
	this->RowDeleted (this, gcnew RowEventArgs(nRow, nullptr));
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::ReceiveSetCurrentRow (int nRow)
{
	SqlRecord* pRecord = ((DBTSlaveBuffered*)GetDBTObject())->GetRow(nRow);
	MSqlRecord^ record = pRecord ? GetRecord(pRecord) : (MSqlRecord^)Record;
	this->CurrentRowChanged (this, gcnew RowEventArgs(nRow, record));
	
	
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::SetCurrentRowByRecord (MSqlRecord^ record)
{
	int index = ((DBTSlaveBuffered*)GetDBTObject())->FindRecordIndex(record->GetSqlRecord());
	ASSERT(index != -1);
	CurrentRow = index;
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::ReceivePrepareAuxColumns (int nRow, System::IntPtr recordPtr)
{
	if (recordPtr == System::IntPtr::Zero)
		return;
	
	SqlRecord* pRecord = (SqlRecord*) recordPtr.ToInt64();
	this->AuxColumnsPrepared (this, gcnew RowEventArgs(nRow, GetRecord(pRecord)));
}

//-----------------------------------------------------------------------------
::DataObj* MDBTSlaveBuffered::OnDefaultIsDuplicateKey (SqlRecord* pRecord, int nRow)
{
	if (!CheckDuplicateKey)
		return nullptr;

	// controllo di default che skippa la duplicazione di foreign key
	MSqlRecord^ record = GetRecord(pRecord);
	for each (IRecordField^ pkfield in record->PrimaryKeyFields)
	{
		if (!IsInForeignKey(pkfield->Name))
		{
			
			MDataObj^ dataObj = ((MDataObj^) pkfield->DataObj);
			if (dataObj == nullptr || dataObj->Value == nullptr)
				continue;

			GetDBTObject()->SetDBTError(cwsprintf(_TB("Duplicated Primary Key!\r\nValue %s contained in field %s is duplicated with another row!"), CString(dataObj->Value->ToString()), CString(pkfield->Name)));
			return dataObj->GetDataObj();
		}
	}

	return nullptr;
}

//-----------------------------------------------------------------------------
bool MDBTSlaveBuffered::IsInForeignKey (System::String^ fieldName)
{	
	for each (MForeignKeyField^ field in MasterForeignKey)
		if (field->IsInForeignKey(fieldName))
			return true;

	return false;
}

//-----------------------------------------------------------------------------
MSqlRecord^	MDBTSlaveBuffered::GetRecord (SqlRecord* pRecord)
{
	return GetRecord(pRecord, true);
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::Clear ()
{
	if (!GetDBTObject())
		return;

	// seguo il criterio di cancellarne una per una 
	// in modo che scattino tutti gli eventi giusti
	for (int i= RowsCount - 1; i >= 0; i--)
		DeleteRow(i);
}

//-----------------------------------------------------------------------------
Object^ MDBTSlaveBuffered::GetMaxValueOf (MDataObj^ mDataObj)
{
	return GetValueOf(mDataObj, 0);
}

//-----------------------------------------------------------------------------
Object^ MDBTSlaveBuffered::GetMinValueOf (MDataObj^ mDataObj)
{
	return GetValueOf(mDataObj, 1);
}

//-----------------------------------------------------------------------------
Object^ MDBTSlaveBuffered::GetSumValueOf (MDataObj^ mDataObj)
{
	return GetValueOf(mDataObj, 2);
}

// valueType: 0 = MAX;  1 = MIN; 2 = SUM
//-----------------------------------------------------------------------------
Object^ MDBTSlaveBuffered::GetValueOf (MDataObj^ mDataObj, int valueType)
{
	if (!GetDBTObject() || !GetDBTObject()->GetRecord() || !mDataObj || !mDataObj->GetDataObj())
		return nullptr;

	::DataObj* pDataObj = mDataObj->GetDataObj();
	::DataObj* pRetValue = pDataObj->Clone();
	
	DBTSlaveBuffered* pBuffered = (DBTSlaveBuffered*) GetDBTObject();
	int nIdx = GetDBTObject()->GetRecord()->GetIndexFromDataObj(pDataObj);
	if (nIdx < 0)
		return nullptr;
	
	pRetValue->Clear();
	for (int i=0; i < pBuffered->GetSize();  i++)
	{
		::DataObj* pDataObj = pBuffered->GetRow(i)->GetAt(nIdx)->GetDataObj();
		
		switch (valueType)
		{
		case 0: // MAX
			if (pRetValue->IsLessThan(*pDataObj))
				*pRetValue = *pDataObj;
			break;
		case 1: // MIN
			if (pRetValue->IsEmpty() || pDataObj->IsLessThan(*pRetValue))
				*pRetValue = *pDataObj;
			break;
		case 2: // SUM
				// essendo i due tipi siano eterogeneri tra loro se sono
				// compatibili, l'operatore di cast dovrebbe mettermi in sicurezza
				if (pDataObj->IsKindOf(RUNTIME_CLASS(DataStr)))
					*((DataStr*) pRetValue) += *((DataStr*) pDataObj);

				if (pDataObj->IsKindOf(RUNTIME_CLASS(DataInt)))
					*((DataInt*) pRetValue) += *((DataInt*) pDataObj);

				if (pDataObj->IsKindOf(RUNTIME_CLASS(DataLng)))
					*((DataLng*) pRetValue) += *((DataLng*) pDataObj);

				if (pDataObj->IsKindOf(RUNTIME_CLASS(DataDbl)))
					*((DataDbl*) pRetValue) += *((DataDbl*) pDataObj);

				if (pDataObj->IsKindOf(RUNTIME_CLASS(DataDate)))
					*((DataDate*) pRetValue) += *((DataDate*) pDataObj);
				break;
		default:
			break;
		}
	}

	// si deve autodistruggere quando servirà
	MDataObj^ retValue = MDataObj::Create(pRetValue); 
	retValue->HasCodeBehind = false;
	return retValue->Value;
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::Sort(System::String^ orderBy)
{
	::DBTSlaveBuffered* pDbtSlaveBuffered = (::DBTSlaveBuffered*)GetDBTObject ();
	if (!pDbtSlaveBuffered)
	{
		return;
	}

	pDbtSlaveBuffered->MemorySort(CString(orderBy));
}

//-----------------------------------------------------------------------------
int MDBTSlaveBuffered::GetRecordIndex(MSqlRecord^ record)
{
	::DBTSlaveBuffered* pDbtSlaveBuffered = (::DBTSlaveBuffered*)GetDBTObject ();
	if (!pDbtSlaveBuffered)
	{
		return -1;
	}

	return pDbtSlaveBuffered->GetRecords()->FindPtr(record->GetSqlRecord());
}

//-----------------------------------------------------------------------------
bool MDBTSlaveBuffered::LoadMoreRows(int preloadStep)
{
	DBTSlaveBuffered* pDbt = (DBTSlaveBuffered*)GetDBTObject();
	if (!pDbt)
	{ 
		return false; 
	}

	return pDbt->LoadMoreRows(preloadStep) == TRUE;
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::AddMasterForeignKey(CString sPrimary, CString sForeign)
{
	DBTObject* pDBT = GetDBTObject();
	if (pDBT && pDBT->IsKindOf(RUNTIME_CLASS(DynDBTSlaveBuffered)))
	{
		((DynDBTSlaveBuffered*)pDBT)->GetQuery()->AddForeignKey(sPrimary, sForeign);
	}
}

//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::OnRecordAdded(SqlRecord* pRecord, int nRow)
{
	MSqlRecord^ record = GetRecord(pRecord);
	RowEventArgs^ args = gcnew RowEventArgs(nRow, record);
	
	//RecordAdded(this, args);
	
	if (!Rows->Contains(record))
	{
		if (nRow < RowsCount - 1)
			Rows->Insert(nRow, record);
		else
			Rows->Add(record);
	}
}
//-----------------------------------------------------------------------------
void MDBTSlaveBuffered::OnRemovingRecord(SqlRecord* pRecord, int nRow)
{
	MSqlRecord^ record = GetRecord(pRecord);
	RowEventArgs^ args = gcnew RowEventArgs(nRow, record);
//	RemovingRecord(this, args);
	
	if (Rows->Contains(record))
		Rows->Remove(record);
}

//-----------------------------------------------------------------------------
MSqlRecord^ MDBTSlaveBuffered::FindRecord (System::String^ columnName, MDataObj^ value, int startPos/* = 0*/)
{
	DBTSlaveBuffered* pDbt = (DBTSlaveBuffered*)GetDBTObject();
	if (!pDbt)
	{
		return nullptr; 
	}

	return GetRecord(pDbt->FindRecord(CString(columnName), (::DataObj*)value->DataObjPtr.ToInt64(), startPos));
}

//-----------------------------------------------------------------------------
MSqlRecord^ MDBTSlaveBuffered::FindRecord (array<System::String^>^ columnNames, array<MDataObj^>^ values, int startPos /*= 0*/)
{
	DBTSlaveBuffered* pDbt = (DBTSlaveBuffered*)GetDBTObject();
	if (!pDbt)
	{
		return nullptr; 
	}
	if (columnNames->Length != values->Length)
	{
		return nullptr; 
	}

	CStringArray arNames;
	DataObjArray arValues; arValues.SetOwns(FALSE);

	for (int i = 0; i < values->Length; i++)
	{
		arNames.Add(CString(columnNames[i]));
		arValues.Add((::DataObj*)values[i]->DataObjPtr.ToInt64());
	}

	return GetRecord(pDbt->FindRecord(arNames, arValues, startPos));
}

//-----------------------------------------------------------------------------
System::Collections::IList^ MDBTSlaveBuffered::Rows::get()
{ 
	if (rows == nullptr)
		rows = gcnew RecordBindingList<MSqlRecord^>();
	
	return rows; 
} 




/////////////////////////////////////////////////////////////////////////////
// 				class MDBTMaster Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDBTMaster::MDBTMaster (System::IntPtr dbtPtr) 
	: 
	MDBTObject(dbtPtr)
{
}

//-----------------------------------------------------------------------------
MDBTMaster::MDBTMaster (System::String^ tableName, System::String^ dbtName, IDocumentDataManager^ document, bool hasCodeBehind)
{
	CAbstractFormDoc* pDoc = document == nullptr ? NULL : ((MDocument^) document)->GetDocument();
	if (!pDoc)
	{
		ASSERT(FALSE);
		return;
	}

	DBTObject* pDBTObject = pDoc->GetDBTByName (CString(dbtName));

	this->HasCodeBehind = hasCodeBehind;
	if (hasCodeBehind)
	{
		*m_ppDBTObject = pDBTObject;
		if (!GetDBTObject())
		{
			ASSERT(FALSE);
			Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("DBT with the name %s does not exists into the document!"), dbtName)));
		}
	}
	else
	{
		if (pDoc->GetMaster())
		{
			ASSERT(FALSE);
			Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("Trying to create duplicate DBT Master! This document has already a DBT Master attached!"), dbtName)));
		}

		if (pDBTObject)
		{
			ASSERT(FALSE);
			Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("Trying to create duplicate DBT! DBT %s already exists !"), dbtName)));
		}

		*m_ppDBTObject = DynDBTMaster::Create(tableName, pDoc, CString(dbtName));
	}
	AttachDefaultEvents();
}

//-----------------------------------------------------------------------------
MDBTMaster::~MDBTMaster()
{
	this->!MDBTMaster();
	System::GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MDBTMaster::!MDBTMaster()
{
	delete table;

	CAbstractFormDoc* pDoc = NULL;
	MDocument^ document = (MDocument^) Document;
	if (!HasCodeBehind && GetDBTObject() && (pDoc = GetDBTObject()->GetDocument()))
	{
		if (document)
		{
			for each (IComponent^ component in document->Components)
			{
				IDocumentSlaveDataManager^ slave = dynamic_cast<IDocumentSlaveDataManager^>(component);
				if (slave == nullptr)
					continue;

				document->Remove((IComponent^)slave);
				delete slave;
			}
		}
		GetDBTObject()->Close();

		pDoc->m_pDBTMaster = NULL;
		delete GetDBTObject();
	}
}

//-----------------------------------------------------------------------------
System::String^	MDBTMaster::BrowserQuery::get ()
{
	return browserQuery;
}

//-----------------------------------------------------------------------------
void MDBTMaster::BrowserQuery::set (System::String^ value)
{
	//controllo skippato per motivi di performance e di coerenza dell'oggetto: se sono
	//nella createcomponents potrei non avere terminato l'inizializzazione e quindi non avere informazioni
	//sufficienti per il controllo (comunque un controllo di validità viene fatto in fase di editing)
	/*if (browserQuery != value 
		&& !System::String::IsNullOrEmpty(value) 
		&& GetDBTObject()
		&& GetDBTObject()->GetDocument()->m_pBrowser)
	{
		CString strError;
		WClause * pClause = CreateValidWhereClause(GetDBTObject()->GetDocument()->m_pBrowser->GetTable(), value, strError);
		if (!pClause)
		{
			Diagnostic->SetError(gcnew System::String(strError));
			return;
		}
		delete pClause;
	}*/
	browserQuery = value;
}

//-----------------------------------------------------------------------------
MDBTObject^ MDBTMaster::CreateAndAttach(System::IntPtr dbtPtr)
{
	return gcnew MDBTMaster(dbtPtr);
}

//-----------------------------------------------------------------------------
void MDBTMaster::AssignInternalState(MGenericDataManager^ source)
{
	__super::AssignInternalState(source);

	MDBTMaster^ m = (MDBTMaster^)source;
	PrimaryKeyPrepared += m->delegatePrimaryKeyPrepared;
}

//-----------------------------------------------------------------------------
DataRelationType MDBTMaster::Relation::get () 
{ 
	return DataRelationType::Master;
}
//-----------------------------------------------------------------------------
void MDBTMaster::DefineQuery (MSqlTable^ mSqlTable)
{
	//no mor code needed here: moved to DynDBT
	__super::DefineQuery(mSqlTable);
}

//-----------------------------------------------------------------------------
void MDBTMaster::PrepareQuery (MSqlTable^ mSqlTable)
{
	//no mor code needed here: moved to DynDBT

	__super::PrepareQuery(mSqlTable);
}

//-----------------------------------------------------------------------------
void MDBTMaster::PrepareBrowser	(MSqlTable^ mSqlTable)
{
	// di default copio solo il criterio di sort perchè i filtri
	// sono sicuramente più restrittitivi di quelli di browse
	if (GetDBTObject() && mSqlTable != nullptr && mSqlTable->GetSqlTable())
	{
		SqlTable* pBrowserTable = mSqlTable->GetSqlTable();
		if (!HasCodeBehind)
		{
			pBrowserTable->SelectAll ();
			SqlTable* pTable = Table->GetSqlTable();
			if (pTable)
				pBrowserTable->m_strSort = pTable->m_strSort;
		}
	
		if (!String::IsNullOrEmpty(browserQuery))
		{

			CString strError;
			WClause* pBrowserClause = CreateValidWhereClause(pBrowserTable, browserQuery, strError);
			if (pBrowserClause)
				pBrowserClause->PrepareQuery(TRUE);
			else
				Diagnostic->SetError(gcnew String(strError));
			delete pBrowserClause;
		}
	
	}
}

//-----------------------------------------------------------------------------
void MDBTMaster::ReceivePreparePrimaryKey (int /*nRow*/, System::IntPtr /*recordPtr*/)
{
	this->PrimaryKeyPrepared (this, EasyBuilderEventArgs::Empty);
}

//-----------------------------------------------------------------------------
bool MDBTMaster::Open ()
{
	return GetDBTObject() ? ((DBTMaster*) GetDBTObject())->Open() == TRUE : false;
}

//-----------------------------------------------------------------------------
bool MDBTMaster::IsToDelete()
{
	MEasyBuilderContainer^ container = dynamic_cast<MEasyBuilderContainer^>(Document);
	if (container != nullptr)
	{
		for each(IComponent^ cmp in container->Components)
		{
			MDBTSlave^ dbtSlave = dynamic_cast<MDBTSlave^>(cmp);
			if (dbtSlave != nullptr && (!dbtSlave->HasCodeBehind || !dbtSlave->IsToDelete()))
				return false;
		}
	}

	return __super::IsToDelete();
}