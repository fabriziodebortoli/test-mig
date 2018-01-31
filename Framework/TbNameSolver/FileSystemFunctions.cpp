#include "stdafx.h"

#include <sys\stat.h>

#include "chars.h"
#include "FileSystemFunctions.h"
#include "IFileSystemManager.h"
#include "Strsafe.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

static const TCHAR szComputerName[]	= _T("ComputerName");
static const TCHAR szUserName[] 	= _T("UserName");

typedef void (WINAPI *PGNSI)(LPSYSTEM_INFO);
typedef BOOL (WINAPI *PGPI)(DWORD, DWORD, DWORD, DWORD, PDWORD);

// gestione dei nomi lunghi
#define MAX_DOSNAME_LEN _MAX_FNAME

#define BUFSIZE 2048

// File management
//=============================================================================
//-----------------------------------------------------------------------------
TB_EXPORT BOOL	IsDirSeparator	(TCHAR ch)
{
	return ch == URL_SLASH_CHAR || ch == SLASH_CHAR; 
}

//-----------------------------------------------------------------------------
TB_EXPORT BOOL	IsDirSeparator	(LPCTSTR s)
{ 
	return IsDirSeparator(s[0]); 
}

// It checks if the name is in one of next correct forms:
//
//      .
//      ..
//      <fileName>[.[<ext>]]
//-----------------------------------------------------------------------------
BOOL name_ok(const CString& strName, int maxLen)
{
	int nLen = strName.GetLength();
	if (nLen == 1 && strName[0] == _T('.'))
		return TRUE;
	else if (nLen == 2 && strName[0] == _T('.') && strName[1] == _T('.'))
		return TRUE;

	int k = strName.ReverseFind(DOT_CHAR);
	if (k == -1) k = strName.GetLength();

	// checks for correct length
	if ((k < 1) || (k > maxLen)) return FALSE;
	int n = strName.FindOneOf (_T("\\/:*?<>|"));

	return n < 0;
}

// It checks if name is in a dos file/path form:
//  [X: | \\<computer_name>\<resource_name> ][<pathExpr>][\[<filename>]]
//-----------------------------------------------------------------------------
BOOL IsDosName(const CString& strName, BOOL bCheckFullPath /* = FALSE */)
{
	if (strName.IsEmpty())
		return FALSE;

	// strName can't be only a drive name
	if (IsDriveName(strName))
		return FALSE;

	int i = strName.Find(_T(':'));     // if it fail i = -1

	if (i >= 0)
	{
		if ((i != 1) || !_istalpha(strName[0]))
			return FALSE;   // it isn't a valid drive name
		
		i++;
	}
    else
    {
		i = 0;
    	
		if (IsServerPath(strName))
		{                   
			// network name
			//             
			// find computer_name
			CString strNetworkPath = strName.Mid(2);
			i = strNetworkPath.FindOneOf(UNC_SLASH_CHARS);   // if it fail i = -1
			
			if (i < 0)
				return FALSE;
			               
			// check computer_name
			if (!name_ok(strNetworkPath.Left(i), i))
				return FALSE;

			// find resource_name
			strNetworkPath = strNetworkPath.Mid(++i);
			int j = strNetworkPath.FindOneOf(UNC_SLASH_CHARS);
			if (j < 0)
				return FALSE;
			
			// check resource_name
			if (!name_ok(strNetworkPath.Left(j), j))
				return FALSE;

			i += 2 + j;		// correct position into strName buffer of fourth "\"
		}
    	else if (bCheckFullPath && !IsDirSeparator(strName[0]))
			return FALSE;
	}
	                  
	if ((i >= strName.GetLength()) || (IsDirSeparator(strName[i]) && (++i) >= strName.GetLength()))
		return TRUE;    // it is a root (X:\ or \ or \\computername\)

	CString strTmp = strName.Mid(i);   // get the rest of name

	// split the name into his components

	for (;;)
	{
		i = strTmp.FindOneOf(UNC_SLASH_CHARS);
		if (i < 0) break;

		if (!name_ok(strTmp.Left(i), MAX_DOSNAME_LEN)) return FALSE;

		if ((++i) >= strTmp.GetLength()) return TRUE;   // last TCHAR of name is a '\'

		strTmp = strTmp.Mid(i);
	}

	// it checks for last segment not terminated by '\'
	return name_ok(strTmp, MAX_DOSNAME_LEN);
}

//-----------------------------------------------------------------------------
// It checks if name is a path in the form:
//		\\computer_name\resurce_name
//-----------------------------------------------------------------------------
BOOL IsServerPath(const CString& strName)
{
	TCHAR* pChar = (TCHAR*)(LPCTSTR) strName;
	if (*pChar != SLASH_CHAR && *pChar != URL_SLASH_CHAR)
		return FALSE;
	
	TCHAR pChar1 = *pChar;
	pChar++;
	return pChar1 == *pChar;
}

