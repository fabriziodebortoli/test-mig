#include "stdafx.h"
#include <TbGeneric\JsonFormEngine.h>
#include "HyperLink.h"
#include "BaseTileManager.h"
#include "TBCommandInterface.h"
#include "Parsbtn.h"
#include "PARSEDT.H"
#include "TileDialogPart.h"
#include "TBLinearGauge.h"
#include "PARSOBJ.H"

#include "BaseTileDialog.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

EnumTileDescriptionAssociations CWndTileDescription::singletonEnumTileDescription;

/*
Ratios between dialog units (RC files) and pixels for various fonts and font sizes
Obtained via MapDialogRect API at 96 DPI

dialog base units
Points    width	height
Segoe UI	 9		1.75	1.88
Segoe UI	10		1.75	2.13
Segoe UI	11		2		2.5

Verdana		 9		2		1.75
Verdana		10		2		2
Verdana		11		2.25	2.25

Arial		 9		1.75	1.88
Arial		10		1.75	2
Arial		11		2		2.13

Calibri		 9		1.5		1.75
Calibri		10		1.75	1.88
Calibri		11		2		2.25

Example 1
---------
I have a control that is 185 dialog units wide, on a dialog which uses Segoe UI 10 font
On the screen such control will appear as 185 * 1.75 = 323.75 -> 324 pixels wide

Example 2
---------
I want that a control appears 200 pixels high.
If my dialog uses Segoe UI 10 font I must draw it 200 / 2.13 = 93,89 -> 94 dialog units in the RC

*/


//==============================================================================
IMPLEMENT_DYNCREATE(CWndTileDescription, CWndPanelDescription)
REGISTER_WND_OBJ_CLASS(CWndTileDescription, Tile)

//-----------------------------------------------------------------------------
void CWndTileDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL3(m_bIsCollapsible, szJsonCollapsible);
	SERIALIZE_BOOL3(m_bIsPinnable, szJsonPinnable);
	SERIALIZE_BOOL(m_bIsCollapsed, szJsonCollapsed, false);
	SERIALIZE_BOOL(m_bIsPinned, szJsonPinned, true);
	SERIALIZE_BOOL(m_bWrapTileParts, szJsonWrapTileParts, true);
	SERIALIZE_BOOL(m_bHasStaticArea, szJsonHasStaticArea, false);
	SERIALIZE_BOOL3(m_bHasTitle, szJsonHasTitle);
	SERIALIZE_ENUM(m_Size, szJsonSize, TILE_STANDARD);
	SERIALIZE_ENUM(m_Style, szJsonTileStyle, TDS_NONE);
	SERIALIZE_BOOL(m_bResetValuesAfterUnpin, szJsonResetValuesAfterUnpin, false);
	SERIALIZE_INT(m_nFlex, szJsonFlex, -1);
	SERIALIZE_INT(m_nCol2Margin, szJsonCol2Margin, NULL_COORD);
	SERIALIZE_INT(m_nMinWidth, szJsonMinWidth, NULL_COORD);
	SERIALIZE_INT(m_nMaxWidth, szJsonMaxWidth, NULL_COORD);
	SERIALIZE_INT(m_nMinHeight, szJsonMinHeight, NULL_COORD);

	__super::SerializeJson(strJson);

}
//-----------------------------------------------------------------------------
void CWndTileDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	PARSE_BOOL3(m_bIsCollapsible, szJsonCollapsible);
	PARSE_BOOL3(m_bIsPinnable, szJsonPinnable);
	PARSE_BOOL(m_bIsCollapsed, szJsonCollapsed);
	PARSE_BOOL(m_bIsPinned, szJsonPinned);
	PARSE_BOOL(m_bWrapTileParts, szJsonWrapTileParts);
	PARSE_BOOL(m_bHasStaticArea, szJsonHasStaticArea);
	PARSE_BOOL3(m_bHasTitle, szJsonHasTitle);
	PARSE_ENUM(m_Size, szJsonSize, TileDialogSize);
	PARSE_ENUM(m_Style, szJsonTileStyle, TileDialogStyle);
	PARSE_BOOL(m_bResetValuesAfterUnpin, szJsonResetValuesAfterUnpin);
	PARSE_INT(m_nFlex, szJsonFlex);
	PARSE_INT(m_nCol2Margin, szJsonCol2Margin);

	PARSE_INT(m_nMinWidth, szJsonMinWidth);
	PARSE_INT(m_nMaxWidth, szJsonMaxWidth);
	PARSE_INT(m_nMinHeight, szJsonMinHeight);

}

//-----------------------------------------------------------------------------
void CWndTileDescription::UpdateWindowText(CWnd *pWnd)
{
	//do nothing
}

//-----------------------------------------------------------------------------
void CWndTileDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	CWndTileDescription* pTileDesc = dynamic_cast<CWndTileDescription*>(pDesc);
	if (pTileDesc)
	{
		m_bIsCollapsible = pTileDesc->m_bIsCollapsible;
		m_bIsPinnable = pTileDesc->m_bIsPinnable;
		m_bIsCollapsed = pTileDesc->m_bIsCollapsed;
		m_bIsPinned = pTileDesc->m_bIsPinned;
		m_bWrapTileParts = pTileDesc->m_bWrapTileParts;
		m_bHasStaticArea = pTileDesc->m_bHasStaticArea;
		m_bHasTitle = pTileDesc->m_bHasTitle;
		m_nCol2Margin = pTileDesc->m_nCol2Margin;
		m_Size = pTileDesc->m_Size;
		m_Style = pTileDesc->m_Style;
		m_nFlex = pTileDesc->m_nFlex;
		m_nMinHeight = pTileDesc->m_nMinHeight;
		m_nMinWidth = pTileDesc->m_nMinWidth;
		m_nMaxWidth = pTileDesc->m_nMaxWidth;
		m_bResetValuesAfterUnpin = pTileDesc->m_bResetValuesAfterUnpin;
	}
}

