
#pragma once
#include <afxhtml.h>

#include <TbGenlib\CEFClasses.h>
//includere alla fine degli include del .H
#include "beginh.dex"

class CBitmap;
//class CPicture;
class CTBPicture;
class CPictureStatic;
class CShowFileTextStatic;
class CParsedRichCtrl;
class CParsedWebCtrl;


//=============================================================================
class TB_EXPORT CParsedBitmap : public CParsedStatic
{
	DECLARE_DYNAMIC (CParsedBitmap)

protected:
	CBitmap*	m_pBitmap;

public:
	// Construction
	CParsedBitmap	(DataObj* pData = NULL);

public:
	// Overridables
	virtual DataType	GetDataType ()	const	{ return DATA_NULL_TYPE; }

public:
	virtual void	Attach		(DataObj*);

	// Overridables
	virtual	void		DrawImage	();
	virtual void		DrawBitmap	(CDC& DCDest, const CRect& rect);

	virtual	void		SetValue(LPCTSTR pszValue = NULL);

public:
	void	SetValue (CBitmap*);
	CSize	GetImageSize();

public:  
	virtual	CSize	AdaptNewSize	(UINT, UINT, BOOL bButtonsIncluded);

	CWndImageDescription* GetControlStructure(CString strId, CWndObjDescriptionContainer* pContainer);

protected:      
	void UpdateDescription(CWndImageDescription* pDesc);
	
	//{{AFX_MSG(CParsedBitmap)
	afx_msg LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);

	afx_msg void OnPaint();
	afx_msg void OnNcPaint();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:                 
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const {	CParsedStatic::AssertValid(); }
#endif // _DEBUG
};

//=============================================================================
//			Class CIntBitmap
//=============================================================================
class TB_EXPORT CNSBitmap : public CParsedBitmap
{
	DECLARE_DYNCREATE(CNSBitmap)

protected:
	CString		m_nsBitmap;

public:
	// Construction
	CNSBitmap();
	CNSBitmap(DataStr*);
	virtual ~CNSBitmap();

public:
	// Overridables
	virtual	void		SetValue(const DataObj& aValue);
	virtual	void		GetValue(DataObj& aValue);

	virtual DataType	GetDataType()	const	{ return DataType::String; }

	virtual void		DrawBitmap(CDC& DCDest, const CRect& rect);

public:
	// Static value management
	void	SetValue(CString sValue);
	CString	GetValue();

	// Diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext&)	const;
	void AssertValid()				const { CParsedBitmap::AssertValid(); }
#endif // _DEBUG
};

//=============================================================================
//			Class CIntBitmap
//=============================================================================
class TB_EXPORT CIntBitmap : public CParsedBitmap
{
	DECLARE_DYNCREATE (CIntBitmap)

protected:
	int			m_nIDBitmap;

public:
	// Construction
	CIntBitmap();
	CIntBitmap(DataInt*);
	virtual ~CIntBitmap();

public:
	// Overridables
	virtual	void		SetValue	(const DataObj& aValue);
	virtual	void		GetValue	(DataObj& aValue);

	virtual DataType	GetDataType ()	const	{ return DataType::Integer; }

	virtual void		DrawBitmap	(CDC& DCDest, const CRect& rect);

public:
    // Static value management
	void	SetValue	(int nValue);
	int		GetValue	();                       

// Diagnostics
#ifdef _DEBUG
public:                 
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const {	CParsedBitmap::AssertValid(); }
#endif // _DEBUG
};

//=============================================================================
//			Class CStateImage
//=============================================================================
class TB_EXPORT CParsedStateImage : public CParsedBitmap
{
	DECLARE_DYNCREATE(CParsedStateImage)

protected:
	class CStateImg
	{
	public:
		CBitmap		m_Bitmap;
		int			m_nIDBitmap;
		CTBNamespace	m_nsImage;
		CString		m_sText;
		CString		m_sTooltip;

		CStateImg(const CString& sText, const CString& sTooltip, int nIDBitmap = 0)
			: 
			m_sText		(sText), 
			m_sTooltip	(sTooltip),
			m_nIDBitmap	(nIDBitmap)
		{
		}

		CStateImg(const CString& sText, const CString& sTooltip, const CString& sNsImage)
			:
			m_sText		(sText),
			m_sTooltip	(sTooltip),
			m_nIDBitmap	(0),
			m_nsImage	(CTBNamespace::IMAGE, sNsImage)
		{
			ASSERT(m_nsImage.IsValid());
		}

		
		virtual ~CStateImg() { }
	};

