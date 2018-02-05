#pragma once

#include "LoginContext.h"
#include "Diagnostic.h"
#include "Observable.h"
#include <BCGCBPro\BCGPFrameWnd.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CWorkersTableObj;



//=============================================================================================
class TB_EXPORT CThreadLocalStorage
{
public:
	virtual ~CThreadLocalStorage() {}
};

//=============================================================================================
template <class T>
class CThreadLocalStorageT : public CThreadLocalStorage
{
public:
	T		m_Variable;
};

//=============================================================================================
template <class T>
class CThreadLocalStorageInitT : public CThreadLocalStorageT<T>
{
	typedef void (*FunctionPointer)(T);
	
	FunctionPointer m_pfnDispose;
public:
	CThreadLocalStorageInitT()
		: m_pfnDispose(NULL)
	{
	}

	~CThreadLocalStorageInitT()
	{
		if (m_pfnDispose)
			m_pfnDispose(m_Variable);
	}

	void Init(const T& initValue, FunctionPointer pfnDispose = NULL)
	{
		m_Variable = initValue;
		m_pfnDispose = pfnDispose;
	}
};

//Gestione ContextBag per progetto #5065: Studio, commercialisti, paghe....
//=============================================================================
class TB_EXPORT CContextObject : public CObject, public CObservable
{
	friend class CContextBag;

	DECLARE_DYNAMIC(CContextObject)

private:
	CString m_strObjectName;

protected:
	virtual void OnContextObjectChanged(const CString&) { } // default do nothing

public:	
	virtual void OnStartTransaction()  {};
	virtual void OnCommitTransaction() {};
	virtual void OnRollbackTransaction()  {};
	virtual LPCTSTR GetObjectName() { return m_strObjectName; }
	virtual void SetObjectName(const CString& strName) { m_strObjectName = strName; }

	virtual CContextObject* CloneContextObject() { ASSERT(FALSE); return NULL;}; //effettua il clone dell'istanza di CContextObject. E' necessario reimplementarlo nella classe derivata poichè solo lei sa come è fatta
};

// Objects added to the context bag are owned by it and they  will be automatically disposed
// when the bag itself will be disposed, at the end of the thread 
//=============================================================================
class TB_EXPORT CContextBag
{	
	friend class CContextObjectEvents;

private:
	CArray<CContextObject*,	CContextObject*>	m_arContextObjects;

public:	
	CContextBag();
	CContextBag(const CContextBag&);
	~CContextBag();

public:
	CContextObject*	AddContextObject		(const CString& strName, CRuntimeClass* pClass, BOOL bAssertWhenExists = TRUE);
	CContextObject*	LookupContextObject	(const CString& strName) const;
	void			RemoveContextObject	(const CString& strName);

protected:
	void ContextObjectChanged(const CString& strName);

private:
	CObArray	m_arContextObjectEvents;
};


#ifdef _DEBUG
typedef CMap<const CTBLockable*, const CTBLockable*, int, int> LockMap;
#endif
typedef CArray<HWND, HWND> HWNDArray;

enum InnerLoopReason { NONE = 0, THREAD_LOOP = 1, MODAL_STATE = 2, BATCH = 3, WOORM_REPORT = 4 };
class CPushMessageLoopDepthMng;

typedef CArray<CPushMessageLoopDepthMng*, CPushMessageLoopDepthMng*> InnerLoopArray;
typedef BOOL (*OnIdleHandler)(LONG lCount);
//=============================================================================================class TB_EXPORT CThreadContext: public CNoTrackObject
class TB_EXPORT CThreadContext: public CNoTrackObject, public CGenericContext<CThreadContext>
{
public:
	typedef CArray<void*> GarbageArray;
	typedef CArray<CDocument*> DocumentArray;
private:
	CDocument*			m_pMainDocument;
	DocumentArray		m_Documents;
	CObArray			m_arWebServiceStateObjects;
	CString				m_strLoginContextName;
	CString				m_strSessionLoginName;
	CString				m_strUICulture;
	HWNDArray			m_arThreadWindows;
	HWNDArray			m_arThreadModalWindows;
	InnerLoopArray		m_nInnerLoopDepth;

