#include "stdafx.h"
#include "TbMenuHandler.h"

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGenlib\TBCommandInterface.h>
#include <TbGenlib\PARSCTRL.H>
#include <TbGenLibManaged\GlobalFunctions.h>
#include <TbGenLibManaged\Main.h>
#include <TBApplication\LoginThread.h>
#include <TBApplication\TaskBuilderApp.h>
#include <TBApplication\TbCommandManager.h>
#include <TbGes\BODYEDIT.H>
#include <TbGenLibManaged\MenuFunctions.h>
#include "DocumentThread.h"
#include "CDesignManipulator.h"
#include "TBWebHandler.h"


class AddOnModule;

#define ADD_LOCALIZED_CONTENT(key, value) \
	jsonSerializer.OpenObject(elements);\
	jsonSerializer.WriteString(_T("key"), key);\
	jsonSerializer.WriteString(_T("value"), value);\
	jsonSerializer.CloseObject();\
	elements++;\

//----------------------------------------------------------------------------
CTbMenuHandler::CTbMenuHandler()
	:
	CTbGenericHandler("tb/menu")
{
	m_bIsIISModule = TRUE == AfxGetApplicationContext()->IsIISModule();

	//richiedono login thread

	functionMap.SetAt(_T("getloginCompanies/"), &CTbMenuHandler::GetLoginCompaniesFunction);
	functionMap.SetAt(_T("doLogin/"), &CTbMenuHandler::DoLoginFunction);
	functionMap.SetAt(_T("ssologoff"), &CTbMenuHandler::DossologoffFunction);
	functionMap.SetAt(_T("ssologin"), &CTbMenuHandler::DossologinFunction);
	functionMap.SetAt(_T("changePassword/"), &CTbMenuHandler::ChangePasswordFunction);
	functionMap.SetAt(_T("canChangeLogin/"), &CTbMenuHandler::CanChangeLoginFunction);
	functionMap.SetAt(_T("setRememberMe/"), &CTbMenuHandler::SetRememberMeFunction);
	functionMap.SetAt(_T("getRememberMe/"), &CTbMenuHandler::GetRememberMeFunction);
	functionMap.SetAt(_T("isAutoLoginable/"), &CTbMenuHandler::IsAutoLoginableFunction);
	functionMap.SetAt(_T("getLoginInitInformation/"), &CTbMenuHandler::GetLoginInitInformationFunction);
	functionMap.SetAt(_T("getLoginInitImage/"), &CTbMenuHandler::GetLoginInitImageFunction);
	functionMap.SetAt(_T("getLoginBackgroundImage/"), &CTbMenuHandler::GetLoginBackgroundImageFunction);
	//functionMap.insert(Pair(_T("staticimage/"), &CTbMenuHandler::StaticImageFunction);
	//functionMap.insert(Pair(_T("staticThumbnail/"), &CTbMenuHandler::StaticThumbnailFunction);
	functionMap.SetAt(_T("getLocalizedElements/"), &CTbMenuHandler::GetLocalizedElementsFunction);
	functionMap.SetAt(_T("LoadDynamicTheme/"), &CTbMenuHandler::LoadDynamicThemeFunction);
	functionMap.SetAt(_T("getMenuElements/"), &CTbMenuHandler::GetMenuElementsFunction);
	functionMap.SetAt(_T("getThemedSettings/"), &CTbMenuHandler::GetThemedSettingsFunction);
	functionMap.SetAt(_T("addToMostUsed/"), &CTbMenuHandler::AddToMostUsedFunction);
	functionMap.SetAt(_T("removeFromMostUsed/"), &CTbMenuHandler::RemoveFromMostUsedFunction);
	functionMap.SetAt(_T("updateMostUsedShowNr/"), &CTbMenuHandler::UpdateMostUsedShowNrFunction);
	functionMap.SetAt(_T("getCulture/"), &CTbMenuHandler::GetCultureFunction);
	functionMap.SetAt(_T("getMostUsedShowNr/"), &CTbMenuHandler::GetMostUsedShowNrFunction);
	functionMap.SetAt(_T("getUserInfo/"), &CTbMenuHandler::GetUserInfoFunction);
	functionMap.SetAt(_T("getBrandInfo/"), &CTbMenuHandler::GetBrandInfoFunction);
	functionMap.SetAt(_T("unFavoriteObject/"), &CTbMenuHandler::UnFavoriteObjectFunction);
	functionMap.SetAt(_T("favoriteObject/"), &CTbMenuHandler::FavoriteObjectFunction);
	functionMap.SetAt(_T("getNewMessages/"), &CTbMenuHandler::GetNewMessagesFunction);
	functionMap.SetAt(_T("updateFavoritesPosition/"), &CTbMenuHandler::UpdateFavoritesPositionFunction);
	functionMap.SetAt(_T("getPreferences/"), &CTbMenuHandler::GetPreferencesFunction);
	functionMap.SetAt(_T("setPreference/"), &CTbMenuHandler::SetPreferenceFunction);
	functionMap.SetAt(_T("clearAllMostUsed/"), &CTbMenuHandler::ClearAllMostUsedFunction);
	functionMap.SetAt(_T("changeApplicationDate/"), &CTbMenuHandler::ChangeApplicationDateFunction);
	functionMap.SetAt(_T("getApplicationDateFormat/"), &CTbMenuHandler::GetApplicationDateFormatFunction);
	functionMap.SetAt(_T("getApplicationDate/"), &CTbMenuHandler::GetApplicationDateFunction);
	functionMap.SetAt(_T("addToHiddenTiles/"), &CTbMenuHandler::AddToHiddenTilesFunction);
	functionMap.SetAt(_T("removeFromHiddenTiles/"), &CTbMenuHandler::RemoveFromHiddenTilesFunction);
	functionMap.SetAt(_T("getThemes/"), &CTbMenuHandler::GetThemesFunction);
	functionMap.SetAt(_T("clearCachedData/"), &CTbMenuHandler::ClearCachedDataFunction);
	functionMap.SetAt(_T("changeThemes/"), &CTbMenuHandler::ChangeThemesFunction);
	functionMap.SetAt(_T("getProductInfo/"), &CTbMenuHandler::GetProductInfoFunction);
	functionMap.SetAt(_T("getCustomizationContextAppAndModule/"), &CTbMenuHandler::GetCustomizationContextAppAndModuleFunction);
	functionMap.SetAt(_T("getConnectionInfo/"), &CTbMenuHandler::GetConnectionInfoFunction);
	functionMap.SetAt(_T("activateViaSMS/"), &CTbMenuHandler::ActivateViaSMSFunction);
	functionMap.SetAt(_T("producerSite/"), &CTbMenuHandler::ProducerSiteFunction);
	functionMap.SetAt(_T("runObject/"), &CTbMenuHandler::RunObjectFunction);
	functionMap.SetAt(_T("runDocument/"), &CTbMenuHandler::RunDocumentFunction);
	functionMap.SetAt(_T("runReport/"), &CTbMenuHandler::RunReportFunction);
	functionMap.SetAt(_T("runFunction/"), &CTbMenuHandler::RunFunctionFunction);
	functionMap.SetAt(_T("runOfficeItem/"), &CTbMenuHandler::RunOfficeItemFunction);
	functionMap.SetAt(_T("closeDocument/"), &CTbMenuHandler::CloseDocumentFunction);
	functionMap.SetAt(_T("showProductInfo/"), &CTbMenuHandler::ShowProductInfoFunction); 
	functionMap.SetAt(_T("openUrlLink/"), &CTbMenuHandler::OpenUrlLinkFunction);
	functionMap.SetAt(_T("changeLogin/"), &CTbMenuHandler::ChangeLoginFunction);
	functionMap.SetAt(_T("newLogin/"), &CTbMenuHandler::NewLoginFunction);
	functionMap.SetAt(_T("activateViaInternet/"), &CTbMenuHandler::ActivateViaInternetFunction);
	functionMap.SetAt(_T("postLogin/"), &CTbMenuHandler::PostLoginFunction);
}

