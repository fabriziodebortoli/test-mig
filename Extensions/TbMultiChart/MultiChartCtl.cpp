
#include "stdafx.h"

#include <TbGenlib\Tfxdatatip.h>
#include <tbwoormviewer\TBPrintDialog.h>

#include "MultiChartCtl.h"
#include "resource.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//=====================================================================================
#define RBG_REV(r,g,b) RGB(b,g,r)

/*
    RGB (255,   0,   0),    // Red
	RGB (  0, 255,   0),    // Green
    RGB (255, 255,   0),    // Yellow
    RGB (255,   0, 255),    // Magenta
    RGB (  0, 255, 255),    // Cyan
    RGB (  0,   0, 255),    // Blue

*/
const COLORREF g_crColors [] = 
{
 	RBG_REV (0,		0,		0xff),		
 	RBG_REV (0,		0xff,	0),    
	RBG_REV (0,		0xff,	0xff),    
	RBG_REV (0xff,	0,		0xff),
	RBG_REV (0xff,	0xff,	0),    
	RBG_REV (0xff,	0,		0), 

    RBG_REV (0xc0, 0xff, 0xff),   
    RBG_REV (0xc0, 0xc0, 0xff),   
    RBG_REV (0xc0, 0xff, 0xc0),   
    RBG_REV (0xff, 0xc0, 0xc0),    

	RBG_REV (0xc0, 0xe0, 0xff),	
    RBG_REV (0xff, 0xff, 0xc0),    
	RBG_REV (0xff, 0xc0, 0xff),  
	RBG_REV (0x80, 0xff, 0x80),    
	RBG_REV (0x80, 0x80, 0xff),			
	RBG_REV (0xff, 0xff, 0x80),    
	RBG_REV (0x80, 0xc0, 0xff),    
	RBG_REV (0xff, 0x80, 0x80),  
    RBG_REV (0x80, 0xff, 0xff),    
	RBG_REV (0xff,	0x80,	0xff),  
	   
	RBG_REV (0,		0xc0,   0),
	RBG_REV (0,		0,		0xc0),		
	RBG_REV (0,		0xc0,	0xc0),    
	RBG_REV (0,		0x40,	0xc0),    
	RBG_REV (0xc0,	0,		0),
	RBG_REV (0,		0x80,	0x80),
    RBG_REV (0,		0x80,	0xff),    
	RBG_REV (0xc0,	0xc0,	0),
	RBG_REV (0xc0,	0,		0xc0),
	RBG_REV (0,		0x40,	0x80),
	RBG_REV (0x80,	0,		0x40),

	RBG_REV (0,		0x80,	0),
	RBG_REV (0x80,	0x80,	0),
	RBG_REV (0,		0,		0x80),		
	RBG_REV (0x80,	0,		0x80),

	RBG_REV (0,		0,		0x40),		
	RBG_REV (0x40,	0x40,	0x80),
	RBG_REV (0,		0x40,	0x40),
	RBG_REV (0,		0x40,	0),
	RBG_REV (0x40,	0x40,	0),
	RBG_REV (0x40,	0,		0),
	RBG_REV (0x40,	0,		0x40),

	// black
	RGB (0, 0, 0)						//42
};

const int NUM_COLOR_BAR	= sizeof(g_crColors) / sizeof(COLORREF);

//-------------------------------------------------------------------------------------

CGDIResources::CGDIResources()
	:
	m_nProgresTagBrush (0)
{
	m_bmpIconInfo.LoadBitmap(IDB_ICON_INFO);
}
//-------------------------------------------------------------------------------------

CGDIResources::~CGDIResources()
{
	m_fontValues.DeleteObject();
	m_fontHeaders.DeleteObject();
	m_bmpIconInfo.DeleteObject();

	CBrush* pBrush=NULL; 
	CPen* pPen=NULL; 
	CString strKey;	
	POSITION pos; 
	COLORREF crKey;
	int i=0;
	
	pos = m_mapBrushes.GetStartPosition();
	while(pos)
	{
		m_mapBrushes.GetNextAssoc( pos, strKey, (CObject*&)pBrush );
		VERIFY( pBrush->DeleteObject() );
		delete pBrush;
		i++;
	}

	pos = m_mapPens.GetStartPosition();
	while(pos)
	{
		m_mapPens.GetNextAssoc( pos, crKey, pPen ) ;
		VERIFY( pPen->DeleteObject() );
		delete pPen;
		i++;
	}

	CBrushInfo* pBInfo;
	pos = m_mapTagBrushes.GetStartPosition();
	while(pos)
	{
		m_mapTagBrushes.GetNextAssoc( pos, strKey, (void*&)pBInfo ) ;
		delete pBInfo;
	}
}

