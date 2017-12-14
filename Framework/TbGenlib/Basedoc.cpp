
#include "stdafx.h"

#include <TbNameSolver\ApplicationContext.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\Templates.h>
#include <TbNameSolver/ThreadContext.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGeneric\ContextFunctions.h>
#include <TbGeneric\TBThemeManager.h>

#include <TbParser\SymTable.h>

#include <TbGenLibManaged\Main.h>

#include <sys/timeb.h>	

//#include "SettingsTableManager.h"

#include "messages.h"
#include "baseapp.h"
#include "AutoExpressionMng.h"
#include "Command.h"
#include "basedoc.h"
#include "basefrm.h"

#include "commands.hrc"

#include "generic.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

////////////////////////////////////////////////////////////////////////////////
//									CDocFieldNames
////////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
//identificatores to add into SymTable
const TCHAR CDocFieldNames::Document[]				= _NID("document");		//document handle
const TCHAR CDocFieldNames::Document_Name[]			= _NID("document.Name"); 
const TCHAR CDocFieldNames::Document_Namespace[]	= _NID("document.Namespace"); 

const TCHAR CDocFieldNames::Document_FormMode[]		= _NID("document.FormMode"); 
	const TCHAR CDocFieldNames::FormMode_New[]		= _NID("const.FormMode.New"); 
	const TCHAR CDocFieldNames::FormMode_Edit[]		= _NID("const.FormMode.Edit"); 
	const TCHAR CDocFieldNames::FormMode_Browse[]	= _NID("const.FormMode.Browse"); 
	const TCHAR CDocFieldNames::FormMode_Find[]		= _NID("const.FormMode.Find"); 

const TCHAR CDocFieldNames::LockStatus_AllLocked[]	= _NID("const.LockStatus.AllLocked"); 
const TCHAR CDocFieldNames::LockStatus_NoAuxData[]	= _NID("const.LockStatus.NoAuxData"); 
const TCHAR CDocFieldNames::LockStatus_LockFailed[]	= _NID("const.LockStatus.LockFailed"); 
 
const TCHAR CDocFieldNames::AddOn[]									= _NID("AddOn"); 
const TCHAR CDocFieldNames::AddOn_MessageRoutingState[]				= _NID("AddOn.MessageRoutingState"); 
	const TCHAR CDocFieldNames::AddOn_MessageRoutingState_Before[]	= _NID("const.MessageRoutingState.Before"); 
	const TCHAR CDocFieldNames::AddOn_MessageRoutingState_After[]	= _NID("const.MessageRoutingState.After"); 

//string
const TCHAR CDocFieldNames::DocumentDot[]			= _NID("document."); 

DECLARE_AND_INIT_THREAD_VARIABLE (const CDocumentDescription* , t_pCurrDocDescri, NULL);

IMPLEMENT_DYNAMIC(CManagedDocComponentObj, CObject)
////////////////////////////////////////////////////////////////////////////////
//									CBaseDocument
////////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CBaseDocument, CDocument)

BEGIN_MESSAGE_MAP(CBaseDocument, CDocument)
	
	ON_COMMAND(UM_CLEAR_MESSAGE_QUEUE, OnClearMessageQueue)
	ON_COMMAND_RANGE(ID_SWITCHTO_START, (UINT)(ID_SWITCHTO_END), OnSwitchTo)

END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CBaseDocument::CBaseDocument()
	:
	m_FormMode				(NONE),
	m_bAborted				(FALSE),
	m_bRetryingLock			(FALSE),
	m_bBatch				(FALSE),
	m_bBatchRunning			(FALSE),
	m_bBatchCloseAfterExecution(FALSE),
	m_bCloseChildReport		(TRUE),
	m_pXMLDataManager		(NULL),
	m_pTemplate				(NULL),
	m_pAutoExpressionMng	(NULL),
	m_bUnattendedMode		(FALSE),
	m_pAssignedCounter		(NULL),
	m_bCacheCounter			(FALSE),
	m_pCallerDocument		(NULL),
	m_pSqlConnection		(NULL),
	m_pExternalControllerInfo(NULL),
	m_pMessages				(NULL),
	m_pDocInvocationInfo	(NULL),
	m_pDocInvocationParams	(NULL),
	m_Type					(VMT_DATAENTRY),
	m_pTbContext			(NULL),
	m_pContextBag			(NULL),
	m_hFrameHandle			(NULL),
	m_pSymTable				(NULL),
	m_bForwardingSysKeydownToChild(FALSE),
	m_bForwardingSysKeydownToParent(FALSE),
	m_pDataSynchroNotifier	(NULL)
{	
	m_ThreadId = AfxGetThread()->m_nThreadID;

	m_pSymTable = new SymTable(); 

	struct _timeb timebuffer;
	if (_ftime_s(&timebuffer) == 0) //returns Zero if successful
		m_nCreationTime = timebuffer.time;
}

