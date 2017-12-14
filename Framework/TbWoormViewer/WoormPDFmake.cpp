#include "stdafx.h"

#include "afxole.h"
#include <TbGeneric\ParametersSections.h>
#include <TbGenlibManaged\EmfToPdf.h>

#include "TBPrintDialog.h"
#include "WOORMFRM.H"
#include "WoormPdfMake.h"
#include "COLUMN.H"
#include "ExpExter.h"

//includere come ultimo include all'inizio del cpp
#include "beginh.dex"

//-----------------------------------------------------------------------------
CWoormPDFmake::CWoormPDFmake(CWoormDocMng* pWDoc)
	:
	m_pDocument(pWDoc)
{
	ASSERT(pWDoc);
	CWoormFrame* pWoormFrame = pWDoc->GetWoormFrame();
	ASSERT(pWoormFrame);
	m_pDC = pWoormFrame->GetDC();
}

//-----------------------------------------------------------------------------
void CWoormPDFmake::Render(UINT nStartPage, UINT nEndPage, UINT nCopies, LPCTSTR pszFileName, LPCTSTR pszPassword, CString sPrinterTemplate)
{
	m_rPdf.NewPDF(pszFileName);

	//recupero la dimensione dei blocchi di pagine in cui splittare la creazione del pdf
	UINT nBlockSize = 1000;
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

	CPdfDocumentWrapper* pdfDocument = NULL;
	CStringArray arPdfNames;

	// Instantiate a CPrintDialog object.
	CTBPrintDialog dlgPrint(FALSE);

	//tento di recuperare un device context dalla stampante di default, se non ci riesco
	//uso il device context dello schermo
	CWinApp* app = AfxGetApp();
	bool bPrinterDC = false;
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

	GetDocument()->SplitCurrentLayout();
	GetDocument()->SetFirstSplitterPage();

	CPrintInfo printInfo;
	printInfo.SetMinPage(nStartPage);
	printInfo.SetMaxPage(nEndPage);
	printInfo.m_bDirect = TRUE;

	OnBeginPrinting(&printInfo);

	// begin page printing loop
	for (printInfo.m_nCurPage = nStartPage; printInfo.m_nCurPage <= nEndPage; printInfo.m_nCurPage++)
	{
		int currNRotateAngle = nRotateAngle;
		//ogni 10 iterazioni rilascio la coda dei messaggi per non bloccare l'applicazione
		if ((printInfo.m_nCurPage % 10) == 0)
			CTBWinThread::PumpThreadMessages();

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
			pdfDocument = new CPdfDocumentWrapper();
		}

		ASSERT(pdfDocument != NULL);

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

		//OnPrepareDC(&memDC, &printInfo); --- Da vedere come rivedere 

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

		OnPrint(&printInfo); //si occupa anche di CAMBIARE PAGINA
									 //---- ----------
		//HENHMETAFILE hMeta = memDC.CloseEnhanced(); //TODO: da vedere 

		//TRACE(cwsprintf(_T("\nAddPageFromMetafile: %f (%d,%d) %d (%d, %d) [%d,%d,%d,%d]"), 
		//					m_dScaleFactor,
		//					dx, dy,
		//					nRotateAngle,
		//					csPrtPage.cx, csPrtPage.cy,
		//					marginRect.left, marginRect.top, marginRect.right, marginRect.bottom
		//					));

		/*pdfDocument->AddPageFromMetafile(
			hMeta,
			marginRect,
			bPrinterDC,
			m_dScaleFactor,
			originalMarginX,
			currNRotateAngle,
			csPrtPage.cx, csPrtPage.cy,
			currbInvertedOrientation
		);

		::DeleteEnhMetaFile(hMeta);*/
	}

	OnEndPrinting(&printInfo);    // clean up after printing

	// Save PDF 
	if (arPdfNames.IsEmpty())
	{
		m_rPdf.SavePdf();
		//pdfDocument->Save(pszFileName, nCopies, pszPassword);
		//SAFE_DELETE(pdfDocument);
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

	if (pSavePageInfo)
	{
		GetDocument()->m_PageInfo = *pSavePageInfo;
		SAFE_DELETE(pSavePageInfo);
	}

	GetDocument()->m_bIsPrinting = dbSaveIsPrintingState;
	m_bPDFPrinting = FALSE;
}

