#include "stdafx.h"

#include <TbNameSolver\IFileSystemManager.h>
#include <TbGes\EXTDOC.h>
#include <TbGes\DBT.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\OslInfo.h>
#include <TbGeneric\CMapi.h>
#include <TbGeneric\FunctionCall.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGeneric\DocumentObjectsInfo.h>
#include <TbGeneric\JsonFormEngine.h>
#include <TbClientCore\ClientObjects.h>
#include <TBNamesolver\LoginContext.h>
#include <TBNamesolver\ThreadContext.h>
#include <TBApplication\TaskBuilderApp.h>
#include <TBApplicationWrapper\EBEnumsManager.h>

#include "MDocument.h"
#include "Utility.h"

using namespace System;
using namespace System::Windows::Forms;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Data::DatabaseLayer;
using namespace Microarea::TaskBuilderNet::Core::NameSolver;
using namespace System::Collections::Generic;


//------------------------------------------------------------------------------------
WeifenLuo::WinFormsUI::Docking::DockPanel^ CUtility::GetHostingPanel(System::IntPtr documentPtr)
{
	CAbstractFormDoc* pDocument = (CAbstractFormDoc*)documentPtr.ToInt64();

	CMasterFrame* pFrame = pDocument->GetMasterFrame();
	
	if (pFrame == NULL)
		return nullptr;
	//la parent della master frame � la finestra dock content che ospita il documento
	CWnd* pWnd = pFrame->GetParent();

	if (pWnd == NULL)
		return nullptr;

	System::Windows::Forms::Control^ control = Form::FromHandle((IntPtr)pWnd->m_hWnd);
	if (control == nullptr)
	{
		return nullptr;
	}

	System::Windows::Forms::ToolStripContentPanel^ toolStripContentPanel = dynamic_cast<System::Windows::Forms::ToolStripContentPanel^>(control);
	if (toolStripContentPanel == nullptr)
	{
		return nullptr;
	}
	System::Windows::Forms::ToolStripContainer^ toolStripContainer = dynamic_cast<System::Windows::Forms::ToolStripContainer^>(toolStripContentPanel->Parent);
	if (toolStripContainer == nullptr)
	{
		return nullptr;
	}
	if (toolStripContainer->Parent == nullptr)
	{
		return nullptr;
	}
	WeifenLuo::WinFormsUI::Docking::DockContent^ form = dynamic_cast<WeifenLuo::WinFormsUI::Docking::DockContent^>(Form::FromHandle(toolStripContainer->Parent->Handle));

	return form == nullptr ? nullptr : form->DockPanel;
}

//------------------------------------------------------------------------------------
WeifenLuo::WinFormsUI::Docking::DockPanel^ CUtility::GetHostingPanelFromHandle(System::IntPtr dockPanelHandle)
{
	WeifenLuo::WinFormsUI::Docking::DockContent^ form = dynamic_cast<WeifenLuo::WinFormsUI::Docking::DockContent^>(Form::FromHandle(dockPanelHandle));
	return form == nullptr ? nullptr : form->DockPanel;
}
//------------------------------------------------------------------------------------
String^ CUtility::GetDocumentTitle(String^ docNamespace)
{
	CTBNamespace nsDoc(docNamespace);
	const CDocumentDescription* pDocInfo = AfxGetDocumentDescription(nsDoc);
	if (!pDocInfo)
		return String::Empty;
	return gcnew String(pDocInfo->GetTitle());
}

//------------------------------------------------------------------------------------
bool CUtility::CanUseDataEntryNamespace(System::String^ docNamespace)
{
	CTBNamespace nsDoc(docNamespace);	
	return AfxGetTbCmdManager()->CanUseNamespace(nsDoc, OSLType_Template, OSL_GRANT_EXECUTE, nullptr) == TRUE;
}

//------------------------------------------------------------------------------------
System::IntPtr CUtility::GetMainFormHandle()
{
	return (System::IntPtr)AfxGetMenuWindowHandle();
}


//------------------------------------------------------------------------------------
 bool CUtility::IsAdmin()
 {
	 const CLoginInfos* pInfos = AfxGetLoginInfos();
	 return pInfos ? pInfos->m_bAdmin == TRUE : false;
 }

//------------------------------------------------------------------------------------
String^ CUtility::GetUser()
{
	 const CLoginInfos* pInfos = AfxGetLoginInfos();
	 return pInfos ? gcnew String(pInfos->m_strUserName) : String::Empty;
}

//------------------------------------------------------------------------------------
bool CUtility::IsActivated(String^ application, String^ module)
{
	return AfxIsActivated(CString(application), CString(module)) == 1;
}

