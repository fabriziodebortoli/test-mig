#pragma once
//#include <BCGCBPro\BCGCBProInc.h>

#include <TbNameSolver\Observable.h>
#include <TbNameSolver\CallbackHandler.h>

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGeneric\GeneralObjects.h>

#include <TbParser\TokensTable.h>

#include <TbGenlibManaged\ManagedWindowWrapper.h>

#include "OslInfo.h"
#include "reswalk.h"
#include "TBStrings.h"
#include "ExternalControllerInfo.h"
#include "LayoutContainer.h"
#include "Behaviour.h"

#include "extres.hjson" //JSON AUTOMATIC UPDATE
#include "parsres.hjson" //JSON AUTOMATIC UPDATE

// Key State Bits
#define KEY_SHIFT_DOWN	0x0001
#define KEY_CTRL_DOWN	0x0002
#define KEY_ALT_DOWN	0x0004
#define CHECK_UNIQUE_NAME(cl,name) 	{\
										DataObj* pDataObj = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, _T("AllActive"), DataBool(FALSE), szTbDefaultSettingFileName);\
										BOOL bByPass = pDataObj && pDataObj->GetDataType() == DATA_BOOL_TYPE ? *((DataBool*)pDataObj) : FALSE;\
										if (!bByPass)\
										{\
											ASSERT_TRACE((name.IsEmpty() || GetWndLinkedCtrlByName(cl, name) == NULL), _T("Found dublicate control name: ") + name + '\n');\
										}\
										else if (GetWndLinkedCtrlByName(cl, name)) {return NULL;}\
									}

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class CEdit; class CParsedCtrl;
	class CParsedEdit;
		class CStrEdit;
			class CPathEdit;
			class CIdentifierEdit;
		class CIntEdit;
		class CLongEdit;
		class CDoubleEdit;
			class CMoneyEdit;
			class CQuantityEdit;
			class CPercEdit;
			class CMetricEdit;
		class CDateEdit;
			class CDateSpinEdit;
			class CDateTimeEdit;			
			class CTimeEdit;
				class CTimeSpinEdit;
		class CElapsedTimeEdit;		//@@ElapsedTime
		class CBoolEdit;
			
class CStatic; class CParsedCtrl;
	class CParsedStatic;
		class CStrStatic;
		class CIntStatic;
		class CLongStatic;
		class CDoubleStatic;
			class CMoneyStatic;
			class CQuantityStatic;
			class CPercStatic;
			class CMetricStatic;
		class CDateStatic;
			class CDateTimeStatic;
			class CTimeStatic;
		class CElapsedTimeStatic;	//@@ElapsedTime
		class CBoolStatic;
		class CEnumStatic;
		class CParsedBitmap;
			class CIntBitmap;
			class CParsedStateImage;
	
class CComboBox; class CParsedCtrl;
	class CParsedCombo;	
		class CStrCombo;
			class CIdentifierCombo;
		class CIntCombo;
		class CLongCombo;
			class CDataObjTypesCombo;
		class CDoubleCombo;
			class CMoneyCombo;
			class CQuantityCombo;
			class CPercCombo;
		class CDateCombo;
		class CBoolCombo;
		class CEnumCombo;

class CListBox; class CParsedCtrl;
	class CParsedListBox;
		class CStrListBox;
		class CIntListBox;
		class CLongListBox;
		class CDoubleListBox;
			class CMoneyListBox;
			class CQuantityListBox;
			class CPercListBox;
		class CDateListBox;
		class CBoolListBox;
		class CEnumListBox;

class CButton; class CParsedCtrl;
	class CExtButton;
		class CParsedButton;
			class CBoolButton;
			class CPushButton;
	
class CStatic; class CParsedCtrl;
	class CParsedGroupBtn;
		class CEnumButton;

//=============================================================================
// useful class definition
class SymTable;
class CLinkButton;
class HotKeyLinkObj;
class CBaseDocument;
class CDateFormatter;
class CDblFormatter;
class CElapsedTimeFormatter;	//@@ElapsedTime
class AddOnApplication;
class CHyperLink;
class ISqlRecord;
class CTBToolBar;
class CTBTabbedToolbar;
class CParsedForm;
class CNumbererRequest;
class CFormatMask;
class CGridControl;
class CHotLinkController;
class ControlLinks;
class HotKeyLink;
//
// Default values for formatting values
//
#define NO_FORMAT		-1		// per inibire la formattazione

#define NO_MORE_SEP		0
#define FIRST_DATE_SEP	1
#define SECOND_DATE_SEP	2
#define DATE_TIME_SEP	3
#define FIRST_TIME_SEP	4
#define SECOND_TIME_SEP	5
#define THIRD_TIME_SEP	6	//@@ElapsedTime
#define FOURTH_TIME_SEP	7	//@@ElapsedTime
#define AMPM_TIME_SEP	8

#define MIN_CTRL_SIZE	1

//
// Default values for double type controls
//
#define DEFAULT_N_DEC		-1
#define DEFAULT_SCALING		10.

// Predefined IDs for associated button
//
#define	BTN_OFFSET		1
#define NO_BUTTON		0x0
#define BTN_DEFAULT		0xFFFF
#define BTN_SPIN_ID		0x7FFF
#define BTN_MENU_ID		0x7FFE
#define BTN_CALENDAR_ID	0x7FFD
#define BTN_COLOR_ID	0x7FFC
#define BTN_OUTLOOK_ID	0x7FFb
#define BTN_EMPTY_ID	ID_EMPTY_BUTTON_UP
#define BTN_DOUBLE_ID	0x7FFA

#define DEFAULT_COMBO_ITEMS		300


// Value checking management
//
// Relationship
//
#define FOREIGN_FOCUSED		1
#define BROTHER_FOCUSED		2
#define RELATIVE_FOCUSED	3
#define PARENT_FOCUSED		4

//
// Error Levels
//
#define	CTRL_IMMEDIATE_NOTIFY			0x0010
#define	CTRL_WARNING_ERROR				0x0001
#define	CTRL_IMMEDIATE_WARNING_ERROR	(CTRL_WARNING_ERROR | CTRL_IMMEDIATE_NOTIFY)
#define	CTRL_FATAL_ERROR				0x0002
#define	CTRL_IMMEDIATE_FATAL_ERROR		(CTRL_FATAL_ERROR | CTRL_IMMEDIATE_NOTIFY)
#define	CTRL_FOCUS_LOSE_REJECTED		0x1000

//
// Gestione espressioni automatiche per TBScheduler
//
#define AUTO_EXPRESSION_SIGNAL	_T("??")
#define DEFAULT_TOOLBAR_HEIGHT	32


//Per la gestione del menù popup del formattatore
//----------------------------------------------------------------------------
struct IndexIDTag
{
	int		nIndex;
	UINT	uiID;
};

extern const IndexIDTag BASED_CODE IDFormatTable[];

//----------------------------------------------------------------------------
enum SubFolderBehaviourOnDropFiles { NO_DEEP, DEEP, UNDEFINED_DEEP }; // comportamento in caso di drop di un folder (anche nel bodyedit)

//=============================================================================
//		Global helpers function
//=============================================================================
TB_EXPORT CParsedCtrl*	GetParsedCtrl			(CObject* pObject);
TB_EXPORT CParsedForm*	GetParsedForm			(CWnd* pWnd);
TB_EXPORT int			GetRelationship			(CWnd*, CWnd*);
TB_EXPORT CParsedForm*	GetParentForm			(CWnd* pWnd);

// Nasconde (o visualizza) tutti i control racchiusi nella groupbox il cui ID viene passato 
// come parametro. Se si vuole evitare che la groupbox stessa venga visualizzata, va disegnata
// con gli attributi visible=false e disabled=true
// NOTA: i control vengono nascosti anche se solo parzialmente racchiusi dalla groupbox
TB_EXPORT void HideControlGroup
	(
		CWnd*	pParentWnd,
		UINT	nGbxID, 
		BOOL	bHide = TRUE
	);

TB_EXPORT void MoveControls			(CWnd*	pParentWnd, CSize offset, ControlLinks* pControlLinks = NULL);
TB_EXPORT BOOL SetControlValue		(ControlLinks* pControlLinks, UINT nIDC, const DataObj& val);

TB_EXPORT void RedrawInnerControls	(UINT	nID, CWnd*	pParentWnd);
TB_EXPORT void RedrawInnerControls	(CWnd*	pGbx, CWnd*	pParentWnd = NULL);

TB_EXPORT BOOL SetAutomaticExpressionAndData
(
	const CString&	strIn,
	CString&		strOut,
	DataObj*		pData,
	BOOL			bRunning
);


////////////////////////////////////////////////////////////////////////////////
//				class ControlLinks definition
////////////////////////////////////////////////////////////////////////////////
//
// Contains all control for the owner View. This class own the controls and
// delete it on destruction to avoid memory leak. Also allow to retrieve the
// control the owns invalidated data (this data are pushed on Invalid array
// by user)
//
//==============================================================================
class TB_EXPORT ControlLinks : public Array
{
	CObArray m_NotifyMap;
	Array	m_StaticsMap;

public:
	ControlLinks();
	static void SetDefaultFocus	(CWnd *pOwnerWnd, HWND* phWndFocus);
	static bool HasFocusableControl(CWnd *pOwnerWnd);

	// overloaded operator helpers
	CWnd*	GetAt		(int nIndex) const	{ return (CWnd*) Array::GetAt(nIndex);}
	CWnd*	operator[]	(int nIndex) const	{ return GetAt(nIndex);}
	CWnd*&	operator[]	(int nIndex)		{ return (CWnd*&) ElementAt(nIndex);}

	void Substitute			(CWnd* pOldWnd, CWnd* pNewWnd);
	void Remove				(CWnd* pWndToDelete);

	//vanno in ricorsione sui Bodyedit
	CParsedCtrl*			GetLinkedParsedCtrl		(UINT nIDC);
	CParsedCtrl*			GetLinkedParsedCtrl		(DataObj* pDataObj);
	CParsedCtrl*			GetLinkedParsedCtrl		(const CTBNamespace& aNS);

	CWnd*					GetWndLinkedCtrl		(UINT nIDC);
	CWnd*					GetWndLinkedCtrl		(const CTBNamespace& aNS);
	CWnd*					GetWndLinkedCtrlByName	(const CString& sName);

	CGridControl*			GetBodyEdits			(int* pnStartIdx = NULL);
	CGridControl*			GetBodyEdits			(const CTBNamespace& aNS);

	BOOL					SetControlValue			(UINT nIDC, const DataObj& val);

	int						FindIndexByHWnd			(HWND h);
	int						FindIndexByIDC			(UINT nIDC);
	void					AlignToTabOrder			(CWnd* pParent);

	void				    AddToNotifyMap		(CWnd* pWnd);
	void				    RemoveToNotifyMap	(CWnd* pWnd);
	void					OnPrepareAuxData		();

	void				    AddToStaticsMap(CWnd* pWnd);
};

//-----------------------------------------------------------------------------
TB_EXPORT CWnd*			GetWndLinkedCtrlByName	(ControlLinks* pControlLinks, const CString& sName);
TB_EXPORT CWnd*			GetWndLinkedCtrl		(ControlLinks* pControlLinks, const CTBNamespace& aNS);
TB_EXPORT CWnd*			GetWndLinkedCtrl		(ControlLinks* pControlLinks, UINT nIDC);
TB_EXPORT CParsedCtrl*	GetLinkedParsedCtrl		(ControlLinks* pControlLinks, UINT nIDC);
TB_EXPORT CParsedCtrl*	GetLinkedParsedCtrl		(ControlLinks* pControlLinks, const CTBNamespace& aNS);
TB_EXPORT CParsedCtrl*	GetLinkedParsedCtrl		(ControlLinks* pControlLinks, DataObj* pDataObj);

//=============================================================================
//			Class ResizableCtrl
//=============================================================================
class TB_EXPORT ResizableCtrl
{
protected:
	int			m_nDirStrech;
	CSize		m_CurSize;
	int			m_nXMargin;
	int			m_nYMargin;
	CSize		m_BornSize;
	BOOL		m_bAlreadyWorking;

private:
	CWnd*		m_pOwnerWnd;

protected:
	ResizableCtrl ();

