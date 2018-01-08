#pragma once

#include <TbNameSolver\JsonSerializer.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\TBResourceLocker.h>

#include "beginh.dex"

#define AUTH_TOKEN_PARAM _T("authtoken")
#define AUTH_TOKEN_PARAMA "authtoken"

#define SESSION_TIMEOUT 1200000 //20 minutes
const TCHAR szNeedLoginThreadUrl[] = _T("needLoginThread/");
const int nNeedLoginThreadUrlLen = _tcslen(szNeedLoginThreadUrl);

const TCHAR szNeedLoginInfinityUrl[] = _T("needLoginInfinity/");
const int szNeedLoginInfinityUrllLen = _tcslen(szNeedLoginInfinityUrl);



#define ENSURE_SESSION() \
	if (!pSession)\
				{\
		aResponse.SetMessage(_TB("Invalid session. Please login again."));\
		return;\
				} 

#define ENSURE_SESSION_BOOL() \
	if (!pSession)\
				{\
		aResponse.SetMessage(_TB("Invalid session. Please login again."));\
		return FALSE;\
				} 

struct CachedFile
{
	CachedFile(BYTE* data, int len) : m_data(data), m_len(len) {}
	~CachedFile() { delete m_data; }
	BYTE* m_data;
	int m_len;
};

class CTBResponse;
class TB_EXPORT CBrowserObj
{
public:
	virtual HWND GetMainWnd() = 0;
	virtual void DoClose(bool forceClose) = 0;
	virtual void Navigate (const CString& sUrl) = 0;
	virtual void ExecuteJavascript (const CString& sCode) = 0;
	virtual void SetCookie (const CString& sUrl, const CString& sName, const CString& sValue) = 0;
	virtual void Reload() = 0;
	virtual void ReloadIgnoreCache() = 0;
	
};
class TB_EXPORT CBrowserEventsObj
{
public:
	virtual void OnAfterCreated(CBrowserObj* pBrowser) {};
	virtual void OnBeforeClose(CBrowserObj* pBrowser) {};
	virtual void DoClose(CBrowserObj* pBrowser) {};
	virtual bool OnRequest(LPCTSTR lpszRequest, CTBResponse& aResponse){ return false; };
	virtual void Reload(){};
	virtual void ReloadIgnoreCache() {};
};
class TB_EXPORT CTBResponse
{
private:
	BYTE* m_responseBytes;
	int m_nBytesLength;
	CString m_strMimeType;
	CNameValueCollection m_Cookies;
	CNameValueCollection m_asHeaders;
	CString m_sRedirectUrl;
	int m_nStatus = 200;
public:
	CTBResponse();
	~CTBResponse() { delete m_responseBytes; }
	CString GetMimeType(){ return m_strMimeType; }
	void SetMimeType(const CString& strMimeType) { ASSERT(!strMimeType.IsEmpty()); m_strMimeType = strMimeType; }
	int GetLength();
	BYTE* GetData();
	void SetData(const CJsonSerializer& jsonData);
	void SetData(CString strData);
	void SetData(BYTE* bytes, int nLength);
	
	void SetCookie(const CString& sName, const CString& sValue)
	{
		m_Cookies.Add(sName, sValue);
	}
	void SetHeader(const CString& sName, const CString& sValue)
	{
		m_asHeaders.Add(sName, sValue);
	}
	const CNameValueCollection& GetCookies(){ return m_Cookies; }
	const CNameValueCollection& GetHeaders(){ return m_asHeaders; }
	void SetRedirectUrl(const CString &sUrl){
		m_sRedirectUrl = sUrl;
	}
	const CString& GetRedirectUrl() { return m_sRedirectUrl; }

	void SetStatus(int nStatus) {
		m_nStatus = nStatus;
	}
	const int GetStatus() { return m_nStatus; }
};

typedef CMap<CString, LPCTSTR, CachedFile*, CachedFile*> CacheMap;
class TB_EXPORT CTBRequestHandlerObj : public CTBLockable
{
	CStringA m_sPath;
	LPCSTR	m_Name;
protected:
	DECLARE_LOCKABLE(CacheMap, m_Cache);

public:
	CTBRequestHandlerObj(LPCSTR szPath);
	
	virtual ~CTBRequestHandlerObj(){}
	virtual bool PreProcessRequest(const CString& method, CMap<CString, LPCTSTR, CString, LPCTSTR>& requestHeaders, const CString& path, CTBResponse& response) = 0;
	virtual void ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response) = 0;
	LPCSTR GetPath() { return m_sPath; }
	LPCSTR GetName() { return m_Name; }
	int GetPathLength() { return m_sPath.GetLength(); }

	CLoginContext* GetLoginContext(const CString& authToken, BOOL bCreate);
	virtual CLoginContext* Login(CString authenticationToken);

protected:
	void SetMimeType(LPCTSTR path, CTBResponse& response);
	bool ReadFileContent(LPCTSTR file, CTBResponse& response);
}; 
class TB_EXPORT CTBPushNotifierObj
{
public:
	virtual ~CTBPushNotifierObj(){}
	virtual bool PushMessage(int to) = 0;
}; 

void TB_EXPORT CEFInitialize();
void TB_EXPORT AddRequestHandler(CTBRequestHandlerObj*);
TB_EXPORT CTBRequestHandlerObj* GetRequestHandler(LPCSTR szPath);
TB_EXPORT CTBRequestHandlerObj* GetRequestHandlerByUrl(CString strUrl);
void TB_EXPORT DisposeRequestHandlers();
void TB_EXPORT CEFUninitialize();
void TB_EXPORT DeleteCache();
bool TB_EXPORT CreateChildBrowser(HWND hwndParent, LPCTSTR sUrl, BOOL bWaitForBrowserReady, CBrowserEventsObj* pEvents = NULL, const RECT* = NULL);
bool TB_EXPORT CreatePopupBrowser(LPCTSTR sUrl, BOOL bWaitForBrowserReady, CBrowserEventsObj* pEvents = NULL); 
TB_EXPORT void GetCookieParams(CString& sCookie, CNameValueCollection& params);
TB_EXPORT void GetQueryParams(CStringA& sQuery, CNameValueCollection& params);
#include "endh.dex"

