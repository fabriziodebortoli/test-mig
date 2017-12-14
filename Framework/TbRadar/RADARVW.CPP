
#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\dib.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\critical.h>
#include <TbGeneric\VisualStylesXp.h>

#include <TbGenlib\reswalk.h>
#include <TbGenlib\baseapp.h>
#include <TbGeneric\TBThemeManager.h>

#include "radardef.h"
#include "radardoc.h"
#include "radarfrm.h"
#include "radarvw.h"
#include "radarbar.h"
#include "radarbdy.h"

#include "radar.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static const TCHAR BASED_CODE szTagMultiLines	[]	= _T("NrMultiLines");
static const TCHAR BASED_CODE szTagEasyReading[]	= _T("EasyReading");
static const TCHAR BASED_CODE szRadVersion[]	= _T("RadVersion");

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CRadarView
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRadarView, CBCGPScrollView)

BEGIN_MESSAGE_MAP(CRadarView, CBCGPScrollView)
	//{{AFX_MSG_MAP(CRadarView)
	ON_WM_ERASEBKGND	()
	ON_WM_KEYDOWN		()
	ON_WM_KEYUP			()
	ON_WM_HSCROLL		()
	ON_WM_LBUTTONDOWN	()
	ON_WM_LBUTTONDBLCLK	()
	ON_WM_LBUTTONUP		()
	ON_WM_MOUSEMOVE		()
	ON_WM_SETCURSOR		()
	ON_WM_SIZE			()
	ON_WM_MOUSEWHEEL	()

	ON_WM_HSCROLL		()
	ON_WM_VSCROLL		()

	ON_COMMAND (ID_RADAR_LEFT,			OnLeft)
	ON_COMMAND (ID_RADAR_RIGHT,			OnRight)
	ON_COMMAND (ID_RADAR_PAGE_LEFT,		OnPageLeft)
	ON_COMMAND (ID_RADAR_PAGE_RIGHT,	OnPageRight)
	ON_COMMAND (ID_RADAR_CONTROL_HOME,	OnHome)
	ON_COMMAND (ID_RADAR_CONTROL_END,	OnEnd)

	ON_COMMAND (ID_RADAR_MINUS_LINES,	OnMinusMultiLines)
	ON_COMMAND (ID_RADAR_PLUS_LINES,	OnPlusMultiLines)
	ON_COMMAND (ID_RADAR_EASY_READING,	OnEasyReading)

	ON_UPDATE_COMMAND_UI (ID_RADAR_MINUS_LINES,	OnUpdateMinusMultiLines)
	ON_UPDATE_COMMAND_UI (ID_RADAR_PLUS_LINES,	OnUpdatePlusMultiLines)
	ON_UPDATE_COMMAND_UI (ID_RADAR_EASY_READING,OnUpdateEasyReading)

	ON_MESSAGE(UM_RADAR_SELECT, OnRadarSelect)
	ON_MESSAGE(UM_RADAR_MOVEROW, OnRadarMoveRow)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRadarView::CRadarView()
	:
	m_bInvertEasyReadingCr	(TRUE),
	m_bMousePosChanged		(FALSE),
	m_bCtrlKeyDown			(FALSE),
	m_bShiftKeyDown			(FALSE),
	m_bIsJustActivated		(FALSE),
	m_bEasyReading			(TRUE),
	m_bDrawXORFirstTime		(TRUE),
	m_nLPOldPosHScrollBar	(0),
	m_nOldActiveRow			(0),
	m_nRowWidth				(0),
	m_nRowHeight			(1),
	m_nNrVisibleRows		(1),
	m_nAction				(ARROW),
	m_LPPrevMousePos		(0,0),
	m_pRadarBody			(NULL),
	m_pRVFont				(NULL),
	m_nRadVersion			(RAD_VERSION),
	m_pVScrollBar			(NULL),
	m_iLastYOnSize			(0),
	m_iLastXOnSize			(0),
	m_lDragScroll			(0),
	m_iScrollMax			(0),
	m_bShowVerticalLine		(TRUE),
	m_bShowHorizontalLine	(FALSE)

{
	LoadBitmapOrPng(&m_MultiLines, TBGlyph(szIconMullines));
	m_bShowVerticalLine		= ! AfxGetThemeManager()->HideRadarSeparatorVertical();
	m_bShowHorizontalLine	= ! AfxGetThemeManager()->HideRadarSeparatorHorizontal();
}

//-----------------------------------------------------------------------------
CRadarView::~CRadarView()
{
	ASSERT_VALID(this);
	// Disabilitazione della clipping region.
	// La chiamata è necessaria in tutti quei casi in cui viene inpostata una
	// regione di clippind e l'utente decide di abbandonare brutalmente il radar
	ClipCursor (NULL);

	if (m_pRVFont)
	{
		VERIFY (m_pRVFont->DeleteObject());
		delete m_pRVFont;
	}

	if (m_pProcessingMouse)
	{
		delete m_pProcessingMouse;
		m_pProcessingMouse = NULL;
	}
	SAFE_DELETE(m_pVScrollBar);
}

// Procedura di disegno
//-----------------------------------------------------------------------------
void CRadarView::OnDraw(CDC* pDC)
{
	// Calcolo della ragione invalida su cui occorre disegnare
	CRect rectClip; pDC->GetClipBox (&rectClip);

	// Selezione dei vari oggetti GDI di memoria
	ASSERT_VALID (m_pRVFont);

	COLORREF crWnd		= AfxGetThemeManager()->GetBackgroundColor();

	CPen* pPen = new CPen (PS_SOLID, 1, AfxGetThemeManager()->GetRadarSeparatorColor());

	pDC->SetBkColor (crWnd);
	CFont*	pOldFont= pDC->SelectObject(m_pRVFont);
	CPen*	pOldPen	= pDC->SelectObject(pPen);

	if (m_pRadarBody->GetGrpTableNrRows() == 0)
		DoDrawEmptyBody(pDC, rectClip);
	else
	{
		// Disegno sul DC di memoria
		DoDraw (pDC, rectClip);
		m_nOldActiveRow = m_pRadarBody->GetActiveGrpRow();
	}
	// Ripristina i vecchi oggetti del GDI in memoria
	pDC->SelectObject	(pOldFont);
	pDC->SelectObject	(pOldPen);
	pPen->DeleteObject	();
	delete pPen;
}

// Scroll orizzonatle della CRadraView e contemporaneo aggiornamento della
// barra dei titoli
//-----------------------------------------------------------------------------
void CRadarView::OnHScroll (UINT nSBCode, UINT nPos, CScrollBar *pScrollBar)
{
	CBCGPScrollView::OnHScroll (nSBCode, nPos, pScrollBar);
	// Calcolo dell'offset di scroll da passare alla barra dei titoli
	TLBScroll ();
	SetVScrollBar();
}

//-----------------------------------------------------------------------------
void CRadarView::OnKeyScrollRowDown	()
{
	m_lDragScroll = m_pVScrollBar->GetScrollPos() + 1;
	if ((LONG) m_lDragScroll > m_iScrollMax) m_lDragScroll = m_iScrollMax;
	m_pVScrollBar->SetScrollPos(m_lDragScroll);
}

