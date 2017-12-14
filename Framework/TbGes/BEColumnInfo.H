#pragma once

#include <TbGenlib/AddressEdit.h>
#include <TbGeneric/TBThemeManager.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class TB_EXPORT ColumnInfoStateCtrlData : public CObject
{
public:
	int			m_nDataInfoIdx;
	BOOL		m_bInvertDefaultState;

	DataObj*	m_pDataObj;

	BOOL		m_bIsCommandBtn;

	CStringArray		m_idbNormal;
	CStringArray		m_idbDisabled;

	BOOL		m_bSetColorInEditableState;
	COLORREF	m_crTextColor;
	COLORREF	m_crBkgColor;

public:
	ColumnInfoStateCtrlData (int nDataInfo, BOOL bInvertStates);

	void SetCtrlState(int s, const CString& idbN, const CString& idbD) { m_idbNormal.SetAtGrow(s, idbN);  m_idbDisabled.SetAtGrow(s, idbD); }

	void SetCommandBtn() { m_bIsCommandBtn = TRUE; }

	void SetColorInEditableState	(COLORREF crBkg, COLORREF crText = AfxGetThemeManager()->GetEnabledControlBkgColor())
	{
		m_bSetColorInEditableState = TRUE;
		m_crBkgColor = crBkg;
		m_crTextColor = crText;
	}

};

//=============================================================================
class TB_EXPORT ColumnInfoComponentsBase : public CObject
{
	DECLARE_DYNAMIC(ColumnInfoComponentsBase)
protected:
	ColumnInfo* m_pColumn;
public:
	ColumnInfoComponentsBase(ColumnInfo* pColumn) : m_pColumn(pColumn) {}
};

//=============================================================================
class TB_EXPORT ColumnInfoComponents : public ColumnInfoComponentsBase
{
	DECLARE_DYNAMIC(ColumnInfoComponents)
public:
	int			m_nCompDataIdx;
	DataObj*	m_pComp;
	BOOL		m_bUseComponentToFormat;

	ColumnInfoComponents(ColumnInfo* pColumn, int nCompDataIdx, DataObj* pComp = NULL);

	DataObj* GetComponent(SqlRecord* pCurrentRow) const;

	void	UseComponentToFormat(BOOL b);
};

//-----------------------------------------------------------------------------
class TB_EXPORT ColumnInfoDynamicDecimalFormatter : public ColumnInfoComponentsBase
{
	DECLARE_DYNAMIC(ColumnInfoDynamicDecimalFormatter)
public:
	int			m_nDecDataIdx;
	DataInt*	m_pNumDec;

	int			m_nMaxDec;

	int			m_nDecXPad;
	int			m_nSepXPad;

	CDblFormatter* m_pFormatter;

	DataInt*   GetNumDec () { return m_pNumDec; }

	ColumnInfoDynamicDecimalFormatter(ColumnInfo* pColumn, int nDecDataIdx, DataInt* m_pNumDec = NULL, int nMaxDec = -1);
	virtual ~ColumnInfoDynamicDecimalFormatter ();
};

//-----------------------------------------------------------------------------
class TB_EXPORT ColumnInfoAddressComponents : public ColumnInfoComponentsBase
{
	DECLARE_DYNAMIC(ColumnInfoAddressComponents)
public:
	DataStr* m_pCity;
	DataStr* m_pCounty;
	DataStr* m_pRegion;
	DataStr* m_pCountry;
	DataStr* m_pZipCode;
	DataStr* m_pLatitude;
	DataStr* m_pLongitude;
	//DataStr* m_pStreetNumber;
	DataStr* m_pAddress;
	DataStr* m_pDistrict;
	DataStr* m_pFederalState;
	DataStr* m_pISOCode;

	int m_nDataIdxCity;
	int m_nDataIdxCounty;
	int m_nDataIdxRegion;
	int m_nDataIdxCountry;
	int m_nDataIdxZipCode;
	int m_nDataIdxLatitude;
	int m_nDataIdxLongitude;
	//int m_nDataIdxStreetNumber;
	int m_nDataIdxAddress;
	int m_nDataIdxDistrict;
	int m_nDataIdxFederalState;
	int m_nDataIdxISOCode;

public:
	ColumnInfoAddressComponents(ColumnInfo* pColumn);
	virtual ~ColumnInfoAddressComponents () {}

