
#include "stdafx.h"

#include <math.h>

#include <TbParser\Parser.h>

#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\globals.h>


#include <TbGenlib\generic.h>
#include <TbGenlib\reswalk.h>
#include <TbGenlib\baseapp.h>

#include "TBPrintDialog.h"
#include "pageinfo.h"
#include "pageinfo.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


//===========================================================================
// CSampleIcon implementation
//===========================================================================

BEGIN_MESSAGE_MAP(CSampleIcon, CButton)
	//{{AFX_MSG_MAP(CSampleIcon)
	ON_WM_DRAWITEM()
	ON_WM_ERASEBKGND()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
void CSampleIcon::SetRefRect(const CPoint& ptUlc, const CSize& sizeRectSize)
{
	m_ptUlc				= ptUlc;
	m_sizeRefRectSize	= sizeRectSize;
}
 
//-----------------------------------------------------------------------------
void CSampleIcon::SizeToContent(double ratio)
{                                                                                            
	m_bLandscape = (ratio > 1.);

	int		nX,nY;
	int		nCx,nCy;

	if (m_bLandscape)
	{
		nCx = m_sizeRefRectSize.cx;
		nCy = (int) (m_sizeRefRectSize.cy / ratio);
		
		nX = m_ptUlc.x;
		nY = (m_sizeRefRectSize.cy - nCy) / 2 + m_ptUlc.y;
	}
	else
	{
		nCx = (int) (m_sizeRefRectSize.cx * ratio);
		nCy = m_sizeRefRectSize.cy;
		
		nX = (m_sizeRefRectSize.cx - nCx) / 2 + m_ptUlc.x;
		nY = m_ptUlc.y;
	}    
    
	SetWindowPos(NULL, nX, nY, nCx, nCy, SWP_NOACTIVATE|SWP_NOZORDER); 
}

//-----------------------------------------------------------------------------
void CSampleIcon::DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct)
{
	CDC* pDC = CDC::FromHandle(lpDrawItemStruct->hDC);
	ASSERT(pDC != NULL);

	CRect rect;
	GetClientRect(rect);
	int cxClient = rect.Width();
	int cyClient = rect.Height();

	// load icon
	HICON hicon = LoadWalkIcon(IDI_PAGE_SAMPLE);
		
	if (hicon == NULL) return;

	// draw icon into off-screen bitmap
	int cxIcon = ::GetSystemMetrics(SM_CXICON);
	int cyIcon = ::GetSystemMetrics(SM_CYICON);

	CBitmap bitmap;
	if (!bitmap.CreateCompatibleBitmap(pDC, cxIcon, cyIcon)) return;
	
	CDC dcMem;
	if (!dcMem.CreateCompatibleDC(pDC)) return;
	
	CBitmap* pBitmapOld = dcMem.SelectObject(&bitmap);
	if (pBitmapOld == NULL) return;

#define CX_SHADOW 4
#define CY_SHADOW 4

	// blt the bits already on the window onto the off-screen bitmap
	dcMem.StretchBlt(0, 0, cxIcon, cyIcon, pDC,
		1, 1, cxClient-CX_SHADOW-2, cyClient-CY_SHADOW-2, SRCCOPY);

	// draw the icon on the background
	dcMem.DrawIcon(0, 0, hicon);

	// draw border around icon
	CPen pen;
	pen.CreateStockObject(BLACK_PEN);
	CPen* pPenOld = pDC->SelectObject(&pen);
	pDC->Rectangle(0, 0, cxClient-CX_SHADOW, cyClient-CY_SHADOW);
	if (pPenOld)
		pDC->SelectObject(pPenOld);

	// draw shadows around icon
	CBrush br;
	br.CreateStockObject(DKGRAY_BRUSH);
	rect.SetRect(cxClient-CX_SHADOW, CY_SHADOW, cxClient, cyClient);
	pDC->FillRect(rect, &br);
	rect.SetRect(CX_SHADOW, cyClient-CY_SHADOW, cxClient, cyClient);
	pDC->FillRect(rect, &br);

	// draw the icon contents
	pDC->StretchBlt(1, 1, cxClient-CX_SHADOW-2, cyClient-CY_SHADOW-2,
		&dcMem, 0, 0, cxIcon, cyIcon, SRCCOPY);
}

//-----------------------------------------------------------------------------
BOOL CSampleIcon::OnEraseBkgnd(CDC*)
{
	return TRUE;    // we don't do any erasing...
}

//===========================================================================
//						PrinterInfoItem implementation
//===========================================================================
//
//---------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (PrinterInfoItem, CObject);

//---------------------------------------------------------------------------
PrinterInfoItem::PrinterInfoItem
	(
		CString	strPaperSize,
		WORD	wPaperType,
		CSize	sPaperSize
	)
	{
		m_strPaperSize	= strPaperSize;
		m_wPaperType	= wPaperType;
		m_sPaperSize	= sPaperSize;
	}

//---------------------------------------------------------------------------
PrinterInfoItem::PrinterInfoItem(const PrinterInfoItem& aSource)
{
	*this = aSource;
}

// copiare anche il vettore.
//------------------------------------------------------------------------------
PrinterInfoItem& PrinterInfoItem::operator = (const PrinterInfoItem& source)
{
	m_strPaperSize	= source.m_strPaperSize;
	m_wPaperType	= source.m_wPaperType;
	m_sPaperSize	= source.m_sPaperSize;

	return *this;
}

//===========================================================================
//						PrinterInfo implementation
//===========================================================================
//
//---------------------------------------------------------------------------
PrinterInfo::PrinterInfo()
{
	m_wMinWidth		= 0;
	m_wMaxWidth		= 0;
	m_wMinHeight	= 0;
	m_wMaxHeight	= 0;
	m_strPreferredPrinter = _T("");

	// determina le caratteristiche della stampante di default
	FillPaperInfo();
}

//---------------------------------------------------------------------------
PrinterInfo::PrinterInfo(CString strPreferredPrinter)
{
	m_wMinWidth		= 0;
	m_wMaxWidth		= 0;
	m_wMinHeight	= 0;
	m_wMaxHeight	= 0;
	m_strPreferredPrinter = strPreferredPrinter;

	// determina le caratteristiche della stampante preferenziale
	FillPaperInfo();
}
//--------------------------------------------------------------------------
#define MAX_AMOUNT   256
#define MAX_PAPERS   69

//--------------------------------------------------------------------------
#define DEVICE_CAPS(c,d,e)  \
	pfnDevCaps \
	(\
		(LPTSTR)m_strPrinterName.GetBuffer(m_strPrinterName.GetLength()), \
		(LPTSTR)m_strPortName.GetBuffer(m_strPortName.GetLength()), \
		(WORD)c, (LPTSTR )d, \
		(LPDEVMODE)e\
	);\
	m_strPrinterName.ReleaseBuffer();\
	m_strPortName.ReleaseBuffer()

//--------------------------------------------------------------------------
#define PAPER_ELEMENT_SIZE	sizeof(WORD)

