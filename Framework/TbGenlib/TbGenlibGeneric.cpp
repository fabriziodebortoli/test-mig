
#include "stdafx.h"

#include <dos.h>
#include <ctype.h>
#include <stdlib.h>
#include <io.h>
#include <sys\stat.h>
#include <math.h>
#include <float.h>
#include <stdarg.h>
#include <afxconv.h>
#include <lm.h>
#include <Vfw.h>

#include "winspool.h"
#undef GetDefaultPrinter

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TbNameSolver\PathFinder.h>

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\ProcessWrapper.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\minmax.h>
#include <TbGeneric\dllmod.h>
#include <TbGeneric\schedule.h>
#include <TbGeneric\crypt.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\FormatsHelpers.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGenlibManaged\EnumsViewerWrapper.h>
#include <TbGenlibManaged\ZCompress.h>
#include <TbGenLibManaged\main.h>
#include <TbGes\DocumentSession.h>

#include <TbParser\TokensTable.h>
#include "Messages.h"

//#include "SettingsTableManager.h"
#include "Baseapp.h"

#include "generic.h"

// resources
#include "generic.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#define GUID_STRING_LENGTH		38


//-----------------------------------------------------------------------------
static const TCHAR szWindowPos	[] = _T("WindowPos");
static const TCHAR szFormat		[] = _T("%u,%u,%d,%d,%d,%d,%d,%d,%d,%d");
static const TCHAR szSeparator	[] = _T(",");

// Alcuni namespace di default utilizzati da TaskBuilder
//-----------------------------------------------------------------------------
const TCHAR szWoormNamespace[]		 = _T("Document.Framework.TbWoormViewer.TbWoormViewer.TbWoorm");
const TCHAR szNewReport[]			 = _T("Function.Framework.TbWoormViewer.TbWoormViewer.ExecNewReport");
const TCHAR szOpenReport[]			 = _T("Function.Framework.TbWoormViewer.TbWoormViewer.ExecOpenReport");

// Events dell'applicativo
//-----------------------------------------------------------------------------
const TCHAR szApplicationStarted[]		= _T("ApplicationStarted");
const TCHAR szApplicationDateChanged[]	= _T("ApplicationDateChanged");
const TCHAR szOnDSNChanged[]			= _T("OnDSNChanged");
const TCHAR szOnQueryCanCloseApp[]		= _T("OnQueryCanCloseApp");
const TCHAR szOnBeforeCanCloseTB[]		= _T("OnBeforeCanCloseTB");

// Espressioni non note a TB ed ottenute come events
//-----------------------------------------------------------------------------
const TCHAR szAccPeriodBeginDate[]		= _T("AccPeriodBeginDate");
const TCHAR szAccPeriodEndDate[]			= _T("AccPeriodEndDate");
const TCHAR szPrevAccPeriodBeginDate[]	= _T("PrevAccPeriodBeginDate");
const TCHAR szPrevAccPeriodEndDate[]		= _T("PrevAccPeriodEndDate");

#pragma warning (disable : 4706)


// Ritorna una stringa letta della sezione e dal tag indicati dal file lpszIniFile.
//-----------------------------------------------------------------------------
CString GetPrivateProfileString (LPCTSTR lpszSection, LPCTSTR lpszEntry, LPCTSTR lpszIniFile, LPCTSTR lpszDefault /*= NULL */)
{
	ASSERT(lpszSection	!= NULL);
	ASSERT(lpszEntry	!= NULL);
	ASSERT(lpszIniFile	!= NULL);

	if (lpszDefault == NULL) lpszDefault = _T("");

	TCHAR szT[4096];
	DWORD dw = ::GetPrivateProfileString (lpszSection, lpszEntry, lpszDefault, szT, sizeof(szT), lpszIniFile);

	ASSERT(dw < 4095);
	return szT;
}

// Carica il salvataggio della precedente posizione e dimensione della finestra
// dell'applicazione. Ritorna TRUE se l'operazione ha successo altrimenti FALSE.
//-----------------------------------------------------------------------------
BOOL LoadWindowPlacement(LPCTSTR lpszSection, WINDOWPLACEMENT& windowPos, LPCTSTR lpszIniFile /* NULL */)
{
	CString szPos;
	if (lpszIniFile)	szPos = GetPrivateProfileString	(lpszSection, szWindowPos, lpszIniFile, NULL);
	else				szPos = AfxGetApp()->GetProfileString(lpszSection, szWindowPos);

	// La stringa è vuota, non esiste nessun tipo di salvattaggio
	if (szPos.IsEmpty ()) return FALSE;

    // Parsa la stringa  letta dal file *.ini
	WINDOWPLACEMENT wp;
	TCHAR* nextToken;
	TCHAR* pszToken;
	TCHAR* pszPos = szPos.GetBuffer (256);

	if (pszToken = _tcstok_s(pszPos, szSeparator, &nextToken)) wp.flags			= (UINT) _ttol(pszToken)  ; else return FALSE;
	if (pszToken = _tcstok_s(NULL,   szSeparator, &nextToken)) wp.showCmd		= (UINT) _ttol(pszToken)  ; else return FALSE;
    if (pszToken = _tcstok_s(NULL,   szSeparator, &nextToken)) wp.ptMinPosition.x= _ttoi(pszToken)		  ; else return FALSE;
	if (pszToken = _tcstok_s(NULL,   szSeparator, &nextToken)) wp.ptMinPosition.y= _ttoi(pszToken)		  ; else return FALSE;
	if (pszToken = _tcstok_s(NULL,   szSeparator, &nextToken)) wp.ptMaxPosition.x= _ttoi(pszToken)		  ; else return FALSE;
	if (pszToken = _tcstok_s(NULL,   szSeparator, &nextToken)) wp.ptMaxPosition.y= _ttoi(pszToken)		  ; else return FALSE;

	if (pszToken = _tcstok_s(NULL,   szSeparator, &nextToken)) wp.rcNormalPosition.left	= _ttoi(pszToken) ; else return FALSE;
	if (pszToken = _tcstok_s(NULL,   szSeparator, &nextToken)) wp.rcNormalPosition.top	= _ttoi(pszToken) ; else return FALSE;
	if (pszToken = _tcstok_s(NULL,   szSeparator, &nextToken)) wp.rcNormalPosition.right = _ttoi(pszToken); else return FALSE;
	if (pszToken = _tcstok_s(NULL,   szSeparator, &nextToken)) wp.rcNormalPosition.bottom= _ttoi(pszToken); else return FALSE;

	wp.length	= sizeof wp;
	windowPos	= wp;

	return TRUE;
}
#pragma warning (default : 4706)

