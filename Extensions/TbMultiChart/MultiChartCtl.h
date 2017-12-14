
#if !defined(AFX_MULTICHARTCTL_H__7902C283_D043_11D1_8BC5_0060086FCFDE__INCLUDED_)
#define AFX_MULTICHARTCTL_H__7902C283_D043_11D1_8BC5_0060086FCFDE__INCLUDED_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

// MultiChartCtl.h : Declaration of the CMultiChartCtrl ActiveX Control class.
#include <afxtempl.h>
#include <afxconv.h>

#include <TbGeneric\TBScrollBar.h>

#include "MultiChartId.h"

//#include "Tfxdatatip.h"

#include "beginh.dex"

extern const COLORREF g_crColors[];
extern const int NUM_COLOR_BAR;
class TFXDataTip;

/////////////////////////////////////////////////////////////////////////////

//--------------------------------------------------------------------------------------
class CBar
{
public:
	double		m_dHeight;
	double		m_dBase;
	DWORD		m_dwItemData;

	CString		m_strText;
	CString		m_strToolTipText;
	COLORREF	m_crColor;
	short		m_nHatchBrush;

	CRect		m_rcBar;
	int			m_nScaleValue;

	int			m_nInd;
	int			m_nIndSegment;

public:
	CBar() 
		: 
		m_dHeight (0),
		m_dBase (0.0), 
		m_dwItemData (0),
		m_crColor (0),
		m_nHatchBrush (0),
		m_nScaleValue (0),
		m_nInd (0),
		m_nIndSegment (0)
		{}


public:
	void Empty() 
	{
		m_dHeight =0;
		m_dBase =0.0; 
		m_dwItemData =0;
		m_crColor =0;
		m_nHatchBrush =0;
		m_nScaleValue =0;
		m_strToolTipText.Empty();
		m_strText.Empty();
	//	m_nInd 
	//	m_nIndSegment 
	}

	CBar& operator=(const CBar& bar)
	{
		if (this != &bar)
		{
			m_crColor		= bar.m_crColor;
			m_dHeight		= bar.m_dHeight;
			m_dBase			= bar.m_dBase;
			m_rcBar			= bar.m_rcBar;
			m_nHatchBrush	= bar.m_nHatchBrush;
			m_nScaleValue	= bar.m_nScaleValue;
			m_nInd			= bar.m_nInd;
			m_nIndSegment	= bar.m_nIndSegment;
			m_dwItemData	= bar.m_dwItemData;
			m_strText		= bar.m_strText;
			m_strToolTipText = bar.m_strToolTipText;
		}
		return *this;
	}
};

//--------------------------------------------------------------------------------------
class CBarHeader
{
public:
	CBar	*m_arBars;
	short	m_nSegment;
	CPoint	m_ptIcon;

	CBarHeader() 
		: 
			m_arBars(NULL), m_nSegment(0), m_ptIcon (0,0)
		{}
};

//--------------------------------------------------------------------------------------
//TODO Quadrante: incrocio riga colonna
class CColumnHeader
{
public:
	CBarHeader *m_arHeaderBars;
};

//--------------------------------------------------------------------------------------
class CRowHeader
{
public:
	CColumnHeader *m_arColumns;
};

//--------------------------------------------------------------------------------------
class CHeaderInfo
{
public:
	COLORREF m_crTextColor;
	COLORREF m_crBackColor;

	CStringArray m_arstrTitle;
	short m_nAlign;	// -1:sinistra, 0:centrato, 1:destra

	CHeaderInfo() { m_crTextColor = RGB(0,0,0); m_crBackColor = RGB(0xC0,0xC0,0xC0); m_nAlign = 0; }
};

//--------------------------------------------------------------------------------------
class CBarInfo
{
public:
		CString m_strTitlePosBar;
		CString m_strFormatHeightPosBar;

		double m_dDefaultHeightPosBar;
		double m_dMaxHeightPosBar;
		double m_dMinHeightPosBar;

		short m_nDCXOffsetBar;
		short m_nDCYOffsetBar;
		short m_nDCWidthBar;
		
		short m_nDCShowMinHeightPosBar;	
		short m_nSegmentPosBar;	

