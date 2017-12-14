#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\Globals.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGenlib\BaseDoc.h>
#include <TbGenlib\Tile.h>
#include <TbGenlib\Parsobj.h>
#include <TbGenlib\BaseTileDialog.h>

#include "parsres.hjson" //JSON AUTOMATIC UPDATE

#include "TileButtons.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif
/////////////////////////////////////////////////////////////////////////////
// 							CCollapseButton
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CCollapseButton, CBCGPButton)

BEGIN_MESSAGE_MAP(CCollapseButton, CBCGPButton)
	ON_CONTROL_REFLECT_EX(BN_CLICKED, OnClicked)
	ON_MESSAGE	(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CCollapseButton::CCollapseButton() 
{
	m_bDontUseWinXPTheme = TRUE;
	m_nFlatStyle = BUTTONSTYLE_NOBORDERS;
	m_bTopImage = TRUE;
	m_clrFace = AfxGetThemeManager()->GetTileDialogTitleBkgColor();
	m_clrRegular = m_clrFace;
	m_bCollapsed = FALSE;
	m_bDrawFocus = FALSE;
	SetImageAutoScale();
	m_hBmpCollapse = NULL;
	m_hBmpExpand = NULL;
}

//-----------------------------------------------------------------------------
void CCollapseButton::SetColor(COLORREF crNewColor, BOOL bTransparent /*TRUE*/)
{
	m_bTransparent = bTransparent;
	if (m_bTransparent)
	{
		m_clrFace = crNewColor;
		m_clrRegular = crNewColor;
	}
}

//-----------------------------------------------------------------------------
CCollapseButton::~CCollapseButton() 
{
	DetachImages();
}

//-----------------------------------------------------------------------------
void CCollapseButton::DetachImages()
{
	if (m_hBmpCollapse != NULL)
		DeleteObject(m_hBmpCollapse);

	if (m_hBmpExpand != NULL)
		DeleteObject(m_hBmpExpand);
}

//-----------------------------------------------------------------------------
BOOL CCollapseButton::Create(LPCTSTR caption, DWORD dwStyle, const RECT&  rect, Tile* pOwner, UINT  nID, BOOL bCollapsed)
{
	m_pOwner = pOwner;
	if (AfxIsRemoteInterface() || AfxIsInUnattendedMode())
		return TRUE;

	BOOL bRet = __super::Create(caption, dwStyle, rect, m_pOwner->GetTileCWnd(), nID);
	
	CString sCollapseImage = AfxGetThemeManager()->GetTileCollapseImage();
	CString sExpandImage = AfxGetThemeManager()->GetTileExpandImage();
	if (sCollapseImage.IsEmpty() || sExpandImage.IsEmpty())
	{
		m_hBmpCollapse = LoadBitmapOrPng(TBIcon(szIconUp, IconSize::CONTROL));
		m_hBmpExpand = LoadBitmapOrPng(TBIcon(szIconDown, IconSize::CONTROL));
	}
	else
	{
		m_hBmpCollapse = LoadBitmapOrPng(sCollapseImage);
		m_hBmpExpand = LoadBitmapOrPng(sExpandImage);
	}
	
	ChangeIcon(bCollapsed);
	return bRet;
}

//-----------------------------------------------------------------------------
void CCollapseButton::ChangeIcon(BOOL bCollapsed)
{
	m_bCollapsed = bCollapsed;
	// set the collapse image if currently expanded, and vice-versa
	if (!m_bCollapsed)
	{
		SetBitmap(m_hBmpCollapse);
	}
	else
	{
		if (!m_hBmpExpand)
		{
			SetBitmap(m_hBmpCollapse);
		}
		else
		{
			SetBitmap(m_hBmpExpand);
		}
	}
	Invalidate();
}

//-----------------------------------------------------------------------------
BOOL CCollapseButton::OnClicked()
{
	if (!m_pOwner)
		return FALSE; // message handled by the parent window

	// Il cick sula freccina "ruba" il fuoco al control che lo ha in questo momento
	CWnd* pWnd = NULL;

	CParsedForm* pForm = GetParentForm(this);
	if (pForm)
	{
		pWnd = pForm->GetLastFocusedCtrl();
		if (pForm->GetDocument() && pForm->GetDocument()->IsInDesignMode())
			return TRUE;
	}

	CBaseTileDialog* pTileDialog = dynamic_cast<CBaseTileDialog*>(m_pOwner);
	if (pTileDialog)
		pTileDialog->m_bCollapseExpandFromClick = TRUE;

	m_pOwner->CollapseExpand();

	// lo si ridà al control se ne ha ancora diritto (qualcuno potrebbe aver cambiato durante il collapse)
	if (pWnd && pForm && pWnd->m_hWnd == pForm->GetLastFocusedCtrl()->m_hWnd)
		pWnd->SetFocus();

	return TRUE; // Parent window will not receive the message
}

//-----------------------------------------------------------------------------
void CCollapseButton::OnFillBackground(CDC* pDC, const CRect& rectClient)
{
	pDC->FillSolidRect(rectClient, m_clrFace);
}

//-----------------------------------------------------------------------------
LRESULT CCollapseButton::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	//does nothing
	return (LRESULT)CWndObjDescription::GetDummyDescription();
}


