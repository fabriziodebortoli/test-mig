#pragma once

#include "rectobj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//==============================================================================

enum EnumChartType
	//ATTENZIONE: tenere allineato in: 
	//c:\development\Standard\TaskBuilder\Framework\TbWoormViewer\Chart.h - EnumChartType
	//c:\development\standard\web\server\report-service\woormviewer\Chart.cs - EnumChartType
	//c:\development\Standard\web\client\reporting-studio\src\models\chart-type.model.ts - ChartType
	//------
{
	Chart_None,

	//1 categoria, n series
	Chart_Bar,
	Chart_BarStacked,
	Chart_BarStacked100,

	Chart_Column,
	Chart_ColumnStacked,
	Chart_ColumnStacked100,

	Chart_Area,
	Chart_AreaStacked,
	Chart_AreaStacked100,

	Chart_Line,

	//1 categoria - 1 input - 1 series
	Chart_Funnel, Chart_Pie, Chart_Doughnut, 

	//1 categoria - 1 input - n series ;
	Chart_DoughnutNested,

	//1 categoria - 2 input - n series
	Chart_RangeBar, Chart_RangeColumn, Chart_RangeArea,

	//1 categoria - 3 input - n serie
	Chart_Bubble, Chart_BubbleScatter,

	//2 input - n serie
	Chart_Scatter, Chart_ScatterLine,

	//2 input - n serie 
	Chart_PolarLine, Chart_PolarArea, Chart_PolarScatter, 

	//1 categoria - n serie
	Chart_RadarLine, Chart_RadarArea, 

	Chart_Wrong,

	//solo Kendo UI
	_Chart_VerticalLine,
	_Chart_VerticalArea,
	_Chart_RadarColumn,

	//solo BCGP
	__Chart_Pyramid,
	__Chart_RadarScatter, 

	__Chart_Column3D,
	__Chart_Bar3D,

};

enum EnumChartStyle
	//ATTENZIONE: tenere allineato in: 
	//c:\development\Standard\TaskBuilder\Framework\TbWoormViewer\Chart.h - EnumChartType
	//c:\development\standard\web\server\report-service\woormviewer\table.cs - EnumChartType
	//c:\development\Standard\web\client\reporting-studio\src\models\chart-type.model.ts - ChartType
	//------
{
	ChartStyle_None, /*linea spezzata*/
	ChartStyle_LineSmooth,	/*spline*/
	ChartStyle_LineStep	
};

enum EnumChartAxisType
{
	NoAxis,
	XY,
	XYCat,
	YXCat,
	Polar,
	Radar	
};

enum EnumChartObject
{
	CHART,
	CATEGORY,
	SERIES,
	COLOR,
	TRASPARENCY,
	LEGEND,
	LABEL,
	MARKER
};

///////////////////////////////////////////////////////////////////////////////

class TB_EXPORT Chart : public BaseRect
{
	friend class  CRSTreeCtrl;
	DECLARE_DYNAMIC (Chart)

public:
	typedef struct struct_ChartTypes { 
		LPCTSTR m_sName; 
		EnumChartType m_eType; 
		EnumChartAxisType m_eAxisType; 
		BOOL m_bHasCategory; 
		int m_nDSNumber; 
		BOOL m_bHasGroups; 
		BOOL m_bSeriesMultiColor;
		BOOL m_bMultipleSeries;
		BOOL m_bSeriesWithDiffType;
		BOOL m_bLineStyle;
	} SChartTypes;

	static SChartTypes	s_arChartTypes[EnumChartType::Chart_Wrong];

public:
	class CSeries: public CObject
	{
		friend Chart;
		
	public:
		BOOL		m_bHidden = FALSE;
		CArray<SymField*>	m_arBindedField;
		CString				m_sTitle;
		EnumChartType		m_eSeriesType = EnumChartType::Chart_None;		
		EnumChartStyle		m_eStyle = ChartStyle_None;		
		int					m_nGroup = 0;	//for grouping stacked column/bar
		SymField*			m_pFieldColor = NULL;
		BOOL				m_bColored = FALSE; 
		COLORREF			m_rgbColor = 0;
		double				m_dTrasparency = 0;
		CString				m_sTrasparency;
		Chart*				m_pParent = NULL;
		BOOL				m_bShowLabels = FALSE;

	private:		
		int					m_nIndex = 0;
		CArray<COLORREF>	m_arRgbColor;		
		
	public:
		CSeries(Chart* parent) : m_pParent(parent) {}
		CSeries() {}
		virtual ~CSeries() {}
		const int GetIndex() { return m_nIndex; }
		void SetIndex(int nIndex) { m_nIndex = nIndex; }
		const CString GetTitle() { return m_sTitle; }
		void SetTitle(CString sTitle) { m_sTitle = sTitle; }
		Chart* GetParent() { return m_pParent; }
		EnumChartType GetSeriesType();
		CString GetTreeNodeDescription();
		virtual COLORREF*	GetColor() { return &m_rgbColor; }
	};

	class CCategories : public CObject
	{
		friend Chart;
	public:
		SymField*			m_pBindedField = NULL;
		CString				m_sTitle;

	private:		
		CStringArray		m_arCategoryValues;
		Chart*				m_pParent = NULL;
	public:
		CCategories(Chart* parent) : m_pParent(parent) {}

