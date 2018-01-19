
#include "stdafx.h"

#include <TbNameSolver\Diagnostic.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <tbgeneric\globals.h>
#include <tbgeneric\WndObjDescription.h>
#include <TbGenlib\LayoutContainer.h>
#include <TbGenlib\BaseTileManager.h>
#include <tbges\ExtDocView.h>
#include <TBApplication\CDesignManipulator.h>
#include <TbWoormViewer\WOORMDOC.H>

#include "CDEasyBuilder.hjson" //JSON AUTOMATIC UPDATE
#include "CDEasyBuilder.h"
#include "NewDocument.h"
#include "JsonDesigner.h"
#include "TBEasyBuilder.h"
#include <TBApplicationWrapper\BusinessObjectParams.h>
#include <TBApplicationWrapper\resource.hjson>

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Resources;
using namespace System::Threading;
using namespace System::Globalization; 
using namespace System::Windows::Forms;
using namespace System::Drawing;
using namespace WeifenLuo::WinFormsUI::Docking;

using namespace Microarea::EasyBuilder;
using namespace Microarea::EasyBuilder::DBScript;
using namespace Microarea::EasyBuilder::UI;
using namespace Microarea::EasyBuilder::Packager;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::UI::WinControls;
using namespace Microarea::TaskBuilderNet::UI::WinControls::Generic;
using namespace Microarea::TaskBuilderNet::Core::DiagnosticManager;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;


//declasso gli erorri a warning per non bloccare l'apertura del documento
#define EXECUTE_SAFE_CONTROLLER_CODE(code) \
	try\
	{\
		if (controller->IsDisposed || !controller->CanBeLoaded)\
			continue;\
		code;\
		if (controller->Diagnostic->Error)\
		{\
			String^ err = controller->Diagnostic->ToString();\
			controller->Diagnostic->Clear();\
			throw gcnew Exception(err);\
		}\
		else\
		{\
			for each (DiagnosticItem^ item in controller->Diagnostic->AllMessages())\
				m_pServerDocument->m_pMessages->Add (CString(item->FullExplain), CMessages::MSG_WARNING); \
			controller->Diagnostic->Clear();\
		}\
	}\
	catch (Exception^ exc)\
	{\
		notWorkingControllers->Add(controller);\
		m_pServerDocument->m_pMessages->Add (CString(exc->Message), CMessages::MSG_WARNING);\
		controllers->LogExceptionsToFile(controller->GetType()->Namespace, exc);\
		continue;\
	}

//classe di comunicazione managed/unmanaged per gestire gli eventi del form manager
//infatti, non posso insereire un event handler nel client document, perché non è una classe managed

//////////////////////////////////////////////////////////////////////////////////	
//								CustomizationInfo class implementation
//////////////////////////////////////////////////////////////////////////////////	
CustomizationInfo::CustomizationInfo()
{
	loadAllCustomizations = true;
	centerControls = AfxCenterControlsEnabled();
	documentRestarted = false;
	customizationNamespace = gcnew NameSpace(String::Empty);
	documentNamespace = gcnew NameSpace(String::Empty);
}

//////////////////////////////////////////////////////////////////////////////////	
//								CDManagedGate class implementation
//////////////////////////////////////////////////////////////////////////////////	
ref class CDManagedGate
{
	String^ m_sOriginalDocumentTitle;
	CAbstractFormDoc* m_pServerDocument;
	HWND n_hwndMasterToolbar;
	HWND n_hwndAuxToolbar;
	CDEasyBuilder* m_pCDEasyBuilder;
	String^ customizationToRun;

	[ThreadStatic]
	static CustomizationInfo^ m_pCustomizationInfo;

public:
	//-----------------------------------------------------------------------------
	CDManagedGate(CAbstractFormDoc* pServerDocument, CDEasyBuilder* cdEasyBuilder)
		: m_pServerDocument(pServerDocument), m_sOriginalDocumentTitle(nullptr), m_pCDEasyBuilder(cdEasyBuilder)
	{
		if(!m_pCustomizationInfo)
			m_pCustomizationInfo = gcnew CustomizationInfo();
	}

	//-----------------------------------------------------------------------------
	void SetCustomizationToRun(String^ customization)
	{
		customizationToRun = customization;
	}

	//-----------------------------------------------------------------------------
	String^ GetCustomizationToRun()
	{
		return customizationToRun;
	}

	//-----------------------------------------------------------------------------
	CustomizationInfo^ GetCustomizationInfo()
	{
		return m_pCustomizationInfo;
	} 

	//-----------------------------------------------------------------------------
	void OnFormEditorDisposed(Object^ sender, EventArgs^ args)
	{
		m_pCDEasyBuilder->SetServerDocumentDesignMode(CBaseDocument::DM_NONE);
		ResetDocumentTitle();
		
		//delete m_pCustomizationInfo;
		AfxSetCenterControlsEnabled(m_pCustomizationInfo->CenterControlsActive);
		MemoryManagement::Flush();
	}
	
	//-----------------------------------------------------------------------------
	void OnFormEditorDirtyChanged(Object^ sender, DirtyChangedEventArgs^ args)
	{
//		customizationNamespace = args->NameSpace;
		CString strTitle = String::Format("{0} ({1})", m_sOriginalDocumentTitle, args->NameSpace->Leaf);
		if (args->Dirty)
			strTitle+= "*";

		if (!m_pServerDocument)
			return;

		CView* pView = m_pServerDocument->GetFirstView();
		if (!pView)
			return;

		//Se la finestra del documento e` visibile allora per cambiarne il titolo basta la SetFormTitle.
		if (IsTBWindowVisible(pView))
		{
			m_pServerDocument->SetFormTitle(strTitle);
			return;
		}

		//Se invece la finestra non `e visibile (per esempio perche` sono aperte delle finestre del code editor
		//sopra di lei) allora devo fare cosi` per cambiarne il titolo:
		CFrameWnd* pFrame = pView->GetParentFrame();
		if (!pFrame)
			return;

		pFrame->SetWindowText(strTitle);
		m_pServerDocument->SetTitle(strTitle);

		::PostMessage(AfxGetMenuWindowHandle(), UM_FRAME_TITLE_UPDATED, (WPARAM)pFrame->m_hWnd, NULL);
	}

	//-----------------------------------------------------------------------------
	void OnRestartDocument(Object^ sender, Microarea::EasyBuilder::RestartEventArgs^ args)
	{
		m_pCustomizationInfo->DocumentNamespace = args->DocumentNamespace;
		m_pCustomizationInfo->CustomizationNamespace = args->CustomizationNamespace;
		m_pCustomizationInfo->LoadAllCustomizations = args->Action == Microarea::EasyBuilder::RestartAction::RestartAndLoadAll;
		bool editOnRestart = args->Action == Microarea::EasyBuilder::RestartAction::RestartAndGoInEdit;
		RestartDocument(editOnRestart, args->DocumentNamespace, args->IsServerDocument);
	}

	//modifico il titolo del documento aggiungendo il nome della customizzazione (ultimo token del namespace)
	//-----------------------------------------------------------------------------
	void SetDocumentTitle(INameSpace^ customizationNamespace)
	{
		if (customizationNamespace == nullptr)
			return;

		m_sOriginalDocumentTitle = gcnew String(m_pServerDocument->GetTitle());
		CString strTitle = String::Format("{0} ({1})", m_sOriginalDocumentTitle, customizationNamespace->Leaf);
		m_pServerDocument->SetTitle(strTitle);
	}

	//-----------------------------------------------------------------------------
	void RestartDocument(bool editOnRestart, INameSpace^ docNamespace, bool isServerDocument)
	{
		//Se il documento è stato restartato e deve rientrare in edit, allora disabilito temporaneamente 
		//l'algoritmo di centratura (in questo modo l'algoritmo non intralcia il funzionamento in edit della customizazzione
		BOOL centerControls = (editOnRestart) ? FALSE : m_pCustomizationInfo->CenterControlsActive;
		AfxSetCenterControlsEnabled(centerControls);

		//Per evitare che i messaggi di documento vengano riproposti in restart del documento, faccio partire 
		//'un'altra sessione di diagnostica....
		AfxGetDiagnostic ()->StartSession();

		CRestartDocumentInvocationInfo* pInfo = NULL;
		if (editOnRestart && !m_pCustomizationInfo->IsEasyStudioDesigner)
			pInfo = new CRestartDocumentInvocationInfo(CBaseDocument::DM_RUNTIME);

		CBaseDocument* pDoc = AfxGetTbCmdManager()->RunDocument(CString(docNamespace->ToString()), szDefaultViewMode, FALSE, NULL, NULL, NULL, NULL, NULL, FALSE, FALSE, pInfo);
		if (!pDoc)
		{
			AfxSetCenterControlsEnabled(m_pCustomizationInfo->CenterControlsActive);
			return;
		}
		
		//...e alla fine della run pulisco i messaggi e chiudo la sessione
		AfxGetDiagnostic ()->ClearMessages();
		AfxGetDiagnostic ()->EndSession();

		m_pServerDocument->CloseDocument();

		//Siccome, quando abbiamo un documento dinamico aggiunto da un verticale nella standard, dobbiamo mantenerlo in memoria
		//mentre si entra in modifica di una customizzazione, è necessario badare a quale offset mandare in in Post per entrare in
		//customizzazione.
		//Se stiamo entrando in modifica proprio del verticale, allora, per come vengono caricate le DLL, questa sarà la prima per cui mandiamo 
		//offset = 0, altrimenti mandiamo offset = 1 (per shiftare di un posto, quello occupato dal verticale).
		int offset = isServerDocument ? 0 : 1;
		
		//Se devo rientrare in edit, invio il messaggio di click del bottone per entrare in edit
		if (pDoc && editOnRestart)
			pDoc->PostMessage(WM_COMMAND, ID_FORM_EDITOR_EDIT,  0);
	}

	//reimposto il titolo del documento al suo valore originale
	//-----------------------------------------------------------------------------
	void ResetDocumentTitle()
	{
		m_pServerDocument->SetTitle(CString(m_sOriginalDocumentTitle));
	}

	//-----------------------------------------------------------------------------
	bool IsFrameDockable()
	{
		CDockableFrame* dockableFrame = dynamic_cast<CDockableFrame*>(m_pServerDocument->GetMasterFrame());

		return dockableFrame && dockableFrame->IsDockable();
	}
};

