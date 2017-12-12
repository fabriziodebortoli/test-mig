#include "stdafx.h" 

#include <TBXMLCore\xmlgeneric.h>
#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TBGENLIB\baseapp.h>
#include <TBGENLIB\DirTreeCtrl.h>
#include <TBWoormEngine\ActionsRepEngin.h>
#include <TBWoormEngine\prgdata.h>
#include <TBWoormEngine\inputmng.h>
#include <TBGES\BARQUERY.H>

#include <XEngine\TBXMLEnvelope\GenFunc.h>
#include <XEngine\TBXMLEnvelope\XEngineObject.h>

#include "XMLDataMng.h"
#include "XMLProfileInfo.h"
#include "ExpCriteriaWiz.h"
#include "ExpCriteriaDlg.h"
#include "GenFunc.h"

// resource declarations
#include "ExpCriteriaDlg.hjson"
#include "xmldatamng.hjson"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


//-----------------------------------------------------------------------------
// CExportWizardPage dialog
//---------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CExportWizardPage, CWizardTabDialog)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CExportWizardPage, CWizardTabDialog)
	//{{AFX_MSG_MAP(CExportWizardPage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CExportWizardPage::CExportWizardPage(UINT nIDTemplate, CWnd* pParent /*=NULL*/, const CString& sName /*= _T("")*/)
	:
	CWizardTabDialog	(sName, nIDTemplate),
	m_pXMLDataMng		(NULL),
	m_pXMLExportDocSel	(NULL)
{
}

//-----------------------------------------------------------------------------
BOOL CExportWizardPage::OnInitDialog()
{
	if (!GetDocument() || !m_pParentTabManager || !m_pParentTabManager->IsKindOf(RUNTIME_CLASS(CTabWizard)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_pXMLDataMng = GetWizardDoc()->m_pDataManagerExport;
	if (!m_pXMLDataMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	//contiene le info relative alla prima parte del wizard:
	// esportare un singolo documento o un insieme di documenti
	// il profilo da utilizzare in fase di esportazione
	m_pXMLExportDocSel = m_pXMLDataMng->GetXMLExportDocSelection();
	if (!m_pXMLExportDocSel)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return  CWizardTabDialog::OnInitDialog();
}

//----------------------------------------------------------------------------
LRESULT CExportWizardPage::OnWizardCancel()
{
	if (m_pXMLDataMng)
		m_pXMLDataMng->SetContinueImportExport (FALSE);

	return CWizardTabDialog::OnWizardCancel();
}

//-----------------------------------------------------------------------------
// CXMLExportPresentationPage dialog
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLExportPresentationPage, CExportWizardPage)

//-----------------------------------------------------------------------------
CXMLExportPresentationPage::CXMLExportPresentationPage(CWnd* pParent /*=NULL*/)
	:
	CExportWizardPage (IDD_WIZARD_PRESENTATION_PAGE, pParent, _T("PresentationPage"))
{
}

//-----------------------------------------------------------------------------
LRESULT CXMLExportPresentationPage::OnGetBitmapID()
{
	return IDB_WIZARD_SEL_DOC_PAGE;
}


//-----------------------------------------------------------------------------
// CXMLExportSchemaPage dialog
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLExportSchemaPage, CExportWizardPage)

//-----------------------------------------------------------------------------
CXMLExportSchemaPage::CXMLExportSchemaPage(CWnd* pParent /*=NULL*/)
	:
	CExportWizardPage (IDD_WIZARD_SCHEMA_PAGE, pParent, _T("SchemaPage"))
{

}

//-----------------------------------------------------------------------------
LRESULT CXMLExportSchemaPage::OnGetBitmapID()
{
	return IDB_WIZARD_SEL_DOC_PAGE;
}

//-----------------------------------------------------------------------------
void CXMLExportSchemaPage::BuildDataControlLinks()
{
	CButton* pBtnOnlySchema = ((CButton*)GetDlgItem(IDC_EXPORT_ONLY_SCHEMA_BTN));
	CButton* pBtnOnlyDoc = ((CButton*)GetDlgItem(IDC_EXPORT_ONLY_DOC_BTN));
	CButton* pBtnOnlySmart = ((CButton*)GetDlgItem(IDC_EXPORT_SMART_SCHEMA_BTN));

	if (AfxIsActivated(MAGONET_APP, XGATE_MOD))
	{	
		pBtnOnlySchema->SetCheck(m_pXMLExportDocSel && m_pXMLExportDocSel->IsOnlySchemaToExport());
		pBtnOnlyDoc->SetCheck(m_pXMLExportDocSel && m_pXMLExportDocSel->IsOnlyDocToExport());
	}
	else
	{
		pBtnOnlySchema->SetCheck(FALSE);
		pBtnOnlySchema->ShowWindow(SW_HIDE);
		pBtnOnlyDoc->SetCheck(FALSE);
		pBtnOnlyDoc->ShowWindow(SW_HIDE);
	}
	

	if (AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD) || AfxIsActivated(TBEXT_APP, MAGICLINK_MOD))
	{
		BOOL bWindowsNTUser = AfxGetLoginManager()->IsWinNTUser(AfxGetLoginInfos()->m_strUserName);
		pBtnOnlySmart->SetCheck
			(
				(
					(m_pXMLExportDocSel && m_pXMLExportDocSel->IsSmartSchemaToExport()) ||
					!AfxIsActivated(MAGONET_APP, XGATE_MOD)
				) 
				&&
				!bWindowsNTUser
			);
		pBtnOnlySmart->EnableWindow(!bWindowsNTUser);
	}
	else
	{
		pBtnOnlySmart->SetCheck(FALSE);
		pBtnOnlySmart->ShowWindow(SW_HIDE);
	}

	// da rimuovere un domani se decidessimo di esportare sia dati sia schema
	GetDlgItem(IDC_EXPORT_DOC_AND_SCHEMA_BTN)->ShowWindow(SW_HIDE);
}		

