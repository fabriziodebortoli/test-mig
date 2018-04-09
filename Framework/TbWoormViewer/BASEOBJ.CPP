
#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\LocalizableObjs.h>

#include <TbGenlib\reswalk.h>
#include <TbGeneric\dibitmap.h>
#include <TbGenlib\TbTreeCtrl.h>
#include <TbWoormViewer\RSEditorUI.h>

#include "bordpen.h"
#include "listdlg.h"
#include "woormini.h"
#include "woormvw.h"
#include "woormfrm.h"
#include "woormdoc.h"
#include "mulselob.h"
#include "Repeater.h"

#include "baseobj.h"
#include "baseobj.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

const CSize	BaseObj::DEFAULTSIZE(MINCELL_X, MINCELL_Y);
#define CROSS_LEN 800


//===========================================================================
//	virtual class ActiveRect
//===========================================================================

//---------------------------------------------------------------------------
ActiveRect::ActiveRect(BOOL nSingleSelection)
	:
	m_bSingleSelection(nSingleSelection),
	m_rectActive(0, 0, 0, 0),
	m_bLast(FALSE)
{

}


//===========================================================================
//	class OldActiveRect implementation
//===========================================================================

//------------------------------------------------------------------------------
OldActiveRect::OldActiveRect(BOOL nSingleSelection)
	:
	ActiveRect(nSingleSelection)
{
	CWalkBitmap bitmap;
	bitmap.LoadBitmap(IDB_BRUSH1);
		
	// draw four square box on rectangle corner
	COLORREF crPen = RGB(255, 0, 255);
	COLORREF crBrush = m_bSingleSelection ? crPen : RGB (255, 255, 255);
	COLORREF crBrushLast = RGB(0, 255, 255);

	m_pBrushBorder	= new CBrush;
	m_pBrushCorner	= new CBrush;
	m_pBrushLast	= new CBrush;
	m_pPen			= new CPen(PS_SOLID, 1, crPen);

	m_pBrushBorder->CreatePatternBrush(&bitmap);
	m_pBrushCorner->CreateSolidBrush(crBrush);              
	m_pBrushLast->CreateSolidBrush(crBrushLast);              
}

//------------------------------------------------------------------------------
OldActiveRect::~OldActiveRect()
{
	ASSERT(m_pBrushBorder);
	ASSERT(m_pBrushCorner);
	ASSERT(m_pBrushLast);
	ASSERT(m_pPen);

	delete	m_pBrushBorder;
	delete	m_pBrushCorner;
	delete	m_pBrushLast;
	delete	m_pPen;

	m_pBrushBorder	= NULL;
	m_pBrushCorner	= NULL;
	m_pBrushLast= NULL;
	m_pPen		= NULL;
}

//------------------------------------------------------------------------------
void OldActiveRect::Attach(CWoormDocMng* pDocument)
{
	m_pDocument = pDocument;
}
 
#define CT	ScalePix(BLACK_SQUARE)

//------------------------------------------------------------------------------
void OldActiveRect::Invalidate(CRect rect, BOOL bEraseBkg)
{
	rect.InflateRect(CT, CT);
	m_pDocument->InvalidateRect(rect, bEraseBkg);
}

//------------------------------------------------------------------------------
static void BrushRect 
	(
				CDC&		DC, 
		const	CRect&		rectSource, 
		const	CRect&		rectInvalid, 
				CBrush* 	pBrush, 
				CWoormView*	pView, 
				BOOL		bFill
	)
{
	CRect rect = rectSource;	
	
	if (rect.IntersectRect(&rect, &rectInvalid))	
	{   
		pBrush->UnrealizeObject();

		CPoint point = rect.TopLeft();		
		DC.LPtoDP (&point);
		pView->ClientToScreen (&point);
		DC.SetBrushOrg (point);	

		CBrush* pOldBrush = DC.SelectObject(pBrush);
		if (bFill) DC.FillRect (rect, pBrush); else DC.Rectangle(rectSource);
		DC.SelectObject(pOldBrush);
	}
}


//------------------------------------------------------------------------------
void OldActiveRect::Paint (CDC& DC, CPrintInfo* /*pInfo*/)
{           
	if (m_rectActive.IsRectNull() || m_rectActive.IsRectEmpty()) return;
	CWoormView*	pView = m_pDocument->GetWoormView();

	CRect rectInvalid;
	DC.GetClipBox(&rectInvalid);
		
	CRect	rect;
	CRect	tl, tr, bl, br;

	if (m_pDocument->m_pWoormIni->m_bTrackInside)
		m_rectActive.InflateRect(-CT, -CT);

	//						contorning rects
	tl = CRect (m_rectActive.left  - CT, m_rectActive.top  - CT, m_rectActive.right + CT, m_rectActive.top);
	tr = CRect (m_rectActive.right,      m_rectActive.top, 		 m_rectActive.right + CT, m_rectActive.bottom + CT);
	bl = CRect (m_rectActive.left  - CT, m_rectActive.bottom,    m_rectActive.right,      m_rectActive.bottom + CT);
	br = CRect (m_rectActive.left  - CT, m_rectActive.top,       m_rectActive.left,		m_rectActive.bottom);
														
	BrushRect(DC, tl, rectInvalid, m_pBrushBorder, pView, TRUE);
	BrushRect(DC, tr, rectInvalid, m_pBrushBorder, pView, TRUE);
	BrushRect(DC, bl, rectInvalid, m_pBrushBorder, pView, TRUE);
	BrushRect(DC, br, rectInvalid, m_pBrushBorder, pView, TRUE);
		

	//                      external square
	tl = CRect (m_rectActive.left  - CT, m_rectActive.top    - CT, m_rectActive.left,       m_rectActive.top);
	tr = CRect (m_rectActive.right,      m_rectActive.top    - CT, m_rectActive.right + CT, m_rectActive.top);
	bl = CRect (m_rectActive.left  - CT, m_rectActive.bottom,      m_rectActive.left,       m_rectActive.bottom + CT);
	br = CRect (m_rectActive.right,      m_rectActive.bottom,      m_rectActive.right + CT, m_rectActive.bottom + CT);

	CPen*	penOld	= DC.SelectObject(m_pPen);
	CBrush* pBrush = m_bLast && !m_bSingleSelection ? m_pBrushLast : m_pBrushCorner;
	
	BrushRect(DC, tl, rectInvalid, pBrush, pView, FALSE);
	BrushRect(DC, tr, rectInvalid, pBrush, pView, FALSE);
	BrushRect(DC, bl, rectInvalid, pBrush, pView, FALSE);
	BrushRect(DC, br, rectInvalid, pBrush, pView, FALSE);
	
	DC.SelectObject(penOld);

	if (m_pDocument->m_pWoormIni->m_bTrackInside)
		m_rectActive.InflateRect(CT, CT);
}

