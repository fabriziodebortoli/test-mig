#include "stdafx.h"

#include "afxole.h"
#include <locale.h>

#include <TbNameSolver\PathFinder.h>
#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\minmax.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\critical.h>
#include <TbGeneric\dib.h>
#include <TbGeneric\dibitmap.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\RdeProtocol.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\pictures.h>
#include <TBGeneric\WndObjDescription.h>

#include <TbGenlib\BaseApp.h>
#include <TbGenlib\BaseFrm.h>
#include <TbGenlib\Tfxdatatip.h>

#include <TbGenlibManaged\EmfToPdf.h>
#include <TbGenlibManaged\main.h>
#include <TbGenlibUI\TBExplorer.h>

#include <TbWoormEngine\repfield.h>
#include <TbWoormEngine\reptable.h>
#include <TbWoormEngine\report.h>
#include <TbWoormEngine\rulengin.h>
#include <TbWoormEngine\report.h>
#include <TbWoormEngine\multilayout.h>
#include <TbWoormEngine\ruledata.h>
#include <TbWoormEngine\edtmng.h>
#include <TbWoormEngine/events.h>
#include <TbWoormEngine/prgdata.h>
#include <TbWoormEngine/repdata.h>
#include <TbWoormEngine/qrydata.h>

#include <thread>
#include <thread>
#include "RSEditView.h"
#include "pageinfo.h"
#include "mulselob.h"
#include "singleob.h"
#include "woormdoc.h"
#include "woormfrm.h"
#include "woormini.h"
#include "listdlg.h"
#include "woormvw.h"
#include "rectobj.h"
#include "TBPrintDialog.h"
#include "column.h"
#include "RSEditorUI.h"

// resources
#include "woormdoc.hjson" 
#include "RSEditorUI.hjson"
 
//includere come ultimo include all'inizio del cpp
#include "beginh.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// numero di pixel per gestire il bordo grigio scuro in erase background
#define BKGND_BORDER				50
#define GRID_SIZE_X					GetDocument()->m_pWoormIni->m_nGridX
#define GRID_SIZE_Y					GetDocument()->m_pWoormIni->m_nGridY

static const COLORREF	MDIBackGrndColor	= RGB ( 132, 148, 232 );	// violet
static const COLORREF	MDIForeGrndColor	= RGB ( 194, 199, 228);		// lilla RGB ( 214, 219, 248);		// lilla

//-----------------------------------------------------------------------------
void CWatermark::Clear () 
{ 
	if (m_pFont) 
		{ m_pFont->DeleteObject();  SAFE_DELETE(m_pFont); } 
	m_bCreated = FALSE;
}

//-----------------------------------------------------------------------------
void CWatermark::Draw (CDC* pDC, const CRect& rect)
{
	return;	//15/12/2010 Andrea - A. Anomalia 18109
	//---------------------------------------------
/*
	if (!pDC->IsPrinting())	//Richiesto da Enrico venerdì 20/08/2010
		return;

	if (!m_bCreated)
	{
		Create(pDC, rect);
	}

	if (m_sText.IsEmpty())
		return;

	// salvo i vecchi ed imposto i nuovi
	int oldMode			= pDC->SetBkMode(TRANSPARENT);
	CFont* oldFont		= pDC->SelectObject(m_pFont);
	COLORREF oldColor	= pDC->SetTextColor(MDIForeGrndColor);

	pDC->DrawText(m_sText, (LPRECT) m_rectPage, DT_BOTTOM | DT_LEFT| DT_SINGLELINE);
	
	// ripristino i precedenti
	pDC->SelectObject (oldFont);
	pDC->SetTextColor (oldColor);
	pDC->SetBkMode(oldMode);
*/
}

//-----------------------------------------------------------------------------
BOOL CWatermark::Create (CDC* pDC, const CRect& rect) 
{
	if (m_pFont) { m_pFont->DeleteObject();  SAFE_DELETE(m_pFont);}

	m_rectPage = rect;

	SetText();

	if (m_sText.IsEmpty())
	{
		m_bCreated = TRUE;
		return TRUE;
	}

	const double pi = 3.14159265359;
	int w = m_rectPage.Width();
	int h = m_rectPage.Height();
	double dDiagonal = sqrt(double(w*w)+double(h*h));

	if (dDiagonal == 0)
		return FALSE;

	m_nFontAngle = (int)(3600.0 / (2.0 * pi) * asin(h / dDiagonal));

	m_pFont = new CFont();
	VERIFY(m_pFont->CreateFont
	(
		m_nFontSize,				// nHeight
		0,							// nWidth
		m_nFontAngle,				// nEscapement
		m_nFontAngle,				// nOrientation
		FW_EXTRALIGHT,				// nWeight
		TRUE,						// bItalic
		FALSE,						// bUnderline
		0,							// cStrikeOut
		ANSI_CHARSET,				// nCharSet
		OUT_DEFAULT_PRECIS,			// nOutPrecision
		CLIP_DEFAULT_PRECIS,		// nClipPrecision
		DEFAULT_QUALITY,			// nQuality
		DEFAULT_PITCH | FF_SWISS,	// nPitchAndFamily
		_T("Verdana")					// lpszFacename
	));

		
	CFont* oldFont = pDC->SelectObject(m_pFont);

	CSize cs = pDC->GetTextExtent(m_sText);
	if (cs.cx == 0)
		return FALSE;

	int sz = int(dDiagonal * m_nFontSize / cs.cx);
	pDC->SelectObject (oldFont);

	m_pFont->DeleteObject();  SAFE_DELETE(m_pFont);

	m_nFontSize = sz - sz / 10;
	if (m_nFontSize < 10)	//10 è molto piccolo perchè sto stampando
		return FALSE;

	m_pFont = new CFont();
	VERIFY(m_pFont->CreateFont
	(
		m_nFontSize,				// nHeight
		0,							// nWidth
		m_nFontAngle,				// nEscapement
		m_nFontAngle,				// nOrientation
		FW_EXTRALIGHT,				// nWeight
		TRUE,						// bItalic
		FALSE,						// bUnderline
		0,							// cStrikeOut
		ANSI_CHARSET,				// nCharSet
		OUT_DEFAULT_PRECIS,			// nOutPrecision
		CLIP_DEFAULT_PRECIS,		// nClipPrecision
		DEFAULT_QUALITY,			// nQuality
		DEFAULT_PITCH | FF_SWISS,	// nPitchAndFamily
		_T("Verdana")					// lpszFacename
	));

	m_bCreated = TRUE;
	return TRUE;
}

//-----------------------------------------------------------------------------
void CWatermark::SetText()
{
	m_sText.Empty();

	if (!AfxGetLoginManager())
		return;

	BOOL bDemo = AfxGetLoginManager()->IsADemo();

	// Il messaggio viene dato solo in inglese (richiesta di Enrico) ma
	// preferisco non dividere la stringa in più pezzi per aprire 
	// l'eventuale strada della traduzione in lingua utente
	switch (AfxGetLoginManager()->GetSerialNumberType())
	{
		case CLoginManagerInterface::Development:
			if (bDemo)		
				m_sText = _T("This product is a Developer Only and Demo Version. ");
			else
				m_sText = _T("This product is a Developer Only Version. ");
			break;
		case CLoginManagerInterface::Reseller:
			if (bDemo)		
				m_sText = _T("This product is a Reseller Only and Demo Version. ");
			else
				m_sText = _T("This product is a Reseller Only Version. ");
			break;
		case CLoginManagerInterface::Distributor:
			if (bDemo)		
				m_sText = _T("This product is a Distributor Only and Demo Version. ");
			else
				m_sText = _T("This product is a Distributor Only Version. ");
			break;
		default:
			if (bDemo)		
				m_sText = _T("This product is a Demo Version. ");
			else
				m_sText.Empty();
			break;
	}
}

//==============================================================================
//		class CWoormPreviewView implementation
//==============================================================================

//------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWoormPreviewView, CBCGPPrintPreviewView)

BEGIN_MESSAGE_MAP(CWoormPreviewView, CBCGPPrintPreviewView)
	ON_WM_CREATE()
	
	ON_COMMAND(AFX_ID_PREVIEW_CLOSE, OnPreviewClose)
	ON_COMMAND(AFX_ID_PREVIEW_NEXT,  OnNextPage)
	ON_COMMAND(AFX_ID_PREVIEW_PREV,  OnPrevPage)

	ON_UPDATE_COMMAND_UI(AFX_ID_PREVIEW_NEXT,    OnUpdateNextPage)
	ON_UPDATE_COMMAND_UI(AFX_ID_PREVIEW_PREV,    OnUpdatePrevPage)
	ON_UPDATE_COMMAND_UI(AFX_ID_PREVIEW_NUMPAGE, OnUpdateNumPageChange)
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CWoormPreviewView::CWoormPreviewView ()
{
}

//------------------------------------------------------------------------------
CWoormPreviewView::~CWoormPreviewView ()
{
}

//------------------------------------------------------------------------------
int CWoormPreviewView::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CPreviewView::OnCreate(lpCreateStruct) == -1)
	{
		return -1;
	}

	ASSERT_VALID(m_pToolBar);

	CFrameWnd* pParentFrame = BCGPGetParentFrame(this);
	ASSERT_VALID(pParentFrame);

	CFrameWnd* pToplevelFrame = pParentFrame;

	if (pToplevelFrame->IsKindOf(RUNTIME_CLASS(CBCGPMDIChildWnd)))
	{
		pToplevelFrame = pToplevelFrame->GetTopLevelFrame();
	}

	if (!m_wndToolBar.Create(m_pToolBar))
	{
		TRACE0("Failed to create print preview toolbar\n");
		return FALSE;      // fail to create
	}

	m_wndToolBar.SetOwner(this);
	m_wndToolBar.SetHeight(ScalePix(m_wndToolBar.GetRowHeight()));

	//-------------------------------------------
	CBCGPDockManager* pDockManager = globalUtils.GetDockManager(pParentFrame);
	ASSERT_VALID(pDockManager);
	pDockManager->AddControlBar(&m_wndToolBar, FALSE);
	
	SetToolbarSize();
	return 0;
}

//------------------------------------------------------------------------------
void CWoormPreviewView::OnPreviewClose()
{
	GetDocument()->RestoreCurrentLayout();
	__super::OnPreviewClose();
}

//------------------------------------------------------------------------------
void CWoormPreviewView::OnNextPage()
{
	BOOL bNextSplitter = GetDocument()->AllowNextSplitterPage();
	if (bNextSplitter)
	{
		m_nCurrentPage--;
		GetDocument()->SetNextSplitterPage();
	}
	else if (GetDocument()->GetNumberOfSplittedPage())
	{
		GetDocument()->SetFirstSplitterPage();
	}

	CPreviewView::OnNextPage();

	if (!bNextSplitter)
	{
		GetDocument()->ReadNextPage();
	}
	else
		GetDocument()->ReadSelectedPage(m_nCurrentPage - 1);
}                                       
									   
//------------------------------------------------------------------------------
void CWoormPreviewView::OnPrevPage()
{
	BOOL bPrevSplitter = GetDocument()->AllowPrevSplitterPage();
	if (bPrevSplitter)
	{
		m_nCurrentPage++;
		GetDocument()->SetPrevSplitterPage();
	}
	else if (GetDocument()->GetNumberOfSplittedPage())
	{
		GetDocument()->SetLastSplitterPage();
	}

	CPreviewView::OnPrevPage();

	if (!bPrevSplitter)
	{
		GetDocument()->ReadPrevPage();
	}	
	else
		GetDocument()->ReadSelectedPage(m_nCurrentPage - 1);
}                                       
									   
//------------------------------------------------------------------------------
void CWoormPreviewView::OnUpdateNextPage(CCmdUI* pCmdUI)
{
	BOOL ok = ( 
				(
					((m_nCurrentPage + m_nPages - 1) < m_pPreviewInfo->GetMaxPage()) 
				||
					(
						(m_nCurrentPage + m_nPages - 1) == m_pPreviewInfo->GetMaxPage() 
						&&
						GetDocument()->AllowNextSplitterPage()
					)
				)
			&&
				!GetDocument()->IsEngineRunning() 
			&& 
				!GetDocument()->m_pRDEmanager->IsClosed()
			);
	
	pCmdUI->Enable(ok);
}
									   
//------------------------------------------------------------------------------
void CWoormPreviewView::OnUpdatePrevPage(CCmdUI* pCmdUI)
{
	BOOL ok = (
				(
					(m_nCurrentPage > m_pPreviewInfo->GetMinPage()) 
				||
					(
						m_nCurrentPage  == m_pPreviewInfo->GetMinPage() 
						&&
						GetDocument()->AllowPrevSplitterPage()
					)
				)
			&&
				!GetDocument()->IsEngineRunning() 
			&& 
				!GetDocument()->m_pRDEmanager->IsClosed()
			);
	
	pCmdUI->Enable(ok);
}                                       
	 
//-----------------------------------------------------------------------------

void CWoormPreviewView::OnUpdateNumPageChange(CCmdUI* pCmdUI)
{ 
	pCmdUI->Enable(FALSE);
}


/////////////////////////////////////////////////////////////////////////////
// CWoormView

//------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWoormView, CBCGPScrollView)


BEGIN_MESSAGE_MAP(CWoormView, CBCGPScrollView)

	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()
	ON_WM_RBUTTONDOWN()
	ON_WM_LBUTTONDBLCLK()

	ON_WM_SETCURSOR()
	ON_WM_TIMER()
	ON_WM_ERASEBKGND()
	
	ON_COMMAND (ID_FILE_PRINT,			OnFilePrint)
	ON_COMMAND (ID_RMOUSE_DOWN,			OnAccelerateRMouseDown)

	ON_UPDATE_COMMAND_UI(ID_RMOUSE_DOWN,		OnUpdateAccelerateRMouseDown)
	ON_UPDATE_COMMAND_UI(ID_FILE_PRINT,			OnUpdateFilePrint)

	ON_COMMAND (ID_WOORMVW_UP, 		OnScrollUp		)
	ON_COMMAND (ID_WOORMVW_DOWN,	OnScrollDown	)
	ON_COMMAND (ID_WOORMVW_LEFT,	OnScrollLeft	)
	ON_COMMAND (ID_WOORMVW_RIGHT,	OnScrollRight	)
	ON_COMMAND (ID_WOORMVW_HOME, 	OnScrollTop		)
	ON_COMMAND (ID_WOORMVW_END,		OnScrollBotton	)

	ON_UPDATE_COMMAND_UI(ID_OBJECT_CUT,		OnUpdateCut)
	ON_COMMAND			(ID_OBJECT_CUT,		OnObjectCut)

	ON_COMMAND			(ID_VK_LEFT,		OnVKLeft)
	ON_COMMAND			(ID_VK_RIGHT,		OnVKRight)
	ON_COMMAND			(ID_VK_UP,			OnVKUp)
	ON_COMMAND			(ID_VK_DOWN,		OnVKDown)

	ON_UPDATE_COMMAND_UI(ID_VK_LEFT,		OnUpdateVKMove)
	ON_UPDATE_COMMAND_UI(ID_VK_RIGHT,		OnUpdateVKMove)
	ON_UPDATE_COMMAND_UI(ID_VK_UP,			OnUpdateVKMove)
	ON_UPDATE_COMMAND_UI(ID_VK_DOWN,		OnUpdateVKMove)

	ON_COMMAND (ID_VK_TAB, OnVKTab)

	ON_MESSAGE (UM_GET_REPORT_NAMESPACE,	OnGetReportNamespace)
	ON_MESSAGE (UM_GET_LOCALIZER_INFO,			OnGetLocalizerInfo)
	
	ON_NOTIFY_EX_RANGE	(TTN_NEEDTEXTW, 0, 0xFFFF, OnToolTipText)
	ON_NOTIFY_EX_RANGE	(TTN_NEEDTEXTA, 0, 0xFFFF, OnToolTipText)

	ON_WM_NCDESTROY()

END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CWoormView construction/destruction

//------------------------------------------------------------------------------
CWoormView::CWoormView()
	: 
	m_bSensibility		(FALSE),
	m_bSupportCollate	(TRUE),
	m_bSupportCopies	(TRUE),
	m_nCurrentCopy		(1),
	m_nFromPage			(1),
	m_nToPage			(1),
	m_PrintStatus		(NO_PRINT_ERROR),
	m_nScaleDenom		(1),
	m_nScaleNum			(1),
	m_bDoScale			(FALSE),
	m_bPDFPrinting		(FALSE),
	m_scaleType			(NONE),
	m_dLowerScale		(1.0),
	m_dScaleFactor		(1.0),
	m_bCalculatedScale	(FALSE),
	m_ptOffsetOrgAfterScaling	(0,0),
	m_pWatermark		(NULL)
{
	// semaforo per gestire la ricorsivita di eventuali messaggi di mouse
	m_pProcessingMouse = new CriticalArea();

	// probably Afx bug (if document is read from maximized status)
	// afx call OnPrepareDc before calling OnInitialUpdate; so mapping mode
	// is wrong and afx give ASSERT violation (see constructor in viewscrl.cpp)
	//
	m_nMapMode = MAPPING_MODE;
		
	m_pDataTip = new TFXDataTip();
	m_pWatermark = new CWatermark();
}

//------------------------------------------------------------------------------
CWoormView::~CWoormView()
{
	ASSERT_VALID(this);
	ASSERT(m_pProcessingMouse);
	SAFE_DELETE(m_pProcessingMouse);

	SAFE_DELETE(m_pDataTip);
	SAFE_DELETE(m_pWatermark);
}

//--------------------------------------------------------------------------
LRESULT CWoormView::OnGetReportNamespace(WPARAM, LPARAM)
{
	return (LRESULT)GetDocument()->GetNamespace().ToString().AllocSysString();
}
//-----------------------------------------------------------------------------
LRESULT CWoormView::OnGetLocalizerInfo(WPARAM wParam, LPARAM lParam)
{
	return GetLocalizerInfo(wParam, lParam);
}

// la pagina a video e' quella data dalla dimensione fisica del foglio meno
// i margini indicati dall'utente. (la scroll area dipende da questa size)
//------------------------------------------------------------------------------
CSize CWoormView::GetPageSizeWithoutMargin_LP()
{ 
	PageInfo& info = GetDocument()->m_PageInfo;
	CSize ptSize(info.GetPageSize_LP());
	if (!GetDocument()->GetObjects().m_bInvertOrientation)
		return CSize
			(
				ptSize.cx - info.m_rectMargins.left - info.m_rectMargins.right,
				ptSize.cy - info.m_rectMargins.top - info.m_rectMargins.bottom
			);
	else
		return CSize
		(
			ptSize.cy - info.m_rectMargins.top - info.m_rectMargins.bottom,
			ptSize.cx - info.m_rectMargins.left - info.m_rectMargins.right
			);
}

//------------------------------------------------------------------------------
CSize CWoormView::GetPrinterPageSizeWithoutMargin_LP()
{ 
	PageInfo& info = GetDocument()->m_PageInfo;
	CSize ptPrtSize(info.GetPrinterPageSize_LP());
	
	if(!GetDocument()->GetObjects().m_bInvertOrientation)
		return CSize
			(
				ptPrtSize.cx - info.m_rectMargins.left - info.m_rectMargins.right,
				ptPrtSize.cy - info.m_rectMargins.top - info.m_rectMargins.bottom
			);
	else 
		return CSize
		(
			ptPrtSize.cy - info.m_rectMargins.top - info.m_rectMargins.bottom,
			ptPrtSize.cx - info.m_rectMargins.left - info.m_rectMargins.right
			);
}

//------------------------------------------------------------------------------
CSize CWoormView::GetScrollSize	()
{ 
	CSize ptSize(GetPageSizeWithoutMargin_LP());
	return CSize(ptSize.cx + BKGND_BORDER, ptSize.cy + BKGND_BORDER);
}

//------------------------------------------------------------------------------
CSize CWoormView::GetPrinterFullPageSize_mm()
{ 
	PageInfo& info = GetDocument()->m_PageInfo;
	return info.GetPrinterPageSize_mm();
}

//------------------------------------------------------------------------------
BOOL CWoormView::DrawBitmap(CDC* pDC, CPoint ptOrigin, const CString& strBmpName)
{
	if (strBmpName.IsEmpty())
		return FALSE;

	CString strName	= AfxGetPathFinder()->FromNs2Path(strBmpName, CTBNamespace::IMAGE, CTBNamespace::FILE);

	// Leggo il bitmap dal file in formato .bmp,.jpg,.png
	CTBPicture bitmap;
	if (!bitmap.ReadFile(strName, TRUE, TRUE))
		return FALSE;

	CSize sizeBitmap(bitmap.GetWidth(), bitmap.GetHeight());
	CRect rectDst	(ptOrigin, sizeBitmap);

	ScaleRect(rectDst, *pDC);

	if (pDC->IsPrinting())  // print e preview
	{
		CSize szPrtSize(GetPrinterPageSizeWithoutMargin_LP());
		CRect rectInside(ptOrigin.x, ptOrigin.y, szPrtSize.cx, szPrtSize.cy);		
		if (m_bDoScale) //in caso di zoom bisogna adattare il rettangolo in cui disegna lo sfondo
		{
			double scale = double(m_nScaleNum) / m_nScaleDenom;

			switch (m_scaleType)
			{
				case ONLY_HORIZONTAL:
				{
					double dRectRight = rectInside.right / scale;
					round(dRectRight, 0);
					rectInside.right = int(dRectRight);
					break;
				}
				case ONLY_VERTICAL:
				{
					double dRectBottom = rectInside.bottom / scale;
					round(dRectBottom, 0);
					rectInside.bottom = int(dRectBottom);
					break;
				}
				case HORIZONTAL_MORETHAN_VERTICAL:
				{
					double dRectRight = rectInside.right / scale;
					round(dRectRight, 0);
					rectInside.right = int(dRectRight);

					double dRectBottom = rectInside.bottom / m_dLowerScale;
					round(dRectBottom, 0);
					rectInside.bottom = int(dRectBottom);
					break;
				}
				case VERTICAL_MORETHAN_HORIZONTAL:
				{
					double dRectBottom = rectInside.bottom / scale;
					round(dRectBottom, 0);
					rectInside.bottom = int(dRectBottom);

					double dRectRight = rectInside.right / m_dLowerScale;
					round(dRectRight, 0);
					rectInside.right = int(dRectRight);
					break;
				}
				
				default:
					ASSERT(FALSE);
					break;
			}
		}
		CRect rectSrc (0, 0, rectInside.right - rectInside.left, rectInside.bottom - rectInside.top);
		GenericDrawObj::RectForDrawBitmap(pDC, rectInside, rectSrc, &bitmap, TRUE);
		bitmap.DrawPicture(*pDC, rectInside, rectSrc);
	}
	else  //a video
	{
		bitmap.DrawPicture(*pDC, rectDst);
	}
	return TRUE;
}

