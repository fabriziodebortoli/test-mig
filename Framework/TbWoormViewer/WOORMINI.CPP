#include "stdafx.h"

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\minmax.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\DllMod.h>

#include <TbGenlib\SettingsTableManager.h>
#include <TbGenlib\AddOnMng.h>
#include <TbGenlib\Baseapp.h>

#include "woormini.h"                            
#include "woormini.hjson" //JSON AUTOMATIC UPDATE
#include "TBPrintDialog.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define MAX_MOUSE_SENSIBILITY 30
#define MIN_SORT_GAP	3
#define MAX_SORT_GAP	100

// default dei settaggi
#define MAX_MOUSE_SENSIBILITY 30

#define	DEFAULT_HRATIO	18
#define	DEFAULT_VRATIO	18 
#define	MAX_GRID		50
#define	MIN_GRID		1

static const TCHAR szPixel[]			= _T("%d Pixel");
static const TCHAR szDraftFaceName[]	= _T("Arial");
static const TCHAR szFileSettings[]	= _T("Woorm.config");

//==============================================================================
//          Class WoormIni implementation
//==============================================================================
WoormIni::WoormIni()
{
	ReadWoormSettings ();
}
        
//------------------------------------------------------------------------------
WoormIni::WoormIni (const WoormIni& source)
	:
	m_Show    				(VALUE),
	m_CopyType				(TXT),
	m_bToClipboard			(TRUE),
	m_nGridX				(MIN_GRID),
	m_nGridY				(MIN_GRID),
	m_nSortGap				(MIN_SORT_GAP),
	m_nMouseSensibility		(1),
	m_bAddColumnBefore		(FALSE),
	m_bAutoTotal			(TRUE),
	m_bTotalOnNewPage		(TRUE),
	m_bResetOnNewPage		(TRUE),
	m_bAlwaysHidden			(FALSE),
	m_bMakeBackupFile		(TRUE),
	m_bEnableTrackCross		(source.m_bEnableTrackCross),
	m_bTrackInside			(source.m_bTrackInside),
	m_bShowGrid				(source.m_bShowGrid),
	m_bLineGrid				(source.m_bLineGrid),
	m_rgb_GridColor			(source.m_rgb_GridColor),
	m_bSizeInGridUnits		(source.m_bSizeInGridUnits),
	m_bSnapToGrid			(source.m_bSnapToGrid),
	m_bShowPrintableArea	(source.m_bShowPrintableArea),
	m_bTransparentCreate	(source.m_bTransparentCreate),
	m_bNoBorderCreate		(source.m_bNoBorderCreate),
	m_bNoLabelCreate		(source.m_bNoLabelCreate),
	m_bSortObjects			(source.m_bSortObjects),
	m_bShowMargins			(source.m_bShowMargins),
	m_bBottomAlign			(source.m_bBottomAlign),
	m_nHorzRatio			(source.m_nHorzRatio),
	m_nVertRatio			(source.m_nVertRatio),
	m_nTableRows			(source.m_nTableRows),
	m_bIncludeTotal			(source.m_bIncludeTotal),

	m_lfCharSet				(source.m_lfCharSet),
	m_lfOutPrecision		(source.m_lfOutPrecision),
	m_lfClipPrecision		(source.m_lfClipPrecision),
	m_lfQuality				(source.m_lfQuality),
	m_lfPitchAndFamily		(source.m_lfPitchAndFamily),
	m_strFaceName			(source.m_strFaceName),

	m_bOptimizedLineBreak	(source.m_bOptimizedLineBreak),
	m_ColumnWidthPercentage	(source.m_ColumnWidthPercentage),

	m_bCheckBarcodeSize					(source.m_bCheckBarcodeSize),
	m_bForceVerticalAlignLabelRelative	(source.m_bForceVerticalAlignLabelRelative),

	m_bShowReportTree				(source.m_bShowReportTree),
	m_bShowAllToolbars				(source.m_bShowAllToolbars),

	m_rgb_TrackLineColor		(source.m_rgb_TrackLineColor),
	m_nTrackLineSize			(source.m_nTrackLineSize),
	m_eTrackLineStyle			(source.m_eTrackLineStyle),
	m_bEnableNewObjectSelection	(source.m_bEnableNewObjectSelection),
	m_rgb_ObjectSelectionColor	(source.m_rgb_ObjectSelectionColor),
	m_nObjectSelectionSize		(source.m_nObjectSelectionSize),
	m_eObjectSelectionLineStyle	(source.m_eObjectSelectionLineStyle),
	m_rgb_HiddenBorderColor		(source.m_rgb_HiddenBorderColor),
	m_eHiddenBorderStyle		(source.m_eHiddenBorderStyle),

	m_iTimeAutoSave				(source.m_iTimeAutoSave)
{
}


