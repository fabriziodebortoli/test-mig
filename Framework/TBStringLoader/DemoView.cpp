

#include "stdafx.h"

#include <afxtempl.h>
#include <atlbase.h>
#include <atlwin.h>

#include "TBStringLoader.h"
#include "StringLoader.h"
#include "DemoView.h"
#include "generic.h"
#include "const.h"


extern CTBStringLoaderApp theApp;

#define END_OF_STYLES		0xFFFFFFFFL
#define BUTTON_ID			0x0080
#define NEXT_IS_ID			0xFFFF
#define WRONG_COLOR			RGB(255,0,0) 
#define SEL_COLOR			RGB(0,0,255) 

static const UINT nBorderSize = 20;
// elenco di stili che non corrispondono a PushButton 
static const DWORD styles[] = 
	{
		BS_CHECKBOX,         
		BS_AUTOCHECKBOX,
		BS_RADIOBUTTON,    
		BS_3STATE,     
		BS_AUTO3STATE,
		BS_GROUPBOX,
		BS_USERBUTTON,
		BS_AUTORADIOBUTTON,
		BS_OWNERDRAW, 
		BS_LEFTTEXT,
		END_OF_STYLES
	};

//=============================================================================
// CBorderWnd
//=============================================================================
class CBorderWnd : public CWnd
{
	COLORREF	m_Color;

public:
	CBorderWnd(COLORREF aColor) {m_Color = aColor;}
	~CBorderWnd() {}

	DECLARE_MESSAGE_MAP()
	afx_msg void OnPaint();
};

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBorderWnd, CWnd)
	ON_WM_PAINT()
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
void CBorderWnd::OnPaint()
{
	CPaintDC dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente
	
	CRect r;  
	GetClientRect(r);  

	CPen aPen;
	aPen.CreatePen(PS_SOLID, 8, m_Color); 
	CPen *pOldPen = dc.SelectObject(&aPen);
	
	CGdiObject *pOldBrush = dc.SelectStockObject(NULL_BRUSH);

	dc.Rectangle(r);
	
	dc.SelectObject(pOldPen);
	dc.SelectObject(pOldBrush);

	aPen.DeleteObject();

	// Do not call CWnd::OnPaint() for painting messages
}

//=============================================================================
// CDemoFrame
//=============================================================================

//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDemoFrame, CFrameWnd)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDemoFrame, CFrameWnd)
	ON_WM_PAINT()
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDemoFrame::CDemoFrame(UINT nResID, HMODULE hModule)
{
	m_nResID =	nResID;
	m_hModule =	hModule;
}

//----------------------------------------------------------------------------
CDemoFrame::~CDemoFrame()
{
	theApp.RemoveWindow(this);
}

//----------------------------------------------------------------------------
void CDemoFrame::OnPaint()
{
	CPaintDC dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente

	// Do not call CFrameWnd::OnPaint() for painting messages
}

//=============================================================================
// CDemoView
//=============================================================================

//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDemoView, CView)

//----------------------------------------------------------------------------
CDemoView::CDemoView()
:
m_pDemoDialog(NULL)
{
}

//----------------------------------------------------------------------------
CDemoView::~CDemoView()
{
	UnmarkControls();

	if (m_pDemoDialog) 
	{
		m_pDemoDialog->DestroyWindow();
		delete m_pDemoDialog;
	}
}

//----------------------------------------------------------------------------
void CDemoView::OnInitialUpdate()
{
	CView::OnInitialUpdate();
}

