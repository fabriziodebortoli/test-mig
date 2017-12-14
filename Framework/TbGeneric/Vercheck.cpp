// vercheck.cpp : Defines the class behaviors for the application.
//
#include "stdafx.h"

#include <winver.h>
#include <windows.h>
#include <direct.h>
#include <stdlib.h>
#include <stdio.h>
#include <sys\types.h>
#include <sys\stat.h>
#include <string.h>
#include <shlwapi.h>

#include "vercheck.h"


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


// il secondo parametro mi dice quale informazione prendere dal blocco StringFileInfo
//----------------------------------------------------------------------------
CString GetStringFromStringVerInfo(HINSTANCE hInst, LPCTSTR pszName)
{
	CString	strVerInfo;
	DWORD   dwVerInfoSize;
	DWORD   dwVerHnd;
	BOOL    bRetCode;
	TCHAR	szFullPath[_MAX_PATH+1];
	const rsize_t nLen = 256;
	TCHAR    szGetName[nLen];
	
	// get path of dll
	VERIFY(::GetModuleFileName(hInst, szFullPath, _MAX_PATH));

	// Get the file version info size
	dwVerInfoSize = GetFileVersionInfoSize(szFullPath, &dwVerHnd);

	if (dwVerInfoSize == 0) return _T("");
 
	LPTSTR   lpstrVffInfo;
	HANDLE  hMem;
	
	// allocate memory to hold the verinfo block
	hMem          = GlobalAlloc(GMEM_MOVEABLE, dwVerInfoSize);
	lpstrVffInfo  = (LPTSTR) GlobalLock(hMem);
	
	GetFileVersionInfo(szFullPath, dwVerHnd, dwVerInfoSize, lpstrVffInfo);
	
	LPTSTR		lpVerInfo   = NULL;
	DWORD FAR*	lpLangAndCP = NULL; 
	WORD		wVerInfoLen = 0;
	UINT		nSize		= 0;

	// Look for the translation table
	TB_TCSCPY(szGetName, _T("\\VarFileInfo\\Translation"));
	bRetCode =	VerQueryValue
					(
						(LPVOID)lpstrVffInfo, 
						(LPTSTR)szGetName,
						(void FAR* FAR*)&lpLangAndCP,
						&nSize
					);
	if (bRetCode && nSize && lpLangAndCP)
		_sntprintf_s
		(
			szGetName, nLen, sizeof (szGetName),
			_T("\\StringFileInfo\\%08lX\\%s"), 
			MAKELONG(HIWORD(*lpLangAndCP), LOWORD(*lpLangAndCP)),
			pszName
		);
	else
		_sntprintf_s
		(
			szGetName, nLen,sizeof (szGetName),
			_T("\\StringFileInfo\\%s"), 
			pszName
		);

	// Look for the corresponding string.
	bRetCode =	VerQueryValue
					(
						(LPVOID)lpstrVffInfo,
						(LPTSTR)szGetName,
						(void FAR* FAR*)&lpVerInfo,
						(UINT FAR *) &wVerInfoLen
					);

	if (bRetCode && wVerInfoLen && lpVerInfo)
		strVerInfo = lpVerInfo;

    GlobalUnlock(hMem);
    GlobalFree(hMem);

	return strVerInfo;
}


//==============================================================================
//  Determine DLL version number: 
//==============================================================================
BOOL GetDLLVersion
	(
		LPCTSTR	szDLLFileName, 
        DWORD*	pdwMajor		/* = NULL*/, 
		DWORD*	pdwMinor		/* = NULL*/, 
		DWORD*	pdwBuildNumber	/* = NULL*/
	)
{
	if (!pdwMajor && !pdwMinor && !pdwBuildNumber)
		return TRUE;

	if (pdwMajor)
		*pdwMajor = 0;
	if (pdwMinor)
		*pdwMinor = 0;
	if (pdwBuildNumber)
		*pdwBuildNumber = 0;
	
	HINSTANCE   hDllInst;						// Instance of loaded DLL
	TCHAR szFileName [_MAX_PATH];				// Temp file name
	BOOL bRes = TRUE;							// Result
    TB_TCSCPY (szFileName, szDLLFileName);		// Save a file name copy for the loading
    hDllInst = LoadLibrary(szFileName);			// load the DLL
    if (hDllInst) {								// Could successfully load the DLL
        DLLGETVERSIONPROC pDllGetVersion;        
/*
        You must get this function explicitly because earlier versions of the DLL 
        don't implement this function. That makes the lack of implementation of the 
        function a version marker in itself.        
*/
        pDllGetVersion = 
			(
				DLLGETVERSIONPROC) GetProcAddress(hDllInst,
				"DllGetVersion"
			);   
        if(pDllGetVersion) 
		{	// DLL supports version retrieval function
            DLLVERSIONINFO    dvi;            
			ZeroMemory(&dvi, sizeof(dvi));
            dvi.cbSize = sizeof(dvi);
            HRESULT hr = (*pDllGetVersion) (&dvi);
            if( SUCCEEDED(hr) ) 
			{	// Finally, the version is at our hands
				if (pdwMajor)
					*pdwMajor = dvi.dwMajorVersion;
				if (pdwMinor)
					*pdwMinor = dvi.dwMinorVersion;
				if (pdwBuildNumber)
					*pdwBuildNumber = dvi.dwBuildNumber;
            } else
                bRes = FALSE;					// Failure
        } else  // GetProcAddress failed, the DLL cannot tell its version
            bRes = FALSE;       // Failure
        FreeLibrary(hDllInst);  // Release DLL    
	} else  
        bRes = FALSE;   // DLL could not be loaded
    return bRes;
}