//------------------------------------------------------------------------------
CBaseDocument::~CBaseDocument()
{
	ASSERT_VALID(this);

	m_Handler.FireDisposing(this);
	// se sono pilotato da External controller, comunico che ho finito
	if (m_pExternalControllerInfo)
		m_pExternalControllerInfo->m_Finished.Set();

	// creato da chi mi chiama, ma distrutto da me che ne divento il possessore
	SAFE_DELETE(m_pDocInvocationInfo);
	if (m_pDocInvocationParams)
		SAFE_DELETE(m_pDocInvocationParams->m_pManagedParameters);
	SAFE_DELETE(m_pDocInvocationParams);
	SAFE_DELETE(m_pSymTable);

	ASSERT(_CrtCheckMemory());
}

//-----------------------------------------------------------------------------
void CBaseDocument::DestroyFrameHandle()
{
	if (m_hFrameHandle == NULL)
		return;

	CWnd* pWnd = CWnd::FromHandle(m_hFrameHandle);
	if (pWnd)
		pWnd->DestroyWindow();
}

//-----------------------------------------------------------------------------
BOOL CBaseDocument::UseEasyReading	()	const
{
	return 	AfxGetThemeManager()->UseEasyReading() &&
			GetFormMode() == CBaseDocument::BROWSE && 
			!m_bBatch;
}

//-----------------------------------------------------------------------------
BOOL CBaseDocument::CanPushToClient()
{
	return AfxGetLoginContext() && !AfxIsRemoteInterface() && !AfxIsInUnattendedMode() 
		&& !m_pExternalControllerInfo;  //se lanciato da scheduler non deve fare PushToclients
}

//-----------------------------------------------------------------------------
CRuntimeClass* CBaseDocument::GetControlClass(UINT id)
{
	CRuntimeClass* pClass = NULL;
	return m_RegisteredControls.Lookup(id, pClass) 
		? pClass 
		: AfxGetApplicationContext()->GetControlClass(id);
}
//-----------------------------------------------------------------------------
void CBaseDocument::RegisterControl(UINT nIDD, CRuntimeClass* pClass)
{
	m_RegisteredControls[nIDD] = pClass;
}
// Find a view of same class if already present (to allow activation or creation)
//------------------------------------------------------------------------------
CView* CBaseDocument::ViewAlreadyPresent(const CRuntimeClass* pViewClass, UINT nViewID /*= 0*/, BOOL bExact/*=FALSE*/) const
// walk through all views
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);

		BOOL bRuntimeClassOk = (bExact ? pView->GetRuntimeClass() == pViewClass : pView->IsKindOf(pViewClass));
		if (!bRuntimeClassOk)
			continue;

		if (!nViewID || pView->GetDlgCtrlID() == nViewID)
			return pView;
	}
	return NULL;
}
// Find a view of same class if already present (to allow activation or creation)
//------------------------------------------------------------------------------
CView* CBaseDocument::ViewAlreadyPresent(UINT nFrameId) const
// walk through all views
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		CDockableFrame *pParentFrame = (CDockableFrame*)pView->GetParentFrame();
		DWORD id = pParentFrame ? pParentFrame->GetIDHelp() : 0;
		if (id == nFrameId)
			return pView;
	}
	return NULL;
}
//-----------------------------------------------------------------------------
CLocalizableFrame* CBaseDocument::GetFrame() const
{
	ASSERT_VALID(this);
	CView* pView = GetFirstView();
	if (!pView)
		return NULL;
	ASSERT_VALID(pView);
	CFrameWnd* pFrame = pView->GetParentFrame();
	if (!pFrame)
		return NULL;
	ASSERT_VALID(pFrame);
	ASSERT(pFrame->IsKindOf(RUNTIME_CLASS(CLocalizableFrame)));
	return (CLocalizableFrame*) pFrame;
}

//-----------------------------------------------------------------------------
void CBaseDocument::SetNamespace (const CTBNamespace& aNamespace) 
{ 
	GetInfoOSL()->m_Namespace = aNamespace; 
}