//-----------------------------------------------------------------------------
void CRadarView::OnKeyScrollRowUp		()
{
	int nScrollPos = m_pVScrollBar->GetScrollPos();
	if (nScrollPos > 0)
		m_lDragScroll = m_pVScrollBar->GetScrollPos() - 1;
	else
		m_lDragScroll = 0;

	if (m_lDragScroll <= 0 ) m_lDragScroll =0;
	m_pVScrollBar->SetScrollPos(m_lDragScroll);
}

//-----------------------------------------------------------------------------
void CRadarView::OnKeyScrollPageDown	()
{
	CRect rectClient; GetClientRect (rectClient);
	m_nNrVisibleRows = rectClient.Height() / m_nRowHeight;
	m_lDragScroll = m_pVScrollBar->GetScrollPos()  + m_nNrVisibleRows;
	if ((LONG)m_lDragScroll > m_iScrollMax) m_lDragScroll = m_iScrollMax;
	m_pVScrollBar->SetScrollPos(m_lDragScroll);
}
//-----------------------------------------------------------------------------
void CRadarView::OnKeyScrollPageUp		()
{
	CRect rectClient; GetClientRect (rectClient);
	m_nNrVisibleRows = rectClient.Height() / m_nRowHeight;

	if (m_pVScrollBar->GetScrollPos() > 0 && m_pVScrollBar->GetScrollPos() > m_nNrVisibleRows)
	{
		m_lDragScroll = m_pVScrollBar->GetScrollPos() - m_nNrVisibleRows;
	}
	else
	{
		m_lDragScroll = 0;
	}
	
	if (m_lDragScroll < 0) m_lDragScroll = 0;
	m_pVScrollBar->SetScrollPos(m_lDragScroll);
}
//-----------------------------------------------------------------------------
void CRadarView::OnKeyScrollPageEnd	()
{
	m_lDragScroll = m_iScrollMax ;
	m_pVScrollBar->SetScrollPos(m_lDragScroll);
}
//-----------------------------------------------------------------------------
void CRadarView::OnKeyScrollPageHome	()
{
	m_lDragScroll = 0;
	m_pVScrollBar->SetScrollPos(m_lDragScroll);
}

//-----------------------------------------------------------------------------
void CRadarView::OnVScroll (UINT nSBCode, UINT nPos, CScrollBar *pScrollBar)
{
	CRadarDoc* pDoc = GetDocument();
	
	// Calcolo della nuovo numero di righe visibili
	CRect rectClient; GetClientRect (rectClient);
	m_nNrVisibleRows = rectClient.Height() / m_nRowHeight;
	
	if (pScrollBar && pDoc)
	{
		int CurPos = m_pVScrollBar->GetScrollPos();
		// Determine the new position of scroll box.
		switch (nSBCode)
		{
			case SB_LINEUP:
				if (CurPos > 0) CurPos--;
				pDoc->RowUp();
				break;

			case SB_LINEDOWN:
				if (CurPos < m_iScrollMax) CurPos++;
				pDoc->RowDown();
				break;

			case SB_PAGEUP:
				// Get the page size. 
				if (CurPos > 0) CurPos = max(0, CurPos - m_nNrVisibleRows);
				else            CurPos = 0;

				pDoc->PageUp();
				break;

			case SB_PAGEDOWN:      // Scroll one page right
				// Get the page size. 
				if (CurPos < m_iScrollMax)
					CurPos = min(m_iScrollMax, CurPos + m_nNrVisibleRows);

				pDoc->PageDown();
				break;

			case SB_TOP:      // Scroll to far left.
				CurPos = 0;
				break;

			case SB_BOTTOM:      // Scroll to far right.
				CurPos = m_iScrollMax;
				break;

			case SB_ENDSCROLL:   // End scroll.
				break;

			case SB_THUMBPOSITION: // Scroll to absolute position. nPos is the position of the scroll box at the end of the drag operation.
				pDoc->GoRow(nPos);
				CurPos = nPos;
				break;

			case SB_THUMBTRACK:   // Drag scroll box to specified position. nPos is the position that the scroll box has been dragged to.
				break;
		}

		ASSERT(CurPos >= 0);

		m_lDragScroll = CurPos;
		//temporany position of scrollBar
		m_pVScrollBar->SetScrollPos(CurPos);
	}
}

// Memorizza l'azione di aver premuto il tasto SHIFT o CTRL
//-----------------------------------------------------------------------------
void CRadarView::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	switch (nChar)
	{
		case VK_SHIFT	: m_bShiftKeyDown	= TRUE;	break;
		case VK_CONTROL	: m_bCtrlKeyDown	= TRUE;	break;
	}
	CBCGPScrollView::OnKeyDown (nChar, nRepCnt, nFlags);
}

// Resetta i flag l'azione di aver premuto il tasto SHIFT o CTRL
//-----------------------------------------------------------------------------
void CRadarView::OnKeyUp (UINT nChar, UINT nRepCnt, UINT nFlags)
{
	switch (nChar)
	{
		case VK_SHIFT	: m_bShiftKeyDown	= FALSE; break;
		case VK_CONTROL	: m_bCtrlKeyDown	= FALSE; break;
	}
	CBCGPScrollView::OnKeyUp (nChar, nRepCnt, nFlags);
}

//-----------------------------------------------------------------------------
void CRadarView::OnLButtonDblClk(UINT nFlags, CPoint DPMousePos)
{
	CBCGPScrollView::OnLButtonDblClk (nFlags, DPMousePos);

	// Controlla se l'area critica non sia già bloccata
	// Il controllo sull'esistenza del puntatore è necessaria per eliminare il
	// problema di processare il messaggio a radar chiuso.
	if (!m_pProcessingMouse || m_pProcessingMouse->IsLocked())
		return;

	BOOL bStayAlive = GetDocument()->IsStayAlive();
	CPoint LPMousePos (DPMousePos);
	DPToLP (LPMousePos);

	if (bStayAlive)
		GetDocument()->SetWaiting(TRUE);
	if	(m_nAction == ARROW)
		if (!GetDocument()->DblClkRecordSelect (LPMousePos.y / m_nRowHeight))
			m_pProcessingMouse->Unlock();

	// Riabilita l'accesso all'area critica
	if (bStayAlive)
	{
		m_pProcessingMouse->Unlock();
		GetDocument()->SetWaiting(FALSE);
	}
}

//-------------------------- --------------------------------------------------
void CRadarView::OnLButtonDown(UINT nFlags, CPoint DPMousePos)
{
	CBCGPScrollView::OnLButtonDown(nFlags, DPMousePos);

	if (!CanDoLButtonDown ())
		return;

	// Controlla se l'area critica non sia già bloccata
	if (m_pProcessingMouse->IsLocked())
		return;

	CPoint LPMousePos (DPMousePos);
	DPToLP (LPMousePos);

	if (!LeftButtonDownExAction (LPMousePos))
		ChangeSelVisibleRow (LPMousePos.y);

	// Riabilita l'accesso all'area critica
	m_pProcessingMouse->Unlock();
}

//-----------------------------------------------------------------------------
void CRadarView::ChangeSelVisibleRow (int yCoord)
{
	GetDocument()->ChangeSelVisibleRow (yCoord / m_nRowHeight, m_bIsJustActivated);
}