	int			m_nImageID;

	CMap<int, int, CStateImg*, CStateImg*> m_mapStateImages;

public:
	// Construction
	CParsedStateImage();
	CParsedStateImage(DataInt*);
	virtual ~CParsedStateImage();

	void AddStateImg(const CString& sText, const CString& sTooltip, int nIDBitmap);
	void AddStateImg(const CString& sText, const CString& sTooltip, const CString& sNsImage, int nId);

public:
	// Overridables
	virtual	void		SetValue(const DataObj& aValue);
	virtual	void		GetValue(DataObj& aValue);

	virtual DataType	GetDataType()	const	{ return DataType::Integer; }

	virtual void		DrawBitmap(CDC& DCDest, const CRect& rect);

public:
	// Static value management
	void	SetValue(int nValue);
	int		GetValue();
	
	virtual CString FormatData(const DataObj* pDataObj, BOOL bEnablePadding) const;
	
	CString GetTooltip(const DataInt* pDataObj) const;

	// Diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext&)	const;
	void AssertValid()				const { CParsedBitmap::AssertValid(); }
#endif // _DEBUG
};

//=============================================================================
//			Class CLinkEdit
//=============================================================================
class TB_EXPORT CLinkEdit : public CStrEdit
{
	DECLARE_DYNCREATE (CLinkEdit)

protected:
	HCURSOR						m_hLinkCursor;
	CFont						m_fontUnderline;
	CFont						m_fontEdit;

	BOOL						m_bEnabledLink;
	CString						m_sPrefix;

public:
	CLinkEdit();
	CLinkEdit(UINT nBtnIDBmp, DataStr* = NULL);
	virtual ~CLinkEdit();

public:
	virtual	BOOL	IsValid				();
	virtual	BOOL	UpdateCtrlData		(BOOL bEmitError, BOOL bSendMessage = FALSE);
	virtual void	ModifiedCtrlData	();

	virtual	BOOL	OnInitCtrl			();
	virtual	void	SetColor			();

	virtual	void	DoEnable			(BOOL bEnable);
	virtual BOOL	EnableCtrl			(BOOL bEnable = TRUE);

			void	SetEnabledLink		(BOOL bEnabled = TRUE);
	virtual	BOOL	IsEnabledLink		() { return m_bEnabledLink; }

			void	SetPrefix			(LPCTSTR sPrefix) { m_sPrefix = sPrefix; }

	virtual  CString	OnCustomizeLink	(CString sValue);
	virtual  CWnd*		SetCtrlFocus(BOOL bSetSel = FALSE);

	virtual  void	OnBrowseLink	();

	virtual void	SetOwnFont		(CFont* pFont, BOOL bOwns = FALSE);
	virtual void	PostClickMessage();

	afx_msg BOOL	OnSetCursor		(CWnd* pWnd, UINT nHitTest, UINT message);
	afx_msg void	OnLButtonDown	(UINT nFlags, CPoint point);

	afx_msg void	OnContextMenu	(CWnd* pWnd, CPoint ptMousePos);
	afx_msg void	OnSetFocus		(CWnd* pWnd);

protected:
	
    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CPathEdit 
//=============================================================================
class TB_EXPORT CPathEdit : public CLinkEdit
{
	DECLARE_DYNCREATE (CPathEdit)

private:
	CString m_strDefaultDir;
	
public:
	// Construction
	CPathEdit();
	CPathEdit(UINT nBtnIDBmp, DataStr* = NULL);

public:
	BOOL	SetDefaultDir(LPCTSTR);
	CString	GetDefaultDir() const { return m_strDefaultDir;}

    // virtual functions
	virtual void	Attach		(DataObj*);

	virtual void	GetValue	(CString&);
	virtual CString GetValue	();
	virtual BOOL	IsValid		();
	virtual	BOOL	DoOnChar	(UINT nChar);
	virtual	BOOL	OnInitCtrl  ();
	
protected:
	//{{AFX_MSG(CPathEdit)
	afx_msg void OnDropFiles(HDROP hDropInfo);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CBrowsePathEdit 
//=============================================================================
class TB_EXPORT CBrowsePathEdit : public CPathEdit
{
	DECLARE_DYNCREATE(CBrowsePathEdit)

public:
	CBrowsePathEdit();

public:
	DataBool		m_bValue;
	LPCTSTR			m_lpszDefExt;
	LPCTSTR			m_lpszFileName;
	DWORD			m_dwFlags;
	CString			m_lpszFilter;
	CWnd*			m_pParentWnd;
	DWORD			m_dwSize;
	BOOL			m_bVistaStyle;
	DataStr			m_DlgTitle;
	DataBool		m_bUseFileDlg;

public:
	virtual	void	DoPushButtonCtrl(WPARAM wParam, LPARAM lParam);