// In caso di stampa gestisce il messaggio standard. A video (non in preview)
// mostra anche i contorni della pagina e se richiesto dall'utente anche la griglia
// di puntini distanziati di 2 millimitri per ogni passo della griglia.
//
//------------------------------------------------------------------------------
BOOL CWoormView::OnEraseBkgnd(CDC* pDC)
{
	if (pDC == NULL)
		return TRUE;
	BOOL bRet = CBCGPScrollView::OnEraseBkgnd(pDC);

	if (pDC->IsPrinting())
	{
		return bRet;
	}

	CWoormDocMng* pDoc = GetDocument();
	ASSERT(pDoc);

	// deve tenere conto dello stile di disegno della pagina (EDGE_RAISED)
	// che utilizzano tre pixel
	CSize csPageSize(GetPageSizeWithoutMargin_LP());
	int cx = csPageSize.cx + 3;
	int cy = csPageSize.cy + 3;

	CRect page (0, 0, cx, cy);
	CPoint sp = GetScrollPosition();
	page.OffsetRect(-sp.x, -sp.y);

	// Gestione della pagina per lo sfondo su video (non su stampante)
	pDC->FillSolidRect(page,RGB(0xFF, 0xFF, 0xFF));

	//il background in editing mode da fastidio per il posizionamento degli oggetti
	//if (!pDoc->m_bAllowEditing)
	//	m_pWatermark->Draw(pDC, page);

	if (!pDoc->m_pDataDefaults->m_bUseAsRadar || pDoc->m_bAllowEditing)
		pDC->DrawEdge(page, EDGE_RAISED, BF_BOTTOMRIGHT);

	// disegna il bitmap di sfondo
	CPoint ptBitmapOrigin(pDoc->m_pOptions->m_BitmapOrigin);
	ptBitmapOrigin.Offset(-sp.x, -sp.y);

	if (!pDoc->m_pOptions->NoBitmap(pDoc, FALSE))
	{
		CString sBkgnBitmap = pDoc->m_pOptions->m_strBkgnBitmap;
		if (!sBkgnBitmap.IsEmpty())
		{
			sBkgnBitmap = AfxGetPathFinder()->FromNs2Path(sBkgnBitmap, CTBNamespace::IMAGE, CTBNamespace::FILE);

			DrawBitmap(pDC, ptBitmapOrigin, sBkgnBitmap);
		}
	}

	CPen	pen(PS_SOLID, 1, pDoc->m_pWoormIni->m_rgb_GridColor);
	CPen*	penOld	= (CPen*)pDC->SelectObject(&pen);

	if (pDoc->m_pWoormIni->m_bShowGrid && pDoc->m_bAllowEditing)
	{
		// si fa vedere la Grid  almeno con due millimetri di step

		int nMinStep = MUtoLP(2);
		int	nStepX = Max(pDoc->m_pWoormIni->m_nGridX, nMinStep);
		int	nStepY = Max(pDoc->m_pWoormIni->m_nGridY, nMinStep);

		// mostra la griglio solo se superiore ai due millimetri
		if (nStepX >= nMinStep && nStepY >= nMinStep)
		{
			if (pDoc->m_pWoormIni->m_bLineGrid)
			//disegno una griglia +++++
			{
				for (int x = 0; x <= cx; x += nStepX)
				{
					pDC->MoveTo(x - sp.x, 0);
					pDC->LineTo(x-sp.x, cy);
				}
				for (int y = 0; y <= cy; y += nStepY)
				{
					pDC->MoveTo(0, y-sp.y);
					pDC->LineTo(cx, y-sp.y);
				}
			}
			else
			//disegno dei puntini ......
			{
				for (int x = 0; x <= cx; x += nStepX)
					for (int y = 0; y <= cy; y += nStepY)
					{
						if (pDC->PtVisible(x - sp.x, y - sp.y))
							pDC->SetPixel(x - sp.x, y - sp.y, RGB(0, 0, 0));

					}
			}
		}
	}
	pDC->SelectObject(penOld);

	// disegna la parte esterna al foglio 
	CRect client;
	GetClientRect(client);
	page.IntersectRect(page, client);

	pDC->ExcludeClipRect(page);

	COLORREF crBkgColor = RGB(241, 241, 241);

	pDC->FillSolidRect
	(
		client,
		pDoc->m_bAllowEditing
			? crBkgColor
			: pDoc->m_pDataDefaults->m_rgbPageBkgn
	);

	// ripristina la regione di clipping per permettere il WM_PAINT
	CRgn rgn; rgn.CreateRectRgnIndirect(page);
	pDC->SelectClipRgn(&rgn);

	return TRUE;
}

//------------------------------------------------------------------------------
void CWoormView::OnTimer (UINT nUI)
{   
	if (nUI == IDT_WOORM)
	{
		GetDocument()->SetRunIndicator();
	}
	else if (nUI == IDT_WOORM_AUTOSAVE)
	{
		GetDocument()->FileAutoSave();
	}
	else if (nUI == IDT_WOORM_RERUN)
	{
		GetDocument()->ReRun();
	}
}

//------------------------------------------------------------------------------
void CWoormView::OnUpdateFilePrint(CCmdUI* pCmdUI)
{   
	BOOL bEnable = !GetDocument()->m_bEngineRunning;
	pCmdUI->Enable(bEnable);
}

//------------------------------------------------------------------------------
void CWoormView::OnAccelerateRMouseDown()
{
	if (!GetDocument()->m_pCurrentObj)
		return;
					   
	// must convert from logical coordinate to fisical one
	CClientDC dc(this);
	OnPrepareDC(&dc);

	// Retrieve the intersecting rectangle. Must show popup only if
	// active object is visibile (all or partial)
	CRect rect = GetDocument()->m_pCurrentObj->GetActiveRect();
	dc.LPtoDP(&rect);

	CRect client;  
	GetClientRect(&client);

	// Simulate right mouse click in top left corner of visibile area
	if (rect.IntersectRect(&rect, &client))
	{
		CPoint point (rect.left + rect.Width() / 2, rect.top + rect.Height() / 2);
		OnRButtonDown(0, point);
	}
}

//------------------------------------------------------------------------------
void CWoormView::OnUpdateAccelerateRMouseDown(CCmdUI* pCmdUI)
{
	// retrieve document
	BOOL ok = GetDocument()->m_pCurrentObj && !Disabled();
	pCmdUI->Enable(ok);
}


//------------------------------------------------------------------------------
void CWoormView::OnUpdate(CView* pSender, LPARAM lHint, CObject* pHint)
{
	// The document has informed this view that some data has changed.
	// but if this windows is sender then they have already updated
	// her display area, so exit.
	if (pSender == this) return;

	// this call set also mapping mode
	SetScrollSizes(MAPPING_MODE, GetScrollSize());

	if (pHint != NULL)
	{   
		if (pHint->IsKindOf(RUNTIME_CLASS(CObArray)))
		{
			// The hint is that a stroke as been added (or changed).
			// So, Invalidate its rectangle. 
			CClientDC dc(this);
			OnPrepareDC(&dc);            

			CInvalidArray*	InvalidAreas = (CInvalidArray*) pHint;
			int ub = InvalidAreas->GetUpperBound();
			for (int i = 0; i <= ub; i++)
			{
				InvalidArea* pInvalidArea = InvalidAreas->GetAt(i);
				CRect invalid = pInvalidArea->Invalid();
				BOOL  bRepaint = pInvalidArea->EraseBkg();
				
				dc.LPtoDP(&invalid);
				InvalidateRect(invalid, bRepaint);
			}
			return;
		}
	}

	// We can't interpret the hint, so assume that anything might
	// have been updated. Use lHint to determinate background bRepaint
	Invalidate(lHint ? TRUE : FALSE);
	return;
}


/////////////////////////////////////////////////////////////////////////////
//  A view's OnInitialUpdate() overrideable function is called immediately
//  after the frame window is created, and the view within the frame
//  window is attached to its document.  This provides the scroll view the
//  opportunity to set its size (m_totalSize) based on the document size.
//------------------------------------------------------------------------------
void CWoormView::OnInitialUpdate()
{
	m_DropTarget.Register(this);
	CBCGPScrollView::OnInitialUpdate();
	ASSERT(GetDocument() != NULL);

	// this call set also mapping mode
	SetScrollSizes(MAPPING_MODE, GetScrollSize());

	EnableToolTips();
	m_pDataTip->Create(this); m_pDataTip->On(TRUE);
}

//------------------------------------------------------------------------------
void CWoormView::GetObjDescription()
{ 
	CWoormDocMng* pWoormDoc = GetDocument();
	if (pWoormDoc)
	{
		pWoormDoc->SetCustomUICulture();
		//TODO
		// si delega ai singoli oggetti contenuti nel documento di disegnarsi
		pWoormDoc->GetObjects().GetObjDescription();
		pWoormDoc->ResetCustomUICulture();
	}
}

/////////////////////////////////////////////////////////////////////////////
// CWoormView drawing

//------------------------------------------------------------------------------
void CWoormView::OnDraw(CDC* pDC)
{                
	// make equal draw for video and print and print preview
	// whit only few difference (like borders and labels)
	// also don't print active attribute and multiple selection
	DoDraw (pDC);
}

//------------------------------------------------------------------------------------------
void CWoormView::DoDraw	(CDC* pDC, CPrintInfo* pInfo)
{   
	GetDocument()->SetCustomUICulture();
	
	#ifndef TBWEB
	//----
	//Reset della descrizione dei link ogni volta che ridisegna la pagina
	//(m_pReportDesc e' valorizzato solo quando si e' in esecuzione tramite MagoWeb)
	if (m_pReportDesc)
		m_pReportDesc->m_arLinks.RemoveAll();
	#endif //TBWEB


	SymField* pF = GetDocument()->m_ViewSymbolTable.GetFieldByID(SpecialReportField::ID.PAGE);
	if (pF)
	{
		int np = 0;
		if (pInfo)
			np = pInfo->m_nCurPage;
		else if (GetDocument()->m_pRDEmanager)
			np = GetDocument()->m_pRDEmanager->CurrPageRead() + 1;
		if (np)
			pF->AssignData(DataLng(np));
	}
	//----

	CSize szPageSize ( 
						(pDC->IsPrinting() || (pInfo && pInfo->m_bPreview)) ?
							GetPrinterPageSizeWithoutMargin_LP() :
							GetPageSizeWithoutMargin_LP()
					);
	CRect page (0, 0, szPageSize.cx, szPageSize.cy);
	// gestione della clip area a video
	if (!pDC->IsPrinting())
	{
		CPoint sp = GetScrollPosition();
		/*ScalePoint(sp, *pDC);*/
		page.OffsetRect(-sp.x, -sp.y);
		// dimensiona la regione di clipping come la pagina di stampa
		CRgn rgn; 
		rgn.CreateRectRgnIndirect(page);
		pDC->SelectClipRgn(&rgn);
	}

	// in stampa non viene chiamato OnEraseBkgn per cui lo faccio qui per
	// disegnare il bitmap di sfondo a partire sempre dal punto 0,0 e per
	// permettere a video di mostrare la griglia (fatta in erasebkgn)
	//
	if (pDC->IsPrinting() && !GetDocument()->m_pOptions->NoBitmap(GetDocument(), TRUE))
		DrawBitmap
		(
			pDC, 
			GetDocument()->m_pOptions->m_BitmapOrigin, 
			GetDocument()->m_pOptions->m_strBkgnBitmap
		);

	//----
	CFont Ft;
	FontStyle* ft = AfxGetFontStyleTable()->GetFontStyle(AfxGetFontStyleTable()->GetFontIdx(FNT_DEFAULT), NULL);
	if (ft)
	{
		LOGFONT lf = ft->GetLogFont();

		CString sFaceName =  AfxGetFontAliasTable()->LookupFaceName(lf.lfFaceName);
		if (!sFaceName.IsEmpty())
			TB_TCSCPY(lf.lfFaceName, (LPCTSTR)sFaceName);

		ScaleLogFont(&lf, *pDC);

		if (Ft.CreateFontIndirect(&lf))
			pDC->SelectObject(&Ft);
	}
	if (Ft.m_hObject == NULL)
	{
		ASSERT(FALSE);
		pDC->SelectObject(AfxGetThemeManager()->GetFormFont());
	}
	//----
	
	// Note: CBCGPScrollView::OnPaint(); //will have already adjusted the
	// viewport origin before calling OnDraw(), to reflect the
	// currently scrolled position.

	// si delega ai singoli oggetti contenuti nel documento di disegnarsi
	GetDocument()->GetObjects().Paint(*pDC, pInfo);
	
	// se sono su stampante o in preview non stampo le caratteristiche
	// di selezione sia singola che multipla
	if (!pInfo)
	{
		// Paint eventual active object selection
		GetDocument()->m_pActiveRect->Paint(*pDC, pInfo);

		// Paint eventual active object selection
		if(m_pTransActiveRect)
			m_pTransActiveRect->Paint(*pDC, pInfo);

		// Paint eventual multiple selection
		if (GetDocument()->m_pMultipleSelObj)
			GetDocument()->m_pMultipleSelObj->Paint(*pDC, pInfo);

		// Paint eventual multiple selection
		if (GetDocument()->m_pMultiColumns)
			GetDocument()->m_pMultiColumns->Paint(*pDC, pInfo);
	}

	if (!pDC->IsPrinting() && (pInfo ? !pInfo->m_bPreview : TRUE))
	{	
		CLongArraySorted* pSplitter = &(GetDocument()->m_PageInfo.m_arHPageSplitter);

		if (pSplitter->GetSize())
		{
			CPen penSplitter (PS_DOT, 1, RGB(255,128,255));
			CPen* oldPen = pDC->SelectObject(&penSplitter);
			for (int i = 0; i < pSplitter->GetSize(); i++)
			{
				pDC->MoveTo(pSplitter->GetAt(i), 0);
				pDC->LineTo(pSplitter->GetAt(i), szPageSize.cy);
			}
			pDC->SelectObject(oldPen);
		}
	}

	// ripristino il font evetualmente modificato da qualche oggetto
	// contenente del testo e cancella quello usato per creare il font voluto
	GetDocument()->ResetCurrentFont(*pDC);

	if (!GetDocument()->m_bAllowEditing)	//Watermark SOPRA
		m_pWatermark->Draw(pDC, page);

	//----
	//GetDocument()->ResetCustomUICulture();
}

//------------------------------------------------------------------------------------------
void CWoormView::CalculateScaleFactor	(CPrintInfo* pInfo)
{	
	if (GetDocument()->m_PageInfo.m_bUsePrintableArea)
		GetDocument()->m_PageInfo.CalculateMargins(pInfo);
	
	int logicalPageWidth =  GetDocument()->m_PageInfo.m_arHPageSplitter.GetSize() ? 
								GetDocument()->GetSplittedPageMaxWidth() : 
								GetPageSizeWithoutMargin_LP().cx;
	int logicalPageLenght = GetPageSizeWithoutMargin_LP().cy;

	/*if (GetDocument()->GetObjects().m_bInvertOrientation)
	{
		int temp = logicalPageWidth;
		logicalPageWidth = logicalPageLenght;
		logicalPageLenght = temp;
	}*/

	m_ptOffsetOrgAfterScaling = CPoint(0,0);

	CSize szPageSize (GetPrinterPageSizeWithoutMargin_LP());
	LONG physicalPageWidth = szPageSize.cx;
	LONG physicalPageLenght = szPageSize.cy;
	
	if (
		GetDocument()->m_PageInfo.UsePrinterPage() &&
		(logicalPageWidth > physicalPageWidth || logicalPageLenght > physicalPageLenght)
		)
	{	
		double horizontalScaleFactor = min((double)physicalPageWidth / (double)logicalPageWidth, 1.0);
		double verticalScaleFactor = min((double)physicalPageLenght / (double)logicalPageLenght, 1.0);

		m_dScaleFactor = min(horizontalScaleFactor, verticalScaleFactor);
		m_dLowerScale = max(horizontalScaleFactor, verticalScaleFactor);
		if (m_dScaleFactor < 1.0)
		{
			m_nScaleDenom = 1000;
			double dScaleNum = m_dScaleFactor * m_nScaleDenom;
			round(dScaleNum, 0);
			m_nScaleNum = (int)dScaleNum;
			m_bDoScale = TRUE;
	
			if (horizontalScaleFactor > verticalScaleFactor) //zoom perche non ci stava in verticale
			{
				m_scaleType = ONLY_VERTICAL;
				if (horizontalScaleFactor < 1)
					m_scaleType = VERTICAL_MORETHAN_HORIZONTAL;

				m_ptOffsetOrgAfterScaling.x = int((physicalPageWidth - (logicalPageWidth * verticalScaleFactor )) / 2.0);
			}
			else if (horizontalScaleFactor < verticalScaleFactor) //zoom perche non ci stava in orizzontale
			{
				m_scaleType = ONLY_HORIZONTAL;
				if (verticalScaleFactor < 1)
					m_scaleType = HORIZONTAL_MORETHAN_VERTICAL;

				m_ptOffsetOrgAfterScaling.y = int((physicalPageLenght - (logicalPageLenght * horizontalScaleFactor )) / 2.0);
			}
		}
	}
	m_bCalculatedScale = TRUE;
}

//------------------------------------------------------------------------------
void CWoormView::OnPrepareDC(CDC* pDC, CPrintInfo* pInfo)
{
	// check for end of print
	if (pInfo && !pInfo->m_bContinuePrinting)
		return;

	CBCGPScrollView::OnPrepareDC(pDC, pInfo);
	
	if (pDC->IsPrinting() || (pInfo && pInfo->m_bPreview))
	{		
		if (!m_bCalculatedScale)
			CalculateScaleFactor(pInfo);
		
		// (!m_bPDFPrinting) An.17879: durante il salvataggio in pdf di un report che necessita di zoom, 
		// non deve effettuare lo scaling
		if (m_bDoScale && !m_bPDFPrinting)
		{
			int num = m_nScaleNum;
			if (pDC->GetMapMode() == MM_TEXT)
			{
				pDC->SetMapMode(MM_ANISOTROPIC);

				// MM_TEXT has WindowExt and ViewportExt of (1 x 1),
				// so ScaleViewportExt doesn't work properly.
				// We compensate by scaling both the WindowExt and ViewportExt.
		
				num *= m_nScaleDenom;

				pDC->ScaleWindowExt(m_nScaleDenom, 1, m_nScaleDenom, 1);
			}
			else 
				pDC->SetMapMode(MM_ANISOTROPIC);

			pDC->ScaleViewportExt(num, m_nScaleDenom, num, m_nScaleDenom);
			//----------------------------------
			//centratura

			CPoint ptOffset = GetPrintableOffset(pDC);
			CPoint ptMargins = GetDocument()->m_PageInfo.m_rectMargins.TopLeft();

			ScalePoint(ptMargins, *pDC);
			CPoint ptOffsetOrgAfterScaling(m_ptOffsetOrgAfterScaling);
			ScalePoint(ptOffsetOrgAfterScaling, *pDC);
			
			CPoint p 	(
							(ptMargins.x - ptOffset.x + ptOffsetOrgAfterScaling.x),
							(ptMargins.y - ptOffset.y + ptOffsetOrgAfterScaling.y) 
						);
			CPoint ptOldOrigin = pDC->SetViewportOrg (p.x, p.y);

			TRACE(cwsprintf(_T("\nSetViewportOrg %d %d %d\n"), ptMargins.x , ptOffset.x , ptOffsetOrgAfterScaling.x));
		}
	}
}

//------------------------------------------------------------------------------
void CWoormView::OnLinkSelected()
{
	GetDocument()->OnLinkSelected();
	m_pProcessingMouse->Unlock();
}

