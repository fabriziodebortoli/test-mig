#include "stdafx.h"

#include <TbClientCore\ClientObjects.h>
#include <TbParser\Parser.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGes\ExtDocView.h>
#include <TbGes\Hotlink.h>

#include "TBExplorer.h"
#include "TBExplorerUtility.h"

#include <TbGenlibUI\TBExplorer.hjson> //JSON AUTOMATIC UPDATE

//NON spostare, lasciare per ultimo altrimenti non compila
#include <windowsx.h> 
//-------------------
#include "begincpp.dex"

extern int CompareSearchObjDetails(CObject* , CObject* );

//============================================================================
//		CTreeItemLoc implementation
//			per localizzazione
//============================================================================
CTreeItemLoc::CTreeItemLoc(CString strName)
{
	m_strName = strName;
};

//==========================================================================
//							CSaveAdminDialog
//==========================================================================
IMPLEMENT_DYNAMIC(CSaveAdminDialog, CParsedDialog)
//--------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSaveAdminDialog, CParsedDialog)
	ON_BN_CLICKED	(IDC_CK_SAVE_ALLUSR,				OnBtnSelAllUsrsClicked)
	ON_BN_CLICKED	(IDC_CK_SAVE_STD,					OnBtnSelStdClicked)
	ON_BN_CLICKED	(IDC_CK_SAVE_IN_CURRENT_LANGUAGE,	OnBtnSelCurrLangClicked)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------
CSaveAdminDialog::CSaveAdminDialog(UINT nIDD, const CStringArray& aUsrs, CStringArray& aUsrsToSave, CString& sReportName, BOOL bFromSave /*FALSE*/, CWnd* pParent /*=NULL*/)
	: 
	CParsedDialog(nIDD, pParent),
	m_strReportName		(sReportName),
	m_aUsrsSave			(aUsrsToSave),
	m_bFromSave			(bFromSave),
	m_bSaveAllUsers		(FALSE),
	m_bSaveStandard		(FALSE),
	m_bSaveCurrLanguage	(FALSE)
{
	m_aListUsers.Copy(aUsrs);		
//#ifdef DEBUG
//	m_bSaveStandard = TRUE;
//#endif
}

//--------------------------------------------------------------------------
CSaveAdminDialog::CSaveAdminDialog(const CStringArray& aUsrs, CStringArray& aUsrsToSave, CString& sReportName, BOOL bFromSave /*FALSE*/, CWnd* pParent /*=NULL*/)
	:
	CSaveAdminDialog(IDD_EXPLORER_ADMIN_SAVE, aUsrs, aUsrsToSave, sReportName, bFromSave, pParent)
{
}

//--------------------------------------------------------------------------
CSaveAdminDialog::~CSaveAdminDialog()
{}

//--------------------------------------------------------------------------
void CSaveAdminDialog::DoDataExchange(CDataExchange* pDX)
{
	__super::DoDataExchange(pDX);
}

//--------------------------------------------------------------------------
BOOL CSaveAdminDialog::OnInitDialog()
{
	__super::OnInitDialog();
	VERIFY(m_ListUserSave		.SubclassDlgItem (IDC_LISTADMIN_USR_SAVE,	this));

	if (!m_bFromSave)
	{	
        ((CButton*)GetDlgItem(IDC_CK_SAVE_ALLUSR))->ShowWindow(SW_HIDE);
		((CButton*)GetDlgItem(IDC_CK_SAVE_STD))->ShowWindow(SW_HIDE);
		((CButton*)GetDlgItem(IDC_CK_SAVE_IN_CURRENT_LANGUAGE))->ShowWindow(SW_HIDE);
	}

	if (!AfxGetBaseApp()->IsDevelopment())
		((CButton*)GetDlgItem(IDC_CK_SAVE_STD))->ShowWindow(SW_HIDE);
		

	FillUserList();
	// imposto come selezione l'utente (amministratore) attualmente loginato
	int n = m_ListUserSave.FindStringExact(-1, (LPCTSTR)AfxGetLoginInfos()->m_strUserName);	
	m_ListUserSave.SetCurSel(n);

	return TRUE;
}

