#include "stdafx.h"

#include <TbGenlib\PARSObj.H>
#include <TbGenlib\PARSEDT.H>
#include <TbGenlib\BaseTileManager.h>
#include <TbGenlib\TBPropertyGrid.h>
#include <TbGenlib\TBCommandInterface.h>
#include <TbGenlib\ParsBtn.h>
#include "TileDialog.h"
#include "TileManager.h"
#include "ExtDocView.h"
#include "HotLink.h"
#include "ExtDoc.h"
#include "ParsedPanel.h"
#include "BodyEdit.h"
#include "FormMng.h"
#include "JsonFormEngineEx.h"


#include "UITileDialog.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//							CEmptyTileDialog								//
//////////////////////////////////////////////////////////////////////////////


//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CEmptyTileDialog, CTileDialog)

//-----------------------------------------------------------------------------
CEmptyTileDialog::CEmptyTileDialog()
	:
	CTileDialog(_T(""), IDD_EMPTY_TILE)
{
}

//-----------------------------------------------------------------------------
CEmptyTileDialog::~CEmptyTileDialog()
{
}

//////////////////////////////////////////////////////////////////////////////
//							CTileDialog										//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTileDialog, CBaseTileDialog)

//-----------------------------------------------------------------------------
CTileDialog::CTileDialog()
	: CBaseTileDialog(_T(""), IDD_EMPTY_TAB)
{
	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CTileDialog::CTileDialog(const CString& sName, int nIDD, CWnd* pParent /*NULL*/)
	:
	CBaseTileDialog(sName, nIDD, pParent),
	m_pTabManager(NULL)
{
	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CTileDialog::~CTileDialog()
{
	SAFE_DELETE(m_pTabManager);

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//-----------------------------------------------------------------------------
HRESULT CTileDialog::get_accName(VARIANT varChild, BSTR *pszName)
{
	// TileDialog namespcae begins with "Form", use "TileDlg" instead to enhance disambiguation
	CString sNamespace = cwsprintf(_T("{0-%s}TileDlg"), GetNamespace().GetObjectName());
	*pszName = ::SysAllocString(sNamespace);
	return S_OK;
}

//-----------------------------------------------------------------------------
CExtButton*	CTileDialog::AddLink
(
	UINT			nIDC,
	const CString&	sName,
	SqlRecord*		pRecord,
	DataObj*		pDataObj
)
{
	if (sName.IsEmpty())
	{
		TRACE("CTabDialog::AddLink: the control idc=%d has empty name\n", nIDC);
		ASSERT(FALSE);
	}

	CRowFormView* pRowView = GetParentRowView();
	if (pRowView)
		pRowView->BuildMappedDataToCtrlLink
		(
			pRecord,
			pDataObj,
			m_DataToCtrlMap,
			m_pControlLinks->GetSize()
		);
	CExtButtonExtendedInfo* pExtInfo = new CExtButtonExtendedInfo(this, GetInfoOSL());
	CExtButton* pBtn = ::AddLink
	(
		sName,
		pExtInfo,
		m_pControlLinks,
		nIDC,
		pRecord,
		pDataObj
	);

	if (pBtn && pBtn->IsKindOf(RUNTIME_CLASS(CParsedButton)) && pDataObj)
	{
		CParsedButton* pCtrl = (CParsedButton*)pBtn;

		SetChildControlNamespace(sName, pCtrl);

		delete pExtInfo;
	}
	return pBtn;
}

//------------------------------------------------------------------------------
CParsedCtrl* CTileDialog::AddLink
(
	UINT			nIDC,
	const CString&	sName,
	SqlRecord*		pRecord,
	DataObj*		pDataObj,
	CRuntimeClass*	pParsedCtrlClass,
	CString			sNsHotKeyLink,
	UINT			nBtnID /*= BTN_DEFAULT*/
)
{
	CTBNamespace nsHKL(CTBNamespace::HOTLINK, sNsHotKeyLink);
	CRuntimeClass* pControlClass = NULL;
	HotKeyLink*		pHotKeyLink = (HotKeyLink*)AfxGetTbCmdManager()->RunHotlink(nsHKL, NULL, &pControlClass);

	CRuntimeClass* pPCClass = pParsedCtrlClass;
	if (pControlClass == NULL)
		pPCClass = pParsedCtrlClass;
	else if (pParsedCtrlClass == NULL)
		pPCClass = pControlClass;
	else if (pControlClass->IsDerivedFrom(pParsedCtrlClass))
		pPCClass = pControlClass;
	else
	{
		TRACE(_T("Incompatible runtime class for parsed control: hotlink has rtc named %s but AddLink has rtc named %s\n"), pControlClass->m_lpszClassName, pParsedCtrlClass->m_lpszClassName);
		//ASSERT(FALSE);
	}

	CParsedCtrl* pCtrl = this->AddLink(nIDC, sName, pRecord, pDataObj, pPCClass, pHotKeyLink, nBtnID);

	if (pCtrl && pHotKeyLink)
	{
		if (pHotKeyLink != pCtrl->GetHotLink())
			delete pHotKeyLink;
		else
			pCtrl->SetOwnHotKeyLink(TRUE);
	}
	return pCtrl;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CTileDialog::AddLink
(
	UINT			nIDC,
	const CString&	sName,
	SqlRecord*		pRecord,
	DataObj*		pDataObj,
	CRuntimeClass*	pParsedCtrlClass,
	HotKeyLink*		pHotKeyLink /*= NULL*/,
	UINT			nBtnID /*= BTN_DEFAULT*/
)
{
	if (sName.IsEmpty())
	{
		TRACE("CTabDialog::AddLink: the control idc=%d has empty name\n", nIDC);
		ASSERT(FALSE);
	}

	CRowFormView* pRowView = GetParentRowView();
	if (pRowView)
		pRowView->BuildMappedDataToCtrlLink
		(
			pRecord,
			pDataObj,
			m_DataToCtrlMap,
			m_pControlLinks->GetSize()
		);

	if (pParsedCtrlClass == RUNTIME_CLASS(CLabelStatic))
	{
		CParsedCtrl* pCtrl = AddLabelLink(nIDC);
		if (pCtrl && pDataObj)
			pCtrl->Attach(pDataObj);
		return pCtrl;
	}
	else
	{
		CParsedCtrl* pCtrl = ::AddLink
		(
			sName,
			this,
			m_pControlLinks,
			nIDC,
			pRecord,
			pDataObj,
			pParsedCtrlClass,
			pHotKeyLink,
			nBtnID
		);
		if (pCtrl)
			SetChildControlNamespace(sName, pCtrl);
		return pCtrl;
	}


}

//-----------------------------------------------------------------------------
CWnd* CTileDialog::AddLink
(
	UINT			nIDC,
	const CString&	sName,
	CRuntimeClass*  prtCtrl
)
{
	if (sName.IsEmpty())
	{
		TRACE("CTileDialog::AddLink: the control idc=%d has empty name\n", nIDC);
		ASSERT(FALSE);
	}

	CWnd* pCtrl = (CWnd*)prtCtrl->CreateObject();

	if (pCtrl && pCtrl->IsKindOf(RUNTIME_CLASS(CMultiSelectionListBox)))
	{
		CMultiSelectionListBox* pList = (CMultiSelectionListBox*)pCtrl;
		pList->SubclassDlgItem(nIDC, this);

		pList->GetInfoOSL()->m_pParent = GetInfoOSL();
		pList->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::CONTROL, sName, GetNamespace());
	}

	m_pControlLinks->Add(pCtrl);
	return pCtrl;
}

//-----------------------------------------------------------------------------
CParsedPanel* CTileDialog::AddLink
(
	UINT			nIDC,
	CRuntimeClass*	pParsedPanelClass,
	CObject*		pPanelOwner,
	const	CString&		sName,
	const	CString&		sCaption,
	BOOL			bCallOnInitialUpdate /*TRUE*/
)
{
	return ::AddLink
	(
		this,
		m_pControlLinks,
		nIDC,
		pParsedPanelClass,
		pPanelOwner,
		sName,
		sCaption,
		bCallOnInitialUpdate
	);
}

//-----------------------------------------------------------------------------
CBodyEdit* CTileDialog::AddLink
(
	UINT				nIDC,
	DBTSlaveBuffered*	pDBT,
	CRuntimeClass*		pBodyEditClass,
	CRuntimeClass*		pRowFormViewClass,
	CString				strRowFormViewTitle,
	CString				sName
)
{
	CBodyEdit* pBody = ::AddLink
	(
		this,
		this,
		m_pControlLinks,
		nIDC,
		pDBT,
		pBodyEditClass,
		pRowFormViewClass,
		strRowFormViewTitle,
		sName
	);

	Register(pBody);
	return pBody;
}

//-----------------------------------------------------------------------------
CTBGridControl*	CTileDialog::AddLinkGrid
(
	UINT				nIDC,
	DBTSlaveBuffered*	pDBT,
	CRuntimeClass*		pGridRuntimeClass/*	= NULL*/,
	CString				sGridName/*			= _T("")*/
)
{
	CTBGridControl* pGridControl =
		(pGridRuntimeClass == NULL) ?
		new CTBGridControl() :
		(CTBGridControl*)pGridRuntimeClass->CreateObject();

	pGridControl->SetName(sGridName);
	pGridControl->SetParentForm(this);

	// Create grid control:
	CRect rect;
	this->GetWindowRect(rect);
	this->ScreenToClient(rect);
	pGridControl->Create(WS_CHILD | WS_VISIBLE, rect, this, nIDC);
	pGridControl->SetFont(AfxGetThemeManager()->GetControlFont());

	pGridControl->SetDataSource(pDBT);

	m_pControlLinks->Add(pGridControl);
	return pGridControl;
}

//-----------------------------------------------------------------------------
CTBGridControl*	CTileDialog::AddLinkGrid
(
	UINT				nIDC,
	RecordArray*		pRA,
	CRuntimeClass*		pGridRuntimeClass/*	= NULL*/,
	CString				sGridName/*			= _T("")*/
)
{
	CTBGridControl* pGridControl =
		(pGridRuntimeClass == NULL) ?
		new CTBGridControl() :
		(CTBGridControl*)pGridRuntimeClass->CreateObject();

	pGridControl->SetName(sGridName);
	pGridControl->SetParentForm(this);

	// Create grid control:
	CRect rect;
	this->GetWindowRect(rect);
	this->ScreenToClient(rect);
	pGridControl->Create(WS_CHILD | WS_VISIBLE, rect, this, nIDC);
	pGridControl->SetFont(AfxGetThemeManager()->GetControlFont());

	pGridControl->SetDataSource(pRA);

	m_pControlLinks->Add(pGridControl);
	return pGridControl;
}

//-----------------------------------------------------------------------------
void CTileDialog::SetChildControlNamespace(const CString& sName, CParsedCtrl* pCtrl)
{
	if (pCtrl)
	{
		pCtrl->GetInfoOSL()->m_pParent = GetInfoOSL();
		pCtrl->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::CONTROL, sName, GetNamespace());

		CAbstractFormDoc* pDoc = GetDocument();
		if (
			pDoc &&
			(
			(pDoc->GetFormMode() == CBaseDocument::NEW && OSL_CAN_DO(pCtrl->GetInfoOSL(), OSL_GRANT_NEW) == 0) ||
				(pDoc->GetFormMode() == CBaseDocument::EDIT && OSL_CAN_DO(pCtrl->GetInfoOSL(), OSL_GRANT_EDIT) == 0)
				)
			)
			pCtrl->SetDataOSLReadOnly(TRUE);

		if (OSL_CAN_DO(pCtrl->GetInfoOSL(), OSL_GRANT_EXECUTE) == 0)
		{
			pCtrl->SetDataOSLReadOnly(TRUE);
			pCtrl->SetDataOSLHide(TRUE);
		}
	}
}


//-----------------------------------------------------------------------------
void CTileDialog::Register(CBodyEdit* pBody)
{
	//memorizzo una sola volta il namespace del BE nei children del DlgInfoItem relativo alla TabDialog corrente
	//la DlgInfoItem ha scope di documento mentre la TabDialog e le relative AddLink vengono chiamate ad ogni attivazione/istanziazione della TabDialog
	if (pBody)
	{//TODO dlgInfo
	/*	CString strNameBE = pBody->GetInfoOSL()->m_Namespace.ToString();
		if (m_pDlgInfo->m_strlistChildren.Find(strNameBE) == NULL)
			m_pDlgInfo->m_strlistChildren.AddHead(strNameBE);*/
	}
}

//-----------------------------------------------------------------------------
BOOL CTileDialog::PrepareAuxData()
{
	m_pControlLinks->OnPrepareAuxData();
	if (!OnPrepareAuxData())
		return FALSE;

	CAbstractFormDoc* pDoc = GetDocument();
	if (pDoc)
	{
		pDoc->OnPrepareAuxData(this);
		if (!pDoc->IsInUnattendedMode())
		{
			POSITION pos = pDoc->GetFirstViewPosition();
			while (pos)
			{
				CView* pView = pDoc->GetNextView(pos);
				if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
				{
					if (!((CAbstractFormView*)pView)->OnPrepareAuxData(this))
						return FALSE;
				}
			}
		}
		if (pDoc->m_pClientDocs)
			pDoc->m_pClientDocs->OnPrepareAuxData(this);
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CTileDialog::EnableTileDialogControls()
{
	CAbstractFormDoc* pDoc = GetDocument();
	if (!pDoc)
		return;
	if (!IsEnabled())
	{
		EnableTileDialogControlLinks(FALSE);
		return;
	}

	// normal processing in interctive mode
	switch (pDoc->GetFormMode())
	{
	case CBaseDocument::BROWSE:
		EnableTileDialogControlLinks(pDoc->IsABatchDocument());
		break;

	case CBaseDocument::NEW:
		EnableTileDialogControlLinks(TRUE);
		break;

	case CBaseDocument::EDIT:
		EnableTileDialogControlLinks(TRUE);
		break;

	case CBaseDocument::FIND:
		EnableTileDialogControlLinks(FALSE);
		break;
	}
}

//-----------------------------------------------------------------------------
void CTileDialog::EnableTileDialogControlLinks(BOOL bEnable /* = TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	CAbstractFormDoc* pDoc = GetDocument();
	if (!pDoc)
		return;

	switch (pDoc->GetFormMode())
	{
	case CBaseDocument::NEW:
		bMustSetOSLReadOnly = bMustSetOSLReadOnly;
		break;

	case CBaseDocument::EDIT:
		bMustSetOSLReadOnly = bMustSetOSLReadOnly;
		break;
	}

	::EnableControlLinks(m_pControlLinks, bEnable, bMustSetOSLReadOnly);
	EnableTabManagerControlLinks(bEnable);

	// normal processing in interctive mode
	switch (pDoc->GetFormMode())
	{
	case CBaseDocument::BROWSE:
		if (pDoc->IsABatchDocument())
			OnDisableControlsForBatch();
		break;

	case CBaseDocument::NEW:
		OnDisableControlsForAddNew();
		break;

	case CBaseDocument::EDIT:
		OnDisableControlsForEdit();
		break;

	case CBaseDocument::FIND:
		OnEnableControlsForFind();
		break;
	}

	OnDisableControlsAlways();
}
//------------------------------------------------------------------------------
void CTileDialog::OnUpdateTitle()
{
	CAbstractFormDoc* pDoc = GetDocument();
	if (pDoc)
	{
		pDoc->OnUpdateTitle(this);
		pDoc->m_pClientDocs->OnUpdateTitle(this);

		CView* pView = pDoc->GetFirstView();
		if (!pDoc->IsInUnattendedMode() && pView && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			((CAbstractFormView*)pView)->OnUpdateTitle(this);
		}
	}
}
//------------------------------------------------------------------------------
void CTileDialog::OnUpdateControls(BOOL bParentIsVisible)
{
	bParentIsVisible = IsDisplayed() && bParentIsVisible;
	__super::OnUpdateControls(bParentIsVisible);

	if (m_pTabManager)
	{
		CTabDialog* pTab = m_pTabManager->GetActiveDlg();
		if (pTab && pTab->m_hWnd)
			pTab->OnUpdateControls(bParentIsVisible);
	}
}
//------------------------------------------------------------------------------
void CTileDialog::OnFindHotLinks()
{
	::OnFindHotLinks(m_pControlLinks);

	if (m_pTabManager)
	{
		CTabDialog* pTab = m_pTabManager->GetActiveDlg();
		if (pTab && pTab->m_hWnd)
			pTab->OnFindHotLinks();
	}
}
//------------------------------------------------------------------------------
CParsedCtrl* CTileDialog::GetLinkedParsedCtrl(DataObj* pDataObj)
{
	return ::GetLinkedParsedCtrl(m_pControlLinks, pDataObj);
}

//------------------------------------------------------------------------------
CParsedCtrl* CTileDialog::GetLinkedParsedCtrl(const CTBNamespace& aNS)
{
	return ::GetLinkedParsedCtrl(m_pControlLinks, aNS);
}

//------------------------------------------------------------------------------
CParsedCtrl* CTileDialog::GetLinkedParsedCtrl(UINT nIDC)
{
	return ::GetLinkedParsedCtrl(m_pControlLinks, nIDC);
}

//------------------------------------------------------------------------------
void CTileDialog::OnResetDataObjs()
{
	::OnResetDataObjs(m_pControlLinks);
}

//-----------------------------------------------------------------------------
CBodyEdit* CTileDialog::GetBodyEdits(const CTBNamespace& aNS)
{
	return ::GetBodyEdits(m_pControlLinks, aNS);
}

//-----------------------------------------------------------------------------
CBodyEdit* CTileDialog::GetBodyEdits(int* pnStartIdx)
{
	return ::GetBodyEdits(m_pControlLinks, pnStartIdx);
}

//-----------------------------------------------------------------------------
int	CTileDialog::GetMinHeight(CRect& rect /*= CRect(0, 0, 0, 0)*/)
{

	switch (m_nMinHeight)
	{
	case ORIGINAL:
	case FREE:
	case AUTO:
	{
		break;
	}

	default:
		// Is present a BodyEdit in Tile ?
		int nMinHeight = -1;
		for (int j = 0; j <= m_pControlLinks->GetCount(); j++)
		{
			CBodyEdit* pBody = GetBodyEdits(&j);
			if (pBody)
			{
				if (IsCollapsed())
				{
					return m_nTitleHeight;
				}
				nMinHeight += m_nTitleHeight + pBody->GetMinHeight();
				break;
			}
		}

		if (nMinHeight > 0)
			return nMinHeight;
	}

	return __super::GetMinHeight(rect);
}

//------------------------------------------------------------------------------
BOOL CTileDialog::OnInitDialog()
{
	__super::OnInitDialog();

	// controllo chi e' il parent prossimo perche' fosse una tab dialog
	// la chiamata di PrepareAuxdata avverrebbe comunque dopo invocata
	// comunque dal tabber. Evito di farla eseguire due volte inutilmente
	if (!this)
		return FALSE;

	CParsedForm* pParsedForm = GetParentForm(this);
	CBaseTabDialog* pParentTabDialog = dynamic_cast<CBaseTabDialog*>(pParsedForm->GetFormCWnd());
	if (!pParentTabDialog)
		PrepareAuxData();

	// Abilita i controls sulla base dello stato del documento
	EnableTileDialogControls();

	// must return FALSE because the default focus is managed 
	// by the containing AbstractFormView
	return FALSE;
}
//------------------------------------------------------------------------------
void CTileDialog::BuildDataControlLinks()
{
}

//------------------------------------------------------------------------------
void CTileDialog::OnBuildDataControlLinks()
{
	CAbstractFormDoc* pDoc = GetDocument();
	// gives the ClientDocs the chanche to addlink their own controls
	if (pDoc)
		pDoc->OnBuildDataControlLinks(this);
}

//TODO unificare con quello di tabber.cpp
//------------------------------------------------------------------------------
BOOL IsCatched(const AFX_MSGMAP* pMsgMap, UINT nID, UINT nCode, const AFX_MSGMAP* pMsgMapRoot)
{
	if (pMsgMap == pMsgMapRoot)
		return FALSE;

	for (int e = 0; ; e++)
	{
		if (pMsgMap->lpEntries[e].nID == nID && pMsgMap->lpEntries[e].nCode == nCode)
			return TRUE;

		if (
			pMsgMap->lpEntries[e].nMessage == 0 && pMsgMap->lpEntries[e].nID == 0 &&
			pMsgMap->lpEntries[e].nCode == 0 && pMsgMap->lpEntries[e].nLastID == 0
			)
			break;
	}
	return ::IsCatched(pMsgMap->pfnGetBaseMap(), nID, nCode, pMsgMapRoot);
}

//------------------------------------------------------------------------------
BOOL CTileDialog::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	BOOL bIsHandled = ::IsCatched(this->GetMessageMap(), nID, nCode, CTileDialog::GetMessageMap());

	if (!bIsHandled)
		return __super::OnCommand(wParam, lParam);

	CAbstractFormDoc* pDoc = GetDocument();

	if (!pDoc || !pDoc->m_pClientDocs)
		return FALSE;

	BOOL bHandledBefore = FALSE;
	BOOL bHandledAfter = FALSE;

	for (int i = 0; i <= pDoc->m_pClientDocs->GetUpperBound(); i++)
	{
		if (pDoc->m_pClientDocs->GetAt(i)->GetMsgRoutingMode() == CClientDoc::CD_MSG_BOTH ||
			pDoc->m_pClientDocs->GetAt(i)->GetMsgRoutingMode() == CClientDoc::CD_MSG_BEFORE)
		{
			pDoc->m_pClientDocs->GetAt(i)->SetMsgState(CClientDoc::ON_BEFORE_MSG);
			bHandledBefore = pDoc->m_pClientDocs->GetAt(i)->OnCmdMsg(nID, nCode, NULL, NULL) || bHandledBefore;
		}
	}

	BOOL bTabHandled = CParsedDialog::OnCommand(wParam, lParam); //SKIP CBaseTabDialog to avoid double routing to Document 

	if (AfxGetThreadContext()->IsValidObject(pDoc)) 	//removed the ExistDocument check because of performance issues
		for (int i = 0; i <= pDoc->m_pClientDocs->GetUpperBound(); i++)
		{
			if (pDoc->m_pClientDocs->GetAt(i)->GetMsgRoutingMode() == CClientDoc::CD_MSG_BOTH ||
				pDoc->m_pClientDocs->GetAt(i)->GetMsgRoutingMode() == CClientDoc::CD_MSG_AFTER)
			{
				pDoc->m_pClientDocs->GetAt(i)->SetMsgState(CClientDoc::ON_AFTER_MSG);
				bHandledAfter = pDoc->m_pClientDocs->GetAt(i)->OnCmdMsg(nID, nCode, NULL, NULL) || bHandledAfter;
			}
		}

	return TRUE;
}

//------------------------------------------------------------------------------
CRowFormView* CTileDialog::GetParentRowView()
{
	return (CRowFormView*)GetFormView(RUNTIME_CLASS(CRowFormView));
}

//--------------------------------------------------------------------------
BOOL CTileDialog::PreTranslateMessage(MSG* pMsg)
{
	ASSERT(pMsg != NULL);
	ASSERT_VALID(this);

#ifndef _OLD_PTM

	if (GetDocument() && GetDocument()->m_bForwardingSysKeydownToChild)
		return m_pTabManager && m_pTabManager->PreTranslateMessage(pMsg);

#endif

	if (pMsg->message == WM_KEYDOWN)
	{
		// As the panel is a dialog, normal TAB navigation would loop inside the dialog 
		// instead of going on the other controls of the containing view.
		// If the next control to be focused is before the current (or after, if going backwards), it
		// means that the focus must go outside the panel
		if (pMsg->wParam == VK_TAB && (GetKeyState(VK_CONTROL) & 0x8000) == 0)
		{
			// normal -> forward, shift -> backward
			bool bBackward = (GetKeyState(VK_SHIFT) & 0x8000) == 0x8000;
			int nID = IDC_STATIC;
			int nNextID = IDC_STATIC;
			CWnd* pFocus = GetLastFocusedCtrl();
			CParsedCtrl* pCtrl = GetParsedCtrl(pFocus);
			if (pFocus && IsChild(pFocus))
			{
				CWnd* pParentWnd = pFocus->GetParent();
				if (dynamic_cast<CGridControlObj*>(pFocus) || dynamic_cast<CGridControlObj*>(pParentWnd))
					return __super::PreTranslateMessage(pMsg);

				nID = pFocus->GetDlgCtrlID();
				CWnd* pNext = GetNextDlgTabItem(pFocus, bBackward);
				if (pNext)
					nNextID = pNext->GetDlgCtrlID();
			}
			else
			{   //Check if focus is inside a Parsed Panel.
				CWnd* pWndFocus = GetFocus();
				CParsedPanel* pParsedPanelContainingFocus = GetParsedPanelContainer(pWndFocus);
				if (pParsedPanelContainingFocus)
				{
					CWnd* pNext = GetNextDlgTabItem(pParsedPanelContainingFocus, bBackward);

					CWnd* pFirstDlgItem = GetNextDlgTabItem(NULL);

					if (
						(!bBackward && pFirstDlgItem == pNext)
						||
						(bBackward && pFirstDlgItem == pParsedPanelContainingFocus->GetParent())
						)
					{
						// prima di spostare il fuoco da' una channce al control di gestire il VK_TAB (vedi per esempio le combo)
						if (pCtrl)
							pCtrl->PreProcessMessage(pMsg);

						//if next candidate to get focus inside the TileDialog, is the first one
						//we need to skip to the next TileDialog (both case forward and backward)
						return SetNextControlFocus(bBackward);
					}

					return __super::PreTranslateMessage(pMsg);
				}
			}

			// nID or nNextID == ID_STATIC -> control not found
			if (nID == IDC_STATIC || nNextID == IDC_STATIC || bBackward ? IsCtrlBefore(nID, nNextID) : IsCtrlBefore(nNextID, nID))
			{
				// prima di spostare il fuoco da' una channce al control di gestire il VK_TAB (vedi per esempio le combo)
				if (pCtrl)
					pCtrl->PreProcessMessage(pMsg);

				if (SetNextControlFocus(bBackward)) //control to focus in next/prev TileDialog
					return TRUE;
			}
		}
	}

#ifndef _OLD_PTM

	return CTaskBuilderTabWnd::PreProcessSysKeyMessage(pMsg, GetDocument(), this) || __super::PreTranslateMessage(pMsg);

#else

	return __super::PreTranslateMessage(pMsg);

#endif
}

//------------------------------------------------------------------------------
CParsedPanel* CTileDialog::GetParsedPanelContainer(CWnd* pWnd)
{
	if (!pWnd)
		return NULL;

	CWnd* pParent = pWnd->GetParent();
	while (pParent)
	{
		if (pParent->IsKindOf(RUNTIME_CLASS(CParsedPanel)))
			return (CParsedPanel*)pParent;

		pParent = pParent->GetParent();
	}

	return NULL;
}

//------------------------------------------------------------------------------
void CTileDialog::DoCollapseExpand()
{
	__super::DoCollapseExpand();
	GetParentTileGroup()->NotifyChildStateChanged(GetNamespace(), IsCollapsed());
}

//------------------------------------------------------------------------------
void CTileDialog::OnPinUnpin()
{
	CAbstractFormDoc* pDoc = GetDocument();
	if (pDoc)
		((CAbstractFormDoc*)pDoc)->DoPinUnpin(this);
}
//------------------------------------------------------------------------------
void CTileDialog::RebuildLinks(SqlRecord* pRec)
{
	CRowFormView* pParentView = GetParentRowView();
	if (!pParentView->IsKindOf(RUNTIME_CLASS(CRowFormView)))
	{
		TRACE("CTileDialog::RebuildLinks: the parent isn't a CRowFormView derived class\n");
		ASSERT(FALSE);
		return;
	}

	pParentView->RebuildMappedLinks(pRec, m_pControlLinks, m_DataToCtrlMap);

	//---- cambiano i dataobj e quindi perdo gli state-flag iniziali: se il B.E non e' protetto si abilita tutto
	CInfoOSL* pInfoOSL = GetInfoOSL();
	BOOL bMustSetOSLReadOnly = FALSE;
	if (GetDocument())
	{
		switch (GetDocument()->GetFormMode())
		{
		case CBaseDocument::NEW:
			bMustSetOSLReadOnly = (OSL_CAN_DO(pInfoOSL, OSL_GRANT_NEW) == 0);
			break;

		case CBaseDocument::EDIT:
			bMustSetOSLReadOnly = (OSL_CAN_DO(pInfoOSL, OSL_GRANT_EDIT) == 0);
			break;
		}
	}
	if (bMustSetOSLReadOnly)
		SetOSLReadOnlyOnControlLinks(m_pControlLinks, m_DataToCtrlMap);
}

//-----------------------------------------------------------------------------
CLabelStatic* CTileDialog::AddLabelLink(UINT nIDC)
{
	CLabelStatic* pLabel = ::AddLabelLink(this, nIDC);
	if (pLabel)
		m_pControlLinks->Add(pLabel);
	return pLabel;
}

//-----------------------------------------------------------------------------
CLabelStatic* CTileDialog::AddLabelLinkWithLine(UINT nIDC, int nSizePen /*= 1*/, int pos /*= CLabelStatic::LP_TOP*/)
{
	CLabelStatic* pLabel = ::AddLabelLinkWithLine(this, nIDC, GetTileStyle()->GetStaticWithLineLineForeColor(), nSizePen, pos);
	if (pLabel)
		m_pControlLinks->Add(pLabel);
	return pLabel;
}


//-----------------------------------------------------------------------------
CLabelStatic* CTileDialog::AddSeparatorLink(UINT nIDC, COLORREF crBorder, int nSizePen/* = 1*/, BOOL  bVertical/* = FALSE*/, CLabelStatic::ELinePos pos/* = CLabelStatic::LP_VCENTER*/)
{
	CLabelStatic* p = ::AddSeparatorLink(this, nIDC, crBorder, nSizePen, bVertical, pos);
	if (p)
		m_pControlLinks->Add(p);
	return p;
}

//-----------------------------------------------------------------------------
CGroupBoxBtn* CTileDialog::AddGroupBoxLink(UINT nIDC)
{
	CGroupBoxBtn* p = ::AddGroupBoxLink(this, nIDC);
	if (p)
		m_pControlLinks->Add(p);
	return p;
}

//-----------------------------------------------------------------------------
CAbstractFormView* CTileDialog::GetFormView(CRuntimeClass* pClass /*NULL*/)
{
	CBaseFormView* pView = GetBaseFormView(pClass);
	return pView && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)) ? (CAbstractFormView*)pView : NULL;
}

//-----------------------------------------------------------------------------
CTBPropertyGrid* CTileDialog::AddLinkPropertyGrid
(
	UINT				nIDC,
	CString				sName,
	CRuntimeClass*		pRuntimeClass /*NULL*/
)
{
	return ::AddLinkPropertyGrid(::GetParsedForm(this), this, m_pControlLinks, nIDC, sName, pRuntimeClass);
}

//-----------------------------------------------------------------------------
CParsedCtrl* CTileDialog::AddLinkAndCreateControl
(
	const CString&	sName,
	DWORD			dwStyle,
	const CRect&	rect,
	UINT			nIDC,
	SqlRecord*		pRecord,
	DataObj*		pDataObj,
	CRuntimeClass*	pParsedCtrlClass,
	HotKeyLink*		pHotKeyLink			/*= NULL*/,
	UINT			nBtnID				/*= BTN_DEFAULT*/
)
{
	CParsedCtrl* pCtrl = ::AddLinkAndCreateControl
	(
		sName,
		dwStyle,
		rect,
		this, //parent
		this->GetControlLinks(),
		nIDC,
		pRecord,
		pDataObj,
		pParsedCtrlClass,
		pHotKeyLink,
		FALSE,
		nBtnID
	);

	SetChildControlNamespace(sName, pCtrl);
	return pCtrl;
}

//-----------------------------------------------------------------------------
CBodyEdit* CTileDialog::AddLinkAndCreateBodyEdit
(
	CRect				rect,
	UINT				nIDC,
	DBTSlaveBuffered*	pDBT,
	CRuntimeClass*		pBodyEditClass,
	CRuntimeClass*		pRowFormViewClass /*= NULL*/,
	CString				strRowFormViewTitle/* = _T("")*/,
	CString				sBodyName  /*= _T("")*/,
	CString				sRowViewName/*  = _T("")*/
)
{
	CBodyEdit* pBody = ::AddLinkAndCreateBodyEdit
	(
		rect,
		this,
		this,
		this->GetControlLinks(),
		nIDC,
		pDBT,
		pBodyEditClass,
		pRowFormViewClass,
		strRowFormViewTitle,
		sBodyName,
		sRowViewName
	);

	return pBody;
}

//-----------------------------------------------------------------------------
CTabManager* CTileDialog::AddTabManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate /*TRUE*/)
{
	m_pTabManager = (CTabManager*) __super::AddBaseTabManager(nIDC, pClass, sName, bCallOnInitialUpdate);
	if (m_pTabManager)
	{
		m_pTabManager->SetParentElement(this);
		if (m_pLayoutContainer)
			m_pLayoutContainer->AddChildElement(m_pTabManager);
	}

	return m_pTabManager;
}

