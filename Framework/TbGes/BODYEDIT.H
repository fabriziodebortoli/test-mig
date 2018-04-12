#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>

#include <TbGeneric\DropBitmaps.h>
#include <TbGeneric\BCMenu.h>
#include <TbGeneric\TBScrollBar.h>

#include <TbGenlib\parsctrl.h>

#include "extdoc.h"
#include "dbt.h"
#include "hotlink.h"
#include "BEColumnInfo.h"

//includere alla fine degli include del .H
#include "beginh.dex"

#define	MAX_BODY_RECORDS		500000
#define DEFAULT_TITLE_ROWS		1.0
//=============================================================================
class DBTSlaveBuffered;
class DataObj;
class SqlRecord;
class CRowFormView;
class CAbstractFormDoc;
class HotKeyLink;
class CSpin;
class CCheckBitmap;
class CTBEDataCoDec;
class CTBExtBEDropTarget;
class CTBExtBEOleDropSource;
class CTBMenu;
class BodyEditColumn;
class CTBEDefaultCoDec;
class TFXDataTip;
class BodyEditInfo;
class CSlaveViewContainer;

//=============================================================================
#define DEFAULT_COL_TITLE				_T("")
#define FLAG_END_COL_NOT_FULL_VISIBLE	0x8000

// Body Edit Control Notification Codes
#define ON_BEN_ROW_CHANGED(id, memberFxn) \
	ON_CONTROL(BEN_ROW_CHANGED, id, memberFxn)

#define	SET_BODY_FOCUS_ONLY	(1000)
#define	SET_BODY_CURR_ROW	(1001)

/****************************************************************************
			DISPLAY Driver extensions
			****************************************************************************/
#ifndef CAPS1 
#define CAPS1   94					/* use with GetDeviceCaps() */
#endif /* ifndef CAPS1 */

#ifndef C1_TRANSPARENT 
#define C1_TRANSPARENT	0x0001
#endif /* ifndef C1_TRANSPARENT */

#ifndef NEWTRANSPARENT
#define NEWTRANSPARENT  3           /* use with SetBkMode() */
#endif  /* ifndef NEWTRANSPARENT */

//#define BMP_ORDERING_OFFSET 4

#define PNG_SIZE			((short) (ScalePix(14)))

#define	LOCK_EXTRARSPACE	(PNG_SIZE+2)
#define	SORT_EXTRARSPACE	(PNG_SIZE+2)

/****************************************************************************/

//==============================================================================

class TB_EXPORT CWndBodyElementDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CWndBodyElementDescription);
protected:
	CWndBodyElementDescription(void);
public:
	bool m_bActiveCell = false;
	bool m_bActiveRow = false;
	int  m_nTextAlign = TA_LEFT;
	bool m_bIsCheckBox = false;
	bool m_bCheckBoxValue = false;
	bool m_bIsHyperLink = false;

	COLORREF m_crBkgColor = EMPTY_COLOR;
	COLORREF m_crTextColor = EMPTY_COLOR;
	int m_nRow = -1;
	int m_nCol = -1;

	CWndBodyElementDescription(CWndObjDescription* pParent);
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
protected:
	virtual void Assign(CWndObjDescription* pDesc);
};
//==============================================================================
class TB_EXPORT CWndBodyColumnDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CWndBodyColumnDescription)
protected:
	CWndBodyColumnDescription(){}
	~CWndBodyColumnDescription() {
		delete m_pItemSourceDescri;

		for (int i = 0; i < m_arValidators.GetCount(); i++)
		{
			SAFE_DELETE(m_arValidators.GetAt(i));
		}
		m_arValidators.RemoveAll();
	}
public:
	bool m_bSort					= false;
	bool m_bHasFooterDescr			= false;
	bool m_bStatusHidden			= false;
	bool m_bStatusGrayed			= false;
	bool m_bStatusNoChange_Grayed	= false;
	bool m_bStatusNoChange_Hidden	= false;
	bool m_bStatusLocked			= false;
	bool m_bStatusSortedAsc			= false;
	bool m_bStatusSortedDes			= false;
	bool m_bAllowEnlarge			= false;
	CItemSourceDescription* m_pItemSourceDescri = NULL;
	CControlBehaviourDescription* m_ControlBehaviourDescri = NULL;
	CArray <CValidatorDescription*, CValidatorDescription*> m_arValidators;
	CDataAdapterDescription* m_pDataAdapterDescri = NULL;

	CString		m_strTooltip;

	CWndBodyColumnDescription(CWndObjDescription* pParent) : CWndObjDescription(pParent){}
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
protected:
	virtual void Assign(CWndObjDescription* pDesc);
	virtual void EvaluateExpressions(CJsonContextObj * JsonContext, bool deep = true);
};

//Descrizione usata per la cella del body edit ad albero che fa le veci di un nodo, quindi ha il tasto per
//espandere e fare collapse
//==============================================================================
class TB_EXPORT CWndBodyTreeElementDescription : public CWndBodyElementDescription
{
	DECLARE_DYNCREATE(CWndBodyTreeElementDescription);
private:
	CWndBodyTreeElementDescription(void);
public:
	BOOL	m_bHasChild		= FALSE;
	BOOL	m_bIsExpanded	= FALSE;
	int		m_nLevel		= -1;

	CWndBodyTreeElementDescription(CWndObjDescription* pParent);
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);

protected:
	virtual void Assign(CWndObjDescription* pDesc);
};



//-----------------------------------------------------------------------------
class TB_EXPORT CWndBodyDescription : public CWndObjDescription
{
	DECLARE_DYNCREATE(CWndBodyDescription);
	//booleani che dicono se ci sono pagine precedenti o succesive da visualizzare nel caso
	//di body edit  paginato
	bool m_bPrevRows = false;
	bool m_bNextRows = false;
	CString m_strRowView;
	int m_nLinesPerRow = 1;
	bool m_bInsertRowEnabled = true;
	bool m_bAddRowEnabled = true;
	bool m_bDeleteRowEnabled = true;

	Bool3 m_bAllowOrdering				= B_UNDEFINED;
	Bool3 m_bAllowOrderingOnBrowse		= B_UNDEFINED;
	Bool3 m_bAllowOrderingOnEdit		= B_UNDEFINED;
	Bool3 m_bAllowCallDialog			= B_UNDEFINED;
	Bool3 m_bAllowColumnLock			= B_UNDEFINED;
	Bool3 m_bAllowColumnLockInteractive = B_UNDEFINED;
	Bool3 m_bAllowCopy					= B_UNDEFINED;
	Bool3 m_bAllowCustomize				= B_UNDEFINED;
	Bool3 m_bAllowDelete				= B_UNDEFINED;
	Bool3 m_bAllowDrag					= B_UNDEFINED;
	Bool3 m_bAllowDrop					= B_UNDEFINED;
	Bool3 m_bAllowPaste					= B_UNDEFINED;
	Bool3 m_bAllowDragReadOnlyDoc		= B_UNDEFINED;
	Bool3 m_bAllowInsert				= B_UNDEFINED;
	Bool3 m_bAllowMultipleSel			= B_UNDEFINED;
	Bool3 m_bAllowRemoveColumnInteractive = B_UNDEFINED;
	Bool3 m_bAllowSearch				= B_UNDEFINED;
	Bool3 m_bChangeColor				= B_UNDEFINED;
	Bool3 m_bEnlargeAllStringColumns	= B_UNDEFINED;
	Bool3 m_bEnlargeCustom				= B_UNDEFINED;
	Bool3 m_bEnlargeLastColumn			= B_UNDEFINED;
	Bool3 m_bEnlargeLastStringColumn	= B_UNDEFINED;
	Bool3 m_bShowBorders				= B_UNDEFINED;
	Bool3 m_bShowColumnHeaders			= B_UNDEFINED;
	Bool3 m_bShowFooterToolbar			= B_UNDEFINED;
	Bool3 m_bShowHeaderToolbar			= B_UNDEFINED;
	Bool3 m_bShowHorizLines				= B_UNDEFINED;
	Bool3 m_bShowHorizScrollbar			= B_UNDEFINED;
	Bool3 m_bShowVertLines				= B_UNDEFINED;
	Bool3 m_bShowVertScrollbar			= B_UNDEFINED;
	Bool3 m_bShowDataTip				= B_TRUE;
	Bool3 m_bShowStatusBar				= B_TRUE;
	Bool3 m_bShowAlternateColor			= B_UNDEFINED;
	int m_nMaxRecords					= MAX_BODY_RECORDS;
	double m_dTitleRows					= DEFAULT_TITLE_ROWS;

	CJsonContext* m_pRowViewContext		= NULL;
private:
	CWndBodyDescription(void);
public:
	~CWndBodyDescription();
	CWndBodyDescription(CWndObjDescription* pParent);
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
	CWndObjDescription* GetRowViewDescription(CJsonContext* pOwnerContext);
protected:
	virtual void Assign(CWndObjDescription* pDesc);
	virtual void UpdateFocusStatus(CWnd *pWnd);
};

//==============================================================================
#define BE_AUXTOOLBAR_DEFAULT_HEIGHT			35
#define BE_AUXTOOLBAR_BUTTON_DEFAULT_WIDTH		33
#define BE_AUXTOOLBAR_BUTTON_DEFAULT_HEIGHT		33

