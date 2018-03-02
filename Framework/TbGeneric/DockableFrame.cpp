#include "StdAfx.h"

#include <atlimage.h>

#include <TbNameSolver\ApplicationContext.h>
#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\ThreadContext.h>

#include "TBThemeManager.h"

#include ".\GeneralFunctions.h"
#include ".\WndObjDescription.h"
#include ".\LocalizableObjs.h"
#include ".\globals.h"
#include ".\DockableFrame.h"
#include "SettingsTable.h"
#include "ParametersSections.h"

//*****************************************************************************
// CDockableFrame
//*****************************************************************************
IMPLEMENT_DYNCREATE(CDockableFrame, CTBLockedFrame)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDockableFrame, CTBLockedFrame)
	ON_WM_ACTIVATE()
	ON_WM_DESTROY()
	ON_WM_CREATE()

	ON_WM_SIZE()

	ON_MESSAGE(WM_ENTERSIZEMOVE, OnEnterSizeMove)
	ON_MESSAGE(WM_EXITSIZEMOVE, OnExitSizeMove)

	ON_MESSAGE(UM_UPDATE_FRAME_STATUS, OnChangeFrameStatus)
	ON_MESSAGE(UM_UPDATE_EXTERNAL_MENU, OnUpdateExternalMenu)
	ON_MESSAGE(UM_ACTIVATE_TAB, OnActivateTab)
	ON_MESSAGE(UM_EXECUTE_FUNCTION, OnExecuteFunction)
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	ON_MESSAGE(UM_GET_COMPONENT, OnGetComponent)
	ON_MESSAGE(UM_MENU_MNG_RESIZING, OnMenuMngResizing)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDockableFrame::CDockableFrame()
	:
	m_bFromExternalResizing		(FALSE),
	m_bAfterOnFrameCreated		(FALSE),
	m_bHasToolbar				(FALSE),
	m_bDelayedLayoutSuspended	(TRUE),
	m_bDocked					(false),
	m_bDockable					(true),
	m_dwAttachedThreadId		(0),
	m_pDockableParent			(NULL),
	m_hMenu						(NULL),
	m_pDocument					(NULL),
	m_bLayoutSuspended			(false),
	m_bIsFrameVisible			(true),
	m_bFirstActivation			(true),
	m_bEditorFrameVisible		(TRUE),
	m_bChildManagement			(FALSE),
	IDisposingSourceImpl		(this)
{
	AfxGetThreadContext()->AddActiveWnd(this);
	m_bChildManagement = *((DataBool*)AfxGetSettingValue(snsTbGenlib, szFormsSection, szChildManagement, DataBool(0)));
}

//-----------------------------------------------------------------------------
CDockableFrame::~CDockableFrame()
{
	CThreadContext* pThreadContext = AfxGetThreadContext();
	if (pThreadContext->IsInErrorState())  //to prevent another exception when destroying a document after an exception
		AfxGetThreadState()->m_pRoutingFrame = NULL;

	CWinThread* pThread = AfxGetThread();
	pThreadContext->RemoveActiveWnd(this);
	if (pThread->m_pActiveWnd == this)
		pThread->m_pActiveWnd = pThreadContext->GetLatestActiveWnd(NULL);
	if (pThreadContext->GetActiveDockableWnd() == this)
		pThreadContext->SetActiveDockableWnd(NULL);
}
//-----------------------------------------------------------------------------
LRESULT CDockableFrame::OnGetComponent(WPARAM wParam, LPARAM lParam)
{
	CJsonSerializer* pResp = (CJsonSerializer*)wParam;
	pResp->OpenObject(_T("component"));
	TCHAR buff[32];
	_itot_s((int)m_hWnd, buff, 10);
	pResp->WriteString(_T("id"), buff);
	
	pResp->WriteString(_T("name"), _T("IDD_Unsupported"));
	pResp->WriteString(_T("app"), _T("Framework"));
	pResp->WriteString(_T("mod"), _T("TbGes"));
	pResp->CloseObject();
	return 1L;
}
//-----------------------------------------------------------------------------
void CDockableFrame::OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	CWinThread *pThread = AfxGetThread();

	if (nState != WA_INACTIVE)
	{
		CThreadContext* pThreadContext = AfxGetThreadContext();
		if (m_bDockable && !AfxIsInUnattendedMode())
		{
			pThreadContext->SetActiveDockableWnd(this);
			::SendMessage(AfxGetMenuWindowHandle(), UM_FRAME_ACTIVATE, (WPARAM)m_hWnd, (LPARAM)NULL);
		}

		if (pThread->m_pActiveWnd != this)
		{
			pThread->m_pActiveWnd = this;
			pThreadContext->SetLatestActiveWnd(this);
		}
	}

	__super::OnActivate(nState, pWndOther, bMinimized);
}