//------------------------------------------------------------------------------------
bool CUtility::IsRemoteInterface()
{
	return AfxIsRemoteInterface() == 1;
}

//------------------------------------------------------------------------------------
String^ CUtility::GetCompany()
{
	const CLoginInfos* pInfos = AfxGetLoginInfos();
	return pInfos ? gcnew String(pInfos->m_strCompanyName) : nullptr;
}

//------------------------------------------------------------------------------------
System::Data::IDbConnection^ CUtility::OpenConnectionToCurrentCompany()
{
	const CLoginInfos* pInfos = AfxGetLoginInfos();
	if (!pInfos)
		return nullptr;
	
	TBConnection^ conn = gcnew TBConnection(
				gcnew String(pInfos->m_strNonProviderCompanyConnectionString),
				(Microarea::TaskBuilderNet::Interfaces::DBMSType) Enum::Parse(Microarea::TaskBuilderNet::Interfaces::DBMSType::typeid, gcnew String(pInfos->m_strDatabaseType))
				);
	conn->Open();
	return conn->DbConnect;
}

//------------------------------------------------------------------------------------
String^ CUtility::GetAuthenticationToken()
{
	return gcnew String(AfxGetAuthenticationToken());
}
//------------------------------------------------------------------------------------
int CUtility::GetCompanyId()
{
	const CLoginInfos* pInfos = AfxGetLoginInfos();
	return pInfos ? pInfos->m_nCompanyId : -1;
}

//------------------------------------------------------------------------------------
int CUtility::GetCompanyId(System::String^ token)
{
	CLoginContext* pContext = AfxGetLoginContext(token);
	if (!pContext)
		return -1;

	const CLoginInfos* pInfos = pContext->GetLoginInfos();
	return pInfos ? pInfos->m_nCompanyId : -1;
}

//------------------------------------------------------------------------------------
DateTime CUtility::GetApplicationDate()
{
	DataDate aDate =  AfxGetApplicationDate();

	return DateTime(aDate.Year(), aDate.Month(), aDate.Day());
}
//------------------------------------------------------------------------------------
int CUtility::GetDataBaseCultureLCID()
{
	const CLoginInfos* pInfos = AfxGetLoginInfos();
	return pInfos ? pInfos->m_wDataBaseCultureLCID : -1;
}

//------------------------------------------------------------------------------------
int CUtility::GetAllOpenDocumentNumber()
{
	CApplicationContext* pContext = AfxGetApplicationContext();
	return pContext ? pContext->GetAllOpenDocumentNumber() : 0;
}

//------------------------------------------------------------------------------------
int CUtility::GetAllOpenDocumentNumberEditMode()
{
	CApplicationContext* pContext = AfxGetApplicationContext();
	return pContext ? pContext->GetAllOpenDocumentNumberEditMode() : 0;
}

//------------------------------------------------------------------------------------
int CUtility::GetLoginSessionCount()
{
	return AfxGetApplicationContext()->GetLoginContextNumber();
}

//------------------------------------------------------------------------------------
void CUtility::RefreshMenuDocument()
{
	::PostMessage(AfxGetMenuWindowHandle(), UM_REFRESH_USER_OBJECTS, REFRESH_USER_DOCUMENT, NULL);
}

//------------------------------------------------------------------------------------
void CUtility::RefreshLogin()
{
	//fa uscire la maschera di login gdi che non vogliamo pi� usare
	//::PostMessage(AfxGetMenuWindowHandle(), UM_REFRESH_USER_LOGIN, NULL, NULL);
}

//------------------------------------------------------------------------------------
void CUtility::ShowImmediateBalloon()
{
	::PostMessage(AfxGetMenuWindowHandle(), UM_IMMEDIATE_BALLOON, NULL, NULL);
}

//------------------------------------------------------------------------------------
void CUtility::ReloadAllMenus()
{
	::PostMessage(AfxGetMenuWindowHandle(), UM_REFRESH_USER_OBJECTS, RELOAD_ALL_MENUS, NULL);
}
//------------------------------------------------------------------------------------
bool CUtility::ReloadApplicationWithManagedState (System::String^ appName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return (AfxGetTBThread() != NULL)
		? AfxReloadApplication(CString(appName)) == TRUE
		: AfxInvokeThreadGlobalFunction<BOOL, const CString&>(AfxGetApp()->m_nThreadID, &AfxReloadApplication, CString(appName)) == TRUE;
}
//------------------------------------------------------------------------------------
bool CUtility::ReloadApplication (System::String^ appName)
{
	//a volte mi viene chiamata in contesti in cui le variabili MFC sono gia' inizializzate, a volte no...
	//uso AfxGetApp come test
	if (!AfxGetApp())
		return ReloadApplicationWithManagedState(appName);

	return (AfxGetTBThread() != NULL)
		? AfxReloadApplication(CString(appName)) == TRUE
		: AfxInvokeThreadGlobalFunction<BOOL, const CString&>(AfxGetApp()->m_nThreadID, &AfxReloadApplication, CString(appName)) == TRUE;
}

