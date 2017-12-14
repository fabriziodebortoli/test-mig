#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

#define MXWLIB_MAX_VERS_LEN    		128
#define MXWLIB_MAX_MSG_LEN			512
#define MXWLIB_MAX_DLL_NUM			128
#define MXWLIB_MAX_FNAME_LEN   		256
#define MXWLIB_MAX_DESCR_LEN    	512

#define MXWLIB_INF_LINEFMT			"%s%s %d%s %ld%s %d/%d/%d"
#define MXWLIB_INF_MODULE_SECTION	"MODULES"
#define MXWLIB_TOKEN_SEPARATOR		","

TB_EXPORT BOOL	GetDLLVersion		(LPCTSTR, DWORD* = NULL, DWORD* = NULL, DWORD* = NULL);
TB_EXPORT BOOL	GetDLLInfo			(LPTSTR, LPTSTR, long&, WORD&, WORD&, WORD&);
TB_EXPORT CString GetTBVersion		();
TB_EXPORT CString GetDllVersion		(HINSTANCE);
TB_EXPORT BOOL	CheckSameDLLVersion (HINSTANCE, HINSTANCE);

// controllo se hanno esattamente la stessa versione
// considerando eventualmente se richiesto anche la pathc versione e la build version
TB_EXPORT BOOL	CheckSameVersion	(LPCTSTR pszVer1, LPCTSTR pszVer2, BOOL bCheckPatch = FALSE, BOOL bCheckBuild = FALSE);


//per ACCESS e il JET
TB_EXPORT BOOL CheckJetDLLVersion(LPCTSTR, LPCTSTR);
TB_EXPORT BOOL CheckAccessDriverDLLVersion(LPCTSTR);
TB_EXPORT CString GetStringFromStringVerInfo(HINSTANCE hInst, LPCTSTR pszName);

#include "endh.dex"
