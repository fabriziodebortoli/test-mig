#include "StdAfx.h"

#include  <afxglobals.h>
#include <TbGeneric\TBThemeManager.h>

#include "SlaveViewContainer.h"
#include "Extdoc.h"
#include "Tabber.h"

#include "EmptyView.hjson"
#include "EXTDOC.hjson" //JSON AUTOMATIC UPDATE

//=============================================================================
IMPLEMENT_DYNAMIC(CSlaveViewContainer, CAbstractFormView)

//=============================================================================
BEGIN_MESSAGE_MAP(CSlaveViewContainer, CAbstractFormView)
	ON_MESSAGE			(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
	ON_MESSAGE			(UM_GET_CONTROL_DESCRIPTION,	OnGetControlDescription)
	ON_WM_ERASEBKGND()
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CSlaveViewContainer::CSlaveViewContainer(UINT nIDC, CWnd* pParent, CBaseDocument* pDocument)
	: 
	CAbstractFormView(_T("Dynamic"), IDD_EMPTY_VIEW)
{
	SubclassDlgItem(nIDC, pParent);

	ModifyStyle		(WS_DLGFRAME|WS_THICKFRAME,			WS_BORDER);	
	ModifyStyleEx	(WS_EX_STATICEDGE|WS_EX_WINDOWEDGE, WS_EX_CLIENTEDGE);	
	ASSERT(GetStyle() & WS_BORDER);
	ASSERT(GetExStyle() & WS_EX_CLIENTEDGE);

	InitSizeInfo(this);

	m_nMapMode = MM_TEXT;

	if (pDocument)
		pDocument->AddView(this);
}

//-----------------------------------------------------------------------------
BOOL CSlaveViewContainer::PreCreateWindow(CREATESTRUCT& cs) 
{	//non viene chiamata! ... forse perchè subisce una SubClassDlgItem
	BOOL bOk = __super::PreCreateWindow(cs);

	cs.style		= cs.style		& ~(WS_BORDER|WS_DLGFRAME|WS_THICKFRAME);
	cs.dwExStyle	= cs.dwExStyle	& ~(WS_EX_STATICEDGE|WS_EX_CLIENTEDGE|WS_EX_WINDOWEDGE);

	cs.style		= cs.style		| WS_BORDER;
	cs.dwExStyle	= cs.dwExStyle	| WS_EX_CLIENTEDGE;
	return bOk;
}

//------------------------------------------------------------------------------
CSlaveViewContainer::~CSlaveViewContainer ()
{
}

//------------------------------------------------------------------------------
LRESULT CSlaveViewContainer::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	if (m_nDirStrech > 0)
	{
		DoRecalcCtrlSize();

		CalcSlaveViewSize();
	}
	return 0L;
}

//------------------------------------------------------------------------------
#define MARGIN  4

void CSlaveViewContainer::CalcSlaveViewSize()
{
	CRect r;
	GetClientRect(r);
	
	for (CWnd *pWnd = GetWindow(GW_CHILD); pWnd; pWnd = pWnd->GetWindow(GW_HWNDNEXT))
	{
		pWnd->MoveWindow(0, 0, r.Width() /*- MARGIN*/, r.Height() /*- MARGIN*/, TRUE);  
	}
}		

//-----------------------------------------------------------------------------
void CSlaveViewContainer::HideSlaveView()
{
	if (m_hWnd)
		for (CWnd *pWnd = GetWindow(GW_CHILD); pWnd; pWnd = pWnd->GetWindow(GW_HWNDNEXT))
		{
			pWnd->ShowWindow(SW_HIDE);
		}
}	

