
#include "StdAfx.h"
#include <TbNameSolver\JsonSerializer.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\Templates.h>
#include <TbGeneric\CollateCultureFunctions.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ParametersSections.h>
#include "TBCommandInterface.h"
#include "CEFClasses.h"

#include <include\cef_parser.h>

#include <include\cef_client.h>
#include <include\cef_app.h>
#include <include\cef_browser.h>
#include <include\cef_task.h>
#include <include\base\cef_bind.h>
#include <include\base\cef_callback_forward.h>
#include <include\wrapper\cef_helpers.h>
#include <include\wrapper\cef_closure_task.h>
#include <atlenc.h>

class CRequestThread : public CWinThread
{
	DECLARE_MESSAGE_MAP()
	HANDLE hReady = CreateEvent(NULL, TRUE, FALSE, _T("TREQUEST_READY"));;
public:
	virtual BOOL InitInstance()
	{
#ifdef DEBUG
		SetThreadName(::GetCurrentThreadId(), "CEF Request Thread Function");
#endif

		CWinThread::InitInstance();
		SetEvent(hReady);

		return TRUE;
	}

	void WaitForCreation()
	{
		WaitForSingleObject(hReady, INFINITE);
		CloseHandle(hReady);
		hReady = NULL;
	}

	//----------------------------------------------------------------------------
	void OnExecuteFunction(WPARAM wParam, LPARAM lParam)
	{
		CBaseFunctionWrapper* pWrapper = (CBaseFunctionWrapper*)wParam;
		pWrapper->OnExecuteFunction();
	}

};
BEGIN_MESSAGE_MAP(CRequestThread, CWinThread)
	ON_THREAD_MESSAGE(UM_EXECUTE_FUNCTION, OnExecuteFunction)
END_MESSAGE_MAP()

#define REQUIRE_UI_THREAD()   ASSERT(CefCurrentlyOn(TID_UI));
#define REQUIRE_IO_THREAD()   ASSERT(CefCurrentlyOn(TID_IO));
#define REQUIRE_FILE_THREAD() ASSERT(CefCurrentlyOn(TID_FILE));
typedef TArray<CTBRequestHandlerObj> RequestHandlerArray;
static DECLARE_LOCKABLE(RequestHandlerArray, s_RequestHandlers)

static CTBPushNotifierObj*		s_pPushNotifier = NULL;
static HANDLE hCefThread = NULL;
static HANDLE hCefReady = NULL;

//thread per gestire le richieste lato server in modo asincrono, altrimenti il browser resta freezato
//nel caso di modulo IIS, questo non è necessario, ci pensa IIS a creare i thread
static CRequestThread* g_pRequestHandlerThread = NULL;


class EmptyCefSetCookieCallback : public CefRefPtr<CefSetCookieCallback>
{
public:

	EmptyCefSetCookieCallback() {}
	virtual void OnComplete(bool success) {};
};

class TbLoaderResourceHandler : public CefResourceHandler {

	
	CBrowserEventsObj* m_pBrowserEvents;
	CTBRequestHandlerObj* m_pTbHandler;
public:

	TbLoaderResourceHandler(CBrowserEventsObj* pBrowserEvents, CTBRequestHandlerObj* pTbHandler) : m_pBrowserEvents(pBrowserEvents), m_pTbHandler(pTbHandler), offset_(0) {}
	~TbLoaderResourceHandler() {}

