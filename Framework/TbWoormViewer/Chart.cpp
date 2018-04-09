#include "stdafx.h"
#include <TbWoormEngine\edtmng.h>

#include "table.h"
#include "woormdoc.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
int CSeries_Compare(CObject*arg1, CObject* arg2)
{
	Chart::CSeries* o1 = (Chart::CSeries*)arg1; DataInt index1 = DataInt(o1->GetIndex());
	Chart::CSeries* o2 = (Chart::CSeries*)arg2; DataInt index2 = DataInt(o2->GetIndex());

	if (index1.IsGreaterThan(index2))
		return 1;
	else if (index1.IsEqual(index2))
		return 0;
	else
		return -1;
}

//-----------------------------------------------------------------------------
EnumChartType Chart::CSeries::GetSeriesType()
{
	if (m_eSeriesType != EnumChartType::Chart_None)
		return m_eSeriesType;
	else
		return m_pParent->m_eChartType;
}

//-----------------------------------------------------------------------------
CString Chart::CSeries::GetTreeNodeDescription()
{
	CString descr = GetTitle();
	if (descr.IsEmpty())
	{
		if (m_arBindedField.GetSize() == 1 && m_arBindedField[0])
			descr = '<' + m_arBindedField[0]->GetName() + '>';
		else
			descr = _TB("<series_") + DataInt(m_nIndex).ToString() + '>';
	}
	else
		descr = '"' + descr + '"';

	return descr;
}

//-----------------------------------------------------------------------------
CString Chart::CCategories::GetTreeNodeDescription()
{
	CString descr = GetTitle();
	if (descr.IsEmpty() && m_pBindedField)
		descr = '<' + m_pBindedField->GetName() + '>';
	else
		descr = '"' + descr + '"';

	return descr;
}


//-------------------------------------------------------------------------
Chart::CSeriesArray::CSeriesArray(Chart* pChartParent)
	: m_pChart(pChartParent)
{	
	// predispone il sort 
	SetCompareFunction(CSeries_Compare);
}

//-------------------------------------------------------------------------
Chart::CSeries* Chart::CSeriesArray::GetSeriesAt(INT_PTR nIndex) const
{
	return (CSeries*)__super::GetAt(nIndex);
}

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (Chart, BaseRect)


Chart::SChartTypes Chart::s_arChartTypes[EnumChartType::Chart_Wrong] =
{
	// Name,					Type,						AxisType	Categ	DS	Group	  Multicolor	MultiSeries	DiffSeriesType	LineStyle
	{_T("Area"),				Chart_Area,					XYCat,		TRUE,	1,	FALSE	, FALSE,		TRUE,		TRUE,			TRUE },
	{ _T("Area Stacked"),		Chart_AreaStacked,			XYCat,		TRUE,	1,	TRUE	, FALSE,		TRUE,		TRUE,			TRUE },
	{_T("Area Stacked 100"),	Chart_AreaStacked100,		XYCat,		TRUE,	1,	TRUE	, FALSE,		TRUE,		TRUE,			TRUE },
	{ _T("Bar"),				Chart_Bar,					YXCat,		TRUE,	1,	FALSE	, FALSE,		TRUE,		TRUE,			FALSE },
	{_T("Bar Stacked"),			Chart_BarStacked,			YXCat,		TRUE,	1,	TRUE	, FALSE,		TRUE,		TRUE,			FALSE },
	{ _T("Bar Stacked 100"),	Chart_BarStacked100,		YXCat,		TRUE,	1,	TRUE	, FALSE,		TRUE,		TRUE,			FALSE },
	{ _T("Bubble"),				Chart_Bubble,				XY,			TRUE,	3,	FALSE	, TRUE,			TRUE,		TRUE,			FALSE },
	{ _T("Bubble Scatter"),		Chart_BubbleScatter,		XY,			FALSE,	3,	FALSE	, TRUE,			TRUE,		TRUE,			FALSE },
	{ _T("Column"),				Chart_Column,				XYCat,		TRUE,	1,	FALSE	, FALSE,		TRUE,		TRUE,			FALSE },
	{ _T("Column Stacked"),		Chart_ColumnStacked,		XYCat,		TRUE,	1,	TRUE	, FALSE,		TRUE,		TRUE,			FALSE },
	{ _T("Column Stacked 100"),	Chart_ColumnStacked100,		XYCat,		TRUE,	1,	TRUE	, FALSE,		TRUE,		TRUE,			FALSE },
	{ _T("Doughnut"),			Chart_Doughnut,				NoAxis,		TRUE,	1,	FALSE	, TRUE,			FALSE,		FALSE,			FALSE },
	{ _T("Doughnut Nested"),	Chart_DoughnutNested,		NoAxis,		TRUE,	1,	FALSE	, TRUE,			TRUE,		FALSE,			FALSE },
	{ _T("Funnel"),				Chart_Funnel ,				NoAxis,		TRUE,	1,	FALSE	, TRUE,			FALSE,		FALSE,			FALSE },
	{ _T("Line"),				Chart_Line ,				XYCat,		TRUE,	1,	FALSE	, FALSE,		TRUE,		TRUE,			TRUE },
	{ _T("Pie"),				Chart_Pie ,					NoAxis,		TRUE,	1,	FALSE	, TRUE,			FALSE,		FALSE,			FALSE },
	{ _T("Polar Area"),			Chart_PolarArea ,			Polar,		FALSE,	2,	FALSE	, FALSE,		TRUE,		TRUE,			TRUE },
	{ _T("Polar Line"),			Chart_PolarLine  ,			Polar,		FALSE,	2,	FALSE	, FALSE,		TRUE,		TRUE,			TRUE },
	{ _T("Polar Scatter"),		Chart_PolarScatter ,		Polar,		FALSE,	2,	FALSE	, FALSE,		TRUE,		TRUE,			FALSE },
	{ _T("Radar Area"),			Chart_RadarArea  ,			Radar,		TRUE,	1,	FALSE	, FALSE,		TRUE,		TRUE,			TRUE },
	{ _T("Radar Line"),			Chart_RadarLine  ,			Radar,		TRUE,	1,	FALSE	, FALSE,		TRUE,		TRUE,			TRUE },
	{ _T("Range Area"),			Chart_RangeArea  ,			XYCat,		TRUE,	2,	FALSE	, FALSE,		TRUE,		TRUE,			TRUE },
	{ _T("Range Bar"),			Chart_RangeBar  ,			YXCat,		TRUE,	2,	FALSE	, FALSE,		TRUE,		FALSE,/*temp*/	FALSE },
	{ _T("Range Column"),		Chart_RangeColumn  ,		XYCat,		TRUE,	2,	FALSE	, FALSE,		TRUE,		TRUE,			FALSE },
	{ _T("Scatter"),			Chart_Scatter  ,			XY,			FALSE,	2,	FALSE	, FALSE,		TRUE,		TRUE,			FALSE },
	{ _T("Scatter Line"),		Chart_ScatterLine  ,		XY,			FALSE,	2,	FALSE	, FALSE,		TRUE,		TRUE,			TRUE },
	{ _T("None"),				Chart_None  ,				NoAxis,		FALSE,	0,	FALSE	, FALSE,		FALSE,		FALSE,			FALSE }
};

