#include "stdafx.h"

#include "MenuFunctions.h"
#include <TbGeneric\TBThemeManager.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGeneric\DocumentObjectsInfo.h>
#include <TbGeneric\JsonFormEngine.h>

using namespace System;
using namespace System::Threading;

using namespace System::Globalization;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Core::NameSolver;
using namespace Microarea::TaskBuilderNet::Core::MenuManagerLoader;
using namespace Microarea::TaskBuilderNet::Data::DatabaseLayer;
using namespace Microarea::TaskBuilderNet::Core::EasyStudioServer;
using namespace Microarea::TaskBuilderNet::Core::EasyStudioServer::Services;
//---------------------------------------------------------------------------------------
void OpenHistoryLink(HWND hwnd, CString link)
{
	NewMenuFunctions::OpenRecentLink((System::IntPtr)hwnd, gcnew System::String(link));
}

//---------------------------------------------------------------------------------------
CString LoadMenuWithFavoritesAsJson(CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return _T("");

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;
	CString strToken = pLoginContext->GetName();

	String^ json = NewMenuLoader::LoadMenuWithFavoritesAsJson(gcnew String(strUser), gcnew String(strCompany), gcnew String(strToken));
	return CString(json);
}

//---------------------------------------------------------------------------------------
CString CheckMenuReload(CLoginContext* pLoginContext, CString strConnection, CString strFileName)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return _T("");

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;
	System::Xml::XmlDocument^ json = nullptr;

	try
	{
		json = NewMenuLoader::GetMenuXmlInfinity(
			gcnew String(strUser),
			gcnew String(strCompany),
			gcnew String(strConnection),
			gcnew String(strFileName),
			gcnew String(DataInt(pInfos->m_nLoginId).ToString()),
			gcnew String(DataInt(pInfos->m_nCompanyId).ToString())
		);
	}
	catch (Exception^ ex)
	{
		CString err = CString(ex->Message) + _T("\r\n");
		WriteEventViewerMessage(err, EVENTLOG_WARNING_TYPE);
		throw new CApplicationErrorException(err);
	}

	return CString(json == nullptr ? _T("") : json->InnerXml);
}

//---------------------------------------------------------------------------------------
BOOL ParseMenuInfinity(CLoginContext* pLoginContext, CString StrConnection, CString strXmlDoc, CString strFileName)
{
	BOOL bParse = false;
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return false;
	MagoMenuParser^ pMenu = nullptr;

	try {
		pMenu = gcnew MagoMenuParser(
			gcnew String(StrConnection),
			gcnew String(strXmlDoc),
			gcnew String(DataInt(pInfos->m_nLoginId).ToString()),
			gcnew String(DataInt(pInfos->m_nCompanyId).ToString()),
			gcnew String(pInfos->m_strUserName),
			gcnew String(pInfos->m_strCompanyName),
			"ParseMenuInfinity"
		);

		bParse = pMenu->Parse();
	}
	catch (Exception^ ex)
	{
		if (pMenu != nullptr)
			delete pMenu;
		CString err = CString(ex->Message) + _T("\r\n");
		WriteEventViewerMessage(err, EVENTLOG_WARNING_TYPE);
		throw new CApplicationErrorException(err);
	}
	finally
	{
		if (pMenu != nullptr)
			delete pMenu;
	}

	return bParse;
}

//---------------------------------------------------------------------------------------
CString LoadHistoryAsJson(CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return _T("");

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	String^ json = NewMenuLoader::LoadHistoryAsJson(gcnew String(strUser), gcnew String(strCompany));
	return CString(json);
}


//---------------------------------------------------------------------------------------
bool AddToMostUsed(CString target, CString objectType, CString objectName, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::AddToMostUsed(gcnew String(target), gcnew String(objectType), gcnew String(objectName), gcnew String(strUser), gcnew String(strCompany));
}

//---------------------------------------------------------------------------------------
bool RemoveFromMostUsed(CString target, CString objectType, CString objectName, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::RemoveFromMostUsed(gcnew String(target), gcnew String(objectType), gcnew String(objectName), gcnew String(strUser), gcnew String(strCompany));
}

//---------------------------------------------------------------------------------------
bool AddToFavorites(CString target, CString objectType, CString objectName, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::AddToFavorites(gcnew String(target), gcnew String(objectType), gcnew String(objectName), gcnew String(strUser), gcnew String(strCompany));
}

//---------------------------------------------------------------------------------------
bool RemoveFromFavorites(CString target, CString objectType, CString objectName, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::RemoveFromFavorites(gcnew String(target), gcnew String(objectType), gcnew String(objectName), gcnew String(strUser), gcnew String(strCompany));
}