//------------------------------------------------------------------------------
void CWoormPDFmake::OnBeginPrinting(CPrintInfo* pInfo)
{
	if (GetDocument()->m_pRDEmanager)
		pInfo->m_nCurPage = GetDocument()->m_pRDEmanager->CurrPageRead() + 1;

	SymField* pF = GetDocument()->m_ViewSymbolTable.GetFieldByID(REPORT_ISPRINTING_ID);
	if (pF)
		pF->AssignData(DataBool(TRUE));

	int pageIndex = m_nFromPage - 1;

	if (!pInfo->m_bPreview && pageIndex <= GetDocument()->m_pRDEmanager->LastPage())
		GetDocument()->ReadSelectedPage(pageIndex);

	GetDocument()->CalcSplittedPageMaxWidth();

	m_bDoScale = FALSE;
	m_scaleType = NONE;
	m_dScaleFactor = m_dLowerScale = 1.0;
	m_bCalculatedScale = FALSE;
	m_ptOffsetOrgAfterScaling = CPoint(0, 0);

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
CSize CWoormPDFmake::GetPrinterPageSizeWithoutMargin_LP()
{
	PageInfo& info = GetDocument()->m_PageInfo;
	CSize ptPrtSize(info.GetPrinterPageSize_LP());
	if (!GetDocument()->GetObjects().m_bInvertOrientation)
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
CSize CWoormPDFmake::GetPrinterFullPageSize_mm()
{
	PageInfo& info = GetDocument()->m_PageInfo;
	return info.GetPrinterPageSize_mm();
}

//------------------------------------------------------------------------------
void CWoormPDFmake::OnEndPrinting(CPrintInfo* pInfo)
{
	SymField* pF = GetDocument()->m_ViewSymbolTable.GetFieldByID(REPORT_ISPRINTING_ID);
	if (pF)
		pF->AssignData(DataBool(FALSE));

	if (GetDocument()->GetNumberOfSplittedPage())
		GetDocument()->RestoreCurrentLayout();

	if (GetDocument()->IsRunningFromExternalController())
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
void CWoormPDFmake::OnPrint(CPrintInfo* pInfo)
{
	CPoint ptOffset;//GetPrintableOffset(pDC);
	ptOffset.x = 0; ptOffset.y = 0;

	CPoint ptMargins = GetDocument()->m_PageInfo.m_rectMargins.TopLeft();
	//ScalePoint(ptMargins, *pDC);

	//Ignora i margini e stampa sempre con 0,0 posizionato al vertice dell'area stampabile
	/*if (!GetDocument()->m_PageInfo.m_bUsePrintableArea && !m_bDoScale)
		pDC->SetViewportOrg(ptMargins.x - ptOffset.x, ptMargins.y - ptOffset.y);*/

	//se serve, ruoto la pagina con una trasformazione (SE STO STAMPANDO UN PDF NON POSSO USARE LA SetWorldTransform e ruoto direttamente in fase di creazione del pdf)
	if (GetDocument()->GetObjects().m_bInvertOrientation && m_bPDFPrinting == FALSE)
	{
		//Save DC
		/*int iSaved = pDC->SaveDC();*/
		/*CFont *pOldFont = pDC->GetCurrentFont();*/

		double pi = 3.14159265358979323846264338328;
		double radian = ((double)2.0 * pi * (double)-90.00) / (double)360.0; //Rotate -90 degrees
		float cosine = (float)cos(radian);
		float sine = (float)sin(radian);

		CSize size(0, 0);
		size.cx = 1024; //pDC->GetDeviceCaps(HORZRES);
		size.cy = 764;  //pDC->GetDeviceCaps(VERTRES);

		XFORM xForm;
		xForm.eM11 = cosine;
		xForm.eM12 = sine;
		xForm.eM21 = -sine;
		xForm.eM22 = cosine;

		xForm.eDx = 0;
		xForm.eDy = (float)(size.cy / m_dScaleFactor);

		//pDC->SetGraphicsMode(GM_ADVANCED);
		//SetWorldTransform(pDC->m_hDC, &xForm);

		/*LOGFONT lgf;
		pDC->GetCurrentFont()->GetLogFont(&lgf);
		lgf.lfOrientation -= lgf.lfEscapement;
		lgf.lfEscapement = 0;
		CFont horFont;
		horFont.CreateFontIndirect(&lgf);*/
		/*pOldFont = pDC->SelectObject(&horFont);*/

		DoDraw(pInfo);

		//Restore DC
		/*pDC->RestoreDC(iSaved);*/
	}

	else
		DoDraw(pInfo);

	// disegna i margini della stampante in preview
	if (pInfo && pInfo->m_bPreview)
		DrawMargins(pInfo);

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

//------------------------------------------------------------------------------------------
void CWoormPDFmake::DrawRound(CRect rect, INT radio, const BorderPen& borderPen, const Borders& borders, COLORREF* pRgbBkgColor, BOOL bIsTransparent)
{
	m_rPdf.DrawRoundedRectangle(rect.left, rect.top, rect.right, rect.bottom, borderPen.m_nWidth, radio, borderPen.m_rgbColor, *pRgbBkgColor, !bIsTransparent);
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::DrawBorders(CRect rect, const BorderPen& borderPen, const Borders& borders, COLORREF* pRgbBkgColor, BOOL bIsTransparent)
{
	if (borders.top && borders.right && borders.bottom && borders.left)
	{
		m_rPdf.DrawRectangle(rect.left, rect.top, rect.right, rect.bottom, borderPen.m_nWidth, borderPen.m_rgbColor, *pRgbBkgColor, !bIsTransparent);
		return;
	}

	INT iLeft = rect.left + (borders.left ? borderPen.m_nWidth : 0);
	INT iTop	= rect.top + (borders.top ? borderPen.m_nWidth : 0);
	INT iRight = rect.right;
	INT iBottom = rect.bottom;
		
	m_rPdf.FillRectangle(
		iLeft	+  borderPen.m_nWidth ,
		iTop	+  borderPen.m_nWidth ,
		iRight  ,
		iBottom + (borders.bottom ? 0 : borderPen.m_nWidth),
		*pRgbBkgColor);
		
	if (borders.top)
		m_rPdf.DrawLine(iLeft, iTop, iRight, iTop, borderPen.m_nWidth, borderPen.m_rgbColor);
	if (borders.right)
		m_rPdf.DrawLine(iRight, iTop, iRight, iBottom, borderPen.m_nWidth, borderPen.m_rgbColor);
	if (borders.bottom)
		m_rPdf.DrawLine(iLeft, iBottom, iRight, iBottom, borderPen.m_nWidth, borderPen.m_rgbColor);
	if (borders.left)
		m_rPdf.DrawLine(iLeft, iTop, iLeft, iBottom, borderPen.m_nWidth, borderPen.m_rgbColor);


}

//------------------------------------------------------------------------------------------
INT CWoormPDFmake::FontPoinSize(LOGFONT lf)
{
	ASSERT(m_pDC);
	INT pointSize = (INT)(MulDiv(lf.lfHeight, 72, GetDeviceCaps(m_pDC->m_hDC, LOGPIXELSY)));
	pointSize = pointSize >= 0 ? pointSize : pointSize * -1;
	return pointSize;
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::SqrRectPDF(BaseObj* pObj)
{
	ASSERT(GetDocument());
	if (GetDocument() == NULL)
		return;
	SqrRect* pSqrRect = ((SqrRect*)pObj);

	COLORREF	crRgbBkgColor	= pSqrRect->GetCurrBkgColor();
	BOOL		bTrasparent		= pSqrRect->IsTransparent();
	CRect		cBaseRect		= pSqrRect->GetBaseRect();

	if (pSqrRect->IsRoundedBox())
		DrawRound(cBaseRect, pSqrRect->GetRatio(), pSqrRect->GetBorderPen(), pSqrRect->GetBorders(), &crRgbBkgColor, bTrasparent);
	else
		DrawBorders(cBaseRect, pSqrRect->GetBorderPen(), pSqrRect->GetBorders(), &crRgbBkgColor, bTrasparent);
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::GraphRectPDF(BaseObj* pObj)
{
	ASSERT(GetDocument());
	if (GetDocument() == NULL)
		return;
	GraphRect* pGraphRect = ((GraphRect*)pObj);

	COLORREF	crRgbBkgColor	= pGraphRect->GetCurrBkgColor();
	BOOL		bIsTrasparent	= pGraphRect->IsTransparent();
	CRect		cBaseRect		= pGraphRect->GetBaseRect();
	CTBPicture* pBitmap			= pGraphRect->GetBitmap();

	DrawBorders(cBaseRect, pGraphRect->GetBorderPen(), pGraphRect->GetBorders(), &crRgbBkgColor, bIsTrasparent);

	if (pBitmap == NULL || !pBitmap->IsOk())
		return;

	CRect rectCuttedHereToFit;
	BOOL bHaveToCutted = FALSE;
	CRect rectIamge = pGraphRect->CalculateBitmapWithFitMode(rectCuttedHereToFit, bHaveToCutted);
	HBITMAP hBitmap = pBitmap->GetHBitmap(bIsTrasparent ? NULL : &crRgbBkgColor);
	ASSERT(hBitmap);
	m_rPdf.DrawImage(hBitmap, rectIamge.left, rectIamge.top, rectIamge.right, rectIamge.bottom);
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::FieldRectPDF(BaseObj* pObj)
{
	ASSERT(GetDocument());
	if (GetDocument() == NULL)
		return;
	FieldRect* pFieldRect = ((FieldRect*)pObj);

	Value _Value(pFieldRect->m_Value);
	Label _Label(pFieldRect->m_Label);

	pFieldRect->EvalExpressions(_Value, _Label);

	CRect cBaseRect = pFieldRect->GetBaseRect();

	CRect rect = pObj->InsideRect(cBaseRect, pFieldRect->GetBorderPen(), pFieldRect->GetBorders(), FALSE);
	CSize cs(NormalizeRect(pFieldRect->GetRatio(), cBaseRect), NormalizeRect(pFieldRect->GetRatio(), cBaseRect, FALSE));
	rect.InflateRect(-cs.cx, -cs.cy);

	BOOL	 bIsTrasparent = pFieldRect->IsTransparent();

	COLORREF crRgbBkgColor = _Value.GetBkgColor();		// rgb Bkg Color

	if (pFieldRect->IsRoundedBox())
		DrawRound(cBaseRect, pFieldRect->GetRatio(), pFieldRect->GetBorderPen(), pFieldRect->GetBorders(), &crRgbBkgColor, bIsTrasparent);
	else
		DrawBorders(cBaseRect, pFieldRect->GetBorderPen(), pFieldRect->GetBorders(), &crRgbBkgColor, bIsTrasparent);

	//----------------

	switch (pFieldRect->m_ShowAs)
	{
		case FT_IMAGE:
		{
			//DrawBitmap(DC, inside);
			break;
		}
		case FT_BARCODE:
		{
			//DrawBarCode(DC, inside, bPreview);
			break;
		}
		case FT_URL:
		{
			//DrawUrlFile(DC, inside);
			break;
		}
		case FT_TEXTFILE:
		default:
		{
			/* TODO
			CString  stLabelText	= pFieldRect->m_Label.GetText();
			AlignType stLabelAlign	= pFieldRect->m_Label.GetAlign();

			CString  stValueText	= pFieldRect->m_Value.GetText();
			AlignType stValueAlign	= pFieldRect->m_Value.GetAlign();

			// must Draw m_Value first in opaque mode (i.e. fill rectangle)
			CString sLabetText = _Label.GetLocalizedText(m_pDocument);
			CString sValueText = _Value.GetText(); // ?? perchè ? GetLocalizedText(m_pDocument);
			if
			(
				AfxGetFontStyleTable()->GetUseVCenterBottomAlignInWoormFields() &&
				(_Value.m_nAlign & DT_EX_VCENTER_LABEL) == DT_EX_VCENTER_LABEL &&
				!sLabetText.IsEmpty() &&
				!sValueText.IsEmpty() &&
				(_Value.GetAlign() & DT_VCENTER) == DT_VCENTER
			)
			{
				//GetText ritorna logical unit
				CSize csLabel = GetTextSize(&DC, sLabetText);
				CSize csValue = GetTextSize(&DC, sValueText);
				//DEVO  fare l' unscale dei due CPoint
				UnScaleSize(csLabel, DC);
				UnScaleSize(csValue, DC);

				CRect rectValue (inside);
				int h = inside.Height();
				if ((csLabel.cy + csValue.cy) <= h)
				rectValue.top += csLabel.cy;
				else
				rectValue.top = max (rectValue.top, rectValue.bottom - csValue.cy); //si sovrappongono: simulo il DT_BOTTOM

				if (!m_bTransparent)
				{
					CRect rect (inside);
					if (DC.IsPrinting())
						ScaleRect(rect, DC);

					CBrush brush;
					brush.CreateSolidBrush(_Value.GetBkgColor());
					CBrush*	old_brush = DC.SelectObject(&brush);

					DC.FillRect(rect, &brush);
					DC.SelectObject(old_brush);
				}

				_Value.Draw(DC, rectValue, m_pDocument, !m_bTransparent, flags, CSize(0,0), m_bMiniHtml);
				_Label.Draw(DC, inside, m_pDocument, FALSE, 0, cs);
			}
			*/

			GenericTextPDF (&_Label, rect, TRUE,			cs);
			GenericTextPDF (&_Value, rect, bIsTrasparent,	cs);
		}
	}
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::GenericTextPDF(GenericText* pGenText, const CRect& rectText, BOOL bIsTransparent, const CSize& cs)
{
	ASSERT_VALID(pGenText);
 
	LOGFONT		lf;  ::memset(&lf, 0, sizeof lf);
	const FontStyle* pFontStyle = GetDocument()->m_pFontStyles->GetFontStyle(pGenText->GetFontIdx(), &(GetDocument()->GetNamespace()));
	ASSERT(pFontStyle);
	lf = pFontStyle->GetLogFont();

	if (pFontStyle && !pGenText->GetText().IsEmpty())
	{
		COLORREF crRgbTextColor = pGenText->GetTextColor();
		if (crRgbTextColor == DEFAULT_TEXTCOLOR) 
			crRgbTextColor = pFontStyle->GetColor();

		ASSERT_VALID(m_pDC);
		m_rPdf.SetColor(GetRValue(crRgbTextColor), GetGValue(crRgbTextColor), GetBValue(crRgbTextColor));

		pGenText->DrawGText
		(
			*m_pDC,
			rectText,
			GetDocument(),
			!bIsTransparent,
			0,	// NORMAL 
			cs,
			TRUE	//  CALC_RECT 
		);

		if (pGenText->m_PreDraw.m_arTextLine.GetCount() == 0)
		{
			m_rPdf.PdfText
			(
				pGenText->m_PreDraw.m_rect,
				pGenText->m_PreDraw.m_strText,
				pGenText->m_PreDraw.m_nAlign,
				FontPoinSize(lf), lf.lfFaceName, lf.lfWeight == FW_BOLD, lf.lfItalic
			);
		}
		else
		{
			for (int i = 0; i < pGenText->m_PreDraw.m_arTextLine.GetCount(); i++)
			{
				m_rPdf.PdfText
				(
					pGenText->m_PreDraw.m_arRectLine[i],
					pGenText->m_PreDraw.m_arTextLine[i],
					pGenText->m_PreDraw.m_nAlign,
					FontPoinSize(lf), lf.lfFaceName, lf.lfWeight == FW_BOLD, lf.lfItalic
				);
			}
		}
	}
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::TextRectPDF(BaseObj* pObj)
{
	ASSERT(GetDocument());
	if (GetDocument() == NULL)
		return;
	
	TextRect* pTRect = ((TextRect*)pObj);
	CRect cBaseRect = pTRect->GetBaseRect();

	DataLng color;
	if (pTRect->m_pTextColorExpr && pTRect->m_pTextColorExpr->Eval(color))
		pTRect->m_StaticText.SetTextColor((COLORREF)(long)color);
	if (pTRect->m_pBkgColorExpr && pTRect->m_pBkgColorExpr->Eval(color))
		pTRect->m_StaticText.SetBkgColor((COLORREF)(long)color);

	CRect rectText = pObj->InsideRect(cBaseRect, pTRect->GetBorderPen(), pTRect->GetBorders(), FALSE);
	CSize cs(NormalizeRect(pTRect->GetRatio(), cBaseRect), NormalizeRect(pTRect->GetRatio(), cBaseRect, FALSE));
	rectText.InflateRect(-cs.cx, -cs.cy);

	COLORREF	crRgbBkgColor	= pTRect->m_StaticText.GetBkgColor();		// rgb Bkg Color

	BOOL		bIsTransparent	= pTRect->IsTransparent();

	if (pTRect->IsRoundedBox())
		DrawRound(cBaseRect, pTRect->GetRatio(), pTRect->GetBorderPen(), pTRect->GetBorders(), &crRgbBkgColor, bIsTransparent);
	else
		DrawBorders(cBaseRect, pTRect->GetBorderPen(), pTRect->GetBorders(), &crRgbBkgColor, bIsTransparent);

	// salva la stringa per aggiungere gli special field formattati
	CString strBackupText = pTRect->BuildText();

	GenericTextPDF(&pTRect->m_StaticText, rectText, bIsTransparent, cs);

	// ritorna alla sola string scritta dall'utente
	pTRect->SetText(strBackupText);
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::FileRectPdf(BaseObj* pObj)
{
	TextRectPDF(pObj);
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::RepeaterPdf(BaseObj* pObj)
{
	TRACE("Repeater");
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::TablePdf(BaseObj* pObj)
{
	Table* pTable = dynamic_cast<Table*>(pObj);
	TRACE("Table");

	if (pTable->CheckIsHidden())
		return;

	CRect rect(pTable->m_BaseCellsRect);
	if (pTable->m_bHideTableTitle)
	{
		rect.top += pTable->m_TitleRect.Height();
	}
	if (pTable->m_bHideColumnsTitle)
	{
	
		rect.top += pTable->GetColumnTitleHeight();
	}

	// DrawDropShadow
	INT nDropShadowHeight = pTable->m_nDropShadowHeight;
	CRect rH(
		rect.left + nDropShadowHeight,
		rect.bottom,
		rect.right + nDropShadowHeight,
		rect.bottom + nDropShadowHeight
	);
	CRect rV(
		rect.right,
		rect.top + nDropShadowHeight,
		rect.right + nDropShadowHeight,
		rect.bottom + nDropShadowHeight
	);

	m_rPdf.DrawRectangle(rH.left, rH.top, rH.right, rH.bottom, 0, pTable->m_crDropShadowColor, pTable->m_crDropShadowColor, TRUE);
	m_rPdf.DrawRectangle(rV.left, rV.top, rV.right, rV.bottom, 0, pTable->m_crDropShadowColor, pTable->m_crDropShadowColor, TRUE);


	Borders title_borders(FALSE);
	rect = pTable->m_TitleRect;

	// don't Draw the bottom line for sourronding rectangle
	title_borders.top = pTable->m_Borders.m_bTableTitleTop;
	title_borders.bottom = pTable->m_Borders.m_bTableTitleBottom;
	title_borders.left = pTable->m_Borders.m_bTableTitleLeft;
	title_borders.right = pTable->m_Borders.m_bTableTitleRight;

	if (pTable->ShowTitles(TRUE))
	{
		COLORREF BkgColor = pTable->m_TitlePen.GetColor();

		DrawBorders(pTable->m_TitleRect, pTable->m_TitlePen, title_borders, &BkgColor, FALSE);

		// Get Font in use
		LOGFONT		lf;  ::memset(&lf, 0, sizeof lf);
		const FontStyle* pFontStyle = GetDocument()->m_pFontStyles->GetFontStyle(pTable->m_Title.m_nFontIdx, &(GetDocument()->GetNamespace()));
		ASSERT(pFontStyle);
		lf = pFontStyle->GetLogFont();

		m_rPdf.SetColor(
			GetRValue(pTable->m_Title.m_rgbTextColor),
			GetGValue(pTable->m_Title.m_rgbTextColor),
			GetBValue(pTable->m_Title.m_rgbTextColor)
		);

		for (int i = 0; i < pTable->m_Title.m_PreDraw.m_arTextLine.GetCount(); i++)
		{
			m_rPdf.PdfText
			(
				pTable->m_Title.m_PreDraw.m_arRectLine[i],
				pTable->m_Title.m_PreDraw.m_arTextLine[i],
				pTable->m_Title.m_PreDraw.m_nAlign,
				FontPoinSize(lf), lf.lfFaceName, lf.lfWeight == FW_BOLD, lf.lfItalic
			);
		}
	}

	// Draw Columns Title
	if (!pTable->m_bHideColumnsTitle)
	{
		BOOL bFirst = TRUE;
		int nLastColumn = pTable->LastVisibleColumn();
		for (int nCol = 0; nCol <= nLastColumn; nCol++)
		{
			TableColumn* pColumn = pTable->m_Columns[nCol];
			BOOL bLast = nCol == nLastColumn;

			if (!pColumn->IsHidden())
			{
				DrawColumnTitle(pColumn, bFirst, bLast, pColumn->GetColumnTitleRect());
				bFirst = FALSE;
			}
		}
	}

	// Draw Row
	DrawRows(pTable);
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::DrawRows(Table *pTable)
{
	BOOL bMarkExportable = pTable->isMarkExportable();

	int nLastColumn = pTable->LastVisibleColumn();
	BOOL bCurrRowHasTail = FALSE;
	BOOL bNextRowHasTail = FALSE;

	if (pTable->m_bAlternateBkgColorOnMultiLineRow)
	{
		if (m_pDocument->m_pRDEmanager)
		{
			int page = m_pDocument->m_pRDEmanager->CurrPageRead();

			if (page < 0)
				pTable->m_bAlternateEasyview = TRUE;
			else if (page == pTable->m_arAlternateEasyviewOnPage.GetSize())
				pTable->m_arAlternateEasyviewOnPage.Add(pTable->m_bAlternateEasyview);
			else if (page < pTable->m_arAlternateEasyviewOnPage.GetSize())
				pTable->m_bAlternateEasyview = pTable->m_arAlternateEasyviewOnPage[page];
		}
	}
	BOOL bRowSep = pTable->m_Columns[0]->GetBorders().m_bRowSeparator || pTable->m_Columns[0]->GetBorders().m_bRowSeparatorDynamic;

	BOOL bThereIsDynamicAttr = pTable->ExistsColumnWithDynamicAttributeOnRow();
	BOOL bMergeRow = pTable->m_bAlternateBkgColorOnMultiLineRow || pTable->m_Columns[0]->GetBorders().m_bRowSeparatorDynamic;
	BOOL bSearchTail = bMergeRow || bThereIsDynamicAttr;
	if (bSearchTail)
	{
		bNextRowHasTail = pTable->ExistsCellTail(0);
	}

	for (int nRow = 0; nRow <= pTable->LastRow(); nRow++)
	{
		if (bSearchTail)
		{
			bCurrRowHasTail = bNextRowHasTail;
			bNextRowHasTail = nRow < pTable->LastRow() ? pTable->ExistsCellTail(nRow + 1) : FALSE;

			if (bMergeRow && pTable->m_bAlternateBkgColorOnMultiLineRow && !bCurrRowHasTail)
				pTable->m_bAlternateEasyview = !pTable->m_bAlternateEasyview;
		}

		BOOL bFirstCol = TRUE;
		for (int nCol = 0; nCol <= nLastColumn; nCol++)
		{
			TableColumn* pColumn = pTable->m_Columns[nCol];
			TableCell* pCell = pColumn->GetCells(nRow);

			if (nCol == 0)
			{
				CString sCustomTitle = pTable->m_LineWithCustomTitles[nRow];
				if (!sCustomTitle.IsEmpty())
				{
					ASSERT(FALSE);
					CString sSaveTitle = pColumn->m_Title.GetText(); pColumn->m_Title.SetText(sCustomTitle);
					Expression* pSaveTitle = pColumn->m_pTitleExpr; pColumn->m_pTitleExpr = NULL;

					CRect rect(pCell->GetCellRect());
					rect.left = pTable->m_BaseRect.left;
					rect.right = pTable->m_BaseRect.right;

					//TODO: da finire perchè non è testabile
					DrawColumnTitle(pColumn, TRUE, TRUE, rect);
					pColumn->m_Title.SetText(sSaveTitle); pColumn->m_pTitleExpr = pSaveTitle;
					break;
				}
			}

			if (!pColumn->IsHidden())
			{
				if (nRow == 0)
					pColumn->m_PreviousValue.m_RDEdata.ResetValid();

				if (pTable->m_LineWithTitles[nRow])
				{
					DrawColumnTitle(pColumn, bFirstCol, (nCol == nLastColumn), pCell->GetCellRect(), nRow);
				}
				else
				{
					DrawCell
					(
						pColumn,
						nRow,
						bFirstCol,
						(nCol == nLastColumn),
						bMarkExportable && GetDocument()->m_pExportData->IncludeColumn(nCol),
						bMarkExportable && GetDocument()->m_pExportData->IsTitlesColumn(nCol),
						pColumn->GetBorders().m_bRowSeparatorDynamic && bNextRowHasTail ? FALSE : bRowSep,
						bCurrRowHasTail
					);
				}

				bFirstCol = FALSE;

				if (pColumn->m_bVMergeEqualCell)
				{
					TableCell* pCell = pColumn->GetCells(nRow);
					if (pCell->m_Value.m_RDEdata.IsValid() && !pCell->m_Value.m_RDEdata.IsTailMultiLineString())
					{
						if (pCell->m_Value.GetText().IsEmpty())
							pColumn->m_PreviousValue.m_RDEdata.ResetValid();
						else
							pColumn->m_PreviousValue = pCell->m_Value;
					}
				}
			}
		}
	}
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::DrawCell(TableColumn* pColumn, int nRow, BOOL bFirstCol, BOOL bLastCol, BOOL bMarkExportable, BOOL bIsExpTitles, BOOL bRowSep, BOOL	bCurrRowHasTail)
{
	TableCell* pCell = pColumn->GetCells(nRow);
	TableCell* pNextCell = NULL;
	ASSERT_TRACE(pCell->m_pColumn == pColumn, _TB("TableColumn::DrawCell: Cell has a wrong m_pColumn"));

	CCellArray*	m_pCells = pColumn->GetCellArray();
	ASSERT(m_pCells);

	if
		(
			nRow < m_pCells->GetUpperBound() &&
			(pNextCell = pColumn->GetCells(nRow + 1)) &&
			(
			(pColumn->m_bVMergeEmptyCell && pNextCell->m_Value.GetText().IsEmpty())
				||
				(pColumn->m_bVMergeTailCell && pNextCell->m_Value.m_RDEdata.IsTailMultiLineString())
				||
				(
					pColumn->m_bVMergeEqualCell &&
					!pCell->m_Value.m_RDEdata.IsSubTotal() &&
					!pNextCell->m_Value.m_RDEdata.IsSubTotal() &&
					(
					(
						!pCell->m_Value.GetText().IsEmpty() &&
						pCell->m_Value.GetText() == pNextCell->m_Value.GetText()
						)
						||
						(
							bCurrRowHasTail &&
							!pCell->m_Value.m_RDEdata.IsValid() &&
							!pNextCell->m_Value.m_RDEdata.IsValid()
							)
						||
						(
							bCurrRowHasTail &&
							!pCell->m_Value.m_RDEdata.IsValid() &&
							pNextCell->m_Value.m_RDEdata.IsValid() &&
							pColumn->m_PreviousValue.m_RDEdata.IsValid() &&
							pColumn->m_PreviousValue.GetText() == pNextCell->m_Value.GetText()
							)
						)
					)
				)
			)
	{
		bRowSep = FALSE;
	}

	//top, left, bottom, right
	CellBorders cell_borders
	(
		nRow == 0 && pColumn->GetTable()->m_bHideColumnsTitle && pColumn->GetTable()->m_Borders.m_bBodyTop,
		bFirstCol && pColumn->GetBorders().m_bBodyLeft,
		(
		((nRow != pColumn->LastRow()) && (bRowSep || pColumn->GetTable()->m_Interlines[nRow]))
			||
			(nRow == pColumn->LastRow() && pColumn->GetBorders().m_bBodyBottom)
			),
			(
		(!bLastCol && pColumn->GetBorders().m_bColumnSeparator)
				||
				(bLastCol && pColumn->GetBorders().m_bBodyRight)
				),
		&pColumn->m_ColumnPen,
		(nRow != pColumn->LastRow() && bRowSep && !pColumn->GetTable()->m_Interlines[nRow] ? pColumn->GetTable()->m_Borders.m_pRowSepPen : NULL)
	);

	CObject* pGraphic = NULL;
	if (pColumn->m_pBitmap)
		pGraphic = pColumn->m_pBitmap;
	else if (pColumn->m_pBarCode)
		pGraphic = pColumn->m_pBarCode;

	BOOL bExport =
		(
			pColumn->GetTable()->m_pDocument->m_pDataDefaults->m_bUseAsRadar &&
			pColumn->GetTable()->m_pDocument->m_arWoormLinks.GetConnectionRadar() &&
			(pColumn->GetTable()->GetInternalID() == pColumn->GetTable()->m_pDocument->m_arWoormLinks.GetConnectionRadar()->m_nAlias) &&
			(pColumn->GetTable()->m_pDocument->m_arWoormLinks.GetConnectionRadar()->m_nCurrentRow == nRow)
			)
		||
		(
			bMarkExportable &&
			pColumn->GetTable()->m_pDocument->m_pExportData->IncludeRow(nRow)
			);

	pCell->m_nCurrRow = nRow;

	if (pColumn->HasDynamicAttributeOnRow())
	{
		GetDocument()->UpdateViewSymbolTable(pColumn->GetTable(), nRow, nRow && bCurrRowHasTail);
	}

	// Draw
	/*pCell->Draw
	(
		DC,
		bPreview,
		rectInvalid,
		m_pTable->m_pDocument,
		m_SubTotal,
		cell_borders,
		NoBorders(DC.IsPrinting()),
		pGraph,
		bExport,
		!m_pTable->m_bTransparent,
		bIsExpTitles,
		m_ShowAs == EFieldShowAs::FT_TEXTFILE
	);*/

	BOOL bNoBorders = pColumn->NoBorders(TRUE);

	CRect inside = pColumn->InsideRect(pCell->m_rectCell, *cell_borders.m_pColumnPen, cell_borders, bNoBorders);

	COLORREF crBkg = pColumn->GetAllCellsBkgColor();

	if (cell_borders.m_pRowSepPen && cell_borders.bottom && !bNoBorders)
	{
		Borders b(cell_borders);
		b.bottom = FALSE;
		if (b.Exists())
			DrawBorders(pCell->m_rectCell, *cell_borders.m_pColumnPen, b, &crBkg, FALSE);

		b.bottom = TRUE; b.left = b.right = b.top = FALSE;
		DrawBorders(pCell->m_rectCell, *cell_borders.m_pRowSepPen, b, &crBkg, FALSE);

		inside.bottom += cell_borders.m_pColumnPen->GetWidth() - cell_borders.m_pRowSepPen->GetWidth();
	}
	else
	{
		DrawBorders(pCell->m_rectCell, *cell_borders.m_pColumnPen, cell_borders, &crBkg, bNoBorders);
	}

	// if there are no data use default cell background only
	if (!pCell->m_Value.m_RDEdata.IsEnabled())
	{
		GenericTextPDF(&pCell->m_Value, inside, TRUE, CSize(0,0));

		//DrawNormalCell(DC, inside, pDoc, bOpaque);
		//if (bMarkExportable)
		//	GenericDrawObj::DrawForExport(DC, m_rectCell, GetValueTextColor(), bIsExpTitles);
		return;
	}

	if (pGraphic == NULL)
	{
		GenericTextPDF(&pCell->m_Value, inside, TRUE, CSize(0, 0));
		return;
	}
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::DrawColumnTitle(TableColumn* pColumn, BOOL bFirst, BOOL bLast, const CRect& rectColumnTitleRect, int nRow /*= -1*/)
{
	COLORREF crText = pColumn->m_Title.GetTextColor();
	COLORREF crBkg = pColumn->m_Title.GetBkgColor();
	DataLng color;

	CRect rect = rectColumnTitleRect; 
	
	if (pColumn->m_pTitleTextColorExpr && pColumn->m_pTitleTextColorExpr->Eval(color))
	{
		pColumn->m_Title.SetTextColor((COLORREF)(long)color);
	}
	if (pColumn->m_pTitleBkgColorExpr && pColumn->m_pTitleBkgColorExpr->Eval(color))
	{
		pColumn->m_Title.SetBkgColor((COLORREF)(long)color);
	}

	// Get Font in use
	LOGFONT		lfCol;  ::memset(&lfCol, 0, sizeof lfCol);
	const FontStyle* pFontStyle = GetDocument()->m_pFontStyles->GetFontStyle(GenericText::NORMAL, &(GetDocument()->GetNamespace()));
	ASSERT(pFontStyle);
	lfCol = pFontStyle->GetLogFont();
	
	
	Borders	title_borders
	(
		pColumn->GetBorders().m_bColumnTitleTop && pColumn->ShowTitles(TRUE),
		bFirst && (pColumn->GetBorders().m_bColumnTitleLeft || pColumn->GetBorders().m_bColumnTitleSeparator) && pColumn->ShowTitles(TRUE),
		(pColumn->GetBorders().m_bColumnTitleBottom && pColumn->ShowTitles(TRUE)) || pColumn->GetBorders().m_bBodyTop,
		pColumn->ShowTitles(TRUE) && (	(bLast && pColumn->GetBorders().m_bColumnTitleRight) ||	(!bLast && pColumn->GetBorders().m_bColumnTitleSeparator))
	);

	DrawBorders(rect, pColumn->m_ColumnTitlePen, title_borders, &crBkg, FALSE);

	CRect inside = pColumn->InsideRect
	(
		rect,
		pColumn->m_ColumnTitlePen,
		title_borders,
		pColumn->NoBorders(TRUE)
	);

	m_rPdf.SetColor(
		GetRValue(crText),
		GetGValue(crText),
		GetBValue(crText)
	);

	m_rPdf.PdfText
	(
		inside,
		pColumn->m_Title.GetText(),
		pColumn->m_Title.GetAlign(),
		FontPoinSize(lfCol), lfCol.lfFaceName, lfCol.lfWeight == FW_BOLD, lfCol.lfItalic
	);

	if (pColumn->m_pTitleTextColorExpr) pColumn->m_Title.SetTextColor(crText);
	if (pColumn->m_pTitleBkgColorExpr) pColumn->m_Title.SetBkgColor(crBkg);
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::DoDraw(CPrintInfo* pInfo)
{
	GetDocument()->SetCustomUICulture();

	SymField* pF = GetDocument()->m_ViewSymbolTable.GetFieldByID(REPORT_PAGE_ID);
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

	CSize pageSize  = GetDocument()->m_PageInfo.GetPrinterPageSize_LP();
	m_rPdf.NewPage(pageSize.cx, pageSize.cy);
	
	//----

	//CSize szPageSize(
	//	(pDC->IsPrinting() || (pInfo && pInfo->m_bPreview)) ?
	//	GetPrinterPageSizeWithoutMargin_LP() :
	//	GetPageSizeWithoutMargin_LP()
	//);
	//CRect page(0, 0, szPageSize.cx, szPageSize.cy);

	//// gestione della clip area a video
	//if (!pDC->IsPrinting())
	//{
	//	CPoint sp = GetScrollPosition();
	//	page.OffsetRect(-sp.x, -sp.y);
	//	// dimensiona la regione di clipping come la pagina di stampa
	//	CRgn rgn; rgn.CreateRectRgnIndirect(page);
	//	pDC->SelectClipRgn(&rgn);
	//}
	//else
	//{
	//	ScaleRect(page, *pDC);
	//	//m_pWatermark->Draw(pDC, page);	//Watermark SOTTO, rimane coperto dagli oggetti
	//}

	// in stampa non viene chiamato OnEraseBkgn per cui lo faccio qui per
	// disegnare il bitmap di sfondo a partire sempre dal punto 0,0 e per
	// permettere a video di mostrare la griglia (fatta in erasebkgn)
	//
	/*
		TODO: immagine di sondo da rimettere
	if (pDC->IsPrinting() && !GetDocument()->m_pOptions->NoBitmap(GetDocument(), TRUE))
		DrawBitmap
		(
			pDC,
			GetDocument()->m_pOptions->m_BitmapOrigin,
			GetDocument()->m_pOptions->m_strBkgnBitmap
		);*/

	//----
	/*CFont Ft;
	FontStyle* ft = AfxGetFontStyleTable()->GetFontStyle(AfxGetFontStyleTable()->GetFontIdx(FNT_DEFAULT), NULL);
	if (ft)
	{
		LOGFONT lf = ft->GetLogFont();

		CString sFaceName = AfxGetFontAliasTable()->LookupFaceName(lf.lfFaceName);
		if (!sFaceName.IsEmpty())
			TB_TCSCPY(lf.lfFaceName, (LPCTSTR)sFaceName);

		if (pDC->IsPrinting())
			ScaleLogFont(&lf, *pDC);

		if (Ft.CreateFontIndirect(&lf))
			pDC->SelectObject(&Ft);
	}
	if (Ft.m_hObject == NULL)
	{
		ASSERT(FALSE);
		pDC->SelectObject(AfxGetThemeManager()->GetFormFont());
	}*/
	//----

	// Note: CBCGPScrollView::OnPaint(); //will have already adjusted the
	// viewport origin before calling OnDraw(), to reflect the
	// currently scrolled position.

	// si delega ai singoli oggetti contenuti nel documento di disegnarsi
	
	//TODO: arry degli oggetti da inserire nella pagina - GetDocument()->GetObjects().Paint(*pDC, pInfo);
	CLayout* pObjects = GetDocument()->m_Objects;
	ASSERT(pObjects);
	for (int i = 0; i <= pObjects->GetUpperBound(); i++)
	{
		BaseObj* pObj = (*pObjects)[i];
		ASSERT_VALID(pObj);

		BaseRect* pBaseRectObj = dynamic_cast<BaseRect*>(pObj);
		if (pBaseRectObj)
		{
			if (!pBaseRectObj->PreDraw(NULL))
				continue;

			if (pObj->IsKindOf(RUNTIME_CLASS(FileRect)))
			{
				FileRectPdf(pObj);
			}
			else if (pObj->IsKindOf(RUNTIME_CLASS(TextRect)))
			{
				TextRectPDF(pObj);
			}
			else if (pObj->IsKindOf(RUNTIME_CLASS(Repeater)))
			{
				RepeaterPdf(pObj);
			}
			else if (pObj->IsKindOf(RUNTIME_CLASS(GraphRect)))
			{
				GraphRectPDF(pObj);
			}
			else if (pObj->IsKindOf(RUNTIME_CLASS(SqrRect)))
			{
				SqrRectPDF(pObj);
			}
			else if (pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				FieldRectPDF(pObj);
			}
			else
				ASSERT(TRUE);
		}
		else if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* table = dynamic_cast<Table*>(pObj);
			if (table->CheckIsHidden())
				continue;
			TablePdf(pObj);
		}
		else
			ASSERT(TRUE);
	}

	//// se sono su stampante o in preview non stampo le caratteristiche
	//// di selezione sia singola che multipla
	//if (!pInfo)
	//{
	//	// Paint eventual active object selection
	//	GetDocument()->m_pActiveRect->Paint(*pDC, pInfo);

	//	// Paint eventual active object selection
	//	if (m_pTransActiveRect)
	//		m_pTransActiveRect->Paint(*pDC, pInfo);

	//	// Paint eventual multiple selection
	//	if (GetDocument()->m_pMultipleSelObj)
	//		GetDocument()->m_pMultipleSelObj->Paint(*pDC, pInfo);

	//	// Paint eventual multiple selection
	//	if (GetDocument()->m_pMultiColumns)
	//		GetDocument()->m_pMultiColumns->Paint(*pDC, pInfo);
	//}

	/*
	TODO: da eliminare
	if (!pDC->IsPrinting() && (pInfo ? !pInfo->m_bPreview : TRUE))
	{
		CLongArraySorted* pSplitter = &(GetDocument()->m_PageInfo.m_arHPageSplitter);

			TRACE("Table");
		}
		else
			ASSERT(TRUE);
	}

	
	// ripristino il font evetualmente modificato da qualche oggetto
	// contenente del testo e cancella quello usato per creare il font voluto
	//GetDocument()->ResetCurrentFont(*pDC);

	//if (!GetDocument()->m_bAllowEditing)	//Watermark SOPRA
	//	m_pWatermark->Draw(pDC, page);

	//----
	//GetDocument()->ResetCustomUICulture();
	*/
}

//------------------------------------------------------------------------------------------
void CWoormPDFmake::DrawMargins(CPrintInfo* pInfo)
{
	ASSERT(pInfo && pInfo->m_bPreview);

	//CPoint ptOffset = GetPrintableOffset(pDC);
	//CSize sizePrintable = GetPrintableSize(pDC);
	//CRect rect;

	//if (m_bDoScale)
	//{
	//	// si mette con origine in 0,0 assolute per disegnare i margini
	//	pDC->SetViewportOrg(-ptOffset.x, -ptOffset.y);

	//	double scale = double(m_nScaleDenom) / m_nScaleNum;

	//	double dOffsetX = ptOffset.x * scale;
	//	round(dOffsetX, 0);
	//	ptOffset.x = int(dOffsetX);

	//	double dOffsetY = ptOffset.y * scale;
	//	round(dOffsetY, 0);
	//	ptOffset.y = int(dOffsetY);

	//	double dSzPrintableX = sizePrintable.cx * scale;
	//	round(dSzPrintableX, 0);
	//	sizePrintable.cx = int(dSzPrintableX);

	//	double dSzPrintableY = sizePrintable.cy * scale;
	//	round(dSzPrintableY, 0);
	//	sizePrintable.cy = int(dSzPrintableY);

	//	rect.top = ptOffset.y;
	//	rect.left = ptOffset.x;
	//	rect.right = rect.left + sizePrintable.cx;
	//	rect.bottom = rect.top + sizePrintable.cy;

	//	CPen pen(PS_SOLID, 0, RGB(128, 128, 128));
	//	CPen* pPenOld = pDC->SelectObject(&pen);

	//	if (GetDocument()->m_pWoormIni->m_bShowPrintableArea)
	//	{
	//		pDC->MoveTo(rect.left, rect.top);
	//		pDC->LineTo(rect.right, rect.top);
	//		pDC->LineTo(rect.right, rect.bottom);
	//		pDC->LineTo(rect.left, rect.bottom);
	//		pDC->LineTo(rect.left, rect.top);
	//	}

	//	// disegna il margine settato dall'utente se non si utilizza
	//	// lo spostamento automatico all'origine della area stampabile
	//	if (GetDocument()->m_pWoormIni->m_bShowMargins)
	//	{
	//		CPen pen2(PS_DOT, 0, RGB(128, 128, 128));
	//		CRect rectMargins = GetDocument()->m_PageInfo.m_rectMargins;

	//		CSize szPageSize(
	//			(pDC->IsPrinting() || (pInfo && pInfo->m_bPreview)) ?
	//			GetPrinterPageSizeWithoutMargin_LP() :
	//			GetPageSizeWithoutMargin_LP()
	//		);

	//		if (GetDocument()->GetObjects().m_bInvertOrientation)
	//		{
	//			int cx = szPageSize.cx;
	//			szPageSize.cx = szPageSize.cy;
	//			szPageSize.cy = cx;
	//		}

	//		ScaleRect(rectMargins, *pDC);
	//		ScaleSize(szPageSize, *pDC);
	//		pDC->SelectObject(&pen2);

	//		double dSzCx = szPageSize.cx * scale;
	//		round(dSzCx, 0);
	//		szPageSize.cx = int(dSzCx);

	//		double dSzCy = szPageSize.cy * scale;
	//		round(dSzCy, 0);
	//		szPageSize.cy = int(dSzCy);

	//		double dRectMarginsTop = rectMargins.top * scale;
	//		round(dRectMarginsTop, 0);
	//		rectMargins.top = int(dRectMarginsTop);

	//		double dRectMarginsLeft = rectMargins.left * scale;
	//		round(dRectMarginsLeft, 0);
	//		rectMargins.left = int(dRectMarginsLeft);

	//		pDC->MoveTo(rectMargins.left, rect.top);
	//		pDC->LineTo(rectMargins.left, rect.bottom);
	//		pDC->MoveTo(rect.left, rectMargins.top);
	//		pDC->LineTo(rect.right, rectMargins.top);

	//		pDC->MoveTo(szPageSize.cx + rectMargins.left, rect.top);
	//		pDC->LineTo(szPageSize.cx + rectMargins.left, rect.bottom);
	//		pDC->MoveTo(rect.left, szPageSize.cy + rectMargins.top);
	//		pDC->LineTo(rect.right, szPageSize.cy + rectMargins.top);
	//	}

	//	pDC->SelectObject(pPenOld);
	//	return;
	//}

	//// si mette con origine in 0,0 assolute per disegnare i margini
	//pDC->SetViewportOrg(-ptOffset.x, -ptOffset.y);

	//rect.top = ptOffset.y;
	//rect.left = ptOffset.x;
	//rect.right = rect.left + sizePrintable.cx;
	//rect.bottom = rect.top + sizePrintable.cy;

	//CPen pen(PS_SOLID, 0, RGB(128, 128, 128));
	//CPen* pPenOld = pDC->SelectObject(&pen);

	//if (GetDocument()->m_pWoormIni->m_bShowPrintableArea)
	//{
	//	pDC->MoveTo(rect.left, rect.top);
	//	pDC->LineTo(rect.right, rect.top);
	//	pDC->LineTo(rect.right, rect.bottom);
	//	pDC->LineTo(rect.left, rect.bottom);
	//	pDC->LineTo(rect.left, rect.top);
	//}

	//// disegna il margine settato dall'utente se non si utilizza
	//// lo spostamento automatico all'origine della area stampabile
	//if (GetDocument()->m_pWoormIni->m_bShowMargins)
	//{
	//	CPen pen2(PS_DOT, 0, RGB(128, 128, 128));
	//	CRect rectMargins = GetDocument()->m_PageInfo.m_rectMargins;

	//	CSize szPageSize(
	//		(pDC->IsPrinting() || (pInfo && pInfo->m_bPreview)) ?
	//		GetPrinterPageSizeWithoutMargin_LP() :
	//		GetPageSizeWithoutMargin_LP()
	//	);

	//	/*if (GetDocument()->GetObjects().m_bInvertOrientation)
	//	{
	//	int cx = szPageSize.cx;
	//	szPageSize.cx = szPageSize.cy;
	//	szPageSize.cy = cx;
	//	}*/

	//	ScaleRect(rectMargins, *pDC);
	//	ScaleSize(szPageSize, *pDC);
	//	pDC->SelectObject(&pen2);

	//	pDC->MoveTo(rectMargins.left, rect.top);
	//	pDC->LineTo(rectMargins.left, rect.bottom);
	//	pDC->MoveTo(rect.left, rectMargins.top);
	//	pDC->LineTo(rect.right, rectMargins.top);

	//	pDC->MoveTo(szPageSize.cx + rectMargins.left, rect.top);
	//	pDC->LineTo(szPageSize.cx + rectMargins.left, rect.bottom);
	//	pDC->MoveTo(rect.left, szPageSize.cy + rectMargins.top);
	//	pDC->LineTo(rect.right, szPageSize.cy + rectMargins.top);
	//}

	//pDC->SelectObject(pPenOld);
}