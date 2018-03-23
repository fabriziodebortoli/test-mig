#include "stdafx.h"

#include <TbNameSolver\ApplicationContext.h>
#include <TbNameSolver\ThreadContext.h>

#include <TbNameSolver\TBResourceLocker.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbGeneric\TBStrings.h>
#include <TbGes\DocumentSession.h>
#include <TbParser\XmlBaseDescriptionParser.h>

#include "Behaviour.h"
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

////////////////////////////////////////////////////////////////////////////////
///							IBehaviourRequest
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
IBehaviourRequest::IBehaviourRequest(CObject* pOwner, CBehavioursRegistryService* pReceiver)
	:
	m_pOwner    (pOwner),
	m_pReceiver (pReceiver),
	m_bEnabled	(TRUE)
{
}

//-----------------------------------------------------------------------------	
inline const CObject* IBehaviourRequest::GetOwner () const { return m_pOwner; }
//-----------------------------------------------------------------------------	
inline CBehavioursRegistryService* IBehaviourRequest::GetReceiver () const { return m_pReceiver; }
//-----------------------------------------------------------------------------	
inline const BOOL IBehaviourRequest::GetEnabled () const { return m_bEnabled; }
//-----------------------------------------------------------------------------	
inline void IBehaviourRequest::SetEnabled (const BOOL bValue) { m_bEnabled = bValue; }
//-----------------------------------------------------------------------------	
void IBehaviourRequest::SetReceiver (CBehavioursRegistryService* pReceiver)
{
	m_pReceiver = pReceiver;
}

//-----------------------------------------------------------------------------	
void IBehaviourRequest::SetService	(const CString& sPartialNamespace)
{
	// TODOBRUNA okkio che qui non vedi il caricamento delle DLL
	// SetReceiver(AfxGetTbCmdManager()->GetBehaviour(aNs.ToString()));
	CTBNamespace aNs (CTBNamespace::BEHAVIOUR, sPartialNamespace);
	SetReceiver(AfxGetBehavioursRegistry()->GetBehaviour(aNs.ToString()));
}

//-----------------------------------------------------------------------------	
void IBehaviourRequest::SetOwner (CObject* pOwner)
{
	m_pOwner = pOwner;
}

//-----------------------------------------------------------------------------	
void IBehaviourRequest::OnRequestExecuted (const BehaviourEvents& evnt, IBehaviourContext* pContext)
{
}

////////////////////////////////////////////////////////////////////////////////
///							IBehaviourService
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IBehaviourService::IBehaviourService()
	:
	m_pContext(NULL)
{
}

//-----------------------------------------------------------------------------	
IBehaviourContext*	IBehaviourService::GetContext()
{
	return m_pContext;
}

//-----------------------------------------------------------------------------	
void IBehaviourService::SetContext (IBehaviourContext* pContext)
{
	m_pContext = pContext;
}

//-----------------------------------------------------------------------------	
bool IBehaviourService::FireEvent (const BehaviourEvents& evnt, IBehaviourRequest* pRequest)
{
	return pRequest->GetEnabled() && CanExecuteRequest(evnt, pRequest) && ExecuteRequest(evnt, pRequest);
}

//-----------------------------------------------------------------------------	
void IBehaviourService::OnInitService ()
{
	if (!GetInstanceName().IsEmpty())
		return;

	CObject* pObject = dynamic_cast<CObject*>(this);
	if (!pObject)
		return; 
	CBehavioursRegistryService* pEntry = AfxGetBehavioursRegistry()->GetBehaviour(pObject->GetRuntimeClass());
	if (pEntry)
		SetInstanceName(pEntry->GetNamespace());
}

//-----------------------------------------------------------------------------	
bool IBehaviourService::ExecuteRequest (const BehaviourEvents& evnt, IBehaviourRequest* pRequest)
{
	return true;
}

//-----------------------------------------------------------------------------	
CString	IBehaviourService::GetInstanceName() const
{ 
	return m_sInstanceName; 
}

//-----------------------------------------------------------------------------	
void IBehaviourService::SetInstanceName(const CString& sName) 
{ 
	m_sInstanceName = sName; 
}


