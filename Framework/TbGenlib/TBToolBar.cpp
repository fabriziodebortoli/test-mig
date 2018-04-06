#include "stdafx.h"

#include <atlimage.h>

#include <TbNameSolver\PathFinder.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGeneric\ParametersSections.h>
#include <tbgeneric\tbcmdui.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\TBThemeManager.h>

#include "TBDockPane.h"
#include <TbGenlib\commands.hrc>

#include <TBNameSolver\ThreadContext.h>

#include <TbGes\EXTDOC.H>
#include <TbGes\EXTDOC.hjson> //JSON AUTOMATIC UPDATE

#include "AddOnMng.h"
#include "TBToolBar.h"
#include "BaseApp.h"
#include "OslInfo.h"
#include "OslBaseInterface.h"
#include "MicroareaVisualManager.h"
#include "ToolBarButton.h"
#include <TbFrameworkImages\CommonImages.h>

#include "basefrm.hjson" //JSON AUTOMATIC UPDATE 
#include "generic.hjson" //JSON AUTOMATIC UPDATE 
#include <TbGes\tbges.hrc>
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#define COLOR_FLATTEN_DISTANCE	30
#define SPACE_TOOLBAR_RIGHT		10
#define NUMBER_OF_BUTTONS		15

#define TOOL_BAR_SPACE_HEIGHT	ScalePix(4)

/////////////////////////////////////////////////////////////////////////////
// ImageEnhancer
/////////////////////////////////////////////////////////////////////////////

class ImageEnhancer
{
public:
	ImageEnhancer();

public:
	void SharpenEdges
	(
		CDC*		pDC,
		CBitmap*	pBitmap,
		COLORREF*	pMask
	);

private:
	BOOL Similar(COLORREF aPixel, COLORREF aColor);
	void Explore(int x, int y, COLORREF aPixel);

private:
	CTypedPtrList<CPtrList, CPoint*>	m_Points;
	COLORREF							m_Mask;

};

//-------------------------------------------------------------------------------------
ImageEnhancer::ImageEnhancer()
{
}

//-------------------------------------------------------------------------------------
BOOL ImageEnhancer::Similar(COLORREF aPixel, COLORREF aColor)
{
	return	abs(GetRValue(aPixel) - GetRValue(aColor)) < COLOR_FLATTEN_DISTANCE &&
		abs(GetGValue(aPixel) - GetGValue(aColor)) < COLOR_FLATTEN_DISTANCE &&
		abs(GetBValue(aPixel) - GetBValue(aColor)) < COLOR_FLATTEN_DISTANCE;
}

//-------------------------------------------------------------------------------------
void ImageEnhancer::Explore(int x, int y, COLORREF aPixel)
{
	// se il pixel c'è (non sono fuori dall'immagine), non è già stato trasformato in ChromaKey
	// sarebbe da trasformare, lo aggiunge a quelli da trasformare
	if (
		aPixel != -1 &&
		aPixel != CLR_MAGENTA &&
		Similar(aPixel, m_Mask)
		)
		m_Points.AddHead(new CPoint(x, y));
}

//-------------------------------------------------------------------------------------
void ImageEnhancer::SharpenEdges
(
	CDC*		pDC,
	CBitmap*	pBitmap,
	COLORREF*	pMask
)
{
	BITMAP		bmImg;
	CBitmap*	pOldBmp;
	CDC			memDC;

	memDC.CreateCompatibleDC(pDC);
	pOldBmp = memDC.SelectObject(pBitmap);
	pBitmap->GetBitmap(&bmImg);

	m_Mask = *pMask;

	m_Points.RemoveAll(); // per sicurezza

	// inizia mettendo da esplorare tutti i pixel del bordo che andrebbero trasformati
	for (int x = 0; x < bmImg.bmWidth; x++)
	{
		Explore(x, 0, memDC.GetPixel(x, 0));
		Explore(x, bmImg.bmHeight - 1, memDC.GetPixel(x, bmImg.bmHeight - 1));
	}
	for (int y = 1; y < bmImg.bmHeight - 1; y++)
	{
		Explore(0, y, memDC.GetPixel(0, y));
		Explore(bmImg.bmWidth - 1, y, memDC.GetPixel(bmImg.bmWidth - 1, y));
	}

	// Trasforma in ChromaKey via via i pixel assegnati all'elenco di trasformazione.
	// Per ogni pixel trasformato, controlla se anche gli adiacenti sarebbero trasformabili, e li
	// aggiunge all'elenco. Non aggiunge quelli il cui colore è più "distante" di un valore prefissato
	// e quindi si ferma al primo bordo che ha un certo gradiente di colore rispetto allo sfondo.
	// L'utilizzo del ChromaKey (che sarà poi usato esternamente come maschera per il bottone)
	// previene l'effetto "cravatta di Caccamo" nel caso alcune parti interne dell'immagine abbiano 
	// lo stesso colore dello sfondo
	while (!m_Points.IsEmpty())
	{
		CPoint*		pPoint = m_Points.RemoveHead();
		COLORREF	aPixel = memDC.GetPixel(*pPoint);

		// se il pixel è sulla lista, vuol dire che è da trasformare. Lo trasforma
		// impostando il colore di ChromaKey
		memDC.SetPixel(*pPoint, CLR_MAGENTA);

		// Esplora i pixel adiacenti solo in senso verticale e orizzontale. Non controlla quelli
		// diagonali per evitare di "saltare" dentro ad un bordo stretto. Tanto tali pixel risulteranno
		// adiacenti a quelli qui inseriti
		Explore(pPoint->x - 1, pPoint->y, memDC.GetPixel(pPoint->x - 1, pPoint->y));
		Explore(pPoint->x + 1, pPoint->y, memDC.GetPixel(pPoint->x + 1, pPoint->y));
		Explore(pPoint->x, pPoint->y + 1, memDC.GetPixel(pPoint->x, pPoint->y + 1));
		Explore(pPoint->x, pPoint->y - 1, memDC.GetPixel(pPoint->x, pPoint->y - 1));

		delete pPoint;
	}

	memDC.SelectObject(pOldBmp);
	memDC.DeleteDC();
	*pMask = CLR_MAGENTA;
}


/////////////////////////////////////////////////////////////////////////////
//					CImageAssociation
/////////////////////////////////////////////////////////////////////////////
//-------------------------------------------------------------------------------------
CImageAssociation::CImageAssociation(UINT nCommandID, UINT nImageID, UINT nIndex)
	:
	m_nCommandID(nCommandID),
	m_nImageID(nImageID),
	m_nIndex(nIndex),
	m_nIndexAlternative(0),
	m_bCustomImage(FALSE),
	m_bCustomText(FALSE),
	m_nOldImageID(0),
	m_nOldImageNS(_T("")),
	m_sImageNameSpace(_T("")),
	m_strText(_T("")),
	m_strTextAlternative(_T(""))
{

}

//-------------------------------------------------------------------------------------
CImageAssociation::CImageAssociation(UINT nCommandID, CString sImageNameSpace, UINT nIndex)
	:
	m_nCommandID(nCommandID),
	m_nImageID(0),
	m_nIndex(nIndex),
	m_nIndexAlternative(0),
	m_bCustomImage(FALSE),
	m_bCustomText(FALSE),
	m_nOldImageID(0),
	m_nOldImageNS(_T("")),
	m_sImageNameSpace(sImageNameSpace),
	m_strText(_T("")),
	m_strTextAlternative(_T(""))
{
}

//-------------------------------------------------------------------------------------
inline const UINT CImageAssociation::GetCommandID() const
{
	return m_nCommandID;
}

//-------------------------------------------------------------------------------------
const UINT CImageAssociation::GetImageID() const
{
	return m_nImageID;
}

//-------------------------------------------------------------------------------------
const UINT CImageAssociation::GetIndex() const
{
	return m_nIndex;
}

//-------------------------------------------------------------------------------------
void CImageAssociation::SetText(CString mText, CString mTextAlternative /*= _T("")*/)
{
	m_strText = mText;
	m_strTextAlternative = mTextAlternative;
}

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

IMPLEMENT_SERIAL(CTBToolbarEditBoxButton, CBCGPToolbarEditBoxButton, 1)
#define ICON_SPACE 1

CTBToolbarEditBoxButton::CTBToolbarEditBoxButton()
{
	m_iHeight = 0;
	m_stLabel = _T("");
	m_hIcon = NULL;
}

CTBToolbarEditBoxButton::CTBToolbarEditBoxButton(const CString& sPrompt)
{
	CTBToolbarEditBoxButton();
	SetPrompt(sPrompt);
}

//-------------------------------------------------------------------------------------
CTBToolbarEditBoxButton::~CTBToolbarEditBoxButton()
{
	DestroyIcon(m_hIcon);
}

//-------------------------------------------------------------------------------------
CString CTBToolbarEditBoxButton::GetLabel()
{
	return m_stLabel;
}

//-------------------------------------------------------------------------------------
void CTBToolbarEditBoxButton::SetLabel(const CString& text)
{
	m_stLabel = text;
	m_rect.bottom += 30;
}

//-------------------------------------------------------------------------------------
void CTBToolbarEditBoxButton::OnMove()
{
	if (m_pWndEdit->GetSafeHwnd() == NULL || (m_pWndEdit->GetStyle() & WS_VISIBLE) == 0)
	{
		return;
	}

	__super::OnMove();

	CRect rectBorder;
	GetEditBorder(rectBorder);

	int iTop = rectBorder.top;
	int iHeight = rectBorder.Height();

	if (m_stLabel.IsEmpty() && AfxGetThemeManager()->IsToolbarInfinity())
	{
		iTop += 2;
		m_pWndEdit->SetWindowPos(NULL, rectBorder.left, iTop, rectBorder.Width(), iHeight, SWP_NOZORDER | SWP_NOACTIVATE);
		return;
	}

	if (m_stLabel.IsEmpty() && iHeight >= m_iHeight)
		return;

	if (!m_stLabel.IsEmpty())
	{
		iTop += 10;
	}

	if (iHeight < m_iHeight)
	{
		iHeight = m_iHeight;
	}

	if (m_hIcon)
	{
		rectBorder.right -= (m_iHeight + ICON_SPACE);
	}

	m_pWndEdit->SetWindowPos(NULL, rectBorder.left, iTop, rectBorder.Width(), iHeight, SWP_NOZORDER | SWP_NOACTIVATE);
}

//-------------------------------------------------------------------------------------
void CTBToolbarEditBoxButton::SetHeight(int iHeight)
{
	m_iHeight = iHeight;
}

//-------------------------------------------------------------------------------------
void CTBToolbarEditBoxButton::SetFont(CFont* pFont, BOOL bRedraw /*= TRUE*/)
{
	m_pWndEdit->SetFont(pFont, bRedraw);
}

//-------------------------------------------------------------------------------------
void CTBToolbarEditBoxButton::SetIcon(HICON hIcon)
{
	if (hIcon)
		m_hIcon = hIcon;
}

//-------------------------------------------------------------------------------------
int  CTBToolbarEditBoxButton::GetHeight()
{
	if (!m_pWndEdit)
	{
		return m_iHeight;
	}

	CRect rectBorder;
	GetEditBorder(rectBorder);

	if (m_iHeight > rectBorder.Height())
	{
		return m_iHeight;
	}
	return rectBorder.Height();
}

//-------------------------------------------------------------------------------------
HBRUSH CTBToolbarEditBoxButton::OnCtlColor(CDC* pDC, UINT nCtlColor)
{
	if (!(m_nStyle & TBBS_DISABLED))
	{
		CWnd* pFocused = CWnd::GetFocus();
		if (pFocused && pFocused->m_hWnd == this->m_pWndEdit->m_hWnd)
		{
			pDC->SetBkColor(AfxGetThemeManager()->GetFocusedControlBkgColor());
			return (HBRUSH)AfxGetThemeManager()->GetFocusedControlBkgColorBrush()->GetSafeHandle();
		}
		pDC->SetBkColor(AfxGetThemeManager()->GetEnabledControlBkgColor());
		return (HBRUSH)AfxGetThemeManager()->GetEnabledControlBkgColorBrush()->GetSafeHandle();
	}

	pDC->SetTextColor(AfxGetThemeManager()->GetDisabledControlForeColor());
	pDC->SetBkColor(AfxGetThemeManager()->GetBackgroundColor());
	return (HBRUSH)AfxGetThemeManager()->GetBackgroundColorBrush()->GetSafeHandle();
}

//-------------------------------------------------------------------------------------
void CTBToolbarEditBoxButton::OnDraw(CDC* pDC, const CRect& rect, CBCGPToolBarImages* pImages,
	BOOL bHorz, BOOL bCustomizeMode,
	BOOL bHighlight, BOOL bDrawBorder,
	BOOL bGrayDisabledButtons)
{
	CTBToolBar* m_pToolBar = dynamic_cast<CTBToolBar*> (m_pWndParent);
	// Toolbar in style Infinity
	if (AfxGetThemeManager()->IsToolbarInfinity())
	{
		CRect rect(rect);
		rect.bottom = rect.bottom - AfxGetThemeManager()->GetToolbarLineDownHeight() + 1;
		rect.top = rect.bottom - AfxGetThemeManager()->GetToolbarInfinityBckButtonHeight();
		pDC->FillSolidRect(rect, AfxGetThemeManager()->GetToolbarInfinityBckButtonColor());
		// Separetor line
		rect.left = rect.right - INFINITY_SEP_WIDTH;
		pDC->FillSolidRect(rect, AfxGetThemeManager()->GetToolbarInfinitySepColor());
	}

	if (!m_stLabel.IsEmpty())
	{
		CRect rectBorder;
		GetEditBorder(rectBorder);
		COLORREF clrText = CBCGPVisualManager::GetInstance()->GetToolbarButtonTextColor(this, CBCGPVisualManager::ButtonsIsRegular);
		COLORREF cltTextOld = pDC->SetTextColor(clrText);
		CRect rectText = rectBorder;
		rectText.top = rectBorder.top - 15;
		rectText.bottom = rectBorder.bottom - 15;
		pDC->DrawText(m_stLabel, &rectText, DT_LEFT | DT_WORDBREAK);
		pDC->SetTextColor(cltTextOld);
	}

	m_bTextBelow = FALSE;
	__super::OnDraw(pDC, &rect, pImages, bHorz, bCustomizeMode, bHighlight, bDrawBorder, bGrayDisabledButtons);

	// Draw icon if is present
	if (m_hIcon)
	{
		CRect rectBorder;
		CRect rectIco;
		GetEditBorder(rectBorder);
		rectIco.SetRect(rectBorder.right, rectBorder.top, rect.right, rectBorder.bottom);
		CBCGPVisualManager::GetInstance()->OnDrawEditBorder(pDC, rectIco, TRUE, !m_bFlat || m_bIsHotEdit, this);
		::DrawIconEx(pDC->GetSafeHdc(),
			rectBorder.right + ICON_SPACE, rectIco.top + ICON_SPACE,
			m_hIcon, m_iHeight, m_iHeight, 0, NULL, DI_NORMAL);
	}
}

//-------------------------------------------------------------------------------------
void CTBToolbarEditBoxButton::CopyFrom(const CBCGPToolbarButton& src)
{
	__super::CopyFrom(src);

	if (src.IsKindOf(RUNTIME_CLASS(CTBToolbarEditBoxButton)))
	{
		CTBToolbarEditBoxButton& s = (CTBToolbarEditBoxButton&)src;
		m_stLabel = s.GetLabel();
		m_iHeight = s.GetHeight();
	}
}

//////////////////////////////////////////////////////////////////////
// CTBToolbarComboBoxButton Construction/Destruction
//////////////////////////////////////////////////////////////////////

IMPLEMENT_SERIAL(CTBToolbarComboBoxButton, CBCGPToolbarComboBoxButton, 1)

//-------------------------------------------------------------------------------------
CTBToolbarComboBoxButton::CTBToolbarComboBoxButton()
{
	m_stLabel = _T("");
	m_bMove = TRUE;
	m_TmpRectCombo.SetRectEmpty();
	m_TmpRerectButton.SetRectEmpty();
	m_TmpRrect.SetRectEmpty();
}

CTBToolbarComboBoxButton::CTBToolbarComboBoxButton(UINT uiID, DWORD dwStyle /*= CBS_DROPDOWNLIST*/, int iWidth/* = 0*/, const CString& sPrompt/* = _T("")*/)
	:
	CBCGPToolbarComboBoxButton(uiID, 0, dwStyle, ScalePix(iWidth))
{
	m_stLabel = _T("");
	m_bMove = TRUE;
	m_TmpRectCombo.SetRectEmpty();
	m_TmpRerectButton.SetRectEmpty();
	m_TmpRrect.SetRectEmpty();
	m_nDropDownHeight = ScalePix(m_nDropDownHeight);
	if (!sPrompt.IsEmpty())
		SetPrompt(sPrompt);
}

//-------------------------------------------------------------------------------------
CTBToolbarComboBoxButton::~CTBToolbarComboBoxButton()
{

}

//-------------------------------------------------------------------------------------
CString CTBToolbarComboBoxButton::GetLabel()
{
	return m_stLabel;
}

//-------------------------------------------------------------------------------------
void CTBToolbarComboBoxButton::SetLabel(const CString& text)
{
	m_stLabel = text;
}

//-------------------------------------------------------------------------------------
void CTBToolbarComboBoxButton::CopyFrom(const CBCGPToolbarButton& src)
{
	__super::CopyFrom(src);
	if (src.IsKindOf(RUNTIME_CLASS(CTBToolbarComboBoxButton)))
	{
		CTBToolbarComboBoxButton& s = (CTBToolbarComboBoxButton&)src;
		m_stLabel = s.GetLabel();
	}
}

//-------------------------------------------------------------------------------------
void CTBToolbarComboBoxButton::OnDraw(CDC* pDC, const CRect& rect, CBCGPToolBarImages* pImages,
	BOOL bHorz, BOOL bCustomizeMode, BOOL bHighlight, BOOL bDrawBorder, BOOL bGrayDisabledButtons)
{
	if (!m_stLabel.IsEmpty())
	{
		COLORREF clrText = CBCGPVisualManager::GetInstance()->GetToolbarButtonTextColor(this, CBCGPVisualManager::ButtonsIsRegular);
		COLORREF cltTextOld = pDC->SetTextColor(clrText);
		CRect rectText = m_rectCombo;
		int yOffset = -(ScalePix(15));
		rectText.top = rectText.top + yOffset;
		rectText.bottom = rectText.bottom + yOffset;
		pDC->DrawText(m_stLabel, &rectText, DT_LEFT | DT_WORDBREAK);
		pDC->SetTextColor(cltTextOld);
	}

	// Toolbar in style Infinity
	if (AfxGetThemeManager()->IsToolbarInfinity())
	{
		CRect rectInfinity(rect);
		rectInfinity.bottom = rect.bottom + 5;
		rectInfinity.top = rectInfinity.bottom - AfxGetThemeManager()->GetToolbarInfinityBckButtonHeight();
		rectInfinity.left -= 1;
		rectInfinity.right += 1;
		pDC->FillSolidRect(rectInfinity, AfxGetThemeManager()->GetToolbarInfinityBckButtonColor());
		// Separetor line
		rectInfinity.left = rectInfinity.right - INFINITY_SEP_WIDTH;
		pDC->FillSolidRect(rectInfinity, AfxGetThemeManager()->GetToolbarInfinitySepColor());
	}

	__super::OnDraw(pDC, rect, pImages, bHorz, bCustomizeMode, bHighlight, bDrawBorder, bGrayDisabledButtons);
}

//-------------------------------------------------------------------------------------
void CTBToolbarComboBoxButton::OnMove()
{
	const int iHorzMargin = 1;	// Offset present in classe BCG in the metod AdjustRect () called from OnMove

	__super::OnMove();

	if (!m_pWndCombo)
		return;

	if (m_bCenterVert && (!m_bTextBelow || m_strText.IsEmpty()))
	{
		CBCGPToolBar* pParentBar = NULL;
		CWnd* pNextBar = m_pWndCombo->GetParent();

		while (pParentBar == NULL && pNextBar != NULL)
		{
			pParentBar = DYNAMIC_DOWNCAST(CBCGPToolBar, pNextBar);
			pNextBar = pNextBar->GetParent();
		}

		if (pParentBar != NULL)
		{
			const int nRowHeight = pParentBar->GetRowHeight();
			const int yOffset = max(0, (nRowHeight / 2) - 8);
			const int nBottom = m_rect.Height() + yOffset;

			m_rectButton.top = yOffset;
			m_rectButton.bottom = nBottom;
			m_rectButton.DeflateRect(0, 2);

			m_rectCombo.top = yOffset;
			m_rectCombo.bottom = nBottom;

			m_rect.top = yOffset;
			m_rect.bottom = nBottom;
		}
	}

	m_pWndCombo->SetWindowPos(NULL,
		m_rect.left + iHorzMargin, m_rect.top, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);

	if (m_pWndEdit)
		m_pWndEdit->SetWindowPos(NULL,
			m_rect.left + iHorzMargin + 3, m_rect.top + 3, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);

}

//////////////////////////////////////////////////////////////////////
// CTBToolbarButton Construction/Destruction
//////////////////////////////////////////////////////////////////////
IMPLEMENT_SERIAL(CTBToolbarButton, CBCGPToolbarButton, 1)

CTBToolbarButton::CTBToolbarButton() :
	m_bGhost(FALSE),
	m_bAutoHide(FALSE),
	m_bClone(FALSE),
	m_bWantText(TRUE),
	m_bSecondaryFillColor(FALSE),
	m_bDefaultButton(FALSE)
{
	if (AfxGetThemeManager()->IsToolbarInfinity()) m_bText = TRUE;
}

//-------------------------------------------------------------------------------------
CTBToolbarButton::~CTBToolbarButton()
{

}

//-------------------------------------------------------------------------------------
SIZE CTBToolbarButton::OnCalculateSize(CDC* pDC, const CSize& sizeDefault, BOOL bHorz)
{
	SIZE sizeRet = __super::OnCalculateSize(pDC, sizeDefault, bHorz);
	if (AfxGetThemeManager()->IsToolbarInfinity()) m_bTextBelow = FALSE;
	return sizeRet;
}

//-------------------------------------------------------------------------------------
void CTBToolbarButton::OnDraw(CDC* pDC, const CRect& rect, CBCGPToolBarImages* pImages,
	BOOL bHorz, BOOL bCustomizeMode, BOOL bHighlight,
	BOOL bDrawBorder, BOOL bGrayDisabledButtons)
{
	if (m_bGhost) { return; }

	m_rectButton = rect;

	CTBToolBar* m_pToolBar = dynamic_cast<CTBToolBar*> (m_pWndParent);
	if (m_bSecondaryFillColor && AfxGetThemeManager()->EnableToolBarDualColor())
	{
		if (m_bDisableFill) { m_bDisableFill = FALSE; }
		CRect rectDraw = rect;


		if (m_pToolBar && m_pToolBar->FindButton(m_nID) > 0)
		{
			// Skip the first buttons
			rectDraw.DeflateRect(1, 1);
		}
		pDC->FillSolidRect(rectDraw, AfxGetThemeManager()->GetToolbarBkgSecondaryColor());
	}

	// Toolbar in style Infinity
	if (AfxGetThemeManager()->IsToolbarInfinity())
	{
		CRect rect(m_rectButton);
		rect.bottom = rect.bottom - AfxGetThemeManager()->GetToolbarLineDownHeight() + 1;
		rect.top = rect.bottom - AfxGetThemeManager()->GetToolbarInfinityBckButtonHeight();
		pDC->FillSolidRect(rect, AfxGetThemeManager()->GetToolbarInfinityBckButtonColor());
		// Separetor line
		rect.left = rect.right - INFINITY_SEP_WIDTH;
		pDC->FillSolidRect(rect, AfxGetThemeManager()->GetToolbarInfinitySepColor());
	}

	__super::OnDraw(pDC, rect, pImages, bHorz, bCustomizeMode, bHighlight, bDrawBorder, bGrayDisabledButtons);

	if (m_bDefaultButton)
	{
		ASSERT_VALID(pDC);
		COLORREF clrBorder = AfxGetThemeManager()->GetToolbarButtonSetDefaultColor();
		m_rectButton.top = rect.bottom - AfxGetThemeManager()->GetToolbarHighlightedHeight();
		pDC->FillSolidRect(m_rectButton, clrBorder);
	}
}

//-------------------------------------------------------------------------------------
void CTBToolbarButton::CopyFrom(const CBCGPToolbarButton& src)
{
	__super::CopyFrom(src);

	if (src.IsKindOf(RUNTIME_CLASS(CTBToolbarButton)))
	{
		CTBToolbarButton* pSrc = (CTBToolbarButton*)&src;
		m_bGhost = pSrc->m_bGhost;
		m_bClone = pSrc->m_bClone;
		m_bWantText = pSrc->m_bWantText;
		m_bSecondaryFillColor = pSrc->m_bSecondaryFillColor;
		m_bDefaultButton = pSrc->m_bDefaultButton;
	}
	else
	{
		ASSERT(FALSE);
	}
}

//-------------------------------------------------------------------------------------
void CTBToolbarButton::SetClone(BOOL bClone /*= TRUE*/)
{
	m_bClone = bClone;
}

//-------------------------------------------------------------------------------------
void CTBToolbarButton::SetGhost(BOOL bGhost /*= TRUE*/)
{
	m_bGhost = bGhost;
}

//-------------------------------------------------------------------------------------
void CTBToolbarButton::SetWantText(BOOL bWantText)
{
	m_bWantText = bWantText;
}

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////
IMPLEMENT_SERIAL(CTBToolbarLabel, CBCGPToolbarButton, 1)

CTBToolbarLabel::CTBToolbarLabel(UINT uiID, LPCTSTR lpszText)
{
	if (lpszText != NULL)
	{
		m_strText = lpszText;
	}
	m_bCustomWidth = FALSE;
	m_bIsRightSpace = FALSE;
	m_iWidth = 0;
	m_bText = TRUE;
	m_nID = uiID;
	m_nAlign = TA_BOTTOM;
	SetStyle(TBBS_DISABLED);
	m_textColor = RGB(0, 0, 0);
	m_bTitle = FALSE;
}

//-------------------------------------------------------------------------------------
CTBToolbarLabel::~CTBToolbarLabel()
{

}

//-------------------------------------------------------------------------------------
void CTBToolbarLabel::CopyFrom(const CBCGPToolbarButton& src)
{
	__super::CopyFrom(src);

	if (src.IsKindOf(RUNTIME_CLASS(CTBToolbarLabel)))
	{
		CTBToolbarLabel* pSrc = (CTBToolbarLabel*)&src;
		m_nAlign = pSrc->GetTextAlign();
		m_bTitle = pSrc->IsTiitle();
		m_textColor = pSrc->GetColorText();
	}
	else
	{
		ASSERT(FALSE);
	}
}

//-------------------------------------------------------------------------------------
void CTBToolbarLabel::OnDraw(CDC* pDC, const CRect& rect, CBCGPToolBarImages* pImages,
	BOOL bHorz, BOOL bCustomizeMode, BOOL bHighlight, BOOL bDrawBorder, BOOL bGrayDisabledButtons)
{
	UINT nStyle = m_nStyle;
	static const CString strDummyAmpSeq = _T("\001\001");
	CSize sizeImage = (pImages == NULL) ? CSize(0, 0) : pImages->GetImageSize(TRUE);

	m_nStyle &= ~TBBS_DISABLED;

	CString strWithoutAmp = GetDisplayText();
	strWithoutAmp.Replace(_T("&&"), strDummyAmpSeq);
	strWithoutAmp.Remove(_T('&'));
	strWithoutAmp.Replace(strDummyAmpSeq, _T("&"));
	CSize sizeText = pDC->GetTextExtent(strWithoutAmp);
	CRect rectText = rect;
	UINT nTextMargin = 3;

	if (!AfxGetThemeManager()->IsToolbarInfinity())
	{
		if (m_nAlign & TA_BOTTOM)
		{
			rectText.top = (rectText.bottom - sizeText.cy) - nTextMargin;
		}
		else if ((m_nAlign & TA_CENTER))
		{
			rectText.top = rectText.top + UINT(sizeText.cy / 2);
		}
		else
		{
			rectText.top = nTextMargin;
		}
	}
	else
	{
		if (m_nAlign & TA_BOTTOM)
		{
			rectText.top = sizeImage.cy + (2 * nTextMargin);
		}
		else if ((m_nAlign & TA_CENTER))
		{
			rectText.top = rectText.top + UINT((sizeImage.cy + (2 * nTextMargin)) / 2);
		}
		else
		{
			rectText.top = rectText.top + 2 * nTextMargin;
		}
	}

	// Toolbar in style Infinity
	CFont* pOldFont = pDC->GetCurrentFont();
	if (m_bTitle)
	{
		// Change Font 
		LOGFONT logFont;
		pOldFont->GetLogFont(&logFont);
		pDC->SelectObject(AfxGetThemeManager()->GetToolBarTitleFont());
		CRect rectTextTmp(rectText);
		pDC->DrawText(strWithoutAmp, &rectTextTmp, DT_CALCRECT | DT_LEFT | DT_CENTER | DT_WORDBREAK | DT_SINGLELINE);
		rectText.left += 10;
		rectText.right = rectText.left + rectTextTmp.Width();
		// Center horizontal
		rectText.top = (rectText.bottom - (rect.Height() / 2)) - (rectTextTmp.Height() / 2);
		CRect rcFill(rectText);
		rcFill.top = rect.top;
		rcFill.bottom = rect.bottom;
		SetWidth(max(rectText.Width(), rect.Width()));
	}
	else if (!m_bIsRightSpace && AfxGetThemeManager()->IsToolbarInfinity())
	{
		CRect rectFill(rectText);
		rectFill.bottom = rectFill.bottom - AfxGetThemeManager()->GetToolbarLineDownHeight() + 1;
		rectFill.top = rectFill.bottom - AfxGetThemeManager()->GetToolbarInfinityBckButtonHeight();
		pDC->FillSolidRect(rectFill, AfxGetThemeManager()->GetToolbarInfinityBckButtonColor());
		// Separetor line
		rectFill.left = rect.right - INFINITY_SEP_WIDTH;
		pDC->FillSolidRect(rectFill, AfxGetThemeManager()->GetToolbarInfinitySepColor());
	}

	// set text color label
	pDC->SetTextColor(m_textColor);
	// draw text
	if (m_bTitle)
		pDC->DrawText(strWithoutAmp, &rectText, DT_LEFT | DT_CENTER | DT_WORDBREAK | DT_SINGLELINE);
	else
		pDC->DrawText(strWithoutAmp, &rectText, DT_LEFT | DT_CENTER | DT_WORDBREAK);

	// restore old font
	if (m_bTitle)
		pDC->SelectObject(pOldFont);

	m_nStyle = nStyle | TBBS_DISABLED;
}

//-------------------------------------------------------------------------------------
void CTBToolbarLabel::SetTitle()
{
	m_bTitle = TRUE;
}

//
// text align: TA_CENTER - TA_TOP - TA_BOTTOM
//-------------------------------------------------------------------------------------
void CTBToolbarLabel::SetTextAlign(UINT nAlign /*= TA_BOTTOM*/)
{
	m_nAlign = nAlign;
}

//-------------------------------------------------------------------------------------
void CTBToolbarLabel::SetRightSpace()
{
	m_bIsRightSpace = TRUE;
	SetWidth(50);
}

//-------------------------------------------------------------------------------------
void CTBToolbarLabel::SetWidth(int width)
{
	m_bCustomWidth = TRUE;
	m_iWidth = width;
}

//-------------------------------------------------------------------------------------
SIZE CTBToolbarLabel::OnCalculateSize(CDC* pDC, const CSize& sizeDefault, BOOL bHorz)
{
	m_iImage = -1;
	if (m_bCustomWidth)
	{
		if (AfxGetThemeManager()->IsToolbarInfinity() && m_bTitle)
		{
			return CSize(m_iWidth, max(sizeDefault.cy, AfxGetThemeManager()->GetToolbarHeight()));
		}

		return CSize(m_iWidth, sizeDefault.cy);
	}

	return __super::OnCalculateSize(pDC, sizeDefault, bHorz);
}

//////////////////////////////////////////////////////////////////////
// CTBToolbarMenuButton
//////////////////////////////////////////////////////////////////////
CTBToolBarMenu::CTBToolBarMenu() :
	m_pDC(NULL)
{
}

CTBToolBarMenu::~CTBToolBarMenu()
{
	IconsListClean();
}

//-------------------------------------------------------------------------------------
void CTBToolBarMenu::IconsListClean()
{
	if (m_iconsListUnChecked.GetCount() > 0)
	{
		for (POSITION pos = m_iconsListUnChecked.GetHeadPosition(); pos != NULL; )
		{
			CIconList pIconList = m_iconsListUnChecked.GetNext(pos);
			HICON hIco = pIconList.GetIcon();
			DestroyIcon(hIco);
		}
	}

	if (m_iconsListChecked.GetCount() > 0)
	{
		for (POSITION pos = m_iconsListChecked.GetHeadPosition(); pos != NULL; )
		{
			CIconList pIconList = m_iconsListChecked.GetNext(pos);
			HICON hIco = pIconList.GetIcon();
			DestroyIcon(hIco);
		}
	}

	m_iconsListUnChecked.RemoveAll();
	m_iconsListChecked.RemoveAll();
}

//-------------------------------------------------------------------------------------
void CTBToolBarMenu::SetDC(CDC* pDC)
{
	m_pDC = pDC;
}