	virtual	BOOL SkipRecalcCtrlSize();

public:
	CWnd*	GetResizableCWnd	() const { return m_pOwnerWnd; }
	int		GetAutoSizeCtrl		() const { return m_nDirStrech; }
	void	SetAutoSizeCtrl		(int n);
	void	SetAutoSizeMargin	(int nX, int nY)		{ m_nXMargin = nX; m_nYMargin = nY; }

//protected:
	void	SetResizableCurSize	(int nWidth, int nHeight);
	void	InitSizeInfo		(CWnd*);
	BOOL	DoRecalcCtrlSize	();

public:	//TODO RICCARDO 
	CSize		m_csOffset;
	CArray<int, int>	m_arAnchoredCtrl;

	void	Anchor(int nIDC);

private:
	int		CalcMinY();
};



//=============================================================================
//=============================================================================
//			Class CLinkButton
//=============================================================================
class TB_EXPORT CLinkButton : public CButton
{
	DECLARE_DYNAMIC (CLinkButton)
	
protected:
	enum MousePosition { OUTSIDE, UPSIDE, DOWNSIDE };
	enum HKLButton { HKLButton_NO, HKLButton_YES, HKLButton_HIDDEN_DS };

	CWnd*			m_pOwner;
	BOOL			m_bTriState;
	MousePosition	m_WherePressed;
	BOOL			m_bCaptured;
	
	// all bitmaps must be the same size
	CWalkBitmap m_BitmapUp;				// normal image (REQUIRED)
	CWalkBitmap m_BitmapDownSideUp;		// selected up image (REQUIRE)
	CWalkBitmap m_BitmapDownSideDown;	// selected down image (OPTIONAL)
	CWalkBitmap m_BitmapDisabled;		// disabled image (REQUIRED)

	// ALTERNATE browser bitmaps 
	HKLButton	m_IsHKL;
	
public:
	CLinkButton(CWnd*);

	BOOL LoadBitmaps(UINT nBitmapResourceID);

	// Operations
	void SizeToContent();
	MousePosition WhereDown(CPoint ptMousePos);

	BOOL IsHKL()		{ return m_IsHKL == HKLButton_YES; }
	BOOL IsHiddenHKL()	{ return m_IsHKL == HKLButton_HIDDEN_DS; }

	void SetHKL()		{ m_IsHKL = HKLButton_YES; }
	void SetHiddenHKL()	{ m_IsHKL = HKLButton_HIDDEN_DS; }
	void DoCLick(BOOL bLower);
protected:
	virtual void DrawItem(LPDRAWITEMSTRUCT lpDIS);

	//{{AFX_MSG(CLinkButton)
	afx_msg void OnLButtonUp 	(UINT nFlags, CPoint ptMousePos);
	afx_msg void OnLButtonDown	(UINT nFlags, CPoint ptMousePos);
	afx_msg	void OnMouseMove	(UINT nFlags, CPoint ptMousePos);

	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CStateButton
//=============================================================================

class TB_EXPORT CStateButton : public CButton
{
	DECLARE_DYNAMIC (CStateButton)
	
protected:
	CWnd*			m_pOwner;
	CParsedCtrl*	m_pParsedCtrl;

private:
	BOOL	m_bCaptured;
	int		m_nCurrentState;
	Array	m_StateBitmaps;		// array di bitmap dedicati a coppie con
								// posizione 1= normale 2=disabilitato (REQUIRED)
	int		m_nBmpWitdh;
	int		m_BitmapID;
public:
	CStateButton (CWnd* pWndOwner, CParsedCtrl* pPCtrlOwner = NULL);
	virtual ~CStateButton() {}

public:
	int GetBitmapID()		{ return m_BitmapID; }
	void SetStateBitmaps	(int nState, UINT nBmpNormal, UINT nBmpDisabled);
	void SetStateBitmaps	(int nState, UINT nBmpNormal);
	void SetStateBitmaps	(int nState, CString strImageNS);
	void SetCurrentState	(int nState);
	void ClearAllBitmaps	();

	void SizeToContent ();

	int	 GetButtonWidth			();
	int	 GetButtonCurrentState	() {return m_nCurrentState;}	

	CParsedCtrl* GetParsedControl() {return m_pParsedCtrl;}
	/// Returns the internal member m_pOwner, i.e. the 
	/// cntrol owner of the current state button.
	CWnd* GetParsedControlOwner() {return m_pOwner;}


protected:
	void DoContextMenu (CPoint ptMousePos);
	void LoadStateBitmaps(int nState, UINT nBmpNormal, UINT nBmpDisabled = 0, CString strImageNS = _T(""));

private:
	CWalkBitmap* GetCurrentStateBmp (BOOL bDisableBmp = FALSE);


protected:
	virtual void DrawItem (LPDRAWITEMSTRUCT lpDIS);
	virtual	BOOL	OnCommand		(WPARAM wParam, LPARAM lParam);
	
	//{{AFX_MSG(CStateButton)
	afx_msg void OnLButtonUp 	(UINT nFlags, CPoint ptMousePos);
	afx_msg void OnLButtonDown	(UINT nFlags, CPoint ptMousePos);
	afx_msg void OnRButtonDown	(UINT nFlags, CPoint ptMousePos);
	afx_msg	void OnMouseMove	(UINT nFlags, CPoint ptMousePos);
	afx_msg LRESULT OnGetControlDescription(WPARAM, LPARAM);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CMenuButton
//=============================================================================
class TB_EXPORT CMenuButton : public CStateButton
{
	DECLARE_DYNAMIC (CMenuButton)

public:
	CMenuButton (CWnd* pWndOwner, CParsedCtrl* pCtrlOwner);
	void OpenContextMenu();
protected:
	//{{AFX_MSG(CMenuButton)
	afx_msg void OnLButtonDown	(UINT nFlags, CPoint ptMousePos);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

class CMonthCalCtrlExWnd;
//=============================================================================
//			Class CCalendarButton
//=============================================================================
class TB_EXPORT CCalendarButton : public CStateButton
{
	DECLARE_DYNAMIC (CCalendarButton)

public:
	ManagedWindowWrapper*	m_pManagedWndWrapper;	//puntatore al gestore della finestra di selezione di range di date

public:
	CCalendarButton (CWnd* pWndOwner, CParsedCtrl* pCtrlOwner);
	~CCalendarButton();
public:
	BOOL				IsDestroyCalendarEnabled();

protected:
	CWnd* m_pCalWnd;
	Scheduler			 m_BatchScheduler;

	//{{AFX_MSG(CMenuButton)
	afx_msg void OnLButtonDown	(UINT nFlags, CPoint ptMousePos);
	afx_msg void OnRButtonDown	(UINT nFlags, CPoint ptMousePos);
	afx_msg LRESULT OnGetControlDescription(WPARAM, LPARAM);
	//}}AFX_MSG

	//BOOL OnCommand(WPARAM wParam, LPARAM lParam);
	//BOOL OnNotify(WPARAM wParam, LPARAM lParam, LRESULT* pResult);
	//void CreateCalendar();
	//LONG OnDestroyCalendar(WPARAM wParam = 0, LPARAM lParam = 0);

	DECLARE_MESSAGE_MAP()
};



//=============================================================================
//			Class COutlookButton
//=============================================================================
class TB_EXPORT COutlookButton : public CStateButton
{
	DECLARE_DYNAMIC (COutlookButton)

public:
	COutlookButton (CWnd* pWndOwner, CParsedCtrl* pCtrlOwner);

protected:
	//{{AFX_MSG(COutlookButton)
	afx_msg void OnLButtonDown	(UINT nFlags, CPoint ptMousePos);
	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
	void ShowAddressLists();
	

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CColorButton
//=============================================================================
class TB_EXPORT CColorButton : public CStateButton
{
	DECLARE_DYNAMIC (CColorButton)

public:
	CColorButton (CWnd* pWndOwner, CParsedCtrl* pCtrlOwner);

protected:
	//{{AFX_MSG(CColorButton)
	afx_msg void OnLButtonDown	(UINT nFlags, CPoint ptMousePos);
	//}}AFX_MSG
	void ShowColorDlg();

	DECLARE_MESSAGE_MAP()
};

///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTooltipProperties
{
public:
	UINT		m_nControlID;

	CString		m_strText;
	CString		m_strTitle;
	int			m_nWidth; 
	COLORREF	m_clrText; 
	COLORREF	m_clrBackgnd;
	int			m_nIcon;
	TOOLINFO	m_ti;

	CTooltipProperties()
		:
		m_nControlID(0),
		m_nWidth(0), 
		m_clrText(AfxGetThemeManager()->GetTooltipForeColor()), 
		m_clrBackgnd(AfxGetThemeManager()->GetTooltipBkgColor()),
		m_nIcon(0)
	{}
};

//=============================================================================
//			Class CColoredControl
//=============================================================================
class TB_EXPORT CColoredControl 
{
	friend class CCustomStatic;
protected:
	CWnd*		m_pWnd; //owner control

	BOOL		m_bColored;

	COLORREF	m_crText;
	COLORREF	m_crBkgnd;
	CBrush*		m_pBrush;

public:
	// Construction
	CColoredControl(CWnd*);
	virtual ~CColoredControl();

protected:
	HBRUSH		CtlColor(CDC* pDC, UINT nCtlColor);
	BOOL		EraseBkgnd(CDC* pDC);

public:
	void		SetColored		(BOOL bColored,		BOOL bRedraw = FALSE);
	BOOL		IsColored		() const { return m_bColored; }

	COLORREF	GetTextColor	() const { return  m_crText; }
	void		SetTextColor	(COLORREF crText,	BOOL bRedraw = FALSE, BOOL bColored = TRUE);

	COLORREF	GetBkgColor		() const { return  m_crBkgnd; }
	void		SetBkgColor		(COLORREF crBkg,	BOOL bRedraw = FALSE, BOOL bColored = TRUE);

	HBRUSH		GetBkgBrushHandle	();
	CBrush*		GetBkgBrush			();

	void		PopulateColorDescription(CWndColoredObjDescription* pDesc);

protected:
	BOOL		m_bCustomDraw;

	COLORREF	m_crLine;		//for "Label --------------" with sep line, GroupBox border, full line separator
	INT			m_nSizeLinePen;

public:
	void	SetCustomDraw			(BOOL b) { m_bCustomDraw = b; }
	BOOL	IsCustomDraw			() const { return m_bCustomDraw; }
protected:      
	void		SetLineColor		(COLORREF crBorder, int nSizePen = 1);
	COLORREF	GetLineColor		() const { return  m_crLine; }

	//void	DoStaticPaint			();

	BOOL	DoStaticEraseBkgnd		(CDC*);

	void	DrawHLine(CDC& dc, int y, int left, int right, COLORREF cr, BOOL bHighlight);
	void	DrawVLine(CDC& dc, int x, int top, int bottom, COLORREF cr, BOOL bHighlight, BOOL bLeft = TRUE);
};


//=============================================================================
//			Class CCustomFont
//=============================================================================
class TB_EXPORT CCustomFont
{
	friend class CParsedCtrl;
protected:
	CWnd*	m_pWnd; //owner control
	CFont*	m_pOwnFont;
	BOOL	m_bOwnsFont;

public:
	// Construction
	CCustomFont(CWnd*);
	virtual ~CCustomFont();

public:
	virtual void SetWndCtrlFont(CFont*, BOOL bRedraw = TRUE);

	CFont*	GetWndPreferredFont() const;
	CFont*	GetOwnFont() const { return m_pOwnFont; }

	virtual	void	SetOwnFont(BOOL bBold, BOOL bItalic, BOOL bUnderline, int nPointSize = 0, LPCTSTR lpszFaceName = NULL);
	void	SetOwnFont(LOGFONT& pLogFont);
	virtual void	SetOwnFont(CFont* pFont, BOOL bOwns = FALSE);

	void	PopulateFontDescription(CWndObjDescription* pDesc);

};
//=============================================================================
//			Class CControlLabel
//=============================================================================
class TB_EXPORT CControlLabel : public CStatic, CCustomFont
{
	DECLARE_DYNCREATE(CControlLabel);

public:
	CControlLabel();
};

class CStateCtrlObj;
class CParsedCtrlEvents;
class CParsedCtrlDropTarget;
class CControlBehaviour;
class IItemSource;
class IDataAdapter;
class IValidator;
//=============================================================================
//			Class CParsedCtrl
//=============================================================================
class TB_EXPORT CParsedCtrl : public CAbstractCtrl, public IBehaviourContext, public IBehaviourConsumer, public IOSLObjectManager
{
	friend class CLinkButton;
	friend class CStateButton;
	friend class CParsedForm;
	friend class CStateCtrlObj;
	friend class CHyperLink;
	friend class HotKeyLinkObj;
	friend class CParsedCtrlEvents;
	friend class CJsonFormEngine;

public:
	enum CaptionPosition
	{
		Upper = 0,
		Left,
		Right
	};