//------------------------------------------------------------------------------
WoormIni& WoormIni::operator = (const WoormIni& source)
{
	m_Show					= source.m_Show;          
	m_CopyType				= source.m_CopyType;          
	m_bToClipboard			= source.m_bToClipboard;
	m_nGridX				= source.m_nGridX;
	m_nGridY				= source.m_nGridY;
	m_nSortGap				= source.m_nSortGap;
	m_nMouseSensibility		= source.m_nMouseSensibility;
	m_bAddColumnBefore		= source.m_bAddColumnBefore;
	m_bAutoTotal			= source.m_bAutoTotal;
	m_bTotalOnNewPage		= source.m_bTotalOnNewPage;
	m_bResetOnNewPage		= source.m_bResetOnNewPage;
	m_bAlwaysHidden			= source.m_bAlwaysHidden;
	m_bMakeBackupFile		= source.m_bMakeBackupFile;
	m_bEnableTrackCross		= source.m_bEnableTrackCross;
	m_bTrackInside			= source.m_bTrackInside;
	m_bShowGrid				= source.m_bShowGrid;
	m_bLineGrid				= source.m_bLineGrid,
	m_rgb_GridColor			= source.m_rgb_GridColor,
	m_bSizeInGridUnits		= source.m_bSizeInGridUnits;
	m_bSnapToGrid			= source.m_bSnapToGrid;
	m_bShowPrintableArea	= source.m_bShowPrintableArea;
	m_bTransparentCreate	= source.m_bTransparentCreate;
	m_bNoBorderCreate		= source.m_bNoBorderCreate;
	m_bNoLabelCreate		= source.m_bNoLabelCreate;
	m_bSortObjects			= source.m_bSortObjects;
	m_bShowMargins			= source.m_bShowMargins;
	m_bBottomAlign			= source.m_bBottomAlign;
	m_nHorzRatio			= source.m_nHorzRatio;
	m_nVertRatio			= source.m_nVertRatio;
	m_nTableRows			= source.m_nTableRows;
	m_bIncludeTotal			= source.m_bIncludeTotal;

	m_lfCharSet				= source.m_lfCharSet;
	m_lfOutPrecision		= source.m_lfOutPrecision;
	m_lfClipPrecision		= source.m_lfClipPrecision;
	m_lfQuality				= source.m_lfQuality;
	m_lfPitchAndFamily		= source.m_lfPitchAndFamily;
	m_strFaceName			= source.m_strFaceName;

	m_bOptimizedLineBreak	= source.m_bOptimizedLineBreak;
	m_ColumnWidthPercentage	= source.m_ColumnWidthPercentage;

	m_bCheckBarcodeSize					= source.m_bCheckBarcodeSize;
	m_bForceVerticalAlignLabelRelative	= source.m_bForceVerticalAlignLabelRelative;

	m_bShowReportTree				= source.m_bShowReportTree;
	m_bShowAllToolbars				= source.m_bShowAllToolbars;

	m_rgb_TrackLineColor		= source.m_rgb_TrackLineColor;
	m_nTrackLineSize			= source.m_nTrackLineSize;
	m_eTrackLineStyle			= source.m_eTrackLineStyle;
	m_bEnableNewObjectSelection = source.m_bEnableNewObjectSelection;
	m_rgb_ObjectSelectionColor	= source.m_rgb_ObjectSelectionColor;
	m_nObjectSelectionSize		= source.m_nObjectSelectionSize;
	m_eObjectSelectionLineStyle = source.m_eObjectSelectionLineStyle;
	m_rgb_HiddenBorderColor		= source.m_rgb_HiddenBorderColor;
	m_eHiddenBorderStyle		= source.m_eHiddenBorderStyle;

	return *this;
}

//------------------------------------------------------------------------------
int WoormIni::operator == (WoormIni& aIni)
{
	return
	(
		(m_Show					== aIni.m_Show)					&&
		(m_CopyType				== aIni.m_CopyType)				&&
		(m_bToClipboard			== aIni.m_bToClipboard)			&&
		(m_nGridX				== aIni.m_nGridX)				&&
		(m_nGridY				== aIni.m_nGridY)				&&
		(m_nSortGap				== aIni.m_nSortGap)				&&
		(m_nMouseSensibility	== aIni.m_nMouseSensibility)	&&
		(m_bAddColumnBefore		== aIni.m_bAddColumnBefore)		&&
		(m_bAutoTotal			== aIni.m_bAutoTotal)			&&
		(m_bTotalOnNewPage		== aIni.m_bTotalOnNewPage)		&&
		(m_bResetOnNewPage		== aIni.m_bResetOnNewPage)		&&
		(m_bAlwaysHidden		== aIni.m_bAlwaysHidden)		&&
		(m_bMakeBackupFile		== aIni.m_bMakeBackupFile)		&&
		(m_bEnableTrackCross	== aIni.m_bEnableTrackCross)	&&
		(m_bTrackInside			== aIni.m_bTrackInside)			&&
		(m_bShowGrid			== aIni.m_bShowGrid)			&&
		(m_bLineGrid			== aIni.m_bLineGrid)			&&
		(m_rgb_GridColor		== aIni.m_rgb_GridColor)		&&
		(m_bSizeInGridUnits		== aIni.m_bSizeInGridUnits)		&&
		(m_bSnapToGrid			== aIni.m_bSnapToGrid)			&&
		(m_bShowPrintableArea	== aIni.m_bShowPrintableArea)	&&
		(m_bTransparentCreate	== aIni.m_bTransparentCreate)	&&
		(m_bNoBorderCreate		== aIni.m_bNoBorderCreate)		&&
		(m_bNoLabelCreate		== aIni.m_bNoLabelCreate)		&&
		(m_bSortObjects			== aIni.m_bSortObjects)			&&
		(m_bShowMargins			== aIni.m_bShowMargins)			&&
		(m_bBottomAlign			== aIni.m_bBottomAlign)			&&
		(m_nHorzRatio			== aIni.m_nHorzRatio)			&&
		(m_nVertRatio			== aIni.m_nVertRatio)			&&
		(m_nTableRows			== aIni.m_nTableRows)			&&
		(m_bIncludeTotal		== aIni.m_bIncludeTotal)		&&

		(m_lfCharSet			== aIni.m_lfCharSet)			&&
		(m_lfOutPrecision		== aIni.m_lfOutPrecision)		&&
		(m_lfClipPrecision		== aIni.m_lfClipPrecision)		&&
		(m_lfQuality			== aIni.m_lfQuality)			&&
		(m_lfPitchAndFamily		== aIni.m_lfPitchAndFamily)		&&
		(m_strFaceName			== aIni.m_strFaceName)			&&

		(m_bOptimizedLineBreak	== aIni.m_bOptimizedLineBreak)	&&
		(m_ColumnWidthPercentage == aIni.m_ColumnWidthPercentage) &&

		(m_bCheckBarcodeSize				== aIni.m_bCheckBarcodeSize) &&
		(m_bForceVerticalAlignLabelRelative	== aIni.m_bForceVerticalAlignLabelRelative) &&

		(m_bShowReportTree				== aIni.m_bShowReportTree) &&
		(m_bShowAllToolbars				== aIni.m_bShowAllToolbars) &&

		(m_rgb_TrackLineColor			== aIni.m_rgb_TrackLineColor) &&
		(m_nTrackLineSize				== aIni.m_nTrackLineSize) &&
		(m_eTrackLineStyle				== aIni.m_eTrackLineStyle) &&
		(m_bEnableNewObjectSelection	== aIni.m_bEnableNewObjectSelection) &&
		(m_rgb_ObjectSelectionColor		== aIni.m_rgb_ObjectSelectionColor) &&
		(m_nObjectSelectionSize			== aIni.m_nObjectSelectionSize) &&
		(m_eObjectSelectionLineStyle	== aIni.m_eObjectSelectionLineStyle) &&
		(m_rgb_HiddenBorderColor		== aIni.m_rgb_HiddenBorderColor ) &&
		(m_eHiddenBorderStyle			== aIni.m_eHiddenBorderStyle)
	);
}