//------------------------------------------------------------------------------
void OldActiveRect::Clear(BOOL bUpdate/* = TRUE*/)
{
	if (!m_rectActive.IsRectNull() && !m_rectActive.IsRectEmpty())
		Invalidate (m_rectActive, TRUE);

	m_rectActive = CRect(0,0,0,0);
	m_originalRect = CRect(0, 0, 0, 0);

	if (bUpdate)
		m_pDocument->UpdateWindow();
}

//------------------------------------------------------------------------------
void OldActiveRect::Redraw()
{
	if (!m_rectActive.IsRectNull() && !m_rectActive.IsRectEmpty())
		Invalidate(m_rectActive, TRUE);
}

//------------------------------------------------------------------------------
BOOL OldActiveRect::SetActive(CRect rect, BOOL bLast)
{                            
	if ((rect == m_rectActive) && (m_bLast == bLast)) return FALSE;
	m_originalRect = rect;

	// set this m_rectActive is m_bLast in a multiple selection
	m_bLast = bLast;
	
	if (!m_rectActive.IsRectNull() && !m_rectActive.IsRectEmpty())
		Invalidate (m_rectActive, TRUE);

	if (!rect.IsRectNull() && !rect.IsRectEmpty())
		Invalidate (rect, FALSE);

	m_rectActive = rect;
	return TRUE;
}

//===========================================================================
//	class NewActiveRect implementation
//===========================================================================
//------------------------------------------------------------------------------
NewActiveRect::NewActiveRect(WoormIni* pWoormIni, BOOL nSingleSelection)
	:
	ActiveRect(nSingleSelection),
	m_pWoormIni(pWoormIni)
{
	m_crSelectionColor = m_pWoormIni->m_rgb_ObjectSelectionColor;
}

//------------------------------------------------------------------------------
NewActiveRect::NewActiveRect(WoormIni* pWoormIni, CWoormDocMng* pDocument, COLORREF rgb)
	:
	ActiveRect(TRUE),
	m_pWoormIni(pWoormIni)
{
	Attach(pDocument);
	m_crSelectionColor = rgb;
}

//------------------------------------------------------------------------------
NewActiveRect::~NewActiveRect()
{
}

//------------------------------------------------------------------------------
void NewActiveRect::Attach(CWoormDocMng* pDocument)
{
	m_pDocument = pDocument;
}

//------------------------------------------------------------------------------
void NewActiveRect::Invalidate(CRect rect, BOOL bEraseBkg)
{
	int nSelectionSize = m_pWoormIni->m_nObjectSelectionSize;
	rect.InflateRect(nSelectionSize, nSelectionSize);
	m_pDocument->InvalidateRect(rect, bEraseBkg);
}

//------------------------------------------------------------------------------
void NewActiveRect::Paint(CDC& DC, CPrintInfo* /*pInfo*/)
{
	if (m_rectActive.IsRectNull() || m_rectActive.IsRectEmpty())
		return;

	int nSelectionStyle = m_pWoormIni->m_eObjectSelectionLineStyle;
	int nSelectionSize = m_pWoormIni->m_nObjectSelectionSize;

	CPen*		pPen		= new CPen(nSelectionStyle, nSelectionSize, m_crSelectionColor);
	CPen*		penOld		= DC.SelectObject(&(*pPen));
	CGdiObject*	brushOld	= DC.SelectStockObject(NULL_BRUSH);

	DC.SetROP2(R2_COPYPEN/*R2_NOTXORPEN*/);

	CRect dpiRect(ScaleRect(m_rectActive));
	DC.Rectangle(dpiRect);

	DC.SelectObject(penOld);
	DC.SelectObject(brushOld);

	delete	pPen;
	pPen = NULL;
}


//------------------------------------------------------------------------------
void NewActiveRect::UpdateColor()
{
	m_crSelectionColor = m_pWoormIni->m_rgb_ObjectSelectionColor;
}

//------------------------------------------------------------------------------
void NewActiveRect::SetColor(COLORREF rgb)
{
	m_crSelectionColor = rgb;
}

//------------------------------------------------------------------------------
void NewActiveRect::Clear(BOOL bUpdate/* = TRUE*/)
{
	if (!m_rectActive.IsRectNull() && !m_rectActive.IsRectEmpty())
		Invalidate(m_rectActive, TRUE);
	
	m_rectActive = CRect(0, 0, 0, 0);
	m_originalRect = CRect(0, 0, 0, 0);

	if (bUpdate)
		m_pDocument->UpdateWindow();
}

//------------------------------------------------------------------------------
void NewActiveRect::Redraw()
{
	if (!m_rectActive.IsRectNull() && !m_rectActive.IsRectEmpty())
	{
		Invalidate(m_rectActive, TRUE);
		m_pDocument->UpdateWindow();
	}
}

//------------------------------------------------------------------------------
BOOL NewActiveRect::SetActive(CRect rect, BOOL bLast)
{
	if ((rect == m_rectActive) && (m_bLast == bLast)) return FALSE;
	m_originalRect = rect;

	int nSelectionSize = m_pWoormIni->m_nObjectSelectionSize;

	COLORREF m_crSelectionColor = m_pWoormIni->m_rgb_ObjectSelectionColor;

	if (m_pDocument->m_pWoormIni->m_bTrackInside)
		rect.DeflateRect(nSelectionSize, nSelectionSize);
	else
		rect.InflateRect(nSelectionSize, nSelectionSize);

	//set this m_rectActive is m_bLast in a multiple selection
	m_bLast = bLast;

	if (!m_rectActive.IsRectNull() && !m_rectActive.IsRectEmpty())
		Invalidate(m_rectActive, TRUE);
	
	if (!rect.IsRectNull() && !rect.IsRectEmpty())
			Invalidate(rect, TRUE);
	
	m_rectActive = rect;

	return TRUE;
}