//------------------------------------------------------------------------------------
void CUtility::ReinitActivationInfos ()
{
	AfxReinitActivationInfos();
}

//------------------------------------------------------------------------------------
void CUtility::AddWindowRef(System::IntPtr hwndWindow, bool modal)
{
	AddThreadWindowRef((HWND)hwndWindow.ToInt64(), modal);
}

//------------------------------------------------------------------------------------
void CUtility::RemoveWindowRef(System::IntPtr hwndWindow, bool modal)
{
	RemoveThreadWindowRef((HWND)hwndWindow.ToInt64(), modal);
}

//------------------------------------------------------------------------------------
void CUtility::GetItemSources(List<System::String^>^ appTitles, List<System::String^>^ modTitles, List<System::String^>^ titles, List<System::String^>^ namespaces)
{
	// ciclo per le AddOnApplications e gli AddOnModules che possiedono la definizione
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (!pAddOnApp)
			continue;

		for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
			
			CTBNamespaceArray aNs;

			for (int k = 0; k <= pAddOnMod->m_pAddOnLibs->GetUpperBound(); k++)
			{
				AddOnLibrary* pLib = pAddOnMod->m_pAddOnLibs->GetAt(k);
				pLib->GetItemSources(aNs);
			}

			for (int l = 0; l <= aNs.GetUpperBound(); l++)
			{
				appTitles->Add(gcnew String(pAddOnApp->GetTitle()));
				modTitles->Add(gcnew String(pAddOnMod->GetModuleTitle()));
				titles->Add(gcnew String(aNs.GetAt(l)->GetObjectName()));
				namespaces->Add(gcnew String(aNs.GetAt(l)->ToString()));
			}
		}
	}
}

//------------------------------------------------------------------------------------
void CUtility::GetValidators(List<System::String^>^ appTitles, List<System::String^>^ modTitles, List<System::String^>^ titles, List<System::String^>^ namespaces)
{
	// ciclo per le AddOnApplications e gli AddOnModules che possiedono la definizione
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (!pAddOnApp)
			continue;

		for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);

			CTBNamespaceArray aNs;

			for (int k = 0; k <= pAddOnMod->m_pAddOnLibs->GetUpperBound(); k++)
			{
				AddOnLibrary* pLib = pAddOnMod->m_pAddOnLibs->GetAt(k);
				pLib->GetValidators(aNs);
			}

			for (int l = 0; l <= aNs.GetUpperBound(); l++)
			{
				appTitles->Add(gcnew String(pAddOnApp->GetTitle()));
				modTitles->Add(gcnew String(pAddOnMod->GetModuleTitle()));
				titles->Add(gcnew String(aNs.GetAt(l)->GetObjectName()));
				namespaces->Add(gcnew String(aNs.GetAt(l)->ToString()));
			}
		}
	}
}


//------------------------------------------------------------------------------------
void CUtility::GetHotLinks(List<System::String^>^ appTitles, List<System::String^>^ modTitles, List<System::String^>^ titles, List<System::String^>^ namespaces)
{
	AfxGetTbCmdManager()->LoadNeededLibraries(CTBNamespace(), NULL, LoadAll);
	// ciclo per le AddOnApplications e gli AddOnModules che possiedono la definizione
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (!pAddOnApp)
			continue;

		for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);

			if (
					!pAddOnMod || !pAddOnMod->m_bIsValid || !pAddOnMod->HasHotlinks() ||
					!AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName())
				)
				continue;
				
			for (int l = 0; l <= pAddOnMod->m_pAddOnLibs->GetUpperBound(); l++)
			{
				AddOnLibrary* pAddOnLib = pAddOnMod->m_pAddOnLibs->GetAt(l);
				if (!pAddOnLib || !pAddOnLib->m_pAddOn)
					continue;

				for (int o = 0; o <= pAddOnLib->m_pAddOn->m_arHotLinks.GetUpperBound(); o++)
				{
					CFunctionDescription* pObject = (CFunctionDescription*) pAddOnLib->m_pAddOn->m_arHotLinks.GetAt(o);
					if	(!pObject->IsPublished())
						continue;

					appTitles->Add(gcnew String(pAddOnApp->GetTitle()));
					modTitles->Add(gcnew String(pAddOnMod->GetModuleTitle()));
					titles->Add(gcnew String((pObject->GetTitle().IsEmpty() ? pObject->GetNamespace().GetObjectName() : pObject->GetTitle())));
					namespaces->Add(gcnew String(pObject->GetNamespace().ToString()));
				}
			}
		}
	}
}

