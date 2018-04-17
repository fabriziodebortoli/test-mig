#include "StdAfx.h"

#include <sys/timeb.h>

#include "MacroToRedifine.h"
#include "ThreadContext.h"
#include "InterfaceClasses.h"

///////////////////////////////////////////////////////////////////////////////
class GarbageObj : public CObject
{
public:
	CObject*	m_pObj;
	CTime		m_TimeStamp;

	GarbageObj(CObject* pO)
		: 
		m_pObj (pO)
	{
		m_TimeStamp = CTime::GetCurrentTime();

		ASSERT(pO);
		ASSERT_VALID(pO);
	}

	virtual ~GarbageObj()
	{
		ASSERT_VALID(m_pObj);
		delete m_pObj;
	}

};

//=============================================================================
//			Class CContextObjectEvents
//=============================================================================
class TB_EXPORT CContextObjectEvents : public CDataEventsObj
{
public:
	CString			m_strName;
	CContextBag*	m_pContextBag;

public:
	CContextObjectEvents(const CString strName, CContextBag* pContextBag) 
	:
	m_strName		(strName),
	m_pContextBag	(pContextBag)
	{
		m_bOwned = true;
	}

	virtual CObserverContext* GetContext() const { return NULL; }
	virtual void Fire	(CObservable*, EventType ) {  }
	virtual void Signal	(CObservable* pSender, EventType eType) 
	{
		if (eType == ON_CHANGED)
			m_pContextBag->ContextObjectChanged(m_strName);
	}
};


//=============================================================================
//	class CContextObject 
//=============================================================================
//--------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CContextObject, CObject)

//=============================================================================
//	class CContextBag 
//=============================================================================

//--------------------------------------------------------------------------
CContextBag::CContextBag()
{
}

//utilizzato nel passaggio del ContextBag nella RunDocument
//--------------------------------------------------------------------------
CContextBag::CContextBag(const CContextBag& aContextBag)
{
	CContextObject* pObject = NULL;
	CContextObject* pNewObject = NULL;
	CString strObjectName;
	for (int i =0; i <= aContextBag.m_arContextObjects.GetUpperBound(); i++)
	{
		pObject = aContextBag.m_arContextObjects.GetAt(i);	
		if (pObject)
		{
			pNewObject = pObject->CloneContextObject();
			strObjectName = pObject->GetObjectName();
			CContextObjectEvents* pEvents = new CContextObjectEvents(strObjectName, this);
			pNewObject->AttachEvents(pEvents);
			CString strKey(strObjectName); 
			strKey.MakeLower();
			pNewObject->m_strObjectName = strObjectName;
			m_arContextObjects.Add(pNewObject);
		}
	}	
}

//--------------------------------------------------------------------------
CContextBag::~CContextBag()
{
	// cleanup of the DataEvents array
	CObject* pO;
	for (int i = 0; i < m_arContextObjectEvents.GetSize(); i++) 
	{
		if (pO = m_arContextObjectEvents.GetAt(i)) 
			delete pO;
	}
	m_arContextObjectEvents.RemoveAll();

	// cleanup of the stored Context Objects
	CContextObject* pObject = NULL;
	for (int i =0; i <= m_arContextObjects.GetUpperBound(); i++)
	{
		pObject =  m_arContextObjects.GetAt(i);	
		delete pObject;		
	}
	m_arContextObjects.RemoveAll();
}

//--------------------------------------------------------------------------
CContextObject* CContextBag::LookupContextObject(const CString& strName) const
{
	CString strKey(strName); 
	strKey.MakeLower();
	CContextObject* pObject;
	for (int i =0; i <= m_arContextObjects.GetUpperBound(); i++)
	{
		pObject = m_arContextObjects.GetAt(i);
		if (pObject->m_strObjectName.CompareNoCase(strName) == 0)
			return pObject;
	}

	return NULL;
}