//---------------------------------------------------------------------------------------
bool SetPreference(CString preferenceName, CString preferenceValue, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;


	return NewMenuSaver::SetPreference(gcnew String(preferenceName), gcnew String(preferenceValue), gcnew String(strUser), gcnew String(strCompany));
}

//---------------------------------------------------------------------------------------
bool UpdateMostUsedShowNrElements(CString nrElements, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::UpdateMostUsedShowNrElements(gcnew String(nrElements), gcnew String(strUser), gcnew String(strCompany));
}

//---------------------------------------------------------------------------------------
bool UpdateHistoryShowNrElements(CString nrElements, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::UpdateHistoryShowNrElements(gcnew String(nrElements), gcnew String(strUser), gcnew String(strCompany));
}

//---------------------------------------------------------------------------------------
CString GetMostUsedShowNrElements(CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return _T("");

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return  CString(NewMenuLoader::GetMostUsedShowNrElements(gcnew String(strUser), gcnew String(strCompany)));
}

//---------------------------------------------------------------------------------------
CString GetPreferencesAsJson(CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return _T("");

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return  CString(NewMenuLoader::GetPreferencesAsJson(gcnew String(strUser), gcnew String(strCompany)));
}

//---------------------------------------------------------------------------------------
CString GetHistoryShowNrElements(CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return _T("");

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuLoader::GetHistoryShowNrElements(gcnew String(strUser), gcnew String(strCompany));
}

//---------------------------------------------------------------------------------------
CString LoadBrandInfoAsJson()
{
	return  CString(NewMenuLoader::LoadBrandInfoAsJson());
}

//---------------------------------------------------------------------------------------
CString GetJsonMenuSettings()
{
	return  CString(NewMenuLoader::GetJsonMenuSettings());
}
//---------------------------------------------------------------------------------------
CString GetMenuPage()
{
	return Microarea::TaskBuilderNet::Core::Generic::InstallationData::BrandLoader->GetMenuPage();
}

//---------------------------------------------------------------------------------------
CString DoLogin
(CString username, CString password, CString company, bool rememberMe, bool winNT,
	bool overwriteLogin, bool ccd, bool relogin, CString& sJsonMessage,
	bool &alreadyLogged, bool &changePassword, bool &changeAutologinInfo, CString saveAutologinInfo)
{
	String^ message;
	String^ culture;
	String^ uiCulture;
	String^ token = NewMenuFunctions::DoLogin(
		gcnew String(username),
		gcnew String(password),
		gcnew String(company),
		rememberMe,
		winNT,
		overwriteLogin,
		ccd,
		relogin,
		(IntPtr)AfxGetApplicationContext()->GetMenuWindowHandle(),
		gcnew DatabaseChecker(),
		message,
		alreadyLogged,
		changePassword,
		changeAutologinInfo,
		gcnew String(saveAutologinInfo),
		culture,
		uiCulture
	);

	if (!String::IsNullOrEmpty(culture))
		Thread::CurrentThread->CurrentCulture = gcnew CultureInfo(culture);

	if (!String::IsNullOrEmpty(uiCulture))
		Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(uiCulture);


	sJsonMessage = message;
	return token;
}

//---------------------------------------------------------------------------------------
void DoSSOLogOff
(CString cryptedtoken)
{
	NewMenuFunctions::DoSSOLogOff(
		gcnew String(cryptedtoken));

}

//---------------------------------------------------------------------------------------
CString DoSSOLoginWeb
(CString cryptedtoken, CString user, CString pwd, CString company, bool winNT, bool overwriteLogin,
	bool relogin, CString& sJsonMessage, bool &alreadyLogged, bool &changePassword,
	bool &changeAutologinInfo, int &res, CString saveAutologinInfo)
{
	String^ message;
	String^ token = NewMenuFunctions::DoSSOLoginWeb(
		gcnew String(cryptedtoken),
		gcnew String(user),
		gcnew String(pwd),
		gcnew String(company),
		winNT,
		overwriteLogin,
		relogin, (IntPtr)AfxGetApplicationContext()->GetMenuWindowHandle(),
		gcnew DatabaseChecker(),
		message,
		alreadyLogged,
		changePassword,
		changeAutologinInfo, res,
		gcnew String(saveAutologinInfo));
	sJsonMessage = message;
	return token;
}
//---------------------------------------------------------------------------------------
CString DoLoginWeb
(CString username, CString password, CString company, bool winNT, bool overwriteLogin,
	bool relogin, CString& sJsonMessage, bool &alreadyLogged, bool &changePassword,
	bool &changeAutologinInfo, CString saveAutologinInfo)
{
	String^ message;
	String^ culture;
	String^  uiCulture;
	String^ token = NewMenuFunctions::DoLoginWeb(
		gcnew String(username),
		gcnew String(password),
		gcnew String(company),
		winNT,
		overwriteLogin,
		relogin,
		gcnew DatabaseChecker(),
		message,
		alreadyLogged,
		changePassword,
		changeAutologinInfo,
		gcnew String(saveAutologinInfo),
		culture,
		uiCulture
	);
	sJsonMessage = message;
	return token;
}
//---------------------------------------------------------------------------------------
CString ChangePassword(CString password)
{
	return NewMenuFunctions::ChangePassword(gcnew String(password));
}


