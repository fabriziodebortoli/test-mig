
#include "stdafx.h"

#include <afxpriv.h>

#include <TbGeneric\critical.h>
#include <TbGeneric\VisualStylesXp.h>
#include <TbGeneric\TBThemeManager.h>


#include "radardoc.h"
#include "radarfrm.h"
#include "radarvw.h"
#include "radarbar.h"
#include "radarbdy.h"
#include "radardlg.h"

#include "radar.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// L'ID della TitleBar deve essere nel range di MFC (FIRST-LAST) per poter godere
// del riposizionamento automatico.
//
#define AFX_IDW_TITLEBAR AFX_IDW_CONTROLBAR_LAST - 1

/////////////////////////////////////////////////////////////////////////////
// Implementazione di CRadarTitleBar
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRadarTitleBar, CBCGPControlBar)

BEGIN_MESSAGE_MAP(CRadarTitleBar, CBCGPControlBar)
	//{{AFX_MSG_MAP(CRadarTitleBar)
	ON_WM_KEYDOWN		()
	ON_WM_KEYUP			()
	ON_WM_LBUTTONUP		()
	ON_WM_MOUSEMOVE		()
	ON_WM_RBUTTONDOWN	()
	ON_WM_ERASEBKGND	()

	ON_WM_SIZE			()
	ON_WM_LBUTTONDOWN	()
	ON_WM_SETCURSOR		()
	ON_MESSAGE			(WM_GETFONT, OnGetFont)
	
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
CRadarTitleBar::CRadarTitleBar (CRadarFrame* pFrame)
	:
	m_pRadarBody		(NULL),
	m_pRBFont			(NULL),
	m_pProcessingMouse	(NULL),
	m_nHeight			(0),
	m_nXNewOrigin		(0),
	m_pRadFrame			(pFrame)
{
}

//-----------------------------------------------------------------------------
CRadarTitleBar::~CRadarTitleBar()
{
	if (m_pProcessingMouse)	delete m_pProcessingMouse;

	if (m_pRBFont)
	{
		VERIFY (m_pRBFont->DeleteObject());
		delete m_pRBFont;
	}
}
//-----------------------------------------------------------------------------
CSize CRadarTitleBar::CalcFixedLayout(BOOL bStretch, BOOL bHorz)
{
	ASSERT(bHorz);
	TEXTMETRIC tm;
	if (m_pRBFont)
	{
		GetRadarTextMetrix	(m_pRBFont, tm);
		m_nHeight = tm.tmHeight + tm.tmInternalLeading;
	}

	CSize m_size = __super::CalcFixedLayout(bStretch, bHorz);

    CRect rectSize;
    rectSize.SetRectEmpty();
    CalcInsideRect(rectSize, bHorz);       // will be negative size
    m_size.cy = m_nHeight - rectSize.Height();

	return m_size;
}

// Vale quando riportato nell'help di MFC
//-----------------------------------------------------------------------------
void CRadarTitleBar::OnLButtonDown(UINT nFlags, CPoint DPMousePos)
{
	__super::OnLButtonDown(nFlags, DPMousePos);

	// Protezione dell'area critica con blocco della stessa
	if (m_pProcessingMouse->IsLocked())
		return;

	// Processa l'attuale posizione del mouse all'interno delle barre
	LeftButtonDown (nFlags, DPMousePos);

	// Set Focus se si trova da un altra parte (Find Search Edit)
	if (GetRadarFrame())
	{
		GetRadarFrame()->SetFocus();
	}

	// Sblocco dell'area critica
	m_pProcessingMouse->Unlock();
}

