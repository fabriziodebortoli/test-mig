#pragma once

#include <TbNameSolver\TBResourceLocker.h>
#include <tbgenlib\CEFClasses.h>
#include <TBApplication\LoginThread.h>
#include <TBApplication\CDesignManipulator.h>
#include "TBGenericHandler.h"
#include "beginh.dex"

class CTbMenuHandler;
class CJSonResponse;
class CDocumentSession;
class CTaskBuilderApp;
class CLoginContext;
class CBaseDocument;

CString GetJsonProductInfo();

//classe che gestisce la chiamate rest usate per il prototipo mobile
class TB_EXPORT CTbWebHandler : public CTbGenericHandler
{
private:

	typedef void(CTbWebHandler::*FUNCPTR)(const CString& path, const CNameValueCollection&, CTBResponse&);
	typedef CMap<CString, LPCTSTR, FUNCPTR, FUNCPTR> CTbWebHandlerFunctionMap;

	CTbWebHandlerFunctionMap functionMap;

public:
	CTbWebHandler();
	~CTbWebHandler();

	LPCSTR  GetObjectName() const { return "CTbWebHandler"; };
	void ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	BOOL ExecuteOnLoginThread(FUNCPTR func, const CString& path, const CNameValueCollection& params, CTBResponse& response);
private:
	CString GetJsonThemesList();

	void InitTBLoginFunction			(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void DoLogoffFunction				(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void CanLogoffFunction				(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void CommandFunction				(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetImageFunction				(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunFunction					(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetHotlinkQuery				(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetRadarQuery					(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetProductInfoFunction			(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetApplicationDateFunction		(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ChangeApplicationDateFunction	(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetOnlineHelpFunction			(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetThemesFunction				(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ChangeThemesFunction			(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void AssignWoormParameters			(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetDBTSlaveBufferedModel		(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void AddRowDBTSlaveBuffered			(const CString& path, const CNameValueCollection& params, CTBResponse& response);

	void AssignWoormParametersInternal  (HWND hWndWoormDoc, CString reportOutParameters);

	void GetAllAppsAndModules(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void SetAppAndModule(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void CreateNewContext(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetDefaultContextXml(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunEasyStudioFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunEasyStudio(const CString& path, const CNameValueCollection& params);
	void RefreshEasyBuilderApps(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetCurrentContext(const CString& path, const CNameValueCollection& params, CTBResponse& response);	
	void IsEasyStudioDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	//void InitEasyStudioData(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetCustomizationsForDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void CloseCustomizationContextFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void CloneEasyStudioDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	bool CloneEasyStudioDocument(const CNameValueCollection& params);
	bool CloneEasyStudioDocument(CString originalNamespace, CString newNamespace, CString strNewTitle);
	void CanModifyContextFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetWebSocketsPortFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);

};

#include "endh.dex"
