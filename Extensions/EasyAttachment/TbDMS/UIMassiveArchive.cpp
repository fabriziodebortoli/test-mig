#include "stdafx.h"

#include "TBRepositoryManager.h"
#include "BDMassiveArchive.h"
#include "UIMassiveArchive.h"

#include "EasyAttachment\JsonForms\UIMassiveArchive\IDD_MASSIVEARCHIVE_TOOLBAR.hjson"
#include "EasyAttachment\JsonForms\UIMassiveArchive\IDD_MASSIVEARCHIVE_WIZARD.hjson"
#include "EasyAttachment\JsonForms\UIMassiveArchive\IDD_RW_MASSIVEARCHIVE_FILESTOADD.hjson"

//////////////////////////////////////////////////////////////////////////////
//						CMassiveArchiveWizardFormView
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CMassiveArchiveWizardFormView, CWizardFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CMassiveArchiveWizardFormView, CWizardFormView)
	//{{AFX_MSG_MAP(CMassiveArchiveWizardFormView)
	ON_WM_TIMER()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CMassiveArchiveWizardFormView::CMassiveArchiveWizardFormView()
	:
	CWizardFormView(_NS_VIEW("MassiveArchiveView"), IDD_MASSIVEARCHIVE_WIZARD_VIEW)
{
}

//-----------------------------------------------------------------------------
void CMassiveArchiveWizardFormView::UpdateTitle()
{
	GetDocument()->CAbstractFormDoc::SetTitle(_TB("Massive Archive"));
}

//------------------------------------------------------------------------------
void CMassiveArchiveWizardFormView::OnTimer(UINT nUI)
{
	if (nUI == CHECK_MASSIVEARCHIVE_TIMER)
		GetDocument()->DoOnTimer();
}

/////////////////////////////////////////////////////////////////////////////
//			class CFilesToArchiveBodyEdit Implementation
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CFilesToArchiveBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------	
CFilesToArchiveBodyEdit::CFilesToArchiveBodyEdit()
{
}

//-----------------------------------------------------------------------------	
BOOL CFilesToArchiveBodyEdit::OnDblClick(UINT nFlags, CBodyEditRowSelected* pCurrentRow)
{
	GetDocument()->ShowDocument(pCurrentRow->m_pRec);
	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL CFilesToArchiveBodyEdit::OnPostCreateClient()
{
	// dopo che ho creato il bodyedit lo abilito ad accettare il drag dei files
	ModifyStyleEx(0, WS_EX_ACCEPTFILES);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFilesToArchiveBodyEdit::OnSubFolderFound()
{
	return (AfxMessageBox(_TB("Consider also files in subfolders?"), MB_YESNO | MB_ICONQUESTION) == IDYES);
} 

//-----------------------------------------------------------------------------
void CFilesToArchiveBodyEdit::OnDropFiles(const CStringArray& arDroppedFiles)
{
	GetDocument()->AddFiles(&arDroppedFiles);
}