//-----------------------------------------------------------------------------
const CDocumentDescription*	CBaseDocument::GetXmlDescription ()
{
	return m_pDocInvocationParams ? m_pDocInvocationParams->GetDocumentDescription() : NULL; 
}


//this method is identical to parent class' one, but uses IsTBWindowVisible
//instead of IsWindowVisible because of problems in web mode
//-----------------------------------------------------------------------------
void CBaseDocument::UpdateFrameCounts()
	 // assumes 1 doc per frame
{
	// walk all frames of views (mark and sweep approach)
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		ASSERT(::IsWindow(pView->m_hWnd));

		//Prj. 6709 - tentativo di risolvere il flickering
		//non vengono contate le finestre "hide"; 
		//visto che faccio "hide" per un "pò" sulla view => le frame delle rowview saltavano => in chiusura scattava SaveModified 
		CParsedForm* pParsedForm = dynamic_cast<CParsedForm*>(pView);

		if (pParsedForm && pParsedForm->GetNativeWindowVisible() || !pParsedForm && IsTBWindowVisible(pView))
		{
			CFrameWnd* pFrame = pView->GetParentFrame();
			if (pFrame != NULL)
				pFrame->m_nWindow = -1;     // unknown
		}
	}

	// now do it again counting the unique ones
	int nFrames = 0;
	pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		ASSERT(::IsWindow(pView->m_hWnd));

		//Prj. 6709 - tentativo di risolvere il flickering
		//non vengono contate le finestre "hide"; 
		//visto che faccio "hide" per un "pò" sulla view => le frame delle rowview saltavano => in chiusura scattava SaveModified 
		CParsedForm* pParsedForm = dynamic_cast<CParsedForm*>(pView);

		if (pParsedForm && pParsedForm->GetNativeWindowVisible() || !pParsedForm && IsTBWindowVisible(pView))   // Do not count invisible windows.
		{
			CFrameWnd* pFrame = pView->GetParentFrame();
			if (pFrame != NULL && pFrame->m_nWindow == -1)
			{
				ASSERT_VALID(pFrame);
				// not yet counted (give it a 1 based number)
				pFrame->m_nWindow = ++nFrames;
			}
		}
	}

	// lastly walk the frames and update titles (assume same order)
	// go through frames updating the appropriate one
	int iFrame = 1;
	pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		ASSERT(::IsWindow(pView->m_hWnd));

		//Prj. 6709 - tentativo di risolvere il flickering
		//non vengono contate le finestre "hide"; 
		//visto che faccio "hide" per un "pò" sulla view => le frame delle rowview saltavano => in chiusura scattava SaveModified 
		CParsedForm* pParsedForm = dynamic_cast<CParsedForm*>(pView);

		if (pParsedForm && pParsedForm->GetNativeWindowVisible() || !pParsedForm && IsTBWindowVisible(pView))   // Do not count invisible windows.
		{
			CFrameWnd* pFrame = pView->GetParentFrame();
			if (pFrame != NULL && pFrame->m_nWindow == iFrame)
			{
				ASSERT_VALID(pFrame);
				if (nFrames == 1)
					pFrame->m_nWindow = 0;      // the only one of its kind
				pFrame->OnUpdateFrameTitle(TRUE);
				iFrame++;
			}
		}
	}
	ASSERT(iFrame == nFrames + 1);
}