////////////////////////////////////////////////////////////////////////////////
///							IBehaviourContext
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IBehaviourContext::IBehaviourContext ()
	:
	m_pClass (NULL)
{
}

//-----------------------------------------------------------------------------	
IBehaviourContext::~IBehaviourContext ()
{
	Dispose();
}

//-----------------------------------------------------------------------------	
void IBehaviourContext::Dispose()
{
	for (int i = m_Services.GetUpperBound(); i >= 0; i--)
	{
		CObject* pObject = dynamic_cast<CObject*>(m_Services.GetAt(i));
		if (pObject)
			delete pObject;
	}

	m_Services.RemoveAll();
}


//-----------------------------------------------------------------------------	
CRuntimeClass* IBehaviourContext::GetContextClass() const
{
	return m_pClass;
}

//-----------------------------------------------------------------------------	
void IBehaviourContext::SetContextClass (CRuntimeClass* pClass)
{
	m_pClass = pClass;
}

//-----------------------------------------------------------------------------	
CArray<IBehaviourService*>&	IBehaviourContext::GetServices() 
{
	return m_Services;
}

//-----------------------------------------------------------------------------	
CArray<IBehaviourConsumer*>& IBehaviourContext::GetConsumers()
{
	return m_Consumers;
}

//-----------------------------------------------------------------------------	
void IBehaviourContext::AddConsumer	(IBehaviourConsumer* pConsumer)
{
	m_Consumers.Add(pConsumer);
}

//-----------------------------------------------------------------------------	
void IBehaviourContext::RemoveConsumer	(IBehaviourConsumer* pConsumer)
{
	for (int i=m_Consumers.GetUpperBound(); i >= 0; i--)
	{
		if (pConsumer == m_Consumers.GetAt(i))
		{
			m_Consumers.RemoveAt(i);
			break;
		}
	}
}

//-----------------------------------------------------------------------------	
IBehaviourContext*  IBehaviourContext::GetContext	()
{
	return this;
}

// it returns a services by name
//-----------------------------------------------------------------------------	
IBehaviourService* IBehaviourContext::GetService(const CString& sName)
{
	for (int i = 0; i <= m_Services.GetUpperBound(); i++)
	{
		IBehaviourService* pCurr = dynamic_cast<IBehaviourService*>(m_Services.GetAt(i));
		if (pCurr->GetInstanceName().CompareNoCase(sName) == 0)
			return pCurr;
	}

	return NULL;
}


// it returns a services in order to execute a request
//-----------------------------------------------------------------------------	
IBehaviourService* IBehaviourContext::GetService(IBehaviourRequest* pRequest)
{
	CBehavioursRegistryService* pReceiver = pRequest->GetReceiver();
	if (!pReceiver)
		return NULL;

	IBehaviourService* pBehaviour = NULL;
	for (int i=0; i <= m_Services.GetUpperBound(); i++)
	{
		IBehaviourService* pCurr = dynamic_cast<IBehaviourService*>(m_Services.GetAt(i));

		if (pCurr->IsCompatibleWith(pRequest) )
		{
			pBehaviour = pCurr;
			break;
		}
	}

	// if I have class registered in registry a can create service
	if (!pBehaviour && pReceiver->GetClass())
		pBehaviour = CreateBehaviourService(pReceiver);
	
	return pBehaviour;
}

//-----------------------------------------------------------------------------	
IBehaviourService*  IBehaviourContext::CreateBehaviourService (CBehavioursRegistryService* pRegistryEntry)
{
	IBehaviourService* pBehaviour = dynamic_cast<IBehaviourService*>(pRegistryEntry->GetClass()->CreateObject());
	pBehaviour->SetInstanceName(pRegistryEntry->GetNamespace());
	pBehaviour->SetContext(GetContext());
	m_Services.Add(pBehaviour);
	
	pBehaviour->OnInitService();
	
	return pBehaviour;
}

//-----------------------------------------------------------------------------	
void IBehaviourContext::AddBehaviourService(IBehaviourService* pService)
{
	pService->SetContext(GetContext());
	m_Services.Add(pService);
	pService->OnInitService();

}

