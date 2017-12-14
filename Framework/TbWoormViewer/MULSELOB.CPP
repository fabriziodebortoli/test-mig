
#include "stdafx.h"

// for manifest constant INT_MAX
#include <limits.h>

#include <TBGenlib\BaseApp.h>

#include "woormini.h"
#include "viewpars.h"
#include "woormdoc.h"
#include "woormfrm.h"
#include "woormvw.h"
#include "singleob.h"

#include "mulselob.h"
#include "RSEditorUI.h"
#include "rectobj.h"

// resources
#include "listdlg.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


//==============================================================================
//          Class SelectedObj
//==============================================================================

//---------------------------------------------------------------------------
SelectedObj::SelectedObj (BaseObj* pObj, CRect rect, CWoormDocMng* pDocument)
	:
	m_pBaseObj		(pObj),
	m_TrackingRect	(rect),
	m_pUndoStack(new Stack())
{
	if (pDocument->m_pWoormIni->m_bEnableNewObjectSelection)
		m_pActive = new NewActiveRect(pDocument->m_pWoormIni, TRUE);
	else
		m_pActive = new OldActiveRect(FALSE);

	m_pActive->Attach(pDocument);
}

//---------------------------------------------------------------------------
SelectedObj::~SelectedObj ()
{
	m_pActive->Clear();
	SAFE_DELETE(m_pActive);
	SAFE_DELETE(m_pUndoStack);
}

//------------------------------------------------------------------------------
void SelectedObj::Paint (CDC& DC, CPrintInfo* pInfo)
{                                                                                              
	if (pInfo) return;
	m_pActive->Paint(DC, pInfo);
}

//---------------------------------------------------------------------------
void SelectedObj::LeftObject (int x)
{
	m_pBaseObj->LeftObject(x);
	m_TrackingRect = m_pBaseObj->GetBaseRect();
}

//---------------------------------------------------------------------------
void  SelectedObj::TopObject (int y)
{
	m_pBaseObj->TopObject(y);
	m_TrackingRect = m_pBaseObj->GetBaseRect();
}

//---------------------------------------------------------------------------
void SelectedObj::MoveObject (CSize offset)
{
	if (m_pBaseObj->IsAnchored()) //nella multiselezione non sposto le colonne ancorate in oriz
	{
		offset.cx = 0;    //non lo posso spostare in orizzontale
	}

	m_pBaseObj->MoveObject(offset);
	m_TrackingRect = m_pBaseObj->GetBaseRect();
}

//---------------------------------------------------------------------------
void SelectedObj::SizeObject (CSize size)
{
	RightSizeObject(size.cx);
	BottomSizeObject(size.cy);
	m_TrackingRect = m_pBaseObj->GetBaseRect();
}

//---------------------------------------------------------------------------
void SelectedObj::RightSizeObject (int Width)
{
	m_pBaseObj->RightSizeObject(Width);
	m_TrackingRect = m_pBaseObj->GetBaseRect();
}

//---------------------------------------------------------------------------
void SelectedObj::BottomSizeObject (int Height)
{
	m_pBaseObj->BottomSizeObject(Height);
	m_TrackingRect = m_pBaseObj->GetBaseRect();
}

//---------------------------------------------------------------------------
void SelectedObj::LeftSizeObject (int Width)
{
	m_pBaseObj->LeftSizeObject(Width);
	m_TrackingRect = m_pBaseObj->GetBaseRect();
}

//---------------------------------------------------------------------------
void SelectedObj::TopSizeObject (int Height)
{
	m_pBaseObj->TopSizeObject(Height);
	m_TrackingRect = m_pBaseObj->GetBaseRect();
}

//---------------------------------------------------------------------------
void SelectedObj::SetActive (BOOL bLast)
{
	m_pActive->SetActive(m_pBaseObj->GetActiveRect(), bLast);
}

//---------------------------------------------------------------------------
void SelectedObj::Undo()
{
	if (m_pUndoStack->GetSize() == 0)
		return;

	// Pop info from storic last action stack info
	UndoInfo* pUndoInfo = (UndoInfo*)m_pUndoStack->Pop();
	CSize size;
	CRect objRect = m_pBaseObj->GetBaseRect();

	size.cx = pUndoInfo->m_rect.Width();
	size.cy = pUndoInfo->m_rect.Height();

	CSize offset = pUndoInfo->m_rect.TopLeft() - objRect.TopLeft();
	m_pBaseObj->MoveObject(offset);

	SizeObject(size);	
	m_TrackingRect = m_pBaseObj->GetBaseRect();

	delete pUndoInfo;
}


//==============================================================================
//          Class SelectionRect implementation
//==============================================================================

//------------------------------------------------------------------------------
SelectionRect::SelectionRect (CWoormDocMng* pDocument)
  :
  SingleItemObj(CPoint(0,0), pDocument)
{
	m_BaseRect.SetRectEmpty();
}