//------------------------------------------------------------------------------
void CGDIResources::ClearMapTagBrushes()
{
	CString strKey;	
	CBrushInfo* pBInfo;
	POSITION pos = m_mapTagBrushes.GetStartPosition();
	while(pos)
	{
		m_mapTagBrushes.GetNextAssoc( pos, strKey, (void*&)pBInfo ) ;
		delete pBInfo;
	}
	m_mapTagBrushes.RemoveAll();
	m_nProgresTagBrush = 0;
}

//-------------------------------------------------------------------------------------
CBrush* CGDIResources::GetBrush (COLORREF crColor, int nHatch /*=0*/)
{
	CBrush* pBrush = NULL;
	CString strKey;
	ASSERT(nHatch < 10);
	strKey.Format(_T("%08x%01x"), crColor, nHatch);
	if ( m_mapBrushes.Lookup(strKey, (CObject*&)pBrush) == 0)
	{
		pBrush = new CBrush;
		if (nHatch != 0)
			pBrush->CreateHatchBrush (nHatch, crColor);
		else
			pBrush->CreateSolidBrush (crColor);

		m_mapBrushes.SetAt(strKey, pBrush);
	}
	return pBrush;
}
//-------------------------------------------------------------------------------------

CPen* CGDIResources::GetPen (COLORREF crColor)
{
	CPen* pPen = NULL;
	if ( m_mapPens.Lookup(crColor, pPen) == 0)
	{
		pPen = new CPen();
		pPen->CreatePen (PS_SOLID, 1, crColor);

		m_mapPens.SetAt(crColor, pPen);
	}
	return pPen;
}
//-------------------------------------------------------------------------------------

int CGDIResources::LookUpTagBrush (BOOL bAddIfMissing, const CString& strTabBrush, COLORREF& crColor, short& nHatch)
{
	int HatchTypes[] = { 0, HS_BDIAGONAL,HS_FDIAGONAL,HS_DIAGCROSS,HS_VERTICAL,HS_CROSS,HS_HORIZONTAL };

	CBrushInfo* pBrushInfo = NULL;

	if (m_mapTagBrushes.Lookup(strTabBrush, (void*&)pBrushInfo) == 0)
	{
		if (bAddIfMissing)
		{
			int nIdxColor = m_nProgresTagBrush % (NUM_COLOR_BAR-1);
			crColor = g_crColors[nIdxColor];

			int nIdxHatch = m_nProgresTagBrush / (NUM_COLOR_BAR -1);
			if (nIdxHatch < 7)
				nHatch = HatchTypes[nIdxHatch];
			else
			{
				TRACE("Non ho piu' modo di differenziare i tag con i colori e gli hatch: uso solo i colori\n");
				ASSERT(FALSE);
			}

			m_nProgresTagBrush++;

			pBrushInfo = new CBrushInfo(crColor,nHatch);
			m_mapTagBrushes.SetAt(strTabBrush, (void*&)pBrushInfo);

			return TRUE;
		}
		
		crColor = 0;
		nHatch = 0;

		return FALSE;
	}

	crColor = pBrushInfo->m_crColor;
	nHatch = pBrushInfo->m_nHatch;

	return TRUE;
}


/////////////////////////////////////////////////////////////////////////////
//Class MultiChartEventArguments Implementation : Arguments Event 
////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(MultiChartEventArguments, CObject)

/////////////////////////////////////////////////////////////////////////////
//Class MultiChartCtrl Implementation 
////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CMultiChartCtrl, CWnd)

BEGIN_MESSAGE_MAP(CMultiChartCtrl, CWnd)

	ON_WM_SIZE ()
	ON_WM_CREATE()
	ON_WM_VSCROLL ()
	ON_WM_HSCROLL ()
	ON_WM_KEYDOWN ()

	ON_WM_LBUTTONDOWN ()
	ON_WM_LBUTTONUP ()
	//ON_WM_RBUTTONDOWN ()
	ON_WM_RBUTTONUP ()

	ON_WM_MOUSEMOVE ()

    ON_COMMAND (IDM_ABOUTBOX, OnAboutBox)
	ON_COMMAND (IDM_MC_SAVE, OnSave)
	ON_COMMAND (IDM_MC_PRINT, Print)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------
