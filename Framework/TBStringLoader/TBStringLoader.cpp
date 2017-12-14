
#include "stdafx.h"

#include <afxtempl.h>
#include <msxml6.h>


#include "TBStringLoader.h"
#include "StringLoader.h"
#include "const.h"
#include "generic.h"



//
//	Note!
//
//		If this DLL is dynamically linked against the MFC
//		DLLs, any functions exported from this DLL which
//		call into MFC must have the AFX_MANAGE_STATE macro
//		added at the very beginning of the function.
//
//		For example:
//
//		extern "C" BOOL PASCAL TB_EXPORT ExportedFunction()
//		{
//			AFX_MANAGE_STATE(AfxGetStaticModuleState());
//			// normal function body here
//		}
//
//		It is very important that this macro appear in each
//		function, prior to any calls into MFC.  This means that
//		it must appear as the first statement within the 
//		function, even before any object variable declarations
//		as their constructors may generate calls into the MFC
//		DLL.
//
//		Please see MFC Technical Notes 33 and 58 for additional
//		details.
//


//////////////////////////////////////////////////////////////////////////////
// CTBStringLoaderApp
//////////////////////////////////////////////////////////////////////////////

//===========================================================================
BEGIN_MESSAGE_MAP(CTBStringLoaderApp, CWinApp)
END_MESSAGE_MAP()

//===========================================================================
CTBStringLoaderApp::CTBStringLoaderApp()
{
	m_pLoader = NULL;
}

//===========================================================================
CTBStringLoaderApp::~CTBStringLoaderApp()
{
}

//===========================================================================
void CTBStringLoaderApp::AddWindow(CObject *pWnd)
{
	m_WindowsArray.Add(pWnd);
}

//===========================================================================
void CTBStringLoaderApp::RemoveWindow(CObject *pWnd)
{
	for (int i=0; i<m_WindowsArray.GetSize(); i++)
	{
		if (m_WindowsArray[i] == pWnd) m_WindowsArray.RemoveAt(i);
	}

}

//===========================================================================
CDemoFrame* CTBStringLoaderApp::GetWndFromIDD(UINT nIDD)
{
	CDemoFrame* pWnd = NULL;
	for (int i=0; i<m_WindowsArray.GetSize(); i++)
	{
		pWnd = (CDemoFrame*)m_WindowsArray[0];
		if (pWnd->m_nResID == nIDD) return pWnd;
	}

	return NULL;
}

//===========================================================================
void CTBStringLoaderApp::AddModule(HMODULE hMod)
{
	m_ModulesArray.Add(hMod);
}

//===========================================================================
BOOL CTBStringLoaderApp::ExistsModule(HMODULE hMod)
{
	HMODULE aMod;
	for (int i=0; i<m_ModulesArray.GetSize(); i++)
	{
		aMod = (HMODULE)m_ModulesArray[i];
		if (aMod == hMod) return TRUE;
	}
	return FALSE;
}

// The one and only CTBStringLoaderApp object
CTBStringLoaderApp theApp;

//===========================================================================
BOOL CTBStringLoaderApp::InitInstance()
{
	CWinApp::InitInstance();

	AfxEnableControlContainer();

	m_strWindowClassName = AfxRegisterWndClass(NULL);

	InitFont();

	return TRUE;
}

//===========================================================================
int CTBStringLoaderApp::ExitInstance()
{
	delete m_pLoader;

	FreeCache();

	return CWinApp::ExitInstance();
}

//===========================================================================
CStringLoader* CTBStringLoaderApp::GetGlobalStringLoader()
{
	if (!m_pLoader) 
	{
		TCHAR path[MAX_PATH];
		GetTempPath(MAX_PATH, path);

		m_pLoader = new CStringLoader(_T(""), path); 
	}

	return m_pLoader;
}

//===========================================================================
void CTBStringLoaderApp::FreeCache()
{
	for (int i=0; i<m_ModulesArray.GetSize(); i++)
		::FreeLibrary((HMODULE) m_ModulesArray[i]);

	CDemoFrame* pWnd = NULL;
	while (m_WindowsArray.GetSize())
	{
		pWnd = (CDemoFrame*)m_WindowsArray[0];
		pWnd->DestroyWindow(); //il distruttore della finestra si preoccupa di rimuoversi dalla lista
	}
}

