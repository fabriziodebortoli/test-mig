// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
/// ==========================================================================
// Name    :    CSettings.h
// Purpose :    simple wrapper class for handling app
//              settings by means of an .ini     

#ifndef SETTINGS_H
#define SETTINGS_H

#include <windows.h>
#include <tchar.h>
#include <list>

#include "util.h"

//defines
//
#define END_OF_STRING   _T( '\0' )
#define BACKSLASH       _T( '\\' )

#define MAX_MSG 4096

const TCHAR g_szSettingsFile[]         = _T("settings.ini");
const TCHAR g_szMstFile[]			   = _T("region.mst");
const TCHAR g_szSetupParameter[]       = _T("SetupParameter");
const TCHAR g_szMsiKey[]               = _T("Msi");

//define
#define LENGTH(A) (sizeof(A)/sizeof(A[0]))


// ==========================================================================
// class CSettings
//
// Purpose:
//  This class wraps an ini file specific to this sample
// ==========================================================================
class CSettings
{
public:
    // Constructors
    CSettings(LPCTSTR pszIni = g_szSettingsFile);
    
    //bool Parse();

	LPCSTR GetMST();

	LANGID DetectLanguage();

    // Getters/Setters
    inline LPCTSTR GetMsi()              { return m_szMsi;              }

    inline HINSTANCE GetHInstance()           { return m_hAppInst;  }
    inline void SetHInstance(HINSTANCE hInst) { m_hAppInst = hInst; }

private:
    
    HINSTANCE   m_hAppInst;
    BOOL        m_bQuietMode;
    TCHAR       m_szIniName[MAX_PATH+1];
    TCHAR       m_szMsi[MAX_PATH+1];
	TCHAR		m_LangBuffer[100];
    std::list<tstring> m_lszLangPackDirList;
};

// global settings object
extern CSettings g_settings;

#endif // SETTINGS_H