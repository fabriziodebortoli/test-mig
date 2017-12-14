#include "stdafx.h"
#include "LayoutContainer.h"

#include "BaseTileManager.h"
#include "TABCORE.H"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


EnumLayoutContainerDescriptionAssociations CWndLayoutContainerDescription::singletonEnumLayoutContainerDescription;


/////////////////////////////////////////////////////////////////////////////
//					class LayoutElement implementation
/////////////////////////////////////////////////////////////////////////////
LayoutElement::LayoutElement()
	:
	m_nFlex				(AUTO), // no flex, maintain original sizes
	m_nMinHeight		(AUTO),
	m_nMinWidth			(AUTO),
	m_nMaxWidth			(AUTO),
	m_pParentElement	(NULL),
	m_bGroupCollapsible	(FALSE),
	m_nRequestedLastFlex(AUTO)
{
}

//------------------------------------------------------------------------------
void LayoutElement::RequestRelayout()
{
	if (m_pParentElement)
		m_pParentElement->RequestRelayout();
	else
	{
		CRect rectAvail;
		GetAvailableRect(rectAvail);
		Relayout(rectAvail);
	}
}

/////////////////////////////////////////////////////////////////////////////
//					class LayoutInfo definition & implementation
/////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
class LayoutInfo
{
public:
	LayoutInfo(LayoutElement* pElem)
		:
		m_Rect(0, 0, 0, 0),
		m_pElem(pElem),
		m_bIsLastRight(FALSE),
		m_nRow(-1),
		m_bIsBottom(FALSE)
	{}

public:
	LayoutElement*	m_pElem;
	CRect			m_Rect;
	BOOL			m_bIsBottom;
	// backward compatibility for STRIPE
	BOOL			m_bIsLastRight;
	int				m_nRow;					//Riga su cui giace la Tile


	CBaseTileDialog* GetTileDialog() { return dynamic_cast<CBaseTileDialog*>(m_pElem); }
};

typedef CArray<LayoutInfo*, LayoutInfo*&> LayoutInfoArray;
//


//*****************************************************************************
// Stripe Manager
//*****************************************************************************
//mantiene le informazioni di rettangolo su cui dovra disegnarsi la tile, e sulle cordinate di riga (m_nRow) e indice sulla riga (m_nColIndex)
class StripeManager : public CArray<LayoutInfo*, LayoutInfo*>
{
public:
	~StripeManager()
	{
		for (int i = GetUpperBound(); i >= 0; i--)
		{
			LayoutInfo* pInfo = GetAt(i);
			SAFE_DELETE(pInfo);
		}
	}

	//------------------------------------------------------------------------------
	void FinalAlignmentAndStretch (BOOL bFillEmptySpace, CRect &rectContainer, int tileSpacing)
	{
		//Allineamento delle tiles al bottom della riga (per riempire la riga)
		AlignTilesOnTheSameRowToBottomRow(bFillEmptySpace);
		//Stretch delle tiles (per ogni riga) in modo che che riempano il contenitore
		StretchHorizontal(bFillEmptySpace, rectContainer, tileSpacing);
		//Allineamento delle tiles dell'ultima riga in basso (per riempire il contenitore) 
		AlignLastBottomTilesToBottomOfContainer(bFillEmptySpace, rectContainer);
	}

	//------------------------------------------------------------------------------
	int CalculateRowBottom( int nRowIndex)
	{
		int maxHeight = 0;
		for (int i = 0; i <= GetUpperBound(); i++)
		{
			LayoutInfo* pInfo = (LayoutInfo*) GetAt(i);
			// scelgo la prima della mia riga
			if (pInfo && pInfo->m_nRow == nRowIndex)
				maxHeight = max(maxHeight, pInfo->m_Rect.bottom);
		}
		return maxHeight;
	}

private:
	//------------------------------------------------------------------------------
	void AlignTilesOnTheSameRowToBottomRow(BOOL bFillEmptySpace)
	{
		for (int i = 0; i <= GetUpperBound(); i++)
		{
			LayoutInfo* pInfo = (LayoutInfo*) GetAt(i);
			
			if (!pInfo || !pInfo->GetTileDialog())
				continue;

			if (bFillEmptySpace || (pInfo->GetTileDialog()->IsAutoFill()))
			{
				int bottom = CalculateRowBottom(pInfo->m_nRow);
				pInfo->m_Rect.bottom = bottom;
				//Adattamento area statica
				pInfo->GetTileDialog()->ResizeStaticAreaHeight(bottom);
			}
		}
	}

	//------------------------------------------------------------------------------
	void StretchHorizontal(BOOL bFillEmptySpace, CRect &rectContainer, int tileSpacing)
	{
		int currentRow = 0;
		int tilesPerRow = 0;
		//Modifica i rettangoli delle tile a destra
		for (int i = 0; i <= GetUpperBound(); i++)
		{
			LayoutInfo* pInfo = (LayoutInfo*) GetAt(i);
			tilesPerRow++;

			if (!pInfo || !pInfo->m_bIsLastRight)
				continue;

			// lavoriamo solo sull'ultima tile della riga
			int availableEmptySpace = rectContainer.right - pInfo->m_Rect.right;
			currentRow = pInfo->m_nRow;
			int spaceAvailableForEachTile = (int)(availableEmptySpace / tilesPerRow);
			tilesPerRow = 0;
			int candidateLeft = -1;

			for (int j = 0; j <= GetUpperBound(); j++)
			{
				LayoutInfo* pCurrentInfo = (LayoutInfo*) GetAt(j);
				if (pInfo->m_nRow == pCurrentInfo->m_nRow)
				{
					int originalWidth = pCurrentInfo->m_Rect.Width();

					if (candidateLeft != -1)
					{
						pCurrentInfo->m_Rect.left = candidateLeft;
					}
					pCurrentInfo->m_Rect.right = pCurrentInfo->m_Rect.left + originalWidth + spaceAvailableForEachTile;
					candidateLeft = pCurrentInfo->m_Rect.right + tileSpacing;
				}

			}
		}
	}

	//------------------------------------------------------------------------------
	void AlignLastBottomTilesToBottomOfContainer(BOOL bFillEmptySpace, CRect &rectContainer)
	{
		//cerco maxRow
		int lastRow = -1;
		for (int i = 0; i <= GetUpperBound(); i++)
		{
			LayoutInfo* pInfo = (LayoutInfo*) GetAt(i);
			lastRow = max(lastRow, pInfo->m_nRow);
		}

		//Modifica i rettangoli delle tile in basso
		for (int i = 0; i <= GetUpperBound(); i++)
		{
			LayoutInfo* pInfo = (LayoutInfo*) GetAt(i);
			if (
				pInfo && 
				pInfo->m_nRow == lastRow										//se sono su ultima riga
				&&	pInfo->m_Rect.bottom < rectContainer.bottom			//se c'e' spazio in basso da riempire
				&& (bFillEmptySpace || pInfo->GetTileDialog()->IsAutoFill())		//se devo riempire lo spazio vuoto
				)
			{
				pInfo->m_Rect.bottom = rectContainer.bottom;
				pInfo->GetTileDialog()->ResizeStaticAreaHeight(rectContainer.bottom);
			}
		}
	}
};


/////////////////////////////////////////////////////////////////////////////
//					class Layout definition & implementation
/////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
class Layout
{
public:
	Layout()
	:
	rectRequired	(0,0,0,0),
	maxElemHeight	(0),
	maxElemWidth	(0)
	{
	}

	~Layout()
	{
		for (int i = 0; i < info.GetSize(); i++)
			delete info[i];
	}

public:
	int LeftmostAvailableColumn(int bottomOf)
	{
		for (int i = info.GetUpperBound(); i >= 0; i--)
		{
			if (info[i]->m_Rect.bottom > bottomOf)
				return info[i]->m_Rect.right;
		}

		return 0;
	}

	void InflateTiles(int first, CRect& rectContainer)
	{
		if (info.GetUpperBound() < 0)
			return;

		int availableSpace = rectContainer.Width() - info[info.GetUpperBound()]->m_Rect.right;
		int delta = availableSpace / (info.GetUpperBound() - first + 1);
		int currShift = 0;
		for (int t = first; t <= info.GetUpperBound(); t++)
		{
			info[t]->m_Rect.left += currShift;
			info[t]->m_Rect.right += currShift + delta;
			currShift += delta;
		}
	}

public:
	LayoutInfoArray info;
	CRect			rectRequired;
	int				maxElemHeight;
	int				maxElemWidth;
};

