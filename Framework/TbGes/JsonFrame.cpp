#include "stdafx.h"

#include <TbGenlib\TBSplitterWnd.h>

#include <TbGenLibManaged\Main.h>
#include "JsonFrame.h"
#include "JsonFormEngineEx.h"
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include "ModuleObjects\XEngineParameters\JsonForms\IDR_EXTDOC.hjson"


//-----------------------------------------------------------------------------
template <class T> CJsonFrameT<T>::CJsonFrameT() {
	m_bHasToolbar = FALSE;

}
//-----------------------------------------------------------------------------
template <class T> CJsonFrameT<T>::~CJsonFrameT() {
	delete m_pJsonContext;
}
//-----------------------------------------------------------------------------
template <class T> CWndFrameDescription* CJsonFrameT<T>::GetFrameDescription()
{
	return (CWndFrameDescription*)m_pJsonContext->m_pDescription;
}

//-----------------------------------------------------------------------------
template <class T> LRESULT CJsonFrameT<T>::OnGetComponent(WPARAM wParam, LPARAM lParam)
{
	if (!m_pJsonContext)
		return 0L;
	CJsonSerializer* pResp = (CJsonSerializer*)wParam;
	pResp->OpenObject(_T("component"));
	TCHAR buff[32];
	_itot_s((int)m_hWnd, buff, 10);
	pResp->WriteString(_T("id"), buff);

	if (GetDockableParent())
	{
		_itot_s((int)GetDockableParent()->m_hWnd, buff, 10);
		pResp->WriteString(_T("parentId"), buff);
	}

	pResp->WriteString(_T("name"), GetName(m_pJsonContext->m_JsonResource.GetFile()));
	pResp->WriteString(_T("app"), m_pJsonContext->m_JsonResource.GetOwnerNamespace().GetApplicationName());
	pResp->WriteString(_T("mod"), m_pJsonContext->m_JsonResource.GetOwnerNamespace().GetModuleName());

	pResp->CloseObject();
	return 1L;
}

//-----------------------------------------------------------------------------
template <class T> LRESULT CJsonFrameT<T>::OnGetComponentStrings(WPARAM wParam, LPARAM lParam)
{
	if (!m_pJsonContext)
		return 0L;
	CThreadContext* pThreadContext = AfxGetThreadContext();
	CString sOldCulture, sCulture = (LPCTSTR)lParam;
	if (!sCulture.IsEmpty())
		sOldCulture = pThreadContext->SetUICulture(sCulture);

	CJsonSerializer* pResp = (CJsonSerializer*)wParam;
	TCHAR buff[32];
	_itot_s((int)m_hWnd, buff, 10);
	pResp->WriteString(_T("id"), buff);
	pResp->OpenArray(_T("strings"));
	
	if (!pThreadContext->GetUICulture().IsEmpty())
		m_pJsonContext->m_pDescription->WriteJsonStrings(pResp);
	
	if (!sCulture.IsEmpty())
		 pThreadContext->SetUICulture(sOldCulture);
	pResp->CloseArray();
	return 1L;
}
//-----------------------------------------------------------------------------
template <class T> LRESULT CJsonFrameT<T>::OnGetActivationData(WPARAM wParam, LPARAM lParam)
{
	if (!m_pJsonContext)
		return 0L;
	CJsonSerializer* pResp = (CJsonSerializer*)wParam;
	int* pIndex = (int*)lParam;
	pResp->OpenObject(*pIndex);
	//i dati di attivazione vengono salvati nel client sul servizio associato alla frame principale
	HWND hwnd = ((CBaseDocument*) GetDocument())->GetFrameHandle();
	if (!hwnd)
		hwnd = m_hWnd;
	TCHAR buff[32];
	_itot_s((int)hwnd, buff, 10);
	pResp->WriteString(_T("id"), buff);
	pResp->OpenObject(_T("activation"));
	CStringArray arIds;
	CArray<bool> arActivated;
	((CJsonContext*)m_pJsonContext)->GetActivationExpressions(arIds, arActivated);

	ASSERT(arIds.GetSize() == arActivated.GetSize());

	for (int i = 0; i < arIds.GetSize(); i++)
	{
		CString sId = arIds[i];
		MengleActivationString(sId);
		pResp->WriteBool(sId, arActivated[i]);
	}
	pResp->CloseObject();
	pResp->CloseObject();
	return 1L;
}
//-----------------------------------------------------------------------------
template <class T> BOOL CJsonFrameT<T>::CreateAccelerator()
{
	return TRUE;
}
//-----------------------------------------------------------------------------
template <class T> void CJsonFrameT<T>::OnCreateStepper()
{
	if (!GetFrameDescription()->m_bStepper)
		return;

	m_pStepper = new CFrameStepper();
	m_pStepper->CreateStepper(this, RUNTIME_CLASS(CStepperBreadCrumb));

	m_pStepper->GetStepper()->SetBreadCrumbFont(AfxGetThemeManager()->GetWizardStepperFont());
	m_pStepper->SetHeight(AfxGetThemeManager()->GetWizardStepperHeight());
}
//-----------------------------------------------------------------------------
template <class T> void CJsonFrameT<T>::OnAdjustFrameSize(CSize& size)
{
	CRect r = m_pJsonContext->m_pDescription->GetRect();

	if (!r.IsRectEmpty())
	{
		CRect rr(CPoint(0, 0), CSize(r.Width(), r.Height()));
		::SafeMapDialog(m_hWnd, rr);
		size.cx = rr.Width();
		size.cy = rr.Height();
	}
	else
	{
		__super::OnAdjustFrameSize(size);
	}

}


