// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
/// ==========================================================================
// Name    :    main.h
// Purpose :    #ncludes, prototypes, globals, and const 
//              definitions for setup sample

#include <windows.h>
#include <tchar.h>
#include <string>
#include <io.h>
// ==========================================================================
// InitiateReboot()
//
// Purpose: 
//   initiates a system reboot
// 
// Return Value:
//   false if failed, true if system shutdown is beginning.
//
// ==========================================================================
bool    InitiateReboot();

// ==========================================================================
// ExecCmd()
//
// Purpose:
//  Executes command-line
// Inputs:
//  LPCTSTR pszCmd: command to run
// Outputs:
//  DWORD dwExitCode: exit code from the command
// Notes: This routine does a CreateProcess on the input cmd-line
//        and waits for the launched process to exit.
// ==========================================================================
DWORD   ExecCmd( LPCTSTR pszCmd );


// ==========================================================================
// ExecMSI()
// ==========================================================================
void  ExecMSI();


// ==========================================================================
// ExecMSI()
// Purpose:
//   Excecute msi without language package 
// ==========================================================================
void ExecDefaultMSI();

// ==========================================================================
// GetFileVersionAs3PartString( LPCTSTR szFile, /*OUT*/ tstring & strFileVer)
//
// Purpose: 
//   Retrieves the 3-part version string for the version of a file in the
//   form of: xx.xx.xxxx
// 
// Parameters:
//   LPCTSTR szFile       - The name of the file to get the version of
//   tstring & strFileVer - The string that will contain the version
//
// Return Value:
//   true if the version was found, false otherwise.
// ==========================================================================
bool GetFileVersionAs3PartString( LPCTSTR szFile, /*OUT*/ tstring & strFileVer );



//Detecting If Terminal Services is Installed
// code is taken directly from  
// http://msdndevstg/library/psdk/termserv/termserv_7mp0.htm


/* -------------------------------------------------------------
   Note that the ValidateProductSuite and IsTerminalServices
   functions use ANSI versions of Win32 functions to maintain
   compatibility with Windows 95/98.
   ------------------------------------------------------------- */
   
   

// ==========================================================================
// IsTerminalServicesEnabled()
//
// Purpose: 
//   Determines if Terminal Services are currently enabled on the machine.
// 
// Return Value:
//   true if terminal services are enabled, false otherwise.
// ==========================================================================
bool IsTerminalServicesEnabled();

// ==========================================================================
// SetTSInInstallMode()
//
// Purpose: checks if Terminal Services is enabled and if so
//          switches machine to INSTALL mode
// ==========================================================================
void SetTSInInstallMode();

// ==========================================================================
// LastError()
//
// Purpose: 
//   Returns the last error.
// ==========================================================================
HRESULT LastError();

// ==========================================================================
// ValidateProductSuite()
//
// Purpose:
//  Terminal Services detection code for systems running
//  Windows NT 4.0 and earlier.
// ==========================================================================
bool    ValidateProductSuite (LPSTR lpszSuiteToValidate);

// ==========================================================================
// FxInstallRequired()
//
// Purpose:
//  Checks whether the provided Microsoft .Net Framework redistributable
//  files should be installed to the local machine
//
// ==========================================================================
bool    FxInstallRequired();

// ==========================================================================
// FxRedistRegKeyExists(const tstring& strVersion)
//
// Purpose:
//  Checks whether the FxRedist Registry key exists. This is used in the
//  detection of whether the Fx Needs to be installed on the machine or not.
//
// Parameters:
//  const tstring& strVersion: The 3-part version in the form: x.x.xxxx 
//                             that is the version number of the dotnetfx.exe
//                             redist setup file.
// Return Value:
//  true if the registry key exists, false otherwise.
//
// ==========================================================================
bool    FxRedistRegKeyExists(const tstring& strVersion);

// ==========================================================================
// LPRegKeyExists( const tstring& strVer, const tstring& strLangID )
//
// Purpose:
//  Checks whether the FxRedist langpack Registry key exists. This is used 
//  in the detection of whether the Fx langpack needs to be installed.
//
// Parameters:
//  const tstring& strVer: The 3-part version in the form: x.x.xxxx that 
//                      is the version number of the langpack.exe file.
//
//  const tstring& strLangID: The 4-digit langID to check(i.e. 1033 for ENU)
//
// Return Value:
//  true if the registry key exists, false otherwise.
//
// ==========================================================================
bool    LPRegKeyExists( const tstring& strVer, const tstring& strLangID );