	//--------------------------------------------------------------------------------
	/// <summary>
	/// Extracts query parameters from the given query string and the given post data. 
	/// Extracted query parameters are put into the given CNameValueCollection.
	/// </summary>
	/// <param name="sUriQuery">Reference to the given query string.</param>
	/// <param name="postData">Smart Pointer to the given post data.</param>
	/// <param name="params">Reference to the given CNameValueCollection to be 
	///	populated with the parsed query parameters. It must reference a not NULL memory position.</param>
	/// <returns></returns>
	void GetRequestQueryParams(CStringA sUriQuery, CefRefPtr<CefPostData> postData, CNameValueCollection& params) 
	{
		if (!sUriQuery.IsEmpty())
		{
			GetQueryParams(sUriQuery, params);
		}
		if (postData.get())
		{
			CefPostData::ElementVector elements;
			postData->GetElements(elements);
			if (elements.size() <= 0)
				return;

			CefRefPtr<CefPostDataElement> element;
			CefPostData::ElementVector::const_iterator it = elements.begin();
			for (; it != elements.end(); ++it)
			{
				element = (*it);
				if (element->GetType() == PDE_TYPE_BYTES)
				{
					// the element is composed of bytes
					if (element->GetBytesCount() == 0)
						continue;

					// leggo i dati ASCII encodati.
					size_t size = element->GetBytesCount();
					CStringA encodedData;
					element->GetBytes(size, encodedData.GetBuffer(size));
					encodedData.ReleaseBuffer();
					GetQueryParams(encodedData, params);
				}
				else if (element->GetType() == PDE_TYPE_FILE)
				{
					//ss << "\n\tFile: " << std::string(element->GetFile());
				}
			}
		}
	}

	virtual bool ProcessRequest(CefRefPtr<CefRequest> request,
		CefRefPtr<CefCallback> callback)
		OVERRIDE {
		AddRef();//per evitare l'eventuale distruzione, il thread chiamato farà a release
		if (g_pRequestHandlerThread)
		{
			AfxInvokeAsyncThreadProcedure<TbLoaderResourceHandler, CefRefPtr<CefRequest>, CefRefPtr<CefCallback>>
				(g_pRequestHandlerThread->m_nThreadID, this, &TbLoaderResourceHandler::InternalProcessRequest, request, callback);
		}
		else
		{
			InternalProcessRequest(request, callback);
		}
		return true;
	}

	void InternalProcessRequest(CefRefPtr<CefRequest> request, CefRefPtr<CefCallback> callback)
	{
		//AutoLock lock_scope(this);
		std::string sUrl = request->GetURL();
		CefString url = sUrl;
		CefURLParts url_parts;
		CefParseURL(url, url_parts);
		LPCTSTR sz = url_parts.path.str;
		if (m_pTbHandler)
			sz += m_pTbHandler->GetPathLength(); //salto il pezzo con il nome a cui l'handler risponde (es tbloader)
		CString urlPath = sz;

		CStringA query = CefString(&url_parts.query).ToString().c_str();
		CNameValueCollection params;

		CefRefPtr<CefPostData> postData = request->GetPostData();
		GetRequestQueryParams(query, postData, params);
		CefRequest::HeaderMap headerMap;
		request->GetHeaderMap(headerMap);
		
		CMap<CString, LPCTSTR, CString, LPCTSTR> requestHeaders;
		if (headerMap.size() > 0) {
			CefRequest::HeaderMap::const_iterator it = headerMap.begin();
			for (; it != headerMap.end(); ++it) {
				std::string first = std::string((*it).first);
				CString strFirst(first.c_str(), first.length());
				std::string second = std::string((*it).second);
				CString cstr(second.c_str(), second.length());
				requestHeaders[strFirst] = cstr;

				if (first.compare("Cookie") == 0)
				{
					GetCookieParams(UTF8ToUnicode(second.c_str()), params);
					continue;
				}

			}
		}
		if (!m_pBrowserEvents || !m_pBrowserEvents->OnRequest(url.c_str(), m_response))
		{
			if (m_pTbHandler->PreProcessRequest(request->GetMethod().c_str(), requestHeaders, urlPath, m_response))
				m_pTbHandler->ProcessRequest(urlPath, params, m_response);
		}
		
		// Indicate the headers are available.
		callback->Continue();
		Release();
	}
	virtual void GetResponseHeaders(CefRefPtr<CefResponse> response,
		int64& response_length,
		CefString& redirectUrl) OVERRIDE {
		REQUIRE_IO_THREAD();

		response->SetMimeType((LPCTSTR)m_response.GetMimeType());

		const CNameValueCollection& cookies = m_response.GetCookies();
		CefResponse::HeaderMap map;

		response->GetHeaderMap(map);
		for (int i = 0; i < cookies.GetSize(); i++)
		{
			CNameValuePair* p = cookies.GetAt(i);
			CefRefPtr<CefCookieManager> manager = CefCookieManager::GetGlobalManager(NULL);
			CefCookie cookie;
			CefString(&cookie.name) = p->GetName();
			CefString(&cookie.value) = p->GetValue();
			CefString(&cookie.path).FromASCII("/");
			CefString url = "http://localhost";
			//CefPostTask(TID_IO, NewCefRunnableMethod(manager.get(), &CefCookieManager::SetCookie, url, cookie));
			EmptyCefSetCookieCallback callback;
			CefPostTask(TID_IO, CefCreateClosureTask(base::Bind(base::IgnoreResult(&CefCookieManager::SetCookie), manager.get(), url, cookie, callback)));
		}

		CString sRedirect = m_response.GetRedirectUrl();
		if (!sRedirect.IsEmpty())
		{
			redirectUrl = sRedirect;
			response->SetStatus(303);
			map.insert(std::pair<CefString, CefString>("Location", (LPCTSTR)sRedirect));
		}
		else
		{
			response->SetStatus(m_response.GetStatus());
		}

		const CNameValueCollection& oResponseHeaders = m_response.GetHeaders();
		for (int iHeader = 0; iHeader < oResponseHeaders.GetSize(); iHeader++)
		{
			// add headers to the cef response.
			CNameValuePair* p = oResponseHeaders.GetAt(iHeader);
			map.insert(std::pair<CefString, CefString>((LPCTSTR)p->GetName(), (LPCTSTR)p->GetValue()));
		}

		response->SetHeaderMap(map);

		// Set the resulting response length
		response_length = m_response.GetLength();
	}

