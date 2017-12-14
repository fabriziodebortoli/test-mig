#pragma once

#define RE_NONE 0
#define RE_MAIL 1
#define RE_FILE 2
#define RE_HTTP 3

#include "TBStrings.h"
#include "PARSCTRL.H"
#include "MiniHtmlStatic.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
//							CAutoCompleteProvider
//=============================================================================
class TB_EXPORT CAutoCompleteProvider : public CObject
{
	CStringArray*	m_pList;
	DataObjArray*	m_pData;

public:
	CAutoCompleteProvider();

public:
	void			SetList				(CStringArray* pList, DataObjArray* pDataArray = NULL);
	void			OnAutoComplete		(CString strValue, CStringList* pList);
	DataObj*		GetDataOf			(CString strItem);
	virtual BOOL	CanAutoComplete		(const CString& strValue, const CString& strItem);
};

//=============================================================================
//			Class CParsedEdit
//=============================================================================
class TB_EXPORT CParsedEdit : public CBCGPEdit, public CParsedCtrl, public CColoredControl, public CCustomFont, public IDisposingSourceImpl
{
	DECLARE_DYNAMIC (CParsedEdit)

private:
	CAutoCompleteProvider*	m_pAutoCompleteProvider;

protected:
	BOOL	m_bDisableSelection;

public:
	// Construction
	CParsedEdit(DataObj* pData = NULL);
	CParsedEdit(UINT nBtnIDBmp, DataObj* pData = NULL);
	virtual ~CParsedEdit();

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);


protected:
	HBRUSH CtlColor(CDC* pDC, UINT nCtlColor) { return CColoredControl::CtlColor(pDC, nCtlColor); }

