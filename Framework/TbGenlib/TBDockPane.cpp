#include "stdafx.h"

#include <TbNameSolver\TbNamespaces.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "parsobj.h"
#include "BaseDoc.h"
#include "OslBaseInterface.h"
#include "TBDockPane.h"
#include "TBToolBar.h"



//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
#define minWidth ScalePix(400)

/////////////////////////////////////////////////////////////////////////////
//						CTaskBuilderDockPaneTabs 
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CTaskBuilderDockPaneTabs, CTaskBuilderTabWnd)

//-----------------------------------------------------------------------------
CTaskBuilderDockPaneTabs::CTaskBuilderDockPaneTabs()
	:
	m_pParentPane(NULL)
{
	AutoDestroyWindow(FALSE);
	SetHoveringBkgColor(AfxGetThemeManager()->GetDockPaneTabberTabHoveringBkgColor());
	SetHoveringForeColor(AfxGetThemeManager()->GetDockPaneTabberTabHoveringForeColor());
}

//-----------------------------------------------------------------------------
CTaskBuilderDockPaneTabs::~CTaskBuilderDockPaneTabs()
{
	for (int i=m_arTabsOSLInfos.GetCount() -1; i >= 0; i--)
	{
		CInfoOSLButton* pInfo = m_arTabsOSLInfos.GetAt(i);
		SAFE_DELETE(pInfo);
		m_arTabsOSLInfos.RemoveAt(i);
	}
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPaneTabs::AttachTabOSLInfo(CInfoOSL* pParent, UINT nID, const CString& sName)
{
	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::FORM, sName, pParent->m_Namespace);

	CInfoOSLButton* pInfo = new CInfoOSLButton(nID, sName);
	pInfo->m_pParent = pParent;
	pInfo->SetType(OSLType_TabDialog);
	pInfo->m_Namespace = aNs;
	m_arTabsOSLInfos.Add(pInfo);

	if (!aNs.IsEmpty() && aNs.IsValid())
		AfxGetSecurityInterface()->GetObjectGrant(pInfo);
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPaneTabs::Create (CTaskBuilderDockPane* pParent)
{
	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::FORM, _T("Tabber"), pParent->GetInfoOSL()->m_Namespace);

	CTaskBuilderTabWnd::Style style = AfxGetThemeManager()->UseFlatStyle() ? CTaskBuilderTabWnd::STYLE_FLAT : CTaskBuilderTabWnd::STYLE_3D;
	if (!__super::Create (style, CRect(), pParent, AfxGetTBResourcesMap()->GetTbResourceID(aNs.ToString(), TbControls)))
	{
		ASSERT_TRACE(FALSE, "CTaskBuilderDockPane::AddTab Failed to create tab window!\n");
		return FALSE;
	}
	m_pParentPane = pParent;
	TBThemeManager* pThemeManager = AfxGetThemeManager();

	SetActiveTabColor(pThemeManager->GetDockPaneTabberTabSelectedBkgColor());
	SetActiveTabTextColor(pThemeManager->GetDockPaneTabberTabSelectedForeColor());


	return TRUE;
}