//-----------------------------------------------------------------------------
LRESULT CXMLExportSchemaPage::OnWizardNext()
{
	if (!m_pXMLExportDocSel)
		return WIZARD_DEFAULT_TAB;
	
	if (((CButton*)GetDlgItem(IDC_EXPORT_ONLY_DOC_BTN))->GetCheck() == BST_CHECKED)
		m_pXMLExportDocSel->m_nSchemaSelType = EXPORT_ONLY_DOC;
	else if (((CButton*)GetDlgItem(IDC_EXPORT_ONLY_SCHEMA_BTN))->GetCheck() == BST_CHECKED)
		m_pXMLExportDocSel->m_nSchemaSelType = EXPORT_ONLY_SCHEMA;
	else if (((CButton*)GetDlgItem(IDC_EXPORT_SMART_SCHEMA_BTN))->GetCheck() == BST_CHECKED)
		m_pXMLExportDocSel->m_nSchemaSelType = EXPORT_SMART_SCHEMA;
	else if (((CButton*)GetDlgItem(IDC_EXPORT_DOC_AND_SCHEMA_BTN))->GetCheck() == BST_CHECKED)
		m_pXMLExportDocSel->m_nSchemaSelType = EXPORT_DOC_AND_SCHEMA;

	return WIZARD_DEFAULT_TAB;
}

//-----------------------------------------------------------------------------
// CXMLExportSelDocPage dialog
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLExportSelDocPage, CExportWizardPage)

//-----------------------------------------------------------------------------
CXMLExportSelDocPage::CXMLExportSelDocPage(CWnd* pParent /*=NULL*/)
	:
	CExportWizardPage (IDD_WIZARD_SEL_DOC_PAGE, pParent, _T("SelDocPage"))
{
}

//-----------------------------------------------------------------------------
void CXMLExportSelDocPage::BuildDataControlLinks()
{
	CAbstractFormDoc* pDoc = GetExportDocument();

	if (!pDoc || !pDoc->ValidCurrentRecord() || pDoc->IsEditingParamsFromExternalController())
	{
		((CButton*)GetDlgItem(IDC_EXPORT_ONLY_CURR_DOC_BTN))->EnableWindow(FALSE);
		if (m_pXMLExportDocSel && m_pXMLExportDocSel->IsOnlyCurrentDocToExport())
		{
			((CButton*)GetDlgItem(IDC_EXPORT_ONLY_CURR_DOC_BTN))->SetCheck(FALSE);
			m_pXMLExportDocSel->m_nDocSelType = EXPORT_DOC_SET;
		}
	}
	else
		((CButton*)GetDlgItem(IDC_EXPORT_ONLY_CURR_DOC_BTN))->SetCheck(m_pXMLExportDocSel && m_pXMLExportDocSel->IsOnlyCurrentDocToExport());

	
	((CButton*)GetDlgItem(IDC_EXPORT_DOCS_SET_BTN))->SetCheck(m_pXMLExportDocSel && m_pXMLExportDocSel->MustUseCriteria());
	((CButton*)GetDlgItem(IDC_EXPORT_ALL_DOCS_BTN))->SetCheck(m_pXMLExportDocSel && m_pXMLExportDocSel->AreAllDocumentsToExport());
}

//-----------------------------------------------------------------------------
LRESULT CXMLExportSelDocPage::OnWizardNext()
{
	if (!m_pXMLExportDocSel)
		return WIZARD_DEFAULT_TAB;
	
	if (((CButton*)GetDlgItem(IDC_EXPORT_ONLY_CURR_DOC_BTN))->GetCheck() == BST_CHECKED)
		m_pXMLExportDocSel->m_nDocSelType = EXPORT_ONLY_CURR_DOC;
	else if (((CButton*)GetDlgItem(IDC_EXPORT_DOCS_SET_BTN))->GetCheck() == BST_CHECKED)
		m_pXMLExportDocSel->m_nDocSelType = EXPORT_DOC_SET;
	else if (((CButton*)GetDlgItem(IDC_EXPORT_ALL_DOCS_BTN))->GetCheck() == BST_CHECKED)
		m_pXMLExportDocSel->m_nDocSelType = EXPORT_ALL_DOCS;

	return WIZARD_DEFAULT_TAB;
}