//----------------------------------------------------------------------------
CTbMenuHandler::~CTbMenuHandler(void)
{
	TB_OBJECT_LOCK(&m_Cache);

	POSITION pos = m_Cache.GetStartPosition();
	while (pos)
	{
		CString sKey;
		CachedFile* pCachedFile = NULL;
		m_Cache.GetNextAssoc(pos, sKey, pCachedFile);
		delete pCachedFile;
	}
}


//--------------------------------------------------------------------------------
BOOL CTbMenuHandler::ExecuteOnLoginThread(FUNCPTR func, const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (!pContext)
	{
		CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
		if (authToken.IsEmpty())
		{
			response.SetData(_TB("Cannot process request on login thread, authentication token is empty."));
			response.SetMimeType(_T("text/plain"));
			response.SetStatus(401);
			return TRUE;
		}

		CLoginContext* pContext = AfxGetLoginContext(authToken);
		if (pContext)
		{
			AfxInvokeThreadProcedure<CTbMenuHandler, const CString&, const CNameValueCollection&, CTBResponse&>
				(
					pContext->m_nThreadID,
					this,
					func,
					path,
					params,
					response
					);
			return TRUE;
		}
	}
	return FALSE;
}


//--------------------------------------------------------------------------------
void CTbMenuHandler::ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	// ulteriore controllo per tradurre la form di login e inizializzarla con la Preferred Language
	if (AfxGetCulture().IsEmpty())
		AfxGetThreadContext()->SetUICulture(AfxGetCommonClientObjects()->GetServerConnectionInfo()->m_sPreferredLanguage);

	FUNCPTR temp;
	if (functionMap.Lookup(path, temp))
	{
		(this->*(temp))(path, params, response);
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"LoadCustomTheme/"))
	{
		LoadCustomThemeFunction(path, params, response);
		return;
	}

	//una staticimage é un'immagine presa da file system
	if ((LPCTSTR)path == wcsstr(path, L"staticimage/"))
	{
		CString file = m_strStandard + SLASH_CHAR + (((LPCTSTR)path) + 12) /*salto staticimage/*/;
		if (ReadFileContent(file, response))
		{
			SetMimeType(path, response);
		}
		else
		{
			response.SetData(cwsprintf(_TB("Error reading file %s"), file));
			response.SetMimeType(_T("text/plain"));
		}

		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"staticThumbnail/"))
	{
		CString ns = params.GetValueByName(_T("ns"));

		CString file = AfxGetPathFinder()->GetMenuThumbnailsFolderPath() + _T("\\") + ns + _T(".jpg");
		if (ReadFileContent(file, response))
		{
			SetMimeType(file, response);
		}
		else
		{
			response.SetData(cwsprintf(_TB("Error reading file %s"), file));
			response.SetMimeType(_T("text/plain"));
		}
		return;
	}

	__super::ProcessRequest(path, params, response);
}

//--------------------------------------------------------------------------------
bool CTbMenuHandler::ExecutedOnLoginInfinity(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if ((LPCTSTR)path != wcsstr(path, szNeedLoginInfinityUrl))
		return false; //non ho necessità di girare nel login thread

	CString cryptedtoken = params.GetValueByName(_T("tk"));
	CString user = params.GetValueByName(_T("user"));
	CString company = params.GetValueByName(_T("company"));
	CString pwd = params.GetValueByName(_T("password"));
	bool bOverWrite1 = params.GetValueByName(_T("overwrite")) == _T("true");
	bool winNT1 = params.GetValueByName(_T("winNT")) == _T("true");
	bool relogin1 = params.GetValueByName(_T("relogin")) == _T("true");
	bool changeAutologinInfo1 = params.GetValueByName(_T("changeAutologinInfo")) == _T("true");
	CString saveAutologinInfo1 = params.GetValueByName(_T("saveAutologinInfo"));
	CString sMessage1;
	bool alreadyLogged1 = false, changePassword1 = false;
	CString sTokenMicroarea = params.GetValueByName(AUTH_TOKEN_PARAM);
	CJSonResponse jsonResponse1;
	int res = 0;
	if (sTokenMicroarea.IsEmpty())
	{
		sTokenMicroarea = DoSSOLoginWeb(cryptedtoken, user, pwd, company, winNT1, bOverWrite1, relogin1, sMessage1, alreadyLogged1, changePassword1, changeAutologinInfo1, res, saveAutologinInfo1);
		response.SetCookie(AUTH_TOKEN_PARAM, sTokenMicroarea);
	}

	if (!sTokenMicroarea.IsEmpty())
	{
		CLoginContext* pContext = CTBRequestHandlerObj::GetLoginContext(sTokenMicroarea, TRUE);//forza la creazione del login context

																		 /*CString file = m_strRoot + L"newMenu.html";
																		 if (ReadFileContent(file, response))
																		 {
																		 SetMimeType(L"newMenu.html", response);
																		 return;
				
														 }*/


		if (!pContext || !pContext->IsValid())
		{
			response.SetData(_TB("Error processing request on login thread, thread communication error."));
			response.SetMimeType(_T("text/plain"));
			return false;
		}

		CNameValueCollection * pParams = (CNameValueCollection*)&params;
		pParams->Add(AUTH_TOKEN_PARAM, sTokenMicroarea);

		CString newPath = (LPCTSTR)path + nNeedLoginThreadUrlLen + 2; //TODO LARA
		try
		{
			AfxInvokeThreadProcedure
				<
				CTbMenuHandler,
				const CString&,
				const CNameValueCollection&,
				CTBResponse&
				>
				(
					pContext->m_nThreadID,
					this,
					&CTbMenuHandler::ProcessRequest,
					newPath,
					params,
					response
					);
		}
		catch (CApplicationErrorException*)
		{
			response.SetData(_TB("Error processing request on login thread, thread communication error."));
			response.SetMimeType(_T("text/plain"));
		}

		return false;
	}

	else
	{
		if (!cryptedtoken.IsEmpty()) {
			CString file = m_strRoot + L"loginhost.html";
			if (ReadFileContent(file, response))
			{
				SetMimeType(L"loginhost.html", response);
				return true;
			}
		}
		else {

			jsonResponse1.SetError();
			if (alreadyLogged1)
				jsonResponse1.WriteBool(L"alreadyLogged", true);
			if (changePassword1)
				jsonResponse1.WriteBool(L"changePassword", true);
			if (changeAutologinInfo1)
				jsonResponse1.WriteBool(L"changeAutologinInfo", true);

			if (!sMessage1.IsEmpty())
				jsonResponse1.WriteJsonFragment(L"messages", sMessage1);
		}

	}
	response.SetData(jsonResponse1);
	return true;
}

//--------------------------------------------------------------------------------
CString CTbMenuHandler::GetAndFormatCustomizationContextApplicationAndModule()
{
	CString appName, modName;
	GetEasyBuilderAppAndModule(appName, modName);
	if (appName.IsEmpty() || modName.IsEmpty())
		return NULL;

	CString res(appName + _T(";") + modName);
	return res;
}

//--------------------------------------------------------------------------------
CString CTbMenuHandler::GetDynamicCssTheme()
{
	CString strCssFullPath = AfxGetThemeManager()->GetThemeCssFullPath();
	if (!ExistFile(strCssFullPath))
		return _T("");

	CLineFile file;
	if (!file.Open(strCssFullPath, CFile::modeRead | CFile::typeText))
		return _T("");


	CString cssResult = file.ReadToEnd();
	file.Close();

	return cssResult;
}