//------------------------------------------------------------------------------------
bool CUtility::SendAsAttachments(List<System::String^>^ attachmentsFiles, List<System::String^>^ attachmentsTitles, System::String^ errorMsg)
{
	if (!AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT))
		return false;
	
	CStringArray arAttachementsFiles;
	CStringArray arAttachmentsTitles;
	for (int i = 0; i < attachmentsFiles->Count; i++)
		arAttachementsFiles.Add(CString(attachmentsFiles[i]));

	for (int i = 0; i < attachmentsTitles->Count; i++)
		arAttachmentsTitles.Add(CString(attachmentsTitles[i]));

	CMessages messages;
	DataObjArray arMsg;

	bool result = AfxGetIMailConnector()->SendAsAttachments(arAttachementsFiles, arAttachmentsTitles, &messages) == TRUE;
	if (!result)
	{
		messages.ToArray(arMsg);
		for(int i = 0 ; i < arMsg.GetSize() ; i++)
			errorMsg = errorMsg + _T(" ") + gcnew String(arMsg.GetAt(i)->Str());
	}

	return result;
}

//------------------------------------------------------------------------------------
System::String^ CUtility::GetAppTitleByAppName(System::String^ appName)
{
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(appName);

	if (pAddOnApp)
		return gcnew String(pAddOnApp->GetTitle());
	
	return gcnew String(_T(""));
}

//----------------------------------------------------------------------------------------------------
System::String^ CUtility::GetModuleTitleByAppAndModuleName(System::String^ appName, System::String^ modName)
{
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(appName);

	if (!pAddOnApp)
		return gcnew String(_T(""));

	for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
	{
		AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);

		if (pAddOnMod && pAddOnMod->GetModuleName().Compare(CString(modName)) == 0)
			return gcnew String(pAddOnMod->GetModuleTitle());
	}

	return gcnew String(_T(""));
}

//------------------------------------------------------------------------------------
void CUtility::GetDocuments
				(
					List<System::Tuple<System::String^, System::String^, System::String^, System::String^>^>^ docsInfos
				)
{
	CStringArray arExcludedModules;

	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (!pAddOnApp || AfxGetPathFinder()->IsASystemApplication(pAddOnApp->m_strAddOnAppName))
			continue;

		for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);

			if (!pAddOnMod || !pAddOnMod->m_bIsValid || !AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()))
			{
				arExcludedModules.Add(pAddOnApp->m_strAddOnAppName + pAddOnMod->GetModuleName());
				continue;
			}
		}
	}
			
	CBaseDescriptionArray* pDescriptions = AfxGetDocumentsDescriptions();
	CDocumentDescription* pDocDescri = NULL;

	if (!pDescriptions)
		return;

	for (int i = 0; i < pDescriptions->GetCount(); i++)
			{
		pDocDescri = (CDocumentDescription*)pDescriptions->GetAt(i);
				
		if (!pDocDescri || !pDocDescri->IsRunnableAlone())
					continue;			
				
				// devo saltare i finder
		CViewModeDescription* pViewMode = pDocDescri->GetFirstViewMode();
				if	(pViewMode == NULL || pViewMode->GetType() == VMT_FINDER)
					continue;
				
		BOOL bFound = FALSE;
		for (int k = 0; k < arExcludedModules.GetCount() && !bFound; k++)
		{
			CString keyDoc = pDocDescri->GetOwner().GetApplicationName() + pDocDescri->GetOwner().GetModuleName();
			if (!keyDoc.Compare(arExcludedModules.GetAt(k)))
				bFound = TRUE;
			}

		if (bFound)
			continue;

		docsInfos->Add
		(
			gcnew Tuple<System::String^, System::String^, System::String^, System::String^>
			(
				gcnew String(pDocDescri->GetOwner().GetApplicationName()),
				gcnew String(pDocDescri->GetOwner().GetModuleName()),
				gcnew String(pDocDescri->GetTitle()), 	
				gcnew String(pDocDescri->GetNamespace().ToString())
				)
		);


		}
	delete pDescriptions;
}

