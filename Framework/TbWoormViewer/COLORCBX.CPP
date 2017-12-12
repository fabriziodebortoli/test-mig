
#include "stdafx.h"

#include <float.h>
#include <math.h>
#include <ctype.h>

#include <TbGeneric\LocalizableObjs.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\reswalk.h>
#include <TbGenlib\baseapp.h>

#include "colorcbx.h"
#include "colorcbx.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define ITEM_OFFSET			2
#define ITEM_RECT_WIDTH		20

#define MAX_NUM_COLORS		16

const CString ColorDescription(int nColor)
{
	switch(nColor)
	{
		case 0x000000:	return _TB("Black");
		case 0x808080:	return _TB("Grey");
		case 0xC0C0C0:	return _TB("Light gray");
		case 0x800000:	return _TB("Navy");
		case 0xFF0000:	return _TB("Blue");
		case 0x008000:	return _TB("Dark green");
		case 0x00FF00:	return _TB("Green");
		case 0x008080:	return _TB("Dark yellow");
		case 0x00FFFF:	return _TB("Yellow");
		case 0x000080:	return _TB("Dark red");
		case 0x0000FF:	return _TB("Red");
		case 0x808000:	return _TB("Dark turquoise");
		case 0xFFFF00:	return _TB("Turquoise");
		case 0x800080:	return _TB("Purple");
		case 0xFF00FF:	return _TB("Magenta");
		case 0xFFFFFF:	return _TB("White");
	}
	
	ASSERT(FALSE);
	return _T("");

}

static const COLORREF clrColorRef [MAX_NUM_COLORS] = 
						{
							0x000000,	// _TB("Black")
							0x808080,	// _TB("Grey")
							0xC0C0C0,	// _TB("Light gray")
							0x800000,	// _TB("Navy")
							0xFF0000,	// _TB("Blue")
							0x008000,	// _TB("Dark green")
							0x00FF00,	// _TB("Green")
							0x008080,	// _TB("Dark yellow")
							0x00FFFF,	// _TB("Yellow")
							0x000080,	// _TB("Dark red")
							0x0000FF,	// _TB("Red")
							0x808000,	// _TB("Dark turquoise")
							0xFFFF00,	// _TB("Turquoise")
							0x800080,	// _TB("Purple")
							0xFF00FF,	// _TB("Magenta")
							0xFFFFFF	// _TB("White")
						};

#define MAX_NUM_PENSTYLES		7
const CString PenStyleDescription(int nStyle)
{
	switch(nStyle)
	{
		case PS_NULL:		return _TB("Null");
		case PS_SOLID:		return _TB("Solid");
		case PS_DASH:		return _TB("Dash");
		case PS_DOT:		return _TB("Dot");
		case PS_DASHDOT:	return _TB("Dash-Dot");
		case PS_DASHDOTDOT:	return _TB("Dash-Dot-Dot");
		case PS_INSIDEFRAME:return _TB("Internal frame");
	}
	
	ASSERT(FALSE);
	return _T("");
};

static const int nPenStyle [MAX_NUM_PENSTYLES] = 
						{
							PS_NULL,		// _TB("Null")
							PS_SOLID,		// _TB("Solid")
							PS_DASH,		// _TB("Dash")
							PS_DOT,			// _TB("Dot")
							PS_DASHDOT,		// _TB("Dash-Dot")
							PS_DASHDOTDOT,	// _TB("Dash-Dot-Dot") 
							PS_INSIDEFRAME  // _TB("Internal frame")
						};

#define MAX_NUM_CHART_PENSTYLES		9
static const int nChartPenStyle [MAX_NUM_CHART_PENSTYLES] = 
						{
							VtPenStyleNull,
							VtPenStyleSolid,
							VtPenStyleDashed,
							VtPenStyleDotted,
							VtPenStyleDashDot,
							VtPenStyleDashDotDot,
							VtPenStyleDitted,
							VtPenStyleDashDit,
							VtPenStyleDashDitDit
						};