//------------------------------------------------------------------------------
int WoormIni::operator != (WoormIni& aIni)
{
	return !(*this == aIni);
}


//------------------------------------------------------------------------------
COLORREF WoormIni::GetObjectSelectionColor()
{
	if (m_bEnableNewObjectSelection)
		return m_rgb_ObjectSelectionColor;
	else 
		return RGB(255, 0, 255);
}

// legge dalla tabella di settaggi i valori da editare 
//------------------------------------------------------------------------------
void WoormIni::ReadWoormSettings ()
{
	CTBNamespace ns(_NS_MOD("Module.Framework.TbWoormViewer"));

	DataObj* pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szRectHRatio, DataInt(DEFAULT_HRATIO));
	m_nHorzRatio = pSetting ? *((DataInt*) pSetting) : DEFAULT_HRATIO;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szRectVRatio, DataInt(DEFAULT_VRATIO));
	m_nVertRatio = pSetting ? *((DataInt*) pSetting) : DEFAULT_VRATIO;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szGridX, DataInt(MIN_GRID));
	m_nGridX = pSetting	? *((DataInt*) pSetting) : MIN_GRID;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szGridY, DataInt(MIN_GRID));
	m_nGridY = pSetting ? *((DataInt*) pSetting) : MIN_GRID;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szSortGap,DataInt(MIN_SORT_GAP));
	m_nSortGap = pSetting ? *((DataInt*) pSetting) : MIN_SORT_GAP;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szDefaultTableRows, DataInt(30));
	m_nTableRows = pSetting	? *((DataInt*) pSetting) : 30;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szIncludeTotal, DataBool(TRUE));
	m_bIncludeTotal = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szMouseSensibility, DataInt(1));
	m_nMouseSensibility = pSetting ? *((DataInt*) pSetting) : 1;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szAddColumnBefore, DataBool(FALSE));
	m_bAddColumnBefore = pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szAutoColumnTotal, DataBool(TRUE));
	m_bAutoTotal = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szTotalOnNewPage, DataBool(TRUE));
	m_bTotalOnNewPage = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szResetOnNewPage, DataBool(TRUE));
	m_bResetOnNewPage = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szAlwaysHidden, DataBool(FALSE));
	m_bAlwaysHidden = pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szDoBakupFile, DataBool(TRUE));
	m_bMakeBackupFile = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szEnableTrackCross, DataBool(TRUE));
	m_bEnableTrackCross = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szTrackLineColor, DataLng(8421504));//rgb(128,128,128)
	m_rgb_TrackLineColor = pSetting ? (COLORREF) (LONG) *((DataLng*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szTrackLineSize, DataInt(1));
	m_nTrackLineSize = pSetting ? *((DataInt*)pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szTrackLineStyle, DataInt(3));
	m_eTrackLineStyle = pSetting ? *((DataInt*)pSetting) : DataInt(1);

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szEnableNewObjectSelection, DataBool(TRUE));
	m_bEnableNewObjectSelection = pSetting ? *((DataBool*)pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szObjectSelectionColor, DataLng(65280));
	m_rgb_ObjectSelectionColor = pSetting ? (COLORREF)(LONG)*((DataLng*)pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szObjectSelectionSize, DataInt(3));
	m_nObjectSelectionSize = pSetting ? *((DataInt*)pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szObjectSelectionStyle, DataInt(1));
	m_eObjectSelectionLineStyle = pSetting ? *((DataInt*)pSetting) : DataInt(1);

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szTrackInside, DataBool(FALSE));
	m_bTrackInside = pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szShowGrid, DataBool(TRUE));
	m_bShowGrid = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szLineGrid, DataBool(TRUE));
	m_bLineGrid = pSetting ? *((DataBool*)pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szGridColor, DataLng(15856113));
	m_rgb_GridColor = pSetting ? (COLORREF)(LONG)*((DataLng*)pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szHiddenBorderColor, DataLng(15453831));
	m_rgb_HiddenBorderColor = pSetting ? (COLORREF)(LONG)*((DataLng*)pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szHiddenBorderStyle, DataInt(15453831));
	m_eHiddenBorderStyle = pSetting ? *((DataInt*)pSetting) : DataInt(4);

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szSnapToGrid, DataBool(FALSE));
	m_bSnapToGrid = pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szSizeInGridUnits, DataBool(FALSE));
	m_bSizeInGridUnits = pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szShowPrintableArea, DataBool(TRUE));
	m_bShowPrintableArea = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szTransparentCreate, DataBool(FALSE));
	m_bTransparentCreate = pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szNoBorderCreate, DataBool(FALSE));
	m_bNoBorderCreate = pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szNoLabelCreate, DataBool(FALSE));
	m_bNoLabelCreate = pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szSortObjects, DataBool(TRUE));
	m_bSortObjects = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szNoShowMargins, DataBool(TRUE));
	m_bShowMargins = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szBottomAlign, DataBool(FALSE));
	m_bBottomAlign = pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szToClipboard, DataBool(TRUE));
	m_bToClipboard = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szShow, DataInt((int) VALUE));
	m_Show = pSetting ? (ShowType) (int) *((DataInt*) pSetting) : VALUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szCopyType, DataInt((int) TXT));
	m_CopyType = pSetting ? (CopyType) (int) *((DataInt*) pSetting) : TXT;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szOptimizedLineBreak, DataBool(TRUE));
	m_bOptimizedLineBreak = pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szColumnWidthPercentage, DataPerc(0.9));
	m_ColumnWidthPercentage = pSetting ? *((DataPerc*) pSetting) : 90;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szCheckBarcodeSize, DataBool(TRUE));
	m_bCheckBarcodeSize = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szForceVerticalAlignLabelRelative, DataBool(TRUE));
	m_bForceVerticalAlignLabelRelative = pSetting ? *((DataBool*) pSetting) : TRUE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szShowReportTree, DataBool(FALSE));
	m_bShowReportTree = pSetting ? *((DataBool*) pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szShowAllToolbars, DataBool(FALSE));
	m_bShowAllToolbars = pSetting ? *((DataBool*)pSetting) : FALSE;

	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szTimeAutoSave, DataInt(0));
	m_iTimeAutoSave = pSetting ? (int)*((DataInt*)pSetting) : DataInt(0);

	// usano una section apposita per facile individuazione
	pSetting = AfxGetSettingValue(ns, szDraftPrint, szCharSet, DataInt((int) ANSI_CHARSET));
	m_lfCharSet = pSetting ? (int) *((DataInt*) pSetting) : ANSI_CHARSET;

	pSetting = AfxGetSettingValue(ns, szDraftPrint, szOutPrecision, DataInt((int) OUT_STROKE_PRECIS));
	m_lfOutPrecision = pSetting ? (int) *((DataInt*) pSetting) : OUT_STROKE_PRECIS;

	pSetting = AfxGetSettingValue(ns, szDraftPrint, szClipPrecision, DataInt((int) CLIP_STROKE_PRECIS));
	m_lfClipPrecision = pSetting ? (int) *((DataInt*) pSetting) : CLIP_STROKE_PRECIS;

	pSetting = AfxGetSettingValue(ns, szDraftPrint, szQuality, DataInt((int) DRAFT_QUALITY));
	m_lfQuality = pSetting ? (int) *((DataInt*) pSetting) : DRAFT_QUALITY;

	pSetting = AfxGetSettingValue(ns, szDraftPrint, szPitchAndFamily, DataInt((int) FF_MODERN | FIXED_PITCH));
	m_lfPitchAndFamily = pSetting ? (int) *((DataInt*) pSetting) : FF_MODERN | FIXED_PITCH;

	pSetting = AfxGetSettingValue(ns, szDraftPrint, szFaceName, DataStr(szDraftFaceName));
	m_strFaceName = pSetting ? pSetting->Str() : szDraftFaceName;
}

