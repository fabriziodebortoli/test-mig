#include "StdAfx.h"

#include <afxsock.h>

#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\Templates.h>


#include <TbGeneric\TBThemeManager.h>

#include "WndObjDescription.h"
#include "Schedule.h"
#include "Globals.h"
#include "LocalizableObjs.h"
#include "wndobjdescription.h"
#include "JsonFormEngine.h"
#include "localizableobjs.h"
#include "Linefile.h"

#include "dialogs.hjson" //JSON AUTOMATIC UPDATE
#include "SettingsTable.h"
#include "ParametersSections.h"

#include "begincpp.dex"

//-----------------------------------------------------------------------------
void AddToBuffer(long number, BYTE* buffer, int& idx)
{
	memcpy(buffer + idx, &number, 4);
	idx += 4;
}
//-----------------------------------------------------------------------------
void AddToBuffer(CString text, BYTE* buffer, int& idx)
{
	CStringA textA = UnicodeToUTF8(text);
	long offset = textA.GetLength();
	AddToBuffer(offset, buffer, idx);
	memcpy(buffer + idx, textA.GetBuffer(), offset);
	idx += offset;
}
//-----------------------------------------------------------------------------
HINSTANCE GetResourceModule(CWnd* pWnd)
{
	if (pWnd->IsKindOf(RUNTIME_CLASS(CLocalizableDialog)))
		return ((CLocalizableDialog*)pWnd)->GetResourceModule();
	if (pWnd->IsKindOf(RUNTIME_CLASS(CLocalizablePropertyPage)))
		return ((CLocalizablePropertyPage*)pWnd)->GetResourceModule();
	if (pWnd->IsKindOf(RUNTIME_CLASS(CLocalizablePropertySheet)))
		return ((CLocalizablePropertySheet*)pWnd)->GetResourceModule();
	return GetDllInstance(pWnd->GetRuntimeClass());
}
//-----------------------------------------------------------------------------
LRESULT GetLocalizerInfo(WPARAM wParam, LPARAM lParam)
{
	if (!wParam || !lParam)
		return -1;
#define BUFF_SIZE 1024

	union
	{
		BYTE buffer[BUFF_SIZE];
		POINT p;
	} mem;
	SIZE_T sz;
	if (!ReadProcessMemory(GetCurrentProcess(), (LPCVOID)wParam, &mem, BUFF_SIZE, &sz))
		return -1;

	CWnd* pWnd = CWnd::WindowFromPoint(mem.p);
	if (!pWnd) return -1;

	pWnd = GetWindowControl(pWnd, mem.p);
	if (!pWnd) return -1;

	int idx = 0;
	AddToBuffer(AfxGetCulture(), mem.buffer, idx);

	CString windowText;
	pWnd->GetWindowText(windowText);
	AddToBuffer(windowText, mem.buffer, idx);

	CWnd* pCurrentWnd = pWnd;
	BSTR reportNs = NULL;
	while (reportNs == NULL && pCurrentWnd != NULL)
	{
		reportNs = (BSTR)pCurrentWnd->SendMessage(UM_GET_REPORT_NAMESPACE);
		pCurrentWnd = pCurrentWnd->GetParent();
	}
	CString reportNamespace;
	if (reportNs)
	{
		reportNamespace = reportNs;
		SysFreeString(reportNs);
	}

	AddToBuffer(reportNamespace, mem.buffer, idx);

	long dialogIDD = 0;
	pCurrentWnd = pWnd;
	while (pCurrentWnd != NULL)
	{
		dialogIDD = pCurrentWnd->SendMessage(UM_GET_DIALOG_ID);
		if (dialogIDD)
			break;
		pCurrentWnd = pCurrentWnd->GetParent();
	}
	AddToBuffer(dialogIDD, mem.buffer, idx);

	if (pCurrentWnd)
	{
		//imposto la dll di default in cui cercare le risorse uguale a quella che contiene la definizione della tabdialog
		CSetResourceHandle h(GetResourceModule(pCurrentWnd));

		CStringArray paths;
		AfxGetDictionaryPathsFromID(dialogIDD, RT_DIALOG, paths);
		CString sPaths;
		for (int i = 0; i < paths.GetCount(); i++)
			sPaths.Append(GetName(paths[i]) + _T(";"));
		AddToBuffer(sPaths, mem.buffer, idx);
	}
	if (!WriteProcessMemory(GetCurrentProcess(), (LPVOID)wParam, &mem, BUFF_SIZE, &sz))
		return -1;
	return idx;
}
//-----------------------------------------------------------------------------
CWnd* GetSafeParent(CWnd* pParent)
{
	HWND hwndModal = AfxGetThreadContext()->GetCurrentModalWindow();
	if (hwndModal)
		return CWnd::FromHandlePermanent(hwndModal);

	pParent = CWnd::FromHandlePermanent(CWnd::GetSafeOwner_(pParent->GetSafeHwnd(), NULL));
	if (pParent)
	{
		BOOL bChildManagement = *((DataBool*)AfxGetSettingValue(snsTbGenlib, szFormsSection, _T("ChildManagement"), DataBool(0)));
		if (pParent->m_hWnd != AfxGetMenuWindowHandle() || !bChildManagement)
		{
			//this window cannot be a parent (for example, AdmFrame cannot be a parent form message boxes)
			while (pParent)
				if (pParent->SendMessage(UM_IS_UNATTENDED_WINDOW, NULL, NULL))
					pParent = pParent->GetParent();
				else
					break;

			if (pParent)
				return pParent;
		}
	}

	pParent = AfxGetThreadContext()->GetActiveDockableWnd();
	if (pParent)
		return pParent;

	pParent = AfxGetMainWnd();
	//this window cannot be a parent (for example, AdmFrame cannot be a parent form message boxes)
	while (pParent)
		if (pParent->SendMessage(UM_IS_UNATTENDED_WINDOW, NULL, NULL))
			pParent = pParent->GetParent();
		else
			break;

	if (pParent)
		return pParent;

	return AfxGetMenuWindow();
}