/////////////////////////////////////////////////////////////////////////////
//					class CLayoutContainer implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CLayoutContainer, CObject)

//------------------------------------------------------------------------------
CLayoutContainer::CLayoutContainer
	(
		LayoutElement*	pOwner, 
		TileStyle*&		pTileStyle
	)	
	:
	IDisposingSourceImpl(this),

	m_LayoutType		(COLUMN),
	m_LayoutAlign		(STRETCH),
	m_pParentScrollView	(NULL),
	m_bDoingLayout		(FALSE),
	m_pTileStyle		(pTileStyle)
{
	SetParentElement(pOwner); // for the container, the owner is the parent
}

//------------------------------------------------------------------------------
CLayoutContainer::~CLayoutContainer ()
{
	for (int i = m_OwnedContainers.GetCount() - 1; i >= 0; i--)
	{
		CLayoutContainer* pContainer = dynamic_cast<CLayoutContainer*>(m_OwnedContainers.GetAt(i));
		if (pContainer)
			delete pContainer;
	}
}

//------------------------------------------------------------------------------
const CString CLayoutContainer::GetElementNameSpace()
{
	CWnd* pWnd = GetCWnd();
	IOSLObjectManager* pOSL = dynamic_cast<IOSLObjectManager*>(pWnd);
	if (pOSL)
		return pOSL->GetInfoOSL()->m_Namespace.ToString();

	return _T("");
}


//------------------------------------------------------------------------------
LayoutElement* CLayoutContainer::AddChildElement(LayoutElement* pChild)
{
	m_Elements.Add(pChild);
	pChild->SetParentElement(this);
	return pChild;
}

//------------------------------------------------------------------------------
void CLayoutContainer::ClearChildElements()
{
	m_Elements.RemoveAll();
}
//------------------------------------------------------------------------------
void CLayoutContainer::RemoveChildElement(LayoutElement* pChild)
{
	int e = FindChildElement(pChild);
	if (e == -1)
	{ 
		ASSERT_TRACE(FALSE, "Element to remove not found in the container");
		return;
	}
	
	pChild->SetParentElement(NULL);
	m_Elements.RemoveAt(e);
}

//------------------------------------------------------------------------------
int CLayoutContainer::FindChildElement(LayoutElement* pChild)
{
	for (int e = 0; e <= m_Elements.GetUpperBound(); e++)
		if (m_Elements[e] == pChild)
			return e;
	
	ASSERT_TRACE(FALSE, "Child element not found in the container");
	return -1;
}

//------------------------------------------------------------------------------
void CLayoutContainer::InsertChildElement(LayoutElement* pChild, int nPos)
{
	ASSERT_TRACE(nPos >= 0, "Bad position");

	if (nPos < 0 )
		nPos = 0;
	if (nPos > m_Elements.GetUpperBound())
		nPos = m_Elements.GetSize();

	if (nPos == m_Elements.GetSize())
		m_Elements.Add(pChild);
	else
		m_Elements.InsertAt(nPos, pChild);

	pChild->SetParentElement(this);
}

//------------------------------------------------------------------------------
void CLayoutContainer::SetGroupCollapsible(BOOL bSet /*= TRUE*/)		
{ 
	m_bGroupCollapsible = bSet; 
	for (int e = 0; e <= m_Elements.GetUpperBound(); e++)
		m_Elements.GetAt(e)->SetGroupCollapsible(bSet);
}

//------------------------------------------------------------------------------
void CLayoutContainer::Relayout(CRect& rectContainer, HDWP hDWP /*= NULL*/)
{
	// avoid repeatedly calling this method (i.e.: while processing resize messages)
	// while layout is still in process
	if (m_bDoingLayout)
		return;

	m_bDoingLayout = TRUE;

	switch (m_LayoutType)
	{
		
		case STRIPE:	DoStripeLayout(rectContainer, hDWP); break;
		case HBOX:		DoHBoxLayout(rectContainer); break;
		case VBOX:		DoVBoxLayout(rectContainer); break;
		case COLUMN:	DoColumnLayout(rectContainer); break;
		default:
		{
			ASSERT_TRACE(FALSE,"Layout not yet supported!\n");
			DoStripeLayout(rectContainer);
		}
	}

	m_bDoingLayout = FALSE;
}

//------------------------------------------------------------------------------
int	CLayoutContainer::GetFlex(FlexDim fd)
{
	if (!IsFlexAuto())
		return m_nFlex;

	if (GetRequestedLastFlex() != AUTO)
		return GetRequestedLastFlex();
	
	return 0;
}

//------------------------------------------------------------------------------
BOOL CLayoutContainer::CanDoLastFlex(FlexDim  fd)
{
	switch (m_LayoutType)
	{
		case HBOX:		
		case VBOX:		
		{
			for (int i = 0; i <= m_Elements.GetUpperBound(); i++)
			{
				if (m_Elements.GetAt(i)->IsVisible() && m_Elements.GetAt(i)->CanDoLastFlex(fd))
					return TRUE;
			}
			return FALSE;
		}
		case COLUMN:
		{
			return	(m_LayoutAlign == STRETCH || m_LayoutAlign == STRETCHMAX) &&
					m_Elements.GetSize() > 0 &&
					m_Elements.GetAt(m_Elements.GetUpperBound())->CanDoLastFlex(fd); // @@TODO approssimazione!!
		}
		default:
		{
			ASSERT_TRACE(FALSE,"Layout not supported!\n");
			return FALSE;
		}
	}
}

//------------------------------------------------------------------------------
void CLayoutContainer::PrepareHBoxLayout(Layout* pLayout, CRect& rectContainer)
{
	int spacing	= m_pTileStyle->GetTileSpacing();

	int totalFlex = 0;
	int totalNonFlexWidth = 0;

	for (int i = 0; i <= m_Elements.GetUpperBound(); i++)
	{
		m_Elements.GetAt(i)->SetRequestedLastFlex(AUTO);

		if (!m_Elements.GetAt(i)->IsVisible())
			continue;

		LayoutInfo* pInfo = new LayoutInfo(m_Elements.GetAt(i));
		
		if (m_Elements.GetAt(i)->IsFlex(WIDTH))
			totalFlex			+= m_Elements.GetAt(i)->GetFlex(WIDTH);
		else
			totalNonFlexWidth	+= m_Elements.GetAt(i)->GetRequiredWidth(rectContainer);

		int nHeight = m_Elements.GetAt(i)->GetRequiredHeight(rectContainer);
		pLayout->maxElemHeight = max(pLayout->maxElemHeight, nHeight);
		pLayout->info.Add(pInfo);
	}

	if (pLayout->info.GetSize() == 0)
		return;

	if (GetRequestedLastFlex() != AUTO && totalFlex == 0)
	{
		for (int i = pLayout->info.GetUpperBound(); i >= 0 ; i--)
		{
			LayoutElement* pElem = pLayout->info[i]->m_pElem;
			if (pLayout->info[i]->m_pElem->CanDoLastFlex(WIDTH))
			{
				pLayout->info[i]->m_pElem->SetRequestedLastFlex(GetRequestedLastFlex());
				totalFlex += pElem->GetFlex(WIDTH);
				totalNonFlexWidth -= pElem->GetRequiredWidth(rectContainer);
				break;
			}
		}
	}

	int totalFlexWidth = rectContainer.Width() - (spacing * (pLayout->info.GetSize() - 1)) - totalNonFlexWidth;
	int totalUnflexedWidth  = 0;
	int totalUnflexed = 0;

	int currLeftColumn = 0;
	CRect rectLeft(rectContainer);
	for (int i = 0; i <= pLayout->info.GetUpperBound(); i++)
	{
		rectLeft.left = currLeftColumn;

		LayoutElement* pElem = pLayout->info[i]->m_pElem;

		int elemWidth;
		if (pElem->IsFlex(WIDTH)) 
		{
			elemWidth =  (int)(((totalFlexWidth * 1.0) / totalFlex) * pElem->GetFlex(WIDTH));
			if (totalUnflexed > 0)
				elemWidth -= max(0, (int)(((totalUnflexedWidth * 1.0) / (totalFlex - totalUnflexed)) * pElem->GetFlex(WIDTH)));

			int nMinWidth = pElem->GetMinWidth();
			if (elemWidth < nMinWidth)
			{
				totalUnflexedWidth += (nMinWidth - elemWidth);
				totalUnflexed += pElem->GetFlex(WIDTH);
				elemWidth = nMinWidth;
			}
		}
		else
			elemWidth =  pElem->GetRequiredWidth(rectLeft);

		int elemHeight	= pElem->GetRequiredHeight(rectLeft);

		// The layout is prepared without taking in count vertical alignment, that is, every element is just placed at the begin of the container
		// The main preparation goal is to calculate sizes, the alignement will be applied later
		pLayout->info[i]->m_Rect = CRect(rectContainer.left + currLeftColumn, rectContainer.top, rectContainer.left + currLeftColumn + elemWidth, rectContainer.top + elemHeight);

		currLeftColumn += elemWidth + spacing;
	}

	for (int i = 0; i <= pLayout->info.GetUpperBound(); i++)
	{
		LayoutInfo* pInfo = pLayout->info[i];
		pLayout->rectRequired.UnionRect(pLayout->rectRequired, pLayout->info[i]->m_Rect);
	}
}