//-------------------------------------------------------------------------------------
HICON CTBToolBarMenu::GetIconUnChecked(UINT nID)
{
	for (POSITION pos = m_iconsListUnChecked.GetHeadPosition(); pos != NULL; )
	{
		CIconList pIconList = m_iconsListUnChecked.GetNext(pos);
		if (pIconList.GetId() == nID)
			return pIconList.GetIcon();
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
HICON CTBToolBarMenu::GetIconChecked(UINT nID)
{
	for (POSITION pos = m_iconsListChecked.GetHeadPosition(); pos != NULL; )
	{
		CIconList pIconList = m_iconsListChecked.GetNext(pos);
		if (pIconList.GetId() == nID)
			return pIconList.GetIcon();
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
void CTBToolBarMenu::SetMenuItemBitmaps(UINT nID, const CString& aLibNamespace, UINT nIDImgUnchecked, UINT nIDImgChecked, BOOL bPng)
{
	HICON hUnchecked = TBLoadImage(m_pDC, ImageNameSpaceWalking(aLibNamespace), nIDImgUnchecked, TOOLBARMENU_ICON_SIZE, bPng);
	m_iconsListUnChecked.AddTail(CIconList(hUnchecked, nID));

	if (nIDImgChecked > 0)
	{
		HICON hChecked = TBLoadImage(m_pDC, ImageNameSpaceWalking(aLibNamespace), nIDImgChecked, TOOLBARMENU_ICON_SIZE, bPng);
		m_iconsListChecked.AddTail(CIconList(hChecked, nID));
	}
}

//-------------------------------------------------------------------------------------
void CTBToolBarMenu::SetMenuItemBitmaps(UINT nID, CBitmap* pBmpUnchecked, CBitmap* pBmpChecked)
{
	/*	Devo controllare se è la medesima immagine, sul puntatore dell' immagine viene applicato il filtro di Mask!
		Qundi se le immagini sono uguali le operazioni di conversione devono essere fatte una sola volta seno la seconda è
		il negativo dell' altra.!
	*/

	HICON hUnChecked = CBitmapToHICON(pBmpUnchecked, m_pDC, TOOLBARMENU_ICON_SIZE);
	HICON hChecked = NULL;

	if (pBmpUnchecked == pBmpChecked)
		hChecked = hUnChecked;
	else
		hChecked = CBitmapToHICON(pBmpChecked, m_pDC, TOOLBARMENU_ICON_SIZE);

	if (pBmpUnchecked->m_hObject && hUnChecked)
		m_iconsListUnChecked.AddTail(CIconList(hUnChecked, nID));

	if (pBmpChecked->m_hObject && hChecked)
		m_iconsListChecked.AddTail(CIconList(hChecked, nID));
}

//-------------------------------------------------------------------------------------
void CTBToolBarMenu::SetMenuItemHICON(UINT nID, HICON hIconUnchecked, HICON hIconChecked)
{
	if (hIconUnchecked)
		m_iconsListUnChecked.AddTail(CIconList(hIconUnchecked, nID));

	if (hIconChecked)
		m_iconsListChecked.AddTail(CIconList(hIconChecked, nID));
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBarMenu::AppendMenu(UINT nFlags, UINT_PTR nIDNewItem, LPCTSTR lpszNewItem, UINT nIDImgUnchecked, UINT nIDImgChecked, BOOL bPng)
{
	if (nIDImgUnchecked > 0)
		SetMenuItemBitmaps(nIDNewItem, _T(""), nIDImgUnchecked, nIDImgChecked, bPng);

	return __super::AppendMenu(nFlags, nIDNewItem, lpszNewItem);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBarMenu::InsertMenu(UINT nPosition, UINT nFlags, UINT_PTR nIDNewItem /*= NULL*/, LPCTSTR lpszNewItem /*= NULL*/, UINT nIDImgUnchecked /*= 0*/, UINT nIDImgChecked /*= 0*/, BOOL bPng /*= TRUE*/)
{
	if (nIDImgUnchecked > 0)
		SetMenuItemBitmaps(nIDNewItem, _T(""), nIDImgUnchecked, nIDImgChecked, bPng);

	return __super::InsertMenu(nPosition, nFlags, nIDNewItem, lpszNewItem);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBarMenu::AppendMenu(UINT nFlags, UINT_PTR nIDNewItem, LPCTSTR lpszNewItem, CBitmap* pBmpUnchecked, CBitmap* pBmpChecked /* = NULL*/)
{
	if (pBmpUnchecked != NULL)
	{
		HICON hUnchecked = CBitmapToHICON(pBmpUnchecked);
		m_iconsListUnChecked.AddTail(CIconList(hUnchecked, (UINT)nIDNewItem));
	}

	if (pBmpChecked != NULL)
	{
		HICON hChecked = CBitmapToHICON(pBmpChecked);
		m_iconsListChecked.AddTail(CIconList(hChecked, (UINT)nIDNewItem));
	}
	return __super::AppendMenu(nFlags, nIDNewItem, lpszNewItem);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBarMenu::InsertMenu(UINT nPosition, UINT nFlags, UINT_PTR nIDNewItem, LPCTSTR lpszNewItem, CBitmap* pBmpUnchecked, CBitmap* pBmpChecked /*= NULL*/)
{
	if (pBmpUnchecked != NULL)
	{
		HICON hUnchecked = CBitmapToHICON(pBmpUnchecked);
		m_iconsListUnChecked.AddTail(CIconList(hUnchecked, (UINT)nIDNewItem));
	}

	if (pBmpChecked != NULL)
	{
		HICON hChecked = CBitmapToHICON(pBmpChecked);
		m_iconsListChecked.AddTail(CIconList(hChecked, (UINT)nIDNewItem));
	}

	return __super::InsertMenu(nPosition, nFlags, nIDNewItem, lpszNewItem);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBarMenu::AppendMenu(UINT nFlags, UINT_PTR nIDNewItem, LPCTSTR lpszNewItem, const CString& sImageNSUnchecked, const CString& sImageNSChecked /*= NULL*/)
{
	HICON hUnChecked = TBLoadPng(sImageNSUnchecked);
	HICON hChecked = NULL;

	if (!sImageNSChecked.IsEmpty())
		hChecked = TBLoadPng(sImageNSChecked);


	if (hUnChecked)
		m_iconsListUnChecked.AddTail(CIconList(hUnChecked, nIDNewItem));

	if (hChecked)
		m_iconsListChecked.AddTail(CIconList(hChecked, nIDNewItem));

	return __super::AppendMenu(nFlags, nIDNewItem, lpszNewItem);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBarMenu::InsertMenu(UINT nPosition, UINT nFlags, UINT_PTR nIDNewItem, LPCTSTR lpszNewItem, const CString& sImageNSUnchecked, const CString& sImageNSChecked /*= NULL*/)
{
	HICON hUnChecked = TBLoadPng(sImageNSUnchecked);
	HICON hChecked = NULL;

	if (!sImageNSChecked.IsEmpty())
		hChecked = TBLoadPng(sImageNSChecked);


	if (hUnChecked)
		m_iconsListUnChecked.AddTail(CIconList(hUnChecked, nIDNewItem));

	if (hChecked)
		m_iconsListChecked.AddTail(CIconList(hChecked, nIDNewItem));

	return __super::InsertMenu(nPosition, nFlags, nIDNewItem, lpszNewItem);
}


//-------------------------------------------------------------------------------------
void CTBToolBarMenu::RemoveAll()
{
	int iCount = GetMenuItemCount();
	for (int i = 0; i < iCount; i++)
	{
		DeleteMenu(0, MF_BYPOSITION);
	}
	IconsListClean();
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBarMenu::ExistID(int nIDToFound)
{
	int iCount = GetMenuItemCount();
	for (int i = 0; i < iCount; i++)
	{
		UINT nID = GetMenuItemID(i);
		if (nID == nIDToFound)
			return TRUE;
	}
	return FALSE;
}

//-------------------------------------------------------------------------------------
void CTBToolBarMenu::AppendFromMenu(CMenu* pSorg)
{
	CTBToolBarMenu* pMenu = dynamic_cast<CTBToolBarMenu*> (pSorg);
	if (!pMenu) return;

	INT iMenuCount = pMenu->GetMenuItemCount();
	for (int i = 0; i < iMenuCount; i++)
	{
#ifdef _DEBUG // Diagnostic
		MENUITEMINFO  info;
		info.cbSize = sizeof(MENUITEMINFO); // must fill up this field
		info.fMask = MIIM_SUBMENU;             // get the state of the menu item
		pMenu->GetMenuItemInfo(i, &info, TRUE);
		ASSERT(!info.hSubMenu);
#endif

		UINT nID = pMenu->GetMenuItemID(i);
		CString menuStr;
		pMenu->GetMenuString(i, menuStr, MF_BYPOSITION);
		HICON hChecked = pMenu->GetIconChecked(nID);
		HICON hUnChecked = pMenu->GetIconUnChecked(nID);

		BOOL bFond = FALSE;
		for (int j = 0; j < GetMenuItemCount(); j++)
		{
			UINT nIDDest = GetMenuItemID(j);

			// Skip separetor and sub menu
			if (nIDDest == -1) continue;
			MENUITEMINFO  info;
			info.cbSize = sizeof(MENUITEMINFO); // must fill up this field
			info.fMask = MIIM_SUBMENU;             // get the state of the menu item
			GetMenuItemInfo(j, &info, TRUE);
			if (info.hSubMenu) continue;
			// No add voice of menu if is present.!
			if (nID == nIDDest)
			{
				bFond = TRUE;
			}
		}

		if (bFond) continue;

		AppendMenu(MF_STRING, nID, menuStr);
		if (hChecked) m_iconsListChecked.AddTail(CIconList(hChecked, nID));
		if (hUnChecked) m_iconsListUnChecked.AddTail(CIconList(hUnChecked, nID));
	}
}

//////////////////////////////////////////////////////////////////////
// CTBToolbarMenuButton
//////////////////////////////////////////////////////////////////////
IMPLEMENT_SERIAL(CTBToolbarMenuButton, CBCGPToolbarMenuButton, 1)

//-------------------------------------------------------------------------------------
CTBToolbarMenuButton::CTBToolbarMenuButton() :
	m_bTBelow(FALSE),
	m_bAutoHide(FALSE),
	m_MissingClick(FALSE)
{
	AfxGetMenuWindow();
	SetAlwaysDropDown(AfxGetThemeManager()->AlwaysDropDown());
	if (AfxGetThemeManager()->IsToolbarInfinity()) m_bText = TRUE;
}

//-------------------------------------------------------------------------------------
CTBToolbarMenuButton::~CTBToolbarMenuButton()
{
	if (GetParentWnd())
	{
		CLocalizableFrame* pParentFrame = dynamic_cast<CLocalizableFrame*>(GetParentWnd()->GetParentFrame());
		if (pParentFrame)
		{
			pParentFrame->SetToolBarActive(NULL);
		}
	}
}

//-------------------------------------------------------------------------------------
SIZE CTBToolbarMenuButton::OnCalculateSize(CDC* pDC, const CSize& sizeDefault, BOOL bHorz)
{
	SIZE sizeRet = __super::OnCalculateSize(pDC, sizeDefault, bHorz);
	if (AfxGetThemeManager()->IsToolbarInfinity()) m_bTextBelow = FALSE;
	return sizeRet;
}

//-------------------------------------------------------------------------------------
CBCGPPopupMenu* CTBToolbarMenuButton::CreatePopupMenu()
{
	CBCGPPopupMenu* pPopupMenu = __super::CreatePopupMenu();

	// ** For CParsedDialog dropDown buttons 
	if (GetParentWnd() && GetParentWnd()->GetParent())
	{
		CWnd* pCWnd = GetParentWnd()->GetParent();
		ASSERT(pCWnd);
		CParsedDialogWithTiles* pDialogWithTiles = dynamic_cast<CParsedDialogWithTiles*>(pCWnd);
		if (pDialogWithTiles == NULL)
		{
			CParsedDialog* pDialog = dynamic_cast<CParsedDialog*>(pCWnd);
			if (pDialog)
				m_pWndMessage = pDialog;
		}
	}
	// ** For CParsedDialog dropDown buttons **

	if (pPopupMenu && m_pWndMessage)
	{
		pPopupMenu->SetMessageWnd(m_pWndMessage);
	}
	else
	{
		if (this->GetParentWnd() && this->GetParentWnd()->GetParent() && this->GetParentWnd()->GetParent()->GetParentFrame())
		{
			m_pWndMessage = this->GetParentWnd()->GetParent()->GetParentFrame();
		}
		else
			ASSERT(FALSE);
	}

	CBCGPPopupMenuBar* pBar = pPopupMenu->GetMenuBar();

	if (pBar)
	{
		INT m_iToolbarMenu_Width = TOOLBARMENU_ICON_SIZE;
		// resize delle icos nella toolbar
		pBar->SetLockedSizes(CSize(m_iToolbarMenu_Width + 6, m_iToolbarMenu_Width + 6), CSize(m_iToolbarMenu_Width, m_iToolbarMenu_Width), TRUE);
		pBar->SetSizes(CSize(m_iToolbarMenu_Width + 6, m_iToolbarMenu_Width + 6), CSize(m_iToolbarMenu_Width, m_iToolbarMenu_Width));
	}

	return pPopupMenu;
}

//-------------------------------------------------------------------------------------
void CTBToolbarMenuButton::TextBelow(BOOL bBelow)
{
	m_bTBelow = bBelow;
}

//-------------------------------------------------------------------------------------
void CTBToolbarMenuButton::CopyFrom(const CBCGPToolbarButton& src)
{
	__super::CopyFrom(src);
	m_bTBelow = ((CTBToolbarMenuButton&)src).m_bTBelow;
}

//-------------------------------------------------------------------------------------
HMENU CTBToolbarMenuButton::GetMenu()
{
	return CBCGPToolbarMenuButton::CreateMenu();
}

//-------------------------------------------------------------------------------------
void CTBToolbarMenuButton::OnDraw(CDC* pDC, const CRect& rect, CBCGPToolBarImages* pImages,
	BOOL bHorz, BOOL bCustomizeMode, BOOL bHighlight, BOOL bDrawBorder, BOOL bGrayDisabledButtons)
{
	// Text below the icon
	if (!AfxGetThemeManager()->IsToolbarInfinity())
		m_bTextBelow = m_bTBelow;

	CTBToolBar* m_pToolBar = dynamic_cast<CTBToolBar*> (m_pWndParent);
	// Toolbar in style Infinity
	if (AfxGetThemeManager()->IsToolbarInfinity())
	{
		CRect rect(rect);
		rect.bottom = rect.bottom - AfxGetThemeManager()->GetToolbarLineDownHeight() + 1;
		rect.top = rect.bottom - AfxGetThemeManager()->GetToolbarInfinityBckButtonHeight();
		pDC->FillSolidRect(rect, AfxGetThemeManager()->GetToolbarInfinityBckButtonColor());
		// Separetor line
		rect.left = rect.right - INFINITY_SEP_WIDTH;
		pDC->FillSolidRect(rect, AfxGetThemeManager()->GetToolbarInfinitySepColor());
	}

	__super::OnDraw(pDC, rect, pImages, bHorz, bCustomizeMode, bHighlight, bDrawBorder, bGrayDisabledButtons);

	if (!AfxGetThemeManager()->AlwaysDropDown() && m_bDrawDownArrow)
	{
		COLORREF clrFill = AfxGetThemeManager()->GetToolbarButtonArrowFillColor();
		CBCGPDrawManager dm(*pDC);

		m_rectArrow.bottom -= (AfxGetThemeManager()->GetToolbarHighlightedHeight() + 3);
		dm.DrawRect(m_rectArrow, clrFill, clrFill);

		// Draw a New Arrow
		LONG nX1 = m_rectArrow.left + 1;
		LONG nX2 = m_rectArrow.right - 2;
		LONG nY1 = m_rectArrow.top + (m_rectArrow.Height() / 2);
		while (nX2 >= nX1)
		{
			dm.DrawLine(nX1, nY1, nX2, nY1, AfxGetThemeManager()->GetToolbarButtonArrowColor());
			nX1 += 1; nX2 -= 1; nY1 += 1;
		}

	}
}

//-------------------------------------------------------------------------------------
void CTBToolbarMenuButton::OnPopulatedMenuButton()
{
	CWnd* pCWnd = GetParentWnd()->GetParent();
	ASSERT(pCWnd);

	CParsedDialog* pDialog = dynamic_cast<CParsedDialog*>(pCWnd);
	if (pDialog && pDialog->OnPopulatedDropDown(this->m_nID))
	{
		return;
	}

	CTaskBuilderDockPane* pPane = dynamic_cast<CTaskBuilderDockPane*>(pCWnd);
	if (pPane)
	{
		pPane->OnPopulatedDropDown(this->m_nID);
	}
	else
	{
		CLocalizableFrame* pParentFrame = dynamic_cast<CLocalizableFrame*>(this->GetParentWnd()->GetParentFrame());
		if (pParentFrame)
			pParentFrame->OnPopulatedDropDown(this->m_nID);
		else
		{
			CParsedDialog* pDialog = dynamic_cast<CParsedDialog*>(this->GetParentWnd()->GetParentFrame());
			if (pDialog)
				pDialog->OnPopulatedDropDown(this->m_nID);
		}
	}
}

//DECLARE_THREAD_VARIABLE(HMENU, t_hTrackingMenu)
//DECLARE_THREAD_VARIABLE(HWND, t_hTrackingWindow)
//-------------------------------------------------------------------------------------
BOOL CTBToolbarMenuButton::OpenPopupMenu(CWnd* pWnd /*= NULL*/)
{
	if (AfxIsRemoteInterface() && pWnd == NULL)
	{
		CTBToolBar* toolbar = dynamic_cast<CTBToolBar*>(GetParentWnd());
		if (toolbar)
		{
			pWnd = toolbar;
		}
	}

	if (GetParentWnd())
	{
		CLocalizableFrame* pParentFrame = dynamic_cast<CLocalizableFrame*>(GetParentWnd()->GetParentFrame());
		if (pParentFrame)
		{
			pParentFrame->SetToolBarActive(GetParentWnd());
		}
	}

	OnPopulatedMenuButton();
	return __super::OpenPopupMenu(pWnd);
}

//-------------------------------------------------------------------------------------
void CTBToolbarMenuButton::EnableAlwaysDropDown(BOOL bAlwaysDropDown /*TRUE*/)
{
	SetAlwaysDropDown(bAlwaysDropDown ? PURE_ALWAYS_DROPDOWN : 0);
}

//-------------------------------------------------------------------------------------
void CTBToolbarMenuButton::SetAlwaysDropDown(int nAlwaysDropDown)
{
	m_nAlwaysDropDown = nAlwaysDropDown;
	if (m_nAlwaysDropDown == PURE_ALWAYS_DROPDOWN)
		SetMenuOnly(TRUE);
}

//-------------------------------------------------------------------------------------
int	CTBToolbarMenuButton::GetAlwaysDropDown() const
{
	return m_nAlwaysDropDown;
}

//-------------------------------------------------------------------------------------
void CTBToolbarMenuButton::SetMissingClick(BOOL bMissing)
{
	m_MissingClick = bMissing;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolbarMenuButton::HasMissingClick() const
{
	return m_MissingClick;
}

//-------------------------------------------------------------------------------------
void CTBToolbarMenuButton::CreateMenu(CMenu* pMenu)
{
	// Append the accelerator in menu of drop down
	CLocalizableFrame* pParentFrame = dynamic_cast<CLocalizableFrame*>(this->GetParentWnd()->GetParentFrame());
	if (pParentFrame)
	{
		CAbstractDoc* pDoc = dynamic_cast<CAbstractDoc*>(pParentFrame->GetActiveDocument());
		if (pDoc)
		{
			INT nItem = pMenu->GetMenuItemCount();
			for (int i = 0; i < nItem; i++)
			{
				UINT nID = pMenu->GetMenuItemID(i);
				CString	accText = pDoc->GetDocAccelText(nID);
				if (!accText.IsEmpty())
				{
					CString menuStr;
					pMenu->GetMenuString(i, menuStr, MF_BYPOSITION);
					// The accelerator is present in string ?
					if (menuStr.Find(accText) < 0)
					{
						menuStr.Append(_T(" ("));
						menuStr.Append(accText);
						menuStr.Append(_T(")"));
						pMenu->ModifyMenu(i, MF_BYPOSITION, nID, menuStr);
					}
				}

				CCmdUI state;
				state.m_nIndexMax = nItem;
				state.m_nIndex = i;
				state.m_pMenu = pMenu;
				state.m_nID = nID;
				pDoc->OnCmdMsg(nID, CN_UPDATE_COMMAND_UI, &state, NULL);
			}
		}
	}

	// Rimozione voce fitizzia se presente
	if (pMenu->GetMenuItemCount() >= 2)
	{
		CString menuStr;
		pMenu->GetMenuString(0, menuStr, MF_BYPOSITION);
		if (menuStr.IsEmpty())
		{
			pMenu->DeleteMenu(0, MF_BYPOSITION);
		}
	}

	// Append menu
	__super::CreateFromMenu(pMenu->Detach());
}

/////////////////////////////////////////////////////////////////////////////
void CTBToolBarCmdUI::Enable(BOOL bOn)
{
	m_bEnableChanged = TRUE;
	CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*>(m_pOther);
	ASSERT(pToolBar != NULL);
	ASSERT_KINDOF(CTBToolBar, pToolBar);
	ASSERT(m_nIndex < m_nIndexMax);

	UINT nNewStyle = pToolBar->GetButtonStyle(m_nIndex) & ~TBBS_DISABLED;

	if (!bOn)
		nNewStyle |= TBBS_DISABLED;
	ASSERT(!(nNewStyle & TBBS_SEPARATOR));
	pToolBar->SetButtonStyle(m_nIndex, nNewStyle);

}

/////////////////////////////////////////////////////////////////////////////
//					CTBToolBar
/////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CTBToolBar, CBCGPToolBar)
	//{{AFX_MSG_MAP(CTaskBuilderToolBar)
	ON_WM_DESTROY()
	ON_WM_LBUTTONDBLCLK()
	ON_WM_LBUTTONUP()
	ON_WM_SIZE()
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	ON_MESSAGE(UM_LAYOUT_SUSPENDED_CHANGED, OnSuspendLayoutChanged)

	ON_REGISTERED_MESSAGE(BCGM_EDIT_ON_FILL_AUTOCOMPLETE_LIST, OnFillAutoComple)
	ON_MESSAGE(UM_TOOLBAR_UPDATE, OnToolBarUpdate)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------
CTBToolBar::CTBToolBar()
	:
	m_pParentOSL(NULL),
	m_pObjectRight(NULL),
	m_pObjectLeft(NULL),
	m_pMenuButtonCollapsed(NULL),
	m_bDefaultActionEnable(FALSE),
	m_pParentTabbedToolbar(NULL),
	m_bSuspendLayout(FALSE),
	m_bButtonsOverlap(FALSE),
	m_bPostToolBarUpdate(FALSE),
	m_pDefaultActionButton(NULL),
	m_bButtonStyleLoopComplite(FALSE),
	m_bRecalcGray(FALSE)
{
	m_bDialog = FALSE;
	m_nDialogUpdateToolBar = 0;
	m_bToRight = FALSE;
	m_bTBelow = TRUE;
	m_iWidthObjectLeft = m_iWidthObjectRight = 0;
	m_bCenterButtons = FALSE;
	m_nIDOverlapButton = -1;
	m_nIDLastDropDown = 0;
	n_IdDefaultAction = -1;
	m_nLastRight = -1;

	m_bToolbarInfinity = AfxGetThemeManager()->IsToolbarInfinity();
	m_iToolbarButton_Width = TOOLBARMENU_ICON_SIZE; /*AfxGetThemeManager()->GetToolbarHeight();*/
	if (m_bToolbarInfinity) m_iToolbarButton_Width = 16;
	m_iToolbarButton_Width = MulDiv(m_iToolbarButton_Width, GetLogPixels(), SCALING_FACTOR);

	m_clrBkgColor = AfxGetThemeManager()->GetToolbarBkgColor();
	m_clrForeColor = AfxGetThemeManager()->GetToolbarTextColor();
	m_cTextColor = AfxGetThemeManager()->GetToolbarTextColor();
	m_cTextColorHighlighted = AfxGetThemeManager()->GetToolbarTextColorHighlighted();
	m_cHighlightedColor = AfxGetThemeManager()->GetToolbarHighlightedColor();
	m_cHighlightedColorClick = AfxGetThemeManager()->GetToolbarHighlightedClickColor();
	m_bHighlightedColorClickEnable = AfxGetThemeManager()->ToolbarHighlightedColorClickEnable();
	m_bAdjustLayoutHideButton = TRUE;
	m_iAdjustLayoutHideButton = -1;
	m_PrevFormMode = CBaseDocument::FormMode::NONE;
	m_bShowShortcutKeys = FALSE;
	m_stIconCollapsed = TBIcon(szIconCollapsed, TOOLBAR);
	m_bAutoHideToolBarButton = AfxGetThemeManager()->AutoHideToolBarButton();
	m_bShowToolBarTab = AfxGetThemeManager()->ShowToolBarTab();

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-------------------------------------------------------------------------------------
CTBToolBar::~CTBToolBar()
{
	for (int i = 0; i <= m_arInfoOSL.GetUpperBound(); i++)
		SAFE_DELETE(m_arInfoOSL.GetAt(i));
	m_arInfoOSL.RemoveAll();

	for (int n = m_Images.GetUpperBound(); n >= 0; n--)
		SAFE_DELETE(m_Images.GetAt(n));
	m_Images.RemoveAll();

	for (POSITION pos = m_ListCollapsedItems.GetHeadPosition(); pos != NULL; )
	{
		CTBCollapsedItem* pItem = m_ListCollapsedItems.GetNext(pos);
		ASSERT(pItem);
		SAFE_DELETE(pItem);
	}
	m_ListCollapsedItems.RemoveAll();

	HICON ico;
	UINT key;
	for (POSITION pos = m_mapCollapsedImage.GetStartPosition(); pos != NULL; ico = NULL, key = NULL)
	{
		m_mapCollapsedImage.GetNextAssoc(pos, key, ico);
		DestroyIcon(ico);
	}

	// Drop down clean cache
	IconsDropDownClean();

	GetLockedImages()->Clear();
	GetLockedDisabledImages()->Clear();

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//-------------------------------------------------------------------------------------
void CTBToolBar::IconsDropDownClean()
{
	HICON ico;
	UINT key;
	for (POSITION pos = m_mapDropDownIconsUnChecked.GetStartPosition(); pos != NULL; ico = NULL, key = NULL)
	{
		m_mapDropDownIconsUnChecked.GetNextAssoc(pos, key, ico);
		DestroyIcon(ico);
	}

	for (POSITION pos = m_mapDropDownIconsChecked.GetStartPosition(); pos != NULL; ico = NULL, key = NULL)
	{
		m_mapDropDownIconsChecked.GetNextAssoc(pos, key, ico);
		DestroyIcon(ico);
	}
}

//-------------------------------------------------------------------------------------
void CTBToolBar::AddCollapsedImage(UINT nID, HICON hIcon)
{
	HICON hIconFound = GetCollapsedImage(nID);
	if (hIconFound)
		DestroyIcon(hIconFound);

	if (hIcon == NULL) return;
	HICON hIconCopy = CopyIcon(hIcon);
	ASSERT(hIconCopy);

	// Remove old icon
	HICON hIcoFound = GetCollapsedImage(nID);
	if (hIcoFound)
		DestroyIcon(hIcoFound);

	m_mapCollapsedImage.SetAt(nID, hIconCopy);
}

//-------------------------------------------------------------------------------------
HICON CTBToolBar::GetCollapsedImage(UINT nID)
{
	HICON hIco;
	if (m_mapCollapsedImage.Lookup(nID, hIco))
	{
		return hIco;
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::OnSize(UINT nType, int cx, int cy)
{
	if (IsLayoutSuspended() || (m_pParentTabbedToolbar && m_pParentTabbedToolbar->IsLayoutSuspended()))
		return;

	__super::OnSize(nType, cx, cy);

	if (RepositionRightButtons() && m_bDialog)
	{
		AdjustLayout();
	}
}

//-------------------------------------------------------------------------------------
HICON CTBToolBar::LoadImageByNameSpace(CString strImageNS, UINT nID)
{
	HICON hIcon = NULL;
	CDC* pDC = GetDC();

	hIcon = TBLoadImage(strImageNS, pDC, m_iToolbarButton_Width);

	AddCollapsedImage(nID, hIcon);
	ReleaseDC(pDC);
	DestroyIcon(hIcon);
	return LoadImageByNameSpace(strImageNS);
}

//-------------------------------------------------------------------------------------
HICON CTBToolBar::LoadImageByNameSpace(CString strImageNS, CDC* pDC /*= NULL*/, UINT nWidth /*= 0*/)
{
	BOOL bReleaseDC = FALSE;
	if (pDC == NULL)
	{
		pDC = GetDC();
		bReleaseDC = TRUE;
	}

	if (nWidth == 0)
	{
		nWidth = m_iToolbarButton_Width;
	}

	CString strImageNS_W = _T("");
	COLORREF clrBkgColor = m_clrBkgColor;

	if (IsToolbarInfinity())
	{
		// Infinity Theme is white
		clrBkgColor = AfxGetThemeManager()->GetToolbarInfinityBckButtonColor();
		strImageNS = ReplaceInfinity(strImageNS);
	}

	if (ULONG(clrBkgColor) < ULONG(RGB(127, 127, 127)))
	{
		// Appen in name fo file icone name _W
		if (strImageNS.Right(4).CompareNoCase(_T(".PNG")) == 0)
		{
			strImageNS_W = strImageNS.Left(strImageNS.GetLength() - 4) + _T("_W") + strImageNS.Right(4);
		}
	}

	HICON hRet;
	BOOL bLoadStandard = TRUE;
	if (!strImageNS_W.IsEmpty())
	{
		hRet = TBLoadImage(strImageNS_W, pDC, nWidth, clrBkgColor);
		if (hRet)
		{
			bLoadStandard = FALSE;
		}
	}

	if (bLoadStandard)
	{
		hRet = TBLoadImage(strImageNS, pDC, nWidth, clrBkgColor);
	}

	if (bReleaseDC)
	{
		ReleaseDC(pDC);
	}

	return hRet;
}

//---------------------------------------------------------------------------
void CTBToolBar::RemoveAccelerator(HACCEL& hAccelTable, UINT nID)
{
	int iNumAccelerators = CopyAcceleratorTable(hAccelTable, NULL, 0);
	int iNewNum = 0;

	ACCEL *pAccels = new ACCEL[iNumAccelerators];
	ACCEL *pNewAccels = new ACCEL[iNumAccelerators + 1];

	// Copy the current table to the buffer
	VERIFY(CopyAcceleratorTable(hAccelTable, pAccels, iNumAccelerators) == iNumAccelerators);
	for (int k = 0; k < iNumAccelerators; k++) {
		if (nID != pAccels[k].cmd)
		{
			pNewAccels[iNewNum].cmd = pAccels[k].cmd;
			pNewAccels[iNewNum].fVirt = pAccels[k].fVirt;
			pNewAccels[iNewNum].key = pAccels[k].key;
			iNewNum++;
		}
	}

	if (hAccelTable)
	{
		// Destroy the current table resource...
		VERIFY(DestroyAcceleratorTable(hAccelTable) == TRUE);
	}

	if (iNewNum > 0)
	{
		// ... create a new one, based on our modified table
		hAccelTable = CreateAcceleratorTable(pNewAccels, iNewNum);
		ASSERT(hAccelTable != NULL || AfxIsRemoteInterface());
	}
	else
		hAccelTable = NULL;

	// Cleanup
	delete[] pAccels;
	delete[] pNewAccels;
}

//---------------------------------------------------------------------------
void CTBToolBar::AppendAccelerator(HACCEL& hAccelTable, UINT nID, BYTE fVirt, WORD key)
{
	int iNumAccelerators = CopyAcceleratorTable(hAccelTable, NULL, 0);

	ACCEL *pAccels = new ACCEL[iNumAccelerators];
	ACCEL *pNewAccels = new ACCEL[iNumAccelerators + 1];

	// Copy the current table to the buffer
	VERIFY(CopyAcceleratorTable(hAccelTable, pAccels, iNumAccelerators) == iNumAccelerators);
	int k = 0;
	for (; k < iNumAccelerators; k++) {
		pNewAccels[k].cmd = pAccels[k].cmd;
		pNewAccels[k].fVirt = pAccels[k].fVirt;
		pNewAccels[k].key = pAccels[k].key;
	}

	// Append new acceleretor
	pNewAccels[k].cmd = nID;
	pNewAccels[k].fVirt = fVirt;
	pNewAccels[k].key = key;

	if (hAccelTable)
	{
		// Destroy the current table resource...
		VERIFY(DestroyAcceleratorTable(hAccelTable) == TRUE);
	}

	// ... create a new one, based on our modified table
	hAccelTable = CreateAcceleratorTable(pNewAccels, k + 1);
	ASSERT(hAccelTable != NULL || AfxIsRemoteInterface());
	// Cleanup
	delete[] pAccels;
	delete[] pNewAccels;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::SetDefaultAction(UINT nCommandID)
{
	HACCEL*	phAccelTable = NULL;
	CWnd* pParentWnd = GetParent();
	if (pParentWnd && pParentWnd->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
		phAccelTable = &((CParsedDialog*)pParentWnd)->m_hAccelTable;
	else
	{
		pParentWnd = GetParentFrame();

		if (pParentWnd && pParentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormFrame)))
			phAccelTable = &((CAbstractFormFrame*)pParentWnd)->m_hAccelTable;
		else
		{
			ASSERT(FALSE);
			return FALSE;
		}
	}

	if (m_pDefaultActionButton)
	{
		m_pDefaultActionButton->SetDefaultButton(FALSE);
		RemoveAccelerator(*phAccelTable, m_pDefaultActionButton->m_nID);
	}

	AppendAccelerator(*phAccelTable, nCommandID, FVIRTKEY, VK_RETURN);

	// Underline the button of set default 
	CTBToolbarButton* pButton = FindButtonPtr(nCommandID);
	if (pButton)
	{
		m_pDefaultActionButton = pButton;
		pButton->SetDefaultButton();
	}

	// aggiunge l'acceleratore di default per IDCANCEL <=> VK_ESCAPE
	if (FindButtonPtr(IDCANCEL) && nCommandID != IDCANCEL)
	{
		RemoveAccelerator(*phAccelTable, ID_EXTDOC_ESCAPE);
		AppendAccelerator(*phAccelTable, IDCANCEL, FVIRTKEY, VK_ESCAPE);
	}

	return TRUE;
}

//-------------------------------------------------------------------------------------
INT CTBToolBar::GetDefaultAction()
{
	return n_IdDefaultAction;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::IsToolbarMenuButton(int iButton)
{
	if (iButton > -1 && AfxGetThemeManager()->AlwaysDropDown())
	{
		CBCGPToolbarButton* pButton = GetButton(iButton);
		if (pButton == NULL)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		if (pButton->IsKindOf(RUNTIME_CLASS(CTBToolbarMenuButton)))
			return TRUE;
	}
	return FALSE;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::OnLButtonDblClk(UINT nFlags, CPoint point)
{
	// La gestione da parte dei BCG genera una ripetizione del
	// OnLButtonDown nelle toolBar

	//int iButton = HitTest(point);
	//if (IsToolbarMenuButton(iButton))
	//	return;
	//__super::OnLButtonDblClk(nFlags, point);
}

//-------------------------------------------------------------------------------------
void CTBToolBar::OnDestroy()
{
	__super::OnDestroy();

}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::GetButtonNamespace(UINT nBtnID, CTBNamespace& ns)
{
	for (int i = 0; i < m_arInfoOSL.GetSize(); i++)
	{
		CInfoOSLButton* pB = m_arInfoOSL.GetAt(i);
		if (pB->m_nID != nBtnID)
			continue;

		ns = pB->m_Namespace;
		return TRUE;
	}
	return FALSE;
}

//Dummy button is a button used to fill space between others button. It has no action, no image, no tooltip.
//-----------------------------------------------------------------------------
BOOL CTBToolBar::IsDummyButton(CBCGPToolbarButton* pBtn)
{
	if (pBtn->IsKindOf(RUNTIME_CLASS(CTBToolbarLabel)))
	{
		return  ((CTBToolbarLabel*)pBtn)->IsRightSpace();
	}
	if (pBtn->IsKindOf(RUNTIME_CLASS(CTBToolbarButton)))
	{
		return  ((CTBToolbarButton*)pBtn)->IsGhost();
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CTBToolBar::SetBkgColor(COLORREF color)
{
	m_clrBkgColor = color;
}

//-----------------------------------------------------------------------------
COLORREF CTBToolBar::GetBkgColor()
{
	return m_clrBkgColor;
}

//-----------------------------------------------------------------------------
void CTBToolBar::SetForeColor(COLORREF color)
{
	m_clrForeColor = color;
}

//-----------------------------------------------------------------------------
COLORREF CTBToolBar::GetForeColor()
{
	return m_clrForeColor;
}

//-----------------------------------------------------------------------------
void CTBToolBar::SetTextColor(COLORREF color)
{
	m_cTextColor = color;
}

//-----------------------------------------------------------------------------
COLORREF CTBToolBar::GetTextColor()
{
	return m_cTextColor;
}

//-----------------------------------------------------------------------------
void CTBToolBar::SetTextColorHighlighted(COLORREF color)
{
	m_cTextColorHighlighted = color;
}

//-----------------------------------------------------------------------------
COLORREF CTBToolBar::GetTextColorHighlighted()
{
	return m_cTextColorHighlighted;
}

//-----------------------------------------------------------------------------
void CTBToolBar::SetHighlightedColor(COLORREF color)
{
	m_cHighlightedColor = color;
}

//-----------------------------------------------------------------------------
COLORREF CTBToolBar::GetHighlightedColor()
{
	return m_cHighlightedColor;
}

//-----------------------------------------------------------------------------
void CTBToolBar::SetHighlightedColorClick(COLORREF color)
{
	m_cHighlightedColorClick = color;
}

//-----------------------------------------------------------------------------
COLORREF CTBToolBar::GetHighlightedColorClick()
{
	return m_cHighlightedColorClick;
}
//-----------------------------------------------------------------------------
void CTBToolBar::EnableHighlightedColorClick(BOOL bEnable/* = TRUE*/)
{
	m_bHighlightedColorClickEnable = bEnable;
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::IsEnableHighlightedColorClick()
{
	return m_bHighlightedColorClickEnable;
}

//---------------------------------------------------------------------------------
LRESULT	CTBToolBar::OnSuspendLayoutChanged(WPARAM wParam, LPARAM lParam)
{
	CDockableFrame* pFrame = (CDockableFrame*)wParam;

	if (pFrame)
	{
		if (pFrame->m_bLayoutSuspended)
			SuspendLayout();
		else
		{
			ResumeLayout(TRUE);
			RepositionRightButtons();
			AdjustSizeImmediate();
			AdjustLayout();

		}
	}

	return 0L;
}

//-----------------------------------------------------------------------------
LRESULT CTBToolBar::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;
	CString sButtonId;
	CFrameWnd* pParentFrame = GetParentFrame();
	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CToolbarDescription* pToolbarDesc = dynamic_cast<CToolbarDescription*>  (pContainer->GetWindowDescription(this, RUNTIME_CLASS(CToolbarDescription), strId));
	ASSERT(pToolbarDesc);
	if (!pToolbarDesc) return NULL;
	pToolbarDesc->UpdateAttributes(this);
	this->WriteTabName(pToolbarDesc);

	if (m_arAccelerators.GetCount())
		pToolbarDesc->m_pAccelerator = new CAcceleratorDescription(m_arAccelerators);

	AdjustLayout();

	int iBtnCount = GetCount();
	for (int i = 0; i < iBtnCount; i++)  //TODOBCG era ctrl.GetButtonCount()
	{
		UINT nID, nStyle;
		int iImage;
		GetButtonInfo(i, nID, nStyle, iImage);
		CBCGPToolbarButton* pBtn = GetButton(i);
		if (IsDummyButton(pBtn))
			continue;

		if ((nStyle & TBBS_HIDDEN) == TBBS_HIDDEN)
			continue;

		if (GetDlgItem(nID))
			continue;
		if (nID == 0)
			nID = i;

		sButtonId.Format(_T("%d_%d"), m_hWnd, nID);

		CToolbarBtnDescription *pButtonDesc = dynamic_cast<CToolbarBtnDescription*> (pToolbarDesc->m_Children.GetWindowDescription(NULL, RUNTIME_CLASS(CToolbarBtnDescription), sButtonId));
		ASSERT(pButtonDesc);
		//if button is a dropdown menu button, check for a context menu open.
		//standard pattern to retrieve context menu description, because CBCGPToolbarMenuButton never calls TrackPopupMenu
		//to show the menu
		CBCGPToolbarMenuButton* pMenuButton = dynamic_cast<CBCGPToolbarMenuButton*>(pBtn);
		if (pMenuButton)
		{
			CBCGPPopupMenu* pPopupMenu = pMenuButton->GetPopupMenu();
			if (pPopupMenu)
			{
				HMENU hMenu = pPopupMenu->GetHMenu();
				ASSERT(::IsMenu(hMenu));
				CMenu* pMenu = CMenu::FromHandle(hMenu);
				CString strId = cwsprintf(_T("%d"), hMenu);
				CMenuDescription* pContextMenu = dynamic_cast<CMenuDescription*> (pToolbarDesc->m_Children.GetWindowDescription(NULL, RUNTIME_CLASS(CMenuDescription), strId));
				ASSERT(pContextMenu);
				pContextMenu->SetOwnerID(pButtonDesc->GetJsonID());
				pContextMenu->UpdateAttributes(pMenu, m_hWnd);
			}
		}

		CRect rectToolbarBtn;
		GetItemRect(i, rectToolbarBtn);

		if (pButtonDesc->m_strText != pBtn->m_strText)
		{
			pButtonDesc->m_strText = pBtn->m_strText;
			pButtonDesc->SetUpdated(&pBtn->m_strText);
		}
		pButtonDesc->m_bVisible = TRUE == pBtn->IsVisible();
		CTBToolbarButton* pTBBtn = dynamic_cast<CTBToolbarButton*>(pBtn);

		if ((nStyle & TBBS_SEPARATOR) != TBBS_SEPARATOR &&
			(nStyle & TBBS_GROUP) != TBBS_GROUP &&
			(nStyle & TBBS_CHECKGROUP) != TBBS_CHECKGROUP
			)
		{
			bool bIsDropdown = (nStyle & TBBS_DROPDOWN) == TBBS_DROPDOWN;
			if (pButtonDesc->m_bIsDropdown != bIsDropdown)
			{
				pButtonDesc->m_bIsDropdown = bIsDropdown;
				pButtonDesc->SetUpdated(&pButtonDesc->m_bIsDropdown);
			}
			CImageAssociation* pImageAssociation = GetImageAssociation(pBtn->m_nID);
			if (pImageAssociation)
			{
				CString sIcon;

				//image are only identified by namaspace. Image embedded as resource are no longer used
				ASSERT(!(pImageAssociation->GetImageNameSpace().IsEmpty()));
				sIcon.Format(pImageAssociation->GetImageNameSpace());

				if (pButtonDesc->m_sIcon != sIcon)
				{
					pButtonDesc->m_sIcon = sIcon;
					pButtonDesc->SetUpdated(&pButtonDesc->m_sIcon);

#ifndef TBWEB
					GetImageBytes(sIcon, pButtonDesc->m_ImageBuffer);
#endif			
				}
			}

			pButtonDesc->m_strCmd.Format(_T("%d"), nID);
		}
		else if ((nStyle & TBBS_SEPARATOR) == TBBS_SEPARATOR)
		{
			// current button is a toolbar separator.
			pButtonDesc->m_bIsSeparator = TRUE;
		}


		ClientToScreen(rectToolbarBtn);

		if (!(pButtonDesc->GetRect().EqualRect(rectToolbarBtn)))
		{
			pButtonDesc->SetRect(rectToolbarBtn, TRUE);
		}

		if (pParentFrame && pButtonDesc->m_strHint.IsEmpty())
		{
			CString strTooltip;
			pParentFrame->GetMessageString(nID, strTooltip);
			if (strTooltip.IsEmpty())
				strTooltip = _T(" ");
			pButtonDesc->m_strHint = strTooltip;
			pButtonDesc->SetUpdated(&pButtonDesc->m_strHint);
		}

		CTBCmdUI ui(nID);
		bool bEnabled = (pParentFrame && ui.DoUpdate(pParentFrame, TRUE))
			? TRUE == ui.GetEnabled()
			: true;
		if (pButtonDesc->m_bEnabled != bEnabled)
		{
			pButtonDesc->m_bEnabled = bEnabled;
			pButtonDesc->SetUpdated(&pButtonDesc->m_bEnabled);
		}
	}

	pToolbarDesc->AddChildWindows(this);
	return (LRESULT)pToolbarDesc;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::ShowToolBarDown(CWnd* pParentWnd, BOOL bShowText /*= FALSE*/)
{
	m_clrForeColor = AfxGetThemeManager()->GetDialogToolbarForeColor();
	m_clrBkgColor = AfxGetThemeManager()->GetDialogToolbarBkgColor();

	SetBarStyle(this->GetBarStyle() & ~(CBRS_GRIPPER | CBRS_SIZE_DYNAMIC));
	EnableDocking(pParentWnd, CBRS_ALIGN_BOTTOM);
	SetBCGStyle(0);
	EnableTextLabels(bShowText);
	AdjustSizeImmediate();
	RepositionRightButtons();
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::ShowInDialog(CWnd* pParentWnd, DWORD dwAlignment /*CBRS_ALIGN_TOP | CBRS_ALIGN_LEFT*/)
{
	m_clrForeColor = AfxGetThemeManager()->GetDialogToolbarForeColor();
	m_clrBkgColor = AfxGetThemeManager()->GetDialogToolbarBkgColor();

	this->SetBarStyle(this->GetBarStyle() & ~(CBRS_GRIPPER | CBRS_SIZE_DYNAMIC));
	if (pParentWnd)
		this->SetParent(pParentWnd);

	__super::EnableDocking(dwAlignment);
	SetBCGStyle(0);
	SetRouteCommandsViaFrame(FALSE);
	m_bDialog = TRUE;
	AdjustLayout();


	return TRUE;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetTBImages()
{
	// resize delle icos nella toolbar
	SetLockedSizes(CSize(m_iToolbarButton_Width + 7, m_iToolbarButton_Width + 6), CSize(m_iToolbarButton_Width, m_iToolbarButton_Width), TRUE);
	SetSizes(CSize(m_iToolbarButton_Width + 7, m_iToolbarButton_Width + 6), CSize(m_iToolbarButton_Width, m_iToolbarButton_Width));
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::IsToolbarInfinity()
{
	return m_bToolbarInfinity;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetParentTabbedToolbar(CTBTabbedToolbar* pParent)
{
	m_pParentTabbedToolbar = pParent;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::AddDocuemntTitle(CString strTitle)
{
	INT nButton = FindButton(ID_EXDOC_TAB_TITLE);
	if (nButton >= 0)
	{
		CBCGPToolbarButton* pButton = GetButton(nButton);
		if (pButton && pButton->IsKindOf(RUNTIME_CLASS(CTBToolbarLabel)))
		{
			pButton->m_strText = strTitle;
		}
		return;
	}

	CTBToolbarLabel tbLabel(ID_EXDOC_TAB_TITLE, strTitle.MakeUpper());
	tbLabel.SetTextAlign(TA_CENTER);
	tbLabel.SetColorText(RGB(255, 255, 255));
	tbLabel.SetTitle();

	__super::InsertButton(tbLabel, 0);

	if (RepositionRightButtons())
		AdjustSizeImmediate();
	AdjustLayout();
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::Create(CWnd* pParentWnd, UINT nToolbarID, CStringArray* pNamespaceArray, DWORD dwStyle, DWORD dwCtrlStyle)
{
	if (!pParentWnd || !::IsWindow(pParentWnd->m_hWnd))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	SetTBImages();

	if (!CreateEx(pParentWnd, dwCtrlStyle, dwStyle, CRect(1, 1, 1, 1), nToolbarID))
	{
		TRACE0("Failed to create toolbar\n");
		return FALSE;      // fail to create
	}

	m_bLocked = TRUE;

	if (pParentWnd->IsKindOf(RUNTIME_CLASS(CLocalizableFrame)))
	{
		CDocument* pDoc = ((CLocalizableFrame*)pParentWnd)->GetActiveDocument();
		if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
			AttachParentOSL(((CBaseDocument*)pDoc)->GetInfoOSL());
	}

	SetBorders(1, 1, 1, 1);

	for (int i = 0; i < GetCount(); i++)
	{
		CBCGPToolbarButton* pButton = GetButton(i);
		if (!pButton)
			continue;

		if (pNamespaceArray && pNamespaceArray->GetCount() > i)
		{
			CTBNamespace aNs(CTBNamespace::TOOLBARBUTTON, pNamespaceArray->GetAt(i));
			AddOslInfo(pButton->m_nID, pNamespaceArray->GetAt(i), aNs.GetObjectName());
		}
	}

	if (IsToolbarInfinity()) AddDocuemntTitle(_T(" "));

	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::CreateEmpty(CWnd* pParentWnd, CString sName, DWORD dwStyle, BOOL bCTBTabbedToolbar)
{
	BOOL ret = FALSE;

	UINT nID = AfxGetTBResourcesMap()->GetTbResourceID(sName, TbResourceType::TbCommands);
	if (!bCTBTabbedToolbar)
		ret = Create(pParentWnd, nID, NULL, dwStyle);
	else
		ret = Create(pParentWnd, nID, NULL, dwStyle, TBSTYLE_FLAT);

	//if (ret)
	//	this->RemoveAllButtons();

	if (this->IsLocked())
	{
		GetLockedImages()->Clear();
		GetLockedDisabledImages()->Clear();

		// set image Size of look image
		CRect rectImage(0, 0, m_iToolbarButton_Width, m_iToolbarButton_Width);
		GetLockedImages()->SetImageSize(rectImage.Size());
		GetLockedDisabledImages()->SetImageSize(rectImage.Size());
	}

	return ret;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::CreateEmptyTabbedToolbar(CWnd* pParentWnd, const CString& sName, const CString& sText)
{
	BOOL ret = CreateEmpty(pParentWnd, sName, WS_CHILD | WS_VISIBLE | CBRS_TOP | CBRS_SIZE_DYNAMIC, TRUE);

	if (!sName.IsEmpty())
	{
		m_sName = sName;
	}
	if (!sText.IsEmpty())
	{
		this->SetWindowText(sText);
		m_sTitle = sText;
	}
	else if (!sName.IsEmpty())
	{
		this->SetWindowText(sName);
		m_sTitle = sName;
	}

	return ret;
}

// Serve per posizzionare delle toolBar 
// sopratutto qundo non deve apparire in alto
//-------------------------------------------------------------------------------------
void CTBToolBar::EnableDocking(CWnd* pParentWnd, DWORD dwAlignment)
{
	CDockableFrame* pDockableFrame = dynamic_cast<CDockableFrame*> (pParentWnd);
	if (pDockableFrame)
	{
		__super::EnableDocking(dwAlignment);
		pDockableFrame->EnableDocking(dwAlignment);
		pDockableFrame->EnableAutoHideBars(dwAlignment);
		pDockableFrame->DockControlBar(this);
	}
}

//-------------------------------------------------------------------------------------
void CTBToolBar::GetObjectGrant(CInfoOSL* pParentOSL)
{
	m_pParentOSL = pParentOSL;
	for (int i = 0; i <= m_arInfoOSL.GetUpperBound(); i++)
	{
		CInfoOSLButton* pInfo = m_arInfoOSL.GetAt(i);
		if (!pInfo)
			continue;

		if (pInfo->m_Namespace.IsEmpty() || m_pParentOSL)
			pInfo->m_Namespace.SetChildNamespace(CTBNamespace::TOOLBARBUTTON, pInfo->m_strName, m_pParentOSL->m_Namespace);

		pInfo->m_pParent = m_pParentOSL;

		AfxGetSecurityInterface()->GetObjectGrant(pInfo);
	}
}

//-------------------------------------------------------------------------------------
void CTBToolBar::AddOslInfo(UINT nCommandID, const CString& sParentNamespace, const CString& sName)
{
	CTBNamespace aNs;
	aNs.AutoCompleteNamespace(CTBNamespace::TOOLBARBUTTON, sName, sParentNamespace);

	CInfoOSLButton* pInfo = new CInfoOSLButton(nCommandID, sName);
	pInfo->m_pParent = m_pParentOSL;
	pInfo->SetType(OSLType_ToolbarButton);
	pInfo->m_Namespace = aNs;
	m_arInfoOSL.Add(pInfo);

	if (!aNs.IsEmpty() && aNs.IsValid())
		AfxGetSecurityInterface()->GetObjectGrant(pInfo);
}

//-------------------------------------------------------------------------------------
void CTBToolBar::AddOslInfo(UINT nCommandID, const CString& sName)
{
	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::TOOLBARBUTTON, sName, GetInfoOSL()->m_Namespace);

	CInfoOSLButton* pInfo = new CInfoOSLButton(nCommandID, sName);
	pInfo->m_pParent = GetInfoOSL();
	pInfo->SetType(OSLType_ToolbarButton);
	pInfo->m_Namespace = aNs;
	m_arInfoOSL.Add(pInfo);

	if (!aNs.IsEmpty() && aNs.IsValid())
	{
		AfxGetSecurityInterface()->GetObjectGrant(pInfo);

		BOOL bVisible = OSL_CAN_DO(pInfo, OSL_GRANT_EXECUTE);
		if (!bVisible)
		{
			HideButton(nCommandID);
		}
	}
}

//-------------------------------------------------------------------------------------
void CTBToolBar::AttachOSLInfo(CInfoOSL* pParent)
{
	CTBNamespace aNs;
	CString strTitle = _T("");
	GetWindowTextW(strTitle);

	aNs.SetChildNamespace(CTBNamespace::TOOLBAR, strTitle, pParent->m_Namespace);

	GetInfoOSL()->m_pParent = pParent;
	GetInfoOSL()->SetType(OSLType_Toolbar);
	GetInfoOSL()->m_Namespace = aNs;

	// refresh the buttons for the new parent 
	for (int i = 0; i <= m_arInfoOSL.GetUpperBound(); i++)
		SAFE_DELETE(m_arInfoOSL.GetAt(i));
	m_arInfoOSL.RemoveAll();

	if (!aNs.IsEmpty() && aNs.IsValid())
		AfxGetSecurityInterface()->GetObjectGrant(GetInfoOSL());

	int iButton = 0;
	for (POSITION pos = m_Buttons.GetHeadPosition(); pos != NULL; iButton++)
	{
		CTBToolbarButton* pButton = dynamic_cast<CTBToolbarButton*>  (m_Buttons.GetNext(pos));
		if (pButton == NULL) continue;
		if (!pButton->m_strText.Trim().IsEmpty())
		{
			AddOslInfo(pButton->m_nID, pButton->m_strText);
		}
	}
}

//-------------------------------------------------------------------------------------
CInfoOSLButton* CTBToolBar::GetInfoOSLButton(UINT nID)
{
	for (int i = 0; i < m_arInfoOSL.GetCount(); i++)
	{
		if (m_arInfoOSL[i]->m_nID == nID)
			return m_arInfoOSL[i];
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::AddSeparator(int nIdx /*-1*/)
{
	// Toolbar in style Infinity not present
	if (IsToolbarInfinity()) return;

	if (nIdx == -1)
	{
		if (m_pObjectRight)
			nIdx = GetRightSpacePos();
	}

	InsertSeparator(nIdx);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::ExButton(UINT nCommandID)
{
	if (FindButton(nCommandID) >= 0)
		return TRUE;
	return FALSE;
}

//-------------------------------------------------------------------------------------
INT CTBToolBar::FindButton(UINT nCommandID)
{
	for (int i = 0; i < GetCount(); i++)
	{
		CBCGPToolbarButton* pButton = GetButton(i);
		if (!pButton || pButton->m_nID != nCommandID)
			continue;

		return i;
	}
	return -1;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetSecondaryToolBarColor(UINT nCommandID, BOOL bActive /*= TRUE*/)
{
	CTBToolbarButton* pButton = FindButtonPtr(nCommandID);
	pButton->SecondaryFillColorButton(bActive);
}

//-------------------------------------------------------------------------------------
CTBToolbarButton* CTBToolBar::FindButtonPtr(UINT nCommandID)
{
	for (int i = 0; i < GetCount(); i++)
	{
		CBCGPToolbarButton* pButton = GetButton(i);
		if (!pButton || pButton->m_nID != nCommandID)
			continue;

		return dynamic_cast<CTBToolbarButton*>(pButton);
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
INT CTBToolBar::FindButton(CString sNameSpace, BOOL bIsComplete /*TRUE*/)
{
	for (int i = 0; i < m_arInfoOSL.GetSize(); i++)
	{
		CInfoOSLButton* pB = m_arInfoOSL.GetAt(i);
		CString sn = pB->m_Namespace.ToUnparsedString();

		// namespace passato per intero
		if (bIsComplete && (pB->m_Namespace.ToUnparsedString().CompareNoCase(sNameSpace) == 0 || pB->m_Namespace.ToString().CompareNoCase(sNameSpace) == 0))
			return pB->m_nID;

		// namespace passato per token finale
		if (!bIsComplete && pB->m_Namespace.GetObjectName().CompareNoCase(sNameSpace) == 0)
			return pB->m_nID;
	}
	return -1;
}

//-------------------------------------------------------------------------------------
CInfoOSLButton* CTBToolBar::FindOslInfoButton(CString sNameSpace)
{
	for (int i = 0; i < m_arInfoOSL.GetSize(); i++)
	{
		CInfoOSLButton* pB = m_arInfoOSL.GetAt(i);
		if (pB->m_Namespace.ToUnparsedString().CompareNoCase(sNameSpace) != 0)
			continue;

		return pB;
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::EnableButton(UINT nCommandID, BOOL enable)
{
	int nIndex = FindButton(nCommandID);
	if (nIndex < 0 || nIndex >= GetCount()) return;

	if (enable)
	{
		SetButtonStyle(nIndex, TBBS_BUTTON);
	}
	else
	{
		SetButtonStyle(nIndex, TBBS_DISABLED);
	}
}

//-------------------------------------------------------------------------------------
void CTBToolBar::PressButton(UINT nCommandID, BOOL setCheckBoxStyle, BOOL check/* = TRUE*/)
{
	int nIndex = FindButton(nCommandID);
	if (nIndex < 0 || nIndex >= GetCount()) return;

	if (setCheckBoxStyle)
		SetButtonStyle(nIndex, TBBS_CHECKBOX | (check ? TBBS_CHECKED : 0));
	else
		SetButtonStyle(nIndex, TBBS_BUTTON);
}

//-------------------------------------------------------------------------------------
void CTBToolBar::CheckButton(UINT nCommandID, BOOL check)
{
	int nIndex = FindButton(nCommandID);
	if (nIndex < 0 || nIndex >= GetCount()) return;

	UINT style = GetButtonStyle(nIndex);
	ASSERT((style & TBBS_CHECKBOX) == TBBS_CHECKBOX);

	if (check)
		SetButtonStyle(nIndex, style | TBBS_CHECKED);
	else
		SetButtonStyle(nIndex, style & ~(TBBS_CHECKED));
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::AddComboBox(UINT nID, const CString& sButtonNameSpace, int nWidth, DWORD dwStyle, const CString& stLabel, INT_PTR iInsertAt, const CString& sPrompt/* = _T("")*/)
{
	CTBToolbarComboBoxButton comboBoxButton(nID, dwStyle, nWidth, sPrompt);
	//comboBoxButton.OnSize(nWidth);
	if (!stLabel.IsEmpty()) comboBoxButton.SetLabel(stLabel);
	InsertButtonInternal(comboBoxButton, iInsertAt);
	comboBoxButton.SetCenterVert();

	AddOslInfo(nID, sButtonNameSpace);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddComboBox(UINT nID, const CString& aLibNamespace, const CString& sName, int nWidth, DWORD dwStyle, const CString& stLabel, INT_PTR iInsertAt)
{
	// TB compatibility old vertical software
	return AddComboBox(nID, aLibNamespace + sName, nWidth, dwStyle, stLabel, iInsertAt);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddComboBoxEdit(UINT nID, const CString& sButtonNameSpace, int nWidth, const CString& stLabel /*= _T("")*/, INT_PTR iInsertAt/* = -1*/, const CString& sPrompt /*= _T("")*/, BOOL bSORT /*= TRUE*/)
{
	DWORD dwStyle = CBS_DROPDOWN | WS_CHILD | WS_VISIBLE | CBS_NOINTEGRALHEIGHT | WS_VSCROLL;
	if (bSORT)
		dwStyle |= CBS_SORT;

	return AddComboBox(nID, sButtonNameSpace, nWidth, dwStyle, stLabel, iInsertAt, sPrompt);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddComboBoxEditToRight(UINT nID, const CString& sButtonNameSpace, int nWidth, const CString& stLabel /*= _T("")*/, INT_PTR iInsertAt /*= -1*/, const CString& sPrompt /*= _T("")*/, BOOL bSORT /*= TRUE*/)
{
	AddRightSpaceObj();
	if (iInsertAt > -1)
		iInsertAt = GetRightSpacePos() + iInsertAt + 1;
	m_bToRight = TRUE;
	AddComboBoxEdit(nID, sButtonNameSpace, nWidth, stLabel, iInsertAt, sPrompt, bSORT);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::AddComboItem(UINT nID, const CString& item, DWORD_PTR dwData)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarComboBoxButton* comboBox = dynamic_cast<CTBToolbarComboBoxButton*> (this->GetButton(nIndex));
	int nIdx = comboBox->AddItem(item, dwData);
	return nIdx;
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::AddComboSortedItem(UINT nID, const CString& item, DWORD_PTR dwData)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarComboBoxButton* comboBox = dynamic_cast<CTBToolbarComboBoxButton*> (this->GetButton(nIndex));
	int nIdx = comboBox->AddSortedItem(item, dwData);
	return nIdx;
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::RemoveAllComboItems(UINT nID)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarComboBoxButton* comboBox = dynamic_cast<CTBToolbarComboBoxButton*> (this->GetButton(nIndex));
	comboBox->RemoveAllItems();
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL CTBToolBar::RemoveComboItem(UINT nID, INT nItem)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarComboBoxButton* comboBox = dynamic_cast<CTBToolbarComboBoxButton*> (this->GetButton(nIndex));
	comboBox->DeleteItem(nItem);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::RemoveComboItem(UINT nID, DWORD_PTR dwData)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarComboBoxButton* comboBox = dynamic_cast<CTBToolbarComboBoxButton*> (this->GetButton(nIndex));
	comboBox->DeleteItem(dwData);
	return TRUE;
}

//-----------------------------------------------------------------------------
INT CTBToolBar::GetComboCount(UINT nID)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return -1;
	CTBToolbarComboBoxButton* comboBox = dynamic_cast<CTBToolbarComboBoxButton*> (this->GetButton(nIndex));
	if (!comboBox)return 0;
	return comboBox->GetCount();
}

//-----------------------------------------------------------------------------
INT CTBToolBar::FindComboStringExact(UINT nID, const CString& sItem)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return -1;
	CTBToolbarComboBoxButton* comboBox = dynamic_cast<CTBToolbarComboBoxButton*> (this->GetButton(nIndex));
	if (!comboBox)return 0;
	for (int k = 0; k < comboBox->GetCount(); k++)
	{
		CString strItem = comboBox->GetItem(k);
		if (strItem.Compare(sItem) == 0)
			return k;
	}
	return -1;
}


//-----------------------------------------------------------------------------
BOOL CTBToolBar::SetComboItemSel(UINT nID, UINT nItem)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarComboBoxButton* comboBox = dynamic_cast<CTBToolbarComboBoxButton*> (this->GetButton(nIndex));
	if (!comboBox) return FALSE;
	comboBox->SelectItem(nItem, FALSE);
	RedrawWindow();
	return TRUE;
}

//-----------------------------------------------------------------------------
INT CTBToolBar::GetComboItemSel(UINT nID)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarComboBoxButton* comboBox = dynamic_cast<CTBToolbarComboBoxButton*> (this->GetButton(nIndex));
	if (!comboBox) return -1;
	return comboBox->GetCurSel();
}

//-----------------------------------------------------------------------------
DWORD_PTR CTBToolBar::GetComboItemData(UINT nID, UINT nItem)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarComboBoxButton* comboBox = dynamic_cast<CTBToolbarComboBoxButton*> (this->GetButton(nIndex));
	if (!comboBox) return -1;
	return comboBox->GetItemData(nItem);
}

//-----------------------------------------------------------------------------
LPCTSTR CTBToolBar::GetComboItemSelText(UINT nID)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarComboBoxButton* comboBox = dynamic_cast<CTBToolbarComboBoxButton*> (this->GetButton(nIndex));
	if (!comboBox) return NULL;
	return comboBox->GetText();
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddEditToRight(UINT nID, const CString& aLibNamespace, const CString& sName,
	int nWidth/* = 150*/, DWORD dwStyle /*= ES_AUTOHSCROLL*/, const CString& stLabel /*= _T("")*/, int nPos /*-1*/)
{
	return AddEditToRight(nID, aLibNamespace, sName, nWidth, dwStyle, stLabel, _T(""), nPos);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddEditToRight(UINT nID, const CString& aLibNamespace, const CString& sName,
	int nWidth/* = 150*/, DWORD dwStyle /*= ES_AUTOHSCROLL*/, const CString& stLabel /*= _T("")*/, const CString& sPrompt  /*= _T("")*/, int nPos /*-1*/)
{
	AddRightSpaceObj();
	if (nPos > -1)
		nPos = GetRightSpacePos() + nPos + 1;
	m_bToRight = TRUE;
	AddEdit(nID, aLibNamespace, sName, nWidth, dwStyle, stLabel, sPrompt, nPos);
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::AddEdit(UINT nID, const CString& aParentNamespace, const CString& sName /*= _T("")*/,
	int nWidth, DWORD dwStyle /*= ES_AUTOHSCROLL*/, const CString& stLabel /*= _T("")*/, const CString& sPrompt  /*= _T("")*/, int nPos /*-1*/)
{
	CTBToolbarEditBoxButton editBoxButton;
	editBoxButton.m_nID = nID;
	editBoxButton.OnSize(ScalePix(nWidth));
	editBoxButton.SetStyle(dwStyle);
	if (!sPrompt.IsEmpty())
		editBoxButton.SetPrompt(sPrompt);
	if (stLabel) editBoxButton.SetLabel(stLabel);
	InsertButtonInternal(editBoxButton, nPos);
	AddOslInfo(nID, aParentNamespace, sName);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::SetFont(UINT nID, CFont* pFont, BOOL bRedraw /*= TRUE*/)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarEditBoxButton* pEditBox = dynamic_cast<CTBToolbarEditBoxButton*> (this->GetButton(nIndex));
	if (pEditBox)
	{
		pEditBox->SetFont(pFont, bRedraw);
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::SetEditIcon(UINT nID, const CString& sImageNameSpace)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarEditBoxButton* pEditBox = dynamic_cast<CTBToolbarEditBoxButton*> (this->GetButton(nIndex));
	if (pEditBox)
	{
		HICON hIcon = LoadImageByNameSpace(sImageNameSpace, nID);
		pEditBox->SetIcon(hIcon);
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::SetEditIcon(UINT nID, UINT nIDBnIDBImage)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarEditBoxButton* pEditBox = dynamic_cast<CTBToolbarEditBoxButton*> (this->GetButton(nIndex));
	if (pEditBox)
	{
		HICON hIcon = TBLoadImage(nIDBnIDBImage);
		pEditBox->SetIcon(hIcon);
		return TRUE;
	}
	return FALSE;
}


//-----------------------------------------------------------------------------
BOOL CTBToolBar::SetEditSize(UINT nID, int iWidth, int iHeight)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarEditBoxButton* pEditBox = dynamic_cast<CTBToolbarEditBoxButton*> (this->GetButton(nIndex));
	if (!pEditBox) return FALSE;
	pEditBox->OnSize(iWidth);
	pEditBox->SetHeight(iHeight);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::SetTextContent(UINT nID, const CString& text)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;

	CBCGPToolbarEditBoxButton* editBox = dynamic_cast<CBCGPToolbarEditBoxButton*> (this->GetButton(nIndex));
	if (!editBox) return FALSE;
	editBox->SetContents(text);
	return TRUE;
}

//-----------------------------------------------------------------------------
CString CTBToolBar::GetTextContent(UINT nID)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CBCGPToolbarButton* pButton = this->GetButton(nIndex);
	CBCGPToolbarEditBoxButton* editBox = dynamic_cast<CBCGPToolbarEditBoxButton*> (pButton);
	if (!editBox) return _T("");
	return editBox->GetContentsAll(nID);
}

void CTBToolBar::SetTextFocus(UINT nID)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return;
	CBCGPToolbarButton* pButton = this->GetButton(nIndex);
	CBCGPToolbarEditBoxButton* editBox = dynamic_cast<CBCGPToolbarEditBoxButton*> (pButton);
	editBox->GetEditBox()->SetFocus();
}

// nAlign : TA_CENTER - TA_TOP - TA_BOTTOM
//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddLabelToRight(UINT nID, const CString& szText, int nPos /*-1*/, UINT nAlign /*= TA_BOTTOM*/)
{
	AddRightSpaceObj();
	if (nPos > -1)
		nPos = GetRightSpacePos() + nPos + 1;
	m_bToRight = TRUE;
	AddLabel(nID, szText, nPos, nAlign);
	return FALSE;
}

// nAlign : TA_CENTER - TA_TOP - TA_BOTTOM
//-----------------------------------------------------------------------------
BOOL CTBToolBar::AddLabel(UINT nID, const CString& szText, int nPos /*-1*/, UINT nAlign /*= TA_BOTTOM*/)
{
	if (!szText.IsEmpty())
	{
		CTBToolbarLabel tbLabel(nID, szText);
		tbLabel.SetTextAlign(nAlign);
		tbLabel.SetColorText(m_cTextColor);
		InsertButtonInternal(tbLabel, nPos);
		return TRUE;
	}
	return FALSE;
}

//-------------------------------------------------------------------------------------
CImageAssociation* CTBToolBar::GetImageAssociation(UINT nIDC)
{
	for (int i = 0; i <= m_Images.GetUpperBound(); i++)
	{
		CImageAssociation* pImage = dynamic_cast<CImageAssociation*>(m_Images.GetAt(i));
		ASSERT(pImage);
		if (pImage && pImage->GetCommandID() == nIDC)
			return pImage;
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
int CTBToolBar::GetImageFromImagesListIDImage(CString nImageNS)
{
	for (int i = 0; i <= m_Images.GetUpperBound(); i++)
	{
		CImageAssociation* pImage = dynamic_cast<CImageAssociation*> (m_Images.GetAt(i));
		ASSERT(pImage);
		if (pImage && pImage->GetImageNameSpace().Compare(nImageNS) == 0)
			return pImage->GetIndex();
	}
	return -1;
}

//-------------------------------------------------------------------------------------
int CTBToolBar::GetImageFromImagesListIDImage(UINT nIDImage)
{
	for (int i = 0; i <= m_Images.GetUpperBound(); i++)
	{
		CImageAssociation* pImage = dynamic_cast<CImageAssociation*> (m_Images.GetAt(i));
		ASSERT(pImage);
		if (pImage && pImage->GetImageID() == nIDImage)
			return pImage->GetIndex();
	}
	return -1;
}

//-------------------------------------------------------------------------------------
int CTBToolBar::GetImageFromImagesListIDC(UINT nIDC)
{
	for (int i = 0; i <= m_Images.GetUpperBound(); i++)
	{
		CImageAssociation* pImage = dynamic_cast<CImageAssociation*> (m_Images.GetAt(i));
		ASSERT(pImage);
		if (pImage && pImage->GetCommandID() == nIDC)
			return pImage->GetIndex();
	}
	return -1;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::RemoveImageFromImagesList(UINT nIDC)
{
	BOOL bFound = FALSE;
	for (int i = 0; i <= m_Images.GetUpperBound(); i++)
	{
		CImageAssociation* pImage = dynamic_cast<CImageAssociation*> (m_Images.GetAt(i));
		ASSERT(pImage);
		if (pImage && pImage->GetCommandID() == nIDC)
		{
			SAFE_DELETE(m_Images.GetAt(i));
			m_Images.RemoveAt(i);
			bFound = TRUE;
		}
	}

	return bFound;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::ChangeImage(UINT nID, UINT nIDImage, BOOL bPng)
{
	ChangeImage(nID, nIDImage, 0, bPng);
}

//-------------------------------------------------------------------------------------
void CTBToolBar::ChangeImage(UINT nID, UINT nIDImage, UINT nIDImageAlternative, BOOL bPng, UINT nIDCollapsedImag /*= 0*/)
{
	CDC* pDC = GetDC();
	if (nIDImage == 0 || nID == 0) return;
	if (nIDCollapsedImag > 0)
	{
		HICON hIco = TBLoadImage(pDC, NULL, nIDCollapsedImag, m_iToolbarButton_Width, bPng);
		AddCollapsedImage(nID, hIco);
		DestroyIcon(hIco);
	}
	else
	{
		HICON hIco;
		if (m_mapCollapsedImage.Lookup(nID, hIco))
		{
			HICON hIco = TBLoadImage(pDC, NULL, nIDImage, m_iToolbarButton_Width, bPng);
			AddCollapsedImage(nID, hIco);
			DestroyIcon(hIco);
		}
	}

	CImageAssociation* pImageList = GetImageAssociation(nID);
	int OldIDImage = 0;
	CString OldNsImage = _T("");
	CString OldNsButton = _T("");

	if (pImageList) {
		OldIDImage = pImageList->GetOldIdImage() == 0 ? pImageList->GetImageID() : pImageList->GetOldIdImage();
		OldNsImage = pImageList->GetOldNsImage().IsEmpty() ? pImageList->GetImageNameSpace() : pImageList->GetOldNsImage();
		OldNsButton = pImageList->GetButtonNameSpace();
	}


	// Load New Image
	RemoveImageFromImagesList(nID);
	HICON hIcon = TBLoadImage(pDC, NULL, nIDImage, m_iToolbarButton_Width, bPng);
	int nIndexImage = GetLockedImages()->AddIcon(hIcon);
	GetLockedDisabledImages()->AddIcon(hIcon);
	m_bRecalcGray = TRUE;
	::DestroyIcon(hIcon);
	// Find Button
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return;

	CImageAssociation* pImage = new CImageAssociation(nID, nIDImage, nIndexImage);
	pImage->SetCustom();

	pImage->SetOldIdImage(OldIDImage);
	pImage->SetOldNsImage(OldNsImage);
	pImage->SetButtonNameSpace(OldNsButton);

	if (nIDImageAlternative != 0)
	{
		HICON hAlterIcon = TBLoadImage(pDC, NULL, nIDImageAlternative, m_iToolbarButton_Width, bPng);
		int nAlterIndexImage = GetLockedImages()->AddIcon(hAlterIcon);
		GetLockedDisabledImages()->AddIcon(hAlterIcon);
		m_bRecalcGray = TRUE;
		::DestroyIcon(hAlterIcon);
		pImage->SetAlternativeIndexImage(nAlterIndexImage);
	}
	else
	{
		pImage->SetAlternativeIndexImage(-1);
	}

	m_Images.Add(pImage);
	ReleaseDC(pDC);
	// replace image in buttons
	CBCGPToolbarButton* pButton = dynamic_cast<CBCGPToolbarButton*> (this->GetButton(nIndex));
	if (!pButton) return;
	pButton->SetImage(nIndexImage);
	InvalidateButton(nIndex);
}

//-------------------------------------------------------------------------------------
void CTBToolBar::ChangeImage(UINT nID, const CString& sImageNameSpace, const CString& sImageAlternativeNameSpace, const CString& sCollapsedImageNameSpace /*= _T("")*/)
{
	if (sImageNameSpace.IsEmpty() || nID == 0) return;

	// Alternative image for collapsed button
	if (!sCollapsedImageNameSpace.IsEmpty())
	{
		HICON hIco = LoadImageByNameSpace(sCollapsedImageNameSpace);
		AddCollapsedImage(nID, hIco);
		DestroyIcon(hIco);
	}
	else
	{
		HICON hIco;
		if (m_mapCollapsedImage.Lookup(nID, hIco))
		{
			HICON hIco = LoadImageByNameSpace(sImageNameSpace);
			AddCollapsedImage(nID, hIco);
			DestroyIcon(hIco);
		}
	}

	CImageAssociation* pImageList = GetImageAssociation(nID);
	int OldIDImage = 0;
	CString OldNsImage = _T("");
	if (pImageList) {
		OldIDImage = pImageList->GetOldIdImage() == 0 ? pImageList->GetImageID() : pImageList->GetOldIdImage();
		OldNsImage = pImageList->GetOldNsImage().IsEmpty() ? pImageList->GetImageNameSpace() : pImageList->GetOldNsImage();
	}

	// Load New Image
	RemoveImageFromImagesList(nID);

	HICON hIcon = LoadImageByNameSpace(sImageNameSpace);
	int nIndexImage = GetLockedImages()->AddIcon(hIcon);
	GetLockedDisabledImages()->AddIcon(hIcon);
	m_bRecalcGray = TRUE;
	::DestroyIcon(hIcon);

	// Find Button
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return;

	CImageAssociation* pImage = new CImageAssociation(nID, sImageNameSpace, nIndexImage);
	pImage->SetCustom();
	pImage->SetOldIdImage(OldIDImage);
	pImage->SetOldNsImage(OldNsImage);

	if (!sImageAlternativeNameSpace.IsEmpty())
	{
		HICON hIcon = LoadImageByNameSpace(sImageAlternativeNameSpace);
		int nAlterIndexImage = GetLockedImages()->AddIcon(hIcon);
		GetLockedDisabledImages()->AddIcon(hIcon);
		m_bRecalcGray = TRUE;
		::DestroyIcon(hIcon);
		pImage->SetAlternativeIndexImage(nAlterIndexImage);
	}
	else
	{
		pImage->SetAlternativeIndexImage(-1);
	}

	m_Images.Add(pImage);

	// replace image in buttons
	CBCGPToolbarButton* pButton = dynamic_cast<CBCGPToolbarButton*> (this->GetButton(nIndex));
	if (!pButton) return;
	pButton->SetImage(nIndexImage);
	InvalidateButton(nIndex);
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetText(UINT nCommandID, LPCTSTR lpszText, LPCTSTR lpszTextAlternative /*= NULL*/)
{
	int nIdx = CommandToIndex(nCommandID);
	if (nIdx == -1)
	{
		//ASSERT(FALSE);
		return;
	}

	CString stText = lpszText;
	if (IsToolbarInfinity())
	{
		stText = stText.MakeUpper();
	}

	CImageAssociation* pImageList = GetImageAssociation(nCommandID);
	if (pImageList != NULL)
	{
		pImageList->SetCustomText();
		if (pImageList->GetOldIdImage() == 0)
		{
			pImageList->SetOldIdImage(pImageList->GetImageID());
		}
		pImageList->SetText(stText, lpszTextAlternative);
	}


	if (IsToolbarInfinity())
	{
		stText = stText.MakeUpper();
	}

	__super::SetButtonText(nIdx, stText);
	InvalidateButton(nIdx);
}

//-------------------------------------------------------------------------------------
int	CTBToolBar::SetButtonInfoChangeImage(UINT nIDC, const CString& sImageNameSpaceOLD)
{
	CImageAssociation* pImageList = GetImageAssociation(nIDC);
	if (pImageList == NULL || !pImageList->IsCustom()) return -1;

	int nImageAlter = pImageList->GetAlternativeIndexImage();
	if (pImageList->GetOldNsImage().Compare(sImageNameSpaceOLD))
		return pImageList->GetIndex();
	else
	{
		return nImageAlter;
	}
	return -1;
}

//-------------------------------------------------------------------------------------
int CTBToolBar::SetButtonInfoChangeImage(UINT nIDC, UINT nIDImageOLD)
{
	CImageAssociation* pImageList = GetImageAssociation(nIDC);
	if (pImageList == NULL || !pImageList->IsCustom()) return -1;

	int nImageAlter = pImageList->GetAlternativeIndexImage();
	if (pImageList->GetOldIdImage() == nIDImageOLD)
		return pImageList->GetIndex();
	else
	{
		return nImageAlter;
	}
	return -1;
}

//-------------------------------------------------------------------------------------
CString CTBToolBar::SetButtonInfoChangeText(UINT nIDC, UINT nIDImageOLD)
{
	CImageAssociation* pImageList = GetImageAssociation(nIDC);
	if (pImageList == NULL || !pImageList->IsCustomText()) return NULL;
	LPCTSTR pTextAlter = pImageList->GetTextAlternative();
	if (pImageList->GetOldIdImage() == nIDImageOLD)
		return pImageList->GetText();
	else
	{
		return pTextAlter;
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetButtonInfo(UINT nID, UINT nStyle, const CString& sImageNameSpace/* = _T("")*/, LPCTSTR lpszText /*= NULL*/, CString strTooltip /*= NULL*/)
{
	int nIndexImage = -1;

	if (sImageNameSpace.IsEmpty())
		return;
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return;

	nIndexImage = SetButtonInfoChangeImage(nID, sImageNameSpace);
	if (nIndexImage < 0)
	{ // ricerca per ID
		CImageAssociation* pImageList = GetImageAssociation(nID);
		ASSERT(pImageList);
		int nFind = pImageList->GetOldIdImage() == 0 ? pImageList->GetImageID() : pImageList->GetOldIdImage();
		nIndexImage = SetButtonInfoChangeImage(nID, nFind);
	}

	if (nIndexImage < 0) nIndexImage = GetImageFromImagesListIDImage(sImageNameSpace);

	if (nIndexImage < 0)
	{
		HICON hIcon = LoadImageByNameSpace(sImageNameSpace, nID);
		nIndexImage = GetLockedImages()->AddIcon(hIcon);
		GetLockedDisabledImages()->AddIcon(hIcon);
		m_bRecalcGray = TRUE;
		::DestroyIcon(hIcon);
		m_Images.Add(new CImageAssociation(nID, sImageNameSpace, nIndexImage));
	}

	if (lpszText)
	{
		if (IsToolbarInfinity())
		{
			CString upperStr = lpszText;
			upperStr = upperStr.MakeUpper();
			__super::SetButtonText(nIndex, upperStr);
		}
		else
			__super::SetButtonText(nIndex, lpszText);
	}

	if (strTooltip)
		SetTextToolTip(nIndex, strTooltip);

	if (nIndexImage < 0) return;
	__super::SetButtonInfo(nIndex, nID, nStyle, nIndexImage);
	InvalidateButton(nIndex);
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetButtonInfo(int nIndex, UINT nID, UINT nStyle, HICON hIcon)
{
	int nIndexImage = GetLockedImages()->AddIcon(hIcon);
	if (nIndexImage < 0) return;
	GetLockedDisabledImages()->AddIcon(hIcon);
	m_bRecalcGray = TRUE;
	__super::SetButtonInfo(nIndex, nID, nStyle, nIndexImage);
	InvalidateButton(nIndex);
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetButtonInfo(UINT nID, UINT nStyle, UINT nIDImage, LPCTSTR lpszText, BOOL bPng)
{
	if (nIDImage <= 0)
		return;

	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return;

	SetButtonInfo(nIndex, nID, nStyle, nIDImage, lpszText, bPng);
	InvalidateButton(nIndex);
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetButtonInfo(int nIndex, UINT nID, UINT nStyle, UINT nIDImage, LPCTSTR lpszText, BOOL bPng)
{
	int nIndexImage = -1;
	if (nIDImage <= 0 || nIndex >= m_Buttons.GetCount() || nIndex <= -1)
		return;

	nIndexImage = SetButtonInfoChangeImage(nID, nIDImage);
	if (nIndexImage < 0)
		nIndexImage = GetImageFromImagesListIDImage(nIDImage);

	if (nIndexImage < 0)
	{
		CDC* pDC = GetDC();
		HICON hIcon = hIcon = TBLoadImage(pDC, NULL, nIDImage, m_iToolbarButton_Width, bPng);
		nIndexImage = GetLockedImages()->AddIcon(hIcon);
		GetLockedDisabledImages()->AddIcon(hIcon);
		m_bRecalcGray = TRUE;
		ReleaseDC(pDC);
		::DestroyIcon(hIcon);
		m_Images.Add(new CImageAssociation(nID, nIDImage, nIndexImage));
	}

	CString pStr = SetButtonInfoChangeText(nID, nIDImage);

	if (IsToolbarInfinity() && !pStr.IsEmpty())
	{
		pStr = pStr.MakeUpper();
	}

	if (!pStr.IsEmpty())
	{
		__super::SetButtonText(nIndex, pStr);
	}
	else if (lpszText)
	{
		__super::SetButtonText(nIndex, lpszText);
	}

	if (nIndexImage < 0) return;
	__super::SetButtonInfo(nIndex, nID, nStyle, nIndexImage);
	InvalidateButton(nIndex);
}

//-------------------------------------------------------------------------------------
void CTBToolBar::AddRightSpaceObj()
{
	if (m_pObjectRight == NULL)
	{
		m_pObjectRight = new CTBToolbarLabel();
		m_pObjectRight->m_nID = -1;
		int nPos = __super::InsertButton(m_pObjectRight);
		m_pObjectRight->SetRightSpace();
		AddSeparator(nPos + 1);
	}
}

//-------------------------------------------------------------------------------------
void CTBToolBar::CenterButtons(BOOL bCenter/* = TRUE*/)
{
	m_bCenterButtons = bCenter;
	AddLeftSpaceObj();
}

//-------------------------------------------------------------------------------------
void CTBToolBar::AddLeftSpaceObj()
{
	if (m_pObjectLeft == NULL)
	{
		m_pObjectLeft = new CTBToolbarLabel();
		m_pObjectLeft->m_nID = -1;
		m_pObjectLeft->SetWidth(50);
		__super::InsertButton(m_pObjectLeft, 0);
	}
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButtonToRight(UINT nCommandID, const CString& sButtonNameSpace, HICON hIcon, const CString& szText, int nPos /*-1*/)
{
	AddRightSpaceObj();
	if (nPos > -1)
		nPos = GetRightSpacePos() + nPos + 1;
	m_bToRight = TRUE;
	AddButton(nCommandID, sButtonNameSpace, hIcon, szText, nPos);
	return TRUE;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::CloneButtonsAllTab()
{
	if (!m_pParentTabbedToolbar) return;

	CBCGPTabWnd*	pTabsWnd = m_pParentTabbedToolbar->GetUnderlinedWindow();
	int nTabCount = pTabsWnd->GetTabsNum();
	if (nTabCount < 2) return;
	INT nSwitch = -1;
	int nPosRightSpaceMasterBar = GetRightSpacePos();

	for (int i = 0; i < pTabsWnd->GetTabsNum(); i++)
	{
		CTBToolBar* pBar = dynamic_cast<CTBToolBar*> (pTabsWnd->GetTabWnd(i));
		if (!pBar || pBar == this)
			continue;

		for (int i = 0; i < GetCount(); i++)
		{
			CBCGPToolbarButton* pButton = GetButton(i);
			// CTBToolbarButton Clone Button
			if (pButton->IsKindOf(RUNTIME_CLASS(CTBToolbarButton)))
			{
				CTBToolbarButton* pTBButton = dynamic_cast<CTBToolbarButton*> (pButton);
				ASSERT(pTBButton);
				if (!pTBButton) break;
				// This is a clonable button ?
				if (pTBButton->IsClone())
				{
					// Get Button ID
					UINT  nID = pTBButton->m_nID;
					INT nFindButton = pBar->FindButton(nID);
					if (nFindButton >= 0) {
						continue;
					}

					BOOL bGhost = IsGhostButton(nID);
					BOOL bHidden = IsHideButton(nID);

					// Get Button Image
					HICON hIcon = GetLockedDisabledImages()->ExtractIcon(pTBButton->GetImage());
					// Get OSL info 
					CInfoOSLButton* pOslInfo = GetInfoOSLButton(nID);
					// Get Button NameSpace
					CString strNameSpace = pOslInfo->m_Namespace.ToString();
					// Get Button name
					CString strName = pTBButton->m_strText;
					// Position to insert the button
					int nPasInsert = i;
					if (nPasInsert < 0) nPasInsert = 0;

					// is a Right buttons ?
					if (nPosRightSpaceMasterBar != -1 && nPasInsert > nPosRightSpaceMasterBar)
					{
						nPasInsert = -1;
						pBar->AddButtonToRight(nID, strNameSpace, hIcon, strName, nPasInsert);
						continue;
					}

					// Add button
					// dasattivo l'auto posizione, ora lo metto nel prim posto disponibile
					pBar->AddButton(nID, strNameSpace, hIcon, strName, /*nPasInsert*/ -1, 0, TRUE);
				}
			}
		}
	}
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButtonToRightAllTab(UINT nCommandID, const CString& sButtonNameSpace, HICON hIcon, const CString& szText, int nPos /*-1*/)
{
	AddRightSpaceObj();
	if (nPos > -1)
		nPos = GetRightSpacePos() + nPos + 1;
	m_bToRight = TRUE;
	AddButton(nCommandID, sButtonNameSpace, hIcon, szText, nPos, 0, TRUE);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButtonToRight(UINT nCommandID, const CString& sButtonNameSpace, UINT nIDB /*0*/, const CString& szText /*NULL*/, BOOL bPNG /*FALSE*/, int nPos /*-1*/)
{
	AddRightSpaceObj();
	if (nPos > -1)
		nPos = GetRightSpacePos() + nPos + 1;
	m_bToRight = TRUE;
	AddButton(nCommandID, sButtonNameSpace, nIDB /*0*/, szText /*NULL*/, bPNG /*FALSE*/, nPos);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL  CTBToolBar::AddButtonToRight(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, int nPos /*-1*/)
{
	CDC* pDC = GetDC();
	BOOL ret = AddButtonToRight(nCommandID, sButtonNameSpace, LoadImageByNameSpace(sImageNameSpace, nCommandID), szText, nPos);
	ReleaseDC(pDC);

	for (int i = 0; i <= m_Images.GetUpperBound(); i++)
	{
		CImageAssociation* pImage = dynamic_cast<CImageAssociation*> (m_Images.GetAt(i));
		ASSERT(pImage);
		if (pImage && pImage->GetCommandID() == nCommandID)
		{
			pImage->SetImageNameSpace(sImageNameSpace);
			return TRUE;
		}
	}
	return ret;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButtonToRight(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, const CString& strToolTipText, int nPos/* = -1*/)
{
	if (!AddButtonToRight(nCommandID, sButtonNameSpace, sImageNameSpace, szText, nPos))
		return FALSE;
	SetTextToolTip(nCommandID, strToolTipText);
	return TRUE;
}

BOOL CTBToolBar::AddButtonToRightAllTab(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, const CString& strToolTipText, int nPos /*= -1*/)
{
	if (!AddButtonToRightAllTab(nCommandID, sButtonNameSpace, sImageNameSpace, szText, nPos)) return FALSE;
	SetTextToolTip(nCommandID, strToolTipText);
	CloneButtonsAllTab();
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButtonToRightAllTab(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, int nPos)
{
	CDC* pDC = GetDC();
	BOOL ret = AddButtonToRightAllTab(nCommandID, sButtonNameSpace,
		LoadImageByNameSpace(sImageNameSpace, nCommandID),
		szText, nPos);
	ReleaseDC(pDC);
	CloneButtonsAllTab();
	for (int i = 0; i <= m_Images.GetUpperBound(); i++)
	{
		CImageAssociation* pImage = dynamic_cast<CImageAssociation*> (m_Images.GetAt(i));
		ASSERT(pImage);
		if (pImage && pImage->GetCommandID() == nCommandID)
		{
			pImage->SetImageNameSpace(sImageNameSpace);
			return TRUE;
		}
	}
	return ret;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButtonToRight(UINT nCommandID, const CString& sButtonNameSpace, const CString& sName, UINT nIDB, const CString& szText, BOOL bPNG, int nPos /*-1*/)
{
	// TB compatibility old vertical software
	return AddButtonToRight(nCommandID, sButtonNameSpace + _T(".") + sName, nIDB, szText, bPNG, nPos);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButtonToRight(UINT nCommandID, const CString& sButtonNameSpace, const CString& sName, HICON hIcon, const CString& szText, int nPos /*-1*/)
{
	// TB compatibility old vertical software
	return AddButtonToRight(nCommandID, sButtonNameSpace + _T(".") + sName, hIcon, szText, nPos);
}

//-------------------------------------------------------------------------------------
int CTBToolBar::GetRightSpacePos()
{
	AddRightSpaceObj();
	for (int i = 0; i < GetCount(); i++)
	{
		CBCGPToolbarButton* pButton = GetButton(i);
		if (pButton->IsKindOf(RUNTIME_CLASS(CTBToolbarLabel)) &&
			((CTBToolbarLabel*)pButton)->IsRightSpace()
			)
			return i;
	}
	return -1;
}

//-------------------------------------------------------------------------------------
int CTBToolBar::InsertButtonPtr(CBCGPToolbarButton* pButton, INT_PTR iInsertAt /*= -1*/)
{
	return InsertButton(pButton, iInsertAt);
}

//-------------------------------------------------------------------------------------
int CTBToolBar::InsertButtonInternal(const CBCGPToolbarButton& button, INT_PTR iInsertAt /* = -1*/)
{
	int nInsert = -1;
	// Toolbar in style Infinity
	if (IsToolbarInfinity())
	{
		AddRightSpaceObj();
		int nSwitch = -1;
		if (m_pParentTabbedToolbar)
		{
			nSwitch = FindButton(m_pParentTabbedToolbar->GetIDSwitch());
		}

		if (iInsertAt == -1)
		{
			int nInsert = GetCount();
			if (m_bToRight)
			{
				nInsert = m_nLastRight = GetCount();
			}
			else if (m_nLastRight >= 0)
			{
				nInsert = m_nLastRight - 1;
				if (nInsert < 1) nInsert = 1;
			}

			// the switch button is last to left
			if (m_pParentTabbedToolbar && button.m_nID == m_pParentTabbedToolbar->GetIDSwitch())
			{
				nInsert = GetCount();
			}

			m_bToRight = FALSE;

			if (nSwitch >= 0 && nInsert >= nSwitch)
				nInsert = nSwitch;

			nInsert = __super::InsertButton(button, nInsert);
			ASSERT(nInsert > -1);
			return nInsert;
		}
		else
		{
			int nCount = GetCount();
			if (iInsertAt == 0)
			{
				iInsertAt = nCount;
			}

			if (nSwitch >= 0 && iInsertAt >= nSwitch)
				iInsertAt = nSwitch;
		}
	}

	if (iInsertAt == -1)
	{
		int iPlaceRight = -1;
		if (m_pObjectRight && !m_bToRight)
			iPlaceRight = GetRightSpacePos();
		m_bToRight = FALSE;
		if (iPlaceRight > -1)
			return __super::InsertButton(button, iPlaceRight);
	}

	nInsert = __super::InsertButton(button, iInsertAt);
	ASSERT(nInsert > -1);
	return nInsert;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::AdjustLayout()
{

	if (IsLayoutSuspended() || (m_pParentTabbedToolbar && m_pParentTabbedToolbar->IsLayoutSuspended()))
		return;
	// se la finestra non e' visibile e non e' in una dialog
	// aspetto a ripristinare l'Adjust
	if (!m_bDialog && !IsTBWindowVisible(this) || !m_hWnd)
		return;


	/*if (IsToolbarInfinity())
	{
		m_bTextLabels = FALSE;
	}*/

	__super::AdjustLayout();
}

//-------------------------------------------------------------------------------------
CImageAssociation* CTBToolBar::AddButton(UINT nCommandID, const CString& sButtonNameSpace,
	HICON hIcon, const CString& szText, int nPos, UINT nIDB /* 0*/, BOOL bClone/* = FALSE*/)
{
	// Is perent a castom immage in theme config ?
	int nIdx = 0;
	BOOL bLoadStandardImage = TRUE;
	CString sCustomImage = AfxGetThemeManager()->GetNameSpaceToolBarImage(sButtonNameSpace);
	if (!sCustomImage.IsEmpty())
	{
		HICON iconCustom = LoadImageByNameSpace(sCustomImage, nCommandID);

		if (iconCustom)
		{
			nIdx = GetLockedImages()->AddIcon(iconCustom);
			GetLockedDisabledImages()->AddIcon(iconCustom);
			bLoadStandardImage = FALSE;
			m_bRecalcGray = TRUE;
		}
		::DestroyIcon(iconCustom);
	}

	if (!AfxIsRemoteInterface())
	{
		if (bLoadStandardImage)
		{
			nIdx = GetLockedImages()->AddIcon(hIcon);
			GetLockedDisabledImages()->AddIcon(hIcon);
			m_bRecalcGray = TRUE;
		}

		::DestroyIcon(hIcon);
	}

	CTBToolbarButton  button;
	button.m_nID = nCommandID;
	button.SetImage(nIdx);
	button.SetClone(bClone);

	CImageAssociation* pAssociation = new CImageAssociation(nCommandID, nIDB, nIdx);
	pAssociation->SetButtonNameSpace(sButtonNameSpace);
	m_Images.Add(pAssociation);

	if (!szText.IsEmpty())
	{
		if (IsToolbarInfinity())
		{
			CString stTmp = szText;
			button.m_strText = stTmp.MakeUpper();
		}
		else
			button.m_strText = szText;
	}
	else
	{
		// empty text is allowed: prevents standard BCG behavior which use tooltip text for it
		button.m_strText.Empty();
		button.SetWantText(FALSE);
	}

	InsertButtonInternal(button, nPos);
	AddOslInfo(nCommandID, sButtonNameSpace);

	//Enable Text Below Button
	OnTextBelowButton();

	AdjustLayout();
	return pAssociation;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButton(UINT nCommandID,
	const CString& sButtonNameSpace,
	UINT nIDB /*0*/,
	const CString& szText /*NULL*/,
	BOOL bPNG /*FALSE*/, int nPos /*= -1*/)
{
	// Load Image
	CDC* pDC = GetDC();
	HICON hIcon = TBLoadImage(pDC, ImageNameSpaceWalking(sButtonNameSpace), nIDB, m_iToolbarButton_Width, bPNG);
	ReleaseDC(pDC);
	CImageAssociation* p = AddButton(nCommandID, sButtonNameSpace, hIcon, szText, nPos, nIDB);
	::DestroyIcon(hIcon);
	// END Load Image
	return p != NULL;
}

// Add Clone button
//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButtonAllTab(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, const CString& strToolTipText, int nPos /*= -1*/)
{
	CImageAssociation* pImage = AddButton(nCommandID, sButtonNameSpace, LoadImageByNameSpace(sImageNameSpace, nCommandID), szText, nPos, 0, TRUE);
	ASSERT(pImage);
	pImage->SetImageNameSpace(sImageNameSpace);
	SetTextToolTip(nCommandID, strToolTipText);
	CloneButtonsAllTab();
	return pImage != NULL;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButtonAllTab(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace /*= _T("")*/, const CString& szText /*= _T("")*/, int nPos /*= -1*/)
{
	CImageAssociation* pImage = AddButton(nCommandID, sButtonNameSpace, LoadImageByNameSpace(sImageNameSpace, nCommandID), szText, nPos, 0, TRUE);
	ASSERT(pImage);
	pImage->SetImageNameSpace(sImageNameSpace);
	CloneButtonsAllTab();
	return pImage != NULL;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButton(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, const CString& strToolTipText, int nPos /*= -1*/)
{
	if (!AddButton(nCommandID, sButtonNameSpace, sImageNameSpace, szText, nPos))
		return FALSE;
	SetTextToolTip(nCommandID, strToolTipText);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButton(UINT nCommandID,
	const CString& sButtonNameSpace,
	const CString& sImageNameSpace,
	const CString& szText,
	int nPos)
{
	CImageAssociation* pImage = AddButton(nCommandID, sButtonNameSpace, LoadImageByNameSpace(sImageNameSpace, nCommandID), szText, nPos);
	ASSERT(pImage);
	pImage->SetImageNameSpace(sImageNameSpace);

	return pImage != NULL;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButton(UINT nCommandID, const CString& sButtonNameSpace, const CString& sName, UINT nIDB, const CString& szText, BOOL bPNG)
{
	// TB compatibility old vertical software
	return AddButton(nCommandID, sButtonNameSpace + _T(".") + sName, nIDB, szText, bPNG);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddButton(UINT nCommandID, const CString& sButtonNameSpace, const CString& sName, HICON hIcon, const CString& szText)
{
	// TB compatibility old vertical software
	return NULL != AddButton(nCommandID, sButtonNameSpace + _T(".") + sName, hIcon, szText);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::MoveButton(UINT nCommandID, UINT nPos)
{
	int nButtonPos = FindButton(nCommandID);
	if (nButtonPos < 0 || nPos < 0) return FALSE;
	if (nButtonPos == nPos) return FALSE;

	CBCGPToolbarButton* pBCGButton = GetButton(nButtonPos);
	ASSERT(pBCGButton);
	if (!pBCGButton) return FALSE;
	int nImg = pBCGButton->GetImage();	// Image
	BOOL isVisible = pBCGButton->IsVisible();
	CString sCustomToolTip = pBCGButton->m_strTextCustom;	// ToolTip
	CString sText = pBCGButton->m_strText;

	if (pBCGButton->IsKindOf(RUNTIME_CLASS(CTBToolbarButton)))
	{
		// Button
		CTBToolbarButton* pButton = dynamic_cast<CTBToolbarButton*> (pBCGButton);
		if (pButton)
		{
			CTBToolbarButton  button;
			button.m_nID = nCommandID;
			button.SetImage(nImg);
			button.SetClone(pButton->IsClone());
			button.m_strText = sText;
			button.m_strTextCustom = sCustomToolTip;
			button.SetVisible(isVisible);
			if (sText.IsEmpty()) button.SetWantText(FALSE);

			__super::RemoveButton(nButtonPos);	// remove old buton
			if (nPos > (UINT)nButtonPos) nPos--;

			InsertButtonInternal(button, nPos); // add button
			return TRUE;
		}
	}
	else if (pBCGButton->IsKindOf(RUNTIME_CLASS(CTBToolbarMenuButton)))
	{
		CTBToolbarMenuButton* pMenuButton = dynamic_cast<CTBToolbarMenuButton*> (pBCGButton);
		if (pMenuButton)
		{
			CTBToolBarMenu tmpMenu;
			HMENU hMenu = pMenuButton->GetMenu();
			if (hMenu) {
				tmpMenu.Attach(hMenu);
			}
			else {
				tmpMenu.CreateMenu();
				AppendMenu(tmpMenu, MF_BYCOMMAND | MF_ENABLED, -1, _T(""));
			}

			CTBToolbarMenuButton menuButton;
			menuButton.m_nID = nCommandID;
			menuButton.SetImage(nImg);
			menuButton.m_strText = sText;
			menuButton.m_strTextCustom = sCustomToolTip;
			menuButton.m_bTextBelow = TRUE;
			menuButton.TextBelow();
			menuButton.CreateMenu(&tmpMenu);
			__super::RemoveButton(nButtonPos);	// remove old buton
			if (nPos > (UINT)nButtonPos) nPos--;
			InsertButtonInternal(menuButton, nPos); // add button
			return TRUE;
		}
	}
	else if (pBCGButton->IsKindOf(RUNTIME_CLASS(CTBToolbarLabel)))
	{
		CTBToolbarLabel tbLabel(nCommandID, sText);
		__super::RemoveButton(nButtonPos);	// remove old buton
		if (nPos > (UINT)nButtonPos) nPos--;
		InsertButtonInternal(tbLabel, nPos);
	}
	else
	{
		ASSERT(FALSE);
	}

	return FALSE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::MoveButtonBefore(UINT nCommandID, UINT nIDInsertPos /*= 0*/)
{
	ASSERT(nCommandID != nIDInsertPos);
	int nIndex = FindButton(nIDInsertPos);
	if (nIndex < 0) return FALSE;
	return MoveButton(nCommandID, nIndex);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::MoveButtonAfter(UINT nCommandID, UINT nIDInsertPos /*= 0*/)
{
	ASSERT(nCommandID != nIDInsertPos);
	int nIndex = FindButton(nIDInsertPos);
	if (nIndex < 0) return FALSE;
	return MoveButton(nCommandID, nIndex + 1);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::InsertButtonAfter(UINT nCommandID, const CString& sButtonNameSpace, UINT nIDB, const CString& szText, BOOL bPNG, UINT nIDInsertPos)
{
	int nIndex = FindButton(nIDInsertPos);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	AddButton(nCommandID, sButtonNameSpace, nIDB, szText, bPNG, nIndex + 1);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::InsertButtonBefore(UINT nCommandID, const CString& sButtonNameSpace, UINT nIDB, const CString& szText, BOOL bPNG, UINT nIDInsertPos)
{
	int nIndex = FindButton(nIDInsertPos);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	AddButton(nCommandID, sButtonNameSpace, nIDB, szText, bPNG, nIndex);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::InsertButtonAfter(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, int nIDInsertPos)
{
	int nIndex = FindButton(nIDInsertPos);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	AddButton(nCommandID, sButtonNameSpace, sImageNameSpace, szText, nIndex + 1);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::InsertButtonBefore(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, int nIDInsertPos)
{
	int nIndex = FindButton(nIDInsertPos);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	AddButton(nCommandID, sButtonNameSpace, sImageNameSpace, szText, nIndex);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddSeparatorAfter(UINT nIDInsertPos)
{
	// Toolbar in style Infinity not present
	if (IsToolbarInfinity()) return FALSE;

	int nIndex = FindButton(nIDInsertPos);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	AddSeparator(nIndex + 1);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddSeparatorBefore(UINT nIDInsertPos)
{
	// Toolbar in style Infinity not present
	if (IsToolbarInfinity()) return FALSE;

	int nIndex = FindButton(nIDInsertPos);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	AddSeparator(nIndex);
	return TRUE;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::OnTextBelowButton()
{
	this->EnableTextLabels();
	m_bTBelow = TRUE;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::OnTextBelowButton(UINT nCommandID, const LPCTSTR pStrText)
{
	int nIndex = FindButton(nCommandID);
	if (nIndex < 0 || nIndex >= GetCount()) return;

	CBCGPToolbarButton* pButton = dynamic_cast<CBCGPToolbarButton*> (this->GetButton(nIndex));
	if (!pButton) return;

	if (pStrText)
		pButton->m_strText = pStrText;

	this->EnableTextLabels();
	m_bTBelow = TRUE;
}

//----------------------------------------------------
void CTBToolBar::OnChangeVisualManager()
{
	for (int i = 0; i <= m_Images.GetUpperBound(); i++)
	{
		CImageAssociation* pImage = dynamic_cast<CImageAssociation*>(m_Images.GetAt(i));
		ASSERT(pImage);
		if (!pImage)
			continue;
		int nIdx = GetLockedImages()->AddImage((HBITMAP)pImage);
		GetLockedDisabledImages()->AddImage((HBITMAP)pImage);
		m_bRecalcGray = TRUE;
		int n = CommandToIndex(pImage->GetCommandID());
		CBCGPToolbarButton* pButton = GetButton(n);
		pButton->SetImage(nIdx);
	}
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddDropdown(
	UINT nCommandID,
	const CString& sButtonNameSpace,
	const CString& sImageNameSpace /*= _T("")*/,
	const CString& szText /*= _T("")*/,
	CMenu* mMenu /*= NULL*/, int nPos /*= -1*/)
{
	CTBToolbarMenuButton menuButton;

	if (!mMenu)
	{
		CTBToolBarMenu tmpMenu;
		tmpMenu.CreateMenu();
		AppendMenu(tmpMenu, MF_BYCOMMAND | MF_ENABLED, -1, _T(""));
		menuButton.CreateMenu(&tmpMenu);
	}
	else
	{
		menuButton.CreateMenu(mMenu);
	}

	menuButton.m_nID = nCommandID;
	if (!sImageNameSpace.IsEmpty())
	{
		HICON hIcon = LoadImageByNameSpace(sImageNameSpace, nCommandID);
		int iImag = GetLockedImages()->AddIcon(hIcon);
		GetLockedDisabledImages()->AddIcon(hIcon);
		m_bRecalcGray = TRUE;
		::DestroyIcon(hIcon);
		menuButton.SetImage(iImag);
	}

	// Titolo sotto le ribbonBar.! si deve forzare nell onDraw!
	menuButton.m_bTextBelow = TRUE;
	if (m_bTBelow)	menuButton.TextBelow();
	if (szText) {

		CString stText = szText;
		if (IsToolbarInfinity())
		{
			stText = stText.MakeUpper();
		}

		menuButton.m_strText = stText;
	}

	InsertButtonInternal(menuButton, nPos);
	AddOslInfo(nCommandID, sButtonNameSpace);

	//Enable Text Below Button
	OnTextBelowButton();

	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::SetDropdown(UINT nCommandID, CMenu* mMenu, LPCTSTR lpszText)
{
	int nIdx = CommandToIndex(nCommandID);
	if (nIdx == -1)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bIsVisible = GetButton(nIdx)->IsVisible();

	int iImag = this->GetButton(nIdx)->GetImage();

	CTBToolbarButton* pButton = dynamic_cast<CTBToolbarButton*> (this->GetButton(nIdx));

	CString sCustomToolTip = _T("");
	if (pButton)
	{
		sCustomToolTip = pButton->m_strTextCustom;
	}

	CTBToolbarMenuButton menuButton;

	if (!mMenu)
	{
		CTBToolBarMenu tmpMenu;
		tmpMenu.CreateMenu();
		AppendMenu(tmpMenu, MF_BYCOMMAND | MF_ENABLED, -1, _T(""));
		menuButton.CreateMenu(&tmpMenu);
	}
	else
	{
		menuButton.CreateMenu(mMenu);
	}

	if (!sCustomToolTip.IsEmpty()) {
		menuButton.m_strTextCustom = sCustomToolTip;
	}

	menuButton.m_nID = nCommandID;
	menuButton.SetImage(iImag);
	// Titolo sotto le ribbonBar.! si deve forzare nell onDraw!
	menuButton.m_bTextBelow = TRUE;

	if (m_bTBelow)
		menuButton.TextBelow();

	if (lpszText)
	{
		menuButton.m_strText = lpszText;
	}
	else
	{
		if (!this->GetButton(nIdx)->m_strText.IsEmpty())
			menuButton.m_strText = this->GetButton(nIdx)->m_strText;
	}

	this->ReplaceButton(nCommandID, menuButton);
	HideButton(nCommandID, !bIsVisible);
	AdjustLayout();
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::UpdateDropdownMenu(UINT nCommandID, CMenu* pMenu)
{
	IconsDropDownClean();
	int nIndex = FindButton(nCommandID);
	ASSERT(nIndex >= 0);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarMenuButton* menuButton = dynamic_cast<CTBToolbarMenuButton*> (this->GetButton(nIndex));
	if (!menuButton) return FALSE;
	CTBToolBarMenu* pToolBarMenu = dynamic_cast<CTBToolBarMenu*> (pMenu);
	if (pToolBarMenu)
	{
		CDC* pDC = GetDC();
		pToolBarMenu->SetDC(pDC);
		int iCount = (int)pToolBarMenu->GetMenuItemCount();
		for (int i = 0; i < iCount; i++)
		{
			UINT nID = pMenu->GetMenuItemID(i);

			HICON hIcoUnChecked = CopyIcon(pToolBarMenu->GetIconUnChecked(nID));
			HICON hIcoChecked = CopyIcon(pToolBarMenu->GetIconUnChecked(nID));

			m_mapDropDownIconsUnChecked.SetAt(nID, hIcoUnChecked);
			m_mapDropDownIconsChecked.SetAt(nID, hIcoChecked);
		}
		pToolBarMenu->SetDC(NULL);
		ReleaseDC(pDC);
	}
	menuButton->CreateMenu(pMenu);
	return TRUE;
}

//-------------------------------------------------------------------------------------
HMENU CTBToolBar::CopyDropdownMenu(UINT nCommandID)
{
	int nIndex = FindButton(nCommandID);
	ASSERT(nIndex >= 0);
	if (nIndex < 0 || nIndex >= GetCount()) return NULL;
	CTBToolbarMenuButton* menuButton = dynamic_cast<CTBToolbarMenuButton*> (this->GetButton(nIndex));
	if (!menuButton) return NULL;
	return menuButton->GetMenu();
}

//Implemented with same logic of CTBToolBar::AddDropdownMenuItem method (see below)
//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddDropdownMenuItemSeparator(UINT nCommandID)
{
	int nIndex = FindButton(nCommandID);
	ASSERT(nIndex >= 0);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarMenuButton* menuButton = dynamic_cast<CTBToolbarMenuButton*> (this->GetButton(nIndex));
	if (!menuButton) return FALSE;

	CTBToolBarMenu tmpMenu;
	HMENU hMenu = menuButton->GetMenu();
	if (!hMenu) {
		return FALSE;
	}

	tmpMenu.Attach(hMenu);
	tmpMenu.AppendMenu(MF_MENUBARBREAK);

	UpdateDropdownMenu(nCommandID, &tmpMenu);
	tmpMenu.Detach();
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::AddDropdownMenuItem(UINT nCommandID, UINT nFlags, UINT_PTR nIDNewItem /*= NULL*/, LPCTSTR lpszNewItem /*= NULL*/, UINT nIDImgUnchecked /*= 0*/, UINT nIDImgChecked/* = 0*/, BOOL bPng /*= TRUE*/)
{
	int nIndex = FindButton(nCommandID);
	ASSERT(nIndex >= 0);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarMenuButton* menuButton = dynamic_cast<CTBToolbarMenuButton*> (this->GetButton(nIndex));
	if (!menuButton) return FALSE;

	// Append voice in menu
	CTBToolBarMenu tmpMenu;
	HMENU hMenu = menuButton->GetMenu();
	if (hMenu) {
		tmpMenu.Attach(hMenu);

		// Rimozione voce fitizzia se presente
		if (tmpMenu.GetMenuItemCount() > 0)
		{
			CString menuStr;
			tmpMenu.GetMenuString(0, menuStr, MF_BYPOSITION);
			if (menuStr.IsEmpty())
			{
				tmpMenu.DeleteMenu(0, MF_BYPOSITION);
			}
		}
	}
	else {
		tmpMenu.CreateMenu();
	}

	if (tmpMenu.ExistID(nIDNewItem))
	{
		tmpMenu.Detach();
		return FALSE;
	}

	tmpMenu.AppendMenu(nFlags, nIDNewItem, lpszNewItem, nIDImgUnchecked, nIDImgChecked, bPng);

	UpdateDropdownMenu(nCommandID, &tmpMenu);
	tmpMenu.Detach();
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::InsertDropdownMenuItem(UINT nPos, UINT nCommandID, UINT nFlags, UINT_PTR nIDNewItem /*= NULL*/, LPCTSTR lpszNewItem /*= NULL*/, UINT nIDImgUnchecked /*= 0*/, UINT nIDImgChecked/* = 0*/, BOOL bPng /*= TRUE*/)
{
	int nIndex = FindButton(nCommandID);
	ASSERT(nIndex >= 0);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarMenuButton* menuButton = dynamic_cast<CTBToolbarMenuButton*> (this->GetButton(nIndex));
	if (!menuButton) return FALSE;

	// Append voice in menu
	CTBToolBarMenu tmpMenu;
	HMENU hMenu = menuButton->GetMenu();
	if (hMenu) {
		tmpMenu.Attach(hMenu);
	}
	else {
		tmpMenu.CreateMenu();
	}

	if (tmpMenu.ExistID(nIDNewItem))
		return FALSE;

	tmpMenu.InsertMenu(nPos, nFlags, nIDNewItem, lpszNewItem, nIDImgUnchecked, nIDImgChecked, bPng);
	UpdateDropdownMenu(nCommandID, &tmpMenu);
	tmpMenu.Detach();
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::RemoveDropdown(UINT nCommandID)
{
	int nIdx = CommandToIndex(nCommandID);
	if (nIdx == -1)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	CBCGPToolbarButton* pButton = GetButton(nIdx);
	int iImag = pButton->GetImage();

	CTBToolbarButton  button;
	button.m_nID = nCommandID;
	button.SetImage(iImag);

	// last operation to execute!
	this->ReplaceButton(nCommandID, button);
	return TRUE;
}

//------------------------------------------------------------------------------------ -
BOOL CTBToolBar::RemoveButtonForID(UINT nCommandID)
{
	// Remove button
	int iPos = FindButton(nCommandID);
	if (iPos >= 0)
	{
		BOOL bRet = __super::RemoveButton(iPos);
		AdjustLayout();
		if (bRet) {
			m_removedListID.AddTail(nCommandID);
		}

		for (INT k = 0; k < m_ListCollapsedItems.GetCount(); k++)
		{
			POSITION pos = m_ListCollapsedItems.FindIndex(k);
			if (!pos) continue;
			CTBCollapsedItem* pItem = m_ListCollapsedItems.GetAt(pos);
			if (nCommandID == pItem->GetID())
			{
				m_ListCollapsedItems.RemoveAt(pos);
			}
		}
		return bRet;
	}
	return FALSE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::EnableAlwaysDropDown(UINT nCommandID, BOOL bAlwaysDropDown /*= TRUE*/)
{
	int nIndex = FindButton(nCommandID);
	ASSERT(nIndex >= 0);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CTBToolbarMenuButton* pMenuButton = dynamic_cast<CTBToolbarMenuButton*> (this->GetButton(nIndex));
	if (!pMenuButton) return FALSE;
	pMenuButton->EnableAlwaysDropDown(bAlwaysDropDown);
	return TRUE;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetAlwaysDropDown(UINT nCommandID, int nAlwaysDropDown)
{
	int nIndex = FindButton(nCommandID);

	if (nIndex < 0 || nIndex >= GetCount())
		return;
	CTBToolbarMenuButton* pMenuButton = dynamic_cast<CTBToolbarMenuButton*> (this->GetButton(nIndex));
	if (pMenuButton)
		pMenuButton->SetAlwaysDropDown(nAlwaysDropDown);
}

//-------------------------------------------------------------------------------------
HICON CTBToolBar::GetIconUnCheckedDropdown(UINT nID)
{
	HICON hIcon = NULL;
	if (m_mapDropDownIconsUnChecked.Lookup(nID, hIcon))
		return hIcon;
	return NULL;
}

//-------------------------------------------------------------------------------------
HICON CTBToolBar::GetIconCheckedDropdown(UINT nID)
{
	HICON hIcon = NULL;
	if (m_mapDropDownIconsChecked.Lookup(nID, hIcon))
		return hIcon;
	return NULL;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetGroup(UINT nCommandID)
{
	int nIdx = CommandToIndex(nCommandID);
	if (nIdx == -1)
	{
		ASSERT(FALSE);
		return;
	}

	SetButtonStyle(nIdx, GetButtonStyle(nIdx) | TBBS_GROUP);
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::IsCloneButton(UINT nID)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	CBCGPToolbarButton* pButton = this->GetButton(nIndex);
	ASSERT(pButton);
	CTBToolbarButton* pTButton = DYNAMIC_DOWNCAST(CTBToolbarButton, pButton);
	if (pTButton == NULL) return FALSE;
	return pTButton->IsClone();
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::IsHideButton(UINT nID)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	return !this->GetButton(nIndex)->IsVisible();
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::IsGhostButton(UINT nID)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;

	CTBToolbarButton* pTButton = DYNAMIC_DOWNCAST(CTBToolbarButton, this->GetButton(nIndex));
	if (pTButton)
	{
		return pTButton->IsGhost();
	}
	return FALSE;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::HideButton(UINT nID, BOOL bHide /*= TRUE*/, BOOL bDisableAutoHide /*= FALSE*/)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount())
		return FALSE;

	CBCGPToolbarButton* pBtn = this->GetButton(nIndex);
	ASSERT_VALID(pBtn);

	CInfoOSLButton* pInfoOSL = GetInfoOSLButton(pBtn->m_nID);
	if (pInfoOSL)
	{
		if (!bHide && !OSL_CAN_DO(pInfoOSL, OSL_GRANT_EXECUTE))
			bHide = TRUE;
	}


	if (bDisableAutoHide)
	{
		CTBToolbarButton* pTBToolBar = dynamic_cast<CTBToolbarButton*> (pBtn);
		if (pTBToolBar)
			pTBToolBar->SetAutoHide(FALSE);
	}

	//se arivo con bHide(FALSE) verifico se c'e' nella lista dei collapsed items
	//se c'e' lo elimino (si vede che qualcuno lo vuole nascosto, quindi lo levo dalla mappa, altrimenti ButtonsOverlapRemove lo mette visibile)
	if (bHide && ButtonsInOverlapList(pBtn->m_nID))
		ButtonsOverlapRemoveItem(pBtn->m_nID);

	BOOL bVisible = pBtn->IsVisible();
	if (bVisible == (!bHide))
	{
		return TRUE;
	}

	pBtn->SetVisible(!bHide);

	if (m_bDialog && m_pObjectRight && !m_bAutoHideToolBarButton)
	{
		AdjustLayout();
	}

	// Logiche sui AddSeparator
	// il separatore successivo ha la stessa sorte del bottone alla sua sinistra
	if ((nIndex + 1) < this->GetCount())
	{
		CBCGPToolbarButton* pButton = this->GetButton(nIndex + 1);
		if (pButton->m_nStyle & TBBS_SEPARATOR)
		{
			pButton->SetVisible(TRUE);
			for (int j = nIndex; j != 0; j--)
			{
				CBCGPToolbarButton* pButtonR = this->GetButton(j);
				if (m_pObjectRight == pButtonR)
					break;

				if (j == 0 && !pButtonR->IsVisible())
				{
					pButton->SetVisible(!bHide);
					break;
				}

				if (!(pButtonR->m_nStyle & TBBS_SEPARATOR) && !pButtonR->IsVisible())
					continue;

				if ((pButtonR->m_nStyle & TBBS_SEPARATOR) && pButtonR->IsVisible())
				{
					pButton->SetVisible(!bHide);
					break;
				}
			}
		}
	}

	SendMessageToolBarUpdate();
	return TRUE;
}

//----------------------------------------------------------------------------
HICON CTBToolBar::GetIconDropdownMenu(const CBCGPToolbarMenuButton* pMenuButton)
{
	HICON hIcon = GetOverlapIconDropdownMenu(pMenuButton);
	if (hIcon)
		return hIcon;

	if (
		((pMenuButton->m_nStyle & TBBS_CHECKED) == TBBS_CHECKED) &&
		((pMenuButton->m_nStyle & TBBS_CHECKBOX) == TBBS_CHECKBOX)
		)
	{
		hIcon = GetIconCheckedDropdown(pMenuButton->m_nID);
		if (hIcon) return hIcon;
	}
	else
	{
		hIcon = GetIconUnCheckedDropdown(pMenuButton->m_nID);
		if (hIcon) return hIcon;
		hIcon = GetIconCheckedDropdown(pMenuButton->m_nID);
		if (hIcon) return hIcon;
	}

	return NULL;
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::GhostButton(UINT nID, BOOL bGhost)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	if (this->GetButton(nIndex)->IsKindOf(RUNTIME_CLASS(CTBToolbarButton)))
	{
		CTBToolbarButton* pSrc = dynamic_cast<CTBToolbarButton*> (this->GetButton(nIndex));
		ASSERT(pSrc);
		if (!pSrc) return FALSE;
		pSrc->SetGhost(bGhost);
	}
	else
	{
		return FALSE;
	}
	return TRUE;
}

//-------------------------------------------------------------------------------------
const CTBNamespace& CTBToolBar::GetNamespace() const
{
	return m_Namespace;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetNamespace(const CTBNamespace& aNs)
{
	m_Namespace = aNs;
}

//----------------------------------------------------------------------------
void CTBToolBar::UpdateWindow()
{
	__super::UpdateWindow();
}

//-------------------------------------------------------------------------------------
BOOL CTBToolBar::SetTextToolTip(UINT nID, CString m_strText)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	this->GetButton(nIndex)->m_strTextCustom = m_strText;
	return TRUE;
}

//-------------------------------------------------------------------------------------
CString CTBToolBar::GetTextToolTip(UINT nID)
{
	int nIndex = FindButton(nID);
	if (nIndex < 0 || nIndex >= GetCount()) return FALSE;
	return this->GetButton(nIndex)->m_strTextCustom;
}

//-------------------------------------------------------------------------------------
void CTBToolBar::SetDefaultButton(int iIndex)
{
	m_iSelected = iIndex;
}

//----------------------------------------------------------------------------
BOOL CTBToolBar::OnUpdateCmdUIDialog()
{
	if (this->IsSuspendedUpdateCmdUI())
		return FALSE;

	CWnd* pTargetToolBar = GetCommandTarget();
	if (m_bDialog && pTargetToolBar)
	{
		UINT iMenuCount = GetCount();
		if (m_nDialogUpdateToolBar >= iMenuCount) {
			m_nDialogUpdateToolBar = 0;
		}
		CBCGPToolbarButton* pButton = GetButton(m_nDialogUpdateToolBar);
		if (pButton)
		{
			CTBToolBarCmdUI state;
			state.m_nIndexMax = iMenuCount;
			state.m_nIndex = m_nDialogUpdateToolBar;
			state.m_pOther = this;
			state.m_nID = pButton->m_nID;
			state.DoUpdate(pTargetToolBar, FALSE);
		}
		m_nDialogUpdateToolBar++;
		return TRUE;
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void CTBToolBar::OnUpdateCmdUI(CFrameWnd* pTarget, BOOL bDisableIfNoHndler)
{
	if (this->IsSuspendedUpdateCmdUI())
		return;

	if (OnUpdateCmdUIDialog())
		return;

	__super::OnUpdateCmdUI(pTarget, bDisableIfNoHndler);
}

//----------------------------------------------------------------------------
BOOL CTBToolBar::OnSetDefaultButtonText(CBCGPToolbarButton* pButton)
{
	ASSERT_VALID(pButton);
	if (!pButton->IsKindOf(RUNTIME_CLASS(CTBToolbarButton)))
		return __super::OnSetDefaultButtonText(pButton);

	CTBToolbarButton* pTBBtn = dynamic_cast<CTBToolbarButton*> (pButton);
	ASSERT(pTBBtn);
	if (!pTBBtn->WantText())
	{
		pButton->m_strText.Empty();
		return TRUE;
	}

	return __super::OnSetDefaultButtonText(pButton);
}

// Get button accelerator
//----------------------------------------------------------------------------
void CTBToolBar::AddToolBarAccelText(CString& strText, CBCGPToolbarButton* pButton) const
{
	CString strAccel;
	// Get Active Document
	if (!pButton) return;

	UINT nID = pButton->m_nID;
	CWnd* pParent = GetParent();
	CLocalizableFrame* pParentFrame = NULL;
	if (pParent)
	{
		pParentFrame = dynamic_cast<CLocalizableFrame*>(pParent->GetParentFrame());
		if (pParentFrame)
		{
			strAccel = GetAcceleratorText(pParentFrame->m_hAccelTable, nID);
		}
	}
	if (strAccel.IsEmpty())
	{
		if (m_pParentTabbedToolbar) {
			CDocument*  pDoc = m_pParentTabbedToolbar->GetParentDocument();
			if (pDoc) {
				CAbstractDoc* pBaseDoc = dynamic_cast<CAbstractDoc*> (pDoc);
				if (pBaseDoc)
				{
					strAccel = pBaseDoc->GetDocAccelText(nID);
					// ID_EXTDOC_EXIT postMessage a ID_FILE_CLOSE
					if (strAccel.IsEmpty() && nID == ID_EXTDOC_EXIT)
					{
						strAccel = pBaseDoc->GetDocAccelText(ID_FILE_CLOSE);
					}
				}
			}
		}
		else if (pParentFrame)
		{
			CAbstractDoc* pDoc = dynamic_cast<CAbstractDoc*>(pParentFrame->GetActiveDocument());
			if (pDoc)
				strAccel = pDoc->GetDocAccelText(nID);
		}
	}
	if (!strAccel.IsEmpty())
	{
		strText.Append(_T(" ("));
		strText.Append(strAccel);
		strText.Append(_T(")"));
	}
}

// Descriptin of ToolTip
//----------------------------------------------------------------------------
void CTBToolBar::GetMessageString(UINT nID, CString& strMessageString) const
{
	for (int i = 0; i < GetCount(); i++)
	{
		CBCGPToolbarButton* pButton = GetButton(i);
		if (!pButton || pButton->m_nID != nID)
			continue;

		// The button is disabled
		if (pButton->m_nStyle & TBBS_DISABLED)
			return;

		if (!pButton->m_strTextCustom.IsEmpty())
		{
			int nPos = pButton->m_strTextCustom.Find(L"\n");
			if (nPos > 0) {
				strMessageString = pButton->m_strTextCustom.Mid(nPos + 1);
				return;
			}

			strMessageString = pButton->m_strTextCustom;
			return;
		}
		else
		{
			__super::GetMessageString(nID, strMessageString);
			if (strMessageString.IsEmpty())
			{
				strMessageString = pButton->m_strText;
				strMessageString.Remove(_T('&'));
			}
			return;
		}
	}

}

// ToolTip Title
//----------------------------------------------------------------------------
BOOL CTBToolBar::OnUserToolTip(CBCGPToolbarButton* pButton, CString& strTipText) const
{
	ASSERT(pButton);

	// Show tooltip in document with have the focus 
	CWnd* pFocus = GetFocus();
	if (pFocus)
	{
		CFrameWnd* pFrame = pFocus->GetParentFrame();

		if (GetParentFrame() != pFrame)
			return TRUE;
	}

	CTBToolbarButton* pTButton = DYNAMIC_DOWNCAST(CTBToolbarButton, pButton);
	if (pTButton && pTButton->IsGhost())
	{
		// not show ToolTip in Ghost button
		return TRUE;
	}

	// The button is disabled
	if (pButton->m_nStyle & TBBS_DISABLED)
		return TRUE;

	if (!pButton->m_strTextCustom.IsEmpty())
	{
		int nPos = pButton->m_strTextCustom.Find(L"\n");

		if (nPos > 0) {
			strTipText = pButton->m_strTextCustom.Mid(0, nPos - 1);
		}
		else
		{
			strTipText = pButton->m_strText;
			strTipText.Remove(_T('&'));
		}
		AddToolBarAccelText(strTipText, pButton);
		return TRUE;
	}

	BOOL bOnUserToolTip = __super::OnUserToolTip(pButton, strTipText);
	if (!bOnUserToolTip || strTipText.IsEmpty())
	{
		// Use button text as tooltip in dialog.!
		if (pButton->m_strText && !pButton->m_strText.IsEmpty())
		{
			strTipText = pButton->m_strText;
			strTipText.Remove(_T('&'));
			bOnUserToolTip = TRUE;
		}
	}
	AddToolBarAccelText(strTipText, pButton);
	return bOnUserToolTip;
}

//----------------------------------------------------------------------------
CSize CTBToolBar::CalcSize(BOOL bVertDock)
{
	return __super::CalcSize(bVertDock);
}

//----------------------------------------------------------------------------
CSize CTBToolBar::CalcLayout(DWORD dwMode, int nLength)
{
	CSize sToolBar = __super::CalcLayout(dwMode, nLength);
	if (m_bDialog)
	{
		CRect rectDockBar;
		CWnd* pParent = this->GetParent();
		if (pParent) {
			pParent->GetClientRect(rectDockBar);
		}
		sToolBar.cx = rectDockBar.Width();
	}

	sToolBar.cy = max(CalcMaxButtonHeight(), sToolBar.cy);
	return sToolBar;
}

//----------------------------------------------------------------------------
int CTBToolBar::CalcMaxButtonHeight()
{
	if (!m_bTextLabels)
	{
		return __super::CalcMaxButtonHeight();
	}

	CClientDC dc(this);
	CFont* pOldFont = SelectDefaultFont(&dc);
	ASSERT(pOldFont != NULL);

	CSize sizeButon = GetButtonSize();
	// Calculate internal of BCGP in CBCGPToolbarButton::OnCalculateSize
	CRect rectText(0, 0, sizeButon.cx * 3, sizeButon.cy);
	UINT uiTextFormat = DT_CENTER | DT_CALCRECT | DT_WORDBREAK;
	dc.DrawText(_T("XXXXXXXXXXXXXXXX XXXXXXXXXXXXXXXX"), rectText, uiTextFormat);

	int iHeightMax = sizeButon.cy + rectText.Height() + ScalePix(CY_BORDER);

	int iButtonHeight = __super::CalcMaxButtonHeight();

	iButtonHeight = max(iButtonHeight, iHeightMax) + TOOL_BAR_SPACE_HEIGHT; /* 4 px of Space*/

	dc.SelectObject(pOldFont);
	if (IsToolbarInfinity())
	{
		// In infinity toolBar the text is in only row, the height is 2 row;
		return iHeightMax + TOOL_BAR_SPACE_HEIGHT;
	}
	return iButtonHeight;
}

//----------------------------------------------------------------------------
void CTBToolBar::DrawSeparator(CDC* pDC, const CRect& rect, BOOL bHorz)
{
	if (!bHorz || m_bDialog)
	{
		__super::DrawSeparator(pDC, rect, bHorz);
		return;
	}

	int x1, x2;
	int y1, y2;

	x1 = x2 = (rect.left + rect.right) / 2;
	y1 = rect.top;
	y2 = rect.bottom - 1;

	CBCGPDrawManager dm(*pDC);
	dm.DrawLine(x1, y1, x2, y2, AfxGetThemeManager()->GetToolbarSeparatorColor());
}

//----------------------------------------------------------------------------
int CTBToolBar::CalcMinimumWidth(BOOL bHidden /*= FALSE*/)
{
	int iWidth = 0;
	int iButton = 0;

	if (m_bButtonsOverlap)
		return 100; // Min ToolBar in px

	for (POSITION pos = m_Buttons.GetHeadPosition(); pos != NULL; iButton++)
	{
		CObject* pObj = m_Buttons.GetNext(pos);

		CBCGPToolbarButton* pButton = dynamic_cast<CBCGPToolbarButton*> (pObj);
		if (pButton == NULL) break;

		if (!bHidden && !pButton->IsVisible()) continue;

		if (pObj->IsKindOf(RUNTIME_CLASS(CTBToolbarLabel)))
		{
			CTBToolbarLabel* pLabel = (CTBToolbarLabel*)pObj;
			if (pLabel && pLabel->IsRightSpace())
			{
				iWidth += 20;
				continue;
			}
		}
		// Qui viene incluso anche la Label del bottone.!
		iWidth += GetButtonWidth(pButton);
	}

	if (m_pObjectLeft && m_bCenterButtons)
	{
		iWidth += 10;
	}

	return iWidth;
}

//----------------------------------------------------------------------------
CRect CTBToolBar::GetDockBarRect()
{
	CBCGPDockBar* pDockBar = GetParentDockBar();
	CBCGPBaseTabbedBar* pTabbedBar = GetParentTabbedBar();
	CRect rectDockBar;

	if (pDockBar && !pTabbedBar)
	{
		pDockBar->GetClientRect(rectDockBar);
	}
	else if (pTabbedBar)
	{
		pTabbedBar->GetClientRect(rectDockBar);
	}
	else
	{
		CWnd* pParent = this->GetParent();
		if (pParent)
		{
			pParent->GetClientRect(rectDockBar);
		}
	}

	return rectDockBar;
}

//  Return TRUE if button stata is changed
//----------------------------------------------------------------------------
BOOL CTBToolBar::AutoHideButton(CBCGPToolbarButton* pButton)
{
	if (m_hWnd == NULL)	return FALSE;

	ASSERT(pButton);
	if (pButton == NULL) return FALSE;

	BOOL ret;
	if (m_mapButtonNotAutoHidden.Lookup(pButton->m_nID, ret)) {
		if (ret) return FALSE;
	}

	CTBToolbarButton* pTBToolBar = dynamic_cast<CTBToolbarButton*> (pButton);
	CTBToolbarMenuButton* pTBToolBarMenu = NULL;
	if (pTBToolBar == NULL) {
		pTBToolBarMenu = dynamic_cast<CTBToolbarMenuButton*> (pButton);
		if (pTBToolBar == NULL && pTBToolBarMenu == NULL) {
			return FALSE;
		}
	}

	// Auto Hide is disable ?
	if (!m_bAutoHideToolBarButton || AfxIsRemoteInterface())
		return FALSE;
	if (pButton && m_pMenuButtonCollapsed && m_pMenuButtonCollapsed->m_nID == pButton->m_nID) return FALSE;

	if (m_hWnd == NULL) return FALSE;

	if (pButton->m_nStyle & TBBS_DISABLED)
	{
		BOOL bret = FALSE;
		if (!IsHideButton(pButton->m_nID))
		{
			HideButton(pButton->m_nID);
			bret = TRUE;

			if (pTBToolBar) {
				pTBToolBar->SetAutoHide(TRUE);
			}
			if (pTBToolBarMenu) {
				pTBToolBarMenu->SetAutoHide(TRUE);
			}
		}

		return bret;
	}
	else
	{
		if (pTBToolBar == NULL && pTBToolBarMenu == NULL)
		{
			HideButton(pButton->m_nID, FALSE);
			return TRUE;
		}

		if (pTBToolBar)
		{
			if (IsHideButton(pButton->m_nID) && pTBToolBar->GetAutoHide())
			{
				HideButton(pButton->m_nID, FALSE);
				pTBToolBar->SetAutoHide(TRUE);
				return TRUE;
			}
		}

		if (pTBToolBarMenu)
		{
			if (IsHideButton(pButton->m_nID) && pTBToolBarMenu->GetAutoHide())
			{
				HideButton(pButton->m_nID, FALSE);
				pTBToolBarMenu->SetAutoHide(TRUE);
				return TRUE;
			}
		}

	}
	return FALSE;
}

//----------------------------------------------------------------------------
void CTBToolBar::PopulatedMenuButton(CTBToolbarMenuButton* pMenuButton)
{
	CDocument* pDoc = GetParentDocument();	
	if (pDoc) {
		CBaseDocument* pBaseDoc = dynamic_cast<CBaseDocument*> (pDoc);
		if (pBaseDoc) {
			CBaseDocument::FormMode eLastFormMode = pBaseDoc->GetFormMode();
			if (m_PrevFormMode != eLastFormMode) {
				// Update all drop Down Menu to change document state
				pMenuButton->OnPopulatedMenuButton();
			}
			m_PrevFormMode = eLastFormMode;
		}
	}
}

//----------------------------------------------------------------------------
CDocument* CTBToolBar::GetParentDocument()
{
	CDocument* pDoc = NULL;
	if (m_pParentTabbedToolbar) 
		pDoc = m_pParentTabbedToolbar->GetParentDocument();
	return pDoc;
}

//----------------------------------------------------------------------------
BOOL CTBToolBar::ISMenuButtonVoicesEnabled(CTBToolbarMenuButton* pMenuButton)
{
	CDocument* pDoc = GetParentDocument();
	PopulatedMenuButton(pMenuButton);

	CMenu menu;
	HMENU hMenu = pMenuButton->GetMenu();
	if (hMenu == NULL) return FALSE;
	menu.Attach(hMenu);
	int iMenuCount = menu.GetMenuItemCount();
	BOOL bAllDisable = TRUE;
	if (pDoc && iMenuCount > 0)
	{
		for (int i = 0; i < iMenuCount; i++)
		{
			CString sMenuString;
			menu.GetMenuString(i, sMenuString, MF_BYPOSITION);
			UINT menuState = menu.GetMenuState(i, MF_BYPOSITION);
			// Is a separator
			if (menuState & MF_SEPARATOR) {
				continue;
			}

			UINT nID = menu.GetMenuItemID(i);
			CCmdUI state;
			state.m_nIndexMax = iMenuCount;
			state.m_nIndex = i;
			state.m_pMenu = &menu;
			state.m_nID = nID;

			ASSERT(pDoc);
			if (pDoc == NULL) return FALSE;
			pDoc->OnCmdMsg(nID, CN_UPDATE_COMMAND_UI, &state, NULL);
			if (!state.m_bEnableChanged) {
				// not present un ->Enable in Command UI function
				bAllDisable = FALSE;
				break;
			}

			menuState = menu.GetMenuState(nID, MF_BYCOMMAND);
			if (menuState & MF_DISABLED) {
				continue;
			}
			bAllDisable = FALSE;
			break;
		}
	}
	else
	{
		bAllDisable = FALSE;
	}

	return bAllDisable;
}

//----------------------------------------------------------------------------
BOOL CTBToolBar::AutoHideMenuButton(int nIndex)
{
	if (m_hWnd == NULL)	return FALSE;
	// Auto Hide is disable ?
	if (!m_bAutoHideToolBarButton) return FALSE;

	// is the Switch button of toolBar ?
	if (m_pParentTabbedToolbar && !m_bShowToolBarTab && nIndex == 0)
	{
		if (m_pParentTabbedToolbar->GetIDSwitch() == this->GetButton(nIndex)->m_nID) {
			return FALSE;
		}
	}

	CTBToolbarMenuButton* pMenuButton = dynamic_cast<CTBToolbarMenuButton*> (this->GetButton(nIndex));
	if (pMenuButton == NULL) return  FALSE;

	BOOL ret;
	if (m_mapButtonNotAutoHidden.Lookup(pMenuButton->m_nID, ret)) {
		if (ret) return FALSE;
	}

	BOOL bAllDisable = ISMenuButtonVoicesEnabled(pMenuButton);

	if (bAllDisable != IsHideButton(pMenuButton->m_nID))
	{
		if (bAllDisable)
		{
			HideButton(pMenuButton->m_nID, bAllDisable);
			pMenuButton->SetAutoHide(bAllDisable);
			return TRUE;
		}
		else
		{
			if (pMenuButton->GetAutoHide())
			{
				HideButton(pMenuButton->m_nID, bAllDisable);
				pMenuButton->SetAutoHide(bAllDisable);
				return TRUE;
			}
		}
	}

	return FALSE;
}

//----------------------------------------------------------------------------
void CTBToolBar::SetButtonStyleByIdc(UINT nCommandID, UINT nStyle)
{
	INT nIndex = FindButton(nCommandID);
	if (nIndex < 0) return;
	SetButtonStyle(nIndex, nStyle);
}

//----------------------------------------------------------------------------
void CTBToolBar::SendMessageToolBarUpdate()
{
	if (m_bPostToolBarUpdate) return;
	m_bPostToolBarUpdate = TRUE;
	PostMessage(UM_TOOLBAR_UPDATE);
}

//----------------------------------------------------------------------------
LRESULT CTBToolBar::OnToolBarUpdate(WPARAM, LPARAM)
{
	if (RepositionRightButtons())
	{
		AdjustSizeImmediate();
	}

	AdjustLayout();
	m_bPostToolBarUpdate = FALSE;
	return 0;
}

//----------------------------------------------------------------------------
BOOL CTBToolBar::SuspendLayout()
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	if (pFrame)
		pFrame->m_bDelayedLayoutSuspended = m_bSuspendLayout;

	if (m_bSuspendLayout)
		return FALSE;
	m_bButtonStyleLoopComplite = FALSE;

	m_bSuspendLayout = TRUE;
	if (pFrame)
		pFrame->m_bDelayedLayoutSuspended = m_bSuspendLayout;

	return TRUE;
}

//----------------------------------------------------------------------------
void CTBToolBar::ResumeLayout(BOOL bForced /*= FALSE*/)
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	if (pFrame)
		pFrame->m_bDelayedLayoutSuspended = m_bSuspendLayout;

	// CTBTabbedToolbar update active toolbar look the  CTBTabbedToolbar::ResumeLayout
	if (!bForced && (!m_bButtonStyleLoopComplite || !m_bSuspendLayout))
		return;

	m_bSuspendLayout = FALSE;
	if (pFrame)
		pFrame->m_bDelayedLayoutSuspended = m_bSuspendLayout;
}

// MFC overload standard metod for control the collapsed button
//----------------------------------------------------------------------------
void CTBToolBar::SetButtonStyle(int nIndex, UINT nStyle)
{
	CBCGPToolbarButton* pButton = GetButton(nIndex);
	__super::SetButtonStyle(nIndex, nStyle);

	if (pButton && ButtonsInOverlapList(pButton->m_nID))
		return;

	// Collapsed button check - Enable always
	int nPosButtonCollapsed = FindButton(m_nIDOverlapButton);
	if (m_pMenuButtonCollapsed && nPosButtonCollapsed == nIndex && nStyle & TBBS_DISABLED)
	{
		pButton->SetStyle(TBBS_BUTTON);
	}

	// Resume layout of tabber if toolbar is not suspend
	if (m_pParentTabbedToolbar && !m_bSuspendLayout && m_bButtonStyleLoopComplite)
	{
		m_pParentTabbedToolbar->ResumeLayout();
	}

	CTBToolbarMenuButton* menuButton = dynamic_cast<CTBToolbarMenuButton*> (this->GetButton(nIndex));
	if (nIndex < m_iAdjustLayoutHideButton || GetCount() == 1 || m_iAdjustLayoutHideButton == -1)
	{
		if (m_bAdjustLayoutHideButton)
		{
			if (m_bButtonStyleLoopComplite)
				ResumeLayout();
			SendMessageToolBarUpdate();
		}

		m_bAdjustLayoutHideButton = FALSE;
	}

	m_iAdjustLayoutHideButton = nIndex;
	if (menuButton && !(pButton->m_nStyle & TBBS_DISABLED))
	{
		if (AutoHideMenuButton(nIndex))
		{
			m_bAdjustLayoutHideButton = TRUE;
			SendMessageToolBarUpdate();
		}
	}
	else
	{
		if (AutoHideButton(pButton))
		{
			m_bAdjustLayoutHideButton = TRUE;
			SendMessageToolBarUpdate();
		}
	}

	// Calc the Last Button for update start
	if (!m_bButtonStyleLoopComplite)
	{
		int nCommandUI = 0;
		for (int k = 0; k < GetCount(); k++)
		{
			CBCGPToolbarButton* pButtonTest = GetButton(k);
			int nIdTest = (pButtonTest->m_nID);
			CTBToolbarButton* pTBToolBar = dynamic_cast<CTBToolbarButton*> (pButtonTest);
			CTBToolbarMenuButton* pTBToolBarMenu = dynamic_cast<CTBToolbarMenuButton*> (pButtonTest);

			if ((pTBToolBar || pTBToolBarMenu) && nIdTest > 1)
			{
				nCommandUI = k;
			}
		}

		if (nCommandUI == nIndex)
		{
			if (!m_bAutoHideToolBarButton)
			{
				GetLockedDisabledImages()->ConvertToGrayScale();
				m_bRecalcGray = FALSE;
			}

			m_bButtonStyleLoopComplite = TRUE;
			ResumeLayout();
		}
	}
	else
	{
		if (m_bRecalcGray && !m_bAutoHideToolBarButton)
		{
			GetLockedDisabledImages()->ConvertToGrayScale();
			m_bRecalcGray = FALSE;
		}
	}
}

//----------------------------------------------------------------------------
BOOL CTBToolBar::AddButtonOverlap(CTBToolBarMenu* pMenu)
{
	int iPlaceRight = -1;
	if (m_pObjectRight && !m_bToRight)
		iPlaceRight = GetRightSpacePos();

	BOOL bInfinity = IsToolbarInfinity();
	// Infinity style toolbar
	if (bInfinity)
	{
		iPlaceRight = GetCount();
		if (m_pParentTabbedToolbar)
		{
			iPlaceRight = FindButton(m_pParentTabbedToolbar->GetIDSwitch());
			if (iPlaceRight >= 0)
				iPlaceRight -= 1;
		}
	}

	if (m_pMenuButtonCollapsed)
	{
		int iPos = FindButton(m_nIDOverlapButton);
		if ((iPos + 1) == iPlaceRight)
		{
			if (IsHideButton(m_nIDOverlapButton))
			{
				m_pMenuButtonCollapsed->SetVisible();
			}
			UpdateDropdownMenu(m_nIDOverlapButton, pMenu);
			AdjustLayout();
			return TRUE;
		}
		// Remove button
		BOOL bRet = __super::RemoveButton(iPos);
		m_pMenuButtonCollapsed = NULL;
	}

	ASSERT(m_pMenuButtonCollapsed == NULL);
	if (m_pMenuButtonCollapsed) return FALSE;

	// Make a new button
	m_pMenuButtonCollapsed = new CTBToolbarMenuButton();

	CTBToolBarMenu tmpMenu;
	tmpMenu.CreateMenu();
	AppendMenu(tmpMenu, MF_BYCOMMAND | MF_ENABLED, -1, _T(""));
	m_pMenuButtonCollapsed->CreateMenu(&tmpMenu);

	m_pMenuButtonCollapsed->m_nID = m_nIDOverlapButton;

	if (!AfxIsRemoteInterface())
	{
		// Load collapsed Icon 
		HICON hIcon = LoadImageByNameSpace(m_stIconCollapsed);
		ASSERT(hIcon);
		int nIdx = GetLockedImages()->AddIcon(hIcon);
		GetLockedDisabledImages()->AddIcon(hIcon);
		m_bRecalcGray = TRUE;
		m_pMenuButtonCollapsed->SetImage(nIdx);
		m_pMenuButtonCollapsed->TextBelow();
		m_pMenuButtonCollapsed->EnableAlwaysDropDown();

		::DestroyIcon(hIcon);
	}

	int nPosButtonCollapsed = __super::InsertButton(m_pMenuButtonCollapsed, iPlaceRight);
	UpdateDropdownMenu(m_nIDOverlapButton, pMenu);
	AdjustLayout();
	return TRUE;
}

//----------------------------------------------------------------------------
HICON CTBToolBar::GetOverlapIconDropdownMenu(const CBCGPToolbarMenuButton* pMenuButton)
{
	HICON hIconFound = NULL;
	// Collapsed dropDown icons
	/*if (m_pMenuButtonCollapsed && m_nIDLastDropDown == m_nIDOverlapButton)
	{*/
	hIconFound = GetCollapsedImage(pMenuButton->m_nID);
	if (hIconFound)
	{
		return hIconFound;
	}
	else
	{
		for (POSITION pos = m_ListCollapsedItems.GetHeadPosition(); pos != NULL; )
		{
			CTBCollapsedItem* pItem = m_ListCollapsedItems.GetNext(pos);
			ASSERT(pItem);
			if (pItem->GetType() == CTBCollapsedItem::ECollapsedItemType::ITEM_SEPARATOR)
				continue;

			if (pItem->GetType() == CTBCollapsedItem::ECollapsedItemType::ITEM_DROPDOWN &&
				pMenuButton->m_strText.Compare(pItem->GetText()) == 0)
			{
				hIconFound = GetCollapsedImage(pItem->GetID());
				if (hIconFound)
				{
					return hIconFound;
				}
				return NULL;
			}
		}
	}
	//}

	return NULL;
}

//----------------------------------------------------------------------------
void CTBToolBar::ButtonsOverlapRemoveByID(UINT nID)
{
	if (m_ListCollapsedItems.GetCount() <= 0) return;
	for (POSITION pos = m_ListCollapsedItems.GetHeadPosition(); pos != NULL; )
	{
		CTBCollapsedItem* pItem = m_ListCollapsedItems.GetNext(pos);
		ASSERT(pItem);
		if (nID == pItem->GetID() && pos)
		{
			SAFE_DELETE(pItem);
			m_ListCollapsedItems.RemoveAt(pos);
			break;
		}
	}
}

//----------------------------------------------------------------------------
BOOL CTBToolBar::ButtonsInOverlapList(UINT nID)
{
	if (!AfxGetThemeManager()->ButtonsOverlap()) return FALSE;
	if (m_ListCollapsedItems.GetCount() <= 0) return FALSE;
	for (POSITION pos = m_ListCollapsedItems.GetHeadPosition(); pos != NULL; )
	{
		CTBCollapsedItem* pItem = m_ListCollapsedItems.GetNext(pos);
		ASSERT(pItem);
		if (nID == pItem->GetID())
		{
			return TRUE;
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void CTBToolBar::ButtonsOverlapMenuSub(UINT nID, CMenu* pMenu, CArray<CTBToolBarMenu*, CTBToolBarMenu*>* ptMenuArray)
{
	INT nPos = FindButton(nID);
	if (nPos < 0) return;
	CBCGPToolbarButton* pButton = GetButton(nPos);
	if (!pButton) return;
	CTBToolbarMenuButton* menuButton = dynamic_cast<CTBToolbarMenuButton*> (pButton);
	if (!menuButton) return;
	// Populate the button menu!
	menuButton->OnPopulatedMenuButton();
	HMENU hMenu = menuButton->GetMenu();
	if (!hMenu) return;
	CMenu menu;
	menu.Attach(hMenu);
	for (int i = 0; i < menu.GetMenuItemCount(); i++)
	{
		MENUITEMINFO  info;
		info.cbSize = sizeof(MENUITEMINFO);
		info.fMask = MIIM_SUBMENU;
		menu.GetMenuItemInfo(i, &info, TRUE);
		UINT nIDmenu = menu.GetMenuItemID(i);
		CString sMenuString;
		menu.GetMenuString(i, sMenuString, MF_BYPOSITION);
		if (info.hSubMenu)
		{
			ptMenuArray->Add(new CTBToolBarMenu());
			UINT nMenuArray = ptMenuArray->GetSize() - 1;
			ptMenuArray->ElementAt(nMenuArray)->CreatePopupMenu();

			CMenu SubAttachMenu;
			SubAttachMenu.Attach(info.hSubMenu);
			int iSubCount = SubAttachMenu.GetMenuItemCount();
			for (int j = 0; j < iSubCount; j++)
			{
				CString sSubMenuString;
				SubAttachMenu.GetMenuString(j, sSubMenuString, MF_BYPOSITION);
				nIDmenu = SubAttachMenu.GetMenuItemID(j);
				if (nIDmenu > 0)
				{
					ptMenuArray->ElementAt(nMenuArray)->AppendMenu(MF_STRING, nIDmenu, sSubMenuString);
					HICON hIconSub = GetIconUnCheckedDropdown(nIDmenu);
					if (hIconSub)
					{	// Set the Icon
						AddCollapsedImage(nIDmenu, hIconSub);
					}
				}
				else
					ptMenuArray->ElementAt(nMenuArray)->AppendMenu(MF_SEPARATOR);
			}
			pMenu->AppendMenu(MF_POPUP, (UINT_PTR)ptMenuArray->ElementAt(nMenuArray)->GetSafeHmenu(), sMenuString);
		}
		else
		{
			if (nIDmenu > 0)
			{
				pMenu->AppendMenu(MF_STRING, nIDmenu, sMenuString);
				HICON hIconSub = GetIconUnCheckedDropdown(nIDmenu);
				if (hIconSub)
				{	// Set the Icon
					AddCollapsedImage(nIDmenu, hIconSub);
				}
			}
			else
				pMenu->AppendMenu(MF_SEPARATOR);
		}
	}
}

//----------------------------------------------------------------------------
void CTBToolBar::ButtonsOverlapMenuUpdate()
{
	if (IsLayoutSuspended())
		return;

	if (m_ListCollapsedItems.GetCount() <= 0) return;
	// Make the new DowpDown Menu
	CArray<CTBToolBarMenu*, CTBToolBarMenu*> ptMenuArray;
	CTBToolBarMenu* pMenuOverlap = new CTBToolBarMenu();
	pMenuOverlap->CreateMenu();
	ptMenuArray.Add(pMenuOverlap);

	// Add icon to menu voice
	for (POSITION pos = m_ListCollapsedItems.GetHeadPosition(); pos != NULL; )
	{
		CTBCollapsedItem* pItem = m_ListCollapsedItems.GetNext(pos);
		ASSERT(pItem);
		if (pItem->GetType() != CTBCollapsedItem::ECollapsedItemType::ITEM_SEPARATOR)
		{
			UINT nID = pItem->GetID();

			INT nPos = FindButton(nID);
			if (nPos >= 0 && m_bAutoHideToolBarButton)
			{
				CBCGPToolbarButton* pButton = GetButton(nPos);
				if (pButton && pButton->m_nStyle & TBBS_DISABLED)
					continue;

				if (pButton && pItem->GetType() == CTBCollapsedItem::ECollapsedItemType::ITEM_DROPDOWN)
				{
					CTBToolbarMenuButton* pMenuButton = dynamic_cast<CTBToolbarMenuButton*> (GetButton(nPos));
					if (pMenuButton &&  ISMenuButtonVoicesEnabled(pMenuButton))
						continue;
				}
			}

			if (pItem->GetType() == CTBCollapsedItem::ECollapsedItemType::ITEM_DROPDOWN)
			{

				CTBToolBarMenu* pMenuSub = new CTBToolBarMenu();
				ptMenuArray.Add(pMenuSub);
				pMenuSub->CreatePopupMenu();
				ButtonsOverlapMenuSub(nID, pMenuSub, &ptMenuArray);
				pMenuOverlap->AppendMenu(MF_POPUP, (UINT)(pMenuSub->m_hMenu), pItem->GetText());
			}
			else
				if (!pItem->GetText().Trim().IsEmpty())
					pMenuOverlap->AppendMenu(MF_STRING, nID, pItem->GetText());

			// Add Icons in menu voice
			HICON hIcon = GetCollapsedImage(pItem->GetID());;
			if (hIcon == NULL)
			{	// Not custom icon set, get icon from button
				INT nPos = FindButton(nID);
				if (nPos < 0) continue;
				CBCGPToolbarButton* pButton = GetButton(nPos);
				ASSERT(pButton);
				hIcon = GetLockedImages()->ExtractIcon(pButton->GetImage());
				// Set the Icon
				AddCollapsedImage(nID, hIcon);
			}

		}

	}

	// Update Menu
	AddButtonOverlap(pMenuOverlap);

	// Destroy costruction menu
	for (INT i = 0; i < ptMenuArray.GetSize(); i++)
	{
		SAFE_DELETE(ptMenuArray[i]);
	}

}

//----------------------------------------------------------------------------
BOOL  CTBToolBar::ButtonsOverlapRemove(int iWidth)
{
	if (m_pMenuButtonCollapsed == NULL || (m_pMenuButtonCollapsed && !m_pMenuButtonCollapsed->IsVisible())) return FALSE;

	CList<UINT, UINT> listRemoveItem;

	int nWidthCollapsed = GetButtonWidth(m_pMenuButtonCollapsed);
	iWidth -= nWidthCollapsed;

	for (INT k = 0; k < m_ListCollapsedItems.GetCount(); k++)
	{
		POSITION pos = m_ListCollapsedItems.FindIndex(k);
		if (!pos) continue;
		CTBCollapsedItem* pItem = m_ListCollapsedItems.GetAt(pos);
		ASSERT(pItem);
		INT nPos = FindButton(pItem->GetID());
		if (nPos < 0) continue;
		CBCGPToolbarButton* pButton = GetButton(nPos);
		ASSERT(pButton);

		iWidth -= pItem->GetWidth();
		if (iWidth > 0 || (m_ListCollapsedItems.GetCount() - listRemoveItem.GetCount() <= 1))
		{
			if (pButton) pButton->SetVisible();

			if (nPos > 0)
			{
				CBCGPToolbarButton* pButtonR = GetButton(nPos - 1);
				if (pButtonR && pButtonR->m_nStyle & TBBS_SEPARATOR)
				{
					pButtonR->SetVisible();
				}
			}

			listRemoveItem.AddTail(k);
		}
		else
		{
			break;
		}
	}

	if (listRemoveItem.GetCount() <= 0)
		return FALSE;

	for (INT k = listRemoveItem.GetCount(); k > 0; k--)
	{
		POSITION pos = m_ListCollapsedItems.FindIndex(k - 1);
		ASSERT(pos);
		if (!pos) continue;
		CTBCollapsedItem* pItem = m_ListCollapsedItems.GetAt(pos);
		SAFE_DELETE(pItem);
		m_ListCollapsedItems.RemoveAt(pos);
	}

	BOOL bVisible = m_ListCollapsedItems.GetCount() > 0;
	m_pMenuButtonCollapsed->SetVisible(bVisible);
	if (!bVisible && IsToolbarInfinity())
	{
		SendMessageToolBarUpdate();
	}

	return TRUE;
}

//-----------------------------------------------------------------------------------
BOOL CTBToolBar::ButtonsOverlapRemoveItem(UINT nID)
{
	if (m_ListCollapsedItems.GetCount() <= 0)
		return FALSE;

	for (INT k = m_ListCollapsedItems.GetCount(); k > 0; k--)
	{
		POSITION pos = m_ListCollapsedItems.FindIndex(k - 1);
		ASSERT(pos);
		if (!pos) continue;
		CTBCollapsedItem* pItem = m_ListCollapsedItems.GetAt(pos);
		UINT id = pItem->GetID();

		if (id != nID)
			continue;

		SAFE_DELETE(pItem);
		m_ListCollapsedItems.RemoveAt(pos);
	}

	m_pMenuButtonCollapsed->SetVisible(m_ListCollapsedItems.GetCount() > 0);
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL  CTBToolBar::ButtonsOverlapAdd(CBCGPToolbarButton* pButton)
{
	if (!m_bButtonStyleLoopComplite)
		return FALSE;

	if ((pButton == NULL || m_hWnd == NULL) ||
		(m_pMenuButtonCollapsed && pButton->m_nID == m_pMenuButtonCollapsed->m_nID) ||
		!pButton->IsVisible())
		return FALSE;

	// is the Switch button of toolBar ?
	if (m_pParentTabbedToolbar && !m_bShowToolBarTab && m_pParentTabbedToolbar->GetIDSwitch() == pButton->m_nID)
		return FALSE;

	int nID = pButton->m_nID;
	if (nID <= 0) return FALSE;

	// Check if button is present
	// Add icon to menu voice
	for (POSITION pos = m_ListCollapsedItems.GetHeadPosition(); pos != NULL; )
	{
		CTBCollapsedItem* pItem = m_ListCollapsedItems.GetNext(pos);
		ASSERT(pItem);
		if (nID == pItem->GetID())
		{
			pButton->SetVisible(FALSE);
			return FALSE;
		}
	}

	CString stButton = pButton->m_strText;
	// Button Type
	CTBCollapsedItem::ECollapsedItemType itemType = CTBCollapsedItem::ECollapsedItemType::ITEM_BUTTON;
	CTBToolbarMenuButton* menuButton = dynamic_cast<CTBToolbarMenuButton*> (pButton);
	if ((pButton->m_nStyle & TBBS_SEPARATOR))
	{
		itemType = CTBCollapsedItem::ECollapsedItemType::ITEM_SEPARATOR;
	}
	else if (menuButton)
	{
		itemType = CTBCollapsedItem::ECollapsedItemType::ITEM_DROPDOWN;
	}

	CTBCollapsedItem* pCollapsedItem = new CTBCollapsedItem(nID, itemType, stButton, pButton->Rect().Width());
	// Add Item

	// Hiven button
	pButton->SetVisible(FALSE);
	INT nPos = FindButton(nID);

	// The previev button is separator ?
	if (nPos > 0)
	{
		CBCGPToolbarButton* pButtonR = GetButton(nPos - 1);
		if (pButtonR && pButtonR->m_nStyle & TBBS_SEPARATOR)
		{
			pButtonR->SetVisible(FALSE);
		}
	}

	// Abb Button to List in order
	if (m_ListCollapsedItems.GetCount() > 0 && nPos >= 0)
	{
		for (INT k = 0; k < m_ListCollapsedItems.GetCount(); k++)
		{
			POSITION pos = m_ListCollapsedItems.FindIndex(k);
			if (!pos) continue;
			CTBCollapsedItem* pItem = m_ListCollapsedItems.GetAt(pos);
			ASSERT(pItem);
			if (pItem->GetType() != CTBCollapsedItem::ECollapsedItemType::ITEM_SEPARATOR)
			{
				INT nPosItem = FindButton(pItem->GetID());
				if (nPosItem > nPos)
				{
					m_ListCollapsedItems.InsertBefore(pos, pCollapsedItem);
					return TRUE;
				}
			}
		}
	}
	m_ListCollapsedItems.AddTail(pCollapsedItem);
	return TRUE;
}

//----------------------------------------------------------------------------
void CTBToolBar::SetCollapsedImage(CString stImageNameSpace)
{
	m_stIconCollapsed = stImageNameSpace;
	if (m_pMenuButtonCollapsed)
	{
		HICON hIcon = LoadImageByNameSpace(m_stIconCollapsed);
		ASSERT(hIcon);
		int nIndexImage = GetLockedImages()->AddIcon(hIcon);
		GetLockedDisabledImages()->AddIcon(hIcon);
		m_bRecalcGray = TRUE;
		::DestroyIcon(hIcon);

		m_pMenuButtonCollapsed->SetImage(nIndexImage);
		int nIndex = FindButton(m_pMenuButtonCollapsed->m_nID);
		InvalidateButton(nIndex);
	}
}

//-------------------------------------------------------------------------------------
CSize CTBToolBar::CalcFixedLayout(BOOL bStretch, BOOL bHorz)
{
	return  __super::CalcFixedLayout(bStretch, bHorz);
}

//----------------------------------------------------------------------------
INT	CTBToolBar::GetButtonWidth(CBCGPToolbarButton* pButton)
{
	INT nWidthButton = pButton->Rect().Width();
	// The buttons is visible but not have a size, calculate the size
	if (nWidthButton <= 0)
	{
		CClientDC dc(this);
		CSize sizeButton = pButton->OnCalculateSize(&dc, GetButtonSize(), TRUE);
		nWidthButton = sizeButton.cx;
	}
	return nWidthButton;
}

//
// Calculate buttons width Left & Right
//
//----------------------------------------------------------------------------
void CTBToolBar::RepositionRightButtonsCalcButtonsSpace(CRect rectDockBar, INT& iWidthLeft, INT& iWidthRight)
{

	BOOL bSun = TRUE;
	BOOL bFirst = FALSE;
	INT iButton = 0;
	INT iButtonSeparatorRight = -1;

	iWidthLeft = 0;
	iWidthRight = 0;

	int dockBarLeft = rectDockBar.left;

	for (POSITION pos = m_Buttons.GetHeadPosition(); pos != NULL; iButton++)
	{
		CObject* pObj = m_Buttons.GetNext(pos);

		if (m_bCenterButtons && iButton == 0)
			continue;

		CBCGPToolbarButton* pButton = dynamic_cast<CBCGPToolbarButton*> (pObj);
		if (pButton == NULL)
		{
			break;
		}

		if (pButton->m_nStyle & TBBS_SEPARATOR && !bSun && iButtonSeparatorRight < 0)
		{
			iButtonSeparatorRight = iButton;
		}

		if (!pButton->IsVisible())
			continue;

		//if (!bFirst)
		//{
		//	// Controllo se il primo bottone si trova in posizione anomale
		//	// e quindi forzo un ricalcolo della ToolBar e dei botoni a destra
		//	bFirst = TRUE;
		//	if (pButton->Rect().left - dockBarLeft  > 5)
		//	{
		//		m_iWidthObjectLeft = m_iWidthObjectRight = 0;
		//	}
		//}

		// Combo Button
		if (pObj->IsKindOf(RUNTIME_CLASS(CTBToolbarComboBoxButton)))
		{
			// Forzo il ricalcolo della Combo se sitrova in posizzione anomala TOP = 0
			CTBToolbarComboBoxButton* pCombo = (CTBToolbarComboBoxButton*)pObj;
			CRect rect = pCombo->Rect();
			if (rect.top == 0) { pCombo->OnMove(); }
		}

		if (pObj->IsKindOf(RUNTIME_CLASS(CTBToolbarLabel)))
		{
			CTBToolbarLabel* pLabel = (CTBToolbarLabel*)pObj;
			if (pLabel && pLabel->IsRightSpace())
			{
				bSun = FALSE;
				continue;
			}
		}

		int nWidthButton = GetButtonWidth(pButton);

		// Qui viene incluso anche la Label del bottone nella dimensione.!
		if (bSun)
			iWidthLeft += nWidthButton;
		else
			iWidthRight += nWidthButton;
	}
}

//----------------------------------------------------------------------------
BOOL CTBToolBar::RepositionRightButtons()
{
	////////////////////////////////////////////////////////////
	// Calculate the ToolBar buttons position
	m_bButtonsOverlap = AfxGetThemeManager()->ButtonsOverlap(); // Enable overlop 
	CRect rectDockBar = GetDockBarRect();
	int dockBarWidth = rectDockBar.Width();
	INT widthLeft;
	INT widthRight;
	BOOL bRet = FALSE;

	if (m_pParentTabbedToolbar)
	{
		int dockBarwidthParent = m_pParentTabbedToolbar->GetMaxWidth();
		dockBarWidth = max(dockBarWidth, dockBarwidthParent);
		// Only in toolbar active
		if (this != m_pParentTabbedToolbar->GetToolBarActive())
			return FALSE;
	}

	RepositionRightButtonsCalcButtonsSpace(rectDockBar, widthLeft, widthRight);

	// calculate the space for center or right buttons -  m_iWidthObjectLeft & m_WidthObjectRight
	int iRightButton = dockBarWidth - widthLeft - widthRight - SPACE_TOOLBAR_RIGHT;
	if (m_bButtonsOverlap && m_nIDOverlapButton >= 0 && !(m_pObjectLeft && m_bCenterButtons))
	{
		// Buoont is in Overlap, hiden the button and add in collapsed drop down button
		BOOL bInsertPrevButton = (m_pMenuButtonCollapsed == NULL || (m_pMenuButtonCollapsed && !m_pMenuButtonCollapsed->IsVisible()));
		BOOL bRecalc = FALSE;
		if (iRightButton <= 0)
		{
			int prtLoop = 0;
			while (iRightButton <= 0)
			{
				INT nPos = -1;
				if (!IsToolbarInfinity())
					nPos = GetRightSpacePos();
				if (nPos < 0) nPos = GetCount();

				CBCGPToolbarButton* pButtonToCollapsed = FindCollapsableButton(nPos);
				if (pButtonToCollapsed)
				{
					bRecalc = ButtonsOverlapAdd(pButtonToCollapsed);
				}
				else
					break;

				if (bInsertPrevButton)
				{
					bInsertPrevButton = FALSE;
					// Add previous button
					pButtonToCollapsed = FindCollapsableButton(nPos--);
					if (pButtonToCollapsed)
					{
						bRecalc = ButtonsOverlapAdd(pButtonToCollapsed);
					}
				}

				ButtonsOverlapMenuUpdate();
				RepositionRightButtonsCalcButtonsSpace(rectDockBar, widthLeft, widthRight);
				int iRightButtonPrev = iRightButton;
				iRightButton = dockBarWidth - widthLeft - widthRight - SPACE_TOOLBAR_RIGHT;
				if (iRightButtonPrev >= iRightButton && iRightButton < 0)
					break;

				// Not permise the loop over 200 attempts
				prtLoop++;
				if (prtLoop > 50)
					break;
			}
		}
		else
		{
			// Not in Overlap
			bRecalc = ButtonsOverlapRemove(iRightButton);
			if (m_pMenuButtonCollapsed && m_pMenuButtonCollapsed->IsVisible())
			{
				ButtonsOverlapMenuUpdate();
			}
		}


		RepositionRightButtonsCalcButtonsSpace(rectDockBar, widthLeft, widthRight);
		iRightButton = dockBarWidth - widthLeft - widthRight - SPACE_TOOLBAR_RIGHT;
		bRet = TRUE;
	}

	if (m_pObjectRight)
	{
		if (iRightButton < 0)
			iRightButton = 0;

		if (!(this->GetBarStyle() & CBRS_GRIPPER))
		{
			// Fill the space in single ToolBar not gripper
			iRightButton += 5;
		}

		if (iRightButton > 0 && m_iWidthObjectRight != iRightButton)
		{
			m_pObjectRight->SetWidth(iRightButton); // space to DX
			m_iWidthObjectRight = iRightButton;
			bRet = TRUE;
		}
	}
	else
	{
		iRightButton = 0;
	}

	// Left & Center
	if (m_pObjectLeft && m_bCenterButtons)
	{
		if (widthLeft > 0)
		{
			int w = (INT)((dockBarWidth - widthRight) / 2 - widthLeft / 2);
			if (iRightButton > 0)
			{
				int iWidth = iRightButton - w;
				if (m_iWidthObjectRight != iWidth)
				{
					m_iWidthObjectRight = iWidth;
					m_pObjectRight->SetWidth(iWidth);
					bRet = TRUE;
				}
			}

			if (m_iWidthObjectLeft != w)
			{
				m_pObjectLeft->SetWidth(w);
				m_iWidthObjectLeft = w;
				bRet = TRUE;
			}
		}
		else
		{
			int iWidth = iRightButton - m_pObjectLeft->Rect().Width();
			if (m_iWidthObjectRight != iWidth)
			{
				m_iWidthObjectRight = iWidth;
				m_pObjectRight->SetWidth(iWidth);
				bRet = TRUE;
			}

		}
	}
	return bRet;
}

//----------------------------------------------------------------------------
void CTBToolBar::AdjustSizeImmediate(BOOL bRecalcLayout /*= TRUE*/)
{
	if (m_pParentTabbedToolbar)
	{
		m_iWidthObjectLeft = m_iWidthObjectRight = 0;
		int dockBarWidth = max(GetDockBarRect().Width(), m_pParentTabbedToolbar->GetMaxWidth());
		CSize sizeCurr = CalcFixedLayout(FALSE, IsHorizontal());
		SetWindowPos(NULL, 0, 0, dockBarWidth, sizeCurr.cy, SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER);
		return;
	}
	__super::AdjustSizeImmediate(bRecalcLayout);
}

//----------------------------------------------------------------------------
CBCGPToolbarButton* CTBToolBar::FindCollapsableButton(INT nPosFrom)
{
	CBCGPToolbarButton* pButtonToCollapsed = NULL;
	while (nPosFrom > 0)
	{
		nPosFrom--;
		pButtonToCollapsed = GetButton(nPosFrom);

		if (IsToolbarInfinity() && nPosFrom == 0)
			continue;

		int nID = pButtonToCollapsed->m_nID;

		if (m_pParentTabbedToolbar && nID == m_pParentTabbedToolbar->GetIDSwitch())
			continue;

		if (nID < 0)
			continue;

		if ((pButtonToCollapsed->IsVisible() && (m_nIDOverlapButton != nID) && !(pButtonToCollapsed->m_nStyle & TBBS_SEPARATOR)) || nPosFrom < 0)
		{
			return pButtonToCollapsed;
		}
	}
	return NULL;
}

//----------------------------------------------------------------------------
void CTBToolBar::DoPaint(CDC* pDCPaint)
{
	// Paint in screen the ToolBar
	__super::DoPaint(pDCPaint);
}

// CalcMinToolBar:
// considera tutti i bottoni anche quelli nascosti e non attivi in modo da 
// non cambiare qundo cambia lo stato dei bottoni
//----------------------------------------------------------------------------
INT CTBToolBar::CalcMinToolBar()
{
	INT iButton = 0;
	CClientDC dc(this);
	CFont* pOldFont = SelectDefaultFont(&dc);
	ASSERT(pOldFont != NULL);
	CSize sizeButon = GetButtonSize();
	INT iMinHeight = sizeButon.cy;
	for (POSITION pos = m_Buttons.GetHeadPosition(); pos != NULL; iButton++)
	{
		CBCGPToolbarButton* pButton = dynamic_cast<CBCGPToolbarButton*> (m_Buttons.GetNext(pos));
		if (!pButton) continue;
		CRect rectText(0, 0, sizeButon.cx * 3, sizeButon.cy);
		UINT uiTextFormat = DT_CENTER | DT_CALCRECT | DT_WORDBREAK;
		dc.DrawText(pButton->m_strText, rectText, uiTextFormat);
		int iHeightMax = sizeButon.cy + rectText.Height() + ScalePix(CY_BORDER);
		iMinHeight = max(iMinHeight, iHeightMax);
	}
	dc.SelectObject(pOldFont);
	return iMinHeight;
}

//----------------------------------------------------------------------------
BOOL CTBToolBar::DrawButton(CDC* pDC, CBCGPToolbarButton* pButton, CBCGPToolBarImages* pImages, BOOL bHighlighted, BOOL bDrawDisabledImages)
{
	BOOL bHorz = GetCurrentAlignment() & CBRS_ORIENT_HORZ ? TRUE : FALSE;
	if (!bHorz) {
		return __super::DrawButton(pDC, pButton, pImages, bHighlighted, bDrawDisabledImages);
	}

	// Calculate for determinate if tollbar in one o two row text
	INT nToolBarH = CalcMaxButtonHeight() - TOOL_BAR_SPACE_HEIGHT;

	INT nMinBtnHeight = 0;
	if (m_pParentTabbedToolbar)
	{
		// all toolbar in toolBar
		nMinBtnHeight = m_pParentTabbedToolbar->CalcMinToolBar();
	}
	else
	{
		// single tollBar
		nMinBtnHeight = CalcMinToolBar();
	}

	// Not center the button if not have the space - e.s. 2 thext line write, include disable button
	if (nToolBarH - nMinBtnHeight <= 0)
		return __super::DrawButton(pDC, pButton, pImages, bHighlighted, bDrawDisabledImages);

	// Center the button
	CRect	buttonRect = pButton->Rect();
	// Get button size
	CSize sizeButon = GetButtonSize();
	INT space = ((buttonRect.Height() - sizeButon.cy) / 2);

	if (space > 0)
	{
		buttonRect.top += (space / 2);
	}

	//---------------------
	// Draw button context:
	//---------------------
	pButton->OnDraw(pDC, buttonRect, pImages, bHorz,
		IsCustomizeMode() && !m_bAltCustomizeMode && !m_bLocked,
		bHighlighted, m_bShowHotBorder,
		m_bGrayDisabledButtons && !bDrawDisabledImages);

	return TRUE;
}

//----------------------------------------------------------------------------
void CTBToolBar::OnFillBackground(CDC* pDC)
{
	CRect rcClient;
	GetClientRect(&rcClient);

	pDC->FillRect(rcClient, &CBrush(m_clrBkgColor));

	if (IsToolbarInfinity())
	{
		rcClient.top = rcClient.bottom - AfxGetThemeManager()->GetToolbarLineDownHeight();
		pDC->FillRect(rcClient, &CBrush(AfxGetThemeManager()->GetToolbarLineDownColor()));
	}

	int nButtons = GetCount();
	if (nButtons >= 1)
	{
		CTBToolbarButton* pButton = dynamic_cast<CTBToolbarButton*>(GetButton(0));
		if (pButton && pButton->IsSecondaryFillColorButton())
		{
			rcClient.right = pButton->getRectButton().right;
			pDC->FillSolidRect(rcClient, AfxGetThemeManager()->GetToolbarBkgSecondaryColor());
		}
	}

}

//----------------------------------------------------------------------------
LRESULT CTBToolBar::OnFillAutoComple(WPARAM wp, LPARAM lp)
{
	CFrameWnd* pFrameWnd = GetParentFrame();
	if (pFrameWnd)
		return pFrameWnd->SendMessage(BCGM_EDIT_ON_FILL_AUTOCOMPLETE_LIST, wp, lp);

	return 0;
}

//Metodo che restituisce una CWndObjDescription che rappresenta la CTBToolBar. Ne valorizza solo il rettangolo che occupa.
//Imposta il tipo a auxradartoolbar
//-----------------------------------------------------------------------------
CWndObjDescription* CTBToolBar::GetControlStructure(CWndObjDescriptionContainer* pContainer)
{
	CWndObjDescription *pDummyToolbarDesc = (pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndObjDescription)));
	pDummyToolbarDesc->UpdateAttributes(this);
	pDummyToolbarDesc->m_Type = CWndObjDescription::Toolbar;
	return pDummyToolbarDesc;
}

void CTBToolBar::WriteTabName(CWndObjDescription* pDescription)
{
	CString sName = this->GetName();
	if (pDescription->m_strText != sName)
	{
		pDescription->m_strText = this->GetName();
		pDescription->SetUpdated(&pDescription->m_strText);
	}
}

//-----------------------------------------------------------------------------
BOOL CTBToolBar::PreTranslateMessage(MSG* pMsg)
{
	CWnd* pFocus = GetFocus();
	if (pFocus && pFocus->IsKindOf(RUNTIME_CLASS(CBCGPToolbarEditCtrl)) && pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_DELETE)
	{
		((CBCGPToolbarEditCtrl*)pFocus)->Clear();
		return TRUE;
	}
	else if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_ESCAPE)
	{
		if ((GetParent()->GetStyle() & WS_POPUP) == WS_POPUP)
			return GetParent()->PreTranslateMessage(pMsg);

		CFrameWnd* pParentFrame = GetParentFrame();
		if (pParentFrame)
		{
			pParentFrame->SendMessage(WM_COMMAND, ID_EXTDOC_ESCAPE);
			return TRUE;
		}
	}

	CAbstractFrame* pParentFrame = dynamic_cast<CAbstractFrame*>(GetParentFrame());
	if (pParentFrame && pParentFrame->IsDestroying())
		return TRUE;
		
	return __super::PreTranslateMessage(pMsg);
}

//-----------------------------------------------------------------------------
void  CTBToolBar::DisableButtonAutoHidden(UINT nID, BOOL bDisable /* = TRUE*/)
{
	BOOL ret;
	if (m_mapButtonNotAutoHidden.Lookup(nID, ret)) {
		if (!bDisable) {
			m_mapButtonNotAutoHidden.RemoveKey(nID);
			return;
		}
	}

	m_mapButtonNotAutoHidden.SetAt(nID, bDisable);
}

//-----------------------------------------------------------------------------
void  CTBToolBar::ShowCommandMessageString(UINT uiCmdId)
{
}

//-----------------------------------------------------------------------------
HRESULT CTBToolBar::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace;
	// varChild.intval == 0 means the entire toolbar
	if (varChild.intVal == 0)
	{
		sNamespace = cwsprintf(_T("{0-%s}{1-%s}"), GetInfoOSL()->m_Namespace.GetObjectName(), GetInfoOSL()->m_Namespace.GetTypeString());
	}
	// varChild.intval > 0, it is the 1-based index of the visible child clicked
	else
	{
		int btn = 1;
		CBCGPToolbarButton* pBtn = NULL;
		int btnCount = GetCount();
		// find the button among those visible, not ghost and not separator
		for (int idx = 0; idx < GetCount(); idx++)
		{
			pBtn = GetButton(idx);
			if (
				(dynamic_cast<CTBToolbarButton*>(pBtn) && dynamic_cast<CTBToolbarButton*>(pBtn)->IsGhost()) || //@@TODO chiarire bene il discorso del ghost
				(pBtn->m_nStyle & TBBS_SEPARATOR) == TBBS_SEPARATOR ||
				!pBtn->IsVisible()
				)
				continue;

			if (btn++ >= varChild.intVal)
				break;
		}

		if (pBtn->m_nID == ID_EXDOC_TAB_SWITCH)
		{
			sNamespace = _T("SwitchToolbarButton"); //"switch" button do not have a namespace assigned
		}
		else if (dynamic_cast<CTBToolbarLabel*>(pBtn) && dynamic_cast<CTBToolbarLabel*>(pBtn)->IsRightSpace())
		{
			sNamespace = _T("RightSpaceToolbarButton"); //"right space" button is a special one
		}
		else
		{
			CInfoOSLButton* pInfo = pBtn ? GetInfoOSLButton(pBtn->m_nID) : NULL;
			if (pInfo)
			{
				sNamespace = cwsprintf(_T("{0-%s}{1-%s}"), pInfo->m_Namespace.GetObjectName(), pInfo->m_Namespace.GetTypeString());
			}
			else
			{
				sNamespace = _T("UnknownToolbarButton"); // should never occur
			}
		}
	}

	*pszName = ::SysAllocString(sNamespace);

	return S_OK;
}

//*************************************************************************************
void CTBToolBar::OnLButtonUp(UINT nFlags, CPoint point)
{
	__super::OnLButtonUp(nFlags, point);

	// durante la morte non e piu' in grado di lavorare
	if (!IsWindow(m_hWnd))
		return;

	CAbstractFrame* pFrame = dynamic_cast<CAbstractFrame*>(GetParentFrame());
	if (!pFrame || pFrame->IsDestroying())
		return;

	// now hit test against CBCGPToolBar buttons
	INT_PTR nHit = this->HitTest(point);

	if (nHit == -1)
		return;

	CTBToolbarMenuButton* pMenuButton = dynamic_cast<CTBToolbarMenuButton*> (GetButton((int)nHit));

	if (pMenuButton && pMenuButton->HasMissingClick() && pMenuButton->GetAlwaysDropDown() == MIXED_ALWAYS_DROPDOWN)
	{
		// Check for posible problem with messageBox.
		POINT pMouseReal;
		if (GetCursorPos(&pMouseReal))
		{
			ScreenToClient(&pMouseReal);
			CRect rect = pMenuButton->Rect();
			if (!rect.PtInRect(pMouseReal))
				return;
		}

		pMenuButton->OpenPopupMenu();

		if (!IsWindow(m_hWnd) || !pFrame || pFrame->IsDestroying())
			return;

		pMenuButton->SetMissingClick(FALSE);
	}
}


/////////////////////////////////////////////////////////////////////////////
//					CTBTabWndToolbar
/////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CTBTabWndToolbar, CBCGPTabWnd)
	//{{AFX_MSG_MAP(CTBTabWndToolbar)
	ON_WM_PAINT()

	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONDBLCLK()
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------
CTBTabWndToolbar::CTBTabWndToolbar() :
	CBCGPTabWnd()
{
	m_bShowToolBarTab = AfxGetThemeManager()->ShowToolBarTab();

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-------------------------------------------------------------------------------------
CTBTabWndToolbar::~CTBTabWndToolbar()
{
	//LUCA: questi sono font bcg che non vengono rilasciati...
	if (m_fntTabs.m_hObject)
		m_fntTabs.DeleteObject();

	if (m_fntTabsBold.m_hObject)
		m_fntTabsBold.DeleteObject();

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//-------------------------------------------------------------------------------------
void CTBTabWndToolbar::OnDraw(CDC* pDC)
{
	__super::OnDraw(pDC);
}

//-------------------------------------------------------------------------------------
void CTBTabWndToolbar::OnPaint()
{
	__super::OnPaint();
}

//-------------------------------------------------------------------------------------
int CTBTabWndToolbar::GetTabsHeight() const
{
	if (!m_bShowToolBarTab) { return 0; }
	return __super::GetTabsHeight();
}

//-------------------------------------------------------------------------------------
void CTBTabWndToolbar::OnLButtonDown(UINT nFlags, CPoint point)
{
	if (!m_bShowToolBarTab) { return; }
	__super::OnLButtonDown(nFlags, point);
}

//-------------------------------------------------------------------------------------
void CTBTabWndToolbar::OnLButtonUp(UINT nFlags, CPoint point)
{
	if (!m_bShowToolBarTab) { return; }
	__super::OnLButtonUp(nFlags, point);
}

//-------------------------------------------------------------------------------------
void CTBTabWndToolbar::OnLButtonDblClk(UINT nFlags, CPoint point)
{
	if (!m_bShowToolBarTab) { return; }
	__super::OnLButtonDblClk(nFlags, point);
}

//-------------------------------------------------------------------------------------
void CTBTabWndToolbar::OnMouseMove(UINT nFlags, CPoint point)
{
	if (!m_bShowToolBarTab) { return; }
	__super::OnLButtonDblClk(nFlags, point);
}

//-----------------------------------------------------------------------------
HRESULT CTBTabWndToolbar::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace = _T("TBTabWndToolbar"); // no need for a qualified name
	*pszName = ::SysAllocString(sNamespace);

	return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
//					CTBTabbedToolbar
/////////////////////////////////////////////////////////////////////////////

BEGIN_MESSAGE_MAP(CTBTabbedToolbar, CBCGPTabbedToolbar)
	ON_REGISTERED_MESSAGE(BCGM_CHANGE_ACTIVE_TAB, OnChangeActiveTab)
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	ON_MESSAGE(UM_ACTIVATE_TAB_PAGE, OnSetActiveTab)

	ON_WM_LBUTTONDBLCLK()
	ON_WM_CREATE()
	ON_WM_SIZE()
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------
CTBTabbedToolbar::CTBTabbedToolbar()
{
	m_bUseLargeButtons = TRUE;
	m_bSuspendLayout = FALSE;
	m_nIDOverlapButtonStart = GET_ID_RANGE(ID_COLLAPSED_BUTTON_RANGE_START, NUMBER_OF_BUTTONS);
	m_nWidth = 0;
	m_iIdSwitch = ID_EXDOC_TAB_SWITCH;
	m_iIdSwitchMenuStart = ID_EXDOC_TAB_SWITCH_MENU_START;
	m_bSwitchEnableAlwaysDropDown = FALSE;
	m_pParentDoc = NULL;
	m_bAutoHideToolBarButton = AfxGetThemeManager()->AutoHideToolBarButton();
	m_bShowToolBarTab = AfxGetThemeManager()->ShowToolBarTab();
	m_bHighlightedColorClickEnable = AfxGetThemeManager()->ToolbarHighlightedColorClickEnable();

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-------------------------------------------------------------------------------------
CTBTabbedToolbar::~CTBTabbedToolbar()
{
	CAbstractFrame* pParentFrame = dynamic_cast<CAbstractFrame*>(GetParentFrame());

	if (pParentFrame)
		pParentFrame->SetDestroying();

	for (int i = m_Toolbars.GetCount() - 1; i >= 0; i--)
		SAFE_DELETE(m_Toolbars.GetAt(i));

	if (m_pTabWnd) {
		SAFE_DELETE(m_pTabWnd);
	}

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::Create(CWnd* pParentWnd, CStringArray* pNamespaceArray)
{
	if (!pParentWnd || !::IsWindow(pParentWnd->m_hWnd))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (pParentWnd->IsKindOf(RUNTIME_CLASS(CLocalizableFrame)))
	{
		CDocument* pDoc = ((CLocalizableFrame*)pParentWnd)->GetDocument();
		ASSERT(pDoc);
		if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
		{
			m_pParentDoc = pDoc;
		}

	}

	if (!CreateEx(pParentWnd, dwDefaultToolbarStyle /* | TBSTYLE_FLAT |  TBSTYLE_TRANSPARENT */))
	{
		TRACE0("Failed to create tabbed toolbar\n");
		return -1;      // fail to create
	}

	this->m_bShowHotBorder = FALSE;

	if (AfxGetThemeManager()->UseFlatStyle())
	{
		this->GetUnderlinedWindow()->ModifyTabStyle(CBCGPTabWnd::STYLE_FLAT);
	}

	m_pTabWnd->ModifyTabStyle(CBCGPTabWnd::STYLE_FLAT_SHARED_HORZ_SCROLL);
	m_pTabWnd->SetFlatFrame(TRUE);
	EnableDocking(pParentWnd);

	return TRUE;
}

// CBCGPTabbedToolbar message handlers
//-------------------------------------------------------------------------------------
int CTBTabbedToolbar::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	//non chiamo quella del papà per evitare la creazione della m_pTabWnd, perché devo creare la mia
	if (CBCGPBaseToolBar::OnCreate(lpCreateStruct) == -1)
		return -1;

	//in ambito web, non la aggiungo alla variabile globale, per via del multithreading
	//questa istruzione è invocata dalla classe madre, siccome io invece chiamo la OnCreate della nonna, devo invocarla io
	if (!AfxIsRemoteInterface())
		((CObList&)CBCGPToolBar::GetAllToolbars()).AddTail(this);

	ASSERT(m_pTabWnd == NULL);

	m_pTabWnd = new CTBTabWndToolbar();

	CRect rectClient(0, 0, lpCreateStruct->cx, lpCreateStruct->cy);
	// Create tabs window:
	if (!m_pTabWnd->Create(AfxIsRemoteInterface() ? CBCGPTabWnd::STYLE_FLAT : CBCGPTabWnd::STYLE_3D, rectClient, this, 101, CBCGPTabWnd::LOCATION_TOP))
	{
		TRACE0("Failed to create tab window\n");
		delete m_pTabWnd;
		m_pTabWnd = NULL;
		return -1;      // fail to create
	}

	SetShowToolBarTab(m_bShowToolBarTab);

	m_pTabWnd->AutoDestroyWindow(FALSE);
	m_pTabWnd->HideSingleTab(FALSE);

	m_pTabWnd->m_bEnableWrapping = FALSE;
	m_pTabWnd->EnableTabSwap(FALSE);
	return 0;
}

//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::SetShowToolBarTab(BOOL bAutoHide /*= TRUE*/)
{
	ASSERT(m_pTabWnd);
	if (!m_pTabWnd) return;
	CTBTabWndToolbar* pTbTabWnd = dynamic_cast<CTBTabWndToolbar*> (m_pTabWnd);
	ASSERT(pTbTabWnd);
	if (!pTbTabWnd) return;
	m_bShowToolBarTab = bAutoHide;
	pTbTabWnd->SetShowToolBarTab(m_bShowToolBarTab);

	// Update ToolBar
	CBCGPTabWnd*	m_pTabsWnd = this->GetUnderlinedWindow();
	if (!m_pTabsWnd)
		return;
	int nTabCount = m_pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* m_pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (m_pTab);
		if (pToolBar)
		{
			pToolBar->SetShowToolBarTab(m_bShowToolBarTab);
		}
	}
}

//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::EnableDocking(CWnd* pParentWnd, DWORD dwAlignment)
{
	if (!pParentWnd)
		return;

	this->SetBarStyle(this->GetBarStyle() & ~(CBRS_GRIPPER | CBRS_SIZE_DYNAMIC));
	__super::EnableDocking(dwAlignment);

	CDockableFrame* pDockableFrame = dynamic_cast<CDockableFrame*> (pParentWnd);
	if (pDockableFrame)
	{
		pDockableFrame->EnableDocking(CBRS_ALIGN_ANY);
		pDockableFrame->DockControlBar(this);
	}
}

//----------------------------------------------------------------------------
BOOL CTBTabbedToolbar::SuspendLayout()
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	if (pFrame)
		pFrame->m_bDelayedLayoutSuspended = m_bSuspendLayout;

	if (m_bSuspendLayout)
		return FALSE;

	m_bSuspendLayout = TRUE;
	if (pFrame)
		pFrame->m_bDelayedLayoutSuspended = m_bSuspendLayout;

	if (pFrame)
		TRACE(_T("CTBTabbedToolbar::SuspendLayout - m_bSuspendLayout = %s\n"), (LPCTSTR)(DataBool(pFrame->m_bDelayedLayoutSuspended)).FormatData());

	// Update ToolBar
	CBCGPTabWnd*	m_pTabsWnd = this->GetUnderlinedWindow();
	if (!m_pTabsWnd)
	{
		return TRUE;
	}

	int nTabCount = m_pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* m_pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (m_pTab);
		if (pToolBar)
		{
			pToolBar->SuspendLayout();
		}
	}

	return TRUE;
}


//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::ResumeLayout(BOOL bForced /*= FALSE*/)
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	if (pFrame)
		pFrame->m_bDelayedLayoutSuspended = m_bSuspendLayout;

	if (!m_bSuspendLayout) return;

	m_bSuspendLayout = FALSE;
	if (pFrame)
		pFrame->m_bDelayedLayoutSuspended = m_bSuspendLayout;

	if (pFrame)
		TRACE(_T("CTBTabbedToolbar::ResumeLayout - m_bSuspendLayout = %s\n"), (LPCTSTR)(DataBool(pFrame->m_bDelayedLayoutSuspended)).FormatData());

	// Update ToolBar
	CBCGPTabWnd*	m_pTabsWnd = this->GetUnderlinedWindow();
	if (!m_pTabsWnd)
		return;
	int nTabCount = m_pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* m_pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (m_pTab);
		if (pToolBar)
		{
			pToolBar->ResumeLayout(bForced);
		}
	}

	UpdateTabWnd();
}

//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::DoUpdateSize()
{
	CWnd* pParentWnd = this->GetParent();
	if (pParentWnd && m_pTabWnd && m_pTabWnd->m_hWnd)
	{
		CRect rectTabbed;
		GetClientRect(rectTabbed);

		// Get height of tollBar
		CBCGPTabWnd*	pTabsWnd = (this->GetUnderlinedWindow());
		CWnd* pTab = pTabsWnd->GetTabWnd(pTabsWnd->GetActiveTab());
		INT nHeight = rectTabbed.Height();
		if (pTab)
		{
			CTBToolBar* m_pToolBar = dynamic_cast<CTBToolBar*> (pTab);
			if (m_pToolBar)
			{
				CSize sizeCurr = m_pToolBar->CalcFixedLayout(FALSE, IsHorizontal());
				nHeight = max(nHeight, sizeCurr.cy + 1);
			}
		}

		CWnd* pParentWnd = this->GetParent();

		CRect m_rectMasterFrame;
		CRect rectTab;

		m_pTabWnd->GetClientRect(rectTab);
		pParentWnd->GetClientRect(m_rectMasterFrame);

		m_nWidth = m_rectMasterFrame.Width() - (m_pTabWnd->GetTabBorderSize() * 2 + 2);
		/*width = max(width, m_nWidth);*/

		rectTabbed.right = rectTabbed.left + m_nWidth;
		rectTabbed.bottom = rectTabbed.top + nHeight;

		rectTab.right = rectTab.left + m_nWidth;
		rectTab.bottom = rectTab.top + nHeight;

		// Tabbed
		//SetWindowPos(NULL, rectTabbed.left, rectTabbed.top, rectTabbed.right, rectTabbed.bottom, SWP_NOZORDER | SWP_NOACTIVATE);

		// Tab
		m_pTabWnd->SetWindowPos(NULL, rectTab.left, rectTab.top, rectTab.right, rectTab.bottom, SWP_NOZORDER | SWP_NOACTIVATE);

		if (!IsLayoutSuspended())
		{
			int nTabActive = m_pTabWnd->GetActiveTab();
			CWnd* m_pTab = m_pTabWnd->GetTabWnd(nTabActive);
			if (!m_pTab) return;
			CTBToolBar* m_pToolBar = dynamic_cast<CTBToolBar*> (m_pTab);
			if (!m_pToolBar) return;
			m_pToolBar->AdjustLayout();
		}
	}

}

//-------------------------------------------------------------------------------------
int CTBTabbedToolbar::CalcMaxButtonHeight()
{
	CBCGPTabWnd*	pTabsWnd = (this->GetUnderlinedWindow());
	CWnd* pTab = pTabsWnd->GetTabWnd(pTabsWnd->GetActiveTab());
	if (pTab)
	{
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			return pToolBar->CalcMaxButtonHeight();
		}
	}
	return 0;
}


//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::OnSize(UINT nType, int cx, int cy)
{
	AdjustLayoutImmediate();
}

//----------------------------------------------------------------------------
void CTBTabbedToolbar::DoPaint(CDC* pDCPaint)
{
	if (IsLayoutSuspended() || !IsTBWindowVisible(this))
		return;

	__super::DoPaint(pDCPaint);
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::HideGhostButtons(const	UINT* HideIDs, const UINT* GhostIDs, const UINT* ShowIDs)
{
	if (m_bAutoHideToolBarButton)
		return TRUE;

	int i;
	// -------------------------
	i = 0;
	if (ShowIDs)
	{
		while (ShowIDs[i] != 0)
		{
			BOOL res = HideButton(ShowIDs[i], FALSE);
			res = GhostButton(ShowIDs[i], FALSE);
			i++;
		}
	}

	// -------------------------
	i = 0;
	if (HideIDs)
	{
		while (HideIDs[i] != 0)
		{
			BOOL res = HideButton(HideIDs[i]);
			i++;
		}
	}

	// -------------------------
	i = 0;
	if (GhostIDs)
	{
		while (GhostIDs[i] != 0)
		{
			BOOL res = HideButton(GhostIDs[i], FALSE);
			res = GhostButton(GhostIDs[i]);
			i++;
		}
	}

	AdjustLayoutImmediate();

	return TRUE;
}

//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::AdjustLayoutImmediate()
{
	DoUpdateSize();
}

//-------------------------------------------------------------------------------------
INT CTBTabbedToolbar::FindButton(CString sNameSpace)
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		int nID = pToolBar->FindButton(sNameSpace);
		if (nID >= 0)
		{
			return nID;
		}
	}
	return -1;
}

//-------------------------------------------------------------------------------------
INT	CTBTabbedToolbar::FindButton(UINT nID)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		int n = pToolBar->FindButton(nID);
		if (n >= 0) return n;
	}
	return -1;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::HideButton(UINT nID, BOOL bHide)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	BOOL bRet = FALSE;
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar == NULL) return FALSE;
		if (pToolBar->HideButton(nID, bHide))
		{
			bRet = TRUE;
		}
	}
	return bRet;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::IsHideButton(UINT nID)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		BOOL rHide = pToolBar->IsHideButton(nID);
		if (rHide)
		{
			return TRUE;
		}
	}
	return FALSE;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::DeleteButton(UINT nID)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	BOOL bRet = FALSE;
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);

		int ni = pToolBar->FindButton(nID);
		if (ni >= 0 && pToolBar->RemoveButton(ni))
		{
			bRet = TRUE;
		}
	}
	return bRet;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::SetCollapsedImage(CString stImageNameSpace)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	BOOL bRet = FALSE;
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		pToolBar->SetCollapsedImage(stImageNameSpace);
		bRet = TRUE;
	}
	return bRet;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::ChangeImage(UINT nID, const CString& sImageNameSpace, const CString& sImageAlternativeNameSpace, const CString& sCollapsedImageNameSpace /*= _T("")*/)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	BOOL bRet = FALSE;
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		int ni = pToolBar->FindButton(nID);
		if (ni >= 0)
		{
			pToolBar->ChangeImage(nID, sImageNameSpace, sImageAlternativeNameSpace, sCollapsedImageNameSpace);
			bRet = TRUE;
		}
	}
	return bRet;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::ChangeImage(UINT nID, UINT nIDImag, UINT nIDImageAlternative, BOOL bPng /*= TRUE*/)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	BOOL bRet = FALSE;
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		int ni = pToolBar->FindButton(nID);
		if (ni >= 0)
		{
			pToolBar->ChangeImage(nID, nIDImag, nIDImageAlternative, bPng);
			bRet = TRUE;
		}
	}
	return bRet;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::GhostButton(UINT nID, BOOL bGhost)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	BOOL bRet = FALSE;
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar->GhostButton(nID, bGhost))
		{
			bRet = true;
		}
	}
	return bRet;
}

//-------------------------------------------------------------------------------------
CTBToolBar* CTBTabbedToolbar::FindToolBar(UINT nCommandID)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			int nIndex = pToolBar->FindButton(nCommandID);
			if (nIndex > -1) return pToolBar;
		}
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::RenameTab(LPCTSTR lpszToolBarName, CString stNewTabLabel)
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			if (pToolBar->GetName().Compare(lpszToolBarName) == 0)
			{
				pTabsWnd->SetTabLabel(i, stNewTabLabel);
				return TRUE;
			}
		}
	}
	return FALSE;
}

//-------------------------------------------------------------------------------------
CTBToolBar*	CTBTabbedToolbar::GetToolBar(int nTab)
{
	CWnd* pTab = m_pTabWnd->GetTabWnd(nTab);
	CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
	return pToolBar;
}

//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::ClosePopupMenu()
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	if (pTabsWnd)
	{
		int nTabCount = pTabsWnd->GetTabsNum();
		for (int i = 0; i < nTabCount; i++)
		{
			CTBToolBar* pToolBarBar = GetToolBar(i);
			if (pToolBarBar)
			{
				CBCGPToolbarMenuButton* pCurrPopupMenu = pToolBarBar->GetDroppedDownMenu();
				if (pCurrPopupMenu)
					pCurrPopupMenu->OnCancelMode();
			}
		}
	}
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::SetActiveTab(LPCTSTR lpszText)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* m_pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (m_pTab);
		if (pToolBar)
		{
			if (pToolBar->GetName().Compare(lpszText) == 0)
			{
				pTabsWnd->SetActiveTab(i);
				return TRUE;
			}
		}
	}
	return FALSE;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::RemoveTab(LPCTSTR lpszText)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			if (pToolBar->GetName().Compare(lpszText) == 0)
			{
				m_pTabWnd->RemoveTab(i);
				for (int i = m_Toolbars.GetCount() - 1; i >= 0; i--)
				{
					if (m_Toolbars.GetAt(i)->GetName().Compare(lpszText) == 0)
					{
						m_Toolbars.RemoveAt(i);
					}
				}

				if (pToolBar->m_hWnd)
					pToolBar->DestroyWindow();
				SAFE_DELETE(pToolBar);
				return TRUE;
			}
		}
	}
	return FALSE;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::SetActiveTab(UINT nTabId)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			CString sTabID = CWndObjDescriptionContainer::GetCtrlID(pToolBar->m_hWnd);
			if (_ttoi(sTabID) == nTabId)
			{
				pTabsWnd->SetActiveTab(i);
				return TRUE;
			}
		}
	}
	return FALSE;
}

//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::AddDocumentTitle(CString strTitle)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return;

	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			pToolBar->AddDocuemntTitle(strTitle);
		}
	}
}

// Update toolBar switch button
//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::UpdateTabWnd(INT nTab)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd) return;
	CWnd* pTab = m_pTabWnd->GetTabWnd(nTab);
	if (!pTab) return;
	CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
	if (!pToolBar) return;
	pToolBar->RepositionRightButtons();
	pToolBar->AdjustSizeImmediate();
	pToolBar->AdjustLayout();
}

//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::UpdateTabWnd()
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd) return;
	int nTabActive = pTabsWnd->GetActiveTab();
	UpdateTabWnd(nTabActive);
}

//-------------------------------------------------------------------------------------
CTBToolBar*	CTBTabbedToolbar::GetToolBarActive()
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return NULL;
	int nTab = pTabsWnd->GetActiveTab();
	CWnd* pTab = m_pTabWnd->GetTabWnd(nTab);
	if (!pTab)
		return NULL;
	CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
	ASSERT(pToolBar);
	return pToolBar;
}

//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::MoveNextTab()
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd) return;
	int nTabCount = pTabsWnd->GetTabsNum();
	int nTabActive = pTabsWnd->GetActiveTab();
	int nTabSel = nTabActive + 1;

	while (nTabActive != nTabSel)
	{
		if (nTabSel >= nTabCount) { nTabSel = 0; }
		CWnd* m_pTab = m_pTabWnd->GetTabWnd(nTabSel);
		CTBToolBar* ToolBarTab = dynamic_cast<CTBToolBar*> (m_pTab);
		if (ToolBarTab->IsVisible()) {
			break;
		}
		nTabSel++;
	}


	pTabsWnd->SetActiveTab(nTabSel);

	// Update toolBar switch button
	UpdateTabWnd(nTabSel);
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::SetActiveTabByPos(UINT nPos)
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd) return FALSE;
	UINT nTabCount = (UINT)pTabsWnd->GetTabsNum();
	if (nPos >= nTabCount) { return FALSE; }
	pTabsWnd->SetActiveTab(nPos);
	// Update toolBar switch button
	UpdateTabWnd(nPos);
	return TRUE;
}

//-------------------------------------------------------------------------------------
int CTBTabbedToolbar::GetActiveTab()
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	return pTabsWnd->GetActiveTab();
}

//-------------------------------------------------------------------------------------
INT CTBTabbedToolbar::FindToolBarPos(LPCTSTR lpszText)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = pTabsWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			if (pToolBar->GetName().Compare(lpszText) == 0)
				return i;
		}
	}
	return -1;
}

//-------------------------------------------------------------------------------------
CTBToolBar*	CTBTabbedToolbar::FindToolBar(LPCTSTR lpszText)
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			if (pToolBar->GetName().Compare(lpszText) == 0)
				return pToolBar;
		}
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
CInfoOSLButton* CTBTabbedToolbar::FindOslInfoButton(CString sNameSpace)
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			CInfoOSLButton* pInfo = pToolBar->FindOslInfoButton(sNameSpace);
			if (pInfo)
				return pInfo;
		}
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
CInfoOSLButton* CTBTabbedToolbar::FindOslInfoButton(UINT nID)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		CInfoOSLButton* pInfo = pToolBar->GetInfoOSLButton(nID);
		if (pInfo)
		{
			return pInfo;
		}
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
CTBToolBar*	CTBTabbedToolbar::FindToolBarOrAdd(CWnd* pParentWnd, LPCTSTR lpszText, LPCTSTR lpszLabel /*= NULL*/)
{
	CTBToolBar* pToolBar = FindToolBar(lpszText);
	if (pToolBar) return pToolBar;
	pToolBar = new CTBToolBar();

	CString sLabel = _T("");
	if (lpszLabel != NULL) sLabel = lpszLabel;

	if (!pToolBar->CreateEmptyTabbedToolbar(pParentWnd, lpszText, sLabel))
	{
		TRACE("Failed to create the toolBar.\n");
		return NULL;
	}
	AddTab(pToolBar, TRUE, FALSE, TRUE);
	return pToolBar;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::SetDropdown(UINT nCommandID, CMenu* mMenu, LPCTSTR lpszText)
{
	CTBToolBar* pToolBar = FindToolBar(nCommandID);
	if (!pToolBar) return FALSE;
	pToolBar->SetDropdown(nCommandID, mMenu, lpszText);
	return TRUE;
}

//-------------------------------------------------------------------------------------
//BOOL CTBTabbedToolbar::DisableButtonAutoHidden(UINT nID, BOOL bDisable /* = TRUE*/)
//{
//	CTBToolBar* m_pToolBar = FindToolBar(nID);
//	if (!m_pToolBar) return FALSE;
//	m_pToolBar->DisableButtonAutoHidden(nID, bDisable);
//	return TRUE;
//}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::UpdateDropdownMenu(UINT nCommandID, CMenu* mMenu)
{
	CTBToolBar* pToolBar = FindToolBar(nCommandID);
	if (!pToolBar) return FALSE;
	pToolBar->UpdateDropdownMenu(nCommandID, mMenu);
	return TRUE;
}

//-------------------------------------------------------------------------------------
HMENU CTBTabbedToolbar::CopyDropdownMenu(UINT nCommandID)
{
	CTBToolBar* pToolBar = FindToolBar(nCommandID);
	if (!pToolBar) return NULL;
	return pToolBar->CopyDropdownMenu(nCommandID);
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::AddDropdownMenuItem(UINT nCommandID, UINT nFlags, UINT_PTR nIDNewItem /*= NULL*/, LPCTSTR lpszNewItem /*= NULL*/, UINT nIDImgUnchecked /*= 0*/, UINT nIDImgChecked /*= 0*/, BOOL bPng /*= TRUE*/)
{
	CTBToolBar* pToolBar = FindToolBar(nCommandID);
	if (!pToolBar) return FALSE;
	pToolBar->AddDropdownMenuItem(nCommandID, nFlags, nIDNewItem, lpszNewItem, nIDImgUnchecked, nIDImgChecked, bPng);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::InsertDropdownMenuItem(UINT nPos, UINT nCommandID, UINT nFlags, UINT_PTR nIDNewItem /*= NULL*/, LPCTSTR lpszNewItem /*= NULL*/, UINT nIDImgUnchecked /*= 0*/, UINT nIDImgChecked /*= 0*/, BOOL bPng /*= TRUE*/)
{
	CTBToolBar* pToolBar = FindToolBar(nCommandID);
	if (!pToolBar) return FALSE;
	pToolBar->InsertDropdownMenuItem(nPos, nCommandID, nFlags, nIDNewItem, lpszNewItem, nIDImgUnchecked, nIDImgChecked, bPng);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::RemoveDropdown(UINT nCommandID)
{
	CTBToolBar* pToolBar = FindToolBar(nCommandID);
	if (!pToolBar) return FALSE;
	pToolBar->RemoveDropdown(nCommandID);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::SetButtonInfo(UINT nID, UINT nStyle, UINT nIDImage, LPCTSTR lpszText, BOOL bPng)
{
	CTBToolBar* pToolBar = FindToolBar(nID);
	if (!pToolBar) return FALSE;
	int nIndex = pToolBar->FindButton(nID);
	pToolBar->SetButtonInfo(nIndex, nID, nStyle, nIDImage, lpszText, bPng);
	return TRUE;
}

//-------------------------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::SetButtonInfo(int nIndexToolBar, int nIndexButon, UINT nID, UINT nStyle, UINT nIDImage, LPCTSTR lpszText, BOOL bPng)
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (nIndexToolBar > pTabsWnd->GetTabsNum())
		return FALSE;
	int nTabCount = pTabsWnd->GetTabsNum();
	CWnd* pTab = m_pTabWnd->GetTabWnd(nIndexToolBar);
	if (!pTab) return FALSE;

	CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
	if (pToolBar)
	{
		pToolBar->SetButtonInfo(nIndexButon, nID, nStyle, nIDImage, lpszText, bPng);
		return TRUE;
	}

	return FALSE;
}

BOOL CTBTabbedToolbar::SetButtonInfo(UINT nID, UINT nStyle, const CString& sImageNameSpace/* = _T("")*/, LPCTSTR lpszText /*= NULL*/)
{
	CTBToolBar* pToolBar = FindToolBar(nID);
	if (!pToolBar) return FALSE;
	int nIndex = pToolBar->FindButton(nID);
	pToolBar->SetButtonInfo(nID, nStyle, sImageNameSpace, lpszText);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBTabbedToolbar::ShowInDialog(CWnd* pParentWnd)
{
	if (pParentWnd)
		this->SetParent(pParentWnd);

	this->SetBarStyle(this->GetBarStyle() & ~(CBRS_GRIPPER | CBRS_SIZE_DYNAMIC));

	__super::EnableDocking(CBRS_ALIGN_ANY);

	// show toolbar in tabber
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			pToolBar->SetParent(this);
			pToolBar->SetOwner(this);
			pToolBar->SetRouteCommandsViaFrame(FALSE);
			pToolBar->RedrawWindow();
			pToolBar->AdjustSizeImmediate();
		}
	}

	this->RecalcLayout();
	this->AdjustSizeImmediate();

	// Toolbar tabbed resize
	CWnd* parent = this->GetParent();
	if (parent)
	{
		CRect windowRect;
		parent->GetWindowRect(windowRect);

		CRect rectTab;
		m_pTabWnd->GetWindowRect(rectTab);

		CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
		pTabsWnd->SetDrawFrame(FALSE);
		pTabsWnd->SetTabBorderSize(0);
		pTabsWnd->SetWindowPos(NULL, 0 /*rectTab.left*/, 0 /*rectTab.top*/, windowRect.Width(), rectTab.Height(), SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOZORDER);
		this->AdjustSizeImmediate();
		this->RedrawWindow();
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CTBTabbedToolbar::OnFillBackground(CDC* pDC)
{
	CRect rcClient;
	GetClientRect(&rcClient);
	pDC->FillRect(rcClient, &CBrush(AfxGetThemeManager()->GetBackgroundColor()));
	this->m_pTabWnd->SetTabBkColor(this->m_pTabWnd->GetActiveTab(), AfxGetThemeManager()->GetBackgroundColor());
}

//-------------------------------------------------------------------------------------
void CTBTabbedToolbar::AttachOSLInfo(CInfoOSL* pParent)
{
	CString strTitle = _T("");
	GetWindowTextW(strTitle);

	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::TOOLBAR, strTitle, pParent->m_Namespace);

	GetInfoOSL()->m_pParent = pParent;
	GetInfoOSL()->SetType(OSLType_Toolbar);
	GetInfoOSL()->m_Namespace = aNs;

	if (!aNs.IsEmpty() && aNs.IsValid())
		AfxGetSecurityInterface()->GetObjectGrant(GetInfoOSL());

	// Accessibility - Set a property used to uniquely identify an object by Ranorex Spy
	// WindowsText is by default associated to AccessibleName if no otherwise specified
	CBCGPDockBar* pDockBar = GetParentDockBar();
	if (pDockBar)
	{
		CString s = aNs.ToString();
		// aNS do not contain the last object name, use those of parent (i.e.: document name)
		CString sNamespace = cwsprintf(_T("{0-%s}{1-%s}DockBar"), pParent->m_Namespace.GetObjectName(), aNs.GetTypeString());
		pDockBar->SetWindowText(sNamespace);
	}
}

// Move the ToolBar TOOLBAR_NAMETOOLS to last tab
//----------------------------------------------------------------------------
void CTBTabbedToolbar::MoveToolbarToolsToLast()
{
	INT nPos = FindToolBarPos(TOOLBAR_NAMETOOLS);
	if (nPos < 0) return;
	CBCGPTabWnd* pTabWnd = GetUnderlinedWindow();
	ASSERT(pTabWnd);
	pTabWnd->MoveTab(nPos, pTabWnd->GetTabsNum() - 1);
}

//----------------------------------------------------------------------------
void CTBTabbedToolbar::SetStartIDCollapsed(UINT nID)
{
	m_nIDOverlapButtonStart = nID;
}

//----------------------------------------------------------------------------
UINT CTBTabbedToolbar::GetMaxIDCollapsed()
{
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd) return FALSE;
	return m_nIDOverlapButtonStart + pTabsWnd->GetTabsNum() - 1;
}

//----------------------------------------------------------------------------
void CTBTabbedToolbar::AddTabSwitch(CTBToolBar* pBar)
{
	if (m_bShowToolBarTab) return;

	if (AfxGetThemeManager()->IsToolbarInfinity())
		pBar->AddDropdown(m_iIdSwitch, _T(""), TBIcon(szIconTabSwitch, TOOLBAR), _TB("Switch"));
	else
		pBar->AddDropdown(m_iIdSwitch, _T(""), TBIcon(szIconTabSwitch, TOOLBAR), _TB("Switch"), NULL, 0);

	pBar->SetTextToolTip(m_iIdSwitch, _TB("Switch\nSwitch ToolBar"));
	pBar->EnableAlwaysDropDown(m_iIdSwitch, m_bSwitchEnableAlwaysDropDown);
}

//----------------------------------------------------------------------------
void  CTBTabbedToolbar::SetIDSwitch(UINT iIdSwitch, UINT iIdSwitchMenuStart)
{
	m_iIdSwitch = iIdSwitch;
	m_iIdSwitchMenuStart = iIdSwitchMenuStart;
}

//----------------------------------------------------------------------------
void CTBTabbedToolbar::SetSwitchEnableAlwaysDropDown(BOOL b)
{
	if (m_bShowToolBarTab)
		return;
	if (m_bSwitchEnableAlwaysDropDown == b)
		return;

	m_bSwitchEnableAlwaysDropDown = b;

	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd)
		return;

	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			pToolBar->EnableAlwaysDropDown(m_iIdSwitch, m_bSwitchEnableAlwaysDropDown);
		}
	}
}

// Populate internal DropDown MenuButton (Collapsed)
//----------------------------------------------------------------------------
BOOL CTBTabbedToolbar::OnPopulatedDropDown(UINT nIdCommand)
{
	CTBToolBar* pToolBarActive = dynamic_cast<CTBToolBar*> (m_pTabWnd->GetTabWnd(GetActiveTab()));

	if (pToolBarActive)
		pToolBarActive->SetLastDropDown(nIdCommand);

	if (nIdCommand == m_iIdSwitch)
	{
		CTBToolBarMenu menu;
		CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
		if (pToolBarActive && pTabsWnd)
		{
			menu.CreateMenu();
			int nTabCount = pTabsWnd->GetTabsNum();

			for (int i = 0; i < nTabCount; i++)
			{
				CWnd* m_pTab = m_pTabWnd->GetTabWnd(i);
				CTBToolBar* ToolBarTab = dynamic_cast<CTBToolBar*> (m_pTab);
				if (!ToolBarTab->IsVisible()) {
					continue;
				}
				CString sTitle = ToolBarTab->GetTitle();
				UINT nFlag = MF_STRING;
				if (i == GetActiveTab())
					nFlag |= MF_CHECKED;
				menu.AppendMenu(nFlag, m_iIdSwitchMenuStart + i, sTitle);
			}
			pToolBarActive->UpdateDropdownMenu(nIdCommand, &menu);
		}
		return TRUE;
	}
	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CTBTabbedToolbar::AddTab(CTBToolBar* pBar, BOOL bVisible /*TRUE*/, BOOL bSetActive /*TRUE*/, BOOL bDetachable /*TRUE*/)
{
	pBar->SetParentTabbedToolbar(this);

	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	int nTabCount = pTabsWnd->GetTabsNum();

	if (nTabCount == 1)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(0);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		AddTabSwitch(pToolBar);
		// Adding TabSwitch is active
		AddTabSwitch(pBar);
	}
	else if (nTabCount > 1)
	{
		// Adding TabSwitch is active
		AddTabSwitch(pBar);
	}

	if (!__super::AddTab(pBar, bVisible, bSetActive, bDetachable))
		return FALSE;

	pBar->AttachOSLInfo(GetInfoOSL());
	pBar->SetAutoHideToolBarButton(m_bAutoHideToolBarButton);
	pBar->SetShowToolBarTab(m_bShowToolBarTab);
	pBar->EnableHighlightedColorClick(m_bHighlightedColorClickEnable);

	m_Toolbars.Add(pBar);

	// *** Assign ID DropDownButton ***
	pBar->m_nIDOverlapButton = GetMaxIDCollapsed();

	// Move ToolBar Tools to last
	MoveToolbarToolsToLast();

	// Clone Buttons 
	CloneButtons(pBar);

	return TRUE;
}

//----------------------------------------------------------------------------
void CTBTabbedToolbar::AdjustLayoutActiveTab()
{
	if (IsLayoutSuspended())
		return;
	CBCGPTabWnd* pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd) return;
	int nTabActive = pTabsWnd->GetActiveTab();
	CWnd* m_pTab = m_pTabWnd->GetTabWnd(nTabActive);
	CTBToolBar* ToolBarTab = dynamic_cast<CTBToolBar*> (m_pTab);
	if (ToolBarTab)
		ToolBarTab->AdjustLayout();
}

//----------------------------------------------------------------------------
void CTBTabbedToolbar::AdjustLayout()
{
	if (IsLayoutSuspended() || !IsTBWindowVisible(this))
		return;

	__super::AdjustLayout();
}

//----------------------------------------------------------------------------
void CTBTabbedToolbar::CloneButtons(CTBToolBar* pBar)
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	int nTabCount = pTabsWnd->GetTabsNum();
	if (nTabCount < 2) return;
	INT nSwitch = -1;

	// The toolBar 0 is the Master
	CTBToolBar* pToolBarMaster = dynamic_cast<CTBToolBar*> (m_pTabWnd->GetTabWnd(0));
	if (pToolBarMaster == NULL) return;
	int nPosRightSpaceMasterBar = pToolBarMaster->GetRightSpacePos();
	for (int i = 0; i < pToolBarMaster->GetCount(); i++)
	{
		CBCGPToolbarButton* pButton = pToolBarMaster->GetButton(i);
		// CTBToolbarButton Clone Button
		if (pButton->IsKindOf(RUNTIME_CLASS(CTBToolbarButton)))
		{
			CTBToolbarButton* pTBButton = dynamic_cast<CTBToolbarButton*> (pButton);
			ASSERT(pTBButton);
			if (!pTBButton) break;
			// This is a clonable button ?
			if (pTBButton->IsClone())
			{
				// Get Button ID
				UINT  nID = pTBButton->m_nID;
				INT nFindButton = pBar->FindButton(nID);
				if (nFindButton >= 0) {
					continue;
				}

				BOOL bGhost = pToolBarMaster->IsGhostButton(nID);
				BOOL bHidden = pToolBarMaster->IsHideButton(nID);

				// Get Button Image
				HICON hIcon = pToolBarMaster->GetLockedDisabledImages()->ExtractIcon(pTBButton->GetImage());
				// Get OSL info 
				CInfoOSLButton* pOslInfo = pToolBarMaster->GetInfoOSLButton(nID);
				// Get Button NameSpace
				CString strNameSpace = pOslInfo->m_Namespace.ToString();
				// Get Button name
				CString strName = pTBButton->m_strText;
				// Position to insert the button
				int nPasInsert = i;
				if (nPasInsert < 0) nPasInsert = 0;

				// is a Right buttons ?
				if (nPosRightSpaceMasterBar != -1 && nPasInsert > nPosRightSpaceMasterBar)
				{
					nPasInsert = -1;
					pBar->AddButtonToRight(nID, strNameSpace, hIcon, strName, nPasInsert);
					continue;
				}

				// Add button
				// dasattivo l'auto posizione, ora lo metto nel prim posto disponibile
				pBar->AddButton(nID, strNameSpace, hIcon, strName, /*nPasInsert*/ -1, 0, TRUE);
			}
		}
	}
}

//----------------------------------------------------------------------------
BOOL CTBTabbedToolbar::AddTabBefore(CTBToolBar* pBar, LPCTSTR lpszBeforeLabelToolBar, BOOL bVisible /*= TRUE*/, BOOL bSetActive /*= FALSE*/, BOOL bDetachable /*= TRUE*/)
{
	INT nPos = FindToolBarPos(lpszBeforeLabelToolBar);
	if (nPos < 0) return FALSE;
	AddTab(pBar, bVisible, bSetActive, bDetachable);
	CBCGPTabWnd* pTabWnd = GetUnderlinedWindow();
	ASSERT(pTabWnd);
	pTabWnd->MoveTab(pTabWnd->GetTabsNum() - 1, nPos);
	MoveToolbarToolsToLast();
	return TRUE;
}

//----------------------------------------------------------------------------
HICON CTBTabbedToolbar::GetIconDropdownMenu(const CBCGPToolbarMenuButton* pMenuButton)
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	ASSERT(pTabsWnd);
	if (!pTabsWnd)
		return NULL;

	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		HICON hIcon = pToolBar->GetIconDropdownMenu(pMenuButton);
		if (hIcon)
			return hIcon;
	}
	return NULL;
}

//----------------------------------------------------------------------------
LRESULT CTBTabbedToolbar::OnChangeActiveTab(WPARAM wParam, LPARAM lParam)
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	CWnd* pTab = m_pTabWnd->GetTabWnd(pTabsWnd->GetActiveTab());
	CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
	if (!pToolBar) return 0;
	pToolBar->AdjustLayout();
	return 0;
}

//----------------------------------------------------------------------------
LRESULT CTBTabbedToolbar::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	CString strId = (LPCTSTR)lParam;
	CTabbedToolbarDescription* pDesc = (CTabbedToolbarDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CTabbedToolbarDescription), strId);
	pDesc->UpdateAttributes(this);

	// get selected/currently active tab index
	int iActiveTab = GetActiveTab();
	pDesc->SetActiveTabIndex(iActiveTab);


	// get toolbars description
	int nCount = m_Toolbars.GetCount();
	//pDesc->AddChildWindows(this);
	// iterate on toolbars.
	for (int i = 0; i < nCount; i++)
	{
		CTBToolBar* pCurrToolbar = GetToolBar(i);
		//// CWndObjDescription* pCurrToolbarDescription = CTBTabbedToolbar::GetControlStructure(&pDesc->m_Children, pCurrToolbar, this);		
		//// GetControlStructure(CWndObjDescriptionContainer* pContainer, CTBToolBar* pItem,/* CBaseTabDialog* pDialog,*/ CTBTabbedToolbar* pParentTabManager)
		//CString strId = cwsprintf(_T("%d_%d"), this->m_hWnd, pCurrToolbar->m_hWnd);
		//pCurrToolbar->SendMessage(UM_GET_CONTROL_DESCRIPTION,(WPARAM)&(pDesc->m_Children), (LPARAM) (LPCTSTR) strId);
		// CWndObjDescription* pCurrToolbarDescription = CTBTabbedToolbar::GetControlStructure(&pDesc->m_Children, pCurrToolbar, this);
		if (iActiveTab == i)
		{
			// current tab is the active one.
			// ask for its children description.
			// 			pCurrToolbarDescription->AddChildWindows(this);

			// CWndObjDescription* pCurrToolbarDescription = CTBTabbedToolbar::GetControlStructure(&pDesc->m_Children, pCurrToolbar, this);		
			// GetControlStructure(CWndObjDescriptionContainer* pContainer, CTBToolBar* pItem,/* CBaseTabDialog* pDialog,*/ CTBTabbedToolbar* pParentTabManager)
			CString strId = cwsprintf(_T("%d_%d"), this->m_hWnd, pCurrToolbar->m_hWnd);
			pCurrToolbar->SendMessage(UM_GET_CONTROL_DESCRIPTION, (WPARAM)&(pDesc->m_Children), (LPARAM)(LPCTSTR)strId);

		}
		else
		{
			CWndObjDescription* pCurrToolbarDescription = CTBTabbedToolbar::GetControlStructure(&pDesc->m_Children, pCurrToolbar, this);
		}
	}

	/*
	if (nCount == 0 && m_pActiveDlg)
	{
		CTabDialog::GetControlStructure(&pDesc->m_Children, m_pActiveDlg->GetDlgInfoItem(), m_pActiveDlg, this);
	}*/
	return (LRESULT)pDesc;

	/*CTBTabbedToolbar::GetControlStructure(NULL, NULL, NULL);
	return 0;*/
}

//----------------------------------------------------------------------------
LRESULT CTBTabbedToolbar::OnSetActiveTab(WPARAM wParam, LPARAM lParam) {
	SetActiveTab((UINT)wParam);
	return 0;
}

//-----------------------------------------------------------------------------
CWndObjDescription* CTBTabbedToolbar::GetControlStructure(CWndObjDescriptionContainer* pContainer, CTBToolBar* pItem,/* CBaseTabDialog* pDialog,*/ CTBTabbedToolbar* pParentTabManager)
{
	CString strId = cwsprintf(_T("%d_%d"), pParentTabManager->m_hWnd, pItem->m_hWnd);
	CToolbarDescription *pToolbarDesc = (CToolbarDescription*)pContainer->GetWindowDescription(pItem, RUNTIME_CLASS(CToolbarDescription), strId);

	CString strTitle = pItem->GetName();
	if (pToolbarDesc->m_strText != strTitle)
	{
		pToolbarDesc->m_strText = strTitle;
		pToolbarDesc->SetUpdated(&pToolbarDesc->m_strText);
	}
	pToolbarDesc->SetID(strId);
	return pToolbarDesc;
}

//----------------------------------------------------------------------------
int CTBTabbedToolbar::CalcMinimumWidth(BOOL bHidden /*= FALSE*/)
{
	int iWidthMax = 0;
	int iWidth = 0;
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		//@@@TODO PORTING -- per dimensionare la toolbar in modo adeguato, salta la aux dove vengono provvisoriamente
		// butatte tutte le vecchie icone
		if (pToolBar->GetName() == _T("Aux"))
			continue;
		iWidth = pToolBar->CalcMinimumWidth(bHidden);
		iWidthMax = max(iWidthMax, iWidth);
	}
	return iWidthMax;
}

//----------------------------------------------------------------------------
void CTBTabbedToolbar::OnLButtonDblClk(UINT nFlags, CPoint point)
{
	// Don't permit dock/undock when user double clicks on item!
	return;
}

//----------------------------------------------------------------------------
void CTBTabbedToolbar::GetRemovedListID(CList<int, int>* pList)
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	if (!pTabsWnd) return;

	int nTabCount = pTabsWnd->GetTabsNum();
	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar && pToolBar->GetRemovedListID())
		{
			for (POSITION pos = pToolBar->GetRemovedListID()->GetHeadPosition(); pos != NULL;)
			{
				int nID = pToolBar->GetRemovedListID()->GetNext(pos);
				if (nID > 0)
					pList->AddTail(nID);
			}
		}
	}
}

//----------------------------------------------------------------------------
INT CTBTabbedToolbar::CalcMinToolBar()
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	int nTabCount = pTabsWnd->GetTabsNum();

	CClientDC dc(this);
	CFont* pOldFont = SelectDefaultFont(&dc);
	ASSERT(pOldFont != NULL);
	INT minToolBar = 0;

	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			INT nMin = pToolBar->CalcMinToolBar();
			minToolBar = max(minToolBar, nMin);
		}
	}
	return minToolBar;
}

//-----------------------------------------------------------------------------
HRESULT CTBTabbedToolbar::get_accName(VARIANT varChild, BSTR *pszName)
{
	// GetInfoOSL()->m_Namespace do not contain the last object name, use those of parent (i.e.: document name)
	CString sNamespace = cwsprintf(_T("{0-%s}{1-%s}"), GetInfoOSL()->m_pParent->m_Namespace.GetObjectName(), GetInfoOSL()->m_Namespace.GetTypeString());
	*pszName = ::SysAllocString(sNamespace);

	return S_OK;
}

//-----------------------------------------------------------------------------
void CTBTabbedToolbar::SetSuspendUpdateCmdUI(BOOL bSuspend/* = TRUE*/)
{
	CBCGPTabWnd*	pTabsWnd = this->GetUnderlinedWindow();
	int nTabCount = pTabsWnd->GetTabsNum();

	for (int i = 0; i < nTabCount; i++)
	{
		CWnd* pTab = m_pTabWnd->GetTabWnd(i);
		CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pTab);
		if (pToolBar)
		{
			pToolBar->SetSuspendUpdateCmdUI(bSuspend);
		}
	}
}

/////////////////////////////////////////////////////////////////////////////
//					CIconList
/////////////////////////////////////////////////////////////////////////////
CIconList::CIconList()
{
	this->hIco = NULL;
	this->iID = 0;
	this->mText = _T("");
}

//-------------------------------------------------------------------------------------
CIconList::CIconList(HICON hico, UINT nID, CString mText)
{
	this->iID = nID;
	this->hIco = hico;
	this->mText = mText;
}

//-------------------------------------------------------------------------------------
CIconList::~CIconList()
{
	this->hIco = NULL;
}


