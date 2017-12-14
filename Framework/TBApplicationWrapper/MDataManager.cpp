#include "StdAfx.h"


#include <TbGes\EXTDOC.H>
#include <TbOleDb\wclause.h>
#include <TbOleDb\Sqltable.h>

#include "MDocument.h"
#include "MDataManager.h"
#include "QueryController.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace ICSharpCode::NRefactory;
using namespace ICSharpCode::NRefactory::CSharp;
using namespace System; 
using namespace System::Collections::Generic;

using namespace Microarea::TaskBuilderNet::Core::DiagnosticManager;
using namespace Microarea::Framework::TBApplicationWrapper;

typedef ICSharpCode::NRefactory::CSharp::Attribute AstAttribute;
typedef ICSharpCode::NRefactory::CSharp::Expression AstExpression;


/////////////////////////////////////////////////////////////////////////////
// 				class GenericDataManagerSerializer Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
void GenericDataManagerSerializer::SerializeRecord (SyntaxTree^ syntaxTree, TypeDeclaration^ dbtClass, MSqlRecord^ record)
{
	DesignerSerializationManager^ mng = gcnew DesignerSerializationManager();
	IDisposable^ session = mng->CreateSession();

	CodeDomSerializer^ ser = (CodeDomSerializer^) mng->GetSerializer(record->GetType(), CodeDomSerializer::typeid);
	if (ser == nullptr || ser->GetType() != RecordSerializer::typeid)
		return;

	RecordSerializer^ recSerializer = (RecordSerializer^) ser;
	
	// related typed class is mandatory
	TypeDeclaration^ recClass = recSerializer->SerializeClass(syntaxTree, record);
	if (recClass == nullptr)
		return;
	
	NamespaceDeclaration^ ns = EasyBuilderSerializer::GetNamespaceDeclaration(syntaxTree);
	ns->Members->Add(recClass);
	SerializeRecordAccessor(syntaxTree, dbtClass, record);
}

//----------------------------------------------------------------------------	
void GenericDataManagerSerializer::SerializeRecordAccessor(SyntaxTree^ syntaxTree, TypeDeclaration^ aClass, MSqlRecord^ record)
{
	AstAttribute^ browAttr = AstFacilities::GetAttribute(BrowsableAttribute::typeid->FullName, gcnew PrimitiveExpression(false));

	// record accessors
	PropertyDeclaration^ accessor = GenerateRecordAccessor(record, GetRecordPtrMethodName, RecordSerializer::RecordPropertyName);
	if (accessor != nullptr)
		aClass->Members->Add(accessor);
	
	//PropertyDeclaration^ typedAccessor = GenerateTypedRecordAccessor(record, RecordSerializer::RecordPropertyName);
	//if (typedAccessor != nullptr)
	//	aClass->Members->Add(typedAccessor);
	
	// field
	FieldDeclaration^ fieldDeclaration = AstFacilities::GetFieldsDeclaration(record->SerializedType, record->SerializedName);
	fieldDeclaration->Modifiers = Modifiers::Public;
	aClass->Members->Add(fieldDeclaration);
}

//----------------------------------------------------------------------------	
PropertyDeclaration^	GenericDataManagerSerializer::GenerateRecordAccessor (MSqlRecord^ record, String^ invokeMethod, String^ propertyName)
{
	//get	{	return _OldTMA_ContactOrigin;	}
	IdentifierExpression^ varNameExpression = gcnew IdentifierExpression(record->SerializedName);
	List<Statement^>^ getStatements = gcnew List<Statement^>();
	getStatements->Add(gcnew ReturnStatement(varNameExpression));
	return GenerateProperty(IRecord::typeid->FullName, propertyName, getStatements, true);
}

//----------------------------------------------------------------------------	
PropertyDeclaration^ GenericDataManagerSerializer::GenerateTypedRecordAccessor (MSqlRecord^ record, String^ propertyName)
{
	List<Statement^>^ getStatements = gcnew List<Statement^>();
	getStatements->Add(
				gcnew ReturnStatement(
					gcnew CastExpression(
						gcnew SimpleType(record->SerializedType),
						gcnew MemberReferenceExpression(
							gcnew ThisReferenceExpression(),
							propertyName
						))
					)
				);

	return GenerateProperty(record->SerializedType, record->SerializedName, getStatements, false);
}