//------------------------------------------------------------------------------
SelectionRect::~SelectionRect ()
{
	ClearMultipleSelObjects();
	m_pDocument->GetWoormFrame()->ShowAlignToolbar(FALSE);
}

//------------------------------------------------------------------------------
void SelectionRect::ClearMultipleSelObjects()
{   
	AddUndoInfo();

	// no objects. Clear size
	m_BaseRect.SetRectEmpty();
	
	// remove all objects before close pDocument
	for (int i = m_SelectedObjects.GetUpperBound(); i >= 0; i--)
	{
		if (m_SelectedObjects[i])
		{
			delete m_SelectedObjects[i];
			m_SelectedObjects[i] = NULL;
		}
	}
	m_SelectedObjects.RemoveAll();
}

//------------------------------------------------------------------------------
void SelectionRect::BuildBaseRect ()
{           
	m_BaseRect.SetRectEmpty();
	
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{                         
		SelectedObj* pObj = m_SelectedObjects[i];
		m_BaseRect.UnionRect (m_BaseRect, pObj->m_pBaseObj->GetBaseRect());
		pObj->SetActive (i == ub);
	}
}

//------------------------------------------------------------------------------
void SelectionRect::AddToSelectedObjects (BaseObj* pBaseObj, CRect rectTracking)
{
	AddUndoInfo();

	previousSize = GetSize();
	// show palette if almost two element are selected
	if (m_SelectedObjects.GetSize() == 1)
	{
		m_pDocument->GetWoormFrame()->ShowAlignToolbar(TRUE);
	}

	SelectedObj* pObj = new SelectedObj(pBaseObj, rectTracking, m_pDocument);
	m_SelectedObjects.Add (pObj);
	m_pDocument->SelectRSTreeItemData(ERefreshEditor::Layouts, pBaseObj, TRUE);
	BuildBaseRect(); 
}

//------------------------------------------------------------------------------
void SelectionRect::RemoveFromSelectedObjects (BaseObj* pBaseObj)
{          
	AddUndoInfo();

	previousSize = GetSize();
	BOOL deleted = FALSE;
	                                      
	// search object and remove it from list
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{                         
		SelectedObj* pObj = m_SelectedObjects[i];
		if (pObj && (pObj->m_pBaseObj == pBaseObj))
		{             
 			m_SelectedObjects.RemoveAt(i);
			delete pObj;
			deleted = TRUE;
			break;
		}
	}
    
    if (deleted)
    {

		// hide palette if there are minus than two element
		if (m_SelectedObjects.GetSize() < 2)
		{
			m_pDocument->GetWoormFrame()->ShowAlignToolbar(FALSE);
		}
		// recalc m_BaseRect as rectangle including all selected object
		BuildBaseRect();
		m_pDocument->DeselectRSTreeItemData(ERefreshEditor::Layouts, pBaseObj);
	}
}

//------------------------------------------------------------------------------
void SelectionRect::Paint (CDC& DC, CPrintInfo* pInfo)
{                                                                                              
	// assuring mousemove in tracking mode, Paint is inhibit
	// also don't print if are on printer
	if (m_bInhibitPaint || pInfo) return;

	CRect invalid;
	CRect rect = m_BaseRect;
	ScaleRect(rect, DC);
	DC.GetClipBox(&invalid);

	int ub = m_SelectedObjects.GetUpperBound();
	if (rect.IntersectRect(&rect, &invalid))
		for (int i = 0; i <= ub; i++)
		{
			SelectedObj* pObj = m_SelectedObjects[i];
			pObj->Paint (DC, pInfo);
		}
}

//------------------------------------------------------------------------------
void SelectionRect::OnTrackingDraw(CDC& DC, CBCGPScrollView* /*pView*/, BOOL /*bShowRect*/)
{
	// track selected object only in MOVE action
	if (m_nAction != MOVE) return;
	
	// update tarcking rect for pDocument status bar update purpose.
	m_pDocument->m_TrackRect = m_BaseRect;

	// Draw all included objects in multiple selection
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		CRect rect = pObj->m_TrackingRect;
		ScaleRect(rect, DC);
		DC.Rectangle(rect);
	}
}

//---------------------------------------------------------------------------
SingleItemObj::ActionType SelectionRect::ItemAction (CPoint aPoint)
{
	if (!m_BaseRect.PtInRect(aPoint)) return LEAVE;
	return MOVE;
}

//------------------------------------------------------------------------------
void SelectionRect::Resize (CPoint c, CPoint p)
{   
	// move all multiple selected objects only if moving selection rectangle
	if (m_nAction != MOVE) return;

	CRect   rectPrev = m_BaseRect;
	CSize	csDelta = c - p;
	
	// make move action on base rect of multiple selection for mantain all
	// base object caratteristics (for this essentially move and mouse management
	m_BaseRect += CPoint(csDelta);

	// constrain object in positive axis
	if ((m_BaseRect.top < 0) || (m_BaseRect.left < 0))
	{
		m_BaseRect = rectPrev;
		return;
	}

	// move all multiple selected rectangle
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		
		if(pObj->m_pBaseObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			BaseRect* pBaseRect = (BaseRect*) pObj->m_pBaseObj;
			if (pBaseRect->IsAnchored())
			{
				pObj->m_TrackingRect += CPoint(0, c.y - p.y);
				continue;
			}
		}
		pObj->m_TrackingRect += CPoint(c - p);
	}

	AddUndoInfo();
}

