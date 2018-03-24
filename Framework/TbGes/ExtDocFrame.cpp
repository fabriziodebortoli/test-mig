
#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\globals.h>
#include <TbGeneric\minmax.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\WebServiceStateObjects.h>

#include <TbGenlib\TBToolBar.h>
#include <TbGenlib\TBDockPane.h>
#include <TbGenlib\ExtStatusControlBar.h>
#include <TbGenlib\reswalk.h>
#include <TbGenlib\parsobj.h>
#include <TbGenlib\messages.h>
#include <TbGenlib\generic.h>

#include <TbGenlib\commands.hrc>
#include <TbGenlibManaged\HelpManager.h>
#include <TbGenlibUI\BrowserDlg.h>

#include <TBOleDb\SqlRecoveryManager.h>

#include <TbWoormEngine\report.h>

#include "browser.h"
#include "bodyedit.h"
#include "hotlink.h"
#include "dbt.h"
#include "barquery.h"
#include "extdoc.h"
#include "tabber.h"
#include "formmng.h"
#include "tbges.hrc"
#include "ReportListDlg.h"
#include "SoapFunctions.h"
#include "JsonFormEngineEx.h"
#include "extdocframe.h"

//................................. resources
#include <TbGenlib\generic.hjson> //JSON AUTOMATIC UPDATE

#include "extdoc.hjson" //JSON AUTOMATIC UPDATE
#include "barquery.hjson" //JSON AUTOMATIC UPDATE
#include "gesapp.hjson" //JSON AUTOMATIC UPDATE
#include "bodyedit.hjson" //JSON AUTOMATIC UPDATE
#include "JsonForms\TbGes\IDD_EXTDOC_TOOLBAR.hjson"
#include "JsonForms\TbGes\IDD_EXTDOC_BATCH_TOOLBAR.hjson"
#include "JsonForms\TbGes\IDD_EXTDOC_ROWFORM_TOOLBAR.hjson"
#include "JsonForms\TbGes\IDD_EXTDOC_WIZARDBATCH_TOOLBAR.hjson"
#include "JsonForms\TbGes\IDD_EXTDOC_SLAVE_TOOLBAR.hjson"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"
//===========================================================================

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define MIN_TOOLBAR_NORMAL_SIZE 650
#define STANDARD_COMMAND_PREFIX _T("ToolbarButton.Framework.TbGes.TbGes.AbstractFormDoc.")

const TCHAR szToolbarNameMain[] = _T("Main");
const TCHAR szToolbarNameTools[] = TOOLBAR_NAMETOOLS;
const TCHAR szToolbarNameExport[] = _T("Export");
const TCHAR szToolbarNameAux[] = _T("Aux");

#define IDS_ADMINS_DOCUMENT GET_ID(IDS_ADMINS_DOCUMENT)
static UINT IDS_IDS_ADMINS_DOCUMENT = GET_IDS(IDS_ADMINS_DOCUMENT, _TB_STRING("Admin"), L"\\Framework\\TbGes\\EXTDOC.hjson");

//-----------------------------------------------------------------------------
CString StdNamespace(const CString& aName)
{
	return CString(STANDARD_COMMAND_PREFIX) + aName;
}

//===========================================================================
class TB_EXPORT CStatusBarNoResizable : public CTaskBuilderStatusBar
{
	DECLARE_MESSAGE_MAP();
protected:
	BOOL PreCreateWindow(CREATESTRUCT& cs);
	LRESULT OnSetText(WPARAM, LPARAM lParam);

};

/////////////////////////////////////////////////////////////////////////////
// CStatusBarNoResizable

BEGIN_MESSAGE_MAP(CStatusBarNoResizable, CTaskBuilderStatusBar)
	ON_MESSAGE(WM_SETTEXT, &CStatusBarNoResizable::OnSetText)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CStatusBarNoResizable::PreCreateWindow(CREATESTRUCT& cs)
{
	cs.style = cs.style &~SBARS_SIZEGRIP;
	return __super::PreCreateWindow(cs);
}

//-----------------------------------------------------------------------------
LRESULT CStatusBarNoResizable::OnSetText(WPARAM, LPARAM lParam)
{
	ASSERT_VALID(this);
	ASSERT(::IsWindow(m_hWnd));

	return SetPaneText(0, (LPCTSTR)lParam) ? 0 : -1;
}

/////////////////////////////////////////////////////////////////////////////
// CAbstractFrame
//============================================================================

IMPLEMENT_DYNCREATE(CAbstractFrame, CBaseFrame)

BEGIN_MESSAGE_MAP(CAbstractFrame, CBaseFrame)
	//{{AFX_MSG_MAP(CAbstractFrame)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTW, 0, 0xFFFF, OnToolTipText)
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTA, 0, 0xFFFF, OnToolTipText)

	ON_MESSAGE(UM_GET_SOAP_PORT, OnGetSoapPort)
	ON_MESSAGE(UM_GET_DOC_NAMESPACE_ICON, OnGetNamespaceAndIcon)
	ON_MESSAGE(UM_GET_DOCUMENT_TITLE_INFO, OnGetDocumentTitleInfo)
	ON_MESSAGE(UM_EXTDOC_FETCH, OnFetchRecord)

	ON_REGISTERED_MESSAGE(BCGM_ON_CLICK_CAPTIONBAR_HYPERLINK, OnCaptionBarHyperlinkClicked)

	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	//}}AFX_MSG_MAP

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAbstractFrame::CAbstractFrame()
	:
	CSplittedForm(this),
	m_pStatusBar(NULL),
	m_pCaptionBar(NULL),
	m_pTabbedToolBar(NULL),
	m_nIcon(0),
	m_bAddLoginDataInTitle(TRUE),
	m_pToolBarActive(NULL)
{
	m_bHasStatusBar = AfxGetThemeManager()->HasStatusBar() && !AfxIsInUnattendedMode();
	m_bHasToolbar = !AfxIsInUnattendedMode();
}

//-----------------------------------------------------------------------------
CAbstractFrame::~CAbstractFrame()
{
	SAFE_DELETE(m_pStatusBar);

	SAFE_DELETE(m_pTabbedToolBar);
	SAFE_DELETE(m_pCaptionBar);
}

//-----------------------------------------------------------------------------
void CAbstractFrame::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	// TabbedToolBar Width Size Modal - overlap of ico
	if (m_pTabbedToolBar && !AfxGetThemeManager()->ButtonsOverlap())
		lpMMI->ptMinTrackSize.x = m_pTabbedToolBar->CalcMinimumWidth();
}

//-----------------------------------------------------------------------------
void CAbstractFrame::OnSize(UINT nType, int cx, int cy)
{
	if (IsLayoutSuspended())
		return;

	__super::OnSize(nType, cx, cy);

	if (m_pTabbedToolBar)
		m_pTabbedToolBar->SetWindowPos(NULL, 0, 0, cx, m_pTabbedToolBar->CalcMaxButtonHeight(), SWP_NOMOVE | SWP_NOZORDER);

}

//-----------------------------------------------------------------------------
HACCEL CAbstractFrame::GetDocumentAccelerator()
{
	CAbstractDoc* pDoc = ((CAbstractDoc*)GetActiveDocument());
	if (!pDoc)
		return NULL;
	return pDoc->GetDocAccel();
}

//-----------------------------------------------------------------------------
CString CAbstractFrame::GetDocAccelText(WORD id)
{
	return GetAcceleratorText(m_hAccelTable, id);
}

// Reimplementa la versione base implementata in winmdi. 
//-----------------------------------------------------------------------------
BOOL CAbstractFrame::PreTranslateMessage(MSG* pMsg)
{
	CBaseDocument* pDocument = (CBaseDocument*)GetActiveDocument();
	if (pMsg->message >= WM_KEYFIRST && pMsg->message <= WM_KEYLAST && pDocument)
	{
		//Se il documento � in DESIGN mode (easybuilder) allora non applico le shortcut
		if (pDocument->IsInDesignMode())
			return FALSE;

		// use document specific accelerator table over m_hAccelTable
		HACCEL hDocAccel = GetDocumentAccelerator();
		CFrameWnd *pFrame = GetActiveFrame();
		if (pFrame &&
			((m_hAccelTable	&& ::TranslateAccelerator(pFrame->m_hWnd, m_hAccelTable, pMsg)) ||
			(hDocAccel		&& ::TranslateAccelerator(pFrame->m_hWnd, hDocAccel, pMsg))))
			return TRUE;
	}

	if (pMsg->message == WM_MOUSEWHEEL)
	{
		// CBCGPGlobalUtils::ProcessMouseWheel(WPARAM wParam, LPARAM lParam)
		// For disable the ProcessMouseWheel(WPARAM wParam, LPARAM lParam) of CBCGPGlobalUtils
		// send the message of object have the focus
		CWnd* pWndFocus = CWnd::GetFocus();
		if (pWndFocus)
		{
			CParsedCombo* pCombo = CParsedCombo::IsChildEditCombo(pWndFocus);
			if (pCombo)
			{
				if (pCombo->GetDroppedState())
				{
					pWndFocus->SendMessage(WM_MOUSEWHEEL, pMsg->wParam, pMsg->lParam);
					return TRUE;
				}
			}
		}
	}

	return __super::PreTranslateMessage(pMsg);
}

//-----------------------------------------------------------------------------
BOOL CAbstractFrame::DestroyWindow()
{
	SetDestroying();
	return __super::DestroyWindow();
}

static int s_ID_GotoMaster = ID_EXTDOC_GOTO_MASTER;	

//-----------------------------------------------------------------------------
BOOL CAbstractFrame::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	__try
	{
		// dopo la chiamata alla __super::OnCmdMsg this � stato distrutto ed i suoi datamember sono "cacca"
		BOOL bDestroying = m_bDestroying;
		HWND hw = m_hWnd;	//il flag sopra non basta, a volte muore nella chiamata __super::OnCmdMsg
		BOOL existToolbar = m_pTabbedToolBar != NULL;
		BOOL bHandled = __super::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);

		// la toolbar potrebbe morire durante il processo di closing
		// per sicurezza controllo che non si sia in distruzione
		// del frame
		if (bDestroying || nID == ID_FILE_CLOSE || nID == s_ID_GotoMaster)
			return bHandled;

		if (!pHandlerInfo && nID && existToolbar && (nCode == CN_COMMAND || nCode == BN_CLICKED))
		{
			//paramento per morte in chiamata __super::OnCmdMsg
			if (hw && !::IsWindow(hw))	//ha un costo, ma comunque marginale rispetto all'azione che segue
				return bHandled;

			ASSERT_VALID(m_pTabbedToolBar);
			CTBToolBar* pToolBar = m_pTabbedToolBar->FindToolBar(nID);

			if (pToolBar)
			{
				int nIndex = pToolBar->FindButton(nID);

				if (nIndex < 0 || nIndex >= pToolBar->GetCount())
					return bHandled;

				CTBToolbarMenuButton* pMenuButton = dynamic_cast<CTBToolbarMenuButton*> (pToolBar->GetButton(nIndex));

				if (pMenuButton && pMenuButton->IsVisible() && !pMenuButton->IsHidden() && pMenuButton->GetAlwaysDropDown() == MIXED_ALWAYS_DROPDOWN)
				{
					CWnd* pWnd = GetActiveWindow();
					if (pWnd == this || pWnd == AfxGetThreadContext()->GetMenuWindow())
						pMenuButton->SetMissingClick(TRUE);
				}
			}
		}
		return bHandled;
	}
	__except (s_pfExpFilter(GetExceptionInformation(), GetExceptionCode()))
	{
		AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)		
		return TRUE;
	}
}

//-----------------------------------------------------------------------------
BOOL CAbstractFrame::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if (m_pCaptionBar && !m_pCaptionBar->CanExecuteCommand(nID, nCode))
		return TRUE;

	if (m_pTabbedToolBar)
	{
		CTBToolBar* pToolBar = m_pTabbedToolBar->FindToolBar(nID);

		if (pToolBar) for (int i = 0; i < pToolBar->m_arInfoOSL.GetSize(); i++)
		{
			CInfoOSLButton* pInfo = pToolBar->m_arInfoOSL.GetAt(i);
			if (pInfo->m_nID == nID)
			{
				if (pInfo)
				{
					CDocument* pDoc = GetActiveDocument();
					if (
						(
							pDoc
							&&
							((CAbstractFormDoc*)pDoc)->GetFormMode() == CBaseDocument::NEW
							&&
							!OSL_CAN_DO(pInfo, OSL_GRANT_NEW)
							)
						||
						(
							pDoc
							&&
							((CAbstractFormDoc*)pDoc)->GetFormMode() == CBaseDocument::EDIT
							&&
							!OSL_CAN_DO(pInfo, OSL_GRANT_EDIT)
							)
						)
					{
						AfxMessageBox(_TB("User hasn't permission to execute this action. Please contact application administrator for obtain it. ") + pInfo->m_Namespace.ToString());
						return TRUE;
					}
				}
			}
		}
	}

	return __super::OnCommand(wParam, lParam);
}

// carical'icona scelta dal programmatore (se esiste, altrimenti default)
//-----------------------------------------------------------------------------
HICON CAbstractFrame::GetFrameIcon()
{
	if (AfxIsInUnattendedMode())
		return NULL;

	HICON hIcon = NULL;
	if (GetIconID() && (hIcon = LoadWalkIcon(GetIconID())) != NULL)
		return hIcon;

	CDocument* pActiveDoc = GetActiveDocument();
	if (
		pActiveDoc
		&&
		pActiveDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc))
		)
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)pActiveDoc;
		if (pDoc->GetType() == VMT_BATCH)
		{
			if (hIcon = TBLoadImage(TBGlyph(szIconBatch)))
				return hIcon;
		}
		else
		{
			if (hIcon = TBLoadImage(TBGlyph(szIconDocument)))
				return hIcon;
		}
	}

	return TBLoadImage(TBGlyph(szIconDocument));
}