		Chart* GetParent() { return m_pParent; }
		CStringArray& GetCategoryValues() { return m_arCategoryValues; }
		const CString GetTitle() { return m_sTitle; }
		void SetTitle(CString sTitle) { m_sTitle = sTitle; }
		CString GetTreeNodeDescription();
	};

	class CLegend : public CObject
	{
	public:
		AlignType	m_Align = 4;
		BOOL		m_bEnabled = FALSE;
		Chart*		m_pParent = NULL;

		CLegend() {}
		CLegend(Chart* parent) : m_pParent(parent) {}
	};

	class CSeriesArray : public Array
	{
	public:
		CSeriesArray(Chart* pChartParent);
		Chart::CSeries* GetSeriesAt(INT_PTR nIndex) const;
		
	protected:
		Chart* m_pChart;
	};

protected:
	CBCGPChartCtrl*			m_pChart = NULL;
	CSeriesArray*			m_arSeries;
	DataArray*				m_arOwnerBag;

public:
	CString				m_sName;
	CString				m_sTitle;
	EnumChartType		m_eChartType = EnumChartType::Chart_None;
	BOOL				m_bHidden = FALSE;
	BOOL				m_bColored = FALSE;
	COLORREF			m_rgbColor = 0;
	COLORREF			m_rgbBkgColor = RGB(255, 255, 255);
	
	CCategories* m_pCategory;
	CLegend*	m_pLegend;
	//-----------------------------

	Chart(CPoint, CWoormDocMng*, WORD wID);
	Chart(const Chart& source);
	virtual  ~Chart();

	virtual BaseObj* Clone() const { return new Chart(*this); }

	virtual CString GetDescription	(BOOL = TRUE) const;
			CString GetName			(BOOL bStringName = FALSE) const;

			CSeriesArray*	GetSeries() { return m_arSeries; }

	virtual void	Draw			(CDC&, BOOL bPreview);
	virtual	CRect	GetRectToInvalidate ();
	virtual COLORREF*	GetBkgColor()  { return &m_rgbBkgColor; }
	virtual COLORREF*	GetColor()  { return &m_rgbColor; }

	virtual	void	ResetCounters	();
	virtual	void	DisableData		();
			void	OnCreate		();
			void	CreateChart		(CRect inside);


			BOOL	ParseLegend		(ViewParser&);
			BOOL	ParseSeries		(ViewParser&, CSeries*);
			BOOL	ParseCategory	(ViewParser&);
	virtual	BOOL	Parse			(ViewParser&);
	virtual	void	Unparse			(ViewUnparser&);
			void	UnparseCategory(ViewUnparser&);
			void	UnparseSeries	(ViewUnparser&, CSeries*);
			void	UnparseLegend	(ViewUnparser&);

	virtual BOOL	DeleteEditorEntry		();
	virtual	void	ClearDynamicAttributes	();

	virtual BOOL	ExistChildID	(WORD wID);

	virtual BOOL	CanSearched		() const { return FALSE; }
	virtual WORD	GetRDESearchID	() const { return 0; }

	virtual BOOL	CanDeleteField	(LPCTSTR sName, CString& sLog) const;

			BOOL	HasCategory() const;
			BOOL	AllowMultipleSeries() const;
			BOOL	AllowSeriesWithDiffType() const;
			BOOL	AllowLineStyle() const;
			
			EnumChartAxisType	GetAxisType() const;
			BOOL	IsRangeChart	(CSeries*) const;		
			BOOL	DoSelectChart	(EnumChartType ct);
			void	SyncChart();
			BOOL	SyncSeries(CSeries*);
			BOOL	SyncXYSeries(CSeries*, CBCGPChartSeries*);
			BOOL	SyncCategoriesSeries(CSeries*, CBCGPChartSeries*);
			BOOL	ReadDataSources(CSeries* pSeries, CArray<DataArray*>* allDSValueArray);
			BOOL	IsValidChart() { return m_pChart != NULL; }
			BCGPChartFormatSeries* GetColoredFormatSeries(CBCGPColor newColor, BCGPChartFormatSeries* formatSeries = NULL);

			BCGPChartType GetBCGPChartType(EnumChartType eChartType);
			BCGPChartCategory GetBCGPChartCategory(EnumChartType eChartType);
			BCGPChartFormatSeries::ChartCurveType GetBCGPChartCurveType(EnumChartStyle eChartStyle);

	virtual CString GetTooltip(int nPage = -1, CPoint point = CPoint(0,0));
	BOOL IsCompatibleChartType(EnumChartType etype);
	static CString ChartTypeDescription(EnumChartType eType);
	static BOOL	HasCategory(EnumChartType eType);
	static BOOL	HasGroups(EnumChartType eType);
	static BOOL	HasSeriesMultiColor(EnumChartType eType);	
	static EnumChartAxisType	GetAxisType(EnumChartType eType);
	static int	GetDSNumber(EnumChartType eType);	
	static BOOL AllowMultipleSeries(EnumChartType eType);
	static BOOL AllowSeriesWithDiffType(EnumChartType eType);
	static BOOL AllowLineStyle(EnumChartType eType);
};

#include "endh.dex"