	enum MessageID {
			EMPTY_MESSAGE = 0,
			FIELD_REDEFINED,
			FIELD_NOT_FOUND,
			FIELD_EMPTY,
			STR_EDIT_EMPTY,
			INT_EDIT_OUT_RANGE,
			LONG_EDIT_OUT_RANGE,
			DOUBLE_EDIT_OUT_RANGE,
			DATE_EDIT_BAD_FORMAT,
			DATE_EDIT_DEMO,
			DATE_EDIT_OUT_RANGE,
			PATH_EDIT_BAD_PATH,
			PATH_EDIT_BAD_FILE,
			PATH_EDIT_EMPTY_PATH,
			PATH_EDIT_EMPTY_FILE,
			PATH_EDIT_NO_PATH,
			PATH_EDIT_NO_FILE,
			TIME_EDIT_BAD_FORMAT,
			TIME_EDIT_OUT_RANGE,
			GENERIC_BAD_VALUE,
			HOTLINK_DATA_NOT_FOUND,
			HOTLINK_RECORD_NOT_FOUND,
			HOTLINK_IS_RUNNING,
			MAX_ITEM_REACHED,
			NAMESPACE_EDIT_BAD_NAMESPACE,
			HOTLINK_DATA_PROTECTED, //used for TBRowSecurity (impr 5185 @@BAUZI)
			// inizio item a disposizione per le eventuali classi derivate
			AVAILABLE_1,
			AVAILABLE_2,
			AVAILABLE_3,
			AVAILABLE_4,
			AVAILABLE_5,
			AVAILABLE_6,
			AVAILABLE_7,
			AVAILABLE_8,
			AVAILABLE_9,
			AVAILABLE_10,
			AVAILABLE_11,
			AVAILABLE_12,
			AVAILABLE_13,
			AVAILABLE_14,
			AVAILABLE_15,
			AVAILABLE_16,
			AVAILABLE_17,
			AVAILABLE_18,
			AVAILABLE_19,
			AVAILABLE_20,
			// fine item a disposizione per le eventuali classi derivate
			USER_STRING_ERROR_MESSAGE = 0xFFFF,
			DUMMY = 0xFFFFFFFF
	};

public:
	CTBNamespace*	m_pFormatContext;
	SqlRecord*		m_pSqlRecord; //ho dei casi in cui mi serve sapere l'eventuale SqlRecord di appartenenza del DataObj linkato al control
	
protected:
	CControlBehaviour* m_pControlBehaviour = NULL;
	IItemSource* m_pItemSource = NULL;
	IDataAdapter* m_pDataAdapter = NULL;
	CArray<IValidator*> m_Validators;
public:
	
	DataObj*		m_pData;
	CWnd*			m_pOwnerWnd;
	CWndObjDescription*	m_pOwnerWndDescription;
	BOOL			m_bModifyFlag;
	DataObj*		m_pOldData;
	CBaseDocument*	m_pDocument;
	HotKeyLinkObj*	m_pHotKeyLink;    
	Array			m_StateCtrls;
	int				m_nButtonsXOffset;
	int				m_nLastStateCtrlChanged;
	CWnd*			m_pButton;	//BTN_SPIN_ID - CSpin deriva da CWnd
	CHyperLink*		m_pHyperLink;
	UINT			m_nButtonIDBmp;
	UINT			m_nButtonID;
	MessageID		m_nErrorID;
	MessageID		m_nWarningID;
	UINT			m_nMsgBoxStyle;
	BOOL			m_bAttached;
	BOOL			m_bOwnHotKeyLink;
	int				m_nFormatIdx;
	Formatter*		m_pFormatter;
	Formatter*		m_pPrivacyFormatter; //serve per visualizzare un dato privato (vedi RowSecurity e OSL) che l'utente corrente non può visualizzare
	int				m_nValueChanging;
	UINT			m_nCtrlLimit;
	DWORD			m_dwCtrlStyle;
	BOOL			m_bUserRunning;
	CString			m_strAutomaticExpression;
	CControlLabel*	m_pCaption;
	Token			m_nCaptionPos;
	CString			m_strUserCaption;
	int				m_nUseSpecialCaption;
	//per la gestione dei comandi associati al menù popup della formattazione
	CStringArray	m_FmtCmdArray;
	int				m_nNewFormatIdx;

	CNumbererRequest*	m_pNumbererRequest;
	WORD				m_wKeyState;

	// DragDrop
	CParsedCtrlDropTarget*	m_pDropTarget;
	
	CArray<CWnd*>			m_arrySpaceForButtonReserved;

	BOOL					m_bShowErrorBox;

protected:
	CString				m_strToolTipBuffer;
	CString				m_strMessage;
	CParsedCtrlEvents*	m_pEvents;

	CHotLinkController*	m_pHotLinkController;

public:
	// Constructors
	CParsedCtrl(DataObj* pData = NULL);
	virtual ~CParsedCtrl();
	
public:
	void			Attach		(CWnd*);
	virtual void	Attach		(UINT nBtnID);

	void	AttachDocument		(CBaseDocument* pDoc);
	void	AttachFormatter		(int nIdx)						{ m_nFormatIdx = nIdx; }
	void	AttachFormatter		(Formatter* pFmt)				{ m_pFormatter = pFmt; }
	void    AttachRecord		(SqlRecord* pRecord);

	void	AttachHotKeyLink	(HotKeyLinkObj*, BOOL bOwned = FALSE);
	void	ReattachHotKeyLink	(HotKeyLinkObj*, BOOL bOwned = FALSE, BOOL bCopyAttributes = TRUE);
	void	DetachHotKeyLink	(HotKeyLinkObj*, BOOL bOwned = FALSE);
	void	DetachHotKeyLink	();
	void	DetachAllStateData	();

	CBaseDocument*	GetDocument			()	const { return m_pDocument;		}

	UINT			GetButtonIDBmp		()  const { return m_nButtonIDBmp;  }

	HotKeyLinkObj*		GetHotLink			()	const { return m_pHotKeyLink;	}
	CHyperLink*			GetHyperLink		()	const { return m_pHyperLink;	}
	CHotLinkController*	GetHotLinkController()	const { return m_pHotLinkController;	}
	void				EnableHotLinkController(const BOOL bEnable = TRUE, const CString& sDecodeNs = _T(""));

	void SetControlBehaviour(CControlBehaviour* pControlBehaviour);
	CControlBehaviour* GetControlBehaviour() { return m_pControlBehaviour; }
	virtual void SetItemSource(IItemSource* pItemSource);
	IItemSource* GetItemSource() { return m_pItemSource; }
	void SetDataAdapter(IDataAdapter* pDataAdapter) { m_pDataAdapter = pDataAdapter; }
	IDataAdapter* GetDataAdapter() { return m_pDataAdapter; }
	void AddValidator(IValidator* pValidator);
	BOOL IsValidForAllValidators();
		
	Array&          GetStateCtrlsArray	()		  { return m_StateCtrls;	}
	CStateCtrlObj*	GetStateCtrl		(DataObj* pDataObj)	const;
	CStateCtrlObj*	GetLastStateCtrlChanged () const;
	CParsedCtrlEvents*		GetParsedCtrlEvents();

	void			SetButtonsXOffset	(int nXOff) { m_nButtonsXOffset = nXOff; }
	int				GetButtonsXOffset	() const	{ return m_nButtonsXOffset; }

	DataObj*	GetOldCtrlData		()		const { return m_pOldData;		}
	void		SaveOldCtrlData		();
	void		RestoreOldCtrlData	(BOOL bShow = FALSE);

	int					GetFormatIdx		() const	{ return m_nFormatIdx ;	}
	const Formatter*	GetCurrentFormatter () const;

	BOOL	SetCaption			(Token nPos = T_DEFAULT, LPCTSTR pszCaption = NULL, int nUseSpecial = -1);
	BOOL	SetSpecialCaption	(UINT nIDCapion);

	void	SetCtrlStyle		(DWORD dwStyle)			{ m_dwCtrlStyle = dwStyle; }
	DWORD	GetCtrlStyle		()				const	{ return m_dwCtrlStyle; }

	virtual void	SetCtrlCaption(const CString& strCaption);
	virtual void	SetCtrlCaption(const CString& strCaption, VerticalAlignment vAlign, CaptionPosition ePosition, int nCaptionWidth);
			void	SetCtrlCaption		(const CString& strCaption, TextAlignment hAlign, VerticalAlignment vAlign, CaptionPosition ePosition, int nCaptionWidth, BOOL bSetPosition = TRUE);
	CString	GetCtrlCaption		();
	void	SetCtrlLabelDefaultPosition(TextAlignment hAlign, VerticalAlignment vAlign, CaptionPosition ePosition, int nCaptionWidth);
	
	BOOL	IsAutomaticExpression ()	const	{ return (m_dwCtrlStyle & CTRL_STYLE_STORED_AUTO_EXPRESSION) == CTRL_STYLE_STORED_AUTO_EXPRESSION; }

	CWnd*	GetCtrlCWnd			()	const { ASSERT(AfxIsRemoteInterface() || m_pOwnerWnd); return m_pOwnerWnd; }
	int		GetCtrlID()	const;
	CWnd*	GetCtrlParent		()	const { ASSERT(m_pOwnerWnd); return m_pOwnerWnd->GetParent(); }
	
	LRESULT	SendMessageToAncestor(BOOL bSend, UINT message, WPARAM wParam = 0, LPARAM lParam = 0) const;

	int		GetDataSize			(UINT nCols);
	void	SetCtrlID			(const UINT nIDC)	const { ASSERT(m_pOwnerWnd); m_pOwnerWnd->SetDlgCtrlID(nIDC); }

	void	SetUserRunning		(BOOL bRunning = TRUE)	{ m_bUserRunning = bRunning; }
	void	SetErrorID			(MessageID nErrID)			{ m_nErrorID = nErrID; }
	void	SetWarningID		(MessageID nWarnID, BOOL bNoCanc = FALSE)
												{
													m_nWarningID = nWarnID;
													m_nMsgBoxStyle = bNoCanc
														? MB_ICONINFORMATION | MB_OK
														: MB_ICONEXCLAMATION | MB_OKCANCEL;
												}

	MessageID	GetWarningID	()				const	{ return m_nWarningID; }
	MessageID	GetErrorID		()				const	{ return m_nErrorID; }
	BOOL		CanResetStatus	()				const	{ return !m_nValueChanging; }
	CControlLabel* GetControlLabel()			const	{ return m_pCaption; }
	
	CButton*	GetButton		();
	
	void	SetError			(const CString& strError)	
												{
													if(strError.IsEmpty()) return;

													m_strMessage = strError; 
													SetErrorID(USER_STRING_ERROR_MESSAGE); 
												}
	
	void	SetWarning			(const CString& strWarning, BOOL bNoCanc = FALSE)
												{
													if(strWarning.IsEmpty()) return;

													m_strMessage = strWarning; 
													SetWarningID(USER_STRING_ERROR_MESSAGE, bNoCanc); 
												}
	CString	GetMessage			()				{ return m_strMessage; }

	static CString FormatMessage		(MessageID ID);

	void	UpdateStateButtons			(int nCmdShow = SW_SHOW);

	void	ReserveSpaceForButton		(UINT nWidthBtn, CButton* pButton);
	void	AddToSpaceReservedArray(CWnd* pButton);
	void	RemoveFromSpaceReservedArray(CWnd* pButton);
	int		GetAllButtonsWitdh			();

	BOOL	ShowZerosInInput			();

	BOOL	GetOwnHotKeyLink			() const { return m_bOwnHotKeyLink; }
	void	SetOwnHotKeyLink			(BOOL bSet = TRUE) { m_bOwnHotKeyLink = bSet; }

	void	DoCellPosChanging			(CRect& rectEdit, UINT nFlags);
	void	SetHyperLinkPos				(CRect&, UINT);
	void	SetToolTipBuffer			(const CString& strTooltip);
	void	SetNamespace				(const CString& strName);
	void	ReAttachButton				(UINT nBmpID);
	BOOL	IsFindMode					() const;
	void	AdjustButtonsVisualization	();
	void	AdjustButtonsVisualizationUpdateControls();