class CDEasyBuilderEventManager : public CEventManager
{
BEGIN_TB_EVENT_MAP(CDEasyBuilderEventManager)
	TB_EVENT 	 (CDEasyBuilder,  EasyBuilderIt)
END_TB_EVENT_MAP
public:
	CDEasyBuilderEventManager()
	{
		
	}

};

class ControllersEventManager : public CEventManager
{
public:
	ControllersEventManager()
	{
		
	}

	virtual int FireAction(const CString& funcName, CString* pstrInputOutput)
	{
		CDEasyBuilder* peb = (CDEasyBuilder*)m_pDocument;
		return peb->FireAction(funcName, pstrInputOutput);
	}
 	virtual int FireAction(const CString& funcName, void* pVoidInputOutput)
	{
		CDEasyBuilder* peb = (CDEasyBuilder*)m_pDocument;
		return peb->FireAction(funcName, pVoidInputOutput);
	}
	
	virtual int FireAction(const CString& funcName)
	{
		CDEasyBuilder* peb = (CDEasyBuilder*)m_pDocument;
		return peb->FireAction(funcName);
	}
	
	virtual BOOL ExistAction(const CString& funcName, MappedFunction** ppMappedFunction = NULL, CObject** ppObj = NULL)
	{
		return TRUE;
	}

};


//////////////////////////////////////////////////////////////////////////////
//             				CDEasyBuilder
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDEasyBuilder, CClientDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDEasyBuilder, CClientDoc)
	// comandi per la gestione dei radar-woorm 

	ON_COMMAND(ID_CHOOSE_CONTEXT, ChooseContext)
	ON_COMMAND(ID_FORM_EDITOR_EDIT, OnEditForm)
	ON_COMMAND_RANGE(ID_FORM_EDITOR_RANGE, (UINT)(ID_FORM_EDITOR_RANGE + MAX_EB_COMMANDS-1), OnEditForm)
	ON_UPDATE_COMMAND_UI(ID_FORM_EDITOR_EDIT,   OnUpdateEditForm)

	ON_UPDATE_COMMAND_UI_RANGE(ID_FORM_EDITOR_RANGE, (UINT)(ID_FORM_EDITOR_RANGE + MAX_EB_COMMANDS - 1), OnUpdateDropdown)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDEasyBuilder::CDEasyBuilder()
	:
	formEditor		(nullptr),
	documentControllers	(nullptr),
	managedGate (nullptr),
	m_bNewDocument(false),
	m_pNsDocToRestart(NULL),
	hInstance(NULL)
{
}

//-----------------------------------------------------------------------------
CDEasyBuilder::~CDEasyBuilder()
{
	//non effettuare la pulizia qui, ma nella OnCloseServerDocument, altrimenti
	//i puntatori degli oggetti di documento sono invalidi!!!
}