//------------------------------------------------------------------------------
void CLayoutContainer::DoHBoxLayout(CRect& rectContainer)
{
	Layout layout;

	PrepareHBoxLayout(&layout, rectContainer);

	if (layout.info.GetSize() == 0)
		return;

	// apply alignement before performing the layout
	for (int i = 0; i <= layout.info.GetUpperBound(); i++)
	{
		int top = 0;
		int bottom = 0;
		switch (m_LayoutAlign)
		{
			case STRETCH:
				top = 0;
				bottom = rectContainer.Height();
			break;

			case STRETCHMAX:
				top = 0;
				bottom = layout.maxElemHeight;
			break;
			
			case BEGIN:
				top = 0;
				bottom = layout.info[i]->m_Rect.Height();
			break;

			case END:
				top = rectContainer.Height() - layout.info[i]->m_Rect.Height();
				bottom = rectContainer.Height();
			break;

			case MIDDLE:
				top = (rectContainer.Height() - layout.info[i]->m_Rect.Height()) / 2;
				bottom = top + layout.info[i]->m_Rect.Height();
			break;
			case NO_ALIGN:
				top = 0;
				bottom = layout.info[i]->m_Rect.bottom;
				break;
		}

		layout.info[i]->m_Rect.top		= rectContainer.top + top;
		layout.info[i]->m_Rect.bottom	= rectContainer.top + bottom;

		layout.info[i]->m_pElem->Relayout(layout.info[i]->m_Rect);
	}
}

//------------------------------------------------------------------------------
void CLayoutContainer::CalculateIdealFlexCondition(CRect &rectContainer)
{
	if (!IsFlexAuto())
	{
		// nel caso di flex non in automatico aggiorno solo le prime impostazioni 
		// in non automatico
		
		for (int i = 0; i <= m_Elements.GetUpperBound(); i++)
		{
			LayoutElement* pElem = m_Elements.GetAt(i);
			Tile* pTile = dynamic_cast<Tile*>(pElem);
			if (pTile && pElem->IsFlexAuto())
			{
				CBaseTileDialog* pTileDialog = dynamic_cast<CBaseTileDialog*>(pElem);
				if (pTileDialog && pTileDialog->IsAutoFill())
					pElem->SetFlex(1, FALSE);
				else
					pElem->SetFlex(0, FALSE);
			}

		}
		return;
	}

	// provo a calcolare io il flex
	// 1) prima tolgo il flex a tutti 
	for (int i = 0; i <= m_Elements.GetUpperBound(); i++)
	{
		LayoutElement* pElem = m_Elements.GetAt(i);
		pElem->SetFlex(0, FALSE);
	}
		
	// 2) poi vado a cercare l'ultima a cui mettere il flex
	int nLastContainer = -1;
	for (int i = m_Elements.GetUpperBound(); i >= 0; i--)
	{
		LayoutElement* pElem = m_Elements.GetAt(i);
		Tile* pTile = dynamic_cast<Tile*>(pElem);
		CLayoutContainer* pContainer = dynamic_cast<CLayoutContainer*>(pElem);
		if (pContainer)
			nLastContainer = i;

		// agisco solo sulle tile e i tile panel visibili
		if (!pElem->IsVisible() || !pTile)
			continue;
		
		// nel caso di HBOX il flex comanda l'orizzontalità
		// e quindi vado a farlo stretchare a prescindere 
		// dello stato di collapsed
		if (m_LayoutType == HBOX)
		{
			pElem->SetFlex(1, FALSE);
			break;
		}
		
		// per il restante invece agisco solo sulle
		// non collassate perche' le collapsed in
		// verticale sono alte solo come il titolo (se c'e')
		if (!pTile->IsCollapsed() && nLastContainer < 0)
		{
			pElem->SetFlex(1, FALSE);
			break;
		}
	}
}
	
//------------------------------------------------------------------------------
void CLayoutContainer::PrepareVBoxLayout(Layout* pLayout, CRect& rectContainer)
{
	int spacing	= m_pTileStyle->GetTileSpacing();

	int totalFlex = 0;
	int totalNonFlexHeight = 0;

	for (int i = 0; i <= m_Elements.GetUpperBound(); i++)
	{
		m_Elements.GetAt(i)->SetRequestedLastFlex(AUTO);

		if (!m_Elements.GetAt(i)->IsVisible())
			continue;

		LayoutInfo* pInfo = new LayoutInfo(m_Elements.GetAt(i));

		if (pInfo->m_pElem->IsFlex(HEIGHT))
			totalFlex += pInfo->m_pElem->GetFlex(HEIGHT);
		else
			totalNonFlexHeight += pInfo->m_pElem->GetRequiredHeight(rectContainer);

		int nWidth = pInfo->m_pElem->GetRequiredWidth(rectContainer);
		pLayout->maxElemWidth = max(pLayout->maxElemWidth, nWidth);
		pLayout->info.Add(pInfo);
	}

	if (pLayout->info.GetSize() == 0)
		return;

	if (GetRequestedLastFlex() != AUTO && totalFlex == 0)
	{
		for (int i = pLayout->info.GetUpperBound(); i >= 0 ; i--)
		{
			LayoutElement* pElem = pLayout->info[i]->m_pElem;
			if (pLayout->info[i]->m_pElem->CanDoLastFlex(HEIGHT))
			{
				pLayout->info[i]->m_pElem->SetRequestedLastFlex(GetRequestedLastFlex());
				totalFlex += pElem->GetFlex(HEIGHT);
				totalNonFlexHeight -= pElem->GetRequiredHeight(rectContainer);
				break;
			}
		}
	}

	int totalFlexHeight = rectContainer.Height() - (spacing * (pLayout->info.GetSize() - 1)) - totalNonFlexHeight;
	int totalUnflexedHeight  = 0;
	int totalUnflexed = 0;

	int currTopRow = 0;

	CRect rectLeft(rectContainer);
	for (int i = 0; i <= pLayout->info.GetUpperBound(); i++)
	{
		rectLeft.top = currTopRow;

		LayoutElement* pElem = pLayout->info[i]->m_pElem;

		int elemHeight;
		if ((pElem->IsFlex(HEIGHT)))
		{
			elemHeight = (int)(( (totalFlexHeight * 1.0) / totalFlex) * pElem->GetFlex(HEIGHT));
			if (totalUnflexed > 0)
				elemHeight -= max(0, (int)(((totalUnflexedHeight * 1.0) / (totalFlex - totalUnflexed)) * pElem->GetFlex(HEIGHT)));

			int nMinHeight = pElem->GetMinHeight(rectLeft);
			if (elemHeight < nMinHeight)
			{
				totalUnflexedHeight += (nMinHeight - elemHeight);
				totalUnflexed += pElem->GetFlex(HEIGHT);
				elemHeight = nMinHeight;
			}
		}
		else
			elemHeight	= pElem->GetRequiredHeight(rectLeft);

		int elemWidth	= pElem->GetRequiredWidth(rectLeft);

		// The layout is prepared without taking in count horizontal alignment, that is, every element is just placed at the begin of the container
		// The main preparation goal is to calculate sizes, the alignement will be applied later
		pLayout->info[i]->m_Rect = CRect(rectContainer.left, rectContainer.top + currTopRow, rectContainer.left + elemWidth, rectContainer.top + currTopRow + elemHeight);

		currTopRow += elemHeight + spacing;
	}

	for (int i = 0; i <= pLayout->info.GetUpperBound(); i++)
	{
		LayoutInfo* pInfo = pLayout->info[i];
		pLayout->rectRequired.UnionRect(pLayout->rectRequired, pLayout->info[i]->m_Rect);
	}
}