#ifdef TBWEB

class CPropertySheetDescription : public CWndDialogDescription
{
public:
	CPropertySheetDescription(CWndObjDescription* pParent)
		: CWndDialogDescription(pParent)
	{}

	virtual BOOL SkipWindow(CWnd* pWnd) 
	{
		return	__super::SkipWindow(pWnd) ||
			pWnd->IsKindOf(RUNTIME_CLASS(CPropertyPage)) ||
			pWnd->IsKindOf(RUNTIME_CLASS(CPropertySheet));
	}
};

#else

class CPropertySheetDescription : public CWndObjDescription
{
public:
	CPropertySheetDescription(CWndObjDescription* pParent)
		: CWndObjDescription(pParent)
	{}

	virtual BOOL SkipWindow(CWnd* pWnd)
	{
		return	__super::SkipWindow(pWnd) ||
			pWnd->IsKindOf(RUNTIME_CLASS(CPropertyPage)) ||
			pWnd->IsKindOf(RUNTIME_CLASS(CPropertySheet));
	}
};

#endif

//*****************************************************************************
// CLocalizableDialog dialog
//*****************************************************************************
IMPLEMENT_DYNAMIC(CLocalizableDialog, CBCGPDialog)

BEGIN_MESSAGE_MAP(CLocalizableDialog, CBCGPDialog)

	ON_WM_DESTROY()
	ON_WM_CREATE()
	//toolbar ToolTip
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTW, 0, 0xFFFF, OnToolTipText)
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTA, 0, 0xFFFF, OnToolTipText)

	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	ON_MESSAGE(UM_GET_LOCALIZER_INFO, OnGetLocalizerInfo)

	ON_MESSAGE(WM_INITDIALOG, HandleInitDialog)
	ON_MESSAGE(UM_GET_COMPONENT, OnGetComponent)
	ON_WM_DESTROY()
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CLocalizableDialog::CLocalizableDialog(LPCTSTR lpszTemplateName, CWnd* pParentWnd /*= NULL*/)
	:
	CBCGPDialog			(lpszTemplateName, pParentWnd),

	m_bInOpenDlgCounter	(TRUE),
	m_bIsModal			(FALSE),
	m_nResult			(0),
	m_bAutoDestroy		(FALSE),
	m_hResourceModule	(NULL)
{
	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CLocalizableDialog::CLocalizableDialog(UINT nIDTemplate, CWnd* pParentWnd /*=NULL*/)
	:
	CBCGPDialog			(nIDTemplate, pParentWnd),

	m_bInOpenDlgCounter	(TRUE),
	m_bIsModal			(FALSE),
	m_nResult			(0),
	m_bAutoDestroy		(FALSE),
	m_hResourceModule	(NULL)
{
	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CLocalizableDialog::CLocalizableDialog()
	:
	CBCGPDialog			(),

	m_bInOpenDlgCounter	(TRUE),
	m_bIsModal			(FALSE),
	m_nResult			(0),
	m_bAutoDestroy		(FALSE),
	m_hResourceModule	(NULL)
{
	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CLocalizableDialog::~CLocalizableDialog()
{
#pragma warning( push )
#pragma warning( disable: 4996 )
	delete m_pJsonContext;
#pragma warning( pop )
	
	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//-----------------------------------------------------------------------------
HRESULT CLocalizableDialog::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace = GetRanorexNamespace();
	*pszName = ::SysAllocString(sNamespace);
	return S_OK;
}

//-----------------------------------------------------------------------------
CString CLocalizableDialog::GetRanorexNamespace() 
{ 
	return _T("NO_NAMESPACE"); 
}

//-----------------------------------------------------------------------------
BOOL CLocalizableDialog::ForceFocusToParent()
{
	return m_pParentWnd && AfxGetLoginContext()->GetDocked();
}
//--------------------------------------------------------------------------
HINSTANCE CLocalizableDialog::GetResourceModule()
{
	/*CRuntimeClass* pClass = GetRuntimeClass();
	while (pClass && pClass != RUNTIME_CLASS(CLocalizableDialog))
	{
	HINSTANCE hInst = GetDllInstance(pClass);
	if (::FindResource(hInst, m_lpszTemplateName, RT_DIALOG))
	return hInst;
	pClass = pClass->m_pfnGetBaseClass();
	}*/
	if (m_hResourceModule == NULL)
	{
#ifdef DEBUG
		if (!CJsonFormEngineObj::GetInstance()->IsValid(this))
		{
			ASSERT(FALSE);
			TRACE("La definizione della classe deve usare le macro DECLARE_DYNAMIC o DECLARE_DYNCREATE");
		}
#endif

		m_hResourceModule = GetDllInstance(GetRuntimeClass());
	}
	return m_hResourceModule;
}
//-----------------------------------------------------------------------------
INT_PTR CLocalizableDialog::DoModal()
{
	m_bIsModal = TRUE;

	CTBWinThread::PumpThreadMessages();  //used to process all pending messages in order to fix active window

	CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);
	m_pParentWnd = GetSafeParent(m_pParentWnd);

	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(m_pParentWnd);
	if (pFrame && pFrame->m_bChildManagement)
	{
		for (int i = 0; i < pFrame->GetChilds().GetCount(); i++)
		{
			HWND hwnd = pFrame->GetChilds().GetAt(i);
			if (::IsWindow(hwnd))
				::EnableWindow(hwnd, FALSE);
		}
	}

	CJsonContextObj* pContext = GetJsonContext();
	INT_PTR res = 0;
	//è stata mappata come risorsa dinamica: si tratta di risorsa in file tbjson
	if (pContext)
	{
		m_nIDHelp = REVERSEMAKEINTRESOURCE(m_lpszTemplateName);
		m_lpszTemplateName = NULL;
		AutoDeletePtr<DLGTEMPLATE> pTemplate = (DLGTEMPLATE*)pContext->CreateTemplate(true);
		if (pTemplate)
		{
			InitModalIndirect(pTemplate, m_pParentWnd);
			res = __super::DoModal();
		}
	}
	else
	{
		//imposto la dll di default in cui cercare le risorse uguale a quella che contiene la definizione della tabdialog
		CSetResourceHandle h(GetResourceModule());
		res = __super::DoModal();
	}

	if (pFrame && pFrame->m_bChildManagement)
	{
		for (int i = 0; i < pFrame->GetChilds().GetCount(); i++)
		{
			HWND hwnd = pFrame->GetChilds().GetAt(i);
			if (::IsWindow(hwnd))
				::EnableWindow(hwnd, TRUE);
		}
	}

	//attivo la parent (in tabbed windows quando parte una modale perde il fuoco)
	if (ForceFocusToParent())
		m_pParentWnd->SetFocus();

	if (res == -1 && !AfxGetThreadContext()->IsClosing())
		throw (new CThreadAbortedException());
	return res;
}

// modeless dialog
//------------------------------------------------------------------------------
BOOL CLocalizableDialog::Create(UINT nIDTemplate, CWnd* pParentWnd)
{
	if (!pParentWnd)
		pParentWnd = GetSafeParent(pParentWnd);

	//uso m_nIDHelp come variabile di appoggio per tenermi da parte l'id dinamico originale
	m_nIDHelp = nIDTemplate;

	CJsonContextObj* pContext = GetJsonContext();
	//è stata mappata come risorsa dinamica: si tratta di risorsa in file tbjson
	if (pContext)
	{
		AutoDeletePtr<DLGTEMPLATE> pTemplate = (DLGTEMPLATE*)pContext->CreateTemplate(false);
		return pTemplate && CreateIndirect(pTemplate, pParentWnd);
	}
#ifdef DEBUG
	CJsonResource sJsonId = AfxGetTBResourcesMap()->DecodeID(TbResources, m_nIDHelp);
	if (!sJsonId.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}
#endif

	//imposto la dll di default in cui cercare le risorse uguale a quella che contiene la definizione della tabdialog
	CSetResourceHandle h(GetResourceModule());
	return __super::Create(nIDTemplate, pParentWnd);
}
//-----------------------------------------------------------------------------
CJsonContextObj* CLocalizableDialog::GetJsonContext()
{
#pragma warning( push )
#pragma warning( disable: 4996)
	if (!m_pJsonContext)
	{
		CJsonResource sJsonId = AfxGetTBResourcesMap()->DecodeID(TbResources, m_nIDHelp);
		if (!sJsonId.IsEmpty())
		{
			m_pJsonContext = CJsonFormEngineObj::GetInstance()->CreateContext(sJsonId);
			ASSERT(m_pJsonContext);
		}
	}
	return m_pJsonContext;
#pragma warning( pop )
}

//-----------------------------------------------------------------------------
int CLocalizableDialog::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	int ret = __super::OnCreate(lpCreateStruct);
	

	return ret;
}
//-----------------------------------------------------------------------------
LRESULT CLocalizableDialog::HandleInitDialog(WPARAM, LPARAM)
{
	//prima creo i controli json, poi chiamo la default che chiamerà al OnInitDialog
	//è stata mappata come risorsa dinamica: si tratta di risorsa in file tbjson
	//in questo caso creo i controlli figli PRIMA della OnInitDialog del padre
	//in modo da chiamarla con i controlli già creati
	//non posso crearli nella Create perché è troppo presto, mi chiama la BuildDataControlLinks prima di aver creato i controlli
	//nella OnCreate non funziona ancora la MapDialogRect

	CJsonContextObj* pContext = GetJsonContext();
	if (pContext)
	{
		SetDlgCtrlID(m_nIDHelp);
		m_lpszTemplateName = MAKEINTRESOURCE(m_nIDHelp);
		CJsonFormEngineObj::GetInstance()->CreateChilds(pContext, this);
	}
	Default();

	// We need to notify the current thread about
	// a new window only when window has been fully 
	// created.
	if (m_bIsModal)
		AfxSetThreadInModalState(TRUE, m_hWnd);
	else if (m_bInOpenDlgCounter)
		AfxGetThreadContext()->AddWindowRef(m_hWnd, FALSE);

	return FALSE;
}
//-----------------------------------------------------------------------------
BOOL CLocalizableDialog::OnInitDialog()
{
	__super::OnInitDialog();
	if (AfxIsRemoteInterface())
		ShowWindow(SW_HIDE);
	CJsonContextObj* pContext = GetJsonContext();
	if (!pContext)//se non sono json, devo tradurre, altrimenti le ha già tradotte l'engine json
	{
		//imposto la dll di default in cui cercare le risorse uguale a quella che contiene la definizione della tabdialog
		CSetResourceHandle h(GetResourceModule());
		// localizza le stringhe della finestra
		AfxLoadWindowStrings(this, m_lpszTemplateName);
	}
	// gestisce il conteggio dei riferimenti alle dialog aperte per gestire il ritorno al Menu Manager nel
	// caso in cui non ci sono aperte nè dlg nè doc
	if (m_bInOpenDlgCounter)
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
			pContext->IncreaseOpenDialogs();
	}

	LockParentEnabilitation(TRUE);

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}