//-----------------------------------------------------------------------------
WebCommandType CDEasyBuilder::OnGetWebCommandType(UINT commandID)
{
	if (
			commandID == ID_FORM_EDITOR_EDIT || 
			(
				commandID >= ID_FORM_EDITOR_RANGE && commandID < (ID_FORM_EDITOR_RANGE + MAX_EB_COMMANDS)
			)
		)
		return WEB_UNSUPPORTED;

	return __super::OnGetWebCommandType(commandID);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::PreTranslateMsg	(HWND hWnd, MSG* pMsg)
{
	// propago il messaggio nella toolbar di EasyBulder
	DocumentControllers^ controllers = documentControllers;
	if (controllers != nullptr && (pMsg->message == WM_KEYDOWN || pMsg->message == WM_SYSKEYDOWN))
	{
		IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
		for each (DocumentController^ controller in controllers)
		{
			if (controller->View == nullptr)
				continue;
			
			EXECUTE_SAFE_CONTROLLER_CODE(
				//chiamo questa perché viene chiamata prima OnBuildDataControlLinks della tab,
				//che però non essendo stata crata la view non fa nulla
				controller->View->PreTranslateMsgKey(pMsg->message, pMsg->wParam, pMsg->lParam);
				
				)
			
		}
	}

	return __super::PreTranslateMsg(hWnd, pMsg); 

}
//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnGetToolTipText (UINT nId, CString& strMessage)
{
	if ((nId == ID_FORM_EDITOR_EDIT) || (nId >= ID_FORM_EDITOR_RANGE && nId < (ID_FORM_EDITOR_RANGE + MAX_EB_COMMANDS)))
	{
		if (hInstance == NULL)
			GET_DLL_HINSTANCE(hInstance);

		strMessage = AfxLoadTBString(ID_FORM_EDITOR_EDIT, hInstance);
		return TRUE;
	}
	if (nId == AFX_IDS_IDLEMESSAGE)
	{
		strMessage = AfxLoadTBString(AFX_IDS_IDLEMESSAGE);

		if (documentControllers && documentControllers->Count > 0)
			strMessage += _T(" ") + cwsprintf(_TB("({0-%d} customizations)"), documentControllers->Count);
		return TRUE;
	}
	return FALSE;
}
//-----------------------------------------------------------------------------
NameSpace^ CDEasyBuilder::GetServerDocumentNamespace ()
{
	return gcnew NameSpace( gcnew String(m_pServerDocument->GetNamespace().ToString()));
}

//-----------------------------------------------------------------------------
int CDEasyBuilder::FireAction(const CString& funcName, CString* pstrInputOutput)
{
	if (documentControllers && documentControllers->HasEventManagerEvents)
	{
		ControllerEventManagerArgs^ args = gcnew ControllerEventManagerArgs(gcnew String(funcName));
		args->Data = gcnew String(*pstrInputOutput);
		documentControllers->DispatchEvent(args);
		*pstrInputOutput = args->Data;
	}
	return CEventManager::FUNCTION_OK;
}
//-----------------------------------------------------------------------------
int CDEasyBuilder::FireAction(const CString& funcName, void* pVoidInputOutput)
{
	if (documentControllers && documentControllers->HasEventManagerEvents)
	{
		ControllerEventManagerArgs^ args = gcnew ControllerEventManagerArgs(gcnew String(funcName));
		documentControllers->DispatchEvent(args);
	}
	return CEventManager::FUNCTION_OK;
}
//-----------------------------------------------------------------------------
int CDEasyBuilder::FireAction(const CString& funcName)
{
	if (documentControllers && documentControllers->HasEventManagerEvents)
	{
		ControllerEventManagerArgs^ args = gcnew ControllerEventManagerArgs(gcnew String(funcName));
		documentControllers->DispatchEvent(args);
	}
	return CEventManager::FUNCTION_OK;
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::Customize()
{
	CreateJsonToolbar(IDD_EASYSTUDIO_TOOLBAR);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnToolbarDropDown (UINT nID, CMenu& menu) 
{ 
	if (nID != ID_FORM_EDITOR_EDIT)
		return FALSE;

	int i = ID_FORM_EDITOR_RANGE;

	if (BaseCustomizationContext::CustomizationContextInstance->CurrentEasyBuilderApp == nullptr)
	{
		AddNewTagType addNewTagType = GetAddNewTag();
		CString addNewTag;
		switch (addNewTagType)
		{
		case AddNewTagType::None: addNewTag = _T(""); break;
		case AddNewTagType::AddNewCustomization: addNewTag = _TB("(Add new customization...)"); break;
		case AddNewTagType::Forbidden:
		{
			AfxMessageBox(_TB("You cannot modify this document because it has been created in a customization context and you now are in a standardization context. If you would like to modify it you should switch to a customization context."));
			return FALSE;
		}
		default:
			return FALSE;
		}

		if (!addNewTag.IsEmpty())
		{
			menu.AppendMenu(MF_STRING, ++i, addNewTag);
			menu.EnableMenuItem(i, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		}
			
			
		return TRUE;
	}

	CAbstractFormFrame* pFrame = m_pServerDocument->GetMasterFrame();
	if (pFrame)
	{
		CTBTabbedToolbar* pTabbedBar = pFrame->GetTabbedToolBar();

		CTBToolBar* pToolbar = pTabbedBar ? pTabbedBar->FindToolBar(szToolbarNameTools) : NULL;
		if (pToolbar)
			pToolbar->SetAlwaysDropDown(ID_FORM_EDITOR_EDIT, PURE_ALWAYS_DROPDOWN);
	}

	//Se il contesto di customizzazione corrente è impostato su Standardizzazione
	//ma il setting nascosto dice che non posso editare le standardizzazioni
	//allora non permetto di entrare in modifica del presente documento.
	if (
		BaseCustomizationContext::CustomizationContextInstance->IsCurrentEasyBuilderAppAStandardization &&
		!BaseCustomizationContext::CustomizationContextInstance->ShouldStandardizationsBeAvailable()
		)
	{
		AfxMessageBox (_TB("You cannot modify this document because a standardization context is the current context."));
		return FALSE;
	}

	DocumentControllers^ controllers = documentControllers;
	controllersNameSpace.RemoveAll();
	if (controllers)
	{
		for each (DocumentController^ controller in controllers)
		{
			if (IsEditableController(controller))
			{
				UINT flags = BaseCustomizationContext::CustomizationContextInstance->IsActiveDocument(controller->CustomizationNameSpace)
					? MF_STRING | MF_CHECKED
					: MF_STRING;

				CString sName(controller->Name);
				int index = ++i;
				menu.AppendMenu(flags, index, sName);
				controllersNameSpace.Add(sName);
			}
		}
	}


	if (BaseCustomizationContext::CustomizationContextInstance->CurrentEasyBuilderApp != nullptr)
	{
		AddNewTagType addNewTagType = GetAddNewTag();
		CString addNewTag;
		switch (addNewTagType)
		{
		case AddNewTagType::None: addNewTag = _T(""); break;
		case AddNewTagType::AddNewCustomization: addNewTag = _TB("(Add new customization...)"); break;
		case AddNewTagType::Forbidden:
		{
			AfxMessageBox(_TB("You cannot modify this document because it has been created in a customization context and you now are in a standardization context. If you would like to modify it you should switch to a customization context."));
			return FALSE;
		}
		default:
			return FALSE;
		}

		if (!addNewTag.IsEmpty())
			menu.AppendMenu(MF_STRING, ++i, addNewTag);
	}

	//menu.AppendMenu(MF_STRING, ID_CHOOSE_CONTEXT, _TB("(Change customization context...)"));
	return TRUE;
}

//-----------------------------------------------------------------------------
DocumentController^ CDEasyBuilder::GetActiveController()
{
	DocumentControllers^ controllers = documentControllers;
	for each (DocumentController^ controller in controllers)
	{
		if (IsEditableController(controller) && BaseCustomizationContext::CustomizationContextInstance->IsActiveDocument(controller->CustomizationNameSpace))
			return controller;
	}

	return nullptr;
}

//-----------------------------------------------------------------------------
bool CDEasyBuilder::IsControllerEditableInCurrentCustomizationContext(DocumentController^ controller)
{
	System::String^ controllerPath = documentControllers->GetControllerPathByController(controller);
	System::String^ pathToSearch = String::Concat(BaseCustomizationContext::CustomizationContextInstance->CurrentApplication, "\\", BaseCustomizationContext::CustomizationContextInstance->CurrentModule);

	return controllerPath->IndexOf(pathToSearch) >= 0;
}

//-----------------------------------------------------------------------------
bool CDEasyBuilder::IsEditableController(DocumentController^ controller)
{
	return
		//Se il controller corrente è stato apportato da una customizzazione 
		//=>aggiungo il controller corrente alla tendina
		//(In pratica il succo del discorso è che nella tendina, se sono in un contesto di customizzazione, vedo tutte le altre customizzazioni)
		controller->CustomizationNameSpace->NameSpaceType->Type == Microarea::TaskBuilderNet::Interfaces::NameSpaceObjectType::Customization;
	
}

//-----------------------------------------------------------------------------
//Se il controller corrente è un server document AND
//siamo un contesto di standarizzazione AND
//il controller che apporta il server document è proprio contenuto nel corrente contesto di standardizzazione.
AddNewTagType CDEasyBuilder::GetAddNewTag()
{
	return AddNewTagType::AddNewCustomization;
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnBuildDataControlLinks(CAbstractFormView* pView)
{
	//Se la view non è una master form view allora significa che il presente documento non è customizzabile da EasyBuilder,
	//=> ritorno perchè è inutile dispatchare i messaggi di un client doc attach-ato ad un documento non customizzabile.
	if (!pView->IsKindOf(RUNTIME_CLASS(CMasterFormView)))
		return;

	__super::OnBuildDataControlLinks (pView);
	
	if (m_pServerDocument->IsInUnattendedMode())
		return;

	//TB://Document.PaiNet.PaiCore.Documents.Bugs?bugid:20564
	//In caso il documento abbia più view (ad esempio in caso di documenti con splitter vari) la builddatacontrollinks
	//viene chiamata per ogni view che trova durante la create components
	//questo tappullo esegue il codice della create components solo sulla firstview 
	//TODOLUCA chiedere a bruna
	CAbstractFormView* pTemp = dynamic_cast<CAbstractFormView*>(m_pServerDocument->GetFirstView());
	if (pTemp != pView)
		return;

	CDesignModeLayoutManipulator* pLayoutManipulator = dynamic_cast<CDesignModeLayoutManipulator*>(m_pServerDocument->GetDesignModeManipulatorObj());
	if (pLayoutManipulator)
		// integro il cambiamento di design mode prima di completare il wrapping
		pLayoutManipulator->OnAfterBuildDataControlLinks(pView);

	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;

	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	if (m_pServerDocument && !m_pServerDocument->IsInDesignMode())
	{
		for each (DocumentController^ controller in controllers)
		{
			if (controller->View == nullptr)
				continue;

			EXECUTE_SAFE_CONTROLLER_CODE(
				//chiamo questa perché viene chiamata prima OnBuildDataControlLinks della tab,
				//che però non essendo stata crata la view non fa nulla
				controller->View->CallCreateComponents();
				controller->View->RaiseOnLoad();
			)
		}
	}

	if (notWorkingControllers->Count == 0)
		return;

	m_pServerDocument->m_pMessages->Show(TRUE);

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnBuildDataControlLinks (CTabDialog* pDialog)
{
	if (m_pServerDocument->IsInUnattendedMode())
		return;

	CDesignModeLayoutManipulator* pLayoutManipulator = dynamic_cast<CDesignModeLayoutManipulator*>(m_pServerDocument->GetDesignModeManipulatorObj());
	if (pLayoutManipulator)
		// integro il cambiamento di design mode prima di completare il wrapping
		pLayoutManipulator->OnAfterBuildDataControlLinks(pDialog);

	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;

	MTab^ tbTab = nullptr;
	MTileGroup^ tbGroup = nullptr;
	MTabber^ tbTabber = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	//if (m_pServerDocument && !m_pServerDocument->IsInDesignMode())
	{
		for each (DocumentController^ controller in controllers)
		{
			if (controller->View == nullptr)
				continue;

			tbTabber = controller->View->GetTabberByName(gcnew String(pDialog->GetParentTabManager()->GetNamespace().GetObjectName()));
			if (tbTabber != nullptr)
			{
				tbTab = tbTabber->GetTabByName(gcnew String(pDialog->GetFormName()));
				if (tbTab != nullptr && tbTab->Handle != IntPtr::Zero)
				{
					EXECUTE_SAFE_CONTROLLER_CODE(tbTab->CallCreateComponents();)
					continue;
				}
			}

			MTileManager^ tileManager = dynamic_cast<MTileManager^>(tbTabber);
			if (tileManager != nullptr)
			{
				tbGroup = tileManager->GetGroupByName(gcnew String(pDialog->GetFormName()));

				if (tbGroup != nullptr)
				{
					tbGroup->SyncTileGroup();
					if (tbGroup->Handle != IntPtr::Zero)
					{
						EXECUTE_SAFE_CONTROLLER_CODE(
							tbGroup->CallCreateComponents();
						
						if (controller->View->LayoutObject != nullptr)
							controller->View->LayoutObject->LayoutChangedFor(tileManager->Namespace);

						);
					}
				}
			}
		}
	}

	if (notWorkingControllers->Count == 0)
		return;

	m_pServerDocument->m_pMessages->Show(TRUE);

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}

//-----------------------------------------------------------------------------
int CDEasyBuilder::AskForDisablingNotWorkingControllers(System::Collections::ICollection^ items)
{
	if (!CUtility::IsAdmin())
		return IDNO;

	System::Text::StringBuilder^ messageBuilder = gcnew System::Text::StringBuilder();
	messageBuilder->Append(gcnew String(CString(_TB("Following customizations encountered some problems and are not able to work properly:"))));

	System::Text::StringBuilder^ detailsBuilder = gcnew System::Text::StringBuilder();
	detailsBuilder->Append(System::Environment::NewLine)->Append(System::Environment::NewLine);

	for each (Object^ obj in items)
		detailsBuilder->Append(obj->ToString())->Append(System::Environment::NewLine);

	detailsBuilder->Append(System::Environment::NewLine)->Append(gcnew String(CString(_TB("Do you want to disable them all?"))));

	return m_pServerDocument->Message(
		CString(messageBuilder->ToString()),
		MB_YESNO,
		0,
		CString(detailsBuilder->ToString()),
		CMessages::MSG_HINT
		);
}
//-----------------------------------------------------------------------------
void CDEasyBuilder::ChooseContext()
{
	PostMessage(AfxGetMenuWindowHandle(), UM_CHOOSE_CUSTOMIZATION_CONTEXT, NULL, NULL);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::EasyBuilderIt()
{
	//chiamiamo il metodo per far entrare il documento in edit come se avessimo cliccato sul bottone
	//della toolbar di Easybuilder 
	OnEditForm(ID_FORM_EDITOR_EDIT);
}

//------------------------------------------------------------------------------
void CDEasyBuilder::OnUpdateEditForm (CCmdUI* pCmdUI)
{	
	BOOL bVisible = managedGate->IsFrameDockable() &&
						AfxIsActivated(NameSolverStrings::Extensions, NameSolverStrings::EasyStudioDesigner) &&
						m_pServerDocument->GetXmlDescription()->IsDesignable();

	pCmdUI->Enable(bVisible && m_pServerDocument->GetFormMode() == CBaseDocument::BROWSE);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::SaveModified()
{
	if (IsEditing())
		return formEditor->AskAndSave(true);
	
	return TRUE;
}
//-----------------------------------------------------------------------------
bool CDEasyBuilder::IsEditing()
{
	return formEditor && !formEditor->IsDisposed;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnIdleHandler (LONG lCount)
{
	CTBWinThread* pThread = AfxGetTBThread();
	if (pThread && !pThread->IsManaged())
		Application::RaiseIdle(EventArgs::Empty);
	return FALSE;
}

//-----------------------------------------------------------------------------
NameSpace^ CDEasyBuilder::GetNewCustomizationName(INameSpace^ documentNamespace)
{
	String^ user = gcnew String(AfxGetLoginInfos()->m_strUserName);

	String^ path = Microarea::TaskBuilderNet::Core::NameSolver::BasePathFinder::BasePathFinderInstance->GetCustomDocumentPath
	(
		NameSolverStrings::AllCompanies, 
		BaseCustomizationContext::CustomizationContextInstance->CurrentApplication,
		BaseCustomizationContext::CustomizationContextInstance->CurrentModule,
		documentNamespace->Document
	);

	List<String^>^ files = gcnew List<String^>();
	String^ userPath = Path::Combine(path, user);
	String^ searchCriteria = "*.dll";
	if (System::IO::Directory::Exists(path))
	{
		array<String^>^ allFiles = System::IO::Directory::GetFiles(path, searchCriteria);
		if (allFiles != nullptr)
		{
			for each (String^ file in allFiles)
				files->Add(file);
		}
	}

	int nCount = files->Count;
	if (System::IO::Directory::Exists(userPath))
	{
		array<String^>^ userFiles = System::IO::Directory::GetFiles(userPath, searchCriteria);
		for each (String^ userFile in userFiles)
		{
			userFile = System::IO::Path::GetFileName(userFile);
			bool found = false;
			for each (String^ file in files)
			{
				file = System::IO::Path::GetFileName(file);
				if (String::Compare(userFile, file))
				{
					found = true;
					break;
				}
			}

			if (!found)
				nCount++;
		}
	}

	String^ name = EasyBuilderSerializer::Escape(documentNamespace->Leaf);
	String^ tempName = (nCount > 0) ? name + nCount : name;

	NameSpaceObjectType aNameSpaceObjectType = NameSpaceObjectType::Customization;
	return gcnew NameSpace
	(
		String::Format("{0}.{1}", documentNamespace->GetNameSpaceWithoutType(), tempName),
		aNameSpaceObjectType
	);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::ChooseCustomizationContextAndRunEasyStudio()
{
	CString nsDoc = m_pServerDocument->GetNamespace().ToString();
	//suicidio
	m_pServerDocument->PostMessage(WM_CLOSE, 0, 0);

	UINT n = nsDoc.GetLength() + 1;
	if (m_pNsDocToRestart)
		SAFE_DELETE(m_pNsDocToRestart);

	m_pNsDocToRestart = new TCHAR[n];
	_tcscpy_s(m_pNsDocToRestart, n, nsDoc);

	//post message a menumanager con namespace
	PostMessage(AfxGetMenuWindowHandle(), UM_CHOOSE_CUSTOMIZATION_CONTEXT_AND_EASYBUILDERIT_AGAIN, (WPARAM)m_pNsDocToRestart, NULL);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnUpdateDropdown(CCmdUI* pCmdUI)
{
	int i = ID_FORM_EDITOR_RANGE;

	DocumentControllers^ controllers = documentControllers;
	if (controllers)
	{
		for each (DocumentController^ controller in controllers)
		{
			if (!IsEditableController(controller))
				continue;

			CString sName(controller->Name);
			int index = ++i;

			if (pCmdUI->m_nID == index)
			{
				BOOL bActive = BaseCustomizationContext::CustomizationContextInstance->CurrentEasyBuilderApp != nullptr &&
					IsControllerEditableInCurrentCustomizationContext(controller);

				pCmdUI->Enable(bActive);
				continue;
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnEditForm()
{
	OnEditForm(ID_FORM_EDITOR_EDIT);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnEditForm(UINT nCmd)
{
	if (IsEditing() || !IsLicenseForEasyBuilderVerified())
		return;
	
	CView* pView = m_pServerDocument->GetFirstView();
	ASSERT(pView);
	bool isEasyStudioDesigner = TRUE == pView->IsKindOf(RUNTIME_CLASS(CEasyStudioDesignerView));
	//se non sono in modalità designer json, verifico che esista una current app per ospitare i cambiamenti
	if (!isEasyStudioDesigner && !BaseCustomizationContext::CustomizationContextInstance->ExistsCurrentEasyBuilderApp)
		return;
	
	int nControllerIdx = nCmd - ID_FORM_EDITOR_RANGE - 1;

	DockPanel^ panel = CUtility::GetHostingPanel((IntPtr)m_pServerDocument);
	if (panel == nullptr)
		return;

	//Imposto la modalità design per il documento (rende il documento inerte, shortcut comprese)
	SetServerDocumentDesignMode (isEasyStudioDesigner ? CBaseDocument::DM_STATIC : CBaseDocument::DM_RUNTIME);
	
	if (!documentControllers)
		documentControllers = gcnew DocumentControllers();
	
	DocumentControllers^ controllers = documentControllers;
	DocumentController^ controller = nullptr;
	
	CustomizationInfo^ aCustInfo = managedGate->GetCustomizationInfo();
	aCustInfo->RunFromButton = true;


	//Se il documento e' restarted, carico il controller by namespace,
	if (aCustInfo != nullptr && aCustInfo->DocumentRestarted)
		controller = controllers->GetControllerByName(aCustInfo->CustomizationNamespace->Leaf, true);
	else
	{
		//qui arriva dal click del controller sulla tendina
		if (-1 < nControllerIdx && nControllerIdx < controllersNameSpace.GetCount())
			controller = controllers->GetControllerByName(gcnew System::String(controllersNameSpace[nControllerIdx]), true);
		//qui arriva dal click sul bottone, va a cercare il controller attivo
		else
			controller = nControllerIdx == -1 ? GetActiveController() : nullptr;
	}
	aCustInfo->Controller = controller;

	INameSpace^ ns = nullptr;
	DocumentView^ view = nullptr;
	MDocument^ document = nullptr;
	
	try
	{
		//Se qui il controller e' ancora nullo, vuol dire che si sta aggiungendo una nuova personalizzazione (add new)
		if (controller == nullptr)
		{
			view = gcnew DocumentView((IntPtr)pView->m_hWnd);
			document = gcnew MDocument ((IntPtr) m_pServerDocument);
			if (isEasyStudioDesigner)
			{
				controller = gcnew DocumentController(document);
				controller->View = view;
			}
			//Creo il namespace nuovo se entro in nuova customizzazione o recupero il namespace da usare se sto 
			//restartando il documento.
			ns = (managedGate->GetCustomizationInfo()->DocumentRestarted)
				? (NameSpace^)managedGate->GetCustomizationInfo()->CustomizationNamespace
				: GetNewCustomizationName(document->Namespace);
		}
		//altrimenti prende le informazioni del controller per passarle in caso di restart del documento e al form editor
		else
		{
			ns = controller->CustomizationNameSpace;
			view = controller->View;
			document = controller->Document;
		}

		//Restarto il documento, questo può essere fatto a causa dell'unload delle customizzazioni e successivo load di solo 
		//quella da customizzazare o semplicemente perchè è necessario disabilitare l'algoritmo di centratura
		//se sto creando un nuovo documento, non devo farlo ripartire: non ho altre customizzazioni né controlli centrati
		if	(!managedGate->GetCustomizationInfo()->DocumentRestarted && !isEasyStudioDesigner)
		{
				InitialMessage();
				managedGate->GetCustomizationInfo()->IsEasyStudioDesigner = isEasyStudioDesigner;
				managedGate->OnRestartDocument(nullptr, gcnew RestartEventArgs(ns, document->Namespace, Microarea::EasyBuilder::RestartAction::RestartAndGoInEdit, m_bNewDocument));
				managedGate->GetCustomizationInfo()->DocumentRestarted = true;
				return;
		}
	
		//Il restart è finito, siamo in edit del documento restartato
		managedGate->GetCustomizationInfo()->DocumentRestarted = false;
		if (m_pServerDocument->GetMasterFrame())
			m_pServerDocument->GetMasterFrame()->GetDockPane()->DestroyPanes();
		managedGate->SetDocumentTitle(ns);

		formEditor = gcnew FormEditor(isEasyStudioDesigner);
		formEditor->Disposed += gcnew EventHandler(managedGate, &CDManagedGate::OnFormEditorDisposed);
		//impedisco il reset del titolo del documento
		if (!isEasyStudioDesigner)
			formEditor->DirtyChanged +=  gcnew EventHandler<DirtyChangedEventArgs^>(managedGate, &CDManagedGate::OnFormEditorDirtyChanged);
		formEditor->RestartDocument += gcnew EventHandler<RestartEventArgs^>(managedGate, &CDManagedGate::OnRestartDocument);
		if (!ToolBox::HasToolboxItems)
			ToolBox::SetToolBoxItems(GetToolBoxItems());
	
		formEditor->Show
			(
				ns,
				document,
				view,
				controller,
				controllers,
				panel,
				m_bNewDocument
			);
	}
	finally
	{
		//se il controller è nullo, allora ho creato una view e un document di appoggio solo
		//per generare l'object model, quindi li devo ripulire
		if (controller == nullptr)
		{
			delete document;
			delete view;
		}
	}
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::InitialMessage()
{
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::AddItem(IList<TbToolBoxItem^>^ items, System::String^ caption, System::Type^ familyType)
{
	TbToolBoxItem^ item = gcnew TbToolBoxItem();
	item->Caption = caption;
	item->ControlType = familyType;

	item->Bitmap = GetBitmapFromType(familyType);
	items->Add(item);
}

//-----------------------------------------------------------------------------
IList<TbToolBoxItem^>^ CDEasyBuilder::GetToolBoxItems()
{
	IList<TbToolBoxItem^>^ items = gcnew List<TbToolBoxItem^>(); 
	
	// assemblies
	CStringArray arAssembles;
	AfxGetParsedControlsRegistry()->GetRegisteredAssemblies (arAssembles);
	for (int i=0; i <= arAssembles.GetUpperBound(); i++)
	{
		String^ assemblyName = gcnew String(arAssembles.GetAt(i));

		CObArray arFamilies;
		AfxGetParsedControlsRegistry()->GetRegisteredFamilies(arAssembles.GetAt(i), arFamilies);
		// families grouped by assemblies can be devided into visual groups
		for (int f = 0; f <= arFamilies.GetUpperBound(); f++)
		{
			CParsedCtrlFamily* pFamily = (CParsedCtrlFamily*) arFamilies.GetAt(f);

			Type^ familyType = Type::GetType(gcnew String(pFamily->GetQualifiedTypeName()));
			if (familyType == nullptr || (familyType == MTilePanelTab::typeid || familyType == MTilePanel::typeid))
				continue;

			AddItem(items, gcnew String(pFamily->GetCaption()), familyType);
			
		}
	}
	// questa sezione di codice non deve essere parsata dal TBLocalizer, si tratta di oggetti programmativi, poi decideremo se tradurli
	AddItem(items, gcnew String(_T("Generic Button")),		GenericPushButton::typeid);
	AddItem(items, gcnew String(_T("Generic ListBox")),		GenericListBox::typeid);
	AddItem(items, gcnew String(_T("Generic CheckBox")),	GenericCheckBox::typeid);
	AddItem(items, gcnew String(_T("Generic RadioButton")), GenericRadioButton::typeid);
	AddItem(items, gcnew String(_T("Generic Edit")),		GenericEdit::typeid);
	AddItem(items, gcnew String(_T("Generic ComboBox")),	GenericComboBox::typeid);
	AddItem(items, gcnew String(_T("Generic Panel")),		MPanel::typeid);
	AddItem(items, gcnew String(_T("Generic TreeView")),	GenericTreeView::typeid);
	AddItem(items, gcnew String(_T("Generic ListCtrl")),	GenericListCtrl::typeid);
	return items;
}

//-----------------------------------------------------------------------------
Bitmap^ CDEasyBuilder::GetBitmapFromType(Type^ familyType)
{
	ResourceManager^ resources = gcnew ResourceManager(familyType);
	try
	{
		//cerco una risorsa embedded il cui namespace è lo stesso del controllo, con estensione png oppure jpg
		Stream^ s = familyType->Assembly->GetManifestResourceStream(familyType->FullName + ".png");
		if (s == nullptr)
			s = familyType->Assembly->GetManifestResourceStream(familyType->FullName + ".jpg");
		if (s == nullptr)
			s = familyType->Assembly->GetManifestResourceStream("Microarea.Framework.TBApplicationWrapper.Control.png");
		if (s != nullptr)
			return (Bitmap^) Bitmap::FromStream(s);
	}
	catch(Exception ^ e)
	{
		System::Diagnostics::Debug::Fail(e->ToString());
		return nullptr;
	}
	delete resources;
	return nullptr;
}


//-----------------------------------------------------------------------------
void CDEasyBuilder::OnBeforeCloseDocument()
{
	if (documentControllers)
		documentControllers->Dispatch(ManagedClientDocEvent::OnBeforeCloseDocument);

	BusinessObject^ executedBO = GetCaller();
	if (executedBO != nullptr)
		executedBO->DispatchEvent(BOEvent::OnBeforeCloseDocument);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnDocumentCreated()
{
	if (documentControllers)
		documentControllers->Dispatch(ManagedClientDocEvent::OnDocumentCreated);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnFrameCreated()
{
	CustomizationInfo^ aCustInfo = managedGate->GetCustomizationInfo();
	if (aCustInfo != nullptr && aCustInfo->RunFromButton)
		return;

	if (!m_pServerDocument->IsInDesignMode() || IsEditing() || !IsLicenseForEasyBuilderVerified())
		return;

	CView* pView = m_pServerDocument->GetFirstView();
	ASSERT(pView);
	bool isEasyStudioDesigner = TRUE == pView->IsKindOf(RUNTIME_CLASS(CEasyStudioDesignerView));

	//se non sono in modalità designer json, verifico che esista una current app per ospitare i cambiamenti
	if (!isEasyStudioDesigner && !BaseCustomizationContext::CustomizationContextInstance->ExistsCurrentEasyBuilderApp)
	{
		ChooseCustomizationContextAndRunEasyStudio();
		return;
	}

	DockPanel^ panel = CUtility::GetHostingPanel((IntPtr)m_pServerDocument);
	if (panel == nullptr)
		return;

	//Imposto la modalità design per il documento (rende il documento inerte, shortcut comprese)
	SetServerDocumentDesignMode(isEasyStudioDesigner ? CBaseDocument::DM_STATIC : CBaseDocument::DM_RUNTIME);

	if (!documentControllers)
		documentControllers = gcnew DocumentControllers();

	DocumentControllers^ controllers = documentControllers;
	DocumentController^ controller = nullptr;

	int nControllerIdx = 1; // = nCmd - ID_FORM_EDITOR_EDIT - 1;

	//Teniamo da parte il fatto che siamo arrivati da un "Save as new Document" 
	controller = controllers->GetControllerByName(managedGate->GetCustomizationToRun(), true);
	
	INameSpace^ ns = nullptr;
	DocumentView^ view = nullptr;
	MDocument^ document = nullptr;

	try
	{
		//Se qui il controller e' ancora nullo, vuol dire che si sta aggiungendo una nuova personalizzazione (add new)
		if (controller == nullptr)
		{
			view = gcnew DocumentView((IntPtr)pView->m_hWnd);
			document = gcnew MDocument((IntPtr)m_pServerDocument);
			if (isEasyStudioDesigner)
			{
				controller = gcnew DocumentController(document);
				controller->View = view;
			}
			//Creo il namespace nuovo se entro in nuova customizzazione o recupero il namespace da usare se sto 
			//restartando il documento.
			ns = (managedGate->GetCustomizationInfo()->DocumentRestarted)
				? (NameSpace^)managedGate->GetCustomizationInfo()->CustomizationNamespace
				: GetNewCustomizationName(document->Namespace);
		}
		//altrimenti prende le informazioni del controller per passarle in caso di restart del documento e al form editor
		else
		{
			ns = controller->CustomizationNameSpace;
			view = controller->View;
			document = controller->Document;
		}
	
		//Il restart è finito, siamo in edit del documento restartato
		managedGate->GetCustomizationInfo()->DocumentRestarted = false;
		if (m_pServerDocument->GetMasterFrame())
			m_pServerDocument->GetMasterFrame()->GetDockPane()->DestroyPanes();
		managedGate->SetDocumentTitle(ns);

		formEditor = gcnew FormEditor(isEasyStudioDesigner);
		formEditor->Disposed += gcnew EventHandler(managedGate, &CDManagedGate::OnFormEditorDisposed);
		//impedisco il reset del titolo del documento
		if (!isEasyStudioDesigner)
			formEditor->DirtyChanged += gcnew EventHandler<DirtyChangedEventArgs^>(managedGate, &CDManagedGate::OnFormEditorDirtyChanged);
		formEditor->RestartDocument += gcnew EventHandler<RestartEventArgs^>(managedGate, &CDManagedGate::OnRestartDocument);
		if (!ToolBox::HasToolboxItems)
			ToolBox::SetToolBoxItems(GetToolBoxItems());

		formEditor->Show
			(
				ns,
				document,
				view,
				controller,
				controllers,
				panel,
				m_bNewDocument
				);
	}
	finally
	{
		//se il controller è nullo, allora ho creato una view e un document di appoggio solo
		//per generare l'object model, quindi li devo ripulire
		if (controller == nullptr)
		{
			delete document;
			delete view;
		}
	}
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnCloseServerDocument()
{
	if (IsEditing())
		delete formEditor;

	if (documentControllers)
	{
		documentControllers->Dispatch(ManagedClientDocEvent::OnCloseDocument);
		delete documentControllers;
		documentControllers = nullptr;

		if (static_cast<CDManagedGate^>(this->managedGate) != nullptr)
		{
			CustomizationInfo^ custInfo = this->managedGate->GetCustomizationInfo();
			if (
				custInfo != nullptr &&
				custInfo->Controller != nullptr
				)
			{
				FormEditor::RestoreThreadStaticController(custInfo->Controller);
				custInfo->Controller = nullptr;
			}
		}
		
	}
	if (m_pNsDocToRestart)
		SAFE_DELETE(m_pNsDocToRestart);
}
//-----------------------------------------------------------------------------
void CDEasyBuilder::OnBeforeBrowseRecord()
{
	if (documentControllers)
		documentControllers->Dispatch(ManagedClientDocEvent::OnBeforeBrowseRecord);
}
//-----------------------------------------------------------------------------
void CDEasyBuilder::OnGoInBrowseMode()
{
	if (documentControllers)
		documentControllers->Dispatch(ManagedClientDocEvent::OnGoInBrowseMode);

	BusinessObject^ executedBO = GetCaller();
	if (executedBO != nullptr)
		executedBO->DispatchEvent(BOEvent::OnGoInBrowseMode);

}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnOkTransaction()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::OnOkTransaction);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnOkEdit()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::OnOkEdit);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnBeforeOkTransaction()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::OnBeforeOkTransaction);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnOkDelete()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::OnOkDelete);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnOkNewRecord()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::OnOkNewRecord);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::CanDoDeleteRecord()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::CanDoDeleteRecord);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::CanDoEditRecord()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::CanDoEditRecord);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::CanDoNewRecord()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::CanDoNewRecord);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnBeforeDeleteRecord()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::OnBeforeDeleteRecord);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnBeforeEditRecord()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::OnBeforeEditRecord);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnBeforeNewRecord()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::OnBeforeNewRecord);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnBeforeNewTransaction()
{
	if (documentControllers)
		return documentControllers->DispatchEvent(ManagedClientDocEvent::OnBeforeTransaction, gcnew TransactionEventArgs(TransactionEventArgs::TransactionMode::New));

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnBeforeEditTransaction()
{
	if (documentControllers)
		return documentControllers->DispatchEvent(ManagedClientDocEvent::OnBeforeTransaction, gcnew TransactionEventArgs(TransactionEventArgs::TransactionMode::Edit));

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnBeforeDeleteTransaction()
{
	if (documentControllers)
		return documentControllers->DispatchEvent(ManagedClientDocEvent::OnBeforeTransaction, gcnew TransactionEventArgs(TransactionEventArgs::TransactionMode::Delete));

	return TRUE;
} 

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnNewTransaction()
{
	if (documentControllers)
		return documentControllers->DispatchEvent(ManagedClientDocEvent::OnTransaction, gcnew TransactionEventArgs(TransactionEventArgs::TransactionMode::New));

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnEditTransaction()
{
	if (documentControllers)
		return documentControllers->DispatchEvent(ManagedClientDocEvent::OnTransaction, gcnew TransactionEventArgs(TransactionEventArgs::TransactionMode::Edit));

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnDeleteTransaction()
{
	if (documentControllers)
		return documentControllers->DispatchEvent(ManagedClientDocEvent::OnTransaction, gcnew TransactionEventArgs(TransactionEventArgs::TransactionMode::Delete));

	return TRUE;
} 

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnExtraNewTransaction()
{
	if (documentControllers)
		return documentControllers->DispatchEvent(ManagedClientDocEvent::OnExtraTransaction, gcnew TransactionEventArgs(TransactionEventArgs::TransactionMode::New));

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnExtraEditTransaction()
{
	if (documentControllers)
		return documentControllers->DispatchEvent(ManagedClientDocEvent::OnExtraTransaction, gcnew TransactionEventArgs(TransactionEventArgs::TransactionMode::Edit));

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnExtraDeleteTransaction()
{
	if (documentControllers)
		return documentControllers->DispatchEvent(ManagedClientDocEvent::OnExtraTransaction, gcnew TransactionEventArgs(TransactionEventArgs::TransactionMode::Delete));

	return TRUE;
} 

//-----------------------------------------------------------------------------
CAbstractFormDoc::LockStatus CDEasyBuilder::OnLockDocumentForNew()
{
	if (documentControllers)
	{
		LockEventArgs^ e = gcnew LockEventArgs(TransactionEventArgs::TransactionMode::New);
		documentControllers->DispatchEvent(ManagedClientDocEvent::OnLockDocument, e);
		return (CAbstractFormDoc::LockStatus) e->Result;
	}

	return CAbstractFormDoc::ALL_LOCKED;
}

//-----------------------------------------------------------------------------
CAbstractFormDoc::LockStatus CDEasyBuilder::OnLockDocumentForEdit()
{
	if (documentControllers)
	{
		LockEventArgs^ e = gcnew LockEventArgs(TransactionEventArgs::TransactionMode::Edit);
		documentControllers->DispatchEvent(ManagedClientDocEvent::OnLockDocument, e);
		return (CAbstractFormDoc::LockStatus) e->Result;
			
	}

	return CAbstractFormDoc::ALL_LOCKED;
} 

//-----------------------------------------------------------------------------
CAbstractFormDoc::LockStatus CDEasyBuilder::OnLockDocumentForDelete()
{
	if (documentControllers)
	{
		LockEventArgs^ e = gcnew LockEventArgs(TransactionEventArgs::TransactionMode::Delete);
		documentControllers->DispatchEvent(ManagedClientDocEvent::OnLockDocument, e);
		return (CAbstractFormDoc::LockStatus) e->Result;
	}

	return CAbstractFormDoc::ALL_LOCKED;
} 

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnBeforeBatchExecute()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::OnBeforeBatchExecute);

	return TRUE;
}
//-----------------------------------------------------------------------------
void CDEasyBuilder::OnDuringBatchExecute(SqlRecord* pCurrProcessedRecord)
{
	if (!documentControllers)
		return;
	MSqlRecord^ record = gcnew MSqlRecord((IntPtr)pCurrProcessedRecord);
	BatchEventArgs^ args = gcnew BatchEventArgs(record);
	documentControllers->DispatchEvent(args);
	delete record;
}
//-----------------------------------------------------------------------------
void CDEasyBuilder::OnAfterBatchExecute()
{
	if (documentControllers)
		documentControllers->Dispatch(ManagedClientDocEvent::OnAfterBatchExecute);

	BusinessObject^ executedBO = GetCaller();
	if (executedBO != nullptr)
		executedBO->DispatchEvent(BOEvent::OnAfterBatchExecute);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnAttachData()
{
	Attach(new CDEasyBuilderEventManager());
	if (documentControllers)
	{
		Attach(new ControllersEventManager());
		DocumentControllers^ controllers = documentControllers;
		IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
		for each (DocumentController^ controller in controllers)
		{
			if (controller->Document != nullptr)
			{
				EXECUTE_SAFE_CONTROLLER_CODE(controller->Document->CallCreateComponents();)
			}
		}

		if (notWorkingControllers->Count > 0)
		{
			m_pServerDocument->m_pMessages->Show(TRUE);

			int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

			//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
			//disabilitiamo le customizzazioni problematiche.
			if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
				documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
		}
		return documentControllers->Dispatch(ManagedClientDocEvent::OnAttachData);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnPrepareAuxData()
{
	if (documentControllers)
	{
		// prima notifico l'evento di queried ai DBT
		if (m_pServerDocument->m_pDBTMaster)
		{
			DispatchDBTEvent (Queried, m_pServerDocument->m_pDBTMaster, -1, NULL);
			DBTArray* pSlaves = m_pServerDocument->m_pDBTMaster->GetDBTSlaves();
			if (pSlaves)
				for (int i=0; i <= pSlaves->GetUpperBound(); i++)
					DispatchDBTEvent (Queried, pSlaves->GetAt(i), -1, NULL);
		}

		return documentControllers->Dispatch(ManagedClientDocEvent::OnPrepareAuxData);
	}

	BusinessObject^ executedBO = GetCaller();
	if (executedBO != nullptr)
		executedBO->DispatchEvent(BOEvent::OnPrepareAuxData);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnInitAuxData()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::OnInitAuxData);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnPrepareAuxData (CAbstractFormView* pView)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;
	
	__super::OnPrepareAuxData (pView);

	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		if (controller->View == nullptr)
			continue;

		EXECUTE_SAFE_CONTROLLER_CODE(controller->View->OnDataLoaded())
	}

	if (notWorkingControllers->Count == 0)
		return;

	m_pServerDocument->m_pMessages->Show(TRUE);

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnPrepareAuxData (CTabDialog* pTabDialog)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;
	
	__super::OnPrepareAuxData (pTabDialog);

	MTab^ tbTab = nullptr;
	MTabber^ tbTabber = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		if (controller->View == nullptr)
			continue;

		EXECUTE_SAFE_CONTROLLER_CODE(
			tbTabber = controller->View->GetTabberByName(gcnew String(pTabDialog->GetParentTabManager()->GetNamespace().GetObjectName()));
			if (tbTabber == nullptr)
				continue;)

		tbTab = tbTabber->GetTabByName(gcnew String(pTabDialog->GetFormName()));
		if (tbTab != nullptr && tbTab->Handle != IntPtr::Zero)
		{
			EXECUTE_SAFE_CONTROLLER_CODE(tbTab->OnDataLoaded())
		}
	}

	if (notWorkingControllers->Count == 0)
		return;

	m_pServerDocument->m_pMessages->Show(TRUE);

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnDisableControlsAlways (CTabDialog* pTabDialog)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;
	
	__super::OnDisableControlsAlways (pTabDialog);

	MTab^ tbTab = nullptr;
	MTabber^ tbTabber = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		if (controller->View == nullptr)
			continue;

		EXECUTE_SAFE_CONTROLLER_CODE(
			tbTabber = controller->View->GetTabberByName(gcnew String(pTabDialog->GetParentTabManager()->GetNamespace().GetObjectName()));
			if (tbTabber == nullptr)
				continue;
			)

		tbTab = tbTabber->GetTabByName(gcnew String(pTabDialog->GetFormName()));
		if (tbTab != nullptr && tbTab->Handle != IntPtr::Zero)
		{
			EXECUTE_SAFE_CONTROLLER_CODE(
				tbTab->OnControlsEnabled();
			)
		}
	}

	if (notWorkingControllers->Count == 0)
		return;

	m_pServerDocument->m_pMessages->Show(TRUE);

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnExistTables()
{
	if (documentControllers)
		return documentControllers->Dispatch(ManagedClientDocEvent::OnExistTables);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnInitDocument()
{
	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));
	m_bNewDocument = m_pServerDocument->IsKindOf(RUNTIME_CLASS(CNewDocument)) == TRUE;
	//siccome la parte managed non ha coda di messaggi, mi occorre questo trucco per 
	//scatenare la OnIdle necessaria al funzionamanto della propertygrid
	//altrimenti se faccio doppio click su un metodo non mi chiama il metodo showcode
	AfxGetThreadContext()->AddOnIdleHandler(&CDEasyBuilder::OnIdleHandler);

	INameSpace^ serverDocumentNamespace = gcnew NameSpace (gcnew String(m_pServerDocument->GetNamespace().ToString()));

	//se ho applicato dei cambiamenti al catalog in memoria nella sessione precedente
	//e li ho utilizzati nell'object model, uscendo e rientrando potrebbe non funzionare più
	//quindi devo per prima cosa applicare gli stessi cambiamenti (che mi ero salvato su file system)
	DatabaseChangesCurrentRelease::UpdateCatalogIfNeeded();

	if (!Microarea::EasyBuilder::ApplicationsChangesLoader::Loaded)
	{
		BaseCustomizationContext::ApplicationChanges->AddRange(Microarea::EasyBuilder::ApplicationsChangesLoader::Instance->Changes);
	}

	managedGate = gcnew CDManagedGate(m_pServerDocument, this);

	CManagedDocComponentObj* pParams = m_pServerDocument->GetManagedParameters();
	CRestartDocumentInvocationInfo* pInfo = dynamic_cast<CRestartDocumentInvocationInfo*>(pParams);
	if (pInfo)
	{
		if (!pInfo->GetCustomizationName().IsEmpty())
			managedGate->SetCustomizationToRun(gcnew String(pInfo->GetCustomizationName()));
			
		// controllo se sto arrivando come opzione dal menu manager e quindi sono già in DesignMode e NON passerò per la OnEditForm
		// ma bensì dal codice della OnFrameCreated. 
		if (managedGate->GetCustomizationInfo() && !managedGate->GetCustomizationInfo()->RunFromButton && m_pServerDocument->IsInDesignMode())
		{
			// editing delle customizzazioni esistenti
			if (managedGate->GetCustomizationInfo()->DocumentNamespace == nullptr || String::IsNullOrEmpty(managedGate->GetCustomizationInfo()->DocumentNamespace->FullNameSpace))
				managedGate->GetCustomizationInfo()->DocumentNamespace = serverDocumentNamespace;

			// questa dovrebbe essere nuova customizzazione che non deve 
			// eseguire la LoadAssembliesForEdit e deve prendere il nuovo namespace
			if (pInfo->GetCustomizationName().IsEmpty())
				return TRUE;

			if (managedGate->GetCustomizationInfo()->CustomizationNamespace == nullptr || String::IsNullOrEmpty(managedGate->GetCustomizationInfo()->CustomizationNamespace->FullNameSpace))
				managedGate->GetCustomizationInfo()->CustomizationNamespace = gcnew NameSpace(serverDocumentNamespace->GetNameSpaceWithoutType() + "." + gcnew String(pInfo->GetCustomizationName()), NameSpaceObjectType::Customization);
		
			// quella esistente invece deve caricare solo l'esistente
			managedGate->GetCustomizationInfo()->DocumentRestarted = true;
			managedGate->GetCustomizationInfo()->LoadAllCustomizations = false;
		}
	}

	try
	{
		documentControllers = gcnew DocumentControllers();
		IntPtr viewHandle = IntPtr::Zero;
		if (m_pServerDocument->GetFirstView())
			viewHandle = (IntPtr) m_pServerDocument->GetFirstView()->m_hWnd;

		CBusinessObjectInvocationInfo* pBOParams = dynamic_cast<CBusinessObjectInvocationInfo*>(pParams);
		if (managedGate->GetCustomizationInfo()->LoadAllCustomizations || (pBOParams && pBOParams->IsExposing()))
			documentControllers->LoadEasyBuilderApps(
				serverDocumentNamespace,
				(IntPtr) m_pServerDocument, 
				viewHandle
				);
		
		else
		{
			documentControllers->LoadAssembliesForEdit
				(
					(IntPtr)m_pServerDocument,
					(IntPtr)m_pServerDocument->GetFirstView()->m_hWnd,
					managedGate->GetCustomizationInfo()->DocumentNamespace,
					managedGate->GetCustomizationInfo()->CustomizationNamespace
					);
		}

		if (!documentControllers->WereThereLoadingTroubles)
		{
			// per poter eseguire gli eventi di inizializzazione devo inizializzare il 
			// controller in modo da far agganciare gli eventi, ma per i DBT e' troppo 
			// presto, quindi avviso la OnAttachData dovrà completare la creazione dei components

			DocumentControllers^ controllers = documentControllers;
			IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
			for each (DocumentController^ controller in controllers)
			{
				if (controller->Document != nullptr)
				{
					controller->Document->StartDuringInitDocument();
					EXECUTE_SAFE_CONTROLLER_CODE(controller->CreateComponents();)
					controller->Document->EndDuringInitDocument();
				}
			}

			return documentControllers->Dispatch(ManagedClientDocEvent::OnInitDocument);
		}

		int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)documentControllers->WrongAssembliesName);

		//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
		//disabilitiamo le customizzazioni problematiche.
		if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
			documentControllers->DisableNotWorkingAssemblies(documentControllers->WrongAssembliesName);
	}
	finally
	{
		if (documentControllers->Count == 0)
		{
			delete documentControllers;//lo imposto a null per motivi di efficienza
			documentControllers = nullptr;
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BusinessObject^ CDEasyBuilder::GetCaller()
{
	BusinessObject^ executedBO = caller;
	if (executedBO == nullptr)
	{ 
		CManagedDocComponentObj* pParams = m_pServerDocument->GetManagedParameters();
		CBusinessObjectInvocationInfo* pInfo = dynamic_cast<CBusinessObjectInvocationInfo*>(pParams);
		if (pInfo && pInfo->GetCallerComponent())
		{
			Microarea::TaskBuilderNet::Core::EasyBuilder::EasyBuilderComponent^ component = pInfo->GetCallerComponent()->Component;
			executedBO = dynamic_cast<BusinessObject^>(component);
			if (executedBO != nullptr)
				caller = executedBO;
		}
	}
	return executedBO;
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnModifyDBTDefineQuery	(DBTObject* pDbt, SqlTable* pTable)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;

	MDBTObject^ dbt = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		EXECUTE_SAFE_CONTROLLER_CODE
		(
			dbt = controller->Document->GetDBT((IntPtr)pDbt);
			if (dbt == nullptr)
				continue;
			dbt->DefineQuery (dbt->Table);
		)
	}

	if (notWorkingControllers->Count == 0)
		return;

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}
//-----------------------------------------------------------------------------
void CDEasyBuilder::OnModifyDBTPrepareQuery	(DBTObject* pDbt, SqlTable* pTable)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;

	MDBTObject^ dbt = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		EXECUTE_SAFE_CONTROLLER_CODE
		(
			dbt = controller->Document->GetDBT((IntPtr)pDbt);
			if (dbt == nullptr)
				continue;
			
			dbt->PrepareQuery (dbt->Table);
		)
	}

	if (notWorkingControllers->Count == 0)
		return;

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnPrepareBrowser (SqlTable* pTable)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;

	MDBTMaster^ dbt = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		EXECUTE_SAFE_CONTROLLER_CODE
		(
			dbt = (MDBTMaster^) controller->Document->Master;
			if (dbt != nullptr)
			{
				MSqlTable^ mTable = gcnew MSqlTable((IntPtr) pTable);
				dbt->PrepareBrowser (mTable);
				delete mTable;
			}
		)
	}

	if (notWorkingControllers->Count == 0)
		return;

	m_pServerDocument->m_pMessages->Show(TRUE);

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::DispatchDBTEvent (DBTEvent theEvent, DBTObject* pDbt, int nRow, SqlRecord* pRecord)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return TRUE;

	MDBTObject^ dbt = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		EXECUTE_SAFE_CONTROLLER_CODE(
			dbt = controller->Document->GetDBT((IntPtr)pDbt);
			if (dbt == nullptr)
				continue;

			switch (theEvent)
			{
			case Queried: 
				dbt->OnQueried ();
				break;
			case PreparePrimaryKey: 
				dbt->ReceivePreparePrimaryKey (nRow, (IntPtr) pRecord);
				break;
			case PrepareRow: 
				if (dbt->GetType()->IsSubclassOf(MDBTSlaveBuffered::typeid))
					((MDBTSlaveBuffered^) dbt)->ReceivePrepareRow (nRow, (IntPtr) pRecord);
				break;
			case BeforeAddRow: 
				if (dbt->GetType()->IsSubclassOf(MDBTSlaveBuffered::typeid))
					return ((MDBTSlaveBuffered^) dbt)->ReceiveBeforeAddRow (nRow);
			case AfterAddRow: 
				if (dbt->GetType()->IsSubclassOf(MDBTSlaveBuffered::typeid))
					((MDBTSlaveBuffered^) dbt)->ReceiveAfterAddRow (nRow, (IntPtr) pRecord);
				break;
			case BeforeInsertRow: 
				if (dbt->GetType()->IsSubclassOf(MDBTSlaveBuffered::typeid))
					return ((MDBTSlaveBuffered^) dbt)->ReceiveBeforeInsertRow (nRow);
			case AfterInsertRow: 
				if (dbt->GetType()->IsSubclassOf(MDBTSlaveBuffered::typeid))
					((MDBTSlaveBuffered^) dbt)->ReceiveAfterInsertRow (nRow, (IntPtr) pRecord);
				break;
			case BeforeDeleteRow: 
				if (dbt->GetType()->IsSubclassOf(MDBTSlaveBuffered::typeid))
					return ((MDBTSlaveBuffered^) dbt)->ReceiveBeforeDeleteRow (nRow, (IntPtr) pRecord);
			case AfterDeleteRow: 
				if (dbt->GetType()->IsSubclassOf(MDBTSlaveBuffered::typeid))
					((MDBTSlaveBuffered^) dbt)->ReceiveAfterDeleteRow (nRow);
				break;
			case SetCurrentRow: 
				if (dbt->GetType()->IsSubclassOf(MDBTSlaveBuffered::typeid))
					((MDBTSlaveBuffered^) dbt)->ReceiveSetCurrentRow (nRow);
				break;
			case PrepareAuxColumns: 
				if (dbt->GetType()->IsSubclassOf(MDBTSlaveBuffered::typeid))
					((MDBTSlaveBuffered^) dbt)->ReceivePrepareAuxColumns (nRow, (IntPtr) pRecord);
				break;
			}
			)
	}

	if (notWorkingControllers->Count == 0)
		return TRUE;

	m_pServerDocument->m_pMessages->Show(TRUE);

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnPreparePrimaryKey (DBTObject* pDbt)
{
	DispatchDBTEvent (PreparePrimaryKey, pDbt, -1, NULL);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnPreparePrimaryKey (DBTSlaveBuffered* pDbt, int nRow, SqlRecord* pRecord)
{
	DispatchDBTEvent (PreparePrimaryKey, pDbt, nRow, pRecord);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnPrepareRow (DBTSlaveBuffered* pDbt, int nRow, SqlRecord* pRecord)
{
	DispatchDBTEvent (PrepareRow, pDbt, nRow, pRecord);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnBeforeAddRow (DBTSlaveBuffered* pDbt, int nRow)
{ 
	return DispatchDBTEvent (BeforeAddRow, pDbt, nRow, nullptr);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnAfterAddRow (DBTSlaveBuffered* pDbt, int nRow, SqlRecord* pRecord)
{
	DispatchDBTEvent (AfterAddRow, pDbt, nRow, pRecord);
}
//-----------------------------------------------------------------------------
void CDEasyBuilder::OnSetCurrentRow (DBTSlaveBuffered* pDbt)
{
	DispatchDBTEvent (SetCurrentRow, pDbt, pDbt->GetCurrentRowIdx(), nullptr);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnBeforeInsertRow (DBTSlaveBuffered* pDbt, int nRow)	
{ 
	return DispatchDBTEvent (BeforeInsertRow, pDbt, nRow, nullptr);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnAfterInsertRow (DBTSlaveBuffered* pDbt, int nRow, SqlRecord* pRecord)
{
	DispatchDBTEvent (AfterInsertRow, pDbt, nRow, pRecord);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnAfterDeleteRow (DBTSlaveBuffered* pDbt, int nRow)
{
	DispatchDBTEvent (AfterDeleteRow, pDbt, nRow, nullptr);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnBeforeDeleteRow (DBTSlaveBuffered* pDbt, int nRow) 
{ 
	return DispatchDBTEvent (BeforeDeleteRow, pDbt, nRow, pDbt->GetRow(nRow));
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnPrepareAuxColumns	(DBTSlaveBuffered* pDbt, SqlRecord* pSqlRecord)
{
	DispatchDBTEvent (PrepareAuxColumns, pDbt, -1, pSqlRecord);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnRunReport (CWoormInfo* pInfo)
{
	if (!documentControllers)
		return TRUE;
	
	WoormEventArgs^ args = gcnew WoormEventArgs(gcnew MWoormInfo((IntPtr)pInfo));
	documentControllers->DispatchEvent(args);
	return !args->Cancel;
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::IsLicenseForEasyBuilderVerified()
{
	CDiagnostic *pDiag = AfxGetDiagnostic();
	try
	{
		EBLicenseManager::DemandLicenseForEasyStudio(gcnew String(AfxGetAuthenticationToken()));
	}
	catch (NotEasyStudioDeveloperLicenseException^ ebdExc)
	{
		if (!pDiag)
			return FALSE;
		pDiag->Add (ebdExc->Message, CDiagnostic::Warning);
		pDiag->Show ();

		return FALSE;
	}
	catch (EasyStudioCalSoldOutLicenseException^ calExc)
	{
		pDiag->Add (calExc->Message, CDiagnostic::Warning);
		pDiag->Show ();

		return FALSE;
	}
	catch (Exception^ exc)
	{
		pDiag->Add (exc->Message, CDiagnostic::Error);
		pDiag->Show ();

		return FALSE;
	}

	return TRUE;
}


//--------------------------------------------------------------------------------------------------------------------------------
void CDEasyBuilder::OnDisableControlsForAddNew ()
{
	if (documentControllers)
		documentControllers->Dispatch(ManagedClientDocEvent::OnDisableControlsForAddNew);
}
//--------------------------------------------------------------------------------------------------------------------------------
void CDEasyBuilder::OnDisableControlsForBatch()
{
	if (documentControllers)
		documentControllers->Dispatch(ManagedClientDocEvent::OnDisableControlsForBatch);
}
//--------------------------------------------------------------------------------------------------------------------------------
void CDEasyBuilder::OnDisableControlsForEdit ()
{
	if (documentControllers)
		documentControllers->Dispatch(ManagedClientDocEvent::OnDisableControlsForEdit);
}

//--------------------------------------------------------------------------------------------------------------------------------
void CDEasyBuilder::OnEnableControlsForFind	()
{
	if (documentControllers)
		documentControllers->Dispatch(ManagedClientDocEvent::OnEnableControlsForFind);
}

//--------------------------------------------------------------------------------------------------------------------------------
void CDEasyBuilder::OnDisableControlsAlways	()
{
	if (documentControllers)
		documentControllers->Dispatch(ManagedClientDocEvent::OnDisableControlsAlways);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnModifyHKLDefineQuery	(HotKeyLink* pHotLink, SqlTable* pTable, HotKeyLink::SelectionType aSelectionType/*HotKeyLink::DIRECT_ACCESS*/)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;

	MHotLink^ hotLink = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		EXECUTE_SAFE_CONTROLLER_CODE
			(
				hotLink = controller->Document->GetWrappedHotLink((IntPtr) pHotLink);
				if (hotLink != nullptr && !hotLink->HasCodeBehind && !hotLink->IsDynamic)
				{
					// okkio che qui devo passare per forza il record della wrapper e non l'hotlink originale C++
					// perchè quello nel frattempo ha sostituito il puntatore e punta a quello nuovo
					MHotLinkTable^ mTable = gcnew MHotLinkTable((IntPtr) pTable, ((MSqlRecord^)hotLink->Record));
					hotLink->OnDefineSearchQuery (mTable, aSelectionType);
					delete mTable;
				}
			)
	}

	if (notWorkingControllers->Count == 0)
		return;

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnModifyHKLPrepareQuery	(HotKeyLink* pHotLink, SqlTable* pTable, DataObj* pDataObj, HotKeyLink::SelectionType aSelectionType/*HotKeyLink::DIRECT_ACCESS*/)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;

	MHotLink^ hotLink = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		EXECUTE_SAFE_CONTROLLER_CODE
			(
				hotLink = controller->Document->GetWrappedHotLink((IntPtr) pHotLink);
				if (hotLink != nullptr && !hotLink->HasCodeBehind && !hotLink->IsDynamic)
				{
					// okkio che qui devo passare per forza il record della wrapper e non l'hotlink originale C++
					// perchè quello nel frattempo ha sostituito il puntatore e punta a quello nuovo
					MHotLinkTable^ mTable = gcnew MHotLinkTable((IntPtr) pTable, (MSqlRecord^) hotLink->Record);
					hotLink->PrepareFilterQuery (mTable, aSelectionType);
					delete mTable;
				}
			)
	}

	if (notWorkingControllers->Count == 0)
		return;

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnHKLIsValid (HotKeyLink* pHotLink)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return TRUE;

	MHotLink^ hotLink = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		EXECUTE_SAFE_CONTROLLER_CODE
			(
				hotLink = controller->Document->GetWrappedHotLink((IntPtr) pHotLink);
				if (hotLink != nullptr)
					return hotLink->ReceiveValidate ((IntPtr) pHotLink->GetAttachedRecord());
			)
	}

	if (notWorkingControllers->Count == 0)
		return TRUE;

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);

	return TRUE;
}


//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnValidateRadarSelection (SqlRecord* pRec, CTBNamespace nsHotLinkNamespace, HotKeyLink* pHotKeyLink)
{
	return OnValidateRadarSelection(pRec, pHotKeyLink);
}

//-----------------------------------------------------------------------------
BOOL CDEasyBuilder::OnValidateRadarSelection (SqlRecord* pRec, HotKeyLink* pHotKeyLink)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return TRUE;

	MHotLink^ hotLink = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		EXECUTE_SAFE_CONTROLLER_CODE
			(
				hotLink = controller->Document->GetWrappedHotLink((IntPtr) pHotKeyLink);
				if (hotLink != nullptr)
					return hotLink->ReceiveValidate ((IntPtr) pRec);
			)
	}

	if (notWorkingControllers->Count == 0)
		return TRUE;

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);

	return TRUE;
}

//-----------------------------------------------------------------------------
CManagedDocComponentObj* CDEasyBuilder::GetComponent (CString& sParentNamespace, CString& sName)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return NULL;

	System::ComponentModel::IComponent^ component = nullptr;
	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	for each (DocumentController^ controller in controllers)
	{
		EXECUTE_SAFE_CONTROLLER_CODE
			(
				if (controller->CustomizationNameSpace->FullNameSpace != gcnew String(sParentNamespace))
					continue;

				component = controller->Document->Components[gcnew String(sName)];
				if (component != nullptr && component->GetType()->IsSubclassOf(EasyBuilderComponent::typeid))
				{
					CBusinessObjectComponent* pObj = new CBusinessObjectComponent();
					pObj->Component = (EasyBuilderComponent^) component;
					return pObj;
				}
			)
	}

	if (notWorkingControllers->Count == 0)
		return NULL;

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);

	return NULL;
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::GetComponents (CManagedDocComponentObj* pRequest, ::Array& returnedComponens)
{
	if (!pRequest || !pRequest->IsKindOf(RUNTIME_CLASS(CBusinessObjectComponentRequest)))
		return;
	
	CBusinessObjectComponentRequest* pTypedRequest = (CBusinessObjectComponentRequest*) pRequest;

	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;
	System::Collections::IList^ list = pTypedRequest->Types;
	System::Collections::ArrayList^ requestedTypes = (System::Collections::ArrayList^) list;
	List<Type^>^ types = gcnew List<Type^>();
	for each (Type^ type in requestedTypes)
		types->Add(type);

	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();
	System::Collections::Generic::List<EasyBuilderComponent^>^ components = gcnew System::Collections::Generic::List<EasyBuilderComponent^>();
	for each (DocumentController^ controller in controllers)
	{
		EXECUTE_SAFE_CONTROLLER_CODE 
			(
				controller->Document->GetEasyBuilderComponents(types, components)
			)
	}

	for each (EasyBuilderComponent^ ebComponent in components)
	{
		CBusinessObjectComponent* pObj = new CBusinessObjectComponent();
		pObj->Component = ebComponent;
		returnedComponens.Add (pObj);
	}

	if (notWorkingControllers->Count == 0)
		return;

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}

//-----------------------------------------------------------------------------
void CDEasyBuilder::OnBuildingSecurityTree (CTBTreeCtrl* pTree, ::Array* pInfoTreeItems)
{
	DocumentControllers^ controllers = documentControllers;
	if (controllers == nullptr)
		return;

	IList<DocumentController^>^ notWorkingControllers = gcnew List<DocumentController^>();

	for each (DocumentController^ controller in controllers)
	{
		EXECUTE_SAFE_CONTROLLER_CODE 
			(
				if (controller->View != nullptr)
					controller->View->OnBuildingSecurityTree((IntPtr) pTree, (IntPtr) pInfoTreeItems);
			)
	}


	if (notWorkingControllers->Count == 0)
		return;

	int msgBoxRes = AskForDisablingNotWorkingControllers((System::Collections::ICollection^)notWorkingControllers);

	//Se siamo in unattended mode oppure l'utente ci ha dato il consenso a farlo allora
	//disabilitiamo le customizzazioni problematiche.
	if (msgBoxRes == NO_MSG_BOX_SHOWN || msgBoxRes == IDYES)
		documentControllers->UnloadAndDisableNotWorkingControllers(notWorkingControllers);
}


