
#pragma once

#include <TbGeneric\mlistbox.h>

//includere alla fine degli include del .H
#include "beginh.dex"
class CItemSource;
typedef void (__stdcall *FILLLISTBOX_FUNC) ();

#define UM_CHKLBOX_FILLEDLISTBOX		(WM_USER + 1011)
#define UM_CHKLBOX_EMPTYLISTBOX			(WM_USER + 1012)

//=============================================================================
//			Class CTBListBox
//=============================================================================
class TB_EXPORT CTBListBox : public CBCGPListBox
{
	DECLARE_DYNAMIC(CTBListBox)

public:
	CTBListBox();
	virtual ~CTBListBox() {};

	//{{AFX_MSG(CParsedListBox)
	afx_msg void	OnWindowPosChanging	(WINDOWPOS FAR* lpwndpos);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CParsedListBox
//=============================================================================
class TB_EXPORT CParsedListBox : public CBCGPListBox, public CParsedCtrl, public IDisposingSourceImpl
{
	DECLARE_DYNAMIC (CParsedListBox)
	
protected:      
	DataObjArray		m_DataAssociations;
	long				m_nMaxItemsNo;
	FILLLISTBOX_FUNC	m_pManagedFillListBoxFuncPtr;
public:
	// Construction
	CParsedListBox();
	CParsedListBox(UINT nBtnIDBmp, DataObj* pData = NULL);
	virtual ~CParsedListBox()	{};

public:
	// explicit creation
	BOOL Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	
	// dynamic subclassing (don't use in conjunction with create)
	BOOL SubclassEdit	(UINT nID, CWnd* pParent, const CString& strName = _T(""));
	void				SetFillListBoxFuncPtr (FILLLISTBOX_FUNC value); 
	void				FillListBox			();
	int					AddAssociation		(LPCTSTR, const DataObj&);
	virtual void		SetItemSource(IItemSource* pItemSource);
public:
	// virtual functions
	virtual void	SetValue		(LPCTSTR);
	virtual void	GetValue		(CString&);
	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	GetValue		(DataObj& aValue);

	virtual	BOOL	IsUpperCase		()	{ return FALSE; }
		
	void		SetMaxItemsNo		(int nItems)	{ m_nMaxItemsNo = nItems; }
	int			GetMaxItemsNo		()	const;

protected:
	// virtual function (should not be implemented from user)
	virtual	void	DoSetValue		(const DataObj& aValue)		= 0;

protected:
	// user overridable	
	virtual	BOOL	OnInitCtrl			();
	virtual	void	OnFillListBox		();
	virtual	BOOL	IsValidItemListBox(const DataObj& aValue);

protected:
	BOOL		DoSetCurSel			(const DataObj&);
	int			GetIdxFromDataObj	(const DataObj&);
	DataObj*	GetDataObjFromIdx	(int);
	void		ResetAssociations	(BOOL bRemoveAll = FALSE);

protected:
	virtual	BOOL		OnCommand			(WPARAM wParam, LPARAM lParam);

