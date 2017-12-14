#pragma once

#include "parsobj.h"

#include <TbGeneric\DataFileInfo.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class EnumItem;
class CItemSource;
class CItemSourceXml;
#define CBS_STYLES	0x00000003	//@@ NON MI PIACE, ma purtroppo gli stili di
								//@@ una combo non sono bit indipendenti (vedi WINDOWS.H)
								//@@					Germano
//=============================================================================
//			Class CParsedCombo
//=============================================================================
typedef void (__stdcall *FILLCOMBO_FUNC) ();

class TB_EXPORT CParsedCombo : public CBCGPComboBox, public CParsedCtrl, public CColoredControl, public CCustomFont,public IDisposingSourceImpl
{
	DECLARE_DYNAMIC (CParsedCombo)

protected:
	WORD				m_wComboStatus;
	DataObjArray		m_DataAssociations;
	CSize				m_sizeListBox;
	int					m_nMaxRowsNo;
	int					m_nMaxItemsNo;
	int					m_nMaxItemsLen;
	CString				m_strBadVal;
	BOOL				m_bFillListBoxEmpty;
    FILLCOMBO_FUNC		m_pManagedFillComboFuncPtr;
	CStrEdit*			m_pReadOnlyEdit;
	CString				m_sReadOnlyText;
	CString				m_sPrefixBeforeDrop;
	BOOL				m_bSorted;
	BOOL				m_bShowHKLDescription;
	CStringArray		m_DescriptionsBuffer;
	BOOL				m_bNoResetAssociations;
	TCHAR*				m_pszStr;

public:
	WNDPROC				m_pOldChildListBoxWndProc;
	WNDPROC				m_pOldChildEditWndProc;

public:
	// Construction
	CParsedCombo(DataObj* pData = NULL);
	CParsedCombo(UINT nBtnIDBmp, DataObj* pData = NULL);
	virtual ~CParsedCombo();

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

public:
	// explicit creation
	BOOL Create			(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);

	// dynamic subclassing (don't use in conjunction with create)
	BOOL SubclassEdit	(UINT nID, CWnd* pParent, const CString& strName = _T(""));

	// default implementation cals UnSubclassChilds and then base UnsubclassWindow
	virtual	HWND	UnSubclassEdit			();

	void			UnSubclassChilds		();
	virtual void	UnSubclassChildEdit		();
	virtual void	UnSubclassChildListBox	();

	CEdit*					GetChildEdit	() { return m_wndEdit.m_pAutoCompleteCombo == this ? &m_wndEdit : NULL; }
	static	CParsedCombo*	IsChildEditCombo(CObject* pObject)
	{
		return (pObject->IsKindOf(RUNTIME_CLASS(CBCGPEdit)) && ((CBCGPEdit*)pObject)->m_pAutoCompleteCombo) ?
			dynamic_cast<CParsedCombo*>(((CBCGPEdit*)pObject)->m_pAutoCompleteCombo) : NULL;
	};

	LRESULT	OnEditKeyUp			(UINT nChar, UINT nRepCnt, UINT nFlags);
	LRESULT	OnEditKeyDown		(UINT nChar, UINT nRepCnt, UINT nFlags);
	LRESULT	OnEditChar			(UINT nChar, UINT nRepCnt, UINT nFlags);
	LRESULT	OnEditKillFocus		(CWnd*);
	LRESULT	OnEditSetFocus		(CWnd*);
	LRESULT	OnEditRButtonDown	(UINT nFlag, CPoint ptMousePos);
	LRESULT	OnEditContextMenu	(CWnd* pWnd, CPoint ptMousePos);
	HBRUSH	OnEditCtlColor		(CDC* pDC, UINT nCtlColor);
	LRESULT	OnEditPaint			();

	LRESULT	OnLBFindString			(WPARAM nIdx, LPARAM pszStr);
	LRESULT	OnLBFindStringExact		(WPARAM nIdx, LPARAM pszStr);
	LRESULT	OnLBGetText				(WPARAM nIdx, LPARAM pszStr);
	LRESULT	OnLBGetTextLen			(WPARAM nIdx, LPARAM /* not used */);
	LRESULT	OnLBWindowPosChanging	(WINDOWPOS FAR* wndPos);

	LRESULT				GetLBTextMFC(int index, CString& strText);

	// generic public functions
	void		FillListBox					();

	void		SetMaxRows					(int nRows)		{ m_nMaxRowsNo = nRows; }
	int			GetMaxRows					()	const		{ return m_nMaxRowsNo; }

	long		GetMaxItemsNo				();
	void		SetMaxItemsNo				(int nItems)	{ m_nMaxItemsNo = nItems == 0 ? m_nMaxItemsNo : nItems; }

	void		SetMaxItemsLen				(int nLen) { m_nMaxItemsLen = nLen; }

	int			GetIdxFromDataObj			(const DataObj&, BOOL bNoCase = FALSE) const;
	DataObj*	GetDataObjFromIdx			(int) const ;

	void		SetFillComboFuncPtr			(FILLCOMBO_FUNC value); 

