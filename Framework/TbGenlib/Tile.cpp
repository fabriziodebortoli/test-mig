#include "stdafx.h"

#include <TbGeneric\TBThemeManager.h>
#include <TbGenlib\BaseTileManager.h>
#include <TbGenlib\OslInfo.h>

#include "Tile.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//					class Tile implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
Tile::Tile(TileStyle* pStyle)
	:
	m_nTitleHeight				(0),
	m_bCollapsed				(FALSE),
	m_nCollapseButtonOffsetY	(TILE_COLLAPSE_BUTTON_OFFSET),
	m_bProcessingSiblingRequest	(FALSE),
	m_bPinned					(TRUE),
	m_bEnabled					(TRUE),
	m_bVisible					(TRUE)
{
	// The tile has a style from the beginning, allowing set some customized proprties in the derived
	// class's constructor. After the dialog is created, it is attached to the style of the parent (either
	// TileGroup or TilePanel). Customized properties are retained
	m_pTileStyle	= TileStyle::Inherit(pStyle);

	m_nCollapseButtonOffsetY = max(TILE_COLLAPSE_BUTTON_OFFSET, GetTileStyle()->GetTitleTopSeparatorWidth());
}

//-----------------------------------------------------------------------------
Tile::~Tile()
{
	SAFE_DELETE(m_pTileStyle);
}

//-----------------------------------------------------------------------------
void Tile::Show(BOOL bVisible)
{
	IOSLObjectManager* pInfo = dynamic_cast< IOSLObjectManager* >(GetTileCWnd());
	if (pInfo) {
		if (!OSL_CAN_DO(pInfo->GetInfoOSL(), OSL_GRANT_EXECUTE))
			bVisible = FALSE;
	}
	
	m_bVisible = bVisible;
	int nCmdShow = bVisible ? SW_SHOW : SW_HIDE;
	if (GetTileCWnd())
		GetTileCWnd()->ShowWindow(nCmdShow);
}

//-----------------------------------------------------------------------------
void Tile::SetTileStyle(TileStyle* pStyle)
{
	m_pTileStyle ->Assign(pStyle);

	CalculateTitleHeight();
	m_nCollapseButtonOffsetY = max(TILE_COLLAPSE_BUTTON_OFFSET, GetTileStyle()->GetTitleTopSeparatorWidth());
}

//-----------------------------------------------------------------------------
void Tile::ResetTileStyle(TileStyle* pStyle)
{
	SAFE_DELETE(m_pTileStyle);
	m_pTileStyle = TileStyle::Inherit(pStyle);
}

//-----------------------------------------------------------------------------
void Tile::SetTileStyleByName(CString sName)
{
	SetTileStyle(AfxGetTileDialogStyle(sName));
}
//-----------------------------------------------------------------------------
void Tile::SetCollapsible(BOOL bSet /*= TRUE*/)
{ 
	m_pTileStyle->SetCollapsible(bSet); 
	if (GetTileCWnd()->m_hWnd && ::IsWindow(GetTileCWnd()->m_hWnd))
		ShowCollapsedButton();
}

//-----------------------------------------------------------------------------
void Tile::SetPinnable(BOOL bSet /*= TRUE*/)
{ 
	m_pTileStyle->SetPinnable(bSet); 
	if (GetTileCWnd()->m_hWnd && ::IsWindow(GetTileCWnd()->m_hWnd))
		ShowPinnedButton();
}

//-----------------------------------------------------------------------------
BOOL Tile::IsPinnable()
{
	return m_pTileStyle->Pinnable();
}

//-----------------------------------------------------------------------------
void Tile::SetHasTitle(BOOL bSet /*= TRUE*/)
{ 
	m_pTileStyle->SetHasTitle(bSet); 
	CalculateTitleHeight();
}

//-----------------------------------------------------------------------------
void Tile::Enable(BOOL bEnable) 
{ 
	m_bEnabled = bEnable; 
}

//-----------------------------------------------------------------------------
void Tile::SetCollapsed (BOOL bCollapsed /*= TRUE*/)
{
	// not yet created, i.e.: called in the constructor or in BuildDataControlLinks
	if (GetTileCWnd() == NULL || GetTileCWnd()->m_hWnd == NULL /*|| !m_bCreationCompleted*/)
	{
		m_bCollapsed = bCollapsed;
		return;
	}

	// it is meaningful only on collapsible tiles
	if (!m_pTileStyle->Collapsible())
		return;

	if (m_bCollapsed != bCollapsed)
		CollapseExpand();
}