// Purpose:		Works around the bug in current version of Win32s (1.25.142) where
//				DC_DINS and DC_PAPERS retrieve a list of DWORD items instead of 
//				a list of WORD items
//--------------------------------------------------------------------------------
BOOL PrinterInfo::BypassWin32sBug(WORD *pList, DWORD dwTotal)
{
	BOOL bBugged = FALSE;
	return bBugged;
}
//-----------------------------------------------------------------------------
BOOL PrinterInfo::FillPaperInfo()
{
	HINSTANCE hDriver = NULL; //only used if on Win32s;
	
	int  (CALLBACK* pfnDevCaps) (
	    LPCTSTR   pDevice,	// address of device-name string 
    	LPCTSTR   pPort,	// address of port-name string 
    	WORD  fwCapability,	// device capability to query 
    	LPTSTR   pOutput,	// address of the output 
    	CONST DEVMODE *  pDevMode 	// address of structure with device data   
	);
			
	CTBPrintDialog dlgPrint(FALSE);

	AfxGetLoginContext()->GetTbDevParams(dlgPrint.m_pd.hDevMode, dlgPrint.m_pd.hDevNames);
	
	//se ha la preferenziale
	HGLOBAL hDevMode = NULL;
	HGLOBAL hDevNames = NULL;
	
	if (!m_strPreferredPrinter.IsEmpty())
	{
		if (GetPrinterDevice(m_strPreferredPrinter, &hDevNames, &hDevMode))
		{
			dlgPrint.m_pd.hDevMode = hDevMode;
			dlgPrint.m_pd.hDevNames = hDevNames;
		}
		else
		{
			ASSERT (FALSE);
		}
	}

	LPDEVNAMES lpDevNames = (LPDEVNAMES)::GlobalLock(dlgPrint.m_pd.hDevNames);
	ASSERT(lpDevNames != NULL);
	if (lpDevNames == NULL)
		return FALSE;

	m_strPrinterName = ((LPCTSTR)lpDevNames + lpDevNames->wDeviceOffset);
	m_strDriverName = (LPCTSTR)lpDevNames + lpDevNames->wDriverOffset;
	m_strPortName = (LPCTSTR)lpDevNames + lpDevNames->wOutputOffset;
	m_strDriverName += _T(".DRV");

	::GlobalUnlock(dlgPrint.m_pd.hDevNames);

	// mappa la routine a partire dal driver
	pfnDevCaps = &DeviceCapabilities;	 			

	DWORD       dwBufSize1 = 0, dwBufSize2 = 0;
	WORD FAR    *pawPaperList;
	POINT FAR   *paptPaperList;
	int i;

	// svuota i correnti (se vi sono) valori all'interno del vettore dei formati supportati
	m_PrinterPaperInfo.RemoveAll();
	if (pfnDevCaps)
	{
		dwBufSize1 = DEVICE_CAPS(DC_PAPERS, NULL, NULL);		// get paper type (i.e. A4, Letter)
		dwBufSize2 = DEVICE_CAPS(DC_PAPERSIZE,NULL, NULL);      // get paper dimensions

		// allocate space for paper sizes
		pawPaperList = (WORD FAR*)malloc(dwBufSize1 * PAPER_ELEMENT_SIZE);
		paptPaperList = (POINT FAR*) malloc(dwBufSize2 * sizeof(POINT));

		// fill buffer with paper list (occhio al buco di Win32S)
		DEVICE_CAPS(DC_PAPERS, pawPaperList, NULL);
		BypassWin32sBug(pawPaperList, dwBufSize1); 

		// fill buffer with paper dimensions
		DEVICE_CAPS(DC_PAPERSIZE, paptPaperList, NULL);

		// display results
		if (dwBufSize1 > 0 && dwBufSize1 < MAX_AMOUNT)
		{
			LPTSTR lpstrPaperNames;
			DWORD dwMemSize = dwBufSize1 * (sizeof(short) + (64 * sizeof(TCHAR)));

			lpstrPaperNames = (LPTSTR)malloc(dwMemSize);
			DEVICE_CAPS(DC_PAPERNAMES, lpstrPaperNames, NULL);

			for (i = 0; i < (int)dwBufSize1; i++)
			{
				CString strPaperSize((LPTSTR )lpstrPaperNames + (64 * i));
				m_PrinterPaperInfo.Add(new PrinterInfoItem
					(
						strPaperSize,
						pawPaperList[i],
						CSize(paptPaperList[i].x, paptPaperList[i].y)
					));
			}
			free(lpstrPaperNames);
		}
		else
		{
			TRACE("Driver gave bad info!\n");
			ASSERT(FALSE);
		}				

		//SIZES
		dwBufSize1 = DEVICE_CAPS(DC_MINEXTENT, NULL, NULL); // get min paper extent
		dwBufSize2 = DEVICE_CAPS(DC_MAXEXTENT, NULL, NULL); // get max paper extent

	
		m_wMinWidth		= LOWORD(dwBufSize1);
		m_wMaxWidth		= LOWORD(dwBufSize2);
		m_wMinHeight	= HIWORD(dwBufSize1);
		m_wMaxHeight	= HIWORD(dwBufSize2);

		// clean up
		free(pawPaperList);
		free(paptPaperList);
   }
   else
   {
		TRACE("DeviceCapabilities is not supported by driver!\n");
		ASSERT(FALSE);
   }				
   if (hDriver)
   {
   		FreeLibrary(hDriver);
		hDriver = NULL;
   }

   if (hDevMode != NULL) 
	   GlobalFree(hDevMode);
   if (hDevNames != NULL)
	   GlobalFree(hDevNames);

   return TRUE;
}

//---------------------------------------------------------------------------
int	PrinterInfo::GetPrinterInfoItem(WORD wPaperSize)
{
	for (int i = 0; i <= m_PrinterPaperInfo.GetUpperBound(); i++)
	{
		PrinterInfoItem* pItem = (PrinterInfoItem*) m_PrinterPaperInfo[i];
		if (pItem->m_wPaperType == wPaperSize)
			return i;
	}

	return -1;
}
//---------------------------------------------------------------------------
PrinterInfoItem* PrinterInfo::GetPrinterInfoItemObject(WORD wPaperSize){
	for (int i = 0; i <= m_PrinterPaperInfo.GetUpperBound(); i++)
	{
		PrinterInfoItem* pItem = (PrinterInfoItem*)m_PrinterPaperInfo[i];
		if (pItem->m_wPaperType == wPaperSize)
			return pItem;
	}

	return NULL;
}

//===========================================================================
//									Generic parsing functions
//===========================================================================
//--------------------------------------------------------------------------
BOOL ParseShort (Parser& lex, short& shortValue)
{
	int nValue;
	BOOL bOk = lex.ParseInt(nValue);
	shortValue = (short) nValue;
	return bOk;
}


void UnparseShort (Unparser& ofile, const short& shortValue)
{
	int nValue = shortValue;

	ofile.UnparseInt(nValue, FALSE);
}

//--------------------------------------------------------------------------
BOOL CheckIsGreater (CLongArraySorted* array, LONG newElement)
{
	for (int i = 0; i < array->GetSize(); i++)
	{
		if (array->GetAt(i) > newElement)
		{
			return FALSE;
		}
	}
	return TRUE;

}

//===========================================================================
//									PrinterPageInfo class
//===========================================================================

//---------------------------------------------------------------------------
PrinterPageInfo::PrinterPageInfo()
{
	dmPaperSize		= DMPAPER_A4;
	dmPaperWidth	= A4_WIDTH;
	dmPaperLength	= A4_HEIGHT;
	m_bUseCloningPrint = TRUE;
}

//---------------------------------------------------------------------------
//PrinterPageInfo::PrinterPageInfo(const PrinterPageInfo& source)
//{
//	*this = source;
//}

//------------------------------------------------------------------------------
PrinterPageInfo& PrinterPageInfo::operator = (const PrinterPageInfo& source)
{
    dmPaperSize			= source.dmPaperSize;
    dmPaperLength		= source.dmPaperLength;
    dmPaperWidth		= source.dmPaperWidth;
	m_bUseCloningPrint	= source.m_bUseCloningPrint;

	return *this;
}

//------------------------------------------------------------------------------
BOOL PrinterPageInfo::operator == (const PrinterPageInfo& source)
{
	return
			(dmPaperSize		== source.dmPaperSize)		&&
			(dmPaperLength		== source.dmPaperLength)	&&
			(dmPaperWidth		== source.dmPaperWidth)		&&
			(m_bUseCloningPrint == source.m_bUseCloningPrint);
}
//--------------------------------------------------------------------------
BOOL PrinterPageInfo::Parse(Parser& lex)
{
	if (!lex.LookAhead(T_PAGE_PRINTER_INFO))
		return TRUE;
	
	lex.SkipToken();

	//usato solo per parsare a perdere il valore, e' stato unificato l'orientamento tra printerPageInfo e PageInfo (e' rimasto solo quello del PageInfo)
	short dummyOrientation;
	BOOL bOk = 
		lex.ParseOpen	() &&
		ParseShort		(lex, dummyOrientation)	&& lex.ParseComma() &&
		ParseShort		(lex, dmPaperSize)		&& lex.ParseComma() &&
		ParseShort		(lex, dmPaperWidth)		&& lex.ParseComma() &&
		ParseShort		(lex, dmPaperLength)	&& lex.ParseComma() &&
		lex.ParseBool	(m_bUseCloningPrint)	&&
		lex.ParseClose	();

	return bOk;
}

//--------------------------------------------------------------------------
void PrinterPageInfo::Unparse(Unparser& ofile)
{
	ofile.UnparseTag	(T_PAGE_PRINTER_INFO, FALSE);
	ofile.UnparseOpen	();
	UnparseShort		(ofile, 1);	ofile.UnparseComma	();
	UnparseShort		(ofile, dmPaperSize);	ofile.UnparseComma	();
	UnparseShort		(ofile, dmPaperWidth);	ofile.UnparseComma	();
	UnparseShort		(ofile, dmPaperLength); ofile.UnparseComma	();
	ofile.UnparseBool	(m_bUseCloningPrint, FALSE);
	ofile.UnparseClose	();
}

