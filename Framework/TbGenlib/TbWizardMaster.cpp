#include "stdafx.h" 

#include <TbGeneric\VisualStylesXP.h>
#include <TbGeneric\TBThemeManager.h>


#include "baseapp.h"
#include "tbwizardmaster.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//---------------------------------------------------------------------------
// CWizardPage dialog
//---------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CWizardPage, CParsedDialog)
//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CWizardPage, CParsedDialog)
	//{{AFX_MSG_MAP(CWizardPage)
	ON_WM_CTLCOLOR()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//---------------------------------------------------------------------------
CWizardPage::CWizardPage(UINT nIDTemplate, CWnd* pParent)
   :
	CParsedDialog	(nIDTemplate,pParent),
	m_bCreated		(FALSE),
	m_bActive		(FALSE),
	m_nDialogID		(nIDTemplate),
	m_pParent		(NULL) 
{
	m_bInOpenDlgCounter = FALSE;
}

//---------------------------------------------------------------------------
CWizardPage::~CWizardPage()
{
	m_brSolidWhite.DeleteObject();
}

//---------------------------------------------------------------------------
// CWizardPage message handlers
//---------------------------------------------------------------------------
BOOL CWizardPage::OnInitDialog() 
{
	CParsedDialog::OnInitDialog();

	DWORD style = GetStyle();
	ASSERT((style & WS_CHILD) != 0);
	ASSERT((style & WS_BORDER) == 0);
	ASSERT((style & WS_DISABLED) != 0);

	m_brSolidWhite.CreateSolidBrush(CLR_WHITE);

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

//---------------------------------------------------------------------------
BOOL CWizardPage::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	if (nID == IDOK && nCode == 0) // viene inviato se si preme "Enter" con il fuoco su di un control della pagina
	{
		if (m_pParent)
		{
			if (m_pParent->GetDlgItem(IDYES) != NULL &&  m_pParent->GetDlgItem(IDYES)->IsWindowEnabled())
				return m_pParent->SendMessage(WM_COMMAND, (WPARAM)MAKELONG(IDYES, BN_CLICKED), (LPARAM)(m_pParent->GetDlgItem(IDYES)->m_hWnd));

			// La funzione GetDefID dovrebbe restituire l'identificare del pushbutton di 
			// default (se esiste). Invece, a me sembra che dia sempre IDOK...
			DWORD dwDefaultID = m_pParent->GetDefID(); 

			if (HIWORD(dwDefaultID) == DC_HASDEFID)
			{
				CWnd* pWnd = m_pParent->GetDlgItem(LOWORD(dwDefaultID));
				if (!pWnd || !pWnd->m_hWnd)
					return FALSE;//CParsedDialog::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
				
				return m_pParent->SendMessage(WM_COMMAND, (WPARAM)MAKELONG(LOWORD(dwDefaultID), BN_CLICKED), (LPARAM)(pWnd->m_hWnd));
			}
		}
	}

	return CParsedDialog::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
}

//---------------------------------------------------------------------------
HBRUSH CWizardPage::OnCtlColor(CDC* pDC, CWnd* , UINT nCtlColor) 
{
	switch (nCtlColor)
	{	   
		case CTLCOLOR_STATIC:
		case CTLCOLOR_EDIT:
		case CTLCOLOR_LISTBOX:
		case CTLCOLOR_SCROLLBAR:
		case CTLCOLOR_BTN:
			pDC->SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());
			pDC->SetBkColor(AfxGetThemeManager()->GetEnabledControlBkgColor());
		case CTLCOLOR_DLG:	    
			return m_brSolidWhite;
	}	
	return m_brSolidWhite;
}

//---------------------------------------------------------------------------
// CWizardMasterDialog dialog
//---------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CWizardMasterDialog, CLocalizableDialog)
//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CWizardMasterDialog, CLocalizableDialog)
	//{{AFX_MSG_MAP(CWizardMasterDialog)
	ON_WM_DESTROY()
	ON_BN_CLICKED(IDCLOSE,	OnWizardFinishBtnClicked)
	ON_BN_CLICKED(IDNO,		OnWizardBackBtnClicked)
	ON_BN_CLICKED(IDYES,	OnWizardNextBtnClicked)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//---------------------------------------------------------------------------
CWizardMasterDialog::CWizardMasterDialog
							(
								UINT	nDlgID, 
								CWnd*	pParent
							)
	:
	CLocalizableDialog (nDlgID, pParent),
	m_pParent		(pParent),
	m_nDlgID		(nDlgID),
	m_nPlaceholderID(0)
{
}