//---------------------------------------------------------------------------------------
CString GetUserPreferredTheme(CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return _T("");

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	//se nel file di brand trovo un DefaultTheme, lo uso come override di tema (per apogeo che non ha il selettore) 
	//GetBrandedStringBySourceString torna la stessa chiave inviata se non trova corrispondenza, quindi controllo che non sia neanche "DefaultTheme"
	CString brandedDefaultTheme = Microarea::TaskBuilderNet::Core::Generic::InstallationData::BrandLoader->GetBrandedStringBySourceString("OverrideTheme", true);
	CString strThemePath = brandedDefaultTheme.IsEmpty()
		? CString(NewMenuFunctions::GetUserPreferredTheme(gcnew String(strUser), gcnew String(strCompany)))
		: brandedDefaultTheme;

	CString strThemeFullFileName = AfxGetPathFinder()->GetThemeElementFullName(strThemePath);	
	return ExistFile(strThemeFullFileName) ? strThemePath : _T("");
}

//---------------------------------------------------------------------------------------
CString GetUserPreferredThemePath(CLoginContext* pLoginContext, BOOL isIMago)
{
	CString strTheme = GetUserPreferredTheme(pLoginContext);
	if (strTheme.IsEmpty())
	{
		if (isIMago)
			strTheme = _T("Infinity");
		else
			strTheme = _T("Default");
	}

	//done here because needs settings
	return AfxGetPathFinder()->GetThemeElementFullName(strTheme);
}

//---------------------------------------------------------------------------------------
void ChangeTheme(CString themePath, CLoginContext* pLoginContext)
{
	//cambio il tema in c++
	AfxGetThemeManager()->InitializeFromFullPath(themePath);

	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;
	//cambio il tema in c#
	NewMenuFunctions::ChangeTheme(gcnew String(themePath), gcnew String(strUser), gcnew String(strCompany));

}

//---------------------------------------------------------------------------------------
CString GetEasyBuilderAppAssembliesPathsAsJson(CString docNs, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return _T("");

	CString strUser = pInfos->m_strUserName;

	const CDocumentDescription*	pDesc = AfxGetDocumentDescription(docNs);
	if (pDesc && !pDesc->IsDesignable())
		return _T("");

	return CString(NewMenuFunctions::GetEasyBuilderAppAssembliesPathsAsJson(gcnew String(docNs), gcnew String(strUser)));
}

//---------------------------------------------------------------------------------------
bool ClearHistory(CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::ClearHistory(gcnew String(strUser), gcnew String(strCompany));
}

//---------------------------------------------------------------------------------------
bool ClearMostUsed(CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::ClearMostUsed(gcnew String(strUser), gcnew String(strCompany));
}

//---------------------------------------------------------------------------------------
bool SetLeftGroupVisibility(CString groupName, CString visibile, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::SetLeftGroupVisibility(gcnew String(groupName), gcnew String(visibile), gcnew String(strUser), gcnew String(strCompany));
}

//---------------------------------------------------------------------------------------
CString GetNewMessages(CLoginContext* pLoginContext)
{
	if (pLoginContext == NULL)
		return _T("");

	return Microarea::TaskBuilderNet::Core::NotificationManager::MessagesManager::GetNewMessages(gcnew String(pLoginContext->GetName()));
}


//---------------------------------------------------------------------------------------
CString IsAutoLoginable()
{
	return  NewMenuFunctions::IsAutoLoginable();
}

//---------------------------------------------------------------------------------------
CString GetRememberMe()
{
	return  NewMenuFunctions::GetRememberMe();
}
//---------------------------------------------------------------------------------------
void SetRememberMe(CString checkedVal)
{
	NewMenuFunctions::SetRememberMe((gcnew String(checkedVal)));
}