//------------------------------------------------------------------------------
CSize PrinterPageInfo::GetPageSize_LP() const
{
	return CSize
				(
					MUtoLP(dmPaperWidth, CM, MU_SCALE, 3), 
					MUtoLP(dmPaperLength, CM, MU_SCALE, 3)
				);
}

//------------------------------------------------------------------------------
CSize PrinterPageInfo::GetPageSize_mm() const
{
	return CSize (dmPaperWidth / 10, dmPaperLength / 10);
}

//--------------------------------------------------------------------------
#define TO_SHORT(n) short(min(n, 32767))

void PrinterPageInfo::CalculatePageSize(PrinterInfoItem* pInfo, short orientation)
{
	// cattiva selezione nella combo o assenza di info default in LETTER
	if (pInfo == NULL)
	{
		dmPaperSize = DMPAPER_A4;
		dmPaperWidth = (orientation == DMORIENT_PORTRAIT) ? A4_WIDTH : A4_HEIGHT;\
		dmPaperLength = (orientation == DMORIENT_PORTRAIT) ? A4_HEIGHT : A4_WIDTH;
	}
	else
	{
		dmPaperSize = pInfo->m_wPaperType;
		dmPaperWidth = (orientation == DMORIENT_PORTRAIT) ? TO_SHORT(pInfo->m_sPaperSize.cx) : TO_SHORT(pInfo->m_sPaperSize.cy);
		dmPaperLength = (orientation == DMORIENT_PORTRAIT) ? TO_SHORT(pInfo->m_sPaperSize.cy) : TO_SHORT(pInfo->m_sPaperSize.cx);
	}
}

//===========================================================================
//									PageInfo class
//===========================================================================
//
// Wrap DEVMODE struct
//---------------------------------------------------------------------------
PageInfo::PageInfo()
{
	dmFields		= DM_ORIENTATION | DM_PAPERSIZE | DM_SCALE | DM_COPIES | DM_COLLATE;
	dmOrientation	= DMORIENT_PORTRAIT;
	dmPaperSize		= DMPAPER_A4;
	dmScale			= 100; // espressa in centesimi
	dmCopies		= 1;
	dmCollate		= DMCOLLATE_TRUE;
	m_rectMargins	= CRect (0,0,0,0);
	m_bUsePrintableArea = TRUE; // stampa senpre con offset all'area stampabile
	dmPrintQuality	= 1;
	
	m_strPreferredPrinter = _T("");
	
	m_hDevMode = NULL;
	m_hDevNames = NULL;

	// prende di default la size di LETTER
	CalculateSize(NULL);
}

//---------------------------------------------------------------------------
PageInfo::PageInfo(const PageInfo& aPageInfo)
{
	*this = aPageInfo;
}

//---------------------------------------------------------------------------
PageInfo::~PageInfo()
{
	FreePrinterDeviceMode();
	FreePrinterDeviceNames();
}

//---------------------------------------------------------------------------
void PageInfo::FreePrinterDeviceMode()
{
	HANDLE ret = NULL;
	if (m_hDevMode)  
		ret = GlobalFree(m_hDevMode);
}

//---------------------------------------------------------------------------
void PageInfo::FreePrinterDeviceNames()
{	
	HANDLE ret = NULL;
	if (m_hDevNames) 
		ret = GlobalFree(m_hDevNames);
}

//---------------------------------------------------------------------------
BOOL PageInfo::IsDefault()
{
	PageInfo aInfo;
	return *this == aInfo;
}

//------------------------------------------------------------------------------
PageInfo& PageInfo::operator = (const PageInfo& source)
{
    dmFields		= source.dmFields;
    dmOrientation	= source.dmOrientation;
    dmPaperSize		= source.dmPaperSize;
    dmPaperLength	= source.dmPaperLength;
    dmPaperWidth	= source.dmPaperWidth;
    dmScale			= source.dmScale;
    dmCopies		= source.dmCopies;
    dmCollate		= source.dmCollate;
	m_rectMargins	= source.m_rectMargins;
	m_bUsePrintableArea = source.m_bUsePrintableArea;
	dmPrintQuality	= source.dmPrintQuality;

	m_PrinterPageInfo = source.m_PrinterPageInfo;

	m_arHPageSplitter.Copy(source.m_arHPageSplitter);
	//m_arVPageSplitter.Copy(source.m_arVPageSplitter);

	m_strPreferredPrinter = source.m_strPreferredPrinter;

	m_hDevMode = CopyHandle(source.m_hDevMode);
	m_hDevNames = CopyHandle(source.m_hDevNames);

	return *this;
}

// ignora le size minime e massime e l'array con le informazioni di pagina
//------------------------------------------------------------------------------
BOOL PageInfo::operator == (const PageInfo& source)
{
	return
		(dmFields		== source.dmFields)			&&
		(dmOrientation	== source.dmOrientation)	&&
		(dmPaperSize	== source.dmPaperSize)		&&
		(dmPaperLength	== source.dmPaperLength)	&&
		(dmPaperWidth	== source.dmPaperWidth)		&&
		(dmScale		== source.dmScale)			&&
		(dmCopies		== source.dmCopies)			&&
		(dmCollate		== source.dmCollate)		&&
		(m_rectMargins	== source.m_rectMargins)	&&
		(m_bUsePrintableArea == source.m_bUsePrintableArea) &&
		(dmPrintQuality	== source.dmPrintQuality) &&
		(m_PrinterPageInfo == source.m_PrinterPageInfo)&&
		(m_arHPageSplitter == source.m_arHPageSplitter)&&
		//(m_arVPageSplitter == source.m_arVPageSplitter)&&
		(m_strPreferredPrinter == source.m_strPreferredPrinter);
}

//------------------------------------------------------------------------------
BOOL PageInfo::operator != (const PageInfo& aInfo)
{
	return !(*this == aInfo);
}

// devo tornare in punti (device point del video) a partire da decimi di millimetro
//------------------------------------------------------------------------------
CSize PageInfo::GetPageSize_LP() const
{
	return CSize
	(
		MUtoLP(dmPaperWidth, CM, MU_SCALE, 3), 
		MUtoLP(dmPaperLength, CM, MU_SCALE, 3)
	);
}

//------------------------------------------------------------------------------
CSize PageInfo::GetPageSize_mm() const
{
	return CSize (dmPaperWidth / 10, dmPaperLength / 10);
}

//--------------------------------------------------------------------------
void PageInfo::CalculateSize(PrinterInfoItem* pInfo)
{
	// cattiva selezione nella combo o assenza di info default in LETTER
	if (pInfo == NULL)
	{
		dmPaperSize = DMPAPER_A4;
		dmPaperWidth = (dmOrientation == DMORIENT_PORTRAIT) ? A4_WIDTH : A4_HEIGHT;\
		dmPaperLength = (dmOrientation == DMORIENT_PORTRAIT) ? A4_HEIGHT : A4_WIDTH;
	}
	else
	{
		dmPaperSize = pInfo->m_wPaperType;
		dmPaperWidth = (dmOrientation == DMORIENT_PORTRAIT) ? (int)pInfo->m_sPaperSize.cx : (int)pInfo->m_sPaperSize.cy;\
		dmPaperLength = (dmOrientation == DMORIENT_PORTRAIT) ? (int)pInfo->m_sPaperSize.cy : (int)pInfo->m_sPaperSize.cx;
	}
}

