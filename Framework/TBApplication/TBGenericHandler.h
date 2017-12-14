#pragma once

#include <TbNameSolver\TBResourceLocker.h>
#include <tbgenlib\CEFClasses.h>
#include <TBApplication\LoginThread.h>
#include "beginh.dex"

class CTbMenuHandler;
class CJSonResponse;
class CDocumentSession;
class CTaskBuilderApp;
class CLoginContext;
class CBaseDocument;

//classe che gestisce la chiamate rest usate per il prototipo mobile
class TB_EXPORT CTbGenericHandler : public CTBRequestHandlerObj
{
protected: 
	CString m_strRoot;
	CString m_strStandard;
public:
	CTbGenericHandler(LPCSTR szPath);
	~CTbGenericHandler();
	CLoginContext* GetLoginContext(const CNameValueCollection& params, BOOL bCreate = FALSE);
	LPCSTR  GetObjectName() const { return "CTbGenericHandler"; }
	bool PreProcessRequest(const CString& method, CMap<CString, LPCTSTR, CString, LPCTSTR>& requestHeaders, const CString& path, CTBResponse& response);
	void ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response);
};

#include "endh.dex"