//=============================================================================
//			Class CFillColorCombo implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CGDIObjCombo, CBCGPComboBox)

BEGIN_MESSAGE_MAP(CGDIObjCombo, CBCGPComboBox)
	//{{AFX_MSG_MAP(CGDIObjCombo)
	ON_WM_KEYDOWN			()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
   
//-----------------------------------------------------------------------------
CGDIObjCombo::CGDIObjCombo()
	:
	CBCGPComboBox	()
{
	InitCtrl();
}

//-----------------------------------------------------------------------------
BOOL CGDIObjCombo::SubclassGDIObjCombo(UINT nID, CWnd* pParentWnd)
{
	return
		SubclassDlgItem(nID, pParentWnd)&&
		InitCtrl();
}

//-----------------------------------------------------------------------------
BOOL CGDIObjCombo::InitCtrl()
{
	return TRUE;
}


//-----------------------------------------------------------------------------
void CGDIObjCombo::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{       
	// Per simulare il CComboBox::SetExtendedUI() in quanto se la combo e` vuota,
	// se si preme la freccia Up/Down, Windows non apre la tendina se lo stile e`
	// CBS_DROPDOWNLIST
	//
	if	(
			!GetDroppedState()	&&
			(nChar == VK_UP || nChar == VK_DOWN)
		)
	{
		SendMessage(CB_SHOWDROPDOWN, (WPARAM) TRUE);
		return;
	}

	CBCGPComboBox::OnKeyDown(nChar, nRepCnt, nFlags);
}


//=============================================================================
//			Class CFillColorCombo implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CFillColorCombo, CGDIObjCombo)

BEGIN_MESSAGE_MAP(CFillColorCombo, CGDIObjCombo)
	//{{AFX_MSG_MAP(CFillColorCombo)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
   
//-----------------------------------------------------------------------------
BOOL CFillColorCombo::InitCtrl()
{
	SetCurSel(-1);

	// per aprire  la tendina su VK_DOWN
	SetExtendedUI();
	
	FillColors();
		
	return TRUE;
}

//-----------------------------------------------------------------------------
void CFillColorCombo::FillColors()
{
	SetRedraw(FALSE);

	CBCGPComboBox::ResetContent();

	for (int i = 0; i < MAX_NUM_COLORS; i++)
	{
		CString strColorDescr;
		int	nIdx;
		strColorDescr = ColorDescription  (clrColorRef[i]);
		nIdx = CBCGPComboBox::AddString(strColorDescr);
		if (nIdx >= 0)
			CBCGPComboBox::SetItemData(nIdx, (DWORD)clrColorRef[i]);
    }

	SetRedraw(TRUE);
	Invalidate(FALSE);
}
//-----------------------------------------------------------------------------
void CFillColorCombo::SetValue(COLORREF clrColor)
{
	SetCurSel(-1);
	int nIdx;
	for (nIdx = 0; nIdx < MAX_NUM_COLORS; nIdx++)
		if (clrColor == (COLORREF)GetItemData(nIdx))
		{
			SetCurSel(nIdx);
			return;
		}
}

//-----------------------------------------------------------------------------
COLORREF CFillColorCombo::GetValue()
{
	int idx = GetCurSel();
	if (idx >= 0 && idx < GetCount())
		return GetItemData(idx);
	else
		return 0L;
}

