#include "stdafx.h"

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TBGenlib\Baseapp.h>
#include <TBGenlib\ParsCtrl.h>
#include <TBGenlib\SettingsTableManager.h>

#include "CustomSaveDialog.h"

// risorse
#include "CustomSaveDialog.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


//============================================================================
//  Dialog di salvataggio customizzazioni
//============================================================================
//--------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CCustomSaveDialog, CParsedDialogWithTiles)

//--------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCustomSaveDialog, CParsedDialogWithTiles)
	//{{AFX_MSG_MAP(CCustomSaveDialog
	ON_EN_VALUE_CHANGED	(IDC_CUSTOMSAVEDLG_STANDARD,	OnModeChanged)
    ON_EN_VALUE_CHANGED	(IDC_CUSTOMSAVEDLG_CURRUSER,	OnModeChanged)
    ON_EN_VALUE_CHANGED	(IDC_CUSTOMSAVEDLG_ALLUSERS,	OnModeChanged)
    ON_EN_VALUE_CHANGED	(IDC_CUSTOMSAVEDLG_CHOOSEUSER,	OnModeChanged)
    ON_EN_VALUE_CHANGED	(IDC_CUSTOMSAVEDLG_CURRCOMPANY,	OnModeChanged)
    ON_EN_VALUE_CHANGED	(IDC_CUSTOMSAVEDLG_ALLCOMPANIES,OnModeChanged)
	ON_EN_VALUE_CHANGED	(IDC_CUSTOMSAVEDLG_USERSLBX,	OnModeChanged)
	ON_WM_CTLCOLOR()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CCustomSaveDialog::CCustomSaveDialog ()
	:
	CParsedDialogWithTiles(IDD_CUSTOMSAVEDLG_SAVE),
	m_bAllCompaniesEnabled 	(FALSE),
	m_pSelections			(NULL)
{
	m_MapCtr.RemoveAll();
}

//-----------------------------------------------------------------------------
CCustomSaveDialog::CCustomSaveDialog(CCustomSaveInterface* pInerface, CWnd* pParent)
	:
	CCustomSaveDialog()
{
	SetInterface(pInerface, pParent);
}

//-----------------------------------------------------------------------------
void CCustomSaveDialog::SetInterface(CCustomSaveInterface* pInterface, CWnd* pParent)
{
	m_pSelections = pInterface;
	m_pParentWnd = pParent;
}

//-----------------------------------------------------------------------------
int CCustomSaveDialog::ShowDialog()
{
	return DoModal();
}

//------------------------------------------------------------------------------
void CCustomSaveDialog::EnableAllCompanies (const BOOL& bValue)
{
	m_bAllCompaniesEnabled = bValue;

	if (m_bAllCompanies.m_hWnd)
		m_bAllCompanies.ShowWindow(m_bAllCompaniesEnabled ? SW_SHOW : SW_HIDE);
}

//------------------------------------------------------------------------------
BOOL CCustomSaveDialog::OnInitDialog()
{
	__super::OnInitDialog();

	m_bStandard.		SubclassDlgItem(IDC_CUSTOMSAVEDLG_STANDARD,		this);
	m_bCurrentUser.		SubclassDlgItem(IDC_CUSTOMSAVEDLG_CURRUSER,		this);
	m_bAllUsers.		SubclassDlgItem(IDC_CUSTOMSAVEDLG_ALLUSERS,		this);
	m_bSelUsers.		SubclassDlgItem(IDC_CUSTOMSAVEDLG_CHOOSEUSER,	this);
	m_lbxUsers.			SubclassDlgItem(IDC_CUSTOMSAVEDLG_USERSLBX,		this);
	m_bCurrentCompany.	SubclassDlgItem(IDC_CUSTOMSAVEDLG_CURRCOMPANY,	this);
	m_bAllCompanies.	SubclassDlgItem(IDC_CUSTOMSAVEDLG_ALLCOMPANIES,	this);

	// gli standard sono abilitati solo in sviluppo
	if (!AfxGetBaseApp()->IsDevelopment())
		m_bStandard.ShowWindow	(SW_HIDE);

	// amministratore
	if (!AfxGetLoginInfos()->m_bAdmin)
	{	
		m_bStandard.		ShowWindow	(SW_HIDE);
		m_bAllCompanies.	ShowWindow	(SW_HIDE);
		m_bSelUsers.		ShowWindow	(SW_HIDE);
		m_bAllUsers.		ShowWindow	(SW_HIDE);
		m_lbxUsers.			EnableWindow(FALSE);
	}

	// se la gestione dell'AllCompanies non è abilitata, non la visualizzo
	if (!m_bAllCompaniesEnabled)
		m_bAllCompanies.ShowWindow	(SW_HIDE);

	InitDefaults(); 

	m_bStandard.		UpdateCtrlView();
	m_bCurrentUser.		UpdateCtrlView();
	m_bAllUsers.		UpdateCtrlView();
	m_bSelUsers.		UpdateCtrlView();
	m_bCurrentCompany.	UpdateCtrlView();
	m_bAllCompanies.	UpdateCtrlView();

	SetToolbarStyle(CParsedDialog::BOTTOM, DEFAULT_TOOLBAR_HEIGHT, TRUE, TRUE);

	CParsedForm::SetBackgroundColor(AfxGetThemeManager()->GetTileGroupBkgColor());

	CenterWindow(); 
	return TRUE;
}