	CString		GetDescriptionsBuffer		( int i)	{ return m_DescriptionsBuffer.GetAt(i);}
	int			GetSizeDescriptionsBuffer	()			{ return m_DescriptionsBuffer.GetSize();}

	void		SetNotResetAssociations(BOOL bVal = TRUE) { m_bNoResetAssociations = bVal; };

public:
	// virtual functions
	virtual BOOL	IsValid			();
	virtual BOOL	IsValid			(const DataObj& aValue);
	virtual void	SetValue		(LPCTSTR);
	virtual void	GetValue		(CString&);
	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	GetValue		(DataObj& aValue);
	            
	virtual	void	SetModifyFlag(BOOL bFlag);
	virtual	BOOL	GetModifyFlag();

	virtual	void	SetCtrlSel		(int, int);
	virtual	void	SetCtrlMaxLen	(UINT, BOOL bApplyNow = TRUE);
	virtual	BOOL	SetCtrlPos		(const CWnd* pWndInsertAfter, int x, int y, int cx, int cy, UINT nFlags);

	virtual void	SetEditReadOnly (const BOOL bValue);
	virtual BOOL	IsEditReadOnly	() const;

	virtual	int		AddAssociation	(LPCTSTR, const DataObj&, BOOL bCallFilter = TRUE);

	virtual	CSize	AdaptNewSize	 (UINT, UINT, BOOL bButtonsIncluded);
	virtual	void	ResetAssociations(BOOL bRemoveAll = FALSE);
	virtual BOOL	IsThemedDropDownList() const;

	virtual BOOL	IsShowDescription() const { return m_bShowHKLDescription; }
	virtual	BOOL	GetDescription(const DataObj*, CString&) const { return FALSE; }

protected:
	virtual int		AddAssociationSorted	(		const CString& sAssoc, DataObj* dataObj, CDC* pDC, CFont* pFont, BOOL bCallFilter = TRUE);
	virtual int		AddAssociationUnsorted	(		const CString& sAssoc, DataObj* dataObj, CDC* pDC, CFont* pFont, BOOL bCallFilter = TRUE);
	virtual int		InsertAssociation	(int nIdx,	const CString& sAssoc, DataObj* dataObj, CDC* pDC, CFont* pFont, BOOL bCallFilter = TRUE);

	virtual	void	DoSetValue			(const DataObj& aValue)		= 0;
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText)	= 0;
	virtual	int		GetAssociatedTextLen(int nIdx);

protected:
	BOOL			SubclassChilds();

	// user overridable	
	virtual BOOL	OnInitCtrl			();
	virtual BOOL	SubclassChildEdit	();
	virtual BOOL	SubclassChildListBox();

	virtual	BOOL	DoKeyDown			(UINT nChar);
	virtual	void	DoKillFocus 		(CWnd*);
	virtual	BOOL	DoSelection			();

	virtual	void	OnFillListBox		();
	virtual	BOOL	IsValidItemListBox	(const DataObj&);

	virtual BOOL	HasFocus			();

	virtual BOOL	GetToolTipProperties (CTooltipProperties& tp);

protected:
	virtual BOOL	DoSetCurSel				(const DataObj&);
	void			SetNewFormatIdx			();

private:
	BOOL			NeedCustomPaint		();
	CRect			GetEditReadOnlyRect	();

	LRESULT			InternalFindString	(WPARAM nIdx, LPARAM pszStr);
	int				DichotomousSearch	(const CString& str, int nStart, int nEnd);