CMultiChartCtrl::CMultiChartCtrl()
{
	m_bIsSaving = false; 
	m_bIsTracking = false;

	m_nTrackRow = -1;
	m_nTrackCol = -1;
	m_pTrackBar = NULL;

	m_nAllocatedRows = 0;
	m_nAllocatedCols = 0;
	m_nAllocatedBars = 0;

	m_State = InvGrid;

	m_arRows = NULL;

	m_arTotHeightPosBar = NULL;

	m_arLimits = NULL;
	m_arInfoBars = NULL;
	m_arInfoRowHeader = NULL;
	m_arInfoColHeader = NULL;
	// ---- 

	m_nTypeChart = 0;

	m_nRows = 3;
	m_nCols = 4;
	m_nBars = 5;

	m_nNumLabelRowHeader = 1;
	m_nNumLabelColHeader = 1;

	m_nDCHeightAllRows = 100;
	m_nDCWidthCols = 100;
	m_nDCHeightColHeader = 20;
	m_nDCWidthRowHeader = 100;

	m_bUseCustomWidthBars = FALSE;
	m_nDCWidthBars = ( (m_nDCWidthCols / (m_nBars*2+m_nBars+1)) * 2);//spaziatura = metà larghezza barra
	m_bUseCustomPosBars = FALSE;

	m_dMaxHeightAllBars = 100.0;
	m_dMinHeightAllBars = 0;
	m_dDefaultHeightBars = 0.0;
	m_dStepHeightGrid = 10.0;
	m_nDCStepHeightGrid = 10;

	m_strFormatHeightBars = _T("%7.2f");
	m_strTitleBars = _T("Not Used");

	m_bShowGrid = TRUE;
	m_bShowHeightBars = FALSE;
	m_bShowTitleBars = FALSE;
	m_bShowTotBars = FALSE;
	m_bShowToolTip = FALSE;
	m_bShowTrueValue = TRUE;
	m_nShowGridScale = 0;
	m_bHideBoxBars = FALSE;
	m_bShowIconInfo = FALSE;

	m_crColorGrid = RGB(180,180,180);
	m_crColorZoneSeparator = RGB(0,0,0);
	m_crBackColor = RGB(255,255,255);
	// ----

	m_szColumnLabel = _T("C");
	m_szRowLabel = _T("R");
	m_nDCHFontValues = 6;
	m_nDCHFontHeaders = 8;
	m_bShowPercentValues = FALSE;
	m_nWhereShowValueBars = 0;
	// ----

	m_nHeight = m_nRows * m_nDCHeightAllRows + m_nDCHeightColHeader;
	m_nWidth = m_nCols * m_nDCWidthCols + m_nDCWidthRowHeader;

    m_nScrollStepX = m_nDCWidthCols;
    m_nScrollStepY = m_nDCHeightAllRows;

	m_nScrollPosX = 0;
	m_nScrollPosY = 0;

	m_bVScroll = FALSE;
	m_bHScroll = FALSE;

	m_pVScrollBar = NULL;
	m_pHScrollBar = NULL;
	m_nScrollPosRow = m_nScrollPosCol = -1;

//	m_pBtnSpinRows= NULL;
	// ----

	m_pSelectedBar = NULL;
	m_nSelectedRowHeader = -1;
	m_nSelectedColHeader = -1;
	m_nXPosLastClick = -1;
	m_nYPosLastClick = -1;
	// ----

	//SetInitialSize( int cx, int cy );
	//	TO DO
	//----

	m_datatip = new TFXDataTip;
	m_datatip->Create(this);
//	SetBackColor( RGB(255,255,255) );

	//-----
	m_nNumeroSoglie = 0;
	m_arSoglie = NULL;

	m_crLeftUpperCornerBackColor = RGB(128, 128, 128);

}
//----------------------------------------------------------------------------
CMultiChartCtrl::~CMultiChartCtrl()
{
//	if ( GetCapture() == this ) ReleaseCapture ();

	FreeGrid();
	
	if (m_pVScrollBar) delete(m_pVScrollBar);
	if (m_pHScrollBar) delete(m_pHScrollBar);
//	if (m_pBtnSpinRows)delete(m_pBtnSpinRows);
	delete m_datatip;
}

//---------------------------------------------------------------------------------
void CMultiChartCtrl::ThrowError(int a, int b) //TODO
{
	CString sErr;
	sErr.Format(_T("Errore %d - %d"), a, b);
	AfxMessageBox(sErr);
}

