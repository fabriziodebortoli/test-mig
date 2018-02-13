#include "StdAfx.h"

//siccome sono in mixed mode, SendMessage non è mappata su SendMessageW e allora mi da errore di linking sulla SendMessage di CBaseDocument
#define SendMessage SendMessageW 


#include <TbGes\EventMng.H>
#include <TbWoormViewer\WOORMDOC.H>
#include <TBGes\ExtDoc.h>
#include <TBGes\DBT.h>
#include <TBGes\XMLGesInfo.h>
#include <TBGes\BODYEDIT.H>
#include <TBGes\Tabber.h>
#include <TBGes\TBRadarInterface.h>

#include "BusinessObjectParams.h"
#include "MDocument.h"

using namespace System;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Interfaces::View;
using namespace Microarea:: Framework::TBApplicationWrapper;

using namespace ICSharpCode::NRefactory::CSharp;
using namespace ICSharpCode::NRefactory::PatternMatching;

/////////////////////////////////////////////////////////////////////////////
// 				class DocumentSerializer Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
Object^ DocumentSerializer::Serialize (IDesignerSerializationManager^ manager, Object^ current)
{
	List<Statement^>^ newCollection = gcnew List<Statement^>();

	MDocument^ doc = (MDocument^) current;
	
	System::String^ className	= doc->SerializedType;
	System::String^ varName		= doc->SerializedName;

	//TODO MATTEO: vedere come gestire i commenti con il nuovo code dom
	//// comment
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	//newCollection->Add(gcnew CodeCommentStatement(String::Concat(" ", className)));
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	
	// construction
	IdentifierExpression^ varExpression	= gcnew IdentifierExpression(varName);

	//this.Add(document_ProspectiveSuppliers);
	Statement^ addStat = AstFacilities::GetInvocationStatement(
		gcnew ThisReferenceExpression(), 
		EasyBuilderSerializer::AddMethodName,
		varExpression,
		gcnew PrimitiveExpression(doc->IsChanged)
		);

	newCollection->Add(addStat);

	SetExpression(manager, doc, varExpression, true);
	
	// properties
	IList<Statement^>^ props = SerializeProperties(manager, doc, varName);
	if (props != nullptr)
		newCollection->AddRange(props);

	// events
	IList<Statement^>^ events = SerializeEvents(manager, doc, doc->SerializedName);
	if (events != nullptr)
		newCollection->AddRange(events);

	return newCollection;
}

//----------------------------------------------------------------------------	
void DocumentSerializer::GenerateFields(IContainer^ container, TypeDeclaration^ classStructure)
{
	__super::GenerateFields(container, classStructure);

	//serializzazione delle fieldDeclaration dei campi locali del document
	MDocument^ document = dynamic_cast<MDocument^>(container);
	CAbstractFormDoc* pDoc = document->GetDocument();
	if (!pDoc || !pDoc->GetVariableArray())
		return;

	for (int i = 0; i <= pDoc->GetVariableArray()->GetUpperBound(); i++)
	{
		CXMLVariable* pVariable = pDoc->GetVariableArray()->GetAt(i);

		MDataObj^ mDataObj = MDataObj::Create(pVariable->GetDataObj());

		System::String^ dataObjClass = mDataObj->GetType()->ToString();
		System::String^ name = gcnew System::String(pVariable->GetName());

		// data member
		String^ serializedName = String::Concat("fld_", EasyBuilderSerializer::Escape(name));

		FieldDeclaration^ field = AstFacilities::GetFieldsDeclaration(dataObjClass, serializedName);
		field->Modifiers = Modifiers::Public;
		classStructure->Members->Add(field);
	}
}

//----------------------------------------------------------------------------	
Type^ DocumentSerializer::ComponentSerializedAs::get()
{
	return MDocument::typeid;
}

//----------------------------------------------------------------------------	
TypeDeclaration^ DocumentSerializer::SerializeClass	(SyntaxTree^ syntaxTree, IComponent^ component) 
{
	if	(	
		component == nullptr || 
		(!component->GetType()->IsSubclassOf(ComponentSerializedAs) && component->GetType() != ComponentSerializedAs)
		)
		return nullptr;
	
	NamespaceDeclaration^ ns = EasyBuilderSerializer::GetNamespaceDeclaration(syntaxTree);
	MDocument^ document = (MDocument^) component; 
	
	String^ className = document->SerializedType;
	RemoveClass(syntaxTree, className);

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(ComponentSerializedAs->Name));
	
	if (!BusinessObject::typeid->IsInstanceOfType(document))
	{
		// Costruttore 
		ConstructorDeclaration^ constr = gcnew ConstructorDeclaration();
		constr->Modifiers = Modifiers::Public;
		constr->Name = aClass->Name;
		aClass->Members->Add(constr);

		constr->Parameters->Add(gcnew ParameterDeclaration(gcnew SimpleType(IntPtr::typeid->FullName), WrappedObjectParamName, ICSharpCode::NRefactory::CSharp::ParameterModifier::None));

		constr->Initializer = AstFacilities::GetConstructorInitializer(
			gcnew IdentifierExpression(WrappedObjectParamName)
			);


		constr->Body = gcnew BlockStatement();
	
	
	}

	return aClass;
};

//----------------------------------------------------------------------------	
IList<Statement^>^ DocumentSerializer::GetAdditionalCreateComponentsStatements(IContainer^ container, IList<System::String^>^ memberDeclaration)
{
	IList<Statement^>^ localFields = gcnew List<Statement^>();

	MDocument^ doc = dynamic_cast<MDocument^>(container);
	if (doc == nullptr)
		return localFields;

	CAbstractFormDoc* pDoc = doc->GetDocument();
	if (!pDoc || !pDoc->GetVariableArray())
		return localFields;

	for (int i = 0; i <= pDoc->GetVariableArray()->GetUpperBound(); i++)
	{
		CXMLVariable* pVariable = pDoc->GetVariableArray()->GetAt(i);

		MDataObj^ mDataObj = MDataObj::Create(pVariable->GetDataObj());

		System::String^ dataObjClass = mDataObj->GetType()->ToString();
		System::String^ name = gcnew System::String(pVariable->GetName());

		// accessor 
		InvocationExpression^ invoke = AstFacilities::GetInvocationExpression(
			gcnew ThisReferenceExpression(),
			GetFieldPtrMethodName,
			gcnew PrimitiveExpression(name)
			);

		//fld_PostInventory = new  MDataBool(...)
		String^ serializedName = String::Concat("fld_", EasyBuilderSerializer::Escape(name));
		
		memberDeclaration->Add(serializedName);

		ObjectCreateExpression^ codeCreateExpresson = AstFacilities::GetObjectCreationExpression(dataObjClass, invoke);
		Statement^ stmt = AstFacilities::GetAssignmentStatement
			(
				gcnew MemberReferenceExpression(gcnew ThisReferenceExpression(), serializedName),
				codeCreateExpresson
				);

		localFields->Add(stmt);
	}

	return localFields;
}


/////////////////////////////////////////////////////////////////////////////
// 				class DocumentContext Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
DocumentContext::DocumentContext (System::IntPtr sqlConnectionPtr)
	:
	m_pContext (NULL)
{
	SqlConnection* pConnection = (SqlConnection*) sqlConnectionPtr.ToInt64();

	m_pContext = new CTBContext(pConnection);
	hasCodeBehind = false;
}

//-----------------------------------------------------------------------------
DocumentContext::DocumentContext (CTBContext* pContext)
	:
	m_pContext (pContext)
{
	hasCodeBehind = true;
}