//-----------------------------------------------------------------------------
LRESULT CLocalizableDialog::OnGetLocalizerInfo(WPARAM wParam, LPARAM lParam)
{
	return GetLocalizerInfo(wParam, lParam);
}

//-----------------------------------------------------------------------------
LRESULT CLocalizableDialog::OnGetComponent(WPARAM wParam, LPARAM lParam)
{
	CJsonSerializer* pResp = (CJsonSerializer*)wParam;
	pResp->OpenObject(_T("component"));
	TCHAR buff[32];
	_itot_s((int)m_hWnd, buff, 10);
	pResp->WriteString(_T("id"), buff);

	if (m_pParentWnd)
	{
		_itot_s((int)m_pParentWnd->m_hWnd, buff, 10);
		pResp->WriteString(_T("parentId"), buff);
	}
	pResp->WriteBool(_T("modal"), IsModal() == TRUE);
	pResp->WriteString(_T("name"), GetName(GetJsonContext()->m_JsonResource.GetFile()));
	pResp->WriteString(_T("app"), GetJsonContext()->m_JsonResource.GetOwnerNamespace().GetApplicationName());
	pResp->WriteString(_T("mod"), GetJsonContext()->m_JsonResource.GetOwnerNamespace().GetModuleName());

	pResp->CloseObject();
	return 1L;
}