// Scrive la corrente posizione e dimensione della finestra.
//-----------------------------------------------------------------------------
BOOL WriteWindowPlacement(LPCTSTR lpszSection, WINDOWPLACEMENT& wp, LPCTSTR lpszIniFile /* NULL */)
{
	wp.flags = 0;
	const rsize_t nLen = sizeof("-32767")*8 + sizeof("65535")*2;
	TCHAR szBuffer[nLen];

	_stprintf_s(
				szBuffer,	nLen,			szFormat,
				wp.flags,					wp.showCmd,
				wp.ptMinPosition.x,			wp.ptMinPosition.y,
				wp.ptMaxPosition.x,			wp.ptMaxPosition.y,
				wp.rcNormalPosition.left,	wp.rcNormalPosition.top,
				wp.rcNormalPosition.right,	wp.rcNormalPosition.bottom
			);

	return (lpszIniFile)
			? :: WritePrivateProfileString	(lpszSection, szWindowPos, szBuffer, lpszIniFile)
			: :: AfxGetApp()->WriteProfileString(lpszSection, szWindowPos, szBuffer);
}


//=============================================================================
// Formattazione con ritorno di CString
//=============================================================================

//-----------------------------------------------------------------------------
CString cwsprintf(UINT idFmt, ...)
{
	CString strFmt;
	CString strBuffer;

	if (!AfxLoadTBString(strFmt, idFmt))
	{		
		ASSERT_TRACE1(FALSE, "resource string %d not found", idFmt);
	}
	else
	{
		va_list marker;
	
	    va_start( marker, idFmt );
		strBuffer = cwvsprintf(strFmt, marker);
	    va_end( marker );
	 }

	return strBuffer;
}

CString cwsprintf(Token t)
{
	return AfxGetTokensTable()->ToString(t);
}

//-----------------------------------------------------------------------------
void DebugPrintf(LPCTSTR strFmt, ...)
{   
	va_list marker;
	
    va_start( marker, strFmt );
	CString strBuffer = cwvsprintf(strFmt, marker);
    va_end( marker );
    
    OutputDebugString (strBuffer);
}

//=======================================================================
// Estensioni a Windows
//=======================================================================

//----------------------------------------------------------------------------
void AdjustClientRect(CWnd& win, int nWidth, int nHeight)
{
	RECT cr, wr;

	//  Prima sistema la dimensione orizzontale; se il menu' cambia il numero di linee,
	//  lo fara' ora...
	
	win.GetClientRect(&cr);
	win.GetWindowRect(&wr);

	int borderX = ( wr.right  - wr.left ) - ( cr.right  - cr.left );
	int borderY = ( wr.bottom - wr.top  ) - ( cr.bottom - cr.top  );

	int w = nWidth  + borderX;
	int h = nHeight + borderY;

	// no redraw -----------------------------------------------v
	win.SetWindowPos(NULL,0,0,w,h,SWP_NOMOVE|SWP_NOZORDER|SWP_NOREDRAW);

	//  Adesso sistemo la dimensione verticale...
	win.GetClientRect(&cr);
	win.GetWindowRect(&wr);

	borderX = ( wr.right  - wr.left ) - ( cr.right  - cr.left );
	borderY = ( wr.bottom - wr.top  ) - ( cr.bottom - cr.top  );

	w = nWidth  + borderX;
	h = nHeight + borderY;

	// redraw -----------------------------------------------v
	win.SetWindowPos(NULL,0,0,w,h,SWP_NOMOVE|SWP_NOZORDER);

	//  Adesso e' tutto a posto...
	//win.GetClientRect(&cr);
}

/////////////////////////////////////////////////////////////////////////////
//						current selected printer info
/////////////////////////////////////////////////////////////////////////////

// Torna il vertice alto-sinistro dell'area non stampabile della stampante.
// Attenzione le misure sono in pounti fisici della stampante e dipendono
// quindi dalla risoluzione (es: 11" a 600dpi = 6600)
//-----------------------------------------------------------------------------
CPoint GetPrintableOffset(CDC* pDC, BOOL bScale)
{
	CPoint pt(0,0);
	pt.x = pDC->GetDeviceCaps(PHYSICALOFFSETX);
	pt.y = pDC->GetDeviceCaps(PHYSICALOFFSETY);

	if (bScale)
	{
		pt.x = MulDiv(pt.x, SCALING_FACTOR, pDC->GetDeviceCaps(LOGPIXELSX));
		pt.y = MulDiv(pt.y, SCALING_FACTOR, pDC->GetDeviceCaps(LOGPIXELSY));
	}

	return pt;
}