//-----------------------------------------------------------------------------
BOOL CDockableFrame::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	__try
	{
		return __super::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
	}
	__except (s_pfExpFilter(GetExceptionInformation(), GetExceptionCode()))
	{
		AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)		
		return TRUE;
	}
}

//-----------------------------------------------------------------------------
BOOL CDockableFrame::OnWndMsg(UINT message, WPARAM wParam, LPARAM lParam, LRESULT* pResult)
{
	__try
	{
		return __super::OnWndMsg(message, wParam, lParam, pResult);
	}
	__except (s_pfExpFilter(GetExceptionInformation(), GetExceptionCode()))
	{
		AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)		
		return TRUE;
	}
}
//-----------------------------------------------------------------------------
BOOL CDockableFrame::OnCreateClient(LPCREATESTRUCT cs, CCreateContext* pContext)
{
	m_pDocument = pContext->m_pCurrentDoc;
	return UseSplitters() ? TRUE : __super::OnCreateClient(cs, pContext);
}

//-----------------------------------------------------------------------------
BOOL CDockableFrame::GetToolbarButtonToolTipText(CBCGPToolbarButton* pButton, CString& strTTText)
{
	CString strFullText;

	TOOLTIPTEXTW TTTW;
	ZeroMemory(&TTTW, sizeof(TOOLTIPTEXTW));
	TTTW.hdr.code = TTN_NEEDTEXTW;
	TTTW.hdr.idFrom = pButton->m_nID;
	LRESULT res = 0;
	CCommonFunctions::OnToolTipText(pButton->m_nID, &TTTW.hdr, &res, GetDllInstance(GetRuntimeClass()));    // message was handled
	strTTText = TTTW.szText;

	strTTText.Remove(_T('&'));
	return TRUE;
}

//-----------------------------------------------------------------------------
void CDockableFrame::AdjustClientArea()
{
	// -------------------------------------------------------------
	// appiccica la View alla ToolBar nel casto della Tabbed tool bar

	__super::AdjustClientArea();

	CBCGPDockManager* pDocManager = GetDockManager();
	if (!pDocManager)
		return;
	CObList bars;
	pDocManager->GetControlBarList(bars, TRUE);
	int i = 0;
	for (POSITION pos = bars.GetHeadPosition(); pos != NULL; i++)
	{
		CBCGPControlBar* pBar = dynamic_cast<CBCGPControlBar*> (bars.GetNext(pos));

		if (!pBar)
			continue;
		CRect childRect;
		CRect rectBar;

		pBar->GetClientRect(rectBar);
		CWnd* pChildWnd = GetDlgItem(AFX_IDW_PANE_FIRST);

		if (!pChildWnd) return;
		pChildWnd->GetWindowRect(childRect);
		pChildWnd->ScreenToClient(childRect);
		//pChildWnd->SetWindowPos (NULL, childRect.left, rectBar.Height(), childRect.Width(), childRect.Height(), SWP_NOACTIVATE); TODOBRUNA
	}
}

