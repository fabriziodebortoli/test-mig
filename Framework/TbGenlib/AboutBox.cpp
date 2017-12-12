
#include "stdafx.h"

#include <dos.h>
#include <direct.h>

#include <TbNameSolver\IFileSystemDriver.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TbNameSolver\FileSystemCache.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "BaseApp.h"
#include "AddOnMng.h"
#include "ParsObj.h"
#include "Basefrm.hjson" //JSON AUTOMATIC UPDATE 

#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include "AboutBox.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CAboutBox dialog
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CAboutBox, CParsedDialog)
BEGIN_MESSAGE_MAP(CAboutBox, CParsedDialog)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAboutBox::CAboutBox(CWnd* pParent /*=NULL*/)
	: 
	CParsedDialog		(IDD_ABOUTBOX, pParent)
{
	m_pAddOnVersions	= new CBCGPEdit();
	m_pImage = new CTranspBmpCtrl(AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szIconAboutStrip), _T("")));
}

//-----------------------------------------------------------------------------
CAboutBox::~CAboutBox()
{
	ASSERT(m_pAddOnVersions);	
	delete m_pAddOnVersions;	    
	delete m_pImage;
}

//-----------------------------------------------------------------------------
BOOL CAboutBox::OnInitDialog()
{
	CParsedDialog::OnInitDialog	();
	m_pAddOnVersions->SubclassDlgItem(IDC_ADD_ON_VERSIONS, this);
	m_pAddOnVersions->ShowScrollBar(SB_VERT,FALSE);

	m_pImage->SubclassDlgItem(IDC_LOGO_MICROAREA, this);
	
	// setta il titolo della applicazione se esiste	
	SetWindowText(AfxGetBaseApp()->GetAppTitle());
		
	CWnd* pWnd = GetDlgItem(IDC_ABOUTBOX_DBTYPE);
	if (pWnd)
		pWnd->SetWindowText(AfxGetLoginInfos()->m_strProviderDescription);

	pWnd = GetDlgItem(IDC_ABOUTBOX_VERSION);
	if (pWnd)
		pWnd->SetWindowText(AfxGetLoginManager()->GetInstallationVersion());

	pWnd = GetDlgItem(IDC_ABOUTBOX_EDITION);
	if (pWnd)
		pWnd->SetWindowText(AfxGetLoginManager()->GetEditionType());

	pWnd = GetDlgItem(IDC_ABOUTBOX_INSTALLATION);
	if (pWnd)
		pWnd->SetWindowText(AfxGetPathFinder()->GetInstallationName());

	pWnd = GetDlgItem(IDC_ABOUTBOX_PROGRAMSTATE);
	if (pWnd)
		pWnd->SetWindowText(AfxGetCommonClientObjects() ? AfxGetCommonClientObjects()->GetActivationStateInfo() : _T(""));
	
	// riempie e visualizza l'elenco delle versioni delle add-on application
	FillAddOnList();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CAboutBox::OnOK ()
{                      
	__super::OnOK	();
}

//-----------------------------------------------------------------------------
void CAboutBox::FillAddOnList()
{
	CString strVersions;

	// accoda i nomi e le relase di tutte le add-on application
	CString sActive;
	CString sAppName;
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++ )
	{
		AddOnApplication* pAddOn = AfxGetAddOnAppsTable()->GetAt(i);
		ASSERT(pAddOn);
		
		sAppName = pAddOn->GetTitle();
		if (sAppName.IsEmpty())
			sAppName = pAddOn->m_strAddOnAppName;

		sActive = AfxIsAppActivated(pAddOn->m_strAddOnAppName) ? _TB("Licensed") : _TB("Not Licensed");
		strVersions += cwsprintf(_TB("{0-%s} rel. {1-%s} ({2-%s})"), sAppName, pAddOn->GetAppVersion(), sActive) + _T("\r\n");
	}

	// aggiunge la release di TB
#ifdef _DEBUG
	strVersions += _T("\r\n") + _TB("Program is running in Debug version");
#endif

	m_pAddOnVersions->SetWindowText((LPCTSTR)strVersions);

	// fa comparire la scroll bar verticale se necessario
	TEXTMETRIC tm;
	CDC* pDC = m_pAddOnVersions->GetDC();
	pDC->GetTextMetrics(&tm);
	m_pAddOnVersions->ReleaseDC(pDC);

	CRect r;
	m_pAddOnVersions->GetRect(&r);
	if (m_pAddOnVersions->GetLineCount() > r.Height()/tm.tmHeight)
		m_pAddOnVersions->ShowScrollBar(SB_VERT,TRUE);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CAboutBox::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CAboutBox\n");
	CParsedDialog::Dump(dc);
}

void CAboutBox::AssertValid() const
{
	CParsedDialog::AssertValid();
}

#endif // _DEBUG