// Torna la dimensione dell'area non stampabile della stampante.
// Attenzione le misure sono in pounti fisici della stampante e dipendono
// quindi dalla risoluzione (es: 11" a 600dpi = 6600)
//-----------------------------------------------------------------------------
CSize GetPhisicalSize(CDC* pDC, BOOL bScale)
{
	CSize size(0,0);
	size.cx = pDC->GetDeviceCaps(PHYSICALWIDTH);
	size.cy = pDC->GetDeviceCaps(PHYSICALHEIGHT);

	if (bScale)
	{
		size.cx = MulDiv(size.cx, SCALING_FACTOR, pDC->GetDeviceCaps(LOGPIXELSX));
		size.cy = MulDiv(size.cy, SCALING_FACTOR, pDC->GetDeviceCaps(LOGPIXELSY));
	}
	return size;
}

// Torna la dimensione fisica della pagina della stampante.
// Attenzione le misure sono in pounti fisici della stampante e dipendono
// quindi dalla risoluzione (es: 11" a 600dpi = 6600)
//-----------------------------------------------------------------------------
CSize GetPrintableSize(CDC* pDC, BOOL bScale)
{
	CSize size(0,0);

	size.cx = pDC->GetDeviceCaps(HORZRES);
	size.cy = pDC->GetDeviceCaps(VERTRES);

	if (bScale)
	{
		size.cx = MulDiv(size.cx, SCALING_FACTOR, pDC->GetDeviceCaps(LOGPIXELSX));
		size.cy = MulDiv(size.cy, SCALING_FACTOR, pDC->GetDeviceCaps(LOGPIXELSY));
	}
	return size;
}