/////////////////////////////////////////////////////////////////////////////
// 				class DataManagerSerializer Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Object^ DataManagerSerializer::Serialize(IDesignerSerializationManager^ manager, Object^ current)
{
	List<Statement^>^ newCollection = gcnew List<Statement^>();

	MDataManager^ dataManager = (MDataManager^) current;
	
	System::String^ className	= dataManager->SerializedType;
	System::String^ varName		= dataManager->SerializedName;

	//TODO MATTEO: vedere come gestire i commenti con il nuovo code dom.
	//// comment
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	//newCollection->Add(gcnew CodeCommentStatement(String::Concat(" ", className)));
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	
	// construction
	IdentifierExpression^ varExpression	= gcnew IdentifierExpression(varName);
	newCollection->Add
		(
			gcnew ExpressionStatement(gcnew AssignmentExpression
			(
				varExpression,
				AssignmentOperatorType::Assign,
				AstFacilities::GetObjectCreationExpression(className)
			))
		);


	// attach process
	//this.Add(_DMMA_Areas);
	newCollection->Add(AstFacilities::GetInvocationStatement(gcnew ThisReferenceExpression(), AddMethodName, varExpression, gcnew PrimitiveExpression(dataManager->IsChanged)));

	SetExpression(manager, dataManager, varExpression, true);

	// properties
	IList<Statement^>^ props = SerializeProperties(manager, dataManager, varName);
	if (props != nullptr)
		newCollection->AddRange(props);

	// events
	IList<Statement^>^ events = SerializeEvents(manager, dataManager, dataManager->SerializedName);
	if (events != nullptr)
		newCollection->AddRange(events);

	return newCollection;
}

//----------------------------------------------------------------------------	
IList<Statement^>^ DataManagerSerializer::SerializeConstruction
	(
	IDesignerSerializationManager^ manager,
	MDataManager^ dataManager,
	System::String^ varName,
	System::String^ className
	)
{
	List<Statement^>^ newCollection = gcnew List<Statement^>();

	// construction
	IdentifierExpression^ varExpression	= gcnew IdentifierExpression(varName);
	newCollection->Add(
			gcnew ExpressionStatement(gcnew AssignmentExpression(
				varExpression,
				AssignmentOperatorType::Assign,
				AstFacilities::GetObjectCreationExpression(
					className,
					gcnew IdentifierExpression(EasyBuilderSerializer::TableNameParameterName),
					gcnew IdentifierExpression(EasyBuilderSerializer::DataManagerParameterName)
				)
			))
		);
	SetExpression(manager, dataManager, varExpression, true);

	return newCollection;
}


//----------------------------------------------------------------------------	
TypeDeclaration^ DataManagerSerializer::SerializeClass (SyntaxTree^ syntaxTree, IComponent^ object)
{
	if (object == nullptr || (object->GetType() != MDataManager::typeid && !object->GetType()->IsSubclassOf(MDataManager::typeid)))
		return nullptr;
	
	MDataManager^ dataManager = (MDataManager^) object; 
	
	String^ className = dataManager->SerializedType;
	RemoveClass(syntaxTree, className);

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MDataManager::typeid->FullName));


	AttributeSection^ attr = AstFacilities::GetAttributeSection(PreserveFieldAttribute::typeid->FullName);
		
	// Costruttore 
	ConstructorDeclaration^ constr = gcnew ConstructorDeclaration();
	constr->Modifiers = Modifiers::Public;
	constr->Name = aClass->Name;
	aClass->Members->Add(constr);

	constr->Body = gcnew BlockStatement();
	constr->Initializer = gcnew ConstructorInitializer();
	constr->Initializer->ConstructorInitializerType = ConstructorInitializerType::Base;
	constr->Initializer->Arguments->Add(gcnew PrimitiveExpression(dataManager->TableName));
	constr->Initializer->Arguments->Add(gcnew PrimitiveExpression(dataManager->Name));
	constr->Initializer->Arguments->Add(gcnew MemberReferenceExpression(gcnew IdentifierExpression(EasyBuilderSerializer::StaticControllerVariableName), "Document"));


	// record
	SerializeRecordAccessor(syntaxTree, aClass, (MSqlRecord^)dataManager->Record);

	//TMyRecord CastToMyRecord(IRecord record) { return (TMyRecord) record; }
	MethodDeclaration^ castToMyRecordMethod = gcnew MethodDeclaration();
	castToMyRecordMethod->Name = CastToMyRecordMethodName;
	castToMyRecordMethod->Modifiers = Modifiers::Public;
	castToMyRecordMethod->ReturnType = gcnew SimpleType (((MSqlRecord^)dataManager->Record)->SerializedType);
	castToMyRecordMethod->Parameters->Add(gcnew ParameterDeclaration(gcnew SimpleType(IRecord::typeid->FullName), RecordParamName, ParameterModifier::None));

	castToMyRecordMethod->Body = gcnew BlockStatement();
	
	castToMyRecordMethod->Body->Add
		(
			gcnew ReturnStatement
			(
				gcnew CastExpression
				(
					gcnew SimpleType(((MSqlRecord^)dataManager->Record)->SerializedType),
					gcnew IdentifierExpression (RecordParamName)
				)
			)
		);
	aClass->Members->Add(castToMyRecordMethod);

	return aClass;
}