//-----------------------------------------------------------------------------
BOOL CBaseDocument::OnOpenDocument	(LPCTSTR pParam)
{
	const CSingleExtDocTemplate* pTemplate = m_pTemplate;
			
	if (pTemplate && GetXmlDescription() && GetXmlDescription()->GetManagedType().IsEmpty())
		SetNamespace(const_cast<CSingleExtDocTemplate*>(pTemplate)->GetNamespace());

	if (pTemplate)
	{
		m_bUnattendedMode = _tcsicmp(pTemplate->m_sViewMode, szUnattendedViewMode) == 0 ||
							_tcsicmp(pTemplate->m_sViewMode, szBackgroundViewMode) == 0;

		// leggo dalla descrizione Xml il tipo di documento
		CViewModeDescription* pViewModeInfo = GetXmlDescription() ? GetXmlDescription()->GetViewMode(pTemplate->m_sViewMode) : NULL;

		if (pViewModeInfo)
			m_Type = pViewModeInfo->GetType();

		// TODOBRUNA eliminare (ex inizializzazione dal template + compatibilità)
		if (!m_bBatch)
			m_bBatch = m_Type == VMT_BATCH || m_bUnattendedMode;
		
		if (m_bBatch && m_Type != VMT_BATCH)
			m_Type = VMT_BATCH;
	}
	/* i documenti managed non hanno template!
	else
	{
		ASSERT(FALSE);
		return FALSE;
	}*/

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBaseDocument::OnCloseDocument()
{
	//sono nel ciclo di lock: non posso uscire altrimenti mi schianterei
	if (m_bRetryingLock)
		return;
	// finché sono pilotato da un external controller non posso uscire
	if (IsEditingParamsFromExternalController() || IsRunningFromExternalController())
	{
		m_pExternalControllerInfo->m_Finished.Set();
		return;
	}
	
	// rimuovo il puntatore dalla lista degli oggetti accessibili
	// dalle funzioni esterne chiamate via SOAP
	AfxRemoveWebServiceStateObject(this);
	AfxGetThreadContext()->RemoveObject(this);

	//removed from document thread's array
	AfxGetThreadContext()->RemoveDocument(this); 

	/*long handle = (long)this->GetFrameHandle();
	if (CanPushToClient())
		PushToClients(AfxGetLoginContext()->GetName(), _T("DocumentListUpdated"), _T(""));
	*/
	if (m_pXMLDataManager)
		delete m_pXMLDataManager;
	m_pXMLDataManager = NULL;

	if(m_pAutoExpressionMng)
		delete m_pAutoExpressionMng;
	m_pAutoExpressionMng = NULL;
	
	if(m_pAssignedCounter)
		delete m_pAssignedCounter;
	m_pAssignedCounter = NULL;
		
	CDocument::OnCloseDocument();
}

//-----------------------------------------------------------------------------
void CBaseDocument::OnClearMessageQueue()
{
	CTBWinThread::PumpThreadMessages();
}

//-----------------------------------------------------------------------------
CString CBaseDocument::GetDefaultMenuDescription()
{
	return _T("");
}

//-----------------------------------------------------------------------------
void CBaseDocument::ClearMessageQueue()
{
	SendMessage(WM_COMMAND, UM_CLEAR_MESSAGE_QUEUE, NULL);
}

//-----------------------------------------------------------------------------
BOOL CBaseDocument::CloseDocument(BOOL bAsync /*= TRUE*/)
{
	if (bAsync)
		return PostMessage(WM_CLOSE, NULL, NULL);
	
	ASSERT(::GetCurrentThreadId() == m_ThreadId);

	/*	se non esiste, si schienterebbe comunque!
		if (!AfxGetTbCmdManager()->ExistDocument(this))
			return TRUE;*/

	if (!SaveModified())
		return FALSE;
	
	OnCloseDocument();
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CBaseDocument::SaveModified()
{
	//when I'm closing main thread document and it is in a message loop (for example a modal dialog)
	//I return false
	if (AfxMultiThreadedDocument() && AfxGetThreadContext()->GetMainDocument() == this && AfxIsThreadInInnerLoop())
	{
		TRACE("Cannot close main document thread because is busy!");
		return FALSE;
	}
	CWnd* pWnd = CWnd::FromHandle(m_hFrameHandle);
	if (pWnd && pWnd->IsKindOf(RUNTIME_CLASS(CFrameWnd)) && ((CFrameWnd*)pWnd)->InModalState())
	{
		TRACE("Cannot close a document in modal state!");
		return FALSE;
	}
	return TRUE;
}

//Metodo che restituisce se un Documento puo' essere chiuso in modo safe.
//imposta il thread in unattanded mode all'inizio del metodo e ripristina lo stato attended alla fine 
//per evitare che vengano visualizzate messagebox
//-----------------------------------------------------------------------------
BOOL CBaseDocument::CanCloseDocument()
{
	ASSERT(::GetCurrentThreadId() == m_ThreadId);

	/*	se non esiste, si schienterebbe comunque!
		if (!AfxGetTbCmdManager()->ExistDocument(this))
			return TRUE;*/

	//setto il thread in unattended(se non lo e' gia') mode cosi non verranno mostrate messagebox 
	//ne da questo documento ne da i suoi figli
	BOOL bIsUnattended = AfxIsInUnattendedMode();
	if (!bIsUnattended)
		AfxGetThreadContext()->SetUserInteractionMode(UNATTENDED);
	
	BOOL bOk = SaveModified();
	
	//Se non si era in UnattendedMode, ripristino la modalita di interazione Attended
	if (!bIsUnattended)
		AfxGetThreadContext()->SetUserInteractionMode(ATTENDED);

	return bOk;
}

//-----------------------------------------------------------------------------
void CBaseDocument::DestroyDocument()
{
	ASSERT(::GetCurrentThreadId() == m_ThreadId);

/*	se non esiste, si schienterebbe comunque!
		if (!AfxGetTbCmdManager()->ExistDocument(this))
			return;*/

	SetModifiedFlag(FALSE);
	
	//when I'm destroying main thread document and it is in a message loop (for example a modal dialog)
	//I interrupt it using an exception
	if (AfxMultiThreadedDocument() && AfxGetThreadContext()->GetMainDocument() == this && AfxIsThreadInInnerLoop())
		throw new CThreadAbortedException();

	OnCloseDocument();
}

//------------------------------------------------------------------------------
BOOL CBaseDocument::IsBackgroundMode ()	const 
{ 
	return m_pTemplate ?	
				m_pTemplate->m_sViewMode == szBackgroundViewMode ||
				m_pTemplate->m_sViewMode == szUnattendedViewMode ||
				m_pTemplate->m_sViewMode == szNoInterface
			: FALSE; 
}

//------------------------------------------------------------------------------
BOOL CBaseDocument::IsABatchDocument ()	const 
{ 
	return GetType() == VMT_BATCH;
}

//------------------------------------------------------------------------------
BOOL CBaseDocument::IsRunningAsADM () const
{
	return m_pDocInvocationParams && m_pDocInvocationParams->m_bIsRunningAsADM;
}

//------------------------------------------------------------------------------
BOOL CBaseDocument::IsABackgroundADM () const
{
	return IsRunningAsADM() && IsBackgroundMode();
}

//------------------------------------------------------------------------------
const ViewModeType CBaseDocument::GetType () const
{
	return m_Type;
}

//------------------------------------------------------------------------------
void CBaseDocument::SetType (const ViewModeType aMode)
{
	m_Type = aMode;
}

//------------------------------------------------------------------------------
BOOL CBaseDocument::IsInUnattendedMode () const	
{ 
	return m_bUnattendedMode || IsRunningFromExternalController() || AfxIsCurrentlyInUnattendedMode();
}

//------------------------------------------------------------------------------
BOOL CBaseDocument::SetInUnattendedMode (BOOL bSet /*= TRUE*/)	
{ 
	BOOL bOldValue = m_bUnattendedMode;
	m_bUnattendedMode = bSet;
	return bOldValue;
}

//------------------------------------------------------------------------------
BOOL CBaseDocument::IsEditingParamsFromExternalController() const 
{ 
	return m_pExternalControllerInfo && m_pExternalControllerInfo->IsEditing();
}
	
//------------------------------------------------------------------------------
BOOL CBaseDocument::IsRunningFromExternalController() const 
{ 
	return m_pExternalControllerInfo && m_pExternalControllerInfo->IsRunning();
}

//------------------------------------------------------------------------------
BOOL CBaseDocument::IsExternalControlled()	const
{
	return m_pExternalControllerInfo && m_pExternalControllerInfo->IsControlling();
}

//------------------------------------------------------------------------------
void CBaseDocument::SetRunningTaskStatus (CExternalControllerInfo::RunningTaskStatus status) const
{
	if (m_pExternalControllerInfo)
		m_pExternalControllerInfo->SetRunningTaskStatus(status);
}

//-----------------------------------------------------------------------------
void CBaseDocument::SetNsCurrentViewParent (const CTBNamespace& aNs)
{
	m_nsCurrentViewParent = aNs;
}

//-----------------------------------------------------------------------------
void CBaseDocument::SetNsCurrentRowView	(const CTBNamespace& aNs)
{
	m_nsCurrentRowView = aNs;
}

//-----------------------------------------------------------------------------
void CBaseDocument::Activate(CView* pView /*= NULL*/, BOOL bPostMessage /*= FALSE*/)
{
	if (pView == NULL)
		pView = GetFirstView();

	CFrameWnd* pFrame = pView->GetParentFrame();
	//nella versione con interfaccia WindowsForm, il frame non e' presente
	if (!pFrame)
	{
		if (!m_hFrameHandle)
			return;
		if (bPostMessage)
			::PostMessage(m_hFrameHandle, WM_ACTIVATE, (WPARAM)WA_ACTIVE, (LPARAM)m_hFrameHandle);
		else
			::SendMessage(m_hFrameHandle, WM_ACTIVATE, (WPARAM)WA_ACTIVE, (LPARAM)m_hFrameHandle);
		
		if (!AfxGetLoginContext()->GetDocked())
			::BringWindowToTop(m_hFrameHandle);
		return;
	}
	ASSERT(pFrame);
	ASSERT_KINDOF(CFrameWnd, pFrame);
	
	//se la finestra e` gia` attiva, non ha senso riattivarla
	//(generando dei messaggi inutili, con malfunzionamenti ad es. nei cespiti an. 17351)
	if (!pFrame || AfxGetThread()->m_pActiveWnd == pFrame)
		return;

	if (bPostMessage)
		pFrame->PostMessage(WM_ACTIVATE, (WPARAM)WA_ACTIVE, (LPARAM)pFrame->m_hWnd);
	else
		pFrame->ActivateFrame();

	pFrame->SetActiveView(pView);

	if (!AfxGetLoginContext()->GetDocked())
		pFrame->BringWindowToTop();
}

//-----------------------------------------------------------------------------
CView* CBaseDocument::GetFirstView() const
{
	POSITION pos = GetFirstViewPosition();
	if (!pos)
		return NULL;

	CView*	pView = GetNextView(pos);
	ASSERT_VALID (pView);
	return pView;
}

//------------------------------------------------------------------------------
CBaseDocument::FormMode CBaseDocument::SetFormMode(FormMode aFormMode)
{
	FormMode fmOldFormMode = GetFormMode();
	m_FormMode = aFormMode;

	return fmOldFormMode;
}

// Controllo delle risorse di sistema necessarie al documento
// In Win 95 non e` necessario
//-----------------------------------------------------------------------------
BOOL CBaseDocument::ResourceAvailable()
{
	return TRUE;
}


//-----------------------------------------------------------------------------
BOOL CBaseDocument::IsExporting	() const
{
	return m_pXMLDataManager ? 
				(m_pXMLDataManager->GetStatus() == CXMLDataManagerObj::XML_MNG_EXPORTING_DATA):
				FALSE;
}

//-----------------------------------------------------------------------------
BOOL CBaseDocument::IsImporting() const
{
	return m_pXMLDataManager ? 
				(m_pXMLDataManager->GetStatus() == CXMLDataManagerObj::XML_MNG_IMPORTING_DATA): 
				FALSE;
}

//-----------------------------------------------------------------------------
BOOL CBaseDocument::IsImportingFastMode() const
{
	return m_pXMLDataManager ? 
			(
				m_pXMLDataManager->GetStatus() == CXMLDataManagerObj::XML_MNG_IMPORTING_DATA &&
				!m_pXMLDataManager->UseOldXTechMode()
			): 
			FALSE;
}

//----------------------------------------------------------------------------
void CBaseDocument::UseAutoExpression(BOOL bUseAutoExpression)
{ 
	if(bUseAutoExpression && !m_pAutoExpressionMng)
		m_pAutoExpressionMng = new CAutoExpressionMng();

	if(!bUseAutoExpression && m_pAutoExpressionMng)
	{
		delete m_pAutoExpressionMng;
		m_pAutoExpressionMng = NULL;
	}
}

//----------------------------------------------------------------------------
void CBaseDocument::SetAssignedCounter (DataObj* pDataObj)
{
	// posso decidere dall'esterno se effettuare il caching del counter
	if(!m_bCacheCounter) return;

	if(m_pAssignedCounter)
	{
		if(m_pAssignedCounter->IsValueLocked ())
			return;

		delete m_pAssignedCounter; 
	}

	m_pAssignedCounter = pDataObj ? pDataObj->DataObjClone() : NULL;
}

//----------------------------------------------------------------------------
///previene l'utilizzo della message box se la messaggistica è disabilitata
int CBaseDocument::Message(LPCTSTR lpszText, UINT nType /*= MB_OK*/, UINT nIDHelp /*= 0*/, LPCTSTR lpszAdditionalText /*= NULL*/, CMessages::MessageType MsgType /*MSG_ERROR*/)
{
	if (IsInUnattendedMode())
	{
		m_pMessages->Add(lpszText, MsgType);
		return NO_MSG_BOX_SHOWN;
	}
	return AfxMessageBox(lpszText + CString(_T(" ")) + CString(lpszAdditionalText), nType, nIDHelp);	
}

//----------------------------------------------------------------------------
CString CBaseDocument::GetDictionaryPath(BOOL bStandard)
{
	return AfxGetDictionaryPathFromNamespace(GetNamespace(), bStandard);
}

//---------------------------------------------------------------------------
BOOL CBaseDocument::InvokeRequired()
{
	return m_ThreadId != ::GetCurrentThreadId();
}

//----------------------------------------------------------------------------
CString CBaseDocument::FormatRollbackLogMessage ()
{
	return _T("");
}

//----------------------------------------------------------------------------
CString CBaseDocument::GetUICulture()
{
	return AfxGetCulture();
}

CString CBaseDocument::SetUICulture(const CString& sCulture)
{
	return AfxGetThreadContext()->SetUICulture(sCulture.IsEmpty() ? _T("en") : sCulture);
}

//----------------------------------------------------------------------------
CManagedDocComponentObj* CBaseDocument::GetManagedParameters () 
{ 
	return m_pDocInvocationParams ?  m_pDocInvocationParams->m_pManagedParameters : NULL; 
}

//----------------------------------------------------------------------------
CDesignModeManipulatorObj* CBaseDocument::GetDesignModeManipulatorObj()
{
	return GetManagedParameters() ? GetManagedParameters()->GetDesignModeManipulatorObj() : NULL;
}

//----------------------------------------------------------------------------
CImportExportParams* CBaseDocument::GetImportExportParams()
{
	CImportExportParams* pManipulator = dynamic_cast<CImportExportParams*>(GetManagedParameters());
	return pManipulator;
}

//----------------------------------------------------------------------------
///<summary>
///It retrives ownerid's namespace web-methods list
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false, thiscall_method=true, inEasyBuilder_method=false)]
DataBool CBaseDocument::GetMethods (DataArray& /*string*/ arMethods)
{
	arMethods.RemoveAll();

	SymField aField(_T(""), DataType::Object);
		DataLng* pData = (DataLng*) aField.GetData();
		ASSERT(pData);
		ASSERT_KINDOF(DataLng, pData);
		pData->Assign((long)(this));
		pData->SetAsHandle();
	
	BOOL bOk = aField.AddMethods(RUNTIME_CLASS(CBaseDocument), AfxGetAddOnAppsTable()->GetMapWebClass());
	if (!bOk)
		return DataBool(FALSE);

	CFunctionDescriptionArray* pfd = aField.GetMethodsList();
	if (!bOk)
		return DataBool(FALSE);

	for (int i = 0; i < pfd->GetCount(); i++)
	{
		CFunctionDescription* pf = pfd->GetAt(i);
		DataStr* psNsf = new DataStr(pf->GetNamespace().ToUnparsedString());
		arMethods.Add(psNsf);
	}
	return DataBool(TRUE);
}