	LPCTSTR			GetExtension	()	{ return m_lpszDefExt; }
	LPCTSTR			GetFilter		()	{ return m_lpszFileName; }
	DWORD			GetFlags		()	{ return m_dwFlags; }
	DataStr			GetDlgTitle		()	{ return m_DlgTitle; }
	CString			GetFilterText	()	{ return m_lpszFilter; }
	CWnd*			GetParentWnd	()	{ return m_pParentWnd; }
	DWORD			GetSize			()	{ return m_dwSize; }
	BOOL			GetVistaStyle	()	{ return m_bVistaStyle; }
	DataBool		GetUseFileDlg	()	{ return m_bUseFileDlg; }

	void			SetExtension	(LPCTSTR aExtension)		{ m_lpszDefExt = aExtension; }
	void			SetFilter		(LPCTSTR aFilter)			{ m_lpszFileName = aFilter; }
	void			SetFlags		(DWORD aFlags)				{ m_dwFlags = aFlags; }
	void			SetDlgTitle		(DataStr aDlgTitle)			{ m_DlgTitle = aDlgTitle; }
	void			SetFilterText	(CString aFilterText)		{ m_lpszFilter = aFilterText; }
	void			SetParentWnd	(CWnd * aParentWnd)			{ m_pParentWnd = aParentWnd; }
	void			SetSize			(DWORD aSize)				{ m_dwSize = aSize; }
	void			SetVistaStyle	(BOOL bVistaStyle)			{ m_bVistaStyle = bVistaStyle; }
	void			SetUseFileDlg	(BOOL bUseFileDlg = TRUE)	{ m_bUseFileDlg = bUseFileDlg; }

public:
	virtual BOOL	OnInitCtrl	();
};

//=============================================================================
//			Class CPhoneEdit
//=============================================================================
class TB_EXPORT CPhoneEdit : public CLinkEdit
{
	DECLARE_DYNCREATE (CPhoneEdit)
protected:
	DataStr* m_pISOCode;
	int		m_nDataIdxParam;

public:
	CPhoneEdit ();
	CPhoneEdit (UINT nBtnIDBmp, DataStr* = NULL);

	virtual	int	GetParamDataIdx		() const { return m_nDataIdxParam; }

	virtual	void BindParam (DataObj* param, int nDataIdxParam = -1) 
		{ 
			ASSERT_KINDOF(DataStr, param);
				m_pISOCode = (DataStr*) param; 
			m_nDataIdxParam = nDataIdxParam;
		}

	virtual CString OnCustomizeLink			(CString sValue);

	static	CString LookupTelephonePrefix	(DataStr* pISOCode);

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CEmailAddressEdit
//=============================================================================
class TB_EXPORT CEmailAddressEdit : public CLinkEdit
{
	DECLARE_DYNCREATE (CEmailAddressEdit)
public:
	CEmailAddressEdit();
	CEmailAddressEdit(UINT nBtnIDBmp, DataStr* = NULL);

	virtual void Attach (UINT /*nBtnID*/);
	virtual CString OnCustomizeLink			(CString sValue);

};

//=============================================================================
//			Class CNamespaceEdit
//=============================================================================
class TB_EXPORT CNamespaceEdit : public CLinkEdit
{
	DECLARE_DYNCREATE (CNamespaceEdit)

protected:
	CTBNamespace::NSObjectType	m_NsType;
	CTBNamespace				m_Ns;

	CPictureStatic*				m_pPictureStatic;
	CShowFileTextStatic*		m_pShowFileTextStatic;
	CParsedRichCtrl*			m_pRichCtrl;
	CParsedWebCtrl*				m_pWebCtrl;

	BOOL						m_bIsRunning;

	DataStr*					m_pParam;
	int							m_nDataIdxParam;

public:
	// Construction
	CNamespaceEdit();
	CNamespaceEdit(UINT nBtnIDBmp, DataStr* = NULL);

public:
	CTBNamespace::NSObjectType	GetNamespaceType	() const { return m_NsType;}
	void						SetNamespaceType	(CTBNamespace::NSObjectType NsType);
	void						SetNamespace		(CTBNamespace	Ns) {m_Ns = Ns;}