//------------------------------------------------------------------------------
void CWoormView::OnLButtonDown(UINT nFlags, CPoint point)
{
	ASSERT_VALID(this);
	ASSERT_VALID(GetDocument());

	if (
			GetDocument()->m_bAllowEditing &&
			GetWoormFrame()->m_pObjectPropertyView &&
			GetWoormFrame()->m_pObjectPropertyView->m_bNeedsApply
		)
	{
		//Viene chiesta la conferma per annullare le modifiche
		GetWoormFrame()->m_pObjectPropertyView->ClearPropertyGrid();
		//NON si può proseguire in entrambi i casi poichè l'interruzione del flusso manda in crisi la gestione del mouse
		m_pProcessingMouse->Unlock();
		GetDocument()->m_pCurrentObj = NULL;
		GetDocument()->DeleteDragRect();
		GetDocument()->OnDeselectAll();
		return;
	}

	// avoid processing of incoming OnLButtonDown
	// condizioni meno restrittive per esportazione: riverifico dopo
	if (GetDocument()->m_bEngineRunning || m_pProcessingMouse->IsLocked())
		return;
	
	ASSERT(m_pProcessingMouse);

	// normalize mouse position to reflect new window origin
	CClientDC dc(this);

	OnPrepareDC(&dc);
	GetDocument()->m_ptCurrPos = point;
	m_bSensibility = TRUE;
	dc.DPtoLP(&GetDocument()->m_ptCurrPos);

	CDC* pDC = GetDC();
	ASSERT(pDC);
	if (pDC)
	{
		UnScalePoint(GetDocument()->m_ptCurrPos, *pDC);
		ReleaseDC(pDC);
	}
	
	if (GetDocument()->IsMouseHoverLink() && !GetDocument()->m_bAllowEditing)
	{
		if	(
				GetDocument()->m_bEngineRunning || 
				!GetDocument()->IsDataLoaded()
			)
		{
			m_pProcessingMouse->Unlock();
			return;
		}

		OnLinkSelected();
		return;
	}

	// gestione della selezione degli oggetti per export data
	if	(GetDocument()->IsDataLoaded() && !GetDocument()->m_bAllowEditing && (nFlags & MK_CONTROL) == MK_CONTROL)
	{
		GetDocument()->SelectForExportData (GetDocument()->m_ptCurrPos, FALSE, (nFlags & MK_SHIFT) == MK_SHIFT);
		
		m_pProcessingMouse->Unlock();
		return;
	}

	if (GetDocument()->m_bPlayback || !GetDocument()->m_bAllowEditing)
	{
		m_pProcessingMouse->Unlock();
		return;
	}

	// capture mouse
	SetCapture(); 
							   
	// process new object creation
	if (GetDocument()->m_Creating != GetDocument()->NONE)
	{       
		// remove multiple selection
		if (GetDocument()->m_pMultipleSelObj)
			GetDocument()->OnDeselectAll();              
			
		// create new object and give it control
		GetDocument()->CreateListObj (dc, this);

		// reenable incoming buttondown
		m_pProcessingMouse->Unlock();
		return;
	}

	// se non ho la multipla selezione attiva si mette da parte il corrente oggetto
	// se esiste, per aggiungerlo alla multipla selezione se si aggiungono rettangoli
	// diversi da quello corrente
	BaseObj* pPrevCurrentObj = GetDocument()->m_pMultipleSelObj
		? NULL : GetDocument()->m_pCurrentObj;
	
	// try to find object and set it current altrimenti crea il drag object
	GetDocument()->SetCurrentObj(GetDocument()->m_ptCurrPos, (nFlags & MK_CONTROL) == MK_CONTROL);

	if (!GetDocument()->m_pCurrentObj)
	{
		if (GetDocument()->m_pMultipleSelObj)
			GetDocument()->OnDeselectAll();

		//pulisco la property grid
		if (GetWoormFrame()->m_pObjectPropertyView)
			GetWoormFrame()->m_pObjectPropertyView->ClearPropertyGrid();

		// click in area vuota allora prova a fare la multipla selezione
		// con la gestione del rettangolo di drag
		GetDocument()->CreateDragRect(dc, this);

		// reenable incoming buttondown
		m_pProcessingMouse->Unlock();
		return;
	}

	// watch control key status
	BOOL bControl = ((nFlags & MK_CONTROL) == MK_CONTROL);

	if (bControl && 
		((pPrevCurrentObj && pPrevCurrentObj->IsKindOf(RUNTIME_CLASS(Table))) || (GetDocument()->m_pMultiColumns)) &&
		GetDocument()->m_pCurrentObj && GetDocument()->m_pCurrentObj->IsKindOf(RUNTIME_CLASS(Table)))
	{
		CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Layouts);

		Table* pTable = (Table*)GetDocument()->m_pCurrentObj;
		TableColumn* pCol = pTable->GetActiveColumn();

		HTREEITEM htObj = pTree->FindItemData((DWORD)pCol, pTree->m_htLayouts);
		ASSERT(htObj);

		pTree->ToggleItemSelect(htObj);
		if (pTree->OnMultiSelect())
		{
			// reenable incoming buttondown
			m_pProcessingMouse->Unlock();
			return;
		}
	}

	// control multiple selection (control key holded down)
	if (bControl && !GetDocument()->m_pMultipleSelObj)
		GetDocument()->m_pMultipleSelObj = new SelectionRect (GetDocument());

	if (GetDocument()->m_pMultipleSelObj)
	{   
		// disable current object active enhancement during multiple selection status
		GetDocument()->m_pActiveRect->Clear();

		if (bControl)
		{
			//aggiunge anche il corrente oggetto se e' una nuova selezione e non ho cliccato sullo stesso
			if (pPrevCurrentObj && pPrevCurrentObj != GetDocument()->m_pCurrentObj)
				//adding previous selected obj
				GetDocument()->m_pMultipleSelObj->AddToSelectedObjects(pPrevCurrentObj, pPrevCurrentObj->GetBaseRect());

			if (GetDocument()->m_pMultipleSelObj->ObjectInSelection(GetDocument()->m_pCurrentObj))
				//removing previous selected obj
				GetDocument()->m_pMultipleSelObj->RemoveFromSelectedObjects(GetDocument()->m_pCurrentObj);
			else
			{
				Table* pTable = dynamic_cast<Table*>(GetDocument()->m_pCurrentObj);
				if (pTable)
					pTable->m_nActiveColumn = pTable->NO_ACTIVE_COLUMN;
				//adding current selected obj
				GetDocument()->m_pMultipleSelObj->AddToSelectedObjects(GetDocument()->m_pCurrentObj, GetDocument()->m_pCurrentObj->GetBaseRect());
			}
			int size = GetDocument()->m_pMultipleSelObj->GetSize();

			if (GetWoormFrame()->m_pObjectPropertyView)
				if (!GetWoormFrame()->m_pObjectPropertyView->LoadMultipleSelectionProperties(GetDocument()->m_pMultipleSelObj))
					//se restituisce false, vuol dire che ha gestito lui la selezione singola
					return;

			//GetDocument()->m_pMultipleSelObj->LeftButtonDown(dc, GetDocument()->m_ptCurrPos, this);
		}
		else
		{
			if (GetDocument()->m_pMultipleSelObj->ObjectInSelection(GetDocument()->m_pCurrentObj))
				GetDocument()->m_pMultipleSelObj->LeftButtonDown(dc, GetDocument()->m_ptCurrPos, this);
			else
			{
				//tentativo risoluzione click su un oggetto singolo con multiselezione attiva
				GetDocument()->OnDeselectAll();
				m_pProcessingMouse->Unlock();
				OnLButtonDown(nFlags, point);
				return;
			}
		}

		// reenable incoming buttondown
		m_pProcessingMouse->Unlock();
		return;
	}

	// allora da il controllo all'oggetto selezionato
	GetDocument()->m_pCurrentObj->LeftButtonDown(dc, GetDocument()->m_ptCurrPos, this);

	// reenable incoming buttondown
	m_pProcessingMouse->Unlock();
}

//------------------------------------------------------------------------------
void CWoormView::OnMouseMove(UINT nFlags, CPoint point)
{
	ASSERT_VALID(this);
	ASSERT_VALID(GetDocument());
	ASSERT(m_pProcessingMouse);
	
	// avoid processing of incoming mouseMove
	if (Disabled() || m_pProcessingMouse->IsLocked())
	{
		if (GetDocument()->m_bAllowEditing)
			return;

		// normalize mouse position to reflect new window origin
		CClientDC dc1(this);
		OnPrepareDC(&dc1);
		GetDocument()->m_ptCurrPos = point;
		dc1.DPtoLP(&GetDocument()->m_ptCurrPos);

		CDC* pDC = GetDC();
		ASSERT(pDC);
		if (pDC)
		{
			UnScalePoint(GetDocument()->m_ptCurrPos, *pDC);
			ReleaseDC(pDC);
		}

		CString sTip = GetDocument()->OnMouseHoverLink();
		m_pDataTip->Set(point, sTip);	//ok anche se empty

		return;
	}
	CClientDC dc(this);
	OnPrepareDC(&dc);
	dc.DPtoLP(&point);

	CPoint mousePint(point);
	CDC* pDC = GetDC();
	ASSERT(pDC);
	if (pDC)
	{
		UnScalePoint(mousePint, *pDC);
		ReleaseDC(pDC);
	}
	
	CSize size = GetDocument()->m_ptCurrPos - mousePint;

	if	(
			!m_bSensibility || 
			abs(size.cx) > (int) GetDocument()->m_pWoormIni->m_nMouseSensibility || 
			abs(size.cy) > (int) GetDocument()->m_pWoormIni->m_nMouseSensibility
		)
	{	
		if (GetDocument()->m_pMultipleSelObj)	
			GetDocument()->m_pMultipleSelObj->	MouseMove(dc, point, this);
		else if (GetDocument()->m_pCurrentObj && GetDocument()->m_pCurrentObj->m_hWnd)
			GetDocument()->m_pCurrentObj->		MouseMove(dc, point, this, (GetKeyState(VK_MENU) & 0x8000)/*= ALT*/);
		else if (GetDocument()->m_pDragRectObj)		
			GetDocument()->m_pDragRectObj->		MouseMove(dc, point, this);
	}
	// reenable incoming buttondown
	m_pProcessingMouse->Unlock();
	
	return;
}


//------------------------------------------------------------------------------
void CWoormView::CancelDragToTransform()
{
	if (m_pTransActiveRect)
		m_pTransActiveRect->Clear();

	if (m_nTransStartColAlias >= 0 || m_nTransDestColAlias >= 0 || m_pTransStartTable)
	{
		m_nTransStartColAlias = -1;
		m_nTransDestColAlias = -1;
		m_pTransStartTable = NULL;

		ASSERT(ClipCursor(NULL) != 0);
		GetDocument()->Invalidate(TRUE);
	}
}


//------------------------------------------------------------------------------
void CWoormView::ManageDragToTransform(CPoint point)
{
	BaseObj* pObj = dynamic_cast<BaseObj*>(GetDocument()->m_pCurrentObj);
	if (!pObj->TrackInProgress())
		return;
	//--------------------------------------------------------------------------------------
	//drag and drop fieldrect su tabelle per trasformarle in colonne e viceversa
	if (GetDocument()->m_pActiveRect)
	{
		Table* pCurrentTab = dynamic_cast<Table*>(pObj);
		if (pCurrentTab)
		{
			m_pTransStartTable = pCurrentTab;
			TableColumn* pCol = pCurrentTab->GetActiveColumn();
			if (pCol)
			{
				m_nTransStartColAlias = pCol->GetInternalID();

				Table* pTab = GetTableAtPoint(point);
				if (pTab)//spostamento colonna
				{
					int nCol = pTab->GetColumnIdxByPoint(point);
					if (nCol >= 0)
					{
						const TableColumn* destCol = pTab->GetColumn(nCol);
						CRect colRect = destCol->GetColumnRect();
						CRect activeRect = m_pTransActiveRect? m_pTransActiveRect->GetOriginalRect(): GetDocument()->m_pActiveRect->GetOriginalRect();

						int destColId = destCol->GetInternalID();
						int colId =    pCol->GetInternalID();

						if (activeRect != colRect && destCol->GetInternalID() != pCol->GetInternalID())
						{
							if (!m_pTransActiveRect)
								m_pTransActiveRect = new NewActiveRect(GetDocument()->m_pWoormIni,GetDocument(), RGB(0, 148,255));

							m_pTransActiveRect->Clear();
							m_pTransActiveRect->SetActive(pTab->GetColumn(nCol)->GetColumnRect());
							m_pTransActiveRect->Redraw();
							
							m_nTransDestColAlias = destCol->GetInternalID();
						}
						else if (destCol->GetInternalID() == pCol->GetInternalID())
						{
							if(m_pTransActiveRect)
								m_pTransActiveRect->Clear();

							m_nTransDestColAlias = -1;
						}
					}
					else
					{
						CancelDragToTransform();
					}
				}
				else //TODO trasformazione in campo singolo
				{
					CancelDragToTransform();
				}
			}
		}

		FieldRect* pField = dynamic_cast<FieldRect*>(pObj);
		if (pField)
		{
			if (!(pField->GetAction() == SingleItemObj::ActionType::MOVE))
			{
				GetDocument()->m_pActiveRect->Clear();
				GetDocument()->m_pActiveRect->SetActive(pField->GetBaseRect());
				GetDocument()->m_pActiveRect->Redraw();
				return;
			}
			Table* pTab = GetTableAtPoint(point);
			if (pTab)
			{
				m_pTransStartTable = pTab;
				GetDocument()->b_TransformColumnOrField = TRUE;
				int nCol = pTab->GetColumnIdxByPoint(point);
				if (nCol >= 0)
				{
					if (!m_pTransActiveRect)
						m_pTransActiveRect = new NewActiveRect(GetDocument()->m_pWoormIni, GetDocument(), RGB(0, 148, 255));
					const TableColumn* destCol = pTab->GetColumn(nCol);
					if (destCol)
					{
						m_nTransDestColAlias = destCol->GetInternalID();
						CRect colRect =destCol->GetColumnRect();
						CRect activeRect = m_pTransActiveRect->GetOriginalRect();
						if (activeRect != colRect)
						{
							m_pTransActiveRect->Clear();
							m_pTransActiveRect->SetActive(colRect);
							m_pTransActiveRect->Redraw();
						}
						return;
					}
				}
			}
			else
			{
				//CancelDragToTransform();

				if (GetDocument()->b_TransformColumnOrField == TRUE)
					GetDocument()->b_TransformColumnOrField = FALSE;	
			}
		}
	}
	//--------------------------------------------------------------------------------------
	else
	{
	}
}

//------------------------------------------------------------------------------
void CWoormView::OnLButtonUp(UINT, CPoint)
{
	ASSERT_VALID(this);
	ASSERT_VALID(GetDocument());

	// If this window (view) didn't capture the mouse, then the user isn't 
	// drawing in this window.
	if (GetCapture() != this) 
	{
		return; 
	}

	// avoid processing of incoming OnLButtonUp
	ASSERT(m_pProcessingMouse);
	if (Disabled() || m_pProcessingMouse->IsLocked()) 
		return;

	// CBCGPScrollView changes the viewport origin and mapping mode.
	CClientDC dc(this);
	OnPrepareDC(&dc);
	m_bSensibility = FALSE;

	if (GetDocument()->m_pMultipleSelObj)	
	{
		ASSERT_VALID(GetDocument()->m_pMultipleSelObj);
		GetDocument()->m_pMultipleSelObj->LeftButtonUp(dc, this);
		GetDocument()->UpdateWindow(); // permette di ridisegnare l'ultima selezione
	}
	else if (GetDocument()->m_pCurrentObj)
	{
		ASSERT_VALID(GetDocument()->m_pCurrentObj);
		GetDocument()->m_pCurrentObj->LeftButtonUp(dc, this);
	}
	else if (GetDocument()->m_pDragRectObj)	
	{
		ASSERT_VALID(GetDocument()->m_pDragRectObj);
		GetDocument()->m_pDragRectObj->LeftButtonUp(dc, this);
	}

	// elimina (se esiste) il rettangolo di draging
	GetDocument()->DeleteDragRect();

	// reenable incoming buttondown
	ReleaseCapture();
	m_pProcessingMouse->Unlock();

	ASSERT(GetDocument()->m_pWoormIni);
	if (GetDocument()->m_pWoormIni->m_bSnapToGrid)
		GetDocument()->DoSnapToGrid();

	if (m_pTransActiveRect)
		m_pTransActiveRect->Clear();

	if (m_pTransStartTable && m_nTransStartColAlias >= 0 && m_nTransDestColAlias >= 0)		//spostamento colonne
		m_pTransStartTable->MoveColumn(m_nTransStartColAlias, m_nTransDestColAlias);
	else if (m_pTransStartTable && m_nTransStartColAlias < 0 && m_nTransDestColAlias >= 0)	//field -> colonna
		TransformFieldRectInColumn();
	
	CancelDragToTransform();
	
	return;
}


void CWoormView::TransformFieldRectInColumn()
{
	ASSERT_VALID(GetDocument()->m_pCurrentObj);
	FieldRect* pField = dynamic_cast<FieldRect*>(GetDocument()->m_pCurrentObj);
	if (!pField || !m_pTransStartTable)
		ASSERT(FALSE);
	else
	{
		int destColIdx = m_pTransStartTable->GetColumnIndexByAlias(m_nTransDestColAlias);
		if (destColIdx < 0)
			ASSERT(FALSE);
		else
		{
			//mi salvo la variabile che diventerà hidden per un breve lasso di tempo
			WoormField* wrmField = GetDocument()->GetEditorSymTable()->GetFieldByID(pField->GetInternalID());
			ASSERT_VALID(wrmField);
			if (wrmField)
			{
				if (wrmField->IsAsk() || wrmField->IsArray())
				{
					AfxMessageBox(_TB("This field is linked to a variable not compatible with the type table"));
					return;
				}
				//rimuovo il fieldrect
				OnObjectCut();

				GetDocument()->m_wCreatingColumnIds.RemoveAll();

				wrmField->SetHidden(FALSE);
				wrmField->SetLen(AfxGetFormatStyleTable()->GetInputCharLen(wrmField->GetDataType(), &GetDocument()->GetNamespace()));
				wrmField->SetFieldType(WoormField::RepFieldType::FIELD_COLUMN);
				wrmField->SetDispTable(m_pTransStartTable->GetName(TRUE));

				GetDocument()->m_wCreatingColumnIds.Add(wrmField->GetId());

				GetDocument()->m_pCurrentObj = NULL;
				GetDocument()->m_pActiveRect->Clear();

				m_pTransStartTable->AddColumn(GetDocument()->m_wCreatingColumnIds);

				GetDocument()->SyncronizeViewSymbolTable(GetDocument()->GetEditorSymTable());

				GetDocument()->RefreshRSTree(ERefreshEditor::Variables);
				GetDocument()->RefreshRSTree(ERefreshEditor::Layouts);

				GetWoormFrame()->SelectLayoutObject((CObject*)m_pTransStartTable);
				//seleziono la colonna
			}
		}
	}
}

//------------------------------------------------------------------------------
void CWoormView::OnRButtonDown(UINT /*nflag*/, CPoint ptMousePos)
{
	// avoid processing of incoming OnRButtonDown
	if (GetDocument()->m_bEngineRunning) 
		return;
						
	if (!GetDocument()->m_bAllowEditing)
	{
		// normalize mouse position to reflect new window origin
		CClientDC dc(this);
		OnPrepareDC(&dc);
		GetDocument()->m_ptCurrPos = ptMousePos;
		dc.DPtoLP(&GetDocument()->m_ptCurrPos);
		//----
		GetDocument()->m_sCopyText.Empty();

		int nRow = -1;
		int nAlias = 0;
		BaseObj* pTableOrRepeater = NULL;
		BaseRect* pField = NULL;
	
		if (!GetDocument()->GetAliasObjectFromPoint(nAlias, nRow, pTableOrRepeater, pField))
		{
			return;
		}

		if (pField && pField->IsKindOf(RUNTIME_CLASS(FieldRect)))
		{
			GetDocument()->m_sCopyText = ((FieldRect*)pField)->GetText();
		}
		else if (pTableOrRepeater && pTableOrRepeater->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pT = (Table*) pTableOrRepeater;
			TableCell* pTC = pT->GetCellFromID(nRow, nAlias);
			if (pTC)
			{
				GetDocument()->m_sCopyText = pTC->GetText();
			}
		}

		if (GetDocument()->m_sCopyText.IsEmpty())
			return;

		CMenu menu;
		menu.CreatePopupMenu();
		//menu.AppendMenu (MF_STRING, ID_COPY_TEXT, (LPTSTR) (LPCTSTR) (_TB("Copy:") + ' ' + GetDocument()->m_sCopyText));
		menu.AppendMenu (MF_STRING, ID_COPY_TEXT, (LPTSTR) (LPCTSTR) _TB("Copy"));
		this->ClientToScreen(&ptMousePos);
		menu.TrackPopupMenu (TPM_LEFTALIGN|TPM_RIGHTBUTTON, ptMousePos.x, ptMousePos.y, this);
		menu.DestroyMenu();

		return;
	}
	//------------------------------------------------------

	if (GetDocument()->m_bPlayback) 
		return;
		
	if (m_pProcessingMouse->IsLocked()) 
		return;

	// right button enabled only in non tracking mode
	if	(
			GetDocument()->m_pCurrentObj && 
			GetDocument()->m_pCurrentObj->TrackInProgress()
		) 
	{
		m_pProcessingMouse->Unlock();
		return;
	}

	//------------------------------------------------------

	// deselect all eventual multiple selected GetObjects()                            
	if (GetDocument()->m_pMultipleSelObj) 
		GetDocument()->OnDeselectAll();

	// delegate processing to object and make it current 
	GetDocument()->SetCurrentObj(GetDocument()->m_ptCurrPos);
	if (GetDocument()->m_pCurrentObj)
	{
		CRect rect = GetDocument()->m_pCurrentObj->GetActiveRect();
		if (GetDocument()->m_pActiveRect->SetActive(rect))
			GetDocument()->UpdateWindow ();

		if (GetDocument()->m_pCurrentObj->InMe(GetDocument()->m_ptCurrPos))
			GetDocument()->m_pCurrentObj->ShowPopup(ptMousePos, this);
	}

	m_pProcessingMouse->Unlock();
}