//------------------------------------------------------------------------------
COLORREF CTaskBuilderDockPaneTabs::GetTabBkColor(int iTab) const
{
	return AfxGetThemeManager()->GetDockPaneTabberTabBkgColor();
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPaneTabs::AddTab (CWnd* pWnd, LPCTSTR szName, LPCTSTR szTitle, UINT nImage, BOOL bDetachable)
{
	__super::AddTab(pWnd, szTitle, nImage, FALSE);
	AttachTabOSLInfo(m_pParentPane->GetInfoOSL(),  pWnd->GetDlgCtrlID(), szName);
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPaneTabs::RemoveTabOf	(CWnd* pWnd)
{
	int idx = GetTabIndexOf(pWnd);
	if (idx < 0)
		return FALSE;
	
	__super::RemoveTab(idx);

	for (int i=m_arTabsOSLInfos.GetUpperBound(); i >=0; i--)
	{
		CInfoOSLButton* pInfo = m_arTabsOSLInfos.GetAt(i);
		if (pWnd->GetDlgCtrlID() == pInfo->m_nID)
		{
			delete pInfo;
			m_arTabsOSLInfos.RemoveAt(i);
			break;
		}
	}
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//					CTaskBuilderDockPaneCaptionButton
/////////////////////////////////////////////////////////////////////////////
const int nCaptionButtonImageSize = 16;
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTaskBuilderDockPaneCaptionButton, CBCGPCaptionButton)
//-----------------------------------------------------------------------------
CTaskBuilderDockPaneCaptionButton::CTaskBuilderDockPaneCaptionButton()
{
}

//-----------------------------------------------------------------------------
CTaskBuilderDockPaneCaptionButton::CTaskBuilderDockPaneCaptionButton(const CString& sName, UINT nID, BOOL bLeftAlign /*FALSE*/, int btnImageSize /*= 16*/)
	:
	CBCGPCaptionButton(nID, bLeftAlign),
	m_sName(sName)	
	
{
	if (HasCustomIcon())
	{	
		HICON hIcon = ::LoadWalkIcon(nID);
		if (hIcon)
		{
			CSize sizeImage(btnImageSize, btnImageSize);
			m_CustomImages.SetImageSize(sizeImage);
			m_CustomImages.AddIcon(hIcon);
		}
	}
}

//-----------------------------------------------------------------------------
CTaskBuilderDockPaneCaptionButton::CTaskBuilderDockPaneCaptionButton(const CString& sName, const CString& sImageNameSpace, BOOL bLeftAlign /*FALSE*/, int btnImageSize /*= 16*/)
	:
	CBCGPCaptionButton(HTNOWHERE, bLeftAlign),
	m_sName(sName),
	m_sImageNameSpace(sImageNameSpace)
	{
	if (HasCustomIcon())
	{	
		HICON hIcon = TBLoadImage(sImageNameSpace, NULL, btnImageSize);
		if (hIcon)
			m_CustomImages.AddIcon(hIcon);
	}
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPaneCaptionButton::SetTooltip(const CString& strTooltip)
{
	m_sTooltip = strTooltip;
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPaneCaptionButton::GetCustomToolTip(CString& strTipText)
{
	if (m_sTooltip.IsEmpty())
		return FALSE;
	
	strTipText = m_sTooltip;
	return TRUE;
}

//-----------------------------------------------------------------------------
const CString& CTaskBuilderDockPaneCaptionButton::GetName() const
{
	return m_sName;
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPaneCaptionButton::OnDraw (CDC* pDC, BOOL bActive, BOOL bHorz, BOOL bMaximized, BOOL bDisabled)
{
	__super::OnDraw (pDC, bActive, bHorz, bMaximized, bDisabled);
	if (!m_bHidden && m_CustomImages.GetCount())
	{
		CBCGPDrawState ds;
		
		if (m_CustomImages.PrepareDrawImage (ds, nCaptionButtonImageSize))
		{
			CRect rc = GetRect ();
			CSize sizeImage = m_CustomImages.GetImageSize();
			CPoint ptImage (rc.left + (rc.Width () - sizeImage.cx) / 2, rc.top + (rc.Height () - sizeImage.cy) / 2);
			m_CustomImages.Draw (pDC, ptImage.x, ptImage.y, 0);
			m_CustomImages.EndDrawImage (ds);
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPaneCaptionButton::HasCustomIcon () const
{
	return (GetHit() > CBCGPMenuImages::IdMinimizeRibbon);
}

/////////////////////////////////////////////////////////////////////////////
//					CTaskBuilderDockPaneForm
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTaskBuilderDockPaneForm, CTaskBuilderDockPane)

//-----------------------------------------------------------------------------
CTaskBuilderDockPaneForm::CTaskBuilderDockPaneForm(CRuntimeClass* pWndClass, 
	const CString& sTabPaneTitle /*_T("")*/, 
	CObject* pContext /*NULL*/, 
	CCreateContext* pCreateContext /*= NULL*/)
	:
	m_pForm(NULL),
	m_sFormTabPaneTitle(sTabPaneTitle),
	m_pContext (pContext),
	m_bValid(TRUE), 
	m_pCreateContext(pCreateContext)
{
	CObject* pObject = pWndClass->CreateObject();
	if (!pObject)
	{
		ASSERT_TRACE1(FALSE, "Cannot create pWndClass class %s!", LPCTSTR(CString(pWndClass->m_lpszClassName)));
		return;
	}

	CParsedForm* pForm = dynamic_cast<CParsedForm*>(pObject);
	if (!pForm)
	{
		TRACE1("pWndClass %s IS NOT a CParsedForm derived class!", LPCTSTR(CString(pWndClass->m_lpszClassName)));
	}

	m_pForm = pForm;
}
//-----------------------------------------------------------------------------
CTaskBuilderDockPaneForm::CTaskBuilderDockPaneForm(CParsedForm* pForm, 
	const CString& sTabPaneTitle /*_T("")*/, 
	CObject* pContext /*NULL*/,
	CCreateContext* pCreateContext /*= NULL*/)
	:
	m_pForm(pForm),
	m_sFormTabPaneTitle(sTabPaneTitle),
	m_pContext(pContext),
	m_bValid(TRUE),
	m_pCreateContext(pCreateContext)
{
}
//-----------------------------------------------------------------------------
CTaskBuilderDockPaneForm::~CTaskBuilderDockPaneForm()
{
	delete m_pCreateContext;

	if (m_pForm)
	{
		CWnd* pWnd = m_pForm->GetFormCWnd();
		ASSERT_VALID(pWnd);
		BOOL bDialog = pWnd->IsKindOf(RUNTIME_CLASS(CParsedDialog));
		if (pWnd->m_hWnd)
			pWnd->DestroyWindow();
		if (bDialog)
			SAFE_DELETE(pWnd);
	}
}

//-----------------------------------------------------------------------------
CWnd* CTaskBuilderDockPaneForm::GetWnd()
{
	return m_pForm ? m_pForm->GetFormCWnd() : NULL;
}

//-----------------------------------------------------------------------------
inline BOOL  CTaskBuilderDockPaneForm::IsValid()
{
	return m_bValid;
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPaneForm::Create(CTaskBuilderDockPane* pPane, BOOL bCallOnInitialUpdate, CSize aSize, CCreateContext* pCreateContext)
{
	if (!m_pForm || !pCreateContext || !m_pForm->GetFormCWnd())
		return TRUE;

	CBaseDocument* pDocument = dynamic_cast<CBaseDocument*>(pCreateContext->m_pCurrentDoc);

	CTaskBuilderDockPaneTabs* pParentTabber = pPane->UseTabber() ? pParentTabber = pPane->GetTabber() : NULL;
	CWnd* pRealParent = pParentTabber;
	if (!pParentTabber)
		pRealParent = pPane;

	m_pForm->OnAttachContext(pPane, m_pContext);

	if (m_pForm->GetFormCWnd()->IsKindOf(RUNTIME_CLASS(CBaseFormView)))
	{
		CBaseFormView* pView  = (CBaseFormView*) m_pForm->GetFormCWnd();
		if (!pView->Create(NULL, NULL, WS_CHILD | WS_VISIBLE, CRect (0, 0, aSize.cx, aSize.cy), pRealParent, pView->GetDialogID(), pCreateContext)) 
		{
			TRACE1("Warning: CTaskBuilderDockPane::Create couldn't create view class %s !\n", pView->GetRuntimeClass()->m_lpszClassName);
			return FALSE;
		}

		if (!OSL_CAN_DO((pView->GetInfoOSL()), OSL_GRANT_EXECUTE))
		{
			m_bValid = FALSE;
			return FALSE;
		}

		pView->SetCenterControls(FALSE);
		// send initial update to all views (and other controls) in the frame
		if (bCallOnInitialUpdate) 
			pView->SendMessage(WM_INITIALUPDATE, 0, 0);
		pView->SetBackgroundColor(pPane->m_BkgColor);

	}
	else if (m_pForm->GetFormCWnd()->IsKindOf(RUNTIME_CLASS(CView)))
	{
		CView* pView = (CView*)m_pForm->GetFormCWnd();
		if (!pView->Create(NULL, NULL, WS_CHILD | WS_VISIBLE, CRect(0, 0, aSize.cx, aSize.cy), pRealParent, 0, pCreateContext))
		{
			TRACE1("Warning: CTaskBuilderDockPane::Create couldn't create view class %s !\n", pView->GetRuntimeClass()->m_lpszClassName);
			return FALSE;
		}

		// send initial update to all views (and other controls) in the frame
		if (bCallOnInitialUpdate)
			pView->SendMessage(WM_INITIALUPDATE, 0, 0);
	}
	else if (m_pForm->GetFormCWnd()->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
	{
		CParsedDialog* pDialog = (CParsedDialog*) m_pForm->GetFormCWnd();
		pDialog->AttachDocument(pDocument);
		if (!pDialog->Create(pDialog->GetDialogID(), pRealParent, pDialog->GetFormName()))
		{
			TRACE1("Warning: CTaskBuilderDockPane::Create couldn't create dialog class %s !\n", pDialog->GetRuntimeClass()->m_lpszClassName);
			return FALSE;
		}

		if (!OSL_CAN_DO((m_pForm->GetInfoOSL()), OSL_GRANT_EXECUTE))
		{
			m_bValid = FALSE;
			return FALSE;
		}

		pDialog->SetCenterControls(FALSE);
		((CBCGPDialog*)pDialog)->SetBackgroundColor(pPane->m_BkgColor, TRUE);
		pDialog->ShowWindow(SW_SHOW);
	}

	// se devo provo ad aggiungerla nel tabpane
	if (pParentTabber)
	{
		pParentTabber->AddTab(m_pForm->GetFormCWnd(), _T("FormTabPane"), m_sFormTabPaneTitle);
		pParentTabber->AttachDocument(pDocument);
	}
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//					CTaskBuilderDockPane
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE (CTaskBuilderDockPane, CBCGPDockingControlBar)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTaskBuilderDockPane, CBCGPDockingControlBar)
	ON_WM_ERASEBKGND()
	ON_WM_WINDOWPOSCHANGED()
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTaskBuilderDockPane::CTaskBuilderDockPane()
	:
	IDisposingSourceImpl(this),
	m_bStretchOnFirstSlide(TRUE), 
	m_BtnImageSize(nCaptionButtonImageSize),	
	m_nToolbarHeight(0),
	m_pToolBar(NULL),
	m_bEnabled(TRUE),
	m_nMinWidth(minWidth),
	m_bUseTimer(TRUE)
{
}

//-----------------------------------------------------------------------------
CTaskBuilderDockPane::CTaskBuilderDockPane(CRuntimeClass* pWndClass, CString sTabPaneTitle /*_T("")*/)
	:
	IDisposingSourceImpl(this),
	m_bStretchOnFirstSlide(TRUE),
	m_bValid(TRUE),
	m_BtnImageSize(nCaptionButtonImageSize),
	m_nToolbarHeight(0),
	m_pToolBar(NULL),
	m_bEnabled(TRUE),
	m_nMinWidth(minWidth),
	m_bUseTimer(TRUE)
{
	AddForm(pWndClass, sTabPaneTitle);
}

//-----------------------------------------------------------------------------
CTaskBuilderDockPane::~CTaskBuilderDockPane()
{
	for (int i = 0; i <= m_Forms.GetUpperBound(); i++)
	{
		CTaskBuilderDockPaneForm* pForm = m_Forms.GetAt(i);
		ASSERT(pForm);
		if (pForm)
		{
			CWnd* pWnd = pForm->GetWnd();
			if (pWnd)
			{
				ASSERT_VALID(pWnd);
				m_Tabber.RemoveTabOf(pWnd);
				delete pForm;
			}
		}
	}
	m_Forms.RemoveAll();
	
	if (m_hWnd && ::IsWindow(m_hWnd))
		DestroyWindow();

	// deve essere fatta DOPO la DestroyWindow (Vedi BCGPToolBar::OnDestroy())
	if (m_pToolBar) 
	{
		ASSERT_VALID(m_pToolBar);
		m_pToolBar->SuspendLayout();
		SAFE_DELETE(m_pToolBar);
	}
}
//---------------------------------------------------------------------------
void CTaskBuilderDockPane::AdjustToolbarHeight()
{
	CRect aRect;
	m_pToolBar->GetClientRect(aRect);
	m_nToolbarHeight = max(m_pToolBar->CalcMaxButtonHeight(), aRect.Height()) + AfxGetThemeManager()->GetToolbarHighlightedHeight(); // Add px for space of cursor
}
//---------------------------------------------------------------------------
void CTaskBuilderDockPane::EnableToolbar(int nToolbarHeight /*25*/, BOOL bWithTexts /*FALSE*/)
{
	ASSERT(!m_pToolBar);

	CString sToolbarName = this->GetInfoOSL()->m_Namespace.ToUnparsedString() + _T("_Toolbar");
	m_pToolBar = new CTBToolBar();
	VERIFY (m_pToolBar->CreateEmpty(this, sToolbarName));
		
	m_pToolBar->SetBkgColor(AfxGetThemeManager()->GetDialogToolbarBkgColor());
	m_pToolBar->SetForeColor(AfxGetThemeManager()->GetDialogToolbarForeColor());

	OnCustomizeToolbar();

	m_pToolBar->EnableTextLabels(bWithTexts);
	CSize szButtons(nToolbarHeight, nToolbarHeight);

	m_pToolBar->SetSizes(szButtons, szButtons);
	m_pToolBar->ShowInDialog(this, CBRS_ALIGN_TOP);

	
	AdjustToolbarHeight();
}

//--------------------------------------------------------------------------
BOOL CTaskBuilderDockPane::PreTranslateMessage(MSG* pMsg)
{
#ifndef _OLD_PTM

	if (!GetDocument()) 
		return __super::PreTranslateMessage(pMsg);

	if (GetDocument()->m_bForwardingSysKeydownToChild)
		return FALSE;

	return (GetParent() && GetParent()->PreTranslateMessage(pMsg)) || __super::PreTranslateMessage(pMsg);

#else

	return __super::PreTranslateMessage(pMsg);

#endif
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::InitGraphics()
{
	CBCGPAutoHideToolBar::m_nShowAHWndDelay = 10;

	TBThemeManager* pThemeManager = AfxGetThemeManager();
	SetBkgColor(pThemeManager->GetDockPaneBkgColor());
	SetTitleBkgColor(pThemeManager->GetDockPaneTitleBkgColor()) ;
	SetTitleForeColor(pThemeManager->GetDockPaneTitleForeColor());
	SetTitleHoveringForeColor(pThemeManager->GetDockPaneTitleHoveringForeColor());
	SetFont(pThemeManager->GetFormFont());
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPane::IsLayoutSuspended()
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(GetParentFrame());

	if (pFrame && pFrame->IsLayoutSuspended(TRUE))
		return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::SetBkgColor(COLORREF color)
{
	m_BkgColor = color;
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::SetTitleBkgColor(COLORREF color)
{
	m_TitleBkgColor = color;
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::SetTitleForeColor(COLORREF color)
{
	m_TitleForeColor = color;
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::SetTitleHoveringForeColor(COLORREF color)
{
	m_TitleHoveringForeColor = color;
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPane::UseTabber()
{
	int nValidForm = 0;
	for (int i = 0; i < m_Forms.GetSize(); i++)
	{
		CTaskBuilderDockPaneForm* pForm = m_Forms.GetAt(i);
		if (pForm->m_bValid)
			nValidForm++;
	}

	return nValidForm > 1;
}
//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::InitialUpdate()
{
	for (int i = 0; i < m_Forms.GetSize(); i++)
	{
		CTaskBuilderDockPaneForm* pForm = m_Forms.GetAt(i);
		dynamic_cast<CWnd*>(pForm->m_pForm)->SendMessage(WM_INITIALUPDATE);
	}
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::AddForm(CRuntimeClass* pWndClass, CString sTabPaneTitle /*T("")*/, CObject* pContext /*NULL*/)
{
	CTaskBuilderDockPaneForm* pForm = new CTaskBuilderDockPaneForm(pWndClass, sTabPaneTitle, pContext);
	AddForm(pForm);
}
//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::AddForm(CTaskBuilderDockPaneForm* pForm)
{
	m_Forms.Add(pForm);
}
//-----------------------------------------------------------------------------
CWnd*	CTaskBuilderDockPane::GetFormWnd(CRuntimeClass* pWndClass) const
{
      
	for (int i = 0; i <= m_Forms.GetUpperBound(); i++)
	{
		CTaskBuilderDockPaneForm* pForm = m_Forms.GetAt(i);
		if (pForm->GetWnd()->GetRuntimeClass() == pWndClass)
			return pForm->GetWnd();
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CWnd*	CTaskBuilderDockPane::GetDerivedFormWnd(CRuntimeClass* pWndClass) const
{
	for (int i = 0; i <= m_Forms.GetUpperBound(); i++)
	{
		CTaskBuilderDockPaneForm* pForm = m_Forms.GetAt(i);
		if (pForm->GetWnd()->GetRuntimeClass()->IsDerivedFrom(pWndClass))
			return pForm->GetWnd();
	}
	return NULL;
}
//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPane::CanBeClosed() const
{
	return AfxGetThemeManager()->CanCloseDockPanes();
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPane::CanFloat() const
{
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::SetMinWidth(int nMinWidth)
{
	m_nMinWidth = ScalePix(nMinWidth);
}

// mette il pane invalido se tutte le form sono invalide
//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPane::CheckPaneValidity()
{
	m_bValid = FALSE;
	
	for (int i = 0; i <= m_Forms.GetUpperBound(); i++)
	{
		CTaskBuilderDockPaneForm* pForm = m_Forms.GetAt(i);
		if (pForm->m_bValid)
		{
			m_bValid = TRUE;
			break;
		}
	}

	return m_bValid;
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPane::Create (CLocalizableFrame* pParent, UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, BOOL bCallOnInitialUpdate, CSize aSize, CCreateContext* pCreateContext /*NULL*/, DWORD dwBCGStyle, BOOL bVisible/* = TRUE*/)
{
	//int width = MulDiv(m_nTabSelectorMinWidth, GetLogPixels(), SCALING_FACTOR);

	CSize aRealSize(
		MulDiv(aSize.cx, GetLogPixels(), SCALING_FACTOR),
		MulDiv(aSize.cy, GetLogPixels(), SCALING_FACTOR));

	DWORD wStyle = WS_CHILD | WS_CLIPSIBLINGS | wAlignment;
	if (bVisible)
		wStyle |= WS_VISIBLE;

	if (!__super::Create(sTitle, pParent, CRect(0, 0, aRealSize.cx, aRealSize.cy), TRUE, nID, wStyle, CBRS_BCGP_REGULAR_TABS, dwBCGStyle))
	{
		TRACE0("Failed to create CTaskBuilderDockPane\n");
		return FALSE;
	}

	InitGraphics();

	EnableDocking(CBRS_ALIGN_ANY);
	pParent->DockControlBar(this);

	
	CBaseDocument* pDocument = NULL;
	if (pCreateContext)
	{
		pDocument = dynamic_cast<CBaseDocument*>(pCreateContext->m_pCurrentDoc);

		GetInfoOSL()->SetType(OSLType_Skip);
		GetInfoOSL()->m_pParent = pDocument->GetInfoOSL();
		GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::BARPANEL, sName, pDocument->GetNamespace());

	}
	AddForms(pDocument);
	
	aRealSize.cy -= (m_Tabber.GetTabsHeight() + m_Tabber.GetTabBorderSize() * 2);
	if (m_pToolBar)
		aRealSize.cx -= m_nToolbarHeight;

	for (int i = 0; i <= m_Forms.GetUpperBound(); i++)
	{
		CTaskBuilderDockPaneForm* pForm = m_Forms.GetAt(i);
		pForm->Create(this, bCallOnInitialUpdate, aRealSize, pForm->m_pCreateContext ? pForm->m_pCreateContext : pCreateContext);

	}
	
	if (!CheckPaneValidity())
		return FALSE;
	
	OnCustomizePane();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::ShowAutoHideButton(BOOL bShow)
{
	if (GetAutoHideButton())
		GetAutoHideButton()->ShowButton(bShow);
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::AddForms(CBaseDocument* pDocument)
{
	OnAddForms();
	if (pDocument)
	{
		m_pDocument = pDocument;

		pDocument->AddFormsOnDockPane(this);
	}
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::DoStretch (int cx, int cy)
{
	CRect aFormRect; CRect aRectToolbar;
	CParsedForm::GetCandidateRectangles(this, m_pToolBar, m_nToolbarHeight, aFormRect, aRectToolbar, cx, cy);

	CWnd* pFirstChild = this->GetWindow(GW_CHILD);
	if (pFirstChild)
	{
		if (m_pToolBar)
		{
			CWnd* pSecondChild = pFirstChild->GetNextWindow();
			if (pSecondChild)
				pSecondChild->SetWindowPos(NULL, 0, aFormRect.top, aFormRect.Width(), aFormRect.Height(), SWP_NOZORDER | SWP_NOACTIVATE);
		}
		else
			pFirstChild->SetWindowPos(NULL, 0, aFormRect.top, aFormRect.Width(), aFormRect.Height(), SWP_NOZORDER | SWP_NOACTIVATE);

	}

	// poi la toolbar 
	if (m_pToolBar)
	{
		m_pToolBar->SetWindowPos(NULL, 0, aRectToolbar.top, aRectToolbar.Width(), aRectToolbar.Height(), SWP_NOZORDER);
		m_pToolBar->RecalcLayout();
	}
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::DoStretch ()
{
	CRect aRect;
	GetClientRect(&aRect);
	DoStretch(aRect.Width() + 1, aRect.Height() + 1);
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::GetMinSize(CSize& aSize) const
{
	aSize = m_sizeMin;
	if (aSize.cx < m_nMinWidth)
		aSize.cx = m_nMinWidth;
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::OnWindowPosChanged(WINDOWPOS* lpwndpos)
{
	if (IsLayoutSuspended())
		return;

	__super::OnWindowPosChanged(lpwndpos);

	if (((lpwndpos->flags & SWP_NOSIZE) == SWP_NOSIZE))
		return;
	DoStretch(lpwndpos->cx, lpwndpos->cy);
}

//-------------------------------------------------------------------------------------
BOOL CTaskBuilderDockPane::OnEraseBkgnd(CDC* pDC)
{
	__super::OnEraseBkgnd(pDC);

	CRect rclientRect;
	GetClientRect(&rclientRect);
	CBrush aBrush(m_BkgColor);
	pDC->FillRect(&rclientRect, &aBrush);
	return TRUE;
}

//-----------------------------------------------------------------------------
CTaskBuilderDockPaneTabs* CTaskBuilderDockPane::GetTabber()
{
	if (m_Tabber.m_hWnd == NULL)
	{
		m_Tabber.Create(this);
		m_Tabber.SetTabBorderSize(0);
	}

	return &m_Tabber;
}

//-----------------------------------------------------------------------------
CTaskBuilderDockPaneForm* CTaskBuilderDockPane::GetForm(int index)
{
	return m_Forms.GetAt(index);
}
//-----------------------------------------------------------------------------
CBaseDocument* CTaskBuilderDockPane::GetDocument()
{
	if (m_pDocument) 
		return m_pDocument;

	CWnd* pParent = GetParent();

	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(pParent);
	if (!pParent || !pFrame)
		return NULL;

	return dynamic_cast<CBaseDocument*>(pFrame->GetActiveDocument());
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::RemoveCaptionButton (UINT nID)
{
	for (int i = m_arrButtons.GetUpperBound(); i >=0; i--)
	{
		CBCGPCaptionButton* pButton = m_arrButtons.GetAt(i);

		if (pButton && pButton->GetHit() == nID)
		{
			delete pButton;
			m_arrButtons.RemoveAt(i);
			break;
		}
	}
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::RemoveCaptionButton(const CString& sImageNameSpace)
{
	for (int i = m_arrButtons.GetUpperBound(); i >= 0; i--)
	{
		CTaskBuilderDockPaneCaptionButton* pButton = dynamic_cast<CTaskBuilderDockPaneCaptionButton*>(m_arrButtons.GetAt(i));

		if (pButton && pButton->GetNamespace().CompareNoCase(sImageNameSpace))
		{
			delete pButton;
			m_arrButtons.RemoveAt(i);
			break;
		}
	}
}

//-----------------------------------------------------------------------------
CTaskBuilderDockPaneCaptionButton* CTaskBuilderDockPane::AddCaptionButton(UINT nIDImage, BOOL bLeftAlign /*FALSE*/, int nPos /*-1*/)
{
	return AddCaptionButton(cwsprintf(_T("%d"), nIDImage), nIDImage, bLeftAlign, nPos);
}

//-----------------------------------------------------------------------------
CTaskBuilderDockPaneCaptionButton* CTaskBuilderDockPane::AddCaptionButton(const CString& sName, UINT nIDImage, BOOL bLeftAlign /*FALSE*/, int nPos /*-1*/)
{
	CTaskBuilderDockPaneCaptionButton* pButton = new CTaskBuilderDockPaneCaptionButton(sName, nIDImage, bLeftAlign, m_BtnImageSize);
	if (nPos >= 0 && nPos < m_arrButtons.GetSize())
		m_arrButtons.InsertAt(nPos, pButton);
	else
		m_arrButtons.Add(pButton);
	
	return pButton;
}

//-----------------------------------------------------------------------------
CTaskBuilderDockPaneCaptionButton* CTaskBuilderDockPane::AddCaptionButton(const CString& sName, const CString& sImageNameSpace, BOOL bLeftAlign /*FALSE*/, int nPos /*-1*/)
{
	CTaskBuilderDockPaneCaptionButton* pButton = new CTaskBuilderDockPaneCaptionButton(sName, sImageNameSpace, bLeftAlign, m_BtnImageSize);
	if (nPos >= 0 && nPos < m_arrButtons.GetSize())
		m_arrButtons.InsertAt(nPos, pButton);
	else
		m_arrButtons.Add(pButton);

	return pButton;
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::OnPressButtons (UINT nHit) 
{
	CTaskBuilderDockPaneCaptionButton* pButton = DYNAMIC_DOWNCAST(CTaskBuilderDockPaneCaptionButton, FindButtonByHit(nHit));
	if (pButton)
		OnCustomCaptionButtonClicked(pButton);
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::OnCustomCaptionButtonClicked (CTaskBuilderDockPaneCaptionButton* pButton)
{
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::SetUpperTabber (BOOL bValue)
{
	if (GetTabber())
		GetTabber()->SetLocation(bValue ? CTaskBuilderTabWnd::LOCATION_TOP : CTaskBuilderTabWnd::LOCATION_BOTTOM);
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::Slide(BOOL bSlideOut, BOOL bUseTimer /*TRUE*/)
{
	if (m_bEnabled)
		__super::Slide(bSlideOut, bUseTimer);
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::OnSlide (BOOL bSlideOut)
{
	__super::OnSlide(bSlideOut);
	
	if (!bSlideOut)
		return;

	// poichè gli oggetti in autohide vengono resi visibili ora
	// sono indietro del primissimo stretch relativo ai CResizableCtrl
	if (m_bStretchOnFirstSlide)
	{
		DoStretch();
		m_bStretchOnFirstSlide = FALSE;
	}
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderDockPane::CheckStopSlideCondition(BOOL bDirection)
{
	BOOL bStop = __super::CheckStopSlideCondition(bDirection);
	if (bStop && bDirection)
	{
		// durante le operazioni di sliding ci sono strane ottimizzazioni sul 
		// paint della caption del titolo per cui non tutti di repaint-a bene
		// costringo il pannello a ridisegnare bene tutta l'area del titolo
		int nCaptionHeight = GetCaptionHeight();
		CWnd* pParent = GetParent();
		if (nCaptionHeight && pParent)
		{
			CRect r;
			GetClientRect(r);
			CRect rectCaption(0, 0, r.right, nCaptionHeight);
			pParent->InvalidateRect(rectCaption);
			pParent->UpdateWindow();
		}
	}

	return bStop;
}

//-----------------------------------------------------------------------------
LRESULT CTaskBuilderDockPane::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{	
	//does nothing
	return (LRESULT)CWndObjDescription::GetDummyDescription();
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::OnUpdateCmdUI(class CFrameWnd *pTarget, int bDisableIfNoHndler)
{
	if (m_pToolBar)
	{
		for (int i = 0; i <= m_Forms.GetUpperBound(); i++)
		{
			CTaskBuilderDockPaneForm* pForm = m_Forms.GetAt(i);
			if (!pForm) continue;
			CWnd* pWnd = pForm->GetWnd();
			if (!pWnd) continue;
			INT iMenuCount = m_pToolBar->GetCount();
			for (int i = 0; i < iMenuCount; i++)
			{
				CBCGPToolbarButton* pButton = m_pToolBar->GetButton(i);
				CTBToolBarCmdUI state;
				state.m_nIndexMax = iMenuCount;
				state.m_nIndex = i;
				state.m_pOther = m_pToolBar;
				state.m_nID = pButton->m_nID;
				state.DoUpdate(pWnd, FALSE);
			}
		}
	}
	__super::OnUpdateCmdUI(pTarget, bDisableIfNoHndler);
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::SetCaptionButtons()
{
	CTaskBuilderDockPaneCaptionButton* pButton = AddCaptionButton(HTCLOSE_BCG);
	if (pButton)
		pButton->SetTooltip(_TB("Close"));
	pButton = AddCaptionButton(HTMAXBUTTON);
	if (pButton)
		pButton->SetTooltip(_TB("Pin/Unpin Auto Hide"));
	pButton = AddCaptionButton(HTMINBUTTON);
	if (pButton)
		pButton->SetTooltip(_TB("Actions"));
}

// si occupa di disabilitare gli oggetti nel pannello (nel caso siano 
// dialog o view si disabilitano per intero)
//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::EnablePane(BOOL bEnable)
{
	m_bEnabled = bEnable;

	CWnd* pWnd = GetWindow(GW_CHILD);

	while (pWnd)
	{
		pWnd->EnableWindow(bEnable);
		pWnd = pWnd->GetNextWindow();
	}
}

//-----------------------------------------------------------------------------
CBCGPAutoHideToolBar* CTaskBuilderDockPane::SetAutoHideMode(BOOL bMode, DWORD dwAlignment, CBCGPAutoHideToolBar* pCurrAutoHideBar /*NULL*/, BOOL bUseTimer /*TRUE*/)
{
	return __super::SetAutoHideMode(bMode, dwAlignment, pCurrAutoHideBar, bUseTimer && m_bUseTimer);
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::SetUseTimer(BOOL bValue)
{
	m_bUseTimer = bValue;
}

//-----------------------------------------------------------------------------
void CTaskBuilderDockPane::HidePane(BOOL bSendInAutoHide /*TRUE*/)
{
	if (bSendInAutoHide && !IsHideInAutoHideMode())
		SetAutoHideMode(FALSE, GetCurrentAlignment());
	ShowWindow(SW_HIDE);
}

/////////////////////////////////////////////////////////////////////////////
//					CDockingPanes
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CDockingPanes, CArray<CTaskBuilderDockPane*>)
//-----------------------------------------------------------------------------
CDockingPanes::~CDockingPanes()
{
	SAFE_DELETE(m_pLayout);
	SAFE_DELETE(m_pMainCreateContext);
}

//-----------------------------------------------------------------------------
CDockingPanes::CDockingPanes()
	:
	m_pLayout(NULL),
	m_pMainCreateContext(NULL),
	m_bInCreateFrame(FALSE)
{
}

//----------------------------------------------------------------------------
const BOOL&	 CDockingPanes::IsInCreateFrame() const
{
	return m_bInCreateFrame;
}


//----------------------------------------------------------------------------
CTaskBuilderDockPane* CDockingPanes::GetPane(UINT nID) const
{
	for (int i = 0; i < GetCount(); i++)
	{
		CTaskBuilderDockPane* pPane = GetAt(i);
		if (pPane->m_nID == nID)
			return pPane;
	}
	return NULL;
}
//----------------------------------------------------------------------------
void CDockingPanes::BeginOnCreateFrame (CCreateContext* pContext)
{
	ASSERT(m_pMainCreateContext == NULL);
	m_pMainCreateContext = new  CCreateContext();
	m_pMainCreateContext->m_pCurrentDoc = pContext->m_pCurrentDoc;
	m_pMainCreateContext->m_pCurrentFrame = pContext->m_pCurrentFrame;
	m_pMainCreateContext->m_pLastView = pContext->m_pLastView;
	m_pMainCreateContext->m_pNewDocTemplate = pContext->m_pNewDocTemplate;
	m_pMainCreateContext->m_pNewViewClass = pContext->m_pNewViewClass;
	m_bInCreateFrame = TRUE;
}

//----------------------------------------------------------------------------
void CDockingPanes::DestroyInvalidObjects()
{
	// per prima cosa rimuovo quelli non validati 
	// durante il processo di create
	for (int i = GetUpperBound(); i >= 0; i--)
	{
		CTaskBuilderDockPane* pPane = GetAt(i);
		// prima elimino le view da togliere
		for (int f = pPane->m_Forms.GetUpperBound(); f >= 0; f--)
		{
			CTaskBuilderDockPaneForm* pForm = pPane->m_Forms.GetAt(f);
			if (!pForm->IsValid())
			{
				pPane->m_Tabber.RemoveTabOf(pForm->GetWnd());
				pPane->m_Forms.RemoveAt(f);
				delete pForm;
			}
		}

		if (pPane->m_bValid)
			continue;

		// cancellazione del pane
		pPane->SetAutoHideMode(FALSE, pPane->GetCurrentAlignment());
		pPane->UnDockControlBar();

		RemoveAt(i);
		delete pPane;
	}
}

//----------------------------------------------------------------------------
void CDockingPanes::EndOnCreateFrame()
{
	m_bInCreateFrame = FALSE; 
	DestroyInvalidObjects();

	// eseguo un primo stretch per aggiornare i pannelli già
	// visibili che non sono soggetti allo sliding di autohide. 
	for (int i=0; i <= GetUpperBound(); i++)
	{
		CTaskBuilderDockPane* pPane = GetAt(i);
		pPane->DoStretch();
	}
}

//----------------------------------------------------------------------------
CCreateContext* CDockingPanes::GetMainCreateContenxt()
{
	return m_pMainCreateContext;
}

//----------------------------------------------------------------------------
BOOL CDockingPanes::IsDocumentInDesignMode()
{
	if (!m_pMainCreateContext)
		return FALSE;

	CBaseDocument* pDoc = dynamic_cast<CBaseDocument*>(m_pMainCreateContext->m_pCurrentDoc);
	return pDoc ? pDoc->IsInDesignMode() : FALSE;
}

//----------------------------------------------------------------------------
CTaskBuilderDockPane* CDockingPanes::CreatePane(CLocalizableFrame* pParent, CRuntimeClass* pCreateClass, UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, CSize aSize, CCreateContext* pCreateContext /*NULL*/, DWORD dwBCGStyle, BOOL bVisible /*= TRUE*/)
{
	CRuntimeClass* pPaneRTC = pCreateClass;
	if (pPaneRTC->IsDerivedFrom(RUNTIME_CLASS(CBaseFormView)) || pPaneRTC->IsDerivedFrom(RUNTIME_CLASS(CParsedDialog)))
		pPaneRTC = RUNTIME_CLASS(CTaskBuilderDockPane); 

	CTaskBuilderDockPane* pPane;

	if (pPaneRTC == pCreateClass)
		pPane = (CTaskBuilderDockPane*) pPaneRTC->CreateObject();
	else
		pPane = new CTaskBuilderDockPane(pCreateClass);

	if (!CreatePane(pParent, pPane, nID, sName, sTitle, wAlignment, aSize, pCreateContext, dwBCGStyle, bVisible))
	{
		pPane->UnDockControlBar();
		pPane = NULL;
	}
	return pPane;
}

//----------------------------------------------------------------------------
CTaskBuilderDockPane* CDockingPanes::CreatePane(CLocalizableFrame* pParent, CTaskBuilderDockPane* pPane, UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, CSize aSize, CCreateContext* pCreateContext /*NULL*/, DWORD dwBCGStyle /*dwDefaultBCGDockingBarStyle*/, BOOL bVisible/* = TRUE*/)
{
/*	if (IsDocumentInDesignMode())
		return FALSE;*/

	ASSERT(pPane);
	if (!pParent->IsKindOf(RUNTIME_CLASS(CBCGPFrameWnd)))
	{
		ASSERT_TRACE(FALSE, "Docking Pane is supported only on CBCGPFrameWnd version build!");
		return FALSE;
	}

	BOOL bOk = pPane->Create(pParent, nID, sName, sTitle, wAlignment, !m_bInCreateFrame, aSize, pCreateContext ? pCreateContext : GetMainCreateContenxt(), dwBCGStyle, bVisible);
	Add(pPane);

	return bOk ? pPane : NULL;
}

//----------------------------------------------------------------------------
void CDockingPanes::EnableDockingLayout(CRuntimeClass* pClass /*NULL*/)
{
	CRuntimeClass* pRTC = pClass;
	if (!pRTC)
		return;
	
	m_pLayout = (IDockingLayout*) pRTC->CreateObject();
}

//----------------------------------------------------------------------------
IDockingLayout::ContainerArea CDockingPanes::ToArea(DWORD wAlignment)
{
	if (
		(wAlignment & CBRS_LEFT) == CBRS_LEFT ||
		(wAlignment & CBRS_ALIGN_LEFT) == CBRS_ALIGN_LEFT
		)
		return IDockingLayout::LEFT;

	if (
		(wAlignment & CBRS_RIGHT) == CBRS_RIGHT ||
		(wAlignment & CBRS_ALIGN_RIGHT) == CBRS_ALIGN_RIGHT
		)
		return IDockingLayout::RIGHT;

	if (
		(wAlignment & CBRS_TOP) == CBRS_TOP ||
		(wAlignment & CBRS_ALIGN_TOP) == CBRS_ALIGN_TOP
		)
		return IDockingLayout::TOP;

	if (
		(wAlignment & CBRS_BOTTOM) == CBRS_BOTTOM ||
		(wAlignment & CBRS_ALIGN_BOTTOM) == CBRS_ALIGN_BOTTOM
		)
		return IDockingLayout::BOTTOM;
	
	ASSERT(FALSE);
	return IDockingLayout::LEFT;
}

//----------------------------------------------------------------------------
BOOL CDockingPanes::DestroyPane(CTaskBuilderDockPane* pPane)
{
	CTaskBuilderDockPane* pItem;
	for (int i = GetUpperBound(); i >= 0; i--)
	{
		pItem = GetAt(i);
		if (pPane == pItem)
		{
			pPane->SetAutoHideMode(FALSE, pPane->GetCurrentAlignment());
			pPane->UnDockControlBar();

			RemoveAt(i);
			delete pPane;
			return TRUE;
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void CDockingPanes::DestroyPanes()
{
	CTaskBuilderDockPane* pItem;
	for (int i = GetUpperBound(); i >= 0; i--)
	{
		pItem = GetAt(i);
		if (!pItem)
			continue;
		ASSERT_VALID(pItem);

		pItem->SetAutoHideMode(FALSE, pItem->GetCurrentAlignment());
		pItem->UnDockControlBar();

		RemoveAt(i);
		delete pItem;
	}

}