//--------------------------------------------------------------------------------
CString CTbMenuHandler::GetDocumentType(CBaseDocument* pDoc)
{
	if (pDoc->IsABatchDocument())
		return _T("Batch");

	return pDoc->GetNamespace().GetTypeString();
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::FindHwndNotInOpenDocuments(CArray<HWND>& arNotInOpenDocuments)
{
	CBaseDocument* pDoc = NULL;
	LongArray arDocs;
	AfxGetWebServiceStateObjectsHandles(arDocs, RUNTIME_CLASS(CBaseDocument));

	CArray<HWND>& ar = AfxGetLoginContext()->GetAllHwndArray();

	for (int i = 0; i <= ar.GetUpperBound(); i++)
	{
		bool found = false;
		HWND hWnd = ar.GetAt(i);
		for (int j = 0; j <= arDocs.GetUpperBound(); j++)
		{
			pDoc = (CBaseDocument*)arDocs[j];
			if (!pDoc->CanShowInOpenDocuments())
				continue;

			if (pDoc->GetFrameHandle() == hWnd)
			{
				found = true;
				continue;
			}
		}

		if (!found)
			arNotInOpenDocuments.Add(hWnd);
	}
}



//--------------------------------------------------------------------------------
CString CTbMenuHandler::GetJsonThemesList()
{
	CStringArray themes;

	AfxGetPathFinder()->GetAvailableThemesFullNames(themes);

	CString strFullPath = AfxGetThemeManager()->GetThemeFullPath();

	CJsonSerializer jsonSerializer;

	jsonSerializer.OpenObject(_T("Themes"));
	jsonSerializer.OpenArray(_T("Theme"));

	for (int i = 0; i <= themes.GetUpperBound(); i++)
	{
		jsonSerializer.OpenObject(i);

		jsonSerializer.WriteString(_T("path"), themes[i]);

		if (strFullPath.CompareNoCase(themes[i]) == 0)
			jsonSerializer.WriteBool(_T("isFavoriteTheme"), true);

		jsonSerializer.CloseObject();
	}
	jsonSerializer.CloseArray();
	jsonSerializer.CloseObject();

	return jsonSerializer.GetJson();
}

//--------------------------------------------------------------------------------
CString CTbMenuHandler::GetJsonOpenDocuments()
{
	LongArray arDocs;
	AfxGetWebServiceStateObjectsHandles(arDocs, RUNTIME_CLASS(CBaseDocument));

	CBaseDocument* pDoc = NULL;

	CJsonSerializer jsonSerializer;

	jsonSerializer.OpenObject(_T("OpenDocuments"));
	jsonSerializer.OpenArray(_T("Document"));

	int arrayIndex = 0;
	for (int i = 0; i <= arDocs.GetUpperBound(); i++)
	{
		pDoc = (CBaseDocument*)arDocs[i];
		if (!pDoc->CanShowInOpenDocuments())
			continue;

		jsonSerializer.OpenObject(arrayIndex);

		jsonSerializer.WriteString(_T("title"), pDoc->GetTitle());
		jsonSerializer.WriteString(_T("target"), pDoc->GetNamespace().ToUnparsedString());
		jsonSerializer.WriteString(_T("handle"), cwsprintf(_T("{0-%d}"), pDoc->GetFrameHandle()));
		jsonSerializer.WriteString(_T("objectType"), GetDocumentType(pDoc));
		jsonSerializer.WriteString(_T("defaultDescription"), pDoc->GetDefaultMenuDescription());

		jsonSerializer.WriteString(_T("creationTime"), cwsprintf(_T("{0-%d}"), pDoc->GetCreationTime()));

		if (pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			CAbstractFormDoc* tempDoc = (CAbstractFormDoc*)pDoc;
			if (tempDoc && tempDoc->GetMaster() && tempDoc->GetMaster()->GetRecord())
				jsonSerializer.WriteString(_T("record"), tempDoc->GetMaster()->GetRecord()->GetPrimaryKeyNameValue());
		}

		jsonSerializer.CloseObject();
		arrayIndex++;
	}

	CArray<HWND> arNotInOpenDocuments;
	FindHwndNotInOpenDocuments(arNotInOpenDocuments);
	for (int i = 0; i <= arNotInOpenDocuments.GetUpperBound(); i++)
	{
		HWND tempHwnd = arNotInOpenDocuments.GetAt(i);
		BOOL isValid = ::IsWindow(tempHwnd);
		if (!isValid)
			continue;

		CWnd* pWnd = CWnd::FromHandle(tempHwnd);
		if (!pWnd)
			continue;

		int bShow = pWnd->SendMessage(UM_SHOW_IN_OPEN_DOCUMENTS, NULL, NULL);
		if (!bShow)
			continue;

		CString str;
		pWnd->GetWindowText(str);

		jsonSerializer.OpenObject(arrayIndex);  //qua non è i, è l'ultimo slot disponibile
		jsonSerializer.WriteString(_T("title"), str);
		jsonSerializer.WriteString(_T("handle"), cwsprintf(_T("{0-%d}"), tempHwnd));
		jsonSerializer.WriteString(_T("target"), _T(""));
		jsonSerializer.WriteString(_T("objectType"), _T("Document"));
		jsonSerializer.WriteString(_T("defaultDescription"), _T(""));

		jsonSerializer.CloseObject();
		arrayIndex++;
	}

	jsonSerializer.CloseArray();
	jsonSerializer.CloseObject();
	return jsonSerializer.GetJson();
}

//--------------------------------------------------------------------------------
CString CTbMenuHandler::GetJsonCompaniesForUser(CString strUser)
{
	CStringArray companies;
	AfxGetLoginManager()->EnumCompanies(strUser, companies);

	CJsonSerializer jsonSerializer;

	jsonSerializer.OpenObject(_T("Companies"));
	jsonSerializer.OpenArray(_T("Company"));

	for (int i = 0; i <= companies.GetUpperBound(); i++)
	{
		jsonSerializer.OpenObject(i);

		jsonSerializer.WriteString(_T("name"), companies[i]);

		jsonSerializer.CloseObject();
	}
	jsonSerializer.CloseArray();
	jsonSerializer.CloseObject();

	return jsonSerializer.GetJson();
}

//--------------------------------------------------------------------------------
CString CTbMenuHandler::GetJsonWorkerInfos(CString authToken)
{
	CLoginContext* pContext = CTBRequestHandlerObj::GetLoginContext(authToken, FALSE);
	
	if (!pContext || !pContext->IsValid())
		return _T("");

	CString workerName, userName, workerLastName, company;
	CWorkersTableObj* workersTable = (CWorkersTableObj*)pContext->GetWorkersTable();
	CString imagePath;
	if (workersTable)
	{
		CWorker* worker = workersTable->GetWorker(pContext->GetWorkerId());
		if (worker)
		{
			workerName = worker->GetName();
			workerLastName = worker->GetLastName();
			imagePath = (worker && !worker->GetImagePath().IsEmpty()) ? worker->GetImagePath() : _T("");
		}
	}

	company = AfxGetLoginInfos()->m_strCompanyName;
	userName = AfxGetLoginInfos()->m_strUserName;

	CString encodedImage = _T("");
	CString file = _T("");

	file = AfxGetPathFinder()->GetFileNameFromNamespace(imagePath, AfxGetLoginInfos()->m_strUserName);
	file.Replace(m_strStandard, _T(""));

	CJsonSerializer jsonSerializer;

	jsonSerializer.OpenObject(_T("UserInfos"));

	if (!workerName.IsEmpty())
		jsonSerializer.WriteString(_T("workerName"), workerName);

	if (!userName.IsEmpty())
		jsonSerializer.WriteString(_T("userName"), userName);
	if (!workerLastName.IsEmpty())
		jsonSerializer.WriteString(_T("workerLastName"), workerLastName);
	if (!file.IsEmpty())
		jsonSerializer.WriteString(_T("image_file"), file);
	if (!company.IsEmpty())
		jsonSerializer.WriteString(_T("company"), company);

	jsonSerializer.CloseObject();

	return jsonSerializer.GetJson();
}

//--------------------------------------------------------------------------------
CString CTbMenuHandler::GetJsonLocalizedContent(const CString& authToken)
{
	//CLoginContext* pContext = AfxGetLoginContext(authToken);
	//if (!pContext)
	//	return _T("");

	//TB://Document.ERP.CustomersSuppliers.Documents.Customers?custsupptype:00310000%3bCustSupp:1%3b
	CJsonSerializer jsonSerializer;

	jsonSerializer.OpenObject(_T("LocalizedElements"));
	jsonSerializer.OpenArray(_T("LocalizedElement"));

	int elements = 0;
	ADD_LOCALIZED_CONTENT(_T("PwdChanged"), _TB("Password changed"));
	ADD_LOCALIZED_CONTENT(_T("ErrUserAlreadyLogged"), _TB("The user is already connected to the system. Do you want to close previous working session and open new one?"));
	ADD_LOCALIZED_CONTENT(_T("changeAutologinInfo"), _TB("Exists information of another user, do you want to replace them with your own? "));
	ADD_LOCALIZED_CONTENT(_T("passwordError"), _TB("Please make sure you entered the password correctly."));
	ADD_LOCALIZED_CONTENT(_T("FullMenuTitle"), _TB("Full Menu"));
	ADD_LOCALIZED_CONTENT(_T("EnvironmentMenuTitle"), _TB("Environment Menu"));
	ADD_LOCALIZED_CONTENT(_T("WorkerLabel"), _TB("Worker"));
	ADD_LOCALIZED_CONTENT(_T("CompanyLabel"), _TB("Company"));
	ADD_LOCALIZED_CONTENT(_T("OptionsLabel"), _TB("Options"));
	ADD_LOCALIZED_CONTENT(_T("ShowAllLabel"), _TB("Show all"));
	ADD_LOCALIZED_CONTENT(_T("ChangeLogin"), _TB("Change Login"));
	ADD_LOCALIZED_CONTENT(_T("GenericOptionsLabel"), _TB("Generic options"));
	ADD_LOCALIZED_CONTENT(_T("ShowHistoryLabel"), _TB("Show history"));
	ADD_LOCALIZED_CONTENT(_T("ShowMostUsedLabel"), _TB("Show most used"));
	ADD_LOCALIZED_CONTENT(_T("ShowThumbnailsLabel"), _TB("Show thumbnails"));
	ADD_LOCALIZED_CONTENT(_T("CloseLabel"), _TB("Close"));
	ADD_LOCALIZED_CONTENT(_T("ReleaseInfoLabel"), _TB("Release Info"));
	ADD_LOCALIZED_CONTENT(_T("FavoritesLabel"), _TB("Favorites"));
	ADD_LOCALIZED_CONTENT(_T("HistoryLabel"), _TB("History"));
	ADD_LOCALIZED_CONTENT(_T("MaximumNumberOfElements"), _TB("Maximum number of elements"));
	ADD_LOCALIZED_CONTENT(_T("MaximumNumberOfRecords"), _TB("Maximum number of records"));
	ADD_LOCALIZED_CONTENT(_T("MostUsedLabel"), _TB("Recently used"));
	ADD_LOCALIZED_CONTENT(_T("AdvancedSearchLabel"), _TB("Advanced Search"));
	ADD_LOCALIZED_CONTENT(_T("SearchInDocuments"), _TB("Search in documents"));
	ADD_LOCALIZED_CONTENT(_T("SearchInReports"), _TB("Search in reports"));
	ADD_LOCALIZED_CONTENT(_T("SearchInBatches"), _TB("Search in batches"));
	ADD_LOCALIZED_CONTENT(_T("LoginLabel"), _TB("Login"));
	ADD_LOCALIZED_CONTENT(_T("ProductLicencedTo"), _TB("Product licenced to"));
	ADD_LOCALIZED_CONTENT(_T("ProductVersion"), _TB("Product version"));
	ADD_LOCALIZED_CONTENT(_T("ShowFullMenu"), _TB("Show Full Menu"));
	ADD_LOCALIZED_CONTENT(_T("ChangeApplicationDate"), _TB("Change operation date"));
	ADD_LOCALIZED_CONTENT(_T("CurrentApplicationDate"), _TB("Operation date"));
	ADD_LOCALIZED_CONTENT(_T("Open"), _TB("Open"));
	ADD_LOCALIZED_CONTENT(_T("UnpinElement"), _TB("Unpin element"));
	ADD_LOCALIZED_CONTENT(_T("PinElement"), _TB("Pin element"));
	ADD_LOCALIZED_CONTENT(_T("RemoveFromFavorites"), _TB("Remove from favorites"));
	ADD_LOCALIZED_CONTENT(_T("AddToFavorites"), _TB("Add to favorites"));
	ADD_LOCALIZED_CONTENT(_T("SetAsStartupMenu"), _TB("Set as startup menu"));
	ADD_LOCALIZED_CONTENT(_T("StartupElement"), _TB("Startup element"));
	ADD_LOCALIZED_CONTENT(_T("MenuOptions"), _TB("Menu options"));
	ADD_LOCALIZED_CONTENT(_T("Gadgets"), _TB("Gadgets"));
	ADD_LOCALIZED_CONTENT(_T("ShowSearchBox"), _TB("Show search box"));
	ADD_LOCALIZED_CONTENT(_T("ShowFilterBox"), _TB("Show filter box"));
	ADD_LOCALIZED_CONTENT(_T("NewFavoriteElement"), _TB("New favorite element"));
	ADD_LOCALIZED_CONTENT(_T("SearchOptions"), _TB("Search options"));
	ADD_LOCALIZED_CONTENT(_T("DeleteAllElements"), _TB("Clear list"));
	ADD_LOCALIZED_CONTENT(_T("DeleteSelectedElements"), _TB("Delete selected elements"));
	ADD_LOCALIZED_CONTENT(_T("ExpandAll"), _TB("Expand all"));
	ADD_LOCALIZED_CONTENT(_T("CollapseAll"), _TB("Collapse all"));
	ADD_LOCALIZED_CONTENT(_T("SearchLabel"), _TB("Search"));
	ADD_LOCALIZED_CONTENT(_T("FilterLabel"), _TB("Filter"));
	ADD_LOCALIZED_CONTENT(_T("ProductInfo"), _TB("Product info"));
	ADD_LOCALIZED_CONTENT(_T("ShowNotifications"), _TB("Show Notifications"));
	ADD_LOCALIZED_CONTENT(_T("ShowHideLeftGroups"), _TB("Show/hide groups"));
	ADD_LOCALIZED_CONTENT(_T("NrMaxItemsSearch"), _TB("Max number of items in search"));
	ADD_LOCALIZED_CONTENT(_T("StartsWith"), _TB("Starts with"));
	ADD_LOCALIZED_CONTENT(_T("ShowAsTile"), _TB("Show tiles"));
	ADD_LOCALIZED_CONTENT(_T("ShowAsTree"), _TB("Show tree"));
	ADD_LOCALIZED_CONTENT(_T("ChangePassword"), _TB("Change Password"));
	ADD_LOCALIZED_CONTENT(_T("PwdMatchError"), _TB("Password doesn't match!"));
	ADD_LOCALIZED_CONTENT(_T("Password"), _TB("Password"));
	ADD_LOCALIZED_CONTENT(_T("Username"), _TB("Username"));
	ADD_LOCALIZED_CONTENT(_T("RememberMe"), _TB("Remember Me"));
	ADD_LOCALIZED_CONTENT(_T("ActivateViaInternet"), _TB("Activate via Internet"));
	ADD_LOCALIZED_CONTENT(_T("ActivateViaSMS"), _TB("Activate via SMS"));
	ADD_LOCALIZED_CONTENT(_T("Activation"), _TB("Activation"));
	ADD_LOCALIZED_CONTENT(_T("Login"), _TB("Login"));
	ADD_LOCALIZED_CONTENT(_T("NewLogin"), _TB("Open New Login"));
	ADD_LOCALIZED_CONTENT(_T("NewPassword"), _TB("New Password"));
	ADD_LOCALIZED_CONTENT(_T("ConfirmNewPassword"), _TB("Confirm New Password"));
	ADD_LOCALIZED_CONTENT(_T("ClearCachedData"), _TB("Clear cached data"));
	ADD_LOCALIZED_CONTENT(_T("WindowsAuthentication"), _TB("Windows Authentication"));
	ADD_LOCALIZED_CONTENT(_T("OpenDocuments"), _TB("Open documents"));
	ADD_LOCALIZED_CONTENT(_T("ChangeTheme"), _TB("Change theme"));
	ADD_LOCALIZED_CONTENT(_T("HideTile"), _TB("Hide element"));
	ADD_LOCALIZED_CONTENT(_T("HiddenTiles"), _TB("Hidden elements"));
	ADD_LOCALIZED_CONTENT(_T("RestoreTile"), _TB("Restore element"));
	ADD_LOCALIZED_CONTENT(_T("ApplicationLabel"), _TB("Application"));
	ADD_LOCALIZED_CONTENT(_T("MenuLabel"), _TB("Menu"));
	ADD_LOCALIZED_CONTENT(_T("ModuleLabel"), _TB("Module"));
	ADD_LOCALIZED_CONTENT(_T("viewOldMsg"), _TB("View old messages"));
	ADD_LOCALIZED_CONTENT(_T("ViewOptions"), _TB("View options"));
	ADD_LOCALIZED_CONTENT(_T("NoCompany"), _TB("This user is not associated to any company."));
	ADD_LOCALIZED_CONTENT(_T("OtherElements"), _TB("Other elements"));
	ADD_LOCALIZED_CONTENT(_T("InstallationVersion"), _TB("Installation Version"));
	ADD_LOCALIZED_CONTENT(_T("Provider"), _TB("Provider"));
	ADD_LOCALIZED_CONTENT(_T("Edition"), _TB("Edition"));
	ADD_LOCALIZED_CONTENT(_T("InstallationName"), _TB("Installation Name"));
	ADD_LOCALIZED_CONTENT(_T("ActivationState"), _TB("Activation State"));
	ADD_LOCALIZED_CONTENT(_T("OK"), _TB("OK"));
	ADD_LOCALIZED_CONTENT(_T("Cancel"), _TB("Cancel"));
	ADD_LOCALIZED_CONTENT(_T("ChangeOpDate"), _TB("Change Operation Date"));
	ADD_LOCALIZED_CONTENT(_T("GotoProducerSite"), _TB("Producer Site"));
	ADD_LOCALIZED_CONTENT(_T("ViewProductInfo"), _TB("Product Info"));
	ADD_LOCALIZED_CONTENT(_T("OnlineCourse"), _TB("Online Course"));
	ADD_LOCALIZED_CONTENT(_T("ConnectionInfo"), _TB("Connection Info"));
	ADD_LOCALIZED_CONTENT(_T("FreeSpace"), _TB("Free space"));
	ADD_LOCALIZED_CONTENT(_T("UsedSpace"), _TB("Used space"));
	ADD_LOCALIZED_CONTENT(_T("DBServer"), _TB("Server"));
	ADD_LOCALIZED_CONTENT(_T("DBUser"), _TB("User"));
	ADD_LOCALIZED_CONTENT(_T("DBName"), _TB("Database"));
	ADD_LOCALIZED_CONTENT(_T("AuditingEnabled"), _TB("Auditing enabled"));
	ADD_LOCALIZED_CONTENT(_T("SecurityEnabled"), _TB("Security enabled"));
	ADD_LOCALIZED_CONTENT(_T("WebApplicationServer"), _TB("Web Application Server"));
	ADD_LOCALIZED_CONTENT(_T("FileApplicationServer"), _TB("File Application Server"));
	ADD_LOCALIZED_CONTENT(_T("Instance"), _TB("Instance"));
	ADD_LOCALIZED_CONTENT(_T("EasyStudioDeveloper"), _TB("Easy Studio Developer"));
	ADD_LOCALIZED_CONTENT(_T("Administrator"), _TB("Administrator"));
	ADD_LOCALIZED_CONTENT(_T("NA"), _TB("Not available"));
	ADD_LOCALIZED_CONTENT(_T("Yes"), _TB("Yes"));
	ADD_LOCALIZED_CONTENT(_T("No"), _TB("No"));
	ADD_LOCALIZED_CONTENT(_T("Error"), _TB("Error"));
	ADD_LOCALIZED_CONTENT(_T("Today"), _TB("Today"));
	ADD_LOCALIZED_CONTENT(_T("Messages"), _TB("Messages"));
	ADD_LOCALIZED_CONTENT(_T("LoginExpired"), _TB("Working session expired, please login again."));

	ADD_LOCALIZED_CONTENT(_T("EasyStudioOptions"), _TB("EasyStudio options"));
	ADD_LOCALIZED_CONTENT(_T("OpenEasyStudioNewCustomization"), _TB("New EasyStudio customization"));
	ADD_LOCALIZED_CONTENT(_T("CloneEasyStudioDocument"), _TB("Clone as a New Document"));
	
	ADD_LOCALIZED_CONTENT(_T("OpenEasyStudioWithCustomization"), _TB("Edit EasyStudio Customization"));
	ADD_LOCALIZED_CONTENT(_T("CurrentApplication"), _TB("Current application"));
	ADD_LOCALIZED_CONTENT(_T("CurrentModule"), _TB("Current module"));
	ADD_LOCALIZED_CONTENT(_T("ChangeCustomizationContext"), _TB("Change customization context"));
	ADD_LOCALIZED_CONTENT(_T("CloseCustomizationContext"), _TB("Close customization context"));
	ADD_LOCALIZED_CONTENT(_T("OpenDefaultContext"), _TB("Open Default Context"));
	ADD_LOCALIZED_CONTENT(_T("NotDesignableDocument"), _TB("This document/batch is not designable"));
	ADD_LOCALIZED_CONTENT(_T("CustomizationNotActive"), _TB("Customization valid only for context"));

	jsonSerializer.CloseArray();
	jsonSerializer.CloseObject();

	return jsonSerializer.GetJson();
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetLoginCompaniesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString user = params.GetValueByName(_T("user"));
	response.SetData(GetJsonCompaniesForUser(user));
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::DossologoffFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString cryptedtoken = params.GetValueByName(_T("tk"));
	DoSSOLogOff(cryptedtoken);
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::ChangePasswordFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString spassword = params.GetValueByName(_T("password"));

	//chiama core - ChangePassword
	CString res = ChangePassword(spassword);

	response.SetData(res);
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::DossologinFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString cryptedtoken = params.GetValueByName(_T("tk"));
	CString tblink = params.GetValueByName(_T("tblink"));
	CString user = params.GetValueByName(_T("user"));
	CString company = params.GetValueByName(_T("company"));
	CString pwd = params.GetValueByName(_T("password"));
	bool bOverWrite = params.GetValueByName(_T("overwrite")) == _T("true");
	bool winNT = params.GetValueByName(_T("winNT")) == _T("true");
	bool relogin = params.GetValueByName(_T("relogin")) == _T("true");
	bool changeAutologinInfo = params.GetValueByName(_T("changeAutologinInfo")) == _T("true");
	CString saveAutologinInfo = params.GetValueByName(_T("saveAutologinInfo"));
	CString sMessage;
	bool alreadyLogged = false;
	bool changePassword = false;
	int res = 0;
	CString sToken = DoSSOLoginWeb(cryptedtoken, user, pwd, company, winNT, bOverWrite, relogin, sMessage, alreadyLogged, changePassword, changeAutologinInfo, res, saveAutologinInfo);

	CJSonResponse jsonResponse;
	if (!sToken.IsEmpty())
	{
		CLoginContext* pContext = CTBRequestHandlerObj::GetLoginContext(sToken, TRUE);//forza la creazione del login context

		if (pContext && pContext->IsValid())
		{
			CString file;
			response.SetCookie(AUTH_TOKEN_PARAM, sToken);

			BOOL bParsedMenu = AfxInvokeThreadFunction<BOOL, CTbMenuHandler, CLoginContext*>(pContext->m_nThreadID, this, &CTbMenuHandler::MenuInfinity, pContext);
			if (bParsedMenu)
				file = m_strRoot + L"newMenu.html";
			else
				file = m_strRoot + L"noMenu.html";

			if (ReadFileContent(file, response))
			{
				SetMimeType(L"newMenu.html", response);
				return;
			}
		}
	}

	else
	{
		if (!cryptedtoken.IsEmpty())
		{
			if (!sMessage.IsEmpty() && res != 0) {
				jsonResponse.WriteJsonFragment(L"messages", sMessage);
				//if (res== 58)//(int)LoginReturnCodes.ImagoUserAlreadyAssociated = 58,
				//{
				response.SetData(jsonResponse);
				return;
			}
			//}
			//if (res == 13)
			//{
			//	jsonResponse.SetError();
			//	jsonResponse.WriteBool(L"passworderror", true);
			//	response.SetData(jsonResponse.GetJson());
			//	response.SetMimeType(L"application/json");
			//	return;
			//}
			else {
				CString file = m_strRoot + L"loginhost.html";
				if (ReadFileContent(file, response))
				{
					SetMimeType(L"loginhost.html", response);
					return;
				}
			}
		}
		else {
			jsonResponse.SetError();
			if (alreadyLogged)
				jsonResponse.WriteBool(L"alreadyLogged", true);
			if (changePassword)
				jsonResponse.WriteBool(L"changePassword", true);
			if (changeAutologinInfo)
				jsonResponse.WriteBool(L"changeAutologinInfo", true);

			if (!sMessage.IsEmpty())
				jsonResponse.WriteJsonFragment(L"messages", sMessage);
		}

	}
	response.SetData(jsonResponse);
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::CanChangeLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::CanChangeLoginFunction, path, params, response))
		return;

	CJSonResponse jsonResponse;
	CLoginThread* pThread = AfxGetLoginThread();
	if (pThread && pThread->GetOpenDocuments() > 0)
	{
		jsonResponse.SetError();
		jsonResponse.SetMessage(_TB("It is not possible to change login at this point.\r\nPlease, check whether there is any opened document in the application framework."));

	}
	else
	{
		jsonResponse.SetOK();
	}

	response.SetData(jsonResponse);
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::SetRememberMeFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString  checkedVal = params.GetValueByName(_T("checked"));
	SetRememberMe(checkedVal);
	response.SetMimeType(L"None");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetRememberMeFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	response.SetData(GetRememberMe());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::IsAutoLoginableFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	response.SetData(IsAutoLoginable());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetLoginInitInformationFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	response.SetData(GetLoginInitInformation());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetLoginInitImageFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	response.SetData(GetLoginInitImage());
	response.SetMimeType(L"text/plain");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetLoginBackgroundImageFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	response.SetData(GetLoginBackgroundImage());
	response.SetMimeType(L"text/plain");
}