//--------------------------------------------------------------------------
CContextObject* CContextBag::AddContextObject(const CString& strName, CRuntimeClass* pClass, BOOL bAssertWhenExists)
{
	//lo aggiungo solo se non è già presente
	CContextObject* pExistObject = LookupContextObject(strName);
	if (pExistObject)
	{
		if (bAssertWhenExists)
		{
			ASSERT_TRACE1(FALSE, "Context Object %s is already defined", strName);
		}
		return pExistObject;
	}

	CObject* pObj = pClass->CreateObject();
	CContextObject* pNewObject = DYNAMIC_DOWNCAST(CContextObject, pObj);
	if (!pNewObject)
	{
		ASSERT_TRACE1(FALSE,"Class %s is not derived from CContextObject", CString(pClass->m_lpszClassName));
		delete pObj;
		return NULL;
	}

	CContextObjectEvents* pEvents = new CContextObjectEvents(strName, this);
	pNewObject->AttachEvents(pEvents);

	CString strKey(strName); 
	strKey.MakeLower();
	pNewObject->m_strObjectName = strName;
	m_arContextObjects.Add(pNewObject);
	return pNewObject;
}

//--------------------------------------------------------------------------
void CContextBag::RemoveContextObject(const CString& strName)
{
	CString strKey(strName); 
	strKey.MakeLower();
	CContextObject* pObject;
	for (int i =0; i <= m_arContextObjects.GetUpperBound(); i++)
	{
		pObject = m_arContextObjects.GetAt(i);
		if (pObject->m_strObjectName.CompareNoCase(strName) == 0)
		{	
			delete pObject;	
			m_arContextObjects.RemoveAt(i);
			for (int i = 0; i < m_arContextObjectEvents.GetSize(); i++)
			{
				CContextObjectEvents* pEvents = (CContextObjectEvents*)m_arContextObjectEvents.GetAt(i);
				if (pEvents->m_strName == strName)
				{
					delete pEvents;
					m_arContextObjectEvents.RemoveAt(i);
					break;
				}
			}
			break;
		}
	}
}

//--------------------------------------------------------------------------
void CContextBag::ContextObjectChanged(const CString& strName)
{
	CContextObject* pObject = NULL;
	for (int i =0; i <= m_arContextObjects.GetUpperBound(); i++)
	{
		pObject = m_arContextObjects.GetAt(i);
		// avoid notify changes to the object who signaled them
		if (pObject->m_strObjectName.CompareNoCase(strName) != 0)
			pObject->OnContextObjectChanged(strName);
	}

	//@@TODO gestire ricorsività indotta !!!
}

//=============================================================================
//	class CThreadContext 
//=============================================================================
//
//-----------------------------------------------------------------------------
CThreadContext::CThreadContext(void)
:
	m_nOperationsDay(0),
	m_nOperationsMonth(0),
	m_nOperationsYear(0),
	m_pLoginContext(NULL),
	m_bSendDocumentEventsToMenu(true),
	m_bCollateCultureSensitive(false), 
	m_pMenuWnd(NULL),
	m_InteractionMode(UNDEFINED),
	m_CallBreak(FALSE, TRUE),
	m_bClosing(false),
	m_bInErrorState(false),
	m_pActiveDockableWnd(NULL),
	m_pMainDocument(NULL),
	m_bAllowDockableFrame(true),
	m_pDocSession(NULL),
	m_pSqlConnectionPoolObj(NULL)
{
}

//-----------------------------------------------------------------------------
CThreadContext::~CThreadContext(void)
{
	ClearObjects();
	
	delete m_pSqlConnectionPoolObj;

	if (m_pMenuWnd)
	{
		m_pMenuWnd->Detach();
		delete m_pMenuWnd;
	}
	delete m_pDocSession;

#ifdef DEBUG
	POSITION pos = m_WriteLockMap.GetStartPosition();
	while (pos)
	{
		const CTBLockable* pObj;
		int nCount = 0;
		m_WriteLockMap.GetNextAssoc(pos, pObj, nCount);
		ASSERT(nCount == 0);

	}
	pos = m_ReadLockMap.GetStartPosition();
	while (pos)
	{
		const CTBLockable* pObj;
		int nCount = 0;
		m_ReadLockMap.GetNextAssoc(pos, pObj, nCount);
		ASSERT(nCount == 0);

	}
#endif
}
//--------------------------------------------------------------------------------
void CThreadContext::PerformExitinstanceOperations()
{
	ClearVoids();
	ClearGarbageObjects();

	m_CallBreak.SetEvent();
}

