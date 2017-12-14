
#include "stdafx.h"
#include <ctype.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\CMapi.h>
#include <TbGenlib\barcombo.h>

#include <TbGes\extdoc.h>
#include <TbGes\hotlink.h>
#include <TbGes\dbt.h>
#include <TbGes\barquery.h>

#include "radardoc.h"
#include "radarfrm.h"
#include "radarvw.h"
#include "radarbar.h"
#include "radarbdy.h"
#include "radardlg.h"

#include "radar.hrc"    //risorse non ancora gestite nel JSON
#include "radar.hjson" //JSON AUTOMATIC UPDATE
#include <TbGes\barquery.hjson> //JSON AUTOMATIC UPDATE
#include <TbGenlib\BaseFrm.hjson> //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// Registrazione del messaggio della finestra di dailogo modeless della riceca stupida 
// Deve essere inizialmente registrata dalla funzione RegisterWindowMessage
static const UINT NEAR findDialogMsg = ::RegisterWindowMessage(FINDMSGSTRING);

// Indicatori della status bar
static const UINT BASED_CODE indicators[] = {	ID_RADAR_INDICATOR_MSG };

// Indicatori delle barra dei titoli
static const UINT BASED_CODE titles[] = {	ID_SEPARATOR };
/*
// Definizione della toolbar standard
static const int ciFixed	= 20;
static const int ciLink		= 21;

// Definizione della toolbar grafica
static const int ciCBoxIdxFontStyle	= 3;
static const int ciGrpSepCount		= 8;
static const int ciEasyReading		= 8;
static const int ciEasyReadingChk	= ciGrpSepCount;

static const UINT BASED_CODE ToolGrpButtons[] =
{
	ID_RADAR_SAVE,			//1
		ID_SEPARATOR,		//2
		ID_SEPARATOR,		//3
		ID_SEPARATOR,		//4
		ID_SEPARATOR,		//5
	ID_RADAR_RESIZE_COL,	//6 
		ID_SEPARATOR,		//7
	ID_RADAR_SHIFT_LEFT,	//8
	ID_RADAR_SHIFT_RIGHT,	//9
		ID_SEPARATOR,		//10
	ID_RADAR_DELETE,		//11
	ID_RADAR_INSERT,		//12
		ID_SEPARATOR,		//13
	ID_RADAR_MINUS_LINES,	//14
	ID_RADAR_PLUS_LINES,	//15
		ID_SEPARATOR,		//16
	ID_RADAR_EASY_READING	//17		
};
*/
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRadarFrame, CAbstractFrame)

BEGIN_MESSAGE_MAP(CRadarFrame, CAbstractFrame)

	ON_WM_CREATE		()
	ON_WM_ACTIVATE		()
	ON_WM_MOVE			()
	ON_WM_SETCURSOR		()
	ON_WM_TIMER			()
	ON_WM_SIZE			()

	ON_CBN_SELENDOK		(ID_RADAR_CB_FONT_STYLE, OnTBCBoxFontStyleChanged)
	ON_CBN_SELENDCANCEL	(ID_RADAR_CB_FONT_STYLE, OnTBCBoxFontStyleCancel)

	ON_COMMAND (ID_RADAR_CB_FONT_STYLE,	OnDropDown)
	ON_COMMAND (ID_RADAR_FIXED,			OnFixed)
	ON_COMMAND (ID_RADAR_LINK_RADAR,	OnLinkRadar)

	ON_UPDATE_COMMAND_UI (ID_RADAR_FIXED,			OnUpdateFixed)
	ON_UPDATE_COMMAND_UI (ID_RADAR_INDICATOR_MSG,	OnUpdatePaneText)
	ON_UPDATE_COMMAND_UI (ID_RADAR_LINK_RADAR,		OnUpdateLinkRadar)

	ON_MESSAGE (UM_FONT_STYLE_CHANGED,	OnFontStyleChanged)
	ON_MESSAGE (UM_FORMAT_STYLE_CHANGED,OnFormatStyleChanged)
	ON_MESSAGE (UM_IS_UNATTENDED_WINDOW,		OnIsUnattendedWindow)

	ON_REGISTERED_MESSAGE	(BCGM_EDIT_ON_FILL_AUTOCOMPLETE_LIST,	OnFillAutoComple)

	ON_WM_GETMINMAXINFO()