//------------------------------------------------------------------------------
BOOL CWoormView::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message)
{                      
	// cursor image will be handled in mouse move	
	if (pWnd != this || nHitTest != HTCLIENT)
		return CBCGPScrollView::OnSetCursor(pWnd, nHitTest, message);

	if (GetDocument()->m_Creating != GetDocument()->NONE)
	{
		GetDocument()->SetAppropriateCursor();
		return TRUE;
	}
	
	if (!GetDocument()->m_pCurrentObj)
		SetCursor(AfxGetApp()->LoadStandardCursor(IDC_ARROW));
	
	return TRUE;        
}

/////////////////////////////////////////////////////////////////////////////
// CWoormView Scroll support

//------------------------------------------------------------------------------
void CWoormView::OnScrollUp		() { OnVScroll (SB_PAGEUP,		0, GetScrollBarCtrl(SB_VERT)); }
void CWoormView::OnScrollDown	() { OnVScroll (SB_PAGEDOWN,	0, GetScrollBarCtrl(SB_VERT)); }
void CWoormView::OnScrollLeft	() { OnHScroll (SB_PAGELEFT,	0, GetScrollBarCtrl(SB_HORZ)); }
void CWoormView::OnScrollRight	() { OnHScroll (SB_PAGERIGHT,	0, GetScrollBarCtrl(SB_HORZ)); }

//------------------------------------------------------------------------------
void CWoormView::OnScrollTop	() 
{
	OnVScroll (SB_TOP,	0, GetScrollBarCtrl(SB_VERT));
	OnHScroll (SB_LEFT,	0, GetScrollBarCtrl(SB_HORZ));
}

//------------------------------------------------------------------------------
void CWoormView::OnScrollBotton	()
{
	OnVScroll (SB_BOTTOM,	0, GetScrollBarCtrl(SB_VERT));
	OnHScroll (SB_LEFT,		0, GetScrollBarCtrl(SB_HORZ));
}

/////////////////////////////////////////////////////////////////////////////
//							CWoormView printing
/////////////////////////////////////////////////////////////////////////////

// VECCHIA VERSIONE DI OnFilePrint
// Attenzione non viene gestita la collate sequence se la stampante non la supporta
//------------------------------------------------------------------------------
void CWoormView::OnFilePrint()
{
	GetDocument()->m_bIsPrinting = TRUE;

	if (GetDocument()->m_pWoormInfo)
		GetDocument()->m_pWoormInfo->m_bPrintAborted = FALSE;

	if (GetDocument()->m_bMustReRun)
	{
		if (GetDocument()->m_pWoormInfo)
		{
			if (!GetDocument()->m_pWoormInfo->m_bAutoPrint)
			{
				GetDocument()->m_pWoormInfo->m_bAutoPrint = TRUE;
				GetDocument()->m_pWoormInfo->m_bShowPrintDialogBeforeRunning = GetDocument()->m_bMultiCopies;

				GetDocument()->OnRun();

				GetDocument()->m_pWoormInfo->m_bAutoPrint = FALSE;

				GetDocument()->m_bMustReRun = FALSE;
				
				GetDocument()->m_bIsPrinting = FALSE;
				return;
			}
		}
		else
		{
			GetDocument()->m_pWoormInfo = new CWoormInfo();
			GetDocument()->m_pWoormInfo->m_bAutoPrint = TRUE;
			GetDocument()->m_pWoormInfo->m_bShowPrintDialogBeforeRunning = GetDocument()->m_bMultiCopies;
			GetDocument()->m_pWoormInfo->m_bIconized = TRUE;

			GetDocument()->m_bCloseChildOnEndPrint = TRUE;

			GetDocument()->OnRun();

			SAFE_DELETE (GetDocument()->m_pWoormInfo);
			GetDocument()->m_pWoormInfo = NULL;

			GetDocument()->m_bMustReRun = FALSE;
			GetDocument()->m_bCloseChildOnEndPrint = FALSE;

			GetDocument()->m_bIsPrinting = FALSE;
			return;
		}
	}
	
	// determina cosa la stampante sa fare e setta i vari flag necessari alla stampa
	GetPrinterCapability(NULL);

	//#an. 7980
	CString strLocal = ::_tsetlocale (LC_ALL, NULL);

	// se il driver supporta la multipla copia allora lo faccio fare a lui
	// ed e' qui alla prima copia che compare la printdialog con la richiesta
	// del numero di copie da stampare.
	if	( GetDocument()->IsRunningFromExternalController() )
	{
		AfxGetTBThread()->IncSuspendIdle();

		OnSilentFilePrint(); // Se la stampa del report viene lanciata dal controller esterno (scheduler)
							 // non si devono visualizzare dialog di stampa o messaggi di errore 

		AfxGetTBThread()->DecSuspendIdle();
	}
	else
		CBCGPScrollView::OnFilePrint();

	// altrimento lancio piu' volte la stampa di tutte le pagine.
	if (!m_bSupportCopies && !AfxIsRemoteInterface() && GetDocument()->m_pWoormInfo) 
		for (m_nCurrentCopy = 2; m_nCurrentCopy <= GetDocument()->m_pWoormInfo->m_nCopies; m_nCurrentCopy++)
			CBCGPScrollView::OnFilePrint();

	//#an. 7980
	CString strLocal2 = ::_tsetlocale (LC_ALL, NULL);
	if (strLocal != strLocal2)
	{
		::_tsetlocale (LC_ALL, strLocal);
	}

	// resetta a valori iniziali per riabilitare la richiesta dei parametri
	// con la file dialog (vedi OnPreparePrinting)
	m_nCurrentCopy = 1;
	GetDocument()->m_bIsPrinting = FALSE;
}
/////////////////////////////////////////////////////////////////////////////
// CWoormView::OnSilentFilePrint()
//
// Carlotta:	Ho cut&pastato e corretto il metodo di MFC CView::OnFilePrint  
//				perche' presentava gravi lacune riguardanti la diagnostica...
//				Infatti, esse erano gia' state rilevate da Enrico: basta vedere
//				il suo commento nella sovrastante OnEndPrinting!!!!
//				Inoltre, non mostro alcuna dialog con messaggi concernenti l'andamento
//				della stampa (stampante e/o numero di pagina in stampa,...) e, 
//				invece delle visualizzazione di MsgBox di errore, imposto degli
//				appropriati messaggi di errore che non bloccano la chiusura del
//				report
//////////////////////////////////////////////////////////////////////////////
BOOL CALLBACK SilentPrintAbortProc(HDC, int)
{
	MSG msg;
	while (::PeekMessage(&msg, NULL, NULL, NULL, PM_NOREMOVE))
	{
		if (!AfxGetThread()->PumpMessage())
			return FALSE;   // terminate if WM_QUIT received
	}
	return TRUE;
}
//////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------

void CWoormView::OnSilentFilePrint (LPCTSTR pstrFileName /*= NULL*/, PrintType printT /*= PT_NORMAL*/)
{
	ASSERT (printT == PT_NORMAL || printT == PT_EMF || printT == PT_BMP);

	m_PrintStatus = NO_PRINT_ERROR;
	
	// get default print info
	CPrintInfo printInfo;
	ASSERT(printInfo.m_pPD != NULL);    // must be set

	if (LOWORD(GetCurrentMessage()->wParam) == ID_FILE_PRINT_DIRECT)
	{
		CCommandLineInfo* pCmdInfo = AfxGetApp()->m_pCmdInfo;

		if (pCmdInfo != NULL)
		{
			if (pCmdInfo->m_nShellCommand == CCommandLineInfo::FilePrintTo)
			{
				printInfo.m_pPD->m_pd.hDC = ::CreateDC(pCmdInfo->m_strDriverName,
					pCmdInfo->m_strPrinterName, pCmdInfo->m_strPortName, NULL);
				if (printInfo.m_pPD->m_pd.hDC == NULL)
				{
					m_PrintStatus = FAILED_TO_START_PRINT;
					return;
				}
			}
		}
		printInfo.m_bDirect = TRUE;
	}

	if (OnPreparePrinting(&printInfo))
	{
		// hDC must be set (did you remember to call DoPreparePrinting?)
		ASSERT(printInfo.m_pPD->m_pd.hDC != NULL);

		// gather file to print to if print-to-file selected
		CString strOutput;
		if (printT == PT_NORMAL && (printInfo.m_pPD->m_pd.Flags & PD_PRINTTOFILE))
		{
			// construct CFileDialog for browsing
			CString strDef(MAKEINTRESOURCE(AFX_IDS_PRINTDEFAULTEXT));
			CString strPrintDef(MAKEINTRESOURCE(AFX_IDS_PRINTDEFAULT));
			CString strFilter(MAKEINTRESOURCE(AFX_IDS_PRINTFILTER));
			CString strCaption(MAKEINTRESOURCE(AFX_IDS_PRINTCAPTION));

			// Consento di browsare su tutto il disco, senza utilizzare 
			// la TBExplorer che limiterebbe la funzionalità
			CFileDialog dlg(FALSE, strDef, strPrintDef,
				OFN_HIDEREADONLY|OFN_OVERWRITEPROMPT, strFilter);
			dlg.m_ofn.lpstrTitle = strCaption;

			if (dlg.DoModal() != IDOK)
				return;

			// set output device to resulting path name
			strOutput = dlg.GetPathName();
		}

		// set up document info and start the document printing process
		CString strTitle;
		CDocument* pDoc = GetDocument();
		if (pDoc != NULL)
			strTitle = pDoc->GetTitle();
		else
			GetWoormFrame()->GetWindowText(strTitle);
		if (strTitle.GetLength() > 31)
			strTitle.ReleaseBuffer(31);

		DOCINFO docInfo;
		memset(&docInfo, 0, sizeof(DOCINFO));
		docInfo.cbSize = sizeof(DOCINFO);
		docInfo.lpszDocName = strTitle;
		int nFormatID;
		if (strOutput.IsEmpty())
		{
			docInfo.lpszOutput = NULL;
			nFormatID = AFX_IDS_PRINTONPORT;
		}
		else
		{
			docInfo.lpszOutput = strOutput;
			nFormatID = AFX_IDS_PRINTTOFILE;
		}

		//Prologo
		CBitmap bmp;
		CDC dcPrint;
		HDC hDC = 0;
		CString strFileName = pstrFileName ? pstrFileName : _T("");
		CString strFilePage;
		int nNumPage = 1;

		if (printT == PT_EMF)
		{
			if( ! strFileName.Right(4).CompareNoCase(_T(".emf")) )
				strFileName = strFileName.Left(strFileName.GetLength()-4);

			if ( ! strFileName.IsEmpty() ) 
				strFilePage.Format(_T("%s%0d.emf"), strFileName, nNumPage);
			else
				strFilePage = _T("");

			// create the new DC in memory
			hDC = CreateEnhMetaFile 
				( 
					printInfo.m_pPD->m_pd.hDC, 
					strFilePage.IsEmpty() ? NULL: (LPCTSTR) strFilePage,
					NULL,  NULL
				);
			ASSERT (hDC != 0);
			if(hDC == 0) 
			{
				TRACE0("Error: Create EnhMetaFile DC failed");
				return;
			}        
		}
		else if (printT == PT_BMP)
		{
			if( ! strFileName.Right(4).CompareNoCase(_T(".bmp")) )
				strFileName = strFileName.Left(strFileName.GetLength()-4);

			if ( ! strFileName.IsEmpty() ) 
				strFilePage.Format(_T("%s%0d.bmp"), strFileName, nNumPage);
			else
				strFilePage = "";

			// create the new DC in memory
			hDC = ::CreateCompatibleDC (printInfo.m_pPD->m_pd.hDC);
			ASSERT (hDC != 0);
			if(hDC == 0) 
			{
				TRACE0("Error: Create compatible memory DC failed");
				return;
			}  
		}
		else if (printT == PT_NORMAL)
		{
			hDC = printInfo.m_pPD->m_pd.hDC;
		}
		dcPrint.Attach(hDC);
		
		dcPrint.m_bPrinting = TRUE;
		
		OnBeginPrinting(&dcPrint, &printInfo);
		
		dcPrint.SetAbortProc(SilentPrintAbortProc);

		// disable main window while printing & init printing status dialog
		AfxGetMainWnd()->EnableWindow(FALSE);

		// start document printing process
		if (printT == PT_NORMAL && dcPrint.StartDoc(&docInfo) == SP_ERROR)
		{
			// enable main window before proceeding
			AfxGetMainWnd()->EnableWindow(TRUE);

			m_PrintStatus = FAILED_TO_START_PRINT;
			// cleanup and show error message
			OnEndPrinting(&dcPrint, &printInfo);
			dcPrint.Detach();   // will be cleaned up by CPrintInfo destructor
			return;
		}

		// Guarantee values are in the valid range
		UINT nEndPage = printInfo.GetToPage();
		UINT nStartPage = printInfo.GetFromPage();

		if (nEndPage < printInfo.GetMinPage())
			nEndPage = printInfo.GetMinPage();
		if (nEndPage > printInfo.GetMaxPage())
			nEndPage = printInfo.GetMaxPage();

		if (nStartPage < printInfo.GetMinPage())
			nStartPage = printInfo.GetMinPage();
		if (nStartPage > printInfo.GetMaxPage())
			nStartPage = printInfo.GetMaxPage();

		int nStep = (nEndPage >= nStartPage) ? 1 : -1;
		nEndPage = (nEndPage == 0xffff) ? 0xffff : nEndPage + nStep;

		CString strTemp;
		VERIFY(strTemp.LoadString(AFX_IDS_PRINTPAGENUM));

		// begin page printing loop
		BOOL bError = FALSE;
		for (printInfo.m_nCurPage = nStartPage;
			printInfo.m_nCurPage != nEndPage; printInfo.m_nCurPage += nStep)
		{
			if (printT == PT_EMF)
			{
				if (nNumPage > 1)
				{
					if ( ! strFileName.IsEmpty () ) 
						strFilePage.Format(_T("%s%0d.emf"), strFileName, nNumPage);
					else
						strFilePage = _T("");

					hDC = CreateEnhMetaFile 
						( 
							printInfo.m_pPD->m_pd.hDC, 
							(LPCTSTR) strFilePage,
							NULL,  NULL
						);
					ASSERT (hDC != 0);
					if(hDC == 0) 
					{
						TRACE0("Error: Create EnhMeta File DC failed");
						return;
					}                   
					dcPrint.Attach(hDC);  // attach printer DC or emf DC
				}
				dcPrint.m_bPrinting = TRUE;
			}
			else if (printT == PT_BMP)
			{
				if (nNumPage > 1)
				{
					if ( ! strFileName.IsEmpty () ) 
						strFilePage.Format(_T("%s%0d%s"), strFileName, nNumPage, printT == PT_BMP ? ".bmp" : ".png");
					else
						strFilePage = _T("");

					// create the new DC in memory
					hDC = ::CreateCompatibleDC (printInfo.m_pPD->m_pd.hDC); 
					ASSERT (hDC != 0);
					if(hDC == 0) 
					{
						TRACE0("Error: Create compatible memory DC failed");
						return;
					}                   	
					dcPrint.Attach(hDC);  
				}
				dcPrint.m_bPrinting = TRUE;
			}
			
			nNumPage++;
			OnPrepareDC (&dcPrint, &printInfo);

			// check for end of print
			if ( ! printInfo.m_bContinuePrinting )
				break;
/*
HORZSIZE   Width of the physical display (in millimeters).
VERTSIZE   Height of the physical display (in millimeters).
HORZRES   Width of the display (in pixels).
VERTRES   Height of the display (in raster lines).
LOGPIXELSX   Number of pixels per logical inch along the display width.
LOGPIXELSY   Number of pixels per logical inch along the display height.
ASPECTX   Relative width of a device pixel as used for line drawing.
ASPECTY   Relative height of a device pixel as used for line drawing.
ASPECTXY   Diagonal width of the device pixel as used for line drawing.
*/

			// write current page
			const rsize_t nLen = 80;
			TCHAR szBuf[nLen];
			_sntprintf_s(szBuf, nLen, sizeof szBuf, strTemp, printInfo.m_nCurPage);

			// set up drawing rect to entire page (in logical coordinates)
			int nHORZRES = dcPrint.GetDeviceCaps(HORZRES);
			int nVERTRES = dcPrint.GetDeviceCaps(VERTRES);
			int nLOGPIXELSX = dcPrint.GetDeviceCaps(LOGPIXELSX);
			int nLOGPIXELSY = dcPrint.GetDeviceCaps(LOGPIXELSY);
			int nXPixelMeter	= (int) (nLOGPIXELSX * 1000.0 / 25.4);
			int nYPixelMeter	= (int) (nLOGPIXELSY * 1000.0 / 25.4);

			if(!GetDocument()->GetObjects().m_bInvertOrientation)
				printInfo.m_rectDraw.SetRect(0, 0, nHORZRES, nVERTRES);
			else
				printInfo.m_rectDraw.SetRect(0, 0, nVERTRES, nHORZRES);
			dcPrint.DPtoLP(&printInfo.m_rectDraw);
			
			if (printT == PT_BMP)
			{
				if ( ! bmp.CreateCompatibleBitmap(&dcPrint, printInfo.m_rectDraw.Width() ,printInfo.m_rectDraw.Height()))
				{ 
					TRACE0("Error: Create compatible Bitmap failed");
					return;
				}  

				dcPrint.SelectObject (&bmp);

				dcPrint.IntersectClipRect(CRect(0,0,printInfo.m_rectDraw.Width() ,printInfo.m_rectDraw.Height()));
				dcPrint.FillSolidRect(CRect(0,0,printInfo.m_rectDraw.Width() ,printInfo.m_rectDraw.Height()), RGB(255,255,255));	
			}

			// attempt to start the current page
			if (printT == PT_NORMAL)
			{
				if (dcPrint.StartPage() < 0)
				{
					bError = TRUE;
					break;
				}
			}

			// must call OnPrepareDC on newer versions of Windows because
			// StartPage now resets the device attributes.
			if (GetProcessVersion(0) >= 0x00040000)
				OnPrepareDC(&dcPrint, &printInfo);

			ASSERT(printInfo.m_bContinuePrinting);
			
			//====
			// page successfully started, so now render the page
			OnPrint(&dcPrint, &printInfo);
			//====

			// attempt to end the current page
			if (printT == PT_EMF)
			{
				dcPrint.Detach();  

				HENHMETAFILE hEmf = CloseEnhMetaFile (hDC);
				
				if ( hEmf && strFileName.IsEmpty() )
				{
					OpenClipboard ();
					EmptyClipboard ();
					::SetClipboardData ( CF_ENHMETAFILE, hEmf );
					CloseClipboard ();
				}
				if (hEmf) 
					DeleteEnhMetaFile(hEmf); 
			}
			else if (printT == PT_BMP)
			{
				dcPrint.Detach();

				if (strFileName.IsEmpty())
				{					
					BOOL bRet = OpenClipboard ();
					EmptyClipboard ();
					HANDLE h = SetClipboardData
						(
							CF_DIB,
							BitmapToDIB
								(
									(HBITMAP) bmp.GetSafeHandle(),
									(HPALETTE)GetSystemPaletteUse(hDC), 
									nXPixelMeter, nYPixelMeter
								)
						);
					CloseClipboard ();
				}
				else
				{
					HANDLE hDib = BitmapToDIB
								(
									(HBITMAP) bmp.Detach(),
									(HPALETTE)GetSystemPaletteUse(hDC),
									nXPixelMeter, nYPixelMeter 
								);

					CDIBitmap dib;
					dib.Attach(hDib);
					dib.SaveBitmap(strFilePage);
				}
			}
			else if (printT == PT_NORMAL)
			{
				if ( dcPrint.EndPage() < 0 || ! SilentPrintAbortProc(dcPrint.m_hDC, 0) )
				{
					bError = TRUE;
					break;
				}
			}
		}

		m_PrintStatus = bError ? PRINT_ABORT : NO_PRINT_ERROR;
		// cleanup document printing process
		if (printT == PT_NORMAL)
		{
			if (!bError)
				dcPrint.EndDoc();
			else
				dcPrint.AbortDoc();
		}

		AfxGetMainWnd()->EnableWindow();    // enable main window

		OnEndPrinting(&dcPrint, &printInfo);    // clean up after printing

		if (printT == PT_NORMAL)
		{
			dcPrint.Detach();   // will be cleaned up by CPrintInfo destructor
		}
	}
}

//----------------------------------------------------------------------

//da ApmGr\PageInfo.cpp
#define DMPAPER_SPECIAL		0
//----

//------------------------------------------------------------------------------
BOOL CWoormView::OnPDFPrint (LPCTSTR pszFileName)
{
	int nLast = GetDocument()->m_pRDEmanager->IsClosed() 
		? 1 
		: max (GetDocument()->m_pRDEmanager->LastPage() + 1, 1); //se non ci sono rules, lastpage vale -1
																//perché non viene prodotto rde
	return OnPDFPrint(1, nLast, 1, pszFileName);
}