//-----------------------------------------------------------------------------
void CAbstractFrame::SetFrameIcon()
{
	HICON hi = GetFrameIcon();
	if (hi == NULL)
		return;

	SET_ICON(m_hWnd, hi);
	::DeleteObject(hi);
}

//-----------------------------------------------------------------------------
HCURSOR CAbstractFrame::OnQueryDragIcon()
{
	// Can return HICON or HCURSOR here
	// Returning NULL gives simple "Black border" icon,
	// If icon, Windows whill convert to blak-and-white
	return (HCURSOR)NULL; //GetFrameIcon();
}
//-----------------------------------------------------------------------------
LRESULT CAbstractFrame::OnGetSoapPort(WPARAM wParam, LPARAM lParam)
{
	return AfxGetTbLoaderSOAPPort();
}
//-----------------------------------------------------------------------------
LRESULT CAbstractFrame::OnGetDocumentTitleInfo(WPARAM wParam, LPARAM lParam)
{
	CDocument* pDoc = GetActiveDocument();
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
	{
		TCHAR* pMsgBuff = (TCHAR*)wParam;
		UINT nSize = (UINT)lParam;

		//restituisco il testo della finestra, pi� la lunghezza del titolo
		//usato dal menu per sapere quanta parte del titolo finestra corrisponde al titolo documento
		//(il testo della finestra infatti inizia sempre col titolo del documento)
		CString strWindowText;
		GetWindowText(strWindowText);

		_tcscpy_s(pMsgBuff, nSize, strWindowText);
		return ((CBaseDocument*)pDoc)->GetTitle().GetLength();
	}
	return 0;
}
//-----------------------------------------------------------------------------
LRESULT CAbstractFrame::OnGetNamespaceAndIcon(WPARAM wParam, LPARAM lParam)
{
	CDocument* pDoc = GetActiveDocument();
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
	{
		TCHAR* pMsgBuff = (TCHAR*)wParam;
		UINT nSize = (UINT)lParam;
		_tcscpy_s(pMsgBuff, nSize, ((CBaseDocument*)pDoc)->GetNamespace().ToString());
	}
	return GetClassLong(m_hWnd, GCL_HICON);
}

//-----------------------------------------------------------------------------
LRESULT CAbstractFrame::OnFetchRecord(WPARAM wParam, LPARAM /* lParam */)
{
	SqlRecord* pRecToFetch = (SqlRecord*)wParam;
	CAbstractDoc* pAbstractDoc = (CAbstractDoc*)GetActiveDocument();
	if (pAbstractDoc && pAbstractDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		((CAbstractFormDoc*)pAbstractDoc)->GoInBrowserMode(pRecToFetch);
	return 0L;
}

//-----------------------------------------------------------------------------
void CAbstractFrame::OnUserHelpList(UINT nID)
{
}

//-----------------------------------------------------------------------------
void CAbstractFrame::GetMessageString(UINT nID, CString& rMessage) const
{
	//cast necessario perch� il 'this' � const
	CDocument* pActiveDoc = ((CAbstractFrame*)this)->GetActiveDocument();

	CString strMessage;
	if (
		pActiveDoc != NULL &&
		pActiveDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) &&
		((CAbstractFormDoc*)pActiveDoc)->GetToolTipText(nID, strMessage)
		)
	{
		// Riproduce il comportamento standard di MFC, per la status bar 
		// usa la parte di messaggio prima del primo \n
		int nPos = strMessage.Find(_T("\n"));
		if (nPos == -1)
			nPos = strMessage.GetLength();
		// torna il tip dinamico solo se non vuoto, altrimenti rimanda alla gestione standard di MFC
		if (nPos > 0)
		{
			rMessage = strMessage.Left(nPos);
			return;
		}
	}

	// gestione standard fatta da MFC
	__super::GetMessageString(nID, rMessage);
}

#define _tbcountof(array) (sizeof(array)/sizeof(array[0]))

//-----------------------------------------------------------------------------
BOOL CAbstractFrame::OnToolTipText(UINT id, NMHDR* pNMHDR, LRESULT* pResult)
{
	ASSERT(pNMHDR->code == TTN_NEEDTEXTA || pNMHDR->code == TTN_NEEDTEXTW);

	// con questa stessa routine gestiamo sia la versione ANSI che UNICODE del messaggio
	TOOLTIPTEXTA* pTTTA = (TOOLTIPTEXTA*)pNMHDR;
	TOOLTIPTEXTW* pTTTW = (TOOLTIPTEXTW*)pNMHDR;

	UINT nID = pNMHDR->idFrom;

	CDocument *pActiveDoc = GetActiveDocument();
	CString strMessage;
	// Gestisce dinamicamente solo i tooltips di toolbar, in ogni altro caso rimanda alla gestione
	// standard di MFC
	if (
		(	// non � un control, ma un bottone di toolbar
			pNMHDR->code == TTN_NEEDTEXTA && !(pTTTA->uFlags & TTF_IDISHWND) ||
			pNMHDR->code == TTN_NEEDTEXTW && !(pTTTW->uFlags & TTF_IDISHWND)
			) &&
			(nID != 0) && // nID � zero su un separatore
		(pActiveDoc != NULL) &&
		pActiveDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) &&
		((CAbstractFormDoc*)pActiveDoc)->GetToolTipText(nID, strMessage)
		)
	{
		// Riproduce il comportamento standard di MFC, per il tooltip 
		// usa la parte di messaggio dopo il primo \n
		int nPos = strMessage.Find(_T("\n"));
		if (nPos > -1) // se il \n non c'�, pazienza, seguir� la strada standard di MFC
		{
			CString strTipText;
			strTipText = strMessage.Mid(nPos + 1);
#ifndef _UNICODE
			if (pNMHDR->code == TTN_NEEDTEXTA)
				lstrcpyn(pTTTA->szText, strTipText, _tbcountof(pTTTA->szText));
			else
				_mbstowcsz(pTTTW->szText, strTipText, _tbcountof(pTTTW->szText));
#else
			if (pNMHDR->code == TTN_NEEDTEXTA)
				_wcstombsz(pTTTA->szText, strTipText, _tbcountof(pTTTA->szText));
			else
				lstrcpyn(pTTTW->szText, strTipText, _tbcountof(pTTTW->szText));
#endif
			// ho gestito dinamicamente il tooltip
			return TRUE;
	}
		else
		{
			if (pNMHDR->code == TTN_NEEDTEXTA)
				_wcstombsz(pTTTA->szText, strMessage, _tbcountof(pTTTA->szText));
			else
				lstrcpyn(pTTTW->szText, strMessage, _tbcountof(pTTTW->szText));
			// ho gestito dinamicamente il tooltip
			return TRUE;
		}
}

	// nessuna gestione dinamica, rimanda alla gestione standard di MFC
	return CLocalizableFrame::OnToolTipText(id, pNMHDR, pResult);
}



/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CAbstractFrame::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CAbstractFrame\n");
	CLocalizableFrame::Dump(dc);
}

void CAbstractFrame::AssertValid() const
{
	CLocalizableFrame::AssertValid();
}
#endif //_DEBUG

////////////////////////////////////////////////////////////////////////////////
//==============================================================================
//	CAbstractFormFrame
//==============================================================================
////////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CFrameStepper, CBCGPDialogBar)
	ON_WM_SIZE()
END_MESSAGE_MAP()
//-----------------------------------------------------------------------------
CFrameStepper::CFrameStepper()
	:
	m_pStepper(NULL),
	m_pStepperDataObj(NULL)
{
	EnableVisualManagerStyle();
}

//-----------------------------------------------------------------------------
CFrameStepper::~CFrameStepper()
{
	delete m_pStepper;
	delete m_pStepperDataObj;
}

//-----------------------------------------------------------------------------
/* static */ CString CFrameStepper::GetStepperName()
{
	return _T("FrameStepper");
}

//-----------------------------------------------------------------------------
CTaskBuilderBreadcrumb* CFrameStepper::GetStepper()
{
	return m_pStepper;
}

//-----------------------------------------------------------------------------
DataStr* CFrameStepper::GetStepperDataObj()
{
	return m_pStepperDataObj;
}

//-----------------------------------------------------------------------------
void CFrameStepper::CreateStepper(CAbstractFormFrame* pParent, CRuntimeClass* pStepperClass)
{

	ASSERT(m_pStepper == NULL);
	ASSERT(m_pStepperDataObj == NULL);
	ASSERT(pParent);
	if (!pStepperClass)
		pStepperClass = RUNTIME_CLASS(CTaskBuilderBreadcrumb);

	UINT nDockBarID = AfxGetTBResourcesMap()->GetTbResourceID(_T("StepperBar"), TbControls);
	// creazione della docking bar
	if (!__super::Create(_T("Breadcrumb"), pParent, FALSE, MAKEINTRESOURCE(IDD_EXTDOC_BREADCRUMB_BAR), CBRS_ALIGN_TOP, nDockBarID, CBRS_BCGP_REGULAR_TABS, 0))
	{
		TRACE0("Failed to create breadcrumb bar\n");
		return;
	}

	m_pStepperDataObj = new DataStr();
	m_pStepperDataObj->SetAlwaysReadOnly(TRUE);

	// namespace del bread crumb
	CAbstractFormDoc* pDoc = ::GetDocument(pParent);
	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::CONTROL, GetStepperName(), pDoc->GetNamespace());
	UINT nID = AfxGetTBResourcesMap()->GetTbResourceID(aNs.ToString(), TbControls);

	// creo il bread crumb e lo setto
	m_pStepper = (CTaskBuilderBreadcrumb*)pStepperClass->CreateObject();
	m_pStepper->CParsedCtrl::Attach(m_pStepperDataObj);
	m_pStepper->AttachDocument(pDoc);

	if (!m_pStepper->Create(WS_VISIBLE | WS_CHILD | BCCS_SHOWROOTALWAYS, CRect(0, 0, 21, 1000), this, nID))
	{
		ASSERT_TRACE(FALSE, "CTabberStepper::CreateStepper: Error creating break crumble bar in frame!!");
		return;
	}

	m_pStepper->UpdateCtrlStatus();
	m_pStepper->UpdateCtrlView();

	EnableDocking(CBRS_ALIGN_ANY);
	pParent->DockControlBar(this);
	pParent->ShowControlBar(this, TRUE, FALSE, FALSE);

}

//-----------------------------------------------------------------------------
void CFrameStepper::SetHeight(int nHeight)
{
	CRect aRect;
	GetClientRect(&aRect);

	SetWindowPos(NULL, 0, 0, aRect.Width(), nHeight, SWP_NOMOVE | SWP_NOZORDER);
}

// mi passo la direzione del passo: -1 back; 0 = fermo; 1 = next
//-----------------------------------------------------------------------------
void CFrameStepper::UpdateStepper(const CString& sRootDescri, CTabManager* pTabManager, int nDirection /*1*/)
{
	if (!m_pStepperDataObj || !m_pStepper)
		return;

	m_pStepper->RemoveAll();

	// prima sistemo la root
	CTaskBuilderBreadcrumbItem* pRoot = m_pStepper->GetRootItem();
	CString sRoot = StripPath(sRootDescri);
	CString strPath = sRoot;
	pRoot->SetText(sRoot);

	// aggiorno gli steps
	CTabDialog* pActiveDlg = pTabManager->GetActiveDlg();
	if (pActiveDlg)
	{
		// e' la prima dialog riparto
		if (pTabManager->GetDlgInfoArray()->GetAt(0) == pActiveDlg->GetDlgInfoItem())
		{
			m_arSteps.RemoveAll();
			m_arSteps.Add(StripPath(pActiveDlg->GetDlgInfoItem()->m_strTitle));
		}
		else
		{
			switch (nDirection)
			{
			case -1:
			{
				m_arSteps.RemoveAt(m_arSteps.GetUpperBound());
			}
			case 0:
				break;
			default:
				m_arSteps.Add(StripPath(pActiveDlg->GetDlgInfoItem()->m_strTitle));
			}
		}
	}

	// aggiorno faccio il refresh del bread crumb
	CTaskBuilderBreadcrumbItem* pCurrent = pRoot;
	for (int i = 0; i < m_arSteps.GetSize(); i++)
	{
		CString sTitle = m_arSteps.GetAt(i);
		pCurrent = pCurrent->AddItem(sTitle, sTitle);
		strPath = strPath + m_pStepper->GetDelimiter() + sTitle;
	}

	m_pStepperDataObj->Assign(strPath);
	m_pStepper->UpdateCtrlStatus();
	m_pStepper->UpdateCtrlView();
	m_pStepper->Invalidate();
	m_pStepper->UpdateWindow();
}

//-----------------------------------------------------------------------------
CString CFrameStepper::StripPath(const CString& strPath)
{
	CString sStripped = strPath;
	sStripped.Replace(_T("&"), _T(""));
	return sStripped;
}

//------------------------------------------------------------------------------
void CFrameStepper::OnSize(UINT nType, int cx, int cy)
{
	__super::OnSize(nType, cx, cy);

	DoRecalcSize(cx, cy);
}

//-----------------------------------------------------------------------------
void CFrameStepper::DoRecalcSize(int cx, int cy)
{
	if (!m_pStepper)
		return;

	CRect aRect;
	GetWindowRect(&aRect);
	GetParent()->ScreenToClient(&aRect);
	m_pStepper->SetWindowPos(NULL, 0, 0, cx, aRect.Height(), SWP_NOMOVE | SWP_NOZORDER);
}

//-----------------------------------------------------------------------------
void CAbstractFrame::SetAddLoginDataInTitle(BOOL bAdd)
{
	m_bAddLoginDataInTitle = bAdd;
}