//-----------------------------------------------------------------------------
BOOL CDockableFrame::Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, LPCTSTR lpszMenuName, DWORD dwExStyle, CCreateContext* pContext)
{
	if (!__super::Create(
		lpszClassName,
		lpszWindowName,
		dwStyle,
		CRect(0, 0, 1024, 768),
		pParentWnd,
		FALSE,
		dwExStyle,
		pContext))
		return FALSE;

	if (AfxGetThemeManager()->UseFlatStyle())
		AfxGetThemeManager()->MakeFlat(this);

	GetWindowRect(m_FloatingRect);

	if (m_hMenu != NULL)
	{
		CMenu* pMenu = CMenu::FromHandle(m_hMenu);
		if (pMenu)
		{
			AfxLoadMenuStrings(pMenu, REVERSEMAKEINTRESOURCE(lpszMenuName));
		}
	}
	DragAcceptFiles(FALSE);
	return TRUE;
}

//-----------------------------------------------------------------------------
CString CDockableFrame::GetScreenshotPath(CString ns)
{
	CString sPath = AfxGetPathFinder()->GetMenuThumbnailsFolderPath();
	LPTSTR pBuffer = sPath.GetBuffer(MAX_PATH);

	CString strHandle;
	strHandle.Format(_T("%s.jpg"), ns);
	PathAppend(pBuffer, strHandle);
	sPath.ReleaseBuffer();

	return sPath;
}

//-----------------------------------------------------------------------------
LRESULT CDockableFrame::OnChangeFrameStatus(WPARAM wParam, LPARAM lParam)
{
	if (!m_bDockable) return 0;

	BOOL bDocked = (BOOL)wParam;
	HWND hwndParent = (HWND)lParam;

	DWORD dwRemoveForDockStyle = WS_THICKFRAME | WS_CAPTION | WS_POPUP;
	DWORD dwAddForDockStyle = WS_CHILD;

	if (bDocked)   //draw window in tab rectangle
	{
		if (!m_bDocked)
		{
			m_bDocked = TRUE;
			//Prj. 6709 - eliminato flickering
			//GetWindowRect(m_FloatingRect);
			ModifyStyle(dwRemoveForDockStyle, dwAddForDockStyle);
			SetParent(CWnd::FromHandle(hwndParent));
			m_dwAttachedThreadId = GetWindowThreadProcessId(hwndParent, NULL);
			//VERIFY(AttachThreadInput(GetCurrentThreadId(), m_dwAttachedThreadId, TRUE));
		}
	}
	else  //draw window in original position with frame
	{
		if (m_bDocked)
		{
			SetParent(CWnd::FromHandle(hwndParent));
			m_bDocked = FALSE;
			ModifyStyle(dwAddForDockStyle, dwRemoveForDockStyle);
			SetMenu(CMenu::FromHandle(m_hMenu));
			SetWindowPos
			(
				AfxGetMenuWindow(),
				m_FloatingRect.left,
				m_FloatingRect.top,
				m_FloatingRect.Width(),
				m_FloatingRect.Height(),
				SWP_SHOWWINDOW | SWP_NOACTIVATE
			);
			//VERIFY(AttachThreadInput(GetCurrentThreadId(), m_dwAttachedThreadId, FALSE));
		}
	}

	return 0;
}

//-----------------------------------------------------------------------------
LRESULT CDockableFrame::OnUpdateExternalMenu(WPARAM wParam, LPARAM lParam)
{
	CCmdUI* pUI = (CCmdUI*)wParam;
	return pUI->DoUpdate(this, TRUE);
}

//-----------------------------------------------------------------------------
LRESULT CDockableFrame::OnActivateTab(WPARAM wParam, LPARAM lParam)
{
	for (int i = 0; i < m_arChilds.GetCount(); i++)
	{
		HWND hwnd = m_arChilds.GetAt(i);
		if (m_bChildManagement)
		{
			CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(CWnd::FromHandlePermanent(hwnd));
			if (pFrame && !pFrame->m_bIsFrameVisible)
				continue;
		}

		if (::IsWindow(hwnd))
			CWnd::FromHandle(hwnd)->ShowWindow(((BOOL)wParam) ? SW_SHOW : SW_HIDE);
	}
	return 0;
}