//-----------------------------------------------------------------------------
DocumentContext::~DocumentContext ()
{
	this->!DocumentContext();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
DocumentContext::!DocumentContext ()
{
	if (!hasCodeBehind)
		delete m_pContext;
}

//-----------------------------------------------------------------------------
CTBContext* DocumentContext::GetContext ()
{
	return m_pContext;
}

//----------------------------------------------------------------------------
System::IntPtr DocumentContext::GetReadOnlySessionPtr ()
{
	return m_pContext ? (System::IntPtr) m_pContext->GetReadOnlySqlSession() : System::IntPtr::Zero;
}

//----------------------------------------------------------------------------
System::IntPtr DocumentContext::GetUpdatableSessionPtr ()
{
	return m_pContext ? (System::IntPtr) m_pContext->GetUpdatableSqlSession() : System::IntPtr::Zero;
}

//----------------------------------------------------------------------------
bool DocumentContext::StartTransaction()
{
	return m_pContext ? m_pContext->StartTransaction() == TRUE : false;
}

//----------------------------------------------------------------------------
void DocumentContext::Commit()
{
	if (m_pContext)
		m_pContext->Commit();
}

//----------------------------------------------------------------------------
void DocumentContext::Rollback()
{
	if (m_pContext)
		m_pContext->Rollback();
}

/////////////////////////////////////////////////////////////////////////////
// 				class MDocument Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MDocument::MDocument (System::IntPtr pDocument)
{
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)pDocument.ToInt64();
	m_ppDocument = new AbstractFormDocPtr(pDoc);
	master = nullptr;
	nameSpace = nullptr;
	componentsCreated = false;
	HasCodeBehind = true;
	m_pInvalidDBTs = NULL;
}