//-----------------------------------------------------------------------------
class TB_EXPORT CBEButton : public CBCGPMenuButton, public IMenuIcon, public IDisposingSourceImpl, public IOSLObjectManager
{
	friend class CBEToolbar;

	DECLARE_DYNAMIC(CBEButton)
public:
	CBEToolbar*	m_pToolBar;
	UINT		m_nID;
	DWORD		m_dwStyle;

	UINT			m_nIDB;
	CTBNamespace	m_nsImage;
	CTBNamespace	m_nsCheckImage;

	int			m_nWidth;

	BOOL		m_bHiddenText;

	CString		m_sName; //Object name del namespace

	BOOL		m_bRightAlign;
	BOOL		m_bDisableOnReadOnly;

	CArray<CInfoOSLButton*>	m_CInfoOSLs;
protected:
	CString		m_strTooltip;
	CString		m_strText;	//testo mostrato a lato del pulsante

	BOOL		m_bEnabled;
	BOOL		m_bVisible;

	CMenu			m_menu;
	CMap<UINT, UINT, CBitmap*, CBitmap*> m_mapMenuImages;

	BOOL		m_bLeftButtonDown;
	int			m_nHeight;

public:

	CBEButton
		(
		CBEToolbar*			pToolBar,
		const CString&		sName,
		int					nWidth,
		UINT				nID,
		UINT				nIDB,
		DWORD				dwBtnStyle,
		const CString&		strTooltip,
		const CString&		strText
		);

	CBEButton
		(
		CBEToolbar*			pToolBar,
		const CString&		sName,
		int					nWidth,
		UINT				nID,
		const CString&		nsImage,
		DWORD				dwBtnStyle,
		const CString&		strTooltip,
		const CString&		strText
		);

	virtual ~CBEButton();

	UINT	   GetBEButtonID() { return m_nID; }

	CBodyEdit* GetBody();

	CMenu* GetMenu() { return &m_menu; }

	virtual int GetWidth(BOOL bCalcOnly);
	virtual int GetHeight();
	virtual void SetHeight(int nHeight);

	virtual BOOL Create(CWnd* pParent);

	virtual HWND GetHWnd() { return m_hWnd; }

	virtual void SetPos(int left, int top, int w, int h);

	virtual void Invalidate() { if (m_hWnd) __super::Invalidate(); }
	virtual void RedrawWindow();

	virtual void ShowButton(BOOL);
	virtual void EnableButton(BOOL);

	virtual BOOL ChangeImage(UINT nIDB, const CString& nsImage = L"");
	virtual BOOL ChangeImage(const CString& nsImage);
	virtual BOOL SetCheckImage(UINT nIDB, const CString& nsImage = L"");
	virtual BOOL SetCheckImage(const CString& nsImage);

	virtual void SetText(const CString& sText);
	virtual void SetTooltip(const CString& sTooltip);

	const CString& GetText() const { return m_strText; }
	const CString& GetTooltip() const { return m_strTooltip; }

	void AddMenuItem(CString aName, CString sDescription, UINT nID, BOOL bEnable = TRUE, BOOL bCheck = FALSE, const CString& sImage = L"");
	void CheckMenuItem(UINT nID, BOOL bCheck);
	void EnableMenuItem(UINT nID, BOOL bEnable);
	void AddMenuSeparator();

	//UINT GetMenuItemIDClicked(int nPos);

	virtual void OnDraw(CDC* pDC, const CRect& rect, UINT uiState);
	virtual BOOL OnDrawMenuImage(CDC* pDC, const CBCGPToolbarMenuButton* pMenuButton, const CRect& rectImage);

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

private:
	virtual void EnableButtonState(BOOL);

protected:
	afx_msg void	OnSetFocus(CWnd* pOldWnd);
	afx_msg	void 	OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg	void 	OnLButtonUp(UINT nFlags, CPoint point);

	DECLARE_MESSAGE_MAP()
};

//-----------------------------------------------------------------------------
class TB_EXPORT CBEBtnCtrl : public CBEButton
{
	friend class CBEToolbar;

	DECLARE_DYNAMIC(CBEBtnCtrl)

private:
	BOOL			m_bAutoSize;
	CBEBtnCtrl*		m_pLabel;

public:
	CWnd*			m_pControl;

	CBEBtnCtrl
		(
		CBEToolbar*			pToolBar,
		CRuntimeClass*		rtcEdit,
		const CString&		sName,
		int					nWidth,
		UINT				nID,
		DWORD				dwBtnStyle,
		const CString&		strTooltip,
		const CString&		strText
		);

	virtual ~CBEBtnCtrl();

	virtual int GetWidth(BOOL bCalcOnly);

	virtual BOOL Create(CWnd* pParent);

	virtual HWND GetHWnd() { return m_pControl ? m_pControl->m_hWnd : NULL; }

	virtual void SetPos(int left, int top, int w, int h);

	virtual void Invalidate();
	virtual void RedrawWindow();

	virtual void ShowButton(BOOL);
	virtual void EnableButton(BOOL);

	virtual void SetAutoSize(bool bAutoSize = TRUE)	{ m_bAutoSize = bAutoSize; }
	virtual BOOL IsAutoSize()						{ return m_bAutoSize; }

	void AttachHotKeyLink(HotKeyLinkObj* pHotKeyLink, UINT nBtnID = BTN_DEFAULT);
private:
	virtual void EnableButtonState(BOOL);
};

//-----------------------------------------------------------------------------
class CBodyEdit;

class TB_EXPORT CBEToolbar : public Array, public IOSLObjectManager
{
	friend class CBodyEdit;

private:
	int				m_nHeight;
	CRect			m_rectToolBar;

public:
	DECLARE_DYNAMIC(CBEToolbar)

	CBodyEdit*		m_pBody;
	
	BOOL			m_bDefaultRightAlign;

	CBEToolbar(CBodyEdit* pBody);
	~CBEToolbar();

	CBEButton* 	GetAt(int nIndex)const	{ return (CBEButton*)Array::GetAt(nIndex); }

	void				SetHeight(int h);
	int					GetHeight()			{return m_nHeight;}

	CRect				GetToolBarRect()	{ return m_rectToolBar;}
	void				SetToolBarRect(CRect rect) { m_rectToolBar  = rect;}


	BOOL				Create(const CString& sName);
	void				SetPos();
	void				Invalidate();
	void				Redraw();
	void				OnUpdateControls(BOOL bParentIsVisible);

	void				EnableButtons(BOOL bBEStatus);
	void				EnableButtons();

	void				Dispose();

	CBEButton*			AddButton
		(
		const CString& sName,
		UINT		nID,
		UINT		nIDB,
		CString		strTooltip,
		CString		strText = L"",
		int			nIndex = -1
		);

	CBEButton*			AddButton
		(
		const CString& sName,
		UINT		nID,
		CString		strNsImage,
		CString		strTooltip,
		CString		strText = L"",
		int			nIndex = -1
		);

	CBEButton*			AddMenuButton
		(
		const CString& sName,
		UINT		nID,
		CString		strNsImage,
		CString		strTooltip,
		CString		strText = L"",
		int			nIndex = -1
		);

	CBEBtnCtrl*			AddControl
		(
		CRuntimeClass*	pRtc,
		const CString&	sName,
		UINT			nID,
		CString			strTooltip,
		CString			strText,
		int				width,
		DWORD			dwBtnStyle = (WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS),
		int				nIndex = -1,
		DataObj*		pDataObj = NULL
		);

	CBEBtnCtrl*			AddStaticControl
		(
			const CString&	sName,
			CString			strTooltip,
			CString			strText,
			int				width,
			int				nIndex = -1,
			DataObj*		pDataObj = NULL
			);

	CBEButton*			FindButton(UINT ID);
	int					FindButtonIndex(UINT ID);

	void				EnableButton(CBEButton*, BOOL bEnable = TRUE);
	void				EnableButton(UINT ID, BOOL bEnable = TRUE);

	void				ShowButton(UINT ID, BOOL bShow = TRUE);
	void				HideButton(UINT ID, BOOL bHide = TRUE) { ShowButton(ID, !bHide); }

private:
	CArray<CImageAssociation*>		m_Images;

	BOOL				isAutoSizeButton(CBEButton* pButton);
	CImageAssociation*	GetImageAssociation(UINT nIDC);
	BOOL				ProcessSysKeyMessage(UINT nKey);
};

//=============================================================================
#define		BE_STYLE_DEFAULT						__int64(0)

#define		BE_STYLE_ALLOW_MULTIPLE_SEL				__int64(1)
#define		BE_STYLE_ALLOW_DRAG						(__int64(1) << 2)
#define		BE_STYLE_ALLOW_DROP						(__int64(1) << 3)
#define		BE_STYLE_ALLOW_COPY						(__int64(1) << 4)
#define		BE_STYLE_ALLOW_PASTE					(__int64(1) << 5)
#define		BE_STYLE_ALLOW_DRAG_READONLY_DOC		(__int64(1) << 6)

#define		BE_STYLE_MULTIPLE_SEL_ACCELERATOR_S		(__int64(1) << 7)

#define		BE_STYLE_ALLOW_INSERT					(__int64(1) << 8)
#define		BE_STYLE_ALLOW_DELETE					(__int64(1) << 9)
#define		BE_STYLE_ALLOW_CALLDIALOG				(__int64(1) << 10)

