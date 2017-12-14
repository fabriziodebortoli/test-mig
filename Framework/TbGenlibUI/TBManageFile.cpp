#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include "TBExplorer.h"
#include "TBManageFile.h"
#include "TBManageFile.hjson" //JSON AUTOMATIC UPDATE

#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CManageFileWizardPage property page implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CManageFileWizardPage, CWizardPage)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManageFileWizardPage, CWizardPage)
	//{{AFX_MSG_MAP(CManageFileWizardPage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CManageFileWizardPage::CManageFileWizardPage(UINT nIDTemplate, CWnd* pParent /*=NULL*/) 
:
	m_pManageFileSheet	(NULL),
	m_Image				( TBGlyph(_T("wiz_pres"))),
	CWizardPage			(nIDTemplate, pParent)
{
	m_pToolTip = new CToolTipCtrl;
}

//--------------------------------------------------------------------
CManageFileWizardPage::~CManageFileWizardPage()
{
	if(m_pToolTip)
	{
		delete m_pToolTip;
		m_pToolTip = NULL;
	}
}

//--------------------------------------------------------------------
BOOL CManageFileWizardPage::OnInitDialog()
{
	CWizardPage::OnInitDialog();

	m_pManageFileSheet = (CManageFileWizMasterDlg*) GetParent();
	ASSERT_KINDOF(CManageFileWizMasterDlg, m_pManageFileSheet);
	
	if(!m_pToolTip->Create(this))
		ASSERT(FALSE);

	m_Image.SubclassDlgItem(IDC_IMAGE, this);
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CManageFileWizardPage::OnCreatePage()
{
	if (!m_pParent || !m_pParent->IsKindOf(RUNTIME_CLASS(CManageFileWizMasterDlg)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CManageFileWizardPage::PreTranslateMessage(MSG* pMsg) 
{
	if(m_pToolTip != NULL)
		m_pToolTip->RelayEvent(pMsg);
	
	return __super::PreTranslateMessage(pMsg);
}

/////////////////////////////////////////////////////////////////////////////
// CManageWizPresentationPage property page implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CManageWizPresentationPage, CManageFileWizardPage)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManageWizPresentationPage, CManageFileWizardPage)
	//{{AFX_MSG_MAP(CManageWizPresentationPage)	
	ON_BN_CLICKED		(IDC_MNGDLG_WIZ_RD_INT,	OnSelInt)				
	ON_BN_CLICKED		(IDC_MNGDLG_WIZ_RD_EXT,	OnSelExt)	
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CManageWizPresentationPage::CManageWizPresentationPage() 
	: 
	CManageFileWizardPage(IDD_MANAGEFILE_WIZ_PRESENTATION)
{
}

//-----------------------------------------------------------------------------
void CManageWizPresentationPage::DoDataExchange(CDataExchange* pDX)
{
	CWizardPage::DoDataExchange(pDX);
}

//--------------------------------------------------------------------
BOOL CManageWizPresentationPage::OnInitDialog()
{
	((CButton*) GetDlgItem(IDC_MNGDLG_WIZ_RD_EXT))->SetCheck(TRUE);
	return CManageFileWizardPage::OnInitDialog();
}

//--------------------------------------------------------------------
void CManageWizPresentationPage::OnSelInt()		   
{										   
	m_pManageFileSheet->m_bExternalCopy = FALSE;
	m_pManageFileSheet->m_aPaths.RemoveAll();
	m_pManageFileSheet->m_aUsrForSave.RemoveAll();
	m_pManageFileSheet->m_bAllUsers = FALSE;
	m_pManageFileSheet->m_Namespace.Clear();
}

//--------------------------------------------------------------------
void CManageWizPresentationPage::OnSelExt()
{
	m_pManageFileSheet->m_bExternalCopy = TRUE;
	m_pManageFileSheet->m_aPaths.RemoveAll();
	m_pManageFileSheet->m_aUsrForSave.RemoveAll();
	m_pManageFileSheet->m_bAllUsers = FALSE;
	m_pManageFileSheet->m_Namespace.Clear();
}

//--------------------------------------------------------------------
void CManageWizPresentationPage::OnActivate()
{	
	m_pManageFileSheet->SetWindowText(_TB("Wizard Object Copy"));
	CManageFileWizardPage::OnActivate();
}

//--------------------------------------------------------------------
LRESULT CManageWizPresentationPage::OnWizardNext()
{
	return IDD_MANAGEFILE_WIZ_SEARCH;
}

/////////////////////////////////////////////////////////////////////////////
// CManageWizSearchPage dialog implementation
/////////////////////////////////////////////////////////////////////////////
//
BEGIN_MESSAGE_MAP(CManageWizSearchPage, CManageFileWizardPage)
	//{{AFX_MSG_MAP(CManageWizSearchPage)
	ON_BN_CLICKED		(IDC_MNGFILEDLG_BTNSEL,	OnBtnSelClicked)
	ON_CBN_SELCHANGE	(IDC_MNGFILEDLG_CMBOBJ,	OnComboObjChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//----------------------------------------------------------------------------
CManageWizSearchPage::CManageWizSearchPage()
	: 
	CManageFileWizardPage		(IDD_MANAGEFILE_WIZ_SEARCH)
{
}

//----------------------------------------------------------------------------
CManageWizSearchPage::~CManageWizSearchPage()
{
} 

//--------------------------------------------------------------------------
void CManageWizSearchPage::DoDataExchange(CDataExchange* pDX)
{
	CLocalizableDialog::DoDataExchange(pDX);
}

//----------------------------------------------------------------------------
BOOL CManageWizSearchPage::OnInitDialog() 
{
	BOOL bInit = CManageFileWizardPage::OnInitDialog();

	VERIFY (m_ComboObj.SubclassDlgItem		(IDC_MNGFILEDLG_CMBOBJ,	this));
	VERIFY (m_ListSelected.SubclassDlgItem	(IDC_MNGFILEDLG_LIST,	this));

	FillComboObj();
	((CButton*)GetDlgItem(IDC_MNGFILEDLG_BTNSEL))->EnableWindow(FALSE);	
	
	return bInit; 
}

//--------------------------------------------------------------------------
void CManageWizSearchPage::FillComboObj()
{
	int nPosNothing = m_ComboObj.AddString(_TB("<No selection>"));
	m_ComboObj.SetItemData(nPosNothing, CTBNamespace::NOT_VALID);

	int nPosReport = m_ComboObj.AddString(_TB("Report"));
	m_ComboObj.SetItemData(nPosReport, CTBNamespace::REPORT);

	int nPosImage = m_ComboObj.AddString(_TB("Image"));
	m_ComboObj.SetItemData(nPosImage, CTBNamespace::IMAGE);

	int nPosText = m_ComboObj.AddString(_TB("Text"));
	m_ComboObj.SetItemData(nPosText, CTBNamespace::TEXT);

	int nPosPdf = m_ComboObj.AddString(_TB("Pdf"));
	m_ComboObj.SetItemData(nPosPdf, CTBNamespace::PDF);

	int nPosRtf = m_ComboObj.AddString(_TB("Rtf"));
	m_ComboObj.SetItemData(nPosRtf, CTBNamespace::RTF);

	//int nPosOdf = m_ComboObj.AddString(_TB("Odf"));
	//m_ComboObj.SetItemData(nPosOdf, CTBNamespace::ODF);

	m_ComboObj.SetCurSel(nPosNothing);
}

//--------------------------------------------------------------------------
void CManageWizSearchPage::FillList()
{
	m_ListSelected.ResetContent();
	if (m_pManageFileSheet->m_aPaths.IsEmpty())
		return;
	
	for (int i = 0; i <= m_pManageFileSheet->m_aPaths.GetUpperBound(); i++)
	{	
		CString strPath = (CString) m_pManageFileSheet->m_aPaths.GetAt(i);
		m_ListSelected.AddString(strPath);
	}

	CString     str;
	CSize		sz;
	int			dx = 0;
	TEXTMETRIC  tm;
	CDC*		pDC		= m_ListSelected.GetDC();
	CFont*		pFont	= m_ListSelected.GetFont();

	// Select the listbox font, save the old font
	CFont* pOldFont = pDC->SelectObject(pFont);
	// Get the text metrics for avg char width
	pDC->GetTextMetrics(&tm); 

	for (int n = 0; n < m_ListSelected.GetCount(); n++)
	{
		m_ListSelected.GetText(n, str);
		sz = pDC->GetTextExtent(str);

	// Add the avg width to prevent clipping
		sz.cx += tm.tmAveCharWidth;

		if (sz.cx > dx)
			dx = sz.cx;
	}
	// Select the old font back into the DC
	pDC->SelectObject(pOldFont);
	m_ListSelected.ReleaseDC(pDC);

	// Set the horizontal extent so every character of all strings 
	// can be scrolled to.
	m_ListSelected.SetHorizontalExtent(dx);
}

//--------------------------------------------------------------------------
void CManageWizSearchPage::OnBtnSelClicked()
{
	m_pManageFileSheet->m_aPaths.RemoveAll();
	
	//devo sapere le ext da cercare
	int nCurSel = m_ComboObj.GetCurSel();
	if (!nCurSel)
		return; 

	if (m_pManageFileSheet->m_bExternalCopy)
		SelectExternObj();
	else
		SelectInternObj();	
}

//--------------------------------------------------------------------------
void CManageWizSearchPage::SelectExternObj()
{
	CString strExt;
	CString	strAny;
	CString	strFilter;
	CString	strFilterFirst;

	switch (m_pManageFileSheet->m_Type)
	{
		case CTBNamespace::REPORT:
			strExt		= FileExtension::WRM_EXT();
			strAny		= FileExtension::ANY_WRMRDE();
			strFilter	= FileExtension::WRMRDE_FILTER();
			break;
		case CTBNamespace::IMAGE:
			strExt		= FileExtension::BMP_EXT();
			strAny		= FileExtension::ANY_BMP();
			strFilter	= FileExtension::BMP_FILTER();
				
			break;
		case CTBNamespace::TEXT:
			strExt		= FileExtension::CSV_EXT();
			strAny		= FileExtension::ANY_TXT();
			strFilter	= FileExtension::TXT_FILTER();
			break;

		case CTBNamespace::PDF:
			strExt		= FileExtension::PDF_EXT();
			strAny		= FileExtension::ANY_PDF();
			strFilter	= FileExtension::PDF_FILTER();
			break;

		case CTBNamespace::RTF:
			strExt		= FileExtension::RTF_EXT();
			strAny		= FileExtension::ANY_RTF();
			strFilter	= FileExtension::RTF_FILTER();
			break;

		case CTBNamespace::ODT:
			strExt		= FileExtension::ODT_EXT();
			strAny		= FileExtension::ANY_ODT();
			strFilter	= FileExtension::ODT_FILTER();
			break;
		case CTBNamespace::ODS:
			strExt		= FileExtension::ODS_EXT();
			strAny		= FileExtension::ANY_ODS();
			strFilter	= FileExtension::ODS_FILTER();
			break;

	}

	CFileDialog dlg(TRUE, strExt, strAny, OFN_ALLOWMULTISELECT|OFN_HIDEREADONLY|OFN_NOCHANGEDIR, strFilter);
	//declare my own buffer
	CString strFileName;
	//set a limit -in character-
	int maxChar = 1000;
	//give dialog a hint
	dlg.m_ofn.lpstrFile = strFileName.GetBuffer(maxChar);
	dlg.m_ofn.nMaxFile = maxChar;
	//show dialog
	if(dlg.DoModal()  == IDOK)
	{
		strFileName.ReleaseBuffer();//release the buffer, if you will do some CString-functions
		POSITION pos = dlg.GetStartPosition();
		
		CString sSelectedFileName;
		CStringArray arInvalids;
		while (pos != NULL)
		{
			sSelectedFileName = dlg.GetNextPathName(pos);
			if (GetName(sSelectedFileName).FindOneOf(CTBNamespace::GetNotSupportedChars()) > 0)
				arInvalids.Add(sSelectedFileName);
			else
				m_pManageFileSheet->m_aPaths.Add(sSelectedFileName);
		}

		if (arInvalids.GetSize())
		{
			CString sInvalids;
			for (int i=0; i <= arInvalids.GetUpperBound(); i++)
				sInvalids += _T("\n") + arInvalids.GetAt(i);
			
			AfxMessageBox (cwsprintf(_TB("The following objects have not allowed special characters into the name!%s\nThese files have been excluded by selection!"), sInvalids));
		}		
		FillList();
		if (m_pManageFileSheet->m_aPaths.GetSize())
			m_pManageFileSheet->EnableNextBtn(TRUE);
		else
			m_pManageFileSheet->EnableNextBtn(FALSE);
	}
	else
	{
		strFileName.ReleaseBuffer();//Release anyway	
		m_pManageFileSheet->m_aPaths.RemoveAll();
		FillList();
		m_pManageFileSheet->EnableNextBtn(FALSE);
	}
}

//--------------------------------------------------------------------------
void CManageWizSearchPage::SelectInternObj()
{
	CTBNamespace ns; 
	ns.SetApplicationName(_T(""));
	ns.SetObjectName(CTBNamespace::MODULE, _T(""));
	ns.SetType(m_pManageFileSheet->m_Type);
	CTBExplorer dlg(CTBExplorer::OPEN, ns); 
	dlg.SetMultiOpen();
	dlg.Open();
	dlg.GetSelPathElements(&m_pManageFileSheet->m_aPaths);
	if (m_pManageFileSheet->m_aPaths.GetSize())
		m_pManageFileSheet->EnableNextBtn(TRUE);
	else
		m_pManageFileSheet->EnableNextBtn(FALSE);
	FillList();
}

//--------------------------------------------------------------------------
void CManageWizSearchPage::OnComboObjChanged()
{
	int nPos = m_ComboObj.GetCurSel();
	CTBNamespace::NSObjectType Type = (CTBNamespace::NSObjectType) m_ComboObj.GetItemData(nPos);

	if (Type == CTBNamespace::NOT_VALID)
	{
		((CButton*)GetDlgItem(IDC_MNGFILEDLG_BTNSEL))->EnableWindow(FALSE);	
		m_pManageFileSheet->m_Type = CTBNamespace::NOT_VALID;
		m_pManageFileSheet->m_aPaths.RemoveAll();
		FillList();
		return;
	}

	
	if (Type != m_pManageFileSheet->m_Type)
	{		
		m_pManageFileSheet->m_Type = Type;
		m_pManageFileSheet->m_aPaths.RemoveAll();
		FillList();
	}

	((CButton*)GetDlgItem(IDC_MNGFILEDLG_BTNSEL))->EnableWindow(TRUE);		
}

//----------------------------------------------------------------------------
void CManageWizSearchPage::OnActivate()
{
	m_pManageFileSheet->SetWindowText(_TB("Wizard Object Copy - Search object to copy"));
	if (!m_pManageFileSheet->m_aPaths.GetSize())
		m_pManageFileSheet->EnableNextBtn(FALSE);

	FillList();
	CManageFileWizardPage::OnActivate();
}

//----------------------------------------------------------------------------
LRESULT CManageWizSearchPage::OnWizardNext()
{
	return IDD_MNGFILEDLG_WIZ_SAVEIMPORT;
}

//----------------------------------------------------------------------------
LRESULT CManageWizSearchPage::OnWizardBack()
{
	return IDD_MANAGEFILE_WIZ_PRESENTATION;
}

//============================================================================
//		CItemNoLocSaveDlg implementation
//			per localizzazione
//============================================================================
CItemNoLocSaveDlg::CItemNoLocSaveDlg(CString strName)
{
	m_strName = strName;
};

/////////////////////////////////////////////////////////////////////////////
// CManageWizSavePage dialog implementation
/////////////////////////////////////////////////////////////////////////////
//
BEGIN_MESSAGE_MAP(CManageWizSavePage, CManageFileWizardPage)
	//{{AFX_MSG_MAP(CManageWizSavePage)
	ON_CBN_SELCHANGE	(IDC_SAVEFILEDLG_CMBAPP,	OnComboAppChanged)
	ON_LBN_SELCHANGE	(IDC_SAVEFILEDLG_LISTMOD,	OnListModsChanged)
	ON_BN_CLICKED		(IDC_SAVEFILEDLG_RDALLUSR,	OnSelAllUsr)				
	ON_BN_CLICKED		(IDC_SAVEFILEDLG_RDUSR,		OnSelUsr)	
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//----------------------------------------------------------------------------
CManageWizSavePage::CManageWizSavePage()
	: 
	CManageFileWizardPage		(IDD_MNGFILEDLG_WIZ_SAVEIMPORT)
{
}

//----------------------------------------------------------------------------
CManageWizSavePage::~CManageWizSavePage()
{
} 

//--------------------------------------------------------------------------
void CManageWizSavePage::DoDataExchange(CDataExchange* pDX)
{
	CLocalizableDialog::DoDataExchange(pDX);
}

//----------------------------------------------------------------------------
BOOL CManageWizSavePage::OnInitDialog() 
{
	BOOL bInit = CManageFileWizardPage::OnInitDialog();

	VERIFY (m_ComboApp.SubclassDlgItem	(IDC_SAVEFILEDLG_CMBAPP,	this));
	VERIFY (m_ListMod.SubclassDlgItem	(IDC_SAVEFILEDLG_LISTMOD,	this));
	VERIFY (m_ListUsr.SubclassDlgItem	(IDC_SAVEFILEDLG_LISTUSR,	this));

	FillComboApp();
	FillListMod(TRUE);

	if (AfxGetLoginInfos()->m_bAdmin)
	{
		((CButton*) GetDlgItem(IDC_SAVEFILEDLG_RDALLUSR))->EnableWindow(TRUE);
		((CButton*) GetDlgItem(IDC_SAVEFILEDLG_RDUSR))->EnableWindow(TRUE);
	}
	else
	{
		((CButton*) GetDlgItem(IDC_SAVEFILEDLG_RDALLUSR))->EnableWindow(FALSE);
		((CButton*) GetDlgItem(IDC_SAVEFILEDLG_RDUSR))->EnableWindow(FALSE);
		m_ListUsr.EnableWindow(FALSE);
	}
		
	m_pManageFileSheet->EnableNextBtn(FALSE);

	((CButton*) GetDlgItem(IDC_SAVEFILEDLG_RDUSR))->SetCheck(TRUE);

	return CLocalizableDialog::OnInitDialog();
}

//--------------------------------------------------------------------------
void CManageWizSavePage::FillComboApp()
{
	CString				strApps			= _T("");
	CString				strDefaultApp	= _T("");
	int					nIdx;
	BOOL				bFirstValideApp = TRUE;
	CItemNoLocSaveDlg*	pItemNoLoc		= NULL;
	CTBNamespace		Ns;
	AddOnApplication*	pAddOnApplication = NULL;

	m_ComboApp.ResetContent();
	m_pManageFileSheet->m_Namespace.SetType(CTBNamespace::MODULE);

	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		strApps = AfxGetAddOnAppsTable()->GetAt(i)->m_strAddOnAppName;

		BOOL bSysApp = AfxGetPathFinder()->IsASystemApplication(strApps);
		if (bSysApp)
			continue;

		if (
			(!AfxGetBaseApp()->IsDevelopment() &&
		     strApps.CompareNoCase(AfxGetBaseApp()->GetTaskBuilderAddOnApp()->m_strAddOnAppName) == 0))
			continue;
		else
		{
			if (!AfxGetAddOnApp(strApps))
				continue;

			nIdx			= m_ComboApp.AddString(AfxGetAddOnApp(strApps)->GetTitle());
			pItemNoLoc		= new CItemNoLocSaveDlg(strApps);
			m_arAppItemLoc.Add(pItemNoLoc);
			m_ComboApp.SetItemData(nIdx, (DWORD) pItemNoLoc);
		}

		if (bFirstValideApp)
		{	
			strDefaultApp = strApps;

			m_pManageFileSheet->m_Namespace.SetApplicationName(strDefaultApp);

			pAddOnApplication =  AfxGetAddOnApp(strDefaultApp);
			AddOnModule* pAddOnMod = pAddOnApplication ? pAddOnApplication->m_pAddOnModules->GetAt(0) : NULL;
			if (pAddOnMod)
			{
				m_pManageFileSheet->m_Namespace.SetObjectName(CTBNamespace::MODULE, pAddOnMod->GetModuleName());
				bFirstValideApp = FALSE;
			}
		}
	}

	pAddOnApplication =  AfxGetAddOnApp(m_pManageFileSheet->m_Namespace.GetApplicationName());
	if (pAddOnApplication)
	{
		int n = m_ComboApp.FindStringExact(-1, (LPCTSTR) pAddOnApplication->GetTitle());
		m_ComboApp.SetCurSel(n);
	}
}

//--------------------------------------------------------------------------
void CManageWizSavePage::FillListMod(bool bFirst /*= FALSE */)
{
	CStringArray		aModules;
	CItemNoLocSaveDlg*	pItemNoLoc = NULL;
	CString				strMods		= _T("");
	int					nIdx;

	m_ListMod.ResetContent();
	m_arModItemLoc.RemoveAll();
	
	AddOnApplication* pAddOnApplication =  AfxGetAddOnApp(m_pManageFileSheet->m_Namespace.GetApplicationName());
	if (!pAddOnApplication)
		return;
	for (int a = 0; a <= pAddOnApplication->m_pAddOnModules->GetUpperBound(); a++)
	{
		AddOnModule* pAddOnMod = pAddOnApplication->m_pAddOnModules->GetAt(a);
		if (!AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()))
			continue;

		nIdx = m_ListMod.AddString(pAddOnMod->GetModuleTitle());
		pItemNoLoc = new CItemNoLocSaveDlg(pAddOnMod->GetModuleName());
		m_arModItemLoc.Add(pItemNoLoc);
		m_ListMod.SetItemData(nIdx, (DWORD) pItemNoLoc);	
	}	

	int m = -1; 	
	AddOnModule* pAddOnMod = NULL;

	if (bFirst)
	{
		for (int s = 0; s <= pAddOnApplication->m_pAddOnModules->GetUpperBound(); s++)
		{
			pAddOnMod = pAddOnApplication->m_pAddOnModules->GetAt(s);
			if (pAddOnMod->GetModuleName().CompareNoCase(m_pManageFileSheet->m_Namespace.GetObjectName(CTBNamespace::MODULE)) == 0)
			{
				m = m_ListMod.FindStringExact(-1,(LPCTSTR) pAddOnMod->GetModuleTitle());
				m_pManageFileSheet->m_Namespace.SetObjectName(CTBNamespace::MODULE, pAddOnMod->GetModuleName());
				m_ListMod.SetCurSel(m);
			}
		}						
	}

	if (!bFirst || m < 0)
	{	
		pAddOnMod = pAddOnApplication->m_pAddOnModules->GetAt(0);
		if (pAddOnMod != NULL)
		{
            m = m_ListMod.FindStringExact(-1, (LPCTSTR) pAddOnMod->GetModuleTitle());
			m_pManageFileSheet->m_Namespace.SetObjectName(CTBNamespace::MODULE, pAddOnMod->GetModuleName());
			m_ListMod.SetCurSel(m);
		}
	}

	m_pManageFileSheet->EnableNextBtn(TRUE);
}

//--------------------------------------------------------------------------
void CManageWizSavePage::FillListUsr()
{
	m_ListUsr.ResetContent();
	for (int n = 0 ; n <= AfxGetLoginInfos()->m_CompanyUsers.GetUpperBound(); n++)
		m_ListUsr.AddString(AfxGetLoginInfos()->m_CompanyUsers.GetAt(n));
	
	int nSel = 0;
	if (!m_pManageFileSheet->m_aUsrForSave.GetSize())
	{
		nSel = m_ListUsr.FindStringExact(-1, (LPCTSTR) AfxGetLoginInfos()->m_strUserName);
		m_ListUsr.SetSel(nSel);		
		return;
	}
	
	for (int i = 0; i <= m_pManageFileSheet->m_aUsrForSave.GetUpperBound(); i++)
	{
		nSel = m_ListUsr.FindStringExact(-1, (LPCTSTR) m_pManageFileSheet->m_aUsrForSave.GetAt(i));
		m_ListUsr.SetSel(nSel);
	}

}

//--------------------------------------------------------------------------
void CManageWizSavePage::OnComboAppChanged()
{
	CString			Label			= _T("");
	BOOL			bFind			= FALSE;
	int				nCurSel			= m_ComboApp.GetCurSel();
	
	CItemNoLocSaveDlg* pItemNoLoc = (CItemNoLocSaveDlg*) m_ComboApp.GetItemData(nCurSel);
	m_ListMod.ResetContent();
	Label = pItemNoLoc->m_strName;

	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		CString strApps = AfxGetAddOnAppsTable()->GetAt(i)->m_strAddOnAppName;
		if (strApps == Label)
			bFind = TRUE; 
	}

	if (!bFind)
		Label = _T(""); 

	m_pManageFileSheet->m_Namespace.SetApplicationName(Label);
	FillListMod();
}

//--------------------------------------------------------------------------
void CManageWizSavePage::OnListModsChanged()
{
	int					nCurSel			= m_ListMod.GetCurSel();
	CItemNoLocSaveDlg*	ItemNoLoc		= (CItemNoLocSaveDlg*) m_ListMod.GetItemData(nCurSel);
	CString				strModulename	= ItemNoLoc->m_strName;
	
	m_pManageFileSheet->m_Namespace.SetObjectName(CTBNamespace::MODULE, strModulename);	
}

//--------------------------------------------------------------------------
void CManageWizSavePage::OnSelAllUsr()
{
	m_ListUsr.EnableWindow(FALSE);
}

//--------------------------------------------------------------------------
void CManageWizSavePage::OnSelUsr()
{
	m_ListUsr.EnableWindow(TRUE);
}

//--------------------------------------------------------------------------
void CManageWizSavePage::GetUserForSave()
{
	m_pManageFileSheet->m_aUsrForSave.RemoveAll();
	m_pManageFileSheet->m_bAllUsers = FALSE;

	if (((CButton*) GetDlgItem(IDC_SAVEFILEDLG_RDALLUSR))->GetCheck())
		m_pManageFileSheet->m_bAllUsers = TRUE;
	else
	{
		int		size	 = m_ListUsr.GetCount();
		CString	strOwner = _T("");

		for (int i = size - 1 ; i >= 0 ; i--)
		{
			if (m_ListUsr.GetSel(i) > 0)
			{
				m_ListUsr.GetText(i, strOwner);
				m_pManageFileSheet->m_aUsrForSave.Add(strOwner);				
			}
		}	
	}
}

//----------------------------------------------------------------------------
void CManageWizSavePage::OnActivate()
{
	m_pManageFileSheet->SetWindowText(_TB("Wizard Object Copy - Selection target save "));
	FillListUsr();
	CManageFileWizardPage::OnActivate();
}

//----------------------------------------------------------------------------
LRESULT CManageWizSavePage::OnWizardNext()
{
	GetUserForSave();
	if (m_pManageFileSheet->m_aPaths.IsEmpty())
		return IDD_MNGFILEDLG_WIZ_SAVEIMPORT;
	return IDD_MANAGEFILE_WIZ_FINISH;
}

//----------------------------------------------------------------------------
LRESULT CManageWizSavePage::OnWizardBack()
{
	return IDD_MANAGEFILE_WIZ_SEARCH;
}


/////////////////////////////////////////////////////////////////////////////
// CManageWizFinishPage property page implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CManageWizFinishPage, CManageFileWizardPage)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManageWizFinishPage, CManageFileWizardPage)
	//{{AFX_MSG_MAP(CManageWizFinishPage)	
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CManageWizFinishPage::CManageWizFinishPage() 
	: 
	CManageFileWizardPage(IDD_MANAGEFILE_WIZ_FINISH)
{
}

//-----------------------------------------------------------------------------
void CManageWizFinishPage::DoDataExchange(CDataExchange* pDX)
{
	CWizardPage::DoDataExchange(pDX);
}

//--------------------------------------------------------------------
BOOL CManageWizFinishPage::OnInitDialog()
{
	VERIFY (m_ListSelected.SubclassDlgItem	(IDC_MNGFILEDLG_LIST,	this));
	return CManageFileWizardPage::OnInitDialog();
}

//--------------------------------------------------------------------
void CManageWizFinishPage::OnActivate()
{	
	m_pManageFileSheet->SetWindowText(_TB("Wizard Object Copy - Summary"));
	CString strType;// = _TB("");
	switch (m_pManageFileSheet->m_Type)
	{
		case CTBNamespace::REPORT:
			strType = _TB("Report");
			break;
		case CTBNamespace::IMAGE:
			strType = _TB("Image");
			break;
		case CTBNamespace::TEXT:
			strType = _TB("Text");
			break;
		case CTBNamespace::PDF:
			strType = _TB("Pdf");
			break;
		case CTBNamespace::RTF:
			strType = _TB("Rtf");
			break;
		case CTBNamespace::ODS:
			strType = _TB("Ods");
			break;
		case CTBNamespace::ODT:
			strType = _TB("Odt");
			break;
	}

	((CStatic*) GetDlgItem(IDC_STATIC_FINISH_TXT))->SetWindowText(cwsprintf(_TB("Will be copy object of type {0-%s}:"), strType));
	((CStatic*) GetDlgItem(IDC_STATIC_FINISH_TXT1))->SetWindowText(
																	cwsprintf(_TB("Save wiil be for:\n- application: {0-%s};\n- module:{1-%s}."),  
																	AfxGetAddOnApp(m_pManageFileSheet->m_Namespace.GetApplicationName())->GetTitle(), 
																	AfxGetAddOnModule(m_pManageFileSheet->m_Namespace)->GetModuleTitle())
																 );	
	CManageFileWizardPage::OnActivate();
	FillListPath();
}

//--------------------------------------------------------------------
void CManageWizFinishPage::FillListPath()
{
	m_ListSelected.ResetContent();
	for (int i = 0; i <= m_pManageFileSheet->m_aPaths.GetUpperBound(); i++)
	{	
		CString strPath = (CString) m_pManageFileSheet->m_aPaths.GetAt(i);
		m_ListSelected.AddString(strPath);
	}

	CString     str;
	CSize		sz;
	int			dx = 0;
	TEXTMETRIC  tm;
	CDC*		pDC		= m_ListSelected.GetDC();
	CFont*		pFont	= m_ListSelected.GetFont();

	// Select the listbox font, save the old font
	CFont* pOldFont = pDC->SelectObject(pFont);
	// Get the text metrics for avg char width
	pDC->GetTextMetrics(&tm); 

	for (int n = 0; n < m_ListSelected.GetCount(); n++)
	{
		m_ListSelected.GetText(n, str);
		sz = pDC->GetTextExtent(str);

	// Add the avg width to prevent clipping
		sz.cx += tm.tmAveCharWidth;

		if (sz.cx > dx)
			dx = sz.cx;
	}
	// Select the old font back into the DC
	pDC->SelectObject(pOldFont);
	m_ListSelected.ReleaseDC(pDC);

	// Set the horizontal extent so every character of all strings 
	// can be scrolled to.
	m_ListSelected.SetHorizontalExtent(dx);

}

//--------------------------------------------------------------------
LRESULT CManageWizFinishPage::OnWizardBack()
{
	return IDD_MNGFILEDLG_WIZ_SAVEIMPORT;
}

/////////////////////////////////////////////////////////////////////////////
//		 CManageFileWizMasterDlg property sheet implementation
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CManageFileWizMasterDlg, CWizardMasterDialog)
//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManageFileWizMasterDlg, CWizardMasterDialog)
	//{{AFX_MSG_MAP(CManageFileWizMasterDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------
CManageFileWizMasterDlg::CManageFileWizMasterDlg(CWnd* pParent /*=NULL*/)
	: 
	CWizardMasterDialog		(IDD_MANAGEFILE_WIZARD_MASTER, pParent),
	m_bAllUsers				(FALSE),
	m_bExternalCopy			(TRUE)
{
	// Add the property pages
	AddPage (&m_ManageWizPresentationPage);
	AddPage (&m_ManageSearchPage);
	AddPage (&m_ManageSavePage);
	AddPage (&m_ManageFinishPage);
}

//---------------------------------------------------------------------------
// CManageFileWizMasterDlg message handlers
//---------------------------------------------------------------------------
BOOL CManageFileWizMasterDlg::OnInitDialog() 
{
	SetPlaceholderID(IDC_MANAGEFILE_WIZ_STATIC_SHEET_RECT);
	CWizardMasterDialog::OnInitDialog();
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

//---------------------------------------------------------------------------
BOOL CManageFileWizMasterDlg::OnWizardFinish() 
{
	CString			strSourceFile = _T(""); 
	CString			strTargetPath = _T("");
	CString			strTargetFile = _T("");
	CStringArray	aMessage;
	int				nCopied = 0;
	
	if (m_aPaths.IsEmpty())
	{
		CLocalizableDialog::OnOK();
		return FALSE;
	}

	switch (m_Type)
	{
		case (CTBNamespace::REPORT):
			if (m_bAllUsers)
			{
				strTargetPath = AfxGetPathFinder()->GetModuleReportPath(m_Namespace, CPathFinder::ALL_USERS, _T(""), TRUE);
				nCopied += ImportCopyFile(strTargetPath, _T(""), &aMessage);	
			}
			else
				for (int n = 0; n <= m_aUsrForSave.GetUpperBound(); n++)
				{
					strTargetPath = AfxGetPathFinder()->GetModuleReportPath(m_Namespace, CPathFinder::USERS, m_aUsrForSave.GetAt(n), TRUE);
					nCopied += ImportCopyFile(strTargetPath, m_aUsrForSave.GetAt(n), &aMessage);					
				}
			break;

		case (CTBNamespace::IMAGE):
		case (CTBNamespace::TEXT):
		case (CTBNamespace::PDF):
		case (CTBNamespace::RTF):
		//case (CTBNamespace::ODF):
		case (CTBNamespace::FILE):
			if (m_bAllUsers)
			{
				m_Namespace.SetType(m_Type);
				strTargetPath = AfxGetPathFinder()->GetModuleFilesPath(m_Namespace, CPathFinder::ALL_USERS, _T(""), TRUE);
				nCopied += ImportCopyFile(strTargetPath, _T(""), &aMessage);
			}
			else
				for (int n = 0; n <= m_aUsrForSave.GetUpperBound(); n++)
				{
					m_Namespace.SetType(m_Type);
					strTargetPath = AfxGetPathFinder()->GetModuleFilesPath(m_Namespace, CPathFinder::USERS, m_aUsrForSave.GetAt(n), TRUE);
					nCopied += ImportCopyFile(strTargetPath, m_aUsrForSave.GetAt(n), &aMessage);					
				}
			break;
	}	

	CString	str = _T("");
	for (int s = 0; s <= aMessage.GetUpperBound(); s++)
		str += aMessage.GetAt(s) + _T("\n");

	int nFileToCopy = m_aUsrForSave.GetSize() * m_aPaths.GetSize();

	if (str.IsEmpty())
	{
		if (nFileToCopy == nCopied)
			AfxMessageBox(_TB("Object copy is successfully completed!"));
		if (nFileToCopy > nCopied)
			if (nFileToCopy == 1)
				return TRUE;
			else
				AfxMessageBox(_TB("Only selected objects are successfully copied!"));
	}
	else
		AfxMessageBox(cwsprintf(_TB("Object copy is not successfully completed.\nFollowing files aren't copy: \n{0-%s}"), str));
	return TRUE;
}

//--------------------------------------------------------------------------
int CManageFileWizMasterDlg::ImportCopyFile(const CString& strTargetPath, const CString& strUsr, CStringArray* aMsg)
{
	CString			strSourceFile = _T(""); 
	CString			strTargetFile = _T("");
	CString			sUser		  = _T("");
	BOOL			nCopied		  = 0;
	BOOL			nToCopy		  = 0;

	for (int i = 0; i <= m_aPaths.GetUpperBound(); i++)
	{
		strSourceFile = m_aPaths.GetAt(i);
		strTargetFile = strTargetPath + SLASH_CHAR + GetNameWithExtension(strSourceFile);
		if(ExistFile(strTargetFile))
		{
			if (strUsr.IsEmpty())	
				sUser = _TB("All the Users");
			else
				sUser = strUsr;
			if (AfxMessageBox(cwsprintf(_TB("Object '{0-%s}' already exists in '{1-%s}'. Do you want to replace it ?"), GetNameWithExtension(strSourceFile), sUser), MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK)
			{
                if(!::CopyFile(strSourceFile, strTargetFile, FALSE))
					aMsg->Add(strSourceFile);
				else
					nCopied++;
			}			
		}
		else
		{
			if(!::CopyFile(strSourceFile, strTargetFile, FALSE))
				aMsg->Add(strSourceFile);
			else
				nCopied++;
		}
	}
	return nCopied;
}
