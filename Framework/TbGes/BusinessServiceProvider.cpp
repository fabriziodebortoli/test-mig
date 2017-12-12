
#include "stdafx.h"

#include "bodyedit.h"

#include "BusinessServiceProvider.h"
#include "UIBusinessServiceProvider.h"

//extdoc resource
#include "extdoc.hjson"
#include "UIBusinessServiceProvider.hjson" 

/////////////////////////////////////////////////////////////////////////////
#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//					CBSPArray implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
int CBSPArray::Add(CBusinessServiceProviderObj* pBSP, BOOL bCheckDuplicates /*= TRUE*/)	
{ 
	if (bCheckDuplicates)
		for (int i = 0; i <= GetUpperBound(); i++)
			//se è già stato associato lo devo distuggere e non agganciarlo di nuovo
			if (GetAt(i)->GetRuntimeClass() == pBSP->GetRuntimeClass()) 
			{
				delete pBSP;
				pBSP = NULL;
				return -1;
			}
	return Array::Add(pBSP);
}

//////////////////////////////////////////////////////////////////////////////
//						CBusinessServiceProviderClientDocObj				//
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CBusinessServiceProviderClientDocObj, CClientDoc)

//-----------------------------------------------------------------------------
CBusinessServiceProviderClientDocObj::CBusinessServiceProviderClientDocObj(CBusinessServiceProviderObj*	pBSP)
	:
	CClientDoc	(),
	m_pBSP		(pBSP)
{
	if (pBSP) { ASSERT_VALID(pBSP); }
	m_hResourceModule = GetDllInstance(m_pBSP->GetRuntimeClass());
}

//TODO PERASSO verificare necessita di distinguere fra numerazione di TB e ERP
//per ora non distinguo
#define MIN_CONTROL_IDC MinTbControl
#define MAX_CONTROL_IDC MaxTbControl

#define MIN_COMMAND_ID MinTbCommand
#define MAX_COMMAND_ID MaxTbCommand

//////////////////////////////////////////////////////////////////////////////
//						CBusinessServiceProviderClientDoc					//
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CBusinessServiceProviderClientDoc, CBusinessServiceProviderClientDocObj)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBusinessServiceProviderClientDoc, CBusinessServiceProviderClientDocObj)
	//{{AFX_MSG_MAP(CBusinessServiceProviderClientDoc)
	ON_EN_VALUE_CHANGED_RANGE	(MIN_CONTROL_IDC,							MAX_CONTROL_IDC,					OnControlChanged)
	ON_CONTROL_RANGE			(BEN_ROW_CHANGED,							MIN_CONTROL_IDC, MAX_CONTROL_IDC,	OnControlChanged)
	ON_COMMAND_RANGE			(MIN_CONTROL_IDC,							MAX_CONTROL_IDC,					OnClicked)
	ON_CTRL_STATE_RANGE			(MIN_CONTROL_IDC,							MAX_CONTROL_IDC,					OnCtrlStateChanged)

	//ON_COMMAND_RANGE(MIN_COMMAND_ID, 0xE099, OnCommand)
	//ON_COMMAND_RANGE(MIN_COMMAND_ID, 0xF000, OnCommand)
	ON_UPDATE_COMMAND_UI_RANGE	(MIN_COMMAND_ID,							MAX_COMMAND_ID,						OnUpdateShowUI)
	ON_UPDATE_COMMAND_UI_RANGE	(MIN_COMMAND_ID,							MAX_COMMAND_ID,						OnUpdateShowUI)

	ON_CONTROL_RANGE			(UM_TREEVIEWADV_SELECTION_CHANGED,			MIN_CONTROL_IDC, MAX_CONTROL_IDC,	OnCtrlTreeviewadvSelectionChanged)
	ON_CONTROL_RANGE			(UM_TREEVIEWADV_ITEM_DRAG,					MIN_CONTROL_IDC, MAX_CONTROL_IDC,	OnCtrlTreeviewadvItemDrag)
	ON_CONTROL_RANGE			(UM_TREEVIEWADV_DRAG_OVER,					MIN_CONTROL_IDC, MAX_CONTROL_IDC,	OnCtrlTreeviewadvDragOver)
	ON_CONTROL_RANGE			(UM_TREEVIEWADV_DRAG_DROP,					MIN_CONTROL_IDC, MAX_CONTROL_IDC,	OnCtrlTreeviewadvDragDrop)
	ON_CONTROL_RANGE			(UM_TREEVIEWADV_MOUSE_UP,					MIN_CONTROL_IDC, MAX_CONTROL_IDC,	OnCtrlTreeviewadvMouseUp)
	ON_CONTROL_RANGE			(UM_TREEVIEWADV_MOUSE_DOWN,					MIN_CONTROL_IDC, MAX_CONTROL_IDC,	OnCtrlTreeviewadvMouseDown)
	ON_CONTROL_RANGE			(UM_TREEVIEWADV_MOUSE_CLICK,				MIN_CONTROL_IDC, MAX_CONTROL_IDC,	OnCtrlTreeviewadvMouseClick)
	ON_CONTROL_RANGE			(UM_TREEVIEWADV_CONTEXT_MENU_ITEM_CLICK,	MIN_CONTROL_IDC, MAX_CONTROL_IDC,	OnCtrlTreeviewadvContextMenuItemClick)
	ON_CONTROL_RANGE			(UM_TREEVIEWADV_MOUSE_DOUBLE_CLICK,			MIN_CONTROL_IDC, MAX_CONTROL_IDC,	OnCtrlTreeviewadvMouseDoubleClick)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBusinessServiceProviderClientDoc::CBusinessServiceProviderClientDoc
	(
			CBusinessServiceProviderObj*	pBSP,
			MessagesToEvents*				pMTEMap
	)
	:
	CBusinessServiceProviderClientDocObj	(pBSP),
	m_pMTEMap								(pMTEMap)
{
	// anche se il parametro ha il default a NULL per l'IMPLEMENT_DYNCREATE, non e' ammesso crearlo
	// senza passare il BusinessServiceProvider
	ASSERT(m_pBSP);
	SetMsgRoutingMode(CD_MSG_AFTER);
}