END_MESSAGE_MAP()  

//-----------------------------------------------------------------------------
CRadarFrame::CRadarFrame()
	:
	m_nCurrentMsg		(0),
	m_pWndTitleBar		(NULL),
	m_pWndGrpToolBar	(NULL),
	m_pWndExportToolBar (NULL),
	m_pWndMainToolBar	(NULL),
	m_pMsgStatusBar		(NULL),
	m_bIsClosing		(FALSE)
{
	SetDockable(FALSE);
}

//-----------------------------------------------------------------------------
CRadarFrame::~CRadarFrame()
{
	ASSERT_VALID(this);
	if (m_pMsgStatusBar		) delete m_pMsgStatusBar;
	if (m_pWndTitleBar		) delete m_pWndTitleBar;
}

//------------------------------------------------------------------------------
void CRadarFrame::CloseRadar()
{
	if (m_bIsClosing)
		return;

	// Serve per disabilitare la ToolBar e l'eventuale Bubble attualmente
	// attivo su uno dei sui bottoni

	if (m_pTabbedToolBar)
	{
		ASSERT_VALID(m_pTabbedToolBar);
		m_pTabbedToolBar->EnableWindow(FALSE);
	}

	if (m_pWndMainToolBar)
	{
		ASSERT_VALID (m_pWndMainToolBar);
		m_pWndMainToolBar->EnableWindow(FALSE);
	}

	if (m_pWndGrpToolBar)
	{
		ASSERT_VALID (m_pWndGrpToolBar);
		m_pWndGrpToolBar->EnableWindow(FALSE);
	}

	m_bIsClosing = TRUE;

	PostMessage (WM_CLOSE, 0, 0);

	//CRadarView* pView = (CRadarView*) GetActiveView();
	//ASSERT_VALID (pView);
	//CRadarDoc* pDoc	= pView->GetDocument();
	//ASSERT_VALID (pDoc);

	////ShowWindow	(SW_HIDE); RIGA COMMENTATA! SE DOPO LA SELEZIONE APPARE UNA MESSAGEBOX DERIVANTE DA onchange, IL FUOCO
	////NON VIENE PIU' RESTITUITO ALL'APPLICAZIONE!!!
	//pDoc->PostMessage (WM_CLOSE, 0, 0);
}


//-----------------------------------------------------------------------------
BOOL CRadarFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	if (!__super::PreCreateWindow(cs)) return FALSE;
   
	cs.hwndParent = GetValidOwner();
	return TRUE;
}
//-----------------------------------------------------------------------------
LRESULT CRadarFrame::OnIsUnattendedWindow(WPARAM wParam, LPARAM lParam)
{
	return GetRadarDocument() ? (LRESULT)!GetRadarDocument()->CanBeParentWindow() : 0L;
}


//-----------------------------------------------------------------------------
void CRadarFrame::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	// TabbedToolBar Width Size Modal - overlap of ico
	if (m_pTabbedToolBar)
		lpMMI->ptMinTrackSize.x = m_pTabbedToolBar->CalcMinimumWidth();
}

//------------------------------------------------------------------------------
BOOL CRadarFrame::PreTranslateMessage(MSG* pMsg)
{
	// Proxy Ctr Accelerator
	if (pMsg->message == WM_KEYDOWN && pMsg->wParam != VK_RETURN /* 13 Enter*/)
	{
		if (m_hAccelTable && ::TranslateAccelerator(this->m_hWnd, m_hAccelTable, pMsg))
			return TRUE;
	}
	
	CWnd* pWnd = GetFocus ();
	if (pWnd && (pWnd->m_hWnd == m_hWnd || pWnd->m_hWnd == GetRadarView()->m_hWnd))
		return CAbstractFrame::PreTranslateMessage(pMsg);

	return FALSE;
}