//-----------------------------------------------------------------------------
LRESULT CXMLExportSelDocPage::OnGetBitmapID()
{
	return IDB_WIZARD_SEL_DOC_PAGE;
}

//////////////////////////////////////////////////////////////////////////////
//             class CSelProfileCombo definition and implementation
//////////////////////////////////////////////////////////////////////////////
//
class CSelProfileCombo : public CStrCombo
{
	DECLARE_DYNCREATE (CSelProfileCombo)

private:
	CStringArray* m_pProfileNames;

public:
	CSelProfileCombo ();

public:
	void SetProfileNames(CStringArray*);

public:
	virtual	void	OnFillListBox();
};


//-----------------------------------------------------------------------------	
IMPLEMENT_DYNCREATE (CSelProfileCombo, CStrCombo)

//-----------------------------------------------------------------------------
CSelProfileCombo::CSelProfileCombo()
	:
	m_pProfileNames	(NULL)
{
}

//-----------------------------------------------------------------------------
void CSelProfileCombo::SetProfileNames(CStringArray* pProfileNames)
{
	//prendo il profilo preferenziale
	m_pProfileNames = pProfileNames;
}

//-----------------------------------------------------------------------------
void CSelProfileCombo::OnFillListBox()
{
	ResetAssociations (TRUE);

	if (!m_pProfileNames)
		return;	

	for (int i = 0 ; i < m_pProfileNames->GetSize() ; i++ )
		if (!m_pProfileNames->GetAt(i).IsEmpty())
		{
			//come descrizione inserisco il nome del profilo
			CString strProfileName = ::GetName(m_pProfileNames->GetAt(i));
			AddAssociation(strProfileName, strProfileName);	
		}
}


