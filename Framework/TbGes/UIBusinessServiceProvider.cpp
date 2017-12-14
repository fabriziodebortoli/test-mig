
#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <tbgenlib\TBCommandInterface.h>

#include "BusinessServiceProvider.h"
#include "UIBusinessServiceProvider.h"

#include "UIBusinessServiceProvider.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//					class CBusinessServiceProviderDoc	Implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CBusinessServiceProviderDoc, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBusinessServiceProviderDoc, CAbstractFormDoc)
	//{{AFX_MSG_MAP(CBusinessServiceProviderDoc)
		ON_COMMAND			(ID_BSP_ALWAYS_ON_FRONT,			OnAlwaysOnFront)
		ON_COMMAND			(ID_BSP_SWITCH_TO_CALLER,			OnSwitchToCaller)

		ON_UPDATE_COMMAND_UI(ID_BSP_ALWAYS_ON_FRONT,			OnUpdateAlwaysOnFront)
		ON_UPDATE_COMMAND_UI(ID_BSP_SWITCH_TO_CALLER,			OnUpdateSwitchToCaller)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBusinessServiceProviderDoc::CBusinessServiceProviderDoc()
	:                                         
	m_pBSP	(NULL)
{
	DisableFamilyClientDoc(TRUE);
	m_bBatch = TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBusinessServiceProviderDoc::OnOpenDocument(LPCTSTR pObject)
{
	if (pObject)
		m_pBSP = (CBusinessServiceProviderObj*)((DocInvocationInfo*) pObject)->m_pAuxInfo;

	// la chiamata deve arrivare per forza da un CBusinessServiceProviderObj
	ASSERT(m_pBSP);
	ASSERT(m_pBSP->IsKindOf(RUNTIME_CLASS(CBusinessServiceProviderObj)));

	UpdatePinStatus();
	if (CAbstractFormDoc::OnOpenDocument(pObject))
	{
		m_pBSP->AttachUIDoc(this);
		return TRUE;
	}
	else
		return FALSE;
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderDoc::OnCloseDocument()
{
	CBusinessServiceProviderObj* pBSP = GetBSP();
	if (pBSP)
		pBSP->OnUIClosed();
	__super::OnCloseDocument();
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderDoc::GetDataSource(CString sDataManager, CString sField, DBTObject*& pDbt, SqlRecord*& pRecord, DataObj*& pField, bool& isVirtual)
{
	__super::GetDataSource(sDataManager, sField, pDbt, pRecord, pField, isVirtual);
	if (!pField && !sField.IsEmpty())
	{
		CAbstractFormDoc* pCallerDoc = GetBSP() ? GetBSP()->GetCallerDoc() : NULL;
		if (pCallerDoc)
			pCallerDoc->GetDataSource(sDataManager, sField, pDbt, pRecord, pField, isVirtual);
	}

}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderDoc::UpdatePinStatus()
{
	CMasterFrame* pFrame = GetMasterFrame();
	if (pFrame && pFrame->GetTabbedToolBar())
	{
		CTBTabbedToolbar* pTabbedToolbar = pFrame->GetTabbedToolBar();
		ASSERT(pTabbedToolbar);
		CTBToolBar* pToolBar = pTabbedToolbar->FindToolBar(ID_BSP_ALWAYS_ON_FRONT);
		ASSERT(pToolBar);
		if (m_pBSP->m_bUIAlwaysOnFront)
			pToolBar->SetButtonInfo(ID_BSP_ALWAYS_ON_FRONT, TBBS_BUTTON, TBIcon(szIconPinned, TOOLBAR), _TB("Unpin"));
		else
			pToolBar->SetButtonInfo(ID_BSP_ALWAYS_ON_FRONT, TBBS_BUTTON, TBIcon(szIconPin, TOOLBAR), _TB("Pin"));
		pFrame->SetOwner(m_pBSP->m_bUIAlwaysOnFront ? pFrame->GetValidOwner() : NULL);
	}
}
//-----------------------------------------------------------------------------
void CBusinessServiceProviderDoc::OnAlwaysOnFront()
{
	if (!m_pBSP)
		return;

	m_pBSP->m_bUIAlwaysOnFront = !m_pBSP->m_bUIAlwaysOnFront;
	UpdatePinStatus();
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderDoc::OnSwitchToCaller()
{
	if (!m_pBSP)
		return;

	if (m_pBSP->m_bUIAlwaysOnFront)
		m_pBSP->m_bUIAlwaysOnFront = FALSE;

	GetMasterFrame()->PostMessage(WM_CLOSE);

	m_pBSP->m_pCallerDoc->Activate(NULL, TRUE);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderDoc::OnUpdateAlwaysOnFront(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_pBSP && m_pBSP->m_bEnableUIAlwaysOnFront);

	if (m_pBSP)
		pCmdUI->SetCheck(m_pBSP->m_bUIAlwaysOnFront);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderDoc::OnUpdateSwitchToCaller(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_pBSP && m_pBSP->m_bEnableSwitchToCaller);
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderDoc::OnPrepareForFind(HotKeyLinkObj* pHKL, SqlRecord* pRec)
{
	__super::OnPrepareForFind(pHKL, pRec);

	if (m_pBSP)
		m_pBSP->OnPrepareForFind(pHKL, pRec);
}

//////////////////////////////////////////////////////////////////////////////
//             class CBusinessServiceProviderView implementation			//
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(CBusinessServiceProviderView, CMasterFormView)

//-----------------------------------------------------------------------------
CBusinessServiceProviderView::CBusinessServiceProviderView
	(
		const	CString&	strName, 
				UINT		nIDTemplate,
		const	CString&	strTitle
	)
	:
	CMasterFormView	(strName,nIDTemplate),
	m_strTitle		(strTitle)
{
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderView::OnInitialUpdate()
{
	GetDocument()->SetFormTitle(m_strTitle);
	CMasterFormView::OnInitialUpdate();
}

/////////////////////////////////////////////////////////////////////////////
//			class CBusinessServiceProviderJSonView Implementation
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CBusinessServiceProviderJSonView, CJsonFormView)

//-----------------------------------------------------------------------------
CBusinessServiceProviderJSonView::CBusinessServiceProviderJSonView(UINT	nIDTemplate)
	:
	CJsonFormView(nIDTemplate)
{
}

//-----------------------------------------------------------------------------
CBusinessServiceProviderObj* CBusinessServiceProviderJSonView::GetBSP(const CString& aCDNamespace)
{
	CAbstractFormDoc* pDoc = GetDocument();
	if (pDoc->IsKindOf(RUNTIME_CLASS(CBusinessServiceProviderDoc)))
	{
		CBusinessServiceProviderObj* pBsp = ((CBusinessServiceProviderDoc*)pDoc)->GetBSP();
		if (pBsp->GetClientDoc() && pBsp->GetClientDoc()->NamespaceEquals(aCDNamespace))
			return pBsp;
		else
			pDoc = pBsp->GetCallerDoc();
	}
	CBusinessServiceProviderClientDoc* pBSPClientDoc = pDoc ? ((CBusinessServiceProviderClientDoc*)(pDoc->GetClientDoc(aCDNamespace))) : NULL;
	
	return pBSPClientDoc ? pBSPClientDoc->GetBSP() : NULL;
}

//////////////////////////////////////////////////////////////////////////////
//             class CBusinessServiceProviderPaneView implementation			//
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(CBusinessServiceProviderPaneView, CAbstractFormView)

//-----------------------------------------------------------------------------
CBusinessServiceProviderPaneView::CBusinessServiceProviderPaneView
	(
		const	CString&	strName, 
				UINT		nIDTemplate
	)
	:
	CAbstractFormView	(strName,nIDTemplate),
	m_pBSP				(NULL)
{
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderPaneView::OnAttachContext(CWnd* pParent, CObject* pContext)
{
	CBusinessServiceProviderDockPane* pPane = dynamic_cast<CBusinessServiceProviderDockPane*>(pParent);
	CBusinessServiceProviderObj*	  pBSP   = dynamic_cast<CBusinessServiceProviderObj*>(pContext);

	m_pBSP = pBSP ? pBSP : pPane->GetBSP();
}

//////////////////////////////////////////////////////////////////////////////
//             class CBusinessServiceProviderDockPane implementation		//
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CBusinessServiceProviderDockPane, CTaskBuilderDockPane)

//-----------------------------------------------------------------------------
CBusinessServiceProviderDockPane::CBusinessServiceProviderDockPane()
	:
	m_pBSP(NULL)
{
}

//-----------------------------------------------------------------------------
CBusinessServiceProviderDockPane::CBusinessServiceProviderDockPane
	(
		CRuntimeClass*	pWndClass, 
		CString			sTabPaneTitle /*=_T("")*/
	)
	:
	CTaskBuilderDockPane	(pWndClass, sTabPaneTitle),
	m_pBSP					(NULL)
{
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderDockPane::OnSlide(BOOL bSlideOut)
{
	if (!this->m_hWnd)
		return;

	__super::OnSlide(bSlideOut);
	if (m_pBSP)
		m_pBSP->OnUIPaneSlide(/*GetFocusedCtrl()*/NULL, bSlideOut);
}

//-----------------------------------------------------------------------------
BOOL CBusinessServiceProviderDockPane::CheckAutoHideCondition()
{
	return FALSE;
}

//////////////////////////////////////////////////////////////////////////////
//             class CBusinessServiceProviderFrame implementation			//
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CBusinessServiceProviderFrame, CMasterFrame)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBusinessServiceProviderFrame, CMasterFrame)
	//{{AFX_MSG_MAP(CBusinessServiceProviderFrame)
	ON_WM_CLOSE ()
	ON_WM_SIZE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBusinessServiceProviderFrame::CBusinessServiceProviderFrame()
{
	SetDockable(FALSE);
}

//-----------------------------------------------------------------------------
BOOL CBusinessServiceProviderFrame::OnCustomizeJsonToolBar()
{
	return CreateJsonToolbar(IDD_BSP_TOOLBAR);
}

//-----------------------------------------------------------------------------
BOOL CBusinessServiceProviderFrame::OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar)
{
	// Toolbar Add
	/*ASSERT(pTabbedBar);
	CTBToolBar* pToolBar = CreateEmptyToolBar(szToolbarNameMain, _TB("Main"));
	pTabbedBar->AddTab (pToolBar, TRUE, TRUE);

	pToolBar->AddButton(ID_BSP_ALWAYS_ON_FRONT,		_NS_TOOLBARBTN("AlwaysOnFront"),	TBIcon(szIconPin, TOOLBAR),		_TB("Pin"));
	pToolBar->AddButton(ID_BSP_SWITCH_TO_CALLER,	_NS_TOOLBARBTN("SwitchToCaller"),	TBIcon(szIconBack, TOOLBAR),	_TB("Back"));
	*/
	return TRUE;
}


//-----------------------------------------------------------------------------
void CBusinessServiceProviderFrame::OnSize(UINT nType, int cx, int cy)
{
	if (IsLayoutSuspended())
		return;

	__super::OnSize(nType, cx, cy);

}

//-----------------------------------------------------------------------------
BOOL CBusinessServiceProviderFrame::CreateStatusBar()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void CBusinessServiceProviderFrame::OnClose() 
{ 
	CBusinessServiceProviderDoc* pDoc = (CBusinessServiceProviderDoc*) GetActiveDocument();
	if	(
			pDoc &&
			pDoc->IsKindOf(RUNTIME_CLASS(CBusinessServiceProviderDoc)) &&
			pDoc->GetBSP()
		)
		pDoc->GetBSP()->OnUIClosed();

	// la OnClose standard fa una PostMessage, ma siccome e' importante che doc frame e view si chiudano prima
	// che il BSP venga distrutto, viene invece fatta ua SendMessage
	// CMasterFrame::OnClose();
	SendMessage(WM_COMMAND, ID_FILE_CLOSE);
}

/////////////////////////////////////////////////////////////////////////////
//					class CBSPWithBottomToolbarFrame Implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CBSPWithBottomToolbarFrame, CBusinessServiceProviderFrame)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBSPWithBottomToolbarFrame, CBusinessServiceProviderFrame)
	//{{AFX_MSG_MAP(CBSPWithBottomToolbarFrame)
	ON_WM_SIZE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBSPWithBottomToolbarFrame::CBSPWithBottomToolbarFrame()
	: 
	CBusinessServiceProviderFrame	(),
	m_pBottomToolbar				(NULL)
{
}

//-----------------------------------------------------------------------------
CBSPWithBottomToolbarFrame::~CBSPWithBottomToolbarFrame()
{
	SAFE_DELETE(m_pTabbedToolBar);
	SAFE_DELETE(m_pBottomTabbedToolbar);
}

//-----------------------------------------------------------------------------
void CBSPWithBottomToolbarFrame::OnSize(UINT nType, int cx, int cy)
{
	if (IsLayoutSuspended())
		return;

	__super::OnSize(nType, cx, cy);

}

//------------------------------------------------------------------
BOOL CBSPWithBottomToolbarFrame::CreateAuxObjects(CCreateContext* pCreateContext)
{
	// Create standart tabbed ToolBar
	m_pBottomTabbedToolbar = new CTBTabbedToolbar();
	m_pBottomTabbedToolbar->SuspendLayout();
	m_pBottomTabbedToolbar->Create(this);
	m_pBottomTabbedToolbar->EnableDocking(this, CBRS_ALIGN_BOTTOM);
	m_pBottomTabbedToolbar->SetWindowText(_TB("Bottom"));
	m_pBottomTabbedToolbar->AttachOSLInfo(((CBaseDocument*)GetDocument())->GetInfoOSL());
	SetMenu(NULL);

	// Add New ToolBar
	m_pBottomToolbar = new CTBToolBar();
	if (!m_pBottomToolbar->CreateEmpty(this, _TB("Bottom")))
	{
		TRACE("Failed to create the main toolBar.\n");
		ASSERT(FALSE);
		return FALSE;
	}
	m_pBottomTabbedToolbar->AddTab(m_pBottomToolbar);

	m_pBottomToolbar->SetBkgColor(AfxGetThemeManager()->GetDialogToolbarBkgColor());
	m_pBottomToolbar->SetForeColor(AfxGetThemeManager()->GetDialogToolbarForeColor());
	m_pBottomToolbar->SetTextColor(AfxGetThemeManager()->GetDialogToolbarTextColor());
	m_pBottomToolbar->SetTextColorHighlighted(AfxGetThemeManager()->GetDialogToolbarTextHighlightedColor());
	m_pBottomToolbar->SetHighlightedColor(AfxGetThemeManager()->GetDialogToolbarHighlightedColor());

	AddCustomButtons();
	m_pBottomToolbar->AddButtonToRight(IDCANCEL, _NS_TOOLBARBTN("Cancel"), TBIcon(szIconEscape, TOOLBAR), _TB("Cancel"));

	return TRUE;

}

/////////////////////////////////////////////////////////////////////////////
//				class CBSPOkFrame Implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CBSPOkFrame, CBSPWithBottomToolbarFrame)

//------------------------------------------------------------------
void CBSPOkFrame::AddCustomButtons()
{
	m_pBottomToolbar->AddButtonToRight(IDOK, _NS_TOOLBARBTN("Ok"), TBIcon(szIconOk, TOOLBAR), _TB("Ok"));	
	m_pBottomToolbar->SetDefaultAction(IDOK);
}

//-----------------------------------------------------------------------------
void CBSPOkFrame::OnAdjustFrameSize(CSize& size)
{
	size = CSize(350, 365);
}