	UWORD				m_nOperationsDay;
	UWORD				m_nOperationsMonth;
	UWORD				m_nOperationsYear;
	CDiagnostic			m_Diagnostic;
	CMap<LPCSTR, LPCSTR, CThreadLocalStorage*, CThreadLocalStorage*> m_arObjects;
	CLoginContext*		m_pLoginContext;
	MapRTCToObject		m_ProxyObjectMap;
	CArray<void*, void*>m_arValidObjects;
	CFrameWnd*			m_pMenuWnd;

	UserInteractionMode m_InteractionMode;
	::CEvent			m_CallBreak;
	GarbageArray		m_arGarbageVoidBag;
	CObArray			m_arGarbageObjectBag;
	bool				m_bClosing;
	CWnd*				m_pActiveDockableWnd;
	CArray<CWnd*>		m_arActiveWndStack;
	bool				m_bInErrorState;
	CArray<OnIdleHandler>m_OnIdleHandlers;
	time_t				m_nLastInteractionTime;  //last interaction Time in seconds since midnight (00:00:00), January 1, 1970, coordinated universal time (UTC).
	time_t				m_nLastGetDescriptionTime; //last request for get window decription from remote interface. Time in seconds since midnight (00:00:00), January 1, 1970, coordinated universal time (UTC).
	bool				m_bAllowDockableFrame;
public:
	CDocumentSessionObj*m_pDocSession;
	IntervalChecker		m_IntervalChecker;

#ifdef _DEBUG
	LockMap				m_WriteLockMap;
	LockMap				m_ReadLockMap;
#endif

	bool				m_bSendDocumentEventsToMenu;
	bool				m_bCollateCultureSensitive;

	CContextBag	m_ContextBag; //impr. 5062



	CThreadContext(void);	
	~CThreadContext(void);

	CString				SetUICulture (const CString& strCulture)	
							{ 
								CString s = m_strUICulture; 
								m_strUICulture = strCulture;
								return s;
							}
	const CString&		GetUICulture()	const { return m_strUICulture; } 
	
	CString				GetSessionLoginName() { return m_strSessionLoginName.IsEmpty() ? AfxGetLoginInfos()->m_strUserName : m_strSessionLoginName; }
	void				SetSessionLoginName(const CString& strLoginName) { m_strSessionLoginName = strLoginName; }

	void				AddWindowRef	(HWND hwnd, BOOL bModal);
	BOOL				RemoveWindowRef	(HWND hwnd, BOOL bModal);
	HWND				GetCurrentModalWindow();
	BOOL				IsThreadInModalState();

	void				IncreaseInnerLoopDepth	(CPushMessageLoopDepthMng* pOwner) { m_nInnerLoopDepth.Add(pOwner); }
	void				DecreaseInnerLoopDepth	() { m_nInnerLoopDepth.RemoveAt(m_nInnerLoopDepth.GetUpperBound()); }

	bool				IsDockableFrameAllowed()			{ return m_bAllowDockableFrame; }
	void				AllowDockableFrame(bool bAllow)		{ m_bAllowDockableFrame = bAllow; }

	HWNDArray&			GetThreadWindows();
	UINT				GetInnerLoopDepth();
	InnerLoopReason		GetInnerLoopMaxReason();
	CPushMessageLoopDepthMng* GetInnerLoopOwner();
	void				PerformExitinstanceOperations();
	MapRTCToObject*		GetProxyObjectMap() { return &m_ProxyObjectMap; }
	void				ClearProxyObjectMap() { m_ProxyObjectMap.RemoveAll(); }
	template<class T>
	CThreadLocalStorageInitT<T>* GetLocalObject (LPCSTR szKey, BOOL& bCreated)
	{
		bCreated = FALSE;
		CThreadLocalStorageInitT<T>* p = (CThreadLocalStorageInitT<T>*) m_arObjects[szKey];
		if (!p)
		{
			bCreated = TRUE;
			p = new CThreadLocalStorageInitT<T>();
			m_arObjects[szKey] = p;
		}
		return p;
	}

	
	template<class T>
	CThreadLocalStorageT<T>* GetLocalObject (LPCSTR szKey)
	{
		CThreadLocalStorageT<T>* p = (CThreadLocalStorageT<T>*) m_arObjects[szKey];
		if (!p)
		{
			p = new CThreadLocalStorageT<T>();
			m_arObjects[szKey] = p;
		}
		return p;
	}

