#pragma once

#include <TBGenlib\parsobj.h>
#include <TbGenlib\ExtStatusControlBar.h>
#include <TbGeneric\TBWebFriendlyMenu.h>

#include "beginh.dex"

#define MAX_TEXT_WIDTH						200
#define	MAX_STATUS_PROGBAR					10

#define	IDC_LOG_MESSAGE						GET_IDC(IDC_LOG_MESSAGE)
#define	IDC_PROGBAR_BUTTON					GET_IDC(IDC_BAR_BUTTON)
#define ID_STATUS_PROGBAR_RANGE_START		GET_ID_RANGE(ID_STATUSPROGBAR, MAX_STATUS_PROGBAR);

#define IDC_LOG_DATA						GET_ID(IDC_LOG_DATA)
#define WM_LOG_OPEN							GET_ID(WM_LOG_OPEN)
#define WM_LOG_UPDATE						GET_ID(WM_LOG_UPDATE)

//======================================================================
class TB_EXPORT CParsedProgressBar : public CBCGPProgressCtrl, public CParsedCtrl
{
	DECLARE_DYNCREATE(CParsedProgressBar);
	
public:
	CParsedProgressBar();
	~CParsedProgressBar();

	void SetMinMaxRange	(int min, int max);

	// pure virtual function
	virtual	BOOL		Create			(DWORD, const RECT&, CWnd*, UINT);
	virtual	BOOL		SubclassEdit	(UINT, CWnd*, const CString& strName = _T(""));
	virtual DataType	GetDataType		() const;
	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);
	virtual BOOL		OwnerDraw		(CDC*, CRect&, DataObj* = NULL);
	
private:
	void DrawBodyEditProgressBar(CDC* pDC, CRect& rectProgress);	

	DECLARE_MESSAGE_MAP();
};

/////////////////////////////////////////////////////////////////////////////
// CTBProgressBarButton 
class CTBProgressBarButton : public CBCGPButton
{
	DECLARE_DYNAMIC(CTBProgressBarButton)

public:
	CTBProgressBarButton();
	CTBProgressBarButton(CTaskBuilderStatusBar* pParent, int nPane);
	~CTBProgressBarButton();

	BOOL Create();
	INT	 GetIconWidth() { return m_nWidthSize; }
	BOOL ResizeAndPosition();

	void SetDialog(CWnd* pWnd) { m_pDialog = pWnd; }
protected:
	INT m_nWidthSize;
	INT m_nPane;
	CTBWebFriendlyMenu  m_Menu;
	CTaskBuilderStatusBar* const m_pStatusBar;
	CWnd* m_pDialog;

protected:
	//{{AFX_MSG(CTBProgressBarButton)
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CProgressBar -  status bar progress control
class CTBStatusBarProgressBar : public CParsedProgressBar
	// Creates a ProgressBar in the status bar
{

public:
	CTBStatusBarProgressBar();
	CTBStatusBarProgressBar(int nSize, int MaxValue, int nPane, CTaskBuilderStatusBar* pBar);
	CTBStatusBarProgressBar(LPCTSTR strMessage, int nSize = 100, int MaxValue = 100, BOOL bSmooth = FALSE, int nPane = 0, CTaskBuilderStatusBar* pBar = NULL);

	~CTBStatusBarProgressBar();
	BOOL Create(LPCTSTR strMessage, int nSize = 100, int MaxValue = 100,
		BOOL bSmooth = FALSE, int nPane = 0);

	DECLARE_DYNCREATE(CTBStatusBarProgressBar)

	void SetHeight(UINT nHeight) { m_nHeight = nHeight; }
	void SetWidth(UINT nWidth) { m_nWidth = nWidth;  }

	// operations
public:
	BOOL SetRange(int nLower, int nUpper, int nStep = 1);
	BOOL SetText(LPCTSTR strMessage);
	BOOL SetSize(int nSize);
	int  SetPos(int nPos);
	int  OffsetPos(int nPos);
	int  SetStep(int nStep);
	int  StepIt();
	void Clear();
	CRect GetRect() { return m_Rect; }

protected:
	int		m_nPosBar;				// position order in status bar
	int		m_nSize;				// Percentage size of control
	int		m_nPane;				// ID of status bar pane progress bar is to appear in
	CString	m_strMessage;			// Message to display to left of control
	CString m_strPrevText;			// Previous text in status bar
	CRect	m_Rect;					// Dimensions of the whole thing
	CTaskBuilderStatusBar* const m_pStatusBar; 

	CTaskBuilderStatusBar *GetStatusBar();
	BOOL Resize();

	UINT m_nHeight;
	UINT m_nWidth;

	// Generated message map functions
protected:
	//{{AFX_MSG(CProgressBar)
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"