	void	EnableDrop					(BOOL bEnable = TRUE, CParsedCtrlDropTarget* pDropTarget = NULL);

public:
	virtual void	FindHotLink();
	virtual BOOL	CheckDataObjType(const DataObj* pDataObj = NULL);
	// pure virtual function
	virtual	BOOL		Create			(DWORD, const RECT&, CWnd*, UINT)	= 0;
	virtual	BOOL		SubclassEdit	(UINT, CWnd*, const CString& strName = _T(""))	= 0;
	virtual	HWND		UnSubclassEdit	() { return m_pOwnerWnd ? m_pOwnerWnd->UnsubclassWindow() : NULL; }

	virtual DataType	GetDataType		()					const	= 0;

	virtual void		AttachRecordToStateData (ISqlRecord* pRecord);
	virtual void		OnAttachStateData (DataObj* pData, BOOL bInvertDefaultStates = FALSE);
	virtual CStateCtrlObj* AttachStateData	 (DataObj* pData, BOOL bInvertDefaultStates = FALSE);
	virtual CStateCtrlObj* AttachStateCmd	 (const CString& sNs);

			void		DetachStateData	 (DataObj* pData);

	virtual	DataObj*	GetMinValue		() { return NULL; }
	virtual	DataObj*	GetMaxValue		() { return NULL; }
	virtual	void		SetMinValue		(const DataObj& value) {};
	virtual	void		SetMaxValue		(const DataObj& value) {};

	virtual IBehaviourContext*	GetContext		();
	CFormatMask*				GetFormatMask	();
	virtual CStateCtrlObj*		AttachNumberer	(CRuntimeClass* pRequestClass);
	virtual CStateCtrlObj*		AttachNumberer	(const CString sEntity = _T(""), BOOL hasButton = TRUE, BOOL hasFormatter = TRUE, CRuntimeClass* pRequestClass = NULL);
	virtual CStateCtrlObj*		AttachNumberer	(CNumbererRequest* pRequestClass, const CString sEntity = _T(""), BOOL hasButton = TRUE, BOOL hasFormatter = TRUE);

public:
	// virtual members
	virtual	CString		GetSpecialCaption	() { return _T(""); }

	virtual void		Attach		(DataObj*);

	virtual	DataObj*		GetCtrlData		()	{ return m_pData; }
	virtual CString			GetPublicName	()	{ return GetInfoOSL()->m_Namespace.ToString(); }
	virtual CString			GetCtrlClass();
	virtual CString			GetCtrlName()	{ return GetInfoOSL()->m_Namespace.GetObjectName(); }
	virtual CTBNamespace&	GetNamespace()  { return GetInfoOSL()->m_Namespace; }
	
	virtual	CString		GetAutomaticExpression	()	const;
	virtual	BOOL		SetAutomaticExpression	(const CString&);

	virtual	BOOL	ErrorMessage	();
	virtual void	BadInput		();

	virtual	CString	FormatErrorMessage	(CParsedCtrl::MessageID nIDP, LPCTSTR pszBadVal);
	virtual	CString	FormatData			(const DataObj* pDataObj, BOOL bEnablePadding = FALSE) const;
	
	virtual	BOOL	UpdateCtrlData		(BOOL bEmitError, BOOL bSendMessage = FALSE);
	virtual void	UpdateCtrlView		();
	virtual	void	UpdateCtrlStatus	();
	virtual	BOOL	ForceUpdateCtrlView	(int = -1);
	virtual	void	OnUpdateCtrlStatus	(int = -1);
	virtual void	ModifiedCtrlData	();
	virtual BOOL	EnableCtrl			(BOOL bEnable = TRUE);
	virtual BOOL	ShowCtrl			(int nCmdShow);
	virtual void	ClearCtrl			();

	virtual CWnd*	SetCtrlFocus	(BOOL bSetSel = FALSE);
	virtual	BOOL	SetCtrlPos		(const CWnd* pWndInsertAfter, int x, int y, int cx, int cy, UINT nFlags);
	virtual	CSize	SetCtrlSize		(UINT nCols, UINT nRows, BOOL bButtonsIncluded);
	virtual	void	SetCtrlSel		(int, int);
	virtual	void	GetCtrlSel		(int&, int&) { /* do nothing */}

	virtual	void	SetCtrlMaxLen	(UINT nLen, BOOL bApplyNow = TRUE);
	virtual	UINT	GetCtrlMaxLen	() const { return m_nCtrlLimit; }
	virtual	void	SetCtrlNumDec	(int)	{ /* do nothing */ }
	virtual	int		GetCtrlNumDec	()		{ return 0; /* do nothing */ }

	virtual	void	SetModifyFlag(BOOL bFlag)	{ m_bModifyFlag = bFlag; }
	virtual	BOOL	GetModifyFlag()				{ return m_bModifyFlag; }

	virtual BOOL	IsValid			();
	virtual	BOOL	IsValid			(const DataObj& aValue);

	virtual	void	DoCommand			(WPARAM wParam, LPARAM lParam);
	virtual	BOOL	PreProcessMessage	(MSG* pMsg);
	virtual void	DoSpinScroll		(UINT nSBCode);
	virtual	void	DoEnable			(BOOL bEnable);
	virtual	void	DoKillFocus 		(CWnd* aWnd);
			void	DoSetFocus			(CWnd* aWnd);
	virtual	BOOL	DoKeyUp				(UINT nChar);
	virtual	BOOL	DoKeyDown			(UINT nChar);
	virtual	BOOL	DoOnChar			(UINT nChar);
	virtual	void	DoPushButtonCtrl	(WPARAM wParam, LPARAM lParam);	
	/// Method for handling click on attached buttons (StateButton, HKL, ecc),
	/// i.e. it is a shortcut to DoPushButtonCtrl functionalities handy for
	/// WebLook and its client server-interaction.
	virtual	void	ExcecPushButtonCtrl	(WPARAM wParam, LPARAM lParam);
	virtual	void	DoCallLink			();
	virtual	BOOL	DoRButtonDown		(UINT nFlag, CPoint ptPoint);
	virtual	BOOL	DoContextMenu		(CWnd* pWnd, CPoint ptPoint);
	virtual	void	DoEditAutomaticExpression ();
	virtual	void	DoBehavior			();
	virtual	void	DoShowEnumValue		() { };
	virtual void	AssignAutomaticExpression(const CString& strExpr);

	//da reimplementare per gestire MenuButton
	virtual	BOOL	GetMenuButton		(CMenu*)	{ /* do nothing */ return FALSE; }
	virtual	CMenu*	GetMenu				()			{ /* do nothing */ return NULL; }

	virtual	void	DoCmdMenuButton		(UINT)		{ /* do nothing */ };

	virtual BOOL	GetToolTipProperties(CTooltipProperties&);

	virtual	void	SetDataModified		(BOOL bMod)			{ if (m_pData) m_pData->SetModified(bMod); }
	virtual	void	SetDataReadOnly		(BOOL bRO)			{ if (m_pData) m_pData->SetReadOnly(bRO); }
	virtual	void	SetDataOSLReadOnly	(BOOL bVal = TRUE)	{ if (m_pData) m_pData->SetOSLReadOnly(bVal); }
	virtual	void	SetDataOSLHide		(BOOL bVal = TRUE)	{ if (m_pData) m_pData->SetOSLHide(bVal); }

	virtual	BOOL	IsDataModified		()	{ return m_pData && m_pData->IsModified(); }
	virtual	BOOL	IsDataOSLReadOnly	()	{ return m_pData && m_pData->IsOSLReadOnly(); }
	virtual	BOOL	IsDataOSLHide		()	{ return m_pData && m_pData->IsOSLHide(); }

	virtual	int		GetOutputCharLen()	const;
	virtual	int		GetInputCharLen	()	const;
	virtual	int		GetInputWidth	()	const;

	virtual void	SetEditReadOnly	(const BOOL bValue);
	virtual BOOL	IsEditReadOnly	() const;

	virtual	void	SetRange	(int nMin, int nMax)						{ ASSERT(FALSE); }
	virtual	void	SetRange	(double nMin, double nMax, int nDec = 0)	{ ASSERT(FALSE); }

	virtual void	UpdateViewModel(BOOL bParentIsVisible);
	virtual void	ReadPropertiesFromJson();
protected:
	virtual int		GetMenuButtonImage			();
	virtual CString GetMenuButtonImageNS		();
	virtual void	OnCreateAssociatedButton	()			{ /* do nothing */ }
	virtual BOOL	OnShowingPopupMenu			(CMenu&);

	virtual	BOOL	OnInitCtrl		();

public:
	
	virtual	void	SetValue	(const DataObj& aValue) = 0;
	virtual	void	GetValue	(DataObj& aValue) = 0;
	virtual	void	GetValue	(CString&	strValue);
	virtual	void	SetValue	(LPCTSTR	strValue);

	virtual	CSize	AdaptNewSize	(UINT, UINT, BOOL bButtonsIncluded);

	virtual	void	OnHotLinkClosed				();

	virtual BOOL	OwnerDraw		(CDC*, CRect&, DataObj* = NULL) { return FALSE; } //per gestire ownerdraw delegato al parsed control di una cella del bodyedit
	virtual BOOL	HasFocus		() { return m_pOwnerWnd == CWnd::GetFocus(); }

	virtual	void	BindParam			(DataObj*, int=-1);
	virtual	int		GetParamDataIdx		() const { return -1; }

	virtual BOOL	UseEasyReading	()	const;

			void	NotifyToParent	(UINT, BOOL bSend = TRUE); //solo per uso interno
	virtual void	OnPrepareAuxData();

	// DragDrop
	virtual void	OnDropFiles		(CStringArray* pDroppedFiles) {};	// passa un array con elenco files droppato nel ctrl
	virtual BOOL	OnSubFolderFound() { return TRUE;  };				// notifica la presenza del primo subfolder trovato nel folder droppato nel ctrl

	int		GetButtonWidth			(int nExpectedWidth = 0);
protected:
	void UpdateCtrlVisibility();
	// NON user overridables
	BOOL	CanDoCallLink			() const;
	BOOL	CheckControl			(UINT nID, CWnd* pParentWnd, LPCTSTR ctrlClassName = NULL);
	BOOL	InitCtrl				();
	BOOL	CreateAssociatedButton	(CWnd*);
	void	SetButtonPos			(CRect&, UINT);
	BOOL	IsAssociatedButton		(CWnd*);
	BOOL	IsStrChar				(UINT nChar, DWORD dwStyle);
	BOOL	HasBeenInvalidated		(BOOL bCheckDataStatus = TRUE);
	BOOL	ManageNumericBackKey	(
										const CString& strValue,
										DWORD& dwPos, int& nPos,
										TCHAR ch1000Sep, TCHAR chDecSep = 0
									);
	int		UpdateNumericString		(
										double nVal,	CString& strValue,
										DWORD dwPos,	int& nPos,
										UINT nChar,		TCHAR ch1000Sep
									);
	void	UpdateNumericInput		(
										double nVal,	CString& strValue,
										DWORD dwPos,	int nPos,
										TCHAR ch1000Sep,	int nCurNr1000Sep,
										CDblFormatter* = NULL
									);
	int		FindDateSepPos			(int nSepIdx, const CString& strIn, CDateFormatter*);
	int		DateSepPermitted		(const CString& strIn, CDateFormatter*, CString& strOut);
	int		TimeSepPermitted		(const CString& strIn, CElapsedTimeFormatter*, CString& strOut);	//@@ElapsedTime
	CString GetDateTimeTemplate		()	const;
	CString GetElapsedTimeTemplate	()	const;	//@@ElapsedTime
	UINT	CreateFormatPopupMenu	(CMenu& menuFormat);
	void	SetNewFormatIdx			();
	void	GridCellPosChanging		(WINDOWPOS* wndPos);

	CExternalControllerInfo::ControllingMode GetControllerMode() const;

	void DoMaskedGetValue(CString& aValue, DataObj& aDataObj);

private:
	void ToggleFormatMask	();
	CJsonContextObj* GetJsonContext();
public:
	CCustomFont*	m_pCustomFont;
	BOOL AttachCustomFont (CCustomFont* pcf) { m_pCustomFont = pcf; return TRUE; }
	CFont*	GetPreferredFont () const;
	virtual void SetCtrlFont (CFont*, BOOL bRedraw = TRUE);
	CFont*	GetCustomOwnFont () const { return m_pCustomFont ? m_pCustomFont->GetOwnFont() : NULL; }

	virtual BOOL IsShowDescription	() const  { return FALSE; }
	int m_nUseComponentToFormat;
	TCHAR m_cPadChar;