//--------------------------------------------------------------------------
void PageInfo::CalculateMargins(CPrintInfo* pInfo /* = NULL*/)
{
	// trucco per costruire un printer DC della stampante di default
	// TODO andrebbe usata stampante preferenziale del report
	CTBPrintDialog dlgPrint(FALSE);
	HDC hdcPrint = NULL;
	CDC dcPrint;
	HGLOBAL hDevMode = NULL;
	HGLOBAL hDevNames = NULL;
	/*if (!dlgPrint)
		return;*/

	if (pInfo == NULL )
	{
		//se ha la preferenziale calcola i margini sulla base della stampante preferenziale (usato per esempio per visualizzare margini in imposta pagina)
		if (!m_strPreferredPrinter.IsEmpty())
		{
			if (GetPrinterDevice(m_strPreferredPrinter.GetString(), &hDevNames, &hDevMode))
			{
				if (dlgPrint.m_pd.hDevMode != NULL)
					GlobalFree(dlgPrint.m_pd.hDevMode);
				dlgPrint.m_pd.hDevMode = hDevMode;
				
				if (dlgPrint.m_pd.hDevNames != NULL)
					GlobalFree(dlgPrint.m_pd.hDevNames);
				dlgPrint.m_pd.hDevNames = hDevNames;
			}
			else
			{
				ASSERT(FALSE);
			}
		}
		else 
		{	
			AfxGetLoginContext()->GetTbDevParams(dlgPrint.m_pd.hDevMode, dlgPrint.m_pd.hDevNames);
		}
	}
	else
	{
		//uso la stampante effetivamente usata in stampa,potrebbe non essere la preferenziale del report
		dlgPrint.m_pd.hDevMode =  pInfo->m_pPD->m_pd.hDevMode;
		dlgPrint.m_pd.hDevNames =  pInfo->m_pPD->m_pd.hDevNames;
	}

	LPDEVMODE lpDevMode = (LPDEVMODE)::GlobalLock(dlgPrint.m_pd.hDevMode);   
	DWORD dwSetFlags = DM_ORIENTATION | DM_COLLATE | DM_COPIES;
					dwSetFlags |= dmPaperSize 
						? DM_PAPERSIZE 
						: (DM_PAPERLENGTH | DM_PAPERWIDTH);
					
	SetPrinterCapability(dwSetFlags, dlgPrint.m_pd.hDevMode);
	hdcPrint = dlgPrint.CreatePrinterDC();

	if (hdcPrint != NULL)
	{
		dcPrint.Attach(hdcPrint);

		// scalo in device point dello schermo
		CPoint ptOffset		= GetPrintableOffset(&dcPrint, TRUE);
		CSize  sizePhisical	= GetPhisicalSize	(&dcPrint, TRUE);
		CSize  sizePrintable= GetPrintableSize	(&dcPrint, TRUE);

		m_rectMargins.top		= ptOffset.y;
		m_rectMargins.left		= ptOffset.x;
		m_rectMargins.bottom	= sizePhisical.cy - sizePrintable.cy - ptOffset.y;
		m_rectMargins.right		= sizePhisical.cx - sizePrintable.cx - ptOffset.x;

		VERIFY(dcPrint.DeleteDC());
	}	
   if (hDevMode != NULL) 
	   GlobalFree(hDevMode);
   if (hDevNames != NULL)
	   GlobalFree(hDevNames);
}

//--------------------------------------------------------------------------
void PageInfo::SetCopies (short nCopies)
{
	dmCopies = nCopies;
}

// parsa la vecchia sintassi tenendo conto che deve passare da pixel in 
// decimi di millimetro come previsto dalla struttura DEVMODE
//--------------------------------------------------------------------------
BOOL PageInfo::OldParse(Parser& lex)
{
	BOOL	bOk = TRUE;
	int		cx,cy;
	
	bOk =
		lex.ParseInt (cx) &&
		lex.ParseInt (cy);

	if (bOk)
	{
		dmOrientation = DMORIENT_PORTRAIT;
		if (lex.LookAhead(T_LANDSCAPE))
		{
			lex.SkipToken();
			dmOrientation = DMORIENT_LANDSCAPE;

			// compatibilita' TaskBuilder rel. 1.1 (valori in pixel video)
			if (cx == 1122 && cy == 794)
			{
				dmPaperSize		= DMPAPER_A4;
				dmPaperWidth	= A4_HEIGHT;
				dmPaperLength	= A4_WIDTH;
	
				return bOk;
			}
		}
	}

	// in decimi di millimetro
	dmPaperSize		= DMPAPER_SPECIAL;
	dmPaperWidth	= (short)LPtoMU(cx, CM, MU_SCALE, MU_DECIMAL);
	dmPaperLength	= (short)LPtoMU(cy, CM, MU_SCALE, MU_DECIMAL);
	
	return bOk;
}

//--------------------------------------------------------------------------
BOOL PageInfo::Parse(Parser& lex)
{
	if (!lex.LookAhead(T_PAGE_INFO))
		return TRUE;
	
	lex.SkipToken();

	// controlla il vecchio formato
	if (lex.LookAhead(T_INT))
		return OldParse(lex);

	BOOL bOk = 
		lex.ParseOpen	() &&
		ParseShort		(lex, dmOrientation)	&& lex.ParseComma() &&
		ParseShort		(lex, dmPaperSize)		&& lex.ParseComma() &&
		ParseShort		(lex, dmPaperWidth)		&& lex.ParseComma() &&
		ParseShort		(lex, dmPaperLength)	&& lex.ParseComma() &&
		ParseShort		(lex, dmScale)			&& lex.ParseComma() &&
		ParseShort		(lex, dmCopies)			&& lex.ParseComma() &&
		ParseShort		(lex, dmCollate)		&& lex.ParseComma() &&
		lex.ParseBool	(m_bUsePrintableArea)	&&
		lex.ParseClose	();

	BOOL marginsParsed = FALSE;
	if (bOk && lex.LookAhead(T_MARGINS))
	{
		int top, left, right, bottom;
		bOk =
			lex.ParseTag        (T_MARGINS) &&
			lex.ParseOpen       ()          &&
			lex.ParseSignedInt  (top)       &&	lex.ParseComma () &&
			lex.ParseSignedInt  (left)      &&	lex.ParseComma () &&
			lex.ParseSignedInt  (bottom)    &&	lex.ParseComma () &&
			lex.ParseSignedInt  (right)     &&
			lex.ParseClose      ();

		m_rectMargins.top = top;
		m_rectMargins.left = left;
		m_rectMargins.right = right;
		m_rectMargins.bottom = bottom;
		marginsParsed = TRUE;
	}
	
	if (bOk && lex.LookAhead(T_PAGE_PRINTER_INFO))
	{
		bOk = m_PrinterPageInfo.Parse(lex);
	}
	else		//if PrinterPageInfo is missing, I suppose Physical page = Logical Page
	{
		m_PrinterPageInfo.dmPaperSize	= dmPaperSize;
		m_PrinterPageInfo.dmPaperWidth	= dmPaperWidth;
		m_PrinterPageInfo.dmPaperLength	= dmPaperLength;
	}

	//importante: prima di calcolare automaticamente i margini deve essere parsato anche il PrinterPageInfo
	if (m_bUsePrintableArea && !marginsParsed)
	{
		CalculateMargins();
	} //else lascia il rettangolo dei margini a (0,0,0,0), usato per le stampe su fincati con layout prestampati


	/*if (bOk && lex.LookAhead(T_PAGE_HSPLITTER))
	{
		bOk = lex.ParseTag(T_PAGE_HSPLITTER) && lex.ParseOpen();
		m_arHPageSplitter.RemoveAll();
		do
		{
			BOOL bCheckSplitter = TRUE;
			short splitter = 0;
			bOk = ParseShort(lex, splitter);
			
			if (bOk && splitter >= m_PrinterPageInfo.dmPaperWidth || splitter < 0)
			{	
				lex.SetError(_TB("Horizontal splitter is out of printer page horizontal size"), _T(""), lex.GetCurrentPos(), lex.GetCurrentLine());
				bCheckSplitter = FALSE;
			}
			if (!CheckIsGreater(&m_arHPageSplitter, splitter))
			{
				lex.SetError(_TB("Horizontal splitter is less than previosly splitter"), _T(""), lex.GetCurrentPos(), lex.GetCurrentLine());	
				bCheckSplitter = FALSE;
			}
			if (bOk && bCheckSplitter)
			{
				m_arHPageSplitter.AddSorted(splitter);
			}

		}while (lex.Matched(T_COMMA) && bOk);
		bOk = bOk && lex.ParseClose      ();
	}

	if (bOk && lex.LookAhead(T_PAGE_VSPLITTER))
	{
		bOk = lex.ParseTag(T_PAGE_VSPLITTER) && lex.ParseOpen();
		m_arVPageSplitter.RemoveAll();
		do
		{
			BOOL bCheckSplitter = TRUE;
			LONG splitter = 0;
			bOk = lex.ParseLong(splitter);
			if (bOk && splitter >= m_PrinterPageInfo.dmPaperLength || splitter < 0)
			{	
				lex.SetError(_TB("Vertical splitter is out of printer page vertical size"), _T(""), lex.GetCurrentPos(), lex.GetCurrentLine());
				bCheckSplitter = FALSE;
			}
			if (!CheckIsGreater(&m_arVPageSplitter, splitter))
			{
				lex.SetError(_TB("Vertical splitter is less than previosly splitter"), _T(""), lex.GetCurrentPos(), lex.GetCurrentLine());	
				bCheckSplitter = FALSE;
			}
			if (bOk && bCheckSplitter)
			{
				m_arVPageSplitter.AddSorted(splitter);
			}
		}while (lex.Matched(T_COMMA) && bOk);
		bOk = bOk && lex.ParseClose();
	}*/
	return bOk;
}