//-----------------------------------------------------------------------------
// It checks if name is a drive in the form:
//      X:
// or
//		\\computer_name\resurce_name
//-----------------------------------------------------------------------------
BOOL IsDriveName(const CString& strName)
{
	if (strName.IsEmpty())
		return FALSE;
		
	if	(IsServerPath(strName))
	{
		// network name : \\computer_name\resurce_name
		//
		// find computer_name
		CString strNetworkPath = strName.Mid(2);
	
		int i = strNetworkPath.FindOneOf(UNC_SLASH_CHARS);     // if it fail i = -1
		
		if (i < 0)
			return FALSE;
			               
		// check computer_name
		if (!name_ok(strNetworkPath.Left(i), i))
			return FALSE;

		// check resource_name
		return name_ok(strNetworkPath.Mid(++i), 128);
	}
	
	return (strName.GetLength() == 2) && _istalpha(strName[0]) && (strName[1] == _T(':'));
}

//  It returns the BaseName of a correct dos file name
//-----------------------------------------------------------------------------
CString GetPath (const CString& strName, BOOL bAppendBkSlash /*= FALSE */)
{
	if (strName.IsEmpty() || !IsDosName(strName)) return _T("");

	TCHAR szDrive  [_MAX_DRIVE];
	TCHAR szDir    [_MAX_DIR];
	TCHAR szFile   [_MAX_FNAME];
	TCHAR szExt    [_MAX_EXT];

	_tsplitpath_s(strName, szDrive,_MAX_DRIVE, szDir, _MAX_DIR, szFile, _MAX_FNAME, szExt, _MAX_EXT);

	int i = _tcslen(szDir) - 1;
	if (IsDirSeparator((szDir[i]))) szDir[i] = NULL_CHAR;

	CString tmpPath(szDrive);
	tmpPath += CString(szDir);

	if (bAppendBkSlash)
		tmpPath += SLASH_CHAR;

	return tmpPath;
}

//-----------------------------------------------------------------------------
CString GetName (const CString& strName)
{
	if (strName.IsEmpty()) return _T("");

	TCHAR szDrive  [_MAX_DRIVE];
	TCHAR szDir    [_MAX_DIR];
	TCHAR szFile   [_MAX_FNAME];
	TCHAR szExt    [_MAX_EXT];

	_tsplitpath_s(strName, szDrive, _MAX_DRIVE, szDir, _MAX_DIR, szFile, _MAX_FNAME, szExt, _MAX_EXT);

	return szFile;
}

//------------------------------------------------------------------------------
CString MakeName (const CString& strFile, const CString& strExt)
{
	CString	strTmp;
	int		point_pos = strFile.ReverseFind(DOT_CHAR);

    // create name for report program. 
	if (point_pos >= 0)
		strTmp = strFile.Left(point_pos);
    else
		strTmp = strFile;

	if (strExt[0] != DOT_CHAR && (strTmp.IsEmpty() ? TRUE : strTmp[strTmp.GetLength() - 1] != DOT_CHAR))
		strTmp += '.';

	strTmp += strExt;

	//strTmp.Replace('<', '(');
	//strTmp.Replace('>', ')');
	//strTmp.Replace('|', '-');
	//strTmp.Replace('\\', '-');
	//strTmp.Replace('/', '-');
	//strTmp.Replace('?', ' ');
	//strTmp.Replace('*', ' ');
	
	return strTmp;
}

// Preserves multiple extensions. MakeName function would substitute 
// all extension with the last one recognized as previously wanted behaviour 
//-----------------------------------------------------------------------------
CString GetNameWithExtension (const CString& strName)
{
	if (strName.IsEmpty()) return _T("");

	TCHAR szDrive  [_MAX_DRIVE];
	TCHAR szDir    [_MAX_DIR];
	TCHAR szFile   [_MAX_FNAME];
	TCHAR szExt    [_MAX_EXT];

	_tsplitpath_s(strName, szDrive, _MAX_DRIVE, szDir, _MAX_DIR, szFile, _MAX_FNAME, szExt, _MAX_EXT);

	return CString(szFile) + szExt;
}

//-----------------------------------------------------------------------------
CString RemoveExtension (const CString& strName)
{
	if (strName.IsEmpty()) return _T("");

	int idx = strName.ReverseFind('.');
	if (idx < 0) 
		return strName;

	return strName.Left(idx);
}

// I cannot use GetName() to remove extension as _tsplitpath does not support
// multiple dotchar in name
//ATTENZIONE che a volte arriva un PATH completo (CTBExplorerUserDlg::OnSavePath())
//-----------------------------------------------------------------------------
BOOL IsValidObjName(const CString& strName, BOOL bCheckFullPath /* = FALSE */)
{
	if (!IsDosName(strName, bCheckFullPath))
		return FALSE;

	CString sName = strName;

	if (!bCheckFullPath && IsDosName(strName, TRUE))
	{
		int nIdx = sName.ReverseFind('\\');
		if (nIdx >= 0)
			sName = sName.Mid(nIdx + 1);
		nIdx = sName.ReverseFind('.');
		if (nIdx >= 0)
			sName.SetAt(nIdx, '_');
	}

	// file system not supported chars plus blank and dot
	for (int k = 0; k < sName.GetLength(); k++)
		if (
				sName[k] <= ' ' || sName[k] == '\"' || sName[k] == '/' || sName[k] == '?' ||
				sName[k] == '*' || sName[k] == '\\' || sName[k] == '<' || sName[k] == '>' ||
				sName[k] == ':' || sName[k] == '|' || sName[k] == '.' 
			)
		{
			ASSERT(FALSE);
			TRACE( _T("Invalid name %s\n"), strName);
			return FALSE;
		}


	return TRUE;
}