//-----------------------------------------------------------------------------
void CFillColorCombo::DrawItem(LPDRAWITEMSTRUCT lpDrawItem)
{
	CDC* 		pDC = CDC::FromHandle(lpDrawItem->hDC);
	CRect 		rectItem;
	CBrush 		brushBk;
	COLORREF	clrTextColor;
	COLORREF	clrBackColor;
	 
	if (lpDrawItem->itemState & ODS_SELECTED)   // item has been selected
			clrBackColor = ::GetSysColor(COLOR_HIGHLIGHT);
	else
		clrBackColor = ::GetSysColor(COLOR_WINDOW);
	pDC->SetBkColor(clrBackColor);
	pDC->SetBkMode(OPAQUE);

	// se il control e' disabilitato occorre utilizzare il relativo colore di sistema
	if (IsWindowEnabled())
		clrTextColor = pDC->GetTextColor();
	else
		clrTextColor = ::GetSysColor(COLOR_GRAYTEXT);
	if (clrTextColor == clrBackColor)
		clrTextColor = RGB(128, 128, 128);
	pDC->SetTextColor(clrTextColor);

	rectItem.CopyRect(&(lpDrawItem->rcItem));

	brushBk.CreateSolidBrush(clrBackColor);
	pDC->FillRect(&rectItem, &brushBk);
	brushBk.DeleteObject();
	

	CRect rectText;		
	CString 	strItemText;
	
	if (lpDrawItem->itemID != CB_ERR)
	{
		GetLBText(lpDrawItem->itemID, strItemText);
		rectText = rectItem;
		rectText.left += 2 * ITEM_OFFSET + ITEM_RECT_WIDTH;		
		pDC->DrawText (strItemText, strItemText.GetLength(), rectText, DT_EXPANDTABS | DT_NOPREFIX | DT_LEFT | DT_VCENTER);

		rectItem.left += ITEM_OFFSET;
		rectItem.top += ITEM_OFFSET;
		rectItem.right = rectItem.left + ITEM_RECT_WIDTH;
		rectItem.bottom -= ITEM_OFFSET;
		brushBk.CreateSolidBrush(lpDrawItem->itemData);
		CBrush* pOldBrush 	= pDC->SelectObject(&brushBk);
		CPen	pen(PS_SOLID,1,RGB(0,0,0));
		CPen* 	pOldPen		= pDC->SelectObject(&pen);
		pDC->Rectangle(&rectItem);
		pDC->SelectObject(pOldBrush);
		pDC->SelectObject(pOldPen);
		brushBk.DeleteObject();
		pen.DeleteObject();
    }
	return;
}

//=============================================================================
//			Class CPenStyleCombo implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CPenStyleCombo, CGDIObjCombo)

BEGIN_MESSAGE_MAP(CPenStyleCombo, CGDIObjCombo)
	//{{AFX_MSG_MAP(CPenStyleCombo)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
   
//-----------------------------------------------------------------------------
BOOL CPenStyleCombo::InitCtrl()
{
	SetCurSel(-1);
	m_nPenWidth = 1;

	// per aprire  la tendina su VK_DOWN
	SetExtendedUI();
	
	FillPenStyles();
		
	return TRUE;
}

//-----------------------------------------------------------------------------
void CPenStyleCombo::FillPenStyles()
{
	SetRedraw(FALSE);

	CBCGPComboBox::ResetContent();

	for (int i = 0; i < MAX_NUM_PENSTYLES; i++)
	{
		CString strPenStyleDescr;
		int	nIdx;
		strPenStyleDescr = PenStyleDescription(nPenStyle[i]);
		nIdx = CBCGPComboBox::AddString(strPenStyleDescr);
		if (nIdx >= 0)
			CBCGPComboBox::SetItemData(nIdx, (DWORD)nPenStyle[i]);
    }

	SetRedraw(TRUE);
	Invalidate(FALSE);
}

//---CChartPenStyleCombo------------------------------------------------------------
void CPenStyleCombo::SetValue(int nPenStyle)
{
	SetCurSel(-1);
	int nIdx;
	for (nIdx = 0; nIdx < MAX_NUM_PENSTYLES; nIdx++)
		if (nPenStyle == (int)GetItemData(nIdx))
		{
			SetCurSel(nIdx);
			return;
		}
}

//-----------------------------------------------------------------------------
int CPenStyleCombo::GetValue()
{
	int idx = GetCurSel();
	if (idx >= 0 && idx < GetCount())
		return (int)GetItemData(idx);
	else
		return PS_NULL;
}