//-----------------------------------------------------------------------------
LRESULT CLocalizableDialog::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	
	CWndDialogDescription* pDesc = (CWndDialogDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndDialogDescription), strId);
	pDesc->m_bIsModal = (m_bIsModal == TRUE);
	pDesc->UpdateAttributes(this);

	pDesc->AddChildWindows(this);
	return (LRESULT)pDesc;

}

//--------------------------------------------------------------------------
void CLocalizableDialog::LockParentEnabilitation(BOOL bLock)
{
	if (!m_bIsModal)
		return;

	CWnd* pParentWnd = GetParent();
	if (!pParentWnd)
		return;

	if (bLock)
		pParentWnd->m_nFlags |= WF_STAYDISABLED;
	else
		pParentWnd->m_nFlags &= ~WF_STAYDISABLED;
}

//-----------------------------------------------------------------------------
BOOL CLocalizableDialog::DestroyWindow()
{
	return __super::DestroyWindow();
}

//--------------------------------------------------------------------------
void CLocalizableDialog::OnDestroy()
{
	__super::OnDestroy();

	if (m_bInOpenDlgCounter)
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
			pContext->DecreaseOpenDialogs();
	}

	if (m_bIsModal)
		AfxSetThreadInModalState(FALSE, m_hWnd);
	else if (m_bInOpenDlgCounter)
		AfxGetThreadContext()->RemoveWindowRef(m_hWnd, FALSE);

}

//--------------------------------------------------------------------------
void CLocalizableDialog::PostNcDestroy()
{
	__super::PostNcDestroy();
	if (m_bAutoDestroy)
		delete this;
}
//--------------------------------------------------------------------------
void CLocalizableDialog::EndDialog(int nResult)
{
	m_nResult = nResult;
	if (m_bIsModal)
	{
		LockParentEnabilitation(FALSE);
		__super::EndDialog(nResult);
	}
	else
		DestroyWindow();
}

//-----------------------------------------------------------------------------
void CLocalizableDialog::OnCancel()
{
	LockParentEnabilitation(FALSE);
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
void CLocalizableDialog::OnOK()
{
	LockParentEnabilitation(FALSE);
	if (!UpdateData(TRUE))
	{
		TRACE(traceAppMsg, 0, "UpdateData failed during dialog termination.\n");
		// the UpdateData routine will set focus to correct item
		return;
	}
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
BOOL CLocalizableDialog::OnToolTipText(UINT nID, NMHDR* pNMHDR, LRESULT* pResult)
{
	ASSERT_TRACE1(pNMHDR->code == TTN_NEEDTEXTA || pNMHDR->code == TTN_NEEDTEXTW, "Wrong pNMHDR->code = %d", pNMHDR->code);

	// allow top level routing frame to handle the message
	if (GetRoutingFrame() != NULL)
		return FALSE;

	return CCommonFunctions::OnToolTipText(nID, pNMHDR, pResult, GetDllInstance(GetRuntimeClass()));    // message was handled
}

//*****************************************************************************
// CLocalizablePropertySheet dialog
//*****************************************************************************
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CLocalizablePropertySheet, CPropertySheet)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CLocalizablePropertySheet, CPropertySheet)
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	ON_MESSAGE(UM_ACTIVATE_TAB_PAGE, OnActivateTabPage)
	ON_MESSAGE(UM_GET_COMPONENT, OnGetComponent)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CLocalizablePropertySheet::CLocalizablePropertySheet()
	: CPropertySheet(),
	m_hResourceModule(NULL)
{
	m_bIsModal = FALSE;
}