// PageInfo w h Landscape	// vecchia versione
// PageInfo (landscape, style, w, h, scale, copies, collate) Margins(top,left,bottom,right)
//--------------------------------------------------------------------------
void PageInfo::Unparse(Unparser& ofile)
{

	ofile.UnparseTag	(T_PAGE_INFO, FALSE);
	ofile.UnparseOpen	();
	UnparseShort		(ofile, dmOrientation);	ofile.UnparseComma	();
	UnparseShort		(ofile, dmPaperSize);	ofile.UnparseComma	();
	UnparseShort		(ofile, dmPaperWidth);	ofile.UnparseComma	();
	UnparseShort		(ofile, dmPaperLength);	ofile.UnparseComma	();
	UnparseShort		(ofile, dmScale);		ofile.UnparseComma	();
	UnparseShort		(ofile, dmCopies);		ofile.UnparseComma	();
	UnparseShort		(ofile, dmCollate);		ofile.UnparseComma	();
	ofile.UnparseBool	(m_bUsePrintableArea, FALSE);
	ofile.UnparseClose	(FALSE);

	// Parsa i margini solo se ci sono e se devo usarli
	if (!m_rectMargins.IsRectNull() && !m_bUsePrintableArea)
	{
		ofile.UnparseTag      (T_MARGINS, FALSE);
		ofile.UnparseOpen     ();
		ofile.UnparseInt      (m_rectMargins.top,      FALSE);	ofile.UnparseComma    ();
		ofile.UnparseInt      (m_rectMargins.left,     FALSE);	ofile.UnparseComma    ();
		ofile.UnparseInt      (m_rectMargins.bottom,   FALSE);	ofile.UnparseComma    ();
		ofile.UnparseInt      (m_rectMargins.right,    FALSE);
		ofile.UnparseClose    ();
	}

	ofile.UnparseCrLf();
	
	if (
			//inutile m_PrinterPageInfo.dmOrientation	!= dmOrientation ||
			m_PrinterPageInfo.dmPaperSize	!= dmPaperSize ||
			m_PrinterPageInfo.dmPaperWidth	!= dmPaperWidth ||
			m_PrinterPageInfo.dmPaperLength	!= dmPaperLength
		)
		m_PrinterPageInfo.Unparse(ofile);

	/*if (m_arHPageSplitter.GetSize() > 0)
	{
		ofile.UnparseTag	(T_PAGE_HSPLITTER, FALSE);
		ofile.UnparseOpen   ();
		ofile.UnparseInt	(m_arHPageSplitter.GetAt(0), FALSE);
		for(int i = 1; i < m_arHPageSplitter.GetSize(); i++)
		{
			ofile.UnparseComma	();
			ofile.UnparseInt (m_arHPageSplitter.GetAt(i), FALSE);
		}
		ofile.UnparseClose    ();
	}
	
	if (m_arVPageSplitter.GetSize() > 0)
	{
		ofile.UnparseTag	(T_PAGE_VSPLITTER, FALSE);
		ofile.UnparseOpen     ();
		ofile.UnparseInt	(m_arVPageSplitter.GetAt(0), FALSE);
		for(int i = 1; i < m_arVPageSplitter.GetSize(); i++)
		{
			ofile.UnparseComma	();
			ofile.UnparseInt	(m_arVPageSplitter.GetAt(i), FALSE);
		}
		ofile.UnparseClose    ();
	}*/
}

//--------------------------------------------------------------------------
BOOL PageInfo::GetPrinterCapability	()
{ 
	// set page orientation as required by page info
	
	HGLOBAL hg, hNames; 
	AfxGetLoginContext()->GetTbDevParams(hg, hNames); 

	if (m_hDevMode != NULL)
		hg = m_hDevMode;
	else if (!m_strPreferredPrinter.IsEmpty())
	{
		//preferred printer
		HGLOBAL hDevNames;
		if (!GetPrinterDevice(m_strPreferredPrinter.GetString(), &hDevNames, &hg))
			ASSERT(FALSE);
	}

	LPDEVMODE lpDevMode = hg ? (LPDEVMODE)::GlobalLock(hg) : NULL;
	if (lpDevMode)
	{
		dmFields		= lpDevMode->dmFields;	
		dmOrientation	= lpDevMode->dmOrientation;
		m_PrinterPageInfo.dmPaperSize	= lpDevMode->dmPaperSize;
		m_PrinterPageInfo.dmPaperLength	= lpDevMode->dmPaperLength;
		m_PrinterPageInfo.dmPaperWidth	= lpDevMode->dmPaperWidth;
		dmScale			= lpDevMode->dmScale;
		dmCopies		= lpDevMode->dmCopies;
		dmCollate		= lpDevMode->dmCollate;
		dmPrintQuality	= lpDevMode->dmPrintQuality;
	}
				
	if (hg)
		::GlobalUnlock(hg);

	return hg != NULL;
}

//--------------------------------------------------------------------------
BOOL PageInfo::SetPrinterCapability	(DWORD nSetFlags)
{ 
	HGLOBAL hg, hNames;
	AfxGetLoginContext()->GetTbDevParams(hg, hNames); 

	if (m_hDevMode != NULL)
		hg = m_hDevMode;
	else if (!m_strPreferredPrinter.IsEmpty())
	{
		//preferred printer
		HGLOBAL hDevNames;
		if (!GetPrinterDevice(m_strPreferredPrinter.GetString(), &hDevNames, &hg))
			ASSERT(FALSE);
	}

	// set page orientation as required by page info
	LPDEVMODE lpDevMode = (hg != NULL) ? 
						(LPDEVMODE)::GlobalLock(hg) : NULL;
	BOOL bOk = (lpDevMode != NULL);

	if (bOk)
	{
		SET_CAPABILITY2(DM_PAPERSIZE,	dmPaperSize,	m_PrinterPageInfo);
		SET_CAPABILITY2(DM_PAPERLENGTH,	dmPaperLength,	m_PrinterPageInfo);
		SET_CAPABILITY2(DM_PAPERWIDTH,	dmPaperWidth,	m_PrinterPageInfo);
		
		SET_CAPABILITY(DM_ORIENTATION,	dmOrientation);
		SET_CAPABILITY(DM_SCALE,		dmScale);
		SET_CAPABILITY(DM_COPIES,		dmCopies);
		SET_CAPABILITY(DM_COLLATE,		dmCollate);
	}
				
	if (hg)
		::GlobalUnlock(hg);

	return bOk;
}


//--------------------------------------------------------------------------
BOOL PageInfo::SetPrinterCapability	(DWORD nSetFlags, HGLOBAL hDevMode)
{ 
	// set page orientation as required by page info
	LPDEVMODE lpDevMode = (hDevMode != NULL) ? 
						(LPDEVMODE)::GlobalLock(hDevMode) : NULL;
	BOOL bOk = (lpDevMode != NULL);

	if (bOk)
	{
		SET_CAPABILITY2(DM_PAPERSIZE,	dmPaperSize,	m_PrinterPageInfo);
		SET_CAPABILITY2(DM_PAPERLENGTH,	dmPaperLength,	m_PrinterPageInfo);
		SET_CAPABILITY2(DM_PAPERWIDTH,	dmPaperWidth,	m_PrinterPageInfo);
		
		SET_CAPABILITY(DM_ORIENTATION,	dmOrientation);
		SET_CAPABILITY(DM_SCALE,		dmScale);
		SET_CAPABILITY(DM_COPIES,		dmCopies);
		SET_CAPABILITY(DM_COLLATE,		dmCollate);
	}
				
	if (hDevMode)
		::GlobalUnlock(hDevMode);

	return bOk;
}


