#include "stdafx.h" 

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGenlib\TBSTRINGS.h>
#include <TbGenlib\CEFClasses.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGenlib\Generic.h>

#include <TbClientCore\ClientObjects.h>

#include "BrowserDlg.h"
#include "BrowserDlg.hjson" //JSON AUTOMATIC UPDATE
#include ".\browserdlg.h"


//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBrowserDlg, CLocalizableDialog )
	ON_WM_SIZE( )
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (CBrowserDlg, CLocalizableDialog );

//------------------------------------------------------------------------------
CBrowserDlg::CBrowserDlg ()
:
CLocalizableDialog (IDD_DLG_BROWSER), m_pBrowser(NULL)
{
}

//------------------------------------------------------------------------------
CBrowserDlg::CBrowserDlg (const CString &strURL)
:
CLocalizableDialog (IDD_DLG_BROWSER), m_pBrowser(NULL)
{
	//ASSERT(!strURL.IsEmpty ());
	m_strURL = strURL;
}
//------------------------------------------------------------------------------
void CBrowserDlg::OnAfterCreated(CBrowserObj* pBrowser)
{
	m_pBrowser = pBrowser;
	CRect aRect;
	GetClientRect (aRect);
	AdjustPosition (aRect.right - aRect.left, aRect.bottom - aRect.top);

}

//------------------------------------------------------------------------------
void CBrowserDlg::OnBeforeClose(CBrowserObj* pBrowser)
{
	m_pBrowser = NULL;
	//OnCancel ();
}
	
//-------------------------------------------------------------------------------
void CBrowserDlg::OnSize(UINT nType, int cx, int cy)
{
	CLocalizableDialog::OnSize(nType, cx, cy);

	AdjustPosition(cx, cy);
}

//-------------------------------------------------------------------------------
BOOL CBrowserDlg::OnInitDialog ()
{
	CLocalizableDialog::OnInitDialog(); 
	VERIFY(CreateChildBrowser(m_hWnd, m_strURL, TRUE, this));

	return TRUE;
}

//-------------------------------------------------------------------------------
void CBrowserDlg::AdjustPosition(int cx, int cy)
{
	HWND hwnd = m_pBrowser ? m_pBrowser->GetMainWnd() : NULL;
	if (hwnd)
		CWnd::FromHandle(hwnd)->SetWindowPos (NULL, 5, 5, cx-10, cy-10, SWP_NOACTIVATE | SWP_NOZORDER);
} 

//------------------------------------------------------------------------------
void CBrowserDlg::Navigate(CString strURL)
{
	if( m_pBrowser && strURL.GetLength() )
		m_pBrowser->Navigate (strURL);
}

//------------------------------------------------------------------------------
void CBrowserDlg::ExecuteJavascript	(const CString& sCode)
{
	if( m_pBrowser)
		m_pBrowser->ExecuteJavascript (sCode);
}
//------------------------------------------------------------------------------
void CBrowserDlg::SetCookie(const CString& sUrl, const CString& sName, const CString& sValue)
{
	ASSERT(m_pBrowser);
	m_pBrowser->SetCookie(sUrl, sName, sValue);
}
//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CModelessBrowserDlg, CBrowserDlg )
	ON_WM_CLOSE()
	ON_WM_DESTROY()
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (CModelessBrowserDlg, CBrowserDlg );

//------------------------------------------------------------------------------
CModelessBrowserDlg::CModelessBrowserDlg()
	:
	CBrowserDlg(_T(""))
{
}
//------------------------------------------------------------------------------
CModelessBrowserDlg::CModelessBrowserDlg(const CString &strURL)
	:
	CBrowserDlg(strURL)
{
}
//------------------------------------------------------------------------------
void CModelessBrowserDlg::OnClose()
{
	__super::OnClose();
}

//------------------------------------------------------------------------------
void CModelessBrowserDlg::OnDestroy()
{
	__super::OnDestroy();
}

//------------------------------------------------------------------------------
BOOL CModelessBrowserDlg::Create()
{
	return __super::Create(IDD_DLG_BROWSER);
}