//-----------------------------------------------------------------------------	
void IBehaviourContext::RemoveBehaviourService(IBehaviourService* pService)
{
	pService->SetContext(NULL);
	for (int i = m_Services.GetUpperBound(); i >= 0; i--)
	{
		if (m_Services.GetAt(i) == pService)
		{
			m_Services.RemoveAt(i);
			break;
		}
	}
}

//-----------------------------------------------------------------------------	
BOOL IBehaviourContext::FireBehaviour (const BehaviourEvents& evnt, BOOL bUpdateClient /*FALSE*/)
{
	bool ok = true;
	for (int c=0; c <= m_Consumers.GetUpperBound(); c++)
	{
		IBehaviourConsumer* pConsumer = m_Consumers.GetAt(c);
		
		for (int i=0; i <= pConsumer->m_Requests.GetUpperBound(); i++)
		{
			IBehaviourRequest* pRequest = (IBehaviourRequest*) pConsumer->m_Requests.GetAt(i);
			if (!pRequest)
				continue;
			IBehaviourService* pBehaviour = GetService(pRequest);
			if (pBehaviour)
				ok = ok && pBehaviour->FireEvent(evnt, pRequest);
		}
	}

	if (ok && bUpdateClient)
		UpdateBehavioursClient();

	return ok == true;
}

//-----------------------------------------------------------------------------	
void IBehaviourContext::UpdateBehavioursClient()
{
	if (AfxIsRemoteInterface())
	{
		CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
		if (pSession)
			pSession->PushBehavioursToClient(this);
	}
}

//-----------------------------------------------------------------------------	
void IBehaviourContext::GetRequestsBy (CRuntimeClass* pClass, CArray<IBehaviourRequest*>& output)
{
	output.RemoveAll();

	bool ok = true;
	for (int c=0; c <= m_Consumers.GetUpperBound(); c++)
	{
		IBehaviourConsumer* pConsumer = m_Consumers.GetAt(c);
		
		for (int i=0; i <= pConsumer->m_Requests.GetUpperBound(); i++)
		{
			IBehaviourRequest* pRequest = (IBehaviourRequest*) pConsumer->m_Requests.GetAt(i);
			if (pRequest && pRequest->IsKindOf(pClass))
				output.Add(pRequest);
		}
	}
}

////////////////////////////////////////////////////////////////////////////////
///						IBehaviourConsumer
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IBehaviourConsumer::IBehaviourConsumer()
{
}

//-----------------------------------------------------------------------------	
IBehaviourConsumer::~IBehaviourConsumer()
{
	for (int i=m_Requests.GetUpperBound(); i >=0; i--)
		delete m_Requests.GetAt(i);
	
	m_Requests.RemoveAll();
}

//-----------------------------------------------------------------------------	
void IBehaviourConsumer::AddRequest (IBehaviourRequest* pRequest)
{
	m_Requests.Add(pRequest);
}

//-----------------------------------------------------------------------------	
CArray<IBehaviourRequest*>&	IBehaviourConsumer::GetRequests()
{
	return m_Requests;
}

////////////////////////////////////////////////////////////////////////////////
///						CBehaviourRegistry
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
CBehavioursRegistry::CBehavioursRegistry()
{
}

//-----------------------------------------------------------------------------	
CBehavioursRegistry::~CBehavioursRegistry()
{
	for (int i=m_Services.GetUpperBound(); i >= 0; i--)
		delete m_Services.GetAt(i);
	
	for (int i=m_Entities.GetUpperBound(); i >= 0; i--)
		delete m_Entities.GetAt(i);

	m_Services.RemoveAll();
	m_Entities.RemoveAll();
}

//-----------------------------------------------------------------------------	
int	CBehavioursRegistry::GetBehavioursCount	() const
{
	return m_Services.GetSize();
}

//-----------------------------------------------------------------------------	
int	CBehavioursRegistry::GetEntitiesCount	() const
{
	return m_Entities.GetSize();
}

//-----------------------------------------------------------------------------	
CBehavioursRegistryService* CBehavioursRegistry::GetBehaviour(const int& i) const
{
	if (i >= 0 && i <= m_Services.GetUpperBound())
		return m_Services.GetAt(i);

	return NULL;
}