//------------------------------------------------------------------------------
BOOL CWoormView::OnPDFPrint (UINT nStartPage, UINT nEndPage, UINT nCopies, LPCTSTR pszFileName)
{
	USES_DIAGNOSTIC();
	try
	{
		ASSERT(pszFileName);
		MailConnectorParams* params = AfxGetIMailConnector()->GetParams();
		
		// an. #7302: se il file esiste già, provo a rimuoverlo.
		// se non riesco a rimuoverlo significa che è in uso, quindi non procedo con la stampa
		if (ExistFile(pszFileName) && !DeleteFile(pszFileName)) 
		{
			m_PrintStatus = FAILED_TO_START_PRINT;
			return FALSE; 
		}
		
		CString pwd;
		if (params->GetCryptFile())
		{
			pwd = params->GetPassword();
			if (pwd.IsEmpty()) 
				pwd = DEFAULT_CRYPT_PDF_PASSWORD;
		}
		
		AfxGetTBThread()->IncSuspendIdle();

		OnPDFPrint(nStartPage, nEndPage, nCopies, pszFileName, pwd, params->GetPrinterTemplate());

		AfxGetTBThread()->DecSuspendIdle();

		return TRUE;
	}
	catch (CException* e)
	{
		AfxGetDiagnostic()->Add(e);
		e->Delete();
		return FALSE;
	}
}

//------------------------------------------------------------------------------
void CWoormView::OnPDFPrint(UINT nStartPage, UINT nEndPage, UINT nCopies, LPCTSTR pszFileName, LPCTSTR pszPassword, CString sPrinterTemplate)
{
	//recupero la dimensione dei blocchi di pagine in cui splittare la creazione del pdf
	UINT nBlockSize = 1000;
	CStringArray arPdfNames;
	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();
	if (params)
		nBlockSize = (UINT)params->GetPdfSplitPages();

	m_bPDFPrinting = TRUE;
	DataBool dbSaveIsPrintingState = GetDocument()->m_bIsPrinting;
	GetDocument()->m_bIsPrinting = TRUE;

	m_nFromPage = nStartPage;
	m_nToPage = nEndPage;
	//----

	PageInfo* pSavePageInfo = NULL;

	int nRotateAngle = 0;
	int deltaX = 0;
	int deltaY = 0;
	int originalMarginX = 0;

	BOOL bPostaLite = GetDocument()->m_Email.m_PostaLiteMsg.m_bUsePostaLite;
	if (bPostaLite)
	{
		PageInfo& pi = GetDocument()->m_PageInfo;
		pSavePageInfo = new PageInfo(pi);

		if ((GetDocument()->m_PageInfo.dmOrientation == DMORIENT_LANDSCAPE) && AfxGetIMailConnector()->RotateLandscape())
			nRotateAngle = -90;

		CRect rectPl_mm;
		VERIFY(AfxGetIMailConnector()->PostaLitePdfMargins(rectPl_mm));

		originalMarginX = (long)LPtoMU((pi.m_rectMargins.left /*+ pi.m_rectMargins.right*/), CM, 10., 3);

		CRect rectPl(
			(long)MUtoLP(rectPl_mm.left, CM, 10., 3),
			(long)MUtoLP(rectPl_mm.top, CM, 10., 3),
			(long)MUtoLP(rectPl_mm.right, CM, 10., 3), 
			(long)MUtoLP(rectPl_mm.bottom, CM, 10., 3)
		);

		if (
			pi.m_bUsePrintableArea					|| 
			pi.m_rectMargins.left	< rectPl.left	||
			pi.m_rectMargins.right	< rectPl.right	||
			pi.m_rectMargins.top	< rectPl.top	||
			pi.m_rectMargins.bottom < rectPl.bottom
			)
		{
			if (pi.m_bUsePrintableArea)
				pi.CalculateMargins();

			pi.m_bUsePrintableArea = FALSE; 

			deltaX = (rectPl_mm.left + rectPl_mm.right) -
					(long)LPtoMU((pi.m_rectMargins.left + pi.m_rectMargins.right), CM, 10., 3);
			deltaY = (rectPl_mm.top + rectPl_mm.bottom) -
					(long)LPtoMU((pi.m_rectMargins.top + pi.m_rectMargins.bottom), CM, 10., 3);

			pi.dmPaperWidth += (deltaX * 10);
			pi.dmPaperLength += (deltaY * 10);

			pi.m_rectMargins.left	= rectPl.left;
			pi.m_rectMargins.right	= rectPl.right;
			pi.m_rectMargins.top	= rectPl.top;
			pi.m_rectMargins.bottom = rectPl.bottom;
		}
	}

	CPdfDocumentWrapper* pdfDocument = NULL;
	
	// Instantiate a CPrintDialog object.
	CTBPrintDialog dlgPrint(FALSE);

	//tento di recuperare un device context dalla stampante di default, se non ci riesco
	//uso il device context dello schermo
	CWinApp* app = AfxGetApp();
	bool bPrinterDC = false;
	CDC dc;
	HGLOBAL hDevMode = NULL;
	HGLOBAL hDevNames = NULL;

	if (!sPrinterTemplate.IsEmpty())
	{
		if (GetPrinterDevice(sPrinterTemplate, &hDevNames, &hDevMode))
		{
			dlgPrint.m_pd.hDevMode = hDevMode;
			dlgPrint.m_pd.hDevNames = hDevNames;

			bPrinterDC = true;
		}
	}
	if (!bPrinterDC)
	{
		sPrinterTemplate = GetDocument()->m_PageInfo.GetPreferredPrinter();

		if (!sPrinterTemplate.IsEmpty())
		{
			if (GetPrinterDevice(sPrinterTemplate, &hDevNames, &hDevMode))
			{
				dlgPrint.m_pd.hDevMode = hDevMode;
				dlgPrint.m_pd.hDevNames = hDevNames;

				bPrinterDC = true;
			}
		}
	}

	if (!bPrinterDC && app->GetPrinterDeviceDefaults(&dlgPrint.m_pd))
	{
		bPrinterDC = true;
	}

	if (dlgPrint.CreatePrinterDC() != NULL)
	{
		dc.Attach(dlgPrint.m_pd.hDC);
	}
	else
	{
		dc.Attach(GetDC()->m_hDC);
		bPrinterDC = false;
	}

	GetDocument()->SplitCurrentLayout();
	GetDocument()->SetFirstSplitterPage();

	CPrintInfo printInfo;
		printInfo.SetMinPage(nStartPage);
		printInfo.SetMaxPage(nEndPage);
		printInfo.m_bDirect = TRUE;
		
	OnBeginPrinting(&dc, &printInfo);
	// begin page printing loop
	for (printInfo.m_nCurPage = nStartPage; printInfo.m_nCurPage <= nEndPage; printInfo.m_nCurPage++)
	{
		int currNRotateAngle = nRotateAngle;
		//ogni 10 iterazioni rilascio la coda dei messaggi per non bloccare l'applicazione
		if ((printInfo.m_nCurPage % 10) == 0)
			CTBWinThread::PumpThreadMessages();
		
		//------------------------------------------------------
		//evento di "breaking" sullo split dei blocchi di pagine
		if (((printInfo.m_nCurPage - nStartPage) % nBlockSize) == 0)
		{
			//se sono in un blocco successivo al primo, salvo e chiudo il precedente CPdfDocumentWrapper
			if (printInfo.m_nCurPage > nBlockSize)
			{
				//salvo il blocco precedente
				//CString fileNameNumerated = NumerateFileName(pszFileName, nBlockNumber - 1);
				CString tmpFileName = GenerateUniqueFileName();
				if (tmpFileName.IsEmpty())
				{
					AfxMessageBox(_TB("error saving in pdf"));
					SAFE_DELETE(pdfDocument);
					return;
				}
				pdfDocument->Save(tmpFileName, nCopies);
				SAFE_DELETE(pdfDocument);
				//aggiungo il nome all'array dei nomi dei file per la concatenazione
				arPdfNames.Add(tmpFileName);
			}
			//nuovo pdf in cui scrivo
			ASSERT(pdfDocument == NULL);
			pdfDocument = new CPdfDocumentWrapper();
		}
		ASSERT(pdfDocument != NULL);
		//------------------------------------------------------

		CMetaFileDC memDC;   

		CSize sizeP(GetPrinterPageSizeWithoutMargin_LP());

		CRect metaRect(0, 0, (long)LPtoMU(sizeP.cx, CM, 10., 3), (long)LPtoMU(sizeP.cy, CM, 10., 3));

		//calcolo i margini del documento
		CRect mr = GetDocument()->m_PageInfo.m_rectMargins;
		CRect marginRect(
			(long)LPtoMU(mr.left, CM, 10., 3),
			(long)LPtoMU(mr.top, CM, 10., 3),
			(long)LPtoMU(mr.right, CM, 10., 3),
			(long)LPtoMU(mr.bottom, CM, 10., 3)
			);

		if (!memDC.CreateEnhanced(&dc, NULL, &metaRect, GetDocument()->GetTitle()))
		{
			GetDocument()->m_bIsPrinting = dbSaveIsPrintingState;

			if (pSavePageInfo)
			{
				GetDocument()->m_PageInfo = *pSavePageInfo;
				SAFE_DELETE(pSavePageInfo);
			}
			return;
		}
		
		memDC.m_bPrinting = TRUE;
		memDC.SetAttribDC(dc); 
		memDC.SetAbortProc(SilentPrintAbortProc);

		OnPrepareDC (&memDC, &printInfo);

		// check for end of print
		if (!printInfo.m_bContinuePrinting)
			break;

		CSize csPrtPage(GetPrinterFullPageSize_mm());

		if (currNRotateAngle == -90)
		{
			int t = csPrtPage.cx;
			csPrtPage.cx = csPrtPage.cy;
			csPrtPage.cy = t;
		}

		BOOL currbInvertedOrientation = GetDocument()->GetObjects().m_bInvertOrientation;
		if (currbInvertedOrientation)
			currNRotateAngle = -90;

		OnPrint(&memDC, &printInfo); //si occupa anche di CAMBIARE PAGINA
		//---- ----------
		HENHMETAFILE hMeta = memDC.CloseEnhanced();

		//TRACE(cwsprintf(_T("\nAddPageFromMetafile: %f (%d,%d) %d (%d, %d) [%d,%d,%d,%d]"), 
		//					m_dScaleFactor,
		//					dx, dy,
		//					nRotateAngle,
		//					csPrtPage.cx, csPrtPage.cy,
		//					marginRect.left, marginRect.top, marginRect.right, marginRect.bottom
		//					));

		pdfDocument->AddPageFromMetafile(
							hMeta, 
							marginRect, 
							bPrinterDC, 
							m_dScaleFactor,
							originalMarginX,
							currNRotateAngle,
							csPrtPage.cx, csPrtPage.cy,
							currbInvertedOrientation
						);

		::DeleteEnhMetaFile(hMeta);
	}

	OnEndPrinting(&dc, &printInfo);    // clean up after printing
	//------------------------------

	if (arPdfNames.IsEmpty())
	{
		pdfDocument->Save(pszFileName, nCopies, pszPassword);
		SAFE_DELETE(pdfDocument);
	}
	else
	{
		//salvo l'ultimo blocco
		/*CString fileNameNumerated = NumerateFileName(pszFileName, nBlockNumber - 1);*/
		CString tmpFileName = GenerateUniqueFileName();
		if (tmpFileName.IsEmpty())
		{
			AfxMessageBox(_TB("error saving in pdf"));
			SAFE_DELETE(pdfDocument);
			return;
		}
		pdfDocument->Save(tmpFileName, nCopies);
		SAFE_DELETE(pdfDocument);
		//aggiungo il nome all'array dei nomi dei file per la concatenazione
		arPdfNames.Add(tmpFileName);

		//concateno i file
		ConcatPdf(arPdfNames, pszFileName, pszPassword);
		//deleto i file
		for (int i = 0; i < arPdfNames.GetSize(); i++)
		{
			DeleteFile(arPdfNames[i]);
		}
	}
	//------------------------------------------------

	if (pSavePageInfo)
	{
		GetDocument()->m_PageInfo = *pSavePageInfo;
		SAFE_DELETE(pSavePageInfo);
	}

	GetDocument()->m_bIsPrinting = dbSaveIsPrintingState;
	m_bPDFPrinting = FALSE;
}

//------------------------------------------------------------------------------
HGLOBAL TbGlobalFree(HGLOBAL hMem)
{
	if (hMem != NULL)
		return GlobalFree(hMem);
	return hMem;
}
//------------------------------------------------------------------------------

// determina la capacita' della stampante correntemente collegata
// utilizzando un pageinfo temporaneo per non sporcare i settaggi 
// impostati dal report (dall'utente)
//------------------------------------------------------------------------------
void CWoormView::GetPrinterCapability(CPrintInfo* pInfo, BOOL bFromPrintDialog /*=FALSE*/)
{
	// chiama GetPrinterDeviceDefaults nella eventualita che non sia
	// mai stata chiamata e quindi inizializzati i vari dati relativi alla stampante
	// ignora eventuali stati di errore (assenza di stampante) perche gestiti dal framework
	
	PageInfo aPageInfo;
	
	if (!bFromPrintDialog && GetDocument() != NULL && GetDocument()->m_pOptions && !GetDocument()->m_pOptions->m_strDefaultPrinter.IsEmpty())
	{
		HGLOBAL hDevMode;
		HGLOBAL hDevNames;

		if (GetPrinterDevice(GetDocument()->m_pOptions->m_strDefaultPrinter.GetString(), &hDevNames, &hDevMode))
		{	
			aPageInfo.SetDevMode(hDevMode);
			aPageInfo.SetDevNames(hDevNames);
			TbGlobalFree(hDevMode);
			TbGlobalFree(hDevNames);
		}
		else
		{
			ASSERT(FALSE);
		}
	}
	else if (bFromPrintDialog && pInfo) // se vengo dalla print dialog assegno al pageinfo la stampante effettivamente selezionata
	{
		aPageInfo.SetDevMode(pInfo->m_pPD->m_pd.hDevMode);
		aPageInfo.SetDevNames(pInfo->m_pPD->m_pd.hDevNames);
	}
	
	aPageInfo.GetPrinterCapability();

	m_bSupportCollate = (aPageInfo.dmFields & DM_COLLATE) == DM_COLLATE;
	m_bSupportCopies = (aPageInfo.dmFields & DM_COPIES) == DM_COPIES;
}

//------------------------------------------------------------------------------
BOOL CWoormView::OnPreparePrinting(CPrintInfo* pInfo)
{   
	WoormField* pF = GetDocument()->m_ViewSymbolTable.GetField(SpecialReportField::NAME.PRINT_ON_LETTERHEAD);
	BOOL bShowLetterhead = pF && (pF->GetRefCount() > 0);
	
	CTBPrintDialog* pDialog = new CTBPrintDialog(pInfo->m_pPD, this, FALSE, bShowLetterhead, &(GetDocument()->m_bPrintOnLetterhead));
	pInfo->m_pPD->m_pd.hDC = NULL;
	SAFE_DELETE(pInfo->m_pPD);
	pInfo->m_pPD = pDialog;

	//TODO z-print
	GetDocument()->SplitCurrentLayout();
	GetDocument()->SetFirstSplitterPage();

	// hook document associated with this view
	UINT max_page = GetDocument()->m_pRDEmanager->IsClosed() 
		? 1 
		: GetDocument()->m_pRDEmanager->LastPage() + 1;

	pInfo->SetMaxPage(max_page);
	pInfo->m_nNumPreviewPages = 1;  // preview 1 pages at a time

	// determina cosa sa fare la stampante
	GetPrinterCapability(pInfo);

	// Gestisce il formato della pagina (Landscape/Portrait)
	DWORD dwSetFlags = DM_ORIENTATION | DM_COLLATE | DM_COPIES;
	dwSetFlags |= (GetDocument()->m_PageInfo.dmPaperSize 
		? DM_PAPERSIZE 
		: (DM_PAPERLENGTH | DM_PAPERWIDTH));

	GetDocument()->m_PageInfo.SetPrinterCapability(dwSetFlags);

	// se il driver non supporta alcune funzioni allora deve farle il report stesso
	if (!m_bSupportCollate || !m_bSupportCopies)
		pInfo->m_pPD->m_pd.Flags &= ~PD_USEDEVMODECOPIESANDCOLLATE;

	// non ha senso per noi stampare su file perche` siamo in forma grafica: NON è vero impediamo l'uso di driver particolari tipo PDF
	//pInfo->m_pPD->m_pd.Flags |= PD_HIDEPRINTTOFILE;

	// comportamento standard se non ho il woorminfo e non devo skippare
	// la richiesta della dialog di stampa. Su multipla copia simulata
	// (stampante non capace) la dialog deve apparire solo una volta
	BOOL bDialog = 
			(
				pInfo->m_bPreview || 
				GetDocument()->m_pWoormInfo == NULL ||
				!GetDocument()->m_pWoormInfo->m_bNoPrintDialog
			) 
			&& m_nCurrentCopy == 1;

	if (AfxIsRemoteInterface())
		bDialog = TRUE;

	BOOL bOk = DoPreparePrinting(pInfo, bDialog);
	if (bOk)
	{
		if (GetDocument()->m_pEngine)
		{
			CString sName;
			if (GetDocument()->m_pWoormInfo)
			{
				sName = GetDocument()->m_pWoormInfo->m_strPrinterName;
				if (pInfo && pInfo->m_pPD && pInfo->m_pPD->m_pd.hDevNames)
				{
					LPDEVNAMES lpDevNames = (LPDEVNAMES)::GlobalLock(pInfo->m_pPD->m_pd.hDevNames);
					ASSERT(lpDevNames != NULL);
					if (lpDevNames)
						GetDocument()->m_pWoormInfo->m_strPrinterName = ((LPCTSTR)lpDevNames + lpDevNames->wDeviceOffset);
					::GlobalUnlock(pInfo->m_pPD->m_pd.hDevNames);
				}
			}

			ASSERT_VALID(GetDocument()->m_pEngine);
			GetDocument()->m_pEngine->OnPreparePrinting();

			if (GetDocument()->m_pWoormInfo)
				GetDocument()->m_pWoormInfo->m_strPrinterName = sName;
		}
	}
	return bOk;
}

// E` necessario stabilire dove prendere o scrivere il numero di copie perche`
// il posto cambia a seconda che il driver supporti nativo la stampa di piu` copie.
// Il discorso vale anche per la Collate ma per ora non e` gestita
//------------------------------------------------------------------------------
int GetCopies(CPrintInfo* pInfo, BOOL bSupportCopies)
{                
	int nCopies = pInfo->m_pPD->m_pd.nCopies;

	// se supporta la copia il driver il numeroo di copie e` in DEVMODE
	// vedi help di PRINTDLG
	if (bSupportCopies && pInfo->m_pPD->m_pd.hDevMode)
	{
		LPDEVMODE lpDevMode = (LPDEVMODE)::GlobalLock(pInfo->m_pPD->m_pd.hDevMode);
		ASSERT(lpDevMode);

		if (lpDevMode)
			nCopies = lpDevMode->dmCopies;

		::GlobalUnlock(pInfo->m_pPD->m_pd.hDevMode);
	}

	return nCopies;
}

//------------------------------------------------------------------------------
void SetCopies(int nCopies, CPrintInfo* pInfo, BOOL bSupportCopies)
{ 
	//An.18521
	//Da mago Web non ha senso stampare piu di uan copia, in quanto viene scaricato un file .pdf.
	//Se un report ha associato un numero di copie > 1, viene generato un file unico e eventualmente l'utente lo stampera il numero 
	//di volte desiderato dal suo pdf reader.
	if (AfxIsRemoteInterface())
		nCopies = 1;

	pInfo->m_pPD->m_pd.nCopies = nCopies;

	// vedi quanto detto sopra
	if (bSupportCopies && pInfo->m_pPD->m_pd.hDevMode)
	{
		LPDEVMODE lpDevMode = (LPDEVMODE)::GlobalLock(pInfo->m_pPD->m_pd.hDevMode);
		ASSERT(lpDevMode);

		if (lpDevMode)
			lpDevMode->dmCopies = nCopies;

		::GlobalUnlock(pInfo->m_pPD->m_pd.hDevMode);
	}
}