//--------------------------------------------------------------------------------
void CTbMenuHandler::StaticImageFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString file = m_strStandard + SLASH_CHAR + (((LPCTSTR)path) + 12) /*salto staticimage/*/;
	if (ReadFileContent(file, response))
	{
		SetMimeType(path, response);
	}
	else
	{
		response.SetData(cwsprintf(_TB("Error reading file %s"), file));
		response.SetMimeType(_T("text/plain"));
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::StaticThumbnailFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString ns = params.GetValueByName(_T("ns"));

	CString file = AfxGetPathFinder()->GetMenuThumbnailsFolderPath() + _T("\\") + ns + _T(".jpg");
	if (ReadFileContent(file, response))
	{
		SetMimeType(file, response);
	}
	else
	{
		response.SetData(cwsprintf(_TB("Error reading file %s"), file));
		response.SetMimeType(_T("text/plain"));
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::LoadCustomThemeFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::LoadCustomThemeFunction, path, params, response))
		return;

	//La logica di questo metodo è:
	//nell'html aggiungo    <link href="needLoginThread/LoadCustomTheme/" rel="stylesheet">
	//questo path mi attiva la ricerca di un file chiamato  [nometema].override.css, se lo trova lo carica
	//se questo css carica altri file (ad esempio immagini o altro), vengono caricati in base al path tornato dalla cartella in cui è presente il file di override

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		CString requestedElement = ((LPCTSTR)path + 16);  //da  LoadCustomTheme/ in avanti
		if (requestedElement.IsEmpty())
		{
			//carica eventuali temi overridati nella cartella di erp (o altri verticali)
			CString filePath = AfxGetThemeManager()->GetThemeCssFullPath();
			CString cssName = PathFindFileName(filePath);
			cssName.Replace(_T(".css"), _T(".override.css"));

			CString file = AfxGetPathFinder()->GetThemeElementFullName(cssName);// (((LPCTSTR)path) + 16)) /*salto LoadCustomTheme/*/;
			if (!ReadFileContent(file, response))
				response.SetData(_T(""));
		}
		else
		{
			//tutti gli altri file che il css overridato si tira dietro (immagini, e altri css) passano di qua
			CString file = AfxGetPathFinder()->GetThemeElementFullName(requestedElement);
			if (ReadFileContent(file, response))
			{
				SetMimeType(path, response);
				return;
			}
		}
	}
	response.SetMimeType(_T("text/css"));
	return;
}