//--------------------------------------------------------------------------
BOOL PageInfo::UsePrinterPage()
{
	return 				
		GetPrinterPageSize_LP().cx < GetPageSize_LP().cx
		||
		GetPrinterPageSize_LP().cy < GetPageSize_LP().cy;
}

//--------------------------------------------------------------------------
HANDLE PageInfo::GetDevMode()
{
	return m_hDevMode;
}

//--------------------------------------------------------------------------
HANDLE PageInfo::GetDevNames()
{
	return m_hDevNames;
}

//--------------------------------------------------------------------------
void PageInfo::SetDevMode(HANDLE hDevMode)
{ 
	FreePrinterDeviceMode(); 
	m_hDevMode = CopyHandle(hDevMode);
}
	
//--------------------------------------------------------------------------
void PageInfo::SetDevNames(HANDLE hDevNames)
{
	FreePrinterDeviceNames(); 
	m_hDevNames = CopyHandle(hDevNames);
	//TODO Scommentare in debug per la comodita' di avere il nome della stampante
	/*
	if (m_hDevNames != NULL)
	{
		LPDEVNAMES lpDevNames = (LPDEVNAMES)::GlobalLock(m_hDevNames);
		ASSERT(lpDevNames != NULL);
		if (lpDevNames == NULL)
			return;
		CString m_strPrinterName = ((LPCTSTR)lpDevNames + lpDevNames->wDeviceOffset);
	}
	*/
}