//------------------------------------------------------------------------------
void ActiveRect::EnsureVisible()
{
	CWoormView* pWView = dynamic_cast<CWoormView*>(m_pDocument->GetFirstView());
	if (pWView)
	{
		int nHorzScrollPos = pWView->GetScrollPos(SB_HORZ);
		int nVertScrollPos = pWView->GetScrollPos(SB_VERT);

		CPoint pointToScroll(nHorzScrollPos, nVertScrollPos);
		CRect layoutRect;
		pWView->GetClientRect(&layoutRect);
		//sposto le coordinate considerando la scrollPosition
		layoutRect.MoveToXY(pointToScroll);

		//Horizontal
		if (m_rectActive.Width() > layoutRect.Width() || (m_rectActive.left < layoutRect.left))
			pointToScroll.x -= layoutRect.left - m_rectActive.left;
		else if (m_rectActive.right > layoutRect.right)
			pointToScroll.x += m_rectActive.right - layoutRect.right;
		//Vertical
		if (m_rectActive.Height() > layoutRect.Height() || (m_rectActive.top < layoutRect.top))
			pointToScroll.y -= layoutRect.top - m_rectActive.top;
		else if (m_rectActive.bottom > layoutRect.bottom)
			pointToScroll.y += m_rectActive.bottom - layoutRect.bottom;

		pWView->ScrollToPosition(pointToScroll);
		m_pDocument->UpdateWindow();
	}
	return;
}


//==============================================================================
//			Class GenericDrawObj implementation
//==============================================================================

//------------------------------------------------------------------------------
void GenericDrawObj::DrawBorder
	(
				CDC&		DC,
				CRect		rect,
		const	BorderPen&	borderPen,
				CBrush*		pBrush
	)
{
	CBrush br;	
	if (!pBrush)
	{
		pBrush = &br;
		br.CreateSolidBrush(borderPen.GetColor());
	}

	ScaleRect(rect, DC);
	DC.FillRect(rect, pBrush);
}

//------------------------------------------------------------------------------
void GenericDrawObj::DrawBorders
	(
				CDC&		DC,
				CRect		rect,
		const	BorderPen&	borderPen,
				Borders*	borders,
				BOOL		bNoBorders
	)
{
	if (bNoBorders) return;

	CBrush	brush;
	brush.CreateSolidBrush(borderPen.GetColor());
	
	int pen = borderPen.GetWidth();

	if (borders->top)		DrawBorder(DC, CRect (rect.left, rect.top, rect.right, rect.top + pen),			borderPen, &brush);
	if (borders->right)		DrawBorder(DC, CRect (rect.right - pen, rect.top, rect.right, rect.bottom),		borderPen, &brush);
	if (borders->bottom)	DrawBorder(DC, CRect (rect.left, rect.bottom - pen, rect.right, rect.bottom),	borderPen, &brush);
	if (borders->left)		DrawBorder(DC, CRect (rect.left, rect.top, rect.left + pen, rect.bottom),		borderPen, &brush);
}

//------------------------------------------------------------------------------
CRect GenericDrawObj::InsideRect
	(                           
				CDC&		/*DC*/,
				CRect		rectInside,
		const	BorderPen&	borderPen,
		const	Borders&	borders,
				BOOL		bNoBorders
	)
{
	return InsideRect(rectInside, borderPen, borders, bNoBorders);
}

//------------------------------------------------------------------------------
CRect GenericDrawObj::InsideRect
(
	CRect	rectInside,
	const	BorderPen&	borderPen,
	const	Borders&	borders,
	BOOL	bNoBorders
)
{
	if (!bNoBorders)
	{                           
		int pen = borderPen.GetWidth();
		
		if (borders.top)	rectInside.top		+= pen;
		if (borders.right)	rectInside.right	-= pen;
		if (borders.bottom)	rectInside.bottom	-= pen;
		if (borders.left)	rectInside.left 	+= pen;
	}
	return rectInside;
}

//------------------------------------------------------------------------------
void GenericDrawObj::DrawInside
	(
		CDC&		DC,
		CRect		rect,
		COLORREF	color
	)
{
	// draw the internal area
	//
	CBrush	brush;
	brush.CreateSolidBrush(color);
	
	// make scale translation
	ScaleRect(rect, DC);

	DC.FillRect(rect, &brush);
}


// evidenzia il rettangolo per indicare che puo' essere esportato. Non deve
// essere fatto se sto stampando
//------------------------------------------------------------------------------
void GenericDrawObj::DrawForExport 
	(
		CDC& DC,
		CRect rectIn,
		COLORREF clrText /* = DEFAULT_TEXTCOLOR */,
		BOOL bDrawAsTitleCol /* = FALSE */
	)
{
	CRect rect(rectIn);
	ScaleRect(rect, DC);
	rect.InflateRect(-1,0);
	if (!DC.IsPrinting())
	{		
		if (bDrawAsTitleCol)
		{
			CDC			DCMemory;
			CBitmap*	pBitmap	= new CBitmap;
			// create a new memory device context and a bitmap
			if	(
					!(DCMemory.CreateCompatibleDC(&DC)) ||
					!(pBitmap->CreateCompatibleBitmap(&DC, rect.Width(), rect.Height()))
				)
				return;

			CBitmap* pOldMemBmp = DCMemory.SelectObject(pBitmap);

			// copy the selected bitmap from display to memory DC
			// save the current content of the cell...
			DCMemory.BitBlt(0, 0, rect.Width(), rect.Height(), &DC, rect.left, rect.top, SRCCOPY);

			// fill the cell with diagonal brush
			CBrush	hbrDiagonal;
			hbrDiagonal.CreateHatchBrush(HS_FDIAGONAL, clrText);
			
			CBrush*	old_brush = DC.SelectObject(&hbrDiagonal);

			DC.FillRect(rect, &hbrDiagonal);

			DC.SelectObject(old_brush);

			// copy the saved bitmap from the memory DC to display
			// SRCAND: Combines pixels of the destination and source bitmaps using the Boolean AND operator.
			DC.BitBlt(rect.left, rect.top, rect.Width(), rect.Height(), &DCMemory, 0, 0, SRCAND);

			if (pOldMemBmp)
				DCMemory.SelectObject(pOldMemBmp);
			
			//delete pBitmap;
			::DeleteObject(pBitmap);
		}
		DC.InvertRect(rect);
	}
}