//------------------------------------------------------------------------------------
bool CUtility::AttachArchivedDocument(System::Int32 archivedDocId, System::Int64 documentPtr, System::Int32% attachmentId, System::String^ errorMsg)
{
	CString strErrorMsg;
	int nAttachmentId;	
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)documentPtr;

	::ArchiveResult archRes = AfxInvokeThreadFunction<::ArchiveResult, CDMSAttachmentManagerObj, int, int&, CString&>(pDoc->GetFrameHandle(), pDoc->GetDMSAttachmentManager(), &CDMSAttachmentManagerObj::AttachArchivedDocument, archivedDocId, nAttachmentId, strErrorMsg);
	//::ArchiveResult archRes = pDoc->GetDMSAttachmentManager()->AttachArchivedDocument(archivedDocId, nAttachmentId, strErrorMsg);
	errorMsg = gcnew String(strErrorMsg);
	attachmentId = nAttachmentId;
	return archRes != ::ArchiveResult::TerminatedWithError;
}


//------------------------------------------------------------------------------------
bool CUtility::CreateNewSosDocument(System::Int64 documentPtr, System::Int32 attachmentId, System::String^% errorMsg)
{
	CString strErrorMsg;
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)documentPtr;

	bool bOk = AfxInvokeThreadFunction<bool, CDMSAttachmentManagerObj, int, CString&>(pDoc->GetFrameHandle(), pDoc->GetDMSAttachmentManager(), &CDMSAttachmentManagerObj::CreateNewSosDocument, attachmentId, strErrorMsg);
	//bool bOk = pDoc->GetDMSAttachmentManager()->CreateNewSosDocument(attachmentId, strErrorMsg);
	errorMsg = gcnew String(strErrorMsg);
	return bOk;
}

//------------------------------------------------------------------------------------
bool CUtility::AddEnumTag (Microarea::TaskBuilderNet::Core::Applications::EnumTag^ enumTag)
{
	EnumsTablePtr pEnumsTable = AfxGetWritableEnumsTable();
	if (!pEnumsTable)
		return false;

	pEnumsTable->AddEnumTag(
		CString(enumTag->Name),
		(WORD)enumTag->Value,
		CString(),//Ho visto che il titolo non � usato dal costruttore C++, siccome non esiste analoga propriet� in C# allora non passo nulla.
		CTBNamespace(CString(((BaseModuleInfo^)enumTag->OwnerModule)->NameSpace->FullNameSpace)),
		(WORD)enumTag->DefaultValue
		);

	for each (Microarea::TaskBuilderNet::Core::Applications::EnumItem^ item in enumTag->EnumItems)
		AddEnumItem(item);

	return true;
}

//------------------------------------------------------------------------------------
bool CUtility::AddEnumItem (Microarea::TaskBuilderNet::Core::Applications::EnumItem^ enumItem)
{
	EnumsTablePtr pEnumsTable = AfxGetWritableEnumsTable();
	if (!pEnumsTable)
		return false;

	const EnumTagArray* tags = pEnumsTable->GetEnumTags();
	if (!tags)
		return false;

	EnumTag* pOwnerTag = tags->GetTagByName(CString(enumItem->Owner->Name));
	if (!pOwnerTag)
		return false;

	pOwnerTag->AddItem(
		CString(enumItem->Name),
		(WORD)enumItem->Value,
		CTBNamespace(CString(((BaseModuleInfo^)enumItem->OwnerModule)->NameSpace->FullNameSpace))
		);

	return true;
}

//------------------------------------------------------------------------------------
bool CUtility::DeleteEnumTag (Microarea::TaskBuilderNet::Core::Applications::EnumTag^ enumTag)
{
	EnumsTablePtr pEnumsTable = AfxGetWritableEnumsTable();
	if (!pEnumsTable)
		return false;

	const EnumTagArray* tags = pEnumsTable->GetEnumTags();
	if (!tags)
		return false;

	EnumTag* pOwnerTag = tags->GetTagByName(CString(enumTag->Name));
	if (!pOwnerTag)
		return false;

	CEBEnumsManager* pEnumsManager = new CEBEnumsManager();
	pEnumsManager->DeleteTag(pOwnerTag, pEnumsTable);
	delete pEnumsManager;

	return true;
}

//------------------------------------------------------------------------------------
bool CUtility::DeleteEnumItem (Microarea::TaskBuilderNet::Core::Applications::EnumItem^ enumItem)
{
	EnumsTablePtr pEnumsTable = AfxGetWritableEnumsTable();
	if (!pEnumsTable)
		return false;

	const EnumTagArray* tags = pEnumsTable->GetEnumTags();
	if (!tags)
		return false;

	EnumTag* pOwnerTag = tags->GetTagByName(CString(enumItem->Owner->Name));
	if (!pOwnerTag)
		return false;

	pOwnerTag->DeleteItem(CString(enumItem->Name));

	return true;
}

