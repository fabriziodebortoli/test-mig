
#include "stdafx.h"

#ifdef new
#undef new
#endif

#include <string>
#include <atlenc.h>
#include <Atlutil.h>
#include <math.h>

using namespace std;

#include <TbNameSolver\Chars.h>
#include <TBNamesolver\threadcontext.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TbNameSolver\applicationcontext.h>
#include <TbNameSolver\logincontext.h> 

#include <TbFrameworkImages\GeneralFunctions.h> 

#include "TbStrings.h"
#include "Crypt.h"
#include "Critical.h"	// serve per far esportare la classe
#include "minmax.h"
#include "LineFile.h"
#include "CollateCultureFunctions.h"
#include "SettingsTable.h"
#include "ParametersSections.h"
#include "TBThemeManager.h"

#include "GeneralFunctions.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR BASED_CODE szUnknown[] = _T("unknown");
static const TCHAR			szStringSeparator[] = _T("@TB@");
//=============================================================================
//
// Decodifica eccezioni sui file
//
//=============================================================================
// character strings to use for dumping CFileException
static const TCHAR szNone[] = _T("Nessuno");
static const TCHAR szGeneric[] = _T("Generico");
static const TCHAR szFileNotFound[] = _T("File non trovato");
static const TCHAR szBadPath[] = _T("Percorso non valido");
static const TCHAR szTooManyOpenFiles[] = _T("Troppi file aperti");
static const TCHAR szAccessDenied[] = _T("Accesso negato");
static const TCHAR szInvalidFile[] = _T("File invalido");
static const TCHAR szRemoveCurrentDir[] = _T("Cancellare la corrente directory");
static const TCHAR szDirectoryFull[] = _T("Directory piena");
static const TCHAR szBadSeek[] = _T("Posizionamento errato");
static const TCHAR szHardIO[] = _T("Errore hardware");
static const TCHAR szSharingViolation[] = _T("Violazione di condivisione");
static const TCHAR szLockViolation[] = _T("Violazione di lock");
static const TCHAR szDiskFull[] = _T("Disco pieno");
static const TCHAR szEndOfFile[] = _T("Fine file");

static const TCHAR FAR* BASED_CODE rgszCFileExceptionCause[] =
{
	szNone,
	szGeneric,
	szFileNotFound,
	szBadPath,
	szTooManyOpenFiles,
	szAccessDenied,
	szInvalidFile,
	szRemoveCurrentDir,
	szDirectoryFull,
	szBadSeek,
	szHardIO,
	szSharingViolation,
	szLockViolation,
	szDiskFull,
	szEndOfFile,
};

////////////////////////////////////////////////////////////////////////////////	                             
//				helper function to get window text size
////////////////////////////////////////////////////////////////////////////////	                             

//------------------------------------------------------------------------------
TB_EXPORT int GetLogPixels()
{
	HDC hdc = GetDC(NULL);
	int _dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
	ReleaseDC(NULL, hdc);
	return _dpiX;
}

//------------------------------------------------------------------------------
CSize GetTextSize(CFont* pFont, const CString& str)
{
	CDC		dc; dc.CreateIC(_T("DISPLAY"), NULL, NULL, NULL);
	CFont*	pOldFont = dc.SelectObject(pFont);
	CSize cs = dc.GetTextExtent(str, str.GetLength());

	TEXTMETRIC tm;
	dc.GetTextMetrics(&tm);
	cs.cy = tm.tmHeight + tm.tmExternalLeading;

	dc.SelectObject(pOldFont);
	return cs;
}

//------------------------------------------------------------------------------
CSize GetMultilineTextSize(CFont* pFont, CString strT)
{
	if (strT.Find(_T('\n')) >= 0)
	{
		TCHAR* nextTK;
		TCHAR* pT = strT.GetBuffer(strT.GetLength());
		TCHAR* pRow = _tcstok_s(pT, _T("\n"), &nextTK);

		int nMaxMultilineTitleWidth = 0;
		CSize cs;

		while (pRow)
		{
			cs = GetTextSize(pFont, pRow);

			if (cs.cx > nMaxMultilineTitleWidth)
				nMaxMultilineTitleWidth = cs.cx;

			pRow = _tcstok_s(NULL, _T("\n"), &nextTK);
		}
		strT.ReleaseBuffer();

		return CSize(nMaxMultilineTitleWidth, cs.cy);
	}
	else
		return GetTextSize(pFont, strT);
}

//------------------------------------------------------------------------------
CSize GetTextSize(CDC* pDC, const CString& str, LOGFONT		lfFont)
{
	CFont f;
	f.CreateFontIndirect(&lfFont);

	return GetTextSize(pDC, str, &f);
}

//------------------------------------------------------------------------------
CSize GetTextSize(CDC* pDC, const CString& str, CFont* pFont/*=NULL*/)
{
	if (!pDC)
		return CSize(0, 0);

	CFont*	pOldFont = NULL;
	if (pFont) pOldFont = pDC->SelectObject(pFont);

	CSize cs = pDC->GetTextExtent(str, str.GetLength());

	if (pFont == NULL)
		ScaleSize(cs, *pDC);

	TEXTMETRIC tm;
	pDC->GetTextMetrics(&tm);
	cs.cy = tm.tmHeight + tm.tmExternalLeading;

	if (pFont) pDC->SelectObject(pOldFont);
	return cs;
}

//------------------------------------------------------------------------------
int GetTextWidth(CDC* pDC, const CString& str, CFont* pFont/*=NULL*/)
{
	if (!pDC)
		return 0;

	CFont*	pOldFont = NULL;
	if (pFont) pOldFont = pDC->SelectObject(pFont);

	CSize cs = pDC->GetTextExtent(str, str.GetLength());

	if (pFont == NULL)
		ScaleSize(cs, *pDC);

	if (pFont) pDC->SelectObject(pOldFont);
	return cs.cx;
}

//------------------------------------------------------------------------------
CSize GetTextSize(CDC* pDC, int nLen, CFont* pFont)
{
	if (nLen <= 0)
		return CSize(0, 0);

	CString str;

	if (!AfxGetCultureInfo()->GetCharSetSample().IsEmpty())
	{
		srand(0);

		for (; nLen > 0; nLen--)
		{
			int nChar = (rand() % AfxGetCultureInfo()->GetCharSetSample().GetLength());
			str += CString(AfxGetCultureInfo()->GetCharSetSample().GetAt(nChar));
		}
		str += BLANK_CHAR;
	}
	else
	{
		srand(nLen);
		for (; nLen > 0; nLen--)
			str += CString((TCHAR)((rand() % (_T('z') - BLANK_CHAR)) + BLANK_CHAR));
	}

	return GetTextSize(pDC, str, pFont);
}

//------------------------------------------------------------------------------
CSize GetMultilineTextSize(CDC* pDC, CFont* pFont, CString strT)
{
	if (strT.Find(LF_CHAR) >= 0)
	{
		TCHAR* pT = strT.GetBuffer(strT.GetLength());
		TCHAR* nextToken;
		TCHAR* pRow = _tcstok_s(pT, _T("\n"), &nextToken);
		int nMaxMultilineTitleWidth = 0;
		CSize cs;
		int y = 0;

		while (pRow)
		{
			cs = GetTextSize(pDC, pRow, pFont);

			if (cs.cx > nMaxMultilineTitleWidth)
				nMaxMultilineTitleWidth = cs.cx;
			y += cs.cy;

			pRow = _tcstok_s(NULL, _T("\n"), &nextToken);
		}
		strT.ReleaseBuffer();

		return CSize(nMaxMultilineTitleWidth, y);
	}
	else
		return GetTextSize(pDC, strT, pFont);
}

//------------------------------------------------------------------------------
CSize GetEditSize(CDC* pDC, CFont* pFont, const CString& str, BOOL bInner /*= FALSE*/)
{
	CSize cs = GetTextSize(pDC, str, pFont);

	if (bInner)
	{
		cs.cx += 2; /// add extra inner space
		cs.cy += 2; /// add extra inner space
	}
	else
	{
		cs.cx += H_EXTRA_EDIT_PIXEL; /// add extra border for edit
		cs.cy += V_EXTRA_EDIT_PIXEL; /// add extra border for edit
	}

	return cs;
}

//------------------------------------------------------------------------------
CSize GetEditSize(CDC* pDC, CFont* pFont, int nLen, int nLine, BOOL bInner /*= FALSE*/)
{
	CSize cs = GetTextSize(pDC, nLen, pFont);

	if (nLine >= 1)
		cs.cy *= nLine;

	if (bInner)
	{
		cs.cx += ScalePix(2); /// add extra inner space
		cs.cy += ScalePix(2); /// add extra inner space
	}
	else
	{
		cs.cx += ScalePix(H_EXTRA_EDIT_PIXEL); /// add extra border for edit
		cs.cy += ScalePix(V_EXTRA_EDIT_PIXEL); /// add extra border for edit
	}

	return cs;
}