//-----------------------------------------------------------------------------
const BOOL&	 CAbstractFrame::GetAddLoginDataInTitle() const
{
	return m_bAddLoginDataInTitle;
}


//@@TODO DA MODIFICARE ENTRAMBI PER TOGLIERE I PULSANTI ID_EXTDOC_PROPERTIES e ID_EXTDOC_EDIT_QUERY

///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CAbstractFormFrame, CAbstractFrame)

BEGIN_MESSAGE_MAP(CAbstractFormFrame, CAbstractFrame)
	//{{AFX_MSG_MAP(CAbstractFormFrame)
	ON_WM_ACTIVATE()
	ON_WM_HELPINFO()

	ON_COMMAND_RANGE(ID_HELP_LIST, (UINT)(ID_HELP_LIST + 99), OnUserHelpList)
	ON_COMMAND(ID_HELP_INDEX, OnFormHelp)
	ON_UPDATE_COMMAND_UI(ID_HELP_INDEX, OnUpdateFormHelp)
	ON_COMMAND(ID_EXTDOC_GOTO_PRODUCERSITE, OnGoToProducerSite)
	ON_COMMAND(ID_EXTDOC_GOTO_SITE_PRIVATE_AREA, OnGoToSitePrivateArea)
	ON_MESSAGE(UM_CLONE_DOCUMENT, OnCloneDocument)
	ON_MESSAGE(UM_IS_ROOT_DOCUMENT, OnIsRootDocument)
	ON_MESSAGE(UM_HAS_INVALID_VIEW, OnHasInvalidView)
	ON_MESSAGE(WM_ENABLE, OnEnable)
	ON_WM_TIMER()

	ON_NOTIFY(TBN_DROPDOWN, AFX_IDW_TOOLBAR, OnToolbarDropDown)
	//}}AFX_MSG_MAP

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAbstractFormFrame::CAbstractFormFrame()
	:
	m_pIDQueryArray(NULL),
	m_pStepper(NULL),
	m_pProgressBar(NULL)
{
}

//-----------------------------------------------------------------------------
CAbstractFormFrame::~CAbstractFormFrame()
{
	delete m_pIDQueryArray;
	delete m_pStepper;
	delete m_pToolbarContext;
	delete m_pProgressBar;
}
//-----------------------------------------------------------------------------
void CAbstractFormFrame::OnTimer(UINT nTimer)
{
	((CAbstractFormDoc*)GetDocument())->OnTimer(nTimer);
	__super::OnTimer(nTimer);
}
//-----------------------------------------------------------------------------
LRESULT CAbstractFormFrame::OnEnable(WPARAM wParam, LPARAM lParam)
{
	CDocument* pActiveDoc = GetActiveDocument();
	if (pActiveDoc && pActiveDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		CAbstractFormDoc* pDoc = ((CAbstractFormDoc*)pActiveDoc);
		if (pDoc->GetFrameHandle() == m_hWnd)
		{
			bool bDocDisabled = pDoc->IsDisabled();
			BOOL bEnabled = wParam;
			//se il documento � disabilitato, vince sullo stato della frame
			if (bDocDisabled && bEnabled)
				EnableWindow(FALSE);
		}
	}
	return 1L;
}