//-----------------------------------------------------------------------------
// CXMLExportSelProfilePage dialog
//---------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLExportSelProfilePage, CExportWizardPage)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXMLExportSelProfilePage, CExportWizardPage)
	//{{AFX_MSG_MAP(CXMLExportSelProfilePage)
	ON_BN_CLICKED	(IDC_EXPORT_PREDEFINED_PROFILE_BTN,	OnSelDefaultProfileChanged)
	ON_BN_CLICKED	(IDC_EXPORT_PREF_PROFILE_BTN,		OnSelDefaultProfileChanged)
	ON_BN_CLICKED	(IDC_EXPORT_SEL_PROFILE_BTN,		OnSelDefaultProfileChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CXMLExportSelProfilePage::CXMLExportSelProfilePage(CWnd* pParent /*=NULL*/)
	:
	CExportWizardPage (IDD_WIZARD_SEL_PROFILE_PAGE, pParent, _T("SelProfilePage"))
{
}

//-----------------------------------------------------------------------------
void CXMLExportSelProfilePage::BuildDataControlLinks()
{
	if (!m_pXMLExportDocSel)
		return;

	//predefinito
	CButton* pButton = ((CButton*)GetDlgItem(IDC_EXPORT_PREDEFINED_PROFILE_BTN));
	pButton->SetCheck(m_pXMLExportDocSel->IsPredefinedProfileToUse());
	pButton->EnableWindow(!m_pXMLExportDocSel->IsSmartSchemaToExport() || m_pXMLExportDocSel->m_bExistPredefined);

	//preferenziale
	pButton = ((CButton*)GetDlgItem(IDC_EXPORT_PREF_PROFILE_BTN));
	pButton->SetCheck(m_pXMLExportDocSel->IsPreferredProfileToUse());
	pButton->EnableWindow(!m_pXMLExportDocSel->m_strPreferredProfile.IsEmpty());

	//combo di selezione profili
	CSelProfileCombo* pCombo = (CSelProfileCombo*)AddLink (IDC_EXPORT_SEL_PROFILE_COMBO, _T("Profilo"),	NULL,	&m_pXMLExportDocSel->m_strProfileName,	RUNTIME_CLASS(CSelProfileCombo));
	pButton = ((CButton*)GetDlgItem(IDC_EXPORT_SEL_PROFILE_BTN));
	pCombo->SetProfileNames(&m_pXMLExportDocSel->m_aProfNamesArray);
	pButton->SetCheck(m_pXMLExportDocSel->IsSelectedProfileToUse() && m_pXMLExportDocSel->AreSelProfilePresent());
	pButton->EnableWindow(m_pXMLExportDocSel->AreSelProfilePresent());
	pCombo->EnableWindow(m_pXMLExportDocSel->AreSelProfilePresent());
	
	OnSelDefaultProfileChanged();
}

//-----------------------------------------------------------------------------
void CXMLExportSelProfilePage::OnDisableControlsForBatch()
{
	if (!m_pXMLExportDocSel)
		return;

	BOOL bSelProfile = ((CButton*)GetDlgItem(IDC_EXPORT_SEL_PROFILE_BTN))->GetCheck() == BST_CHECKED;
	if (!bSelProfile)
		m_pXMLExportDocSel->m_strProfileName.Clear();

	m_pXMLExportDocSel->m_strProfileName.SetReadOnly(!bSelProfile);
}

//-----------------------------------------------------------------------------
void CXMLExportSelProfilePage::OnSelDefaultProfileChanged()
{
	if (!m_pXMLExportDocSel)
		return;

	BOOL bSelProfile = ((CButton*)GetDlgItem(IDC_EXPORT_SEL_PROFILE_BTN))->GetCheck() == BST_CHECKED;
	if (!bSelProfile)
		m_pXMLExportDocSel->m_strProfileName.Clear();
	else
		if (m_pXMLExportDocSel->m_strProfileName.IsEmpty() && m_pXMLExportDocSel->m_aProfNamesArray.GetSize() > 0)
			m_pXMLExportDocSel->m_strProfileName = ::GetName(m_pXMLExportDocSel->m_aProfNamesArray.GetAt(0));
	
	m_pXMLExportDocSel->m_strProfileName.SetReadOnly(!bSelProfile); 
	
	CString strText = _TB("Pre&ferred profile");
	CButton* pButton = (CButton*)GetDlgItem(IDC_EXPORT_PREF_PROFILE_BTN);
	if (!m_pXMLExportDocSel->m_strPreferredProfile.IsEmpty())
		strText += cwsprintf(_T(" (%s)"), m_pXMLExportDocSel->m_strPreferredProfile);
	
    pButton->SetWindowText(strText);

	pButton = (CButton*)GetDlgItem(IDC_EXPORT_PREDEFINED_PROFILE_BTN);
	
	strText =  _TB("&Default profile");
	
	CString strProfile = m_pXMLExportDocSel->m_bExistPredefined 
								? szPredefined
								: _TB("Document description");
	if (!strProfile.IsEmpty())
		strText += cwsprintf (_T(" (%s)"), strProfile);
		
	pButton->SetWindowText(strText);
	
	GetWizardDoc()->UpdateDataView(); 
}		

//-----------------------------------------------------------------------------
LRESULT CXMLExportSelProfilePage::OnWizardNext()
{
	if (!m_pXMLExportDocSel)
		return WIZARD_DEFAULT_TAB;
	
	if (((CButton*)GetDlgItem(IDC_EXPORT_PREDEFINED_PROFILE_BTN))->GetCheck() == BST_CHECKED)
		m_pXMLExportDocSel->m_nProfileSelType = USE_PREDEFINED_PROFILE;
	else 
	{
		if (((CButton*)GetDlgItem(IDC_EXPORT_PREF_PROFILE_BTN))->GetCheck() == BST_CHECKED)
			m_pXMLExportDocSel->m_nProfileSelType = USE_PREFERRED_PROFILE;

		else 
			if (((CButton*)GetDlgItem(IDC_EXPORT_SEL_PROFILE_BTN))->GetCheck() == BST_CHECKED)
				m_pXMLExportDocSel->m_nProfileSelType = USE_SELECTED_PROFILE;
	}

	return WIZARD_DEFAULT_TAB;
}

//-----------------------------------------------------------------------------
LRESULT CXMLExportSelProfilePage::OnGetBitmapID()
{
	return IDB_WIZARD_SEL_PROF_PAGE;
}


//-----------------------------------------------------------------------------
//  CXMLExportCriteriaPage PropertyPage
//---------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLExportCriteriaPage, CExportWizardPage)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXMLExportCriteriaPage, CExportWizardPage)
	//{{AFX_MSG_MAP(CXMLExportCriteriaPage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CXMLExportCriteriaPage::CXMLExportCriteriaPage(CWnd* pParent /*=NULL*/)
	:
	CExportWizardPage(IDD_WIZARD_SEL_CRITERIA_PAGE, pParent, _T("CriteriaPage"))
{	
}

//-----------------------------------------------------------------------------
void CXMLExportCriteriaPage::BuildDataControlLinks()
{
	CPreferencesCriteria* pPreferencesCriteria = GetWizardDoc()->GetPreferencesCriteria();
	if (!pPreferencesCriteria)
		return;

	AddLink (IDC_EXPORT_OSL_SEL_CHECK,				 _T("OSL_SEL_CHECK"), NULL,	&pPreferencesCriteria->m_bSelModeOSL,		RUNTIME_CLASS(CBoolButton));
	AddLink (IDC_EXPORT_APP_SEL_CHECK,				 _T("APP_SEL_CHECK"), NULL,	&pPreferencesCriteria->m_bSelModeApp,		RUNTIME_CLASS(CBoolButton));
	AddLink (IDC_EXPORT_USER_SEL_CHECK,				 _T("USER_SEL_CHECK"), NULL,	&pPreferencesCriteria->m_bSelModeUsr,		RUNTIME_CLASS(CBoolButton));
}

//-----------------------------------------------------------------------------
void CXMLExportCriteriaPage::OnDisableControlsForBatch()
{
	CPreferencesCriteria* pPreferencesCriteria = GetWizardDoc()->GetPreferencesCriteria();
	if (!pPreferencesCriteria)
	{
		ASSERT(FALSE);
		return;
	}
	CUserExportCriteria* pUserCrit = GetWizardDoc()->GetUserExportCriteria();

	pPreferencesCriteria->m_bSelModeOSL = pPreferencesCriteria->m_bSelModeOSL && GetWizardDoc()->IsTracedDBTMasterTable();
	pPreferencesCriteria->m_bSelModeOSL.SetReadOnly(!(
														AfxGetLoginInfos()->m_bAuditing && 
														AfxIsActivated(TBEXT_APP, TBAUDITING_ACT) && 
														GetWizardDoc()->IsTracedDBTMasterTable()
													  )
													);
	pPreferencesCriteria->m_bSelModeApp.SetReadOnly(GetWizardDoc()->GetFirstAppTabIDD() == 0);
	pPreferencesCriteria->m_bSelModeUsr.SetReadOnly(!pUserCrit || 
													!pUserCrit->m_pQueryInfo || 
													( pUserCrit->m_pQueryInfo->m_TableInfo.m_strFilter.IsEmpty() && 
													  pUserCrit->m_pQueryInfo->m_TableInfo.m_strSort.IsEmpty())
												   );	
}

//-----------------------------------------------------------------------------
LRESULT CXMLExportCriteriaPage::OnGetBitmapID()
{
	return IDB_WIZARD_SEL_CRITERIA_PAGE;
}


//-----------------------------------------------------------------------------
// CXMLExportOSLCriteriaPage dialog
//---------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLExportOSLCriteriaPage, CExportWizardPage)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXMLExportOSLCriteriaPage, CExportWizardPage)
	//{{AFX_MSG_MAP(CXMLExportOSLCriteriaPage)
	ON_EN_VALUE_CHANGED	(IDC_EXPORT_AUDIT_FROMDATE_EDIT,	OnOSLFromDateChanged)
	ON_EN_VALUE_CHANGED	(IDC_EXPORT_AUDIT_TODATE_EDIT,	OnOSLToDateChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-----------------------------------------------------------------------------

CXMLExportOSLCriteriaPage::CXMLExportOSLCriteriaPage(CWnd* pParent /*=NULL*/)
	:
	CExportWizardPage(IDD_WIZARD_SEL_AUDIT_PAGE, pParent, _T("OSLCriteriaPage"))
{
}

//-----------------------------------------------------------------------------
void CXMLExportOSLCriteriaPage::BuildDataControlLinks() 
{
	//CPreferencesCriteria* pPreferencesCriteria = GetWizardDoc()->GetPreferencesCriteria();
	COSLExportCriteria* pOSLExportCriteria = GetWizardDoc()->GetOSLExportCriteria();

	AddLink (IDC_EXPORT_AUDIT_FROMDATE_EDIT,	_T("FROMDATE"), NULL,	&pOSLExportCriteria->m_FromDate,	RUNTIME_CLASS(CDateTimeEdit));
	AddLink (IDC_EXPORT_AUDIT_TODATE_EDIT,		_T("TODATE"),	NULL,	&pOSLExportCriteria->m_ToDate,		RUNTIME_CLASS(CDateTimeEdit));
	AddLink (IDC_EXPORT_AUDIT_INSERTED_BTN,  	_T("INSERTED"), NULL,	&pOSLExportCriteria->m_bInserted,	RUNTIME_CLASS(CBoolButton));
	AddLink (IDC_EXPORT_AUDIT_UPDATED_BTN,  	_T("UPDATED"),  NULL,	&pOSLExportCriteria->m_bUpdated,	RUNTIME_CLASS(CBoolButton));
	AddLink (IDC_EXPORT_AUDIT_DELETED_BTN,  	_T("DELETED"),	NULL,	&pOSLExportCriteria->m_bDeleted,	RUNTIME_CLASS(CBoolButton));
}

//-----------------------------------------------------------------------------
LRESULT CXMLExportOSLCriteriaPage::OnWizardBack()
{
	if (!GetWizardDoc()->CheckDate())
		return WIZARD_SAME_TAB;

	return WIZARD_DEFAULT_TAB;
}

//-----------------------------------------------------------------------------
void CXMLExportOSLCriteriaPage::OnOSLFromDateChanged()
{
	COSLExportCriteria* pOSLExportCriteria = GetWizardDoc()->GetOSLExportCriteria();

	if (!GetWizardDoc()->CheckDate())
	{
		pOSLExportCriteria->m_FromDate = pOSLExportCriteria->m_ToDate;
		GetWizardDoc()->UpdateDataView();
	}
}

//-----------------------------------------------------------------------------
void CXMLExportOSLCriteriaPage::OnOSLToDateChanged()
{
	COSLExportCriteria* pOSLExportCriteria = GetWizardDoc()->GetOSLExportCriteria();

	if (!GetWizardDoc()->CheckDate())
	{
		pOSLExportCriteria->m_ToDate = pOSLExportCriteria->m_FromDate;
		GetWizardDoc()->UpdateDataView();
	}
}

//-----------------------------------------------------------------------------
LRESULT CXMLExportOSLCriteriaPage::OnGetBitmapID()
{
	return IDB_WIZARD_SEL_AUDIT_PAGE;
}

//-----------------------------------------------------------------------------
// CXMLExportUserCriteriaPage dialog
//---------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLExportUserCriteriaPage, CExportWizardPage)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXMLExportUserCriteriaPage, CExportWizardPage)
	//{{AFX_MSG_MAP(CXMLExportOSLCriteriaPage)
	ON_WM_SIZE				()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CXMLExportUserCriteriaPage::CXMLExportUserCriteriaPage(CWnd* pParent /*=NULL*/)
	:
	CExportWizardPage	(IDD_WIZARD_SEL_USR_CRITERIA_PAGE, pParent, _T("UserCriteriaPage")),
	m_pInputMng			(NULL)
{
	m_pInputMng = new InputMng(GetDocument());
}

//-----------------------------------------------------------------------------
CXMLExportUserCriteriaPage::~CXMLExportUserCriteriaPage()
{
	if (m_pInputMng)
		delete m_pInputMng;
}

// Standard behaviour to manage message from owned controls
//------------------------------------------------------------------------------
BOOL CXMLExportUserCriteriaPage::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if (nID && m_pInputMng && nCode == EN_VALUE_CHANGED)
	{
		m_pInputMng->DoDynValueChanged(nID);
		return TRUE;
	}

	return CExportWizardPage::OnCommand(wParam, lParam);
}

//------------------------------------------------------------------------------
void CXMLExportUserCriteriaPage::OnSize(UINT nType, int cx, int cy)
{
	m_bEnableHScrolling = m_bEnableVScrolling = FALSE;	// inusabile perchè i control spariscono (Germano)

	__super::OnSize(nType, cx, cy);

	if (m_nHeight < 0 || m_nWidth < 0)
	{
		if (m_pParentTabManager && m_pParentTabManager->IsKindOf(RUNTIME_CLASS(CTabWizard)))
		{
			CUserExportCriteria* pUserExportCriteria = GetWizardDoc()->GetUserExportCriteria();
			if (pUserExportCriteria)
			{
				CSize dlgSize = pUserExportCriteria->ExecAskRules(this, m_pInputMng);

				if (GetWizardDoc()->m_bIsInputMngToInit)
					GetWizardDoc()->m_bIsInputMngToInit = FALSE;

				if (dlgSize == CSize(0, 0))
					return;

				m_nHeight = dlgSize.cy;
				m_nWidth = dlgSize.cx;
			}
		}
	}
}

//-----------------------------------------------------------------------------
LRESULT CXMLExportUserCriteriaPage::OnWizardNext()
{
	CUserExportCriteria* pUserExportCriteria = GetWizardDoc()->GetUserExportCriteria();
	if (pUserExportCriteria)
		pUserExportCriteria->m_bCriteriaInit = FALSE;

	return WIZARD_DEFAULT_TAB;	
}

//-----------------------------------------------------------------------------
LRESULT CXMLExportUserCriteriaPage::OnGetBitmapID()
{
	return IDB_WIZARD_SEL_USR_CRITERIA_PAGE;
}



//-----------------------------------------------------------------------------
// CXMLExportOptionPage dialog
//---------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLExportOptionPage, CExportWizardPage)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXMLExportOptionPage, CExportWizardPage)
	//{{AFX_MSG_MAP(CXMLExportOptionPage)
	ON_EN_VALUE_CHANGED(IDC_EXPORT_USING_PATH_CHECK, OnUsingPathChanged)
	ON_BN_CLICKED(IDC_EXPORT_PATH_BTN, OnSelectExportPath)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-----------------------------------------------------------------------------
CXMLExportOptionPage::CXMLExportOptionPage(CWnd* pParent /*=NULL*/)
	:
	CExportWizardPage(IDD_WIZARD_OPTION_PAGE, pParent, _T("OptionPage"))
{
}

//-----------------------------------------------------------------------------
void CXMLExportOptionPage::OnDisableControlsForBatch()
{
	GetDlgItem(IDC_EXPORT_PATH_BTN)->EnableWindow(m_pXMLExportDocSel->m_bUseAlternativePath);
	m_pXMLExportDocSel->m_strAlternativePath.SetReadOnly(!m_pXMLExportDocSel->m_bUseAlternativePath);
}

//-----------------------------------------------------------------------------
void CXMLExportOptionPage::OnUsingPathChanged()
{
	//if (!m_pXMLExportDocSel->m_bUseAlternativePath)
	//	m_pXMLExportDocSel->m_strAlternativePath.Clear();
	//else
	//	m_pXMLExportDocSel->m_strAlternativePath = AfxGetSiteParams()->f_ExportPath;

	
	GetDlgItem(IDC_EXPORT_PATH_BTN)->EnableWindow(m_pXMLExportDocSel->m_bUseAlternativePath);
	m_pXMLExportDocSel->m_strAlternativePath.SetReadOnly(!m_pXMLExportDocSel->m_bUseAlternativePath);
	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void CXMLExportOptionPage::OnSelectExportPath()
{
	CString strChoosedPath = m_pXMLExportDocSel->m_strAlternativePath;
	CChooseDirDlg dlg(strChoosedPath);
	if (dlg.DoModal() == IDOK)
	{
		strChoosedPath = dlg.GetSelectedDir();
		strChoosedPath.MakeUpper();
		strChoosedPath.Replace (URL_SLASH_CHAR, SLASH_CHAR);			
	}
	
	m_pXMLExportDocSel->m_strAlternativePath = strChoosedPath;
	GetDocument()->UpdateDataView();
}


//-----------------------------------------------------------------------------
void CXMLExportOptionPage::BuildDataControlLinks()
{
	CExpCriteriaWizardDoc*	pWizDoc = GetWizardDoc();
	ASSERT(pWizDoc);

	CAbstractFormDoc* pExportedDocument = pWizDoc ? pWizDoc->GetExportedDocument() : NULL; 	

	//Export option group
	AddLink (IDC_EXPORT_USING_PATH_CHECK, _T("UsePath"), NULL, &m_pXMLExportDocSel->m_bUseAlternativePath, RUNTIME_CLASS(CBoolButton));
	AddLink (IDC_EXPORT_PATH_EDIT, _T("ExportPath"), NULL, &m_pXMLExportDocSel->m_strAlternativePath, RUNTIME_CLASS(CStrEdit));	
	AddLink (IDC_EXPORT_ZIP_CHECK, _T("UseZip"), NULL, &m_pXMLExportDocSel->m_bCompressFile,		 RUNTIME_CLASS(CBoolButton));

    CButton* pBtn = (CBoolButton*)AddLink (IDC_EXPORT_SEND_ENVELOPE_NOW, _T("SendNow"), NULL, &m_pXMLExportDocSel->m_bSendEnvelopeNow, RUNTIME_CLASS(CBoolButton));
	BOOL bShow = TRUE;
	pBtn->ShowWindow(SW_HIDE);

	//export criteria save group
	pBtn = (CBoolButton*)AddLink (IDC_EXPORT_SAVE_EXPCRITERIA, _T("SaveCriteria"), NULL, &pWizDoc->m_bSaveCriteria, RUNTIME_CLASS(CBoolButton));
	int nShow = (m_pXMLExportDocSel->MustUseCriteria() && !pExportedDocument->IsEditingParamsFromExternalController()) ? SW_SHOW : SW_HIDE;
	pBtn->ShowWindow(nShow);
	GetDlgItem(IDC_EXPORT_SAVECRITERIA_GROUP)->ShowWindow(nShow);
	
	OnUsingPathChanged();
}


//-----------------------------------------------------------------------------
// CXMLExportSummaryPage dialog
//---------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLExportSummaryPage, CExportWizardPage)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXMLExportSummaryPage, CExportWizardPage)
	//{{AFX_MSG_MAP(CXMLExportSummaryPage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-----------------------------------------------------------------------------
CXMLExportSummaryPage::CXMLExportSummaryPage(CWnd* pParent /*=NULL*/)
	:
	CExportWizardPage(IDD_WIZARD_SUMMARY_PAGE, pParent, _T("SummaryPage"))
{
}

//-----------------------------------------------------------------------------
void CXMLExportSummaryPage::BuildDataControlLinks()
{
	CExpCriteriaWizardDoc*	pWizDoc = GetWizardDoc();
	ASSERT(pWizDoc);

	CAbstractFormDoc* pExportedDocument = pWizDoc ? pWizDoc->GetExportedDocument() : NULL; 	


	CString strSumText;
	
	strSumText = _TB("The user has selected the following options:");

 	if (m_pXMLExportDocSel->IsOnlySchemaToExport())
	{
		strSumText += _T("\r\n\t ") + _TB("- export only schema files");	
	}
	else
	{
		if (m_pXMLExportDocSel->IsSmartSchemaToExport())
			strSumText += _T("\r\n\t ") + _TB("- export schema files for Magic Documents integration");	
		else
		{	
			strSumText += _T("\r\n\t ");
			switch (m_pXMLExportDocSel->m_nDocSelType)
			{
				case EXPORT_ONLY_CURR_DOC: strSumText += _TB("- export only current document");	 break;
				case EXPORT_DOC_SET:	   strSumText += _TB("- export documents matching export criteria"); break;
				case EXPORT_ALL_DOCS:      strSumText += _TB("- export all documents matching the default query"); break;
			}		

			if (m_pXMLExportDocSel->IsDocAndSchemaToExport())
				strSumText += _T("\r\n\t ") + _TB("- export schema files");	
		}
	}
	
	strSumText += _T("\r\n\t ") + _TB("- use profile:");
	switch (m_pXMLExportDocSel->m_nProfileSelType)
	{
		case USE_PREDEFINED_PROFILE:
			strSumText += _TB("default"); break;			
		case USE_PREFERRED_PROFILE:	
			strSumText += cwsprintf(_TB("preferred {0-%s}"), m_pXMLExportDocSel->m_strPreferredProfile); break;
		case USE_SELECTED_PROFILE:	
			strSumText += m_pXMLExportDocSel->GetProfileName().GetString(); break;
	}

	
	CPreferencesCriteria* pPreferencesCriteria = pWizDoc->GetPreferencesCriteria();

	if (	
			m_pXMLExportDocSel->MustUseCriteria() &&
			pPreferencesCriteria &&
			(
				pPreferencesCriteria->IsCriteriaModeOSL()	||
				pPreferencesCriteria->IsCriteriaModeUser()	||
				pPreferencesCriteria->IsCriteriaModeApp()
			)
	   )
	  
	{
		strSumText += _T("\r\n\t ") + _TB("- use selection criteria");
		
		if (pPreferencesCriteria->IsCriteriaModeApp())
			strSumText += _T("\r\n\t\t ") + _TB("- default");
				
		if (pPreferencesCriteria->IsCriteriaModeUser())
			strSumText += _T("\r\n\t\t ") + _TB("- custom");

		if (pPreferencesCriteria->IsCriteriaModeOSL())
			strSumText += _T("\r\n\t\t ") + _TB("- Auditing Manager based");
	}

	strSumText += _T("\n\n\n\r") + _TB("Press the end button if you want to go on");

	CStatic* pSumEdit = (CStatic*) GetDlgItem(IDC_EXPORT_SUMMARY_EDIT);
	if (pSumEdit)
		pSumEdit->SetWindowText(strSumText);
}

//-----------------------------------------------------------------------------
LRESULT CXMLExportSummaryPage::OnWizardFinish()
{
	CExpCriteriaWizardDoc*	pWizDoc = GetWizardDoc();
	ASSERT_VALID(pWizDoc);
	CAbstractFormDoc* pExportedDocument = pWizDoc ? pWizDoc->GetExportedDocument() : NULL; 	
#ifdef _DEBUG
	if (pWizDoc) ASSERT_VALID(pWizDoc);
	if (pExportedDocument) ASSERT_VALID(pExportedDocument);
	ASSERT_NULL_OR_POINTER(m_pXMLExportDocSel, CXMLExportDocSelection);
#endif
	
	if (m_pXMLExportDocSel->m_strAlternativePath.GetString().CompareNoCase(GetXMLTXTargetSitePath(AfxGetSiteName(), TRUE, FALSE)) == 0)
		m_pXMLExportDocSel->m_strAlternativePath.Clear();
	
	//salvataggio criteri preferenziali
	if (
			pWizDoc && pWizDoc->m_bSaveCriteria && 
			pExportedDocument &&
			m_pXMLExportDocSel && 
			m_pXMLExportDocSel->MustUseCriteria()
		)
	{
		CXMLProfileInfo* pExpProfile = m_pXMLExportDocSel->GetProfileInfo();
		if (pExpProfile)
		{
			CXMLExportCriteria* pExpCriteria = pExpProfile->GetCurrentXMLExportCriteria();
			if (pExpCriteria)
			{
				CString strExpCriteriaFullName;
				if (!m_pXMLExportDocSel->m_strExpCriteriaFileName.IsEmpty() || !pExportedDocument->IsEditingParamsFromExternalController())
				{
					strExpCriteriaFullName = MakeExpCriteriaVarFile
											(
												pExportedDocument->GetNamespace(), 
												pExpProfile->GetName(), 
												m_pXMLExportDocSel->m_strExpCriteriaFileName, 
												CPathFinder::USERS, 
												AfxGetLoginInfos()->m_strUserName
											);
				}
				else
				{

					CString strFileName;					
					// il file con i criteri preferenziali viene salvato in custom dello user del profilo prescelto
					// se la finestra è stata lanciata dallo scheduler in fase di editazione parametri
					//se il nome è vuoto vuol dire che sto associando per la prima volta i criteri di esportazione
					//devo creare un file con un nome univoco per non sovvrascrivere quello già esistente
					int n = 1;
					BOOL bExist = TRUE;
					while (bExist)
					{
						strFileName.Format(_T("TASK_ExpCriteriaVars_%d.xml"), n);
						strExpCriteriaFullName = MakeExpCriteriaVarFile
										(
											pExportedDocument->GetNamespace(), 
											pExpProfile->GetName(), 
											strFileName, 
											CPathFinder::USERS, 
											AfxGetLoginInfos()->m_strUserName
										);
						bExist = ::ExistFile(strExpCriteriaFullName);
						n++;
					}
				}				

				pExpCriteria->UnparseExpCriteriaFile(strExpCriteriaFullName, _T(""), GetWizardDoc()->m_pAutoExpressionMng);
				pExpProfile->m_strExpCriteriaFileName = strExpCriteriaFullName;
			}
		}
	}

	return CExportWizardPage::OnWizardFinish();
}