//-----------------------------------------------------------------------------
CLocalizablePropertySheet::CLocalizablePropertySheet(UINT nIDCaption, CWnd* pParentWnd, UINT iSelectPage)
	: CPropertySheet(nIDCaption, pParentWnd, iSelectPage),
	m_hResourceModule(NULL)
{
	m_bIsModal = FALSE;
}

//-----------------------------------------------------------------------------
CLocalizablePropertySheet::CLocalizablePropertySheet(LPCTSTR pszCaption, CWnd* pParentWnd, UINT iSelectPage)
	: CPropertySheet(pszCaption, pParentWnd, iSelectPage),
	m_hResourceModule(NULL)
{
	m_bIsModal = FALSE;
}

//-----------------------------------------------------------------------------
CLocalizablePropertySheet::CLocalizablePropertySheet(UINT nIDCaption, CWnd* pParentWnd,
	UINT iSelectPage, HBITMAP hbmWatermark, HPALETTE hpalWatermark, HBITMAP hbmHeader)
	: CPropertySheet(nIDCaption, pParentWnd, iSelectPage, hbmWatermark, hpalWatermark, hbmHeader),
	m_hResourceModule(NULL)
{
	m_bIsModal = FALSE;
}

//-----------------------------------------------------------------------------
CLocalizablePropertySheet::CLocalizablePropertySheet(LPCTSTR pszCaption, CWnd* pParentWnd,
	UINT iSelectPage, HBITMAP hbmWatermark, HPALETTE hpalWatermark, HBITMAP hbmHeader)
	: CPropertySheet(pszCaption, pParentWnd, iSelectPage, hbmWatermark, hpalWatermark, hbmHeader),
	m_hResourceModule(NULL)
{
	m_bIsModal = FALSE;
}

//------------------------------------------------------------------------------
LRESULT CLocalizablePropertySheet::OnActivateTabPage(WPARAM wParam, LPARAM)
{
	CPropertyPage* pPage;
	for (int i = 0; i < GetPageCount(); i++)
	{
		pPage = GetPage(i);
		if (wParam == pPage->GetDlgCtrlID() || (HWND)wParam == pPage->m_hWnd)
		{
			SetActivePage(pPage);
			break;
		}
	}

	return 0L;
}

//-----------------------------------------------------------------------------
LRESULT CLocalizablePropertySheet::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulal base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CPropertySheetDescription* pDesc = (CPropertySheetDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CPropertySheetDescription), strId);
	pDesc->UpdateAttributes(this);
	pDesc->m_Type = CWndObjDescription::PropertyDialog;
	((CPropertySheetDescription*)pDesc)->m_bIsModal = ((GetExStyle() & WS_EX_DLGMODALFRAME) == WS_EX_DLGMODALFRAME); 

	CWndObjDescription *pTabberDesc = pDesc->m_Children.GetWindowDescription(GetTabControl(), RUNTIME_CLASS(CWndObjDescription));
	pTabberDesc->UpdateAttributes(GetTabControl());
	pTabberDesc->m_Type = CWndObjDescription::Tabber;
	pTabberDesc->m_Children.SetSortable(FALSE);


	//prima aggiunco le figlie di tipo CLocalizablePropertyPage
	CPropertyPage* pActive = GetActivePage();
	for (int i = 0; i < GetPageCount(); i++)
	{
		CLocalizablePropertyPage * pDialog = (CLocalizablePropertyPage *)GetPage(i);
		pDialog->GetControlStructure(&pTabberDesc->m_Children, pActive == pDialog, m_hWnd);
	}

	//ciclo sulle finestre figlie per ottenere la descrizione, ma devo saltare 
	//le CLocalizablePropertyPage che ho aggiunto nel ciclo precedente
	CWnd *pChild = this->GetWindow(GW_CHILD);
	while (pChild)
	{
		if (!pChild->IsKindOf(RUNTIME_CLASS(CLocalizablePropertyPage)))
			pDesc->AddChildWindow(pChild);
		pChild = pChild->GetWindow(GW_HWNDNEXT);
	}
	return (LRESULT)pDesc;
}

//-----------------------------------------------------------------------------
BOOL CLocalizablePropertySheet::ForceFocusToParent()
{
	return m_pParentWnd && AfxGetLoginContext()->GetDocked();
}

//-----------------------------------------------------------------------------
INT_PTR CLocalizablePropertySheet::DoModal()
{
	m_bIsModal = TRUE;
	CTBWinThread::PumpThreadMessages(); //used to process all pending messages in order to fix active window

	CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);

	m_pParentWnd = GetSafeParent(m_pParentWnd);

	//imposto la dll di default in cui cercare le risorse uguale a quella che contiene la definizione della tabdialog
	CSetResourceHandle h(GetResourceModule());
	INT_PTR res = __super::DoModal();

	//attivo la parent (in tabbed windows quando parte una modale perde il fuoco)
	if (ForceFocusToParent())
		m_pParentWnd->SetFocus();

	if (res == -1)
		throw (new CThreadAbortedException());
	return res;
}