// Ritorna il nome del modulo trovato (completo di Path !)
//-----------------------------------------------------------------------------
BOOL GetDLLInfo
			(
				LPTSTR	szDLLFileName,
				LPTSTR	szDLLVersion,
				long	&lFileSize,
				WORD	&wDay,
				WORD	&wMonth,
				WORD	&wYear
			)
{
    DWORD   		dwHandle;
    DWORD  		 	dwVerBufferSize;
    LPTSTR			lpVersionInfo;
	TCHAR			szFullPath[_MAX_PATH+1];

	struct _stat	statBuffer;
	struct tm		newTime;

	//ricerco il path completo del file 
    if (SearchPath(NULL, szDLLFileName, NULL, _MAX_PATH, szFullPath, NULL) == -1) 
		return FALSE;

    TB_TCSCPY(szDLLFileName, szFullPath);
    // Ricavo la dimensione in bytes e la data di creazione del file

	/* Check if statistics are valid: */
	if (! _tstat(szDLLFileName, &statBuffer) )
	{
		lFileSize = (LONG) statBuffer.st_size;
		gmtime_s(&newTime, &(statBuffer.st_mtime));
		wDay	= (WORD)newTime.tm_mday;
		wMonth  = (WORD)newTime.tm_mon + 1;
		wYear   = (WORD)newTime.tm_year;
	}

	dwVerBufferSize = GetFileVersionInfoSize(szDLLFileName, &dwHandle);
	if(dwVerBufferSize == 0)
	{
		TB_TCSCPY(szDLLVersion, _T("*****"));
		return TRUE;
	}

	// ATTENZIONE! la VerQueryValue necessita ASSOLUTAMENTE di parametri
	// allocati dinamicamente
	
	HANDLE	hMem	= GlobalAlloc(GMEM_MOVEABLE, dwVerBufferSize + 1);
	LPVOID	pbData 	= GlobalLock(hMem);
	
	BOOL bOk = GetFileVersionInfo
			(
				szDLLFileName, 
				dwHandle, 
				dwVerBufferSize, 
				pbData
			);


	if (bOk)
	{
		DWORD FAR*	lpLangAndCP = NULL; 
		UINT		nSize = 0;
		const rsize_t nLen = 256;
		TCHAR		szDLLBlockName[nLen];
		TCHAR szSubBlockName[nLen];
		TB_TCSCPY(szSubBlockName, _T("\\VarFileInfo\\Translation"));
		if
		(	VerQueryValue
				(
					pbData,
					szSubBlockName,
					(void FAR* FAR*)&lpLangAndCP,
					&nSize
				)&&
			lpLangAndCP &&
			nSize > 0
		)
			_sntprintf_s
				(
					szDLLBlockName, nLen, sizeof (szDLLBlockName),
					_T("\\StringFileInfo\\%08lX\\FileVersion"), 
					MAKELONG(HIWORD(*lpLangAndCP), LOWORD(*lpLangAndCP))
				);
		else
			TB_TCSCPY(szDLLBlockName, _T("\\StringFileInfo\\FileVersion"));
		
		lpVersionInfo = NULL;
		bOk = VerQueryValue
				(
					pbData,                                      
					szDLLBlockName,
					(void FAR* FAR*)&lpVersionInfo,
					&nSize
				);
			
		bOk = lpVersionInfo && nSize;

		if (bOk)
			TB_TCSCPY(szDLLVersion, (LPCTSTR)lpVersionInfo);
	}

    GlobalUnlock(hMem);
    GlobalFree(hMem);

	return bOk;
}


//=============================================================================
//
// Gestione della versione di prodotto
//
//=============================================================================

