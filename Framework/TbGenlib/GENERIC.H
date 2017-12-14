#pragma once

#include <limits.h>
#include <comdef.h>

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\TBFileDialog.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbParser\TokensTable.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class DataLng;
class CStringAddress;
class AddOnApplication;

// stili di default delle toolbar
//=============================================================================
#define DEFAULT_TOOLBAR_STYLES	TBSTYLE_TOOLTIPS | CBRS_TOOLTIPS | CBRS_ALIGN_TOP | CBRS_SIZE_DYNAMIC
//=============================================================================

// flags for file common dialog
#define READFLAGS	(OFN_PATHMUSTEXIST | OFN_HIDEREADONLY | OFN_FILEMUSTEXIST)
#define WRITEFLAGS	(OFN_PATHMUSTEXIST | OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT | OFN_NOREADONLYRETURN)

#if !defined(_AFXDLL) && !defined(AfxFindResourceHandle)
// In non-AFXDLL, the resource handle is always AfxGetResourceHandle
#define AfxFindResourceHandle(lpszResource, lpszType) AfxGetResourceHandle()
#endif

// String management
//=============================================================================

//-----------------------------------------------------------------------------
TB_EXPORT void CDECL DebugPrintf (LPCTSTR pszFmt, ...);

// attention max buffer size for formattation is 512 bytes (don't excedeed)
TB_EXPORT CString CDECL  cwsprintf (UINT idFmt, ...);
TB_EXPORT CString cwsprintf(Token);

//-----------------------------------------------------------------------------
#define AfxMessageBox AfxTBMessageBox

TB_EXPORT int AfxTBMessageBox(UINT nIDPrompt, UINT nType = MB_OK, UINT nIDHelp = (UINT)-1);
TB_EXPORT int AfxTBMessageBox(LPCTSTR lpszText, UINT nType = MB_OK, UINT nIDHelp = 0);

//-----------------------------------------------------------------------------
#ifdef ShellExecute
#undef ShellExecute
#define ShellExecute TBShellExecute
#endif
TB_EXPORT HINSTANCE TBShellExecute(HWND hwnd, LPCTSTR lpOperation, LPCTSTR lpFile, LPCTSTR lpParameters, LPCTSTR lpDirectory, INT nShowCmd);
TB_EXPORT HINSTANCE TBShellExecute(LPCTSTR lpFile, LPCTSTR lpParameters = NULL);


//-----------------------------------------------------------------------------
#define CFileDialog CTBFileDialog

// Esecuzione di un programma esterno
//=============================================================================
TB_EXPORT BOOL	SpawnProgramExecution (LPCTSTR lpszProgram, LPCTSTR lpszCmdLine, BOOL bWaitEnd);

// decodifica errore ShellExecute
TB_EXPORT CString ShellExecuteErrMsg(int nErr);

// decodifica errore comunicazione COM
TB_EXPORT CString DecodeComException ( _com_error * pe );

// Window Profile setting enhancements (scrive in WIN.INI)
//=============================================================================
//
TB_EXPORT BOOL LoadWindowPlacement	(LPCTSTR lpszSection, WINDOWPLACEMENT& windowPos, LPCTSTR lpszIniFile = NULL);
TB_EXPORT BOOL WriteWindowPlacement	(LPCTSTR lpszSection, WINDOWPLACEMENT& windowPos, LPCTSTR lpszIniFile = NULL);

// Window enhancements
//=============================================================================

// Evitare di usare nelle applicazioni finali i metodi messi a disposizioni da MFC
// perche` non soddisfano le condizioni run-time di TaskBuilder
TB_EXPORT void		AdjustClientRect		(CWnd& Win, int nWidth, int nHeight);

TB_EXPORT inline void GetDlgItemText(CWnd* pWnd, int nID, CString& strOut)
	{	CWnd* pWndTmp = pWnd->GetDlgItem(nID); if (pWndTmp) pWndTmp->GetWindowText(strOut); }

// helper function to get window text size
//=============================================================================
// helper to scale log font, rect, point and size from one DC to another!
TB_EXPORT inline CSize 	ScaleSize	(const CSize& size,		const CDC& dc)	{ CSize  tmp = size; ScaleSize(tmp, dc); return tmp; }
TB_EXPORT inline CPoint	ScalePoint	(const CPoint& point,	const CDC& dc)	{ CPoint tmp = point; ScalePoint(tmp, dc); return tmp; }
TB_EXPORT inline CRect	ScaleRect	(const CRect& rect,		const CDC& dc)	{ CRect  tmp = rect; ScaleRect(tmp, dc); return tmp; }