//--------------------------------------------------------------------------------
void CThreadContext::AddWebServiceStateObject(CObject *pObject) 
{
	m_arWebServiceStateObjects.Add(pObject); 
}
	
//--------------------------------------------------------------------------------
void CThreadContext::RemoveWebServiceStateObject(CObject *pObject)
{
	for (int i = 0; i < m_arWebServiceStateObjects.GetCount(); i++)
	{
		CObject *p =  m_arWebServiceStateObjects[i];
		if (p == pObject)
			m_arWebServiceStateObjects.RemoveAt(i);
	}
}

//--------------------------------------------------------------------------------
void CThreadContext::AddDocument(CDocument* pDocument)
{
	m_Documents.Add(pDocument);
	if (m_pMainDocument == NULL)
	{
		m_pMainDocument = pDocument;

		if (m_pMenuWnd)
		{
			CActiveMenuFrame* pMenuFrame = dynamic_cast<CActiveMenuFrame*>(m_pMenuWnd);
			if (pMenuFrame)
				pMenuFrame->SetDocument(m_pMainDocument);
		}
	}
}
	
//--------------------------------------------------------------------------------
void CThreadContext::RemoveDocument(CDocument* pDocument)
{
	for ( int i = 0; i < m_Documents.GetCount(); i++)
	{
		if (m_Documents[i] == pDocument)
		{
			m_Documents.RemoveAt(i);
			break;
		}
	}

	//se sto cancellando il main document, il nuovo
	//main document diventa il primodella slista (se c'è)
	if (pDocument == m_pMainDocument)
	{
		m_pMainDocument = (m_Documents.GetCount()  == 0)
			? NULL
			: m_Documents[0];
	}
	//se non ho più documenti e neanche finestre aperte sul thread, posso chiudere il thread
	if (!m_pMainDocument && m_arThreadWindows.GetCount() == 0)
	{
		CTBWinThread *pThread = AfxGetTBThread();
		if (pThread && pThread->IsDocumentThread())
			pThread->PostThreadMessage(WM_QUIT, 0, NULL);
	}

}

//----------------------------------------------------------------------------
void CThreadContext::ClearObjects()
{
	POSITION pos = m_arObjects.GetStartPosition();
	CThreadLocalStorage* pObject = NULL;
	LPCSTR szKey;
	while (pos)
	{
		m_arObjects.GetNextAssoc(pos, szKey, pObject);
		delete pObject;		
		m_arObjects[szKey] = NULL;
	}

	m_arObjects.RemoveAll();
}

//-----------------------------------------------------------------------------
void CThreadContext::ResetCallBreakEvent()
{
	if (!this) return;
	
	//if the event already exists, a previous call has set the document busy using this method 
	//but the call to the action method has not been done (so no one has released the document thread)
	m_CallBreak.PulseEvent();
}


//----------------------------------------------------------------------------
CWnd* CThreadContext::GetLatestActiveWnd(CWnd* pWnd)
{
	if (!pWnd)
	{
		return m_arActiveWndStack.GetCount() 
			? m_arActiveWndStack[m_arActiveWndStack.GetUpperBound()] 
			: NULL;
	}
	for (int i = m_arActiveWndStack.GetUpperBound(); i >=0; i--)
	{
		if (m_arActiveWndStack[i] == pWnd)
			return i == 0 ? NULL : m_arActiveWndStack[i-1];
	}
	return NULL;
}
//----------------------------------------------------------------------------
void CThreadContext::SetLatestActiveWnd(CWnd* pWnd)
{
	if (m_arActiveWndStack.GetCount() > 1)
	{
		RemoveActiveWnd(pWnd);
		AddActiveWnd(pWnd);
	}
}
//----------------------------------------------------------------------------
void CThreadContext::AddActiveWnd(CWnd* pWnd)
{
	m_arActiveWndStack.Add(pWnd);
}