//------------------------------------------------------------------------------
void CLayoutContainer::DoVBoxLayout(CRect& rectContainer)
{
	Layout layout;

	PrepareVBoxLayout(&layout, rectContainer);
	
	if (layout.info.GetSize() == 0)
		return;

	// apply alignement before performing the layout
	for (int i = 0; i <= layout.info.GetUpperBound(); i++)
	{
		int left = 0;
		int right = 0;
		switch (m_LayoutAlign)
		{
			case STRETCH:
				left = 0;
				right = rectContainer.Width();
			break;

			case STRETCHMAX:
				left = 0;
				right = layout.maxElemWidth;
			break;
			
			case BEGIN:
				left = 0;
				right = layout.info[i]->m_Rect.Width();
			break;

			case END:
				left = rectContainer.Width() - layout.info[i]->m_Rect.Width();
				right = rectContainer.Width();
			break;

			case MIDDLE:
				left = (rectContainer.Width() - layout.info[i]->m_Rect.Width()) / 2;
				right = left + layout.info[i]->m_Rect.Width();
			break;
			case NO_ALIGN:
				left = 0;
				right = layout.info[i]->m_Rect.right;
				break;

		}

		layout.info[i]->m_Rect.left		= rectContainer.left + left;
		layout.info[i]->m_Rect.right	= rectContainer.left + right;

		layout.info[i]->m_pElem->Relayout(layout.info[i]->m_Rect);
	}
}

//------------------------------------------------------------------------------
void CLayoutContainer::PrepareColumnLayout(Layout* pLayout, CRect& rectContainer)
{
	int spacing	= m_pTileStyle->GetTileSpacing();

	LayoutInfoArray bottom; 

	int currTopRow = 0;
	int currBottomRow = 0;
	int currRightColumn = 0;
	int lowest = 0;
	int firstTileOnRow = 0;
	int currRow = 0;

	for (int i = 0; i <= m_Elements.GetUpperBound(); i++)
	{
		m_Elements.GetAt(i)->SetRequestedLastFlex(AUTO);

		if (!m_Elements.GetAt(i)->IsVisible())
			continue;

		LayoutInfo* pInfo = new LayoutInfo(m_Elements.GetAt(i));
		//pInfo->m_Rect.bottom = m_Elements.GetAt(i)->GetRequiredHeight(CRect(0,0,0,0));
		//pInfo->m_Rect.right = m_Elements.GetAt(i)->GetRequiredWidth(CRect(0,0,0,0));
		pInfo->m_Rect.bottom = m_Elements.GetAt(i)->GetMinHeight(rectContainer);
		pInfo->m_Rect.right = m_Elements.GetAt(i)->GetRequiredWidth(rectContainer);

		// new element does not fit, but must not be the very first (given size too small)
		if (pLayout->info.GetSize() > 0 && currRightColumn + pInfo->m_Rect.Width() > rectContainer.Width())
		{
			if (m_LayoutAlign == STRETCH || m_LayoutAlign == STRETCHMAX)
			{
				pLayout->InflateTiles(firstTileOnRow, rectContainer);
				firstTileOnRow = pLayout->info.GetUpperBound() + 1;
			}

			// move below the higher element already placed
			currTopRow = currBottomRow + spacing;
			// move on the leftmost available column, maybe 0
			currRightColumn = pLayout->LeftmostAvailableColumn(currBottomRow);

			// if too large for this place reset its position
			if (currRightColumn > 0 && currRightColumn + spacing + pInfo->m_Rect.Width() > rectContainer.Width())
			{
				currTopRow = lowest + spacing;
				currRightColumn = 0;
			}

			//if not at 0, take in count the inter-element height
			if (currRightColumn > 0) 
			{
				if (m_LayoutAlign != STRETCH && m_LayoutAlign != STRETCHMAX)
					for (int b = 0; b <= bottom.GetUpperBound(); b++)
						if (bottom[b]->m_Rect.right == currRightColumn)
						{
							for (int bb = b + 1; bb <= bottom.GetUpperBound(); bb++)
								bottom[bb]->m_Rect.bottom = lowest;

							break;
						}

				currRightColumn += spacing;
			}

			currRow++;
		}
		pInfo->m_nRow = currRow;

		// if restarted at the beginning of the container, stretch all the lowest element
		// of the "line"
		if (currRightColumn == 0 && (m_LayoutAlign == STRETCH || m_LayoutAlign == STRETCHMAX))
		{
			for (int b = 0; b <= bottom.GetUpperBound(); b++)
				//if (bottom[b]->m_Rect.bottom < lowest)
				{
					bottom[b]->m_pElem->SetRequestedLastFlex(1);
					bottom[b]->m_Rect.bottom = lowest;
				}
			lowest = 0;
			bottom.RemoveAll();
		}

		// place the element, then move right
		pInfo->m_Rect.MoveToXY(currRightColumn, currTopRow);
		currRightColumn += pInfo->m_Rect.Width() + spacing;
		currBottomRow = pInfo->m_Rect.bottom;
		lowest = max(lowest, currBottomRow);

		// if the new placed element is below the previous ones (that is guessed because it is left of them), 
		// they are no more "bottom elements" of the current "line"
		while (bottom.GetSize() > 0 && bottom[bottom.GetUpperBound()]->m_Rect.left >= pInfo->m_Rect.left)
			bottom.RemoveAt(bottom.GetUpperBound());
		bottom.Add(pInfo);

		pLayout->info.Add(pInfo);
	}

	// apply the stretch to the latest bottom elements
	if (m_LayoutAlign == STRETCH || m_LayoutAlign == STRETCHMAX)
	{
		pLayout->InflateTiles(firstTileOnRow, rectContainer);
		for (int b = 0; b <= bottom.GetUpperBound(); b++)
			//if (bottom[b]->m_Rect.bottom < lowest)
			{
				bottom[b]->m_pElem->SetRequestedLastFlex(1);
				bottom[b]->m_Rect.bottom = lowest;
				bottom[b]->m_bIsBottom = TRUE;
			}
	}

	for (int i = 0; i <= pLayout->info.GetUpperBound(); i++)
	{
		LayoutInfo* pInfo = pLayout->info[i];
		pLayout->rectRequired.UnionRect(pLayout->rectRequired, pLayout->info[i]->m_Rect);
	}
}

//------------------------------------------------------------------------------
void CLayoutContainer::DoColumnLayout(CRect& rectContainer)
{
	Layout layout;

	PrepareColumnLayout(&layout, rectContainer);

	if (layout.info.GetSize() == 0)
		return;

	int lastRow = -1;
	for (int i = 0; i <= layout.info.GetUpperBound(); i++)
	{
		LayoutInfo* pInfo = layout.info[i];
		CRect rectNew(pInfo->m_Rect);
	
		rectNew.OffsetRect(rectContainer.left, rectContainer.top);

		if (GetRequestedLastFlex() != AUTO && pInfo->m_bIsBottom)
			rectNew.bottom = rectContainer.bottom;

		pInfo->m_pElem->Relayout(rectNew);
	}
}

//-----------------------------------------------------------------------------
void CLayoutContainer::GetAvailableRect(CRect &rectAvail)
{
	m_pParentElement->GetAvailableRect(rectAvail);
}