// Caricamento statico delle risorse stringhe e cursori per ottimizzare la velocità
// del modulo
// NOTA: tutti i cursori devono essere memorizzati nello stesso file .RC
//------------------------------------------------------------------------------
void CRadarFrame::LoadResources()
{   
	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(IDC_RADAR_COL_RESIZE), RT_GROUP_CURSOR);

	m_strStatusFormat = _TB("Searching - Current record: {0-%06ld}");

	ASSERT (!m_pMsgStatusBar);
	// caricamento delle stringhe visualizzate nella status bar in base
	// al cursore attualmente selezionato (corrispondenza 1 a 1 tra i due array).
	m_pMsgStatusBar	= new CStringArray;

	m_pMsgStatusBar->Add (_T(" "));
	m_pMsgStatusBar->Add (_TB("Resize column width"));
	m_pMsgStatusBar->Add (_TB("Select a row"));
	m_pMsgStatusBar->Add (_TB("Select/Deselect all columns"));
	m_pMsgStatusBar->Add (_TB("Select/Deselect a column"));
	m_pMsgStatusBar->Add (_TB("Insert a deleted column"));

	// caricamento dei cursori nel relativo array
	m_rghCursors [0] = ::LoadCursor(NULL,  MAKEINTRESOURCE(IDC_ARROW			));
	m_rghCursors [1] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_RADAR_COL_RESIZE	));
	m_rghCursors [2] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_RADAR_LB_SEL_ROW	));
	m_rghCursors [3] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_RADAR_LB_SEL_COL	));
	m_rghCursors [4] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_RADAR_TIB_SEL_COL));
	m_rghCursors [5] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_RADAR_INS_DEL_COL));

}

//-----------------------------------------------------------------------------
void CRadarFrame::OnFrameCreated()
{
	//impedisco alla frame di visualizzarsi in automatico, la visualizzerò poi io più avanti, quando ho caricato la sua posizione
	SetFrameVisible(FALSE);
	__super::OnFrameCreated();
	ScaleFrame((CFrameWnd*) this, TRUE);
	SetFrameVisible(TRUE);
}