//-----------------------------------------------------------------------------
void CLocalizablePropertySheet::BuildPropPageArray()
{
	// delete existing prop page array
	free((void*)m_psh.ppsp);
	m_psh.ppsp = NULL;

	// determine size of PROPSHEETPAGE array
	int i;
	int nBytes = 0;
	for (i = 0; i < m_pages.GetSize(); i++)
	{
		CPropertyPage* pPage = GetPage(i);
		nBytes += pPage->m_psp.dwSize;
	}

	// build new PROPSHEETPAGE array
	PROPSHEETPAGE* ppsp = (PROPSHEETPAGE*)malloc(nBytes);
	BYTE* ppspOrigByte = reinterpret_cast<BYTE*>(ppsp);
	if (ppsp == NULL)
		AfxThrowMemoryException();
	BYTE* pPropSheetPagesArrEnd = ppspOrigByte + nBytes;
	ENSURE(pPropSheetPagesArrEnd >= ppspOrigByte);
	m_psh.ppsp = ppsp;
	BOOL bWizard = (m_psh.dwFlags & (PSH_WIZARD | PSH_WIZARD97));
	for (i = 0; i < m_pages.GetSize(); i++)
	{
		CPropertyPage* pPage = GetPage(i);
		BYTE* ppspByte = reinterpret_cast<BYTE*>(ppsp);
		ENSURE_THROW(ppspByte >= ppspOrigByte && ppspByte <= pPropSheetPagesArrEnd, AfxThrowMemoryException());
		Checked::memcpy_s(ppsp, pPropSheetPagesArrEnd - reinterpret_cast<BYTE*>(ppsp), &pPage->m_psp, pPage->m_psp.dwSize);

		
		((CLocalizablePropertyPage*) pPage)->PreProcessPageTemplate(*ppsp, bWizard);
		(BYTE*&)ppsp += ppsp->dwSize;
	}
	m_psh.nPages = (int)m_pages.GetSize();
}

//-----------------------------------------------------------------------------
BOOL CLocalizablePropertySheet::OnInitDialog()
{
	BOOL bResult = CPropertySheet::OnInitDialog();


	TCITEM tci;
	tci.mask = TCIF_TEXT;
	CTabCtrl* pCtrl = GetTabControl();

	ASSERT_TRACE(pCtrl, "Tab control not found");

	// salvo l'indice della tab corrente
	int nActivePage = GetActiveIndex();

	// attivo in sequenza tutte le tab 
	// e le localizzo (devo farlo su tutte perché è visibile
	// il titolo dalla tab)
	for (int i = GetPageCount() - 1; i >= 0; i--)
	{
		CLocalizablePropertyPage *pPage = (CLocalizablePropertyPage*)GetPage(i);
		ASSERT_KINDOF(CLocalizablePropertyPage, pPage);
		SetActivePage(pPage);
		pPage->Localize();

		// rinfresco la caption a seguito della traduzione
		tci.pszText = (LPTSTR)(LPCTSTR)pPage->GetCaption();;
		pCtrl->SetItem(i, &tci);
	}
	

	// ripristino la tab corrente
	SetActivePage(nActivePage);


	// We need to notify the current thread about
	// a new window only when window has been fully 
	// created.
	if (m_bIsModal)
		AfxSetThreadInModalState(TRUE, m_hWnd);
	else
		AfxGetThreadContext()->AddWindowRef(m_hWnd, FALSE);

	return bResult;
}

//-----------------------------------------------------------------------------
BOOL CLocalizablePropertySheet::DestroyWindow()
{
	if (m_bIsModal)
		AfxSetThreadInModalState(FALSE, m_hWnd);
	else
		AfxGetThreadContext()->RemoveWindowRef(m_hWnd, FALSE);

	return CPropertySheet::DestroyWindow();
}
//--------------------------------------------------------------------------
HINSTANCE CLocalizablePropertySheet::GetResourceModule()
{
	if (m_hResourceModule == NULL)
	{
		m_hResourceModule = GetDllInstance(GetRuntimeClass());
	}
	return m_hResourceModule;
}

//-----------------------------------------------------------------------------
LRESULT CLocalizablePropertySheet::OnGetComponent(WPARAM wParam, LPARAM lParam)
{
	//TODO 
	return 0L;
}


//*****************************************************************************
// CLocalizablePropertyPage dialog
//*****************************************************************************

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CLocalizablePropertyPage, CPropertyPage)


//-----------------------------------------------------------------------------
CLocalizablePropertyPage::CLocalizablePropertyPage()
{
}

//-----------------------------------------------------------------------------
CLocalizablePropertyPage::CLocalizablePropertyPage(UINT nIDTemplate, UINT nIDCaption, DWORD dwSize)
	: CPropertyPage(nIDTemplate, nIDCaption, dwSize)
{
}

//-----------------------------------------------------------------------------
CLocalizablePropertyPage::CLocalizablePropertyPage(LPCTSTR lpszTemplateName, UINT nIDCaption, DWORD dwSize)
	: CPropertyPage(lpszTemplateName, nIDCaption, dwSize)
{
}

//-----------------------------------------------------------------------------
CLocalizablePropertyPage::CLocalizablePropertyPage(UINT nIDTemplate, UINT nIDCaption, UINT nIDHeaderTitle, UINT nIDHeaderSubTitle, DWORD dwSize)
	: CPropertyPage(nIDTemplate, nIDCaption, nIDHeaderTitle, nIDHeaderSubTitle, dwSize)
{
}