//-----------------------------------------------------------------------------
CTileManager* CTileDialog::AddTileManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate /*TRUE*/)
{
	if (!pClass->IsDerivedFrom(RUNTIME_CLASS(CTileManager)))
	{
		ASSERT_TRACE1(FALSE, "Runtime class parameter %s must be a CTabDialog!\n", (LPCTSTR)CString(pClass->m_lpszClassName));
		return NULL;
	}

	return (CTileManager*)AddTabManager(nIDC, pClass, sName, bCallOnInitialUpdate);
}

//-----------------------------------------------------------------------------
void CTileDialog::Relayout(CRect &rectNew, HDWP hDWP)
{
	__super::Relayout(rectNew, hDWP);
	if (m_pTabManager)
	{
		// devo ricalcolarmi bene di quanto relayout-are il tab manager interno
		// partendo dalle coordinate a cui e' posizionato rispetto al suo client
		CRect aTileRect, aTabRect;
		GetClientRect(aTileRect);
		m_pTabManager->GetWindowRect(aTabRect);
		ScreenToClient(aTabRect);

		m_pTabManager->Relayout(CRect(aTabRect.left, aTabRect.top, aTileRect.Width(), aTileRect.Height()), hDWP);
	}
}

//-----------------------------------------------------------------------------
void CTileDialog::EnableTabManagerControlLinks(BOOL bEnable)
{
	if (!m_pTabManager)
		return;

	CTabDialog* pTab = m_pTabManager->GetActiveDlg();
	if (pTab && pTab->m_hWnd)
		pTab->EnableTabDialogControlLinks(bEnable);
}