//see "support.microsoft.com/kb/166129" How to programmatically print to a non-default printer in MFC 
//-----------------------------------------------------------------------------
BOOL GetPrinterDevice(CString pszPrinterName, HGLOBAL* phDevNames, HGLOBAL* phDevMode)
{
    // if NULL is passed, then assume we are setting app object's
    // devmode and devnames
    if (phDevMode == NULL || phDevNames == NULL)
        return FALSE;

    // Open printer
    HANDLE hPrinter;
	if (OpenPrinter(pszPrinterName.GetBuffer(), &hPrinter, NULL) == FALSE)
        return FALSE;

    // obtain PRINTER_INFO_2 structure and close printer
    DWORD dwBytesReturned, dwBytesNeeded;
    GetPrinter(hPrinter, 2, NULL, 0, &dwBytesNeeded);
    PRINTER_INFO_2* p2 = (PRINTER_INFO_2*)GlobalAlloc(GPTR,dwBytesNeeded);
    if (GetPrinter(hPrinter, 2, (LPBYTE)p2, dwBytesNeeded,&dwBytesReturned) == 0) 
      {
       GlobalFree(p2);
       ClosePrinter(hPrinter);
       return FALSE;
    }
     ClosePrinter(hPrinter);

    // Allocate a global handle for DEVMODE
    HGLOBAL  hDevMode = GlobalAlloc(GHND, sizeof(*p2->pDevMode) + p2->pDevMode->dmDriverExtra);
    ASSERT(hDevMode);
    DEVMODE* pDevMode = (DEVMODE*)GlobalLock(hDevMode);
    ASSERT(pDevMode);

    // copy DEVMODE data from PRINTER_INFO_2::pDevMode
    memcpy(pDevMode, p2->pDevMode, sizeof(*p2->pDevMode) + p2->pDevMode->dmDriverExtra);
    GlobalUnlock(hDevMode);

    // Compute size of DEVNAMES structure from PRINTER_INFO_2's data
    DWORD drvNameLen = lstrlen(p2->pDriverName)+1;  // driver name
    DWORD ptrNameLen = lstrlen(p2->pPrinterName)+1; // printer name
    DWORD porNameLen = lstrlen(p2->pPortName)+1;    // port name

    // Allocate a global handle big enough to hold DEVNAMES.
    HGLOBAL hDevNames = GlobalAlloc(GHND, sizeof(DEVNAMES) +(drvNameLen + ptrNameLen + porNameLen)*sizeof(TCHAR));
    ASSERT(hDevNames);
    DEVNAMES* pDevNames = (DEVNAMES*)GlobalLock(hDevNames);
    ASSERT(pDevNames);

    // Copy the DEVNAMES information from PRINTER_INFO_2
    // tcOffset = TCHAR Offset into structure
    int tcOffset = sizeof(DEVNAMES)/sizeof(TCHAR);
    ASSERT(sizeof(DEVNAMES) == tcOffset*sizeof(TCHAR));

    pDevNames->wDriverOffset = tcOffset;
    memcpy((LPTSTR)pDevNames + tcOffset, p2->pDriverName,drvNameLen*sizeof(TCHAR));
    tcOffset += drvNameLen;

    pDevNames->wDeviceOffset = tcOffset;
    memcpy((LPTSTR)pDevNames + tcOffset, p2->pPrinterName,ptrNameLen*sizeof(TCHAR));
    tcOffset += ptrNameLen;

    pDevNames->wOutputOffset = tcOffset;
    memcpy((LPTSTR)pDevNames + tcOffset, p2->pPortName,porNameLen*sizeof(TCHAR));
    pDevNames->wDefault = 0;

    GlobalUnlock(hDevNames);
    GlobalFree(p2);   // free PRINTER_INFO_2

    // set the new hDevMode and hDevNames
    *phDevMode = hDevMode;
    *phDevNames = hDevNames;
    return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SetDefaultPrinter (const CString& strPrinterName)
{
	if (strPrinterName.IsEmpty())
		return FALSE;

	TCHAR szPrinter[256]; 
	GetProfileString(_T("devices"), strPrinterName, _T(""), szPrinter, sizeof(szPrinter));

	if (szPrinter[0] == NULL_CHAR)
		return FALSE;

	// costruisco l'entry come previsto da win.ini
	CString strDefault = strPrinterName + _T(",") + szPrinter;
	::WriteProfileString(_T("windows"), _T("device"), strDefault);
	::SendMessage(HWND_BROADCAST, WM_WININICHANGE, 0L, (LPARAM)(LPCTSTR)_T("windows"));

	return TRUE;
}

// ritona la default printer se esiste
//-----------------------------------------------------------------------------
CString	GetDefaultPrinter()
{
	TCHAR szDefaultPrinter[80];
	LPTSTR szPrinter;
	TCHAR* nextToken;
	GetProfileString(_T("windows"), _T("device"), _T(",,,"), szDefaultPrinter, sizeof(szDefaultPrinter));
	szPrinter = _tcstok_s(szDefaultPrinter, _T(","), &nextToken);

	return szPrinter;
}

// riempie un array con i nomi delle stampanti e ritorna la posizione di quella
// indicata come parametro se c'e` altrimenti -1
//-----------------------------------------------------------------------------
int GetPrinterNames(CStringArray& aPrinters, const CString& strPrinterName)
{
	TCHAR szAllDevices[8192]; 
	LPTSTR szPtr;
	GetProfileString(_T("devices"), NULL, _T(""), szAllDevices, sizeof(szAllDevices));

	szPtr = szAllDevices;
	int nPos = -1;
	int i = 0;
	while (*szPtr)
	{
		if (strPrinterName.CompareNoCase(szPtr) == 0)
			nPos = i;

		aPrinters.Add(szPtr);
		szPtr += _tcslen (szPtr) + 1;
		i++;
	}

	return nPos;
}

//-----------------------------------------------------------------------------
BOOL ExistPrinter(const CString& strPrinterName)
{
	TCHAR szPrinter[256]; 
	GetProfileString(_T("devices"), strPrinterName, _T(""), szPrinter, sizeof(szPrinter));

	return szPrinter[0];
}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
// risposta di default in caso di messagebox che richiede una risposta dell'utente
// in unattended mode
int DefaultMessageResponse(UINT nType)
{	
	// evito il try perché mi potrebbe condurre a situazioni cicliche
	if ((nType & MB_CANCELTRYCONTINUE) == MB_CANCELTRYCONTINUE)
		return IDCONTINUE;
	
	// evito il retry perché mi potrebbe condurre a situazioni cicliche
	if ((nType & MB_RETRYCANCEL) == MB_RETRYCANCEL)
		return IDCANCEL;

	// presumo che, se sono in unattended mode e faccio una operazione, voglio continuare
	// (è un'approssimazione, il risultato dipende ovviamente dal contesto)
	if ((nType & MB_YESNO) == MB_YESNO)
		return IDYES;
	
	// presumo che, se sono in unattended mode e faccio una operazione, voglio continuare
	// (è un'approssimazione, il risultato dipende ovviamente dal contesto)
	if ((nType & MB_YESNOCANCEL) == MB_YESNOCANCEL)
		return IDYES;

	if ((nType & MB_ABORTRETRYIGNORE)  == MB_ABORTRETRYIGNORE )
		return IDIGNORE;
	
	// presumo che, se sono in unattended mode e faccio una operazione, voglio continuare
	// (è un'approssimazione, il risultato dipende ovviamente dal contesto)
	if ((nType & MB_OKCANCEL) == MB_OKCANCEL)
		return IDOK;

    //se arrivo qui il tipo era MBOK                              
	return IDOK;
}

#undef AfxMessageBox
//-----------------------------------------------------------------------------
int AfxTBMessageBox(UINT nIDPrompt, UINT nType, UINT nIDHelp)
{
	return AfxTBMessageBox(AfxLoadTBString(nIDPrompt), nType, nIDHelp);
}

static const TCHAR szDocumentNamespace[]		= _T("documentNamespace");
static const TCHAR szNewProfileName[]			= _T("newProfileName");
static const TCHAR szPosType[]					= _T("posType");

//-----------------------------------------------------------------------------
int AfxTBMessageBox(LPCTSTR lpszTxt, UINT nType, UINT nIDHelp)
{
	int idx = _tcsspn(lpszTxt, L" \n\r\t");
	CString sText(&lpszTxt[idx]);
	CString sCaption(_TB("Info"));

	for (
			idx = sText.GetLength() - 1;  
			idx >= 0 && 
			(sText[idx] == '\n' || sText[0] == '\r' || sText[idx] == ' ' || sText[0] == '\t')
			; idx --
		);
	sText.Truncate(idx + 1);
	sText.Replace(L"\r\n\r\n", L"\r\n");
	sText.Replace(L"\n\n", L"\n");
	//-------------------

	int nResult = 0;
	CFunctionDescription fd;
	BOOL bMacroRecorder = AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder"));
	//Se il Macrorecorder è attivato e siamo in stato di play
	if (
		bMacroRecorder &&
		AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::PLAYING
		)
	{
		if (AfxGetTbCmdManager()->GetFunctionDescription(_NS_WEB("TestManager.TBMacroRecorder.TBMacroRecorder.PlayMessageBox"), fd))
		{
			fd.SetParamValue(_T("msgText"), DataStr(sText));
			fd.SetParamValue(_T("type"),	DataInt((int)nType));
			
			if (AfxGetTbCmdManager()->RunFunction(&fd, 0) && fd.GetReturnValue())
			{
				BOOL bPlayed = *((DataBool*)fd.GetReturnValue());
				//se c'è una risposta alla messagebox, usala come risposta, se errore torna ID_CANCEL
				if (bPlayed)
				{
					BOOL bError = *((DataBool*)fd.GetParamValue(_T("error")));
					if (!bError)
					{
						nResult = *((DataInt*)fd.GetParamValue(_T("result")));
						return nResult;
					}
					else
						return IDCANCEL;
				}
			}
		}
	}

	if (sText.IsEmpty())
	{
		ASSERT(FALSE);
		return DefaultMessageResponse(nType);
	}

	// thread is in unattended diagnostic
	if (AfxIsCurrentlyInUnattendedMode())
	{	
		CDiagnostic::MsgType type = CDiagnostic::Info;
		if ((nType & MB_ICONEXCLAMATION) == MB_ICONEXCLAMATION || (nType & MB_ICONSTOP) == MB_ICONSTOP)
			type = CDiagnostic::Error;
		AfxGetDiagnostic()->Add(sText, type);
		return DefaultMessageResponse(nType);
	}
	
	if (AfxIsRemoteInterface())
	{
		CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
		ASSERT (pSession);
		nResult = pSession->MessageBoxDialog(sText, nType);
	}
	else
	{
		// BCGP custom MessageBox parameters
		BCGP_MSGBOXPARAMS params;
		params.cbSize = sizeof(params);
		params.hwndOwner = ::GetActiveWindow();
		params.hInstance = NULL;
		params.lpszText = sText;
		params.lpszCaption = sCaption;
		params.dwStyle = nType;
		params.lpszIcon = NULL;
		params.dwContextHelpId = nIDHelp;
		params.lpfnMsgBoxCallback = NULL;
		params.bUseNativeCaption = CBCGPMessageBox::m_bUseNativeCaption;
		params.bUseNativeControls = CBCGPMessageBox::m_bUseNativeControls;

		// Show message box
		TBMessageBox mb(&params);
		nResult = (int)mb.DoModal();
	}
	
	//Record MessageBox Value
	if (
		bMacroRecorder &&
		AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::RECORDING
		)
	{
		if (AfxGetTbCmdManager()->GetFunctionDescription(_T("TestManager.TBMacroRecorder.TBMacroRecorder.RecordMessageBox"), fd))
		{
			fd.SetParamValue(_T("msgText"), DataStr(sText));
			fd.SetParamValue(_T("type"), DataInt((int)nType));
			fd.SetParamValue(_T("result"), DataInt((int)nResult));
			AfxGetTbCmdManager()->RunFunction(&fd, 0);
		}
	}	
	
	return nResult;
}

#undef ShellExecute
//Metodo che chiama ShellExecute se non si e' in modalita' web (interfaccia remota), altrimenti mostra una message box
// con un messaggio di funzionalita' non supportata
//-----------------------------------------------------------------------------
HINSTANCE TBShellExecute(HWND hwnd, LPCTSTR lpOperation, LPCTSTR lpFile, LPCTSTR lpParameters, LPCTSTR lpDirectory, INT nShowCmd)
{
	if (AfxIsRemoteInterface())
	{
		AfxTBMessageBox(_TB("This command is not available in Web Mode"));
		return NULL;
	}
	else
	{
		//chiamo la versione Unicode della ShellExecute(W), siccome la  macro Shell e' undefinita(vedere undef sopra il metodo)
		return ::ShellExecuteW(hwnd, lpOperation, lpFile, lpParameters, lpDirectory, nShowCmd);
	}
}

//------------------------------------------------------------------------------
TB_EXPORT HINSTANCE TBShellExecute(LPCTSTR lpFile, LPCTSTR lpParameters/*=NULL*/)
{
	return ::TBShellExecute(HWND_DESKTOP, _T("open"), lpFile, lpParameters, NULL, SW_SHOW );
}

//-----------------------------------------------------------------------------

/*
ci si rifa alla seguente struttura definita in objbase.h di visualc++
typedef struct _GUID
{
    unsigned long Data1;
    unsigned short Data2;
    unsigned short Data3;
    unsigned TCHAR Data4[8];
} GUID;
*/

//----------------------------------------------------------------------------
void LPVOIDtoVARIANT (VARIANT* pV, LPVOID pBinBuffer, unsigned int nLenBuffer)
{
	::VariantInit(pV);
	pV->vt = VT_ARRAY | VT_BSTR;	
	SAFEARRAY* psa = ::SafeArrayCreateVector(VT_UI1, 0, (nLenBuffer+3)/sizeof(unsigned int) + 1);
	pV->parray = psa;
	LPVOID lpArrayData;
	::SafeArrayAccessData( psa, &lpArrayData );
	::memcpy( lpArrayData, &nLenBuffer, sizeof(unsigned int) );
	::memcpy( ((LPBYTE)lpArrayData)+sizeof(unsigned int), pBinBuffer, nLenBuffer );
	::SafeArrayUnaccessData( psa );
}

//----------------------------------------------------------------------------
void VARIANTtoLPVOID (LPVOID& pBinBuffer, unsigned int& nLenBuffer, VARIANT* pV)
{
	LPVOID lpArrayData;
	::SafeArrayAccessData( pV->parray, &lpArrayData );
	nLenBuffer = *((unsigned int *)lpArrayData);

	pBinBuffer = ::malloc(nLenBuffer);
	
	::memcpy(pBinBuffer, ((LPBYTE)lpArrayData)+sizeof(unsigned int), nLenBuffer);
}

//----------------------------------------------------------------------------
#define BUFFER_SIZE ((UINT)2048)

// ------------------------------------------------------------------------------
CString ShellExecuteErrMsg(int nErr)
{
	if (nErr > 32)
		return _TB("Operation completed successfully.");

	switch (nErr)
	{
		case 0 :
		case SE_ERR_OOM :
			return _TB("The operating system does not have enough memory or resources to end the operation.");

		case ERROR_BAD_FORMAT : 
			return _TB("Invalid executable file (it is not Win32 or there is an error in its image)."); 
 
		case SE_ERR_FNF :
			return _TB("Unable to find the specified file."); 
 
		case SE_ERR_PNF :
			return _TB("Unable to find the specified path."); 

		case SE_ERR_ACCESSDENIED :
			return _TB("Access denied to the specified file."); 
 
		case SE_ERR_NOASSOC :
			return _TB("No application is associated to the extension of the specified file."); 
		
		case SE_ERR_ASSOCINCOMPLETE :
			return _TB("Incomplete or invalid filename association."); 
 
		case SE_ERR_DDEBUSY :
			return _TB("The DDE transaction cannot be ended due to other DDE transactions."); 
 
		case SE_ERR_DDEFAIL :
			return _TB("DDE transaction failed."); 
 
		case SE_ERR_DDETIMEOUT :
			return _TB("The DDE transaction was not ended due to timeout."); 
 
		case SE_ERR_DLLNOTFOUND :
			return _TB("Unable to find the specified dynamic link library."); 
 
		case SE_ERR_SHARE :
			return _TB("A file sharing violation occurred."); 
	}

	return _T("");
}

//-------------------------------------------------------------------------------
CString DecodeComException ( _com_error * pe )
{
    _bstr_t bstrSource( pe->Source() );
    _bstr_t bstrDescription( pe->Description() );
	CString strEx;

	strEx.Format
		(
			_TB("Error in the communication with the external program:\r\n\tError code= {0-%08lx}\r\n\t{1-%s}\r\n\tSource = {2-%s}\r\n\tDescription = {3-%s}"), 
			pe->Error(), pe->ErrorMessage(),
			(LPCTSTR) bstrSource, (LPCTSTR) bstrDescription
		);
	return strEx;
}

//=================================================================================
//conta il numero di bit che valgono 1: (bc(7)==3, bc(255)==8, bc(256)==1)
int BitCount(DWORD dw)
{
	int c=0;
	int nb = 8 * sizeof(DWORD);

	for(int i =0; i <  nb; i++, dw >>= 1)
		if (dw & 1) c++;
	return c;
}
//=================================================================================
BOOL SpawnProgramExecution(LPCTSTR lpszProgram, LPCTSTR lpszCmdLine, BOOL bWaitEnd)
{
	if (!lpszProgram || !*lpszProgram)
		return FALSE;

	CString strProgram(lpszProgram);
	CString strCmdLine;
	if (lpszCmdLine && *lpszCmdLine)
		strCmdLine.Format(_T("\"%s\" %s"), lpszProgram, lpszCmdLine);

	STARTUPINFO startInfo;
	ZeroMemory(&startInfo, sizeof(STARTUPINFO));
    startInfo.cb = sizeof(STARTUPINFO);

	PROCESS_INFORMATION procInfo;
    ZeroMemory(&procInfo, sizeof( PROCESS_INFORMATION ));

	SECURITY_ATTRIBUTES SecurityAttrs;
	SecurityAttrs.nLength=sizeof(SECURITY_ATTRIBUTES);
	SecurityAttrs.lpSecurityDescriptor=NULL;
	SecurityAttrs.bInheritHandle=TRUE;

    DWORD dwExitCode = 0;
	DWORD dwCreationFlags = NORMAL_PRIORITY_CLASS|DETACHED_PROCESS;
	BOOL bOK = ::CreateProcess
					(
						NULL, 
						strCmdLine.GetBuffer(strCmdLine.GetLength()), 
						&SecurityAttrs,
						&SecurityAttrs,
						FALSE,
						dwCreationFlags,
						NULL,
						NULL,
						&startInfo,
						&procInfo
					);
	strProgram.ReleaseBuffer();
	strCmdLine.ReleaseBuffer();

	if (bOK && bWaitEnd)
	{
		WaitForSingleObject (procInfo.hProcess, INFINITE );
		GetExitCodeProcess (procInfo.hProcess, &dwExitCode );
		if (dwExitCode)
			return FALSE;
	}
	
	if (bOK)
	{
		::CloseHandle(procInfo.hThread);
		::CloseHandle(procInfo.hProcess);
	}

	if (bOK)
		return TRUE;
	
	LPVOID lpMsgBuf;
	DWORD  dwError = GetLastError();

	FormatMessage( 
		FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
		NULL,
		dwError,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
		(LPTSTR) &lpMsgBuf,
		0,
		NULL 
	);
	AfxMessageBox ((LPCTSTR)lpMsgBuf, MB_ICONSTOP);
	// Free the buffer.
	LocalFree( lpMsgBuf );
	
	return FALSE;
}

//------------------------------------------------------------------------------
DataDate AFXAPI AfxSetApplicationDate (DataDate aDate)
{
	DataDate oldDate;	
	// E' stata assegnata almeno una volta quindi la salvo
	CLoginContext *pContext = AfxGetLoginContext();
	if (pContext->GetOperationsYear())
		oldDate = DataDate (
			pContext->GetOperationsDay(),
			pContext->GetOperationsMonth(),
			pContext->GetOperationsYear());

	DataDate maxDate;
	
	if (! AfxIsValidDate(aDate, maxDate))
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("Sadly your M.L.U. service has expired the {0-%s}.\r\nThe operation date cannot follow the M.L.U. expiry date. For this reason, the automatically set date is {0-%s}.\r\nRestore the full efficiency of you Mago.net!\r\nFind out how by visiting the website: {1-%s}"), maxDate.Str(1),AfxGetLoginManager()->GetBrandedKey(_T("MLURenewalShortLink")) ), CDiagnostic::Warning, L"MLU-01");
		aDate = maxDate;
	}
	
	pContext->SetOperationsDate(aDate.Day(), aDate.Month(), aDate.Year());
	AfxGetThreadContext()->SetOperationsDate(aDate.Day(), aDate.Month(), aDate.Year());
	if (!oldDate.IsEmpty() && oldDate != aDate )
	{
        //CloseExistingDocument();
		pContext->UpdateMenuTitle();
		CTbCommandInterface* pCommand = AfxGetTbCmdManager();
		if (pCommand) 
			pCommand->FireEvent (szApplicationDateChanged, NULL);
	}
	return oldDate;
}