//-----------------------------------------------------------------------------
CString GetDriver (const CString& strName)
{
	if (strName.IsEmpty()) return _T("");

	TCHAR szDrive  [_MAX_DRIVE];
	TCHAR szDir    [_MAX_DIR];
	TCHAR szFile   [_MAX_FNAME];
	TCHAR szExt    [_MAX_EXT];

	_tsplitpath_s(strName, szDrive, _MAX_DRIVE, szDir, _MAX_DIR, szFile, _MAX_FNAME, szExt, _MAX_EXT);

	return szDrive;
}

//-----------------------------------------------------------------------------
// It checks existence of a path
//-----------------------------------------------------------------------------
BOOL ExistPath (const CString& strName)
{
	if (strName.IsEmpty()) return TRUE;

	BOOL bOk = FALSE;

	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();

	if (pFileSystemManager)
		bOk = pFileSystemManager->ExistPath (strName);
	else
	{
		CString strTmp (strName);

			if (IsDriveName (strName))
			strTmp = strName + CString(SLASH_CHAR);
		else
			if (IsDirSeparator(strName[strName.GetLength() - 1]))
			{
				strTmp = strName.Left(strName.GetLength() - 1);
				if (IsDriveName (strTmp)) strTmp = strName;
			}

		struct _stat statbuf;
		bOk =  (_tstat((TCHAR*)(LPCTSTR)strTmp, &statbuf) == 0) && ((statbuf.st_mode & _S_IFDIR) == _S_IFDIR);
	}

	return bOk;
}                                                             


//-----------------------------------------------------------------------------
// It checks existence of a file
//-----------------------------------------------------------------------------
BOOL ExistFile(const CString& strName)
{
	if (strName.IsEmpty()) return FALSE;

	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();

	if (pFileSystemManager)
		return pFileSystemManager->ExistFile (strName);
	
	CString strTmp (strName);

		if (IsDriveName (strName))
		strTmp = strName + CString(SLASH_CHAR);
	else
		if (IsDirSeparator(strName[strName.GetLength() - 1]))
		{
			strTmp = strName.Left(strName.GetLength() - 1);
			if (IsDriveName (strTmp)) strTmp = strName;
		}

	struct _stat statbuf;
	return (_tstat((TCHAR*)(LPCTSTR)strName, &statbuf) == 0) && ((statbuf.st_mode & _S_IFREG) == _S_IFREG);
}


//-----------------------------------------------------------------------------
TB_EXPORT TBFile* GetTBFile(const CString& name)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	//if (pFileSystemManager)
	//	return pFileSystemManager->GetTBFile(strName);
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL DeleteFile (const CString& strName)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (pFileSystemManager)
		return pFileSystemManager->RemoveFile (strName);

	if (_taccess(strName, 02) == -1)
		_tchmod (strName, _S_IREAD | _S_IWRITE);

	return _tremove ((LPCTSTR) strName) == 0;
}

//-----------------------------------------------------------------------------
BOOL RemoveFile (const CString& strName)
{
	return DeleteFile(strName);
}

//-----------------------------------------------------------------------------
BOOL RenameFile(const CString& sOldFileName, const CString& sNewFileName)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	if (pFileSystemManager)
		return pFileSystemManager->RenameFile(sOldFileName, sNewFileName);

	return _trename(sOldFileName, sNewFileName) == 0;
}

// se necessario, elimina eventuali caratteri speciali per evitare 
// turbamenti nelle query di lock con apici o cose strane
//-----------------------------------------------------------------------------
CString GetComputerName(BOOL bStripSpecial) 
{
	TCHAR szName[MAX_COMPUTERNAME_LENGTH + 1];
	DWORD dwSize = MAX_COMPUTERNAME_LENGTH + 1;

	if (!::GetComputerName(szName, &dwSize))
		return szComputerName;

	if (bStripSpecial)
		for (size_t i = 0; i < _tcslen(szName); i++)
			if (_istalnum(szName[i]) == 0) szName[i] = BLANK_CHAR;

	CString strName = szName;
	if (strName.IsEmpty()) strName = szComputerName;

	return strName;
}
#include <Psapi.h.>

//-----------------------------------------------------------------------------
TB_EXPORT CString GetProcessFileName()
{
	CString sName;

    // Get a handle to the process.

    HANDLE hProcess = OpenProcess( PROCESS_QUERY_INFORMATION |
                                   PROCESS_VM_READ,
								   FALSE, ::GetCurrentProcessId() );

    // Get the process name.

    if (NULL != hProcess )
    {
        HMODULE hMod;
        DWORD cbNeeded;

        if ( EnumProcessModules( hProcess, &hMod, sizeof(hMod), 
             &cbNeeded) )
        {
			GetModuleFileName(hMod, sName.GetBuffer(_MAX_FNAME+1), _MAX_FNAME+1 );
        }
    }

    // Print the process name and identifier.

    CloseHandle( hProcess );

	sName.ReleaseBuffer();
	return sName;

}

