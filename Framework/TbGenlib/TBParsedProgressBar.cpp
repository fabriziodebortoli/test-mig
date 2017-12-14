#include "StdAfx.h"
#include "TBParsedProgressBar.h"
#include "BaseFrm.hjson"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//					CParsedProgressBar
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNCREATE (CParsedProgressBar, CBCGPProgressCtrl)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CParsedProgressBar, CBCGPProgressCtrl)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CParsedProgressBar::CParsedProgressBar()	
	:
	CParsedCtrl		(NULL)
{
	CParsedCtrl::Attach(this);
}

//-----------------------------------------------------------------------------
CParsedProgressBar::~CParsedProgressBar()
{
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedProgressBar::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	BOOL bOk = CheckControl(nID, pParentWnd) && CBCGPProgressCtrl::Create(dwStyle, rect, pParentWnd, nID);
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CParsedProgressBar::SubclassEdit (UINT nID, CWnd* pParent, const CString& strName)
{
	BOOL bOk =
			CheckControl(nID, pParent, _T("msctls_progress32"))	&&
			SubclassDlgItem(nID, pParent) &&			
			InitCtrl();

	if (bOk)
		SetNamespace(strName);

	CBCGPProgressCtrl::SetRange(0, 100);
	CBCGPProgressCtrl::SetStep(1);

	return TRUE;
}

//-----------------------------------------------------------------------------
DataType CParsedProgressBar::GetDataType() const
{
	return DATA_INT_TYPE;
}

//-----------------------------------------------------------------------------
void CParsedProgressBar::SetMinMaxRange(int min, int max)
{
	CBCGPProgressCtrl::SetRange(min, max);
}

//-----------------------------------------------------------------------------
void CParsedProgressBar::SetValue(const DataObj& aValue)
{
	if (aValue.GetDataType() != DATA_INT_TYPE)
	{
		ASSERT_TRACE(FALSE, " CParsedProgressBar::SetValue error: dataobj is not a DataInt");
		return;
	}
	
	int min, max;
	this->GetRange(min, max);
	
	int current = (int)(DataInt&) aValue;
	if (current > max || current < min)
		return;

	this->SetPos(current);
}

//-----------------------------------------------------------------------------
void CParsedProgressBar::GetValue(DataObj& aValue)
{
	if (aValue.GetDataType() != DATA_INT_TYPE)
	{
		ASSERT_TRACE(FALSE, " CParsedProgressBar::GetValue error: dataobj is not a DataInt");
		return;
	}
	((DataInt &)aValue).Assign(this->GetPos());
}

//-----------------------------------------------------------------------------
void CParsedProgressBar::DrawBodyEditProgressBar(CDC* pDC, CRect& rectProgress)
{ 
	//salvo il valore vecchio dello sfondo, perchè la draw della progress bar lo altera
	COLORREF oldColor = pDC->GetBkColor();
	
	BOOL bInfiniteMode = (GetStyle() & PBS_MARQUEE) != 0;

	int nMin = 0;
	int nMax = 100;
	int nPos = m_nMarqueeStep;
	
	if (!bInfiniteMode)
	{
		GetRange (nMin, nMax);
		nPos = GetPos();
	}
	
	CRect rectChunk(0, 0, 0, 0);
	CBCGPDrawOnGlass dog (m_bOnGlass);

	CBCGPRibbonProgressBar dummy(0, 0, 0, (GetStyle() & PBS_VERTICAL) != 0);
	dummy.SetRange(nMin, nMax);
	dummy.SetPos(nPos, FALSE);
	dummy.SetInfiniteMode(bInfiniteMode);
	dummy.CalculateChunkRect(rectProgress, rectChunk);
	
	CBCGPVisualManager::GetInstance()->OnDrawRibbonProgressBar(pDC, &dummy, rectProgress, rectChunk, bInfiniteMode);

	//ripristino il vecchio colore di sfondo
	pDC->SetBkColor(oldColor);
}

//-----------------------------------------------------------------------------
BOOL CParsedProgressBar::OwnerDraw(CDC* pDC, CRect& rectProgress, DataObj* dataObj)
{ 
	SetValue(*dataObj);
	dataObj->SetReadOnly(TRUE); 
	
	DrawBodyEditProgressBar(pDC, rectProgress);

	return TRUE; 
} 

/////////////////////////////////////////////////////////////////////////////
//					CTBProgressBarButton
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CTBProgressBarButton, CBCGPButton)