//------------------------------------------------------------------------------
void WoormIni::WriteWoormSettings (const BOOL bSave /*FALSE*/)
{
	CTBNamespace ns(_T("Module.Framework.TbWoormViewer"));

	AfxSetSettingValue (ns, szWoormGeneralOptions, szRectHRatio,		m_nHorzRatio, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szRectVRatio,		m_nVertRatio, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szDefaultTableRows,	m_nTableRows, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szIncludeTotal,		m_bIncludeTotal, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szSortGap,			DataInt(m_nSortGap), szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szMouseSensibility,	m_nMouseSensibility, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szAddColumnBefore,	m_bAddColumnBefore, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szAutoColumnTotal,	m_bAutoTotal, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szTotalOnNewPage,	m_bTotalOnNewPage, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szResetOnNewPage,	m_bResetOnNewPage, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szAlwaysHidden,		m_bAlwaysHidden, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szDoBakupFile,		m_bMakeBackupFile, szFileSettings);
	//Grid
	AfxSetSettingValue(ns, szWoormGeneralOptions, szShowGrid,					m_bShowGrid, szFileSettings);
	AfxSetSettingValue(ns, szWoormGeneralOptions, szGridColor,					DataLng(m_rgb_GridColor), szFileSettings);
	AfxSetSettingValue(ns, szWoormGeneralOptions, szGridX,						DataInt(m_nGridX), szFileSettings);
	AfxSetSettingValue(ns, szWoormGeneralOptions, szGridY,						DataInt(m_nGridY), szFileSettings);
	AfxSetSettingValue(ns, szWoormGeneralOptions, szLineGrid,					m_bLineGrid, szFileSettings);
	//Tracking Line
	AfxSetSettingValue (ns, szWoormGeneralOptions, szEnableTrackCross,			m_bEnableTrackCross, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szTrackLineColor,			DataLng(m_rgb_TrackLineColor), szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szTrackLineSize,				DataInt(m_nTrackLineSize), szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szTrackLineStyle,			DataInt(m_eTrackLineStyle), szFileSettings);
	//Object Selection
	AfxSetSettingValue (ns, szWoormGeneralOptions, szEnableNewObjectSelection,	m_bEnableNewObjectSelection, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szObjectSelectionColor,		DataLng(m_rgb_ObjectSelectionColor), szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szObjectSelectionSize,		DataInt(m_nObjectSelectionSize), szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szObjectSelectionStyle,		DataInt(m_eObjectSelectionLineStyle), szFileSettings);
	//Hidden border
	AfxSetSettingValue(ns, szWoormGeneralOptions, szHiddenBorderColor,			DataLng(m_rgb_HiddenBorderColor), szFileSettings);
	AfxSetSettingValue(ns, szWoormGeneralOptions, szHiddenBorderStyle,			DataInt(m_eHiddenBorderStyle), szFileSettings);

	AfxSetSettingValue (ns, szWoormGeneralOptions, szTrackInside,		m_bTrackInside, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szSnapToGrid,		m_bSnapToGrid, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szSizeInGridUnits,	m_bSizeInGridUnits, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szShowPrintableArea,	m_bShowPrintableArea, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szTransparentCreate,	m_bTransparentCreate, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szNoBorderCreate,	m_bNoBorderCreate, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szNoLabelCreate,		m_bNoLabelCreate, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szSortObjects,		m_bSortObjects, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szNoShowMargins,		m_bShowMargins, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szBottomAlign,		m_bBottomAlign, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szToClipboard,		m_bToClipboard, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szShow,				DataInt((int) m_Show), szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szCopyType,			DataInt((int) m_CopyType), szFileSettings);

	AfxSetSettingValue (ns, szWoormGeneralOptions, szOptimizedLineBreak,	m_bOptimizedLineBreak, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szColumnWidthPercentage,	m_ColumnWidthPercentage, szFileSettings);
	
	AfxSetSettingValue (ns, szWoormGeneralOptions, szCheckBarcodeSize,					m_bCheckBarcodeSize, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szForceVerticalAlignLabelRelative,	m_bForceVerticalAlignLabelRelative, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szShowReportTree,					m_bShowReportTree, szFileSettings);
	AfxSetSettingValue (ns, szWoormGeneralOptions, szShowAllToolbars,					m_bShowAllToolbars, szFileSettings);
	AfxSetSettingValue(ns,  szWoormGeneralOptions, szTimeAutoSave,						DataInt(m_iTimeAutoSave), szFileSettings);

	//AfxSetSettingValue(ns, szWoormGeneralOptions, szShowAdvancedForms, m_bShowReportTree, szFileSettings);

	AfxSetSettingValue (ns, szDraftPrint, szCharSet,			DataInt(m_lfCharSet), szFileSettings);
	AfxSetSettingValue (ns, szDraftPrint, szOutPrecision,		DataInt(m_lfOutPrecision), szFileSettings);
	AfxSetSettingValue (ns, szDraftPrint, szClipPrecision,		DataInt(m_lfClipPrecision), szFileSettings);
	AfxSetSettingValue (ns, szDraftPrint, szQuality,			DataInt(m_lfQuality), szFileSettings);
	AfxSetSettingValue (ns, szDraftPrint, szPitchAndFamily,		DataInt(m_lfPitchAndFamily), szFileSettings);
	AfxSetSettingValue (ns, szDraftPrint, szFaceName,			DataStr(m_strFaceName), szFileSettings);

	//if (bSave)
	AfxSaveSettingsFile(ns, szFileSettings, bSave);
}