//----------------------------------------------------------------------------
BOOL CBaseDocument::CanRunDocumentInStandAloneMode() //restituisce TRUE se non c'è un unico documento aperto e se non ci sono altri utenti connessi all'azienda, FALSE altrimenti
{ 
	return AfxGetLoginManager()->GetCompanyLoggedUsersNumber(AfxGetLoginInfos()->m_nCompanyId) == 1 && AfxGetLoginContext()->GetOpenDocuments() == 1;
}

//-----------------------------------------------------------------------------
BOOL CBaseDocument::CheckContextObject(const CString& strName)
{
	if (!m_pContextBag)
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return FALSE; 
	}

	return ::CheckContextObject(m_pContextBag, strName);
}

//-----------------------------------------------------------------------------
DataStr* CBaseDocument::AddContextString(const DataStr& aName)
{
	if (!m_pContextBag)
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL; 
	}

	return ::AddContextString(m_pContextBag, aName);
}

//-----------------------------------------------------------------------------
DataInt* CBaseDocument::AddContextInt(const DataStr& aName)
{
	if (!m_pContextBag)
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL;
	}

	return ::AddContextInt(m_pContextBag, aName);
}

//-----------------------------------------------------------------------------
DataDate* CBaseDocument::AddContextDate(const DataStr& aName)
{
	if (!m_pContextBag)
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL;
	}

	return ::AddContextDate(m_pContextBag, aName);
}