//-----------------------------------------------------------------------------
MDocument::~MDocument ()
{
	this->!MDocument();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MDocument::!MDocument()
{
	if (master != nullptr)
		delete master;
	componentsCreated = false;

	if (components != nullptr)
	{
		for (int i = components->Count - 1; i >= 0; i--)
		{
			IDocumentSlaveDataManager^ slave = dynamic_cast<IDocumentSlaveDataManager^>(components[i]);
			if (slave == nullptr)
				continue;

			delete slave;
		}
	}

	SAFE_DELETE(m_pInvalidDBTs);
	SAFE_DELETE(m_ppDocument);
}

//-----------------------------------------------------------------------------
void MDocument::AttachHotLink(MHotLink^ mHotLink)
{
	GetDocument()->Attach(mHotLink->GetHotLink());
}

//----------------------------------------------------------------------------	
generic<class T> where T : MDocument
T MDocument::Create(System::String^ documentNamespace)
{
	return Create<T>(documentNamespace, nullptr);
}

//----------------------------------------------------------------------------	
generic<class T> where T : MDocument
T MDocument::Create(System::IntPtr documentPtr)
{
	MDocument^ doc = gcnew MDocument (documentPtr);
	doc->WrapExistingObjectsInRunning = true;
	doc->CallCreateComponents();
	return (T)doc;
}
//----------------------------------------------------------------------------	
generic<class T> where T : MDocument
T MDocument::Create(System::String^ documentNamespace, DocumentContext^ context)
{
	return Create<T>(documentNamespace, false, false, context, false);
}

//----------------------------------------------------------------------------	
generic<class T> where T : MDocument
T MDocument::CreateForSerialization(System::String^ documentNamespace)
{
	return Create<T>(documentNamespace, true, false, nullptr, true);
}

//----------------------------------------------------------------------------	
generic<class T> where T : MDocument
T MDocument::CreateUnattended(System::String^ documentNamespace, DocumentContext^ context)
{
	return Create<T>(documentNamespace, true, false, context, false);
}

//----------------------------------------------------------------------------	
generic<class T> where T : MDocument
T MDocument::CreateHidden(System::String^ documentNamespace, DocumentContext^ context)
{
	return Create<T>(documentNamespace, true, true, context, false);
}		
//-----------------------------------------------------------------------------
generic<class T> where T : MDocument
T MDocument::Create (System::String^ docNamespace, 
										bool unattendedMode, 
										bool invisible, 
										DocumentContext^ context, 
										bool isExposing
										)
{
	if (System::String::IsNullOrEmpty(docNamespace))
		throw gcnew ArgumentException(_T("docNamespace"));
	CString ns(docNamespace);
	
	const CDocumentDescription* pDocDescri = AfxGetDocumentDescription(ns);
	if (!pDocDescri)
		throw gcnew ApplicationException(gcnew System::String(_TB("Document description not found")));


	CString sViewMode(szDefaultViewMode);
	if (invisible)
	{
		sViewMode = szNoInterface;
	}
	else if (unattendedMode)
	{
		if (pDocDescri->IsDynamic())
			sViewMode = szBackgroundViewMode;
		else
		{
			CViewModeDescription* pUnattendedVMode = pDocDescri->GetViewMode(szBackgroundViewMode);
			if (!pUnattendedVMode)
				pUnattendedVMode = pDocDescri->GetViewMode(szUnattendedViewMode);

			if (pUnattendedVMode)
				sViewMode = pUnattendedVMode->GetName();
		}
	}

	CBusinessObjectInvocationInfo* pInfo = new CBusinessObjectInvocationInfo(isExposing);

	CTBContext* pContext = context == nullptr ? NULL : context->GetContext();
	CAbstractFormDoc* pDocument = (CAbstractFormDoc*) AfxGetTbCmdManager()->RunDocument
		(
			ns, 
			sViewMode, 
			NULL, 
			NULL, 
			NULL, 
			NULL, 
			NULL, 
			NULL, 
			!pDocDescri->GetInterfaceClass().IsEmpty(),
			pContext,
			pInfo
		);

	if (!pDocument)
		throw gcnew ApplicationException(gcnew System::String(cwsprintf(_TB("Error creating document '{0-%s}'"), ns)));

	pDocument->SetInUnattendedMode(unattendedMode || invisible);

	T doc = (T)Activator::CreateInstance(T::typeid, (System::IntPtr) pDocument);
	pInfo->SetCaller(doc);
	doc->context = context;
	doc->HasCodeBehind = !pDocDescri->IsDynamic();

	return (T)doc;
}

//----------------------------------------------------------------------------
void MDocument::Close()
{
	if (GetDocument())
		AfxInvokeThreadProcedure<CAbstractFormDoc>(GetDocument()->GetFrameHandle(), GetDocument(), &CAbstractFormDoc::OnCloseDocument);
}

//----------------------------------------------------------------------------
bool MDocument::DesignMode::get ()
{
	return GetDocument() ? GetDocument()->GetDesignMode() != CBaseDocument::DM_NONE : false;
}

//----------------------------------------------------------------------------
bool MDocument::Batch::get()
{
	if (!GetDocument())
		return false;
	
	CFrameWnd* pFrame = GetDocument()->GetMasterFrame();
	return pFrame && pFrame->IsKindOf(RUNTIME_CLASS(CBatchFrame));
}
		
//----------------------------------------------------------------------------
DocumentContext^ MDocument::Context::get ()
{
	if (context == nullptr && GetDocument() && GetDocument()->m_pTbContext)
		context = GetDocument() ? gcnew DocumentContext(GetDocument()->m_pTbContext) : nullptr;

	return context;
}


//-----------------------------------------------------------------------------
MXMLVariableArray^ MDocument::XMLVariableArray::get()
{
	if (!GetDocument() || !GetDocument()->GetVariableArray())
		return nullptr;

	return gcnew MXMLVariableArray(GetDocument()->GetVariableArray());
}

//-----------------------------------------------------------------------------
MXMLVariableArray^ MDocument::BookmarkXMLVariables::get()
{
	if (!GetDocument() || !GetDocument()->GetDMSAttachmentManager() || !GetDocument()->GetDMSAttachmentManager()->GetBookmarkXMLVariables())
		return nullptr;

	return gcnew MXMLVariableArray(GetDocument()->GetDMSAttachmentManager()->GetBookmarkXMLVariables());
}


//----------------------------------------------------------------------------
SymTable* MDocument::GetSymTable()
{
	if (!GetDocument())
		return NULL;
	
	GetDocument()->ReloadSymbolTable();
	SymTable* pTable = GetDocument()->GetSymTable();
	for each (EasyBuilderComponent ^ cmp in Components)
	{ 
		if (MDBTSlaveBuffered::typeid->IsInstanceOfType(cmp))
		{
			//se ho la riga corrente, prendo quella, altrimenti la riga del prototipo
			//così ho sempre i campi del dbt in symbol table anche per scopi di editing della query
			MDBTSlaveBuffered^ buff = (MDBTSlaveBuffered^)cmp;
			MSqlRecord^ record = buff->GetCurrentRecord();
			if (record == nullptr)
				record = (MSqlRecord^) buff->Record;
			GetDocument()->AddToSymbolTable(record->GetSqlRecord(), cmp->SerializedName);
		}
		else if (MGenericDataManager::typeid->IsInstanceOfType(cmp))
		{
			MGenericDataManager^ gdm = (MGenericDataManager^)cmp;
			GetDocument()->AddToSymbolTable(((MSqlRecord^)gdm->Record)->GetSqlRecord(), cmp->SerializedName);
		}
	}
	return pTable;
}

//-----------------------------------------------------------------------------
bool MDocument::Modified::get()
{
	return GetDocument() ? GetDocument()->IsModified() == TRUE : false;
}

//-----------------------------------------------------------------------------
void MDocument::Modified::set(bool value)
{
	if (GetDocument())
		GetDocument()->SetModifiedFlag(value == true);
}

//-----------------------------------------------------------------------------
bool MDocument::BatchCloseAfterExecution::get()
{
	return GetDocument() ? GetDocument()->m_bBatchCloseAfterExecution == TRUE : false;
}

//-----------------------------------------------------------------------------
void MDocument::BatchCloseAfterExecution::set(bool value)
{
	if (GetDocument())
		GetDocument()->m_bBatchCloseAfterExecution = value;
}
//-----------------------------------------------------------------------------
bool MDocument::CanClose::get()
{
	return GetDocument() ? GetDocument()->CanCloseDocument() == TRUE : true;
}

//-----------------------------------------------------------------------------
bool MDocument::BatchRunning::get()
{
	return GetDocument() ? GetDocument()->m_bBatchRunning == TRUE : true;
}

//-----------------------------------------------------------------------------
void MDocument::AddDynamicDocumentObject(INameSpace^ docNamespace, INameSpace^ templateDocNamespace, System::String^ title, bool isBatch)
{
	CTBNamespace tbns (CString(docNamespace->ToString()));
	CDocumentDescription* pDescri = new CDocumentDescription(tbns, CString(title));
	//posso esser euna customizzazione di un documento esistente
	if (templateDocNamespace)
		pDescri->SetTemplateNamespace(new CTBNamespace (CString(templateDocNamespace->ToString())));
	else //oppure un documento dinamico
		pDescri->SetDynamic(TRUE);
	if (isBatch)
	{
		CViewModeDescription* pVM = new CViewModeDescription();
		pVM->SetName(szDefaultViewMode);
		pVM->SetType(VMT_BATCH);
		pDescri->AddViewMode(pVM); 
	}
	AfxAddDocumentDescription(pDescri);
}

//-----------------------------------------------------------------------------
Int32 MDocument::TbHandle::get()
{ 
	return (long) GetDocument();
}

//-----------------------------------------------------------------------------
INameSpace^ MDocument::Namespace::get ()
{
	if (!GetDocument())
		return NameSpace::Empty;
	if (nameSpace == nullptr)
		nameSpace = gcnew NameSpace (gcnew System::String(GetDocument()->GetNamespace().ToString())); 
	return nameSpace;
}
//-----------------------------------------------------------------------------
void MDocument::Namespace::set (INameSpace^ ns)
{
	if (!GetDocument())
	{
		ASSERT(FALSE);
		return;
	}
	nameSpace = (NameSpace^)ns;
	GetDocument()->SetNamespace(CTBNamespace(CString(ns->ToString())));

}

//-----------------------------------------------------------------------------
FormModeType MDocument::FormMode::get()
{
	if (!GetDocument())
	{
		ASSERT(FALSE);
		return FormModeType::None;
	}
	
	return (FormModeType)((int)GetDocument()->GetFormMode());
}

//-----------------------------------------------------------------------------
void MDocument::FormMode::set(FormModeType value)
{
	if (!GetDocument())
	{
		ASSERT(FALSE);
		return;
	}
	
	if (FormMode == value)
		return;

	switch (value)
	{
	case FormModeType::New:
		if (FormMode == FormModeType::Browse)
			EnterInNewRecord();
		break;
	case FormModeType::Edit:
		if (FormMode == FormModeType::Browse)
			EditCurrentRecord();
		break;
	case FormModeType::Find:
		if (FormMode == FormModeType::Browse)
			GoInFindMode();
		break;
	case FormModeType::Browse:
		if (FormMode != FormModeType::Browse)
			GoInBrowseMode();
		break;
	}
}

//-----------------------------------------------------------------------------
bool MDocument::InUnattendedMode::get()
{ 
	if (!GetDocument())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return GetDocument()->IsInUnattendedMode() == TRUE;	
}

//-----------------------------------------------------------------------------
bool MDocument::OnlyOneRecord::get()
{
	if (!GetDocument())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return GetDocument()->IsOnlyOneRecord() == TRUE;	
}
//-----------------------------------------------------------------------------
void MDocument::OnlyOneRecord::set(bool value)
{
	if (!GetDocument())
	{
		ASSERT(FALSE);
		return;
	}

	GetDocument()->SetOnlyOneRecord(value);	
}

//-----------------------------------------------------------------------------
NameSpace^ MDocument::TemplateNamespace::get()
{
	if (!GetDocument() || !GetDocument()->GetXmlDescription() || !GetDocument()->GetXmlDescription()->GetTemplateNamespace())
		return nullptr;
	
	return gcnew NameSpace(gcnew String(GetDocument()->GetXmlDescription()->GetTemplateNamespace()->ToString()));
}

//-----------------------------------------------------------------------------
System::String^ MDocument::SerializedName::get ()
{ 
	return System::String::Concat("document_", EasyBuilderSerializer::Escape(Namespace->Document));
}

//-----------------------------------------------------------------------------
System::String^ MDocument::SerializedType::get ()
{ 
	return System::String::Concat("MD", EasyBuilderSerializer::Escape(Namespace->Document));
}

//-----------------------------------------------------------------------------
IDocumentMasterDataManager^ MDocument::Master::get ()
{
	return master;
}

//-----------------------------------------------------------------------------
DocumentMessageProvider^ MDocument::GetMessageProvider() 
{
	return gcnew DocumentMessageProvider(GetDocument()); 
}

//-----------------------------------------------------------------------------
MDBTObject^ MDocument::GetDBT(INameSpace^ nameSpace)
{
	if (master == nullptr)
		return nullptr;

	if (System::String::Compare(master->Namespace->FullNameSpace, nameSpace->FullNameSpace, true) == 0)
		return master;
	
	for each (IComponent^ component in Components)
	{
		MDBTSlave^ slave = dynamic_cast<MDBTSlave^>(component);
		if (slave == nullptr)
			continue;

		if (System::String::Compare(slave->Namespace->FullNameSpace, nameSpace->FullNameSpace, true) == 0)
			return slave;

		if (MDBTSlaveBuffered::typeid->IsInstanceOfType(slave))
		{
			MDBTObject^ subSlave = ((MDBTSlaveBuffered^) slave)->GetDBT(nameSpace);
			if (subSlave != nullptr)
				return subSlave;
		}
	}
	
	return nullptr;
}
//-----------------------------------------------------------------------------
MDBTObject^ MDocument::GetDBT(System::String^ name)
{
	if (master == nullptr)
		return nullptr;

	if (System::String::Compare(master->Name, name, true) == 0)
		return master;
	
	for each (IComponent^ component in Components)
	{
		MDBTSlave^ slave = dynamic_cast<MDBTSlave^>(component);
		if (slave == nullptr)
			continue;

		if (System::String::Compare(slave->Name, name, true) == 0)
			return slave;

		if (MDBTSlaveBuffered::typeid->IsInstanceOfType(slave))
		{
			MDBTObject^ subSlave = ((MDBTSlaveBuffered^) slave)->GetDBT(name);
			if (subSlave != nullptr)
				return subSlave;
		}
	}
	
	return nullptr;
}
//-----------------------------------------------------------------------------
MDBTObject^ MDocument::GetDBT(System::IntPtr dbtPtr)
{
	if (master == nullptr)
		return nullptr;
	DBTObject* p = (DBTObject*)dbtPtr.ToInt64();
	if (master->GetDBTObject() == p)
		return master;
	
	for each (IComponent^ component in Components)
	{
		MDBTSlave^ slave = dynamic_cast<MDBTSlave^>(component);
		if (slave == nullptr)
			continue;

		if (slave->GetDBTObject() == p)
			return slave;

		if (MDBTSlaveBuffered::typeid->IsInstanceOfType(slave))
		{
			MDBTObject^ subSlave = ((MDBTSlaveBuffered^) slave)->GetDBT(dbtPtr);
			if (subSlave != nullptr)
				return subSlave;
		}
	}
	
	return nullptr;
}

//-----------------------------------------------------------------------------
IDataManager^ MDocument::GetDataManager (SqlRecord* pRecord)
{
	for each (IComponent^ component in Components)
	{
		if (!IEasyBuilderContainer::typeid->IsInstanceOfType(component))
			continue;

		IDataManager^ dataManager = ((IDataManager^) component);
		if (((MSqlRecord^) dataManager->Record)->GetSqlRecord() == pRecord)
			return dataManager;
	}

	return nullptr;
}

//-----------------------------------------------------------------------------
System::IntPtr MDocument::GetFieldPtr(System::String^ name)
{
	if (!GetDocument())
		return System::IntPtr::Zero;

	CXMLVariable* pXmlVariable = GetDocument()->GetVariable (name);
	return pXmlVariable ? (System::IntPtr) pXmlVariable->GetDataObj() : System::IntPtr::Zero;
}

//-----------------------------------------------------------------------------
void MDocument::StartDuringInitDocument()
{
	componentsCreated = true;
}

//-----------------------------------------------------------------------------
void MDocument::EndDuringInitDocument()
{
	componentsCreated = false;
}

//-----------------------------------------------------------------------------
void MDocument::Remove(IComponent^ component)
{
	if (component == nullptr)
		return;

	if (component->GetType()->IsSubclassOf(MDBTMaster::typeid))
		master = nullptr;
	
	__super::Remove(component);
}

//-----------------------------------------------------------------------------
void MDocument::CallCreateComponents()
{
	if (!componentsCreated)
	{
		__super::CallCreateComponents();

		if (!Batch && Master != nullptr && !Master->HasCodeBehind)
			((MDBTMaster^) Master)->Open();
		
		componentsCreated = true;
	}
}


//-----------------------------------------------------------------------------
void MDocument::OnAfterCreateComponents()
{
	if (master == nullptr && !GetDocument()->m_pDBTMaster)
		return;

	if (master == nullptr && GetDocument()->m_pDBTMaster)
	{
		MDBTMaster^ dbtMaster = gcnew MDBTMaster((System::IntPtr) GetDocument()->m_pDBTMaster);
		AttachMaster(dbtMaster);
	}
	DBTMaster* pDbtMaster = ((DBTMaster*)((MDBTObject^)master)->GetDBTObject());
	DBTArray* pDBTSlaves = pDbtMaster->GetDBTSlaves();

	for (int nIdx = 0; nIdx < pDBTSlaves->GetSize(); nIdx++)
	{
		DBTSlave* pDBTSlave = pDBTSlaves->GetAt(nIdx);
		MDBTSlave^ slave = nullptr;
		if (pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
			slave = gcnew MDBTSlaveBuffered((System::IntPtr) (DBTSlaveBuffered*) pDBTSlave);
		else
			slave = gcnew MDBTSlave((System::IntPtr) pDBTSlave);

		AttachSlave(slave);
	}
}

//----------------------------------------------------------------------------
EDesignMode MDocument::DesignModeType::get()
{
	CBaseDocument* pDoc = GetDocument();
	if (!pDoc)
		return EDesignMode::None;
	switch (pDoc->GetDesignMode())
	{
	case CBaseDocument::DM_RUNTIME:
		return EDesignMode::Runtime;

	case CBaseDocument::DM_STATIC:
		return EDesignMode::Static;
	}
	return EDesignMode::None;
}

//-----------------------------------------------------------------------------
void MDocument::BrowseRecord(System::String^ primaryKey)
{
	SqlRecord* pMasterRec = (GetDocument()->m_pDBTMaster) ? GetDocument()->m_pDBTMaster->GetRecord() : NULL;
	if (pMasterRec)
	{
		pMasterRec->SetPrimaryKeyNameValue(primaryKey);
		AfxInvokeThreadProcedure<CAbstractFormDoc, BOOL, BOOL>(GetDocument()->GetFrameHandle(), GetDocument(), &CAbstractFormDoc::OnRadarRecordSelected, FALSE, FALSE);		
	}
}

//-----------------------------------------------------------------------------
void MDocument::BrowseRecord()
{
	if (GetDocument())
		GetDocument()->OnRadarRecordSelected(FALSE);	
}

//-----------------------------------------------------------------------------
void MDocument::Browse()
{
	if (GetDocument())
		GetDocument()->OnRefreshRowset ();	
}

//-----------------------------------------------------------------------------
void MDocument::AttachMaster(IDocumentMasterDataManager^ dbtMaster)
{
	if (!((MDBTObject^) dbtMaster)->GetDBTObject())
		return;

	// doppio attaching di un Master
	if (GetDocument()->m_pDBTMaster && !dbtMaster->HasCodeBehind)
	{
		ASSERT(FALSE);
		return;
	}

	// il Master è apportato dalla customizzazione
	if (!GetDocument()->m_pDBTMaster && !dbtMaster->HasCodeBehind)
	{
		DBTMaster* pDBTMaster = (DBTMaster*) ((MDBTMaster^)dbtMaster)->GetDBTObject();

		if (!pDBTMaster || !pDBTMaster->IsValidRecordsSchema(GetDocument()->GetMessages()))
		{
			ASSERT(FALSE);
			return;
		}

		GetDocument()->m_pDBTMaster = pDBTMaster;

		if (GetDocument()->GetXMLDocInfo())
			pDBTMaster->LoadXMLDBTInfo();
	}
	
	master = (MDBTMaster^)dbtMaster;
	this->Add(master);

}

//-----------------------------------------------------------------------------
void MDocument::AttachSlave(IDocumentSlaveDataManager^ dbtSlave)
{
	MDBTObject^ dbtObject = (MDBTObject^)dbtSlave;
	if (!dbtObject->GetDBTObject())
		return;

	if (master == nullptr)
	{
		ASSERT(FALSE);
		return;
	}
	
	BOOL bIsInvalid = IsInvalidDBT(dbtObject->GetDBTObject()->GetNamespace().ToString());

	if (!dbtSlave->HasCodeBehind)
		GetDocument()->GetMaster()->Attach((DBTSlave*)dbtObject->GetDBTObject());

	Add(dbtObject);

	dbtSlave->Master = master;

	if (bIsInvalid)
	{
		// se non e' già stato attach-ato lo devo attach-are per gestire bene la distruzione
		// visto che deve rimanere con HasCodeBehind a true
		if (!GetDocument()->GetMaster()->GetDBTSlaves()->GetBy(CString(dbtObject->Namespace->FullNameSpace)))
			GetDocument()->GetMaster()->Attach((DBTSlave*)dbtObject->GetDBTObject());
		dbtObject->IsValidComponent = false;
	}
}

//-----------------------------------------------------------------------------
List<MXMLSearchBookmark^>^ MDocument::GetXMLSearchBookmark(INameSpace^ nameSpace, [Out] int% version)
{
	MDBTObject^ dbt = GetDBT(nameSpace);
	List<MXMLSearchBookmark^>^ bookmarkList = gcnew List<MXMLSearchBookmark^>();

	if (dbt == nullptr)
		return bookmarkList;

	
	DBTObject* dbtPtr	= dbt->GetDBTObject();
	if (GetDocument() && dbtPtr && GetDocument()->CanLoadXMLDescription())
	{
		GetDocument()->LoadXMLDescription();
		CXMLDBTInfo* pXMLDBTInfo = dbtPtr->GetXMLDBTInfo();
		if (pXMLDBTInfo)
		{
			CXMLSearchBookmark* pBookmark = NULL;
			CXMLSearchBookmarkArray* pBookmarkArray = pXMLDBTInfo->GetXMLSearchBookmarkArray();
			if (pBookmarkArray && pBookmarkArray->GetCount() > 0)
			{
				version = pBookmarkArray->GetVersion();
				
				for (int i = 0; i < pBookmarkArray->GetCount(); i++)
				{
					pBookmark = pBookmarkArray->GetAt(i);
					if (pBookmark)
						bookmarkList->Add(gcnew MXMLSearchBookmark((System::IntPtr)pBookmark));
				}						
			}
		}
	}
	return bookmarkList;
}

//-----------------------------------------------------------------------------
int MDocument::GetFiscalYear()
{
	if (!GetDocument())
		return MIN_YEAR;

	return (int)GetDocument()->GetFiscalYear();
}

//-----------------------------------------------------------------------------
System::String^ MDocument::GetSosSuffix()
{
	return (GetDocument()) ? gcnew System::String(GetDocument()->GetSosSuffix()) :  System::String::Empty;
}

//-----------------------------------------------------------------------------
String^ MDocument::GetSosDocumentType()
{
	return (GetDocument()) ? gcnew String(GetDocument()->GetSosDocumentType()) : String::Empty;
}


//-----------------------------------------------------------------------------
String^ MDocument::GetCompanyName() 
{
	return (GetDocument()) ? gcnew String(GetDocument()->GetCompanyName()) : String::Empty;
}

//-----------------------------------------------------------------------------
String^ MDocument::GetTaxIdNumber()
{
	return (GetDocument()) ? gcnew String(GetDocument()->GetTaxIdNumber()) : String::Empty;
}

//-----------------------------------------------------------------------------
String^ MDocument::GetFiscalCode()
{
	return (GetDocument()) ? gcnew String(GetDocument()->GetFiscalCode()) : String::Empty;
}

//----------------------------------------------------------------------------
SortedDictionary<System::String^, System::String^>^ MDocument::GetUnWrappedHotLinks()
{
	if (!GetDocument())
		return nullptr;

	SortedDictionary<System::String^, System::String^>^ hotlinks = gcnew SortedDictionary<System::String^, System::String^>();

	for (int i = 0; i < GetDocument()->m_pHotKeyLinks->GetSize(); i++)
	{
		HotKeyLink* pHKL = (HotKeyLink*) GetDocument()->m_pHotKeyLinks->GetAt(i);
		if (!pHKL->GetName().IsEmpty() && !IsHotLinkWrapped(pHKL))
			hotlinks[gcnew System::String(pHKL->GetName())] = gcnew System::String(pHKL->GetHotlinkDescription() ? pHKL->GetHotlinkDescription()->GetTitle() : pHKL->GetNamespace().ToUnparsedString());
	}
	return hotlinks;
}

//----------------------------------------------------------------------------
System::IntPtr MDocument::GetReadOnlySessionPtr ()
{
	return Context == nullptr ? System::IntPtr::Zero : Context->GetReadOnlySessionPtr();
}

//----------------------------------------------------------------------------
System::IntPtr MDocument::GetUpdatableSessionPtr ()
{
	 return Context == nullptr ? System::IntPtr::Zero : Context->GetUpdatableSessionPtr();
}

//----------------------------------------------------------------------------
bool MDocument::IsHotLinkWrapped(HotKeyLink* pHotKeyLink)
{
	return GetWrappedHotLink((System::IntPtr) pHotKeyLink) != nullptr;
}

//----------------------------------------------------------------------------
bool MDocument::ErrorFound()
{
	return (GetDocument() && GetDocument()->m_pMessages) ? GetDocument()->m_pMessages->ErrorFound() == TRUE : false;
}

//----------------------------------------------------------------------------
bool MDocument::WarningFound()
{
	return (GetDocument() && GetDocument()->m_pMessages) ? GetDocument()->m_pMessages->WarningFound() == TRUE : false;
}

//----------------------------------------------------------------------------
bool MDocument::InfoFound()
{
	return (GetDocument() && GetDocument()->m_pMessages) ? GetDocument()->m_pMessages->InfoFound() == TRUE : false;
}

//----------------------------------------------------------------------------
bool MDocument::MessageFound()
{
	return (GetDocument() && GetDocument()->m_pMessages) ? GetDocument()->m_pMessages->MessageFound() == TRUE : false;
}

//----------------------------------------------------------------------------
void MDocument::StartMessageSession(System::String^ openingBanner)
{
	if (GetDocument() && GetDocument()->m_pMessages)
		GetDocument()->m_pMessages->StartSession(CString(openingBanner));
}

//----------------------------------------------------------------------------
void MDocument::EndMessageSession(System::String^ closingBanner)
{
	if (GetDocument() && GetDocument()->m_pMessages)
		GetDocument()->m_pMessages->EndSession(CString(closingBanner));
}

//----------------------------------------------------------------------------
void MDocument::AddMessage(System::String^ message, DiagnosticType type)
{
	if (GetDocument() && GetDocument()->m_pMessages)
		GetDocument()->m_pMessages->Add(CString(message), (CMessages::MessageType)((int)type));
}

//----------------------------------------------------------------------------
bool MDocument::ShowMessage(bool clearMessages)
{
	return (GetDocument() && GetDocument()->m_pMessages) ? GetDocument()->m_pMessages->Show(clearMessages) == TRUE: true;
}

//----------------------------------------------------------------------------
bool MDocument::PostMessageUM(int msg, System::IntPtr wParam, System::IntPtr lParam)
{
	return (GetDocument()) ? GetDocument()->PostMessage((UINT)msg ,(UINT)(WPARAM)(wParam.ToInt64()), (long)(LPARAM)(lParam.ToInt64())) == TRUE : true;
}

//----------------------------------------------------------------------------
long MDocument::SendMessageUM(int msg, System::IntPtr wParam, System::IntPtr lParam)
{
	return (GetDocument()) ? (long)((CBaseDocument*)GetDocument())->SendMessage((UINT)msg,(UINT)(wParam.ToInt64()), (long)(lParam.ToInt64())) : 0;
}
//----------------------------------------------------------------------------
void MDocument::SetBadData (MDataObj^ data, System::String^ message)
{
	if (GetDocument())
		GetDocument()->SetBadData(*data->GetDataObj(), CString(message));
}
//----------------------------------------------------------------------------
void MDocument::SetError (System::String^ message)
{
	if (GetDocument())
		GetDocument()->SetError(CString(message));
}
//----------------------------------------------------------------------------
void MDocument::SetWarning (System::String^ message)
{
	if (GetDocument())
		GetDocument()->SetWarning(CString(message));
}

//----------------------------------------------------------------------------
 List<System::String^>^ MDocument::GetAllMessages()
{
	List<System::String^>^ messages = gcnew List<System::String^>();

	if (GetDocument() && GetDocument()->m_pMessages)
	{
		CStringArray arValues;
		GetDocument()->m_pMessages->ToStringArray(arValues);
		for (int i=0; i <= arValues.GetUpperBound(); i++)
			messages->Add(gcnew System::String(arValues.GetAt(i)));
	}

	return messages;
}


//----------------------------------------------------------------------------
bool MDocument::StartTransaction()
{
	return Context == nullptr ? false : Context->StartTransaction();
}

//----------------------------------------------------------------------------
void MDocument::Commit()
{
	if (Context != nullptr)
		Context->Commit();
}

//----------------------------------------------------------------------------
void MDocument::Rollback()
{
	if (Context != nullptr)
		Context->Rollback();
}

//----------------------------------------------------------------------------
void MDocument::ExecuteBatch()
{
	if (GetDocument())
		GetDocument()->OnBatchStartStop();
}


//-----------------------------------------------------------------------------
bool MDocument::EnterInNewRecord ()
{
	return GetDocument() ? GetDocument()->NewRecord() == TRUE : false;
}

//-----------------------------------------------------------------------------
bool MDocument::EditCurrentRecord ()
{
	return GetDocument() ? GetDocument()->EditRecord () == TRUE : false;
}

//-----------------------------------------------------------------------------
bool MDocument::SaveCurrentRecord ()
{
	return GetDocument() ? GetDocument()->SaveRecord() == TRUE : false;
}

//-----------------------------------------------------------------------------
bool MDocument::DeleteCurrentRecord ()
{
	return GetDocument() ? GetDocument()->DeleteRecord	() == TRUE : false;
}

//-----------------------------------------------------------------------------
void MDocument::GoInBrowseMode ()
{
	if (GetDocument())
		GetDocument()->GoInBrowseMode();
}

//-----------------------------------------------------------------------------
void MDocument::GoInFindMode ()
{
	if (GetDocument())
		GetDocument()->FindRecord();
}

//-----------------------------------------------------------------------------
MHotLink^ MDocument::GetHotLink (System::String^ name)
{
	if (!GetDocument())
		return nullptr;

	//prima cerco nelle mie componenti per vedere se c'è quello tipizzato
	for each (IComponent^ cmp in Components)
	{
		if (!MHotLink::typeid->IsInstanceOfType(cmp))
			continue;
		MHotLink^ hkl = (MHotLink^) cmp;
		if (hkl->Name == name)
			return hkl;
	}
	//poi creo un wrapper dinamico per quello non tipizzato, se c'è
	for (int i = 0; i < GetDocument()->m_pHotKeyLinks->GetSize(); i++)
	{
		HotKeyLink* pHKL = (HotKeyLink*) GetDocument()->m_pHotKeyLinks->GetAt(i);
		if (pHKL && pHKL->GetName() == CString(name))
			return gcnew MHotLink (pHKL);
	}
	return nullptr;
}

//-----------------------------------------------------------------------------
MHotLink^ MDocument::GetWrappedHotLink (System::IntPtr hotKeyLinkPtr)
{
	if (!GetDocument())
		return nullptr;

	HotKeyLink* pHotKeyLink = (HotKeyLink*) hotKeyLinkPtr.ToInt64();
	for each (IComponent^ cmp in Components)
		if (MHotLink::typeid->IsInstanceOfType(cmp) && ((MHotLink^) cmp)->GetHotLink() == pHotKeyLink)
			return (MHotLink^) cmp;

	return nullptr;
}

//-----------------------------------------------------------------------------
System::String^ MDocument::Title::get()
{
	return GetDocument() ? gcnew System::String(GetDocument()->GetTitle()) : System::String::Empty;
}

//-----------------------------------------------------------------------------
void MDocument::Title::set(System::String^  value)
{
	if (GetDocument())
		GetDocument()->SetTitle(CString(value));
}

//-----------------------------------------------------------------------------
MDataManager^ MDocument::GetDataManager (System::String^ name)
{
	if (!GetDocument())
		return nullptr;

	//prima cerco nelle mie componenti per vedere se c'è quello tipizzato
	for each (IComponent^ cmp in Components)
	{
		if (!MDataManager::typeid->IsInstanceOfType(cmp))
			continue;
		MDataManager^ dm = (MDataManager^) cmp;
		if (dm->Name == name)
			return dm;
	}
	
	return nullptr;
}

//-----------------------------------------------------------------------------
System::String^ MDocument::ControllerType::get ()
{
	return EasyBuilderSerializer::DocumentControllerClassName;
}

//-----------------------------------------------------------------------------
void MDocument::UnlockAll ()
{
	if (GetDocument())
		GetDocument()->UnlockAll();
}

//-----------------------------------------------------------------------------
void MDocument::AddInvalidDBT(CString dbtNameSpace)
{
	if (!m_pInvalidDBTs)
		m_pInvalidDBTs = new CStringArray();
	
	// se non c'e' già la aggiungo
	if (GetInvalidDBTIdx(dbtNameSpace) < 0)
		m_pInvalidDBTs->Add(dbtNameSpace);	
}

//-----------------------------------------------------------------------------
void MDocument::RemoveInvalidDBT(CString dbtNameSpace)
{
	if (!m_pInvalidDBTs)
		return;

	int nPos = GetInvalidDBTIdx(dbtNameSpace);
	if (nPos >= 0)
		m_pInvalidDBTs->RemoveAt(nPos);
}

//-----------------------------------------------------------------------------
bool MDocument::IsInvalidDBT(CString dbtNameSpace)
{
	return GetInvalidDBTIdx(dbtNameSpace) >= 0;
}

//-----------------------------------------------------------------------------
int MDocument::GetInvalidDBTIdx(CString dbtNameSpace)
{
	if (!m_pInvalidDBTs)
		return -1;

	for (int i = 0; i <= m_pInvalidDBTs->GetUpperBound(); i++)
	{
		const CString sNameSpace = m_pInvalidDBTs->GetAt(i);
		if (sNameSpace.CompareNoCase(dbtNameSpace) == 0)
			return i;
	}

	return -1;
}
/////////////////////////////////////////////////////////////////////////////
// 				class BusinessObjectSerializer Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
Type^ BusinessObjectSerializer::ComponentSerializedAs::get()
{
	return BusinessObject::typeid;
}

//----------------------------------------------------------------------------	
TypeDeclaration^ BusinessObjectSerializer::SerializeClass (SyntaxTree^ syntaxTree, IComponent^ component) 
{
	TypeDeclaration^ aClass = __super::SerializeClass(syntaxTree, component);

	if (aClass == nullptr)
		return nullptr;

	BusinessObject^ businessObject = (BusinessObject^) component;

	AttributeSection^ documentNamespaceAttr = AstFacilities::GetAttributeSection(
		AstFacilities::GetAttribute(
			DocumentNamespaceAttribute::typeid->FullName,
			gcnew PrimitiveExpression(businessObject->Namespace->FullNameSpace)
			)
		);

	aClass->Attributes->Add(documentNamespaceAttr);
 
	Statement^ constrStmt = AstFacilities::GetAssignmentStatement
		(
			gcnew IdentifierExpression(EasyBuilderSerializer::StaticControllerVariableName),
			AstFacilities::GetObjectCreationExpression(businessObject->ControllerType, gcnew ThisReferenceExpression())
			);

	// Costruttore di default
	ConstructorDeclaration^ constr = gcnew ConstructorDeclaration();
	constr->Modifiers = Modifiers::Public;
	constr->Name = aClass->Name;
	aClass->Members->Add(constr);

	constr->Parameters->Add(gcnew ParameterDeclaration(gcnew SimpleType(IntPtr::typeid->FullName), WrappedObjectParamName, ICSharpCode::NRefactory::CSharp::ParameterModifier::None));
	
	constr->Initializer = AstFacilities::GetConstructorInitializer(gcnew IdentifierExpression(WrappedObjectParamName));

	constr->Body = gcnew BlockStatement();
	constr->Body->Statements->Add(constrStmt);
	constr->Body->Statements->Add(AstFacilities::GetInvocationStatement(gcnew ThisReferenceExpression(), "CallCreateComponents"));
	
	// Create di default
	MethodDeclaration^ defaultCreate = gcnew MethodDeclaration();
	defaultCreate->Name = CreateMethodName;
	defaultCreate->Modifiers = Modifiers::Public | Modifiers::Static;
	defaultCreate->ReturnType = gcnew SimpleType (businessObject->SerializedType);

	defaultCreate->Body = gcnew BlockStatement();

	MemberReferenceExpression^ memberRef = gcnew MemberReferenceExpression(
			gcnew IdentifierExpression(businessObject->SerializedType),
			CreateMethodName
		);
	memberRef->TypeArguments->Add(gcnew SimpleType(businessObject->SerializedType));

	List<ICSharpCode::NRefactory::CSharp::Expression^>^ arguments = gcnew List<ICSharpCode::NRefactory::CSharp::Expression^>();
	arguments->Add(gcnew PrimitiveExpression(businessObject->Namespace->FullNameSpace));

	defaultCreate->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				gcnew InvocationExpression(memberRef, arguments)
			)
		);
	aClass->Members->Add(defaultCreate);

	// Create con Context
	MethodDeclaration^ contextCreate = gcnew MethodDeclaration();
	contextCreate->Name = CreateMethodName;
	contextCreate->Modifiers = Modifiers::Public | Modifiers::Static;
	contextCreate->Parameters->Add(gcnew ParameterDeclaration(gcnew SimpleType(DocumentContext::typeid->FullName), ContextParamName, ParameterModifier::None));
	contextCreate->ReturnType = gcnew SimpleType (businessObject->SerializedType);
	
	contextCreate->Body = gcnew BlockStatement();

	memberRef = gcnew MemberReferenceExpression(
		gcnew IdentifierExpression(businessObject->SerializedType),
		CreateMethodName
	);
	memberRef->TypeArguments->Add(gcnew SimpleType(businessObject->SerializedType));

	arguments = gcnew List<ICSharpCode::NRefactory::CSharp::Expression^>();
	arguments->Add(gcnew PrimitiveExpression(businessObject->Namespace->FullNameSpace));
	arguments->Add(gcnew IdentifierExpression(ContextParamName));

	contextCreate->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				gcnew InvocationExpression(memberRef, arguments)
			)
		);
	aClass->Members->Add(contextCreate);

	// CreateUnattended
	MethodDeclaration^ createUnattended = gcnew MethodDeclaration();
	createUnattended->Name = CreateUnattendedMethodName;
	createUnattended->Modifiers = Modifiers::Public | Modifiers::Static;
	createUnattended->Parameters->Add(gcnew ParameterDeclaration(gcnew SimpleType(DocumentContext::typeid->FullName), ContextParamName, ParameterModifier::None));
	createUnattended->ReturnType = gcnew SimpleType (businessObject->SerializedType);

	createUnattended->Body = gcnew BlockStatement();

	memberRef = gcnew MemberReferenceExpression(
		gcnew IdentifierExpression(businessObject->SerializedType),
		CreateUnattendedMethodName
	);
	memberRef->TypeArguments->Add(gcnew SimpleType(businessObject->SerializedType));

	arguments = gcnew List<ICSharpCode::NRefactory::CSharp::Expression^>();
	arguments->Add(gcnew PrimitiveExpression(businessObject->Namespace->FullNameSpace));
	arguments->Add(gcnew IdentifierExpression(ContextParamName));

	createUnattended->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				gcnew InvocationExpression(memberRef, arguments)
			)
		);

	aClass->Members->Add(createUnattended);

	// CreateHidden
	MethodDeclaration^ createHidden = gcnew MethodDeclaration();
	createHidden->Name = CreateHiddenMethodName;
	createHidden->Modifiers = Modifiers::Public | Modifiers::Static;
	createHidden->Parameters->Add(gcnew ParameterDeclaration(gcnew SimpleType(DocumentContext::typeid->FullName), ContextParamName, ParameterModifier::None));
	createHidden->ReturnType = gcnew SimpleType (businessObject->SerializedType);

	createHidden->Body = gcnew BlockStatement();

	memberRef = gcnew MemberReferenceExpression(
		gcnew IdentifierExpression(businessObject->SerializedType),
		CreateHiddenMethodName
	);
	memberRef->TypeArguments->Add(gcnew SimpleType(businessObject->SerializedType));

	arguments = gcnew List<ICSharpCode::NRefactory::CSharp::Expression^>();
	arguments->Add(gcnew PrimitiveExpression(businessObject->Namespace->FullNameSpace));
	arguments->Add(gcnew IdentifierExpression(ContextParamName));

	createHidden->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				gcnew InvocationExpression(memberRef, arguments)
			)
		);

	aClass->Members->Add(createHidden);

	System::Collections::Generic::IList<MethodDeclaration^>^ methods = SerializeWebMethods(businessObject);
	if (methods != nullptr)
	{
		for (int i = 0; i < methods->Count; i++)
		{
			aClass->Members->Add(methods[i]);
		}
	}

	return aClass;
};