//-----------------------------------------------------------------------------------
LRESULT CDockableFrame::OnMenuMngResizing(WPARAM wParam, LPARAM lParam)
{
	m_bFromExternalResizing = TRUE;
	ShowWindow(SW_HIDE);
	
	return 0L;
}

//-----------------------------------------------------------------------------
void CDockableFrame::OnSize(UINT nType, int cx, int cy)
{
	__super::OnSize(nType, cx, cy);

	if (m_bFromExternalResizing)
	{
		ShowWindow(SW_SHOW);
		m_bFromExternalResizing = FALSE;
	}

}

//----------------------------------------------------------------------------
LRESULT CDockableFrame::OnExitSizeMove(WPARAM wParam, LPARAM lParam)
{
	ResumeLayout();
	// codice di ricalcolo dei bcg da non fare
	//__super::OnExitSizeMove(wParam, lParam);  

	CRect rect;
	GetWindowRect(rect);

	if (m_FreezedRect.Width() == rect.Width() && m_FreezedRect.Height() == rect.Height())
		return 0;

	rect.InflateRect(1, 1); //TODOLUCA: riusciremo a togliere sto schifo di inflate?
	SetWindowPos(NULL, 0, 0, rect.Width(), rect.Height(), SWP_NOMOVE | SWP_NOACTIVATE | SWP_FRAMECHANGED);
	return 0;
}

//--------------------------------------------------------------------------------------
BOOL CDockableFrame::IsLayoutSuspended(BOOL bDelayed /*= FALSE*/) const 
{ 
	if (AfxIsRemoteInterface())
		return TRUE;

	if (!m_bHasToolbar)
		return FALSE;

	if (!bDelayed)
		return m_bLayoutSuspended;

	if (!m_bAfterOnFrameCreated)
		return TRUE;

	return m_bLayoutSuspended || m_bDelayedLayoutSuspended;
}

//----------------------------------------------------------------------------
LRESULT CDockableFrame::OnEnterSizeMove(WPARAM wParam, LPARAM lParam)
{
	CRect rect;
	GetWindowRect(rect);

	m_FreezedRect = rect;

	SuspendLayout();
	return 0;
}

//----------------------------------------------------------------------------
void CDockableFrame::SuspendLayout()
{
	m_bLayoutSuspended = TRUE;

	CRect rect;
	GetWindowRect(rect);

	SendMessageToDescendants(UM_LAYOUT_SUSPENDED_CHANGED, (WPARAM) this);
}

//----------------------------------------------------------------------------
void CDockableFrame::ResumeLayout()
{
	m_bLayoutSuspended = FALSE;
	SendMessageToDescendants(UM_LAYOUT_SUSPENDED_CHANGED, (WPARAM) this);
}

//----------------------------------------------------------------------------
LRESULT CDockableFrame::OnExecuteFunction(WPARAM wParam, LPARAM lParam)
{
	CBaseFunctionWrapper* pWrapper = (CBaseFunctionWrapper*)wParam;
	return pWrapper->OnExecuteFunction();
}

//-----------------------------------------------------------------------------
LRESULT CDockableFrame::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
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
	CWndObjDescription* pFrameDesc = pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndObjDescription), strId);
	pFrameDesc->UpdateAttributes(this);
	pFrameDesc->m_Type = CWndObjDescription::Frame;

	AcceleratorArray accelerators;
	if (m_hAccelTable)
		accelerators.Add(m_hAccelTable);
	HACCEL hDocAccelerator = GetDocumentAccelerator();
	if (hDocAccelerator)
		accelerators.Add(hDocAccelerator);

	if (accelerators.GetCount())
	{
		//Se avevo gia valorizzato gli acceleratori per questa finestra, non devo ripassare la loro descrizione
		//(gli acceleratori non cambiano dinamicamente)
		if (pFrameDesc->m_pAccelerator == NULL)
		{
			pFrameDesc->m_pAccelerator = new CAcceleratorDescription(accelerators);
			pFrameDesc->SetUpdated(&pFrameDesc->m_pAccelerator);
		}
	}

	//aggiungo descrizione delle finestre figlie della frame (toolbar, view, statusbar)
	pFrameDesc->AddChildWindows(this);
	return (LRESULT)pFrameDesc;
}