/////////////////////////////////////////////////////////////////////////////
// 				class DataManagerSerializerForModuleController Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Object^ DataManagerSerializerForModuleController::Serialize(IDesignerSerializationManager^ manager, Object^ current)
{
	List<Statement^>^ newCollection = gcnew List<Statement^>();

	MDataManager^ dataManager = (MDataManager^) current;
	
	System::String^ className	= dataManager->SerializedType;
	System::String^ varName		= dataManager->SerializedName;

	//TODO MATTEO: gestire commenti
	//// comment
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	//newCollection->Add(gcnew CodeCommentStatement(String::Concat(" ", className)));
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));

	//DMAccounting _DMAccounting = null;
	
	VariableDeclarationStatement^ varStmt = gcnew VariableDeclarationStatement(
		gcnew SimpleType(className),
		varName,
		gcnew PrimitiveExpression(nullptr)
		);

	newCollection->Add(varStmt);
	IdentifierExpression^ variableDeclExpression = gcnew IdentifierExpression(varName);

	//_DMAccounting = new DMAccounting(controller);
	newCollection->Add
	(
		AstFacilities::GetAssignmentStatement(
			variableDeclExpression,
			AstFacilities::GetObjectCreationExpression(
				className
			)
		)
	);
	SetExpression(manager, dataManager, variableDeclExpression, true);

	// properties
	IList<Statement^>^ props = SerializeProperties(manager, dataManager, varName);
	if (props != nullptr)
		newCollection->AddRange(props);

	// events
	IList<Statement^>^ events = SerializeEvents(manager, dataManager, varName);
	if (events != nullptr)
		newCollection->AddRange(events);
	return newCollection;
}

//----------------------------------------------------------------------------	
ICSharpCode::NRefactory::CSharp::Expression^ DataManagerSerializerForModuleController::GetEventHandlerOwner()
{
	return gcnew ThisReferenceExpression();
}

//----------------------------------------------------------------------------	
AssignmentExpression^ DataManagerSerializerForModuleController::GenerateCodeAttachEventStatement(
			System::String^ varName,
            Microarea::TaskBuilderNet::Core::EasyBuilder::EventInfo^ changedEvent,
            ICSharpCode::NRefactory::CSharp::Expression^ handlerExpression
            )
{
	return gcnew AssignmentExpression(
		gcnew MemberReferenceExpression(gcnew IdentifierExpression(varName), changedEvent->EventName),
		AssignmentOperatorType::Add,
		handlerExpression
	);
}

//----------------------------------------------------------------------------	
System::String^ DataManagerSerializerForModuleController::GetOwnerController()
{
	return gcnew System::String(EasyBuilderSerializer::ModuleControllerClassName);
}