//------------------------------------------------------------------------------------
void CUtility::GetDataFiles(List<System::String^>^ appTitles, List<System::String^>^ modTitles, List<System::String^>^ namespaces, bool useCountry)
{
	// ciclo per le AddOnApplications e gli AddOnModules che possiedono la definizione
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (!pAddOnApp)
			continue;

		for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);

			if (!pAddOnMod || !pAddOnMod->m_bIsValid || !AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()))
				continue;

			CString sPath = AfxGetPathFinder()->GetModuleDataFilePath(pAddOnMod->m_Namespace, CPathFinder::STANDARD);
			CString sCulture = useCountry ? AfxGetLoginManager()->GetProductLanguage() : AfxGetCulture();
			sPath = sPath + SLASH_CHAR + (sCulture.IsEmpty() ? _T("en") : sCulture);
			
			CStringArray arFiles;
			AfxGetFileSystemManager()->GetFiles(sPath, _T("*.xml"), &arFiles);

			for (int f = 0; f <= arFiles.GetUpperBound(); f++)
			{
				CString sFile = arFiles.GetAt(f);
				CTBNamespace ns = AfxGetPathFinder()->GetNamespaceFromPath(sFile);
				if (ns.IsValid())
				{
					ns.SetType(CTBNamespace::DATAFILE);
					// devo togliere l'estensione xche� il DataFileInfo non la vuole
					CString sName = ns.GetObjectName();
					sName = sName.Left(sName.GetLength() - 4);
					ns.SetObjectName(sName);

					appTitles->Add(gcnew String(pAddOnApp->GetTitle()));
					modTitles->Add(gcnew String(pAddOnMod->GetModuleTitle()));
					namespaces->Add(gcnew String(ns.ToString()));
				}
			}
		}
	}
}

//------------------------------------------------------------------------------------
void CUtility:: FireDMS_MassiveRowProcessed(System::Int32 archiveDocId, System::Int32 actionToDo, System::Int32 massiveResult, List<System::Int32>^ attachmentsList)
{
	FunctionDataInterface fdi;
	fdi.AddIntParam(_T("archivedDocId"), archiveDocId);
	fdi.AddIntParam(_T("actionToDo"), actionToDo);
	fdi.AddIntParam(_T("massiveResult"), massiveResult);
	DataInt aDummy;
	DataArray* attachmentList = new DataArray(aDummy.GetDataType());
	for (int i = 0; i < attachmentsList->Count; i++)
		attachmentList->Add(new DataInt(int(attachmentsList[i])));
	fdi.AddParam(_T("attachmentsList"), attachmentList);

	CTbCommandInterface* pCommand = AfxGetTbCmdManager();
	if (pCommand) 
		pCommand->FireEvent(_T("OnDMSMassiveRowProcessed"), &fdi);

	delete attachmentList;
}


//------------------------------------------------------------------------------------
void CUtility::FireDMS_MassiveProcessTerminated()
{
	CTbCommandInterface* pCommand = AfxGetTbCmdManager();
	if (pCommand) 
		pCommand->FireEvent(_T("OnDMSMassiveProcessTerminated"), NULL);
}

//------------------------------------------------------------------------------------
String^ CUtility::CryptString (String^ s)
{
	return gcnew String(::TbCryptString(CString(s)));
}


//------------------------------------------------------------------
class CManagedObjectWrapper : public CObject
{
	gcroot<Object^> m_Object;
	DECLARE_DYNCREATE(CManagedObjectWrapper);
public:
	CManagedObjectWrapper(){}
	CManagedObjectWrapper(Object^ obj)
	{
		m_Object = obj;
	}
	~CManagedObjectWrapper()
	{
		delete m_Object;
	}
	Object^ GetObject() { return m_Object; }
};
IMPLEMENT_DYNCREATE(CManagedObjectWrapper, CObject);

//------------------------------------------------------------------
long CUtility::CreateWebServiceStateObject(System::String^ type, ...array<Object^> ^args)
{
	if (!AfxGetLoginContext())
		return 0;

	CManagedObjectWrapper* pObj = new CManagedObjectWrapper(Activator::CreateInstance(Type::GetType(type), args));
	AfxAddWebServiceStateObject(pObj);
	return (long)pObj;

}

//------------------------------------------------------------------
bool CUtility::RemoveWebServiceStateObject(long handle)
{
	if (!AfxGetLoginContext())
		return false;

	if (!AfxExistWebServiceStateObject((CObject*)handle))
	{
		return false;
	} 
	AfxRemoveWebServiceStateObject((CObject*)handle, TRUE);

	return true;
}
//------------------------------------------------------------------
Object^ CUtility::GetWebServiceStateObject(long handle)
{
	if (!AfxGetLoginContext())
		return nullptr;

	CObject* pObj = ((CObject*)handle);
	if (!pObj)
		return nullptr;

	if (!AfxExistWebServiceStateObject(pObj))
		return nullptr;

	if (pObj->IsKindOf(RUNTIME_CLASS(CManagedObjectWrapper)))
	{
		return ((CManagedObjectWrapper*)pObj)->GetObject();
	}
	return nullptr;
}