protected:
	virtual	BOOL	OnCommand			(WPARAM wParam, LPARAM lParam);

	//{{AFX_MSG(CParsedCombo)
	afx_msg	void	OnKillFocus			(CWnd*);
	afx_msg	void	OnSetFocus			(CWnd*);
	afx_msg void	OnWindowPosChanging	(WINDOWPOS FAR* lpwndpos);
	afx_msg	void	OnEnable			(BOOL bEnable);
	afx_msg void	OnVScroll			(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg void	OnLButtonUp			(UINT nFlag, CPoint ptMousePos);
	afx_msg void	OnRButtonDown		(UINT nFlag, CPoint ptMousePos);
	afx_msg void	OnContextMenu		(CWnd* pWnd, CPoint ptMousePos);
	afx_msg void	OnChar				(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void	OnKeyUp				(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void	OnKeyDown			(UINT nChar, UINT nRepCnt, UINT nFlags);

	afx_msg	LRESULT	OnPushButtonCtrl			(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT	OnGetControlDescription		(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT	OnCheckDropDown				(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT	OnGetCurSel					(WPARAM /* not used */, LPARAM /* not used */);

		//afx_msg	void 	OnSearchOnLinkUpper	();
		//afx_msg	void 	OnSearchOnLinkLower	();
		//afx_msg	void 	OnCallLink			();
		//afx_msg	void 	OnEditAutomaticExpression	();
		//afx_msg	void 	OnBehavior			();
	afx_msg	void 	OnFormatPopupMenuItemSelected	(UINT nID);
	afx_msg HBRUSH	OnCtlColor						(CDC* pDC, CWnd* pWnd, UINT nCtlColor);
	afx_msg HBRUSH	CtlColor						(CDC* pDC, UINT nCtlColor);
	afx_msg	void 	OnPaint							();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CStrCombo
//=============================================================================
class TB_EXPORT CStrCombo : public CParsedCombo
{
	DECLARE_DYNCREATE (CStrCombo)
	
public:
	// Construction
	CStrCombo();
	CStrCombo(UINT nBtnIDBmp, DataStr* = NULL);

protected:
	virtual CString GetString (const DataObj* pObj) const { ASSERT_KINDOF(DataStr, pObj); return ((DataStr*)pObj)->GetString(); }
public:
	virtual BOOL	IsValid			()						{ return CParsedCombo::IsValid() && IsValidStr(GetValue()); }
	virtual BOOL	IsValid			(const DataObj& aValue)	{ return CParsedCombo::IsValid(aValue) &&
																	 IsValidStr(((DataStr&)aValue).GetString()); }
	// public virtual members
	virtual void		Attach			(DataObj*);
	virtual DataType	GetDataType		()	const	{ return DataType::String; }

	virtual	CString	FormatErrorMessage	(MessageID nIDP, LPCTSTR pszBadVal);
	
	// specific public members
	virtual CString	GetValue	();                       
			BOOL	IsValidStr	(LPCTSTR);

	virtual	int		AddAssociation		(LPCTSTR, const DataObj&, BOOL bCallFilter = TRUE);

	int		AddAssociation	(LPCTSTR, LPCTSTR);

protected:
	// user overridables
	virtual	BOOL	OnInitCtrl			();

	virtual BOOL	DoOnChar			(UINT nChar);
    virtual	void	DoSetValue			(const DataObj& aValue);
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);
	virtual	int		GetAssociatedTextLen(int nIdx);
	virtual void	GetValue			(DataObj& aValue);

public:
	virtual	void	SetValue			(const DataObj& aValue);
	virtual void	SetValue			(LPCTSTR sz) { __super::SetValue(sz); }

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CDescriptionCombo
//=============================================================================
class TB_EXPORT CDescriptionCombo : public CParsedCombo, public ResizableCtrl
{
	friend class CHyperLink;
	DECLARE_DYNCREATE (CDescriptionCombo)
public:

	CDescriptionCombo();
	CDescriptionCombo(UINT nBtnIDBmp, DataObj* = NULL);

	virtual void		Attach		(DataObj* pObj) { CParsedCtrl::Attach(pObj); }

	virtual DataType	GetDataType			()	const	{ return DataType::String; }
	virtual	void		ShowDescription		(BOOL bShowDescription = TRUE);
	
	virtual	CString		FormatData			(const DataObj* pDataObj, BOOL bEnablePadding = FALSE) const;
	virtual void		SetValue			(const DataObj& aValue);
	
	virtual BOOL		ForceUpdateCtrlView	(int)	{ return TRUE; }

	virtual	int		AddAssociation		(LPCTSTR, const DataObj&, BOOL bCallFilter = TRUE);

protected:
	virtual BOOL	PreCreateWindow		(CREATESTRUCT& cs);
	virtual	BOOL	OnInitCtrl			();
	virtual void	DoSetValue			(const DataObj& aValue);
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);
	virtual	BOOL	GetDescription		(const DataObj* pDataObj, CString& str) const;

	void	AddDescription		(const DataObj* pDataObj, const CString& str);

public:
	void  InitSizeInfo() { ResizableCtrl::InitSizeInfo(this); }

	//virtual	BOOL	SubclassDlgItem	(UINT, CWnd*);
	BOOL SubclassEdit	(UINT nID, CWnd* pParent, const CString& strName = _T(""));

	int GetInputWidth() const;

protected:
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CLongIDCombo
//=============================================================================
class TB_EXPORT CLongIDCombo : public CDescriptionCombo
{
	DECLARE_DYNCREATE (CLongIDCombo)
public:
	CLongIDCombo();
	CLongIDCombo(UINT nBtnIDBmp, DataLng* = NULL);

protected:
	virtual CString GetString		(const DataObj* pObj) const { ASSERT_KINDOF(DataLng, pObj); return pObj->Str(0,0); }
	virtual	void	DoSetValue		(const DataObj& aValue);

public:
	virtual DataType	GetDataType		()	const	{ return DataType::Long; }

	int		AddAssociation	(LPCTSTR, long);
};

//=============================================================================
//			Class CIntCombo
//=============================================================================
class TB_EXPORT CIntCombo : public CParsedCombo
{
	DECLARE_DYNCREATE (CIntCombo)
	
protected:
	DataInt	m_nMin;
	DataInt	m_nMax;
	int		m_nCurValue;

public:
	// Construction
	CIntCombo();
	CIntCombo(UINT nBtnIDBmp, DataInt* = NULL);

public:
	// virtual members
	virtual BOOL		IsValid			();
	virtual BOOL		IsValid			(const DataObj& aValue)	{ return CParsedCombo::IsValid(aValue) &&
																		 IsValidInt((int) ((DataInt &)aValue)); }

	virtual DataType	GetDataType		()	const	{ return DataType::Integer; }

	virtual	CString	FormatErrorMessage	(MessageID nIDP, LPCTSTR pszBadVal);
	virtual void	DoSpinScroll		(UINT nSBCode);

	// specific public members
	virtual void		SetRange	(int nMin, int nMax);
	virtual	DataObj*	GetMinValue		() { return &m_nMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_nMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_nMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_nMax.Assign(value); }
	
	void	SetValue	(int nValue);
	int		GetValue	();                       
	BOOL	IsValidInt	(int nValue);
	int		Format		(int nValue, LPTSTR outStr);
	int		AddAssociation	(LPCTSTR, int);
	
	virtual	int		AddAssociation		(LPCTSTR, const DataObj&, BOOL bCallFilter = TRUE);

protected:
	virtual BOOL 	DoOnChar			(UINT nChar);
	virtual	BOOL	DoKeyDown			(UINT nChar);
	virtual	void	DoKillFocus 		(CWnd*);
    virtual	void	DoSetValue			(const DataObj& aValue);
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);
	
protected:
	// user overridable	
	virtual BOOL	OnInitCtrl		();

protected:
	//{{AFX_MSG(CIntCombo)
	afx_msg	LRESULT OnFormatStyleChange				(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CLongCombo
//=============================================================================
class TB_EXPORT CLongCombo : public CParsedCombo
{
	DECLARE_DYNCREATE (CLongCombo)
	
protected:
	DataLng	m_lMin;
	DataLng	m_lMax;
	long	m_lCurValue;

public:
	// Construction
	CLongCombo(DataLng* = NULL);
	CLongCombo(UINT nBtnIDBmp, DataLng* = NULL);

public:
	// virtual members
	virtual BOOL		IsValid			();
	virtual BOOL		IsValid			(const DataObj& aValue)	{ return __super::IsValid(aValue) &&
																		 IsValidLong((long) ((DataLng &)aValue)); }

	virtual DataType	GetDataType		()	const	{ return DataType::Long; }

	virtual	CString	FormatErrorMessage	(MessageID nIDP, LPCTSTR pszBadVal);
	virtual void	DoSpinScroll		(UINT nSBCode);

    // edit value management
	virtual	void		SetRange		(int nMin, int nMax);
	virtual	DataObj*	GetMinValue		() { return &m_lMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_lMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_lMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_lMax.Assign(value); }

	void	SetValue	(long nValue);
	long	GetValue	();
	BOOL	IsValidLong	(long nValue);

	int		Format		(long nValue, LPTSTR outStr);

	int		AddAssociation	(LPCTSTR, long);

	virtual	int		AddAssociation		(LPCTSTR, const DataObj&, BOOL bCallFilter = TRUE);

protected:
	virtual BOOL	DoOnChar			(UINT nChar);
	virtual	BOOL	DoKeyDown			(UINT nChar);
	virtual	void	DoKillFocus 		(CWnd*);
	virtual	void	DoSetValue			(const DataObj& aValue);
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);

protected:
	virtual BOOL	OnInitCtrl		();

public:
	virtual	BOOL	GetDescription		(const DataObj*, CString& ) const
						{ ASSERT_TRACE(FALSE, "Use class CLongIDCombo instead of me"); return FALSE; }

protected:
	afx_msg	LRESULT OnFormatStyleChange (WPARAM wParam, LPARAM lParam);

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CDataObjTypesCombo : public CLongCombo
{
	DECLARE_DYNCREATE (CDataObjTypesCombo)

protected:
	CWordArray	m_DataTypes;

public:
	// Construction
	CDataObjTypesCombo();
	virtual ~CDataObjTypesCombo();

public:
	CWordArray&	GetDataTypes		()	{ return m_DataTypes; }
	void		SetTypeValue		(const DataType&);
	DataType 	GetTypeValue		();
	BOOL		IsPrecisionEnabled	();

protected:
	virtual	BOOL	OnInitCtrl			();
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);
	virtual	int		GetAssociatedTextLen(int nIdx);
	virtual	void	DoSetValue			(const DataObj&);
	virtual BOOL	DoOnChar			(UINT nChar);
	virtual	BOOL	DoKeyUp				(UINT nChar);
	virtual	BOOL	DoKeyDown			(UINT nChar);
	virtual	void	DoKillFocus 		(CWnd*);
	virtual	void	OnFillListBox		();
	virtual	BOOL	DoContextMenu		(CWnd* pWnd, CPoint ptPoint);

protected:
	//{{AFX_MSG(CDataObjTypesCombo)
	afx_msg void OnContextMenu	(CWnd* pWnd, CPoint Point);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

//=============================================================================
//			Class CDoubleCombo
//=============================================================================
class TB_EXPORT CDoubleCombo : public CParsedCombo
{
	DECLARE_DYNCREATE (CDoubleCombo)
	
protected:
	DataDbl	m_dMin;
	DataDbl m_dMax;
	int		m_nDec;
	double	m_dCurValue;

	
public:
	// Construction
	CDoubleCombo();
	CDoubleCombo(UINT nBtnIDBmp, DataDbl* = NULL);

public:
	// virtual members
	virtual	void		SetCtrlNumDec	(int nNumDec)	{ m_nDec = nNumDec; }
	virtual	int			GetCtrlNumDec	();
	virtual	void		GetValue		(DataObj& aValue);
	virtual	double		GetValue		();
	
	virtual BOOL		IsValid			();
	virtual BOOL		IsValid			(const DataObj& aValue)	{ return CParsedCombo::IsValid(aValue) &&
																		 IsValidDouble((double) ((DataDbl &)aValue)); }

	virtual DataType	GetDataType		()	const	{ return DataType::Double; }

	virtual	CString	FormatErrorMessage	(MessageID nIDP, LPCTSTR pszBadVal);
	virtual void	DoSpinScroll		(UINT nSBCode);

    // edit value management
	virtual	void		SetRange		(double nMin, double nMax, int nDec = DEFAULT_N_DEC);
	virtual	DataObj*	GetMinValue		() { return &m_dMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_dMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_dMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_dMax.Assign(value); }

	virtual	CString		FormatData		(const DataObj* pDataObj, BOOL bEnablePadding = FALSE) const;

public:
    // edit value management
	void	SetValue		(const DataObj& aValue);
	void	SetValue		(double nValue);
	BOOL	IsValidDouble	(double nValue);
	int		Format			(double nValue, LPTSTR outStr);

	virtual	int		AddAssociation		(LPCTSTR, double);

	virtual	int		AddAssociation		(LPCTSTR, const DataObj&, BOOL bCallFilter = TRUE);

protected:
	LRESULT FormatValue(DataDbl& aValue);

protected:
	virtual	BOOL	DoOnChar			(UINT nChar);
	virtual	BOOL	DoKeyDown			(UINT nChar);
	virtual	void	DoKillFocus 		(CWnd*);
	virtual	void	DoSetValue			(const DataObj& aValue);
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);
	
protected:
	// user overridable	
	virtual BOOL	OnInitCtrl	();

protected:
	//{{AFX_MSG(CDoubleCombo)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CMoneyCombo
//=============================================================================
class TB_EXPORT CMoneyCombo : public CDoubleCombo
{
	DECLARE_DYNCREATE (CMoneyCombo)
	
public:
	// Construction
	CMoneyCombo();
	CMoneyCombo(UINT nBtnIDBmp, DataMon* = NULL);

public:
	// virtual members
	virtual	void		GetValue		(DataObj& aValue);
	virtual	double		GetValue		();

	virtual DataType	GetDataType		()	const	{ return DataType::Money; }
	virtual	int			AddAssociation	(LPCTSTR, double);

protected:
	virtual	void	DoSetValue			(const DataObj& aValue);
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);

protected:
	//{{AFX_MSG(CMoneyCombo)
		afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
    
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CQuantityCombo
//=============================================================================
class TB_EXPORT CQuantityCombo : public CDoubleCombo
{
	DECLARE_DYNCREATE (CQuantityCombo)
	
public:
	// Construction
	CQuantityCombo();
	CQuantityCombo(UINT nBtnIDBmp, DataQty* = NULL);

public:
	// virtual members
	virtual	void		GetValue		(DataObj& aValue);
	virtual	double		GetValue		();

	virtual DataType	GetDataType		()	const	{ return DataType::Quantity; }
	virtual	int			AddAssociation	(LPCTSTR, double);

protected:
	virtual	void	DoSetValue			(const DataObj& aValue);
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);

protected:
	//{{AFX_MSG(CQuantityCombo)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
    
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CPercCombo
//=============================================================================
class TB_EXPORT CPercCombo : public CDoubleCombo
{
	DECLARE_DYNCREATE (CPercCombo)
	
public:
	// Construction
	CPercCombo();
	CPercCombo(UINT nBtnIDBmp, DataPerc* = NULL);

public:
	// virtual members
	virtual	void		GetValue		(DataObj& aValue);
	virtual	double		GetValue		();

	virtual DataType	GetDataType		()	const	{ return DataType::Percent; }

    // edit value management
	virtual void		SetRange		(double nMin, double nMax, int nDec = DEFAULT_N_DEC);

	virtual	int		AddAssociation		(LPCTSTR, double);

protected:
	virtual	void	DoSetValue			(const DataObj& aValue);
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);

protected:
	//{{AFX_MSG(CPercCombo)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CDateCombo
//=============================================================================
class TB_EXPORT CDateCombo : public CParsedCombo
{
	DECLARE_DYNCREATE (CDateCombo)
	
protected:
	DataDate	m_lMin;
	DataDate	m_lMax;
	long		m_nCurDate;
	
public:
	// Construction
	CDateCombo();
	CDateCombo(UINT nBtnIDBmp, DataDate* = NULL);

public:
	// virtual members
	virtual BOOL		IsValid			();
	virtual BOOL		IsValid			(const DataObj& aValue)	{ return CParsedCombo::IsValid(aValue) &&
																		 IsValidDate((long) ((DataDate &)aValue)); }

	virtual DataType	GetDataType		()	const	{ return DataType::Date; }

	virtual	CString	FormatErrorMessage	(MessageID nIDP, LPCTSTR pszBadVal);
	virtual void	DoSpinScroll		(UINT nSBCode);

    // edit value management
	virtual void	SetRange	(int nMin, int nMax);
	virtual void	SetRange	(WORD dMin, WORD mMin, WORD yMin, WORD dMax, WORD mMax, WORD yMax);

	virtual	DataObj*	GetMinValue		() { return &m_lMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_lMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_lMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_lMax.Assign(value); }


	void	SetValue	(long nValue);
	void	SetValue	(WORD d, WORD m, WORD y);
	long	GetValue	();
	BOOL	IsValidDate	(long nValue);

	int		AddAssociation	(LPCTSTR, long);

	virtual	int		AddAssociation		(LPCTSTR, const DataObj&, BOOL bCallFilter = TRUE);

protected:
	// user overridables
	virtual	BOOL	OnInitCtrl	();

protected:
	virtual	BOOL	DoOnChar			(UINT nChar);
	virtual	void	DoSetValue			(const DataObj& aValue);
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);
	
protected:
	//{{AFX_MSG(CDateCombo)
	afx_msg	LRESULT	OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CBoolCombo
//=============================================================================
class TB_EXPORT CBoolCombo : public CParsedCombo
{
	DECLARE_DYNCREATE (CBoolCombo)
	
public:
	// Construction
	CBoolCombo();
	CBoolCombo(UINT nBtnIDBmp, DataBool* = NULL);

public:
	// virtual members
	virtual DataType	GetDataType		()	const	{ return DataType::Bool; }

	// specific public members
	void	SetValue	(BOOL bValue);
	BOOL	GetValue	();                       
	
	virtual	int		AddAssociation		(LPCTSTR, const DataObj&, BOOL bCallFilter = TRUE);

protected:
	virtual BOOL 	DoOnChar			(UINT nChar);
    virtual	void	DoSetValue			(const DataObj& aValue);
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);
	
protected:
	// user overridable	
	virtual	void	OnFillListBox	();

protected:
	//{{AFX_MSG(CBoolCombo)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CIdentifierCombo
//=============================================================================
class TB_EXPORT CIdentifierCombo : public CStrCombo
{
	DECLARE_DYNCREATE (CIdentifierCombo)
	
protected:
	SymTable*	m_pSymTable;
	CObject*	m_pItemToCheck;
	
public:
	// Construction
	CIdentifierCombo();
	CIdentifierCombo(SymTable*, DataStr*, CObject* pItem = NULL, UINT nBtnIDBmp = NO_BUTTON);
	CIdentifierCombo(SymTable*, CObject* pItem = NULL, UINT nBtnIDBmp = NO_BUTTON, DataStr* = NULL);

public:
	// virtual members
	virtual BOOL	IsValid		();

protected:
	// not overridables
	virtual BOOL	DoOnChar	(UINT nChar);
	
protected:
	// user overridable
	virtual	void	OnFillListBox	();

protected:
	//{{AFX_MSG(CIdentifierCombo)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CEnumCombo
//=============================================================================
class TB_EXPORT CEnumCombo : public CParsedCombo
{
	DECLARE_DYNCREATE (CEnumCombo)

public:
	BOOL	m_bShowEnumValue;	

protected:
	WORD	m_wTag;
	
public:
	// Construction
	CEnumCombo();
	CEnumCombo(UINT nBtnIDBmp, DataEnum* = NULL);

public:
	// public virtual members
	virtual void		Attach		(DataObj*);
	virtual DataType	GetDataType	()	const	{ return DataType(DATA_ENUM_TYPE, m_wTag); }

protected:
	virtual BOOL	DoOnChar			(UINT nChar);
	virtual	BOOL	DoKeyUp				(UINT nChar);
	virtual	BOOL	DoKeyDown			(UINT nChar);
	virtual	void	DoSetValue			(const DataObj& aValue);
	virtual	int		GetAssociatedText	(int nIdx, LPTSTR strText);
	virtual CString SetItemDescription	(EnumItem*);
	
protected:
	// user overridable
	virtual	void	OnFillListBox	();
	virtual	BOOL	OnInitCtrl		();

public:
	void	SetTagValue		(WORD wTag)		{ m_wTag = wTag; }

public:
	const   WORD& GetTagValue() const	{ return m_wTag; }

protected:
	virtual void	DoShowEnumValue ();
		
protected:
	//{{AFX_MSG(CEnumCombo)
	afx_msg void OnRButtonDown	(UINT , CPoint );
	afx_msg void OnContextMenu	(CWnd* pWnd, CPoint Point);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CComboBoxExt
//=============================================================================
class TB_EXPORT CComboBoxExt : public CBCGPComboBox
{
// Construction
public:
	CComboBoxExt() {}
	virtual ~CComboBoxExt() {}

protected:
    void RecalcDropWidth();

	afx_msg void OnDropdown();

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CResizableComboBox
//=============================================================================
class TB_EXPORT CResizableComboBox : public CComboBoxExt, public ResizableCtrl
{
	DECLARE_DYNCREATE (CResizableComboBox)

public:
	CResizableComboBox () {}

	virtual	BOOL	SubclassDlgItem	(UINT, CWnd*);

	void  InitSizeInfo() { ResizableCtrl::InitSizeInfo(this); }

protected:
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CXmlCombo
//=============================================================================
class TB_EXPORT CXmlCombo : public CStrCombo
{
	DECLARE_DYNCREATE (CXmlCombo)

public:
	// Construction
	CXmlCombo();
	~CXmlCombo();

private:
	CDataFileInfo*				m_pDfi;
	DataObjArray				pAttachedDataObj;
	CStringArray				pAttachedDataObjNames;
	BOOL						m_bUseProductLanguage;
	BOOL						m_bEnableAddElements;
		

public:
	void	SetNameSpace		(LPCTSTR lpcszNamespacee, BOOL bAllowChanges = FALSE);
	void	UseProductLanguage	(BOOL bUseProductLanguage) {m_bUseProductLanguage = bUseProductLanguage;}
	void	AllowChanges		();

	void	SetKey				(const CString& strFieldName);
	void	SetHidden			(const CString& strFieldName, BOOL bValue = TRUE);
	//void	SetType				(CString sNomeCampo, DataType tType);

	void	EnableAddElements	();

	virtual void	Attach		(DataObj* p) { __super::Attach(p); }
	virtual void	Attach		(DataObj* pDataObj, CString pName);
	
	void	DetachDataObjs		();
	
public: //inline
	CDataFileInfo*	GetDataFileInfo() const { return m_pDfi; }	
	DataObj*		GetAttachedDataByName	(const CString& sNomeCampo);

public:	// public virtual members
	virtual	CString	GetValue		();                       

protected:
	// user overridable
	virtual	void	OnFillListBox	();
	virtual BOOL	OnShowingPopupMenu	(CMenu&);
	virtual	BOOL	OnCommand		 (WPARAM wParam, LPARAM lParam);
	
protected:
	//{{AFX_MSG(CXmlCombo)
	afx_msg void OnContextMenu	(CWnd* pWnd, CPoint Point);	
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};


//=============================================================================
//			Class CIntMonthNameCombo
//=============================================================================
class TB_EXPORT CIntMonthNameCombo : public CIntCombo
{
	DECLARE_DYNCREATE (CIntMonthNameCombo)

public:
	CIntMonthNameCombo();

protected:
	virtual BOOL	OnInitCtrl();
	virtual	void	OnFillListBox();

	virtual	int		GetAssociatedText	(int, LPTSTR)	{ return -1; }	// standard behaviour
	virtual	int		GetAssociatedTextLen(int)			{ return -1; }	// standard behaviour

public:
	virtual	void	SetRange		(int nMin, int nMax);
	CString			GetTextValue	(int index);
};

//=============================================================================
//			Class CIntWeekDaysCombo
//=============================================================================
// 
class TB_EXPORT CIntWeekDaysCombo : public CIntCombo
{
	DECLARE_DYNCREATE (CIntWeekDaysCombo)

public:
	CIntWeekDaysCombo();

protected:
	virtual BOOL	OnInitCtrl();
	virtual	void	OnFillListBox();

	virtual	int		GetAssociatedText	(int, LPTSTR)	{ return -1; }	// standard behaviour
	virtual	int		GetAssociatedTextLen(int)			{ return -1; }	// standard behaviour

public:
	virtual	void	SetRange		(int nMin, int nMax);
	CString			GetTextValue	(int index);
};

//=============================================================================
//			Class CMSStrCombo - multi selection descriprtion combo
//=============================================================================
class TB_EXPORT CMSStrCombo : public  CDescriptionCombo
{
	DECLARE_DYNCREATE (CMSStrCombo)

public:
	WNDPROC		m_pOldMSStrComboWndProc;
	
private:
	CRect		m_RectBitmap;

protected:
	CString		m_Separator;

	DataType	m_DataTypeKey;		//per WOORM
	BOOL		m_bDecodeListKey;	//per WOORM

	BOOL       m_bItemHeightSet;  // A flag used in MeasureItem, see comments there	
	CByteArray m_ItemsCheckState; // 0 - No check, 1 - Checked
	BOOL	   m_bTextUpdated;	  // Update string containing the combobox to display

public:
	// Construction
	CMSStrCombo();
	CMSStrCombo(UINT nBtnIDBmp, DataStr* = NULL);

	void Attach (UINT /*nBtnID*/) { m_nButtonIDBmp = NO_BUTTON; }

public:
	virtual CString	GetValue ();     
	virtual	void	SetValue (const DataObj& aValue);
	virtual void	SetValue (LPCTSTR);
	
	void RecalcText();	// Routine to update the text
	virtual void OnRecalcText(CString& strText);

	int     SetCheck		 (CString strText, BOOL bFlag);
	int     SetCheck         (int nIndex, BOOL bFlag);		// Selects unselects the specified item
	BOOL    GetCheck         (int nIndex);					// Returns checked state
	
	void    GetArrayValue	(DataObjArray& sSelectedValues) const;
	void    SetArrayValue   (const DataObjArray& sSelectedValues);
	CString GetSqlNativeValues	() const;

	void    LoadUnattending();

	void    SetComboValue(CString strText);
	BOOL	IsSelectAll();
	virtual BOOL	OnShowingPopupMenu			(CMenu&);

	void	SetSeparator	(const CString& sep)	{ m_Separator = sep; }	

	void	SetDataTypeKey	(DataType dt)			{ m_DataTypeKey = dt; }		//per WOORM
	void	SetDecodeListKey(BOOL b = TRUE)			{ m_bDecodeListKey = b; }	//per WOORM

	virtual void OnDraw(CDC* pDC, BOOL bDrawPrompt);

	void RefreshAllCheck(CString comboStr);

	void SelectAll();
	void UnSelectAll();
	void InvertSelected();

	virtual	BOOL	SubclassChildListBox();
	virtual void	UnSubclassChildListBox();

protected:
	void CalculateSize();

	virtual BOOL PreCreateWindow		(CREATESTRUCT& cs);
	virtual int	 CompareItem(LPCOMPAREITEMSTRUCT lpCompareItemStruct);
	virtual void DrawItem				(LPDRAWITEMSTRUCT lpDrawItemStruct);
	virtual void GetValue 				(DataObj& aValue);
	virtual	void DoSetValue				(const DataObj& aValue);
	virtual	int	 GetAssociatedText		(int nIdx, LPTSTR pszStr);
	virtual	int	 GetAssociatedTextLen	(int nIdx);
	virtual BOOL DoSetCurSel			(const DataObj&);
	virtual BOOL GetToolTipProperties	(CTooltipProperties&);
	virtual	void ResetAssociations		(BOOL bRemoveAll = FALSE);

	virtual	BOOL	OnCommand		 (WPARAM wParam, LPARAM lParam);
	virtual	BOOL	OnInitCtrl		();
	virtual	BOOL	OnOpenDropDown	(CPoint point)	{ return TRUE; }

	//{{AFX_MSG (CMSStrCombo)
	afx_msg BOOL    OnDropDown              ();
	afx_msg	LRESULT	OnRecalcCtrlSize		(WPARAM, LPARAM);
	afx_msg void	OnLButtonDown			(UINT nFlags, CPoint point);
	afx_msg void	OnLButtonDblClk			(UINT nFlags, CPoint point);
	afx_msg	void 	OnPaint					();
	afx_msg	int		OnCompareItem			(int nIDCtl, LPCOMPAREITEMSTRUCT lpCompareItemStruct);
	//}}AFX_MSG
public:
	virtual DataObj*	GetCtrlData();

	virtual int			GetRowNumber() const { return 1; }
	virtual BOOL		IsMultiValue() const { return TRUE; }
	virtual DataObj*	GetCtrlData(const CString& /*sName*/, int /*nRow*/ = 0);
	virtual int			EnumColumnName(CStringArray& /*arNames*/, BOOL bAll = TRUE);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CMSStrButton - multi selection 
//=============================================================================
class TB_EXPORT CMSStrButton : public  CMSStrCombo
{
	DECLARE_DYNCREATE(CMSStrButton)

public:
	// Construction
	CMSStrButton();
	~CMSStrButton();

public:
	void SetImageNS(CString strImageNS);

	virtual void OnDraw(CDC* pDC, BOOL bDrawPrompt);

private:
	CBitmap m_CBmp;
	BOOL m_bIamge;

protected:
	virtual	BOOL	OnOpenDropDown(CPoint point);

	//{{AFX_MSG(CMSStrButton)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//==============================================================================
//		class CRadioCombo
//------------------------------------------------------------------------------
// Gestione di più DataBool in un unico combo box con funzionalità tipo
// radio button, ossia assegnazione a solo un DataBool del valore TRUE
//==============================================================================
class TB_EXPORT CRadioCombo : public CStrCombo
{ 
	DECLARE_DYNCREATE (CRadioCombo) 

public:
	CRadioCombo() : CStrCombo() {}

protected:
	DataObjArray	m_arDataBool;
	DataObjArray	m_arDataStr;

protected:
	virtual BOOL	OnInitCtrl				();
	virtual	void	OnFillListBox			();
	virtual void	GetValue				(DataObj& aValue);
	virtual	void	SetValue				(const DataObj& aValue);
	virtual	BOOL	DoSelection				();
	virtual BOOL	DoSetCurSel				(const DataObj&);
	virtual	void	DoSetValue				(const DataObj& aValue);
	virtual	void	SetValue				(LPCTSTR pszValue);
	virtual void	SetDataObjFromSelection	(int idx);
	virtual	CString	FormatData				(const DataObj* pDataObj, BOOL bEnablePadding = FALSE) const;
	virtual	BOOL	ForceUpdateCtrlView		(int i = -1);
	virtual DataStr	GetSelectedString		() const;
	virtual int		GetSelectedIndex		() const;
	virtual int		GetAssociatedText		(int, LPTSTR);
	virtual int		GetAssociatedTextLen	(int);
	virtual	void	ResetAssociations		(BOOL bRemoveAll = FALSE);

public:
	DataBool*	GetSelectedData	()	const;
	void		AddAssociation	(CString sDescription, DataBool* pBool);
}; 

//=============================================================================
#include "endh.dex"