	virtual void Cancel() OVERRIDE {
		REQUIRE_IO_THREAD();
	}

	virtual bool ReadResponse(void* data_out,
		int bytes_to_read,
		int& bytes_read,
		CefRefPtr<CefCallback> callback)
		OVERRIDE {
		REQUIRE_IO_THREAD();

		bool has_data = false;
		bytes_read = 0;

		AutoLock lock_scope(this);

		if (offset_ < (size_t)m_response.GetLength()) {
			// Copy the next block of data into the buffer.
			int transfer_size =
				min(bytes_to_read, static_cast<int>(m_response.GetLength() - offset_));
			memcpy(data_out, (LPCSTR)m_response.GetData() + offset_, transfer_size);
			offset_ += transfer_size;

			bytes_read = transfer_size;
			has_data = true;
		}

		return has_data;
	}


private:
	CTBResponse m_response;
	size_t offset_;
	IMPLEMENT_REFCOUNTING(TbLoaderResourceHandler);
	IMPLEMENT_LOCKING(TbLoaderResourceHandler);
};
class TB_EXPORT CBrowser : public CBrowserObj
{
	friend class TBHandler;
private:
	CefRefPtr<CefBrowser> m_pBrowser;
public:
	CBrowser(CefRefPtr<CefBrowser> pBrowser) : m_pBrowser(pBrowser) {}
	HWND CBrowser::GetMainWnd()
	{
		if (!m_pBrowser)
			return NULL;

		return m_pBrowser->GetHost()->GetWindowHandle();
	}

	void CBrowser::Navigate(const CString& sUrl)
	{
		ASSERT(m_pBrowser);
		m_pBrowser->GetMainFrame()->LoadURL((LPCTSTR)sUrl);
	}
	void CBrowser::SetCookie(const CString& sUrl, const CString& sName, const CString& sValue)
	{
		CefRefPtr<CefCookieManager> manager = CefCookieManager::GetGlobalManager(NULL);
		CefCookie cookie;
		CefString(&cookie.name) = sName;
		CefString(&cookie.value) = sValue;
		CefString(&cookie.path).FromASCII("/");
		CefString url = sUrl;
		
		//CefPostTask(TID_IO, NewCefRunnableMethod(manager.get(), &CefCookieManager::SetCookie, url, cookie));
		EmptyCefSetCookieCallback callback;
		CefPostTask(TID_IO, CefCreateClosureTask(base::Bind(base::IgnoreResult(&CefCookieManager::SetCookie), manager.get(), url, cookie, callback)));
	}

