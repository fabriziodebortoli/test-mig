#include "stdafx.h" 

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "extdoc.h"
#include "ReportListDlg.h"
#include "formmng.h"

//................................. resources
#include <TbWoormViewer\listdlg.hjson> //JSON AUTOMATIC UPDATE

#include <TbGenlib\commands.hrc>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//==============================================================================
//          Class CListDocumentReportsDlg implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CListDocumentReportsDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CListDocumentReportsDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CListDocumentReportsDlg)
		ON_LBN_DBLCLK       (IDC_GENERIC_LIST,			OnReportSelected)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CListDocumentReportsDlg::CListDocumentReportsDlg(CAbstractFormDoc* pDoc, CReportMenuNode*& pReportRootNode, CWnd* aParent /*=NULL*/)
	:
	CParsedDialog	(IDD_GENERIC_LIST, aParent),
	m_pDoc			(pDoc),
	m_pReports		(pReportRootNode),
	m_mlbReports	(TBIcon(szIconOk, IconSize::CONTROL))
{
	ASSERT_VALID(m_pReports);
}

CListDocumentReportsDlg::~CListDocumentReportsDlg()
{
	SAFE_DELETE(m_pReports);
}

//-----------------------------------------------------------------------------
BOOL CListDocumentReportsDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	SetWindowText(_TB("List of Reports"));
	
	m_mlbReports.SubclassDlgItem (IDC_GENERIC_LIST, this);
	m_mlbReports.SetStyle(CMultiListBox::UNSORTED);
	
	int nDefaultIdx = m_pDoc->m_pFormManager->GetIndexReportDefault();
	for (int i = 0; i <= m_pReports->GetSonsUpperBound(); i++)
	{
		m_mlbReports.AddString(m_pReports->GetSonAt(i)->GetNodeTag(), _T(""));
	}
	m_mlbReports.SetFlag(nDefaultIdx, CMultiListBox::CHECK_ONE);
    return TRUE;
}

//-----------------------------------------------------------------------------
void CListDocumentReportsDlg::OnOK()
{
	OnReportSelected();
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CListDocumentReportsDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
void CListDocumentReportsDlg::OnReportSelected()
{
	int idx = m_mlbReports.GetCurSel();
	if (idx > -1)
	{
		//m_pDoc->SelReport(idx); fa casino (una dialog modale non puo' lanciare un document)
		if (m_pDoc)
		{
			m_pDoc->PostMessage(WM_COMMAND, ID_EXTDOC_DDTB_ENUMREPORT0 + idx, 0);
		}
	}
	EndDialog(IDOK);
}
