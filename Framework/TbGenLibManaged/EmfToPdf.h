#pragma once

#include "stdafx.h"
#include "beginh.dex"
//-----------------------------------------------------------------------------

BOOL	TB_EXPORT	ConcatPdf			(const CStringArray& strDocs, const CString& strDestination, const CString& strPassword = _T(""));
int		TB_EXPORT	GetPdfPageNumber	(const CString& strDoc, const CString& strPassword = _T(""));
BOOL	TB_EXPORT	CheckPdf			(const CString& strFile);
CString TB_EXPORT	NumerateFileName	(const CString fileName, int number, int extensionLenght = 3); /*extension lenght is 3 by default because pdf, jpg, txt are 3 characters long*/
CString TB_EXPORT	GenerateUniqueFileName ();

//-----------------------------------------------------------------------------
class CInternalPdfDocumentWrapper;

class TB_EXPORT CPdfDocumentWrapper
{
private:
	CInternalPdfDocumentWrapper* m_pDocument;

	int m_nPagesWithNotFlushedMemory;

public:
	CPdfDocumentWrapper();
	~CPdfDocumentWrapper();

	void AddPageFromMetafile
		(
			HENHMETAFILE handle, 
			const CRect& margins, 
			bool bPrinterDC, 
			double scale = 1.0,
			int	originalMarginX = 0,
			int rotateAngle = 0,
			int mmPageWidth = 0, 
			int mmPageHeight = 0,
			BOOL invertedOrientation = FALSE
	//		LOGFONT lfDefault = LOGFONT()
		);
	
	void Save
		(
			const CString& strPath, 
			UINT nCopies = 1,
			const CString& strPassword = _T(""),
			BOOL bOpen = FALSE
		);
};
#include "endh.dex"