// in preview comunque non fa vedere la dialog di print sulla stampante
//------------------------------------------------------------------------------
BOOL CWoormView::DoPreparePrinting(CPrintInfo* pInfo, BOOL bPrintDialog/* = TRUE*/)
{                
	// se esiste una stampante preferenziale e non sono in preview allora la setto
	// anche come default permettendo alla print dialog di mettere la combo selezionata
	// in automatico su essa
	if (!pInfo->m_bPreview)
	{ 
		HGLOBAL hDevMode;
		HGLOBAL hDevNames;
		if 
			(
				GetDocument()->m_pWoormInfo &&
				GetDocument()->m_pWoormInfo->m_bNoPrintDialog &&
				!GetDocument()->m_pWoormInfo->m_strPrinterName.IsEmpty() &&
				GetDocument()->m_pWoormInfo->GetDevMode() == NULL && 
				GetDocument()->m_pWoormInfo->GetDevNames() == NULL
			)
		{
			// set to non-default printer without changing default app printer
			if (GetPrinterDevice(GetDocument()->m_pWoormInfo->m_strPrinterName, &hDevNames, &hDevMode))
			{
				TbGlobalFree(pInfo->m_pPD->m_pd.hDevMode);
				pInfo->m_pPD->m_pd.hDevMode = hDevMode;
				TbGlobalFree(pInfo->m_pPD->m_pd.hDevNames);
				pInfo->m_pPD->m_pd.hDevNames = hDevNames;
			}
			else
			{
				ASSERT(FALSE);
			}
		}
		else if 
			(
				!GetDocument()->m_pOptions->m_strDefaultPrinter.IsEmpty()&&
				GetDocument()->m_pWoormInfo->GetDevMode() == NULL && 
				GetDocument()->m_pWoormInfo->GetDevNames() == NULL
			)
		{
			// set to non-default printer without changing default app printer
			if (GetPrinterDevice(GetDocument()->m_pOptions->m_strDefaultPrinter.Str(), &hDevNames, &hDevMode))
			{
			
				TbGlobalFree(pInfo->m_pPD->m_pd.hDevMode);
				pInfo->m_pPD->m_pd.hDevMode = hDevMode;
				TbGlobalFree(pInfo->m_pPD->m_pd.hDevNames);
				pInfo->m_pPD->m_pd.hDevNames = hDevNames;
			}
			else
			{
				ASSERT(FALSE);
			}
		}
		else if (
					GetDocument()->m_pWoormInfo != NULL && 
					GetDocument()->m_pWoormInfo->GetDevMode() != NULL && 
					GetDocument()->m_pWoormInfo->GetDevNames() != NULL
				)
		{
			TbGlobalFree(pInfo->m_pPD->m_pd.hDevMode);
			pInfo->m_pPD->m_pd.hDevMode = CopyHandle(GetDocument()->m_pWoormInfo->GetDevMode());
			TbGlobalFree(pInfo->m_pPD->m_pd.hDevNames);
			pInfo->m_pPD->m_pd.hDevNames = CopyHandle(GetDocument()->m_pWoormInfo->GetDevNames());
		}

		if (pInfo->m_pPD->m_pd.hDevMode == NULL && pInfo->m_pPD->m_pd.hDevNames == NULL) //imposto stampante di applicazione
		{
			TbGlobalFree(pInfo->m_pPD->m_pd.hDevMode);
			TbGlobalFree(pInfo->m_pPD->m_pd.hDevNames);
			AfxGetLoginContext()->GetTbDevParams(pInfo->m_pPD->m_pd.hDevMode, pInfo->m_pPD->m_pd.hDevNames);
		}
		
		GetPrinterCapability(NULL);

		DWORD dwSetFlags = DM_ORIENTATION | DM_COLLATE | DM_COPIES;
			dwSetFlags |= (GetDocument()->m_PageInfo.dmPaperSize 
			? DM_PAPERSIZE 
			: (DM_PAPERLENGTH | DM_PAPERWIDTH));
	
		GetDocument()->m_PageInfo.SetPrinterCapability(dwSetFlags, pInfo->m_pPD->m_pd.hDevMode);
		
	//	GetDocument()->m_PageInfo.SetDevMode(pInfo->m_pPD->m_pd.hDevMode);
	//	GetDocument()->m_PageInfo.SetDevNames(pInfo->m_pPD->m_pd.hDevNames);
	}

	BOOL bOk = TRUE;
	if (bPrintDialog)
	{
		if (GetDocument()->m_pWoormInfo)
			SetCopies(GetDocument()->m_pWoormInfo->m_nCopies, pInfo, m_bSupportCopies);

	//	bOk = CBCGPScrollView::DoPreparePrinting(pInfo); //remove this row beacause PrintDialog in MFC changes application printer
		bOk = MFCCloneDoPreparePrinting(pInfo);

		//handles to printer - used to mantain a scope(to print on the selected printer in the first report) during batch procedure which print n report in cascade
		if (GetDocument()->m_pWoormInfo)
		{
			GetDocument()->m_pWoormInfo->SetDevMode(pInfo->m_pPD->m_pd.hDevMode);
			GetDocument()->m_pWoormInfo->SetDevNames(pInfo->m_pPD->m_pd.hDevNames);
		}
		// ridetermina il supporto delle multiple copie e del collate
		// perche potrebbe essere cambiata la stampante con la combo specifica
		// presente nella print dialog di default 
		
		GetPrinterCapability(pInfo, bPrintDialog);

		// mi memorizzo le copie scelte dall'utente nella dialog per riutilizzarlo
		// successivamente se non presento la dialog (ad esempio stampe in differita)
		// che reistanziano lo stesso report ma devono presentare la dialog solo per il primo
		if (GetDocument()->m_pWoormInfo)
		{
			GetDocument()->m_pWoormInfo->m_nCopies = GetCopies(pInfo, m_bSupportCopies);
			GetDocument()->m_pWoormInfo->m_bPrintAborted = !bOk;
			if (GetDocument()->m_pOldWoormInfo)
				GetDocument()->m_pOldWoormInfo->m_bPrintAborted = !bOk;
			if (GetDocument()->m_pWoormInfo->GetMassiveOperation())
				GetDocument()->m_pWoormInfo->m_bPrintOnLetterHead = GetDocument()->m_bPrintOnLetterhead;
		}
		else
			SetCopies(pInfo->m_pPD->GetCopies(), pInfo, m_bSupportCopies);

		// si mette da parte alcune informazioni che provengono dalle scelte
		// fatte dall'utente nella dialog di richiesta
		m_nFromPage = pInfo->GetFromPage();
		m_nToPage = pInfo->GetToPage();
	}
	else
	{
		// in stampe batch (tipo fatturazione differita) non posso che stampare
		// tutte le pagine (solo la prima stampa da pagina a pagina) e recuperare
		// anche il numero di copie indicate la prima volta che e` comparsa la
		// print dialog all'utente.
		if (GetDocument()->m_pWoormInfo)
		{
			if (!GetDocument()->m_pRDEmanager->IsClosed())
				m_nToPage = GetDocument()->m_pRDEmanager->LastPage() + 1;
	 
			SetCopies(GetDocument()->m_pWoormInfo->m_nCopies, pInfo, m_bSupportCopies);
			if (GetDocument()->m_pWoormInfo->GetMassiveOperation())
				GetDocument()->m_bPrintOnLetterhead = GetDocument()->m_pWoormInfo->m_bPrintOnLetterHead;
		}

		if (pInfo->m_pPD->m_pd.hDC != NULL)
		{	//delete old DC
			CDC::FromHandle(pInfo->m_pPD->m_pd.hDC)->DeleteDC();
		}
		pInfo->m_pPD->CreatePrinterDC();

		// le successive stampe (dopo la prima) non chiedono dati ma ereditano
		// quelli precedentemente inseriti dall'utente alla prima pagina
		pInfo->SetMinPage(m_nFromPage);
		pInfo->SetMaxPage(m_nToPage);

		// evita la richiesta dei parametri di stampa tarmite print dialog
		pInfo->m_bDirect = TRUE;
		bOk = CBCGPScrollView::DoPreparePrinting(pInfo);

		// se era stato abortito qualche report della catena me lo ricordo ed inibisco i successivi
		if (bOk && GetDocument()->m_pWoormInfo && GetDocument()->m_pWoormInfo->m_bPrintAborted)
			bOk = FALSE;
	}

	// segnala al documento che si sta stampantdo sulla stampante considerata di
	// default dall'utente nell'apposita dialog (opzioni di report)
	GetDocument()->m_bPrintingOnDefault=
		GetDocument()->m_pOptions->m_strDefaultPrinter.Str().
			CompareNoCase(pInfo->m_pPD->GetDeviceName()) == 0;

	// in caso di abort da parte dell'utente ripristino la
	// stampante di default (solo in caso di stampa vera e propria)
	if (!bOk)
	{		
		GetDocument()->RestoreCurrentLayout();
	}
	return bOk;
}

//reimplemented to avoid call of MFC PrintDialog (which changes application printer)
//---------------------------------------------------------------------------------
BOOL CWoormView::MFCCloneDoPreparePrinting(CPrintInfo* pInfo, BOOL bPrintDialog/* = TRUE*/)
{
	ASSERT(pInfo != NULL);
	ASSERT(pInfo->m_pPD != NULL);

	if (pInfo->m_pPD->m_pd.nMinPage > pInfo->m_pPD->m_pd.nMaxPage)
		pInfo->m_pPD->m_pd.nMaxPage = pInfo->m_pPD->m_pd.nMinPage;

	// don't prompt the user if we're doing print preview, printing directly,
	// or printing via IPrint and have been instructed not to ask

	if (pInfo->m_bPreview || pInfo->m_bDirect ||
		(pInfo->m_bDocObject && !(pInfo->m_dwFlags & PRINTFLAG_PROMPTUSER)))
	{
		if (pInfo->m_pPD->m_pd.hDC == NULL)
		{
			if (GetDocument() != NULL && GetDocument()->m_pOptions && !GetDocument()->m_pOptions->m_strDefaultPrinter.IsEmpty())
			{
				HGLOBAL hDevMode;
				HGLOBAL hDevNames;

				if (GetPrinterDevice(GetDocument()->m_pOptions->m_strDefaultPrinter.GetString(), &hDevNames, &hDevMode))
				{
					pInfo->m_pPD->m_pd.hDevMode  = hDevMode;
					pInfo->m_pPD->m_pd.hDevNames = hDevNames;
				}
				else
				{
					ASSERT(FALSE);
				}
			}
			else
			{
				// if no printer set then, get default printer DC and create DC without calling
				//   print dialog.
				TbGlobalFree(pInfo->m_pPD->m_pd.hDevMode);
				TbGlobalFree(pInfo->m_pPD->m_pd.hDevNames);
				AfxGetLoginContext()->GetTbDevParams(pInfo->m_pPD->m_pd.hDevMode, pInfo->m_pPD->m_pd.hDevNames);
			}
			if (pInfo->m_pPD->m_pd.hDC == NULL)
			{
				//an. 16344 in preview prima di creare i lDeviceContext setto l'orientamento e le dimensioni
				//lette dal printerpageinfo
				if (pInfo->m_bPreview)
				{
					DWORD dwSetFlags = DM_ORIENTATION | DM_COLLATE | DM_COPIES;
					dwSetFlags |= (GetDocument()->m_PageInfo.dmPaperSize 
						? DM_PAPERSIZE 
						: (DM_PAPERLENGTH | DM_PAPERWIDTH));

					GetDocument()->m_PageInfo.SetPrinterCapability(dwSetFlags, pInfo->m_pPD->m_pd.hDevMode);
				}
				// call CreatePrinterDC if DC was not created by above
				if (pInfo->m_pPD->CreatePrinterDC() == NULL)
					return FALSE;
			}
		}
		// set up From and To page range from Min and Max
		pInfo->m_pPD->m_pd.nFromPage = (WORD)pInfo->GetMinPage();
		pInfo->m_pPD->m_pd.nToPage = (WORD)pInfo->GetMaxPage();
	}
	else
	{
		// otherwise, bring up the print dialog and allow user to change things
		// preset From-To range same as Min-Max range
		pInfo->m_pPD->m_pd.nFromPage = (WORD)pInfo->GetMinPage();
		pInfo->m_pPD->m_pd.nToPage = (WORD)pInfo->GetMaxPage();

		//Bug. 16981 
		//Caused by a change in the way Windows Vista/7 handles copies and collation values 
		//returned by the native print dialog. We relied on DEVMODE to have the correct value 
		//and it used to work in XP and previous Windows even though we did not set the 
		//flag PD_USEDEVMODECOPIESANDCOLLATE but now it stopped working in Vista/7.
		pInfo->m_pPD->m_pd.Flags = pInfo->m_pPD->m_pd.Flags | PD_USEDEVMODECOPIESANDCOLLATE;
	
		if (pInfo->m_pPD->DoModal() != IDOK)   // (AfxGetApp()->DoPrintDialog(pInfo->m_pPD) != IDOK)
		{
			return FALSE;       // do not print
		}

		if (((CTBPrintDialog*)pInfo->m_pPD)->m_bMaintainPrinter)
		{
			AfxGetLoginContext()->SetTbDevParams(pInfo->m_pPD->m_pd.hDevMode, pInfo->m_pPD->m_pd.hDevNames);
		}

		GetDocument()->m_PageInfo.SetDevMode(pInfo->m_pPD->m_pd.hDevMode);
		GetDocument()->m_PageInfo.SetDevNames(pInfo->m_pPD->m_pd.hDevNames);
	}
	ASSERT(pInfo->m_pPD != NULL);
	ASSERT(pInfo->m_pPD->m_pd.hDC != NULL);
	if (pInfo->m_pPD->m_pd.hDC == NULL)
		return FALSE;

	pInfo->m_nNumPreviewPages = AfxGetApp()->m_nNumPreviewPages;
	VERIFY(pInfo->m_strPageDesc.LoadString(AFX_IDS_PREVIEWPAGEDESC));

	return TRUE;
}

//------------------------------------------------------------------------------
void CWoormView::OnBeginPrinting(CDC* /*pDC*/, CPrintInfo* pInfo)
{                
	if (GetDocument()->m_pRDEmanager)
		pInfo->m_nCurPage = GetDocument()->m_pRDEmanager->CurrPageRead() + 1;

	SymField* pF = GetDocument()->m_ViewSymbolTable.GetFieldByID(SpecialReportField::ID.IS_PRINTING);
	if (pF)
		pF->AssignData(DataBool(TRUE));

	int pageIndex = m_nFromPage - 1;

	if (!pInfo->m_bPreview && pageIndex <= GetDocument()->m_pRDEmanager->LastPage())
		GetDocument()->ReadSelectedPage (pageIndex);

	GetDocument()->CalcSplittedPageMaxWidth();

	m_bDoScale = FALSE;
	m_scaleType	= NONE;
	m_dScaleFactor = m_dLowerScale = 1.0;
	m_bCalculatedScale = FALSE;
	m_ptOffsetOrgAfterScaling = CPoint(0,0);

	if (GetDocument()->m_pEngine)
	{
		ASSERT_VALID(GetDocument()->m_pEngine);

		CString sName;
		if (GetDocument()->m_pWoormInfo)
		{
			sName = GetDocument()->m_pWoormInfo->m_strPrinterName;
			if (pInfo && pInfo->m_pPD && pInfo->m_pPD->m_pd.hDevNames)
			{
				LPDEVNAMES lpDevNames = (LPDEVNAMES)::GlobalLock(pInfo->m_pPD->m_pd.hDevNames);
				ASSERT(lpDevNames != NULL);
				if (lpDevNames)
					GetDocument()->m_pWoormInfo->m_strPrinterName = ((LPCTSTR)lpDevNames + lpDevNames->wDeviceOffset);
				::GlobalUnlock(pInfo->m_pPD->m_pd.hDevNames);
			}
		}

		ASSERT_VALID(GetDocument()->m_pEngine);
		GetDocument()->m_pEngine->OnBeginPrinting(pInfo->m_bPreview);

		if (GetDocument()->m_pWoormInfo)
			GetDocument()->m_pWoormInfo->m_strPrinterName = sName;
	}
}

//------------------------------------------------------------------------------
void CWoormView::OnEndPrinting(CDC* pDCPrint, CPrintInfo* pInfo)
{   
	SymField* pF = GetDocument()->m_ViewSymbolTable.GetFieldByID(SpecialReportField::ID.IS_PRINTING);
	if (pF)
		pF->AssignData(DataBool(FALSE));

	if (GetDocument()->GetNumberOfSplittedPage())
		GetDocument()->RestoreCurrentLayout();
		
	if	(GetDocument()->IsRunningFromExternalController())
	{
		switch (m_PrintStatus)
		{
			case NO_PRINT_ERROR:
				GetDocument()->SetRunningTaskStatus(CExternalControllerInfo::TASK_REPORT_PRINT_ENDED);
				break;
			case PRINT_ABORT:
				GetDocument()->SetRunningTaskStatus(CExternalControllerInfo::TASK_USER_ABORT);
				break;
			case FAILED_TO_START_PRINT:
				GetDocument()->SetRunningTaskStatus(CExternalControllerInfo::TASK_FAILED);
					//(void*)(LPCTSTR)pInfo->m_pPD->GetDeviceName() PERASSO: prima veniva aggiunta anche questa informazione
				break;
			default:
				ASSERT(FALSE);
				break;
		}
	}
	
	if (GetDocument()->m_pWoormInfo)
	{
		if (m_PrintStatus == NO_PRINT_ERROR)
		{
			GetDocument()->m_pWoormInfo->m_bPrinted = !pInfo->m_bPreview && !GetDocument()->m_pWoormInfo->m_bPrintAborted;
			if (GetDocument()->m_pOldWoormInfo)
				GetDocument()->m_pOldWoormInfo->m_bPrinted = GetDocument()->m_pWoormInfo->m_bPrinted;
		}
		else
		{
			GetDocument()->m_pWoormInfo->m_bPrinted = FALSE;
			if (GetDocument()->m_pOldWoormInfo)
				GetDocument()->m_pOldWoormInfo->m_bPrinted = FALSE;
		}
	}

	if (GetDocument()->m_pEngine)
	{
		ASSERT_VALID(GetDocument()->m_pEngine);
		GetDocument()->m_pEngine->OnEndPrinting();
	}

	GetDocument()->m_PageInfo.SetDevMode(NULL);
	GetDocument()->m_PageInfo.SetDevNames(NULL);	
}

//------------------------------------------------------------------------------
void CWoormView::OnPrint (CDC* pDC, CPrintInfo* pInfo)
{
	ASSERT_VALID(pDC);

	//todo: verificare. é stata messa per rifargli calcolare il punto d'origine in caso di scaling con layout con diverso orientamento
	if(m_bDoScale)
		OnPrepareDC(pDC, pInfo);

	CPoint ptOffset = GetPrintableOffset(pDC);

	CPoint ptMargins = GetDocument()->m_PageInfo.m_rectMargins.TopLeft();
	ScalePoint(ptMargins, *pDC);

	//Ignora i margini e stampa sempre con 0,0 posizionato al vertice dell'area stampabile
	if (!GetDocument()->m_PageInfo.m_bUsePrintableArea && !m_bDoScale)
		pDC->SetViewportOrg(ptMargins.x - ptOffset.x, ptMargins.y - ptOffset.y);

	//se serve, ruoto la pagina con una trasformazione (SE STO STAMPANDO UN PDF NON POSSO USARE LA SetWorldTransform e ruoto direttamente in fase di creazione del pdf)
	if (GetDocument()->GetObjects().m_bInvertOrientation && m_bPDFPrinting == FALSE)
	{
		//Save DC
		int iSaved = pDC->SaveDC();
		CFont *pOldFont = pDC->GetCurrentFont();

		double pi = 3.14159265358979323846264338328;
		double radian = ((double)2.0 * pi * (double)-90.00) / (double)360.0; //Rotate -90 degrees
		float cosine = (float)cos(radian);
		float sine = (float)sin(radian);

		CSize size(0, 0);
		/*size.cx = pDC->GetDeviceCaps(HORZRES);*/
		size.cy = pDC->GetDeviceCaps(VERTRES);

		XFORM xForm;
		xForm.eM11 = cosine;
		xForm.eM12 = sine;
		xForm.eM21 = -sine;
		xForm.eM22 = cosine;
	
		xForm.eDx = 0;
		xForm.eDy = (float)(size.cy / m_dScaleFactor);

		pDC->SetGraphicsMode(GM_ADVANCED);
		SetWorldTransform(pDC->m_hDC, &xForm);

		LOGFONT lgf;
		pDC->GetCurrentFont()->GetLogFont(&lgf);
		lgf.lfOrientation -= lgf.lfEscapement;
		lgf.lfEscapement = 0;
		CFont horFont;
		horFont.CreateFontIndirect(&lgf);
		pOldFont = pDC->SelectObject(&horFont);

		DoDraw(pDC, pInfo);

		//Restore DC
		pDC->RestoreDC(iSaved);
	}

	else
		DoDraw(pDC, pInfo);

	// disegna i margini della stampante in preview
	if (pInfo && pInfo->m_bPreview)
		DrawMargins(pDC, pInfo);

	// test end of print
	if (pInfo->m_bPreview)
		return;

	//----Preparazione dati per la pagina successiva
	BOOL bNextSplitter = GetDocument()->AllowNextSplitterPage();
	if (bNextSplitter)
		GetDocument()->SetNextSplitterPage();
	else if (GetDocument()->GetNumberOfSplittedPage())
		GetDocument()->SetFirstSplitterPage();

	if 
		(
			pInfo->m_nCurPage > m_nToPage
			||
			(
				pInfo->m_nCurPage == m_nToPage 
				&&
				!bNextSplitter
			)
		)
		pInfo->m_bContinuePrinting = FALSE;

	// in preview mode use next-prev button to move page
	if (/*!pInfo->m_bPreview && */pInfo->m_bContinuePrinting)
	{
		if (bNextSplitter)
		{
			pInfo->m_nCurPage--;
			GetDocument()->ReadSelectedPage(pInfo->m_nCurPage);
		}
		else
			GetDocument()->ReadNextPage();
	}
}