// se necessario, elimina eventuali caratteri speciali per evitare 
// turbamenti nelle query di lock con apici o cose strane
//-----------------------------------------------------------------------------
CString GetUserName(BOOL bStripSpecial)
{ 
	TCHAR szName[255];
	DWORD dwSize = 255;
	
	if (!::GetUserName(szName, &dwSize))
		return szUserName;

	if (bStripSpecial)
		for (size_t i = 0; i < _tcslen(szName); i++)
			if (_istalnum(szName[i]) == 0) szName[i] = BLANK_CHAR;

	CString strName = szName;
	if (strName.IsEmpty()) strName = szUserName;

	return strName;
}

#pragma warning (disable : 4996)	
//-----------------------------------------------------------------------------
BOOL IsWorkStation()
{
	OSVERSIONINFOEX osvi;
	ZeroMemory(&osvi, sizeof(OSVERSIONINFOEX));
	osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
	GetVersionEx((LPOSVERSIONINFO)&osvi);
	if (osvi.wProductType == VER_NT_WORKSTATION)
	{
		// Workstation
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL GetOSDisplayString(CString& strOS)
{
	OSVERSIONINFOEX osvi;
	SYSTEM_INFO si;
	PGNSI pGNSI;
	PGPI pGPI;
	DWORD dwType;

	ZeroMemory(&si, sizeof(SYSTEM_INFO));
	ZeroMemory(&osvi, sizeof(OSVERSIONINFOEX));

	osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
	if (!GetVersionEx((OSVERSIONINFO*) &osvi))
		return FALSE;

	pGNSI = (PGNSI) GetProcAddress(GetModuleHandle(TEXT("kernel32.dll")), "GetNativeSystemInfo");
	if (pGNSI != NULL)
		pGNSI(&si);
	else 
		GetSystemInfo(&si);

	if (osvi.dwPlatformId == VER_PLATFORM_WIN32_NT &&  osvi.dwMajorVersion > 4 )
	{
		TCHAR* pszOS = strOS.GetBuffer(BUFSIZE);
	
		StringCchCopy(pszOS, BUFSIZE, TEXT("Microsoft "));

		// Test for the specific product.

		if ( osvi.dwMajorVersion == 6 )
		{
			if( osvi.dwMinorVersion == 0 )
			{
				if( osvi.wProductType == VER_NT_WORKSTATION )
					StringCchCat(pszOS, BUFSIZE, TEXT("Windows Vista "));
				else 
					StringCchCat(pszOS, BUFSIZE, TEXT("Windows Server 2008 " ));
			}

			if ( osvi.dwMinorVersion == 1 || osvi.dwMinorVersion == 2 )
			{
				if ( osvi.wProductType == VER_NT_WORKSTATION && osvi.dwMinorVersion == 1)
					StringCchCat(pszOS, BUFSIZE, TEXT("Windows 7 "));
				else if ( osvi.wProductType == VER_NT_WORKSTATION && osvi.dwMinorVersion == 2)
					StringCchCat(pszOS, BUFSIZE, TEXT("Windows 8 "));
				else 
					StringCchCat(pszOS, BUFSIZE, TEXT("Windows Server 2008 R2 " ));
			}
         
			pGPI = (PGPI) GetProcAddress(GetModuleHandle(TEXT("kernel32.dll")), "GetProductInfo");
			pGPI( osvi.dwMajorVersion, osvi.dwMinorVersion, 0, 0, &dwType);

			switch( dwType )
			{
				case PRODUCT_ULTIMATE:
					StringCchCat(pszOS, BUFSIZE, TEXT("Ultimate Edition" ));
					break;
				case PRODUCT_PROFESSIONAL:
					StringCchCat(pszOS, BUFSIZE, TEXT("Professional" ));
					break;
				case PRODUCT_HOME_PREMIUM:
					StringCchCat(pszOS, BUFSIZE, TEXT("Home Premium Edition" ));
					break;
				case PRODUCT_HOME_BASIC:
					StringCchCat(pszOS, BUFSIZE, TEXT("Home Basic Edition" ));
					break;
				case PRODUCT_ENTERPRISE:
					StringCchCat(pszOS, BUFSIZE, TEXT("Enterprise Edition" ));
					break;
				case PRODUCT_BUSINESS:
					StringCchCat(pszOS, BUFSIZE, TEXT("Business Edition" ));
					break;
				case PRODUCT_STARTER:
					StringCchCat(pszOS, BUFSIZE, TEXT("Starter Edition" ));
					break;
				case PRODUCT_CLUSTER_SERVER:
					StringCchCat(pszOS, BUFSIZE, TEXT("Cluster Server Edition" ));
					break;
				case PRODUCT_DATACENTER_SERVER:
					StringCchCat(pszOS, BUFSIZE, TEXT("Datacenter Edition" ));
					break;
				case PRODUCT_DATACENTER_SERVER_CORE:
					StringCchCat(pszOS, BUFSIZE, TEXT("Datacenter Edition (core installation)" ));
					break;
				case PRODUCT_ENTERPRISE_SERVER:
					StringCchCat(pszOS, BUFSIZE, TEXT("Enterprise Edition" ));
					break;
				case PRODUCT_ENTERPRISE_SERVER_CORE:
					StringCchCat(pszOS, BUFSIZE, TEXT("Enterprise Edition (core installation)" ));
					break;
				case PRODUCT_ENTERPRISE_SERVER_IA64:
					StringCchCat(pszOS, BUFSIZE, TEXT("Enterprise Edition for Itanium-based Systems" ));
					break;
				case PRODUCT_SMALLBUSINESS_SERVER:
					StringCchCat(pszOS, BUFSIZE, TEXT("Small Business Server" ));
					break;
				case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM:
					StringCchCat(pszOS, BUFSIZE, TEXT("Small Business Server Premium Edition" ));
					break;
				case PRODUCT_STANDARD_SERVER:
					StringCchCat(pszOS, BUFSIZE, TEXT("Standard Edition" ));
					break;
				case PRODUCT_STANDARD_SERVER_CORE:
					StringCchCat(pszOS, BUFSIZE, TEXT("Standard Edition (core installation)" ));
					break;
				case PRODUCT_WEB_SERVER:
					StringCchCat(pszOS, BUFSIZE, TEXT("Web Server Edition" ));
					break;
			}
		}

		if ( osvi.dwMajorVersion == 5 && osvi.dwMinorVersion == 2 )
		{
			if ( GetSystemMetrics(SM_SERVERR2) )
				StringCchCat(pszOS, BUFSIZE, TEXT( "Windows Server 2003 R2, "));
			else if ( osvi.wSuiteMask & VER_SUITE_STORAGE_SERVER )
				StringCchCat(pszOS, BUFSIZE, TEXT( "Windows Storage Server 2003"));
			else if ( osvi.wSuiteMask & VER_SUITE_WH_SERVER )
				StringCchCat(pszOS, BUFSIZE, TEXT( "Windows Home Server"));
			else if( osvi.wProductType == VER_NT_WORKSTATION && si.wProcessorArchitecture==PROCESSOR_ARCHITECTURE_AMD64)
			   StringCchCat(pszOS, BUFSIZE, TEXT( "Windows XP Professional x64 Edition"));
			else 
				StringCchCat(pszOS, BUFSIZE, TEXT("Windows Server 2003, "));

			// Test for the server type.
			if ( osvi.wProductType != VER_NT_WORKSTATION )
			{
				if ( si.wProcessorArchitecture==PROCESSOR_ARCHITECTURE_IA64 )
				{
					if( osvi.wSuiteMask & VER_SUITE_DATACENTER )
						StringCchCat(pszOS, BUFSIZE, TEXT( "Datacenter Edition for Itanium-based Systems" ));
					else if( osvi.wSuiteMask & VER_SUITE_ENTERPRISE )
						StringCchCat(pszOS, BUFSIZE, TEXT( "Enterprise Edition for Itanium-based Systems" ));
				}

				else if ( si.wProcessorArchitecture==PROCESSOR_ARCHITECTURE_AMD64 )
				{
				    if( osvi.wSuiteMask & VER_SUITE_DATACENTER )
						StringCchCat(pszOS, BUFSIZE, TEXT( "Datacenter x64 Edition" ));
				    else if( osvi.wSuiteMask & VER_SUITE_ENTERPRISE )
						StringCchCat(pszOS, BUFSIZE, TEXT( "Enterprise x64 Edition" ));
				    else 
						StringCchCat(pszOS, BUFSIZE, TEXT( "Standard x64 Edition" ));
				}

				else
				{
				    if ( osvi.wSuiteMask & VER_SUITE_COMPUTE_SERVER )
						StringCchCat(pszOS, BUFSIZE, TEXT( "Compute Cluster Edition" ));
				    else if( osvi.wSuiteMask & VER_SUITE_DATACENTER )
						StringCchCat(pszOS, BUFSIZE, TEXT( "Datacenter Edition" ));
				    else if( osvi.wSuiteMask & VER_SUITE_ENTERPRISE )
						StringCchCat(pszOS, BUFSIZE, TEXT( "Enterprise Edition" ));
				    else if ( osvi.wSuiteMask & VER_SUITE_BLADE )
						StringCchCat(pszOS, BUFSIZE, TEXT( "Web Edition" ));
				    else 
						StringCchCat(pszOS, BUFSIZE, TEXT( "Standard Edition" ));
				}
			}
		}

		if (osvi.dwMajorVersion == 5 && osvi.dwMinorVersion == 1 )
		{
			StringCchCat(pszOS, BUFSIZE, TEXT("Windows XP "));
			if (osvi.wSuiteMask & VER_SUITE_PERSONAL )
				StringCchCat(pszOS, BUFSIZE, TEXT( "Home Edition" ));
			else
				StringCchCat(pszOS, BUFSIZE, TEXT( "Professional" ));
		}

		if (osvi.dwMajorVersion == 5 && osvi.dwMinorVersion == 0 )
		{
			StringCchCat(pszOS, BUFSIZE, TEXT("Windows 2000 "));

			if (osvi.wProductType == VER_NT_WORKSTATION )
				StringCchCat(pszOS, BUFSIZE, TEXT( "Professional" ));
			else 
			{
				if (osvi.wSuiteMask & VER_SUITE_DATACENTER )
					StringCchCat(pszOS, BUFSIZE, TEXT( "Datacenter Server" ));
				else if( osvi.wSuiteMask & VER_SUITE_ENTERPRISE )
					StringCchCat(pszOS, BUFSIZE, TEXT( "Advanced Server" ));
				else 
					StringCchCat(pszOS, BUFSIZE, TEXT( "Server" ));
			}
		}

		 // Include service pack (if any) and build number.

		if (_tcslen(osvi.szCSDVersion) > 0 )
		{
			StringCchCat(pszOS, BUFSIZE, TEXT(" ") );
		    StringCchCat(pszOS, BUFSIZE, osvi.szCSDVersion);
		}

		TCHAR buf[80];

		StringCchPrintf( buf, 80, TEXT(" (build %d)"), osvi.dwBuildNumber);
		StringCchCat(pszOS, BUFSIZE, buf);

		if ( osvi.dwMajorVersion >= 6 )
		{
			if ( si.wProcessorArchitecture==PROCESSOR_ARCHITECTURE_AMD64 )
				StringCchCat(pszOS, BUFSIZE, TEXT( ", 64-bit" ));
			else if (si.wProcessorArchitecture==PROCESSOR_ARCHITECTURE_INTEL )
				StringCchCat(pszOS, BUFSIZE, TEXT(", 32-bit"));
		}
		strOS.ReleaseBuffer();
		return TRUE; 
   }
   else
   {  
		return FALSE;
   }
}
#pragma warning(default:4996)

//-----------------------------------------------------------------------------
BOOL IsWindowsVersionGreaterThan(const int& wMajor, const int& wMinor)
{
	OSVERSIONINFOEX osvi;
	DWORDLONG dwlConditionMask = 0;
	int op = VER_GREATER_EQUAL;

	// Initialize the OSVERSIONINFOEX structure.
	ZeroMemory(&osvi, sizeof(OSVERSIONINFOEX));
	osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
	osvi.dwMajorVersion = wMajor;
	osvi.dwMinorVersion = wMinor;

	// Initialize the condition mask.

	VER_SET_CONDITION(dwlConditionMask, VER_MAJORVERSION, op);
	VER_SET_CONDITION(dwlConditionMask, VER_MINORVERSION, op);

	// Perform the test.

	return VerifyVersionInfo(
							&osvi,
							VER_MAJORVERSION | VER_MINORVERSION,
							dwlConditionMask
							);
}

//-----------------------------------------------------------------------------
BOOL IsWindowsVersion(const int& wMajor, const int& wMinor)
{
	OSVERSIONINFOEX osvi;
	DWORDLONG dwlConditionMask = 0;
	int op = VER_EQUAL;

	// Initialize the OSVERSIONINFOEX structure.
	ZeroMemory(&osvi, sizeof(OSVERSIONINFOEX));
	osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
	osvi.dwMajorVersion = wMajor;
	osvi.dwMinorVersion = wMinor;

	// Initialize the condition mask.

	VER_SET_CONDITION(dwlConditionMask, VER_MAJORVERSION, op);
	VER_SET_CONDITION(dwlConditionMask, VER_MINORVERSION, op);

	// Perform the test.

	return VerifyVersionInfo(
		&osvi,
		VER_MAJORVERSION | VER_MINORVERSION,
		dwlConditionMask);
}

//-----------------------------------------------------------------------------
BOOL IsOSWindows2K()
{
	return IsWindowsVersion(5, 2);
}

//-----------------------------------------------------------------------------
BOOL IsOSWindowsXp()
{
	return IsWindowsVersion(5, 1);
}

//-----------------------------------------------------------------------------
BOOL IsOSWinXpOrGreater()
{
	return IsWindowsVersionGreaterThan(5, 1);
}

//-----------------------------------------------------------------------------
BOOL IsOSVista()
{
	return IsWindowsVersion(6, 0);
}

//-----------------------------------------------------------------------------
BOOL IsOSVistaOrGreater()
{
	return IsWindowsVersionGreaterThan(6, 0);
}

//-----------------------------------------------------------------------------
BOOL IsOSWin7()
{
	return IsWindowsVersion(6, 1);
}

//-----------------------------------------------------------------------------
CString GetExtension (const CString& strName)
{
	if (strName.IsEmpty()) 
		return _T("");

	TCHAR szDrive  [_MAX_DRIVE];
	TCHAR szDir    [_MAX_DIR];
	TCHAR szFile   [_MAX_FNAME];
	TCHAR szExt    [_MAX_EXT];

	_tsplitpath_s(strName, szDrive, _MAX_DRIVE, szDir, _MAX_DIR, szFile, _MAX_FNAME, szExt, _MAX_EXT);

	return szExt;
}


// It checks if name is a dos file name and the existence
//-----------------------------------------------------------------------------
BOOL IsFileName (const CString& strName, BOOL bCheckExist /* = TRUE */, BOOL bCheckFullPath /* = FALSE */)
{
	if (!IsDosName(strName, bCheckFullPath)) return FALSE;
	
	if (IsDirSeparator(strName[strName.GetLength() - 1])) return FALSE;
    
    // se deve essere un nome di file non deve esistere come path
	if (ExistPath(strName)) return FALSE;
	
	if (!bCheckExist) return TRUE;

	return ExistFile(strName);
}

//-----------------------------------------------------------------------------
BOOL IsRelativePath(const CString& strName)
{
	if (strName.IsEmpty()) return TRUE;
	if (IsDirSeparator(strName[0])) return FALSE;
	if (strName.GetLength() > 1 && strName[1] == _T(':')) return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
// appends the extension supplied if 'name' doesn't contain it 
CString	AppendExtensionIfNeeded(const CString& name, const CString& ext)
{
	if (ext.IsEmpty())
		return name;

	CString strResult(name);
	CString strExt(ext);
	if (strExt[0] != '.') 
		strExt = '.' + strExt;

	if (GetExtension(strResult).CompareNoCase(strExt) != 0)
		strResult.Append(strExt);
	return strResult;
}

//-----------------------------------------------------------------------------
//  It returns the file path making a concatenation of three parameters:
//
//  -   path: can be empty, then only the fileName and ext are utilized;
//      if path don't terminate with a '\' the function add a '\' after the path
//
//  -   fileName: cannot be empty; an eventual '\' at the begin of fileName is ignored,
//      but there cannot be only a '\'
//
//  -   ext: is optional; if exist can or cannot start with a '.'
//
//  -   before return it's verified the global sintax of file path calling
//      IsFileName function
//
//-----------------------------------------------------------------------------
CString MakeFilePath (const CString& strPath, const CString& strFileName, const CString& strExt)
{
	CString strTmp (strPath);

	if (!strPath.IsEmpty() && !IsDirSeparator(strPath[strPath.GetLength() - 1]))
		strTmp += SLASH_CHAR;

	if (!strFileName.IsEmpty())
	{
		if (IsDirSeparator(strFileName[0]))
			if (strFileName.GetLength() > 1)
				strTmp += strFileName.Mid(1);
			else
				return CString("");
		else
			strTmp += strFileName;

		if (!strExt.IsEmpty())
			if (strExt[0] == DOT_CHAR)
				strTmp += strExt;
			else
				strTmp += CString(DOT_CHAR) + strExt;

		if (IsFileName(strTmp, FALSE))
			return strTmp;
	}

	return CString("");
}

//-----------------------------------------------------------------------------
BOOL RemoveFolderTree(const CString& strPath, BOOL bRemoveRoot /*= TRUE*/)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (pFileSystemManager)
		return pFileSystemManager->RemoveFolder (strPath, TRUE, bRemoveRoot);

	CString strFilePath;
	if (IsDirSeparator(strPath.Right(1)))
		strFilePath = strPath.Left(strPath.GetLength() - 1);
	else
		strFilePath = strPath;

	CFileFind finder;  
	BOOL bWorking =  finder.FindFile(strPath + URL_SLASH_CHAR + _T("*.*"));   
	BOOL bResult = TRUE;
	while (bWorking)
	{     
		bWorking = finder.FindNextFile();

		// evito "." e ".." per evitare ricorsione
		if (finder.IsDots())
			continue;

		if (finder.IsDirectory())
		{
			if(!RemoveFolderTree(finder.GetFilePath()))
				bResult = FALSE;
		}
		else
		{
			if(!::DeleteFile(finder.GetFilePath()))
				bResult = FALSE;
		}
	}
	
	finder.Close ();
	
	if(bRemoveRoot && !::RemoveDirectory (strFilePath))
		bResult = FALSE;

	return bResult;
}

// Si occupa di rimuovere le directory vuote figlie del ramo.  Si differenzia
// dalla funzione precedente perchè tenta di rimuovere ciò che può senza dare
// errore, cicla sulle directory rimuovendo solo quelle realmente vuote e  si
// ferma appena non riesce più. Va chiamata con il path più innestato  perchè
// non si preoccupa di vedere le sottodirectory del path originale.
//-----------------------------------------------------------------------------
TB_EXPORT BOOL RemoveEmptyParentFolders (const CString& strPath)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (pFileSystemManager)
		return pFileSystemManager->RemoveFolder (strPath, TRUE, FALSE, TRUE);

	CString strFilePath;
	if (IsDirSeparator(strPath.Right(1)))
		strFilePath = strPath.Left(strPath.GetLength() - 1);
	else
		strFilePath = strPath;

	CFileFind finder;  
	BOOL bWorking =  finder.FindFile(strPath + URL_SLASH_CHAR + _T("*.*"));   
	BOOL bToRemove = TRUE;
	while (bWorking)
	{     
		bWorking = finder.FindNextFile();

		// ho trovato un file o una directory quindi non rimuoverò la dir
		if (!finder.IsDots())
		{
			bToRemove = FALSE;
			break;
		}
	}
	
	finder.Close ();
	
	// se è vuota e la riesco a rimuovere saldo alla parent
	if (bToRemove && ::RemoveDirectory (strFilePath))
		RemoveEmptyParentFolders(GetPath(strFilePath));

	return TRUE;

}

//-----------------------------------------------------------------------------
BOOL RecursiveCreateFolders(const CString& strPath)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (pFileSystemManager)
		return pFileSystemManager->CreateFolder (strPath, TRUE);

	CString strParentPath;
	
	if (IsDirSeparator(strPath.Right(1)))
		strParentPath = GetPath(strPath.Left(strPath.GetLength() - 1));
	else
		strParentPath = GetPath(strPath);

	if (!ExistPath(strParentPath) && !RecursiveCreateFolders(strParentPath))
		return FALSE;
	
	return CreateDirectory(strPath, NULL);
}