// don't refresh object (ghost object) and m_pActive status
//------------------------------------------------------------------------------
void SelectionRect::ChangedAction()
{
	// if multiple selection active move selected GetObjects()
	m_pDocument->MoveObjects();
	m_pDocument->UpdateWindow();
}

// move all object and reconstruct m_BaseRect for multple selection
// set m_pActive all object moved (like BuildBaseRect())
//
//------------------------------------------------------------------------------
void SelectionRect::MoveMultipleSelObjects(CSize offset)
{
	m_BaseRect.SetRectEmpty();

	// move all multiple selected rectangle
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		// move only object because m_TrackingRect is already moved by Resize function
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->AddUndoInfo();
		pObj->MoveObject (offset);
		m_BaseRect.UnionRect (m_BaseRect, pObj->m_pBaseObj->GetBaseRect());
		pObj->SetActive (i == ub);
	}
	
}

// se sono tutti On o Tutti Off torno FALSE altrimenti TRUE.
//------------------------------------------------------------------------------
BOOL SelectionRect::IsBorderOn(UINT nID)
{
	BOOL bOn = FALSE;

	for (int i = 0; i <= m_SelectedObjects.GetUpperBound(); i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		if (pObj->m_pBaseObj->IsBorderOn(nID))
			bOn = TRUE;
	}
	return bOn;
}

// se sono tutti On o Tutti Off torno FALSE altrimenti TRUE.
//------------------------------------------------------------------------------
BOOL SelectionRect::IsTransparentOn()
{
	BOOL bOn = FALSE;

	for (int i = 0; i <= m_SelectedObjects.GetUpperBound(); i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		if (pObj->m_pBaseObj->IsTransparent())
			bOn = TRUE;
	}
	return bOn;
}

//------------------------------------------------------------------------------
void SelectionRect::ToggleBorder(UINT nID)
{
	BOOL bOn = IsBorderOn(nID);
	for (int i = 0; i <= m_SelectedObjects.GetUpperBound(); i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->m_pBaseObj->ToggleBorder(nID, !bOn);
	}
}

//------------------------------------------------------------------------------
void SelectionRect::SnapToGrid()
{
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->m_pBaseObj->SnapToGrid();

		m_BaseRect.UnionRect (m_BaseRect, pObj->m_pBaseObj->GetBaseRect());
		pObj->SetActive (i == ub);
	}
}

// se sono tutti On o Tutti Off torno FALSE altrimenti TRUE.
//------------------------------------------------------------------------------
void SelectionRect::ToggleTransparent()
{
	BOOL bOn = IsTransparentOn();
	for (int i = 0; i <= m_SelectedObjects.GetUpperBound(); i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->m_pBaseObj->ToggleTransparent(!bOn);
	}
}

//------------------------------------------------------------------------------
BOOL SelectionRect::IsEmpty() const
{
	return (m_SelectedObjects.GetSize() == 0);
}

//------------------------------------------------------------------------------
void SelectionRect::SizeLargeAsLast()             
{
	int ub = m_SelectedObjects.GetUpperBound();
	// pick last object for reference
	SelectedObj* pLastObj = m_SelectedObjects[ub];
	int nWidth = pLastObj->Width();

	// size objects
	for (int i = 0; i < ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->AddUndoInfo();
		pObj->RightSizeObject (nWidth);
	}
}

//------------------------------------------------------------------------------
void SelectionRect::SizeHValue (int nWidth)
{
	// size objects
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->AddUndoInfo();
		pObj->RightSizeObject (nWidth);
	}
}

//------------------------------------------------------------------------------
void SelectionRect::SizeHighAsLast ()                
{
	int ub = m_SelectedObjects.GetUpperBound();
	// pick last object for reference
	SelectedObj* pLastObj = m_SelectedObjects[ub];
	int nHeight = pLastObj->Height();

	// size objects
	for (int i = 0; i <= ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->AddUndoInfo();
		pObj->BottomSizeObject (nHeight);
	}
}

//------------------------------------------------------------------------------
void SelectionRect::SizeVValue (int nHeight)
{
	// size objects
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->AddUndoInfo();
		pObj->BottomSizeObject (nHeight);
	}
}

//------------------------------------------------------------------------------
void SelectionRect::StackLeft()
{
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = ub; i >0; i--)
	{   
		SelectedObj* pObj1 = m_SelectedObjects[i]; 
		SelectedObj* pObj2 = m_SelectedObjects[i-1];
 
		CSize offset(pObj1->Right() - pObj2->Left(), 0);
		pObj2->MoveObject (offset);	
	}			
}