//-----------------------------------------------------------------------------
void CLayoutContainer::GetUsedRect(CRect &rectUsed)
{
	for (int e = 0; e <= m_Elements.GetUpperBound(); e++)
	{
		if (!m_Elements.GetAt(e)->IsVisible())
			continue;

		CRect rectElem;
		m_Elements.GetAt(e)->GetUsedRect(rectElem);
		rectUsed.UnionRect(rectUsed, rectElem);
	}	
	// Nello STRIPE layout la prima tile inizia <TileSpacing> pixel piu` in basso
	// Sommo quindi il TileSpacing su asse y, perche' lo spazio in alto, tra il contenitore e la prima tile dialog, 
	// non e' considerato nell'unione dei rettangoli delle singole tileDialog
	if (m_LayoutType == STRIPE)
		rectUsed.bottom += AfxGetThemeManager()->GetTileSpacing();
}

//-----------------------------------------------------------------------------
int CLayoutContainer::GetMinWidth()
{
	switch (m_LayoutType)
	{
		case HBOX:
		{
			int minWidth = 0;
			for (int e = 0; e <= m_Elements.GetUpperBound(); e++)
			{
				if (!m_Elements.GetAt(e)->IsVisible())
					continue;
				minWidth += m_Elements.GetAt(e)->GetMinWidth() + m_pTileStyle->GetTileSpacing();
			}
			minWidth = max(0, minWidth - m_pTileStyle->GetTileSpacing());

			return minWidth;
		}
		break;
		
		case VBOX:
		{
			int minWidth = 0;
			for (int e = 0; e <= m_Elements.GetUpperBound(); e++)
			{
				if (!m_Elements.GetAt(e)->IsVisible())
					continue;
				minWidth = max(minWidth, m_Elements.GetAt(e)->GetMinWidth());
			}
			return minWidth;
		}
		break;

		case COLUMN:
		{
			return 0; //@@@TODO migliorare
		}
		break;

		default:
		{
			ASSERT_TRACE(FALSE,"Layout not yet supported!\n");
			return 0;
		}
	}	
}

//-----------------------------------------------------------------------------
int CLayoutContainer::GetMinHeight(CRect& rect /*= CRect(0, 0, 0, 0)*/)
{
	if (m_nMinHeight > 0)
		return m_nMinHeight;

	switch (m_LayoutType)
	{
		case HBOX:
		{
			int minHeight = 0;
			for (int e = 0; e <= m_Elements.GetUpperBound(); e++)
			{
				if (!m_Elements.GetAt(e)->IsVisible())
					continue;
				int nHeight = m_Elements.GetAt(e)->GetMinHeight(rect);
				minHeight = max(minHeight, nHeight);
			}
			return minHeight;
		}
		break;
		
		case VBOX:
		{
			int minHeight = 0;
			for (int e = 0; e <= m_Elements.GetUpperBound(); e++)
			{
				if (!m_Elements.GetAt(e)->IsVisible())
					continue;
				minHeight += m_Elements.GetAt(e)->GetMinHeight(rect) + m_pTileStyle->GetTileSpacing();
			}
			minHeight = max(0, minHeight - m_pTileStyle->GetTileSpacing());

			return minHeight;
		}
		break;
		
		case COLUMN:
		case STRIPE:
		{
			Layout layout;
			PrepareColumnLayout(&layout, rect);
			return layout.rectRequired.Height();
		}
		break;

		default:
		{
			ASSERT_TRACE(FALSE,"Layout not yet supported!\n");
			return 0;
		}
	}	
}

//-----------------------------------------------------------------------------
int CLayoutContainer::GetRequiredHeight(CRect &rectAvail)
{ 
	switch (m_LayoutType)
	{
		case HBOX:
		{
			Layout layout;

			PrepareHBoxLayout(&layout, rectAvail);
			return max(m_nMinHeight, layout.rectRequired.Height());
		}
		break;
		
		case VBOX:
		{
			int reqHeight = 0;
			BOOL bSomeFlex = FALSE;
			for (int e = 0; e <= m_Elements.GetUpperBound(); e++)
			{
				if (!m_Elements.GetAt(e)->IsVisible())
					continue;

				if (m_Elements.GetAt(e)->IsFlex(HEIGHT))
					// if the element is flex, VBOX can resize its height, until the minimum
					reqHeight += m_Elements.GetAt(e)->GetMinHeight(rectAvail);
				else
					// non-flex items will be stacked
					reqHeight += m_Elements.GetAt(e)->GetRequiredHeight(rectAvail);

				// spacing will occur on both flex and non-flex items
				reqHeight += m_pTileStyle->GetTileSpacing();
			}
			// only inter-element spacing has to be counted, remove last spacing
			reqHeight = max(0, reqHeight - m_pTileStyle->GetTileSpacing());

			return reqHeight;
		}
		break;

		case COLUMN:
		{
			Layout layout;
			PrepareColumnLayout(&layout, rectAvail);
			return max(m_nMinHeight, layout.rectRequired.Height());
		}
		break;

		default:
		{
			ASSERT_TRACE(FALSE,"Layout not yet supported!\n");
			return 0;
		}
	}	
}

//-----------------------------------------------------------------------------
int CLayoutContainer::GetRequiredWidth(CRect &rectAvail)
{ 
	switch (m_LayoutType)
	{
		case HBOX:
		{
			int reqWidth = 0;
			BOOL bSomeFlex = FALSE;
			for (int e = 0; e <= m_Elements.GetUpperBound(); e++)
			{
				if (!m_Elements.GetAt(e)->IsVisible())
					continue;

				if (m_Elements.GetAt(e)->IsFlex(WIDTH))
					// if the element is flex, HBOX can resize its width, until the minimum
					reqWidth += m_Elements.GetAt(e)->GetMinWidth();
				else
					// non-flex items will be juxtaposed
					reqWidth += m_Elements.GetAt(e)->GetRequiredWidth(CRect(0,0,0,0));
				
				// spacing will occur on both flex and non-flex items
				reqWidth += m_pTileStyle->GetTileSpacing();
			}
			// only inter-element spacing has to be counted, remove last spacing
			reqWidth = max(0, reqWidth - m_pTileStyle->GetTileSpacing());

			return reqWidth;
		}
		break;
		
		case VBOX:
		{
			// VBOX does not care about its width, just find out the widest element in it
			int reqWidth = 0;
			for (int e = 0; e <= m_Elements.GetUpperBound(); e++)
			{
				if (!m_Elements.GetAt(e)->IsVisible())
					continue;
				int nWidth =  m_Elements.GetAt(e)->GetRequiredWidth(rectAvail);//(CRect(0,0,0,0));
				reqWidth = max(reqWidth, nWidth);
			}
			return reqWidth;
		}
		break;

		case COLUMN:
		case STRIPE:
		{
			Layout layout;

			PrepareColumnLayout(&layout, rectAvail);
			if (m_nMaxWidth > 0)
				return min(m_nMaxWidth, layout.rectRequired.Width());
			else
				// FREE, AUTO or ORIGINAL
				return layout.rectRequired.Width();
		}
		break;

		default:
		{
			ASSERT_TRACE(FALSE,"Layout not yet supported!\n");
			return 0;
		}
	}	
}

//=============================================================================
// STRIPE Layout
//=============================================================================