#define		BE_STYLE_ALLOW_SORT						(__int64(1) << 11)		
#define		BE_STYLE_APPLY_SORT_ON_BROWSE			(__int64(1) << 12)		
#define		BE_STYLE_ALLOW_SORT_ON_EDIT				(__int64(1) << 13)	

#define		BE_STYLE_ALLOW_SEARCH					(__int64(1) << 14)	
#define		BE_STYLE_ALLOW_FILTER					(__int64(1) << 15)

#define		BE_STYLE_ALLOW_COLUMN_LOCK				(__int64(1) << 16)
#define		BE_STYLE_ALLOW_COLUMN_LOCK_INTERACTIVE	(__int64(1) << 17)

#define		BE_STYLE_ALLOW_REMOVE_COLUMN_INTERACTIVE (__int64(1) << 18)

#define		BE_STYLE_ALLOW_CUSTOMIZE				(__int64(1) << 19)

#define		BE_STYLE_SHOW_VERT_SCROLLBAR			(__int64(1) << 20)
#define		BE_STYLE_SHOW_HORIZ_SCROLLBAR			(__int64(1) << 21)

#define		BE_STYLE_SHOW_SPIN_ROW_HEIGHT			(__int64(1) << 22)
#define		BE_STYLE_SHOW_ROW_HOME_END				(__int64(1) << 23)
#define		BE_STYLE_SHOW_ROWINDICATOR				(__int64(1) << 24)

#define		BE_STYLE_SHOW_ALTERNATE_COLOR			(__int64(1) << 25)

#define		BE_STYLE_SHOW_HEADER_TOOLBAR			(__int64(1) << 26)
#define		BE_STYLE_SHOW_FOOTER_TOOLBAR			(__int64(1) << 27)

#define		BE_STYLE_SHOW_COLUMN_HEADERS			(__int64(1) << 28)

#define		BE_STYLE_SHOW_VERT_LINES				(__int64(1) << 29)
#define		BE_STYLE_SHOW_HORIZ_LINES				(__int64(1) << 30)

#define		BE_STYLE_CHANGE_COLOR					(__int64(1) << 31)

#define		BE_STYLE_ALIGN_TITLES					(__int64(1) << 32)

#define		BE_STYLE_ENLARGE_LASTCOLUMN				(__int64(1) << 33)
#define		BE_STYLE_ENLARGE_LASTSTRINGCOLUMN		(__int64(1) << 34)
#define		BE_STYLE_ENLARGE_ALLSTRINGCOLUMNS		(__int64(1) << 35)
#define		BE_STYLE_ENLARGE_CUSTOM					(__int64(1) << 36)

#define		BE_STYLE_UPPERCASE_COLUMN_TITLES		(__int64(1) << 37)

#define		BE_STYLE_SHOW_BORDERS					(__int64(1) << 38)
#define		BE_STYLE_SHOW_DATATIP					(__int64(1) << 39)
#define		BE_STYLE_SHOW_STATUSBAR					(__int64(1) << 40)

#define		BE_STYLE_SHOW_CONTEXTMENU				(__int64(1) << 41)

//-----------------------------------------------------------------------------
class TB_EXPORT CBodyEdit : public CGridControl
{
	friend class CBodyBitmapButton;
	friend class ColumnInfo;
	friend class CRowFormView;
	friend class DBTSlaveBuffered;
	friend class CDBTTreeEdit;
	friend class CFooterCalculator;
	friend class CBEToolbar;
	friend class CBEButton;
public:
	DECLARE_DYNCREATE(CBodyEdit)

	enum	CursorPosArea	{ IN_BODY, OUT_OF_BODY, IN_RESIZE_GRIP, IN_COLUMN_TITLE, IN_NEW_READ_ONLY_ROW };
	enum	MultiSelMode	{ NONE, MULTIPLE_SEL };
	enum	DragSelType		{ DST_TOP, DST_CENTER, DST_BOTTOM };
	enum	DragButton		{ DB_NONE, DB_LEFT, DB_RIGHT };
	enum	InvertMode		{ INVERT, NOT_INVERT };
	enum	DataTipStyle
	{
		BE_DTSTYLE_NONE = 0,
		BE_DTSTYLE_CUSTOM = 0x01,
		BE_DTSTYLE_HEADER = 0x02,
		BE_DTSTYLE_BROWSE = 0x04,
		BE_DTSTYLE_BATCH = 0x08,
		BE_DTSTYLE_EDIT = 0x10,
		BE_DTSTYLE_HEADER_ON_CELL = 0x20,

		BE_DTSTYLE_READONLY =
		(BE_DTSTYLE_CUSTOM | BE_DTSTYLE_HEADER | BE_DTSTYLE_BROWSE),	//7
		BE_DTSTYLE_ALL =
		(BE_DTSTYLE_CUSTOM | BE_DTSTYLE_HEADER | BE_DTSTYLE_BROWSE |
		BE_DTSTYLE_BATCH | BE_DTSTYLE_EDIT | BE_DTSTYLE_HEADER_ON_CELL)	//63
	};

	// auxtoolbar
	CBEToolbar			m_HeaderToolBar;
	CBEToolbar			m_FooterToolBar;

	struct s1 {
		int		nRow;
		UINT	nColIDC;
		CGuid	gSession;
		BOOL	bIsDrag;
	} m_CopyOrDragSourceCell;

	struct s2 {
		int		nRow;
		UINT	nColIDC;
		CGuid	gSession;
		BOOL	bIsDrop;
	} m_PasteOrDropTargetCell;

	//Array				m_UserButtons;

protected:
	CImageList*			m_pImageList		= NULL;
	CParsedStateImage*	m_pImages			= NULL;
	int					m_nBmpMaxWidth		= 0;

	//Multiselezione - Drag&Drop

	DBTSlaveBuffered*	m_pDBT				= NULL;
	UINT				m_nID				= 0;
	CString				m_strBodyTitle;
	ColumnsInfoArray	m_AllColumnsInfo;
	ColumnsInfoArray	m_ColumnsInfo		= TRUE;
	CRowFormView*		m_pRowFormView		= NULL;
	CRuntimeClass*		m_pFormViewClass	= NULL;
	CString				m_strRowFormViewTitle;
	CString				m_strRowFormViewName;
	int					m_nMaxBodyRecords	= 0;
	MultiSelMode		m_MultiSelection	= NONE;
	// body status
	int				m_bRejectUpdate			= FALSE;
	int				m_nNotValidCellRec		= -1;
	int				m_nNotValidCellCol		= -1;
	BOOL			m_bBodyModified			= FALSE;
	BOOL			m_bBodyFocused			= FALSE;
	BOOL			m_bBodyActive			= FALSE;
	BOOL			m_bSetFocusOnly			= FALSE;
	WORD			m_DataTipStyle			= 0;
	__int64			m_dwExStyles			= 0;

	CBaseDocument::FormMode		m_PrevFormMode = CBaseDocument::FormMode::NONE;			// previosus mode for document

	bool	m_bModified = FALSE;// True if the body edit content has been changed.

	BOOL	m_bIconized = FALSE;

	// body status info: all values are zero based
	int		m_nStartRecordIdx				= 0;		// current first body record
	int		m_nStartColumnIdx				= 0;		// current first body column

	int		m_nCurrRecordIdx				= -1;		// current active body record
	int		m_nRecordIdxToFocus				= -1;		// current active body record that will be focused when the bodyedit becomes active
	int		m_nCurrColumnIdx				= -1;		// current active column

	int		m_nOldStartRecordIdx			= -1;		// old first body record before scrolling
	int		m_nOldStartColumnIdx			= -1;		// old first body column before scrolling

	BOOL	m_bInsertRowEnabled				= TRUE;
	BOOL	m_bAddRowEnabled				= TRUE;
	BOOL	m_bDeleteRowEnabled				= TRUE;
	BOOL	m_bFormViewCallEnabled			= TRUE;

	// visual information
	CRect		m_rectTitles				= CRect(0, 0, 0, 0);;
	CRect		m_rectBody					= CRect(0, 0, 0, 0);;
	CRect		m_rectHScrollBar			= CRect(0,0,0,0);
	int			m_nRowsHeight				= 0;
	int			m_nLinesPerRow				= 1;		// number of lines in every row (normally 1 - can be set with SetMultipleLinesPerRow)
	CWordArray	m_LastColsIdx;							// array of last columns for every starting column
	int			m_nLastBodyRow				= 0;		// last row of body  (#rows - 1)

	CTBScrollBar*	m_pHScrollBar			= NULL;
	CTBScrollBar*	m_pVScrollBar			= NULL;

	DataStr		m_dsRowIndicator;

	CriticalArea*	m_pProcessingMouse		= NULL;
	CPoint			m_PrevMousePos			= CPoint(0,0);
	BOOL			m_bMousePosChanged		= FALSE;
	BOOL			m_bDrawXORFirstTime		= TRUE;
	int				m_nResizingCol			= -1;
	double			m_dUITitlesRows			= 1.0;
	BOOL			m_bTwoClicks			= FALSE;	// gestione Hyperlink: serve per distinguere il primo click di spostamento del cursore
	// dal secondo che fa scattare l'hyperlink
	int				m_nLastColumRight		= 0;		//calcolata in DrawTitles

	// Sort
	CBitmap			m_bmpSortDesc;
	CBitmap			m_bmpSortAsc;
	CObArray		m_OrdSelectedColumn;
	BOOL			m_bOrdering				= FALSE;