//--------------------------------------------------------------------------
void CSaveAdminDialog::OnBtnSelAllUsrsClicked()
{
	if (((CButton*)GetDlgItem(IDC_CK_SAVE_ALLUSR))->GetCheck())
	{
		((CButton*)GetDlgItem(IDC_CK_SAVE_ALLUSR))->SetCheck(TRUE);
		((CButton*)GetDlgItem(IDC_CK_SAVE_STD))->SetCheck(FALSE);
		m_bSaveAllUsers = TRUE;
		m_bSaveStandard = FALSE;
		m_ListUserSave.EnableWindow(FALSE);	
		return;
	}
	
	((CButton*)GetDlgItem(IDC_CK_SAVE_ALLUSR))->SetCheck(FALSE);
	m_bSaveAllUsers = FALSE;
	m_ListUserSave.EnableWindow(TRUE);			
}

//--------------------------------------------------------------------------
void CSaveAdminDialog::OnBtnSelStdClicked()
{
	if (((CButton*)GetDlgItem(IDC_CK_SAVE_STD))->GetCheck())
	{
		((CButton*)GetDlgItem(IDC_CK_SAVE_STD))->SetCheck(TRUE);
		((CButton*)GetDlgItem(IDC_CK_SAVE_ALLUSR))->SetCheck(FALSE);
		m_bSaveAllUsers = FALSE;
		m_bSaveStandard = TRUE;
		m_ListUserSave.EnableWindow(FALSE);	
		return;
	}
	
	((CButton*)GetDlgItem(IDC_CK_SAVE_STD))->SetCheck(FALSE);
	m_bSaveStandard = FALSE;
	m_ListUserSave.EnableWindow(TRUE);			
}

//--------------------------------------------------------------------------
void CSaveAdminDialog::OnBtnSelCurrLangClicked()
{
	m_bSaveCurrLanguage = TRUE;
}

//--------------------------------------------------------------------------
BOOL CSaveAdminDialog::FillUserList()
{
	int nIdx;
	for (int i = 0; i <= m_aListUsers.GetUpperBound(); i++)
	{
		CString strUsers = m_aListUsers.GetAt(i);
		if (!strUsers.IsEmpty())
		{
			nIdx = m_ListUserSave.AddString(strUsers);
			m_ListUserSave.SetItemData(nIdx, (DWORD) i);
		}				
	}	

	int nSel = m_ListUserSave.FindStringExact(-1, AfxGetLoginInfos()->m_strUserName);
	m_ListUserSave.SetSel(nSel);	
	return TRUE;
}

//--------------------------------------------------------------------------
void CSaveAdminDialog::OnOK()
{
	// controllo se ho selezionato almeno uno user
	int size = m_ListUserSave.GetCount();
	int nCount = 0;
	CString strSaveUsr = _T("");
	
	if (((CButton*)GetDlgItem(IDC_CK_SAVE_ALLUSR))->GetCheck() || ((CButton*)GetDlgItem(IDC_CK_SAVE_STD))->GetCheck())
		m_aUsrsSave.RemoveAll();
	else
	{
		for (int i = size - 1 ; i >= 0 ; i--)
		{
			if (m_ListUserSave.GetSel(i) > 0)
			{
				m_ListUserSave.GetText(i, strSaveUsr);
				m_aUsrsSave.Add(strSaveUsr);
				nCount++;
			}
		}	

		if (!nCount)
			AfxMessageBox(_TB("Warning, no user is selected!"));
	}
	
	__super::OnOK();
}

