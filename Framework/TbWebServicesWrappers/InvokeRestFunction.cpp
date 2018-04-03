#include "StdAfx.h"
#include <strsafe.h>
#include "InvokeRestFunction.h"
#include <TbGeneric\LocalizableObjs.h>
#include <Windows.h>
#include <WinHttp.h>
#include <stdio.h>
#include <iostream> //getchar
#include <fstream>
#pragma comment(lib, "winhttp.lib")
// -------------------------------------

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


//-----------------------------------------------------------------------------
CRestFunctionDescription::CRestFunctionDescription(CString strFunction)
	:
	CBaseFunctionDescription(strFunction),
	m_hSession (NULL), 
	m_hConnect (NULL), 
	m_hRequest (NULL),
	m_strUserAgent(_T("M4Http")),
	m_nPort		(0),
	m_bTextPlainContentType(false)
{	
}


//-----------------------------------------------------------------------------
void CRestFunctionDescription::InitConnectionInfo(const CString& strVerb, const CString& strURL, int nPort)
{
	m_strVerb = strVerb;
	m_strURL = strURL;	
	m_nPort = 5000; // nPort;
	m_strDomain = _T("localhost");  
	m_strURL = _T("account-manager/") + this->GetName();
}

//-----------------------------------------------------------------------------
BOOL CRestFunctionDescription::ExecuteRequest()
{
	if (!OpenConnection())
		return FALSE;
	m_strStatusCode.Empty();
	m_strMessage.Empty();
	CString strPost = SerializeRequest();
	CString strResponse;
	BOOL bResult = SendPostRequestWithParameters(strPost) && ReceiveResponse(strResponse) && DeserializeResponse(strResponse);
	CloseConnection();
	return bResult;
}



