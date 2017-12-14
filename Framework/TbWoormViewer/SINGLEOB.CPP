
#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\array.h>
#include <TbGeneric\minmax.h>
#include <TbGeneric\LocalizableObjs.h>

#include <TbGenlib\reswalk.h>

#include "listdlg.h"
#include "viewpars.h"
#include "woormdoc.h"
#include "woormini.h"

#include "singleob.h"
#include "RectObj.h"

// resources
#include "rectobj.hjson" //JSON AUTOMATIC UPDATE
#include "woormdoc.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


//==============================================================================
//          Class SingleItemObj implementation
//==============================================================================

IMPLEMENT_DYNAMIC (SingleItemObj, BaseObj)

//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SingleItemObj, BaseObj)
	//{{AFX_MSG_MAP(SingleItemObj)
	ON_COMMAND(ID_LINE_WIDTH,      LineWidth)
	ON_COMMAND(ID_SIZE,            SetSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//------------------------------------------------------------------------------
SingleItemObj::SingleItemObj (CPoint ptCurrPos, CWoormDocMng* pDocument)
	:
	BaseObj (ptCurrPos, pDocument),
	m_nAction (SE)
{
}

//------------------------------------------------------------------------------
SingleItemObj::SingleItemObj (const SingleItemObj& source)
	:
	BaseObj		(source),
	m_nAction	(source.m_nAction),
	m_BorderPen	(source.m_BorderPen)
{
}

//---------------------------------------------------------------------------
void SingleItemObj::LeftObject (int x)
{
	CRect   rectPrev = m_BaseRect;
	int     width = m_BaseRect.Width();

	m_BaseRect.left = x;
	m_BaseRect.right = m_BaseRect.left + width;

	if (rectPrev != m_BaseRect)
		m_pDocument->InvalidateRect(rectPrev, TRUE);
	m_pDocument->InvalidateRect(m_BaseRect);
}

//---------------------------------------------------------------------------
void SingleItemObj::TopObject (int y)
{
	CRect   rectPrev = m_BaseRect;
	int     height = m_BaseRect.Height();

	m_BaseRect.top = y;
	m_BaseRect.bottom = m_BaseRect.top + height;

	if (rectPrev != m_BaseRect)
		m_pDocument->InvalidateRect(rectPrev, TRUE);
	m_pDocument->InvalidateRect(m_BaseRect);
}

//------------------------------------------------------------------------------
void SingleItemObj::MoveObject(CSize offset)
{
	if (IsAnchored())
	{
		offset.cx = 0;
	}
	
	CRect   rectPrev = m_BaseRect;
	CRect	rectNew = m_BaseRect;
	           
	// accept only positive rectangle	         
	rectNew.OffsetRect(offset);
	if (rectNew.top < 0 || rectNew.left < 0)
		return;

	m_BaseRect = rectNew;
	if (rectPrev != m_BaseRect)
		m_pDocument->InvalidateRect(rectPrev, TRUE);
	m_pDocument->InvalidateRect(m_BaseRect);

	BOOL bPrevPersistent = m_bPersistent;
	m_bPersistent = TRUE;
	if (bPrevPersistent != m_bPersistent)
		m_pDocument->RefreshRSTree(ERefreshEditor::Layouts, this);
}

//------------------------------------------------------------------------------
void SingleItemObj::SizeObject(CSize size)
{
	RightSizeObject(size.cx);
	BottomSizeObject(size.cy);
}

//------------------------------------------------------------------------------
BOOL BadRectSize(CRect& baseRect, const CRect& rectPrev)
{
	if	(
			(baseRect.top < 0) || 
			(baseRect.left < 0) || 
			(baseRect.Width() <= MINCELL_X) || 
			(baseRect.Height() <= MINCELL_Y)
		)
	{
		baseRect = rectPrev;
		return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------
void SingleItemObj::RightSizeObject(int width)
{                                          
	if (IsAnchored())
		return;
	
	CRect rectPrev = m_BaseRect;
	m_BaseRect.right = m_BaseRect.left + width;

	if	(BadRectSize(m_BaseRect, rectPrev))
		return;

	if (rectPrev != m_BaseRect)
		m_pDocument->InvalidateRect(rectPrev, TRUE);
	m_pDocument->InvalidateRect(m_BaseRect);
}

//------------------------------------------------------------------------------
void SingleItemObj::BottomSizeObject(int height)
{
	if (IsAnchored())
		return;

	CRect rectPrev = m_BaseRect;
	m_BaseRect.bottom = m_BaseRect.top + height;

	if	(BadRectSize(m_BaseRect, rectPrev))
		return;

	if (rectPrev != m_BaseRect)
		m_pDocument->InvalidateRect(rectPrev, TRUE);
	m_pDocument->InvalidateRect(m_BaseRect);
}

//------------------------------------------------------------------------------
void SingleItemObj::LeftSizeObject(int width)
{   
	if (IsAnchored())
		return;

	CRect rectPrev = m_BaseRect;
	m_BaseRect.left = m_BaseRect.right - width;

	if	(BadRectSize(m_BaseRect, rectPrev))
		return;

	if (rectPrev != m_BaseRect)
		m_pDocument->InvalidateRect(rectPrev, TRUE);
	m_pDocument->InvalidateRect(m_BaseRect);
}

//------------------------------------------------------------------------------
void SingleItemObj::TopSizeObject(int height)
{
	if (IsAnchored())
		return;

	CRect rectPrev = m_BaseRect;
	m_BaseRect.top = m_BaseRect.bottom - height;

	if	(BadRectSize(m_BaseRect, rectPrev))
		return;

	if (rectPrev != m_BaseRect)
		m_pDocument->InvalidateRect(rectPrev, TRUE);
	m_pDocument->InvalidateRect(m_BaseRect);
}

//------------------------------------------------------------------------------
void SingleItemObj::Paint (CDC& DC, CPrintInfo* pInfo)
{                        
	// assuring mousemove in tracking mode, Paint is inhibit
	if (m_bInhibitPaint) 
		return;

	CRect rect = GetRectToInvalidate ();
	ScaleRect(rect, DC);
	/*
		DC.GetClipBox(&invalid):

			COMPLEXREGION Clipping region has overlapping borders.
			ERROR Device context is not valid.
			NULLREGION Clipping region is empty.
			SIMPLEREGION Clipping region has no overlapping borders. 
	*/
	CRect invalid;
	int nRet = DC.GetClipBox ( &invalid );

	// in stampa non gestisco l'invalidazione ma stampo comunque
	//if (pInfo || rect.IntersectRect(&rect, &invalid))
	BOOL bRet = (nRet == NULLREGION);
	if ( nRet == COMPLEXREGION || nRet == SIMPLEREGION )
		bRet = rect.IntersectRect(&rect, &invalid);

	if (pInfo || bRet)
	{
		Draw(DC, pInfo ? pInfo->m_bPreview : FALSE);
	}
	
	// copy bitmap info into clipboard if posted from user Action
	if (m_bPostedCopyClipboard)
		CopyClipboard();
}


/*

//------------------------------------------------------------------------------
void SingleItemObj::ClipActionCursor (CDC& dc, CPoint ptMousePos, CBCGPScrollView* pView)
{
	CRect   client;
	CRect   limit;
	CRect   rect (m_BaseRect);
	CPoint  point = ptMousePos;
	CPoint	scroll = pView->GetDeviceScrollPosition();
	CPoint	minp(MINCELL_X, MINCELL_Y);

	dc.LPtoDP(&rect); 
	dc.LPtoDP(&point);

	// minp must be normalized of scroll position because modified by LPtoDP
    dc.LPtoDP(&minp);
	minp.Offset	(scroll.x, scroll.y);
	
	pView->GetClientRect(client);
	int right   = client.Width();
	int bottom  = client.Height();
	
	switch (m_nAction)
	{
	case NORD:	limit = CRect(rect.left, 0, rect.right, rect.bottom - minp.y);			break;
	case SUD :	limit = CRect(rect.left, rect.top + minp.y, rect.right, bottom);		break;
	case EST :	limit = CRect(rect.left + minp.x, rect.top, right, rect.bottom);	    break;
	case WEST:	limit = CRect(0, rect.top, rect.right - minp.x, rect.bottom);			break;                                                                      
	case NW  : 	limit = CRect(0, 0, rect.right - minp.x, rect.bottom - minp.y); 		break;
	case NE  : 	limit = CRect(rect.left + minp.x, 0, right, rect.bottom - minp.y);		break;
	case SW  : 	limit = CRect(0, rect.top + minp.y, rect.right - minp.y, bottom); 		break;
	case SE  : 	limit = CRect(rect.left + minp.x, rect.top + minp.y, right, bottom);	break;
	default  : 	limit = CRect(point.x - rect.left, point.y - rect.top, right, bottom);	break;
	}

	pView->ClientToScreen (limit);
    ClipCursor(limit);
}
*/

//------------------------------------------------------------------------------
void SingleItemObj::ClipActionCursor (CDC& dc, CPoint ptMousePos, CBCGPScrollView* pView)
{
	CRect   client;
	CRect   limit;
	CRect   rect (m_BaseRect);
	CPoint  point = ptMousePos;
	CRect	minr(0, 0, MINCELL_X, MINCELL_Y);

	ScaleRect(rect, dc);
	ScaleRect(minr, dc);
	ScalePoint(point, dc);

	dc.LPtoDP(&rect); 
	dc.LPtoDP(&point);
    dc.LPtoDP(&minr);
	
	pView->GetClientRect(client);
	int right   = client.Width();
	int bottom  = client.Height();
	int minx	= minr.Width();
	int miny	= minr.Height();
	
	rect.IntersectRect(&rect, &client);
	switch (m_nAction)
	{
	case NORD		:	limit = CRect(rect.left, 0, rect.right, rect.bottom - miny);			break;
	case SUD		:	limit = CRect(rect.left, rect.top + miny, rect.right, bottom);			break;
	case EST		:	limit = CRect(rect.left + minx, rect.top, right, rect.bottom);	    	break;
	case WEST		:	limit = CRect(0, rect.top, rect.right - minx, rect.bottom);				break;                                                                      
	case NW			: 	limit = CRect(0, 0, rect.right - minx, rect.bottom - miny); 			break;
	case NE			: 	limit = CRect(rect.left + minx, 0, right, rect.bottom - miny);			break;
	case SW			: 	limit = CRect(0, rect.top + miny, rect.right - miny, bottom); 			break;
	case SE			: 	limit = CRect(rect.left + minx, rect.top + miny, right, bottom);		break;
	case DRAGGING	: 	limit = CRect(client.left,client.top, right, bottom);					break;
	default  : 	limit = CRect(point.x - rect.left, point.y - rect.top, right, bottom);	break;
	}

	pView->ClientToScreen (limit);
    ClipCursor(limit);
}

//------------------------------------------------------------------------------
BOOL SingleItemObj::InMoveArea (CPoint ptMousePos)
{
	return ItemAction (ptMousePos) == MOVE;
}

//------------------------------------------------------------------------------
void SingleItemObj::SetActionCursor (CPoint ptMousePos)
{
	ActionType tmpAction = ItemAction (ptMousePos);

	if  (
		(tmpAction == LEAVE) ||
		(tmpAction == MOVE)
		)
	{
		SetObjCursor (LEAVE);
		return;
	}

	SetObjCursor (tmpAction);
}

#define NUM_CURSORS 10
//------------------------------------------------------------------------------
static const HCURSOR* LoadCursors()
{   
	static HCURSOR NEAR rghCursors[NUM_CURSORS];
	// initilization of static cursor array for speed optimization
	// Note: all track cursors must live in same module
	HINSTANCE hInst = AfxFindResourceHandle(
		MAKEINTRESOURCE(IDC_ARROW_VERT), RT_GROUP_CURSOR);

	// initialize the cursor array
	rghCursors[0] = ::LoadCursor(NULL,  MAKEINTRESOURCE(IDC_ARROW));
	rghCursors[1] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_VERT));
	rghCursors[2] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_VERT));
	rghCursors[3] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_HORZ));
	rghCursors[4] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_HORZ));
	rghCursors[5] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_NW_SE));
	rghCursors[6] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_NE_SW));
	rghCursors[7] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_NE_SW));
	rghCursors[8] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_NW_SE));
	rghCursors[9] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_MOVE));

	return rghCursors;
}	


