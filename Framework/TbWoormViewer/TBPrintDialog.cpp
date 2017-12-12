// TBPrintDialog.cpp : implementation file
//

#include "stdafx.h"

#include <tbgeneric\globals.h>
#include <tbgeneric\TBStrings.h>
#include <tbgeneric\dib.h>
#include <TbGeneric\TBThemeManager.h>
#include <tbgeneric\wndobjdescription.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\ParsEdt.h>

#include "woormvw.h"
#include "woormdoc.hjson" //JSON AUTOMATIC UPDATE

#include "TBPrintDialog.h"

// CTBPrintDialog
#define IDC_BUTTON_MAINTAINPRINTER  2039
#define IDC_BUTTON_LETTERHEAD   2040
#define IDC_BUTTON_M4   2041


//-----------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(CTBPrintButtonOk, CButton)
END_MESSAGE_MAP()



//-----------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(CTBPrintDialog, CPrintDialog)

BEGIN_MESSAGE_MAP(CTBPrintDialog, CPrintDialog)
	ON_BN_CLICKED(IDC_BUTTON_MAINTAINPRINTER, OnToggleApplicationPrinter)
	ON_BN_CLICKED(IDC_BUTTON_LETTERHEAD, OnTogglePrintOnLetterhead)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTBPrintDialog::CTBPrintDialog(BOOL bPrintSetupOnly, DWORD dwFlags, CWnd* pParentWnd)
	: 
	CPrintDialog(bPrintSetupOnly, dwFlags, pParentWnd), 
	m_pView(NULL)
{
	m_bMaintainPrinter = FALSE;
	m_bShowPrintOnLetterhead = FALSE;
	m_pbPrintOnLetterhead = NULL;

	AddCustomTemplate();
}

//-----------------------------------------------------------------------------
CTBPrintDialog::CTBPrintDialog (CPrintDialog* pSource, CWoormView* pView, BOOL bMaintainPrinter, BOOL bShowPrintOnLetterhead, DataBool* pbPrintOnLetterhead)
	:
	CPrintDialog (
					FALSE,
					PD_ALLPAGES | PD_USEDEVMODECOPIES |	PD_NOSELECTION, 
					pSource->m_hWnd ? pSource->GetParent() : NULL
				),
	m_pView(pView)
{
	Assign(pSource);

	m_bMaintainPrinter = bMaintainPrinter;
	m_bShowPrintOnLetterhead = bShowPrintOnLetterhead;
	m_pbPrintOnLetterhead = pbPrintOnLetterhead;

	AddCustomTemplate();
}

//-----------------------------------------------------------------------------
void CTBPrintDialog::AddCustomTemplate()
{
	if (AfxIsRemoteInterface())
	{
		m_pd.Flags |= PD_ENABLEPRINTTEMPLATE;
		m_pd.hInstance = AfxFindResourceHandle(MAKEINTRESOURCE(IDD_DLG_WEBPRINT), RT_DIALOG);	 
		m_pd.lpPrintTemplateName = MAKEINTRESOURCE(IDD_DLG_WEBPRINT);
	}
}

//-----------------------------------------------------------------------------
void CTBPrintDialog::Assign(CPrintDialog* pDialog)
{
	m_pd.nFromPage = pDialog->m_pd.nFromPage;
	m_pd.nMaxPage = pDialog->m_pd.nMaxPage;
	m_pd.nToPage = pDialog->m_pd.nToPage;
	m_pd.nMinPage = pDialog->m_pd.nMinPage;
	m_pd.nCopies = pDialog->m_pd.nCopies;
	m_pd.Flags = pDialog->m_pd.Flags;
	m_pd.hDevMode = CopyHandle(pDialog->m_pd.hDevMode);
	m_pd.hDevNames = CopyHandle(pDialog->m_pd.hDevNames);
	m_pd.hDC = pDialog->m_pd.hDC;
	m_pd.hwndOwner =  pDialog->m_pd.hwndOwner;
	m_hWndOwner = pDialog->m_hWndOwner;
	m_nFlags = pDialog->m_nFlags;
}