//------------------------------------------------------------------------------
// disegna i margini della stampante in preview
void CWoormView::DrawMargins(CDC* pDC, CPrintInfo* pInfo)
{
	ASSERT(pInfo && pInfo->m_bPreview);

	CPoint ptOffset = GetPrintableOffset(pDC);
	CSize sizePrintable = GetPrintableSize(pDC);
	CRect rect;

	if (m_bDoScale)
	{
		// si mette con origine in 0,0 assolute per disegnare i margini
		pDC->SetViewportOrg(-ptOffset.x, -ptOffset.y);

		double scale = double(m_nScaleDenom) / m_nScaleNum;

		double dOffsetX = ptOffset.x * scale;
		round(dOffsetX, 0);
		ptOffset.x = int(dOffsetX);

		double dOffsetY = ptOffset.y * scale;
		round(dOffsetY, 0);
		ptOffset.y = int(dOffsetY);

		double dSzPrintableX = sizePrintable.cx * scale;
		round(dSzPrintableX, 0);
		sizePrintable.cx = int(dSzPrintableX);

		double dSzPrintableY = sizePrintable.cy * scale;
		round(dSzPrintableY, 0);
		sizePrintable.cy = int(dSzPrintableY);

		rect.top = ptOffset.y;
		rect.left = ptOffset.x;
		rect.right = rect.left + sizePrintable.cx;
		rect.bottom = rect.top + sizePrintable.cy;

		CPen pen(PS_SOLID, 0, RGB(128,128,128));
		CPen* pPenOld = pDC->SelectObject(&pen);

		if (GetDocument()->m_pWoormIni->m_bShowPrintableArea)
		{
			pDC->MoveTo(rect.left,	rect.top);
			pDC->LineTo(rect.right,	rect.top);
			pDC->LineTo(rect.right,	rect.bottom);
			pDC->LineTo(rect.left,	rect.bottom);
			pDC->LineTo(rect.left,	rect.top);
		}

		// disegna il margine settato dall'utente se non si utilizza
		// lo spostamento automatico all'origine della area stampabile
		if (GetDocument()->m_pWoormIni->m_bShowMargins)
		{
			CPen pen2(PS_DOT, 0, RGB(128, 128, 128));
			CRect rectMargins = GetDocument()->m_PageInfo.m_rectMargins;

			CSize szPageSize ( 
					(pDC->IsPrinting() || (pInfo && pInfo->m_bPreview)) ?
						GetPrinterPageSizeWithoutMargin_LP() :
						GetPageSizeWithoutMargin_LP()
				);

			if (GetDocument()->GetObjects().m_bInvertOrientation)
			{
				int cx = szPageSize.cx;
				szPageSize.cx = szPageSize.cy;
				szPageSize.cy = cx;
			}

			ScaleRect(rectMargins, *pDC);
			ScaleSize(szPageSize, *pDC);
			pDC->SelectObject(&pen2);

			double dSzCx = szPageSize.cx * scale;
			round(dSzCx, 0);
			szPageSize.cx = int(dSzCx);

			double dSzCy = szPageSize.cy * scale;
			round(dSzCy, 0);
			szPageSize.cy = int(dSzCy);

			double dRectMarginsTop = rectMargins.top * scale;
			round(dRectMarginsTop, 0);
			rectMargins.top = int(dRectMarginsTop);

			double dRectMarginsLeft = rectMargins.left * scale;
			round(dRectMarginsLeft, 0);
			rectMargins.left = int(dRectMarginsLeft);

			pDC->MoveTo(rectMargins.left,					rect.top);
			pDC->LineTo(rectMargins.left,					rect.bottom);
			pDC->MoveTo(rect.left,							rectMargins.top);
			pDC->LineTo(rect.right,							rectMargins.top);

			pDC->MoveTo(szPageSize.cx + rectMargins.left,	rect.top);
			pDC->LineTo(szPageSize.cx + rectMargins.left,	rect.bottom);
			pDC->MoveTo(rect.left,							szPageSize.cy + rectMargins.top);
			pDC->LineTo(rect.right,							szPageSize.cy + rectMargins.top);
		}

		pDC->SelectObject(pPenOld);
		return;
	}

	// si mette con origine in 0,0 assolute per disegnare i margini
	pDC->SetViewportOrg(-ptOffset.x, -ptOffset.y);

	rect.top = ptOffset.y;
	rect.left = ptOffset.x;
	rect.right = rect.left + sizePrintable.cx;
	rect.bottom = rect.top + sizePrintable.cy;

	CPen pen(PS_SOLID, 0, RGB(128,128,128));
	CPen* pPenOld = pDC->SelectObject(&pen);

	if (GetDocument()->m_pWoormIni->m_bShowPrintableArea)
	{
		pDC->MoveTo(rect.left,	rect.top);
		pDC->LineTo(rect.right,	rect.top);
		pDC->LineTo(rect.right,	rect.bottom);
		pDC->LineTo(rect.left,	rect.bottom);
		pDC->LineTo(rect.left,	rect.top);
	}

	// disegna il margine settato dall'utente se non si utilizza
	// lo spostamento automatico all'origine della area stampabile
	if (GetDocument()->m_pWoormIni->m_bShowMargins)
	{
		CPen pen2(PS_DOT, 0, RGB(128, 128, 128));
		CRect rectMargins = GetDocument()->m_PageInfo.m_rectMargins;

		CSize szPageSize ( 
							(pDC->IsPrinting() || (pInfo && pInfo->m_bPreview)) ?
								GetPrinterPageSizeWithoutMargin_LP() :
								GetPageSizeWithoutMargin_LP()
						);

		/*if (GetDocument()->GetObjects().m_bInvertOrientation)
		{
			int cx = szPageSize.cx;
			szPageSize.cx = szPageSize.cy;
			szPageSize.cy = cx;
		}*/

		ScaleRect(rectMargins, *pDC);
		ScaleSize(szPageSize, *pDC);
		pDC->SelectObject(&pen2);

		pDC->MoveTo(rectMargins.left,					rect.top);
		pDC->LineTo(rectMargins.left,					rect.bottom);
		pDC->MoveTo(rect.left,							rectMargins.top);
		pDC->LineTo(rect.right,							rectMargins.top);

		pDC->MoveTo(szPageSize.cx + rectMargins.left,	rect.top);
		pDC->LineTo(szPageSize.cx + rectMargins.left,	rect.bottom);
		pDC->MoveTo(rect.left,							szPageSize.cy + rectMargins.top);
		pDC->LineTo(rect.right,							szPageSize.cy + rectMargins.top);
	}

	pDC->SelectObject(pPenOld);
}

//------------------------------------------------------------------------------
BOOL CWoormView::Disabled(BOOL bCheckEditMode/* = TRUE*/)
{
	BOOL bOk =
		GetDocument()->m_bPlayback	||
		GetDocument()->m_bEngineRunning;
														  
	if (!bCheckEditMode)
		return bOk;

	return 	bOk || !GetDocument()->m_bAllowEditing;
}

//-----------------------------------------------------------------------------
#define _tbcountof(array) (sizeof(array)/sizeof(array[0]))

BOOL CWoormView::OnToolTipText(UINT id, NMHDR* pNMHDR, LRESULT* pResult)
{
	ASSERT(pNMHDR->code == TTN_NEEDTEXTA || pNMHDR->code == TTN_NEEDTEXTW);

	// check to see if the message is going directly to this window or not
	const MSG* pMsg = GetCurrentMessage();
	if (pMsg->hwnd != m_hWnd)
	{
		// let top level frame handle this for us
		return FALSE;
	}

	// con questa stessa routine gestiamo sia la versione ANSI che UNICODE del messaggio
	TOOLTIPTEXTA* pTTTA = (TOOLTIPTEXTA*)pNMHDR;
	TOOLTIPTEXTW* pTTTW = (TOOLTIPTEXTW*)pNMHDR;

	UINT nID = pNMHDR->idFrom;

	CString strMessage;
	// Gestisce dinamicamente solo i tooltips di toolbar, in ogni altro caso rimanda alla gestione
	// standard di MFC
	if	(
			(	// non è un control, ma un bottone di toolbar
				pNMHDR->code == TTN_NEEDTEXTA && !(pTTTA->uFlags & TTF_IDISHWND) ||
				pNMHDR->code == TTN_NEEDTEXTW && !(pTTTW->uFlags & TTF_IDISHWND)
			) 
		)
	{
		//(nID != 0) && // nID è zero su un separatore
		BOOL bOk = !(GetDocument()->m_bEngineRunning || m_pProcessingMouse->IsLocked() || GetDocument()->m_bAllowEditing || !GetDocument()->IsDataLoaded());
		
		if (bOk)
		{
			// normalize mouse position to reflect new window origin
			CClientDC dc(this);
			OnPrepareDC(&dc);
			CPoint pt;
			::GetCursorPos(&pt);
			dc.DPtoLP(&pt);

			//int nAlias;
			//int nRow;
			//Table* pTable;
			//FieldRect* pField;

			BOOL bOk = GetDocument()->OnGetToolTipText(&pt, strMessage);

			if (bOk)	
			{
			// Riproduce il comportamento standard di MFC, per il tooltip 
			// usa la parte di messaggio dopo il primo \n
	#ifndef _UNICODE
				if (pNMHDR->code == TTN_NEEDTEXTA)
					lstrcpyn(pTTTA->szText, strMessage, _tbcountof(pTTTA->szText));
				else
					_mbstowcsz(pTTTW->szText, strMessage, _tbcountof(pTTTW->szText));
	#else
				if (pNMHDR->code == TTN_NEEDTEXTA)
					_wcstombsz(pTTTA->szText, strMessage, _tbcountof(pTTTA->szText));
				else
					lstrcpyn(pTTTW->szText, strMessage, _tbcountof(pTTTW->szText));
	#endif
				// ho gestito dinamicamente il tooltip
				return TRUE;
			}
		}
	}
	return CCommonFunctions::OnToolTipText(id, pNMHDR, pResult, GetDllInstance(this));    // message was handled
}

//-----------------------------------------------------------------------------
void CWoormView::OnNcDestroy()
{
	CBCGPScrollView::OnNcDestroy();
}

//-----------------------------------------------------------------------------
BOOL CWoormView::OnObjectsSelChanged(CObject* pObj)
{
	if 
		(
			!pObj ||
			GetDocument()->m_bPlayback ||
			GetDocument()->m_bEngineRunning ||
			m_pProcessingMouse->IsLocked() ||
			!GetDocument()->m_bAllowEditing
		)
		return FALSE;

	ASSERT_VALID(pObj);

	// remove multiple selection
	if (GetDocument()->m_pMultipleSelObj)
		GetDocument()->OnDeselectAll();   

	// remove multiple selection
	if (GetDocument()->m_pMultiColumns)
		GetDocument()->m_pMultiColumns->ClearMultipleSelObjects();

	CPoint pt;
	if (pObj->IsKindOf(RUNTIME_CLASS(BaseObj)))
		GetDocument()->SetCurrentObj((BaseObj*)pObj, pt);
	else if (pObj->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pCol = (TableColumn*)pObj;

		if (!pCol->m_bIsHidden)
			GetDocument()->SetCurrentObj(pCol->GetInternalID(), pt);
		else
		{
			Table* pTable = pCol->GetTable();
			pTable->m_nActiveColumn = pTable->NO_ACTIVE_COLUMN;

			//deselezione vecchio current obj (tabella)------
			GetDocument()->m_pCurrentObj = NULL;
			if (GetDocument()->m_pActiveRect)
				GetDocument()->m_pActiveRect->Clear();
			//-----------------------------------------------
		}
			
	}
	else if (pObj->IsKindOf(RUNTIME_CLASS(TotalCell)))
	{
		TotalCell* pTot = (TotalCell*)pObj;
		Table* pTable = pTot->m_pColumn->GetTable();

		int nCol = pTable->GetIdxColFromAlias(pTot->m_pColumn->GetInternalID());
		if (nCol >= 0)
		{
			pTable->m_nActiveColumn = nCol;
			pTable->m_nActiveRow = Table::ACTIVE_IS_TOTAL;
			GetDocument()->m_pActiveRect->SetActive(pTable->GetActiveRect());
		}
	}
	else
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!GetDocument()->m_pCurrentObj)
	{
		ReleaseCapture();
		m_pProcessingMouse->Unlock();
		return FALSE;
	}
	
	GetDocument()->m_ptCurrPos = pt;
	// allora da il controllo all'oggetto selezionato
	/*CClientDC dc(this);
	OnPrepareDC(&dc);
	GetDocument()->m_pCurrentObj->LeftButtonDown(dc, GetDocument()->m_ptCurrPos, this);
	GetDocument()->m_pCurrentObj->LeftButtonUp(dc, this);*/

	// elimina (se esiste) il rettangolo di draging
	GetDocument()->DeleteDragRect();

	// reenable incoming buttondown
	ReleaseCapture();
	m_pProcessingMouse->Unlock();

	if (GetDocument()->m_pWoormIni->m_bSnapToGrid)
		GetDocument()->DoSnapToGrid();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CWoormView::OnObjectCut()
{
	CWoormDocMng* pDocument = GetDocument();
	ASSERT(pDocument);

	pDocument->OnObjectCut();
}

//-----------------------------------------------------------------------------
void CWoormView::OnVKTab()
{
	CWoormDocMng* pDocument = GetDocument();
	ASSERT(pDocument);

	pDocument->OnVKTab();
}

//-----------------------------------------------------------------------------
void CWoormView::OnVKDown()
{
	CWoormDocMng* pDocument = GetDocument();
	ASSERT(pDocument);

	pDocument->OnVKDown();
}

//-----------------------------------------------------------------------------
void CWoormView::OnVKUp()
{
	CWoormDocMng* pDocument = GetDocument();
	ASSERT(pDocument);

	pDocument->OnVKUp();
}

//-----------------------------------------------------------------------------
void CWoormView::OnVKLeft()
{
	CWoormDocMng* pDocument = GetDocument();
	ASSERT(pDocument);

	pDocument->OnVKLeft();
}

//-----------------------------------------------------------------------------
void CWoormView::OnVKRight()
{
	CWoormDocMng* pDocument = GetDocument();
	ASSERT(pDocument);

	pDocument->OnVKRight();
}

//-----------------------------------------------------------------------------
void CWoormView::OnUpdateVKMove(CCmdUI* pCmdUI)
{
	CWoormDocMng* pDocument = GetDocument();
	ASSERT(pDocument);

	if (pDocument->m_pDataDefaults->m_bUseAsRadar && !pDocument->m_bAllowEditing)
	{
		pCmdUI->Enable(TRUE);
		return;
	}

	pDocument->OnUpdateVKMove(pCmdUI);
}

//-----------------------------------------------------------------------------
void CWoormView::OnUpdateCut(CCmdUI* pCmdUI)
{
	CWoormDocMng* pDocument = GetDocument();
	ASSERT(pDocument);

	pDocument->OnUpdateCut(pCmdUI);
}

///////////////////////////////////////////////////////////////////////////////
BOOL CWoormView::UpdateNodeSelection(CRSTreeCtrl* sourceTree)
{
	if (!GetDocument()->m_pNodesSelection || GetDocument()->m_pNodesSelection->GetCount() == 0)
	{
		ASSERT_VALID(sourceTree);
		HTREEITEM ht = sourceTree->GetDragItem();
		ASSERT(ht);
		if (ht == NULL)
			return FALSE;
		CNodeTree* pNode = sourceTree->GetNode(ht);
		ASSERT_VALID(pNode);
		if (pNode == NULL)
			return FALSE;

		GetDocument()->m_pNodesSelection = new CNodeTreeArray;
		GetDocument()->m_pNodesSelection->Add(pNode);
	}
	return GetDocument()->m_pNodesSelection->GetSize() > 0;
}

//-----------------------------------------------------------------------------
Table* CWoormView::GetTableAtDropPos(CClientDC& dc)
{
	// normalize mouse position to reflect new window origin
	CPoint ptCurrPos = m_PointDrop;

	return GetTableAtPoint(ptCurrPos, TRUE);
}

//-----------------------------------------------------------------------------
Table* CWoormView::GetTableAtPoint(CPoint pt, BOOL bSetActive)
{
	Table* pTable = NULL;
	for (int i = 0; i <= this->GetDocument()->GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = this->GetDocument()->GetObjects()[i];
		if ((pObj->IsKindOf(RUNTIME_CLASS(Table))) && (pObj->InMe(pt))) //sono su tabella
		{
			//controllo se sono su titolo, allora abilito selezione con click semplice
			int nRow = -1; int nAlias = 0;
			pTable = (Table*)pObj;
			pTable->GetPosition(pt, nRow, &nAlias);
			if (nAlias && bSetActive)
			{
				CPoint point;
				GetDocument()->SetCurrentObj(nAlias, point);
			}
			break;
		}
	}
	return pTable;
}