//-----------------------------------------------------------------------------
BOOL CreateDirectory (const CString& strPath)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (pFileSystemManager)
		return pFileSystemManager->CreateFolder (strPath, FALSE);

	 return ::CreateDirectory (strPath, NULL);
}

//----------------------------------------------------------------------------------------------
void GetSubFolders	(const CString sHomeDir, CStringArray* pSubFolders)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (pFileSystemManager)
	{
		pFileSystemManager->GetSubFolders (sHomeDir, pSubFolders);
		return;
	}

	CFileFind aFinder;
	BOOL bWorking = aFinder.FindFile(sHomeDir + _T("\\*.*"));
	CString sSubDirName;
	while (bWorking)
	{    
		bWorking = aFinder.FindNextFile();

		// salto ., .., e i files
		if (aFinder.IsDots () || !aFinder.IsDirectory())
			continue;
		
		sSubDirName = aFinder.GetFileName();
		if (!sSubDirName.IsEmpty())
			pSubFolders->Add (sSubDirName);
	}
}

//----------------------------------------------------------------------------------------------
void GetFiles(const CString& sPathName, const CString& sFileExt, CStringArray* pFiles)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	if (pFileSystemManager)
	{
		pFileSystemManager->GetFiles(sPathName, sFileExt, pFiles);
		return;
	}
}