//-----------------------------------------------------------------------------
CTBPrintDialog::~CTBPrintDialog()
{
}

//-----------------------------------------------------------------------------
INT_PTR CTBPrintDialog::DoModal()
{
	CTBWinThread::PumpThreadMessages();  //used to process all pending messages in order to fix active window

	CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);

	INT_PTR res = __super::DoModal();
	if (res == -1 && ! AfxGetThreadContext()->IsClosing())
		throw (new CThreadAbortedException());

	AfxSetThreadInModalState(FALSE, m_hWndPersistent);
	if (AfxIsRemoteInterface())
	{
		if (res == IDOK)
		{
			ASSERT(m_pView);
			CString sPath = CWndObjDescription::GetTempImagesPath(m_bOkButton.m_strPdfFile); 
			m_pView->OnPDFPrint(m_pd.nFromPage, m_pd.nToPage, m_pd.nCopies, sPath);
		}
		return IDCANCEL;
	}
	return res;
}

//-----------------------------------------------------------------------------
BOOL CTBPrintDialog::OnInitDialog()
{
	BOOL bResult = __super::OnInitDialog();

	if (!AfxIsRemoteInterface())
	{
		CRect rect;
		GetWindowRect(&rect);

		int right = rect.Width() - 20;
		int bottom = rect.Height();

		rect.InflateRect(0, (m_bShowPrintOnLetterhead && m_pbPrintOnLetterhead) ? 30 : 20);
		SetWindowPos(NULL, 0, 0, rect.Width(), rect.Height(), SWP_NOMOVE);

		m_chkSetApplicationPrinter.Create(_TB("Set as preferred application printer"),WS_VISIBLE|WS_CHILD|BS_CHECKBOX, CRect(20, 310, right, 325), this, IDC_BUTTON_MAINTAINPRINTER);
		m_chkSetApplicationPrinter.SetFont(GetFont());
		m_chkSetApplicationPrinter.SetCheck(m_bMaintainPrinter);

		if (m_bShowPrintOnLetterhead && m_pbPrintOnLetterhead)
		{
			m_chkPrintOnLetterhead.Create(_TB("Print on Letterhead"), WS_VISIBLE | WS_CHILD | BS_CHECKBOX, CRect(20, 335, right, 350), this, IDC_BUTTON_LETTERHEAD);
			m_chkPrintOnLetterhead.SetFont(GetFont());
			m_chkPrintOnLetterhead.SetCheck(*m_pbPrintOnLetterhead ? 1 : 0);
		}
	}
	else
	{
		VERIFY(m_bOkButton.SubclassDlgItem(IDOK, this));
		CGuid aGuid;
		aGuid.GenerateGuid();
		m_bOkButton.m_strPdfFile = CString(aGuid) + _T(".pdf");
	}
	AfxSetThreadInModalState(TRUE, m_hWnd);
	//Devo mettere da parte l'handle di finestra per poterla rimuovere dalle finestre di thread in
	//uscita dalla DoModal (I metodi DestroyWindow e EndDialog non venivano chiamati)
	m_hWndPersistent = m_hWnd; 

	return bResult;  
}

//-----------------------------------------------------------------------------
void CTBPrintDialog::OnToggleApplicationPrinter()
{
	m_bMaintainPrinter = !m_chkSetApplicationPrinter.GetCheck();

	m_chkSetApplicationPrinter.SetCheck(m_bMaintainPrinter);
}

//-----------------------------------------------------------------------------
void CTBPrintDialog::OnTogglePrintOnLetterhead()
{
	ASSERT(m_bShowPrintOnLetterhead);
	ASSERT_VALID(m_pbPrintOnLetterhead);

	*m_pbPrintOnLetterhead = !m_chkPrintOnLetterhead.GetCheck();

	m_chkPrintOnLetterhead.SetCheck(*m_pbPrintOnLetterhead ? 1 : 0);
}

