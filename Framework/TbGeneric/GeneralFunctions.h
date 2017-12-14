#pragma once
#include <limits.h>
#include <atlimage.h>
#include <TbNameSolver\FileSystemFunctions.h>

//includere alla fine degli include del .H
#include "beginh.dex"

#define	H_EXTRA_EDIT_PIXEL	 8
#define	V_EXTRA_EDIT_PIXEL	 6
#define SCALING_FACTOR		 96

#define BaseWidthLU			 164
#define BaseWidthPX			 286

//=============================================================================
enum TB_EXPORT MeasureUnits { CM = 0, INCH = 1, STD_MU = 999 };
enum TB_EXPORT TextSizeAlgo { TSA_CHECK_INI, TSA_NEW, TSA_OLD };

// helper function to get window text size
//=============================================================================
TB_EXPORT int GetLogPixels();

TB_EXPORT CSize GetTextSize(CDC* pDC, const CString& str, LOGFONT lfFont);
TB_EXPORT CSize GetTextSize(CDC* pDC, const CString& str, CFont* pFont = NULL);
TB_EXPORT CSize GetTextSize(CDC* pDC, int nLen, CFont* pFont);

TB_EXPORT CSize GetMultilineTextSize(CDC* pDC, CFont* pFont, CString str);

TB_EXPORT CSize GetEditSize(CDC* pDC, CFont* pFont, const CString& str, BOOL bInner = FALSE);
TB_EXPORT CSize GetEditSize(CDC* pDC, CFont* pFont, int nLen, int nLine = 1, BOOL bInner = FALSE);

TB_EXPORT CSize GetMultilineTextSize	(CFont* pFont, CString str);
TB_EXPORT CSize GetTextSize				(CFont* pFont, const CString& str);

TB_EXPORT void	CenterWindow	(CWnd* pWin, CWnd* pCnt);
TB_EXPORT void	GetMonitorRect	(CWnd* pWin, CRect& monitorRect, CRect& workAreaRect);

// conversion between logic point and mesurement units
//=============================================================================
TB_EXPORT int		MUtoLP	(double muv, MeasureUnits baseMu = CM, double scale = 10., int nDec = 2);
TB_EXPORT double	LPtoMU	(int lpv, MeasureUnits baseMu = CM, double scale = 10., int nDec = 2);
TB_EXPORT void		round	(double& val, int nDec);

TB_EXPORT int		GetDisplayFontHeight		(int nPointSize);
TB_EXPORT int		GetDisplayFontPointSize		(int nHeight);

// mapping mode related constants (in MM_TEXT)
//=============================================================================
// helper to scale log font member from one DC to another!
TB_EXPORT void ScaleLogFont		(LPLOGFONT plf, const CDC& dc);
TB_EXPORT int  ScaleHeightFont	(int lfHeight, const CDC& dc);

TB_EXPORT void ScaleSize	(CSize& size,	const CDC& dc);
TB_EXPORT void ScalePoint	(CPoint& point, const CDC& dc);
TB_EXPORT void ScaleRect	(CRect& rect,	const CDC& dc);

TB_EXPORT void UnScaleSize  (CSize& size,	const CDC& dc);
TB_EXPORT void UnScaleRect	(CRect& rect,	const CDC& dc);
TB_EXPORT void UnScalePoint  (CPoint& point, const CDC& dc);

TB_EXPORT int	GetFontPointHeight(CDC* pDC, LOGFONT& lf, BOOL bPreview = FALSE);
TB_EXPORT LONG  UnScalePix	(LONG px);
TB_EXPORT LONG  ScalePix	(LONG px);
TB_EXPORT CRect ScaleRect	(CRect rect);
TB_EXPORT BOOL  IsScale		();
TB_EXPORT void  ScaleFrame(CFrameWnd* hWnd, BOOL center = TRUE);