		short m_nWhereShowHeightPosBar;	

public:
	CBarInfo ()
		:
			m_dDefaultHeightPosBar(0.0),
			m_dMaxHeightPosBar (0.0),
			m_dMinHeightPosBar (0.0),
			m_nDCXOffsetBar(0),
			m_nDCYOffsetBar(0),
			m_nDCWidthBar(0),	
			m_nDCShowMinHeightPosBar(0),	
			m_nSegmentPosBar(0),	
			m_nWhereShowHeightPosBar(0)
		{}

		CBarInfo& operator=(const CBarInfo& bar)
		{
			if (this != &bar)
			{
				m_strTitlePosBar		= bar.m_strTitlePosBar;
				m_strFormatHeightPosBar	= bar.m_strFormatHeightPosBar;
				m_dDefaultHeightPosBar	= bar.m_dDefaultHeightPosBar;
				m_dMaxHeightPosBar		= bar.m_dMaxHeightPosBar;
				m_dMinHeightPosBar		= bar.m_dMinHeightPosBar;
				m_nDCXOffsetBar			= bar.m_nDCXOffsetBar;
				m_nDCYOffsetBar			= bar.m_nDCYOffsetBar;
				m_nDCWidthBar			= bar.m_nDCWidthBar;
				m_nDCShowMinHeightPosBar= bar.m_nDCShowMinHeightPosBar;
				m_nSegmentPosBar		= bar.m_nSegmentPosBar;
				m_nWhereShowHeightPosBar = bar.m_nWhereShowHeightPosBar;
			}
			return *this;
		}
};

//--------------------------------------------------------------------------------------
class CLimitBar {
public:
	short m_nLimits;
	double * m_arValLimits;
	COLORREF * m_arColorLimits;
};

//--------------------------------------------------------------------------------------
class CSoglie {
public:
	double m_dHeight;
	COLORREF m_crColor;

	CSoglie() : m_dHeight(0.0), m_crColor(0) {}
};

/////////////////////////////////////////////////////////////////////////////////////////

class CGDIResources : public CObject
{
	class CBrushInfo
	{
	public:
		COLORREF	m_crColor;
		short		m_nHatch;

		CBrushInfo() : m_crColor(0), m_nHatch(0) {}
		CBrushInfo(COLORREF	crColor, short nHatch = 0) 
			: m_crColor(crColor), m_nHatch(nHatch) {}
	};

	CMapStringToPtr m_mapTagBrushes;
	int m_nProgresTagBrush;

	CMapStringToOb m_mapBrushes;

	CMap<COLORREF,COLORREF,CPen*,CPen*> m_mapPens;

//	CBrush	m_brushLeftBarSelected;
//	CBrush	m_brushRightBarSelected;
public:
	CBitmap m_bmpIconInfo;
	CFont	m_fontHeaders;
	CFont	m_fontValues;

public:
	CGDIResources();
	~CGDIResources();
	
	void		ClearMapTagBrushes();

	int			LookUpTagBrush (BOOL bAddIfMissing, const CString& strTabBrush, COLORREF& crColor, short& nHatch);

	CBrush*		GetBrush	(COLORREF crColor, int Hatch = 0);

	CPen*		GetPen		(COLORREF crColor);

//	CBrush*		GetLeftBarSelectedBrush	()  { return &m_brushLeftBarSelected; } 
//	CBrush*		GetRightBarSelectedBrush()  { return &m_brushRightBarSelected; } 
};
/////////////////////////////////////////////////////////////////////////////
//Class MultiChartEventArguments Declaration : Arguments Event 
//-------------------------------------------------------------------------
class TB_EXPORT MultiChartEventArguments : public CObject
{
	DECLARE_DYNAMIC(MultiChartEventArguments)

public:
	MultiChartEventArguments
		(
			short nR,
			short nC,
			short nB,
			short nS,
			short nX,
			short nY,
			short nDstR = -1,
			short nDstC = -1,
			short nDstB = -1,
			short nDstS = -1
		)
		:
		m_nR(nR),
		m_nC(nC),
		m_nB(nB),
		m_nS(nS),
		m_nX(nX),
		m_nY(nY),
		m_nDstR(nDstR),
		m_nDstC(nDstC),
		m_nDstB(nDstB),
		m_nDstS(nDstS)
		{}
	MultiChartEventArguments
		() {Clear();}