//------------------------------------------------------------------------------------
int CUtility::GetWorkerId()
{
	return AfxGetWorkerId();
}

//------------------------------------------------------------------------------------
int CUtility::GetWorkerId(System::String^ token)
{
	return AfxGetWorkerId(CString(token));
}

// ------------------------------------------------------------------------------------
MWorker^ CUtility::GetCurrentWorker()
{
	return gcnew MWorker(AfxGetWorkersTable()->GetWorker(AfxGetWorkerId()));
}

//------------------------------------------------------------------------------------
System::String^ CUtility::GetViewManagedTypeName(int documentHandle, INameSpace^ documentPartNamespace)
{
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)(int)documentHandle;

	if (!pDoc)
		return nullptr;

	const CDocumentDescription* pDocumentDescription = pDoc->GetXmlDescription();

	if (!pDocumentDescription)
		return nullptr;

	CTBNamespace aNs((CString)documentPartNamespace->FullNameSpace);

	CDocumentPartDescription* pPartDescription =
		(CDocumentPartDescription*)pDocumentDescription->GetDocumentParts().GetInfo(aNs);

	if (!pPartDescription)
		return nullptr;

	CViewModeDescription* firstViewMode = pPartDescription->GetFirstViewMode();

	if (!firstViewMode)
		return System::String::Empty;

	return gcnew String(firstViewMode->GetManagedType());
}
		
//-----------------------------------------------------------------------------
bool CUtility::RunDocument(System::String^ command, System::String^ arguments)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CString aux = arguments;
	CBaseDocument* pDocument = AfxGetTbCmdManager()->RunDocument(CString(command), szDefaultViewMode, FALSE, NULL, (LPAUXINFO)(LPCTSTR)aux);
	return pDocument != NULL;
}


//-----------------------------------------------------------------------------
bool CUtility::IsLoginContextValid(System::String^ sAuthenticationToken)
{
	CLoginContext* pContext = AfxGetLoginContext(CString(sAuthenticationToken));
	return pContext != NULL;
}

//-----------------------------------------------------------------------------
System::String^ CUtility::GetJsonContext(System::String^ file)
{
	CJsonResource res;
	res.PopulateFromFile(file);
	return gcnew String(res.m_strContext);
}

//-----------------------------------------------------------------------------
TileDialogSize CUtility::TransfertEnumTileSize(ETileDialogSize tdSize)
{
	switch (tdSize)
	{
		case ETileDialogSize::Mini: 	return TileDialogSize::TILE_MINI;
		case ETileDialogSize::Wide:		return TileDialogSize::TILE_WIDE;
		default: 						return TileDialogSize::TILE_STANDARD;
	}

	return TileDialogSize::TILE_STANDARD;
}