	// Ricerca/Filtro
	CBitmap				m_bmpFiltered;
	CBitmap				m_bmpSearched;
	UINT				m_nSearchColumnIDC	= 0;
	ECompareType		m_cmpType			= CMP_EQUAL;
	CStringArray		m_arSearchText;
	int					m_nFoundRow			= -1;
	int					m_nFoundColumn		= -1;

	//----
	CBitmap				m_bmpClose;

	// lock colonne
	UINT			m_nLockedColumnIDC		= 0;
	CBitmap			m_bmpLocked;
	CBitmap			m_bmpLock;
	CBitmap			m_bmpUnlock;
	int				m_nMouseOverTitle		= -1;
	BOOL			m_bMouseOverLockBitmap	= FALSE;

	BOOL			m_bMouseOverCloseBitmap = FALSE;
	CString			m_sRowIndicatorFormat;

	DragButton				m_DragButton	= DB_NONE;
	BOOL					m_AllowRightDrag = FALSE;
	CTBMenu					m_ContextMenu;

	// Selezioni multiple
	CBodyBitmapButton*		m_pBtnMultiSelection	= NULL;
	SelArray				m_SelRecords;
	int						m_IdxEndSelection		= -1;
	int						m_IdxLastSelection		= -1;

	//altro
	BOOL		m_bAlternateColor			= FALSE;
	COLORREF	m_crAlternateColor			= RGB(180, 180, 180);
	COLORREF	m_crMultiSelTextColor		= RGB(0, 0, 0);
	COLORREF	m_crMultiSelBackColor		= RGB(130, 130, 130);

	int			m_nColSelected				= -1;

	TFXDataTip*				m_pDataTip = NULL;
	CBETooltipProperties	m_ttp;

	BOOL					m_bAllowColumnHeaderSmallFont	= FALSE;
	BOOL					m_bUseColumnHeaderSmallFont		= FALSE;
	BOOL					m_bAddColumnHeaderExtraSpace	= FALSE;

	BOOL					m_bHasFooter		= FALSE;

	SubFolderBehaviourOnDropFiles m_SubFolderBehaviour = UNDEFINED_DEEP;	//x drag files/folders

	// Drag & Drop
	BOOL					m_bDragging			= FALSE;
	BOOL					m_bTargetDragging	= FALSE;
	UINT					m_TimerScroll		= 0;
	int						m_IdxRecBeginDrag	= -1;
	int						m_IdxDragRecDest	= -1;

	CRect					m_rcDrag			= CRect(0,0,0,0);
	CTBExtDropBitmaps		m_DropBmps;

	CTBExtBEDropTarget*		m_pDropTarget	= NULL;
	DragSelType				m_DragSelType	= DragSelType::DST_TOP;
	CRuntimeClass*			m_pRtDropTarget = NULL;

	CTBEDataCoDec*			m_pDataCoDec	= NULL;
	BOOL					m_bOwnDataCoDec = FALSE;
	CTBEDataCoDec*			m_pClpBrdDataCodec	= NULL;		//x Cipboard; può essere diverso da m_pDataCoDec

private:
	BOOL	m_bValidatingCell				= FALSE;

	int		m_nWebRowStart					= -1;
	int		m_nWebRowEnd					= -1;

	int		m_nLayoutSuspended				= 0;

	CSlaveViewContainer*	m_pRowViewContainer = NULL;

	BOOL	m_bInternalReadOnly				= FALSE;
	BOOL	m_bAllColumnsReadOnly			= FALSE;

	static const UINT	g_InternalCommand[];

public:
	// Constructors
	CBodyEdit(const CString sName = _T(""));
	virtual ~CBodyEdit();

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

public:

	BOOL		IsReadOnly() const { return m_bInternalReadOnly || m_bAllColumnsReadOnly; }
	BOOL		IsAllColumnsReadOnly() const { return m_bAllColumnsReadOnly; }

	BOOL		BEGetExStyle(__int64 dwStatus) const		{ return (m_dwExStyles & dwStatus) == __int64(dwStatus); }
	void		BESetExStyle(__int64 dwStatus, BOOL bSet)	{ bSet ? (m_dwExStyles |= dwStatus) : (m_dwExStyles &= ~dwStatus); }
	void		BEAddExStyle(__int64 dwStatus)				{ BESetExStyle(dwStatus, TRUE); }
	void		BERemoveExStyle(__int64 dwStatus)			{ BESetExStyle(dwStatus, FALSE); }
	BOOL		BEExistsExStyle(__int64 dwStatus) const		{ return (m_dwExStyles & dwStatus) != 0; }

	CString				GetBEName() const { return m_sName; }
	const CString&		GetTitle() const { return m_strBodyTitle; }
	void				SetTitle(CString strBodyTitle)		{ m_strBodyTitle  = strBodyTitle;}

	// data attach
	void	Attach
		(
		DBTSlaveBuffered*	pDBT,
		CRuntimeClass*		pRowFormViewClass = NULL,
		const CString&		strRowFormViewTitle = _T(""),
		const CString&		sRowViewName = _T("")
		);

	void	ReAttachRowFormView
		(
		CRuntimeClass*		pRowFormViewClass,
		const CString&		strRowFormViewTitle = _T(""),
		const CString&		sRowViewName = _T("")
		);

	void	SetBodyEditID(UINT nIDC) { m_nID = nIDC; }
	UINT	GetBodyEditID() { return m_nID; }

	int		GetRowsHeight() const { return m_nRowsHeight; }
	int		GetMinHeight();

	void	SuspendLayout();
	void	ResumeLayout(BOOL bAlignDBT = TRUE);
	void	RecalcColumnsTitleWidth();

	//use pParentWnd only if nIDC is in another dialog, not the bodyedit one
	CSlaveViewContainer* SetRowViewContainer(UINT nIDC, CWnd* pParentWnd = NULL);

	BodyEditColumn* AddColumnInFormManager(ColumnInfo* pInfo, BodyEditInfo* pBodyEditInfo, const int& nPos);

	// customizers
	// to associate customized ParsedCtrl control
	ColumnInfo*	AddColumn(
		const CString&	strName,
		const CString&	strColTitle,
		DWORD			dwStyle,
		UINT			nIDC,
		CParsedCtrl*	pCtrl
		);

	ColumnInfo*	AddColumn(
		const CString&	strName,
		const CString&	strColTitle,
		DWORD			dwStyle,
		UINT			nIDC,

		DataObj*		pDataObj,
		CRuntimeClass*	pParsedCtrlClass,

		HotKeyLink*		pHotKeyLink = NULL,
		UINT			nBtnID = BTN_DEFAULT
		);
	ColumnInfo*	AddColumn(
		const CString&	strName,
		const CString&	strColTitle,
		DWORD			dwStyle,
		UINT			nIDC,

		DataObj*		pDataObj,
		CRuntimeClass*	pParsedCtrlClass,

		CString			sNsHotKeyLink,
		UINT			nBtnID = BTN_DEFAULT
		);

	ColumnInfo*	CreateDefaultColumn(CString sControlClass, const CString& sColName);

	void	SetMaxRecords(int nRec)					{ m_nMaxBodyRecords = nRec; }
	int		GetMaxRecords()							{ return m_nMaxBodyRecords; }
	void	SetMultipleLinesPerRow(int nLines = 1)	{ ASSERT(m_hWnd == NULL); m_nLinesPerRow = nLines; }
	void	EnableInsertRow(BOOL bEnable)			{ m_bInsertRowEnabled = bEnable; }
	BOOL	CanInsertRow()							{ return m_bInsertRowEnabled; }
	void	EnableAddRow(BOOL bEnable)				{ m_bAddRowEnabled = bEnable; }
	BOOL	CanAddRow()								{ return m_bAddRowEnabled; }
	void	EnableDeleteRow(BOOL bEnable)			{ m_bDeleteRowEnabled = bEnable; }
	BOOL	CanDeleteRow()							{ return m_bDeleteRowEnabled; }
	void	EnableFormViewCall(BOOL bEnable)		{ m_bFormViewCallEnabled = bEnable; }
	BOOL	CanCallFormView()						{ return m_bFormViewCallEnabled; }

	void	EnableUserButton(BOOL bEnable, BOOL bDisableOnReadOnly = FALSE, UINT nIDC = 0);
	void    EnableOrdering(BOOL bEnable, BOOL bApplyOnBrowse = FALSE, BOOL bEnableDuringEdit = FALSE);
	void    SetColumnOrdering(ColumnInfo* pCol, ColumnInfo::ColumnOrderingType Order);
	void	RemoveColumnFromOrderingList(ColumnInfo* pCol, BOOL bForceRemove = FALSE);
	void	ToggleColumnOrdering(ColumnInfo* pCol, BOOL bClearAll);
	void	ClearOrderingList();
	int		NumberOfUserButtons();
	BOOL	EnableWindow(BOOL bEnable = TRUE);

	ColumnInfoStateCtrlData*	AttachStateData(ColumnInfo* pColInfo, DataObj* pDataObj, BOOL bInvertDefaultStates = FALSE);
	ColumnInfoStateCtrlData*	AttachStateCmd(ColumnInfo* pColInfo, const CString& idb);

	DBTSlaveBuffered*	GetDBT()	const	{ return m_pDBT; }
	CAbstractFormDoc*	GetDocument()	const;
	CRowFormView*		GetRowFormView()	{ return m_pRowFormView; }