//------------------------------------------------------------------------------
void SelectionRect::StackRight()
{
	int i;
	int ub = m_SelectedObjects.GetUpperBound();

	// pick last object for reference
	SelectedObj* pObj = m_SelectedObjects[ub];	
	int nTotalDim = pObj->Left(); 
	
	// try to fit in client area (only positive coordinate)
	for (i = 0; i < ub; i++)
	{
		pObj = m_SelectedObjects[i]; 
		nTotalDim = nTotalDim - pObj->Width();
		if (nTotalDim < 0)
		{
			AfxMessageBox (_TB("Objects exceed sheet limits when aligned!"));
			return;
		}
	}

	// can stack so do it	
	for (i = ub; i >0 ; i--)
	{   
		SelectedObj* pObj1 = m_SelectedObjects[i]; 
		SelectedObj* pObj2 = m_SelectedObjects[i-1];
 
		CSize offset(pObj1->Left() - pObj2->Right(), 0);
		pObj2->MoveObject (offset);
	}	
}


//------------------------------------------------------------------------------
void SelectionRect::StackBottom()
{
	int i;	
	int ub = m_SelectedObjects.GetUpperBound();

	// pick last object for reference
	SelectedObj* pObj = m_SelectedObjects[ub];	
	int nTotalDim = pObj->Top(); 
	
	// try to fit in client area (only positive coordinate)
	for (i = 0; i < ub; i++)
	{
		pObj = m_SelectedObjects[i]; 
		nTotalDim = nTotalDim - pObj->Height();
		if (nTotalDim < 0)
		{
			AfxMessageBox (_TB("Objects exceed sheet limits when aligned!"));
			return;
		}
	}
	
	// can stack so do it	
	for (i = ub; i > 0; i--)
	{   
		SelectedObj* pObj1 = m_SelectedObjects[i]; 
		SelectedObj* pObj2 = m_SelectedObjects[i-1];
 
		CSize offset(0, pObj1->Top() - pObj2->Bottom());
		pObj2->MoveObject (offset);
	}	
}

//------------------------------------------------------------------------------
void SelectionRect::StackTop()
{
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = ub; i >0; i--)
	{   
		SelectedObj* pObj1 = m_SelectedObjects[i]; 
		SelectedObj* pObj2 = m_SelectedObjects[i-1];
 
		CSize offset(0, pObj1->Bottom() - pObj2->Top());
		pObj2->MoveObject (offset);
	}	
}

//------------------------------------------------------------------------------
void SelectionRect::AlignHLeft()
{
	int ub = m_SelectedObjects.GetUpperBound();
	// pick last object for reference
	SelectedObj* pLastObj = m_SelectedObjects[ub];
	int Left = pLastObj->Left();

	// move objects
	for (int i = 0; i < ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->AddUndoInfo();
		CSize offset(Left - pObj->Left(), 0);
		pObj->MoveObject (offset);
	}
}

//------------------------------------------------------------------------------
void SelectionRect::AlignHRight()
{
	int ub = m_SelectedObjects.GetUpperBound();
	// pick last object for reference
	SelectedObj* pLastObj = m_SelectedObjects[ub];
	int Right = pLastObj->Right();

	// move objects
	for (int i = 0; i < ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->AddUndoInfo();
		CSize offset(Right - pObj->Right(), 0);
		pObj->MoveObject (offset);
	}
}

//------------------------------------------------------------------------------
void SelectionRect::AlignVTop()
{
	int ub = m_SelectedObjects.GetUpperBound();
	// pick last object for reference
	SelectedObj* pLastObj = m_SelectedObjects[ub];
	int Top = pLastObj->Top();

	// move objects
	for (int i = 0; i < ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->AddUndoInfo();
		CSize offset(0, Top - pObj->Top());
		pObj->MoveObject (offset);
	}
}

//------------------------------------------------------------------------------
void SelectionRect::AlignVBottom()
{
	int ub = m_SelectedObjects.GetUpperBound();
	// pick last object for reference
	SelectedObj* pLastObj = m_SelectedObjects[ub];
	int Bottom = pLastObj->Bottom();

	// move objects
	for (int i = 0; i < ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		pObj->AddUndoInfo();
		CSize offset(0, Bottom - pObj->Bottom());
		pObj->MoveObject (offset);
	}
}

//
//  Resizes objects to the Left border of the last selected object
//------------------------------------------------------------------------------
void SelectionRect::CutHLeft()
{
    int nSelObj = m_SelectedObjects.GetUpperBound();
    int nLeftRef = (m_SelectedObjects[nSelObj])->Left();
	    
	for (int i = 0; i < nSelObj; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		if ((pObj->Right() - nLeftRef) > MINCELL_X)
			pObj->LeftSizeObject(pObj->Right() - nLeftRef);
	}
}