//-----------------------------------------------------------------------------
void CDockableFrame::ActivateFrame(int nCmdShow)
{
	//la prima attivazione non ha effetto: la faccio nascere nascosta, così
	//può ridimensionarsi e riposizionarsi senza effetti sgradevoli, la riattiverò più avanti
	if (m_bFirstActivation)
	{
		if (nCmdShow == -1)
		{
			nCmdShow = SW_HIDE;
		}
		m_bFirstActivation = false;
	}

	__super::ActivateFrame(nCmdShow);

	if (m_bDockable)
	{
		CWinThread* pThread = AfxGetThread();
		if (pThread->m_pActiveWnd != this)
			pThread->m_pActiveWnd = this;
	}

	//PERASSO: lo commento, il problema originario sembra essere rientrato, e questa SendMessage mi causa 
	//l'invio di una KillFocus prematura nel parsed control in fase di apertura del documento, che
	//causa un travaso di un dato vuoto dalla finestra al dataobj inizializzato nella OnAttachData
	//annullando l'inizializzazione
	/********************************************************************************
		//Usato perche a volte la BringToTop chiamata da CFrameWnd::ActivateFrame fallisce.
		//es. nel caso di radar chiamato da hotlink, l'attivazione del frame di documento non va a buon fine
		//e di conseguenza il radar non venendo disattivato non muore
		//SendMessage(WM_ACTIVATE, WA_ACTIVE, (LPARAM)m_hWnd);
	********************************************************************************/
}

//-----------------------------------------------------------------------------
void CALLBACK FrameCaptured
(
	HWND hwnd,
	UINT uMsg,
	ULONG_PTR dwAttached,
	LRESULT lResult
)
{
	ASSERT_TRACE1(uMsg == UM_DOCUMENT_CREATED, "Bad uMsg parameter, must be UM_DOCUMENT_CREATED: uMsg = %d", uMsg);
	*((int*)dwAttached) = 1;
}


//-----------------------------------------------------------------------------
void CDockableFrame::SetWoormEditorVisible(BOOL isVisible) 
{
	m_bEditorFrameVisible = isVisible;
}