//-----------------------------------------------------------------------------
/*static*/CString CWndTileDescription::GetEnumDescription(TileDialogSize value)
{
	return singletonEnumTileDescription.m_arTileDialogSize.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void CWndTileDescription::GetEnumValue(CString description, TileDialogSize& retVal)
{
	retVal = singletonEnumTileDescription.m_arTileDialogSize.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndTileDescription::GetEnumDescription(TileDialogStyle value)
{
	return singletonEnumTileDescription.m_arTileDialogStyle.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void CWndTileDescription::GetEnumValue(CString description, TileDialogStyle& retVal)
{
	retVal = singletonEnumTileDescription.m_arTileDialogStyle.GetEnum(description);
}


//////////////////////////////////////////////////////////////////////////////
//							CBaseTileDialog								//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CBaseTileDialog, CParsedDialog)

BEGIN_MESSAGE_MAP(CBaseTileDialog, CParsedDialog)

	ON_WM_PAINT()
	ON_WM_ERASEBKGND()
	ON_WM_CTLCOLOR()

	ON_WM_WINDOWPOSCHANGED()

	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	ON_MESSAGE(UM_CTRL_FOCUSED, OnCtrlFocused)

	ON_WM_LBUTTONDOWN()

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBaseTileDialog::CBaseTileDialog(const CString& sName, int nIDD, CWnd* pParent /*=NULL*/)
	:
	CParsedDialog(nIDD, pParent, sName),
	Tile(AfxGetTileDialogStyleNormal()),
	m_nIDCStaticArea(IDC_STATIC_AREA),
	m_rectOriginal(0, 0, 0, 0),
	m_bCreationCompleted(FALSE),
	m_bLayoutInitialized(FALSE)

{
	m_bInOpenDlgCounter = FALSE;

	for (int p = 0; p < 3; p++)
		m_TilePartsFlex[p] = 1;
}

//-----------------------------------------------------------------------------
CBaseTileDialog::~CBaseTileDialog()
{
	for (int p = 0; p <= m_TileDialogParts.GetUpperBound(); p++)
		delete m_TileDialogParts[p];

	SAFE_DELETE(m_pLayoutContainer);
}

//--------------------------------------------------------------------------
//TODO il metodo è molto diverso da quello di CBaseTabDialog
BOOL CBaseTileDialog::PreProcessMessage(MSG* pMsg)
{
#ifndef _OLD_PTM

	//	Germano : la PreProcessMessage ora è attrezzata per ruotare i messaggi corettamente
	return CParsedForm::PreProcessMessage(pMsg);

#else
	// since next statement will eat frame window accelerators,
	//   we call the ParsedForm::PreProcessMessage first
	if (__super::PreProcessMessage(pMsg))
		return TRUE;

	CFrameWnd* pFrameWnd = GetParentFrame();

	if (pFrameWnd == NULL)
		return FALSE;

	if (pFrameWnd != NULL && pFrameWnd->m_bHelpMode)
		return FALSE;

	if (pFrameWnd->PreTranslateMessage(pMsg))
		return TRUE;        // eaten by frame accelerator

	return FALSE;

#endif
}

//--------------------------------------------------------------------------
void CBaseTileDialog::EndDialog(int nResult)
{
	//Do nothing
	//TileDialog must not to close
}

//--------------------------------------------------------------------------
void CBaseTileDialog::OnOK()
{
	//Do nothing
	//TileDialog doesn't act as a normal ParsedDialog
}

//--------------------------------------------------------------------------
void CBaseTileDialog::OnCancel()
{
	//Do nothing
	//TileDialog doesn't act as a normal ParsedDialog
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::OnWindowPosChanged(WINDOWPOS* lpwndpos)
{
	__super::OnWindowPosChanged(lpwndpos);

	Tile::OnTilePosChanged();
}

//-----------------------------------------------------------------------------
BOOL CBaseTileDialog::OnEraseBkgnd(CDC* pDC)
{
	CRect rclientRect;
	this->GetClientRect(rclientRect);

	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	if (pFrame && pFrame->IsLayoutSuspended())
	{
		CWnd* pCtrl = this->GetWindow(GW_CHILD);
		for (; pCtrl; pCtrl = pCtrl->GetNextWindow())
		{
			if ((pCtrl->GetStyle() & BS_GROUPBOX) == BS_GROUPBOX)
				continue;

			if (!pCtrl->IsWindowVisible())
				continue;

			CRect screen;
			pCtrl->GetWindowRect(&screen);
			this->ScreenToClient(&screen);
			pDC->ExcludeClipRect(&screen);
		}
	}

	if (!IsCollapsed())
		pDC->FillRect(&rclientRect, GetBackgroundBrush());

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::SetTileStyle(TileStyle* pStyle)
{
	// by explicitly setting a style, the tile will no more inherit the style from its container
	m_bInheritParentStyle = FALSE;

	Tile::SetTileStyle(pStyle);
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::AddStaticArea(int nIDC)
{
	CWnd* pWnd = GetDlgItem(nIDC);
	if (!pWnd)
		return;

	pWnd->ShowWindow(GetTileDesignModeParams()->FreeResize() ? SW_SHOW : SW_HIDE);

	CRect aRect;
	pWnd->GetWindowRect(aRect);
	ScreenToClient(aRect);

	m_TileDialogParts.Add(new CTileDialogPart(this, aRect));
}

//-----------------------------------------------------------------------------
BOOL CBaseTileDialog::HasStaticArea() const
{
	return GetPartSize() > 0;
}

//-----------------------------------------------------------------------------
BOOL CBaseTileDialog::HasParts() const
{
	return GetPartSize() > 1;
}

//-----------------------------------------------------------------------------
BOOL CBaseTileDialog::GetPartSize() const
{
	return m_TileDialogParts.GetSize();
}
//-----------------------------------------------------------------------------
void CBaseTileDialog::SetWrapTileParts(BOOL bSet /*= TRUE*/, int flex1 /*= 0*/, int flex2 /*= 0*/, int flex3 /*= 0*/)
{
	m_bWrapTileParts = bSet;

	m_TilePartsFlex[0] = flex1;
	m_TilePartsFlex[1] = flex2;
	m_TilePartsFlex[2] = flex3;
}

//-----------------------------------------------------------------------------
BOOL CBaseTileDialog::OnInitDialog()
{
	ASSERT(GetParent());

	if (GetParent()->IsKindOf(RUNTIME_CLASS(CBaseTileGroup)))
		m_pParentTileGroup = ((CBaseTileGroup*)GetParent());
	else if (GetParent()->IsKindOf(RUNTIME_CLASS(CTilePanelTab)))
		m_pParentTileGroup = ((CTilePanelTab*)GetParent())->GetParentTileGroup();

	ASSERT_TRACE1(m_pParentTileGroup, "A TileDialog must be contained into a TileGroup or TilePanel %s\n", CString(GetParent()->GetRuntimeClass()->m_lpszClassName));

	TileStyle* pStyle = NULL;
	if (m_bInheritParentStyle)
	{
		if (GetParent()->IsKindOf(RUNTIME_CLASS(CBaseTileGroup)))
			pStyle = ((CBaseTileGroup*)GetParent())->GetTileDialogStyle();
		else if (GetParent()->IsKindOf(RUNTIME_CLASS(CTilePanelTab)))
			pStyle = ((CTilePanelTab*)GetParent())->GetTileDialogStyle();

		// Attach to the tile the style of the parent (either TileGroup or TilePanel). 
		// Customized properties are retained
		if (pStyle)
			Tile::SetTileStyle(pStyle);

	}
	pStyle = GetTileStyle();
	if (pStyle)
	{
		// provo ad ereditare  anche la minheight di stile
		if (pStyle->GetMinHeight() != AUTO && pStyle->GetMinHeight() != m_nMinHeight)
			SetMinHeight(pStyle->GetMinHeight());
	}

	__super::OnInitDialog();

	OnAttachParents();
	BuildDataControlLinks();
	// gives the ClientDocs the chanche to addlink their own controls
	// (reimplemented in TileDialog as the method to be called is in AbstractFormDoc and not in BaseDocument)
	OnBuildDataControlLinks();
	// after this point all visual elements are defined
	m_bCreationCompleted = TRUE;

	CParsedForm::SetBackgroundColor(GetTileStyle()->GetBackgroundColor());

	SetFont(GetParent()->GetFont());
	
	CRect rectActual;
	GetWindowRect(&rectActual);
	ScreenToClient(&rectActual);
	RemoveParts();
	GenerateParts();

	if (HasParts())
	{
		if (GetDocument() && !GetDocument()->IsInStaticDesignMode())
		{
			if (!m_pLayoutContainer)
				m_pLayoutContainer = new CLayoutContainer(this, m_pTileStyle);
			// default layout type is COLUMN, and the parts will wrap. If wrap is unwanted, use an HBOX style instead
			if (!m_bWrapTileParts)
				m_pLayoutContainer->SetLayoutType(CLayoutContainer::HBOX);
		}
		for (int p = 0; p <= m_TileDialogParts.GetUpperBound(); p++)
		{
			if (p < 3)
				m_TileDialogParts[p]->SetFlex(m_TilePartsFlex[p]);
			m_TileDialogParts[p]->SetOriginalRect(rectActual, p < m_TileDialogParts.GetUpperBound() ? m_TileDialogParts[p + 1] : NULL);
		}
	}
	else if (HasStaticArea())
		m_TileDialogParts[0]->SetOriginalRect(rectActual);

	return TRUE;  // return TRUE  unless you set the focus to a control
}


//-----------------------------------------------------------------------------
void CBaseTileDialog::OnLButtonDown(UINT nFlags, CPoint point)
{
	if (point.y < m_nTitleHeight)
	{
		m_bCollapseExpandFromClick = TRUE;
		Tile::CollapseExpand();
	}
	else
	{
		__super::OnLButtonDown(nFlags, point);
	}
}

//-----------------------------------------------------------------------------
LRESULT	CBaseTileDialog::OnCtrlFocused(WPARAM wParam, LPARAM lParam)
{
	if (!IsDisplayed())
		return 0L;

	return __super::OnCtrlFocused(wParam, lParam);
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::UpdateTitleView()
{
	if (!GetTileStyle()->HasTitle())
		return;

	CRect rectTitle;
	GetWindowRect(&rectTitle);
	ScreenToClient(&rectTitle);
	rectTitle.bottom = m_nTitleHeight;
	InvalidateRect(rectTitle);
}

//-----------------------------------------------------------------------------
CRect CBaseTileDialog::GetStaticAreaRect(int nIdx /*= 0*/)
{
	ASSERT(nIdx >= 0 && nIdx <= m_TileDialogParts.GetUpperBound());
	if (nIdx >= 0 && nIdx <= m_TileDialogParts.GetUpperBound())
		return m_TileDialogParts[nIdx]->GetStaticAreaRect();
	else
		return CRect(0, 0, 0, 0);
}

//-----------------------------------------------------------------------------
BOOL CBaseTileDialog::IsInStaticArea(CWnd* pWnd, int* pIdx /*= NULL*/)
{
	CRect rect;
	pWnd->GetWindowRect(&rect);
	ScreenToClient(&rect);

	return IsInStaticArea(rect, pIdx);
}

//-----------------------------------------------------------------------------
BOOL CBaseTileDialog::IsInStaticArea(const CRect& rectCtrl, int* pIdx /*= NULL*/)
{
	for (int r = 0; r <= m_TileDialogParts.GetUpperBound(); r++)
		if (m_TileDialogParts[r]->IsInStaticArea(rectCtrl))
		{
		if (pIdx) *pIdx = r;
		return TRUE;
		}

	if (pIdx) *pIdx = -1;
	return FALSE;
}

//-----------------------------------------------------------------------------
CBrush* CBaseTileDialog::GetStaticAreaBrush()
{
	return GetTileStyle()->GetStaticAreaBkgColorBrush();
}

//-----------------------------------------------------------------------------
COLORREF CBaseTileDialog::GetStaticAreaColor()
{
	return GetTileStyle()->GetStaticAreaBkgColor();
}

//-----------------------------------------------------------------------------
CBrush* CBaseTileDialog::GetBackgroundBrush()
{
	return GetTileStyle()->GetBackgroundColorBrush();
}

//-----------------------------------------------------------------------------
COLORREF CBaseTileDialog::GetBackgroundColor()
{
	return GetTileStyle()->GetBackgroundColor();
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::OnPaint()
{
	{ // Scope del CPaintDC
		CPaintDC dc(this);

		// colore di sfondo parte di label
		if (HasStaticArea())
		{
			CRect rectActual;
			GetClientRect(rectActual);
			int nBottom = 0;
			for (int s = 0; s <= m_TileDialogParts.GetUpperBound(); s++)
			{
				CRect staticAreaRect(m_TileDialogParts[s]->GetStaticAreaRect());
				dc.FillRect(staticAreaRect, GetStaticAreaBrush());
				nBottom = max(nBottom, staticAreaRect.bottom);
			}
			// need to prolongue the static area painting if parts are shorter then the container tile
			// @@TODO non funziona bene se > 2 parti!!!
			if (HasParts() && nBottom < rectActual.bottom)
				for (int s = 0; s <= m_TileDialogParts.GetUpperBound(); s++)
				{
					CRect rectBottom(m_TileDialogParts[s]->GetStaticAreaRect());
					rectBottom.top = rectBottom.bottom;
					rectBottom.bottom = rectActual.bottom;
					dc.FillRect(rectBottom, GetStaticAreaBrush());
				}
		}

		Tile::OnTilePaint(&dc);
	}

	__super::OnPaint();
}

//-----------------------------------------------------------------------------
BOOL CBaseTileDialog::Create(UINT nIDC, const CString& strTitle, CWnd* pParentWnd, TileDialogSize tileSize)
{
	m_TileSize = tileSize;

	// this permit to create a tile without the resource ID in the class, but assigned in the Add method (i.e.: placeholder)
	if (m_nID == 0 && nIDC != 0)
		m_nID = nIDC;

	if (!__super::Create(nIDC != 0 ? nIDC : m_nID, pParentWnd))
	{
		return FALSE;
	}

	Tile::TileCreate(strTitle);

	// set proper dimension 
	SetSize();

	//security
	Show(OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EXECUTE));

	return TRUE;
}

//-----------------------------------------------------------------------------
/*static*/ int CBaseTileDialog::GetTileWidth(TileDialogSize tileSize)
{
	int baseWidth = MulDiv(BaseWidthLU, AfxGetThemeManager()->GetBaseUnitsWidth(), 100);

	switch (tileSize)
	{
		case TILE_WIDE:		return baseWidth * 4;		// 4 MINIs
		case TILE_LARGE:	return (baseWidth * 3);		// 3 MINIs
		case TILE_STANDARD:
		case TILE_AUTOFILL:	return baseWidth * 2;		// 2 MINIs
		case TILE_MINI:		return baseWidth;
		case TILE_MICRO:	return (baseWidth) / 2;		// half a MINI
		default:
		{
			ASSERT_TRACE(FALSE, "Invalid tile size");
			return 0;
		}
	}
}

//-----------------------------------------------------------------------------
int CalculateRectArea(CRect rect)
{
	return rect.Width() * rect.Height();
}

//-----------------------------------------------------------------------------
CTileDesignModeParamsObj* CBaseTileDialog::GetTileDesignModeParams()
{
	if (GetDocument() && GetDocument()->GetManagedParameters())
	{
		CTileDesignModeParamsObj* pParam = dynamic_cast<CTileDesignModeParamsObj*>(GetDocument()->GetDesignModeManipulatorObj());
		if (pParam)
			return pParam;
	}
	
	return dynamic_cast<CTileDesignModeParamsObj*>(AfxGetThemeManager());
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::SetSize()
{
	CRect rectActual;
	GetWindowRect(&rectActual);
	ScreenToClient(&rectActual);
	SetSize(rectActual, FALSE);
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::GenerateParts()
{
	AddStaticArea(m_nIDCStaticArea);
	AddStaticArea(IDC_STATIC_AREA_2);
	//AddStaticArea(IDC_STATIC_AREA_3);

	CheckTileSize();

	// se non avevo definito parti prima devo aggiungere anche il layout
	// per gestirle perche' prima era NULL
	if (HasParts() && GetDocument() && !GetDocument()->IsInStaticDesignMode())
	{
		if (!m_pLayoutContainer)
			m_pLayoutContainer = new CLayoutContainer(this, m_pTileStyle);
	}
	for (int i = 0; i < m_TileDialogParts.GetSize(); i++)
	{
		CTileDialogPart* pPart = m_TileDialogParts[i];
		if (m_pLayoutContainer)
			m_pLayoutContainer->AddChildElement(pPart);
		pPart->SetOriginalRect(m_rectOriginal, i < m_TileDialogParts.GetUpperBound() ? m_TileDialogParts[i + 1] : NULL);
	}

}

//------------------------------------------------------------------------------------
void CBaseTileDialog::CheckTileSize()
{
	if (HasParts() && m_TileSize == TileDialogSize::TILE_STANDARD)
	{
		ASSERT(FALSE);
		m_TileSize = TileDialogSize::TILE_WIDE;
	}

}

//-----------------------------------------------------------------------------
void CBaseTileDialog::RemoveParts()
{
	for (int p = m_TileDialogParts.GetUpperBound(); p >= 0; p--)
	{
		if (m_pLayoutContainer)
			m_pLayoutContainer->RemoveChildElement(m_TileDialogParts[p]);
		delete m_TileDialogParts[p];

		m_TileDialogParts.RemoveAt(p);
	}
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::RecalcParts()
{
	RemoveParts();
	GenerateParts();

	// sti calcoli danno per scontato che sia tutto senza titolo
	// per evitare di farnel altre copie, azzero un secondo il titolo
	int nTempTitleHeight = m_nTitleHeight;
	m_nTitleHeight = 0;

	SetSize(m_rectOriginal, FALSE, TRUE);

	m_nTitleHeight = nTempTitleHeight;
	RequestRelayout();
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::InitializeLayout()
{
	if (m_bLayoutInitialized)
		return;

	RemoveParts();
	GenerateParts();
	SetSize(m_rectOriginal, TRUE);
	
	m_bLayoutInitialized = TRUE;
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::LinkControlsToParts(CRect& rectActual, int& bottomLine, int& rightmostColumn, BOOL bMoveControls /*TRUE*/)
{
	CDC* pDC = GetDC();
	CFont* pOldFont = pDC->SelectObject(GetFont());

	int anchorSize = GetTileDesignModeParams()->GetTileAnchorSize();

	CWnd* pCtrl = GetWindow(GW_CHILD);
	m_MaxStaticWidth = 0;
	while (pCtrl)
	{
		// move only non-hidden controls and hyperlinks (which are hidden by default)
		if ((::GetWindowLong(pCtrl->m_hWnd, GWL_STYLE) & WS_VISIBLE || pCtrl->IsKindOf(RUNTIME_CLASS(CHyperLink)))
			&&
			!(pCtrl->IsKindOf(RUNTIME_CLASS(CPinButton)) || pCtrl->IsKindOf(RUNTIME_CLASS(CCollapseButton)))
			)
		{
			CRect r;
			pCtrl->GetWindowRect(&r);
			ScreenToClient(&r);
			// shifts all the controls down to make room for the title bar
			if (bMoveControls)
			{
				pCtrl->MoveWindow(r.left, r.top + m_nTitleHeight, r.Width(), r.Height());

				if (pCtrl->IsKindOf(RUNTIME_CLASS(CParsedButton)))
				{
					CParsedButton* pBoolButton = (CParsedButton*)pCtrl;
					pBoolButton->RecalculatePaintInfo();
				}

			}

			CRect maxIntersectionRect;
			int nOwnerTilePartIndex = -1;
			maxIntersectionRect.SetRectEmpty();
			for (int s = 0; s <= m_TileDialogParts.GetUpperBound(); s++)
			{
				CRect intersectionRect = m_TileDialogParts[s]->Intersect(r);
				if (intersectionRect != NULL)
				{
					if (CalculateRectArea(intersectionRect) > CalculateRectArea(maxIntersectionRect))
					{
						maxIntersectionRect = intersectionRect;
						nOwnerTilePartIndex = s;
					}
				}
			}

			if (nOwnerTilePartIndex != -1)
			{
				m_TileDialogParts[nOwnerTilePartIndex]->AddControl(pCtrl, r, bMoveControls);
			}
			else if (AfxIsRemoteInterface() && m_TileDialogParts.GetCount() > 0) //TODO Silvano State Button x weblook, hanno dimensione 0,0 -> non verrebbero aggiunti
			{
				m_TileDialogParts[0]->AddControl(pCtrl, r, bMoveControls);
			}

			// calculate the maximum extent of labels in the static area
			if (IsInStaticArea(r))
			{
				CString strLabel;
				pCtrl->GetWindowTextW(strLabel);

				strLabel.Trim();
				pCtrl->SetWindowText(strLabel);

				CSize sz = pDC->GetTextExtent(strLabel);
				// because of multiline labels ("\n" inside) uses DrawText with control Style
				CRect rectMeasure(0, 0, sz.cx, 0);
				if (bMoveControls)
					pDC->DrawText(strLabel, &rectMeasure, pCtrl->GetStyle());

				//TRACE3("%s - %d ; %d\n", strLabel, sz.cx, sz1.cx);
				m_MaxStaticWidth = max(m_MaxStaticWidth, rectMeasure.Width());
			}
			
			// look for controls in the anchor area
			// if anchor size is not specified in the theme, anchor is not managed

			//anchor right most controls to the right side of TILE, but not rectActual
			CRect aTileRect;
			GetWindowRect(aTileRect);
			ScreenToClient(aTileRect);
			CParsedStatic* pParsed = NULL;
			if (pCtrl->IsKindOf(RUNTIME_CLASS(CParsedStatic)))
				pParsed = (CParsedStatic*)pCtrl;

			if (
				IsStretchableControl(pCtrl) &&  //anchor  only edit, combo and boolbutton
				anchorSize != 0 &&
				/*pParsed && pParsed->m_bRightAnchor*/
				r.right >= aTileRect.Width() - anchorSize
				)
			{
				if (bMoveControls)
					m_Anchored.Add(pCtrl->GetSafeHwnd());
			}
			else
				// keep track of the rightmost non-anchored control
				rightmostColumn = max(rightmostColumn, r.right);

			// keeps track of the lowest non-hidden control
			if (r.bottom > bottomLine)
				bottomLine = r.bottom;
		}

		pCtrl = pCtrl->GetNextWindow();
	}

	pDC->SelectObject(pOldFont);
	ReleaseDC(pDC);
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::SetSize(CRect rectActual, BOOL bMoveControls, BOOL bSetAsOriginal /*FALSE*/)
{
	TBThemeManager* pThemeManager = AfxGetThemeManager();

	int maxHeight = GetTileDesignModeParams()->GetTileMaxHeightUnit();
	int anchorSize = GetTileDesignModeParams()->GetTileAnchorSize();

	int width = GetTileWidth(m_TileSize);
	// dialog is truncated at maximum height

	int nOriginalHeight = m_nTitleHeight +
		(maxHeight < 0 ?
		rectActual.bottom :
		min(rectActual.bottom, MulDiv(maxHeight, pThemeManager->GetBaseUnitsHeight(), 100))
		);

	// in caso di cambio dimensioni customizzate il
	// vince il rettangolo di spazio maggiore 
	if (bSetAsOriginal)
	{
		SetMinWidth(ORIGINAL);
		if (rectActual.Width() > width)
			width = rectActual.Width();

		if (rectActual.Height() > nOriginalHeight)
			nOriginalHeight = m_nTitleHeight + rectActual.Height();
		SetMinHeight(ORIGINAL);
	}

	int bottomLine = 0;
	int rightmostColumn = 0;

	LinkControlsToParts(rectActual, bottomLine, rightmostColumn, bMoveControls);

	// In case of AUTO or FREE MinHeight adjusts height so that it includes the lowest non-hidden controls
	// Usually controls hidden due to localization or configuration are placed at the bottom
	if (bSetAsOriginal || m_nMinHeight == AUTO || m_nMinHeight == FREE)
	{
		int bottomPadding = GetTileStyle()->GetAspect() == TileStyle::EDGE ? 4 : 2; // some bottom margin
		if (bSetAsOriginal)
			nOriginalHeight = max(nOriginalHeight, bottomLine + m_nTitleHeight + bottomPadding);
		else
			nOriginalHeight = bottomLine + m_nTitleHeight + bottomPadding;
	}

	// save the startup rect for resizing purposes

	ASSERT(m_rectOriginal.left == 0);
	m_rectOriginal.right = width;

	if (m_rectOriginal.bottom < nOriginalHeight)		//corina
		m_rectOriginal.bottom = nOriginalHeight;		//corina

	for (int s = 0; s <= m_TileDialogParts.GetUpperBound(); s++)
		m_TileDialogParts[s]->SetSize();

	if (bSetAsOriginal)
	{
		m_nAutoMinWidth = 0;
	}
	else
	{
		//@@@TODO vale la pena di avere un parametro per la flessibilita` di resize?
		m_nAutoMinWidth = max((int)(m_rectOriginal.Width() * 0.8), rightmostColumn);
	}
	m_nAutoMaxWidth = (int)(m_rectOriginal.Width() * 1.2);

	// autofill tiles resize themselves to any size
	if (m_TileSize == TILE_AUTOFILL)
	{
		SetMinHeight(0);
		SetMinWidth(0);
		SetMaxWidth(FREE);
	}
}

//-----------------------------------------------------------------------------
BOOL CBaseTileDialog::IsStretchableControl(CWnd* pCtrl)
{
	return	(
		pCtrl->IsKindOf(RUNTIME_CLASS(CEdit))
		|| pCtrl->IsKindOf(RUNTIME_CLASS(CComboBox))
		|| pCtrl->IsKindOf(RUNTIME_CLASS(CBoolButton))
		|| pCtrl->IsKindOf(RUNTIME_CLASS(CStatic))
		|| pCtrl->IsKindOf(RUNTIME_CLASS(CTBLinearGaugeCtrl))
		|| pCtrl->IsKindOf(RUNTIME_CLASS(CTBCircularGaugeCtrl))
		|| pCtrl->IsKindOf(RUNTIME_CLASS(CChildButton))
		)
		&& !pCtrl->IsKindOf(RUNTIME_CLASS(CDateEdit));
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::ResizeStaticAreaWidth(int nNewWidth)
{
	if (!HasStaticArea())
		return;

	TBThemeManager* pThemeManager = AfxGetThemeManager();
	if (HasParts())
	{
		int delta = 0;
		for (int p = 0; p <= m_TileDialogParts.GetUpperBound(); p++)
			delta += m_TileDialogParts[p]->ResizeStaticAreaWidth(delta, nNewWidth);

		m_rectOriginal.right += delta;
		m_nAutoMinWidth += delta;
		m_nAutoMaxWidth += delta;

		return;
	}


	CWnd* pCtrl = GetWindow(GW_CHILD);
	int delta = nNewWidth - m_TileDialogParts[0]->GetStaticAreaRect().Width();

	int leftPadding = GetTileStyle()->GetAspect() == TileStyle::EDGE ? pThemeManager->GetTileStaticAreaInnerLeftPadding() + 1 : pThemeManager->GetTileStaticAreaInnerLeftPadding();
	int rightPadding = pThemeManager->GetTileStaticAreaInnerRightPadding();

	while (pCtrl)
	{
		if ((::GetWindowLong(pCtrl->m_hWnd, GWL_STYLE) & WS_VISIBLE))
		{
			CRect r;
			pCtrl->GetWindowRect(&r);
			ScreenToClient(&r);
			int nIdx;

			if (!IsInStaticArea(r, &nIdx) || nIdx != 0)
			{
				// just shifted
				pCtrl->MoveWindow(max(0, r.left + delta + rightPadding) + pThemeManager->GetTileInnerLeftPadding(), r.top, r.Width(), r.Height());
			}
			else
			{
				// shifted and resized
				if (!pCtrl->IsKindOf(RUNTIME_CLASS(CBoolButton)))
					pCtrl->MoveWindow(leftPadding, r.top, r.Width() + (r.left - leftPadding) + delta - leftPadding, r.Height());
				else
				{
					CBoolButton* pBoolButton = (CBoolButton*)pCtrl;
					pBoolButton->RecalculatePaintInfo();
					pCtrl->MoveWindow(leftPadding, r.top, r.Width() + (r.left - leftPadding) + delta - leftPadding + rightPadding, r.Height());
					pCtrl->ModifyStyle(WS_BORDER, NULL);
				}

			}
		}

		pCtrl = pCtrl->GetNextWindow();
	}
	m_TileDialogParts[0]->SetStaticAreaExtent(0, nNewWidth + rightPadding);
	m_rectOriginal.right += delta;
	m_nAutoMinWidth += delta;
	m_nAutoMaxWidth += delta;
}

//-----------------------------------------------------------------------------
int CBaseTileDialog::GetRequiredHeight(CRect& rect)
{
	if (IsCollapsed())
		return m_nTitleHeight;

	if (HasParts() && m_pLayoutContainer)
		return m_pLayoutContainer->GetRequiredHeight(rect) + m_nTitleHeight;
	else
		return m_rectOriginal.Height();
}

//-----------------------------------------------------------------------------
int CBaseTileDialog::GetRequiredWidth(CRect& rect)
{
	if (HasParts() && m_pLayoutContainer)
		return m_pLayoutContainer->GetRequiredWidth(rect);
	
	if (m_TileSize == TILE_AUTOFILL)
		return m_rectOriginal.Width();

	return GetTileWidth(m_TileSize);
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::GetAvailableRect(CRect &rectAvail)
{
	rectAvail = m_rectOriginal;
	if (IsCollapsed() && rectAvail.Height() > m_nTitleHeight)
		rectAvail.bottom = rectAvail.top + m_nTitleHeight;
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::GetUsedRect(CRect &rectUsed)
{
	GetWindowRect(rectUsed);

	if (IsCollapsed())
		rectUsed.bottom = rectUsed.top + m_nTitleHeight;

}

//-----------------------------------------------------------------------------
void CBaseTileDialog::Relayout(CRect &rectNew, HDWP hDWP/*= NULL*/)
{
	if (!m_hWnd || !IsVisible())
		return;

	if (IsLayoutSuspended())
		return;

	CRect rectActual;
	GetWindowRect(rectActual);
	GetParent()->ScreenToClient(rectActual);
	if (HasParts() && !IsCollapsed())
	{
		// let the parts do their layout and get the used ractangle to resize itself
		CRect rectParts(rectNew);
		// the parts expect a rectangle like a "client area" of the tile: 0 based and below the title
		rectParts.OffsetRect(-rectNew.left, -rectNew.top);
		rectParts.top += m_nTitleHeight;
		if (m_pLayoutContainer)
			m_pLayoutContainer->Relayout(rectParts, hDWP);
		CRect rectUsed;
		if (m_pLayoutContainer)
			m_pLayoutContainer->GetUsedRect(rectUsed);
		// transforms back the coordinates origin to the given rectangle
		rectUsed.top -= m_nTitleHeight;
		rectUsed.OffsetRect(rectNew.left, rectNew.top);
		// If the parts fit on a shorter rectangle, enlarge those used by the container tile to match the requested rect
		// In case of flex the tile needs to fill all the assigned area
		if (rectUsed.Height() < rectNew.Height())
			rectUsed.bottom = rectUsed.top + rectNew.Height();
		rectNew = rectUsed;

		if (rectActual != rectNew)
		{
			//if (this->GetParent())
			//	this->GetParent()->InvalidateRect(rectActual);

			SetWindowPos(NULL, rectNew.left, rectNew.top, rectNew.Width(), rectNew.Height(), SWP_NOZORDER | SWP_FRAMECHANGED);
			UpdateWindow();
		}

		return;
	}

	if (IsCollapsed())
		rectNew.bottom = rectNew.top + m_nTitleHeight;
	else
		if (rectNew.Height() < GetMinHeight())
			rectNew.bottom = rectNew.top + GetMinHeight();

	// collapsed tiles with parts which wrap have a minimum size divided by the number of parts
	int minWidth = HasParts() && IsCollapsed() && m_bWrapTileParts ? GetMinWidth() / m_TileDialogParts.GetSize() : GetMinWidth();
	if (rectNew.Width() < minWidth)
		rectNew.right = rectNew.left + minWidth;

	if (!IsCollapsed())
	{
		ResizeRightmostControls(rectNew.Width());
		ResizeStaticAreaHeight(rectNew.Height());
	}

	if (rectActual != rectNew)
	{
		//if (this->GetParent())
		//	this->GetParent()->InvalidateRect(rectActual);

		SetWindowPos(NULL, rectNew.left, rectNew.top, rectNew.Width(), rectNew.Height(), SWP_NOZORDER | SWP_FRAMECHANGED);
		UpdateWindow();
	}
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::ResizeRightmostControls(int nNewWidth)
{
	if (m_Anchored.GetSize() == 0 || !GetTileDesignModeParams()->IsInternalControlMovementEnabled())
		return; // no controls anchored on the right

	CRect rectActual;
	GetWindowRect(rectActual);
	ScreenToClient(rectActual);

	nNewWidth = min(max(nNewWidth, GetMinWidth()), GetMaxWidth());
	
	if (rectActual.Width() == nNewWidth)
		return; // same width, no bothering to resize

	for (int c = 0; c <= m_Anchored.GetUpperBound(); c++)
	{
		CWnd* pCtrl = CWnd::FromHandle(m_Anchored.GetAt(c));
		if (!pCtrl)
			continue;

		CRect rectCtrl;
		pCtrl->GetWindowRect(rectCtrl);
		ScreenToClient(rectCtrl);
		
		pCtrl->MoveWindow(rectCtrl.left, rectCtrl.top, nNewWidth - AfxGetThemeManager()->GetTileRightPadding() - rectCtrl.left, rectCtrl.Height());
		if (pCtrl->IsKindOf(RUNTIME_CLASS(CParsedButton)))
		{
			CParsedButton* pBoolButton = (CParsedButton*)pCtrl;
			pBoolButton->RecalculatePaintInfo();
		}
		//riposizionamento bottoni
		CParsedCtrl* pParsedCtrl = ::GetParsedCtrl(pCtrl);
		if (pParsedCtrl)
		{
			pParsedCtrl->AdjustButtonsVisualization();
		}
	}
}

//------------------------------------------------------------------------------
BOOL CBaseTileDialog::CanDoLastFlex(FlexDim  fd)
{
	if (fd == HEIGHT && IsCollapsed())
		return FALSE;

	if (IsFlexAuto())
		return TRUE;

	if (IsAutoFill())
		return TRUE;

	return IsFlex(fd);
}

//------------------------------------------------------------------------------
int CBaseTileDialog::GetFlex(FlexDim dim)
{
	if (dim == HEIGHT && IsCollapsed())
		return 0;

	if (IsFlexAuto())
	{
		if (IsAutoFill())
			return 1;
		else
			if (GetRequestedLastFlex() != AUTO)
				return GetRequestedLastFlex();
			else
				return 0;
	}

	return __super::GetFlex(dim);
}

//------------------------------------------------------------------------------
int CBaseTileDialog::GetMinWidth()
{
	switch (m_nMinWidth)
	{
	case ORIGINAL: return m_rectOriginal.Width();
	case FREE:
	case AUTO: return m_nAutoMinWidth;
	default: return m_nMinWidth;
	}
}

//------------------------------------------------------------------------------
int CBaseTileDialog::GetMinHeight(CRect& rect /*= CRect(0, 0, 0, 0)*/)
{
	switch (m_nMinHeight)
	{
	case ORIGINAL:
	case FREE:
	case AUTO:
	{
		if (IsCollapsed())
			return m_nTitleHeight;
		if (HasParts() && m_pLayoutContainer)
			return m_pLayoutContainer->GetMinHeight(rect) + m_nTitleHeight;
		else
			return m_rectOriginal.Height();
	}
	default: return m_nTitleHeight + m_nMinHeight;
	}
}

//------------------------------------------------------------------------------
int CBaseTileDialog::GetMaxWidth()
{
	switch (m_nMaxWidth)
	{
	case ORIGINAL: return m_rectOriginal.Width();
	case FREE: return INT_MAX;
	case AUTO: return m_nAutoMaxWidth;
	default: return m_nMaxWidth;
	}
}

//------------------------------------------------------------------------------
void CBaseTileDialog::ResizeStaticAreaHeight(int nNewHeight)
{
	if (!HasStaticArea() || !GetTileDesignModeParams()->IsInternalControlMovementEnabled())
		return;

	for (int s = 0; s <= m_TileDialogParts.GetUpperBound(); s++)
		m_TileDialogParts[s]->ResizeStaticAreaHeight(nNewHeight);
}

//-----------------------------------------------------------------------------
HBRUSH CBaseTileDialog::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	/*	HBRUSH lResult = DoOnCtlColor(pDC, pWnd, nCtlColor);
		if (lResult)
		return lResult;     // catched: eat it
		*/

	if ((nCtlColor == CTLCOLOR_DLG))
	{
		pDC->SetBkColor(GetTileStyle()->GetBackgroundColor());
		return (HBRUSH)(GetTileStyle()->GetBackgroundColorBrush())->GetSafeHandle();
	}

	CEdit* candidateCEdit = dynamic_cast<CEdit*>(pWnd);
	//I campi CEdit in ReadOnly mode generano un CTLCOLOR_STATIC (e non un CTLCOLOR_EDIT)
	//(fix. an in test prog. 5607 spec 20 riga 11: I campi color edit non sono colorati in browse (Sales Orders\Open Orders\Confirmation Levels))
	if ((nCtlColor == CTLCOLOR_STATIC) && HasStaticArea() && candidateCEdit == NULL)
	{
		CRect statRect;
		CRect overlapRect;
		pWnd->GetWindowRect(&statRect);
		ScreenToClient(&statRect);
		if (IsInStaticArea(statRect))
		{
			pDC->SetBkColor(GetTileStyle()->GetStaticAreaBkgColor());
			return (HBRUSH)(GetTileStyle()->GetStaticAreaBkgColorBrush())->GetSafeHandle();
		}
		else
		{
			pDC->SetBkColor(GetTileStyle()->GetBackgroundColor());
			return (HBRUSH)(GetTileStyle()->GetBackgroundColorBrush())->GetSafeHandle();
		}
	}

	return __super::OnCtlColor(pDC, pWnd, nCtlColor);
}

//------------------------------------------------------------------------------
void CBaseTileDialog::DoCollapseExpand()
{
	CBaseFormView* pView = NULL;
	BOOL bOldStateWnd = TRUE;
	
	if (m_bCollapseExpandFromClick)
	{
		CWnd* pParent = GetParent();
		while (pParent->GetParent() && !pParent->GetParent()->IsKindOf(RUNTIME_CLASS(CDockableFrame)))
			pParent = pParent->GetParent();

		if (pParent)
			pView = dynamic_cast<CBaseFormView*>(pParent);
	}

	BOOL bDoShowHide = m_bCollapseExpandFromClick && pView && !pView->IsModal();
	if (bDoShowHide)
		bOldStateWnd = pView->ShowWindow(SW_HIDE);

	// if the tile is inside a container, a relayout is needed
	if (m_pParentElement)
	{
		// if the collapse/expand action was initiated by a sibling, it will be its responsability to invoke a relayout
		if (!m_bProcessingSiblingRequest)
			RequestRelayout();
	}
	else
	{
		// standalone tiles resize themselves
		CWnd* pParentWnd = GetParent();

		if (!pParentWnd)
			return;
		MoveWindow(0, 0, m_rectOriginal.Width(), IsCollapsed() ? m_nTitleHeight : m_rectOriginal.Height());

		pParentWnd->SendMessage(UM_RECALC_CTRL_SIZE);
	}
	
	OnUpdateControls(!IsCollapsed());

	// in case the collapsed tile has a customized title, update it
	UpdateTitleView();

	if (bDoShowHide && bOldStateWnd)
		pView->ShowWindow(SW_SHOW);

	m_bCollapseExpandFromClick = FALSE;

	if (IsCollapsed())
	{
		CWnd* pWnd = GetLastFocusedCtrl();
		if (pWnd)
		{
			CWnd* pWndChild = GetWindow(GW_CHILD);
			while(pWndChild != NULL)
			{
				if (pWnd->m_hWnd == pWndChild->m_hWnd)
				{
					SetNextControlFocus(FALSE);
					return;
				}
			
				pWndChild = pWndChild->GetNextWindow();
			}
		}
	}
}

//------------------------------------------------------------------------------
void CBaseTileDialog::DoPinUnpin()
{
	m_bVisible = IsPinned();
	ShowWindow(m_bVisible ? SW_SHOW : SW_HIDE);
	RequestRelayout();
	if (IsPinned())
		m_pParentTileGroup->OnTileDialogPin(this);
	else
		m_pParentTileGroup->OnTileDialogUnpin(this);

	OnPinUnpin();
}

//------------------------------------------------------------------------------
BOOL CBaseTileDialog::IsAutoFill() const
{
	return m_TileSize == TILE_AUTOFILL;
}

//------------------------------------------------------------------------------
void CBaseTileDialog::Enable(BOOL bEnable)
{
	__super::Enable(bEnable);

	for (int i = 0; i < m_pControlLinks->GetSize(); i++)
	{
		CWnd* pWnd = m_pControlLinks->GetAt(i);
		ASSERT(pWnd);
		CParsedCtrl* pCtrl = GetParsedCtrl(pWnd);
		if (pCtrl)
		{
			pCtrl->EnableCtrl(bEnable);
		}
		else
		{
			pWnd->EnableWindow(bEnable);
		}

	}

	// Collapsed button in TileDialog always enable
	EnableWindow();
}

//-----------------------------------------------------------------------------
LRESULT CBaseTileDialog::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CWndTileDescription* pTileDesc = (CWndTileDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndTileDescription), strId);
	pTileDesc->UpdateAttributes(this);
	TileStyle* pTileStyle = GetTileStyle();
	Bool3 bIsCollapsible = B_UNDEFINED;
	if (pTileStyle)
		bIsCollapsible = pTileStyle->Collapsible() ? B_TRUE : B_FALSE;
	if (pTileDesc->m_bIsCollapsible != bIsCollapsible)
	{
		pTileDesc->m_bIsCollapsible = bIsCollapsible;
		pTileDesc->SetUpdated(&pTileDesc->m_bIsCollapsible);
	}

	Bool3 bHasTitle = B_UNDEFINED;
	if (pTileStyle)
		bHasTitle = pTileStyle->HasTitle() ? B_TRUE : B_FALSE;

	if (pTileDesc->m_bHasTitle != bHasTitle)
	{
		pTileDesc->m_bHasTitle = bHasTitle;
		pTileDesc->SetUpdated(&pTileDesc->m_bHasTitle);
	}

	if (pTileDesc->m_strText != GetTitle())
	{
		pTileDesc->m_strText = GetTitle();
		pTileDesc->SetUpdated(&pTileDesc->m_strText);
	}
	return (LRESULT)pTileDesc;

}

//-----------------------------------------------------------------------------
BOOL CBaseTileDialog::IsDisplayed()
{
	if (IsCollapsed())
		return FALSE;

	LayoutElement* pParentElement = GetParentElement();

	while (pParentElement)
	{
		Tile* pTile = dynamic_cast<Tile*>(pParentElement);
		if (pTile && pTile->IsCollapsed())
			return FALSE;

		pParentElement = pParentElement->GetParentElement();
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBaseTileDialog::IsGroupCollapsible()
{
	LayoutElement* pParentElement = AsALayoutElement()->GetParentElement();
	return pParentElement && pParentElement->IsGroupCollapsible();

}

//------------------------------------------------------------------------------
BOOL CBaseTileDialog::SetNextControlFocus(BOOL bBackward)
{
	return GetParentTileGroup()->SetNextControlFocus(this, bBackward, NULL);
}

// nSizeAction = 0 NoAction
// nSizeAction = 1 tileTypeChanged
//-----------------------------------------------------------------------------
void CBaseTileDialog::ChangeSizeTo(CSize aSize, int nSizeAction /* 0*/)
{
	CRect rectActual;
	GetWindowRect(&rectActual);
	ScreenToClient(&rectActual);
	
	// ricalcolo della size considerando rect originale
	rectActual.right = m_rectOriginal.left + aSize.cx;
	rectActual.bottom = m_rectOriginal.top + aSize.cy;
	
	m_rectOriginal = rectActual;

	if (nSizeAction > 0)
	{
		RemoveParts();
		GenerateParts();
	}
	
	// sti calcoli danno per scontato che sia tutto senza titolo
	// per evitare di farne altre copie, azzero un secondo il titolo
	int nTempTitleHeight = m_nTitleHeight;
	m_nTitleHeight = 0;
	SetSize(rectActual, FALSE, TRUE);
	m_nTitleHeight = nTempTitleHeight;
	RequestRelayout();
}

//-----------------------------------------------------------------------------
void CBaseTileDialog::ApplyDeltaToSecondStaticArea(int nDelta)
{
	if (!HasParts())
		return;

	CTileDialogPart* pSecondPart = m_TileDialogParts[1];

	if (!pSecondPart)
		return;

	CRect aRect = pSecondPart->GetStaticAreaRect();

	aRect.left += nDelta;
	aRect.right += nDelta;

	CRect actualRect;
	pSecondPart->GetUsedRect(actualRect);

	actualRect.left = aRect.left;
	pSecondPart->Relayout(actualRect);
	pSecondPart->SetStaticAreaExtent(aRect.left, aRect.right);

	Invalidate();
}

//-----------------------------------------------------------------------------
int CBaseTileDialog::GetIntersectPartNo(CRect r)
{
	CRect maxIntersectionRect;
	int nPart = -1;
	maxIntersectionRect.SetRectEmpty();
	for (int p = 0; p <= m_TileDialogParts.GetUpperBound(); p++)
	{
		CRect intersectionRect = m_TileDialogParts[p]->Intersect(r);
		if (intersectionRect != NULL)
		{
			if (CalculateRectArea(intersectionRect) > CalculateRectArea(maxIntersectionRect))
			{
				maxIntersectionRect = intersectionRect;
				nPart = p;
			}
		}
	}
	return nPart;
}

//-----------------------------------------------------------------------------
int CBaseTileDialog::GetOwnerPart(CWnd* pWnd)
{
	if (m_TileDialogParts.GetSize() == 0)
		return 0;

	CRect aRect;
	pWnd->GetWindowRect(&aRect);
	ScreenToClient(&aRect);
	// prima guardo se appartiene ai controlli della part
	for (int i = m_TileDialogParts.GetSize() - 1; i >= 0; i--)
	{
		CTileDialogPart* pPart = m_TileDialogParts.GetAt(i);
		if (pPart && pPart->Contains(pWnd))
			return i;
	}


	BOOL bInDesignMode = GetDocument() && GetDocument()->IsInDesignMode();
	if (bInDesignMode)
	{
		// se sono in design mode vado a cercarmi dove si trova la static area
		// della seconda parte 
		CWnd* pStaticArea2 = GetDlgItem(IDC_STATIC_AREA_2);
		if (pStaticArea2)
		{
			CRect aStaticRect;
			pStaticArea2->GetWindowRect(aStaticRect);
			ScreenToClient(aStaticRect);
			if (aRect.right > aStaticRect.right)
				return 1;
		}
		return 0;
	}
	
	for (int i = m_TileDialogParts.GetSize() - 1; i >= 0; i--)
	{
		CTileDialogPart* pPart = m_TileDialogParts.GetAt(i);
		if (pPart && aRect.right > pPart->GetStaticAreaRect().right)
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
CTileDialogPart* CBaseTileDialog::GetPart(int nIdx)
{
	return nIdx >= 0 && nIdx < m_TileDialogParts.GetSize() ? m_TileDialogParts[nIdx] : NULL;
}

/////////////////////////////////////////////////////////////////////////////
// 							TileDialogArray
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TileDialogArray, Array)

/////////////////////////////////////////////////////////////////////////////
// TileDialogArray diagnostics

#ifdef _DEBUG
void TileDialogArray::AssertValid() const
{
	Array::AssertValid();
}

void TileDialogArray::Dump(CDumpContext& dc) const
{
	Array::Dump(dc);
	AFX_DUMP0(dc, "\nTileDialogArray");
}
#endif //_DEBUG

////////////////////////////////////////////////////////////////////////////////
//				class CParsedDialogWithTiles definition
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CParsedDialogWithTiles, CParsedDialog)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CParsedDialogWithTiles, CParsedDialog)
	ON_WM_SHOWWINDOW()
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CParsedDialogWithTiles::CParsedDialogWithTiles(UINT nIdd, CWnd* pWndParent, const CString& sName)
	:
	CParsedDialog(nIdd, pWndParent, sName),
	m_pTileGroup(NULL),
	m_pTileStyle(NULL)
{
	InitLayout();
}

//-----------------------------------------------------------------------------
CParsedDialogWithTiles::CParsedDialogWithTiles()
	:
	m_pTileGroup(NULL),
	m_pTileStyle(NULL)
{
	InitLayout();
}

//-----------------------------------------------------------------------------
CParsedDialogWithTiles::~CParsedDialogWithTiles()
{
	SAFE_DELETE(m_pTileGroup);
	SAFE_DELETE(m_pLayoutContainer);
	SAFE_DELETE(m_pTileStyle);
}

//---------------------------------------------------------------------------
void CParsedDialogWithTiles::RequestRelayout()
{
	__super::RequestRelayout();
	DoStretch();
}

//---------------------------------------------------------------------------
void CParsedDialogWithTiles::OnShowWindow(BOOL bShow, UINT nStatus)
{
	__super::OnShowWindow(bShow, nStatus);

	if (!bShow)
		return;

	if (m_pTileGroup)
		DoStretch();

	CWnd* pWnd = GetLastFocusedCtrl();
	if (pWnd)
		pWnd->SetFocus();
	else if (GetControlLinks())
		GetControlLinks()->SetDefaultFocus(m_pOwnerWnd, m_phLastCtrlFocused);
}

//---------------------------------------------------------------------------
void CParsedDialogWithTiles::InitLayout()
{
	m_pTileStyle = TileStyle::Inherit(AfxGetTileDialogStyleNormal());
	m_pLayoutContainer = new CLayoutContainer(this, m_pTileStyle);
	m_pLayoutContainer->SetLayoutType(CLayoutContainer::VBOX);
}

//---------------------------------------------------------------------------
CBaseTileGroup* CParsedDialogWithTiles::AddTileGroup(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate)
{
	m_pTileGroup = __super::AddBaseTileGroup(nIDC, pClass, sName, bCallOnInitialUpdate);

	// disabilito lo stretch che altrimenti mi rema contro quando ricalcolo il layout
	m_pTileGroup->SetAutoSizeCtrl(0);

	return m_pTileGroup;
}

//------------------------------------------------------------------------------
void CParsedDialogWithTiles::GetAvailableRect(CRect &rectAvail)
{
	GetClientRect(rectAvail);
}

//------------------------------------------------------------------------------
int	CParsedDialogWithTiles::GetRequiredHeight(CRect &rectAvail)
{
	return rectAvail.Height();
}

//------------------------------------------------------------------------------
void CParsedDialogWithTiles::GetUsedRect(CRect &rectUsed)
{
	GetWindowRect(rectUsed);
}

//---------------------------------------------------------------------------
void CParsedDialogWithTiles::ResizeOtherComponents(CRect aRect)
{
	if (m_pTileGroup)
	{
		if (aRect.top > 0 && (m_pToolBar->GetEnabledAlignment() & CBRS_ALIGN_BOTTOM) == CBRS_ALIGN_BOTTOM)
		{
			ASSERT(FALSE);
		}
		m_pTileGroup->SetWindowPos(NULL, 0, aRect.top, aRect.Width(), aRect.Height(), SWP_NOZORDER);
		Relayout(aRect);
	}
}

//------------------------------------------------------------------------------
void CParsedDialogWithTiles::Relayout(CRect &rectNew, HDWP hDWP /*= NULL*/)
{
	if (IsLayoutSuspended())
		return;

	m_pLayoutContainer->Relayout(rectNew);
}

//-------------------------------------------------------------------------------
void CParsedDialogWithTiles::OnUpdateControls(BOOL bParentIsVisible)
{
	if (m_pControlLinks)
		__super::OnUpdateControls(bParentIsVisible);

	if (m_pTileGroup)
		m_pTileGroup->OnUpdateControls(bParentIsVisible);
}

//-----------------------------------------------------------------------------
BOOL CParsedDialogWithTiles::PreProcessMessage(MSG* pMsg)
{
	if (pMsg->message == WM_KEYDOWN)
	{
		switch (pMsg->wParam)
		{
		case VK_ESCAPE:
		{
			if ((GetStyle() & WS_POPUP) == WS_POPUP)
			{
				m_pOwnerWnd->SendMessage(WM_COMMAND, IDCANCEL, 0);
				return TRUE;
			}
		}
		}
	}

	return FALSE;
}
//-----------------------------------------------------------------------------
void EnumTileDescriptionAssociations::InitEnumTileDescriptionStructures()
{
	/*TileDialogSize*/
	m_arTileDialogSize.Add(TILE_MICRO,		_T("Micro"));
	m_arTileDialogSize.Add(TILE_MINI,		_T("Mini"));
	m_arTileDialogSize.Add(TILE_LARGE,		_T("Large"));
	m_arTileDialogSize.Add(TILE_WIDE,		_T("Wide"));
	m_arTileDialogSize.Add(TILE_AUTOFILL,	_T("AutoFill"));
	m_arTileDialogSize.Add(TILE_STANDARD,	_T("Standard"));
	
	/*TileDialogStyle*/
	m_arTileDialogStyle.Add(TDS_NORMAL,		_T("Normal"));
	m_arTileDialogStyle.Add(TDS_FILTER,		_T("Filter"));
	m_arTileDialogStyle.Add(TDS_HEADER,		_T("Header"));
	m_arTileDialogStyle.Add(TDS_FOOTER,		_T("Footer"));
	m_arTileDialogStyle.Add(TDS_WIZARD,		_T("Wizard"));
	m_arTileDialogStyle.Add(TDS_PARAMETERS,	_T("Parameters"));
	m_arTileDialogStyle.Add(TDS_BATCH,		_T("Batch"));
	m_arTileDialogStyle.Add(TDS_NONE,		_T("None"));
}