	void	AttachPicture		(CPictureStatic*		pPictureStatic);
	void	AttachFileText		(CShowFileTextStatic*	pShowFileTextStatic);
	void	AttachRichCtrl		(CParsedRichCtrl*		pRichCtrl);
	void	AttachWebCtrl		(CParsedWebCtrl*		pWebCtrl);

	virtual BOOL	IsValid				();

	BOOL	IsRunning					()			{ return m_bIsRunning; }
	
	void	Attach		(UINT /*nBtnID*/)			{ m_nButtonIDBmp = BTN_MENU_ID; }

	virtual void	Attach				(DataObj*);

	virtual	BOOL	GetMenuButton		(CMenu*);
	virtual	void	DoCmdMenuButton		(UINT nUINT);

	virtual  void	OnBrowseLink			();
	
	virtual BOOL GetToolTipProperties	(CTooltipProperties&);

	virtual	int	GetParamDataIdx		() const { return m_nDataIdxParam; }

	virtual	void BindParam (DataObj* param, int nDataIdxParam = -1) 
		{ 
			ASSERT_KINDOF(DataStr, param);
				m_pParam = (DataStr*) param; 
			m_nDataIdxParam = nDataIdxParam;
		}
	virtual CString GetMenuButtonImageNS();

	virtual void	ReadStaticPropertiesFromJson();
protected:
	BOOL			IsValidNamespace	(CString& strNS);

	//{{AFX_MSG(CNamespaceEdit)
	afx_msg void	OnContextMenu				(CWnd* pWnd, CPoint ptMousePos);

	afx_msg void	OnSearchObject				(CTBNamespace::NSObjectType NsType);
	afx_msg void	OnSearchPicture				();
	afx_msg void	OnSearchText				();
	afx_msg void	OnSearchPdf					();
	afx_msg void	OnSearchRtf					();
	//afx_msg void	OnSearchOdf					();
	afx_msg void	OnSearchReport				();
	afx_msg void	OnBrowseFiles				();
	afx_msg void	OnSearchDocument			();
	afx_msg void	OnSearchObjectInOthers		();
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};
/*
class TB_EXPORT CReportNamespaceEdit : public CNamespaceEdit
{
	DECLARE_DYNCREATE (CReportNamespaceEdit)
public:
	CReportNamespaceEdit()
	{
		SetNamespaceType(CTBNamespace::REPORT);
	}
};

class TB_EXPORT CImageNamespaceEdit : public CNamespaceEdit
{
	DECLARE_DYNCREATE (CImageNamespaceEdit)
public:
	CImageNamespaceEdit()
	{
		SetNamespaceType(CTBNamespace::IMAGE);
	}
};

class TB_EXPORT CTextNamespaceEdit : public CNamespaceEdit
{
	DECLARE_DYNCREATE (CTextNamespaceEdit)
public:
	CTextNamespaceEdit()
	{
		SetNamespaceType(CTBNamespace::TEXT);
	}
};*/

//=============================================================================
//			Class CPictureStatic
//=============================================================================
class TB_EXPORT CPictureStatic : public CParsedStatic, public ResizableCtrl
{
DECLARE_DYNCREATE (CPictureStatic)

protected:
	CString			m_strCurrValue;
	CTBPicture*		m_pPicture;
	BOOL			m_bValid;
	HCURSOR			m_hLinkCursor;

public:
	// Construction
	CPictureStatic	();
	CPictureStatic	(UINT nBtnIDBmp, DataStr* pData /* = NULL */);
	~CPictureStatic	();
	
public:
	// Overridables
	virtual DataType	GetDataType ()	const	{ return DataType::String; }

public:
	virtual void	Attach		(DataObj*);

	// Overridables
	virtual	void	SetValue				(LPCTSTR strValue/*= NULL*/);
	virtual	void	GetValue				(CString& strValue)	{ strValue = m_strCurrValue; }

	virtual	void	ShowError				(const CString& strMsg);
	virtual void	DrawBitmap				(CDC& DCDest, const CRect& rect);
	
	virtual	void	OnCtrlStyleBest			();
	virtual	void	OnCtrlStyleNormal		();
	virtual	void	OnCtrlStyleHorizontal	();
	virtual	void	OnCtrlStyleVertical		();

	virtual	BOOL	OnShowingPopupMenu		(CMenu& menu);
	virtual	BOOL	OnInitCtrl				();
	virtual	BOOL	SubclassEdit			(UINT IDC, CWnd* pParent, const CString& strName = _T(""));