//----------------------------------------------------------------------------
void CThreadContext::RemoveActiveWnd(CWnd* pWnd)
{
	for (int i =0; i < m_arActiveWndStack.GetCount(); i++)
		if (m_arActiveWndStack[i] == pWnd)
			m_arActiveWndStack.RemoveAt(i);
}
	
//----------------------------------------------------------------------------
void CThreadContext::CollectObject(CObject* p)
{ 
	m_arGarbageObjectBag.InsertAt(0, new GarbageObj(p));
}

//----------------------------------------------------------------------------
void CThreadContext::ClearVoids()
{
	for (int i = 0; i < m_arGarbageVoidBag.GetSize(); i++)
	{
		try
		{
			void* ptr = m_arGarbageVoidBag.GetAt(i);
			::delete(ptr);
		}
		catch(...)
		{
			TRACE(_T("Bud woorm info on delete\n"));
			ASSERT(FALSE);
		}
	}
	m_arGarbageVoidBag.RemoveAll();
}

//----------------------------------------------------------------------------
void CThreadContext::ClearOldObjects()
{
	CTime CurrTimeStamp = CTime::GetCurrentTime();

	for (int i = m_arGarbageObjectBag.GetUpperBound(); i >= 0; i--)
	{
		GarbageObj* pO = (GarbageObj*) m_arGarbageObjectBag.GetAt(i);
		ASSERT_VALID(pO);

		CTimeSpan elapsedTime = CurrTimeStamp - pO->m_TimeStamp;
		BOOL bIsOld = elapsedTime.GetTotalSeconds() > 60;	//1 minuto

		if (!bIsOld)
			break;	//i nuovi vengono aggiunti/inseriti all'indice 0 //continue;

		try
		{
			delete pO;
			m_arGarbageObjectBag.RemoveAt(i);
		}
		catch(...)
		{
			TRACE(_T("Bud garbage object on delete\n"));
			ASSERT(FALSE);
		}
	}
}

//----------------------------------------------------------------------------
void CThreadContext::ClearGarbageObjects()
{
	for (int i = 0; i < m_arGarbageObjectBag.GetSize(); i++)
	{
		GarbageObj* pO = (GarbageObj*) m_arGarbageObjectBag.GetAt(i);
		ASSERT_VALID(pO);

		try
		{
			delete pO;
		}
		catch(...)
		{
			TRACE(_T("Bud garbage object on delete\n"));
			ASSERT(FALSE);
		}
	}

	m_arGarbageObjectBag.RemoveAll();
}
//-----------------------------------------------------------------------------
void CThreadContext::RaiseCallBreakEvent()
{
	if (!this) return;

	m_CallBreak.SetEvent();

	//Before entering in modal state we need to notify client of a new component creation
	if (m_pDocSession)
	{
		m_pDocSession->ResumePushToClient();
	}
}

//-----------------------------------------------------------------------------
HANDLE CThreadContext::GetCallBreakEventHandle()
{
	if (!this) return NULL;

	return m_CallBreak.m_hObject;
}

//----------------------------------------------------------------------------
void CThreadContext::AddObject(void* pObject) 
{
	m_arValidObjects.Add(pObject); 
}
//----------------------------------------------------------------------------
void CThreadContext::RemoveObject(void* pObject) 
{
	for (int i = 0; i < m_arValidObjects.GetCount(); i++)
	{
		void* p = m_arValidObjects[i];
		if (p == pObject)
		{
			m_arValidObjects.RemoveAt(i);
			return;
		}
	}
}
//----------------------------------------------------------------------------
bool CThreadContext::IsValidObject(void* pObject) 
{
	for (int i = 0; i < m_arValidObjects.GetCount(); i++)
	{
		void* p = m_arValidObjects[i];
		if (p == pObject)
			return true;
	}

	return false;
}

//----------------------------------------------------------------------------
CWnd* CThreadContext::GetMenuWindow()
{
	if (m_pMenuWnd) 
		return m_pMenuWnd;

	CLoginContext* pContext = AfxGetLoginContext();
	HWND hwnd = pContext ? pContext->GetMenuWindowHandle() : NULL;
	if (!hwnd || !::IsWindow(hwnd))
		return NULL;

	m_pMenuWnd = new CActiveMenuFrame();
	m_pMenuWnd->Attach(hwnd);
	return m_pMenuWnd;
}
//----------------------------------------------------------------------------
void CThreadContext::SetLoginContext(const CString& strName)
{
	m_strLoginContextName = strName;
	m_pLoginContext = AfxGetLoginContext(strName);
}