//-----------------------------------------------------------------------------
void CCustomSaveDialog::InitDefaults ()
{
	if (!m_pSelections)
		return;

	m_bStandard.		SetCheck (m_pSelections->m_eSaveMode == CCustomSaveInterface::STANDARD);
	m_bCurrentUser.		SetCheck (TRUE);
	m_bAllUsers.		SetCheck (FALSE);
	m_bSelUsers.		SetCheck (FALSE);
	m_bCurrentCompany.	SetCheck (TRUE);
	m_bAllCompanies.	SetCheck (FALSE);

	FillUsers ();

	m_lbxUsers.EnableWindow(m_bSelUsers.GetCheck());
	
	CString sWhoIs = cwsprintf
					(	_TB("Connection data: user {0-%s} company {1-%s}"), 
						(LPCTSTR) AfxGetLoginInfos()->m_strUserName, 
						(LPCTSTR) AfxGetLoginInfos()->m_strCompanyName
					);
	CWnd* pWnd = GetDlgItem(IDC_CUSTOMSAVEDLG_WHOIS);
	if (pWnd)
		pWnd->SetWindowText (sWhoIs);	
}

//-----------------------------------------------------------------------------
void CCustomSaveDialog::FillUsers ()
{
	CStringArray arUsers;
	arUsers.Copy(AfxGetLoginInfos()->m_CompanyUsers);

	if (!arUsers.GetSize())
		return;

	AfxGetLoginManager()->GetCompanyUsersWithoutEasyLookSystem(arUsers);

	// se non sono l'amministratore carico solo il mio utente
	// per evidenziare che i salvataggi saranno solo per me.
	if (!AfxGetLoginInfos()->m_bAdmin)
	{
		m_lbxUsers.AddString(AfxGetLoginInfos()->m_strUserName);
		return;
	}

	// se sono l'Admin aggiungo tutti gli utenti possibili
	for (int i=0; i <= arUsers.GetUpperBound(); i++)
		m_lbxUsers.AddString(arUsers.GetAt(i));
}

//-----------------------------------------------------------------------------
void CCustomSaveDialog::OnModeChanged ()
{
	if (m_bStandard.GetCheck())
	{
		m_bCurrentUser.		SetCheck(FALSE);		
		m_bAllUsers.		SetCheck(TRUE);
		m_bSelUsers.		SetCheck(FALSE);
	}

	m_lbxUsers.EnableWindow(m_bSelUsers.GetCheck());
}

//-----------------------------------------------------------------------------
HBRUSH CCustomSaveDialog::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	UINT nStyle = (UINT)(pWnd->GetStyle() & 0x0F);
	if (nStyle == BS_GROUPBOX)
	{
		// Diasble the windows theme		
		BOOL bRet;
		HWND hWndCtl = pWnd->GetSafeHwnd();

		if (!m_MapCtr.Lookup(hWndCtl, bRet))
		{
			// Creare una lista, di pWnd per disattivare il tema di windows delle GROUPBOX!
			::SetWindowTheme(hWndCtl, _T(""), _T(""));
			m_MapCtr.SetAt(hWndCtl, TRUE);
		}
		pDC->SetTextColor(AfxGetThemeManager()->GetTileTitleSeparatorColor());
	}
	return __super::OnCtlColor(pDC, pWnd, nCtlColor);
}

//-----------------------------------------------------------------------------
BOOL CCustomSaveDialog::CheckSelections	()
{
	if (!m_bSelUsers.GetCheck())
		return TRUE;

	BOOL bOneSelection = FALSE;
	for (int i = 0; i < m_lbxUsers.GetCount(); i++)
		if (m_lbxUsers.GetSel(i) > 0)
		{
			bOneSelection = TRUE;
			break;
		}

	if (!bOneSelection)
		AfxMessageBox (_TB("Select a user or the \"Current User\" item to save settings") );

	return bOneSelection;
}

//-----------------------------------------------------------------------------
void CCustomSaveDialog::OnOK ()
{
	if (!m_pSelections || !CheckSelections ())
		return;

	// comunico le selezioni all'interfaccia
	if (m_bStandard.GetCheck())
		m_pSelections->m_eSaveMode = CCustomSaveInterface::STANDARD;
	else if (m_bAllCompanies.GetCheck())
		m_pSelections->m_eSaveMode = CCustomSaveInterface::ALLCOMPANY_USERS;
	else 
		m_pSelections->m_eSaveMode = CCustomSaveInterface::COMPANY_USERS;

	// devo gestire solo il mio utente
	m_pSelections->m_aUsers.RemoveAll();

	CString sUser;

	// gestione utenti
	if (m_bCurrentUser.GetCheck())
		m_pSelections->m_aUsers.Add(AfxGetLoginInfos()->m_strUserName);
	else if (m_bAllUsers.GetCheck())
	{
		// aggiungo un utente per indicare almeno un salvataggio da fare
		m_pSelections->m_bSaveAllUsers = TRUE;
		m_pSelections->m_aUsers.Add(_T(""));
	}
	else if (m_bSelUsers.GetCheck())
		for (int i = 0;i < m_lbxUsers.GetCount(); i++)
			if (m_lbxUsers.GetSel(i) > 0)
			{
				m_lbxUsers.GetText(i, sUser);
				m_pSelections->m_aUsers.Add(sUser);
			}

	__super::OnOK();
}