/////////////////////////////////////////////////////////////////////////////
// 				class BusinessObject Implementation
/////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------	
BusinessObject::BusinessObject (System::IntPtr wrappedObject)
	:
	MDocument(wrappedObject)
{
}

//----------------------------------------------------------------------------	
BusinessObject::~BusinessObject ()
{
	this->!BusinessObject();
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------	
BusinessObject::!BusinessObject ()
{
	Close();
}
//-----------------------------------------------------------------------------
List<System::String^>^ BusinessObject::InternalClasses::get()
{ 
	if (!GetDocument())
		return nullptr;

	List<System::String^>^ classes = gcnew List<System::String^>();
	CRuntimeClass* pClass = GetDocument()->GetRuntimeClass();
	do
	{
		classes->Add(gcnew System::String(CString(pClass->m_lpszClassName)));
		pClass = pClass->m_pfnGetBaseClass();
	}
	while (pClass != NULL && pClass != RUNTIME_CLASS(CCmdTarget));
	return classes;
}

//-----------------------------------------------------------------------------
System::String^ BusinessObject::ControllerType::get ()
{
	return EasyBuilderSerializer::BaseControllerClassName;
}
 
//-----------------------------------------------------------------------------
bool BusinessObject::AutoValueChanged::get ()
{
	return autoValueChanged;
}

//-----------------------------------------------------------------------------
void BusinessObject::AutoValueChanged::set (bool value)
{
	autoValueChanged = value;
}

//-----------------------------------------------------------------------------
void BusinessObject::BrowseRecord()
{
	if (GetDocument() && GetDocument()->GetADM())
		GetDocument()->BrowseRecord();
	else
		__super::BrowseRecord();
}

//-----------------------------------------------------------------------------
bool BusinessObject::EnterInNewRecord ()
{
	BOOL ok = GetDocument() && GetDocument()->GetADM() ? GetDocument()->GetADM()->ADMNewDocument() : __super::EnterInNewRecord();

	if (ok)
		Modified = true;
	
	return ok == TRUE;
}

//-----------------------------------------------------------------------------
bool BusinessObject::EditCurrentRecord ()
{
	BOOL ok = GetDocument() && GetDocument()->GetADM() ? GetDocument()->GetADM()->ADMEditDocument() : __super::EditCurrentRecord();

	if (ok)
		Modified = true;
	
	return ok == TRUE;
}

//-----------------------------------------------------------------------------
bool BusinessObject::SaveCurrentRecord ()
{
	return GetDocument() && GetDocument()->GetADM() ? GetDocument()->GetADM()->ADMSaveDocument() == TRUE : __super::SaveCurrentRecord() == TRUE;
}

//-----------------------------------------------------------------------------
bool BusinessObject::DeleteCurrentRecord ()
{
	return GetDocument() && GetDocument()->GetADM() ? GetDocument()->GetADM()->ADMDeleteDocument() == TRUE : __super::DeleteCurrentRecord() == TRUE;
}

//-----------------------------------------------------------------------------
void BusinessObject::GoInBrowseMode ()
{
	if (GetDocument() && GetDocument()->GetADM())
		GetDocument()->GetADM()->ADMGoInBrowseMode();
	else
		__super::GoInBrowseMode();
}

//-----------------------------------------------------------------------------
void BusinessObject::GetEasyBuilderComponents(List<Type^>^ requestedTypes, List<EasyBuilderComponent^>^ components)
{
	if (!GetDocument())
		return;
	
	__super::GetEasyBuilderComponents(requestedTypes, components);

	CBusinessObjectComponentRequest* pRequest = new CBusinessObjectComponentRequest();
	pRequest->Types = gcnew System::Collections::ArrayList(requestedTypes);

	::Array arComponents;
	GetDocument()->GetComponents(pRequest, arComponents);

	for (int i=0; i <= arComponents.GetUpperBound(); i++)
	{
		CBusinessObjectComponent* pResponse = (CBusinessObjectComponent*) arComponents.GetAt(i);
		if (pResponse)
			components->Add(pResponse->Component);
	}
	delete pRequest;
}

//-----------------------------------------------------------------------------
void BusinessObject::OnClosing()
{
	Closing(this, gcnew EasyBuilderEventArgs());
}

//-----------------------------------------------------------------------------
void BusinessObject::OnSaved()
{
	Saved(this, gcnew EasyBuilderEventArgs());
}

//-----------------------------------------------------------------------------
void BusinessObject::OnDataLoaded()
{
	DataLoaded(this, gcnew EasyBuilderEventArgs());
}
//-----------------------------------------------------------------------------
void BusinessObject::OnBatchExecuted()
{
	BatchExecuted(this, gcnew EasyBuilderEventArgs());
}

//-----------------------------------------------------------------------------
void BusinessObject::DispatchEvent(BOEvent evnt)
{
	switch (evnt)
	{
	case BOEvent::OnPrepareAuxData:
		OnDataLoaded();
		break;
	case BOEvent::OnGoInBrowseMode:
		OnSaved();
		break;
	case BOEvent::OnBeforeCloseDocument:
		OnClosing();
		break;
	case BOEvent::OnAfterBatchExecute:
		OnBatchExecuted();
		break;

	}
}

/////////////////////////////////////////////////////////////////////////////
// 				class MXMLVariable Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
MXMLVariable::MXMLVariable(CXMLVariable* pXMLVariable) 
{  
	m_pXMLVariable = pXMLVariable; 
}

//-----------------------------------------------------------------------------
MDataObj^ MXMLVariable::DataObj::get()
{ 
	MDataObj^ mDataObj = MDataObj::Create(m_pXMLVariable->GetDataObj());
	
	return mDataObj;
}

//-----------------------------------------------------------------------------
String^ MXMLVariable::Name::get()
{ 
	return gcnew String (m_pXMLVariable->GetName());
}

/////////////////////////////////////////////////////////////////////////////
// 				class MXMLVariableArray Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
MXMLVariableArray::MXMLVariableArray(CXMLVariableArray* pXMLVariableArray)
{
	m_pXMLVariableArray = pXMLVariableArray;
}

//-----------------------------------------------------------------------------
List<MXMLVariable^>^ MXMLVariableArray::XMLVariables::get()
{
	List<MXMLVariable^>^ variableList = gcnew List<MXMLVariable^>();
	CXMLVariable* pVariable = NULL;
	if (m_pXMLVariableArray)
	{
		for (int i = 0; i < m_pXMLVariableArray->GetSize(); i++)
		{
			pVariable = m_pXMLVariableArray->GetAt(i);
			if (pVariable)
				variableList->Add(gcnew MXMLVariable(pVariable));
		}
	}
	return variableList;
}

//-----------------------------------------------------------------------------
MXMLVariable^ MXMLVariableArray::GetVariable(System::String^ name)
{
	CXMLVariable* pVariable = NULL;
	if (m_pXMLVariableArray)
	{ 
		int nIdx = m_pXMLVariableArray->GetVariable((CString)name);
		if (nIdx > -1)
			pVariable = m_pXMLVariableArray->GetAt(nIdx);
	}
	return (pVariable) ? gcnew MXMLVariable(pVariable) : nullptr;	
}
		
//
///////////////////////////////////////////////////////////////////////////////
//// 				class MClientDocBag Implementation
///////////////////////////////////////////////////////////////////////////////
//MClientDocBag::MClientDocBag (const CDocumentDescription* pDocInfo, const CServerDocDescription* pServerInfo, const CClientDocDescription* pClientDocInfo)
//{
//	m_pDocumentInfo	 = pDocInfo;
//	m_pClientDocInfo = pClientDocInfo;
//	m_pServerInfo	 = pServerInfo;
//}
//
////-----------------------------------------------------------------------------
//String^ MClientDocBag::ClientDocAssemblyFullName::get()
//{
//	return GetAssemblyName(m_pClientDocInfo->GetNamespace(), m_pClientDocInfo->IsManaged());
//}
//
////-----------------------------------------------------------------------------
//String^ MClientDocBag::DocumentAssemblyFullName::get()
//{
//	return GetAssemblyName(m_pDocumentInfo->GetNamespace(),  m_pDocumentInfo->IsDynamic());
//}
//
////-----------------------------------------------------------------------------
//String^  MClientDocBag::Controller::get()
//{ 
//	return gcnew String(m_pClientDocInfo->GetController());
//}
//
////-----------------------------------------------------------------------------
//String^  MClientDocBag::ServerDocument::get()
//{ 
//	CString sClass = m_pServerInfo->GetClass();
//	if (sClass.IsEmpty())
//	{
//		sClass = m_pDocumentInfo->GetManagedHierarchy();
//		int nPos = sClass.ReverseFind(DOT_CHAR);
//		if (nPos > 0)
//			sClass = sClass.Mid(nPos + 1);
//	}
//
//	return gcnew String(sClass);
//
//}
//
////-----------------------------------------------------------------------------
//NameSpace^  MClientDocBag::ServerDocumentNameSpace::get()
//{ 
//	return gcnew NameSpace(gcnew String(m_pDocumentInfo->GetNamespace().ToString()));
//}
//
////-----------------------------------------------------------------------------
//String^ MClientDocBag::GetAssemblyName (const CTBNamespace& sNameSpace, BOOL bIsManaged)
//{
//	CString sName = sNameSpace.GetApplicationName() + DOT_CHAR + sNameSpace.GetModuleName();
//	if (!bIsManaged)
//		sName += + sNameSpace.GetObjectName(CTBNamespace::LIBRARY);
//	
//	sName += _T(".dll");
//
//	String^ dllName = gcnew String(sName);
//	return System::IO::Path::Combine(gcnew String(AfxGetPathFinder()->GetExePath()), dllName);
//}
//
///////////////////////////////////////////////////////////////////////////////
//// 				class MClientDoc Implementation
///////////////////////////////////////////////////////////////////////////////
//MClientDoc::MClientDoc (MDocument^ serverDocument)
//	:
//	MDocument(serverDocument->GetDocumentPtr())
//{
//	this->serverDocument = serverDocument;
//}
//
////-----------------------------------------------------------------------------
//MDocument^  MClientDoc::ServerDocument::get()
//{ 
//	return serverDocument;
//}
//
////-----------------------------------------------------------------------------
//void MClientDoc::ServerDocument::set (MDocument^ value)
//{
//	serverDocument = value;
//}
