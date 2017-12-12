#include "stdafx.h"

#include <TbGeneric\VisualStylesXP.h>

#include "extdoc.h"
#include "ExtDocView.h"
#include "HotLink.h"
#include <TbGeneric\TBThemeManager.h>
#include <TbGeneric\JsonFormEngine.h>


#include "ParsedPanel.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//							CPanelContainer									//
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CPanelContainer : public CWnd
{
	DECLARE_DYNAMIC (CPanelContainer)

public:
	CPanelContainer(CParsedPanel* pOwner);

private:
	CParsedPanel* m_pOwner;

protected:
	afx_msg	void 	OnSetFocus			(CWnd*);

	DECLARE_MESSAGE_MAP()
};

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CPanelContainer, CWnd)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPanelContainer, CWnd)
	ON_WM_SETFOCUS					()
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPanelContainer::CPanelContainer(CParsedPanel* pOwner)
	:
	m_pOwner(pOwner)
{
}

//-----------------------------------------------------------------------------
void CPanelContainer::OnSetFocus(CWnd* pOldCWnd)
{
	CWnd* pFirst = m_pOwner->GetNextDlgTabItem(NULL);
	if (pFirst)
		pFirst->SetFocus();
	else
		__super::OnSetFocus(pOldCWnd);
}


//////////////////////////////////////////////////////////////////////////////
//							CParsedPanel									//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CParsedPanel, CParsedDialog)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CParsedPanel, CParsedDialog)
	//{{AFX_MSG_MAP(CParsedPanel)
	ON_WM_CTLCOLOR	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CParsedPanel::CParsedPanel (const CString& sName, UINT nIDD/*=0*/)
	:
	CParsedDialog	(nIDD),
	m_colorDlg		(AfxGetThemeManager()->GetBackgroundColor()),
	m_pContainer	(NULL),
	m_bFitHeight	(TRUE),
	m_bFitWidth		(TRUE)
{
	m_sName	= sName;

	CString strName = sName;
	strName.Trim();
	if (strName.IsEmpty())
		strName = GetRuntimeClass()->m_lpszClassName;
	m_sName = strName;

	m_bInOpenDlgCounter = FALSE;
}

//------------------------------------------------------------------------------
CParsedPanel::~CParsedPanel ()
{
}

//-----------------------------------------------------------------------------
BOOL CParsedPanel::Create (UINT nIDC, LPCTSTR lpszCaption, CWnd* pParentWnd)
{
	if (m_nID == 0)
	{
		ASSERT(FALSE);
		TRACE0("IDD in CParsedPanel. Create method failed, tab m_nID is not initialized\n");
		return FALSE;
    }

	m_brushDlg.CreateSolidBrush(m_colorDlg);

	CWnd* pPlaceHolderWnd = pParentWnd->GetDlgItem(nIDC);
	if (!pPlaceHolderWnd)
	{
		ASSERT(FALSE);
		TRACE1 ("CParsedPanel::Create cannot find control ID %d into the form", nIDC);
		return FALSE;
	}

	m_Caption = lpszCaption;
    
	// OSL info set
	// continua in OnInitDialog
	CTBNamespace aNs(CTBNamespace::CONTROL, m_sName);
	if (aNs.IsValid())
		m_sName = aNs.GetObjectName();

	GetInfoOSL()->SetType(OSLType_Tabber);
	if (dynamic_cast<CParsedForm*>(pParentWnd))
	{
		IOSLObjectManager* pInfoOSL = dynamic_cast<IOSLObjectManager*>(pParentWnd);
		if (pInfoOSL)
			GetInfoOSL()->m_pParent = pInfoOSL->GetInfoOSL();
	}
	//

	if (!CParsedDialog::Create(m_nID, pPlaceHolderWnd, m_sName))
		return FALSE;
	
	m_pParentWnd = pParentWnd;

	CRect rectBtn;
	pPlaceHolderWnd->GetWindowRect(&rectBtn);
	pParentWnd->ScreenToClient(rectBtn);

	if (!m_bFitHeight || !m_bFitWidth)
	{
		CRect rectDlg;
		GetWindowRect(&rectDlg);
		pParentWnd->ScreenToClient(rectDlg);

		pPlaceHolderWnd->SetWindowPos(NULL, 0, 0, m_bFitWidth ? rectBtn.Width() : rectDlg.Width(), m_bFitHeight ? rectBtn.Height() : rectDlg.Height(), SWP_NOMOVE | SWP_NOZORDER);
	
		// update the rect to the changed size
		pPlaceHolderWnd->GetWindowRect(&rectBtn);
		pParentWnd->ScreenToClient(rectBtn);
	}

	SetWindowPos(NULL, 0, 0, rectBtn.Width(), rectBtn.Height(), SWP_NOMOVE | SWP_NOZORDER);

	DWORD dwRemoveForDockStyle = WS_THICKFRAME | WS_CAPTION | WS_POPUP;
	DWORD dwAddForDockStyle = WS_CHILD;
	ModifyStyle(dwRemoveForDockStyle, dwAddForDockStyle);

	m_pContainer = new CPanelContainer(this);
	m_pContainer->SubclassDlgItem(nIDC, pParentWnd);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedPanel::ShowCaption(UINT nIDC)
{
	if (m_Caption.IsEmpty())
		return;

	CWnd* pWnd = GetDlgItem(nIDC);
	if (!pWnd)
		return;
	
	pWnd->SetWindowTextW(m_Caption);
}

//-----------------------------------------------------------------------------
HBRUSH CParsedPanel::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	//Altrimenti i control disabilitati e gli static non ricevono ON_WM_CTLCOLOR_REFLECT
	//necessario per la colorazione custom 
	LRESULT lResult;
	if (pWnd->SendChildNotifyLastMsg(&lResult))
		return (HBRUSH)lResult;     // catched: eat it
	//----

	if( (nCtlColor == CTLCOLOR_DLG) || (nCtlColor == CTLCOLOR_STATIC) )
	{
		pDC->SetBkColor(m_colorDlg);
		return (HBRUSH)m_brushDlg.GetSafeHandle();
	}
	
	return CParsedDialog::OnCtlColor(pDC, pWnd, nCtlColor);
}