BEGIN_MESSAGE_MAP(CTBProgressBarButton, CBCGPButton)
	//{{AFX_MSG_MAP(CTBStatusBarProgressBar)
	ON_WM_ERASEBKGND()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTBProgressBarButton::CTBProgressBarButton() :
	m_nWidthSize	(0),
	m_nPane			(0),
	m_pStatusBar	(NULL),
	m_pDialog		(NULL)
{
}

//-----------------------------------------------------------------------------
CTBProgressBarButton::CTBProgressBarButton(CTaskBuilderStatusBar* pParent, int nPane) :
	m_nWidthSize(0),
	m_nPane(nPane),
	m_pStatusBar(pParent),
	m_pDialog		(NULL)
{
	Create();
	ResizeAndPosition();
}

//-----------------------------------------------------------------------------
CTBProgressBarButton::~CTBProgressBarButton()
{
	if (m_pDialog && m_pDialog->m_hWnd)
		SAFE_DELETE(m_pDialog);
}

//-----------------------------------------------------------------------------
BOOL CTBProgressBarButton::Create()
{
	BOOL bRet = __super::Create(_T(""), (WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS), CRect(0, 0, 0, 0), m_pStatusBar, IDC_PROGBAR_BUTTON);
	CBitmap	bitmap;
	BITMAP bm;
	BOOL bImageLoaded = ::LoadBitmapOrPng(&bitmap, TBGlyph(szProgressBarList));
	bitmap.GetObject(sizeof(BITMAP), &bm);

	m_nWidthSize = bm.bmWidth;

	// Load Icon button
	CDC* pDC = GetDC();
	HICON ico = ::CBitmapToHICON(&bitmap, pDC, m_nWidthSize, FALSE, RGB(255, 255, 255), FALSE);
	ReleaseDC(pDC);

	SetImage(ico, 1, NULL);
	
	// delete bitmap
	bitmap.DeleteObject();
	return bRet;
}