//
//  Resizes objects to the Right border of the last selected object
//------------------------------------------------------------------------------
void SelectionRect::CutHRight()
{
    int nSelObj = m_SelectedObjects.GetUpperBound();
    int nRightRef = (m_SelectedObjects[nSelObj])->Right();
	    
	for (int i = 0; i < nSelObj; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		if ((nRightRef - pObj->Left()) > MINCELL_X )
			pObj->RightSizeObject(nRightRef - pObj->Left());
	}
}

//
//  Resizes objects to the Top border of the last selected object
//------------------------------------------------------------------------------
void SelectionRect::CutVTop()
{
    int nSelObj	= m_SelectedObjects.GetUpperBound();
    int nTopRef		= (m_SelectedObjects[nSelObj])->Top();
	    
	for (int i = 0; i < nSelObj; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		if ((pObj->Bottom() - nTopRef) > MINCELL_Y) 
			pObj->TopSizeObject(pObj->Bottom() - nTopRef);
	}
}

//
//  Resizes objects to the Bottom border of the last selected object
//------------------------------------------------------------------------------
void SelectionRect::CutVBottom()
{
    int nSelObj	= m_SelectedObjects.GetUpperBound();
    int nBottomRef 	= (m_SelectedObjects[nSelObj])->Bottom();
	    
	for (int i = 0; i < nSelObj; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		if ((nBottomRef - pObj->Top()) > MINCELL_Y) 
			pObj->BottomSizeObject(nBottomRef - pObj->Top());
	}
}

// use bubble sort method because there are few object to sort
//
//------------------------------------------------------------------------------
void SelectionRect::SortPosition(BOOL bVertical)
{
	// sort if almost two elemnt are selected
	if (m_SelectedObjects.GetSize() < 2) return;

	int i = 0;
	int nFirst;
	int nSecond;

	SelectedObj* pObj1;
	SelectedObj* pObj2;

	while (TRUE)
	{
		// end sort condition
		if (i == m_SelectedObjects.GetUpperBound()) return;

		pObj1 = m_SelectedObjects[i];
		pObj2 = m_SelectedObjects[i + 1];

		nFirst   = bVertical ? pObj1->Top() : pObj1->Left();
		nSecond  = bVertical ? pObj2->Top() : pObj2->Left();

		if (nFirst > nSecond)
		{
			m_SelectedObjects[i]      = pObj2;
			m_SelectedObjects[i + 1]  = pObj1;

			i = 0;
			continue;
		}

		i++;
	}
}

//------------------------------------------------------------------------------
void SelectionRect::AlignSpaceEqual(BOOL bVertical)
{
	int i;
	int nSizes				= 0;
	int nMaxDistance		= 0;
	int nTotalEmptySpace	= 0;

	int nGap			= 0;
	int nRealGap		= 0;
	int nRemainingGap	= 0;
	int nAdjust			= 0;

	int nNrElement  = m_SelectedObjects.GetSize();
	int nNrSpaceble = nNrElement - 2;
	int nNrSpace    = nNrElement - 1;

	// valid only if almost three objects
	if (nNrElement < 3) return;

	// sort selected object using hor/vert position
	SortPosition (bVertical);

	// calculate global Width of all object contained between nFirst and last one
	for (i = 1; i <= nNrSpaceble; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		nSizes += (bVertical ? pObj->Height() : pObj->Width());
	}

	// calculate distance between end of nFirst object and start of last object
	SelectedObj* pFirst  = m_SelectedObjects[0];
	SelectedObj* pLast   = m_SelectedObjects[m_SelectedObjects.GetUpperBound()];

	nMaxDistance = bVertical ?
		pLast->Top() - pFirst->Bottom():
		pLast->Left() - pFirst->Right();

	// do alignement action only if there are enough space
	nTotalEmptySpace = nMaxDistance - nSizes;
	if (nTotalEmptySpace <= 0) return;

	// calculate nGap and remaining nGap
	nGap = nTotalEmptySpace / nNrSpace;
	nRemainingGap = nTotalEmptySpace - (nGap * nNrSpace);

	// move objects (don't count nFirst and last object)
	for (i = 1; i <= nNrSpaceble; i++)
	{
		SelectedObj* pObj1 = m_SelectedObjects[i - 1];
		SelectedObj* pObj2 = m_SelectedObjects[i];

		nAdjust = nRemainingGap ? 1 : 0;
		nRealGap = (bVertical ? pObj1->Bottom() : pObj1->Right()) + nGap + nAdjust;
		bVertical ? pObj2->TopObject (nRealGap) : pObj2->LeftObject (nRealGap);

		if (nRemainingGap) nRemainingGap--;
	}
}

//------------------------------------------------------------------------------
void SelectionRect::AlignVSpaceEqual()
{
	AlignSpaceEqual (TRUE);
}

//------------------------------------------------------------------------------
void SelectionRect::AlignHSpaceEqual()
{
	AlignSpaceEqual (FALSE);
}

