
#include "stdafx.h"

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include <TbGeneric\globals.h>  
#include <TbGeneric\dibitmap.h>
#include <TbGeneric\spin.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\FontsTable.h>
#include <TBGeneric\WndObjDescription.h>
#include <TbGeneric\VisualStylesXP.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\TBThemeManager.h>

#include <TbGenlib\tabcore.h>	
#include <TbGenlib\generic.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\HyperLink.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\Tfxdatatip.h>
#include <TbGenlib\PARSLBX.H>
#include <TbGenlib\PARSEDT.H>
#include <TbGenlib\AddressEdit.h>
#include <TbGenlib\MicroareaVisualManager.h>

#include <TbOledb\sqlcatalog.h>
#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>

#include <TbGenlib\generic.hjson> //JSON AUTOMATIC UPDATE

#include "bodyedit.h"
#include "tabber.h"
#include "hotlink.h"
#include "formmng.h"

#include "TBExtBEDropTarget.h"
#include "TBExtBEOleDataSource.h"
#include "TBExtBEOleDropSource.h"
#include "TBEDataCodec.h"
#include "TbExtBEDefaultCodec.h"
#include "DbtTreeEdit.h"
#include "BodyEditTree.h"
#include "SlaveViewContainer.h"
#include "bodyedit.hjson" //JSON AUTOMATIC UPDATE
#include "TileDialog.h"
#include "CAddressDlg.h"


//includere come ultimo include all'inizio del cpp
#include"begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//Numero di righe massime mostrate per volta via web. Oltre questo numero il bodyedit "pagina",
//cioe' viene mandata una "pagina" per volta, con  WEB_PAGE_SIZE righe
#define WEB_PAGE_SIZE 45

#define EMPTY_COLOR 0xffffffff
#define CHECK_BOX_WIDTH 10
#define CALENDAR_BOX_WIDTH 17

//-----------------------------------------------------------------------------
void CWndBodyColumnDescription::SerializeJson(CJsonSerializer& strJson)
{

	SERIALIZE_BOOL(m_bSort, szJsonSort, false);
	SERIALIZE_BOOL(m_bHasFooterDescr, szJsonHasFooter, false);
	SERIALIZE_BOOL(m_bStatusHidden, szJsonStatusHidden, false);
	SERIALIZE_BOOL(m_bStatusGrayed, szJsonStatusGrayed, false);
	SERIALIZE_BOOL(m_bStatusNoChange_Grayed, szJsonStatusNoChange_Grayed, false);
	SERIALIZE_BOOL(m_bStatusNoChange_Hidden, szJsonStatusNoChange_Hidden, false);
	SERIALIZE_BOOL(m_bStatusLocked, szJsonStatusLocked, false);
	SERIALIZE_BOOL(m_bStatusSortedAsc, szJsonStatusSortedAsc, false);
	SERIALIZE_BOOL(m_bStatusSortedDes, szJsonStatusSortedDes, false);
	SERIALIZE_BOOL(m_bAllowEnlarge, szJsonAllowEnlarge, false);
	SERIALIZE_STRING(m_strTooltip, szJsonTooltip);

	if (m_pItemSourceDescri)
	{
		m_pItemSourceDescri->SerializeJson(strJson);
	}
	__super::SerializeJson(strJson);

}

//-----------------------------------------------------------------------------
void CWndBodyColumnDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	PARSE_BOOL(m_bSort, szJsonSort);
	PARSE_BOOL(m_bHasFooterDescr, szJsonHasFooter);
	PARSE_BOOL(m_bStatusHidden, szJsonStatusHidden);
	PARSE_BOOL(m_bStatusGrayed, szJsonStatusGrayed);
	PARSE_BOOL(m_bStatusNoChange_Grayed, szJsonStatusNoChange_Grayed);
	PARSE_BOOL(m_bStatusNoChange_Hidden, szJsonStatusNoChange_Hidden);
	PARSE_BOOL(m_bStatusLocked, szJsonStatusLocked);
	PARSE_BOOL(m_bStatusSortedAsc, szJsonStatusSortedAsc);
	PARSE_BOOL(m_bStatusSortedDes, szJsonStatusSortedDes);
	PARSE_BOOL(m_bAllowEnlarge, szJsonAllowEnlarge);
	PARSE_STRING(m_strTooltip, szJsonTooltip);

	if (parser.Has(szJsonItemSource))
	{
		if (!m_pItemSourceDescri)
		{
			m_pItemSourceDescri = new CItemSourceDescription();
			m_pItemSourceDescri->SetParent(this);
		}
		m_pItemSourceDescri->ParseJson(parser);
	}
}
//-----------------------------------------------------------------------------
void CWndBodyColumnDescription::EvaluateExpressions(CJsonContextObj * pJsonContext, bool deep /*= true*/)
{
	__super::EvaluateExpressions(pJsonContext, deep);
	if (m_pItemSourceDescri)
		pJsonContext->Evaluate(m_pItemSourceDescri->m_Expressions, this);

	if (m_ControlBehaviourDescri)
		pJsonContext->Evaluate(m_ControlBehaviourDescri->m_Expressions, this);
}
//-----------------------------------------------------------------------------
void CWndBodyColumnDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	m_bSort = ((CWndBodyColumnDescription*)pDesc)->m_bSort;
	m_bHasFooterDescr = ((CWndBodyColumnDescription*)pDesc)->m_bHasFooterDescr;
	m_bStatusHidden = ((CWndBodyColumnDescription*)pDesc)->m_bStatusHidden;
	m_bStatusGrayed = ((CWndBodyColumnDescription*)pDesc)->m_bStatusGrayed;
	m_bStatusNoChange_Grayed = ((CWndBodyColumnDescription*)pDesc)->m_bStatusNoChange_Grayed;
	m_bStatusNoChange_Hidden = ((CWndBodyColumnDescription*)pDesc)->m_bStatusNoChange_Hidden;
	m_bStatusLocked = ((CWndBodyColumnDescription*)pDesc)->m_bStatusLocked;
	m_bStatusSortedAsc = ((CWndBodyColumnDescription*)pDesc)->m_bStatusSortedAsc;
	m_bStatusSortedDes = ((CWndBodyColumnDescription*)pDesc)->m_bStatusSortedDes;
	m_bAllowEnlarge = ((CWndBodyColumnDescription*)pDesc)->m_bAllowEnlarge;
	m_strTooltip = ((CWndBodyColumnDescription*)pDesc)->m_strTooltip;

	if (((CWndBodyColumnDescription*)pDesc)->m_pItemSourceDescri)
		m_pItemSourceDescri = ((CWndBodyColumnDescription*)pDesc)->m_pItemSourceDescri->Clone();

	if (((CWndBodyColumnDescription*)pDesc)->m_ControlBehaviourDescri)
		m_ControlBehaviourDescri = ((CWndBodyColumnDescription*)pDesc)->m_ControlBehaviourDescri->Clone();
}

IMPLEMENT_DYNCREATE(CWndBodyColumnDescription, CWndObjDescription)

//-----------------------------------------------------------------------------
CWndBodyElementDescription::CWndBodyElementDescription(void)
{
}

//-----------------------------------------------------------------------------
CWndBodyElementDescription::CWndBodyElementDescription(CWndObjDescription* pParent)
	: CWndObjDescription(pParent)
{
}

//-----------------------------------------------------------------------------
void CWndBodyElementDescription::SerializeJson(CJsonSerializer& strJson)
{
	__super::SerializeJson(strJson);

}

//-----------------------------------------------------------------------------
void CWndBodyElementDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	
	if (parser.Has(szJsonTextAlign))
		m_nTextAlign = parser.ReadBool(szJsonTextAlign);

	if (parser.Has(szJsonBkgColor))
		m_crBkgColor = parser.ReadInt(szJsonBkgColor);
	if (parser.Has(szJsonForeColor))
		m_crTextColor = parser.ReadInt(szJsonForeColor);

}
//-----------------------------------------------------------------------------
void CWndBodyElementDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_bActiveCell = ((CWndBodyElementDescription*)pDesc)->m_bActiveCell;
	m_bActiveRow = ((CWndBodyElementDescription*)pDesc)->m_bActiveRow;
	m_nTextAlign = ((CWndBodyElementDescription*)pDesc)->m_nTextAlign;
	m_bIsCheckBox = ((CWndBodyElementDescription*)pDesc)->m_bIsCheckBox;
	m_bCheckBoxValue = ((CWndBodyElementDescription*)pDesc)->m_bCheckBoxValue;
	m_bIsHyperLink = ((CWndBodyElementDescription*)pDesc)->m_bIsHyperLink;
	m_crBkgColor = ((CWndBodyElementDescription*)pDesc)->m_crBkgColor;
	m_crTextColor = ((CWndBodyElementDescription*)pDesc)->m_crTextColor;
	m_nRow = ((CWndBodyElementDescription*)pDesc)->m_nRow;
	m_nCol = ((CWndBodyElementDescription*)pDesc)->m_nCol;
}

IMPLEMENT_DYNCREATE(CWndBodyElementDescription, CWndObjDescription)
REGISTER_WND_OBJ_CLASS(CWndBodyColumnDescription, ColTitle)
//=============================================================================
//Descrizione della cella che visualizza il nodo nel body edit ad albero
//=============================================================================

//-----------------------------------------------------------------------------
CWndBodyTreeElementDescription::CWndBodyTreeElementDescription(void)
{
}

//-----------------------------------------------------------------------------
CWndBodyTreeElementDescription::CWndBodyTreeElementDescription(CWndObjDescription* pParent)
	: CWndBodyElementDescription(pParent)
{
}

//-----------------------------------------------------------------------------
void CWndBodyTreeElementDescription::SerializeJson(CJsonSerializer& strJson)
{
	strJson.WriteBool(szJsonHasChild, m_bHasChild == TRUE);
	strJson.WriteBool(szJsonIsExpanded, m_bIsExpanded == TRUE);
	strJson.WriteInt(szJsonLevel, m_nLevel);
	__super::SerializeJson(strJson);

}


//-----------------------------------------------------------------------------
void CWndBodyTreeElementDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	if (parser.Has(szJsonHasChild))
		m_bHasChild = parser.ReadBool(szJsonHasChild);

	if (parser.Has(szJsonIsExpanded))
		m_bIsExpanded = parser.ReadBool(szJsonIsExpanded);


	if (parser.Has(szJsonLevel))
		m_nLevel = parser.ReadInt(szJsonLevel);
}

//-----------------------------------------------------------------------------
void CWndBodyTreeElementDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_bHasChild = ((CWndBodyTreeElementDescription*)pDesc)->m_bHasChild;
	m_bIsExpanded = ((CWndBodyTreeElementDescription*)pDesc)->m_bIsExpanded;
	m_nLevel = ((CWndBodyTreeElementDescription*)pDesc)->m_nLevel;
}

IMPLEMENT_DYNCREATE(CWndBodyTreeElementDescription, CWndBodyElementDescription)

//-----------------------------------------------------------------------------
CWndBodyDescription::CWndBodyDescription(void)
{
	m_strRowsIndicator = _T("");
	m_Type = CWndObjDescription::BodyEdit;
}

//-----------------------------------------------------------------------------
CWndBodyDescription::CWndBodyDescription(CWndObjDescription* pParent)
	: CWndObjDescription(pParent)
{
	m_strRowsIndicator = _T("");
	m_Type = CWndObjDescription::BodyEdit;
}

//-----------------------------------------------------------------------------
void CWndBodyDescription::SerializeJson(CJsonSerializer& strJson)
{
	// At the moment "rowsIndicator" is not read by JS code.
	SERIALIZE_STRING(m_strRowView, szJsonRowView);
	SERIALIZE_INT(m_nLinesPerRow, szJsonLinesPerRow, 1);
	SERIALIZE_BOOL(m_bInsertRowEnabled, szJsonInsertRowEnabled, true);
	SERIALIZE_BOOL(m_bAddRowEnabled, szJsonAddRowEnabled, true);
	SERIALIZE_BOOL(m_bDeleteRowEnabled, szJsonDeleteRowEnabled, true);

	SERIALIZE_BOOL3(m_bAllowCallDialog, szJsonAllowCallDialog);

	SERIALIZE_BOOL3(m_bAllowOrdering, szJsonAllowOrdering);
	SERIALIZE_BOOL3(m_bAllowOrderingOnBrowse, szJsonAllowOrderingOnBrowse);
	SERIALIZE_BOOL3(m_bAllowOrderingOnEdit, szJsonAllowOrderingOnEdit);

	SERIALIZE_BOOL3(m_bAllowDelete, szJsonAllowDelete);
	SERIALIZE_BOOL3(m_bAllowInsert, szJsonAllowInsert);
	SERIALIZE_BOOL3(m_bAllowMultipleSel, szJsonAllowMultipleSel);
	SERIALIZE_BOOL3(m_bAllowRemoveColumnInteractive, szJsonAllowRemoveColumnInteractive);
	SERIALIZE_BOOL3(m_bChangeColor, szJsonChangeColor);
	SERIALIZE_BOOL3(m_bEnlargeAllStringColumns, szJsonEnlargeAllStringColumns);
	SERIALIZE_BOOL3(m_bEnlargeCustom, szJsonEnlargeCustom);
	SERIALIZE_BOOL3(m_bEnlargeLastColumn, szJsonEnlargeLastColumn);
	SERIALIZE_BOOL3(m_bEnlargeLastStringColumn, szJsonEnlargeLastStringColumn);

	SERIALIZE_BOOL3(m_bShowColumnHeaders, szJsonShowColumnHeaders);
	SERIALIZE_BOOL3(m_bShowVertScrollbar, szJsonShowVertScrollbar);
	SERIALIZE_BOOL3(m_bShowHorizScrollbar, szJsonShowHorizScrollbar);
	SERIALIZE_BOOL3(m_bShowFooterToolbar, szJsonShowFooterToolbar);
	SERIALIZE_BOOL3(m_bShowHeaderToolbar, szJsonShowHeaderToolbar);
	SERIALIZE_BOOL3(m_bShowHorizLines, szJsonShowHorizLines);
	SERIALIZE_BOOL3(m_bShowVertLines, szJsonShowVertLines);
	SERIALIZE_BOOL3(m_bShowBorders, szJsonShowBorders);
	SERIALIZE_BOOL3(m_bShowDataTip, szJsonShowDataTip);
	SERIALIZE_BOOL3(m_bShowStatusBar, szJsonShowStatusBar);
	SERIALIZE_BOOL3(m_bShowAlternateColor, szJsonShowAlternateColor);

	SERIALIZE_BOOL3(m_bAllowDrag, szJsonAllowDrag);
	SERIALIZE_BOOL3(m_bAllowDrop, szJsonAllowDrop);
	SERIALIZE_BOOL3(m_bAllowPaste, szJsonAllowPaste);
	SERIALIZE_BOOL3(m_bAllowCopy, szJsonAllowCopy);
	SERIALIZE_BOOL3(m_bAllowDragReadOnlyDoc, szJsonAllowDragReadOnlyDoc);
	SERIALIZE_BOOL3(m_bAllowColumnLock, szJsonAllowColumnLock);
	SERIALIZE_BOOL3(m_bAllowColumnLockInteractive, szJsonAllowColumnLockInteractive);
	SERIALIZE_BOOL3(m_bAllowCustomize, szJsonAllowCustomize);
	SERIALIZE_BOOL3(m_bAllowSearch, szJsonAllowSearch);

	SERIALIZE_INT(m_nMaxRecords, szJsonMaxRecords, MAX_BODY_RECORDS);
	SERIALIZE_DOUBLE(m_dTitleRows, szJsonTitleRows, DEFAULT_TITLE_ROWS);
	__super::SerializeJson(strJson);

}
//-----------------------------------------------------------------------------
void CWndBodyDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_STRING(m_strRowView, szJsonRowView);
	PARSE_INT(m_nLinesPerRow, szJsonLinesPerRow);
	PARSE_BOOL(m_bInsertRowEnabled, szJsonInsertRowEnabled);
	PARSE_BOOL(m_bAddRowEnabled, szJsonAddRowEnabled);
	PARSE_BOOL(m_bDeleteRowEnabled, szJsonDeleteRowEnabled);

	PARSE_BOOL3(m_bAllowOrdering, szJsonAllowOrdering);
	PARSE_BOOL3(m_bAllowOrderingOnBrowse, szJsonAllowOrderingOnBrowse);
	PARSE_BOOL3(m_bAllowOrderingOnEdit, szJsonAllowOrderingOnEdit);

	PARSE_BOOL3(m_bAllowCallDialog, szJsonAllowCallDialog);
	PARSE_BOOL3(m_bAllowDelete, szJsonAllowDelete);
	PARSE_BOOL3(m_bAllowInsert, szJsonAllowInsert);
	PARSE_BOOL3(m_bAllowMultipleSel, szJsonAllowMultipleSel);
	PARSE_BOOL3(m_bAllowRemoveColumnInteractive, szJsonAllowRemoveColumnInteractive);
	PARSE_BOOL3(m_bChangeColor, szJsonChangeColor);
	PARSE_BOOL3(m_bEnlargeAllStringColumns, szJsonEnlargeAllStringColumns);
	PARSE_BOOL3(m_bEnlargeCustom, szJsonEnlargeCustom);
	PARSE_BOOL3(m_bEnlargeLastColumn, szJsonEnlargeLastColumn);
	PARSE_BOOL3(m_bEnlargeLastStringColumn, szJsonEnlargeLastStringColumn);
	PARSE_BOOL3(m_bShowColumnHeaders, szJsonShowColumnHeaders);
	PARSE_BOOL3(m_bShowVertScrollbar, szJsonShowVertScrollbar);
	PARSE_BOOL3(m_bShowHorizScrollbar, szJsonShowHorizScrollbar);
	PARSE_BOOL3(m_bShowFooterToolbar, szJsonShowFooterToolbar);
	PARSE_BOOL3(m_bShowHeaderToolbar, szJsonShowHeaderToolbar);
	PARSE_BOOL3(m_bShowHorizLines, szJsonShowHorizLines);
	PARSE_BOOL3(m_bShowVertLines, szJsonShowVertLines);
	PARSE_BOOL3(m_bShowBorders, szJsonShowBorders);
	PARSE_BOOL3(m_bShowDataTip, szJsonShowDataTip);
	PARSE_BOOL3(m_bShowStatusBar, szJsonShowStatusBar);
	PARSE_BOOL3(m_bShowAlternateColor, szJsonShowAlternateColor);

	PARSE_BOOL3(m_bAllowDrag, szJsonAllowDrag);
	PARSE_BOOL3(m_bAllowDrop, szJsonAllowDrop);
	PARSE_BOOL3(m_bAllowPaste, szJsonAllowPaste);
	PARSE_BOOL3(m_bAllowCopy, szJsonAllowCopy);
	PARSE_BOOL3(m_bAllowDragReadOnlyDoc, szJsonAllowDragReadOnlyDoc);
	PARSE_BOOL3(m_bAllowColumnLock, szJsonAllowColumnLock);
	PARSE_BOOL3(m_bAllowColumnLockInteractive, szJsonAllowColumnLockInteractive);
	PARSE_BOOL3(m_bAllowCustomize, szJsonAllowCustomize);
	PARSE_BOOL3(m_bAllowSearch, szJsonAllowSearch);

	if (parser.Has(szJsonRowsIndicator))
		m_strRowsIndicator = parser.ReadString(szJsonRowsIndicator);

	PARSE_INT(m_nMaxRecords, szJsonMaxRecords);
	PARSE_DOUBLE(m_dTitleRows, szJsonTitleRows);

}
//-----------------------------------------------------------------------------
void CWndBodyDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_strRowsIndicator = ((CWndBodyDescription*)pDesc)->m_strRowsIndicator;
	m_bPrevRows = ((CWndBodyDescription*)pDesc)->m_bPrevRows;
	m_bNextRows = ((CWndBodyDescription*)pDesc)->m_bNextRows;
	m_strRowView = ((CWndBodyDescription*)pDesc)->m_strRowView;
	m_nLinesPerRow = ((CWndBodyDescription*)pDesc)->m_nLinesPerRow;
	m_bInsertRowEnabled = ((CWndBodyDescription*)pDesc)->m_bInsertRowEnabled;
	m_bAddRowEnabled = ((CWndBodyDescription*)pDesc)->m_bAddRowEnabled;
	m_bDeleteRowEnabled = ((CWndBodyDescription*)pDesc)->m_bDeleteRowEnabled;
	m_bAllowCallDialog = ((CWndBodyDescription*)pDesc)->m_bAllowCallDialog;
	m_bAllowOrdering = ((CWndBodyDescription*)pDesc)->m_bAllowOrdering;
	m_bAllowOrderingOnBrowse = ((CWndBodyDescription*)pDesc)->m_bAllowOrderingOnBrowse;
	m_bAllowOrderingOnEdit = ((CWndBodyDescription*)pDesc)->m_bAllowOrderingOnEdit;
	m_bAllowDelete = ((CWndBodyDescription*)pDesc)->m_bAllowDelete;
	m_bAllowInsert = ((CWndBodyDescription*)pDesc)->m_bAllowInsert;
	m_bAllowMultipleSel = ((CWndBodyDescription*)pDesc)->m_bAllowMultipleSel;
	m_bAllowRemoveColumnInteractive = ((CWndBodyDescription*)pDesc)->m_bAllowRemoveColumnInteractive;
	m_bChangeColor = ((CWndBodyDescription*)pDesc)->m_bChangeColor;
	m_bEnlargeAllStringColumns = ((CWndBodyDescription*)pDesc)->m_bEnlargeAllStringColumns;
	m_bEnlargeCustom = ((CWndBodyDescription*)pDesc)->m_bEnlargeCustom;
	m_bEnlargeLastColumn = ((CWndBodyDescription*)pDesc)->m_bEnlargeLastColumn;
	m_bEnlargeLastStringColumn = ((CWndBodyDescription*)pDesc)->m_bEnlargeLastStringColumn;
	m_bShowColumnHeaders = ((CWndBodyDescription*)pDesc)->m_bShowColumnHeaders;
	m_bShowVertScrollbar = ((CWndBodyDescription*)pDesc)->m_bShowVertScrollbar;
	m_bShowHorizScrollbar = ((CWndBodyDescription*)pDesc)->m_bShowHorizScrollbar;
	m_bShowFooterToolbar = ((CWndBodyDescription*)pDesc)->m_bShowFooterToolbar;
	m_bShowHeaderToolbar = ((CWndBodyDescription*)pDesc)->m_bShowHeaderToolbar;
	m_bShowHorizLines = ((CWndBodyDescription*)pDesc)->m_bShowHorizLines;
	m_bShowVertLines = ((CWndBodyDescription*)pDesc)->m_bShowVertLines;
	m_bShowBorders = ((CWndBodyDescription*)pDesc)->m_bShowBorders;
	m_bShowDataTip = ((CWndBodyDescription*)pDesc)->m_bShowDataTip;
	m_bShowStatusBar = ((CWndBodyDescription*)pDesc)->m_bShowStatusBar;
	m_bShowAlternateColor = ((CWndBodyDescription*)pDesc)->m_bShowAlternateColor;

	m_bAllowDrag = ((CWndBodyDescription*)pDesc)->m_bAllowDrag;
	m_bAllowDrop = ((CWndBodyDescription*)pDesc)->m_bAllowDrop;
	m_bAllowPaste = ((CWndBodyDescription*)pDesc)->m_bAllowPaste;
	m_bAllowCopy = ((CWndBodyDescription*)pDesc)->m_bAllowCopy;
	m_bAllowDragReadOnlyDoc = ((CWndBodyDescription*)pDesc)->m_bAllowDragReadOnlyDoc;
	m_bAllowColumnLock = ((CWndBodyDescription*)pDesc)->m_bAllowColumnLock;
	m_bAllowColumnLockInteractive = ((CWndBodyDescription*)pDesc)->m_bAllowColumnLockInteractive;
	m_bAllowCustomize = ((CWndBodyDescription*)pDesc)->m_bAllowCustomize;
	m_bAllowSearch = ((CWndBodyDescription*)pDesc)->m_bAllowSearch;

	m_nMaxRecords = ((CWndBodyDescription*)pDesc)->m_nMaxRecords;
	m_dTitleRows = ((CWndBodyDescription*)pDesc)->m_dTitleRows;
}

//-----------------------------------------------------------------------------
void CWndBodyDescription::UpdateFocusStatus(CWnd *pWnd)
{
	//non assegna mai il fuoco al bodyedit nella descrizione perche tanto lato Web e' un controllo
	//non fucusable e quindi causerebbe un ridisegno inutile di tutto il body edit
	return;
}

IMPLEMENT_DYNCREATE(CWndBodyDescription, CWndObjDescription)
REGISTER_WND_OBJ_CLASS(CWndBodyDescription, BodyEdit)
REGISTER_WND_OBJ_CLASS(CWndBodyDescription, TreeBodyEdit)

/////////////////////////////////////////////////////////////////////////////
const UINT CBodyEdit::g_InternalCommand[] =
{
	IDC_BE_INSERT,
	IDC_BE_DELETE,
	IDC_BE_CALLDIALOG,
	IDC_BE_MULTISELECTION,
	IDC_BE_ALTERNATECOLOR,

	IDC_BE_SEARCH,
	IDC_BE_SEARCH_TEXT,

	IDC_BE_GOHOME,
	IDC_BE_GOEND,

	IDC_BE_ROW_HEIGHT_PLUS,
	IDC_BE_ROW_HEIGHT_MINUS,

	IDC_BE_ROWINDICATOR,

	IDC_BE_HSCROLLBAR,
	IDC_BE_VSCROLLBAR,

	IDC_BE_SPINROWS
};

//-----------------------------------------------------------------------------
BOOL CBodyEdit::IsFocusCommand(UINT nID)
{
	return !(
		nID == IDC_BE_GOHOME || nID == IDC_BE_GOEND ||
		nID == IDC_BE_INSERT || nID == IDC_BE_DELETE || nID == IDC_BE_CALLDIALOG ||
		nID == IDC_BE_SPINROWS || IDC_BE_ROW_HEIGHT_PLUS || IDC_BE_ROW_HEIGHT_MINUS
		);
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::IsInternalCommand(UINT nID)
{
	int nCmds = sizeof(g_InternalCommand) / sizeof(UINT);
	for (int i = 0; i < nCmds; i++)
		if (g_InternalCommand[i] == nID) return TRUE;
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::IsStandardButton(UINT nID)
{
	return nID == IDC_BE_INSERT || nID == IDC_BE_DELETE ||
		nID == IDC_BE_CALLDIALOG || nID == IDC_BE_MULTISELECTION || nID == IDC_BE_ALTERNATECOLOR ||
		nID == IDC_BE_SEARCH || nID == IDC_BE_SEARCH_TEXT ||
		nID == IDC_BE_GOHOME || nID == IDC_BE_GOEND ||
		nID == IDC_BE_ROW_HEIGHT_PLUS || nID == IDC_BE_ROW_HEIGHT_MINUS ||
		nID == IDC_BE_ROWINDICATOR ||
		nID == IDC_BE_HSCROLLBAR;
}

/////////////////////////////////////////////////////////////////////////////
static const TCHAR szRowIndicatorFormat[] = _T(" %5d/%5d");
static const TCHAR szRowIndicatorTemplate[] = _T("99999/99999");

#define DATA_TXT_COL_SIZE  64
#define DATA_TXT_ROWS_SIZE 32

#define BE_FINDER_HEADER_HEIGHT 36

#define	DRAG_DELTA		5

#define	ID_TIMER_SCROLL				2021
#define DELAY_TIMER_HIDDENSCROLL	(1500)		// Specifies the time-out value, or interval, in milliseconds.

//=============================================================================

////////////////////////////////////////////////////////////////////////////////
//				class CBodyEdit implementation
////////////////////////////////////////////////////////////////////////////////

/*
SOME MODIFICATIONS DONE TO INTEGRATE COLUMN LOCK (IMPR. 3523):
COMPARING TO PREVIOUS VERSION, IN WHICH THERE WERE MULTIPLE PARALLEL METHODS TO HANDLE SCROLLING
(ONE FOR THE 'LOCKED' CASE, ONE FOR THE 'NORMAL' CASE), THE PROGRAM FLOW HAS HERE BEEN UNIFIED, AND REDUNDANT CODE
HAS BEEN REMOVED.
VARIABLE m_nStartColumnIdx IS THE KEY FOR UNDERSTANDING THE NEW BEHAVIOUR:
- WHEN NO COLUMN IS LOCKED, IT HAS THE USUAL MEANING (THE INDEX OF THE FIRST VISIBLE COLUMN)
- WHEN SOME COLUMNS ARE LOCKED, THE FIRST VISIBLE ONE IS ALWAYS THE FIRST (INDEX 0), SO THIS
VARIABLE BECOMES THE NUMBER OF HIDDEN COLUMNS AFTER THE LAST LOCKED ONE
WHEN I SCROLL TO POSITION 1, FOR EXAMPLE, IF COLUMNS ARE NOT LOCKED THE INDEX HAS VALUE 1, AND
COLUMNS ARE DRAWN STARTING FROM 1; IF COLUMNS ARE LOCKED, THE INDEX HAS VALUE 1 BUT COLUMNS ARE
DRAWN STARTING FROM 0 (THIS IS POSSIBLE BECAUSE THE DYNAMIC METHOD GetLogicalStartColumnIdx IS USED);
1 COLUMN AFTER THE LAST LOCKED ONE IS MARKED WITH m_bVisible = FALSE: THIS CAUSES ITS CURRENT WIDTH
TO BE EVALUATED AS ZERO, SO THE COLUMN IS NOT VISIBLE AND ONE MORE COLUMN APPERARS TO THE RIGHT

IN EACH CASE, THE VARIABLE REPRESENTS THE CURRENT SCROLL POSITION; WHEN IT IS ACCESSED
WITH THIS MEANING, IT CAN BE USED DIRECTLY, WHEN IT IS USED FOR DRAWING PURPOSES, THE METHOD GetLogicalStartColumnIdx
HAS TO BE USED INSTEAD

THE METHOD DoScrollHoriz NEEDS A SCROLL POSITION, BUT NOTE: m_nCurrColumnIdx IS NOT A VALID
SCROLL POSITION, BECAUSE IF FOR EXAMPLE ITS VALUE IS 4 AND 2 COLUMNS ARE LOCKED, THE CORRESPONDING
SCROLL POSITION IS 4 - 2 = 2; SO IF I WANT TO SCROLL TO THE 4 POSITION WE MUST CALL
DoScrollHoriz WITH 2, NOT 4 (THIS ADJUSTMENT IS DONE BY METHOD PhysicalToLogicalColIdx)
*/


IMPLEMENT_DYNCREATE(CBodyEdit, CGridControl)

BEGIN_MESSAGE_MAP(CBodyEdit, CGridControl)

	ON_WM_HSCROLL()
	ON_WM_VSCROLL()
	ON_WM_ACTIVATE()
	ON_WM_KILLFOCUS()
	ON_WM_SETFOCUS()

	ON_WM_SETCURSOR()
	ON_WM_TIMER()
	ON_WM_MEASUREITEM()
	ON_WM_DROPFILES()

	ON_WM_CONTEXTMENU()
	ON_WM_LBUTTONDOWN()
	ON_WM_RBUTTONDOWN()
	ON_WM_LBUTTONDBLCLK()
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()
	ON_WM_MOUSEWHEEL()
	ON_MESSAGE(WM_MOUSELEAVE, OnMouseLeave)

	//ON_WM_SIZE()
	ON_WM_WINDOWPOSCHANGED()
	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)

	ON_MESSAGE(UM_BAD_VALUE, OnBadValue)
	ON_MESSAGE(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)
	ON_MESSAGE(UM_LOSING_FOCUS, OnLosingFocus)
	ON_MESSAGE(UM_VALUE_CHANGED, OnValueChanged)

	ON_COMMAND(ID_BE_COPY, OnCopy)
	ON_COMMAND(ID_BE_PASTE, OnPaste)
	ON_COMMAND(ID_BE_SELECT_ALL, OnSelectAll)
	ON_COMMAND(ID_BE_UNSELECT_ALL, OnUnselectAll)
	ON_COMMAND(ID_BE_INVERT_SELECTION, OnInvertSelection)

	ON_COMMAND(IDC_BE_ALTERNATECOLOR, OnToggleAlternateColor)

	ON_COMMAND(ID_BE_TOGGLE_ALL_COLUMNS_WIDTH, OnToggleAllColumnsWidth)
	ON_COMMAND(ID_BE_TOGGLE_CURRENT_COLUMN_WIDTH, OnToggleCurrentColumnWidth)

	ON_COMMAND(ID_BE_UNLOCK_COLUMN, OnUnlockColumn)

	ON_COMMAND(ID_BE_STATISTIC, OnColumnStatistic)

	ON_COMMAND(ID_BE_SHOW_SEARCH, OnShowSearch)
	ON_COMMAND(ID_BE_SHOW_FILTERS, OnShowFilters)
	ON_COMMAND(ID_BE_REMOVE_COLUMN_FILTER, OnRemoveColumnFilter)
	ON_COMMAND(ID_BE_REMOVE_ALL_FILTERS, OnRemoveAllFilters)

	ON_COMMAND(IDC_BE_INSERT, InsertRecord)
	ON_COMMAND(IDC_BE_DELETE, DeleteRecord)
	ON_COMMAND(IDC_BE_CALLDIALOG, OnCallDialog)

	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	ON_MESSAGE(UM_BE_SETCELL, OnSetCell)

	//ON_NOTIFY_REFLECT_EX	(TTN_NEEDTEXT, OnNotifyToolTip)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBodyEdit::CBodyEdit(const CString sName)
	:
	CGridControl(OSLType_BodyEdit, sName),
	m_dwExStyles
	(
		BE_STYLE_ALLOW_INSERT |
		BE_STYLE_ALLOW_DELETE |
		BE_STYLE_ALLOW_CALLDIALOG |
		BE_STYLE_SHOW_COLUMN_HEADERS |

		BE_STYLE_SHOW_ALTERNATE_COLOR |
		BE_STYLE_ALLOW_MULTIPLE_SEL |
		BE_STYLE_ALLOW_DRAG |
		BE_STYLE_ALLOW_COPY |
		BE_STYLE_ALLOW_DRAG_READONLY_DOC |

		BE_STYLE_ALLOW_COLUMN_LOCK |
		BE_STYLE_ALLOW_COLUMN_LOCK_INTERACTIVE |
		BE_STYLE_ALLOW_REMOVE_COLUMN_INTERACTIVE |
		BE_STYLE_ALLOW_CUSTOMIZE |

		BE_STYLE_ALLOW_SEARCH |
		BE_STYLE_CHANGE_COLOR |

		BE_STYLE_SHOW_FOOTER_TOOLBAR |

		BE_STYLE_SHOW_HORIZ_SCROLLBAR | /* removing this SCROLLBAR in ToolBar */
		BE_STYLE_SHOW_VERT_SCROLLBAR |

		BE_STYLE_SHOW_VERT_LINES |
		BE_STYLE_SHOW_BORDERS |
		BE_STYLE_SHOW_DATATIP |
		BE_STYLE_SHOW_STATUSBAR |
		BE_STYLE_SHOW_CONTEXTMENU

	),
	m_bRejectUpdate(UPDATE_BODY_REJECT_NONE),
	m_nMaxBodyRecords(MAX_BODY_RECORDS),
	m_PrevFormMode(CBaseDocument::BROWSE),

	m_nWebRowEnd(WEB_PAGE_SIZE),
	m_DataTipStyle(BE_DTSTYLE_ALL),
	m_crAlternateColor(AfxGetThemeManager()->GetAlternateColor()),
	m_crMultiSelTextColor(AfxGetThemeManager()->GetBEMultiSelForeColor()),
	m_crMultiSelBackColor(AfxGetThemeManager()->GetBEMultiSelBkgColor()),
	m_HeaderToolBar(this),
	m_FooterToolBar(this),

	m_sRowIndicatorFormat(szRowIndicatorFormat)
{
	LoadBitmapOrPng(&m_bmpSortDesc, TBGlyph(szGlyphSortDown));

	LoadBitmapOrPng(&m_bmpSortAsc, TBGlyph(szGlyphSortUp));

	LoadBitmapOrPng(&m_bmpClose, TBGlyph(szGlyphInvisible));

	LoadBitmapOrPng(&m_bmpFiltered, TBGlyph(szGlyphFilter));

	LoadBitmapOrPng(&m_bmpSearched, TBGlyph(szGlyphSearch));

	LoadBitmapOrPng(&m_bmpLock, TBGlyph(szGlyphLock));

	LoadBitmapOrPng(&m_bmpLocked, TBGlyph(szGlyphLocked));

	LoadBitmapOrPng(&m_bmpUnlock, TBGlyph(szGlyphUnlock));

	m_pProcessingMouse = new CriticalArea;

	DataObj* pS = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szAllowBodyeditColumnHeaderSmallFont, DataInt(3), szTbDefaultSettingFileName);
	m_bAllowColumnHeaderSmallFont = pS ? *((DataInt*)pS) > 0 : FALSE;

	pS = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szAddBodyeditColumnHeaderExtraSpace, DataBool(FALSE), szTbDefaultSettingFileName);
	m_bAddColumnHeaderExtraSpace = pS ? *((DataBool*)pS) : FALSE;

	pS = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szDataTipDelay, DataInt(300), szTbDefaultSettingFileName);
	TFXDataTip::SetDelay(pS ? *((DataInt*)pS) : 300);

	pS = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szDataTipMaxWidth, DataInt(800), szTbDefaultSettingFileName);
	DataObj* pS2 = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szDataTipMaxHeight, DataInt(600), szTbDefaultSettingFileName);
	TFXDataTip::SetMaxDim(pS ? *((DataInt*)pS) : 800, pS2 ? *((DataInt*)pS2) : 600);

	pS = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szDataTipLevelOnBodyedit, DataInt(BE_DTSTYLE_ALL), szTbDefaultSettingFileName);
	m_DataTipStyle = pS ? (short)(*((DataInt*)pS)) : BE_DTSTYLE_ALL;

	//istanzio il datatip solo se necessario
	m_pDataTip = (m_DataTipStyle == BE_DTSTYLE_NONE || AfxIsRemoteInterface())
		? NULL
		: new TFXDataTip();

	if (AfxGetThemeManager()->UppercaseGridTitles())
		BEAddExStyle(BE_STYLE_UPPERCASE_COLUMN_TITLES);

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CBodyEdit::~CBodyEdit()
{
	//m_Handler.FireDisposing(this);
	ClipCursor(NULL);

	if (m_pDataTip)
		m_pDataTip->On(FALSE);

	if (m_pProcessingMouse)
	{
		delete m_pProcessingMouse;
		m_pProcessingMouse = NULL;
	}

	// control all related pointer to right cleanup
	if (m_pDBT)
	{
		//DEVE essere fatta prima della RebuildLinks
		//provo a rimuoverla: è comodo tenersi la riga corrente del dbt, così quando il body edit
		//si riaggancia è già posizionato sulla riga giusta
		//m_pDBT->SetCurrentRow(-1, FALSE);

		if (m_pRowFormView)
		{
			// MUST reattach record template for cleanup purpose
			RebuildLinks();

			m_pRowFormView->Detach();
			m_pRowFormView->GetParentFrame()->PostMessage(WM_CLOSE);
			m_pRowFormView = NULL;
		}
	}

#ifdef UNDO_DELETE
	SAFE_DELETE(m_pRecDeleted)
#endif

		SAFE_DELETE(m_pHScrollBar)
		SAFE_DELETE(m_pVScrollBar)
		//SAFE_DELETE(m_pBtnGoHome)
		//SAFE_DELETE(m_pBtnGoEnd)
		//SAFE_DELETE(m_pBtnInsert)
		//SAFE_DELETE(m_pBtnDelete)
		//SAFE_DELETE(m_pBtnCallDialog)
		//SAFE_DELETE(m_pBtnSpinRows)
		//SAFE_DELETE(m_pRowIndicator)
		//SAFE_DELETE(m_pFontRowIndicator)
		//SAFE_DELETE(m_pBtnSearch);
		//SAFE_DELETE(m_pBtnMultiSelection)

		m_bmpSortDesc.DeleteObject();
	m_bmpSortAsc.DeleteObject();
	m_bmpClose.DeleteObject();
	m_bmpFiltered.DeleteObject();
	m_bmpSearched.DeleteObject();
	m_bmpLock.DeleteObject();
	m_bmpLocked.DeleteObject();
	m_bmpUnlock.DeleteObject();

	m_OrdSelectedColumn.RemoveAll();

	SAFE_DELETE(m_pDataTip);

	SAFE_DELETE(m_pDropTarget);
	ASSERT(m_TimerScroll == 0);
	// rilascia il codec
	AttachDataCoDec(NULL, FALSE);

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//-----------------------------------------------------------------------------
HRESULT CBodyEdit::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace = cwsprintf(_T("{0-%s}{1-%s}"), GetNamespace().GetObjectName(), GetNamespace().GetTypeString());
	*pszName = ::SysAllocString(sNamespace);
	return S_OK;
}

//------------------------------------------------------------------------------
LRESULT CBodyEdit::OnRecalcCtrlSize(WPARAM w, LPARAM l)
{
	__super::OnRecalcCtrlSize(w, l);

	if (m_nDirStrech > 0)
	{
		CRect rectClient;
		GetClientRect(rectClient);

		//if (m_HeaderToolBar.HasButtonRightAligned())
		{
			m_HeaderToolBar.m_rectToolBar.right = rectClient.right;
			m_HeaderToolBar.SetPos();
		}
		//if (m_FooterToolBar.HasButtonRightAligned())
		{
			m_FooterToolBar.m_rectToolBar.right = rectClient.right;
			m_FooterToolBar.SetPos();
		}
	}

	return 0L;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::EnableWindow(BOOL bEnable/* = TRUE*/)
{
	BOOL b = __super::EnableWindow(bEnable);
	RedrawWindow(NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ERASE | RDW_ALLCHILDREN);
	return b;
}

//-----------------------------------------------------------------------------
int CBodyEdit::GetEndColumnIdx() const
{
	if (m_ColumnsInfo.GetSize() == 0)
		return -1;

	if (m_LastColsIdx.GetSize() == 0)
		return 0;

	ASSERT(m_LastColsIdx.GetSize() > 0);
	int idx = GetLogicalStartColumnIdx();
	if (idx < 0 || idx >= m_LastColsIdx.GetSize())
	{
		//ASSERT(FALSE);
		if (idx >= m_LastColsIdx.GetSize())
			idx = m_LastColsIdx.GetUpperBound();
		if (idx < 0) idx = 0;
	}

	if (m_LastColsIdx.GetUpperBound() < idx)
		return -1;

	return m_LastColsIdx[idx] & ~FLAG_END_COL_NOT_FULL_VISIBLE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::IsEndColFullVisible(int nStart) const
{
	int idx = nStart;
	if (idx < 0 || idx >= m_LastColsIdx.GetSize())
	{
		ASSERT(FALSE);

		if (idx >= m_LastColsIdx.GetSize())
			idx = m_LastColsIdx.GetUpperBound();
		if (idx < 0) idx = 0;

		if (idx >= m_LastColsIdx.GetSize())
			return FALSE;
	}
	return (m_LastColsIdx[idx] & FLAG_END_COL_NOT_FULL_VISIBLE) != FLAG_END_COL_NOT_FULL_VISIBLE;
}

//-----------------------------------------------------------------------------
int CBodyEdit::GetEndRecordIdx() const
{
	return min(m_pDBT ? m_pDBT->GetUpperBound() : -1, m_nStartRecordIdx + m_nLastBodyRow);
}

//-----------------------------------------------------------------------------
int CBodyEdit::GetLogicalStartColumnIdx() const
{
	if (ColumnLocked())
		return 0;

	ASSERT(m_nStartColumnIdx >= 0);

	return m_nStartColumnIdx;
}

//-----------------------------------------------------------------------------
int CBodyEdit::PhysicalToLogicalColIdx(int nPhysical)
{
	if (!ColumnLocked())
		return nPhysical;

	int nIndex = GetVisibleColumnIdx(m_nLockedColumnIDC) + 1;
	if (nPhysical <= nIndex)
		return 0;

	return nPhysical - nIndex;
}

//-----------------------------------------------------------------------------
void CBodyEdit::NextWebPage()
{
	m_nWebRowStart += (WEB_PAGE_SIZE + 1);
}

//-----------------------------------------------------------------------------
void CBodyEdit::PrevWebPage()
{
	m_nWebRowStart -= (WEB_PAGE_SIZE + 1);
}

//-----------------------------------------------------------------------------
void CBodyEdit::ReadRows(int upperLimit)
{
	while (upperLimit > (GetRowNumber() - 1) && (m_pDBT && m_pDBT->MoreRecords()))
		LoadNextRecords(TRUE);
}

//-----------------------------------------------------------------------------
LRESULT CBodyEdit::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;
	CString strId = (LPCTSTR)lParam;
	CWndBodyDescription* pBodyEditDesc = (CWndBodyDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndBodyDescription), strId);
	pBodyEditDesc->UpdateAttributes(this);

	int x = pBodyEditDesc->m_X, y = pBodyEditDesc->m_Y; //per l'editor json mi servono solo le colonne
	GetColumnInfoDescription(pBodyEditDesc, x, y);
	return (LRESULT)pBodyEditDesc;

	return (LRESULT)pBodyEditDesc;
}
//-----------------------------------------------------------------------------
void CBodyEdit::GetColumnInfoDescription(CWndObjDescription* pDesc, int& x, int& y)
{
	ColumnsInfoArray& ar = m_AllColumnsInfo;
	int iColumnsBound = ar.GetUpperBound();
	for (int nCol = 0; nCol <= iColumnsBound; nCol++)
	{
		ColumnInfo *pInfo = ar.GetAt(nCol);
		CString strName = pInfo->GetNamespace().GetObjectName();
		CString strColTitleId = pInfo->m_nCtrlIDC ? AfxGetTBResourcesMap()->DecodeID(TbControls, pInfo->m_nCtrlIDC).m_strName : _T("");
		if (strColTitleId.IsEmpty())
			strColTitleId = strName;

		CWndBodyColumnDescription *pColTitleDesc = (CWndBodyColumnDescription *)pDesc->m_Children.GetWindowDescription(pInfo->GetParsedCtrl()->GetCtrlCWnd(), RUNTIME_CLASS(CWndBodyColumnDescription), strColTitleId);
		pColTitleDesc->m_Type = CWndObjDescription::ColTitle;
		// adds the editor for the column.
		CParsedCtrl* pCtrl = pInfo->GetParsedCtrl();
		pColTitleDesc->AttachTo(pInfo->GetParsedCtrl()->GetCtrlCWnd()->m_hWnd);
		pColTitleDesc->m_strControlClass = pCtrl->GetCtrlClass();

		//calcolo il rettangolo dell'header di colonna
		CRect colTitleRect(0, 0, 0, 0);
		colTitleRect.left = x;
		colTitleRect.top = y;
		x += pInfo->GetOriginalScreenWidth();
		colTitleRect.right = x;
		colTitleRect.bottom = y + m_rectTitles.Height();
		// pColTitleDesc->m_Children.AddWindow(pInfo->GetParsedCtrl()->GetCtrlCWnd());
		if (pColTitleDesc->GetRect() != colTitleRect)
		{
			pColTitleDesc->SetRect(colTitleRect, TRUE);
		}
		if (pColTitleDesc->m_strText != pInfo->m_strTitle)
		{
			pColTitleDesc->m_strText = pInfo->m_strTitle;
			pColTitleDesc->SetUpdated(&pColTitleDesc->m_strText);
		}
		if (pColTitleDesc->m_strName != strName)
		{
			pColTitleDesc->m_strName = strName;
			pColTitleDesc->SetUpdated(&pColTitleDesc->m_strName);
		}

	}
}

//-----------------------------------------------------------------------------
CString CBodyEdit::GetCellText(int nRow, int nCol)
{
	if (!m_pDBT)
		return _T("");
	ColumnInfo *pInfo = m_ColumnsInfo.GetAt(nCol);
	SqlRecord* pRec = m_pDBT->GetRow(nRow);

	DataObj* pDataObj = GetDataObjAt(pRec, pInfo);

	CParsedCtrl* pControl = pInfo->GetParsedCtrl();

	return pControl->FormatData(pDataObj);
}
//-----------------------------------------------------------------------------
void CBodyEdit::SetCurrLine(int nRec, int nStart /*= -1*/)
{
	SetCurrRecord(nRec);

	CGridControlCellPos aCellPos;
	GetCurrCellPos(aCellPos);
	aCellPos.m_nStartRowIdx = nStart >= 0 ? nStart : max(0, nRec - this->m_nLastBodyRow + 1);
	aCellPos.m_nCurrRowIdx = nRec;
	SetCurrCellPos(aCellPos);
}

//-----------------------------------------------------------------------------
LRESULT CBodyEdit::OnSetCell(WPARAM wParam, LPARAM lParam)
{
	if ((int)wParam >= m_pDBT->GetSize())
		return 1;

	SetCurrRecord(wParam);

	SetFocus();

	CGridControlCellPos cg;
	GetCurrCellPos(cg);
	cg.m_nCurrRowIdx = wParam;
	cg.m_nCurrColIdx = lParam;
	SetCurrCellPos(cg);

	ShowCtrl(TRUE);

	return 1;
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetCellPos(int nRow, int nColIDC, BOOL bAsync/* = TRUE*/)
{
	int nIdxCol = max(0, GetVisibleColumnIdx(nColIDC));
	if (bAsync)
	{
		PostMessage(UM_BE_SETCELL, (WPARAM)nRow, (LPARAM)nIdxCol);
	}
	else
		OnSetCell((WPARAM)nRow, (LPARAM)nIdxCol);
}

//-----------------------------------------------------------------------------
void CBodyEdit::ReAttachRowFormView
(
	CRuntimeClass*		pRowFormViewClass,
	const CString&		strRowFormViewTitle /*= ""*/,
	const CString&		sRowViewName		/*= _T("")*/
)
{
	if (!m_pFormViewClass  && pRowFormViewClass && BEGetExStyle(BE_STYLE_ALLOW_CALLDIALOG) && BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR))
	{
		if (this->m_FooterToolBar.FindButton(IDC_BE_CALLDIALOG) == NULL)
		{

			int idx = this->m_FooterToolBar.FindButtonIndex(IDC_BE_MULTISELECTION);
			if (idx == -1)
				idx = this->m_FooterToolBar.FindButtonIndex(IDC_BE_ALTERNATECOLOR);
			if (idx == -1)
			{
				idx = this->m_FooterToolBar.FindButtonIndex(IDC_BE_DELETE);
				if (idx > -1) idx++;
			}
			if (idx == -1)
			{
				idx = this->m_FooterToolBar.FindButtonIndex(IDC_BE_INSERT);
				if (idx > -1) idx++;
				else idx = 0;
			}

			m_FooterToolBar.AddButton(_NS_BE_TOOLBAR_BTN("ShowRowView"), IDC_BE_CALLDIALOG, TBIcon(szIconRowView, MINI), _TB("Open rowview window"), L"", idx);
			m_FooterToolBar.SetPos();
			//m_FooterToolBar.Redraw();
		}
	}

	m_pFormViewClass = pRowFormViewClass;
	m_bFormViewCallEnabled = pRowFormViewClass != NULL;

	if (!strRowFormViewTitle.IsEmpty())
		m_strRowFormViewTitle = strRowFormViewTitle;
	else if (!m_strBodyTitle.IsEmpty())
		m_strRowFormViewTitle = m_strBodyTitle;

	if (m_pRowFormView)
	{
		m_pRowFormView->GetFrame()->SendMessage(WM_CLOSE);
	}

	m_strRowFormViewName = sRowViewName;

	if (sRowViewName.IsEmpty() || !pRowFormViewClass)
		m_FooterToolBar.EnableButton(IDC_BE_CALLDIALOG, FALSE);
}

//-----------------------------------------------------------------------------

void CBodyEdit::Attach
(
	DBTSlaveBuffered*	pDBT,
	CRuntimeClass*		pRowFormViewClass,		/*= NULL*/
	const CString&		strRowFormViewTitle		/*= ""*/,
	const CString&		sRowViewName			/*= _T("")*/
)
{
	ASSERT(m_pFormViewClass == NULL);
	ASSERT(m_pRowFormView == NULL);
	ASSERT(m_pDBT == NULL);

	SetDBT(pDBT);

	m_pFormViewClass = pRowFormViewClass;
	m_bFormViewCallEnabled = pRowFormViewClass != NULL;
	m_strRowFormViewName = sRowViewName;

	//ASSERT(GetDocument()); 
	ASSERT(m_pParentForm);

	//	GetNamespace().SetChildNamespace(CTBNamespace::GRID, m_sName, m_pParentForm->GetNamespace());
	ASSERT(GetNamespace().IsValid());
	ASSERT(GetInfoOSL()->m_pParent);
	AfxGetSecurityInterface()->GetObjectGrant(GetInfoOSL());

	BeforeCustomize();
	Customize();

	CAbstractFormDoc* pDoc = GetDocument();
	//dispatch al documento per cambiare/aggiungere altre proprietà delle colonne
	if (pDoc)
	{
		pDoc->CallCustomizeBodyEdit(this);
	}

	// nella Customize potrebbe essere stato dato un titolo di body:
	// lo si usa come titolo della RowFormView
	//
	if (m_strRowFormViewTitle.IsEmpty())
	{
		if (!strRowFormViewTitle.IsEmpty())
			m_strRowFormViewTitle = strRowFormViewTitle;
		else if (!m_strBodyTitle.IsEmpty())
			m_strRowFormViewTitle = m_strBodyTitle;
	}
	BodyEditInfo* pBEObject = NULL;
	if (pDoc && pDoc->m_pFormManager)
	{
		pBEObject = pDoc->m_pFormManager->GetBodyEditInfo(GetInfoOSL()->m_Namespace);

		if (!pBEObject && this->BEGetExStyle(BE_STYLE_ALLOW_CUSTOMIZE))
		{
			pBEObject = new BodyEditInfo(GetInfoOSL()->m_Namespace, m_strRowFormViewTitle);
			pDoc->m_pFormManager->AddBodyEditInfo(pBEObject);
			//@@TODO: non posso collegarlo al diretto parent perche' non e' detto che sia
			//istanziato quando inseriro' in sicurezza ...TAPPULLO : lo collego al master doc.
			pBEObject->GetInfoOSL()->m_pParent = pDoc->GetInfoOSL();
		}

		if (pBEObject)
			pBEObject->m_pRowFormViewClass = pRowFormViewClass;
	}

	if (pBEObject && !pBEObject->IsKindOf(RUNTIME_CLASS(BodyEditInfo)))
	{
		ASSERT(FALSE);
		pBEObject = NULL;
	}

	for (int i = 0; i <= m_AllColumnsInfo.GetUpperBound(); i++)
	{
		BodyEditColumn* pBEC = NULL;
		ColumnInfo* pColInfo = m_AllColumnsInfo[i];
		ASSERT(pColInfo->GetInfoOSL()->m_Namespace.IsValid());

		if (pBEObject && !pDoc->IsInStaticDesignMode())
			pBEC = AddColumnInFormManager(pColInfo, pBEObject, i);

		//OSL
		ASSERT(pColInfo == m_AllColumnsInfo[i]);

		CInfoOSL* pInfoOSL = pColInfo->GetInfoOSL();
		pInfoOSL->m_pParent = GetInfoOSL();
		AfxGetSecurityInterface()->GetObjectGrant(pInfoOSL);

		if
			(
				!OSL_CAN_DO(pInfoOSL, OSL_GRANT_EDIT) &&
				!OSL_CAN_DO(pInfoOSL, OSL_GRANT_NEW) &&
				!OSL_CAN_DO(pInfoOSL, OSL_GRANT_BE_ADDROW)
				)
		{
			pColInfo->m_wColumnStatus |= STATUS_GRAYED | STATUS_NOCHANGE_GRAYED;
			pColInfo->GetParsedCtrl()->SetDataOSLReadOnly();
		}

		if (!OSL_CAN_DO(pInfoOSL, OSL_GRANT_EXECUTE))
		{
			pColInfo->m_wColumnStatus |= STATUS_HIDDEN | STATUS_NOCHANGE_HIDDEN;
			pColInfo->GetParsedCtrl()->SetDataOSLReadOnly();
			pColInfo->GetParsedCtrl()->SetDataOSLHide();
		}

		// costruisce la "mappa di visibilita`" delle colonne sulla base di quanto
		// descritto dal FormManager
		if (!pBEC || (pColInfo->m_wColumnStatus & STATUS_HIDDEN) != STATUS_HIDDEN)
		{
			m_ColumnsInfo.Add(pColInfo);
			if (pColInfo->HasFooter())
				m_bHasFooter = TRUE;
		}
	}

	// valido le informazioni del bodyedit per renderlo visibile nel tree
	// di modifica interattiva
	if (pBEObject)
	{
		m_ColumnsInfo.BubbleSort();
		m_AllColumnsInfo.BubbleSort();

		pBEObject->BubbleSort();
		pBEObject->m_bValid = TRUE;
	}
	//reimposto colonna locked
	if (BEGetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK))
	{
		for (int i = 0; i < m_ColumnsInfo.GetSize(); i++)
		{
			ColumnInfo* pCol = m_ColumnsInfo.GetAt(i);
			if ((pCol->m_wColumnStatus & STATUS_LOCKED) == STATUS_LOCKED)
			{
				LockColumn(pCol->m_nCtrlIDC, TRUE);
				break;
			}
		}
	}

	//nel caso di DBT Slavable, non è detto che debba agganciare il dbt passato (che corrisponde al prototipo)
	//devo piuttosto agganciare il dbt attivo. Allora, dopo aver fatto tutte le operazioni di Customize
	//(che vanno fatte sul dbt di prototi[po per evitare schiantamenti) imposto il dbt da usare usando quello attivo
	if (pDBT)
		SetDBT(pDBT->GetActiveSibling());//se sono già attivo, ritorna mé stesso e l'istruzione non ha effetto
}

//-----------------------------------------------------------------------------
ColumnInfo* CBodyEdit::AddColumn
(
	const CString&	strName,
	const CString&	strColTitle,
	DWORD			dwStyle,
	UINT			nIDC,
	DataObj*		pData,
	CRuntimeClass*	pParsedCtrlClass,
	CString			sNsHotKeyLink,
	UINT			nBtnID				/*= BTN_DEFAULT*/
)
{
	CTBNamespace nsHKL(CTBNamespace::HOTLINK, sNsHotKeyLink);
	CRuntimeClass* pControlClass = NULL;
	HotKeyLink*		pHotKeyLink = (HotKeyLink*)AfxGetTbCmdManager()->RunHotlink(nsHKL, NULL, &pControlClass);

	CRuntimeClass* pPCClass = pParsedCtrlClass;
	if (pControlClass == NULL)
		pPCClass = pParsedCtrlClass;
	else if (pParsedCtrlClass == NULL)
		pPCClass = pControlClass;
	else if (pControlClass->IsDerivedFrom(pParsedCtrlClass))
		pPCClass = pControlClass;
	else
	{
		TRACE(_T("Incompatible runtime class for parsed control: hotlink has rtc named %s but AddLink has rtc named %s\n"), pControlClass->m_lpszClassName, pParsedCtrlClass->m_lpszClassName);
		//ASSERT(FALSE);
	}

	ColumnInfo* pCol = AddColumn(strName, strColTitle, dwStyle, nIDC, pData, pPCClass, pHotKeyLink, nBtnID);

	if (pCol && pCol->GetParsedCtrl() && pHotKeyLink)
	{
		if (pHotKeyLink != pCol->GetParsedCtrl()->GetHotLink())
			delete pHotKeyLink;
		else
			pCol->GetParsedCtrl()->SetOwnHotKeyLink(TRUE);
	}
	return pCol;
}

//-----------------------------------------------------------------------------
ColumnInfo* CBodyEdit::AddColumn
(
	const CString&	strName,
	const CString&	strColTitle,
	DWORD			dwStyle,
	UINT			nIDC,
	DataObj*		pData,
	CRuntimeClass*	pParsedCtrlClass,
	HotKeyLink*		pHotKeyLink			/*= NULL*/,
	UINT			nBtnID				/*= BTN_DEFAULT*/
)
{
	CParsedCtrl* pControl = CreateParsedCtrl(strName, nIDC, pData, pParsedCtrlClass, pHotKeyLink, nBtnID);
	if (!pControl)
		return NULL;

	ColumnInfo* pColInfo = AddColumn(strName, strColTitle, dwStyle, nIDC, pControl);

	if (pColInfo == NULL)
	{
		delete pControl;
		return NULL;
	}

	return pColInfo;
}

//-----------------------------------------------------------------------------
ColumnInfo* CBodyEdit::AddColumn
(
	const CString&	strName,
	const CString&	strTitle,
	DWORD dwStyle,
	UINT nIDC,
	CParsedCtrl* pCtrl
)
{

	if (nIDC < 1000)
	{
		TRACE(
			"CBodyEdit::AddColumn: invalid column/control ID %d, column name '%s', in CBodyEdit %s\n",
			nIDC, (LPCTSTR)strName, (LPCTSTR)GetRuntimeClass()->m_lpszClassName
		);

		ASSERT(FALSE);
		return NULL;
	}

	CString sName(strName); sName.Trim();
	if (sName.IsEmpty())
	{
		ASSERT_TRACE(FALSE,
			cwsprintf(
				_T("CBodyEdit::AddColumn: invalid column name '%s' of column/control ID %d in CBodyEdit %s\n"),
				(LPCTSTR)strName, nIDC, (LPCTSTR)GetRuntimeClass()->m_lpszClassName)
		);
		sName = GetRuntimeClass()->m_lpszClassName;
	}

	for (int j = 0; j < m_AllColumnsInfo.GetSize(); j++)
	{
		if (strName.CompareNoCase(m_AllColumnsInfo[j]->GetNamespace().GetObjectName()) == 0)
		{
			TRACE(
				"CBodyEdit::AddColumn: duplicate column name '%s' of column/control ID %d in CBodyEdit %s\n",
				(LPCTSTR)strName, nIDC, (LPCTSTR)GetRuntimeClass()->m_lpszClassName
			);
			//ASSERT(FALSE);
		}
	}
	//---------	
	CString strColTitle = strTitle;

	// associated DataObj is property of m_pDBT->m_pRecord and it is used only
	// for coloumn creation process
	DataObj*	pData = pCtrl->GetCtrlData();
	//ASSERT_VALID(pData);
	SqlRecord*	pRec = m_pDBT ? m_pDBT->GetRecord() : NULL;
	//ASSERT_VALID(pRec);

	// lookup data position in the current record
	// get associated field informations from record and dictionary	
	int nDataInfoIdx = -1;
	SqlRecordItem* pRecItem = NULL;
	if (pData && pRec)
	{
		nDataInfoIdx = pRec->Lookup(pData);
		pRecItem = pRec->GetAt(nDataInfoIdx);
		if (pRecItem)
		{
#ifdef _DEBUG
			Unparser up;
			DataType dt = pRecItem->GetDataType();

			ASSERT_TRACE(
				(
					pData->GetDataType() == dt
					||
					(
						pData->GetDataType() == DATA_DATE_TYPE	&&
						dt == DATA_DATE_TYPE
						)
					||
					(
						pData->GetDataType() == DATA_TXT_TYPE	&&
						dt == DATA_STR_TYPE
						)
					),
				cwsprintf(_T("CBodyEdit::AddColumn - Different data type on bind column %s (%s, %s)\n"),
					strName,
					up.DataTypeToString(pData->GetDataType()),
					up.DataTypeToString(dt)
				)
			);
#endif
			pCtrl->m_pSqlRecord = pRec;
		}
	}
	// Se non viene assegnata una descrizione tramite un IDS da risorsa
	// viene usato il nome della colonna

	if (strColTitle.IsEmpty() && pRecItem)
	{
		strColTitle = pRecItem->GetColumnTitle(pRec->GetTableName());
		if (strColTitle.IsEmpty())
			strColTitle = pRecItem->GetColumnName();
	}

	BOOL bIsEnumCombo = pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CEnumCombo));
	// se e` stato passato uno EnumCombo questa colonna e` meglio
	// che abbia lo stile CBS_DROPDOWNLIST
	if (bIsEnumCombo)
		dwStyle = (dwStyle & ~(CBS_SIMPLE | CBS_DROPDOWN)) | CBS_DROPDOWNLIST;

	if (pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
	{
		ASSERT_TRACE(((dwStyle & CBS_STYLES) != 0), cwsprintf(_T("CBodyEdit::AddColumn - ComboBox style CBS_DROPDOWN or CBS_DROPDOWNLIST missing for column %s\n"), strName));

		dwStyle = dwStyle | WS_VSCROLL;
	}

	// di default setto il delete in caso di dropdownlist
	if (!bIsEnumCombo && (dwStyle & CBS_STYLES) == CBS_DROPDOWNLIST)
		pCtrl->SetCtrlStyle(pCtrl->GetCtrlStyle() | COMBO_DROPDOWNLIST_STYLE_ENABLE_DEL);

	// se e` stato passato uno CBoolButton questa colonna e` meglio
	// che abbia lo stile BS_AUTOCHECKBOX
	if (pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
		dwStyle = BS_AUTOCHECKBOX;

	dwStyle = (dwStyle & ~WS_VISIBLE) | WS_CHILD | WS_CLIPSIBLINGS | WS_BORDER;

	// add a new column info	
	ColumnInfo* pColInfo = new ColumnInfo
	(
		this,
		/*sName, */strColTitle, nDataInfoIdx,
		pCtrl, dwStyle, nIDC
	);
	ASSERT(pColInfo);
	pColInfo->GetNamespace().SetChildNamespace(CTBNamespace::GRIDCOLUMN, sName, GetNamespace());
	pCtrl->GetNamespace() = pColInfo->GetNamespace();

	m_AllColumnsInfo.Add(pColInfo);

	// Default control width e textalign
	// Qui non deve esere ASSOLUTAMENTE chiamata la pColInfo->SetColWidth()
	// utilizzabile solo dall'esterno (vedi sopra ColumnInfo::ResizeColumn()).
	//
	// pColInfo->SetCtrlSize(MIN_CTRL_SIZE) serve per inizializzare gli altri campi in quanto,
	// se il programmatore non modifichera` tale impostazione all'uscita
	// della AddColumn, il control si dimensionera` in maniera congruente al tipo di dato
	// vedi ParsedCtrl::GetDataSize(...)
	//
	RecalcCtrlLen(pData, pRecItem ? pRecItem->GetColumnLength() : 0, pColInfo);

	if (pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CLongIDCombo)))
	{
		pColInfo->SetTextAlign(TA_LEFT);
	}

	return pColInfo;
}

//-----------------------------------------------------------------------------
void CBodyEdit::RecalcCtrlLen(DataObj* pDataObj, int nLen, ColumnInfo* pColInfo)
{
	if (pDataObj)
		switch (pDataObj->GetDataType().m_wType)
		{
		case DATA_BOOL_TYPE:
		{
			pColInfo->SetTextAlign(TA_CENTER);
			pColInfo->SetCtrlSize(MIN_CTRL_SIZE);
			break;
		}
		case DATA_INT_TYPE:
		case DATA_LNG_TYPE:
		case DATA_DBL_TYPE:
		case DATA_MON_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE:
		{
			pColInfo->SetTextAlign(TA_RIGHT);
			pColInfo->SetCtrlSize(MIN_CTRL_SIZE);
			break;
		}
		case DATA_STR_TYPE:
		{
			pColInfo->SetTextAlign(TA_LEFT);

			ASSERT_TRACE(nLen > 0 && nLen < 4096,
				cwsprintf(_T("Wrong Column length: %s %d \n"),
					pColInfo->GetNamespace().ToString(), nLen));

			pColInfo->SetCtrlSize(nLen);
			if (pColInfo->GetParsedCtrl()->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CNamespaceEdit)))
				pColInfo->GetParsedCtrl()->SetCtrlMaxLen(0, FALSE);	// the CNamespaceEdit has long size	
			break;
		}
		case DATA_GUID_TYPE:
		{
			pColInfo->SetTextAlign(TA_LEFT);
			int nLen = 38; //pSqlColInfo->GetColumnLength(); //the method returns 0
			pColInfo->SetCtrlSize(nLen);
			break;
		}
		case DATA_TXT_TYPE:
		{
			pColInfo->SetTextAlign(TA_LEFT);
			pColInfo->SetCtrlSize(DATA_TXT_COL_SIZE, DATA_TXT_ROWS_SIZE);
			pColInfo->GetParsedCtrl()->SetCtrlMaxLen(0, FALSE);	// the datatext does not have max lenght	
			break;
		}

		case DATA_DATE_TYPE:
		case DATA_ENUM_TYPE:
		{
			pColInfo->SetTextAlign(TA_LEFT);
			pColInfo->SetCtrlSize(MIN_CTRL_SIZE);
			break;
		}
		default: ASSERT(FALSE);
		}
}

//-----------------------------------------------------------------------------
ColumnInfo*	CBodyEdit::CreateDefaultColumn(CString sControlClass, const CString& sColName)
{
	// default control class
	if (sControlClass.IsEmpty())
	{
		const CParsedCtrlFamily* pFamily = AfxGetParsedControlsRegistry()->GetDefaultFamilyInfo(DATA_STR_TYPE);
		if (pFamily)
			sControlClass = AfxGetParsedControlsRegistry()->GetFamilyDefaultControl(pFamily->GetName());
	}

	CRegisteredParsedCtrl* pCtrl = AfxGetParsedControlsRegistry()->GetRegisteredControl(sControlClass);

	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::GRIDCOLUMN, sColName, GetNamespace());
	ColumnInfo* pColumnInfo = AddColumn
	(
		sColName,
		sColName,
		pCtrl->GetNeededStyle() | pCtrl->GetNeededExStyle(),
		AfxGetTBResourcesMap()->GetTbResourceID(sColName, TbControls),
		NULL,
		pCtrl ? pCtrl->GetClass() : NULL
	);
	if (pColumnInfo && pColumnInfo->CreateCtrl(this))
		m_ColumnsInfo.Add(pColumnInfo);
	else
		ASSERT(FALSE);

	// il calcolo delle size lo demando al termine di tutto il disegno
	return pColumnInfo;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	ASSERT(m_hWnd == NULL);
	ASSERT(m_pParentForm);

	BOOL bOk = CGridControl::Create(NULL, dwStyle | WS_CLIPCHILDREN, rect, pParentWnd, nID);
	if (!bOk)
		return FALSE;

	bOk = OnCreateClient();
	if (!bOk)
		return FALSE;

	//DragAcceptFiles ();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::SubclassDlgItem(UINT nID, CWnd* pParentWnd)
{
	ASSERT(m_hWnd == NULL);
	ASSERT(m_pParentForm);

	BOOL bOk = CGridControl::SubclassDlgItem(nID, pParentWnd);

	if (!bOk)
		return FALSE;

	//DragAcceptFiles ();

	bOk = OnCreateClient();

	return bOk;
}

//-----------------------------------------------------------------------------
void CBodyEdit::GetCurrCellPos(CGridControlCellPos& aCellPos)
{
	if (m_nOldStartRecordIdx == -1)
		aCellPos.m_nStartRowIdx = m_nStartRecordIdx;	// current first body record
	else
		aCellPos.m_nStartRowIdx = m_nOldStartRecordIdx;	// current unscrolled first body record

	if (m_nOldStartColumnIdx == -1)
		aCellPos.m_nStartColIdx = m_nStartColumnIdx;	// current first body column
	else
		aCellPos.m_nStartColIdx = m_nOldStartColumnIdx;	// current unscrolled first body column

	aCellPos.m_nCurrRowIdx = m_nCurrRecordIdx;		// current active body record
	aCellPos.m_nCurrColIdx = m_nCurrColumnIdx;		// current active column

	aCellPos.m_nCurrLinesPerRow = m_nLinesPerRow;	// current # lines per rows
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetCurrCellPos(const CGridControlCellPos& aCellPos, BOOL bNotify /* = TRUE */)
{
	if (IsCtrlVisible())
		SetFocusOnly();

	if (aCellPos.m_nStartRowIdx >= 0 && aCellPos.m_nStartRowIdx <= (m_pDBT ? m_pDBT->GetUpperBound() : -1))
		m_nStartRecordIdx = aCellPos.m_nStartRowIdx;	// current first body record
	else
		m_nStartRecordIdx = 0;

	if (aCellPos.m_nCurrRowIdx >= 0 && aCellPos.m_nCurrRowIdx <= (m_pDBT ? m_pDBT->GetUpperBound() : -1))
		m_nCurrRecordIdx = aCellPos.m_nCurrRowIdx;	// current active body record
	else
		m_nCurrRecordIdx = -1;

	if (aCellPos.m_nCurrColIdx >= 0 && aCellPos.m_nCurrColIdx <= m_ColumnsInfo.GetUpperBound())
		m_nCurrColumnIdx = aCellPos.m_nCurrColIdx;	// current active column
	else
		m_nCurrColumnIdx = -1;

	if (aCellPos.m_nStartColIdx >= 0 && aCellPos.m_nStartColIdx <= m_ColumnsInfo.GetUpperBound())
	{
		int index = aCellPos.m_nStartColIdx;
		//An. 19138 Colonne bloccate da web
		if (ColumnLocked())
		{
			index = PhysicalToLogicalColIdx(index);
		}
		DoScrollHoriz(index);
	}
	else
		DoScrollHoriz(0);

	if (aCellPos.m_nCurrLinesPerRow >= 0)
		m_nLinesPerRow = aCellPos.m_nCurrLinesPerRow;	// current # lines per rows

	RecalcRowsLayout();

	if (m_nCurrRecordIdx >= 0)
		SetCurrRecord(m_nCurrRecordIdx, bNotify);

	// removed: scroll position is already set by m_nStartRowIdx and m_nStartColIdx
	ResetToVisiblePosition();

	UpdateVScrollPos();
	UpdateHScrollPos();
	InvalidateBody();
}

//-----------------------------------------------------------------------------
void CBodyEdit::AddUserButton(const CString& sName, UINT nIDC, UINT nIDBU, UINT nIDBD, UINT nIDBDI, LPCTSTR szToolTip)
{
	AddFooterButton(sName, nIDC, nIDBU, szToolTip);
	return;
	/*
		if (!BEGetExStyle(BE_STYLE_SHOW_MINI_TOOLBAR))
		{
		if (BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR))
		{
		TRACE2("Add button on hidden mini toolbar (%s, %d)\n", sName, nIDC);

		AddFooterButton(sName, nIDC, nIDBU, szToolTip);
		return;
		}
		ASSERT_TRACE2(FALSE, "Add button on hidden mini toolbar (%s, %d)\n", sName, nIDC);
		}

		for (int i = 0; i <= m_UserButtons.GetUpperBound(); i++)
		if
		(
		(((CBodyBitmapButton*)m_UserButtons[i])->m_nButtonIDC == nIDC)
		||
		((!sName.IsEmpty()) && sName.CompareNoCase(((CBodyBitmapButton*)m_UserButtons[i])->GetInfoOSL()->m_Namespace.GetObjectName()) == 0)
		)
		{
		TRACE("BodyEdit::AddUserButton: button ID=%u and Name=%s already defined\n", nIDC, sName);
		ASSERT(FALSE);
		return;
		}
		CBodyBitmapButton* pB = new CBodyBitmapButton(nIDC, nIDBU, nIDBD, nIDBDI, szToolTip);
		if (!sName.IsEmpty())
		{
		pB->GetInfoOSL()->SetType(OSLType_ToolbarButton);
		pB->GetInfoOSL()->m_pParent = GetInfoOSL();
		pB->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::TOOLBARBUTTON, sName, GetNamespace());

		AfxGetSecurityInterface()->GetObjectGrant(&(pB->GetInfoOSL()));
		}
		m_UserButtons.Add(pB);
		*/
}

//-----------------------------------------------------------------------------
void CBodyEdit::AddUserButton(const CString& sName, UINT nIDC, const CString& sBitmapResource, LPCTSTR szToolTip)
{
	AddFooterButton(sName, nIDC, sBitmapResource, szToolTip);
	return;
	/*
		//ASSERT_TRACE(BEGetExStyle(BE_STYLE_SHOW_MINI_TOOLBAR), "Add button on hidden mini toolbar\n");
		if (!BEGetExStyle(BE_STYLE_SHOW_MINI_TOOLBAR))
		{
		if (BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR))
		{
		TRACE2("Add button on hidden mini toolbar (%s, %d)\n", sName, nIDC);

		AddFooterButton(sName, nIDC, sBitmapResource, szToolTip);
		return;
		}
		ASSERT_TRACE2(FALSE, "Add button on hidden mini toolbar (%s, %d)\n", sName, nIDC);
		}

		for (int i = 0; i <= m_UserButtons.GetUpperBound(); i++)
		if
		(
		(((CBodyBitmapButton*)m_UserButtons[i])->m_nButtonIDC == nIDC)
		||
		((!sName.IsEmpty()) && sName.CompareNoCase(((CBodyBitmapButton*)m_UserButtons[i])->GetInfoOSL()->m_Namespace.GetObjectName()) == 0)
		)
		{
		TRACE("BodyEdit::AddUserButton: button ID=%u and Name=%s already defined\n", nIDC, sName);
		ASSERT(FALSE);
		return;
		}

		CBodyBitmapButton* pB = new CBodyBitmapButton(nIDC, sBitmapResource, sBitmapResource, sBitmapResource, szToolTip);
		if (!sName.IsEmpty())
		{
		pB->GetInfoOSL()->SetType(OSLType_ToolbarButton);
		pB->GetInfoOSL()->m_pParent = GetInfoOSL();
		pB->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::TOOLBARBUTTON, sName, GetNamespace());

		AfxGetSecurityInterface()->GetObjectGrant(pB->GetInfoOSL());
		}
		m_UserButtons.Add(pB);
		*/
}

//-----------------------------------------------------------------------------
void CBodyEdit::AddUserButton(UINT nIDC, UINT nIDBU, UINT nIDBD, UINT nIDBDI, LPCTSTR szToolTip)
{
	AddUserButton(cwsprintf(_T("button%d"), nIDC), nIDC, nIDBU, nIDBD, nIDBDI, szToolTip);
}

//-----------------------------------------------------------------------------
void CBodyEdit::AddUserReportButton(UINT nIDC, LPCTSTR sToolTip, BOOL bEnable/*=TRUE*/, BOOL bDisableOnReadOnly /*=FALSE*/)
{
	AddUserButton
	(
		_NS_BEUSRBTN("ReportSheets"),
		nIDC,
		TBIcon(szIconReport, MINI),
		sToolTip
	);
	EnableUserButton(bEnable, bDisableOnReadOnly, nIDC);
}

//-----------------------------------------------------------------------------
CBEButton* CBodyEdit::AddHeaderButton(LPCTSTR szName, UINT nIDC, UINT nIDB, LPCTSTR szToolTip, BOOL bEnable/*=TRUE*/, BOOL bDisableOnReadOnly /*=FALSE*/)
{
	CBEButton* pBtn = m_HeaderToolBar.AddButton
	(
		szName,
		nIDC,
		nIDB,
		szToolTip,
		L"", //testo vicino all'immagine
		0	//prima posizione
	);

	pBtn->m_bDisableOnReadOnly = bDisableOnReadOnly;
	pBtn->EnableButton(bEnable);
	return pBtn;
}

//-----------------------------------------------------------------------------
CBEButton* CBodyEdit::AddHeaderButton(LPCTSTR szName, UINT nIDC, const CString& sBitmapResource, LPCTSTR szToolTip, BOOL bEnable /*= TRUE*/, BOOL bDisableOnReadOnly /*= FALSE*/)
{
	CBEButton* pBtn = m_HeaderToolBar.AddButton
	(
		szName,
		nIDC,
		0,
		szToolTip,
		L"", //testo vicino all'immagine
		0	//prima posizione
	);

	pBtn->m_bDisableOnReadOnly = bDisableOnReadOnly;
	pBtn->EnableButton(bEnable);

	pBtn->m_nsImage = sBitmapResource;
	return pBtn;
}

//-----------------------------------------------------------------------------
CBEButton* CBodyEdit::AddFooterButton(LPCTSTR szName, UINT nIDC, UINT nIDB, LPCTSTR sToolTip, BOOL bEnable/*=TRUE*/, BOOL bDisableOnReadOnly /*=FALSE*/)
{
	CBEButton* pBtn = m_FooterToolBar.AddButton
	(
		szName,
		nIDC,
		nIDB,
		sToolTip,
		L"", //testo vicino all'immagine
		0	//prima posizione
	);

	pBtn->m_bDisableOnReadOnly = bDisableOnReadOnly;
	pBtn->EnableButton(bEnable);
	return pBtn;
}

//-----------------------------------------------------------------------------
CBEButton* CBodyEdit::AddFooterButton(LPCTSTR szName, UINT nIDC, const CString& sBitmapResource, LPCTSTR sToolTip, BOOL bEnable/*=TRUE*/, BOOL bDisableOnReadOnly /*=FALSE*/)
{
	CBEButton* pBtn = m_FooterToolBar.AddButton
	(
		szName,
		nIDC,
		0,
		sToolTip,
		L"", //testo vicino all'immagine
		0	//prima posizione
	);

	pBtn->m_bDisableOnReadOnly = bDisableOnReadOnly;
	pBtn->EnableButton(bEnable);

	pBtn->m_nsImage = sBitmapResource;
	return pBtn;
}

//-----------------------------------------------------------------------------
CBEButton* CBodyEdit::AddFooterReportButton(UINT nIDC, LPCTSTR sToolTip, BOOL bEnable/*=TRUE*/, BOOL bDisableOnReadOnly /*=FALSE*/)
{
	return AddFooterButton(_NS_BEUSRBTN("ReportSheets"), nIDC, TBIcon(szIconReport, MINI), sToolTip);
}

//-----------------------------------------------------------------------------
void CBodyEdit::ChangeUserButton(UINT nIDC, UINT nIDBU, UINT nIDBD, UINT nIDBDI, LPCTSTR szToolTip/*= NULL*/)
{
	ASSERT(nIDC && nIDBU && nIDBD && nIDBDI);

	//if (!BEGetExStyle(BE_STYLE_SHOW_MINI_TOOLBAR))
	{
		TRACE1("ChangeUserButton on hidden mini toolbar (%d)\n", nIDC);

		if (BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR))
		{
			CBEButton* pBtn = m_FooterToolBar.FindButton(nIDC);
			if (pBtn)
			{
				pBtn->ChangeImage(nIDBU);
				return;
			}
		}
		ASSERT_TRACE1(FALSE, "ChangeUserButton on hidden mini toolbar (%d)\n", nIDC);
	}

	ASSERT_TRACE1(FALSE, "BodyEdit::ChangeUserButton: not found button ID=%u\n", nIDC);
}

//-----------------------------------------------------------------------------
void CBodyEdit::ChangeUserButton(UINT nIDC, const CString& sBitmapResource, LPCTSTR szToolTip/*= NULL*/)
{
	ASSERT(nIDC && !sBitmapResource.IsEmpty());

	//if (!BEGetExStyle(BE_STYLE_SHOW_MINI_TOOLBAR))
	{
		TRACE1("ChangeUserButton on hidden mini toolbar (%d)\n", nIDC);

		if (BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR))
		{
			CBEButton* pBtn = m_FooterToolBar.FindButton(nIDC);
			if (pBtn)
			{
				pBtn->ChangeImage(0, sBitmapResource);
				return;
			}
		}
		ASSERT_TRACE1(FALSE, "ChangeUserButton on hidden mini toolbar (%d)\n", nIDC);
	}

	ASSERT_TRACE1(FALSE, "BodyEdit::ChangeUserButton: not found button ID=%u\n", nIDC);
}

//-----------------------------------------------------------------------------
void CBodyEdit::EnableUserButton(BOOL bEnable, BOOL bDisableOnReadOnly /* =FALSE */, UINT nIDC /* =0 */)
{
	//if (!BEGetExStyle(BE_STYLE_SHOW_MINI_TOOLBAR))
	{
		if (BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR))
		{

			CBEButton* pBtn = this->m_FooterToolBar.FindButton(nIDC);
			if (pBtn)
			{
				pBtn->m_bDisableOnReadOnly = bDisableOnReadOnly;
				pBtn->EnableButton(bEnable);

				TRACE1("Enable User button on hidden mini toolbar (%d)\n", nIDC);
			}
			else if (!nIDC && m_FooterToolBar.GetSize())
			{
				pBtn = m_FooterToolBar.GetAt(0);
				if (!IsInternalCommand(pBtn->m_nID))
				{
					pBtn->m_bDisableOnReadOnly = bDisableOnReadOnly;
					pBtn->EnableButton(bEnable);

					TRACE1("Enable User button on 0 position on hidden mini toolbar (%d)\n", pBtn->m_nID);
				}
			}
			return;
		}
		ASSERT_TRACE(FALSE, "Enable User button on hidden mini toolbar\n");
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::UpdateColumnSizes()
{
	//TODO Eliminare doppio ciclo sulle colonne soprattutto chiamata pColInfo->ResizeColumn
	BodyEditInfo* pBEObject = NULL;
	BodyEditColumn* pBEC = NULL;
	CAbstractFormDoc* pDoc = GetDocument();
	if (pDoc && pDoc->m_pFormManager)
		pBEObject = pDoc->m_pFormManager->GetBodyEditInfo(GetInfoOSL()->m_Namespace);

	if (pBEObject && !pBEObject->IsKindOf(RUNTIME_CLASS(BodyEditInfo)))
	{
		ASSERT(FALSE);
		pBEObject = NULL;
	}

	// it's updated by ColumnInfo::ResizeColumn
	int	xOffset = 0;

	// update columns and controls sizes
	for (int i = 0; i < m_ColumnsInfo.GetSize(); i++)
	{
		ColumnInfo* pColInfo = m_ColumnsInfo[i];

		BodyEditColumn* pBEC = pBEObject
			? pBEObject->GetColumnObject(pColInfo->GetInfoOSL()->m_Namespace)
			: NULL;

		// per poter ottenere il calcolo corretto delle aree devo comunicare ai singoli
		// controls che esistono anche i bottoni di stato. Uso il template, tanto è solo temporaneo
		if (m_pDBT)
			pColInfo->AttachCtrlStateData(m_pDBT->GetRecord(), NULL);

		pColInfo->ResizeColumn(GetFont(), xOffset, m_rectBody.Width());
		pColInfo->GetParsedCtrl()->DetachAllStateData();

		pColInfo->m_pParsedCtrl->EnableCtrl(FALSE);

		// memorizzo i valori di default come definiti dal programmatore
		if (pBEC)
			pBEC->m_nDefaultColScreenWidth = pColInfo->m_nDefaultColScreenWidth;
	}

	if (pBEObject)
	{
		// it's updated by ColumnInfo::ResizeColumn
		int	xOffset = 0;
		// Ricalcolo tutto utilizzando i valori salvati dal form manager
		for (int i = 0; i < m_ColumnsInfo.GetSize(); i++)
		{
			ColumnInfo* pColInfo = m_ColumnsInfo[i];

			if ((pBEC = pBEObject->GetColumnObject(pColInfo->GetInfoOSL()->m_Namespace)) != NULL)
			{
				if (pBEC->m_nColScreenWidth > 0)
					pColInfo->SetScreenWidth(pBEC->m_nColScreenWidth, TRUE);
				else
					pBEC->m_nColScreenWidth = pColInfo->GetScreenWidth();

				pColInfo->SetScreenWidth(pBEC->m_nColScreenWidth, TRUE);
			}
			pColInfo->ResizeColumn(GetFont(), xOffset, m_rectBody.Width());
		}
	}
	if (m_bUseColumnHeaderSmallFont)
		RecalcColumnsTitleWidth();
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::OnCreateClient()
{
	if (m_pOwnFont)
		SetFont(m_pOwnFont);

	//////////////////////////////////
	// Ridimensiona le colonne
	//
	if (BEGetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK_INTERACTIVE))
	{
		for (int c = 0; c <= m_AllColumnsInfo.GetUpperBound(); c++)
		{
			m_AllColumnsInfo.GetAt(c)->SetTitleExtraRSpace(m_AllColumnsInfo.GetAt(c)->GetTitleExtraRSpace() + LOCK_EXTRARSPACE);
		}
	}

	if (BEGetExStyle(BE_STYLE_ALLOW_SORT))
	{
		for (int c = 0; c <= m_AllColumnsInfo.GetUpperBound(); c++)
		{
			m_AllColumnsInfo.GetAt(c)->SetTitleExtraRSpace(m_AllColumnsInfo.GetAt(c)->GetTitleExtraRSpace() + SORT_EXTRARSPACE);
		}
	}

	if (BEGetExStyle(BE_STYLE_SHOW_DATATIP))
	{
		GetParent()->EnableToolTips();

		if (m_pDataTip)
		{
			m_pDataTip->Create(this);
			m_pDataTip->On(TRUE);
		}
	}

	if ((GetStyle() & WS_CLIPCHILDREN) != WS_CLIPCHILDREN)
	{
		TRACE1("CBodyEdit::OnCreateClient: %d: WS_CLIPCHILDREN style not specified in resource", GetDlgCtrlID());
		ASSERT(FALSE);
	}

	InitSizeInfo(this);

	// create all controls
	int i = 0;
	for (i = 0; i < m_AllColumnsInfo.GetSize(); i++)
	{
		ColumnInfo* pCol = m_AllColumnsInfo[i];
		if (!pCol->CreateCtrl(this))
			return FALSE;
	}

	CalcBodyLayout();

	UpdateColumnSizes();

	// creates the last columns index array
	if (!CreateAccessories())
		return FALSE;

	if (BEGetExStyle(BE_STYLE_SHOW_HEADER_TOOLBAR))
	{
		//AddStandardButtons(m_BtnHeaderToolBarArray);
		if (m_HeaderToolBar.Create(L"HeaderToolbar"))
			m_HeaderToolBar.SetPos();
	}
	if (BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR))
	{
		//AddStandardButtons(m_FooterToolBar.m_Buttons);
		if (m_FooterToolBar.Create(L"FooterToolbar"))
			m_FooterToolBar.SetPos();
	}

	RepositionAccessories();

	if (!RecalcRowsLayout())
		return FALSE;

	//////////////////////////////////
	// registro il body come drop target...
	if (m_pRtDropTarget && !m_pDropTarget)
	{
		// creo il drop target...
		m_pDropTarget = (CTBExtBEDropTarget*)m_pRtDropTarget->CreateObject();
		ASSERT(m_pDropTarget->IsKindOf(RUNTIME_CLASS(CTBExtBEDropTarget)));
		m_pDropTarget->AttachTarget(this);

		m_IdxDragRecDest = -1;
	}

	if (!OnPostCreateClient())
		return FALSE;

	if (GetDocument() && !GetDocument()->DispatchOnPostCreateClient(this))
		return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CreateAccessories()
{
	DWORD dwCommonStyles = WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS;
	CRect rect(0, 0, 0, 0);

	if (BEGetExStyle(BE_STYLE_SHOW_VERT_SCROLLBAR))
	{
		m_pVScrollBar = new CTBScrollBar();
		if (!m_pVScrollBar->Create(SBS_VERT | dwCommonStyles, rect, this, IDC_BE_VSCROLLBAR))
		{
			TRACE0("BodyEdit::CreateAccessories: failed to create vertical scroll bar");
			SAFE_DELETE(m_pVScrollBar);
		}
		else
		{
			SCROLLINFO sinfo;
			if (m_pVScrollBar->GetScrollInfo(&sinfo))
			{
				sinfo.nPage = 1;
				VERIFY(m_pVScrollBar->SetScrollInfo(&sinfo));
			}
		}
	}

	if (BEGetExStyle(BE_STYLE_SHOW_HORIZ_SCROLLBAR) && !AfxGetThemeManager()->GetBodyEditScrollBarInToolBar())
	{
		m_pHScrollBar = new CTBScrollBar();
		if (!m_pHScrollBar->Create(SBS_HORZ | dwCommonStyles, rect, this, IDC_BE_HSCROLLBAR))
		{
			TRACE0("BodyEdit::CreateAccessories: failed to create horizontal scroll bar");
			SAFE_DELETE(m_pHScrollBar);
		}
		else
		{
			SCROLLINFO sinfo;
			if (m_pHScrollBar->GetScrollInfo(&sinfo))
			{
				sinfo.nPage = 1;
				VERIFY(m_pHScrollBar->SetScrollInfo(&sinfo));
			}
		}
	}

	// creates the button below of the VScrollBar
	DWORD dwBtnStyle;

	dwBtnStyle = BS_FLAT | BS_OWNERDRAW | dwCommonStyles;

	SetBodyReadOnly(TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBodyEdit::CalcBodyLayout()
{
	CRect rectClient;
	GetClientRect(rectClient);

	// diminuisce la client area per disegnare la cornice 3D
	rectClient.left += 2;	// increase the left and rect for include the black border
	rectClient.top += 1;
	rectClient.bottom -= 2;
	rectClient.right -= 2;

	m_rectBody = rectClient;

	// sets the position of titles bar
	m_rectTitles.top = m_rectBody.top;
	m_rectTitles.left = m_rectBody.left;
	m_rectTitles.right = m_rectBody.right;

	if (BEGetExStyle(BE_STYLE_SHOW_COLUMN_HEADERS))
	{
		CFont* pFont = GetFont();

		// calculate new body top and title bottom coordinates
		//m_rectBody.top += GetEditSize(pFont, 1).cy - 1;
		CDC* pDC = GetDC();

		if (IsScale())
		{
			m_rectBody.top += (long)(GetEditSize(pDC, pFont, 1).cy  * m_dUITitlesRows) + ScalePix(7);
		}
		else
		{
			m_rectBody.top += (long)(GetEditSize(pDC, pFont, 1).cy  * m_dUITitlesRows) + 1;
		}

		ReleaseDC(pDC);
	}
	m_rectTitles.bottom = m_rectBody.top;

	// calculate body right and title rigth coordinates excluding right vertical
	// scroll bar

	if (BEGetExStyle(BE_STYLE_SHOW_HORIZ_SCROLLBAR) && !AfxGetThemeManager()->GetBodyEditScrollBarInToolBar())
	{
		m_rectBody.bottom -= GetSystemMetrics(SM_CYHSCROLL);
	}

	if (m_pVScrollBar && m_pVScrollBar->isVisible())
	{
		int nXVScrollSize = GetSystemMetrics(SM_CXVSCROLL);
		m_rectBody.right -= nXVScrollSize;
		m_rectTitles.right -= nXVScrollSize;
	}

	//auxtoolbars
	if (BEGetExStyle(BE_STYLE_SHOW_HEADER_TOOLBAR) && m_HeaderToolBar.GetSize() > 0)
	{
		// abbasso e restringo m_rectBody
		m_rectBody.top += m_HeaderToolBar.m_nHeight;

		// determino m_HeaderToolBar.m_rectToolBar ed abbasso m_rectTitles
		m_HeaderToolBar.m_rectToolBar.left = rectClient.left;
		m_HeaderToolBar.m_rectToolBar.right = rectClient.right;
		m_HeaderToolBar.m_rectToolBar.top = rectClient.top;

		if (BEGetExStyle(BE_STYLE_SHOW_COLUMN_HEADERS))
		{
			m_rectTitles.OffsetRect(0, m_HeaderToolBar.m_nHeight);

			m_HeaderToolBar.m_rectToolBar.bottom = m_rectTitles.top;
		}
		else
			m_HeaderToolBar.m_rectToolBar.bottom = m_rectBody.top;
	}

	if (BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR) && m_FooterToolBar.GetSize() > 0)
	{
		// restringo m_rectBody
		m_rectBody.bottom -= m_FooterToolBar.m_nHeight;

		// determino m_FooterToolBar.m_rectToolBar
		m_FooterToolBar.m_rectToolBar.left = rectClient.left;
		m_FooterToolBar.m_rectToolBar.right = rectClient.right;
		m_FooterToolBar.m_rectToolBar.top = rectClient.bottom - m_FooterToolBar.m_nHeight;
		m_FooterToolBar.m_rectToolBar.bottom = rectClient.bottom;
	}
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::RecalcRowsLayout()
{
	ASSERT(m_hWnd);
	// get a standard edit size to have the rows height                                         
	// (minus one to overlap line separators)
	CDC* pDC = GetDC();
	m_nRowsHeight = GetEditSize(pDC, GetFont(), 1, m_nLinesPerRow > 0 ? m_nLinesPerRow : 1).cy - ScalePix(1);
	
	// extra inner space 4k
	if (IsScale())
		m_nRowsHeight += ScalePix(10);

	ReleaseDC(pDC);

	if (!m_pDBT)
		return TRUE;

	if (!m_ColumnsInfo.GetSize())
		return TRUE;

	int nPrevLastBodyRow = m_nLastBodyRow;

	// set max row index
	m_nLastBodyRow = (m_rectBody.Height() / m_nRowsHeight) - 1;

	BOOL bOk = m_nLastBodyRow >= 0 && m_rectBody.Height() >= m_nRowsHeight;
	if (!bOk)
	{
		TRACE2("Bodyedit [class:%s,IDC:%d] height is short to display rows\n",
			(LPCTSTR)CString(this->GetRuntimeClass()->m_lpszClassName),
			this->GetDlgCtrlID());

		int offset = m_nRowsHeight - m_rectBody.Height() + 1;

		m_rectBody.bottom += offset;

		if (m_pHScrollBar)
		{
			m_rectHScrollBar.top += offset;
			m_rectHScrollBar.bottom += offset;
		}
		m_FooterToolBar.m_rectToolBar.top += offset;
		m_FooterToolBar.m_rectToolBar.bottom += offset;

		bOk = TRUE;
	}
	if (m_nLastBodyRow < 0) m_nLastBodyRow = 0;

	if (
		nPrevLastBodyRow != m_nLastBodyRow &&
		m_pDBT->GetPreloadStep() > 0 &&
		m_pDBT->GetPreloadStep() != max(15, m_nLastBodyRow * 2)
		)
	{
		m_pDBT->SetPreloadStep(max(15, m_nLastBodyRow * 2));

		// La RecalcRowsLayout() chiamata dalla OnCreateClient potrebbe trovare
		// il DBT vuoto in caso di DelayedRead
		if (m_pDBT->m_bLoaded)
			LoadNextRecords();
	}

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::RecalcColumnsLayout()
{
	// update columns and controls sizes
	int	xOffset = 0;	// it's updated by ColumnInfo::ResizeColumn
	m_bHasFooter = FALSE;
	for (int i = 0; i < m_ColumnsInfo.GetSize(); i++)
	{
		ColumnInfo* pInfo = m_ColumnsInfo[i];
		pInfo->ResizeColumn(GetFont(), /* by ref */xOffset, m_rectBody.Width(), TRUE);
		if (pInfo->HasFooter())
			m_bHasFooter = TRUE;
	}

	RepositionAccessories();

	Invalidate();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBodyEdit::RepositionAccessories()
{
	int nXHScrollSize = 0;
	int nYHScrollSize = 0;
	int nXVScrollSize = 0;
	int nYVScrollSize = 0;

	// sotto WinNT4 e Win9x questi valori possono essere variati da control panel
	if (BEGetExStyle(BE_STYLE_SHOW_HORIZ_SCROLLBAR) && !AfxGetThemeManager()->GetBodyEditScrollBarInToolBar())
	{
		nXHScrollSize = GetSystemMetrics(SM_CXHSCROLL);
		nYHScrollSize = GetSystemMetrics(SM_CYHSCROLL);
	}

	if (m_pVScrollBar  && m_pVScrollBar->isVisible())
	{
		nXVScrollSize = GetSystemMetrics(SM_CXVSCROLL);
		nYVScrollSize = GetSystemMetrics(SM_CYVSCROLL);
	}

	CRect rectVScroll;
	rectVScroll.top = m_rectTitles.bottom + 1;
	rectVScroll.right = m_rectBody.right + nXVScrollSize;

	// nr. of button below of VScrollBar ----------------------------------------------v
	int nrbbvs = (m_nLinesPerRow > 0 ? 1 : 0) + (BEGetExStyle(BE_STYLE_SHOW_ROW_HOME_END) ? 2 : 0);

	rectVScroll.bottom = m_rectBody.bottom + nYHScrollSize - nrbbvs * nYVScrollSize;

	rectVScroll.left = m_rectBody.right;

	if (m_pVScrollBar && m_pVScrollBar->m_hWnd)
		m_pVScrollBar->SetWindowPos(NULL, rectVScroll.left, rectVScroll.top, rectVScroll.Width(), rectVScroll.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);

	CRect rectDraw
	(
		rectVScroll.left, rectVScroll.bottom,
		rectVScroll.right, rectVScroll.bottom + nYVScrollSize
	);
	/*
		if (m_nLinesPerRow > 0 && m_pBtnSpinRows && m_pBtnSpinRows->m_hWnd)
		{
		m_pBtnSpinRows->SetWindowPos(NULL, rectDraw.left, rectDraw.top, rectDraw.Width(), rectDraw.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);
		rectDraw.OffsetRect(0, nYVScrollSize);
		}

		if (BEGetExStyle(BE_STYLE_SHOW_ROW_HOME_END) && m_pBtnGoHome && m_pBtnGoHome->m_hWnd  && m_pBtnGoEnd && m_pBtnGoEnd->m_hWnd)
		{
		m_pBtnGoHome->SetWindowPos(NULL, rectDraw.left, rectDraw.top, rectDraw.Width(), rectDraw.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);

		rectDraw.OffsetRect(0, nYVScrollSize);

		m_pBtnGoEnd->SetWindowPos(NULL, rectDraw.left, rectDraw.top, rectDraw.Width(), rectDraw.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);
		}

		if (BEGetExStyle(BE_STYLE_SHOW_ROWINDICATOR) && m_pRowIndicator && m_pRowIndicator->m_hWnd)
		{
		// dimensiona e crea il row indicator
		CDC* pDC = GetDC();

		CRect sbRect;
		sbRect.right = m_rectBody.right;
		sbRect.left = sbRect.right - GetEditSize(pDC, m_pFontRowIndicator, szRowIndicatorTemplate).cx - 2;
		ReleaseDC(pDC);
		sbRect.bottom = m_rectBody.bottom + nYHScrollSize;// -1 per disegnare un filetto sotto
		sbRect.top = m_rectBody.bottom + 3;				// +1 per disegnare un filetto sopra

		m_pRowIndicator->SetWindowPos(NULL, sbRect.left, sbRect.top, sbRect.Width(), sbRect.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);
		}
		*/
	int nButtons = NumberOfUserButtons();

	// dimensiona il rettangolo ove dovrebbe essere posizionata la scroll bar orizzontale
	m_rectHScrollBar.left = m_rectBody.left + (nButtons * nXHScrollSize);
	m_rectHScrollBar.top = m_rectBody.bottom;
	m_rectHScrollBar.bottom = m_rectBody.bottom + nYHScrollSize;
	m_rectHScrollBar.right = m_rectBody.right;
	/*
		if (BEGetExStyle(BE_STYLE_SHOW_ROWINDICATOR) && m_pRowIndicator && m_pRowIndicator->m_hWnd)
		{
		CDC* pMyDC = GetDC();

		m_rectHScrollBar.right = m_rectBody.right - GetEditSize(pMyDC, m_pFontRowIndicator, szRowIndicatorTemplate).cx - 2;

		ReleaseDC(pMyDC);
		}
		*/
	MakeLastColsIdxArray();

	if (m_pHScrollBar)
	{
		m_pHScrollBar->SetWindowPos
		(
			NULL,
			m_rectHScrollBar.left, m_rectHScrollBar.top,
			m_rectHScrollBar.Width(), m_rectHScrollBar.Height(),
			SWP_NOZORDER
		);
	}

	if (FindHScrollBar())
	{
		int nLastIdx = GetStartIdxForLastColumn();
		SetHScrollRange(0, nLastIdx);
		CenterHScrollCenter();
		UpdateHScrollPos();

		//if (nLastIdx > 0)
		if (!IsLastColumnInScreen())
		{
			// le colonne vanno oltre il margine destro quindi occorre la ScrollBar orizzontale
			SetHVisible(TRUE);
			SetHEnableWindow(TRUE);

		}
		else
		{
			DoScrollLeft(0);
			SetHEnableWindow(FALSE);
			SetHVisible(FALSE);
		}
	}
	// rettangolo del primo bottone 
	CRect rectDrawHBtn
	(
		m_rectBody.left, m_rectBody.bottom,
		m_rectBody.left + nXHScrollSize, m_rectBody.bottom + nYHScrollSize
	);
	/*
		// crea un bottone custom se richiesto dal programmatore
		for (int i = 0; i <= m_UserButtons.GetUpperBound(); i++)
		{
		((CBodyBitmapButton*)m_UserButtons[i])->SetWindowPos
		(
		NULL,
		rectDrawHBtn.left, rectDrawHBtn.top,
		rectDrawHBtn.Width(), rectDrawHBtn.Height(),
		SWP_SHOWWINDOW | SWP_NOZORDER
		);

		rectDrawHBtn.OffsetRect(nXHScrollSize, 0);
		}

		if (BEGetExStyle(BE_STYLE_ALLOW_INSERT) && m_pBtnInsert && m_pBtnInsert->m_hWnd)
		{
		m_pBtnInsert->SetWindowPos(NULL, rectDrawHBtn.left, rectDrawHBtn.top, rectDrawHBtn.Width(), rectDrawHBtn.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);
		rectDrawHBtn.OffsetRect(nXHScrollSize, 0);
		}

		if (BEGetExStyle(BE_STYLE_ALLOW_DELETE) && m_pBtnDelete && m_pBtnDelete->m_hWnd)
		{
		m_pBtnDelete->SetWindowPos(NULL, rectDrawHBtn.left, rectDrawHBtn.top, rectDrawHBtn.Width(), rectDrawHBtn.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);
		rectDrawHBtn.OffsetRect(nXHScrollSize, 0);
		}

		if (BEGetExStyle(BE_STYLE_ALLOW_CALLDIALOG) && m_pBtnCallDialog && m_pBtnCallDialog->m_hWnd)
		{
		m_pBtnCallDialog->SetWindowPos(NULL, rectDrawHBtn.left, rectDrawHBtn.top, rectDrawHBtn.Width(), rectDrawHBtn.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);
		rectDrawHBtn.OffsetRect(nXHScrollSize, 0);
		}

		if (BEGetExStyle(BE_STYLE_ALLOW_SEARCH) && m_pBtnSearch && m_pBtnSearch->m_hWnd)
		{
		m_pBtnSearch->SetWindowPos(NULL, rectDrawHBtn.left, rectDrawHBtn.top, rectDrawHBtn.Width(), rectDrawHBtn.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);
		rectDrawHBtn.OffsetRect(nXHScrollSize, 0);
		}

		if (BEGetExStyle(BE_STYLE_ALLOW_MULTIPLE_SEL) && m_pBtnMultiSelection && m_pBtnMultiSelection->m_hWnd)
		{
		m_pBtnMultiSelection->SetWindowPos(NULL, rectDrawHBtn.left, rectDrawHBtn.top, rectDrawHBtn.Width(), rectDrawHBtn.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);
		rectDrawHBtn.OffsetRect(nXHScrollSize, 0);
		}
		*/
		//----
	if (BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR) && m_FooterToolBar.GetSize() > 0)
	{
		m_FooterToolBar.SetPos();
	}
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CalculateLastColIdx(int nStartIdx, int nStartWidth, int& nLastIdx)
{
	for (nLastIdx = nStartIdx; nLastIdx <= m_ColumnsInfo.GetUpperBound(); nLastIdx++)
	{
		nStartWidth += m_ColumnsInfo[nLastIdx]->GetScreenWidth();
		if (nStartWidth >= m_rectBody.Width())
		{
			if (nStartWidth > m_rectBody.Width())
				nLastIdx = nLastIdx | FLAG_END_COL_NOT_FULL_VISIBLE;

			return nLastIdx != m_ColumnsInfo.GetUpperBound();
		}
	}

	nLastIdx = m_ColumnsInfo.GetUpperBound();
	return FALSE;
}

//-----------------------------------------------------------------------------
int CBodyEdit::GetStartIdxForLastColumn()
{
	int		nLockedWidth = 0;
	// calcolo la larghezza delle colonne bloccate
	if (ColumnLocked())
	{
		int nLockedColumnIdx = GetVisibleColumnIdx(m_nLockedColumnIDC);
		for (int nLockedCol = 0; nLockedCol <= nLockedColumnIdx; nLockedCol++)
			nLockedWidth += m_ColumnsInfo[nLockedCol]->GetScreenWidth();
	}

	int availableWidth = m_rectBody.Width() - nLockedWidth;
	int index = m_ColumnsInfo.GetUpperBound();
	if (availableWidth < 0)
		return PhysicalToLogicalColIdx(index);

	while (index >= 0)
	{
		availableWidth -= m_ColumnsInfo[index]->GetOriginalScreenWidth();
		if (availableWidth < 0)
			return PhysicalToLogicalColIdx(index + 1);
		index--;
	}

	return 0;
}

//-----------------------------------------------------------------------------
void CBodyEdit::MakeLastColsIdxArray()
{
	BOOL	bMoreCol = TRUE;
	int		nLastIdx = 0, nLockedWidth = 0;
	int		nLockedCol = 0;

	m_LastColsIdx.RemoveAll();
	int nLockedColumnIdx = GetVisibleColumnIdx(m_nLockedColumnIDC);
	// calcolo la larghezza delle colonne bloccate
	if (ColumnLocked())
		for (nLockedCol = 0; nLockedCol <= nLockedColumnIdx; nLockedCol++)
			nLockedWidth += m_ColumnsInfo[nLockedCol]->GetScreenWidth();

	for (int nStartIdx = nLockedCol; nStartIdx <= m_ColumnsInfo.GetUpperBound(); nStartIdx++)
	{
		if (bMoreCol)
		{
			bMoreCol = FALSE;

			if (nStartIdx < m_ColumnsInfo.GetUpperBound())
				bMoreCol = CalculateLastColIdx(nStartIdx, nLockedWidth, nLastIdx);
		}

		// assegno alle colonne bloccate la prima end column trovata
		if (m_LastColsIdx.IsEmpty())
			for (int i = 0; i <= nLockedColumnIdx; i++)
				m_LastColsIdx.Add((WORD)nLastIdx);

		m_LastColsIdx.Add((WORD)nLastIdx);
	}

	ASSERT(m_LastColsIdx.GetSize() == m_ColumnsInfo.GetSize()); //each column must have and ending one

}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::IsLastColumnInScreen()
{
	if (m_ColumnsInfo.GetUpperBound() < 0)
		return TRUE;

	int withBody = m_rectBody.Width();
	if (this->m_pVScrollBar && this->m_pVScrollBar->m_hWnd && m_pVScrollBar->isVisible())
	{
		withBody -= GetSystemMetrics(SM_CXVSCROLL);
	}

	ColumnInfo* pLastCol = m_ColumnsInfo[m_ColumnsInfo.GetUpperBound()];
	int rightLastCol = pLastCol->GetHorzColOffset() + pLastCol->GetScreenWidth();

	return rightLastCol < withBody;
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnWindowPosChanged(WINDOWPOS FAR* wndPos)
{
	BOOL bCtrlVisible = IsCtrlVisible();
	if (bCtrlVisible)
	{
		CRect ctrlRect;
		m_ColumnsInfo[m_nCurrColumnIdx]->m_pParsedCtrl->GetCtrlCWnd()->GetWindowRect(ctrlRect);
		ScreenToClient(ctrlRect);
		bCtrlVisible = !((ctrlRect.bottom > m_rectBody.bottom || ctrlRect.right > m_rectBody.right) && !m_bIconized);

		// Il resize della finestra provocava la sprizione parziale del control => lo si nasconde del tutto e poi lo si riaccende con un post di setfocus (vedi sotto)
		SetFocusOnly();
	}

	m_bIconized = (wndPos->cx == 0 && wndPos->cy == 0 && wndPos->flags == 0x1016);

	CalcBodyLayout();
	RepositionAccessories();

	if (
		!((wndPos->flags & SWP_NOSIZE) == SWP_NOSIZE) && m_ColumnsInfo.GetSize() &&
		BEExistsExStyle(BE_STYLE_ENLARGE_LASTCOLUMN | BE_STYLE_ENLARGE_LASTSTRINGCOLUMN | BE_STYLE_ENLARGE_ALLSTRINGCOLUMNS | BE_STYLE_ENLARGE_CUSTOM)
		)
	{
		ColumnInfo* pLastCol = m_ColumnsInfo[m_ColumnsInfo.GetUpperBound()];
		int hOffLastCol = pLastCol->GetHorzColOffset();
		int lastColWidth = pLastCol->GetScreenWidth();
		int withBody = m_rectBody.Width();
		int vsbw = 0;	//vertical scrollbar width
		if (this->m_pVScrollBar && this->m_pVScrollBar->m_hWnd && m_pVScrollBar->isVisible())
		{
			vsbw = GetSystemMetrics(SM_CXVSCROLL);
		}

		int dispWidth = withBody - vsbw - hOffLastCol - lastColWidth;

		if (
			BEGetExStyle(BE_STYLE_ENLARGE_LASTCOLUMN)
			||
			(
				BEGetExStyle(BE_STYLE_ENLARGE_LASTSTRINGCOLUMN) &&
				pLastCol->GetParsedCtrl() &&
				(
					pLastCol->GetParsedCtrl()->GetDataType() == DataType::String
					||
					pLastCol->GetParsedCtrl()->GetDataType().m_wType == DATA_ENUM_TYPE
					||
					pLastCol->GetControl()->IsKindOf(RUNTIME_CLASS(CDescriptionCombo))
					||
					pLastCol->GetControl()->IsKindOf(RUNTIME_CLASS(CXmlCombo))
					)
				)
			)
		{
			if ((hOffLastCol - vsbw) < withBody)
			{
				int newColWidth = withBody - hOffLastCol - vsbw;
				if (newColWidth > pLastCol->m_nDefaultColScreenWidth)
					pLastCol->SetScreenWidth(newColWidth, FALSE);
			}
		}
		else if (BEExistsExStyle(BE_STYLE_ENLARGE_ALLSTRINGCOLUMNS | BE_STYLE_ENLARGE_CUSTOM)/* && dispWidth > 0*/)
		{
			CArray< ColumnInfo* > arStrColumns;
			int totWidth = 0;
			for (int i = 0; i < m_ColumnsInfo.GetSize(); i++)
			{
				ColumnInfo* pCol = m_ColumnsInfo[i];
				BOOL bGetCol = FALSE;

				if (BEGetExStyle(BE_STYLE_ENLARGE_ALLSTRINGCOLUMNS))
				{

					bGetCol = (
						pCol->GetParsedCtrl()->GetDataType() == DataType::String
						||
						pCol->GetParsedCtrl()->GetDataType().m_wType == DATA_ENUM_TYPE
						||
						pLastCol->GetControl()->IsKindOf(RUNTIME_CLASS(CDescriptionCombo))
						||
						pLastCol->GetControl()->IsKindOf(RUNTIME_CLASS(CXmlCombo))
						);
				}
				else if (BEGetExStyle(BE_STYLE_ENLARGE_CUSTOM))
				{
					bGetCol = pCol->GetAllowEnlarge();
				}

				if (bGetCol)
				{
					arStrColumns.Add(pCol);
					totWidth += pCol->GetScreenWidth();
				}
			}

			if (arStrColumns.GetSize())
			{
				SuspendLayout();
				for (int i = 0; i < arStrColumns.GetSize(); i++)
				{
					ColumnInfo* pCol = arStrColumns[i];
					int w = pCol->GetScreenWidth();

					int nw = (dispWidth * w) / totWidth;
					pCol->SetScreenWidth(max(w + nw, pCol->m_nDefaultColScreenWidth), FALSE);
				}
				ResumeLayout(FALSE);

				if (bCtrlVisible)
					PostMessage(WM_SETFOCUS, (WPARAM)m_hWnd, NULL);
				return;
			}
		}
	}

	RecalcRowsLayout();
	SetVScrollRange();

	if (bCtrlVisible)
		PostMessage(WM_SETFOCUS, (WPARAM)m_hWnd, NULL);
}

//-----------------------------------------------------------------------------
void CBodyEdit::InvalidateBody()
{
	// per includere il readraw del carret nel caso in cui questo
	// sia parzialmente sovrapposto ai titoli (carret sulla prima riga)
	// si invalida un rettangolo piu` alto di un pixel
	//CRect rect(m_rectBody);
	//rect.top--;
	//InvalidateRect(rect);

	Invalidate();
}

void CBodyEdit::Invalidate(BOOL bErase/*=TRUE*/)
{
	if (m_nLayoutSuspended)
		return;

	__super::Invalidate(bErase);

	m_HeaderToolBar.Invalidate();
	m_FooterToolBar.Invalidate();

	CTBScrollBar* pScroll = FindHScrollBar();
	if (pScroll)
		pScroll->Invalidate();
}

//-----------------------------------------------------------------------------
CTBScrollBar* CBodyEdit::FindHScrollBar()
{
	if (m_pHScrollBar)
	{
		return m_pHScrollBar;
	}
	else
	{
		CBEButton* pB = m_FooterToolBar.FindButton(IDC_BE_HSCROLLBAR);
		if (!pB)
			pB = m_HeaderToolBar.FindButton(IDC_BE_HSCROLLBAR);
		if (!pB)
			return NULL;
		if (!pB->IsKindOf(RUNTIME_CLASS(CBEBtnCtrl)))
			return NULL;

		CTBScrollBar* pSbar = (CTBScrollBar*)(((CBEBtnCtrl*)pB)->m_pControl);
		if (!pSbar->m_hWnd || !pSbar->IsKindOf(RUNTIME_CLASS(CTBScrollBar)))
			return NULL;

		return pSbar;
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::CenterHScrollCenter()
{
	CTBScrollBar* pScrollBar = CBodyEdit::FindHScrollBar();
	if (!pScrollBar) return;
	CRect recBar, recScroll;
	GetWindowRect(recBar);
	pScrollBar->GetWindowRect(recScroll);
	pScrollBar->SetWindowPos(NULL, recScroll.left, recBar.top + 5, recScroll.Width(), recScroll.Height(), SWP_SHOWWINDOW | SWP_NOZORDER);
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetHScrollRange()
{
	if (m_nLayoutSuspended > 0 || !m_pDBT)
		return;

	int nLastPos = GetVisibleColumnsInfoUpperBound();

	if (nLastPos > 0)
		SetHScrollRange(0, nLastPos);

	UpdateHScrollPos();
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetHScrollRange(int nMin, int nMax)
{
	if (m_nLayoutSuspended > 0)
		return;

	CTBScrollBar* pScrollBar = CBodyEdit::FindHScrollBar();
	if (!pScrollBar) return;
	pScrollBar->SetScrollRange(nMin, nMax);
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetHScrollPos(int nPos)
{
	CTBScrollBar* pScrollBar = CBodyEdit::FindHScrollBar();
	if (!pScrollBar) return;
	pScrollBar->SetScrollPos(nPos);
}

//-----------------------------------------------------------------------------
void CBodyEdit::UpdateHScrollPos()
{
	InvalidateRect(m_rectTitles);
	SetHScrollPos(m_nStartColumnIdx);
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetHEnableWindow(BOOL bEnable)
{
	CTBScrollBar* pScrollBar = CBodyEdit::FindHScrollBar();
	if (!pScrollBar) return;
	pScrollBar->EnableWindow(bEnable);
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetHVisible(BOOL bVisible)
{
	CTBScrollBar* pScrollBar = CBodyEdit::FindHScrollBar();
	if (!pScrollBar) return;

	CBEButton* pB = m_FooterToolBar.FindButton(IDC_BE_HSCROLLBAR);
	if (pB)
		pB->ShowButton(bVisible);
	else
		pScrollBar->ShowWindow(bVisible ? SW_SHOWNORMAL : SW_HIDE);
}

//-----------------------------------------------------------------------------
void CBodyEdit::ShowRowIndicator()
{
	if (!m_hWnd)
		return;

	CBEButton* pB = m_HeaderToolBar.FindButton(IDC_BE_ROWINDICATOR);
	if (!pB)
		pB = m_FooterToolBar.FindButton(IDC_BE_ROWINDICATOR);
	if (pB)
	{
		ASSERT_KINDOF(CBEBtnCtrl, pB);

		CBEBtnCtrl* pBtnRowIndicator = dynamic_cast<CBEBtnCtrl*>(pB);
		ASSERT_VALID(pBtnRowIndicator->m_pControl);
		if (pBtnRowIndicator->m_pControl->m_hWnd)
		{
			CParsedCtrl* pCtrl = GetParsedCtrl(pBtnRowIndicator->m_pControl);
			if (pCtrl)
				pCtrl->SetValue(m_dsRowIndicator);
			else
				pBtnRowIndicator->m_pControl->SetWindowText(m_dsRowIndicator.GetString());

			pBtnRowIndicator->Invalidate();
		}
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnUpdateRowIndicator()
{
	m_dsRowIndicator = cwsprintf(m_sRowIndicatorFormat, m_nCurrRecordIdx + 1, m_pDBT ? max(m_pDBT->GetSize(), m_pDBT->GetTotalRowCount()) : 0);
}

void CBodyEdit::UpdateRowIndicator()
{
	OnUpdateRowIndicator();

	ShowRowIndicator();
}

void CBodyEdit::OnResetRowIndicator()
{
	m_dsRowIndicator = cwsprintf(m_sRowIndicatorFormat, 0, 0);
}

void CBodyEdit::ResetRowIndicator()
{
	OnResetRowIndicator();

	ShowRowIndicator();
}

//-----------------------------------------------------------------------------
void CBodyEdit::RefreshBody(BOOL bRedraw /*= TRUE*/)
{
	if (!m_hWnd)
		return;

	ResizeSelections();

	if (HasFocus())
		SetFocusOnly();

	SetVScrollRange();

	UpdateRowIndicator();

	InvalidateBody();

	if (bRedraw)
	{
		RedrawWindow();

		m_HeaderToolBar.Redraw();
		m_FooterToolBar.Redraw();
	}
}

//=============================================================================
void CBodyEdit::DrawSepLine(CDC* pDC, int nXStart, int nYStart, int nXEnd, int nYEnd)
{
	// draws the dot line	
	pDC->MoveTo(nXStart, nYStart);
	pDC->LineTo(nXEnd, nYEnd);
}

//-----------------------------------------------------------------------------
void CBodyEdit::FillRect(CDC* pDC, const CRect& rect, COLORREF color)
{
	CBrush brushBkgrd;
	brushBkgrd.CreateSolidBrush(color);
	pDC->FillRect(rect, &brushBkgrd);
	brushBkgrd.DeleteObject();
}

//-----------------------------------------------------------------------------
void CBodyEdit::DrawCarret(int nCurrRow, CDC* pDC /* = NULL */)
{
	// per far disegnare il carret si calcola il rettangolo della riga corrente
	int nYRowHilighted = m_pDBT && m_pDBT->IsEmpty() ? m_rectBody.top : -1;
	int nYCoord = m_rectBody.top;
	int nLastRow = min(GetEndRecordIdx() + 1, m_pDBT ? m_pDBT->GetUpperBound() : -1);

	for (int nRow = m_nStartRecordIdx; nRow <= nLastRow; nRow++)
	{
		if (nRow == nCurrRow)
		{
			nYRowHilighted = nYCoord;
			break;
		}

		nYCoord += m_nRowsHeight;
	}

	if (nYRowHilighted < 0)
		return;

	CRect rect = m_rectBody;
	rect.top = nYRowHilighted;
	rect.bottom = rect.top + m_nRowsHeight;
	rect.top--;

	if (m_nLastColumRight > 0 && m_nLastColumRight < rect.right)
		rect.right = this->m_nLastColumRight;

	BOOL bLocalDC = pDC == NULL;
	if (bLocalDC)
		pDC = new CClientDC(this);

	COLORREF crOldBkCol = pDC->SetBkColor(RGB(255, 255, 255));
	pDC->DrawFocusRect(rect);
	pDC->SetBkColor(crOldBkCol);

	if (bLocalDC)
		delete pDC;
}

//-----------------------------------------------------------------------------
void CBodyEdit::DrawTransparentBitmap(CDC* pDC, CBitmap& Bmp, const CPoint& ptBmpDest, int nBmpDestHeight, COLORREF crBkColor)
{
	BITMAP bmInfo;

	CDC	dcImage;
	if (!dcImage.CreateCompatibleDC(pDC))
		return;

	dcImage.SelectObject(&Bmp);
	Bmp.GetObject(sizeof(bmInfo), &bmInfo);

	int nBmpDestWidth = (nBmpDestHeight * bmInfo.bmWidth) / bmInfo.bmHeight + 1;

	CBitmap		bmpMask;
	CDC			dcMask;

	CBitmap* pOldImageBmp = dcImage.SelectObject(&Bmp);

	// Only attempt this if device supports functionality.
	if (pDC->GetDeviceCaps(CAPS1) & C1_TRANSPARENT)
	{
		// Special transparency background mode
		int OldBkgndMode = pDC->SetBkMode(NEWTRANSPARENT);
		COLORREF clrOldBkgnd = pDC->SetBkColor(crBkColor);
		// Actual blt is a simple source copy; transparency is automatic.
		pDC->StretchBlt
		(
			ptBmpDest.x - nBmpDestWidth,
			ptBmpDest.y,
			nBmpDestWidth,
			nBmpDestHeight,
			&dcImage, 0, 0,
			bmInfo.bmWidth, bmInfo.bmHeight,
			SRCCOPY
		);
		pDC->SetBkColor(clrOldBkgnd);
		pDC->SetBkMode(OldBkgndMode);
		return;
	}

	if
		(	// Create a monochrome bitmap for the mask (True Mask method)
			bmpMask.CreateBitmap(nBmpDestWidth, nBmpDestHeight, 1, 1, NULL) &&
			dcMask.CreateCompatibleDC(pDC)
			)
	{
		// Select the mask bitmap into its DC.
		CBitmap* pMaskOld = dcMask.SelectObject(&bmpMask);
		// Build mask based on transparent colour	
		dcImage.SetBkColor(crBkColor); // Set the transparency color 
		dcMask.StretchBlt
		(
			0,
			0,
			nBmpDestWidth,
			nBmpDestHeight,
			&dcImage, 0, 0,
			bmInfo.bmWidth, bmInfo.bmHeight,
			SRCCOPY
		);

		COLORREF clrOldBkgnd = pDC->SetBkColor(AfxGetThemeManager()->GetEnabledControlBkgColor());
		COLORREF clrOldText = pDC->SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());
		// XOR the image with the destination.
		pDC->StretchBlt
		(
			ptBmpDest.x - nBmpDestWidth,
			ptBmpDest.y,
			nBmpDestWidth,
			nBmpDestHeight,
			&dcImage, 0, 0,
			bmInfo.bmWidth, bmInfo.bmHeight,
			SRCINVERT
		);
		// AND the destination with the mask.
		pDC->BitBlt
		(
			ptBmpDest.x - nBmpDestWidth,
			ptBmpDest.y,
			nBmpDestWidth,
			nBmpDestHeight,
			&dcMask, 0, 0,
			SRCAND
		);
		// XOR the destination with the image again.
		pDC->StretchBlt
		(
			ptBmpDest.x - nBmpDestWidth,
			ptBmpDest.y,
			nBmpDestWidth,
			nBmpDestHeight,
			&dcImage, 0, 0,
			bmInfo.bmWidth, bmInfo.bmHeight,
			SRCINVERT
		);

		if (pMaskOld)
			dcMask.SelectObject(pMaskOld);
		bmpMask.DeleteObject();
		if (pOldImageBmp)
			dcImage.SelectObject(pOldImageBmp);
		pDC->SetBkColor(clrOldBkgnd);
		pDC->SetTextColor(clrOldText);
		return;
	}

	pDC->StretchBlt
	(
		ptBmpDest.x - nBmpDestWidth,
		ptBmpDest.y,
		nBmpDestWidth,
		nBmpDestHeight,
		&dcImage, 0, 0,
		bmInfo.bmWidth, bmInfo.bmHeight,
		SRCCOPY
	);
	if (pOldImageBmp)
		dcImage.SelectObject(pOldImageBmp);
}

//-----------------------------------------------------------------------------
void CBodyEdit::DrawItem(LPDRAWITEMSTRUCT lpDIS)
{
	CDC* pDC = CDC::FromHandle(lpDIS->hDC);
	CRect clientRect;
	GetClientRect(clientRect);

	// creates the memory device contents and prepare it for draw
	CDC		DCMem;		DCMem.CreateCompatibleDC(pDC);
	CBitmap BmpDCMem;	BmpDCMem.CreateCompatibleBitmap(pDC, clientRect.Width(), clientRect.Height());

	COLORREF crSepLines = AfxGetThemeManager()->GetBESeparatorColor();
	COLORREF crBackgnd = AfxGetThemeManager()->GetBackgroundColor();

	CWnd* pParent = m_pWnd->GetParent();
	if (pParent->IsKindOf(RUNTIME_CLASS(CBaseFormView)))
	{
		CBaseFormView* pView = (CBaseFormView*)pParent;
		if (pView->GetBackgroundBrush())
		{
			crBackgnd = pView->GetBackgroundColor();
		}
	}
	else if (pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
	{
		CParsedDialog* pView = (CParsedDialog*)pParent;
		if (pView->GetBackgroundBrush())
		{
			crBackgnd = pView->GetBackgroundColor();
		}
	}

	COLORREF crBkgTitle = AfxGetThemeManager()->GetBETitlesBkgColor() == -1 ? crBackgnd : AfxGetThemeManager()->GetBETitlesBkgColor();

	if (IsWindowEnabled() && BEGetExStyle(BE_STYLE_CHANGE_COLOR))
	{
		if (
			!GetDocument()
			||
			GetDocument()->GetFormMode() == CBaseDocument::EDIT
			||
			GetDocument()->GetFormMode() == CBaseDocument::NEW
			||
			(
			(
				GetDocument()->GetFormMode() == CBaseDocument::NONE
				||
				GetDocument()->GetFormMode() == CBaseDocument::BROWSE
				)
				&&
				GetDocument()->m_bBatch
				)
			)
		{
			crBackgnd = AfxGetThemeManager()->GetEnabledControlBkgColor();
		}
	}

	COLORREF	crOldBakcgnd = DCMem.SetBkColor(crBackgnd);
	CFont*		pOldFont = DCMem.SelectObject(GetFont());
	CBitmap*	pBmpDCMemOld = DCMem.SelectObject(&BmpDCMem);

	FillRect(&DCMem, clientRect, crBackgnd);

	// draw the titles bar
	if (YCoordInRect(m_rectTitles.top, m_rectTitles.bottom, lpDIS->rcItem))
		DrawTitles(&DCMem, lpDIS, crBkgTitle);

	if (BEGetExStyle(BE_STYLE_SHOW_HEADER_TOOLBAR) && m_HeaderToolBar.GetSize() > 0)
	{
		if (YCoordInRect(m_HeaderToolBar.m_rectToolBar.top, m_HeaderToolBar.m_rectToolBar.bottom, lpDIS->rcItem))
		{
			DrawAuxToolBars(&DCMem, lpDIS, m_HeaderToolBar.m_rectToolBar); m_HeaderToolBar.Redraw();

			CPen	pen(PS_SOLID, 1, crSepLines);
			CPen*	pOldPen = DCMem.SelectObject(&pen);

			DrawSepLine(&DCMem, m_HeaderToolBar.m_rectToolBar.left, m_HeaderToolBar.m_rectToolBar.bottom, m_HeaderToolBar.m_rectToolBar.right, m_HeaderToolBar.m_rectToolBar.bottom);

			if (pOldPen) DCMem.SelectObject(&pOldPen);
		}
	}

	// draw body
	DoDraw(&DCMem, lpDIS, crSepLines, crBackgnd);

	if (BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR) && m_FooterToolBar.GetSize() > 0)
	{
		if (YCoordInRect(m_FooterToolBar.m_rectToolBar.top, m_FooterToolBar.m_rectToolBar.bottom, lpDIS->rcItem))
		{
			DrawAuxToolBars(&DCMem, lpDIS, m_FooterToolBar.m_rectToolBar); m_FooterToolBar.Redraw();
		}
	}

	pDC->BitBlt(
		0,
		0,
		clientRect.Width(), clientRect.Height(),
		&DCMem,
		0,
		0,
		SRCCOPY
	);

	// restores the previous graphic objects
	DCMem.SelectObject(pBmpDCMemOld);
	DCMem.SelectObject(pOldFont);
	DCMem.SetBkColor(crOldBakcgnd);
	DCMem.DeleteDC();
	BmpDCMem.DeleteObject();

	// draw frame
	DrawFrame(pDC);
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoDraw(CDC* pDC, LPDRAWITEMSTRUCT lpDIS, COLORREF crSepLines, const COLORREF crBackgnd)
{
	if (m_nLayoutSuspended)
		return;

	CPen	pen(PS_SOLID, 1, crSepLines);
	CPen*	pOldPen = pDC->SelectObject(&pen);

	int nYStart = m_rectBody.top;
	int nYEnd;

	int nLastRow = min(m_nStartRecordIdx + m_nLastBodyRow + 1, m_pDBT ? m_pDBT->GetUpperBound() : -1);
	//ASSERT(nLastRow >= 0 && m_nStartRecordIdx >= 0);

	int right = m_rectBody.right;
	if (m_nLastColumRight > 0 && m_nLastColumRight < right)
		right = m_nLastColumRight;

	if (nLastRow >= 0 && m_nStartRecordIdx >= 0)
	{
		for (int nRow = m_nStartRecordIdx; nRow <= nLastRow; nRow++)
		{
			nYEnd = nYStart + m_nRowsHeight;

			if (YCoordInRect(nYStart, nYEnd, lpDIS->rcItem))
				DrawRow(pDC, lpDIS, nRow, nYStart, nRow == m_nCurrRecordIdx);

			if (!GetHasUINoHorizLines())
				DrawSepLine(pDC, m_rectBody.left, nYEnd - 1, right, nYEnd - 1);

			nYStart = nYEnd;
		}
	}
	else if (HasFocus()/* && !IsReadOnly()*/)
	{
		CRect rect(m_rectBody.left + 2, nYStart, right, nYStart + m_nRowsHeight);
		FillRect(pDC, rect, AfxGetThemeManager()->GetBERowSelectedBkgColor());

		DrawCarret(0, pDC);
	}

	if (m_bHasFooter && m_pDBT && m_pDBT->GetSize())
	{
		nYEnd = nYStart + m_nRowsHeight;

		DrawFooter(pDC, lpDIS, nYStart);

		if (!GetHasUINoHorizLines())
			DrawSepLine(pDC, m_rectBody.left, nYEnd - 1, right, nYEnd - 1);

		nYStart = nYEnd;
	}

	// draws, if necessary, the empty row line at the bottom of client area
	CRect rectFill(m_rectBody);
	rectFill.top = nYStart;
	rectFill.bottom++;

	// paint the background (behind the text)
	BOOL bTwoColors = FALSE;  int x = 0; int pos = 0;
	if (m_ColumnsInfo.GetUpperBound() > -1 && IsWindowEnabled())
	{
		ColumnInfo* pLastCol = m_ColumnsInfo[m_ColumnsInfo.GetUpperBound()];
		x = pLastCol->m_nHorzColOffset + pLastCol->m_nColScrnWidth;
		CTBScrollBar* htb = FindHScrollBar();
		if (htb && ::IsWindow(htb->m_hWnd) && htb->IsWindowEnabled() && htb->isVisible())
		{
			pos = htb->GetScrollPos();
			if (pos >= 0 && pos <= m_ColumnsInfo.GetUpperBound())
			{
				ColumnInfo* pFirstCol = m_ColumnsInfo[pos];
				x -= pFirstCol->m_nHorzColOffset;
			}
		}
		bTwoColors = x < m_rectBody.right;
	}
	if (bTwoColors)
	{
		CRect rectSelection(rectFill);
		rectSelection.right = x;
		FillRect(pDC, rectSelection, crBackgnd);

		CRect rectSelection2(rectFill);
		rectSelection2.left = x + 1;
		FillRect(pDC, rectSelection2, AfxGetThemeManager()->GetBackgroundColor());
	}
	else
		FillRect(pDC, rectFill, IsWindowEnabled() ? pDC->GetBkColor() : AfxGetThemeManager()->GetBackgroundColor());

	nYStart += m_nRowsHeight - 1;

	if (m_nRowsHeight)
	{
		while (m_rectBody.bottom > nYStart)
		{
			if (!GetHasUINoHorizLines())
				DrawSepLine(pDC, m_rectBody.left, nYStart, right, nYStart);

			nYStart += m_nRowsHeight;
		}
	}

	// if the list box is shorter of the client area draw the vertical line
	int nXEnd = m_rectBody.left;
	if (m_LastColsIdx.GetSize())
	{
		int nStartCol = max(0, GetLogicalStartColumnIdx());
		int nLastCol = min(m_ColumnsInfo.GetUpperBound(), GetEndColumnIdx());
		for (int nCol = nStartCol; nCol <= nLastCol; nCol++)
		{
			// calculates the new right coordinate
			ColumnInfo* pCol = m_ColumnsInfo[nCol];
			nXEnd += pCol->GetScreenWidth();

			if (!GetHasUINoVertLines() && ShowUIVertLine(pCol))
				DrawSepLine(pDC, nXEnd, m_rectBody.top, nXEnd, m_rectBody.bottom);
		}
	}

	// restores the previous graphic objects
	pDC->SelectObject(pOldPen);
	pen.DeleteObject();

	if (m_bBodyActive)
		DrawCarret(m_nCurrRecordIdx, pDC);

	//------------------------------------
	{
		// disegno gli indicatori di drop
		if (!m_bTargetDragging && (m_IdxDragRecDest != -1))
			return;

		// cerco la y relativa al record in questione.
		int nLastRow = min(GetEndRecordIdx() + 1, m_pDBT ? m_pDBT->GetUpperBound() : -1);

		int nYStart = m_rectBody.top;
		for (int nRow = m_nStartRecordIdx; nRow <= nLastRow; nRow++)
		{
			if (nRow == m_IdxDragRecDest)
			{
				int nYEnd = nYStart + m_nRowsHeight;
				if (YCoordInRect(nYStart, nYEnd, lpDIS->rcItem))
				{
					CRect rect = m_rectBody;
					rect.top = nYStart;
					rect.bottom = nYEnd;

					switch (m_DragSelType) {
					case DST_CENTER:
						break;
					case DST_BOTTOM:
						rect.top = rect.bottom;
						break;
					case DST_TOP:
						rect.bottom = rect.top;
						break;
					}

					m_DropBmps.Draw(pDC, rect, BMP_TOP | BMP_BOTTOM, FALSE);

					if (m_DragSelType != DST_CENTER)
					{
						CPen	pen(PS_SOLID, 1, CLR_BLACK); //TODO THEME
						CPen*	pOldPen = pDC->SelectObject(&pen);
						int	nLineY = rect.top;
						pDC->MoveTo(m_rectBody.left, nLineY);
						pDC->LineTo(m_rectBody.right, nLineY);
						pDC->SelectObject(pOldPen);
					}
				}

				break;
			}
			nYStart += m_nRowsHeight;
		}

		// locking
		if (!BEGetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK) || GetLockedColumnIDC() == 0)
			return;

		COLORREF crLockedSepLines = AfxGetThemeManager()->GetBELockedSeparatorColor();

		CPen	pen(PS_SOLID, 1, crLockedSepLines);
		CPen*	pOldPen = pDC->SelectObject(&pen);

		// disegno la linea verticale corrispondente alla colonna bloccata
		int nXEnd = m_rectBody.left;
		int nLastCol = min(m_ColumnsInfo.GetUpperBound(), GetEndColumnIdx());
		for (int nCol = max(0, GetLogicalStartColumnIdx()); nCol <= nLastCol; nCol++)
		{
			if (m_ColumnsInfo[nCol]->GetScreenWidth() > 0)
			{
				// calculates the new right coordinate
				nXEnd += m_ColumnsInfo[nCol]->GetScreenWidth();

				if (nCol == GetVisibleColumnIdx(m_nLockedColumnIDC))
				{
					// linea sulla griglia
					DrawSepLine(pDC, nXEnd, m_rectBody.top, nXEnd, m_rectBody.bottom);

					// linea sui titoli
					//if (GetHasUIOutlookStyle())
					//	DrawSepLine	(pDC, nXEnd, m_rectTitles.top+3, nXEnd, m_rectTitles.bottom-2);
				}
			}
		}

		pDC->SelectObject(pOldPen);
		pen.DeleteObject();
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::DrawFrame(CDC* pDC)
{
	if (!BEGetExStyle(BE_STYLE_SHOW_BORDERS))
		return;

	CRect rectClient; GetClientRect(&rectClient);

	// draws the border arround the client area
	CPen* pPen;
	CPen* pOldPen;

	pPen = new CPen(PS_SOLID, 1, AfxGetThemeManager()->GetBESeparatorColor());
	pOldPen = pDC->SelectObject(pPen);

	DrawSepLine(pDC, rectClient.left, rectClient.top, rectClient.left, rectClient.bottom);
	DrawSepLine(pDC, rectClient.left, rectClient.top, rectClient.right, rectClient.top);
	DrawSepLine(pDC, rectClient.right - 1, rectClient.top, rectClient.right - 1, rectClient.bottom);
	DrawSepLine(pDC, rectClient.left, rectClient.bottom - 1, rectClient.right, rectClient.bottom - 1);

	pDC->SelectObject(pOldPen);
	pPen->DeleteObject();
	delete pPen;

	rectClient.left++;
	rectClient.top++;
	rectClient.right--;
	rectClient.bottom--;

	pPen = new CPen(PS_SOLID, 1, AfxGetThemeManager()->GetBEToolbarBtnHighlightColor());
	pOldPen = pDC->SelectObject(pPen);

	DrawSepLine(pDC, rectClient.right - 1, rectClient.top, rectClient.right - 1, rectClient.bottom);
	DrawSepLine(pDC, rectClient.left, rectClient.bottom - 1, rectClient.right, rectClient.bottom - 1);
	DrawSepLine(pDC, rectClient.left, rectClient.top, rectClient.left, rectClient.bottom);
	DrawSepLine(pDC, rectClient.left, rectClient.top, rectClient.right, rectClient.top);

	pDC->SelectObject(pOldPen);
	pPen->DeleteObject();
	delete pPen;
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnBeforeDrawRow(CDCDrawInfo& drawinfo)
{
	CMicroareaVisualManager* pManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());

	// se c'e' in corso una operazione di drop... evidenzio la riga...
	BOOL	bHilight = m_bTargetDragging && drawinfo.GetRowIdx() == m_IdxDragRecDest;
	// riga intera...
	CDC* pDC = drawinfo.GetDC();
	if (bHilight && (m_DragSelType == DST_CENTER))
	{
		drawinfo.m_crBkgrd = AfxGetThemeManager()->GetDisabledControlForeColor();
		drawinfo.m_crText = pDC->GetBkColor();

		drawinfo.m_crOldBkgrd = pDC->SetBkColor(drawinfo.m_crBkgrd);
		drawinfo.m_crOldText = pDC->SetTextColor(drawinfo.m_crText);
		if (pDC)
			ReleaseDC(pDC);
		return;
	}

	BOOL bEnabled = IsWindowEnabled();

	if (GetMultiSelMode() != MULTIPLE_SEL)
	{
		if (drawinfo.GetIsActiveRow() && bEnabled)
		{
			// Prepare for highlighting or un-highlighting
			drawinfo.m_crBkgrd = AfxGetThemeManager()->GetBERowSelectedBkgColor();
			drawinfo.m_crOldBkgrd = pDC->SetBkColor(drawinfo.m_crBkgrd);

			drawinfo.m_crText = pManager ? pManager->GetControlForeColor(this, TRUE, AfxGetThemeManager()->GetBERowSelectedForeColor()) : AfxGetThemeManager()->GetBERowSelectedForeColor();
			drawinfo.m_crOldText = pDC->SetTextColor(drawinfo.m_crText);
		}
		else
		{
			if ((drawinfo.GetRowIdx() % 2) && m_bAlternateColor && bEnabled)
			{
				drawinfo.m_crBkgrd = AfxGetThemeManager()->GetBERowBkgAlternateColor();
				drawinfo.m_crOldBkgrd = pDC->SetBkColor(drawinfo.m_crBkgrd);
			}
			else
				drawinfo.m_crBkgrd = pDC->GetBkColor();

			drawinfo.m_crText = pManager ? pManager->GetControlForeColor(this, FALSE, pDC->GetTextColor()) : pDC->GetTextColor();
			if (IsReadOnly() || !bEnabled)
				drawinfo.m_crOldText = pDC->SetTextColor(drawinfo.m_crText);
		}
		if (pDC)
			ReleaseDC(pDC);

		return;
	}

	// ---- GetMultiSelMode() == MULTIPLE_SEL
	if (
		(drawinfo.GetRowIdx() <= m_SelRecords.GetUpperBound()) &&
		(m_SelRecords[drawinfo.GetRowIdx()] == SELECTED)
		)
	{
		drawinfo.m_crBkgrd = m_crMultiSelBackColor;
		drawinfo.m_crText = m_crMultiSelTextColor;

		drawinfo.m_crOldBkgrd = pDC->SetBkColor(drawinfo.m_crBkgrd);
		drawinfo.m_crOldText = pDC->SetTextColor(drawinfo.m_crText);
	}
	else
	{
		drawinfo.m_crText = pManager ? pManager->GetControlForeColor(this, bEnabled, pDC->GetTextColor()) : pDC->GetTextColor();
		drawinfo.m_crOldText = pDC->SetTextColor(drawinfo.m_crText);

		if ((drawinfo.GetRowIdx() % 2) && m_bAlternateColor)
		{
			drawinfo.m_crBkgrd = m_crAlternateColor;
			drawinfo.m_crOldBkgrd = pDC->SetBkColor(drawinfo.m_crBkgrd);
		}
	}
	if (pDC)
		ReleaseDC(pDC);

	//---- ---------
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnAfterDrawRow(CDCDrawInfo& drawinfo)
{
	CDC* pDC = drawinfo.GetDC();
	// se c'e' in corso una operazione di drop... evidenzio la riga...
	if (m_bTargetDragging)
	{
		if (m_DragSelType == DST_CENTER)
		{
			pDC->SetBkColor(drawinfo.m_crOldBkgrd);
			pDC->SetTextColor(drawinfo.m_crOldText);
			ReleaseDC(pDC);

			return;
		}
	}

	BOOL bEnabled = IsWindowEnabled();

	if (GetMultiSelMode() != MULTIPLE_SEL)
	{
		// Restore the DC.
		if (drawinfo.GetIsActiveRow() && bEnabled)
		{
			pDC->SetBkColor(drawinfo.m_crOldBkgrd);
			pDC->SetTextColor(drawinfo.m_crOldText);
		}
		else if (IsReadOnly() || !bEnabled)
			pDC->SetTextColor(drawinfo.m_crOldText);

		if ((drawinfo.GetRowIdx() % 2) && m_bAlternateColor && bEnabled)
			pDC->SetBkColor(drawinfo.m_crOldBkgrd);

		ReleaseDC(pDC);

		return;
	}

	// ---- GetMultiSelMode() == MULTIPLE_SEL
	if (
		(drawinfo.GetRowIdx() <= m_SelRecords.GetUpperBound()) &&
		(m_SelRecords[drawinfo.GetRowIdx()] == SELECTED)
		)
	{
		pDC->SetBkColor(drawinfo.m_crOldBkgrd);
		pDC->SetTextColor(drawinfo.m_crOldText);
	}
	else
	{
		pDC->SetTextColor(drawinfo.m_crOldText);

		if ((drawinfo.GetRowIdx() % 2) && m_bAlternateColor)
			pDC->SetBkColor(drawinfo.m_crOldBkgrd);
	}

	ReleaseDC(pDC);

}

//-----------------------------------------------------------------------------
#define TITLE_YOFFSET 3

void CBodyEdit::DrawTitles(CDC* pDC, LPDRAWITEMSTRUCT lpDIS, COLORREF crBackgnd)
{
	if (!BEGetExStyle(BE_STYLE_SHOW_COLUMN_HEADERS))
		return;

	BOOL bWinEnabled = IsWindowEnabled();

	COLORREF crForegnd = AfxGetThemeManager()->GetButtonFaceForeColor();

	CPen		pen(PS_SOLID, 1, crForegnd);
	CPen*		pOldPen = pDC->SelectObject(&pen);

	int			nOldBkMode = pDC->SetBkMode(TRANSPARENT);
	UINT		nOldAlign = pDC->SetTextAlign(TA_CENTER);

	COLORREF	crOldBackgnd = pDC->SetBkColor(crBackgnd);
	COLORREF	crOldForegnd = pDC->SetTextColor(crForegnd);

	CPen		pShdw(PS_SOLID, 1, AfxGetThemeManager()->GetBEToolbarBtnShadowColor());
	CPen		pSep(PS_SOLID, 1, AfxGetThemeManager()->GetBESeparatorColor());
	CPen		pLght(PS_SOLID, 1, AfxGetThemeManager()->GetBEToolbarBtnHighlightColor());
	CPen		pShdwLight(PS_SOLID, 1, AfxGetThemeManager()->GetBETitlesBorderColor());
	CPen*		pTmpPen;

	TEXTMETRIC	tm;
	pDC->GetTextMetrics(&tm);

	FillRect(pDC, m_rectTitles, crBackgnd);

	int bodyWidth = m_rectBody.Width(); //rcVSB.Width() - 4;

	int nXStart = m_rectTitles.left;
	int nXEnd = m_rectTitles.left;
	int nLastCol = m_LastColsIdx.GetSize() ? min(GetEndColumnIdx(), m_ColumnsInfo.GetUpperBound()) : -1;
	int nStartCol = max(0, GetLogicalStartColumnIdx());

	// Column Lock
	// Imposto la giusta spaziatura per la colonna bloccata e per quelle ordinate
	// prima del disegno standard
	BOOL bShowExtraRSpace = FALSE;
	int  nTitleExtraRSpace = 0;
	int	 nCol = 0;

	int idxLockedColumn = m_nLockedColumnIDC ? GetVisibleColumnIdx(m_nLockedColumnIDC) : -1;
	int idxSearchedColumn = m_nSearchColumnIDC ? GetVisibleColumnIdx(m_nSearchColumnIDC) : -1;

	if (m_bAddColumnHeaderExtraSpace && bWinEnabled)
	{
		for (nCol = nStartCol; nCol <= nLastCol; nCol++)
		{
			bShowExtraRSpace = FALSE;
			nTitleExtraRSpace = 0;

			ColumnInfo* pColumnInfo = m_ColumnsInfo[nCol];

			int idxFilteredColumn = -1;
			if (GetDBT())
			{
				for (int k = 0; k < GetDBT()->m_arMemFilters.GetSize(); k++)
				{
					DBTSlaveBuffered::MemFilter* mf = (DBTSlaveBuffered::MemFilter*) GetDBT()->m_arMemFilters[k];
					ASSERT_VALID(mf);

					if (mf->m_bUI && mf->m_arColumnsDataIndex[0] == pColumnInfo->GetDataInfoIdx())
					{
						idxFilteredColumn = nCol;
						break;
					}
				}
			}

			if (BEGetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK_INTERACTIVE))
			{
				// disegno la bitmap del locking
				if (nCol == idxLockedColumn || nCol == m_nMouseOverTitle)
				{
					bShowExtraRSpace = TRUE;
					nTitleExtraRSpace += LOCK_EXTRARSPACE;
				}
			}

			if (BEGetExStyle(BE_STYLE_ALLOW_FILTER))
			{
				// disegno la bitmap del locking
				if (nCol == idxFilteredColumn || nCol == m_nMouseOverTitle)
				{
					bShowExtraRSpace = TRUE;
					nTitleExtraRSpace += LOCK_EXTRARSPACE;
				}
			}

			if (BEGetExStyle(BE_STYLE_ALLOW_SORT))		//Sort
			{
				if
					(
						pColumnInfo->GetColumnOrdering() == ColumnInfo::ASC ||
						pColumnInfo->GetColumnOrdering() == ColumnInfo::DESC ||
						pColumnInfo->GetColumnOrdering() == ColumnInfo::ASCFIXED ||
						pColumnInfo->GetColumnOrdering() == ColumnInfo::DESCFIXED
						)
				{
					bShowExtraRSpace = TRUE;
					nTitleExtraRSpace += SORT_EXTRARSPACE;
				}
			}

			pColumnInfo->SetShowExtraRSpace(bShowExtraRSpace);
			pColumnInfo->SetTitleExtraRSpace(nTitleExtraRSpace);
		}
	}

	for (nCol = nStartCol; nCol <= nLastCol; nCol++)
	{
		ColumnInfo* pColumnInfo = m_ColumnsInfo[nCol];

		int idxFilteredColumn = -1;
		if (GetDBT())
		{
			for (int k = 0; k < GetDBT()->m_arMemFilters.GetSize(); k++)
			{
				DBTSlaveBuffered::MemFilter* mf = (DBTSlaveBuffered::MemFilter*) GetDBT()->m_arMemFilters[k];
				ASSERT_VALID(mf);

				if (mf->m_bUI && mf->m_arColumnsDataIndex[0] == pColumnInfo->GetDataInfoIdx())
				{
					idxFilteredColumn = nCol;
					break;
				}
			}
		}
		if (pColumnInfo->GetScreenWidth() == 0)	// Column Lock
			continue;

		// calculates the new right coordinate
		nXEnd += pColumnInfo->GetScreenWidth();

		if (pColumnInfo->GetScreenWidth() == 1)	// resize minimale
		{
			nXStart = nXEnd;
			continue;
		}

		if (XCoordInRect(nXStart, nXEnd, lpDIS->rcItem))
		{
			int nWidhtIDB = 0;
			if (pColumnInfo->GetTitleIDB())
			{
				if (m_pImageList)
				{
					IMAGEINFO	ii;
					m_pImageList->GetImageInfo(pColumnInfo->GetTitleIDB(), &ii);
					nWidhtIDB = abs(ii.rcImage.right - ii.rcImage.left) + 1;
				}
				else if (m_pImages)
				{
					//TODO IMAGELIST
				}
				else
				{
					nWidhtIDB = pColumnInfo->m_bmpTitle.GetBitmapDimension().cx;
				}
				ASSERT(nWidhtIDB);
			}

			CRect rectDraw
			(
				nXStart + DEFAULT_COLUMN_OFFSET,
				m_rectTitles.top + 1,
				nXEnd - DEFAULT_COLUMN_OFFSET,
				m_rectTitles.top + m_rectTitles.Height()
			);

			int	xTitle = ((nXStart + nXEnd) / 2);	//centrato...
			if (BEGetExStyle(BE_STYLE_ALIGN_TITLES))
			{
				switch (pColumnInfo->m_nAlignFlags)
				{
				case TA_CENTER:
					xTitle += nWidhtIDB / 2;
					break;

				case TA_LEFT:
					xTitle = nXStart + DEFAULT_COLUMN_OFFSET + nWidhtIDB;
					break;

				case TA_RIGHT:
					xTitle = nXEnd - DEFAULT_COLUMN_OFFSET;
					if (pColumnInfo->GetShowExtraRSpace())
						xTitle -= pColumnInfo->GetTitleExtraRSpace();

					break;
				};
				pDC->SetTextAlign(pColumnInfo->m_nAlignFlags);
			}

			//hyperlink su HKL e custom column header
			COLORREF		crOldText = 0;
			COLORREF		crOldBkg = 0;
			CFont*			pOldTitleFont = NULL;
			BOOL			bHyperLink = FALSE;
			CFont			fontHL;

			if (bWinEnabled)
			{
				bHyperLink = pColumnInfo->HasHyperLink();
				if (bHyperLink)
				{
					if (pColumnInfo->UseCustomTextColor())
						crOldText = pDC->SetTextColor(pColumnInfo->GetTextColor());
					else
						crOldText = pDC->SetTextColor(AfxGetThemeManager()->GetHyperLinkForeColor());

					CFont* pF = NULL;

					if (pColumnInfo->GetControl())
					{
						pF = AfxGetThemeManager()->GetFormFont(m_bUseColumnHeaderSmallFont); //TODO font pColumnInfo->GetControl()->GetFont();
						if (pF)
						{
							LOGFONT lf;	memset(&lf, 0, sizeof(lf));
							pF->GetLogFont(&lf);
							lf.lfUnderline = TRUE;

							fontHL.CreateFontIndirect(&lf);

							pF = &fontHL;
						}
					}

					if (!pF)
						pF = AfxGetThemeManager()->GetHyperlinkFont(m_bUseColumnHeaderSmallFont);

					pOldTitleFont = pDC->SelectObject(pF);
				}
				else
				{
					if (pColumnInfo->UseCustomTextColor())
						crOldText = pDC->SetTextColor(pColumnInfo->GetTextColor());

					pOldTitleFont = pDC->SelectObject(AfxGetThemeManager()->GetFormFont(m_bUseColumnHeaderSmallFont));
				}

				if (pColumnInfo->UseCustomBkgColor())
				{
					crOldBkg = pDC->SetBkColor(pColumnInfo->GetBkgColor());

					CRect rcFiller(rectDraw);
					rcFiller.left -= DEFAULT_COLUMN_OFFSET;
					rcFiller.right += DEFAULT_COLUMN_OFFSET;
					FillRect(pDC, rcFiller, pColumnInfo->GetBkgColor());
				}
			}
			else
			{
				crOldText = pDC->SetTextColor(AfxGetThemeManager()->GetBEDisabledTitleForeColor());

				pOldTitleFont = pDC->SelectObject(AfxGetThemeManager()->GetControlFont(m_bUseColumnHeaderSmallFont));
			}

			int yTitle = m_rectTitles.top + TITLE_YOFFSET;

			if (pColumnInfo->m_strTitle.Find(LF_CHAR) >= 0)
			{
				CString strT = pColumnInfo->m_strTitle;
				TCHAR * nextToken;
				TCHAR * pT = strT.GetBuffer(strT.GetLength());
				TCHAR * pRow = _tcstok_s(pT, _T("\n"), &nextToken);
				while (pRow)
				{
					if (BEGetExStyle(BE_STYLE_UPPERCASE_COLUMN_TITLES))
					{
						CString s(pRow);
						DataStr  upperTitle(s);
						upperTitle.SetCollateCultureSensitive();
						upperTitle.MakeUpper();

						pDC->ExtTextOut
						(
							xTitle,
							yTitle,
							(yTitle == TITLE_YOFFSET ? ETO_OPAQUE : 0) | ETO_CLIPPED,
							&rectDraw,
							upperTitle.GetString(),
							_tcslen(pRow),
							NULL
						);

					}
					else
						pDC->ExtTextOut
						(
							xTitle,
							yTitle,
							(yTitle == TITLE_YOFFSET ? ETO_OPAQUE : 0) | ETO_CLIPPED,
							&rectDraw,
							pRow,
							_tcslen(pRow),
							NULL
						);

					if (IsScale())
						yTitle += tm.tmHeight + ScalePix(5);
					else
						yTitle += tm.tmHeight;
					
					pRow = _tcstok_s(NULL, _T("\n"), &nextToken);
				};
				strT.ReleaseBuffer();
			}
			else
			{
				if (BEGetExStyle(BE_STYLE_UPPERCASE_COLUMN_TITLES))
				{
					DataStr  upperTitle(pColumnInfo->m_strTitle);
					upperTitle.SetCollateCultureSensitive();
					upperTitle.MakeUpper();

					pDC->ExtTextOut(
						xTitle,
						yTitle,
						ETO_OPAQUE | ETO_CLIPPED,
						&rectDraw,
						upperTitle.GetString(),
						pColumnInfo->m_strTitle.GetLength(),
						NULL
					);
				}
				else
					pDC->ExtTextOut(
						xTitle,
						yTitle,
						ETO_OPAQUE | ETO_CLIPPED,
						&rectDraw,
						pColumnInfo->m_strTitle,
						pColumnInfo->m_strTitle.GetLength(),
						NULL
					);
			}

			if (nWidhtIDB)
			{
				int nX = xTitle;
				if (BEGetExStyle(BE_STYLE_ALIGN_TITLES) && pColumnInfo->m_nAlignFlags == TA_RIGHT)
					nX -= (GetTextSize(pDC, pColumnInfo->m_strTitle).cx + nWidhtIDB);
				else if (BEGetExStyle(BE_STYLE_ALIGN_TITLES) && pColumnInfo->m_nAlignFlags == TA_CENTER)
					nX -= (GetTextSize(pDC, pColumnInfo->m_strTitle).cx / 2 + nWidhtIDB);
				else if (pColumnInfo->m_nAlignFlags == TA_LEFT)
					nX -= nWidhtIDB;

				int nY = yTitle;//+  abs(ii.rcImage.top - ii.rcImage.bottom) / 2;

				if (m_pImageList)
				{
					m_pImageList->Draw(pDC, pColumnInfo->GetTitleIDB(), CPoint(nX, nY), ILD_TRANSPARENT);
				}
				else if (m_pImages)
				{
					//TODO IMAGELIST
				}
				else
				{
					CDC memDC;
					memDC.CreateCompatibleDC(pDC);
					CBitmap* pOld = memDC.SelectObject(&pColumnInfo->m_bmpTitle);

					CSize sz = pColumnInfo->m_bmpTitle.GetBitmapDimension();

					pDC->StretchBlt
					(
						nX, nY,
						sz.cx, sz.cy,
						&memDC,
						0, 0,
						sz.cx, sz.cy,
						SRCCOPY
					);
					memDC.SelectObject(pOld);
				}
			}

			//rispristina colore e font cambiati per hyperlink su HKL o per custom column header
			if (bHyperLink)
			{
				pDC->SetTextColor(crOldText);
				pDC->SelectObject(pOldTitleFont);
				fontHL.DeleteObject();
			}
			else
			{
				if (!bWinEnabled || pColumnInfo->UseCustomTextColor())
				{
					pDC->SetTextColor(crOldText);
				}

				if (m_bUseColumnHeaderSmallFont)
				{
					pDC->SelectObject(pOldTitleFont);
				}
			}

			if (pColumnInfo->UseCustomBkgColor())
			{
				pDC->SetBkColor(crOldBkg);
			}
			//----

			pTmpPen = pDC->SelectObject(&pSep);
			DrawSepLine(pDC, nXEnd, m_rectTitles.top + 5, nXEnd, m_rectTitles.bottom - 5);

			pDC->SelectObject(&pLght);
			DrawSepLine(pDC, nXEnd + 1, m_rectTitles.top + 5, nXEnd + 1, m_rectTitles.bottom - 5);

			pDC->SelectObject(pTmpPen);
		}

		//Sort
		if
			(
				bWinEnabled && BEGetExStyle(BE_STYLE_ALLOW_SORT) &&
				(
					pColumnInfo->GetColumnOrdering() == ColumnInfo::ASC ||
					pColumnInfo->GetColumnOrdering() == ColumnInfo::DESC ||
					pColumnInfo->GetColumnOrdering() == ColumnInfo::ASCFIXED ||
					pColumnInfo->GetColumnOrdering() == ColumnInfo::DESCFIXED
					)
				)
		{
			int	x = nXEnd - pColumnInfo->GetTitleExtraRSpace() + PNG_SIZE;
			int	y = m_rectTitles.top + ((m_rectTitles.Height() - PNG_SIZE) / 2) + 2;

			if
				(
					pColumnInfo->GetColumnOrdering() == ColumnInfo::ASC ||
					pColumnInfo->GetColumnOrdering() == ColumnInfo::ASCFIXED
					)
				DrawTransparentBitmap(pDC, m_bmpSortAsc, CPoint(x, y), PNG_SIZE);
			else
				DrawTransparentBitmap(pDC, m_bmpSortDesc, CPoint(x, y), PNG_SIZE);
		}

		//Hide
		if
			(
				bWinEnabled &&
				m_nMouseOverTitle == nCol &&
				nCol != idxLockedColumn &&
				CanHideColumn(pColumnInfo)
				)
		{
			int	x = nXStart + 2 + PNG_SIZE;
			int	y = m_rectTitles.top + ((m_rectTitles.Height() - PNG_SIZE) / 2);

			DrawTransparentBitmap(pDC, m_bmpClose, CPoint(x, y), PNG_SIZE);
		}

		//Column Lock
		if (
			bWinEnabled &&
			BEGetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK_INTERACTIVE) &&
			(pColumnInfo->m_nHorzColOffset + pColumnInfo->GetScreenWidth()) < bodyWidth &&
			nCol != m_ColumnsInfo.GetUpperBound()
			)
		{
			// Disegno il lucchetto
			int x = nXEnd - LOCK_EXTRARSPACE + PNG_SIZE;
			int y = m_rectTitles.top + ((m_rectTitles.Height() - PNG_SIZE) / 2) + 1;

			if (m_nMouseOverTitle == nCol)
			{
				if (nCol == idxLockedColumn)
					DrawTransparentBitmap(pDC, m_bmpLock, CPoint(x, y), PNG_SIZE);
				else
					DrawTransparentBitmap(pDC, m_bmpUnlock, CPoint(x, y), PNG_SIZE);
			}
			else
			{
				if (nCol == idxLockedColumn)
					DrawTransparentBitmap(pDC, m_bmpLocked, CPoint(x, y), PNG_SIZE);
			}
		}

		if (
			bWinEnabled &&
			BEGetExStyle(BE_STYLE_ALLOW_FILTER) &&
			(pColumnInfo->m_nHorzColOffset + pColumnInfo->GetScreenWidth()) < bodyWidth &&
			nCol == idxFilteredColumn
			)
		{
			// Disegno l'imbuto
			int x = nXStart + LOCK_EXTRARSPACE + 1;

			if (m_nMouseOverTitle == nCol && CanHideColumn(pColumnInfo))
				x += LOCK_EXTRARSPACE + 1;

			int y = m_rectTitles.top + ((m_rectTitles.Height() - PNG_SIZE) / 2) + 1;

			DrawTransparentBitmap(pDC, m_bmpFiltered, CPoint(x, y), PNG_SIZE);

			if (nCol == idxSearchedColumn)
			{
				x += LOCK_EXTRARSPACE + 1;

				DrawTransparentBitmap(pDC, m_bmpSearched, CPoint(x, y), PNG_SIZE);
			}
		}
		else if (
			bWinEnabled &&
			(pColumnInfo->m_nHorzColOffset + pColumnInfo->GetScreenWidth()) < bodyWidth &&
			nCol == idxSearchedColumn
			)
		{
			int x = nXStart + LOCK_EXTRARSPACE + 1;

			if (m_nMouseOverTitle == nCol && CanHideColumn(pColumnInfo))
				x += LOCK_EXTRARSPACE + 1;

			int y = m_rectTitles.top + ((m_rectTitles.Height() - PNG_SIZE) / 2) + 1;

			DrawTransparentBitmap(pDC, m_bmpSearched, CPoint(x, y), PNG_SIZE);
		}

		//---------
		nXStart = nXEnd;
	}

	pDC->SelectObject(&pSep);
	DrawSepLine(pDC, m_rectTitles.left, m_rectTitles.bottom - 2, nXEnd /*m_rectTitles.right*/, m_rectTitles.bottom - 2);
	m_nLastColumRight = nXEnd;

	// restores the old values for graphic device
	pDC->SetTextAlign(nOldAlign);
	pDC->SetTextColor(crOldForegnd);
	pDC->SetBkColor(crOldBackgnd);
	pDC->SetBkMode(nOldBkMode);
	pDC->SelectObject(pOldPen);
	pen.DeleteObject();
}

//-----------------------------------------------------------------------------
void CBodyEdit::DrawRow(CDC* pDC, LPDRAWITEMSTRUCT lpDIS, int nRow, int nYStart, BOOL bActiveRow)
{
	if (!m_pDBT)
		return;

	if (nRow < m_nStartRecordIdx || nRow > m_pDBT->GetUpperBound())
	{
		ASSERT(FALSE);
		return;
	}

	CDCDrawInfo		drawinfo(pDC, nRow, bActiveRow, nYStart);

	OnBeforeDrawRow(drawinfo);

	if (drawinfo.m_pFont)
	{
		drawinfo.m_pOldFont = pDC->SelectObject(drawinfo.m_pFont);
	}

	// paint the background (behind the text)
	if (m_nLastColumRight > 0 && m_nLastColumRight < m_rectBody.right)
	{
		CRect rectSelection(m_rectBody.left, nYStart, m_nLastColumRight, nYStart + m_nRowsHeight);
		FillRect(pDC, rectSelection, drawinfo.m_crBkgrd);

		CRect rectSelection2(m_nLastColumRight + 1, nYStart, m_rectBody.right, nYStart + m_nRowsHeight);
		FillRect(pDC, rectSelection2, AfxGetThemeManager()->GetBackgroundColor());
	}
	else
	{
		CRect rectSelection(m_rectBody.left, nYStart, m_nLastColumRight > 0 ? m_nLastColumRight : m_rectBody.right, nYStart + m_nRowsHeight);
		FillRect(pDC, rectSelection, drawinfo.m_crBkgrd);
	}

	SqlRecord* pRec = m_pDBT->GetRow(nRow);

	// Get the data for the specific check.
	int nXTxtStart = 0;
	int nXNextStart = m_rectBody.left;
	int nXStart = 0;
	int nXEnd = m_rectBody.left;
	int nYEnd = nYStart + m_nRowsHeight;

	if (m_LastColsIdx.GetSize())
	{
		for (int nCol = GetLogicalStartColumnIdx(); nCol <= GetEndColumnIdx(); nCol++)
		{
			ColumnInfo* pOriginalColumnInfo = m_ColumnsInfo[nCol];

			if (pOriginalColumnInfo->GetScreenWidth() == 0)	//column lock
				continue;

			// calculates the new right coordinate
			nXStart = nXNextStart;
			nXEnd += pOriginalColumnInfo->GetScreenWidth();
			nXNextStart = nXEnd;

			CRect rectDraw(nXStart, nYStart, nXEnd, nYEnd);

			ColumnInfo* pStoledColInfo = NULL;
			ColumnInfo* pToBeUsedColInfo = NULL;
			if (pOriginalColumnInfo->m_bUseOtherColumnCtrl)
			{
				pStoledColInfo = GetAlternativeColumnForEditing(pOriginalColumnInfo, pRec);
				if (pStoledColInfo)
					pToBeUsedColInfo = pStoledColInfo;
				else
					pToBeUsedColInfo = pOriginalColumnInfo;
			}
			else
			{
				pToBeUsedColInfo = pOriginalColumnInfo;
			}

			DataObj* pDataObj = GetDataObjAt(pRec, pToBeUsedColInfo);
			ASSERT(pDataObj);
			//-- gestione custom color
			//@@TODO: manca esclusione riga selezionata
			COLORREF crOldTextColor, crOldBkgColor;
			CBitmap bmpIcon;
			BITMAP bmIconInfo;

			CBodyEditRowSelected CurRow;
			CurRow.m_pCol = pToBeUsedColInfo;
			CurRow.m_pRec = pRec;
			CurRow.m_pCellData = pDataObj;
			CurRow.m_nColumnIdx = nCol;
			CurRow.m_nRowDbt = nRow;
			CurRow.m_nColumnIDC = pToBeUsedColInfo->GetParsedCtrl()->GetCtrlID();
			CurRow.m_crBkgColor = pDC->GetBkColor();
			CurRow.m_crTextColor = pDC->GetTextColor();
			CurRow.m_nIconIDB = 0;
			CurRow.m_pFont = drawinfo.m_pFont;

			// Se alla colonna e` stato connesso un CParsedBitmap viene gestita il draw del bitmap
			// relativo al dato della corrente cella

			CWnd* pControl = pToBeUsedColInfo->GetControl();
			ASSERT_VALID(pControl);

			BOOL bUseCustomColor = GetCustomColor(nCol, nRow, &CurRow);
			if (bUseCustomColor)
			{
				CRect rcFiller(nXStart, nYStart, nXEnd, nYEnd);
				FillRect(pDC, rcFiller, CurRow.m_crBkgColor);
			}

			if (pControl->IsKindOf(RUNTIME_CLASS(CParsedBitmap)))
			{
				pToBeUsedColInfo->GetParsedCtrl()->SetValue(*pDataObj);
				if (!pDataObj->IsHide())
				{
					// DPI scale Icons (STRETCH)
					CParsedBitmap* pParsedBmp = ((CParsedBitmap*)pControl);
					if (IsScale())
					{
						pParsedBmp->SetCtrlStyle(BMP_STYLE_STRETCH);
						CSize sImage = pParsedBmp->GetImageSize();
						ScaleSize(sImage, *pDC);

						int nLeft = rectDraw.Width() <= 0 ? 0 : (rectDraw.Width() - sImage.cx) / 2;
						LONG w = sImage.cx > rectDraw.Width() ? rectDraw.Width() : sImage.cx;
						LONG h = sImage.cy > rectDraw.Height() ? rectDraw.Height() : sImage.cy;

						rectDraw.left = rectDraw.left + nLeft;
						rectDraw.bottom = rectDraw.top + h;
						rectDraw.right  = rectDraw.left + w;
					}

					pParsedBmp->DrawBitmap(*pDC, rectDraw);
				}
				continue;
			}

			if (pToBeUsedColInfo->m_pCheckBitmap)
			{
				// Se alla colonna e` stato connesso un CBoolButton e se il dato e` TRUE
				// viene gestito il draw del bitmap relativo al dato della corrente cella
				if (!pDataObj->IsHide())
				{
					pToBeUsedColInfo->m_pCheckBitmap->FloodDrawBitmap
					(
						pDC->m_hDC,
						(nXStart + nXEnd - ScalePix(pToBeUsedColInfo->m_pCheckBitmap->GetWidth())) / 2,
						(nYStart + nYEnd - ScalePix(pToBeUsedColInfo->m_pCheckBitmap->GetHeight())) / 2,
						SRCCOPY,
						bUseCustomColor ? CurRow.m_crTextColor : drawinfo.m_crText,
						bUseCustomColor ? CurRow.m_crBkgColor : drawinfo.m_crBkgrd,
						(BOOL)*((DataBool*)pDataObj), TRUE
					);
				}
				continue;
			}

			BOOL bDrawCurrentCell = !bActiveRow || nCol != m_nCurrColumnIdx ||
				!IsCtrlVisible() || OnDrawCurrentCell(nRow, nCol);
			if (
				XCoordInRect(nXStart, nXEnd, lpDIS->rcItem) &&
				bDrawCurrentCell
				)
			{
				// Viene formattato il dato
				if (pToBeUsedColInfo->GetParsedCtrl()->ForceUpdateCtrlView(nRow))
					pToBeUsedColInfo->GetParsedCtrl()->OnUpdateCtrlStatus(nRow);

				// determina la posizione sulla base del TextAlign
				int nRectOffs = 0;
				switch (pToBeUsedColInfo->m_nAlignFlags)
				{
				case TA_CENTER:
					nXTxtStart = (nXStart + nXEnd) / 2;
					nRectOffs = DEFAULT_COLUMN_OFFSET;
					break;

				case TA_LEFT:
					nXTxtStart = nXStart + DEFAULT_COLUMN_OFFSET;
					break;

				case TA_RIGHT:
					nXTxtStart = nXEnd - DEFAULT_COLUMN_OFFSET;
					break;

				default:
					TRACE1("CBodyEdit::DrawRow: the column %s has an illegal TextAlign\n", (LPCTSTR)pToBeUsedColInfo->m_strTitle);
					ASSERT(FALSE);

					nXTxtStart = nXStart;
					break;
				}

				//Cella Owner Draw: priorità al control ed infine al B.E. stesso
				BOOL bDoOwnerDrawCell =
					(
						pToBeUsedColInfo->GetParsedCtrl()->OwnerDraw(pDC, rectDraw, pDataObj)
						||
						OnDrawCell(&CurRow, pDC, rectDraw)
						);
				if (bDoOwnerDrawCell)
					continue;

				if (
					!bUseCustomColor &&
					pControl->IsKindOf(RUNTIME_CLASS(CColorEdit)) &&
					pDataObj->IsKindOf(RUNTIME_CLASS(DataLng))
					)
				{
					bUseCustomColor = TRUE;
					CurRow.m_crBkgColor = (COLORREF)(long)*(DataLng*)pDataObj;
					CurRow.m_crTextColor = CurRow.m_crBkgColor;
				}

				if (
					!bUseCustomColor &&
					m_nFoundRow == nRow &&
					(m_nSearchColumnIDC == CurRow.m_nColumnIDC || m_nFoundColumn == nCol)
					)
				{
					bUseCustomColor = TRUE;
					CurRow.m_crBkgColor = RGB(255, 128, 0);
				}

				int nXOffset = 0;
				CString strCell = OnFormatData(&CurRow, nXOffset);

				if (pDataObj->IsHide())
					strCell.Empty();
				else if (
					pToBeUsedColInfo->GetParsedCtrl()->GetCtrlCWnd() &&
					pToBeUsedColInfo->GetParsedCtrl()->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CEdit)) &&
					pToBeUsedColInfo->GetParsedCtrl()->GetCtrlCWnd()->GetStyle() & ES_PASSWORD
					)
					strCell = L"**********";

				// imposta il TextAlign dato dalla AddColumn o quello forzato dal programmatore
				pDC->SetTextAlign(pToBeUsedColInfo->m_nAlignFlags);

				if (
					pDataObj->GetDataType().m_wType == DATA_STR_TYPE ||
					pDataObj->GetDataType().m_wType == DATA_TXT_TYPE
					)
				{
					// Se la stringa contiene piu` righe (separate da CR-LF o LF) sfrutta le
					// righe a disposizione e poi visualizza 3 punti di sospensione nel caso
					// di overflow
					//
					int nRows = m_nLinesPerRow > 0 ? m_nLinesPerRow - 1 : 0;
					int nLen = strCell.GetLength();
					for (int i = 0; i < nLen; i++)
						if (strCell[i] == LF_CHAR || strCell[i] == CR_CHAR)
						{
							if (nRows == 0)
							{
								if (i == (nLen - 1) || (i == (nLen - 2) && strCell[i + 1] == LF_CHAR))
								{
									strCell.SetAt(i, '\0');
									break;
								}

								// per appendere "..." ----------------------v
								TCHAR* pszStr = strCell.GetBufferSetLength(i + 3);
								TB_TCSCPY(&pszStr[i], _T("..."));
								strCell.ReleaseBuffer();
								break;
							}
							else
								if (strCell[i] == LF_CHAR)
									nRows--;
						}
				}

				if (bUseCustomColor)
				{
					BOOL bEraseBk = CurRow.m_crBkgColor != pDC->GetBkColor();

					crOldBkgColor = pDC->SetBkColor(CurRow.m_crBkgColor);
					crOldTextColor = pDC->SetTextColor(CurRow.m_crTextColor);

					if (bEraseBk)
					{
						if (CurRow.m_dBkgPercFill < 1.0 && CurRow.m_dBkgPercFill >= 0.0)
						{
							int nXRight = int(nXStart + (nXEnd - nXStart) * CurRow.m_dBkgPercFill);

							CRect rcFiller2(nXStart, nYStart, nXRight, nYEnd);
							FillRect(pDC, rcFiller2, CurRow.m_crBkgColorFill);
						}
					}

					if (CurRow.m_nIconIDB)
					{
						//TODO IMAGELIST
						if
							(
								!bmpIcon.LoadBitmap(CurRow.m_nIconIDB)
								||
								!(bmpIcon.GetObject(sizeof(bmIconInfo), &bmIconInfo) == sizeof(bmIconInfo))
								)
						{
							CurRow.m_nIconIDB = 0;
							ASSERT(FALSE);
						}
						else if (pToBeUsedColInfo->m_nAlignFlags == TA_LEFT)
							nXTxtStart += bmIconInfo.bmWidth;
					}
				}

				CFont* pOldFont = pDC->SelectObject
				(
					CurRow.m_pFont
					?
					CurRow.m_pFont
					:
					pToBeUsedColInfo->GetParsedCtrl()->GetPreferredFont()
				);

				if (
					(pDataObj->GetDataType().m_wType == DATA_STR_TYPE || pDataObj->GetDataType().m_wType == DATA_TXT_TYPE) &&
					m_nLinesPerRow > 1 && !IsColumnSingleLine(pToBeUsedColInfo)
					)
				{
					rectDraw = CRect(nXTxtStart + 1, nYStart + 3, nXEnd /*- DEFAULT_COLUMN_OFFSET*/, nYEnd);

					OnDrawText(pDC, rectDraw, strCell, pRec, pToBeUsedColInfo);
				}
				else
				{
					rectDraw = CRect(nXStart + nRectOffs + (pToBeUsedColInfo->m_nAlignFlags == TA_LEFT ? DEFAULT_COLUMN_OFFSET : 1), nYStart, nXEnd /*- DEFAULT_COLUMN_OFFSET*/, nYEnd);

					OnExtTextOut(nXTxtStart + nXOffset, nYStart + 3, pDC, rectDraw, strCell, pRec, pToBeUsedColInfo); //reimplementata da CTreeBodyEdit
				}
				//-------------------------------

				if (bUseCustomColor)
				{
					if (CurRow.m_nIconIDB)
					{
						CDC memDC;
						memDC.CreateCompatibleDC(pDC);
						CBitmap* pOld = memDC.SelectObject(&bmpIcon);

						// draw the icon contents
						pDC->StretchBlt
						(
							nXStart, nYStart,
							bmIconInfo.bmWidth, min(nYEnd - nYStart, bmIconInfo.bmHeight),
							&memDC,
							0, 0,
							bmIconInfo.bmWidth, bmIconInfo.bmHeight,
							SRCCOPY
						);
						memDC.SelectObject(pOld);
					}
					pDC->SetBkColor(crOldBkgColor);
					pDC->SetTextColor(crOldTextColor);
				}
				pDC->SelectObject(pOldFont);
			}
		}
	}

	OnAfterDrawRow(drawinfo);
	if (drawinfo.m_pOldFont)
	{
		pDC->SelectObject(drawinfo.m_pOldFont);
		drawinfo.m_pOldFont = NULL;
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::DrawFooter(CDC* pDC, LPDRAWITEMSTRUCT lpDIS, int nYStart)
{
	if (!m_pDBT)
		return;

	int nRow = -1;
	BOOL bActiveRow = FALSE;
	CMicroareaVisualManager* pManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());

	CDCDrawInfo		drawinfo(pDC, nRow, bActiveRow, nYStart);
	LOGFONT lf;
	AfxGetThemeManager()->GetControlFont()->GetLogFont(&lf);
	lf.lfWeight *= 2;
	CFont font;
	font.CreateFontIndirect(&lf);

	drawinfo.m_pFont = &font;
	drawinfo.m_pOldFont = pDC->SelectObject(drawinfo.m_pFont);
	drawinfo.m_crOldBkgrd = pDC->SetBkColor(AfxGetThemeManager()->GetBackgroundColor());

	// paint the background (behind the text)
	CRect rectSelection(m_rectBody.left, nYStart, m_rectBody.right, nYStart + m_nRowsHeight);
	FillRect(pDC, rectSelection, AfxGetThemeManager()->GetBackgroundColor());

	SqlRecord* pRec = GetDBT()->GetRecord();	//Il record del footer e' questo

	// Get the data for the specific check.
	int nXTxtStart = 0;
	int nXNextStart = m_rectBody.left;
	int nXStart = 0;
	int nXEnd = m_rectBody.left;
	int nYEnd = nYStart + m_nRowsHeight;

	if (m_LastColsIdx.GetSize())
		for (int nCol = GetLogicalStartColumnIdx(); nCol <= GetEndColumnIdx(); nCol++)
		{
			ColumnInfo* pColumnInfo = m_ColumnsInfo[nCol];

			if (pColumnInfo->GetScreenWidth() == 0)	//column lock
				continue;

			// calculates the new right coordinate
			nXStart = nXNextStart;
			nXEnd += pColumnInfo->GetScreenWidth();
			nXNextStart = nXEnd;

			CRect rectDraw(nXStart, nYStart, nXEnd, nYEnd);

			DataObj* pDataObj = pColumnInfo->GetFooterDataObj();
			if (!pDataObj)
				continue;

			//-- gestione custom color
			//@@TODO: manca esclusione riga selezionata
			COLORREF crOldTextColor, crOldBkgColor;
			CBitmap bmpIcon;
			BITMAP bmIconInfo;

			CBodyEditRowSelected CurRow;
			CurRow.m_pCol = pColumnInfo;
			CurRow.m_pRec = pRec;
			CurRow.m_pCellData = pDataObj;
			CurRow.m_nColumnIdx = nCol;
			CurRow.m_nRowDbt = nRow;
			CurRow.m_nColumnIDC = pColumnInfo->GetParsedCtrl()->GetCtrlID();
			CurRow.m_crBkgColor = pDC->GetBkColor();
			CurRow.m_crTextColor = pManager && pManager->IsInADataEntryInBrowse(pColumnInfo->GetParsedCtrl()->GetCtrlCWnd()) ? globalData.clrGrayedText : pDC->GetTextColor();

			CurRow.m_nIconIDB = 0;
			CurRow.m_bFooter = TRUE;

			CWnd* pControl = pColumnInfo->GetControl();
			ASSERT_VALID(pControl);

			//priorità ai ClientDoc, poi al Doc ed infine al B.E. stesso
			BOOL bUseCustomColor = GetCustomColor(nCol, nRow, &CurRow);
			if (bUseCustomColor)
			{
				CRect rcFiller(nXStart, nYStart, nXEnd, nYEnd);
				FillRect(pDC, rcFiller, CurRow.m_crBkgColor);
			}

			// Se alla colonna e` stato connesso un CParsedBitmap viene gestita il draw del bitmap
			// relativo al dato della corrente cella
			if (pControl->IsKindOf(RUNTIME_CLASS(CParsedBitmap)))
			{
				pColumnInfo->GetParsedCtrl()->SetValue(*pDataObj);

				if (!pDataObj->IsHide())
					((CParsedBitmap*)pControl)->DrawBitmap(*pDC, rectDraw);

				continue;
			}

			if (pColumnInfo->m_pCheckBitmap)
			{
				// Se alla colonna e` stato connesso un CBoolButton e se il dato e` TRUE
				// viene gestito il draw del bitmap relativo al dato della corrente cella
				if (!pDataObj->IsHide())
				{
					pColumnInfo->m_pCheckBitmap->FloodDrawBitmap
					(
						pDC->m_hDC,
						(nXStart + nXEnd - pColumnInfo->m_pCheckBitmap->GetWidth()) / 2,
						(nYStart + nYEnd - pColumnInfo->m_pCheckBitmap->GetHeight()) / 2,
						SRCCOPY,
						bUseCustomColor ? CurRow.m_crTextColor : drawinfo.m_crText,
						bUseCustomColor ? CurRow.m_crBkgColor : drawinfo.m_crBkgrd,
						(BOOL)*((DataBool*)pDataObj), TRUE
					);
				}
				continue;
			}

			BOOL bDrawCurrentCell = !bActiveRow || nCol != m_nCurrColumnIdx ||
				!IsCtrlVisible() || OnDrawCurrentCell(nRow, nCol);
			if (
				XCoordInRect(nXStart, nXEnd, lpDIS->rcItem) &&
				bDrawCurrentCell
				)
			{
				// Viene formattato il dato
				if (pColumnInfo->GetParsedCtrl()->ForceUpdateCtrlView(nRow))
					pColumnInfo->GetParsedCtrl()->OnUpdateCtrlStatus(nRow);

				// determina la posizione sulla base del TextAlign
				int nRectOffs = 0;
				switch (pColumnInfo->m_nAlignFlags)
				{
				case TA_CENTER:
					nXTxtStart = (nXStart + nXEnd) / 2;
					nRectOffs = DEFAULT_COLUMN_OFFSET;
					break;

				case TA_LEFT:
					nXTxtStart = nXStart + DEFAULT_COLUMN_OFFSET;
					break;

				case TA_RIGHT:
					nXTxtStart = nXEnd - DEFAULT_COLUMN_OFFSET;
					break;

				default:
					TRACE1("CBodyEdit::DrawRow: the column %s has an illegal TextAlign\n", (LPCTSTR)pColumnInfo->m_strTitle);
					ASSERT(FALSE);

					nXTxtStart = nXStart;
					break;
				}

				crOldTextColor = pDC->SetTextColor(CurRow.m_crTextColor);
				//Cella Owner Draw: priorità al control ed infine al B.E. stesso
				BOOL bDoOwnerDrawCell =
					(
						pColumnInfo->GetParsedCtrl()->OwnerDraw(pDC, rectDraw, pDataObj)
						||
						OnDrawCell(&CurRow, pDC, rectDraw)
						);
				if (bDoOwnerDrawCell)
					continue;

				if (
					!bUseCustomColor &&
					pControl->IsKindOf(RUNTIME_CLASS(CColorEdit)) &&
					pDataObj->IsKindOf(RUNTIME_CLASS(DataLng))
					)
				{
					bUseCustomColor = TRUE;
					CurRow.m_crBkgColor = (COLORREF)(long)*(DataLng*)pDataObj;
					CurRow.m_crTextColor = CurRow.m_crBkgColor;
				}

				int nXOffset = 0;
				CString strCell = OnFormatData(&CurRow, nXOffset);
				if (pDataObj->IsHide())
					strCell.Empty();

				// imposta il TextAlign dato dalla AddColumn o quello forzato dal programmatore
				pDC->SetTextAlign(pColumnInfo->m_nAlignFlags);

				if (
					pDataObj->GetDataType().m_wType == DATA_STR_TYPE ||
					pDataObj->GetDataType().m_wType == DATA_TXT_TYPE
					)
				{
					// Se la stringa contiene piu` righe (separate da CR-LF o LF) sfrutta le
					// righe a disposizione e poi visualizza 3 punti di sospensione nel caso
					// di overflow
					//
					int nRows = m_nLinesPerRow > 0 ? m_nLinesPerRow - 1 : 0;
					int nLen = strCell.GetLength();
					for (int i = 0; i < nLen; i++)
						if (strCell[i] == LF_CHAR || strCell[i] == CR_CHAR)
						{
							if (nRows == 0)
							{
								if (i == (nLen - 1) || (i == (nLen - 2) && strCell[i + 1] == LF_CHAR))
								{
									strCell.SetAt(i, '\0');
									break;
								}

								// per appendere "..." ----------------------v
								TCHAR* pszStr = strCell.GetBufferSetLength(i + 3);
								TB_TCSCPY(&pszStr[i], _T("..."));
								strCell.ReleaseBuffer();
								break;
							}
							else
								if (strCell[i] == LF_CHAR)
									nRows--;
						}
				}

				if (bUseCustomColor)
				{
					BOOL bEraseBk = CurRow.m_crBkgColor != pDC->GetBkColor();

					crOldBkgColor = pDC->SetBkColor(CurRow.m_crBkgColor);
					crOldTextColor = pDC->SetTextColor(CurRow.m_crTextColor);

					if (bEraseBk)
					{
						if (CurRow.m_dBkgPercFill < 1.0 && CurRow.m_dBkgPercFill >= 0.0)
						{
							int nXRight = int(nXStart + (nXEnd - nXStart) * CurRow.m_dBkgPercFill);

							CRect rcFiller2(nXStart, nYStart, nXRight, nYEnd);
							FillRect(pDC, rcFiller2, CurRow.m_crBkgColorFill);
						}
					}

					if (CurRow.m_nIconIDB)
					{
						if
							(
								!bmpIcon.LoadBitmap(CurRow.m_nIconIDB)
								||
								!(bmpIcon.GetObject(sizeof(bmIconInfo), &bmIconInfo) == sizeof(bmIconInfo))
								)
						{
							CurRow.m_nIconIDB = 0;
							ASSERT(FALSE);
						}
						else if (pColumnInfo->m_nAlignFlags == TA_LEFT)
							nXTxtStart += bmIconInfo.bmWidth;
					}
				}

				if (
					(pDataObj->GetDataType().m_wType == DATA_STR_TYPE || pDataObj->GetDataType().m_wType == DATA_TXT_TYPE) &&
					m_nLinesPerRow > 1 && !IsColumnSingleLine(pColumnInfo)
					)
				{
					rectDraw = CRect(nXTxtStart + 1, nYStart + 3, nXEnd /*- DEFAULT_COLUMN_OFFSET*/, nYEnd);

					OnDrawText(pDC, rectDraw, strCell, pRec, pColumnInfo);
				}
				else
				{
					rectDraw = CRect(nXStart + nRectOffs + (pColumnInfo->m_nAlignFlags == TA_LEFT ? DEFAULT_COLUMN_OFFSET : 1), nYStart, nXEnd /*- DEFAULT_COLUMN_OFFSET*/, nYEnd);

					OnExtTextOut(nXTxtStart + nXOffset, nYStart + 3, pDC, rectDraw, strCell, pRec, pColumnInfo); //reimplementata da CTreeBodyEdit
				}

				if (bUseCustomColor)
				{
					if (CurRow.m_nIconIDB)
					{
						CDC memDC;
						memDC.CreateCompatibleDC(pDC);
						CBitmap* pOld = memDC.SelectObject(&bmpIcon);

						// draw the icon contents
						pDC->StretchBlt
						(
							nXStart, nYStart,
							bmIconInfo.bmWidth, min(nYEnd - nYStart, bmIconInfo.bmHeight),
							&memDC,
							0, 0,
							bmIconInfo.bmWidth, bmIconInfo.bmHeight,
							SRCCOPY
						);
						memDC.SelectObject(pOld);
					}
					pDC->SetBkColor(crOldBkgColor);
					pDC->SetTextColor(crOldTextColor);
				}
			}
		}

	if (drawinfo.m_pOldFont)
	{
		pDC->SelectObject(drawinfo.m_pOldFont);
		drawinfo.m_pOldFont = NULL;
	}
	font.DeleteObject();
	pDC->SetBkColor(drawinfo.m_crOldBkgrd);
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::GetCustomColor(int nRow, int nCol, CBodyEditRowSelected* pCurRow)
{
	return (
		GetDocument()->DispatchOnGetCustomColor(this, pCurRow)
		||
		OnGetCustomColor(pCurRow)
		);
}

//-----------------------------------------------------------------------------
CString CBodyEdit::OnFormatData(CBodyEditRowSelected* pCurrentRow, int& nXOffset)
{
	ColumnInfo* pCol = pCurrentRow->m_pCol;
	if (pCol->m_bUseOtherColumnCtrl)
	{
		pCol = GetAlternativeColumnForEditing(pCurrentRow->m_pCol, pCurrentRow->m_pRec);
		if (pCol)
		{
			pCurrentRow->m_pCellData = pCurrentRow->m_pRec->GetDataObjAt(pCol->GetDataInfoIdx());
			pCurrentRow->m_nColumnIdx = pCol->m_nColPos;
			pCurrentRow->m_nColumnIDC = pCol->GetCtrlID();
			pCurrentRow->m_pCol = pCol;
		}
		else
			pCol = pCurrentRow->m_pCol;
	}

	if (pCol->IsDynamicDecimalFormatter())
	{
		return pCol->OnFormatDynamicDecimal(pCurrentRow->m_pCellData, pCurrentRow->m_pRec, nXOffset);
	}

	if (pCol->HasSingleComponentToFormat())
	{
		ASSERT(pCol->m_pComponent);
		DataObj* pObj = pCol->m_pComponent->GetComponent(pCurrentRow->m_pRec);

		return pObj->Str();
	}

	return pCol->GetParsedCtrl()->FormatData(pCurrentRow->m_pCellData);
}

//-----------------------------------------------------------------------------
BOOL  CBodyEdit::OnExtTextOut(int nXTxtStart, int nYStart, CDC* pDC, CRect& rectDraw, const CString& strCell, SqlRecord*, ColumnInfo*)
{
	return pDC->ExtTextOut
	(
		nXTxtStart, nYStart,
		ETO_OPAQUE | ETO_CLIPPED, &rectDraw,
		strCell, strCell.GetLength(),
		NULL
	);
}

//-----------------------------------------------------------------------------
BOOL  CBodyEdit::OnDrawText(CDC* pDC, CRect& rectDraw, const CString& strCell, SqlRecord*, ColumnInfo*)
{
	return 	pDC->DrawText(strCell, strCell.GetLength(), &rectDraw, DT_WORDBREAK | DT_LEFT | DT_NOPREFIX);
}

//=============================================================================
void CBodyEdit::OnCalcFooter(ColumnInfo* pInfo, DataObj* pValue)
{
	if (!m_pDBT)
		return;
	ASSERT_VALID(pValue);
	if (!pValue)
		return;

	m_pDBT->CalcSum(pInfo->m_nDataInfoIdx, *pValue);

	InvalidateBody();
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CanHideColumn(ColumnInfo* pCol)
{
	ASSERT_VALID(pCol);
	if (!BEGetExStyle(BE_STYLE_ALLOW_REMOVE_COLUMN_INTERACTIVE))
		return FALSE;
	return (pCol->GetStatus() & STATUS_NOCHANGE_HIDDEN) == 0;
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnKillFocus(CWnd* pWnd)
{
	// se perde il fuoco per un suo control non si deve fare nella
	if (!pWnd || IsChild(pWnd) || !m_pDBT)
		return;

	int nRelationship = GetRelationship(this, pWnd);

	if (!CheckRecords(FALSE))
	{
		if (nRelationship != FOREIGN_FOCUSED && pWnd->GetDlgCtrlID() != ID_HELP)
			PostMessage(UM_BAD_VALUE, GetDlgCtrlID(), CTRL_IMMEDIATE_FATAL_ERROR | CTRL_FOCUS_LOSE_REJECTED);
		else
			SendMessage(UM_BAD_VALUE, GetDlgCtrlID(), CTRL_FATAL_ERROR | CTRL_FOCUS_LOSE_REJECTED);
	}

	m_wKeyState = 0;
	m_bBodyActive = FALSE;
	m_bBodyFocused = nRelationship == FOREIGN_FOCUSED;

	DrawCarret(m_nCurrRecordIdx);
	EnableButtons();

	if (m_nNotValidCellCol == -1 && m_nNotValidCellRec == -1)
	{
		CParsedForm* pForm = dynamic_cast<CParsedForm*>(GetParent());
		if (pForm)
			pForm->GetFormAncestor()->SendMessage(UM_LOSING_FOCUS, GetDlgCtrlID(), nRelationship);
	}

	ColumnInfo* pCol = GetCurrColumn();
	if (pCol && pCol->GetParsedCtrl()->GetButton() && (pCol->GetParsedCtrl()->GetDataType() == DataType::Date || pCol->GetParsedCtrl()->GetDataType() == DataType::DateTime))
		pCol->GetParsedCtrl()->GetButton()->SendMessage(UM_DESTROY_CALENDAR);
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnSetFocus(CWnd* pOldWnd)
{
	if (!m_pDBT || m_ColumnsInfo.GetSize() == 0 || m_nNotValidCellCol >= 0)
	{
		return;
	}

	BOOL bCurrActive = m_bBodyActive;

	m_bBodyFocused = TRUE;
	m_bBodyActive = TRUE;

	GetParent()->SEND_WM_COMMAND(GetDlgCtrlID(), BN_SETFOCUS, m_hWnd);

	if (m_bSetFocusOnly)
	{
		m_bSetFocusOnly = FALSE;
		return;
	}

	if ((m_nRecordIdxToFocus > -1 || m_nCurrColumnIdx < 0 || m_nCurrRecordIdx < 0) && (!IsReadOnly() || !m_pDBT->IsEmpty()))
	{
		//se la riga corrente del bodyedit e'stata cambiata a seguito di un cambio della corrente del dbt
		//(sono tenute allineate per far funzionare Magic Link e EasyBuilder) allora mi sono messo da parte
		//l'índice di riga che dovra'essere attivato al momento del guadagno di fuoco
		//cosicche' possa scattare il BEN_ROW_CHANGED
		int idx = max(m_nRecordIdxToFocus, 0);
		m_nStartRecordIdx = max(idx - m_nLastBodyRow, 0);

		m_nRecordIdxToFocus = -1;//disattivo il comportamento
		SetCurrRecord(idx);

		SetNextEditableColumnFrom(0, max(-1, m_nCurrColumnIdx - 1));

		m_nOldStartColumnIdx = m_nStartColumnIdx;
		m_nOldStartRecordIdx = m_nStartRecordIdx;
	}

	BOOL bRedraw = FALSE;
	if (m_nCurrRecordIdx >= 0 && m_nCurrRecordIdx <= m_pDBT->GetUpperBound() && !IsReadOnly())
	{
		if (IsCtrlVisible())
			m_ColumnsInfo[m_nCurrColumnIdx]->m_pParsedCtrl->SetCtrlFocus(TRUE);
		else
		{
			bRedraw = ResetToVisiblePosition();
			ShowCtrl(bRedraw);
		}
	}

	if (!bCurrActive && !bRedraw)
		DrawCarret(m_nCurrRecordIdx);

	EnableButtons();
}

//----------------------------------------------------------------------------
static void SetBodyCursor(LPTSTR pszIDC)
{
	HCURSOR hCur;
	if (
		pszIDC == MAKEINTRESOURCE(AFX_IDC_HSPLITBAR) ||
		pszIDC == MAKEINTRESOURCE(IDC_TB_HAND)
		)
	{
		HINSTANCE hInst = AfxFindResourceHandle(pszIDC, RT_GROUP_CURSOR);

		if ((hCur = ::LoadCursor(hInst, pszIDC)) == NULL)
		{
			// will not look as good
			TRACE0("SetBodyCursor: (Warning) not find splitter cursor - using system provided alternative.\n");

			if (pszIDC == MAKEINTRESOURCE(IDC_TB_HAND))
				hCur = ::LoadCursor(NULL, IDC_ARROW);
			else
				hCur = ::LoadCursor(NULL, IDC_SIZEWE);
		}
	}
	else
		hCur = ::LoadCursor(NULL, pszIDC);

	::SetCursor(hCur);
}

//----------------------------------------------------------------------------
int CBodyEdit::GetHorizontalOffset(ColumnInfo* pColumn)
{
	if (!pColumn->m_bVisible || ((pColumn->GetStatus() & STATUS_HIDDEN) == STATUS_HIDDEN))
		return 0;

	int colIdx = -1;
	for (int i = 0; i < m_ColumnsInfo.GetSize(); i++)
	{
		if (m_ColumnsInfo[i] == pColumn)
		{
			colIdx = i;
			break;
		}
	}

	if (colIdx < 0 || colIdx > m_ColumnsInfo.GetUpperBound())
	{
		ASSERT(FALSE);
		return 0;
	}
	int startIdx = GetLogicalStartColumnIdx();
	if (startIdx < 0 || startIdx > m_ColumnsInfo.GetUpperBound())
	{
		ASSERT(FALSE);
		return 0;
	}

	int xStartOff = m_ColumnsInfo[startIdx]->m_nHorzColOffset + 1 - CTRL_NOT_INSIDE;
	return m_rectBody.left + m_ColumnsInfo[colIdx]->m_nHorzColOffset - xStartOff;
}

//----------------------------------------------------------------------------
CBodyEdit::CursorPosArea CBodyEdit::GetCursorBodyPos
(
	CPoint& point,
	int& nCol, int& xColOffs,
	int& nRow, int& nNewCurrRec
)
{
	CBodyEdit::CursorPosArea retVal = OUT_OF_BODY;
	nNewCurrRec = -1;
	nCol = nRow = -1;

	if (!m_ColumnsInfo.GetSize())
		return retVal;

	if (!m_rectTitles.PtInRect(point) && !m_rectBody.PtInRect(point))
		return retVal;

	int startIdx = GetLogicalStartColumnIdx();
	if (startIdx < 0 || startIdx > m_ColumnsInfo.GetUpperBound())
		return retVal;

	int endIdx = GetEndColumnIdx();
	if (endIdx < 0 || endIdx > m_ColumnsInfo.GetUpperBound())
		return retVal;

	int xStartOff = m_ColumnsInfo[startIdx]->m_nHorzColOffset + 1 - CTRL_NOT_INSIDE;

	ColumnInfo* pColInfo = NULL;
	for (nCol = startIdx; nCol <= endIdx; nCol++)
	{
		pColInfo = m_ColumnsInfo[nCol];
		ASSERT_VALID(pColInfo);

		xColOffs = m_rectBody.left + pColInfo->m_nHorzColOffset - xStartOff;

		if (point.x > xColOffs && point.x <= xColOffs + pColInfo->GetScreenWidth())
			break;
	}

	if (nCol > m_ColumnsInfo.GetUpperBound())
		return retVal;

	if (m_rectTitles.PtInRect(point))
	{
		if (point.x > xColOffs + 2 && point.x <= xColOffs + pColInfo->GetScreenWidth() - 2)
			return IN_COLUMN_TITLE;

		if (nCol > 0 && point.x <= (xColOffs + 2))
		{
			nCol--;
			xColOffs = m_rectBody.left + m_ColumnsInfo[nCol]->m_nHorzColOffset - xStartOff;
		}

		retVal = IN_RESIZE_GRIP;
		return retVal;
	}

	nRow = (point.y - m_rectBody.top) / m_nRowsHeight;
	nNewCurrRec = nRow + m_nStartRecordIdx;

	// si puo` andare oltre l'ultimo record di solo una unita`
	if (nNewCurrRec > (m_pDBT ? m_pDBT->GetSize() : 0))
		return retVal;

	// impedisce di aggiungere piu` record di quelli prestabiliti (vedi SetMaxRecords())
	if (nNewCurrRec > m_nMaxBodyRecords - 1)
	{
		if (nNewCurrRec > MAX_BODY_RECORDS)
		{
			TRACE1("CBodyEdit::GetCursorBodyPos: too many body rows %d", nNewCurrRec);
			ASSERT(FALSE);
		}
		return retVal;
	}

	// se e` read only non si possono aggiungere records
	if (IsReadOnly() && nNewCurrRec > (m_pDBT ? m_pDBT->GetUpperBound() : -1))
		return IN_NEW_READ_ONLY_ROW;

	return IN_BODY;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CanLeaveCurrPos(int nNewCurrRec, BOOL bCheckLastRow /*= TRUE*/)
{
	if (!m_pDBT)
		return TRUE;

	if (!OnCanLeaveCurrPos(nNewCurrRec))
		return FALSE;

	if (!IsReadOnly() && m_nCurrRecordIdx >= 0 /*&& m_nCurrRecordIdx <= m_pDBT->GetUpperBound()*/)
	{
		ASSERT(m_nCurrRecordIdx <= m_pDBT->GetUpperBound());

		// anche se il control non e` visibile si deve fare il controllo
		// per tenere conto della possibilta` che il corrente record non sia valido

		int fromCurr = nNewCurrRec - m_nCurrRecordIdx;

		// fromCurr < 0 means go up
		// fromCurr != 0 means check current row before leave it 
		if (!CanDoMoving(fromCurr < 0, fromCurr != 0))
			return FALSE;

		//bCheckLastRow aggiunto da PERASSO perche' non sappiamo il senso del controllo sull'ultima riga

		// give DBTSlaveBuffered a chance to check LAST record if not checked by previousus CanDoMoving()
		if (bCheckLastRow &&
			m_nCurrRecordIdx != m_pDBT->GetUpperBound() &&
			(nNewCurrRec == m_pDBT->GetUpperBound() + 1 || nNewCurrRec < 0) &&
			SetToBadCell(m_pDBT->GetUpperBound(), m_pDBT->CheckRow(m_pDBT->GetUpperBound()), TRUE)
			)
			return FALSE;
	}

	return TRUE;
}

// Viene utilizzata per l'XOR a video quando si ridimensiona la colonna tramite
// il mouse
//-----------------------------------------------------------------------------
void CBodyEdit::TrackingDraw(CDC& dc)
{
	// Disegno della riga tratteggiate nella vecchia e nella nuova posizione
	// (essendo in XOR disegnare uno stesso oggetto nella vecchia posizione
	// significa cancellarlo)
	dc.MoveTo(m_PrevMousePos.x, m_rectTitles.top);
	dc.LineTo(m_PrevMousePos.x, m_rectBody.bottom);
}

// Procedura che disegna la linea verticale tratteggiat quando si ridimensiona
// una colonna tramite il mouse.
//-----------------------------------------------------------------------------
void CBodyEdit::DrawVerticalLine(CPoint& point, COLORREF crPen, int nPenStyle)
{
	// Preparazionde del DC su cui operare
	CClientDC dc(this);

	// Selezione degli oggetti grafici predefiniti nel DC
	CPen	pen(nPenStyle, 1, crPen);
	CPen*	pOldPen = dc.SelectObject(&pen);

	// funzione di XOR sul DC
	// Cancellazione della linea precedente
	dc.SetROP2(R2_NOTXORPEN);
	if (m_bDrawXORFirstTime)
		m_bDrawXORFirstTime = FALSE;
	else
		TrackingDraw(dc);

	// Visualizzazione della linea nella posizione corrente
	if (point.x != -1 && point.y != -1)
	{
		m_PrevMousePos = point;
		TrackingDraw(dc);
	}

	// Deselezione degli oggetti grafici nel DC
	dc.SelectObject(pOldPen);
	VERIFY(pen.DeleteObject());
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnMouseMove(UINT nFlags, CPoint point)
{
	CPoint ptMouse = point;
	CGridControl::OnMouseMove(nFlags, point);

	CString sMessageOnStatusBar;

	// Controlla se l'area critica non sia già bloccata
	if (!(m_nResizingCol < 0 || m_pProcessingMouse->IsLocked()))
	{
		// Ridisegno nella posizione corrente del mouse la nuova dimensione della colonna.
		if (m_PrevMousePos != point && abs(point.x - m_PrevMousePos.x) >= 2)
		{
			DrawVerticalLine(point, AfxGetThemeManager()->GetBEResizeColVerticalLineColor(), PS_DOT);

			m_bMousePosChanged = TRUE;
		}

		// Riabilita l'accesso all'area critica
		m_pProcessingMouse->Unlock();
		return;
	}

	//--------------------------------------
	int nCol, xColOffs, nRow, nNewCurrRec;
	CursorPosArea cpa = GetCursorBodyPos(point, nCol, xColOffs, nRow, nNewCurrRec);

	// gestione valida solo nel caso di selezioni multiple
	if (GetMultiSelMode() == MULTIPLE_SEL)
	{
		// se non sono in stato di cattura mouse (ovvero il pulsante sinistro e'
		// stato rilasciato) non devo fare niente...
		if (GetCapture() == this)
		{
			// cerco la riga sotto il cursore...
			// se e' diversa da quella precedentemente attiva
			if (cpa == IN_BODY)
			{
				if (nNewCurrRec != m_IdxEndSelection)
				{
					// tolgo la selezione precedente
					ChangeRangeStatus(m_IdxLastSelection, m_IdxEndSelection, NOT_SELECTED, NOT_INVERT, FALSE);

					// ed imposto la nuova...
					m_IdxEndSelection = nNewCurrRec;
					ChangeRangeStatus(m_IdxLastSelection, m_IdxEndSelection, SELECTED);
				}
			}
		}
	}

	ColumnInfo* pColumnInfo = nCol >= 0 && nCol < m_ColumnsInfo.GetSize() ? m_ColumnsInfo[nCol] : NULL;

	//column lock
	if (BEGetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK_INTERACTIVE))
	{
		if (pColumnInfo && m_rectTitles.PtInRect(point))
		{
			m_nMouseOverTitle = nCol;

			CRect rectBitmap;
			rectBitmap.left = xColOffs + pColumnInfo->GetScreenWidth() - LOCK_EXTRARSPACE - 2;
			rectBitmap.top = m_rectTitles.top + ((m_rectTitles.Height() - PNG_SIZE) / 2) + 1;
			rectBitmap.right = rectBitmap.left + PNG_SIZE;
			rectBitmap.bottom = rectBitmap.top + PNG_SIZE;

			m_bMouseOverLockBitmap = rectBitmap.PtInRect(point);

			//----
			rectBitmap.left = xColOffs + 2;
			rectBitmap.top = m_rectTitles.top + ((m_rectTitles.Height() - PNG_SIZE) / 2);
			rectBitmap.right = rectBitmap.left + PNG_SIZE;
			rectBitmap.bottom = rectBitmap.top + PNG_SIZE;

			m_bMouseOverCloseBitmap = rectBitmap.PtInRect(point);
		}
		else
		{
			m_nMouseOverTitle = -1;

			m_bMouseOverCloseBitmap = m_bMouseOverLockBitmap = FALSE;
		}

		InvalidateRect(m_rectTitles);
	}

	TRACKMOUSEEVENT tk;
	tk.cbSize = sizeof(tk);
	tk.dwFlags = TME_LEAVE;
	tk.dwHoverTime = 0;
	tk.hwndTrack = m_hWnd;
	_TrackMouseEvent(&tk);

	m_ttp.m_strTitle.Empty();
	m_ttp.m_strText.Empty();

	if (cpa == IN_COLUMN_TITLE)
	{
		if (pColumnInfo && m_rectTitles.PtInRect(point))
		{
			m_ttp.m_nColumnIdx = nCol;
			m_ttp.m_pCol = pColumnInfo;
			m_ttp.m_nControlID = pColumnInfo->GetCtrlID();
			m_ttp.m_ti.uId = nCol;

			m_ttp.m_strText = pColumnInfo->GetToolTip();

			//TooltipTrackPos(ptMouse);

			if (BEGetExStyle(BE_STYLE_SHOW_DATATIP) && m_pDataTip && !m_ttp.m_strText.IsEmpty())
				m_pDataTip->Set(ptMouse, m_ttp.m_strText);

			sMessageOnStatusBar = m_ttp.m_strText;
		}
	}
	else if (pColumnInfo && cpa == IN_BODY && m_pDBT && nNewCurrRec < m_pDBT->GetSize())
	{
		m_ttp.m_nColumnIdx = nCol;
		m_ttp.m_pCol = pColumnInfo;
		m_ttp.m_nControlID = pColumnInfo->GetCtrlID();
		m_ttp.m_nRowDbt = nNewCurrRec;
		m_ttp.m_pRec = m_pDBT->GetRow(nNewCurrRec);
		m_ttp.m_pCellData = m_ttp.m_pRec ? GetDataObjAt(m_ttp.m_pRec, m_ttp.m_pCol) : NULL;

		m_ttp.m_ti.uId = (nCol << 16) + nNewCurrRec;

		if (m_DataTipStyle & CBodyEdit::BE_DTSTYLE_HEADER_ON_CELL)
			m_ttp.m_strTitle = pColumnInfo->GetToolTip();

		if (m_ttp.m_pCellData && !m_ttp.m_pCellData->IsHide() && OnGetToolTipProperties(&m_ttp))
		{
			if (BEGetExStyle(BE_STYLE_SHOW_DATATIP) && m_pDataTip && !m_ttp.m_strText.IsEmpty())
			{
				m_pDataTip->Set
				(
					ptMouse,
					m_ttp.m_strTitle.IsEmpty() ? m_ttp.m_strText : ('[' + m_ttp.m_strTitle + ']' + '\n' + m_ttp.m_strText)
				);
			}
			if (!m_ttp.m_strText.IsEmpty() || !m_ttp.m_strTitle.IsEmpty())
			{
				m_ttp.m_strText.Replace('\n', ' ');
				sMessageOnStatusBar = m_ttp.m_strTitle.IsEmpty() ? m_ttp.m_strText : m_ttp.m_strTitle + _T(": ") + m_ttp.m_strText;
			}
		}
	}

	SetStatusBarText(sMessageOnStatusBar);

	// ScrollBar hidden
	if (m_pVScrollBar && !m_pVScrollBar->isVisible())
	{
		int min = m_rectBody.right - GetSystemMetrics(SM_CXVSCROLL) - 5; // 20px Gap
		int max = m_rectBody.right;
		if ((ptMouse.x > min && ptMouse.x < max) && (ptMouse.y > m_rectBody.top && ptMouse.y < m_rectBody.bottom))
		{
			m_pVScrollBar->SetVisible(TRUE);
			CalcBodyLayout();
			RepositionAccessories();
			InvalidateBody();
		}
	}

}

//-----------------------------------------------------------------------------
void CBodyEdit::SetStatusBarText(CString strText)
{
	if (!BEGetExStyle(BE_STYLE_SHOW_STATUSBAR))
		return;
	CAbstractFormDoc* pDoc = GetDocument();
	if (pDoc)
	{
		CMasterFrame* pFrame = pDoc->GetMasterFrame();
		if (pFrame)
		{
			if (strText.IsEmpty())
				pFrame->GetMessageString(AFX_IDS_IDLEMESSAGE, strText);

			pFrame->SetStatusBarText(strText);
		}
	}
}
//-----------------------------------------------------------------------------
LRESULT CBodyEdit::OnMouseLeave(WPARAM wPawam, LPARAM lParam)
{
	if (BEGetExStyle(BE_STYLE_SHOW_DATATIP) && m_pDataTip && !m_pDataTip->IsVisible())
		SetStatusBarText(_T(""));
	return !0; //
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT nMsg)
{
	// Se non sono sulla windows corrente e non sono nella cliente area devo utilizzare
	// la procedura di default.
	if (pWnd != this || nHitTest != HTCLIENT || !m_ColumnsInfo.GetSize())
		return CGridControl::OnSetCursor(pWnd, nHitTest, nMsg);

	// Selezione del cursore e del relativo messaggio nella status bar in base alla
	// posizione del mouse
	CPoint point;
	GetCursorPos(&point);
	ScreenToClient(&point);

	if (BEGetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK_INTERACTIVE) && m_bMouseOverLockBitmap)
	{
		HCURSOR hCur;
		HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(IDC_HAND), RT_GROUP_CURSOR);

		if ((hCur = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_HAND))) == NULL)
			hCur = ::LoadCursor(NULL, IDC_ARROW);

		::SetCursor(hCur);
		return TRUE;
	}

	// dummy usate per i calcoli interni
	int nCol, xColOffs, nRow, nNewCurrRec;
	CursorPosArea cpa = GetCursorBodyPos(point, nCol, xColOffs, nRow, nNewCurrRec);
	if (cpa == IN_RESIZE_GRIP)
		SetBodyCursor(MAKEINTRESOURCE(AFX_IDC_HSPLITBAR));
	else if (cpa == IN_BODY)
	{
		ColumnInfo*		pCol = m_ColumnsInfo[nCol];
		SqlRecord*		pRec = NULL;
		DataObj*		pD = NULL;
		if (
			pCol &&
			pCol->IsReadOnly(GetCurrRecord()) &&
			nNewCurrRec == m_nCurrRecordIdx &&
			pCol->HasHyperLink() &&
			m_pDBT &&
			!m_pDBT->IsEmpty() &&
			(pRec = m_pDBT->GetRow(m_nCurrRecordIdx)) != NULL &&
			(pD = GetDataObjAt(pRec, pCol)) != NULL &&
			!pD->IsEmpty()
			)
			SetBodyCursor(MAKEINTRESOURCE(IDC_TB_HAND));
		else
			SetBodyCursor(IDC_ARROW);
	}
	else
		SetBodyCursor(IDC_ARROW);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::GetCoordCell(CRect& rectCell, int& nRowDbt, UINT& nIDC, int& nCol)
{
	//Get the mouse position
	const MSG* pMessage = GetCurrentMessage();
	ASSERT(pMessage);
	CPoint pt = pMessage->pt; //Get the point from the message

	ScreenToClient(&pt); //Convert the point's coords to be relative to this control

	//See if the point falls onto a cell
	int xColOffs, nVisRow;
	CursorPosArea cpa = GetCursorBodyPos(pt, nCol, xColOffs, nVisRow, nRowDbt);
	if (cpa != IN_BODY || nCol < 0 || nCol > m_ColumnsInfo.GetUpperBound())
		return FALSE;

	if (
		!IsCtrlVisible() ||
		m_nCurrColumnIdx != nCol || m_nCurrRecordIdx != nRowDbt
		)
	{
		rectCell.left = xColOffs;
		rectCell.right = rectCell.left + m_ColumnsInfo[nCol]->GetScreenWidth();
		rectCell.top = m_rectBody.top + m_nRowsHeight * nVisRow;
		rectCell.bottom = rectCell.top + m_nRowsHeight;

		nIDC = m_ColumnsInfo[nCol]->m_nCtrlIDC;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBodyEdit::CalcCellRect(CRect& rect, int nRow, int nCol)
{
	int	xColOffs = m_rectBody.left;

	for (int nColonna = GetLogicalStartColumnIdx(); nColonna < nCol; nColonna++)
	{
		xColOffs += m_ColumnsInfo[nColonna]->GetScreenWidth();
	}

	rect.top = m_rectBody.top + (nRow - m_nStartRecordIdx) * m_nRowsHeight;
	rect.bottom = rect.top + m_nRowsHeight;
	rect.left = xColOffs;
	rect.right = xColOffs + m_ColumnsInfo[nCol]->GetScreenWidth();
}

//-----------------------------------------------------------------------------
int	CBodyEdit::DoToolHitTest(CPoint point, TOOLINFO* pTI)
{
	//See if the point falls onto a cell
	int nCol, xColOffs, nVisRow, nRowDbt;
	CursorPosArea cpa = GetCursorBodyPos(point, nCol, xColOffs, nVisRow, nRowDbt);

	if (cpa == IN_COLUMN_TITLE)
	{
		pTI->hwnd = m_hWnd;
		pTI->uId = (UINT)nCol;
		pTI->lpszText = LPSTR_TEXTCALLBACK;
		pTI->uFlags |= TTF_IDISHWND;

		return pTI->uId;
	}
	else if
		(
			cpa != IN_BODY ||
			(
				IsCtrlVisible() &&
				m_nCurrColumnIdx == nCol && m_nCurrRecordIdx == nRowDbt
				)
			)
		return CGridControl::OnToolHitTest(point, pTI);

	pTI->hwnd = m_hWnd;
	pTI->uId = (nCol << 16) + nRowDbt;
	pTI->lpszText = LPSTR_TEXTCALLBACK;
	pTI->uFlags |= TTF_IDISHWND;

	return pTI->uId;	//By returning a unique value per cell Item,
	//we ensure that when the mouse moves over another cell,
	//the tooltip will change
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::DoToolTipNotify(CTooltipProperties& tp)
{
	m_ttp.m_ti.uId = tp.m_nControlID;
	tp.m_strText.Empty();
	/*
			if (tp.m_nControlID == IDC_BE_GOHOME)
			{
			if (m_pBtnGoHome && m_pBtnGoHome->m_hWnd)
			{
			tp.m_strText = m_pBtnGoHome->m_strToolTip;
			return TRUE;
			}
			}
			else if (tp.m_nControlID == IDC_BE_GOEND)
			{
			if (m_pBtnGoEnd && m_pBtnGoEnd->m_hWnd)
			{
			tp.m_strText = m_pBtnGoEnd->m_strToolTip;
			return TRUE;
			}
			}

			else if (tp.m_nControlID == IDC_BE_INSERT)
			{
			if (m_pBtnInsert && m_pBtnInsert->m_hWnd)
			{
			tp.m_strText = m_pBtnInsert->m_strToolTip;
			return TRUE;
			}
			}
			else if (tp.m_nControlID == IDC_BE_DELETE)
			{
			if (m_pBtnDelete && m_pBtnDelete->m_hWnd)
			{
			tp.m_strText = m_pBtnDelete->m_strToolTip;
			return TRUE;
			}
			}
			else if (tp.m_nControlID == IDC_BE_CALLDIALOG)
			{
			if (m_pBtnCallDialog && m_pBtnCallDialog->m_hWnd)
			{
			tp.m_strText = m_pBtnCallDialog->m_strToolTip;
			return TRUE;
			}
			}
			else if (tp.m_nControlID == IDC_BE_SPINROWS)
			{
			if (m_pBtnSpinRows && m_pBtnSpinRows->m_hWnd)
			{
			ASSERT(tp.m_nControlID == IDC_BE_SPINROWS);
			tp.m_strText = _TB("Increase\\reduce row's width");
			return TRUE;
			}
			}
			else if (tp.m_nControlID == IDC_BE_SEARCH)
			{
			if (m_pBtnSearch && m_pBtnSearch->m_hWnd)
			{
			tp.m_strText = m_pBtnSearch->m_strToolTip;
			return TRUE;
			}
			}
			else if (tp.m_nControlID == IDC_BE_MULTISELECTION)
			{
			if (m_pBtnMultiSelection && m_pBtnMultiSelection->m_hWnd)
			{
			tp.m_strText = m_pBtnMultiSelection->m_strToolTip;
			return TRUE;
			}
			}

			for (int i = 0; i <= m_UserButtons.GetUpperBound(); i++)
			{
			CBodyBitmapButton* pBodyBitmapButton = (CBodyBitmapButton*)m_UserButtons[i];

			if ((pBodyBitmapButton->m_nButtonIDC == tp.m_nControlID) && !pBodyBitmapButton->m_strToolTip.IsEmpty())
			{
			tp.m_strText = pBodyBitmapButton->m_strToolTip;
			return !tp.m_strText.IsEmpty();
			}
			}
			*/
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::OnGetToolTipProperties(CBETooltipProperties* tp)
{
	if (m_DataTipStyle == BE_DTSTYLE_NONE)
		return FALSE;

	CWnd* pCtrl = NULL;
	if (tp->m_pCol->GetParsedCtrl())
	{
		pCtrl = tp->m_pCol->GetParsedCtrl()->GetCtrlCWnd();
		if (pCtrl && pCtrl->IsKindOf(RUNTIME_CLASS(CIntBitmap)))
			return FALSE;
		if (pCtrl && pCtrl->IsKindOf(RUNTIME_CLASS(CEdit)) && pCtrl->GetStyle() & ES_PASSWORD)
			return FALSE;
	}

	tp->m_strTitle = tp->m_pCol->GetToolTip();

	if (pCtrl && pCtrl->IsKindOf(RUNTIME_CLASS(CParsedStateImage)))
	{
		CParsedStateImage* pState = (CParsedStateImage*)pCtrl;
		ASSERT_KINDOF(DataInt, tp->m_pCellData);
		tp->m_strText = pState->GetTooltip((DataInt*)tp->m_pCellData);
		return TRUE;
	}

	if (tp->m_pCellData->IsKindOf(RUNTIME_CLASS(DataDate)))
	{
		DataDate* pd = (DataDate*)tp->m_pCellData;
		if (!pd->IsEmpty() && !pd->IsATime())
		{
			tp->m_strText = tp->m_pCellData->FormatData() + '\n' + cwsprintf(_TB("{0-%s}; Week: {1-%d}"), (LPCTSTR)pd->WeekDayName(), pd->WeekOfYear());
			return TRUE;
		}
	}

	if (GetDocument())
	{
		if (
			((m_DataTipStyle & CBodyEdit::BE_DTSTYLE_BROWSE) == 0)
			&&
			GetDocument()->GetFormMode() == CBaseDocument::BROWSE
			)
			return FALSE;
		if (
			((m_DataTipStyle & CBodyEdit::BE_DTSTYLE_EDIT) == 0)
			&&
			(
				GetDocument()->GetFormMode() == CBaseDocument::EDIT ||
				GetDocument()->GetFormMode() == CBaseDocument::NEW
				)
			)
			return FALSE;
		if (
			((m_DataTipStyle & CBodyEdit::BE_DTSTYLE_BATCH) == 0)
			&&
			(
				GetDocument()->GetType() == VMT_BATCH
				)
			)
			return FALSE;
	}

	tp->m_strText = tp->m_pCellData->FormatData();

	if (tp->m_pCol && tp->m_pCol->IsAddress() && !tp->m_strText.IsEmpty())
	{
		tp->m_strText += '\n' + tp->m_pCol->m_pAddressComp->GetAddressText(tp->m_pRec);
	}

	CDC* pDC = GetDC();
	CFont* pFont = GetFont();
	CSize sz = ::GetTextSize(pDC, tp->m_strText, pFont);
	ReleaseDC(pDC);
	if (sz.cx < (tp->m_pCol->m_nColScrnWidth - DEFAULT_COLUMN_OFFSET))
		return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnLButtonDblClk(UINT nFlags, CPoint point)
{
	if (!m_pDBT)
		return;

	int nCol, xColOffs, nRow, nNewCurrRec;

	CursorPosArea cpa = GetCursorBodyPos(point, nCol, xColOffs, nRow, nNewCurrRec);
	if (cpa != IN_BODY)
		return;

	if (nNewCurrRec >= m_pDBT->GetSize())
		return;

	// viene salvato il corrente stato di body attivo che potrebbe
	// essere modificato dalle istruzioni seguenti
	BOOL bCurrActive = m_bBodyActive;

	// questo impedisce di forzare il fuoco se il control e` visibile perche`
	// in stato di bad
	//			
	if (!m_bBodyFocused && !IsCtrlVisible())
		SetFocusOnly();

	if (!CanLeaveCurrPos(nNewCurrRec)) return;

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	BOOL bRedraw = FALSE;

	// update current record 
	if (nNewCurrRec != m_nCurrRecordIdx)
	{
		SetCurrRecord(nNewCurrRec);
		bRedraw = TRUE;
	}

	if (nRow > m_nLastBodyRow)
	{
		m_nStartRecordIdx++;
		UpdateVScrollPos();
		bRedraw = TRUE;
	}

	// update current column
	m_nCurrColumnIdx = nCol;

	if (m_nCurrColumnIdx == GetEndColumnIdx() && !IsEndColFullVisible(GetLogicalStartColumnIdx()))
	{
		DoScrollHoriz(GetNextPageStartColumnIdx());
		UpdateHScrollPos();
		bRedraw = TRUE;
	}

	ColumnInfo* pCol = m_ColumnsInfo[m_nCurrColumnIdx];
	if (pCol)
	{
		BOOL bReadOnly = IsReadOnly() || pCol->IsReadOnly(GetCurrRecord());
		if (bReadOnly)
		{
			// Se un control e` visibile e si clicca su una colonna readonly
			// si deve dare il fuoco al body per far nascondere il control 
			SetFocusOnly();

			CParsedCtrl* pCtrl = pCol->GetParsedCtrl();

			if (pCtrl)
			{
				if (m_pDBT->IsEmpty())
				{
					ASSERT(FALSE);
					goto l_exit;
				}

				SqlRecord* pRec = m_pDBT->GetRow(nNewCurrRec);
				if (pRec == NULL)
				{
					ASSERT(FALSE);
					goto l_exit;
				}

				DataObj* pD = GetDataObjAt(pRec, pCol);

				CBodyEditRowSelected CurRow;
				CurRow.m_pCol = pCol;
				CurRow.m_pRec = pRec;
				CurRow.m_pCellData = pD;
				CurRow.m_nColumnIdx = m_nCurrColumnIdx;
				CurRow.m_nRowDbt = nNewCurrRec;
				CurRow.m_nColumnIDC = pCtrl->GetCtrlID();

				BOOL bOverridden =
					//priorità ai ClientDoc, poi al Doc ed infine al B.E. stesso
					(
					(GetDocument() && GetDocument()->DispatchOnDblClick(this, nFlags, &CurRow))
						||
						OnDblClick(nFlags, &CurRow)
						);

				if (!bOverridden)
				{
					if (pD && !pD->IsEmpty() && pCtrl->GetHotLink() && pCtrl->GetHyperLink())
						pCtrl->GetHyperLink()->DoFollowHyperlink(pD);
				}
			}
		}
	}
l_exit:
	if (bRedraw)
		InvalidateBody();
	if (!bCurrActive && !bRedraw)
		DrawCarret(m_nCurrRecordIdx);

	EnableButtons();
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnRButtonDown(UINT nFlags, CPoint point)
{
	int nCol, xColOffs, nRow, nNewCurrRec;
	CursorPosArea cpa = GetCursorBodyPos(point, nCol, xColOffs, nRow, nNewCurrRec);

	// questo impedisce di forzare il fuoco se il control e` visibile perche` in stato di bad			
	if (IsCtrlVisible())
	{
		if (!CanLeaveCurrPos(nNewCurrRec))
			return;
		SetFocusOnly();
	}

	if (GetMultiSelMode() != MULTIPLE_SEL)
	{
		CGridControl::OnRButtonDown(nFlags, point);
		return;
	}

	// gestione estesa....

	if ((cpa == IN_BODY) && GetAllowRDrag())
	{
		m_IdxRecBeginDrag = -1;

		// se la riga e' selezionata... prova il drag & drop....
		if (m_SelRecords[nNewCurrRec] == SELECTED)
		{
			m_rcDrag = CRect(
				point.x - DRAG_DELTA,
				point.y - DRAG_DELTA,
				point.x + DRAG_DELTA,
				point.y + DRAG_DELTA
			);

			ColumnInfo* pCol = nCol >= 0 && nCol <= m_ColumnsInfo.GetUpperBound() ? m_ColumnsInfo[nCol] : NULL;

			m_IdxRecBeginDrag = nNewCurrRec;
			m_DragButton = DB_RIGHT;
			if (!DoDrag(nRow, pCol ? pCol->GetCtrlID() : 0))
			{
				m_IdxRecBeginDrag = -1;
				// simula LBUTTON_UP
				StopScrollTimer();
				if (GetCapture() == this)
					ReleaseCapture();

				// Context Menu
				POINT	pt;
				CRect	rect_temp = m_rcDrag;
				::GetCursorPos(&pt);
				ClientToScreen(&rect_temp);

				if (rect_temp.PtInRect(pt))
				{
					SendMessage(WM_CONTEXTMENU, (WPARAM)m_hWnd, MAKELPARAM(pt.x, pt.y));
				}
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnLButtonDown(UINT nFlags, CPoint point)
{
	if (!m_pDBT || !GetDocument())
		return;

	int nCol, xColOffs, nRow, nNewCurrRec;
	CursorPosArea cpa = GetCursorBodyPos(point, nCol, xColOffs, nRow, nNewCurrRec);

	// Gestione blocco colonne interattivo
	if ((cpa == IN_COLUMN_TITLE) && BEGetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK_INTERACTIVE) && m_bMouseOverLockBitmap)
	{
		ColumnInfo* pColInfo = m_ColumnsInfo[nCol];

		// tolgo il fuoco ai controls...
		if (!UpdateData(TRUE))
			return;

		if (IsCtrlVisible())
			SetFocusOnly();

		if (pColInfo->GetCtrlID() == m_nLockedColumnIDC)
			UnLockColumn();
		else
			LockColumn(pColInfo->GetCtrlID());

		if (GetDocument() && GetDocument()->CanDoCustomize())
		{
			BodyEditInfo* pBEObject = NULL;
			CFormManager* pFormManager = GetDocument() ? GetDocument()->m_pFormManager : NULL;
			if (pFormManager)
				pBEObject = pFormManager->GetBodyEditInfo(GetInfoOSL()->m_Namespace);

			BodyEditColumn* beCol = GetColumnDescription(pColInfo);
			if (beCol)
			{
				beCol->m_bModified = TRUE;
				if (pBEObject)
					pBEObject->m_bModified = TRUE;
				if (pFormManager)
					pFormManager->m_bTBFModified = TRUE;
			}
		}
		Invalidate();
		return;
	}

	// Gestione nascondi colonne interattivo
	if ((cpa == IN_COLUMN_TITLE) && m_bMouseOverCloseBitmap)
	{
		ColumnInfo* pColInfo = m_ColumnsInfo[nCol];
		if (pColInfo->GetCtrlID() == m_nLockedColumnIDC)
			return;

		// tolgo il fuoco ai controls...
		if (!UpdateData(TRUE))
			return;

		if (IsCtrlVisible())
			SetFocusOnly();

		if (!CanHideColumn(pColInfo))
			return;

		if ((nFlags & MK_CONTROL) == 0)
		{
			if (AfxMessageBox(_TB("Do you want hide the column ? (You could avoid the question by pressing the CTRL button too)"), MB_YESNO | MB_ICONQUESTION) == IDNO)
			{
				return;
			}
		}
		if (!pColInfo->HideCtrl())
			return;

		pColInfo->m_bVisible = FALSE;

		if (GetDocument() && GetDocument()->CanDoCustomize())
		{
			//no! pColInfo->SetVisible(FALSE);
			pColInfo->m_wColumnStatus |= STATUS_HIDDEN;

			BodyEditInfo* pBEObject = NULL;
			CFormManager* pFormManager = GetDocument() ? GetDocument()->m_pFormManager : NULL;
			if (pFormManager)
				pBEObject = pFormManager->GetBodyEditInfo(GetInfoOSL()->m_Namespace);

			BodyEditColumn* beCol = GetColumnDescription(pColInfo);
			if (beCol)
			{
				beCol->m_wStatus |= STATUS_HIDDEN;
				beCol->m_bStatusChanged = TRUE;
				beCol->m_bModified = TRUE;

				if (pBEObject)
					pBEObject->m_bModified = TRUE;
				if (pFormManager)
					pFormManager->m_bTBFModified = TRUE;
			}
		}

		RecalcColumnsLayout();
		return;
	}

	if (cpa == IN_COLUMN_TITLE && BEGetExStyle(BE_STYLE_ALLOW_SORT))
	{
		ColumnInfo::ColumnOrderingType Ordering = m_ColumnsInfo[nCol]->GetColumnOrdering();

		if (
			Ordering != ColumnInfo::ASCFIXED &&
			Ordering != ColumnInfo::DESCFIXED &&
			Ordering != ColumnInfo::DISABLE
			)
		{
			if (!UpdateData(TRUE))
				return;

			if (IsCtrlVisible())
				SetFocusOnly();

			ToggleColumnOrdering(m_ColumnsInfo[nCol], (nFlags & MK_SHIFT) == 0);

			Sort();	//Click on column header 

			Invalidate();
			return;
		}
	}

	if (cpa != IN_BODY && cpa != IN_RESIZE_GRIP)
		return;

	if (cpa == IN_RESIZE_GRIP)
	{
		if (IsCtrlVisible() && !CanLeaveCurrPos(nNewCurrRec)) return;

		// Controlla se l'area critica non sia già bloccata
		if (m_pProcessingMouse->IsLocked())
			return;

		// Imposto la regione di catura degli eventi di imput (mouse e tastiera)
		// alla view corrente
		SetCapture();

		// Calcolo della regione di clipping
		CRect rectClient(m_rectTitles);
		rectClient.left = xColOffs + 3;
		rectClient.bottom = m_rectBody.bottom;

		ClientToScreen(&rectClient);

		ClipCursor(&rectClient);

		// Selezione di un nuovo cursore
		SetBodyCursor(MAKEINTRESOURCE(AFX_IDC_HSPLITBAR));

		m_PrevMousePos = point;

		// Selezione dei flag di modifica
		m_bMousePosChanged = FALSE;
		m_bDrawXORFirstTime = TRUE;
		m_nResizingCol = nCol;

		if (IsCtrlVisible())
			SetFocusOnly();

		// Riabilita l'accesso all'area critica
		m_pProcessingMouse->Unlock();
		return;
	}

	// viene salvato il corrente stato di body attivo che potrebbe
	// essere modificato dalle istruzioni seguenti
	//
	BOOL bCurrActive = m_bBodyActive;

	// questo impedisce di forzare il fuoco se il control e` visibile perche`
	// in stato di bad
	//			
	if (/*!m_bBodyFocused && */!IsCtrlVisible())	//bug.17160
		SetFocusOnly();

	if (!CanLeaveCurrPos(nNewCurrRec))
		return;

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	BOOL bRedraw = FALSE;

	// update current record 
	if (nNewCurrRec != m_nCurrRecordIdx)
	{
		SetCurrRecord(nNewCurrRec);

		bRedraw = TRUE;
		m_bTwoClicks = FALSE;
	}
	else
	{
		m_bTwoClicks = TRUE; // ho fatto click sulla riga corrente
		SetCapture(); // catturo il mouse, nel caso il bottone venga rilasciato altrove
	}

	if (nRow > m_nLastBodyRow)
	{
		m_nStartRecordIdx++;
		UpdateVScrollPos();
		bRedraw = TRUE;
	}

	// update current column
	m_nCurrColumnIdx = nCol;

	if (m_nCurrColumnIdx == GetEndColumnIdx() && !IsEndColFullVisible(GetLogicalStartColumnIdx()))
	{
		DoScrollHoriz(GetNextPageStartColumnIdx());
		UpdateHScrollPos();
		bRedraw = TRUE;
	}

	ColumnInfo* pCol = m_ColumnsInfo[m_nCurrColumnIdx];
	ASSERT_VALID(pCol);
	ColumnInfo* pColToBeUse = pCol;
	SqlRecord* pCurRec = GetCurrRecord();
	CParsedCtrl* pCtrl = NULL;

	if (pCol->m_bUseOtherColumnCtrl)
	{
		if (pCurRec)
		{
			ColumnInfo* pAltCol = GetAlternativeColumnForEditing(GetCurrColumn(), pCurRec);
			if (pAltCol)
			{
				pCtrl = pAltCol->GetParsedCtrl();
				pColToBeUse = pAltCol;
			}
		}
	}
	else
	{
		pCtrl = pCol->GetParsedCtrl();
		ASSERT(pCtrl);
	}

	BOOL bReadOnly = pCtrl ? pColToBeUse->IsReadOnly(pCurRec) : TRUE;

	if (bReadOnly)
	{
		// Se un control e` visibile e si clicca su una colonna readonly
		// si deve dare il fuoco al body per far nascondere il control
		SetFocusOnly();

		if (bRedraw)
			InvalidateBody();

		OnSelCell(pCurRec, pColToBeUse);
		if (m_pDBT && m_pDBT->GetDocument())
			m_pDBT->GetDocument()->DispatchOnBESelCell(this, pCurRec, pColToBeUse);
	}
	else
	{
		if (
			ShowCtrl(bRedraw) &&
			pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton))
			)
		{
			// Nel caso la colonna selezionata sia associata ad un CBoolButton
			// la transizione di stato del valore viene forzata immediatamente
			// all'atto del click del mouse sulla colonna stessa

			// corrente valore del DataBool associato
			BOOL bValue = (BOOL)*((DataBool*)(pCtrl->GetCtrlData()));

			// viene usata la SetCheck di CButton (MFC) poiche` la SetValue di CBoolButton (TB)
			// non invia il messaggio di VALUE_CHANGED
			//
			((CBoolButton*)(pCtrl->GetCtrlCWnd()))->SetCheck(bValue ? 0 : 1);
		}
	}

	if (!bCurrActive && !bRedraw)
		DrawCarret(m_nCurrRecordIdx);

	EnableButtons();

	// verifico se il click e' interno al body (non compresa la colonna che "chiude" a destra..
	if (cpa != IN_BODY)
	{
		return;
	}

	if (GetMultiSelMode() != MULTIPLE_SEL)
	{
		if (
			GetDocument() &&
			bReadOnly &&
			pCtrl &&
			pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedBitmap))
			)
		{
			GetDocument()->PostMessage(WM_COMMAND, pCtrl->GetCtrlCWnd()->GetDlgCtrlID(), (LPARAM)m_hWnd);
		}
		return;
	}

	////////////////////////////////////
	// Gestione Selezioni Multiple
	//

	m_IdxRecBeginDrag = -1;

	// se la riga e' selezionata... prova il drag & drop....
	if (nNewCurrRec >= 0 && nNewCurrRec < m_SelRecords.GetSize() && m_SelRecords[nNewCurrRec] == SELECTED)
	{
		m_rcDrag = CRect(
			point.x - DRAG_DELTA,
			point.y - DRAG_DELTA,
			point.x + DRAG_DELTA,
			point.y + DRAG_DELTA
		);

		m_IdxRecBeginDrag = nNewCurrRec;
		m_DragButton = DB_LEFT;
		if (!DoDrag(nRow, pCol->GetCtrlID()))
		{
			m_IdxRecBeginDrag = -1;
			// simula LBUTTON_DOWN
			HandleSelection(nFlags, nNewCurrRec);
			// simula LBUTTON_UP
			StopScrollTimer();
			if (GetCapture() == this)
				ReleaseCapture();
		}
	}
	else
	{
		HandleSelection(nFlags, nNewCurrRec);
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnLButtonUp(UINT, CPoint point)
{
	BOOL bMousePosChanged = m_bMousePosChanged;

	// Se non sono in fase di resizing della colonna, allora controlla se sto attivando un Hyperlink
	if (m_nResizingCol < 0 || m_pProcessingMouse->IsLocked())
	{
		int nCol, xColOffs, nRow, nNewCurrRec;
		CursorPosArea cpa = GetCursorBodyPos(point, nCol, xColOffs, nRow, nNewCurrRec);

		// se non sono nel body, non ho niente da fare
		if (cpa == IN_BODY)
		{
			ColumnInfo* pCol = m_ColumnsInfo[nCol];
			SqlRecord* pRec = NULL;
			if (
				m_bTwoClicks &&												// ho fatto click sulla riga corrente
				m_nCurrRecordIdx == nNewCurrRec	&&							// non ho cambiato riga tra il down e l'up
				pCol->IsReadOnly(GetCurrRecord()) &&						// il dataobj è read-only
				pCol->HasHyperLink() &&										// c'è un hyperlink
				m_pDBT &&													//C'è IL DBT
				!m_pDBT->IsEmpty() &&										// c'è almeno una riga
				(pRec = m_pDBT->GetRow(m_nCurrRecordIdx)) != NULL			// ho un record corrente
				)
			{
				pCol->BrowseHyperLink(pRec);
			}
		}

		// reset della situazione, e si rilascia il controllo del mouse
		m_bTwoClicks = FALSE;
		ReleaseCapture();
		goto l_end;
	}

	if (m_bMousePosChanged)
	{
		// spegne la linea tratteggiata
		DrawVerticalLine(CPoint(-1, -1), AfxGetThemeManager()->GetBEResizeColVerticalLineColor(), PS_DOT);

		ColumnInfo* pColInfo = m_ColumnsInfo[m_nResizingCol];

		int xColOffs = m_rectBody.left + pColInfo->m_nHorzColOffset -
			m_ColumnsInfo[GetLogicalStartColumnIdx()]->m_nHorzColOffset +
			1 - CTRL_NOT_INSIDE;

		int nNewWidth = m_PrevMousePos.x - xColOffs;

		ResizeColumn(pColInfo, nNewWidth, TRUE);
	}

	m_bMousePosChanged = FALSE;
	m_nResizingCol = -1;

	ReleaseCapture();
	ClipCursor(NULL);
	SetBodyCursor(IDC_ARROW);

	// Riabilita l'accesso all'area critica
	m_pProcessingMouse->Unlock();

l_end:
	if (GetMultiSelMode() != MULTIPLE_SEL)
		return;

	StopScrollTimer();

	if (GetCapture() == this)
		ReleaseCapture();
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::GetBodyItem(CPoint point, ColumnInfo*& pCol, int& nRowDBT, DataObj*& pCell)
{
	pCol = NULL;
	nRowDBT = -1;
	pCell = NULL;
	//----

	int nCol, xColOffs, nRowBE;
	CursorPosArea cpa = GetCursorBodyPos(point, nCol, xColOffs, nRowBE, nRowDBT);

	if (cpa == IN_COLUMN_TITLE)
	{
		pCol = m_ColumnsInfo[nCol];
		return TRUE;
	}

	if (cpa != IN_BODY)
		return FALSE;

	pCol = m_ColumnsInfo[nCol];

	if (nRowDBT > m_pDBT->GetUpperBound())	//Click oltre ultima riga: normalmente provoca l'append di una nuova riga.
		return FALSE;

	SqlRecord* pRec = m_pDBT->GetRow(nRowDBT);

	pCell = pRec->GetDataObjAt(pCol->GetDataInfoIdx());

	return TRUE;
}

//------------------------------------------------------------------------------
void CBodyEdit::ResizeColumn(ColumnInfo* pColInfo, ColumnInfo::ColumnWidthType wt)
{
	pColInfo->SetScreenWidth(wt);

	ResizeColumn(pColInfo);
}

//------------------------------------------------------------------------------
int CBodyEdit::ResizeColumn(ColumnInfo* pColInfo, int nNewWidth, BOOL bUpdateTbf)
{
	int nOldWidth = pColInfo->GetScreenWidth();
	pColInfo->SetScreenWidth(nNewWidth, bUpdateTbf);

	if (bUpdateTbf)
	{
		BodyEditInfo* pBEObject = NULL;
		CFormManager* pFormManager = GetDocument() ? GetDocument()->m_pFormManager : NULL;

		if (pFormManager)
			pBEObject = pFormManager->GetBodyEditInfo(GetInfoOSL()->m_Namespace);

		BodyEditColumn* pBEC = GetColumnDescription(pColInfo);
		if (pBEC)
		{
			pBEC->m_nColScreenWidth = nNewWidth;
			if (pBEObject)
				pBEObject->m_bModified = TRUE;
			pBEC->m_bModified = TRUE;
			if (pFormManager)
				pFormManager->m_bTBFModified = TRUE;
		}
	}

	ResizeColumn(pColInfo);

	return nOldWidth;
}

//------------------------------------------------------------------------------
void CBodyEdit::ResizeColumn(ColumnInfo* pColInfo)
{
	if (m_nLayoutSuspended)
		return;

	//TODO ricalcolare la width delle colonne solo a partire dalla corrente
	RecalcColumnsLayout();

	// devo far ridimensionare il control corrente corretto sulla base di tutti i ricalcoli
	if (pColInfo->m_pParsedCtrl && pColInfo->m_pParsedCtrl->GetCtrlCWnd())
	{
		CWnd* pCurrCtrlWnd = pColInfo->m_pParsedCtrl->GetCtrlCWnd();
		CRect aCurrCtrlPos;

		pCurrCtrlWnd->GetWindowRect(aCurrCtrlPos);
		pCurrCtrlWnd->ScreenToClient(aCurrCtrlPos);
		aCurrCtrlPos.right = aCurrCtrlPos.left + pColInfo->GetScreenWidth() - pColInfo->m_pParsedCtrl->GetAllButtonsWitdh();
		pCurrCtrlWnd->SetWindowPos(NULL, aCurrCtrlPos.left, aCurrCtrlPos.top, aCurrCtrlPos.Width(), aCurrCtrlPos.Height(), SWP_NOZORDER);
	}
}

//------------------------------------------------------------------------------
int CBodyEdit::ResizeColumn(UINT nIDC, int nNewWidth)
{
	// tolgo il fuoco ai controls...
	if (!UpdateData(TRUE))
		return -1;

	if (IsCtrlVisible())
		SetFocusOnly();

	ColumnInfo* pColInfo = this->GetColumn(nIDC);
	if (pColInfo == NULL)
	{
		TRACE(_T("Invalid resize column, unkonwn IDC %d\n"), nIDC);
		ASSERT(FALSE);
		return -1;
	}
	return ResizeColumn(pColInfo, nNewWidth, FALSE);
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnToggleAllColumnsWidth()
{
	// tolgo il fuoco ai controls...
	if (!UpdateData(TRUE))
		return;

	if (IsCtrlVisible())
		SetFocusOnly();

	CWaitCursor cur;
	ColumnInfo::ColumnWidthType wt;
	BOOL bFirst = TRUE;

	for (int i = 0; i < m_ColumnsInfo.GetSize(); i++)
	{
		ColumnInfo* pColInfo = m_ColumnsInfo[i];
		if (pColInfo->GetScreenWidth() == 0)
			continue;

		if (bFirst)
		{
			wt = pColInfo->GetNextWidthType();
			bFirst = FALSE;
		}
		ResizeColumn(pColInfo, wt);
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnToggleCurrentColumnWidth()
{
	// tolgo il fuoco ai controls...
	if (!UpdateData(TRUE))
		return;

	if (IsCtrlVisible())
		SetFocusOnly();

	if (m_nColSelected >= 0 && m_nColSelected < m_ColumnsInfo.GetSize())
	{
		ColumnInfo* pColumnInfo = m_ColumnsInfo[m_nColSelected];

		ResizeColumn(pColumnInfo, pColumnInfo->GetNextWidthType(FALSE));
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::RecalcColumnsTitleWidth()
{
	CDC* pDC = GetDC();
	CFont* pFont = GetFont();
	for (int i = 0; i < m_ColumnsInfo.GetSize(); i++)
	{
		ColumnInfo* pColInfo = m_ColumnsInfo[i];

		pColInfo->CalcTitleWidth(pDC, pFont);
	}
	ReleaseDC(pDC);
}

//------------------------------------------------------------------------------
BodyEditColumn* CBodyEdit::GetColumnDescription(ColumnInfo* pColInfo)
{
	if (pColInfo == NULL)
	{
		ASSERT(FALSE);
		return NULL;
	}

	BodyEditInfo* pBEObject = NULL;
	BodyEditColumn* pBEC = NULL;
	CFormManager* pFormManager = GetDocument() ? GetDocument()->m_pFormManager : NULL;
	if (pFormManager)
		pBEObject = pFormManager->GetBodyEditInfo(GetInfoOSL()->m_Namespace);

	if (pBEObject && pBEObject->IsKindOf(RUNTIME_CLASS(BodyEditInfo)))
	{
		pBEC = pBEObject->GetColumnObject(pColInfo->GetInfoOSL()->m_Namespace);
		if (pBEC)
			return pBEC;
	}
	return NULL;
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::DoInternalCommand(UINT nID, UINT nCode)
{
	if (nCode == 0)
	{
		{
			if (nID == IDC_BE_GOHOME)
			{
				DoMoveToHomeEnd(TRUE);
				return TRUE;
			}
			else if (nID == IDC_BE_GOEND)
			{
				DoMoveToHomeEnd(FALSE);
				return TRUE;
			}

			else if (nID == IDC_BE_CALLDIALOG)
			{
				if (!m_pDBT)
					return TRUE;
				// se sono in selezione multipla disabilito la rowview...
				if (GetMultiSelMode() == MULTIPLE_SEL)
				{
					return TRUE;
				}

				// se il bodyedit non e` ancora attivo gli si da` il fuoco
				if (!m_bBodyFocused || !m_bBodyActive || m_pDBT->IsEmpty())
				{
					CWnd* pOldWnd = SetFocus();
					if (pOldWnd && pOldWnd->m_hWnd == m_hWnd)
						OnSetFocus(this);
				}

				OnCallDialog();
				return TRUE;
			}

			else if (nID == IDC_BE_INSERT)
			{
				if (!m_pDBT || IsReadOnly())
					return TRUE;

				if (m_bBodyActive)
				{
					if (m_pDBT->IsEmpty())
					{
						m_nOldStartRecordIdx = -1;
						m_nOldStartColumnIdx = -1;

						CWnd* pOldWnd = SetFocus();
						if (pOldWnd && pOldWnd->m_hWnd == m_hWnd)
							OnSetFocus(this);
					}
					else
					{
						InsertRecord();
					}
				}

				return TRUE;
			}

			else if (nID == IDC_BE_DELETE)
			{
				if (!m_pDBT || m_pDBT->IsEmpty() || m_bInternalReadOnly)
					return TRUE;

				if (m_bBodyActive)
				{
					DeleteRecord();
				}

				return TRUE;
			}

			else if (nID == IDC_BE_SEARCH)
			{
				if (!m_pDBT || m_pDBT->IsEmpty())
					return TRUE;

				OnDoSearch();

				InvalidateBody();
				return TRUE;
			}

			else if (nID == IDC_BE_MULTISELECTION)
			{
				OnToggleMultipleSelection();
				return  TRUE;
			}

			else if (nID == IDC_BE_ROW_HEIGHT_MINUS)
			{
				ChangeHeightRow(SB_LINEUP);
				return TRUE;
			}
			else if (nID == IDC_BE_ROW_HEIGHT_PLUS)
			{
				ChangeHeightRow(SB_LINEDOWN);
				return TRUE;
			}
		}
	}

	if (nCode == EN_VALUE_CHANGED && nID == IDC_BE_SEARCH_TEXT)
	{
		if (!m_pDBT || m_pDBT->IsEmpty())
			return TRUE;

		CBEButton* pB = m_FooterToolBar.FindButton(IDC_BE_SEARCH_TEXT);
		if (!pB)
			pB = m_HeaderToolBar.FindButton(IDC_BE_SEARCH_TEXT);
		if (pB)
		{
			ASSERT_KINDOF(CBEBtnCtrl, pB);

			CBEBtnCtrl* pBtnSearchText = dynamic_cast<CBEBtnCtrl*>(pB);
			ASSERT_VALID(pBtnSearchText->m_pControl);

			ResetSearchPoint(TRUE);

			CString s;
			GetParsedCtrl(pBtnSearchText->m_pControl)->GetValue(s);
			if (!s.IsEmpty())
				m_arSearchText.Add(s);
			else
			{
				ResetSearchPoint(TRUE);
				return TRUE;
			}

			OnDoSearch();

			InvalidateBody();
			return TRUE;
		}
	}

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::OnCommand(WPARAM wParam, LPARAM lParam)
{
	if (!m_pDBT)
		return __super::OnCommand(wParam, lParam);

	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if (nID == 0 || hWndCtrl == NULL)
	{
		if (nID && GetDocument())
			GetDocument()->SendMessage(WM_COMMAND, wParam, lParam);

		return CGridControl::OnCommand(wParam, lParam);
	}

	ASSERT(::IsWindow(hWndCtrl));

	// body button management
	if (IsInternalCommand(nID))
	{
		if (DoInternalCommand(nID, nCode))
			return TRUE;
	}

	// Value Changed management
	if (nCode == EN_VALUE_CHANGED)
	{
		// Valida la riga corrente perche' qualcosa e' stato modificato nel control
		// e quindi si considera validata dall'utente la riga stessa.
		if (m_pDBT && m_nCurrRecordIdx >= 0 && m_nCurrRecordIdx < m_pDBT->GetSize())
		{
			SqlRecord* pRec = m_pDBT->GetRow(m_nCurrRecordIdx);
			if (!pRec->IsNeverStorable())
				pRec->SetStorable();
		}

		// Segnala che qualcosa e' stato modificato
		if (GetParent())
			GetParent()->SendMessage(WM_COMMAND, wParam, lParam);

		// in risposta al messaggio potrebbero essere stati modificati
		// gli stati di abilitazione dei bottoni per la corrente riga	
		EnableButtons();

		m_bBodyModified = TRUE;

		return TRUE;
	}

	CWnd* pWnd = CWnd::FromHandle(hWndCtrl);
	if (IsChild(pWnd) && GetParent())
	{
		if (nCode == 0)
		{
			if (/*nCode == 0 && */pWnd->IsKindOf(RUNTIME_CLASS(CBEButton)))
			{
				CBEButton* pBtn = (CBEButton*)pWnd;

				//allineato con CBEButton::OnLButtonUp:
				if (pBtn->m_hMenu && pBtn->m_nMenuResult)
				{
					SetFocusOnly();

					BOOL b = __super::OnCommand(MAKEWPARAM(pBtn->m_nMenuResult, 0), lParam);

					//forse non serve piu
					LRESULT lr = GetParent()->SEND_WM_COMMAND(pBtn->m_nMenuResult, 0, m_hWnd);

					OnCommandDone();

					Invalidate();
					UpdateWindow(); // immediate feedback

					return lr;	//TODO RICCARDO : manca OR con b ?
				}
			}

			__super::OnCommand(wParam, lParam);
		}

		return GetParent()->SendMessage(WM_COMMAND, wParam, lParam) != 0;
	}
	return __super::OnCommand(wParam, lParam);
}

//------------------------------------------------------------------------------
void CBodyEdit::OnCommandDone()
{
	EnableButtons();
	InvalidateBody();

	if (m_pDBT)
		m_pDBT->SetReadOnlyFields();
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnAbortForm()
{
	if (IsCtrlVisible())
	{
		m_ColumnsInfo[m_nCurrColumnIdx]->ReattachBaseData();
		SetFocusOnly();
	}

	m_nNotValidCellCol = -1;
	m_nNotValidCellRec = -1;

	// Elimina eventuali link a dati che potrebbero morire (righe nuove)
	if (m_pRowFormView)
		RebuildLinks(-1, TRUE);	//@@versione 2.5.1 : RebuildLinks()
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::OnCheckForm(BOOL bSignalError)
{
	if (!m_pDBT || m_pDBT->IsEmpty())
	{
		ASSERT(!IsCtrlVisible());

		ResetBodyStatus();

		return TRUE;
	}

	// tenta di updatare il dato : in caso di errore/warning
	// NON da subito il messaggio (FALSE) e viene SENDATO il
	// messaggio di UM_BAD_VALUE (TRUE); in questo modo vengono
	// solamente valorizzati i membri m_nNotValidCtrlID e m_wErrorMode
	// che sono poi utilizzati piu` sotto
	//
	return UpdateData(bSignalError, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::OnKeyHit(UINT nIDC, UINT nKey, UINT nHitState)
{
	if (nIDC == 0)
		return FALSE;

	if (nHitState == WM_SYSKEYDOWN || nHitState == WM_SYSKEYUP)
		return m_HeaderToolBar.ProcessSysKeyMessage(nKey) || m_FooterToolBar.ProcessSysKeyMessage(nKey);

	BOOL bDo = (
		nIDC == (UINT)GetDlgCtrlID() ||
		(GetCurrColumn() && nIDC == GetCurrColumn()->GetCtrlID())
		);

	if (!bDo && GetCurrColumn() && GetCurrColumn()->m_bUseOtherColumnCtrl && GetDBT() && GetDBT()->GetCurrentRow())
	{
		ColumnInfo* pCol = GetAlternativeColumnForEditing(GetCurrColumn(), GetDBT()->GetCurrentRow());
		if (pCol)
			bDo = (nIDC == pCol->GetCtrlID());
	}

	if (bDo)
	{
		if (nHitState == WM_KEYDOWN)
			return DoKeyDown(nKey);

		return DoKeyUp(nKey);
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
LRESULT CBodyEdit::OnBadValue(WPARAM nCtrlID, LPARAM nMode)
{
	if (m_nCurrRecordIdx == -1 || !m_pDBT)
		return 99L;

	if (nCtrlID)
	{
		// Messaggio inviato da un control del body: viene chiesta alla parent
		// form l'autorizzazione gestire il messaggio di errore, dicendo pero` che
		// non e` immediato in modo che la CParsedForm::DoBadValue non faccia nessuna
		// messaggistica
		//
		CParsedForm* pForm = dynamic_cast<CParsedForm*>(GetParent());
		if (pForm && pForm->GetFormAncestor()->SendMessage(UM_BAD_VALUE, GetDlgCtrlID(), nMode & ~CTRL_IMMEDIATE_NOTIFY) == 99L)
		{
			if (m_ColumnsInfo[m_nCurrColumnIdx]->m_bUseOtherColumnCtrl)
				GetAlternativeColumnForEditing(m_ColumnsInfo[m_nCurrColumnIdx], GetDBT()->GetCurrentRow())->ReattachBaseData();
			else
				m_ColumnsInfo[m_nCurrColumnIdx]->ReattachBaseData();

			return 99L;
		}
	}
	else
		if ((nMode & ON_BODY_ROW_CHANGED_MESSAGE) == ON_BODY_ROW_CHANGED_MESSAGE)
		{
			// mi sono automandatato un messaggio speciale su errore per cambio riga
			// (vedi DoMessageOnRecordChanged)

			m_pDBT->m_bDBTErrorPending = FALSE;

			UINT nMsgBoxStyle = (nMode & ON_BODY_ROW_CHANGED_WARNING) == ON_BODY_ROW_CHANGED_WARNING
				? MB_ICONEXCLAMATION
				: MB_ICONSTOP;

			// In questo caso la diagnostica deve lasciare lo stato del body
			// invariato quindi se il control e` visibile si lascia che al ritorno
			// dalla MessageBox il fuoco, ritornando al BodyEdit (vedi OnSetFocus)
			// faccia riapparire il control; altrimenti se non e` visibile si segnala
			// che alla OnSetFocus di non far riapparire il control
			BOOL bCtrlVisible = IsCtrlVisible();

			m_bSetFocusOnly = !bCtrlVisible;

			m_pDBT->ConditionalDisplayMessage(m_pDBT->m_strDBTError, MB_OK | nMsgBoxStyle);

			m_bSetFocusOnly = FALSE;

			if (!bCtrlVisible && m_bBodyFocused)
			{
				// dato che la OnSetFocus non lavorato totalmente
				// si forza il ritorno del BodyEdit allo stato "grafico"
				// corretto
				DrawCarret(m_nCurrRecordIdx);
				EnableButtons();
			}

			return 99L;
		}

	if (m_nNotValidCellCol == -1 && m_nNotValidCellRec == -1)
	{
		m_nNotValidCellCol = m_nCurrColumnIdx;
		m_nNotValidCellRec = m_nCurrRecordIdx;
	}

	// Deve essere effettuata una immediata diagnostica ?
	//
	if ((nMode & CTRL_IMMEDIATE_NOTIFY) != CTRL_IMMEDIATE_NOTIFY)
		return 99L;

	m_nCurrColumnIdx = m_nNotValidCellCol;
	SetCurrRecord(m_nNotValidCellRec, FALSE);

	if ((nMode & CTRL_FOCUS_LOSE_REJECTED) == CTRL_FOCUS_LOSE_REJECTED)
	{
		// Viene gestita la messaggistica specifica del DBT provocata dalla
		// CheckRecords fallita nella OnLosingFocus

		// riabilita il riempimento della stringa di messaggio
		m_pDBT->m_bDBTErrorPending = FALSE;

		if ((nMode & CTRL_FATAL_ERROR) == CTRL_FATAL_ERROR)
		{
			// E` un errore
			m_pDBT->ConditionalDisplayMessage(m_pDBT->m_strDBTError, MB_OK | MB_ICONSTOP);

			// Si provoca il riposizionamento del control
			SetFocus();

			return 99L;
		}

		// E` un warning
		//@@TODO: 	Ho aggiunto una variabile membro m_bCancelOnWarning alla classe
		//			CBaseDocument nel file genlib\basedoc.h e di conseguenza un ulteriore 
		//			parametro booleano (inizializzato per default a FALSE) alla funzione  
		//			SetWarning di CAbstractFormDoc nel file ExtDoc.h per risolvere (tapullare...),  
		//			almeno temporaneamente, un problema urgente che aveva Gianfranco  
		//			sulla gestione dei fuochi ed il relativo ripristino del valore
		//			precedente nel caso in cui si prema CANCEL.
		UINT nMsgBoxStyle = MB_ICONEXCLAMATION;
		nMsgBoxStyle |= AfxGetBaseApp()->IsNoCancOnWarning() ? MB_OK : MB_OKCANCEL;

		BOOL bShow = !m_pDBT->m_pDocument || !m_pDBT->m_pDocument->IsInUnattendedMode();
		BOOL bOK = bShow ?
			(AfxMessageBox(m_pDBT->m_strDBTError, nMsgBoxStyle) == IDOK) :
			AfxGetBaseApp()->IsNoCancOnWarning();

		if (bOK)
		{
			// Nel caso in cui l'anomalia sia stata accettata dall'utente
			// si conclude l'azione sospesa dalla fallimento della CheckRecords
			// nella CBodyEdit::OnBadValue
			CParsedForm* pForm = dynamic_cast<CParsedForm*>(GetParent());
			if (pForm)
				pForm->GetFormAncestor()->SendMessage(UM_LOSING_FOCUS, GetDlgCtrlID(), 0);
			m_nNotValidCellCol = -1;
			m_nNotValidCellRec = -1;
			m_bBodyFocused = FALSE;
			m_bBodyActive = FALSE;
			DrawCarret(m_nCurrRecordIdx);
			EnableButtons();

			return 99L;
		}

		// Si provoca il riposizionamento del control
		SetFocus();
		return 99L;
	}

	// Normale diagnostica legata alla invalidazione del dato digitato nel control
	if (m_nNotValidCellCol < 0)
		return 99L;

	CParsedCtrl* aCtrl = (m_ColumnsInfo[m_nNotValidCellCol]->GetParsedCtrl());
	if (!aCtrl)
		return 99L;

	if (aCtrl->ErrorMessage())
	{
		if (!IsTBWindowVisible(aCtrl->GetCtrlCWnd()))
			SetFocus();

		return 99L;
	}

	CWnd* pWnd = GetFocus();
	if (pWnd == NULL || IsChild(pWnd))
		SetFocus();

	return 99L;
}

//-----------------------------------------------------------------------------
LRESULT CBodyEdit::OnFormatStyleChange(WPARAM nCtrlID, LPARAM nMode)
{
	InvalidateBody();
	return 0L;
}
// gestisce il messaggio di perdita di fuoco inviato da uno dei suoi controls
//--------------------------------------------------------------------------
LRESULT CBodyEdit::OnLosingFocus(WPARAM wParam, LPARAM lParam)
{
	//se sto validando la cella corrente prima di passare alla successiva,
	//potrebbe succedere che una message box o una dialog scateni la perdita di fuoco
	//che pero' non deve essere presa in considerazione, perche non e detto che il fuoco debba effettivamente
	//uscire dalla cella (potrebbe essere rimesso li' in caso di errori (es. chiamate a SetError)
	//si veda an. #13250
	if (m_bValidatingCell)
		return 0;

	// In questo modo si segnala alla parent di resettare lo stato di errore
	//
	CParsedForm* pForm = dynamic_cast<CParsedForm*>(GetParent());
	if (pForm)
		pForm->GetFormAncestor()->SendMessage(UM_LOSING_FOCUS, GetDlgCtrlID(), lParam);

	m_nNotValidCellCol = -1;
	m_nNotValidCellRec = -1;

	int nRelationship = (int)lParam;

	if (wParam == 0)
	{
		// Il messaggio e` stato inviato non su perdita di fuoco (vedi per es. TabCore.cpp):
		// quindi per spegnere il control si da il fuoco al body stesso
		SetFocusOnly();
	}
	else
	{
		ColumnInfo* pCol = GetColumn((UINT)wParam);
		if (pCol)
			pCol->HideCtrl();
	}

	if (lParam == 0)
		return (LRESULT)0;

	if (nRelationship != BROTHER_FOCUSED && nRelationship != PARENT_FOCUSED)
		m_wKeyState = 0;

	// Se il fuoco sta andando fuori da body si da` la possibilita` di verificare
	// la congruenza di tutte le righe

	if (nRelationship == RELATIVE_FOCUSED)
	{
		// @@TODO: un giorno la CheckRecords potrebbe settare una condizione di warning:
		// la OnBadValue e` gia` predisposta per gestirlo
		//
		if (!CheckRecords(FALSE))
			return MAKELRESULT(GetCurrColumn()->GetCtrlID(), CTRL_FATAL_ERROR);

		// se il control sta dando il fuoco ad una window che appartiene alla
		// stessa view si perde il fuoco
		m_bBodyFocused = FALSE;
	}

	// se il control sta dando il fuoco ad una window che non e` suo parente
	// diretto (un control di una colonna (fratello) oppure al bodyedit stesso
	// (padre del control) allora si perde l'attivazione
	if (nRelationship != BROTHER_FOCUSED && nRelationship != PARENT_FOCUSED)
	{
		m_bBodyActive = FALSE;
		DrawCarret(m_nCurrRecordIdx);
	}

	EnableButtons();

	return (LRESULT)0;
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	if (nState < 1000)
	{
		TRACE0("CBodyEdit::OnActivate()\n");
		CGridControl::OnActivate(nState, pWndOther, bMinimized);
		return;
	}

	ActivateBody(nState);
}

//-----------------------------------------------------------------------------
LRESULT CBodyEdit::OnValueChanged(WPARAM wParam, LPARAM lParam)
{
	LRESULT lResult = GetParent()->SendMessage(UM_VALUE_CHANGED, wParam, lParam);

	// in risposta al messaggio potrebbero essere stati modificati
	// gli stati di abilitazione dei bottoni per la corrente riga	
	EnableButtons();

	return lResult;
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetFocusOnly()
{
	m_bSetFocusOnly = TRUE;

	// il messaggio di SetFocus non viene inviato se la window ha gia` il fuoco
	//
	CWnd* pFocus = CWnd::GetFocus();
	if (pFocus && pFocus->m_hWnd == m_hWnd)
		OnSetFocus(NULL);
	else
		SetFocus();
}

//-----------------------------------------------------------------------------
void CBodyEdit::ActivateBody(UINT nMode)
{
	switch (nMode)
	{
	case SET_BODY_FOCUS_ONLY:
	{
		SetFocusOnly();

		break;
	}
	case SET_BODY_CURR_ROW:
	{
		if (m_nCurrRecordIdx < 0 && (!IsReadOnly() || (m_pDBT && !m_pDBT->IsEmpty())))
		{
			m_nStartRecordIdx = 0;

			SetCurrRecord(0);
			SetNextEditableColumnFrom(0, -1);

			m_nOldStartRecordIdx = m_nStartRecordIdx;

			if (m_nCurrRecordIdx >= 0)
				InvalidateBody();
		}
		break;
	}
	}
}

// gestisce eventuali messaggistiche effettuate tramite le
// CBaseApp::SetError o CBaseApp::SetWarning
//-----------------------------------------------------------------------------
void CBodyEdit::DoMessageOnRecordChanged()
{
	CString	strError = AfxGetBaseApp()->GetError();
	BOOL bError = AfxGetBaseApp()->ErrorFound();

	CString	strWarning = AfxGetBaseApp()->GetWarning();
	BOOL bWarning = AfxGetBaseApp()->WarningFound();

	if (!bError && !bWarning)
		return;

	// questa resetta anche la stringa di errore
	AfxGetBaseApp()->ClearMessages();

	if (m_pDBT->m_bDBTErrorPending)
		return;

	LPARAM	nMsgStyle;

	nMsgStyle = bWarning ? ON_BODY_ROW_CHANGED_WARNING : ON_BODY_ROW_CHANGED_ERROR;

	m_pDBT->SetError(bWarning ? strWarning : strError);

	m_pDBT->m_bDBTErrorPending = TRUE;
	PostMessage(UM_BAD_VALUE, 0, nMsgStyle);
}

// -----------------------------------------------------------------------------
void CBodyEdit::UpdateBodyStatus()
{
	if (m_pDBT)
	{
		BOOL bNoEdit = FALSE;
		if
			(
				m_pDBT->GetDocument()->GetFormMode() == CBaseDocument::EDIT &&
				!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EDIT)
				)
		{
			bNoEdit = TRUE;
			m_pDBT->SetDataOSLReadOnly();
		}

		for (int nColumnIdx = 0; nColumnIdx < m_ColumnsInfo.GetSize(); nColumnIdx++)
		{
			ColumnInfo* pCol = m_ColumnsInfo[nColumnIdx];
			ASSERT_VALID(pCol->m_pBaseDataObj);
			CParsedCtrl* pCtrl = pCol->GetParsedCtrl();
			if (pCtrl)
			{
				pCtrl->ReadPropertiesFromJson();
				if
					(
						m_pDBT->GetDocument()->GetFormMode() == CBaseDocument::NEW &&
						!OSL_CAN_DO(pCol->GetInfoOSL(), OSL_GRANT_NEW)
						)
					pCtrl->SetDataOSLReadOnly();

				if
					(
						m_pDBT->GetDocument()->GetFormMode() == CBaseDocument::EDIT &&
						!OSL_CAN_DO(pCol->GetInfoOSL(), OSL_GRANT_EDIT)
						)
				{
					if (!bNoEdit)
						m_pDBT->SetDataOSLReadOnly(TRUE, pCol->m_nDataInfoIdx);

					if (!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BE_ADDROW))
						pCtrl->SetDataOSLReadOnly();
				}
			}

			if (
				nColumnIdx == m_nCurrColumnIdx && IsCtrlVisible() &&
				(
					pCol->m_pBaseDataObj->IsReadOnly() ||
					(pCol->m_pParsedCtrl->GetCtrlData() && pCol->m_pParsedCtrl->GetCtrlData()->IsReadOnly()) ||
					pCol->IsReadOnly(NULL)
					)
				)
			{
				// per spegnere il control si da il fuoco al bodyedit
				PostMessage(WM_ACTIVATE, (WPARAM)SET_BODY_FOCUS_ONLY, MAKELPARAM(m_hWnd, 0));
			}
		}
	}
	//----
	BOOL bReadOnly = !m_pDBT || m_pDBT->IsReadOnly();
	//if (!bAllReadOnly)
	m_bAllColumnsReadOnly = CheckAllColumnsReadOnly();

	// aggiorna la variabile di appoggio interna sulla base
	// del dbt o dello stato delle colonne
	SetBodyReadOnly(bReadOnly);
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CheckAllColumnsReadOnly()
{
	for (int i = 0; i < m_ColumnsInfo.GetSize(); i++)
	{
		if (!m_ColumnsInfo[i]->IsReadOnly(NULL))
			return FALSE;
	}
	return m_ColumnsInfo.GetSize() > 0;
}

//@@TODO i due parametri servirebbero per invalidare lo stretto necessario!!
//-----------------------------------------------------------------------------
void CBodyEdit::UpdateCtrlBody(UINT /* nColumnIDC  = -1 */, int /* nRow  = -1 */)
{
	if (!m_pDBT)
		return;

	// viene rifiutato l'update (per esempio nella DeleteRecord
	if (m_bRejectUpdate == UPDATE_BODY_REJECT_ALL)
		return;

	BOOL bFormModeChanged = GetDocument() && m_PrevFormMode != GetDocument()->GetFormMode();
	if (GetDocument())
		m_PrevFormMode = GetDocument()->GetFormMode();

	//Tentativo di soluzione anomalia magic link righe descrizione
	//Viene settata la riga corrente prendendola dal DBT solo se siamo in UnattendedMode per MagicLink
	//lo faccio anche se sono in attended mode per easybuilder
	//altrimenti io imposto una current row sul dbt a livello di onjectmodel, chiamo una updatedataview 
	//e il bodyedit mi rimette la sua; i calcoli fatti dal documento risultano pertanto sballati
	//an. 20712 per easybuilder, l'aggiunta di righe dal codice sposta 
	//la row del dbt ma non del bodyedit, questa riga di codice la allinea
	m_nCurrRecordIdx = m_pDBT->m_nCurrentRow;
	if (m_nRecordIdxToFocus > -1)
		m_nRecordIdxToFocus = m_nCurrRecordIdx;

	// il control potrebbe essere visibile in una situazione pero` di corpo modificato
	// (per esempio da una cancellazione dei record da programma: caso per esempio
	// della cancellazione del intero documento in modo EDIT), quindi in tal caso 
	// poiche` non e` detto che si mandi prima una AbortForm ci si premunisce da tale
	// situazione anomala
	if (IsCtrlVisible())
	{
		if (!bFormModeChanged && m_nCurrRecordIdx >= 0 && m_nCurrRecordIdx <= m_pDBT->GetUpperBound())
		{
			if (m_pRowFormView)
				RebuildLinks(m_nCurrRecordIdx, TRUE);

			if (m_ColumnsInfo[m_nCurrColumnIdx]->m_pParsedCtrl->ForceUpdateCtrlView(m_nCurrRecordIdx))
				m_ColumnsInfo[m_nCurrColumnIdx]->m_pParsedCtrl->OnUpdateCtrlStatus(m_nCurrRecordIdx);

			m_ColumnsInfo[m_nCurrColumnIdx]->m_pParsedCtrl->UpdateCtrlView();

			SetVScrollRange();
			UpdateRowIndicator();
			InvalidateBody();
			return;
		}
		else
		{
			// visto che la posizione non e` piu` valida si spegne il control dando il fuoco
			// al BodyEdit stesso
			m_ColumnsInfo[m_nCurrColumnIdx]->ReattachBaseData();
			PostMessage(WM_ACTIVATE, (WPARAM)SET_BODY_FOCUS_ONLY, MAKELPARAM(m_hWnd, 0));
		}
	}

	if (m_pDBT->IsEmpty())
	{
		ResetBodyStatus();
		return;
	}

	if (BEGetExStyle(BE_STYLE_ALLOW_SORT) && GetDocument() && !GetDocument()->m_bBatch)
	{
		if
			(
			(
				BEGetExStyle(BE_STYLE_APPLY_SORT_ON_BROWSE)
				&&
				(GetDocument()->GetFormMode() == CBaseDocument::BROWSE)
				)
				||
				(
					BEGetExStyle(BE_STYLE_ALLOW_SORT_ON_EDIT)
					&&
					(GetDocument()->GetFormMode() == CBaseDocument::EDIT || GetDocument()->GetFormMode() == CBaseDocument::NEW)
					)
				)
		{
			Sort();
		}
		else if (!m_bOrdering)
		{
			//se sto browsando i documenti con i pulsanti in toolbar o con il radar pinned
			//un sort automatico che provoca il caricamento completo dei DBT puo' essere troppo oneroso
			ClearOrderingList();
		}
	}

	if (m_bRejectUpdate == UPDATE_BODY_REJECT_ROW_MOVE)
	{
		InvalidateBody();
		return;
	}

	int nNewCurrRec = m_nCurrRecordIdx;

	if (bFormModeChanged || GetDocument()->GetFormMode() == CBaseDocument::BROWSE)
	{
		if (m_nCurrRecordIdx >= 0 || m_bBodyFocused)
		{
			// Il DBT si e` riposizionato correttamente

			/*
			// Anomalia N° 21870 Click su immagine in griglia porta la riga in ultima posizione
			nNewCurrRec				= m_pDBT->GetCurrentRowIdx();
			m_nStartRecordIdx		= nNewCurrRec - m_nLastBodyRow;
			*/
			m_nOldStartRecordIdx = -1;
			if (m_nStartRecordIdx < 0) m_nStartRecordIdx = 0;
			if (m_nCurrColumnIdx < 0) m_nCurrColumnIdx = 0;
		}
		else
		{
			m_nStartRecordIdx = 0;
			m_nOldStartRecordIdx = -1;
		}
	}
	else
	{
		if (m_nCurrRecordIdx > m_pDBT->GetUpperBound())
		{
			nNewCurrRec = m_pDBT->GetUpperBound();
			m_nStartRecordIdx = nNewCurrRec - m_nLastBodyRow;
			if (m_nStartRecordIdx < 0) m_nStartRecordIdx = 0;
			m_nOldStartRecordIdx = -1;
		}
	}

	SetVScrollRange();

	// non fa fare la notifica di ROW_CHANGED per evitare ricorsivita` indotte
	// da invalidazioni effettuate dal programmatore
	SetCurrRecord(nNewCurrRec, FALSE);

	ResetToVisiblePosition();

	InvalidateBody();
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::DoKeyDown(UINT nKey)
{
	if (!__super::DoKeyDown(nKey, FALSE))
		return FALSE;

	if (!IsCtrlVisible() && !IsReadOnly())
		ShowCtrl(ResetToVisiblePosition());

	return DoMovingKey(nKey);
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::DoMovingKey(UINT nChar)
{
	BOOL shiftDown;
	BOOL ctrlDown;
	BOOL altDown;
	GetKeyDownState(shiftDown, ctrlDown, altDown);

	if (ctrlDown && altDown && BEGetExStyle(BE_STYLE_MULTIPLE_SEL_ACCELERATOR_S))
	{
		//Collide con i dataentry esistenti di Mago
		switch (nChar)
		{
		case 'S':
		case 's':
			if (BEGetExStyle(BE_STYLE_ALLOW_MULTIPLE_SEL))
			{
				return TRUE;
			}
		}
	}
	//----------------------------------

	// in stato readonly accetta anche l'uso dei tasti funzione senza <CTRL>
	if (ctrlDown)
	{
		switch (nChar)
		{
		case _T('U'):
		case _T('u'): if (altDown) return FALSE;// NOT EATEN !!!
		case VK_UP:	DoMoveToPrevRow(TRUE);	return TRUE;
		case _T('D'):
		case _T('d'): if (altDown) return FALSE;// NOT EATEN !!!
		case VK_DOWN:	DoMoveToNextRow(TRUE);	return TRUE;
		case VK_LEFT:	DoMoveToPrevCol();		return TRUE;
		case VK_RIGHT:	DoMoveToNextCol();		return TRUE;
		case VK_PRIOR: 	if (shiftDown)	DoPageLeft();		else DoPageUp();			return TRUE;
		case VK_NEXT:	if (shiftDown)	DoPageRight();		else DoPageDown();			return TRUE;
		case VK_HOME:	if (shiftDown)	DoMoveToFirstCol();	else DoMoveToFirstRow(TRUE); return TRUE;
		case VK_END:	if (shiftDown)	DoMoveToLastCol();	else DoMoveToLastRow(TRUE);	return TRUE;
		case VK_INSERT:	if (shiftDown)
		{
			PostMessage(WM_COMMAND,
				(this->IsKindOf(RUNTIME_CLASS(CTreeBodyEdit)) ?
					ID_BE_TREE_INSERT : IDC_BE_INSERT), (LPARAM)m_hWnd);
			return TRUE;
		}
						if (altDown)
						{
							PostMessage(WM_COMMAND, ID_BE_TREE_INSERTCHILD, (LPARAM)m_hWnd);
							return TRUE;
						}
						break;
		case VK_F11:	DoDownExit();
			break;

		case VK_DELETE:	if (shiftDown) { PostMessage(WM_COMMAND, IDC_BE_DELETE, (LPARAM)m_hWnd);	return TRUE; }				break;
		}
	}
	else
	{
		switch (nChar)
		{
		case VK_F11:
			if (shiftDown)
			{
				DoUpExit();
				return TRUE;
			}

			return FALSE;

		case VK_F12:	CallDialog();												return TRUE;
		case VK_TAB:	if (shiftDown) DoMoveToPrevCtrl(); else	DoMoveToNextCtrl();	return TRUE;
		}

		if (IsReadOnly() || !IsCtrlVisible())
		{
			switch (nChar)
			{
			case VK_UP:	DoMoveToPrevRow(TRUE);	return TRUE;
			case VK_DOWN:	DoMoveToNextRow(TRUE);	return TRUE;
			case VK_LEFT:	DoMoveToPrevCol();		return TRUE;
			case VK_RIGHT:	DoMoveToNextCol();		return TRUE;
			case VK_PRIOR: 	if (shiftDown)	DoPageLeft();		else DoPageUp();			return TRUE;
			case VK_NEXT:	if (shiftDown)	DoPageRight();		else DoPageDown();			return TRUE;
			case VK_HOME:	if (shiftDown)	DoMoveToFirstCol();	else DoMoveToFirstRow(TRUE); return TRUE;
			case VK_END:	if (shiftDown)	DoMoveToLastCol();	else DoMoveToLastRow(TRUE);	return TRUE;
			}
		}
	}

	m_wKeyState = 0;

	return FALSE;	// NOT EATEN !!!
}

//-----------------------------------------------------------------------------
void CBodyEdit::NotifyToParent(UINT mess)
{
	GetParent()->SEND_WM_COMMAND(GetDlgCtrlID(), mess, m_hWnd);
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetBodyReadOnly(BOOL bReadOnly)
{
	if (GetMultiSelMode() == MULTIPLE_SEL) bReadOnly = TRUE;

	if (!bReadOnly && m_pDBT && m_pDBT->GetDocument())
	{
		if (m_pDBT->GetDocument()->GetFormMode() == CBaseDocument::NEW)
			bReadOnly =
			!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_NEW) &&
			!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BE_ADDROW) &&
			!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BE_DELETEROW);
		else if (m_pDBT->GetDocument()->GetFormMode() == CBaseDocument::EDIT)
			bReadOnly =
			!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EDIT) &&
			!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BE_ADDROW) &&
			!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BE_DELETEROW);
		//TODO batch e finder ?
	}

	//if (IsReadOnly() == bReadOnly)
	//	return;

	if (IsCtrlVisible() && bReadOnly)
	{
		m_ColumnsInfo[m_nCurrColumnIdx]->ReattachBaseData();
		PostMessage(WM_ACTIVATE, (WPARAM)SET_BODY_FOCUS_ONLY, MAKELPARAM(m_hWnd, 0));
	}

	m_bInternalReadOnly = bReadOnly;

	EnableButtons();
}

//-----------------------------------------------------------------------------
void CBodyEdit::EnableButtons()
{
	if (!GetDocument())
		return;

	// insert non soggetto a riga corrente
	BOOL bEnable = !m_bInternalReadOnly;
	BOOL b = bEnable && OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BE_ADDROW) && m_bInsertRowEnabled;

	//if (m_pBtnInsert && m_pBtnInsert->m_hWnd)
	//	m_pBtnInsert->EnableWindow(b);
	//else 

	EnableButton(IDC_BE_INSERT, b);

	// i restanti bottoni sono soggetti anche  a riga corrente
	bEnable = !m_bInternalReadOnly && m_nCurrRecordIdx >= 0;
	b = bEnable && OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BE_DELETEROW) && m_bDeleteRowEnabled;

	//if (m_pBtnDelete && m_pBtnDelete->m_hWnd)
	//	m_pBtnDelete->EnableWindow(b);
	//else 
	EnableButton(IDC_BE_DELETE, b);

	b = OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BE_SHOWROWVIEW) && m_bFormViewCallEnabled && m_pFormViewClass;

	//if (m_pBtnCallDialog && m_pBtnCallDialog->m_hWnd)
	//	m_pBtnCallDialog->EnableWindow(b);
	//else 
	EnableButton(IDC_BE_CALLDIALOG, b);

	//if (m_pBtnSearch && m_pBtnSearch->m_hWnd)
	//	m_pBtnSearch->EnableWindow(BEGetExStyle(BE_STYLE_ALLOW_SEARCH));
	//else 
	EnableButton(IDC_BE_SEARCH, BEGetExStyle(BE_STYLE_ALLOW_SEARCH));

	//if (m_pBtnMultiSelection && m_pBtnMultiSelection->m_hWnd)
	//	m_pBtnMultiSelection->EnableWindow(BEGetExStyle(BE_STYLE_ALLOW_MULTIPLE_SEL));
	//else 
	EnableButton(IDC_BE_MULTISELECTION, BEGetExStyle(BE_STYLE_ALLOW_MULTIPLE_SEL));

	// Se esiste una RowFormView aperta allora deve essere abilitata/disabilitata
	if (m_pRowFormView)
		m_pRowFormView->EnableWindow(m_bFormViewCallEnabled /*&& bEnable*/);

	/*
		for (int i = 0; i <= m_UserButtons.GetUpperBound(); i++)
		{
		CBodyBitmapButton* pBtn = (CBodyBitmapButton*)m_UserButtons[i];
		if (!pBtn->m_hWnd)
		continue;

		BOOL bE =
		bEnable
		&&
		OSL_CAN_DO(pBtn->GetInfoOSL(), OSL_GRANT_EXECUTE) != 0
		&&
		(
		(GetDocument()->GetFormMode() == CBaseDocument::NEW && OSL_CAN_DO(pBtn->GetInfoOSL(), OSL_GRANT_NEW) != 0)
		||
		(GetDocument()->GetFormMode() == CBaseDocument::EDIT && OSL_CAN_DO(pBtn->GetInfoOSL(), OSL_GRANT_EDIT) != 0)
		||
		(GetDocument()->GetType() == VMT_BATCH)
		||
		(GetDocument()->GetType() == VMT_FINDER)
		);

		pBtn->EnableButton(bE);
		}
		*/
	m_HeaderToolBar.EnableButtons(!m_bInternalReadOnly);
	m_FooterToolBar.EnableButtons(!m_bInternalReadOnly);
}

//-----------------------------------------------------------------------------
ColumnInfo*	CBodyEdit::GetColumnFromDataIdx(int nDataIdx)
{
	for (int nColumnIdx = 0; nColumnIdx < m_AllColumnsInfo.GetSize(); nColumnIdx++)
		if (m_AllColumnsInfo[nColumnIdx]->m_nDataInfoIdx == nDataIdx)
			return m_AllColumnsInfo[nColumnIdx];

	return NULL;
}

//-----------------------------------------------------------------------------
ColumnInfo*	CBodyEdit::GetVisibleColumnFromDataIdx(int nDataIdx)
{
	for (int nColumnIdx = 0; nColumnIdx < m_ColumnsInfo.GetSize(); nColumnIdx++)
		if (m_ColumnsInfo[nColumnIdx]->m_nDataInfoIdx == nDataIdx)
			return m_ColumnsInfo[nColumnIdx];

	return NULL;
}

//-----------------------------------------------------------------------------
ColumnInfo*	CBodyEdit::GetColumn(UINT nCtrlID)
{
	ColumnInfo* pInfo = NULL;
	for (int nColumnIdx = 0; nColumnIdx < m_AllColumnsInfo.GetSize(); nColumnIdx++)
	{
		pInfo = m_AllColumnsInfo[nColumnIdx];
		if (pInfo->m_nCtrlIDC == nCtrlID)
			return pInfo;

	}
	return NULL;
}

//-----------------------------------------------------------------------------
ColumnInfo*	CBodyEdit::GetColumn(const CString& sName)
{
	ColumnInfo* pInfo = NULL;
	for (int nColumnIdx = 0; nColumnIdx < m_AllColumnsInfo.GetSize(); nColumnIdx++)
	{
		pInfo = m_AllColumnsInfo[nColumnIdx];
		if (pInfo->GetNamespace().GetObjectName() == sName)
			return pInfo;
	}
	return NULL;
}
//-----------------------------------------------------------------------------
ColumnInfo*	CBodyEdit::GetVisibleColumn(UINT nCtrlID)
{
	for (int nColumnIdx = 0; nColumnIdx < m_ColumnsInfo.GetSize(); nColumnIdx++)
		if (m_ColumnsInfo[nColumnIdx]->m_nCtrlIDC == nCtrlID)
			return m_ColumnsInfo[nColumnIdx];

	return NULL;
}

//-----------------------------------------------------------------------------
int	CBodyEdit::GetVisibleColumnIdx(UINT nCtrlID)
{
	for (int nColumnIdx = 0; nColumnIdx < m_ColumnsInfo.GetSize(); nColumnIdx++)
		if (m_ColumnsInfo[nColumnIdx]->m_nCtrlIDC == nCtrlID)
			return nColumnIdx;

	return -1;
}

//-----------------------------------------------------------------------------
int	CBodyEdit::GetAllColumnIdx(UINT nCtrlID)
{
	for (int nColumnIdx = 0; nColumnIdx < m_AllColumnsInfo.GetSize(); nColumnIdx++)
		if (m_AllColumnsInfo[nColumnIdx]->m_nCtrlIDC == nCtrlID)
			return nColumnIdx;

	return -1;
}

//------------------------------------------------------------------------------
int CBodyEdit::GetPrevPageStartColumnIdx()
{
	int nStartIdx = 0;
	for (nStartIdx = 0; nStartIdx < m_ColumnsInfo.GetSize(); nStartIdx++)
		if ((m_LastColsIdx[nStartIdx] & ~FLAG_END_COL_NOT_FULL_VISIBLE) == (WORD)m_nStartColumnIdx)
			break;

	if (nStartIdx == m_ColumnsInfo.GetSize())
		return 0;

	if (nStartIdx > 0 && IsEndColFullVisible(nStartIdx - 1))
		nStartIdx--;

	return nStartIdx;
}

//------------------------------------------------------------------------------
int CBodyEdit::GetNextPageStartColumnIdx()
{
	int nStart = IsEndColFullVisible(GetLogicalStartColumnIdx()) ? GetEndColumnIdx() + 1 : GetEndColumnIdx();
	int nStartForLast = GetStartIdxForLastColumn();
	if (nStart > nStartForLast || GetEndColumnIdx() == m_ColumnsInfo.GetUpperBound())
		return nStartForLast;

	return nStart;
}

//------------------------------------------------------------------------------
void CBodyEdit::ScrollToShowUnlockedHiddenColumns()
{
	if (m_ColumnsInfo.GetCount() == 0 || m_nCurrColumnIdx < 0)
		return;

	ASSERT(m_nStartColumnIdx >= 0);
	ASSERT(m_nCurrColumnIdx >= 0);

	ColumnInfo* pCol = m_ColumnsInfo[m_nCurrColumnIdx];
	ASSERT_VALID(pCol);

	if (!pCol->m_bVisible)
	{
		int offset = GetFirstUnlockedVisibleColumn() - m_nCurrColumnIdx;
		if (m_nStartColumnIdx >= offset)
			DoScrollHoriz(m_nStartColumnIdx - offset);
	}
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::SetNextEditableColumnFrom(int nStart, int nCol)
{
	if (m_ColumnsInfo.GetCount() == 0)
		return FALSE;
	BOOL bRedraw = m_nStartColumnIdx != nStart;

	DoScrollHoriz(nStart);

	int nPrevCol = m_nCurrColumnIdx;
	m_nCurrColumnIdx = GetNextEditableColumnFrom(nCol);
	if (m_nCurrColumnIdx < 0)
		return FALSE;
	if (m_nCurrColumnIdx >= 0)
		ScrollToShowUnlockedHiddenColumns();

	// attenzione! In caso di diagnostica non si spegne la colonna precedente bene
	// questa riga serve per correggere l'anomalia #9914
	if (nPrevCol >= 0 && nPrevCol <= m_nCurrColumnIdx && m_ColumnsInfo[nPrevCol]->GetParsedCtrl())
		m_ColumnsInfo[nPrevCol]->HideCtrl();

	bRedraw = AdjustScrollPosition() || bRedraw;


	if (bRedraw)
		UpdateHScrollPos();

	return bRedraw;
}

//------------------------------------------------------------------------------
int CBodyEdit::GetNextEditableColumnFrom(int nCol)
{
	if (IsReadOnly())
	{
		if (nCol < m_ColumnsInfo.GetUpperBound())
			return nCol + 1;

		return nCol;
	}

	for (int i = nCol + 1; i <= m_ColumnsInfo.GetUpperBound(); i++)
		if (!m_ColumnsInfo[i]->IsReadOnly(GetCurrRecord()))
			return i;

	if (nCol == -1)
		return 0;

	return -1;
}

//------------------------------------------------------------------------------
int CBodyEdit::GetPrevEditableColumnFrom(int nCol)
{
	if (IsReadOnly())
	{
		if (nCol > 0)
			return nCol - 1;

		return nCol;
	}

	for (int i = nCol - 1; i >= 0; i--)
		if (!m_ColumnsInfo[i]->IsReadOnly(GetCurrRecord()))
			return i;

	if (nCol == m_ColumnsInfo.GetUpperBound() + 1)
		return GetStartIdxForLastColumn();

	return -1;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CanDoMoving(BOOL bUp /*=FALSE*/, BOOL bCheckRow /*=FALSE*/, BOOL bEmitError /*= TRUE*/)
{
	if (IsReadOnly() || !m_pDBT)
		return TRUE;

	if (m_ColumnsInfo.GetCount() > 0 && (m_nCurrColumnIdx < 0 || m_nCurrColumnIdx >  m_ColumnsInfo.GetUpperBound()))
		return TRUE;
	if (m_nCurrRecordIdx < 0 || m_nCurrRecordIdx > m_pDBT->GetUpperBound())
		return TRUE;

	m_bValidatingCell = TRUE; //inibisco la OnLosingFocus, in questa fase il fuoco non deve uscire dalla cella
	BOOL bUpdate = UpdateData(TRUE);

	//se il fuoco e` stato spostato a causa di qualche finestra aperta nella UpdateData (OnChange del campo)
	//lo devo ripristinare
	if (m_nCurrColumnIdx >= 0 && m_nCurrColumnIdx < m_ColumnsInfo.GetCount())
		m_ColumnsInfo[m_nCurrColumnIdx]->m_pParsedCtrl->SetCtrlFocus(FALSE);

	//riabilito la OnLosingFocus
	m_bValidatingCell = FALSE;

	if (bUpdate)
	{
		// give DBTSlaveBuffered a chance to check row data
		if (bCheckRow && m_nCurrRecordIdx > -1 && (!bUp || m_pDBT->GetRow(m_nCurrRecordIdx)->IsStorable()))
			return !SetToBadCell(m_nCurrRecordIdx, m_pDBT->CheckRow(m_nCurrRecordIdx), bEmitError);

		return TRUE;
	}

	//@@	SetFocusOnly();            

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CheckRecords(BOOL bEmitError)
{
	if (!m_pDBT || m_pDBT->IsEmpty() || IsReadOnly() || !m_bBodyModified)
		return TRUE;

	// La CheckRecords valorizza nBadRec e quindi ci si deve appoggiare
	// ad un DataObj* temporaneo per passare il nuovo valore di nBadRec
	// alla SetToBadCell

	int nBadRec;
	DataObj* pBadData = m_pDBT->CheckRecords(nBadRec);

	if (!SetToBadCell(nBadRec, pBadData, bEmitError))
		m_bBodyModified = FALSE;

	return !m_bBodyModified;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::SetToBadCell(int nBadRec, DataObj* pData, BOOL bEmitError)
{
	if (m_pDBT->IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (pData == NULL)
		return FALSE;

	if (nBadRec != m_nCurrRecordIdx)
		SetCurrRecord(nBadRec, FALSE);

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	// lookup data position in the current record	
	SqlRecord* pCurRec = m_pDBT->GetRow(m_nCurrRecordIdx);
	int nDataInfoIdx = pCurRec->Lookup(pData);
	ASSERT(nDataInfoIdx != -1);

	for (m_nCurrColumnIdx = 0; m_nCurrColumnIdx < m_ColumnsInfo.GetSize(); m_nCurrColumnIdx++)
		if (m_ColumnsInfo[m_nCurrColumnIdx]->m_nDataInfoIdx == nDataInfoIdx)
			break;

	if (m_nCurrColumnIdx >= m_ColumnsInfo.GetSize())
	{
		TRACE
		(
			"CBodyEdit::SetToBadCell : the field in the position %d of the record %s, in the row %d, itsn't a BodyEdit %s column\n",
			nDataInfoIdx,
			(LPCTSTR)pCurRec->GetRuntimeClass()->m_lpszClassName,
			m_nCurrRecordIdx,
			(LPCTSTR)GetRuntimeClass()->m_lpszClassName
		);

		m_nCurrColumnIdx = 0;
	}

	if (ResetToVisiblePosition())
		InvalidateBody();

	if (bEmitError)
	{
		m_pDBT->m_bDBTErrorPending = FALSE;
		m_pDBT->ConditionalDisplayMessage(m_pDBT->m_strDBTError, MB_OK | MB_ICONSTOP);

		ShowCtrl(FALSE);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBodyEdit::UpdateVScrollPos()
{
	if (!m_pVScrollBar || !m_pDBT)
		return;

	if (m_nStartRecordIdx > m_pDBT->GetUpperBound())
		m_nStartRecordIdx = 0;

	m_pVScrollBar->SetScrollPos(max(0, m_nStartRecordIdx));

	m_pVScrollBar->EnableWindow(m_pDBT->GetUpperBound() > m_nLastBodyRow || m_nStartRecordIdx > 0);
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetVScrollRange()
{
	if (m_nLayoutSuspended > 0 || !m_pDBT)
		return;

	int nLastPos = m_pDBT->GetUpperBound() - m_nLastBodyRow;
	if (m_bHasFooter)
		nLastPos++;

	if (m_pVScrollBar)
	{
		m_pVScrollBar->SetScrollRange(0, max(m_nLastBodyRow, nLastPos));
	}

	UpdateVScrollPos();

	UpdateFooters();

	ResizeSelections();
}

//-----------------------------------------------------------------------------
void CBodyEdit::UpdateFooters()
{
	if (!m_pDBT || !m_pDBT->GetRecords()->m_bObservablesEnabled || m_nLayoutSuspended)
		return;

	for (int c = 0; c <= m_ColumnsInfo.GetUpperBound(); c++)
	{
		ColumnInfo*		pCol = m_ColumnsInfo[c];
		if (pCol->m_pFooterCalculator)
		{
			pCol->UpdateFooter();
		}
	}
}

//------------------------------------------------------------------------------
void CBodyEdit::SaveStartRecord()
{
	if (m_nCurrRecordIdx >= 0 && m_nOldStartRecordIdx < 0)
		m_nOldStartRecordIdx = m_nStartRecordIdx;
}

//------------------------------------------------------------------------------
void CBodyEdit::SaveStartColumn()
{
	if (m_nCurrColumnIdx >= 0 && m_nOldStartColumnIdx < 0)
		m_nOldStartColumnIdx = m_nStartColumnIdx;
}

//------------------------------------------------------------------------------
void CBodyEdit::UnsaveStartRecord()
{
	if (
		m_nCurrRecordIdx >= m_nStartRecordIdx	&&
		m_nCurrRecordIdx <= GetEndRecordIdx()
		)
		m_nOldStartRecordIdx = -1;
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::IsCurrColFullVisible() const
{
	return 	m_nCurrColumnIdx >= m_nStartColumnIdx &&
		(
			m_nCurrColumnIdx < GetEndColumnIdx() ||
			(
				m_nCurrColumnIdx == GetEndColumnIdx() &&
				IsEndColFullVisible(GetLogicalStartColumnIdx())
				)
			);
}

//------------------------------------------------------------------------------
void CBodyEdit::UnsaveStartColumn()
{
	if (IsCurrColFullVisible())
		m_nOldStartColumnIdx = -1;
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::AdjustScrollPosition()
{
	if (m_nCurrColumnIdx < 0)
		return FALSE;

	BOOL bRedraw = FALSE;
	if (m_nCurrColumnIdx < m_nStartColumnIdx)
	{ //moving backwards
		DoScrollHoriz(PhysicalToLogicalColIdx(m_nCurrColumnIdx));
		UpdateHScrollPos();
		bRedraw = TRUE;
	}
	else if (CurrentColumnBeyondLastFullyVisible())
	{ //moving forwards

		while (CurrentColumnBeyondLastFullyVisible() && CanDoScrollRight())
			DoScrollRight(m_nStartColumnIdx + 1);
		UpdateHScrollPos();
		bRedraw = TRUE;
	}
	return bRedraw;
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::ResetToVisiblePosition()
{
	BOOL bRedraw = FALSE;

	if (m_nOldStartRecordIdx < 0)
		if (m_nCurrRecordIdx < m_nStartRecordIdx)
			m_nOldStartRecordIdx = m_nCurrRecordIdx;
		else
			if (m_nCurrRecordIdx > GetEndRecordIdx())
				m_nOldStartRecordIdx =
				m_nStartRecordIdx + m_nCurrRecordIdx - GetEndRecordIdx();

	if (m_nOldStartRecordIdx >= 0)
	{
		m_nStartRecordIdx = m_nOldStartRecordIdx;
		m_nOldStartRecordIdx = -1;
		UpdateVScrollPos();

		bRedraw = TRUE;
	}

	if (m_nOldStartColumnIdx < 0)
	{
		bRedraw = AdjustScrollPosition();
	}
	if (m_nOldStartColumnIdx >= 0)
	{
		DoScrollHoriz(PhysicalToLogicalColIdx(m_nOldStartColumnIdx));
		m_nOldStartColumnIdx = -1;

		UpdateHScrollPos();

		bRedraw = TRUE;
	}

	return bRedraw;
}

//------------------------------------------------------------------------------
void CBodyEdit::ResetBodyStatus()
{
	m_nStartColumnIdx = 0;
	m_nStartRecordIdx = 0;
	m_nCurrColumnIdx = -1;
	m_nCurrRecordIdx = -1;
	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;
	m_nNotValidCellCol = -1;
	m_nNotValidCellRec = -1;

	// DEVE essere fatta prima della RebuildLinks
	if (m_pDBT)
		m_pDBT->SetCurrentRow(-1);

	if (m_pRowFormView)
		RebuildLinks(-1, TRUE);

	ResetRowIndicator();

	UpdateHScrollPos();
	UpdateVScrollPos();
	InvalidateBody();

	EnableButtons();
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::SetCurrRecord(int nNewRec, BOOL bNotify /* = TRUE */)
{
	if (!m_pDBT || !m_hWnd) return FALSE;

	//cambiare la riga del body mi resetta il dataobj che sto cambiando (se ho avuto il via libera per cambiare riga a monte, significa che ill cambiamento è andato a buon fine)
	//mi serve resettare perché altrimenti la find automatica dell'hotlink non funziona, pensa che il dataobj sia ancora in fase di cambiamento e mi mette una descrizione errata sulla riga 
	AfxGetBaseApp()->SetOldCtrlData(NULL, NULL);
	if (nNewRec > m_pDBT->GetUpperBound())
	{
		if (!m_bAddRowEnabled)
			return FALSE;
		if (m_bInternalReadOnly)
			return FALSE;
		if (!CanDoInsertRow())
			return FALSE;
		if (OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BE_ADDROW) == 0)
			return FALSE;

		SqlRecord* pNewRec = m_pDBT->AddRecord();
		BOOL bOk = pNewRec != NULL;
		if (pNewRec)
		{
			pNewRec->SetInsertedByUI();
			GetDocument()->UpdateDataView();//force FindRecord for HKL descriptions
		}
		DoMessageOnRecordChanged();

		if (!bOk) return FALSE;

		SetVScrollRange();

		// inform document for changed data
		m_bBodyModified = TRUE;
		GetDocument()->SetModifiedFlag();

		if (m_pDBT->IsAllowFilter())
		{
			nNewRec = m_pDBT->GetRecords()->FindPtr(pNewRec);
		}
	}

	m_nCurrRecordIdx = nNewRec;

	if (m_nCurrRecordIdx >= 0 || (GetDocument()->GetXMLDataManager() && GetDocument()->GetXMLDataManager()->GetStatus() == CXMLDataManagerObj::XML_MNG_EXPORTING_DATA))
		m_pDBT->SetCurrentRow(m_nCurrRecordIdx);

	UpdateRowIndicator();

	//	EnableButtons();

	m_bRejectUpdate = UPDATE_BODY_REJECT_ROW_MOVE;

	if (m_nCurrRecordIdx >= 0 && bNotify)
	{
		NotifyToParent(BEN_ROW_CHANGED);
		DoMessageOnRecordChanged();
	}

	// Spostato qui dalla posizione commentata perche' se sul change di riga
	// si cambiava lo stato dei bottoni non venivano refreshati
	EnableButtons();

	if (!m_pDBT)//potrebbe essere stato annullato nel frattempo dalla NotifyToParent
		return FALSE;
	//rinfresca eventuali altri documenti aperti collegati da hyperlink
	SqlRecord* pRec = NULL;
	if (
		m_nCurrRecordIdx >= 0 &&
		!m_pDBT->IsEmpty() &&
		(pRec = m_pDBT->GetRow(m_nCurrRecordIdx)) != NULL
		)
	{
		for (int c = 0; c <= m_ColumnsInfo.GetUpperBound(); c++)
		{
			ColumnInfo*		pCol = m_ColumnsInfo[c];
			if (pCol->HasHyperLink())
			{
				pCol->RefreshHyperLink(pRec);
			}

			pCol->m_pParsedCtrl->m_pSqlRecord = pRec;
		}
	}

	m_bRejectUpdate = UPDATE_BODY_REJECT_NONE;

	if (m_pRowFormView)
	{
		CAbstractFormFrame* pFrame = dynamic_cast<CAbstractFormFrame*>(m_pRowFormView->GetParentFrame());
		if (pFrame)
		{
			pFrame->SetSubTitle(GetRowFormViewTitle());
			pFrame->OnUpdateFrameTitle(TRUE);
		}

		RebuildLinks(m_nCurrRecordIdx, bNotify);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::UpdateData(BOOL bSignalError, BOOL bSendMessage /* = FALSE */)
{
	if (!IsCtrlVisible())
		return TRUE;

	ColumnInfo* pOriginalCol = m_ColumnsInfo[m_nCurrColumnIdx];
	ColumnInfo* pStoledCol = GetAlternativeColumnForEditing(pOriginalCol, GetDBT()->GetCurrentRow());
	ColumnInfo* pToBeUsedCol = pOriginalCol;
	if (pStoledCol)
		pToBeUsedCol = pStoledCol;

	if (
		!pToBeUsedCol->UpdateData(bSignalError, bSendMessage) ||
		pToBeUsedCol->GetParsedCtrl()->GetWarningID()
		)
	{
		m_nNotValidCellCol = m_nCurrColumnIdx;
		m_nNotValidCellRec = m_nCurrRecordIdx;

		return FALSE;
	}

	m_nNotValidCellCol = -1;
	m_nNotValidCellRec = -1;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::ShowCtrl(BOOL bRedraw)
{
	ASSERT_VALID(m_pDBT);

	if (bRedraw)
		InvalidateBody();

	if (IsReadOnly() || m_nCurrColumnIdx < 0 || m_nCurrRecordIdx < 0)
		return FALSE;

	if (m_pDBT->IsEmpty())
	{
		//ASSERT(FALSE);
		TRACE("CBodyEdit::ShowCtrl on empty DBT\n");
		return FALSE;
	}

	SqlRecord* pCurRec = m_pDBT->GetRow(m_nCurrRecordIdx);
	if (pCurRec == NULL)
	{
		//ASSERT(FALSE);
		TRACE("CBodyEdit::ShowCtrl bad internal row index\n");
		return FALSE;
	}

	if (m_nStartColumnIdx >= m_ColumnsInfo.GetSize() || m_nCurrColumnIdx >= m_ColumnsInfo.GetSize())
	{
		//ASSERT(FALSE);
		TRACE("CBodyEdit::ShowCtrl bad internal column index\n");
		return FALSE;
	}

	int xColOffs = m_rectBody.left +
		m_ColumnsInfo[m_nCurrColumnIdx]->m_nHorzColOffset -
		m_ColumnsInfo[GetLogicalStartColumnIdx()]->m_nHorzColOffset +
		1 - CTRL_NOT_INSIDE;

	int yPos = ((m_nCurrRecordIdx - m_nStartRecordIdx) * m_nRowsHeight) + m_rectBody.top - CTRL_NOT_INSIDE;

	CRect rect, rectCol;
	BOOL bAlternative = FALSE;

	ColumnInfo* pOriginalCol = m_ColumnsInfo[m_nCurrColumnIdx];
	CWnd* pOriginalControl = pOriginalCol->GetControl();
	if (pOriginalControl)
		pOriginalControl->GetClientRect(rectCol);
	else
		ASSERT(false); //Non sono riuscito a recuperare il control

	ColumnInfo* pStoledCol = NULL;
	ColumnInfo* pColToBeUse = NULL;
	CWnd* pStoledControl = NULL;
	if (pOriginalCol->m_bUseOtherColumnCtrl)
	{
		pStoledCol = GetAlternativeColumnForEditing(pOriginalCol, GetDBT()->GetCurrentRow());
		pStoledControl = pStoledCol->GetControl();
		ASSERT(pStoledControl);
		if (pStoledCol)
		{
			DataObj* pStoledDataObj = GetDataObjAt(GetDBT()->GetCurrentRow(), pStoledCol);
			bAlternative = TRUE;
			if (pStoledDataObj->IsKindOf(RUNTIME_CLASS(DataBool)))
			{
				rect.left = xColOffs + (rectCol.Width() / 2) - (CHECK_BOX_WIDTH / 2);
				rect.right = xColOffs + (rectCol.Width() / 2) + CHECK_BOX_WIDTH;
			}
			else
			{
				if (pStoledDataObj->IsKindOf(RUNTIME_CLASS(DataDate)))
					rect.right = xColOffs + rectCol.Width() - CALENDAR_BOX_WIDTH;
				else
					rect.right = xColOffs + rectCol.Width();

				rect.left = xColOffs;
			}

			rect.top = yPos;
			rect.bottom = yPos + rectCol.Height();
			pColToBeUse = pStoledCol;
		}
		else
		{
			pColToBeUse = pOriginalCol;
		}
	}
	else
	{
		pColToBeUse = pOriginalCol;
	}

	if (pColToBeUse->IsReadOnly(pCurRec) || !pOriginalCol->m_bVisible)
	{
		SetFocusOnly();
		return FALSE;
	}

	pColToBeUse->GetParsedCtrl()->ForceUpdateCtrlView(m_nCurrRecordIdx);

	if (m_pDataTip)
		m_pDataTip->Hide();

	SetUpAlternativeColumn(pOriginalCol, pColToBeUse);

	if (OnShowCtrl(pCurRec, pColToBeUse, xColOffs, yPos, rect) || bAlternative) //permette di ridimensionare il control, reimplementata da CTreeBodyEdit
		pColToBeUse->ShowCtrl(pCurRec, rect);
	else
		pColToBeUse->ShowCtrl(pCurRec, xColOffs, yPos);

	OnSelCell(pCurRec, pColToBeUse);
	if (m_pDBT->GetDocument())
		m_pDBT->GetDocument()->DispatchOnBESelCell(this, pCurRec, pColToBeUse);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::IsCtrlVisible() const
{
	if (IsReadOnly())
		return FALSE;

	if (m_nCurrColumnIdx < 0 || m_nCurrColumnIdx >= m_ColumnsInfo.GetSize())
		return FALSE;

	ColumnInfo* pCol = m_ColumnsInfo[m_nCurrColumnIdx];
	if (pCol->m_bUseOtherColumnCtrl)
	{
		if (GetDBT() && GetCurrRecord())
		{
			pCol = const_cast<CBodyEdit*>(this)->GetAlternativeColumnForEditing(pCol, GetCurrRecord());
			if (!pCol)
				pCol = m_ColumnsInfo[m_nCurrColumnIdx];
		}
	}

	return IsTBWindowVisible(pCol->m_pParsedCtrl->GetCtrlCWnd());
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CanInsertRowSimple()
{
	return
		m_pDBT &&
		m_bAddRowEnabled &&
		m_bInsertRowEnabled &&
		!m_bInternalReadOnly;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CanInsertRowByBodyTree()
{
	return m_pDBT && !(
		!m_bInsertRowEnabled ||
		m_bInternalReadOnly ||
		m_nCurrRecordIdx < 0
		);
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CanInsertRowByTreeEdit(BOOL bEmitError /*= TRUE*/)
{
	return 	m_bAddRowEnabled						&&
		m_bInsertRowEnabled &&
		!m_bInternalReadOnly					&&
		m_pDBT									&&
		CanDoMoving(FALSE, TRUE, bEmitError) &&
		GetDBT()->GetSize() < m_nMaxBodyRecords;
}

//controlli massimali: deve contenere l'unione dei due precedenti metodi
//-----------------------------------------------------------------------------
BOOL CBodyEdit::InternalCanInsertRow(BOOL bEmitError /*= TRUE*/)
{
	return m_pDBT && !(
		(m_nCurrRecordIdx < 0 && m_pDBT->GetSize() && !m_bAddRowEnabled) ||
		!CanInsertRowByTreeEdit(bEmitError) ||
		//(m_pBtnInsert->m_hWnd && !m_pBtnInsert->IsWindowEnabled()) ||
		!CanDoInsertRow()
		);
}

//-----------------------------------------------------------------------------
void CBodyEdit::InsertRecord()
{
	DoInsertRecord();
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::DoInsertRecord()
{
	if (!InternalCanInsertRow())
		return FALSE;

	SqlRecord* pRec = m_nCurrRecordIdx > -1 ? m_pDBT->InsertRecord(m_nCurrRecordIdx) : m_pDBT->AddRecord();
	if (pRec)
	{
		pRec->SetInsertedByUI();
		GetDocument()->UpdateDataView();//force FindRecord for HKL descriptions
	}
	DoMessageOnRecordChanged();

	if (!pRec)
		return FALSE;

	SetVScrollRange();

	// inform document for changed data
	m_bBodyModified = TRUE;
	GetDocument()->SetModifiedFlag();

	m_nCurrRecordIdx = m_pDBT->GetRecords()->FindPtr(pRec);

	//RICCARDO
	//Eventuali UpdatedataView chiamate dai metodi virtuali girati ai client document
	//impostavano una errata riga corrente
	//per sicurezza viene ricalcolato l'indice
	SetCurrRecord(m_nCurrRecordIdx);

	ShowCtrl(TRUE);
	ResizeSelections();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CanDeleteRowSimple()
{
	return
		m_bDeleteRowEnabled &&
		!m_bInternalReadOnly	&&
		m_pDBT &&
		GetDBT()->GetSize() &&
		m_nCurrRecordIdx >= 0;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::InternalCanDeleteRow()
{
	return m_pDBT && !(
		!m_bDeleteRowEnabled ||
		m_bInternalReadOnly ||

		m_nCurrRecordIdx < 0 ||

		GetDBT()->GetSize() == 0 ||

		!DoCanDoDeleteRow()
		);
}

//-----------------------------------------------------------------------------
void CBodyEdit::DeleteRecord()
{
	if (!InternalCanDeleteRow())
		return;

	// deve essere fatta prima di dire al DBT di cancellare la riga
	BOOL bLastRec = m_nCurrRecordIdx == m_pDBT->GetUpperBound();

	m_bRejectUpdate = UPDATE_BODY_REJECT_ALL;

	if (IsCtrlVisible())
	{
		// Serve per resettare gli eventuali stati di bad e spegnere il
		// il control se correntemente visualizzato, in quanto se vengo effettuate
		// messaggistiche o cambi di fuoco nella delete record dopo aver eliminato
		// il record, e quindi il dataobj connesso al control, non venga inescato un
		// refenziamento ad un dataobj inesistente da parte del control stesso.
		m_ColumnsInfo[m_nCurrColumnIdx]->ReattachBaseData();

		SetFocusOnly();
	}

	BOOL bWasReadOnly = m_bInternalReadOnly;
	SetBodyReadOnly(TRUE); //an. 13499

	BOOL bOk = m_pDBT->DeleteRecord(m_nCurrRecordIdx);

	if (!bLastRec && m_pDBT->IsKindOf(RUNTIME_CLASS(DBTTree)))
		bLastRec = m_nCurrRecordIdx > m_pDBT->GetUpperBound();

	DoMessageOnRecordChanged();

	if (!bOk)
	{
		m_bRejectUpdate = UPDATE_BODY_REJECT_NONE;

		//an. 13499: problems arise when a MessageBox moves focus from bodyedit in an unexpected way,
		//so a force lose focus here
		SetBodyReadOnly(bWasReadOnly);
		ResizeSelections();
		return;
	}

	m_bRejectUpdate = UPDATE_BODY_REJECT_NONE;

	SetVScrollRange();

	if (bLastRec)
	{
		/*@@ Old Version
				if (m_nCurrRecordIdx == 0)
				{
				ResetBodyStatus();

				// go out of body on previous control in tab stop sequence,
				// if no other control are present, current CBodyEdit is default
				GetNextViewTabItem(TRUE, this)->SetFocus();

				// inform document for changed data
				m_bBodyModified = TRUE;
				GetDocument()->SetModifiedFlag();

				return;
				}
				@@*/
		if (m_nCurrRecordIdx > 0 && m_nCurrRecordIdx == m_nStartRecordIdx)
		{
			m_nStartRecordIdx--;
			if (m_nStartRecordIdx < 0)
				m_nStartRecordIdx = 0;

			UpdateVScrollPos();
		}

		m_nCurrRecordIdx--;

		if (m_nStartRecordIdx < -1)
			m_nStartRecordIdx = -1;
	}

	// inform document for changed data
	m_bBodyModified = TRUE;
	GetDocument()->SetModifiedFlag();

	m_nNotValidCellCol = -1;
	m_nNotValidCellRec = -1;

	SetCurrRecord(m_nCurrRecordIdx);

	SetBodyReadOnly(bWasReadOnly); //an. 13499

	ShowCtrl(TRUE);
	ResizeSelections();
}

//il parametro BOOL bSkipCheckOnLastVisibleRow /*= FALSE */ serve per evitare il controllo che permette 
//il caricamento delle righe successive solo quando si e' arrivati a visualizzare la riga precedente a quelel da caricare.
//Via web devo caricare indipendentemente da quello che e' visualizzato nel backend
//-----------------------------------------------------------------------------
void CBodyEdit::LoadNextRecords(BOOL bSkipCheckOnLastVisibleRow /*= FALSE */)
{
	if (!m_pDBT)
		return;

	if ((GetEndRecordIdx() >= m_pDBT->GetUpperBound() || bSkipCheckOnLastVisibleRow) && m_pDBT->MoreRecords())
	{
		m_pDBT->LoadNextRecords();

		UpdateRowIndicator();

		SetVScrollRange();

		ResizeSelections();
	}
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::CallDialog()
{
	if (
		!m_pDBT ||
		!m_bFormViewCallEnabled ||
		//(m_pBtnCallDialog && m_pBtnCallDialog->m_hWnd && !m_pBtnCallDialog->IsWindowEnabled()) ||
		!GetDocument()
		)
		return FALSE;

	BOOL bRowViewVisible = m_pRowFormView != NULL;

	if (!bRowViewVisible)
	{
		CTBNamespace nsRowView = GetNamespace();
		nsRowView.SetChildNamespace(CTBNamespace::FORM, GetRowFormViewName(), GetNamespace());

		GetDocument()->SetNsCurrentViewParent(GetNamespace());
		GetDocument()->SetNsCurrentRowView(nsRowView);

		ASSERT(m_pFormViewClass);
		m_pRowFormView = (CRowFormView*)(GetDocument()->CreateSlaveView
		(
			m_pFormViewClass,
			GetRowFormViewTitle(),
			NULL,
			GetRowFormViewName(),
			m_pRowViewContainer,
			GetRowFormViewId()
		));
		if (m_pRowFormView)
		{
			ASSERT_VALID(m_pRowFormView);
			if (m_strRowFormViewName.IsEmpty())
				m_strRowFormViewName = m_pRowFormView->GetNamespace().GetObjectName();

			if (m_pRowViewContainer)
			{
				ASSERT_VALID(m_pRowViewContainer);

				AfxGetThemeManager()->MakeFlat(m_pRowFormView);
				m_pRowFormView->ModifyStyle(WS_BORDER | WS_DLGFRAME | WS_THICKFRAME, 0);

				m_pRowViewContainer->CalcSlaveViewSize();
			}
		}

		GetDocument()->SetNsCurrentViewParent(CTBNamespace());
		GetDocument()->SetNsCurrentRowView(CTBNamespace());

		//#ifdef _DEBUG
		//		ASSERT(m_nCurrRecordIdx == nCurrRec);
		//#endif
		// se il dato nella cella correntemente attiva non fosse buono la CreateSlaveView
		// fallisce
		if (m_pRowFormView == NULL)
			return FALSE;

		if (m_nCurrRecordIdx < 0)
		{
			int r = GetDBT()->GetCurrentRowIdx();
			if (r >= 0)
				SetCurrLine(r);
			else if (!GetDBT()->IsEmpty())
				SetCurrLine(0);

			ASSERT(m_nCurrRecordIdx > -1);
		}

		m_pRowFormView->Attach(this);

		// la PrepareAuxData e` gia` stata fatta dalla OnInitialUpdate
		RebuildLinks(m_nCurrRecordIdx);

		GetDocument()->m_pClientDocs->OnBodyEditRowView(this);
	}

	GetDocument()->Activate(m_pRowFormView);
	return !bRowViewVisible;
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnRowFormViewDied(CRowFormView* pRowFormView)
{
	ASSERT(pRowFormView == m_pRowFormView);
	ASSERT_VALID(m_pRowFormView);

	if (pRowFormView == m_pRowFormView)
	{
		CAbstractFormDoc* pDoc = GetDocument();
		if (pDoc)
			pDoc->m_pClientDocs->OnRowFormViewDied(pRowFormView);
		m_pRowFormView = NULL;
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::RebuildLinks(int nRec /*= -1*/, BOOL bDoPrepareAuxData /*= FALSE */)
{
	if (!m_pDBT)
		return;

	ASSERT_VALID(m_pRowFormView);

	if (bDoPrepareAuxData)
	{
		m_bRejectUpdate = UPDATE_BODY_REJECT_ALL;
		m_pRowFormView->OnPrepareAuxData();
		m_bRejectUpdate = UPDATE_BODY_REJECT_NONE;
	}

	SqlRecord* pRec = NULL;
	if (nRec == -1)
	{
		pRec = m_pDBT->GetRecord();
		pRec->Init();
	}
	else
	{
		ASSERT(!m_pDBT->IsEmpty());

		pRec = m_pDBT->GetRow(nRec);
	}
	m_pDBT->PrepareDynamicColumns(pRec, FALSE);
	m_pRowFormView->RebuildLinks(pRec);

}

//-----------------------------------------------------------------------------
void CBodyEdit::DoMoveToPrevCtrl()
{
	if (IsReadOnly())
	{
		DoUpExit();
		return;
	}

	if (m_nCurrColumnIdx < 0 || m_nCurrRecordIdx < 0)
	{
		//ASSERT(FALSE);
		return;
	}

	if (!CanDoMoving())
		return;

	int newCol = GetPrevEditableColumnFrom(m_nCurrColumnIdx);
	BOOL bPrevRow = newCol < 0;
	BOOL bRedraw = FALSE;

	if (bPrevRow)
	{
		if (
			m_pDBT->GetRow(m_nCurrRecordIdx)->IsStorable() &&
			SetToBadCell(m_nCurrRecordIdx, m_pDBT->CheckRow(m_nCurrRecordIdx), TRUE)
			)
			return;

		if (m_nCurrRecordIdx == 0)
		{
			// leave current edit position and go out of
			// body on previous control in tab stop sequence
			PrevDlgItemSetFocus();
			return;
		}

		if (m_nCurrRecordIdx == m_nStartRecordIdx)
		{
			m_nStartRecordIdx--;
			UpdateVScrollPos();
		}

		SetCurrRecord(m_nCurrRecordIdx - 1);
		m_nCurrColumnIdx = GetPrevEditableColumnFrom(m_ColumnsInfo.GetUpperBound() + 1);

		ScrollToShowUnlockedHiddenColumns();
		DoScrollHoriz(min(PhysicalToLogicalColIdx(m_nCurrColumnIdx), GetStartIdxForLastColumn()));
		UpdateHScrollPos();

		bRedraw = TRUE;
	}
	else
	{
		m_nCurrColumnIdx = newCol;

		ScrollToShowUnlockedHiddenColumns();

		if (m_nCurrColumnIdx < m_nStartColumnIdx)
		{
			DoScrollHoriz(PhysicalToLogicalColIdx(m_nCurrColumnIdx));
			UpdateHScrollPos();

			bRedraw = TRUE;
		}
	}

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	ShowCtrl(bRedraw);
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoMoveToNextCtrl()
{
	if (IsReadOnly())
	{
		DoDownExit();
		return;
	}

	// tolto assert x easybuilder (si ci muove su prima colonna/primo record)
	if (m_nCurrColumnIdx < 0 || m_nCurrRecordIdx < 0)
		return;

	if (!CanDoMoving(FALSE, FALSE))
		return;

	if (GetNextEditableColumnFrom(m_nCurrColumnIdx) < 0)
	{
		DoMoveToNextRow(TRUE, TRUE);
		return;
	}

	BOOL bRedraw = SetNextEditableColumnFrom(m_nStartColumnIdx, m_nCurrColumnIdx);

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	ShowCtrl(bRedraw);
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoMoveToPrevCol()
{
	if (IsReadOnly())
	{
		DoScrollLeft(m_nStartColumnIdx - 1);
		return;
	}

	DoMoveToPrevCtrl();
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoMoveToNextCol()
{
	if (IsReadOnly())
	{
		DoScrollRight(m_nStartColumnIdx + 1);
		return;
	}

	DoMoveToNextCtrl();
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoMoveToPrevRow(BOOL bShowCtrl /* = FALSE */)
{
	if (m_nCurrRecordIdx < 0)
	{
		return;
	}

	if (!CanDoMoving(TRUE, TRUE))
		return;

	if (m_nCurrRecordIdx == 0)
	{
		// leave current edit position and go out of
		// body on previous control in tab stop sequence
		PrevDlgItemSetFocus();
		return;
	}

	if (m_nCurrRecordIdx == m_nStartRecordIdx)
	{
		m_nStartRecordIdx--;
		UpdateVScrollPos();
	}

	SetCurrRecord(m_nCurrRecordIdx - 1);

	ScrollToShowUnlockedHiddenColumns();

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	if (!bShowCtrl)
	{
		if (IsCtrlVisible())
			SetFocusOnly();

		InvalidateBody();
		return;
	}

	ShowCtrl(TRUE);
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoMoveToNextRow(BOOL bShowCtrl /* = FALSE */, BOOL bGoHead /* = FALSE */, BOOL bWeel /*= FALSE*/)
{
	if (!m_pDBT)
		return;

	ASSERT(m_nCurrRecordIdx >= -1);

	if (!CanDoMoving(FALSE, TRUE))
		return;

	if (m_nCurrRecordIdx == m_pDBT->GetUpperBound() && m_pDBT->MoreRecords())
		LoadNextRecords();

	BOOL bAppendNewRecord = m_nCurrRecordIdx < 0 || m_nCurrRecordIdx == m_pDBT->GetUpperBound();

	if (
		m_nCurrRecordIdx == m_nMaxBodyRecords - 1 ||
		(bAppendNewRecord && (IsReadOnly() || !m_bAddRowEnabled))
		)
	{
		// leave current edit position and go out of
		// body on next control in tab stop sequence
		if (!bWeel)
			NextDlgItemSetFocus();
		return;
	}

	SetCurrRecord(m_nCurrRecordIdx + 1);

	ScrollToShowUnlockedHiddenColumns();

	if (bAppendNewRecord || bGoHead)
		SetNextEditableColumnFrom(0, -1);

	if ((m_nCurrRecordIdx - m_nStartRecordIdx) > m_nLastBodyRow)
	{
		m_nStartRecordIdx++;
		UpdateVScrollPos();
	}

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	if (!bShowCtrl)
	{
		if (IsCtrlVisible())
			SetFocusOnly();

		InvalidateBody();
		return;
	}

	ShowCtrl(TRUE);
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoMoveToFirstCol()
{
	if (m_nCurrColumnIdx < 0 || m_nCurrRecordIdx < 0)
	{
		ASSERT(FALSE);
		return;
	}

	if (m_nCurrColumnIdx == GetNextEditableColumnFrom(-1))
		return;

	if (!CanDoMoving())
		return;

	BOOL bRedraw = SetNextEditableColumnFrom(0, -1);

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	ShowCtrl(bRedraw);
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoMoveToLastCol()
{
	if (m_nCurrColumnIdx < 0 || m_nCurrRecordIdx < 0)
	{
		ASSERT(FALSE);
		return;
	}

	if (!CanDoMoving())
		return;

	int newCol = GetPrevEditableColumnFrom(m_ColumnsInfo.GetUpperBound() + 1);
	if (m_nCurrColumnIdx == newCol)
		return;

	m_nCurrColumnIdx = newCol;
	DoScrollHoriz(min(PhysicalToLogicalColIdx(m_nCurrColumnIdx), GetStartIdxForLastColumn()));
	UpdateHScrollPos();

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	ShowCtrl(TRUE);
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoMoveToFirstRow(BOOL bShowCtrl /* = FALSE */)
{
	if (!CanDoMoving(TRUE, TRUE))
		return;

	SetNextEditableColumnFrom(0, -1);

	m_nStartRecordIdx = 0;
	SetCurrRecord(0);
	UpdateVScrollPos();

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	if (!bShowCtrl)
	{
		if (IsCtrlVisible())
			SetFocusOnly();

		InvalidateBody();
		return;
	}

	ShowCtrl(TRUE);
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoMoveToLastRow(BOOL bShowCtrl /* = FALSE */)
{
	if (!CanDoMoving(FALSE, TRUE))
		return;

	SetNextEditableColumnFrom(0, -1);

	SetCurrRecord(m_pDBT->GetUpperBound());
	m_nStartRecordIdx = m_pDBT->GetUpperBound() > m_nLastBodyRow
		? m_pDBT->GetUpperBound() - m_nLastBodyRow
		: 0;
	UpdateVScrollPos();

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	if (!bShowCtrl)
	{
		if (IsCtrlVisible())
			SetFocusOnly();

		InvalidateBody();
		return;
	}

	ShowCtrl(TRUE);
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::DoPageLeft()
{
	if (m_nCurrColumnIdx < 0 || m_nCurrRecordIdx < 0)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!CanDoMoving())
		return FALSE;

	int nStartCol = GetPrevPageStartColumnIdx();

	SetNextEditableColumnFrom(nStartCol, nStartCol - 1);

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	ShowCtrl(TRUE);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::DoPageRight()
{
	if (m_nCurrColumnIdx < 0 || m_nCurrRecordIdx < 0)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!CanDoMoving())
		return FALSE;

	int nStartCol = GetNextPageStartColumnIdx();
	int nLastEdit = GetPrevEditableColumnFrom(m_ColumnsInfo.GetUpperBound() + 1);

	if (nStartCol > nLastEdit)
		return FALSE;

	int nCurrCol = nStartCol;

	if (nStartCol == m_nStartColumnIdx)
	{
		if (m_nCurrColumnIdx == nLastEdit)
			return FALSE;

		nCurrCol = nLastEdit;
	}

	SetNextEditableColumnFrom(nStartCol, nCurrCol - 1);

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	ShowCtrl(TRUE);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::DoPageUp()
{
	if (m_nCurrColumnIdx < 0 || m_nCurrRecordIdx < 0)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!CanDoMoving(TRUE, TRUE))
		return FALSE;

	if (m_nCurrRecordIdx == 0)
		return FALSE;

	if (m_nStartRecordIdx > m_nLastBodyRow)
		m_nStartRecordIdx -= m_nLastBodyRow;
	else
		m_nStartRecordIdx = 0;

	if (m_nCurrRecordIdx > m_nLastBodyRow)
		SetCurrRecord(m_nCurrRecordIdx - m_nLastBodyRow);
	else
		SetCurrRecord(0);

	UpdateVScrollPos();

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	ShowCtrl(TRUE);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::DoPageDown()
{
	if (m_nCurrColumnIdx < 0 || m_nCurrRecordIdx < 0)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!CanDoMoving(FALSE, TRUE))
		return FALSE;

	if (m_nCurrRecordIdx == m_pDBT->GetUpperBound())
	{
		if (!m_pDBT->MoreRecords())
			return FALSE;

		LoadNextRecords();
	}

	if (GetEndRecordIdx() < m_pDBT->GetUpperBound())
		m_nStartRecordIdx += m_nLastBodyRow;

	if (m_nStartRecordIdx + m_nLastBodyRow > m_pDBT->GetUpperBound() && m_pDBT->MoreRecords())
		LoadNextRecords();

	if ((m_nCurrRecordIdx + m_nLastBodyRow) < m_pDBT->GetUpperBound())
		SetCurrRecord(m_nCurrRecordIdx + m_nLastBodyRow);
	else
		SetCurrRecord(m_pDBT->GetUpperBound());

	UpdateVScrollPos();

	m_nOldStartRecordIdx = -1;
	m_nOldStartColumnIdx = -1;

	ShowCtrl(TRUE);
	return TRUE;
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoMoveToHomeEnd(BOOL bGotoHome)
{
	if (!m_pDBT)
		return;

	if (m_pDBT->IsEmpty() && IsReadOnly())
		return;

	if (m_pDBT->IsEmpty())
	{
		m_nOldStartRecordIdx = -1;
		m_nOldStartColumnIdx = -1;

		CWnd* pOldWnd = SetFocus();
		if (pOldWnd && pOldWnd->m_hWnd == m_hWnd)
			OnSetFocus(this);
	}
	else
	{
		m_bBodyFocused = TRUE;
		m_bBodyActive = TRUE;

		if (bGotoHome)
			DoMoveToFirstRow(TRUE);
		else
			DoMoveToLastRow(TRUE);

		EnableButtons();
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoUpExit()
{
	if (CanDoMoving(TRUE, TRUE))
		PrevDlgItemSetFocus();
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoDownExit()
{
	if (CanDoMoving(TRUE, TRUE))
		NextDlgItemSetFocus();
}

//-----------------------------------------------------------------------------
void CBodyEdit::PrevDlgItemSetFocus()
{
	// give DBTSlaveBuffered a chance to check all records
	if (!CheckRecords(TRUE))
		return;

	CWnd* pWnd = GetNextViewTabItem();

	if (pWnd)
	{
		pWnd->SetFocus();

		m_nOldStartRecordIdx = -1;
		m_nOldStartColumnIdx = -1;
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::NextDlgItemSetFocus()
{
	// give DBTSlaveBuffered a chance to check all records
	if (!CheckRecords(TRUE))
		return;

	CWnd* pWnd = GetNextViewTabItem(FALSE);

	if (pWnd)
	{
		pWnd->SetFocus();

		m_nOldStartRecordIdx = -1;
		m_nOldStartColumnIdx = -1;
	}
}

//------------------------------------------------------------------------------
int CBodyEdit::GetFirstUnlockedVisibleColumn()
{
	int i = GetVisibleColumnIdx(m_nLockedColumnIDC) + 1;

	while (i < m_ColumnsInfo.GetSize() && !m_ColumnsInfo[i]->m_bVisible)
		i++;

	ASSERT(i < m_ColumnsInfo.GetSize());
	return i;
}

//------------------------------------------------------------------------------
void CBodyEdit::DoScrollHoriz(int nNewStart)
{
	if (m_nStartColumnIdx == nNewStart)
		return;

	if (IsCtrlVisible())
		SetFocusOnly();

	if (ColumnLocked())
	{
		int nStart = GetVisibleColumnIdx(m_nLockedColumnIDC);
		nStart++;
		int offset = m_ColumnsInfo[nStart]->GetHorzColOffset();
		for (int i = nStart; i < m_ColumnsInfo.GetSize(); i++)
		{
			ColumnInfo* pColumnInfo = m_ColumnsInfo[i];

			pColumnInfo->m_bVisible = i >= (nStart + nNewStart);

			pColumnInfo->ResizeColumn(GetFont(), offset, m_rectBody.Width());
		}

		MakeLastColsIdxArray();
	}

	SaveStartColumn();
	m_nStartColumnIdx = nNewStart;
	UnsaveStartColumn();

	UpdateHScrollPos();
	InvalidateBody();

	if (!IsReadOnly() && m_bBodyFocused)
	{
		if (m_nOldStartColumnIdx == -1 && m_nOldStartRecordIdx == -1)
			ShowCtrl(FALSE);
		else if (this->m_pDBT->GetSize() && m_nCurrColumnIdx > 0)
		{
			// serve per far ricevere dal control i key down da tastiera
			m_ColumnsInfo[m_nCurrColumnIdx]->m_pParsedCtrl->EnableCtrl(TRUE);
			m_ColumnsInfo[m_nCurrColumnIdx]->m_pParsedCtrl->SetCtrlFocus(FALSE);
		}
	}
}

//------------------------------------------------------------------------------
void CBodyEdit::DoScrollLeft(int nNewStart)
{
	if (CanDoScrollLeft())
		DoScrollHoriz(nNewStart < 0 ? 0 : nNewStart);
}

//------------------------------------------------------------------------------
void CBodyEdit::DoScrollRight(int nNewStart)
{
	int nStartForLast = GetStartIdxForLastColumn();
	if (CanDoScrollRight())
		DoScrollHoriz(nNewStart > nStartForLast ? nStartForLast : nNewStart);
}

//------------------------------------------------------------------------------
void CBodyEdit::DoScrollVert(int nNewStart)
{
	if (IsCtrlVisible())
		SetFocusOnly();

	SaveStartRecord();
	m_nStartRecordIdx = nNewStart;
	UnsaveStartRecord();

	UpdateVScrollPos();
	InvalidateBody();

	if (!IsReadOnly() && m_bBodyFocused)
		if (m_nOldStartColumnIdx == -1 && m_nOldStartRecordIdx == -1)
			ShowCtrl(FALSE);
		else
			if (m_nCurrColumnIdx > 0)
			{
				// serve per far ricevere dal control i key down da tastiera
				m_ColumnsInfo[m_nCurrColumnIdx]->m_pParsedCtrl->EnableCtrl(TRUE);
				m_ColumnsInfo[m_nCurrColumnIdx]->m_pParsedCtrl->SetCtrlFocus(FALSE);
			}
}

//------------------------------------------------------------------------------
void CBodyEdit::DoScrollDown(int nNewStart)
{
	if (m_pDBT && GetEndRecordIdx() < m_pDBT->GetUpperBound())
		DoScrollVert(nNewStart);
}

//------------------------------------------------------------------------------
void CBodyEdit::DoScrollUp(int nNewStart)
{
	if (m_nStartRecordIdx != 0)
		DoScrollVert(nNewStart < 0 ? 0 : nNewStart);
}

//------------------------------------------------------------------------------
void CBodyEdit::OnHScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{
	CWnd::OnHScroll(nSBCode, nPos, pScrollBar);

	if (nSBCode == SB_ENDSCROLL)
		return;

	if (!UpdateData(TRUE))
	{
		UpdateHScrollPos();
		return;
	}

	switch (nSBCode)
	{
	case SB_LINELEFT: DoScrollLeft(m_nStartColumnIdx - 1);			break;
	case SB_LEFT: DoScrollLeft(0);								break;
	case SB_PAGELEFT: DoScrollLeft(GetPrevPageStartColumnIdx());	break;
	case SB_LINERIGHT: DoScrollRight(m_nStartColumnIdx + 1);			break;
	case SB_RIGHT: DoScrollRight(GetStartIdxForLastColumn());	break;
	case SB_PAGERIGHT: DoScrollRight(GetNextPageStartColumnIdx());	break;
	case SB_THUMBPOSITION:
	case SB_THUMBTRACK: if ((int)nPos != m_nStartColumnIdx) DoScrollHoriz(nPos); break;
	}
}

//------------------------------------------------------------------------------
void CBodyEdit::ChangeHeightRow(UINT nSBCode)
{
	if (!UpdateData(TRUE))
		return;

	switch (nSBCode)
	{
	case SB_LINEUP:

		if (m_nLinesPerRow <= 1)
			return;

		m_nLinesPerRow--;
		break;

	case SB_LINEDOWN:

		m_nLinesPerRow++;

		if (RecalcRowsLayout())
			break;

		m_nLinesPerRow--;

		RecalcRowsLayout();
		return;
	}

	RecalcRowsLayout();
	SetVScrollRange();

	if (m_nCurrRecordIdx < 0)
		DoScrollVert(m_nStartRecordIdx);
	else
		DoScrollVert(m_nCurrRecordIdx);
}

//------------------------------------------------------------------------------
void CBodyEdit::OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{
	if (pScrollBar && pScrollBar->IsKindOf(RUNTIME_CLASS(CSpin)))
	{
		ChangeHeightRow(nSBCode);
		return;
	}

	CWnd::OnVScroll(nSBCode, nPos, pScrollBar);

	if (nSBCode == SB_ENDSCROLL)
		return;

	if (!UpdateData(TRUE))
		return;

	switch (nSBCode)
	{
	case SB_LINEDOWN:	DoScrollDown(m_nStartRecordIdx + 1);
		LoadNextRecords();
		break;

	case SB_BOTTOM:	DoScrollDown(m_pDBT->GetUpperBound() - m_nLastBodyRow);	break;
	case SB_PAGEDOWN: {
		int nRec = GetEndRecordIdx();
		if (nRec == m_nStartRecordIdx) nRec++;
		DoScrollDown(nRec);
		LoadNextRecords();
	}
					  break;

	case SB_LINEUP:	DoScrollUp(m_nStartRecordIdx - 1);						break;
	case SB_TOP:	DoScrollUp(0);											break;
	case SB_PAGEUP:	DoScrollUp(m_nStartRecordIdx - m_nLastBodyRow);			break;
	case SB_THUMBPOSITION:
	case SB_THUMBTRACK:	if ((int)nPos != m_nStartRecordIdx)
	{
		DoScrollVert(nPos);
		LoadNextRecords();
	}
						break;
	}
}

//------------------------------------------------------------------------------
/*
nFlags : Indicates whether various virtual keys are down. This parameter can be any combination of the following values:
MK_CONTROL   Set if the CTRL key is down.
MK_LBUTTON   Set if the left mouse button is down.
MK_MBUTTON   Set if the middle mouse button is down.
MK_RBUTTON   Set if the right mouse button is down.
MK_SHIFT	 Set if the SHIFT key is down.
zDelta : Indicates distance rotated.
The zDelta value is expressed in multiples or divisions of WHEEL_DELTA, which is 120.
A value less than zero indicates rotating back (toward the user)
while a value greater than zero indicates rotating forward (away from the user).
The user can reverse this response by changing the Wheel setting in the mouse software.
See the Remarks for more information about this parameter.
pt : Specifies the x- and y-coordinate of the cursor. These coordinates are always relative to the upper-left corner of the screen.

*/
BOOL CBodyEdit::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt)
{
	// per inibire l'uso della rotellina quando è attivo il FieldInspector (problemi di redraw del rettangolo)
	if (m_bIsInspecting)
		return FALSE;

	if (zDelta > 0)
	{
		DoMoveToPrevRow(TRUE);
	}
	else if (zDelta < 0)
	{
		BOOL bSave_m_bAddRowEnabled = m_bAddRowEnabled;
		m_bAddRowEnabled = FALSE;
		DoMoveToNextRow(TRUE, FALSE, TRUE);
		m_bAddRowEnabled = bSave_m_bAddRowEnabled;
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::CanDoMoveToPrevRow()
{
	return m_pDBT && !m_pDBT->IsEmpty() && m_nCurrRecordIdx > 0;
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::CanDoMoveToNextRow()
{
	return	m_pDBT && m_nCurrRecordIdx >= 0 &&
		(
			m_nCurrRecordIdx < m_pDBT->GetUpperBound() ||
			m_pDBT->MoreRecords() ||
			(m_bAddRowEnabled && !IsReadOnly())
			);
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::CanDoMoveToFirstRow()
{
	return	m_pDBT &&
		(
			m_nCurrRecordIdx > 0 ||
			m_nCurrRecordIdx < 0 && (!m_pDBT->IsEmpty() || (m_bAddRowEnabled && !IsReadOnly()))
			);
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::CanDoMoveToLastRow()
{
	return m_pDBT && !m_pDBT->IsEmpty() && m_nCurrRecordIdx < m_pDBT->GetUpperBound();
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::ShowColumnCtrl(int nCol)
{
	if (IsReadOnly() || m_nCurrRecordIdx < 0 || nCol < 0 || nCol > m_ColumnsInfo.GetUpperBound())
		return FALSE;

	m_nCurrColumnIdx = nCol;	// current active column

	if (IsCtrlVisible())
		return ShowCtrl(FALSE);

	if (m_nCurrColumnIdx > GetEndColumnIdx())
	{
		DoScrollHoriz(GetNextPageStartColumnIdx());
		UpdateHScrollPos();
	}

	CParsedCtrl* pCtrl = m_ColumnsInfo[m_nCurrColumnIdx]->GetParsedCtrl();
	if (
		ShowCtrl(TRUE) &&
		pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton))
		)
	{
		// Nel caso la colonna selezionata sia associata ad un CBoolButton
		// la transizione di stato del valore viene forzata immediatamente
		// all'atto del click del mouse sulla colonna stessa

		// corrente valore del DataBool associato
		BOOL bValue = (BOOL)*((DataBool*)(pCtrl->GetCtrlData()));

		// viene usata la SetCheck di CButton (MFC) poiche` la SetValue di CBoolButton (TB)
		// non invia il messaggio di VALUE_CHANGED
		//
		((CBoolButton*)(pCtrl->GetCtrlCWnd()))->SetCheck(bValue ? 0 : 1);
	}

	if (ShowCtrl(TRUE))
	{
		if (pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
		{

			// Nel caso la colonna selezionata sia associata ad un CBoolButton
			// la transizione di stato del valore viene forzata immediatamente
			// all'atto del click del mouse sulla colonna stessa

			// corrente valore del DataBool associato
			BOOL bValue = (BOOL)*((DataBool*)(pCtrl->GetCtrlData()));

			// viene usata la SetCheck di CButton (MFC) poiche` la SetValue di CBoolButton (TB)
			// non invia il messaggio di VALUE_CHANGED
			//
			((CBoolButton*)(pCtrl->GetCtrlCWnd()))->SetCheck(bValue ? 0 : 1);
		}
	}
	return TRUE;
}

//------------------------------------------------------------------------------
ColumnInfo*  CBodyEdit::GetVisibleColumn(DataObj* pData)
{
	for (int i = 0; i < m_ColumnsInfo.GetSize(); i++)
	{
		ColumnInfo* pColInfo = m_ColumnsInfo[i];
		if (pColInfo && pColInfo->m_pParsedCtrl)
		{
			if (pColInfo->m_pParsedCtrl->GetCtrlData() == pData)
				return pColInfo;
		}
	}
	return NULL;
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::ShowColumnCtrl(DataObj* pData)
{
	ColumnInfo* pColInfo = GetVisibleColumn(pData);
	if (pColInfo)
	{
		return ShowColumnCtrl(pColInfo->m_nColPos);
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
// nr. of button on the left of HScrollBar
int CBodyEdit::NumberOfUserButtons()
{
	return 0;
	/*
		int nb = m_UserButtons.GetSize();

		if (BEGetExStyle(BE_STYLE_SHOW_MINI_TOOLBAR))
		{
		nb +=
		(BEGetExStyle(BE_STYLE_ALLOW_INSERT) ? 1 : 0) +
		(BEGetExStyle(BE_STYLE_ALLOW_DELETE) ? 1 : 0) +
		(BEGetExStyle(BE_STYLE_ALLOW_CALLDIALOG) ? 1 : 0) +
		(BEGetExStyle(BE_STYLE_ALLOW_SEARCH) ? 1 : 0) +
		(BEGetExStyle(BE_STYLE_ALLOW_MULTIPLE_SEL) ? 1 : 0) +
		(m_bAllowColumnHeaderSmallFont ? 1 : 0)
		;
		}
		return nb;
		*/
}

//-----------------------------------------------------------------------------
void CBodyEdit::EnableOrdering(BOOL bEnable, BOOL bApplyOnBrowse /*= FALSE*/, BOOL bEnableDuringEdit /*= FALSE*/)
{
	BESetExStyle(BE_STYLE_ALLOW_SORT, bEnable);
	BESetExStyle(BE_STYLE_APPLY_SORT_ON_BROWSE, bEnable && bApplyOnBrowse);
	BESetExStyle(BE_STYLE_ALLOW_SORT_ON_EDIT, bEnable && bEnableDuringEdit);
}

//-----------------------------------------------------------------------------
void CBodyEdit::RemoveColumnFromOrderingList(ColumnInfo* pCol, BOOL bForceRemove/*=FALSE*/)
{
	for (int i = 0; i <= m_OrdSelectedColumn.GetUpperBound(); i++)
	{
		if (m_OrdSelectedColumn.GetAt(i) == pCol)
		{
			if
				(
					bForceRemove ||
					(
						pCol->m_Ordering != ColumnInfo::ASCFIXED &&
						pCol->m_Ordering != ColumnInfo::DESCFIXED
						)
					)
			{
				pCol->m_Ordering = ColumnInfo::NONE;
				m_OrdSelectedColumn.RemoveAt(i);
			}
			return;
		}
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::ClearOrderingList()
{
	DataBool bClear = FALSE;
	for (int i = 0; i <= m_OrdSelectedColumn.GetUpperBound();)
	{
		ColumnInfo* pCol = (ColumnInfo*)m_OrdSelectedColumn[i];

		if
			(
				pCol->m_Ordering != ColumnInfo::ASCFIXED
				&&
				pCol->m_Ordering != ColumnInfo::DESCFIXED
				)
		{
			bClear = TRUE;
			pCol->SetColumnOrdering(ColumnInfo::NONE);

			m_OrdSelectedColumn.RemoveAt(i);
		}
		else
			i++;
	}
	if (bClear)
		Invalidate();
}

//-----------------------------------------------------------------------------
void CBodyEdit::ToggleColumnOrdering(ColumnInfo* pCol, BOOL bClearAll)
{
	//NON cambia l'ordine dei FIXED
	ASSERT(pCol);
	if (!pCol->ToggleColumnOrdering())
		return;
	ColumnInfo::ColumnOrderingType Order = pCol->GetColumnOrdering();

	if (bClearAll)
		ClearOrderingList();
	else
		RemoveColumnFromOrderingList(pCol);

	if
		(
			Order == ColumnInfo::ASC ||
			Order == ColumnInfo::DESC
			)
	{
		pCol->SetColumnOrdering(Order);
		m_OrdSelectedColumn.Add(pCol);

		TRACE((LPCTSTR)cwsprintf(_T("Set ordering %s on %s\n"), (Order == ColumnInfo::ASC ? L"ASC" : L" DESC"), (LPCTSTR)pCol->GetTitle()));
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetColumnOrdering(ColumnInfo* pCol, ColumnInfo::ColumnOrderingType Order)
{
	ASSERT(pCol);
	RemoveColumnFromOrderingList(pCol, TRUE); //forza l'eliminazione anceh dei FIXED (tipicamente per la customize)
	pCol->SetColumnOrdering(Order);

	if
		(
			Order == ColumnInfo::ASCFIXED ||
			Order == ColumnInfo::DESCFIXED ||
			Order == ColumnInfo::ASC ||
			Order == ColumnInfo::DESC
			)
		m_OrdSelectedColumn.Add(pCol);
}

//-----------------------------------------------------------------------------
void CBodyEdit::Sort()
{
	if (!GetDocument())
		return;
	CWaitCursor curW;

	CString ColumnOrder;
	CString ColumnName;
	ColumnInfo::ColumnOrderingType Ordering;

	for (int i = 0; i <= m_OrdSelectedColumn.GetUpperBound(); i++)
	{
		ColumnInfo* pCol = ((ColumnInfo*)m_OrdSelectedColumn.GetAt(i));
		Ordering = pCol->GetColumnOrdering();

		if (
			Ordering == ColumnInfo::DESC ||
			Ordering == ColumnInfo::ASC ||
			Ordering == ColumnInfo::DESCFIXED ||
			Ordering == ColumnInfo::ASCFIXED
			)
		{
			ColumnName = m_pDBT->GetRecord()->GetColumnName(pCol->GetDataInfoIdx());
			ColumnOrder = ColumnOrder + ColumnName;

			if
				(
					Ordering == ColumnInfo::DESC ||
					Ordering == ColumnInfo::DESCFIXED
					)
				ColumnOrder = ColumnOrder + _T(" DESC,");
			else
				ColumnOrder = ColumnOrder + _T(",");
		}
		else
		{
			ASSERT(FALSE);
		}
	}

	if (ColumnOrder.GetLength() > 0)
	{
		ColumnOrder = ColumnOrder.Left(ColumnOrder.GetLength() - 1);

		SqlRecord* pRec = GetCurrRecord();

		LoadAllRows();

		CString sMsg;
		if (m_OrdSelectedColumn.GetUpperBound() > 1)
			sMsg = (_TB("Ordering on columns:") + ' ' + ColumnOrder);
		else
			sMsg = (_TB("Ordering on column:") + ' ' + ColumnOrder + _T(" - ") + _TB("Press SHIFT key to select more columns"));

		AfxSetStatusBarText(sMsg);
		TRACE(sMsg + '\n');

		m_bOrdering = TRUE;

		RecordArray arSelectedRec;
		if (GetMultiSelMode() == MULTIPLE_SEL)
		{
			CopySelections(arSelectedRec);
			for (int c = 0; c < m_SelRecords.GetSize(); c++)
			{
				m_SelRecords[c] = NOT_SELECTED;
			}
		}

		m_pDBT->MemorySort(ColumnOrder);

		int idx = pRec ? m_pDBT->GetRecords()->FindPtr(pRec) : -1;
		if (idx >= 0)
		{
			SetCurrLine(idx, idx < m_nLastBodyRow ? 0 : -1);
		}

		if (
			(GetDocument()->GetFormMode() == CBaseDocument::EDIT)
			||
			(GetDocument()->GetFormMode() == CBaseDocument::NEW)
			)
			m_pDBT->PrepareAllPrimaryKeys();

		if (GetMultiSelMode() == MULTIPLE_SEL)
			ApplySelections(arSelectedRec);

		m_bOrdering = FALSE;
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::LoadAllRows(BOOL bAskConfirm /*= FALSE*/)
{
	if (m_pDBT->MoreRecords() || m_pDBT->GetDelayedReadType() != DBTSlave::NO_DELAYED)
	{
		if (bAskConfirm && AfxMessageBox(_TB("There are other records to load. Do you want load and select them too?"), MB_YESNO) != IDYES)
		{
			return;
		}

		CWaitCursor cw;
		AfxSetStatusBarText(_TB("Loading all rows"));

		while (m_pDBT->MoreRecords())
			m_pDBT->LoadNextRecords();

		SetVScrollRange();
	}
}

//-----------------------------------------------------------------------------
ColumnInfoStateCtrlData* CBodyEdit::AttachStateData(ColumnInfo* pColInfo, DataObj* pDataObj, BOOL bInvertDefaultStates /*= FALSE*/)
{
	if (pColInfo && pDataObj)
	{
		return pColInfo->AttachStateData(pDataObj, bInvertDefaultStates);
	}
	return NULL;
}

ColumnInfoStateCtrlData* CBodyEdit::AttachStateCmd(ColumnInfo* pColInfo, const CString& idb)
{
	static DataBool db(TRUE);
	ColumnInfoStateCtrlData* pStateCtrl = AttachStateData(pColInfo, &db);
	pStateCtrl->SetCommandBtn();
	pStateCtrl->SetCtrlState(0, idb, idb);
	pStateCtrl->SetCtrlState(1, idb, idb);
	return pStateCtrl;
}

//------------------------------------------------------------------------------
CParsedCtrl* CBodyEdit::GetCurrentParsedCtrl(UINT nIDC)
{
	ColumnInfo* pColInfo;
	CParsedCtrl* pCtrl;
	for (int i = 0; i <= m_ColumnsInfo.GetUpperBound(); i++)
	{
		pColInfo = (ColumnInfo*)m_ColumnsInfo.GetAt(i);
		pCtrl = pColInfo->GetParsedCtrl();

		if (pCtrl->GetCtrlID() == nIDC)
			return pCtrl;
	}

	return NULL;
}

//------------------------------------------------------------------------------
CParsedCtrl* CBodyEdit::GetCurrentParsedCtrl(CTBNamespace aNS)
{
	ColumnInfo* pColInfo;
	for (int i = 0; i < GetAllColumnsInfoNumber(); i++)
	{
		pColInfo = GetColumnFromIdx(i);

		if (!pColInfo)
			continue;

		if (pColInfo->GetNamespace() == aNS)
			return pColInfo->GetParsedCtrl();
	}
	return NULL;
}

//------------------------------------------------------------------------------
CParsedCtrl* CBodyEdit::GetCurrentParsedCtrl(DataObj* pDataObj)
{
	ColumnInfo* pColInfo;
	for (int i = 0; i < GetAllColumnsInfoNumber(); i++)
	{
		pColInfo = GetColumnFromIdx(i);

		if (!pColInfo)
			continue;

		CParsedCtrl* pCtrl = pColInfo->GetParsedCtrl();
		if (pCtrl && pCtrl->GetCtrlData() == pDataObj)
			return pCtrl;
	}
	return NULL;
}

//---------------------------------------------------------------------------------------------
int CBodyEdit::GetCtrlColumnDataIdx(const CTBNamespace aNS)
{
	ColumnInfo* pColInfo;

	for (int i = 0; i < GetAllColumnsInfoNumber(); i++)
	{
		pColInfo = GetColumnFromIdx(i);
		if (!pColInfo)
			continue;

		if (pColInfo->GetNamespace() == aNS)
			return pColInfo->GetDataInfoIdx();
	}
	return -1;
}

//---------------------------------------------------------------------------------------------
int CBodyEdit::GetColumnIdxFromParsedCtrl(CTBNamespace aNS)
{
	for (int currentColumnIdx = 0; currentColumnIdx <= m_ColumnsInfo.GetUpperBound(); currentColumnIdx++)
	{
		if (aNS == m_ColumnsInfo[currentColumnIdx]->GetNamespace())
			return currentColumnIdx;
	}
	return -1;
}

//-----------------------------------------------------------------------------
int CBodyEdit::GetColumnIdx(ColumnInfo* pColInfo)
{
	for (int i = 0; i <= m_AllColumnsInfo.GetUpperBound(); i++)
	{
		ColumnInfo* pInfo = m_AllColumnsInfo.GetAt(i);
		if (pInfo == pColInfo)
			return i;
	}

	return -1;
}
//-----------------------------------------------------------------------------
int CBodyEdit::GetRowNumber() const
{
	return m_pDBT->GetSize();
}

//-----------------------------------------------------------------------------
//reimplementato su CAbstractCtrl, utilizzato da ExternalControllerInfo (TbScheduler)
DataObj* CBodyEdit::GetCtrlData(const CString& sName, int nRow /*= 0*/)
{
	SqlRecord* pRec = NULL;
	if (nRow == GetRowNumber())
	{
		pRec = m_pDBT->AddNewRecord(FALSE);
		if (pRec)
		{
			pRec->SetInsertedByUI();
			GetDocument()->UpdateDataView();//force FindRecord for HKL descriptions
		}
	}
	else if (nRow < GetRowNumber())
	{
		pRec = m_pDBT->GetRow(nRow);
	}
	else
	{
		ASSERT(FALSE);
		return NULL;
	}
	return GetDataObjFromColumnName(pRec, sName);
}

//-----------------------------------------------------------------------------
DataObj* CBodyEdit::GetDataObjFromColumnName(SqlRecord* pRec, const CString& sName)
{
	DataObj* pDataObj = pRec->GetDataObjFromColumnName(sName);
	ASSERT_VALID(pDataObj);
	return pDataObj;
}

//-----------------------------------------------------------------------------
DataObj* CBodyEdit::GetDataObjAt(SqlRecord* pRec, ColumnInfo* pColumnInfo)
{
	ASSERT_VALID(pRec);
	ASSERT_VALID(pColumnInfo);

	DataObj* pDataObj = pRec->GetDataObjAt(pColumnInfo->m_nDataInfoIdx);
	ASSERT_VALID(pDataObj);
	return pDataObj;
}

//-----------------------------------------------------------------------------
int CBodyEdit::EnumColumnName(CStringArray& arNames, BOOL /*bAll = TRUE*/)
{
	arNames.RemoveAll();
	ColumnInfo* pColInfo;
	CString sColumnName;
	for (int i = 0; i <= m_ColumnsInfo.GetUpperBound(); i++)
	{
		pColInfo = (ColumnInfo*)m_ColumnsInfo.GetAt(i);
		sColumnName = m_pDBT->GetRecord()->GetColumnName(pColInfo->GetDataInfoIdx());
		arNames.Add(sColumnName);
	}
	return arNames.GetSize();
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnToggleAlternateColor()
{
	m_bAlternateColor = !m_bAlternateColor;

	CString strImg = m_bAlternateColor ? TBIcon(szIconEasyReadingOn, MINI) : TBIcon(szIconEasyReadingOff, MINI);

	CBEButton* pBtn = m_FooterToolBar.FindButton(IDC_BE_ALTERNATECOLOR);
	if (pBtn)
		pBtn->ChangeImage(strImg);

	Invalidate();
}

///////////////////////////////////////////////////////////////////////////////

void CBodyEdit::OnUnlockColumn()
{
	UnLockColumn();
	Invalidate();
}

//-----------------------------------------------------------------------------
void CBodyEdit::InitializeLock(UINT nLockedColumnIDC, BOOL bFromCustomize /*=FALSE*/)
{
	int nIdxCol = nLockedColumnIDC ? GetVisibleColumnIdx(nLockedColumnIDC) : -1;
	if (nIdxCol == m_ColumnsInfo.GetUpperBound())
		return;

	ColumnInfo* pColInfo = NULL;
	BodyEditColumn* pBEC = NULL;

	if (m_nLockedColumnIDC > 0 && !bFromCustomize)
	{
		pColInfo = GetColumn(m_nLockedColumnIDC);
		pBEC = GetColumnDescription(pColInfo);
		if (pBEC)
		{
			pBEC->m_wStatus &= ~STATUS_LOCKED;
			pBEC->m_bStatusChanged = TRUE;
		}
		pColInfo = NULL;
	}

	if (nIdxCol >= 0)
	{
		pColInfo = m_ColumnsInfo[nIdxCol];

		int x = this->m_rectBody.Width();
		if ((pColInfo->m_nHorzColOffset + pColInfo->GetScreenWidth()) > x)
		{
			nLockedColumnIDC = 0;
			pColInfo = NULL;
		}
	}

	m_nLockedColumnIDC = nLockedColumnIDC;

	if (pColInfo && !bFromCustomize)
	{
		pBEC = GetColumnDescription(pColInfo);
		if (pBEC)
		{
			pBEC->m_wStatus |= STATUS_LOCKED;
			pBEC->m_bStatusChanged = TRUE;
		}
	}

	if (m_nLockedColumnIDC <= 0)
	{
		for (int i = 0; i < m_ColumnsInfo.GetSize(); i++)
			m_ColumnsInfo[i]->m_bVisible = TRUE;

		DoScrollHoriz(0);
	}

	if (!bFromCustomize)
		RecalcColumnsLayout();
}

// Il parametro bFromCustomize deve essere impostato a TRUE solo se la chiamata alla
// LockColumn viene fatta nella Customize del BodyEdit
//-----------------------------------------------------------------------------
void CBodyEdit::LockColumn(UINT nIDC, BOOL bFromCustomize /*= FALSE*/)
{
	InitializeLock(nIDC, bFromCustomize);
}

//-----------------------------------------------------------------------------
void CBodyEdit::UnLockColumn(BOOL bFromCustomize /*=FALSE*/)
{
	// ripristino le larghezze originali
	InitializeLock(0, bFromCustomize);
}

//-----------------------------------------------------------------------------
void CBodyEdit::EnableColumnLocking(BOOL bEnable, BOOL bInteractive)
{
	BESetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK, bEnable);
	BESetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK_INTERACTIVE, bInteractive);
}

///////////////////////////////////////////////////////////////////////////////
// ---- Multiselezione

void CBodyEdit::OnToggleMultipleSelection()
{
	if (!CanDoMultipleSel())
		return;

	if (GetMultiSelMode() == NONE)
	{
		BeginMultipleSel();
	}
	else if (GetMultiSelMode() == MULTIPLE_SEL)
	{
		EndMultipleSel();
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::EnableMultipleSel(BOOL bSet /*= TRUE*/)
{
	BESetExStyle(BE_STYLE_ALLOW_MULTIPLE_SEL, bSet);
	BESetExStyle(BE_STYLE_ALLOW_COPY, bSet);
	BESetExStyle(BE_STYLE_ALLOW_DRAG, bSet);
	BESetExStyle(BE_STYLE_ALLOW_DRAG_READONLY_DOC, bSet);

	if (m_pBtnMultiSelection)
		m_pBtnMultiSelection->SetEnableState(bSet, FALSE);
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetMultiSelMode(MultiSelMode mode)
{
	m_MultiSelection = mode;
}

//-----------------------------------------------------------------------------
void CBodyEdit::BeginMultipleSel()
{
	// verifico se il body ha qualche controllo in edit....
	if (!UpdateData(FALSE))
		return;

	// tolgo il fuoco ai controls...
	if (IsCtrlVisible())
		SetFocusOnly();

	// cambio lo stato del body
	if (::IsWindow(m_hWnd))
		SetBodyReadOnly(TRUE);

	// predispongo le righe selezionate...
	// di default e' selezionata la stessa della modalità standard..
	ResizeSelections(TRUE);

	if (m_nCurrRecordIdx >= 0 && m_nCurrRecordIdx < m_pDBT->GetSize())
	{
		ASSERT(m_nCurrRecordIdx < m_SelRecords.GetSize());
		m_SelRecords[m_nCurrRecordIdx] = SelStatus::SELECTED;
	}
	// precarico la riga selezionata del be...
	m_IdxLastSelection = m_nCurrRecordIdx;

	SetMultiSelMode(MULTIPLE_SEL);

	if (::IsWindow(m_hWnd))
		InvalidateBody();

	DoOnBeginMultipleSel();
}

//-----------------------------------------------------------------------------
void CBodyEdit::EndMultipleSel(BOOL bRefreshBody/*=TRUE*/)
{
	m_SelRecords.RemoveAll();
	SetMultiSelMode(NONE);

	// ripristina lo stato del body
	//	SetBodyReadOnly(m_WasBodyReadOnly);
	UpdateBodyStatus();

	// rinfresco la vista
	if (::IsWindow(m_hWnd))
	{
		InvalidateBody();
		RedrawWindow();
	}

	DoOnEndMultipleSel();
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnSelectAll()
{
	if (!m_pDBT)
		return;

	if (GetDocument()->GetFormMode() == CBaseDocument::BROWSE)
	{
		LoadAllRows(/*TRUE*/);
	}
	m_SelRecords.SetSize(m_pDBT->GetSize());

	ChangeRangeStatus(0, m_SelRecords.GetUpperBound(), SELECTED);
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnUnselectAll()
{
	// Se si è in BROWSE ed è stato impostato il drag in Browse
	// allora si forza il caricamento di tutti  i dati del DBT
	if (
		m_pDBT &&
		GetDocument()->GetFormMode() == CBaseDocument::BROWSE &&
		BEGetExStyle(BE_STYLE_ALLOW_DRAG_READONLY_DOC)
		)
	{
		LoadAllRows();	// Reload e Findata si escludono a vicenda
	}

	ChangeRangeStatus(0, m_SelRecords.GetUpperBound(), NOT_SELECTED);
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnInvertSelection()
{
	// Se si è in BROWSE ed è stato impostato il drag in Browse
	// allora si forza il caricamento di tutti  i dati del DBT
	if (m_pDBT &&
		GetDocument()->GetFormMode() == CBaseDocument::BROWSE &&
		BEGetExStyle(BE_STYLE_ALLOW_DRAG_READONLY_DOC))
	{
		LoadAllRows();											// Reload e Findata si escludono a vicenda
	}

	ChangeRangeStatus(0, m_SelRecords.GetUpperBound(), SELECTED, INVERT);
}

//-----------------------------------------------------------------------------
void CBodyEdit::ChangeRangeStatus(int nStart, int nEnd, SelStatus status, InvertMode invert/*= NOT_INVERT*/, BOOL bInvalidate/* = TRUE*/)
{
	int	theStart = nStart > nEnd ? nEnd : nStart;
	int	theEnd = nStart > nEnd ? nStart : nEnd;

	theEnd = min(theEnd, m_SelRecords.GetUpperBound());
	theStart = max(theStart, 0);

	ASSERT(theStart >= 0);
	ASSERT(theEnd <= m_SelRecords.GetUpperBound());

	for (int c = theStart; c <= theEnd; c++)
	{
		if (invert == INVERT)
		{
			m_SelRecords[c] = m_SelRecords[c] == SELECTED ? NOT_SELECTED : SELECTED;
		}
		else
		{
			m_SelRecords[c] = status;
		}
	}

	if (bInvalidate)
	{
		DoCustomizeSelections(m_SelRecords);

		Invalidate();
		RedrawWindow();
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::SelectRows(CArray<int>& aiSelectedRow)
{
	if (aiSelectedRow.GetCount() >= 0)
	{
		if (IsMultiselection())
		{

			int iRecordsCount = m_SelRecords.GetCount();
			for (int iIndex = 0; iIndex < aiSelectedRow.GetSize(); iIndex++)
			{
				int iRowIndex = aiSelectedRow.GetAt(iIndex);

				if (iRowIndex >= 0 && iRowIndex < iRecordsCount)
				{
					m_SelRecords[aiSelectedRow.GetAt(iIndex)] = SELECTED;
				}
			}

		}
		else if (CanLeaveCurrPos(aiSelectedRow.GetAt(0)))
		{
			SetCurrLine(aiSelectedRow.GetAt(0));
		}
	}
}

//-----------------------------------------------------------------------------
int	 CBodyEdit::CountSelectedRows()
{
	ResizeSelections();
	int n = 0;
	for (int r = 0; r <= GetDBT()->GetUpperBound(); r++)
	{
		if (m_SelRecords[r] == SELECTED)
			n++;
	}
	return n;
}

//-----------------------------------------------------------------------------
void CBodyEdit::ClearSelections()
{
	int row = GetDBT()->GetUpperBound();
	ChangeRangeStatus(0, row, NOT_SELECTED);
}

//-----------------------------------------------------------------------------
void CBodyEdit::ResizeSelections(BOOL bStart)
{
	if (GetMultiSelMode() == NONE && !bStart)
	{
		m_SelRecords.RemoveAll();
		return;
	}

	// Se e' diminuito il numero di records reimposto le selezioni
	if (m_pDBT->GetSize() < m_SelRecords.GetSize())
		m_SelRecords.RemoveAll();

	// aggiungo gli eventuali nuovi slot
	for (
		int c = m_SelRecords.GetUpperBound() + 1;
		c <= m_pDBT->GetUpperBound();
		c++
		)
	{
		m_SelRecords.Add(NOT_SELECTED);
	}

	DoCustomizeSelections(m_SelRecords);
}

//-----------------------------------------------------------------------------
void	CBodyEdit::HandleSelection(UINT nFlags, int	nNewCurrRec)
{
	ASSERT(GetMultiSelMode() == MULTIPLE_SEL);

	if (((nFlags & MK_SHIFT) == MK_SHIFT) && (m_IdxLastSelection != -1))
	{
		if (nNewCurrRec != m_IdxEndSelection)
		{
			// tolgo la selezione precedente
			ChangeRangeStatus(m_IdxLastSelection, m_IdxEndSelection, NOT_SELECTED, NOT_INVERT, FALSE);
			// ed imposto la nuova...
			m_IdxEndSelection = nNewCurrRec;

			ChangeRangeStatus(m_IdxLastSelection, m_IdxEndSelection, SELECTED);

			SetCapture();
			return;
		}
	}

	// se e' premuto CTRL non cancello le vecchie selezioni....
	if ((nFlags & MK_CONTROL) != MK_CONTROL)
	{
		ChangeRangeStatus(0, m_SelRecords.GetUpperBound(), NOT_SELECTED, NOT_INVERT, FALSE);
	}

	// seleziona una riga alla volta

	ChangeRangeStatus(nNewCurrRec, nNewCurrRec, SELECTED, INVERT);

	m_IdxLastSelection = nNewCurrRec;
	m_IdxEndSelection = nNewCurrRec;

	SetCapture();
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoOnBeginMultipleSel()
{
	BOOL b = GetDocument()->DispatchOnBEBeginMultipleSel(this);
	if (!b) OnBeginMultipleSel();
}

void CBodyEdit::DoOnEndMultipleSel()
{
	BOOL b = GetDocument()->DispatchOnBEEndMultipleSel(this);
	if (!b) OnEndMultipleSel();
}

void CBodyEdit::DoCustomizeSelections(SelArray&	sel)
{
	BOOL b = GetDocument()->DispatchOnBECustomizeSelections(this, sel);
	if (!b) CustomizeSelections(sel);
}

//------------------------------------------------------------------------------
BOOL CBodyEdit::DoCanDoDeleteRow()
{
	BOOL b = GetDocument()->DispatchOnBECanDoDeleteRow(this);

	return (!b ? FALSE : CanDoDeleteRow());
}

///////////////////////////////////////////////////////////////////////////////
void CBodyEdit::OnTimer(UINT nIDEvent)
{
	// Scroll
	if (nIDEvent == ID_TIMER_SCROLL)
	{
		HandleScroll();
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::StopScrollTimer()
{
	if (m_TimerScroll)
	{
		KillTimer(m_TimerScroll);
		m_TimerScroll = 0;
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnCopy()
{
	m_CopyOrDragSourceCell.bIsDrag = FALSE;

	CWaitCursor		wait;
	CTBExtBEOleDataSource ods(this);
	ods.CopyToClipboard();
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnPaste()
{
	if (m_pClpBrdDataCodec || m_pDataCoDec)
	{
		m_PasteOrDropTargetCell.bIsDrop = FALSE;
		// Arrivati a questo punto i dati nel file temporaneo (il cui nome
		// è contenuto nella cliboard) sono stati caricati in memoria.
		// Devo chiamare il metodo che li travasi nella destinazione passando
		// il codec giusto.
		OnPaste(m_pClpBrdDataCodec ? m_pClpBrdDataCodec : m_pDataCoDec, m_nCurrRecordIdx);
	}
}

//-----------------------------------------------------------------------------
// Operazione di drag & drop....
// Se effettuata con il pulsante destro del mouse, nell'applicazione target viene
// propost un menu' contestuale dal quale scegliere l'operazione.
// In questo caso non viene gestita correttamente la memoria (credo sia un problema di MFC)
// Se si esegue l'operazione con il pulsante sinistro funziona tutto bene.
//
//-----------------------------------------------------------------------------
BOOL CBodyEdit::DoDrag(int nSourceRow, UINT nSourceColIDC)
{
	if (!GetDocument())
		return FALSE;

	if (BEGetExStyle(BE_STYLE_ALLOW_DRAG) && !m_bDragging)
	{
		// controlla lo stato del documento...
		if (
			!GetDocument()->m_bBatch &&
			!BEGetExStyle(BE_STYLE_ALLOW_DRAG_READONLY_DOC) &&
			(
			(GetDocument()->GetFormMode() != CBaseDocument::EDIT) &&
				(GetDocument()->GetFormMode() != CBaseDocument::NEW)
				)
			)
		{
			return FALSE;
		}


		CTBExtBEOleDropSource*	pDropSource = CreateDropSource();
		ASSERT_VALID(pDropSource);
		CTBExtBEOleDataSource* pDataSource = new CTBExtBEOleDataSource(this);
		m_bDragging = TRUE;
		m_CopyOrDragSourceCell.bIsDrag = TRUE;
		m_CopyOrDragSourceCell.nRow = nSourceRow;
		m_CopyOrDragSourceCell.nColIDC = nSourceColIDC;
		DROPEFFECT bRet = pDataSource->DoDragDrop(
			DROPEFFECT_COPY /*|DROPEFFECT_MOVE /*|DROPEFFECT_LINK*/,
			NULL,
			pDropSource
		);
		m_bDragging = FALSE;

		// Data Objects and Data Sources: Creation and Destruction
		// http://msdn.microsoft.com/en-us/library/s7s7td0c.aspx

		pDataSource->DoDragDrop();
		pDataSource->ExternalRelease();

		delete pDropSource;
		return (bRet != DROPEFFECT_NONE);
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
CTBExtBEOleDropSource*	CBodyEdit::CreateDropSource()
{
	return new CTBExtBEOleDropSource(this);
}

//-----------------------------------------------------------------------------
void	CBodyEdit::HandleScroll()
{
	if (!m_pVScrollBar)
		return;
	if ((GetCapture() != this) && (m_IdxDragRecDest != -1))
		return;

	POINT	point;
	::GetCursorPos(&point);

	CRect	rect = m_rectBody;
	ClientToScreen(rect);

	// cerco di capire se sono sopra o sotto il body...

	int nlpr = max(1, m_nLinesPerRow);

	if (point.y < rect.top)
	{
		int	delta = max(1, (rect.top - point.y) / (m_nRowsHeight / nlpr));

		// sto salendo..
		for (int c = 0; c < delta; c++)
		{
			SendMessage(WM_VSCROLL, MAKEWPARAM(SB_LINEUP, 0), (LPARAM)m_pVScrollBar->m_hWnd);
		}
	}
	else if (point.y > rect.bottom)
	{
		int	delta = max(1, (point.y - rect.bottom) / (m_nRowsHeight / nlpr));
		// sto scendendo...
		for (int c = 0; c < delta; c++)
		{
			SendMessage(WM_VSCROLL, MAKELONG(SB_LINEDOWN, 0), (LPARAM)m_pVScrollBar->m_hWnd);
		}
	}
}

//-----------------------------------------------------------------------------
void	CBodyEdit::GetDragPosition(const CPoint& cpoint, int& nIdx, DragSelType& seltype)
{
	// cerco la riga sotto il cursore...
	int xColOffs, nNewCurrRec, nCol;
	nCol = -1;
	CPoint point(cpoint);

	m_PasteOrDropTargetCell.nRow = -1;
	m_PasteOrDropTargetCell.nColIDC = 0;
	m_PasteOrDropTargetCell.bIsDrop = TRUE;

	CursorPosArea	cpa = GetCursorBodyPos(point, nCol, xColOffs, m_PasteOrDropTargetCell.nRow, nNewCurrRec);
	seltype = m_DragSelType;

	if (cpa != IN_BODY)
	{
		nIdx = -1;
	}
	else
	{
		if (nCol >= 0)
		{
			ColumnInfo* pCol = m_ColumnsInfo[nCol];
			m_PasteOrDropTargetCell.nColIDC = pCol->GetCtrlID();
		}
		// cerco di capire se sono vicino al bordo superiore o inferiore della riga..
		int	nYStart = m_rectBody.top + m_PasteOrDropTargetCell.nRow * m_nRowsHeight;
		int	nYEnd = nYStart + m_nRowsHeight;

		int nlpr = max(1, m_nLinesPerRow);

		int	delta = (m_nRowsHeight / nlpr) / 4;	// 1/4 di una riga normale..

		if ((point.y >= nYStart) && (point.y <= nYStart + delta))
		{
			// sono sul bordo superiore...
			seltype = DST_TOP;
		}
		else if ((point.y >= nYEnd - delta) && (point.y <= nYEnd))
		{
			seltype = DST_BOTTOM;
		}
		else
		{
			seltype = DST_CENTER;
		}

		nIdx = nNewCurrRec;
	}
}

//-----------------------------------------------------------------------------
void	CBodyEdit::ShowDragging(const CPoint& point)
{
	DragSelType	dst;
	int	nNewCurrRec;
	GetDragPosition(point, nNewCurrRec, dst);

	if ((m_IdxDragRecDest != nNewCurrRec) || (m_DragSelType != dst))
	{
		m_IdxDragRecDest = nNewCurrRec;
		m_DragSelType = dst;

		InvalidateBody();
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::StopDragging()
{
	if (!m_bTargetDragging)
		return;

	StopScrollTimer();

	m_bTargetDragging = FALSE;

	m_DropBmps.Release();

	if (m_IdxDragRecDest != -1)
	{
		m_IdxDragRecDest = -1;
		InvalidateBody();
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::StartDragging()
{
	m_IdxDragRecDest = -1;
	m_bTargetDragging = TRUE;

	//rispetto a prima queste nuove frecce hanno nomi inversi
	m_DropBmps.Create(TBGlyph(szGlyphArrowDown), TBGlyph(szGlyphArrowUp), TBGlyph(szGlyphArrowRight), TBGlyph(szGlyphArrowLeft));
}

//-----------------------------------------------------------------------------
void CBodyEdit::CopySelections(RecordArray	&ra)
{
	ra.SetOwns(FALSE);		// !!!! x essere sicuri che non vengano eliminati dal dbt
	ra.RemoveAll();

	if (GetMultiSelMode() == MULTIPLE_SEL)
	{
		//ASSERT(m_SelRecords.GetSize() == m_pDBT->GetSize());
		ResizeSelections();

		for (int c = 0; c <= m_SelRecords.GetUpperBound(); c++)
		{
			if (m_SelRecords[c] == SELECTED)
			{
				ra.Add(m_pDBT->GetRow(c));
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::ApplySelections(RecordArray	&ra)
{
	if (GetMultiSelMode() == MULTIPLE_SEL)
	{
		//ASSERT(m_SelRecords.GetSize() == m_pDBT->GetSize());
		ResizeSelections();

		for (int s = 0; s < ra.GetSize(); s++)
		{
			SqlRecord* pRec = ra[s];
			ASSERT_VALID(pRec);

			int pos = m_pDBT->FindRecordIndex(pRec);
			if (pos >= 0)
			{
				m_SelRecords[pos] = SELECTED;
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnContextMenu(CWnd* pWnd, CPoint ptMousePos)
{
	if (!GetDocument() || !BEGetExStyle(BE_STYLE_SHOW_CONTEXTMENU))
		return;

	if (!m_bBodyFocused && !IsCtrlVisible())
		SetFocusOnly();

	// identifica la riga/colonna su cui e' stato premuto il pulsante destro..
	int nCol, xColOffs, nRow, nCurrRec;
	CPoint	point(ptMousePos);
	ScreenToClient(&point);
	CursorPosArea cpa = GetCursorBodyPos(point, nCol, xColOffs, nRow, nCurrRec);
	//----

	if (
		(cpa == IN_BODY)
		&&
		(nCurrRec <= m_pDBT->GetUpperBound())
		)
	{
		// preso da CBodyEdit::OnLButtonDown

		// questo impedisce di forzare il fuoco se il control e` visibile perche` in stato di bad
		if (!CanLeaveCurrPos(nCurrRec))
			return;

		m_nOldStartRecordIdx = -1;
		m_nOldStartColumnIdx = -1;

		// update current record
		if (nCurrRec != m_nCurrRecordIdx)
		{
			SetCurrRecord(nCurrRec);
			InvalidateBody();
		}
	}
	//----
	m_nColSelected = -1;

	m_ContextMenu.CreatePopupMenu();

	BOOL bOk = TRUE;

	BOOL bIsCustomMenu = (GetMultiSelMode() != MULTIPLE_SEL) && OnCustomContextMenu(nCol, nCurrRec, point);
	if (!bIsCustomMenu)
	{
		if (nCol >= 0 && nCol < m_ColumnsInfo.GetSize())
		{
			ColumnInfo* pColumnInfo = m_ColumnsInfo[nCol];
			m_nColSelected = nCol;

			DataType dt = pColumnInfo->GetBaseDataObj()->GetDataType();

			if (cpa == IN_COLUMN_TITLE || !BEGetExStyle(BE_STYLE_SHOW_COLUMN_HEADERS))
			{
				m_ContextMenu.AppendMenu(MF_STRING, ID_BE_SHOW_SEARCH, (LPTSTR)(LPCTSTR)_TB("Search..."));

				if (GetDBT() && !GetDBT()->IsKindOf(RUNTIME_CLASS(DBTTree)))
				{
					m_ContextMenu.AppendMenu(MF_STRING, ID_BE_SHOW_FILTERS, (LPTSTR)(LPCTSTR)_TB("Add filters..."));

					if (/*GetDBT() && */GetDBT()->IsMemoryUIFilterActive())
					{
						if (GetDBT()->IsMemoryUIFilterActive(pColumnInfo->GetDataInfoIdx()))
						{
							ASSERT(pColumnInfo->GetDataInfoIdx() > -1);
							m_ContextMenu.AppendMenu(MF_STRING, ID_BE_REMOVE_COLUMN_FILTER, (LPTSTR)(LPCTSTR)_TB("Remove filter..."));
						}
						m_ContextMenu.AppendMenu(MF_STRING, ID_BE_REMOVE_ALL_FILTERS, (LPTSTR)(LPCTSTR)_TB("Remove all filters..."));
					}
				}

				if (
					dt == DataType::Money || dt == DataType::Quantity || dt == DataType::Percent || dt == DataType::Double ||
					dt == DataType::Integer || (dt == DataType::Long && !dt.IsAHandle()) ||
					dt == DataType::Date || dt == DataType::DateTime || dt == DataType::ElapsedTime || dt == DataType::Time
					)
				{
					m_ContextMenu.AppendMenu(MF_STRING, ID_BE_STATISTIC, (LPTSTR)(LPCTSTR)_TB("Column statistic..."));
				}
				else if (dt == DataType::String)
				{
					m_ContextMenu.AppendMenu(MF_STRING, ID_BE_STATISTIC, (LPTSTR)(LPCTSTR)_TB("Concat texts..."));
				}

				m_ContextMenu.AppendMenu(MF_STRING, ID_BE_TOGGLE_CURRENT_COLUMN_WIDTH, (LPTSTR)(LPCTSTR)_TB("Toggle fit current column width..."));
				m_ContextMenu.AppendMenu(MF_STRING, ID_BE_TOGGLE_ALL_COLUMNS_WIDTH, (LPTSTR)(LPCTSTR)_TB("Toggle fit all columns width..."));

				if (m_nLockedColumnIDC > 0 && BEGetExStyle(BE_STYLE_ALLOW_COLUMN_LOCK_INTERACTIVE))
				{
					m_ContextMenu.AppendMenu(MF_STRING | MF_ENABLED, ID_BE_UNLOCK_COLUMN, (LPTSTR)(LPCTSTR)_TB("Unlock column"));
				}
			}
		}

		if (cpa == IN_BODY)
		{
			// solo su selezione multipla
			if (GetMultiSelMode() == MULTIPLE_SEL)
			{
				m_ContextMenu.AppendMenu(MF_SEPARATOR, 0, NULL);

				m_ContextMenu.AppendMenu(MF_STRING | MF_ENABLED, ID_BE_SELECT_ALL, (LPTSTR)(LPCTSTR)_TB("Select all"));

				m_ContextMenu.AppendMenu(MF_STRING | MF_ENABLED, ID_BE_UNSELECT_ALL, (LPTSTR)(LPCTSTR)_TB("Unselect all"));

				m_ContextMenu.AppendMenu(MF_STRING | MF_ENABLED, ID_BE_INVERT_SELECTION, (LPTSTR)(LPCTSTR)_TB("Toggle selection"));

				// copia & incolla
				if (GetAllowCopy() || GetAllowPaste())
				{
					m_ContextMenu.AppendMenu(MF_SEPARATOR, 0, NULL);

					m_ContextMenu.AppendMenu(MF_STRING | MF_ENABLED, ID_BE_COPY, (LPTSTR)(LPCTSTR)_TB("Copy"));

					m_ContextMenu.AppendMenu(MF_STRING | MF_ENABLED, ID_BE_PASTE, (LPTSTR)(LPCTSTR)_TB("Paste"));

					// disabilitazione comandi
					UINT	enable_flags = MF_BYCOMMAND | MF_ENABLED;
					UINT	disable_flags = MF_BYCOMMAND | MF_DISABLED | MF_GRAYED;

					if (IsCopyEnabled())
						m_ContextMenu.EnableMenuItem(ID_BE_COPY, enable_flags);
					else
						m_ContextMenu.EnableMenuItem(ID_BE_COPY, disable_flags);

					if (IsPasteEnabled())
						m_ContextMenu.EnableMenuItem(ID_BE_PASTE, enable_flags);
					else
						m_ContextMenu.EnableMenuItem(ID_BE_PASTE, disable_flags);
				}

				m_ContextMenu.AppendMenu(MF_SEPARATOR, 0, NULL);
			}

			m_ContextMenu.AppendMenu(MF_STRING | MF_ENABLED | (m_bAlternateColor ? MF_CHECKED : 0), IDC_BE_ALTERNATECOLOR, (LPTSTR)(LPCTSTR)_TB("Alternate color"));
		}

		//--------------------------------------
		bOk = OnShowContextMenu(&m_ContextMenu);

		GetDocument()->DispatchOnShowingBodyEditContextMenu(this, &m_ContextMenu, nCol, nCurrRec, point);
	}
	else
	{
		bOk = OnShowCustomContextMenu(&m_ContextMenu);
		if (!bOk)
			bOk = OnShowCustomContextMenu(&m_ContextMenu, nCol, nCurrRec, point);

		GetDocument()->DispatchOnShowingBodyEditContextMenu(this, &m_ContextMenu, nCol, nCurrRec, point);
	}

	// personalizza menu
	if (bOk && m_ContextMenu.GetMenuItemCount() > 0)
	{
		// mosta menu
		m_ContextMenu.TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON, ptMousePos.x, ptMousePos.y, this);
	}

	m_ContextMenu.DestroyMenu();
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::IsCopyEnabled() const //BOOL EnableCopy() const
{
	if (!GetDocument())
		return FALSE;
	switch (GetDocument()->GetFormMode())
	{
	case CBaseDocument::EDIT:
	case CBaseDocument::NEW:
		return GetAllowCopy() && CanDoCopy();
		break;
	case CBaseDocument::BROWSE:
		return GetAllowCopy() && CanDoCopyInBrowse();
		break;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::IsPasteEnabled() //BOOL EnablePaste()
{
	if (!GetAllowPaste())
		return FALSE;

	if (!(
		!GetDocument() ||
		GetDocument()->m_bBatch ||
		GetDocument()->GetFormMode() == CBaseDocument::EDIT ||
		GetDocument()->GetFormMode() == CBaseDocument::NEW
		))
		return FALSE;

	// Cerca, all'interno del codec associato al bodyedit, l'istanza di un
	// codec adatto ad eseguire la corretta scrittura dei dati che potrebbero
	// derivare da un BE diverso da quello corrente.
	CLIPFORMAT		cfClpBrd = 0;
	if (!GetDataCoDec()->FindClipBoardFormat(m_pClpBrdDataCodec, cfClpBrd))
		return FALSE;

	// leggo le info nella clipboard
	COleDataObject	obj;
	if (obj.AttachClipboard())
	{
		if (obj.IsDataAvailable(cfClpBrd))
		{
			CWaitCursor			wait;
			TBEDisableWnd		freeze(GetDocument());

			HGLOBAL hmem = obj.GetGlobalData(cfClpBrd);

			CMemFile sf((BYTE*) ::GlobalLock(hmem), ::GlobalSize(hmem));
			CString		fname;

			LPTSTR str = fname.GetBufferSetLength(::GlobalSize(hmem));
			sf.Read(str, ::GlobalSize(hmem));
			::GlobalUnlock(hmem);
			fname.ReleaseBuffer();

			m_pClpBrdDataCodec->Load(fname, cfClpBrd);

			return (CanDoPaste(m_pClpBrdDataCodec));
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void	CBodyEdit::AttachDataCoDec(CTBEDataCoDec*	pDataCoDec, BOOL bOwn)
{
	if (m_pDataCoDec && m_bOwnDataCoDec)
	{
		delete m_pDataCoDec;
		m_pDataCoDec = NULL;
	}

	m_pDataCoDec = pDataCoDec;
	m_bOwnDataCoDec = bOwn;
}

//-----------------------------------------------------------------------------
void CBodyEdit::EnablePaste(BOOL bEnable/* = TRUE*/, BOOL bSetTool/* = TRUE*/, CTBEDataCoDec* pDataCoDec /*= NULL*/)
{
	BESetExStyle(BE_STYLE_ALLOW_PASTE, bEnable);
	if (bEnable && bSetTool) AttachDataCoDec(pDataCoDec ? pDataCoDec : new CTBEDefaultCoDec(m_pDBT->GetRuntimeClass()), TRUE);
}

//-----------------------------------------------------------------------------
void CBodyEdit::EnableDrop(BOOL bEnable/* = TRUE*/, BOOL bSetTool/* = TRUE*/, CRuntimeClass* pRTDrop/* = NULL*/)
{
	BESetExStyle(BE_STYLE_ALLOW_DROP, bEnable);
	if (bEnable && bSetTool) SetDropTarget(pRTDrop ? pRTDrop : RUNTIME_CLASS(CTBEDefaultDropTargetCoDec));
}

//-----------------------------------------------------------------------------
void	CBodyEdit::DoPasteForDrop(CTBEDataCoDec*	pClpBrdDataCodec, int nIdxRec, DragSelType dstWhere)
{
	ASSERT(pClpBrdDataCodec);
	if (pClpBrdDataCodec)
		OnPaste(pClpBrdDataCodec, nIdxRec, dstWhere);
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnPaste(CTBEDataCoDec*	pClpBrdDataCodec, int nIdxRec, DragSelType dstWhere)
{
	CTBEDefaultCoDec*	pCodec = NULL;
	if (pClpBrdDataCodec->IsKindOf(RUNTIME_CLASS(CTBEDefaultCoDec)))
	{
		pCodec = DYNAMIC_DOWNCAST(CTBEDefaultCoDec, pClpBrdDataCodec);
		if (!pCodec)
		{
			ASSERT(FALSE);
			return;
		}
		m_PasteOrDropTargetCell.gSession = pCodec->GetSession();
		pCodec->LoadRows(GetDocument(), m_pDBT, this, nIdxRec);
	}

	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnMeasureItem(int nIDCtl, LPMEASUREITEMSTRUCT lpMeasureItemStruct)
{
	BOOL setflag = FALSE;
	if (lpMeasureItemStruct->CtlType == ODT_MENU)
	{
		if (m_ContextMenu.IsMenu((HMENU)lpMeasureItemStruct->itemID) && m_ContextMenu.IsMenu((HMENU)lpMeasureItemStruct->itemID))
		{
			m_ContextMenu.MeasureItem(lpMeasureItemStruct);
			return;
		}
	}

	CGridControl::OnMeasureItem(nIDCtl, lpMeasureItemStruct);
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnSwitchDBT(DBTSlaveBuffered* pNewDbt)
{
	SuspendLayout();

	SetDBT(pNewDbt);

	ResumeLayout(FALSE);

	if (m_hWnd)
	{
		UpdateCtrlBody();
		UpdateBodyStatus();
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetDBT(DBTSlaveBuffered* pDBT, BOOL bInitCurrRecord /*= TRUE*/)
{
	if (m_pDBT == pDBT)
		return;
	m_pDBT = pDBT;
	if (m_pDBT)
		m_pDBT->AddBodyEdit(this);

	if (bInitCurrRecord)//FALSE in EasyBuilder in fase di impostazione del databinding
	{
		SetCurrRecord(m_pDBT ? m_pDBT->GetCurrentRowIdx() : -1);
		if (m_pDBT)
			m_pDBT->AlignDBTSlaveToCurrentRow();
	}
	if (m_hWnd)
		InvalidateBody();
}

//-----------------------------------------------------------------------------
CAbstractFormDoc* CBodyEdit::GetDocument()	const
{
	if (m_pDBT)
		return m_pDBT->GetDocument();

	CBaseDocument* pDoc = CGridControlObj::GetDocument();

	ASSERT(pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)));

	return (CAbstractFormDoc*)pDoc;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CBodyEdit::CreateParsedCtrl(
	CString			strName,
	UINT			nIDC,
	DataObj*		pData,
	CRuntimeClass*	pParsedCtrlClass,
	HotKeyLink*		pHotKeyLink			/*= NULL*/,
	UINT			nBtnID				/*= BTN_DEFAULT*/
)
{
	CParsedCtrl* pControl = GetDocument()->DispatchOnCreateParsedCtrl(nIDC, pParsedCtrlClass);
	if (pControl == NULL)
		pControl = OnCreateParsedCtrl(nIDC, pParsedCtrlClass);
	if (pControl == NULL)
	{
		CObject* pO = pParsedCtrlClass->CreateObject();
		pControl = ::GetParsedCtrl(pO);
	}
	if (pControl == NULL)
	{
		TRACE(_T("Failed to create control for column %s, IDC %d"), strName, nIDC);
		ASSERT(FALSE);
		return NULL;
	}

	if (!pControl->GetCtrlCWnd()->IsKindOf(pParsedCtrlClass))
	{
		TRACE(_T("AddColumn %s - IDC %d subclassed from runtime class %s to %s\n"), strName, nIDC, pParsedCtrlClass->m_lpszClassName, pControl->GetCtrlCWnd()->GetRuntimeClass()->m_lpszClassName);
		ASSERT(FALSE);
	}

	pControl->Attach(nBtnID);
	if (pData)
		pControl->Attach(pData);
	pControl->AttachDocument(GetDocument());

	if (pHotKeyLink)
	{
		pHotKeyLink->AttachDocument(GetDocument());
		pControl->AttachHotKeyLink(pHotKeyLink);
	}

	CAddressEdit* pAddressEdit = dynamic_cast<CAddressEdit*>(pControl->GetCtrlCWnd());
	if (pAddressEdit)
	{
		pAddressEdit->SetAddressDlgClass(RUNTIME_CLASS(CAddressDlg));
		pAddressEdit->SetSelectAddressDlgClass(RUNTIME_CLASS(CSelectAddressDlg));
	}

	return pControl;
}

//-----------------------------------------------------------------------------
void CBodyEdit::RemoveColumn(ColumnInfo* pInfo)
{
	m_ColumnsInfo.Remove(pInfo);
	m_AllColumnsInfo.Remove(pInfo);

	if (m_nLayoutSuspended)
		return;

	RecalcColumnsLayout();
	UpdateCtrlBody();
}

//-----------------------------------------------------------------------------
void CBodyEdit::Rename(CString& sName)
{
	CString sOldNamespace = GetInfoOSL()->m_Namespace.ToUnparsedString();
	GetInfoOSL()->m_Namespace.SetObjectName(sName, TRUE);
	CString sNewNamespace = GetInfoOSL()->m_Namespace.ToUnparsedString();

	ColumnInfo* pInfo;
	CString sOldColumnNs;
	CString sNewColumnNs;
	CString sOldCtrlNs;
	CString sNewCtrlNs;

	// oggetti relativi alle colonne
	for (int i = 0; i <= m_AllColumnsInfo.GetUpperBound(); i++)
	{
		pInfo = m_AllColumnsInfo.GetAt(i);
		sOldColumnNs = pInfo->GetNamespace().ToUnparsedString();

		if (sOldColumnNs.Left(sOldNamespace.GetLength()).CompareNoCase(sOldNamespace) != 0)
			continue;

		sNewColumnNs = sNewNamespace + sOldColumnNs.Mid(sOldNamespace.GetLength());

		pInfo->GetInfoOSL()->m_Namespace.SetNamespace(pInfo->GetInfoOSL()->m_Namespace.GetTypeString() + CTBNamespace::GetSeparator() + sNewColumnNs);

		if (!pInfo->GetParsedCtrl())
			continue;

		sOldCtrlNs = pInfo->GetParsedCtrl()->GetNamespace().ToUnparsedString();

		if (sOldCtrlNs.Left(sOldColumnNs.GetLength()).CompareNoCase(sOldColumnNs) != 0)
			continue;

		sNewCtrlNs = pInfo->GetParsedCtrl()->GetNamespace().GetTypeString() + CTBNamespace::GetSeparator() +
			sNewColumnNs +
			sOldCtrlNs.Mid(sOldColumnNs.GetLength());

		pInfo->GetParsedCtrl()->GetNamespace().SetNamespace(sNewCtrlNs);
	}

	// rowformview per ora non gestita
	if (m_pRowFormView)
		ASSERT(FALSE);

	// i vari bottoni accessori legati alle toolbar e alle
	// funzionalità aggiuntive pur ospitando un COSLInfo non
	// utilizzano il campo namespace che viene lasciato sempre vuoto
	// in caso di rename quindi non devo fare niente per questi oggetti
}

//-----------------------------------------------------------------------------
void CBodyEdit::MoveColumnTo(ColumnInfo* pInfo, const int& nIdx)
{
	m_AllColumnsInfo.MoveTo(pInfo, nIdx);

	BodyEditInfo* pBEInfo = GetDocument() && GetDocument()->m_pFormManager ? GetDocument()->m_pFormManager->GetBodyEditInfo(GetNamespace()) : NULL;

	for (int i = 0; i < m_AllColumnsInfo.GetSize(); i++)
	{
		ColumnInfo* pColInfo = m_AllColumnsInfo.GetAt(i);

		BodyEditColumn* pBEInfoCol = pBEInfo ? pBEInfo->GetColumnObject(pColInfo->GetNamespace()) : NULL;
		// rifletto le modifiche apportate da EasyStudio anche sul TBF dove possibile e laddove non 
		// modificato dall'utente
		if (pBEInfoCol && !pBEInfoCol->m_bModified)
			pBEInfoCol->m_nColPos = i;
	}

	if (m_nLayoutSuspended)
		return;
	if (!IsWindow(m_hWnd))
		return;

	// l'array delle colonne visibili è differente, va ricalcolato bene
	RecalcVisibleColumns();
	RecalcColumnsLayout();
	RecalcRowsLayout();
}

//-----------------------------------------------------------------------------
void CBodyEdit::MoveColumnNear(ColumnInfo* pInfo, UINT nBeforeIDC, UINT nAfterIDC/* = 0*/)
{
	ASSERT(nBeforeIDC > 0 || nAfterIDC > 0);

	int nIdx = -1;

	if (nBeforeIDC)
		nIdx = GetAllColumnIdx(nBeforeIDC);

	if (nIdx < 0 && nAfterIDC)
		nIdx = GetAllColumnIdx(nAfterIDC);

	if (nIdx < 0)
	{
		ASSERT_TRACE(FALSE,
			cwsprintf(
				_T("MoveColumnNear does not found column with IDC %d\n"),
				(LPCTSTR)max(nBeforeIDC, nAfterIDC))
		);
		return;
	}

	if (nBeforeIDC && nIdx > 0 && pInfo == m_AllColumnsInfo[nIdx - 1])
		return;
	if (nAfterIDC && nIdx < m_AllColumnsInfo.GetUpperBound() && pInfo == m_AllColumnsInfo[nIdx + 1])
		return;

	MoveColumnTo(pInfo, nIdx);
}

//-----------------------------------------------------------------------------
void CBodyEdit::MoveColumnNear(ColumnInfo* pInfo, DataObj* pData, BOOL bAfter /*= TRUE*/)
{
	ASSERT(pData);

	SqlRecord* pRec = m_pDBT ? m_pDBT->GetRecord() : NULL;
	ASSERT(m_pDBT);
	if (!m_pDBT)
		return;

	int nDataIdx = pRec->Lookup(pData);
	int nIdx = GetColumnIdx(GetColumnFromDataIdx(nDataIdx));

	if (nIdx < 0)
	{
		ASSERT_TRACE(FALSE,
			cwsprintf(
				_T("MoveColumnNear does not found column %s\n"),
				(LPCTSTR)pRec->GetColumnName(nDataIdx)
			));
		return;
	}

	if (bAfter) nIdx++;

	MoveColumnTo(pInfo, nIdx);
}

//-----------------------------------------------------------------------------
BodyEditColumn* CBodyEdit::AddColumnInFormManager(ColumnInfo* pColInfo, BodyEditInfo* pBodyEditInfo, const int& nPos)
{
	BodyEditColumn* pBodyEditColumn = NULL;

	if (pColInfo->GetInfoOSL()->m_Namespace.IsValid())
		pBodyEditColumn = pBodyEditInfo->GetColumnObject(pColInfo->GetInfoOSL()->m_Namespace);

	if (pBodyEditColumn)
	{
		// stato di default imposto dal programmatore
		pBodyEditColumn->m_wDefaultStatus = pColInfo->m_wColumnStatus;
		if (pColInfo->IsStatic())
			pBodyEditColumn->m_wDefaultStatus |= (STATUS_GRAYED | STATUS_NOCHANGE_GRAYED);

		pBodyEditColumn->m_strDefaultColumnTitle = pColInfo->m_strTitle;
		pBodyEditColumn->m_nDefaultColPos = nPos;

		//possibili default 
		if (pBodyEditColumn->m_nColPos == -1)
			pBodyEditColumn->m_nColPos = nPos;

		// stato di corrente parsato da file o modificato interattivamente
		// vedi an. #14272
		if (pBodyEditColumn->m_bStatusChanged)
			pColInfo->m_wColumnStatus = pBodyEditColumn->m_wStatus | (pColInfo->m_wColumnStatus & (STATUS_NOCHANGE_GRAYED | STATUS_NOCHANGE_HIDDEN));
		else
			pBodyEditColumn->m_wStatus = pColInfo->m_wColumnStatus;

		if (!pBodyEditColumn->m_strColumnTitle.IsEmpty())
		{
			if (pBodyEditColumn->m_UICulture.CompareNoCase(AfxGetCulture()) == 0)
				pColInfo->m_strTitle = pBodyEditColumn->m_strColumnTitle;
			else
				pBodyEditColumn->m_strColumnTitle = pColInfo->m_strTitle;
		}
		pColInfo->m_nColPos = pBodyEditColumn->m_nColPos;
	}
	else
	{
		pColInfo->m_nColPos = nPos;

		WORD wStatus = pColInfo->m_wColumnStatus;
		if (pColInfo->IsStatic())
			wStatus |= (STATUS_GRAYED | STATUS_NOCHANGE_GRAYED);

		//ASSERT_VALID(pColInfo->m_pBaseDataObj);

		pBodyEditColumn = new BodyEditColumn
		(
			pColInfo->GetInfoOSL()->m_Namespace,
			pColInfo->m_strTitle,	// current status
			pColInfo->m_strTitle,	// default status
			pColInfo->m_pBaseDataObj ? pColInfo->m_pBaseDataObj->GetDataType() : DataType::String,
			wStatus,	// default status
			wStatus,	// current status
			nPos, nPos,
			0, 0
		);
		pBodyEditInfo->Add(pBodyEditColumn);
	}

	return pBodyEditColumn;
}

//-----------------------------------------------------------------------------
void CBodyEdit::SuspendLayout()
{
	m_nLayoutSuspended++;
	if (m_hWnd && GetDBT() && GetCurrColumn())
		GetCurrColumn()->HideCtrl();
}

//-----------------------------------------------------------------------------
void CBodyEdit::ResumeLayout(BOOL bAlignDBT /*TRUE*/)
{
	m_nLayoutSuspended--;
	ASSERT(m_nLayoutSuspended >= 0);//ResumeLayout chiamata un numero di volte maggiore di SuspendLayout

	if (m_nLayoutSuspended)
		return;
	if (!IsWindow(m_hWnd))
		return;

	DBTSlaveBuffered* pDBT = GetDBT();
	if (
		bAlignDBT &&  pDBT && pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)) &&
		pDBT->GetMaster() && pDBT->GetMaster()->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))
		)
	{
		((DBTSlaveBuffered*)pDBT->GetMaster())->AlignDBTSlaveToCurrentRow();
	}

	// durante la sospensione del layout l'attributo di visible non 
	// ha calcolato nulla visto che le colonne non erano ancora 
	// presenti nei columnInfo, quindi ora faccio scattare un ricalcolo
	// dell'array. Nel frattempo m_AllColumnsInfo è stato riordinato x le colPos
	RecalcVisibleColumns();
	RecalcColumnsLayout();
	RecalcRowsLayout();
	SetVScrollRange();
}

//-----------------------------------------------------------------------------
void CBodyEdit::RecalcVisibleColumns()
{
	m_ColumnsInfo.RemoveAll();
	for (int i = 0; i <= m_AllColumnsInfo.GetUpperBound(); i++)
	{
		ColumnInfo* pColInfo = m_AllColumnsInfo.GetAt(i);
		if (!pColInfo || (pColInfo->m_wColumnStatus & STATUS_HIDDEN) != STATUS_HIDDEN)
		{
			if (pColInfo->m_pParsedCtrl)
			{
				if (pColInfo->m_pParsedCtrl->GetCtrlCWnd() && !pColInfo->m_pParsedCtrl->GetCtrlCWnd()->m_hWnd)
					pColInfo->CreateCtrl(this);
				if (!pColInfo->m_pParsedCtrl->GetCtrlData() && pColInfo->m_pBaseDataObj)
					pColInfo->m_pParsedCtrl->Attach(pColInfo->m_pBaseDataObj);
			}
			m_ColumnsInfo.Add(pColInfo);
		}
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::RecalcColumnsOrder()
{
	m_AllColumnsInfo.Sort();

	if (m_nLayoutSuspended)
		return;
	if (!IsWindow(m_hWnd))
		return;

	RecalcVisibleColumns();
	RecalcColumnsLayout();
	RecalcRowsLayout();
}

//-----------------------------------------------------------------------------
void CBodyEdit::RecalculateAllColPos()
{
	BodyEditInfo* pBEInfo = GetDocument() && GetDocument()->m_pFormManager ? GetDocument()->m_pFormManager->GetBodyEditInfo(GetNamespace()) : NULL;

	for (int i = 0; i < m_AllColumnsInfo.GetSize(); i++)
	{
		ColumnInfo* pColInfo = m_AllColumnsInfo.GetAt(i);

		// le colonne arrivano qui già messe nella corretta posizione da EasyStudio. Se il tbf non l'ha cambiata
		// allora rifletto le modifiche apportate da EasyStudio anche sul TBF dove possibile
	/*	BodyEditColumn* pBEInfoCol = pBEInfo ? pBEInfo->GetColumnObject(pColInfo->GetNamespace()) : NULL;
		if (pBEInfoCol)
		{
			// arriva dal TBF quindi lo devo lasciare così come è oppure mi arriva una colonna 
			// che non e' modificata da nessuno la lascio nella sua posizione
			if (pBEInfoCol->m_bModified && pBEInfoCol->m_nColPos == pBEInfoCol->m_nDefaultColPos)
				continue;


			// altrimenti applico l'ordine di EasyStudio al TBF
			if (pBEInfoCol->m_bModified)
			{
				pColInfo->SetColPos(pBEInfoCol->m_nColPos);
			}
			else
			{
				pBEInfoCol->m_nColPos = i;
				pColInfo->SetColPos(i);
			}
		}*/
	}

	//m_AllColumnsInfo.Sort();
}

//-----------------------------------------------------------------------------
CSlaveViewContainer* CBodyEdit::SetRowViewContainer(UINT nIDC, CWnd* pParentWnd /*=NULL*/)
{
	ASSERT(!m_pRowViewContainer);
	CSlaveViewContainer* pRowViewContainer = new CSlaveViewContainer(nIDC, pParentWnd ? pParentWnd : GetParent(), GetDocument());

	return SetRowViewContainer(pRowViewContainer, TRUE);
}

//-----------------------------------------------------------------------------
CSlaveViewContainer* CBodyEdit::SetRowViewContainer(CSlaveViewContainer* pContainer, BOOL bCallDialog)
{
	ASSERT(!m_pRowViewContainer);
	m_pRowViewContainer = pContainer;
	if (bCallDialog)
		AfxInvokeAsyncThreadFunction<BOOL, CBodyEdit>(GetCurrentThreadId(), this, &CBodyEdit::CallDialog);
	return m_pRowViewContainer;
}

//------------------------------------------------------------------------------
void CBodyEdit::OnDropFiles(const CStringArray&)
{
}

//------------------------------------------------------------------------------
void CBodyEdit::OnDropFiles(HDROP hDropInfo)
{
	CStringArray arDroppedFiles;
	m_SubFolderBehaviour = UNDEFINED_DEEP;

	UINT nFiles = (UINT)::DragQueryFile(hDropInfo, (UINT)-1, NULL, 0);
	if (nFiles > 0)
	{
		for (UINT i = 0; i < nFiles; i++)
		{
			CString strFile;
			UINT nFilenameSize = DragQueryFile(hDropInfo, i, NULL, NULL);
			DragQueryFile(hDropInfo, i, strFile.GetBuffer(nFilenameSize + 1), nFilenameSize + 1);
			strFile.ReleaseBuffer();

			GetAllFiles(arDroppedFiles, strFile);
		}
		OnDropFiles(arDroppedFiles);
	}
	::DragFinish(hDropInfo);
}

// metodo ricorsivo che ritorna tutti i file presenti nel folder + subfolder
//------------------------------------------------------------------------------
void CBodyEdit::GetAllFiles(CStringArray& arDroppedFiles, CString strPath)
{
	DWORD dwFileAttr = GetTbFileAttributes(strPath);
	if (dwFileAttr & FILE_ATTRIBUTE_DIRECTORY)
	{
		CStringArray pSubfolders;
		GetSubFolders(strPath, &pSubfolders); // ritorna SOLO il nome del folder!

		int dirCount = pSubfolders.GetCount();
		if (dirCount > 0 && (m_SubFolderBehaviour == UNDEFINED_DEEP))
			m_SubFolderBehaviour = OnSubFolderFound() ? DEEP : NO_DEEP;

		// vado in ricorsione sui folder figli solo se l'utente mi ha dato l'OK
		if (m_SubFolderBehaviour == DEEP)
		{
			for (int i = 0; i < dirCount; i++)
			{
				// devo comporre il fullpath
				CString fullDirPath = strPath + SLASH_CHAR + pSubfolders.GetAt(i);
				GetAllFiles(arDroppedFiles, fullDirPath);
			}
		}

		CStringArray arFiles;
		GetFiles(strPath, _T("*.*"), &arFiles);
		for (int i = 0; i < arFiles.GetCount(); i++)
			GetAllFiles(arDroppedFiles, arFiles.GetAt(i));
	}
	else
		arDroppedFiles.Add(strPath);
}

//=============================================================================
class CShowStatisticDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CShowStatisticDlg)
public:
	CString		m_sDlgTitle;
	DataStr		m_sStat;

	CShowStatisticDlg(CWnd* aParent = NULL)
		:
		CParsedDialog(IDD_BE_COL_STATISTIC, aParent)
	{
	}

protected:
	CResizableStrEdit m_edtStat;

	virtual BOOL OnInitDialog()
	{
		CParsedDialog::OnInitDialog();
		if (!m_sDlgTitle.IsEmpty())
			SetWindowText(m_sDlgTitle);

		m_edtStat.SubclassEdit(IDC_BE_COL_STAT, this);
		m_edtStat.Attach(&m_sStat);
		m_edtStat.UpdateCtrlStatus();
		m_edtStat.UpdateCtrlView();
		return TRUE;
	}
};
IMPLEMENT_DYNAMIC(CShowStatisticDlg, CParsedDialog)
//-----------------------------------------------------------------------------
void CBodyEdit::OnColumnStatistic()
{
	if (!m_pDBT || m_pDBT->GetUpperBound() < 1)
		return;
	if (m_nColSelected < 0 || m_nColSelected > m_ColumnsInfo.GetUpperBound())
		return;

	LoadAllRows();
	//----
	ColumnInfo* pColInfo = m_ColumnsInfo[m_nColSelected];
	int nIdxData = pColInfo->GetDataInfoIdx();
	DataObj* pBaseObj = pColInfo->GetBaseDataObj();
	DataType dt = pBaseObj->GetDataType();

	BOOL isNumber =
		dt == DataType::Money || dt == DataType::Quantity || dt == DataType::Percent || dt == DataType::Double ||
		dt == DataType::Integer || (dt == DataType::Long && !dt.IsAHandle()) || dt == DataType::ElapsedTime;

	DataObj* pMin = NULL;
	DataObj* pMax = NULL;
	DataObj* pSum = NULL;
	DataObj* pAvg = NULL;

	DataStr ds;
	BOOL bUseDescription = GetCellFormattedText(pColInfo, m_pDBT->GetRow(0), ds);
	if (bUseDescription)
	{
		isNumber = FALSE;
		dt = DataType::String;

		pMin = new DataStr();
		pMax = new DataStr();
		pSum = new DataStr();
		pAvg = new DataStr();
	}
	else
	{
		pMin = pBaseObj->Clone();
		pMax = pBaseObj->Clone();
		pSum = pBaseObj->Clone();
		pAvg = pBaseObj->Clone();
	}

	int nR = 0;
	for (; nR <= m_pDBT->GetUpperBound(); nR++)
	{
		SqlRecord* pRec = m_pDBT->GetRow(nR);

		DataObj* pObj = pRec->GetDataObjAt(nIdxData);

		if (nR == 0)
		{
			if (!bUseDescription)
			{
				*pMin = *pMax = *pSum = *pObj;
			}
			else
			{
				*pMin = *pMax = *pSum = ds;
			}

			continue;
		}

		if (bUseDescription)
		{
			GetCellFormattedText(pColInfo, pRec, ds);
			pObj = &ds;
		}

		if (*pMin > *pObj) *pMin = *pObj;
		if (*pMax < *pObj) *pMax = *pObj;

		if (isNumber)
		{
			if (dt == DataType::Integer)
				*pSum = (*(DataInt*)pSum + *(DataInt*)pObj);
			else if (dt == DataType::Long || dt == DataType::ElapsedTime)
				*pSum = (*(DataLng*)pSum + *(DataLng*)pObj);
			else if (dt == DataType::Money || dt == DataType::Quantity || dt == DataType::Percent || dt == DataType::Double)
				*pSum = (*(DataDbl*)pSum + *(DataDbl*)pObj);
		}
		else if (dt == DataType::String)
			*pSum = (*(DataStr*)pSum + '\r' + '\n' + *(DataStr*)pObj);
	}

	CString sStat;

	if (isNumber)
	{
		if (dt == DataType::Integer)
			*pAvg = (*(DataInt*)pSum / nR);
		else if (dt == DataType::Long)
			*pAvg = (*(DataLng*)pSum / nR);
		else if (dt == DataType::Money || dt == DataType::Quantity || dt == DataType::Percent || dt == DataType::Double)
			*pAvg = (*(DataDbl*)pSum / nR);

		sStat += _TB("Min:") + ' ' + pColInfo->GetParsedCtrl()->FormatData(pMin) + '\r' + '\n';
		sStat += _TB("Avg:") + ' ' + pColInfo->GetParsedCtrl()->FormatData(pAvg) + '\r' + '\n';
		sStat += _TB("Max:") + ' ' + pColInfo->GetParsedCtrl()->FormatData(pMax) + '\r' + '\n';
		sStat += _TB("Sum:") + ' ' + pColInfo->GetParsedCtrl()->FormatData(pSum) + '\r' + '\n';
	}
	else if (dt == DataType::String)
	{
		sStat = *(DataStr*)pSum + '\r' + '\n';
	}
	else
	{
		sStat += _TB("Min:") + ' ' + pColInfo->GetParsedCtrl()->FormatData(pMin) + '\r' + '\n';
		sStat += _TB("Max:") + ' ' + pColInfo->GetParsedCtrl()->FormatData(pMax) + '\r' + '\n';
	}

	//----
	CShowStatisticDlg dlg;
	dlg.m_sStat = sStat;
	if (dt == DataType::String)
		dlg.m_sDlgTitle = _TB("All column texts");
	dlg.DoModal();
	//----
	delete pMin;
	delete pMax;
	delete pSum;
	delete pAvg;
}
//=============================================================================
class CShowSearchAndFiltersDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CShowSearchAndFiltersDlg)
public:
	CString				m_sDlgTitle;
	BOOL				m_bIsSearchDlg;

	CBodyEdit*			m_pBody;
	int					m_nCol;
	ColumnInfo*			m_pColInfo;
	int					m_nDataIdx;

	CButton				m_rbFilterOnRowValues;
	CButton				m_rbFilterOnText;

	BOOL				m_bFilterOnRowValues;
	ECompareType		m_cmpType;

	CResizableStrEdit	m_edtText;
	DataStr				m_sText;

	CResizableComboBox	m_cbxFilterTypes;
	CResizableComboBox	m_cbxColumns;

	CParsedCheckListBox m_chklbValues;
	CStringArray		m_arValues;
	CStringArray		m_arSelected;

	CButton				m_chkCaseSensitive;
	BOOL				m_bCaseSensitive;

	CButton				m_chkSelectAll;
	BOOL				m_bSelectAll;

	CBCGPButton			m_btnOk;
	CBCGPButton			m_btnCancel;

	//---------
	CShowSearchAndFiltersDlg(CBodyEdit* pBody, CWnd* aParent, BOOL bIsSearchDlg)
		:
		CParsedDialog(IDD_BE_SHOW_FILTERS, aParent),
		m_pBody(pBody),
		m_bIsSearchDlg(bIsSearchDlg),
		m_nCol(-1),
		m_pColInfo(NULL),
		m_nDataIdx(-1),
		m_cmpType(ECompareType::CMP_EQUAL),
		m_bFilterOnRowValues(FALSE),
		m_bCaseSensitive(FALSE),
		m_bSelectAll(FALSE)
	{
		ASSERT_VALID(pBody);

		SetColumn(pBody->GetSelectedColumn());
	}

	BOOL SetColumn(int nCol)
	{
		ASSERT_VALID(m_pBody);
		m_pColInfo = NULL;
		m_nDataIdx = -1;
		m_arValues.RemoveAll();

		if (m_chklbValues.m_hWnd)
			m_chklbValues.ResetContent();

		m_nCol = nCol;
		if (m_nCol < 0)
			return FALSE;

		ASSERT(m_nCol >= 0 && m_nCol < m_pBody->GetVisibleColumnsInfoNumber());
		m_pColInfo = m_pBody->GetVisibleColumnFromIdx(m_nCol);
		ASSERT_VALID(m_pColInfo);

		m_nDataIdx = m_pColInfo->GetDataInfoIdx();
		ASSERT(m_nDataIdx >= 0 && m_nDataIdx < m_pBody->GetDBT()->GetRecord()->GetSizeEx());

		return TRUE;
	}

	void EnumValues()
	{
		// già caricato
		if (m_arValues.GetCount() > 0)
			return;

		CWaitCursor wc;
		CParsedCtrl* pCtrl = m_pColInfo->GetParsedCtrl();

		if (m_pColInfo->IsSortedByDescription())
		{
			for (int i = 0; i < m_pBody->GetDBT()->GetSize(); i++)
			{
				SqlRecord* pRec = m_pBody->GetDBT()->GetRow(i);

				CString s; DataObj* pObj;

				if (pCtrl->m_nUseComponentToFormat > -1)
				{
					pObj = pRec->GetDataObjAt(pCtrl->m_nUseComponentToFormat);
					ASSERT_VALID(pObj);

					s = pObj->Str();
				}
				else
				{
					pObj = pRec->GetDataObjAt(m_nDataIdx);
					ASSERT_VALID(pObj);

					s = pCtrl->FormatData(pObj);
				}

				CStringArray_SortInsert(m_arValues, s);
			}

			for (int i = 0; i < m_arValues.GetCount(); i++)
			{
				m_chklbValues.AddString(m_arValues.GetAt(i));
				m_chklbValues.SetCheck(i, 0);
			}
		}
		else
		{
			DataObjArray arValues;
			arValues.SetOwns(FALSE);
			for (int i = 0; i < m_pBody->GetDBT()->GetSize(); i++)
			{
				SqlRecord* pRec = m_pBody->GetDBT()->GetRow(i);

				DataObj* pObj;

				if (pCtrl->m_nUseComponentToFormat > -1)
					pObj = pRec->GetDataObjAt(pCtrl->m_nUseComponentToFormat);
				else
					pObj = pRec->GetDataObjAt(m_nDataIdx);

				ASSERT_VALID(pObj);
				// sort per valore di dataobj
				arValues.SortedInsert(pObj);
			}

			for (int i = 0; i < arValues.GetCount(); i++)
			{
				CString s;
				if (pCtrl->m_nUseComponentToFormat > -1)
					s = arValues.GetAt(i)->Str();
				else
					s = pCtrl->FormatData(arValues.GetAt(i));

				m_arValues.Add(s);
				m_chklbValues.AddString(s);
				m_chklbValues.SetCheck(i, 0);
			}
		}

	}


	void LoadFilterTypes()
	{
		if (m_bFilterOnRowValues)
			::FillCompareType(&m_cbxFilterTypes, DataType::Void);
		else if (!m_pColInfo || m_pColInfo->GetControl()->IsKindOf(RUNTIME_CLASS(CDescriptionCombo)))
			::FillCompareType(&m_cbxFilterTypes, DataType::String);
		else
			::FillCompareType(&m_cbxFilterTypes, m_pColInfo->GetBaseDataObj()->GetDataType());

		if (m_bIsSearchDlg && !m_bFilterOnRowValues)
			::SelectCompareType(&m_cbxFilterTypes, ECompareType::CMP_CONTAINS);
	}

	void LoadColumns()
	{
		int idx = m_cbxColumns.AddString(m_bIsSearchDlg ? CString('<' + _TB("All columns") + '>') : CString('<' + _TB("Select a column...") + '>'));
		m_cbxColumns.SetItemData(idx, -1);

		for (int i = 0; i < m_pBody->GetVisibleColumnsInfoNumber(); i++)
		{
			ColumnInfo* pCol = m_pBody->GetVisibleColumnFromIdx(i);

			int idx = m_cbxColumns.AddString(pCol->GetTitle());

			m_cbxColumns.SetItemData(idx, i);
		}

		if (m_nCol != -1)
			m_cbxColumns.SetCurSel(m_nCol + 1);
		else
		{
			m_cbxColumns.SetCurSel(0);
		}
	}

	void SelectAllValues(BOOL bSet)
	{
		for (int i = 0; i < m_chklbValues.GetCount(); i++)
		{
			m_chklbValues.SetCheck(i, bSet);
		}
	}

	virtual BOOL OnInitDialog()
	{
		__super::OnInitDialog();

		m_edtText.SubclassEdit(IDC_BE_TEXT, this);
		m_cbxColumns.SubclassDlgItem(IDC_BE_SEARCHIN, this);
		m_cbxFilterTypes.SubclassDlgItem(IDC_BE_FILTERS_TYPE, this);
		m_chklbValues.SubclassDlgItem(IDC_BE_FILTERS_MULTISEL, this);

		m_rbFilterOnRowValues.SubclassDlgItem(IDC_BE_FILTER_ON_ROWS, this);
		m_rbFilterOnText.SubclassDlgItem(IDC_BE_FILTER_ON_VALUE, this);
		m_chkSelectAll.SubclassDlgItem(IDC_BE_FILTER_SELECTALL, this);

		m_btnOk.SubclassDlgItem(IDOK, this);
		m_btnCancel.SubclassDlgItem(IDCANCEL, this);

		if (!m_sDlgTitle.IsEmpty())
		{
			SetWindowText(m_sDlgTitle);
		}

		//-------------------------------------------

		m_edtText.Attach(&m_sText);
		m_edtText.UpdateCtrlStatus();
		m_edtText.UpdateCtrlView();

		LoadColumns();

		LoadFilterTypes();

		m_chklbValues.SetCheckStyle(BS_AUTOCHECKBOX);
		m_chklbValues.ResetContent();

		if (m_nCol == -1)
		{
			m_rbFilterOnRowValues.EnableWindow(FALSE);
		}

		CheckDlgButton(IDC_BE_CASE_SENSITIVE, 0);

		m_chkSelectAll.SetCheck(0);

		m_rbFilterOnText.SetCheck(1);
		m_rbFilterOnRowValues.SetCheck(0);

		m_edtText.EnableWindow(TRUE);
		m_chklbValues.EnableWindow(FALSE);
		m_chkSelectAll.EnableWindow(FALSE);

		UINT nIDB = m_pBody->GetAuxToolbarImageIDB(IDOK);
		if (nIDB)
		{
			m_btnOk.SetWindowTextW(L"");
			m_btnOk.SetImage(nIDB);
		}

		nIDB = m_pBody->GetAuxToolbarImageIDB(IDCANCEL);
		if (nIDB)
		{
			m_btnCancel.SetWindowTextW(L"");
			m_btnCancel.SetImage(nIDB);
		}



		return TRUE;
	}

	void OnClickFilterByRows()
	{
		m_bFilterOnRowValues = TRUE;
		m_chklbValues.EnableWindow(TRUE);
		m_edtText.EnableWindow(FALSE);
		m_chkSelectAll.EnableWindow(TRUE);

		LoadFilterTypes();

		EnumValues();
	}

	void OnClickFilterByValue()
	{
		m_bFilterOnRowValues = FALSE;
		m_chklbValues.EnableWindow(FALSE);
		m_edtText.EnableWindow(TRUE);
		m_chkSelectAll.EnableWindow(FALSE);

		LoadFilterTypes();
	}

	void OnClickSelectAll()
	{
		m_bSelectAll = !m_bSelectAll;

		SelectAllValues(m_bSelectAll);
	}

	void OnClickCaseSensitive()
	{
		m_bCaseSensitive = !m_bCaseSensitive;
	}

	void OnChangeColumn()
	{
		int nSel = m_cbxColumns.GetCurSel();
		ASSERT(nSel != CB_ERR);

		SetColumn(nSel - 1);

		LoadFilterTypes();

		m_rbFilterOnText.SetCheck(1);
		m_rbFilterOnRowValues.SetCheck(0);
		m_rbFilterOnRowValues.EnableWindow(m_nCol != -1);
	}

	void OnOK()
	{
		if (m_nCol == -1 && !m_bIsSearchDlg)
			return;

		if (m_bFilterOnRowValues)
		{
			CWaitCursor wc;
			for (int i = 0; i < m_chklbValues.GetCount(); i++)
			{
				if (m_chklbValues.GetCheck(i) == 1)
				{
					CString s;
					m_chklbValues.GetText(i, s);

					m_arSelected.Add(s);
				}
			}
			if (m_arSelected.GetCount() == 0)
			{
				return;
			}
		}

		int nSel = m_cbxFilterTypes.GetCurSel();
		if (nSel >= 0)
			m_cmpType = (ECompareType)m_cbxFilterTypes.GetItemData(nSel);
		else
			m_cmpType = ECompareType::CMP_EQUAL;

		__super::OnOK();
	}

	//void OnCancel()
	//{
	//	__super::OnCancel();
	//}

	DECLARE_MESSAGE_MAP();
};

IMPLEMENT_DYNAMIC(CShowSearchAndFiltersDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CShowSearchAndFiltersDlg, CParsedDialog)

	ON_CBN_SELCHANGE(IDC_BE_SEARCHIN, OnChangeColumn)

	ON_BN_CLICKED(IDC_BE_FILTER_ON_ROWS, OnClickFilterByRows)
	ON_BN_CLICKED(IDC_BE_FILTER_ON_VALUE, OnClickFilterByValue)

	ON_BN_CLICKED(IDC_BE_CASE_SENSITIVE, OnClickCaseSensitive)
	ON_BN_CLICKED(IDC_BE_FILTER_SELECTALL, OnClickSelectAll)

END_MESSAGE_MAP()

///////////////////////////////////////////////////////////////////////////////
void CBodyEdit::EnableSearch(BOOL bSet /*= TRUE*/, BOOL bSetFilter /*= TRUE*/)
{
	BESetExStyle(BE_STYLE_ALLOW_SEARCH, bSet);
	BESetExStyle(BE_STYLE_ALLOW_FILTER, bSetFilter);

	//if (m_pBtnSearch && m_pBtnSearch->m_hWnd)
	//	m_pBtnSearch->SetEnableState(bSet || bSetFilter, FALSE);
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnDoSearch()
{
	if (!m_pDBT || m_pDBT->GetSize() == 0)
		return;

	if (m_arSearchText.IsEmpty())
	{
		CBEButton* pB = m_HeaderToolBar.FindButton(IDC_BE_SEARCH_TEXT);
		if (!pB)
			pB = m_FooterToolBar.FindButton(IDC_BE_SEARCH_TEXT);
		if (pB)
		{
			ASSERT_KINDOF(CBEBtnCtrl, pB);

			CBEBtnCtrl* pBtnSearchText = dynamic_cast<CBEBtnCtrl*>(pB);
			ASSERT_VALID(pBtnSearchText->m_pControl);

			CString s;
			GetParsedCtrl(pBtnSearchText->m_pControl)->GetValue(s);
			s.Trim();

			if (s.IsEmpty())
			{
				OnShowSearch();
				return;
			}

			m_arSearchText.Add(s);

			m_cmpType = ECompareType::CMP_CONTAINS;
		}
		else
		{
			OnShowSearch();
			return;
		}
	}

	ColumnInfo* pCol = NULL;
	int nCol = -1;

	if (m_nSearchColumnIDC)
	{
		nCol = this->GetVisibleColumnIdx(m_nSearchColumnIDC);
		ASSERT(nCol >= 0);
		if (nCol >= 0)
			pCol = m_ColumnsInfo[nCol];
	}

	if (DoSearch(pCol, m_arSearchText))
	{
		// imposto la nuova riga corrente
		SetCurrLine(m_nFoundRow);

		if (nCol == -1)
		{
			nCol = m_nFoundColumn;
			if (nCol >= 0)
				pCol = m_ColumnsInfo[nCol];
		}

		if (nCol > -1 && pCol && ShowColumnCtrl(nCol) && ShowCtrl(ResetToVisiblePosition()))
		{
			// evidenzio la stringa trovata
			CParsedCtrl* pCtrl = pCol->GetParsedCtrl();
			if (
				m_arSearchText.GetCount() == 1
				&&
				(
					m_cmpType == ECompareType::CMP_EQUAL ||
					m_cmpType == ECompareType::CMP_BEGIN_WITH ||
					m_cmpType == ECompareType::CMP_CONTAINS ||
					m_cmpType == ECompareType::CMP_END_WITH
					)
				)
			{
				CString s; DataObj* pObj;

				if (pCtrl->m_nUseComponentToFormat > -1)
				{
					pObj = GetDBT()->GetCurrentRow()->GetDataObjAt(pCtrl->m_nUseComponentToFormat);
					ASSERT_VALID(pObj);

					s = pObj->Str();
				}
				else
				{
					pObj = GetDBT()->GetCurrentRow()->GetDataObjAt(pCol->m_nDataInfoIdx);
					ASSERT_VALID(pObj);

					s = pCtrl->FormatData(pObj);
				}

				s.MakeUpper(); CString ss(m_arSearchText.GetAt(0)); ss.MakeUpper();
				int pos = s.Find(ss);
				if (pos > -1)
					pCtrl->SetCtrlSel(pos, pos + m_arSearchText.GetAt(0).GetLength());
			}
		}
	}
	else
	{
		if (AfxMessageBox(_TB("The last row reached. Restart the search from the first ?"), MB_YESNO) == IDYES)
		{
			ResetSearchPoint(FALSE);

			SetCurrLine(0);

			ShowCtrl(ResetToVisiblePosition());

			OnDoSearch();
		}
		else
			ResetSearchPoint(TRUE);
	}
}

//-----------------------------------------------------------------------------
void CBodyEdit::ResetSearchPoint(BOOL bAll)
{
	m_nFoundColumn = m_nFoundRow = -1;

	if (!bAll)
		return;

	m_cmpType = ECompareType::CMP_EQUAL;
	m_arSearchText.RemoveAll();
	m_nSearchColumnIDC = 0;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::GetCellFormattedText(ColumnInfo* pCol, SqlRecord* pRec, DataStr& ds)
{
	ASSERT_VALID(pCol);
	ASSERT_VALID(pRec);

	if (pCol->IsSortedByDescription())
	{
		DataObj* pObj;
		if (pCol->GetParsedCtrl()->m_nUseComponentToFormat > -1)
		{
			pObj = pRec->GetDataObjAt(pCol->GetParsedCtrl()->m_nUseComponentToFormat);
			ASSERT_VALID(pObj);

			ds = pObj->Str();
		}
		else
		{
			pObj = pRec->GetDataObjAt(pCol->GetDataInfoIdx());
			ASSERT_VALID(pObj);

			ds = pCol->GetParsedCtrl()->FormatData(pObj);
		}
#ifdef _DEBUG
		ds += cwsprintf(L" [%s]", pObj->Str());
#endif
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::DoSearch(ColumnInfo* pCol, const CStringArray& sCmpValues, ECompareType cmpType /*= ECompareType::CMP_CONTAINS*/)
{
	int nColMax = this->GetVisibleColumnsInfoNumber();

	if (m_nFoundRow == (m_pDBT->GetSize() - 1))
	{
		// we already are on the last row,
		// return false in order to ask the user if 
		// we need to go back on the first row for 
		// the next search.
		if (pCol)
		{
			m_nFoundRow = -1;
			return FALSE;
		}
		else if ((m_nFoundColumn + 1) == nColMax)
		{
			m_nFoundRow = -1;
			m_nFoundColumn = -1;
			return FALSE;
		}
	}

	int startrow = (m_nFoundRow > -1) ? (m_nFoundRow + (pCol ? 1 : 0)) : m_nCurrRecordIdx;
	m_nFoundRow = -1;

	if (startrow < 0 || startrow == m_pDBT->GetSize())
		startrow = 0;

	for (int nR = startrow; nR <= m_pDBT->GetUpperBound(); nR++)
	{
		SqlRecord* pRec = m_pDBT->GetRow(nR);

		if (pCol)
		{
			ASSERT_VALID(pCol);
			DataStr ds;
			if (GetCellFormattedText(pCol, pRec, ds))
			{
				if (ds.CompareBy(cmpType, sCmpValues))
				{
					m_nFoundRow = nR;
					return TRUE;
				}
			}
			else
			{
				if (pRec->CompareFieldBy(pCol->GetDataInfoIdx(), cmpType, sCmpValues))
				{
					m_nFoundRow = nR;
					return TRUE;
				}
			}
		}
		else
		{
			ASSERT(sCmpValues.GetCount() == 1);
			for (int c = m_nFoundColumn + 1; c < nColMax; c++)
			{
				ColumnInfo* pC = this->m_ColumnsInfo[c];

				DataStr ds;
				if (GetCellFormattedText(pC, pRec, ds))
				{
					DataStr dsSub(sCmpValues[0]);

					if (ds.CompareBy(cmpType, &dsSub))
					{
						m_nFoundRow = nR;
						m_nFoundColumn = c;
						return TRUE;
					}
				}
				else
				{
					if (pRec->CompareFieldBy(pC->GetDataInfoIdx(), cmpType, sCmpValues))
					{
						m_nFoundRow = nR;
						m_nFoundColumn = c;
						return TRUE;
					}
				}
			}
			m_nFoundColumn = -1;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnShowSearch()
{
	if (!m_pDBT || m_pDBT->GetSize() == 0)
		return;
	//----

	LoadAllRows();

	//----
	CShowSearchAndFiltersDlg dlg(this, this, TRUE);

	dlg.m_sDlgTitle = _TB("Search");

	if (m_arSearchText.GetCount() == 1)
		dlg.m_sText = m_arSearchText.GetAt(0);

	ResetSearchPoint(TRUE);

	if (dlg.DoModal() == IDOK)
	{
		m_nSearchColumnIDC = dlg.m_pColInfo ? dlg.m_pColInfo->GetControl()->GetDlgCtrlID() : 0;

		m_cmpType = dlg.m_cmpType;

		if (dlg.m_bFilterOnRowValues)
		{
			m_arSearchText.Copy(dlg.m_arSelected);
		}
		else
		{
			if (!dlg.m_sText.IsEmpty())
				m_arSearchText.Add(dlg.m_sText);
			else
			{
				ResetSearchPoint(TRUE);
				return;
			}
		}

		OnDoSearch();
	}
	else
		ResetSearchPoint(TRUE);
}

//-----------------------------------------------------------------------------
void CBodyEdit::DoRemoveFilter(BOOL bRefreshBody/* = TRUE*/, int nColSelected/* = -1*/)
{
	if (!GetDBT())
		return;
	ASSERT_VALID(GetDBT());

	ColumnInfo* pCol = GetVisibleColumnFromIdx(nColSelected);

	GetDBT()->RemoveMemoryFilter(bRefreshBody, DBTSlaveBuffered::REMOVE_FILTER_UI, (pCol ? pCol->GetDataInfoIdx() : -1));

	m_nCurrRecordIdx = -1;

	ResetSearchPoint(TRUE);	//sono cambiate le righe
}

void CBodyEdit::OnRemoveColumnFilter()
{
	DoRemoveFilter(TRUE, m_nColSelected);
}

void CBodyEdit::OnRemoveAllFilters()
{
	DoRemoveFilter(TRUE);
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnShowFilters()
{
	if (!GetDBT() || GetDBT()->GetSize() == 0)
		return;
	if (m_nColSelected < 0 || m_nColSelected > m_ColumnsInfo.GetUpperBound())
		return;

	//----
	CShowSearchAndFiltersDlg dlg(this, this, FALSE);
	dlg.m_sDlgTitle = _TB("Filter rows");

	if (dlg.DoModal() == IDOK)
	{
		ASSERT_VALID(dlg.m_pColInfo);
		ASSERT_VALID(dlg.m_pColInfo->GetControl());

		int nFilteredColumnIDC = dlg.m_pColInfo ? dlg.m_pColInfo->GetControl()->GetDlgCtrlID() : 0;
		if (nFilteredColumnIDC)
		{
			ASSERT_VALID(GetDBT());
			CWaitCursor wc;

			LoadAllRows();

			//DoRemoveFilter(TRUE);
			GetDBT()->SetAllowFilter(TRUE);

			CParsedCtrl* pCtrl = dlg.m_pColInfo ? dlg.m_pColInfo->GetParsedCtrl() : NULL;

			if (dlg.m_bFilterOnRowValues)
			{
				GetDBT()->MemoryFilter(dlg.m_nDataIdx, dlg.m_arSelected, dlg.m_cmpType, pCtrl);
			}
			else
			{
				GetDBT()->MemoryFilter(dlg.m_nDataIdx, dlg.m_sText, dlg.m_cmpType, pCtrl);
			}
		}
	}
}

///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
void CBodyEdit::EnableButton(UINT id, BOOL bEnable)
{
	this->m_HeaderToolBar.EnableButton(id, bEnable);
	this->m_FooterToolBar.EnableButton(id, bEnable);
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::IsDuplicateButton(UINT nID)
{
	if (m_HeaderToolBar.FindButton(nID))
		return TRUE;
	if (m_FooterToolBar.FindButton(nID))
		return TRUE;
	/*
		if (nID == IDC_BE_INSERT)
		{
		if (this->m_pBtnInsert)
		return TRUE;
		}
		else if (nID == IDC_BE_DELETE){
		if (this->m_pBtnDelete)
		return TRUE;
		}
		else if (nID == IDC_BE_CALLDIALOG){
		if (this->m_pBtnCallDialog)
		return TRUE;
		}
		else if (nID == IDC_BE_MULTISELECTION)
		{
		if (this->m_pBtnMultiSelection)
		return TRUE;
		}

		else if (nID == IDC_BE_SEARCH)
		{
		if (this->m_pBtnSearch)
		return TRUE;
		}
		else if (nID == IDC_BE_SEARCH_TEXT)
		{
		}
		else if (nID == IDC_BE_GOHOME)
		{
		if (this->m_pBtnGoHome)
		return TRUE;
		}
		else if (nID == IDC_BE_GOEND)
		{
		if (this->m_pBtnGoEnd)
		return TRUE;
		}
		else if (nID == IDC_BE_ROW_HEIGHT_PLUS)
		{
		//if (this->m_pBtnSpinRows)
		//	return TRUE;
		}
		else if (nID == IDC_BE_ROW_HEIGHT_MINUS)
		{
		}

		else if (nID == IDC_BE_ROWINDICATOR)
		{
		if (this->m_pRowIndicator)
		return TRUE;
		}
		*/
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CBodyEdit::GetImageIDB(UINT nID, UINT& nIDB_up, UINT& nIDB_down, UINT& nIDB_disable)
{
	UINT nIDB = 0;
	/*
		if (nID == IDC_BE_GOHOME)
		{
		nIDB_up = IDB_BODYEDIT_HOMEU;		nIDB_down = IDB_BODYEDIT_HOMED;			nIDB_disable = IDB_BODYEDIT_HOMEDI;
		return TRUE;
		}
		else if (nID == IDC_BE_GOEND)
		{
		nIDB_up = IDB_BODYEDIT_ENDU;		nIDB_down = IDB_BODYEDIT_ENDD;			nIDB_disable = IDB_BODYEDIT_ENDDI;
		return TRUE;
		}
		else if (nID == IDC_BE_INSERT)
		{
		nIDB_up = IDB_BODYEDIT_INSERTU;		nIDB_down = IDB_BODYEDIT_INSERTD;		nIDB_disable = IDB_BODYEDIT_INSERTDI;
		return TRUE;
		}
		else if (nID == IDC_BE_DELETE)
		{
		nIDB_up = IDB_BODYEDIT_DELETEU;		nIDB_down = IDB_BODYEDIT_DELETED;		nIDB_disable = IDB_BODYEDIT_DELETEDI;
		return TRUE;
		}
		else if (nID == IDC_BE_CALLDIALOG)
		{
		nIDB_up = IDB_BODYEDIT_CALLDLGU;	nIDB_down = IDB_BODYEDIT_CALLDLGD;		nIDB_disable = IDB_BODYEDIT_CALLDLGDI;
		return TRUE;
		}
		else if (nID == IDC_BE_SEARCH)
		{
		nIDB_up = IDB_BODYEDIT_FIND_UP;		nIDB_down = IDB_BODYEDIT_FIND_DOWN;		nIDB_disable = IDB_BODYEDIT_FIND_DIS;
		return TRUE;
		}
		else if (nID == IDC_BE_MULTISELECTION)
		{
		nIDB_up = IDB_BODYEDIT_MULTISEL_UP;	nIDB_down = IDB_BODYEDIT_MULTISEL_DOWN;	nIDB_disable = IDB_BODYEDIT_MULTISEL_DIS;
		return TRUE;
		}
		*/
	ASSERT(FALSE);
	return FALSE;
}

//-----------------------------------------------------------------------------
UINT CBodyEdit::ReplaceImageIDB(UINT nIDB)
{
	return nIDB;
}

//-----------------------------------------------------------------------------
UINT CBodyEdit::GetAuxToolbarImageIDB(UINT nID)
{
	/*
		if (nID == IDC_BE_INSERT)
		return IDB_BODYEDIT_INSERTU;
		else if (nID == IDC_BE_DELETE)
		return IDB_BODYEDIT_DELETEU;
		else if (nID == IDC_BE_CALLDIALOG)
		return IDB_BODYEDIT_CALLDLGU;
		else if (nID == IDC_BE_MULTISELECTION)
		return IDB_BODYEDIT_MULTISEL_UP;
		else if (nID == IDC_BE_SEARCH)
		return IDB_BODYEDIT_FIND_UP;
		else if (nID == IDC_BE_GOHOME)
		return IDB_BODYEDIT_HOMEU;
		else if (nID == IDC_BE_GOEND)
		return IDB_BODYEDIT_ENDU;
		else if (nID == IDC_BE_ROW_HEIGHT_PLUS)
		return IDB_BODYEDIT_HEIGHT_PLUS;
		else if (nID == IDC_BE_ROW_HEIGHT_MINUS)
		return IDB_BODYEDIT_HEIGHT_MINUS;
		*/
		//ASSERT(FALSE);
	return 0;
}

//-----------------------------------------------------------------------------
void CBodyEdit::DrawAuxToolBars(CDC* pDC, LPDRAWITEMSTRUCT /*lpDIS*/, CRect& rect)
{
	COLORREF crBackgnd = AfxGetThemeManager()->GetBackgroundColor();
	COLORREF crForegnd = AfxGetThemeManager()->GetButtonFaceForeColor();

	CPen		pen(PS_SOLID, 1, crForegnd);
	CPen*		pOldPen = pDC->SelectObject(&pen);
	int			nOldBkMode = pDC->SetBkMode(TRANSPARENT);
	COLORREF	crOldBackgnd = pDC->SetBkColor(crBackgnd);
	COLORREF	crOldForegnd = pDC->SetTextColor(crForegnd);
	CPen		pShdw(PS_SOLID, 1, AfxGetThemeManager()->GetBEToolbarBtnShadowColor());
	CPen		pLght(PS_SOLID, 1, AfxGetThemeManager()->GetBEToolbarBtnHighlightColor());

	FillRect(pDC, rect, crBackgnd);

	pDC->SelectObject(&pLght);
	DrawSepLine
	(
		pDC,
		rect.left,
		rect.top,
		rect.right,
		rect.top
	);
	DrawSepLine
	(
		pDC,
		rect.left,
		rect.top,
		rect.left,
		rect.bottom - 1
	);

	pDC->SelectObject(&pShdw);
	DrawSepLine
	(
		pDC,
		rect.left,
		rect.bottom - 1,
		rect.right,
		rect.bottom - 1
	);

	DrawSepLine
	(
		pDC,
		rect.right - 1,
		rect.top,
		rect.right - 1,
		rect.bottom - 1
	);

	// restores the old values for graphic device
	pDC->SetTextColor(crOldForegnd);
	pDC->SetBkColor(crOldBackgnd);
	pDC->SetBkMode(nOldBkMode);
	pDC->SelectObject(pOldPen);

	pen.DeleteObject();
}

//-----------------------------------------------------------------------------
CBEButton* CBodyEdit::AddAuxToolBarButton(const CString& sBtnName, int nIDC, int nIDB, const CString& stooltip)
{
	return m_HeaderToolBar.AddButton(sBtnName, nIDC, nIDB, stooltip);
}

//-----------------------------------------------------------------------------
CBEButton* CBodyEdit::AddAuxToolBarButton(const CString& sName, UINT nID, CString strNsImage, CString strTooltip, CString strText /* = L""*/, int	nIndex /*= -1*/)
{
	return m_HeaderToolBar.AddButton(sName, nID, strNsImage, strTooltip, strText, nIndex);
}

//-----------------------------------------------------------------------------
void CBodyEdit::BeforeCustomize()
{
	OnBeforeCustomize();

	if (BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR))
	{
		//footer
		if (BEGetExStyle(BE_STYLE_ALLOW_INSERT))
			m_FooterToolBar.AddButton(_NS_BE_TOOLBAR_BTN("Insert"), IDC_BE_INSERT, TBIcon(szIconAdd, MINI), _TB("Insert before current row"));
		if (BEGetExStyle(BE_STYLE_ALLOW_DELETE))
			m_FooterToolBar.AddButton(_NS_BE_TOOLBAR_BTN("Delete"), IDC_BE_DELETE, TBIcon(szIconDelete, MINI), _TB("Delete current row"));
		if (BEGetExStyle(BE_STYLE_ALLOW_CALLDIALOG) && this->m_pFormViewClass)
			m_FooterToolBar.AddButton(_NS_BE_TOOLBAR_BTN("ShowRowView"), IDC_BE_CALLDIALOG, TBIcon(szIconRowView, MINI), _TB("Open rowview window"));
		if (BEGetExStyle(BE_STYLE_ALLOW_MULTIPLE_SEL))
			m_FooterToolBar.AddButton(_NS_BE_TOOLBAR_BTN("MultiSelection"), IDC_BE_MULTISELECTION, TBIcon(szIconMultiSelect, MINI), _TB("Multiple selections"));
		if (BEGetExStyle(BE_STYLE_SHOW_ALTERNATE_COLOR))
			m_FooterToolBar.AddButton(_NS_BE_TOOLBAR_BTN("AlternateColor"), IDC_BE_ALTERNATECOLOR, TBIcon(szIconEasyReadingOff, MINI), _TB("Alternate color"));

		if (BEGetExStyle(BE_STYLE_ALLOW_SEARCH))
		{
			m_FooterToolBar.AddControl(RUNTIME_CLASS(CStrEdit), _NS_BE_TOOLBAR_BTN("SearchText"), IDC_BE_SEARCH_TEXT, _TB("Search text on all columns..."), L"", 100);
			m_FooterToolBar.AddButton(_NS_BE_TOOLBAR_BTN("Search"), IDC_BE_SEARCH, TBIcon(szIconFind, MINI), _TB("Find..."));
		}

		if (AfxGetThemeManager()->GetBodyEditScrollBarInToolBar())
		{
			CBEBtnCtrl* pHScroll = m_FooterToolBar.AddControl(RUNTIME_CLASS(CTBScrollBar), _NS_BE_TOOLBAR_BTN("HScrollbar"), IDC_BE_HSCROLLBAR, _TB("HScrollbar"), L"", 200);
			pHScroll->SetAutoSize();
		}

		m_FooterToolBar.m_bDefaultRightAlign = TRUE;

		m_FooterToolBar.AddButton(_NS_BE_TOOLBAR_BTN("RowHeightPlus"), IDC_BE_ROW_HEIGHT_PLUS, TBIcon(szIconEnlarge, MINI), _TB("Enlarge row height"));
		m_FooterToolBar.AddButton(_NS_BE_TOOLBAR_BTN("RowHeightMinus"), IDC_BE_ROW_HEIGHT_MINUS, TBIcon(szIconShrink, MINI), _TB("Shrink row height"));
		m_FooterToolBar.AddButton(_NS_BE_TOOLBAR_BTN("GoHome"), IDC_BE_GOHOME, TBIcon(szIconTop, MINI), _TB("Go to first row"));
		m_FooterToolBar.AddButton(_NS_BE_TOOLBAR_BTN("GoEnd"), IDC_BE_GOEND, TBIcon(szIconBottom, MINI), _TB("Go to last row"));

		CBEBtnCtrl* pRowInd = m_FooterToolBar.AddControl(RUNTIME_CLASS(CStrEdit), _NS_BE_TOOLBAR_BTN("RowCounter"), IDC_BE_ROWINDICATOR, _TB("Row counter indicator"), L"", 80);
		pRowInd->EnableButton(FALSE);

		m_FooterToolBar.m_bDefaultRightAlign = FALSE;
	}
}

//-----------------------------------------------------------------------------
int CBodyEdit::GetMinHeight()
{
	// minimum height of bodyEdit (For Tile)
	int nHeight = m_nRowsHeight * 3;
	// Add tiles height
	nHeight += m_rectTitles.Height();
	// Add Footer
	if (BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR) && m_FooterToolBar.GetSize() > 0)
	{
		nHeight += m_FooterToolBar.m_rectToolBar.Height();
	}
	// Add Footer
	if (BEGetExStyle(BE_STYLE_SHOW_HEADER_TOOLBAR) && m_HeaderToolBar.GetSize() > 0)
	{
		nHeight += m_HeaderToolBar.m_rectToolBar.Height();
	}
	return nHeight;
}

//-----------------------------------------------------------------------------
void CBodyEdit::OnUpdateControls(BOOL bParentIsVisible)
{
	DBTSlaveBuffered* pDBT = GetDBT();

	if (pDBT && (pDBT->IsModified()))
	{
		UpdateBodyStatus();
		UpdateCtrlBody();
	}

	m_HeaderToolBar.OnUpdateControls(bParentIsVisible);
	m_FooterToolBar.OnUpdateControls(bParentIsVisible);
}

//-----------------------------------------------------------------------------
void CBodyEdit::SetDataModified(BOOL bMod)
{
	DBTSlaveBuffered* pDBT = GetDBT();
	if (pDBT)
		pDBT->SetModified(bMod);
}

//-----------------------------------------------------------------------------
// Diagnostics
#ifdef _DEBUG

void CBodyEdit::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nCBodyEdit :");
	AFX_DUMP1(dc, "\n\tID = ", GetDlgCtrlID());
}

void CBodyEdit::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CBEButton, CBCGPMenuButton)

BEGIN_MESSAGE_MAP(CBEButton, CBCGPMenuButton)

	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
	ON_WM_SETFOCUS()

END_MESSAGE_MAP()

CBEButton::CBEButton
(
	CBEToolbar*			pToolBar,
	const CString&		sName,
	int					nWidth,
	UINT				nID,
	UINT				nIDB,
	DWORD				dwBtnStyle,
	const CString&		strTooltip,
	const CString&		strText
)
	:
	IDisposingSourceImpl(this),
	IOSLObjectManager(OSLType_ToolbarButton),
	m_pToolBar(pToolBar),
	m_sName(sName),
	m_nWidth(nWidth),
	m_nHeight(0),
	m_nID(nID),
	m_nIDB(nIDB),
	m_dwStyle(dwBtnStyle),
	m_strTooltip(strTooltip),
	m_strText(strText),
	m_nsImage(CTBNamespace::IMAGE),
	m_bHiddenText(FALSE),
	m_bDisableOnReadOnly(FALSE),
	m_bRightAlign(FALSE),
	m_bEnabled(TRUE),
	m_bVisible(TRUE),
	m_bLeftButtonDown(FALSE)
{
	m_bOSMenu = FALSE;
	m_bVisualManagerStyle = TRUE;

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

CBEButton::CBEButton
(
	CBEToolbar*			pToolBar,
	const CString&		sName,
	int					nWidth,
	UINT				nID,
	const CString&		strNsImage,
	DWORD				dwBtnStyle,
	const CString&		strTooltip,
	const CString&		strText
)
	:
	IDisposingSourceImpl(this),
	IOSLObjectManager(OSLType_ToolbarButton),
	m_pToolBar(pToolBar),
	m_sName(sName),
	m_nWidth(nWidth),
	m_nHeight(0),
	m_nID(nID),
	m_nIDB(0),
	m_dwStyle(dwBtnStyle),
	m_strTooltip(strTooltip),
	m_strText(strText),
	m_nsImage(strNsImage),
	m_bHiddenText(FALSE),
	m_bDisableOnReadOnly(FALSE),
	m_bRightAlign(FALSE),
	m_bEnabled(TRUE),
	m_bVisible(TRUE),
	m_bLeftButtonDown(FALSE)
{
	m_bOSMenu = FALSE;

	m_bVisualManagerStyle = TRUE;

	ASSERT(m_nsImage.IsEmpty() || m_nsImage.IsValid());

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CBEButton::~CBEButton()
{
	if (m_mapMenuImages.GetCount())
	{
		POSITION pos = m_mapMenuImages.GetStartPosition();
		while (pos)
		{
			CBitmap *pImg(NULL);
			UINT key;
			m_mapMenuImages.GetNextAssoc(pos, key, pImg);
			if (pImg) delete pImg;
		}
		m_mapMenuImages.RemoveAll();
	}

	for (int i = m_CInfoOSLs.GetCount() - 1; i >= 0; i--)
		delete m_CInfoOSLs.GetAt(i);

	m_CInfoOSLs.RemoveAll();

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//-----------------------------------------------------------------------------
HRESULT CBEButton::get_accName(VARIANT varChild, BSTR *pszName)
{
	//IOSLObjectManager* temp = dynamic_cast<IOSLObjectManager*>(this);
	//if (!temp)
	//{
	//	ASSERT(FALSE);
	//	return S_OK;
	//}

	//GetInfoOSL()->m_Namespace

	//*pszName = temp->GetObjectNamespace();

	CString sNamespace = cwsprintf(_T("{0-%s}{1-%s}"), GetInfoOSL()->m_Namespace.GetObjectName(), GetInfoOSL()->m_Namespace.GetTypeString());
	*pszName = ::SysAllocString(sNamespace);

	return S_OK;
}

//-----------------------------------------------------------------------------
CBodyEdit* CBEButton::GetBody()
{
	ASSERT_VALID(m_pToolBar);
	ASSERT_VALID(m_pToolBar->m_pBody);
	return m_pToolBar->m_pBody;
}

//-----------------------------------------------------------------------------
BOOL CBEButton::Create(CWnd* pParentBody)
{
	ASSERT(m_nID);
	ASSERT_VALID(pParentBody);
	ASSERT(pParentBody->m_hWnd);

	if (!__super::Create(m_strText, m_dwStyle, CRect(0, 0, 0, 0), pParentBody, m_nID))
		return FALSE;

	SetFaceColor(AfxGetThemeManager()->GetBackgroundColor(), FALSE);
	m_clrHover = AfxGetThemeManager()->GetTabSelectorHoveringForeColor();

	if (!AfxIsInUnattendedMode())
	{
		if (m_nsImage.IsValid())
		{
			// caricamento dell'icona via namespace e del calcola della icona disattivata
			ChangeImage(0, m_nsImage.ToString());
		}
		else if (m_nIDB)
		{
			ChangeImage(m_nIDB);
		}
		SetTooltip(m_strTooltip);
	}

	SetWindowText(m_strText);

	SetFont(AfxGetThemeManager()->GetFormFont(m_pToolBar->m_pBody->m_bUseColumnHeaderSmallFont));

	//if (AfxGetThemeManager()->UseFlatStyle())
	m_nFlatStyle = CBCGPButton::BUTTONSTYLE_NOBORDERS;
	//else
	//	m_nFlatStyle = CBCGPButton::BUTTONSTYLE_SEMIFLAT;

	ASSERT(m_hWnd);

	EnableWindow(m_bEnabled ? TRUE : FALSE);

	ShowWindow(m_bVisible ? SW_NORMAL : SW_HIDE);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBEButton::SetText(const CString& sText)
{
	m_strText = sText;

	if (m_hWnd)
	{
		SetWindowText(m_strText);

		m_pToolBar->SetPos();
		m_pToolBar->Redraw();
	}
}

//-----------------------------------------------------------------------------
void CBEButton::SetTooltip(const CString& sTooltip)
{
	m_strTooltip = sTooltip;

	if (m_pToolBar->m_pBody->GetDocument())
	{
		CString sAcc = m_pToolBar->m_pBody->GetDocument()->GetDocAccelText(m_nID);
		if (!sAcc.IsEmpty() && m_strTooltip.Find(sAcc) < 0)
		{
			m_strTooltip += sAcc;
		}
	}

	if (m_hWnd && !AfxIsRemoteInterface())
	{
		__super::SetTooltip(m_strTooltip);
	}
}


//-----------------------------------------------------------------------------
BOOL CBEButton::ChangeImage(UINT nIDB, const CString& nsImage/* = L""*/)
{
	ASSERT_VALID(m_pToolBar);
	return ::LoadBitmapOrPng(this, nsImage, nIDB, m_pToolBar->m_pBody);
}

//-----------------------------------------------------------------------------
BOOL CBEButton::ChangeImage(const CString& nsImage)
{
	if (nsImage.IsEmpty())
	{
		return FALSE;
	}
	return ChangeImage(0, nsImage);
}

//-----------------------------------------------------------------------------
BOOL CBEButton::SetCheckImage(const CString& nsImage)
{
	if (nsImage.IsEmpty())
	{
		return FALSE;
	}
	return SetCheckImage(0, nsImage);
}

//-----------------------------------------------------------------------------
BOOL CBEButton::SetCheckImage(UINT nIDB, const CString& nsImage/* = L""*/)
{
	ASSERT_VALID(m_pToolBar);
	return ::LoadBitmapOrPng(this, nsImage, nIDB, m_pToolBar->m_pBody, TRUE);
}

//-----------------------------------------------------------------------------
void CBEButton::ShowButton(BOOL b)
{
	m_bVisible = b;

	if (m_hWnd)
	{
		this->ShowWindow(b ? SW_NORMAL : SW_HIDE);
	}
}

//-----------------------------------------------------------------------------
void CBEButton::EnableButton(BOOL b)
{
	//m_bLeftButtonDown = FALSE;	//TAPPULLO

	BOOL bDocReadOnly = FALSE;

	if (m_pToolBar->m_pBody->GetDocument())
	{
		if (m_pToolBar->m_pBody->GetDocument()->GetFormMode() == CBaseDocument::NEW && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_NEW))
		{
			b = FALSE;
		}
		else if (m_pToolBar->m_pBody->GetDocument()->GetFormMode() == CBaseDocument::EDIT && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EDIT))
		{
			b = FALSE;
		}
	}

	m_bEnabled = b;

	if (m_hWnd)
	{
		this->EnableWindow(b);
	}
}

//-----------------------------------------------------------------------------
void CBEButton::EnableButtonState(BOOL bBodyState)
{
	//m_bLeftButtonDown = FALSE;	//TAPPULLO

	BOOL b = m_bEnabled;

	if (m_pToolBar->m_pBody->GetDocument())
	{
		if (m_pToolBar->m_pBody->GetDocument()->GetFormMode() == CBaseDocument::NEW && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_NEW))
		{
			b = FALSE;
		}
		else if (m_pToolBar->m_pBody->GetDocument()->GetFormMode() == CBaseDocument::EDIT && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EDIT))
		{
			b = FALSE;
		}
	}

	if (m_hWnd)
	{
		this->EnableWindow(b && !(!bBodyState && m_bDisableOnReadOnly));
	}
}

//-----------------------------------------------------------------------------
int CBEButton::GetWidth(BOOL bCalcOnly)
{
	CSize csBtn = SizeToContent(bCalcOnly);
	return  csBtn.cx;
}

//-----------------------------------------------------------------------------
int CBEButton::GetHeight()
{
	return m_nHeight;
}

//-----------------------------------------------------------------------------
void CBEButton::SetHeight(int nHeight)
{
	m_nHeight = nHeight;
}


//-----------------------------------------------------------------------------
void CBEButton::SetPos(int left, int top, int w, int h)
{
	SetWindowPos
	(
		NULL,
		left,
		top,
		w,
		h,
		SWP_SHOWWINDOW | SWP_NOZORDER
	);
}

//-----------------------------------------------------------------------------
void CBEButton::RedrawWindow()
{
	if (::IsWindow(m_hWnd))
		__super::RedrawWindow();
}

//-----------------------------------------------------------------------------
void CBEButton::OnSetFocus(CWnd* pOldWnd)
{
	if (pOldWnd)
	{
		CBodyEdit* pBE = dynamic_cast<CBodyEdit*>(GetParent());
		ASSERT(pBE);
		if (pOldWnd->m_hWnd == GetParent()->m_hWnd)
			pBE->SetFocusOnly();
		else
			if (pBE->IsChild(pOldWnd))
				if (GetParsedCtrl(pOldWnd))
					pOldWnd->SetFocus();
				else
					pBE->SetFocusOnly();
	}
}

//-----------------------------------------------------------------------------
void CBEButton::OnLButtonDown(UINT nFlags, CPoint point)
{
	m_bLeftButtonDown = TRUE;

	__super::OnLButtonDown(nFlags, point);
}

//-----------------------------------------------------------------------------
void CBEButton::OnLButtonUp(UINT nFlags, CPoint point)
{
	if (m_hMenu && m_bMenuIsActive)
	{
		m_bLeftButtonDown = FALSE;
		__super::OnLButtonUp(nFlags, point);
		return;
	}

	if (!m_bLeftButtonDown)
	{
		CBCGPButton::OnLButtonUp(nFlags, point);
		return;		//Impedisce che il pulsante prenda un Left Mouse Up  senza aver prima preso il Down (es.: body in tile collassate)
	}

	m_bLeftButtonDown = FALSE;
	BOOL bInternal = CBodyEdit::IsInternalCommand(m_nID);

	CWnd* pWnd = GetParent();
	if (pWnd == NULL)
	{
		CBCGPButton::OnLButtonUp(nFlags, point);
		return;
	}

	CBodyEdit* pBE = dynamic_cast<CBodyEdit*>(pWnd);
	ASSERT(pBE);
	pBE->SetFocusOnly();

	pWnd->SEND_WM_COMMAND(GetDlgCtrlID(), 0, m_hWnd);

	if (!bInternal)
		GetBody()->OnCommandDone();

	CBCGPButton::OnLButtonUp(nFlags, point);
}

//-----------------------------------------------------------------------------
void CBEButton::OnDraw(CDC* pDC, const CRect& rect, UINT uiState)
{
	if (m_hMenu)
		__super::OnDraw(pDC, rect, uiState);
	else
		CBCGPButton::OnDraw(pDC, rect, uiState);
}

//-----------------------------------------------------------------------------
void CBEButton::AddMenuItem(CString aName, CString sDescription, UINT nMenuID, BOOL bEnable /*= TRUE*/, BOOL bCheck /*= FALSE*/, const CString& sImageNS /*= L""*/)
{
	if (!m_hMenu)
	{
		m_menu.CreateMenu();
		m_hMenu = m_menu.GetSafeHmenu();
	}

	if (m_pToolBar->m_pBody->GetDocument())
	{
		CString sAcc = m_pToolBar->m_pBody->GetDocument()->GetDocAccelText(nMenuID);
		if (!sAcc.IsEmpty() && m_strTooltip.Find(sAcc) < 0)
		{
			sDescription += sAcc;
		}
	}

	CInfoOSLButton* infoOSL = new CInfoOSLButton();

	infoOSL->m_strName = aName;
	infoOSL->m_nID = nMenuID;
	infoOSL->SetType(OSLTypeObject::OSLType_ToolbarButton);

	m_CInfoOSLs.Add(infoOSL);

	m_menu.AppendMenu(MF_STRING, nMenuID, sDescription);

	CheckMenuItem(nMenuID, bCheck);
	EnableMenuItem(nMenuID, bEnable);

	if (!sImageNS.IsEmpty())
	{
		CBitmap* pBmp = new CBitmap;

		if (::LoadBitmapOrPng(pBmp, sImageNS))
		{
			m_mapMenuImages.SetAt(nMenuID, pBmp);
		}
		else
			delete pBmp;
	}
}

//-----------------------------------------------------------------------------
void CBEButton::CheckMenuItem(UINT nID, BOOL bCheck)
{
	if (m_hMenu)
		::CheckMenuItem(m_hMenu, nID, bCheck ? MF_CHECKED : MF_UNCHECKED);
}

void CBEButton::EnableMenuItem(UINT nID, BOOL bEnable)
{
	if (m_hMenu)
		::EnableMenuItem(m_hMenu, nID, bEnable ? MF_ENABLED : MF_DISABLED);
}

void CBEButton::AddMenuSeparator()
{
	if (m_hMenu)
		m_menu.AppendMenu(MF_SEPARATOR);
}

//-----------------------------------------------------------------------------
BOOL CBEButton::OnDrawMenuImage
(
	CDC* pDC,
	const CBCGPToolbarMenuButton* pMenuButton,
	const CRect& rectImage
)
{
	if (!m_hMenu)
		return FALSE;

	CBitmap* pBmp = NULL;
	if (!m_mapMenuImages.Lookup(pMenuButton->m_nID, pBmp))
		return FALSE;
	ASSERT_VALID(pBmp);

	::DrawBitmap(pBmp, pDC, rectImage);

	return TRUE;
}

//==============================================================================
IMPLEMENT_DYNAMIC(CBEBtnCtrl, CBEButton)

CBEBtnCtrl::CBEBtnCtrl
(
	CBEToolbar*			pToolBar,
	CRuntimeClass*		rtcCtrl,
	const CString&		sName,
	int					nWidth,
	UINT				nID,
	DWORD				dwBtnStyle,
	const CString&		strTooltip,
	const CString&		strText
)
	:
	CBEButton(pToolBar, sName, nWidth, nID, 0, dwBtnStyle, strTooltip, strText),
	m_bAutoSize(FALSE),
	m_pLabel(NULL)
{
	m_pControl = (rtcCtrl == RUNTIME_CLASS(CBCGPStatic) ? (CWnd*)(new CBCGPStatic()) : (CWnd*)rtcCtrl->CreateObject());
	ASSERT_VALID(m_pControl);
}

//-----------------------------------------------------------------------------
CBEBtnCtrl::~CBEBtnCtrl()
{
	SAFE_DELETE(m_pControl);
}

//-----------------------------------------------------------------------------
int CBEBtnCtrl::GetWidth(BOOL)
{
	int w = m_nWidth;

	CParsedCtrl* pCtrl = dynamic_cast<CParsedCtrl*>(m_pControl);
	if (pCtrl)
	{
		if (pCtrl->GetButton())
			w += pCtrl->GetButtonWidth(0) + 2;

		Array& a = pCtrl->GetStateCtrlsArray();
		w += a.GetSize() * (20 + 2);
	}

	return  w;
}

//-----------------------------------------------------------------------------
BOOL CBEBtnCtrl::Create(CWnd* pBody)
{
	ASSERT_VALID(m_pControl);
	CParsedCtrl* pCtrl = GetParsedCtrl(m_pControl);
	if (pCtrl)
	{
		if (m_pControl->IsKindOf(RUNTIME_CLASS(CDateEdit)))
		{
			pCtrl->Attach(BTN_CALENDAR_ID);
			//m_nWidth -= 20; // size == CONTROL
		}
		if (!pCtrl->Create(this->m_dwStyle, CRect(0, 0, 0, 0), pBody, m_nID))
		{
			TRACE("Create Bodyedit Toolbar Parsed Control fails on %d\n", m_nID);
			ASSERT(FALSE);
			return FALSE;
		}

		pCtrl->AttachDocument(((CBodyEdit*)pBody)->GetDocument());

		//label automatica di un altro controllo
		if (m_nWidth == 0 && !m_strText.IsEmpty()/*&& m_pControl->IsKindOf(RUNTIME_CLASS(CStrEdit)) && !m_bEnabled*/)
		{
			CDC* pDC = this->m_pToolBar->m_pBody->GetDC();
			m_nWidth = ::GetTextSize(pDC, m_strText, AfxGetThemeManager()->GetFormFont()).cx + 16;
			m_pToolBar->m_pBody->ReleaseDC(pDC);

			m_nHeight = 20;
			pCtrl->SetValue(m_strText);
		}

		m_pControl->SetFont(AfxGetThemeManager()->GetControlFont());
	}
	else if (m_pControl->IsKindOf(RUNTIME_CLASS(CBCGPStatic)))
	{
		CBCGPStatic* pSt = (CBCGPStatic*)m_pControl;
		if (!pSt->Create(this->m_strText, this->m_dwStyle, CRect(0, 0, 0, 0), pBody, m_nID))
		{
			TRACE("Create Bodyedit Toolbar CBCGPStatic control fails on %d\n", m_nID);
			ASSERT(FALSE);
			return FALSE;
		}
		CDC* pDC = this->m_pToolBar->m_pBody->GetDC();
		m_nWidth = ::GetTextSize(pDC, m_strText, AfxGetThemeManager()->GetFormFont()).cx + 4;
		m_pToolBar->m_pBody->ReleaseDC(pDC);

		m_pControl->SetFont(AfxGetThemeManager()->GetControlFont());
	}
	else if (m_pControl->IsKindOf(RUNTIME_CLASS(CTBScrollBar)))
	{
		CTBScrollBar* pSt = dynamic_cast<CTBScrollBar*>(m_pControl);
		ASSERT_VALID(pSt);
		if (!pSt->Create(SBS_HORZ | WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 0, 0), pBody, IDC_BE_HSCROLLBAR))
		{
			TRACE("Create Bodyedit Toolbar hscrollbar control fails on %d\n", m_nID);
			ASSERT(FALSE);
			return FALSE;
		}
		SCROLLINFO sinfo;
		if (pSt->GetScrollInfo(&sinfo))
		{
			sinfo.nPage = 1;
			VERIFY(pSt->SetScrollInfo(&sinfo));
		}
		m_pControl->SetFont(AfxGetThemeManager()->GetFormFont());
	}
	else
		return FALSE;

	if (!m_bEnabled)
		m_pControl->EnableWindow(FALSE);
	if (!m_bVisible)
		m_pControl->ShowWindow(SW_HIDE);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBEBtnCtrl::ShowButton(BOOL b)
{
	if (m_bVisible == b)
		return;

	m_bVisible = b;

	if (m_pControl && m_pControl->m_hWnd)
	{
		CParsedCtrl* pCtrl = GetParsedCtrl(m_pControl);
		if (pCtrl)
		{
			pCtrl->ShowCtrl(b);
		}
		else
			m_pControl->ShowWindow(b ? SW_NORMAL : SW_HIDE);
	}

	if (m_pLabel)
		m_pLabel->ShowButton(b);
}

//-----------------------------------------------------------------------------
void CBEBtnCtrl::EnableButton(BOOL b)
{
	if (m_pControl)
	{
		CParsedCtrl* pCtrl = GetParsedCtrl(m_pControl);
		if (pCtrl && pCtrl->GetCtrlData() && pCtrl->GetCtrlData()->IsAlwaysReadOnly())
			b = FALSE;
	}

	if (m_bEnabled == b)
		return;

	m_bEnabled = b;

	if (m_pControl && m_pControl->m_hWnd)
	{
		CParsedCtrl* pCtrl = GetParsedCtrl(m_pControl);
		if (pCtrl)
		{
			pCtrl->EnableCtrl(b);
		}
		else
			m_pControl->EnableWindow(b);
	}
}

//-----------------------------------------------------------------------------
void CBEBtnCtrl::EnableButtonState(BOOL bBodyState)
{
	BOOL b = m_bEnabled;

	if (m_pToolBar->m_pBody->GetDocument())
	{
		if (m_pToolBar->m_pBody->GetDocument()->GetFormMode() == CBaseDocument::NEW && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_NEW))
		{
			b = FALSE;
		}
		else if (m_pToolBar->m_pBody->GetDocument()->GetFormMode() == CBaseDocument::EDIT && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EDIT))
		{
			b = FALSE;
		}
	}

	if (m_pControl && m_pControl->m_hWnd)
	{
		CParsedCtrl* pCtrl = GetParsedCtrl(m_pControl);
		if (pCtrl)
		{
			pCtrl->EnableCtrl(b && !(!bBodyState && m_bDisableOnReadOnly));
		}
		else
			m_pControl->EnableWindow(b && !(!bBodyState && m_bDisableOnReadOnly));
	}
}

//-----------------------------------------------------------------------------
void CBEBtnCtrl::SetPos(int left, int top, int w, int h)
{
	ASSERT_VALID(m_pControl);

	if (m_pControl && ::IsWindow(m_pControl->m_hWnd))
	{
		if (m_pControl->IsKindOf(RUNTIME_CLASS(CComboBox)))
		{
			CRect r;
			m_pControl->GetWindowRect(r);
			int rh = r.Height();
			if (rh < h)
			{
				top += (h - rh) / 2;

				m_pControl->SetWindowPos
				(
					NULL,
					left + 2,
					top,
					w - 4,
					h,
					SWP_SHOWWINDOW | SWP_NOZORDER
				);
			}
		}
		else
		{
			if (m_pControl->IsKindOf(RUNTIME_CLASS(CDateEdit)))
			{
				// BTN_CALENDAR_ID
				w -= 20; // size == CONTROL
			}

			//m_nWidth -= 20; // size == CONTROL
			m_pControl->SetWindowPos
			(
				NULL,
				left + 2,
				top,
				w - 4,
				h,
				SWP_SHOWWINDOW | SWP_NOZORDER
			);

		}
	}
}

//-----------------------------------------------------------------------------
void CBEBtnCtrl::Invalidate()
{
	ASSERT_VALID(m_pControl);

	if (m_pControl && ::IsWindow(m_pControl->m_hWnd))
		m_pControl->Invalidate();
}

//-----------------------------------------------------------------------------
void CBEBtnCtrl::RedrawWindow()
{
	ASSERT_VALID(m_pControl);

	if (m_pControl && ::IsWindow(m_pControl->m_hWnd))
		m_pControl->RedrawWindow();
}

//-----------------------------------------------------------------------------
void CBEBtnCtrl::AttachHotKeyLink(HotKeyLinkObj* pHotKeyLink, UINT nBtnID/* = BTN_DEFAULT*/)
{
	ASSERT_VALID(m_pControl);
	CParsedCtrl* pCtrl = dynamic_cast<CParsedCtrl*>(m_pControl);
	ASSERT(pCtrl);
	if (pCtrl)
	{
		pCtrl->Attach(nBtnID);
		pCtrl->AttachHotKeyLink(pHotKeyLink);
	}
}

//==============================================================================
IMPLEMENT_DYNAMIC(CBEToolbar, Array)

CBEToolbar::CBEToolbar(CBodyEdit* pBody)
	:
	IOSLObjectManager(OSLType_Toolbar),
	m_pBody(pBody),
	m_rectToolBar(0, 0, 0, 0),
	m_bDefaultRightAlign(FALSE)
{
	m_nHeight = ScalePix(BE_AUXTOOLBAR_DEFAULT_HEIGHT);
	ASSERT_VALID(pBody);
}

//-----------------------------------------------------------------------------
CBEToolbar::~CBEToolbar()
{
	for (int n = m_Images.GetUpperBound(); n >= 0; n--)
		SAFE_DELETE(m_Images.GetAt(n));

	m_Images.RemoveAll();
}

//-------------------------------------------------------------------------------------
CImageAssociation* CBEToolbar::GetImageAssociation(UINT nIDC)
{
	for (int i = 0; i <= m_Images.GetUpperBound(); i++)
	{
		CImageAssociation* pImage = (CImageAssociation*)m_Images.GetAt(i);

		if (pImage->GetCommandID() == nIDC)
			return pImage;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CBEToolbar::SetHeight(int h)
{
	m_nHeight = h;
	m_rectToolBar.bottom = m_rectToolBar.top + m_nHeight;

	if (m_pBody && m_pBody->m_hWnd)
		m_pBody->Invalidate();
}


//-----------------------------------------------------------------------------
BOOL CBEToolbar::Create(const CString& sName)
{
	if (GetSize() == 0)
		return FALSE;

	CRect rect(0, 0, 0, 0);

	if (!sName.IsEmpty())
	{
		GetInfoOSL()->m_pParent = m_pBody->GetInfoOSL();
		GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::TOOLBAR, sName, m_pBody->GetNamespace());

		if (GetInfoOSL()->m_Namespace.IsValid())
			AfxGetSecurityInterface()->GetObjectGrant(GetInfoOSL());
	}

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CBEButton* pbd = GetAt(i);
		ASSERT_VALID(pbd);

		pbd->GetInfoOSL()->m_pParent = GetInfoOSL();
		if (!pbd->m_sName.IsEmpty())
			pbd->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::TOOLBARBUTTON, pbd->m_sName, GetInfoOSL()->m_Namespace);

		if (pbd->GetInfoOSL()->m_Namespace.IsValid())
		{
			AfxGetSecurityInterface()->GetObjectGrant(pbd->GetInfoOSL());
			if (!OSL_CAN_DO(pbd->GetInfoOSL(), OSL_GRANT_EXECUTE))
			{
				RemoveAt(i);	//shift verso sinistra degli elementi
				i--;			//compensa l'indice per rimanere nella posizione corrente
				continue;
			}
		}
		if (pbd->m_CInfoOSLs.GetCount() > 0)
		{
			for (int y = 0; y < pbd->m_CInfoOSLs.GetCount(); y++)
			{
				CInfoOSLButton* infoOSL = pbd->m_CInfoOSLs.GetAt(y);

				infoOSL->m_pParent = pbd->GetInfoOSL();

				if (!infoOSL->m_strName.IsEmpty())
					infoOSL->m_Namespace.SetChildNamespace(CTBNamespace::TOOLBARBUTTON, infoOSL->m_strName, pbd->GetInfoOSL()->m_Namespace);

				if (infoOSL->m_Namespace.IsValid())
				{
					AfxGetSecurityInterface()->GetObjectGrant(infoOSL);
					if (!OSL_CAN_DO(infoOSL, OSL_GRANT_EXECUTE))
					{
						pbd->m_CInfoOSLs.RemoveAt(y);
						pbd->GetMenu()->RemoveMenu(y, MF_BYPOSITION);
						y--;
						continue;
					}
				}
			}
		}
		if (!pbd->Create(m_pBody))
		{
			ASSERT(FALSE);

			RemoveAt(i);	//shift verso sinistra degli elementi
			i--;			//compensa l'indice per rimanere nella posizione corrente
			continue;
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBEToolbar::isAutoSizeButton(CBEButton* pBD)
{
	if (
		!pBD->GetHWnd() ||
		!pBD->IsKindOf(RUNTIME_CLASS(CBEBtnCtrl)) ||
		!((CBEBtnCtrl*)pBD)->IsAutoSize()
		)
		return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBEToolbar::SetPos()
{
	int nb = GetSize();

	int left = m_rectToolBar.left + 2;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CBEButton* pBD = GetAt(i);
		ASSERT_VALID(pBD);
		if (!pBD->GetHWnd())
			continue;

		if (pBD->m_bRightAlign)
			continue;

		if (isAutoSizeButton(pBD)) continue;

		int w = pBD->GetWidth(FALSE);

		int top = m_rectToolBar.top;
		int h = m_nHeight;
		if (pBD->GetHeight())
		{
			int hB = pBD->GetHeight();
			if (hB < h)
			{
				top += (h - hB) / 2;
				h = hB;
			}
		}

		pBD->SetPos(left, top, w, h);

		if (!--nb)
			break;

		left += w + 1;
	}

	int right = m_rectToolBar.right - 4;
	for (int i = GetUpperBound(); i >= 0; i--)
	{
		CBEButton* pBD = GetAt(i);
		ASSERT_VALID(pBD);
		if (!pBD->GetHWnd())
			continue;
		//if (!pBD->IsWindowVisible())
		//	continue;

		if (!pBD->m_bRightAlign)
			continue;

		if (isAutoSizeButton(pBD))
			continue;

		int w = pBD->GetWidth(FALSE);

		right -= (w + 1);

		int top = m_rectToolBar.top;
		int h = m_nHeight;
		if (pBD->GetHeight())
		{
			int hB = pBD->GetHeight();
			if (hB < h)
			{
				top += (h - hB) / 2;
				h = hB;
			}
		}

		pBD->SetPos
		(
			right,
			top,
			w,
			h
		);

		if (!--nb)
			break;
	}

	// Auto Size Control calculate
	for (int i = GetUpperBound(); i >= 0; i--)
	{
		CBEButton* pBD = GetAt(i);
		ASSERT_VALID(pBD);

		if (!isAutoSizeButton(pBD)) continue;

		int top = m_rectToolBar.top;
		int h = m_nHeight;
		if (pBD->GetHeight())
		{
			int hB = pBD->GetHeight();
			if (hB < h)
			{
				top += (h - hB) / 2;
				h = hB;
			}
		}

		if (pBD->IsKindOf(RUNTIME_CLASS(CBEBtnCtrl)))
		{
			if (((CBEBtnCtrl*)pBD)->m_pControl->IsKindOf(RUNTIME_CLASS(CTBScrollBar)))
			{
				int ay = (int)(m_rectToolBar.Height() / 2);
				CTBScrollBar* pScrollBar = (CTBScrollBar*)((CBEBtnCtrl*)pBD)->m_pControl;
				h = pScrollBar->GetHeight();
				top += ay - (int)(h / 2);
			}
		}

		pBD->SetPos(left, top, right - left, h);
	}
}

//-----------------------------------------------------------------------------
CBEButton* CBEToolbar::AddButton
(
	const CString& sName,
	UINT		nID,
	UINT		nIDB,
	CString		strTooltip,
	CString		strText /*= L""*/,
	int			nIndex /*= -1*/
)
{
	ASSERT_VALID(this);
	ASSERT(ID);
	ASSERT(!sName.IsEmpty());
	ASSERT_VALID(m_pBody);

	if (&m_pBody->m_FooterToolBar == this)
	{
		ASSERT_TRACE(m_pBody->BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR), "Add button on hidden footer toolbar\n");
	}
	if (&m_pBody->m_HeaderToolBar == this)
	{
		ASSERT_TRACE(m_pBody->BEGetExStyle(BE_STYLE_SHOW_HEADER_TOOLBAR), "Add button on hidden header toolbar\n");
	}

	if (m_pBody->IsDuplicateButton(nID))
	{
		ASSERT_TRACE3(FALSE, "It has failed to add a duplicate button (%s, %d, %s)\n", sName, nID, strTooltip);
		return NULL;
	}

	CBEButton* pBtn = new CBEButton(this, sName, BE_AUXTOOLBAR_BUTTON_DEFAULT_WIDTH, nID, nIDB, (WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS), strTooltip, strText);
	ASSERT_VALID(pBtn);

	pBtn->m_bRightAlign = this->m_bDefaultRightAlign;

	if (nIndex < 0)
		__super::Add(pBtn);
	else
	{
		__super::InsertAt(nIndex, pBtn);
	}

	return pBtn;
}

//-----------------------------------------------------------------------------
CBEButton* CBEToolbar::AddButton
(
	const CString& sName,
	UINT		nID,
	CString		strNsImage,
	CString		strTooltip,
	CString		strText /* = L""*/,
	int			nIndex /*= -1*/
)
{
	ASSERT_VALID(this);
	ASSERT(ID);
	ASSERT(!sName.IsEmpty());
	ASSERT_VALID(m_pBody);

	if (&m_pBody->m_FooterToolBar == this)
	{
		ASSERT_TRACE(m_pBody->BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR), "Add button on hidden footer toolbar\n");
	}
	if (&m_pBody->m_HeaderToolBar == this)
	{
		ASSERT_TRACE(m_pBody->BEGetExStyle(BE_STYLE_SHOW_HEADER_TOOLBAR), "Add button on hidden header toolbar\n");
	}

	if (m_pBody->IsDuplicateButton(nID))
	{
		ASSERT_TRACE3(FALSE, "It has failed to add a duplicate button (%s, %d, %s)\n", sName, nID, strTooltip);
		return NULL;
	}

	CBEButton* pBtn = new CBEButton(this, sName, BE_AUXTOOLBAR_BUTTON_DEFAULT_WIDTH, nID, strNsImage, (WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS), strTooltip, strText);
	ASSERT_VALID(pBtn);

	pBtn->m_bRightAlign = this->m_bDefaultRightAlign;

	if (nIndex < 0)
		__super::Add(pBtn);
	else
	{
		__super::InsertAt(nIndex, pBtn);
	}

	CImageAssociation* pAssociation = new CImageAssociation(nID, strNsImage, 0);
	m_Images.Add(pAssociation);
	return pBtn;
}

//-----------------------------------------------------------------------------
CBEBtnCtrl* CBEToolbar::AddControl
(
	CRuntimeClass*	pRtc,
	const CString&	sName,
	UINT			nID,
	CString			strTooltip,
	CString			strText,
	int				width,
	DWORD			dwBtnStyle/* = (WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS)*/,
	int				nIndex/* = -1*/,
	DataObj*		pDataObj /*= NULL*/
)
{
	ASSERT_VALID(this);
	ASSERT(ID);
	ASSERT(!sName.IsEmpty());
	ASSERT_VALID(m_pBody);
	width = ScalePix(width);
	if (&m_pBody->m_FooterToolBar == this)
	{
		ASSERT_TRACE(m_pBody->BEGetExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR), "Add button on hidden footer toolbar\n");
	}
	if (&m_pBody->m_HeaderToolBar == this)
	{
		ASSERT_TRACE(m_pBody->BEGetExStyle(BE_STYLE_SHOW_HEADER_TOOLBAR), "Add button on hidden header toolbar\n");
	}

	if (m_pBody->IsDuplicateButton(nID))
	{
		ASSERT_TRACE3(FALSE, "It has failed to add a duplicate button (%s, %d, %s)\n", sName, nID, strTooltip);
		return NULL;
	}

	CBEBtnCtrl* pLabel = NULL;
	if (!strText.IsEmpty() && (pRtc->IsDerivedFrom(RUNTIME_CLASS(CComboBox)) || pRtc->IsDerivedFrom(RUNTIME_CLASS(CEdit))))
	{
		//pLabel = AddControl(RUNTIME_CLASS(CBCGPStatic), sName + L"_Label", IDC_STATIC, L"", strText, 0, (WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS), nIndex < 0 ? -1 : nIndex++);
		//TODO sostituire riga sopra con le seguenti
		CString sResourceName = cwsprintf(L"%s_%d", LPCTSTR(sName + L"_Label"), nID);
		int nIDlabel = AfxGetTBResourcesMap()->GetTbResourceID(sResourceName, TbResourceType::TbControls);

		pLabel = AddControl(RUNTIME_CLASS(CStrEdit), sName + L"_Label", nIDlabel,
			L"", L"", 0, (WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS), nIndex < 0 ? -1 : nIndex++);

		pLabel->m_strText = strText; //altrimenti va in ricorsione
		pLabel->EnableButton(FALSE);
		//----

		strText.Empty();
	}

	CBEBtnCtrl* pBtn = new CBEBtnCtrl(this, pRtc, sName, width, nID, dwBtnStyle, strTooltip, strText);
	ASSERT_VALID(pBtn);

	pBtn->m_pLabel = pLabel;

	if (pDataObj)
	{
		ASSERT_VALID(pDataObj);
		CParsedCtrl* pCtrl = dynamic_cast<CParsedCtrl*>(pBtn->m_pControl);
		if (pCtrl) pCtrl->Attach(pDataObj);
	}

	pBtn->m_bRightAlign = this->m_bDefaultRightAlign;

	if (nIndex < 0)
		__super::Add(pBtn);
	else
	{
		__super::InsertAt(nIndex, pBtn);
	}

	return pBtn;
}

//-----------------------------------------------------------------------------
CBEBtnCtrl* CBEToolbar::AddStaticControl
(
	const CString&	sName,
	CString			strTooltip,
	CString			strText,
	int				width,
	int				nIndex/* = -1*/,
	DataObj*		pDataObj /*= NULL*/
)
{
	int nID = AfxGetTBResourcesMap()->GetTbResourceID(sName, TbResourceType::TbControls);

	CBEBtnCtrl* pBtn = AddControl(RUNTIME_CLASS(CStrEdit), sName, nID, strTooltip, strText, width, (WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS), nIndex, pDataObj);
	pBtn->m_nHeight = 20;

	if (pDataObj)
		pDataObj->SetAlwaysReadOnly();
	else
		pBtn->EnableButton(FALSE);

	return pBtn;
}

//-----------------------------------------------------------------------------
void CBEToolbar::EnableButton(UINT ID, BOOL bEnable)
{
	CBEButton* pBtn = FindButton(ID);
	if (pBtn)
	{
		pBtn->EnableButton(bEnable);
	}
}

//-----------------------------------------------------------------------------
void CBEToolbar::EnableButton(CBEButton* pBtn, BOOL bEnable)
{
	if (pBtn)
	{
		pBtn->EnableButton(bEnable);
	}
}

//-----------------------------------------------------------------------------
void CBEToolbar::ShowButton(UINT ID, BOOL bShow)
{
	CBEButton* pBtn = FindButton(ID);
	if (pBtn)
	{
		pBtn->ShowButton(bShow);
	}
}

//-----------------------------------------------------------------------------
int CBEToolbar::FindButtonIndex(UINT ID)
{
	for (int i = 0; i < GetSize(); i++)
	{
		CBEButton* pBtn = GetAt(i);
		if (pBtn->m_nID == ID)
			return i;
	}

	return -1;
}

//-----------------------------------------------------------------------------
CBEButton* CBEToolbar::FindButton(UINT ID)
{
	int i = FindButtonIndex(ID);

	return i > -1 ? GetAt(i) : NULL;
}

//-----------------------------------------------------------------------------
void CBEToolbar::EnableButtons(BOOL bBodyState)
{
	for (int i = 0; i < GetSize(); i++)
	{
		CBEButton* pBtn = GetAt(i);

		if (m_pBody->IsStandardButton(pBtn->m_nID))
			continue;

		pBtn->EnableButtonState(bBodyState);

		m_pBody->OnBEEnableButton(pBtn);
		if (m_pBody->GetDocument())
			m_pBody->GetDocument()->DispatchOnBEEnableButton(m_pBody, pBtn);
	}
}

//-----------------------------------------------------------------------------
void CBEToolbar::EnableButtons()
{
	for (int i = 0; i < GetSize(); i++)
	{
		CBEButton* pBtn = GetAt(i);

		if (m_pBody->IsStandardButton(pBtn->m_nID))
			continue;

		m_pBody->OnBEEnableButton(pBtn);
		if (m_pBody->GetDocument())
			m_pBody->GetDocument()->DispatchOnBEEnableButton(m_pBody, pBtn);
	}
}

//-----------------------------------------------------------------------------
void CBEToolbar::Invalidate()
{
	for (int i = 0; i < GetSize(); i++)
	{
		CBEButton* pBtn = GetAt(i);

		if (::IsWindow(pBtn->m_hWnd))
			pBtn->Invalidate();
	}
}

//-----------------------------------------------------------------------------
void CBEToolbar::Redraw()
{
	for (int i = 0; i < GetSize(); i++)
	{
		CBEButton* pBtn = GetAt(i);

		if (!pBtn->m_bVisible && ::IsWindow(pBtn->m_hWnd) && pBtn->IsWindowVisible())
			pBtn->RedrawWindow();
	}
}

//-----------------------------------------------------------------------------
void CBEToolbar::OnUpdateControls(BOOL /*bParentIsVisible*/)
{
	for (int i = 0; i < GetSize(); i++)
	{
		CBEBtnCtrl* pBtn = dynamic_cast<CBEBtnCtrl*>(GetAt(i));
		if (!pBtn) continue;
		if (!pBtn->m_pControl) continue;

		CParsedCtrl* pCtrl = dynamic_cast<CParsedCtrl*>(pBtn->m_pControl);
		if (!pCtrl) continue;

		if (
			pCtrl->IsDataModified() ||
			pCtrl->ForceUpdateCtrlView()
			)
		{
			pCtrl->UpdateCtrlStatus();
			pCtrl->UpdateCtrlView();
			pCtrl->GetCtrlCWnd()->UpdateWindow();
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CBEToolbar::ProcessSysKeyMessage(UINT nKey)
{
	CString strPattern(_T("&"));
	strPattern += (TCHAR)nKey;

	for (int i = 0; i < GetSize(); i++)
	{
		CBEButton* pBtn = GetAt(i);

		if (!(pBtn->m_bVisible && ::IsWindow(pBtn->m_hWnd) && pBtn->IsWindowVisible()) || !pBtn->m_bEnabled)
			continue;

		CString strCaption = pBtn->m_strText.MakeUpper();
		if (strCaption.Find(strPattern) != -1)
		{
			// Vedi CBEButton::OnLButtonDown
			pBtn->GetParent()->SEND_WM_COMMAND(pBtn->m_nID, 0, pBtn->m_hWnd);
			m_pBody->OnCommandDone();

			return TRUE;
		}
	}

	return FALSE;
}

//=============================================================================