//------------------------------------------------------------------------------
void CParsedPanel::PostNcDestroy()
{
	ASSERT(m_hWnd == NULL);
	m_pContainer->UnsubclassWindow();
	SAFE_DELETE(m_pContainer);
}

//------------------------------------------------------------------------------
BOOL CParsedPanel::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if (nCode == EN_VALUE_CHANGED && m_pControlLinks->GetWndLinkedCtrl(nID)) //@@@TODO GetWndLinkedCtrl non va in ricorsione su control contenuti come b.e. e tabber
	{
		// simula l'invio di un changed come se arrivasse dal bottone placeholder del parsedpanel
		int nIDC = m_pContainer->GetDlgCtrlID();
		CWnd* pParent = m_pContainer->GetParent();
		pParent->POST_WM_COMMAND(nIDC, nCode, m_pContainer->m_hWnd);
	}

	return __super::OnCommand(wParam, lParam);
}

//--------------------------------------------------------------------------
BOOL CParsedPanel::IsCtrlBefore(int nBefore, int nAfter)
{
	if (nBefore == nAfter)
		return TRUE;

	CWnd* pCtrl = GetWindow(GW_CHILD);
	while (pCtrl) 
	{
		int nID = pCtrl->GetDlgCtrlID();
		// actually, the TABSTOP style cannot be checked on all the radiobuttons of a group
		// The Microsoft documentation suggests that "If the group contains radio buttons, the application 
		// should apply the WS_TABSTOP style only to the first control in the group"
		// Apparently, this style is removed from the radiobuttons after the first one of a group
		if	(
				nID != IDC_STATIC &&
				(
					(pCtrl->GetStyle() & WS_TABSTOP) == WS_TABSTOP ||
					(pCtrl->GetStyle() & BS_AUTORADIOBUTTON) == BS_AUTORADIOBUTTON
				)
			)
		{
			if (nID == nBefore)
				return TRUE;
			if (nID  == nAfter)
				return FALSE;
		}

		pCtrl = pCtrl->GetNextWindow();
	}

	return FALSE;
}

//--------------------------------------------------------------------------
BOOL CParsedPanel::PreTranslateMessage(MSG* pMsg)
{
	ASSERT(pMsg != NULL);
	ASSERT_VALID(this);

	if (pMsg->message == WM_KEYDOWN)
	{
		// As the panel is a dialog, normal TAB navigation would loop inside the dialog 
		// instead of going on the other controls of the containing view.
		// If the next control to be focused is before the current (or after, if going backwards), it
		// means that the focus must go outside the panel
		if (pMsg->wParam == VK_TAB && (GetKeyState(VK_CONTROL) & 0x8000) == 0)
		{
			// normal -> forward, shift -> backward
			BOOL bBackward = GetKeyState(VK_SHIFT) & 0x8000;
			int nID = IDC_STATIC;
			int nNextID = IDC_STATIC;
			CWnd* pFocus = GetLastFocusedCtrl();
			if (pFocus)
			{
				nID = pFocus->GetDlgCtrlID();
				CWnd* pNext = GetNextDlgTabItem(pFocus, bBackward);
				if (pNext)
					nNextID = pNext->GetDlgCtrlID();
			}

			// nID or nNextID == ID_STATIC -> control not found
			if (nID == IDC_STATIC || nNextID == IDC_STATIC || bBackward ? IsCtrlBefore(nID, nNextID) : IsCtrlBefore(nNextID, nID))
				return FALSE; // returning false will let the TAB key to be managed by the parent window, that is the PanelContainer
		}
	}

	return __super::PreTranslateMessage(pMsg);
}