//==============================================================================
//          Class CWoormBaseDialog implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CWoormBaseDialog, CParsedDialog)
BEGIN_MESSAGE_MAP(CWoormBaseDialog, CParsedDialog)
	ON_WM_SHOWWINDOW()
	ON_WM_ERASEBKGND()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//------------------------------------------------------------------------------
CWoormBaseDialog::CWoormBaseDialog(UINT nIdd, CWnd* pWndParent, const CString& sName)
	:
	CParsedDialog(nIdd, pWndParent, sName)
{

}

//------------------------------------------------------------------------------
void CWoormBaseDialog::OnShowWindow(BOOL bShow, UINT nStatus)
{
	if (m_pOwner && bShow && !nStatus && !IsVisualManagerStyle())
	{
		ApplyTBVisualManager();
		SetDefaultFont();
	}
}


// ripristiniamo la OnEraseBackGround originale pre modifica del flickering
// perche' queste finestre hanno una gestione specifica per i control di RC
// che nel caso di __super danno dei problemi di PAINT su i check e bool button
//-----------------------------------------------------------------------------
BOOL CWoormBaseDialog::OnEraseBkgnd(CDC* pDC)
{
	if (DoEraseBkgnd(pDC))
		return TRUE;

	return __super::OnEraseBkgnd(pDC);
}


//==============================================================================
//          Class CWoormIniDlg implementation
//==============================================================================

BEGIN_MESSAGE_MAP(CWoormIniDlg, CWoormBaseDialog)
	//{{AFX_MSG_MAP(CWoormIniDlg)
	ON_EN_VALUE_CHANGED	(IDC_GRID_X,		OnChangeGridX)
	ON_EN_VALUE_CHANGED	(IDC_GRID_Y,		OnChangeGridY)
	ON_BN_CLICKED		(IDC_GRID_OLD_PAPER, OnCheckGridOldPaper)
	ON_BN_CLICKED		(IDC_DRAFT_FONT_SEL, OnDraftFontSelection)
	ON_BN_CLICKED		(IDC_OPTIMIZED_LINE_BREAK, OnOptimizedLineBreakClicked)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//------------------------------------------------------------------------------