//-----------------------------------------------------------------------------
void Tile::SetPinned (BOOL bPinned /*= TRUE*/)
{
	// not yet created, i.e.: called in the constructor or in BuildDataControlLinks
	if (GetTileCWnd() == NULL || GetTileCWnd()->m_hWnd == NULL /*|| !m_bCreationCompleted*/)
	{
		TRACE("You shouldn't call this method until the tile window has been created!");
		ASSERT(FALSE);
		m_bPinned= bPinned;
		return;
	}

	// it is meaningful only on collapsible tiles
	if (!m_pTileStyle->Pinnable())
		return;

	if (m_bPinned != bPinned)
		PinUnpin();
}

//------------------------------------------------------------------------------
void Tile::PinUnpin()
{
	if (!m_pTileStyle->Pinnable())
		return;

	m_bPinned = !m_bPinned;

	RefreshPinButton();
	DoPinUnpin();
}

//------------------------------------------------------------------------------
void Tile::RefreshPinButton()
{
	if (HasPinnedButton())
		m_PinButton.ChangeIcon(m_bPinned);
}

//-----------------------------------------------------------------------------
void Tile::ForceSetPinned(BOOL bPinned /*= TRUE*/)
{
	m_bPinned = bPinned;

	// not yet created, i.e.: called in the constructor or in BuildDataControlLinks
	if (GetTileCWnd() == NULL || GetTileCWnd()->m_hWnd == NULL /*|| !m_bCreationCompleted*/)
	{
		return;
	}

	// it is meaningful only on collapsible tiles
	if (!m_pTileStyle->Pinnable())
		return;
	RefreshPinButton();

	DoPinUnpin();
}

//------------------------------------------------------------------------------
void Tile::CollapseExpand()
{
	if (!m_pTileStyle->Collapsible())
		return;

	m_bCollapsed = !m_bCollapsed;

	LayoutElement* pParent = AsALayoutElement()->GetParentElement();
	if	(
			!m_bProcessingSiblingRequest && // avoid recursive calls if the collapse/expand action has been required by a sibling
			pParent &&
			IsGroupCollapsible() && 
			pParent->GetContainedElements()
		)
	{
		CRect rectTile;
		GetTileCWnd()->GetWindowRect(rectTile);

		for (int e = 0; e <= pParent->GetContainedElements()->GetUpperBound(); e++)
		{
			Tile* pSibling =  pParent->GetContainedElements()->GetAt(e)->AsATile();
			if	(
					!pSibling ||
					pSibling->GetTileCWnd() == this->GetTileCWnd()||
					!pSibling->m_pTileStyle->Collapsible() ||
					pSibling->m_bCollapsed == m_bCollapsed
				)
				continue;

			CRect rectSibling;
			pSibling->GetTileCWnd()->GetWindowRect(rectSibling);

			if (rectSibling.top != rectTile.top)
				continue; // not on the same row

			// avoid recursive calls
			pSibling->m_bProcessingSiblingRequest = TRUE;
			pSibling->CollapseExpand();
			pSibling->m_bProcessingSiblingRequest = FALSE;
		}
	}
		
	if (HasCollapsedButton())
		m_CollapseButton.ChangeIcon(m_bCollapsed);

	DoCollapseExpand();
}

//-----------------------------------------------------------------------------
BOOL Tile::HasCollapsedButton () const
{
	return m_CollapseButton.m_hWnd != NULL;
}

//-----------------------------------------------------------------------------
BOOL Tile::HasPinnedButton () const
{
	return m_PinButton.m_hWnd != NULL;
}

//-----------------------------------------------------------------------------
void Tile::ShowCollapsedButton()
{
	if (m_pTileStyle->Collapsible() && !HasCollapsedButton())
	{
		CRect rectButton(TILE_COLLAPSE_BUTTON_OFFSET, m_nCollapseButtonOffsetY, TILE_COLLAPSE_BUTTON_OFFSET + TILE_COLLAPSE_BUTTON_SIZE, m_nCollapseButtonOffsetY+ TILE_COLLAPSE_BUTTON_SIZE);
		if (!HasTransparentBackground())
			m_CollapseButton.SetColor(m_pTileStyle->GetTitleBkgColor());
		m_CollapseButton.Create(L"", WS_CHILD|WS_VISIBLE|BS_OWNERDRAW, ScaleRect(rectButton) , this, ID_TILE_COLLAPSE_BUTTON, m_bCollapsed);
	}
	else if (!m_pTileStyle->Collapsible() && HasCollapsedButton())
	{
		m_CollapseButton.DetachImages();
		m_CollapseButton.DestroyWindow();
	}
		
}