//-----------------------------------------------------------------------------
void CRadarView::SetSelectedVisibleRow(int iRow)
{
	GetDocument()->ChangeSelVisibleRow(iRow, m_bIsJustActivated);
}
//-----------------------------------------------------------------------------
void CRadarView::OnLButtonUp(UINT nFlags, CPoint DPMousePos)
{
	CBCGPScrollView::OnLButtonUp(nFlags, DPMousePos);

	// Controlla se l'area critica non sia già bloccata
	if (m_pProcessingMouse->IsLocked())
		return;

	CPoint LPMousePos (DPMousePos);
	DPToLP		(LPMousePos);
	LeftButtonUp(LPMousePos);

	// Riabilita l'accesso all'area critica
	m_pProcessingMouse->Unlock();
}

//-----------------------------------------------------------------------------
void CRadarView::LeftButtonUp (CPoint& LPMousePos)
{
	if (!CanDoLButtonUp ())
	{
		ResetActionAndCursor();
		return;
	}

	if (m_bMousePosChanged)
	{
		CClientDC dc (this); OnPrepareDC (&dc);
		TrackingDraw (dc);
		GetDocument ()->SetWidthSelColumn (LPMousePos.x);
	}

	m_bMousePosChanged = FALSE;
	ResetActionAndCursor ();
}

//-----------------------------------------------------------------------------
void CRadarView::OnMouseMove(UINT nFlags, CPoint DPMousePos)
{
	CBCGPScrollView::OnMouseMove(nFlags, DPMousePos);

	// Controlla se l'area critica non sia già bloccata
	if (m_pProcessingMouse->IsLocked())
		return;

	// Ridisegno nella posizione corrente del mouse la nuova dimensione della colonna.
	CPoint LPMousePos (DPMousePos);
	DPToLP (LPMousePos);
	MouseMove (LPMousePos);

	// Riabilita l'accesso all'area critica
	m_pProcessingMouse->Unlock();
}

//-----------------------------------------------------------------------------
BOOL CRadarView::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT nMsg)
{
	if (!CanDoSetCursor ())
		return TRUE;

	// Se non sono sulla windows corrente e non sono nella cliente area devo utilizzare
	// la procedura di default.
	if (pWnd != this || nHitTest != HTCLIENT)
		return CBCGPScrollView::OnSetCursor(pWnd, nHitTest, nMsg);

	// Selezione del cursore e del relativo messaggio nella status bar in base alla
	// posizione del mouse
	CPoint DPMousePos;
	GetCursorPos	(&DPMousePos);
	ScreenToClient	(&DPMousePos);
	DPToLP			(DPMousePos);
	GetRadarFrame()->SetCursorAndMsgStatusBar (m_pRadarBody->GetCursorAction (DPMousePos));
	return TRUE;
}

//-----------------------------------------------------------------------------
void CRadarView::OnSize(UINT nType, int cx, int cy)
{
	//if (cy > 768)	
	//{
	//	int nBorder = 6;
	//	cx += nBorder;
	//	//diminuisce manifestarsi malfunzionamento RADAR in alta risoluzione (+ di 51*2 righe...)
	//	//non tiene conto della dimensione del Font: al peggio il radar con large font sarà più piccolo
	//	SetWindowPos( NULL, 0,0, cx, min(cy, 768), SWP_NOMOVE );
	//	GetRadarFrame()->SetWindowPos( NULL, 0,0, cx , min(cy, 768), SWP_NOMOVE|SWP_NOACTIVATE );
	//	return;
	//}

	if (cx > 0 && cy > 0 && m_pRadarBody && m_pRadarBody->GetCorrectCharWidth())
	{
		// Aggiornamente delle dimensioni per gestire lo scroll orizzontale
		UpdateScrollSizes();
		// Aggiornamente degli indice della righe visualizzatei
		GetDocument()->ShowCorrectRows();
		TLBScroll (FALSE);
	}

	CBCGPScrollView::OnSize (nType, cx, cy);
	m_iLastXOnSize = cx;
	m_iLastYOnSize = cy;

	SetVScrollBar();
}

//-----------------------------------------------------------------------------
void CRadarView::SetVScrollBar()
{
	if (m_pVScrollBar)
	{
		CSize csBar;
		int barSize = 0;
		int barBottom = m_iLastYOnSize;
		
		GetScrollBarSizes(csBar);
		barSize = csBar.cy;

		CRect rectVScroll(0,0,0,0);
		rectVScroll.top = 0;
		rectVScroll.bottom = barBottom;
		rectVScroll.left = m_iLastXOnSize - barSize;
		rectVScroll.right = m_iLastXOnSize;
		m_pVScrollBar->SetWindowPos(NULL, rectVScroll.left, rectVScroll.top, rectVScroll.Width(), rectVScroll.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);
				
		CRadarDoc* pDoc = GetDocument();
		if (pDoc && pDoc->m_nRowCount > 0)
		{
			m_iScrollMax = pDoc->m_nRowCount;
		}

		m_pVScrollBar->SetScrollRange(0, m_iScrollMax);
	}
}

//-----------------------------------------------------------------------------
void CRadarView::OnInitialUpdate()
{
	m_pProcessingMouse = new CriticalArea();

	DWORD dwCommonStyles = WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS;

	CRect rect(0,0,0,0);

	m_pVScrollBar = new  CTBScrollBar();
	if	(!m_pVScrollBar->Create(SBS_VERT | dwCommonStyles, rect, this, IDC_RA_VSCROLLBAR))
	{
		TRACE0("BodyEdit::CreateAccessories: failed to create vertical scroll bar");
		return;
	}
}