//-----------------------------------------------------------------------------
CLocalizablePropertyPage::CLocalizablePropertyPage(LPCTSTR lpszTemplateName, UINT nIDCaption, UINT nIDHeaderTitle, UINT nIDHeaderSubTitle, DWORD dwSize)
	: CPropertyPage(lpszTemplateName, nIDCaption, nIDHeaderTitle, nIDHeaderSubTitle, dwSize)
{
}

//-----------------------------------------------------------------------------
CLocalizablePropertyPage::~CLocalizablePropertyPage()
{

#pragma warning( push )
#pragma warning( disable: 4996 )
	delete m_pJsonContext;
#pragma warning( pop )
	delete m_pJsonTemplate;
}
//--------------------------------------------------------------------------
void CLocalizablePropertyPage::PreProcessPageTemplate(PROPSHEETPAGE& psp, BOOL bWizard)
{
	CJsonContextObj* pContext = GetJsonContext();
	//è stata mappata come risorsa dinamica: si tratta di risorsa in file tbjson
	if (pContext)
	{
		m_pJsonTemplate = (DLGTEMPLATE*)pContext->CreateTemplate(false);
		psp.pResource = m_pJsonTemplate;
		psp.dwFlags |= (PSP_DLGINDIRECT);
		this->m_strCaption = AfxLoadJsonString(pContext->m_pDescription->m_strText, pContext->m_pDescription);
	}
	__super::PreProcessPageTemplate(psp, bWizard);
}
//--------------------------------------------------------------------------
HINSTANCE CLocalizablePropertyPage::GetResourceModule()
{
	/*CRuntimeClass* pClass = GetRuntimeClass();
	while (pClass && pClass != RUNTIME_CLASS(CLocalizablePropertyPage))
	{
	HINSTANCE hInst = GetDllInstance(pClass);
	if (::FindResource(hInst, m_lpszTemplateName, RT_DIALOG))
	return hInst;
	pClass = pClass->m_pfnGetBaseClass();
	}*/
	if (m_hResourceModule == NULL)
	{
		ASSERT(GetRuntimeClass() != RUNTIME_CLASS(CLocalizablePropertyPage));

		m_hResourceModule = GetDllInstance(GetRuntimeClass());
	}
	return m_hResourceModule;
}

// modeless dialog
//------------------------------------------------------------------------------
BOOL CLocalizablePropertyPage::Create(UINT nIDTemplate, CWnd* pParentWnd)
{
	//uso m_nIDHelp come variabile di appoggio per tenermi da parte l'id dinamico originale
	m_nIDHelp = nIDTemplate;

	CJsonContextObj* pContext = GetJsonContext();
	//è stata mappata come risorsa dinamica: si tratta di risorsa in file tbjson
	if (pContext)
	{
		AutoDeletePtr<DLGTEMPLATE> pTemplate = (DLGTEMPLATE*)pContext->CreateTemplate(false);
		return pTemplate && CreateIndirect(pTemplate, pParentWnd);
	}
#ifdef DEBUG
	CJsonResource sJsonId = AfxGetTBResourcesMap()->DecodeID(TbResources, m_nIDHelp);
	if (!sJsonId.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}
#endif
	//imposto la dll di default in cui cercare le risorse uguale a quella che contiene la definizione della tabdialog
	CSetResourceHandle h(GetResourceModule());
	return __super::Create(nIDTemplate, pParentWnd);
}
//-----------------------------------------------------------------------------
CTabDescription* CLocalizablePropertyPage::GetControlStructure(CWndObjDescriptionContainer* pContainer, bool bActive, HWND parentHandle)
{	
	CString strId = cwsprintf(_T("%d_%d"), parentHandle, m_hWnd);
	CTabDescription *pTabDesc = (CTabDescription*)(pContainer->GetWindowDescription(this, RUNTIME_CLASS(CTabDescription), strId));

	if (pTabDesc->m_strText != m_strCaption)
	{
		pTabDesc->m_strText = m_strCaption;
		pTabDesc->SetUpdated(&pTabDesc->m_strText);
	}
	if (pTabDesc->m_bActive != bActive)
	{
		pTabDesc->m_bActive = bActive;
		pTabDesc->SetUpdated(&pTabDesc->m_bActive);
	}
	//solo per la CLocalizablePropertyPage attiva chiedo alle sue finestre figlie la loro descrizione(come ottimizzazione)
	if (bActive)
	{
		CRect tabRect(0, 0, 0, 0);
		GetWindowRect(tabRect);
		if (pTabDesc->GetRect() != tabRect)
		{
			pTabDesc->SetRect(tabRect, TRUE);
		}
		pTabDesc->AddChildWindows(this);
	}
	return pTabDesc;
}

//-----------------------------------------------------------------------------
CJsonContextObj* CLocalizablePropertyPage::GetJsonContext()
{
#pragma warning( push )
#pragma warning( disable: 4996)
	if (!m_pJsonContext)
	{
		CJsonResource sJsonId = AfxGetTBResourcesMap()->DecodeID(TbResources, m_nIDHelp);
		if (!sJsonId.IsEmpty())
		{
			m_pJsonContext = CJsonFormEngineObj::GetInstance()->CreateContext(sJsonId);
		}
	}
	return m_pJsonContext;
#pragma warning( pop )
}