//-----------------------------------------------------------------------------
CString GetDllVersion(HINSTANCE hInst)
{

	CString	strVersion;
	DWORD   dwVerInfoSize;
	DWORD   dwVerHnd;
	BOOL    bRetCode;
	const	rsize_t nLen = 256;
	TCHAR	szFullPath[_MAX_PATH+1];
	TCHAR   szGetName[nLen];
	
	VERIFY(::GetModuleFileName(hInst, szFullPath, _MAX_PATH));

	// Get the file version info size
	dwVerInfoSize = GetFileVersionInfoSize(szFullPath, &dwVerHnd);

	if (dwVerInfoSize == 0) return _T("");
 
	LPTSTR   lpstrVffInfo;
	HANDLE  hMem;
	
	// allocate memory to hold the verinfo block
	hMem          = GlobalAlloc(GMEM_MOVEABLE, dwVerInfoSize);
	lpstrVffInfo  = (LPTSTR) GlobalLock(hMem);
	
	GetFileVersionInfo(szFullPath, dwVerHnd, dwVerInfoSize, lpstrVffInfo);
	
	// Do this the American english translation be default.
	// Keep track of the string length for easy updating.
	// 040904E4 represents the language ID and the four
	// least significant digits represent the codepage for
	// which the data is formatted.  The language ID is
	// composed of two parts: the low ten bits represent
	// the major language and the high six bits represent
	// the sub language.
	
	LPTSTR		lpVersion   = NULL;
	DWORD FAR*	lpLangAndCP = NULL; 
	UINT		wVersionLen = 0;
	UINT		nSize		= 0;

	// Look for the translation table
	TB_TCSCPY(szGetName, _T("\\VarFileInfo\\Translation"));
	bRetCode =	VerQueryValue
					(
						(LPVOID)lpstrVffInfo, 
						(LPTSTR)szGetName,
						(void FAR* FAR*)&lpLangAndCP,
						&nSize
					);
	if (bRetCode && nSize && lpLangAndCP)
		_sntprintf_s
		(
			szGetName, nLen, sizeof szGetName,
			_T("\\StringFileInfo\\%08lX\\FileVersion"), 
			MAKELONG(HIWORD(*lpLangAndCP), LOWORD(*lpLangAndCP))
		);
	else
		TB_TCSCPY(szGetName, _T("\\StringFileInfo\\FileVersion"));

	// Look for the corresponding string.
	bRetCode =	VerQueryValue
					(
						(LPVOID)lpstrVffInfo,
						(LPTSTR)szGetName,
						(void FAR* FAR*)&lpVersion,
						(UINT FAR *) &wVersionLen
					);

	if (bRetCode && wVersionLen && lpVersion)
		strVersion = lpVersion;

    GlobalUnlock(hMem);
    GlobalFree(hMem);

	return strVersion;
}

//-----------------------------------------------------------------------------
CString GetTBVersion()
{
	// get path of executable
	HINSTANCE hInst = AfxGetInstanceHandle();
	return GetDllVersion(hInst);
}

#define SEP_CHAR _T(".")
struct DLLVersion
{
	int nMajorVer;
	int nMinorVer;
	int nPatchVer;
	int nBuildVer;
} ;

DLLVersion* BuildDllVersion(LPCTSTR);
BOOL CheckVersion(DLLVersion*, DLLVersion*);

//-----------------------------------------------------------------------------
DLLVersion* BuildDllVersion(LPCTSTR szVersion)
{
	TCHAR* pszVer = new TCHAR[_tcslen(szVersion) + 1];
	TB_TCSCPY(pszVer, szVersion);
	TCHAR* token; 
	TCHAR* nextToken; 
	DLLVersion* pVer = new DLLVersion;

	token = _tcstok_s(pszVer, SEP_CHAR, &nextToken);
	if (token) pVer->nMajorVer = _tstoi(token); else pVer->nMajorVer = 0;
	token = _tcstok_s(NULL, SEP_CHAR, &nextToken);
	if (token) pVer->nMinorVer = _tstoi(token); else pVer->nMinorVer = 0;
	token = _tcstok_s(NULL, SEP_CHAR, &nextToken);
	if (token) pVer->nPatchVer = _tstoi(token); else pVer->nPatchVer = 0;
	token = _tcstok_s(NULL, SEP_CHAR, &nextToken);
	if (token) pVer->nBuildVer = _tstoi(token); else pVer->nBuildVer = 0;
	
	delete[] pszVer;
	return pVer;
}