//-----------------------------------------------------------------------------
void CRadarView::OnUpdate(CView* pSender, LPARAM lHint, CObject* pHint)
{
	ASSERT (m_pRadarBody);
	// Resetto il flag che tiene memorizzata ogni volta che attivo il radar
	// quando è stay alive
	m_bIsJustActivated = FALSE;

	CRadarFrame*	pFrame	= GetRadarFrame();
	CRadarTitleBar*	pTBL	= pFrame->GetTitleBar();

	CRadarDoc* pDoc = GetDocument();
	int rRow = 0;

	switch ((int) HIWORD(lHint))
	{
		case UPDATE_VIEW:
			if (pDoc)
				rRow = pDoc->GetSqlRowActive();

			SetVScrollBar(); // For Query Editor

			if (rRow <= m_iScrollMax)
			{
				m_pVScrollBar->SetScrollPos(rRow);
			}
		break;

		case UPDATE_FROM_ROW_DOWN	: ScrollRowDown		();	return;
		case UPDATE_FROM_ROW_UP		: ScrollRowUp		();	return;

		case UPDATE_VIEW_SCROLL_TITLEBAR:
			// La sequenza di chiamate non deve essere modificata
			UpdateScrollSizes();
			pTBL->UpdateAndSetNewOrigin (UpdateOldPosHScrollBar());
			break;

		case UPDATE_SCROLL_SIZE:
			UpdateScrollSizes	();
			pTBL->UpdateWindow	();
			break;

		case UPDATE_SCROLL_SIZE_FORCE_REPAINT:
			UpdateScrollSizes	();
			pTBL->UpdateAndSetNewOrigin (UpdateOldPosHScrollBar());
			break;

		case UPDATE_FROM_PAGE_DOWN:
		case UPDATE_FROM_PAGE_UP:
			Invalidate		();
			break;

		case UPDATE_OPTIMIZATION:
		if (m_pRadarBody->GetGrpTableNrRows() > 0)
		{
			ASSERT (m_pRadarBody->GetActiveGrpRow() >= 0);
			ASSERT (m_nOldActiveRow >= 0);
	
			CRect rectClient; GetClientRect (rectClient);
			CClientDC dc (this);
			dc.DPtoLP (&rectClient);
			int nLFBOffset = pTBL->GetHeight();
	
			// Disegna la vecchia riga e la nuova selezionata 
			RedrawRow (m_nOldActiveRow, rectClient);
			RedrawRow (m_pRadarBody->GetActiveGrpRow(), rectClient);
			return;
		} 

		default:
			pTBL->UpdateWindow	();
	}
	CBCGPScrollView::OnUpdate(pSender, lHint, pHint);
}
//-----------------------------------------------------------------------------
void CRadarView::RedrawRow (int nIdxRow, CRect& rectClient)
{
	nIdxRow -= m_pRadarBody->GetFirstIdxGrpRow();

	ASSERT (nIdxRow >= 0);
	if (nIdxRow < 0)
		return;

	rectClient.top		= nIdxRow * m_nRowHeight;
	rectClient.bottom	= rectClient.top + m_nRowHeight;

	InvalidateRect	(rectClient);
	UpdateWindow ();
}

// Scrolla la finestra di un riga, verso il basso o verso l'alto (in base all'
// offeset imposto) invalidando prima di effetture l'operazione di scroll un
// invalidazione della riga attiva per evitare di scrollare la riga attiva per
// poi ridisegnarla disattiva con effetti di flicker
//-----------------------------------------------------------------------------
void CRadarView::ScrollRowDown ()
{
	// Calcolo dell'area da scrollare e di quella da invalidare
	CRect rect; GetClientRect(rect);
	int nRow = GetRowHeight();
	rect.bottom -= (rect.Height() % nRow);

	// Invalidazione del rettangolo della corrente riga attiva
	CRect rect1 (rect.left, rect.bottom - nRow, rect.right, rect.bottom);
	ASSERT_VALID (m_pRadarBody);
	InvalidateRect (rect1);

	ScrollWindow (0, -nRow, rect, rect);
	UpdateWindow ();
}

// Scrolla la finestra di un riga, verso il basso o verso l'alto (in base all'
// offeset imposto) invalidando prima di effetture l'operazione di scroll un
// invalidazione della riga attiva per evitare di scrollare la riga attiva per
// poi ridisegnarla disattiva con effetti di flicker
//-----------------------------------------------------------------------------
void CRadarView::ScrollRowUp ()
{
	// Calcolo dell'area da scrollare e di quella da invalidare
	CRect rect; GetClientRect(rect);
	int nRow = GetRowHeight();
	rect.bottom -= (rect.Height() % nRow);

	// Invalidazione del rettangolo della corrente riga attiva
	CRect rect1 (rect.left, rect.top, rect.right, rect.top + nRow);
	ASSERT_VALID (m_pRadarBody);
	InvalidateRect (rect1);

	ScrollWindow (0, nRow, rect, rect);
	UpdateWindow ();
}

// Calcola l'altezza della riga e la dimensione media di un carattere (in pixel)
// in base al font attualemente usato.
//-----------------------------------------------------------------------------
void CRadarView::SetRowHeight()
{
	ASSERT_VALID (m_pRVFont);
	ASSERT_VALID (m_pRadarBody);

	CSize cs;
	
	int nCharWidth = 0;

	CDC* pDC = GetDC();

	// il vecchio algoritmo non tiene conto del CharSet di esempio, mentre
	// il nuovo prende due caratteri e si fa ritornare la size vera 
	if (AfxGetCultureInfo()->GetCharSetSample().IsEmpty())
	{
		TEXTMETRIC tm;
		GetRadarTextMetrix (m_pRVFont, tm);

		// Calcola l'altezza della riga e la dimensione media di un carattere
		int nRadH = (tm.tmHeight + tm.tmDescent / 2) * m_pRadarBody->GetNrMultiLines();
		int nBodyH = ::GetEditSize(pDC, m_pRVFont, 1, m_pRadarBody->GetNrMultiLines() > 0 ? m_pRadarBody->GetNrMultiLines() : 1).cy - 1;
		m_nRowHeight = max(nRadH, nBodyH);

		nCharWidth = (int)((tm.tmAveCharWidth + tm.tmMaxCharWidth) / 1.5);
	}
	else
	{
		// ne chiedo due così tiene conto anche della spaziatura
		cs = GetTextSize(pDC, 2, m_pRVFont);

		m_nRowHeight = cs.cy * m_pRadarBody->GetNrMultiLines();
		nCharWidth = cs.cx / 2;
	}

	// la size del titolo ora viene calcolata sulla stringa vera
	CString sTitleChar = GetRadarFrame()->GetTitle();
	int nTitleCharWidth = 0;
	if (!sTitleChar.IsEmpty())
		nTitleCharWidth = GetTextSize (pDC, sTitleChar, m_pRVFont).cx / sTitleChar.GetLength();

	ReleaseDC(pDC);

	m_pRadarBody->SetCorrectCharWidth (nCharWidth, nTitleCharWidth);
}

// Aggiorna il rango della CRadarView (deriva da CBCGPScrollView) calcolando i nuovi
// valori dei scroll per la riga e la pagine (SOLO IN ORIZZONTALE) e aggiorna il
// numero di righe visibili, aggiornando eventualmente gli oggetti ad essa collegati.
// Viene chiamata dalla OnSize e tutte quelle volto che viene modificato un attributo
// grafico per è necessario ricalcolore i suddetti valori (Es.: cambio di font)
//-----------------------------------------------------------------------------
void CRadarView::UpdateScrollSizes()
{
	ASSERT_VALID (m_pRadarBody);
	CRect rectClient; GetClientRect (rectClient);

	// Calcolo della nuovo numero di righe visibili
	m_nNrVisibleRows = rectClient.Height() / m_nRowHeight;

	// Il minumo numero di righe visibili deve essere comunque 1
	if (m_nNrVisibleRows == 0)
		m_nNrVisibleRows = 1;

	// Aggiorno la dimensione della tabella dei dati grafici in base al nuovo numero di
	// righe visibili
	m_pRadarBody->SetSizeGrpTable (m_nNrVisibleRows);

	// Controllo che la coordinata finale dell'ultima colonna della tabella dei dati grafici
	// non sia maggiore del valore massimo amesso per lo scroll orizzontale dalla CBCGPScrollView
	int nSizeGrp = m_pRadarBody->GetGrpTableNrCols();
	if (nSizeGrp)
	{
		m_nRowWidth	= m_pRadarBody->GetColumn(nSizeGrp - 1)->m_nXEnd;
		if (m_nRowWidth >= INT_MAX - 1)
		    m_nRowWidth = INT_MAX - 1;
	}
	else
		m_nRowWidth = 0;

	// Setto il rango totale, di pagina e di riga per lo scroll in orizzontale.
	// ========================================================================
	// ATTENZIONE: LO SCROLL VERTICALE DEVE ESSERE SEMPRE DISABILITATO
	// ========================================================================
	
	int nScrollBarWidth = 0;
	if (m_pVScrollBar)
	{
		CRect cScrollBarRect;
		m_pVScrollBar->GetWindowRect(cScrollBarRect);
		nScrollBarWidth = cScrollBarRect.Width();
	}

	nScrollBarWidth = ((nScrollBarWidth > 0) ? nScrollBarWidth : 20);

	CSize sizeTotal	(m_nRowWidth + nScrollBarWidth, 0);
	if (rectClient.Width() < m_nRowWidth)
	{
		CSize sizePage	(rectClient.Width() - 10);
		CSize sizeLine	(m_pRadarBody->GetCorrectCharWidth() * 10, 0);
		SetScrollSizes	(MM_TEXT, sizeTotal, sizePage, sizeLine);
		return;
	}
	SetScrollSizes	(MM_TEXT, sizeTotal);
}