	void ResetCallBreakEvent();
	void RaiseCallBreakEvent();
	HANDLE GetCallBreakEventHandle();
	void SetUserInteractionMode (UserInteractionMode interactionMode)	{ m_InteractionMode = interactionMode; }
	UserInteractionMode GetUserInteractionMode ()						{ return m_InteractionMode; }
	virtual void	CollectVoid		(void* p)		{ m_arGarbageVoidBag.Add(p); }
	virtual void	CollectObject	(CObject* p);

	void ClearVoids();
	void ClearGarbageObjects();
	void ClearOldObjects();
	bool IsClosing() { return m_bClosing; }
	void SetClosing(bool bSet = true) { m_bClosing = bSet; }
	bool IsInErrorState() { return m_bInErrorState; }
	void SetInErrorState(bool bSet = true) { m_bInErrorState = bSet; }
	void AddActiveWnd(CWnd* pWnd);
	void RemoveActiveWnd(CWnd* pWnd);
	CWnd* GetLatestActiveWnd(CWnd* pWnd);
	void SetLatestActiveWnd(CWnd* pWnd);
	CWnd* GetActiveDockableWnd() { return m_pActiveDockableWnd; }
	void SetActiveDockableWnd(CWnd* pWnd) { m_pActiveDockableWnd = pWnd; }
	const DocumentArray& GetDocuments() { return m_Documents; }
	void RemoveDocument(CDocument* /*pDocument*/);
	void AddDocument(CDocument* /*pDocument*/);
	CDocument* GetMainDocument() { return m_pMainDocument; }
	void AddOnIdleHandler(OnIdleHandler handler) { m_OnIdleHandlers.Add(handler); }
	BOOL OnIdle(LONG lCount);
	void AddWebServiceStateObject(CObject* /*pObject*/);
	void RemoveWebServiceStateObject(CObject* /*pObject*/);
	const CObArray& GetWebServiceStateObjects() { return m_arWebServiceStateObjects; }
	UWORD				GetOperationsDay()	{ return m_nOperationsDay; }
	UWORD				GetOperationsMonth(){ return m_nOperationsMonth; }
	UWORD				GetOperationsYear()	{ return m_nOperationsYear; }
	void				SetOperationsDate(UWORD nDay, UWORD nMonth, UWORD nYear);

	inline bool			IsCollateCultureSensitive()					{ return m_bCollateCultureSensitive; }
	void SetLoginContext(const CString& strName);
	inline void			SetCollateCultureSensitive()				{ m_bCollateCultureSensitive = true; }
	void				SetThreadInteractionTime();
	void				SetThreadGetDescriptionTime();

	const CString&	GetLoginContextName()			{ return m_strLoginContextName; }
	inline CLoginContext*	GetLoginContext()		{ return m_pLoginContext; }
	CDiagnostic*	GetDiagnostic	();
	void			ClearObjects	();

	CContextBag* GetContextBag() { return &m_ContextBag; };

	void AddObject(void* pObject);
	void RemoveObject(void* pObject);
	bool IsValidObject(void* pObject);
	CWnd* GetMenuWindow();

	time_t GetLastInteractionTime() { return m_nLastInteractionTime;  }
	time_t GetLastGetDescriptionTime() { return  m_nLastGetDescriptionTime; }
private:
	virtual CObject* InternalGetObject(GetMethod getMethod) { return ((this)->*getMethod)(); }
	virtual BOOL IsValid() { return this != NULL; }
};

//==================================================================================
class TB_EXPORT CTBLockedFrame : public CBCGPFrameWnd
{
	DECLARE_DYNAMIC(CTBLockedFrame)

public:
	CTBLockedFrame();

public:
	DECLARE_MESSAGE_MAP()

	afx_msg int	 OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnDestroy();
};