//---------------------------------------------------------------------------------
void CMultiChartCtrl::Print() 
{
    CDC dc;
    CTBPrintDialog printDlg(FALSE);

    if (printDlg.DoModal() == IDCANCEL)         // Get printer settings from user
        return;

    dc.Attach(printDlg.GetPrinterDC());         // Attach a printer DC
    dc.m_bPrinting = TRUE;


    CString strTitle = _T("Grafico");                           // Get the application title
    //strTitle.LoadString(AFX_IDS_APP_TITLE);

    DOCINFO di;                                 // Initialise print document details
    ::ZeroMemory (&di, sizeof (DOCINFO));
    di.cbSize = sizeof (DOCINFO);
    di.lpszDocName = strTitle;

    BOOL bPrintingOK = dc.StartDoc(&di);        // Begin a new print job

    // Get the printing extents and store in the m_rectDraw field of a 
    // CPrintInfo object
    CPrintInfo Info;
    Info.m_rectDraw.SetRect(0,0, 
                            dc.GetDeviceCaps(HORZRES), 
                            dc.GetDeviceCaps(VERTRES));

	dc.DPtoLP(Info.m_rectDraw);

	//m_bIsSaving = true;

    dc.StartPage();      // begin new page

	PrepareScrollInfo (dc.GetDeviceCaps(HORZRES), dc.GetDeviceCaps(VERTRES));

	OnDraw(&dc, Info.m_rectDraw);
       
	bPrintingOK = (dc.EndPage() > 0);       // end page

    if (bPrintingOK)
        dc.EndDoc();                            // end a print job
    else
        dc.AbortDoc();                          // abort job.

    dc.Detach();                         // detach the printer DC
}
//--------------------------------------------------------------------
    //CString strTitle;                           // Get the application title
    //strTitle.LoadString(AFX_IDS_APP_TITLE);
/*
    DOCINFO di;                                 // Initialise print document details
    ::ZeroMemory (&di, sizeof (DOCINFO));
    di.cbSize = sizeof (DOCINFO);
    di.lpszDocName = strTitle;

    BOOL bPrintingOK = dc.StartDoc(&di);        // Begin a new print job

    // Get the printing extents and store in the m_rectDraw field of a 
    // CPrintInfo object
    CPrintInfo Info;
    Info.m_rectDraw.SetRect(0,0, 
                            dc.GetDeviceCaps(HORZRES), 
                            dc.GetDeviceCaps(VERTRES));

    OnBeginPrinting(&dc, &Info);                // Call your "Init printing" funtion

    for (UINT page = Info.GetMinPage(); 
         page <= Info.GetMaxPage() && bPrintingOK; 
         page++)
    {
        dc.StartPage();                         // begin new page
        Info.m_nCurPage = page;
        OnPrint(&dc, &Info);                    // Call your "Print page" function
        bPrintingOK = (dc.EndPage() > 0);       // end page
    }

    OnEndPrinting(&dc, &Info);                  // Call your "Clean up" funtion

    if (bPrintingOK)
        dc.EndDoc();                            // end a print job
    else
        dc.AbortDoc();                          // abort job.
*/