//----------------------------------------------------------------------------
BOOL CParsedPanel::OnInitDialog()
{
	__super::OnInitDialog();
	
	CInfoOSL* pInfoOSL = GetInfoOSL();
	pInfoOSL->m_Namespace.SetChildNamespace(CTBNamespace::CONTROL, m_sName , (pInfoOSL->m_pParent)->m_Namespace);

	// Allow the dialog customization by "linking" (subclassing) some controls
	BuildDataControlLinks();

	PrepareAuxData();

	if (GetDocument()->GetType() == VMT_BATCH)
		BatchEnableControls();
	else
		EnableControls();

	OnUpdateControls();

	// must return FALSE because the default focus is managed 
	// by the containing AbstractFormView
	return FALSE;
}

//-----------------------------------------------------------------------------
void CParsedPanel::EnableControlLinks (BOOL bEnable /* = TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	//CInfoOSL* pInfoOSL = m_pDlgInfo->GetInfoOSL();
	//switch (GetDocument()->GetFormMode())
	//{
	//		case CBaseDocument::NEW:
	//			bMustSetOSLReadOnly = bMustSetOSLReadOnly || (OSL_CAN_DO( pInfoOSL, OSL_GRANT_NEW) == 0);
	//			break;

	//		case CBaseDocument::EDIT:
	//			bMustSetOSLReadOnly = bMustSetOSLReadOnly || (OSL_CAN_DO( pInfoOSL, OSL_GRANT_EDIT) == 0);
	//			break;
	//}

	::EnableControlLinks (m_pControlLinks, bEnable, bMustSetOSLReadOnly);

	if (GetDocument()->GetType() == VMT_BATCH || GetDocument()->GetType() == VMT_FINDER)
	{
		OnDisableControlsForBatch();
		OnDisableControlsAlways();
	}
	else switch (GetDocument()->GetFormMode())
	{
			case CBaseDocument::NEW:
				OnDisableControlsForAddNew();
				OnDisableControlsAlways();
				break;

			case CBaseDocument::EDIT:
				OnDisableControlsForEdit();
				OnDisableControlsAlways();
				break;

			case CBaseDocument::FIND:
				OnEnableControlsForFind();
				break;
	}

	//if (GetDocument()->GetFormMode() != CBaseDocument::BROWSE)
	//	GetDocument()->m_pClientDocs->OnDisableControlsAlways(this);

}
		
//-----------------------------------------------------------------------------
void CParsedPanel::BatchEnableControls()
{
	if (GetDocument()->GetType() != VMT_BATCH)
		return;
		
	EnableControlLinks (!GetDocument()->m_bBatchRunning);
	if (!GetDocument()->m_bBatchRunning)
		OnDisableControlsForBatch();

}

//----------------------------------------------------------------------------
void CParsedPanel::EnableControls()
{
	if (GetDocument()->GetType() == VMT_BATCH)
		return;
		
	// normal processing in interctive mode
	switch (GetDocument()->GetFormMode())
	{
		case CBaseDocument::BROWSE:
			EnableControlLinks	(FALSE);
			break;
			
		case CBaseDocument::NEW:
			EnableControlLinks	(TRUE);
			OnDisableControlsForAddNew();
			break;

		case CBaseDocument::EDIT:
			EnableControlLinks	(TRUE);
			OnDisableControlsForEdit();
			break;

		case CBaseDocument::FIND:
			EnableControlLinks	(FALSE);
			OnEnableControlsForFind();
			break;			
	}

	//if (GetDocument()->GetFormMode() != CBaseDocument::BROWSE)
	//	GetDocument()->m_pClientDocs->OnDisableControlsAlways(this);
}

//-----------------------------------------------------------------------------
BOOL CParsedPanel::PrepareAuxData()
{
	OnPrepareAuxData();

	return TRUE;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CParsedPanel::AddLink
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
		TRACE("CParsedPanel::AddLink: the control idc=%d has empty name\n", nIDC);
		ASSERT(FALSE);
	}

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

	SetChildControlNamespace(sName, pCtrl);
	return pCtrl;
}

//-----------------------------------------------------------------------------
CExtButton*	CParsedPanel::AddLink
		(
					UINT		nIDC, 
			const	CString&	sName,
					SqlRecord*	pRecord,
					DataObj*	pDataObj
		)
{
	if (sName.IsEmpty())
	{
		TRACE("CParsedPanel::AddLink: the control idc=%d has empty name\n", nIDC);
		ASSERT(FALSE);
	}

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
		CParsedButton* pCtrl = (CParsedButton*) pBtn;

		SetChildControlNamespace(sName, pCtrl);

		delete pExtInfo;
	}
	return pBtn;
}
							
//-----------------------------------------------------------------------------
CParsedPanel* CParsedPanel::AddLink
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


