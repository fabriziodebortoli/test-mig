#include "stdafx.h" 
#include <afxconv.h>

#include <TbNameSolver\FileSystemFunctions.h>

#include "xmlgeneric.h"

/////////////////////////////////////////////////////////////////////////////
static const TCHAR	szTrue			[] = _T("1");
static const TCHAR	szFalse			[] = _T("0");
static const TCHAR	szTrueString	[] = _T("true");	// Soap prevede solominuscolo
static const TCHAR	szFalseString	[] = _T("false");	// "" ""
/////////////////////////////////////////////////////////////////////////////

#define HOSTNAME_BUFFER_LENGTH		128
/////////////////////////////////////////////////////////////////////////////
// Per eseguire la verifica della corrente versione di Explorer si controlla 
// il valore di "Version" nella seguente chiave del registry:
//	HKEY_LOCAL_MACHINE
//		Software
//			Microsoft
//				Internet Explorer
// Se il valore "Version" è presente in questa chiave vuol dire che sulla
// macchina è installato Internet Explorer 4.0 o sue versioni successive.
/////////////////////////////////////////////////////////////////////////////
BOOL IsIE5OrLaterInstalled()
{	
	// Open the registry key.
	HKEY hKey = NULL;
	LONG lResult = RegOpenKeyEx
			( 
				HKEY_LOCAL_MACHINE,
				_T("Software\\Microsoft\\Internet Explorer"),
				0,
				KEY_QUERY_VALUE,
				&hKey
			);
	if (lResult != ERROR_SUCCESS)
		return FALSE;
	
	DWORD	dwValueType;
	TCHAR	szIEVersion[80];
	DWORD	dwBufLen = sizeof(szIEVersion);

	lResult = RegQueryValueEx
				(
					hKey,
					_T("Version"),
					0, //Reserved; must be NULL. 
					&dwValueType,
					(LPBYTE)szIEVersion,
					&dwBufLen
				);
	RegCloseKey(hKey);
	
	if (lResult != ERROR_SUCCESS && lResult != ERROR_MORE_DATA)
		return FALSE;

	return szIEVersion[0] >= _T('5');
}

/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
// The way this function works is that it attempts to define which are the 
// IP addresses which can automatically be excluded from consideration. Then
// it defines which IP addresses would signify that the computer is connected
// to the Internet.
/////////////////////////////////////////////////////////////////////////////
BOOL IsInternetConnected ()
{
	/*// Dynamically link to Ws2_32.dll.  If it's not there just return.
	HINSTANCE hlibWs2_32 = ::LoadLibrary(_T("WS2_32.DLL"));
	if (hlibWs2_32 == NULL  || hlibWs2_32 == INVALID_HANDLE_VALUE)
		return FALSE;
	LPWSASTARTUP		lpWSAStartupFunction = NULL;
	LPWSACLEANUP		lpWSACleanupFunction = NULL;
	LPWSAGETLASTERROR	lpWSAGetLastErrorFunction = NULL;
	LPGETHOSTNAME		lpgethostnamedFunction = NULL;
	LPGETHOSTBYNAME		lpgethostbynameFunction = NULL;

    if 
		(
			!(lpWSAStartupFunction = (LPWSASTARTUP) GetProcAddress (hlibWs2_32, "WSAStartup"))||
			!(lpWSACleanupFunction = (LPWSACLEANUP) GetProcAddress (hlibWs2_32, "WSACleanup"))||
			!(lpWSAGetLastErrorFunction = (LPWSAGETLASTERROR) GetProcAddress (hlibWs2_32, "WSAGetLastError"))||
			!(lpgethostnamedFunction = (LPGETHOSTNAME) GetProcAddress (hlibWs2_32, "gethostname"))||
			!(lpgethostbynameFunction = (LPGETHOSTBYNAME) GetProcAddress (hlibWs2_32, "gethostbyname")) 
		)
	{
		FreeLibrary (hlibWs2_32) ;
		return FALSE;
	}
	
	WSADATA  wsaData;
    if ((*lpWSAStartupFunction)(0x0101, &wsaData))
	{
		FreeLibrary (hlibWs2_32) ;
		return FALSE;
	}
	
	BOOL bRC = FALSE;
	TCHAR szHostName[HOSTNAME_BUFFER_LENGTH];
	BOOL bPrivateAdr = FALSE; // Private Address area
	BOOL bClassA = FALSE;     // Class A definition
	BOOL bClassB = FALSE;     // Class B definition
	BOOL bClassC = FALSE;     // Class C definition
	BOOL bAutoNet = FALSE;    // AutoNet definition
	CString strAddress;
	
	if ((*lpgethostnamedFunction)(szHostName, HOSTNAME_BUFFER_LENGTH) == 0 )
	{
		// Get host adresses
		struct hostent* pHost = (*lpgethostbynameFunction)(szHostName);

		for (int i = 0;  pHost!= NULL && pHost->h_addr_list[i]!= NULL;  i++ )
		{
			strAddress.Empty();

			bClassA = bClassB = bClassC = FALSE;
			for( int j = 0; j < pHost->h_length; j++ )
			{
				CString addr;

				if( j > 0 )
					strAddress += _T(".");
				UINT ipb = (unsigned int)((unsigned char*)pHost->h_addr_list[i])[j];

				// Define the IP range for exclusion
				if(j==0)
				{
					 if(bClassA = (ipb == 10)) 
						 break; // Class A defined
					 
					 bClassB = (ipb == 172); 
					 bClassC = (ipb == 192);
					 bAutoNet = (ipb == 169);
				}
				else if (j==1)
				{
				 // Class B defined
				 if(bClassB = (bClassB && ipb >= 16 && ipb <= 31)) 
					 break;

				 // Class C defined
				 if(bClassC = (bClassC && ipb == 168)) 
					 break;

				 //AutoNet pasibility defined
				 if(bAutoNet = (bAutoNet && ipb == 254)) 
					 break;
				}
				addr.Format(_T("%u"), ipb );
				strAddress += addr;
			}
			// str now contains one local IP address 
			// If any address of Private Address area has been found bPrivateAdr = TRUE
			bPrivateAdr = bPrivateAdr || bClassA || bClassB || bClassC;

			// If any address of Internet Address area has been found returns TRUE
			if 
				(
					!bClassA			&&
					!bClassB			&&
					!bClassC			&&
					!bAutoNet			&&
					strAddress != _T("127.0.0.1")  &&
					!strAddress.IsEmpty()) 
			{
				bRC = TRUE;
				break;
			}
		}
	}

	if (bPrivateAdr)
	{
		// The system has IP address from Private Address area only.
		// Internet from the computer can be accessable via Proxy.
		// The function believe Internet accessable.
		bRC = TRUE;
	}

	if (!bRC)
	{
		int nError = (*lpWSAGetLastErrorFunction)();
	}
	
	(*lpWSACleanupFunction)();
	FreeLibrary (hlibWs2_32) ;
	return bRC;*/return TRUE;
}