//-----------------------------------------------------------------------------	
CBehavioursRegistryService*	CBehavioursRegistry::GetBehaviour(CRuntimeClass* pClass) const
{
	for (int i = 0; i <= m_Services.GetUpperBound(); i++)
	{
		CBehavioursRegistryService* pService = m_Services.GetAt(i);
		if (pService->GetClass() == pClass)
			return pService;
	}
	return NULL;

}

//-----------------------------------------------------------------------------	
CBehavioursRegistryEntity* CBehavioursRegistry::GetEntity(const int& i) const
{
	if (i >= 0 && i <= m_Entities.GetUpperBound())
		return m_Entities.GetAt(i);

	return NULL;
}

//-----------------------------------------------------------------------------	
void CBehavioursRegistry::RegisterBehaviour (const CString& sNamespace, const CString& sTitle)
{
	if (sNamespace.IsEmpty())
	{
		ASSERT(FALSE);
		return;
	}

	CBehavioursRegistryService* pItem = new CBehavioursRegistryService();
	pItem->m_sNamespace = sNamespace;
	pItem->m_sTitle = sTitle;

	TB_LOCK_FOR_WRITE();	
	m_Services.Add (pItem);
}

//-----------------------------------------------------------------------------	
void CBehavioursRegistry::RegisterBehaviour (const CString& sNamespace, CRuntimeClass* pClass)
{
	if (sNamespace.IsEmpty() || !pClass)
	{
		ASSERT(FALSE);
		return;
	}
	
	CBehavioursRegistryService* pBehaviour = NULL;
	TB_LOCK_FOR_WRITE();	
	for (int i=0; i <= m_Services.GetUpperBound(); i++)
	{
		pBehaviour = m_Services.GetAt(i);
		if (pBehaviour->m_sNamespace.CompareNoCase(sNamespace) == 0)
			pBehaviour->m_pClass = pClass;
	}
}

//-----------------------------------------------------------------------------	
void CBehavioursRegistry::RegisterEntity (const CString& sNamespace, const CString& sService, const CString& sTitle)
{
	if (sNamespace.IsEmpty() || sService.IsEmpty())
	{
		ASSERT(FALSE);
		return;
	}

	CBehavioursRegistryEntity* pItem = new CBehavioursRegistryEntity();
	pItem->m_sNamespace = sNamespace;
	pItem->m_sService = sService;
	pItem->m_sTitle = sTitle;

	TB_LOCK_FOR_WRITE();	
	m_Entities.Add (pItem);
}

//------------------------------------------------------------------------------
CBehavioursRegistryService* CBehavioursRegistry::GetBehaviour(const CString& sNamespace) const
{
	TB_LOCK_FOR_READ();

	CBehavioursRegistryService* pService = NULL;
	for (int i=0; i <= m_Services.GetUpperBound(); i++)
	{
		pService = m_Services.GetAt(i);
		if (pService->m_sNamespace.CompareNoCase(sNamespace) == 0)
			return pService;
	}
	
	return NULL;
}

//------------------------------------------------------------------------------
CBehavioursRegistryEntity* CBehavioursRegistry::GetEntity(const CString& sNamespace) const
{
	TB_LOCK_FOR_READ();

	CBehavioursRegistryEntity* pEntity = NULL;
	for (int i=0; i <= m_Entities.GetUpperBound(); i++)
	{
		pEntity = m_Entities.GetAt(i);
		if (pEntity->m_sNamespace.CompareNoCase(sNamespace) == 0)
			return pEntity;
	}
	
	return NULL;
}
//------------------------------------------------------------------------------
CBehavioursRegistryConstPtr AfxGetBehavioursRegistry()
{
	return CBehavioursRegistryConstPtr(AfxGetApplicationContext()->GetObject<CBehavioursRegistry>(&CApplicationContext::GetBehavioursRegistry), FALSE);
}

//------------------------------------------------------------------------------
CBehavioursRegistryPtr AfxGetWritableBehavioursRegistry () 
{
	return CBehavioursRegistryPtr(AfxGetApplicationContext()->GetObject<CBehavioursRegistry>(&CApplicationContext::GetBehavioursRegistry), TRUE);
}


////////////////////////////////////////////////////////////////////////////////
static const TCHAR szXmlRoot[] = _T("BehaviourObjects");