/*inizio get per le min size dei controlli*/

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealTileSizeLU(ETileDialogSize eSize)
{
	switch (eSize)
	{
		case ETileDialogSize::Mini: return System::Drawing::Size(161, 177);
		case ETileDialogSize::Wide:	return System::Drawing::Size(654, 177);
		default:					return System::Drawing::Size(325, 177);
	}
	return System::Drawing::Size::Empty;
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealLabelSize()
{
	return System::Drawing::Size(100, 8);//logical units
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdeaGenericComboBoxSize()
{
	return System::Drawing::Size(100, 64);//logical units
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealCheckRadioButtonSize()
{
	return System::Drawing::Size(100, 10)/* logical units*/;
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::Get100x100Size()
{
	return System::Drawing::Size(100, 100); //logical units
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::Get200x200Size()
{
	return System::Drawing::Size(200, 200); //logical units
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealBodyEdiytSize()
{
	return System::Drawing::Size(200, 100);//logical units
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealBaseWindowWrapperSize()
{
	return System::Drawing::Size(100, 12);//logical units
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealTabberTileManagerSize()
{
	return System::Drawing::Size(300, 300);//logica units;
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealToolbarSize(bool getMinSize)
{
	return getMinSize ? System::Drawing::Size(16, 16) : System::Drawing::Size(110, 16);
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealStaticAreaSize()
{
	return System::Drawing::Size(175, 153);
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealTreeViewSize()
{
	return System::Drawing::Size(300, 200);//logical units
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealViewSize()
{
	return System::Drawing::Size(438, 381);//logical units
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealTileGroupSize()
{
	return CUtility::GetIdealTabberTileManagerSize();
}

//-----------------------------------------------------------------------------
System::Drawing::Size CUtility::GetIdealHeaderStripSize()
{
	return System::Drawing::Size(438, 57);//logical units
}
/*fine get per le min size dei controlli*/


//-----------------------------------------------------------------------------
PathFinderWrapper::PathFinderWrapper()
{

}

//-----------------------------------------------------------------------------
System::String^ PathFinderWrapper::GetEasyStudioReferenceAssembliesPath()
{
	return gcnew String(AfxGetPathFinder()->GetEasyStudioReferencedAssembliesPath());
}

//-----------------------------------------------------------------------------
System::String^ PathFinderWrapper::GetEasyStudioEnumsAssemblyName()
{
	return gcnew String(AfxGetPathFinder()->GetEasyStudioEnumsAssemblyName());
}

//-----------------------------------------------------------------------------
System::String^ PathFinderWrapper::GetEasyStudioCustomizationsPath()
{
	return gcnew System::String(AfxGetPathFinder()->GetEasyStudioCustomizationsPath());
}

//-----------------------------------------------------------------------------
System::String^ PathFinderWrapper::GetTemplatesPath(bool inCustom, bool createDir)
{
	return gcnew System::String
	(
		AfxGetPathFinder()->GetTemplatesPath
		(
			CTBNamespace(_T("Module.Extensions.EasyStudio")), 
			inCustom ? CPathFinder::CUSTOM : CPathFinder::STANDARD,
			createDir
		)
	);
}

//-----------------------------------------------------------------------------
bool PathFinderWrapper::ExistFile(System::String^ fileName)
{
	return ::ExistFile(fileName) == TRUE;
}

//-----------------------------------------------------------------------------
bool PathFinderWrapper::ExistFolder(System::String^ path)
{
	return ExistPath(path) == TRUE;
}

//-----------------------------------------------------------------------------
System::Collections::Generic::List<System::String^>^ PathFinderWrapper::GetFiles(System::String^ path, System::String^ searchKey)
{
	List<System::String^>^ files = gcnew List<System::String^>();
	CStringArray aFiles;
	::GetFiles(CString(path), CString(searchKey), &aFiles);
	for (int i = 0; i < aFiles.GetSize(); i++)
	{
		CString sFileName = aFiles.GetAt(i);
		files->Add(gcnew String(sFileName));
	}

	return files;
}

//--------------------------------------------------------------------------------
String^ PathFinderWrapper::GetEasyStudioAssemblyFullName(String^ customizationNameSpace, String^ user)
{
	// devo cambiare il tipo perche' non e' gestito
	CTBNamespace aNs((CString)customizationNameSpace);
	aNs.SetType(CTBNamespace::FORM);
	String^	pathRoot = gcnew String(AfxGetPathFinder()->GetDocumentPath(aNs, CPathFinder::CUSTOM, FALSE, CPathFinder::EASYSTUDIO, (CString) user));
	return System::IO::Path::Combine(pathRoot, gcnew String(aNs.GetObjectName()) + NameSolverStrings::DllExtension);
}

//---------------------------------------------------------------------------------
String^ PathFinderWrapper::GetImageNamespace(String^ appName, String^ moduleName, String^ nameWithExtension)
{
	return String::Join(".",NameSolverStrings::Image, appName, moduleName, NameSolverStrings::Images, nameWithExtension);
}

//---------------------------------------------------------------------------------
String^ PathFinderWrapper::GetImageFolderPath(String^ appName, String^ moduleName)
{
	CTBNamespace aNs(CTBNamespace::IMAGE, appName + CTBNamespace::GetSeparator() + moduleName);
	return gcnew String(AfxGetPathFinder()->GetModuleFilesPath(aNs, CPathFinder::CUSTOM, _T(""), FALSE, CPathFinder::EASYSTUDIO));
}

//--------------------------------------------------------------------------------
void PathFinderWrapper::TraceEasyStudioCustomizationLog(System::String^ text)
{
	String^ dirName = System::IO::Path::Combine(GetEasyStudioCustomizationsPath(), NameSolverStrings::CustomizationsLog);
	dirName = System::IO::Path::Combine(dirName, "Log");

	if (!ExistPath(dirName))
		RecursiveCreateFolders(dirName);

	DateTime now = DateTime::Now;
	String^ fullName =  String::Concat(dirName, "_", now.Year, "_", now.Month, "_", now.Day, ".txt");

	System::IO::File::AppendAllText(fullName,text);
}