	BEGIN_TB_STRING_MAP(Strings)
		TB_LOCALIZED(FIRST, "FIRST")
		TB_LOCALIZED(LAST, "LAST")
	END_TB_STRING_MAP()
};

//=============================================================================
//			Class CParsedCtrlEvents
//=============================================================================
class TB_EXPORT CParsedCtrlEvents : public CDataEventsObj
{
	CParsedCtrl* m_pControl;

public:
	CParsedCtrlEvents(CParsedCtrl* pControl) 
		: m_pControl (pControl)
	{}
	virtual void Fire(CObservable* pSender, EventType eType);
	virtual CObserverContext* GetContext() const;
	virtual void OnDeletingObservable(CObservable* pSender);

	CParsedCtrl* GetControl() { return m_pControl; }
};

//=============================================================================
//			Class CGridControlCellPos
//=============================================================================
class TB_EXPORT CGridControlCellPos
{
public:
	int		m_nStartRowIdx;		// current first body row
	int		m_nStartColIdx;		// current first body column
	int		m_nCurrRowIdx;		// current active body row
	int		m_nCurrColIdx;		// current active column
	int		m_nCurrLinesPerRow;	// current # lines per row

	CGridControlCellPos()
		:
		m_nStartRowIdx		(0),
		m_nStartColIdx		(0),
		m_nCurrRowIdx		(-1),
		m_nCurrColIdx		(-1),
		m_nCurrLinesPerRow	(0)
	{}

	CGridControlCellPos (const CGridControlCellPos& cp) { Assign(cp); }

	void Assign (const CGridControlCellPos& cp)
	{
		m_nStartRowIdx		= cp.m_nStartRowIdx;
		m_nStartColIdx		= cp.m_nStartColIdx;
		m_nCurrRowIdx		= cp.m_nCurrRowIdx;
		m_nCurrColIdx		= cp.m_nCurrColIdx;
		m_nCurrLinesPerRow	= cp.m_nCurrLinesPerRow;
	}
	
	CGridControlCellPos& operator=(const CGridControlCellPos& cp) 
	{ 
		if (this != &cp) Assign(cp);
		return *this;
	}
};

// oggetto che mi mantiene lo stato impostato dal programmatore
//=============================================================================
class TB_EXPORT CStateCtrlState : public CObject
{
private:
	UINT	m_nBmpNormalId;
	UINT	m_nBmpDisabledId;

	CString m_imageNS;
	CString m_imageNSDisabled;

	BOOL	m_bEnableCtrl;

public:
	CStateCtrlState (const CString& strImageNS, BOOL bEnableCtrl);
	CStateCtrlState (const CString& strImageNS, const CString& strImageNSDisabled, BOOL bEnableCtrl);

	CStateCtrlState (UINT nBmpNormalId, BOOL bEnableCtrl);
	CStateCtrlState (UINT nBmpNormalId, UINT nBmpDisabledId, BOOL bEnableCtrl);

public:
	const UINT GetBmpNormalId	() const { return m_nBmpNormalId; }
	const UINT GetBmpDisabledId	() const { return m_nBmpDisabledId; }

	const BOOL IsToEnableCtrl	() const { return m_bEnableCtrl; }

	const CString GetBmpNS				()	   const { return m_imageNS; }
	const CString GetBmpNSDisabled		()	   const { return m_imageNSDisabled; }

	void Set (const CString& strImageNS, BOOL bEnableCtrl);
	void Set (const CString& strImageNS, const CString& strImageNSDisabled, BOOL bEnableCtrl);

	void Set (UINT nBmpNormalId, BOOL bEnableCtrl);
	void Set (UINT nBmpNormalId, UINT nBmpDisabledId, BOOL bEnableCtrl);
};

//=============================================================================
//							class CStateCtrlObj
//		Gestione della funzionalità di stato agganciata ad un bottone
//=============================================================================
class TB_EXPORT CStateCtrlObj : public CObject
{
	friend CParsedCtrl;
	friend CParsedEdit;
	friend CParsedStatic;
	friend CParsedCombo;

private:
	CParsedCtrl*	m_pParsedCtrl;
	DataObj*		m_pOldDataObj;
	DataObj*		m_pDataObj;
	CStateButton*	m_pButton;
	int				m_nDataInfoIdx;

	BOOL			m_bManualReadOnly;			// utilizzato per rendere readonly bottone e campo da codice 
	BOOL			m_bBtnManuallyDisabled;		// comandato dalla DisableBotton
	BOOL			m_bCtrlInEditModeEnabled;	// disabilita l'intero control
	BOOL			m_bStateInEditModeEnabled;	// disabilita lo switch di stato in Edit
	BOOL			m_bPushButtonNotify;		// notifica dei messaggi 
	BOOL			m_bCtrlChangedNotify;		// notifica dei changed del control
	BOOL			m_bClearDataWhenEditable;	// utilizzato per far scattare la clear in automatico
	
	BOOL			m_bIsCommandBtn;			// caso particolare: invia un WM_COMMAND con l'IDC del parsed control

	Array			m_States;					// array di stati richiesti dal programmatore

	CParsedCtrl::MessageID	m_nCurrErrorID;
	CParsedCtrl::MessageID	m_nCurrWarningID;
	CString					m_strCurrMessage;
	CNumbererRequest*		m_pNumberingRequest;		// richiesta di servizio di numerazione
	int						m_nOldLen;					// mi salvo quando è stato chiesto il lungo control in origine

	BOOL					m_bSetColorInEditableState;
		COLORREF				m_crTextColor;
		COLORREF				m_crBkgColor;
		COLORREF				m_crSaveTextColor;
		COLORREF				m_crSaveBkgColor;

public:

	CStateCtrlObj();
	~CStateCtrlObj();

public:
	// Init members
	void AttachDataObj				(DataObj* pData, BOOL bInvertDefaultStates = FALSE);
	void DetachDataObj				();

	void ProcessFormModeChanged		(CNumbererRequest* pNumberingRequest = NULL);

	// Get members
	BOOL IsCtrlEnabledInEditMode	() const { return m_bCtrlInEditModeEnabled; }
	BOOL IsStateEnabledInEditMode	() const { return m_bStateInEditModeEnabled; }
	BOOL IsManualReadOnly			() const { return m_bManualReadOnly; }
	BOOL IsManuallyDisabled			() const { return m_bBtnManuallyDisabled; }
	BOOL IsPushButtonNotifyEnabled	() const { return m_bPushButtonNotify; }
	BOOL IsCtrlChangedNotify		() const { return m_bCtrlChangedNotify; }
	BOOL IsClearDataWhenEditable	() const { return m_bClearDataWhenEditable; }
	BOOL IsCommandBtn				() const { return m_bIsCommandBtn; }

	// Set members
	void SetManualReadOnly			(BOOL bReaOnly = TRUE);
	void EnableCtrlInEditMode		(BOOL bEnable = TRUE);
	void EnableStateInEditMode		(BOOL bEnable = TRUE);
	void EnablePushButtonNotify		(BOOL bEnable = TRUE);
	void EnableCtrlChangedNotify	(BOOL bEnable = TRUE);
	void DisableButton				(BOOL bDisabled = FALSE);
	void SetClearDataWhenEditable	(BOOL bClear = TRUE);

	void SetCommandBtn				(BOOL bSet = TRUE);
	void SetColorInEditableState	(COLORREF crBkg, COLORREF crText = AfxGetThemeManager()->GetEnabledControlBkgColor());

	// informazioni di stato
	const int		GetCurrentCtrlState	() const;
	DataObj*		GetDataObj			() const;
	const DataObj*	GetOldDataObj		() const;
	void			SetDataObj			(DataObj* pDataObj);

	// gestione degli stati
	CStateButton*		GetButton		() const { return m_pButton; } 
	int					GetButtonWidth	() const;

	CStateCtrlState*	GetCtrlState		(int nState);
	void				SetCtrlStateSingle	(int nState, UINT nBmpNormal,    BOOL bEnableCtrl = TRUE);
	void				SetCtrlStateSingle  (int nState, const CString&  strImageNS, BOOL bEnableCtrl = TRUE);
	void				SetCtrlState		(int nState, UINT nBmpNormal,    UINT nBmpDisabled, BOOL bEnableCtrl = TRUE);
	void				SetCtrlState		(int nState, const CString& nBmpNormal, const CString& nBmpDisabled, BOOL bEnableCtrl = TRUE);

	void				InitMessages	();
	BOOL				IsValid			();
	BOOL				IsInEditableState();
	BOOL				DoChar			(UINT nChar);

private:
	void Init				();
	int  IsToEnableCtrl		();
	void DoEnable			(BOOL bEnable);
	void DoPushButtonCtrl	(WPARAM wParam, LPARAM lParam);
	void ChangeCtrlStatus	(BOOL bNotify = FALSE);
	void SetButtonPos		(CRect& rectBtn, UINT nFlags);
	BOOL CreateButton		(CWnd* pParentWnd);
	void ShowButton			(int nCmdShow = SW_SHOW);
	void DeleteButton		();

	// collegamento con la numerazione
	void AttachNumberingRequest		(CNumbererRequest* pRequest);
	void ExecuteDefaultBehaviours	(bool statusHasChanged);
	int  GetEditaleStateValue		();
	void AssignEditableValueToDataObj();
};

//=============================================================================
//			Class CGridControlObj
//=============================================================================
class TB_EXPORT CGridControlObj : public CAbstractCtrl, public ResizableCtrl, public IOSLObjectManager 
{
	friend CParsedCtrl;
	friend CParsedForm;

private:
	CWnd*			m_pControlWnd;

protected:
	CString			m_sName;
	CParsedForm*	m_pParentForm;
	WORD			m_wKeyState;
	BOOL			m_bIsInspecting;

public:
	CGridControlObj(CWnd* pWnd, OSLTypeObject eType, const CString& sName);

	CTBNamespace*	GetOwnerModule			();
	BOOL			IsDisableOtherContext	();
	CBaseDocument*	GetDocument				() const;
	void			SetParentForm			(CParsedForm* pParentForm);
	void			SetName					(const CString& sName);
	CString			GetName					(){ return m_sName; }
	CTBNamespace&	GetNamespace			() { return GetInfoOSL()->m_Namespace; }
	CWnd*			GetNextViewTabItem		(BOOL bPrev = TRUE, CWnd* pDef = NULL);
	void			SetFieldInspecting		(BOOL bSet) { m_bIsInspecting = bSet; }
	CParsedForm*	GetParentForm			()	const { return m_pParentForm; }
	
public:
	virtual	void			Customize				()						= 0;
	virtual CParsedCtrl*	GetCurrentParsedCtrl	(UINT nIDC)				= 0;
	virtual CParsedCtrl*	GetCurrentParsedCtrl	(CTBNamespace aNS)		= 0;
	virtual CParsedCtrl*	GetCurrentParsedCtrl	(DataObj* pDataObj)		= 0;
	virtual void			OnUpdateControls		(BOOL bParentIsVisible = TRUE)	= 0;
	virtual void			SetDataModified			(BOOL bMod)				= 0;
	
protected:
	virtual	void	OnAbortForm			()									= 0;
	virtual	BOOL	OnCheckForm			(BOOL bEmitError)					= 0;
	virtual	BOOL	OnKeyHit			(UINT nIDC, UINT nKey, UINT nHitState)	= 0;
	virtual int		DoToolHitTest		(CPoint, TOOLINFO*)					= 0;
	virtual BOOL	DoToolTipNotify		(CTooltipProperties&)				= 0;
	virtual	BOOL	DoMovingKey			(UINT nChar)						= 0;

	void			GetKeyDownState		(BOOL& shiftDown, BOOL& ctrlDown, BOOL& altDown) const;

public:
	virtual BOOL		DoKeyDown				(UINT nChar, BOOL bDoMove = TRUE);
	virtual BOOL		DoKeyUp					(UINT nChar);
	virtual BOOL		IsAutomaticExpression	() const			{ return FALSE; }
	virtual CString		GetAutomaticExpression	() const			{ return _T(""); }
	virtual BOOL		SetAutomaticExpression	(const CString&)	{ return FALSE; }
	virtual	DataObj*	GetCtrlData				()					{ return NULL; }
	virtual	CString		GetPublicName			()					{ return GetInfoOSL()->m_Namespace.ToString(); }