//-----------------------------------------------------------------------------
template <class T> BOOL CJsonFrameT<T>::OnPopulatedDropDown(UINT nIdCommand)
{
	CString sId = AfxGetTBResourcesMap()->DecodeID(TbResourceType::TbCommands, nIdCommand).m_strName;
	CToolbarBtnDescription* pToolBarBtnDesc = (CToolbarBtnDescription*)m_pToolbarContext->m_pDescription->Find(sId);
	if (!pToolBarBtnDesc || !pToolBarBtnDesc->m_bIsDropdown || pToolBarBtnDesc->m_Children.GetCount() == 0)
		return __super::OnPopulatedDropDown(nIdCommand);
	pToolBarBtnDesc->EvaluateExpressions(m_pToolbarContext);
	CTBToolBarMenu menu;
	menu.CreateMenu();

	for (int k = 0; k < pToolBarBtnDesc->m_Children.GetCount(); k++)
	{
		CWndObjDescription* pDescMenuItem = pToolBarBtnDesc->m_Children.GetAt(k);
		if (!pDescMenuItem->IsKindOf(RUNTIME_CLASS(CMenuItemDescription)))
		{
			ASSERT(FALSE);
			continue;
		}
		if (((CMenuItemDescription*)pDescMenuItem)->m_bIsSeparator)
		{
			menu.AppendMenu(MF_SEPARATOR);
			continue;
		}
		int nId = CJsonFormEngineObj::GetID(pDescMenuItem);
		if (!((CJsonContext*)m_pToolbarContext)->CanCreateControl(pDescMenuItem, nId))
		{
			continue;
		}
		CString sText = AfxLoadJsonString(pDescMenuItem->m_strText, pDescMenuItem);

		menu.AppendMenu(MF_STRING, nId, (LPTSTR)(LPCTSTR)sText);
	}

	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		((CAbstractFormDoc*)pDoc)->DispatchToolbarDropDown(nIdCommand, menu);

	m_pTabbedToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	return TRUE;
}

//-----------------------------------------------------------------------------
template <class T> void CJsonFrameT<T>::CreateHotLinks(CWndObjDescription* pWndDesc, CAbstractFormDoc* pDoc)
{
	//registro tutti gli hotlink da usare nel binding,
	if (pWndDesc->m_pBindings)
	{
		HotLinkInfo* pInfo = pWndDesc->m_pBindings->m_pHotLink;
		CString sHotLink;
		if (pInfo)
			sHotLink = pInfo->m_strName;
		if (!sHotLink.IsEmpty() && pDoc)
		{
			pDoc->RegisterForwardHotLink(sHotLink, pInfo->m_strNamespace);
		}
	}
	//ricorsione sui figli
	for (int i = 0; i < pWndDesc->m_Children.GetCount(); i++)
	{
		CWndObjDescription* pChild = pWndDesc->m_Children[i];
		CreateHotLinks(pChild, pDoc);
	}
}