//-----------------------------------------------------------------------------
BOOL CLocalizablePropertyPage::OnInitDialog()
{
	//è stata mappata come risorsa dinamica: si tratta di risorsa in file tbjson
	//in questo caso creo i controlli figli PRIMA della OnInitDialog del padre
	//in modo da chiamarla con i controlli già creati
	//non posso crearli nella Create perché è troppo presto, mi chiama la BuildDataControlLinks prima di aver creato i controlli
	//nella OnCreate non funziona ancora la MapDialogRect
	CJsonContextObj* pContext = GetJsonContext();
	if (pContext)
	{
		SetDlgCtrlID(m_nIDHelp);
		m_lpszTemplateName = MAKEINTRESOURCE(m_nIDHelp);
		CJsonFormEngineObj::GetInstance()->CreateChilds(pContext, this);
	}
	CPropertyPage::OnInitDialog();

	ASSERT_KINDOF(CLocalizablePropertySheet, GetParent());
	
	Localize();

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}

//-----------------------------------------------------------------------------
BOOL CLocalizablePropertyPage::OnSetActive()
{
	Localize();

	return CPropertyPage::OnSetActive();
}

//-----------------------------------------------------------------------------
void CLocalizablePropertyPage::Localize()
{
	if (m_bLocalized)
		return;
	CJsonContextObj* pContext = GetJsonContext();
	if (!pContext)//se non sono json, devo tradurre, altrimenti le ha già tradotte l'engine json
	{
		//imposto la dll di default in cui cercare le risorse uguale a quella che contiene la definizione della tabdialog
		CSetResourceHandle h(GetResourceModule());
		// localizza le stringhe della finestra
		AfxLoadWindowStrings(this, m_lpszTemplateName);
		GetWindowText(m_strCaption);
	}
	m_bLocalized = TRUE;
}


//*****************************************************************************
// CLocalizableMenu
//*****************************************************************************
IMPLEMENT_DYNCREATE(CLocalizableMenu, CMenu)

//-----------------------------------------------------------------------------
BOOL CLocalizableMenu::LoadMenu(LPCTSTR lpszResourceName)
{
	if (CMenu::LoadMenu(lpszResourceName))
	{
		AfxLoadMenuStrings(this, REVERSEMAKEINTRESOURCE(lpszResourceName));
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CLocalizableMenu::LoadMenu(UINT nIDResource)
{
	if (CMenu::LoadMenu(nIDResource))
	{
		AfxLoadMenuStrings(this, nIDResource);
		return TRUE;
	}
	return FALSE;
}


//*****************************************************************************
// CLocalizableFrame
//*****************************************************************************

IMPLEMENT_DYNCREATE(CLocalizableFrame, CDockableFrame)

//-----------------------------------------------------------------------------
CLocalizableFrame::CLocalizableFrame()
{
	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CLocalizableFrame::~CLocalizableFrame()
{
}



//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CLocalizableFrame, CDockableFrame)
	ON_WM_CREATE()

	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTW, 0, 0xFFFF, OnToolTipText)
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTA, 0, 0xFFFF, OnToolTipText)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
void CLocalizableFrame::GetMessageString(UINT nID, CString& rMessage) const
{
	CLocalizableFrame* pFrame = (CLocalizableFrame*)this;
	void* pObject = pFrame->GetActiveDocument() ? pFrame->GetActiveDocument()->GetRuntimeClass() : GetRuntimeClass();
	rMessage = CCommonFunctions::GetMessageString(nID, GetDllInstance(pObject));
}

// identica a quella di classe madre + classe nonna, ma utilizza
// AfxLoadTBString per localizzare la stringa del tooltip
//-----------------------------------------------------------------------------
BOOL CLocalizableFrame::OnToolTipText(UINT nID, NMHDR* pNMHDR, LRESULT* pResult)
{
	if (pNMHDR->code == TTN_NEEDTEXTA || pNMHDR->code == TTN_NEEDTEXTW, "Wrong pNMHDR->code = %d", pNMHDR->code)
	{
		// check to see if the message is going directly to this window or not
		const MSG* pMsg = GetCurrentMessage();
		if (pMsg->hwnd != m_hWnd)
		{
			// let top level frame handle this for us
			return FALSE;
		}
	}
	void* pObject = GetActiveDocument() ? GetActiveDocument()->GetRuntimeClass() : GetRuntimeClass();
	return CCommonFunctions::OnToolTipText(nID, pNMHDR, pResult, GetDllInstance(pObject));    // message was handled
}

//-----------------------------------------------------------------------------
void CLocalizableFrame::ExtractToolTipText(const CString &strFullText, NMHDR* lpnmhdr, LRESULT* pResult)
{
	CCommonFunctions::ExtractToolTipText(strFullText, lpnmhdr, pResult);
}

// localizza il titolo della finestra
//-----------------------------------------------------------------------------
BOOL CLocalizableFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	void* pObject = GetActiveDocument() ? GetActiveDocument()->GetRuntimeClass() : GetRuntimeClass();
	CCommonFunctions::LocalizeTitle(m_nIDHelp, m_strTitle, cs, GetDllInstance(pObject));

	return __super::PreCreateWindow(cs);
}

//-----------------------------------------------------------------------------
HRESULT CLocalizableFrame::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace = GetRanorexNamespace();
	*pszName = ::SysAllocString(sNamespace);
	return S_OK;
}

//-----------------------------------------------------------------------------
CString CLocalizableFrame::GetRanorexNamespace()
{
	return _T("CLocalizableFrame");
}