// ==========================================================================
// bool InstallFxRedist(bool & bFxReboot)
//
// Purpose:
//   Installs the .NET Framework redist msi if necessary.
// Inputs:
//   - 
// Outputs:
//   bFxRebot - this boolean value is set to true if installing the .NET fx
//              required a reboot.  This reboot can be defered until the end
//              of all setups.
//   bool return value - true if successful, false if some terminal error
//              occured.  Calling function should not continue on if return
//              value is false.
// ==========================================================================
bool    InstallFxRedist(bool & bFxReboot);


// ==========================================================================
// bool IsValidLangId( const tstring& strLangID)
//
// Purpose:
//   Checks if given language id is valid or not
//
// Description:
//   Checks the given language id against all the valid language ids. If
//   the given ID is not amongst the valid id then returns false otherwise 
//   returns true.
//
// Inputs:
//   strLangID- This is a reference to the string containing the language id
//              of the language we intend to install langpack of.
//
// Outputs:-
//
//   return value - true if the input language id is valid, false otherwise
//
// ==========================================================================
bool IsValidLangId( const tstring& strLangID);


// ==========================================================================
// bool InstallLangPacks(bool & bFxReboot)
//
// Purpose:
//   Installs the .NET Framework langpacks.
//
// Description:
//   Gets the list of langid directory names from the [LangPacks] section 
//   of the ini file. For each directory it tries to find the langpack.exe
//   setup package and install it with full UI.
//   
// Inputs:
//   hInstance- This is handle to the current instance. This is used to load 
//              the string from the string table.
// Outputs:
//   bRebot   - this boolean value is set to true if installing the .NET fx
//              required a reboot.  This reboot can be defered until the end
//              of all setups.
//
//   return value - true if successful, false if some terminal error
//              occured.  Calling function should not continue on if return
//              value is false.
// ==========================================================================
bool    InstallLangPacks(bool & bReboot,HINSTANCE hInstance);

// ==========================================================================
// bool InstallHostMSI(bool & bAppReboot)
//
// Purpose:
//   Installs the host MSI setup
//
// Outputs:
//   bAppRebot- this boolean value is set to true if installing the Host MSI
//              requires a reboot at the end of setup.
//
//   return value - true if successful, false if some terminal error
//              occured.  Calling function should not continue on if return
//              value is false.
// ==========================================================================
bool    InstallHostMSI(bool & bAppReboot);


// ==========================================================================
// bool ExecuteExe()
// ==========================================================================
void    ExecuteCSharp();

//    Private message to tell the thread to destroy the window
const UINT  PWM_THREADDESTROYWND = WM_USER; 

// single instance data
//
const TCHAR g_tszBootstrapMutexName[] = _T( "NDP FxApp Install Bootstrapper" );

// dotnetfx.exe installer app name
// and command-line args
//
const TCHAR g_tszFxInstaller[] = _T("dotnetfx.exe");
const TCHAR g_tszLPInstaller[] = _T("langpack.exe");
const TCHAR g_tszFxInstallerCmdLine[] = _T("/q:a /c:\"install /l \"");
const TCHAR g_tszLpInstallerCmdLine[] = _T("/q:a /c:\"inst /l \"");

// msi install cmd-line
const TCHAR g_tszMsiCmdLine[] = _T("Msiexec /I \"%s\" TRANSFORMS=\"%s\"");

// msi install cmd-lint without language pack
const TCHAR g_tszMsiDefaultCmdLine[] = _T("Msiexec /I \"%s\"");

// reg key for fx policy info
// used to detect if fx is installed
// this key resides in HKEY_LOCAL_MACHINE
const TCHAR g_tszFxRegKeyRoot[] = _T("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\");
const TCHAR g_tszFxENUKey[] = _T("1033");
const TCHAR g_tszKeySeparator[] = _T("\\");
const TCHAR g_tszKeyVersionFlag[] = _T("v");


const TCHAR   g_tszTSChangeUserToInstall[] = _T("change user /INSTALL");

//RegValue name which is is used to detect if .net framework is installed
const TCHAR g_tszInstallKeyName[] = _T("Install");
//If Install value name has value = 1 that means .net framework is installed
const DWORD g_dwInstallTrue = 1;