//-----------------------------------------------------------------------------
template <class T> void CJsonFrameT<T>::OnFrameCreated()
{
	// EasyStudio evita di creare tutto, toolbar e pannelli laterali
	if (GetDocument() && ((CBaseDocument*)GetDocument())->GetDesignMode() == CBaseDocument::DM_RUNTIME)
	{
		__super::OnFrameCreated();
		return;
	}

	ASSERT(m_pTabbedToolBar == NULL);
	m_pTabbedToolBar = new CTBTabbedToolbar();
	m_bDelayedLayoutSuspended = TRUE;
	m_pTabbedToolBar->SuspendLayout();
	m_pTabbedToolBar->Create(this);
	//m_pTabbedToolBar->SetWindowText(_T("TabbedToolBar"));
	m_pTabbedToolBar->AttachOSLInfo(((CBaseDocument*)GetDocument())->GetInfoOSL());
	SetMenu(NULL);

	//valuto le espressioni dinamiche
	m_pJsonContext->m_pDescription->EvaluateExpressions(m_pJsonContext);
	//solo qui posso creare i docking pane, perché devo aspettare che sia stata chiamata la OnAttachData del documento per 
	//avere le variabili bindate

	Array templates;
	templates.SetOwns(TRUE);
	for (int i = 0; i < m_pJsonContext->m_pDescription->m_Children.GetCount(); i++)
	{
		CWndObjDescription* pChild = m_pJsonContext->m_pDescription->m_Children[i];
		switch (pChild->m_Type)
		{
		case CWndObjDescription::TabbedToolbar:
		case CWndObjDescription::Toolbar:
		{
			if (!m_pToolbarContext)
			{
				m_pToolbarContext = (CJsonContext*)CJsonContext::Create();
				m_pToolbarContext->Assign(m_pJsonContext);
				m_pToolbarContext->m_pDescription = pChild;
				m_pToolbarContext->m_bOwnDescription = false;
			}

			__super::CreateJsonToolbar(pChild);

			break;
		}
		case CWndObjDescription::DockingPane:
		{
			UINT nPaneId = CJsonFormEngineObj::GetID(pChild);
			if (!m_pJsonContext->CanCreateControl(pChild, nPaneId))
				continue;

			CSize size(300, 700);
			if (pChild->m_Width > 0)
				size.cx = pChild->m_Width;
			if (pChild->m_Height > 0)
				size.cy = pChild->m_Height;
			::SafeMapDialog(m_hWnd, size);

			CRuntimeClass* pDockPaneClass = ((CBaseDocument*)GetDocument())->GetControlClass(nPaneId);
			if (!pDockPaneClass)
				pDockPaneClass = RUNTIME_CLASS(CTaskBuilderDockPane);

			CTaskBuilderDockPane* pPane = (CTaskBuilderDockPane*)pDockPaneClass->CreateObject();
			pPane->m_nID = nPaneId;
			CTaskBuilderDockPaneForm* pFirstForm = NULL;
			CToolbarDescription* pToolbarDesc = NULL;
			for (int j = 0; j < pChild->m_Children.GetCount(); j++)
			{
				CWndObjDescription* pPaneItem = pChild->m_Children[j];
				if (pPaneItem->m_Type == CWndObjDescription::View)
				{
					UINT nId = CJsonFormEngineObj::GetID(pPaneItem);
					if (!m_pJsonContext->CanCreateControl(pPaneItem, nId))
						continue;
					CRuntimeClass* pClass = ((CBaseDocument*)GetDocument())->GetControlClass(nId);
					if (!pClass)
						pClass = RUNTIME_CLASS(CJsonFormView);

					CJsonContext* pViewContext = (CJsonContext*)CJsonContext::Create();
					pViewContext->Assign(m_pJsonContext);
					pViewContext->m_pDescription = pPaneItem;
					pViewContext->m_bOwnDescription = false;

					CCreateContext *pCreateContext = new CCreateContext;
					//mi serve solo per passare l'id della view ed il json context
					pCreateContext->m_pCurrentDoc = GetDocument();
					pCreateContext->m_pCurrentFrame = this;
					pCreateContext->m_pLastView = NULL;

					pCreateContext->m_pNewDocTemplate = new CSingleExtDocTemplate(0, NULL, NULL, NULL);
					templates.Add(pCreateContext->m_pNewDocTemplate);
					pCreateContext->m_pNewViewClass = pClass;
					pCreateContext->m_pCurrentDoc = GetDocument();
					((CSingleExtDocTemplate*)pCreateContext->m_pNewDocTemplate)->m_pJsonContext = pViewContext;
					((CSingleExtDocTemplate*)pCreateContext->m_pNewDocTemplate)->m_nViewID = nId;


					CJsonFormView* pView = pClass == RUNTIME_CLASS(CJsonFormView)
						? new CJsonFormView(nId)
						: (CJsonFormView*)pClass->CreateObject();

					CTaskBuilderDockPaneForm* pForm = new CTaskBuilderDockPaneForm(pView, AfxLoadJsonString(pPaneItem->m_strText, pPaneItem), NULL, pCreateContext);
					pPane->AddForm(pForm);
					if (!pFirstForm)
						pFirstForm = pForm;
				}
				else if (pPaneItem->m_Type == CWndObjDescription::Toolbar)
				{
					ASSERT(!pToolbarDesc);
					ASSERT_KINDOF(CToolbarDescription, pPaneItem);
					pToolbarDesc = (CToolbarDescription*)pPaneItem;

				}
				else
				{
					TRACE("Docking panes can contain only view or toolbar types");
					ASSERT(FALSE);
				}
			}
			if (!m_DockingPanes.CreatePane(this,
				pPane,
				((CSingleExtDocTemplate*)pFirstForm->GetCreateContext()->m_pNewDocTemplate)->m_nViewID,
				pChild->m_strName,
				AfxLoadJsonString(pChild->m_strText, pChild),
				CBRS_ALIGN_RIGHT,
				size,
				pFirstForm->GetCreateContext(),
				dwDefaultBCGDockingBarStyle,
				TRUE))
				continue;

			pPane->SetAutoHideMode(TRUE, CBRS_ALIGN_RIGHT | CBRS_HIDE_INPLACE);

			if (pToolbarDesc)
			{
				pPane->EnableToolbar();
				CreateToolbarFromDesc(pPane->GetToolBar(), pToolbarDesc);
				if (pToolbarDesc->m_bBottom)
					pPane->GetToolBar()->ShowInDialog(pPane, CBRS_ALIGN_BOTTOM);
				pPane->AdjustToolbarHeight();
			}

			pPane->InitialUpdate();
			break;
		}
		}
	}
	BOOL b = OnCustomizeTabbedToolBar(m_pTabbedToolBar) &&
		OnAddClientDocToolbarButtons();
	__super::OnFrameCreated();
}

