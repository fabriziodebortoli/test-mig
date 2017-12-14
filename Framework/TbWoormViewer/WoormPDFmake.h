#pragma once

#include <TbGenlibManaged\RenderPdfWrappers.h>

#include "woormdoc.h"
#include "RECTOBJ.H"
#include "Table.H"
#include "Repeater.H"

//includere alla fine degli include del .H
#include "beginh.dex"
//===========================================================================
class TB_EXPORT CWoormPDFmake
{
	enum ScalingType { ONLY_VERTICAL, ONLY_HORIZONTAL, HORIZONTAL_MORETHAN_VERTICAL, VERTICAL_MORETHAN_HORIZONTAL, NONE };
	enum PrintJobStatus { NO_PRINT_ERROR, PRINT_ABORT, FAILED_TO_START_PRINT };

	private:
		RenderPdfWrappers	m_rPdf;
		CWoormDocMng*		m_pDocument;
		CDC*				m_pDC;
		
		BOOL				m_bPDFPrinting	= TRUE;
		UINT				m_nFromPage		= 0;
		UINT				m_nToPage		= 0;

	public:
		CWoormPDFmake(CWoormDocMng* pWDoc);
		void Render(UINT nStartPage, UINT nEndPage, UINT nCopies, LPCTSTR pszFileName, LPCTSTR pszPassword, CString sPrinterTemplate);

	protected:
		BOOL	m_bDoScale;
		ScalingType	m_scaleType;
		double  m_dLowerScale;
		double  m_dScaleFactor;
		BOOL	m_bCalculatedScale;
		CPoint  m_ptOffsetOrgAfterScaling;
		PrintJobStatus m_PrintStatus;

	private:
		CWoormDocMng*	GetDocument() { ASSERT(m_pDocument); return m_pDocument;}
		void            OnBeginPrinting(CPrintInfo* pInfo);
		CSize			GetPrinterPageSizeWithoutMargin_LP();
		CSize			GetPrinterFullPageSize_mm();
		void			OnEndPrinting(CPrintInfo* pInfo);
		void			OnPrint(CPrintInfo* pInfo);
		void			DoDraw(CPrintInfo* pInfo);
		void			DrawMargins(CPrintInfo* pInfo);

		void			GenericTextPDF(GenericText* pGenText, const CRect& rectText, BOOL bIsTransparent, const CSize& cs);

		void			TextRectPDF(BaseObj* pObj);
		void			FieldRectPDF(BaseObj* pObj);
		void			SqrRectPDF(BaseObj* pObj);
		void			GraphRectPDF(BaseObj* pObj);
		void			DrawColumnTitle(TableColumn* pColumn, BOOL bFirst, BOOL bLast, const CRect& rectColumnTitleRect, int nRow = -1);
		void			DrawCell(TableColumn* pColumn, int nRow, BOOL bFirstCol, BOOL bLastCol,	BOOL bMarkExportable, BOOL bIsExpTitles, BOOL bRowSep, BOOL	bCurrRowHasTail);

		void			DrawRows(Table *pTable);
		void			TablePdf(BaseObj* pObj);
		void			FileRectPdf(BaseObj* pObj);
		void			RepeaterPdf(BaseObj* pObj);

		void			DrawBorders	(CRect rect, const BorderPen& borderPen, const Borders& borders, COLORREF* pRgbBkgColor, BOOL bIsTransparent);
		void			DrawRound(CRect rect, INT radio, const BorderPen& borderPen, const Borders& borders, COLORREF* pRgbBkgColor, BOOL bIsTransparent);

		INT			FontPoinSize(LOGFONT lf);
};

#include "endh.dex"