//-----------------------------------------------------------------------------
#define SLASH_CHAR		_T('\\')
#define URL_SLASH_CHAR	_T('/')
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
CString GetFileFullPath(const CString& strFileName, BOOL bAppendLastSlash /*= FALSE*/)
{
	LPTSTR lpszFilenamePart;
	CString strPath;
	::GetFullPathName(strFileName, _MAX_PATH, strPath.GetBuffer(_MAX_PATH), &lpszFilenamePart);
	if (lpszFilenamePart)
		*lpszFilenamePart = _T('\0');
	strPath.ReleaseBuffer();

	if (bAppendLastSlash)
	{
		if (!IsDirSeparator(strPath.Right(1)))
			strPath += SLASH_CHAR;
	}
	else if (IsDirSeparator(strPath.Right(1)))
		strPath = strPath.Left(strPath.GetLength()-1);

	return strPath;
}

//-----------------------------------------------------------------------------
BOOL CreateDirectoryTree(const CString& strPath)
{
	if (strPath.IsEmpty()) return TRUE;
	
	CString strParentPath;
	
	if (IsDirSeparator(strPath.Right(1)))
		strParentPath = GetFileFullPath(strPath.Left(strPath.GetLength() - 1));
	else
		strParentPath = GetFileFullPath(strPath);

	if (!ExistPath(strParentPath) && (strParentPath == strPath || !CreateDirectoryTree(strParentPath)))
		return FALSE;
	
	return CreateDirectory(strPath);
}

//-----------------------------------------------------------------------------
BOOL CreateFileDirectoryIfNecessary(const CString& strFileName)
{
	CString strXMLPath(GetFileFullPath(strFileName));

	return ExistPath(strXMLPath) || CreateDirectoryTree(strXMLPath);
}

