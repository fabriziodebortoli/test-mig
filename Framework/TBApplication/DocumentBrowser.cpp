#include "StdAfx.h"
#include <TbNameSolver\ThreadContext.h>
#include <TbGeneric\DataObj.h>
#include "DocumentBrowser.h"



IMPLEMENT_DYNAMIC(CDocumentBrowser, CModelessBrowserDlg);
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDocumentBrowser, CModelessBrowserDlg)

	ON_MESSAGE(UM_UPDATE_FRAME_STATUS, OnChangeFrameStatus)
	ON_MESSAGE(UM_GET_DOC_NAMESPACE_ICON,			OnGetNamespaceAndIcon)
	ON_MESSAGE(UM_GET_DOCUMENT_TITLE_INFO,			OnGetDocumentTitleInfo)
	ON_WM_SIZE( )

	ON_WM_DESTROY()
END_MESSAGE_MAP()
//------------------------------------------------------------------------------
BOOL CDocumentBrowser::OnInitDialog ()
{
	BOOL b = __super::OnInitDialog();
	ShowWindow(SW_HIDE);
	if (AfxGetThreadContext()->m_bSendDocumentEventsToMenu)
	{
		::PostMessage(AfxGetMenuWindowHandle(), UM_DOCUMENT_CREATED, (WPARAM)m_hWnd, (LPARAM)NULL);
	}
	return b;
}

//-----------------------------------------------------------------------------
LRESULT CDocumentBrowser::OnChangeFrameStatus(WPARAM wParam, LPARAM lParam)
{
	BOOL bDocked = (BOOL) wParam;
	HWND hwndParent = (HWND)lParam;

	DWORD dwRemoveForDockStyle = WS_THICKFRAME | WS_CAPTION | WS_POPUP;
	DWORD dwAddForDockStyle = WS_CHILD;

	if ( bDocked )   //draw window in tab rectangle
	{
		ModifyStyle(dwRemoveForDockStyle, dwAddForDockStyle);
		SetParent(CWnd::FromHandle(hwndParent));
		ShowWindow(SW_SHOW);//SW_MAXIMIZE non va bene perche' altera la dimensione della finestra dockata (fenomeni di finestra piccola dentro tab grande)
	}	
	return 0;
}
//-----------------------------------------------------------------------------
LRESULT CDocumentBrowser::OnGetDocumentTitleInfo(WPARAM wParam, LPARAM lParam)
{
	/*CDocument* pDoc = GetActiveDocument();
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
	{
	TCHAR* pMsgBuff = (TCHAR*) wParam;
	UINT nSize = (UINT) lParam;

	//restituisco il testo della finestra, più la lunghezza del titolo
	//usato dal menu per sapere quanta parte del titolo finestra corrisponde al titolo documento
	//(il testo della finestra infatti inizia sempre col titolo del documento)
	CString strWindowText;
	GetWindowText(strWindowText);

	_tcscpy_s(pMsgBuff, nSize, strWindowText);
	return ((CBaseDocument*)pDoc)->GetTitle().GetLength();
	}
	return 0;*/

	TCHAR* pMsgBuff = (TCHAR*) wParam;
	UINT nSize = (UINT) lParam;

	//restituisco il testo della finestra, più la lunghezza del titolo
	//usato dal menu per sapere quanta parte del titolo finestra corrisponde al titolo documento
	//(il testo della finestra infatti inizia sempre col titolo del documento)
	CString strWindowText;
	//if (m_pBrowser)
	//{
	//	::GetWindowText(m_pBrowser->GetMainWnd(), pMsgBuff, nSize);
	//}
	//else
	{
		GetWindowText(strWindowText);
		_tcscpy_s(pMsgBuff, nSize, strWindowText);
	}
	return 0;
}
//-----------------------------------------------------------------------------
LRESULT CDocumentBrowser::OnGetNamespaceAndIcon(WPARAM wParam, LPARAM lParam)
{
	/*
	CDocument* pDoc = GetActiveDocument();
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
	{
	TCHAR* pMsgBuff = (TCHAR*) wParam;
	UINT nSize = (UINT) lParam;
	_tcscpy_s(pMsgBuff, nSize, ((CBaseDocument*)pDoc)->GetNamespace().ToString());
	}
	*/
	TCHAR* pMsgBuff = (TCHAR*) wParam;
	UINT nSize = (UINT) lParam;
	_tcscpy_s(pMsgBuff, nSize, m_sNamespace);
	return GetClassLong(m_hWnd, GCL_HICON);
}

//-----------------------------------------------------------------------------
void CDocumentBrowser::OpenDocument(const CString& sNamespace)
{
	m_sNamespace = sNamespace;
	CString sUrl = cwsprintf(L"http://localhost/tb/document/document.html?ns=%s&session=%d", sNamespace, GetTickCount());
	Navigate(sUrl);
	SetCookie(L"http://localhost", AUTH_TOKEN_PARAM, AfxGetAuthenticationToken());
}

//-----------------------------------------------------------------------------
void CDocumentBrowser::OnDestroy()
{
	CModelessBrowserDlg::OnDestroy();

	if (AfxGetThreadContext()->m_bSendDocumentEventsToMenu)
	{
		::PostMessage(AfxGetMenuWindowHandle(), UM_DOCUMENT_DESTROYED, (WPARAM)m_hWnd, NULL);
		//nascondo la finestra per evitare flickering quando la sgancio dal parent
		ShowWindow(SW_HIDE);
	}
}