//===========================================================================
void CTBStringLoaderApp::InitFont()
{
	if (m_FormFont.m_hObject)
		m_FormFont.DeleteObject();

	CString sFFName		= _T("Verdana");
	int height = -12;
	CWnd *pWnd = GetMainWnd();
	if (pWnd)
	{
		CDC* pDC = pWnd->GetDC();
		if (pDC)
		{
			height = -MulDiv(height, pDC->GetDeviceCaps(LOGPIXELSY), 72);
			pWnd->ReleaseDC(pDC);
		}
	}

	m_FormFont.CreateFont
	(
		height, 0, 0, 0,
		FW_NORMAL, FALSE, FALSE, FALSE, DEFAULT_CHARSET,
		OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY, 
		DEFAULT_PITCH, sFFName
	);

}

//===========================================================================
void CTBStringLoaderApp::SetFormFont()
{
	LOGFONT lf;
	m_FormFont.GetLogFont(&lf);
	
	CFontDialog dlg(&lf);
	if (dlg.DoModal()==IDOK)
	{
		if (m_FormFont.m_hObject)
			m_FormFont.DeleteObject();
		m_FormFont.CreateFontIndirect(&lf);
	}
}


//===========================================================================
// GLOBAL FUNCTIONS
//===========================================================================
//---------------------------------------------------------------------------
HMODULE LoadModule(LPCTSTR lpcszModule)
{
	HMODULE hMod = LoadLibraryEx(lpcszModule, NULL, LOAD_LIBRARY_AS_IMAGE_RESOURCE); 
	if (!hMod) 
	{
		/*CString strMsg;
		strMsg.Format(LOAD_FAILED, lpcszModule);
		AfxMessageBox(strMsg);*/
		return NULL;
	}

	if (!theApp.ExistsModule (hMod)) theApp.AddModule (hMod);

	return hMod;
}
//---------------------------------------------------------------------------
CDemoFrame* CreateDemoWindow(HWND hParent, LPCTSTR lpcszTitle, LPCTSTR lpcszModule, UINT nIDD)
{
	HMODULE hMod = LoadModule(lpcszModule);

	CDemoFrame *pFrame = theApp.GetWndFromIDD(nIDD);
	if (!pFrame)
	{
		CCreateContext cc;
		cc.m_pNewViewClass = RUNTIME_CLASS(CDemoView);

		pFrame = new CDemoFrame(nIDD, hMod);
		CRect wRect(0,0,0,0);

		CWnd* pParentWnd = NULL;
		if (hParent)
		{
			pParentWnd = CWnd::FromHandle(hParent);
			pParentWnd->GetWindowRect(wRect);		
		}

		if (!pFrame->Create (theApp.m_strWindowClassName, lpcszTitle, WS_OVERLAPPEDWINDOW, CRect(wRect.left,wRect.bottom,0, 0), pParentWnd, NULL, 0, &cc))
		{
			delete pFrame;
			return NULL;
		}	
		
		pFrame->InitialUpdateFrame(NULL, TRUE);

		// aggiungo la finestra alla lista di quelle aperte
		theApp.AddWindow(pFrame);
	}
	return pFrame;
}

//---------------------------------------------------------------------------
void FreeCache()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());  
	theApp.FreeCache();
}



//---------------------------------------------------------------------------
void SetFormFont()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());  
	
	theApp.SetFormFont();
}

//---------------------------------------------------------------------------
LPCTSTR LoadXMLString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrFileName, LPCTSTR lpcstrDictionaryPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CString strResult;

	strResult = theApp.GetGlobalStringLoader()->LoadXMLString(lpcstrBaseString, lpcstrFileName, lpcstrDictionaryPath); 

	int count = strResult.GetLength() + 1;
	LPTSTR lpResult = (LPTSTR) CoTaskMemAlloc( count * sizeof(TCHAR) );
	_tcscpy_s( lpResult, count, strResult );
	return lpResult;
}