//-----------------------------------------------------------------------------
BOOL CRestFunctionDescription::OpenConnection()
{
	if (m_nPort == 0 || m_strDomain.IsEmpty())
		return FALSE;

	LPCWSTR sVerb = (m_strVerb.IsEmpty()) ? _T("POST") : m_strVerb;

	try
	{
		//se cambio WINHTTP_ACCESS_TYPE_DEFAULT_PROXY in no proxy funziona niente
		m_hSession = WinHttpOpen(m_strUserAgent, WINHTTP_ACCESS_TYPE_AUTOMATIC_PROXY, WINHTTP_NO_PROXY_NAME, WINHTTP_NO_PROXY_BYPASS, 0);
		if (m_hSession == NULL)
			return FALSE;


		m_hConnect = WinHttpConnect(m_hSession, m_strDomain, m_nPort, 0);
		if (m_hConnect == NULL)
			return FALSE;

		//6 parametro di dafult questo qui WINHTTP_DEFAULT_ACCEPT_TYPES
		//7 paraemtro diventera WINHTTP_FLAG_SECURE xche andremo in ssl
		m_hRequest = WinHttpOpenRequest(m_hConnect, sVerb, m_strURL, NULL, WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES, 0);

		wchar_t buf[256];
		FormatMessageW(FORMAT_MESSAGE_FROM_SYSTEM, NULL, GetLastError(),
			MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), buf, 256, NULL);

		if (m_hRequest == NULL)
			return FALSE;
	}
	catch (CException*)
	{
		return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRestFunctionDescription::SendPostRequestWithParameters(CString& postData)
{
	BOOL  bResults;
	try
	{
		unsigned long dataLen = postData.GetLength();
		bResults = WinHttpSendRequest(m_hRequest, _T("Content-type: application/json"), -1, (char*)postData.GetBuffer(), dataLen, dataLen, dataLen);

		if (bResults == 0)
			ShowErrorMessage();
	}
	catch (CException*)
	{
		return NULL;	
	}

	return bResults;
}

//-----------------------------------------------------------------------------
BOOL CRestFunctionDescription::SendRequest() //X la Get
{
	BOOL  bResults;
	try
	{

		bResults = WinHttpSendRequest(m_hRequest, WINHTTP_NO_ADDITIONAL_HEADERS, 0, WINHTTP_NO_REQUEST_DATA, 0, 0, 0);
		if (bResults == 0)
			ShowErrorMessage();
	}
	catch (CException*)
	{
		return NULL;
	}
	return bResults;
}

//-----------------------------------------------------------------------------
void CRestFunctionDescription::ShowErrorMessage()
{
	CString lpMsgBuf;
	DWORD  dwError = GetLastError();

	if (dwError != NULL)
		lpMsgBuf = DecodeErrorMsg(dwError);

	if (lpMsgBuf)
		AfxMessageBox((LPCTSTR)lpMsgBuf, MB_ICONSTOP);
}

//-----------------------------------------------------------------------------
void GetHeaderInfo(HINTERNET requestHandle, ULONG queryFlags, CString& headerInfo)
{
	WCHAR headerBuffer[256];
	ULONG headerLength = sizeof(headerBuffer);
	if (WinHttpQueryHeaders(requestHandle, WINHTTP_QUERY_STATUS_CODE, NULL, headerBuffer, &headerLength, WINHTTP_NO_HEADER_INDEX))
		headerInfo = headerBuffer;
}

//-----------------------------------------------------------------------------
BOOL CRestFunctionDescription::ReceiveResponse(CString& ReturnData)
{
#define BUFFER_SIZE_1KB 1024
	BOOL  bResults;
	PCHAR pFormHeader = 0;
	PUCHAR pBuf = 0;

	try
	{
		bResults = WinHttpReceiveResponse(m_hRequest, NULL);
		if (bResults == 0)
		{
			ShowErrorMessage();
			return FALSE;
		}
		CString strContentType;
		GetHeaderInfo(m_hRequest, WINHTTP_QUERY_STATUS_CODE, m_strStatusCode);
		GetHeaderInfo(m_hRequest, WINHTTP_QUERY_CONTENT_TYPE, strContentType);
		m_bTextPlainContentType = (strContentType.Compare(_T("text / plain")) == 0);
		
		DWORD		dwDownloaded = 0;

		DWORD dwSize = 0;
		if (bResults)
		{
			do
			{
				if (!WinHttpQueryDataAvailable(m_hRequest, &dwSize))
					break;

				// Allocate space for the buffer.
				LPSTR pszOutBuffer = new char[dwSize + 1];
				if (!pszOutBuffer)
					break;
				else
				{
					// Read the data.
					ZeroMemory(pszOutBuffer, dwSize + 1);
					if (!WinHttpReadData(m_hRequest, (LPVOID)pszOutBuffer, dwSize, &dwDownloaded))
						break;
					else
						ReturnData.Append(CString(pszOutBuffer));	// concateno il buffer di ritorno ReturnData xche l ultimo e' stringa vuota 

					delete[] pszOutBuffer;
				}
			} while (dwSize > 0);
		}
	}
	catch (CException*)
	{
		return NULL;
	}
	return bResults;
}

//-----------------------------------------------------------------------------
void CRestFunctionDescription::CloseConnection()
{
	// Close any open handles.
	if (m_hRequest)
		WinHttpCloseHandle(m_hRequest);

	if (m_hConnect)
		WinHttpCloseHandle(m_hConnect);

	if (m_hSession)
		WinHttpCloseHandle(m_hSession);
	
	m_hRequest = m_hConnect = m_hSession = 0;
}

// ------------------------------------------------------------------------------
CString CRestFunctionDescription::DecodeErrorMsg(DWORD  dwError)
{


	switch (dwError)
	{

	case 12178:
		return _TB("WinHttpGetProxyForUrl  a proxy for the specified URL cannot be located.");

	case 12180:
		return _TB("Error returned by WinHttpDetectAutoProxyConfigUrl if WinHTTP was unable to discover the URL of the Proxy Auto-Configuration (PAC) file");

	case 12166:
		return _TB("An error occurred executing the script code in the Proxy Auto-Configuration (PAC) file.");

	case 12103:
		return _TB("Error returned  by the HttpRequest object if a specified option cannot be requested after the Open method has been called.");

	case 12102:
		return _TB("Error returned  by the HttpRequest object if a requested operation cannot be performed after calling the Send method.");

	case 12100:
		return _TB("Error returned  by the HttpRequest object if a requested operation cannot be performed before calling the Open method.");

	case 12101:
		return _TB("Error returned  by the HttpRequest object if a requested operation cannot be performed before calling the Send method.");

	case 12029:
		return _TB("Connection to the server failed.");

	case 12183:
		return _TB("Error returned  by WinHttpReceiveResponse when an overflow condition is encountered in the course of parsing chunked encoding.");

	case 12044:
		return _TB("Error returned  by WinHttpReceiveResponse when the server requests client authentication.Windows Server 2003 with SP1 and Windows XP with SP2 : This error is not supported.");

	case 12030:
		return _TB("The connection with the server has been reset or terminated, or an incompatible SSL protocol was encountered.For example, WinHTTP version 5.1 does not support SSL2 unless the client specifically enables it.");

	case 12155:
		return _TB("Obsolete; no longer used.");
	case 12181:
		return _TB("Error returned  by WinHttpReceiveResponse when a larger number of headers were present in a response than WinHTTP could receive.");
	case 12150:
		return _TB("The requested header cannot be located.");
	case 12182:
		return _TB("Error returned  by WinHttpReceiveResponse when the size of headers received exceeds the limit for the request handle.");
	case 12019:
		return _TB("The requested operation cannot be carried out because the handle supplied is not in the correct state.");
	case 12018:
		return _TB("The type of handle supplied is incorrect for this operation.");
	case 12004:
		return _TB("An internal error has occurred.");
	case 12009:
		return _TB("A request to WinHttpQueryOption or WinHttpSetOption specified an invalid option value.");
	case 12154:
		return _TB("Obsolete; no longer used.");
	case 12152:
		return _TB("The server response cannot be parsed.");
	case 12005:
		return _TB("The URL is not valid.");
	case 12015:
		return _TB("The login attempt failed. When this error is encountered, the request handle should be closed withWinHttpCloseHandle. A new request handle must be created before retrying the function that originally produced this error.");
	case 12007:
		return _TB("The server name cannot be resolved.");
	case 12172:
		return _TB("Obsolete; no longer used.");
	case 12017:
		return _TB("The operation was canceled, usually because the handle on which the request was operating was closed before the operation completed.");
	case 12011:
		return _TB("The requested option cannot be set, only queried.");
	case 12001:
		return _TB("Obsolete; no longer used.");
	case 12156:
		return _TB("The redirection failed because either the scheme changed or all attempts made to redirect failed (default is five attempts).");
	case 12032:
		return _TB("The WinHTTP function failed. The desired function can be retried on the same request handle.");
	case 12184:
		return _TB("Returned when an incoming response exceeds an internal WinHTTP size limit.");
	case 12177:
		return _TB("An error was encountered while executing a script.");
	case 12038:
		return _TB("Error returned  when a certificate CN name does not match the passed value (equivalent to a CERT_E_CN_NO_MATCH error).");
	case 12037:
		return _TB("Error indicates that a required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file, or that the validity periods of the certification chain do not nest correctly (equivalent to a CERT_E_EXPIRED or a CERT_E_VALIDITYPERIODNESTING error).");
	case 12057:
		return _TB("Error indicates that revocation cannot be checked because the revocation server was offline (equivalent to CRYPT_E_REVOCATION_OFFLINE).");
	case 12170:
		return _TB("Error indicates that a certificate has been revoked (equivalent to CRYPT_E_REVOKED).");
	case 12179:
		return _TB("Error indicates that a certificate is not valid for the requested usage (equivalent to CERT_E_WRONG_USAGE).");
	case 12157:
		return _TB("Error indicates that an error occurred having to do with a secure channel (equivalent to error codes that begin with 'SEC_E_' and 'SEC_I_' listed in the 'winerror.h' header file).");
	case 12175:
		return _TB("One or more errors were found in the Secure Sockets Layer (SSL) certificate sent by the server. To determine what type of error was encountered, check for a WINHTTP_CALLBACK_STATUS_SECURE_FAILURE notification in a status callback function. For more information, see WINHTTP_STATUS_CALLBACK.");
	case 12045:
		return _TB("Error indicates that a certificate chain was processed, but terminated in a root certificate that is not trusted by the trust provider (equivalent to CERT_E_UNTRUSTEDROOT).");
	case 12169:
		return _TB("Error indicates that a certificate is invalid (equivalent to errors such as CERT_E_ROLE, CERT_E_PATHLENCONST, CERT_E_CRITICAL, CERT_E_PURPOSE, CERT_E_ISSUERCHAINING, CERT_E_MALFORMED and CERT_E_CHAINING).");
	case 12012:
		return _TB("The WinHTTP function support is being shut down or unloaded.");
	case 12002:
		return _TB("The request has timed out.");
	case 12167:
		return _TB("The PAC file cannot be downloaded. For example, the server referenced by the PAC URL may not have been reachable, or the server returned a 404 NOT FOUND response.");
	case 12176:
		return _TB("The script type is not supported.");
	case 12006:
		return _TB("The URL specified a scheme other than 'http:' or 'https : '.");
	}

	return _T("");
}



// ------------------------------------------------------------------------------
CString CRestFunctionDescription::SerializeRequest()
{
	CJsonSerializer serializer;
	serializer.OpenObject();
	CDataObjDescription* pParam = NULL;
	for (int i = 0; i < this->m_arFunctionParams.GetSize(); i++)
	{
		pParam = (CDataObjDescription*)m_arFunctionParams.GetAt(i);
		if (pParam->GetPassedMode() == CDataObjDescription::_INOUT || pParam->GetPassedMode() == CDataObjDescription::_IN)
			serializer.WriteString (pParam->GetName(), pParam->GetValue()->FormatDataForXML());		
	}
	serializer.CloseObject();
	return serializer.GetJson();
}

// ------------------------------------------------------------------------------
BOOL CRestFunctionDescription::DeserializeResponse(CString& strJsonReturData)
{
	if (m_bTextPlainContentType)
	{
		m_strMessage = strJsonReturData;
		return (!m_strStatusCode.IsEmpty());
	}
	
	CJsonParser parser;
	CDataObjDescription* pParam = NULL;
	CString strValue;
	parser.ReadJsonFromString(strJsonReturData);
	for (int i = 0; i < this->m_arFunctionParams.GetSize(); i++)
	{
		pParam = (CDataObjDescription*)m_arFunctionParams.GetAt(i);
		if (pParam->GetPassedMode() == CDataObjDescription::_OUT)
		{
			strValue = parser.ReadString(pParam->GetName());
			pParam->SetValue(strValue);
		}
	}

	return TRUE;
}