	void CBrowser::ExecuteJavascript(const CString& sCode)
	{
		ASSERT(m_pBrowser);
		CefString code = sCode;
		CefString url;
		m_pBrowser->GetMainFrame()->ExecuteJavaScript(code, url, 0);
	}
	void CBrowser::DoClose(bool forceClose)
	{
		ASSERT(m_pBrowser);
		m_pBrowser->GetHost()->CloseBrowser(forceClose);
	}

	void CBrowser::Reload()
	{
		ASSERT(m_pBrowser);
		m_pBrowser->Reload();
	}

	void CBrowser::ReloadIgnoreCache()
	{
		ASSERT(m_pBrowser);
		m_pBrowser->ReloadIgnoreCache();
	}
};

CTBResponse::CTBResponse()
	: m_responseBytes(NULL), m_nBytesLength(0), m_strMimeType(_T("text/plain"))
{
}

int CTBResponse::GetLength()
{
	return m_nBytesLength;
}

BYTE* CTBResponse::GetData()
{
	return m_responseBytes;
}

void CTBResponse::SetData(const CJsonSerializer& jsonData)
{
	SetMimeType(L"application/json");
	SetData(jsonData.GetJson());
}
void CTBResponse::SetData(CString strData)
{
	m_nBytesLength = AtlUnicodeToUTF8(strData, strData.GetLength(), NULL, 0); //calcolo la dimensione richiesta per il buffer

	m_responseBytes = new BYTE[m_nBytesLength];

	AtlUnicodeToUTF8(strData, strData.GetLength(), (LPSTR)m_responseBytes, m_nBytesLength);

}
void CTBResponse::SetData(BYTE* bytes, int nLength)
{
	m_nBytesLength = nLength;
	m_responseBytes = bytes;
}

//================================================================================
//---------------------------CTBRequestHandlerObj---------------------------------
//================================================================================


//--------------------------------------------------------------------------------
CTBRequestHandlerObj::CTBRequestHandlerObj(LPCSTR szPath)
{
	m_Name = szPath;
	m_sPath.Append("/");
	m_sPath.Append(szPath);
	m_sPath.Append("/");
}

//--------------------------------------------------------------------------------
bool CTBRequestHandlerObj::ReadFileContent(LPCTSTR file, CTBResponse& response)
{
	if (!ExistFile(file))
		return false;
#ifndef DEBUG
	CachedFile* pCachedFile = NULL;
	TB_OBJECT_LOCK(&m_Cache);
	if (m_Cache.Lookup(file, pCachedFile))
	{
		BYTE * pData = new BYTE[pCachedFile->m_len];
		memcpy_s(pData, pCachedFile->m_len, pCachedFile->m_data, pCachedFile->m_len);
		response.SetData(pData, pCachedFile->m_len);
		return true;
	}
#endif
	CFile f;
	f.Open(file, CFile::modeRead | CFile::typeBinary);
	int nLength = (int)f.GetLength();
	BYTE* buff = new BYTE[nLength];
	f.Read(buff, nLength);
	response.SetData(buff, nLength);
#ifndef DEBUG
	BYTE * pData = new BYTE[nLength];
	memcpy_s(pData, nLength, buff, nLength);
	m_Cache.SetAt(file, new CachedFile(pData, nLength));
#endif
	return true;
}

//----------------------------------------------------------------------------
///restituisce il login context associato al token; se non esiste, lo crea
//resetta il timer di timeout di sessione
CLoginContext* CTBRequestHandlerObj::GetLoginContext(const CString& authToken, BOOL bCreate)
{
	TB_LOCK_FOR_WRITE();
	CLoginContext* pContext = AfxGetLoginContext(authToken);
	if (!pContext && bCreate)
	{
		pContext = AfxGetLoginContext(authToken);
		if (!pContext)
		{
			pContext = AfxInvokeThreadFunction<CLoginContext*, CTBRequestHandlerObj, CString>(AfxGetApplicationContext()->GetAppMainWnd(), this, &CTBRequestHandlerObj::Login, authToken);
		}
	}

	if (!pContext || !pContext->IsValid())
	{
		return pContext;
	}
	//if we are in IIS context, login thread has a timeout
	//if (m_bIsIISModule)
	//	AfxInvokeAsyncThreadProcedure<CLoginThread, UINT>(pContext->m_nThreadID, (CLoginThread*)pContext, &CLoginThread::StartTimeoutTimer, SESSION_TIMEOUT);
	return pContext;
}