//--------------------------------------------------------------------------------
void CTbMenuHandler::GetLocalizedElementsFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString needLoginThread = params.GetValueByName(_T("needLoginThread"));
	BOOL bNeedLoginThread = needLoginThread.CompareNoCase(_T("true")) == 0;
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);

	//è l'unica eccezione alla logica di ExecuteOnLoginThread, questo metodo viene chiamato sia quando non siamo loginati, sia quando lo siamo
	if (bNeedLoginThread && ExecuteOnLoginThread(&CTbMenuHandler::GetLocalizedElementsFunction, path, params, response))
		return;

	response.SetData(GetJsonLocalizedContent(authToken));
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::LoadDynamicThemeFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::LoadDynamicThemeFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetData(GetDynamicCssTheme());
		response.SetMimeType(L"text/css");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetMenuElementsFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetMenuElementsFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetData(LoadMenuWithFavoritesAsJson(pContext));
		response.SetMimeType(L"application/json");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetThemedSettingsFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetThemedSettingsFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetData(GetJsonMenuSettings());
		response.SetMimeType(L"application/json");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::AddToMostUsedFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::AddToMostUsedFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		CString target = params.GetValueByName(_T("target"));
		CString objectType = params.GetValueByName(_T("objectType"));
		CString objectName = params.GetValueByName(_T("objectName"));
		AddToMostUsed(target, objectType, objectName, pContext);
		response.SetMimeType(L"None");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::RemoveFromMostUsedFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::RemoveFromMostUsedFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		CString target = params.GetValueByName(_T("target"));
		CString objectType = params.GetValueByName(_T("objectType"));
		CString objectName = params.GetValueByName(_T("objectName"));

		RemoveFromMostUsed(target, objectType, objectName, pContext);
		response.SetMimeType(L"None");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::UpdateMostUsedShowNrFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::UpdateMostUsedShowNrFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
		CString nrElement = params.GetValueByName(_T("nr"));
		response.SetMimeType(L"None");

		UpdateMostUsedShowNrElements(nrElement, pContext);
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetCultureFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetCultureFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		CString culture = AfxGetCultureInfo()->GetUICulture();
		if (culture.IsEmpty())
			culture = _T("en");

		response.SetData(culture);
		response.SetMimeType(L"application/json");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetMostUsedShowNrFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetMostUsedShowNrFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
		response.SetData(GetMostUsedShowNrElements(pContext));
		response.SetMimeType(L"application/json");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetUserInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetUserInfoFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{

		CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
		response.SetData(GetJsonWorkerInfos(authToken));
	}
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetBrandInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetBrandInfoFunction, path, params, response))
		return;

	response.SetData(LoadBrandInfoAsJson());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::UnFavoriteObjectFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::UnFavoriteObjectFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetMimeType(L"None");
		CString target = params.GetValueByName(_T("target"));
		CString objectType = params.GetValueByName(_T("objectType"));
		CString objectName = params.GetValueByName(_T("objectName"));
		RemoveFromFavorites(target, objectType, objectName, pContext);
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::FavoriteObjectFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::FavoriteObjectFunction, path, params, response))
		return;
	
	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetMimeType(L"None");
		CString target = params.GetValueByName(_T("target"));
		CString objectType = params.GetValueByName(_T("objectType"));
		CString objectName = params.GetValueByName(_T("objectName"));
		AddToFavorites(target, objectType, objectName, pContext);
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetNewMessagesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetNewMessagesFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetData(GetNewMessages(pContext));
		response.SetMimeType(L"application/json");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::UpdateFavoritesPositionFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::UpdateFavoritesPositionFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		CString target1 = params.GetValueByName(_T("target1"));
		CString objectType1 = params.GetValueByName(_T("objectType1"));
		CString objectName1 = params.GetValueByName(_T("objectName1"));
		CString target2 = params.GetValueByName(_T("target2"));
		CString objectType2 = params.GetValueByName(_T("objectType2"));
		CString objectName2 = params.GetValueByName(_T("objectName2"));
		UpdateFavoritesPosition(target1, objectType1, objectName1, target2, objectType2, objectName2, pContext);
		response.SetMimeType(L"None");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetPreferencesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetPreferencesFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetData(GetPreferencesAsJson(pContext));
		response.SetMimeType(L"application/json");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::SetPreferenceFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::SetPreferenceFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		CString preferenceName = params.GetValueByName(_T("name"));
		CString preferenceValue = params.GetValueByName(_T("value"));

		SetPreference(preferenceName, preferenceValue, pContext);

		//response.SetMimeType(L"None");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::ClearAllMostUsedFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::ClearAllMostUsedFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		ClearMostUsed(pContext);
		response.SetMimeType(L"None");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::ChangeApplicationDateFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::ChangeApplicationDateFunction, path, params, response))
		return;

	response.SetMimeType(L"application/json");
	CJSonResponse jsonResponse;
	if (AfxGetLoginThread()->GetOpenDocuments() > 0)
	{
		jsonResponse.SetError();
		jsonResponse.SetMessage(_TB("It is not possible to change the operation date at this point.\r\nPlease, check whether there is any opened document in the application framework."));
		response.SetData(jsonResponse.GetJson());
		return;
	}

	CString day = params.GetValueByName(_T("day"));
	CString month = params.GetValueByName(_T("month"));
	CString year = params.GetValueByName(_T("year"));

	int d = _ttoi(day);
	int m = _ttoi(month);
	int y = _ttoi(year);

	DataDate date(d, m, y);
	AfxSetApplicationDate(date);
	//ChangeOperationsDate();
	jsonResponse.SetOK();
	response.SetData(jsonResponse.GetJson());
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetApplicationDateFormatFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetApplicationDateFormatFunction, path, params, response))
		return;

	DataDate pDate = AfxGetApplicationDate();
	CDateFormatter* pFormatter = (CDateFormatter*)AfxGetFormatStyleTable()->GetFormatter(pDate.GetDataType(), NULL);
	if (pFormatter)
	{

		CDateFormatHelper::FormatTag  format = pFormatter->GetFormat();
		CString formatString = _T("");
		switch (format)
		{
		case	CDateFormatHelper::FormatTag::DATE_DMY:
			formatString = _T("dd/mm/yyyy");
			break;
		case	CDateFormatHelper::FormatTag::DATE_MDY:
			formatString = _T("mm/dd/yyyy");
			break;
		case	CDateFormatHelper::FormatTag::DATE_YMD:
			formatString = _T("yyyy/mm/dd");
			break;
		}

		response.SetData(formatString);
		response.SetMimeType(L"text/plain");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetApplicationDateFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetApplicationDateFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetData(AfxGetApplicationDate().FormatDataForXML());
		response.SetMimeType(L"text/plain");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::AddToHiddenTilesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::AddToHiddenTilesFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		CString appName = params.GetValueByName(_T("application"));
		CString groupName = params.GetValueByName(_T("group"));
		CString menuName = params.GetValueByName(_T("menu"));
		CString tileName = params.GetValueByName(_T("tile"));

		AddToHiddenTiles(appName, groupName, menuName, tileName, pContext);
		response.SetMimeType(L"None");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::RemoveFromHiddenTilesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::RemoveFromHiddenTilesFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		CString appName = params.GetValueByName(_T("application"));
		CString groupName = params.GetValueByName(_T("group"));
		CString menuName = params.GetValueByName(_T("menu"));
		CString tileName = params.GetValueByName(_T("tile"));

		RemoveFromHiddenTiles(appName, groupName, menuName, tileName, pContext);
		response.SetMimeType(L"None");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetThemesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetThemesFunction, path, params, response))
		return;

	response.SetData(GetJsonThemesList());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::ClearCachedDataFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::ClearCachedDataFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		ClearCachedData(pContext);
		response.SetMimeType(L"None");
	}
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::ChangeThemesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::ChangeThemesFunction, path, params, response))
		return;

	response.SetMimeType(L"application/json");
	CLoginContext* pContext = AfxGetLoginContext();
	CJSonResponse jsonResponse;
	if (pContext)
	{
		if (pContext->GetOpenDocuments() > 0)
		{
			jsonResponse.SetError();
			jsonResponse.SetMessage(_TB("It is not possible to change the theme at this point.\r\nPlease, check whether there is any opened document in the application framework."));
			response.SetData(jsonResponse.GetJson());
		}
		else
		{
			CString themePath = params.GetValueByName(_T("theme"));
			ChangeTheme(themePath, pContext);
			jsonResponse.SetOK();
		}
	}
	response.SetData(jsonResponse.GetJson());
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::GetProductInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetProductInfoFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetData(GetJsonProductInfo());
		response.SetMimeType(L"application/json");
	}
}
//--------------------------------------------------------------------------------
void CTbMenuHandler::GetCustomizationContextAppAndModuleFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetCustomizationContextAppAndModuleFunction, path, params, response))
		return;

	response.SetData(GetAndFormatCustomizationContextApplicationAndModule());
	response.SetMimeType(L"application/json");
}