TB_EXPORT void DpiConvertPoint	(CPoint& point, const INT DpiSource, const INT DpiDest);
TB_EXPORT void DpiConvertSize	(CSize&   size, const INT DpiSource, const INT DpiDest);

//------------------------------------------------------------------------------

TB_EXPORT int PtToLfHeight(int pt);
TB_EXPORT int LfHeightToPt(int lfh);

//------------------------------------------------------------------------------
// ritorna il delta tra il rettangolo contenente l'ellisse e quello contenuto 
// rispetto all'asse desiderato
TB_EXPORT int NormalizeRect (int nRatio, const CRect& rect, BOOL bHorz = TRUE);

// File management non spostabili in NameSolver
//=============================================================================
TB_EXPORT BOOL	CopyFile				(const CString& strSource, const CString& strTarget, BOOL bShowMessage=TRUE);
TB_EXPORT BOOL	CopyFolderTree			(const CString& strOrigin, const CString& strDestination, BOOL bOverWrite = FALSE, BOOL bShowMessage = FALSE);
TB_EXPORT BOOL	RenameFilePath			(const CString& strOldPath, const CString& strNewPath, BOOL bShowMessage =TRUE); //serve per il rename di file o path
TB_EXPORT BOOL  CheckDirectoryAccess	(const CString& sPath);
TB_EXPORT void	GetAllFiles				(CString sPath, CString sExt, BOOL bRecursive, CStringArray& files);
TB_EXPORT void	ReplaceFileInvalidChars(CString& sPath, TCHAR replace);
//=============================================================================
TB_EXPORT CString TbCryptString		(CString str, LPCTSTR pszAux = NULL);
TB_EXPORT CString TbDecryptString	(CString strCrypted, LPCTSTR pszAux = NULL);

//-----------------------------------------------------------------------------
TB_EXPORT LPCTSTR	PCause		(CFileException* e);

// strings functions
//=============================================================================
TB_EXPORT CString CDECL  cwvsprintf	(const TCHAR *fmt, va_list marker);
TB_EXPORT CString CDECL  cwsprintf	(LPCTSTR pszFmt, ...);
TB_EXPORT CString FixBareLF(const CString &str);

//---------------------------------------------------------------------------
// torna il minore tra il numero di decimali richiesti e il massimo ammissibili in stampa 
// per il numero (in ragione del numero di cifre prima della virgola), in modo che il totale
// non superi 15 (massima precisione per il double)
TB_EXPORT int GetNumDecimals(double nVal, int nRequestedDecimals = 15);

// equivalente a cwsprintf("%*.*f",nLen,nDec,nVal) con la opportuna gestione del massimo nr. di
// decimali rappresentabili e dei default per nLen (-1 = lungo quanto serve) e per nDec(-1 = precisione 
// di default a 6 cifre)
TB_EXPORT CString dblcwsprintf(double nVal, int nLen = -1, int nDec = -1);

// idem, ma come sprintf e sempre lunghezza massima
TB_EXPORT CString dblsprintf(double nVal, int nDec = -1);

//---------------------------------------------------------------------------
TB_EXPORT int		ReverseFindOneOf (const CString& source, LPCTSTR lpszCharSet, int nStart);
TB_EXPORT int		FindOneOf		(const CString& source, LPCTSTR lpszCharSet, int nStart);
TB_EXPORT int		FindNoCase		(CString source, CString sub, int startIndex = 0);
TB_EXPORT CString&	ReplaceNoCase	(CString& source, CString sFrom, const CString& sTo, int startIndex = 0, int nOccurence = 0);
TB_EXPORT int		FindOccurence	(CString source, CString sub, int nOccurence, int nStartIndex = 0, BOOL bNoCase = FALSE);
TB_EXPORT int		ReverseFind		(CString source, CString sub, int nStartIndex = -1, int nOccurence = 1, BOOL bNoCase = FALSE);
TB_EXPORT int		FindWord		(CString source, CString word, BOOL noCase = TRUE, int startIndex = 0);
TB_EXPORT int		ReverseFindWord	(CString source, CString word, BOOL noCase = TRUE, int startIndex = -1);
TB_EXPORT int		CountChars		(const CString& source, TCHAR c, int startIndex = 0, int endIndex = -1);
TB_EXPORT CString	RemoveChars		(const CString& strSource, const CString& strCharToRemove);
TB_EXPORT bool		WildcardMatch	(LPCTSTR pszString, LPCTSTR pszMatch);

