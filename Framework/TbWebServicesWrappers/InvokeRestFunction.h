#pragma once

#include "winhttp.h"
#include "Beginh.dex"
#include <TbGeneric\DataObj.h>
#include <TbGeneric\DataObjDescription.h>
#include <TbGeneric\FunctionCall.h>

//===========================================================================
// InvokeRestFunction : rest protocoll 
//===========================================================================
//
class TB_EXPORT CRestFunctionDescription : public CBaseFunctionDescription
{

private:
	CString m_strDomain;
	CString m_strUrl;
	CString m_strUserAgent;
	int		m_nPort;
	CString m_strVerb;
	CString m_strURL;
	bool    m_bTextPlainContentType;
	CString m_strStatusCode;
	CString m_strMessage;

private:
	HINTERNET	m_hSession = NULL, m_hConnect = NULL, m_hRequest = NULL;
	CString		DecodeErrorMsg(DWORD  dwError);
	void		ShowErrorMessage();

public:
	CRestFunctionDescription(CString strFunction);

public:
	void InitConnectionInfo(const CString& strVerb, const CString& strURL, int nPort);
	BOOL ExecuteRequest();

private:
	BOOL OpenConnection();
	BOOL SendRequest();
	BOOL SendPostRequestWithParameters(CString& postData);
	BOOL ReceiveResponse(CString& strReturnData);
	void CloseConnection();


	CString SerializeRequest();
	BOOL DeserializeResponse(CString& strReturnData);
};