//------------------------------------------------------------------------------
/*
void GenericDrawObj::DrawRect
	(
				CDC&		DC,
				CRect		rect,
		const	BorderPen&	borderPen,
				COLORREF	color,
		const	Borders&	borders,
				BOOL		bNoBorders,
				BOOL		bOpaque
	)
{
	if (bOpaque)
		DrawInside (DC, InsideRect(DC, rect, borderPen, borders, bNoBorders), color);

	DrawBorders (DC, rect, borderPen, borders, bNoBorders);
}
*/
//-----------------------------------------------------------------------------------------
//usato dai command tipo: :OnResizeProportional ()
void GenericDrawObj::RectForDrawBitmap
	(
				CDC*		pDC,
				CRect&		rect,
				CRect&		rectSrc,
				CTBPicture*	pBitmap,
				BOOL		bIsCutted
	)
{   
	CDC pDC_local;
	if (pDC == NULL)
	{
		pDC_local.Attach(::GetDC(0));
		pDC = &pDC_local;
	}

	// make scale translation
	ScaleRect (rect, *pDC);

	if (pBitmap->IsBestFitSetted() && pBitmap->IsOk())
	{
		int nHBitmap;
		int nWBitmap;
		int nHInside;
		int nWInside;

		if (bIsCutted)
		{
			nHBitmap = rectSrc.Height();
			nWBitmap = rectSrc.Width(); 
		}
		else
		{
			nHBitmap = pBitmap->GetHeight();
			nWBitmap = pBitmap->GetWidth();
		}
		nHInside = rect.Height();
		nWInside = rect.Width(); 
		//----

		if ( ! (nHBitmap == nHInside && nWBitmap == nWInside) )
		{
			ASSERT(nHInside && nHBitmap && nWBitmap);

			int nY = (int) ( (double)nHBitmap / nWBitmap * nWInside );
			int nX = (int) ( (double)nWBitmap / nHBitmap * nHInside );

			if ( ( nY < nHInside && nX >= nWInside) || ( nY >= nHInside && nX < nWInside) )
			{ 
				if ( nY < nHInside )
				{
					rect.bottom = rect.top + nY;
				}
				if ( nX < nWInside )
				{
					rect.right = rect.left + nX;
				}
			}
		}
	}
}

//------------------------------------------------------------------------------
void GenericDrawObj::CalculateBitmapWithFitMode(
	CRect&		rectInside,
	CRect&		rectCuttedHereToFit,
	BOOL&		bHaveToCutted,
	CTBPicture*	pBitmap,
	BOOL		bTransparent,
	AlignType	align/*=(DT_LEFT | DT_TOP)*/,
	BOOL		bIsCuttedFromUser,
	CRect&		rectCuttedFromUser
	)
{
	switch (pBitmap->m_ImageFitMode)
	{
	case CTBPicture::ImageFitMode::NORMAL: //dimensione originale
	{
		//dimensione originale
		CSize originalSize(pBitmap->GetWidth(), pBitmap->GetHeight());

		//se l'immagine mi arriva ritagliata dall'utente non prendo la dimensione originale ma quella del ritaglio
		if (bIsCuttedFromUser)
		{
			originalSize.cx = rectCuttedFromUser.Width();
			originalSize.cy = rectCuttedFromUser.Height();
		}

		//posizione
		CRect originalRect;

		//in base alla dimensione originale dell'immagine (o del ritaglio definito dall'utente) che voglio andare a ritagliare, 
		//al rect dell'obj e all'alineamento, definisco in quale porzione del report voglio andare a disegnare l'immagine

		//gestione alignment horizontal
		if ((align & DT_CENTER) == DT_CENTER)
			originalRect.left = rectInside.CenterPoint().x - originalSize.cx / 2;
		else if ((align & DT_RIGHT) == DT_RIGHT)
			originalRect.left = rectInside.right - originalSize.cx;
		else//LEFT
			originalRect.left = rectInside.left;

		//gestione alignment vertical
		if ((align & DT_VCENTER) == DT_VCENTER)
			originalRect.top = rectInside.CenterPoint().y - originalSize.cy / 2;
		else if ((align & DT_BOTTOM) == DT_BOTTOM)
			originalRect.top = rectInside.bottom - originalSize.cy;
		else//TOP
			originalRect.top = rectInside.top;

		originalRect.right = originalRect.left + originalSize.cx;
		originalRect.bottom = originalRect.top + originalSize.cy;

		//controllo se devo effettuare ritagli
		if (originalRect.Height() > rectInside.Height() || originalRect.Width() > rectInside.Width())
		{
			//l'immagine sfora il rect che la contiene quindi effettuo un ritaglio
			bHaveToCutted = TRUE;
			
			//se l'immagine non mi arriva tagliata dall'utente, la taglio io "da zero"
			LONG x = 0;
			LONG y = 0;

			//gestione alignment horizontal
			if ((align & DT_CENTER) == DT_CENTER)
			{
				rectCuttedHereToFit.left = rectInside.left - originalRect.left;
				rectCuttedHereToFit.right = rectCuttedHereToFit.left + rectInside.Width();
			}
			else if ((align & DT_RIGHT) == DT_RIGHT)
			{
				rectCuttedHereToFit.left = rectInside.left - originalRect.left;
				rectCuttedHereToFit.right = originalRect.Width();
			}
			else //LEFT
			{
				rectCuttedHereToFit.left = 0;
				rectCuttedHereToFit.right = rectInside.Width();
			}

			//gestione alignment vertical
			if ((align & DT_VCENTER) == DT_VCENTER)
			{
				rectCuttedHereToFit.top = rectInside.top - originalRect.top;
				rectCuttedHereToFit.bottom = rectCuttedHereToFit.top + rectInside.Height();
			}
			else if ((align & DT_BOTTOM) == DT_BOTTOM)
			{
				rectCuttedHereToFit.top = rectInside.top - originalRect.top;
				rectCuttedHereToFit.bottom = originalRect.Height();
			}
			else //TOP
			{
				rectCuttedHereToFit.top = 0;
				rectCuttedHereToFit.bottom = rectInside.Height();
			}

			//altrimenti, se mi arriva già tagliata, devo tagliarla ulteriormente
			if (bIsCuttedFromUser)//else
			{
				LONG cuttedX = (originalRect.Width() -	rectCuttedHereToFit.Width())	< 0 ? 0 : (originalRect.Width() - rectCuttedHereToFit.Width());
				LONG cuttedY = (originalRect.Height() - rectCuttedHereToFit.Height())	< 0 ? 0 : (originalRect.Height() - rectCuttedHereToFit.Height());
				
				//gestione alignment horizontal
				if ((align & DT_CENTER) == DT_CENTER)
				{
					rectCuttedHereToFit.left = rectCuttedFromUser.left + cuttedX / 2;
					rectCuttedHereToFit.right = rectCuttedFromUser.right - cuttedX / 2;
				}
				else if ((align & DT_RIGHT) == DT_RIGHT)
				{
					rectCuttedHereToFit.left = rectCuttedFromUser.left + cuttedX;
					rectCuttedHereToFit.right = rectCuttedFromUser.right;
				}
				else //LEFT
				{
					rectCuttedHereToFit.left = rectCuttedFromUser.left;
					rectCuttedHereToFit.right = rectCuttedFromUser.right - cuttedX;
				}

				//gestione alignment vertical
				if ((align & DT_VCENTER) == DT_VCENTER)
				{
					rectCuttedHereToFit.top = rectCuttedFromUser.top + cuttedY / 2;
					rectCuttedHereToFit.bottom = rectCuttedFromUser.bottom - cuttedY / 2;
				}
				else if ((align & DT_BOTTOM) == DT_BOTTOM)
				{
					rectCuttedHereToFit.top = rectCuttedFromUser.top + cuttedY;
					rectCuttedHereToFit.bottom = rectCuttedFromUser.bottom;
				}
				else //TOP
				{
					rectCuttedHereToFit.top = rectCuttedFromUser.top;
					rectCuttedHereToFit.bottom = rectCuttedFromUser.bottom - cuttedY;
				}
				//in base al ritaglio ulteriore effettuato (solo componente orizzontale, solo verticale o entrambe) valorizzo il rect dove andare a disegnare l'immagine
				if (!cuttedX)
				{
					rectInside.left = originalRect.left;
					rectInside.right = originalRect.right;
				}
				else if (!cuttedY)
				{
					rectInside.top = originalRect.top;
					rectInside.bottom = originalRect.bottom;
				}
			}
		}
		else
			//non effettua ritagli quindi sostituisco il rettangolo dell'immagine originale a quello del rettangolo contenitore
			rectInside = originalRect;
		break;
	}
	case CTBPicture::ImageFitMode::BEST:
	{
		int nHBitmap;
		int nWBitmap;
		int nHInside;
		int nWInside;

		CRect rectBestFit = rectInside;

		//------------------------------vecchio algoritmo di fit del rect usato per trovare la dimensione (più che la posizione)
		if (bIsCuttedFromUser)
		{
			nHBitmap = rectCuttedFromUser.Height();
			nWBitmap = rectCuttedFromUser.Width();
		}
		else
		{
			nHBitmap = pBitmap->GetHeight();
			nWBitmap = pBitmap->GetWidth();
		}
		nHInside = rectInside.Height();
		nWInside = rectInside.Width();

		if ( ! (nHBitmap == nHInside && nWBitmap == nWInside) )
		{
			ASSERT(nHInside && nHBitmap && nWBitmap);

			int nY = (int) ( (double)nHBitmap / nWBitmap * nWInside );
			int nX = (int) ( (double)nWBitmap / nHBitmap * nHInside );

			if ( ( nY < nHInside && nX >= nWInside) || ( nY >= nHInside && nX < nWInside) )
			{ 
				if ( nY < nHInside )
				{
					rectBestFit.bottom = rectBestFit.top + nY;
				}
				if ( nX < nWInside )
				{
					rectBestFit.right = rectBestFit.left + nX;
				}
			}
		}

		//--------------------------------------------fine vecchio algoritmo e inizio centratura

		//ho trovato le coordinate del'img bestfit (rectBestFit) quindi ora anche la dimensione e posso allinearlo
		CSize sizeBestFit(rectBestFit.Width(), rectBestFit.Height());
		CRect rectWhereToDraw;

		//gestione alignment horizontal
		if ((align & DT_CENTER) == DT_CENTER)
			rectWhereToDraw.left = rectInside.CenterPoint().x - sizeBestFit.cx / 2;
		else if ((align & DT_RIGHT) == DT_RIGHT)
			rectWhereToDraw.left = rectInside.right - sizeBestFit.cx;
		else//LEFT
			rectWhereToDraw.left = rectInside.left;

		//gestione alignment vertical
		if ((align & DT_VCENTER) == DT_VCENTER)
			rectWhereToDraw.top = rectInside.CenterPoint().y - sizeBestFit.cy / 2;
		else if ((align & DT_BOTTOM) == DT_BOTTOM)
			rectWhereToDraw.top = rectInside.bottom - sizeBestFit.cy;
		else//TOP
			rectWhereToDraw.top = rectInside.top;

		rectWhereToDraw.right = rectWhereToDraw.left + sizeBestFit.cx;
		rectWhereToDraw.bottom = rectWhereToDraw.top + sizeBestFit.cy;

		rectInside = rectWhereToDraw;

		break;
	}
	default: //STRETCH
		break;
	}
}