//-----------------------------------------------------------------------------
template <class T> BOOL CJsonFrameT<T>::PreCreateWindow(CREATESTRUCT& cs)
{
	BOOL b = __super::PreCreateWindow(cs);
	if (!m_pJsonContext->m_pDescription->m_bChild)
		cs.hwndParent = NULL;//il finder non ha parent, a meno che sia pinnato
	if (GetFrameDescription()->m_bSystemMenu)
		cs.style |= WS_SYSMENU;
	else
		cs.style &= ~WS_SYSMENU;
	return b;
}

//-----------------------------------------------------------------------------
template <class T> BOOL CJsonFrameT<T>::OnCreateClient(LPCREATESTRUCT pStruct, CCreateContext* pContext)
{
	m_bHasStatusBar = GetFrameDescription()->m_bStatusBar;
	{
		CreateHotLinks(m_pJsonContext->m_pDescription, (CAbstractFormDoc*)pContext->m_pCurrentDoc);
	}

	for (int i = 0; i < m_pJsonContext->m_pDescription->m_Children.GetCount(); i++)
	{
		CWndObjDescription* pChild = m_pJsonContext->m_pDescription->m_Children[i];
		switch (pChild->m_Type)
		{

		case CWndObjDescription::Splitter:
		{
			CSplitterDescription* pSplitterDesc = (CSplitterDescription*)(pChild);
			((CJsonContext*)m_pJsonContext)->CreateSplitter(pSplitterDesc, this, (CBaseDocument*)pContext->m_pCurrentDoc);
			break;
		}

		case CWndObjDescription::View:
		{
			ASSERT(m_hWnd != NULL);
			ASSERT(::IsWindow(m_hWnd));
			ASSERT_KINDOF(CWndPanelDescription, pChild);
			bool bWizard = GetFrameDescription()->m_bWizard;
			UINT nID = CJsonFormEngineObj::GetID(pChild);
			CRuntimeClass* pClass = ((CBaseDocument*)pContext->m_pCurrentDoc)->GetControlClass(nID);
			if (!pClass)
				pClass = bWizard ? RUNTIME_CLASS(CWizardFormView) : RUNTIME_CLASS(CJsonFormView);

			// Note: can be a CWnd with PostNcDestroy self cleanup
			CAbstractFormView* pView = (CAbstractFormView*)pClass->CreateObject();
			if (pView == NULL)
			{
				TRACE(traceAppMsg, 0, "Warning: Dynamic create of view type %hs failed.\n",
					pClass->m_lpszClassName);
				return NULL;
			}
			ASSERT_KINDOF(CAbstractFormView, pView);
			pView->SetDialogID(nID);
			if (!pChild->m_strName.IsEmpty())
				pView->SetFormName(pChild->m_strName);
			CJsonContext* pJsonContext = (CJsonContext*)CJsonContext::Create();
			pJsonContext->Assign(m_pJsonContext);
			pJsonContext->m_pDescription = pChild;
			pJsonContext->m_bOwnDescription = false;
			pView->SetJsonContext(pJsonContext);
			// views are always created with a border!
			if (!pView->Create(NULL, NULL, AFX_WS_DEFAULT_VIEW,
				CRect(0, 0, 0, 0), this, AFX_IDW_PANE_FIRST, pContext))
			{
				TRACE(traceAppMsg, 0, "Warning: could not create view for frame.\n");
				return NULL;        // can't continue without a view
			}

			if (pView->GetExStyle() & WS_EX_CLIENTEDGE)
			{
				// remove the 3d style from the frame, since the view is
				//  providing it.
				// make sure to recalc the non-client area
				ModifyStyleEx(WS_EX_CLIENTEDGE, 0, SWP_FRAMECHANGED);
			}

		}

		}
	}
	return __super::OnCreateClient(pStruct, pContext);
}