	//{{AFX_MSG(CParsedListBox)
	afx_msg void	OnWindowPosChanging	(WINDOWPOS FAR* lpwndpos);
	afx_msg	void	OnKillFocus			(CWnd*);
	afx_msg	void	OnEnable			(BOOL bEnable);
	afx_msg void	OnLButtonUp			(UINT nFlag, CPoint ptMousePos);
	afx_msg void	OnRButtonDown		(UINT nFlag, CPoint ptMousePos);
	afx_msg void	OnContextMenu		(CWnd* pWnd, CPoint ptMousePos);
	afx_msg void	OnKeyUp				(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void	OnKeyDown			(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg	LRESULT	OnPushButtonCtrl	(WPARAM wParam, LPARAM lParam);
	//afx_msg	void 	OnSearchOnLinkUpper	();
	//afx_msg	void 	OnSearchOnLinkLower	();
	//afx_msg	void 	OnCallLink			();
	//afx_msg	void 	OnEditAutomaticExpression	();
	//afx_msg	void 	OnBehavior			();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CStrListBox
//=============================================================================
class TB_EXPORT CStrListBox : public CParsedListBox
{
	DECLARE_DYNCREATE (CStrListBox)
	
public:
	// Construction
	CStrListBox();
	CStrListBox(UINT nBtnIDBmp, DataStr* = NULL);

public:
	// public virtual members
	virtual DataType	GetDataType		()	const	{ return DataType::String; }

public:
    // value management
	CString	GetValue	();                       
	
protected:
    virtual	void DoSetValue	(const DataObj& aValue);
	virtual void OnDropFiles(CStringArray* pDroppedFiles);

protected:
	int		AddAssociation		(LPCTSTR, LPCTSTR);

protected:
	//{{AFX_MSG(CStrListBox)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CIntListBox
//=============================================================================
class TB_EXPORT CIntListBox : public CParsedListBox
{
	DECLARE_DYNCREATE (CIntListBox)
	
public:
	// Construction
	CIntListBox();
	CIntListBox(UINT nBtnIDBmp, DataInt* = NULL);

public:
	// virtual members
	virtual DataType	GetDataType		()	const	{ return DataType::Integer; }

public:
	// specific public members
	void	SetValue	(int nValue);
	int		GetValue	();                       
	
protected:
    virtual	void	DoSetValue	(const DataObj& aValue);
	
protected:
	int		AddAssociation		(LPCTSTR, int);
	
protected:
	//{{AFX_MSG(CIntListBox)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CLongListBox
//=============================================================================
class TB_EXPORT CLongListBox : public CParsedListBox
{
	DECLARE_DYNCREATE (CLongListBox)
	
public:
	// Construction
	CLongListBox();
	CLongListBox(UINT nBtnIDBmp, DataLng* = NULL);

public:
	// virtual members
	virtual DataType	GetDataType		()	const	{ return DataType::Long; }

public:
    // value management
	void	SetValue	(long nValue);
	long	GetValue	();

protected:
	virtual	void	DoSetValue	(const DataObj& aValue);

protected:
	int		AddAssociation		(LPCTSTR, long);
	
protected:
	//{{AFX_MSG(CLongListBox)
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CDoubleListBox
//=============================================================================
class TB_EXPORT CDoubleListBox : public CParsedListBox
{
	DECLARE_DYNCREATE (CDoubleListBox)
	
protected:
	int		m_nDec;

public:
	// Construction
	CDoubleListBox();
	CDoubleListBox(UINT nBtnIDBmp, DataDbl* = NULL);

public:
	// virtual members
	virtual	void		SetCtrlNumDec	(int nNumDec)	{ m_nDec = nNumDec; }
	virtual	int			GetCtrlNumDec	();
	virtual	void		GetValue		(DataObj& aValue);
	virtual	double		GetValue		();

	virtual DataType	GetDataType		()	const	{ return DataType::Double; }
	virtual	CString		FormatData		(const DataObj* pDataObj, BOOL bEnablePadding = FALSE) const;


public:
    // value management
	void	SetValue		(double nValue);

protected:
	virtual	void	DoSetValue		(const DataObj& aValue);
	virtual int		AddAssociation	(LPCTSTR, double);

protected:
	//{{AFX_MSG(CDoubleListBox)
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CMoneyListBox
//=============================================================================
class TB_EXPORT CMoneyListBox : public CDoubleListBox
{
	DECLARE_DYNCREATE (CMoneyListBox)
	
public:
	// Construction
	CMoneyListBox();
	CMoneyListBox(UINT nBtnIDBmp, DataMon* = NULL);

public:
	// virtual members
	virtual	void		GetValue		(DataObj& aValue);
	virtual	double		GetValue		();

	virtual DataType	GetDataType		()	const	{ return DataType::Money; }

protected:
	virtual	void	DoSetValue		(const DataObj& aValue);
	virtual int		AddAssociation	(LPCTSTR, double);

protected:
	//{{AFX_MSG(CMoneyListBox)
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CQuantityListBox
//=============================================================================
class TB_EXPORT CQuantityListBox : public CDoubleListBox
{
	DECLARE_DYNCREATE (CQuantityListBox)
	
public:
	// Construction
	CQuantityListBox();
	CQuantityListBox(UINT nBtnIDBmp, DataQty* = NULL);

public:
	// virtual members
	virtual	void		GetValue		(DataObj& aValue);
	virtual	double		GetValue		();

	virtual DataType	GetDataType		()	const	{ return DataType::Quantity; }

protected:
	virtual	void	DoSetValue		(const DataObj& aValue);
	virtual int		AddAssociation	(LPCTSTR, double);

protected:
	//{{AFX_MSG(CQuantityListBox)
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CPercListBox
//=============================================================================
class TB_EXPORT CPercListBox : public CDoubleListBox
{
	DECLARE_DYNCREATE (CPercListBox)
	
public:
	// Construction
	CPercListBox();
	CPercListBox(UINT nBtnIDBmp, DataPerc* = NULL);

public:
	// virtual members
	virtual	void		GetValue		(DataObj& aValue);
	virtual	double		GetValue		();

	virtual DataType	GetDataType		()	const	{ return DataType::Percent; }

protected:
	virtual	void	DoSetValue		(const DataObj& aValue);
	virtual int		AddAssociation	(LPCTSTR, double);

protected:
	//{{AFX_MSG(CPercListBox)
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CDateListBox
//=============================================================================
class TB_EXPORT CDateListBox : public CParsedListBox
{
	DECLARE_DYNCREATE (CDateListBox)
	
public:
	// Construction
	CDateListBox();
	CDateListBox(UINT nBtnIDBmp, DataDate* = NULL);

public:
	// virtual members
	virtual DataType	GetDataType		()	const	{ return DataType::Date; }

public:
    // value management
	void	SetValue	(long nValue);
	void	SetValue	(WORD d, WORD m, WORD y);
	long	GetValue	();

protected:
	virtual	void	DoSetValue	(const DataObj& aValue);

protected:
	int		AddAssociation	(LPCTSTR, long);
	
protected:
	//{{AFX_MSG(CDateListBox)
	afx_msg	LRESULT	OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CBoolListBox
//=============================================================================
class TB_EXPORT CBoolListBox : public CParsedListBox
{
	DECLARE_DYNCREATE (CBoolListBox)
	
public:
	// Construction
	CBoolListBox();
	CBoolListBox(UINT nBtnIDBmp, DataBool* = NULL);

public:
	// virtual members
	virtual DataType	GetDataType		()	const	{ return DataType::Bool; }

public:
	// specific public members
	void	SetValue	(BOOL bValue);
	BOOL	GetValue	();                       
	
protected:
    virtual	void	DoSetValue	(const DataObj& aValue);
	
protected:
	// user overridable
	virtual	void	OnFillListBox	();

protected:
	//{{AFX_MSG(CBoolListBox)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CEnumListBox
//=============================================================================
class TB_EXPORT CEnumListBox : public CParsedListBox
{
	DECLARE_DYNCREATE (CEnumListBox)

protected:	
	WORD	m_wTag;

public:
	BOOL	m_bShowEnumValue;	
	
public:
	// Construction
	CEnumListBox();
	CEnumListBox(UINT nBtnIDBmp, DataEnum* = NULL);

public:
	// virtual functions
	virtual void		Attach			(DataObj*);
	virtual DataType	GetDataType		()	const	{ return DataType(DATA_ENUM_TYPE, m_wTag); }

public:
	void	SetTagValue	(WORD wTag)		{ m_wTag = wTag; }

public:
	const   WORD& GetTagValue() const	{ return m_wTag; }

protected:
	virtual	void	DoSetValue		(const DataObj& aValue);

protected:
	// user overridable
	virtual	void	OnFillListBox	();
	virtual	BOOL	OnCommand		(WPARAM wParam, LPARAM lParam);
	virtual void	DoShowEnumValue ();

protected:
	//{{AFX_MSG(CEnumListBox)
	afx_msg void OnContextMenu	(CWnd*, CPoint);
	afx_msg void OnRButtonDown	(UINT nFlags, CPoint point);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};
//=============================================================================
//			Class CResizableListBox
//=============================================================================
class TB_EXPORT CResizableListBox : public CBCGPListBox, public ResizableCtrl
{
	DECLARE_DYNCREATE (CResizableListBox)

public:
	CResizableListBox () {}

	virtual	BOOL	SubclassDlgItem	(UINT, CWnd*);

	void  InitSizeInfo() { ResizableCtrl::InitSizeInfo(this); }
	void  CalcHorizontalExtent();

protected:
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CResizableMultiListBox
//=============================================================================
class TB_EXPORT CResizableMultiListBox : public CMultiListBox, public ResizableCtrl
{
	DECLARE_DYNCREATE (CResizableMultiListBox)

public:
	CResizableMultiListBox()
		: 	CMultiListBox (0, 0, 0, 0, 0) 
		{}

	CResizableMultiListBox (UINT nID1, UINT nID2 = 0, UINT nID3 = 0, UINT nID4 = 0, UINT nID5 = 0)
		: 	CMultiListBox (nID1, nID2, nID3, nID4, nID5)
		{}

	virtual	BOOL	SubclassDlgItem	(UINT, CWnd*);

	void  InitSizeInfo() { ResizableCtrl::InitSizeInfo(this); }

protected:
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CResizableListCtrl
//=============================================================================
class TB_EXPORT CResizableListCtrl : public CBCGPListCtrl, public ResizableCtrl
{
	DECLARE_DYNCREATE (CResizableListCtrl)

public:
	CResizableListCtrl () {}

	virtual	BOOL	SubclassDlgItem	(UINT, CWnd*);

	void  InitSizeInfo() { ResizableCtrl::InitSizeInfo(this); }

protected:
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CParsedListBox
//=============================================================================
class TB_EXPORT CParsedCheckListBox : public CCheckListBox, public CParsedCtrl, public ResizableCtrl
{
	DECLARE_DYNCREATE(CParsedCheckListBox)

protected:
	DataArray			m_DataAssociations;

	long				m_nMaxItemsNo = 400;
	FILLLISTBOX_FUNC	m_pManagedFillListBoxFuncPtr = NULL;
	CString				m_Separator;
public:
	BOOL			m_bCustomDraw = TRUE;
	BOOL			m_bTrasparent = TRUE;
	COLORREF		m_crBkgColor = RGB(255,255,255);

public:
	// Construction
	CParsedCheckListBox();
	CParsedCheckListBox(UINT nBtnIDBmp, DataObj* pData = NULL);
	~CParsedCheckListBox() {}

public:
	// explicit creation
	BOOL Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);

	// dynamic subclassing (don't use in conjunction with create)
	BOOL SubclassEdit(UINT nID, CWnd* pParent, const CString& strName = _T(""));

	void Attach(UINT /*nBtnID*/) { m_nButtonIDBmp = NO_BUTTON; }

	void				SetFillListBoxFuncPtr(FILLLISTBOX_FUNC value);

	void				FillListBox();
	int					AddAssociation(LPCTSTR, const DataObj&);

	virtual DataType	GetDataType()	const { return DataType::Array; }

public:
	// virtual functions
	virtual void	SetValue(LPCTSTR);
	virtual void	GetValue(CString&);
	virtual	void	SetValue(const DataObj& aValue);
	virtual	void	GetValue(DataObj& aValue);

	void    GetArrayValue(DataObjArray& sSelectedValues);
	void    SetArrayValue(const DataObjArray& sSelectedValues);

	virtual	BOOL	IsUpperCase() { return FALSE; }

	void		SetMaxItemsNo(int nItems) { m_nMaxItemsNo = nItems; }
	int			GetMaxItemsNo()	const;

	void SelectAll();
	void UnSelectAll();
	void InvertSelected();
	BOOL IsSelectAll();

protected:
	virtual	void	DoSetValue(const DataObj& aValue);
	virtual	BOOL	OnInitCtrl();
	virtual	void	OnFillListBox();
	virtual	BOOL	IsValidItemListBox(const DataObj&) { return TRUE; }
	virtual void	RefreshAllCheck(CString sItems);

	void		ResetAssociations();
	CString		ComponeString();

	int InsertAssociation(int nIdx, const CString& sAssoc, DataObj* dataObj);
	int AddAssociationUnsorted(const CString& sAssoc, DataObj* dataObj);

	virtual	BOOL OnCommand(WPARAM wParam, LPARAM lParam);
	virtual BOOL OnAuxCommand(UINT nCode);

	void  InitSizeInfo() { ResizableCtrl::InitSizeInfo(this); }

	/// Changes the checked status of the given 
	/// row (iRow) according to the given value (bChecked).
	void ChangeCheckStateColumnRow(int iRow, bool bChecked);

	virtual void	DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct);
	virtual BOOL	OnShowingPopupMenu(CMenu& menu);

	virtual	BOOL	UpdateCtrlData(BOOL bEmitError, BOOL bSendMessage = FALSE);

	afx_msg	LRESULT	OnRecalcCtrlSize(WPARAM, LPARAM);
	afx_msg	LRESULT	OnPushButtonCtrl(WPARAM wParam, LPARAM lParam);
	afx_msg BOOL	OnEraseBkgnd(CDC* pDC);

	afx_msg	void	OnKillFocus(CWnd*);
	afx_msg	void	OnEnable(BOOL bEnable);
	afx_msg void	OnLButtonUp(UINT nFlag, CPoint ptMousePos);
	afx_msg void	OnRButtonDown(UINT nFlag, CPoint ptMousePos);
	afx_msg void	OnContextMenu(CWnd* pWnd, CPoint ptMousePos);
	afx_msg void	OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void	OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags);
public:
	virtual DataObj*	GetCtrlData();

	virtual int			GetRowNumber() const { return 1; }
	virtual BOOL		IsMultiValue() const { return TRUE; }
	virtual DataObj*	GetCtrlData(const CString& /*sName*/, int /*nRow*/ = 0);
	virtual int			EnumColumnName(CStringArray& /*arNames*/, BOOL bAll = TRUE);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CBoolCheckListBox
//=============================================================================
class TB_EXPORT CBoolCheckListBox : public CParsedCheckListBox
{
	DECLARE_DYNCREATE (CBoolCheckListBox)
	
protected:      
	CStringArray	m_TagDataAssociations;

public:
	CBoolCheckListBox();
	CBoolCheckListBox(UINT nBtnIDBmp);

	// Metodi di gestione BoolListBox con Tag
	int		AddDataBool			(LPCTSTR, DataBool&);
	int		SetDataBoolAt		(int, DataBool&, LPCTSTR = NULL);

	int		AddTagDataBool		(CString aTag, LPCTSTR aStr, DataBool& bDataObj);
	void	SetTagDataBool		(CString aTag, BOOL bSet);
	int		GetIdxFromTag		(CString aTag);
	BOOL	IsCurrentTag		(DataBool& bDataObj);

	// virtual functions
	virtual void		ResetContent	();

	virtual void		SetValue		(LPCTSTR) {}
	virtual void		GetValue		(CString&) {}

	virtual	void		SetValue		(const DataObj&);
	virtual	void		GetValue		(DataObj&);

	virtual DataType	GetDataType		()	const { return DataType::Bool; }

	virtual void	UpdateCtrlView		();
	virtual	void	UpdateCtrlStatus	();
	virtual void	ModifiedCtrlData	();

	virtual	void	SetDataModified		(BOOL bMod);
	virtual	void	SetDataReadOnly		(BOOL bRO);
	virtual	void	SetDataOSLReadOnly	(BOOL bVal = TRUE);
	virtual	void	SetDataOSLHide		(BOOL bVal = TRUE);

	virtual	BOOL	IsDataModified		();
	virtual	BOOL	IsDataOSLReadOnly	();
	virtual	BOOL	IsDataOSLHide		();

	virtual DataObj*	GetCtrlData		(const CString& /*sName*/, int /*nRow*/ = 0);
	virtual int			EnumColumnName	(CStringArray& /*arNames*/, BOOL bAll = TRUE);

protected:
	virtual BOOL OnAuxCommand(UINT nCode);
};

//=============================================================================
//			Class CMultiSelectionListBox
//=============================================================================
class TB_EXPORT CMultiSelectionListBox : public CResizableListBox, public CAbstractCtrl, public IOSLObjectManager
{
	DECLARE_DYNCREATE(CMultiSelectionListBox)

protected:
	DataObjArray	m_DataAssociations;
	CStringArray	m_TagDataAssociations;

private:
	int AddString(LPCTSTR) { ASSERT(FALSE); return -1; }

public:
	// Construction
	CMultiSelectionListBox() : IOSLObjectManager(OSLType_Control) {}
	virtual ~CMultiSelectionListBox() {}

	int		AddTag(CString aTag, LPCTSTR aTitle, DataObj* pData);

	int		GetSelectedTags(CStringArray& arSelectedTags) { EnumColumnName(arSelectedTags); return arSelectedTags.GetSize(); }
	int  	GetSelectedItemData(DataObjArray& arSelectedData);
	void	SetSelTags(CStringArray& arSelectTags);
	void	SetSelTags(DataStrArray& arSelectTags);

	DataObj*	GetData(int i) { return m_DataAssociations.GetAt(i); }
	CString		GetTag(int i) { return m_TagDataAssociations.GetAt(i); }

public:
	// per derivazione da AbstractCtrl
	virtual BOOL		IsAutomaticExpression() const { return FALSE; }
	virtual CString		GetAutomaticExpression() const { return _T(""); }
	virtual BOOL		SetAutomaticExpression(const CString&) { return FALSE; }
	virtual DataObj*	GetCtrlData() { return NULL; }
	virtual CString		GetPublicName() { return GetInfoOSL()->m_Namespace.ToString(); }
	virtual CString		GetCtrlClass() { return _T("MultiSelectionListBox"); }
	virtual CString		GetCtrlName() { return GetInfoOSL()->m_Namespace.GetObjectName(); }

	virtual int			GetRowNumber() const { return 1; }
	virtual BOOL		IsMultiValue() const { return TRUE; }
	virtual DataObj*	GetCtrlData(const CString& /*sName*/, int /*nRow*/ = 0);
	virtual int			EnumColumnName(CStringArray& /*arNames*/, BOOL bAll = TRUE);

	virtual void		ResetContent();
};

#include "endh.dex"