// Vale quando riportato nell'help di MFC
//-----------------------------------------------------------------------------
BOOL CRadarTitleBar::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT)
{
	// Conversione della attuali coordinat del mouse per stabilire l'azione
	// da eseguire
	CPoint DPMousePos;
	GetCursorPos	(&DPMousePos);
	ScreenToClient	(&DPMousePos);

	if (!(pWnd == this && nHitTest == HTCLIENT && CursorMustChange (DPMousePos)))
		// Selezione del nuovo cursore e messaggio nella status bar
		GetRadarFrame()->SetCursorAndMsgStatusBar();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRadarTitleBar::Create(CWnd* pParentWnd)
{
	ASSERT(this->m_pRadFrame == pParentWnd);

	CRect rect; rect.SetRectEmpty();

	// save the style (some of these style bits are MFC specific)
	m_dwStyle = CBRS_TOP;

	if (!CWnd::Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, rect, pParentWnd, AFX_IDW_TITLEBAR))
	{
		TRACE0 ("CRadarTitleBar::Create failed.\n");
		return FALSE;
	}

	// Creazione del font di default per la barra
	m_pProcessingMouse	= new CriticalArea;

	// Creazione del brush per il disegno del bottone non selezionato
	m_pRBFont = ::CreateRadarDefaultFont(10);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CRadarTitleBar::FontChanged (LOGFONT logFont)
{
	if (m_pRBFont)
	{
		// Cancellazione del vecchio font sia come oggetto GDI che C++
		VERIFY (m_pRBFont->DeleteObject());
		delete m_pRBFont;
		m_pRBFont = NULL;
	}

	m_pRBFont = ::CreateRadarFont(logFont);
	ASSERT_VALID (m_pRBFont);
}

// Disegna una linea solida di spessore 1 pixel
//-----------------------------------------------------------------------------
void CRadarTitleBar::DrawSepLine (CDC* pDC, int nXS, int nYS, int nXE, int nYE, COLORREF cr)
{
	if (AfxGetThemeManager()->HideRadarSeparatorVertical())
	{
		return;
	}

	// Creazione e selezione della penna nel DC corrente
	CPen pen (PS_SOLID, 1, cr);
	CPen* pOldPen = pDC->SelectObject (&pen);
	// Disegna una riga 
	pDC->MoveTo (nXS, nYS);
	pDC->LineTo (nXE, nYE);

	// Selzione della vecchia penna nel DC e cancellazione di quella creata
	pDC->SelectObject (pOldPen);
	VERIFY (pen.DeleteObject ());
}

//-----------------------------------------------------------------------------
CRadarFrame* CRadarTitleBar::GetRadarFrame()
{
	CRadarFrame* pFrame = dynamic_cast<CRadarFrame*>(GetParent());
	ASSERT_VALID (pFrame);
	ASSERT_KINDOF(CRadarFrame, pFrame);
	ASSERT(this->m_pRadFrame == pFrame);
	return pFrame;
}

// Converte le coordinate device in logiche in accordo con l'attuale origine, essendo
// la barra scrollabile
//-----------------------------------------------------------------------------
void CRadarTitleBar::DPToLP (CPoint& DPMousePos)
{
	CClientDC dc (NULL);
	dc.SetViewportOrg	(m_nXNewOrigin, 0);
	dc.DPtoLP			(&DPMousePos);
}

// Viene richiamata dalla classe padre nel metodo OnSetCursor per impostare il
// nuovo cursore e il messaggio nella status bar a secondo della posizione del
// mouse. Se sono in fase di inserimento di un colonna cancella per mezzo del
// mouse non devo fare niente.
//-----------------------------------------------------------------------------
BOOL CRadarTitleBar::CursorMustChange (CPoint& DPMousePos)
{
	// Conversione di coordinata
	CPoint LPMousePos (DPMousePos);
	DPToLP (LPMousePos);

	SelectCursor nAction = GetRadarView()->GetCursorAction (LPMousePos);
	switch (nAction)
	{
		// Sono posizionato sulla riga di separazione tra due colonne, posso quindi
		// modificare la dimensione della prima colonna
		case COL_RESIZE	: GetRadarFrame()->SetCursorAndMsgStatusBar (nAction);
		case INS_DEL_COL: break;
		default:
			if (!m_pRadarBody->GetColumnFromPoint (LPMousePos))
				return FALSE;
				
			// Sono posizionato su di un titolo di colonna, allora posso selezionare o
			// deselezionare la colonna
			GetRadarFrame()->SetCursorAndMsgStatusBar(TIB_SEL_COL);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRadarTitleBar::OnEraseBkgnd(CDC* pDC)
{
	return TRUE;
}

// Disegna la barra dei titoli
//-----------------------------------------------------------------------------
void CRadarTitleBar::DoPaint(CDC* pDC)
{
	ASSERT_VALID (m_pRadarBody);
	ASSERT_VALID (m_pRBFont);

	// disegna i bordi
	//CControlBar::DoPaint(pDC);

	CRect rect; GetClientRect(rect);
	CBrush brush; brush.CreateSolidBrush (AfxGetThemeManager()->GetBackgroundColor());
	pDC->FillRect (&rect, &brush);
	pDC->SetBkColor(AfxGetThemeManager()->GetBackgroundColor());

	// Prepare il DC per il disegno, settanto l'origine, l'attributo dello sfondo,
	// l'allineamento del testo e il font desiderato.
	CRect rectClip; pDC->GetClipBox	(&rectClip);
	
	pDC->SetViewportOrg (m_nXNewOrigin, 0);
	
	pDC->DPtoLP			(&rectClip);

	pDC->SetTextAlign	(TA_CENTER);

	// Caricamento degli oggetti GDI per disegnare il titolo e suo visualizzazione
	CFont*	pOldFont = pDC->SelectObject (m_pRBFont);

	// Calcola l'area su cui dovrà essere disegnato il bottone e ne riempe la
	// relativa area.
	RadarColumn* pCol;
	//BOOL bNeedLastButton = TRUE;
	int h1 = rect.Height();
	int h2 = m_nHeight + 4;
	CRect rectHeader (0, 0, 0, h2);
	
	// Disegna le linee di separazione tra i bottoni/column header della griglia
	DrawSepLine(pDC, rectHeader.left, rectHeader.top, rectHeader.left, rectHeader.bottom, AfxGetThemeManager()->GetBESeparatorColor()); //COLOR_LINE_SEP

	for (int nIdx = 0; nIdx < m_pRadarBody->GetGrpTableNrCols(); nIdx++)
	{
		// Non disegno niente sino a quanto non rientro nel range di pixel visibili
		pCol = m_pRadarBody->GetColumn (nIdx);

		if (pCol->m_nXEnd < rectClip.left)
			continue;

		// Inizializzazione dei rettangoli per il paint
		CRect rectDraw(rectHeader);
		rectDraw.left	= pCol->m_nXStart + 2;	//chissa perchè +2 , comunque ci vuole
		rectDraw.right	= pCol->m_nXEnd + 2;
		rectDraw.bottom -= 1;

		// Disegna le linee di separazione tra i bottoni/column header della griglia
		// DrawSepLine(pDC, rectDraw.left - 1, rectDraw.top, rectDraw.right, rectDraw.top, AfxGetThemeManager()->GetBESeparatorColor());	//COLOR_LINE_SEP (azzurro)
		DrawSepLine(pDC, rectDraw.left - 1, rectDraw.bottom + 1, rectDraw.right, rectDraw.bottom + 1, AfxGetThemeManager()->GetBESeparatorColor());
		DrawSepLine(pDC, rectDraw.right, rectDraw.top, rectDraw.right, rectDraw.bottom, AfxGetThemeManager()->GetBESeparatorColor());

		rectDraw.top += 1;

		if (pCol->IsSelected ())
		{
			pDC->FillSolidRect(&rectDraw, AfxGetThemeManager()->GetRadarTitleBarSelectedButtonBkgColor() ); //COLOR_SELECTED  (arancio)

			rectDraw.left += 2;
			rectDraw.top += 1;
			rectDraw.right -= 2;
			rectDraw.bottom -= 1;
		}
		else 
			rectDraw.bottom -= 1;

		pDC->FillSolidRect(&rectDraw, AfxGetThemeManager()->GetBackgroundColor() );

		CString	Title (pCol->GetTitle ());

		//----
		pDC->SetBkMode	(TRANSPARENT);
		pDC->ExtTextOut	(
							(rectDraw.left + rectDraw.right) / 2, rectDraw.top + 2, //(rectDraw.top + rectDraw.bottom) / 2,
							ETO_CLIPPED, &rectDraw,	Title, Title.GetLength(), NULL
						);
		pDC->SetBkMode	(OPAQUE);

		// La coordinata finale del bottone supera le dimensioni della client area 
		// allora abbandono la paint senza necessita' di disegnare un bottone fittizio
		//if (pCol->m_nXEnd >= rectClip.right)
		//{
		//	bNeedLastButton = FALSE;
		//	break;
		//}
	}

	// Disegna un falso bottone nella barra dei titoli, dove questi vengono meno
	// e se di dimensioni accettabili, lasciando il solito pixel di separazione tra i bottoni
	//if (bNeedLastButton)
	//{
	//	CRect rectClient; GetClientRect (rectClient);
	//	pDC->DPtoLP (&rectClient);
	//	CRect lastRect (pCol->m_nXEnd + 2 + 1, 2, rectClient.right - 1, m_nHeight + 4);
	//	if (lastRect.Width() > 4)
	//		pDC->DrawFrameControl(&lastRect, DFC_BUTTON, DFCS_FLAT);
	//}

	// Deselezione degli oggetti GDI precedentemente selezionati nel DC
	pDC->SelectObject (pOldFont);
	VERIFY (brush.DeleteObject());
}

// Processa il messagio OnLButtonDown, e a secondo della posizione del mouse seleziona
// o deseleziona la colonna
//-----------------------------------------------------------------------------
void CRadarTitleBar::LeftButtonDown (UINT nFlags, CPoint& DPMousePos)
{
	CPoint	LPMousePos	(DPMousePos);
	DPToLP (LPMousePos);

	CRadarDoc* pDoc = GetRadarFrame ()->GetRadarDocument();

	// La sequenza dell'if non deve essere cambiata. Primo controllo se l'azione, in base
	// allo stato attuale, viene seguita. Se ricevo una risposta negativa allora controllo
	// se posso deselezionare la colonna o le colonne
	if (GetRadarView()->LeftButtonDownExAction (LPMousePos))
		return;
	if (!pDoc->CanDoSelOrDesCols ())
		return;

	int nCol = m_pRadarBody->FindColumn(LPMousePos);
	if (nCol < 0)
		return;

	DataType dataType;
	if (nFlags & MK_CONTROL)
	{
		// E' attualmente premuto il tasto di ctrl, eseguo una selezione tipo file manager
		dataType = m_pRadarBody->CtrlKeyDownEnable (nCol);

		// Aggiorno la condizione per la ricerca stupida
		if (dataType == DATA_NULL_TYPE)
			pDoc->FindStupidDisable ();
		else
			pDoc->FindStupidReset (dataType);

		// Invalidazione della singola colonna
		RadarColumn* pCol = m_pRadarBody->GetColumn (nCol);

		InvalidateUpdateRect (pCol->m_nXStart + m_nXNewOrigin,  pCol->m_nXEnd + m_nXNewOrigin);
		return;
	}

	int nXStart, nXEnd;
	dataType = (nFlags & MK_SHIFT)
					? m_pRadarBody->ShiftKeyEnable	(nCol, nXStart, nXEnd)
					: m_pRadarBody->SelectColumn	(nCol, nXStart, nXEnd);

	if (dataType == DATA_NULL_TYPE)
	{
		InvalidateUpdateRect	(nXStart, nXEnd);
		return;
	}

	pDoc->FindStupidReset	(dataType);

	InvalidateUpdateRect	(nXStart, nXEnd);
}

// Seleziona le colonna da ridisegnare per ottimizzare la procedura di paint
//-----------------------------------------------------------------------------
void CRadarTitleBar::InvalidateUpdateRect (int nXStart, int nXEnd)
{
	if (nXStart >= nXEnd)
		return;

	CRect rectDraw (nXStart + m_nXNewOrigin - 1, 0, nXEnd + m_nXNewOrigin + 2, m_nHeight + 4);

	InvalidateRect (rectDraw);
	UpdateWindow ();
}

// Disegna la riga tratteggiate quando esegue il ridimensionamento della colonna
// tramite il mouse.
//-----------------------------------------------------------------------------
void CRadarTitleBar::DrawLine (int nXPos, int nDrawMode)
{ 
	// Preparo il DC per il paint
	CRect		rectClip;
	CClientDC	dc (this);

	dc.GetClipBox		(rectClip);
	dc.SetViewportOrg	(m_nXNewOrigin, 0);
	dc.DPtoLP			(&rectClip);

	// Crea la penna per linea a tratteggi
	CPen	pen	(PS_DOT, 1, AfxGetThemeManager()->GetRadarTitleBarLineColor() );
	CPen*	pOldPen = (CPen*) dc.SelectObject(&pen);

	// Setto il DC in modo XOR
	if (nDrawMode == R2_NOTXORPEN)
		dc.SetROP2 (R2_NOTXORPEN);

	// Disegno la riga tratteggiata
	dc.MoveTo (nXPos + 2, rectClip.top);
	dc.LineTo (nXPos + 2, rectClip.bottom);

	dc.SelectObject (pOldPen);
	VERIFY (pen.DeleteObject());
}

// Prepare lo spostamento a sinistra o a destra in accordo con la CRadarView.
// Se il flag di redraw è attivo aggiorno il contenuto.
//-----------------------------------------------------------------------------
void CRadarTitleBar::ScrollTitle(int nDPOffset, BOOL bRedraw)
{
	// Aggiorno il valore dell'origina che sarà utilizzato da SetWindowPos
	if (nDPOffset != 0)
		m_nXNewOrigin += nDPOffset;

	if (bRedraw)
	{	// Sposto i titoli dell'offset richiesto e aggiorno il contenuto
		ScrollWindow (nDPOffset, 0);
		UpdateWindow();
	}
}

// Prepare lo spostamento a sinistra o a destra in accordo con la CRadarView.
// Se il flag di redraw è attivo aggiorno il contenuto.
//-----------------------------------------------------------------------------
void CRadarTitleBar::UpdateAndSetNewOrigin(int nDPOffset)
{
	// Aggiorno il valore dell'origine che sarà utilizzato da SetWindowPos
	if (nDPOffset)
		m_nXNewOrigin += nDPOffset;

	Invalidate	();
	UpdateWindow();
}

//-----------------------------------------------------------------------------
void CRadarTitleBar::AttachRadarBody (RadarBody* pRadarBody)
{
	m_pRadarBody = pRadarBody;
}

// Seleziona o deseleziona tutte le colonne ottimizzando la procedura di paint
//-----------------------------------------------------------------------------
void CRadarTitleBar::SelectAllColumns(BOOL bSelAll)
{
	ASSERT_VALID (m_pRadarBody);

	int nXStart, nXEnd;
	if (bSelAll)
	{
		DataType dataType = m_pRadarBody->SelectAllColumns (nXStart, nXEnd);
		GetRadarFrame ()->GetRadarDocument()->FindStupidReset (dataType);
	}
	else
		m_pRadarBody->DeselectAllColumns(nXStart, nXEnd);

	InvalidateUpdateRect (nXStart, nXEnd);
}

// Vale quando riportato nell'help di MFC
//-----------------------------------------------------------------------------
void CRadarTitleBar::OnLButtonUp(UINT nFlags, CPoint DPMousePos)
{
	__super::OnLButtonUp(nFlags, DPMousePos);

	// Protezione dell'area critica con blocco della stessa
	if (m_pProcessingMouse->IsLocked())
		return;

	CPoint LPMousePos (DPMousePos);
	DPToLP (LPMousePos);

	GetRadarView()->LeftButtonUp (LPMousePos);

	// Sblocco dell'area critica
	m_pProcessingMouse->Unlock();
}

// Vale quando riportato nell'help di MFC
//-----------------------------------------------------------------------------
void CRadarTitleBar::OnMouseMove(UINT nFlags, CPoint DPMousePos)
{
	__super::OnMouseMove(nFlags, DPMousePos);

	// Protezione dell'area critica con blocco della stessa
	if (m_pProcessingMouse->IsLocked())
		return;

	CPoint LPMousePos (DPMousePos);
	DPToLP (LPMousePos);

	GetRadarView()->MouseMove (LPMousePos);

	// Sblocco dell'area critica
	m_pProcessingMouse->Unlock();
}

// Vale quando riportato nell'help di MFC
//-----------------------------------------------------------------------------
void CRadarTitleBar::OnRButtonDown(UINT nFlags, CPoint DPMousePos)
{
	__super::OnRButtonDown(nFlags, DPMousePos);

	// Protezione dell'area critica con blocco della stessa
	if (m_pProcessingMouse->IsLocked())
		return;

	// Sblocco dell'area critica
	m_pProcessingMouse->Unlock();
}

//-----------------------------------------------------------------------------
int CRadarTitleBar::GetRowHeight() {return GetRadarView()->GetRowHeight();}

//-----------------------------------------------------------------------------
CRadarView* CRadarTitleBar::GetRadarView() { return GetRadarFrame()->GetRadarView (); }

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarTitleBar::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarTitleBar\n");

	__super::Dump(dc);
}
#endif // _DEBUG