//-----------------------------------------------------------------------------
BOOL IsGUIDStringValid(LPCTSTR lpszGUIDToTest, BOOL bTestIfRegistered /* = FALSE*/, GUID* lpConvertedGUID /* = NULL*/)
{
	if (lpConvertedGUID)
		ZeroMemory(lpConvertedGUID, sizeof(GUID));

	if (!lpszGUIDToTest || !_tcslen(lpszGUIDToTest))
		return FALSE;

	CString strGUIDToTest(lpszGUIDToTest);
	strGUIDToTest.TrimLeft();
	strGUIDToTest.TrimRight();
	// la funzione CLSIDFromString vuole le parentesi graffe!!!
	if (strGUIDToTest[0] != _T('{'))
		strGUIDToTest = _T("{") + strGUIDToTest + _T("}");
	
	USES_CONVERSION;
	GUID guidConverted;
	HRESULT hr = ::CLSIDFromString
					(
						T2W((LPTSTR)strGUIDToTest.GetBuffer(0)),	// Pointer to the string representation of the GUID
						&guidConverted					// Pointer to the GUID
					);
	
	strGUIDToTest.ReleaseBuffer();

	if (lpConvertedGUID && (hr == NOERROR || hr == REGDB_E_WRITEREGDB))
		*lpConvertedGUID = guidConverted;
	
	switch(hr)
	{
		case NOERROR:				// The GUID was obtained successfully.
			return TRUE;
		case REGDB_E_WRITEREGDB:	// The CLSID corresponding to the class string was not found in the registry. 
			return !bTestIfRegistered;

		case E_INVALIDARG:		// Invalid argument.
		//case CO_E_CLASSTRING:	// The class string was improperly formatted. 
		case CO_E_CLASSSTRING:	// The class string was improperly formatted. 
			return FALSE;
		default:
			break;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
CString GetGUIDString(const GUID& guid, BOOL bWithoutCurlyBraces /*= FALSE*/)
{
	USES_CONVERSION;
	CString strGUID;
	LPOLESTR lpolestrGUID;
	if(SUCCEEDED(::StringFromCLSID(guid, &lpolestrGUID)))
		strGUID = CString(W2T(lpolestrGUID));
	::CoTaskMemFree(lpolestrGUID);

	if (bWithoutCurlyBraces && strGUID.Left(1) == _T('{') && strGUID.Right(1) == _T('}'))
		strGUID = strGUID.Mid(1,strGUID.GetLength() -2);
	
	return strGUID;
}

//-----------------------------------------------------------------------------
void SetBSTRText(LPCTSTR lpszBstrText, BSTR& bstrText)
{
	ASSERT(lpszBstrText);

#ifdef _UNICODE
	bstrText = SysAllocString(lpszBstrText);
#else
	int nBufferSize = ::MultiByteToWideChar(CP_ACP, 0, lpszBstrText, -1, NULL, 0);
	OLECHAR* lpszwText = new OLECHAR[nBufferSize];
	::MultiByteToWideChar(CP_ACP, 0, lpszBstrText, -1, lpszwText, nBufferSize);
	bstrText = SysAllocString(lpszwText);
	delete [] lpszwText;
#endif
}

//-----------------------------------------------------------------------------
void SetVariantText(LPCTSTR lpszVarText, VARIANT& vaText)
{
	ASSERT(lpszVarText);

	VariantInit(&vaText);
	vaText.vt = VT_BSTR;

	SetBSTRText(lpszVarText, vaText.bstrVal);
}

//----------------------------------------------------------------------------
// Reserved characters in script elements must be made opaque. Script elements
// frequently include greater than (<) and less than (>) symbols, the ampersand
// symbol (&), and other characters that are reserved in XML.
// If you are creating a closely conformant XML file, you must make sure that
// these reserved characters do not confuse the XML parser.
// You can individually mark characters with escape sequences (for example,
// specify "<" as "&lt;"), or you can make an entire script element opaque by
// enclosing it in a <![CDATA[ ...]]> section.
//----------------------------------------------------------------------------
CString FormatStringForXML (LPCTSTR lpszString)
{
	// Ho verificato che i metodi del XML DOM effettuano in automatico
	// la conversione delle stringhe in UTF-8 in scrittura e, viceversa,
	// da UTF-8 in lettura e, quindi, non è necessario manipolare le
	// stringhe.
	// Lascio la funzione per simmetria con le altre tipologie di dato
	// e per possibili modifiche che si vorranno introdurre in futuro
	return lpszString;
}

//----------------------------------------------------------------------------
// XML Data Type: boolean
// 0 or 1, where 0 == "false" and 1 =="true".
//----------------------------------------------------------------------------
CString FormatBoolForXML (const BOOL& aBool, BOOL bSoapMode)
{
	if (bSoapMode) return aBool ? szTrueString : szFalseString;
	return aBool ? szTrue : szFalse;
}

//prende una stringa che se contiene 1 0 true (True) false (False) yes no le converte a true o false
//----------------------------------------------------------------------------
BOOL GetBoolFromXML(const CString& strValue)
{
	return 	!strValue.IsEmpty() &&
			(!strValue.CompareNoCase(szTrueString) || _ttoi((LPCTSTR)strValue) != 0);
}