	void		AddComponent(CBaseAddressEdit::FieldType eFieldType, int nDataIdx, DataStr* pComp);
	DataStr*	GetComponent(CBaseAddressEdit::FieldType eFieldType, SqlRecord* pRec) const;

	CString		GetAddressText(SqlRecord* pCurrentRow) const;
};

//=============================================================================
class TB_EXPORT ColumnInfo : public CObject, public IOSLObjectManager 
{                                   
	friend class CBodyEdit;
	friend class ColumnsInfoArray;
	friend class CRowFormView;
	friend class CFooterCalculator;
public:
	enum ColumnOrderingType {NONE ,ASC, DESC, DISABLE, ASCFIXED, DESCFIXED};

	CCheckBitmap*	m_pCheckBitmap;
	BOOL			m_bUseOtherColumnCtrl;

protected:
	CParsedCtrl*	m_pParsedCtrl;	// owned 
	DataObj*		m_pBaseDataObj;
	CString			m_strTitle;

	int				m_nDataInfoIdx;

	int				m_nHorzColOffset;
	int				m_nHorzCtrlOffset;
	int				m_nColScrnWidth;
	int				m_nDefaultColScreenWidth;
	int				m_nDefaultCtrlLen;
	int				m_nDefaultCtrlRows;
	int				m_nColCharWidth;
	WORD			m_wColumnStatus;
	UINT			m_nAlignFlags;
	int				m_nColPos;

	DWORD			m_dwCtrlStyle;
	UINT			m_nCtrlIDC;
	int				m_nCtrlLen;
	int				m_nCtrlRows;

	short			m_nTitleExtraRSpace;
	BOOL			m_bShowExtraRSpace;

	ColumnOrderingType  m_Ordering;

	Array			m_arStateDataInfoIdx;

	CString			m_strTitleGroup;

	UINT			m_nTitleIDB;
	CBitmap			m_bmpTitle;

	COLORREF		m_crBkgColor;
	COLORREF		m_crTextColor;

	CString			m_strToolTip;

	BOOL			m_bVisible;

	CBodyEdit*		m_pBody;

	ColumnInfoDynamicDecimalFormatter*	m_pDynamicDecFmt;
	ColumnInfoAddressComponents*		m_pAddressComp;
	ColumnInfoComponents*				m_pComponent;

	//Toggle columns width
	enum	ColumnWidthType { WT_TITLE, WT_DATA, WT_CUSTOM };

	ColumnWidthType	m_eWidthType;
	int				m_nTitleWidth;
	int				m_nDataScrnWidth;
	int				m_nCustomScrnWidth;

	CFooterCalculator*	m_pFooterCalculator;

	BOOL m_bAllowEnlarge;

public:
	ColumnInfo ();

protected:
	ColumnInfo	(CBodyEdit* pBody, LPCTSTR, int, CParsedCtrl*, DWORD, UINT);
	~ColumnInfo	();

	BOOL	ToggleColumnOrdering ();	
	void	SetColumnOrdering	(ColumnOrderingType nOrdering)	{ m_Ordering = nOrdering; }
	
public:
	CTBNamespace&		GetNamespace	() { return GetInfoOSL()->m_Namespace; }
	WORD	GetStatus	()  const			{ return m_wColumnStatus; }
	void	SetStatus	(WORD nStatus);

	CString	GetTitle ()							{ return m_strTitle; }
	void	SetTitle (const CString& strT);

	void	SetCtrlSize	(int nChars, int nRows = 1, int nTextLimit = 0);
	void	SetColWidth	(int nChars);
	void	SetTextAlign(UINT nAlignFlags)				{ m_nAlignFlags = nAlignFlags; }
	
	UINT	GetCtrlID	()	const						{ return m_nCtrlIDC; }
	void	SetCtrlID	(UINT nId)						{ m_nCtrlIDC = nId; }
	DWORD	GetCtrlStyle	()	const					{ return m_dwCtrlStyle; }
	void	SetCtrlStyle	(DWORD dwCtrlStyle)			{ m_dwCtrlStyle = dwCtrlStyle; }
	BOOL	IsReadOnly	(SqlRecord* pRec = NULL/*, CBodyEdit* pBody = NULL, BOOL bOrLocked = TRUE*/) const;
	BOOL	IsStatic	() const;
	BOOL    IsHidden	()  const;