//----------------------------------------------------------------------------
BOOL CDemoView::RefreshDialog(CStringBlock *pBlock, BOOL bPositionFrame)
{
	CDemoFrame* pFrame = (CDemoFrame*) GetParentFrame (); 
	ASSERT(pFrame);
	
	ShowWindow(SW_HIDE); 

	if (!CreateDemoDialog(pBlock, pFrame))
		return FALSE;	

	if (bPositionFrame) PlaceFrame();

	PlaceDialog(); 
	MarkControls();
	
	m_pDemoDialog->ShowWindow(SW_SHOWNORMAL); 

	ShowWindow(SW_SHOW); 

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CDemoView::CreateDemoDialog(CStringBlock *pBlock, CDemoFrame* pFrame)
{
	if (m_pDemoDialog) 
	{
		m_pDemoDialog->DestroyWindow();
		delete m_pDemoDialog;
	}

	m_pDemoDialog = new CDemoDialog();
		
	if (!m_pDemoDialog->Init(pBlock, pFrame, this))
	{
		m_pDemoDialog->DestroyWindow();
		delete m_pDemoDialog;
		m_pDemoDialog = NULL;
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
void CDemoView::PlaceFrame()
{
	if (!m_pDemoDialog) return;

	CDemoFrame* pFrame = (CDemoFrame*) GetParentFrame ();

	CRect frameRect;
	pFrame->GetWindowRect(frameRect);
	
	CRect dialogRect;
	m_pDemoDialog->GetWindowRect(dialogRect);
		
	CRect viewRect;
	GetWindowRect(viewRect);

	// calcolo l'offset della view rispetto alla frame
	UINT dCX = frameRect.right - frameRect.left - viewRect.right + viewRect.left;
	UINT dCY = frameRect.bottom - frameRect.top - viewRect.bottom + viewRect.top;

	CPoint ptStart(frameRect.left, frameRect.top);
	CSize cSize (dialogRect.right - dialogRect.left + dCX + nBorderSize*2, dialogRect.bottom - dialogRect.top + dCY + nBorderSize*2);
	
	// controllo che la finestra non esca dallo schermo
	CPoint ptEnd = ptStart + cSize; 
	CSize cScreenSize(GetSystemMetrics (SM_CXVIRTUALSCREEN), GetSystemMetrics (SM_CYVIRTUALSCREEN) - 40 ); 
	if (ptEnd.y > cScreenSize.cy) 
		ptStart.y = max(0, ptStart.y-(ptEnd.y-cScreenSize.cy));

	if (ptEnd.x > cScreenSize.cx)
		ptStart.x = max(0, ptStart.x-(ptEnd.x-cScreenSize.cx));

	pFrame->SetWindowPos(NULL, 
					ptStart.x, 
					ptStart.y, 
					cSize.cx,  
					cSize.cy, 
					SWP_NOZORDER);
}

//----------------------------------------------------------------------------
void CDemoView::PlaceDialog()
{
	if (!m_pDemoDialog) return;

	CRect dialogRect;
	m_pDemoDialog->GetWindowRect(dialogRect);

	CRect viewRect;
	GetWindowRect(viewRect);

	CDemoFrame* pFrame = (CDemoFrame*) GetParentFrame ();
	ASSERT(pFrame);

	pFrame->ScreenToClient(viewRect);
	pFrame->ScreenToClient(dialogRect);
	
	m_pDemoDialog->SetWindowPos(NULL, 
						viewRect.left + nBorderSize, 
						viewRect.top + nBorderSize, 
						dialogRect.right - dialogRect.left,  
						dialogRect.bottom - dialogRect.top, 
						SWP_NOZORDER);
}

//----------------------------------------------------------------------------
void CDemoView::UnmarkControls()
{
	for (int i = 0; i<m_arBorderWnds.GetSize(); i++)
	{
		CBorderWnd *pWnd = (CBorderWnd*) m_arBorderWnds[i];
		ASSERT_KINDOF(CBorderWnd, pWnd);
		pWnd->DestroyWindow();
		delete pWnd;
	}

	m_arBorderWnds.RemoveAll();
}

//----------------------------------------------------------------------------
void CDemoView::MarkControls()
{
	if (!m_pDemoDialog) return;

	UnmarkControls();
	
	for (int i=0; i<m_pDemoDialog->m_NonLocalizedWindows.GetSize(); i++)
	{
		DrawBorder(CWnd::FromHandle ((HWND)m_pDemoDialog->m_NonLocalizedWindows[i]), WRONG_COLOR);
	}

	DrawBorder(CWnd::FromHandle (m_pDemoDialog->m_hwndCurrentSelection), SEL_COLOR);
}

//----------------------------------------------------------------------------
void CDemoView::OnDraw(CDC* pDC)
{ 
	
}

//----------------------------------------------------------------------------
void CDemoView::DrawBorder(CWnd *pWnd, COLORREF aColor)
{
	if (!pWnd) return;

	CRect r;
 	pWnd->GetWindowRect(r);
 	ScreenToClient(r);
	r.InflateRect(4,4);

	CBorderWnd *pBorderWnd = new CBorderWnd(aColor);
	if (!pBorderWnd->Create (theApp.m_strWindowClassName, _T(""), WS_VISIBLE, r, this, 0))
	{
		delete pBorderWnd;
		return;
	}

	m_arBorderWnds.Add(pBorderWnd);
}

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDemoView, CView)
END_MESSAGE_MAP()


// CDemoView diagnostics
//----------------------------------------------------------------------------
#ifdef _DEBUG
//----------------------------------------------------------------------------
void CDemoView::AssertValid() const
{
	CView::AssertValid();
}

//----------------------------------------------------------------------------
void CDemoView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
} 
#endif //_DEBUG


//=============================================================================
// CDemoDialog dialog
//=============================================================================

//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDemoDialog, CDialog)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDemoDialog, CDialog)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDemoDialog::CDemoDialog()
	:
	m_pBlock(NULL)
{
	m_WrongBrush.CreateSolidBrush(WRONG_COLOR);
	m_SelBrush.CreateSolidBrush(SEL_COLOR);
}

//----------------------------------------------------------------------------
BOOL CDemoDialog::Init(CStringBlock *pBlock, CDemoFrame *pFrame, CWnd* pParentOwner) 
{
	ASSERT(pFrame);
	
	m_pBlock = pBlock;

	LPDLGTEMPLATE lpDialogTemplate = NULL;
	
	//salvo il modulo corrente
	HINSTANCE hOldModule = AfxGetResourceHandle();

	//imposto il nuovo modulo per evitare il walking delle risorse
	AfxSetResourceHandle(pFrame->m_hModule);

	try
	{
		lpDialogTemplate = GetLocalDialogTemplate(pFrame->m_nResID);
		if (!lpDialogTemplate)
		{
			// ripristino il modulo originario
			AfxSetResourceHandle(hOldModule);

			return FALSE;
		}

		AdjustTemplate(lpDialogTemplate);

		if (!CreateIndirect(lpDialogTemplate))  
		{
			delete lpDialogTemplate;

			// ripristino il modulo originario
			AfxSetResourceHandle(hOldModule);

			return FALSE;
		}
	}
	catch(...)
	{
		delete lpDialogTemplate;

		// ripristino il modulo originario
		AfxSetResourceHandle(hOldModule);

		return FALSE;
	}
		
	delete lpDialogTemplate;

	// ripristino il modulo originario
	AfxSetResourceHandle(hOldModule);

	SetParent(pParentOwner);
	SetOwner(pParentOwner);
	
	return TRUE;
}

//----------------------------------------------------------------------------
void CDemoDialog::SetFont(CFont *pFont)
{
	CWnd* pwndChild = GetWindow (GW_CHILD);
	while (pwndChild)
	{
		if (pwndChild->m_hWnd != m_hWnd)
			pwndChild->SetFont(pFont, TRUE);
		pwndChild = pwndChild->GetNextWindow ();
	}
}

//----------------------------------------------------------------------------
CDemoDialog::~CDemoDialog()
{
	theApp.RemoveWindow(this);
	m_WrongBrush.DeleteObject();
	m_SelBrush.DeleteObject();
	//RemoveButtonItems();
}

//----------------------------------------------------------------------------
LPDLGTEMPLATE CDemoDialog::GetLocalDialogTemplate(UINT nResID)
{
	LPCTSTR lpszResource = MAKEINTRESOURCE(nResID);

	HINSTANCE hInst = AfxFindResourceHandle(lpszResource, RT_DIALOG);
	HRSRC hResource = ::FindResource(hInst, lpszResource, RT_DIALOG);

	if (!hResource) return NULL;

	HGLOBAL hTemplate = LoadResource(hInst, hResource); 
	
	LPCDLGTEMPLATE pTemplate =(LPCDLGTEMPLATE)LockResource(hTemplate);
	
	SIZE_T nBytes = 0;

	//scorro il template per calcolarne la fine (e quindi la dimensione)
	//e intanto prendo tutti gli ID dei bottoni
	DLGITEMTEMPLATE *pItem = _DialogSplitHelper::FindFirstDlgItem (pTemplate);	
	BOOL bIsDialogEx = _DialogSplitHelper::IsDialogEx(pTemplate);
	for (int i=0; i<_DialogSplitHelper::DlgTemplateItemCount (pTemplate); i++)
		pItem = _DialogSplitHelper::FindNextDlgItem(pItem, bIsDialogEx);

	nBytes = (BYTE*)pItem - (BYTE*)pTemplate;
			
	LPDLGTEMPLATE lpDialogTemplate = (LPDLGTEMPLATE) new BYTE[nBytes];
	memcpy(lpDialogTemplate, pTemplate, nBytes);
	
	UnlockResource(hTemplate);
	FreeResource(hTemplate);

	return lpDialogTemplate;
}

//----------------------------------------------------------------------------
void CDemoDialog::AdjustTemplate(LPDLGTEMPLATE pTemplate)
{
	ASSERT(pTemplate);

	BOOL bIsDialogEx = _DialogSplitHelper::IsDialogEx(pTemplate);

	if (bIsDialogEx)
	{
		((_DialogSplitHelper::DLGTEMPLATEEX*) pTemplate)->style = 
			((_DialogSplitHelper::DLGTEMPLATEEX*) pTemplate)->style & ~WS_VISIBLE;
	}
	else
		pTemplate->style = pTemplate->style & ~WS_VISIBLE;
}

//----------------------------------------------------------------------------
BOOL CDemoDialog::OnInitDialog()
{
	__super::OnInitDialog();

	EnableWindow(FALSE);

	SetFont(&theApp.m_FormFont);

	if (m_pBlock)
		RefreshStrings(this);

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}

//----------------------------------------------------------------------------
void CDemoDialog::RefreshStrings(CWnd* pWnd)
{	
	CStringLoader *pStrLoader = theApp.GetGlobalStringLoader();

	if (!pStrLoader || !m_pBlock) return;
	
	m_hwndCurrentSelection = 0;					
	
	CString strCurrent;			
	CStringItem * pItem = m_pBlock->FindByAttributeValue(CURRENT_ATTRIBUTE, XML_TRUE, strCurrent);
	if (pItem)
	{
		if (IsCurrentControl(this, strCurrent))
		{
			m_hwndCurrentSelection = m_hWnd;
		}
		else
		{
			CWnd* pwndChild = GetWindow(GW_CHILD);				
			while (pwndChild)
			{
				if (IsCurrentControl(pwndChild, strCurrent))
				{
					m_hwndCurrentSelection = pwndChild->m_hWnd;
					break;
				}
						
				pwndChild = pwndChild->GetNextWindow ();
			} 
		}
	}

	pStrLoader->LoadWindowStrings(pWnd, m_pBlock, TRUE); 
}

//----------------------------------------------------------------------------
void CDemoDialog::AddNotFoundString(CWnd* pWnd)
{
	m_NonLocalizedWindows.Add(pWnd->m_hWnd);	
}

//----------------------------------------------------------------------------
BOOL CDemoDialog::IsCurrentControl(CWnd* pWnd, const CString& strCurrentText)
{
	CString strText, strTmpCurrentText(strCurrentText);
	pWnd->GetWindowText(strText);
	if (strText == strTmpCurrentText) return TRUE;
	
	strText = strText.Trim();
	strTmpCurrentText = strTmpCurrentText.Trim();
	if (strText == strTmpCurrentText) return TRUE;
	
	strText.Replace(_T("\r\n"), _T("\n"));
	strTmpCurrentText.Replace(_T("\r\n"), _T("\n"));

	if (strText == strTmpCurrentText) return TRUE;

	return FALSE;

}