//---------------------------------------------------------------------------------------
CString GetLoginInitInformation()
{
	return  CString(NewMenuLoader::GetLoginInitInformation());
}
//---------------------------------------------------------------------------------------
CString GetConnectionInformation()
{
	return  CString(NewMenuLoader::GetConnectionInformation());
}

//---------------------------------------------------------------------------------------
CString GetLoginInitImage()
{
	return  CString(NewMenuLoader::GetLoginInitImage());
}

//---------------------------------------------------------------------------------------
CString GetCurrentUTCTime()
{
	return  CString(DateTime::UtcNow.ToString("o"));
}

//---------------------------------------------------------------------------------------
CString GetLoginBackgroundImage()
{
	return  CString(NewMenuLoader::GetLoginBackgroundImage());
}

//---------------------------------------------------------------------------------------
bool UpdateFavoritesPosition(CString target1, CString objectType1, CString objectName1, CString target2, CString objectType2, CString objectName2, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::UpdateFavoritesPosition
	(
		gcnew String(strUser),
		gcnew String(strCompany),
		gcnew String(target1),
		gcnew String(objectType1),
		gcnew String(objectName1),
		gcnew String(target2),
		gcnew String(objectType2),
		gcnew String(objectName2)
	);
}
//---------------------------------------------------------------------------------------
bool AddToHiddenTiles(CString& appName, CString& groupName, CString& menuName, CString& tileName, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::AddToHiddenTiles
	(
		gcnew String(strUser),
		gcnew String(strCompany),
		gcnew String(appName),
		gcnew String(groupName),
		gcnew String(menuName),
		gcnew String(tileName)
	);
}

//---------------------------------------------------------------------------------------
void ClearCachedData(CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return;

	CString strUser = pInfos->m_strUserName;

	Microarea::TaskBuilderNet::Core::Generic::Functions::ClearCachedData(gcnew String(strUser));
	AfxGetStringLoader()->ClearCache();
	CJsonFormEngineObj::GetInstance()->ClearCache();
}


//---------------------------------------------------------------------------------------
bool RemoveFromHiddenTiles(CString& appName, CString& groupName, CString& menuName, CString& tileName, CLoginContext* pLoginContext)
{
	const CLoginInfos* pInfos = pLoginContext->GetLoginInfos();
	if (pInfos == NULL)
		return true;

	CString strUser = pInfos->m_strUserName;
	CString strCompany = pInfos->m_strCompanyName;

	return NewMenuSaver::RemoveFromHiddenTiles
	(
		gcnew String(strUser),
		gcnew String(strCompany),
		gcnew String(appName),
		gcnew String(groupName),
		gcnew String(menuName),
		gcnew String(tileName)
	);
}


//---------------------------------------------------------------------------------------
void PingViaSMS()
{
	Microarea::TaskBuilderNet::Core::Generic::MenuStaticFunctions::PingViaSMS();
}
//---------------------------------------------------------------------------------------
void OpenProducerSite()
{
	Microarea::TaskBuilderNet::Core::Generic::MenuStaticFunctions::OpenProducerSite();
}

//---------------------------------------------------------------------------------------
void CloseCustomizationContext()
{
	BaseCustomizationContext::CustomizationContextInstance->ChangeEasyBuilderApp(String::Empty, String::Empty);
}

//---------------------------------------------------------------------------------------
bool DbCheck(CString& authToken, CString& messages)
{
	try
	{
		IDatabaseCkecker^ checker = gcnew	DatabaseChecker();
		bool ok = checker->Check(gcnew String(authToken));
		if (!ok)
		{
			for each (IDiagnosticItem^ item in checker->Diagnostic->AllItems)
			{
				messages += CString(item->FullExplain);
			}
		}
		return ok;
	}
	catch (Exception^ ex)
	{
		messages += CString(ex->Message);
		return false;
	}
}

//---------------------------------------------------------------------------------------
void SendCurrentToken(HWND menuHandle, CString& token)
{
	NewMenuFunctions::SendCurrentToken((System::IntPtr) menuHandle, gcnew String(token));
}

//---------------------------------------------------------------------------------------
void InitTBPostLogin(HWND menuHandle, CString& authToken, CString& username, bool clearCacheData, CString& culture, CString& uiCulture, CString& messages)
{
	NewMenuFunctions::InitTBPostLogin((System::IntPtr) menuHandle, gcnew String(authToken),  gcnew String(username), clearCacheData, gcnew String(culture), gcnew String(uiCulture));
}