//-----------------------------------------------------------------------------
// Costruisce un nome di file temporaneo
//-----------------------------------------------------------------------------
CString GetTempName(LPCTSTR pszPrefix/* = NULL*/, BOOL bTempPath/* = TRUE*/)
{
	CString strTmp;		

	CString strPath = _T(".");
	if (bTempPath)
	{
		GetTempPath(MAX_PATH, strPath.GetBuffer(MAX_PATH));
		strPath.ReleaseBuffer();
	}

	GetTempFileName(strPath, pszPrefix ? pszPrefix : _T("ITR"), 0, strTmp.GetBuffer(MAX_PATH));
	strTmp.ReleaseBuffer();

	return strTmp;
}

//-----------------------------------------------------------------------------
// It checks if name is a dos file name and the existence of entire path
//-----------------------------------------------------------------------------
BOOL IsPathName (const CString& strName, BOOL bCheckExist /* = TRUE */)
{
	if (!IsDosName(strName)) return FALSE;

	if (!bCheckExist) return TRUE;

	return ExistPath(strName);
}

//------------------------------------------------------------------------------
CString MakeName (const CString& strFile, UINT wExtensionID)
{
	CString sExt;
	sExt.LoadString(wExtensionID);
	return MakeName(strFile, sExt);
}

//----------------------------------------------------------------------------------------------
DWORD GetTbFileAttributes (LPCTSTR  name)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (pFileSystemManager)
		return pFileSystemManager->GetFileAttributes(name);
	
	return ::GetFileAttributes (name);
}