//-----------------------------------------------------------------------------
CBusinessServiceProviderClientDoc::~CBusinessServiceProviderClientDoc()
{
	SAFE_DELETE(m_pMTEMap);
	// il BSP owner e' registrato come EventManager per questo ClientDoc, ma non deve essere distrutto
	// in questo momento, previene la SAFE_DELETE(m_pEventManager) che c'e' nel distruttore di ClientDoc
	m_pEventManager = NULL;
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnControlChanged(UINT nID)
{
	CString strEvent;

	if	(
			m_pMTEMap &&
			m_pMTEMap->HasOnValueChangedEvent(nID,strEvent)
		)
		m_pBSP->FireAction(strEvent);

	if (
			m_pBSP->m_nButtonCmd != 0 &&
			m_pBSP->m_nRowChangedBE != 0 &&
			nID == m_pBSP->m_nRowChangedBE
		)
		m_pBSP->EnableBEButtonUI();
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnClicked(UINT nID)
{
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnCtrlStateChanged(UINT nID)
{
	CString strEvent;

	if	(
			m_pMTEMap &&
			m_pMTEMap->HasOnCtrlStateChanged(nID,strEvent)
		)
		m_pBSP->FireAction(strEvent);
}

// @@BAUZI metodo nato per non mangiare i cmd della slaveframe non intercettati dal documento
//vedi anomalia 18483 (correzione concordata con A. Rinaldi)
//-----------------------------------------------------------------------------
BOOL  CBusinessServiceProviderClientDoc::IsBadCmdMsg(UINT nID)
{
	return (nID == ID_EXTDOC_GOTO_MASTER);
}

//-----------------------------------------------------------------------------
BOOL CBusinessServiceProviderClientDoc::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	// vedo se si tratta di un comando che gestisco io (e lo gestisco)
	if (nCode == CN_COMMAND && OnCommand(nID))
		return TRUE;			//gestito

	// Se si tratta di un comando di update, seguo il giro normale, che eventualmente chiamarà il mio OnUpdateShowUI
	// Altrimenti, seguo il giro normale
	return __super::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
}

//-----------------------------------------------------------------------------
BOOL CBusinessServiceProviderClientDoc::OnCommand(UINT nID)
{
	CString strEvent;

	if (
			m_pBSP->m_nButtonCmd != 0 &&
			nID == m_pBSP->m_nButtonCmd
		)
	{
		m_pBSP->OnShowUI(GetFocusedCtrl());
		return TRUE;//gestito
	}
	else if
		(
			m_pMTEMap &&
			m_pMTEMap->HasOnCommandEvent(nID, strEvent)
		)
	{
		m_pBSP->FireAction(strEvent);
		return TRUE;//gestito
	}
	return FALSE;//non gestito
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnUpdateShowUI(CCmdUI* PCCmdUI)
{
	MappedEvent* pEvent;
	CParsedCtrl* pCtrl;
	if (
		m_pBSP->m_nButtonCmd != 0 &&
		PCCmdUI->m_nID == m_pBSP->m_nButtonCmd
		)
	{
		PCCmdUI->Enable
		(
			m_pBSP->EnableUI() &&
			(
				!m_pMTEMap->HasEnabledWhenFocused() ||
				(
				(pCtrl = GetFocusedCtrl()) != NULL &&
				m_pMTEMap->IsUIEnabledWhenFocused(pCtrl->GetCtrlID())
			)
			)
		);
	}
	else if
		(
			m_pMTEMap &&
			m_pMTEMap->HasOnUpdateCommandUI(PCCmdUI->m_nID, pEvent)
		)
	{
		//m_pBSP->FireAction(strEvent,PCCmdUI);
		(pEvent->m_pObj->*(pEvent->m_pFunction->m_voidPtrFunc))(PCCmdUI);
	}
	else
	{
		PCCmdUI->ContinueRouting();//non lo gestisco io, proseguo il routing
	}
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnBEEnableButton(CBodyEdit* pBE, CBEButton* pBEButton)
{
	CParsedCtrl* pCtrl;

	if (
			m_pBSP->m_nButtonCmd != 0 &&
			pBEButton->m_nID == m_pBSP->m_nButtonCmd
   	   )
	{
		pBEButton->EnableButton
		(
			m_pBSP->EnableUI() &&
			(
				!m_pMTEMap->HasEnabledWhenFocused() ||
				(
				(pCtrl = GetFocusedCtrl()) != NULL &&
				m_pMTEMap->IsUIEnabledWhenFocused(pCtrl->GetCtrlID())
			)
			)
		);
	}
}

//Metodo che data un'azione (COMMAND), dice se e' supportata in modalita' web
//------------------------------------------------------------------------------
WebCommandType CBusinessServiceProviderClientDoc::OnGetWebCommandType(UINT commandID)
{	
	return m_pBSP->GetWebCommandType(commandID);
}

//-----------------------------------------------------------------------------
CParsedCtrl* CBusinessServiceProviderClientDoc::GetFocusedCtrl()
{
	CWnd* pWnd = CWnd::GetFocus();
	if (!pWnd)
		return NULL;

	if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedEdit)))
		return (CParsedEdit*)pWnd;

	// caso speciale della ComboBox TB: il control con il fuoco non e' il vero ParsedCtrl, devo 
	// recuperarlo
	return CParsedCombo::IsChildEditCombo(pWnd);
}

//-----------------------------------------------------------------------------
BOOL CBusinessServiceProviderClientDoc::IsUIEnabledWhenFocused(int nID)
{
	return	!m_pMTEMap ||
			m_pMTEMap->IsUIEnabledWhenFocused(nID);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::Customize()
{ 
	// add the default UI button via its icon namespace
	if (m_pBSP->m_UIStyle == CBusinessServiceProviderObj::POPUP && m_pBSP->m_nButtonCmd != 0 && !m_pBSP->m_strButtonIcon.IsEmpty())
	{
		AddButton(
						m_pBSP->m_nButtonCmd, 
						m_pBSP->m_strNamespace + "." + m_pBSP->m_strButtonName, 
						m_pBSP->m_strButtonIcon,  
						m_pBSP->m_strButtonCaption,
						m_pBSP->m_strToolbarName.IsEmpty() ? szToolbarNameMain : m_pBSP->m_strToolbarName,
						m_pBSP->m_strButtonToolTip,
						FALSE
					);
	}

	// allow specific buttons and/or panes to be added by the derived BSP 
	m_pBSP->CreateUIPane();
	m_pBSP->Customize(); 
}
//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnFrameCreated()
{
	if (m_pBSP->GetUIStyle() != CBusinessServiceProviderObj::UIStyle::JSON_PANE || !m_pBSP->m_nPaneId)
		return;

	CAbstractFormDoc* pDoc  = GetMasterDocument();
	if (!pDoc)
		return;

	if (pDoc->GetMasterFrame())
		m_pBSP->m_pUIPane = (CBusinessServiceProviderDockPane*)pDoc->GetMasterFrame()->GetDockPane()->GetPane(m_pBSP->m_nPaneId);

	if (!m_pBSP->m_pUIPane)
		return;
	
	m_pBSP->m_pUIPane->AttachBSP(m_pBSP);

}
//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::CustomizeBodyEdit(CBodyEdit* pBodyEdit)
{ 
	// add the default UI button via its icon namespace
	if (
			m_pBSP->m_UIStyle == CBusinessServiceProviderObj::BE_POPUP		  && 
			m_pBSP->m_nButtonCmd != 0										  && 
			!m_pBSP->m_strButtonIcon.IsEmpty()								  &&
			pBodyEdit->GetNamespace().GetObjectName() == m_pBSP->m_strBEName
	   )
	{
		m_pBSP->m_pBEBtn = pBodyEdit->m_HeaderToolBar.AddButton
		(
			m_pBSP->m_strButtonName,
			m_pBSP->m_nButtonCmd,
			m_pBSP->m_strButtonIcon,
			m_pBSP->m_strButtonToolTip,
			m_pBSP->m_strButtonCaption
		);
		m_pBSP->m_pBEBtn->EnableButton(FALSE);
	}

	m_pBSP->CustomizeBodyEdit(pBodyEdit); 
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnCtrlTreeviewadvSelectionChanged(UINT nID)
{
	CString strEvent;

	if	(
			m_pMTEMap &&
			m_pMTEMap->HasOnCtrlTreeviewadvSelectionChanged(nID,strEvent)
		)
		m_pBSP->FireAction(strEvent);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnCtrlTreeviewadvItemDrag(UINT nID)
{
	CString strEvent;

	if	(
			m_pMTEMap &&
			m_pMTEMap->HasOnCtrlTreeviewadvItemDrag(nID, strEvent)
		)
		m_pBSP->FireAction(strEvent);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnCtrlTreeviewadvDragOver(UINT nID)
{
	CString strEvent;

	if	(
			m_pMTEMap &&
			m_pMTEMap->HasOnCtrlTreeviewadvDragOver(nID, strEvent)
		)
		m_pBSP->FireAction(strEvent);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnCtrlTreeviewadvDragDrop(UINT nID)
{
	CString strEvent;

	if	(
			m_pMTEMap &&
			m_pMTEMap->HasOnCtrlTreeviewadvDragDrop(nID, strEvent)
		)
		m_pBSP->FireAction(strEvent);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnCtrlTreeviewadvMouseUp(UINT nID)
{
	CString strEvent;

	if	(
			m_pMTEMap &&
			m_pMTEMap->HasOnCtrlTreeviewadvMouseUp(nID, strEvent)
		)
		m_pBSP->FireAction(strEvent);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnCtrlTreeviewadvMouseDown(UINT nID)
{
	CString strEvent;

	if	(
			m_pMTEMap &&
			m_pMTEMap->HasOnCtrlTreeviewadvMouseDown(nID, strEvent)
		)
		m_pBSP->FireAction(strEvent);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnCtrlTreeviewadvMouseClick(UINT nID)
{
	CString strEvent;

	if	(
			m_pMTEMap &&
			m_pMTEMap->HasOnCtrlTreeviewadvMouseClick(nID, strEvent)
		)
		m_pBSP->FireAction(strEvent);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnCtrlTreeviewadvContextMenuItemClick(UINT nID)
{
	CString strEvent;

	if	(
			m_pMTEMap &&
			m_pMTEMap->HasOnCtrlTreeviewadvContextMenuItemClick(nID, strEvent)
		)
		m_pBSP->FireAction(strEvent);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderClientDoc::OnCtrlTreeviewadvMouseDoubleClick(UINT nID)
{
	CString strEvent;

	if	(
			m_pMTEMap &&
			m_pMTEMap->HasOnCtrlTreeviewadvMouseDoubleClick(nID, strEvent)
		)
		m_pBSP->FireAction(strEvent);
}

//////////////////////////////////////////////////////////////////////////////
//						class CBusinessServiceProviderObj
//////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC (CBusinessServiceProviderObj, CEventManager)

//-----------------------------------------------------------------------------	
CBusinessServiceProviderObj::CBusinessServiceProviderObj
	(
				CAbstractFormDoc*			pCallerDoc,
		const	CString&					strNamespace
	)
	:
	m_pCallerDoc			(pCallerDoc),
	m_nButtonCmd			(0),
	m_UIStyle				(NONE),
	m_strButtonName			(_T("")),
	m_nDocAccelIDR			(0),
	m_strNamespace			(strNamespace),
	m_nPaneId				(0),
	m_pPaneUIViewClass		(NULL),
	m_pUIDoc				(NULL),
	m_pUIPane				(NULL),
	m_pClientDoc			(NULL),
	m_bMessageRoutingEnabled(FALSE), // routing disabilitato per default (evito il ClientDoc)
	m_bUIAlwaysOnFront		(FALSE),
	m_bEnableUIAlwaysOnFront(TRUE),
	m_bEnableSwitchToCaller	(TRUE),
	m_bUIOpeningRequested	(FALSE),
	m_initCX				(500),
	m_initCY				(500),
	m_pBEBtn				(NULL),
	m_pMTEMap				(NULL)
{
	// il documento puo' anche essere inizialmente NULL, poi viene valorizzato dopo
	// (es.: TestServiceProvider)
	if (m_pCallerDoc)
		ASSERT(m_pCallerDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)));

	m_pCallerDoc->m_pBSPs->Add(this);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::SetUIStyle(UIStyle aStyle)
{
	m_UIStyle = aStyle;
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::SetButton	
	(
		const	UINT						nButtonCmd,
		const	CString&					sButtonName,
		const	CString&					sButtonCaption,
		const	CString&					sButtonIcon,
		const	UINT						nDocAccelIDR,
		const	CString&					sToolbarName	/*= _T("")*/,
		const	CString&					sButtonToolTip	/*= _T("")*/
	)
{ 
	m_UIStyle			= POPUP;
	m_nButtonCmd		= nButtonCmd;
	m_strButtonName		= sButtonName;
	m_strButtonCaption	= sButtonCaption;
	m_strButtonIcon     = sButtonIcon;
	m_nDocAccelIDR		= nDocAccelIDR;
	m_strToolbarName	= sToolbarName;
	
	m_strButtonToolTip	= sButtonToolTip.IsEmpty() ? m_strButtonCaption : sButtonToolTip;
}


//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::SetJsonButton
(
	const	UINT						nButtonCmd,
	const	UINT						nDocAccelIDR
)
{
	m_UIStyle		= POPUP;
	m_nButtonCmd	= nButtonCmd;
	m_nDocAccelIDR	= nDocAccelIDR;
}


//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::SetStatusTile()
{
	m_UIStyle		= STATUS_TILE;
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::SetJsonPane(UINT nPaneID /*= 0*/)
{
	m_UIStyle = JSON_PANE;
	m_nPaneId = nPaneID;
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::SetPane	
	(
			const	UINT						nPaneId,
					CRuntimeClass*				pUIViewClass,
			const	CString&					sPaneName,
			const	CString&					sPaneCaption,
			const	CString&					sPaneIcon,
					CString						sTabPaneMainCaption /*= _T("")*/
)
{
	m_UIStyle				= PANE;
	m_nPaneId				= nPaneId;
	m_pPaneUIViewClass		= pUIViewClass;
	m_strPaneName			= sPaneName;
	m_strPaneCaption		= sPaneCaption;
	m_strPaneIcon			= sPaneIcon;
	m_strTabPaneMainCaption = sTabPaneMainCaption;
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::SetBEButton	
	(
		const	UINT						nButtonCmd,
		const	CString&					sButtonName,
		const	CString&					sButtonCaption,
		const	CString&					sButtonToolTip,
		const	CString&					sButtonIcon,
		const	CString&					sBEName,
		const   UINT						nRowChangedBE /* 0 */,
		const	UINT						nDocAccelIDR /* 0 */
	)
{ 
	m_UIStyle			= BE_POPUP;
	m_nButtonCmd		= nButtonCmd;
	m_strButtonName		= sButtonName;
	m_strButtonCaption	= sButtonCaption;
	m_strButtonToolTip  = sButtonToolTip;
	m_strButtonIcon     = sButtonIcon;
	m_strBEName			= sBEName;
	m_nRowChangedBE	    = nRowChangedBE;
	m_nDocAccelIDR		= nDocAccelIDR;
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::SetBEJsonButton
(
	const	UINT						nButtonCmd,
	const   UINT						nRowChangedBE /* 0 */,
	const	UINT						nDocAccelIDR /* 0 */
)
{
	m_UIStyle		= BE_POPUP;
	m_nButtonCmd	= nButtonCmd;
	m_nRowChangedBE = nRowChangedBE;
	m_nDocAccelIDR	= nDocAccelIDR;
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::EnableBEButtonUI(BOOL bEnabled)
{
	if (m_pBEBtn)
		m_pBEBtn->EnableButton(bEnabled);
}

//-----------------------------------------------------------------------------	
void CBusinessServiceProviderObj::EnableBEButtonUI()
{
	if (m_pBEBtn)
		m_pBEBtn->EnableButton(EnableUI());
}

//-----------------------------------------------------------------------------
CBusinessServiceProviderObj::~CBusinessServiceProviderObj()
{
	if (m_pUIDoc)
	{
		ASSERT(FALSE);
		TRACE("E' necessario chiamare la OnBeforeDocument prima di distruggere il BSP");
	}
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::OnBeforeCloseDocument()
{
	if (m_pUIDoc)
	{
		if (m_pUIDoc->GetMasterFrame())
			m_pUIDoc->GetMasterFrame()->SendMessage(WM_CLOSE);
		m_pUIDoc = NULL;
	}
}

//-----------------------------------------------------------------------------
CBusinessServiceProviderObj* CBusinessServiceProviderObj::GetBSP(CWnd* pView)
{ 
	if (!pView)
	{
		ASSERT_TRACE(FALSE,"Invalid NULL view");
		return NULL;
	}

	if (pView->IsKindOf(RUNTIME_CLASS(CBusinessServiceProviderView)))
		return ((CBusinessServiceProviderView*)pView)->GetBSP(); 

	if (pView->IsKindOf(RUNTIME_CLASS(CBusinessServiceProviderPaneView)))
		return ((CBusinessServiceProviderPaneView*)pView)->GetBSP();

	ASSERT_TRACE(FALSE,"pView is not an CBusinessServiceProvider view");
	return NULL;
}

//-----------------------------------------------------------------------------	
void CBusinessServiceProviderObj::Init(MessagesToEvents* pMTEMap /*= NULL*/)
{
	// a questo punto il documento deve esserci
	ASSERT(m_pCallerDoc);
	
	m_pMTEMap = pMTEMap;

	AddFunctionPointers();
	// se sono stati associati degli ID a degli eventi, controlla che questi siano
	// effettivamente mappati da funzioni del BSP
	if (m_pMTEMap)
	{
		CheckEvents(m_pMTEMap->m_OnCommand);
		CheckEvents(m_pMTEMap->m_OnValueChanged);
		CheckEvents(m_pMTEMap->m_OnCtrlStateChanged);
		CheckEvents(m_pMTEMap->m_OnUpdateCommandUI);
		CheckEvents(m_pMTEMap->m_OnCtrlTreeviewadvSelectionChanged);
		CheckEvents(m_pMTEMap->m_OnCtrlTreeviewadvItemDrag);
		CheckEvents(m_pMTEMap->m_OnCtrlTreeviewadvDragOver);
		CheckEvents(m_pMTEMap->m_OnCtrlTreeviewadvDragDrop);
		CheckEvents(m_pMTEMap->m_OnCtrlTreeviewadvMouseUp);
		CheckEvents(m_pMTEMap->m_OnCtrlTreeviewadvMouseDown);
		CheckEvents(m_pMTEMap->m_OnCtrlTreeviewadvMouseClick);
		CheckEvents(m_pMTEMap->m_OnCtrlTreeviewadvContextMenuItemClick);
		CheckEvents(m_pMTEMap->m_OnCtrlTreeviewadvMouseDoubleClick);
	}

	// Se il BSP ha previsto una user interface, mappa l'evento che ne consentira' l'apertura
	// associandolo al command ID del bottone aggiunto alla toolbar
	if (m_nButtonCmd != 0)
	{
		//ASSERT_TRACE(m_UIStyle == POPUP,"CBusinessServiceProviderObj::Init(): m_nButtonCmd != 0 with UIStyle different from POPUP");
		if (!m_pMTEMap)
			m_pMTEMap = new MessagesToEvents();

		m_pMTEMap->OnCommand(m_nButtonCmd, _BSP_EVENT("OnShowUI"));
	}

	if (m_UIStyle == PANE || m_UIStyle == TAB_PANE || m_UIStyle == JSON_PANE)
	{
		//ASSERT_TRACE(m_nButtonCmd == 0,"CBusinessServiceProviderObj::Init(): m_nButtonCmd == 0 with PANE or TAB_PANE UIStyle");
		// the ClientDoc is mandatory to add the pane in the customize event
		EnableMessageRouting();
	}

	// Se ho mappato qualche evento (richiesto da fuori o fatto per gestire la UI), ho bisogno del routing
	// dei messaggi
	if (m_pMTEMap)
		EnableMessageRouting();

	// fa partire il clientDoc solo se ho bisogno del routing dei messaggi
	if (m_bMessageRoutingEnabled)
	{
		// Passa al client doc la mappa id-eventi, sara' lui a gestirla e a distruggerla
		// I dati per fare partire la UI (bottone, command, ecc.) vengono passati solo se c'e' l'ID del bottone
		// che fa partire la UI
		// icon namespace, will add the button in the Customize event
		m_pClientDoc = new CBusinessServiceProviderClientDoc(this, m_pMTEMap);

		// costruisce un namespace fittizio per il ClientDoc, a partire da quello del BSP
		CString strClientDocNamespace = m_strNamespace + _T("ClientDoc");
		m_pClientDoc->m_Namespace.AutoCompleteNamespace(CTBNamespace::DOCUMENT, strClientDocNamespace, m_pClientDoc->m_Namespace);
		m_pCallerDoc->m_pClientDocs->Add(m_pClientDoc,FALSE);
		// si registra anche come EventManager del client doc creato, in modo da poter intercettare eventuali 
		// eventi (TBEvent) scatenati direttamente dal documento
		m_pClientDoc->Attach(this);
		m_pClientDoc->Attach(m_pCallerDoc);
		m_pClientDoc->SetDocAccel(m_nDocAccelIDR);
	}
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::AddButton
	(
				UINT		nCommandID,
		const	CString&	sButtonName,
		const	CString&	sButtonCaption,
		const	CString&	sButtonIcon,
		const	CString&	sToolbarName /*= _T("")*/,
		const	CString&	sButtonTooltip /*= _T("")*/,
				BOOL		bDropDown /*= FALSE*/
	)
{
	m_UIStyle = POPUP;
	m_pClientDoc->AddButton(nCommandID, sButtonName, sButtonIcon, sButtonCaption, sToolbarName.IsEmpty() ? szToolbarNameMain : sToolbarName, sButtonTooltip, bDropDown);
}
//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::AddDropdownMenuItem(UINT nCommandID, UINT_PTR nIDNewItem, const CString& sNewItem, const CString& sToolBarName)
{
	m_pClientDoc->AddDropdownMenuItem(nCommandID, nIDNewItem, sNewItem, sToolBarName);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::AddTabDialog
	(
					UINT			nTabDlgID, 
					CRuntimeClass*	pTabDlgClass, 
			const	CString&		strTabMngRTCName, 
					int				nOrdPos/*=- 1*/,
					UINT			nBeforeIDD/*= 0*/
	)
{
	if (!m_pClientDoc)
		return;

	m_pClientDoc->AddTabDialog(nTabDlgID, pTabDlgClass, strTabMngRTCName, nOrdPos, nBeforeIDD);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::AddTileGroup
(
			CRuntimeClass*  pTileGroupClass,
			CString			sNameTileGroup,
			CString			sTitleTileGroup,
			UINT			nTileGroupID,
			CString			sTileGroupImage,
			CString			sTooltip,
			int				nOrdPos,
			UINT			nBeforeIDD,
	const	CString&		strTileMngRTName
)
{
	m_pClientDoc->m_TileGroups.Add
		(
			new CClientDocTileGroup
			(
				pTileGroupClass,
				sNameTileGroup,
				sTitleTileGroup,
				sTileGroupImage,
				sTooltip,
				nTileGroupID,
				strTileMngRTName
			)
		);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::AddTileDialog
(
	UINT			nTileDlgID,
	CRuntimeClass*	pTileDlgClass,
	const	CString&		strTileDlgTitle,
	TileDialogSize	aTileSize,
	CRuntimeClass*	pTileGroupClass,
	UINT			nBeforeIDD /*= 0*/,
	int				nFlex /*=  -1*/
	)
{
	m_pClientDoc->m_TileDialogs.Add
		(
			new CClientDocTileDialog
			(
			nTileDlgID,
			pTileDlgClass,
			strTileDlgTitle,
			aTileSize,
			nFlex,
			pTileGroupClass,
			nBeforeIDD
			)
		);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::AddTileDialog
(
	UINT			nTileDlgID,
	CRuntimeClass*	pTileDlgClass,
	const	CString&		strTileDlgTitle,
	TileDialogSize	aTileSize,
	UINT			nTileGroupIDC,
	UINT			nBeforeIDD /*= 0*/,
	int				nFlex /*=  -1*/
	)
{
	m_pClientDoc->m_TileDialogs.Add
		(
			new CClientDocTileDialog
			(
			nTileDlgID,
			pTileDlgClass,
			strTileDlgTitle,
			aTileSize,
			nFlex,
			nTileGroupIDC,
			nBeforeIDD
			)
		);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::OnShowUI(CParsedCtrl*)
{
	ASSERT(m_pCallerDoc);

	if (m_UIStyle != POPUP && m_UIStyle != BE_POPUP && m_UIStyle != STATUS_TILE)
		return;

	if (!m_pUIDoc)
	{
		m_pUIDoc				= NULL;
		m_bUIOpeningRequested	= FALSE;
	}

	if (!m_pUIDoc)
	{
		// non faccio niente se sto gia' elaborando una richiesta di apertura
		if (m_bUIOpeningRequested)
			return;

		// siccome la richiesta di apertura potrebbe arrivare piu' volte mentre la si sta elaborando, 
		// ad esempio a causa dei meccanismi di perdita/assegnazione del fuoco sui control, oppure per clic "nervosi"
		// dell'utente, si fa in modo da inviare il comando di apertura una sola volta
		if (m_UIOpening.IsLocked())
			return;

		if	(
				AfxGetTbCmdManager()->RunDocument
					(
						m_strNamespace, 
						szDefaultViewMode,
						FALSE,
						m_pCallerDoc,
						(LPAUXINFO)this
					)
			)
			// da questo momento sono in attesa che il documento di UI si apra e non elaboro altri messaggi
			m_bUIOpeningRequested = TRUE;

		m_UIOpening.Unlock();
	}
	else
	{
		// se ripigio sul bottone a UI gia' aperta, la porto in primo piano
		m_pUIDoc->Activate(NULL, TRUE);
	}
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::OnUIPaneSlide(CParsedCtrl* pCtrl, BOOL bSlideOut)
{
	if  ((m_UIStyle != PANE && m_UIStyle != TAB_PANE && m_UIStyle != JSON_PANE) || !m_pUIPane)
		return;

	// if already processing an opening/closing request, do nothing
	if	(m_bUIOpeningRequested == bSlideOut)
		return;

	// avoid repeatedly calling the method, as the notification of the sliding pan can occur multiple times
	if (m_UIOpening.IsLocked())
		return;
	
	// waiting for the panel to open/close, no more requests processed
	m_bUIOpeningRequested = bSlideOut;

	if (m_bUIOpeningRequested)
		OnShowUI(pCtrl);
	else
		OnUIClosed();

	m_UIOpening.Unlock();
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::OnUIClosed()
{ 
	switch (m_UIStyle)
	{
		case POPUP	:
		case BE_POPUP:
		case STATUS_TILE:
			m_pUIDoc = NULL; 		break;
		
		case PANE	:	
		case TAB_PANE :
		{
			// if not processing an opening, do nothing
			if (!m_bUIOpeningRequested)
				return;

			// avoid repeatedly calling the method, as the notification of the sliding pan can occur multiple times
			if (m_UIOpening.IsLocked())
				return;

			// waiting for the panel to close, no more requests processed
			m_bUIOpeningRequested = FALSE;

			m_UIOpening.Unlock();
		}
		break;
		
		default:	;
	}
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::UpdateUI()
{
	switch (m_UIStyle)
	{
		case POPUP :
		case BE_POPUP:
		case STATUS_TILE:
		{
			if (m_pUIDoc)
				m_pUIDoc->UpdateDataView();
		}
		break;

		case JSON_PANE:
			if (GetCallerDoc())
				GetCallerDoc()->UpdateDataView();
			break;

		case PANE :
		case TAB_PANE:
		{
			//@@TODO ottimizzare per non forzare un update di tutto il doc
			if (m_pUIPane && (m_pUIPane->IsVisible() || m_bUIOpeningRequested))
				GetCallerDoc()->UpdateDataView();
		}
		break;

		default:
		;
	}
}

//-----------------------------------------------------------------------------
BOOL CBusinessServiceProviderObj::IsUIOpened()
{ 
	switch (m_UIStyle)
	{
		case POPUP :	
		case BE_POPUP:
		case STATUS_TILE:
			return m_pUIDoc/* != NULL*/; 
		case PANE :
		case TAB_PANE :
		case JSON_PANE:
			return m_pUIPane && (m_pUIPane->IsVisible() || m_bUIOpeningRequested);
		default:		
			return FALSE;
	}
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::AttachUIDoc(CBusinessServiceProviderDoc* pUIDoc)
{
	//ASSERT_TRACE(m_UIStyle == POPUP, "no UI doc allowed without the POPUP UI Style");
	m_pUIDoc = pUIDoc;
	// una volta che il documento di UI si e' aperto, la richiesta di puo' considerarsi esaudita
	m_bUIOpeningRequested = FALSE;
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderObj::CreateUIPane()
{
	if (m_UIStyle != PANE || m_pUIPane != NULL)
		return;

	ASSERT_TRACE(m_nPaneId != 0, "m_nPaneId must be assigned to the resource ID of the pane");
	ASSERT_TRACE(m_pPaneUIViewClass != NULL, "m_pPaneUIViewClass must be assigned to the Runtime Class of the pane view");

	//add the dockable pane to the frame of the hosting document
	CMasterFrame* pFrame = (CMasterFrame*) m_pCallerDoc->GetFrame();

	if (pFrame)
	{
		if (m_strTabPaneMainCaption == _T(""))
			m_strTabPaneMainCaption = m_strPaneCaption;

		m_pUIPane = new CBusinessServiceProviderDockPane(m_pPaneUIViewClass, m_strTabPaneMainCaption);
		m_pUIPane->AttachBSP(this);
		
		if (pFrame->CreateDockingPane(m_pUIPane, m_nPaneId, m_strPaneName, m_strPaneCaption, CBRS_ALIGN_RIGHT, CSize(m_initCX, m_initCY)))
			m_pUIPane->SetAutoHideMode(TRUE, CBRS_ALIGN_RIGHT | CBRS_HIDE_INPLACE);
	}
}

//-----------------------------------------------------------------------------
CView* CBusinessServiceProviderObj::GetActiveView()
{
	if (m_pUIDoc)
		return m_pUIDoc->GetMasterFrame()->GetActiveView();
	else
		return NULL;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CBusinessServiceProviderObj::GetFocusedCtrl(BOOL bUIEnabled /*= TRUE*/)
{
	CParsedCtrl* pCtrl = m_pClientDoc->GetFocusedCtrl();

	if (bUIEnabled)
		if	(
				pCtrl &&
				m_pClientDoc &&
				m_pClientDoc->IsUIEnabledWhenFocused(pCtrl->GetCtrlID())
			)
			return pCtrl;
		else
			return NULL;

	return pCtrl;
}

//-----------------------------------------------------------------------------	
void CBusinessServiceProviderObj::CheckEvents(IDToEvent& aMap)
{
	POSITION p = aMap.GetStartPosition();
	int		nKey;
	MappedEvent* pEvent;
	while (p != NULL)
	{
		aMap.GetNextAssoc(p,nKey,pEvent);
		if (pEvent && !ExistAction(pEvent->m_strEvent, &pEvent->m_pFunction, &pEvent->m_pObj))
		{
			ASSERT(FALSE);
			TRACE1("L'evento %s non esiste", (LPCTSTR)pEvent->m_strEvent);
		}
	}
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CBusinessServiceProviderObj::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP1(dc,"\n",GetRuntimeClass()->m_lpszClassName);
}

void CBusinessServiceProviderObj::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

//////////////////////////////////////////////////////////////////////////////
//						MessagesToEvents									//
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(MessagesToEvents, CObject)

//-----------------------------------------------------------------------------
MessagesToEvents::~MessagesToEvents()
{
	ClearIdToEvent(m_OnValueChanged);
	ClearIdToEvent(m_OnCommand);
	ClearIdToEvent(m_OnUpdateCommandUI);
	ClearIdToEvent(m_Enabled);
	ClearIdToEvent(m_OnCtrlStateChanged);

	// spostati dalla classe TreeView 
	ClearIdToEvent(m_OnCtrlTreeviewadvSelectionChanged);
	ClearIdToEvent(m_OnCtrlTreeviewadvItemDrag);
	ClearIdToEvent(m_OnCtrlTreeviewadvDragOver);
	ClearIdToEvent(m_OnCtrlTreeviewadvDragDrop);
	ClearIdToEvent(m_OnCtrlTreeviewadvMouseUp);
	ClearIdToEvent(m_OnCtrlTreeviewadvMouseDown);
	ClearIdToEvent(m_OnCtrlTreeviewadvMouseClick);
	ClearIdToEvent(m_OnCtrlTreeviewadvContextMenuItemClick);
	ClearIdToEvent(m_OnCtrlTreeviewadvMouseDoubleClick);
}

//-----------------------------------------------------------------------------
BOOL MessagesToEvents::HasEvent(IDToEvent& idToEvent, int nID, MappedEvent*& pEvent)
{ 
	if (idToEvent.Lookup(nID,pEvent))
		return TRUE;
	else
		return FALSE; 
}

//-----------------------------------------------------------------------------
BOOL MessagesToEvents::HasEvent(IDToEvent& idToEvent, int nID, CString& strEvent)
{ 
	MappedEvent* pEvent;
	if (HasEvent(idToEvent, nID, pEvent))
	{
		strEvent = pEvent->m_strEvent;
		return TRUE;
	}
	else
		return FALSE; 
}

//-----------------------------------------------------------------------------
void MessagesToEvents::ClearIdToEvent(IDToEvent& idToEvent)
{
	POSITION p = idToEvent.GetStartPosition();
	int		nKey;
	MappedEvent* pEvent;
	while (p != NULL)
	{
		idToEvent.GetNextAssoc(p,nKey,pEvent);
		if (pEvent)
			SAFE_DELETE(pEvent);
	}
}
//-----------------------------------------------------------------------------
void MessagesToEvents::OnValueChanged(int nID, const CString& strEvent)
{
	MappedEvent* pExisting = NULL;
	ASSERT(!m_OnValueChanged.Lookup(nID, pExisting));
	m_OnValueChanged.SetAt(nID, new MappedEvent(strEvent)); 
}