	void Clear()
		{
			m_nR =-1;
			m_nC =-1;
			m_nB =-1;
			m_nS =-1;
			m_nX =-1;
			m_nY =-1;
			m_nDstR =-1;
			m_nDstC =-1;
			m_nDstB =-1;
			m_nDstS =-1;
		}
public:
	short m_nR;
	short m_nC;
	short m_nB;
	short m_nS;
	short m_nX;
	short m_nY;
	short m_nDstR;
	short m_nDstC;
	short m_nDstB;
	short m_nDstS;

};
/////////////////////////////////////////////////////////////////////////////
// CMultiChartCtrl : See MultiChartCtl.cpp for implementation.
////////////////////////////////////////////////////////////////////
class TB_EXPORT CMultiChartCtrl : public CButton
{
	DECLARE_DYNCREATE(CMultiChartCtrl)

	DECLARE_MESSAGE_MAP()
	
	//metodi e proprieta
	
	typedef enum { InvGrid, InvDim, InvDimBar, InvDraw, Ok } StateGridBars;
	typedef enum 
		{ 
			Tipo_Verticale =0, 
			Tipo_Orizzontale =1,
			Tipo_SogliaVerticale =2, 
			Tipo_SogliaOrizzontale =3,
			Tipo_Torta =4,
			Tipo_VerticaleCumulativo =5, 
			Tipo_OrizzontaleCumulativo =6
		} TypeChart;

protected:
	int m_nScrollStepX;
	int m_nScrollStepY;

	int m_nPageSizeY;
	int m_nScrollMaxY;

	int m_nPageSizeX;
	int m_nScrollMaxX;

	int m_nScrollPosX;
	int m_nScrollPosY;
	
	int m_nHeight;
	int m_nWidth;
	// ----

	BOOL m_bVScroll;
	BOOL m_bHScroll;

	short m_nRows;
	short m_nCols;
	short m_nBars;

	short m_nAllocatedRows;
	short m_nAllocatedCols;
	short m_nAllocatedBars;

	BOOL m_bShowGrid;
	BOOL m_bShowPercentValues;
	BOOL m_bShowTrueValue;
	BOOL m_bShowHeightBars;
	BOOL m_bShowTitleBars;
	BOOL m_bShowTotBars;
	BOOL m_bShowIconInfo;
	short m_nShowGridScale;

	short m_nDCHFontValues;
	short m_nDCHFontHeaders;
	
	short m_nWhereShowValueBars;

	double m_dMaxHeightAllBars;
	double m_dMinHeightAllBars;
	double m_dDefaultHeightBars;
	double m_dStepHeightGrid;
	short m_nDCStepHeightGrid;

	short m_nDCHeightAllRows;
	short m_nDCWidthCols;

	short m_nDCHeightColHeader;
	short m_nDCWidthRowHeader;

	BOOL m_bUseCustomWidthBars;
	short m_nDCWidthBars;
	BOOL m_bUseCustomPosBars;

	CString m_strFormatHeightBars;
	CString m_strTitleBars;


	short m_nTypeChart;

	BOOL m_bHideBoxBars;
	COLORREF m_crColorGrid;
	COLORREF m_crColorZoneSeparator;
	COLORREF m_crBackColor; //RGB(255,255,255)
	COLORREF m_crLeftUpperCornerBackColor;

	// ---- Selezioni correnti
	CBar* m_pSelectedBar;
	short m_nSelectedRowHeader;
	short m_nSelectedColHeader;

	short m_nXPosLastClick;
	short m_nYPosLastClick;
	//----

	BOOL m_bIsSaving;	//paint su "save" (EMF DC)

	//----
	BOOL m_bIsTracking;
	short m_nTrackRow;
	short m_nTrackCol;
	CBar* m_pTrackBar;

	//----

	TFXDataTip* m_datatip;		// Simulazione ToolTip
	BOOL m_bShowToolTip;
	//----

	StateGridBars m_State;	//stato del control
	//----

	CRowHeader* m_arRows;	// array dei row headers
	CBarInfo*	m_arInfoBars; // array dei bar headers
	//----

	CLimitBar* m_arLimits;

	double* m_arTotHeightPosBar; // bars * cols

	// ---- Label Header 
	CString m_szColumnLabel;
	CString m_szRowLabel;

	CHeaderInfo* m_arInfoRowHeader;
	CHeaderInfo* m_arInfoColHeader; 
	short m_nNumLabelRowHeader;
	short m_nNumLabelColHeader;

	//---- Brush & pen
	CGDIResources m_gdiRes;

	//Soglie
	short m_nNumeroSoglie;
	CSoglie* m_arSoglie;
	
