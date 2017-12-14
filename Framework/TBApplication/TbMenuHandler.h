#pragma once
#include <TbNameSolver\TBResourceLocker.h>
#include <tbgenlib\CEFClasses.h>
#include <TBApplication\LoginThread.h>
#include "TBGenericHandler.h"
#include "beginh.dex"

class CJSonResponse;
class CDocumentSession;
class CTaskBuilderApp;
class CLoginContext;
class CBaseDocument;
//typedef void(*pfunc)(void);


class TB_EXPORT CTbMenuHandler : public CTbGenericHandler
{	
	
	friend class CRestApiHandler;
	bool	m_bIsIISModule;

protected:

	typedef void (CTbMenuHandler::*FUNCPTR)(const CString& path, const CNameValueCollection&, CTBResponse&);
	typedef CMap<CString, LPCTSTR, FUNCPTR, FUNCPTR> CTbMenuHandlerFunctionMap;

	CTbMenuHandlerFunctionMap functionMap;
public:
	CTbMenuHandler();
	~CTbMenuHandler(void);

	void ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	LPCSTR  GetObjectName() const { return "CTbMenuHandler"; };
	CString GetAndFormatCustomizationContextApplicationAndModule();

private:	
	BOOL ExecuteOnLoginThread(FUNCPTR, const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunObject(const CNameValueCollection& params, CJSonResponse& aResponse, ObjectType type);
	void GetLoginCompaniesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void DoLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void DossologoffFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void DossologinFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);	
	void ChangePasswordFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void CanChangeLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void SetRememberMeFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetRememberMeFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void IsAutoLoginableFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetLoginInitInformationFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetLoginInitImageFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetLoginBackgroundImageFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void StaticImageFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void StaticThumbnailFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void LoadCustomThemeFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetLocalizedElementsFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void LoadDynamicThemeFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetMenuElementsFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetThemedSettingsFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void AddToMostUsedFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RemoveFromMostUsedFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void UpdateMostUsedShowNrFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetCultureFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetMostUsedShowNrFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetUserInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetBrandInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void UnFavoriteObjectFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void FavoriteObjectFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetNewMessagesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void UpdateFavoritesPositionFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetPreferencesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void SetPreferenceFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ClearAllMostUsedFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ChangeApplicationDateFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetApplicationDateFormatFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetApplicationDateFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void AddToHiddenTilesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RemoveFromHiddenTilesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetThemesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ClearCachedDataFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ChangeThemesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetProductInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetCustomizationContextAppAndModuleFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void GetConnectionInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ActivateViaSMSFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ProducerSiteFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);

	//l'handler gdi le overrida per la gestione specifica interattiva
	void RunObjectFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunReportFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunFunctionFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void RunOfficeItemFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void CloseDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ShowProductInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void OpenUrlLinkFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ChangeLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void NewLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void ActivateViaInternetFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	void PostLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
protected:

	void FindHwndNotInOpenDocuments(CArray<HWND>& arNotInOpenDocuments);
	
	CString GetJsonLocalizedContent(const CString& authToken);
	CString GetJsonOpenDocuments();
	CString GetJsonThemesList();
	CString GetDynamicCssTheme();
	
	CString GetJsonCompaniesForUser(CString strUser);
	CString GetJsonWorkerInfos(CString authToken);

	CString GetDocumentType(CBaseDocument* pDoc);
	BOOL	MenuInfinity(CLoginContext* pContext);

	// EasyStudio Handlers

	void CloneEasyStudioDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response);
	bool CloneEasyStudioDocument(const CNameValueCollection& params);
	bool CloneEasyStudioDocument(CString originalNamespace, CString newNamespace, CString strNewTitle);

protected:

	bool ExecutedOnLoginInfinity(const CString& path, const CNameValueCollection& params, CTBResponse& response);
};


#include "endh.dex"
