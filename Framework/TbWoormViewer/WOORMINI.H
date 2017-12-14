#pragma once
#include <afxadv.h>
#include <TbGeneric\DataObj.h>
#include <TbGenlib\parsctrl.h>

//includere alla fine degli include del .H
#include "beginh.dex"

// give abstraction on align type when using DrawText or ExtTextOut
typedef DWORD AlignType;

#define DT_EX_NONE				0

//centra verticalmente escludendo l'altezza della label 
#define DT_EX_VCENTER_LABEL		0x10000
//stile di allineamento GroupBox like
#define DT_EX_FIELD_SET			0x20000
#define DT_EX_ORIENTATION_90	0x40000
#define DT_EX_ORIENTATION_270	0x80000

#define DT_EX_DEFAULT			DT_EX_VCENTER_LABEL	

//stile di allineamento GroupBox like, è sovrapposto allo stile DT_EDITCONTROL che non è applicabile in Woorm
//#define TB_DT_FIELD_SET              0x00002000

// default comuni agli oggetti di woorm
//==========================================================================                     
// standard color for text
#define	DEFAULT_TEXTCOLOR		RGB(0, 0, 0)
#define	DEFAULT_BKGCOLOR		RGB(255, 255, 255)
#define DEFAULT_ALIGN			(DT_CENTER	| DT_VCENTER | DT_NOPREFIX | DT_SINGLELINE | DT_EXPANDTABS)
#define DEFAULT_GRAPH_ALIGN		(DT_CENTER	| DT_VCENTER)

// standard align for value and label in Rect objects
#define DEFAULT_TEXT_ALIGN		(DT_LEFT | DT_TOP | DT_NOPREFIX | DT_WORDBREAK | DT_EXPANDTABS)
#define DEFAULT_VALUE_ALIGN		(DT_CENTER	| DT_VCENTER | DT_NOPREFIX | DT_SINGLELINE | DT_EXPANDTABS)
#define DEFAULT_LABEL_ALIGN		(DT_LEFT	| DT_TOP | DT_NOPREFIX | DT_SINGLELINE | DT_EXPANDTABS)
#define DEFAULT_FIELD_ALIGN		(DEFAULT_VALUE_ALIGN | DT_EX_VCENTER_LABEL)

// standard color and align for table title
#define	DEFAULT_TABLE_TITLE_FOREGROUND		RGB(0, 0, 0)
#define	DEFAULT_TABLE_TITLE_BACKGROUND		RGB(255, 255, 255)
#define DEFAULT_TABLE_TITLE_ALIGN			DEFAULT_ALIGN

// standard color and align for column title
#define	DEFAULT_COLUMN_TITLE_FOREGROUND	RGB(0, 0, 0)
#define	DEFAULT_COLUMN_TITLE_BACKGROUND	RGB(192, 192, 192)
#define DEFAULT_COLUMN_TITLE_ALIGN		DEFAULT_ALIGN

// standard color and align for cell
#define	DEFAULT_CELL_FOREGROUND			RGB(0, 0, 0)
#define	DEFAULT_CELL_BACKGROUND			RGB(255, 255, 255)
#define DEFAULT_CELL_STRING_ALIGN		(DT_LEFT | DT_VCENTER | DT_NOPREFIX | DT_SINGLELINE | DT_EXPANDTABS)
#define DEFAULT_CELL_NUM_ALIGN			(DT_RIGHT| DT_VCENTER | DT_NOPREFIX | DT_SINGLELINE | DT_EXPANDTABS)

// standard color and align for total
#define	DEFAULT_TOTAL_FOREGROUND		RGB(0, 0, 0)
#define	DEFAULT_TOTAL_BACKGROUND		RGB(255, 255, 255)
#define DEFAULT_TOTAL_STRING_ALIGN		(DT_LEFT | DT_TOP | DT_NOPREFIX | DT_EXPANDTABS)
#define DEFAULT_TOTAL_NUM_ALIGN			(DT_RIGHT| DT_TOP | DT_NOPREFIX | DT_EXPANDTABS)

// standard color and align for subtotal
#define	DEFAULT_SUBTOTAL_FOREGROUND		RGB(0, 0, 0)
#define	DEFAULT_SUBTOTAL_BACKGROUND		RGB(255, 255, 255)
#define DEFAULT_SUBTOTAL_STRING_ALIGN	DEFAULT_TOTAL_STRING_ALIGN
#define DEFAULT_SUBTOTAL_NUM_ALIGN		DEFAULT_TOTAL_NUM_ALIGN

#define	DEFAULT_TABLE_CELL_WIDTH			60
#define	DEFAULT_TABLE_CELL_HEIGHT			26
#define	DEFAULT_TABLE_COLUMN_TITLE_HEIGHT	26
#define	DEFAULT_TABLE_TITLE_HEIGHT			26

#define	DEFAULT_TEXT_RECT_HEIGHT	40
#define	DEFAULT_TEXT_RECT_WIDTH		76

#define	DEFAULT_FILE_RECT_HEIGHT	150
#define	DEFAULT_FILE_RECT_WIDTH		150

#define	PEN_TINY			1
#define	PEN_SMALL			2
#define	PEN_MEDIUM			3
#define	PEN_LARGE			5
#define	PEN_EXTRA			7
#define	MIN_PEN_SIZE		1
#define	MAX_PEN_SIZE		12
#define	DEFAULT_PEN_WIDTH	1

// Special Fields token separator
#define SPECIAL_FIELD_SEP_START	_T('{')
#define SPECIAL_FIELD_SEP_END	_T('}')