/////////////////////////////////////////////////////////////////////////////
// 				class DataManagerSerializerForSharedDataManagers Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Object^ DataManagerSerializerForSharedDataManagers::Serialize(IDesignerSerializationManager^ manager, Object^ current)
{
	List<Statement^>^ newCollection = gcnew List<Statement^>();

	MDataManager^ dataManager = (MDataManager^) current;
	
	System::String^ className	= dataManager->SerializedType;
	System::String^ varName		= dataManager->SerializedName;

	//TODO MATTEO gestire commenti
	//// comment
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	//newCollection->Add(gcnew CodeCommentStatement(String::Concat(" ", className)));
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	
	IList<Statement^>^ construction = SerializeConstruction(manager, dataManager, varName, className);
	//Se non serializza la creazione allora e` inutile continuare.
	if (construction == nullptr)
		return newCollection;

	newCollection->AddRange(construction);

	ThisReferenceExpression^ thisExpression = gcnew ThisReferenceExpression();
	newCollection->Add
	(
		//this.Add(DMAccount...);
		AstFacilities::GetInvocationStatement(
			thisExpression,
			EasyBuilderSerializer::AddMethodName,
			gcnew MemberReferenceExpression(thisExpression, varName), 
			gcnew PrimitiveExpression(dataManager->IsChanged)
		)
	);

		// properties
	IList<Statement^>^ props = SerializeProperties(manager, dataManager, varName);
	if (props != nullptr)
		newCollection->AddRange(props);

	// events
	IList<Statement^>^ events = SerializeEvents(manager, dataManager, dataManager->SerializedName);
	if (events != nullptr)
		newCollection->AddRange(events);

	return newCollection;
}

//----------------------------------------------------------------------------	
IList<Statement^>^ DataManagerSerializerForSharedDataManagers::SerializeConstruction
	(
	IDesignerSerializationManager^ manager,
	MDataManager^ dataManager,
	System::String^ varName,
	System::String^ className
	)
{
	//_DMBanks = Module.NewApplication1.NewModule1.ModuleController.Factory.Crete_DMBanks(tableName, dataManagerName);
	System::String^ dataManagerClassFullName = dataManager->GetType()->FullName;
	System::String^ moduleDllNamespace = dataManagerClassFullName->Substring(0, dataManagerClassFullName->LastIndexOf('.'));
	System::String^ moduleControllerFullName = System::String::Concat(moduleDllNamespace, ".ModuleController");

	List<Statement^>^ newCollection = gcnew List<Statement^>();

	// construction
	IdentifierExpression^ varExpression	= gcnew IdentifierExpression(varName);

	TypeReferenceExpression^ moduleControllerReferenceExpression = gcnew TypeReferenceExpression(gcnew SimpleType(moduleControllerFullName));
	MemberReferenceExpression^ factoryPropertyReferenceExpression = gcnew MemberReferenceExpression(moduleControllerReferenceExpression, "Factory");

	InvocationExpression^ createInvokeExpression = AstFacilities::GetInvocationExpression(
		factoryPropertyReferenceExpression,
		EasyBuilderSerializer::GetFactoryMethodNameFromClassName(className)
		);

	newCollection->Add(AstFacilities::GetAssignmentStatement(varExpression, createInvokeExpression));

	SetExpression(manager, dataManager, varExpression, true);

	return newCollection;
}

//----------------------------------------------------------------------------	
TypeDeclaration^ DataManagerSerializerForSharedDataManagers::SerializeClass(SyntaxTree^ syntaxTree, IComponent^ object)
{
	return nullptr;
}


/////////////////////////////////////////////////////////////////////////////
// 				class MDataManager Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDataManager::MDataManager (System::String^ tableName, System::String^ datamanagerName, EasyBuilderComponent^ document)
{
	this->name = datamanagerName;
	this->table = gcnew MSqlTable(gcnew MSqlRecord(tableName));
	this->status = ReadResult::None;
	this->m_pOldRecord = NULL;
	this->isUpdatable = false;
	if (document != nullptr)
		ParentComponent = document;
}

//-----------------------------------------------------------------------------
MDataManager::~MDataManager()
{
	this->!MDataManager();
	System::GC::SuppressFinalize(this);
}
//-----------------------------------------------------------------------------
MDataManager::!MDataManager()
{
	Close();
	delete m_pOldRecord;
	delete table;
}

//-----------------------------------------------------------------------------
bool MDataManager::IsUpdatable::get () 
{
	return isUpdatable; 
}
//-----------------------------------------------------------------------------
void MDataManager::IsUpdatable::set(bool value) 
{
	isUpdatable = value;
	if (isUpdatable && m_pOldRecord == NULL)
		m_pOldRecord = (SqlRecord*) GetTable()->GetRecord()->GetRuntimeClass()->CreateObject();
}
	
//-----------------------------------------------------------------------------
bool MDataManager::AutoCommit::get () 
{
	return GetTable()->IsAutocommit() == TRUE;
}
//-----------------------------------------------------------------------------
void MDataManager::AutoCommit::set(bool value) 
{
	GetTable()->SetAutocommit(value);
}
			