//-----------------------------------------------------------------------------
LRESULT CAbstractFormFrame::OnHasInvalidView(WPARAM /*wParam*/, LPARAM /*lParam*/)
{
	CAbstractFormDoc* pDocument = (CAbstractFormDoc*)GetActiveDocument();
	return pDocument && IsWindowEnabled() ? pDocument->GetNotValidView(TRUE) != NULL : FALSE;
}
//-----------------------------------------------------------------------------
LRESULT CAbstractFormFrame::OnIsRootDocument(WPARAM /*wParam*/, LPARAM /*lParam*/)
{
	CAbstractFormDoc* pDocument = (CAbstractFormDoc*)GetActiveDocument();
	//faccio eseguire la rundocument al thread di login
	if (AfxGetThreadContext()->GetDocuments().GetCount() == 0 || AfxGetThreadContext()->GetMainDocument() == pDocument)
		return 1L;
	return 0L;
}
//-----------------------------------------------------------------------------
LRESULT CAbstractFormFrame::OnCloneDocument(WPARAM, LPARAM)
{
	CBaseDocument* pDocument = (CBaseDocument*)GetActiveDocument();
	//faccio eseguire la rundocument al thread di login
	if (pDocument)
		AfxInvokeAsyncThreadGlobalFunction<DataLng, DataStr, DataStr>(AfxGetLoginContext()->m_nThreadID, &RunDocument, pDocument->GetNamespace().ToString(), _T(""));
	return 1L;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::LoadFrame(UINT nIDResource, DWORD dwDefaultStyle, CWnd* pParentWnd, CCreateContext* pContext)
{
	CBCGPDockManager* pDockManager = GetDockManager();
	if (pDockManager)
	{
		pDockManager->DisableRestoreDockState(TRUE);
		EnableLoadDockState(FALSE);
	}
	BOOL bRes = CAbstractFrame::LoadFrame(nIDResource, dwDefaultStyle, pParentWnd, pContext);
	if (bRes && !AfxIsRemoteInterface())
	{
		CreateAccelerator();
		if (m_pTabbedToolBar)
		{
			CList<int, int>	listID;
			m_pTabbedToolBar->GetRemovedListID(&listID);
			RemovedAcceleratorList(&listID);
		}
	}
	return bRes;
}

//-----------------------------------------------------------------------------
CTBNamespace CAbstractFormFrame::GetAuxToolBarButtonNameSpace(int nID)
{
	//TODO:TB
	ASSERT(FALSE);
	return CTBNamespace(_T(""));
}

//---------------------------------------------------------------------------------------------
int CAbstractFormFrame::GetAuxToolbarCommandID(const CTBNamespace aNS)
{
	//TODO:TB
	ASSERT(FALSE);
	return -1;
}

//-----------------------------------------------------------------------------
CTaskBuilderCaptionBar*	CAbstractFrame::GetCaptionBar()
{
	return m_pCaptionBar;
}

//-----------------------------------------------------------------------------
void CAbstractFrame::SetToolBarActive(CWnd* pCWnd)
{
	if (!pCWnd) return;
	CTBToolBar* pToolBar = dynamic_cast<CTBToolBar*> (pCWnd);
	if (pToolBar)
		m_pToolBarActive = pToolBar;
	else
		m_pToolBarActive = NULL;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFrame::OnDrawMenuImage(CDC* pDC, const CBCGPToolbarMenuButton* pMenuButton, const CRect& rectImage)
{
	BOOL bIcon = FALSE;
	HICON hIcon = NULL;

	if (m_pToolBarActive)
	{
		hIcon = m_pToolBarActive->GetIconDropdownMenu(pMenuButton);
	}
	else if (m_pTabbedToolBar && pMenuButton)
	{
		hIcon = m_pTabbedToolBar->GetIconDropdownMenu(pMenuButton);
	}


	if (hIcon)
	{
		// Calculate for center the iconI 
		CSize sz = rectImage.Size();
		int xSpace = (int)((sz.cx / 2) - (TOOLBARMENU_ICON_SIZE / 2));
		int ySpace = (int)((sz.cy / 2) - (TOOLBARMENU_ICON_SIZE / 2));
		int xPos = rectImage.left + xSpace;
		int yPos = rectImage.top + ySpace;

		// Draw the icon 
		::DrawIconEx(pDC->GetSafeHdc(), xPos, yPos, hIcon, TOOLBARMENU_ICON_SIZE, TOOLBARMENU_ICON_SIZE, 0, NULL, DI_NORMAL);
		bIcon = TRUE;
	}


	// Draw the Checked
	if (pMenuButton->m_nStyle & TBBS_CHECKED)
	{
		if (bIcon)
		{
			CRect rect = CRect(rectImage.left + TOOLBARMENU_ICON_SIZE, rectImage.top, rectImage.right + TOOLBARMENU_ICON_SIZE, rectImage.bottom);
			CBCGPMenuImages::Draw(pDC, (CBCGPMenuImages::IMAGES_IDS) CBCGPMenuImages::IdCheck, rect, CBCGPMenuImages::ImageGray);
		}
		else
		{
			CBCGPVisualManager::GetInstance()->OnDrawMenuCheck(pDC, (CBCGPToolbarMenuButton*)pMenuButton, rectImage, 16, FALSE);
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFrame::CreateCaptionBar(UINT nID, CString strName)
{
	if (!this->IsKindOf(RUNTIME_CLASS(CBCGPFrameWnd)))
	{
		ASSERT_TRACE(FALSE, "Caption bar is supported only on CBCGPFrameWnd version build!");
		return FALSE;
	}

	if (!m_pCaptionBar)
		m_pCaptionBar = new CTaskBuilderCaptionBar();

	return m_pCaptionBar->Create(nID, this);
}

//----------------------------------------------------------------------------
LRESULT CAbstractFrame::OnCaptionBarHyperlinkClicked(WPARAM wParam, LPARAM lParam)
{
	if (m_pCaptionBar && m_pCaptionBar->CanExecuteLink())
		return SendMessage(UM_CAPTION_BAR_HYPERLINK_CLICKED, NULL, NULL);

	return 0L;
}

//----------------------------------------------------------------------------
void CAbstractFormFrame::OnFrameCreated()
{
	CreateToolBar();
	__super::OnFrameCreated();
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::CreateToolBar()
{
	if (!m_bHasToolbar || m_pTabbedToolBar)
		return TRUE;
	// prima creo la tabbed toolbar
	if (!CreateTabbedToolBar())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::CreateTabbedToolBar()
{
	// Tabbed Toolbar Add
	// ------------------------------------------------------------------------------------------
	ASSERT(m_pTabbedToolBar == NULL);
	m_pTabbedToolBar = new CTBTabbedToolbar();
	m_pTabbedToolBar->SuspendLayout();
	m_pTabbedToolBar->Create(this);
	//m_pTabbedToolBar->SetWindowText(_T("TabbedToolBar"));
	m_pTabbedToolBar->AttachOSLInfo(((CBaseDocument*)GetDocument())->GetInfoOSL());
	SetMenu(NULL);
	BOOL b = CreateJsonToolbar() &&
		OnCustomizeTabbedToolBar(m_pTabbedToolBar) &&
		OnAddClientDocToolbarButtons();

	ASSERT(b);
	return b;

}

//-----------------------------------------------------------------------------
CTBToolBar*	CAbstractFormFrame::CreateEmptyToolBar(LPCTSTR name, LPCTSTR text /*= NULL*/)
{
	CTBToolBar*	pToolBar = new CTBToolBar();

	if (!pToolBar->CreateEmptyTabbedToolbar(this, name, text))
	{
		TRACE("Failed to create the main toolBar.\n");
		return NULL;
	}
	return pToolBar;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::CreateJsonToolbar()
{
	if (!m_pToolbarContext)
		return TRUE;

	m_pToolbarContext->Associate(this);
	if (!m_pToolbarContext->m_pDescription)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	//valuto le espressioni della toolbar
	m_pToolbarContext->m_pDescription->EvaluateExpressions(m_pToolbarContext);
	return CreateJsonToolbar(m_pToolbarContext->m_pDescription);
}
//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::CreateJsonToolbar(CWndObjDescription* pDescription)
{
	switch (pDescription->m_Type)
	{
	case CWndObjDescription::TabbedToolbar:
	{

		CTabbedToolbarDescription* pTabbedDesc = (CTabbedToolbarDescription*)pDescription;
		for (int i = 0; i < pTabbedDesc->m_Children.GetCount(); i++)
		{
			CWndObjDescription* pDesc = pTabbedDesc->m_Children.GetAt(i);
			if (!pDesc->IsKindOf(RUNTIME_CLASS(CToolbarDescription)))
			{
				ASSERT(FALSE);
				continue;
			}
			//if (!((CJsonContext*)m_pToolbarContext)->CanCreateControl(pDesc))
			//	continue;

			CToolbarDescription* pToolBarDesc = (CToolbarDescription*)pDesc;
			CTBToolBar*	pToolBar = m_pTabbedToolBar->FindToolBarOrAdd(this, pToolBarDesc->m_strName);
			if (!pToolBar)
			{
				ASSERT(FALSE);
				continue;
			}

			CreateToolbarFromDesc(pToolBar, pToolBarDesc);
		}
		return TRUE;
	}
	case CWndObjDescription::Toolbar:
	{
		CToolbarDescription* pToolBarDesc = (CToolbarDescription*)pDescription;
		CTBToolBar*	pToolBar = m_pTabbedToolBar->FindToolBarOrAdd(this, pToolBarDesc->m_strName);
		if (!pToolBar)
		{
			ASSERT(FALSE);
		}
		CreateToolbarFromDesc(pToolBar, pToolBarDesc);
		//enable bottom docking 
		if (pToolBarDesc->m_bBottom)
			m_pTabbedToolBar->EnableDocking(this, CBRS_ALIGN_BOTTOM);
		else
			m_pTabbedToolBar->EnableDocking(this, CBRS_ALIGN_TOP);

		return TRUE;
	}
	}
	ASSERT(FALSE);
	return TRUE;
}
//-----------------------------------------------------------------------------
void CAbstractFormFrame::CreateToolbarFromDesc(CTBToolBar *pToolBar, CToolbarDescription* pToolBarDesc)
{

	bool separatorPending = false;

	//manage color for bottom toolbar
	if (pToolBarDesc->m_bBottom)
	{
		pToolBar->SetBkgColor(AfxGetThemeManager()->GetDialogToolbarBkgColor());
		pToolBar->SetForeColor(AfxGetThemeManager()->GetDialogToolbarForeColor());
		pToolBar->SetTextColor(AfxGetThemeManager()->GetDialogToolbarTextColor());
		pToolBar->SetTextColorHighlighted(AfxGetThemeManager()->GetDialogToolbarTextHighlightedColor());
		pToolBar->SetHighlightedColor(AfxGetThemeManager()->GetDialogToolbarHighlightedColor());
	}
	pToolBar->SetAutoHideToolBarButton(pToolBarDesc->m_bAutoHide);
	//manage toolbar color from json (when custom background color is wished and not standard color from theme)
	if (pToolBarDesc->m_crBkgColor != EMPTY_COLOR)
		pToolBar->SetBkgColor(pToolBarDesc->m_crBkgColor);

	for (int j = 0; j < pToolBarDesc->m_Children.GetCount(); j++)
	{
		CWndObjDescription* pDesc1 = pToolBarDesc->m_Children.GetAt(j);
		if (!pDesc1->IsKindOf(RUNTIME_CLASS(CToolbarBtnDescription)))
		{
			ASSERT(FALSE);
			continue;
		}
		UINT nId = CJsonFormEngineObj::GetID(pDesc1);
		if (!((CJsonContext*)m_pToolbarContext)->CanCreateControl(pDesc1, nId))
			continue;

		CToolbarBtnDescription* pToolBarBtnDesc = (CToolbarBtnDescription*)pDesc1;
		if (pToolBarBtnDesc->m_bIsSeparator)
		{
			//non metto subito il separatore, per non avere due o pi� separatori contigui nel caso di bottoni non visibili
			separatorPending = true;
			continue;
		}
		CWndObjDescription* pLocalRoot = pToolBarBtnDesc->GetRootOfFile();
		//ToolbarButton.Framework.TbGes.AbstractFormDoc
		CTBNamespace modNs = pLocalRoot->GetResource().GetOwnerNamespace();
		CTBNamespace btnNs(CTBNamespace::TOOLBARBUTTON,
			modNs.GetApplicationName() +
			_T(".") +
			modNs.GetModuleName() +
			_T(".") +
			(pToolBarDesc->m_strName.IsEmpty() ? pLocalRoot->GetResource().m_strName : pToolBarDesc->m_strName) +
			_T(".") +
			pToolBarBtnDesc->m_strName);
		CString sText = AfxLoadJsonString(pToolBarBtnDesc->m_strText, pToolBarBtnDesc);
		CString sHint = AfxLoadJsonString(pToolBarBtnDesc->m_strHint, pToolBarBtnDesc);
		//prima di creare un bottone, vedo se ho separatori rimasti in sospeso
		if (separatorPending && pToolBarBtnDesc->m_bVisible)
		{
			pToolBar->AddSeparator();
			separatorPending = false;
		}
		if (pToolBarBtnDesc->m_bRight)
		{
			pToolBar->AddButtonToRight(nId, btnNs.ToString(), pToolBarBtnDesc->m_sIcon, sText, sHint);
		}
		else
		{
			pToolBar->AddButton(nId, btnNs.ToString(), pToolBarBtnDesc->m_sIcon, sText, sHint);
		}
		if (pToolBarBtnDesc->m_bDefault)
			pToolBar->SetDefaultAction(nId);

		if (pToolBarBtnDesc->m_bIsDropdown)
		{
			pToolBar->SetDropdown(nId);
			for (int k = 0; k < pToolBarBtnDesc->m_Children.GetCount(); k++)
			{
				CWndObjDescription* pDescMenuItem = pToolBarBtnDesc->m_Children.GetAt(k);
				if (!pDescMenuItem->IsKindOf(RUNTIME_CLASS(CMenuItemDescription)))
				{
					ASSERT(FALSE);
					continue;
				}
				int itemId = CJsonFormEngineObj::GetID(pDescMenuItem);
				if (((CMenuItemDescription*)pDescMenuItem)->m_bIsSeparator)
				{
					pToolBar->AddDropdownMenuItemSeparator(nId);
					continue;
				}
				if (!((CJsonContext*)m_pToolbarContext)->CanCreateControl(pDescMenuItem, itemId))
				{
					continue;
				}
				CString sText = AfxLoadJsonString(pDescMenuItem->m_strText, pDescMenuItem);

				pToolBar->AddDropdownMenuItem(nId, MF_STRING, itemId, sText);
			}
		}
		if (!pToolBarBtnDesc->m_bVisible)
			pToolBar->HideButton(nId);

	}
}
//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::CreateJsonToolbar(UINT nID)
{
	CJsonResource res = AfxGetTBResourcesMap()->DecodeID(TbResources, nID);
	if (res.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	if (!m_pToolbarContext)
	{
		m_pToolbarContext = CJsonFormEngineObj::GetInstance()->CreateContext(res);
		m_pToolbarContext->Associate(this);
		return m_pToolbarContext && m_pToolbarContext->m_pDescription;
	}
	CJsonFormEngineObj::GetInstance()->MergeContext(res, m_pToolbarContext);
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::OnCustomizeJsonToolBar()
{
	return CreateJsonToolbar(IDD_EXTDOC_TOOLBAR);
}
//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	return TRUE;
}

static const UINT anExtDocStatusBarButtons[] =
{
	ID_EXTDOC_CONTROL_INDICATORS,
	ID_STATUS_BAR_SWITCH,
	ID_STATUS_BAR_HOME
};
// return the status bar pane position, -1 not found
//-----------------------------------------------------------------------------
INT CAbstractFormFrame::FindPane(INT nIDPane)
{
	for (int nIndexPane = 0; nIndexPane < m_pStatusBar->GetPanesCount(); nIndexPane++)
	{
		UINT nGetID;
		UINT nStyle;
		int  cxWidth;
		m_pStatusBar->GetPaneInfo(nIndexPane, nGetID, nStyle, cxWidth);
		if (nGetID == nIDPane)
		{
			return nIndexPane;
		}
	}
	return -1;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::CreateStatusBar()
{
	if (!m_bHasStatusBar)
		return TRUE;

	ASSERT(m_pStatusBar == NULL);
	m_pStatusBar = new CStatusBarNoResizable();
	if (!m_pStatusBar->Create(this))
	{
		TRACE("CAbstractFormFrame::CreateStatusBar() failed\n");
		ASSERT(FALSE);
		return FALSE;
	}

	if (!m_pStatusBar->SetIndicators(anExtDocStatusBarButtons, sizeof(anExtDocStatusBarButtons) / sizeof(UINT)))
	{
		TRACE("CAbstractFormFrame::CreateStatusBar() failed\n");
		ASSERT(FALSE);
		return FALSE;
	}

	SetPane(m_pStatusBar, ID_EXTDOC_CONTROL_INDICATORS, 100, SBPS_STRETCH);
	m_pStatusbarButtonHome = AddButtonToPane(m_pStatusBar, ID_STATUS_BAR_HOME, ID_EXTDOC_BACKTOMENU, _T("Home"), TBIcon(szIconBack, TOOLBAR), _TB("Return to Home"));
	m_pStatusbarButtonSwitch = AddButtonToPane(m_pStatusBar, ID_STATUS_BAR_SWITCH, ID_EXTDOC_SWITCHTO, _T("Switch"), TBIcon(szIconSwitchTo, TOOLBAR), _TB("Switch Document"));
	return TRUE;
}

//-----------------------------------------------------------------------------
void CAbstractFormFrame::SetStatusBarText(const CString& strText)
{
	if (m_pStatusBar) //I WizardDoc non hanno la StatusBar
	{
		INT nPaine = FindPane(ID_EXTDOC_CONTROL_INDICATORS);
		if (nPaine >= 0) {
			m_pStatusBar->SetPaneText(nPaine, strText);
		}
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormFrame::DoModal()
{
	CDockableFrame* pDockableParent = GetDockableParent();
	BOOL wasDisabled = FALSE;
	if (pDockableParent)
		wasDisabled = pDockableParent->EnableWindow(FALSE);

	TBEventDisposablePtr<CAbstractFormFrame> frame = this;
	CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);
	AfxGetThreadContext()->RaiseCallBreakEvent();
	CTBWinThread::LoopUntil(&frame.m_Disposed);

	if (pDockableParent)
		pDockableParent->EnableWindow(!wasDisabled);
}
//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::CreateAccelerator()
{
	if (LoadAccelTable(MAKEINTRESOURCE(IDR_EXTDOC)))
		return TRUE;

	TRACE("CAbstractFormFrame::CreateAccelerator() failed\n");
	ASSERT(FALSE);
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::RemovedAcceleratorList(CList<int, int>* pList)
{
	int iNumAccelerators = CopyAcceleratorTable(m_hAccelTable, NULL, 0);
	int iNewNum = 0;

	ACCEL *pAccels = new ACCEL[iNumAccelerators];
	ACCEL *pNewAccels = new ACCEL[iNumAccelerators];

	// Copy the current table to the buffer
	VERIFY(CopyAcceleratorTable(m_hAccelTable, pAccels, iNumAccelerators) == iNumAccelerators);


	for (int k = 0; k < iNumAccelerators; k++) {
		int iID = pAccels[k].cmd;
		POSITION  pos = pList->Find(iID);
		if (pos == NULL)
		{
			pNewAccels[iNewNum].cmd = iID;
			pNewAccels[iNewNum].fVirt = pAccels[k].fVirt;
			pNewAccels[iNewNum].key = pAccels[k].key;
			iNewNum++;
		}
	}
	if (m_hAccelTable)
	{
		// Destroy the current table resource...
		VERIFY(DestroyAcceleratorTable(m_hAccelTable) == TRUE);
	}
	// ... create a new one, based on our modified table
	m_hAccelTable = CreateAcceleratorTable(pNewAccels, iNewNum);
	ASSERT(m_hAccelTable != NULL || AfxIsRemoteInterface());
	// Cleanup
	delete[] pAccels;
	delete[] pNewAccels;

	return TRUE;
}

#define ADD_MAGIC_X	4
#define ADD_MAGIC_Y	4

//------------------------------------------------------------------------------
void CAbstractFormFrame::SetFrameSize(CSize csDialogSize)
{
	if (csDialogSize.cx <= 0 || csDialogSize.cy <= 0)
		return;

	CRect	rect(0, 0, csDialogSize.cx, csDialogSize.cy);
	int		nNcHeight = 0;

	if (m_pTabbedToolBar)
	{
		CRect rectTabbedToolBar;
		m_pTabbedToolBar->GetWindowRect(&rectTabbedToolBar);
		nNcHeight += rectTabbedToolBar.Height();
		rect.bottom += rectTabbedToolBar.Height();
		rect.right = rect.left + max(m_pTabbedToolBar->CalcMinimumWidth(), rect.Width());
	}

	if (m_pStatusBar)
	{
		CRect rectStatusBar;
		m_pStatusBar->GetWindowRect(&rectStatusBar);
		nNcHeight += rectStatusBar.Height();
		rect.bottom += rectStatusBar.Height();
	}

	// altezza della parte non client del main frame assumendo
	// che statusbar e toolbar siano della stessa dimensione di quelle
	// nel corrente frame (accettabile approssimazione). Non tiene conto
	// del menu perche' nei nostri frame non ce lo mettiamo
	nNcHeight += ::GetSystemMetrics(SM_CYCAPTION);

	CMenu* pMenu = GetMenu();
	if (pMenu)
	{
		int nMenuHeight = ::GetSystemMetrics(SM_CYMENU);
		nNcHeight += nMenuHeight;
		rect.bottom += nMenuHeight;
	}

	// Per "riparare" un probabile buco aggiungo a mano i bordi spessi... 
	// la CalcWindowRect sembra, infatti, non tenerne correttamente conto
	int cxWndBorders = ::GetSystemMetrics(SM_CXSIZEFRAME) / 2;
	int cyWndBorders = ::GetSystemMetrics(SM_CYSIZEFRAME) / 2;
	rect.InflateRect(cxWndBorders, cyWndBorders);

	CalcWindowRect(&rect, CWnd::adjustOutside);

	CClientDC aDC(this);
	int nMaxWidth = aDC.GetDeviceCaps(HORZRES);
	int nMaxHeight = aDC.GetDeviceCaps(VERTRES) - nNcHeight;
	if (rect.Height() > nMaxHeight)
	{
		rect.bottom = rect.top + nMaxHeight;
		rect.right += ::GetSystemMetrics(SM_CXVSCROLL);
	}

	if (rect.Width() > nMaxWidth)
		rect.right = rect.left + nMaxWidth;

	// aggiunta del bordo
	nNcHeight += 25;
	rect.bottom += 25;

	SetCalcFrameSize(CSize(rect.Width(), nNcHeight));

	SetWindowPos
	(
		NULL, 0, 0, rect.Width(), rect.Height(),
		SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER

	);

	RedrawWindow();
}

//------------------------------------------------------------------------------
void CAbstractFormFrame::SetSubTitle(UINT nSubTitleID)
{
	void* pObject = GetActiveDocument() ? GetActiveDocument()->GetRuntimeClass() : GetRuntimeClass();

	CString strSubTitle;

	strSubTitle = AfxLoadTBString(nSubTitleID, GetDllInstance(pObject));
	SetSubTitle(strSubTitle);
}

//------------------------------------------------------------------------------
void CAbstractFormFrame::SetSubTitle(const CString& strSubTitle)
{
	m_strSubTitle = strSubTitle;
}

// aggiunge sia titolo del documento che il subtitolo del frame
//-----------------------------------------------------------------------------
void CAbstractFormFrame::OnUpdateFrameTitle(BOOL bAddToTitle)
{
	DoUpdateFrameTitle(bAddToTitle);
}

//-----------------------------------------------------------------------------
void CAbstractFormFrame::DoUpdateFrameTitle(BOOL bAddToTitle, BOOL bOnlySubTitle)
{
	if ((GetStyle() & FWS_ADDTOTITLE) == 0)
		return;     // leave child window alone!

	CDocument* pDocument = GetActiveDocument();
	if (bAddToTitle && pDocument != NULL)
	{
		CString old;
		CString text;
		GetWindowText(old);

		if (bOnlySubTitle)
			text = m_strSubTitle;
		else
		{
			// aggiunge il sub titolo al titolo principale			
			text = pDocument->GetTitle();
			if (!m_strSubTitle.IsEmpty())
				text += _T(" - ") + m_strSubTitle;

			if (m_bAddLoginDataInTitle)
			{
				text += _T(" - ");

				text += cwsprintf(_TB("Company: {0-%s} - User: {1-%s}"),
					AfxGetLoginInfos()->m_strCompanyName,
					AfxGetLoginInfos()->m_strUserName
				);

				text += cwsprintf(_TB(" - Operation date: [{0-%s}]"), AfxGetApplicationDate().Str(1));
			}

			// aggiunge il nome della query al titolo principale			
			if (!m_strQueryName.IsEmpty())
				text += _T(" (") + m_strQueryName + _T(")");
		}

		// set title if changed, but don't remove completely
		if (text != old)
		{
			SetWindowText(text);
			//comunico al menu che il titolo e' cambiato cosi` aggiorna il testo della tab
			SendTitleUpdatesToMenu();
		}
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormFrame::OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	CView* pView = GetActiveView();

	CAbstractFrame::OnActivate(nState, pWndOther, bMinimized);

	OnActivateHandler(nState != WA_INACTIVE, pView);

	CDocument *pActiveDoc = GetActiveDocument();
	if (!pActiveDoc)
		return;

	// Close popup menu in toolbar open
	CTBTabbedToolbar* pTabbedBar = GetTabbedToolBar();
	if (pTabbedBar)
	{
		pTabbedBar->ClosePopupMenu();
		pTabbedBar->AdjustLayoutImmediate();
		pTabbedBar->UpdateTabWnd();
	}

	// Close active popup menu in the document
	CBCGPPopupMenu*  pActivePopupMenu = CBCGPPopupMenu::GetSafeActivePopupMenu();
	if (pActivePopupMenu)
	{
		pActivePopupMenu->SendMessage(WM_CLOSE);
		pActivePopupMenu = NULL;
	}

	CAbstractFormDoc* pActiveFormDoc = dynamic_cast<CAbstractFormDoc*> (pActiveDoc);
	ASSERT(pActiveFormDoc);
	if (!pActiveFormDoc)
		return;

	// _UPDATE_CONTROLS_OPTIMAZED_BEGIN
	//	pActiveFormDoc->UpdateDataView();
	// _UPDATE_CONTROLS_OPTIMAZED_END

	pActiveFormDoc->DispatchOnActivate(this, nState, pWndOther, bMinimized);
}

//-----------------------------------------------------------------------------
void CAbstractFormFrame::OnActivateHandler(BOOL bActivate, CView* pView)
{
	if (bActivate && pView && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		((CAbstractFormView*)pView)->DoActivate(bActivate);
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::OnCreateClient(LPCREATESTRUCT lpcs, CCreateContext* pContext)
{
	// in caso di design intervengo a disabilitarla
	if (!GetTileDesignModeParams(pContext->m_pCurrentDoc)->HasStatusBar())
		m_bHasStatusBar = FALSE;

	if (!CreateStatusBar())
		return FALSE;

	if (!__super::OnCreateClient(lpcs, pContext))
		return FALSE;

	if (!OnCustomizeJsonToolBar())
		return FALSE;
	OnCreateStepper();

	if (AfxGetThemeManager()->UseFlatStyle())
		ModifyStyleEx(WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_WINDOWEDGE, 0);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CAbstractFormFrame::OnUpdateFormHelp(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}

//-----------------------------------------------------------------------------
void CAbstractFormFrame::OnFormHelp()
{
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();
	if (!pDoc)
	{
		ASSERT(FALSE);
		return;
	}

	ShowHelp(pDoc->GetNamespace().ToString());
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::OnHelpInfo(HELPINFO* pHelpInfo)
{
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();
	if (!pDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	if (pDoc->IsInDesignMode())
		return FALSE;
	return ShowHelp(pDoc->GetNamespace().ToString());

}

//-----------------------------------------------------------------------------
void CAbstractFormFrame::OnGoToProducerSite()
{
	ConnectToProducerSite();
}

//-----------------------------------------------------------------------------
void CAbstractFormFrame::OnGoToSitePrivateArea()
{
	ConnectToProducerSiteLoginPage();
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::PreTranslateMessage(MSG* pMsg)
{
	if (CAbstractFrame::PreTranslateMessage(pMsg))
		return TRUE;

	CDocument* pDoc = (CAbstractFormDoc*)GetActiveDocument();
	if (
		pDoc &&
		pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) &&
		((CAbstractFormDoc*)pDoc)->m_pClientDocs &&
		pMsg->message >= WM_KEYFIRST && pMsg->message <= WM_KEYLAST
		)
		return GetActiveFrame() ? ((CAbstractFormDoc*)pDoc)->m_pClientDocs->PreTranslateMsg(GetActiveFrame()->m_hWnd, pMsg) : FALSE;

	return FALSE;
}


//-----------------------------------------------------------------------------
BOOL CAbstractFormFrame::OnPopulatedDropDown(UINT nIdCommand)
{
	if (m_pTabbedToolBar)
	{
		m_pTabbedToolBar->OnPopulatedDropDown(nIdCommand);
	}

	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();

	CTBToolBarMenu menu;
	menu.CreateMenu();

	if (nIdCommand == ID_EXTDOC_EXEC_QUERY)
	{
		if (pDoc && OSL_CAN_DO(pDoc->GetInfoOSL(), OSL_GRANT_BROWSE | OSL_GRANT_BROWSE_EXTENDED))
		{
			CreateQueryMenu(OSL_CAN_DO(pDoc->GetInfoOSL(), OSL_GRANT_EDITQUERY));
		}
		else
		{
			m_pTabbedToolBar->RemoveDropdown(ID_EXTDOC_EXEC_QUERY);
		}
	}
	else if (nIdCommand == ID_EXTDOC_REPORT)
	{
		CReportMenuNode* pRoot = new CReportMenuNode;
		pRoot->SetNodeTag(_T("ROOT"));
		pRoot->SetVisible(FALSE);

		if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			CString s = pDoc->GetDefaultTitleForm();	//TODO mettere la descrizione
			BOOL bOverrideDefault(FALSE);
			if (!s.IsEmpty())
			{
				menu.AppendMenu(MF_STRING | MF_CHECKED, ID_EXTDOC_FORMREPORT, (LPTSTR)(LPCTSTR)s);
				bOverrideDefault = TRUE;
			}

			if (pDoc->m_pFormManager)
				pDoc->m_pFormManager->EnumReportAlias(pRoot);

			if (pRoot->GetSonsUpperBound() > ID_EXTDOC_DDTB_ENUMREPORT_MAX)
			{
				menu.AppendMenu(MF_STRING, ID_EXTDOC_LIST_REPORTS, (LPTSTR)(LPCTSTR)_TB("Reports list..."));
			}
			else
			{
				if (pRoot->GetMaxDepth() == 2)
				{
					int nID = ID_EXTDOC_DDTB_ENUMREPORT0;
					int nDefaultIdx = pDoc->m_pFormManager->GetIndexReportDefault();
					for (int i = 0; i <= min(pRoot->GetSonsUpperBound(), ID_EXTDOC_DDTB_ENUMREPORT_MAX); i++)
					{
						menu.AppendMenu(i == nDefaultIdx && !bOverrideDefault ? MF_STRING | MF_CHECKED : MF_STRING, nID++, (LPTSTR)(LPCTSTR)pRoot->GetSonAt(i)->GetNodeTag());
					}
				}
				else
				{
					//gestione gruppi
					int nID = ID_EXTDOC_DDTB_ENUMREPORT0;
					int nDefaultIdx = pDoc->m_pFormManager->GetIndexReportDefault();
					int currentIdx = 0;
					CString sMenutPath;
					BOOL bCheck(FALSE);

					//------------------------------------------------
					// gestisco tutti i nodi riguardati report aggiunti 
					// tramite customizzazione da interfaccia
					// che saranno finiti nella custom
					//------------------------------------------------
					BOOL bCustomReportExist = FALSE;
					BOOL bGrupsExists = FALSE;
					for (int i = 0; i <= min(pRoot->GetSonsUpperBound(), ID_EXTDOC_DDTB_ENUMREPORT_MAX); i++)
					{
						if (pRoot->GetSonAt(i)->GetSonsUpperBound() >= 0)
						{
							bGrupsExists = TRUE;
							continue; // � un gruppo...
						}

						bCheck = currentIdx == nDefaultIdx;
						sMenutPath = pRoot->GetSonAt(i)->GetNodeTag();
						menu.AppendMenu(bCheck && !bOverrideDefault ? MF_STRING | MF_CHECKED : MF_STRING, nID++, (LPTSTR)(LPCTSTR)sMenutPath);
						bCustomReportExist = TRUE;
						currentIdx++;
					}

					if (bCustomReportExist && bGrupsExists)
					{
						// aggiungo un separatore alla fine delle voci
						// dei report custom
						menu.AppendMenu(MF_SEPARATOR);
					}
					//------------------------------------------------

					int sonsCount = currentIdx;
					for (int i = 0; i <= min(pRoot->GetSonsUpperBound(), ID_EXTDOC_DDTB_ENUMREPORT_MAX); i++)
					{
						if (pRoot->GetSonAt(i)->GetSonsUpperBound() < 0)
							continue; // si tratta di un report Custom gi� inserito in precedenza

						if (pRoot->GetSonAt(i)->GetUseSubMenu())
						{
							// Devo annidare i sottomenu
							for (int j = 0; j <= min(pRoot->GetSonAt(i)->GetSonsUpperBound(), ID_EXTDOC_DDTB_ENUMREPORT_MAX); j++)
							{
								currentIdx = sonsCount + j;

								sMenutPath = pRoot->GetSonAt(i)->GetNodeTag() + _T("\\") + pRoot->GetSonAt(i)->GetSonAt(j)->GetNodeTag();
								bCheck = currentIdx == nDefaultIdx;
								if (!CTBToolbarMenuExtFunctions::AddMenuItem(menu.GetSafeHmenu(), sMenutPath, nID++, bCheck))
								{
									ASSERT(FALSE);
								}
							}

							BOOL bIsNotTheLastNode = (i + 1) <= min(pRoot->GetSonsUpperBound(), ID_EXTDOC_DDTB_ENUMREPORT_MAX);
							if (bIsNotTheLastNode)
							{
								BOOL bNextNodeUsesSubMenu = pRoot->GetSonAt(i + 1)->GetUseSubMenu();
								if (!bNextNodeUsesSubMenu)
									menu.AppendMenu(MF_SEPARATOR);
							}
						}
						else
						{
							// Non devo annidare i sottomenu ma devo usare un separatore
							int upperBound = min(pRoot->GetSonAt(i)->GetSonsUpperBound(), ID_EXTDOC_DDTB_ENUMREPORT_MAX);
							for (int j = 0; j <= upperBound; j++)
							{
								currentIdx = sonsCount + j;

								sMenutPath = pRoot->GetSonAt(i)->GetSonAt(j)->GetNodeTag();
								bCheck = currentIdx == nDefaultIdx;
								menu.AppendMenu(bCheck && !bOverrideDefault ? MF_STRING | MF_CHECKED : MF_STRING, nID++, (LPTSTR)(LPCTSTR)sMenutPath);
							}

							BOOL bIsNotTheLastNode = (i + 1) <= min(pRoot->GetSonsUpperBound(), ID_EXTDOC_DDTB_ENUMREPORT_MAX);
							if (bIsNotTheLastNode)
								menu.AppendMenu(MF_SEPARATOR);
						}
						sonsCount += pRoot->GetSonAt(i)->GetSonsUpperBound() + 1;
					}
				}
			}
			m_pTabbedToolBar->UpdateDropdownMenu(nIdCommand, &menu);
		}
		SAFE_DELETE(pRoot);
	}
	else if (nIdCommand == ID_EXTDOC_RADAR)
	{
		CStringArray arRadars;
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();

		if (pDoc  && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{

			if (pDoc->m_pFormManager)
				pDoc->m_pFormManager->EnumRadarAlias(arRadars);

			if (pDoc->CanDoRunWrmRadar())
				menu.AppendMenu(MF_STRING, ID_EXTDOC_NEWNRT, (LPTSTR)(LPCTSTR)_TB("Radar Report"), TBIcon(szIconRadarReport, TOOLBAR));

			menu.AppendMenu(MF_STRING, ID_EXTDOC_RADAR, (LPTSTR)(LPCTSTR)_TB("Radar"), TBIcon(szIconRadar, TOOLBAR));

			if (arRadars.GetSize())
				menu.AppendMenu(MF_SEPARATOR);

			int nID = ID_EXTDOC_DDTB_ENUMRADAR0;
			int nDefaultIdx = pDoc->m_pFormManager->GetIndexRadarDefault();
			for (int i = 0; i < min(arRadars.GetSize(), 10); i++)
			{
				menu.AppendMenu(i == nDefaultIdx ? MF_STRING | MF_CHECKED : MF_STRING, nID++, (LPTSTR)(LPCTSTR)arRadars.GetAt(i), TBIcon(szIconRadarReport, TOOLBAR));
			}
			m_pTabbedToolBar->UpdateDropdownMenu(nIdCommand, &menu);
		}
	}
	else if (nIdCommand == ID_EXTDOC_FIND)
	{
		menu.AppendMenu(MF_STRING, ID_EXTDOC_FIND, _TB("Specific"));

		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();
		if (pDoc && pDoc->AsQuery())
			menu.AppendMenu(MF_STRING, ID_EXTDOC_QUERY, _TB("By Query"), TBIcon(szQuery_Find, TOOLBAR));

		m_pTabbedToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	}
	else if (nIdCommand == ID_EXTDOC_SWITCHTO)
	{
		MakeSwitchTomenu(&menu);
		m_pTabbedToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	}
	else
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();
		if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			((CAbstractFormDoc*)pDoc)->DispatchToolbarDropDown(nIdCommand, menu);

			HMENU hMenuToolBar = m_pTabbedToolBar->CopyDropdownMenu(nIdCommand);
			if (hMenuToolBar)
			{
				CTBToolBarMenu tmpMenuCopy;
				tmpMenuCopy.Attach(hMenuToolBar);
				tmpMenuCopy.AppendFromMenu(&menu);
				m_pTabbedToolBar->UpdateDropdownMenu(nIdCommand, &tmpMenuCopy);
				tmpMenuCopy.Detach();
			}
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CAbstractFormFrame::OnToolbarDropDown(NMHDR* pnmh, LRESULT *plr)
{
	ASSERT(FALSE);

	NMTOOLBAR* pnmtb = (NMTOOLBAR*)pnmh;

	BOOL bShow = FALSE;
	BOOL bAuxToolbar = FALSE;

	// Switch on button command id's.
	CTBMenu menu;
	menu.CreatePopupMenu();
	if (pnmtb->iItem == ID_EXTDOC_EXEC_QUERY)
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();
		if (pDoc && OSL_CAN_DO(pDoc->GetInfoOSL(), OSL_GRANT_BROWSE | OSL_GRANT_BROWSE_EXTENDED))
			CreateQueryMenu(OSL_CAN_DO(pDoc->GetInfoOSL(), OSL_GRANT_EDITQUERY));
		return;
	}

	else if (pnmtb->iItem == ID_EXTDOC_REPORT)
	{
		//TODOLUCA, questo codice � parzialmente copiat in ExtDoc sotto il metodo GetAttachedReport()
		//Andrebbero unificati
		CStringArray arReports;
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();
		CReportMenuNode* pRoot = new CReportMenuNode;
		pRoot->SetNodeTag(_T("ROOT"));
		pRoot->SetVisible(FALSE);

		if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{

			CString s = pDoc->GetDefaultTitleForm();	//TODO mettere la descrizione
			BOOL bOverrideDefault(FALSE);
			if (!s.IsEmpty())
			{
				menu.AppendMenu(MF_STRING | MF_CHECKED, ID_EXTDOC_FORMREPORT, (LPTSTR)(LPCTSTR)s);
				bOverrideDefault = TRUE;
			}

			if (pDoc->m_pFormManager)
				pDoc->m_pFormManager->EnumReportAlias(pRoot);

			if (arReports.GetSize() > ID_EXTDOC_DDTB_ENUMREPORT_MAX)   //show new dialog with list of all reports
			{
				CListDocumentReportsDlg dlgDocumentReports(pDoc, pRoot);
				dlgDocumentReports.DoModal();
			}
			else
			{
				int nID = ID_EXTDOC_DDTB_ENUMREPORT0;
				int nDefaultIdx = pDoc->m_pFormManager->GetIndexReportDefault();
				for (int i = 0; i < min(arReports.GetSize(), ID_EXTDOC_DDTB_ENUMREPORT_MAX); i++)
				{
					bShow = TRUE;
					menu.AppendMenu(i == nDefaultIdx && !bOverrideDefault ? MF_STRING | MF_CHECKED : MF_STRING, nID++, (LPTSTR)(LPCTSTR)arReports.GetAt(i));
				}
			}
		}

		SAFE_DELETE(pRoot);
	}

	else if (pnmtb->iItem == ID_EXTDOC_RADAR)
	{
		bShow = TRUE;
		CStringArray arRadars;
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();

		if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{

			if (pDoc->m_pFormManager)
				pDoc->m_pFormManager->EnumRadarAlias(arRadars);

			if (pDoc->CanDoRunWrmRadar())
				menu.AppendMenu(MF_STRING, ID_EXTDOC_NEWNRT, (LPTSTR)(LPCTSTR)_TB("Radar Report"));

			menu.AppendMenu(MF_STRING, pDoc->UseWoormRadar() ? ID_EXTDOC_ALTRADAR : ID_EXTDOC_RADAR, (LPTSTR)(LPCTSTR)_TB("Radar"));
			if (arRadars.GetSize())
				menu.AppendMenu(MF_SEPARATOR);

			int nID = ID_EXTDOC_DDTB_ENUMRADAR0;
			int nDefaultIdx = pDoc->m_pFormManager->GetIndexRadarDefault();
			for (int i = 0; i < min(arRadars.GetSize(), ID_EXTDOC_DDTB_ENUMRADAR_MAX); i++)
			{
				menu.AppendMenu(i == nDefaultIdx ? MF_STRING | MF_CHECKED : MF_STRING, nID++, (LPTSTR)(LPCTSTR)arRadars.GetAt(i));
			}
		}
	}

	else
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();
		if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
			bShow = ((CAbstractFormDoc*)pDoc)->DispatchToolbarDropDown(pnmtb->iItem, menu);
	}
}

// creo il men� popup per la gestione delle query.
// questo men� visualizza: la query tutt'ora in esecuzione, la query predefinita e il comando
// per modificare-editare le query
//-----------------------------------------------------------------------------
void CAbstractFormFrame::CreateQueryMenu(BOOL bShowQueryManager /*=FALSE*/)
{
	CDocument *pDoc = GetActiveDocument();
	if (!pDoc ||
		!pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) ||
		!(((CAbstractFormDoc*)pDoc)->m_pQueryManager)) return;

	if (!m_pIDQueryArray) m_pIDQueryArray = new CIdStringArray;
	m_pIDQueryArray->RemoveAll();

	CQueryMenu menu(((CAbstractFormDoc*)pDoc)->m_pQueryManager, m_pIDQueryArray, bShowQueryManager);


	if (m_pTabbedToolBar)
		m_pTabbedToolBar->UpdateDropdownMenu(ID_EXTDOC_EXEC_QUERY, &menu);
}

//-----------------------------------------------------------------------------
CString CAbstractFormFrame::GetQueryName(UINT nID)
{
	if (!m_pIDQueryArray) return _T("");

	return m_pIDQueryArray->GetStringByID(nID);
}

//-----------------------------------------------------------------------------
CAbstractFormView* CAbstractFormFrame::GetViewByCtrlID(UINT nIDC)
{
	POSITION pos = GetDocument()->GetFirstViewPosition();
	CView* pView = GetDocument()->GetNextView(pos);
	do
	{
		CAbstractFormView* pAbsView = dynamic_cast<CAbstractFormView*>(pView);
		if (pAbsView)
		{
			if (pAbsView->GetDlgCtrlID() == nIDC || pAbsView->GetDialogID() == nIDC)
				return pAbsView;

			CWnd* pWnd = pAbsView->GetTabber(nIDC);
			if (pWnd)
				return pAbsView;
		}

		pView = GetDocument()->GetNextView(pos);
	} while (pView != NULL);

	return NULL;
}

//-----------------------------------------------------------------------------
CPoint CAbstractFormFrame::GetPositionSwitchTo()
{
	CPoint point(0, 0);

	if (!m_bHasStatusBar || m_pStatusbarButtonSwitch == NULL)
		return point;

	CRect rect;
	m_pStatusbarButtonSwitch->GetWindowRect(rect);
	point.x = rect.left;
	point.y = rect.top;
	return point;
}


/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CAbstractFormFrame::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CAbstractFormFrame\n");
}

void CAbstractFormFrame::AssertValid() const
{
	CAbstractFrame::AssertValid();
}
#endif //_DEBUG

//==============================================================================
//	CDefferedCreateSlave
//==============================================================================
CCreateSlaveData::CCreateSlaveData(CRuntimeClass* pSlaveClass, CString& sTitle, HWND wndToClose/*NULL*/, BOOL bDestroyMe /*TRUE*/, BOOL bDisableDocument /*FALSE*/)
	:
	m_pClass(pSlaveClass),
	m_sTitle(sTitle),
	m_hwndToClose(wndToClose),
	m_bDestroyMe(bDestroyMe),
	m_bDisableDocument(bDisableDocument)
{
}

//==============================================================================
//	CMasterFrame
//==============================================================================

IMPLEMENT_DYNCREATE(CMasterFrame, CAbstractFormFrame)
BEGIN_MESSAGE_MAP(CMasterFrame, CAbstractFormFrame)
	//{{AFX_MSG_MAP(CMasterFrame)
	ON_WM_CLOSE()
	ON_COMMAND(ID_EXTDOC_EXIT, OnClose)

	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTW, 0, 0xFFFF, OnToolTipText)
	ON_NOTIFY_EX_RANGE(TTN_NEEDTEXTA, 0, 0xFFFF, OnToolTipText)

	ON_MESSAGE(UM_GET_DOC_NAMESPACE_ICON, OnGetNamespaceAndIcon)
	ON_MESSAGE(UM_GET_DOCUMENT_TITLE_INFO, OnGetDocumentTitleInfo)
	ON_MESSAGE(UM_IS_UNATTENDED_WINDOW, OnIsUnattendedWindow)
	ON_MESSAGE(UM_DEFERRED_CREATE_SLAVE, OnDeferredCreateSlaveView)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CMasterFrame::CMasterFrame()
	:
	CAbstractFormFrame(),
	m_pAdminIcon(NULL)
{
	SAFE_DELETE(m_pAdminIcon);
}

//-----------------------------------------------------------------------------
CMasterFrame::~CMasterFrame()
{
}

//-----------------------------------------------------------------------------
BOOL CMasterFrame::OnAddClientDocToolbarButtons()
{
	return ((CAbstractFormDoc*)GetDocument())->m_ToolBarButtons.CreateNewButtons(m_pTabbedToolBar);
}
// close all attached views, and also close document
//------------------------------------------------------------------------------
void CMasterFrame::OnClose()
{
	SetDestroying();
	PostMessage(WM_COMMAND, ID_FILE_CLOSE);

	CDocument* pDoc = GetActiveDocument();
	if (!pDoc) return;

	CString aNS = ((CBaseDocument*)pDoc)->GetNamespace().ToString();
	CFunctionDescription fd;
	if (
		AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder")) &&
		AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::RECORDING
		)
	{
		if (AfxGetTbCmdManager()->GetFunctionDescription(_NS_WEB("TestManager.TBMacroRecorder.TBMacroRecorder.RecordCloseDocument"), fd))
		{
			fd.SetParamValue(_T("nameSpace"), DataStr(aNS));
			AfxGetTbCmdManager()->RunFunction(&fd, 0);
		}
	}

}

//-----------------------------------------------------------------------------
BOOL CMasterFrame::OnToolTipText(UINT nui, NMHDR* lpnmhdr, LRESULT* pResult)
{
	return CAbstractFormFrame::OnToolTipText(nui, lpnmhdr, pResult);
}

//----------------------------------------------------------------------------
LRESULT CMasterFrame::OnIsUnattendedWindow(WPARAM wParam, LPARAM lParam)
{
	CDocument* pDoc = GetActiveDocument();
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
		return ((CBaseDocument*)pDoc)->IsInUnattendedMode();

	return FALSE;
}

//-----------------------------------------------------------------------------
LRESULT CMasterFrame::OnDeferredCreateSlaveView(WPARAM wParam, LPARAM lParam)
{
	CCreateSlaveData* pDeffSlave = lParam != NULL ? (CCreateSlaveData*)lParam : NULL;
	if (!pDeffSlave)
		return 0L;

	if (pDeffSlave->m_hwndToClose)
		::SendMessage(pDeffSlave->m_hwndToClose, WM_CLOSE, NULL, NULL);

	CAbstractFormDoc* pDoc = GetDocument()->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) ? (CAbstractFormDoc*)GetDocument() : NULL;
	if (pDoc)
		pDoc->CreateSlaveView(pDeffSlave->m_pClass, pDeffSlave->m_sTitle);

	// qui faccio l'if per evitare di fare -- a vuoto dei ref di disable
	if (pDeffSlave->m_bDisableDocument)
		pDoc->Disable(true);

	if (pDeffSlave && pDeffSlave->m_bDestroyMe)
		delete pDeffSlave;

	return 0L;
}

//-----------------------------------------------------------------------------
void CMasterFrame::OnAdjustFrameSize(CSize& size)
{
	CClientDC aDC(this);
	int nDefaultWidth = min(AfxGetThemeManager()->GetDefaultMasterFrameWidth(), aDC.GetDeviceCaps(HORZRES));
	int nDefaultHeight = min(AfxGetThemeManager()->GetDefaultMasterFrameHeight(), aDC.GetDeviceCaps(VERTRES));
	if (nDefaultWidth > 0)
		size.cx = max(size.cx, nDefaultWidth);
	if (nDefaultHeight > 0)
		size.cy = max(size.cy, nDefaultHeight);
}

//-----------------------------------------------------------------------------
void CMasterFrame::AdjustTabbedToolBar()
{
	if (!m_pTabbedToolBar)
	{
		__super::AdjustTabbedToolBar();
		return;
	}

	m_pTabbedToolBar->AdjustLayoutActiveTab();
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CMasterFrame::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CMasterFrame\n");
}

void CMasterFrame::AssertValid() const
{
	CAbstractFormFrame::AssertValid();
}
#endif //_DEBUG

//==============================================================================
//	CBatchFrame
//==============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CBatchFrame, CMasterFrame)
BEGIN_MESSAGE_MAP(CBatchFrame, CMasterFrame)
	//{{AFX_MSG_MAP(CBatchFrame)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CBatchFrame::OnCustomizeJsonToolBar()
{
	return CreateJsonToolbar(IDD_EXTDOC_BATCH_TOOLBAR);

}

//-----------------------------------------------------------------------------
void CBatchFrame::SwitchBatchRunButtonState()
{
	CDocument* pDoc = GetActiveDocument();
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

//==============================================================================
//	CSlaveFrame
//==============================================================================

IMPLEMENT_DYNCREATE(CSlaveFrame, CAbstractFormFrame)

BEGIN_MESSAGE_MAP(CSlaveFrame, CAbstractFormFrame)
	//{{AFX_MSG_MAP(CSlaveFrame)
	ON_WM_CLOSE()

	ON_COMMAND(ID_EXTDOC_GOTO_MASTER, OnGotoMaster)
	ON_COMMAND(ID_EXTDOC_EXIT, OnClose)
	ON_COMMAND(ID_EXTDOC_CUSTOMIZE, OnCustomize)
	ON_REGISTERED_MESSAGE(BCGM_CHANGEVISUALMANAGER, OnChangeVisualManager)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CSlaveFrame::CSlaveFrame()
{
	SetDockable(FALSE);
}

//-----------------------------------------------------------------------------
CSlaveFrame::~CSlaveFrame()
{}

//------------------------------------------------------------------------------
void CSlaveFrame::OnCustomize()
{
	if (GetDocument() && GetDocument()->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		((CAbstractFormDoc*)GetDocument())->PostMessage(WM_COMMAND, ID_EXTDOC_CUSTOMIZE, 0);
	}
}
//-----------------------------------------------------------------------------
BOOL CSlaveFrame::OnCustomizeJsonToolBar()
{
	return CreateJsonToolbar(IDD_EXTDOC_SLAVE_TOOLBAR);
}
//-----------------------------------------------------------------------------
BOOL CSlaveFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	return TRUE;

}

//-----------------------------------------------------------------------------
BOOL CSlaveFrame::CreateAccelerator()
{
	if (LoadAccelTable(MAKEINTRESOURCE(IDR_EXTDOC_SLAVE_ACCELERATOR)))
		return TRUE;

	TRACE("CSlaveFrame::CreateAccelerator() failed\n");
	ASSERT(FALSE);
	return FALSE;
}

// close only attached view, leaving document alive.
//
//------------------------------------------------------------------------------
void CSlaveFrame::OnClose()
{
	// get document assuming this frame is active frame and attached     
	// document is of CAbstractFormDoc (UAE otherwise)
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetActiveDocument();

	// if in edit mode, admit close olny if some control has changed data on it
	// and data is correct; otherwise alert user .
	if (pDoc && pDoc->GetNotValidView(TRUE))
		return;

	CAbstractFormFrame::OnClose();
}

//-----------------------------------------------------------------------------------
LRESULT CSlaveFrame::OnChangeVisualManager(WPARAM wParam, LPARAM lParam)
{
	return 0L;
}

//------------------------------------------------------------------------------
void CSlaveFrame::OnGotoMaster()
{
	if (AfxGetLoginContext()->GetDocked())
	{
		OnClose();
		return;
	}

	OnClose();
}

// Called by the framework before the creation of the Windows window attached to this CWnd object.
//-----------------------------------------------------------------------------
BOOL CSlaveFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	cs.hwndParent = GetValidOwner();
	return __super::PreCreateWindow(cs);
}
//-----------------------------------------------------------------------------
BOOL CSlaveFrame::Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, LPCTSTR lpszMenuName, DWORD dwExStyle, CCreateContext* pContext)
{
	return __super::Create(lpszClassName, lpszWindowName, dwStyle, rect, pParentWnd, NULL/*nessun menu!*/, dwExStyle, pContext);
}

//-----------------------------------------------------------------------------
void CSlaveFrame::OnAdjustFrameSize(CSize& size)
{
	TBThemeManager* pThemeManager = AfxGetThemeManager();

	CClientDC aDC(this);
	int nDefaultWidth = min(AfxGetThemeManager()->GetDefaultSlaveFrameWidth(), aDC.GetDeviceCaps(HORZRES));
	int nDefaultHeight = min(AfxGetThemeManager()->GetDefaultSlaveFrameHeight(), aDC.GetDeviceCaps(VERTRES));
	if (nDefaultWidth > 0)
		size.cx = max(size.cx, nDefaultWidth);
	if (nDefaultHeight > 0)
		size.cy = min(size.cy, nDefaultHeight);
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CSlaveFrame::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CSlaveFrame\n");
}

void CSlaveFrame::AssertValid() const
{
	CAbstractFormFrame::AssertValid();
}
#endif //_DEBUG

//==============================================================================
//	CRowFormFrame
//==============================================================================

IMPLEMENT_DYNCREATE(CRowFormFrame, CSlaveFrame)
BEGIN_MESSAGE_MAP(CRowFormFrame, CSlaveFrame)
	//{{AFX_MSG_MAP(CRowFormFrame)
	ON_WM_CLOSE()
	ON_WM_GETMINMAXINFO()

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
CRowFormFrame::CRowFormFrame()
{}

//-----------------------------------------------------------------------------
CRowFormFrame::~CRowFormFrame()
{}

//-----------------------------------------------------------------------------
void CRowFormFrame::OnClose()
{
	CRowFormView* pRowView = (CRowFormView*)GetActiveView();

	if (pRowView->GetDBT() && pRowView->GetDBT()->GetCurrentRowIdx() >= 0)
		CSlaveFrame::OnClose();
	else
	{
		// se il corrente record e` negativo deve chiudere la form
		// senza alcun controllo
		CAbstractFormFrame::OnClose();
	}
}

//-----------------------------------------------------------------------------
void CRowFormFrame::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	lpMMI->ptMinTrackSize.x = 600;
	lpMMI->ptMinTrackSize.y = 400;
}

//-----------------------------------------------------------------------------
BOOL CRowFormFrame::OnCustomizeJsonToolBar()
{
	return CreateJsonToolbar(IDD_EXTDOC_ROWFORM_TOOLBAR);
}
//-----------------------------------------------------------------------------	
BOOL CRowFormFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRowFormFrame::CreateAccelerator()
{
	if (LoadAccelTable(MAKEINTRESOURCE(IDR_EXTDOC_ROW_ACCELERATOR)))
		return TRUE;

	TRACE("CRowFormFrame::CreateAccelerator() failed\n");
	ASSERT(FALSE);
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CRowFormFrame::CreateStatusBar()
{
	if (!m_bHasStatusBar) return TRUE;
	ASSERT(m_pStatusBar == NULL);
	m_pStatusBar = new CStatusBarNoResizable();
	if (!m_pStatusBar->Create(this))
	{
		TRACE("CAbstractFormFrame::CreateStatusBar() failed\n");
		ASSERT(FALSE);
		return FALSE;
	}

	if (!m_pStatusBar->SetIndicators(anExtDocStatusBarButtons, sizeof(anExtDocStatusBarButtons) / sizeof(UINT)))
	{
		TRACE("CAbstractFormFrame::CreateStatusBar() failed\n");
		ASSERT(FALSE);
		return FALSE;
	}

	SetPane(m_pStatusBar, ID_EXTDOC_CONTROL_INDICATORS, 100, SBPS_STRETCH);

	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CRowFormFrame::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CRowFormFrame\n");
}

void CRowFormFrame::AssertValid() const
{
	CSlaveFrame::AssertValid();
}
#endif //_DEBUG


//==============================================================================
//	CRowFormSimpleFrame
//==============================================================================

IMPLEMENT_DYNCREATE(CRowFormSimpleFrame, CRowFormFrame)

//=============================================================================
//-----------------------------------------------------------------------------
BOOL CRowFormSimpleFrame::OnCustomizeJsonToolBar()
{
	return __super::OnCustomizeJsonToolBar();
}
//-----------------------------------------------------------------------------
BOOL CRowFormSimpleFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	ASSERT(pTabbedBar);

	__super::OnCustomizeTabbedToolBar(pTabbedBar);

	pTabbedBar->HideButton(ID_SEPARATOR);
	pTabbedBar->HideButton(ID_EXTDOC_NEW);
	pTabbedBar->HideButton(ID_EXTDOC_EDIT);
	pTabbedBar->HideButton(ID_EXTDOC_SAVE);
	//pTabbedBar->HideButton(ID_EXTDOC_FIRST);
	//pTabbedBar->HideButton(ID_EXTDOC_PREV);
	//pTabbedBar->HideButton(ID_EXTDOC_NEXT);
	//pTabbedBar->HideButton(ID_EXTDOC_LAST);
	pTabbedBar->HideButton(ID_EXTDOC_ESCAPE);
	pTabbedBar->HideButton(ID_EXTDOC_EXIT);
	//pTabbedBar->HideButton(ID_EXTDOC_HELP);
	pTabbedBar->HideButton(ID_SEPARATOR);
	return TRUE;
}

//=============================================================================

//==============================================================================
//	CRowFormEmbeddedSimpleFrame
//==============================================================================

IMPLEMENT_DYNCREATE(CRowFormEmbeddedSimpleFrame, CRowFormFrame)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRowFormEmbeddedSimpleFrame, CRowFormFrame)

	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CRowFormEmbeddedSimpleFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	BOOL bOk = __super::PreCreateWindow(cs);
	return
		cs.style = cs.style & ~(WS_BORDER | WS_DLGFRAME | WS_THICKFRAME);
	cs.dwExStyle = cs.dwExStyle & ~(WS_EX_STATICEDGE | WS_EX_CLIENTEDGE | WS_EX_WINDOWEDGE);

	return bOk;
}

//-----------------------------------------------------------------------------
LRESULT CRowFormEmbeddedSimpleFrame::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
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
	CWndObjDescription* pDesc = pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndObjDescription), strId);
	pDesc->UpdateAttributes(this);
	pDesc->m_Type = CWndObjDescription::GenericWndObj;

	pDesc->AddChildWindows(this);

	return (LRESULT)pDesc;
}

//-----------------------------------------------------------------------------
void CRowFormEmbeddedSimpleFrame::OnFrameCreated()
{

	//__super::OnFrameCreated();

	//La OnFrameCreated di DockableFrame assume che tutti i frame siano finestre di primo livello (non embeddate dentro altre), 
	//e quindi l'aggiunge ad un array specifico. In questo caso siccome il frame e' embedded, va rimosso da questo array

	//AfxGetThreadContext()->RemoveWindowRef(m_hWnd, FALSE);
}

//-----------------------------------------------------------------------------
BOOL CRowFormEmbeddedSimpleFrame::OnCustomizeJsonToolBar()
{
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL CRowFormEmbeddedSimpleFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	ASSERT(pTabbedBar);
	CTBToolBar* pToolBar = CreateEmptyToolBar(szToolbarNameMain, _TB("Main"));
	CTBToolBar* pToolBarTool = CreateEmptyToolBar(szToolbarNameTools, _TB("Tools"));

	//Main Tool Bar
	if (!AfxGetThemeManager()->ToolBarEditingButtonsFix())
	{
		pToolBar->AddButton(ID_EXTDOC_GOTO_MASTER, StdNamespace(_T("GotoMaster")), TBIcon(szIconBack, TOOLBAR), _TB("Main Window"), _TB("Return to the document's main window"));
		pToolBar->AddSeparator();
		pToolBar->AddButton(ID_EXTDOC_NEW, StdNamespace(_T("New")), TBIcon(szIconNew, TOOLBAR), _TB("New"), _TB("Create new document"));
		pToolBar->AddButton(ID_EXTDOC_EDIT, StdNamespace(_T("Edit")), TBIcon(szIconEdit, TOOLBAR), _TB("Edit"), _TB("Edit current document"));
		pToolBar->AddButton(ID_EXTDOC_SAVE, StdNamespace(_T("Save")), TBIcon(szIconSave, TOOLBAR), _TB("Save"), _TB("Save the document"));
	}
	else
	{
		pToolBar->AddButtonAllTab(ID_EXTDOC_NEW, StdNamespace(_T("New")), TBIcon(szIconNew, TOOLBAR), _TB("New"), _TB("Create new document"));
		pToolBar->AddButtonAllTab(ID_EXTDOC_EDIT, StdNamespace(_T("Edit")), TBIcon(szIconEdit, TOOLBAR), _TB("Edit"), _TB("Edit current document"));
		pToolBar->AddButtonAllTab(ID_EXTDOC_SAVE, StdNamespace(_T("Save")), TBIcon(szIconSave, TOOLBAR), _TB("Save"), _TB("Save the document"));
	}


	pToolBar->AddSeparator();
	pToolBar->AddButton(ID_EXTDOC_FIRST, StdNamespace(_T("First")), TBIcon(szIconFirst, TOOLBAR), _TB("First"), _TB("Go to first"));
	pToolBar->AddButton(ID_EXTDOC_PREV, StdNamespace(_T("Prev")), TBIcon(szIconPrev, TOOLBAR), _TB("Previous"), _TB("Go to previous"));
	pToolBar->AddButton(ID_EXTDOC_NEXT, StdNamespace(_T("Next")), TBIcon(szIconNext, TOOLBAR), _TB("Next"), _TB("Go to next"));
	pToolBar->AddButton(ID_EXTDOC_LAST, StdNamespace(_T("Last")), TBIcon(szIconLast, TOOLBAR), _TB("Last"), _TB("Go to last"));
	pToolBar->AddSeparator();
	pToolBar->AddButton(ID_EXTDOC_REFRESH_ROWSET, StdNamespace(_T("RefreshRowset")), TBIcon(szIconRefresh, TOOLBAR), _TB("Refresh data"), _TB("Refresh data from company database"));
	pToolBar->AddButton(ID_EXTDOC_INSERT_ROW, StdNamespace(_T("InsertRow")), TBIcon(szIconNew, TOOLBAR), _TB("Insert row"), _TB("Insert a new row"));

	if (!m_bHasStatusBar)
	{
		pToolBar->AddButtonToRight(ID_EXTDOC_SWITCHTO, StdNamespace(_T("SwitchTo")), TBIcon(szIconSwitchTo, TOOLBAR), _TB("Switch to..."), _TB("Switch to..."));
	}

	if (!AfxGetThemeManager()->ToolBarEditingButtonsFix())
	{
		pToolBar->AddButtonToRight(ID_EXTDOC_GOTO_MASTER, StdNamespace(_T("GotoMaster")), TBIcon(szIconBack, TOOLBAR), _TB("Main Window"), _TB("Return to the document's main window"));
	}

	pToolBarTool->AddButton(ID_EXTDOC_FIRST_ROW, StdNamespace(_T("FirstRow")), TBIcon(szIconTop, TOOLBAR), _TB("First row"), _TB("Go to first row"));
	pToolBarTool->AddButton(ID_EXTDOC_PREV_ROW, StdNamespace(_T("PrevRow")), TBIcon(szIconUp, TOOLBAR), _TB("Previous row"), _TB("Go to previous row"));
	pToolBarTool->AddButton(ID_EXTDOC_NEXT_ROW, StdNamespace(_T("NextRow")), TBIcon(szIconDown, TOOLBAR), _TB("Next row"), _TB("Go to next row"));
	pToolBarTool->AddButton(ID_EXTDOC_LAST_ROW, StdNamespace(_T("LastRow")), TBIcon(szIconBottom, TOOLBAR), _TB("Last row"), _TB("Go to last row"));
	pToolBarTool->AddButton(ID_EXTDOC_DELETE_ROW, StdNamespace(_T("DeleteRow")), TBIcon(szIconDelete, TOOLBAR), _TB("Delete row"), _TB("Delete current row"));
	pToolBarTool->AddButton(ID_EXTDOC_ESCAPE, StdNamespace(_T("Escape")), TBIcon(szIconEscape, TOOLBAR), _TB("Undo Changes"));	//TODO: immagine
	pToolBarTool->AddButtonToRight(ID_ACTIONS_COPY, StdNamespace(_T("ShareDocument")), TBIcon(szCopyDocumentLink, TOOLBAR), _TB("Share"), _TB("Share document link"));
	pToolBar->AddButtonToRightAllTab(ID_EXTDOC_EXIT, StdNamespace(_T("Exit")), TBIcon(szIconExit, TOOLBAR), _TB("Exit"));

	// ------------------------------------------------------------------------------------------
	pTabbedBar->AddTab(pToolBar, TRUE, TRUE);
	pTabbedBar->AddTab(pToolBarTool);
	// Active text below icons
	pTabbedBar->HideButton(ID_EXTDOC_GOTO_MASTER);
	pTabbedBar->HideButton(ID_EXTDOC_NEW);
	pTabbedBar->HideButton(ID_EXTDOC_EDIT);
	pTabbedBar->HideButton(ID_EXTDOC_SAVE);
	pTabbedBar->HideButton(ID_EXTDOC_REFRESH_ROWSET);
	//pTabbedBar->HideButton(ID_EXTDOC_FIRST);
	//pTabbedBar->HideButton(ID_EXTDOC_PREV);
	//pTabbedBar->HideButton(ID_EXTDOC_NEXT);
	//pTabbedBar->HideButton(ID_EXTDOC_LAST);
	pTabbedBar->HideButton(ID_EXTDOC_ESCAPE);
	pTabbedBar->HideButton(ID_EXTDOC_EXIT);

	if (!m_bHasStatusBar)
	{
		pToolBar->SetDropdown(ID_EXTDOC_SWITCHTO);
	}

	return TRUE;
}

//=============================================================================
//	CRowFormEmbeddedFrame
//==============================================================================

IMPLEMENT_DYNCREATE(CRowFormEmbeddedFrame, CRowFormEmbeddedSimpleFrame)

//=============================================================================

//==============================================================================
//	CSlaveFormEmbeddedFrame
//==============================================================================

IMPLEMENT_DYNCREATE(CSlaveFormEmbeddedFrame, CSlaveFrame)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSlaveFormEmbeddedFrame, CSlaveFrame)
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CSlaveFormEmbeddedFrame::CSlaveFormEmbeddedFrame()
{
	//m_bFixedSize = TRUE;
}

//-----------------------------------------------------------------------------
BOOL CSlaveFormEmbeddedFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	BOOL bOk = __super::PreCreateWindow(cs);

	cs.style = cs.style		& ~(WS_BORDER | WS_DLGFRAME | WS_THICKFRAME);
	cs.dwExStyle = cs.dwExStyle	& ~(WS_EX_STATICEDGE | WS_EX_CLIENTEDGE | WS_EX_WINDOWEDGE);

	return bOk;
}

//-----------------------------------------------------------------------------
LRESULT CSlaveFormEmbeddedFrame::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
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
	CWndObjDescription* pDesc = pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndObjDescription), strId);

	pDesc->UpdateAttributes(this);
	pDesc->m_Type = CWndObjDescription::GenericWndObj;

	pDesc->AddChildWindows(this);

	return (LRESULT)pDesc;
}