TB_EXPORT BOOL		IsValidName(const CString&);
TB_EXPORT CString	HTMLEncode	(const CString& strSource);

TB_EXPORT CString&	StripBlankNearSquareBrackets (CString& str);
TB_EXPORT void		StripBlank	(TCHAR* szBuf);
TB_EXPORT CString&	StripBlank	(CString& s); // usare s.TrimRight(); 
TB_EXPORT CString&	AddCR(CString& str);
TB_EXPORT CString&	RemoveCR(CString& str);
TB_EXPORT CString&	StrAppend(CString& str, LPCTSTR sToAdd, BOOL space, BOOL newLine);

//	GUID useful manager routines
//---------------------------------------------------------------------------
TB_EXPORT CString	GuidToString (REFGUID);
TB_EXPORT GUID		StringToGuid (LPCTSTR pszGUID);

//=============================================================================
TB_EXPORT BOOL		IsIntNumber			(LPCTSTR s);
TB_EXPORT BOOL		IsFloatNumber		(LPCTSTR s);
TB_EXPORT BOOL		IsExtAlphaNumeric	(int ch);
TB_EXPORT BOOL		IsExtAlpha			(int ch);

//-----------------------------------------------------------------------------
TB_EXPORT UINT GetActiveWindows(HWND hwnd,  CPtrArray& arWindows);
TB_EXPORT CWnd* GetWindowControl(CWnd* pOuterWnd, CPoint point);
TB_EXPORT void InvalidateWndAndAllChilds(CWnd* pWnd);

//-----------------------------------------------------------------------------
TB_EXPORT void	DrawBitmap(CBitmap* pBmp, CDC* pDC, const CRect& rect, BOOL bStretch = FALSE);
//-----------------------------------------------------------------------------
TB_EXPORT UINT	GetNumIDS	(const UINT IdString);//il numero ad essa assegnata all'interno del file
TB_EXPORT BOOL	IsFontInstalled(LPCTSTR szFaceName);
//-----------------------------------------------------------------------------
TB_EXPORT void SafeArrayToStringArray(SAFEARRAY *pInArray, CStringArray& arOutArray, BOOL bDestroyOriginal);
//-----------------------------------------------------------------------------
TB_EXPORT void AddEnvironmentVariable(const CString& strName, const CString& strValue);

//-----------------------------------------------------------------------------
//since under IIs ::IsWindowVisible always returns TRUE, use this version that tests window status instead
TB_EXPORT BOOL IsTBWindowVisible(CWnd* pWnd);

///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT TBEDisableWnd : public CObject
{
protected:
	CWnd*	m_pWnd;

public:
	// Costruttore con CWnd esplicita
	TBEDisableWnd(CWnd* pWnd)
	{
		m_pWnd = pWnd;
		ASSERT(pWnd);
		ASSERT(pWnd->IsKindOf(RUNTIME_CLASS(CWnd)));

		// disabilito la CWnd
		Enable(FALSE);
	}
	
	// Costruttore con un CDocument, la mia CWnd e' la frame.
	TBEDisableWnd(CDocument* pDoc)
	{
		ASSERT(pDoc);
		ASSERT(pDoc->IsKindOf(RUNTIME_CLASS(CDocument)));
		POSITION	pos = pDoc->GetFirstViewPosition();
		m_pWnd	= pDoc->GetNextView(pos)->GetParentFrame();
		
		// disabilito la frame
		Enable(FALSE);
	}	
	
	// distruttore, riabilito la CWnd
	~TBEDisableWnd()
	{
		Restore();
	}

	// riabilita la CWnd
	void	Restore()
	{
		Enable(TRUE);
	}

protected:
	// abilita / disabilita la CWnd
	void	Enable(BOOL bEnable)
	{
		if (m_pWnd)
		{
			m_pWnd->EnableWindow(bEnable);
		}
	}
};