//---------------------------------------------------------------------------
LPCTSTR CheckDialog(LPCTSTR lpcszModule, UINT nIDD, float fRatio, LPCTSTR lpcszDictionaryPath)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CComBSTR strResult = L"";
	
	if( nIDD != 0)
	{
		CDemoFrame* pFrame = CreateDemoWindow(NULL, NULL, lpcszModule, nIDD);

		CDemoView* pView = (CDemoView*) pFrame->GetActiveView();
			
		if (pView && pView->CreateDemoDialog(NULL, pFrame))
		{
			CDemoDialog *pDialog = pView->GetDemoDialog();

			CComPtr<IXMLDOMDocument> pDom;
			pDom.CoCreateInstance(__uuidof(DOMDocument60));
			CComPtr<IXMLDOMElement> pRoot;
			CComPtr<IXMLDOMNode> pChild;
			pDom.p->createElement(CComBSTR(STRINGS_NODE), &pRoot);
			pDom.p->appendChild(pRoot, &pChild);
			
			CStringArray arStrings, arExpected, arActual, arDeviations;
			if (!CheckRectSizes(pDialog, fRatio, arStrings, arExpected, arActual, arDeviations) && !arStrings.IsEmpty())
			{			
				for (int i=0; i<arStrings.GetSize(); i++)
				{
					CComPtr<IXMLDOMElement> pNode;
					pDom.p->createElement(CComBSTR(STRING_NODE), &pNode);
					CComPtr<IXMLDOMNode> pChild1;
					pRoot.p->appendChild(pNode, &pChild1);
			
					CComPtr<IXMLDOMText> pText;
					pDom.p->createTextNode(CComBSTR(arStrings[i]), &pText); 
					CComPtr<IXMLDOMNode> pChild2;
					pNode.p->appendChild(pText, &pChild2);
 
					pNode.p->setAttribute(CComBSTR(EXPECTED_ATTRIBUTE), CComVariant(arExpected[i]));
					
					pNode.p->setAttribute(CComBSTR(ACTUAL_ATTRIBUTE), CComVariant(arActual[i]));
					
					pNode.p->setAttribute(CComBSTR(DEVIATION_ATTRIBUTE), CComVariant(arDeviations[i]));
				}
			}
			pDom.p->get_xml(&strResult);
		}

		CloseDemoDialog(nIDD);
	}

	int nSize = strResult.Length() + 1;
	LPTSTR lpResult = (LPTSTR) CoTaskMemAlloc( nSize * sizeof(BSTR) );
	_tcscpy_s( lpResult, nSize, strResult );
	return lpResult;
}

//---------------------------------------------------------------------------
BOOL ExistDialog(LPCTSTR lpcszModule, UINT nIDD)
{ 
	if (!lpcszModule|| !lpcszModule[0]) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	AFX_MANAGE_STATE(AfxGetStaticModuleState());  

	HMODULE hMod = LoadModule(lpcszModule);
	HRSRC hResource = ::FindResource(hMod, MAKEINTRESOURCE(nIDD), RT_DIALOG);
	return hResource != NULL;
}
//---------------------------------------------------------------------------
BOOL ShowDemoDialog(HWND hParent, LPCTSTR lpcszXML, LPCTSTR lpcszModule, UINT nIDD)
{ 
	if (!lpcszXML || !lpcszXML[0] || !lpcszModule|| !lpcszModule[0]) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	AFX_MANAGE_STATE(AfxGetStaticModuleState());  

	if (!ExistDialog(lpcszModule, nIDD))
		return FALSE;

	//prendo il nome della dialog da utilizzare come titolo della finestra
	CString strTitle;
	CStringBlock tmpBlock;
	tmpBlock.ParseString(lpcszXML, strTitle);
	
	CDemoFrame* pFrame = CreateDemoWindow(hParent, strTitle, lpcszModule, nIDD);
	if (!pFrame) 
		return FALSE;
		
	CDemoView* pView = (CDemoView*)pFrame->GetActiveView ();
	if (pView && pView->RefreshDialog(&tmpBlock, TRUE))
		return TRUE;

	CloseDemoDialog(nIDD);
	return FALSE;
}

//---------------------------------------------------------------------------
BOOL CloseDemoDialog(UINT nIDD)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());  
	
	CWnd *pWnd = theApp.GetWndFromIDD(nIDD);
	if (pWnd) 
		return pWnd->DestroyWindow();
	
	return FALSE;
}