//-----------------------------------------------------------------------------
BOOL CTBProgressBarButton::ResizeAndPosition()
{
	// Found for ID the last progress bar
	int nID = ID_STATUS_PROGBAR_RANGE_START;
	CWnd* pWndCtrl = NULL;
	for (int k = 0; k < MAX_STATUS_PROGBAR; k++)
	{
		CWnd* pWndCtrlTest = m_pStatusBar->GetDlgItem(nID);
		if (pWndCtrlTest == NULL) {
			break;
		}
		pWndCtrl = pWndCtrlTest;
		nID++;
	}
	ASSERT(pWndCtrl);
	CTBStatusBarProgressBar* pProgBar = dynamic_cast<CTBStatusBarProgressBar*>(pWndCtrl);
	ASSERT(pProgBar);
	CRect rectProgresBar;
	if (pProgBar)
	{
		rectProgresBar = pProgBar->GetRect();
	}

	CRect rButtonPos;
	int nIconWidth = GetIconWidth();
	if (nIconWidth <= 0) {
		// Icon standard 20px
		nIconWidth = 20;
	}

	CRect rc;
	m_pStatusBar->GetItemRect(m_nPane, rc);
	INT nCenter = rc.top + (rc.Height() / 2);

	// center to scrollbar
	rButtonPos.top = nCenter - (GetIconWidth() / 2);
	rButtonPos.bottom = nCenter + (GetIconWidth() / 2);
	rButtonPos.left = rectProgresBar.right + 10;
	rButtonPos.right = rButtonPos.left + 50;

	// rButtonPos.left = rc.left + 10;
	// rButtonPos.right = rButtonPos.left + 50;

	// Move button
	MoveWindow(&rButtonPos);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBProgressBarButton::OnEraseBkgnd(CDC* pDC)
{
	ResizeAndPosition();
	return __super::OnEraseBkgnd(pDC);
}

/////////////////////////////////////////////////////////////////////////////
//					CTBStatusBarProgressBar
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNCREATE(CTBStatusBarProgressBar, CParsedProgressBar)

BEGIN_MESSAGE_MAP(CTBStatusBarProgressBar, CParsedProgressBar)
	//{{AFX_MSG_MAP(CTBStatusBarProgressBar)
	ON_WM_ERASEBKGND()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTBStatusBarProgressBar::CTBStatusBarProgressBar() :
	m_pStatusBar(NULL),
	m_nHeight(10),
	m_nWidth(200),
	m_nPosBar(0)
{
	m_Rect.SetRect(0, 0, 0, 0);
}

//-----------------------------------------------------------------------------
CTBStatusBarProgressBar::CTBStatusBarProgressBar(LPCTSTR strMessage, int nSize /*=100*/,
	int MaxValue /*=100*/, BOOL bSmooth /*=FALSE*/,
	int nPane /*=0*/, CTaskBuilderStatusBar* pBar /*=NULL*/) :
	m_pStatusBar(pBar),
	m_nHeight(10),
	m_nWidth(200),
	m_nPosBar(0)
{
	Create(strMessage, nSize, MaxValue, bSmooth, nPane);
}

//-----------------------------------------------------------------------------
CTBStatusBarProgressBar::CTBStatusBarProgressBar(int nSize, int MaxValue, int nPane, CTaskBuilderStatusBar* pBar) :
	m_pStatusBar(pBar),
	m_nHeight(10),
	m_nWidth(200),
	m_nPosBar(0)
{
	Create(_TB("Progress: "), nSize, MaxValue, FALSE, nPane);
}

//-----------------------------------------------------------------------------
CTBStatusBarProgressBar::~CTBStatusBarProgressBar()
{
	Clear();
}

//-----------------------------------------------------------------------------
CTaskBuilderStatusBar* CTBStatusBarProgressBar::GetStatusBar()
{
	if (m_pStatusBar)
	{
		return m_pStatusBar;
	}
	return NULL;
}

// Create the ProgressCtrl as a child of the status bar positioned
// over the first pane, extending "nSize" percentage across pane.
// Sets the range to be 0 to MaxValue, with a step of 1.
//-----------------------------------------------------------------------------
BOOL CTBStatusBarProgressBar::Create(LPCTSTR strMessage, int nSize /*=100*/,
	int MaxValue /*=100*/, BOOL bSmooth /*=FALSE*/,
	int nPane /*=0*/)
{
	BOOL bSuccess = FALSE;

	CTaskBuilderStatusBar *pStatusBar = GetStatusBar();
	if (!pStatusBar)
		return FALSE;

	DWORD dwStyle = WS_CHILD | WS_VISIBLE;
	 
	if (bSmooth) dwStyle |= PBS_SMOOTH;
	
	// Until m_nPane is initialized, Resize() must not be called. But it can be called (which 
	// happens in multi-threaded programs) in CTBStatusBarProgressBar::OnEraseBkgnd after the control is 
	// created in CTBStatusBarProgressBar::Create.
	m_nSize = nSize;
	m_nPane = nPane;
	m_strPrevText = pStatusBar->GetPaneText(m_nPane);
	SetText(strMessage);
	
	// Get CRect coordinates for requested status bar pane
	CRect rPaneRect;
	pStatusBar->GetItemRect(nPane, &rPaneRect);

	// Found a free ID for a progress bar
	int nID = ID_STATUS_PROGBAR_RANGE_START;
	BOOL bFound = FALSE;
	for (int k = 0; k < MAX_STATUS_PROGBAR; k++)
	{
		CWnd* pWndCtrl = pStatusBar->GetDlgItem(nID);
		if (pWndCtrl == NULL)
		{
			bFound = TRUE;
			break;
		}
		nID++;
		m_nPosBar++;
	}
	ASSERT_TRACE(bFound, "ID free not found.!")
	
	// Create the progress bar
	bSuccess = __super::Create(dwStyle, rPaneRect, pStatusBar, nID);
	ASSERT(bSuccess);
	if (!bSuccess)
		return FALSE;

	m_MarqueeStyle = BCGP_MARQUEE_DOTS;
	m_bVisualManagerStyle = TRUE;

	// Set range and step
	SetRange(0, MaxValue);
	SetStep(1);

	// Resize the control to its desired width
	Resize();

	if (m_pStatusBar) {
		m_pStatusBar->SetPaneWithProges();
	}	
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CTBStatusBarProgressBar::Clear()
{
	if (m_pStatusBar) {
		m_pStatusBar->SetPaneWithProges(FALSE);
	}

	if (!IsWindow(GetSafeHwnd()))
		return;

	// Hide the window. This is necessary so that a cleared
	// window is not redrawn if "Resize" is called
	ModifyStyle(WS_VISIBLE, 0);

	CString str;
	if (m_nPane == 0)
		str.LoadString(AFX_IDS_IDLEMESSAGE);   // Get the IDLE_MESSAGE
	else
		str = m_strPrevText;                   // Restore previous text

	// Place the IDLE_MESSAGE in the status bar 
	CTaskBuilderStatusBar *pStatusBar = GetStatusBar();
	if (pStatusBar)
	{
		pStatusBar->SetPaneText(m_nPane, str);
		pStatusBar->UpdateWindow();
	}
}

//-----------------------------------------------------------------------------
BOOL CTBStatusBarProgressBar::SetText(LPCTSTR strMessage)
{
	CTaskBuilderStatusBar *pStatusBar = GetStatusBar();
	if (pStatusBar)
	{
		m_strMessage = strMessage;
		pStatusBar->SetPaneText(m_nPane, m_strMessage);
	}

	return Resize();
}

//-----------------------------------------------------------------------------
BOOL CTBStatusBarProgressBar::SetSize(int nSize)
{
	m_nSize = nSize;
	return Resize();
}

//-----------------------------------------------------------------------------
BOOL CTBStatusBarProgressBar::SetRange(int nLower, int nUpper, int nStep /* = 1 */)
{
	if (!IsWindow(GetSafeHwnd()))
		return FALSE;

	SendMessage(PBM_SETRANGE32, (WPARAM)nLower, (LPARAM)nUpper);

	__super::SetStep(nStep);
	return TRUE;
}

//-----------------------------------------------------------------------------
int CTBStatusBarProgressBar::SetPos(int nPos)
{
	if (!IsWindow(GetSafeHwnd()))
		return 0;

	ModifyStyle(0, WS_VISIBLE);
	return __super::SetPos(nPos);
}

//-----------------------------------------------------------------------------
int CTBStatusBarProgressBar::OffsetPos(int nPos)
{
	if (!IsWindow(GetSafeHwnd()))
		return 0;

	ModifyStyle(0, WS_VISIBLE);
	return __super::OffsetPos(nPos);
}

//-----------------------------------------------------------------------------
int CTBStatusBarProgressBar::SetStep(int nStep)
{
	if (!IsWindow(GetSafeHwnd()))
		return 0;

	ModifyStyle(0, WS_VISIBLE);
	return __super::SetStep(nStep);
}

//-----------------------------------------------------------------------------
int CTBStatusBarProgressBar::StepIt()
{
	if (!IsWindow(GetSafeHwnd()))
		return 0;

	ModifyStyle(0, WS_VISIBLE);
	return __super::StepIt();
}

//-----------------------------------------------------------------------------
BOOL CTBStatusBarProgressBar::Resize()
{
	if (!IsWindow(GetSafeHwnd()))
		return FALSE;

	CTaskBuilderStatusBar *pStatusBar = GetStatusBar();
	if (!pStatusBar)
		return FALSE;

	// Redraw the window text
	if (IsWindowVisible())
	{
		pStatusBar->UpdateWindow();
	}

	// Calculate how much space the text takes up
	CClientDC dc(pStatusBar);
	CFont *pOldFont = dc.SelectObject(pStatusBar->GetFont());
	CSize size = dc.GetTextExtent(m_strMessage);		// Length of text
	int margin = dc.GetTextExtent(_T(" ")).cx * 2;		// Text margin
	dc.SelectObject(pOldFont);

	// Set the min size of text zone
	size.cx = max(size.cx, MAX_TEXT_WIDTH);

	// Used in pane drawing
	pStatusBar->SetPaneWithProgesTextZone(size.cx);

	// Now calculate the rectangle in which we will draw the progress bar
	CRect rc;
	pStatusBar->GetItemRect(m_nPane, rc);

	// Position left of progress bar after text and right of progress bar
	if (!m_strMessage.IsEmpty()) rc.left += (size.cx + 2 * margin);
	rc.left += margin + (m_nPosBar * (m_nWidth + margin * 2));
	
	// calculate the limit of right
	int nRightLimit = rc.right - (rc.right - rc.left) * (100 - m_nSize) / 100;
	rc.right = rc.left + m_nWidth;		
	if (rc.right > nRightLimit && nRightLimit > 0) {
		rc.right = nRightLimit;
	}

	ASSERT(rc.right > rc.left);
	
	// set scroll Bar Height 10px
	INT nHeight = m_nHeight / 2;
	INT nCenter = rc.top + (rc.Height() /2 );
	rc.top = nCenter - nHeight;
	rc.bottom = nCenter + nHeight;

	// If the window size has changed, resize the window
	if (rc != m_Rect)
	{
		MoveWindow(&rc);
		m_Rect = rc;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBStatusBarProgressBar::OnEraseBkgnd(CDC* pDC)
{
	Resize();
	return __super::OnEraseBkgnd(pDC);
}