//-----------------------------------------------------------------------------
TB_EXPORT void CEditGetSel(const CEdit& Ce, int& nStart, int& nStop);
TB_EXPORT void CEditReplaceSel(CEdit& Ce, LPCTSTR str);
TB_EXPORT void CEditReplaceSelection(CEdit& Ce, CString str);

//-----------------------------------------------------------------------------
TB_EXPORT COLORREF OppositeRGB			(COLORREF rgb);
TB_EXPORT COLORREF GetColorFromString	(LPCTSTR lpszColor);

//-----------------------------------------------------------------------------
TB_EXPORT CString EscapeURL(const CString& s);
TB_EXPORT CString GetTBNavigateUrl(const CString& sNamespace, const CString& sPrimaryKeyArg);

//-----------------------------------------------------------------------------

TB_EXPORT BOOL CopyToClipboard(const CString& string);

//-----------------------------------------------------------------------------
TB_EXPORT CTime		CStringToCTime(CString	strValue);
TB_EXPORT CString	CTimeToCString(CTime	aTime);
TB_EXPORT BOOL		ParseSockArray(CString strTextReceived, CStringArray& resultArray);

//-----------------------------------------------------------------------------
// find
TB_EXPORT TCHAR * _tcsistr(const TCHAR * str, const TCHAR * substr);
TB_EXPORT int _tcsccnt(const TCHAR *str, TCHAR ch);

// removal
TB_EXPORT TCHAR * _tcscrem(TCHAR *str, TCHAR ch);
TB_EXPORT TCHAR * _tcsicrem(TCHAR *str, TCHAR ch);
TB_EXPORT TCHAR * _tcsstrrem(TCHAR *str, const TCHAR *substr);
TB_EXPORT TCHAR * _tcsistrrem(TCHAR *str, const TCHAR *substr);

// replace
TB_EXPORT TCHAR * _tcscrep(TCHAR *str, TCHAR chOld, TCHAR chNew);
TB_EXPORT TCHAR * _tcsicrep(TCHAR *str, TCHAR chOld, TCHAR chNew);
TB_EXPORT int     _tcsistrrep(const TCHAR * lpszStr, 
					const TCHAR * lpszOld, 
					const TCHAR * lpszNew, 
					TCHAR * lpszResult);

// trim
TB_EXPORT TCHAR *_tcsltrim(TCHAR *str, const TCHAR *targets);
TB_EXPORT TCHAR *_tcsrtrim(TCHAR *str, const TCHAR *targets);
TB_EXPORT TCHAR *_tcstrim(TCHAR *str, const TCHAR *targets);

// copy
TB_EXPORT TCHAR *_tcsnzdup(const TCHAR *str, size_t count);
TB_EXPORT TCHAR *_tcszdup(const TCHAR * str);

/////////////////////////////////////////////////////////////////////////////

TB_EXPORT CString GetTimeZoneDifference	();	// Ritorna "+01:00"

TB_EXPORT CString GetAcceleratorText(const ACCEL& accel);

/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTBToolbarMenuExtFunctions
{
public:
	// this is a recursive function which will attempt
	// to add the item "itemText" to the menu with the
	// given ID number. The "itemText" will be parsed for
	// delimiting "\" characters for levels between
	// popup menus. If a popup menu does not exist, it will
	// be created and inserted at the end of the menu
	// itemID of 0 will cause a separator to be added
	static BOOL AddMenuItem(HMENU hTargetMenu, const CString& itemText, UINT itemID, BOOL bCheck = FALSE);
};

//=============================================================================
#include "endh.dex"