// Predispone il radar per eseguire una determinata azione
//-----------------------------------------------------------------------------
void CRadarView::StartAction (CPoint LPMousePos, int nRightBorder)
{
	// Imposto la regione di catura degli eventi di imput (mouse e tastiera)
	// alla view corrente
	SetCapture (); 

	// Conversione di qello che sarà il bordo sinisto della clipping region da LP in DP
	CPoint ptDPLeft(m_pRadarBody->GetXStartSelColumn() + m_pRadarBody->GetCorrectCharWidth() * COLUMN_CHAR_WIDTH_MIN_VALUE, 0);
	LPToDP (ptDPLeft);

	// Calcolo della regione di clipping
	CRect rectClient; GetClientRect (rectClient);
	rectClient.left = max (ptDPLeft.x, rectClient.left);
	// Calcolo del bordo di destra della regione di clippin
	if (nRightBorder)
	{
		CPoint ptDPRight (nRightBorder, 0);
		LPToDP (ptDPRight);
		rectClient.right = min (ptDPRight.x, rectClient.right);
    }
	ClientToScreen (&rectClient);
	// Incremento della regione di clipping
	rectClient.top -= GetRadarFrame()->GetTitleBar()->GetHeight();

	ClipCursor (&rectClient);
	// Selezione di un nuovo cursore e relativo messaggio nella status bar
	GetRadarFrame()->SetCursorAndMsgStatusBar (m_nAction);
	m_LPPrevMousePos = LPMousePos;

	// Selezione dei flag di modifica
	m_bMousePosChanged	= FALSE;
	m_bDrawXORFirstTime = TRUE;
}

//-----------------------------------------------------------------------------
void CRadarView::OnActivateView (BOOL bActive, CView* pView1, CView* pView2)
{
	CBCGPScrollView::OnActivateView (bActive, pView1, pView2);
	m_bIsJustActivated = bActive;
}

// Converte le coordinate da Device Point a Logical Point
//-----------------------------------------------------------------------------
void CRadarView::DPToLP (CPoint& LPPoint)
{
	CClientDC dc (this); OnPrepareDC (&dc);
	dc.DPtoLP (&LPPoint);
}

// Converte le coordinate da Logical Point a Device Point
//-----------------------------------------------------------------------------
void CRadarView::LPToDP (CPoint& DPPoint)
{
	CClientDC dc (this); OnPrepareDC (&dc);
    dc.LPtoDP (&DPPoint);
}

//-----------------------------------------------------------------------------
void CRadarView::DoDraw(CDC* pDC, CRect rectInvalid)
{
	// L'indice calcolato e quello assoluto nella client area, avendo considerato indice 0 quello
	// della prima riga visualizzata. Essendo il rettangolo rectInvalid trasformato da DP a LP l'indice
	// così calcolato dovrebbere essere l'indice assoluto all'interno dell'array di record formattati.
	// Nel nostro caso non abilitando lo scroll in verticale della CBCGPScrollView il valore così calcolato
	// è proprio l'indice che cerchiamo.
	// ATTENZIONE: SE VIENE ABILITATO LO SCROLL VERTICALE AGGIORNARE I CALCOLI
	int nIdx	= rectInvalid.top / m_nRowHeight;
	// Calcolo dell'indice all'interno dell'array di record formattati dell'ultima riga che occorre disegnare
	int nFirstIdx = m_pRadarBody->GetFirstIdxGrpRow();
	int nLastRow= nFirstIdx + min (rectInvalid.bottom / m_nRowHeight + 1, GetAvailableVisibleRows());
	// Coordinate assolute dell'attuale client area (NO SCROLL VERTICALE) da cui si inizia a disegnare
	int nYCoord	= nIdx * m_nRowHeight;
	int nRow	= nFirstIdx + nIdx;

	if (nYCoord >= rectInvalid.bottom || (!nRow && !rectInvalid.top && rectInvalid.Height() < m_nRowHeight))
	{
		// Utilizzato solo quando l'area viene invalidata dalla toolbar
		nRow == m_pRadarBody->GetActiveGrpRow() ? DrawRowActive (pDC, nRow, rectInvalid): DrawRowDeactive (pDC, nRow, rectInvalid);
		return;
	}
	// Disegna le righe del radar
	for	(; nYCoord < rectInvalid.bottom && nRow < nLastRow; nRow++)
		nYCoord = (nRow == m_pRadarBody->GetActiveGrpRow())
						? DrawRowActive		(pDC, nRow, rectInvalid)
						: DrawRowDeactive	(pDC, nRow, rectInvalid);

	if (nYCoord < rectInvalid.bottom)
	{
		// Disegna lo sfondo della parte non occupata dalle righe
		CBrush brush; brush.CreateSolidBrush(AfxGetThemeManager()->GetBackgroundColor());
		CRect rectFill (rectInvalid);
		rectFill.top = nYCoord;
		pDC->FillRect (&rectFill, &brush);
		VERIFY (brush.DeleteObject());

		// Disegna la parte non occupata dalle righe
		for (int nCol = 0; nCol < m_pRadarBody->GetGrpTableNrCols(); nCol++)
		{
			RadarColumn* pCol = m_pRadarBody->GetColumn (nCol);
			if (pCol->m_nXEnd < rectInvalid.left)
				continue;

			DrawSepLine	(pDC, pCol->m_nXEnd, nYCoord, pCol->m_nXEnd, rectInvalid.bottom);
			if (pCol->m_nXEnd > rectInvalid.right)
				break;
		}
		nYCoord += m_nRowHeight - 1;
		while (rectInvalid.bottom > nYCoord)
		{
			DrawSepLine	(pDC, rectInvalid.left, nYCoord, rectInvalid.right, nYCoord);
			nYCoord += m_nRowHeight;
		}
	}
}

//-----------------------------------------------------------------------------
LRESULT CRadarView::OnRadarSelect(WPARAM wParam, LPARAM lParam)
{
	GetDocument()->ChangeSelVisibleRow (wParam, m_bIsJustActivated);
	return GetDocument()->DblClkRecordSelect(wParam);
}