	void		SetDBT(DBTSlaveBuffered* pDBT, BOOL bInitCurrRecord = TRUE);
	void		OnSwitchDBT(DBTSlaveBuffered* pNewDbt);
	BOOL		SetToBadCell(int nNewRec, DataObj* pData, BOOL bEmitError);
	BOOL		CheckRecords(BOOL bEmitError);
	BOOL		SetCurrRecord(int nNewRec, BOOL bNotify = TRUE);
	SqlRecord*	GetCurrRecord()	const		{ return m_nCurrRecordIdx < 0 || m_nCurrRecordIdx > m_pDBT->GetUpperBound() ? NULL : m_pDBT->GetRow(m_nCurrRecordIdx); }
	int			GetCurrRecordIdx()	const	{ return m_nCurrRecordIdx; }
	ColumnInfo*	GetCurrColumn()	const		{ return m_nCurrColumnIdx < 0 ? NULL : m_ColumnsInfo[m_nCurrColumnIdx]; }

	int			GetAllColumnsInfoNumber() const	{ return m_AllColumnsInfo.GetSize(); }
	ColumnInfo*	GetColumn(const CString& sName);
	ColumnInfo*	GetColumn(UINT nIDC);
	ColumnInfo*	GetColumnFromIdx(int nIdx)		{ return (nIdx >= 0 && nIdx < m_AllColumnsInfo.GetSize()) ? m_AllColumnsInfo.GetAt(nIdx) : NULL; }
	ColumnInfo*	GetColumnFromDataIdx(int nDataIdx);
	int			GetAllColumnIdx(UINT nIDC);
	int			GetColumnIdx(ColumnInfo* pColInfo);
	int         GetStartRecordIdx()				{ return m_nStartRecordIdx; }

	int			GetAllColumnsInfoUpperBound() const		{ return m_AllColumnsInfo.GetUpperBound(); }
	int			GetVisibleColumnsInfoUpperBound() const	{ return m_ColumnsInfo.GetUpperBound(); }
	int			GetColumnsInfoNumber() const			{ return m_ColumnsInfo.GetSize(); }
	int			GetVisibleColumnsInfoNumber() const		{ return m_ColumnsInfo.GetSize(); }
	ColumnInfo*	GetVisibleColumn(UINT nIDC);
	ColumnInfo*	GetVisibleColumnFromIdx(int nIdx)		{ return (nIdx >= 0 && nIdx < m_ColumnsInfo.GetSize()) ? m_ColumnsInfo.GetAt(nIdx) : NULL; }
	ColumnInfo*	GetVisibleColumnFromDataIdx(int nDataIdx);
	int			GetVisibleColumnIdx(UINT nIDC);
	ColumnInfo*	GetVisibleColumn(DataObj* pData);
	int			GetSelectedColumn() const				{ return m_nColSelected; }
	CString		GetCellText(int nRow, int nCol);

	void	ActivateBody(UINT nMode);
	void	UpdateBodyStatus();
	void	UpdateCtrlBody(UINT nColumnIDC = -1, int nRow = -1);
	void	OnRowFormViewDied(CRowFormView*);
	BOOL	ShowColumnCtrl(int nCol);
	BOOL	ShowColumnCtrl(DataObj* pData);

	void	DoMoveToHomeEnd(BOOL bGotoHome);
	void	DoMoveToFirstCol();
	void	DoMoveToLastCol();
	void	DoMoveToPrevRow(BOOL bShowCtrl = FALSE);
	void	DoMoveToNextRow(BOOL bShowCtrl = FALSE, BOOL bGoHead = FALSE, BOOL bWeel = FALSE);
	void	DoMoveToFirstRow(BOOL bShowCtrl = FALSE);
	void	DoMoveToLastRow(BOOL bShowCtrl = FALSE);
	BOOL	DoInsertRecord();

	BOOL	GetHasUINoVertLines()	const			{ return !BEGetExStyle(BE_STYLE_SHOW_VERT_LINES); }
	void	SetUINoVertLines(BOOL bSet = TRUE)		{ BESetExStyle(BE_STYLE_SHOW_VERT_LINES, !bSet); }

	BOOL	GetHasUINoHorizLines()	const			{ return !BEGetExStyle(BE_STYLE_SHOW_HORIZ_LINES); }
	void	SetUINoHorizLines(BOOL bSet = TRUE)		{ BESetExStyle(BE_STYLE_SHOW_HORIZ_LINES, !bSet); }

	void	SetUITitlesRows(double d)					{ m_dUITitlesRows = d; }
	double	GetUITitlesRows()					const	{ return m_dUITitlesRows; }

	void	SetCurrLine(int nRec, int nStart = -1);

	BOOL	HasFocus()								{ return m_bBodyFocused; }

	void	RecalculateAllColPos();
	void	UpdateColumnSizes();
	BOOL	RecalcColumnsLayout();
	void	RecalcVisibleColumns();

	int		GetNextEditableColumnFrom(int nCol);
	int		GetPrevEditableColumnFrom(int nCol);

	void	SetVScrollRange();
	void	SetHScrollRange();

	BOOL	CanLeaveCurrPos(int nNewCurrRec, BOOL bCheckLastRow = TRUE);

	void	Invalidate(BOOL bErase = TRUE);
	void	InvalidateBody();
	void	RefreshBody(BOOL bRedraw = TRUE);

	void	UpdateFooters();

	//Lock Column
	void	InitializeLock(UINT nLockedColumnIDC, BOOL bFromCustomize = FALSE);
	void	EnableColumnLocking(BOOL bEnable, BOOL bInteractive);
	void	LockColumn(UINT nIDC, BOOL bFromCustomize = FALSE);
	void	UnLockColumn(BOOL bFromCustomize = FALSE);
	UINT	GetLockedColumnIDC()  const { return m_nLockedColumnIDC; }
	int		GetFirstUnlockedVisibleColumn();

	//----
	void	SetImageList(CImageList* pImgList, int nWidth)
	{
		m_pImageList = pImgList;  m_nBmpMaxWidth = nWidth;
	}
	void	SetImageList(CParsedStateImage* pImgList, int nWidth)
	{
		m_pImages = pImgList;  m_nBmpMaxWidth = nWidth;
	}

	// per aggiungere un bottone nella toolbarina in basso a sinistra
	void	AddUserButton(UINT nIDC, UINT nIDBUp, UINT nIDBDown, UINT nIDBDisabled, LPCTSTR szToolTip);
	void	AddUserButton(const CString& sName, UINT nIDC, UINT nIDBUp, UINT nIDBDown, UINT nIDBDisabled, LPCTSTR szToolTip);
	void	AddUserButton(const CString& sName, UINT nIDC, const CString& sBitmapResource, LPCTSTR szToolTip);

	void	AddUserReportButton(UINT nIDC, LPCTSTR szToolTip, BOOL bEnable = TRUE, BOOL bDisableOnReadOnly = FALSE);

	CBEButton* 	AddFooterButton(LPCTSTR szName, UINT nIDC, UINT nIDB, LPCTSTR szToolTip, BOOL bEnable = TRUE, BOOL bDisableOnReadOnly = FALSE);
	CBEButton* 	AddFooterButton(LPCTSTR szName, UINT nIDC, const CString& sBitmapResource, LPCTSTR szToolTip, BOOL bEnable = TRUE, BOOL bDisableOnReadOnly = FALSE);
	CBEButton* 	AddHeaderButton(LPCTSTR szName, UINT nIDC, UINT nIDB, LPCTSTR szToolTip, BOOL bEnable = TRUE, BOOL bDisableOnReadOnly = FALSE);
	CBEButton* 	AddHeaderButton(LPCTSTR szName, UINT nIDC, const CString& sBitmapResource, LPCTSTR szToolTip, BOOL bEnable = TRUE, BOOL bDisableOnReadOnly = FALSE);

	CBEButton* 	AddFooterReportButton(UINT nIDC, LPCTSTR szToolTip, BOOL bEnable = TRUE, BOOL bDisableOnReadOnly = FALSE);

	// per modificare la bitmap di un user botton
	void	ChangeUserButton(UINT nIDC, UINT nIDBU, UINT nIDBD, UINT nIDBDI, LPCTSTR szToolTip = NULL);
	void	ChangeUserButton(UINT nIDC, const CString& sBitmapResource, LPCTSTR szToolTip = NULL);

	//---- MultiSelection
	void				BeginMultipleSel();
	void				EndMultipleSel(BOOL bRefreshBody = TRUE);
	void				EnableMultipleSel(BOOL bSet = TRUE);
	MultiSelMode		GetMultiSelMode() const { return m_MultiSelection; }
	void				SetMultiSelMode(MultiSelMode mode);
	bool				IsMultiselection() { return GetMultiSelMode() == MultiSelMode::MULTIPLE_SEL; }
	void				ChangeRangeStatus(int nStart, int nEnd, SelStatus status, InvertMode invert = NOT_INVERT, BOOL bInvalidate = TRUE);
	const	SelArray&	GetSelections()			const { return m_SelRecords; }
	int					GetSelRowsUpperBound()			const { return m_SelRecords.GetUpperBound(); }
	SelStatus			GetSelRowsStatus(int nIdx)	const { return m_SelRecords[nIdx]; }
	void				SelectRows(CArray<int>& aiSelectedRow);
	int					CountSelectedRows();
	void				CopySelections(RecordArray&	ra);
	void				ApplySelections(RecordArray&	ra);
	void				ClearSelections();
	void				ResizeSelections(BOOL bStart = FALSE);