#define	MINCELL_X		8
#define	MINCELL_Y		8
#define	MINTOTAL_Y		8

#define	MAX_RATIO		32000

#define	ACTION_SENS		8
#define	BLACK_SQUARE	6

#define	MARGIN_FROM_BORDER	2

// MAPPING MODE DEPENDING CONSTANTS (see related cpp initialization)
//------------------------------------------------------------------------------
#define MAPPING_MODE	MM_TEXT

//===========================================================================
class TB_EXPORT WoormIni:public CObject
{
public:

	enum ShowType	{ ID, NAME, VALUE };
	enum CopyType	{ TXT = 0, DIF, BMP };

	ShowType	m_Show;
	CopyType	m_CopyType;

	DataBool	m_bToClipboard;
	int			m_nSortGap;
	DataInt		m_nMouseSensibility;
	DataBool	m_bAddColumnBefore;
	DataBool	m_bAutoTotal;
	DataBool	m_bTotalOnNewPage;
	DataBool	m_bResetOnNewPage;
	DataBool	m_bAlwaysHidden;
	DataBool	m_bMakeBackupFile;

	//Grid
	DataBool	m_bShowGrid;
	COLORREF	m_rgb_GridColor;	
	int			m_nGridX;
	int			m_nGridY;
	DataBool	m_bLineGrid;
	//Tracking Line
	DataBool	m_bEnableTrackCross;
	COLORREF	m_rgb_TrackLineColor;
	int			m_nTrackLineSize;
	int			m_eTrackLineStyle;
	//Object Selection
	DataBool	m_bEnableNewObjectSelection;
	COLORREF	m_rgb_ObjectSelectionColor;
	int			m_nObjectSelectionSize;
	int			m_eObjectSelectionLineStyle;
	//Hidden border (border drawn in editing for objects without user defined borders)
	COLORREF	m_rgb_HiddenBorderColor;
	int			m_eHiddenBorderStyle;

	// Time setting Auto Save
	int			m_iTimeAutoSave;

	DataBool	m_bTrackInside;
	DataBool	m_bSizeInGridUnits;
	DataBool	m_bSnapToGrid;
	DataBool	m_bShowPrintableArea;
	DataBool	m_bTransparentCreate;
	DataBool	m_bNoBorderCreate;
	DataBool	m_bNoLabelCreate;
	DataBool	m_bSortObjects;
	DataBool	m_bShowMargins;
	DataBool	m_bBottomAlign;
	DataInt		m_nVertRatio;
	DataInt		m_nHorzRatio;
	DataInt		m_nTableRows;
	DataBool	m_bIncludeTotal;

	DataBool	m_bOptimizedLineBreak;
	DataPerc	m_ColumnWidthPercentage;

	DataBool	m_bCheckBarcodeSize;
	DataBool	m_bForceVerticalAlignLabelRelative;
	DataBool	m_bShowReportTree;
	DataBool	m_bShowAllToolbars;

	CString		m_strFaceName;
		BYTE	m_lfCharSet;
		BYTE	m_lfOutPrecision;
		BYTE	m_lfClipPrecision;
		BYTE	m_lfQuality;
		BYTE	m_lfPitchAndFamily;

		

// constructors
public:
	WoormIni	();
	WoormIni	(const WoormIni&);

// operators
public:
	WoormIni&	operator =	(const WoormIni&);
	int			operator ==	(WoormIni&);
	int			operator !=	(WoormIni&);

	COLORREF	GetObjectSelectionColor();

// m_ini file read/write functions
public:
	void	ReadWoormSettings ();
	void	WriteWoormSettings(const BOOL bSave = FALSE);
};

//===========================================================================
class TB_EXPORT CWoormBaseDialog : public CParsedDialog
{
	DECLARE_DYNAMIC(CWoormBaseDialog)
public:
	CWoormBaseDialog(UINT nIdd, CWnd* pWndParent = NULL, const CString& = _T(""));
	
	afx_msg void  OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg BOOL	OnEraseBkgnd(CDC* pDC);

	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class TB_EXPORT CWoormIniDlg : public CWoormBaseDialog
{
private:
	WoormIni		m_ini;
	CMetricEdit		m_HRatioEdit;
	CMetricEdit		m_VRatioEdit;
	CMetricEdit		m_GridXEdit;
	CMetricEdit		m_GridYEdit;
	CMetricEdit		m_SortGapEdit;
	CIntEdit		m_MouseSensibilityEdit;
	CIntEdit		m_TableRowsEdit;
	CPercEdit		m_edtColumnWidthPercentage;

public:
	CWoormIniDlg(CWnd* = NULL);
	    
	virtual BOOL	OnInitDialog	();
	virtual void	OnOK			();

protected:
	BOOL		InitIntEdit		(int nID, int nValue, int nMin, int nMax);
	CIntEdit&	GetIntEdit		(int nID);

	BOOL			InitMetricEdit	(int nID, int nValue, int nMin, int nMax);
	CMetricEdit&	GetMetricEdit	(int nID);
	BOOL			IsTrueTypeFont  (LOGFONT* lf);

protected:
	//{{AFX_MSG(CWoormIniDlg)
	afx_msg	void OnChangeGridX();
	afx_msg	void OnChangeGridY();
	afx_msg	void OnCheckGridOldPaper();
	afx_msg void OnDraftFontSelection();
	afx_msg void OnKeepCryptoStateClicked();
	afx_msg void OnOptimizedLineBreakClicked();

	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};
#include "endh.dex"