//----------------------------------------------------------------------------
CLoginContext* CTBRequestHandlerObj::Login(CString authenticationToken)
{
	CLoginContext* pContext = AfxGetLoginContext(authenticationToken);
	if (!pContext)
	{
		AfxGetTbCmdManager()->Login(authenticationToken);
		pContext = AfxGetLoginContext(authenticationToken);
	}
	
	AfxGetApplicationContext()->SetRemoteInterface(TRUE);
	
	return pContext;
}

//--------------------------------------------------------------------------------
void CTBRequestHandlerObj::SetMimeType(LPCTSTR path, CTBResponse& response)
{
	if (StrStrI(path, L".js") != NULL) { response.SetMimeType(L"text/javascript"); return; }
	if (StrStrI(path, L".ts") != NULL) { response.SetMimeType(L"application/x-typescript"); return; }
	if (StrStrI(path, L".html") != NULL) { response.SetMimeType(L"text/html"); return; }
	if (StrStrI(path, L".css") != NULL) { response.SetMimeType(L"text/css"); return; }
	if (StrStrI(path, L".gif") != NULL) { response.SetMimeType(L"image/gif"); return; }
	if (StrStrI(path, L".png") != NULL) { response.SetMimeType(L"image/png"); return; }
	if (StrStrI(path, L".jpg") != NULL) { response.SetMimeType(L"image/jpg"); return; }
	if (StrStrI(path, L".bmp") != NULL) { response.SetMimeType(L"image/bmp"); return; }
	if (StrStrI(path, L".eot") != NULL) { response.SetMimeType(L"application/vnd.ms-fontobject"); return; }
	if (StrStrI(path, L".woff") != NULL) { response.SetMimeType(L"application/x-font-woff"); return; }
	if (StrStrI(path, L".ttf") != NULL) { response.SetMimeType(L"application/x-font-TrueType"); return; }
	if (StrStrI(path, L".svg") != NULL) { response.SetMimeType(L"image/svg+xml"); return; }
	if (StrStrI(path, L".ico") != NULL) { response.SetMimeType(L"image/x-icon"); return; }
	
	//in caso non riconosciamo il tipo, proviamo a non mandare niente
	ASSERT(FALSE);
	//response.SetMimeType(L"text/html");
}