//---------------------------------------------------------------------------
CWizardMasterDialog::~CWizardMasterDialog()
{
}

//---------------------------------------------------------------------------
// CWizardMasterDialog message handlers
//---------------------------------------------------------------------------
BOOL CWizardMasterDialog::OnInitDialog() 
{
	CLocalizableDialog::OnInitDialog();
	
	ModifyStyleEx (0, WS_EX_CONTROLPARENT);	

	ASSERT(m_nPlaceholderID != 0);	// Be sure to call SetPlaceholderID from
									// your dialogs OnInitDialog

	CenterWindow();

	// Make the first page of the wizard active
	SetFirstPage();

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::DestroyPage(CWizardPage* pPage)
{
	if (!pPage || !pPage->m_bCreated)
		return;
	
	pPage->OnDestroyPage();
	pPage->DestroyWindow();
	pPage->m_bCreated = FALSE;
}

//---------------------------------------------------------------------------
CWizardPage* CWizardMasterDialog::GetFirstPage() const
{
	return (CWizardPage*)m_PageList.GetHead();	
}

//---------------------------------------------------------------------------
CWizardPage* CWizardMasterDialog::GetLastPage() const
{
	return (CWizardPage*)m_PageList.GetTail();	
}

//---------------------------------------------------------------------------
CWizardPage* CWizardMasterDialog::GetActivePage() const
{
	CWizardPage* pPage;
	POSITION pos = m_PageList.GetHeadPosition();
	while (pos)
	{
		pPage = (CWizardPage*)m_PageList.GetNext(pos);
		if (pPage && pPage->m_bActive)
		{
			return pPage;
		}
	}
	return NULL;
}

//---------------------------------------------------------------------------
CWizardPage* CWizardMasterDialog::GetNextPage() const
{
	CWizardPage* pPage;
	POSITION pos = m_PageList.GetHeadPosition();
	while (pos)
	{
		pPage = (CWizardPage*)m_PageList.GetNext(pos);
		if (pPage && pPage->m_bActive)
		{
			if (pos == NULL) return NULL;
			return (CWizardPage*)m_PageList.GetAt(pos);
		}
	}
	return NULL;
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::SetPlaceholderID(int nPlaceholderID)
{
	m_nPlaceholderID = nPlaceholderID;
}

//---------------------------------------------------------------------------
BOOL CWizardMasterDialog::DeactivateCurrentPage()
{
	CWizardPage* pPage = GetActivePage();
	if (!pPage)
		return TRUE;

	ASSERT(pPage->m_bCreated != FALSE);
	if (!pPage->OnDeactivate())
		return FALSE;
	pPage->ShowWindow(SW_HIDE);
	pPage->m_bActive = FALSE;
	return TRUE;
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::AddPage(CWizardPage* pPage)
{
	if (!pPage)
		return;

	m_PageList.AddTail(pPage);
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::SetActivePageByResource(UINT nResourceID)
{
	CWizardPage* pPage = GetPageByResourceID(nResourceID);
	if (!pPage)
		return;

	if (!DeactivateCurrentPage())
		return;

	SetActivePage(pPage);
}


//---------------------------------------------------------------------------
CWizardPage* CWizardMasterDialog::GetPageByResourceID(UINT nResourceID)
{
	CWizardPage* pPage;
	POSITION pos = m_PageList.GetHeadPosition();
	while (pos)
	{
		pPage = (CWizardPage*)m_PageList.GetNext(pos);
		if (pPage && pPage->m_nDialogID == nResourceID)
		{
			return pPage;
		}
	}
	return NULL;
}

//---------------------------------------------------------------------------
BOOL CWizardMasterDialog::SetFirstPage()
{
	CWizardPage* pPage = GetFirstPage();

	if (!DeactivateCurrentPage())
		return FALSE;

	EnableBackBtn(FALSE);
	EnableFinishBtn(m_PageList.GetCount() <= 1);
	EnableNextBtn(m_PageList.GetCount() > 1);

	return SetActivePage(pPage);
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::SetNextPage()
{
	CWizardPage* pPage = GetNextPage();
	if (SetActivePage(pPage))
		EnableBackBtn(TRUE);
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::OnDestroy() 
{
	CWizardPage* pPage;
	POSITION pos = m_PageList.GetHeadPosition();
	while (pos)
	{
		pPage = (CWizardPage*)m_PageList.GetNext(pos);

		DestroyPage(pPage);
	}
	CLocalizableDialog::OnDestroy();
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::OnWizardFinishBtnClicked() 
{
	CWizardPage* pPage;

	pPage = GetActivePage();

	if (!pPage || !pPage->OnDeactivate())
		return;
	
	// notify all pages that we are finishing
	POSITION pos = m_PageList.GetHeadPosition();
	while (pos)
	{
		pPage = (CWizardPage*)m_PageList.GetNext(pos);
		if (pPage && pPage->m_bCreated)
		{
			if (!pPage->OnWizardFinish())
			{
				// data validation failed for one of the pages so we can't close
				return;
			}
		}
	}
	
	if (!OnWizardFinish())
		return;

	// The only reason this line would be needed is if you had controls
	// place in your main wizard dialog outside of the wizard pages.
	// In most "normal" implementations, this line does nothing
	UpdateData(TRUE);

	// close the dialog and return IDCLOSE
	CLocalizableDialog::EndDialog(IDCLOSE);
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::OnWizardBackBtnClicked() 
{
	CWizardPage* pPage = GetActivePage();
	if(!pPage)
	{
		ASSERT(FALSE);
		return;
	}

	LRESULT lResult = pPage->OnWizardBack();
	if (lResult == -1)
		return;

	if (lResult == 0)
	{
		POSITION pos = m_PageList.Find(pPage);
		pPage = NULL;
		if(pos)
		{
			m_PageList.GetPrev(pos);
			if (pos)
				pPage = (CWizardPage*)m_PageList.GetAt(pos);
		}
	}
	else
		pPage = GetPageByResourceID(lResult);

	if (!pPage)
		return;

	if (!SetActivePage(pPage))
		return;	

	if (pPage == GetFirstPage())
		EnableBackBtn(FALSE);
	
	EnableFinishBtn(FALSE);
	EnableNextBtn(TRUE, TRUE);
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::OnWizardNextBtnClicked() 
{
	CWizardPage* pPage = GetActivePage();
	if(!pPage)
	{
		ASSERT(FALSE);
		return;
	}

	LRESULT lResult = pPage->OnWizardNext();
	if (lResult == -1)
		return;

	if (lResult == 0)
	{
		POSITION pos = m_PageList.Find(pPage);
		pPage = NULL;
		if(pos)
		{
			m_PageList.GetNext(pos);
			if (pos)
				pPage = (CWizardPage*)m_PageList.GetAt(pos);
		}
	}
	else
		pPage = GetPageByResourceID(lResult);
	
	if (!pPage)
		return;

	if (!SetActivePage(pPage))
		return;
	
	EnableBackBtn(TRUE);		
	
	if (pPage == GetLastPage())
	{
		EnableNextBtn(FALSE);
		EnableFinishBtn(TRUE, TRUE);
	}
	else
		EnableNextBtn(TRUE, TRUE);	
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::OnCancel()
{
	CWizardPage* pPage;
	POSITION pos = m_PageList.GetHeadPosition();
	while (pos)
	{
		pPage = (CWizardPage*)m_PageList.GetNext(pos);
		if (pPage && pPage->m_bCreated)
		{
			// can we cancel?
			if (!pPage->OnQueryCancel())
				return;
		}
	}
	if (!OnQueryCancel())
		return;

	CLocalizableDialog::OnCancel();
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::EnableFinishBtn(BOOL bEnable, BOOL bSetFocus)
{
	ASSERT(::IsWindow(m_hWnd));
	CWnd* pCloseWnd = GetDlgItem(IDCLOSE);
	if(!pCloseWnd)
	{
		ASSERT(FALSE);// You must have an IDCLOSE on your dialog!
		return;
	}
	pCloseWnd->EnableWindow(bEnable);		
	if (bSetFocus)
		pCloseWnd->SetFocus();		
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::EnableBackBtn(BOOL bEnable, BOOL bSetFocus)
{
	ASSERT(::IsWindow(m_hWnd));
	CWnd* pNoBtn = GetDlgItem(IDNO);
	if(!pNoBtn)
	{
		ASSERT(FALSE);// You must have an IDNO on your dialog!
		return;
	}
	pNoBtn->EnableWindow(bEnable);	
	if (bSetFocus)
		pNoBtn->SetFocus();	
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::EnableNextBtn(BOOL bEnable, BOOL bSetFocus)
{
	ASSERT(::IsWindow(m_hWnd));
	CWnd* pYesBtn = GetDlgItem(IDYES);
	
	if(!pYesBtn)
	{
		ASSERT(FALSE);// You must have an IDYES on your dialog!
		return;
	}
	pYesBtn->EnableWindow(bEnable);		
	if (bSetFocus)
		pYesBtn->SetFocus();
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::SetTitle(LPCTSTR lpszText)
{
	ASSERT(::IsWindow(m_hWnd));
	SetWindowText(lpszText);
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::SetTitle(UINT nTitleTextID)
{
	SetTitle((LPCTSTR)AfxLoadTBString(nTitleTextID, GetResourceModule()));
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::SetFinishBtnText(LPCTSTR lpszText)
{
	ASSERT(::IsWindow(m_hWnd));
	if (!GetDlgItem(IDCLOSE))
	{
		ASSERT(FALSE); // You must have an IDCLOSE on your dialog
		return;
	}
	GetDlgItem(IDCLOSE)->SetWindowText(lpszText);		
}

//---------------------------------------------------------------------------
void CWizardMasterDialog::SetFinishBtnText(UINT nFinishTextID)
{
	SetFinishBtnText((LPCTSTR)AfxLoadTBString(nFinishTextID, GetResourceModule()));
}

//---------------------------------------------------------------------------
//  Functions to mimic the behavior of CPropertySheet
//---------------------------------------------------------------------------

//---------------------------------------------------------------------------
int CWizardMasterDialog::GetActiveIndex() const
{
	CWizardPage* pPage;
	POSITION pos = m_PageList.GetHeadPosition();
	int nIndex = 0;
	while (pos)
	{
		pPage = (CWizardPage*)m_PageList.GetNext(pos);
		if (pPage && pPage->m_bActive)
		{
			return nIndex;
		}
		++nIndex;
	}
	return -1;
}

//---------------------------------------------------------------------------
int CWizardMasterDialog::GetPageIndex(CWizardPage* pPage) const
{
	if(!pPage)
		return -1;
	
	CWizardPage* pTestPage;
	POSITION pos = m_PageList.GetHeadPosition();
	int nIndex = 0;
	while (pos)
	{
		pTestPage = (CWizardPage*)m_PageList.GetNext(pos);
		if (pTestPage == pPage)
		{
			return nIndex;
		}
		++nIndex;
	}
	return -1;
}

//---------------------------------------------------------------------------
int CWizardMasterDialog::GetPageCount() const
{
	return m_PageList.GetCount();
}

//---------------------------------------------------------------------------
CWizardPage* CWizardMasterDialog::GetPage(int nPage) const
{
	POSITION pos = m_PageList.FindIndex(nPage);

	return pos ? (CWizardPage*)m_PageList.GetAt(pos) : NULL;
}

//---------------------------------------------------------------------------
BOOL CWizardMasterDialog::SetActivePage(int nPage)
{
	CWizardPage* pPage = GetPage(nPage);

	return pPage ? SetActivePage(pPage) : FALSE;
}

//---------------------------------------------------------------------------
BOOL CWizardMasterDialog::SetActivePage(CWizardPage* pPage)
{
	if(!m_nPlaceholderID || !pPage ||!::IsWindow(m_hWnd))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	// if the page has not been created, then create it
	if (pPage && pPage->m_bCreated == FALSE)
	{
		if (!pPage->Create(pPage->m_nDialogID, this))
			return FALSE;
		pPage->m_bCreated = TRUE;
		pPage->m_pParent = this;

	  if (!pPage->OnCreatePage())
		  return FALSE;
	}
	
	// deactivate the current page
	if (!DeactivateCurrentPage())
		return FALSE;
	
	CWnd *pWnd = GetDlgItem(m_nPlaceholderID);
	if (!pWnd || !::IsWindow(pWnd->m_hWnd))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CRect rect;
	pWnd->GetWindowRect(&rect);
	ScreenToClient(&rect);
	
	pPage->SetWindowPos
				(
					NULL,
					rect.left, rect.top, 0, 0, 
					SWP_NOZORDER | SWP_NOSIZE | SWP_NOACTIVATE
				);
	pPage->EnableWindow(TRUE);

	pPage->ShowWindow(SW_SHOW);
	pPage->InvalidateRect(NULL);
	
	pPage->OnActivate();
	
	pPage->UpdateWindow();
	

	return TRUE;
}