	virtual BOOL		CanDoMultipleSel() const { return BEGetExStyle(BE_STYLE_ALLOW_MULTIPLE_SEL); }

	virtual void		OnBeginMultipleSel	() {}
	virtual void		OnEndMultipleSel	() {}
	virtual	void		CustomizeSelections	(SelArray&) {}

	void		DoOnBeginMultipleSel();
	void		DoOnEndMultipleSel();
	void		DoCustomizeSelections(SelArray&	sel);

	//---- END Multiselection

	void	NextWebPage();
	void	PrevWebPage();

	BOOL	GetBodyItem(CPoint point, ColumnInfo*& pCol, int& nRowDBT, DataObj*& pCell);

	void	SetCellPos(int nR, int nColIDC, BOOL bAsync = TRUE);

	void	SetContentModified(bool bModified){ m_bModified = bModified; }
	bool	GetContentModified(){ return m_bModified; }

	void	ReadRows(int upperLimit);

	void		ShowDragging(const CPoint& point);
	void		StartDragging();
	void		StopDragging();

	void		SetAllowRDrag(BOOL bSet = TRUE)	{ m_AllowRightDrag = bSet; }
	BOOL		GetAllowRDrag() const			{ return m_AllowRightDrag; }
	DragButton	GetDragButton() const			{ return m_DragButton; }

	void		SetAllowCopy(BOOL bSet = TRUE)	{ BESetExStyle(BE_STYLE_ALLOW_COPY, bSet); }
	BOOL		GetAllowCopy() const			{ return	BEGetExStyle(BE_STYLE_ALLOW_COPY); }

	void		SetAllowPaste(BOOL bSet = TRUE)	{ BESetExStyle(BE_STYLE_ALLOW_PASTE, bSet); }
	BOOL		GetAllowPaste() const			{ return	BEGetExStyle(BE_STYLE_ALLOW_PASTE); }
	void		DoPasteForDrop(CTBEDataCoDec*	pClpBrdDataCodec, int nIdxRec, DragSelType dstWhere);

	void		EnableDrag(BOOL bEnable = TRUE) { BESetExStyle(BE_STYLE_ALLOW_DRAG, bEnable); }
	void		EnableDragReadOnly(BOOL bEnable = TRUE) { BESetExStyle(BE_STYLE_ALLOW_DRAG_READONLY_DOC, bEnable); }
	void		GetDragPosition(const CPoint& point, int& nIdx, DragSelType& seltype);

	BOOL		DoDrag(int nSourceRow, UINT nSourceColIDC);
	void		HandleSelection(UINT nFlags, int	nNewCurrRec);
	void		HandleScroll();
	void		StopScrollTimer();

	// protetta perche' devono essere chiamata nella customize

	void			AttachDataCoDec(CTBEDataCoDec*	pDataCoDec, BOOL bOwn);
	CTBEDataCoDec*	GetDataCoDec() const { return m_pDataCoDec; }

	void		SetDropTarget(CRuntimeClass*	pRTDrop) { m_pRtDropTarget = pRTDrop; }

	void		EnablePaste(BOOL bEnable = TRUE, BOOL bSetTool = TRUE, CTBEDataCoDec* pDataCoDec = NULL);
	//{
	//	BESetExStyle(BE_STYLE_ALLOW_PASTE, bEnable);
	//	if (bEnable && bSetTool) AttachDataCoDec(pDataCoDec ? pDataCoDec : new CTBEDefaultCoDec(m_pDBT->GetRuntimeClass()), TRUE));
	//}

	void		EnableDrop(BOOL bEnable = TRUE, BOOL bSetTool = TRUE, CRuntimeClass* pRTDrop = NULL);
	//{
	//	BESetExStyle(BE_STYLE_ALLOW_DROP, bEnable);
	//	if (bEnable && bSetTool) SetDropTarget(pRTDrop ? pRTDrop : RUNTIME_CLASS(CTBEDefaultDropTargetCoDec));
	//}

	// Vedi CanDoCopy
	BOOL	IsCopyEnabled() const;	//Renamed 2.1.5: BOOL EnableCopy() const;
	// Vedi CanDoPaste
	BOOL	IsPasteEnabled();	//Renamed 2.1.5: BOOL EnablePaste() const;

	int		ResizeColumn(ColumnInfo* pColInfo, int nNewWidth, BOOL bUpdateTbf = FALSE);
	int		ResizeColumn(UINT nIDC, int nNewWidth);
	void	RemoveColumn(ColumnInfo* pInfo);

	void	Rename(CString& sName);

	void	MoveColumnTo(ColumnInfo* pInfo, const int& nIdx);
	void	MoveColumnNear(ColumnInfo* pInfo, UINT nBeforeIDC, UINT nAfterIDC = 0);
	void	MoveColumnNear(ColumnInfo* pInfo, DataObj* pData, BOOL bAfter = TRUE);
	void	RecalcColumnsOrder();

	static BOOL IsInternalCommand(UINT nID);
	static BOOL IsFocusCommand(UINT nID);
	static BOOL IsStandardButton(UINT nID);

protected:
	void		ShowRowIndicator();
	void		UpdateRowIndicator();
	void		ResetRowIndicator();

	// H Scroll Bar
	void			CenterHScrollCenter();
	void			SetHScrollRange(int nMin, int nMax);
	void			SetHScrollPos(int nPos);
	void			SetHEnableWindow(BOOL bEnable);
	void			SetHVisible(BOOL bVisible);

	CTBScrollBar*	FindHScrollBar();

	BOOL	RecalcRowsLayout();
	void	RepositionAccessories();
	BOOL	UpdateData(BOOL bSignalError, BOOL bSendMessage = FALSE);
	void	NotifyToParent(UINT mess);
	void	SetFocusOnly();
	int		GetLogicalStartColumnIdx()			const;
	int		GetEndColumnIdx()			const;//	{ return m_LastColsIdx[GetLogicalStartColumnIdx()] & ~FLAG_END_COL_NOT_FULL_VISIBLE; }
	BOOL	IsEndColFullVisible(int nStart)const;//	{ return (m_LastColsIdx[nStart] & FLAG_END_COL_NOT_FULL_VISIBLE) != FLAG_END_COL_NOT_FULL_VISIBLE; }
	int		GetEndRecordIdx()			const;//	{ return min(m_pDBT->GetUpperBound(), m_nStartRecordIdx + m_nLastBodyRow);}
	BOOL	IsCurrColFullVisible()			const;

	int		GetPrevPageStartColumnIdx();
	int		GetNextPageStartColumnIdx();
	int		GetStartIdxForLastColumn();

	BOOL	SetNextEditableColumnFrom(int nStart, int nCol);
	void	ScrollToShowUnlockedHiddenColumns();

	void	SaveStartRecord();
	void	SaveStartColumn();
	void	UnsaveStartRecord();
	void	UnsaveStartColumn();
	void	ResetBodyStatus();
	BOOL	CanDoMoving(BOOL bUp = FALSE, BOOL bCheckRow = FALSE, BOOL bEmitError = TRUE);

	BOOL	CallDialog();
	void	RebuildLinks(int nRec = -1, BOOL bDoPrepareAuxData = FALSE);

	void	LoadNextRecords(BOOL bSkipCheckOnLastVisibleRow = FALSE);

	BOOL	ResetToVisiblePosition();
	BOOL	IsCtrlVisible()	const;
	void	UpdateHScrollPos();
	void	UpdateVScrollPos();

	void	DoMessageOnRecordChanged();

	BOOL	CreateAccessories();
	void	MakeLastColsIdxArray();
	BOOL	CalculateLastColIdx(int nStartIdx, int nStartWidth, int& nLastIdx);
	void	DrawFrame(CDC* pDC);
	void	DrawCarret(int nCurrRow, CDC* pDC = NULL);
	void	DrawSepLine(CDC* pDC, int nXStart, int nYStart, int nXEnd, int nYEnd);
	void	DrawVerticalLine(CPoint&, COLORREF, int nPenStyle);
	void	FillRect(CDC* pDC, const CRect& rect, COLORREF color);
	void	TrackingDraw(CDC&);

	void	DoMoveToPrevCtrl();
	void	DoMoveToNextCtrl();
	void	DoMoveToPrevCol();
	void	DoMoveToNextCol();

	BOOL	DoPageLeft();
	BOOL	DoPageRight();
	BOOL	DoPageUp();
	BOOL	DoPageDown();

	void	DoUpExit();
	void	DoDownExit();
	void	PrevDlgItemSetFocus();
	void	NextDlgItemSetFocus();

	BOOL	CanDoMoveToPrevRow();
	BOOL	CanDoMoveToNextRow();
	BOOL	CanDoMoveToFirstRow();
	BOOL	CanDoMoveToLastRow();

	BOOL	CanDoScrollLeft() { return m_nStartColumnIdx > 0; }
	BOOL	CanDoScrollRight() { return m_nStartColumnIdx < GetStartIdxForLastColumn(); }
	void	DoScrollHoriz(int nNewStart);
	void	DoScrollLeft(int nNewStart);
	void	DoScrollRight(int nNewStart);
	void	DoScrollVert(int nNewStart);
	void	DoScrollDown(int nNewStart);
	void	DoScrollUp(int nNewStart);

	void	OnCommandDone();

