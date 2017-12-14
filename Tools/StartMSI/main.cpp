// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
/// =======================================================================
// Name    :    main.cpp
// Purpose :    Windows bootstrap application that installs the 
//              Microsoft .Net Framework redistributable files,
//              if necessary, and an additional application msi.

// includes
#include <windows.h>
#include <tchar.h>
#include <stdio.h>
#include "CSettings.h"          // ini-based app globals/settings
//#include "resource.h"           // string defines

#include "main.h"

CSettings g_settings;
// ==========================================================================
// WinMain()
//
// Purpose: application entry point
//
// ==========================================================================
int APIENTRY WinMain(HINSTANCE hInstance,
                     HINSTANCE hPrevInstance,
                     LPSTR     lpCmdLine,
                     int       nCmdShow)
{
    UINT    uRetCode = 0;       // bootstrapper return code
  

    // Initialize Current Working Directory
    TCHAR szCurMod[_MAX_PATH + 1] = {0};
    TCHAR szDrive[_MAX_PATH + 1] = {0};
    TCHAR szDir[_MAX_PATH + 1] = {0};
    GetModuleFileName(NULL, szCurMod, _MAX_PATH);

    _tsplitpath(szCurMod, szDrive, szDir, NULL, NULL);
    _tmakepath(szCurMod, szDrive, szDir, NULL, NULL);
    SetCurrentDirectory(szCurMod);
    
    try 
    {
        if(0 == _taccess(g_settings.GetMST(), 00))
			ExecMSI();
		else
			ExecDefaultMSI();
    }
    catch( ... )
    {
		::MessageBox(NULL, "Error found!!\r\nPlease check files: *.msi, *.mst, Settings.ini", "Error", MB_OK);
    }

    return uRetCode;
}

void ExecMSI()
{
	DWORD dwResult;
	TCHAR szMsiInstallCmd[ MAX_PATH + LENGTH(g_tszMsiCmdLine) + 2];

	_sntprintf( szMsiInstallCmd, 
                LENGTH(szMsiInstallCmd) - 1, 
                g_tszMsiCmdLine, 
                g_settings.GetMsi(),
				g_settings.GetMST());

    dwResult = ExecCmd(szMsiInstallCmd);
}

void ExecDefaultMSI()
{
	DWORD dwResult;
	TCHAR szMsiDefaultInstallCmd[ MAX_PATH + LENGTH(g_tszMsiDefaultCmdLine) + 2];
	_sntprintf( szMsiDefaultInstallCmd, 
                LENGTH(szMsiDefaultInstallCmd) - 1, 
                g_tszMsiDefaultCmdLine, 
                g_settings.GetMsi());

    dwResult = ExecCmd(szMsiDefaultInstallCmd);
}

DWORD ExecCmd( LPCTSTR pszCmd )
{
    BOOL  bReturnVal   = false ;
    STARTUPINFO  si ;
    DWORD  dwExitCode ;
    SECURITY_ATTRIBUTES saProcess, saThread ;
    PROCESS_INFORMATION process_info ;

    ZeroMemory(&si, sizeof(si)) ;
    si.cb = sizeof(si) ;

    saProcess.nLength = sizeof(saProcess) ;
    saProcess.lpSecurityDescriptor = NULL ;
    saProcess.bInheritHandle = TRUE ;

    saThread.nLength = sizeof(saThread) ;
    saThread.lpSecurityDescriptor = NULL ;
    saThread.bInheritHandle = FALSE ;

    bReturnVal = CreateProcess(NULL, 
                               (LPTSTR)pszCmd, 
                               &saProcess, 
                               &saThread, 
                               FALSE, 
                               DETACHED_PROCESS, 
                               NULL, 
                               NULL, 
                               &si, 
                               &process_info) ;

    if (bReturnVal)
    {
        CloseHandle( process_info.hThread ) ;
        WaitForSingleObject( process_info.hProcess, INFINITE ) ;
        GetExitCodeProcess( process_info.hProcess, &dwExitCode ) ;
        CloseHandle( process_info.hProcess ) ;
    }
    else
    {
		throw "Create process failure";
    }

    return dwExitCode;
}