//-----------------------------------------------------------------------------
LRESULT  CRadarView::OnRadarMoveRow(WPARAM wParam, LPARAM lParam)
{
	GetDocument()->ChangeSelVisibleRow (wParam, m_bIsJustActivated);
	return 0L;
}



// Disegna il corpo del radar nel caso in cui le righe estratte da una query
// sono nulle
//-----------------------------------------------------------------------------
void CRadarView::DoDrawEmptyBody(CDC* pDC, CRect rectInvalid)
{
	CBrush brush; brush.CreateSolidBrush (AfxGetThemeManager()->GetBackgroundColor());
	pDC->FillRect (&rectInvalid, &brush);
	VERIFY (brush.DeleteObject());

	// Disegna le linee di separazione delle colonne
	for (int nCol = 0; nCol < m_pRadarBody->GetGrpTableNrCols(); nCol++)
	{
		RadarColumn* pCol = m_pRadarBody->GetColumn (nCol);
		if (pCol->m_nXEnd < rectInvalid.left)
			continue;
		if (this->m_bShowVerticalLine)
			DrawSepLine	(pDC, pCol->m_nXEnd, rectInvalid.top, pCol->m_nXEnd, rectInvalid.bottom);
		if (pCol->m_nXEnd > rectInvalid.right)
			break;
	}

	int nIdx	= rectInvalid.top / m_nRowHeight;
	int nYCoord	= nIdx ? nIdx * m_nRowHeight - 1 : m_nRowHeight - 1;

	// Disegna le linee di separazione delle righe
	while (nYCoord < rectInvalid.bottom)
	{
		if (this->m_bShowHorizontalLine)
			DrawSepLine(pDC, rectInvalid.left, nYCoord, rectInvalid.right, nYCoord);
		nYCoord += m_nRowHeight;
	}

	// Visualizza sul corpo del radar la scritta di tabella vuota
    CRadarDoc* pDoc = GetDocument ();
	CString strCaption	(pDoc->GetEmptyTableCaption	());
	CString strName		(pDoc->GetTableTitle		());

	CString msg = _TB("contains no records.");
	CRect rectDraw (0, 0, 0, 0);
    CRect rect; GetClientRect (rect);

	int nXCoord = rect.Width () / 2;
	nYCoord = rect.Height() / 4;

	pDC->SetTextAlign (TA_CENTER);
	pDC->ExtTextOut	(nXCoord, nYCoord, NULL, &rectDraw, strCaption, strCaption.GetLength(), NULL);
	nYCoord += m_nRowHeight;
	pDC->ExtTextOut	(nXCoord, nYCoord, NULL, &rectDraw, strName, strName.GetLength(), NULL);
	nYCoord += m_nRowHeight;
	pDC->ExtTextOut	(nXCoord, nYCoord, NULL, &rectDraw, msg, msg.GetLength(), NULL);
}

// Disegna un singola riga
//-----------------------------------------------------------------------------
int CRadarView::DrawRow (CDC *pDC, int nRow, const CRect& rect, COLORREF crBkgnd)
{
	ASSERT (nRow < m_pRadarBody->GetGrpTableNrRows() && nRow >= 0);

	int nIdxRow = nRow - m_pRadarBody->GetFirstIdxGrpRow();
	ASSERT (nIdxRow >= 0);

	// Riempie lo sfondo della riga da disegnare (è necessario in quanto con
	// la ExtTextOut non  vado a scrivere a file della riga di separazione delle
	// colonne, per cui rimarebbe fuori una zona non disegnata da nessuno)
	// Siccome la lettura facilitata deve essere visualizzata con colori solidi,
	// calcolo nella risoluzione attaule il colore solido che si avvicina di
	// più a quello impostati come default
	crBkgnd = pDC->GetNearestColor (crBkgnd);

	CRect rectDraw (rect.left, nIdxRow * m_nRowHeight, rect.right, (nIdxRow + 1) * m_nRowHeight);
	CBrush brushBkgrd; brushBkgrd.CreateSolidBrush (crBkgnd);
	pDC->FillRect (&rectDraw, &brushBkgrd);

	int nOldBkMode = pDC->SetBkMode (TRANSPARENT);
	// Disegna l'intera riga
	for (int nCol = 0; nCol < m_pRadarBody->GetGrpTableNrCols(); nCol++)
	{
		RadarColumn* pCol = m_pRadarBody->GetColumn (nCol);
		if (pCol->m_nXEnd < rect.left)
			continue;

		// Visualizza il contenuto della singola cella della colonna
		CString strCell ;
		BOOL bMultiLines = pCol->GetString (nRow, strCell);
		rectDraw.left	= pCol->m_nXStartOffset;
		rectDraw.right	= pCol->m_nXEndOffset;
		if (!bMultiLines)
			pDC->DrawText (strCell, strCell.GetLength(), &rectDraw, pCol->m_nAlign | DT_NOPREFIX);

		if (bMultiLines)
		{
			pDC->DrawText(strCell, strCell.GetLength(), &rectDraw, pCol->m_nAlign | DT_WORDBREAK | DT_NOPREFIX | DT_EDITCONTROL | DT_WORD_ELLIPSIS | DT_END_ELLIPSIS);
		
			int nWidth = 10;
			int nHeight = 10;

			CDC	dcImage;
			dcImage.CreateCompatibleDC(pDC);
			HGDIOBJ pOldObj;
			pOldObj = dcImage.SelectObject(m_MultiLines);

			// Set beckground in BitBlt
			COLORREF bckColorMask = dcImage.GetPixel(0, 0);
			COLORREF colorRead;
			for (int x = 0; x < nWidth; x++)
			{
				for (int y = 0; y < nHeight; y++)
				{
					colorRead = dcImage.GetPixel(x, y);
					
					if (GetRValue(bckColorMask) == GetRValue(colorRead) && 
						GetGValue(bckColorMask) == GetGValue(colorRead) &&
						GetBValue(bckColorMask) == GetBValue(colorRead)
						)
						{ 
							dcImage.SetPixel(x,y, crBkgnd);
						}
				}
			}

			pDC->BitBlt(pCol->m_nXEndOffset - 10, rectDraw.bottom - 11, nWidth, nHeight, &dcImage, 0, 0, SRCCOPY);
			dcImage.SelectObject(pOldObj);
			dcImage.DeleteDC();
		}

		// Disegna la riga di separazione tra le colonne
		if (this->m_bShowVerticalLine)
			DrawSepLine	(pDC, pCol->m_nXEnd, rectDraw.top, pCol->m_nXEnd, rectDraw.bottom);
		if (pCol->m_nXEnd > rect.right)
			break;
	}

	VERIFY (brushBkgrd.DeleteObject	());
	pDC->SetBkMode (nOldBkMode);

	// Disegna la riga di separazione tra le righe
	if (this->m_bShowHorizontalLine)
		DrawSepLine	(pDC, rect.left, rectDraw.bottom - 1, rect.right, rectDraw.bottom - 1);
	return rectDraw.bottom;
}