//-----------------------------------------------------------------------------
void CSlaveViewContainer::DeleteSlaveView (CWnd* pCurrent)
{
	if (::IsWindow(m_hWnd)/* && pCurrent && pCurrent->m_hWnd*/)
	{
		//ASSERT_VALID(pCurrent);
		for (CWnd *pWnd = GetWindow(GW_CHILD); pWnd && ::IsWindow(pWnd->m_hWnd); /*pWnd = pWnd->GetWindow(GW_HWNDNEXT)*/)
		{
			ASSERT_VALID(pWnd);
			ASSERT_KINDOF(CSlaveFormEmbeddedFrame, pWnd);
			//ASSERT_VALID(pCurrent->GetParent());

			if (::IsWindow(pWnd->m_hWnd)/* && ::IsWindow(pCurrent->GetParent()->m_hWnd) && pWnd->m_hWnd != pCurrent->GetParent()->m_hWnd*/)
			{
				pWnd->ShowWindow(SW_HIDE);
				CWnd* pW = pWnd->GetWindow(GW_HWNDNEXT);
				pWnd->SendMessage(WM_CLOSE);
				pWnd = pW;
			}
			else
				pWnd = pWnd->GetWindow(GW_HWNDNEXT);
		}
	}
}		

//-----------------------------------------------------------------------------
void CSlaveViewContainer::SetFocusOnActiveSlaveView ()
{
	CWnd *pWnd = GetFirstSlaveView();
	if (pWnd)
	{
		pWnd->SetActiveWindow();
		pWnd->SetFocus();
	}
}	

//-----------------------------------------------------------------------------
CView * CSlaveViewContainer::GetFirstSlaveView()
{
	if (m_hWnd)
		for (CWnd *pWnd = GetWindow(GW_CHILD); pWnd; pWnd = pWnd->GetWindow(GW_HWNDNEXT))
		{
			if (pWnd->IsWindowVisible())
			{
				if (pWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormFrame)))
					return ((CAbstractFormFrame*)pWnd)->GetActiveView();
			}
		}
	ASSERT(FALSE);
	return NULL;
}		

//-----------------------------------------------------------------------------
LRESULT CSlaveViewContainer::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*) wParam;
	
	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit). 
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CWndObjDescription* pDesc = pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndObjDescription), strId);
	pDesc->UpdateAttributes(this);
	pDesc->AddChildWindows(this);

	return (LRESULT) pDesc;
}

//=============================================================================

BOOL CSlaveViewContainer::OnEraseBkgnd(CDC* pDC)
{
	CRect rect;
    GetClientRect(rect);
	rect.InflateRect(-2, -2);
	
	pDC->FillRect(&rect, const_cast<CBrush*>(AfxGetThemeManager()->GetBackgroundColorBrush ()));

    return TRUE; //afxGlobalData.DrawParentBackground(this, pDC, rect);
}

///////////////////////////////////////////////////////////////////////////////
//				Class CSlaveViewTree Implementation				
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CSlaveViewTree, CTreeViewAdvCtrl)

//-----------------------------------------------------------------------------
void CSlaveViewTree::OnInitControl ()
{
	__super::OnInitControl();
	AddImages();
	AddControls();
	if (m_bCanHide)
		AddHidingCommand();
	
	AddResizeControl();
	SetAllowDrop(FALSE);
	SetViewContextMenu(FALSE);
	SetDragAndDropOnSameLevel(FALSE);
}
//-----------------------------------------------------------------------------
void CSlaveViewTree::LoadNodes()
{
	ASSERT(!m_bLoadingNodes);
	m_bLoadingNodes = TRUE;
	BeginWaitCursor();
	
	ClearTree();
	SetNodeStateIcon(TRUE);

	//manage ToolTip
	SetCustomToolTip(AfxGetThemeManager()->GetSlaveViewContainerTooltipBkgColor(), AfxGetThemeManager()->GetSlaveViewContainerTooltipForeColor());

	//Aggiunge nodi quadri e relative sezioni
	BOOL bOk;
	for (int i = 0; i < m_Nodes.GetSize(); i++)
	{
		CNodeInfo* pNode = (CNodeInfo*) m_Nodes.GetAt(i);

		if (pNode->m_sParentKey.IsEmpty())
		{	//Aggiunge nodo principale
			bOk = AddNode
				(
					pNode->m_sTitle, 
					pNode->m_sKey,
					GetNodeImage(pNode)
				);
			//SetStyleForSelectedNode(TRUE);
		}
		else
			bOk = InsertChild
				(
					pNode->m_sParentKey, 
					pNode->m_sTitle, 
					pNode->m_sKey,
					GetNodeImage(pNode)
				);
	}
	// seleziona il primo nodo possibile (root o primo visibile)
	SelectFirstNode();
	// invia un messaggio al parent frame in modo che simuli il click e venga caricata la view selezionata
#ifdef NEWTREE
	GetParentFrame()->PostMessageW(WM_COMMAND, (WPARAM)MAKELONG(GetDlgCtrlID(), UM_TREEVIEWADV_MOUSE_CLICK), (LPARAM)GetParentFrame()->m_hWnd);
#else
	GetParentFrame()->PostMessageW(WM_COMMAND, (WPARAM)MAKELONG(GetDlgID(), UM_TREEVIEWADV_MOUSE_CLICK), (LPARAM)GetParentFrame()->m_hWnd);
#endif
	m_sKeyCurNode.Empty();

	EndWaitCursor();
	m_bLoadingNodes = FALSE;
}

