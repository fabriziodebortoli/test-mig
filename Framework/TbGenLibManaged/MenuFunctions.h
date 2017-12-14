#pragma once

#include "stdafx.h"
#include <TbNameSolver\LoginContext.h>
#include <TbGenLibManaged\main.h>

#include "beginh.dex"

TB_EXPORT void OpenHistoryLink(HWND hwnd, CString link);
TB_EXPORT CString LoadMenuWithFavoritesAsJson(CLoginContext* pLoginContext);
TB_EXPORT CString LoadHistoryAsJson(CLoginContext* pLoginContext);

TB_EXPORT bool UpdateMostUsedShowNrElements(CString nrElements, CLoginContext* pLoginContext);
TB_EXPORT bool UpdateHistoryShowNrElements(CString nrElements, CLoginContext* pLoginContext);
TB_EXPORT CString GetMostUsedShowNrElements(CLoginContext* pLoginContext);
TB_EXPORT CString GetPreferencesAsJson(CLoginContext* pLoginContext);
TB_EXPORT CString GetHistoryShowNrElements(CLoginContext* pLoginContext);


TB_EXPORT bool AddToMostUsed(CString target, CString objectType, CString objectName, CLoginContext* pLoginContext);
TB_EXPORT bool RemoveFromMostUsed(CString target, CString objectType, CString objectName, CLoginContext* pLoginContext);
TB_EXPORT bool ClearMostUsed(CLoginContext* pLoginContext);

TB_EXPORT bool AddToFavorites(CString target, CString objectType, CString objectName, CLoginContext* pLoginContext);
TB_EXPORT bool RemoveFromFavorites(CString target, CString objectType, CString objectName, CLoginContext* pLoginContext);

TB_EXPORT bool ClearHistory(CLoginContext* pLoginContext);


TB_EXPORT CString GetNewMessages(CLoginContext* pLoginContext);
TB_EXPORT bool SetLeftGroupVisibility(CString groupName, CString visibile, CLoginContext* pLoginContext);

TB_EXPORT bool UpdateFavoritesPosition(CString target1, CString objectType1, CString objectName1, CString target2, CString objectType2, CString objectName2, CLoginContext* pLoginContext);

TB_EXPORT bool SetPreference(CString preferenceName, CString preferenceValue, CLoginContext* pLoginContext);
TB_EXPORT CString LoadBrandInfoAsJson();
TB_EXPORT CString GetJsonMenuSettings();
TB_EXPORT CString GetLoginInitInformation();
TB_EXPORT CString GetConnectionInformation();
TB_EXPORT CString GetLoginInitImage();
TB_EXPORT CString GetLoginBackgroundImage();
TB_EXPORT CString IsAutoLoginable();
TB_EXPORT CString GetRememberMe();
TB_EXPORT void SetRememberMe(CString checkedVal);
TB_EXPORT bool AddToHiddenTiles(CString& appName, CString& groupName, CString& menuName, CString& tileName, CLoginContext* pLoginContext);
TB_EXPORT bool RemoveFromHiddenTiles(CString& appName, CString& groupName, CString& menuName, CString& tileName, CLoginContext* pLoginContext);
TB_EXPORT CString GetMenuPage();	
TB_EXPORT CString DoLogin(CString username, CString password, CString company, bool rememberMe, bool winNT, bool overwriteLogin, bool ccd, bool relogin, CString& sJsonMessage, bool &alreadyLogged, bool &changePassword, bool &changeAutologinInfo, CString saveAutologinInfo);
TB_EXPORT CString DoLoginWeb(CString username, CString password, CString company, bool winNT, bool overwriteLogin, bool relogin, CString& sJsonMessage, bool &alreadyLogged, bool &changePassword, bool &changeAutologinInfo, CString saveAutologinInfo);
TB_EXPORT CString DoSSOLoginWeb(CString cryptedtoken, CString user, CString pwd, CString company, bool winNT, bool overwriteLogin, bool relogin, CString& sJsonMessage, bool &alreadyLogged, bool &changePassword, bool &changeAutologinInfo, int &res, CString saveAutologinInfo);
TB_EXPORT void DoSSOLogOff(CString cryptedtoken);
TB_EXPORT CString  ChangePassword(CString password);

TB_EXPORT void ChangeTheme(CString themePath, CLoginContext* pLoginContext);
TB_EXPORT CString GetEasyBuilderAppAssembliesPathsAsJson(CString docNs, CLoginContext* pLoginContext);
TB_EXPORT CString GetUserPreferredTheme(CLoginContext* pLoginContext);
TB_EXPORT CString GetUserPreferredThemePath(CLoginContext* pLoginContext, BOOL isIMago);

TB_EXPORT void ClearCachedData(CLoginContext* pLoginContext);

TB_EXPORT void GetEasyBuilderAppAndModule(CString& applicationName, CString& moduleName);
TB_EXPORT void GetDefaultContextMF(CString& applicationName, CString& moduleName);
TB_EXPORT void SetDefaultContextMF(CString applicationName, CString moduleName);

TB_EXPORT void PingViaSMS();
TB_EXPORT void OpenProducerSite();
TB_EXPORT CString GetEasyBuilderApps();
TB_EXPORT void SetApplicAndModule(CString app, CString mod);
//TB_EXPORT void SetDefaultContext(CString app, CString mod);
TB_EXPORT void CreateNewContextNF(CString app, CString mod, CString type);
TB_EXPORT bool ProcessOfficeFile(CString&  application, CString&  ns, CString&  subType, CLoginContext* pContext);
TB_EXPORT void CloseCustomizationContext();
TB_EXPORT void InitTBPostLogin(HWND menuHandle, CString& token, CString& username, bool clearCacheData, CString& culture, CString& uiCulture, CString& messages);
TB_EXPORT void SendCurrentToken(HWND menuHandle, CString& token);
TB_EXPORT bool DbCheck(CString& authToken, CString& messages);
TB_EXPORT BOOL CloneAsEasyStudioDocument(CString strOriginalNs, CString strNewNs, CString strNewTitle);
TB_EXPORT CString CheckMenuReload(CLoginContext* pLoginContext, CString strConnection, CString strLogFileName);
TB_EXPORT BOOL ParseMenuInfinity(CLoginContext* pLoginContext, CString strConnection, CString strXmlDoc, CString strLogFileName);

TB_EXPORT BOOL ThereAreOpenedDocumentsWithES();
TB_EXPORT void RefreshESApps(const CString& path, const CNameValueCollection& params);
#include "endh.dex"

