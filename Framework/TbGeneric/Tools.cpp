
#include "stdafx.h"

#include <TbNameSolver\Chars.h>
#include "globals.h"

#include "Tools.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//================================================================================
//	Function AfxOpenTmpFile: 
//	Crea ed apre un file temporaneo binario
//	author:  Carlotta Patrucco (vedere sorgenti Wizard) 
//================================================================================
BOOL AfxOpenTmpFile ( CFile& fileTmp, CString& strFileName, LPCTSTR lpszPrefix, LPCTSTR lpszExt) 
{
	TCHAR	szTmpPath[_MAX_PATH];
	TCHAR	szTmpFile[_MAX_PATH];

	if 
		(
			! 
				(
					::GetTempPath(_MAX_PATH, szTmpPath) ?  
					::GetTempFileName(szTmpPath, lpszPrefix, 1, szTmpFile) :
					FALSE
				)
		)
	{
		ASSERT (FALSE);
		TRACE("Error in creating temporary file.");
		return FALSE;
	}
	if ( _tcslen(lpszExt) )
		TB_TCSCPY( &(szTmpFile[ max ( _tcslen(szTmpFile) - 4, 0 ) ] ), lpszExt);
	
	strFileName = szTmpFile;
	if (!fileTmp.Open(szTmpFile, CFile::modeWrite | CFile::modeCreate | CFile::shareDenyNone | CFile::typeBinary ))
	{
		ASSERT_TRACE1(FALSE,"Cannot open temporary file: %s",szTmpFile);
		return FALSE;
	}

	return TRUE;
}

//================================================================================
//	Function AfxLoadAnimCursor: 
//	estrae dalle risorse un file .ANI, lo memorizza in un file temporaneo e lo carica
//	author:  Carlotta Patrucco (vedere sorgenti Wizard) 
//================================================================================

HCURSOR AfxLoadAnimCursor(UINT nCursorResId)
{
	HINSTANCE hInst = AfxFindResourceHandle
						(
							MAKEINTRESOURCE(nCursorResId),
							RT_ANICURSOR
						);
	HRSRC	hResource = hInst ? ::FindResource(hInst, MAKEINTRESOURCE(nCursorResId), RT_ANICURSOR) : NULL;
	HCURSOR	hSearchCursor = hResource ? (HCURSOR)::LoadResource(hInst, hResource) : NULL;
	if (hSearchCursor)
	{
		// Al momento i cursori animati funzionano solamente se caricati direttamente
		// da file utilizzando l'API LoadCursorFromFile (LoadCursor non funziona!!!!)
		// e quindi non mi viene in mente, per ora, nulla di meglio che scaricarmi su
		// un file temporaneo la risorsa....
		CString	strTmpANI;
		CFile	fANIFile;
		DWORD dwSize = SizeofResource (hInst, (HRSRC)hSearchCursor);
		try
		{
			if (
					dwSize && 
					AfxOpenTmpFile( fANIFile, strTmpANI, _T("CUR_"), _T(".ani") )
				)
			{
				LPVOID lpResource = LockResource((HGLOBAL)hSearchCursor);
				if (lpResource)
					fANIFile.Write(lpResource, dwSize); 
				fANIFile.Close();
				UnlockResource(hSearchCursor);
				FreeResource(hSearchCursor);
				hSearchCursor = LoadCursorFromFile (strTmpANI);
				DeleteFile(strTmpANI);
			}
			else
				hSearchCursor = NULL;
		}
		catch(...)
		{
			hSearchCursor = NULL;
		}
	}
	return hSearchCursor;
}

//==============================================================================
//  Implementazione Classe CEnhMetafile
//					 
//==============================================================================
IMPLEMENT_DYNAMIC ( CEnhMetaFile, CObject )
//------------------------------------------------------------------------------
CEnhMetaFile::CEnhMetaFile ()
{
	m_strFileName = "";
	m_hemfMetaFile = (HENHMETAFILE) NULL;
}

//------------------------------------------------------------------------------
CEnhMetaFile::CEnhMetaFile ( const CString& strName )
{
	m_hemfMetaFile = (HENHMETAFILE) NULL;
	LoadFromFile(strName);
}

//------------------------------------------------------------------------------
CEnhMetaFile::~CEnhMetaFile()
{
	if ( m_hemfMetaFile )
		DeleteEnhMetaFile( m_hemfMetaFile );
}

//------------------------------------------------------------------------------
HENHMETAFILE CEnhMetaFile::LoadFromFile(const CString& strName )
{
	m_strFileName = strName;

	if ( m_hemfMetaFile )
		DeleteEnhMetaFile( m_hemfMetaFile );

	return m_hemfMetaFile = GetEnhMetaFile(strName);
}

//------------------------------------------------------------------------------
BOOL CEnhMetaFile::Play ( CDC& dc, const CRect& rectInside )
{
	return PlayEnhMetaFile( dc, m_hemfMetaFile, &rectInside );
}


