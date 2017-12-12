
#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

//==============================================================================
class TB_EXPORT CExtButtonExtendedInfo : public IOSLObjectManager
{
private:
	CWnd*		m_pParentWnd;

public:
	CExtButtonExtendedInfo (CWnd* pParentWnd, CInfoOSL* pParentInfoOSL)
		:
		IOSLObjectManager	(*pParentInfoOSL),
		m_pParentWnd		(pParentWnd)
	{
	}

public:
	CWnd*		GetParentWnd	() { return m_pParentWnd; }
	void		SetNamespace	(const CString& sName) { GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::CONTROL, sName, GetInfoOSL()->m_Namespace); }
};  

//=============================================================================
//			Class CExtButton
//=============================================================================
class TB_EXPORT CExtButton : public CButton, public CColoredControl, public CCustomFont, public IDisposingSourceImpl
{
	DECLARE_DYNCREATE (CExtButton)
	
	// all bitmaps must be the same size
	CWalkBitmap* m_pBitmapUp;		// normal image
	CWalkBitmap* m_pBitmapDown;		// selected image
	CWalkBitmap* m_pBitmapDisabled;	// disabled image
		
	CExtButtonExtendedInfo*	m_pExtInfo;

	// PNG
	UINT m_nPngIDStdImage;
	UINT m_nPngIDAltImage;
	UINT m_nPngCurrImage;
	// PNG NS
	CString m_sPngNSStdImage;
	CString m_sPngNSAltImage;
	CString m_sPngCurrImage;

public:
	BOOL	m_bUseImageSize;
	BOOL	m_bDrawFrame;

public:
	// Construction
	CExtButton();
	virtual ~CExtButton();

public:
	BOOL	LoadBitmaps	(UINT nBitmapResourceID);
	BOOL	LoadBitmaps	(const CString& sBitmapResourceName, const CString& sBitmapResourceDownName = _T(""), const CString& sBitmapResourceDisabledName = _T(""));

	void CExtButton::CalculateCheckboxAndRadioRect
		(
			const	CRect&		aResourceRect,
			CString&			strText,
			CRect&				rectCheck,
			DWORD&				dwCheckStyle,
			CRect&				rectText,
			DWORD&				dwTextStyle,
			CFont*				pFont = NULL
		);
	
	CExtButtonExtendedInfo*		GetExtInfo()								 { return m_pExtInfo; }
	void						SetExtInfo(CExtButtonExtendedInfo* pExtInfo) { m_pExtInfo = pExtInfo; }
	void SetPngImages		(UINT nIDStdImage, UINT nIDAltImage = 0);
	void SetPngImages		(CString sNSStdImage, CString sNSAltImage = _T(""));
	void ShowAltImage		(BOOL bIsAlt = TRUE);
	
	virtual BOOL ForceUpdateCtrlView();

protected:
	virtual	void DrawPNGItem	(LPDRAWITEMSTRUCT lpDIS);
	virtual void DrawItem		(LPDRAWITEMSTRUCT lpDIS);

	HBRUSH CtlColor(CDC* pDC, UINT nCtlColor) { return CColoredControl::CtlColor(pDC, nCtlColor); }

	afx_msg	void	OnSetFocus		(CWnd* pOldWnd);
	afx_msg void	OnWindowPosChanging	(WINDOWPOS FAR* lpwndpos);
	afx_msg LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	UINT OnGetDlgCode();
	void OnChar(UINT nChar, UINT nRepCnt, UINT nFlags);
	void OnKeyDown(UINT nChar, UINT nPreCnt, UINT nFlags);
	DECLARE_MESSAGE_MAP()
};
	
//=============================================================================
//		class CFrameToolTip
//=============================================================================
class TB_EXPORT CFrameToolTip : public CBCGPToolTipCtrl
{
	DECLARE_MESSAGE_MAP();

public:
	CFrameToolTip();

	void SetToolTip(CWnd* pWnd, CString strText);

protected:
	//{{AFX_MSG(CFrameToolTip)
	afx_msg void OnPop(NMHDR* pNMHDR, LRESULT* pResult);
	//}}AFX_MSG
};

//=============================================================================
//			Class CPaneButton	- Button for status bar
//=============================================================================
class TB_EXPORT CPaneButton : public CExtButton
{
	DECLARE_DYNAMIC(CPaneButton)

	CPaneButton();
	~CPaneButton();

public:
	void SetToolTip(CString strText);

private:
	Gdiplus::Bitmap* m_pBitmap;
	CFrameToolTip	 m_ToolTip;

public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);

protected:
	virtual	void DrawPNGItem(LPDRAWITEMSTRUCT lpDIS);
	void LoadButtonImage();

protected:
	//{{AFX_MSG(CParsedButton)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CParsedButton