	void	SetHasFooter(BOOL bSet);
	void	UpdateFooter();

	// DA USARE CON ATTENZIONE !!
	CParsedCtrl*	GetParsedCtrl	()	const	{ return m_pParsedCtrl; }
	DataObj*		GetBaseDataObj	()			{ return m_pBaseDataObj; }
	DataObj*		GetFooterDataObj();
	BOOL			HasFooter		()	const	{ return m_pFooterCalculator != NULL; }		
	CWnd*			GetControl		()	const	{ return m_pParsedCtrl ? m_pParsedCtrl->GetCtrlCWnd() : NULL; }
	CBodyEdit*		GetBodyEdit		()	const	{ return m_pBody; }

	BOOL	CreateCtrl			(CBodyEdit* pWndOwner);
	
	void	ResizeColumn	(CFont* pFont, int& xOffset, int nMaxColWidth, BOOL bButtonsToRemoveInRightAlign = FALSE);
	int		ResizeCtrlWidth	(const int& nHeight);
	CSize	GetCtrlSize();
	void	ShowCtrl		(SqlRecord* pRec, int xPos, int yPos);
	void	ShowCtrl		(SqlRecord* pRec, const CRect& rect);
	BOOL	HideCtrl		();
	BOOL	UpdateData		(BOOL bSignalError, BOOL bSendMessage);
	void	ReattachBaseData();
	
	int		GetOriginalScreenWidth	()			const	{ return m_nColScrnWidth; }
	int		GetScreenWidth			()				const	{ return m_bVisible ? m_nColScrnWidth : 0; }
	void	SetScreenWidth			(int nWidth, BOOL bSave); 
	void	SetScreenWidth			(ColumnWidthType wt); 
	void	SetDefaultScreenWidth	(const int& nWidth);

	int		GetDataInfoIdx		()				const	{ return m_nDataInfoIdx;		}
	
	void	SetTitleExtraRSpace	(const short cx)		{ m_nTitleExtraRSpace = cx;		}
	short	GetTitleExtraRSpace	()				const	{ return m_nTitleExtraRSpace;	}
	
	void	SetShowExtraRSpace	(BOOL bSet = TRUE)		{ m_bShowExtraRSpace = bSet;	}
	BOOL	GetShowExtraRSpace	()				const	{ return m_bShowExtraRSpace;	}

	void	GetCtrlSize	(CSize& aSize)			const	{ aSize.cx = m_nCtrlRows; aSize.cy = m_nCtrlLen;}
	int		GetColWidth	()						const	{ return m_nColCharWidth; }
	UINT	GetTextAlign()						const	{ return m_nAlignFlags; }

	void	SetHorzColOffset	(int nHorzColOffset)	{ m_nHorzColOffset = nHorzColOffset; }
	int		GetHorzColOffset	()				const	{ return m_nHorzColOffset;	}

	void	SetHorzCtrlOffset	(int nHorzCtrlOffset)	{ m_nHorzCtrlOffset = nHorzCtrlOffset; }
	int		GetHorzCtrlOffset	()				const	{ return m_nHorzCtrlOffset;	}
	
	void	SetCtrlRows	(int nCtrlRows);
	int		GetCtrlRows	() const						{ return m_nCtrlRows;	}

	ColumnOrderingType		GetColumnOrdering	()	const	{ return m_Ordering;	}

	int		GetColPos				() const { return m_nColPos; }
	void	SetColPos				(const int& nPos);

	ColumnInfoStateCtrlData*	AttachStateData				(int nDataObjIdx, BOOL bInvertDefaultStates = FALSE);
	ColumnInfoStateCtrlData*	AttachStateData				(DataObj* pDataObj, BOOL bInvertDefaultStates = FALSE);

	void						AttachCtrlStateData			(ColumnInfoStateCtrlData* pData, SqlRecord* pRec, CParsedCtrl* pRWControl/* = NULL*/);
	void						AttachCtrlStateData			(SqlRecord* pRec, CParsedCtrl* pRWControl);

	void	SetBaseDataObj			(DataObj* pDataObj);

	void	ChangeParsedControlTo	(CRuntimeClass* pNewClass, DWORD dwOldStyles, DWORD dwStyles, CString sName);
	
public:
	//TB.NET 2.0
	CString	GetToolTip ();
	void	SetToolTip (const CString& strT)	{ m_strToolTip = strT; }