//-----------------------------------------------------------------------------
void CLayoutContainer::ResizeRightElements(LayoutElementArray& rightElements, const CRect& rcGroup, HDWP  hDWP)
{
	BOOL bFillEmptySpace = m_pParentElement->IsFillEmptySpaceMode();
	
	//Fill to the right, matching the parent container
	if (rightElements.GetCount() > 0)
	{
		for (int i = 0; i <= rightElements.GetUpperBound(); i++)
		{
			LayoutElement* pElem = rightElements.GetAt(i);
			CBaseTileDialog* pTileDialog = pElem->GetTileDialog();
			if (bFillEmptySpace || pTileDialog->IsAutoFill())
			{
				//TODOLUCA
				CRect rcTile;
				pTileDialog->GetWindowRect(&rcTile);
				pTileDialog->ScreenToClient(&rcTile);
				pTileDialog->SetWindowPos(NULL,0,0, rcGroup.right - rcTile.left, rcTile.Height(), SWP_NOMOVE | SWP_NOZORDER);
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CLayoutContainer::ResizeBottomElements(int bottomLine, LayoutElementArray& bottomElements, const CRect& rcGroup, HDWP hDWP)
{
	CWnd* pWndOwner = m_pParentElement->GetCWnd();
	if (!pWndOwner)
		return;

	CBaseTileDialog* pLastRightTile = NULL;
	int nMaxRight = 0;
	for (int e = 0; e <= bottomElements.GetUpperBound(); e++)
	{
		CBaseTileDialog*	pBottomTile = bottomElements.GetAt(e)->GetTileDialog();
		if (!pBottomTile)
			continue;

		CRect rcBottomTile;
		pBottomTile->GetWindowRect(&rcBottomTile);
		pWndOwner->ScreenToClient(&rcBottomTile);

		// I save max right tile
		if (rcBottomTile.right > nMaxRight)
		{
			pLastRightTile = pBottomTile;
			nMaxRight = rcBottomTile.right;
		}

		//if Tile is collapsed no need to resize its bottom edge
		if (pBottomTile->IsCollapsed())
			continue;

		int bottomEdge = pBottomTile->IsAutoFill() ? rcGroup.bottom : bottomLine;
		if (bottomEdge <= rcBottomTile.bottom)
			continue;
					
		int newTileHeight = bottomEdge - rcBottomTile.top;

		pBottomTile->SetWindowPos(NULL,0,0,rcBottomTile.Width(), newTileHeight, SWP_NOMOVE | SWP_NOZORDER);
		pBottomTile->ResizeStaticAreaHeight(newTileHeight);
	}

	// last on the right have to fill group right side
	if (pLastRightTile && pLastRightTile->IsAutoFill() && !pLastRightTile->IsCollapsed() && pLastRightTile->IsVisible())
	{
		CRect rcTile;
		pLastRightTile->GetWindowRect(&rcTile);
		pLastRightTile->ScreenToClient(&rcTile);
		pLastRightTile->SetWindowPos(NULL,0,0, rcGroup.right - rcTile.left, rcTile.Height(), SWP_NOMOVE | SWP_NOZORDER);
	}
}

//------------------------------------------------------------------------------
int CLayoutContainer::CalculateRowBottom(Layout* pLayout, int nRowIndex)
{
	int maxHeight = 0;
	for (int i = 0; i <= pLayout->info.GetUpperBound(); i++)
	{
		LayoutInfo* pInfo = (LayoutInfo*) pLayout->info.GetAt(i);
		// scelgo la prima della mia riga
		if	(pInfo->m_nRow == nRowIndex)
			maxHeight = max(maxHeight, pInfo->m_Rect.bottom);
	}
	return maxHeight;
}

//------------------------------------------------------------------------------
void CLayoutContainer::AlignTilesOnTheSameRowToBottomRow(Layout* pLayout, BOOL bFillEmptySpace)
{
	for (int i = 0; i <= pLayout->info.GetUpperBound(); i++)
	{
		LayoutInfo* pInfo = (LayoutInfo*) pLayout->info.GetAt(i);

		// all elements in a STRIPE layout must be TileDialogs
		ASSERT(pInfo->m_pElem->GetTileDialog());
		if (pInfo->m_pElem->GetTileDialog())
			if (bFillEmptySpace || pInfo->m_pElem->GetTileDialog()->IsAutoFill())
			{
				int bottom = CalculateRowBottom(pLayout, pInfo->m_nRow);
				pInfo->m_Rect.bottom = bottom;
				//Adattamento area statica
				pInfo->m_pElem->GetTileDialog()->ResizeStaticAreaHeight(bottom);
			}
	}
}

//------------------------------------------------------------------------------
void CLayoutContainer::StretchHorizontal(Layout* pLayout, BOOL bFillEmptySpace, CRect &rectContainer, int tileSpacing)
{
	int currentRow = 0;
	int tilesPerRow = 0;
	//Modifica i rettangoli delle tile a destra
	for (int i = 0; i <= pLayout->info.GetUpperBound(); i++)
	{
		LayoutInfo* pInfo = (LayoutInfo*) pLayout->info.GetAt(i);
		tilesPerRow++;
		
		if (!pInfo->m_bIsLastRight)
			continue;

		// lavoriamo solo sull'ultima tile della riga
		int availableEmptySpace = rectContainer.right - pInfo->m_Rect.right;
		currentRow = pInfo->m_nRow;
		int spaceAvailableForEachTile = (int)(availableEmptySpace / tilesPerRow);
		tilesPerRow = 0;
		int candidateLeft = -1;
		
		for (int j = 0; j <= pLayout->info.GetUpperBound(); j++)
		{
			LayoutInfo* pCurrentInfo = (LayoutInfo*) pLayout->info.GetAt(j);
			if (pInfo->m_nRow == pCurrentInfo->m_nRow)
			{
				int originalWidth = pCurrentInfo->m_Rect.Width();

				if (candidateLeft != -1)
				{
					pCurrentInfo->m_Rect.left = candidateLeft;
				}
				pCurrentInfo->m_Rect.right = pCurrentInfo->m_Rect.left + originalWidth + spaceAvailableForEachTile;
				candidateLeft = pCurrentInfo->m_Rect.right + tileSpacing;				
			}

		}
	}
}

//------------------------------------------------------------------------------
void CLayoutContainer::AlignLastBottomTilesToBottomOfContainer(Layout* pLayout, BOOL bFillEmptySpace, CRect &rectContainer)
{
	//cerco maxRow
	int lastRow = -1;
	for (int i = 0; i <= pLayout->info.GetUpperBound(); i++)
	{
		LayoutInfo* pInfo = (LayoutInfo*) pLayout->info.GetAt(i);
		lastRow = max(lastRow, pInfo->m_nRow);	
	}

	//Modifica i rettangoli delle tile in basso
	for (int i = 0; i <= pLayout->info.GetUpperBound(); i++)
	{
		LayoutInfo* pInfo = (LayoutInfo*) pLayout->info.GetAt(i);
		// all elements in a STRIPE layout must be TileDialogs
		if (pInfo->m_pElem->GetTileDialog())
			if	( 
						pInfo->m_nRow == lastRow										//se sono su ultima riga
					&& pInfo->m_pElem->IsVisible() 
					&&	pInfo->m_Rect.bottom < rectContainer.bottom			//se c'e' spazio in basso da riempire
					&&	(bFillEmptySpace || pInfo->m_pElem->GetTileDialog()->IsAutoFill())		//se devo riempire lo spazio vuoto
				)
			{
				pInfo->m_Rect.bottom = rectContainer.bottom;
				pInfo->m_pElem->GetTileDialog()->ResizeStaticAreaHeight(rectContainer.bottom);
			}
	}
}
/////////////// APOGEO STRIPE LAYOUT ALGORITHM ///////////////

//------------------------------------------------------------------------------
void CLayoutContainer::DoStripeLayout(CRect& rectContainer, HDWP hDWP)
{
	if (m_Elements.GetCount() < 1)
	{
		return;
	}
	int tileSpacing = AfxGetThemeManager()->GetTileSpacing();
	LayoutInfo*		pTileInfo = NULL;
	LayoutElement*		pCurrElem = NULL;
	CBaseTileDialog*	pTileDialog;

	int nWidthVScrollSize = 0;
	CScrollView* pScrollView = GetParentScrollView();
	if (pScrollView)
	{
		// if vertical scroll is not visible, algorithm assume a space in case it will be shown later,
		// to avoid in borderline case (a Tile has enough space to lay on a Row without scrollbar, but not with scrollbar)
		// a replacement on a new row of the last Tile (bug 21705 PAI)
		BOOL bHasVerticalScroll = FALSE, bHasHorizontalScroll = FALSE;
		pScrollView->CheckScrollBars(bHasHorizontalScroll, bHasVerticalScroll);
		if (!bHasVerticalScroll)
		{
			nWidthVScrollSize += GetSystemMetrics(SM_CXVSCROLL);
		}
	}

	int currentTileLeft = tileSpacing;
	int currentTileTop = tileSpacing;
	int nLastRowTop = -1;

	int nRowIndex = 0;

	StripeManager	arTiles;

	//Definizione dei rect candidati
	for (int i = 0; i <= m_Elements.GetUpperBound(); i++)
	{
		pCurrElem = m_Elements.GetAt(i);

		if (!pCurrElem->IsVisible())
			continue;

		pTileDialog = pCurrElem->GetTileDialog();


		//E' su una nuova riga se ha il bool impostato, oppure se non ci sta nella riga corrente 
		int requiredWidth = pCurrElem->GetRequiredWidth(rectContainer);
		BOOL bIsNewLine = (pTileDialog && pTileDialog->m_bHasToLayInNewLine) || (currentTileLeft + requiredWidth > (rectContainer.right - nWidthVScrollSize));

		if (bIsNewLine)
		{
			//se vado a capo vuol dire che quello precedente e' l'utima a dx
			int upperBound = arTiles.GetUpperBound();
			if (upperBound >= 0)
			{
				LayoutInfo* pPrevTileInfo = (LayoutInfo*)arTiles.GetAt(upperBound);
				pPrevTileInfo->m_bIsLastRight = TRUE;
			}

			currentTileLeft = tileSpacing;
			currentTileTop = arTiles.CalculateRowBottom(nRowIndex) + tileSpacing;
			nRowIndex++;
		}

		// Primo tentativo di posizionamento
		pTileInfo = new LayoutInfo(pCurrElem->GetTileDialog() ? pCurrElem->GetTileDialog() : pCurrElem);
		pTileInfo->m_Rect.left = currentTileLeft;
		pTileInfo->m_Rect.right = pTileInfo->m_Rect.left + pCurrElem->GetRequiredWidth(rectContainer);
		pTileInfo->m_Rect.top = currentTileTop;
		pTileInfo->m_Rect.bottom = pTileInfo->m_Rect.top + pCurrElem->GetRequiredHeight(rectContainer);
		pTileInfo->m_nRow = nRowIndex;
		arTiles.Add(pTileInfo);

		currentTileLeft = (pTileInfo->m_Rect.right + tileSpacing);
	}
	//ultima tile in assoluto e' anche ultima a destra della sua riga
	pTileInfo->m_bIsLastRight = TRUE;

	// Riempimento degli spazi
	BOOL bFillEmptySpace = m_pParentElement->IsFillEmptySpaceMode();

	arTiles.FinalAlignmentAndStretch(bFillEmptySpace, rectContainer, tileSpacing);

	m_LastInvalidatedRect = CRect(0, 0, 0, 0);
	// vero e proprio disegno
	for (int i = 0; i <= arTiles.GetUpperBound(); i++)
	{
		LayoutInfo* pTileInfo = (LayoutInfo*)arTiles.GetAt(i);
		if (pTileInfo->GetTileDialog())
			pTileInfo->GetTileDialog()->Relayout(pTileInfo->m_Rect, hDWP);
		m_LastInvalidatedRect.UnionRect(m_LastInvalidatedRect, pTileInfo->m_Rect);
	}
	// Aggiunta eventuali scroll
	AdjustScrollSize(rectContainer);
}

//------------------------------------------------------------------------------
CScrollView* CLayoutContainer::GetParentScrollView()
{
	if (m_pParentScrollView)
		return m_pParentScrollView;
	
	CWnd* pParentView = m_pParentElement->GetCWnd();
	if (!pParentView)
		return NULL;

	//Risale fino alla scrollview
	while (pParentView != NULL && !pParentView->IsKindOf(RUNTIME_CLASS(CScrollView)))
	{
		pParentView = pParentView->GetParent();
	}
	if (pParentView != NULL)
	{
		m_pParentScrollView = (CScrollView*)pParentView;
		return m_pParentScrollView;
	}
	return NULL;
}

//------------------------------------------------------------------------------
void CLayoutContainer::AdjustScrollSize(CRect& rectContainer)
{
	CWnd* pWndOwner = m_pParentElement->GetCWnd();
	if (!pWndOwner)
		return;
	if (!pWndOwner->IsWindowVisible())
		return;
		
	CScrollView* pScrollView = GetParentScrollView();

	if (pScrollView != NULL)
	{
		CRect rectScrollView;
		pScrollView->GetWindowRect(&rectScrollView);
		
		//Ciclo sui figli di primo della view per capire di che spazio necessita
		CWnd* pCtrl = pScrollView->GetWindow(GW_CHILD);
		CRect rectCtrl, rectTotal;
		while (pCtrl)
		{	
			if (pCtrl->IsWindowVisible())
			{
				if (pCtrl->IsKindOf(RUNTIME_CLASS(CBaseTabManager)))
				{
					CBaseTabManager* pTabManager = (CBaseTabManager*)pCtrl;
					pTabManager->GetUsedRect(rectCtrl);
				}
				else if (pCtrl->IsKindOf(RUNTIME_CLASS(CBaseTileGroup)))
				{
					CBaseTileGroup* pTileGroup = (CBaseTileGroup*)pCtrl;
					pTileGroup->GetUsedRect(rectCtrl);
				}
				else
				{
					pCtrl->GetWindowRect(&rectCtrl);	
				}
				rectTotal.UnionRect(rectTotal, rectCtrl);
			}
			pCtrl = pCtrl->GetNextWindow();
		}
		
		pScrollView->ScreenToClient(&rectTotal);
			
		SIZE sz;
		sz.cx = m_LastInvalidatedRect.Width();
		sz.cy = rectTotal.Height();
		pScrollView->SetScrollSizes(MM_TEXT, sz);
	}
}

//------------------------------------------------------------------------------
void CLayoutContainer::AddOwnedContainer(CLayoutContainer* pContainer)
{
	AddChildElement(pContainer);
	m_OwnedContainers.Add(pContainer);
}


CWndLayoutContainerDescription* CLayoutContainer::GetDescription(CWndObjDescriptionContainer* pContainer)
{
	CString strId = cwsprintf(_T("%d"), this);
	CWndLayoutContainerDescription* pDesc = (CWndLayoutContainerDescription*)pContainer->GetWindowDescription(NULL, RUNTIME_CLASS(CWndLayoutContainerDescription), strId);
	pDesc->UpdateLayoutAttributes(this);
	
	return pDesc;
}


//==============================================================================
IMPLEMENT_DYNCREATE(CWndLayoutContainerDescription, CWndPanelDescription)
IMPLEMENT_DYNCREATE(CWndLayoutStatusContainerDescription, CWndLayoutContainerDescription)
REGISTER_WND_OBJ_CLASS(CWndLayoutContainerDescription, TileGroup)
REGISTER_WND_OBJ_CLASS(CWndLayoutContainerDescription, LayoutContainer)
REGISTER_WND_OBJ_CLASS(CWndLayoutContainerDescription, TilePanel)
REGISTER_WND_OBJ_CLASS(CWndLayoutStatusContainerDescription, StatusTilePanel)


//-----------------------------------------------------------------------------
void CWndLayoutContainerDescription::SerializeJson(CJsonSerializer& strJson)
{
	__super::SerializeJson(strJson);
	SERIALIZE_ENUM(m_LayoutType, szJsonLayoutType, CLayoutContainer::COLUMN);
	SERIALIZE_ENUM(m_LayoutAlign, szJsonLayoutAlign, CLayoutContainer::STRETCH);
	SERIALIZE_ENUM(m_Style, szJsonTileStyle, TileDialogStyle::TDS_NONE);
	SERIALIZE_STRING(m_strIcon, szJsonIcon);
	SERIALIZE_ENUM(m_IconType, szJsonIconType, CWndObjDescription::IconTypes::IMG);
	SERIALIZE_BOOL3(m_bIsCollapsible, szJsonCollapsible);
	SERIALIZE_BOOL(m_bIsCollapsed, szJsonCollapsed, false);
	SERIALIZE_BOOL(m_bShowAsTile, szJsonShowAsTile, false);
	SERIALIZE_BOOL(m_bManageUnpinned, szJsonHasPinnableTiles, false);
	SERIALIZE_BOOL(m_bOwnsPane, szJsonOwnsPane, true);
	SERIALIZE_INT(m_nFlex, szJsonFlex, -1);
}
//-----------------------------------------------------------------------------
void CWndLayoutContainerDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	PARSE_ENUM(m_LayoutType, szJsonLayoutType, CLayoutContainer::LayoutType);
	PARSE_ENUM(m_LayoutAlign, szJsonLayoutAlign, CLayoutContainer::LayoutAlign);
	PARSE_ENUM(m_Style, szJsonTileStyle, TileDialogStyle);
	PARSE_STRING(m_strIcon, szJsonIcon);
	PARSE_ENUM(m_IconType, szJsonIconType, CWndObjDescription::IconTypes);
	PARSE_BOOL3(m_bIsCollapsible, szJsonCollapsible);
	PARSE_BOOL(m_bIsCollapsed, szJsonCollapsed);
	PARSE_BOOL(m_bShowAsTile, szJsonShowAsTile);
	PARSE_BOOL(m_bManageUnpinned, szJsonHasPinnableTiles);
	PARSE_BOOL(m_bOwnsPane, szJsonOwnsPane);
	PARSE_INT(m_nFlex, szJsonFlex);
}

//-----------------------------------------------------------------------------
void CWndLayoutContainerDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	CWndLayoutContainerDescription* pLayoutDesc = dynamic_cast<CWndLayoutContainerDescription*>(pDesc);
	if (pLayoutDesc)
	{
		m_LayoutType = pLayoutDesc->m_LayoutType;
		m_LayoutAlign = pLayoutDesc->m_LayoutAlign;
		m_strIcon = pLayoutDesc->m_strIcon;
		m_IconType = pLayoutDesc->m_IconType;
		m_bIsCollapsible = pLayoutDesc->m_bIsCollapsible;
		m_bIsCollapsed = pLayoutDesc->m_bIsCollapsed;
		m_bShowAsTile = pLayoutDesc->m_bShowAsTile;
		m_bManageUnpinned = pLayoutDesc->m_bManageUnpinned;
		m_bOwnsPane = pLayoutDesc->m_bOwnsPane;
		m_Style = pLayoutDesc->m_Style;
		m_nFlex = pLayoutDesc->m_nFlex;
	}
}


//-----------------------------------------------------------------------------
void CWndLayoutContainerDescription::AddChildWindows(CWnd* pWnd)
{
	if (!pWnd) {
		return;
	}
	CWnd* pChild = pWnd->GetWindow(GW_CHILD);
	while (pChild)
	{	
		if (!SkipWindow(pChild))
		{
			LayoutElement* pLayoutElement = dynamic_cast<LayoutElement*>(pChild);
			ASSERT(pLayoutElement);
			if (pLayoutElement)
			{
				//controllo il parent LayoutContainer del LayoutElement
				// Potrebbe essere un cwnd, es. Tilegroup, oppure un LayoutContainer??? Verificare, potrebbe mancare un controllo
				CLayoutContainer* pParentLayoutContainer = dynamic_cast<CLayoutContainer*>(pLayoutElement->GetParentElement());
				if (!pParentLayoutContainer)
				{
					ASSERT(FALSE);
				}
				else
				{
					CWndLayoutContainerDescription* pLayoutcontainerDesc = this->GetChildLayoutContainer(pParentLayoutContainer);
					CWndObjDescription* pCandidateChildDescription = (CWndObjDescription*)pChild->SendMessage(UM_GET_CONTROL_DESCRIPTION, (WPARAM)&(pLayoutcontainerDesc->m_Children), NULL);
					if (!pCandidateChildDescription)
					{
						ASSERT(FALSE); //per i layout element abbiamo reimplementato il metodo OnGetControlDescription, non sono controlli base (Label, static ecc..)
					}
				}
			}
			else 
			{
				ASSERT(FALSE); //i figli di un layout container devono essere layout element
			}
		}
		pChild = pChild->GetWindow(GW_HWNDNEXT);
	}
}

//Metodo che dato un puntatore a un CLayoutContainer (nota bene, non CWnd!), crea o restituisce(se gia' creata) la sua descrizione.
//-----------------------------------------------------------------------------
CWndLayoutContainerDescription* CWndLayoutContainerDescription::GetChildLayoutContainer(CLayoutContainer* pLayoutContainer)
{
	if (!pLayoutContainer)
		return NULL;

	CString strId = cwsprintf(_T("%d"), pLayoutContainer);

	//Il layout container padre della cwnd, e' questa descrizione stessa, non ci sono layout container in mezzo
	if (HasID(strId))
	{
		return this;
	}

	CWndLayoutContainerDescription* pDesc = (CWndLayoutContainerDescription*)pLayoutContainer->GetDescription(&m_Children);
	if (pDesc)
	{
		return pDesc;
	}
	return NULL;
}

//Aggiorna gli attributi che sono cambiati, e se ci sono cambiamenti setta lo stato della descrizione a UPDATED
//-----------------------------------------------------------------------------
void CWndLayoutContainerDescription::UpdateLayoutAttributes(CLayoutContainer* pLayoutContainer)
{
	if (m_LayoutType != pLayoutContainer->GetLayoutType())
	{
		m_LayoutType = pLayoutContainer->GetLayoutType();
		SetUpdated(&m_LayoutType);
	}

	if (m_LayoutAlign != pLayoutContainer->GetLayoutAlign())
	{
		m_LayoutAlign = pLayoutContainer->GetLayoutAlign();
		SetUpdated(&m_LayoutAlign);
	}
}

//-----------------------------------------------------------------------------
/*static*/CString CWndLayoutContainerDescription::GetEnumDescription(CLayoutContainer::LayoutType value)
{
	return singletonEnumLayoutContainerDescription.m_arLayoutType.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void CWndLayoutContainerDescription::GetEnumValue(CString description, CLayoutContainer::LayoutType& retVal)
{
	retVal = singletonEnumLayoutContainerDescription.m_arLayoutType.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndLayoutContainerDescription::GetEnumDescription(CLayoutContainer::LayoutAlign value)
{
	return singletonEnumLayoutContainerDescription.m_arLayoutAlign.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void CWndLayoutContainerDescription::GetEnumValue(CString description, CLayoutContainer::LayoutAlign& retVal)
{
	retVal = singletonEnumLayoutContainerDescription.m_arLayoutAlign.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndLayoutContainerDescription::GetEnumDescription(TileDialogStyle value)
{
	return singletonEnumLayoutContainerDescription.m_arTileDialogStyle.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void CWndLayoutContainerDescription::GetEnumValue(CString description, TileDialogStyle& retVal)
{
	retVal = singletonEnumLayoutContainerDescription.m_arTileDialogStyle.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndLayoutContainerDescription::GetEnumDescription(IconTypes value)
{
	return singletonEnumDescription.m_arIconTypes.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void CWndLayoutContainerDescription::GetEnumValue(CString description, IconTypes& retVal)
{
	retVal = singletonEnumDescription.m_arIconTypes.GetEnum(description);
}

//-----------------------------------------------------------------------------
void EnumLayoutContainerDescriptionAssociations::InitEnumLayoutContainerDescriptionStructures()
{
	/*LayoutType*/
	m_arLayoutType.Add(CLayoutContainer::STRIPE,	_T("Stripe"));
	m_arLayoutType.Add(CLayoutContainer::HBOX,		_T("HBox"));
	m_arLayoutType.Add(CLayoutContainer::VBOX,		_T("VBox"));
	m_arLayoutType.Add(CLayoutContainer::COLUMN,	_T("Column"));
	
	/*LayoutAlign*/
	m_arLayoutAlign.Add(CLayoutContainer::BEGIN,		_T("Begin"));
	m_arLayoutAlign.Add(CLayoutContainer::MIDDLE,		_T("Middle"));
	m_arLayoutAlign.Add(CLayoutContainer::END,			_T("End"));
	m_arLayoutAlign.Add(CLayoutContainer::STRETCHMAX,	_T("StretchMax"));
	m_arLayoutAlign.Add(CLayoutContainer::STRETCH,		_T("Stretch"));
	
	/*TileDialogStyle*/
	m_arTileDialogStyle.Add(TDS_NORMAL, _T("Normal"));
	m_arTileDialogStyle.Add(TDS_FILTER, _T("Filter"));
	m_arTileDialogStyle.Add(TDS_HEADER, _T("Header"));
	m_arTileDialogStyle.Add(TDS_FOOTER, _T("Footer"));
	m_arTileDialogStyle.Add(TDS_WIZARD, _T("Wizard"));
	m_arTileDialogStyle.Add(TDS_PARAMETERS, _T("Parameters"));
	m_arTileDialogStyle.Add(TDS_BATCH, _T("Batch"));
	m_arTileDialogStyle.Add(TDS_NONE, _T("None"));
}