//--------------------------------------------------------------------------------
void CTbMenuHandler::GetConnectionInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::GetConnectionInfoFunction, path, params, response))
		return;

	response.SetData(GetConnectionInformation());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::ActivateViaSMSFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::ActivateViaSMSFunction, path, params, response))
		return;

	PingViaSMS();
	response.SetMimeType(L"None");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::ProducerSiteFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::ProducerSiteFunction, path, params, response))
		return;

	OpenProducerSite();
	response.SetMimeType(L"None");
}

//----------------------------------------------------------------------------
void CTbMenuHandler::RunObject(const CNameValueCollection& params, CJSonResponse& aResponse, ObjectType type)
{
	CString sNamespace = params.GetValueByName(_T("ns"));
	CString sArguments = params.GetValueByName(_T("sKeyArgs"));
	
	if (!AfxGetLoginContext())
	{
		aResponse.SetMessage(_TB("Invalid login context"));
		aResponse.SetError();
		return;
	}
	CBaseDocument* pDoc = type == REPORT
		? (CBaseDocument*)AfxGetTbCmdManager()->RunWoormReport(sNamespace)
		: (type == DOCUMENT
			? AfxGetTbCmdManager()->RunDocument(sNamespace, szDefaultViewMode, FALSE, NULL, NULL)
			: NULL);

	if (!pDoc)
	{
		aResponse.SetError();
		return;
	}
	else if (type == DOCUMENT && !sArguments.IsEmpty())
	{
		//ASSERT_VALID(pDoc); non si puo' fare perchè fa un AssertValid anche della view
		CFunctionDescription fd;
		if (fd.ParseArguments(sArguments))
			pDoc->GoInBrowserMode(&fd);
	}
	aResponse.SetOK();	
}