//=============================================================================
class TB_EXPORT CParsedButton : public CExtButton, public CParsedCtrl
{
	DECLARE_DYNAMIC (CParsedButton)
	

protected:
	BOOL		m_bIsHovering = FALSE;

public:
	// Construction
	CParsedButton();
	CParsedButton(DataBool*);
	virtual ~CParsedButton()	{};

public:
	// explicit creation
	BOOL Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	
	// dynamic subclassing (don't use in conjunction with create)
	BOOL SubclassEdit(UINT nID, CWnd* pParent, const CString& strName = _T(""));

public:
	// pure virtual functions
	virtual	void		SetValue				(const DataObj& aValue)		= 0;
	virtual	void		GetValue				(DataObj& aValue)			= 0;
	virtual	void		RecalculatePaintInfo	()	{}
	virtual void 	SetCtrlCaption(const CString& strCaption) {}
	virtual void	SetCtrlCaption(const CString& strCaption, VerticalAlignment vAlign, CaptionPosition ePosition, int nCaptionWidth) {}

	virtual DataType	GetDataType		()	const	{ return DataType::Bool; }

private:
	void RemoveHovering ();

protected:
	virtual	BOOL		OnCommand					(WPARAM wParam, LPARAM lParam);


protected:
	//{{AFX_MSG(CParsedButton)
	afx_msg	void OnEnable		(BOOL bEnable);
	afx_msg	void OnKillFocus	(CWnd*);
	afx_msg void OnLButtonDown	(UINT nFlag, CPoint ptMousePos);
	afx_msg void OnContextMenu	(CWnd* pWnd, CPoint ptMousePos);
	afx_msg void OnCancelMode();
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);

	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CBoolButton
//=============================================================================
class TB_EXPORT CBoolButton : public CParsedButton
{
	DECLARE_DYNCREATE (CBoolButton)

protected:
	BOOL		m_bManualChecking;

public:
	// Construction
	CBoolButton();
	CBoolButton(DataBool*);

public:
	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	GetValue		(DataObj& aValue);
	virtual	BOOL	OnInitCtrl		();
	virtual	void	DoEnable		(BOOL bEnable);
	virtual	CSize	AdaptNewSize	(UINT, UINT, BOOL bButtonsIncluded);