//------------------------------------------------------------------------------
DataDate AFXAPI AfxSetApplicationDateIMago(DataDate aDate)
{
	DataDate oldDate;
	// E' stata assegnata almeno una volta quindi la salvo
	CLoginContext *pContext = AfxGetLoginContext();
	if (pContext->GetOperationsYear())
		oldDate = DataDate(
			pContext->GetOperationsDay(),
			pContext->GetOperationsMonth(),
			pContext->GetOperationsYear());

	DataDate maxDate;
	if (AfxGetBaseApp()->GetNrOpenDocuments() > 0)
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("There are one or more documents open. Please close them and then retry!")), CDiagnostic::Error, L"IMAGO");
		return oldDate;
	}

	if (!AfxIsValidDate(aDate, maxDate))
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("Sadly your M.L.U. service has expired the {0-%s}.\r\nThe operation date cannot follow the M.L.U. expiry date. For this reason, the automatically set date is {0-%s}.\r\nRestore the full efficiency of you Mago.net!\r\nFind out how by visiting the website: {1-%s}"), maxDate.Str(1), AfxGetLoginManager()->GetBrandedKey(_T("MLURenewalShortLink"))), CDiagnostic::Warning, L"MLU-01");
		aDate = maxDate;
	}

	pContext->SetOperationsDate(aDate.Day(), aDate.Month(), aDate.Year());
	AfxGetThreadContext()->SetOperationsDate(aDate.Day(), aDate.Month(), aDate.Year());
	if (!oldDate.IsEmpty() && oldDate != aDate)
	{
		//CloseExistingDocument();
		pContext->UpdateMenuTitle();
		CTbCommandInterface* pCommand = AfxGetTbCmdManager();
		if (pCommand)
			pCommand->FireEvent(szApplicationDateChanged, NULL);
		AfxGetDiagnostic()->Add(cwsprintf(_TB("The Application Date was successfully changed!")), CDiagnostic::Info, L"IMAGO");
	}
	return oldDate;
}