//------------------------------------------------------------------------------
void GenericDrawObj::DrawBitmapWithFitMode
(
	CDC&		DC,
	CRect&		rectInside,
	CTBPicture*	pBitmap,
	BOOL		bTransparent,
	
	COLORREF	rgbBkgColor,
	AlignType	align/*=(DT_LEFT | DT_TOP)*/,
	BOOL		bIsCuttedFromUser,
	CRect&		rectCuttedFromUser
	)
{
	BOOL bHaveToCutted = FALSE;
	CRect rectCuttedHereToFit;

	//scalo per il device context appropriato
	ScaleRect(rectInside, DC);
	
	//colore di sfondo
	if (!bTransparent)
		DC.FillSolidRect(rectInside, rgbBkgColor);

	// Calculate image position & Size
	CalculateBitmapWithFitMode(rectInside, rectCuttedHereToFit, bHaveToCutted, pBitmap, bTransparent, align,	bIsCuttedFromUser, rectCuttedFromUser);

	//immagine ritagliata perchè troppo grossa rispetto al rettangolo che la deve contenere
	if (bHaveToCutted)
		DrawBitmap(DC, rectInside, rectCuttedHereToFit, pBitmap);
	//immagine ritagliata dall'utente
	else if (bIsCuttedFromUser)
		DrawBitmap(DC, rectInside, rectCuttedFromUser, pBitmap);
	//immagine intera
	else
		DrawBitmap(DC, rectInside, pBitmap);
}

//-----------------------------------------------------------------------------------------
void GenericDrawObj::DrawBitmap
	(
				CDC&		DC,
				CRect&		rectInside,
				CRect&		rectSrc,
				CTBPicture*	pBitmap,
				BOOL		bIsCutted,
				AlignType	align/*=(DT_LEFT | DT_TOP)*/
	)
{
	CRect rect = rectInside;

	if (rectSrc.IsRectEmpty())
		pBitmap->DrawPicture(DC, rect);
	else
		// ritaglio la parte di bitmap che mi serve
		pBitmap->DrawPicture(DC, rect, rectSrc);
}

//------------------------------------------------------------------------------
void GenericDrawObj::DrawBitmap
	(
				CDC&		DC,
				CRect&		rectInside,
				CTBPicture*	pBitmap,
				AlignType	align/*=(DT_LEFT | DT_TOP)*/
	)
{
	CRect rcNotUsed(0, 0, 0, 0);
	DrawBitmap ( DC, rectInside, rcNotUsed, pBitmap, FALSE, align );
}