	UINT	GetTitleIDB ()						{ return m_nTitleIDB; }
	void	SetTitleIDB (UINT nTitleIDB);//		{ m_nTitleIDB = nTitleIDB; }

	COLORREF GetBkgColor ()						{ return m_crBkgColor; }
	void	SetBkgColor (COLORREF crBkgColor = (0xFF << 24))	{ m_crBkgColor = crBkgColor; }
	BOOL	UseCustomBkgColor ()				{ return (m_crBkgColor & (0xFF << 24)) == 0;}

	COLORREF GetTextColor ()					{ return m_crTextColor; }
	void	SetTextColor (COLORREF crTextColor = (0xFF << 24))	{ m_crTextColor = crTextColor; }
	BOOL	UseCustomTextColor ()				{ return (m_crTextColor & (0xFF << 24)) == 0;}

	//Toggle columns width
	//void				SetAlternativeWidth	();
	int					GetAlternativeWidth	(ColumnWidthType wt) const;
	ColumnWidthType		GetCurrentWidthType	() const;
	ColumnWidthType		GetNextWidthType	(BOOL bFixed = TRUE) const;
	void				CalcTitleWidth		(CDC* pDC, CFont* pFont);

	//Dynamic Decimal
	BOOL	EnableDynamicDecimalFormatterByRecord	(SqlRecord* pRec, DataObj* pDecField, int nMaxDec = -1);
	BOOL	EnableDynamicDecimalFormatterByColumn	(ColumnInfo* pColDec, int nMaxDec = -1);
	BOOL	EnableDynamicDecimalFormatterByExternal	(DataInt* pNumDec, int nMaxDec = -1);
	BOOL	IsDynamicDecimalFormatter				() const { return m_pDynamicDecFmt != NULL; }
	int		GetDynamicDecimalFormatter				(SqlRecord* pCurrentRow);
	int		SetDynamicDecimalFormatter				(SqlRecord* pCurrentRow);
	CString	OnFormatDynamicDecimal					(DataObj* pCellData, SqlRecord* pCurrentRow, int& nXOffset);
	BOOL	IsSortedByDescription					() const;

	//Bind Address components
	BOOL	AddAddressComponentByRecord		(CBaseAddressEdit::FieldType ft, SqlRecord* pRec, DataStr* pComp);
	BOOL	AddAddressComponentByColumn		(CBaseAddressEdit::FieldType ft, ColumnInfo* pCol);
	BOOL	AddAddressComponentByExternal	(CBaseAddressEdit::FieldType ft, DataStr* pComp);
	BOOL	IsAddress						() const { return m_pAddressComp != NULL; }

	//Bind single component components
	ColumnInfoComponents*	AddComponentByRecord	(SqlRecord* pRec, DataObj* pComp);
	ColumnInfoComponents*	AddComponentByColumn	(ColumnInfo* pCol);
	ColumnInfoComponents*	AddComponentByExternal	(DataObj* pComp);

	BOOL	HasHyperLink		() const;
	void	BrowseHyperLink		(SqlRecord* pRec);
	void	RefreshHyperLink	(SqlRecord* pRec);

	BOOL	GetAllowEnlarge() const	 { return m_bAllowEnlarge; }
	void	SetAllowEnlarge(BOOL bEnlarge = TRUE)	{ m_bAllowEnlarge = bEnlarge; }

	void	SetVisible	  (BOOL bVisible, BOOL bApply = TRUE);

	//TODO
	CString	GetTitleGroup ()					{ return m_strTitleGroup; }
	void	SetTitleGroup (const CString& strT)	{ m_strTitleGroup = strT; }

protected:	
	//Dynamic Decimal
	BOOL	EnableDynamicDecimalFormatter	(int nDecDataInfoIdx, DataInt* pNumDec, int nMaxDec = -1);
	BOOL	PrepareDynamicDecimalFormatter	();

	//Bind Address components
	BOOL	AddAddressComponent		(CBaseAddressEdit::FieldType ft, int nDataInfoIdx, DataStr* pComp);
	void	SetAddressComponents	(SqlRecord* pCurrentRow);

	//Bind single components
	ColumnInfoComponents*	AddComponent (int nDataInfoIdx, DataObj* pComp);
	void					SetComponent (SqlRecord* pCurrentRow);