//-----------------------------------------------------------------------------
void CSlaveFormEmbeddedFrame::OnFrameCreated()
{
	//__super::OnFrameCreated();

	//La OnFrameCreated di DockableFrame assume che tutti i frame siano finestre di primo livello (non embeddate dentro altre), 
	//e quindi l'aggiunge ad un array specifico. In questo caso siccome il frame e' embedded, va rimosso da questo array

	//AfxGetThreadContext()->RemoveWindowRef(m_hWnd, FALSE);
}


//==============================================================================
//	CWizardFrame
//==============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWizardFrame, CMasterFrame)
BEGIN_MESSAGE_MAP(CWizardFrame, CMasterFrame)
	//{{AFX_MSG_MAP(CWizardFrame)
	ON_WM_ACTIVATE()
	ON_MESSAGE(UM_EXTDOC_BATCH_COMPLETED, OnBatchCompleted)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
CWizardFrame::CWizardFrame()
{
	SetDockable(FALSE);
	m_bHasToolbar = FALSE;
}

//-----------------------------------------------------------------------------
void CWizardFrame::OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	CMasterFrame::OnActivate(nState, pWndOther, bMinimized);

	OnActivateHandler(nState != WA_INACTIVE, pWndOther);
}

//----------------------------------------------------------------------------
LRESULT	CWizardFrame::OnBatchCompleted(WPARAM wParam, LPARAM lParam)
{
	CWizardFormView* pView = dynamic_cast<CWizardFormView*>(GetActiveView());

	if (pView)
		pView->SendMessage(UM_EXTDOC_BATCH_COMPLETED, wParam, lParam);

	return 0L;
}