//----------------------------------------------------------------------------
void CThreadContext::SetOperationsDate(UWORD nDay, UWORD nMonth, UWORD nYear)	
{
	m_nOperationsDay = nDay;
	m_nOperationsMonth = nMonth; 
	m_nOperationsYear = nYear; 
}

//----------------------------------------------------------------------------
CDiagnostic* CThreadContext::GetDiagnostic()
{
	return &m_Diagnostic;
}

//-----------------------------------------------------------------------------
void CThreadContext::AddWindowRef(HWND hwnd, BOOL bModal)
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext && !bModal)
		pContext->AddToAllHwndArray(hwnd);

	m_arThreadWindows.Add(hwnd);
	if (bModal)
		m_arThreadModalWindows.Add(hwnd);

	if (m_pDocSession)
		m_pDocSession->OnAddThreadWindow(hwnd);
}

//-----------------------------------------------------------------------------
BOOL CThreadContext::RemoveWindowRef(HWND hwnd, BOOL bModal)
{
	BOOL bRemoved = FALSE;
	for (int i = 0; i < m_arThreadWindows.GetCount(); i++)
	{
		if (m_arThreadWindows[i] == hwnd)
		{
			m_arThreadWindows.RemoveAt(i);
			bRemoved = TRUE;
			break;
		}
	}

	if (bModal)
	{
		for (int i = 0; i < m_arThreadModalWindows.GetCount(); i++)
		{
			if (m_arThreadModalWindows[i] == hwnd)
			{
				m_arThreadModalWindows.RemoveAt(i);
				bRemoved = TRUE;
				break;
			}
		}
	}

	if (m_arThreadWindows.GetCount() == 0)
	{
	
		CTBWinThread *pThread = AfxGetTBThread();
		if (pThread && !bModal && pThread->IsDocumentThread())
			pThread->PostThreadMessage(WM_QUIT, 0, NULL);
	}
	if (bRemoved)
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (m_pDocSession)
			m_pDocSession->OnRemoveThreadWindow(hwnd);

		if (pContext && !bModal)
			pContext->RemoveFromAllHwndArray(hwnd);

	}
	return bRemoved;
}

//-----------------------------------------------------------------------------
HWNDArray& CThreadContext::GetThreadWindows()
{
	return m_arThreadWindows; 
}

//-----------------------------------------------------------------------------
BOOL CThreadContext::OnIdle(LONG lCount)
{
	BOOL bContinueIdle = FALSE;
	for (int i = 0; i < m_OnIdleHandlers.GetCount(); i++)
	{
		//se almeno un handler ha bisogno di continuare a lavorare, riturno TRUE
		if (m_OnIdleHandlers[i](lCount))
			bContinueIdle = TRUE;
	}
	
	return bContinueIdle;
}
//-----------------------------------------------------------------------------
UINT CThreadContext::GetInnerLoopDepth()
{
	return m_nInnerLoopDepth.GetCount(); 
}

//-----------------------------------------------------------------------------
InnerLoopReason CThreadContext::GetInnerLoopMaxReason() 
{
	InnerLoopReason reason = NONE;
	for (int i = 0; i < m_nInnerLoopDepth.GetCount(); i++)
		reason = max(reason, m_nInnerLoopDepth[i]->GetLoopReason());
	return reason; 
}

//-----------------------------------------------------------------------------
CPushMessageLoopDepthMng* CThreadContext::GetInnerLoopOwner()
{
	int n = m_nInnerLoopDepth.GetCount();
	if (n == 0)
		return NULL;

	return m_nInnerLoopDepth[n-1];
}
//-----------------------------------------------------------------------------
BOOL CThreadContext::IsThreadInModalState()
{
	return m_arThreadModalWindows.GetCount();
}
//-----------------------------------------------------------------------------
HWND CThreadContext::GetCurrentModalWindow()
{
	if (m_arThreadModalWindows.GetCount() == 0)
		return NULL;
	return m_arThreadModalWindows[m_arThreadModalWindows.GetUpperBound()];
}