//------------------------------------------------------------------------------
void GenericDrawObj::DrawBitmap
	(
				CDC&		DC,
				CRect&		rectInside,
				CRect&		rectSrc,
				CTBPicture*	pBitmap,
				AlignType	align/*=(DT_LEFT | DT_TOP)*/
	)
{
	DrawBitmap ( DC, rectInside, rectSrc, pBitmap, TRUE, align );
}

//------------------------------------------------------------------------------
BOOL GenericDrawObj::DrawBitmap
	(
				CDC&		DC,
				CRect&		rectInside,
				CTBPicture*	pBitmap,
		const	CString&	name,
				AlignType	align/*=(DT_LEFT | DT_TOP)*/
	)
{
	// skip if not exist bitmap or bad pointer
	if	(pBitmap && pBitmap->ReadFile(name, TRUE))
	{
		DrawBitmap (DC, rectInside, pBitmap, align);
		return TRUE;
	}

	return FALSE;
}

//------------------------------------------------------------------------------
void GenericDrawObj::DrawLine (CDC& DC, CPoint vt, int width, const BorderPen& borderPen)
{
	CRect	rect(vt, CSize(width, borderPen.GetWidth()));
	CPen	pen(PS_INSIDEFRAME, borderPen.GetWidth(), borderPen.GetColor());

	CPen*	penOld		= DC.SelectObject(&pen);
	CBrush*	brushOld	= (CBrush*) DC.SelectStockObject(NULL_BRUSH);

	// make scale translation
	ScaleRect(rect, DC);

	DC.Rectangle(rect);

	DC.SelectObject(penOld);
	DC.SelectObject(brushOld);
}

//==============================================================================
//			Class BaseObj implementation
//==============================================================================

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (BaseObj, IBaseObj)

//------------------------------------------------------------------------------
BaseObj::BaseObj(CPoint ptCurrPos, CWoormDocMng* pDocument)
	:
	m_pDocument				(pDocument),
	m_BaseRect				(ptCurrPos, DEFAULTSIZE),
	m_bPostedCopyClipboard	(FALSE),
	m_bTrackMode			(FALSE),
	m_bInhibitPaint			(FALSE),
	m_bFirstTime			(TRUE),
	m_bChanged				(FALSE),
	m_ptPrevPos				(0, 0),
	m_pRectSE				(NULL),
	m_rectStartMouseMove	(0,0,0,0),
	m_bTransparent			(FALSE),
	m_bInheritByTemplate	(FALSE),
	m_bPersistent			(TRUE),
	m_AnchorRepeaterID		(0),
	m_nRepeaterRow			(-1),
	IDisposingSourceImpl(this)
{
	ASSERT_VALID(m_pDocument);
	// associate a CWnd object for managing RButtonDown
	
	//non esiste ancora (il parse avviene nella OnOpenDocumentFile)
	//CWnd* pWnd = m_pDocument->GetWoormView(); 
	CWnd* pWnd = m_pDocument->GetFrame();
	if (pWnd)
		__super::Create(NULL, NULL, WS_CHILD | WS_CLIPSIBLINGS, CRect(0,0,0,0), pWnd, DUMMY_ID);

	//AddCreateUndoInfo ();	
}

//------------------------------------------------------------------------------
BaseObj::BaseObj (const BaseObj& source)
	:
	m_bPostedCopyClipboard	(source.m_bPostedCopyClipboard),
	m_bTrackMode			(source.m_bTrackMode),
	m_bInhibitPaint			(source.m_bInhibitPaint),
	m_bFirstTime			(source.m_bFirstTime),
	m_bChanged				(source.m_bChanged),
	m_ptPrevPos				(source.m_ptPrevPos),
	m_pRectSE				(source.m_pRectSE),
	m_rectStartMouseMove	(source.m_rectStartMouseMove),

	m_pDocument				(source.m_pDocument),
	m_BaseRect				(source.m_BaseRect),
	m_bTransparent			(source.m_bTransparent),
	m_bInheritByTemplate	(source.m_bInheritByTemplate),
	m_bPersistent			(source.m_bPersistent),
	m_AnchorRepeaterID		(source.m_AnchorRepeaterID),
	m_nRepeaterRow			(source.m_nRepeaterRow),
	IDisposingSourceImpl(this)
{
	m_wInternalID =	source.m_wInternalID;

	// associate a CWnd object for managing RButtonDown
	CWnd* pW;
	if (m_pDocument && (pW = m_pDocument->GetFrame()))
		__super::Create(NULL, NULL, WS_CHILD | WS_CLIPSIBLINGS, CRect(0,0,0,0), pW, DUMMY_ID);
}

//------------------------------------------------------------------------------
void BaseObj::Create (CWoormDocMng* pDocument)
{
	ASSERT_VALID(pDocument);
	m_pDocument = pDocument;

	__super::Create(NULL, NULL, WS_CHILD | WS_CLIPSIBLINGS, CRect(0,0,0,0), m_pDocument->GetWoormFrame(), DUMMY_ID);
}

// Deve essere implementato cosi` altrimenti ho un TRACE da parte di ~CWnd in wincore
// che vi consiglio di andare a guardare
//------------------------------------------------------------------------------
BaseObj::~BaseObj()
{
	DestroyWindow();
}
	
// save info for undo features (null rect mean delete object)
//------------------------------------------------------------------------------
void BaseObj::AddCreateUndoInfo()
{
	m_pDocument->m_pUndoStack->Push(new UndoInfo(this, m_rectStartMouseMove));
}

//------------------------------------------------------------------------------
CPoint BaseObj::StartTrackPoint()
{
	CSize s(1, 1);
	return (m_BaseRect.BottomRight() - s);
}
	
// save info for undo features
//------------------------------------------------------------------------------
void BaseObj::AddUndoInfo()
{
	m_pDocument->m_pUndoStack->Push(new UndoInfo(this, m_BaseRect));
}