//-----------------------------------------------------------------------------
void CWizardFrame::OnActivateHandler(BOOL bActivate, CWnd* pActivateWnd)
{
	if (!bActivate) // the frame is being deactivated
	{
		CView* pView = GetActiveView();
		//potrebbe non essere ancora stata creata, in qualche remoto caso si verifica
		if (!pView)
			return;

		if (pView && pView->GetDocument() && pView->GetDocument()->IsKindOf(RUNTIME_CLASS(CWizardFormDoc)))
		{
			CBaseDocument* pWaitingDoc = ((CWizardFormDoc*)pView->GetDocument())->m_pWaitingDoc;

			if (pWaitingDoc)
			{
				POSITION pos = pWaitingDoc->GetFirstViewPosition();
				while (pos != NULL)
				{
					CView* pView = pWaitingDoc->GetNextView(pos);
					CFrameWnd* pFrame = pView->GetParentFrame();
					if
						(
							pFrame &&
							::IsWindow(pFrame->m_hWnd) &&
							pFrame == pActivateWnd
							)
					{   // Si sta disattivando la frame del Wizard in favore della frame del
						// documento posto in attesa che, invece, non deve essere attivabile
						PostMessage(WM_ACTIVATE, (WPARAM)WA_ACTIVE, (LPARAM)m_hWnd);
						return;
					}
				}
			}
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CWizardFrame::Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, LPCTSTR lpszMenuName, DWORD dwExStyle, CCreateContext* pContext)
{
	return __super::Create(lpszClassName, lpszWindowName, dwStyle, rect, pParentWnd, NULL/*nessun menu!*/, dwExStyle, pContext);
}

//-----------------------------------------------------------------------------
BOOL CWizardFrame::CreateStatusBar()
{
	ASSERT(m_pStatusBar == NULL);
	return TRUE;
}


/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CWizardFrame::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CWizardFrame\n");
}

void CWizardFrame::AssertValid() const
{
	CMasterFrame::AssertValid();
}
#endif //_DEBUG

//==============================================================================
//	CWizardBatchFrame					CBatchFrame
//==============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWizardBatchFrame, CBatchFrame)
BEGIN_MESSAGE_MAP(CWizardBatchFrame, CBatchFrame)
	//{{AFX_MSG_MAP(CWizardBatchFrame)
	ON_MESSAGE(UM_EXTDOC_BATCH_COMPLETED, OnBatchCompleted)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CWizardBatchFrame::CWizardBatchFrame()
{
	SetDockable(TRUE);
}
//-----------------------------------------------------------------------------
BOOL CWizardBatchFrame::OnCustomizeJsonToolBar()
{
	return CreateJsonToolbar(IDD_EXTDOC_WIZARDBATCH_TOOLBAR);
}
//-----------------------------------------------------------------------------	
BOOL CWizardBatchFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	return TRUE;
}