//==================================================================================
class TB_EXPORT CActiveMenuFrame : public CTBLockedFrame
{
	DECLARE_DYNAMIC(CActiveMenuFrame)

private:
	CDocument*	m_pMainDocument;

public:
	CActiveMenuFrame();
	void SetDocument(CDocument*	pMainDocument);
	virtual BOOL OnDrawMenuImage (	CDC* pDC, const CBCGPToolbarMenuButton* pMenuButton, const CRect& rectImage);
};


TB_EXPORT CThreadContext* AfxGetThreadContext();
TB_EXPORT CContextBag* AfxGetThreadContextBag();

inline TB_EXPORT bool AfxIsThreadCollateCultureSensitive() { return AfxGetThreadContext()->IsCollateCultureSensitive(); }

//classe che imposta un mode nel costruttore e ripristina l'originale nel distruttore
//=============================================================================================
class TB_EXPORT SwitchTemporarilyMode
{
	UserInteractionMode m_OldMode;
	CThreadContext* m_pThread;
public:
	SwitchTemporarilyMode(UserInteractionMode mode)
	{
		m_pThread = AfxGetThreadContext();
		m_OldMode = m_pThread->GetUserInteractionMode();
		m_pThread->SetUserInteractionMode(mode);
	}
	~SwitchTemporarilyMode()
	{
		m_pThread->SetUserInteractionMode(m_OldMode);
	}
};
//==================================================================================
class CPushMessageLoopDepthMng
{
	CThreadContext* m_pContext;
	InnerLoopReason m_Reason;
public:
	CPushMessageLoopDepthMng(InnerLoopReason reason, BOOL bActive = TRUE)
		: m_Reason(reason)
	{
		m_pContext = bActive ? AfxGetThreadContext() : NULL;
		if (m_pContext)
			m_pContext->IncreaseInnerLoopDepth(this);
	}

	~CPushMessageLoopDepthMng()
	{
		if (m_pContext)
			m_pContext->DecreaseInnerLoopDepth();
	}

	InnerLoopReason GetLoopReason() { return m_Reason; }
};

#define DECLARE_THREAD_VARIABLE(variableType, variableName) \
	variableType& get_##variableName() \
	{ \
		return AfxGetThreadContext()->GetLocalObject<variableType>(__FILE__ #variableName)->m_Variable; \
	}

#define DECLARE_AND_INIT_THREAD_VARIABLE(variableType, variableName, initValue) \
			DECLARE_INIT_DISPOSE_THREAD_VARIABLE(variableType, variableName, initValue, NULL)

#define DECLARE_INIT_DISPOSE_THREAD_VARIABLE(variableType, variableName, initValue, disposeFunction) \
	variableType& get_##variableName() \
	{\
		BOOL bCreated = FALSE;\
		CThreadLocalStorageInitT<variableType>*	p = AfxGetThreadContext()->GetLocalObject<variableType>(__FILE__ #variableName, bCreated);\
		if (bCreated) p->Init(initValue, disposeFunction);\
		return p->m_Variable; \
	}

#define GET_THREAD_VARIABLE(variableType, variableName)\
	variableType& variableName = get_##variableName();

TB_EXPORT const CString& AfxGetAuthenticationToken();

//imposta l'ID dell'utente correntemente connesso
TB_EXPORT const int AfxGetWorkerId();

TB_EXPORT const int AfxGetWorkerId(CString token);

//recupera l'ID dell'utente correntemente connesso
TB_EXPORT void AfxSetWorkerId(int nWorkerId);
//valorizza la tabella dei workers  associati alla company corrente
TB_EXPORT void AfxSetWorkersTable(CWorkersTableObj* pWorkersTable);
//restituisce la tabella dei workers associati alla company corrente
TB_EXPORT CWorkersTableObj* AfxGetWorkersTable();

// permette di invalidare la login
TB_EXPORT void AfxSetValid(bool bValid);

TB_EXPORT void AfxAttachThreadToLoginContext(const CString& strContextName);
TB_EXPORT BOOL AFXAPI AfxIsInUnattendedMode();

TB_EXPORT void AddThreadWindowRef(HWND hwndWindow, bool modal);
TB_EXPORT void RemoveThreadWindowRef(HWND hwndWindow, bool modal);

//includere alla fine degli include del .H
#include "endh.dex"