static const TCHAR szXmlEntity[] = _T("/BehaviourObjects/Entities/Entity");
static const TCHAR szXmlBehaviour[] = _T("/BehaviourObjects/Services/Service");

static const TCHAR szXmlNamespace[] = _T("namespace");
static const TCHAR szXmlLocalize[] = _T("localize");
static const TCHAR szXmlService[] = _T("service");

////////////////////////////////////////////////////////////////////////////////
//				class CBehavioursContent implementation
////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CBehavioursContent, CXMLSaxContent);
//-----------------------------------------------------------------------------
CBehavioursContent::CBehavioursContent()
{
}

//-----------------------------------------------------------------------------
CString CBehavioursContent::OnGetRootTag() const
{
	return szXmlRoot;
}

//-----------------------------------------------------------------------------
void CBehavioursContent::OnBindParseFunctions()
{
	BIND_PARSE_ATTRIBUTES(szXmlEntity, &CBehavioursContent::ParseEntity);
	BIND_PARSE_ATTRIBUTES(szXmlBehaviour, &CBehavioursContent::ParseService);
}

//-----------------------------------------------------------------------------
int	 CBehavioursContent::ParseEntity(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	CString sNamespace = arAttributes.GetAttributeByName(szXmlNamespace);
	if (sNamespace.IsEmpty())
	{
		AddError(_TB("Entity element without 'namespace' attribute! "));
		return CXMLSaxContent::SKIP_THE_CHILDS;
	}

	CString sService = arAttributes.GetAttributeByName(szXmlService);

	if (sService.IsEmpty())
	{
		AddError(_TB("Entity element without 'service' attribute! "));
		return CXMLSaxContent::SKIP_THE_CHILDS;
	}

	// country checking
	CString strTemp = arAttributes.GetAttributeByName(XML_ALLOWISO_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry(strTemp, TRUE))
		return CXMLSaxContent::SKIP_THE_CHILDS;

	strTemp = arAttributes.GetAttributeByName(XML_DENYISO_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry(strTemp, FALSE))
		return CXMLSaxContent::SKIP_THE_CHILDS;

	// activation checking
	strTemp = arAttributes.GetAttributeByName(XML_ACTIVATION_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidActivation(strTemp))
		return CXMLSaxContent::SKIP_THE_CHILDS;

	CTBNamespace aNamespace(CTBNamespace::ENTITY, sNamespace);
	CString sTitle = arAttributes.GetAttributeByName(szXmlLocalize);
	if (sTitle.IsEmpty())
		sTitle = aNamespace.GetObjectName();

	CTBNamespace aServiceNs(CTBNamespace::BEHAVIOUR, sService);
	AfxGetWritableBehavioursRegistry()->RegisterEntity(aNamespace.ToString(), aServiceNs.ToString(), sTitle);
	return CXMLSaxContent::OK;
}

//-----------------------------------------------------------------------------
int	 CBehavioursContent::ParseService(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	CString sNamespace = arAttributes.GetAttributeByName(szXmlNamespace);
	if (sNamespace.IsEmpty())
	{
		AddError(_TB("Service element without 'namespace' attribute! "));
		return CXMLSaxContent::SKIP_THE_CHILDS;
	}

	// country checking
	CString strTemp = arAttributes.GetAttributeByName(XML_ALLOWISO_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry(strTemp, TRUE))
		return CXMLSaxContent::SKIP_THE_CHILDS;

	strTemp = arAttributes.GetAttributeByName(XML_DENYISO_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidCountry(strTemp, FALSE))
		return CXMLSaxContent::SKIP_THE_CHILDS;

	// activation checking
	strTemp = arAttributes.GetAttributeByName(XML_ACTIVATION_ATTRIBUTE);
	if (!strTemp.IsEmpty() && !CXmlAttributeValidator::IsValidActivation(strTemp))
		return CXMLSaxContent::SKIP_THE_CHILDS;

	CString sTitle = arAttributes.GetAttributeByName(szXmlLocalize);

	CTBNamespace aNamespace(CTBNamespace::BEHAVIOUR, sNamespace);
	AfxGetWritableBehavioursRegistry()->RegisterBehaviour(aNamespace.ToString(), sTitle);

	return CXMLSaxContent::OK;
}