CWoormIniDlg::CWoormIniDlg (CWnd* aParent)
	:
	CWoormBaseDialog		(IDD_WOORM_INI, aParent),
	m_HRatioEdit			(BTN_SPIN_ID),
	m_VRatioEdit			(BTN_SPIN_ID),
	m_GridXEdit				(BTN_SPIN_ID),
	m_GridYEdit				(BTN_SPIN_ID),
	m_SortGapEdit			(BTN_SPIN_ID),
	m_MouseSensibilityEdit	(BTN_SPIN_ID),
	m_TableRowsEdit			(BTN_SPIN_ID)
{
}

//-----------------------------------------------------------------------------
BOOL CWoormIniDlg::InitMetricEdit(int nID, int nValue, int nMin, int nMax)
{
	CMetricEdit& edit = GetMetricEdit(nID);

	if (!edit.SubclassEdit(nID, this)) return FALSE;
                                                 
	// input in mm with two decimal digit                                                 
	edit.SetRange(nMin, nMax);
	edit.SetValue(nValue);

	return TRUE;
}

//------------------------------------------------------------------------------
void CWoormIniDlg::OnChangeGridX()
{
	GetDlgItem(IDC_GRID_X_PIXEL)->SetWindowText(cwsprintf(szPixel, m_GridXEdit.GetValue()));
}

//------------------------------------------------------------------------------
void CWoormIniDlg::OnChangeGridY()
{
	GetDlgItem(IDC_GRID_Y_PIXEL)->SetWindowText(cwsprintf(szPixel, m_GridYEdit.GetValue()));
}

// formato fanfold standard
//------------------------------------------------------------------------------
void CWoormIniDlg::OnCheckGridOldPaper()
{
	m_ini.m_nGridX = 10;	// 1/8"
	m_ini.m_nGridY = 16;	// 1/6"

	m_GridXEdit.SetValue(m_ini.m_nGridX);
	m_GridYEdit.SetValue(m_ini.m_nGridY);

	// aggiorna il display in pixel
	OnChangeGridX();
	OnChangeGridY();
}

//-----------------------------------------------------------------------------
CMetricEdit& CWoormIniDlg::GetMetricEdit(int nID)
{
	if (nID == IDC_INI_HRATIO)
		return m_HRatioEdit;
	if (nID == IDC_INI_VRATIO)
		return m_VRatioEdit;
	if (nID == IDC_GRID_X)
		return m_GridXEdit;
	if (nID == IDC_GRID_Y)
		return m_GridYEdit;
	if (nID == IDC_SORT_GAP)
		return m_SortGapEdit;

	ASSERT(FALSE);
	return m_HRatioEdit;
}

//-----------------------------------------------------------------------------
BOOL CWoormIniDlg::InitIntEdit(int nID, int nValue, int nMin, int nMax)
{
	CIntEdit& edit = GetIntEdit(nID);

	if (!edit.SubclassEdit(nID, this)) return FALSE;

	edit.SetRange(nMin, nMax);
	edit.SetValue(nValue);

	return TRUE;
}

//-----------------------------------------------------------------------------
CIntEdit& CWoormIniDlg::GetIntEdit(int nID)
{
	if (nID == IDC_MOUSE_SENSIBILITY) 
		return m_MouseSensibilityEdit;

	return m_TableRowsEdit;
}

//------------------------------------------------------------------------------
void CWoormIniDlg::OnOptimizedLineBreakClicked()
{
	GetDlgItem(IDC_COLUMN_WIDTH_PERCENTAGE)->EnableWindow
		(((CButton*)GetDlgItem(IDC_OPTIMIZED_LINE_BREAK))->GetCheck() == 1);
}