//-----------------------------------------------------------------------------
void CPenStyleCombo::DrawItem(LPDRAWITEMSTRUCT lpDrawItem)
{
	CDC* 		pDC = CDC::FromHandle(lpDrawItem->hDC);
	CRect 		rectItem;
	CBrush 		brushBk;
	COLORREF	clrTextColor;
	COLORREF	clrBackColor;
	 
	if (lpDrawItem->itemState & ODS_SELECTED)   // item has been selected
			clrBackColor = ::GetSysColor(COLOR_HIGHLIGHT);
	else
		clrBackColor = ::GetSysColor(COLOR_WINDOW);
	pDC->SetBkColor(clrBackColor);
	pDC->SetBkMode(OPAQUE);

	// se il control e' disabilitato occorre utilizzare il relativo colore di sistema
	if (IsWindowEnabled())
		clrTextColor = pDC->GetTextColor();
	else
		clrTextColor = ::GetSysColor(COLOR_GRAYTEXT);
	if (clrTextColor == clrBackColor)
		clrTextColor = RGB(128, 128, 128);
	pDC->SetTextColor(clrTextColor);

	rectItem.CopyRect(&(lpDrawItem->rcItem));

	brushBk.CreateSolidBrush(clrBackColor);
	pDC->FillRect(&rectItem, &brushBk);
	brushBk.DeleteObject();
	

	CRect rectText;		
	CString 	strItemText;
	
	if (lpDrawItem->itemID != CB_ERR)
	{
		GetLBText(lpDrawItem->itemID, strItemText);
		rectText = rectItem;
		rectText.left += 2 * ITEM_OFFSET + 3*ITEM_RECT_WIDTH;		
		pDC->DrawText (strItemText, strItemText.GetLength(), rectText, DT_EXPANDTABS | DT_NOPREFIX | DT_LEFT | DT_VCENTER);
	
		rectItem.left += ITEM_OFFSET;
		rectItem.right = rectItem.left + 3*ITEM_RECT_WIDTH;
		CPen	pen((int)lpDrawItem->itemData, m_nPenWidth, RGB(0,0,0));
		CPen* 	pOldPen		= pDC->SelectObject(&pen);
		pDC->MoveTo(rectItem.left, (rectItem.top + rectItem.bottom)/2);
		pDC->LineTo(rectItem.right, (rectItem.top + rectItem.bottom)/2);
		pDC->SelectObject(pOldPen);
		pen.DeleteObject();
    }
	return;
}

//=============================================================================
//			Class CChartPenStyleCombo implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CChartPenStyleCombo, CGDIObjCombo)

BEGIN_MESSAGE_MAP(CChartPenStyleCombo, CGDIObjCombo)
	//{{AFX_MSG_MAP(CChartPenStyleCombo)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
   
//-----------------------------------------------------------------------------
CChartPenStyleCombo::CChartPenStyleCombo()
	:
	CGDIObjCombo		(),
	m_pbmpPenStylesList	(NULL)
{
}

//-----------------------------------------------------------------------------
CChartPenStyleCombo::~CChartPenStyleCombo()
{
	if (m_pbmpPenStylesList) 
	{
		m_pbmpPenStylesList->DeleteObject();
		delete m_pbmpPenStylesList;
	}
}

//-----------------------------------------------------------------------------
BOOL CChartPenStyleCombo::InitCtrl()
{
	SetCurSel(-1);
    
    m_pbmpPenStylesList = new CWalkBitmap;
    if (!(m_pbmpPenStylesList && m_pbmpPenStylesList->LoadBitmap(IDB_CHART_PENSTYLES)))
	{
		delete m_pbmpPenStylesList;
		m_pbmpPenStylesList = NULL;
	}
	// per aprire  la tendina su VK_DOWN
	SetExtendedUI();
	
	FillPenStyles();
		
	return TRUE;
}