	void		SetValue			(BOOL);
	BOOL		GetValue			();
	virtual		void				RecalculatePaintInfo	() { m_bFirstPaint = TRUE; }

private:
	BOOL	NeedCustomPaint();
	void	DrawFocusRectForChildWindows();

private:
	CRect		m_rectCheck;
	DWORD		m_dwCheckStyle;
	CRect		m_rectText;
	DWORD		m_dwTextStyle;
	BOOL		m_bFirstPaint;

protected:
	//{{AFX_MSG(CBoolButton)
	afx_msg LRESULT OnBtnSetCheck	(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnSetText		(WPARAM wParam, LPARAM lParam);
	afx_msg	void 	OnPaint			();
	afx_msg	void 	OnNcPaint			();
	afx_msg	BOOL	OnEraseBkgnd	(CDC* pDC);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CBoolButtonStatic
//=============================================================================
class TB_EXPORT CBoolButtonStatic : public CBoolButton
{
	DECLARE_DYNCREATE(CBoolButtonStatic)

public:
	// Construction
	CBoolButtonStatic();

public:
	virtual void Attach(DataObj* pDataObj);
};

//=============================================================================
//				Class CPushButton
// Cambia lo stato di abilitazione/disabilitazione sulla base del VALORE del
// dato associato
//=============================================================================
class TB_EXPORT CPushButton : public CParsedButton
{
	DECLARE_DYNCREATE (CPushButton)
	
public:
	// Construction
	CPushButton();
	CPushButton(DataBool*);

public:
	virtual	void	SetValue			(const DataObj& aValue);
	virtual	void	GetValue			(DataObj& aValue);
	virtual	BOOL	OnInitCtrl			();
	
	// non fa nulla poiche` e` sensibile solo al valore
	virtual	void	UpdateCtrlStatus	() {}
	virtual	BOOL	EnableCtrl			(BOOL bEnable  = TRUE);
	virtual BOOL	ShowCtrl			(int nCmdShow);

public:
	void	SetValue		(BOOL);
	BOOL	GetValue		();

protected:
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CChildButton
// this class is used only to managing Enumeration data type
//=============================================================================
class TB_EXPORT CChildButton : public CParsedButton
{
	DECLARE_DYNAMIC (CChildButton)
	friend class CParsedGroupBtn;
	friend class CEnumButton;
	
protected:
	WORD				m_wEnumItem;
	CParsedGroupBtn*	m_pOwnerGroupBtn;
	BOOL				m_bAttached	;
	UINT				m_nID;

public:
	// Constructors
	CChildButton(CParsedGroupBtn*, WORD wItem, UINT nID);
	virtual ~CChildButton();

public:
	// explicit creation
	BOOL Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	
	// dynamic subclassing (don't use in conjunction with create)
	BOOL SubclassEdit(UINT nID, CWnd* pParent);

	// to get attached CParsedGroupBtn
	CParsedGroupBtn*	GetOwnerGroupBtn	()	const				{ return m_pOwnerGroupBtn; }
	//void				SetOwnerGroupBtn	(CParsedGroupBtn* pGb)	{ m_pOwnerGroupBtn = pGb; }

	// pure virtual functions
	virtual	void SetValue(const DataObj& aValue)	{}
	virtual	void GetValue(DataObj& aValue)			{}
	virtual	void RecalculatePaintInfo()				{ m_bFirstPaint = TRUE; }

	virtual	void DoEnable(BOOL bEnable);

private:
	BOOL	NeedCustomPaint();

private:
	CRect	m_rectCheck;
	DWORD	m_dwCheckStyle;
	CRect	m_rectText;
	DWORD	m_dwTextStyle;
	BOOL	m_bNeedEdge;
	BOOL	m_bFirstPaint;

protected:
	//{{AFX_MSG(CChildButton)
	afx_msg	void 	OnKillFocus		(CWnd*);
	afx_msg LRESULT OnBtnSetCheck	(WPARAM wParam, LPARAM lParam);
	afx_msg void	OnContextMenu	(CWnd* pWnd, CPoint ptMousePos);
	afx_msg	void 	OnPaint			();
	afx_msg	void	OnEnable		(BOOL bEnable);
	afx_msg	void	OnFind			();
	//}}AFX_MSG	
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CParsedGroupBtn
//=============================================================================
class TB_EXPORT CGroupBoxBtn : public CExtButton
{
	friend class CParsedForm;

	DECLARE_DYNAMIC (CGroupBoxBtn)
public:
	CArray<HWND> m_arVirtualChilds;

public:
	CGroupBoxBtn();

	void SetZOrderInnerControls (CWnd* pParentWnd= NULL);

protected:	
	void	PaintGroupBox			();

	afx_msg	void 	OnPaint			();
	afx_msg	BOOL	OnEraseBkgnd	(CDC* pDC);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CParsedGroupBtn
//=============================================================================
class TB_EXPORT CParsedGroupBtn : public CGroupBoxBtn, public CParsedCtrl
{
	DECLARE_DYNAMIC (CParsedGroupBtn)
	friend class CChildButton;
	
protected:      
	Array	m_ButtonAssociations;
	WORD	m_wTag;
	BOOL	m_bManualChecking;
	
public:
	// Construction
	CParsedGroupBtn();

public:
	// explicit creation
	BOOL Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	
	// dynamic subclassing (don't use in conjunction with create)
	BOOL SubclassEdit(UINT nID, CWnd* pParent, const CString& strName = _T(""));

public:
	// pure virtual functions
	virtual	void		SetValue		(const DataObj& aValue)		= 0;
	virtual	void		GetValue		(DataObj& aValue)			= 0;
	
	virtual DataType	GetDataType		()					const	= 0;
	virtual	void		DoEnable		(BOOL bEnable);

public:
	CChildButton*	AddAssociation			(UINT nID, DWORD dwTagItem);
	CChildButton*	GetAssociatedButton		(UINT nID);
	
	void	SetTagValue		(WORD wTag)					{ m_wTag = wTag; }

public:
	const   WORD& GetTagValue() const	{ return m_wTag; }
			
public:
	// virtual function re-implemented for this specified control
	virtual CWnd*	SetCtrlFocus		(BOOL bSetSel = FALSE);
	virtual void	SetCtrlFont			(CFont*, BOOL bRedraw = TRUE);
	virtual	BOOL	SetCtrlPos			(const CWnd* pWndInsertAfter, int x, int y, int cx, int cy, UINT nFlags);
	virtual BOOL	ShowCtrl			(int nCmdShow);

	virtual	void	DoKillFocus 		(CWnd* aWnd);
	virtual void 	SetCtrlCaption(const CString& strCaption) {}
	virtual void	SetCtrlCaption(const CString& strCaption, VerticalAlignment vAlign, CaptionPosition ePosition, int nCaptionWidth) {}
	
protected:	
	//{{AFX_MSG(CParsedGroupBtn)
	afx_msg	void	OnEnable	(BOOL bEnable);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CEnumButton
//=============================================================================
class TB_EXPORT CEnumButton : public CParsedGroupBtn
{
	DECLARE_DYNCREATE (CEnumButton)

public:
	// Construction
	CEnumButton();
	CEnumButton(DataEnum*);

public:
	// virtual functions
	virtual void		Attach			(DataObj*);

	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);

	virtual DataType	GetDataType		()	const { return DataType(DATA_ENUM_TYPE, m_wTag); }

protected:
	//{{AFX_MSG(CEnumButton)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

///////////////////////////////////////////////////////////////////////////////


//=============================================================================
#include "endh.dex"