//-----------------------------------------------------------------------------
int CRadarFrame::OnCreate (LPCREATESTRUCT lpcs)
{
	if (CAbstractFrame::OnCreate(lpcs) == -1)
	{
		TRACE0("CRadarFrame::OnCreate\n\tframe class not created.\n");
		return -1;
	}
	// Caricamento delle risorse (stringhe e cursori)
	LoadResources();

	m_pTabbedToolBar = new CTBTabbedToolbar();
	if (!m_pTabbedToolBar->Create(this)) 
		return -1;

	m_pTabbedToolBar->SuspendLayout();
	m_pTabbedToolBar->SetWindowText(_T("Tabbed"));

	//
	// Main Tool Bar
	//////////////////////
	m_pWndMainToolBar = new CTBToolBar();
	if (!m_pWndMainToolBar->CreateEmptyTabbedToolbar(this, szToolbarNameMain, _TB("Main")))
	{
		TRACE("Failed to create the main toolBar.\n");	
		return -1;
	}
	m_pTabbedToolBar->AddTab(m_pWndMainToolBar);

	if (AfxGetThemeManager()->ShowRadarFixed())
	{
		m_pWndMainToolBar->AddButton(ID_RADAR_FIXED, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconPin, TOOLBAR), _TB("Fix Radar"));
	}

	if (AfxGetThemeManager()->ShowRadarEdit())
	{
		m_pWndMainToolBar->AddSeparator();
		m_pWndMainToolBar->AddButton(ID_RADAR_EDIT, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconEdit, TOOLBAR), _TB("Edit Record"));
		m_pWndMainToolBar->AddSeparator();
	}

	m_pWndMainToolBar->AddButton(ID_RADAR_EXEC_QUERY, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconQuery, TOOLBAR), _TB("Query"));
	m_pWndMainToolBar->AddButton(ID_RADAR_EDIT_QUERY, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconEditQuery, TOOLBAR), _TB("Edit Query"));

	m_pWndMainToolBar->AddSeparator();
	m_pWndMainToolBar->AddButton(ID_RADAR_ORDER_ASCE, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconOrderAsc, TOOLBAR), _TB("Sort Ascending")); 
	m_pWndMainToolBar->AddButton(ID_RADAR_ORDER_DESC, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconOrderDesc, TOOLBAR),  _TB("Sort Descending")); 
	m_pWndMainToolBar->AddSeparator();
	m_pWndMainToolBar->AddButton(ID_RADAR_HOME, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconFirst, TOOLBAR), _TB("Table Beginning"));
	m_pWndMainToolBar->AddButton(ID_RADAR_PAGE_UP, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconTop, TOOLBAR), _TB("Previous page"));
	m_pWndMainToolBar->AddButton(ID_RADAR_ROW_UP, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconUp, TOOLBAR),   _TB("Previous row"));
	m_pWndMainToolBar->AddButton(ID_RADAR_ROW_DOWN, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconDown, TOOLBAR), _TB("Next row"));
	m_pWndMainToolBar->AddButton(ID_RADAR_PAGE_DOWN, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconBottom, TOOLBAR), _TB("Next page"));
	m_pWndMainToolBar->AddButton(ID_RADAR_END, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconLast, TOOLBAR), _TB("Table End"));
	
	m_pWndMainToolBar->AddEditToRight(ID_TOOLBAR_FIND_EDITBOX, STANDARD_IMAGE_LIBRARY_NS, _T("RadarFindEdit"), 150, ES_AUTOHSCROLL, _T(""), _TB("Search:")); \
	m_pWndMainToolBar->AddButtonToRight(ID_TOOLBAR_FIND_PREV, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconPrev, TOOLBAR), _TB("Previous")); \
	m_pWndMainToolBar->AddButtonToRight(ID_TOOLBAR_FIND_NEXT, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconNext, TOOLBAR), _TB("Next")); \
	m_pWndMainToolBar->SetDropdown(ID_TOOLBAR_FIND_NEXT);
	m_pWndMainToolBar->EnableAlwaysDropDown(ID_TOOLBAR_FIND_NEXT, FALSE);

	// Export ToolBar //////////////////////
	m_pWndExportToolBar  = new CTBToolBar();
	if (!m_pWndExportToolBar->CreateEmptyTabbedToolbar(this, szToolbarNameExport, _TB("Export")))
	{
		TRACE("Failed to create the main toolBar.\n");	
		return -1;
	}
	m_pTabbedToolBar->AddTab(m_pWndExportToolBar);

	m_pWndExportToolBar->AddButton(ID_RADAR_EXCEL, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconExportToExcel, TOOLBAR) , _TB("Export to Excel")); 
	m_pWndExportToolBar->AddButton(ID_RADAR_ODS, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconExportToOOCalc, TOOLBAR), _TB("Export to Open Office"));
	
	// Tool ToolBar //////////////////////
	m_pWndGrpToolBar = new CTBToolBar();
	if (!m_pWndGrpToolBar->CreateEmptyTabbedToolbar(this, szToolbarNameTools, _TB("Tools")))
		return -1;
	
	m_pTabbedToolBar->AddTab(m_pWndGrpToolBar);

	m_pWndGrpToolBar->AddButton(ID_RADAR_SAVE, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconSave, TOOLBAR), _TB("Save Graphic Attributes"));
	m_pWndGrpToolBar->AddSeparator();
	m_pWndGrpToolBar->AddButton(ID_RADAR_RESIZE_COL, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconColResize, TOOLBAR),  _TB("Columns Width"));
	m_pWndGrpToolBar->AddSeparator();
	m_pWndGrpToolBar->AddButton(ID_RADAR_SHIFT_LEFT, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconMoveColLeft, TOOLBAR), _TB("Shift Left"));
	m_pWndGrpToolBar->AddButton(ID_RADAR_SHIFT_RIGHT, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconMoveColRight, TOOLBAR), _TB("Shift Right"));
	m_pWndGrpToolBar->AddSeparator();
	m_pWndGrpToolBar->AddButton(ID_RADAR_DELETE, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconDelete, TOOLBAR),  _TB("Delete Selected"));
	m_pWndGrpToolBar->AddButton(ID_RADAR_INSERT, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconNew, TOOLBAR),  _TB("Insert columns"));
	m_pWndGrpToolBar->AddSeparator();
	m_pWndGrpToolBar->AddButton(ID_RADAR_MINUS_LINES, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconMinus, TOOLBAR), _TB("Decrease"));
	m_pWndGrpToolBar->AddButton(ID_RADAR_PLUS_LINES, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconAdd, TOOLBAR), _TB("Increase"));
	m_pWndGrpToolBar->AddSeparator();
	m_pWndGrpToolBar->AddButton(ID_RADAR_EASY_READING, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconEasyReadingOff, TOOLBAR), _TB("Easy Reading"));

	m_pWndGrpToolBar->AddLabel(-1, _TB("Character style:") );
	m_pWndGrpToolBar->AddComboBox(ID_RADAR_CB_FONT_STYLE, STANDARD_IMAGE_LIBRARY_NS, _T("CharacterStyle"), 100);
		
	
	// Allocazione e creazione della barra dei titoli
	m_pWndTitleBar = new CRadarTitleBar(this);
	if (!m_pWndTitleBar->Create(this))
		return -1;
	DockControlBar(m_pWndTitleBar);

	// Creazione della status bar e inizializzazione dei pannelli
	if	(
			!m_wndStatusBar.Create			(this)		||
			!m_wndStatusBar.SetIndicators	(indicators, sizeof(indicators)/sizeof(UINT))
		)
	{
		TRACE0("CRadarFrame::OnCreate\n\tStatusBar not created.\n");
		return -1;
	}
	int cxWidth;
	UINT nID, nStyle;
	m_wndStatusBar.GetPaneInfo(0, nID, nStyle, cxWidth);
	m_wndStatusBar.SetPaneInfo(0, nID, SBPS_STRETCH | SBPS_NORMAL, cxWidth);
	return 0;
}