//-----------------------------------------------------------------------------
void CThreadContext::GarbageUnusedSqlSession()
{
	if (m_pSqlConnectionPoolObj)
		m_pSqlConnectionPoolObj->GarbageUnusedSqlSession();

}


//Imposta l'istante in cui si e' avuta l'ultima interazione con l'utente
//Time in seconds since midnight (00:00:00), January 1, 1970, coordinated universal time (UTC).
//ftime64_s, which uses the __timeb64 structure, allows file-creation dates to be expressed up through 23:59:59, December 31, 3000, UTC;
//In Visual C++ 2008, _ftime_s is equivalent to _ftime64_s and _timeb contains a 64-bit time. This is true unless _USE_32BIT_TIME_T is defined, in which case the old behavior is in effect; _ftime_s uses a 32-bit time and _timeb contains a 32-bit time. 

//--------------------------------------------------------------------------------
void CThreadContext::SetThreadInteractionTime()
{
   struct _timeb timebuffer;
   if (_ftime_s( &timebuffer ) == 0) //returns Zero if successful
		m_nLastInteractionTime = timebuffer.time;
}

//--------------------------------------------------------------------------------
void CThreadContext::SetThreadGetDescriptionTime()
{
   struct _timeb timebuffer;
   if (_ftime_s( &timebuffer ) == 0) //returns Zero if successful
		m_nLastGetDescriptionTime = timebuffer.time;
}

//-----------------------------------------------------------------------------
THREAD_LOCAL(CThreadContext, _threadContext)

//-----------------------------------------------------------------------------
CThreadContext* AfxGetThreadContext()
{
	return _threadContext.GetData();
}

//-----------------------------------------------------------------------------
const CString& AfxGetAuthenticationToken()
{
	return AfxGetThreadContext()->GetLoginContextName(); 
}

//-----------------------------------------------------------------------------
void AfxAttachThreadToLoginContext(const CString& strContextName)
{
	AfxGetThreadContext()->SetLoginContext(strContextName);
}

//-----------------------------------------------------------------------------
const int AfxGetWorkerId()
{

	CLoginContext* pTempContext = AfxGetThreadContext()->GetLoginContext();

	CLoginContext* pContext = AfxGetLoginContext();
	return pContext ? pContext->GetWorkerId() : 0;
}

//-----------------------------------------------------------------------------
const int AfxGetWorkerId(CString token)
{
	CLoginContext* pContext = AfxGetLoginContext(token);
	return pContext ? pContext->GetWorkerId() : 0;
}

//-----------------------------------------------------------------------------
void AfxSetWorkerId(int nWorkerId)
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
		pContext->SetWorkerId(nWorkerId);
}
//-----------------------------------------------------------------------------
void AfxSetWorkersTable(CWorkersTableObj* pWorkersTable)
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
		pContext->AttachWorkersTable((CObject*)pWorkersTable);
}

//-----------------------------------------------------------------------------
CWorkersTableObj* AfxGetWorkersTable()
{
	CLoginContext* pContext = AfxGetLoginContext();
	return (pContext) ?	(CWorkersTableObj*)pContext->GetWorkersTable() : NULL;	
}

//-----------------------------------------------------------------------------
void AfxSetValid(bool bValid)
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
		pContext->SetValid(bValid);
}

//-----------------------------------------------------------------------------
MapRTCToObject* GetProxyObjectMap()
{
	return AfxGetThreadContext()->GetProxyObjectMap();
}

//------------------------------------------------------------------------------
BOOL AFXAPI AfxIsInUnattendedMode() 
{ 
	CThreadContext* pThread = AfxGetThreadContext();
	if (pThread->GetUserInteractionMode() == UNDEFINED)
		return AfxGetApplicationContext()->IsInUnattendedMode();  
	
	return pThread->GetUserInteractionMode() == UNATTENDED;
}