//-----------------------------------------------------------------------------
void Tile::ShowPinnedButton()
{
	if (m_pTileStyle->Pinnable() && !HasPinnedButton())
	{
		CRect rectButton;
		GetTileCWnd()->GetClientRect(rectButton);

		rectButton.top = m_nCollapseButtonOffsetY;
		rectButton.bottom = m_nCollapseButtonOffsetY + TILE_COLLAPSE_BUTTON_SIZE;
		rectButton.right -= TILE_COLLAPSE_BUTTON_OFFSET;
		rectButton.left = rectButton.right - TILE_COLLAPSE_BUTTON_SIZE;

		m_PinButton.Create(L"", WS_CHILD | WS_VISIBLE | BS_OWNERDRAW, ScaleRect(rectButton), this, ID_TILE_COLLAPSE_BUTTON, m_bPinned);
		m_PinButton.SetColor(m_pTileStyle->GetTitleBkgColor(), HasTransparentBackground());

	}
	else if (!m_pTileStyle->Pinnable() && HasPinnedButton())
	{
		m_PinButton.DetachImages();
		m_PinButton.DestroyWindow();
	}
}

//-----------------------------------------------------------------------------
void Tile::CalculateTitleHeight()
{
	// sometime the request is made before the tile is created
	if (!GetParentTileGroup())
		return;

	m_nTitleHeight = GetTileStyle()->HasTitle() ? 
						MulDiv(AfxGetThemeManager()->GetTileTitleHeight(), AfxGetThemeManager()->GetBaseUnitsHeight(), 100) : 
						GetTileStyle()->GetTitleTopSeparatorWidth() + GetTileStyle()->GetTitleBottomSeparatorWidth();
}

//-----------------------------------------------------------------------------
void Tile::OnTilePosChanged()
{	
	LONG buttonSize = ScalePix(TILE_COLLAPSE_BUTTON_SIZE);
	LONG buttonOff = ScalePix(TILE_COLLAPSE_BUTTON_OFFSET);

	if (HasCollapsedButton())
	{
		m_CollapseButton.MoveWindow(buttonOff, m_nCollapseButtonOffsetY, buttonSize, buttonSize, TRUE);
	}
	if (HasPinnedButton())
	{
		CRect rectTile;
		GetTileCWnd()->GetClientRect(rectTile);
		m_PinButton.MoveWindow(rectTile.right - buttonSize - buttonOff, m_nCollapseButtonOffsetY, buttonSize, buttonSize, TRUE);
	}
}

//-----------------------------------------------------------------------------
void Tile::TileCreate(const CString& strTitle)
{	
	SetTitle(strTitle);
	CalculateTitleHeight();
	ShowCollapsedButton();
	ShowPinnedButton();
}