//----------------------------------------------------------------------------------
/*
void PrintPreview(CWnd *pWnd) 
{
	CString strWords;
	CWnd* pWnd = GetDlgItem(IDC_STATIC_PREVIEW); // This is a rectangle control
	CDC* dc;
	CDC memoriDC;
	CBitmap memBMP;
	CBitmap* pOldBMP;
	CFont fnt;
	CFont* pOldFnt;
	CRect rect;
	CRect rectMemory;
	CSize zMetrix;

	//
	//
	//
	CTBPrintDialog pdlg(FALSE);
	DOCINFO di;
	CDC prnDC;

	di.cbSize = sizeof(DOCINFO);
	di.lpszDocName = "This string will appear in Printer Queue";
	di.lpszOutput = NULL;     
	di.lpszDatatype = NULL;
	di.fwType = 0;

	//
	// Get current printer setting
	//
	pdlg.GetDefaults();

	
	//
	dc = pWnd->GetDC();
	pWnd->GetClientRect(&rect);

	// DC printer???
	if( !prnDC.Attach(pdlg.GetPrinterDC()) )
		AfxMessageBox("Invalid Printer DC");

	memoriDC.CreateCompatibleDC(&prnDC); // Create DC for Preview

	//
	// Get the resolution of Screen and Current Default Printer
	//
	int iPrnX = prnDC.GetDeviceCaps(HORZRES);
	int iPrnY = prnDC.GetDeviceCaps(VERTRES);
	int iMonX = dc->GetDeviceCaps(HORZRES); // Device Target is Monitor
	int iMonY = dc->GetDeviceCaps(VERTRES);


	rectMemory.top = 0;
	rectMemory.left = 0;
	rectMemory.bottom = iPrnY;
	rectMemory.right = iPrnX;

	//
	// Create a Memory Bitmap that is compatible with the Printer DC
	// then select or make the bitmap as current GDI active object
	//
	memBMP.CreateCompatibleBitmap(&prnDC, rectMemory.Width(), rectMemory.Height());
	pOldBMP = memoriDC.SelectObject(&memBMP);

	//
	// Clear memory DC or in other words
	// paint the bitmap with white colour and transparent text
	//
	memoriDC.SetBkMode(TRANSPARENT);
	memoriDC.SetTextColor(RGB(0, 0, 0));
	memoriDC.PatBlt(0, 0, rectMemory.Width(), rectMemory.Height(), WHITENESS);

	//
	// Prepare the font
	//
	int iPointz = 100;
	fnt.CreatePointFont(iPointz, "OCR A", &memoriDC);
	strWords.Format("This is line number    ");        // Test string
	pOldFnt = memoriDC.SelectObject(&fnt);
	zMetrix = memoriDC.GetTextExtent(strWords);
	int iPos = 0;

	//
	// Write string or Paint something
	//
	int iMaksimal = 0;
	int iLineHeight = 1;
	int iLoop;
	CString strPuncak;

	//
	// Calculate how many lines we could fit
	//
	for(iLoop = 1; iLoop < 100; iLoop++)
	{
		if( ((zMetrix.cy+iLineHeight)*iLoop) < iPrnY ) 
			iMaksimal++;
	}

	strPuncak.Format("Maximum Amount of line(s) for %d points are %d lines", 
		iPointz, iMaksimal);
	
	//
	//
	//
	for(iLoop = 0; iLoop < iMaksimal; iLoop++)
	{
		strWords.Format("This is line %d", iLoop);
		memoriDC.TextOut(0, iLoop*(zMetrix.cy+iLineHeight), strWords);
	}
	
	//
	// Reseting font
	//
	memoriDC.SelectObject(pOldFnt);

	//
	// Calculate ratio
	//
	float fXRatio = (float) iMonX/iPrnX;
	float fYRatio = (float) iMonY/iPrnY;
	
	
	//  iLebar = Width
	//  iTinggi = Height
	//  iXPosisiPreview = horisontal location of preview
	//  iYPosisiPreview = vertical location of preview
	//
	int iLebar = rect.Width()*fXRatio;
	int iTinggi = rect.Height()*fYRatio;
	int iXPosisiPreview = (rect.Width() - iLebar)/2;
	int iYPosisiPreview = (rect.Height() - iTinggi)/2;
	CPen pen(PS_SOLID, 2, RGB(255, 0, 0));
	CPen* pOldPen;

	//
	// Create an outline
	//	
	pOldPen = dc->SelectObject(&pen);
	dc->Rectangle(iXPosisiPreview, iYPosisiPreview,iXPosisiPreview + iLebar + 2,
		iYPosisiPreview + iTinggi + 2);
	dc->SelectObject(pOldPen);

	//
	// Put in the box
	//
	dc->StretchBlt(iXPosisiPreview , iYPosisiPreview, iLebar, iTinggi,
		&memoriDC, 0, 0, rectMemory.Width(), rectMemory.Height(), SRCCOPY);
	
	//
	// Cleaning Up
	//
	fnt.DeleteObject();
	memoriDC.SelectObject(pOldBMP);
	memoriDC.DeleteDC();
	memBMP.DeleteObject();
	prnDC.Detach();

	//
	pWnd->ReleaseDC(dc);
}

  // crea il font verticale: Arial 9 punti 
	LOGFONT lf;
	memset(&lf, 0, sizeof(LOGFONT));
	lf.lfHeight = 90;
	lf.lfWeight = FW_NORMAL;
	lf.lfCharSet = DEFAULT_CHARSET;
	LPCTSTR lpszFaceName = _T("Arial");
	lstrcpyn(lf.lfFaceName, lpszFaceName, sizeof(lpszFaceName) / sizeof(lpszFaceName[0]));
	lf.lfEscapement = lf.lfOrientation = 900;	// decimi di grado
	m_fontVertical.CreatePointFontIndirect(&lf, pDC);

*/