//-----------------------------------------------------------------------------
void CSlaveViewTree::SelectFirstNode()
{
	// si posiziona sulla root
	SetSelectRoot();
	if (m_bSelectFirstVisibleNode)
	{
		// si posiziona sul primo ramo in cui non ci siano rami figli
		while (HasChildrenSelectedNode())
			SetSelectFirstChild();
	}
}

//-----------------------------------------------------------------------------
CSlaveViewContainer* CSlaveViewTree::AttachContainer(UINT nIDCPlaceHolder, CWnd* pParentWnd)
{
	ASSERT(!m_pSlaveContainer);

	m_pSlaveContainer = new CSlaveViewContainer(nIDCPlaceHolder, pParentWnd ? pParentWnd : GetParent(), GetDocument());
	m_pSlaveContainer->SetCenterControls(FALSE);
	m_pSlaveContainer->ModifyStyle(0, WS_VSCROLL);
	return m_pSlaveContainer;
}

//------------------------------------------------------------------------------
BOOL CSlaveViewTree::CreateSlaveDialog(CNodeInfo* pNode)
{
	if (!m_pSlaveContainer || !pNode->m_pClassView)
		return FALSE;
	
	if (pNode->m_pView)
	{
		ASSERT_VALID(pNode->m_pView);
		return TRUE;
	}

	ASSERT_VALID(GetDocument());
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*) GetDocument();

	CTBNamespace nsParent;
	CWnd* pParent = m_pSlaveContainer->GetParent();
	CParsedForm* pForm = dynamic_cast<CParsedForm*>(pParent);
	if (pForm)
		nsParent = pForm->GetNamespace();
	else
	{
		ASSERT_TRACE(FALSE, "CSlaveViewTree called by an unsupported parent window");
		return FALSE;
	}
	//----

	CTBNamespace nsSlaveView = nsParent;
	nsSlaveView.SetChildNamespace(CTBNamespace::FORM, CString(pNode->m_pClassView->m_lpszClassName), nsParent);
	ASSERT(nsSlaveView.IsValid());

	pDoc->SetNsCurrentViewParent(nsParent);
	pDoc->SetNsCurrentRowView(nsSlaveView);

	pNode->m_pView = (CSlaveFormView*) pDoc->CreateSlaveView 
												(
													pNode->m_pClassView, 
													NULL,
													NULL,
													NULL,
													m_pSlaveContainer
												);
	ASSERT_VALID(pNode->m_pView);
	if (!pNode->m_pView)
	{
		TRACE("CSlaveViewTree fails to CreateSlaveView");
		return FALSE;
	}
	CSlaveFrame* pFrame = pNode->m_pView->GetFrame();
	//pFrame->SetOwner(m_pSlaveContainer);
	::SetWindowLong(pFrame->m_hWnd, GWL_HWNDPARENT, (LONG)m_pSlaveContainer->m_hWnd);

	pDoc->SetNsCurrentViewParent(CTBNamespace());
	pDoc->SetNsCurrentRowView(CTBNamespace());
		
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CSlaveViewTree::CallDialog(CNodeInfo* pNode)
{
	ASSERT_VALID(m_pSlaveContainer);
	ASSERT_VALID(GetDocument());
	ASSERT(pNode->m_pClassView);
	if (!m_pSlaveContainer || !pNode->m_pClassView)
		return FALSE;
	//----
	if (m_bDeleteSlaveView)
	{
		for (int i = 0; i <	this->m_Nodes.GetCount(); i++)
		{
			CNodeInfo* pN = dynamic_cast<CNodeInfo*>(this->m_Nodes.GetAt(i));
			//if (pNode != pN)
				pN->m_pView = NULL;
		}
		m_pSlaveContainer->DeleteSlaveView (NULL);
		//pNode->m_pView = NULL;
	}
	
	if (!CreateSlaveDialog(pNode))
		return FALSE;

	ASSERT_VALID(pNode->m_pView);
	ASSERT(pNode->m_pView->m_hWnd);

	m_pSlaveContainer->CalcSlaveViewSize();

	if (!m_bDeleteSlaveView)
		m_pSlaveContainer->HideSlaveView();	
	
	pNode->m_pView->GetParentFrame()->ShowWindow(SW_SHOWNORMAL);
	pNode->m_pView->UpdateWindow();//forza il ridisegno, altrimenti ci sono problemi con gli static
	GetDocument()->Activate(pNode->m_pView);

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CSlaveViewTree::CallChildDialog(CNodeInfo* pNode)
{
	return FALSE;
/*
	ASSERT_VALID(m_pSlaveContainer);
	ASSERT_VALID(pNode);
	ASSERT_VALID(GetDocument());
	if (!m_pSlaveContainer)
		return FALSE;
	//----

	m_pSlaveContainer->HideSlaveView();

	CArray<CString> arChildNodes;
	GetAllChildrenFromSelectedNode (arChildNodes);

	CRect rC;
	m_pSlaveContainer->GetClientRect(rC);
	int y (2);
	CNodeInfo* pFirstNode (NULL);
	for (int i = 0; i < arChildNodes.GetSize(); i++)
	{
		CNodeInfo* pNode = GetNodeElem(arChildNodes.GetAt(i));
		ASSERT_VALID(pNode);
		if (!pNode) 
			continue;
		if (!pNode->m_pClassView) 
			continue;

		if (!CreateSlaveDialog(pNode))
			continue;
		ASSERT_VALID(pNode->m_pView);
		ASSERT(pNode->m_pView->m_hWnd);

		if (!pFirstNode) pFirstNode = pNode;

		CSlaveFrame* pFrame = pNode->m_pView->GetFrame();

		CRect ri;
		pFrame->GetClientRect(ri);
		pFrame->MoveWindow(2, y, ri.Width(), ri.Height() , TRUE); 

		pFrame->ShowWindow(SW_SHOWNORMAL);

		y += ri.Height();
		//m_arActiveSlaves.Add(pNode->m_pView);
	}
	if (!pFirstNode)
		return FALSE;

	if (y > rC.Height())
	{
		m_pSlaveContainer->SetScrollSizes(MM_TEXT, CSize(rC.Width(), y));
		m_pSlaveContainer->SetScrollPos(SB_VERT, 0, TRUE);
	}
	else
	{
		CRect rW;
		m_pSlaveContainer->GetWindowRect(rW);

		m_pSlaveContainer->SetScrollSizes(MM_TEXT, CSize(
			rW.Width() - GetSystemMetrics(SM_CXVSCROLL) - 1,
			rW.Height() - GetSystemMetrics(SM_CYHSCROLL) - 1
			));
		m_pSlaveContainer->SetScrollPos(SB_VERT, 0, TRUE);
		m_pSlaveContainer->SetScrollPos(SB_HORZ, 0, TRUE);
		//m_pSlaveContainer->SetScrollPos(SB_BOTH, 0, TRUE);
	}

	GetDocument()->Activate(pFirstNode->m_pView);

	return TRUE;
	*/
}

//-----------------------------------------------------------------------------
CSlaveViewTree::CNodeInfo* CSlaveViewTree::GetNodeElem(const CString& sKey)
{
	for (int i = 0; i < m_Nodes.GetSize(); i++)
	{
		CNodeInfo* pNode = (CNodeInfo*) m_Nodes.GetAt(i);

		if (pNode->m_sKey.CompareNoCase(sKey) == 0)
			return pNode;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CSlaveViewTree::CNodeInfo* CSlaveViewTree::GetAllNodeElem(const CString& sKey)
{
	for (int i = 0; i < m_AllNodes.GetSize(); i++)
	{
		CNodeInfo* pNode = (CNodeInfo*) m_AllNodes.GetAt(i);

		if (pNode->m_sKey.CompareNoCase(sKey) == 0)
			return pNode;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CString CSlaveViewTree::GetNodeImage(CNodeInfo* pNode)
{
	return pNode->m_sImage;
}

//-----------------------------------------------------------------------------
void CSlaveViewTree::OnSelectionChanged()
{
	if (m_bLoadingNodes)
		return;

	BOOL bDocInEdit = (GetDocument()->GetFormMode() == CAbstractFormDoc::NEW ||
					   GetDocument()->GetFormMode() == CAbstractFormDoc::EDIT);

	CString sKey = GetSelectedNodeKey();
	// se chiave vuota o se stessa chiave esce. 
	if (sKey.IsEmpty() || sKey.CompareNoCase(m_sKeyCurNode) == 0)
		return;

	// effettua i controlli sul quadro in precedenza selezionato
	if (bDocInEdit)
	{
		BOOL bOk = OnBeforeSelectionChanged(m_sKeyCurNode);
		if (!bOk)
		{
			SelectNode(m_sKeyCurNode);
			return;		
		}

		// se il quadro non è selezionabile allora avvisa e ripristina il precedente
		if (!CanDoSelectionChanged(sKey))
		{
			SelectionChangedFailed(sKey);

			SelectNode(m_sKeyCurNode);
			return;
		}
	}

	if (!sKey.IsEmpty())
	{
		OnPrepareSelectionChanged(sKey);
	}

	//---------------------
	if (m_bLoadingNodes)
		return;

	sKey = GetSelectedNodeKey();
	if (!sKey.IsEmpty())
	{
		if (sKey.CompareNoCase(m_sKeyCurNode) == 0)
			return;

		CNodeInfo* pNode = GetNodeElem(sKey);
		ASSERT_VALID(pNode);
		if (!pNode)
		{
			return;
		}

		BOOL bBookmark = pNode->m_bIsBookmarkToParent;
		int  nBookmark = pNode->m_nBookmark;
		if (bBookmark)
		{
			sKey = pNode->m_sParentKey;
			pNode = GetNodeElem(sKey);
		}

		m_pSlaveContainer->HideSlaveView(); 

		if (pNode->m_pClassView)
		{
			CallDialog(pNode);
		}
		else if (pNode->m_bShowChilds)
		{
			CallChildDialog(pNode);
		}

		m_sKeyCurNode = sKey;

		if (bBookmark)
		{
			CView* pView = m_pSlaveContainer->GetFirstSlaveView();
			if (pView)
			{
				((CScrollView*)pView)->ScrollToPosition(CPoint(0, nBookmark));
			}
		}
	}
	else
	{
		//TODO va in ricorsione infinita
		//if (!m_sKeyCurNode.IsEmpty())
		//	SetNodeAsSelected(m_sKeyCurNode);
		//else
			m_pSlaveContainer->HideSlaveView();
	}
}

//-----------------------------------------------------------------------------
void CSlaveViewTree::RemoveContextMenu()
{
	// rimuove l'eventuale menu contestuale esistente
	for (int i = 10; i >= 0; i--)
		RemoveContextMenuItem(i);
}

//-----------------------------------------------------------------------------
void CSlaveViewTree::OnSizeChanged() 
{
	int nNewWidth = GetSize().cx;
	int offset = m_nWidth == 0 ? 0 : nNewWidth - m_nWidth;
	m_nWidth = nNewWidth;
	if (offset == 0 || !m_pSlaveContainer || !IsAnimating())
		return;
	CRect r;
	m_pSlaveContainer->GetWindowRect(r);
	m_pSlaveContainer->GetParent()->ScreenToClient(r);
	r.left += offset;
	m_pSlaveContainer->MoveWindow(r, TRUE);
	m_pSlaveContainer->OnRecalcCtrlSize(NULL, NULL);
}

//-----------------------------------------------------------------------------
void CSlaveViewTree::SetNodeImage (CString& sKey, CString& sIcon)
{
	BOOL bOldLoadingNodes = m_bLoadingNodes;
	m_bLoadingNodes = TRUE;

	SetImageNoSel(sKey, sIcon);

	m_bLoadingNodes = bOldLoadingNodes;
}
