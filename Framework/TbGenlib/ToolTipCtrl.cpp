#include "stdafx.h"

#include <atlimage.h>

#include <TbNameSolver\PathFinder.h>
#include <TbGenlib\ParsObjManaged.h>
#include <TbGenlib\ToolTipCtrl.h>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//					CTBToolTipCtrl
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CTBToolTipCtrl, CBCGPToolTipCtrl)

BEGIN_MESSAGE_MAP(CTBToolTipCtrl, CBCGPToolTipCtrl)
	//{{AFX_MSG_MAP(CRibbonTooltipCtrl)
	ON_WM_PAINT()
	ON_WM_CREATE()
	//}}AFX_MSG_MAP
	ON_NOTIFY_REFLECT(TTN_SHOW, OnShow)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------
CTBToolTipCtrl::CTBToolTipCtrl()
{
	m_pParentMenuBar	= NULL;
	m_pParentRibbon		= NULL;
	m_pParentCWND		= NULL;
	/*m_pTreeViewAdvCtrl	= NULL;*/
	m_bShowIcons		= FALSE;
	m_Params.m_bDrawIcon = m_bShowIcons;
	m_TextToolTip = _T("");
}

//-------------------------------------------------------------------------------------
CTBToolTipCtrl::~CTBToolTipCtrl()
{
	for( POSITION pos = m_iconsList.GetHeadPosition(); pos != NULL; )
	{
		CIconList pIconList = m_iconsList.GetNext( pos );
		::DestroyIcon (pIconList.GetIcon());
	}
}

//-------------------------------------------------------------------------------------
void CTBToolTipCtrl::ShowIcons(BOOL bShow /*= TRUE*/) 
{ 
	m_bShowIcons = bShow;
}; 

//-------------------------------------------------------------------------------------
void CTBToolTipCtrl::OnPaint() 
{
	m_Params.m_bDrawIcon = m_bShowIcons;

	__super::OnPaint();
}

//-------------------------------------------------------------------------------------
void CTBToolTipCtrl::OnShow(NMHDR* pNMHDR, LRESULT* pResult)
{
	m_nID = 0;
	CPoint point;
	::GetCursorPos (&point);

	if (m_pParentRibbon != NULL)
	{
		ASSERT_VALID (m_pParentRibbon);

		m_pParentRibbon->ScreenToClient (&point);

		CBCGPBaseRibbonElement* pHit = m_pParentRibbon->HitTest (point, TRUE);

		if (pHit != NULL)
		{
			ASSERT_VALID (pHit);
			m_nID = pHit->GetID ();
		}
	}
	else if (m_pParentMenuBar != NULL)
	{
		ASSERT_VALID (m_pParentMenuBar);

		m_pParentMenuBar->ScreenToClient (&point);
		
		CBCGPBaseRibbonElement* pHit = m_pParentMenuBar->HitTest (point);

		if (pHit != NULL)
		{
			ASSERT_VALID (pHit);
			m_nID = pHit->GetID ();
		}
	}
	else
	{
		if (m_pParentCWND)
		{
			CTreeViewAdvCtrl* pParentTree = DYNAMIC_DOWNCAST(CTreeViewAdvCtrl, m_pParentCWND);
			if (pParentTree)
			{
				pParentTree->ScreenToClient(&point);
				m_TextToolTip = pParentTree->GetKeyByPosition(point);
			}
		}
	}

	CBCGPToolTipCtrl::OnShow (pNMHDR, pResult);
}

//-------------------------------------------------------------------------------------
void CTBToolTipCtrl::SetCWND(CWnd* pWnd)
{
	m_pParentCWND = pWnd;
}

//-------------------------------------------------------------------------------------
int CTBToolTipCtrl::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
	if (CBCGPToolTipCtrl::OnCreate(lpCreateStruct) == -1)
		return -1;

	m_pParentRibbon = DYNAMIC_DOWNCAST (CBCGPRibbonBar, 
		CWnd::FromHandlePermanent (lpCreateStruct->hwndParent));

	m_pParentMenuBar = DYNAMIC_DOWNCAST (CBCGPRibbonPanelMenuBar,
		CWnd::FromHandlePermanent (lpCreateStruct->hwndParent));
	return 0;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolTipCtrl::AddIcon(HICON hImg, UINT nID, LPCTSTR lpszText)
{
	if (hImg && nID > 0)
	{
		if (lpszText)
			m_iconsList.AddTail(CIconList(hImg, nID, lpszText));
		else
			m_iconsList.AddTail(CIconList(hImg, nID));
	}
	else
		return FALSE;
	return TRUE;
}

//-------------------------------------------------------------------------------------
HICON	CTBToolTipCtrl::GetIconById(UINT nID)
{
	 for( POSITION pos = m_iconsList.GetHeadPosition(); pos != NULL; )
	 {
		CIconList pIconList = m_iconsList.GetNext( pos );
		if (pIconList.GetId() == nID)
			return pIconList.GetIcon();
	 }
	 return NULL;
}

//-------------------------------------------------------------------------------------
CString	CTBToolTipCtrl::GetTextById(UINT nID)
{
	 for( POSITION pos = m_iconsList.GetHeadPosition(); pos != NULL; )
	 {
		CIconList pIconList = m_iconsList.GetNext( pos );
		if (pIconList.GetId() == nID)
			return pIconList.GetText();
	 }
	 return _T("");
}

//-------------------------------------------------------------------------------------
CString CTBToolTipCtrl::GetLabel ()
{
	if (m_pParentCWND)
	{
		CTreeViewAdvCtrl* pParentTree = DYNAMIC_DOWNCAST(CTreeViewAdvCtrl, m_pParentCWND);
		if (pParentTree)
		{
			return 	m_TextToolTip;
		}
	}

	if(!m_pHotButton)
	{
		return __super::GetLabel();
	}

	ASSERT_VALID(m_pHotButton);

	if (m_pHotButton->m_nID == 0 || 
		m_pHotButton->m_nID == (UINT)-1)
	{
		return __super::GetLabel ();
	}

	CString strText = GetTextById(m_pHotButton->m_nID);

	if (strText.IsEmpty())
		return __super::GetLabel ();
	
	return strText;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolTipCtrl::OnDrawIcon (CDC* pDC, CRect rectImage)
{
	if (!m_bShowIcons)
		return FALSE;

	if (m_pHotButton == NULL)
	{
		return FALSE;
	}

	ASSERT_VALID (m_pHotButton);

	if (m_pHotButton->m_nID == 0 || 
		m_pHotButton->m_nID == (UINT)-1)
	{
		return FALSE;
	}

	ASSERT_VALID (pDC);

	HICON m_hIcon = GetIconById(m_pHotButton->m_nID);

	if (!m_hIcon)
	{
		int nImg = m_pHotButton->GetImage() ;
		if (nImg >= 0 && m_pToolBarImages != NULL)
		{
			m_hIcon = m_pToolBarImages->ExtractIcon(nImg);
		}
		else
			return FALSE;
	}

	::DrawIconEx (pDC->GetSafeHdc (),
		rectImage.left, rectImage.top, m_hIcon,
		0, 0, 0, NULL, DI_NORMAL);

	return TRUE;
}