// Disegna la riga correntemente selezionata
//-----------------------------------------------------------------------------
int CRadarView::DrawRowActive (CDC *pDC, int nRow, const CRect& rect)
{
	// Prepare gli oggetti GDI per disegnare la riga attiva
	COLORREF crOldText = pDC->SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());
	int nYCoord = DrawRow(pDC, nRow, rect, AfxGetThemeManager()->GetBERowSelectedBkgColor());
	// Ripristina nel GDI i vecchi oggetti
	pDC->SetTextColor (crOldText);
	return nYCoord;
}

// Aggiorna le nuova e vecchia posizone per lo scroll della barra dei titoli
//-----------------------------------------------------------------------------
int CRadarView::UpdateOldPosHScrollBar()
{
	CRect rectClient; GetClientRect (rectClient);

	CPoint PointOldPos (m_nLPOldPosHScrollBar, 0);
	CPoint PointNewPos (rectClient.left, 0);

	// Conversioni
	LPToDP (PointOldPos);
	DPToLP (PointNewPos);

	// Aggiornamento della vecchia posizione
	m_nLPOldPosHScrollBar = PointNewPos.x;
	return (PointOldPos.x - rectClient.left);
}

// Predispone l'inserimento delle colonne cancellate tramite il mouse
//-----------------------------------------------------------------------------
void CRadarView::InsertColumnWithMouse()
{
	m_nAction = INS_DEL_COL;
	GetRadarFrame()->SetCursorAndMsgStatusBar (m_nAction);

	// Setta la regione di clipping
	CRect rectClient; 
	GetClientRect	(rectClient);
	ClientToScreen	(&rectClient);
	rectClient.top -= GetRadarFrame()->GetTitleBar()->GetHeight();
	ClipCursor (&rectClient);
}

// Viene utilizzata per l'XOR a video quando si ridimensiona la colonna tramite
// il mouse
//-----------------------------------------------------------------------------
void CRadarView::TrackingDraw (CDC& aDC, int nDrawMode)
{
	CRect rectClient; GetClientRect (rectClient);
	aDC.DPtoLP (&rectClient);

	// Disegno della riga tratteggiate nella vecchia e nella nuova posizione
	// (essendo in XOR disegnare uno stesso oggetto nella vecchia posizione
	// significa cancellarlo)
	aDC.MoveTo (m_LPPrevMousePos.x, rectClient.top);
	aDC.LineTo (m_LPPrevMousePos.x, rectClient.bottom);
	if (m_nAction == COL_RESIZE)
		GetRadarFrame()->GetTitleBar()->DrawLine(m_LPPrevMousePos.x, nDrawMode);
}

// Collega il RadarBody alla view e viceversa
//-----------------------------------------------------------------------------
void CRadarView::AttachBodyToView(RadarBody* pRadarBody)
{
	ASSERT_VALID (pRadarBody);
	m_pRadarBody = pRadarBody;
	// Collegata alla barra dei titoli e della barra a sinistra il RadarBody
	GetRadarFrame()->AttachRadarBodyToToolBar (m_pRadarBody);
}

// In base alla posizione corrente del mouse individua l'azione e la svole. 
//-----------------------------------------------------------------------------
BOOL CRadarView::LeftButtonDownExAction (CPoint& LPMousePos)
{
	// Sono in inserimento attraverso il mouse di colonna cancellate ?
	m_nAction = GetCursorAction (LPMousePos);
	if (m_nAction == INS_DEL_COL)
	{
		ResetActionAndCursor ();
		// Aggiungo la nuova colonna nella
		GetDocument()->AddDelColumnGrpTableFromMouse (LPMousePos);
		return TRUE;
	}

	// Se non sono in fase di modifica della dimensione della colonna abbandono
	if (m_nAction != COL_RESIZE)
		return FALSE;

	// Predispongo il radar per il cambiamento di dimensione della colonna
	StartAction (LPMousePos);
	return TRUE;
}

// Ritorna l'azione corrente o la nuova azione
//-----------------------------------------------------------------------------
SelectCursor CRadarView::GetCursorAction (CPoint& LPMousePos)
{
	// Controllo se sono in inserimento di una nuova colonna.
	return m_nAction == INS_DEL_COL
				? m_nAction
				: m_pRadarBody->GetCursorAction (LPMousePos);
}

// Aggiorna ai valori di default il cursore (FRECCIA - ARROW) e i messaggi
// nella status bar (Stringa vuota);
//-----------------------------------------------------------------------------
void CRadarView::ResetActionAndCursor ()
{
	ReleaseCapture	();
	ClipCursor		(NULL);
	m_nAction = ARROW;
	GetRadarFrame()->SetCursorAndMsgStatusBar ();
}

// Procedura che disegna la linea verticale tratteggiat quando si ridimensiona
// una colonna tramite il mouse.
//-----------------------------------------------------------------------------
void CRadarView::DrawVerticalLine (CPoint& LPMousePos, COLORREF crPen, int nPenStyle)
{
	if (m_LPPrevMousePos == LPMousePos || abs(LPMousePos.x - m_LPPrevMousePos.x) < 2)
		return;

	// Preparazionde del DC su cui operare
	CClientDC dc (this); OnPrepareDC (&dc);

	// Selezione degli oggetti grafici predefiniti nel DC
	CPen	pen	(nPenStyle, 1, crPen);
	CPen*	pOldPen = dc.SelectObject(&pen);

	// funzione di XOR sul DC
	// Cancellazione della linea precedente
	dc.SetROP2 (R2_NOTXORPEN);
	if (m_bDrawXORFirstTime)
		m_bDrawXORFirstTime = FALSE;
	else
		TrackingDraw(dc, R2_NOTXORPEN);

	// Visualizzazione della linea nella posizione corrente
	m_LPPrevMousePos = LPMousePos;
	TrackingDraw (dc, R2_NOTXORPEN);
	m_bMousePosChanged = TRUE;

	// Deselezione degli oggetti grafici nel DC
	dc.SelectObject (pOldPen);
	VERIFY (pen.DeleteObject());
}

// Crea un nuovo font in base alla struttura LOGFFONT ricavata dalla tabella
// degli stili dell'applicazione
//-----------------------------------------------------------------------------
void CRadarView::FontChanged (LOGFONT logFont)
{
	if (m_pRVFont)
	{
		// Cancellazione del vecchio font sia come oggetto GDI che C++
		VERIFY (m_pRVFont->DeleteObject());
		delete m_pRVFont;
		m_pRVFont = NULL;
	}
	// Creazione del nuovo font selezionato dall'utente nella combo
	// box nella toolbar grafica.
	CreateFontIndirect (logFont);


	// Aggiornamento dell coordinate di inizio e fine e attivazione del radar
	m_pRadarBody->UpdateColumnsStartEnd (0);
}

// Crea un nuovo font in base alla struttura LOGFFONT ricavata dalla tabella
// degli stili dell'applicazione
//-----------------------------------------------------------------------------
void CRadarView::CreateFontIndirect (LOGFONT logFont)
{
	ASSERT (m_pRVFont == NULL);

	// Creazione del nuovo font selezionato o dal file .INI o dalla combo
	// box nella toolbar grafica.
	m_pRVFont = ::CreateRadarFont(logFont);
	ASSERT_VALID (m_pRVFont);

	// Calcola l'altezza della riga
	SetRowHeight();

	CRadarFrame* pFrame = GetRadarFrame	();
	if (pFrame)
	{
		pFrame->GetTitleBar	()->FontChanged (logFont);
	}
}