	virtual BOOL	OwnerDraw (CDC*, CRect&, DataObj* = NULL); //per gestire ownerdraw delegato al parsed control di una cella del bodyedit

public:
	void SetValue	 (CTBPicture* pPicture);
	void InitSizeInfo() { ResizableCtrl::InitSizeInfo(this); }

protected:
	//{{AFX_MSG(CPictureStatic)
	afx_msg void	OnContextMenu	(CWnd* pWnd, CPoint ptMousePos);
	afx_msg	BOOL	OnEraseBkgnd	(CDC* pDC);
	afx_msg	void	OnPaint			();
	afx_msg	LRESULT	OnRecalcCtrlSize(WPARAM, LPARAM);
	afx_msg void	OnShowInOtherEditor			();
	afx_msg void	OnWindowPosChanging	(WINDOWPOS FAR* lpwndpos);
	afx_msg LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);

	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CShowFileTextStatic
//=============================================================================
class TB_EXPORT CShowFileTextStatic : public CParsedEdit, public ResizableCtrl
{
DECLARE_DYNCREATE (CShowFileTextStatic)

protected:
	CString			m_strFullPath;
	
public:
	// Construction
	CShowFileTextStatic	();
	CShowFileTextStatic	(UINT nBtnIDBmp, DataStr* pData /* = NULL */ );
	
public:
	virtual	void	InitSizeInfo	() { ResizableCtrl::InitSizeInfo(this); }
	virtual	BOOL	SubclassEdit	(UINT IDC, CWnd* pParent, const CString& strName = _T(""));

	virtual	void	SetValue		(LPCTSTR strValue /*= NULL*/ );
	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	GetValue		(DataObj& aValue);

	virtual DataType	GetDataType ()	const	{ return DataType::String; }
	virtual void		Attach		(DataObj*);

	virtual	void		DrawTextFile	(CDC&, CRect&);
	virtual CString		LoadTextFile	(const CString& strFileName);

protected:      
	virtual	BOOL		OnInitCtrl	();

	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CParsedRichCtrl
//=============================================================================
class TB_EXPORT CParsedRichCtrl : public CRichEditCtrl, public CParsedCtrl, public ResizableCtrl, public CColoredControl
{
DECLARE_DYNCREATE (CParsedRichCtrl)

protected:
	CString		m_strLink;
	BOOL		m_bBrowsingLink;

