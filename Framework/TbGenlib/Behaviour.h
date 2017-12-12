#pragma once

#include <TbXmlCore\XMLSaxReader.h>
#include "beginh.dex"

// managed behaviour events
enum BehaviourEvents 
{ 
	bhe_FormModeChanged, 
	bhe_LockDocumentForNew, 
	bhe_NewTransaction, 
	bhe_EditTransaction,
	bhe_DeleteTransaction, 
	bhe_BeforeEscape,
	bhe_OnPrepareAuxData,
	bhe_OnInitAuxData,
	bhe_OnLoadRequestInfo,
	bhe_OnValueChanged
};

// classe di parsing XML
//===========================================================================
class TB_EXPORT CBehavioursContent : public CXMLSaxContent
{
	DECLARE_DYNAMIC(CBehavioursContent);

public:
	CBehavioursContent();

public:
	virtual CString OnGetRootTag() const;
	virtual void	OnBindParseFunctions();

private:
	int	 ParseEntity(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
	int	 ParseService(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
};

// class used from registry to store registered services
//=============================================================================
class TB_EXPORT CBehavioursRegistryService
{
	friend class CBehavioursContent;
	friend class CBehavioursRegistry;

	CString			m_sNamespace;
	CString			m_sTitle;
	CRuntimeClass*	m_pClass;

public:
	CBehavioursRegistryService() :  m_pClass(NULL) {}

	const CString&	GetNamespace() const { return m_sNamespace; }
	const CString&	GetTitle	() const { return m_sTitle; }
	CRuntimeClass*	GetClass	() const { return m_pClass; }
};

class IBehaviourContext;
// it a request to a specified service class
//=============================================================================
class TB_EXPORT IBehaviourRequest : public CObject
{
	CObject*					m_pOwner;
	BOOL						m_bEnabled;
	CBehavioursRegistryService*	m_pReceiver;

public:
	IBehaviourRequest(CObject* pOwner, CBehavioursRegistryService* pReceiver);

	const CObject*	GetOwner	() const;
	void			SetOwner	(CObject* pOwner);
	const BOOL		GetEnabled	() const;
	void			SetEnabled	(const BOOL bValue = TRUE);

	CBehavioursRegistryService*	GetReceiver	() const;
	void						SetReceiver	(CBehavioursRegistryService* pReceiver);
	void						SetService	(const CString& sPartialNamespace);

	virtual void OnRequestExecuted	(const BehaviourEvents& evnt, IBehaviourContext* pContext);
};

// it is the specialized service executing code
//=============================================================================
class TB_EXPORT IBehaviourService
{
private:	
	IBehaviourContext*	m_pContext;
	CString				m_sInstanceName;

public:
	IBehaviourService();

public:
	IBehaviourContext*	GetContext();
	void				SetContext(IBehaviourContext* pContext);
	CString				GetInstanceName() const;
	void				SetInstanceName(const CString& sName);

public:
	virtual bool ExecuteRequest		(const BehaviourEvents& evnt, IBehaviourRequest* pRequest);
	virtual bool CanExecuteRequest	(const BehaviourEvents& evnt, IBehaviourRequest* pRequest) = 0;
	virtual void OnInitService		();
	virtual bool FireEvent			(const BehaviourEvents& evnt, IBehaviourRequest* pRequest);
	virtual bool IsCompatibleWith	(IBehaviourRequest* pRequest) = 0;
};

//=============================================================================
// event behaviour map
//=============================================================	================
#define DECLARE_BEHAVIOUR_EVENTMAP() virtual bool ExecuteRequest (const BehaviourEvents& evnt, IBehaviourRequest* pRequest);
#define BEGIN_BEHAVIOUR_EVENTMAP(c) bool c::ExecuteRequest (const BehaviourEvents& evnt, IBehaviourRequest* pRequest) {\
											bool bOk =  false;\
											switch (evnt) {
#define ON_BEHAVIOUR_EVENT(e, m)				case e: bOk = m (pRequest); break;
#define END_BEHAVIOUR_EVENTMAP()			default: break; } \
											if (bOk) { pRequest->OnRequestExecuted (evnt, GetContext()); return true; }\
											return __super::ExecuteRequest (evnt, pRequest); }

// it is the interface to implement in order to be a provider of requests
//=============================================================================
class TB_EXPORT IBehaviourConsumer 
{ 
	friend class IBehaviourContext;

protected:
	CArray<IBehaviourRequest*> m_Requests;

public:
	IBehaviourConsumer();
	~IBehaviourConsumer();

public:
	void AddRequest(IBehaviourRequest*);
	
	CArray<IBehaviourRequest*>&		GetRequests();
};

// it is the interface to implement in order to be a provider of services
//=============================================================================
class TB_EXPORT IBehaviourContext
{
	CRuntimeClass*				m_pClass;
	CArray<IBehaviourService*>	m_Services;
	CArray<IBehaviourConsumer*> m_Consumers;

public:
	IBehaviourContext();
	~IBehaviourContext ();

public:
	CRuntimeClass*	GetContextClass	() const;
	void			SetContextClass (CRuntimeClass* pClass);
	void			GetRequestsBy	(CRuntimeClass* pClass, CArray<IBehaviourRequest*>& output);
	
	IBehaviourService*  GetService		(IBehaviourRequest* pRequest);
	IBehaviourService*  GetService		(const CString& sName);
	virtual BOOL		FireBehaviour	(const BehaviourEvents& evnt, BOOL bUpdateClient = FALSE);

	// this cast is needed to have wright memory when object
	// uses multiple inheritance or not CObject class (e.i. CParsedCtrl)
	virtual IBehaviourContext*	 GetContext	();

	// available from client doc too
	void AddConsumer	(IBehaviourConsumer* pConsumer);
	void RemoveConsumer	(IBehaviourConsumer* pConsumer);

	void				AddBehaviourService		(IBehaviourService* pService);
	void				RemoveBehaviourService	(IBehaviourService* pService);
	IBehaviourService*  CreateBehaviourService	(CBehavioursRegistryService* pRegistryEntry);

	CArray<IBehaviourService*>&		GetServices();
	CArray<IBehaviourConsumer*>&	GetConsumers();

private:
	void UpdateBehavioursClient();
};

// class used by registry to store a registered entity
//=============================================================================
class TB_EXPORT CBehavioursRegistryEntity
{
	friend class CBehavioursRegistry;

	CString			m_sNamespace;
	CString			m_sService;
	CString			m_sTitle;

public:
	CBehavioursRegistryEntity() {}

	const CString&	GetNamespace() const { return m_sNamespace; }
	const CString&	GetTitle	() const { return m_sTitle; }
	const CString&	GetService	() const { return m_sService; }
};

// registry of both services and entities
//=============================================================================
class TB_EXPORT CBehavioursRegistry : public CObject, public CTBLockable
{
	friend class CBehavioursContent;

	CArray<CBehavioursRegistryEntity*>	m_Entities;
	CArray<CBehavioursRegistryService*>	m_Services;

public:
	CBehavioursRegistry();
	~CBehavioursRegistry();

	virtual	LPCSTR	GetObjectName() const	{ return "CBehaviourRegistry"; }

	int		GetBehavioursCount	() const;
	void	RegisterBehaviour	(const CString& sNamespace, const CString& sTitle);
	void	RegisterBehaviour	(const CString& sNamespace, CRuntimeClass* pClass);
	int		GetEntitiesCount	() const;

	CBehavioursRegistryEntity*	GetEntity	 (const CString& sNamespace) const;
	CBehavioursRegistryEntity*	GetEntity	 (const int& i) const;
	CBehavioursRegistryService* GetBehaviour (const int& i) const;
	CBehavioursRegistryService*	GetBehaviour (const CString& sNamespace) const;
	CBehavioursRegistryService*	GetBehaviour (CRuntimeClass* pClass) const;

public:
	void RegisterEntity		(const CString& sNamespace, const CString& sService, const CString& sTitle);
};

// general functions
//=============================================================================
DECLARE_SMART_LOCK_PTR(CBehavioursRegistry)
DECLARE_CONST_SMART_LOCK_PTR(CBehavioursRegistry)

TB_EXPORT CBehavioursRegistryConstPtr	AfxGetBehavioursRegistry();
TB_EXPORT CBehavioursRegistryPtr		AfxGetWritableBehavioursRegistry();

// macro used in interface to register services
//=============================================================================
#define	BEGIN_BEHAVIOURS() CTBNamespace aBehNs;
	#define	DECLARE_BEHAVIOUR_SERVICE(behaviour, service) aBehNs.AutoCompleteNamespace(CTBNamespace::BEHAVIOUR, behaviour, aNamespace); AfxGetWritableBehavioursRegistry()->RegisterBehaviour(aBehNs.ToString(), RUNTIME_CLASS(service));
#define	END_BEHAVIOURS() return TRUE;

#include "endh.dex"