/////////////////////////////////////////////////////////////////////////////
// 							CPinButton
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CPinButton, CBCGPButton)

BEGIN_MESSAGE_MAP(CPinButton, CBCGPButton)
	ON_CONTROL_REFLECT_EX(BN_CLICKED, OnClicked)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPinButton::CPinButton() 
{
	m_bDontUseWinXPTheme = TRUE;
	m_nFlatStyle = BUTTONSTYLE_NOBORDERS;
	m_bVisualManagerStyle = TRUE;
	m_bTopImage = TRUE;

	m_bPinned = FALSE;
	m_bDrawFocus = FALSE;
	m_clrFace = AfxGetThemeManager()->GetBackgroundColor();
	m_clrRegular = m_clrFace;
	SetImageAutoScale();

	m_hBmpPin = NULL;
	m_hBmpUnpin = NULL;

}

//-----------------------------------------------------------------------------
void CPinButton::SetColor(COLORREF crNewColor, BOOL bTransparent /*FALSE*/)
{
	m_bTransparent = bTransparent;
	if (!m_bTransparent)
	{
		m_clrFace = crNewColor;
		m_clrRegular = crNewColor;
	}
}

//-----------------------------------------------------------------------------
CPinButton::~CPinButton() 
{
	DetachImages();
}

//-----------------------------------------------------------------------------
void CPinButton::DetachImages()
{
	if (m_hBmpPin != NULL)
		DeleteObject(m_hBmpPin);

	if (m_hBmpUnpin != NULL)
		DeleteObject(m_hBmpUnpin);
}

//-----------------------------------------------------------------------------
BOOL CPinButton::Create(LPCTSTR caption, DWORD dwStyle, const RECT&  rect, Tile* pOwner, UINT  nID, BOOL bPinned)
{
	m_pOwner = pOwner;

	if (AfxIsRemoteInterface() || AfxIsInUnattendedMode())
		return TRUE;

	BOOL bRet = __super::Create(caption, dwStyle, rect, m_pOwner->GetTileCWnd(), nID);
	
	CString sPinImage = TBIcon(szIconPin, IconSize::CONTROL);
	CString sUnpinImage = TBIcon(szIconPinned, IconSize::CONTROL);

	m_hBmpPin = LoadBitmapOrPng(sPinImage);
	m_hBmpUnpin = LoadBitmapOrPng(sUnpinImage);

	ASSERT(m_hBmpPin);
	ASSERT(m_hBmpUnpin);

	ChangeIcon(bPinned);
	
	return bRet;
}

//-----------------------------------------------------------------------------
void CPinButton::ChangeIcon(BOOL bPinned)
{
	m_bPinned = bPinned;
	Invalidate();
}

//-----------------------------------------------------------------------------
BOOL CPinButton::OnClicked()
{
	if (!m_pOwner)
		return FALSE; // message handled by the parent window

	m_pOwner->PinUnpin();
	return TRUE; // Parent window will not receive the message
}

//-----------------------------------------------------------------------------
void CPinButton::OnFillBackground(CDC* pDC, const CRect& rectClient)
{
	
	pDC->FillSolidRect(rectClient, m_clrFace);
}

//-----------------------------------------------------------------------------
void CPinButton::DoDrawItem(CDC* pDCPaint, CRect rectClient, UINT itemState)
{
	CBCGPMemDC memDC(*pDCPaint, this);
	CDC* pDC = &memDC.GetDC();

	pDC->FillSolidRect(rectClient, m_clrFace);

	//---------------------
	// Draw button content:
	//---------------------
	OnDraw(pDC, rectClient, itemState);

	ReleaseDC(pDC);
}

//-----------------------------------------------------------------------------
void CPinButton::OnDraw(CDC* pDC, const CRect& rect, UINT uiState)
{
	CPoint center;
	center.x = rect.left + (rect.Width() / 2);
	center.y = rect.top + (rect.Height() / 2);

	HBITMAP hBITMAP = NULL;
	if (m_bPinned)
		hBITMAP = m_hBmpUnpin;
	else
		hBITMAP = m_hBmpPin;

	BITMAP bm;
	GetObject(hBITMAP, sizeof(BITMAP), &bm);
	BOOL bIsDisabled = (uiState & ODS_DISABLED) && m_bGrayDisabled;

	if (hBITMAP && !bIsDisabled)
	{
		CRect rectImage;
		CRect rectZone = rect;
		
		int w = ScalePix(bm.bmWidth);
		int h = ScalePix(bm.bmHeight);

		rectImage.left = center.x - (w / 2);
		rectImage.right = rectImage.left + w;
		rectImage.top = center.y - (h / 2);
		rectImage.bottom = rectImage.top + h;
	
		CDC	dcImage;
		if (!dcImage.CreateCompatibleDC(pDC))
			return;

		HGDIOBJ old = dcImage.SelectObject(hBITMAP);
		pDC->StretchBlt(rectImage.left, rectImage.top, w, h, &dcImage, 0, 0, bm.bmWidth, bm.bmHeight, SRCCOPY);
		
		dcImage.SelectObject(old);
		dcImage.DeleteDC();
	}
	else
	{
		__super::OnDraw(pDC, rect, uiState);
	}
}