//--------------------------------------------------------------------------------
void CTbMenuHandler::RunObjectFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::RunObjectFunction, path, params, response))
		return;

	CJSonResponse jsonResponse;
	CLoginContext* pContext = GetLoginContext(params);
	if (pContext)
	{
		CString sType = params.GetValueByName(_T("type"));
		ObjectType type = (ObjectType)_ttoi(sType);
		AfxInvokeThreadProcedure<CTbMenuHandler, const CNameValueCollection&, CJSonResponse&, ObjectType>
			(
				pContext->m_nThreadID,
				this,
				&CTbMenuHandler::RunObject,
				params,
				jsonResponse,
				type);

	}
	else
	{
		jsonResponse.SetError();
	}
	response.SetData(jsonResponse.GetJson());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::RunDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::RunDocumentFunction, path, params, response))
		return;

	CJSonResponse jsonResponse;
	RunObject(params, jsonResponse, DOCUMENT);
	response.SetData(jsonResponse.GetJson());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::RunReportFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::RunReportFunction, path, params, response))
		return;

	CJSonResponse jsonResponse;
	//RunObject(params, jsonResponse, REPORT);
	jsonResponse.SetOK();
	jsonResponse.WriteBool(_T("desktop"), false);
	response.SetData(jsonResponse.GetJson());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::RunFunctionFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	//if (ExecuteOnLoginThread(&CTbMenuHandler::RunFunctionFunction, path, params, response))
	//	return;

	response.SetMimeType(L"None");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::RunOfficeItemFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	//if (ExecuteOnLoginThread(&CTbMenuHandler::RunOfficeItemFunction, path, params, response))
	//	return;

	response.SetMimeType(L"None");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::CloseDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	//if (ExecuteOnLoginThread(&CTbMenuHandler::CloseDocumentFunction, path, params, response))
	//	return;

	response.SetMimeType(L"None");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::ShowProductInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::ShowProductInfoFunction, path, params, response))
		return;

	response.SetMimeType(L"None");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::OpenUrlLinkFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	//if (ExecuteOnLoginThread(&CTbMenuHandler::OpenUrlLinkFunction, path, params, response))
	//	return;

	response.SetMimeType(L"None");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::ChangeLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	//if (ExecuteOnLoginThread(&CTbMenuHandler::ChangeLoginFunction, path, params, response))
	//	return;

	response.SetMimeType(L"None");
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::NewLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	//if (ExecuteOnLoginThread(&CTbMenuHandler::NewLoginFunction, path, params, response))
	//	return;
	response.SetMimeType(L"None");
}