	//Event Arguments
	MultiChartEventArguments m_MCEventArguments;
	
	//scrollBar
	CTBScrollBar * m_pVScrollBar;
	CTBScrollBar * m_pHScrollBar;
	short m_nScrollPosRow;
	short m_nScrollPosCol;

public:
// Constructor
	CMultiChartCtrl();
	~CMultiChartCtrl();

//protected:
//	virtual void OnEndHScroll			(UINT nPos) {}

protected:
// ---- Message maps
	afx_msg void OnAboutBox();

	afx_msg BOOL PreCreateWindow		(CREATESTRUCT& cs);
	afx_msg int	 OnCreate				(LPCREATESTRUCT lpCreateStruct);
	afx_msg BOOL PreTranslateMessage	(LPMSG lpmsg);

	virtual	void 	DrawItem	(LPDRAWITEMSTRUCT lpDIS);
	void OnDraw				(CDC* pdc, const CRect& rcBounds);

	afx_msg void OnSize				(UINT nType, int cx, int cy);
	afx_msg void OnVScroll			(UINT nCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg void OnHScroll			(UINT nCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg void OnKeyDown			(UINT nChar, UINT nRepCnt, UINT nFlags);

	afx_msg void OnLButtonUp		( UINT nFlags, CPoint point );
	afx_msg void OnRButtonUp		( UINT nFlags, CPoint point );
	afx_msg void OnMouseMove		( UINT, CPoint );
	afx_msg void OnLButtonDown		( UINT nFlags, CPoint point );

	afx_msg void OnSave				();

	void DoSave						( CString* strFileName = NULL );

// Implementation
protected:

	
	afx_msg void OnRowsChanged ();
	afx_msg void OnColsChanged ();
	afx_msg void OnBarsChanged ();

	afx_msg void OnCheckShowGridChanged		();
	afx_msg void OnShowHeightBarsChanged	();

	afx_msg void OnMaxHeightAllBarsChanged	();
	afx_msg void OnMinHeightAllBarsChanged	();
	afx_msg void OnDefaultHeightBarsChanged	();

	afx_msg void OnWhereShowValueBarsChanged ();

	afx_msg void OnStepHeightGridChanged	();

	afx_msg void OnDCHeightAllRowsChanged	();
	afx_msg void OnDCWidthColsChanged		();

	afx_msg void OnHFontValuesChanged		();
	afx_msg void OnHFontHeadersChanged		();

	afx_msg void OnDCWidthRowHeaderChanged	();
	afx_msg void OnDCHeightColHeaderChanged ();

	afx_msg void OnUseCustomWidthBarsChanged ();
	afx_msg void OnDCCustomWidthBarsChanged ();
			void DoCalcWidthBars (); 
	afx_msg void OnUseCustomPosBarsChanged ();

	afx_msg void OnNumLabelRowHeaderChanged	();
	afx_msg void OnNumLabelColHeaderChanged	();

	afx_msg void OnGenericInvDraw ();
	afx_msg void OnTypeChartChanged ();
	afx_msg void OnFormatHeightBarsChanged ();
	afx_msg void OnUseLimitsChanged ();

	afx_msg void OnShowToolTip ();

//property
public:
	
	short GetRows()							{ return m_nRows; }
	void  SetRows(short r)					{ m_nRows = r; OnRowsChanged(); }
	short GetCols()							{ return m_nCols; }
	void  SetCols(short c)					{ m_nCols = c; OnColsChanged(); }
	short GetBars()							{ return m_nBars; }
	void  SetBars(short b)					{ m_nBars = b; OnColsChanged(); }
	short GetCurrentCol()					{ return m_MCEventArguments.m_nC;}
	short GetCurrentRow()					{ return m_MCEventArguments.m_nR;}
	short GetDCHeightAllRows()				{ return   m_nDCHeightAllRows;}
	void  SetDCHeightAllRows(short DCHeightAllRows){ m_nDCHeightAllRows = DCHeightAllRows;
													 OnDCHeightAllRowsChanged();}
	short GetDCWidthCols()					{return m_nDCWidthCols;}
	void  SetDCWidthCols(short DCWidthCols) { m_nDCWidthCols = DCWidthCols ;							
											 OnDCWidthColsChanged();}
	BOOL  GetShowGrid()						{return m_bShowGrid;}
	void  SetShowGrid(BOOL ShowGrid)		{ m_bShowGrid = ShowGrid;
											  OnCheckShowGridChanged();}
	double GetMaxHeightAllBars()			{return m_dMaxHeightAllBars;}
	void   SetMaxHeightAllBars(double MaxHeightAllBars){ m_dMaxHeightAllBars = MaxHeightAllBars;
														 OnMaxHeightAllBarsChanged();} 
	double GetStepHeightGrid()				{return m_dStepHeightGrid;}
	void   SetStepHeightGrid(double StepHeightGrid){ m_dStepHeightGrid = StepHeightGrid;
													 OnStepHeightGridChanged();}
	short  GetDCWidthRowHeader()			{return m_nDCWidthRowHeader;}
	void   SetDCWidthRowHeader(short DCWidthRowHeader){	m_nDCWidthRowHeader = DCWidthRowHeader;
														OnDCWidthRowHeaderChanged();}
	short  GetDCHeightColHeader()			{return m_nDCHeightColHeader;}
	void   SetDCHeightColHeader(short DCHeightColHeader){ m_nDCHeightColHeader = DCHeightColHeader ;
														  OnDCHeightColHeaderChanged();}
	short  GetNumLabelRowHeader()			{return m_nNumLabelRowHeader;}
	void   SetNumLabelRowHeader(short NumLabelRowHeader ){ m_nNumLabelRowHeader = NumLabelRowHeader;
														   OnNumLabelRowHeaderChanged();}

	short  GetNumLabelColHeader()			{return m_nNumLabelColHeader;}
	void   SetNumLabelColHeader(short NumLabelColHeader){ m_nNumLabelColHeader = NumLabelColHeader ;
														  OnNumLabelColHeaderChanged();}
	double GetDefaultHeightBars()			{return m_dDefaultHeightBars;}
	void   SetDefaultHeightBars(double DefaultHeightBars){ m_dDefaultHeightBars = DefaultHeightBars;
														   OnDefaultHeightBarsChanged();}						
	BOOL   GetShowHeightBars()				{ return m_bShowHeightBars;}
	void   SetShowHeightBars(BOOL ShowHeightBars)		 { m_bShowHeightBars = ShowHeightBars;
													       OnShowHeightBarsChanged();}
	CString GetFormatHeightBars()			{ return m_strFormatHeightBars;}
	void    SetFormatHeightBars(CString FormatHeightBars){ m_strFormatHeightBars =  FormatHeightBars;
														   OnFormatHeightBarsChanged();}
	BOOL	GetShowTitleBars()				{ return m_bShowTitleBars;}
	void    SetShowTitleBars(BOOL ShowTitleBars){ m_bShowTitleBars = ShowTitleBars;
												  OnGenericInvDraw();}	
	BOOL    GetShowTotBars()				{ return m_bShowTitleBars;}
	void    SetShowTotBars(BOOL ShowTotBars){ m_bShowTitleBars = ShowTotBars;
											  OnGenericInvDraw();} 
	short   GetTypeChart()					{ return m_nTypeChart;}
	void    SetTypeChart(short TypChart)	{ m_nTypeChart = TypChart;
											  OnTypeChartChanged();} 	
	CString	GetTitleBars()					{ return m_strTitleBars; }
	void    SetTitleBars(CString TitleBars) { m_strTitleBars = TitleBars;
											  OnGenericInvDraw();}
	BOOL    GetShowTrueValue()				{ return m_bShowTrueValue;}
	void    SetShowTrueValue(BOOL ShowTrueValue) {m_bShowTrueValue = ShowTrueValue ;
												  OnGenericInvDraw();}

	BOOL    GetShowToolTip()				{ return m_bShowToolTip;}
	void    SetShowToolTip(BOOL ShowToolTip){ m_bShowToolTip = ShowToolTip; 
											  OnShowToolTip();}

	short   GetDCHeightFont()				{ return  m_nDCHFontValues;}
	void    SetDCHeightFont(short DCHeightFont) { m_nDCHFontValues = DCHeightFont;}

	short   GetWhereShowValueBars()			{ return m_nWhereShowValueBars;}
	void    SetWhereShowValueBars(short WhereShowValueBars){ m_nWhereShowValueBars = WhereShowValueBars;
															 OnWhereShowValueBarsChanged();}
	BOOL    GetShowPercentValues()			{ return m_bShowPercentValues;}
	void    SetShowPercentValues(BOOL ShowPercentValues){ m_bShowPercentValues = ShowPercentValues ;
														  OnGenericInvDraw();}
	BOOL	GetUseCustomWidthBars()			{ return m_bUseCustomWidthBars;}
	void    SetUseCustomWidthBars(BOOL UseCustomWidthBar){ m_bUseCustomWidthBars = UseCustomWidthBar;
														   OnUseCustomWidthBarsChanged();}
	short   GetDCCustomWidthBars()			{ return m_nDCWidthBars;}
	void    SetDCCustomWidthBars(short DCCustomWidthBars){ m_nDCWidthBars = DCCustomWidthBars;
														   OnDCCustomWidthBarsChanged();}
	BOOL    GetHideBoxBars()				{ return m_bHideBoxBars;}
	void    SetHideBoxBars(BOOL HideBoxBars){ m_bHideBoxBars = HideBoxBars; 
										      OnGenericInvDraw();}

	unsigned long GetGridColor()				   { return m_crColorGrid;}
	void    SetGridColor(unsigned long crColorGrid){ m_crColorGrid = crColorGrid;
													OnGenericInvDraw();}
	
	unsigned long GetZoneSeparatorColor()							 { return m_crColorZoneSeparator;}
	void     SetZoneSeparatorColor(unsigned long ZoneSeparatorColor) { m_crColorZoneSeparator = ZoneSeparatorColor;
																	   OnGenericInvDraw();}
	COLORREF GetBackColor()				   { return m_crBackColor;}
	void    SetBackColor(COLORREF crBackColor) { m_crBackColor = crBackColor;
													OnGenericInvDraw();}

	COLORREF GetLeftUpperCornerBackColor() { return m_crLeftUpperCornerBackColor; }
	void	 SetLeftUpperCornerBackColor(COLORREF crLUCornerBackColor) { m_crLeftUpperCornerBackColor = crLUCornerBackColor;
													OnGenericInvDraw();}

	double GetMinHeightAllBars()			{return m_dMinHeightAllBars;}
	void   SetMinHeightAllBars(double MinHeightAllBars){ m_dMinHeightAllBars = MinHeightAllBars;
														 OnMinHeightAllBarsChanged();} 

	BOOL   GetUseCustomPosBars()			{return m_bUseCustomPosBars;}
	void   SetUseCustomPosBars(BOOL UseCustomPosBars ) { m_bUseCustomPosBars = UseCustomPosBars;
														OnUseCustomPosBarsChanged();}
														
	short  GetShowGridScale()               {return m_nShowGridScale;}
	void   SetShowGridScale(short ShowGridScale){ m_nShowGridScale = ShowGridScale;
												  OnGenericInvDraw();}

	BOOL   GetShowIconInfo()				 {return m_bShowIconInfo;}
	void   SetShowIconInfo(BOOL ShowIconInfo){m_bShowIconInfo = ShowIconInfo;
											  OnGenericInvDraw();}

	short  GetDCHeightFontHeaders()          {return m_nDCHFontHeaders;}
	void   SetDCHeightFontHeaders(short DCHeightFontHeaders) {m_nDCHFontHeaders = DCHeightFontHeaders;
															  OnHFontHeadersChanged();} 

	short  GetNumeroSoglie()				  {return m_nNumeroSoglie;}
	void   SetNumeroSoglie(short NumeroSoglie){m_nNumeroSoglie = NumeroSoglie;
											   OnNumeroSoglieChanged();}

//Method:
public:

	afx_msg void Refresh ();

	afx_msg void SetHeightBar			(short nR, short nC, short nB, double nH);
	afx_msg void SetDimBar				(short nR, short nC, short nB, double dBase, double nH);

	afx_msg void SetColorBar			(short nR, short nC, short nB, COLORREF crColor);
	afx_msg void SetColorAllBars		(short nB, COLORREF crColor);

	afx_msg void SetLabelRowHeader		(short nR, LPCTSTR sLabel, short nL);
	afx_msg void SetLabelColHeader		(short nC, LPCTSTR sLabel, short nL);
	afx_msg void SetAlignLabelRowHeader	(short nR, short nA);
	afx_msg void SetAlignLabelColHeader	(short nC, short nA);
	afx_msg void SetAlignLabelAllRowHeader	(short nA);
	afx_msg void SetAlignLabelAllColHeader	(short nA);

	afx_msg void GetPosLastClick		(short * pnX, short * pnY);

	afx_msg void SetMaxHeightPosBar		(short nB, double nH);
	afx_msg void SetMinHeightPosBar		(short nB, double nH);
	afx_msg void SetDefaultHeightPosBar	(short nB, double nH);
	afx_msg void SetFormatHeightPosBar	(short nB, LPCTSTR sFormat);
	afx_msg void SetTitlePosBar			(short nB, LPCTSTR sTitle);

	afx_msg void SetWhereShowHeightPosBar	(short nB, short n);
	afx_msg short GetWhereShowHeightPosBar	(short nB);

	afx_msg void SetDCShowMinHeightPosBar	(short nB, short nH);
	afx_msg void SetDCOffsetAndWidthPosBar	(short nB, short nXOffset, short nYOffset, short nWidth);
	afx_msg void SetTextBar					(short nR, short nC, short nB, LPCTSTR sText);

	afx_msg void Save					(LPCTSTR sFileName);
	afx_msg void SetNumLimitPosBar		(short nB, short nL);
	afx_msg void SetLimitPosBar			(short nB, short nL, double dVal, COLORREF crColor );

	afx_msg void GetPropertiesBar		(short nR, short nC, short nB, COLORREF* pcrColor, double* pdBase, double* pdDim);

	afx_msg long GetItemData			(short nR, short nC, short nB);
	afx_msg void SetItemData			(short nR, short nC, short nB, long dwItemData);

	afx_msg short GetBarSegments		(short nR, short nC, short nB);
	afx_msg void SetBarSegments			(short nR, short nC, short nB, short nSegments);

	afx_msg long GetItemDataBarSegment	(short nR, short nC, short nB, short nS);
	afx_msg void SetItemDataBarSegment	(short nR, short nC, short nB, short nS, long dwItemData);

	afx_msg void SetHeightBarSegment	(short nR, short nC, short nB, short nS, double dBase, double dH);

	afx_msg COLORREF GetColorBarSegment	(short nR, short nC, short nB, short nS);
	afx_msg void SetColorBarSegment		(short nR, short nC, short nB, short nS, COLORREF crColor);

	afx_msg short GetHatchBrushBarSegment(short nR, short nC, short nB, short nS);
	afx_msg void SetHatchBrushBarSegment(short nR, short nC, short nB, short nS, short nHatchBrush);

	afx_msg LPCTSTR GetTextBarSegment		(short nR, short nC, short nB, short nS);
	afx_msg void SetTextBarSegment		(short nR, short nC, short nB, short nS, LPCTSTR sText);
	afx_msg LPCTSTR GetToolTipTextBarSegment(short nR, short nC, short nB, short nS);
	afx_msg void SetToolTipTextBarSegment(short nR, short nC, short nB, short nS, LPCTSTR sText);
	
	afx_msg LPCTSTR GetTagColorBarSegment	(short nR, short nC, short nB, short nS);
	afx_msg void SetTagColorBarSegment	(short nR, short nC, short nB, short nS, LPCTSTR sTagColor);

	afx_msg COLORREF GetTextColorRowHeaderCell	(short nR);
	afx_msg void SetTextColorRowHeaderCell		(short nR, COLORREF crColor );
	afx_msg COLORREF GetBackColorRowHeaderCell	(short nR);
	afx_msg void SetBackColorRowHeaderCell		(short nR, COLORREF crColor );
	afx_msg COLORREF GetTextColorColHeaderCell	(short nC);
	afx_msg void SetTextColorColHeaderCell		(short nC, COLORREF crColor );
	afx_msg COLORREF GetBackColorColHeaderCell	(short nC);
	afx_msg void SetBackColorColHeaderCell		(short nC, COLORREF crColor );

	afx_msg void SetSoglia		(short nSoglia, COLORREF crColor, double dHeight );
	afx_msg void OnNumeroSoglieChanged ();
	
	afx_msg void RefreshAndSetPos (short nR = -1, short nC = -1);
	BOOL GetScrollPosRowAndCol (short& nR, short& nC);
	afx_msg void MoveBarSegment	(short nSrcR, short nSrcC, short nSrcB, short nSrcS, short nDstR, short nDstC, short nDstB, short nDstS);
	afx_msg void Print ();

protected:
	void InvalidateControl() { Invalidate(); } 
	//DA REIPLEMENTARE
	void ThrowError(int, int) ;//TODO

// ---- Event maps
	void FireBarLButtonDown			(short nR, short nC, short nB);
	void FireBarRButtonDown			(short nR, short nC, short nB);
	void FireBarSegmentLButtonDown	(short nR, short nC, short nB, short nS);
	void FireBarSegmentRButtonDown	(short nR, short nC, short nB, short nS);

	void FireBarSegmentMoved		(short nR, short nC, short nB, short nS, short nDstR, short nDstC, short nDstB, short nDstS);
	void FireBarIconRButtonDown		(short nR, short nC, short nB);

	void FireRowHeaderLButtonDown	(short nR);
	void FireColHeaderLButtonDown	(short nC);
	void FireCornerHeaderLButtonDown();
	void FireRowHeaderRButtonDown	(short nR);
	void FireColHeaderRButtonDown	(short nC);
	void FireCornerHeaderRButtonDown();

	// ----

	void AllocGrid	();
	void FreeGrid	();
	void DimBars	();
	void ReAllocBars ();

	virtual void PrepareScrollInfo	(int cx, int cy);

	CBar* TouchBars			(int nR, int nC, CPoint pt);
	CBar* TouchBars			(CPoint pt);
	
	void DrawSepLine		(CDC* pDC, int nXStart, int nYStart, int nXEnd, int nYEnd);
	void DrawBars			(CDC* pdc, const CRect& rcBounds);
	void DrawOneBar			(CDC *pdc, CBar *pBar, BOOL bSelected = FALSE);
	void DrawPies			(CDC *pdc, short nR, short nC);
	void DrawStackedBars	(CDC *pdc, short nR, short nC);
	void DrawHeightBars		(CDC *pdc, short nR, short nC);

	void DrawRowHeader		(CDC* pdc, const CRect& rcBounds);
	void DrawColHeader		(CDC* pdc, const CRect& rcBounds);
	void DrawCorner			(CDC* pdc, const CRect& rcBounds);
	void PrintHeaderText 
		( 
			CDC* pdc, CRect rcBounds, 
			CHeaderInfo&, 
			short nNumLabel,
			short nC,
			BOOL bIsSelected
		);

	void OnButtonUp(UINT nFlags, CPoint point, BOOL bLeft);
	
	void DrawBmpIconInfo(CDC* pDC, int nR, int nC, int nB);

	virtual void OnBarLButtonDown         (WPARAM, LPARAM) {}
	virtual void OnBarRButtonDown         (WPARAM, LPARAM) {}
	virtual void OnRowHeaderLButtonDown   (WPARAM, LPARAM) {}
	virtual void OnColHeaderLButtonDown   (WPARAM, LPARAM) {}
	virtual void OnCornerHeaderLButtonDown(WPARAM, LPARAM) {}
	virtual void OnRowHeaderRButtonDown   (WPARAM, LPARAM) {}
	virtual void OnColHeaderRButtonDown   (WPARAM, LPARAM) {}
	virtual void OnIconRButtonDown        (WPARAM, LPARAM) {}
	virtual void OnSegmentLButtonDown     (WPARAM, LPARAM) {}
	virtual void OnSegmentRButtonDown     (WPARAM, LPARAM) {}
	virtual void OnSegmentMoved           (WPARAM, LPARAM) {}
};

//-------------------------------------------------------------------------
#define SQR(p) ((p)*(p))
#define DIST(x1,y1,x2,y2) (sqrt(double(SQR((x1)-(x2))+SQR((y1)-(y2)))))

/*
Step 1: da ocx a control in dll mfc extension (no TB, no UNICODE) 
x 1.1 : eliminazione file/classi inutili
1.2 : esposizione delle property come coppia di metodi Get/Set
 1.3 : predisposizione applicazione di prova (proprietà ed unicode
x 1.4 : reimplementazione metodi propri di COleControl utilizzati nel codice
1.5 : si disegna ?
1.6 : trasfomazione del fire degli eventi in messaggi send/post message , predisporre unica struct per passaggio parametri ...
1.7 : l'applicazione di prova funziona ?

Step 2: Rinominare/pulire/eliminare codice morto etc etc

Step 4: aggiungere supporto TB.NET, localizzazione e messaggistica

Step 5: spostare il sample nella TestApplication (TAFramework)
*/
#endif // !defined(AFX_MULTICHARTCTL_H__7902C283_D043_11D1_8BC5_0060086FCFDE__INCLUDED)

#include "endh.dex"