//-----------------------------------------------------------------------------
DataStr* CBaseDocument::LookupContextString(const DataStr& aName)
{
	if (!m_pContextBag)
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL; 
	}

	return ::LookupContextString(m_pContextBag, aName);
}

//-----------------------------------------------------------------------------
DataInt* CBaseDocument::LookupContextInt(const DataStr& aName)
{
	if (!m_pContextBag)
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL;
	}

	return ::LookupContextInt(m_pContextBag, aName);
}

//-----------------------------------------------------------------------------
DataDate* CBaseDocument::LookupContextDate(const DataStr& aName)
{
	if (!m_pContextBag)
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL;
	}

	return ::LookupContextDate(m_pContextBag, aName);
}

//-----------------------------------------------------------------------------
DataStr CBaseDocument::ReadContextString(const DataStr& aName)
{
	if (!m_pContextBag)
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return _T("");
	}

	return ::ReadContextString(m_pContextBag, aName);
}

//-----------------------------------------------------------------------------
DataInt CBaseDocument::ReadContextInt(const DataStr& aName)
{
	if (!m_pContextBag)
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return -1;
	}

	return ::ReadContextInt(m_pContextBag, aName);
}

//-----------------------------------------------------------------------------
DataDate CBaseDocument::ReadContextDate(const DataStr& aName)
{
	if (!m_pContextBag)
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return DataDate::NULLDATE;
	}

	return ::ReadContextDate(m_pContextBag, aName);
}