// Aggiorna l'altezza di un riga
//-----------------------------------------------------------------------------
void CRadarView::UpdateRowHeight (BOOL bUpdate)
{
	if (!bUpdate)
		return;

	SetRowHeight		();
	UpdateScrollSizes	();
	GetDocument()->UpdateNrMultiLines(GetVisibleRows());
}

//-----------------------------------------------------------------------------
void CRadarView::ReadNoAskConfirm (LPCTSTR lpszSection, LPCTSTR lpszRadarFile)
{
	ASSERT (lpszSection);
	ASSERT (lpszRadarFile);
	ASSERT_VALID (m_pRadarBody);

	m_pRadarBody->SetNrMultiLines (::GetPrivateProfileInt(lpszSection, szTagMultiLines, MULTI_LINES_MIN, lpszRadarFile));

	m_bEasyReading = ::GetPrivateProfileInt(lpszSection, szTagEasyReading, TRUE, lpszRadarFile);

	m_nRadVersion = ::GetPrivateProfileInt(lpszSection, szRadVersion, 1, lpszRadarFile);
	
	if (m_bEasyReading)
		GetRadarFrame ()->UpdateButtonEasyReading (m_bEasyReading);
}

// Salva gli attributi senza richiedere conferma all'utente 
//-----------------------------------------------------------------------------
void CRadarView::WriteNoAskConfirm (LPCTSTR lpszSection, LPCTSTR lpszRadarFile)
{
	ASSERT (lpszSection);
	ASSERT (lpszRadarFile);
	ASSERT_VALID (m_pRadarBody);

	WriteRadarProfileInt (lpszSection, szTagMultiLines,	m_pRadarBody->GetNrMultiLines(), lpszRadarFile);
	WriteRadarProfileInt (lpszSection, szTagEasyReading, m_bEasyReading, lpszRadarFile);

	WriteRadarProfileInt (lpszSection, szRadVersion, RAD_VERSION, lpszRadarFile);
}

//-----------------------------------------------------------------------------
BOOL CRadarView::WriteRadarProfileInt (LPCTSTR lpszSection, LPCTSTR lpszEntry, int nValue, LPCTSTR lpszRadarFile)
{
	return ::WritePrivateProfileString (lpszSection, lpszEntry, cwsprintf (_T("%d"), nValue), lpszRadarFile);
}

// Ritorna il numero di righe viusalizzate prendendo il minimo tra il numero di
// righe della tabella dei dati grafici e le righe attaulmente visibili a video
//-----------------------------------------------------------------------------
int CRadarView::GetAvailableVisibleRows()
{
	ASSERT_VALID (m_pRadarBody);
	return min (m_nNrVisibleRows, m_pRadarBody->GetGrpTableNrRows());
}

// Aggiorna il flag, l'immagine sul bottone della toolbar grafica e imposta
// un repaint del corpo del radar
//-----------------------------------------------------------------------------
void CRadarView::OnEasyReading ()
{
	if (!GetRadarFrame ())
		return;

	m_bEasyReading = !m_bEasyReading;
	GetRadarFrame ()->UpdateButtonEasyReading (m_bEasyReading);
	Invalidate	();
	UpdateWindow();
}

// Controlla se sono in fase di inserimento di colonne cancellate tramite il mouse
// resettando eventualmente lo stato.
//-----------------------------------------------------------------------------
BOOL CRadarView::ResetInsertDeleteColumns ()
{
	if (m_nAction != INS_DEL_COL)
		return FALSE;

	ResetActionAndCursor();
	return TRUE;
}

//-----------------------------------------------------------------------------

COLORREF CRadarView::GetBkgndColor (int nRow)
{
	if (m_bEasyReading)
	{
		if (m_bInvertEasyReadingCr)
			return nRow % 2 ? AfxGetThemeManager()->GetEnabledControlBkgColor() : AfxGetThemeManager()->GetAlternateColor();

		return nRow % 2 ? AfxGetThemeManager()->GetAlternateColor() : AfxGetThemeManager()->GetEnabledControlBkgColor();
	}
	return AfxGetThemeManager()->GetEnabledControlBkgColor();
}

//------------------------------------------------------------------------------
/*
nFlags : Indicates whether various virtual keys are down. This parameter can be any combination of the following values: 
	MK_CONTROL   Set if the CTRL key is down. 
	MK_LBUTTON   Set if the left mouse button is down. 
	MK_MBUTTON   Set if the middle mouse button is down. 
	MK_RBUTTON   Set if the right mouse button is down. 
	MK_SHIFT	 Set if the SHIFT key is down. 
zDelta : Indicates distance rotated. 
	The zDelta value is expressed in multiples or divisions of WHEEL_DELTA, which is 120. 
	A value less than zero indicates rotating back (toward the user) 
	while a value greater than zero indicates rotating forward (away from the user). 
	The user can reverse this response by changing the Wheel setting in the mouse software. 
	See the Remarks for more information about this parameter. 
pt : Specifies the x- and y-coordinate of the cursor. These coordinates are always relative to the upper-left corner of the screen. 

*/
BOOL CRadarView::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt)
{
	if (zDelta > 0)
	{
		GetDocument()->OnRowUp ();
	}
	else if (zDelta < 0)
	{
		GetDocument()->OnRowDown ();
	}
	return TRUE;
}
	
///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarView::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarView\n");
	CBCGPScrollView::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//
//	Inlines
//
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
inline void CRadarView::DrawSepLine(CDC* pDC, int nXStart, int nYStart, int nXEnd, int nYEnd)
	{ 
		if (AfxGetThemeManager()->HideRadarSeparatorHorizontal() && nXStart != nXEnd) {	return;	}
		if (AfxGetThemeManager()->HideRadarSeparatorVertical() && nXStart == nXEnd)	{ return; }

		pDC->MoveTo (nXStart, nYStart); 
		pDC->LineTo (nXEnd, nYEnd); 
	}
inline void CRadarView::TLBScroll(BOOL bRedraw)
	{ GetRadarFrame()->GetTitleBar()->ScrollTitle (UpdateOldPosHScrollBar(), bRedraw); }
inline void CRadarView::OnMinusMultiLines ()
	{ if (m_pRadarBody) UpdateRowHeight (m_pRadarBody->DecreaseMultiLines());}
inline void CRadarView::OnPlusMultiLines ()
	{ if (m_pRadarBody) UpdateRowHeight (m_pRadarBody->IncreaseMultiLines()); }
inline void CRadarView::OnUpdateMinusMultiLines (CCmdUI* pCmd)
	{ pCmd->Enable (m_pRadarBody && m_pRadarBody->CanDoDecreaseMultiLines()); }
inline void CRadarView::OnUpdatePlusMultiLines (CCmdUI* pCmd)
	{ pCmd->Enable (m_pRadarBody && m_pRadarBody->CanDoIncreaseMultiLines());}
inline void CRadarView::OnUpdateEasyReading (CCmdUI* pCmd)
	{ pCmd->Enable (TRUE); pCmd->SetCheck (m_bEasyReading); }