	static CGridControlObj* FromChild(CWnd* pWnd, CWnd* pWndContainer = NULL);
};

//=============================================================================
//			Class CGridControl
//=============================================================================
class TB_EXPORT CGridControl : public CButton, public CGridControlObj, public CCustomFont, public IDisposingSourceImpl
{
	DECLARE_DYNAMIC (CGridControl)
	friend CParsedCtrl;
	friend CParsedForm;

public:
	CGridControl(OSLTypeObject eType, const CString& sName) : CGridControlObj(this, eType, sName), CCustomFont(this), IDisposingSourceImpl(this) {}

public:
	virtual	void	GetCurrCellPos		(CGridControlCellPos&)		= 0;
	virtual	void	SetCurrCellPos		(const CGridControlCellPos&, BOOL bNotify = TRUE)	= 0;
	virtual void	OnUpdateControls	(BOOL bParentIsVisible = TRUE);

protected:
	virtual BOOL	GetCoordCell		(CRect& /*rectCell*/, int& /*nRow*/,  UINT& /*nIDC*/, int& /*nCol*/) = 0;

protected:
	//{{AFX_MSG(CGridControl)
	afx_msg	BOOL 	OnEraseBkgnd		(CDC* pDC);
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CTBGridControlObj
//=============================================================================
class TB_EXPORT CTBGridControlObj : public CBCGPGridCtrl, public ResizableCtrl //, public CCustomFont
{
	DECLARE_DYNAMIC(CTBGridControlObj)

public:
	CTBGridControlObj ();
public:
	virtual void OnUpdateControls(BOOL bParentIsVisible = TRUE);

	virtual BOOL Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);

protected:
	//{{AFX_MSG(CResizableStrStatic)
	afx_msg	LRESULT	OnRecalcCtrlSize(WPARAM, LPARAM);// { DoRecalcCtrlSize(); AdjustLayout(); return 0L; }
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

class CBaseTabManager;
class CBaseFormView;
class TileGroupInfoItem;
class CParsedDialog;
//=============================================================================
//			Class CParsedForm
//=============================================================================
class TB_EXPORT CParsedForm : public IOSLObjectManager 
{  
	friend class CJsonFormEngine;
	friend class CJsonFormEngineEx;

public:
	enum BackgroundLocation
		{
			BACKGR_TILE,
			BACKGR_TOPLEFT /*TODO ,
			BACKGR_TOPRIGHT,
			BACKGR_BOTTOMLEFT,
			BACKGR_BOTTOMRIGHT,
			BACKGR_CENTER,
			BACKGR_FIT,
			BACKGR_BESTFIT*/
		};
private:
	//membri per centratura controlli
	BOOL				m_bCenterControls;
	BOOL				m_bCenterControlsCustomized;   //indica se la centratura dei controlli e' stata impostata programmativamente
	BOOL				m_bTransparent;

	CWnd*				GetActiveChild();
	//Prj. 6709 - tentativo di risolvere il flickering
	//non vengono contate le finestre "hide"; 
	//visto che faccio "hide" per un "pò" sulla view => le frame delle rowview saltavano => in chiusura scattava SaveModified 
	//per cui mi memorizzo lo stato nativo della finestra 
	BOOL				m_bNativeWindowVisible;
	//

public:
	//Prj. 6709 - tentativo di risolvere il flickering
	//non vengono contate le finestre "hide"; 
	//visto che faccio "hide" per un "pò" sulla view => le frame delle rowview saltavano => in chiusura scattava SaveModified 
	//per cui mi memorizzo lo stato nativo della finestra 
	BOOL			GetNativeWindowVisible			() { return m_bNativeWindowVisible; }
	void			SetNativeWindowVisible			(BOOL bSet) { m_bNativeWindowVisible = bSet;}
	//
	BOOL			GetCenterControls				() { return m_bCenterControls;}
	BOOL			IsCenterControlsCustomized		() { return m_bCenterControlsCustomized;}
	void			SetCenterControls				(BOOL bCenter, BOOL bCustom = TRUE)
		{
			//Metodo che imposta via programmativa il booleano che indica se applicare la centratura dei controlli o meno.
			//Se viene chiamato viene tenuto traccia nella variabile m_bCenterControlsCustomized che il valore e' stato impostato via programmativa, 
			//e quindi non verra piu letto per questa view il setting globale di applicazione 
			m_bCenterControlsCustomized = bCustom;
			m_bCenterControls = bCenter;
		}

	static void		CenterControls					(CWnd* pWnd, int cx, int cy);
	static void		GetCandidateRectangles(CWnd* pWnd, CTBToolBar* pToolBar, int nToolbarHeight, CRect& rcOtherComponents, CRect& rcToolbar, int cx = -1, int cy = -1);

protected:	
	// data members
	CTBNamespace*	m_pOwner;
	UINT			m_nID;
	CString			m_sName;
	CWnd*			m_pOwnerWnd;
	CWnd*			m_pAncestorWnd;
	HWND*			m_phLastCtrlFocused;
	UINT*			m_pNotValidCtrlID;
	WORD*			m_pErrorMode;
	BOOL			m_bNoOtherContext; // serve per impedire che venga modificato il contesto di default

	CString			m_strToolTipBuffer;
	int				m_nDefaultToolTipWidth;
	COLORREF		m_clrDefaultToolTipText;
	COLORREF		m_clrDefaultToolTipBackgnd;

	//background image
	COLORREF		m_clrBackground;
	CBrush*			m_pBrushBackground;

	HBITMAP				m_hBackgroundImg;
	int					m_nImgWidth;
	int					m_nImgHeight;
	CString				m_strBackgroundImg;
	BackgroundLocation  m_locBackgroundImg;
	CSize				m_csOrigin;
	//---

	CFont*				m_pFont;

	CMap<HWND, HWND, CWnd*, CWnd*>	m_Panel;
	CList<UINT, UINT>				m_Excluded;
	CLayoutContainer*				m_pLayoutContainer;

	CBaseDocument*			m_pDocument;
	ControlLinks*			m_pControlLinks;
protected:
	// constructors
	CParsedForm(CWnd*, UINT, const CString& sName);

public:
	virtual ~CParsedForm();	

public:
	CLayoutContainer* GetLayoutContainer() { return  m_pLayoutContainer; }
	void	SetOwnerContext		(CTBNamespace* pOwner)	{ if (!m_bNoOtherContext && pOwner) m_pOwner = pOwner; }
	void	SetToolTipBuffer	(const CString& strTooltip);
	
	virtual CTBNamespace&	GetNamespace			() { return GetInfoOSL()->m_Namespace; }

	const CString&			GetFormName				() const { return m_sName; }
	CWnd*					GetFormCWnd				() const { ASSERT(m_pOwnerWnd); return m_pOwnerWnd; }
	UINT					GetDialogID				() const { return m_nID; }
	void					SetDialogID				(UINT nID) { m_nID = nID; }

	CTBNamespace*			GetOwnerModule			() const { return m_pOwner; }

	void 					SetFormName				(const CString& sName) { m_sName = sName; }
	void					AttachDocument			(CBaseDocument* pDocument) { m_pDocument = pDocument; }
	CBaseDocument*			GetDocument				() const { return m_pDocument; }

	BOOL			IsDisableOtherContext	() const	{ return m_bNoOtherContext; } 
	void			DisableOtherContext		()			{ m_bNoOtherContext = TRUE; }

	// edit control related member
	CWnd*	GetLastFocusedCtrl	()	const;
	CWnd*	GetFormAncestor		()	{ return m_pAncestorWnd; }
	UINT&	GetNotValidCtrlID	()	{ return *m_pNotValidCtrlID; }
	WORD&	GetErrorModeEmitting()	{ return *m_pErrorMode; }

	void	AbortForm			();
	void	InitFormFlags		();

	//sposta tutti i control che sono sotto la coordinata indicata sommando la quantità Offset alle ordinate dei control
	void			ShiftControl		(UINT nTop, UINT nOffset);
	void			DoResizeControls	();
	static  void	DoResizeControls	(CWnd*);

	virtual BOOL	CheckForm			(BOOL bEmitError = TRUE);
	
	int		SetZOrderInnerControls		(CWnd*	pGbx, CWnd*	pParentWnd /*= NULL*/);

	void	PrepareDefaultExclusions(CObArray& arStatics);
	void	ApplyStaticSubclassing	(CObArray& arStatics);

	void	SetInitialDefaultFocus(UINT nIDC);
protected:
    void	SetBadEdit					(UINT idc, WORD wMode);
	LRESULT	DoBadValue					(UINT nCtrlID, WORD wMode);
	void	DoLosingFocus				(UINT nCtrlID, int nRelationship);
	BOOL	DoToolTipNotify				(UINT nId, NMHDR* pNMHDR, LRESULT* pResult);
	BOOL	DoToolTipHide				(UINT nId, NMHDR* pNMHDR, LRESULT* pResult);
	int		DoToolHitTest				(CPoint point, TOOLINFO* pTI) const;
	void	DoOnScroll					(UINT, UINT, CScrollBar*, BOOL);
	void	SetDefaultFont				();
	BOOL	DoEraseBkgnd				(CDC* pDC);
	void	AddBkgndImageDescription	(CWndImageDescription* pDesc);
	HBRUSH	DoOnCtlColor				(CDC* pDC, CWnd* pWnd, UINT nCtlColor);

	CBaseTabManager*			AddBaseTabManager	(CRuntimeClass* pClass);
	CBaseTileGroup*				AddBaseTileGroup(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE, TileGroupInfoItem* pDlgInfoItem = NULL, CRect rectWnd = CRect(0, 0, 0, 0));
	virtual CBaseTabManager*	AddBaseTabManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE);
	virtual CBaseTabManager*	AddBaseTileManager	(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE);

			void				NotifyLayoutChanged();
public:
	void		SetOwnFont				(LPCTSTR szFaceName, int nSize);
	CFont*		GetOwnFont				() const	{ return m_pFont; }

	void		SetBackgroundColor		(COLORREF crBackground);
	COLORREF	GetBackgroundColor		(HWND hCtrl = NULL) const;
	CBrush*		GetBackgroundBrush		(HWND hCtrl = NULL) const;
	CBrush&		GetDlgBackBrush			() const;
	void		SetTransparent			(BOOL bTransparent) { m_bTransparent = bTransparent; }
	const BOOL& IsTransparent()			{ return m_bTransparent; }

	BOOL		SetBackgroundImage		(const CString& strImage, BackgroundLocation = BACKGR_TILE, CSize csOrigin = CSize(0,0) );
	BOOL		SetBackgroundImage		(UINT nIDBImage, BackgroundLocation = BACKGR_TILE, CSize csOrigin = CSize(0,0) );
	CString		GetBackgroundImage		() { return m_strBackgroundImg; }

	BOOL		IsLayoutSuspended		(BOOL bDelay = FALSE) const;

	CBaseFormView*	GetBaseFormView		(CRuntimeClass* pClass);
	CParsedDialog*	GetAncestorDialog	();

protected:	
	// Implementation enhancement
	virtual	BOOL	DoCommand				(WPARAM wParam, LPARAM lParam);
	virtual	BOOL	DoActivate				(BOOL bActivate);
	virtual	void	DoFormatStyleChange		(WPARAM wParam, LPARAM lParam);
	virtual void	DoWindowPosChanged		(WINDOWPOS*);
	virtual	BOOL	PreProcessMessage		(MSG* pMsg);
	virtual BOOL	GetToolTipProperties	(CTooltipProperties&) { /*do nothing*/ return FALSE; }

public:
	ControlLinks*	GetControlLinks				() { return m_pControlLinks; }

	void			SetChildControlNamespace	(const CString& sName, CParsedCtrl*);

	void			HideControlGroup(UINT nGbxID, BOOL bHide = TRUE);

	static BOOL DoCommand(WPARAM wParam, LPARAM lParam, HWND hOwnerWnd, HWND* phCtrlFocus);

	void AddStaticIDC(UINT nIDC)	{ m_Excluded.AddHead(nIDC); }

			BOOL			HideControl					(UINT nIDC, BOOL bHide = TRUE);	

	virtual CWnd*	GetWndLinkedCtrl(const CTBNamespace& aNS);
	virtual CWnd*	GetWndLinkedCtrl(UINT nIDC);

	CWnd*			GetWndFromIDC			(const DWORD nIDC);
	static	CWnd*	GetChildCtrlWndFromID	(CWnd* pParent, UINT nID);

	virtual CParsedCtrl*		GetLinkedParsedCtrl(const CTBNamespace& aNS);

	virtual	void				BuildDataControlLinks ()	{/* do nothing*/ }
	virtual void				OnBuildDataControlLinks()	{/* do nothing*/ }