	void	LoadAllRows(BOOL bAskConfirm = FALSE);

	//Sort
	void	Sort();
	//Search/Filter
	void	EnableSearch(BOOL bSet = TRUE, BOOL bSetFilter = TRUE);
	void	ResetSearchPoint(BOOL bAll);
	void	DoRemoveFilter(BOOL bRefreshBody, int nColSelected = -1);

	BOOL	GetCellFormattedText(ColumnInfo* pCol, SqlRecord* pRec, DataStr& ds);
	void	ChangeHeightRow(UINT nSBCode);
	BOOL	IsDuplicateButton(UINT nID);

	//Toolbars
	CBEButton*	AddAuxToolBarButton(const CString& sBtnName, int nIDC, int nIDB, const CString& stooltip);	//add to the header toolbar
	CBEButton*  AddAuxToolBarButton(const CString& sName, UINT nID, CString strNsImage, CString strTooltip, CString strText = L"", int	nIndex = -1);
	void		EnableButton(UINT ID, BOOL bEnable);
	//void		CreateMiniToolbar			();

	BOOL CanInsertRowByBodyTree();
	BOOL CanInsertRowByTreeEdit(BOOL bEmitError = TRUE);

	DataObj*			GetDataObjAt(SqlRecord* pRec, ColumnInfo* pCol);
	DataObj*			GetDataObjFromColumnName(SqlRecord* pRec, const CString& sName);
	BodyEditColumn*		GetColumnDescription(ColumnInfo* pColInfo);

	void	ResizeColumn(ColumnInfo* pColInfo);
	void	ResizeColumn(ColumnInfo* pColInfo, ColumnInfo::ColumnWidthType wt);

	void	OnToggleMultipleSelection();

private:
	CParsedCtrl*	CreateParsedCtrl(
		CString			strName,
		UINT			nIDC,
		DataObj*		pData,
		CRuntimeClass*	pParsedCtrlClass,
		HotKeyLink*		pHotKeyLink = NULL,
		UINT			nBtnID = BTN_DEFAULT
		);
	void			RecalcCtrlLen(DataObj* pDataObj, int nLen, ColumnInfo* pColumnInfo);

	BOOL	ColumnLocked() const { return BEGetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK) && GetLockedColumnIDC() != 0; }
	int		PhysicalToLogicalColIdx(int nPhysical);
	BOOL	GetCustomColor(int nRow, int nCol, CBodyEditRowSelected* pCurRow);

	BOOL	CurrentColumnBeyondLastFullyVisible()
	{
		return m_nCurrColumnIdx > GetEndColumnIdx()
			|| (m_nCurrColumnIdx == GetEndColumnIdx() && !IsEndColFullVisible(GetLogicalStartColumnIdx()));
	}

	BOOL	AdjustScrollPosition();
	void	SetStatusBarText(CString strText);
	void	GetAllFiles(CStringArray& arFiles, CString strPath);

	BOOL	DoSearch(ColumnInfo* pCol, const CStringArray& arCmpValues, ECompareType cmpType = ECompareType::CMP_CONTAINS);

	CSlaveViewContainer* SetRowViewContainer(CSlaveViewContainer* pContainer, BOOL bCallDialog);


	static		void    DrawTransparentBitmap(CDC* pDC, CBitmap& Bmp, const CPoint& ptBmpDest, int nBmpDestHeight, COLORREF crBkColor = AfxGetThemeManager()->GetBETransparentBmpColor());

public:
	virtual	void			Customize()	{}
	virtual CParsedCtrl*	GetCurrentParsedCtrl(UINT nIDC);
	virtual CParsedCtrl*	GetCurrentParsedCtrl(CTBNamespace aNS);
	virtual CParsedCtrl*	GetCurrentParsedCtrl(DataObj* pDataObj);
	virtual void			OnUpdateControls(BOOL bParentIsVisible = TRUE);
	virtual void			SetDataModified(BOOL bMod);

	virtual BOOL	Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID = 0xffff);
	virtual BOOL	SubclassDlgItem(UINT nID, CWnd* pParent); //prima si chiamava impropriamente SubclassEdit

	virtual	void	BeforeCustomize();
	virtual	void	OnBeforeCustomize() {}

	virtual	void	GetCurrCellPos(CGridControlCellPos&);
	virtual	void	SetCurrCellPos(const CGridControlCellPos&, BOOL bNotify = TRUE);
	virtual	void	SetBodyReadOnly(BOOL bReadOnly = TRUE);
	virtual BOOL	CheckAllColumnsReadOnly();


	virtual CString	GetRowFormViewTitle() { return m_strRowFormViewTitle; }
	virtual CString	GetRowFormViewName() { return m_strRowFormViewName; }
	virtual UINT	GetRowFormViewId() { return 0; }
	virtual BOOL			ShowCtrl(BOOL bRedraw);
	virtual CursorPosArea	GetCursorBodyPos(CPoint& point, int& nCol, int& xColOffs, int& nRow, int& nNewCurrRec);
	virtual void			OnDoSearch();

	//Gestione ContextMenu
	virtual	BOOL	OnShowContextMenu(CTBMenu*) { return TRUE; }
	virtual	BOOL	OnShowTitleContextMenu(CTBMenu*) { return TRUE; }
	//Menu di contesto differente per la cella/area della cella corrente
	virtual BOOL	OnCustomContextMenu(int /*nCol*/, int /*nRow*/, CPoint /*ptClient*/) { return FALSE; }
	virtual	BOOL	OnShowCustomContextMenu(CTBMenu*) { return FALSE; }
	virtual	BOOL	OnShowCustomContextMenu(CTBMenu*, int /*nCol*/, int /*nRow*/, CPoint /*ptClient*/) { return FALSE; }
	virtual	CTBExtBEOleDropSource*	CreateDropSource();

	// Metodi da re-implementare nel BE derivato per abilitare i comandi nel menu contestuale

	// Il copy in EDIT o in NEW è abilitato di default o se nel BE è re-implementato il CanDoCopy()
	virtual	BOOL		CanDoCopy() const { return GetAllowCopy(); }
	// Il copy in BROWSE è abilitato di default. Per disabilitarlo re-implementare il CanDoCopyInBrowse()
	virtual	BOOL		CanDoCopyInBrowse() const { return GetAllowCopy(); }
	// Il paste è disabilitato di default e per abilitarlo (solo in NEW o EDIT)
	// occorre che  i dati nella cliboard siano in un formato noto
	// e il BE autorizzi il paste con CanDoPaste(), pClpBrdDataCodec contiene i dati eventualmente caricati
	virtual	BOOL		CanDoPaste(CTBEDataCoDec*	pClpBrdDataCodec, BOOL bFromDrop = FALSE) const { return FALSE; }
	// Abilita il drag quando il doc è in browse
	virtual	BOOL		CanDoDragInBrowse() const { return FALSE; }

	//aggiunge una callback da chiamare alla distruzione del documento
	//void AddDisposingHandler (CObject* pListener, ON_DISPOSING_METHOD pHandler) { m_Handler.AddDisposingHandler(pListener, pHandler); }
	//void RemoveDisposingHandlers (CObject* pListener) { m_Handler.RemoveDisposingHandlers(pListener); }

	virtual int			 GetCtrlColumnDataIdx(const CTBNamespace aNS);
	virtual int			 GetColumnIdxFromParsedCtrl(CTBNamespace aNS);

	virtual BOOL CanInsertRowSimple();
	virtual BOOL CanDeleteRowSimple();

	virtual BOOL InternalCanDeleteRow();
	BOOL InternalCanInsertRow(BOOL bEmitError = TRUE);

	virtual BOOL		IsMultiValue() const { return TRUE; }
	virtual int			GetRowNumber() const;
	virtual DataObj*	GetCtrlData(const CString& /*sName*/, int /*nRow*/ = 0);
	virtual CString		GetCtrlClass()	{ return _T("BodyEdit"); }
	virtual CString		GetCtrlName()	{ return GetInfoOSL()->m_Namespace.GetObjectName(); }
	virtual int			EnumColumnName(CStringArray& /*arNames*/, BOOL /*bAll*/ = TRUE);
	virtual CString		OnFormatData(CBodyEditRowSelected* pCurrRow, int& nXOffset);

	virtual BOOL	CanDoInsertRow() { return TRUE; }
	//virtual BOOL	CanDoAppendRow		() { return CanDoInsertRow(); }
	virtual BOOL	CanDoDeleteRow() { return TRUE; }

			BOOL	DoCanDoDeleteRow();

	virtual BOOL	OnCanLeaveCurrPos(int nNewCurrRec) { return TRUE; }

	virtual BOOL	CanHideColumn(ColumnInfo*);
	virtual	UINT	GetAuxToolbarImageIDB(UINT nID);
	int GetHorizontalOffset(ColumnInfo* pColumn);