//------------------------------------------------------------------------------
Chart::Chart(CPoint ptCurrPos, CWoormDocMng* pDocument, WORD wID)
	:
	BaseRect(ptCurrPos, pDocument)
{
	this->m_wInternalID  = wID;
	this->m_bTransparent = TRUE;
	this->m_arOwnerBag =  new DataArray();
	this->m_pCategory = NULL;
	this->m_arSeries = new CSeriesArray(this);
	this->m_sName = cwsprintf(L"Chart_%d", this->m_wInternalID);
	this->m_pLegend = new Chart::CLegend(this);
}

Chart::Chart(const Chart& source)
	: 
	BaseRect(source)
{
}

Chart::~Chart()
{
	m_arSeries->RemoveAll();
	m_arOwnerBag->RemoveAll();
	SAFE_DELETE(m_pChart );
	
}

//------------------------------------------------------------------------------
CString Chart::GetDescription (BOOL /*= TRUE*/) const
{ 
	return m_sTitle.IsEmpty() ? m_sName : m_sTitle;
}

//------------------------------------------------------------------------------
CString Chart::GetName (BOOL bStringName /*= FALSE*/) const
{ 
	return m_sName;
}

//------------------------------------------------------------------------------
CRect Chart::GetRectToInvalidate ()
{
	return m_BaseRect;
}

//------------------------------------------------------------------------------
void Chart::CreateChart(CRect inside)
{
	m_pChart = new CBCGPChartCtrl();
	m_pChart->Create(inside, this, m_pDocument->m_idcCounter++);
	m_pChart->SetGraphicsManager(CBCGPGraphicsManager::BCGP_GRAPHICS_MANAGER_GDI);
	SyncChart();
}

//------------------------------------------------------------------------------
void Chart::OnCreate()
{
	SAFE_DELETE(m_pChart);
	Draw(*GetDC(), FALSE);
	m_pDocument->m_pActiveRect->SetActive(GetActiveRect());
	m_pDocument->UpdateWindow();
}

//------------------------------------------------------------------------------
void Chart::Draw (CDC& DC, BOOL bPreview)
{           
	if (!PreDraw(&DC))
		return;

	Borders borders(this->m_Borders);
	DrawBorders(DC, &borders);
	CRect inside (InsideRect(DC, m_BaseRect, m_BorderPen, borders, NoBorders(DC.IsPrinting())));
	//scalo per il device context appropriato
	ScaleRect(inside, DC);

	if (!m_pChart) 
		CreateChart(inside);


	SyncChart();

	CPrintInfo prtInfo;
	prtInfo.m_rectDraw.SetRect(inside.left, inside.top, inside.right, inside.bottom);
	m_pChart->DoPrint(&DC, &prtInfo);
	
	PostDraw(DC, bPreview, inside);

	return;
}

//------------------------------------------------------------------------------
void Chart::ResetCounters()
{

}

//------------------------------------------------------------------------------
void Chart::DisableData()
{
	
}

//------------------------------------------------------------------------------
void Chart::ClearDynamicAttributes()
{
	
}