//------------------------------------------------------------------------------
BOOL CWoormIniDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	// showing styles
	CheckDlgButton(IDC_SHOW_ID,			m_ini.m_Show == WoormIni::ID);
	CheckDlgButton(IDC_SHOW_NAME,		m_ini.m_Show == WoormIni::NAME);
	CheckDlgButton(IDC_SHOW_VALUE,		m_ini.m_Show == WoormIni::VALUE);

	// column default
	CheckDlgButton(IDC_ADD_COLUMN_BEFORE,   m_ini.m_bAddColumnBefore);

	// column total defaults
	CheckDlgButton(IDC_AUTO_TOTAL,          m_ini.m_bAutoTotal);
	CheckDlgButton(IDC_TOTAL_ON_NEW_PAGE,   m_ini.m_bTotalOnNewPage);
	CheckDlgButton(IDC_RESET_ON_NEW_PAGE,   m_ini.m_bResetOnNewPage);
	CheckDlgButton(IDC_ALWAYS_HIDDEN,       m_ini.m_bAlwaysHidden);
	CheckDlgButton(IDC_MAKE_BACKUP_FILE,    m_ini.m_bMakeBackupFile);
	CheckDlgButton(IDC_ENABLE_TRACK_CROSS,  m_ini.m_bEnableTrackCross);
	CheckDlgButton(IDC_TRACK_INSIDE,		m_ini.m_bTrackInside);
	CheckDlgButton(IDC_SHOW_GRID,			m_ini.m_bShowGrid);
	CheckDlgButton(IDC_SIZE_IN_GRID_UNITS,	m_ini.m_bSizeInGridUnits);
	CheckDlgButton(IDC_SNAP_TO_GRID,		m_ini.m_bSnapToGrid);
	CheckDlgButton(IDC_SHOW_PRINTABLE_AREA,	m_ini.m_bShowPrintableArea);
	CheckDlgButton(IDC_TRANSAPARENT_CREATE,	m_ini.m_bTransparentCreate);
	CheckDlgButton(IDC_NO_BORDER_CREATE,	m_ini.m_bNoBorderCreate);
	CheckDlgButton(IDC_NO_LABEL_CREATE,		m_ini.m_bNoLabelCreate);
	CheckDlgButton(IDC_SORT_OBJECTS,		m_ini.m_bSortObjects);
	CheckDlgButton(IDC_SHOW_MARGINS,		m_ini.m_bShowMargins);
	CheckDlgButton(IDC_BOTTOM_ALIGN,		m_ini.m_bBottomAlign);
	
	//OPTIMIZED_LINE_BREAK
	CheckDlgButton(IDC_OPTIMIZED_LINE_BREAK, m_ini.m_bOptimizedLineBreak);
	if (!m_edtColumnWidthPercentage.SubclassEdit(IDC_COLUMN_WIDTH_PERCENTAGE, this)) 
		return FALSE;
	m_edtColumnWidthPercentage.SetRange(50,100);
	m_edtColumnWidthPercentage.SetValue(m_ini.m_ColumnWidthPercentage);
	m_edtColumnWidthPercentage.EnableWindow
		(((CButton*)GetDlgItem(IDC_OPTIMIZED_LINE_BREAK))->GetCheck() == 1);

	CheckDlgButton(IDC_CHK_CHECK_BARCODE_SIZE,	m_ini.m_bCheckBarcodeSize);
	CheckDlgButton(IDC_CHK_FORCE_VALIGN_LREL,	m_ini.m_bForceVerticalAlignLabelRelative);
	CheckDlgButton(IDC_CHK_SHOW_ADVANCED_FORMS,	m_ini.m_bShowReportTree);

	if 	(
		!InitMetricEdit	(IDC_INI_HRATIO,    m_ini.m_nHorzRatio,  0, MAX_RATIO)		||
		!InitMetricEdit	(IDC_INI_VRATIO,    m_ini.m_nVertRatio,  0, MAX_RATIO)		||
		!InitMetricEdit	(IDC_GRID_X,        m_ini.m_nGridX,      0,	MAX_GRID)		||
		!InitMetricEdit	(IDC_GRID_Y,        m_ini.m_nGridY,      0,	MAX_GRID)		||
		!InitMetricEdit	(IDC_SORT_GAP,      m_ini.m_nSortGap,    0,	MAX_SORT_GAP)	||
		!InitIntEdit	(IDC_TABLE_ROWS,    m_ini.m_nTableRows,  2, 900)			||
		!InitIntEdit	(IDC_MOUSE_SENSIBILITY,m_ini.m_nMouseSensibility, 1, MAX_MOUSE_SENSIBILITY)
		)
	{
		EndDialog(IDCANCEL);
		return FALSE;
	}

	CheckDlgButton(IDC_INCLUDE_TOTAL, m_ini.m_bIncludeTotal);

	OnChangeGridX();
	OnChangeGridY();

	return TRUE;  // return TRUE  unless you set the focus to a control
}

//------------------------------------------------------------------------------
void CWoormIniDlg::OnOK()
{
                       
    if (!CheckForm())
    	return;
    	
	// showing styles
		 if (IsDlgButtonChecked(IDC_SHOW_ID))       m_ini.m_Show = WoormIni::ID;
	else if (IsDlgButtonChecked(IDC_SHOW_NAME))     m_ini.m_Show = WoormIni::NAME;
	else if (IsDlgButtonChecked(IDC_SHOW_VALUE))    m_ini.m_Show = WoormIni::VALUE;
	
	// column default
	m_ini.m_bAddColumnBefore = IsDlgButtonChecked(IDC_ADD_COLUMN_BEFORE);

	// column total defaults
	m_ini.m_bAutoTotal       = IsDlgButtonChecked(IDC_AUTO_TOTAL);

	m_ini.m_bTotalOnNewPage  = IsDlgButtonChecked(IDC_TOTAL_ON_NEW_PAGE);
	m_ini.m_bResetOnNewPage  = IsDlgButtonChecked(IDC_RESET_ON_NEW_PAGE);
	m_ini.m_bAlwaysHidden    = IsDlgButtonChecked(IDC_ALWAYS_HIDDEN);
	m_ini.m_bMakeBackupFile  = IsDlgButtonChecked(IDC_MAKE_BACKUP_FILE);
	m_ini.m_bEnableTrackCross= IsDlgButtonChecked(IDC_ENABLE_TRACK_CROSS);
	m_ini.m_bTrackInside	 = IsDlgButtonChecked(IDC_TRACK_INSIDE);
	m_ini.m_bShowGrid		 = IsDlgButtonChecked(IDC_SHOW_GRID);
	m_ini.m_bSizeInGridUnits = IsDlgButtonChecked(IDC_SIZE_IN_GRID_UNITS);
	m_ini.m_bSnapToGrid		 = IsDlgButtonChecked(IDC_SNAP_TO_GRID);

	m_ini.m_bShowPrintableArea	= IsDlgButtonChecked(IDC_SHOW_PRINTABLE_AREA);
	m_ini.m_bTransparentCreate	= IsDlgButtonChecked(IDC_TRANSAPARENT_CREATE);
	m_ini.m_bNoBorderCreate		= IsDlgButtonChecked(IDC_NO_BORDER_CREATE);
	m_ini.m_bNoLabelCreate		= IsDlgButtonChecked(IDC_NO_LABEL_CREATE);
	m_ini.m_bSortObjects		= IsDlgButtonChecked(IDC_SORT_OBJECTS);
	m_ini.m_bShowMargins		= IsDlgButtonChecked(IDC_SHOW_MARGINS);
	m_ini.m_bBottomAlign		= IsDlgButtonChecked(IDC_BOTTOM_ALIGN);	

	// ratios
	m_ini.m_nHorzRatio	= GetMetricEdit(IDC_INI_HRATIO).GetValue();
	m_ini.m_nVertRatio	= GetMetricEdit(IDC_INI_VRATIO).GetValue();

	// tables Grids ( a causa degli arrotondamenti controllo che stia sopra il pixel
	m_ini.m_nGridX		= Max(GetMetricEdit(IDC_GRID_X).	GetValue(), MIN_GRID);
	m_ini.m_nGridY		= Max(GetMetricEdit(IDC_GRID_Y).	GetValue(), MIN_GRID);
	m_ini.m_nSortGap	= Max(GetMetricEdit(IDC_SORT_GAP).	GetValue(), MIN_SORT_GAP);

	// tables default
	m_ini.m_nMouseSensibility	= GetIntEdit(IDC_MOUSE_SENSIBILITY).GetValue();
	m_ini.m_nTableRows			= GetIntEdit(IDC_TABLE_ROWS).GetValue();

	m_ini.m_bIncludeTotal = IsDlgButtonChecked(IDC_INCLUDE_TOTAL);       

	m_ini.m_bOptimizedLineBreak = IsDlgButtonChecked(IDC_OPTIMIZED_LINE_BREAK);
	m_edtColumnWidthPercentage.GetValue(m_ini.m_ColumnWidthPercentage);

	m_ini.m_bCheckBarcodeSize					= IsDlgButtonChecked(IDC_CHK_CHECK_BARCODE_SIZE);       
	m_ini.m_bForceVerticalAlignLabelRelative	= IsDlgButtonChecked(IDC_CHK_FORCE_VALIGN_LREL);       
	//m_ini.m_bShowReportTree					= IsDlgButtonChecked(IDC_CHK_SHOW_ADVANCED_FORMS);    

	// write modified value and exit
	m_ini.WriteWoormSettings(TRUE);

	EndDialog(IDOK);
}