//+--------------------------------------
//|      |       |
//|      |       |
//|      |       |
//|------+-------+-------
//|      |       |
//|------+-------+-------
//|      |       |
//|      |       |
//|      |       |
//
// bisogna tenere conto degli eventuali rettangoli della selezione
//------------------------------------------------------------------------------
void BaseObj::TrackingCrossDraw(CDC& aDC, CBCGPScrollView* pView)
{
	// disegna un crocicchio a partire dallo spigolo TopLeft
	if (m_pDocument->m_pWoormIni->m_bEnableTrackCross)
	{
		CRect baseRect(m_BaseRect);
		ScaleRect(baseRect, aDC);

		// devo tenere conto dell'eventuale scroll H/V
		CRect rectClient;
		pView->GetClientRect(rectClient);
		aDC.DPtoLP(rectClient);

		// linea a sinistra in alto
		aDC.MoveTo(0, baseRect.top);
		aDC.LineTo(baseRect.left - CT, baseRect.top);

		// linea a sinistra in basso
		aDC.MoveTo(0, baseRect.bottom - 1);
		aDC.LineTo(baseRect.left - CT, baseRect.bottom - 1);

		// linea in alto a sinistra
		aDC.MoveTo(baseRect.left, 0);
		aDC.LineTo(baseRect.left, baseRect.top - CT);

		// linea in alto a destra
		aDC.MoveTo(baseRect.right - 1,0);
		aDC.LineTo(baseRect.right - 1, baseRect.top - CT);

		// linea in basso a sinistra
		aDC.MoveTo(baseRect.left, baseRect.bottom + CT);
		aDC.LineTo(baseRect.left, rectClient.bottom);

		// linea in basso a destra
		aDC.MoveTo(baseRect.right - 1, baseRect.bottom + CT);
		aDC.LineTo(baseRect.right - 1, rectClient.bottom);

		// linea a destra in basso
		aDC.MoveTo(baseRect.right + CT, baseRect.bottom - 1);
		aDC.LineTo(rectClient.right, baseRect.bottom - 1);

		// linea a destra in alto
		aDC.MoveTo(baseRect.right + CT, baseRect.top);
		aDC.LineTo(rectClient.right, baseRect.top);

	}
}

//------------------------------------------------------------------------------
void BaseObj::TrackingDraw(CDC& aDC, CBCGPScrollView* pView, BOOL bShowRect)
{
	WoormIni* wrmIni = m_pDocument->m_pWoormIni;
	CPen	pen				(wrmIni->m_eTrackLineStyle, wrmIni->m_nTrackLineSize, wrmIni->m_rgb_TrackLineColor);
	CBrush*	brushOld	=	(CBrush*)	aDC.SelectStockObject(NULL_BRUSH);
	CPen*	penOld		=	aDC.SelectObject(&pen);
	aDC.SetROP2				(R2_NOTXORPEN);

	OnTrackingDraw(aDC, pView, bShowRect);
	
	aDC.SelectObject(brushOld);
	aDC.SelectObject(penOld);
}

//------------------------------------------------------------------------------
void BaseObj::LeftButtonDown (CDC& aDC, CPoint ptMousePos, CBCGPScrollView* pView)
{
	m_bTrackMode = TRUE;

	if (!Action(ptMousePos))
	{
		m_bTrackMode = FALSE;
		return;
	}

	// process procedure needed befor action is done (only if the are action !)
	BeforeAction();                                   

	// needed for link cursor with object variation
	ClipActionCursor(aDC, ptMousePos, pView);

	// set new cursor style (only possible if captured) for selected object
	SetObjCursor ();

	// set starting value for mouse position e rect dimension
	m_ptPrevPos = ptMousePos;
	m_rectStartMouseMove = m_BaseRect;
	AddUndoInfo();
		
	// set flags for managing modified objects
	m_bFirstTime	= TRUE;
	m_bChanged		= FALSE;

	// permette il repaint di tutti gli oggetti invalidi prima di processare
	// il tracking durante i mouse move
	pView->UpdateWindow();

	// Mette i crocicchi nella prima posizione per far vedere al primo
	// clic dove e` posizionato il rettangolo
	TrackingDraw(aDC, pView, FALSE);
}

//------------------------------------------------------------------------------
void BaseObj::MouseMove (CDC& aDC, CPoint ptMousePos, CBCGPScrollView* pView, BOOL bTransMode)
{
	CPoint mousePos(ptMousePos);
	UnScalePoint(mousePos, aDC);

	// avoid spure mousemove message with not real move
	if (m_ptPrevPos == mousePos)
		return;

	if (!m_bTrackMode)
	{
		// set cursor for signaling possible action when non tracking
		SetActionCursor (mousePos);
		return;
	}
	
	// resize only in m_bChanged position based on grid sensibility
	CSize sz = mousePos - m_ptPrevPos;

	int nGridX = m_pDocument->m_pWoormIni->m_bSnapToGrid ? m_pDocument->m_pWoormIni->m_nGridX : 1;
	int nGridY = m_pDocument->m_pWoormIni->m_bSnapToGrid ? m_pDocument->m_pWoormIni->m_nGridY : 1;

	// resiza solo se il mouse si muove oltre il grid setting
	if ((abs(sz.cx) >= nGridX) || (abs(sz.cy) >= nGridY))
	{
		// evita spostamenti che non siano snap to grid nella direzione
		if (abs(sz.cx) < nGridX)	
			mousePos.x = m_ptPrevPos.x;
		if (abs(sz.cy) < nGridY)	
			mousePos.y = m_ptPrevPos.y;

		// inihibit Paint during resize of object
		m_bInhibitPaint = TRUE;

		TrackingDraw(aDC, pView, !m_bFirstTime);
		m_bFirstTime = FALSE;



		Resize (mousePos, m_ptPrevPos);
		TrackingDraw(aDC, pView);

		m_ptPrevPos = mousePos;
		m_bChanged = TRUE;
	}

	//se sono un SqrRect, aggiorno il nodo del tree (per le coordinate e la dimensione)
	SqrRect* pSqr = dynamic_cast<SqrRect*>(this);
	if (pSqr)
	{
		HTREEITEM ht = m_pDocument->GetRSTree(ERefreshEditor::Layouts)->FindItemData((DWORD)this, m_pDocument->GetRSTree(ERefreshEditor::Layouts)->m_htLayouts);
		if (ht)
		{
			CNodeTree* pNode = (CNodeTree*)m_pDocument->GetRSTree(ERefreshEditor::Layouts)->GetItemData(ht);
			m_pDocument->GetRSTree(ERefreshEditor::Layouts)->UpdateRSTreeNode(pNode);
		}
	}
}

//------------------------------------------------------------------------------
void BaseObj::LeftButtonUp (CDC& aDC, CBCGPScrollView* pView)
{
	// work only in tackmode
	if (!m_bTrackMode) return;

	// end tracking mode and reenable object Paint
	m_bTrackMode = FALSE;
	m_bInhibitPaint = FALSE;

	// Pulisce i crocicchi nell'ultima posizione di Track.
	TrackingDraw(aDC, pView, FALSE);

	// process the action at the mouse up message (i.e.  add_row)
	AfterAction ();

	// needed for link cursor with object variation
	ClipCursor (NULL);

	// redraw only modified e se e' il caso riallinea alla griglia
	if ((!m_bFirstTime) && m_bChanged) 
		ChangedAction();
}