//----------------------------------------------------------------------------
LRESULT	CWizardBatchFrame::OnBatchCompleted(WPARAM wParam, LPARAM lParam)
{
	CWizardFormView* pView = dynamic_cast<CWizardFormView*>(GetActiveView());

	if (pView)
		pView->SendMessage(UM_EXTDOC_BATCH_COMPLETED, wParam, lParam);

	return 0L;
}

//-----------------------------------------------------------------------------
BOOL CWizardBatchFrame::Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, LPCTSTR lpszMenuName, DWORD dwExStyle, CCreateContext* pContext)
{
	return __super::Create(lpszClassName, lpszWindowName, dwStyle, rect, pParentWnd, NULL/*nessun menu!*/, dwExStyle, pContext);
}


/////////////////////////////////////////////////////////////////////////////
//					class CWizardStepperBatchFrame Implementation
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CWizardStepperBatchFrame, CWizardBatchFrame)

//-----------------------------------------------------------------------------
CWizardStepperBatchFrame::CWizardStepperBatchFrame()
	:
	CWizardBatchFrame()
{
}

//-----------------------------------------------------------------------------
void CWizardStepperBatchFrame::OnCreateStepper()
{
	m_pStepper = new CFrameStepper();
	m_pStepper->CreateStepper(this, RUNTIME_CLASS(CStepperBreadCrumb));

	m_pStepper->GetStepper()->SetBreadCrumbFont(AfxGetThemeManager()->GetWizardStepperFont());
	m_pStepper->SetHeight(AfxGetThemeManager()->GetWizardStepperHeight());
}