//-----------------------------------------------------------------------------
template <class T> void CJsonFrameT<T>::OnUpdateFrameTitle(BOOL bAddToTitle)
{
	//valuto le espressioni dinamiche
	if (m_pJsonContext->m_pDoc)
		m_pJsonContext->m_pDescription->EvaluateExpressions(m_pJsonContext, false);

	if (m_pJsonContext->m_pDescription->m_strText.IsEmpty())
	{
		__super::OnUpdateFrameTitle(bAddToTitle);
	}
	else
	{
		CString strTitle = AfxLoadJsonString(m_pJsonContext->m_pDescription->m_strText, m_pJsonContext->m_pDescription);
		SetWindowText(strTitle);
	}
}

//-----------------------------------------------------------------------------
template <class T> BOOL CJsonFrameT<T>::LoadFrame(UINT nIDResource,
	DWORD dwDefaultStyle /*= WS_OVERLAPPEDWINDOW | FWS_ADDTOTITLE*/,
	CWnd* pParentWnd /*= NULL*/,
	CCreateContext* pContext /*= NULL*/)
{
	CJsonResource res = AfxGetTBResourcesMap()->DecodeID(TbResources, nIDResource);
	//è stata mappata come risorsa dinamica: si tratta di risorsa in file tbjson
	if (res.IsEmpty())
		return FALSE;

	m_pJsonContext = (CJsonContext*)CJsonFormEngineObj::GetInstance()->CreateContext(res);
	if (!m_pJsonContext
		|| !m_pJsonContext->m_pDescription
		|| !m_pJsonContext->m_pDescription->IsKindOf(RUNTIME_CLASS(CWndFrameDescription)))
	{
		ASSERT(FALSE);
		return FALSE;
	}


	if (!((CWndFrameDescription*)m_pJsonContext->m_pDescription)->m_bDockable)
		SetDockable(FALSE);

	SetHasToolbar(HasToolbar());

	if (!__super::LoadFrame(IDR_EXTDOC, dwDefaultStyle, pParentWnd, pContext))
		return FALSE;

	m_pJsonContext->Associate(this);

	m_nIDHelp = nIDResource;

	//devo farlo dopo la LoadFrame, perché altrimenti ho un ASSERT in MFC perché trova già gli acceleratori
	if (m_pJsonContext->m_pDescription->m_pAccelerator)
	{
		int nSize = 0;
		AutoDeletePtr<ACCEL> pAccel = m_pJsonContext->m_pDescription->m_pAccelerator->ToACCEL(m_pJsonContext, nSize);

		if (m_hAccelTable)
			DestroyAcceleratorTable(m_hAccelTable);
		m_hAccelTable = CreateAcceleratorTable(pAccel, nSize);
	}
	return TRUE;
}