	BOOL	HasSingleComponent			() const { return m_pComponent != NULL; }
	BOOL	HasSingleComponentToFormat	() const { return HasSingleComponent() && m_pComponent->m_bUseComponentToFormat; }

private:
	void	InitializeLengthsFromBaseDataObj	();

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

//=============================================================================
class TB_EXPORT ColumnsInfoArray : public Array
{
public:
	ColumnsInfoArray (BOOL bSpecial) 
		{ 
			if(bSpecial) 
				{m_bOwnsElements = FALSE; m_bRemoveAllOnExit = FALSE;} 
		}
	ColumnsInfoArray () {};

	// Accessing elements
	ColumnInfo*		GetAt(int nIndex) const	{ return nIndex >= 0 && nIndex <= CObArray::GetUpperBound() ?  (ColumnInfo*) CObArray::GetAt(nIndex) : NULL; }
	ColumnInfo*&	ElementAt(int nIndex)	{ return (ColumnInfo*&) CObArray::ElementAt(nIndex); }
	
	// overloaded operator helpers
	ColumnInfo*		operator[](int nIndex) const	{ return GetAt(nIndex); }
	ColumnInfo*&	operator[](int nIndex)			{ return ElementAt(nIndex); }

	int		GetColumnIdx(ColumnInfo* pInfo);

	virtual BOOL	LessThen (CObject*, CObject*) const ;
	virtual int		Compare(CObject* po1, CObject* po2) const;

	virtual void    Add		 (ColumnInfo* pInfo);
	virtual void    Remove	 (ColumnInfo* pInfo);
	virtual void    MoveTo	 (ColumnInfo* pInfo, const int& nIdx);
};

//=============================================================================
class TB_EXPORT CBodyBitmapButton : public CBitmapButton, public IOSLObjectManager 
{
	DECLARE_DYNAMIC(CBodyBitmapButton)

public:
	UINT	m_nButtonIDC;

	UINT	m_nButtonIDBU;
	UINT	m_nButtonIDBD;
	UINT	m_nButtonIDBDI;

	CString	m_sButtonIDBUName;
	CString	m_sButtonIDBDName;
	CString	m_sButtonIDBDIName;

	BOOL	m_bButtonEnabled;
	BOOL	m_bDisableOnReadOnly;
	CString m_strToolTip;

public:
	CBodyBitmapButton(UINT nIDC, UINT nIDBU, UINT nIDBD, UINT nIDBDI, LPCTSTR szToolTip);
	CBodyBitmapButton(UINT nIDC, const CString& sBitmapResourceUp, const CString& sBitmapResourceDown, const CString& sBitmapResourceDisable, LPCTSTR szToolTip);
	CTBNamespace GetNamespace	 () {return GetInfoOSL()->m_Namespace;}

protected:
	BOOL LoadWalkBitmap (CBitmap &bitmapResource, UINT nBitmapResource);
	BOOL LoadWalkBitmap (CBitmap &bitmapResource, const CString& sBitmapResource);

public:
	BOOL	Create(LPCTSTR psz, DWORD style, const RECT& rect, CWnd* pParent);
	BOOL	LoadBitmaps	(
							UINT nBitmapResource,
							UINT nBitmapResourceSel,
							UINT nBitmapFocus		= 0,
							UINT nBitmapDisabled	= 0
						);
	BOOL	LoadBitmaps	(
							const CString& sBitmapResource,
							const CString& sBitmapResourceSel,
							const CString& sBitmapFocus		= _T(""),
							const CString& sBitmapDisabled	= _T("")
						);

	void	SetEnableState(BOOL bEnable, BOOL bDisableOnReadOnly = FALSE);

	void	EnableButton(BOOL bBodyState);
protected:
	virtual	void 	DrawItem	(LPDRAWITEMSTRUCT lpDIS);
	
	//{{AFX_MSG(CBodyBitmapButton)
	afx_msg	void 	OnLButtonDown	(UINT nFlags, CPoint point);
	afx_msg	void 	OnLButtonUp		(UINT nFlags, CPoint point);
	afx_msg	void 	OnMouseMove		(UINT nFlags, CPoint point);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

//=============================================================================
// Struttura per contenere le informazioni DC
//	Nuova intestazione per le informazioni di disegno da passare alle 
//	derivate per customizzare l'aspetto grafico
//=============================================================================
class TB_EXPORT CDCDrawInfo
{
protected:
	// Device Context
	CDC*		m_pDC;