//////////////////////////////////////////////////////////////////////////////
//		class CStepperBreadCrumb implementation								//
//////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CStepperBreadCrumb, CTaskBuilderBreadcrumb)

//-----------------------------------------------------------------------------
CStepperBreadCrumb::CStepperBreadCrumb()
	:
	CTaskBuilderBreadcrumb()
{
	SetBkgColor(AfxGetThemeManager()->GetWizardStepperBkgColor());
	SetForeColor(AfxGetThemeManager()->GetWizardStepperForeColor());
	//SetBkgColor(RGB(100, 149, 237));
	//SetForeColor(RGB(255, 255, 255));
}

//-----------------------------------------------------------------------------
BOOL CStepperBreadCrumb::OnInitCtrl()
{
	BOOL bResult = __super::OnInitCtrl();

	SetRoot(_T(""));

	return bResult;
}

//-----------------------------------------------------------------------------
void CStepperBreadCrumb::SetRoot(const CString& sRoot)
{
	RemoveAll();
	CTaskBuilderBreadcrumbItem* pRoot = GetRootItem();
	CString sRoot2 = CFrameStepper::StripPath(sRoot);
	pRoot->SetText(sRoot2);

	m_arItemsList.RemoveAll();

	UpdateCtrlView();
	Invalidate();
	UpdateWindow();
}

//-----------------------------------------------------------------------------
CTaskBuilderBreadcrumbItem* CStepperBreadCrumb::AddItem(const CString& sName, const CString& sText, CTaskBuilderBreadcrumbItem* pParent/* NULL*/)
{
	CString sText2 = CFrameStepper::StripPath(sText);
	if (sText2.IsEmpty())
		sText2 = _T(".");

	CString sName2 = sName;
	if (sName2.IsEmpty())
		sName2 = _T(".");

	CTaskBuilderBreadcrumbItem* pItem = __super::AddItem(sName2, sText2, pParent);
	m_arItemsList.Add(sText2);
	return pItem;
}

//-----------------------------------------------------------------------------
void CStepperBreadCrumb::UpdateBreadCrumb()
{
	CTaskBuilderBreadcrumbItem* pCurrent = GetRootItem();
	CString strPath = pCurrent->GetText();
	for (int i = 0; i < m_arItemsList.GetSize(); i++)
	{
		CString sTitle = m_arItemsList.GetAt(i);
		strPath = strPath + GetDelimiter() + sTitle;
	}

	m_pData->Assign(strPath);
	UpdateCtrlView();
	Invalidate();
	UpdateWindow();
}

//=============================================================================

IMPLEMENT_DYNCREATE(CBrowserFrame, CAbstractFormFrame)