//----------------------------------------------------------------------------
template <class T> BOOL CJsonFrameT<T>::HasToolbar()
{
	CWndObjDescriptionContainer& container = ((CWndFrameDescription*)m_pJsonContext->m_pDescription)->m_Children;

	BOOL bFound = FALSE;
	for (int i = 0; i < container.GetCount() && !bFound; i++)
	{
		CWndObjDescription* pChild = container.GetAt(i);
		if (pChild && (pChild->m_Type == CWndObjDescription::TabbedToolbar || pChild->m_Type == CWndObjDescription::Toolbar))
			bFound = TRUE;
	}

	return bFound;
}

IMPLEMENT_DYNCREATE(CJsonFrame, CMasterFrame)
BEGIN_MESSAGE_MAP(CJsonFrame, CMasterFrame)
	ON_MESSAGE(UM_GET_COMPONENT, OnGetComponent)
	ON_MESSAGE(UM_GET_COMPONENT_STRINGS, OnGetComponentStrings)
	ON_MESSAGE(UM_GET_ACTIVATION_DATA, OnGetActivationData)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
void CJsonFrame::SwitchBatchRunButtonState()
{
	CDocument* pDoc = NULL;
	if (m_pJsonContext)
	{
		pDoc = ((CJsonContext*)m_pJsonContext)->m_pDoc;
	}

	if
		(
			!pDoc ||
			!pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) ||
			((CAbstractFormDoc*)pDoc)->GetType() != VMT_BATCH
			)
		return;


	CTBTabbedToolbar* pTabbedToolbar = GetTabbedToolBar();
	if (pTabbedToolbar)
	{
		if (IsEditingParamsFromExternalController())
		{
			pTabbedToolbar->SetButtonInfo
			(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN,
				TBBS_BUTTON,
				TBIcon(szIconSave, TOOLBAR),
				_TB("Save")
			);
			pTabbedToolbar->HideButton(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN);
			return;
		}

		pTabbedToolbar->SetButtonInfo
		(
			ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN,
			TBBS_BUTTON,
			((CAbstractFormDoc*)pDoc)->m_bBatchRunning ? TBIcon(szIconStop, TOOLBAR) : TBIcon(szIconStart, TOOLBAR),
			((CAbstractFormDoc*)pDoc)->m_bBatchRunning ? _TB("Stop") : _TB("Start")
		);

		pTabbedToolbar->HideButton(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN, !((CAbstractFormDoc*)pDoc)->m_bBatchRunning);
		if (((CAbstractFormDoc*)pDoc)->m_bBatchRunning)
		{
			pTabbedToolbar->SetButtonInfo
			(
				ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN,
				TBBS_BUTTON,
				((CAbstractFormDoc*)pDoc)->GetBatchScheduler().IsPaused() ? TBIcon(szIconResume, TOOLBAR) : TBIcon(szIconPause, TOOLBAR),
				((CAbstractFormDoc*)pDoc)->GetBatchScheduler().IsPaused() ? _TB("Resume") : _TB("Pause")
			);
		}
		else
		{
			pTabbedToolbar->SetButtonInfo(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN, TBBS_BUTTON, TBIcon(szIconResume, TOOLBAR), _TB("Resume"));
		}

	}
}

IMPLEMENT_DYNCREATE(CJsonSlaveFrame, CSlaveFrame)
BEGIN_MESSAGE_MAP(CJsonSlaveFrame, CSlaveFrame)
	ON_MESSAGE(UM_GET_COMPONENT, OnGetComponent)
	ON_MESSAGE(UM_GET_ACTIVATION_DATA, OnGetActivationData)
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
CJsonDialog::CJsonDialog(CAbstractFormDoc* pDoc, UINT nIDD)
	: CParsedDialog(nIDD)
{
	ASSERT(pDoc);
	AttachDocument(pDoc);
	pDoc->m_JsonDialogs.Add(this);
}


//-----------------------------------------------------------------------------
CJsonDialog::~CJsonDialog()
{
	for (int i = 0; i < GetDocument()->m_JsonDialogs.GetCount(); i++)
	{
		if (GetDocument()->m_JsonDialogs[i] == this)
		{
			GetDocument()->m_JsonDialogs.RemoveAt(i);
			break;
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CJsonDialog::OnInitDialog()
{
	__super::OnInitDialog();

	GetDocument()->UpdateDataView();

	return TRUE;  // return TRUE unless you set the focus to a control
				  // EXCEPTION: OCX Property Pages should return FALSE
}