// TBHandler implements CefClient and a number of other interfaces.
class TBHandler : public CefClient,
	//public CefContextMenuHandler,
	//public CefDisplayHandler,
	//public CefDownloadHandler,
	//public CefDragHandler,
	//public CefGeolocationHandler,
	// public CefKeyboardHandler,
	public CefLifeSpanHandler,
	//public CefLoadHandler,
	public CefRequestHandler
{
private:
	CBrowserEventsObj* m_pBrowserEvents;
	CArray<CefRefPtr<CefBrowser>> m_arChildBrowsers;
	CBrowser* m_pBrowser;
	HANDLE m_BrowserReady;
public:
	TBHandler(CBrowserEventsObj* pBrowserEvents, BOOL bWaitForBrowserReady) : m_pBrowserEvents(pBrowserEvents), m_pBrowser(NULL)
	{
		m_BrowserReady = bWaitForBrowserReady ? CreateEvent(NULL, TRUE, FALSE, NULL) : NULL;
	}

	void WaitForBrowserReady()
	{
		if (!m_BrowserReady)
			return;
		while (WAIT_OBJECT_0 != ::WaitForSingleObject(m_BrowserReady, 1))
			CTBWinThread::PumpThreadMessages();
		CloseHandle(m_BrowserReady);
		m_BrowserReady = NULL;
	}

	virtual CefRefPtr<CefRequestHandler> GetRequestHandler() OVERRIDE {
		return this;
	}
	virtual CefRefPtr<CefLifeSpanHandler> GetLifeSpanHandler() OVERRIDE {
		return this;
	}

	virtual void OnRenderProcessTerminated(CefRefPtr<CefBrowser> browser,
		TerminationStatus status) OVERRIDE {
		// A render process has crashed...
	}


	virtual void OnProtocolExecution(CefRefPtr<CefBrowser> browser,
		const CefString& url,
		bool& allow_os_execution) OVERRIDE {
		// Handle execution of external protocols...
	}



	// CefRequestHandler methods
	virtual CefRefPtr<CefResourceHandler> GetResourceHandler(
		CefRefPtr<CefBrowser> browser,
		CefRefPtr<CefFrame> frame,
		CefRefPtr<CefRequest> request) OVERRIDE {

		std::string url = request->GetURL();
		// http://host:port/path
		// get a pointer to host:port url session
		const char* sScanPtr = strstr(url.c_str(), "//");
		// skip the "//" sequence
		sScanPtr += 2;
		// move to the beginning of the path
		sScanPtr = strstr(sScanPtr, "/");
		TB_OBJECT_LOCK_FOR_READ(&s_RequestHandlers);
		for (int i = 0; i < s_RequestHandlers.GetCount(); i++)
		{
			CTBRequestHandlerObj* pHandler = s_RequestHandlers[i];
			// move to /tbloader/ path section.
			const char* sLoader = strstr(sScanPtr, pHandler->GetPath()/*"/tbloader/"*/);
			if (sLoader == sScanPtr)
				return new TbLoaderResourceHandler(m_pBrowserEvents, pHandler);
		}
		// 
		return NULL;

	}

	///
	// Called after a new browser is created.
	///
	/*--cef()--*/
	virtual void OnAfterCreated(CefRefPtr<CefBrowser> browser) OVERRIDE
	{
		//only root browser needs a wrapper, popup child browsers have its own life cycle
		if (m_pBrowserEvents && !m_pBrowser)
		{
			m_pBrowser = new CBrowser(browser);
			m_pBrowserEvents->OnAfterCreated(m_pBrowser);
		}
		else
		{
			m_arChildBrowsers.Add(browser);
		}
		::SetEvent(m_BrowserReady);
	}

	virtual void OnBeforeClose(CefRefPtr<CefBrowser> browser) OVERRIDE
	{
		//only root browser needs event management, popup child browsers have its own life cycle
		if (m_pBrowserEvents && m_pBrowser && m_pBrowser->m_pBrowser->GetIdentifier() == browser->GetIdentifier())
		{
			m_pBrowserEvents->OnBeforeClose(m_pBrowser);
			delete m_pBrowser;
			m_pBrowser = NULL;
			//closes child browsers
			for (int i = m_arChildBrowsers.GetUpperBound(); i >= 0; i--)
			{
				CefRefPtr<CefBrowser> b = m_arChildBrowsers.GetAt(i);
				b->GetHost()->CloseBrowser(true);

			}
		}
		else
		{
			for (int i = 0; i < m_arChildBrowsers.GetCount(); i++)
			{
				CefRefPtr<CefBrowser> b = m_arChildBrowsers.GetAt(i);
				if (b->GetIdentifier() == browser->GetIdentifier())
				{
					m_arChildBrowsers.RemoveAt(i);
					break;
				}
			}
		}
	}

	virtual bool DoClose(CefRefPtr<CefBrowser> browser)
	{
		//only root browser needs event management, popup child browsers have its own life cycle
		if (m_pBrowserEvents && m_pBrowser && m_pBrowser->m_pBrowser->GetIdentifier() == browser->GetIdentifier())
		{
			m_pBrowserEvents->DoClose(m_pBrowser);
		}
		return __super::DoClose(browser);
	}
	IMPLEMENT_REFCOUNTING(TBHandler);
};

class SimpleApp : public CefApp,
	public CefBrowserProcessHandler {
public:
	SimpleApp();

	// CefApp methods:
	virtual CefRefPtr<CefBrowserProcessHandler> GetBrowserProcessHandler()
		OVERRIDE {
		return this;
	}

	// CefBrowserProcessHandler methods:
	virtual void OnContextInitialized() OVERRIDE;

private:
	// Include the default reference counting implementation.
	IMPLEMENT_REFCOUNTING(SimpleApp);


};