//------------------------------------------------------------------------------
CContextBag* AfxGetThreadContextBag()
{
	CThreadContext* pContext = AfxGetThreadContext();
	return (pContext) ? pContext->GetContextBag() : NULL;
}

//------------------------------------------------------------------------------------
void AddThreadWindowRef(HWND hwndWindow, bool modal)
{
	if (modal)
		AfxSetThreadInModalState(true, hwndWindow);
	else
	{
		AfxGetThreadContext()->AddWindowRef(hwndWindow, modal);
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
			pContext->IncreaseOpenDialogs(); 
	}
	
}

//------------------------------------------------------------------------------------
void RemoveThreadWindowRef(HWND hwndWindow, bool modal)
{
	if (modal)
		AfxSetThreadInModalState(false, hwndWindow);
	else
	{
		if (AfxGetThreadContext()->RemoveWindowRef(hwndWindow, modal))
		{
			CLoginContext* pContext = AfxGetLoginContext();
			if (pContext)
				pContext->DecreaseOpenDialogs(); 
		}
	}
}


//==================================================================================
IMPLEMENT_DYNAMIC(CTBLockedFrame, CBCGPFrameWnd)

//------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTBLockedFrame, CBCGPFrameWnd)
	ON_WM_CREATE()
	ON_WM_DESTROY()
END_MESSAGE_MAP()

//------------------------------------------------------------------------------------
CTBLockedFrame::CTBLockedFrame()
{
}

//------------------------------------------------------------------------------------
int	 CTBLockedFrame::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	TB_OBJECT_LOCK(&AfxGetApplicationContext()->m_FrameListCodeLocker);
	return __super::OnCreate(lpCreateStruct);
}

//------------------------------------------------------------------------------------
void CTBLockedFrame::OnDestroy()
{
	TB_OBJECT_LOCK(&AfxGetApplicationContext()->m_FrameListCodeLocker);
	__super::OnDestroy();
}


IMPLEMENT_DYNAMIC(CActiveMenuFrame, CTBLockedFrame)
//------------------------------------------------------------------------------------
CActiveMenuFrame::CActiveMenuFrame()
{
}

//------------------------------------------------------------------------------------
void CActiveMenuFrame::SetDocument(CDocument* pMainDocument)
{
	m_pMainDocument = pMainDocument;
}

//-----------------------------------------------------------------------------
BOOL CActiveMenuFrame::OnDrawMenuImage (CDC* pDC, const CBCGPToolbarMenuButton* pMenuButton, const CRect& rectImage)
{
	ASSERT(m_pMainDocument);
	ASSERT(pMenuButton);
	if (!m_pMainDocument) return FALSE;

	if (pMenuButton->GetParentWnd() && pMenuButton->GetParentWnd()->GetParent())
	{
		CWnd* pParent = pMenuButton->GetParentWnd()->GetParent();
		CBCGPPopupMenu* pMenu = DYNAMIC_DOWNCAST (CBCGPPopupMenu, pParent);
		if (pMenu)
		{
			//Return a pointer to the window where the framework routes the pop-up menu messages
			CWnd* pMsg = pMenu->GetMessageWnd();

			// is a Frame class ?
			if (
				pMsg->IsKindOf(RUNTIME_CLASS(CBCGPFrameWnd)) && 
				pMsg != this //e' capitato che la GetMessageWnd tornasse la CActiveMenuFrame....in questo caso evitiamo la stack overflow exception
				)
			{
				CBCGPFrameWnd* pFrame = DYNAMIC_DOWNCAST (CBCGPFrameWnd, pMsg);
				return pFrame->OnDrawMenuImage(pDC, pMenuButton, rectImage);
			}
			else if (pMsg->IsKindOf(RUNTIME_CLASS(CBCGPMenuButton)))
			{
				IMenuIcon* pMenuBtn = dynamic_cast<IMenuIcon*>(pMsg);
				if (pMenuBtn)
					return pMenuBtn->OnDrawMenuImage(pDC, pMenuButton, rectImage);
			}
		}
	}	

	return __super::OnDrawMenuImage (pDC, pMenuButton, rectImage);
}