//===========================================================================
// CPageSetupDlg dialog
//===========================================================================
IMPLEMENT_DYNAMIC(CPageSetupDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CPageSetupDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CPageSetupDlg)
	ON_CBN_SELCHANGE	(IDC_PAPER_TYPE,		OnSelchangePaperSize)
	ON_EN_KILLFOCUS		(IDC_PAGE_WIDTH,		OnKillfocusPageSize)
	ON_EN_KILLFOCUS		(IDC_PAGE_HEIGHT,		OnKillfocusPageSize)

	ON_CONTROL			(EN_SPIN_RELEASED,	IDC_PAGE_WIDTH,		OnKillfocusPageSize)
	ON_CONTROL			(EN_SPIN_RELEASED,	IDC_PAGE_HEIGHT,	OnKillfocusPageSize)

	ON_BN_CLICKED		(IDC_PORTRAIT,				OnClickedPortrait)
	ON_BN_CLICKED		(IDC_LANDSCAPE,				OnClickedLandscape)
	ON_BN_CLICKED		(IDC_PS_SPECIAL_SIZE,		OnSpecialSize)
	ON_BN_CLICKED		(IDC_PS_USE_PRINTABLE_AREA,	OnUsePrintableArea)

	ON_CBN_SELCHANGE	(IDC_PRINTER_PAPER_TYPE,		OnSelchangePrinterPaperSize)
	ON_BN_CLICKED		(IDC_PS_PRINTER_SPECIAL_SIZE,	OnPrinterSpecialSize)

	ON_EN_KILLFOCUS		(IDC_PRINTER_PAGE_WIDTH,		OnKillfocusPrinterPageSize)
	ON_EN_KILLFOCUS		(IDC_PRINTER_PAGE_HEIGHT,		OnKillfocusPrinterPageSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//---------------------------------------------------------------------------
CPageSetupDlg::CPageSetupDlg(PageInfo& aPageInfo, CWnd* pWndParent)
	:
	CParsedDialog				(IDD_PAGE_SETUP, pWndParent),
	m_PageWidthEdit				(BTN_SPIN_ID),
	m_PageHeightEdit			(BTN_SPIN_ID),
	m_PrinterPageWidthEdit		(BTN_SPIN_ID),
	m_PrinterPageHeightEdit		(BTN_SPIN_ID),
	m_PageInfo					(aPageInfo),
	m_TopMarginEdit				(BTN_SPIN_ID),
	m_BottomMarginEdit			(BTN_SPIN_ID),
	m_LeftMarginEdit			(BTN_SPIN_ID),
	m_RightMarginEdit			(BTN_SPIN_ID),
	m_CopiesEdit				(BTN_SPIN_ID),
	m_PrinterInfo				(aPageInfo.m_strPreferredPrinter)
{                             
}

//---------------------------------------------------------------------------
CPageSetupDlg::CPageSetupDlg(PageInfo& aPageInfo, UINT nIDD, CWnd* pWndParent)
	:
	CParsedDialog				(nIDD, pWndParent),
	m_PageWidthEdit				(BTN_SPIN_ID),
	m_PageHeightEdit			(BTN_SPIN_ID),
	m_PrinterPageWidthEdit		(BTN_SPIN_ID),
	m_PrinterPageHeightEdit		(BTN_SPIN_ID),
	m_PageInfo					(aPageInfo),
	m_TopMarginEdit				(BTN_SPIN_ID),
	m_BottomMarginEdit			(BTN_SPIN_ID),
	m_LeftMarginEdit			(BTN_SPIN_ID),
	m_RightMarginEdit			(BTN_SPIN_ID),
	m_CopiesEdit				(BTN_SPIN_ID),
	m_PrinterInfo				(aPageInfo.m_strPreferredPrinter)
{   
}

//-----------------------------------------------------------------------------
void CPageSetupDlg::ExchangeSize()
{
	int nWidth	= m_PageWidthEdit.GetValue();
	int nHeight = m_PageHeightEdit.GetValue();

	m_PageWidthEdit.SetValue(nHeight);
	m_PageHeightEdit.SetValue(nWidth);
}

//-----------------------------------------------------------------------------
void CPageSetupDlg::ExchangePrinterSize()
{
	int nWidth	= m_PrinterPageWidthEdit.GetValue();
	int nHeight = m_PrinterPageHeightEdit.GetValue();

	m_PrinterPageWidthEdit.SetValue(nHeight);
	m_PrinterPageHeightEdit.SetValue(nWidth);
}

//-----------------------------------------------------------------------------
void CPageSetupDlg::DrawPageSample()
{                          
	m_SampleIcon.SizeToContent
				(
					(double)m_PageWidthEdit.GetValue() /
					(double)m_PageHeightEdit.GetValue()
				);
}

//--------------------------------------------------------------------------
BOOL CPageSetupDlg::OnInitDialog()
{                               
	CParsedDialog::OnInitDialog();

	// margini di stampa espressi in millimetri
	m_TopMarginEdit.	SubclassEdit(IDC_PS_TOPMARGIN,		this);
	m_LeftMarginEdit.	SubclassEdit(IDC_PS_LEFTMARGIN,		this);
	m_BottomMarginEdit.	SubclassEdit(IDC_PS_BOTTOMMARGIN,	this);
	m_RightMarginEdit.	SubclassEdit(IDC_PS_RIGHTMARGIN,	this);

	m_CopiesEdit.		SubclassEdit(IDC_PS_PAGENUM,		this);

	m_TopMarginEdit.	SetMeasureUnits(DEFAULT_SCALING, CM);
	m_LeftMarginEdit.	SetMeasureUnits(DEFAULT_SCALING, CM);
	m_BottomMarginEdit.	SetMeasureUnits(DEFAULT_SCALING, CM);
	m_RightMarginEdit.	SetMeasureUnits(DEFAULT_SCALING, CM);
	
	m_TopMarginEdit.	SetValue(m_PageInfo.m_rectMargins.top);
	m_LeftMarginEdit.	SetValue(m_PageInfo.m_rectMargins.left);
	m_BottomMarginEdit.	SetValue(m_PageInfo.m_rectMargins.bottom);
	m_RightMarginEdit.	SetValue(m_PageInfo.m_rectMargins.right);

	m_CopiesEdit.		SetValue(m_PageInfo.dmCopies);

	m_TopMarginEdit.	SetRange(0, INT_MAX, MARGIN_PRECISION);
	m_BottomMarginEdit.	SetRange(0, INT_MAX, MARGIN_PRECISION);
	m_LeftMarginEdit.	SetRange(0, INT_MAX, MARGIN_PRECISION);
	m_RightMarginEdit.	SetRange(0, INT_MAX, MARGIN_PRECISION);

	m_CopiesEdit.		SetRange(1, INT_MAX);

	// Page PaperSize
	m_PaperSizeCombo.	SubclassDlgItem(IDC_PAPER_TYPE,	this);
	m_PageWidthEdit.	SubclassEdit(IDC_PAGE_WIDTH, this);
//	m_PageWidthEdit.	SetSpecialCaption(IDC_WIDTH_CAPT);
	m_PageHeightEdit.	SubclassEdit(IDC_PAGE_HEIGHT, this);
//	m_PageHeightEdit.	SetSpecialCaption(IDC_HEIGHT_CAPT);

	m_PrinterPaperSizeCombo.	SubclassDlgItem(IDC_PRINTER_PAPER_TYPE,	this);
	m_PrinterPageWidthEdit.		SubclassDlgItem(IDC_PRINTER_PAGE_WIDTH,	this);
	m_PrinterPageHeightEdit.	SubclassDlgItem(IDC_PRINTER_PAGE_HEIGHT, this);

	// set range and precision with 2 decimals and scaling mm (10.)		                              
	m_PageWidthEdit.	SetRange(1, INT_MAX, PAGE_PRECISION);
	m_PageHeightEdit.	SetRange(1, INT_MAX, PAGE_PRECISION);

	// sono memorizzati in decimi di millimetro (vedi DEVMODE help)
	m_PageWidthEdit.	SetValue(MUtoLP(m_PageInfo.dmPaperWidth, CM, MU_SCALE, MU_DECIMAL));
	m_PageHeightEdit.	SetValue(MUtoLP(m_PageInfo.dmPaperLength,CM, MU_SCALE, MU_DECIMAL));

	// set range and precision and value with 2 decimals and scaling mm (10.) for printer page info		                              
	m_PrinterPageWidthEdit.		SetRange(1, INT_MAX, PAGE_PRECISION);
	m_PrinterPageHeightEdit.	SetRange(1, INT_MAX, PAGE_PRECISION);
	m_PrinterPageWidthEdit.		SetValue(MUtoLP(m_PageInfo.m_PrinterPageInfo.dmPaperWidth, CM, MU_SCALE, MU_DECIMAL));
	m_PrinterPageHeightEdit.	SetValue(MUtoLP(m_PageInfo.m_PrinterPageInfo.dmPaperLength,CM, MU_SCALE, MU_DECIMAL));

	// posizionamento LANDSCAPE/PORTRAIT
	m_PortraitRadio.	SubclassDlgItem(IDC_PORTRAIT,		this);
	m_LandscapeRadio.	SubclassDlgItem(IDC_LANDSCAPE,		this);
	m_SampleIcon.		SubclassDlgItem(IDC_PAGE_SAMPLE,	this);

	for (int i = 0; i <= m_PrinterInfo.m_PrinterPaperInfo.GetUpperBound(); i++)
	{
		PrinterInfoItem* pItem = (PrinterInfoItem*) m_PrinterInfo.m_PrinterPaperInfo[i];
		m_PaperSizeCombo.AddString(pItem->m_strPaperSize);
		m_PaperSizeCombo.SetItemDataPtr(i, pItem);
		//fill printerPageInfo combo too
		m_PrinterPaperSizeCombo.AddString(pItem->m_strPaperSize);
		m_PrinterPaperSizeCombo.SetItemDataPtr(i, pItem);
	}

	// abilita la combo degli stili e le dimensioni della pagina
	EnablePageSize();
	EnablePrinterPageSize();

	m_PortraitRadio.	SetCheck(m_PageInfo.dmOrientation  == DMORIENT_PORTRAIT);
	m_LandscapeRadio.	SetCheck(m_PageInfo.dmOrientation  == DMORIENT_LANDSCAPE);
   
	// collating e area di stampa (uso dei margini)
	CheckDlgButton(IDC_PS_COLLATE, m_PageInfo.dmCollate == DMCOLLATE_TRUE);
	CheckDlgButton(IDC_PS_USE_PRINTABLE_AREA, m_PageInfo.m_bUsePrintableArea);

	// setta i margini di default se si usa l'area di stampa della printer corrente
	SetDefaultMargins();

	// initialize the sample icon control
	CRect rect;
	GetDlgItem(IDC_PAGE_SAMPLE)->GetWindowRect(rect);
	ScreenToClient(rect);

	// carica il bitmap opportuno
	((CStatic*) GetDlgItem(IDC_ORIENTATION))->SetIcon(LoadWalkIcon
		((m_PageInfo.dmOrientation  == DMORIENT_PORTRAIT) ? IDI_PORTRAIT : IDI_LANDSCAPE));

	m_SampleIcon.SetRefRect(rect.TopLeft(), rect.Size());

	DrawPageSample();
	
	// evidenzia nel titolo la stampante usata
	SetWindowText(m_PrinterInfo.m_strPrinterName + _TB(" on ") + m_PrinterInfo.m_strPortName);

	return TRUE;
}

//--------------------------------------------------------------------------
void CPageSetupDlg::OnOK()
{   
	if (!CheckForm())
		return;
	
	// An.18499
	if (m_PageInfo.m_bUsePrintableArea)
		m_PageInfo.CalculateMargins();
	else
	{
		m_PageInfo.m_rectMargins.top	= m_TopMarginEdit.		GetValue();
		m_PageInfo.m_rectMargins.left	= m_LeftMarginEdit.		GetValue();
		m_PageInfo.m_rectMargins.bottom	= m_BottomMarginEdit.	GetValue();
		m_PageInfo.m_rectMargins.right	= m_RightMarginEdit.	GetValue();
	}

	m_PageInfo.dmCopies				= m_CopiesEdit.			GetValue();

	m_PageInfo.dmOrientation		= IsDlgButtonChecked(IDC_LANDSCAPE) ? DMORIENT_LANDSCAPE : DMORIENT_PORTRAIT;
	m_PageInfo.dmCollate			= IsDlgButtonChecked(IDC_PS_COLLATE) ? DMCOLLATE_TRUE : DMCOLLATE_FALSE;
	m_PageInfo.m_bUsePrintableArea	= IsDlgButtonChecked(IDC_PS_USE_PRINTABLE_AREA);

	SetPageSize();
	SetPrinterPageSize();

	// Si deve chiamare la OnOK di CDialog poiche` quella di CParsedDialog
	// rifarebbe la CheckForm
	__super::OnOK();
}

//--------------------------------------------------------------------------
void CPageSetupDlg::OnSelchangePaperSize()
{
	int nPos = m_PaperSizeCombo.GetCurSel();

	// calcola la size della pagina sulla base della PaperSize selezionata
	m_PageInfo.CalculateSize(nPos == CB_ERR ? NULL : (PrinterInfoItem*)m_PaperSizeCombo.GetItemDataPtr(nPos));

	// sono memorizzati in decimi di millimetro (vedi DEVMODE help)
	m_PageWidthEdit.	SetValue(MUtoLP(m_PageInfo.dmPaperWidth, CM, MU_SCALE, MU_DECIMAL));
	m_PageHeightEdit.	SetValue(MUtoLP(m_PageInfo.dmPaperLength,CM, MU_SCALE, MU_DECIMAL));

	DrawPageSample();
}

//--------------------------------------------------------------------------
void CPageSetupDlg::OnSelchangePrinterPaperSize()
{
	int nPos = m_PrinterPaperSizeCombo.GetCurSel();

	// calcola la size della pagina sulla base della PaperSize selezionata
	m_PageInfo.m_PrinterPageInfo.CalculatePageSize(nPos == CB_ERR ? NULL : (PrinterInfoItem*)m_PrinterPaperSizeCombo.GetItemDataPtr(nPos), m_PageInfo.dmOrientation);

	// sono memorizzati in decimi di millimetro (vedi DEVMODE help)
	m_PrinterPageWidthEdit.		SetValue(MUtoLP(m_PageInfo.m_PrinterPageInfo.dmPaperWidth, CM, MU_SCALE, MU_DECIMAL));
	m_PrinterPageHeightEdit.	SetValue(MUtoLP(m_PageInfo.m_PrinterPageInfo.dmPaperLength,CM, MU_SCALE, MU_DECIMAL));

}

//--------------------------------------------------------------------------
void CPageSetupDlg::OnKillfocusPageSize()
{
	m_PageInfo.dmPaperWidth		= (short) LPtoMU(m_PageWidthEdit.GetValue(),  CM, MU_SCALE, MU_DECIMAL);
	m_PageInfo.dmPaperLength	= (short) LPtoMU(m_PageHeightEdit.GetValue(), CM, MU_SCALE, MU_DECIMAL);
    
	DrawPageSample();
}

//--------------------------------------------------------------------------
void CPageSetupDlg::OnKillfocusPrinterPageSize()
{
	m_PageInfo.m_PrinterPageInfo.dmPaperWidth	= (short) LPtoMU(m_PrinterPageWidthEdit.GetValue(),  CM, MU_SCALE, MU_DECIMAL);
	m_PageInfo.m_PrinterPageInfo.dmPaperLength	= (short) LPtoMU(m_PrinterPageHeightEdit.GetValue(), CM, MU_SCALE, MU_DECIMAL);
}

//--------------------------------------------------------------------------
void CPageSetupDlg::OnClickedPortrait()
{   
	if (m_PageInfo.dmOrientation == DMORIENT_PORTRAIT)
		return;

	m_PageInfo.dmOrientation = DMORIENT_PORTRAIT;
	ExchangeSize(); 
	ExchangePrinterSize(); 

	((CStatic*) GetDlgItem(IDC_ORIENTATION))->SetIcon(LoadWalkIcon(IDI_PORTRAIT));
	DrawPageSample();
}

//--------------------------------------------------------------------------
void CPageSetupDlg::OnClickedLandscape()
{
	if (m_PageInfo.dmOrientation == DMORIENT_LANDSCAPE)
		return;

	m_PageInfo.dmOrientation = DMORIENT_LANDSCAPE;

	ExchangeSize(); 
	ExchangePrinterSize(); 

	((CStatic*) GetDlgItem(IDC_ORIENTATION))->SetIcon(LoadWalkIcon(IDI_LANDSCAPE));
	DrawPageSample();
}

//--------------------------------------------------------------------------
void CPageSetupDlg::OnSpecialSize()
{
	SetPageSize();
	EnablePageSize();
}

//--------------------------------------------------------------------------
void CPageSetupDlg::OnPrinterSpecialSize()
{
	SetPrinterPageSize();
	EnablePrinterPageSize();
}
//--------------------------------------------------------------------------
void CPageSetupDlg::OnUsePrintableArea()
{
	m_PageInfo.m_bUsePrintableArea = !m_PageInfo.m_bUsePrintableArea;
	SetDefaultMargins();
}

//--------------------------------------------------------------------------
void CPageSetupDlg::SetDefaultMargins()
{
	BOOL bUsePrintableArea = m_PageInfo.m_bUsePrintableArea;
	//disabilita/abilita la finestra dei margini perche' sono letti da stampante
	m_TopMarginEdit.EnableWindow(!bUsePrintableArea);
	m_LeftMarginEdit.EnableWindow(!bUsePrintableArea);
	m_BottomMarginEdit.EnableWindow(!bUsePrintableArea);
	m_RightMarginEdit.EnableWindow(!bUsePrintableArea);
	
	if (bUsePrintableArea)
	{
		m_PageInfo.m_rectMargins.top	= 0;
		m_PageInfo.m_rectMargins.bottom	= 0;
		m_PageInfo.m_rectMargins.left	= 0;
		m_PageInfo.m_rectMargins.right	= 0;

		m_PageInfo.CalculateMargins();

		m_TopMarginEdit.	SetValue(m_PageInfo.m_rectMargins.top);
		m_LeftMarginEdit.	SetValue(m_PageInfo.m_rectMargins.left);
		m_BottomMarginEdit.	SetValue(m_PageInfo.m_rectMargins.bottom);
		m_RightMarginEdit.	SetValue(m_PageInfo.m_rectMargins.right);
	}
}

//--------------------------------------------------------------------------
void CPageSetupDlg::SetPageSize()
{                                                  
	// occorre tornare dall'input in CM ai decimi di millimetro (DEVMODE required)
	if (IsDlgButtonChecked(IDC_PS_SPECIAL_SIZE))
	{
		m_PageInfo.dmPaperSize		= DMPAPER_SPECIAL;  // significa dimensioni speciali
		m_PageInfo.dmPaperWidth		= (short) LPtoMU(m_PageWidthEdit.GetValue(),  CM, MU_SCALE, MU_DECIMAL);
		m_PageInfo.dmPaperLength	= (short) LPtoMU(m_PageHeightEdit.GetValue(), CM, MU_SCALE, MU_DECIMAL);
	}
	else
	{
		// senza selezione di default vado in LETTER
		int nPos = m_PaperSizeCombo.GetCurSel();
		m_PageInfo.CalculateSize(nPos == CB_ERR ? NULL : (PrinterInfoItem*)m_PaperSizeCombo.GetItemDataPtr(nPos));

		// sono memorizzati in decimi di millimetro (vedi DEVMODE help)
		m_PageWidthEdit.	SetValue(MUtoLP(m_PageInfo.dmPaperWidth, CM, MU_SCALE, MU_DECIMAL));
		m_PageHeightEdit.	SetValue(MUtoLP(m_PageInfo.dmPaperLength,CM, MU_SCALE, MU_DECIMAL));
	}
}

//--------------------------------------------------------------------------
void CPageSetupDlg::SetPrinterPageSize()
{                                                  
	// occorre tornare dall'input in CM ai decimi di millimetro (DEVMODE required)
	if (IsDlgButtonChecked(IDC_PS_PRINTER_SPECIAL_SIZE))
	{
		m_PageInfo.m_PrinterPageInfo.dmPaperSize		= DMPAPER_SPECIAL;  // significa dimensioni speciali
		m_PageInfo.m_PrinterPageInfo.dmPaperWidth	= (short) LPtoMU(m_PrinterPageWidthEdit.GetValue(),  CM, MU_SCALE, MU_DECIMAL);
		m_PageInfo.m_PrinterPageInfo.dmPaperLength	= (short) LPtoMU(m_PrinterPageHeightEdit.GetValue(), CM, MU_SCALE, MU_DECIMAL);
	}
	else
	{
		// senza selezione di default vado in LETTER
		int nPos = m_PrinterPaperSizeCombo.GetCurSel();
		m_PageInfo.m_PrinterPageInfo.CalculatePageSize(nPos == CB_ERR ? NULL : (PrinterInfoItem*)m_PrinterPaperSizeCombo.GetItemDataPtr(nPos), m_PageInfo.dmOrientation);

		// sono memorizzati in decimi di millimetro (vedi DEVMODE help)
		m_PrinterPageWidthEdit.		SetValue(MUtoLP(m_PageInfo.m_PrinterPageInfo.dmPaperWidth, CM, MU_SCALE, MU_DECIMAL));
		m_PrinterPageHeightEdit.	SetValue(MUtoLP(m_PageInfo.m_PrinterPageInfo.dmPaperLength,CM, MU_SCALE, MU_DECIMAL));
	}
}

//--------------------------------------------------------------------------
void CPageSetupDlg::EnablePageSize()
{                                                  
	if (m_PageInfo.dmPaperSize == DMPAPER_SPECIAL)
	{
		CheckDlgButton(IDC_PS_SPECIAL_SIZE, TRUE);
		m_PaperSizeCombo.	EnableWindow(FALSE);
		m_PageWidthEdit.	EnableWindow(TRUE);
		m_PageHeightEdit.	EnableWindow(TRUE);
	}
	else
	{
		CheckDlgButton(IDC_PS_SPECIAL_SIZE, FALSE);
		m_PaperSizeCombo.	EnableWindow(TRUE);
		m_PageWidthEdit.	EnableWindow(FALSE);
		m_PageHeightEdit.	EnableWindow(FALSE);
		m_PaperSizeCombo.	SetCurSel(m_PrinterInfo.GetPrinterInfoItem(m_PageInfo.dmPaperSize));
	}
}

//--------------------------------------------------------------------------
void CPageSetupDlg::EnablePrinterPageSize()
{                                                  
	if (m_PageInfo.m_PrinterPageInfo.dmPaperSize == DMPAPER_SPECIAL)
	{
		CheckDlgButton(IDC_PS_PRINTER_SPECIAL_SIZE, TRUE);
		m_PrinterPaperSizeCombo.	EnableWindow(FALSE);
		m_PrinterPageWidthEdit.		EnableWindow(TRUE);
		m_PrinterPageHeightEdit.	EnableWindow(TRUE);
	}
	else
	{
		CheckDlgButton(IDC_PS_PRINTER_SPECIAL_SIZE, FALSE);
		m_PrinterPaperSizeCombo.	EnableWindow(TRUE);
		m_PrinterPageWidthEdit.		EnableWindow(FALSE);
		m_PrinterPageHeightEdit.	EnableWindow(FALSE);
		m_PrinterPaperSizeCombo.	SetCurSel(m_PrinterInfo.GetPrinterInfoItem(m_PageInfo.m_PrinterPageInfo.dmPaperSize));
	}
}