	// Informazioni sulla riga
	int			m_nRowIdx;
	BOOL		m_bActiveRow;
	int			m_nYStart;

public:
	// Colori del testo e dello sfondo...
	COLORREF	m_crText;
	COLORREF	m_crOldText;
	COLORREF	m_crBkgrd;
	COLORREF	m_crOldBkgrd;

	CFont*		m_pFont;
	CFont*		m_pOldFont;

public:
	// Costruttore 
	CDCDrawInfo(CDC* pDC, int nRowIdx, BOOL bActiveRow, int nYStart)
	{
		m_pDC				= pDC;
		m_nRowIdx			= nRowIdx;
		m_bActiveRow		= bActiveRow;
		m_nYStart			= nYStart;
		
		m_crText			= AfxGetThemeManager()->GetEnabledControlForeColor();
		m_crOldText			= AfxGetThemeManager()->GetEnabledControlForeColor();
		m_crBkgrd			= AfxGetThemeManager()->GetEnabledControlBkgColor();
		m_crOldBkgrd		= AfxGetThemeManager()->GetEnabledControlBkgColor();

		m_pOldFont			= NULL;
		m_pFont				= NULL;
	}

	CDC*	GetDC()				const	{ return m_pDC;}
	int		GetRowIdx()			const	{ return m_nRowIdx; }
	BOOL	GetIsActiveRow()	const	{ return m_bActiveRow; }
	int		GetYStart()			const	{ return m_nYStart; }
};

//=============================================================================
class TB_EXPORT CBodyEditCellSelected
{
public:
	CBodyEditCellSelected() 
		: 
		m_pRec(NULL), 
		m_nRowDbt (0),
		m_pCol(NULL),
		m_nColumnIDC(0), 
		m_pCellData(NULL),
		m_nColumnIdx(0)
	{}

	SqlRecord*		m_pRec;
	int				m_nRowDbt;

	ColumnInfo*		m_pCol;
	int				m_nColumnIDC;
	int				m_nColumnIdx;
	DataObj*		m_pCellData;
};

//=============================================================================
class TB_EXPORT CBodyEditRowSelected: public CBodyEditCellSelected
{
public:
	CBodyEditRowSelected() 
		: 
		CBodyEditCellSelected	(),
		m_crTextColor			(AfxGetThemeManager()->GetBERowSelectedForeColor()), 
		m_crBkgColor			(AfxGetThemeManager()->GetBERowSelectedBkgColor()),
		m_nIconIDB				(0),
		m_bFooter				(FALSE),
		m_pFont					(NULL),
		m_dBkgPercFill			(1.0),
		m_crBkgColorFill		(AfxGetThemeManager()->GetBERowSelectedFillColor())
	{}

	COLORREF		m_crTextColor;
	COLORREF		m_crBkgColor;
	UINT			m_nIconIDB;
	BOOL			m_bFooter;
	CFont*			m_pFont;

	double			m_dBkgPercFill;
	COLORREF		m_crBkgColorFill;
};

//=============================================================================
class TB_EXPORT CBETooltipProperties//: public CTooltipProperties, public CBodyEditCellSelected
{
public:
	UINT		m_nControlID;
	CString		m_strText;
	CString		m_strTitle;
	int			m_nWidth; 
	COLORREF	m_clrText; 
	COLORREF	m_clrBackgnd;
	int			m_nIcon;

	SqlRecord*		m_pRec;
	int				m_nRowDbt;

	ColumnInfo*		m_pCol;
	int				m_nColumnIdx;
	DataObj*		m_pCellData;
	BOOL			m_bShowFloatingTip;
	BOOL			m_bShowStatusBarTip;

	TOOLINFO	m_ti;

	CBETooltipProperties()
		:
		m_nControlID(0),
		m_nWidth(0), 
		m_clrText(AfxGetThemeManager()->GetBETooltipForeColor()), 
		m_clrBackgnd(AfxGetThemeManager()->GetBETooltipForeColor()),
		m_nIcon(0),
		m_pRec(NULL), 
		m_nRowDbt (0),
		m_pCol(NULL),
		m_pCellData(NULL),
		m_nColumnIdx(0)
	{
		
	}

};

//==============================================================================
#include "endh.dex"