	virtual CBaseTileGroup*		GetChildTileGroup () const	{ return NULL; }
	virtual CBaseTabManager*	GetChildTabManager () const	{ return NULL; }

	virtual void				EnableTabControls()			{/* do nothing*/ }

	virtual void				OnAttachContext	(CWnd* pParent, CObject* pContext) { }
	CWnd*	AddLink(CRuntimeClass* pCtrlClass, int nIDC, BOOL bHKL = FALSE) { return NULL; }
	virtual CParsedCtrl* AddLink
		(
		UINT			nIDC,
		const CString&	sName,
		SqlRecord*		pRecord,
		DataObj*		pDataObj,
		CRuntimeClass*	pParsedCtrlClass,

		HotKeyLink*		pHotKeyLink = NULL,
		UINT			nBtnID = BTN_DEFAULT
		)
	{
		return NULL;
	}
	bool SetDefaultFocus();
	BOOL IsCtrlBefore(int nBefore, int nAfter);

	static bool SetTBFocus(CWnd* pWnd, BOOL bBackward);

	LRESULT CParsedForm::DoGetCWnd(WPARAM nID, LPARAM);
	LRESULT CParsedForm::DoGetParsedCtrl(WPARAM nID, LPARAM);
	LRESULT CParsedForm::DoGetLinkedParsedCtrl(WPARAM nID, LPARAM nsParam);
};

//=============================================================================
//		class CSplittedForm
//=============================================================================
class CTaskBuilderSplitterWnd;
class TB_EXPORT CSplittedForm
{
	friend class CParsedDialog;
	friend class CJsonContext;

protected:
	CWnd*		m_pForm;
private:
	
	CFrameWnd*	m_pFrame;
	CObArray	m_Splitters;

public:
	CSplittedForm (CFrameWnd* pFrame);
	CSplittedForm (CDialog* pWnd);
	~CSplittedForm ();

	CFrameWnd*					GetFrame	();
	CTaskBuilderSplitterWnd*	GetSplitter	();
	CTaskBuilderSplitterWnd*	GetSplitter	(int nIdx);

protected:
	CTaskBuilderSplitterWnd*	CreateSplitter (CRuntimeClass* pClass, int nRows, int nCols, CWnd* pParent = NULL, UINT nID = AFX_IDW_PANE_FIRST);
	CTaskBuilderSplitterWnd*	CreateSplitter (int nRows, int nCols, CWnd* pParent = NULL, UINT nID = AFX_IDW_PANE_FIRST);

	BOOL CreateSplitterEnvironment	(CWnd* pParentWnd);
	void DoResize();
	virtual	BOOL UseSplitters		();
	virtual void OnCreateSplitters	(CCreateContext* pContext) {};

private:
	BOOL CreateFrame ();
};

//=============================================================================
//		class CParsedDialog
//=============================================================================
class TB_EXPORT CParsedDialog : public CLocalizableDialog, public CParsedForm, public CSplittedForm, public IDisposingSourceImpl
{
	DECLARE_DYNAMIC (CParsedDialog)

public:
	CExternalControllerInfo::ControllingMode m_ControllingMode;

	HACCEL	m_hAccelTable;

	//gestione scrolling 
	BOOL m_bEnableVScrolling;
	BOOL m_bEnableHScrolling;

private:

	int m_nScrollStepX;
	int m_nScrollStepY;

	int m_nPageSizeY;
	int m_nScrollMaxY;

	int m_nPageSizeX;
	int m_nScrollMaxX;

	int m_nScrollPosX;
	int m_nScrollPosY;

public:
	int m_nHeight;
	int m_nWidth;
	//---- end gestione scrolling 

private:	// preso in prestito dal meccanismo delle formview                               
	HWND	m_hWndFocusDlg;
	UINT	m_NotValidCtrlID;
	WORD	m_ErrorMode;

protected:
	CTBToolBar*				m_pToolBar;
	CTBTabbedToolbar*		m_pTabbedToolBar;
	int						m_nToolbarHeight;
	BOOL					m_bHasDefaultOkCancelBar;
	CWnd*					m_pParentWnd;

public:
	enum ToolbarStyle { NONE, TOP, BOTTOM };

	
protected:                                 
	// constructors
	CParsedDialog(UINT nIdd, CWnd* pWndParent = NULL, const CString&  = _T(""));
	CParsedDialog();
	virtual ~CParsedDialog();

public:
	CTBTabbedToolbar*	GetTabbedToolbar()	{ return m_pTabbedToolBar; }
	CTBToolBar*			GetToolbar()		{ return m_pToolBar; }

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual CString GetRanorexNamespace();

public:
	virtual	BOOL			Create					(UINT nIDTemplate, CWnd* pParentWnd = NULL, const CString&  = _T(""));
			BOOL			DoModeless				();
	virtual int				DoModal					();

	virtual void			SetToolbarStyle(ToolbarStyle style, int nHeight = DEFAULT_TOOLBAR_HEIGHT, BOOL bDefaultOkCancelBar = FALSE, BOOL bWithTexts = FALSE);

	void	LoadAccelerators	(UINT nAccelIDR);
	void	AppendAccelerator	(UINT nID, BYTE fVirt, WORD key);
	void	RemoveAccelerator	(UINT nID);

	void	DestroyToolbar		();

	virtual void	OnUpdateControls(BOOL bParentIsVisible = TRUE); 
	void 			DoStretch(int cx = -1, int cy = -1);
	virtual	BOOL	OnPopulatedDropDown(UINT nIdCommand) { return FALSE; }
	void OnFindHotLinks();
	void OnResetDataObjs();
protected:
	void			ApplyTBVisualManager();

	virtual void	OnOK				();
	virtual void	OnCancel			();
	virtual BOOL	OnInitDialog		();
	virtual void	BuildJsonControlLinks();
	virtual	BOOL	OnCreateToolbar		()						{ return FALSE; }
	virtual	BOOL	OnToolbarDropDown	(UINT id, CMenu& menu)	{ return FALSE; }

			void	SendActivateToMacroRecorder				(CWnd* hWnd, int nID);
			void	SendCloseParsedDialogToMacroRecorder	(CWnd* pWnd, int nID);
			void    SendCommandToMacroRecorder				(int nID, int nCode, CString strValue, CString strText, int nBooleanValue, CString strNameSpace);
			void	DoCommandMacroRecorder					(WPARAM wParam, LPARAM lParam);
			
	virtual	BOOL	DoActivate			(BOOL bActivate);
	virtual void	EndDialog			(int nResult);
	virtual	BOOL	OnCommand			(WPARAM wParam, LPARAM lParam);
	virtual	BOOL	PreTranslateMessage	(MSG* pMsg);
	virtual int		OnToolHitTest		(CPoint point, TOOLINFO* pTI) const	{ return DoToolHitTest(point, pTI); }
	virtual void	OnCustomizeToolbar();

	void			RepositionBars2();

	void GetCandidateRectangles(CRect& rcOtherComponents, CRect& rcToolbar, int cx = -1, int cy = -1);
	virtual void ResizeOtherComponents(CRect aRect) {};
	CParsedDialog* GetParentParsedDialog();
	CParsedCtrl* GetParsedCtrlItem(UINT nIDC);
public:	
	// Generated message map functions
	//{{AFX_MSG(CParsedDialog)

	// Generated message map functions
	afx_msg LRESULT	OnBadValue				(WPARAM, LPARAM);
	afx_msg LRESULT	OnLosingFocus			(WPARAM, LPARAM);
	afx_msg LRESULT OnGetFont				(WPARAM, LPARAM);
	afx_msg	LRESULT OnFormatStyleChange		(WPARAM wParam, LPARAM lParam);
	afx_msg	void	OnActivate				(UINT, CWnd*, BOOL);
	afx_msg	void	OnSetFocus				(CWnd* pOldWnd);
	afx_msg	void	OnDestroy				();

	afx_msg BOOL	OnToolTipNotify			(UINT nId, NMHDR* pNMHDR, LRESULT* pResult) 
						{ return DoToolTipNotify(nId, pNMHDR, pResult); }
	afx_msg BOOL	OnToolTipHide			(UINT nId, NMHDR* pNMHDR, LRESULT* pResult) 
						{ return DoToolTipHide(nId, pNMHDR, pResult); }