//------------------------------------------------------------------------------
void Tile::OnTilePaint(CDC* pDC)
{
	//rectangle for tile title
	CRect rectTitle;
	GetTileCWnd()->GetWindowRect(&rectTitle);
	GetTileCWnd()->ScreenToClient(&rectTitle);
	rectTitle.bottom = m_nTitleHeight;
	CString sTitle = GetCurrentTitle();
	// first, draw the rectagle for the title
	BOOL bHasTitle = GetTileStyle()->HasTitle();
	BOOL bIsTrasparent = HasTransparentBackground();
	if (bHasTitle && !bIsTrasparent)
	{
		pDC->SetBkColor(GetTileStyle()->GetTitleBkgColor());
		pDC->FillRect(&rectTitle, GetTileStyle()->GetTitleBkgColorBrush());
	}
	else
	{
		pDC->SetBkColor(GetTileStyle()->GetTitleBkgColor());
	}

	// then, draw the edge, if any
	CRect originalRectTile;
	if (GetTileStyle()->GetAspect() == TileStyle::EDGE)
	{
		CRect rectTile;
		GetTileCWnd()->GetClientRect(&rectTile);
		rectTile.top += m_nTitleHeight / 2;
		UINT nFlags = IsCollapsed() ? BF_TOP : BF_RECT;
		originalRectTile = rectTile;

		if (bIsTrasparent)
		{
			pDC->DrawEdge(rectTile, EDGE_ETCHED, BF_BOTTOMLEFT);
			pDC->DrawEdge(rectTile, EDGE_ETCHED, BF_RIGHT);
			CRect cornerRect(originalRectTile);
			
			if (bHasTitle)
				cornerRect.right = GetTileStyle()->GetTitlePadding();

			pDC->DrawEdge(cornerRect, EDGE_ETCHED, BF_TOP);
		}
		else
			pDC->DrawEdge(rectTile, EDGE_ETCHED, nFlags);
	}
	else if (GetTileStyle()->GetAspect() == TileStyle::TOP)
	{
		CRect rectTile;
		GetTileCWnd()->GetClientRect(&rectTile);
		rectTile.top += m_nTitleHeight / 2;
		if (bIsTrasparent && bHasTitle)
			rectTile.right = GetTileStyle()->GetTitlePadding();
		pDC->DrawEdge(rectTile, EDGE_ETCHED, BF_TOP);
	}

	// finally, draw the title text
	if (bHasTitle)
	{
		//@@ TODO ma serve??
		//pDC->SelectObject((HFONT) AfxGetThemeManager()->GetFormFont()->GetSafeHandle());

		UINT dtStyle = DT_SINGLELINE | DT_VCENTER;
		int nAlign = GetTileStyle()->GetTitleAlignment();
		switch (nAlign)
		{ //0=left, 1=center 2=right
		case 1:
			dtStyle |= DT_CENTER;
			break;
		case 2:
			dtStyle |= DT_RIGHT;
			break;
		default:
		{
			dtStyle |= DT_LEFT;
			rectTitle.OffsetRect(GetTileStyle()->GetTitlePadding(), 0);
			break;
		}
		}

		//Shift the title right to make room for the collapse/pin button
		if (GetTileStyle()->Collapsible())
		{
			rectTitle.left += ScalePix(TILE_COLLAPSE_BUTTON_OFFSET + TILE_COLLAPSE_BUTTON_SIZE + TILE_COLLAPSE_BUTTON_OFFSET);
		}

		pDC->SetTextColor(GetTileStyle()->GetTitleForeColor());

		CFont*	pOldFont = pDC->SelectObject(GetTileStyle()->GetTitleFont());

		if (bIsTrasparent)
		{
			pDC->SetBkMode(TRANSPARENT);
			if (GetTileStyle()->GetAspect() == TileStyle::EDGE || GetTileStyle()->GetAspect() == TileStyle::TOP)
			{
				CRect cornerRect(originalRectTile);

				if (!sTitle.IsEmpty())
				{
					CSize textSize = pDC->GetTextExtent(sTitle + _T(" "));
					cornerRect.left = cornerRect.left + textSize.cx + 10;
				}
				pDC->DrawEdge(cornerRect, EDGE_ETCHED, BF_TOP);
			}
		}

		if (!sTitle.IsEmpty())
		{
			if (GetTileStyle()->GetAspect() == TileStyle::EDGE || GetTileStyle()->GetAspect() == TileStyle::TOP)
				pDC->DrawText(_T(" ") + sTitle + _T(" "), &rectTitle, dtStyle);
			else
				pDC->DrawText(sTitle, &rectTitle, dtStyle);
		}

		pDC->SelectObject(pOldFont);
	}

	if (GetTileStyle()->GetTitleTopSeparatorWidth() > 0)
	{
		CRect rectSeparator;
		GetTileCWnd()->GetWindowRect(&rectSeparator);
		GetTileCWnd()->ScreenToClient(&rectSeparator);

		rectSeparator.bottom = rectSeparator.top + GetTileStyle()->GetTitleTopSeparatorWidth();
		pDC->FillSolidRect(&rectSeparator, GetTileStyle()->GetTitleSeparatorColor());
	}
	if (GetTileStyle()->GetTitleBottomSeparatorWidth() > 0)
	{
		CRect rectSeparator;
		GetTileCWnd()->GetWindowRect(&rectSeparator);
		GetTileCWnd()->ScreenToClient(&rectSeparator);

		rectSeparator.bottom = rectSeparator.top + m_nTitleHeight;
		rectSeparator.top = rectSeparator.bottom - GetTileStyle()->GetTitleBottomSeparatorWidth();

		pDC->FillSolidRect(&rectSeparator, GetTileStyle()->GetTitleSeparatorColor());
	}

	
}

//------------------------------------------------------------------------------
BOOL Tile::HasTransparentBackground()
{
	if (!GetTileCWnd()->IsKindOf(RUNTIME_CLASS(CBaseTileDialog)))
		return FALSE;
	CBaseTileDialog*  pDialog = dynamic_cast<CBaseTileDialog*> (GetTileCWnd());
	return pDialog && pDialog->IsTransparent();
}

//------------------------------------------------------------------------------
BOOL Tile::IsGroupCollapsible()
{
	return FALSE;
}