// stabilisce gli stili da usare (vedi help su CHOOSEFONT)
//-----------------------------------------------------------------------------
void CWoormIniDlg::OnDraftFontSelection()
{
	CDC* pDC = NULL;
	CDC dcPrint;
	DWORD dwFlags = CF_EFFECTS | CF_PRINTERFONTS | CF_INITTOLOGFONTSTRUCT;

	// la richiesta alla stampante puo' essere un processo lungo
	BeginWaitCursor();

	// magic to get printer dialog that would be used if we were printing!
	HDC hdcPrint = NULL;
	CTBPrintDialog dlgPrint(FALSE);

	if	(
			!AfxGetApp()->GetPrinterDeviceDefaults(&dlgPrint.m_pd) ||
			NULL == (hdcPrint = dlgPrint.CreatePrinterDC())
		)
	{
		EndWaitCursor();
		return;
	}

	dcPrint.Attach(hdcPrint);
	pDC = &dcPrint;


    // inizializza con il default draft font ("Arial 10" sostituisce precedente "Courier 12" per problemi generazione pdf)
	LOGFONT lf;
	lf.lfHeight			= - ::MulDiv(10, GetDeviceCaps(pDC->m_hDC, LOGPIXELSY), 72); //-16;  // default font corpo 12
	lf.lfWidth			= 0;
	lf.lfEscapement		= 0;
	lf.lfOrientation	= 0;
	lf.lfWeight			= FW_NORMAL; 
	lf.lfItalic			= 0;
	lf.lfUnderline		= 0;
	lf.lfStrikeOut		= 0;
	lf.lfCharSet		= m_ini.m_lfCharSet;
	lf.lfOutPrecision	= m_ini.m_lfOutPrecision;
	lf.lfClipPrecision	= m_ini.m_lfClipPrecision;
	lf.lfQuality		= m_ini.m_lfQuality;
	lf.lfPitchAndFamily	= m_ini.m_lfPitchAndFamily;
	TB_TCSCPY(lf.lfFaceName, m_ini.m_strFaceName);

    // CFontDialog use device point
	CFontDialog dialog(&lf, dwFlags, pDC, this);

	// this is a trick for set style combo in font dialog (afx bugs)
	dialog.m_cf.Flags = dialog.m_cf.Flags & ~CF_USESTYLE;
	dialog.m_cf.rgbColors = RGB(0,0,0);

	EndWaitCursor();

	if (dialog.DoModal() == IDOK)
	{
		if (!IsTrueTypeFont(&lf))
		{
			AfxMessageBox(_TB("Only TrueType Font are allowed."));
			return;
		}
		m_ini.m_lfCharSet			= lf.lfCharSet;
		m_ini.m_lfOutPrecision		= lf.lfOutPrecision;
		m_ini.m_lfClipPrecision		= lf.lfClipPrecision;
		m_ini.m_lfQuality			= lf.lfQuality;
		m_ini.m_lfPitchAndFamily	= lf.lfPitchAndFamily;
		m_ini.m_strFaceName			= lf.lfFaceName;
	}
}
// Testa se il font e' trueType perche se nn lo e' si pianta la stampa PDF
//-----------------------------------------------------------------------------
BOOL CWoormIniDlg::IsTrueTypeFont(LOGFONT* lf)
{
	CFont font;
	
	font.CreateFontIndirect(lf); 
	CDC		dc; dc.CreateIC(_T("DISPLAY"), NULL, NULL, NULL);
	dc.SelectObject(&font);

	TEXTMETRIC tm;
	dc.GetTextMetrics(&tm);

	if ((tm).tmPitchAndFamily == 39)
		return true;

	return false;

}
	