//---------------------------------------------------------------------------------------
bool ProcessOfficeFile(CString&  application, CString&  ns, CString&  subType, CLoginContext* pContext)
{
	if (!pContext)
		return false;

	const CLoginInfos* pInfos = pContext->GetLoginInfos();

	return Microarea::TaskBuilderNet::Core::Generic::MenuStaticFunctions::ProcessOfficeFile
	(
		gcnew String(application),
		gcnew String(ns),
		gcnew String(subType),
		gcnew String(pInfos->m_strUserName),
		gcnew String(pInfos->m_strCompanyName),
		gcnew String(pInfos->m_strPreferredLanguage)
	);
}

//---------------------------------------------------------------------------------------
CString GetEasyBuilderApps()
{
	ApplicationService^ appService = (ApplicationService^)ServicesManager::ServicesManagerInstance->GetService(ApplicationService::typeid);
	return	appService->GetAppsModsAsJson(Microarea::TaskBuilderNet::Interfaces::ApplicationType::Customization, NewMenuFunctions::IsDeveloperEdition());
	//	return NewMenuFunctions::GetAllApps();
}

//---------------------------------------------------------------------------------------
void SetApplicAndModule(CString app, CString mod)
{
	NewMenuFunctions::SetAppAndModule(gcnew System::String(app), gcnew System::String(mod));	
}

//---------------------------------------------------------------------------------------
void CreateNewContextNF(CString app, CString mod, CString type)
{
	NewMenuFunctions::CreateNewContext(gcnew System::String(app), gcnew System::String(mod), gcnew System::String(type));
}

//---------------------------------------------------------------------------------------
void GetEasyBuilderAppAndModule(CString& applicationName, CString& moduleName)
{
	String^ appNameLastSelect;
	String^ modNameLastSelect;

	NewMenuFunctions::GetEasyBuilderAppAndModule(appNameLastSelect, modNameLastSelect);

	applicationName = appNameLastSelect;
	moduleName = modNameLastSelect;
}

//---------------------------------------------------------------------------------------
void GetDefaultContextMF(CString& applicationName, CString& moduleName)
{
	String^ appNamePredefXml;
	String^ modNamePredefXml;
	CLoginContext* pContext = AfxGetLoginContext();
	if (!pContext)
		return;

	const CLoginInfos* pInfos = pContext->GetLoginInfos();
	NewMenuFunctions::GetDefaultContextXml(
		gcnew System::String(appNamePredefXml), gcnew System::String(modNamePredefXml),
		gcnew String(pInfos->m_strUserName), gcnew String(pInfos->m_strCompanyName)
	);

	applicationName = appNamePredefXml;
	moduleName = modNamePredefXml;
}

//---------------------------------------------------------------------------------------
void SetDefaultContextMF(CString applicationName, CString moduleName)
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (!pContext)
		return;

	const CLoginInfos* pInfos = pContext->GetLoginInfos();
	NewMenuFunctions::SetDefaultContext(
		gcnew System::String(applicationName), gcnew System::String(moduleName),
		gcnew String(pInfos->m_strUserName), gcnew String(pInfos->m_strCompanyName)
	);
}

//---------------------------------------------------------------------------------------
BOOL CloneAsEasyStudioDocument(CString strOriginalNs, CString strNewNs, CString strNewTitle)
{
	CTBNamespace* pOriginalNs = new CTBNamespace(CTBNamespace::DOCUMENT, strOriginalNs);
	CTBNamespace newNs(CTBNamespace::DOCUMENT, strNewNs);

	DocumentService^ service = dynamic_cast<DocumentService^>(ServicesManager::ServicesManagerInstance->GetService(DocumentService::typeid));
	if (
		service == nullptr ||
		!service->DeclareDocumentFrom
		(
			gcnew String(pOriginalNs->ToString()),
			gcnew String(newNs.ToString()),
			gcnew String(strNewTitle)
			)
		)
		return FALSE;

	// document new name and clone
	CDocumentDescription* pOriginalDescription = const_cast<CDocumentDescription*>(AfxGetDocumentDescription(*pOriginalNs));
	CDocumentDescription* pDescription = pOriginalDescription->Clone();
	if (pDescription)
	{
		pDescription->SetTemplateNamespace(pOriginalNs);
		pDescription->SetNamespace(newNs);
		pDescription->SetNotLocalizedTitle(strNewTitle);
		AfxAddDocumentDescription(pDescription);
	}
	
	return TRUE;
}
//--------------------------------------------------------------------------------
void RefreshESApps(const CString& path, const CNameValueCollection& params)
{
	NewMenuFunctions::RefreshESApps();
}

//--------------------------------------------------------------------------------
BOOL ThereAreOpenedDocumentsWithES()
{
	CLoginContext* pContext = AfxGetLoginContext();
	return pContext && pContext->GetOpenDocumentsInDesignMode() > 0;
}