	virtual BOOL	InitScroll				();
			void	PrepareScrollInfo		(int cx, int cy);
	afx_msg	void 	OnHScroll				(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg	void 	OnVScroll				(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg BOOL	OnMouseWheel			(UINT nFlags, short zDelta, CPoint pt);
	afx_msg void	OnSize					(UINT nType, int cx, int cy);
	afx_msg	void	OnWindowPosChanged		(WINDOWPOS*);
	afx_msg void	OnNcPaint();
	afx_msg	void	ToolbarDropDown			(NMHDR* pnmtb, LRESULT *plr);

	afx_msg LRESULT	OnGetDialogId			(WPARAM, LPARAM);
	afx_msg LRESULT	OnGetParsedCtrl			(WPARAM nID, LPARAM);
	afx_msg LRESULT OnGetCWnd				(WPARAM nID, LPARAM);
	afx_msg LRESULT	OnGetLinkedParsedCtrl	(WPARAM nID, LPARAM nsParam);

	afx_msg BOOL	OnEraseBkgnd			(CDC* pDC);
	afx_msg HBRUSH	OnCtlColor				(CDC* pDC, CWnd* pWnd, UINT nCtlColor);

	/*CBCGP*/afx_msg void OnShowWindow		(BOOL bShow, UINT nStatus);

	afx_msg LRESULT	OnCtrlFocused			(WPARAM, LPARAM);
	afx_msg LRESULT OnValueChanged			(WPARAM, LPARAM);

	afx_msg LRESULT OnKickIdle(WPARAM, LPARAM);

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


//===========================================================================
//		class CBaseFormView
//===========================================================================
class TB_EXPORT CBaseFormView : public CBCGPFormView, public CParsedForm, public LayoutElement, public IDisposingSourceImpl
{                    
	friend class CParsedDialog;

	DECLARE_DYNAMIC(CBaseFormView)

protected:
	CView::m_pDocument;	//Forza lo scope di visualizzazione del doppio data member ereditato da CParsedForm::m_pDocument

private:
	HINSTANCE			m_hResourceModule;
	TileStyle*			m_pTileStyle; // default tile style
	BOOL				m_bIsModal;

private:	// preso in prestito dal meccanismo delle formview                               
	HWND	m_hWndFocusView;
	UINT	m_NotValidCtrlID;
	WORD	m_ErrorMode;
	bool	m_bUnattendedMode;//for performance reasons
protected:	
	CJsonContextObj*	m_pJsonContext = NULL;
	// constructors
	// don't must implement CAbstractFormView(LPCTSTR lpszTemplateName) constructor
	CBaseFormView(CString sName, UINT nIDTemplate);
	virtual ~CBaseFormView();

public:	// my function implementation
	BOOL				IsModal() { return m_bIsModal; }
	void				SetModal(BOOL bSet) { m_bIsModal = bSet; }
	CJsonContextObj*	GetJsonContext() const { return m_pJsonContext; }
	void SetJsonContext(CJsonContextObj* pContext) { m_pJsonContext = pContext; }
	void	CalculateOSLInfo();
	CSize	GetDialogSize		() const;
	void	ScrollFormViewToCtrl(CWnd* pCtrl);
	BOOL	JsonCreate(DWORD dwRequestedStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CCreateContext* pContext, BOOL bNotForJson);
	virtual BOOL	Create(
								LPCTSTR lpszClassName, LPCTSTR lpszWindowName,
								DWORD dwRequestedStyle, const RECT& rect, CWnd* pParentWnd, UINT nID,
								CCreateContext* pContext
							);
protected:
	// call in the derived classes to enable the layout manager (default is disabled)
	void	EnableLayout		 (BOOL bEnable = TRUE);	
	void	ApplyTBVisualManager ();

public:
	// LayoutElement interface implementation
	virtual const	CString				GetElementNameSpace		() { return GetInfoOSL()->m_Namespace.ToString(); }
	virtual const	LayoutElementArray*	GetContainedElements	()										{ return m_pLayoutContainer ? m_pLayoutContainer->GetContainedElements() : NULL;}
	virtual			BOOL				IsVisible				()										{ return TRUE; }
	virtual			int					GetRequiredHeight		(CRect &rectAvail)						{ return rectAvail.Height(); }
	virtual			int					GetRequiredWidth		(CRect &rectAvail)						{ return rectAvail.Width(); }
	virtual			void				GetAvailableRect		(CRect &rectAvail);
	virtual			void				Relayout				(CRect &rectNew, HDWP hDWP = NULL);
	virtual			void				GetUsedRect				(CRect &rectUsed);

public:
	CBaseDocument*	GetDocument() const { return (CBaseDocument*) CView::m_pDocument; }
					void				AdjustScrollSize		(BOOL bSizeOnly = FALSE);

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);
	virtual CString GetRanorexNamespace();

protected:	// Implementation enhancement
    // virtual functions from CParsedForm
	virtual BOOL DoActivate (BOOL bActivate);
	
protected:	// Implementation enhancement
	virtual void	OnInitialUpdate		();
	virtual void	OnActivateView		(BOOL, CView*, CView*); 
	virtual BOOL	OnCmdMsg			(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);
	virtual	BOOL	OnCommand			(WPARAM wParam, LPARAM lParam);
	virtual int		OnToolHitTest		(CPoint point, TOOLINFO* pTI) const
						{ return DoToolHitTest(point, pTI); }
	virtual CParsedCtrl*	GetLinkedParsedCtrl		(const CTBNamespace& aNS) { return NULL; }
	HINSTANCE				GetResourceModule();
	void					SetResourceModule(HINSTANCE hInstance) { m_hResourceModule = hInstance; }

	
	// Generated message map functions
	//{{AFX_MSG(CBaseFormView)
	afx_msg LRESULT	OnBadValue			(WPARAM, LPARAM);
	afx_msg LRESULT	OnLosingFocus		(WPARAM, LPARAM);
	afx_msg LRESULT	OnCtrlFocused		(WPARAM, LPARAM);
	afx_msg LRESULT OnGetFont			(WPARAM, LPARAM);
	afx_msg LRESULT OnGetDialogId		(WPARAM, LPARAM);
	afx_msg	void	OnSetFocus			(CWnd* pOldWnd);
	afx_msg	void	OnWindowPosChanged	(WINDOWPOS* wndPos);
	afx_msg BOOL	OnToolTipNotify		(UINT nId, NMHDR* pNMHDR, LRESULT* pResult) { return DoToolTipNotify(nId, pNMHDR, pResult); }
	afx_msg BOOL	OnToolTipHide		(UINT nId, NMHDR* pNMHDR, LRESULT* pResult) { return DoToolTipHide(nId, pNMHDR, pResult); }
	afx_msg	void 	OnHScroll			(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg	void 	OnVScroll			(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg BOOL	OnMouseWheel		(UINT, short, CPoint );
	afx_msg HBRUSH	OnCtlColor			(CDC* pDC, CWnd* pWnd, UINT nCtlColor);
	afx_msg void	OnSize				(UINT nType, int cx, int cy);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

class CParsedCtrlFamily;
//===========================================================================
//		class CRegisteredParsedCtrl
//===========================================================================
class TB_EXPORT CRegisteredParsedCtrl : public CObject
{                    
	DECLARE_DYNAMIC(CRegisteredParsedCtrl)

private:
	CString				m_sName;
	CString				m_sLocalizedText;
	DataType			m_DataType;
	CRuntimeClass*		m_pClass;
	DWORD				m_wNeededStyle;
	DWORD				m_wNeededExStyle;
	DWORD				m_wNotWantedStyle;
	DWORD				m_wNotWantedExStyle;
	CParsedCtrlFamily*	m_pFamily;


public:
	CRegisteredParsedCtrl	(
								const CString& sName, 
								const CString& sLocalizedText, 
								CRuntimeClass* pClass, 
								DataType aDataType,
								DWORD wNeededStyle,
								DWORD wNeededExStyle,
								DWORD wNotWantedStyle,
								DWORD wNotWantedExStyle
							);

public:
	CRuntimeClass*				GetClass		() const { return m_pClass; }
	const CString				GetName			() const { return m_sName; }
	const CString				GetLocalizedText() const { return m_sLocalizedText; }
	const DataType				GetDataType		() const { return m_DataType; }
	const DWORD					GetNeededExStyle() const { return m_wNeededExStyle; }
	const DWORD					GetNeededStyle() const { return m_wNeededStyle; }
	const DWORD					GetNotWantedExStyle() const { return m_wNotWantedExStyle; }
	const DWORD					GetNotWantedStyle() const { return m_wNotWantedStyle; }
	const CParsedCtrlFamily*	GetFamily		() const { return m_pFamily; }

	void SetFamily (CParsedCtrlFamily* pFamily);
};

//===========================================================================
//		class CParsedCtrlFamily
//===========================================================================
class TB_EXPORT CParsedCtrlFamily : public CMapStringToOb
{
	DECLARE_DYNAMIC(CParsedCtrlFamily)

	friend class CParsedCtrlRegistry;
private:
	CString	 m_sName;
	CString	 m_sNamespace;
	CString	 m_sAssemblyName;
	CString	 m_sVersioning;
	CString	 m_sCaption;
	DataType m_DefaultType;
	BOOL	 m_bIsAParsObjFamily;
	
	CArray <CRegisteredParsedCtrl*> m_arDefaultControls;

public:
	CParsedCtrlFamily (const CString& sName, BOOL bIsAParsobjFamily = TRUE);
	~CParsedCtrlFamily ();

public:
	const CString&	GetName				() const;
	const CString&	GetNamespace		() const;
	const CString&	GetAssemblyName		() const;
	const CString&	GetVersioning		() const;
	const CString&	GetCaption			() const;
	const CString	GetDefaultControl	() const;
	const CString	GetDefaultControl	(DataType aDataType) const;
	const CString	GetFullName			() const;
	const CString	GetQualifiedTypeName() const;

	static void SplitQualifiedName	(const CString& sQualifiedName, CString& sName, CString& sNamespace, CString& sAssemblyName, CString& sVersioning);
	static BOOL IsQualifiedName		(const CString& sQualifiedName);

	CRegisteredParsedCtrl*	GetRegisteredControl	(const CString& sName) const;
	CRegisteredParsedCtrl*	GetRegisteredControl	(DataType aDataType) const;
	CRegisteredParsedCtrl*	GetRegisteredControl    (LPCSTR szClassName, DWORD wStyles, DWORD wExStyles) const;

	void GetAllRegisteredControls	(CObArray& arControls) const;
	void SetDefaultControl			(const CString& sDefaultControl);
	BOOL IsAParsObjFamily			() const;
};

//===========================================================================
//		class CParsedCtrlRegistry
//===========================================================================
class TB_EXPORT CParsedCtrlRegistry : public Array, public CTBLockable
{
private:
	Array					m_DefaultDataTypeFamiles;
	CParsedCtrlFamily*		m_pDefaultParsedControlFamily;

public:
	CParsedCtrlRegistry();
	~CParsedCtrlRegistry();

public:
	CRegisteredParsedCtrl*		GetRegisteredControl		(const CString& sFamily, const DataType& aDataType) const;
	CRegisteredParsedCtrl*		GetRegisteredControl		(const CString& sFamily, const CString& sName) const;
	CRegisteredParsedCtrl*		GetRegisteredControl		(CWnd* pOnwerWnd) const;
	CRegisteredParsedCtrl*		GetRegisteredControl		(const CString& sName) const;
	const CParsedCtrlFamily*	GetRegisteredControlFamily	(CWnd* pOnwerWnd) const;
	void						GetAllRegisteredControls	(CObArray& arControls) const;
	void						GetRegisteredAssemblies		(CStringArray& arNames) const;
	void						GetRegisteredFamilies		(const CString& sAssemblyName, CObArray& arFamiles) const;
	const CParsedCtrlFamily*	GetDefaultFamilyInfo		(const DataType& aDataType) const;
	const CString				GetFamilyDefaultControl		(const CString& sFamilyName) const;

	void	AddFamilyDefaultType	(const CString& sFamilyName, CRegisteredParsedCtrl *pCtrl);
	void	SetTypeDefaultFamily	(const DataType& aDataType, const CString& sFamilyName);
	void	RegisterControlsFamily	(const CString& sFamilyName, const CString& sCaption, const DataType& defaultType, const BOOL isAParsedControlFamily = TRUE);
	void	RegisterParsedControl	(const CString& sFamilyName, CRegisteredParsedCtrl* pCtrl);

public:
	virtual	LPCSTR	GetObjectName() const	{ return "CParsedCtrlRegistry"; }
	CParsedCtrlFamily*	GetFamily(const CString& sFamilyName) const;
	
private:
	BOOL				IsExcludedControl (CWnd* pWnd) const;
};

DECLARE_SMART_LOCK_PTR(CParsedCtrlRegistry)
DECLARE_CONST_SMART_LOCK_PTR(CParsedCtrlRegistry)

TB_EXPORT CParsedCtrlRegistryConstPtr AfxGetParsedControlsRegistry();
TB_EXPORT CParsedCtrlRegistryPtr	 AfxGetWritableParsedControlsRegistry();

//=============================================================================
TB_EXPORT BOOL		UseFormFont				(CWnd* pWnd);
TB_EXPORT void		SetDefaultFontControl	(CWnd* pWnd, CParsedCtrl*);

//=============================================================================
TB_EXPORT CParsedCtrl* CreateControl
	(
		const CString&	sName,
		DWORD			dwStyle, 
		const CRect&	rect,
		CWnd*			pWnd,
		CBaseDocument*	pDocument,
		ControlLinks*	pControlLinks,
		UINT			nIDC, 
		SqlRecord*		pIRecord,
		DataObj*		pDataObj, 
		CRuntimeClass*	pParsedCtrlClass,
		void*			pHotKeyLink			= NULL,
		BOOL			bIsARuntimeClass	= FALSE,
		UINT			nBtnID				= BTN_DEFAULT
	);

TB_EXPORT void	OnUpdateControls(ControlLinks* pControlLinks, BOOL bParentIsVisible = TRUE);
TB_EXPORT void	OnFindHotLinks(ControlLinks* pControlLinks);
TB_EXPORT void	OnResetDataObjs(ControlLinks* pControlLinks);
TB_EXPORT void  OnModifyDataObjs(ControlLinks* pControlLinks, BOOL bModify);

/////////////////////////////////////////////////////////////////////////////
// CParsedCtrlDropTarget (classe per abilitare il Drag-Drop su un ParsedCtrl) 
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CParsedCtrlDropTarget : public COleDropTarget
{
	DECLARE_DYNCREATE(CParsedCtrlDropTarget);

protected:
	CParsedCtrl* m_pTarget;

public:
	CParsedCtrlDropTarget();

public:
	void AttachTarget(CParsedCtrl* pCtrl);

public:
	virtual ~CParsedCtrlDropTarget() {};

	virtual DROPEFFECT	OnDragEnter(CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point) { return DROPEFFECT_NONE; };
	virtual DROPEFFECT	OnDragOver(CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point) { return DROPEFFECT_NONE; };
	virtual BOOL		OnDrop(CWnd* pWnd, COleDataObject* pDataObject, DROPEFFECT dropEffect, CPoint point) { return TRUE; };
	virtual void		OnDragLeave(CWnd* pWnd) {};
};

/////////////////////////////////////////////////////////////////////////////
// CParsedCtrlDropFilesTarget 
// (classe per abilitare il Drag-Drop di files su un ParsedCtrl) 
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CParsedCtrlDropFilesTarget : public CParsedCtrlDropTarget
{
	DECLARE_DYNCREATE(CParsedCtrlDropFilesTarget);

private:
	SubFolderBehaviourOnDropFiles m_SubFolderBehaviour = UNDEFINED_DEEP;

	UINT	m_uiFileGroupDescriptor;
	UINT	m_uiFileContents;

	BOOL	m_bEnableMsgFiles;
	BOOL	m_bDeleteTempFiles;
	CString m_sTemporaryPath;

public:
	CParsedCtrlDropFilesTarget();
	virtual ~CParsedCtrlDropFilesTarget() {};

	virtual DROPEFFECT	OnDragEnter	(CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point);
	virtual DROPEFFECT	OnDragOver	(CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point);
	virtual BOOL		OnDrop		(CWnd* pWnd, COleDataObject* pDataObject, DROPEFFECT dropEffect, CPoint point);
	virtual void		OnDragLeave	(CWnd* pWnd) {};

public:
	void EnableMsgFiles		(BOOL bEnable = TRUE, BOOL bDeleteTempFiles = TRUE);
	void SetTemporaryPath	(CString sTemporaryPath);

private:
	void GetAllFiles		(CString strPath, CStringArray& aDroppedFiles);
};

#include "endh.dex"