//-----------------------------------------------------------------------------
void CDockableFrame::OnFrameCreated()
{
	if (AfxIsRemoteInterface() || !m_bIsFrameVisible)
	{
		if (m_bChildManagement && m_pDockableParent && !m_bDockable)
			m_pDockableParent->AddChild(m_hWnd);

		AfxGetThreadContext()->AddWindowRef(m_hWnd, FALSE);
		return;
	}

	volatile int bReady = 0;
	if (m_bDockable)
	{
		if (AfxGetLoginContext()->GetDocked())
			ModifyStyle(WS_THICKFRAME | WS_CAPTION, WS_CHILD);

		if (AfxGetThreadContext()->m_bSendDocumentEventsToMenu)
		{
			//prima aspettavo che la callback venisse chiamata prima di uscire, per correzione dell'anomalia #16414 per
			//la quale il fuoco viene impostato troppo presto (la finestra è ancora invisibile e il fuoco viene impostato nel posto sbagliato)
			//questo però causa un forte rallentamento in fase di apertura dei report in particolare, per cui adesso faccio una sendmessage asincrona,
			//non aspetto ma mi tengo in un booleano quando la finestra è agganciata al menu
			//quando poi devo impostare il fuoco, lì aspetto che il booleano mi dia il via libera
			if (!::SendMessageCallback(AfxGetMenuWindowHandle(), UM_DOCUMENT_CREATED, (WPARAM)m_hWnd, (LPARAM)m_hMenu, &FrameCaptured, (ULONG_PTR)&bReady))
				bReady = 2;//se non ho mandato il messaggio al menu, posso proseguire come se fossi già agganciato, altrimenti la callback mi imposta a 1

			while (bReady == 0)
				// questa deve essere la pumpMessage di MFC e non quella di CTBwinThread::PumpThreadMessage() perche'
				// la PeekMessage in essa contenuta potrebbe andare ad anticipare dei dispatch di messaggi di disegno e
				// di conseguenza il documento runn-nato non si disegna bene (vd. scheduler o rundocument via adm sincrone)
				// Per contro questa istruzione che esegue la ::GetMessage() potrebbe essere avere effetti di loop di attesa
				// che per adesso non abbiamo ancora trovato.
				AfxPumpMessage();

		}
	}
	else
	{
		//se la finestra non e' visibile (szNoInterface), non devo metterla in lista, altrimenti mi verra' visualizzata nella OnActivateTab
		{
			if (m_pDockableParent)
				m_pDockableParent->AddChild(m_hWnd);
			AdjustFrameSize();
			CenterWindow();  //An: 19893
			
			AfxPumpMessage();
		}
	}

	AdjustTabbedToolBar();
	AfxGetThreadContext()->AddWindowRef(m_hWnd, FALSE);

	if (m_bEditorFrameVisible)	// modifica fatta per editor frame di woorm (in tutti i casi settato a true tranne il caricamento di EditView do woorm.)
		ShowWindow(SW_SHOW);//SW_MAXIMIZE non va bene perche' altera la dimensione della finestra dockata (fenomeni di finestra piccola dentro tab grande)
	/*else
	ShowWindow(SW_HIDE);*/
}
//-----------------------------------------------------------------------------
void CDockableFrame::AdjustFrameSize()
{
	CSize sizeframe = GetCalcFrameSize();
	if (sizeframe.cx != 0 || sizeframe.cy != 0)
	{
		CRect frameRect;
		this->GetClientRect(&frameRect);
		sizeframe.cx = max(frameRect.Width(), sizeframe.cx);
		sizeframe.cy += frameRect.Height();
		OnAdjustFrameSize(sizeframe);

		SetWindowPos
		(
			NULL, 0, 0, sizeframe.cx, sizeframe.cy,
			SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER

		);
	}
	AdjustTabbedToolBar();
}
//-----------------------------------------------------------------------------
CSize CDockableFrame::GetCalcFrameSize()
{
	return m_FrameSize;
}

//-----------------------------------------------------------------------------
void CDockableFrame::SetCalcFrameSize(CSize nSize)
{
	m_FrameSize = nSize;
}
//-----------------------------------------------------------------------------
void CDockableFrame::SendTitleUpdatesToMenu()
{
	//devo tenere allineato il titolo della tab di menumanager, gli comunico con un messaggio che e' cambiato
	::PostMessage(AfxGetMenuWindowHandle(), UM_FRAME_TITLE_UPDATED, (WPARAM)m_hWnd, NULL);
}

//-----------------------------------------------------------------------------
void CDockableFrame::OnDestroy()
{
	//when this assert occurs, a modal dialog box popped up when this window was 
	//still alive, but in process of dying; check the code that opend this message box
#ifdef DEBUG
	if (InModalState())
		TRACE("WARNING! YOU ARE CLOSING A WINDOW THAT IS IN MODAL STATE!\n");
#endif
	//avoid this window to be main window for a modal dialog
	CWinThread* pThread = AfxGetThread();
	CThreadContext* pThreadContext = AfxGetThreadContext();
	pThreadContext->RemoveActiveWnd(this);
	if (pThread->m_pActiveWnd == this)
		pThread->m_pActiveWnd = pThreadContext->GetLatestActiveWnd(NULL);
	if (pThreadContext->GetActiveDockableWnd() == this)
		pThreadContext->SetActiveDockableWnd(NULL);
	//avoid ASSERT in CFrameWnd::OnDestroy()
	m_hMenuDefault = NULL;
	if (::IsMenu(m_hMenu))
		::DestroyMenu(m_hMenu);

	__super::OnDestroy();

	/*CString sFile = GetBitmapFilePath(m_hWnd);
	if (::ExistFile(sFile))
		::DeleteFile(sFile);*/
}