//-----------------------------------------------------------------------------
System::String^ MDataManager::ToString()
{
	return ReflectionUtils::GetComponentFullPath(this); 
}

//-----------------------------------------------------------------------------
System::String^ MDataManager::Name::get()
{
	return name;
}

//------------------------------------------------------------------------F-----
INameSpace^ MDataManager::Namespace::get()
{
	return nullptr; //TODOPERASSO
}

//------------------------------------------------------------------------F-----
System::String^	 MDataManager::TableName::get()
{
	return Record->Name;
}

//-----------------------------------------------------------------------------
System::IntPtr MDataManager::GetRecordPtr ()
{
	return (System::IntPtr)((MSqlRecord^)table->Record)->GetSqlRecord();
}

//-----------------------------------------------------------------------------
IRecord^ MDataManager::Record::get()
{
	return ((MSqlRecord^)table->Record);
}

//-----------------------------------------------------------------------------
System::String^ MDataManager::SerializedType::get ()
{ 
	return System::String::Concat("DM", EasyBuilderSerializer::Escape(Name));
}


//----------------------------------------------------------------------------
bool MDataManager::CanChangeProperty (System::String^ propertyName) 
{
	return true;
}

//----------------------------------------------------------------------------
void MDataManager::DefineQuery (MSqlTable^ mSqlTable)
{
	mSqlTable->GetSqlTable()->SelectAll();
	__super::DefineQuery(mSqlTable);
}
//----------------------------------------------------------------------------
MDataManager::ReadResult MDataManager::Read()
{
	return Read(false);
}
//----------------------------------------------------------------------------
MDataManager::ReadResult MDataManager::Read(bool lock)
{
	SqlTable* pTable = GetTable();
	if (!pTable->IsOpen())
		ReExecuteQuery();
	else if (!pTable->IsEOF())	
		pTable->MoveNext();
	
	if (pTable->IsEOF())
	{	//non ho una riga corrente
		if (isUpdatable)
			m_pOldRecord->Init(); //azzero il vecchio record
		status = ReadResult::NotFound; //imposto lo stato a not found
		return status;
	}
	else
	{//se ho una riga corrente
		status = ReadResult::Found; //lo stato e` found
		if (isUpdatable)//se sono updatable, metto da parte la riga corrente (ante modifica che verra` fatta in seguito)
			*m_pOldRecord = *pTable->GetRecord();
		
		// se devo locckare, eseguo il lock
		if (lock)
			LockCurrent();
		return status;
	}
	
}

//----------------------------------------------------------------------------
bool MDataManager::LockCurrent()
{
	if (!isUpdatable)
	{
		CString strMsg = _TB("Error locking current record. The IsUpdatable property of the DataManager object must have the value True in order to lock the record");
		Diagnostic->SetError(gcnew System::String(strMsg));
		return false;
	}
	
	if (GetTable()->LockCurrent())
		return true;
	
	status = ReadResult::Locked;
	return false;
}