//-----------------------------------------------------------------------------
void CBaseDocument::AddFormsOnDockPane(CTaskBuilderDockPane* pPane) 
{ 
	OnAddFormsOnDockPane(pPane); 
}
//-----------------------------------------------------------------------------
void CBaseDocument::OnUpdateSwitchTo	(CCmdUI* pCmdUI)	{ pCmdUI->Enable(TRUE); }
//-----------------------------------------------------------------------------
void CBaseDocument::OnUpdateBackToMenu	(CCmdUI* pCmdUI)	{ pCmdUI->Enable(TRUE); }
//-----------------------------------------------------------------------------
void CBaseDocument::OnUpdateActionsCopy	(CCmdUI* pCmdUI)	{ pCmdUI->Enable(TRUE); }

//-----------------------------------------------------------------------------
void CBaseDocument::OnBackToMenu()
{
	if (!GetNotValidView(TRUE))
		::PostMessage(AfxGetMenuWindowHandle(), UM_ACTIVATE_MENU, (WPARAM)GetFrameHandle(), NULL);
}

//-----------------------------------------------------------------------------
void CBaseDocument::OnSwitchTo()
{
	CMenu menu;
	menu.CreatePopupMenu();
	
	CBaseFrame* pFrame = dynamic_cast<CBaseFrame*>(GetFrame());
	if (!pFrame) return;

	pFrame->MakeSwitchTomenu(&menu);

	CPoint point = pFrame->GetPositionSwitchTo();

	CSize  sizeX;
	sizeX.cx = 0; sizeX.cy = 22;	//altezza voce di menu
	// aproxymate position
	CONST UINT space = 10;

	CRect r;
	pFrame->GetWindowRect(r);

	BOOL bDesc = abs(point.y - r.top) < abs(r.bottom - point.y);

	int y1;
	if (bDesc)
		y1 = point.y;
	else
		y1 = point.y - (menu.GetMenuItemCount() * sizeX.cy + space);

	menu.TrackPopupMenu(TPM_LEFTBUTTON, point.x, y1, pFrame);
}