TB_EXPORT inline CPoint DpiConvertPoint(const CPoint& point, const INT DpiSource, const INT DpiDest)
{
	CPoint tmp = point; 
	DpiConvertPoint(tmp, DpiSource, DpiDest);
	return tmp;
}

TB_EXPORT inline CSize DpiConvertSize(const CSize& size, const INT DpiSource, const INT DpiDest)
{
	CSize  tmp = size; 
	DpiConvertSize(tmp, DpiSource, DpiDest);
	return tmp;
}
// printer functions
//============================================================================

//	current selected printer info (in printer logical pixel o video se scaled)
TB_EXPORT CPoint GetPrintableOffset	(CDC* pDC, BOOL bScale = FALSE);
TB_EXPORT CSize	GetPhisicalSize		(CDC* pDC, BOOL bScale = FALSE);
TB_EXPORT CSize	GetPrintableSize	(CDC* pDC, BOOL bScale = FALSE);

TB_EXPORT BOOL		GetPrinterDevice	(CString pszPrinterName, HGLOBAL* phDevNames, HGLOBAL* phDevMode);
TB_EXPORT BOOL		SetDefaultPrinter	(const CString& strPrinterName);
TB_EXPORT CString	GetDefaultPrinter	();
TB_EXPORT int		GetPrinterNames		(CStringArray&, const CString& strPrinterName);
TB_EXPORT BOOL		ExistPrinter		(const CString& strPrinterName);

//============================================================================
//conta il numero di bit che valgono 1: (bc(7)==3, bc(255)==8, bc(256)==1)
TB_EXPORT int BitCount(DWORD dw);

//---------------------------------------------------------------------------
//le funzioni seguenti inseriscono un buffer binario all'interno 
//di un VARIANT di tipo SAFEARRAY di unsigned int:
//la lunghezza del buffer e' inserita nel primo elemento dell'array,
//il buffer e sovrapposto a partire dal secondo elemento dell'array.
TB_EXPORT void LPVOIDtoVARIANT (VARIANT* pV, LPVOID pBinBuffer, unsigned int nLenBuffer);
//ATTENZIONE: la seguente funzione alloca, tramite la ::malloc, 
//la memoria per il pBinBuffer: il chiamante dovrà
//premurarsi di chiamare la ::free per rilasciare la memoria
TB_EXPORT void VARIANTtoLPVOID (LPVOID& pBinBuffer, unsigned int& nLenBuffer, VARIANT* pV);

// settaggio della data di applicazione
//---------------------------------------------------------------------------
TB_EXPORT DataDate	AFXAPI AfxSetApplicationDate (const DataDate aDate);

TB_EXPORT DataDate	AFXAPI AfxSetApplicationDateIMago(const DataDate aDate);

TB_EXPORT BOOL	AFXAPI AfxRunEnumsViewer	(CString sCulture = _T(""), CString sInstallation = _T(""));
TB_EXPORT BOOL	AFXAPI AfxCloseEnumsViewer	();
TB_EXPORT UINT AFXAPI AfxGetTbLoaderSOAPPort ();
TB_EXPORT UINT AFXAPI AfxGetTbLoaderTCPPort	();

TB_EXPORT BOOL ZUncompress(const CString & strFileIn, const CString & strFileOut, CString* sError = NULL);
TB_EXPORT BOOL ZCompress(const CString& strFileIn, const CString& strFileOut, const CString& strTitle, CString* sError = NULL);

TB_EXPORT HINSTANCE ImageNameSpaceWalking(const CString& strLibraryNamespace);
TB_EXPORT bool		GetImageBytes(LPCTSTR iconSource, CImageBuffer &imageBuffer);


//===========================================================================
class TB_EXPORT RetryLockedResource
{
public:
	BOOL		m_bDisabled;
	BOOL		m_bBeepDisabled;
	int			m_nMaxLockRetry;
	int			m_nMaxLockTime;
	int			m_nLockRetry;
	CString		m_strMsg;

public:
	RetryLockedResource();
public:
	virtual	BOOL	Wait(BOOL bUseMessageBox);
	virtual	void	EnableDocumentFrame(BOOL) {}
};

//===========================================================================
class TB_EXPORT TBMessageBox  : public CBCGPMessageBox
{
public:
	TBMessageBox(const BCGP_MSGBOXPARAMS* pParams);
	~TBMessageBox();

	virtual INT_PTR DoModal();

protected:
	// Generated message map functions
	//{{AFX_MSG(CBCGPMessageBox)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