// controllo se hanno esattamente la stessa versione
// considerando eventualmente se richiesto anche la pathc versione e la build version
//-----------------------------------------------------------------------------
BOOL CheckSameVersion(LPCTSTR pszVer1, LPCTSTR pszVer2, BOOL bCheckPatch /*= FALSE*/, BOOL bCheckBuild /*= FALSE*/)
{
	DLLVersion* pVer1 = BuildDllVersion(pszVer1);
	DLLVersion* pVer2 = BuildDllVersion(pszVer2);

	BOOL bOk =  
		(
			pVer1->nMajorVer == pVer2->nMajorVer &&
			pVer1->nMinorVer == pVer2->nMinorVer &&
			(!bCheckPatch || pVer1->nPatchVer == pVer2->nPatchVer) &&
			(!bCheckBuild || pVer1->nBuildVer == pVer2->nBuildVer)
		);

	delete pVer1;
	delete pVer2;

	return bOk;
}


//-----------------------------------------------------------------------------
BOOL CheckVersion(DLLVersion* pDLLVer, DLLVersion* pToCheckVer)
{
	// se il primo numero della versione della DLL risulta superiore a quello della versione valida
	// va bene
	if (pDLLVer->nMajorVer > pToCheckVer->nMajorVer) return TRUE;

	// se risulta uguale, controllo il secondo numero
	if (pDLLVer->nMajorVer == pToCheckVer->nMajorVer)
	{
		// se è maggiore OK
		if (pDLLVer->nMinorVer > pToCheckVer->nMinorVer) return TRUE;

		// se sono uguali controllo anche il terzo numero
		if (pDLLVer->nMinorVer == pToCheckVer->nMinorVer)
			// se risulta maggiore o uguale OK
			return (pDLLVer->nPatchVer >= pToCheckVer->nPatchVer);
		// se sono uguali controllo anche il quarto numero
		if (pDLLVer->nPatchVer == pToCheckVer->nPatchVer)
			// se risulta maggiore o uguale OK
			return (pDLLVer->nBuildVer >= pToCheckVer->nBuildVer);
	}
	return FALSE;
}

		

// PER IL CONTROLLO DELLE DLL PER ACCESS E IL JET
//-----------------------------------------------------------------------------
BOOL CheckJetDLLVersion(LPCTSTR szFileName, LPCTSTR szVersionToCheck)
{
    TCHAR szDLLFileName[MXWLIB_MAX_FNAME_LEN];
	TCHAR szDLLVersion[MXWLIB_MAX_VERS_LEN];
	TCHAR szVerToCheck[MXWLIB_MAX_VERS_LEN];
	LONG lFileSize;
	WORD wDay;
	WORD wMonth;
	WORD wYear;

	DLLVersion* pDLLVer;
	DLLVersion* pToCheckVer;

	TB_TCSCPY(szDLLFileName, szFileName);
	TB_TCSCPY(szVerToCheck, szVersionToCheck);
	if (GetDLLInfo(szDLLFileName, szDLLVersion, lFileSize, wDay, wMonth, wYear))
	{
		//prima controllo se sono uguali le versioni
		if (_tcscmp(szDLLVersion, szVerToCheck) != 0)
		{
			pDLLVer = BuildDllVersion(szDLLVersion);
			pToCheckVer = BuildDllVersion(szVerToCheck);
			BOOL bOk = CheckVersion(pDLLVer, pToCheckVer);
			delete pDLLVer;
			delete pToCheckVer;
			return bOk;
		}
	}
	return TRUE;
}

// controllo se due dll hanno la stessa identica versione
//-----------------------------------------------------------------------------
BOOL CheckSameDLLVersion(HINSTANCE hFirsInst, HINSTANCE hSecondInst)
{
	CString strFirstVersion = GetDllVersion(hFirsInst);
	CString strSecondVersion = GetDllVersion(hSecondInst);
	return (
				!strFirstVersion.IsEmpty() &&
				!strFirstVersion.IsEmpty() &&
				!strFirstVersion.CompareNoCase(strSecondVersion)
			);
				
}


//-----------------------------------------------------------------------------
BOOL CheckAccessDriverDLLVersion(LPCTSTR szDriverName)
{
	TCHAR szVersion[MXWLIB_MAX_VERS_LEN];
	LONG lFileSize;
	WORD wDay;
	WORD wMonth;
	WORD wYear;

	TCHAR szDLLFileName[MXWLIB_MAX_FNAME_LEN];
	TB_TCSCPY(szDLLFileName, szDriverName);
	if (GetDLLInfo(szDLLFileName, szVersion, lFileSize, wDay, wMonth, wYear) && _tstoi(szVersion) >= 4)	
		return FALSE;
	
	//controllo anche le dll del jet
	if	(
//			!CheckJetDLLVersion("MSEXCL35.dll","3.51.2723.2")	||
//			!CheckJetDLLVersion("MSJT4JLT.dll","3.52.2723.0")	||
//			!CheckJetDLLVersion("MSREPL35.dll","3.51.2404.0")	||
			!CheckJetDLLVersion(_T("MSJET35.dll"), _T("3.51.2723.0"))
		)
		return FALSE;
	
	return TRUE;
}