//------------------------------------------------------------------------------
void SelectionRect::MoveMultipleSelObjects ()	
{ 
	MoveMultipleSelObjects(MovingOffset()); 
}

//------------------------------------------------------------------------------
BOOL SelectionRect::ObjectInSelection (BaseObj* pBaseObj)
{
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{                         
		SelectedObj* pObj =  m_SelectedObjects[i];
		if (pObj && (pObj->m_pBaseObj == pBaseObj))
			return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL SelectionRect::ContainsAtLeastOneFieldRect()
{
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		if (pObj && (pObj->m_pBaseObj->IsKindOf(RUNTIME_CLASS(FieldRect))))
			return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------
SelectedObj* SelectionRect::GetSelectedObjInSelection (BaseObj* pBaseObj)
{
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{                         
		SelectedObj* pObj =  m_SelectedObjects[i];
		if (pObj && (pObj->m_pBaseObj == pBaseObj))
			return pObj;
	}
	return NULL;
}

//------------------------------------------------------------------------------
void SelectionRect::SizeMultipleSelObjects(int hID, int vID, CSize size)
{
// siri
//		case IDC_H_SMALLEST :   sizeHSmallest   ();     break;
//		case IDC_H_LARGEST  :   sizeHLargest    ();     break;         
		if (hID == IDC_LAST_LARGE)	
			SizeLargeAsLast ();		
		else if (hID == IDC_H_VALUE)    
			SizeHValue  (size.cx);  

//		case IDC_V_SMALLEST :   sizeVSmallest   ();         break;
//		case IDC_V_LARGEST  :   sizeVLargest    ();         break;
		if (vID == IDC_LAST_HIGH)
			SizeLargeAsLast();			
		else if (vID == IDC_V_VALUE)
			SizeVValue(size.cy);  
	
	// recalc modified multiple selection m_BaseRect
	BuildBaseRect();
}


//------------------------------------------------------------------------------
void SelectionRect::AlignMultipleSelObjects(int hID, int vID)
{
		if (hID == IDC_H_LEFT)   
			AlignHLeft();      
		else if (hID == IDC_H_RIGHT)        
			AlignHRight();  
		else if (hID == IDC_H_SPACE_EQUAL)   
			AlignHSpaceEqual(); 
		else if (hID == IDC_STACK_LEFT)			
			StackLeft();	
		else if (hID == IDC_STACK_RIGHT)	
			StackRight();		
		else if (hID == IDC_CUT_H_LEFT)	
			CutHLeft();			
		else if (hID == IDC_CUT_H_RIGHT)	
			CutHRight();		

		if (vID == IDC_V_TOP)
			AlignVTop();        
		else if (vID == IDC_V_BOTTOM)
			AlignVBottom();     
		else if (vID == IDC_V_SPACE_EQUAL)
			AlignVSpaceEqual(); 
		else if (vID == IDC_STACK_TOP)
			StackTop();			
		else if (vID == IDC_STACK_BOTTOM)
			StackBottom();		
		else if (vID == IDC_CUT_V_TOP)
			CutVTop();			
		else if (vID == IDC_CUT_V_BOTTOM)
			CutVBottom();		
	
	// recalc modified multiple selection m_BaseRect
	BuildBaseRect();
}

//------------------------------------------------------------------------------
BaseObj* SelectionRect::GetObjAt(int i)
{
	if (i > m_SelectedObjects.GetUpperBound())
	{
		ASSERT(FALSE);
		return NULL;
	}
	BaseObj* pObj = m_SelectedObjects[i]->m_pBaseObj;
	ASSERT_VALID(pObj);
	return pObj;
}

//------------------------------------------------------------------------------todo andrea: se non è usata, una volta completata la property grid, rimuovere
const CRect& SelectionRect::GetRectAt(int i)
{
	if (i > m_SelectedObjects.GetUpperBound())
		ASSERT(FALSE);
	return m_SelectedObjects[i]->m_pBaseObj->GetBaseRect();
}

//------------------------------------------------------------------------------
void SelectionRect::Redraw()
{
	for (int i = 0; i <= m_SelectedObjects.GetUpperBound(); i++)
		m_SelectedObjects[i]->m_pBaseObj->Redraw();
}

//------------------------------------------------------------------------------
void SelectionRect::SetWrmStyleClass(CObject* templateObj)
{
	for (int i = 0; i <= m_SelectedObjects.GetUpperBound(); i++)
		m_SelectedObjects[i]->m_pBaseObj->SetWrmStyleClass(templateObj);
}

//------------------------------------------------------------------------------
CString SelectionRect::GetWrmStyleClass()
{
	int i;
	CString commonStyle;

	//should be the first row of every case statement
	BaseObj* pObj = GetObjAt(0);
	ASSERT_VALID(pObj);
	if (!pObj)
		return NULL;

	commonStyle = pObj->GetWrmStyleClass();

	for (i = 0; i < GetSize(); i++)
		if (commonStyle != GetObjAt(i)->GetWrmStyleClass()) break;

	if (i == GetSize())
	{
		//li ho scorsi tutti, quindi sono tutti uguali e valorizzo la runtime class a quella comune, altrimenti rimarrà vuota
		if (commonStyle &&  commonStyle.IsEmpty())
			commonStyle = L"<Default>";
		return commonStyle;
	}
	else return NULL;
}

//------------------------------------------------------------------------------
void SelectionRect::UndoMoveAndSize(CRect rect)
{
	CSize offset = rect.TopLeft() - m_BaseRect.TopLeft();
	MoveMultipleSelObjects(offset);
}

//------------------------------------------------------------------------------
void SelectionRect::Undo()
{
	m_BaseRect.SetRectEmpty();

	// move all multiple selected rectangle
	int ub = m_SelectedObjects.GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		SelectedObj* pObj = m_SelectedObjects[i];
		m_SelectedObjects[i]->Undo();
		m_BaseRect.UnionRect(m_BaseRect, pObj->m_pBaseObj->GetBaseRect());
		pObj->SetActive(i == ub);
	}
}

//------------------------------------------------------------------------------
INT_PTR SelectionRect::Find(CNodeTree* pNode)
{
	BaseObj* pBaseObj = dynamic_cast<BaseObj*>(pNode->m_pItemData);
	if (pBaseObj)
		for (int i = m_SelectedObjects.GetUpperBound(); i >= 0; i--)
			if (this->m_SelectedObjects[i]->m_pBaseObj == pBaseObj)
				return i;

	return -1;
}

//==============================================================================
//							Class DrawRect
//==============================================================================
//
//------------------------------------------------------------------------------
DragRect::DragRect(CPoint ptCurrPos, CWoormDocMng* pDocument)
  :
  SingleItemObj(ptCurrPos, pDocument)
{	
	//forzo la dimensione iniziale a (0,0)
	m_BaseRect.bottom = m_BaseRect.top;
	m_BaseRect.right = m_BaseRect.left;
}

// aggiunge tutti gli oggetti nell' area selezionata alla multipla selezione
//------------------------------------------------------------------------------
void DragRect::ChangedAction()
{
	m_pDocument->MultipleSelect(m_pDocument->m_TrackRect, FALSE);
}

// Pulisce i crocicchi nell'ultima posizione di Track. Deve pulire anche il 
// rettangolo perche' non implementa il metodo Draw e quindi nessuno fa un rettangolo
//------------------------------------------------------------------------------
void DragRect::OnTrackingDraw(CDC& aDC, CBCGPScrollView* pView, BOOL bShowRect)
{
	m_pDocument->m_TrackRect = GetTrackRect();
	aDC.Rectangle(m_BaseRect);
}

//------------------------------------------------------------------------------
void DragRect::MouseMove(CDC& aDC, CPoint ptMousePos, CBCGPScrollView* pView)
{
	// avoid spure mousemove message with not real move
	if (m_ptPrevPos == ptMousePos)
		return;

	// resize only in m_bChanged position based on grid sensibility
	CSize sz = ptMousePos - m_ptPrevPos;

	// inihibit Paint during resize of object
	m_bInhibitPaint = TRUE;

	TrackingDraw(aDC, pView, !m_bFirstTime);
	m_bFirstTime = FALSE;

	Resize(ptMousePos, m_ptPrevPos);
	TrackingDraw(aDC, pView);

	m_ptPrevPos = ptMousePos;
	m_bChanged = TRUE;
}

//------------------------------------------------------------------------------
void DragRect::InitDraggingRect(CDC& aDC, CPoint ptMousePos, CBCGPScrollView* pView)
{
	m_bTrackMode = TRUE;

	m_nAction = DRAGGING; //has the limited area equal to the client area

	// process procedure needed befor action is done (only if the are action !)
	BeforeAction();

	// needed for link cursor with object variation
	ClipActionCursor(aDC, ptMousePos, pView);

	// set starting value for mouse position e rect dimension
	m_ptPrevPos = ptMousePos;
	m_rectStartMouseMove = m_BaseRect;
	AddUndoInfo();

	// set flags for managing modified objects
	m_bFirstTime = TRUE;
	m_bChanged = FALSE;

	// permette il repaint di tutti gli oggetti invalidi prima di processare
	// il tracking durante i mouse move
	pView->UpdateWindow();

	// Mette i crocicchi nella prima posizione per far vedere al primo
	// clic dove e` posizionato il rettangolo
	TrackingDraw(aDC, pView, FALSE);
}

//------------------------------------------------------------------------------
//inverto le coordinate se la dimensione relativa ad essa è negativa
CRect DragRect::GetTrackRect()
{
	CRect tmpRect(m_BaseRect);

	if (tmpRect.Width() < 0)
		tmpRect.SwapLeftRight();

	if (tmpRect.Height() < 0)
	{
		LONG tempY = tmpRect.top;
		tmpRect.top = tmpRect.bottom;
		tmpRect.bottom = tempY;
	}

	return tmpRect;
}		

//==============================================================================
//							Class MultiColumnSelection
//==============================================================================
//
//------------------------------------------------------------------------------
MultiColumnSelection::MultiColumnSelection(CWoormDocMng* pDocument)
	:
	IDisposingSourceImpl(this),
	m_pDocument(pDocument)
{
}

//-----------------------------------------------------------------------------
void MultiColumnSelection::AddToSelectedColumns(TableColumn* pCol, CRect rect)
{
	// remove all objects before close pDocument
	for (int i = m_columns.GetUpperBound(); i >= 0; i--)
		if (m_columns[i]->m_pColumn == pCol)
			return;

	previousSize = GetSize();

	ActiveColumn* selCol = new ActiveColumn(pCol, m_pDocument);

	m_columns.Add(selCol);

	BuildBaseRect();
}

//-----------------------------------------------------------------------------
void MultiColumnSelection::RemoveAllSelectedColumns()
{
	m_columns.RemoveAll();
}

//-----------------------------------------------------------------------------
void MultiColumnSelection::DeleteAllSelectedColumns()
{
	if (m_columns.GetSize() == 0)
		return;

	int i = m_columns.GetUpperBound();
	Table* pTable = m_columns[i]->m_pColumn->GetTable();
	ASSERT_VALID(pTable);

	// delete all columns in the selection
	for (; i >= 0; i--)
	{
		TableColumn* pColumn = m_columns[i]->m_pColumn;
		ASSERT_VALID(pColumn);

		m_columns.RemoveAt(i);

		int nCol = pTable->GetColumnIndexByPtr(pColumn);
		if (nCol > -1)
			pTable->DeleteColumnComplete(nCol, FALSE);
	}
	
	m_columns.RemoveAll();
}

//-----------------------------------------------------------------------------
CString MultiColumnSelection::GetAllTitles()
{
	CString allTitles;

	for (int i = 0; i <= m_columns.GetUpperBound(); i++)
	{
		if (i != 0) allTitles += L", ";
		TableColumn* pCol = m_columns[i]->m_pColumn;
		allTitles += pCol->GetTitle();
	}
	return allTitles;
}

//-----------------------------------------------------------------------------
void MultiColumnSelection::Redraw()
{
	//todo stremizzato con una redraw delle tabelle, vedere se basta ridisegnare le colonn
	Table* pTable;
	for (int i = 0; i <= m_columns.GetUpperBound(); i++)
	{
		if (i == 0 || !pTable || m_columns[i]->m_pColumn->GetTable() != pTable)
		{
			pTable = m_columns[i]->m_pColumn->GetTable();
			pTable->Redraw();
		}
	}
	//todo vedere se serve
	BuildBaseRect();
}


//-----------------------------------------------------------------------------
TableColumn* MultiColumnSelection::GetAt(int index)
{
	if (index > m_columns.GetUpperBound())
		ASSERT(FALSE);
	return m_columns[index]->m_pColumn;
}

//------------------------------------------------------------------------------
void MultiColumnSelection::BuildBaseRect()
{
	int ub = m_columns.GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{
		ActiveColumn* pActCol = m_columns[i];
		if (!pActCol->m_pColumn->IsHidden())
			pActCol->SetActive();
	}
	m_pDocument->UpdateWindow();
}


//------------------------------------------------------------------------------
void MultiColumnSelection::Paint(CDC& DC, CPrintInfo* pInfo)
{
	int ub = m_columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		ActiveColumn* pActCol = m_columns[i];
		pActCol->Paint(DC, pInfo);
	}
}

//------------------------------------------------------------------------------
MultiColumnSelection::~MultiColumnSelection()
{
	ClearMultipleSelObjects();
}

//------------------------------------------------------------------------------
void MultiColumnSelection::ClearMultipleSelObjects()
{
	int ub = m_columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		ActiveColumn* pActCol = m_columns[i];
		if(pActCol)
		{
			delete m_columns[i];
			m_columns[i] = NULL;
		}
	}

	m_columns.RemoveAll();
}

//------------------------------------------------------------------------------
INT_PTR MultiColumnSelection::Find(CNodeTree* pNode)
{
	ASSERT_VALID(pNode);
	if (!pNode || !pNode->m_pItemData)
		return -1;
	CObject* pObj = (CObject*)pNode->m_pItemData;
	ASSERT_VALID(pObj);
	TableColumn* pCol = dynamic_cast<TableColumn*>(pObj);
	if (pCol)
		for (int i = m_columns.GetUpperBound(); i >= 0; i--)
			if (this->m_columns[i]->m_pColumn == pCol)
				return i;

	return -1;
}

//------------------------------------------------------------------------------
BOOL MultiColumnSelection::AreMultiRows	()
{
	for (int i = m_columns.GetUpperBound(); i >= 0; i--)
		if (!this->m_columns[i]->m_pColumn->IsMultiRow())
			return FALSE;
	return TRUE;
}