public:
	// explicit creation
	BOOL Create			(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	
	// dynamic subclassing (don't use in conjunction with create)
	BOOL SubclassEdit	(UINT nID, CWnd* pParent, const CString& strName = _T(""));
	void SetAutoCompleteList(CStringArray* pAutoCompleteList, DataObjArray* pDataArray = NULL);

public:
	// virtual functions
	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	GetValue		(DataObj& aValue);
		
	virtual	void	SetModifyFlag	(BOOL f);
	virtual BOOL	GetModifyFlag	();

	virtual DataType	GetDataType	()	const = 0;

	virtual	BOOL	ForceUpdateCtrlView	(int i = -1);

	virtual void	SetEditReadOnly (const BOOL bValue);
			void	DisableSelection (BOOL bDisableSelection = TRUE) { m_bDisableSelection = bDisableSelection; }
	// Overridables
	virtual	void	SetCtrlSel		(int, int);
	virtual	void	GetCtrlSel		(int&, int&);
	virtual	void	SetCtrlMaxLen	(UINT, BOOL bApplyNow = TRUE);
	virtual	BOOL	OnInitCtrl		();

protected:
	virtual	BOOL	OnCommand				(WPARAM wParam, LPARAM lParam);
	virtual BOOL	OnGetAutoCompleteList	(const CString& strEditText, CStringList& lstAutocomplete);

	virtual	BOOL	OnChildNotify			(UINT uMsg, WPARAM wParam, LPARAM lParam, LRESULT* pResult);

private:
	BOOL	NeedCustomPaint();
	//An.21569
	BOOL	OnChangeInternal(CString strText);

protected:      
	//{{AFX_MSG(CParsedEdit)
	afx_msg	void	OnKillFocus						(CWnd*);
	afx_msg void	OnWindowPosChanging				(WINDOWPOS FAR* lpwndpos);
	afx_msg	void	OnEnable						(BOOL bEnable);
	afx_msg void	OnVScroll						(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg void	OnLButtonUp						(UINT nFlag, CPoint ptMousePos);
	afx_msg void	OnRButtonDown					(UINT nFlag, CPoint ptMousePos);
	afx_msg void	OnContextMenu					(CWnd* pWnd, CPoint ptMousePos);
	afx_msg void	OnKeyUp							(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void	OnKeyDown						(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void	OnChar							(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg	void 	OnSetFocus						(CWnd*);
	afx_msg	LRESULT	OnPushButtonCtrl				(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT	OnGetControlDescription			(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT	OnPaste							(WPARAM wParam, LPARAM lParam);
	afx_msg	void 	OnFormatPopupMenuItemSelected	(UINT nID);
	afx_msg	BOOL	OnEraseBkgnd					(CDC* pDC) { return CColoredControl::EraseBkgnd(pDC); }
	afx_msg	void 	OnPaint							( );

	afx_msg void	OnUndo();
	afx_msg void	OnPaste();
	afx_msg void	OnCopy();
	afx_msg void	OnCut();
	afx_msg void	OnClear();
	afx_msg void	OnSelectAll();

	//An.21569
	afx_msg	BOOL	OnChange();
	//}}AFX_MSG
	BOOL	CanEdit() const;

	DECLARE_MESSAGE_MAP() 

	void DestroyCalendar();
public:
	void AppendEditMenu(CMenu* pMenu);
};

//=============================================================================
//			Class CStrEdit
//=============================================================================
class TB_EXPORT CStrEdit : public CParsedEdit, public ResizableCtrl
{
	DECLARE_DYNCREATE (CStrEdit)
	
public:
	// Construction
	CStrEdit();
	CStrEdit(UINT nBtnIDBmp, DataStr* = NULL);

public:
	// Overridables
	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	GetValue		(DataObj& aValue);
	virtual BOOL	IsValid			()						{ return CParsedEdit::IsValid() && IsValidStr(GetValue()); }
	virtual BOOL	IsValid			(const DataObj& aValue)	{ return CParsedEdit::IsValid(aValue) &&
																	 IsValidStr(((DataStr&)aValue).GetString()); }
	
	virtual void		Attach		(DataObj*);
	virtual DataType	GetDataType	()	const				{ return DataType::String; }

	virtual	BOOL		DoOnChar	(UINT nChar);

	virtual	BOOL		SubclassEdit(UINT, CWnd*, const CString& strName = _T(""));

public:
	// specific public members
	void			SetValue	(LPCTSTR strValue)	{ CParsedCtrl::SetValue(strValue); }
	virtual void	GetValue	(CString& strValue)	{ CParsedCtrl::GetValue(strValue); }
	virtual CString	GetValue	();                       
	
	BOOL		IsValidStr	(LPCTSTR);

	void	InitSizeInfo	()	{ ResizableCtrl::InitSizeInfo(this); }

	void	PositionCursor	(DWORD dwCurPos);
	void	PositionCursor	(int nLine, int nCol);

	//BCGP Browse button extensions
	void EnableFindBrowseButton(BOOL bEnable = TRUE, 
								LPCTSTR szLabel = L"", 
								BOOL bRedraw = TRUE);
	virtual void OnBrowse();

protected:      
	// Overridables
	virtual	BOOL	OnInitCtrl	();

protected:      
	//{{AFX_MSG(CStrEdit)
	afx_msg	void 	OnKillFocus			(CWnd*);
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CResizableStrEdit
//=============================================================================
class TB_EXPORT CResizableStrEdit : public CStrEdit
{
	DECLARE_DYNCREATE (CResizableStrEdit)

public:
	CResizableStrEdit	();
	CResizableStrEdit	(UINT nBtnIDBmp, DataStr* = NULL);
};

//=============================================================================
//			Class CTextEdit
//=============================================================================
class TB_EXPORT CTextEdit : public CResizableStrEdit
{
	DECLARE_DYNCREATE (CTextEdit)
	
public:
	// Construction
	CTextEdit();
	CTextEdit(UINT nBtnIDBmp, DataText* = NULL);

public:
	virtual DataType	GetDataType	()	const	{ return DataType::Text; }

protected:      
	//{{AFX_MSG(CTextEdit)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CIntEdit
//=============================================================================
class TB_EXPORT CIntEdit : public CParsedEdit
{
	DECLARE_DYNCREATE (CIntEdit)
	
protected:
	DataInt		m_nMin;
	DataInt		m_nMax;
	int			m_nCurValue;

public:
	// Construction
	CIntEdit();
	CIntEdit(UINT nBtnIDBmp, DataInt* = NULL);

public:
	// virtual members
    virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);
			void		SetValue		(int nValue);
			int			GetValue		();        

			BOOL		IsValidInt		(int nVal);
	virtual BOOL		IsValid			()						{ return CParsedEdit::IsValid() && IsValidInt(GetValue()); }
	virtual BOOL		IsValid			(const DataObj& aValue)	{ return CParsedEdit::IsValid(aValue) &&
																		 IsValidInt((int) ((DataInt &)aValue)); }

	virtual DataType	GetDataType		()	const				{ return DataType::Integer; }

	virtual	CString		FormatErrorMessage	(MessageID nIDP, LPCTSTR pszBadVal);
	virtual void		DoSpinScroll		(UINT nSBCode);
	virtual	BOOL		DoOnChar			(UINT nChar);
	virtual	BOOL		DoKeyDown			(UINT nChar);
	virtual	void		DoKillFocus 		(CWnd* aWnd);

	virtual	DataObj*	GetMinValue		() { return &m_nMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_nMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_nMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_nMax.Assign(value); }
	virtual void		SetRange		(int nMin, int nMax);

protected:      
	// Overridables
	virtual	BOOL	OnInitCtrl	();

	afx_msg	LRESULT OnFormatStyleChange				(WPARAM wParam, LPARAM lParam);

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CLongEdit
//=============================================================================
class TB_EXPORT CLongEdit : public CParsedEdit
{
	DECLARE_DYNCREATE (CLongEdit)
	
protected:
	DataLng	m_lMin;
	DataLng	m_lMax;
	long	m_lCurValue;

public:
	// Construction
	CLongEdit();
	CLongEdit(UINT nBtnIDBmp, DataLng* = NULL);

public:
	// virtual members
	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);
	virtual BOOL		IsValid			()						{ return CParsedEdit::IsValid() && IsValidLong(GetValue()); }
	virtual BOOL		IsValid			(const DataObj& aValue)	{ return CParsedEdit::IsValid(aValue) &&
																		 IsValidLong((long) ((DataLng &)aValue)); }

	virtual DataType	GetDataType		()	const				{ return DataType::Long; }

	virtual	CString		FormatErrorMessage	(MessageID nIDP, LPCTSTR pszBadVal);
	virtual void		DoSpinScroll		(UINT nSBCode);
	virtual	BOOL		DoOnChar			(UINT nChar);
	virtual	BOOL		DoKeyDown			(UINT nChar);
	virtual	void		DoKillFocus 		(CWnd* aWnd);

public:
	BOOL IsValidChar(UINT, const CString&, DWORD, int);
		
public:
    // edit value management
	void	SetValue	(long nValue);
	long	GetValue	();

	BOOL	IsValidLong	(long nVal);

	virtual	DataObj*	GetMinValue		() { return &m_lMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_lMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_lMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_lMax.Assign(value); }
	virtual void		SetRange		(int nMin, int nMax);

protected:      
	// Overridables
	virtual	BOOL	OnInitCtrl	();

	afx_msg	LRESULT OnFormatStyleChange				(WPARAM wParam, LPARAM lParam);

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CDoubleEdit
//=============================================================================
class TB_EXPORT CDoubleEdit : public CParsedEdit
{
	DECLARE_DYNCREATE (CDoubleEdit)
	
protected:
	DataDbl		m_dMin				= -DBL_MAX;
	DataDbl		m_dMax				= DBL_MAX;
	int			m_nDec				= DEFAULT_N_DEC;
	double		m_dCurValue			= 0.;
	DataInt*	m_pExternalNumDec	= NULL;
	
public:
	// Construction
	CDoubleEdit();
	CDoubleEdit(UINT nBtnIDBmp, DataDbl* = NULL);

public:
	// virtual members
	virtual	void		SetCtrlNumDec	(int nNumDec)						{ m_nDec = nNumDec; }
	virtual	void		SetCtrlNumDecByProvider	(DataInt* pExternalNumDec)	{ m_pExternalNumDec = pExternalNumDec; }
	virtual	int			GetCtrlNumDec	();

	virtual	DataObj*	GetMinValue		() { return &m_dMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_dMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_dMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_dMax.Assign(value); }
	virtual void		SetRange		(double nMin, double nMax, int nDec = DEFAULT_N_DEC);

	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);
			void		SetValue		(double nValue);
			double		GetValue		();

			BOOL		IsValidDouble	(double nVal);
	virtual BOOL		IsValid			()						{ return CParsedEdit::IsValid() && IsValidDouble(GetValue()); }
	virtual BOOL		IsValid			(const DataObj& aValue)	{ return CParsedEdit::IsValid(aValue) &&
																		 IsValidDouble((double) ((DataDbl &)aValue)); }
																		 
	virtual DataType	GetDataType		()	const				{ return DataType::Double; }

	virtual	CString		FormatErrorMessage	(MessageID nIDP, LPCTSTR pszBadVal);
	virtual void		DoSpinScroll		(UINT nSBCode);
	virtual	BOOL		DoOnChar			(UINT nChar);
	virtual	BOOL		DoKeyDown			(UINT nChar);
	virtual	void		DoKillFocus 		(CWnd* aWnd);

	virtual	CString		FormatData			(const DataObj* pDataObj, BOOL bEnablePadding = FALSE) const;

	CDblFormatter* GetFormatter() const;

protected:      
	LRESULT FormatValue(DataDbl& aValue);

	// Overridables
	virtual	BOOL	OnInitCtrl	();

	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CMoneyEdit
//=============================================================================
class TB_EXPORT CMoneyEdit : public CDoubleEdit
{
	DECLARE_DYNCREATE (CMoneyEdit)

public:
	// Construction
	CMoneyEdit();
	CMoneyEdit(UINT nBtnIDBmp, DataMon* = NULL);

public:
	// virtual members
	virtual	void		SetValue	(const DataObj& aValue);
	virtual	void		GetValue	(DataObj& aValue);

	virtual DataType	GetDataType	()	const		{ return DataType::Money; }


protected:
	//{{AFX_MSG(CMoneyEdit)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CQuantityEdit
//=============================================================================
class TB_EXPORT CQuantityEdit : public CDoubleEdit
{
	DECLARE_DYNCREATE (CQuantityEdit)
	
public:
	// Construction
	CQuantityEdit();
	CQuantityEdit(UINT nBtnIDBmp, DataQty* = NULL);

public:
	// virtual members
	virtual	void		SetValue	(const DataObj& aValue);
	virtual	void		GetValue	(DataObj& aValue);

	virtual DataType	GetDataType	()	const	{ return DataType::Quantity; }

	
protected:
	//{{AFX_MSG(CQuantityEdit)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CPercEdit
//=============================================================================
class TB_EXPORT CPercEdit : public CDoubleEdit
{
	DECLARE_DYNCREATE (CPercEdit)
	
public:
	// Construction
	CPercEdit();
	CPercEdit(UINT nBtnIDBmp, DataPerc* = NULL);

public:
	// virtual members
	virtual	void		SetValue	(const DataObj& aValue);
	virtual	void		GetValue	(DataObj& aValue);

	virtual DataType	GetDataType	()	const	{ return DataType::Percent; }

public:
    // edit value management
	virtual void		SetRange	(double nMin, double nMax, int nDec = 0);

protected:
	//{{AFX_MSG(CPercEdit)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CMetricEdit  
//=============================================================================
class TB_EXPORT CMetricEdit : public CDoubleEdit
{
	DECLARE_DYNCREATE (CMetricEdit)
	
protected:
	double			m_nScale;
	MeasureUnits	m_UserMeasureUnits;
	
public:
	// Construction
	CMetricEdit();
	CMetricEdit(UINT nBtnIDBmp);
	
public:
	// virtual members
	virtual	CString	GetSpecialCaption	();

public:
    // edit value management
	// to input dimension between nMin and nMax in mu / scale measurement units (i.e. cm/10 => mm)
	virtual void	SetRange			(int nMin, int nMax, int nDec = DEFAULT_N_DEC, double nScale = DEFAULT_SCALING);
	
	void			SetMeasureUnits		(double aScale, MeasureUnits mu = STD_MU);
	MeasureUnits    GetMeasureUnits		()	{ return m_UserMeasureUnits;}
	
	void			SetValue			(int nValue);
	int				GetValue			();

protected:      
	// Overridables
	virtual	BOOL OnInitCtrl	();
};


//=============================================================================
//			Class CDateEdit
//=============================================================================
class TB_EXPORT CDateEdit : public CParsedEdit
{
	DECLARE_DYNCREATE (CDateEdit)
	
protected:
	DataDate	m_lMin;
	DataDate	m_lMax;
	long	m_nCurDate;
	//Usato per memorizzare la data originale nel caso di selezione dal dateRangeSelector
	long	m_nOriginalDate;
	
public:
	// Construction
	CDateEdit();
	CDateEdit(UINT nBtnIDBmp, DataDate* = NULL);

public:
	// virtual members
	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);
	virtual BOOL		IsValid			()						{ return CParsedEdit::IsValid() && IsValidDate(GetValue()); }
	virtual BOOL		IsValid			(const DataObj& aValue)	{ return CParsedEdit::IsValid(aValue) &&
																		 IsValidDate((long) ((DataDate &)aValue)); }
	
	virtual DataType	GetDataType		()	const	{ return DataType::Date; }

	virtual	CString		FormatErrorMessage	(MessageID nIDP, LPCTSTR pszBadVal);
	virtual void		DoSpinScroll		(UINT nSBCode);
	virtual	BOOL		DoOnChar			(UINT nChar);

    // edit value management
	void	SetValue	(long nValue);
	void	SetValue	(WORD d, WORD m, WORD y);
	long	GetValue	();

	BOOL	IsValidDate	(long nVal);

	virtual	DataObj*	GetMinValue		() { return &m_lMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_lMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_lMin = (long) ((const DataDate&) value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_lMax = (long) ((const DataDate&) value); }
	virtual void		SetRange		(int nMin, int nMax);
			void		SetRange		(WORD dMin, WORD mMin, WORD yMin, WORD dMax, WORD mMax, WORD yMax);

protected:      
	// Overridables
	virtual	BOOL	OnInitCtrl				();
	virtual	void	DoKillFocus 			(CWnd*);

public:
	virtual void	Attach					(UINT /*nBtnID*/);

	virtual BOOL	GetToolTipProperties	(CTooltipProperties&);
		
protected:
	//{{AFX_MSG(CDateEdit)
	afx_msg	LRESULT OnFormatStyleChange		(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT OnGetParsedCtrlType		(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT OnRangeSelectorClosed	(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT OnRangeSelectorSelected (WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CDateSpin : public CSpinButtonCtrl
{
	DECLARE_DYNCREATE(CDateSpin)

	CDateSpinEdit*	m_pDateEdit;

public:
	CDateSpin();
	BOOL CDateSpin::Create(CDateSpinEdit*, const RECT&, UINT);

protected:
	// Generated message map functions
	//{{AFX_MSG(CDateSpin)
	afx_msg void OnLButtonDown(UINT, CPoint);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CDateSpinEdit : public CDateEdit
{
	DECLARE_DYNCREATE(CDateSpinEdit)

	CDateSpin*	m_pDateSpin;
	long		m_nStartDate;

public:
	CDateSpinEdit();
	~CDateSpinEdit();

	BOOL		SubclassEdit(UINT nID, CWnd* pParent, const CString& strName = _T(""));
	CDateSpin*	AttachSpin	(UINT nSpinID = IDC_STATIC);

	void		UpdateStartDate();
	void		SetStartDate	(long nStartDate)	{m_nStartDate = nStartDate;}
	long		GetStartDate	()	const			{return m_nStartDate;}

	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	DoKillFocus		(CWnd*);
	virtual void	Attach	(UINT /*nBtnID*/) { m_nButtonIDBmp = 0; }
	
protected:
	// Generated message map functions
	//{{AFX_MSG(CDateSpinEdit)
	LRESULT OnSetTimeText(WPARAM, LPARAM);
	afx_msg void OnEnable(BOOL);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CDateTimeEdit
//=============================================================================
class TB_EXPORT CDateTimeEdit : public CDateEdit
{
	DECLARE_DYNCREATE (CDateTimeEdit)
	
protected:
	long	m_nCurTime;
	
public:
	// Construction
	CDateTimeEdit();
	CDateTimeEdit(UINT nBtnIDBmp, DataDate* = NULL);

public:
	// virtual members
	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);
	virtual BOOL		IsValid			()						{ return CParsedEdit::IsValid() && IsValidDateTime(GetValue()); }
	virtual BOOL		IsValid			(const DataObj& aValue)	{ return CParsedEdit::IsValid(aValue) &&
																		 IsValidDateTime(((DataDate &)aValue).GetDateTime()); }
	virtual	DBTIMESTAMP	GetValue();
	
	virtual DataType	GetDataType		()	const	{ return DataType::DateTime; }

	virtual	BOOL		DoOnChar		(UINT nChar);

public:
    // edit value management
	void				SetValue		(const DBTIMESTAMP&);
	BOOL				IsValidDateTime	(const DBTIMESTAMP&);

protected:      
	// Overridables
	virtual	BOOL OnInitCtrl	();

protected:
	//{{AFX_MSG(CDateTimeEdit)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CTimeEdit
//=============================================================================
class TB_EXPORT CTimeEdit : public CDateTimeEdit
{
	DECLARE_DYNCREATE (CTimeEdit)
	
public:
	// Construction
	CTimeEdit();
	CTimeEdit(UINT nBtnIDBmp, DataDate* = NULL);

public:
	// virtual members
	virtual	void		GetValue		(DataObj& aValue);

	virtual	DBTIMESTAMP	GetValue();

	virtual DataType	GetDataType		()	const	{ return DataType::Time; }
	virtual void Attach (UINT /*nBtnID*/) { m_nButtonIDBmp = 0; }

protected:
	//{{AFX_MSG(CTimeEdit)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

protected:      
    DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CTimeSpin : public CSpinButtonCtrl
{
	DECLARE_DYNCREATE(CTimeSpin)

	CTimeSpinEdit*	m_pTimeEdit;

public:
	CTimeSpin();
	BOOL CTimeSpin::Create(CTimeSpinEdit*, const RECT&, UINT);
protected:
	// Generated message map functions
	//{{AFX_MSG(CTimeSpin)
	afx_msg void OnLButtonDown(UINT, CPoint);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CTimeSpinEdit : public CTimeEdit
{
	DECLARE_DYNCREATE(CTimeSpinEdit)

	CTimeSpin*	m_pTimeSpin;

public:
	CTimeSpinEdit();
	~CTimeSpinEdit();

	BOOL		SubclassEdit(UINT nID, CWnd* pParent, const CString& strName = _T(""));
	CTimeSpin*	AttachSpin	(UINT nSpinID = IDC_STATIC);

	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	DoKillFocus		(CWnd*);

protected:
	// Generated message map functions
	//{{AFX_MSG(CTimeSpinEdit)
	LRESULT OnSetTimeText(WPARAM, LPARAM);
	afx_msg void OnEnable(BOOL);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//@@ElapsedTime			Class CElapsedTimeEdit
//=============================================================================
class TB_EXPORT CElapsedTimeEdit : public CParsedEdit
{
	DECLARE_DYNCREATE (CElapsedTimeEdit)
	
protected:
	DataLng	m_lMin;
	DataLng	m_lMax;
	long	m_nCurTime;
	
public:
	// Construction
	CElapsedTimeEdit();
	CElapsedTimeEdit(UINT nBtnIDBmp, DataLng* = NULL);

public:
	// virtual members
	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);
			void		SetValue		(long nValue);
			long		GetValue		();

			BOOL		IsValidTime		(long nVal);

	virtual BOOL		IsValid			()						{ return CParsedEdit::IsValid() && IsValidTime(GetValue()); }
	virtual BOOL		IsValid			(const DataObj& aValue)	{ return CParsedEdit::IsValid(aValue) &&
																		 IsValidTime((long) ((DataLng &)aValue)); }
	
	virtual DataType	GetDataType		()	const	{ return DataType::ElapsedTime; }

	virtual	CString		FormatErrorMessage	(MessageID nIDP, LPCTSTR pszBadVal);
	virtual void		DoSpinScroll		(UINT nSBCode);
	virtual	BOOL		DoOnChar			(UINT nChar);
	virtual	void		DoKillFocus 		(CWnd* aWnd);

	virtual	CString		GetSpecialCaption	();
public:
    // edit value management
protected:      
	// Overridables
	virtual	BOOL OnInitCtrl	();
	
public:
	virtual	DataObj*	GetMinValue		() { return &m_lMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_lMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_lMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_lMax.Assign(value); }
	virtual void		SetRange		(int nMin, int nMax);

protected:
	//{{AFX_MSG(CDateEdit)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CBoolEdit 
//=============================================================================
class TB_EXPORT CBoolEdit : public CParsedEdit
{
	DECLARE_DYNCREATE (CBoolEdit)
	
public:
	// Construction
	CBoolEdit();
	CBoolEdit(UINT nBtnIDBmp, DataBool* = NULL);

public:
	// virtual members
	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);
	virtual BOOL		IsValid			()						{ return CParsedEdit::IsValid(); }
	virtual BOOL		IsValid			(const DataObj& aValue)	{ return CParsedEdit::IsValid(aValue); }
	
	virtual DataType	GetDataType		()	const				{ return DataType::Bool; }

	virtual	BOOL		DoOnChar		(UINT nChar);

public:
    // edit value management
	void	SetValue	(BOOL nValue);
	BOOL	GetValue	();

protected:
	//{{AFX_MSG(CBoolEdit)
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CIdentifierEdit 
//=============================================================================
class TB_EXPORT CIdentifierEdit : public CStrEdit
{
	DECLARE_DYNCREATE (CIdentifierEdit)
	
protected:
	SymTable*	m_pSymTable;
	CObject*	m_pItemToCheck;
	
public:
	// Construction
	CIdentifierEdit();
	CIdentifierEdit(SymTable*, DataStr*, CObject* pItem = NULL, UINT nBtnIDBmp = NO_BUTTON);
	CIdentifierEdit(SymTable*, CObject* pItem = NULL, UINT nBtnIDBmp = NO_BUTTON, DataStr* = NULL);

public:
    // virtual functions
	virtual BOOL	IsValid		();
	virtual	BOOL	DoOnChar	(UINT nChar);

protected:
	//{{AFX_MSG(CIdentifierEdit)
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CGuidEdit
//=============================================================================
class TB_EXPORT CGuidEdit : public CParsedEdit
{
	DECLARE_DYNCREATE (CGuidEdit)
	
protected:
	GUID m_guidCurValue;

public:
	// Construction
	CGuidEdit();
	CGuidEdit(UINT nBtnIDBmp, DataGuid* = NULL);

public:
	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);
	virtual DataType	GetDataType		() const { return DataType::Guid; }

public:
    // edit value management
	void	SetValue	(GUID nValue);
	GUID	GetValue	();                       

public:
	BOOL IsValidChar(UINT, const CString&, DWORD, int);

public:
	virtual void	Attach		(DataObj*);
	virtual	BOOL	DoOnChar	(UINT nChar);
	virtual	void	DoKillFocus (CWnd* aWnd);

protected:      
	//{{AFX_MSG(CGuidEdit)
	afx_msg	LRESULT OnFormatStyleChange (WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//=============================================================================
//			Class CParsedStatic
//=============================================================================
class CParsedLabelDescription;
class TB_EXPORT CParsedStatic : public CBCGPStatic, public CParsedCtrl, public CColoredControl, public CCustomFont, public IDisposingSourceImpl
{
	DECLARE_DYNAMIC (CParsedStatic)
public:
	// Construction
	CParsedStatic(DataObj *pData = NULL);
	CParsedStatic(UINT nBtnIDBmp, DataObj *pData = NULL);
	virtual ~CParsedStatic();

public:
	BOOL			m_bRightAnchor;

protected:
	HBRUSH	CtlColor(CDC* pDC, UINT nCtlColor) { return CColoredControl::CtlColor(pDC, nCtlColor); }

	// Overridables
	virtual	BOOL OnInitCtrl	();

public:
	// explicit creation
	BOOL Create			(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	
	// dynamic subclassing (don't use in conjunction with create)
	BOOL SubclassEdit(UINT nID, CWnd* pParent, const CString& strName = _T(""));

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

public:
	// virtual functions
	
	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	SetValue		(LPCTSTR pszValue = NULL);

	virtual	void	GetValue		(DataObj& aValue);
	virtual	void	GetValue		(CString& strValue);
	
	virtual DataType GetDataType	()	const	= 0;

	virtual	BOOL	ForceUpdateCtrlView	(int = -1);

	static	CRuntimeClass* GetDefaultClassFor (DataObj* pDataObj);

public:
	BOOL	HasEdgeStyle();
	BOOL	HasSunkenStyle();

	void	PaintStatic				();	//DT_MULTILINE

protected:      
	virtual void GetControlDescription(CParsedLabelDescription* pDesc);
	afx_msg void	OnWindowPosChanging	(WINDOWPOS FAR* lpwndpos);
	afx_msg	void	OnEnable			(BOOL bEnable);
	afx_msg	void	OnVScroll			(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg	LRESULT	OnPushButtonCtrl	(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT	OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT	OnGetControlDescription	(WPARAM wParam, LPARAM lParam);

	afx_msg	void	OnNcPaint		();
	afx_msg	void	OnPaint			();
	afx_msg	BOOL	OnEraseBkgnd	(CDC* pDC);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CStrStatic
//=============================================================================
class TB_EXPORT CStrStatic : public CParsedStatic
{
	DECLARE_DYNCREATE (CStrStatic)

public:
	// Construction
	CStrStatic();
	CStrStatic(UINT nBtnIDBmp, DataStr* = NULL);

public:
	// Overridables
	virtual	void		SetValue	(const DataObj& aValue);
	virtual	void		SetValue	(LPCTSTR pszValue = NULL)		{ __super::SetValue(pszValue); }
	virtual	void		SetValue	(const CString& sValue)			{ __super::SetValue((LPCTSTR)sValue); }

	virtual	void		GetValue	(DataObj& aValue);
	virtual	void		GetValue	(CString& strValue)				{ __super::GetValue(strValue); }
	CString				GetValue	() { CString str; __super::GetValue(str); return str; }

	virtual DataType	GetDataType	()	const { return DataType::String; }

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CResizableStrStatic
//=============================================================================
class TB_EXPORT CResizableStrStatic : public CStrStatic, public ResizableCtrl
{
	DECLARE_DYNCREATE (CResizableStrStatic)

public:
	CResizableStrStatic	() {}
	CResizableStrStatic	(UINT nBtnIDBmp, DataStr* = NULL);

	virtual	BOOL	SubclassEdit	(UINT, CWnd*, const CString& strName = _T(""));

	void  InitSizeInfo() { ResizableCtrl::InitSizeInfo(this); }

protected:
	//{{AFX_MSG(CResizableStrStatic)
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CTextStatic
//=============================================================================
class TB_EXPORT CTextStatic : public CResizableStrStatic
{
	DECLARE_DYNCREATE (CTextStatic)
	
public:
	// Construction
	CTextStatic();
	CTextStatic(UINT nBtnIDBmp, DataText* = NULL);

public:
	virtual DataType	GetDataType	()	const		{ return DataType::Text; }

protected:      
	//{{AFX_MSG(CStrStatic)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CLabelStatic
//=============================================================================
class TB_EXPORT CLabelStatic : public CResizableStrStatic
{
	friend class CParsedForm;

	DECLARE_DYNCREATE (CLabelStatic)

public:
	enum	ELinePos { LP_NONE = 0, LP_TOP=1, LP_VCENTER=2, LP_BOTTOM=4, LP_LEFT=8, LP_RIGHT=16, LP_HCENTER=32 };

protected:
	BOOL		m_bShowSeparator;
	BOOL		m_bVerticalSeparator;
	ELinePos	m_LinePos = LP_NONE;	//bitvector
	BOOL		m_bMiniHtml;
public:
	CArray<HWND> m_arVirtualChilds;
	
public:
	CLabelStatic();
	ELinePos GetLinePosition() { return m_LinePos; }
	void SetLinePosition(ELinePos pos) { m_LinePos = pos; }
	virtual	BOOL	ForceUpdateCtrlView	(int = -1) { return TRUE; }

	void	ShowTextWithLine		(COLORREF crBorder, int nSizePen = 1, /*ELinePos*/int pos = LP_VCENTER);
	void	ShowSeparator			(COLORREF crBorder, int nSizePen = 1, BOOL  bVertical = FALSE, ELinePos pos = LP_NONE);
	void	ShowMiniHtml			();

	void	SetZOrderInnerControls	(CWnd* pParentWnd= NULL);

	void	SetFontStyleAndColor	(BOOL bBold, BOOL bItalic, BOOL bUnderline, COLORREF aColor);


	virtual	void	SetOwnFont(BOOL bBold, BOOL bItalic, BOOL bUnderline, int nPointSize = 0, LPCTSTR lpszFaceName = NULL);
	virtual void	SetOwnFont(CFont* pFont, BOOL bOwns = FALSE);

	BOOL  IsShowSeparator() const { return m_bShowSeparator; }
	BOOL  IsShowTextWithLine() const { return m_LinePos != LP_NONE; }

protected:      
	afx_msg void	OnPaint				();
	afx_msg BOOL	OnEraseBkgnd		(CDC* pDC);

	void	PaintStaticWithLine		();	//DT_SINGLELINE
	void	PaintSeparator			();
	void	PaintMiniHtml			();
	virtual void GetControlDescription(CParsedLabelDescription* pDesc);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class EnumParsedLabelDescriptionAssociations
//=============================================================================
// Singleton containing all the enum-description associations
class EnumParsedLabelDescriptionAssociations
{
public:
	EnumDescriptionCollection<CLabelStatic::ELinePos>	m_arELinePos;
	
public:
	EnumParsedLabelDescriptionAssociations()
	{
		InitEnumParsedLabelDescriptionStructures();
	}

	void InitEnumParsedLabelDescriptionStructures();
};

//=============================================================================
//			Class CLabelStaticDescription
//=============================================================================
class TB_EXPORT CParsedLabelDescription : public CLabelDescription
{
	DECLARE_DYNCREATE(CParsedLabelDescription);

	static EnumParsedLabelDescriptionAssociations singletonParsedLabelDescription;

	CLabelStatic::ELinePos m_LinePos = CLabelStatic::LP_NONE;
public:
	CParsedLabelDescription() {}
	CParsedLabelDescription(CWndObjDescription* pParent) : CLabelDescription(pParent) {}
	void SerializeJson(CJsonSerializer& strJson);
	void ParseJson(CJsonFormParser& parser);
	void Assign(CWndObjDescription* pDesc);
	
	static	CString		GetEnumDescription(CLabelStatic::ELinePos value);
	static	void		GetEnumValue(CString description, CLabelStatic::ELinePos& retVal);
};
//=============================================================================
//			Class CIntStatic
//=============================================================================
class TB_EXPORT CIntStatic : public CParsedStatic
{
	DECLARE_DYNCREATE (CIntStatic)

protected:
	DataInt		m_nMin;
	DataInt		m_nMax;

public:
	// Construction
	CIntStatic();
	CIntStatic(UINT nBtnIDBmp, DataInt* = NULL);

public:
	// Overridables
	virtual	void		SetValue		(const DataObj& aValue);
	void				SetValue		(int nValue);
	int					GetValue		();
	virtual	void		GetValue		(DataObj& aValue);
    // Static value management
	

	virtual DataType	GetDataType		()	const	{ return DataType::Integer; }

	virtual void		DoSpinScroll	(UINT nSBCode);

	virtual	DataObj*	GetMinValue		() { return &m_nMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_nMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_nMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_nMax.Assign(value); }
	virtual void		SetRange		(int nMin, int nMax);

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CLongStatic
//=============================================================================
class TB_EXPORT CLongStatic : public CParsedStatic
{
	DECLARE_DYNCREATE (CLongStatic)

protected:
	DataLng	m_lMin;
	DataLng	m_lMax;

public:
	// Construction
	CLongStatic();
	CLongStatic(UINT nBtnIDBmp, DataLng* = NULL);

public:
	// Overridables
	virtual	void		SetValue		(const DataObj& aValue);
	void				SetValue(long nValue);

	long				GetValue();
	virtual	void		GetValue		(DataObj& aValue);
    // Static value management
	

	virtual DataType	GetDataType		()	const	{ return DataType::Long; }

	virtual void		DoSpinScroll	(UINT nSBCode);

	virtual	DataObj*	GetMinValue		() { return &m_lMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_lMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_lMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_lMax.Assign(value); }
	virtual void		SetRange		(int nMin, int nMax);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CDoubleStatic
//=============================================================================
class TB_EXPORT CDoubleStatic : public CParsedStatic
{
	DECLARE_DYNCREATE (CDoubleStatic)

protected:
	DataDbl		m_dMin;
	DataDbl		m_dMax;
	int			m_nDec;
	double		m_dCurValue;
	DataInt*	m_pExternalNumDec;

public:
	// Construction
	CDoubleStatic();
	CDoubleStatic(UINT nBtnIDBmp, DataDbl* = NULL);

public:
	// Overridables
	virtual	void		SetCtrlNumDec			(int nNumDec)					{ m_nDec = nNumDec; }
	virtual	void		SetCtrlNumDecByProvider	(DataInt* pExternalNumDec)		{ m_pExternalNumDec = pExternalNumDec; }
	virtual	int			GetCtrlNumDec	();

	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);
    // Static value management
			void		SetValue		(double nValue);
			double		GetValue		();                       

	virtual DataType	GetDataType		()	const	{ return DataType::Double; }

	virtual void		DoSpinScroll	(UINT nSBCode);

	virtual	CString		FormatData		(const DataObj* pDataObj, BOOL bEnablePadding = FALSE) const;
			CDblFormatter* GetFormatter() const;

	virtual	DataObj*	GetMinValue		() { return &m_dMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_dMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_dMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_dMax.Assign(value); }
	virtual void		SetRange		(double nMin, double nMax, int nDec = DEFAULT_N_DEC);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CMoneyStatic
//=============================================================================
class TB_EXPORT CMoneyStatic : public CDoubleStatic
{
	DECLARE_DYNCREATE (CMoneyStatic)

public:
	// Construction
	CMoneyStatic();
	CMoneyStatic(UINT nBtnIDBmp, DataMon* = NULL);

public:
	// Overridables
	virtual	void		SetValue	(const DataObj& aValue);
	virtual	void		GetValue	(DataObj& aValue);

	virtual DataType	GetDataType	()	const	{ return DataType::Money; }

protected:
	//{{AFX_MSG(CMoneyStatic)
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CQuantityStatic
//=============================================================================
class TB_EXPORT CQuantityStatic : public CDoubleStatic
{
	DECLARE_DYNCREATE (CQuantityStatic)

public:
	// Construction
	CQuantityStatic();
	CQuantityStatic(UINT nBtnIDBmp, DataQty* = NULL);

public:
	// Overridables
	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);

	virtual DataType	GetDataType	()	const	{ return DataType::Quantity; }

protected:
	//{{AFX_MSG(CQuantityStatic)
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CPercStatic
//=============================================================================
class TB_EXPORT CPercStatic : public CDoubleStatic
{
	DECLARE_DYNCREATE (CPercStatic)

public:
	// Construction
	CPercStatic();
	CPercStatic(UINT nBtnIDBmp, DataPerc* = NULL);

public:
	// Overridables
	virtual	void		SetValue	(const DataObj& aValue);
	virtual	void		GetValue	(DataObj& aValue);

	virtual DataType	GetDataType	()	const	{ return DataType::Percent; }

    // edit value management
	virtual void		SetRange	(double nMin, double nMax, int nDec = DEFAULT_N_DEC);

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CDateStatic
//=============================================================================
class TB_EXPORT CDateStatic : public CParsedStatic
{
	DECLARE_DYNCREATE (CDateStatic)

protected:
	DataDate	m_lMin;
	DataDate	m_lMax;
	long		m_nCurDate;
	
public:
	// Construction
	CDateStatic();
	CDateStatic(UINT nBtnIDBmp, DataDate* = NULL);

public:
	// Overridables
	virtual	void		SetValue	(const DataObj& aValue);
	virtual	void		GetValue	(DataObj& aValue);
	
	virtual DataType	GetDataType	()	const	{ return DataType::Date; }

	virtual void		DoSpinScroll(UINT nSBCode);

	virtual BOOL		GetToolTipProperties	(CTooltipProperties&);

public:
    // Static value management
	virtual void	SetRange	(int nMin, int nMax);
			void	SetRange	(WORD dMin, WORD mMin, WORD yMin, WORD dMax, WORD mMax, WORD yMax);

	void	SetValue	(long nValue);
	void	SetValue	(WORD d, WORD m, WORD y);
	long	GetValue	();

	virtual	DataObj*	GetMinValue		() { return &m_lMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_lMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_lMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_lMax.Assign(value); }

protected:
	//{{AFX_MSG(CDateStatic)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CDateTimeStatic
//=============================================================================
class TB_EXPORT CDateTimeStatic : public CDateStatic
{
	DECLARE_DYNCREATE (CDateTimeStatic)
	
protected:
	long	m_nCurTime;

public:
	// Construction
	CDateTimeStatic();
	CDateTimeStatic(UINT nBtnIDBmp, DataDate* = NULL);

public:
	// virtual members
	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);

	virtual	DBTIMESTAMP	GetValue();
	
	virtual DataType	GetDataType		()	const	{ return DataType::DateTime; }

public:
    // Static value management
	void				SetValue		(const DBTIMESTAMP&);

protected:
	//{{AFX_MSG(CDateTimeStatic)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CTimeStatic
//=============================================================================
class TB_EXPORT CTimeStatic : public CDateTimeStatic
{
	DECLARE_DYNCREATE (CTimeStatic)
	
public:
	// Construction
	CTimeStatic();
	CTimeStatic(UINT nBtnIDBmp, DataDate* = NULL);

public:
	virtual	void		GetValue		(DataObj& aValue);

	virtual	DBTIMESTAMP	GetValue();

	virtual DataType	GetDataType		()	const	{ return DataType::Time; }

protected:
	//{{AFX_MSG(CTimeStatic)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//@@ElapsedTime			Class CElapsedTimeStatic
//=============================================================================
class TB_EXPORT CElapsedTimeStatic : public CParsedStatic
{
	DECLARE_DYNCREATE (CElapsedTimeStatic)

protected:
	DataLng	m_lMin;
	DataLng	m_lMax;
	long	m_nCurTime;
	
public:
	// Construction
	CElapsedTimeStatic();
	CElapsedTimeStatic(UINT nBtnIDBmp, DataLng* = NULL);

public:
	// Overridables
	virtual	void		SetValue	(const DataObj& aValue);
	virtual	void		GetValue	(DataObj& aValue);
			void		SetValue	(long nValue);
			long		GetValue	();
	
	virtual DataType	GetDataType	()	const	{ return DataType::ElapsedTime; }

	virtual void		DoSpinScroll(UINT nSBCode);

	virtual	CString		GetSpecialCaption	();

	virtual	DataObj*	GetMinValue		() { return &m_lMin; }
	virtual	DataObj*	GetMaxValue		() { return &m_lMax; }
	virtual	void		SetMinValue		(const DataObj& value) { m_lMin.Assign(value); }
	virtual	void		SetMaxValue		(const DataObj& value) { m_lMax.Assign(value); }
	virtual void		SetRange		(int nMin, int nMax);

protected:      
	// Overridables
	virtual	BOOL OnInitCtrl	();
		
protected:
	//{{AFX_MSG(CElapsedTimeStatic)
	afx_msg	LRESULT OnFormatStyleChange	(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CBoolStatic 
//=============================================================================
class TB_EXPORT CBoolStatic : public CParsedStatic
{
	DECLARE_DYNCREATE (CBoolStatic)
	
public:
	// Construction
	CBoolStatic();
	CBoolStatic(UINT nBtnIDBmp, DataBool* = NULL);

public:
	// Overridables
	virtual	void		SetValue	(const DataObj& aValue);
	virtual	void		GetValue	(DataObj& aValue);

	virtual DataType	GetDataType	()	const	{ return DataType::Bool; }

public:
    // Static value management
	void	SetValue	(BOOL nValue);
	BOOL	GetValue	();

protected:
	//{{AFX_MSG(CBoolStatic)
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};
    
//=============================================================================
//			Class CEnumStatic
//=============================================================================
class TB_EXPORT CEnumStatic : public CParsedStatic
{
	DECLARE_DYNCREATE (CEnumStatic)

protected:	
	WORD	m_wTag;

public:
	// Construction
	CEnumStatic();
	CEnumStatic(UINT nBtnIDBmp, DataEnum* = NULL);

public:
	// Overridables
	virtual void		Attach		(DataObj*);
	virtual	void		SetValue	(const DataObj& aValue);
	virtual	void		GetValue	(DataObj& aValue);

	virtual DataType	GetDataType	()	const	{ return DataType(DATA_ENUM_TYPE, m_wTag); }

public:
    // Static value management
	void	SetTagValue	(WORD wTag)	{ m_wTag = wTag; }

public:
	const   WORD& GetTagValue() const	{ return m_wTag; }

protected:
	//{{AFX_MSG(CEnumStatic)
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CGuidStatic
//=============================================================================
class TB_EXPORT CGuidStatic : public CParsedStatic
{
	DECLARE_DYNCREATE (CGuidStatic)
	
public:
	// Construction
	CGuidStatic();
	CGuidStatic(UINT nBtnIDBmp, DataGuid* = NULL);

public:
	// Overridables
	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);

	virtual DataType	GetDataType		() const { return DataType::Guid; }

public:
    // Static value management
	void	SetValue	(GUID nValue);
	GUID	GetValue	();                       

protected:      
	//{{AFX_MSG(CStrStatic)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"

#include "ParsEdtOther.h"