//------------------------------------------------------------------------------------
int	 CDockableFrame::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	return __super::OnCreate(lpCreateStruct);
}

//-----------------------------------------------------------------------------
void CDockableFrame::PostNcDestroy()
{
	m_pViewActive = NULL;

	Dispose();

	__super::PostNcDestroy();
}

//-----------------------------------------------------------------------------
void CDockableFrame::GetPhantomBitmapFolderPaths(CStringArray& arPaths)
{
	CString sPath;
	LPTSTR pBuffer = sPath.GetBuffer(MAX_PATH);
	GetTempPath(MAX_PATH, pBuffer);
	PathAppend(pBuffer, _T("TBIMG-*"));
	sPath.ReleaseBuffer();

	CFileFind finder;
	BOOL bWorking = finder.FindFile(sPath);
	BOOL bResult = TRUE;
	while (bWorking)
	{
		bWorking = finder.FindNextFile();

		if (finder.IsDots())
			continue;

		if (finder.IsDirectory())
		{
			CString sFileName = finder.GetFileName();
			int i;
			_stscanf_s((LPCTSTR)sFileName, _T("TBIMG-%d"), &i);
			HANDLE h = OpenProcess(PROCESS_QUERY_INFORMATION, FALSE, i);
			if (h)
				CloseHandle(h);
			else
				arPaths.Add(finder.GetFilePath());
		}

	}
}

//-----------------------------------------------------------------------------
BOOL CDockableFrame::DestroyWindow()
{
	this->SuspendLayout();

	if (m_bDockable)
	{
		if (AfxGetThreadContext()->m_bSendDocumentEventsToMenu)
		{
			CWnd* pOldParent = SetParent(NULL);

			::PostMessage(AfxGetMenuWindowHandle(), UM_DOCUMENT_DESTROYED, (WPARAM)m_hWnd, NULL);
			//nascondo la finestra per evitare flickering quando la sgancio dal parent
			ShowWindow(SW_HIDE);
			//mi sgancio dal parent
			//se avevo un parent, gli comunico che sto per morire
			//e il parent (managed) effettuera` la dispose degli oggetti managed
			if (pOldParent)
				pOldParent->SendMessage(UM_DESTROYING_DOCKABLE_FRAME);
		}
		for (int i = 0; i < m_arChilds.GetCount(); i++)
		{
			CWnd *pWnd = CWnd::FromHandle(m_arChilds[i]);
			CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(pWnd);
			if (pFrame)
				pFrame->m_pDockableParent = NULL;

			break;
		}
	}
	else
	{
		if (m_pDockableParent)
		{
			for (int i = 0; i < m_pDockableParent->m_arChilds.GetCount(); i++)
				if (m_pDockableParent->m_arChilds.GetAt(i) == m_hWnd)
				{
					m_pDockableParent->m_arChilds.RemoveAt(i);
					break;
				}
		}
	}
	AfxGetThreadContext()->RemoveWindowRef(m_hWnd, FALSE);

	return __super::DestroyWindow();
}