//------------------------------------------------------------------------------
void SingleItemObj::SetObjCursor(ActionType nAction)
{            
	ASSERT(nAction < NUM_CURSORS);
	// various GDI objects we need to draw
	static const HCURSOR* NEAR rghCursors = LoadCursors();

	::SetCursor(rghCursors[nAction]);
}

//------------------------------------------------------------------------------
void SingleItemObj::OnTrackingDraw(CDC& aDC, CBCGPScrollView* pView, BOOL bShowRect)
{
	m_pDocument->m_TrackRect = m_BaseRect;
	if (bShowRect)
	{
		CRect baseRect(m_BaseRect);
		ScaleRect(baseRect, aDC);
		aDC.Rectangle(baseRect);
	}

	TrackingCrossDraw(aDC, pView);
}

//------------------------------------------------------------------------------
void SingleItemObj::LineWidth()
{
	CLineWidthDlg dialog (m_BorderPen);
	if (dialog.DoModal() == IDOK)
	{
		m_pDocument->InvalidateRect(m_BaseRect);
		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
void SingleItemObj::SetSize ()
{
	CRect   rectPrev = m_BaseRect;
	CObjValueDlg dialog(rectPrev, FALSE, NULL, IsAnchored());

	if ((dialog.DoModal() == IDOK) && (rectPrev != m_BaseRect))
	{
		// refresh position and active indication
		m_pDocument->InvalidateRect (m_BaseRect, TRUE);
		m_pDocument->InvalidateRect (rectPrev);
		m_BaseRect = rectPrev;

		// refresh active status if this are current
		if (m_pDocument->m_pCurrentObj == this)
			m_pDocument->m_pActiveRect->SetActive (GetActiveRect());
			
		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}


//---------------------------------------------------------------------------
SingleItemObj::ActionType SingleItemObj::ItemAction (CPoint ptPoint)
{
	if (!m_BaseRect.PtInRect(ptPoint)) 
		return LEAVE;
	CRect rect (m_BaseRect);

	int xSens = Min ((int)(0.5 * rect.Width()), ACTION_SENS);
	int ySens = Min ((int)(0.5 * rect.Height()), ACTION_SENS);

	int x0c = rect.left     + xSens;
	int x1c = rect.right    - xSens;
	int y0c = rect.top      + ySens;
	int y1c = rect.bottom   - ySens;

	if (ptPoint.x < x0c)
	{
		if (ptPoint.y < y0c) 
			return NW;
		if (ptPoint.y > y1c) 
			return SW;
		return WEST;
	}

	if (ptPoint.x > x1c)
	{
		if (ptPoint.y < y0c) 
			return NE;
		if (ptPoint.y > y1c) 
			return SE;
		return EST;
	}

	if (ptPoint.y < y0c) 
		return NORD;
	if (ptPoint.y > y1c) 
		return SUD;

	return MOVE;
}


//---------------------------------------------------------------------------
BOOL SingleItemObj::Action (CPoint ptMousePos)
{
	m_nAction = ItemAction(ptMousePos);
	return (m_nAction != LEAVE);
}


//---------------------------------------------------------------------------
void SingleItemObj::Resize (ActionType CT, CPoint C, CPoint P)
{
	long&    x0 = m_BaseRect.left;
	long&    y0 = m_BaseRect.top;
	long&    x1 = m_BaseRect.right;
	long&    y1 = m_BaseRect.bottom;

	CRect   prev_rect = m_BaseRect;
	CSize   delta = C - P;

	switch (CT)
	{
	case MOVE: 
		m_BaseRect += CPoint(delta); 
		break;
	//--------------------------------------------------------
	case EST:
		m_BaseRect.right += delta.cx;
		break;
	case SUD:
		m_BaseRect.bottom += delta.cy;
		break;
	case WEST:
		m_BaseRect.left += delta.cx;
		break;
	case NORD:
		m_BaseRect.top += delta.cy;
		break;
	//--------------------------------------------------------
	case NE:
		m_BaseRect.right += delta.cx;
		m_BaseRect.top += delta.cy;
		break;
	case SE:
	case DRAGGING:
		m_BaseRect.right += delta.cx;
		m_BaseRect.bottom += delta.cy;
		break;
	case SW:
		m_BaseRect.left += delta.cx;
		m_BaseRect.bottom += delta.cy;
		break;
	case NW:
		m_BaseRect.left += delta.cx;
		m_BaseRect.top += delta.cy;
		break;
	default  :
		break;
	}

	// constrain object in positive axis
	if ((m_BaseRect.top < 0) || (m_BaseRect.left < 0))
		m_BaseRect = prev_rect;
}

//------------------------------------------------------------------------------
void SingleItemObj::BeforeAction ()
{
	m_pDocument->m_bTrackRectIndicatorActive = TRUE;
}


//------------------------------------------------------------------------------
void SingleItemObj::AfterAction ()
{
	m_pDocument->m_bTrackRectIndicatorActive = FALSE;
}


//------------------------------------------------------------------------------
BOOL SingleItemObj::NoBorders (BOOL printing)
{
	return 	m_pDocument->m_pOptions->NoBorders(m_pDocument, printing);
}

//------------------------------------------------------------------------------
BOOL SingleItemObj::ShowLabels (BOOL printing)
{
	return 	m_pDocument->m_pOptions->ShowLabels(m_pDocument, printing);
}