//----------------------------------------------------------------------------
void MDataManager::AddNew()
{
	if (!isUpdatable)
	{
		CString strMsg = _TB("Error updating current record. The IsUpdatable property of the DataManager object must have the value True in order to update the record");
		Diagnostic->SetError(gcnew System::String(strMsg));
		return;
	}
	SqlTable* pTable = GetTable();
    TRY
    {
		if (pTable->IsOpen() && !pTable->IsUpdatable())
			pTable->Close();
		if (!pTable->IsOpen())
		{
			pTable->SetSqlSession( ((MDocument^)Document)->GetDocument()->GetUpdatableSqlSession() );
			pTable->Open(TRUE);
			pTable->SelectAll();
		}
		pTable->AddNew(TRUE);
	}
	CATCH(SqlException, e)	
	{
		CString strMsg = cwsprintf(_TB("Error updating table {0-%s}."), (LPCTSTR)pTable->GetRecord()->GetTableName());
		Diagnostic->SetError(gcnew System::String(strMsg));
		Diagnostic->SetError(gcnew System::String(e->m_strError));
		e->Delete();
	}
	END_CATCH
}
//----------------------------------------------------------------------------
bool MDataManager::UpdateCurrent()
{
	if (!isUpdatable)
	{
		CString strMsg = _TB("Error updating current record. The IsUpdatable property of the DataManager object must have the value True in order to update the record");
		Diagnostic->SetError(gcnew System::String(strMsg));
		return false;
	}
	bool bResult = false;
	SqlTable* pTable = GetTable();
    TRY
    {
		if (!pTable->IsOpen())
		{
			ASSERT(FALSE);
			TRACE0("UpdateRecord(): used with no FindRecord call\n");
			return bResult;
		}

		// Il record e' locked non si puo' aggiornare
		if (status == ReadResult::Locked)
		{
			ASSERT(FALSE);
			TRACE0("UpdateRecord(): the record is locked\n");
			return bResult;
		}
	
    	// inibisce l'inizializzazione del record in caso di AddNew per non fumare
    	// i dati modificati dal programmataore dipo la FindRecord.
		if (pTable->IsInNoMode())
		{
			if (pTable->IsEmpty()) 
				pTable->AddNew(FALSE);
			else 
				pTable->Edit();
		}
		// l'old deve  arrivare
		// a questo punto buono
		ASSERT(m_pOldRecord);

		pTable->Update(m_pOldRecord);
	    
	    bResult = true;
	}
	CATCH(SqlException, e)	
	{
		CString strMsg = cwsprintf(_TB("Error updating table {0-%s}."), (LPCTSTR)pTable->GetRecord()->GetTableName());
		Diagnostic->SetError(gcnew System::String(strMsg));
		Diagnostic->SetError(gcnew System::String(e->m_strError));
		e->Delete();
		bResult = false;
	}
	END_CATCH
	return bResult;
}
//----------------------------------------------------------------------------
bool MDataManager::DeleteCurrent()
{
	if (!isUpdatable)
	{
		CString strMsg = _TB("Error deleting current record. The IsUpdatable property of the DataManager object must have the value True in order to delete the record");
		Diagnostic->SetError(gcnew System::String(strMsg));
		return false;
	}
	bool bResult = false;
	SqlTable* pTable = GetTable();
  
    TRY
    {
		if (!pTable->IsOpen())
		{
			ASSERT(FALSE);
			TRACE0("DeleteRecord() used with no FindRecord call\n");
			return bResult;
		}

		// Il record e' locked non si puo' aggiornare
		if (status == ReadResult::Locked)
		{
			ASSERT(FALSE);
			TRACE0("UpdateRecord(): the record is locked\n");
			return bResult;
		}
	
		ASSERT(!pTable->IsEmpty());

		// l'old deve  arrivare
		// a questo punto buono
		ASSERT(m_pOldRecord);

		pTable->Delete(m_pOldRecord);
	    
	    bResult = true;
	}
	CATCH(SqlException, e)	
	{
		CString strMsg = cwsprintf(_TB("Error deleting a record in table {0-%s}."), (LPCTSTR)pTable->GetRecord()->GetTableName());
			
		Diagnostic->SetError(gcnew System::String(strMsg));
		Diagnostic->SetError(gcnew System::String(e->m_strError));
		e->Delete();
		bResult = false;
	}
	END_CATCH
	return bResult;
}

//----------------------------------------------------------------------------
void MDataManager::Close()
{
	SqlTable* pTable = GetTable();
	if (pTable)
	{
		pTable->Close();
		pTable->GetRecord()->Init();
	}
	SAFE_DELETE(whereClause);
}
//----------------------------------------------------------------------------
void MDataManager::ReExecuteQuery()
{
	SqlTable* pTable = GetTable();
    TRY
    {
		if (!pTable->IsOpen())
		{
			if (Document != nullptr)
			{
				pTable->SetSqlSession(isUpdatable 
					? ((MDocument^)Document)->GetDocument()->GetUpdatableSqlSession() 
					: ((MDocument^)Document)->GetDocument()->GetReadOnlySqlSession());
			}
			else
				pTable->SetSqlSession(AfxGetDefaultSqlSession());

			pTable->Open(isUpdatable, E_FAST_FORWARD_ONLY);
			
			DefineQuery(table);
			PrepareQuery(table);
		}
		else
		{
			PrepareQuery(table);
		}
		pTable->Query();
		OnQueried();
	}
	CATCH(SqlException, e)	
	{
		if (pTable)
		{
			// potrebbe non essere mai stata aperta, oppure chiusa d'ufficio
			// dalla tabella LRU
			if (pTable->IsOpen()) 
				pTable->Close();
		}

		pTable->GetRecord()->Init();
		
		Diagnostic->SetError(gcnew System::String(e->m_strError));
		e->Delete();
	}
	END_CATCH
}
