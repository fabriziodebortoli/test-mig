// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
/// ==========================================================================
// Name    :    CSettings.cpp
// Purpose :    simple wrapper class for handling app
//              settings by means of an .ini file

// includes
#include <windows.h>
#include <shlwapi.h>
#include <stdio.h>
#include "CSettings.h"
#include <io.h>

// ==========================================================================
// CSetting::CSetting()
//
// Purpose:
//  parameterless CSetting object construction
// ==========================================================================
CSettings::CSettings(LPCTSTR pszIniName)
{
    m_bQuietMode = false;

   
    TCHAR * pszWalk = NULL;

    *m_szMsi              = END_OF_STRING;
	
	GetModuleFileName( g_settings.GetHInstance(), 
                       m_szIniName, 
                       LENGTH(m_szIniName) );
    
    pszWalk = _tcsrchr( m_szIniName, BACKSLASH );

    if (pszWalk)
    {
        pszWalk++; // preserve trailing '\'
        *pszWalk = END_OF_STRING;

    }

	// pszIniName is never NULL due to ctor init
    StrCatBuff(m_szIniName, pszIniName, LENGTH(m_szIniName)); 


	GetPrivateProfileString(g_szSetupParameter,
                                     g_szMsiKey,
                                     NULL,
                                     m_szMsi,
                                     LENGTH(m_szMsi),
                                     m_szIniName);
}


//Get MST file according to the language
LPCSTR CSettings ::GetMST()
{
	LANGID lanID= DetectLanguage(); //local user language setting

	_itot(lanID,m_LangBuffer,10);

	StrCatBuff(m_LangBuffer,g_szMstFile,100);

	return m_LangBuffer;
}

//--------------------------
//detect system lang
//--------------------------
LANGID CSettings::DetectLanguage() 
{ 
	#define MAX_KEY_BUFFER 80 

	OSVERSIONINFO VersionInfo; 
	LANGID uiLangID = 0; 
	HKEY hKey; 
	DWORD Type, BuffLen = MAX_KEY_BUFFER; 
	TCHAR LangKeyValue[MAX_KEY_BUFFER]; 


	VersionInfo.dwOSVersionInfoSize = sizeof(OSVERSIONINFO); 
	if( !GetVersionEx(&VersionInfo) ) 
		return(0); 

	switch( VersionInfo.dwPlatformId ) 
	{ 
	// On Windows NT, Windows 2000 or higher 
		case VER_PLATFORM_WIN32_NT: 
			if( VersionInfo.dwMajorVersion >= 5) // Windows 2000 or higher 
				uiLangID = GetUserDefaultLangID(); 
			break; 
		// On Windows 95, Windows 98 or Windows ME 
		case VER_PLATFORM_WIN32_WINDOWS: 
			// Open the registry key for the UI language 
			if( RegOpenKeyEx(HKEY_CURRENT_USER,_T("Default\\Control Panel\\Desktop\\ResourceLocale"), 0, 
				KEY_QUERY_VALUE, &hKey) == ERROR_SUCCESS ) 
			{ 
				// Get the type of the default key 
				if( RegQueryValueEx(hKey, NULL, NULL, &Type, NULL, NULL) == ERROR_SUCCESS 
				&& Type == REG_SZ ) 
				{ 
					// Read the key value 
					if( RegQueryValueEx(hKey, NULL, NULL, &Type, (LPBYTE)LangKeyValue, &BuffLen) 
					== ERROR_SUCCESS ) 
					{ 
						uiLangID = _ttoi(LangKeyValue); 
					} 
				} 
				RegCloseKey(hKey); 
			} 
			break; 
	} 

	if (uiLangID == 0) 
	{ 
		uiLangID = GetUserDefaultLangID(); 
	} 
	// Return the found language ID. 
	return (uiLangID); 
}