//----------------------------------------------------------------------------
void CenterWindow(CWnd* win, CWnd* cnt)
{
	CRect   rectWin, rectCenter;

	if (!cnt || !cnt->m_hWnd)
		CWnd::GetDesktopWindow()->GetWindowRect(rectCenter);
	else
		cnt->GetWindowRect(rectCenter);

	win->GetWindowRect(rectWin);

	int x = (rectCenter.Width() - rectWin.Width()) / 2 + rectCenter.left;
	int y = (rectCenter.Height() - rectWin.Height()) / 2 + rectCenter.top;

	// la SetWindowPos prende come riferimento l'angolo in alto a sinistra del framework di TB
	//  mentre la mia x e la mia y (quelle della Ask) hanno come riferimento l'angolo in alto a sinistra
	//  del monitor principale. Modifico di conseguenza le mie coordinate.
	x = x - rectCenter.left;
	y = y - rectCenter.top;

	win->SetWindowPos(NULL, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
}

//-----------------------------------------------------------------------------
void GetMonitorRect(CWnd* pWin, CRect& monitorRect, CRect& workAreaRect)
{
	MONITORINFO mi;
	mi.cbSize = sizeof(mi);
	GetMonitorInfo(MonitorFromWindow(pWin->m_hWnd, MONITOR_DEFAULTTOPRIMARY), &mi);
	monitorRect = mi.rcMonitor;
	workAreaRect = mi.rcWork;
}

//-----------------------------------------------------------------------------
void InvalidateWndAndAllChilds(CWnd* pWnd)
{
	pWnd->InvalidateRect(NULL, TRUE);
	CWnd* pWndChild = pWnd->GetWindow(GW_CHILD);
	while (pWndChild)
	{
		InvalidateWndAndAllChilds(pWndChild);
		pWndChild = pWndChild->GetWindow(GW_HWNDNEXT);
	}
}

//--------------------------------------------------------------------------------
CWnd* GetWindowControl(CWnd* pOuterWnd, CPoint point)
{
	CRect rect;

	CWnd* pwndChild = pOuterWnd->GetWindow(GW_CHILD);
	while (pwndChild)
	{
		pwndChild->GetWindowRect(rect);
		if (rect.PtInRect(point))
		{
			CWnd *pInnerWindow = GetWindowControl(pwndChild, point);
			if (pInnerWindow) return pInnerWindow;
			return pwndChild;
		}

		pwndChild = pwndChild->GetNextWindow();
	}

	pOuterWnd->GetWindowRect(rect);

	return rect.PtInRect(point) ? pOuterWnd : NULL;
}



//----------------------------------------------------------------------------
int MUtoLP(double muv, MeasureUnits baseMu, double scale, int nDec)
{
	HDC screen = GetDC(0);
	int dpiY = GetDeviceCaps(screen, LOGPIXELSY);
	ReleaseDC(0, screen);
	

	// convert to inch if user works in cm
	if (baseMu == CM)
		muv /= 2.54;

	// round to nDec decimal digits		
	round(muv, nDec);

	// convert to base user mesurement units (inch or cm)
	muv /= scale;

	int sign = (muv < 0. ? -1 : 1);

	// convert from inch to logical units
	if (fabs(muv) * dpiY <= INT_MAX)
	{
		muv *= dpiY;
		return sign * (int)floor(fabs(muv) + 0.5);
	}
	else
		return sign * INT_MAX;
}

//----------------------------------------------------------------------------
double LPtoMU(int lpv, MeasureUnits baseMu, double scale, int nDec)
{
	HDC screen = GetDC(0);
	int dpiY = GetDeviceCaps(screen, LOGPIXELSY);
	ReleaseDC(0, screen);

	// convert from logical units to inch
	double muv = (double)lpv / dpiY;

	// convert to cm if user works in cm
	if (baseMu == CM)
		muv *= 2.54;

	// convert to derived user mesurement units (INCH/scale or CM/scale)
	muv *= scale;

	// round to nDec decimal digits
	round(muv, nDec);

	return muv;
}

//----------------------------------------------------------------------------
void round(double& val, int nDec)
{
	if (nDec < 0) return;

	long nPow = 1;

	for (int i = 0; i < nDec; i++) nPow *= 10;

	val *= nPow;

	//val = floor(fabs(val) + 0.5) * (val < 0. ? -1 : 1);
	val = val > 0. ? floor(val + 0.5) : ceil(val - 0.5);

	val /= nPow;
}

////////////////////////////////////////////////////////////////////////////////	                             
//							MM_TEXT
////////////////////////////////////////////////////////////////////////////////	                             
//----------------------------------------------------------------------------
int GetFontPointHeight(CDC* pDC, LOGFONT& lf, BOOL bPreview/* = FALSE*/)	//unscaling
{
	ASSERT_VALID(pDC);
	int fSize  = abs(MulDiv(lf.lfHeight, 72, GetLogPixels()));
	if (fSize < 0) fSize = fSize * -1;
	return fSize;
}

//-----------------------------------------------------------------------------
BOOL IsScale()
{
	return (GetLogPixels() != SCALING_FACTOR);
}

//-----------------------------------------------------------------------------
LONG  ScalePix(LONG px)
{
	if(!IsScale())
		return px;
	return abs(MulDiv(px, GetLogPixels(), SCALING_FACTOR));
}


//-----------------------------------------------------------------------------
LONG UnScalePix(LONG px)
{
	if (!IsScale())
		return px;
	return abs(MulDiv(px, SCALING_FACTOR, GetLogPixels()));
}

//-----------------------------------------------------------------------------
CRect ScaleRect(CRect rect)
{
	if (!IsScale())
		return rect;

	rect.top = abs(MulDiv(rect.top, GetLogPixels(), SCALING_FACTOR));
	rect.left = abs(MulDiv(rect.left, GetLogPixels(), SCALING_FACTOR));
	rect.bottom = abs(MulDiv(rect.bottom, GetLogPixels(), SCALING_FACTOR));
	rect.right = abs(MulDiv(rect.right, GetLogPixels(), SCALING_FACTOR));
	return rect;
}

//-----------------------------------------------------------------------------
TB_EXPORT void ScaleFrame(CFrameWnd* hWnd, BOOL center /*= TRUE*/)
{
	if (!IsScale() || hWnd == NULL)
		return;

	CRect rect;
	hWnd->GetWindowRect(rect);
	rect = ScaleRect(rect);
	hWnd->SetWindowPos(NULL, rect.left, rect.top, rect.Width(), rect.Height(), SWP_SHOWWINDOW);

	if (center)
		hWnd->CenterWindow();
}

//-----------------------------------------------------------------------------
int ScaleHeightFont(int lfHeight, const CDC& dc)
{
	return MulDiv(lfHeight, dc.GetDeviceCaps(LOGPIXELSY), (/*dc.IsPrinting() ? 300 : */SCALING_FACTOR));
}

//-----------------------------------------------------------------------------
void ScaleLogFont(LPLOGFONT plf, const CDC& dc)
{
	plf->lfHeight = MulDiv(plf->lfHeight, dc.GetDeviceCaps(LOGPIXELSY), SCALING_FACTOR);
	plf->lfWidth = MulDiv(plf->lfWidth, dc.GetDeviceCaps(LOGPIXELSX), SCALING_FACTOR);
}

//-----------------------------------------------------------------------------
void ScaleSize(CSize& size, const CDC& dc)
{
	size.cy = MulDiv(size.cy, dc.GetDeviceCaps(LOGPIXELSY), SCALING_FACTOR);
	size.cx = MulDiv(size.cx, dc.GetDeviceCaps(LOGPIXELSX), SCALING_FACTOR);
}

//-----------------------------------------------------------------------------
void UnScaleSize(CSize& size, const CDC& dc)
{
	size.cy = MulDiv(size.cy, SCALING_FACTOR, dc.GetDeviceCaps(LOGPIXELSY));
	size.cx = MulDiv(size.cx, SCALING_FACTOR, dc.GetDeviceCaps(LOGPIXELSX));
}

//-----------------------------------------------------------------------------
void UnScalePoint(CPoint& point, const CDC& dc)
{
	point.y = MulDiv(point.y, SCALING_FACTOR, dc.GetDeviceCaps(LOGPIXELSY));
	point.x = MulDiv(point.x, SCALING_FACTOR, dc.GetDeviceCaps(LOGPIXELSX));
}

//-----------------------------------------------------------------------------
void ScalePoint(CPoint& point, const CDC& dc)
{
	point.y = MulDiv(point.y, dc.GetDeviceCaps(LOGPIXELSY), SCALING_FACTOR);
	point.x = MulDiv(point.x, dc.GetDeviceCaps(LOGPIXELSX), SCALING_FACTOR);
}

//-----------------------------------------------------------------------------
void ScaleRect(CRect& rect, const CDC& dc)
{
	rect.top = MulDiv(rect.top, dc.GetDeviceCaps(LOGPIXELSY), SCALING_FACTOR);
	rect.bottom = MulDiv(rect.bottom, dc.GetDeviceCaps(LOGPIXELSY), SCALING_FACTOR);
	rect.left = MulDiv(rect.left, dc.GetDeviceCaps(LOGPIXELSX), SCALING_FACTOR);
	rect.right = MulDiv(rect.right, dc.GetDeviceCaps(LOGPIXELSX), SCALING_FACTOR);
}

//-----------------------------------------------------------------------------
void UnScaleRect(CRect& rect, const CDC& dc)
{
	rect.top = MulDiv(rect.top, SCALING_FACTOR, dc.GetDeviceCaps(LOGPIXELSY));
	rect.bottom = MulDiv(rect.bottom, SCALING_FACTOR, dc.GetDeviceCaps(LOGPIXELSY));
	rect.left = MulDiv(rect.left, SCALING_FACTOR, dc.GetDeviceCaps(LOGPIXELSX));
	rect.right = MulDiv(rect.right, SCALING_FACTOR, dc.GetDeviceCaps(LOGPIXELSX));
}

//-----------------------------------------------------------------------------
void DpiConvertPoint(CPoint& point, const INT DpiSource, const INT DpiDest)
{
	point.y = (int) ceil((point.y * DpiDest) / DpiSource);
	point.x = (int) ceil((point.x * DpiDest) / DpiSource);
}

//-----------------------------------------------------------------------------
void DpiConvertSize(CSize&   size, const INT DpiSource, const INT DpiDest)
{
	size.cy = (int) ceil((size.cy * DpiDest) / DpiSource);
	size.cx = (int) ceil((size.cx * DpiDest) / DpiSource);
}

//------------------------------------------------------------------------------
// ritorna il delta tra il rettangolo contenente l'ellisse e quello contenuto 
// rispetto all'asse desiderato
int NormalizeRect(int nRatio, const CRect& rect, BOOL bHorz/* = TRUE*/)
{
	int size = bHorz ? rect.Width() : rect.Height();

	return (int)ceil(min(nRatio, size) * 0.1464);
}

///////////////////////////////////////////////////////////////////////////////

int GetDisplayFontHeight(int nPointSize)
{
	HDC hdc = ::CreateDC(_T("DISPLAY"), NULL, NULL, NULL);
	ASSERT(hdc);
	int cyPixelsPerInch = ::GetDeviceCaps(hdc, LOGPIXELSY);
	::DeleteDC(hdc);

	int nHeight = -MulDiv(nPointSize, cyPixelsPerInch, 72);

	return nHeight;
}

int GetDisplayFontPointSize(int nHeight)
{
	HDC hdc = ::CreateDC(_T("DISPLAY"), NULL, NULL, NULL);
	ASSERT(hdc);
	int cyPixelsPerInch = ::GetDeviceCaps(hdc, LOGPIXELSY);
	::DeleteDC(hdc);

	int nPointSize = MulDiv(nHeight, 72, cyPixelsPerInch);
	if (nPointSize < 0)
		nPointSize = -nPointSize;

	return nPointSize;
}


//=======================================================================
//	File Management
//=======================================================================

//------------------------------------------------------------------------------
BOOL CopyFile(const CString& strSource, const CString& strTarget, BOOL bShowMessage /*= TRUE*/)
{
	return AfxGetFileSystemManager()->CopyFile(strSource, strTarget, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CopyFolderTree(const CString& strOrigin, const CString& strDestination, BOOL bOverWrite /*= FALSE*/, BOOL bShowMessage /*= FALSE*/)
{
	return AfxGetFileSystemManager()->CopyFolder(strOrigin, strDestination, bOverWrite, TRUE);
}

//rinomina un file o una directory
//-------------------------------------------------------------------------------------
BOOL RenameFilePath(const CString& strOldPath, const CString& strNewPath, BOOL bShowMessage /*=TRUE*/)
{
	return AfxGetFileSystemManager()->RenameFile(strOldPath, strNewPath);
}
//-------------------------------------------------------------------------------------
void GetAllFiles(CString sPath, CString sExt, BOOL bRecursive, CStringArray& files)
{
	CString search_path = sPath + _T("*.*");
	WIN32_FIND_DATA fd;
	HANDLE hFind;
	if ((hFind = ::FindFirstFile((LPCTSTR)search_path, &fd)) != INVALID_HANDLE_VALUE)
		do {
			if ((fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)) {
				if (!bRecursive || _tcscmp(fd.cFileName, L".") == 0 || _tcscmp(fd.cFileName, L"..") == 0)
					continue;
				GetAllFiles(sPath + fd.cFileName + L"\\", sExt, bRecursive, files);
			}
			else if (GetExtension(fd.cFileName) == sExt)
				files.Add(sPath + fd.cFileName);
		} while (::FindNextFile(hFind, &fd));
		::FindClose(hFind);
}


//-----------------------------------------------------------------------------
void ReplaceFileInvalidChars(CString& sPath, TCHAR replace)
{
	CString illegalChars = L"\\/:?\"<>|";

	TCHAR* p = sPath.GetBuffer();
	while (*p)
	{
		if (illegalChars.Find(*p) > -1)
			*p = replace;
		p++;
	}
	sPath.ReleaseBuffer();
}

//-----------------------------------------------------------------------------
// Tries to create a temporary file in specified path to check the access this path
//-----------------------------------------------------------------------------
BOOL CheckDirectoryAccess(const CString& sPath)
{
	CString strTempFileName;
	GetTempFileName(sPath, _T("Tmp"), 0, strTempFileName.GetBuffer(MAX_PATH));
	strTempFileName.ReleaseBuffer();

	CLineFile file;

	TRY
	{
		if (!file.Open(strTempFileName, CFile::modeCreate | CFile::modeWrite | CFile::typeText))
			return FALSE;

		file.WriteString(_T("temporary string"));
		file.Close();
		DeleteFile(strTempFileName);
	}
	CATCH(CException, e)
	{
		return FALSE;
	}
	END_CATCH

	return TRUE;
}

//=================================================================================
// String crypt/decrypt
//=================================================================================
CString TbCryptString(CString str, LPCTSTR pszAux/*=NULL*/)
{
	if (str.IsEmpty())
		return _T("");

	CByteArray data;
	CCrypto c;
	if (c.HasErrors())
	{
		AfxGetDiagnostic()->Add(c.m_arErrors);
		return _T("");
	}
	c.DeriveKey(pszAux);
	if (c.HasErrors())
	{
		AfxGetDiagnostic()->Add(c.m_arErrors);
		return _T("");
	}

	bool ok = c.Encrypt(str, data);
	if (!ok || c.HasErrors())
	{
		AfxGetDiagnostic()->Add(c.m_arErrors);
		return _T("");
	}

	int nCount = Base64EncodeGetRequiredLength(data.GetCount());
	char* szBuff = (char*)_malloca(nCount + 1);//alloco memoria nello stack (NOTA: in DEBUG la funzione alloca sempre sullo HEAP)

	BOOL bOk = Base64Encode(data.GetData(), data.GetCount(), szBuff, &nCount);
	if (!bOk)
	{
		AfxGetDiagnostic()->Add(_T("Base64Encode crypted string failed"));
		return _T("");
	}

	szBuff[nCount] = '\0';
	USES_CONVERSION;
	CString ret = A2T(szBuff);

	_freea(szBuff); //rilascio la memoria

	return ret;
}

//----------------------------------------------------------------------------------
CString TbDecryptString(CString strHexCrypted, LPCTSTR pszAux/*=NULL*/)
{
	if (strHexCrypted.IsEmpty())
		return _T("");

	USES_CONVERSION;
	CStringA s = T2A(strHexCrypted);

	int numBytes = Base64DecodeGetRequiredLength(s.GetLength());

	BYTE *buffer = (BYTE *)strHexCrypted.GetBuffer();
	CByteArray data;
	data.SetSize(numBytes);

	BOOL bOk = Base64Decode(s, s.GetLength(), data.GetData(), &numBytes);
	if (!bOk)
	{
		AfxGetDiagnostic()->Add(_T("Base64Decode decrypted string failed"));
		return _T("");
	}
	data.SetSize(numBytes);

	CString strDecrypted;
	CCrypto c;
	c.DeriveKey(pszAux);
	if (c.HasErrors())
	{
		AfxGetDiagnostic()->Add(c.m_arErrors);
		return _T("");
	}

	bool ok = c.Decrypt(data, strDecrypted);
	if (!ok || c.HasErrors())
	{
		AfxGetDiagnostic()->Add(c.m_arErrors);
		return _T("");
	}

	return strDecrypted;
}

//=================================================================================
// strings functions
//=================================================================================



//-----------------------------------------------------------------------------
CString FixBareLF(const CString &str)
{
	int index = 0;
	CString strResult = str;
	while ((index = strResult.Find(LF_CHAR, index)) != -1)
	{
		if (index && strResult[index - 1] != CR_CHAR)
		{
			strResult.Insert(index, CR_CHAR);
			index++;
		}
		index++;
	}

	return strResult;
}

template <class T>
CString FormatParamT(const CString& strParam, va_list& marker)
{
	CString ret;
	T v = va_arg(marker, T);
	ret.Format(strParam, v);
	ret.Replace(_T("%"), _T("%%"));
	return ret;
}
template <class T>
CString FormatParamT1(const CString& strParam, va_list& marker)
{
	int i = va_arg(marker, int);
	CString ret;
	T v = va_arg(marker, T);
	ret.Format(strParam, i, v);
	ret.Replace(_T("%"), _T("%%"));
	return ret;
}
template <class T>
CString FormatParamT2(const CString& strParam, va_list& marker)
{
	int i1 = va_arg(marker, int);
	int i2 = va_arg(marker, int);
	CString ret;
	T v = va_arg(marker, T);
	ret.Format(strParam, i1, i2, v);
	ret.Replace(_T("%"), _T("%%"));
	return ret;
}

CString FormatParam(const CString& strParam, int additionalIntParams, va_list& marker)
{
	CString ret;
	TCHAR type = strParam[strParam.GetLength() - 1];
	switch (type)
	{
	case 'c':
	case 'C':
	case 'd':
	case 'i':
	case 'o':
	case 'u':
	case 'x':
	case 'X':
		switch (additionalIntParams)
		{
		case 0:
			return FormatParamT<int>(strParam, marker);
		case 1:
			return FormatParamT1<int>(strParam, marker);
		case 2:
			return FormatParamT2<int>(strParam, marker);
		}
	case 'e':
	case 'E':
	case 'f':
	case 'g':
	case 'G':
	case 'a':
	case 'A':
		switch (additionalIntParams)
		{
		case 0:
			return FormatParamT<double>(strParam, marker);
		case 1:
			return FormatParamT1<double>(strParam, marker);
		case 2:
			return FormatParamT2<double>(strParam, marker);
		}
	case 'n':
		switch (additionalIntParams)
		{
		case 0:
			return FormatParamT<int*>(strParam, marker);
		case 1:
			return FormatParamT1<int*>(strParam, marker);
		case 2:
			return FormatParamT2<int*>(strParam, marker);
		}
	case 'p':
		switch (additionalIntParams)
		{
		case 0:
			return FormatParamT<void*>(strParam, marker);
		case 1:
			return FormatParamT1<void*>(strParam, marker);
		case 2:
			return FormatParamT2<void*>(strParam, marker);
		}
	case 's':
	case 'S':
		switch (additionalIntParams)
		{
		case 0:
			return FormatParamT<TCHAR*>(strParam, marker);
		case 1:
			return FormatParamT1<TCHAR*>(strParam, marker);
		case 2:
			return FormatParamT2<TCHAR*>(strParam, marker);
		}
	default:
		ASSERT(FALSE);
		return _T("");
	}
}

#define MAX_PARAMS 50
class CStringBuffer
{
public:
	CStringBuffer()
	{
		for (int i = 0; i < MAX_PARAMS; i++)
		{
			m_arParamStart[i] = 0;
			m_arParamEnd[i] = 0;
			m_arParamNumber[i] = 0;
			AdditionalIntParams[i] = 0;
			ParamToEvaluate[i] = false;
		}
	}
	int m_arParamStart[MAX_PARAMS]; //usato per tenere traccia della posizione del parametro nella stringa
	int m_arParamEnd[MAX_PARAMS]; //usato per tenere traccia della posizione del parametro nella stringa
	int m_arParamNumber[MAX_PARAMS]; //usato per sapere il numero del parametro nella stringa

	CString m_arParamValues[MAX_PARAMS];//usato per decodificare i valori dei parametri
	int AdditionalIntParams[MAX_PARAMS];//usato per decodificare i valori dei parametri
	bool ParamToEvaluate[MAX_PARAMS];//usato per decodificare i valori dei parametri

	CString m_sFormattedString;		//usata per allocare il buffer che contiene i caratteri
};

DECLARE_THREAD_VARIABLE(CStringBuffer, t_StringBuffer);

/*
Questa funzione prende una stringa con dei placeholder di formattazione ed un elenco variabile di parametri
contenenti i dati da formattare; il pattern di formattazione e' simile a quello della funzione printf, ma racchiude
i placeholder fra graffe prefissandoli con un numero posizionale (es. {0-%s} per formattare una stringa: lo 0 indica che
andra` formattato il primo parametro)

Internamente questa funzione effettua le seguente operazioni:
1)	parsa tutta la stringa utilizzando una macchina a stati per individuare tutti i parametri passati; memorizza in una
struttura parallela gli indici di inizio e fine del parametro ed il suo numero di posizione, nonche' i caratteri di formattazione
'puri', escludendo graffe e numero di posizione
2)	formatta i parametri cosi' individuati, ordinandoli prima per numero di posizione, usando le api del C++ e MFC
3)	ricostruisce una stringa parallela, sulla base delle informazioni raccolte negli step precedenti, che a questo punto sara' formattata

La ricostruzione della stringa parallela utilizza un buffer preallocato (se troppo piccolo viene allargato e rimane tale a beneficio
di successive chiamate) per motivi di efficienza
*/
//-----------------------------------------------------------------------------
CString cwvsprintf(const TCHAR *str, va_list marker)
{
	const TCHAR *szOriginal = str;
	try
	{
		ASSERT_TRACE(str && str[0], "parameter str is either null or empty");
		if (str == NULL || str[0] == 0)
			return _T("");

		//recupero l'area di lavoro di thread per migliorare l'efficienza
		GET_THREAD_VARIABLE(CStringBuffer, t_StringBuffer);

		int nOriginalLength = _tcslen(szOriginal);
		int nBufferLength = (int)(nOriginalLength * 1.5); //alloco un po' di piu' per i parametri

		int pos = 0;
		int nParam = 0;
		enum state
		{
			None, FoundBrace, FoundNumber, FoundSeparator, FoundToken
		}
		currState;

		CString strNumber, strParam;
		currState = None;
		TCHAR ch;
		int startOfParam = 0, additionalIntParams = 0;
		while (ch = str[0])
		{
			// se trovo una { potrei aver trovato un formattatore
			// mi aspetto successivamente un numero
			if (ch == OPEN_CURLY_CHAR)
			{
				currState = FoundBrace;
				strParam.Empty();
				strNumber.Empty();
				additionalIntParams = 0;
			}

			// se trovo un numero, potrebbe essere:
			//	- il progressivo del formattatore (se lo stato è FoundBrace o FoundNumber)
			//	- parte del formattatore stesso (se lo stato è FoundToken)
			//	- un carattere non rilevante in ogni altro caso
			else if (_istdigit(ch))
			{
				if (currState == FoundBrace)
				{
					//siccome prima ho trovato una graffa, sicuramente si tratta di un parametro che inizia alla pos. precedente
					startOfParam = pos - 1;
					currState = FoundNumber;
					strNumber += ch;
				}
				else if (currState == FoundNumber)
				{
					currState = FoundNumber;
					strNumber += ch;
				}
				else if (currState == FoundToken)
				{
					strParam += ch;
				}
				else
				{
					currState = None;
				}
			}
			// se trovo un -, potrebbe essere:
			//	- il separatore fra progressivo e formattatore (se lo stato è FoundNumber)
			//	- parte del formattatore stesso (se lo stato è FoundToken)
			//	- un carattere non rilevante in ogni altro caso
			else if (ch == MINUS_CHAR)
			{
				if (currState == FoundNumber)
				{
					currState = FoundSeparator;
				}
				else if (currState == FoundToken)
				{
					strParam += ch;
				}
				else
				{
					currState = None;
				}
			}
			// se trovo un %, potrebbe essere:
			//	- l'inizio del formattatore (se lo stato è FoundSeparator)
			//	- parte del formattatore stesso (se lo stato è FoundToken)
			//	- un carattere non rilevante in ogni altro caso
			else if (ch == PERCENT_CHAR)
			{
				if (currState == FoundSeparator)
				{
					currState = FoundToken;
					strParam += ch;
				}
				else if (currState == FoundToken)
				{
					strParam += ch;
				}
				else
				{
					currState = None;
				}
			}
			// se trovo un %, potrebbe essere:
			//	- l'inizio del formattatore (se lo stato è FoundSeparator)
			//	- parte del formattatore stesso (se lo stato è FoundToken)
			//	- un carattere non rilevante in ogni altro caso
			else if (ch == ASTERISK_CHAR)
			{
				if (currState == FoundToken)
				{
					additionalIntParams++;
					strParam += ch;
				}
			}
			// se trovo un }, potrebbe essere:
			//	- la fine del formattatore (se lo stato è FoundToken); in questo caso, aggiungo il formattatore trovato all'array
			//	- un carattere non rilevante in ogni altro caso
			else if (ch == CLOSE_CURLY_CHAR)
			{
				if (currState == FoundToken)
				{
					currState = None;
					int paramNumber = _ttoi(strNumber);

					if (nParam >= MAX_PARAMS || paramNumber >= MAX_PARAMS)
					{
						ASSERT(FALSE);
						break;
					}
					t_StringBuffer.m_arParamStart[nParam] = startOfParam;
					t_StringBuffer.m_arParamEnd[nParam] = pos;
					t_StringBuffer.m_arParamNumber[nParam] = paramNumber;
					t_StringBuffer.m_arParamValues[paramNumber] = strParam;
					t_StringBuffer.AdditionalIntParams[paramNumber] = additionalIntParams;
					t_StringBuffer.ParamToEvaluate[paramNumber] = true;

					nParam++;
				}
				else
				{
					currState = None;
				}
			}
			// ogni altro carattere potrebbe essere:
			//	- parte del formattatore (se lo stato è FoundToken)
			//	- un carattere non rilevante in ogni altro caso
			else
			{
				if (currState == FoundToken)
				{
					strParam += ch;
				}
				else
				{
					currState = None;
				}
			}

			str++;
			pos++;
		}

		if (nParam == 0) //nessun parametro con graffe? provo a formattare in modo classico
		{
			CString s;
			s.FormatV(szOriginal, marker);
			return s;
		}

		int nFormatted = 0;
		//vado in ordine DI NUMERO del parametro, non nell'ordine in cui li trovo nella stringa!
		for (int i = 0; i < nParam; i++)
		{
			if (i >= MAX_PARAMS)
			{
				ASSERT(FALSE);
				break;
			}
			int number = t_StringBuffer.m_arParamNumber[i];
			if (t_StringBuffer.ParamToEvaluate[number])
			{
				if (t_StringBuffer.m_arParamValues[nFormatted].IsEmpty())
				{
					ASSERT_TRACE1(FALSE, "Wrong parameters for string '%s'", szOriginal);
					return szOriginal;
				}
				t_StringBuffer.m_arParamValues[nFormatted] = FormatParam(t_StringBuffer.m_arParamValues[nFormatted], t_StringBuffer.AdditionalIntParams[nFormatted], marker);
				t_StringBuffer.ParamToEvaluate[number] = false;
				nFormatted++;
			}
		}

	start:
		pos = 0;
		int nChars = 0;
		int nCharsToWrite = 0;
		//alloco il buffer di dimensioni adeguate (1 carattere per il terminatore)
		TCHAR* pNewStr = t_StringBuffer.m_sFormattedString.GetBuffer(nBufferLength + 1);

		TCHAR* pS = pNewStr;
		//adesso vado in sequenza sulla stringa originaria e creo una stringa parallela in cui sostituisco i
		//segnaposto dei parametri con i valori calcolati nel loop precedente
		for (int i = 0; i < nParam; i++)
		{
			nChars = t_StringBuffer.m_arParamStart[i] - pos;

			nCharsToWrite += nChars;
			//non ho piu` spazio: lo rialloco e ricomincio da capo (tanto il buffer sara' riutilizzato in seguito)
			if (nCharsToWrite > nBufferLength)
			{
				nBufferLength = (int)(nCharsToWrite * 1.2);//incremento lo spazio del 20%
				goto start;
			}

			//scrivo la parte costante che precede il parametro
			memcpy_s(pS, nBufferLength*sizeof(TCHAR), szOriginal + pos, nChars*sizeof(TCHAR));
			pS += nChars;

			CString& strParam = t_StringBuffer.m_arParamValues[t_StringBuffer.m_arParamNumber[i]];
			nChars = strParam.GetLength();

			nCharsToWrite += nChars;
			//non ho piu` spazio: lo rialloco e ricomincio da capo (tanto il buffer sara' riutilizzato in seguito)
			if (nCharsToWrite > nBufferLength)
			{
				nBufferLength = (int)(nCharsToWrite * 1.2);//incremento lo spazio del 20%
				goto start;
			}
			//scrivo il parametro
			memcpy_s(pS, nBufferLength*sizeof(TCHAR), strParam, nChars*sizeof(TCHAR));
			pS += nChars;

			pos = t_StringBuffer.m_arParamEnd[i] + 1;
		}

		nChars = nOriginalLength - pos;
		nCharsToWrite += nChars;
		//non ho piu` spazio: lo rialloco e ricomincio da capo (tanto il buffer sara' riutilizzato in seguito)
		if (nCharsToWrite > nBufferLength)
		{
			nBufferLength = (int)(nCharsToWrite * 1.2);//incremento lo spazio del 20%
			goto start;
		}
		//scrivo la parte costante terminale della stringa
		memcpy_s(pS, nBufferLength*sizeof(TCHAR), szOriginal + pos, nChars*sizeof(TCHAR));
		pS += nChars;
		*pS = _T('\0');

		t_StringBuffer.m_sFormattedString.ReleaseBuffer();

		return t_StringBuffer.m_sFormattedString;
	}
	catch (CParameterException* pException)
	{
		//non faccio cwsprintf o CString::Format per non entrare in loop
		AfxGetDiagnostic()->Add(_TB("Error formatting string"), CDiagnostic::Warning);
		//AfxGetDiagnostic()->Add(szOriginal, CDiagnostic::Warning), CDiagnostic::Warning;
		AfxGetDiagnostic()->Add(pException, szOriginal, CDiagnostic::Warning);
		pException->Delete();
	}
	catch (...)
	{
		//non faccio cwsprintf o CString::Format per non entrare in loop
		AfxGetDiagnostic()->Add(_TB("Error formatting string"), CDiagnostic::Warning);
		AfxGetDiagnostic()->Add(szOriginal, CDiagnostic::Warning);
	}
	ASSERT(FALSE);
	return szOriginal;		//ritorno la stringa non formattata
}

//-----------------------------------------------------------------------------
CString cwsprintf(LPCTSTR strFmt, ...)
{
	if (strFmt == NULL || *strFmt == '\0')
	{
		ASSERT_TRACE(strFmt != NULL && *strFmt != '\0', "parameter strFmt must be not null and not empty");
		return _T("");
	}
	va_list marker;

	va_start(marker, strFmt);
	CString strBuffer = cwvsprintf(strFmt, marker);
	va_end(marker);

	return strBuffer;
}

//-----------------------------------------------------------------------------
int GetNumDecimals(double nVal, int nRequestedDecimals /* = 15 */)
{
	// Disabilita la warning sulla perdita di precisione nella conversione tra double e int
	// Infatti si vuole proprio ottenere la parte intera del logaritmo per calcolare il numero
	// di cifre significative prima della virgola
#pragma warning(disable:4244)
	int nBeforeComma = min(max(0, log10(nVal) + 1), 15);
#pragma warning(default:4244)

	// In molti punti di TB, usare un nr. di decimali pari a -1, significa "decimali di default"
	// Il numero di decimali usato per default dalla printf e` 6
	if (nRequestedDecimals < 0)
		nRequestedDecimals = 6;

	// Fa in modo che il numero totale di cifre formattate (prima e dopo la virgola) sia al massimo 15,
	// così evita effetti visivi indesiderati che si hanno formattando un numero di cifre significative
	// superiori a quelle gestite dal double
	return min(nRequestedDecimals, 15 - nBeforeComma);
}

//-----------------------------------------------------------------------------
CString dblcwsprintf(double nVal, int nLen /*= -1*/, int nDec /*= -1*/)
{
	if (nLen <= 0) // nessuna impostazione sulla dimensione dell'ouput formattato
		return cwsprintf(_T("%.*f"), GetNumDecimals(nVal, nDec), nVal);
	else
		return cwsprintf(_T("%*.*f"), nLen, GetNumDecimals(nVal, nDec), nVal);
}

//-----------------------------------------------------------------------------
CString dblsprintf(double nVal, int nDec /*= -1*/)
{
	return cwsprintf(_T("%.*f"), GetNumDecimals(nVal, nDec), nVal);
}

//-----------------------------------------------------------------------------
void StripBlank(TCHAR* pszBuf)
{
	int i = _tcslen(pszBuf) - 1;
	while (i >= 0 && pszBuf[i] == BLANK_CHAR) i--;
	pszBuf[i + 1] = NULL_CHAR;
}

//-----------------------------------------------------------------------------
CString& StripBlank(CString& s)
{
	s.TrimRight();
	return s;
}

// tolgo dalla stringa i caratteri inseriti nel set passato come secondo parametro
//--------------------------------------------------------------------------
CString RemoveChars(const CString& strSource, const CString& strCharToRemove)
{
	CString strDest;
	//scorro la stringa origine
	for (int n = 0; n < strSource.GetLength(); n++)
	{
		//se trovo un carattere che non e' da escludere lo aggiungo alla stringa origine.
		if (strCharToRemove.Find(strSource[n], 0) == -1)
			strDest += strSource[n];
	}

	return strDest;
}

//-----------------------------------------------------------------------------
int FindOneOf(const CString& sString, LPCTSTR lpszCharSet, int nStart)
{
	wstring s;
	s = sString;
	return s.find_first_of(lpszCharSet, nStart);
}

//-----------------------------------------------------------------------------
int ReverseFindOneOf(const CString& sString, LPCTSTR lpszCharSet, int nStart)
{
	wstring s;
	s = sString;
	return s.find_last_of(lpszCharSet, nStart);
}

//-----------------------------------------------------------------------------
int FindOccurence(CString sString, CString sSub, int nOccurence, int nStartIndex/* = 0*/, BOOL bNoCase/*= FALSE*/)
{
	ASSERT(nOccurence >= 1 && nStartIndex >= 0);

	int idx = bNoCase ?
					::FindNoCase(sString, sSub, nStartIndex)
					:
					sString.Find(sSub, nStartIndex);

	for (; idx >= 0 && nOccurence > 1; nOccurence--)
	{
		nStartIndex = idx + sSub.GetLength();

		idx = bNoCase ?
					::FindNoCase(sString, sSub, nStartIndex)
					: 
					sString.Find(sSub, nStartIndex);
	}

	return idx;
}

//-----------------------------------------------------------------------------
int	ReverseFind(CString sString, CString sSubString, int nStartIndex/* = -1*/, int nOccurence/* = 1*/, BOOL bNoCase/*= FALSE*/)
{
	if (nStartIndex < 0)
		nStartIndex = sString.GetLength() - 1;

	sString.MakeReverse();
	sSubString.MakeReverse();

	nStartIndex = sString.GetLength() - nStartIndex - 1;

	int idx = ::FindOccurence(sString, sSubString, nOccurence, nStartIndex, bNoCase);

	if (idx >= 0)
		idx = sString.GetLength() - idx - sSubString.GetLength();

	return idx;
}

//-----------------------------------------------------------------------------
int FindNoCase(CString s, CString sub, int startIndex /*= 0*/)
{
	s.MakeLower();
	sub.MakeLower();
	return s.Find(sub, startIndex);
}

//-----------------------------------------------------------------------------
int FindWord(CString s, CString sub, BOOL noCase/* = TRUE*/, int startIndex /*= 0*/)
{
	if (noCase)
	{
		s.MakeLower();
		sub.MakeLower();
	}

	while((startIndex + sub.GetLength()) <= s.GetLength())
	{	
		int idx = s.Find(sub, startIndex);
		if (idx < 0)
			break;

		startIndex = idx + sub.GetLength();

		if (idx > 0 && IsCharAlphaNumeric(s[idx - 1]))
			continue;

		if (startIndex < s.GetLength() && IsCharAlphaNumeric(s[startIndex]))
			continue;

		return idx;
	}
	return -1;
}

int ReverseFindWord(CString source, CString word, BOOL noCase/* = TRUE*/, int startIndex /*= -1*/)
{
	if (startIndex < 0)
		startIndex = source.GetLength() - 1;

	source.MakeReverse();
	word.MakeReverse();

	startIndex = source.GetLength() - startIndex - 1;

	return FindWord(source, word, noCase, startIndex);
}

//-----------------------------------------------------------------------------
int CountChars(const CString& s, TCHAR c, int startIndex/* = 0*/, int endIndex/* = -1*/)
{
	if (endIndex == -1) endIndex = s.GetLength() - 1;
	int start = startIndex;
	int count = 0;

	int newl = 0;
	while (newl >= 0)
	{
		newl = s.Find(c, start);
		if (newl < 0) break;
		count++;
		start = newl + 1;
		if (start > endIndex) break;
	}
	return count;
}

//-----------------------------------------------------------------------------
CString& ReplaceNoCase(CString& str, CString sFrom, const CString& sTo, int startIndex /*= 0*/, int nOccurence /*= 0*/)
{
	sFrom.MakeLower();
	CString s(str);	s.MakeLower();
	BOOL bOne = nOccurence > 0;

	int idx = s.Find(sFrom, startIndex); nOccurence--;

	int diff = 0;
	while (idx > -1)
	{
		startIndex = idx + sFrom.GetLength();

		if (!bOne || (bOne && nOccurence == 0))
			str = (str.Left(idx + diff) + sTo + str.Mid(startIndex + diff));

		if (bOne && nOccurence == 0)
			break;

		idx = s.Find(sFrom, startIndex); nOccurence--;

		if (!bOne)
			diff += sTo.GetLength() - sFrom.GetLength();
	}
	return str;
}

//-----------------------------------------------------------------------------
bool WildcardMatch(LPCTSTR pszString, LPCTSTR pszMatch)
{
	const TCHAR *mp;
	const TCHAR *cp = NULL;

	while (*pszString)
	{
		if (*pszMatch == _T('*'))
		{
			if (!*++pszMatch)
				return true;
			mp = pszMatch;
			cp = pszString + 1;
		}
		else if (*pszMatch == _T('?') || _totupper(*pszMatch) == _totupper(*pszString))
		{
			pszMatch++;
			pszString++;
		}
		else if (!cp)
			return false;
		else
		{
			pszMatch = mp;
			pszString = cp++;
		}
	}

	while (*pszMatch == _T('*'))
		pszMatch++;

	return !*pszMatch;
}

//----------------------------------------------------------------------------
BOOL IsValidName (const CString& name)
{
	if (name.GetLength() == 0 || (!::isalpha(name[0]) && name[0] != L'_'))
		return FALSE;

	for (int i = 1; i < name.GetLength(); i++)
	{
		if (!::isalnum(name[i]) && name[i] != L'_')
			return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
CString& StrAppend(CString& str, LPCTSTR sToAdd, BOOL space, BOOL newLine)
{
	str += sToAdd;
	if (space) 		str += " ";
	if (newLine)	str += LF_CHAR;
	return str;
}

//----------------------------------------------------------------------------
CString& RemoveCR(CString& str)
{
	str.Remove('\r');
	return str;
}

CString& AddCR(CString& str)
{
	str.Remove('\r');
	str.Replace(L"\n", L"\r\n");
	return str;
}

//-----------------------------------------------------------------------------
CString& StripBlankNearSquareBrackets(CString& str)
{
	while (str.Replace(_T("[ "), _T("[")) > 0);
	while (str.Replace(_T(" ]"), _T("]")) > 0);
	return str;
}

//-----------------------------------------------------------------------------
LPCTSTR PCause(CFileException* e)
{
	if (e->m_cause >= 0 &&
		e->m_cause < sizeof(rgszCFileExceptionCause) / sizeof(TCHAR FAR*))
	{
		return rgszCFileExceptionCause[e->m_cause];
	}
	else
	{
		return szUnknown;
	}
}

//-----------------------------------------------------------------------------
CString GuidToString(REFGUID aGuid)
{
	const rsize_t nLen = 256;
	TCHAR szGUID[nLen];
	_sntprintf_s
		(
		szGUID, nLen, sizeof szGUID,
		_T("{%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X}"),
		aGuid.Data1, aGuid.Data2, aGuid.Data3,
		aGuid.Data4[0], aGuid.Data4[1], aGuid.Data4[2], aGuid.Data4[3],
		aGuid.Data4[4], aGuid.Data4[5], aGuid.Data4[6], aGuid.Data4[7]
		);

	return szGUID;
}

//-----------------------------------------------------------------------------
GUID StringToGuid(LPCTSTR pszGUID)
{
	if (!pszGUID)
		return NULL_GUID;

	GUID aGuid = NULL_GUID;

	int nConv = _stscanf_s(pszGUID, _T("{%8lX-%4hX-%4hX-}"), &aGuid.Data1, &aGuid.Data2, &aGuid.Data3);
	for (int i = 0; i < 8; i++)
	{
		int nBuffer;
		int nOffset = 20 + 2 * i; if (i > 1) nOffset++;

		nConv = _stscanf_s(pszGUID + nOffset, _T("%2X"), &nBuffer);
		aGuid.Data4[i] = (unsigned char)nBuffer;
	}
	return aGuid;
}

//=============================================================================
CString HTMLEncode(const CString & strString)
{
	CString strResult;
	for (int nRep = 0; nRep < strString.GetLength(); nRep++)
	{
		TCHAR car = strString[nRep];
		if (car < 128)
		{
			switch (car)
			{
			case '!':
			case '*':
			case '\'':
			case '(':
			case ')':
			case ';':
			case ':':
			case '@':
			case '&':
			case '=':
			case '+':
			case '$':
			case ',':
			case '/':
			case '?':
			case '#':
			case '[':
			case ']':
			case '<':
			case '>':
			case '~':
			case '.':
			case '"':
			case '{':
			case '}':
			case '|':
			case '\\':
			case '-':
			case '`':
			case '_':
			case '^':
			case '%':
			case ' ':
			{
				CString strBuffer;
				//strBuffer.Format(_T("&#%d;"),car);
				strBuffer.Format(_T("%%%2x"), car);
				strResult += strBuffer;
				break;
			}
			default:
				strResult += car;
			}
		}
		else
		{
			CString strBuffer;
			//strBuffer.Format(_T("%%u%4x"), car);	//rejected by W3C
			if (car <= 0x07FF)
			{
				BYTE b1 = 0xC0 | (car >> 6);
				BYTE b2 = 0x80 | (car & 0x3F);
				strBuffer.Format(_T("%%%2x%%%2x"), b1, b2);
			}
			else // 0x7FF < car <= 0xFFFF
			{
				BYTE b1 = 0xE0 | (car >> 12);
				BYTE b2 = 0x80 | ((car >> 6) & 0x3F);
				BYTE b3 = 0x80 | (car & 0x3F);

				strBuffer.Format(_T("%%%2x%%%2x%%%2x"), b1, b2, b3);
			}
			strResult += strBuffer;
		}
	}
	return strResult;
}

//-----------------------------------------------------------------------------
BOOL IsExtAlphaNumeric(int ch)
{
	return _istalnum(ch) || (ch >= 128 && ch <= 165);
}

//-----------------------------------------------------------------------------
BOOL IsExtAlpha(int ch)
{
	return _istalpha(ch) || (ch >= 128 && ch <= 165);
}

//-----------------------------------------------------------------------------
TB_EXPORT BOOL	IsIntNumber(LPCTSTR s)
{
	size_t l = _tcslen(s);
	return l == 0 || _tcsspn(s, _T("\t -+0123456789")) == l;
}

//-----------------------------------------------------------------------------
TB_EXPORT BOOL	IsFloatNumber(LPCTSTR s)
{
	size_t l = _tcslen(s);
	return l == 0 || _tcsspn(s, _T("\t eE-+.0123456789")) == l;
}

//-----------------------------------------------------------------------------
TB_EXPORT UINT GetActiveWindows(HWND hwnd, CPtrArray& arWindows)
{
	DWORD dwCurrPID = GetCurrentProcessId();

	arWindows.RemoveAll();

	DWORD dwPId;

	HWND h = FindWindowEx(hwnd, NULL, NULL, NULL);
	while (h)
	{
		GetWindowThreadProcessId(h, &dwPId);
		if (dwPId == dwCurrPID)
		{
			CWnd* pWnd = CWnd::FromHandle(h);
			if (
				pWnd->IsWindowEnabled()
				&& IsTBWindowVisible(pWnd)
				&& (pWnd->GetStyle() & WS_POPUP)
				)
				arWindows.Add(h);
		}

		h = FindWindowEx(hwnd, h, NULL, NULL);
	}

	return arWindows.GetCount();;
}

//-----------------------------------------------------------------------------
void SafeArrayToStringArray(SAFEARRAY *pInArray, CStringArray& arOutArray, BOOL bDestroyOriginal)
{
	long uBound;
	HRESULT hr = SafeArrayGetUBound(pInArray, 1, &uBound);
	BSTR val;

	for (long l = 0; l <= uBound; l++)
	{
		hr = SafeArrayGetElement(pInArray, &l, &val);
		arOutArray.Add(val);
		SysFreeString(val);
	}

	if (bDestroyOriginal)
		SafeArrayDestroy(pInArray);
}

//-----------------------------------------------------------------------------
UINT GetNumIDS(const UINT IdString)
{
	return (UINT)(IdString & 0x3FFF);
}

//-----------------------------------------------------------------------------
int CALLBACK EnumFontFamExProc(
	const LOGFONT    *lpelfe,
	const TEXTMETRIC *lpntme,
	DWORD      FontType,
	LPARAM     lParam
	)
{
	int* i = (int*)lParam;
	(*i)++;
	return *i;
}
//-----------------------------------------------------------------------------
BOOL IsFontInstalled(LPCTSTR szFaceName)
{
	static CCriticalSection g_sect;
	static CMap<CString, LPCTSTR, BOOL, BOOL> g_Fonts;
	CSingleLock l(&g_sect, TRUE);
	BOOL b;
	if (g_Fonts.Lookup(szFaceName, b))
		return b;

	LOGFONT lf;
	ZeroMemory(&lf, sizeof(LOGFONT));
	_tcscpy_s(lf.lfFaceName, szFaceName);
	lf.lfCharSet = DEFAULT_CHARSET;
	lf.lfPitchAndFamily = 0;
	int nCount = 0;
	HDC hdc = GetDC(NULL);
	int i = EnumFontFamiliesEx(hdc, &lf, EnumFontFamExProc, (LPARAM)&nCount, 0);
	ReleaseDC(NULL, hdc);
	b = (nCount > 0);
	g_Fonts.SetAt(szFaceName, b);
	return b;

}
//-----------------------------------------------------------------------------
void AddEnvironmentVariable(const CString& strName, const CString& strValue)
{
	CString var;
	GetEnvironmentVariable(strName, var.GetBuffer(32767), 32767);
	var.ReleaseBuffer();
	if (var.Find(strValue) != -1)
		return;

	var = strValue + _T(";") + var;
	SetEnvironmentVariable(strName, var);
}

//-----------------------------------------------------------------------------
BOOL InternalIsTBWindowVisible(CWnd* pWnd)
{
	BOOL bVisible = (pWnd->GetStyle() & WS_VISIBLE) == WS_VISIBLE;
	if (!bVisible)
		return FALSE;

	CWnd* pParent = pWnd->GetParent();
	if (!pParent)
		return TRUE;
	return InternalIsTBWindowVisible(pParent);
}

//-----------------------------------------------------------------------------
BOOL IsTBWindowVisible(CWnd* pWnd)
{
	if (!pWnd)
		return FALSE;

	if (!AfxIsRemoteInterface())
		return pWnd->IsWindowVisible();

	return InternalIsTBWindowVisible(pWnd);
}

//----------------------------------------------------------------------------
void CEditGetSel(const CEdit& Ce, int& nStart, int& nStop)
{
	DWORD dw;

	dw = Ce.GetSel();
	nStart = (int)LOWORD(dw);
	nStop = (int)LOWORD(dw);
}

//----------------------------------------------------------------------------
void CEditReplaceSel(CEdit& Ce, LPCTSTR str)
{
	int nStart, nStop;

	::CEditGetSel(Ce, nStart, nStop);

	int len = _tcslen(str);
	Ce.ReplaceSel(str);

	Ce.SetFocus();
	Ce.SetSel(nStart + len, nStart + len);
}

//----------------------------------------------------------------------------
void CEditReplaceSelection(CEdit& Ce, CString str)
{
	int 	nStart, nStop, nLenFunc;
	::CEditGetSel(Ce, nStart, nStop);
	nLenFunc = str.GetLength();
	str += " ";
	::CEditReplaceSel(Ce, str);
	Ce.SetSel(nStart + nLenFunc + 1, nStart + nLenFunc + 1);
}

///////////////////////////////////////////////////////////////////////////////
COLORREF OppositeRGB(COLORREF rgb)
{
	WORD r = GetRValue(rgb);
	WORD g = GetGValue(rgb);
	WORD b = GetBValue(rgb);

	WORD nr = 255 - r;
	WORD ng = 255 - g;
	WORD nb = 255 - b;

	//Se è quasi un grigio mediano cerco di sfasarlo su una componente
	if (abs(r - nr) < 30 && abs(g - ng) < 30 && abs(b - nb) < 30)
	{
		if (abs(r - nr) < 30)
		{
			if (r < 128)
				nr = 255;
			else
				nr = 0;
		}
		else if (abs(g - ng) < 30)
		{
			if (g < 128)
				ng = 255;
			else
				ng = 0;
		}
		else if (abs(b - nb) < 30)
		{
			if (b < 128)
				nb = 255;
			else
				nb = 0;
		}
	}

	return RGB(nr, ng, nb);
}

///////////////////////////////////////////////////////////////////////////////
// lpszColor:  "255,0,0" or "#0000FF" or "RGB(x,x,x)" 
COLORREF GetColorFromString(LPCTSTR lpszColor)
{
	_ASSERTE(lpszColor);

	COLORREF color = AfxGetThemeManager()->GetFontDefaultForeColor();		// initialize to black

	if (!lpszColor)
		return color;

	BYTE r = 0;
	BYTE g = 0;
	BYTE b = 0;
	TCHAR *cp = 0;

	// "RGB(255,0,0"
	TCHAR* pr = (TCHAR*)_tcschr(lpszColor, _T('('));
	if (pr)
		lpszColor = ++pr;

	//TODO eliminare eventuali blank

	if ((cp = (TCHAR*)_tcschr(lpszColor, _T(','))) != NULL)
	{
		// "255,0,0"
		r = (BYTE)_ttoi(lpszColor);

		cp++;
		g = (BYTE)_ttoi(cp);
		cp = _tcschr(cp, _T(','));
		if (cp)
		{
			cp++;
			b = (BYTE)_ttoi(cp);
		}

		color = RGB(r, g, b);
	}
	else if ((cp = (TCHAR*)_tcschr(lpszColor, _T('#'))) != NULL)
	{
		// "#0000FF"
		if (_tcslen(lpszColor) == 7)
		{
			TCHAR s[3] = { _T('\0') };
			cp++;
			s[0] = *cp++;
			s[1] = *cp++;
			r = (BYTE)_tcstoul(s, NULL, 16);
			s[0] = *cp++;
			s[1] = *cp++;
			g = (BYTE)_tcstoul(s, NULL, 16);
			s[0] = *cp++;
			s[1] = *cp++;
			b = (BYTE)_tcstoul(s, NULL, 16);

			color = RGB(r, g, b);
		}
	}
	return color;
}

///////////////////////////////////////////////////////////////////////////////
TB_EXPORT BOOL CheckVMemory(LPCVOID lpMem)
{
	MEMORY_BASIC_INFORMATION meminf;
	BOOL  bOk = VirtualQuery(lpMem, &meminf, sizeof(meminf));
	if (!bOk)
	{
		ASSERT_TRACE(FALSE, cwsprintf(_T("CheckVirtualMemory - api VirtualQuery failed: last error was %d\n"), GetLastError()));
		return FALSE;
	}

	bOk = (
		(meminf.State == MEM_COMMIT) &&
		(0 != (meminf.Protect & (PAGE_READWRITE | PAGE_WRITECOPY | PAGE_EXECUTE_READWRITE)))
		);

	ASSERT_TRACE(bOk, _T("CheckVirtualMemory found an error\n"));
	return bOk;
}

TB_EXPORT BOOL CheckHMemory(LPCVOID lpMem)
{
	HANDLE hHeap = GetProcessHeap();
	if (hHeap == NULL)
	{
		ASSERT_TRACE(FALSE, cwsprintf(_T("CheckHeapMemory - api GetProcessHeap failed: last error was %d\n"), GetLastError()));
		return FALSE;
	}

	BOOL bOk = HeapValidate(hHeap, 0, lpMem);

	ASSERT_TRACE(bOk, _T("CheckHeapMemory found an error\n"));
	return bOk;
}



///////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------
CString EscapeURL(const CString& s)
{
	/*
		inline BOOL AtlEscapeUrl(
		LPCWSTR szStringIn,
		LPWSTR szStringOut,
		DWORD* pdwStrLen,
		DWORD dwMaxLength,
		DWORD dwFlags = 0
		);
		*/
	if (s == "") return CString("");
	DWORD lenMax = s.GetLength() * 2 + 1;
	CString s1;
	BOOL b = ATL::AtlCanonicalizeUrl(
		(LPCTSTR)s,
		s1.GetBuffer(lenMax),
		&lenMax
		);
	s1.ReleaseBuffer();
	return b ? s1 : s;
}

//----------------------------------------------------------------------------
CString GetTBNavigateUrl(const CString& sNamespace, const CString& sPrimaryKeyArg)
{
	CString s1 = EscapeURL(sPrimaryKeyArg);
	if (s1 == "")
		return _T("TB://") + AfxGetPathFinder()->GetServerName() + _T("/") + AfxGetPathFinder()->GetInstallationName() + _T("/") + sNamespace;
	return _T("TB://") + AfxGetPathFinder()->GetServerName() + _T("/") + AfxGetPathFinder()->GetInstallationName() + _T("/") + sNamespace + "?" + s1;
}

//----------------------------------------------------------------------------
BOOL CopyToClipboard(const CString& str)
{
	CWnd* pWnd = AfxGetMainWnd();
	if (!pWnd->OpenClipboard())
		return FALSE;

	if (!EmptyClipboard())
		return FALSE;

	int nLength = str.GetLength();
	if (nLength <= 0)
	{
		CloseClipboard();
		return FALSE;
	}

	HGLOBAL hglbCopy;;
	hglbCopy = GlobalAlloc(GHND, ((str.GetLength() + 1) * sizeof(TCHAR)));
	if (hglbCopy == NULL)
	{
		CloseClipboard();
		return FALSE;
	}

	LPTSTR lptstrCopy = (LPTSTR)GlobalLock(hglbCopy);
	memcpy(lptstrCopy, (LPCTSTR)str, (str.GetLength() * sizeof(TCHAR)));
	lptstrCopy[str.GetLength()] = (TCHAR)0;    // null character 
	GlobalUnlock(hglbCopy);

	if (SetClipboardData(CF_UNICODETEXT, hglbCopy) == NULL)
	{
		CloseClipboard();
		return FALSE;
	}

	CloseClipboard();
	return TRUE;
}

//converte in CTime una stringa del tipo aaaa-mm-ggThh:mm:ss
CTime CStringToCTime(CString strValue)
{
	int Y = _ttoi(strValue.Left(4));
	int M = _ttoi(strValue.Mid(5, 2));
	int D = _ttoi(strValue.Mid(8, 2));
	int H = _ttoi(strValue.Mid(11, 2));
	int m = _ttoi(strValue.Mid(14, 2));
	int s = _ttoi(strValue.Right(2));

	CTime t(Y, M, D, H, m, s);

	return t;
}

//----------------------------------------------------------------------------
CString	CTimeToCString(CTime aTime)
{
	return aTime.Format(_T("%Y-%m-%dT%H:%M:%S"));
}

//----------------------------------------------------------------------------
BOOL ParseSockArray(CString strTextReceived, CStringArray& resultArray)
{
	CXMLDocumentObject aXMLDocumentObject;
	aXMLDocumentObject.LoadXML(strTextReceived);
	CXMLNode* root = aXMLDocumentObject.GetRoot();
	if (!root)
		return FALSE;

	CXMLNodeChildsList* pXMLNodeChildsList = root->GetChilds();
	if (!pXMLNodeChildsList)
		return TRUE;

	for (int i = 0; i < pXMLNodeChildsList->GetSize(); i++)
	{
		CXMLNode* pValue = pXMLNodeChildsList->GetAt(i);
		CString value;
		pValue->GetText(value);
		resultArray.Add(value);
	}

	return TRUE;
}
///////////////////////////////////////////////////////////////////////////////

// XString.cpp  Version 1.1 - article available at www.codeproject.com
//        NAME                            DESCRIPTION
//   ----------------    ------------------------------------------------------
//   find
//      _tcsistr()       Find a substring in a string (case insensitive)
//      _tcsccnt()       Count number of occurrences of a character in a string
//   removal
//      _tcscrem()       Remove character in a string (case sensitive)
//      _tcsicrem()      Remove character in a string (case insensitive)
//      _tcsstrrem()     Remove substring in a string (case sensitive)
//      _tcsistrrem()    Remove substring in a string (case insensitive)
//   replace
//      _tcscrep()       Replace character in a string (case sensitive)
//      _tcsicrep()      Replace character in a string (case insensitive)
//      _tcsistrrep()    Replace one substring in a string with another 
//                       substring (case insensitive)
//   trim
//      _tcsltrim()      Removes (trims) leading whitespace characters from a string
//      _tcsrtrim()      Removes (trims) trailing whitespace characters from a string
//      _tcstrim()       Removes (trims) leading and trailing whitespace characters 
//                       from a string
//    copy
//      _tcszdup()       Allocates buffer with new, fills it with zeros, copies
//                       string
//      _tcsnzdup()      Allocates buffer with new, fills it with zeros, copies
//                       count characters from string to buffer
//
///////////////////////////////////////////////////////////////////////////////

#pragma warning(disable : 4127)	// conditional expression is constant (_ASSERTE)
#pragma warning(disable : 4996)	// disable bogus deprecation warning

///////////////////////////////////////////////////////////////////////////////
//
// _tcscrep()
//
// Purpose:     Replace character in a string (case sensitive)
//
// Parameters:  str     - pointer to string; upon return, str will be updated 
//                        with the character replacements
//              chOld   - character to look for
//              chNew   - character to replace it with
//
// Returns:     TCHAR * - Pointer to the updated string. Because the 
//                        modification is done in place, the pointer  
//                        returned is the same as the pointer passed 
//                        as the input argument. 
//
TCHAR * _tcscrep(TCHAR *str, TCHAR chOld, TCHAR chNew)
{
	if (!str)
		return str;

	TCHAR *str1 = str;
	while (*str1)
	{
		if (*str1 == chOld)
			*str1 = chNew;
		str1++;
	}

	return str;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcsicrep()
//
// Purpose:     Replace character in a string (case insensitive)
//
// Parameters:  str     - pointer to string; upon return, str will be updated 
//                        with the character replacements
//              chOld   - character to look for
//              chNew   - character to replace it with
//
// Returns:     TCHAR * - Pointer to the updated string. Because the 
//                        modification is done in place, the pointer  
//                        returned is the same as the pointer passed 
//                        as the input argument. 
//
TCHAR * _tcsicrep(TCHAR *str, TCHAR chOld, TCHAR chNew)
{
	if (!str)
		return str;

	size_t nLen = _tcslen(str);
	TCHAR *newstr = new TCHAR[nLen + 1];
	ZeroMemory(newstr, nLen + 1);
	_tcscpy(newstr, str);
	_tcslwr(newstr);

	TCHAR oldstr[2] = { chOld, _T('\0') };
	_tcslwr(oldstr);

	TCHAR *cp = newstr;
	while (*cp)
	{
		if (*cp == oldstr[0])
			str[cp - newstr] = chNew;
		cp++;
	}
	delete[] newstr;

	return str;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcsicrem()
//
// Purpose:     Remove character in a string (case insensitive)
//
// Parameters:  str     - pointer to string; upon return, str will be updated 
//                        with the character removals
//              ch      - character to remove
//
// Returns:     TCHAR * - Pointer to the updated string. Because the 
//                        modification is done in place, the pointer  
//                        returned is the same as the pointer passed 
//                        as the input argument. 
//
TCHAR * _tcsicrem(TCHAR * str, TCHAR ch)
{
	if (!str)
		return str;

	size_t nLen = _tcslen(str);
	TCHAR *newstr = new TCHAR[nLen + 1];
	ZeroMemory(newstr, nLen + 1);
	_tcscpy(newstr, str);
	_tcslwr(newstr);

	TCHAR chstr[2] = { ch, _T('\0') };
	_tcslwr(chstr);

	TCHAR *cp1 = str;
	TCHAR *cp2 = newstr;

	while (*cp2)
	{
		if (*cp2 != chstr[0])
			*cp1++ = str[cp2 - newstr];
		cp2++;
	}
	*cp1 = _T('\0');

	delete[] newstr;

	return str;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcscrem()
//
// Purpose:     Remove character in a string (case sensitive)
//
// Parameters:  str     - pointer to string; upon return, str will be updated 
//                        with the character removals
//              ch      - character to remove
//
// Returns:     TCHAR * - Pointer to the updated string. Because the 
//                        modification is done in place, the pointer  
//                        returned is the same as the pointer passed 
//                        as the input argument. 
//
TCHAR * _tcscrem(TCHAR * str, TCHAR ch)
{
	if (!str)
		return str;

	TCHAR *cp1 = str;
	TCHAR *cp2 = str;

	while (*cp2)
	{
		if (*cp2 != ch)
			*cp1++ = *cp2;
		cp2++;
	}
	*cp1 = _T('\0');

	return str;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcsistrrem()
//
// Purpose:     Remove substring in a string (case insensitive)
//
// Parameters:  str     - pointer to string; upon return, str will be updated 
//                        with the substring removals
//              substr  - substring to remove
//
// Returns:     TCHAR * - Pointer to the updated string. Because the 
//                        modification is done in place, the pointer  
//                        returned is the same as the pointer passed 
//                        as the input argument. 
//
TCHAR * _tcsistrrem(TCHAR * str, const TCHAR *substr)
{
	if (!str)
		return str;
	if (!substr)
		return str;

	TCHAR *target = NULL;
	size_t nSubstrLen = _tcslen(substr);
	TCHAR *cp = str;
	while ((target = _tcsistr(cp, substr)) != NULL)
	{
		_tcscpy(target, target + nSubstrLen);
		cp = target;
	}

	return str;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcsstrrem()
//
// Purpose:     Remove substring in a string (case sensitive)
//
// Parameters:  str     - pointer to string; upon return, str will be updated 
//                        with the substring removals
//              substr  - substring to remove
//
// Returns:     TCHAR * - Pointer to the updated string. Because the 
//                        modification is done in place, the pointer  
//                        returned is the same as the pointer passed 
//                        as the input argument. 
//
TCHAR * _tcsstrrem(TCHAR * str, const TCHAR *substr)
{
	if (!str)
		return str;
	if (!substr)
		return str;

	TCHAR *target = NULL;
	size_t nSubstrLen = _tcslen(substr);
	TCHAR *cp = str;
	while ((target = _tcsstr(cp, substr)) != NULL)
	{
		_tcscpy(target, target + nSubstrLen);
		cp = target;
	}

	return str;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcsistr()
//
// Purpose:     Find a substring in a string (case insensitive)
//
// Parameters:  str     - pointer to string; upon return, str will be updated 
//                        with the character removals
//              substr  - substring to find
//
// Returns:     TCHAR * - Pointer to the first occurrence of substr in str, 
//                        or NULL if substr does not appear in string.  If 
//                        substr points to a string of zero length, the 
//                        function returns str.
//
TCHAR * _tcsistr(const TCHAR * str, const TCHAR * substr)
{
	if (!str || !substr || (substr[0] == _T('\0')))
		return (TCHAR *)str;

	size_t nLen = _tcslen(substr);
	while (*str)
	{
		if (_tcsnicmp(str, substr, nLen) == 0)
			break;
		str++;
	}

	if (*str == _T('\0'))
		str = NULL;

	return (TCHAR *)str;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcsistrrep()
//
// Purpose:     Replace one substring in a string with another 
//              substring (case insensitive)
//
// Parameters:  lpszStr    - Pointer to string; upon return, lpszStr will be 
//                           updated with the character removals
//              lpszOld    - Pointer to substring that is to be replaced
//              lpszNew    - Pointer to substring that will replace lpszOld
//              lpszResult - Pointer to buffer that receives result string.  
//                           This may be NULL, in which case the required size
//                           of the result buffer will be returned. (Call
//                           _tcsistrrep once to get size, then allocate 
//                           buffer, and call _tcsistrrep again.)
//
// Returns:     int        - Size of result string.  If lpszResult is NULL,
//                           the size of the buffer (in TCHARs) required to 
//                           hold the result string is returned.  Does not 
//                           include terminating nul character.  Returns 0
//                           if no replacements.
//
int _tcsistrrep(const TCHAR * lpszStr,
	const TCHAR * lpszOld,
	const TCHAR * lpszNew,
	TCHAR * lpszResult)
{
	if (!lpszStr || !lpszOld || !lpszNew)
		return 0;

	size_t nStrLen = _tcslen(lpszStr);
	if (nStrLen == 0)
		return 0;

	size_t nOldLen = _tcslen(lpszOld);
	if (nOldLen == 0)
		return 0;

	size_t nNewLen = _tcslen(lpszNew);

	// loop once to figure out the size of the result string
	int nCount = 0;
	TCHAR *pszStart = (TCHAR *)lpszStr;
	TCHAR *pszEnd = (TCHAR *)lpszStr + nStrLen;
	TCHAR *pszTarget = NULL;
	TCHAR * pszResultStr = NULL;

	while (pszStart < pszEnd)
	{
		while ((pszTarget = _tcsistr(pszStart, lpszOld)) != NULL)
		{
			nCount++;
			pszStart = pszTarget + nOldLen;
		}
		pszStart += _tcslen(pszStart);
	}

	// if any changes, make them now
	if (nCount > 0)
	{
		// allocate buffer for result string
		size_t nResultStrSize = nStrLen + (nNewLen - nOldLen) * nCount + 2;
		pszResultStr = new TCHAR[nResultStrSize];
		ZeroMemory(pszResultStr, nResultStrSize*sizeof(TCHAR));

		pszStart = (TCHAR *)lpszStr;
		pszEnd = (TCHAR *)lpszStr + nStrLen;
		TCHAR *cp = pszResultStr;

		// loop again to actually do the work
		while (pszStart < pszEnd)
		{
			while ((pszTarget = _tcsistr(pszStart, lpszOld)) != NULL)
			{
				int nCopyLen = (int)(pszTarget - pszStart);
				_tcsncpy(cp, &lpszStr[pszStart - lpszStr], nCopyLen);

				cp += nCopyLen;

				pszStart = pszTarget + nOldLen;

				_tcscpy(cp, lpszNew);

				cp += nNewLen;
			}
			_tcscpy(cp, pszStart);
			pszStart += _tcslen(pszStart);
		}

		//TRACE("pszResultStr=<%s>\n", pszResultStr);

		_ASSERTE(pszResultStr[nResultStrSize - 2] == _T('\0'));

		if (lpszResult && pszResultStr)
			_tcscpy(lpszResult, pszResultStr);
	}

	int nSize = 0;
	if (pszResultStr)
	{
		nSize = (int)_tcslen(pszResultStr);
		delete[] pszResultStr;
	}

	//TRACE("_tcsistrrep returning %d\n", nSize);

	return nSize;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcstrim()
//
// Purpose:     Removes (trims) leading and trailing whitespace characters 
//              from a string
//
// Parameters:  str     - Pointer to the null-terminated string to be trimmed. 
//                        On return, str will hold the trimmed string.
//              targets - Pointer to string containing whitespace characters.
//                        If this is NULL, a default set of whitespace
//                        characters is used.
//
// Returns:     TCHAR * - Pointer to trimmed string
//
TCHAR *_tcstrim(TCHAR *str, const TCHAR *targets)
{
	return _tcsltrim(_tcsrtrim(str, targets), targets);
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcsrtrim()
//
// Purpose:     Removes (trims) trailing whitespace characters from a string
//
// Parameters:  str     - Pointer to the null-terminated string to be trimmed. 
//                        On return, str will hold the trimmed string.
//              targets - Pointer to string containing whitespace characters.
//                        If this is NULL, a default set of whitespace
//                        characters is used.
//
// Returns:     TCHAR * - Pointer to trimmed string
//
TCHAR *_tcsrtrim(TCHAR *str, const TCHAR *targets)
{
	TCHAR *end;

	if (!str)
		return NULL;

	if (!targets)
		targets = _T(" \t\n\r");

	end = str + _tcslen(str);

	while (end-- > str)
	{
		if (!_tcschr(targets, *end))
			return str;
		*end = 0;
	}
	return str;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcsltrim()
//
// Purpose:     Removes (trims) leading whitespace characters from a string
//
// Parameters:  str     - Pointer to the null-terminated string to be trimmed. 
//                        On return, str will hold the trimmed string.
//              targets - Pointer to string containing whitespace characters.
//                        If this is NULL, a default set of whitespace
//                        characters is used.
//
// Returns:     TCHAR * - Pointer to trimmed string
//
TCHAR *_tcsltrim(TCHAR *str, const TCHAR *targets)
{
	if (!str)
		return NULL;

	if (!targets)
		targets = _T(" \t\r\n");

	while (*str)
	{
		if (!_tcschr(targets, *str))
			return str;
		++str;
	}
	return str;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcszdup()
//
// Purpose:     Allocates buffer with new, fills it with zeros, copies string
//              to buffer
//
// Parameters:  str     - Pointer to the null-terminated string to be copied. 
//
// Returns:     TCHAR * - Pointer to duplicated string (allocated with new)
//
TCHAR *_tcszdup(const TCHAR *str)
{
	if (!str)
		return NULL;

	size_t len = _tcslen(str);
	TCHAR *cp = new TCHAR[len + 16];
	memset(cp, 0, (len + 16)*sizeof(TCHAR));
	_tcsncpy(cp, str, len);

	return cp;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcsnzdup()
//
// Purpose:     Allocates buffer with new, fills it with zeros, copies count
//              characters from string to buffer
//
// Parameters:  str     - Pointer to the null-terminated string to be copied
//              count   - Number of characters to copy
//
// Returns:     TCHAR * - Pointer to duplicated string (allocated with new)
//
TCHAR *_tcsnzdup(const TCHAR *str, size_t count)
{
	if (!str)
		return NULL;

	TCHAR *cp = new TCHAR[count + 16];
	memset(cp, 0, (count + 16)*sizeof(TCHAR));
	_tcsncpy(cp, str, count);

	return cp;
}

///////////////////////////////////////////////////////////////////////////////
//
// _tcsccnt()
//
// Purpose:     Count number of occurrences of a character in a string
//
// Parameters:  str - pointer to string
//              ch  - character to look for
//
// Returns:     int - Number of times ch is found in str
//
int _tcsccnt(const TCHAR *str, TCHAR ch)
{
	if (!str)
		return 0;

	int count = 0;

	while (*str)
	{
		if (*str++ == ch)
			count++;
	}

	return count;
}

// Ritorna "+01:00" cioè lo scostamento rispetto a Greenwich
//----------------------------------------------------------------------------
CString GetTimeZoneDifference()
{
	// Get the local system time.
	SYSTEMTIME LocalTime = { 0 };
	GetSystemTime(&LocalTime);

	// Get the timezone info.
	TIME_ZONE_INFORMATION TimeZoneInfo;
	GetTimeZoneInformation(&TimeZoneInfo);

	// Convert local time to UTC.
	SYSTEMTIME GmtTime = { 0 };
	TzSpecificLocalTimeToSystemTime(&TimeZoneInfo, &LocalTime, &GmtTime);

	// TimeZoneInfo.Bias is the difference between local time and GMT in minutes.
	float TimeZoneDifference = -(float(TimeZoneInfo.Bias) / 60);

	CString csLocalTimeInGmt;
	csLocalTimeInGmt.Format(_T("%02.0f"), TimeZoneDifference);
	if (TimeZoneDifference > 0.0)
		csLocalTimeInGmt = _T("+") + csLocalTimeInGmt + _T(":00");
	else
		csLocalTimeInGmt = _T("-") + csLocalTimeInGmt + _T(":00");

	if (csLocalTimeInGmt.GetLength() != 6)	// in caso di errori metto un default sensato per l'Italia
		csLocalTimeInGmt = _T("+01:00");

	return csLocalTimeInGmt;
}

//----------------------------------------------------------------------------
void DrawBitmap(CBitmap* pBmp, CDC* pDC, const CRect& rect, BOOL bStretch /*FALSE*/)
{
	DrawBitmap(pBmp, pDC, rect, AfxGetThemeManager()->GetButtonFaceBkgColor(), bStretch);
}

//////////////////////////////////////////////////////////////////////
// CTBToolbarMenuExtFunctions
//////////////////////////////////////////////////////////////////////
//-------------------------------------------------------------------------------------
BOOL CTBToolbarMenuExtFunctions::AddMenuItem(HMENU hTargetMenu, const CString& itemText, UINT itemID, BOOL bCheck /*= FALSE*/)
{
	BOOL bSuccess = FALSE;

	ASSERT(itemText.GetLength() > 0);
	ASSERT(hTargetMenu != NULL);

	// first, does the menu item have
	// any required submenus to be found/created?
	if (itemText.Find('\\') >= 0)
	{
		// yes, we need to do a recursive call
		// on a submenu handle and with that sub
		// menu name removed from itemText

		// 1:get the popup menu name
		CString popupMenuName = itemText.Left(itemText.Find('\\'));

		// 2:get the rest of the menu item name
		// minus the delimiting '\' character
		CString remainingText =
			itemText.Right(itemText.GetLength()
			- popupMenuName.GetLength() - 1);

		// 3:See whether the popup menu already exists
		int itemCount = ::GetMenuItemCount(hTargetMenu);
		BOOL bFoundSubMenu = FALSE;
		MENUITEMINFO menuItemInfo;

		memset(&menuItemInfo, 0, sizeof(MENUITEMINFO));
		menuItemInfo.cbSize = sizeof(MENUITEMINFO);
		menuItemInfo.fMask =
			MIIM_TYPE | MIIM_STATE | MIIM_ID | MIIM_SUBMENU;
		for (int itemIndex = 0;
			itemIndex < itemCount && !bFoundSubMenu; itemIndex++)
		{
			::GetMenuItemInfo(
				hTargetMenu,
				itemIndex,
				TRUE,
				&menuItemInfo);
			if (menuItemInfo.hSubMenu != 0)
			{
				// this menu item is a popup menu (non popups give 0)
				TCHAR    buffer[MAX_PATH];
				::GetMenuString(
					hTargetMenu,
					itemIndex,
					buffer,
					MAX_PATH,
					MF_BYPOSITION);
				if (popupMenuName == buffer)
				{
					// this is the popup menu we have to add to
					bFoundSubMenu = TRUE;
					if (bCheck)
						CheckMenuItem(hTargetMenu, menuItemInfo.wID, MF_CHECKED);
				}
			}
		}
		// 4: If exists, do recursive call,
		// else create do recursive call
		// and then insert it
		if (bFoundSubMenu)
		{
			bSuccess = AddMenuItem(
				menuItemInfo.hSubMenu,
				remainingText,
				itemID,
				bCheck);
		}
		else
		{
			// we need to create a new sub menu and insert it
			HMENU hPopupMenu = ::CreatePopupMenu();
			if (hPopupMenu != NULL)
			{
				bSuccess = AddMenuItem(
					hPopupMenu,
					remainingText,
					itemID,
					bCheck);
				if (bSuccess)
				{
					if (::AppendMenu(
						hTargetMenu,
						bCheck ? MF_POPUP | MF_CHECKED : MF_POPUP,
						(UINT)hPopupMenu,
						popupMenuName) > 0)
					{
						bSuccess = TRUE;
						// hPopupMenu now owned by hTargetMenu,
						// we do not need to destroy it
					}
					else
					{
						// failed to insert the popup menu
						bSuccess = FALSE;
						// stop a resource leak
						::DestroyMenu(hPopupMenu);
					}
				}
			}
		}
	}
	else
	{
		// no sub menus required, add this item to this HMENU
		// item ID of 0 means we are adding a separator
		if (itemID != 0)
		{
			// its a normal menu command
			if (::AppendMenu(
				hTargetMenu,
				bCheck ? MF_STRING | MF_CHECKED : MF_STRING,//MF_BYCOMMAND,
				itemID,
				itemText) > 0)
			{
				// we successfully added the item to the menu
				bSuccess = TRUE;
			}
		}
		else
		{
			// we are inserting a separator
			if (::AppendMenu(
				hTargetMenu,
				MF_SEPARATOR,
				itemID,
				itemText) > 0)
			{
				// we successfully added the separator to the menu
				bSuccess = TRUE;
			}
		}
	}

	return bSuccess;
}

//------------------------------------------------------------------------------
void Capitalize(CString& str) {
	// Don't use MakeLower() or MakeUpper() as they may be wired to the CRT.
	// The CRT versions of MakeUpper and MakeLower doesn't handle char sets
	// other than US ASCII very well. Ä does not work for instance.
	LPTSTR lpszBuf = str.GetBuffer();
	CharUpperBuff(lpszBuf, 1);
	if (str.GetLength() > 1)
		CharLowerBuff(lpszBuf + 1, str.GetLength() - 1);
	str.ReleaseBuffer();
}

//------------------------------------------------------------------------------
bool GetKeyName(UINT nVK, CString& str) {
	UINT nScanCode = MapVirtualKeyEx(nVK, 0, GetKeyboardLayout(0));

	switch (nVK) {
		// Keys which are "extended" (except for Return which is Numeric Enter as extended)
	case VK_INSERT:
	case VK_DELETE:
	case VK_HOME:
	case VK_END:
	case VK_NEXT:  // Page down
	case VK_PRIOR: // Page up
	case VK_LEFT:
	case VK_RIGHT:
	case VK_UP:
	case VK_DOWN:
		nScanCode |= 0x100; // Add extended bit
	}

	// accelleratiri che necessitano di 
	// traduzione in lingua utente e non di o.s.
	switch (nVK) {
	case VK_HOME:
		str = _TB("Home");	return true;
	case VK_END:	
		str = _TB("End");	return true;
	case VK_DELETE:
		str = _TB("Del");	return true;
	case VK_RETURN:
		str = _TB("Enter");	return true;
	}

	// GetKeyNameText() expects the scan code to be on the same format as WM_KEYDOWN
	// Hence the left shift
	BOOL bResult = GetKeyNameText(nScanCode << 16, str.GetBuffer(20), 19);
	str.ReleaseBuffer();
	return bResult != FALSE;
}
//------------------------------------------------------------------------------
CString GetAcceleratorText(const ACCEL& accel)
{
	CString strAccel;
	CString strTemp;

	if (accel.fVirt & FSHIFT) {
		GetKeyName(VK_SHIFT, strTemp);
		Capitalize(strTemp);
		strAccel.Append(strTemp);
	}

	if (accel.fVirt & FCONTROL) {
		if (strAccel.GetLength())
			strAccel.Append(_T("+"));

		GetKeyName(VK_CONTROL, strTemp);
		Capitalize(strTemp);
		strAccel.Append(strTemp);
	}

	if (accel.fVirt & FALT) {
		if (strAccel.GetLength())
			strAccel.Append(_T("+"));

		GetKeyName(VK_MENU, strTemp);
		Capitalize(strTemp);
		strAccel.Append(strTemp);
	}

	if (accel.fVirt & FVIRTKEY) {
		CString strKey;
		if (GetKeyName(accel.key, strKey)) {
			Capitalize(strKey);
			if (strAccel.GetLength())
				strAccel.Append(_T("+"));
			strAccel.Append(strKey);
		}
	}
	else {
		// key field is an ASCII key code. 
		CString strKey;
		char    ca = (char)accel.key;
		wchar_t cu;

		MultiByteToWideChar(CP_ACP, MB_PRECOMPOSED, &ca, 1, &cu, 1);
		strKey.AppendChar(cu);

		if (strAccel.GetLength())
			strAccel.Append(_T("+"));
		strAccel.Append(strKey);
	}

	return strAccel;
}

//-----------------------------------------------------------------------------
int PtToLfHeight(int pt)
{
	HDC hdc = GetDC(NULL);
	int i = -MulDiv(pt, GetDeviceCaps(hdc, LOGPIXELSY), 72);
	ReleaseDC(NULL, hdc);
	return i;
}
//-----------------------------------------------------------------------------
int LfHeightToPt(int lfh)
{
	HDC hdc = GetDC(NULL);
	int i = abs(MulDiv(lfh, 72, GetDeviceCaps(hdc, LOGPIXELSY)));
	ReleaseDC(NULL, hdc);
	return i;
}