//-----------------------------------------------------------------------------
LONG GetFileSize (LPCTSTR  fileName)
{
	BOOL                        fOk;
    WIN32_FILE_ATTRIBUTE_DATA   fileInfo;

    if (NULL == fileName)
        return -1;

    fOk = GetFileAttributesEx(fileName, GetFileExInfoStandard, (void*)&fileInfo);
    if (!fOk)
        return -1;
    ASSERT(0 == fileInfo.nFileSizeHigh);
    return (long)fileInfo.nFileSizeLow;

}

//-----------------------------------------------------------------------------
BOOL GetStatus (const CString& sFileName, CFileStatus& fs)
{
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager ();
	if (pFileSystemManager)
		return pFileSystemManager->GetFileStatus(sFileName,fs);

	return CFile::GetStatus(sFileName, fs);
}

//----------------------------------------------------------------------------------------------
SYSTEMTIME GetFileDate(const CString& sFileName, BOOL bLastWrite /*TRUE*/)
{
	SYSTEMTIME aDate;
	GetSystemTime(&aDate);

	CFileStatus fs;

	if (GetStatus (sFileName, fs))
	{
		CTime time = bLastWrite ? fs.m_mtime : fs.m_ctime;
		time.GetAsSystemTime (aDate);
	}

	return aDate;
}