//==========================================================================
//							CFullSaveAdminDialog
//==========================================================================
IMPLEMENT_DYNAMIC(CFullSaveAdminDialog, CSaveAdminDialog)
//--------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFullSaveAdminDialog, CSaveAdminDialog)
	//	ON_EN_CHANGE		(IDC_EDIT_OBJ,			OnChanged)
	//	ON_CBN_SELCHANGE	(IDC_COMBO_MODS,		OnChanged)
	ON_EN_VALUE_CHANGED(IDC_FULL_APPS, OnApplicationChanged)
	ON_EN_VALUE_CHANGED(IDC_FULL_MODS, OnModuleChanged)
	ON_EN_VALUE_CHANGED(IDC_FULL_NAME, OnNameChanged)

END_MESSAGE_MAP()

//--------------------------------------------------------------------------
CFullSaveAdminDialog::CFullSaveAdminDialog(const CTBNamespace& ns, CWnd* pParent /*=NULL*/)
	:
	m_Namespace (ns),
	m_sNamespace(ns.ToString()),
	CSaveAdminDialog(IDD_FULL_ADMIN_SAVE, AfxGetLoginInfos()->m_CompanyUsers, m_arUsersToSave, m_sNamespace, TRUE, pParent)
{
	//m_arUsers.Copy(AfxGetLoginInfos()->m_CompanyUsers);
}

//--------------------------------------------------------------------------
BOOL CFullSaveAdminDialog::OnInitDialog()
{
	__super::OnInitDialog();

	//if (m_bFromSave)
	//	((CButton*)GetDlgItem(IDC_CK_SAVE_STD))->ShowWindow(SW_SHOW);

	BOOL bOk = ::SubclassParsedControl(this, IDC_FULL_NAME, &m_edtName,	&m_dsName, _T("Name"));

	bOk = ::SubclassParsedControl(this, IDC_FULL_APPS, &m_cbxApps,	&m_dsApplication, _T("Application"), _NS_HKL("Framework.TbGes.TbGes.Applications"), NO_BUTTON);

	bOk = ::SubclassParsedControl(this, IDC_FULL_MODS, &m_cbxMods,	&m_dsModule, _T("Module"), _NS_HKL("Framework.TbGes.TbGes.Modules"), NO_BUTTON);

	if (m_Namespace.IsValid())
	{
		m_dsApplication = m_Namespace.GetApplicationName();
		m_cbxApps.FillListBox();
		m_cbxApps.SelectString(-1, m_dsApplication.GetString());

		m_dsModule = m_Namespace.GetModuleName();
		m_cbxMods.FillListBox();
		m_cbxMods.SelectString(-1, m_dsModule.GetString());

		m_dsName = m_Namespace.GetObjectName();
		m_edtName.UpdateCtrlView();
	}

	return TRUE;
}

//--------------------------------------------------------------------------
int CFullSaveAdminDialog::GetSavePath(CStringArray& ar)
{
	ar.Copy(this->m_aUsrsSave);
	return ar.GetCount();
}

//--------------------------------------------------------------------------
void CFullSaveAdminDialog::OnNameChanged()
{
	
}

void CFullSaveAdminDialog::OnApplicationChanged()
{
	HKLModules* pHKL = dynamic_cast<HKLModules*>(m_cbxMods.GetHotLink());
	if (!pHKL)
		return;
	pHKL->SetApplication(m_dsApplication);

	m_dsModule.Clear();
	m_cbxMods.ResetAssociations(TRUE);
}

void CFullSaveAdminDialog::OnModuleChanged()
{
}

//==========================================================================
//							CAskCopyLinkDialog
//==========================================================================
IMPLEMENT_DYNAMIC(CAskCopyLinkDialog, CParsedDialog)
//--------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAskCopyLinkDialog, CParsedDialog)
	ON_BN_CLICKED	(IDC_ASKCOPYLNK_RD_COPY, OnRdCopyClicked)			
	ON_BN_CLICKED	(IDC_ASKCOPYLNK_RD_LINK, OnRdLinkClicked)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------