	BOOL		m_bIsExternalFile;
	CString		m_strFullPath;

public:
	// Construction
	CParsedRichCtrl	();
	virtual ~CParsedRichCtrl() {}

protected:
	HBRUSH			CtlColor		 (CDC* pDC, UINT nCtlColor) { return CColoredControl::CtlColor(pDC, nCtlColor); }
	void			InitSizeInfo	 ()							{ ResizableCtrl::InitSizeInfo(this); }
	afx_msg	LRESULT	OnRecalcCtrlSize (WPARAM, LPARAM)			{ DoRecalcCtrlSize(); return 0L; }

public:
	// explicit creation
	virtual BOOL Create		(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
			BOOL CreateEx	(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, const CString& strName);
	
	// dynamic subclassing (don't use in conjunction with create)
	BOOL SubclassEdit	(UINT nID, CWnd* pParent, const CString& strName = _T(""));
	BOOL CheckControl	(UINT nID, CWnd* pParentWnd, LPCTSTR ctrlClassName = NULL);

	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	GetValue		(DataObj& aValue);

	virtual DataType	GetDataType ()	const	{ return DataType::String; }
	virtual void		Attach		(DataObj*);

	virtual	void		SetValue		(LPCTSTR strValue /*= NULL*/ );

	virtual	void		DrawTextFile	(CDC&, CRect&);
	virtual CString		LoadTextFile	(const CString& strFileName);

	virtual void		SetEditReadOnly (const BOOL bValue);
	virtual	void		SetCtrlMaxLen	(UINT, BOOL bApplyNow = TRUE);

	virtual	void		SetModifyFlag	(BOOL f);
	virtual BOOL		GetModifyFlag	();

	virtual void		SetIsExternalFile (BOOL bSet = TRUE);
	virtual BOOL		IsExternalFile () { return m_bIsExternalFile; }

protected:      
	// Overridables
	virtual	BOOL		OnInitCtrl	();

	afx_msg void	OnWindowPosChanging				(WINDOWPOS FAR* lpwndpos);
	afx_msg	void	OnKillFocus						(CWnd*);

	afx_msg	void	OnEnable						(BOOL bEnable);
	afx_msg void	OnKeyUp							(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void	OnKeyDown						(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void	OnChar							(UINT nChar, UINT nRepCnt, UINT nFlags);

	afx_msg	LRESULT	OnPushButtonCtrl				(WPARAM wParam, LPARAM lParam);
	afx_msg void	OnLinkNotify					(NMHDR* nmhdr, LRESULT* pResult );

	afx_msg void	OnRButtonDown					(UINT nFlag, CPoint ptMousePos);
	afx_msg void	OnLButtonDown					(UINT nFlags, CPoint point);
	afx_msg void	OnNcMouseMove					(UINT nHitTest, CPoint point);
	afx_msg void	OnMouseMove						(UINT nFlags, CPoint point);

protected:

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CWebBrowser
//=============================================================================
class TB_EXPORT CWebBrowser : public CWnd, protected CBrowserEventsObj
{

	CBrowserObj*	m_pBrowser = NULL;
public:
	CWebBrowser();
	~CWebBrowser();

protected:
	void OnAfterCreated(CBrowserObj* pBrowser);
	void OnBeforeClose(CBrowserObj* pBrowser);

	void AdjustPosition(int cx, int cy);

public:
	DECLARE_MESSAGE_MAP()
public:
	BOOL Create(const RECT& rect, CWnd* pParentWnd, UINT nID, LPCTSTR lpszUrl = L"");

	void Navigate(LPCTSTR lpszUrl);
	void Refresh();
	void Paint();
	HWND GetMainWindow();
};

//=============================================================================
//			Class CParsedWebCtrl
//=============================================================================
class TB_EXPORT CParsedWebCtrl : public CWebBrowser, public CParsedCtrl, public ResizableCtrl
{
DECLARE_DYNCREATE (CParsedWebCtrl)

public:
	// Construction
	CParsedWebCtrl	();
	virtual ~CParsedWebCtrl() {}

protected:
	void			InitSizeInfo	 ()							{ ResizableCtrl::InitSizeInfo(this); }
	afx_msg	LRESULT	OnRecalcCtrlSize(WPARAM, LPARAM);
	afx_msg void	OnPaint();
public:
	// explicit creation
	virtual BOOL Create		(DWORD, const RECT& rect, CWnd* pParentWnd, UINT nID);
			BOOL CreateEx	(const RECT& rect, CWnd* pParentWnd, UINT nID, const CString& strName);

	// dynamic subclassing (don't use in conjunction with create)
	BOOL SubclassEdit	(UINT nID, CWnd* pParent, const CString& strName = _T(""));
	BOOL CheckControl	(UINT nID, CWnd* pParentWnd, LPCTSTR ctrlClassName = NULL);

	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	GetValue		(DataObj& aValue);

	virtual DataType	GetDataType ()	const	{ return DataType::String; }
	virtual void		Attach		(DataObj*);

	virtual	void		SetValue		(LPCTSTR strValue /*= NULL*/ );
	virtual	void		SetDataReadOnly(BOOL bRO) {/* non fa nulla*/ }

protected:      
	// Overridables
	virtual	BOOL	OnInitCtrl	();
	
protected:

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CColorEdit
//=============================================================================
class TB_EXPORT CColorEdit : public CLongEdit
{
	friend class CColorButton;

	DECLARE_DYNCREATE (CColorEdit)
protected:
	BOOL m_bTrackSelection;
	//BOOL m_bActive;
	BOOL m_bIsRunning;

public:
	CColorEdit ();

public:
	virtual BOOL EnableCtrl		(BOOL bEnable = TRUE);

	virtual void Attach			(UINT /*nBtnID*/);
	virtual void Attach			(DataObj*);

	virtual	void SetValue		(const DataObj& aValue);
	virtual void SetValue		(COLORREF rgb);

	virtual BOOL GetToolTipProperties	(CTooltipProperties&);

	BOOL	IsRunning()			{ return m_bIsRunning; }

protected:      
	//afx_msg LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	//x_msg BOOL OnClicked();
    afx_msg LONG OnSelEndOK(UINT lParam, LONG wParam);
    afx_msg LONG OnSelEndCancel(UINT lParam, LONG wParam);
    afx_msg LONG OnSelChange(UINT lParam, LONG wParam);

	DECLARE_MESSAGE_MAP ();
};



#include "endh.dex"


   