//-----------------------------------------------------------------------------
BOOL IsMutexPresent(const CString& mutexName)
{
	// Creo un Mutex per impedire piu' istanze	
	SECURITY_DESCRIPTOR sd;
	if(!InitializeSecurityDescriptor(&sd, SECURITY_DESCRIPTOR_REVISION))
		return FALSE;
	
	if(!SetSecurityDescriptorDacl(&sd, TRUE, NULL, FALSE))
		return FALSE;
	
	SECURITY_ATTRIBUTES sa;
	sa.nLength = sizeof(SECURITY_ATTRIBUTES);
	sa.lpSecurityDescriptor = &sd;
	sa.bInheritHandle = FALSE;

	HANDLE hMutex = CreateMutex(&sa, TRUE, mutexName);
	if (hMutex != NULL)
	{
		ReleaseMutex(hMutex);
		CloseHandle(hMutex);
		return FALSE;
	}

	return TRUE;
}

static ATTACHEVENT_FUNC m_pManagedAddFileToCustomizationFuncPtr;
//-----------------------------------------------------------------------------
void AfxAttachCustomizationContextPointer (ATTACHEVENT_FUNC value)
{
	m_pManagedAddFileToCustomizationFuncPtr = value;
}

//-----------------------------------------------------------------------------
void AfxAddFileToCustomizationContext(const CString& path)
{
	if (m_pManagedAddFileToCustomizationFuncPtr)
		m_pManagedAddFileToCustomizationFuncPtr(path);
}