////////////////////////////////////////////////////////////////////////////////
// converte l'indice passato (n) nella corrispondente label di colonna di Excel
//
CString IndexToExcelLabel (int n)
{	
	CString strLabel;
	LPTSTR pszLabel = strLabel.GetBuffer(5);
	// gestisce fino a "ZZ"
	const int b = 26;
	//ASSERT( n < (26*26) )
	if ( n > (26*26) )
	{	
		pszLabel[0] = '\0';
		return pszLabel;
	}
	// ----
	if ( n > b ) 
	{
		if( n % b ) 
		{
			pszLabel[0] = ('A' - 1 + n / b );	
			pszLabel[1] = ('A' - 1 + n % b );
		}
		else
		{
			pszLabel[0] = ('A' - 2 + n / b );	
			pszLabel[1] = ('A' - 1 + b );
		}
		pszLabel[2] = '\0';
	}
	else 
	{
		pszLabel[0] = ( 'A' - 1 + n );
		pszLabel[1] = '\0';
	}
	strLabel.ReleaseBuffer();
	return strLabel;
}
//-----------------------------------------------------------------------------

typedef struct {
	LPCTSTR pszWindowTitle;
	//size_t nLen;
	BOOL bFinded;
	HWND hWnd;
	LPCTSTR pszSkipParent;
} TypeFindWindow;

static BOOL CALLBACK EnumWindowsProc 
	( 
		HWND hwnd,  // handle to parent window 
		LPARAM lParam // application-defined value 
	)
{
	TCHAR szTitle[400];
	TypeFindWindow* pwfind;

	pwfind = (TypeFindWindow*) lParam;

	CWnd* pWnd = CWnd::FromHandle(hwnd);
	pWnd->GetWindowText(szTitle, 400);

	CString strApp(pwfind->pszWindowTitle);
	CString strWinTitle(szTitle);

	if ( ! strWinTitle.IsEmpty() && strWinTitle.Find(strApp) >= 0)
	{
		BOOL bVisible = pWnd->IsWindowVisible();
		if (!bVisible)
			return TRUE;

		CWnd* pParent = pWnd->GetParent();
		if (pParent == NULL)
		{
			pParent = pWnd->GetOwner();
		}
		if (pParent && pwfind->pszSkipParent && *(pwfind->pszSkipParent))
		{
			pParent->GetWindowText(szTitle, 400);
			CString strParentTitle(szTitle);
			if (strParentTitle.Find(pwfind->pszSkipParent) >= 0)
				return TRUE;
		}

		pwfind->hWnd = hwnd;
		pwfind->bFinded = TRUE;
		return FALSE;
	}
	return TRUE;
}

HWND ThereIsTopWindowWithTitle (LPCTSTR pszTitle, LPCTSTR pszSkipFalseParent/* = NULL*/)
{
	TypeFindWindow wfind;
	
	//wfind.nLen = _tcslen(pszTitle);
	wfind.pszWindowTitle = pszTitle;
	wfind.bFinded = FALSE;
	wfind.hWnd = (HWND)NULL;
	wfind.pszSkipParent = pszSkipFalseParent;

	EnumWindows( EnumWindowsProc, (LPARAM) &wfind ) ;	
	
	return wfind.bFinded ? wfind.hWnd : 0;
}

//-------------------------------------------------------------------------
BOOL IsDCOMInstalled()
{
	if( GetProcAddress(GetModuleHandle(_T("OLE32")), "CoInitializeEx") == NULL) 
		return FALSE; //Il free threading non e' supportato

	HKEY hKey;
	LONG lResult = RegOpenKeyEx
			( 
				HKEY_LOCAL_MACHINE,
				_T("SOFTWARE\\Microsoft\\Ole"),
				0,
				KEY_READ,
				&hKey
			);
	ASSERT_TRACE1( lResult == ERROR_SUCCESS, "opening registry key for OLE failed, error = %d", lResult);
	if ( lResult != ERROR_SUCCESS )
		return FALSE;

	TCHAR rgch[2];
	DWORD cb = sizeof rgch;
	lResult = RegQueryValueEx
				(
					hKey,
					TEXT("EnableDCOM"),
					0, NULL, (LPBYTE)rgch, &cb
				);
	ASSERT_TRACE1( lResult == ERROR_SUCCESS, "Opening registry key for enabling DCOM failed, error = %d", lResult );
	if ( lResult != ERROR_SUCCESS )
		return FALSE;
	
	lResult = RegCloseKey(hKey);
	ASSERT_TRACE1( lResult == ERROR_SUCCESS,"Closing registry failed, error = %d",lResult);
	if ( lResult != ERROR_SUCCESS )
		return FALSE;

	if (rgch[0] != _T('y') && rgch[0] != _T('Y'))
		return FALSE;

	return TRUE;	//DCOM disponibile
}


//=========================================================================
