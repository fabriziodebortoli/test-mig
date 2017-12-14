#pragma once

#include "TbMenuHandler.h"

#include "beginh.dex"
class TB_EXPORT CTbMenuHandlerGDI : public CTbMenuHandler
{
private:
	typedef void(CTbMenuHandlerGDI::*FUNCPTR)(const CString& path, const CNameValueCollection&, CTBResponse&);
	typedef CMap<CString, LPCTSTR, FUNCPTR, FUNCPTR> CTbMenuHandlerGDIFunctionMap;

	CTbMenuHandlerGDIFunctionMap functionMap;

public:
	CTbMenuHandlerGDI();
	virtual ~CTbMenuHandlerGDI(void);

	virtual void ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	BOOL ExecuteOnLoginThread(FUNCPTR func, const CString& path, const CNameValueCollection& params, CTBResponse& response);

private:
	bool RunDocument(const CNameValueCollection& params);
	bool RunDocument(const CString ns, const CString sAuxInfoArgs, const CString sKeyArgs, BOOL bForeground = FALSE);
	bool RunReport(const CNameValueCollection& params);
	bool RunFunction(const CNameValueCollection& params);
	bool RunHistory(const CNameValueCollection& params);
	bool RunHistory(CString link);
	bool RunOfficeItem(const CNameValueCollection& params);
	bool CloseDocument(const CNameValueCollection& params);
	//	void SetWord(T Hi, T Low, DWORD &out);

	void RunDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunReportFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunFunctionFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunOfficeItemFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void CloseDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ShowProductInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void OpenUrlLinkFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void NewLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ActivateViaInternetFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void DoLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void PostLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunUrlFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetInstallationInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);

protected:
	virtual CLoginContext* Login(CString authenticationToken);
};

#include "endh.dex"