SimpleApp::SimpleApp() {
}


void SimpleApp::OnContextInitialized() {

}
void AddRequestHandler(CTBRequestHandlerObj* pTbHandler)
{
	TB_OBJECT_LOCK(&s_RequestHandlers);
	s_RequestHandlers.Add(pTbHandler);
}

void DisposeRequestHandlers()
{
	TB_OBJECT_LOCK(&s_RequestHandlers);
	s_RequestHandlers.Reset();
}



DWORD WINAPI CEFThreadFunction(LPVOID param)
{
#ifdef DEBUG
	SetThreadName(::GetCurrentThreadId(), "CEF Thread Function");
#endif
	CefMainArgs args;
	CefSettings settings;
	settings.multi_threaded_message_loop = false;
	settings.single_process = false;
	settings.no_sandbox = true;
	CefString(&settings.browser_subprocess_path) = AfxGetPathFinder()->GetTBDllPath() + L"\\TbCEFProcess.exe";
	//settings.release_dcheck_enabled = true;
#ifdef DEBUG
	settings.log_severity = LOGSEVERITY_ERROR;
	settings.remote_debugging_port = 1717;
	
#endif
	DataInt* pDataObj = (DataInt*)AfxGetSettingValue(CTBNamespace(szTbGenlibNamespace), szEnvironment, _T("ChromeDebuggingPort"), DataInt(0), szTbDefaultSettingFileName);
	if (pDataObj && *pDataObj > 0)
	{
		settings.remote_debugging_port = *pDataObj;
	}
	CefString(&settings.cache_path) = AfxGetPathFinder()->GetAppDataPath(TRUE) + L"\\CefCache";
	CefString(&settings.log_file) = AfxGetPathFinder()->GetTBDllPath() + L"\\cef.log";

	CefRefPtr<CefApp> app = new SimpleApp();
	
	CefInitialize(args, settings, app.get(), NULL);
	SetEvent(hCefReady);
	CefRunMessageLoop();
	CefShutdown();
	return 0;
}
void CEFInitialize()
{
	hCefReady = CreateEvent(NULL, TRUE, FALSE, _T("CEF_READY"));
	hCefThread = CreateThread(NULL, 0, CEFThreadFunction, NULL, 0, NULL);
	WaitForSingleObject(hCefReady, INFINITE);
	CloseHandle(hCefReady);
	//thread per gestire le richieste lato server in modo asincrono, altrimenti il browser resta freezato
	//nel caso di modulo IIS, questo non è necessario, ci pensa IIS a creare i thread
	if (!AfxGetApplicationContext()->IsIISModule())
	{
		g_pRequestHandlerThread = new CRequestThread();
		g_pRequestHandlerThread->CreateThread();
		g_pRequestHandlerThread->WaitForCreation();
	}
}
CTBRequestHandlerObj* TB_EXPORT GetRequestHandler(LPCSTR szPath)
{
	TB_OBJECT_LOCK_FOR_READ(&s_RequestHandlers);
	for (int i = 0; i < s_RequestHandlers.GetCount(); i++)
	{
		CTBRequestHandlerObj* pHandler = s_RequestHandlers[i];
		// move to /tbloader/ path section.
		if (strcmp(szPath, pHandler->GetPath()/*"/tbloader/"*/) == 0)
			return pHandler;
	}
	return NULL;
}

CTBRequestHandlerObj* TB_EXPORT GetRequestHandlerByUrl(CString strUrl)
{
	TB_OBJECT_LOCK_FOR_READ(&s_RequestHandlers);
	for (int i = 0; i < s_RequestHandlers.GetCount(); i++)
	{
		CTBRequestHandlerObj* pHandler = s_RequestHandlers[i];
		// move to /tbloader/ path section.

		CString strHandlerPath(pHandler->GetPath());
		if (strUrl.Find(strHandlerPath) >= 0)
			return pHandler;
	}
	return NULL;
}