//-----------------------------------------------------------------------------
BaseObj* CWoormView::GetTargetDropObject(CClientDC& dc)
{
	// normalize mouse position to reflect new window origin
	CPoint ptCurrPos = m_PointDrop;
	dc.DPtoLP(&ptCurrPos);

	for (int i = 0; i <= this->GetDocument()->GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = this->GetDocument()->GetObjects()[i];
		if (pObj->InMe(ptCurrPos)) //sono su tabella
			return pObj;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL CWoormView::AddDBTableFromDrop()
{
	ASSERT_VALID(GetDocument()->m_pNodesSelection);
	ASSERT(GetDocument()->m_pNodesSelection->GetSize() == 1);
	if (!GetDocument()->m_pNodesSelection || GetDocument()->m_pNodesSelection->GetSize() == 0)
	{
		SAFE_DELETE(GetDocument()->m_pNodesSelection);
		return FALSE;
	}
	//----

	CNodeTree* pNode = (*GetDocument()->m_pNodesSelection)[0];
	ASSERT(pNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_DBTABLE || pNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_DBVIEW);

	SqlCatalogEntry* pCatEntry = dynamic_cast<SqlCatalogEntry*>(pNode->m_pItemData);
	ASSERT_VALID(pCatEntry);
	if (!pCatEntry)
	{
		SAFE_DELETE(GetDocument()->m_pNodesSelection);
		return FALSE;
	}

	TblRuleData* pTblRule = new TblRuleData(*GetDocument()->GetEditorSymTable(), AfxGetDefaultSqlConnection(), pCatEntry->m_strTableName);

	RuleDataArray* pRuleData = GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData();
	pRuleData->Add(pTblRule);

	//----
	ASSERT_VALID(GetWoormFrame()->m_pToolBoxDBView);

	//----

	ASSERT_VALID(GetWoormFrame()->m_pObjectPropertyView);

	CClientDC dc(this);
	OnPrepareDC(&dc);

	GetWoormFrame()->GetDocument()->m_pCurrentObj = GetTargetDropObject(dc);

	GetWoormFrame()->m_pObjectPropertyView->NewDBElement(GetWoormFrame()->m_pToolBoxDBView->m_bAddTable, FALSE, pTblRule);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWoormView::AddDBColumnsFromDrop(BOOL bFromTree)
{
	ASSERT_VALID(GetDocument()->m_pNodesSelection);
	ASSERT(GetDocument()->m_pNodesSelection->GetSize());
	if (!GetDocument()->m_pNodesSelection || GetDocument()->m_pNodesSelection->GetSize() == 0)
	{
		SAFE_DELETE(GetDocument()->m_pNodesSelection);
		return FALSE;
	}
	//--------------
	CNodeTree* pNode = (*GetDocument()->m_pNodesSelection)[0];

	TblRuleData* pTblRule = GetDocument()->AddDBColumns_GetOrCreateTblRule(pNode);
	if (!pTblRule) 
	{
		SAFE_DELETE(GetDocument()->m_pNodesSelection);
		return NULL;
	}
	SqlCatalogEntry* pCatEntry = dynamic_cast<SqlCatalogEntry*>(pNode->m_pParentItemData);
	ASSERT_VALID(pCatEntry);
	if (!pCatEntry)
	{
		return NULL;
	}
	int idx = pTblRule->m_arSqlTableJoinInfoArray.Find(pCatEntry->m_strTableName);
	ASSERT(idx >= 0);

	//----
	ASSERT_VALID(GetWoormFrame()->m_pToolBoxDBView);

	BOOL creatingNewTable = GetWoormFrame()->m_pToolBoxDBView->m_bAddTable;
	BOOL creatingNewHiddenVariables = GetWoormFrame()->m_pToolBoxDBView->m_bAddHidden;

	ASSERT((!creatingNewTable && !creatingNewHiddenVariables) || (creatingNewTable != creatingNewHiddenVariables));

	//----
	CNodeTreeArray* pAr = new CNodeTreeArray;
	
	for (int i = 0; i < GetDocument()->m_pNodesSelection->GetSize(); i++)
	{
		pNode = (*GetDocument()->m_pNodesSelection)[i];

		WoormField* pRepField = GetDocument()->AddDBColumns_CreateField(pNode, pTblRule, idx);
		if (!pRepField) 
			continue;

		if (creatingNewHiddenVariables) continue;
		//----
		CNodeTree* pNewNode = new CNodeTree(NULL, CRSTreeCtrlImgIdx::NoGLyph, CNodeTree::ENodeType::NT_VARIABLE, pRepField, pTblRule->GetSymTable());
		pAr->Add(pNewNode); 
	}

	SAFE_DELETE(GetDocument()->m_pNodesSelection);
	//----

	GetDocument()->RefreshRSTree(ERefreshEditor::Rules);
	if (bFromTree)
	{
		HTREEITEM htTableRule = GetDocument()->SelectRSTreeItemData(ERefreshEditor::Rules, pTblRule);
		if (htTableRule)
		{

			CRSTreeCtrl* pEngineTreeCtrl = &(GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl);
			CString nodeLabel;
			HTREEITEM htFrom = pEngineTreeCtrl->GetNextItem(htTableRule, TVGN_CHILD);	
			if (htFrom && htFrom != TVI_ROOT)
				nodeLabel = pEngineTreeCtrl->GetItemText(htFrom);
			pEngineTreeCtrl->Expand(htFrom, TVE_EXPAND);

			HTREEITEM htFirstRule = pEngineTreeCtrl->GetNextItem(htFrom, TVGN_CHILD);
			HTREEITEM RightRule = htFirstRule;
			nodeLabel = pEngineTreeCtrl->GetItemText(RightRule);
			for (int i = 0; i < idx; i++)
			{
				RightRule = pEngineTreeCtrl->GetNextItem(RightRule, TVGN_NEXT);
				nodeLabel = pEngineTreeCtrl->GetItemText(RightRule);
			}
			pEngineTreeCtrl->Expand(RightRule, TVE_EXPAND);

			HTREEITEM ht1 = pEngineTreeCtrl->GetNextItem(RightRule, TVGN_CHILD);
			nodeLabel = pEngineTreeCtrl->GetItemText(ht1);
			if (idx > 0) //there is "On" (of join) node
				ht1 = pEngineTreeCtrl->GetNextItem(ht1, TVGN_NEXT);
			pEngineTreeCtrl->Expand(ht1, TVE_EXPAND);
			HTREEITEM ht2 = pEngineTreeCtrl->GetNextItem(ht1, TVGN_NEXT);
			pEngineTreeCtrl->Expand(ht2, TVE_EXPAND);
		}
	}
		
	if (creatingNewHiddenVariables)
	{
		GetDocument()->RefreshRSTree(ERefreshEditor::Variables);
		SAFE_DELETE(pAr);
		return TRUE;
	}

	return AddHiddenVariableFromDrop(pAr, creatingNewTable, pTblRule);
}

//-----------------------------------------------------------------------------
BOOL CWoormView::AddHiddenVariableFromDrop(CNodeTreeArray*& pAr, BOOL isNewTable, TblRuleData* pTblRule)
{
	if (!pAr || pAr->GetSize() == 0)
	{
		SAFE_DELETE(pAr);
		return FALSE;
	}
	//--------------

	CClientDC dc(this);
	OnPrepareDC(&dc);

	Table* pTable = GetTableAtDropPos(dc);
	BOOL msgForArrayShowed = FALSE;	// makes the msg to be shown only once

	//---------------------
	GetDocument()->m_wCreatingColumnIds.RemoveAll();

	WoormField* pRepField = NULL;
	for (int i = 0; i < pAr->GetSize(); i++)
	{
		CNodeTree* pNode = (*pAr)[i];

		pRepField = dynamic_cast<WoormField*>(pNode->m_pItemData);
		ASSERT_VALID(pRepField);
		if (!pRepField->IsHidden()) 
			continue;

		if (pRepField->IsArray())
		{
			if (!msgForArrayShowed)
			{
				AfxMessageBox(_TB("The field of type \"Array\" cannot be set to visible"));
				msgForArrayShowed = TRUE;
			}
			pRepField = NULL;
			continue;
		}

		if (pRepField->IsArray())
		{
			if (!msgForArrayShowed)
			{
				AfxMessageBox(_TB("The field of type \"Array\" cannot be set to visible"));
				msgForArrayShowed = TRUE;
			}
			pRepField = NULL;
			continue;
		}

		pRepField->SetHidden(FALSE);
		pRepField->SetLen(AfxGetFormatStyleTable()->GetInputCharLen(pRepField->GetDataType(), &GetDocument()->GetNamespace()));

		if (pTable)
		{
			pRepField->SetFieldType(WoormField::RepFieldType::FIELD_COLUMN);
			pRepField->SetDispTable(pTable->GetName(TRUE));
		}
		GetDocument()->m_wCreatingColumnIds.Add(pRepField->GetId());
	}

	GetDocument()->m_ptCurrPos = m_PointDrop;
	GetDocument()->m_pCurrentObj = NULL;
	GetDocument()->m_pActiveRect->Clear();

	if (isNewTable && GetDocument()->m_wCreatingColumnIds.GetCount() != 0) //crea una nuova tabella
	{
		GetDocument()->m_wCreatingId = 0;
		if (GetDocument()->m_pEditorManager->AddTableDataField(GetDocument()->m_wCreatingId, GetDocument()->m_wCreatingColumnIds, TRUE))
			GetDocument()->m_Creating = CWoormDocMng::TABLE;

		GetDocument()->CreateListObj(dc, this, TRUE);		
		GetDocument()->m_Creating = CWoormDocMng::NONE;	
		GetWoormFrame()->m_pObjectPropertyView->ClearPropertyGrid();

		CLayout* currLayout = GetDocument()->m_Objects;

		GetDocument()->RefreshRSTree(ERefreshEditor::Variables);
		GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, currLayout);
	}
	else if (pTable) // inserisce nuove colonne in una tabella esistente 
	{

		pTable->AddColumn(GetDocument()->m_wCreatingColumnIds);

		GetDocument()->SyncronizeViewSymbolTable(GetDocument()->GetEditorSymTable());

		GetDocument()->RefreshRSTree(ERefreshEditor::Variables);
		GetDocument()->RefreshRSTree(ERefreshEditor::Layouts);

		GetWoormFrame()->SelectLayoutObject(/*pTable->GetActiveColumn() ? (CObject*)pTable->GetActiveColumn() : */(CObject*)pTable);
	}
	else // crea nuovi campi singoli
	{
		if (!pRepField)
			return FALSE;

		GetDocument()->m_wCreatingId = pRepField->GetId();
		GetDocument()->m_Creating = CWoormDocMng::ObjectType::FIELDRECT;

		if (GetDocument()->m_pMultipleSelObj)
			GetDocument()->OnDeselectAll();

		GetDocument()->CreateListObj(dc, this, TRUE);
		GetDocument()->m_Creating = CWoormDocMng::NONE;
		m_pProcessingMouse->Unlock();

		GetDocument()->SyncronizeViewSymbolTable(GetDocument()->GetEditorSymTable());
	}

	if (pTable/* || isNewTable*/)
	{
		ASSERT_VALID(pTable);
		if (GetDocument()->m_wCreatingColumnIds.GetSize())
		{
			CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Layouts);
			pTree->ClearSelection();
			HTREEITEM htTable = pTree->FindItemData((DWORD)pTable, pTree->m_htLayouts);
			pTree->Expand(htTable, TVE_EXPAND);
			pTree->EnsureVisible(htTable);

			for (int i = 0; i < GetDocument()->m_wCreatingColumnIds.GetSize(); i++)
			{
				WORD idCol = GetDocument()->m_wCreatingColumnIds[i];

				TableColumn* pCol = pTable->GetColumnByAlias(idCol);
				ASSERT_VALID(pCol);
				
				HTREEITEM htCol = pTree->FindItemData((DWORD)pCol, pTree->m_htLayouts);
				ASSERT(htCol);

				pTree->SetItemState(htCol, TVIS_SELECTED, TVIS_SELECTED);
			}
			pTree->OnMultiSelect();
		}
	}

	SAFE_DELETE(pAr);

/* funzionaltà sospesa
	if (pTblRule)
	{
		ASSERT_VALID(pTblRule);
		HTREEITEM htRule = GetDocument()->SelectRSTreeItemData(ERefreshEditor::Rules, pTblRule);
		if (htRule)
		{
			CNodeTree* pWhere = GetDocument()->GetRSTree(ERefreshEditor::Rules)->GetDescendantNode(htRule, CNodeTree::ENodeType::NT_RULE_QUERY_WHERE); 
			if (pWhere)
			{
				GetDocument()->GetWoormFrame()->GetEngineTreeView()->OnOpenEditor(pWhere);
			}
		}
	}
*/
	return TRUE;
}

// When assingning a LPCSTR to a CStringW or a LPCWSTR to a CStringA,
// ANSI / Unicode conversion is performed.
//-----------------------------------------------------------------------------
void CWoormView::GetDropText(CString& str, COleDataObject* pDataObject)
{
	str = _T("");
	if (pDataObject->IsDataAvailable(CF_UNICODETEXT))
	{
		HGLOBAL hGlobal = pDataObject->GetGlobalData(CF_UNICODETEXT);
		LPCWSTR lpSrc = static_cast<LPCWSTR>(::GlobalLock(hGlobal));
		str = lpSrc;
		::GlobalUnlock(hGlobal);
	}
	else if (pDataObject->IsDataAvailable(CF_TEXT))
	{
		HGLOBAL hGlobal = pDataObject->GetGlobalData(CF_TEXT);
		LPCSTR lpSrc = static_cast<LPCSTR>(::GlobalLock(hGlobal));
		str = lpSrc;
		::GlobalUnlock(hGlobal);
	}
}

// Dropped accept
//-----------------------------------------------------------------------------
BOOL CWoormView::IsDropText(COleDataObject* pDataObject)
{
	return pDataObject->IsDataAvailable(CF_UNICODETEXT) || pDataObject->IsDataAvailable(CF_TEXT);
}

// Drag & Drop View Target
//-----------------------------------------------------------------------------
BOOL CWoormView::ReadDropData(COleDataObject* pDataObject)
{
	ASSERT_VALID(GetDocument());
	ASSERT_VALID(GetWoormFrame());

	CString strCommand;
	GetDropText(strCommand, pDataObject);

	BOOL bOk = FALSE;
	
	//m_PointDrop contains mouse coordinate
	CPoint sp = GetScrollPosition();
	m_PointDrop.x += sp.x;
	m_PointDrop.y += sp.y;
		
	CDC* pDc = GetDC();
	if (pDc)
	{
		UnScalePoint(m_PointDrop, *pDc);
		ReleaseDC(pDc);
	}

	if (strCommand.Compare(DRAGDROP_WOORM_ENGINE_VIEW) == 0)
	{
		ASSERT_VALID(GetWoormFrame()->GetHiddenVariablesTreeView());
		if (UpdateNodeSelection(&GetWoormFrame()->GetHiddenVariablesTreeView()->m_TreeCtrl))
		{
			CNodeTreeArray* pNodesSelection = GetDocument()->m_pNodesSelection;
			ASSERT_VALID(pNodesSelection);
			CNodeTree* pNode = (*pNodesSelection)[0];
			ASSERT_VALID(pNode);

			if (pNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_COLUMN_INFO)
			{
				bOk = AddDBColumnsFromDrop(TRUE);
			}
			else if (pNode->m_NodeType == CNodeTree::ENodeType::NT_VARIABLE)
			{
				bOk =  AddHiddenVariableFromDrop(GetDocument()->m_pNodesSelection, GetWoormFrame()->GetEngineTreeView()->m_bAddTable, NULL);
			}
			else
			{
				ASSERT(FALSE);
				return FALSE;
			}
			if (GetWoormFrame()->GetEngineTreeView()->m_bAddTable)
			{
				//deseleziona il pulsante di creazione nuovo oggetto tabella
				GetWoormFrame()->GetEngineTreeView()->OnCheckAddTable();
			}
			
			return bOk;
		}
	}
	//ToolBox Database
	else if (strCommand.Compare(DRAGDROP_WOORM_DB_VIEW) == 0)	
	{
		ASSERT_VALID(GetWoormFrame()->m_pToolBoxDBView);
		if (UpdateNodeSelection(&GetWoormFrame()->m_pToolBoxDBView->m_TreeCtrl))
		{
			CNodeTreeArray* pNodesSelection = GetDocument()->m_pNodesSelection;
			ASSERT_VALID(pNodesSelection);
			ASSERT(pNodesSelection->GetSize());

			CNodeTree* pNode = (*pNodesSelection)[0];
			ASSERT_VALID(pNode);

			if (pNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_COLUMN_INFO)
			{
				bOk = AddDBColumnsFromDrop(FALSE);
			}
			else if (pNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_DBTABLE ||
						pNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_DBVIEW)
			{
				return AddDBTableFromDrop();
			}
			else
			{
				ASSERT(FALSE);
				return FALSE;
			}

			if (GetWoormFrame()->m_pToolBoxDBView->m_bAddTable)
			{
				//deseleziona il pulsante di creazione nuovo oggetto tabella
				GetWoormFrame()->m_pToolBoxDBView->OnCheckAddTable();
			}
		
			return bOk;
		}
	}
	//ToolBox Objects
	else if (strCommand.Compare(DRAGDROP_WOORM_OBJECTS_VIEW) == 0)
	{
		SAFE_DELETE(this->GetDocument()->m_pNodesSelection);
		ASSERT_VALID(GetWoormFrame()->m_pToolBoxTreeView);

		if (UpdateNodeSelection(&GetWoormFrame()->m_pToolBoxTreeView->m_TreeCtrl))
		{
			ASSERT_VALID(GetDocument()->m_pNodesSelection);
			CNodeTree* pNode = (*GetDocument()->m_pNodesSelection)[0];
			ASSERT_VALID(pNode);

			//empty rect
			if (pNode->m_pItemData == (CObject*)ID_ADD_SQR_RECT)
				GetDocument()->m_Creating = CWoormDocMng::SQRRECT;

			//text rect. Opens edit view for text input
			else if (pNode->m_pItemData == (CObject*)ID_ADD_TEXT_RECT)
			{
				CRSEditView* pEditView = dynamic_cast<CRSEditView*>(GetDocument()->CreateSlaveView(RUNTIME_CLASS(CRSEditView)));
				ASSERT_VALID(pEditView);

				GetDocument()->m_ptCurrPos = m_PointDrop;

				CString sText;
				BOOL bSaved = FALSE;
				pEditView->LoadElement(&sText, &bSaved);
				pEditView->DoEvent();
				if (!bSaved)
					return FALSE;

				GetDocument()->m_strTxtName = sText;
				GetDocument()->m_Creating = CWoormDocMng::TEXTRECT;
			}
				
			//file rect. Opens explorer
			else if (pNode->m_pItemData == (CObject*)ID_ADD_FILE_RECT)
			{
				CTBNamespace aNamespace(GetDocument()->GetNamespace());
				aNamespace.SetObjectName(_T(""));
				aNamespace.SetType(CTBNamespace::TEXT);

				GetDocument()->m_ptCurrPos = m_PointDrop;

				CTBExplorer aExplorer(CTBExplorer::OPEN, aNamespace);
				aExplorer.SetCanLink();
				if (!aExplorer.Open())
					return FALSE;

				CString strPath;
				aExplorer.GetSelPathElement(strPath);

				if (strPath.IsEmpty())
					return FALSE;

				GetDocument()->m_strTxtName = strPath;
				GetDocument()->m_Creating = CWoormDocMng::FILERECT;
			}
			//image rect. Opens explorer
			else if (pNode->m_pItemData == (CObject*)ID_ADD_GRAPH_RECT)
			{
				CTBNamespace aNamespace(GetDocument()->GetNamespace());
				aNamespace.SetObjectName(_T(""));
				aNamespace.SetType(CTBNamespace::IMAGE);

				GetDocument()->m_ptCurrPos = m_PointDrop;

				CTBExplorer aExplorer(CTBExplorer::OPEN, aNamespace);
				aExplorer.SetCanLink();
				if (!aExplorer.Open())
					return FALSE;

				CString strPath;
				aExplorer.GetSelPathElement(strPath);

				if (strPath.IsEmpty())
					return FALSE;

				GetDocument()->m_strBmpName = strPath;

				if (_tcsicmp(GetDocument()->m_strBmpName.Right(3), _T("EMF")) == 0)
					GetDocument()->m_Creating = CWoormDocMng::METAFILERECT;
				else
					GetDocument()->m_Creating = CWoormDocMng::GRAPHRECT;

			}
			//repeater rect
			else if (pNode->m_pItemData == (CObject*)ID_ADD_REPEATER)
			{	
				GetDocument()->m_ptCurrPos = m_PointDrop;
				GetDocument()->m_pEditorManager->AddRepeater(GetDocument()->m_wCreatingId);
				GetDocument()->m_Creating = CWoormDocMng::REPEATER;
			}
			else if (pNode->m_pItemData == (CObject*)ID_ADD_CHART)
			{
				GetDocument()->m_ptCurrPos = m_PointDrop;
				GetDocument()->m_pEditorManager->AddChart(GetDocument()->m_wCreatingId);
				GetDocument()->m_Creating = CWoormDocMng::CHART;
			}
			else if (pNode->m_pItemData == (CObject*)ID_RS_ADD_LINK)
			{
				CClientDC dc(this);
				GetDocument()->m_ptCurrPos = m_PointDrop;
				BaseObj* targetObj = GetTargetDropObject(dc);

				if (!targetObj)
					return FALSE;

				//------------------------------------------
				if (!targetObj->IsKindOf(RUNTIME_CLASS(FieldRect)) && !targetObj->IsKindOf(RUNTIME_CLASS(Table)))
					return FALSE;
				//------------------------------------------

				GetDocument()->m_pCurrentObj=targetObj;
				GetWoormFrame()->m_pPropertyPane->ShowControlBar(TRUE, FALSE, TRUE);
				GetDocument()->OnAddLink();
				return TRUE;
			}
			//total column
			else if (pNode->m_pItemData == (CObject*)ID_COL_ADD_TOTAL)
			{
				CClientDC dc(this);
				GetDocument()->m_ptCurrPos = m_PointDrop;
				Table* targetObj = GetTableAtDropPos(dc);
				if (!targetObj)
					return FALSE;
				TableColumn* col=targetObj->GetActiveColumn();
				if (!col)
					return FALSE;
				col->AddColumnTotal();
				GetDocument()->Invalidate();
				//aggiorno il tree per aggiungere o rimuovere il totale dai figli della colonna
				GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, col);
				return TRUE;
			}
			//func/expr field
			else if (pNode->m_pItemData == (CObject*)ID_RS_ADD_FIELD_NEW_FUNCEXPR)
			{				
				ASSERT_VALID(GetWoormFrame()->m_pObjectPropertyView);
				GetDocument()->m_ptCurrPos = m_PointDrop;

				GetWoormFrame()->m_pPropertyPane->ShowControlBar(TRUE, FALSE, TRUE);

				if (GetWoormFrame()->m_pToolBoxTreeView->m_bAddTable)
					GetWoormFrame()->m_pObjectPropertyView->NewElement(TRUE, FALSE, TRUE);
				else
				{
					CClientDC dc(this);
					Table* targetObj = GetTableAtDropPos(dc);
					if (targetObj)
						GetDocument()->m_pCurrentObj = targetObj;				
					GetWoormFrame()->m_pObjectPropertyView->NewElement(FALSE,FALSE,TRUE);
				}
					
				return TRUE;
			}
			//barcode
			else if (pNode->m_pItemData == (CObject*)ID_ADD_EA_BARCODE)
			{
				GetDocument()->m_ptCurrPos = m_PointDrop;
				GetDocument()->m_Creating = CWoormDocMng::BARCODE;
			}
			else if (pNode->m_pItemData == (CObject*)ID_ADD_ARRAY)
			{
				CRS_ObjectPropertyView* propView = GetWoormFrame()->m_pObjectPropertyView;
				CRS_PropertyGrid* propGrid = propView->GetPropertyGrid();
				propView->NewElement(FALSE, FALSE, TRUE,TRUE);

				// hide type
				propGrid->GetProperty(0)->GetSubItem(0)->RemoveAllOptions();
				propGrid->GetProperty(0)->GetSubItem(0)->AddOption(_TB("Function"), TRUE, 0);
				propGrid->GetProperty(0)->GetSubItem(0)->SelectOption(0);
				propGrid->GetProperty(0)->GetSubItem(0)->Show(TRUE);
				
				//set hidden
				propGrid->GetProperty(0)->GetSubItem(3)->RemoveAllOptions();
				propGrid->GetProperty(0)->GetSubItem(3)->AddOption((LPCTSTR)L"True", TRUE, TRUE);
				propGrid->GetProperty(0)->GetSubItem(3)->SelectOption(0);
				propGrid->GetProperty(0)->GetSubItem(3)->Show(TRUE);

				// set inpit
				propGrid->GetProperty(0)->GetSubItem(4)->RemoveAllOptions();
				propGrid->GetProperty(0)->GetSubItem(4)->AddOption(_T("Input"), TRUE, WoormField::RepFieldType::FIELD_INPUT);
				propView->m_fieldType = WoormField::RepFieldType::FIELD_INPUT;
				propGrid->GetProperty(0)->GetSubItem(4)->SelectOption(0);
				propGrid->GetProperty(0)->GetSubItem(4)->Show(TRUE);

				//set Type
				propGrid->GetProperty(0)->GetSubItem(5)->RemoveAllOptions();
				*((CRSBoolProp*)propGrid->GetProperty(0)->GetSubItem(5))->m_pBValue = TRUE;
				propGrid->GetProperty(0)->GetSubItem(5)->SetValue(true);
				propGrid->GetProperty(0)->GetSubItem(5)->Show(TRUE);

			}
				
			//Drop of the object and creation
			CClientDC dc(this);
			GetDocument()->m_ptCurrPos = m_PointDrop;
			GetDocument()->CreateListObj(dc, this, TRUE);
			m_pProcessingMouse->Unlock();
			GetDocument()->m_Creating = CWoormDocMng::NONE;
		}
	}
	else
		return FALSE;

	GetDocument()->Invalidate(TRUE);
	return TRUE;
}

//-----------------------------------------------------------------------------
DROPEFFECT CWoormView::OnDragEnter(COleDataObject* pDataObject, DWORD dwKeyState, CPoint point)
{
	if (IsDropText(pDataObject))
	{
		m_PointDrop = point;
		return DROPEFFECT_COPY;
	}
	return DROPEFFECT_NONE;
}

//-----------------------------------------------------------------------------
DROPEFFECT CWoormView::OnDragOver(COleDataObject* pDataObject, DWORD dwKeyState, CPoint point)
{
	if (IsDropText(pDataObject))
	{
		m_PointDrop = point;
		return DROPEFFECT_COPY;
	}
	return DROPEFFECT_NONE;
}

//-----------------------------------------------------------------------------
BOOL CWoormView::OnDrop(COleDataObject* pDataObject, DROPEFFECT dropEffect, CPoint point)
{
	BOOL bRet;
	bRet = ReadDropData(pDataObject);
	//update repeater
	GetDocument()->ApplyRepeater(GetDocument()->m_Objects);
	return bRet;
}

//-----------------------------------------------------------------------------
void CWoormView::OnDragLeave()
{
	
}

/////////////////////////////////////////////////////////////////////////////
// CWoormView diagnostics

#ifdef _DEBUG
void CWoormView::AssertValid() const
{
	CBCGPScrollView::AssertValid();
}

void CWoormView::Dump(CDumpContext& dc) const
{
	CBCGPScrollView::Dump(dc);
}

CWoormDocMng* CWoormView::GetDocument() // non-debug version is inline
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDocMng)));
	return (CWoormDocMng*) m_pDocument;
}

#endif //_DEBUG