// redraw only modified object (m_bChanged only if move is over grid tic)
//------------------------------------------------------------------------------
void BaseObj::ChangedAction()
{
	ASSERT_VALID(m_pDocument);
	// in all point where m_BaseRect change, must deactivate old position
	// and activate it in new position
	if (m_rectStartMouseMove != m_BaseRect)
	{
		m_pDocument->InvalidateRect (m_rectStartMouseMove, TRUE);

		m_pDocument->GetObjects().ApplyRepeater();
	}

	m_pDocument->m_pActiveRect->SetActive(GetActiveRect());

	m_pDocument->InvalidateRect(this->GetRectToInvalidate());

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();      

	return;
}

//------------------------------------------------------------------------------
void BaseObj::UpdateRepeater()
{
	if (this->IsKindOf(RUNTIME_CLASS(Repeater)))
		return;

	CLayout* layout = &m_pDocument->GetObjects();
	for (int i = 0; i < layout->GetSize(); i++)
	{
		BaseObj* pO = (*layout)[i];
		if (pO->IsKindOf(RUNTIME_CLASS(Repeater)))
		{
			Repeater* pRep = (Repeater*)pO;

			CRect repRect = pRep->GetBaseRect();
			CRect objRect = GetBaseRect();

			BOOL bIsInsideRepeater =	repRect.left	<= objRect.left &&
										repRect.top		<= objRect.top &&
										repRect.right	>= objRect.right &&
										repRect.bottom	>= objRect.bottom;

			if ((bIsInsideRepeater && !pRep->Contains(this)) || (!bIsInsideRepeater && pRep->Contains(this)))
				ChangedAction();
		}
	}
}

//------------------------------------------------------------------------------
BOOL BaseObj::InMe (CPoint ptWhere)	
{   
	CRect rect (m_BaseRect);
	
	if (!m_pDocument->m_pWoormIni->m_bTrackInside)
		rect.InflateRect(BLACK_SQUARE, BLACK_SQUARE);

	if (m_nLayer != m_pDocument->m_nCurrentLayer)
		return FALSE;

	return rect.PtInRect(ptWhere);
	
}

//------------------------------------------------------------------------------
void BaseObj::ToggleTransparent()
{
	m_bTransparent = !m_bTransparent;

	m_pDocument->InvalidateRect(m_BaseRect, m_bTransparent);
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void BaseObj::ToggleTransparent(BOOL bTransparent)
{
	m_bTransparent = bTransparent;

	m_pDocument->InvalidateRect(m_BaseRect, m_bTransparent);
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
BOOL BaseObj::AssignData (WORD, RDEManager*)
{
	// default action is don't read anythings because base object has no ID
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL BaseObj::ExecCommand (WORD, RDEManager*)
{
	// default action is don't read anythings because base object has no ID
	return FALSE;
}


//------------------------------------------------------------------------------
void BaseObj::CopyClipboard(CRect rect)
{             
	CDC			memDC;
	CRect		rectClient, rectIntersect;
	CBitmap*	pBitmap	= new CBitmap;
	CWoormView*	pView	= m_pDocument->GetWoormView();
	CClientDC	viewDC	(pView);
	CClientDC	DC		(pView);
	
	// toggle posted copy for overcome border from m_rectActive object (caused by delayed Paint)
	m_bPostedCopyClipboard = FALSE;

	// calculate the rectangle area that to be copy in clipboard
	pView->BeginWaitCursor	();
	pView->GetClientRect	(&rectClient);
	
	pView->OnPrepareDC		(&viewDC);
	viewDC.LPtoDP			(&rect);
	rectIntersect.IntersectRect	(rectClient, rect);
						  
	// create the new DC in memory
	if	(
			!(memDC.CreateCompatibleDC(&DC)) ||
			!(pBitmap->CreateCompatibleBitmap(&DC, rectIntersect.Width(), rectIntersect.Height()))
		)
	{
		pView->EndWaitCursor();
		return;
	}                   
		
	memDC.SelectObject(pBitmap);
	// copy from DC display to DC memory the select bitmap
	memDC.BitBlt(0, 0, rectIntersect.Width(), rectIntersect.Height(), &DC, rectIntersect.left, rectIntersect.top, SRCCOPY);

	// copy in the clipboard the DIB bitmap
	if (pView->OpenClipboard())
	{
		EmptyClipboard();
		SetClipboardData
			(
				CF_DIB,
				BitmapToDIB
					(
						(HBITMAP) pBitmap->Detach(),
						(HPALETTE)GetSystemPaletteUse(DC.GetSafeHdc()), 0,0
					)
			);
		CloseClipboard();
	}
													 
	//delete pBitmap;
	::DeleteObject(pBitmap);
	pView->EndWaitCursor();
	
}


//------------------------------------------------------------------------------
void BaseObj::CopyClipboard()
{
	CopyClipboard(m_BaseRect);
}

//------------------------------------------------------------------------------
void BaseObj::UndoMoveAndSize (CRect rect)
{
	CSize offset = rect.TopLeft() - m_BaseRect.TopLeft();
	CSize size (rect.Width(), rect.Height());
	
	if (offset.cx || offset.cy)	MoveObject	(offset);
	if (size.cx && size.cy)	SizeObject	(size);
}

//---------------------------------------------------------------------------
void BaseObj::DrawDropShadow(CDC& DC, CRect& rect, int nDropShadowHeight, COLORREF crDropShadowColor)
{
	if (nDropShadowHeight > 0)
	{
		CRect rH (
			rect.left + nDropShadowHeight, 
			rect.bottom,  
			rect.right + nDropShadowHeight,
			rect.bottom + nDropShadowHeight
			);
		CRect rV (
			rect.right , 
			rect.top + nDropShadowHeight,  
			rect.right + nDropShadowHeight,
			rect.bottom + nDropShadowHeight
			);
						
		CBrush brush;
		brush.CreateSolidBrush(crDropShadowColor);
		CBrush*	old_brush = DC.SelectObject(&brush);
		
		
		ScaleRect(rH, DC);
		ScaleRect(rV, DC);
				
		DC.FillRect(rH, &brush);
		DC.FillRect(rV, &brush);

		DC.SelectObject(old_brush);
	}
}