//-----------------------------------------------------------------------------
void CBaseDocument::OnSwitchTo(UINT nID)
{
	GetNotValidView(TRUE);

	int n = nID - ID_SWITCHTO_START;
	int k = 0;
	LongArray handles;
	AfxGetWebServiceStateObjectsHandles(handles, RUNTIME_CLASS(CBaseDocument));
	for (int i = 0; i < handles.GetCount(); i++)
	{
		CBaseDocument* pDoc = (CBaseDocument*)handles[i];
		if (pDoc == this) continue;
		if (!pDoc->CanShowInOpenDocuments()) continue;

		if (n == k)
		{
			pDoc->SendMessage(WM_ACTIVATE, WA_ACTIVE, (LPARAM)GetFrameHandle());
		}
		k++;
	}
}

//----------------------------------------------------------------------------
void CBaseDocument::NotifiyToDataSynchronizer()
{
	if (m_pDataSynchroNotifier)
		m_pDataSynchroNotifier->NotifiyToDataSynchronizer();
}

//----------------------------------------------------------------------------
void CBaseDocument::RemoveNotifications()
{
	if (m_pDataSynchroNotifier)
		m_pDataSynchroNotifier->RemoveNotifications();
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CBaseDocument::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nCBaseDocument");
}

void CBaseDocument::AssertValid() const
{
	CDocument::AssertValid();
}
#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// CXMLDataManagerObj
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CXMLDataManagerObj, CObject)

CXMLDataManagerObj::~CXMLDataManagerObj()
{
}

/////////////////////////////////////////////////////////////////////////////
// CDataSynchroNotifierObj
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CDataSynchroNotifierObj, CObject)