//--------------------------------------------------------------------------------
void CTbMenuHandler::ActivateViaInternetFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	//if (ExecuteOnLoginThread(&CTbMenuHandler::ActivateViaInternetFunction, path, params, response))
	//	return;

	response.SetMimeType(L"None");
}


//--------------------------------------------------------------------------------
void CTbMenuHandler::DoLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString sUser = params.GetValueByName(_T("user"));
	CString sCompany = params.GetValueByName(_T("company"));
	CString sPassword = params.GetValueByName(_T("password"));
	bool bOverWrite = params.GetValueByName(_T("overwrite")) == _T("true");
	//bool sRemember = params.GetValueByName(_T("rememberme")) == _T("true");
	bool winNT = params.GetValueByName(_T("winNT")) == _T("true");
	//bool ccd = params.GetValueByName(_T("ccd")) == _T("true");
	bool relogin = params.GetValueByName(_T("relogin")) == _T("true");
	bool changeAutologinInfo = params.GetValueByName(_T("changeAutologinInfo")) == _T("true");
	CString saveAutologinInfo = params.GetValueByName(_T("saveAutologinInfo"));
	CString sMessage;
	bool alreadyLogged = false, changePassword = false;
	CString sToken = DoLoginWeb(sUser, sPassword, sCompany, winNT, bOverWrite, relogin, sMessage, alreadyLogged, changePassword, changeAutologinInfo, saveAutologinInfo);

	CJSonResponse jsonResponse;
	if (!sToken.IsEmpty())
	{
		CLoginContext* pContext = CTBRequestHandlerObj::GetLoginContext(sToken, TRUE);//forza la creazione del login context
		if (pContext && pContext->IsValid())
		{
			response.SetCookie(AUTH_TOKEN_PARAM, sToken);
			jsonResponse.SetOK();
			jsonResponse.WriteString(L"menuPage", GetMenuPage());

		}
		else
		{
			jsonResponse.SetError();
			jsonResponse.WriteString(L"messages", _TB("Invalid LoginContext. Check if company database is up to date."));
		}
	}
	else
	{
		jsonResponse.SetError();
		if (alreadyLogged)
			jsonResponse.WriteBool(L"alreadyLogged", true);
		if (changePassword)
			jsonResponse.WriteBool(L"changePassword", true);
		if (changeAutologinInfo)
			jsonResponse.WriteBool(L"changeAutologinInfo", true);

		if (!sMessage.IsEmpty())
			jsonResponse.WriteJsonFragment(L"messages", sMessage);

	}
	response.SetData(jsonResponse);
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::PostLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	//if (ExecuteOnLoginThread(&CTbMenuHandler::PostLoginFunction, path, params, response))
	//	return;

	response.SetMimeType(L"None");
}


//--------------------------------------------------------------------------------
BOOL CTbMenuHandler::MenuInfinity(CLoginContext* pContext)
{
	CString file = AfxGetPathFinder()->GetUserLogPath(AfxGetLoginInfos()->m_strUserName, AfxGetLoginInfos()->m_strCompanyName) + SLASH_CHAR + _T("IMAGO.log");
	try {
		CString xml = CheckMenuReload(pContext, AfxGetLoginManager()->GetSystemDBConnectionString(), file);

		if (!xml.IsEmpty())
			return ParseMenuInfinity(pContext, AfxGetLoginManager()->GetSystemDBConnectionString(), xml, file);
	}
	catch (CApplicationErrorException* e)
	{
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		return false;
	}
	return true;
}

//--------------------------------------------------------------------------------
void CTbMenuHandler::CloneEasyStudioDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandler::CloneEasyStudioDocumentFunction, path, params, response))
		return;

	CString newNs = params.GetValueByName(_T("newNamespace"));
	response.SetMimeType(L"None");
	const CDocumentDescription* pDescription = AfxGetDocumentDescription(newNs);
	if (pDescription)
	{
		CJSonResponse jsonResponse;
		jsonResponse.SetMessage
		(
			cwsprintf
			(
				_TB("Document {0-%s} is already existing.\r\nPlease, change your name."),
				pDescription->GetName()
			)
		);
		response.SetData(jsonResponse.GetJson());
		return;
	}

	CloneEasyStudioDocument(params);
}

//--------------------------------------------------------------------------------
bool CTbMenuHandler::CloneEasyStudioDocument(const CNameValueCollection& params)
{
	CString originalNs = params.GetValueByName(_T("ns"));
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	CString newNs = params.GetValueByName(_T("newNamespace"));
	CString newTitle = params.GetValueByName(_T("newTitle"));

	AfxInvokeAsyncThreadFunction
		<
		bool,
		CTbMenuHandler,
		CString,
		CString,
		CString
		>
		(
			AfxGetLoginContext(authToken)->m_nThreadID,
			this,
			&CTbMenuHandler::CloneEasyStudioDocument,
			originalNs,
			newNs,
			newTitle
			);
	return true;

}

//--------------------------------------------------------------------------------
bool CTbMenuHandler::CloneEasyStudioDocument(CString strOriginalNs, CString strNewNs, CString strNewTitle)
{
	CTBNamespace newNs(CTBNamespace::DOCUMENT, strNewNs);

	// already existing
	if (AfxGetDocumentDescription(newNs))
		return false;
	return ::CloneAsEasyStudioDocument(strOriginalNs, strNewNs, strNewTitle) == TRUE;
}