CAskCopyLinkDialog::CAskCopyLinkDialog(CWnd* pParent /*=NULL*/)
	: 
	CParsedDialog(IDD_ASKCOPYLNK_DLG, pParent),
	m_bSaveLink			(FALSE)
{		
}

//--------------------------------------------------------------------------
CAskCopyLinkDialog::~CAskCopyLinkDialog()
{}

//--------------------------------------------------------------------------
void CAskCopyLinkDialog::DoDataExchange(CDataExchange* pDX)
{
	CParsedDialog::DoDataExchange(pDX);
}

//--------------------------------------------------------------------------
BOOL CAskCopyLinkDialog::OnInitDialog()
{
	__super::OnInitDialog();

	VERIFY (m_rd_SaveLnk.SubclassDlgItem(IDC_ASKCOPYLNK_RD_LINK, this));
	VERIFY (m_rd_CopyObj.SubclassDlgItem(IDC_ASKCOPYLNK_RD_COPY, this));
	
	m_rd_CopyObj.SetCheck(TRUE);
	//m_rd_SaveLnk.SetCheck(FALSE);
	return TRUE;
}

//--------------------------------------------------------------------------
void CAskCopyLinkDialog::OnOK()
{
	CParsedDialog::OnOK();
}

//--------------------------------------------------------------------------
void CAskCopyLinkDialog::OnRdCopyClicked()
{
	if (m_rd_CopyObj.GetCheck())
	{
		m_bSaveLink = TRUE;
		return;
	}
	m_rd_CopyObj.SetCheck(TRUE);
	m_bSaveLink = FALSE;
}

//--------------------------------------------------------------------------
void CAskCopyLinkDialog::OnRdLinkClicked()
{
	if (m_rd_SaveLnk.GetCheck())
	{
		m_bSaveLink = TRUE;
		return;
	}		
}
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
BOOL GetReportTitle (const CString& sReportPath, CString& sTitle, CString& sSubject)
{
	sTitle.Empty(); sSubject.Empty();

	//Non è possibile recuperare il titolo nei file .RDE perchè sono binari
	if (sReportPath.GetLength() > 4 && sReportPath.Right(4).CompareNoCase(_T(".rde")) == 0)
	{
		return FALSE;
	}

	Parser lex;
	if (!lex.Open(sReportPath))
		return FALSE;

	Token arT1[] =  { T_PROPERTIES, T_PAGE_LAYOUT };
	Token arT2[] =  { T_TITLE, T_SUBJECT, T_END };

	if (!lex.SkipToToken(arT1, sizeof(arT1)/sizeof(Token)))
		return FALSE;

	if (lex.LookAhead(T_PAGE_LAYOUT))
		return FALSE;

	BOOL bFinded = FALSE;
	// no syntax check here: simply if you find the Title property, just store it!
	if (lex.Matched(T_PROPERTIES) && lex.Matched(T_BEGIN))
	{
		if (lex.SkipToToken(arT2, sizeof(arT2)/sizeof(Token)))
		{
			if (lex.Matched(T_END))
				return FALSE;

			if (lex.Matched(T_TITLE))
			{
				lex.ParseString(sTitle);
				bFinded = TRUE;
			}
			if (lex.Matched(T_SUBJECT))
				lex.ParseCEdit(sSubject);

			if (lex.SkipToToken(arT2, sizeof(arT2)/sizeof(Token)))
			{
				if (lex.Matched(T_TITLE))
				{
					lex.ParseString(sTitle);
					bFinded = TRUE;
				}
				if (lex.Matched(T_SUBJECT))
					lex.ParseCEdit(sSubject);
			}
		}
	}
	if (!sTitle.IsEmpty())
	{
		sTitle = AfxLoadReportString(sTitle, sReportPath);
	}
	if (!sSubject.IsEmpty())
	{
		sSubject = AfxLoadReportString(sSubject, sReportPath);
	}
	return bFinded;
}