//-----------------------------------------------------------------------------
void CRadarFrame::AdjustTabbedToolBar()
{
	if (!m_pTabbedToolBar) return;
	m_pTabbedToolBar->AdjustLayoutActiveTab();
}

//-----------------------------------------------------------------------------
BOOL CRadarFrame::LoadFrame(UINT nIDResource, DWORD dwDefaultStyle , CWnd* pParentWnd , CCreateContext* pContext)
{
	BOOL bRes = CAbstractFrame::LoadFrame(nIDResource, dwDefaultStyle, pParentWnd, pContext);

	// Caricamento degli acceleratori
	if (!LoadAccelTable (MAKEINTRESOURCE(IDR_RADAR_ACCELERATOR)))
		TRACE0("CRadarFrame::OnCreate\n\tIDR accellerator table not loaded.\n");

	return bRes;
}

// aggiunge sia titolo del documento che il subtitolo del frame
//-----------------------------------------------------------------------------
void CRadarFrame::OnUpdateFrameTitle(BOOL bAddToTitle)
{
	if ((GetStyle() & FWS_ADDTOTITLE) == 0)
		return;     // leave child window alone!

	CDocument* pDocument = GetActiveDocument();
	if (bAddToTitle && pDocument != NULL)
	{
		CString old;
		CString text;
		GetWindowText(old);

		// aggiunge il nome della query al titolo principale			
		text = pDocument->GetTitle();
		if (!m_strQueryName.IsEmpty())
			text += _T(" (") + m_strQueryName + _T(")");

		// set title if changed, but don't remove completely
		if (text != old)
		{
			SetWindowText(text);
			//comunico al menu che il titolo e' cambiato cosi` aggiorna il testo della tab
			SendTitleUpdatesToMenu();
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CRadarFrame::OnPopulatedDropDown(UINT nIdCommand)
{
	// gira il messagio al documento
	CDocument* pDocument = GetActiveDocument();
	if (pDocument && pDocument->IsKindOf(RUNTIME_CLASS(CRadarDoc)))
	{
		((CRadarDoc*)pDocument)->OnPopulatedDropDown(nIdCommand);
	}
	return TRUE;
}

// Cambia la stato del bottone di stay alive in accordo con lo stato attuale.
// Automaticamente modifica lo stato del bottone di Link radar
//-----------------------------------------------------------------------------
void CRadarFrame::OnFixed()
{
	SetFixedCheck();
}

//-----------------------------------------------------------------------------
void CRadarFrame::SetFixedCheck()
{
	if (!GetRadarDocument())
		return; 
	GetRadarDocument()->ChangeStayAlive() ?
		m_pWndMainToolBar->SetButtonInfo(ID_RADAR_FIXED, TBBS_BUTTON, TBIcon(szIconPinned, TOOLBAR))
	:
	m_pWndMainToolBar->SetButtonInfo(ID_RADAR_FIXED, TBBS_BUTTON, TBIcon(szIconPin, TOOLBAR));
	// Seleziona il bitmap da visualizzare sul bottone in accordo con lo stato
	// attuale del radar
	ChangeBtnLinkRadar(GetRadarDocument()->IsActiveLinkRadar());
}


//-----------------------------------------------------------------------------
void CRadarFrame::OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	if (m_bIsClosing)
		return;

	BOOL bActivateOther = pWndOther && nState != WA_INACTIVE;

	if (!bActivateOther)
	{
		CRadarDoc* pDoc	= (CRadarDoc*)GetActiveDocument();
		if (pDoc)
		{	
			if (pDoc->CanDoExit() && !InModalState() && IsWindowEnabled())
			{
				CloseRadar();	// Prima di morire deve scollegarsi dal documento chiamante
				return; 
			}
		}
	}

	CAbstractFrame::OnActivate(nState, pWndOther, bMinimized);	
}

//-----------------------------------------------------------------------------
BOOL CRadarFrame::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT nMsg)
{
	if (!GetRadarDocument())
		return CAbstractFrame::OnSetCursor(pWnd, nHitTest, nMsg);

	if (!GetRadarDocument()->IsActiveFind())
		m_nCurrentMsg = 0;

	return CAbstractFrame::OnSetCursor(pWnd, nHitTest, nMsg);
}

// Intercetta il cambiamento dello stile del carattere imposto dall'utente
// modificando la selezione nella ralativa combo box
//-----------------------------------------------------------------------------
void CRadarFrame::OnTBCBoxFontStyleChanged ()
{
	ASSERT_VALID (m_pWndGrpToolBar);
		int nIdx = m_pWndGrpToolBar->GetComboItemSel(ID_RADAR_CB_FONT_STYLE);
		SetFocus();
		CString sName = AfxGetFontStyleTable()->GetStyleName(nIdx);
		if (GetRadarDocument())
			GetRadarDocument()->FontStyleUpdate (sName);
}

// Funzioni che abilitano / disabilitano i vari bottoni presenti nella toolbar
// in base allo stato attuale del radar
//-----------------------------------------------------------------------------
void CRadarFrame::OnUpdateFixed(CCmdUI* pCmdUI)
{
	if (!GetRadarDocument())
		return;

	BOOL bEnable = GetRadarDocument()->IsEnableStayAlive();

	pCmdUI->Enable(bEnable);
	if (bEnable)
		pCmdUI->SetCheck (GetRadarDocument()->IsStayAlive());
}

//-----------------------------------------------------------------------------
void CRadarFrame::OnUpdateLinkRadar(CCmdUI* pCmdUI)
{
	if (!GetRadarDocument())
		return;
	BOOL bEnable = GetRadarDocument()->IsStayAlive();

	pCmdUI->Enable	(bEnable);
	pCmdUI->SetCheck(bEnable && GetRadarDocument()->IsActiveLinkRadar());
}

//-----------------------------------------------------------------------------
void CRadarFrame::OnUpdatePaneText(CCmdUI* pCmdUI)
{
	pCmdUI->Enable	(TRUE);
	if (m_pMsgStatusBar)
		pCmdUI->SetText	(m_pMsgStatusBar->GetAt(m_nCurrentMsg));
}

//-----------------------------------------------------------------------------
void CRadarFrame::OnTimer(UINT nUI)
{
	if (nUI == IDT_RADAR) SetRunIndicator ();
}

//-----------------------------------------------------------------------------
void CRadarFrame::OnSize(UINT nType, int cx, int cy)
{
	if (IsLayoutSuspended())
		return;

	__super::OnSize(nType, cx,cy);

}

// Collega alla barra dei titoli la struttura dati contenente tutte
// le informazioni per la corretta gestione delle stesse
//-----------------------------------------------------------------------------
void CRadarFrame::AttachRadarBodyToToolBar (RadarBody* pRadarBody)
{
	//TODO: da rivedere
	ASSERT(m_pWndTitleBar);
	ASSERT(pRadarBody);
	m_pWndTitleBar->AttachRadarBody (pRadarBody);
}

// Aggiorna il contenuto della status bar e l'attua le icone del cursore
//-----------------------------------------------------------------------------
void CRadarFrame::SetCursorAndMsgStatusBar (SelectCursor nCursor)
{
	ASSERT (nCursor >= 0 && nCursor < NUMBER_CURSORS);
	SetCursor (m_rghCursors[nCursor]);
	m_nCurrentMsg = nCursor;
}

// E' utlizzata dalla find stupida per indicare l'attuale stato della ricerca.
// Aggiorna il messaggio nella status bar riportando il corrente numero di record
// controllati.
//------------------------------------------------------------------------------
void CRadarFrame::SetRunIndicator ()
{
    CString strStatus = cwsprintf (m_strStatusFormat, GetRadarDocument() ? GetRadarDocument()->m_nFindTic : 0);
	CClientDC dc(&m_wndStatusBar);

	CRect rect; m_wndStatusBar.GetItemRect(0, &rect);
    rect.InflateRect (-2, -2);

	CFont*	 pOldFont	= dc.SelectObject(m_wndStatusBar.GetFont());
	COLORREF crOldColor	= dc.SetBkColor (AfxGetThemeManager()->GetButtonFaceBkgColor());
	dc.ExtTextOut	(rect.left, rect.top - 3, ETO_OPAQUE | ETO_CLIPPED, rect, strStatus, strStatus.GetLength(), NULL);
	dc.SelectObject	(pOldFont);
	dc.SetBkColor	(crOldColor);
}

// Riempie la combo box con gli stile attualmente definiti nell'applicazione e
// seleziona nel suo edit lo stile attualmente usato
//-----------------------------------------------------------------------------
void CRadarFrame::FillFontStyleComboBox(int nIdxCurrentFontStyle)
{
	if (!m_pWndGrpToolBar)
		return;

	m_pWndGrpToolBar->RemoveAllComboItems(ID_RADAR_CB_FONT_STYLE);
	FontStyleTableConstPtr pFontStyleTbl = AfxGetFontStyleTable();
	for (int nIdx = 0; nIdx < pFontStyleTbl->GetSize(); nIdx++)
		m_pWndGrpToolBar->AddComboItem(ID_RADAR_CB_FONT_STYLE, pFontStyleTbl->GetStyleTitle(nIdx));

	// Seleziona nell'edit della combo box lo stile attualmente usato
	m_pWndGrpToolBar->SetComboItemSel(ID_RADAR_CB_FONT_STYLE, nIdxCurrentFontStyle);
}

// Aggiorna lo stato del bottone per la lettura facilitata
//-----------------------------------------------------------------------------
void CRadarFrame::UpdateButtonEasyReading (BOOL bEasyReading)
{  
	if (!m_pWndGrpToolBar) 
		return;

	m_pWndGrpToolBar->SetButtonInfo
			( ID_RADAR_EASY_READING, 	TBBS_BUTTON,
				bEasyReading ? 
				TBIcon(szIconEasyReadingOn, TOOLBAR)  :   
				TBIcon(szIconEasyReadingOff,TOOLBAR) 
			);
}

//------------------------------------------------------------------------------
LPTSTR CRadarFrame::GetMenuCaption(UINT id, CString& strBuffer)
{
	strBuffer = cwsprintf(id);
	int idx = strBuffer.Find('\n');
	if (idx > 0)
		strBuffer = strBuffer.Left(idx);
	strBuffer.Trim();
	return (LPTSTR)(LPCTSTR)strBuffer;
}

//-----------------------------------------------------------------------------
CRadarView*	CRadarFrame::GetRadarView()
{ 
	CRadarDoc* pDoc = GetRadarDocument();
	CRadarView* pView = NULL;
	if (pDoc)
		pView = pDoc->GetRadarView();
	
	ASSERT(pView); 
	return pView;
}

//-----------------------------------------------------------------------------
CRadarDoc* CRadarFrame::GetRadarDocument()
{
	CRadarDoc* pDoc = dynamic_cast<CRadarDoc*>(GetActiveDocument());
	ASSERT_VALID (pDoc);
	return pDoc;
}

//-----------------------------------------------------------------------------
BOOL CRadarFrame::Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, LPCTSTR lpszMenuName, DWORD dwExStyle, CCreateContext* pContext)
{
	return __super::Create(lpszClassName, lpszWindowName, dwStyle, rect, pParentWnd, NULL/*nessun menu!*/, dwExStyle, pContext);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarFrame::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarFarme\n");
	CAbstractFrame::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//
// Funzioni Inlines
//
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
inline void CRadarFrame::OnDropDown()
	{ 
	}

//-----------------------------------------------------------------------------
inline LRESULT CRadarFrame::OnFontStyleChanged (WPARAM, LPARAM)
	{ if (GetRadarDocument()) GetRadarDocument()->OnFontStyleChanged(); return (LRESULT) 0L; }
inline LRESULT CRadarFrame::OnFormatStyleChanged (WPARAM, LPARAM)
	{ if (GetRadarDocument()) GetRadarDocument()->FormatStyleChanged();	return (LRESULT) 0L; }

//-----------------------------------------------------------------------------
inline void CRadarFrame::OnLinkRadar()
{
	if (GetRadarDocument()) 
		ChangeBtnLinkRadar(GetRadarDocument()->ChangeLinkRadar());
}

//-----------------------------------------------------------------------------
inline void CRadarFrame::ChangeBtnLinkRadar(BOOL bEnable)
{
}

//-----------------------------------------------------------------------------
LRESULT CRadarFrame::OnFillAutoComple(WPARAM wp, LPARAM lp)
{
	// gira il messagio al documento
	CDocument* pDocument = GetActiveDocument();
	if(pDocument && pDocument->IsKindOf(RUNTIME_CLASS(CRadarDoc)))
	{
		return ((CRadarDoc*) pDocument)->FillAutoComple(wp, lp);
	}
	return 0;
}

//-----------------------------------------------------------------------------
void CRadarFrame::RemoveTabInToolbar(LPCTSTR lpszText)
{
	if (m_pWndMainToolBar && m_pWndMainToolBar->GetName() == lpszText)
		m_pWndMainToolBar = NULL;
	else  if (m_pWndExportToolBar && m_pWndExportToolBar->GetName() == lpszText)
		m_pWndExportToolBar = NULL;
	else if (m_pWndGrpToolBar && m_pWndGrpToolBar->GetName() == lpszText)
		m_pWndGrpToolBar = NULL;

	m_pTabbedToolBar->RemoveTab(lpszText);
}