protected:
	// NON user overridable
	virtual	void 	OnAbortForm();
	virtual	BOOL 	OnCheckForm(BOOL bSignalError);
	virtual	BOOL	OnKeyHit(UINT nIDC, UINT nKey, UINT nHitState);
	virtual int		DoToolHitTest(CPoint point, TOOLINFO* pTI);
	virtual BOOL	DoToolTipNotify(CTooltipProperties& tp);

	virtual void	OnUpdateRowIndicator();
	virtual void	OnResetRowIndicator();
	virtual BOOL	GetCoordCell(CRect& /*rectCell*/, int& /*nRow*/, UINT& /*nIDC*/, int& /*nCol*/);
	virtual void	CalcCellRect(CRect& rect, int nRow, int nCol);
	virtual BOOL	OnDblClick(UINT /*nFlags*/, CBodyEditRowSelected* /*pRow*/) { return FALSE; }
	virtual BOOL	OnGetCustomColor(CBodyEditRowSelected* /*pRow*/) { return FALSE; }
	virtual BOOL	OnDrawCell(CBodyEditRowSelected* /*pRow*/, CDC*, CRect&) { return FALSE; }
	virtual CParsedCtrl* OnCreateParsedCtrl(UINT /*nIDC*/, CRuntimeClass* /*pParsedCtrlClass*/) { return NULL; }

	virtual BOOL	OnGetToolTipProperties(CBETooltipProperties* pTp);
	virtual void	CalcBodyLayout();

	virtual void	EnableButtons();
	virtual void	OnBEEnableButton(CBEButton*) {}	//girato al documento ed ai client document
	virtual void	OnEnableAuxToolBarButton(CBEButton* pb) { OnBEEnableButton (pb) ; }

	virtual	void	OnPaste(CTBEDataCoDec*	pClpBrdDataCodec = NULL, int nIdxRec = -1, DragSelType dstWhere = DST_CENTER);

	virtual void	OnDropFiles		(const CStringArray&);
	virtual BOOL	OnSubFolderFound() { return TRUE; } ; // intercettare per modificare la ricorsione in caso di drop di subfolder

	virtual BOOL	DoKeyDown(UINT nChar);
	virtual	BOOL	DoMovingKey(UINT nChar);

	virtual	void	DrawItem(LPDRAWITEMSTRUCT);
	virtual	void	DoDraw(CDC* pDC, LPDRAWITEMSTRUCT, const COLORREF crSepLine, const COLORREF crBackgnds);
	virtual	void	DrawAuxToolBars(CDC* pDC, LPDRAWITEMSTRUCT lpDIS, CRect&);
	virtual	void	DrawTitles(CDC* pDC, LPDRAWITEMSTRUCT, const COLORREF crBackgnd);
	virtual	void	DrawRow(CDC* pDC, LPDRAWITEMSTRUCT, int nRow, int nYCoord, BOOL bActiveRow);
	virtual	void		OnBeforeDrawRow(CDCDrawInfo& drawinfo);
	virtual	void		OnAfterDrawRow(CDCDrawInfo& drawinfo);
	virtual	BOOL		OnExtTextOut(int nXTxtStart, int nYStart, CDC* pDC, CRect& rectDraw, const CString& strCell, SqlRecord* pRec, ColumnInfo* pCol);
	virtual	BOOL		OnDrawText(CDC* pDC, CRect& rectDraw, const CString& strCell, SqlRecord*, ColumnInfo*);
	virtual void	DrawFooter(CDC* pDC, LPDRAWITEMSTRUCT, int nYCoord);

	virtual void	OnCalcFooter(ColumnInfo* pInfo, DataObj* pValue);

	virtual	BOOL	IsColumnSingleLine(ColumnInfo*) { return FALSE; }
	virtual BOOL	OnShowCtrl(SqlRecord*, ColumnInfo*, int /*xPos*/, int /*yPos*/, CRect&) { return FALSE; }
	virtual BOOL	OnDrawCurrentCell(int /*nRow*/, int /*nCol*/) { return FALSE; }
	virtual BOOL	ShowUIVertLine(ColumnInfo*) { return TRUE; }

	virtual ColumnInfo* GetAlternativeColumnForEditing(ColumnInfo*, SqlRecord*)							{ return NULL; }
	virtual void		SetUpAlternativeColumn(ColumnInfo* pOriginal, ColumnInfo* pAlternative)	{}

	// MFC specifics
	virtual	BOOL	OnCommand(WPARAM wParam, LPARAM lParam);
	virtual BOOL	DoInternalCommand(UINT nID, UINT nCode);

	virtual BOOL	OnCreateClient();
	virtual BOOL	OnPostCreateClient() { return TRUE; }

	virtual void	AddCtrlOffset(ColumnInfo*, int&) { }	//used by BodyEditTree

	virtual	BOOL	GetImageIDB(UINT nID, UINT& nIDB_up, UINT& nIDB_down, UINT& nIDB_disable);
	virtual	UINT	ReplaceImageIDB(UINT nIDB);

	BOOL IsLastColumnInScreen();

	virtual void 	OnSelCell(SqlRecord* pCurRec, ColumnInfo* pCol) {}
	virtual void 	OnShowCtrl(SqlRecord* pCurRec, ColumnInfo* pCol) {}
	virtual void 	OnHideCtrl(SqlRecord* pCurRec, ColumnInfo* pCol) {}

public:
	afx_msg	void	OnSelectAll();

protected:
	afx_msg	void 	OnHScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg	void 	OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg void	OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized);
	afx_msg	void	OnWindowPosChanged(WINDOWPOS FAR*);
	afx_msg void	OnKillFocus(CWnd* pOldWnd);
	afx_msg void	OnSetFocus(CWnd* pOldWnd);
	afx_msg void	OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void	OnRButtonDown(UINT nFlags, CPoint point);
	afx_msg void	OnLButtonUp(UINT nFlags, CPoint point);
	afx_msg void	OnLButtonDblClk(UINT nFlags, CPoint point);
	afx_msg void	OnMouseMove(UINT nFlags, CPoint point);
	afx_msg LRESULT	OnMouseLeave(WPARAM wPawam, LPARAM lParam);
	afx_msg BOOL	OnMouseWheel(UINT nFlags, short zDelta, CPoint pt);
	afx_msg	LRESULT	OnRecalcCtrlSize(WPARAM, LPARAM);
	afx_msg BOOL	OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT nMsg);
	afx_msg	void	OnTimer(UINT nIDEvent);
	afx_msg	void	OnMeasureItem(int nIDCtl, LPMEASUREITEMSTRUCT lpMeasureItemStruct);
	afx_msg	void	OnContextMenu(CWnd* pWnd, CPoint ptMousePos);
	afx_msg void	OnDropFiles(HDROP hDropInfo);
	afx_msg LRESULT	OnBadValue(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT	OnFormatStyleChange(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT	OnLosingFocus(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT	OnValueChanged(WPARAM wParam, LPARAM lParam);
	afx_msg void	OnCopy();
	afx_msg void	OnPaste();

	afx_msg	void	OnUnselectAll();
	afx_msg	void	OnInvertSelection();
	afx_msg	void	OnToggleAlternateColor();
	afx_msg	void	OnToggleAllColumnsWidth();
	afx_msg	void	OnToggleCurrentColumnWidth();
	afx_msg	void	OnUnlockColumn();
	afx_msg	void	OnColumnStatistic();
	afx_msg	void	OnShowSearch();
	afx_msg	void	OnShowFilters();
	afx_msg	void	OnRemoveFilters();
	afx_msg	void	OnRemoveColumnFilter();
	afx_msg	void	OnRemoveAllFilters();
	afx_msg	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT	OnSetCell(WPARAM wParam, LPARAM lParam);
	void			GetColumnInfoDescription(CWndObjDescription* pDesc, int& x, int& y);

public:

	afx_msg	void	InsertRecord();
	afx_msg	void	DeleteRecord();
	afx_msg	void	OnCallDialog() { CallDialog(); }

	DECLARE_MESSAGE_MAP()

	// diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

//==============================================================================
//INTERNAL
#define _NS_BE_TOOLBAR_BTN(name) _T(name)


#define DEFAULT_COLUMN_OFFSET	4

// se posto a zero determina che il control appaia all'interno
// della cella
#define CTRL_NOT_INSIDE		1

// Semaforizzazione di UpdateCtrlBody
#define UPDATE_BODY_REJECT_NONE			0
#define UPDATE_BODY_REJECT_ALL			1
#define UPDATE_BODY_REJECT_ROW_MOVE		2

// per gestire un caso particolare di messaggistica (vedi DoMessageOnRecordChanged)
#define	ON_BODY_ROW_CHANGED_MESSAGE		0x80000000
#define	ON_BODY_ROW_CHANGED_WARNING		(CTRL_IMMEDIATE_WARNING_ERROR | ON_BODY_ROW_CHANGED_MESSAGE)
#define	ON_BODY_ROW_CHANGED_ERROR		(CTRL_IMMEDIATE_FATAL_ERROR | ON_BODY_ROW_CHANGED_MESSAGE)

#define ID_BE_SELECT_ALL                ID_SELECT_ALL
#define ID_BE_UNSELECT_ALL              ID_UNSELECT_ALL
#define ID_BE_INVERT_SELECTION          ID_INVERT_SELECTION

//-----------------------------------------------------------------------------
inline BOOL XCoordInRect(int x1, int x2, const RECT& rect)
{
	return x1 <= rect.right && x2 >= rect.left;
}

//-----------------------------------------------------------------------------
inline BOOL YCoordInRect(int y1, int y2, const RECT& rect)
{
	return y1 <= rect.bottom && y2 >= rect.top;
}

//==============================================================================
#define TBE_AUXTOOLBAR_YOFFSET		22
#define TBE_AUXTOOLBUTTON_XOFFSET	24
#define TBE_AUXTOOLBUTTON_YOFFSET	21

//==============================================================================

#include "endh.dex"