//-----------------------------------------------------------------------------
void CChartPenStyleCombo::FillPenStyles()
{
	SetRedraw(FALSE);

	CBCGPComboBox::ResetContent();

	for (int i = 0; i < MAX_NUM_CHART_PENSTYLES; i++)
	{
		CString strPenStyleDescr;
		int	nIdx;
		nIdx = CBCGPComboBox::AddString(cwsprintf(_T("%d"),nChartPenStyle[i]));
		if (nIdx >= 0)
			CBCGPComboBox::SetItemData(nIdx, (DWORD)nChartPenStyle[i]);
    }

	SetRedraw(TRUE);
	Invalidate(FALSE);
}

//-----------------------------------------------------------------------------
void CChartPenStyleCombo::SetValue(short nPenStyle)
{
	SetCurSel(-1);
	int nIdx;
	for (nIdx = 0; nIdx < MAX_NUM_CHART_PENSTYLES; nIdx++)
		if (nPenStyle == (short)GetItemData(nIdx))
		{
			SetCurSel(nIdx);
			return;
		}
}

//-----------------------------------------------------------------------------
short CChartPenStyleCombo::GetValue()
{
	int idx = GetCurSel();
	if (idx >= 0 && idx < GetCount())
		return (short)GetItemData(idx);
	else
		return (short)VtPenStyleNull;
}

//-----------------------------------------------------------------------------
void CChartPenStyleCombo::DrawItem(LPDRAWITEMSTRUCT lpDrawItem)
{
	CDC* 		pDC = CDC::FromHandle(lpDrawItem->hDC);
	CRect 		rectItem;
	COLORREF	clrBackColor;
	CBrush 		brushBk;
	
 
	if (lpDrawItem->itemState & ODS_SELECTED)   // item has been selected
			clrBackColor = ::GetSysColor(COLOR_HIGHLIGHT);
	else
		clrBackColor = ::GetSysColor(COLOR_WINDOW);
	pDC->SetBkColor(clrBackColor);
	pDC->SetBkMode(OPAQUE);

	rectItem.CopyRect(&(lpDrawItem->rcItem));

	brushBk.CreateSolidBrush(clrBackColor);
	pDC->FillRect(&rectItem, &brushBk);
	brushBk.DeleteObject();
	
	
	if (m_pbmpPenStylesList)
	{
		CDC	dcMemory; 
		dcMemory.CreateCompatibleDC (pDC);
		CBitmap* 	pOldMemoryBmp	= dcMemory.SelectObject (m_pbmpPenStylesList);
		BITMAP		bmpInfo;
		m_pbmpPenStylesList->GetObject(sizeof(BITMAP), &bmpInfo);
		int			nItemHeight = bmpInfo.bmHeight/MAX_NUM_CHART_PENSTYLES;
	
		if (lpDrawItem->itemID != CB_ERR)
			pDC->BitBlt
					(
						rectItem.left + (rectItem.Width() - bmpInfo.bmWidth)/2,
						rectItem.top + (rectItem.Height() - nItemHeight)/2,
						rectItem.Width(),
						nItemHeight, 
						&dcMemory,
						0, 
						lpDrawItem->itemID * nItemHeight, 
						(lpDrawItem->itemState & ODS_SELECTED)? MERGEPAINT : MERGECOPY
					);
		dcMemory.SelectObject	(pOldMemoryBmp);
		dcMemory.DeleteDC		();
    }
	return;
}
//-----------------------------------------------------------------------------
void CChartPenStyleCombo::MeasureItem(LPMEASUREITEMSTRUCT lpMeasureItem)
{
	if (m_pbmpPenStylesList)
	{
		BITMAP		bmpInfo;
		m_pbmpPenStylesList->GetObject(sizeof(BITMAP), &bmpInfo);
	
		lpMeasureItem->itemHeight = bmpInfo.bmHeight/MAX_NUM_CHART_PENSTYLES;
	}
	return;
}