void CEFUninitialize()
{
	if (g_pRequestHandlerThread)
	{
		g_pRequestHandlerThread->PostThreadMessage(WM_QUIT, NULL, NULL);
		WaitForSingleObject(g_pRequestHandlerThread->m_hThread, INFINITE);
	}
	
	VERIFY(CefPostTask(TID_UI, CefCreateClosureTask(base::Bind(&CefQuitMessageLoop))));
	WaitForSingleObject(hCefThread, INFINITE);


}

bool CreatePopupBrowser(LPCTSTR sUrl, BOOL bWaitForBrowserReady, CBrowserEventsObj* pEvents /*= NULL*/)
{
	//  REQUIRE_UI_THREAD();

	// Information used when creating the native window.
	CefWindowInfo window_info;

	window_info.SetAsPopup(NULL, "TBLoader");

	// SimpleHandler implements browser-level callbacks.
	CefRefPtr<TBHandler> handler(new TBHandler(pEvents, bWaitForBrowserReady));

	// Specify CEF browser settings here.
	CefBrowserSettings browser_settings;
	browser_settings.file_access_from_file_urls = STATE_ENABLED;
	CefString url;
	url.FromWString(sUrl);

	if (!CefBrowserHost::CreateBrowser(window_info, handler.get(), url,
		browser_settings, NULL))
		return false;
	handler->WaitForBrowserReady();
	return true;
}

bool CreateChildBrowser(HWND hwndParent, LPCTSTR sUrl, BOOL bWaitForBrowserReady, CBrowserEventsObj* pEvents /*= NULL*/, const RECT* pRect/* = NULL*/)
{
	// Information used when creating the native window.
	CefWindowInfo window_info;
	RECT r;
	if (pRect)
	{
		r.left = pRect->left;
		r.top = pRect->top;
		r.right = pRect->right;
		r.bottom = pRect->bottom;
	}
	else
	{
		r.left = 0;
		r.top = 0;
		r.right = 800;
		r.bottom = 800;
	}
	window_info.SetAsChild(hwndParent, r);

	// SimpleHandler implements browser-level callbacks.
	CefRefPtr<TBHandler> handler(new TBHandler(pEvents, bWaitForBrowserReady));

	// Specify CEF browser settings here.
	CefBrowserSettings browser_settings;
	browser_settings.file_access_from_file_urls = STATE_ENABLED;

	std::wstring url = sUrl;
	if (!CefBrowserHost::CreateBrowser(window_info, handler.get(), url, browser_settings, NULL))
		return false;

	handler->WaitForBrowserReady();
	return true;
}

//--------------------------------------------------------------------------------
void GetCookieParams(CString& sCookie, CNameValueCollection& params)
{
	CString token;
	int curPos = 0;
	CString resToken = sCookie.Tokenize(L";", curPos);
	while (!resToken.IsEmpty())
	{
		resToken.Trim();
		int equalIdx = resToken.Find('=');

		CString sName = resToken.Left(equalIdx);
		CString sValue = resToken.Mid(equalIdx + 1).Trim();
		params.Add(sName, sValue);
		resToken = sCookie.Tokenize(L";", curPos);
	};
}

//--------------------------------------------------------------------------------
void GetQueryParams(CStringA& sQuery, CNameValueCollection& params)
{
	int externalCurPos = 0;

	CString strName;
	CString strValue;
	if (sQuery.Find("{", 0) >=0) //se mi arriva un object json...TODOLUCA, non è molto elegante, ma mi manca il content type nella richiesta
	{
		CJsonParser s;
		CString body(sQuery);
		s.ReadJsonFromString(body);
		CJsonIterator* pIterator = s.BeginIteration();
		while (pIterator->GetNext(strName, strValue))
		{
			params.Add(strName, strValue);
		} 
	}
	else
	{
		CString sParam;
		// not empty query string.
		// get uri parameters, they have the form ?name1=value1&name2=value2&...
		// get the uri params string.
		while (!(sParam = sQuery.Tokenize("&", externalCurPos)).IsEmpty())
		{
			int innerCurPos = 0;
			CString strName = sParam.Tokenize(_T("="), innerCurPos);
			CString strValue = sParam.Mid(innerCurPos);
			params.Add(TBUrlUnescape(CStringA(strName)), TBUrlUnescape(CStringA((strValue))));
		}
	}
}