/////////////////////////////////////////////////////////////////////////////
//			Class CBatchHeaderTileDlg Implementation
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CBatchHeaderTileDlg, CTileDialog)

//-----------------------------------------------------------------------------
CBatchHeaderTileDlg::CBatchHeaderTileDlg()
	:
	CTileDialog(_NS_TILEDLG("Titles"), IDD_TD_BATCH_DOC_HEADER),
	m_nIDCTitle(IDC_BATCH_DOC_HEADER_TITLE),
	m_nIDCSubTitle(IDC_BATCH_DOC_HEADER_SUBTITLE),
	m_pTitle(NULL),
	m_pSubTitle(NULL)
{
	SetMaxWidth(FREE);

	SetTileStyle(AfxGetTileDialogStyleHeader());
	SetResourceModule(GetDllInstance(RUNTIME_CLASS(CBatchHeaderTileDlg)));
}

//-----------------------------------------------------------------------------
CBatchHeaderTileDlg::CBatchHeaderTileDlg(const CString& sTitle, UINT nIDD, UINT nIDCTitle /*0*/, UINT nIDCSubTitle /*0*/)
	:
	CTileDialog(sTitle, nIDD),
	m_nIDCTitle(nIDCTitle),
	m_nIDCSubTitle(nIDCSubTitle),
	m_pTitle(NULL),
	m_pSubTitle(NULL)
{
	if (!m_nIDCTitle && !m_nIDCSubTitle)
		ASSERT_TRACE(FALSE, "CBatchHeaderTileDlg without title and subtitle!");

	SetMaxWidth(FREE);

	SetTileStyle(AfxGetTileDialogStyleHeader());
}