//-----------------------------------------------------------------------------
void CDockableFrame::SetOwner(CWnd* pWndOwner)
{
	while (pWndOwner != NULL && (::GetWindowLong(pWndOwner->m_hWnd, GWL_STYLE) & WS_CHILD))
		pWndOwner = pWndOwner->GetParent();
	SetWindowLong(m_hWnd, GWL_HWNDPARENT, pWndOwner ? (LONG)pWndOwner->m_hWnd : 0L);
}
//-----------------------------------------------------------------------------
void CDockableFrame::SetOwner(HWND hWndOwner)
{
	SetOwner(CWnd::FromHandle(hWndOwner));
}
//-----------------------------------------------------------------------------
HWND CDockableFrame::GetValidOwner()
{
	CWnd* pActiveDockableWnd = AfxGetThreadContext()->GetLatestActiveWnd(this);
	CWnd* pParent = (pActiveDockableWnd != NULL) ? pActiveDockableWnd : AfxGetMainWnd();
	//devo impostare un parent al radar, altrimenti le messagebox non sono in grado di ritrovarlo con la GetLastActivePopup e non lo prendono come owner,
	//quindi si chiude sotto il culo della messagebox e successivamente si schianta
	while (pParent != NULL && (::GetWindowLong(pParent->m_hWnd, GWL_STYLE) & WS_CHILD))
		pParent = pParent->GetParent();
	return pParent->m_hWnd;
}

//-----------------------------------------------------------------------------
BOOL CDockableFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	DataObj* pDataObj = AfxGetSettingValue(snsTbGenlib, szFormsSection, _T("TabbedDocuments"), DataBool(TRUE), szTbDefaultSettingFileName);

	//il thread potrebbe impedire per qualche motivo il docking (es. perche' la finestra deve nascere invisibile)
	//la metto qui e non nel costruttore per evitare che il costruttore derivato me la sovrascriva
	if (
		!AfxGetThreadContext()->IsDockableFrameAllowed() ||
		(pDataObj != NULL && pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)) && (*(DataBool*)pDataObj) == FALSE)
		)
		m_bDockable = false;

	if (!__super::PreCreateWindow(cs)) return FALSE;

	if (!m_bDockable)  //non-dockable window, doesn't have to appear in the desktop task bar 
	{
		CWnd* pActiveDockableWnd = AfxGetThreadContext()->GetActiveDockableWnd();
		CWnd* pParent = (pActiveDockableWnd != NULL) ? pActiveDockableWnd : AfxGetMainWnd();

		cs.style |= WS_POPUP;

		// Nel caso venga lanciata per la parametrizzazione di un job schedulato
		// qui arriva cs.hwndParent NULL quindi diventerbbe figlia del desktop e nel caso di schedulazione di report
		// l'askdialog istanziata sarebbe modeless rispetto alla AdministrationConsole
		//		Germano 17/11/2015
		if (cs.hwndParent == NULL)
			cs.hwndParent = GetValidOwner();

		CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(pParent);
		if (pFrame)
		{
			m_pDockableParent = pFrame;
			CRect cRect;
			pFrame->GetWindowRect(cRect);
			// Center it 
			INT cyScren = ::GetSystemMetrics(SM_CYSCREEN);
			INT cxScren = ::GetSystemMetrics(SM_CXSCREEN);
			cs.y = cRect.top + ((cyScren / 2) - (cs.cy / 2));
			cs.x = cRect.left + ((cxScren / 2) - (cs.cx / 2));
		}
	}
	if (cs.hMenu)
		m_hMenu = cs.hMenu;
	return TRUE;
}

//-----------------------------------------------------------------------------
void CDockableFrame::RemoveChild(HWND hwndChild)
{
	for (int i = m_arChilds.GetUpperBound(); i >= 0; i--)
	{
		HWND hwnd = m_arChilds.GetAt(i);
		if (hwnd == hwndChild)
			m_arChilds.RemoveAt(i);
	}
}

//-----------------------------------------------------------------------------
BOOL CDockableFrame::PreTranslateMessage(MSG* pMsg)
{
	//mando CTRL TAB al tabber del menumanager qualora sia in modalità TABBED
	//altrimenti il documento si mangia il messaggio e non riesco a switchare di tab con la tastiera
	if (m_bDocked && pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_TAB && GetKeyState(VK_CONTROL) & 0x8000)
		::PostMessage(AfxGetMenuWindowHandle(), UM_SWITCH_ACTIVE_TAB, NULL, NULL);
	return __super::PreTranslateMessage(pMsg);
}