//---------------------------------------------------------------------------
BOOL Chart::ExistChildID (WORD wID)
{
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL Chart::ParseSeries(ViewParser& lex, CSeries* pSeries)
{
	if (!lex.ParseBegin())
		return FALSE;

	//può avere un titolo
	if (lex.Matched(T_TITLE))
	{
		CString s;
		if (!lex.ParseString(s))
			return FALSE;
		else
			pSeries->SetTitle(s);
	}

	//può avere un tipo (per differenziarla dal grafico)
	if (lex.Matched(T_TYPE))
	{
		if (!lex.ParseInt((int&)pSeries->m_eSeriesType))
			return FALSE;
	}

	WoormField* pF;
	CString sVarName;
	while (lex.Matched(T_DATASOURCE))
	{
		if (!lex.ParseID(sVarName))
			return FALSE;

		pF = dynamic_cast<WoormField*>(m_pDocument->m_ViewSymbolTable.GetField(sVarName));
		if (!pF)
		{
			lex.SetError(_TB("Field associated to series doesn't exist"));
			return FALSE;
		}

		if (!pF->IsArray() && !pF->IsColumn())
		{
			lex.SetError(_TB("Field associated to series must be an array or a table column"));
			return FALSE;
		}

		if (pF->IsColumn())
		{
			if (pF->IsColTotal() || pF->IsSubTotal())
			{
				lex.SetError(_TB("Field associated to series can't be a table column total or subtotal"));
				return FALSE;
			}
			if (!pF->GetDataType().IsNumeric())
			{
				lex.SetError(_TB("Field associated to series must be numeric"));
				return FALSE;
			}
		}
		else //if (pF->IsArray())
		{
			//TODO check array basetype Isnumeric

			//lex.SetError(_TB("TODO - il campo associato alla serie deve essere numerico"));
			//return FALSE;
		}

		pSeries->m_arBindedField.Add(pF);
	}

	if (lex.Matched(T_GROUP))
	{
		if (!lex.ParseInt(pSeries->m_nGroup))
			return FALSE;
	}

	if (lex.Matched(T_COLOR))
	{
		CString sVarName;
		if(lex.LookAhead(T_ID))
		{ 
			if (lex.ParseID(sVarName))
			{
				WoormField* pF = dynamic_cast<WoormField*>(m_pDocument->m_ViewSymbolTable.GetField(sVarName));
				if (!pF)
				{
					lex.SetError(_TB("Field associated to color set of series doesn't exist"));
					return FALSE;
				}

				if (!pF->IsArray() && !pF->IsColumn())
				{
					lex.SetError(_TB("Field associated to color set of series must be an array or a column"));
					return FALSE;
				}

				if (pF->IsColumn())
				{
					if (pF->GetDataType() != DataType::String && pF->GetDataType() != DataType::Long)
					{
						lex.SetError(_TB("Field associated to color set of series must be a long (rgb)"));
						return FALSE;
					}
				}
				//else if (pF->IsArray())
				//{
				//	//TODO check array basetype Isnumeric

				//	//lex.SetError(_TB("TODO - il campo associato alla serie deve essere numerico"));
				//	//return FALSE;
				//}

				pSeries->m_pFieldColor = pF;
			}		
		}
		else
		{
			if (!lex.ParseColor(T_NULL_TOKEN, pSeries->m_rgbColor))
				return FALSE;
		}
		pSeries->m_bColored = TRUE;
	}

	if (lex.Matched(T_TRANSPARENT))
	{
		if (lex.LookAhead(T_DOUBLE))
		{
			if(!lex.ParseDouble(pSeries->m_dTrasparency))
				return FALSE;
		}
		else
			pSeries->m_dTrasparency = 1;			
	}

	if (lex.Matched(T_STYLE))
	{
		if (!lex.ParseInt((int&)pSeries->m_eStyle))
			return FALSE;
	}

	if (lex.Matched(T_LABEL))
	{
		if (!lex.ParseBool(pSeries->m_bShowLabels))
			return FALSE;
	}

	return lex.ParseEnd();
}

//------------------------------------------------------------------------------
BOOL Chart::ParseCategory(ViewParser& lex)
{
	if (!lex.ParseBegin())
		return FALSE;

	//può avere un titolo

	if (lex.Matched(T_TITLE))
	{
		CString s;
		if (!lex.ParseString(s))
			return FALSE;
		else
			m_pCategory->SetTitle(s);
	}

	if (lex.Matched(T_DATASOURCE))
	{
		CString sVarName;
		if (!lex.ParseID(sVarName))
			return FALSE;
		else
		{
			WoormField* pF = dynamic_cast<WoormField*>(m_pDocument->m_ViewSymbolTable.GetField(sVarName));
			if (!pF)
			{
				lex.SetError(_TB("Field associated to series doesn't exist"));
				return FALSE;
			}

			if (!pF->IsArray() && !pF->IsColumn())
			{
				lex.SetError(_TB("Field associated to series must be an array or a column"));
				return FALSE;
			}

			if (pF->IsColumn())
			{
				if (pF->IsColTotal() || pF->IsSubTotal())
				{
					lex.SetError(_TB("Field associated to series can't be  a table column total or subtotal"));
					return FALSE;
				}
			}

			m_pCategory->m_pBindedField = pF;
		}
	}

	

	return lex.ParseEnd();
}

//------------------------------------------------------------------------------
BOOL Chart::ParseLegend(ViewParser& lex)
{
	BOOL ok = lex.ParseBegin() /*&& lex.ParseTag(T_HIDDEN) && lex.ParseBool(this->m_pLegend->m_bEnabled)*/;
	if (!ok)
		return FALSE;

	ok = lex.ParseAlign(this->m_pLegend->m_Align);
	if (!ok)
		return FALSE;
	return lex.ParseEnd();
}

//------------------------------------------------------------------------------
BOOL Chart::Parse (ViewParser& lex)
{
	BOOL ok = lex.ParseTag(T_CHART) &&
		lex.ParseID(m_sName) &&
		lex.ParseBegin() &&
		lex.ParseAlias(m_wInternalID);

	if (lex.LookAhead(T_TITLE))
	{ 
		ok = ok && lex.ParseTag(T_TITLE) &&
		lex.ParseString(m_sTitle);
	}

	ok = ok && lex.ParseTag(T_TYPE)&& lex.ParseInt((int&)m_eChartType);

	if (lex.LookAhead(T_COLOR))
	{
		if (!lex.ParseColor(T_COLOR, m_rgbColor))
			return FALSE;
		m_bColored = TRUE;
	}

	ok = ok && lex.ParseRect(m_BaseRect);

	if(lex.LookAhead(T_BEGIN))
		ok = ok && ParseBlock (lex);

	if (!ok) return FALSE;

	m_pCategory = new CCategories(this);
	if (HasCategory() 
		&& lex.Matched(T_CHART_CATEGORIES)
		&& !ParseCategory(lex))
			return FALSE;

	int sIndex = 0;
	while (lex.Matched(T_CHART_SERIES))
	{
		CSeries* pSeries = new CSeries(this);
		pSeries->m_nIndex = sIndex++;
		if (!ParseSeries(lex, pSeries))
		{
			delete pSeries;
			continue;
		}
		m_arSeries->Add(pSeries);
	}

	if (lex.Matched(T_CHART_LEGEND))
	{ 
		if(!ParseLegend(lex))
			return FALSE;
		else 
			m_pLegend->m_bEnabled = TRUE;
	}

	if (!lex.ParseEnd())
		return FALSE;
	//------------------------
	ASSERT_VALID(m_pDocument);
	ASSERT_VALID(m_pDocument->m_pEditorManager);
	m_pDocument->m_pEditorManager->SetLastId(m_wInternalID);

	return TRUE;
}

//------------------------------------------------------------------------------
void Chart::UnparseSeries(ViewUnparser& ofile, CSeries* pSeries)
{
	ASSERT_VALID(pSeries);

	ofile.UnparseTag(T_CHART_SERIES);
	ofile.UnparseBegin(); 

	ofile.UnparseTag(T_TITLE, FALSE);
	ofile.UnparseString(pSeries->m_sTitle);

	if (pSeries->m_eSeriesType != EnumChartType::Chart_None)
	{
		ofile.UnparseTag(T_TYPE, FALSE);
		ofile.UnparseInt(int(pSeries->m_eSeriesType));
	}

	for (int i = 0; i < pSeries->m_arBindedField.GetSize(); i++)
	{
		SymField* pF = pSeries->m_arBindedField[i];
		ASSERT_VALID(pF);
		ofile.UnparseTag(T_DATASOURCE, FALSE);
		ofile.UnparseID(pF->GetName());
	}
	if (pSeries->m_nGroup)
	{
		ofile.UnparseTag(T_GROUP, FALSE);
		ofile.UnparseInt(pSeries->m_nGroup);
	}
	if (pSeries->m_bColored)
	{
		if (pSeries->m_pFieldColor
			&& Chart::HasSeriesMultiColor(pSeries->GetSeriesType()))
		{
			ofile.UnparseTag(T_COLOR, FALSE);
			ofile.UnparseID(pSeries->m_pFieldColor->GetName());
		}
		else
			ofile.UnparseColor(T_COLOR, pSeries->m_rgbColor);
	}
	if (pSeries->m_dTrasparency > 0)
	{
		ofile.UnparseTag(T_TRANSPARENT, FALSE);
		if (pSeries->m_dTrasparency < 1)
		{
			ofile.UnparseDouble(pSeries->m_dTrasparency, TRUE, L"%.1lf");
		}
		else
			ofile.UnparseCrLf();
	}
	if (pSeries->m_eStyle)
	{
		ofile.UnparseTag(T_STYLE, FALSE);
		ofile.UnparseInt(int(pSeries->m_eStyle));
	}
	ofile.UnparseTag(T_LABEL, FALSE);
	ofile.UnparseBool(pSeries->m_bShowLabels);
	
	ofile.UnparseEnd(); 
}

//------------------------------------------------------------------------------
void Chart::UnparseCategory(ViewUnparser& ofile)
{
	ofile.UnparseTag(T_CHART_CATEGORIES);
	ofile.UnparseBegin();

	if(!m_pCategory->GetTitle().IsEmpty())
	{
		ofile.UnparseTag(T_TITLE, FALSE);
		ofile.UnparseString(m_pCategory->GetTitle());
	}
	

	if(m_pCategory->m_pBindedField)
	{ 
		ofile.UnparseTag(T_DATASOURCE, FALSE);
		ofile.UnparseID(m_pCategory->m_pBindedField->GetName());
	}

	ofile.UnparseEnd();
}

//------------------------------------------------------------------------------
void Chart::UnparseLegend(ViewUnparser& ofile)
{
	if (!this->m_pLegend->m_bEnabled)
		return;

	ofile.UnparseTag(T_CHART_LEGEND);
	ofile.UnparseBegin();

	ofile.UnparseAlign(m_pLegend->m_Align);

	ofile.UnparseEnd();
}

//------------------------------------------------------------------------------
void Chart::Unparse (ViewUnparser& ofile)
{
	//---- Template Override
	BaseRect* pDefault = m_pDefault;
	if (m_bTemplate && m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		m_pDefault = NULL;
	}
	//----
	ofile.UnparseTag	(T_CHART, FALSE);
	ofile.UnparseID		(m_sName);

	ofile.UnparseBegin();
	ofile.UnparseAlias(m_wInternalID);
	ofile.UnparseTag(T_TITLE, FALSE);
	ofile.UnparseString	(m_sTitle);
	ofile.UnparseTag(T_TYPE, FALSE);
	ofile.UnparseInt(int(m_eChartType));
	if (m_bColored)
		ofile.UnparseColor(T_COLOR, m_rgbColor);
	ofile.UnparseRect	(m_BaseRect, FALSE);
	UnparseProp (ofile);
	//----

	if (HasCategory())
		UnparseCategory(ofile);

	for (int s = 0; s < m_arSeries->GetSize(); s++)
	{
		if (!AllowMultipleSeries() && s > 0)
			break;
		CSeries* pSeries = (CSeries*)m_arSeries->GetAt(s);
		ASSERT_VALID(pSeries);

		if (pSeries->m_arBindedField.GetSize() > 0)
			UnparseSeries(ofile, pSeries);
	}

	//----
	UnparseLegend(ofile);
	ofile.UnparseEnd();
	//----
	m_pDefault = pDefault;
}

//------------------------------------------------------------------------------
BOOL Chart::DeleteEditorEntry ()
{
	return __super::DeleteEditorEntry ();
}

//-----------------------------------------------------------------------------
BOOL Chart::CanDeleteField(LPCTSTR pszFieldName, CString& sLog) const
{
	if (!__super::CanDeleteField(pszFieldName, sLog))
		return FALSE;

	/*if (HasCategory() && m_pCategory!= NULL && m_pCategory->m_pBindedField)
	{
		ASSERT_VALID(m_pCategory->m_pBindedField);

		if (m_pCategory->m_pBindedField->GetName().CompareNoCase(pszFieldName) == 0)
			return FALSE;
	}

	for (int s = 0; s < m_arSeries.GetSize(); s++)
	{
		CSeries* pSeries = m_arSeries[s];
		ASSERT_VALID(pSeries);
		for (int i = 0; i < pSeries->m_arBindedField.GetSize(); i++)
		{
			SymField* pF = pSeries->m_arBindedField[i];
			ASSERT_VALID(pF);
			if (pF->GetName().CompareNoCase(pszFieldName) == 0)
				return FALSE;
		}
	}*/
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL Chart::HasCategory() const
{
	return HasCategory(m_eChartType);
}

//------------------------------------------------------------------------------
BOOL Chart::AllowMultipleSeries() const
{
	return AllowMultipleSeries(m_eChartType);
}

//------------------------------------------------------------------------------
BOOL Chart::AllowSeriesWithDiffType() const
{
	return AllowSeriesWithDiffType(m_eChartType);
}

//------------------------------------------------------------------------------
BOOL Chart::AllowLineStyle() const
{
	return AllowLineStyle(m_eChartType);
}

//------------------------------------------------------------------------------
EnumChartAxisType Chart::GetAxisType() const
{
	return GetAxisType(m_eChartType);
}
//------------------------------------------------------------------------------
BOOL Chart::IsRangeChart(CSeries* pSeries) const
{
	return	pSeries->GetSeriesType() == EnumChartType::Chart_RangeBar ||
		pSeries->GetSeriesType() == EnumChartType::Chart_RangeColumn ||
		pSeries->GetSeriesType() == EnumChartType::Chart_RangeArea;
}

//------------------------------------------------------------------------------
BOOL Chart::DoSelectChart(EnumChartType ct)
{
	this->m_eChartType = ct;
	//-----
	//if (this->m_eChartType < EnumChartType::Chart_Wrong)
	{
		if (!m_pChart)
		{
			m_pChart = new CBCGPChartCtrl();
			m_pChart->Create(m_BaseRect, this, m_pDocument->m_idcCounter++);
			m_pChart->ShowWindow(SW_NORMAL);
		}
		else
		{
			ASSERT_VALID(m_pChart);

			CBCGPChartVisualObject* pChart = m_pChart->GetChart();
			ASSERT_VALID(pChart);
			pChart->CleanUpChartData();

			//m_pChart->SetWindowPos(this, m_BaseRect.left, m_BaseRect.top, m_BaseRect.Width(), m_BaseRect.Height(), SWP_SHOWWINDOW);
			//m_pChart->ShowWindow(SW_NORMAL);
		}

		CBCGPChartVisualObject* pChart = m_pChart->GetChart();
		ASSERT_VALID(pChart);

		pChart->CleanUpChartData();

		pChart->SetChartType(GetBCGPChartCategory(m_eChartType), GetBCGPChartType(m_eChartType));

		//pChart->ShowDataLabels(TRUE);


		SyncChart();	//carica i dati della tabella nel chart
	}

	CRect rect(m_BaseRect);
	//m_pDocument->InvalidateRect(m_TitleRect, FALSE);
	m_pDocument->InvalidateRect(rect, m_bTransparent);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();

	return TRUE;
}

//------------------------------------------------------------------------------
void Chart::SyncChart()
{
	//ASSERT(m_eChartType);
	if (m_eChartType == 0)
		return;

	ASSERT(m_pChart);
	if (m_pChart == NULL)
		return;

	CBCGPChartVisualObject* pChart = m_pChart->GetChart();
	ASSERT_VALID(pChart);

	pChart->CleanUpChartData();
	pChart->SetChartTitle(m_sTitle);
	pChart->ShowDataLabels(TRUE);


	if (*GetBkgColor() != 0)
	{
		CBCGPBrush br = CBCGPBrush(CBCGPColor(m_rgbBkgColor));
		pChart->SetChartFillColor(br);
		//per modificare anche il colore della zona del diagramma vero e proprio
		//pChart->SetDiagramFillColor(br);
	}
	//se il grafico avesse attiva la proprietà del colore
	//if(!m_bColored || *GetColor() == 0)
		//pChart->SetColors(CBCGPChartTheme::ChartTheme::CT_GREEN);
	
	pChart->SetChartType(GetBCGPChartCategory(m_eChartType), GetBCGPChartType(m_eChartType));

	if(HasCategory() && m_pCategory)
	{ 
		SymField* m_pFieldCat = m_pCategory->m_pBindedField;
		//load dei valori della categoria		
		if(m_pFieldCat )
			m_pDocument->FillSeries(m_pCategory->m_arCategoryValues, m_pFieldCat->GetId());
	}

	for (int s = 0; s < m_arSeries->GetSize(); s++)
	{
		//load dei valori di ciascuna serie 
		if (!AllowMultipleSeries() && s > 0)
			break;
		CSeries* pSeries = (CSeries*)m_arSeries->GetAt(s);
		ASSERT_VALID(pSeries);
		if (!SyncSeries(pSeries)) //TODO Errore?
			continue;
	}

	pChart->m_chartLayout.m_legendPosition = !m_pLegend->m_bEnabled ? BCGPChartLayout::LP_NONE : (BCGPChartLayout::LegendPosition)m_pLegend->m_Align;
	
}

//------------------------------------------------------------------------------
BOOL Chart::SyncSeries(CSeries* pSeries)
{
	CBCGPChartVisualObject* pChart = m_pChart->GetChart();

	BOOL bMultiColorSeries = FALSE;
	CBCGPColor BCGColor = CBCGPColor();
	if(pSeries->m_bColored)
	{
		if(pSeries->m_pFieldColor && HasSeriesMultiColor(pSeries->GetSeriesType()))
		{
			pSeries->m_arRgbColor.RemoveAll();
			DataArray* seriesColors = m_pDocument->GetDataArrayFromId(pSeries->m_pFieldColor->GetId(), m_arOwnerBag);
			if(seriesColors)
			{
				COLORREF color = 0;
				for (int i = 0; i < seriesColors->GetSize(); i++)
				{
					DataObj* pColor = seriesColors->GetAt(i);
					if (pColor->IsKindOf(RUNTIME_CLASS(DataLng)))
					color = (long) *(DataLng*)pColor;		
					pSeries->m_arRgbColor.Add(color);
				}
			}
		}
		else
		{
			COLORREF  chartColor = *(pSeries->GetColor());
			BCGColor = CBCGPColor(chartColor);
		}
	}
	/*else if (pSeries->GetParent()->m_bColored)
	{
		//se ci fosse una propietà colore del grafico
		COLORREF  chartColor = *(pSeries->GetParent()->GetColor());
		BCGColor = CBCGPColor(chartColor);
	}*/

	if (m_eChartType == EnumChartType::Chart_RadarArea ||
		m_eChartType == EnumChartType::Chart_RadarLine)
	{
		CBCGPChartAxisPolarY* pYAxis = DYNAMIC_DOWNCAST(CBCGPChartAxisPolarY, pChart->GetChartAxis(BCGP_CHART_Y_POLAR_AXIS));
		ASSERT_VALID(pYAxis);
		pYAxis->m_bRadialGridLines = FALSE;
	}

	CBCGPChartSeries* pBCGSeries;
	EnumChartType eSeriesType = pSeries->GetSeriesType();
	pBCGSeries = pChart->CreateSeries(pSeries->m_sTitle, BCGColor, GetBCGPChartType(eSeriesType), GetBCGPChartCategory(eSeriesType));
	if (!pBCGSeries)
		return FALSE;
	pBCGSeries->m_strSeriesName = pSeries->m_sTitle;

	//rimuovo lo stile di default con gradiente
	pBCGSeries->SetDefaultFillGradientType(CBCGPBrush::BCGP_NO_GRADIENT);

	if (pSeries->m_bColored && !HasSeriesMultiColor(pSeries->GetSeriesType()))
	{
		BCGPChartFormatSeries* coloredSeriesFormat = GetColoredFormatSeries(BCGColor, new BCGPChartFormatSeries(pBCGSeries->GetSeriesFormat()));
		//se si vuole mantenere lo stile con gradiente
		/*CBCGPColor lighterColor = BCGColor;
		lighterColor.MakeLighter(0.3);
		CBCGPBrush currentBrush = coloredSeriesFormat->m_seriesElementFormat.m_brFillColor;
		currentBrush.SetColors(lighterColor, BCGColor, CBCGPBrush::BCGP_GRADIENT_CENTER_HORIZONTAL);
		coloredSeriesFormat->m_seriesElementFormat.m_brFillColor = currentBrush;*/
		pBCGSeries->SetSeriesFormat(*coloredSeriesFormat);

	}

	pBCGSeries->ShowDataLabel(pSeries->m_bShowLabels);
	if (m_eChartType == EnumChartType::Chart_Doughnut ||
		m_eChartType == EnumChartType::Chart_Pie ||
		m_eChartType == EnumChartType::Chart_DoughnutNested)
	{
		BCGPChartDataLabelOptions dataLabelOptions = pChart->GetDataLabelOptions();
		dataLabelOptions.m_position = BCGPChartDataLabelOptions::LabelPosition::LP_DEFAULT_POS;
		dataLabelOptions.m_bUnderlineDataLabel = TRUE;
		dataLabelOptions.m_bDrawDataLabelBorder = dataLabelOptions.m_position != BCGPChartDataLabelOptions::LP_DEFAULT_POS && dataLabelOptions.m_position != BCGPChartDataLabelOptions::LP_OUTSIDE_END;
		pChart->SetDataLabelsOptions(dataLabelOptions);
	}
	BCGPChartFormatSeries style = pBCGSeries->GetSeriesFormat();
	style.SetSeriesFillOpacity(1 - pSeries->m_dTrasparency);
	pBCGSeries->SetSeriesFormat(style);
	

	BOOL ok = FALSE;
	if (!HasCategory())
		ok = SyncXYSeries(pSeries, pBCGSeries);
	else if(m_pCategory->m_pBindedField)
		ok = SyncCategoriesSeries(pSeries, pBCGSeries);
			
	if (eSeriesType == EnumChartType::Chart_RadarArea ||
		eSeriesType == EnumChartType::Chart_PolarArea)
	{
		CBCGPChartPolarSeries* pSeries = DYNAMIC_DOWNCAST(CBCGPChartPolarSeries, pBCGSeries);
		ASSERT_VALID(pSeries);
		pSeries->CloseShape(TRUE, TRUE);
	}
	if (eSeriesType == EnumChartType::Chart_Bubble ||
		eSeriesType == EnumChartType::Chart_BubbleScatter)
	{
		CBCGPChartBubbleSeries* pBubbleSeries = DYNAMIC_DOWNCAST(CBCGPChartBubbleSeries, pBCGSeries);
		pBubbleSeries->SetMarkerShape(BCGPChartMarkerOptions::MarkerShape::MS_CIRCLE);
		pBubbleSeries->SetBubbleScale(300);
		pBubbleSeries->EnableAutoColorDataPoints();
		pBubbleSeries->m_bIncludeDataPointLabelsToLegend = TRUE;
		pBubbleSeries->SetDataLabelContent(BCGPChartDataLabelOptions::LC_BUBBLE_SIZE);
	}
	if(eSeriesType == EnumChartType::Chart_Scatter
		|| eSeriesType == EnumChartType::Chart_PolarScatter)
	{
		pBCGSeries->SetCurveType(BCGPChartFormatSeries::CCT_NO_LINE);
		pBCGSeries->ShowMarker(TRUE);
		pBCGSeries->SetDataLabelDataFormat(_T("%.2f"));
	}
	else
		pBCGSeries->SetCurveType(GetBCGPChartCurveType(pSeries->m_eStyle));
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL Chart::SyncXYSeries(CSeries* pSeries, CBCGPChartSeries* pBCGSeries)
{
	CBCGPChartVisualObject* pChart = m_pChart->GetChart();
	DataArray* ar_X_Values = NULL;
	CArray<DataArray*>* allValuesArray = new CArray<DataArray*>();
	EnumChartType eSeriesType = pSeries->GetSeriesType();

	if (!ReadDataSources(pSeries, allValuesArray) || allValuesArray->GetSize() < 2)
		return FALSE;

	ar_X_Values = allValuesArray->GetAt(0);

	BOOL bSetColor = pSeries->m_bColored
		&& HasSeriesMultiColor(pSeries->GetSeriesType())
		&& pSeries->m_arRgbColor.GetSize() > 0;

	if (IsRangeChart(pSeries))
	{
		//kendo non prevede range chart senza categoria
	}
	else if (Chart::GetDSNumber(eSeriesType) == 2 && allValuesArray->GetSize() >= 2)
	{
		DataArray* arValues = allValuesArray->GetAt(1);

		ASSERT(arValues->GetSize() <= ar_X_Values->GetSize());

		double valX = 0;
		double valY = 0;
		for (int i = 0; i < arValues->GetSize(); i++)
		{
			DataObj* pValY = arValues->GetAt(i);
			DataObj* pValX = ar_X_Values->GetAt(i);

			if (pValX->IsKindOf(RUNTIME_CLASS(DataDbl)))
				valX = (double) *(DataDbl*)pValX;
			else if (pValX->IsKindOf(RUNTIME_CLASS(DataInt)))
				valX = (short) *(DataInt*)pValX;
			else if (pValX->IsKindOf(RUNTIME_CLASS(DataLng)))
				valX = (long) *(DataLng*)pValX;	

			if (pValY->IsKindOf(RUNTIME_CLASS(DataDbl)))
				valY = (double) *(DataDbl*)pValY;
			else if (pValY->IsKindOf(RUNTIME_CLASS(DataInt)))
				valY = (short) *(DataInt*)pValY;
			else if (pValY->IsKindOf(RUNTIME_CLASS(DataLng)))
				valY = (long) *(DataLng*)pValY;
			if(bSetColor && i < pSeries->m_arRgbColor.GetSize())
			{ 
				BCGPChartFormatSeries* pFSeries = GetColoredFormatSeries(pSeries->m_arRgbColor[i], new BCGPChartFormatSeries(pBCGSeries->GetSeriesFormat()));
				pBCGSeries->AddDataPoint(valY, valX, pFSeries);
				SAFE_DELETE(pFSeries);
			}
			else
				pBCGSeries->AddDataPoint(valY, valX);
		}
	}
	else if (Chart::GetDSNumber(eSeriesType) == 3 && allValuesArray->GetSize() >= 3)
	{
		//es.:bubble
		DataArray* arValues_1 = allValuesArray->GetAt(1);
		DataArray* arValues_2 = allValuesArray->GetAt(2);

		ASSERT(arValues_1->GetSize() == arValues_2->GetSize() 
			&& arValues_1->GetSize() <= ar_X_Values->GetSize());

		double val_X = 0;
		double val_1 = 0;
		double val_2 = 0;
		double n = 1;
		for (int i = 0; i < arValues_1->GetSize(); i++)
		{
			DataObj* pValX = ar_X_Values->GetAt(i);
			DataObj* pVal_1 = arValues_1->GetAt(i);
			DataObj* pVal_2 = arValues_2->GetAt(i);

			if (pValX->IsKindOf(RUNTIME_CLASS(DataDbl)))
				val_X = (double) *(DataDbl*)pValX;
			else if (pValX->IsKindOf(RUNTIME_CLASS(DataInt)))
				val_X = (short) *(DataInt*)pValX;
			else if (pValX->IsKindOf(RUNTIME_CLASS(DataLng)))
				val_X = (long) *(DataLng*)pValX;

			if (pVal_1->IsKindOf(RUNTIME_CLASS(DataDbl)))
				val_1 = (double) *(DataDbl*)pVal_1;
			else if (pVal_1->IsKindOf(RUNTIME_CLASS(DataInt)))
				val_1 = (short) *(DataInt*)pVal_1;
			else if (pVal_1->IsKindOf(RUNTIME_CLASS(DataLng)))
				val_1 = (long) *(DataLng*)pVal_1;

			if (pVal_2->IsKindOf(RUNTIME_CLASS(DataDbl)))
				val_2 = (double) *(DataDbl*)pVal_2;
			else if (pVal_2->IsKindOf(RUNTIME_CLASS(DataInt)))
				val_2 = (short) *(DataInt*)pVal_2;
			else if (pVal_2->IsKindOf(RUNTIME_CLASS(DataLng)))
				val_2 = (long) *(DataLng*)pVal_2;

			if (bSetColor && i < pSeries->m_arRgbColor.GetSize())
			{
				BCGPChartFormatSeries* pFSeries = GetColoredFormatSeries(pSeries->m_arRgbColor[i], new BCGPChartFormatSeries(pBCGSeries->GetSeriesFormat()));
				pBCGSeries->GetChartCtrl()->AddChartDataYXY1(val_1, val_X, val_2, pSeries->m_nIndex, 0, pFSeries);
				SAFE_DELETE(pFSeries);
			}
			else
				pBCGSeries->GetChartCtrl()->AddChartDataYXY1(val_1, val_X, val_2, pSeries->m_nIndex);
		}
	}

	allValuesArray->RemoveAll();
	SAFE_DELETE(allValuesArray);

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL Chart::SyncCategoriesSeries(CSeries* pSeries, CBCGPChartSeries* pBCGSeries)
{
	CArray<DataArray*>* allValuesArray = new CArray<DataArray*>();
	EnumChartType eSeriesType = pSeries->GetSeriesType();

	if (!ReadDataSources(pSeries, allValuesArray) || allValuesArray->GetSize() < 1)
		return FALSE;

	BOOL bSetColor = pSeries->m_bColored
		&& HasSeriesMultiColor(pSeries->GetSeriesType())
		&& pSeries->m_arRgbColor.GetSize() > 0;

	//Check che tutti gli array siano compatibili in lunghezza
	int lenArray = allValuesArray->GetAt(0)->GetSize();

	for(int i = 0; i < allValuesArray->GetSize(); i++)
	{ 
		if (allValuesArray->GetAt(i)->GetSize() != lenArray || 
			m_pCategory->m_arCategoryValues.GetSize() < allValuesArray->GetAt(i)->GetSize())
		{
			ASSERT(FALSE);
			return FALSE;
		}			
	}
		
	if (IsRangeChart(pSeries))
	{
		if (allValuesArray->GetSize() < 2)
			return FALSE;

		DataArray* arValues_1 = allValuesArray->GetAt(0);
		DataArray* arValues_2 = allValuesArray->GetAt(1);

		double val_1 = 0;
		double val_2 = 0;
		double n = 1;
		for (int i = 0; i < arValues_1->GetSize(); i++)
		{
			DataObj* pVal_1 = arValues_1->GetAt(i);
			DataObj* pVal_2 = arValues_2->GetAt(i);
			CString sCatValue = m_pCategory->m_arCategoryValues[i];

			if (pVal_1->IsKindOf(RUNTIME_CLASS(DataDbl)))
				val_1 = (double) *(DataDbl*)pVal_1;
			else if (pVal_1->IsKindOf(RUNTIME_CLASS(DataInt)))
				val_1 = (short) *(DataInt*)pVal_1;
			else if (pVal_1->IsKindOf(RUNTIME_CLASS(DataLng)))
				val_1 = (long) *(DataLng*)pVal_1;

			if (pVal_2->IsKindOf(RUNTIME_CLASS(DataDbl)))
				val_2 = (double) *(DataDbl*)pVal_2;
			else if (pVal_2->IsKindOf(RUNTIME_CLASS(DataInt)))
				val_2 = (short) *(DataInt*)pVal_2;
			else if (pVal_2->IsKindOf(RUNTIME_CLASS(DataLng)))
				val_2 = (long) *(DataLng*)pVal_2;
			
			if (bSetColor && i < pSeries->m_arRgbColor.GetSize())
			{
				BCGPChartFormatSeries* pFSeries = GetColoredFormatSeries(pSeries->m_arRgbColor[i], new BCGPChartFormatSeries(pBCGSeries->GetSeriesFormat()));
				
				pBCGSeries->GetChartCtrl()->AddChartDataYY1(sCatValue, val_1, val_2 - val_1, pSeries->m_nIndex, pFSeries);
				SAFE_DELETE(pFSeries);
			}
			else
				pBCGSeries->GetChartCtrl()->AddChartDataYY1(sCatValue, val_1, val_2 - val_1, pSeries->m_nIndex);		
		}
	}
	else if(Chart::GetDSNumber(eSeriesType) == 1 &&  allValuesArray->GetSize() > 0)
	{
		DataArray* arValues = allValuesArray->GetAt(0);

		double val = 0;
		for (int i = 0; i < arValues->GetSize(); i++)
		{
			DataObj* pVal = arValues->GetAt(i);
			CString sCatValue = m_pCategory->m_arCategoryValues[i];

			if (pVal->IsKindOf(RUNTIME_CLASS(DataDbl)))
				val = (double) *(DataDbl*)pVal;
			else if (pVal->IsKindOf(RUNTIME_CLASS(DataInt)))
				val = (short) *(DataInt*)pVal;
			else if (pVal->IsKindOf(RUNTIME_CLASS(DataLng)))
				val = (long) *(DataLng*)pVal;
			

			if (bSetColor && i < pSeries->m_arRgbColor.GetSize())
			{
				COLORREF col = pSeries->m_arRgbColor[i];
				BCGPChartFormatSeries* pFSeries = GetColoredFormatSeries(col, new BCGPChartFormatSeries(pBCGSeries->GetSeriesFormat()));

				pBCGSeries->AddDataPoint(sCatValue, val, pFSeries);
				SAFE_DELETE(pFSeries);
			}
			else
				pBCGSeries->AddDataPoint(sCatValue, val);
			if (Chart::HasGroups(eSeriesType))
				pBCGSeries->SetGroupID(pSeries->m_nGroup);
		}
	}
	else if (eSeriesType == EnumChartType::Chart_Bubble)
	{
		if (allValuesArray->GetSize() < 3)
			return FALSE;

		DataArray* arValues_X = allValuesArray->GetAt(0);
		DataArray* arValues_Y = allValuesArray->GetAt(1);
		DataArray* arValues_Area = allValuesArray->GetAt(2);

		double val_X = 0;
		double val_Y = 0;
		double val_Area = 0;
		double n = 1;
		for (int i = 0; i < arValues_X->GetSize(); i++)
		{
			DataObj* pVal_X = arValues_X->GetAt(i);
			DataObj* pVal_Y = arValues_Y->GetAt(i);
			DataObj* pVal_Area = arValues_Area->GetAt(i);
			CString sCatValue = m_pCategory->m_arCategoryValues[i];

			if (pVal_X->IsKindOf(RUNTIME_CLASS(DataDbl)))
				val_X = (double) *(DataDbl*)pVal_X;
			else if (pVal_X->IsKindOf(RUNTIME_CLASS(DataInt)))
				val_X = (short) *(DataInt*)pVal_X;
			else if (pVal_X->IsKindOf(RUNTIME_CLASS(DataLng)))
				val_X = (long) *(DataLng*)pVal_X;

			if (pVal_Y->IsKindOf(RUNTIME_CLASS(DataDbl)))
				val_Y = (double) *(DataDbl*)pVal_Y;
			else if (pVal_Y->IsKindOf(RUNTIME_CLASS(DataInt)))
				val_Y = (short) *(DataInt*)pVal_Y;
			else if (pVal_Y->IsKindOf(RUNTIME_CLASS(DataLng)))
				val_Y = (long) *(DataLng*)pVal_Y;

			if (pVal_Area->IsKindOf(RUNTIME_CLASS(DataDbl)))
				val_Area = (double) *(DataDbl*)pVal_Area;
			else if (pVal_Area->IsKindOf(RUNTIME_CLASS(DataInt)))
				val_Area = (short) *(DataInt*)pVal_Area;
			else if (pVal_Area->IsKindOf(RUNTIME_CLASS(DataLng)))
				val_Area = (long) *(DataLng*)pVal_Area;

			if (bSetColor && i < pSeries->m_arRgbColor.GetSize())
			{
				BCGPChartFormatSeries* pFSeries = GetColoredFormatSeries(pSeries->m_arRgbColor[i], new BCGPChartFormatSeries(pBCGSeries->GetSeriesFormat()));
				pBCGSeries->GetChartCtrl()->AddChartDataYXY1(sCatValue, val_Y, val_X, val_Area, pSeries->m_nIndex, 0, pFSeries);
				SAFE_DELETE(pFSeries);
			}
			else
				pBCGSeries->GetChartCtrl()->AddChartDataYXY1(sCatValue, val_Y, val_X, val_Area, pSeries->m_nIndex);
		}
	}

	allValuesArray->RemoveAll();
	SAFE_DELETE(allValuesArray);
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL Chart::ReadDataSources(CSeries* pSeries, CArray<DataArray*>* allDSValuesArray)
{
	SymField* m_pFieldSeries;
	for (int i = 0; i < pSeries->m_arBindedField.GetSize(); i++)
	{
		m_pFieldSeries = pSeries->m_arBindedField[i];
		ASSERT_VALID(m_pFieldSeries);
		if (!m_pFieldSeries)
			return FALSE;

		DataArray* arValues = m_pDocument->GetDataArrayFromId(m_pFieldSeries->GetId(), m_arOwnerBag);
		if (arValues && arValues->GetSize() > 0)
			allDSValuesArray->Add(arValues);
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BCGPChartCategory Chart::GetBCGPChartCategory(EnumChartType eChartType)
{
	switch (eChartType)
	{
		case EnumChartType::Chart_Area:
		case EnumChartType::Chart_AreaStacked:
		case EnumChartType::Chart_AreaStacked100:
		case EnumChartType::Chart_RangeArea:
		{
			return BCGPChartCategory::BCGPChartArea;
		}
		case EnumChartType::Chart_Bar:
		case EnumChartType::Chart_BarStacked:
		case EnumChartType::Chart_BarStacked100:
		case EnumChartType::Chart_RangeBar:
		{
			return BCGPChartCategory::BCGPChartBar;
		}
		case EnumChartType::Chart_Bubble:
		case EnumChartType::Chart_BubbleScatter:
		{
			return BCGPChartCategory::BCGPChartBubble;
		}
		case EnumChartType::Chart_Column:
		case EnumChartType::Chart_ColumnStacked:
		case EnumChartType::Chart_ColumnStacked100:
		case EnumChartType::Chart_RangeColumn:
		{
			return BCGPChartCategory::BCGPChartColumn;
		}
		case EnumChartType::Chart_Doughnut:
		{
			return BCGPChartCategory::BCGPChartDoughnut;
		}
		case EnumChartType::Chart_DoughnutNested:
		{
			return BCGPChartCategory::BCGPChartDoughnutNested;
		}
		case EnumChartType::Chart_Funnel:
		{
			return BCGPChartCategory::BCGPChartFunnel;
		}
		case EnumChartType::Chart_Line:
		{
			return BCGPChartCategory::BCGPChartLine;
		}
		case EnumChartType::Chart_Pie:
		{
			return BCGPChartCategory::BCGPChartPie;
		}
		case EnumChartType::Chart_PolarArea:
		case EnumChartType::Chart_PolarLine:
		case EnumChartType::Chart_PolarScatter:
		case EnumChartType::Chart_RadarArea:
		case EnumChartType::Chart_RadarLine:
		{
			return BCGPChartCategory::BCGPChartPolar;
		}			
		case  EnumChartType::Chart_None:
		default:
		{
			BCGPChartCategory::BCGPChartDefault;
			break;
		}
	}
	return BCGPChartCategory::BCGPChartDefault;
}

//------------------------------------------------------------------------------
BCGPChartType Chart::GetBCGPChartType(EnumChartType eChartType)
{
	switch (eChartType)
	{
		case EnumChartType::Chart_AreaStacked:
		case EnumChartType::Chart_BarStacked:
		case EnumChartType::Chart_ColumnStacked:		
		{
			return BCGPChartType::BCGP_CT_STACKED;
		}
		case EnumChartType::Chart_AreaStacked100:
		case EnumChartType::Chart_BarStacked100:
		case EnumChartType::Chart_ColumnStacked100:
		{
			return BCGPChartType::BCGP_CT_100STACKED;
		}
		case EnumChartType::Chart_Area:
		case EnumChartType::Chart_Bar:
		case EnumChartType::Chart_Bubble:
		case EnumChartType::Chart_BubbleScatter:
		case EnumChartType::Chart_Column:
		case EnumChartType::Chart_Doughnut:
		case EnumChartType::Chart_DoughnutNested:
		case EnumChartType::Chart_Funnel:
		case EnumChartType::Chart_Line:
		case EnumChartType::Chart_Pie:
		case EnumChartType::Chart_PolarArea:
		case EnumChartType::Chart_PolarLine:
		case EnumChartType::Chart_PolarScatter:
		case EnumChartType::Chart_RadarArea:
		case EnumChartType::Chart_RadarLine:
		{
			return BCGPChartType::BCGP_CT_SIMPLE;
		}

		case EnumChartType::Chart_RangeBar:
		case EnumChartType::Chart_RangeColumn:
		case EnumChartType::Chart_RangeArea:
		{
			return BCGPChartType::BCGP_CT_RANGE;
		}
		case Chart_None:
		default:
		{
			return BCGPChartType::BCGP_CT_SIMPLE;
		}
	}

}

//------------------------------------------------------------------------------
BCGPChartFormatSeries::ChartCurveType Chart::GetBCGPChartCurveType(EnumChartStyle eChartStyle)
{
	switch (eChartStyle)
	{
		case EnumChartStyle::ChartStyle_None:
		{
			return BCGPChartFormatSeries::CCT_LINE;
		}
		case EnumChartStyle::ChartStyle_LineStep:
		{
			return BCGPChartFormatSeries::CCT_STEP;
		}
		case EnumChartStyle::ChartStyle_LineSmooth:
		{
			return BCGPChartFormatSeries::CCT_SPLINE;
		}
		default:
		{
			return BCGPChartFormatSeries::CCT_LINE;
		}
	}

}

//------------------------------------------------------------------------------
CString Chart::GetTooltip( int /*nPage = -1*/, CPoint point )
{
	DataStr sTip;
	CString spToolTip;
	CString spDesc;
	CBCGPPoint pt = CBCGPPoint(point.x - m_BaseRect.left, point.y - m_BaseRect.top);
	

	if (m_pTooltipExpr)
		m_pTooltipExpr->Eval(sTip);
	else
	{
		if(m_pChart)
		{ 
			m_pChart->SetDirty(FALSE);
			m_pChart->OnGetToolTip(pt, spToolTip, spDesc);
			sTip = spToolTip + _T(" ") + spDesc;
			sTip = sTip.Trim();
		}
	}

	return sTip;
}

//------------------------------------------------------------------------------
CString Chart::ChartTypeDescription(EnumChartType eType)
{
	for (int i = 0; i < EnumChartType::Chart_Wrong; i++)
		if (s_arChartTypes[i].m_eType == eType)
			return s_arChartTypes[i].m_sName;
	ASSERT(FALSE);
	return _T("");
}

//------------------------------------------------------------------------------
BOOL Chart::HasCategory(EnumChartType eType)
{
	for (int i = 0; i < EnumChartType::Chart_Wrong; i++)
		if (s_arChartTypes[i].m_eType == eType)
			return s_arChartTypes[i].m_bHasCategory;
	ASSERT(FALSE);
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL Chart::AllowMultipleSeries(EnumChartType eType)
{
	for (int i = 0; i < EnumChartType::Chart_Wrong; i++)
		if (s_arChartTypes[i].m_eType == eType)
			return s_arChartTypes[i].m_bMultipleSeries;
	ASSERT(FALSE);
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL Chart::AllowSeriesWithDiffType(EnumChartType eType)
{
	for (int i = 0; i < EnumChartType::Chart_Wrong; i++)
		if (s_arChartTypes[i].m_eType == eType)
			return s_arChartTypes[i].m_bSeriesWithDiffType;
	ASSERT(FALSE);
	return FALSE;
}

//------------------------------------------------------------------------------
EnumChartAxisType Chart::GetAxisType(EnumChartType eType)
{
	for (int i = 0; i < EnumChartType::Chart_Wrong; i++)
		if (s_arChartTypes[i].m_eType == eType)
			return s_arChartTypes[i].m_eAxisType;
	ASSERT(FALSE);
	return NoAxis;
}

//------------------------------------------------------------------------------
int Chart::GetDSNumber(EnumChartType eType)
{
	for (int i = 0; i < EnumChartType::Chart_Wrong; i++)
		if (s_arChartTypes[i].m_eType == eType)
			return s_arChartTypes[i].m_nDSNumber;
	ASSERT(FALSE);
	return -1;
}

//------------------------------------------------------------------------------
BOOL Chart::HasGroups(EnumChartType eType)
{
	for (int i = 0; i < EnumChartType::Chart_Wrong; i++)
		if (s_arChartTypes[i].m_eType == eType)
			return s_arChartTypes[i].m_bHasGroups;
	ASSERT(FALSE);
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL Chart::HasSeriesMultiColor(EnumChartType eType)
{
	for (int i = 0; i < EnumChartType::Chart_Wrong; i++)
		if (s_arChartTypes[i].m_eType == eType)
			return s_arChartTypes[i].m_bSeriesMultiColor;
	ASSERT(FALSE);
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL Chart::AllowLineStyle(EnumChartType eType)
{
	for (int i = 0; i < EnumChartType::Chart_Wrong; i++)
		if (s_arChartTypes[i].m_eType == eType)
			return s_arChartTypes[i].m_bLineStyle;
	ASSERT(FALSE);
	return FALSE;
}
//------------------------------------------------------------------------------
BCGPChartFormatSeries* Chart::GetColoredFormatSeries(CBCGPColor newColor, BCGPChartFormatSeries* formatSeries /*= NULL*/)
{
	if (!formatSeries)
		formatSeries = new BCGPChartFormatSeries();

	CBCGPColor newColorBase = newColor;
	CBCGPColor newColorLighter = newColor;
	CBCGPColor newColorMoreLighter = newColorLighter;
	CBCGPColor newColorDarker = newColor;

	newColorLighter.MakeLighter(1);
	while(!newColorMoreLighter.IsLight())
		newColorMoreLighter.MakeLighter(0.5);

	newColorDarker.MakeDarker(.5);

	CBCGPBrush brMain = CBCGPBrush(newColorBase);
	CBCGPBrush brLabelFill = CBCGPBrush(newColorLighter);
	CBCGPBrush brLine = CBCGPBrush(newColorDarker);
	CBCGPBrush brLightLabelFill = CBCGPBrush(newColorMoreLighter);

	formatSeries->SetSeriesFill(brMain);
	formatSeries->SetSeriesLineColor(brLine);

	if (newColorLighter.IsDark())
		formatSeries->SetDataLabelFill(brLightLabelFill);
	else
		formatSeries->SetDataLabelFill(brLabelFill);
	formatSeries->SetDataLabelLineColor(brLine);
	formatSeries->SetMarkerLineColor(brLine);
	formatSeries->m_dataLabelFormat.m_brTextColor = brLine;

	return formatSeries;
}

//------------------------------------------------------------------------------
BOOL Chart::IsCompatibleChartType(EnumChartType etype)
{
	if (m_eChartType == EnumChartType::Chart_None) return TRUE;
	if (etype <= 0) return TRUE;

	EnumChartAxisType eChartAxisType = GetAxisType();
	BOOL bChartHasCategory = HasCategory();
	int nChartDSNo = Chart::GetDSNumber(m_eChartType);
	
	//se il tipo non è compatibile con quello del grafico non permetto di assegnarlo alla serie
	return Chart::GetAxisType(etype) == eChartAxisType &&
		bChartHasCategory == Chart::HasCategory(etype) &&
		nChartDSNo == Chart::GetDSNumber(etype);
}