//////////////////////////////////////////////////////////////////////
//		class RetryLockedResource
//////////////////////////////////////////////////////////////////////
//
//=====================================================================
RetryLockedResource::RetryLockedResource()
:
	m_nLockRetry (1324616) // numero magico preso da md2000
{
	DataObj* pSetting = AfxGetSettingValue(snsTbOleDb, szLockManager, szDisableLockRetry, DataBool(m_bDisabled), szTbDefaultSettingFileName);
	m_bDisabled	= pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(snsTbOleDb, szLockManager, szDisableBeep, DataBool(m_bBeepDisabled), szTbDefaultSettingFileName);
	m_bBeepDisabled	= pSetting ? *((DataBool*) pSetting) : FALSE;
	
	pSetting = AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxLockRetry, DataInt(m_nMaxLockRetry), szTbDefaultSettingFileName);
	m_nMaxLockRetry	= pSetting ? *((DataInt*) pSetting) : 8;

	pSetting = AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxLockTime, DataInt(m_nMaxLockTime), szTbDefaultSettingFileName);
	m_nMaxLockTime	= pSetting  ? *((DataInt*) pSetting) : 3000;

}

//-----------------------------------------------------------------------------
BOOL RetryLockedResource::Wait(BOOL bUseMessageBox)
{
	if (m_bDisabled)
		m_nLockRetry = m_nMaxLockRetry;

	// Lock vero, provo a fare la simulazione di sleep con retry
	if (m_nLockRetry >= m_nMaxLockRetry)
	{
		CString strFullMsg = m_strMsg;

		if (!m_bDisabled)
			strFullMsg += cwsprintf
				(
					_TB("Busy resources for {0-%d} retries for a total of {1-%d} seconds. Try again?"),
					m_nMaxLockRetry,
					m_nMaxLockRetry * m_nMaxLockTime / 1000
				);

		strFullMsg.Trim();

		if 	(
				!bUseMessageBox || 
				AfxMessageBox(strFullMsg, MB_ICONQUESTION | MB_RETRYCANCEL) == IDCANCEL
			)
		{
			return FALSE;
		}
		else
			m_nLockRetry = 0;
	}
	
	// simulazione di sleep	(in millisecondi)
	DWORD startTime = ::GetCurrentTime();
	int nPrevSecond = -1;
	m_nLockRetry++;

	AfxGetApp()->BeginWaitCursor();
	EnableDocumentFrame(FALSE);

	Scheduler aScheduler;
	while (TRUE)
	{
		// permette di cedere il controllo ad altre finestre
		aScheduler.CheckMessage();

		DWORD currTime = ::GetCurrentTime();
		int nTime = (currTime - startTime);

		if ( nTime > m_nMaxLockTime)
			break;

		int nSecond = nTime / 1000;
		if (nSecond != nPrevSecond)
		{
			AfxSetStatusBarText(cwsprintf
				(
					_TB("Busy resources. Trial no.{0-%d}, the job has been suspended for {1-%d} seconds"), 
					m_nLockRetry, 
					(m_nLockRetry - 1) * (m_nMaxLockTime / 1000) + nSecond + 1
				));
			nPrevSecond = nSecond;

			if (!m_bBeepDisabled)
				MessageBeep(MB_ICONEXCLAMATION);
		}
	}

	EnableDocumentFrame(TRUE);
	AfxGetApp()->EndWaitCursor();

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL AFXAPI AfxRunEnumsViewer (CString sCulture /*_T("")*/, CString sInstallation /*_T("")*/)
{
	if (sCulture.IsEmpty())
		sCulture = AfxGetCulture();

	if (sInstallation.IsEmpty())
		sInstallation = AfxGetPathFinder()->GetInstallationName();

	return EnumsViewerWrapper::Open(sCulture, sInstallation);
}

//------------------------------------------------------------------------------
BOOL AFXAPI AfxCloseEnumsViewer ()
{
	return EnumsViewerWrapper::Close();
}

//********************************************************************
//reindirizzamenti per non obbligare ad inserire la dipendenza esplicita da tbgenlibmanaged
//che in alcuni casi da problemi di referenze duplicate (ad es se usato in tbapplicationwrapper
//che a sua volta e' managed
//-----------------------------------------------------------------------------
UINT AFXAPI AfxGetTbLoaderSOAPPort () 
{
	return GetTbLoaderSOAPPort();
}

//-----------------------------------------------------------------------------
UINT AFXAPI AfxGetTbLoaderTCPPort()
{
	return GetTbLoaderTCPPort();
}
//**********************************************************************

//------------------------------------------------------------------------------
BOOL ZCompress(const CString& strFileIn, const CString& strFileOut, const CString& strTitle, CString* sError/* = NULL*/)
{
	return CompressFile(strFileIn, strFileOut, strTitle, sError);
}

//------------------------------------------------------------------------------
BOOL ZUncompress(const CString & strFileIn, const CString & strFileOut, CString* sError/* = NULL*/)
{
	return UncompressFile(strFileIn, strFileOut, sError);
}

//------------------------------------------------------------------------------
HINSTANCE ImageNameSpaceWalking(const CString& strLibraryNamespace)
{
	CTBNamespace	aNS(CTBNamespace::LIBRARY, strLibraryNamespace);
	AddOnLibrary*	pLibrary = AfxGetAddOnLibrary(aNS);
	// la libreria che contiene le immagini potrebbe essere non ancora caricata
	if (!pLibrary)
	{
		AfxGetTbCmdManager()->LoadNeededLibraries(aNS);
		pLibrary = AfxGetAddOnLibrary(aNS);
	}
		
	// a questo punto dovrebbe averla caricata
	if (!pLibrary) return NULL;
	return  pLibrary->m_pAddOnDll->GetInstance();
}


//--------------------------------------------------------------------------------
bool GetImageBytes(LPCTSTR iconSource, CImageBuffer &imageBuffer)
{
	CString strImageNS = iconSource;

	Gdiplus::Color colorMaskPng;
	
	if (strImageNS.Left(6) != _T("Image."))
		strImageNS = _T("Image.") + strImageNS;
	CString sUser;
	if (AfxGetLoginContext())
		sUser = AfxGetLoginInfos()->m_strUserName;

	CString	sImagePath = AfxGetPathFinder()->GetFileNameFromNamespace(CTBNamespace(strImageNS), sUser);

	// Get file extension
	CString	sTest = sImagePath.Right(4);
	// Is a .PNG ?
	if (sImagePath.Right(4).CompareNoCase(_T(".PNG")) == 0 || sImagePath.Right(4).CompareNoCase(_T(".JPG")) == 0)
	{
		// New load PNG
		Gdiplus::Bitmap* gdibitmap = LoadGdiplusBitmapOrPng(AfxGetPathFinder()->GetNamespaceFromPath(sImagePath).ToString(), FALSE, TRUE);

		ASSERT(gdibitmap);
		if (!gdibitmap) return false;
		//Come secondo parametro non serve un Id, visto che qui il CImageBuffer viene usato solo per farsi popolare lo stream di Bytes 
		imageBuffer.Assign(gdibitmap, _T("dummyID"));

		SAFE_DELETE(gdibitmap);
		return true;
	}

	if (sImagePath.Right(4).CompareNoCase(_T(".BMP")) == 0)
	{
		// .Bmp
		HBITMAP hBmp = NULL;
		CImage image;
		image.Load(sImagePath);
		hBmp = image.Detach();
		HDC hdc = GetDC(NULL);
		//Come secondo parametro non serve un Id, visto che qui il CImageBuffer viene usato solo per farsi popolare lo stream di Bytes 
		imageBuffer.Assign(hBmp, _T("dummyID"), hdc);
		ReleaseDC(NULL, hdc);
		DeleteObject(hBmp);
		return true;
	}
	else
	{
		// format image not supported
		ASSERT(FALSE);
		return NULL;
	}
	return false;
}

//////////////////////////////////////////////////////////////////////
//		class TBMessageBox
//////////////////////////////////////////////////////////////////////
//

BEGIN_MESSAGE_MAP(TBMessageBox, CBCGPMessageBox)
	//{{AFX_MSG_MAP(TBMessageBox)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------
TBMessageBox::TBMessageBox(const BCGP_MSGBOXPARAMS* pParams) :
	CBCGPMessageBox(pParams)
{
}

//--------------------------------------------------------------------------------
TBMessageBox::~TBMessageBox()
{
}

//-----------------------------------------------------------------------------
INT_PTR TBMessageBox::DoModal()
{
	CTBWinThread::PumpThreadMessages();  //used to process all pending messages in order to fix active window

	CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);
	m_pParentWnd = GetSafeParent(m_pParentWnd);
	
	INT_PTR res = __super::DoModal();
	
	//attivo la parent (in tabbed windows quando parte una modale perde il fuoco)
	if (m_pParentWnd && AfxGetLoginContext()->GetDocked()) 
		m_pParentWnd->SetFocus();

	if (res == -1 && !AfxGetThreadContext()->IsClosing())
		throw (new CThreadAbortedException());
	return res;
}