//-----------------------------------------------------------------------------
void CBatchHeaderTileDlg::SetTextTitle(DataStr aStrTitle)
{
	if (m_pTitle)
		m_pTitle->SetWindowTextW(aStrTitle.FormatData());
}

//-----------------------------------------------------------------------------
void CBatchHeaderTileDlg::SetTextSubTitle(DataStr aStrSubTitle, BOOL bBoldTitle)
{
	if (m_pSubTitle)
	{
		m_pSubTitle->SetWindowTextW(aStrSubTitle.FormatData());
		// imposta font a grassetto sul titolo se il sottotitolo non è vuoto
		if (m_pTitle && bBoldTitle)
		{
			m_pTitle->SetOwnFont(!aStrSubTitle.IsEmpty(), FALSE, FALSE, 12);
			m_pTitle->SetCustomDraw(FALSE);
		}
	}
}

//-----------------------------------------------------------------------------
void CBatchHeaderTileDlg::BuildDataControlLinks()
{
	SetFlex(0);
	TileStyle* pStyle = GetTileStyle();
	if (m_nIDCTitle)
	{
		m_pTitle = AddLabelLink(m_nIDCTitle);
		// imposta font normale sul titolo. Sarà la SetSubTitle ad impostarlo a grassetto se necessario
		// in modo che in caso ci sia solo il titolo, esso sarà con font normale
		m_pTitle->SetOwnFont(FALSE, FALSE, FALSE, 12);
		m_pTitle->SetCustomDraw(FALSE);
		m_pTitle->SetTextColor(pStyle->GetTitleForeColor());
	}

	if (m_nIDCSubTitle)
	{
		m_pSubTitle = AddLabelLink(m_nIDCSubTitle);
		m_pSubTitle->SetOwnFont(FALSE, FALSE, FALSE, 12);
		m_pSubTitle->SetCustomDraw(FALSE);
		m_pSubTitle->SetTextColor(pStyle->GetTitleForeColor());
	}
}

/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CServicesHeaderTileDlg, CTileDialog)

//-----------------------------------------------------------------------------
CServicesHeaderTileDlg::CServicesHeaderTileDlg()
	:
	CTileDialog(_NS_TILEDLG("Header"), IDD_TD_SERVICES_HEADER)
{
	SetTileStyle(AfxGetTileDialogStyleHeader());
	SetResourceModule(GetDllInstance(RUNTIME_CLASS(CServicesHeaderTileDlg)));
}
//-----------------------------------------------------------------------------
void CServicesHeaderTileDlg::BuildDataControlLinks()
{
}