
#include "stdafx.h"     

#include <TbGeneric\minmax.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlibUI\FormatDialog.h>
#include <TbGenlibUI\FontsDialog.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGenlib\const.h>

#include <TbWoormEngine\edtmng.h>
#include <TbWoormEngine\RpSymTbl.h>
#include <TbWoormEngine\RepTable.h>
#include <TbWoormEngine\DispTbl.h>

#include "cell.h"
#include "column.h"
#include "viewpars.h"
#include "woormini.h"
#include "woormdoc.h"
#include "woormvw.h"
#include "woormfrm.h"
#include "table.h"
#include "rectobj.h"
//#include "EditorLinksDlg.h"
#include "mclrdlg.h"
#include "listdlg.hjson" //JSON AUTOMATIC UPDATE

#include "commands.hrc"
#include "table1.hrc"
#include "rectobj.hjson" //JSON AUTOMATIC UPDATE
#include "woormdoc.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "beginh.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


IMPLEMENT_DYNAMIC (Table, BaseObj)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(Table, BaseObj)

	ON_COMMAND(ID_HIDE_TABLE_TITLE,		   OnHideTableTitle)
	ON_COMMAND(ID_HIDE_COLUMNS_TITLE,	   OnHideColumnsTitle)

	ON_COMMAND(ID_WRM_EASY_READING_BYCOLOR,				OnEasyReadingByColor)
	ON_COMMAND(ID_WRM_EASY_READING_BYCOLOR_DYNAMIC,		OnEasyReadingByColorDynamic)
	ON_COMMAND(ID_WRM_EASY_READING_BYLINE,				OnEasyReadingByLine)
	ON_COMMAND(ID_WRM_EASY_READING_BYLINE_DYNAMIC,		OnEasyReadingByLineDynamic)

	ON_COMMAND(ID_FISCAL_END,							OnFiscalEnd)

	ON_COMMAND(ID_TABLETITLE_LINEWIDTH,    OnTableTitleLineWidth)
	ON_COMMAND(ID_TABLETITLE_COLORS,       OnTableTitleColors)
	ON_COMMAND(ID_TABLETITLE_ALIGN,        OnTableTitleAlign)
	ON_COMMAND(ID_TABLETITLE_FONT_STYLE,   OnTableTitleFontStyle)
	ON_COMMAND(ID_TRANSPARENT,             OnSetTrasparent)

	ON_COMMAND(ID_COLUMNTITLE_LINEWIDTH,   OnColumnTitleLineWidth)
	ON_COMMAND(ID_COLUMNTITLE_ALIGN,       OnColumnTitleAlign)
	ON_COMMAND(ID_COLUMNTITLE_FONT_STYLE,  OnColumnTitleFontStyle)
	ON_COMMAND(ID_SUBTOTAL_FONT_STYLE,     OnSubTotalFontStyle)
	ON_COMMAND(ID_SUBTOTAL_COLORS,         OnSubTotalColors)

	ON_COMMAND(ID_COLUMN_COLOR,					OnColumnColor)
	ON_COMMAND(ID_COLUMN_LINEWIDTH,				OnColumnLineWidth)
	//ON_COMMAND(ID_COLUMN_ALIGN,					OnColumnAlign)
	//ON_COMMAND(ID_COLUMN_FONT_STYLE,			OnColumnFontStyle)
	ON_COMMAND(ID_COLUMN_DYNAMIC_FONT_STYLE,	OnColumnDynamicFontStyle)
	ON_COMMAND(ID_COLUMN_DYNAMIC_BORDERS,		OnColumnDynamicBorders)
	ON_COMMAND(ID_COLUMN_DYNAMIC_FORMAT_STYLE,	OnColumnDynamicFormatStyle)
	ON_COMMAND(ID_COLUMN_FORMAT_STYLE,			OnColumnFormatStyle)
	ON_COMMAND(ID_COLUMN_TOGGLE_MULTI_ROW,		OnColumnToggleMultiRow)
	ON_COMMAND(ID_COLUMN_TOGGLE_BITMAP,			OnColumnToggleBitmap)
	ON_COMMAND(ID_COLUMN_TOGGLE_BARCODE,		OnColumnToggleBarCode)
	ON_COMMAND(ID_COLUMN_TOGGLE_TEXTFILE,		OnColumnToggleTextFile)
	ON_COMMAND(ID_COLUMN_TOGGLE_HIDDEN,			OnColumnToggleHidden)

	//ON_COMMAND(ID_COLUMN_MANAGE_LINK,			OnColumnToggleLink)
	//ON_COMMAND(ID_SHOW_ALL_HIDDEN_COLUMN,    OnShowAllHiddenColumn)
	//ON_COMMAND(ID_SHOW_ALL_LINKS,			 OnShowAllLinks) 

	ON_COMMAND(ID_CELL_COLORS,             OnCellColor)
	ON_COMMAND(ID_CELL_ALIGN,              OnCellAlign)
	ON_COMMAND(ID_CELL_FONT_STYLE,         OnCellFontStyle)
	ON_COMMAND(ID_CELL_BARCODE_ATTRIBUTES, OnCellBarCodeAttrs)

	ON_COMMAND(ID_TOTAL_COLORS,            OnTotalColors)
	ON_COMMAND(ID_TOTAL_LINEWIDTH,         OnTotalLineWidth)
	ON_COMMAND(ID_TOTAL_ALIGN,             OnTotalAlign)
	ON_COMMAND(ID_TOTAL_FONT_STYLE,        OnTotalFontStyle)
	ON_COMMAND(ID_TOTAL_MODIFY,            OnTotalModify)

	ON_COMMAND(ID_TABLE_BORDERS,           OnBorders)
	ON_COMMAND(ID_TABLE_SIZE,              OnTableMove)
	ON_COMMAND(ID_TABLETITLE,              OnTableTitle)
	ON_COMMAND(ID_COLUMNTITLE,             OnColumnTitle)
	ON_COMMAND(ID_TABLE_MODIFY_ENTRY,      OnModifyField)

	ON_COMMAND (ID_PROPORTIONAL,			OnUpdateShowProportional )

	ON_COMMAND(ID_COLUMN_SPLITTER,			OnColumnToggleSplitter)
	ON_COMMAND(ID_COLUMN_FIXED,				OnColumnToggleFixed)
	ON_COMMAND(ID_COLUMN_ANCHOR_LEFT,		OnColumnToggleAnchorLeft)
	ON_COMMAND(ID_COLUMN_ANCHOR_RIGHT,		OnColumnToggleAnchorRight)
	
	ON_COMMAND(ID_COLUMN_TOGGLE_VMERGE_EMPTY_CELL,	OnColumnToggleVMergeEmptyCell)
	ON_COMMAND(ID_COLUMN_TOGGLE_VMERGE_EQUAL_CELL,	OnColumnToggleVMergeEqualCell)
	ON_COMMAND(ID_COLUMN_TOGGLE_VMERGE_TAIL_CELL,	OnColumnToggleVMergeTailCell)

	ON_COMMAND(ID_COLUMN_HIDDEN_WHEN_EMPTY,	OnColumnToggleHiddenWhenEmpy)
	ON_COMMAND(ID_COLUMN_WIDTH_OPTIMIZED,	OnColumnToggleOptimizedWidth)

	ON_COMMAND_RANGE (ID_BRING_TO_FRONT_0, (UINT)(ID_BRING_TO_FRONT_4), OnBringToFront)
	ON_COMMAND (ID_SEND_TO_BACK,								OnSendToBack)

	ON_COMMAND(ID_COPY_ATTRIBUTES,				OnCopyActiveColumnAttributes)
	ON_COMMAND(ID_COPY_COLUMNTITLE_ATTRIBUTES,	OnCopyActiveColumnTitleAttributes)

	ON_COMMAND(ID_SET_DROPSHADOW,			OnSetDropShadow)
	ON_COMMAND(ID_SET_HEIGHTS,				OnSetHeights)
	ON_COMMAND(ID_SET_TABLE_ROWS,			OnSetTableRows)

	ON_COMMAND(ID_SET_COLUMNTITLE_TOOLTIP,	OnSetColumnTitleTooltip)

	ON_COMMAND (ID_SET_HIDDEN,				OnSetHidden)
	ON_COMMAND (ID_SET_WIDTH,				OnColumnSetWidth)

	ON_COMMAND(ID_COLUMN_TOGGLE_TEMPLATE,		OnColumnToggleTemplate)
	ON_COMMAND(ID_TOGGLE_TEMPLATE,				OnToggleTemplate)
	
	ON_COMMAND(ID_COLUMN_SET_STYLE,				OnColumnSetStyle)
	ON_COMMAND(ID_SELECT_STYLE,					OnTableSetStyle)
	ON_COMMAND(ID_COLUMN_SET_STYLE_NAME,		OnColumnSetStyleName)
	ON_COMMAND(ID_SET_NEW_STYLE_NAME,				OnTableSetStyleName)

	ON_COMMAND(ID_COLUMN_CLEAR_CUSTOM_STYLE,	OnClearColumnCustomStyles)
	ON_COMMAND(ID_CLEAR_CUSTOM_STYLE,			OnClearTableCustomStyles)
	ON_COMMAND(ID_CLEAR_ALL_CUSTOM_STYLE,		OnClearAllCustomStyles)

	ON_COMMAND(ID_LAYOUT_CLONE_OBJECT,			OnLayoutCopyObject)
	ON_COMMAND(ID_LAYOUT_MOVE_OBJECT,			OnLayoutMoveObject)

END_MESSAGE_MAP()

// Quando si nasconde il titolo lo si porta anche a dimensione minima e si perde
// il current object e la sua evidenziazione
//------------------------------------------------------------------------------
void Table::OnHideTableTitle()
{
	m_bHideTableTitle = !m_bHideTableTitle;
	if (m_bHideTableTitle)
		m_nLastTitleHeight = m_TitleRect.Height();

	// se nascondo i titoli delle colonne allora nascondo anche il titolo della tabella
	if (m_bHideColumnsTitle) m_bHideTableTitle = TRUE;

	m_pDocument->InvalidateRect(m_TitleRect, TRUE);
	VExpandTitle
		(
			m_bHideTableTitle 
			? m_TitleRect.Height() - MINCELL_Y
			:-(m_nLastTitleHeight - MINCELL_Y)
		);
	m_pDocument->InvalidateRect(m_TitleRect, TRUE);

	m_pDocument->m_pCurrentObj = NULL;
	m_pDocument->m_pActiveRect->Clear();

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnHideColumnsTitle()
{
	m_bHideColumnsTitle = !m_bHideColumnsTitle;
	OnHideTableTitle();

	m_pDocument->InvalidateRect(AllColumnsTitleRect(), TRUE);

	m_pDocument->m_pCurrentObj = NULL;
	m_pDocument->m_pActiveRect->Clear();

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
BOOL Table::EasyReadingByLineEnabled ()
{ return m_Borders.m_bRowSeparator; }

BOOL Table::EasyReadingByLineDynamicEnabled ()
{ return m_Borders.m_bRowSeparatorDynamic; }

//------------------------------------------------------------------------------
void Table::OnEasyReadingByLine ()
{
	m_Borders.m_bRowSeparator = !m_Borders.m_bRowSeparator;

	if (m_Borders.m_bRowSeparator)
		m_Borders.m_bRowSeparatorDynamic = FALSE;

	m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnEasyReadingByLineDynamic ()
{
	m_Borders.m_bRowSeparatorDynamic = !m_Borders.m_bRowSeparatorDynamic;

	if (m_Borders.m_bRowSeparatorDynamic)
		m_Borders.m_bRowSeparator = FALSE;

	m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
BOOL Table::EasyReadingByColorEnabled ()
{ return m_bAlternateBkgColorOnRow && m_crAlternateBkgColorOnRow != RGB(255,255,255); }

BOOL Table::EasyReadingByColorDynamicEnabled ()
{ return m_bAlternateBkgColorOnMultiLineRow && m_crAlternateBkgColorOnRow != RGB(255,255,255); }

//------------------------------------------------------------------------------
void Table::OnEasyReadingByColor ()
{
	m_bAlternateBkgColorOnRow = !EasyReadingByColorEnabled();

	m_arAlternateEasyviewOnPage.RemoveAll(); m_bAlternateEasyview = TRUE;
	
	if (m_bAlternateBkgColorOnRow)
	{
		m_bAlternateBkgColorOnMultiLineRow = FALSE;
	
		if (m_crAlternateBkgColorOnRow == RGB(255,255,255))
			m_crAlternateBkgColorOnRow = AfxGetThemeManager()->GetAlternateColor();
	}
	m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnEasyReadingByColorDynamic ()
{
	m_bAlternateBkgColorOnMultiLineRow = !EasyReadingByColorDynamicEnabled();

	m_arAlternateEasyviewOnPage.RemoveAll(); m_bAlternateEasyview = TRUE;
	
	if (m_bAlternateBkgColorOnMultiLineRow)
	{
		m_bAlternateBkgColorOnRow = FALSE;
	
		if (m_crAlternateBkgColorOnRow == RGB(255,255,255))
			m_crAlternateBkgColorOnRow = AfxGetThemeManager()->GetAlternateColor();
	}

	m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnSetHidden ()
{	
	CHiddenWhenDlg   dialog (m_pHideExpr, &(m_pDocument->m_ViewSymbolTable), this);
 
	if (dialog.DoModal() != IDOK) 
		return;
		
	m_pDocument->InvalidateRect(this->m_TitleRect, TRUE);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnUpdatePlusMultiLines ()
{	
	m_arRowAlignInitial.RemoveAll();

	for (int i=0; i < m_Columns.GetSize(); i++)
		m_arRowAlignInitial.Add(GetColumnAlign(i));

	AlignType nAlign;
	
	int newHeight = m_pDocument->m_PageInfo.dmPaperLength + (10 * (LastRow() + 1) * 3);
	if (newHeight > 20000)
		return;  //2108: vecchio valore di altezza impostato di dafault
	
	if (CanVExpandRow (0, 10))
	{		
		m_nRowHeightZoom++;
		VExpandRow(0, 10);
		m_pDocument->m_PageInfo.dmPaperLength = newHeight;
		for (int i = 0; i < m_Columns.GetSize(); i++)
		{
			nAlign = m_arRowAlignInitial.GetAt(i);
			nAlign = nAlign | DT_WORDBREAK;
			nAlign = nAlign & ~DT_SINGLELINE;
			SetColumnAlign(nAlign, i);
			if (m_pDocument->m_pWoormIni->m_bIncludeTotal)
			{
				SetTotalAlign(nAlign, i);
				m_pDocument->InvalidateRect(TotalRect(i), TRUE);
			}
		}
		m_pDocument->Invalidate();
		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
void Table::OnUpdateMinusMultiLines ()
{
	m_arRowAlignInitial.RemoveAll();

	for (int i=0; i < m_Columns.GetSize(); i++)
		m_arRowAlignInitial.Add(GetColumnAlign(i));

	AlignType nAlign;

	if (CanVExpandRow (0, -10) && m_nRowHeightZoom > 0)
	{	
		m_nRowHeightZoom--;
		VExpandRow(0, -10);
		CRect invalidateRect = BodyRect();
		m_pDocument->m_PageInfo.dmPaperLength = m_pDocument->m_PageInfo.dmPaperLength - (10 * (LastRow() + 1) * 3);
		if (m_nRowHeightZoom == 0)
		{
			for (int i = 0; i < m_Columns.GetSize(); i++)
			{
				nAlign = m_arRowAlignInitial.GetAt(i);
				nAlign = nAlign & ~DT_WORDBREAK;
				nAlign = nAlign | DT_SINGLELINE;
				SetColumnAlign(nAlign, i);
				if (m_pDocument->m_pWoormIni->m_bIncludeTotal)
				{
					SetTotalAlign(nAlign, i);
					m_pDocument->InvalidateRect(TotalRect(i), TRUE);
				}
			}
		}
		m_pDocument->Invalidate();
		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}


//------------------------------------------------------------------------------
void Table::OnFiscalEnd()
{
	m_bFiscalEnd = !m_bFiscalEnd;

	m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnTableTitleLineWidth   ()
{
	BorderPen pen = m_TitlePen;
	CLineWidthDlg dialog (pen);

	if (dialog.DoModal() == IDOK)
	{
		m_TitlePen = pen;

		m_pDocument->InvalidateRect(m_TitleRect, m_bTransparent);
		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
void Table::OnTotalLineWidth()
{

	BOOL            bAllColumns = FALSE;
	BorderPen       pen = GetTotalPen(m_nActiveColumn);
	CLineWidthDlg   dialog (pen, bAllColumns);

	if (dialog.DoModal() != IDOK) return;

	if (bAllColumns)
	{
		SetAllTotalsPen (pen);
		m_pDocument->InvalidateRect(AllTotalsRect(), TRUE);
	}
	else
	{
		SetTotalPen (pen, m_nActiveColumn);
		m_pDocument->InvalidateRect(TotalRect(m_nActiveColumn), TRUE);
	}

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}


//------------------------------------------------------------------------------
void Table::OnTotalAlign()
{
	BOOL        bAllColumns = FALSE;
	AlignType   nAlign = GetTotalAlign(m_nActiveColumn);
	CAlignDlg   dialog(IDD_TABLE_ALIGN, nAlign, bAllColumns);

	if (dialog.DoModal() != IDOK) return;

	if (bAllColumns)
	{
		SetAllTotalsAlign (nAlign);
		m_pDocument->InvalidateRect(AllTotalsRect(), TRUE);
	}
	else
	{
		SetTotalAlign (nAlign, m_nActiveColumn);
		m_pDocument->InvalidateRect(TotalRect(m_nActiveColumn), TRUE);
	}

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}


//------------------------------------------------------------------------------
void Table::OnTotalFontStyle()
{
	//@@ TODO gestire bAllColumns
	BOOL        	bAllColumns = FALSE;
	FontIdx     	nFontIdx = GetTotalFontIdx(m_nActiveColumn);
	CFontStylesDlg	dialog(*(m_pDocument->m_pFontStyles), nFontIdx, FALSE, NULL, m_pDocument->GetNamespace(), FALSE, m_pDocument->m_Template.m_bIsTemplate);

	if (dialog.DoModal() != IDOK) 
	{
		if (!m_pDocument->IsModified())
			m_pDocument->SetModifiedFlag(m_pDocument->m_pFontStyles->IsModified());
		return;
	}

	if (bAllColumns)
		SetAllTotalsFontIdx (nFontIdx);
	else
		SetTotalFontIdx (nFontIdx, m_nActiveColumn);

	// must Invalidate all m_pDocument because font style can be used by other objects
	m_pDocument->Invalidate();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnTotalModify()
{
	m_Columns[m_nActiveColumn]->ModifyColumnTotal();
}


//------------------------------------------------------------------------------
void Table::OnColumnTitleLineWidth  ()
{
	BOOL            bAllColumns = FALSE;
	BorderPen       pen = GetColumnTitlePen(m_nActiveColumn);
	CLineWidthDlg   dialog (pen, bAllColumns);

	if (dialog.DoModal() != IDOK) return;

	if (bAllColumns)
	{
		SetAllColumnsTitlePen (pen);
		m_pDocument->InvalidateRect(AllColumnsTitleRect(), m_bTransparent);
	}
	else
	{
		SetColumnTitlePen (pen, m_nActiveColumn);
		m_pDocument->InvalidateRect(ColumnTitleRect(m_nActiveColumn), m_bTransparent);
	}

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnColumnLineWidth ()
{
	BOOL            bAllColumns = FALSE;
	BorderPen       pen = GetColumnPen(m_nActiveColumn);
	CLineWidthDlg   dialog (pen, bAllColumns);

	if (dialog.DoModal() != IDOK) return;

	if (bAllColumns)
	{
		SetAllColumnsPen (pen);
		m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);
		if (m_pDocument->m_pWoormIni->m_bIncludeTotal)
		{
			SetAllTotalsPen (pen);
			m_pDocument->InvalidateRect(AllTotalsRect(), TRUE);
		}
	}
	else
	{
		SetColumnPen (pen, m_nActiveColumn);
		m_pDocument->InvalidateRect(ColumnCellsRect(m_nActiveColumn), m_bTransparent);

		if (m_pDocument->m_pWoormIni->m_bIncludeTotal)
		{
			SetTotalPen (pen, m_nActiveColumn);
			m_pDocument->InvalidateRect(TotalRect(m_nActiveColumn), TRUE);
		}
	}

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnTableTitleAlign()
{
	AlignType   nAlign = m_Title.GetAlign();
	CAlignDlg   dialog(nAlign);

	if (dialog.DoModal() == IDOK)
	{
		m_Title.SetAlign(nAlign);

		m_pDocument->InvalidateRect(m_TitleRect, m_bTransparent);
		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
void Table::OnTableTitleFontStyle()
{
	SetTableTitleFontStyle(FALSE);
}

void Table::OnTableSubTitleFontStyle()
{
	SetTableTitleFontStyle(TRUE);
}

void Table::SetTableTitleFontStyle(BOOL bSubTitle)

{
	FontIdx     	nFontIdx = bSubTitle ? m_SubTitle.GetFontIdx() : m_Title.GetFontIdx();
	CFontStylesDlg	dialog (*(m_pDocument->m_pFontStyles), nFontIdx, FALSE, NULL, m_pDocument->GetNamespace(), FALSE, m_pDocument->m_Template.m_bIsTemplate);

	if (dialog.DoModal() != IDOK)
	{
		if (!m_pDocument->IsModified())
			m_pDocument->SetModifiedFlag(m_pDocument->m_pFontStyles->IsModified());
		return;
	}

	if (bSubTitle)
		m_SubTitle.SetFontIdx(nFontIdx);
	else
		m_Title.SetFontIdx(nFontIdx);

	m_pDocument->Invalidate();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnColumnTitleAlign()
{
	BOOL        bAllColumns = FALSE;
	AlignType   nAlign = GetColumnTitleAlign(m_nActiveColumn);
	CAlignDlg   dialog(IDD_TABLE_ALIGN, nAlign, bAllColumns);

	if (dialog.DoModal() != IDOK) return;

	if (bAllColumns)
	{
		SetAllColumnsTitleAlign (nAlign);
		m_pDocument->InvalidateRect(AllColumnsTitleRect(), m_bTransparent);
	}
	else
	{
		SetColumnTitleAlign (nAlign, m_nActiveColumn);
		m_pDocument->InvalidateRect(ColumnTitleRect(m_nActiveColumn), m_bTransparent);
	}

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnColumnTitleFontStyle()
{
	FontIdx     	nFontIdx = GetColumnTitleFontIdx (m_nActiveColumn);
	
	CFontStylesDlg	dialog (*(m_pDocument->m_pFontStyles), nFontIdx, FALSE, NULL, m_pDocument->GetNamespace(), FALSE, m_pDocument->m_Template.m_bIsTemplate);
	
	dialog.m_bEnableApplyAll = TRUE;

	if (dialog.DoModal() != IDOK)
	{
		if (!m_pDocument->IsModified())
			m_pDocument->SetModifiedFlag(m_pDocument->m_pFontStyles->IsModified());
		return;
	}
	
	if (dialog.m_bApplyAll)
		SetAllColumnsTitleFontIdx (nFontIdx);
	else
		SetColumnTitleFontIdx (nFontIdx, m_nActiveColumn);

	m_pDocument->Invalidate();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnSubTotalFontStyle ()
{
	FontIdx     	nFontIdx = GetSubTotalFontIdx (m_nActiveColumn);
	CFontStylesDlg	dialog(*(m_pDocument->m_pFontStyles), nFontIdx, FALSE, NULL, m_pDocument->GetNamespace(), FALSE, m_pDocument->m_Template.m_bIsTemplate);
	
	dialog.m_bEnableApplyAll = TRUE;

	if (dialog.DoModal() != IDOK)
	{
		if (!m_pDocument->IsModified())
			m_pDocument->SetModifiedFlag(m_pDocument->m_pFontStyles->IsModified());
		return;
	}
	
	if (dialog.m_bApplyAll)
		SetAllSubTotalsFontIdx (nFontIdx);
	else
		SetSubTotalFontIdx (nFontIdx, m_nActiveColumn);

	m_pDocument->Invalidate();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
/*
void Table::OnColumnAlign()
{
	BOOL        bAllColumns = FALSE;
	AlignType   nAlign = GetColumnAlign(m_nActiveColumn);

	if (m_Columns[m_nActiveColumn]->m_bMixedColumnAlign)
	{
		if (AfxMessageBox(_TB("The column has some cells with different alignments.\r\n the default value will be suggested."), MB_OKCANCEL | MB_ICONEXCLAMATION) == IDCANCEL)
			return;

		nAlign = IsShowedAsString(m_pDocument->m_pEditorManager->GetDataType(m_Columns[m_nActiveColumn]->m_wInternalID))
			? DEFAULT_CELL_STRING_ALIGN
			: DEFAULT_CELL_NUM_ALIGN;
	}

	CAlignDlg   dialog(IDD_TABLE_ALIGN, nAlign, bAllColumns);
	WoormIni    ini;

	if (dialog.DoModal() != IDOK) 
		return;

	if (bAllColumns)
	{
		for (int i = 0; i <= m_Columns.GetUpperBound(); i++)
			if (m_Columns[i]->IsMultiRow() && (nAlign & DT_SINGLELINE) == 0)
			{
				AfxMessageBox(_TB("Some columns are in  'Distribute over multiline if too long' status and cannot accept some of the settings entered"), MB_OK | MB_ICONEXCLAMATION);
				break;
			}

		SetAllColumnsAlign (nAlign);

		m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);

		if (ini.m_bIncludeTotal)
		{
			SetAllTotalsAlign (nAlign);

			m_pDocument->InvalidateRect(AllTotalsRect(), TRUE);
		}
	}
	else
	{
		if (ColumnIsMultiRow() && (nAlign & DT_SINGLELINE) == 0)
			AfxMessageBox(_TB("The column is in a 'Distribute on more lines if too long' status and cannot accept some settings."), MB_OK | MB_ICONEXCLAMATION);

		SetColumnAlign (nAlign, m_nActiveColumn);

		m_pDocument->InvalidateRect(ColumnCellsRect(m_nActiveColumn), m_bTransparent);

		if (ini.m_bIncludeTotal)
		{
			SetTotalAlign (nAlign, m_nActiveColumn);
			m_pDocument->InvalidateRect(TotalRect(m_nActiveColumn), TRUE);
		}
	}

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}
*/

//------------------------------------------------------------------------------
void Table::OnColumnFontStyle()
{
	FontIdx     	nFontIdx = GetColumnFontIdx(m_nActiveColumn);

	//if (m_Columns[m_nActiveColumn]->m_bMixedColumnFont)
	//{
	//	if (AfxMessageBox(_TB("The column has some sells with different fonts. \r\nThe default font will be suggested."), MB_OKCANCEL | MB_ICONEXCLAMATION) == IDCANCEL)
	//		return;

	//	nFontIdx = 
	//		IsShowedAsString(m_pDocument->m_pEditorManager->GetDataType(m_Columns[m_nActiveColumn]->m_wInternalID))
	//		? 
	//			m_pDocument->m_pFontStyles->GetFontIdx(FNT_CELL_STRING)
	//		: 
	//			m_pDocument->m_pFontStyles->GetFontIdx(FNT_CELL_NUM);
	//}

	CFontStylesDlg	dialog(*(m_pDocument->m_pFontStyles), nFontIdx, FALSE, NULL, m_pDocument->GetNamespace(), FALSE, m_pDocument->m_Template.m_bIsTemplate);
	
	dialog.m_bEnableApplyAll = TRUE;

	if (dialog.DoModal() != IDOK)
	{
		if (!m_pDocument->IsModified())
			m_pDocument->SetModifiedFlag(m_pDocument->m_pFontStyles->IsModified());
		return;
	}

	if (dialog.m_bApplyAll)
	{
		SetAllColumnsFontIdx (nFontIdx);
		if (m_pDocument->m_pWoormIni->m_bIncludeTotal)
			SetAllTotalsFontIdx (nFontIdx);
	}
	else
	{
		SetColumnFontIdx (nFontIdx, m_nActiveColumn);
		if (m_pDocument->m_pWoormIni->m_bIncludeTotal)
			SetTotalFontIdx (nFontIdx, m_nActiveColumn);
	}

	m_pDocument->Invalidate();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnColumnFormatStyle()
{
	FormatIdx   nFormatIdx = GetColumnFormatIdx(m_nActiveColumn);
	CFormatDlg  dialog(*(m_pDocument->m_pFormatStyles), nFormatIdx, FALSE, NULL, m_pDocument->GetNamespace());

	if (dialog.DoModal() == IDOK)
	{
		// must reformat all because style can used by other object
		SetColumnFormatIdx (nFormatIdx, m_nActiveColumn);
		m_pDocument->FormatAll();
	}

	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleMultiRow()
{
	TableColumn* pCol = m_Columns[m_nActiveColumn];

	pCol->ToggleMultiRow();
	pCol->SetFieldWidth();

	m_pDocument->InvalidateRect(pCol->GetColumnCellsRect(), m_bTransparent);
	m_pDocument->UpdateWindow();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleBitmap()
{
	m_Columns[m_nActiveColumn]->ToggleBitmap();

	m_pDocument->InvalidateRect(ColumnCellsRect(m_nActiveColumn), m_bTransparent);
	m_pDocument->UpdateWindow();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleBarCode()
{
	m_Columns[m_nActiveColumn]->ToggleBarCode();
	m_pDocument->InvalidateRect(ColumnCellsRect(m_nActiveColumn), m_bTransparent);
	m_pDocument->UpdateWindow();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleTextFile()
{
	m_Columns[m_nActiveColumn]->ToggleTextFile();
	m_pDocument->InvalidateRect(ColumnCellsRect(m_nActiveColumn), m_bTransparent);
	m_pDocument->UpdateWindow();
}

//metodo che imposta la colonna attiva come colonna splitter (cioe' spezza la tabella per
//mandarla in stampa) NOTA: Supportato solo se e' presente una sola tabella
//------------------------------------------------------------------------------
void Table::OnColumnToggleSplitter()
{
	m_Columns[m_nActiveColumn]->ToggleSplitter();
}

//metodo che imposta la colonna attiva come colonna fissa (cioe' colonna che deve essere
//stampata in ogni pagina "splittata" del report) NOTA: Supportato solo se e' presente una sola tabella
//------------------------------------------------------------------------------
void Table::OnColumnToggleFixed()		
{
	m_Columns[m_nActiveColumn]->ToggleFixed();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleAnchorLeft()	
{
	m_Columns[m_nActiveColumn]->ToggleAnchorLeft();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleAnchorRight()	
{
	m_Columns[m_nActiveColumn]->ToggleAnchorRight();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleVMergeEmptyCell()
{
	m_Columns[m_nActiveColumn]->ToggleVMergeEmptyCell();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleVMergeEqualCell()
{
	m_Columns[m_nActiveColumn]->ToggleVMergeEqualCell();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleVMergeTailCell()
{
	m_Columns[m_nActiveColumn]->ToggleVMergeTailCell();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleHiddenWhenEmpy()
{
	m_Columns[m_nActiveColumn]->ToggleHiddenWhenEmpy();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleOptimizedWidth()
{
	m_Columns[m_nActiveColumn]->ToggleOptimizedWidth();
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleHidden()	
{
	ColumnToggleHidden(m_nActiveColumn);
}

//------------------------------------------------------------------------------
//From UI
void Table::ColumnToggleHidden(int nCol)
{
	CString labelColumn = m_Columns[nCol]->m_Title.GetText();
	if ( labelColumn.IsEmpty())
		{
			SymField* rp =  m_pDocument->m_pEditorManager->GetSymTable()->GetFieldByID (m_Columns[nCol]->GetInternalID());
			
			labelColumn = _T("(");
			labelColumn.Append(rp->GetName()); 
			labelColumn.Append(_T(")"));
		}
	
	
	CHiddenWhenDlg   dialog (m_Columns[nCol]->m_pHideExpr, m_Columns[nCol]->IsHidden(), labelColumn, m_pDocument->m_pEditorManager->GetSymTable());
 
	if (dialog.DoModal() != IDOK) return;
		
	if (
			(m_Columns[nCol]->m_pHideExpr == NULL) 
			&& 
			( m_Columns[nCol]->IsHidden() != dialog.GetState())
		)
		SetColumnHiddenStatus (nCol, dialog.GetState()); 

	if ( m_Columns[nCol]->m_pHideExpr == NULL && m_Columns[nCol]->IsHidden() )
		m_Columns[nCol]->RemoveSplitter();

	m_pDocument->m_pActiveRect->SetActive(m_TitleRect);
	m_pDocument->Invalidate();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
//From Paint
BOOL Table::CheckHiddenStatus(int nCol)
{
	ASSERT(m_Columns[nCol]->m_pHideExpr != NULL);

	BOOL bSaveHidden = m_Columns[nCol]->IsHidden();

	DataBool bHidden(TRUE);
	if (!m_Columns[nCol]->m_pHideExpr->Eval(bHidden))
	{
		TRACE(_TB("Valutation error in control expression for column visualization:") + m_Columns[nCol]->m_Title.GetText());
		//return FALSE;
	}

	if  (bSaveHidden == bHidden)
		return FALSE;

	SetColumnHiddenStatus(nCol, bHidden);
	return TRUE;
}

//------------------------------------------------------------------------------
void Table::SetColumnHiddenStatus(int nCol, BOOL bHidden)
{	
	int width = bHidden ? m_Columns[nCol]->Width() : m_Columns[nCol]->m_nSavedWidth;

	m_Columns[nCol]->SetHidden(bHidden);

	if (nCol <= m_Columns.GetUpperBound())
	{
		if (bHidden)
		{
			LeftShiftColumn (nCol + 1, width);

			m_nActiveColumn = NO_ACTIVE_COLUMN;
		}
		else
			RightShiftColumn (nCol, width);
	}
	if (bHidden)
	{

	}
}

//------------------------------------------------------------------------------
BOOL Table::CheckDynamicWidth(int nCol)
{
	TableColumn* pCol = m_Columns[nCol];
	ASSERT(pCol->m_pDynamicWidthExpr != NULL);

	DataInt width;
	if (!pCol->m_pDynamicWidthExpr->Eval(width))
	{
		TRACE(_TB("Valutation error in dynamic expression for column width:") + pCol->m_Title.GetText());
		//return FALSE;
	}

	if (width == pCol->Width())
		return FALSE;

	OnColumnSetWidth(pCol, width, FALSE);
	return TRUE;
}

/*
//------------------------------------------------------------------------------
void Table::OnShowAllHiddenColumn()
{
	CColumnArray arHiddenColumns;

	for (int nCol= 0; nCol <= m_Columns.GetUpperBound(); nCol++)
	{
		if 	( m_Columns[nCol]->IsHidden() || m_Columns[nCol]->m_pHideExpr ) 
		{
			arHiddenColumns.Add( m_Columns[nCol] );
		}
	}
 
	if (arHiddenColumns.GetCount() == 0)
		return;
	
	CListHiddenColsDlg   dialog ( &arHiddenColumns, &m_Columns, m_pDocument, this);
	if (dialog.DoModal() != IDOK) return;
}

//------------------------------------------------------------------------------
void Table::OnColumnToggleLink()	
{
	OnColumnManageLink(m_nActiveColumn);
}

//---------------------------------------------------------------------------

void Table::OnColumnManageLink (int nCol)
{
	WORD columnInternalID = m_Columns[nCol]->GetInternalID();
	CString labelColumn = m_Columns[nCol]->m_Title.GetText();
	
	CManageLinkDlg dialog (labelColumn, m_pDocument, columnInternalID);
	
	dialog.DoModal();
}

//------------------------------------------------------------------------------
void Table::OnShowAllLinks()
{
	//TODO show dialog con elenco totale delle colone nascoste
	CColumnArray arLinkColumns;   //colonne che hanno almeno un link associato

	for (int nCol= 0; nCol <= m_Columns.GetUpperBound(); nCol++)
	{
		for (int i = 0; i < m_pDocument->m_arWoormLinks.GetSize(); i++)
		{
			if ( m_Columns[nCol]->GetInternalID() == m_pDocument->m_arWoormLinks.GetAt(i)->m_nAlias)
			{
				arLinkColumns.Add( m_Columns[nCol] );
				break;
			}
		}	
	}
	CAllLinksColsDlg   dialog ( &arLinkColumns, m_pDocument, this);
	if (dialog.DoModal() != IDOK) return;
}
*/
//------------------------------------------------------------------------------
void Table::OnTotalColors()
{
	BOOL        bAllColumns = FALSE, bAborted = FALSE;
	COLORREF    rgbColors[CTotalColorsDlg::MAX];

	GetTotalColor (rgbColors, m_nActiveColumn);

	//copia espressioni originarie, da ripristinare in caso di annullamento dell'overwrite
	Expression* totalTextColorExprTemp = m_Columns[m_nActiveColumn]->m_pTotalTextColorExpr 
											?	new Expression(*(m_Columns[m_nActiveColumn]->m_pTotalTextColorExpr))
											:	NULL;	
	Expression* totalBkgColorExprTemp  = m_Columns[m_nActiveColumn]->m_pTotalBkgColorExpr
											?	new Expression(*(m_Columns[m_nActiveColumn]->m_pTotalBkgColorExpr))
											:	NULL;

	CTotalColorsDlg dialog(
								rgbColors, 
								bAllColumns,
								m_Columns[m_nActiveColumn]->m_pTotalTextColorExpr,
								m_Columns[m_nActiveColumn]->m_pTotalBkgColorExpr,
								&(m_pDocument->m_ViewSymbolTable)
						   );

	if (dialog.DoModal() != IDOK) 
	{
		SAFE_DELETE (totalTextColorExprTemp);
		SAFE_DELETE (totalBkgColorExprTemp);
		return;
	}

	if (bAllColumns)
	{
		SetAllColumnDynamicExpr(m_nActiveColumn, TextColorTotal, m_Columns[m_nActiveColumn]->m_pTotalTextColorExpr, bAborted);
		if (!bAborted) 
			SetAllColumnDynamicExpr(m_nActiveColumn, BkgColorTotal, m_Columns[m_nActiveColumn]->m_pTotalBkgColorExpr, bAborted);
		if (bAborted)
		{	//ripristino espressioni iniziale, perche utente ha annullato l'overwrite su tutte le colonne
			SAFE_DELETE(m_Columns[m_nActiveColumn]->m_pTotalTextColorExpr);
			SAFE_DELETE(m_Columns[m_nActiveColumn]->m_pTotalBkgColorExpr);
			m_Columns[m_nActiveColumn]->m_pTotalTextColorExpr = totalTextColorExprTemp;
			m_Columns[m_nActiveColumn]->m_pTotalBkgColorExpr = totalBkgColorExprTemp;
			m_pDocument->UpdateWindow();
			m_pDocument->SetModifiedFlag();
			return;
		}
		SetAllTotalsColor(rgbColors);
		m_pDocument->InvalidateRect(AllTotalsRect(), TRUE);
	}
	else
	{
		SetTotalColor (rgbColors, m_nActiveColumn);
		m_pDocument->InvalidateRect(TotalRect(m_nActiveColumn), TRUE);
	}

	SAFE_DELETE (totalTextColorExprTemp);
	SAFE_DELETE (totalBkgColorExprTemp);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnTableTitleColors()
{
	COLORREF    rgbColors[CTableTitleColorsDlg::MAX];  // label, back, border, easyview

	GetTableTitleColor (rgbColors);

	CTableTitleColorsDlg dialog(rgbColors);
	if (dialog.DoModal() == IDOK)
	{
		if (m_crAlternateBkgColorOnRow != rgbColors[CTableTitleColorsDlg::EASYVIEW])
			m_pDocument->InvalidateRect(m_BaseCellsRect, m_bTransparent);
		else
			m_pDocument->InvalidateRect(m_TitleRect, m_bTransparent);

		SetTableTitleColor (rgbColors);

		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
void Table::OnColumnTitleColors ()
{
	BOOL        bAllColumns = FALSE, bAborted = FALSE;
	COLORREF    rgbColors[CColumnTitleColorsDlg::MAX];  // label, back, border

	GetColumnTitleColor (rgbColors, m_nActiveColumn);

	//copia espressioni originarie, da ripristinare in caso di annullamento dell'overwrite
	Expression* titleTextColorExprTemp = m_Columns[m_nActiveColumn]->m_pTitleTextColorExpr 
											?	new Expression(*(m_Columns[m_nActiveColumn]->m_pTitleTextColorExpr))
											:	NULL;	
	Expression* titleBkgColorExprTemp  = m_Columns[m_nActiveColumn]->m_pTitleBkgColorExpr
											?	new Expression(*(m_Columns[m_nActiveColumn]->m_pTitleBkgColorExpr))
											:	NULL;
	
	CColumnTitleColorsDlg dialog(
									rgbColors, 
									bAllColumns,
									m_Columns[m_nActiveColumn]->m_pTitleTextColorExpr,
									m_Columns[m_nActiveColumn]->m_pTitleBkgColorExpr,
									&(m_pDocument->m_ViewSymbolTable)
								);

	
	if (dialog.DoModal() != IDOK) 
	{
		SAFE_DELETE (titleTextColorExprTemp);
		SAFE_DELETE (titleBkgColorExprTemp);
		return;
	}

	if (bAllColumns)
	{
		SetAllColumnDynamicExpr(m_nActiveColumn, TextColorTitle, m_Columns[m_nActiveColumn]->m_pTitleTextColorExpr,	bAborted);
		if (!bAborted) 
			SetAllColumnDynamicExpr(m_nActiveColumn, BkgColorTitle, m_Columns[m_nActiveColumn]->m_pTitleBkgColorExpr, bAborted);
		if (bAborted)
		{	//ripristino espressioni iniziale, perche utente ha annullato l'overwrite su tutte le colonne
			SAFE_DELETE(m_Columns[m_nActiveColumn]->m_pTitleTextColorExpr);
			SAFE_DELETE(m_Columns[m_nActiveColumn]->m_pTitleBkgColorExpr);
			m_Columns[m_nActiveColumn]->m_pTitleTextColorExpr = titleTextColorExprTemp;
			m_Columns[m_nActiveColumn]->m_pTitleBkgColorExpr = titleBkgColorExprTemp;
			m_pDocument->UpdateWindow();
			m_pDocument->SetModifiedFlag();
			return;
		}
		SetAllColumnsTitleColor(rgbColors);
		m_pDocument->InvalidateRect(AllColumnsTitleRect(), m_bTransparent);
	}
	else
	{
		SetColumnTitleColor (rgbColors, m_nActiveColumn);
		m_pDocument->InvalidateRect(ColumnTitleRect(m_nActiveColumn), m_bTransparent);
	}

	SAFE_DELETE (titleTextColorExprTemp);
	SAFE_DELETE (titleBkgColorExprTemp);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}


//------------------------------------------------------------------------------
void Table::OnSubTotalColors ()
{
	BOOL        bAllColumns = FALSE, bAborted = FALSE;
	COLORREF    rgbColors[CSubTotalColorsDlg::MAX];  // value, back

	GetSubTotalColor (rgbColors, m_nActiveColumn);

	//copia espressioni originarie, da ripristinare in caso di annullamento dell'overwrite
	Expression* subTotalTextColorExprTemp = m_Columns[m_nActiveColumn]->m_SubTotal.m_pTextColorExpr
											?	new Expression(*(m_Columns[m_nActiveColumn]->m_SubTotal.m_pTextColorExpr))
											:	NULL;	
	Expression* subTotalBkgColorExprTemp  = m_Columns[m_nActiveColumn]->m_SubTotal.m_pBkgColorExpr
											?	new Expression(*(m_Columns[m_nActiveColumn]->m_SubTotal.m_pBkgColorExpr))
											:	NULL;

	CSubTotalColorsDlg dialog(	
								rgbColors, 
								bAllColumns,
								m_Columns[m_nActiveColumn]->m_SubTotal.m_pTextColorExpr,
								m_Columns[m_nActiveColumn]->m_SubTotal.m_pBkgColorExpr,
								&(m_pDocument->m_ViewSymbolTable)
							  );

	if (dialog.DoModal() != IDOK)
	{
		SAFE_DELETE (subTotalTextColorExprTemp);
		SAFE_DELETE (subTotalBkgColorExprTemp);
		return;
	}

	if (bAllColumns)
	{
		SetAllColumnDynamicExpr(m_nActiveColumn, TextColorSubTotal, m_Columns[m_nActiveColumn]->m_SubTotal.m_pTextColorExpr,	bAborted);
		if (!bAborted) 
			SetAllColumnDynamicExpr(m_nActiveColumn, BkgColorSubTotal, m_Columns[m_nActiveColumn]->m_SubTotal.m_pBkgColorExpr, bAborted);
		if (bAborted)
		{	//ripristino espressioni iniziale, perche utente ha annullato l'overwrite su tutte le colonne
			SAFE_DELETE(m_Columns[m_nActiveColumn]->m_SubTotal.m_pTextColorExpr);
			SAFE_DELETE(m_Columns[m_nActiveColumn]->m_SubTotal.m_pBkgColorExpr);
			m_Columns[m_nActiveColumn]->m_SubTotal.m_pTextColorExpr = subTotalTextColorExprTemp;
			m_Columns[m_nActiveColumn]->m_SubTotal.m_pBkgColorExpr = subTotalBkgColorExprTemp;
			m_pDocument->UpdateWindow();
			m_pDocument->SetModifiedFlag();
			return;
		}
		SetAllSubTotalsColor(rgbColors);
		m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);

	}
	else
	{
		SetSubTotalColor (rgbColors, m_nActiveColumn);
		m_pDocument->InvalidateRect(ColumnRect(m_nActiveColumn), m_bTransparent);
	}

	SAFE_DELETE (subTotalTextColorExprTemp);
	SAFE_DELETE (subTotalBkgColorExprTemp);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnColumnColor ()
{
	BOOL        bAllColumns = FALSE, bAborted = FALSE;
	COLORREF    rgbColors[CColumnColorsDlg::MAX];  // value, back, border

	// any cell color can be used. default use cell in nRow 0
	// because almost one nRow must exist
	GetCellColor (rgbColors, 0, m_nActiveColumn);

	//copia espressioni originarie, da ripristinare in caso di annullamento dell'overwrite
	Expression* textColorExprTemp = m_Columns[m_nActiveColumn]->m_pTextColorExpr 
									?	new Expression(*(m_Columns[m_nActiveColumn]->m_pTextColorExpr))
									:	NULL;	
	Expression* bkgColorExprTemp  = m_Columns[m_nActiveColumn]->m_pBkgColorExpr
									?	new Expression(*(m_Columns[m_nActiveColumn]->m_pBkgColorExpr))
									:	NULL;
	CColumnColorsDlg dialog(
								rgbColors, 
								bAllColumns,
								m_Columns[m_nActiveColumn]->m_pTextColorExpr,
								m_Columns[m_nActiveColumn]->m_pBkgColorExpr,
								&(m_pDocument->m_ViewSymbolTable)	
							);

	if (dialog.DoModal() != IDOK) 
	{
		SAFE_DELETE (textColorExprTemp);
		SAFE_DELETE (bkgColorExprTemp);
		return;
	}

	if (bAllColumns)
	{
		SetAllColumnDynamicExpr(m_nActiveColumn, TextColorCell, m_Columns[m_nActiveColumn]->m_pTextColorExpr,	bAborted);
		if (!bAborted)
			SetAllColumnDynamicExpr(m_nActiveColumn, BkgColorCell, m_Columns[m_nActiveColumn]->m_pBkgColorExpr, bAborted);
		if (bAborted)
		{	//ripristino espressioni iniziale, perche utente ha annullato l'overwrite su tutte le colonne
			SAFE_DELETE(m_Columns[m_nActiveColumn]->m_pTextColorExpr);
			SAFE_DELETE(m_Columns[m_nActiveColumn]->m_pBkgColorExpr);
			m_Columns[m_nActiveColumn]->m_pTextColorExpr = textColorExprTemp;
			m_Columns[m_nActiveColumn]->m_pBkgColorExpr = bkgColorExprTemp;
			m_pDocument->UpdateWindow();
			m_pDocument->SetModifiedFlag();
			return;
		}
		
		SetAllCellsColor (rgbColors);
		m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);
		if (m_pDocument->m_pWoormIni->m_bIncludeTotal)
		{
			SetAllTotalsColor (rgbColors);
			m_pDocument->InvalidateRect(AllTotalsRect(), TRUE);
		}
	}
	else
	{
		SetColumnCellsColor (rgbColors, m_nActiveColumn);
		m_pDocument->InvalidateRect(ColumnCellsRect(m_nActiveColumn), m_bTransparent);
		if (m_pDocument->m_pWoormIni->m_bIncludeTotal)
		{
			SetTotalColor (rgbColors, m_nActiveColumn);
			m_pDocument->InvalidateRect(TotalRect(m_nActiveColumn), TRUE);
		}
	}
	
	SAFE_DELETE (textColorExprTemp);
	SAFE_DELETE (bkgColorExprTemp);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnCellColor ()
{
	CCellColorsDlg::What what = CCellColorsDlg::CELL;
	COLORREF    rgbColors[CCellColorsDlg::MAX];  // value, back, border

	GetCellColor (rgbColors, m_nActiveRow, m_nActiveColumn);

	CCellColorsDlg dialog(rgbColors, what);

	if (dialog.DoModal() != IDOK) return;

	switch (what)
	{
	case CCellColorsDlg::CELL:

		SetCellColor (rgbColors, m_nActiveRow, m_nActiveColumn);
		m_pDocument->InvalidateRect(CellRect(m_nActiveRow, m_nActiveColumn), m_bTransparent);
		break;

	case CCellColorsDlg::ROW:

		SetRowColor(rgbColors, m_nActiveRow);
		m_pDocument->InvalidateRect(RowRect(m_nActiveRow), m_bTransparent);
		break;

	case CCellColorsDlg::COLUMN:

		SetColumnCellsColor(rgbColors, m_nActiveColumn);
		m_pDocument->InvalidateRect(ColumnCellsRect(m_nActiveColumn), m_bTransparent);
		break;

	case CCellColorsDlg::ALL:

		SetAllCellsColor (rgbColors);
		m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);
		break;
	}

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnCellAlign ()
{
	BOOL        bAllColumns = FALSE;
	AlignType   nAlign = GetCellAlign (m_nActiveRow, m_nActiveColumn);
	CAlignDlg   dialog(IDD_CELL_ALIGN, nAlign, bAllColumns);

	if (dialog.DoModal() != IDOK) return;

	if (bAllColumns)
	{
		for (int i = 0; i <= m_Columns.GetUpperBound(); i++)
			if (m_Columns[i]->IsMultiRow() && (nAlign & DT_SINGLELINE) == 0)
			{
				AfxMessageBox(_TB("Some columns are in  'Distribute over multiline if too long' status and cannot accept some of the settings entered"), MB_OK | MB_ICONEXCLAMATION);
				break;
			}

		SetRowAlign(nAlign, m_nActiveRow);
		m_pDocument->InvalidateRect(RowRect(m_nActiveRow), m_bTransparent);
	}
	else
	{
		if (ColumnIsMultiRow() && (nAlign & DT_SINGLELINE) == 0)
			AfxMessageBox(_TB("The column is in a 'Distribute on more lines if too long' status and cannot accept some settings."), MB_OK | MB_ICONEXCLAMATION);

		SetCellAlign (nAlign, m_nActiveRow, m_nActiveColumn);
		m_pDocument->InvalidateRect(CellRect(m_nActiveRow, m_nActiveColumn), m_bTransparent);
	}

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnCellFontStyle ()
{
	//@@ TODO gestire bAllColumns
	BOOL        	bAllColumns = FALSE;
	FontIdx     	nFontIdx = GetCellFontIdx (m_nActiveRow, m_nActiveColumn);
	CFontStylesDlg	dialog(*(m_pDocument->m_pFontStyles), nFontIdx, FALSE, NULL, m_pDocument->GetNamespace(), FALSE, m_pDocument->m_Template.m_bIsTemplate);

	if (dialog.DoModal() != IDOK)
	{
		if (!m_pDocument->IsModified())
			m_pDocument->SetModifiedFlag(m_pDocument->m_pFontStyles->IsModified());
		return;
	}

	if (bAllColumns)
		SetRowFontIdx (nFontIdx, m_nActiveRow);
	else
		SetCellFontIdx (nFontIdx, m_nActiveRow, m_nActiveColumn);

	m_pDocument->Invalidate();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnCellBarCodeAttrs ()
{
	CBarCodeAttrsDlg dialog(m_Columns[m_nActiveColumn]->GetBarCode(), &m_pDocument->m_ViewSymbolTable);

	if ((dialog.DoModal() != IDOK)) return;

	m_pDocument->InvalidateRect(m_BaseRect, m_bTransparent);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}


//------------------------------------------------------------------------------
void Table::OnTableTitle ()
{
	if (m_Title.SetTextDlg() == IDOK)
	{
		m_Title.Format (m_wInternalID, m_pDocument);

		m_pDocument->InvalidateRect(m_TitleRect, m_bTransparent);
		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}


//------------------------------------------------------------------------------
void Table::OnTableMove ()
{
	CRect   rectPrev = m_BaseRect;
	CObjValueDlg dialog(rectPrev, TRUE);

	if ((dialog.DoModal() == IDOK) && (rectPrev != m_BaseRect))
	{
		CSize offset
				(
					rectPrev.left - m_BaseRect.left,
					rectPrev.top - m_BaseRect.top
				);

		MoveObject(offset);
		// refresh active status if this are current
		if (m_pDocument->m_pCurrentObj == this)
			m_pDocument->m_pActiveRect->SetActive (GetActiveRect());

		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
void Table::OnColumnTitle ()
{
	int nCol = NormalizedColumn();

	if (m_Columns[nCol]->SetColumnTitle() == IDOK)
	{
		m_pDocument->InvalidateRect(ColumnTitleRect(nCol), m_bTransparent);
		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
void Table::OnBorders ()
{
	CTableBordersDlg dialog(this, m_Borders);
	if (dialog.DoModal() == IDOK)
	{
		m_pDocument->InvalidateRect(m_BaseRect, m_bTransparent);
		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
//void Table::OnSubTotalDefinition ()
//{
//	m_pDocument->m_pEditorManager->ShowSubTotals(m_wInternalID);
//}

//------------------------------------------------------------------------------
void Table::OnSetTrasparent ()
{
	m_bTransparent = !m_bTransparent;

	m_pDocument->InvalidateRect(m_BaseRect, TRUE);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::SetAllColumnsPen (const BorderPen& aPen)
{
	int ub = m_Columns.GetUpperBound();
	for (int i =0; i <= ub; i++)
		SetColumnPen (aPen, i);
}

//------------------------------------------------------------------------------
void Table::SetAllTotalsPen (const BorderPen& aPen)
{
	int ub = m_Columns.GetUpperBound();
	for (int i =0; i <= ub; i++)
		SetTotalPen (aPen, i);
}

//------------------------------------------------------------------------------
void Table::SetAllColumnsTitlePen (const BorderPen& aPen)
{
	int ub = m_Columns.GetUpperBound();
	for (int i =0; i <= ub; i++)
		SetColumnTitlePen (aPen, i);
}

//------------------------------------------------------------------------------
void Table::SetRowColor (COLORREF* pColors, int nRow)
{
	int ub = m_Columns.GetUpperBound();
	for (int i =0; i <= ub; i++)
		SetCellColor(pColors, nRow, i);
}

//------------------------------------------------------------------------------
void Table::SetAllColumnDynamicExpr (int nActiveColumn, EnumDynamicExpr exprType, Expression* pSourceExpr, BOOL& bAborted)
{
	BOOL bOverwriteAll = TRUE, bOldExprFound = FALSE;
	Expression* pNewExpr = NULL;
	
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		TableColumn* pTableColumn = (TableColumn*) m_Columns.GetAt(i);
		if (i != nActiveColumn && pTableColumn->GetDynamicExpr(exprType))
		{
			bOldExprFound = TRUE;
			break;
		}
	}
	if (bOldExprFound)
	{
		int	nID = AfxMessageBox( 
								 _TB("There was detected saved expressions in others columns. Do you want overwrite them?"),
								 MB_YESNOCANCEL
								);
		switch (nID)
		{
			case IDCANCEL:
				{
					bAborted = TRUE;
					return;       // don't continue
				}
			case IDYES:
				{
					//saving expression for all column overwriting existing color expressions
					for (int i = 0; i <= ub; i++)
					{
						TableColumn* pTableColumn = (TableColumn*)m_Columns.GetAt(i);
						if ( i != nActiveColumn )
						{
							SAFE_DELETE(pTableColumn->GetDynamicExpr(exprType));
							if (pSourceExpr)
								pNewExpr = new Expression(*pSourceExpr);
							pTableColumn->GetDynamicExpr(exprType) = pNewExpr;
						}
					}	
					break;
				}

			case IDNO:
				{
					//saving expression for all column previously empty (doesn't overwrite existing color expressions) 
					for (int i = 0; i <= ub; i++)
					{
						TableColumn* pTableColumn = (TableColumn*)m_Columns.GetAt(i);

						if (pTableColumn->GetDynamicExpr(exprType) == NULL)
						{
							if (pSourceExpr != NULL)
								pNewExpr = new Expression(*pSourceExpr);
							pTableColumn->GetDynamicExpr(exprType) = pNewExpr;
						}
					}	
					break;
				}
		}
	}
	else
	{ //saving expression for all column 
		for (int i = 0; i <= ub; i++)
		{
			TableColumn* pTableColumn = (TableColumn*)m_Columns.GetAt(i);
			if (i != nActiveColumn)
			{
				if (pSourceExpr != NULL)
					pNewExpr = new Expression(*pSourceExpr);
				pTableColumn->GetDynamicExpr(exprType) = pNewExpr;		
			}
		}	
	}
}

//------------------------------------------------------------------------------
void Table::SetAllCellsColor (COLORREF* pColors)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetColumnCellsColor(pColors, i);
}

//------------------------------------------------------------------------------
void Table::SetAllColumnsTitleColor (COLORREF* pColors)
{
	int ub = m_Columns.GetUpperBound();
	for (int i =0; i <= ub; i++)
		SetColumnTitleColor (pColors, i);
}

//------------------------------------------------------------------------------
void Table::SetAllColumnTitleTextColor(COLORREF color)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetColumnTitleTextColor(color, i);
}

//------------------------------------------------------------------------------
void Table::SetAllColumnTitleBkgColor(COLORREF color)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetColumnTitleBkgColor(color, i);
}

//------------------------------------------------------------------------------
void Table::SetAllColumnTitleBorderColor(COLORREF color)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetColumnTitleBorderColor(color, i);
}

//------------------------------------------------------------------------------
void Table::SetAllColumnTitleBorderWidth(int width)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetColumnTitleBorderWidth(width, i);
}

//------------------------------------------------------------------------------
void Table::SetAllSubTotalsColor (COLORREF* pColors)
{
	int ub = m_Columns.GetUpperBound();
	for (int i =0; i <= ub; i++)
		SetSubTotalColor (pColors, i);
}

//------------------------------------------------------------------------------
void Table::SetAllTotalsColor (COLORREF* pColors)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetTotalColor (pColors, i);
}

//------------------------------------------------------------------------------
void Table::SetAllSubTotalsTextColor(COLORREF color)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		m_Columns[i]->m_SubTotal.m_rgbTextColor = color;
}

//------------------------------------------------------------------------------
void Table::SetAllSubTotalsBkgColor(COLORREF color)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		m_Columns[i]->m_SubTotal.m_rgbBkgColor = color;
}

//------------------------------------------------------------------------------
void Table::SetAllTotalsTextColor(COLORREF color)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetTotalTextColor(color, i);
}

//------------------------------------------------------------------------------
void Table::SetAllTotalsBkgColor(COLORREF color)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetTotalBkgColor(color, i);
}

//------------------------------------------------------------------------------
void Table::SetAllTotalsBorderColor(COLORREF color)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetTotalBorderColor(color, i);
}

//------------------------------------------------------------------------------
void Table::SetAllTotalsBorderWidth(int width)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetTotalBorderWidth(width, i);
}


//------------------------------------------------------------------------------
void Table::SetBodyBkgColor(COLORREF color, int nCol)
{
	m_Columns[nCol]->SetAllCellsBkgColor(color);
}

//------------------------------------------------------------------------------
void Table::SetBodyTextColor(COLORREF color, int nCol)
{
	m_Columns[nCol]->SetAllCellsTextColor(color);
}

//------------------------------------------------------------------------------
void Table::SetBodyBorderWidth(int width)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		m_Columns[i]->m_ColumnPen.m_nWidth = width;
}

//------------------------------------------------------------------------------
void Table::SetAllBodyBkgColor(COLORREF color)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetBodyBkgColor(color, i);
}


//------------------------------------------------------------------------------
void Table::SetAllBodyTextColor(COLORREF color)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetBodyTextColor(color, i);
}


//---------------------------------------------------------------------------
void Table::SetTableTitleColor (COLORREF* pColors)
{
	m_Title.SetTextColor	(pColors[CTableTitleColorsDlg::LABEL]);
	m_Title.SetBkgColor		(pColors[CTableTitleColorsDlg::BACKGROUND]);
	m_TitlePen.SetColor   	(pColors[CTableTitleColorsDlg::BORDER]);
	m_crAlternateBkgColorOnRow     = pColors[CTableTitleColorsDlg::EASYVIEW];
}


//---------------------------------------------------------------------------
void Table::GetTableTitleColor (COLORREF* pColors) const
{
	pColors[CTableTitleColorsDlg::LABEL]        = m_Title.GetTextColor();
	pColors[CTableTitleColorsDlg::BACKGROUND]   = m_Title.GetBkgColor();
	pColors[CTableTitleColorsDlg::BORDER]       = m_TitlePen.GetColor();
	pColors[CTableTitleColorsDlg::EASYVIEW]     = m_crAlternateBkgColorOnRow;
}

//------------------------------------------------------------------------------
void Table::SetRowAlign (AlignType wAlign, int nRow)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetCellAlign(wAlign, nRow, i);
}

//------------------------------------------------------------------------------
void Table::SetAllColumnsAlign (AlignType wAlign)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetColumnAlign (wAlign, i);
}

//------------------------------------------------------------------------------
void Table::SetAllTotalsAlign (AlignType wAlign)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetTotalAlign (wAlign, i);
}

//------------------------------------------------------------------------------
void Table::SetAllColumnsTitleAlign (AlignType wAlign)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetColumnTitleAlign (wAlign, i);
}

//------------------------------------------------------------------------------
void Table::SetRowFontIdx (FontIdx nIdx, int nRow)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetCellFontIdx(nIdx, nRow, i);
}

//------------------------------------------------------------------------------
void Table::SetAllColumnsFontIdx (FontIdx nIdx)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetColumnFontIdx (nIdx, i);
}

//------------------------------------------------------------------------------
void Table::SetAllTotalsFontIdx (FontIdx nIdx)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetTotalFontIdx (nIdx, i);
}

//------------------------------------------------------------------------------
void Table::SetAllColumnsTitleFontIdx (FontIdx nIdx)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetColumnTitleFontIdx (nIdx, i);
}

//------------------------------------------------------------------------------
void Table::SetAllSubTotalsFontIdx (FontIdx nIdx)
{
	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
		SetSubTotalFontIdx (nIdx, i);
}

// in caso di cambio di tipo devo anche aggiornare il formattatore
//------------------------------------------------------------------------------
void Table::OnModifyField()
{
	if (m_nActiveColumn == NO_ACTIVE_COLUMN) return;

	WORD wID = m_Columns[m_nActiveColumn]->GetInternalID();
	DataType olddtype = m_pDocument->m_pEditorManager->GetDataType(wID);

	if (m_pDocument->m_pEditorManager->ModifyField(wID))
	{
		DataType dtype = m_pDocument->m_pEditorManager->GetDataType(wID);
		if (olddtype != dtype)
			m_Columns[m_nActiveColumn]->UpdateTypeInfo();
	}
}


//------------------------------------------------------------------------------
void Table::ShowTotalPopup (CPoint ptPoint, CBCGPScrollView* pView)
{
	//CMenu   menu;
	//menu.CreatePopupMenu();

	//pView->ClientToScreen (&ptPoint);
	//menu.TrackPopupMenu (TPM_LEFTBUTTON, ptPoint.x, ptPoint.y, this);
}

//------------------------------------------------------------------------------
void Table::ShowTablePopup (CPoint ptPoint, CBCGPScrollView* pView)
{
	CMenu   menu;
	menu.CreatePopupMenu();

	m_pDocument->BringToFrontPopup (this, menu);

	ShowTablePopup (menu);
		
	pView->ClientToScreen (&ptPoint);
	menu.TrackPopupMenu (TPM_LEFTBUTTON, ptPoint.x, ptPoint.y, this);
}

//------------------------------------------------------------------------------
void Table::ShowTablePopup (CMenu& menu)
{
	CString s = GetDescription(FALSE);
	if (!s.IsEmpty())
		menu.AppendMenu(MF_STRING|MF_GRAYED, 0, s);

	if (m_pDocument->m_Template.m_bIsTemplate || m_pDocument->m_Template.m_bLoaded)
	{

		if (m_pDocument->m_Template.m_bIsTemplate)
		{
			CString s = _TB("Set new Class Style Name...");
			if (!m_sStyleClass.IsEmpty())
			{
				s += _T(" (") + m_sStyleClass + ')';
			}
			menu.AppendMenu(MF_SEPARATOR);

			menu.AppendMenu(MF_STRING, ID_SET_NEW_STYLE_NAME, s);

			menu.AppendMenu(MF_STRING | (m_bTemplate ? MF_CHECKED : 0), ID_TOGGLE_TEMPLATE, _TB("It is a Class Style &Template"));
		}
	}
	//-----
	
	if (m_pDocument->m_Layouts.GetCount() > 1)
	{
		menu.AppendMenu(MF_SEPARATOR);

		menu.AppendMenu(MF_STRING, ID_LAYOUT_CLONE_OBJECT,		_TB("Clone into an other layout..."));
		menu.AppendMenu(MF_STRING, ID_LAYOUT_MOVE_OBJECT,		_TB("Move to an other layout..."));
	}
}

//------------------------------------------------------------------------------
void Table::ShowColumnPopup (CPoint ptPoint, CBCGPScrollView* pView)
{
	CMenu   menu;
	menu.CreatePopupMenu();

	m_pDocument->BringToFrontPopup (this, menu);

	ShowColumnPopup (menu);
		
	pView->ClientToScreen (&ptPoint);
	menu.TrackPopupMenu (TPM_LEFTBUTTON, ptPoint.x, ptPoint.y, this);
}

//------------------------------------------------------------------------------
void Table::ShowColumnPopup (CMenu& menu)
{
	//if (m_bHideTableTitle)
	//{
	//	CMenu table_menu;
	//	table_menu.CreatePopupMenu();

	//	ShowTablePopup (table_menu);

	//	menu.AppendMenu(MF_STRING | MF_POPUP, (UINT) table_menu.m_hMenu, _TB("Table style"));
	//}
			
	TableColumn* col = m_Columns[m_nActiveColumn];

	if (m_pDocument->m_Template.m_bIsTemplate || m_pDocument->m_Template.m_bLoaded)
	{
		if (m_pDocument->m_Template.m_bIsTemplate)
		{
			CString s = _TB("Set new Class Style Name...");
			if (!col->m_sStyleClass.IsEmpty())
			{
				s += _T(" (") + col->m_sStyleClass + ')';
			}
			menu.AppendMenu(MF_STRING, ID_COLUMN_SET_STYLE_NAME, s);
			menu.AppendMenu(MF_STRING | (col->m_bTemplate ? MF_CHECKED : 0), ID_COLUMN_TOGGLE_TEMPLATE, _TB("It is a Class Style &Template"));
		}
	}	
}

//------------------------------------------------------------------------------
void Table::ShowCellPopup (CPoint ptPoint, CBCGPScrollView* pView)
{
	CMenu menu;
	
	menu.CreatePopupMenu();

	m_pDocument->BringToFrontPopup (this, menu);
	
	pView->ClientToScreen (&ptPoint);
	menu.TrackPopupMenu (TPM_LEFTBUTTON, ptPoint.x, ptPoint.y, this);
}

//------------------------------------------------------------------------------
void Table::OnUpdateShowProportional	()
{
	if ( m_nActiveColumn < 0 || m_nActiveColumn > LastColumn() )
		return;

	if ( ColumnCanBeBitmap() && ColumnIsBitmap() ) 
	{
		TableColumn* col = m_Columns[m_nActiveColumn];
		
		if ( col->m_pBitmap )
		{
			if (col->m_pBitmap->m_ImageFitMode == CTBPicture::ImageFitMode::BEST)
				col->m_pBitmap->m_ImageFitMode = CTBPicture::ImageFitMode::STRETCH;
			else
				col->m_pBitmap->m_ImageFitMode = CTBPicture::ImageFitMode::BEST;

			m_pDocument->Invalidate();
			m_pDocument->UpdateWindow();
		}
	}
}

//------------------------------------------------------------------------------
void Table::Format()
{
	m_Title.Format(m_wInternalID, m_pDocument);

	m_pDocument->InvalidateRect(m_TitleRect, m_bTransparent);

	int ub = m_Columns.GetUpperBound();
	for (int nCol= 0; nCol <= ub; nCol++)
	{
		m_Columns[nCol]->Format();
	}
}

//  fundamental value for reconstruct table:
//      title_height
//      nColumnTitleHeight
//      nRowHeight
//      TotalHeight
//      column_title_width[nCol]
//
//------------------------------------------------------------------------------
void Table::Unparse (ViewUnparser& ofile)
{
	//---- Template Override
	Table* pDefault = m_pDefault;
	if (m_bTemplate && m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		m_pDefault = NULL;
	}
	//----

	ofile.UnparseTable  (LastRow() + 1, LastColumn() + 1,   FALSE); 
	ofile.UnparseString
			(
				ofile.IsLocalizableTextInCurrentLanguage() ?
					ofile.LoadReportString(m_Title.GetText()) : 
					m_Title.GetText(), 
				FALSE
			);
	ofile.UnparseAlias  (m_wInternalID,                     FALSE);
	ofile.Write	(_T(" /* ") + GetName() +  _T(" */ "),	FALSE);

	ofile.UnparseOrigin (m_TitleRect.TopLeft(),             FALSE);

	ofile.UnparseHeights
	(
		m_TitleRect.          Height(),
		ColumnTitleRect(0). Height(),
		CellRect(0,0).      Height(),
		TotalRect(0).       Height()
	);

	if (m_nLayer)
	{
		ofile.UnparseTag(T_LAYER, FALSE);
		ofile.UnparseInt(m_nLayer);
	}

	UnparseTitleAndBorders	(ofile);
	UnparseColumnsBlock		(ofile);

	ofile.UnparseCrLf   ();
	ofile.UnparseCrLf   ();
	
	//----
	m_pDefault = pDefault;
}

//------------------------------------------------------------------------------
void Table::UnparseAllColumns (ViewUnparser& ofile)	
{
	int ub = m_Columns.GetUpperBound();
	for (int nCol= 0; nCol <= ub; nCol++)
	{
		m_Columns[nCol]->Unparse(ofile);
	}
}

//------------------------------------------------------------------------------
void Table::UnparseColumnsBlock (ViewUnparser& ofile)	
{
	ofile.UnparseBegin  ();
	
	UnparseAllColumns	(ofile);

	//attributi statici grafici della tabella
	UnparseTableAttrib	(ofile);	//TODO LUCA posticipare inizialmente
	
	ofile.UnparseEnd    ();
}

//------------------------------------------------------------------------------
void Table::UnparseTitleAndBorders (ViewUnparser& ofile)	
{
	if (!m_sStyleClass.IsEmpty())
	{
		ofile.UnparseTag (T_STYLE, FALSE);
		ofile.UnparseString(m_sStyleClass);
	}
	if (m_bTemplate)
		ofile.UnparseTag (T_TEMPLATE);

	if (m_bHideTableTitle && !m_bHideColumnsTitle)
		ofile.UnparseTag (T_HIDE_TABLE_TITLE);

	if (m_bHideColumnsTitle)
		ofile.UnparseTag (T_HIDE_ALL_TABLE_TITLE);

	if (m_bAlternateBkgColorOnRow || m_bAlternateBkgColorOnMultiLineRow)
	{
		ofile.UnparseTag (T_EASYVIEW, FALSE);

		if (m_bAlternateBkgColorOnMultiLineRow)
		{
			 ofile.UnparseTag (T_DYNAMIC, FALSE);
		}

		if (m_crAlternateBkgColorOnRow != AfxGetThemeManager()->GetAlternateColor())
		{
			 ofile.UnparseColor	(T_COLOR, m_crAlternateBkgColorOnRow, FALSE);
		}
		ofile.UnparseCrLf ();
	}

	if (m_nDropShadowHeight > 0)
	{
		ofile.UnparseTag	(T_DROPSHADOW, FALSE);
		ofile.UnparseInt	(m_nDropShadowHeight, FALSE);
		ofile.UnparseBlank	(FALSE);
		ofile.UnparseColor	(T_COLOR, m_crDropShadowColor);
	}

	if (m_bTransparent)
		ofile.UnparseTag (T_TRANSPARENT);

	if (m_bFiscalEnd)
		ofile.UnparseTag (T_FISCAL_END);

	UnparseHidden (ofile);

	UnparseTitle(ofile);
	UnparseSubTitle(ofile);

	// unparsa i bordi della tabella
	m_Borders.Unparse   (ofile);
}

//------------------------------------------------------------------------------
void Table::UnparseTitle(ViewUnparser& ofile)
{
	BOOL col	= m_Title.m_rgbTextColor	!= DEFAULT_TABLE_TITLE_FOREGROUND;
	BOOL bkg    = m_Title.m_rgbBkgColor		!= DEFAULT_TABLE_TITLE_BACKGROUND;
	BOOL alg    = m_Title.m_nAlign			!= DEFAULT_TABLE_TITLE_ALIGN;
	BOOL fnt    = m_Title.m_nFontIdx		!= m_pDocument->m_pFontStyles->GetFontIdx(FNT_TABLE_TITLE);
	BOOL pen    = !m_TitlePen.IsDefault();
	WORD wbr    = (WORD) (col + bkg + alg + fnt + pen);
	BOOL blk	= wbr > 1;
	
	if (wbr)	ofile.UnparseTag		(T_TITLE, blk);
	if (blk)	ofile.UnparseBegin		();
	if (col)    ofile.UnparseTextColor	(m_Title.m_rgbTextColor);
	if (bkg)    ofile.UnparseBkgColor	(m_Title.m_rgbBkgColor);
	if (alg)    ofile.UnparseAlign		(m_Title.m_nAlign);
	if (fnt)    ofile.UnparseFont		(m_pDocument->m_pFontStyles, m_Title.m_nFontIdx);
	if (pen)    ofile.UnparsePen		(m_TitlePen);
	if (blk)	ofile.UnparseEnd		();
}

//------------------------------------------------------------------------------
void Table::UnparseSubTitle(ViewUnparser& ofile)
{
	BOOL col = m_SubTitle.m_rgbTextColor != DEFAULT_SUBTOTAL_FOREGROUND;
	BOOL bkg = m_SubTitle.m_rgbBkgColor != DEFAULT_SUBTOTAL_BACKGROUND;
	BOOL alg = m_SubTitle.m_nAlign != DEFAULT_SUBTOTAL_STRING_ALIGN;
	BOOL fnt = m_SubTitle.m_nFontIdx != m_pDocument->m_pFontStyles->GetFontIdx(FNT_SUBTOTAL_STRING);
	WORD wbr = (WORD)(col + bkg + alg + fnt );
	BOOL blk = wbr > 1;

	if (wbr)	{ ofile.UnparseTag(T_SUBTOTAL, FALSE); ofile.UnparseTag(T_TITLE, blk); }
	if (blk)	ofile.UnparseBegin();
	if (col)    ofile.UnparseTextColor(m_SubTitle.m_rgbTextColor);
	if (bkg)    ofile.UnparseBkgColor(m_SubTitle.m_rgbBkgColor);
	if (alg)    ofile.UnparseAlign(m_SubTitle.m_nAlign);
	if (fnt)    ofile.UnparseFont(m_pDocument->m_pFontStyles, m_SubTitle.m_nFontIdx);
	if (blk)	ofile.UnparseEnd();
}

//------------------------------------------------------------------------------
CString Table::GetName (BOOL bStringName/*=FALSE*/) const
{
	CString strName; 
	if (bStringName && m_pDocument->m_pEngine && m_pDocument->m_pEngine->GetSymTable())
	{
		WoormTable* pST = m_pDocument->m_pEngine->GetSymTable();
		int idx = pST && pST->GetDisplayTables() ?
			pST->GetDisplayTables()->Find(GetInternalID(), m_pDocument->m_dsCurrentLayoutView)
			: - 1;
		if (idx > -1)
		{
			strName = pST->GetDisplayTables()->GetAt(idx)->GetTableName();
		}
	}
	else if (bStringName && m_pDocument->m_pEditorManager)
	{
		ASSERT_VALID(m_pDocument->m_pEditorManager);
		DisplayTables* pTbls = m_pDocument->m_pEditorManager->GetDispTable();
		if (pTbls)
		{
			ASSERT_VALID(pTbls);
			strName = pTbls->GetName(this->GetInternalID());
		}
	}
	if (strName.IsEmpty())
		strName= m_Title.GetText();
	if (strName.IsEmpty())
		strName.Format(_T("Table%d"), GetInternalID());
	return strName;
}

//------------------------------------------------------------------------------
BOOL Table::GetSchema (CXSDGenerator* pSchema, WoormTable* pRst)
{
	CString strTableName (GetName());	//TODO FATTURAZIONE ELETTRONICA: GetTableName(TRUE)

	pSchema->BeginComplexElement(strTableName, _T("1"), _T("1"));
		
		CString tagRow = _T("Row");		//TODO FATTURAZIONE ELETTRONICA: strTableName + _T("_Row")
		pSchema->BeginComplexElement(tagRow, _T("0"), SCHEMA_XSD_UNBOUNDED_VALUE);

			pSchema->InsertAttribute(XML_TYPE_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE);

			int ub = m_Columns.GetUpperBound();
			for (int nCol= 0; nCol <= ub; nCol++)
			{
				TableColumn* pCol = m_Columns[nCol];

				WoormField* pFieldCol = pRst->GetFieldByID(pCol->GetInternalID());
				ASSERT(pFieldCol);

				if (pFieldCol && !pFieldCol->GetSkipXml())
				{
					CString strType = pFieldCol->GetData()->GetXMLType(FALSE);
					pSchema->InsertElement(pFieldCol->GetXmlName(), strType, _T("1"), _T("1")); 
				}
			}
		pSchema->EndComplexElement();	//Row

	pSchema->EndComplexElement();	//Table

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL Table::Parse (ViewParser& lex)
{
	CPoint  ptOrigin;
	int     nTableTitleHeight;
	int     nColumnTitleHeight;
	int     nRowHeight;
	int     TotalHeight;
	CString strTmp;

	if ( ! (
			// ParseTable already parsed from m_pDocument for build right rows and m_Columns table
			lex.ParseString (strTmp)          &&
			lex.ParseAlias  (m_wInternalID)    &&
			lex.ParseOrigin (ptOrigin)        &&
			lex.ParseHeights
					(
						nTableTitleHeight,
						nColumnTitleHeight,
						nRowHeight,
						TotalHeight
					)
		   )
	  )
		return FALSE;

	if (lex.Matched(T_LAYER))
	{
		if (!lex.ParseInt(m_nLayer))
			return FALSE;
	}

	// must use setText because title is not generaltext but TableTitleText
	m_Title.SetText(strTmp);

	// update counter of last wID for create new obcject with unique ID
	m_pDocument->m_pEditorManager->SetLastId(m_wInternalID);

	// parse table title and borders                         
	if (!ParseTitleAndBorders (lex))
		return FALSE;

	// parse all m_Columns
	if (!ParseColumnsBlock(lex))
		return FALSE;

	// reconstruct table layout and Format contents
	RebuildTableSizes
	(
		ptOrigin,
		nTableTitleHeight,
		nColumnTitleHeight,
		nRowHeight,
		TotalHeight
	);

	Format ();

	return TRUE;
}           

//------------------------------------------------------------------------------
BOOL Table::ParseAllColumns (ViewParser& lex)
{
	int ub = m_Columns.GetUpperBound();
	for (int nCol= 0; nCol <= ub; nCol++)
	{
		if (!(m_Columns[nCol]->Parse(lex)))
			return FALSE;
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL Table::ParseColumnsBlock (ViewParser& lex)
{
	return 
		lex.ParseBegin		() 			&&
		ParseAllColumns		(lex)		&&
		ParseTableAttrib	(lex)		&&
		lex.ParseEnd		();
}

//------------------------------------------------------------------------------
BOOL Table::ParseTitleAndBorders (ViewParser& lex)
{
	// parse general table info not ever present
	for (;;) switch (lex.LookAhead())
	{
		case T_HIDE_TABLE_TITLE:
			lex.SkipToken(); 
			m_bHideTableTitle = TRUE;    
			break;

		case T_HIDE_ALL_TABLE_TITLE:
			lex.SkipToken(); 
			m_bHideTableTitle = m_bHideColumnsTitle = TRUE;    
			break;

		case T_FISCAL_END:
			lex.SkipToken(); 
			m_bFiscalEnd = TRUE;    
			break;

		case T_TRANSPARENT:
			lex.SkipToken(); 
			m_bTransparent = TRUE;    
			break;

		case T_TITLE :
			if (!ParseTableTitle (lex)) 
				return FALSE;
			break;
		case T_SUBTOTAL:	// T_TITLE 
			if (!ParseTableSubTitle(lex))
				return FALSE;
			break;

		case T_BORDERS :
		case T_NO_BORDERS :
			if (!m_Borders.Parse (lex)) 
				return FALSE;
			break;

		case T_EASYVIEW:
			lex.SkipToken(); 

			if (lex.Matched(T_DYNAMIC))
				m_bAlternateBkgColorOnMultiLineRow = TRUE;
			else
				m_bAlternateBkgColorOnRow = TRUE;

			if (lex.LookAhead(T_COLOR))
			{
				if (!lex.ParseColor(T_COLOR, m_crAlternateBkgColorOnRow))
					return FALSE;
			}
			break;

		case T_DROPSHADOW:
			lex.SkipToken(); 
			if (
					!lex.ParseInt(m_nDropShadowHeight) ||
					!lex.ParseColor(T_COLOR, m_crDropShadowColor)
				)
				return FALSE;
			break;
			
		case T_STYLE:
			lex.SkipToken(); 
			if (!lex.ParseString(m_sStyleClass))
				return FALSE;
			m_sStyleClass.Trim();
			break;

		case T_TEMPLATE:
			lex.SkipToken(); 
			m_bTemplate = TRUE;    
			break;

		case T_HIDDEN :
				{
					if (!ParseHidden(lex))
						return FALSE;	
					break;
				}
		
		default :
			return TRUE;
	}
}

//------------------------------------------------------------------------------
BOOL Table::ParseHidden(ViewParser& lex)
{
	if (!lex.Match(T_HIDDEN)) 
		return FALSE;	
	lex.Matched(T_WHEN);
	
	WoormTable* pST = &(m_pDocument->m_ViewSymbolTable);

	m_pHideExpr = new Expression(pST);
	m_pHideExpr->SetStopTokens(T_SEP);
	if (!m_pHideExpr->Parse(lex, DataType::Bool, TRUE))
		return FALSE;

	lex.Match(T_SEP);
	
	return TRUE;
}

//------------------------------------------------------------------------------
void Table::UnparseHidden (ViewUnparser& ofile)
{
	if (m_pHideExpr  && ! m_pDocument->m_Template.m_bIsSavingTemplate) 
	{
		ofile.UnparseTag (T_HIDDEN);
		ofile.UnparseBlank ();
		ofile.UnparseTag (T_WHEN);

		if (m_pDocument->m_bReplaceHiddenWhenExpr)
		{
			ofile.UnparseTag (T_FALSE);
		}
		else
		{
			ofile.UnparseExpr(m_pHideExpr->ToString(), FALSE);
		}
		ofile.UnparseTag (T_SEP);
	}
}

//------------------------------------------------------------------------------
BOOL Table::ParseTableTitleOption (ViewParser& lex, BOOL blk)
{
	BOOL ok = TRUE;

	do
	{
		switch (lex.LookAhead())
		{
			case T_TEXTCOLOR    : ok = lex.ParseTextColor   (m_Title.m_rgbTextColor);  break;
			case T_BKGCOLOR     : ok = lex.ParseBkgColor    (m_Title.m_rgbBkgColor);   break;

			case T_ALIGN        : ok = lex.ParseAlign       (m_Title.m_nAlign);      break;
			case T_FONTSTYLE    :
				ok = lex.ParseFont (m_pDocument->m_pFontStyles, m_Title.m_nFontIdx);
				break;

			case T_PEN          : ok = lex.ParsePen         (m_TitlePen);         break;

			case T_END :
				if (blk) return ok;
				lex.SetError(_TB("Unexpected END on the table title"));
				return FALSE;

			default :
				if (blk)
				{
					lex.SetError(_TB("There is no final END after more options on the table title"));
					ok = FALSE;
				}
		}
	}
	while (ok && blk);

	return ok;
}

//------------------------------------------------------------------------------
BOOL Table::ParseTableTitle (ViewParser& lex)
{
	if (!lex.Matched(T_TITLE))
		return TRUE;

	if (!lex.LookAhead(T_BEGIN))
		return ParseTableTitleOption(lex, FALSE);

	if (!lex.ParseBegin())
		return FALSE;

	BOOL ok =TRUE;
	do 
	{ 
		ok = ParseTableTitleOption(lex) && !lex.Bad() && !lex.Eof(); 
	} while (ok && !lex.LookAhead(T_END));
			
	return ok && lex.ParseEnd();
}

//------------------------------------------------------------------------------
BOOL Table::ParseTableSubTitle(ViewParser& lex)
{
	if (!lex.Matched(T_SUBTOTAL))
		return TRUE;

	if (!lex.Match(T_TITLE))
		return FALSE;

	if (!lex.LookAhead(T_BEGIN))
		return ParseTableSubTitleOption(lex, FALSE);

	if (!lex.ParseBegin())
		return FALSE;

	BOOL ok = TRUE;
	do
	{
		ok = ParseTableSubTitleOption(lex) && !lex.Bad() && !lex.Eof();
	} while (ok && !lex.LookAhead(T_END));

	return ok && lex.ParseEnd();
}

//------------------------------------------------------------------------------
BOOL Table::ParseTableSubTitleOption(ViewParser& lex, BOOL blk)
{
	BOOL ok = TRUE;

	do
	{
		switch (lex.LookAhead())
		{
		case T_TEXTCOLOR: ok = lex.ParseTextColor(m_SubTitle.m_rgbTextColor);  break;
		case T_BKGCOLOR: ok = lex.ParseBkgColor(m_SubTitle.m_rgbBkgColor);   break;

		case T_ALIGN: ok = lex.ParseAlign(m_SubTitle.m_nAlign);      break;
		case T_FONTSTYLE:
			ok = lex.ParseFont(m_pDocument->m_pFontStyles, m_SubTitle.m_nFontIdx);
			break;

		case T_END:
			if (blk) return ok;
			lex.SetError(_TB("Unexpected END on the table sub-title"));
			return FALSE;

		default:
			if (blk)
			{
				lex.SetError(_TB("There is no final END after more options on the table sub-title"));
				ok = FALSE;
			}
		}
	} while (ok && blk);

	return ok;
}
//-----------------------------------------------------------------------------

// on assume that all title column width is alread rigth setted
// and all rows have same height
//
//  must rebuild:
//
//      cell.CellRect;
//      total.cellrect;
//
//      column.ColumnTitleRect; // only column title
//      column.m_ColumnCellsRect; // only column title and column cells
//      column.m_ColumnRect;      // including column title, column cells, total
//
//      table.m_BaseRect
//      table.m_BaseCellsRect;    // include table title, columns titles adn all cells
//      table.m_TitleRect;
//
//------------------------------------------------------------------------------
void Table::RebuildTableSizes
	(
		CPoint  ptOrigin,
		int     nHTableTitle,
		int     nHColTitle,
		int     nHRow,
		int     nHTotal
	)
{
	// calculate table title width and make It
	int nRow = 0;
	for (int i = 0; i <= LastColumn(); i++)
		nRow += m_Columns[i]->Width();

	m_BaseRect    = CRect
					(
						ptOrigin,
						CSize(nRow, nHTableTitle + nHColTitle + nHRow * RowsNumber() + nHTotal)
					);
	m_BaseCellsRect=CRect
					(
						ptOrigin,
						CSize(nRow, nHTableTitle + nHColTitle + nHRow * RowsNumber())
					);
	m_TitleRect   = CRect(ptOrigin, CSize(nRow, nHTableTitle));

	// inital value;
	int x = m_TitleRect.left;
	int y = m_TitleRect.bottom;
	int h = nHColTitle;

	m_pDocument->m_PageInfo.m_arHPageSplitter.RemoveAll();
	
	for (int nCol = 0; nCol <= LastColumn(); nCol++)
	{
		TableColumn* pColumn = m_Columns[nCol];
		int w = pColumn->Width();

		pColumn->m_ColumnTitleRect = CRect (CPoint(x,y),CSize(w, h));
		pColumn->m_ColumnCellsRect = CRect (CPoint(x,y),CSize(w, h + RowsNumber() * nHRow));
		pColumn->m_ColumnRect      = CRect (CPoint(x,y),CSize(w, h + RowsNumber() * nHRow + nHTotal));

		pColumn->ResizeCells     (CPoint(x, y + h), CSize(w, nHRow));
		pColumn->ResizeTotalCell (CPoint(x, y + h + RowsNumber() * nHRow), CSize(w, nHTotal));
		x += w;

		if (pColumn->m_bSplitterColumn)
		{
			m_pDocument->m_PageInfo.m_arHPageSplitter.AddSorted(x);
		}
	}
}

// must set m_nActiveRow and m_nActiveColumn for next call like showPopup etc...
//------------------------------------------------------------------------------
BOOL Table::InMe (CPoint ptPoint)
{
	if (m_nLayer != m_pDocument->m_nCurrentLayer)
		return FALSE;

	// set also active cell
	m_nActiveRow    = ACTIVE_IS_TITLE;
	if (!m_bTransMode)
		m_nActiveColumn = NO_ACTIVE_COLUMN;

	// position outsize entire table limits
	if (!m_BaseRect.PtInRect(ptPoint))  
		return FALSE;
	if (m_TitleRect.PtInRect(ptPoint))  
		return m_pDocument->m_bAllowEditing ? !m_bHideTableTitle : TRUE;

	int ub = m_Columns.GetUpperBound();
	for (int nCol = 0; nCol <= ub; nCol++)
		if (ColumnRect(nCol).PtInRect(ptPoint))
		{
			if(!m_bTransMode)
				m_nActiveColumn = nCol;
			if (ColumnTitleRect(nCol).PtInRect(ptPoint)) return TRUE;

			// see if in total area
			if (TotalRect(nCol).PtInRect(ptPoint))
			{
				if (HasTotal(nCol))
				{
					m_nActiveRow = ACTIVE_IS_TOTAL;
					return TRUE;
				}
				else
					return FALSE;
			}

			// set attribute for cell or nRow 
			for (int nRow = 0; nRow <= LastRow(); nRow++)
				if (CellRect(nRow, nCol).PtInRect(ptPoint))
				{
					m_nActiveRow = nRow;
					return TRUE;
				}
		}
	return FALSE;
}

//------------------------------------------------------------------------------
int Table::GetColumnIdxByPoint(CPoint pt)
{
	int ub = m_Columns.GetUpperBound();
	for (int nCol = 0; nCol <= ub; nCol++)
	{
		TableColumn* pCol = m_Columns[nCol];
		if (pCol->IsHidden())
			continue;
		if (pCol->GetColumnRect().PtInRect(pt))
			return nCol;
	}
	return -1;
}

//------------------------------------------------------------------------------
BOOL Table::SetActiveColumn (int nID, CPoint& ptPoint)
{
	int nCol = GetIdxColFromAlias(nID);
	if (nCol < 0)
		return FALSE;

	m_nActiveRow    = ACTIVE_IS_TITLE;
	m_nActiveColumn = nCol;
	//TableColumn* pCol = (TableColumn*)(m_Columns.GetAt(nCol));
	ptPoint = CPoint(ColumnTitleRect(nCol).left + 1, ColumnTitleRect(nCol).top + 1) ;
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL Table::SetActiveColumn(TableColumn* pCol)
{
	ASSERT_VALID(pCol);

	for (int i = 0; i < m_Columns.GetUpperBound();i++)
		if (m_Columns.GetAt(i) == pCol)
		{
			CPoint point;
			SetActiveColumn(pCol->GetInternalID(), point);
			return TRUE;
		}

	return FALSE;
}

//------------------------------------------------------------------------------
TableColumn* Table::GetActiveColumn()
{
	if (m_nActiveColumn == -1)
		return NULL;
	return m_Columns[m_nActiveColumn];
}

//------------------------------------------------------------------------------
int Table::GetIdxColFromAlias(int nAliasCol) const
{
	for (int nCol = 0; nCol <= m_Columns.GetUpperBound(); nCol++)
	{
		if (((TableColumn*)m_Columns.GetAt(nCol))->GetInternalID() == nAliasCol)
		{
			return nCol;
		}
	}
	return -1;
}

//------------------------------------------------------------------------------
Table::SelPosition Table::GetPosition(CPoint ptPoint, int& nRow, int* pnColumnAlias/*=NULL*/) const
{
	nRow = -1;
	if (pnColumnAlias)
		*pnColumnAlias = 0;

	if (m_TitleRect.PtInRect(ptPoint))
		return POS_WHOLE_TABLE;

	for (int nCol = 0; nCol <= m_Columns.GetUpperBound(); nCol++)
	{
		if (ColumnTitleRect(nCol).PtInRect(ptPoint))
		{
			if (pnColumnAlias)
				*pnColumnAlias = ((TableColumn*)m_Columns.GetAt(nCol))->GetInternalID();

			return POS_COLUMN;
		}
		// set attribute for cell or nRow 
		for (int r = 0; r <= LastRow(); r++)
			if (CellRect(r, nCol).PtInRect(ptPoint))
			{
				nRow = r;

				TableCell* pCell = m_Columns[nCol]->m_Cells[r];
				
				CString str = pCell->GetText();
				str.Trim();
				if (str.IsEmpty()) 
					break;

				if (pnColumnAlias)
					*pnColumnAlias = ((TableColumn*)m_Columns.GetAt(nCol))->GetInternalID();

				return POS_ROW;
			}
	}
	
	// qualsiasi altro punto che non sia colonna
	return POS_WHOLE_TABLE;
}

//------------------------------------------------------------------------------
void Table::ShowPopup (CPoint ptPoint, CBCGPScrollView* pView)
{
			if (m_nActiveColumn    == NO_ACTIVE_COLUMN)	ShowTablePopup (ptPoint, pView);
	else    if (m_nActiveRow       == ACTIVE_IS_TITLE)	ShowColumnPopup (ptPoint, pView);
	else    if (m_nActiveRow       == ACTIVE_IS_TOTAL)	ShowTotalPopup (ptPoint, pView);
	else												ShowCellPopup  (ptPoint, pView);
}

//------------------------------------------------------------------------------
BOOL Table::AssignData (WORD wID, RDEManager* pRDEmanager)
{
	if (m_wInternalID == wID)
	{
		m_pDocument->Message(_TB("Table identifier does not correspond to data item sent")+_TB("\nHas a column internal ID equals to table's ID ?"));
		return TRUE;
	}

	// try all m_Columns to find right wID column
	int ub = m_Columns.GetUpperBound();
	for (int nCol = 0; nCol <= ub; nCol++)
	{
		TableColumn* pColumn = m_Columns[nCol];
		if (pColumn->GetInternalID() == wID)
		{
			pColumn->AssignData(wID, pRDEmanager);
			return TRUE;
		}
	}

	// no ID found in this table
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL Table::ExecCommand (WORD wID, RDEManager* pRDEmanager)
{
	BOOL bSkipData = TRUE;
	// execute command over all table
	if (m_wInternalID == wID)
	{
		switch (pRDEmanager->GetCommand())
		{
			case RDEManager::NEXT_LINE:
				m_nCurrentRow++;
				break;

			case RDEManager::TITLE_LINE :
				m_LineWithTitles[m_nCurrentRow] = TRUE;
				m_nCurrentRow++;
				break;

			case RDEManager::CUSTOM_TITLE_LINE:
			{
				bSkipData = FALSE;
				RDEData RDEdata;
				if (!pRDEmanager->GetDataValue(RDEdata))
					return FALSE;

				m_LineWithCustomTitles[m_nCurrentRow] = (LPCTSTR)RDEdata.GetData();
				m_nCurrentRow++;
				break;
			}

			case RDEManager::INTER_LINE :
				if (m_nCurrentRow)
					m_Interlines[m_nCurrentRow - 1] = TRUE;
				break;

			default: break;
		}

		// ignore data
		if (bSkipData) 
			pRDEmanager->SkipData();
		return TRUE;
	}

	// command non for table but must investigate over columns
	// try all columns to find right wID column for execute command
	int ub = m_Columns.GetUpperBound();
	for (int nCol = 0; nCol <= ub; nCol++)
	{
		if (m_Columns[nCol]->ExecCommand(wID, pRDEmanager))
			return TRUE;
	}

	// no ID found in this table
	return FALSE;
}

//------------------------------------------------------------------------------
void Table::ResetCounters()
{
	m_nCurrentRow = 0;

	// clear interline flags
	int ub = m_Interlines.GetUpperBound();
	for (int nRow = 0; nRow <= ub; nRow++)
	{
		ASSERT(ub == m_LineWithTitles.GetUpperBound());
		ASSERT(ub == m_LineWithCustomTitles.GetUpperBound());

		m_Interlines[nRow] = FALSE;
		m_LineWithTitles[nRow] = FALSE;
		m_LineWithCustomTitles[nRow] = L"";
	}

	if (m_bAlternateBkgColorOnMultiLineRow)
	{
		// reset tail multistring flag
		ub = m_Columns.GetUpperBound();
		for (int nCol = 0; nCol <= ub; nCol++)
		{
			m_Columns[nCol]->ResetCells();
		}
	}
}

//------------------------------------------------------------------------------
void Table::DisableData()
{
	int ub = m_Columns.GetUpperBound();
	for (int nCol = 0; nCol <= ub; nCol++)
	{
		m_Columns[nCol]->DisableData();
	}
}

// if table is selected delete all columns and table
// otherwhise delete only active column and leave table as is
//
//------------------------------------------------------------------------------
BOOL Table::DeleteEditorEntry ()
{
	if (NoActiveColumn() || m_Columns.GetSize() == 1)
	{
		BOOL ok = TRUE;
		BOOL bMustUpdate = FALSE;

		// delete table entry
		if (m_wInternalID && ok)
		{
			if (!m_pDocument->m_Layouts.ExistsFieldID(m_wInternalID, TRUE))
			{
				if (!m_pDocument->m_pEditorManager->DeleteTable(m_wInternalID))
				{
					// table wID not found must signal error but must delete table
					m_pDocument->Message(_TB("Id table not found"));
				}
			}
		}
		
		if (!ok && bMustUpdate)
		{
			m_pDocument->m_pActiveRect->SetActive (GetActiveRect());
			m_pDocument->UpdateWindow();
		}

		//impedisce l'UNDO
		for (int nCol = m_Columns.GetUpperBound(); nCol >= 0; nCol--)
		{
			DeleteColumnComplete(nCol, nCol == 0);
		}
/*
		//TODO problema per campi TOTALI E SUBTOTALI, se rimangono il report viene salvato corrotto (A. 24341)
		//VEDI void CWoormDocMng::ObjectUndo(BaseObj * pObject)

		for (int nCol = 0; nCol <= m_Columns.GetUpperBound(); nCol++)
		{
			TableColumn* pCol = m_Columns[nCol];

			WoormField* pF = m_pDocument->GetEditorSymTable()->GetFieldByID(pCol->GetInternalID());
			pF->SetFieldType(WoormField::RepFieldType::FIELD_NORMAL);
			pF->SetHidden(TRUE);

			m_pDocument->SyncronizeViewSymbolTable(pF);
		}
*/
		//--------
		return ok;
	}
	else
	{
		DeleteColumnComplete(m_nActiveColumn);
		return FALSE;
	}
}

// where position mouse when creating table
//------------------------------------------------------------------------------
CPoint Table::StartTrackPoint()
{
	CRect   rect (CellActionRect(CELL_HSIZE, 0, 0));

	return CPoint (rect.left + 2, rect.bottom - 2);
}

//------------------------------------------------------------------------------
void Table::MyClipCursor(CDC& dc, CBCGPScrollView* pView, CRect rect)
{
	CRect   client;
	CRect	columnTitlesRect;

	CPoint scrollPos = m_pDocument->GetWoormView()->GetScrollPosition();
	scrollPos.x = -scrollPos.x;
	scrollPos.y = -scrollPos.y;

	rect.OffsetRect(scrollPos);

	pView->GetClientRect(client);
	
	IntersectRect(&columnTitlesRect, rect, client);

	pView->ClientToScreen(columnTitlesRect);
	ClipCursor(columnTitlesRect);
}

//------------------------------------------------------------------------------
void Table::ClipActionCursor(CDC& dc, CPoint ptMousePos, CBCGPScrollView* pView)
{                   
	CRect   client;
	CRect   limit;
	CRect	minr(0, 0, MINCELL_X, MINCELL_Y);

	pView->GetClientRect(client);
	UnScaleRect(client, dc);
	int right   = client.Width();
	int bottom  = client.Height();

	dc.LPtoDP(&minr);
	int minx	= minr.Width();
	int miny	= minr.Height();

	switch (m_nAction)
	{
		case ADD_ROW :
		{
			CRect rect (m_BaseRect);
			CRect cell (CellRect(0, 0));

			dc.LPtoDP(&rect); 
			dc.LPtoDP(&cell);

			rect.IntersectRect(&rect, &client);
			cell.IntersectRect(&cell, &client);
			
			limit = CRect(rect.left, cell.bottom, rect.right, bottom);
			break;
		}

		case CELL_HSIZE:
		{
			// assume is in active cell (othervise normalize it to last cell)
			CRect cell (CellRect(NormalizedRow(), NormalizedColumn()));
			dc.LPtoDP(&cell);
			cell.IntersectRect(&cell, &client);

			limit = CRect (cell.left + minx,	cell.top, right, cell.bottom);
			break;
		}

		case CELL_VSIZE :
		{
			// assume is in active cell (othervise normalize it to last cell)
			CRect cell (CellRect(NormalizedRow(), NormalizedColumn()));
			dc.LPtoDP(&cell);
			cell.IntersectRect(&cell, &client);

			limit = CRect (cell.left, cell.top + miny, cell.right, bottom);
			break;
		}

		case CELL_SIZE:
		{
			// assume is in active cell (othervise normalize it to last cell)
			CRect cell (CellRect(NormalizedRow(), NormalizedColumn()));
			dc.LPtoDP(&cell);
			cell.IntersectRect(&cell, &client);

			limit = CRect (cell.left + minx,	cell.top + miny, right, bottom);
			break;
		}

		case T_VSIZE :
		{
			CRect rect (m_TitleRect);
			dc.LPtoDP(&rect);
			rect.IntersectRect(&rect, &client);

			limit = CRect (	rect.left, 0, rect.right, rect.bottom - miny);
			break;
		}

		case CT_VSIZE :
		{
			// assume is in active column (othervise normalize it to last column)
			CRect rect (m_TitleRect);
			CRect column (ColumnTitleRect(NormalizedColumn()));

			dc.LPtoDP(&rect); 
			dc.LPtoDP(&column);

			rect.IntersectRect(&rect, &client);
			column.IntersectRect(&column, &client);

			limit = CRect (column.left, rect.Height(), column.right, column.bottom - miny);
			break;
		}

		case TOT_VSIZE :
		{
			// assume is in active column (othervise normalize it to last column)
			CRect total (TotalRect(NormalizedColumn()));
			dc.LPtoDP(&total);
			total.IntersectRect(&total, &client);

			limit = CRect (total.left, total.top + miny, total.right, bottom);
			break;
		}

		default :
		{
			// MOVE table
			CRect   rect (m_BaseRect);
			CPoint  ptPoint = ptMousePos;

			dc.LPtoDP(&rect); 
			dc.LPtoDP(&ptPoint);
			rect.IntersectRect(&rect, &client);
			
			limit = CRect (ptPoint.x - rect.left, ptPoint.y - rect.top, right, bottom);
			break;
		}
	}
	
	ScaleRect(limit, dc);
	pView->ClientToScreen (limit);
	ClipCursor(limit);
}

//------------------------------------------------------------------------------
BOOL Table::InMoveArea (CPoint ptMousePos)
{
	return TableAction (ptMousePos) == MOVE;
}

// table object update active rect (nRow or column can be changed)
// also if position are not changed
//
//------------------------------------------------------------------------------
void Table::LeftButtonUp (CDC& aDC, CBCGPScrollView* pView)
{
	BaseObj::LeftButtonUp (aDC, pView);

	if(m_nDropShadowHeight > 0)
		m_pDocument->Invalidate(TRUE);
}

//------------------------------------------------------------------------------
void Table::SetActionCursor(CPoint ptMousePos)
{
	// save current active position
	int nRow = m_nActiveRow;
	int nCol = m_nActiveColumn;

	ActionType tmp_action = TableAction (ptMousePos);
	
	// restore active position because TableAction change it
	m_nActiveRow       = nRow;
	m_nActiveColumn    = nCol;

	if  (
		(tmp_action == LEAVE)   ||
		(tmp_action == MOVE)    ||
		(tmp_action == COL_MOVW) ||
		(tmp_action == COL_MOVE)
		)
	{
		SetObjCursor (LEAVE);
		return;
	}

	SetObjCursor (tmp_action);
}

//------------------------------------------------------------------------------
void Table::MoveObject(CSize offset)
{
	CRect   rectPrev = m_BaseRect;
	CRect	rectNew = m_BaseRect;
			   
	// accept only positive rectangle	         
	rectNew.OffsetRect(offset);
	if (rectNew.top < 0 || rectNew.left < 0)
		return;

	MoveTable (offset);
	if (rectPrev != m_BaseRect)
		m_pDocument->InvalidateRect(rectPrev, TRUE);
	m_pDocument->InvalidateRect(m_BaseRect, m_bTransparent);
	m_pDocument->UpdateWindow();
}


//------------------------------------------------------------------------------
void Table::SizeObject(CSize nSize)
{
	RightSizeObject (nSize.cx);
	BottomSizeObject (nSize.cy);
}

//------------------------------------------------------------------------------
void Table::RightSizeObject(int /*width*/)
{
	// do nothing, only used for resizing multiple object
}

//------------------------------------------------------------------------------
void Table::BottomSizeObject(int /*height*/)
{
	// do nothing, only used for resizing multiple object
}

//------------------------------------------------------------------------------
void Table::LeftSizeObject(int /*width*/)
{
	// do nothing, only used for resizing multiple object
}

//------------------------------------------------------------------------------
void Table::TopSizeObject(int /*height*/)
{
	// do nothing, only used for resizing multiple object
}

//------------------------------------------------------------------------------
void Table::LeftObject(int /*x*/)
{
	// do nothing, only used for align multiple object
}

//------------------------------------------------------------------------------
void Table::TopObject(int /*y*/)
{
	// do nothing, only used for align multiple object
}

// toggla i bordi della intera tabella prendendo un qualsiasi bordo a caso
//------------------------------------------------------------------------------
void Table::ToggleBorder(UINT nID)
{
	if (nID == ID_TOGGLE_BORDER_ALL)
	{
		m_Borders.Init(!m_Borders.m_bTableTitleTop); // ne usa una a caso

		m_Borders.m_bRowSeparator = FALSE;
		m_Borders.m_bRowSeparatorDynamic = FALSE;

		m_pDocument->InvalidateRect(m_BaseRect, m_bTransparent);
		m_pDocument->SetModifiedFlag();
	}
}

// Helper macros per rendere leggibile la routine di SnapToGrid
//------------------------------------------------------------------------------
#define GRIDX		m_pDocument->m_pWoormIni->m_nGridX
#define GRIDY		m_pDocument->m_pWoormIni->m_nGridY

//10 12 2 6  2
//10 18 8 6  -2
//10  7 7 6  -3
//10  2 2 6  2
#define CALC_X(a,b)	int a = b.left % GRIDX;\
					if (a > (GRIDX / 2)) a = a - GRIDX;\
//					TRACE2(#a"= %d "#b"=%d\n",a,b.left);

#define CALC_Y(a,b)	int a = b.top % GRIDY;\
					if (a > (GRIDY / 2)) a = a - GRIDY;\
//					TRACE2(#a"= %d "#b"=%d\n",a,b.top);

#define CALC_W(a,b)	int a = b.Width() % GRIDX;\
					if (a > (GRIDX / 2)) a = a - GRIDX; else\
					if ((b.Width() - a) < GRIDX) a = a - GRIDX;\
//					TRACE2(#a"= %d "#b"=%d\n",a,b.Width());

#define CALC_H(a,b)	int a = b.Height() % GRIDY;\
					if (a > (GRIDY / 2)) a = a - GRIDY; else\
					if ((b.Height() - a) < GRIDY) a = a - GRIDY;\
//					TRACE2(#a"= %d "#b"=%d\n",a,b.Height());

//------------------------------------------------------------------------------
void Table::SnapToGrid()
{
	// salva il rettangolo per la successiva invalidazione
	CRect   rectPrev = m_BaseRect;

	CALC_H(yTitle,		m_TitleRect);
	CALC_H(yColTitle,	ColumnTitleRect(0));
	CALC_H(yCell,		CellRect(0,0));
	CALC_H(yTotal,		TotalRect(0));

	VExpandTotal		(	-yTotal);
	VExpandRow			(0,	-yCell);
	VExpandColumnTitle	(	yColTitle);
	VExpandTitle		(	yTitle);

	for (int nCol = 0; nCol <= LastColumn(); nCol++)
	{
		CALC_W(width, ColumnRect(nCol));
		HExpandColumn(nCol, -width, TRUE);
	}

	CALC_X(x, m_BaseRect);
	CALC_Y(y, m_BaseRect);

	MoveTable (CSize(-x, -y));

	// invalida per le nuove dimensioni se sono cambiate
	if (rectPrev != m_BaseRect)
		m_pDocument->InvalidateRect(rectPrev, TRUE);

	m_pDocument->InvalidateRect(m_BaseRect, m_bTransparent);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
BOOL Table::CheckTypeConsistence(CString& sError)
{
	// check type consistence for all columns
	int ub = m_Columns.GetUpperBound();
	for (int nCol = 0; nCol <= ub; nCol++)
		if (!m_Columns[nCol]->CheckTypeConsistence(sError))
			return FALSE;

	return TRUE;
}

//------------------------------------------------------------------------------
WORD Table::GetRDESearchID () const
{
	 if (NoActiveColumn()) return 0;
	 return m_Columns[m_nActiveColumn]->GetInternalID();
}

//------------------------------------------------------------------------------
BOOL Table::CanSearched () const
{
	 if (NoActiveColumn()) return FALSE;
	 return m_Columns[m_nActiveColumn]->CanSearched();      
}


//------------------------------------------------------------------------------
BOOL Table::NoBorders (BOOL bPrinting)
{
	return m_pDocument->m_pOptions->NoBorders(m_pDocument, bPrinting);
}

//------------------------------------------------------------------------------
BOOL Table::ShowTitles (BOOL bPrinting)
{
	// forza la non visualizzazione dei titolo il modo incondizionato
	// rispetto a i parametri di report m_bHideColumnsTitle implica m_bHideTableTitle (altrimenti rimane una striscia bianca vuota)
	if (m_bHideTableTitle || m_bHideColumnsTitle)
		return FALSE;

	return m_pDocument->m_pOptions->ShowTitles(m_pDocument, bPrinting);
}

//------------------------------------------------------------------------------
void Table::CopyTable()
{	
	//TODO
}

//------------------------------------------------------------------------------
void Table::CopyActiveColumn()
{
	//TODO
	//TableColumn* pColumn = m_Columns[m_nActiveColumn]; 
}

//------------------------------------------------------------------------------
void Table::CopyClipboard ()
{ 
	if (NoActiveColumn())
		CopyTable ();
	else 
		CopyActiveColumn ();
}

//	converts an unformatted number to string (Carlotta !!!!!!)
//------------------------------------------------------------------------------
CString Table::NumToString(TableCell& cell, const DataType& dataType)
{   
	if (!(cell.IsEnabledRDEData())) 
		return _T("");

	const rsize_t nLen = 64;
	TCHAR str[nLen];  
	void* num = cell.GetRDEData();
	
	switch (dataType.m_wType)
	{
		case DATA_INT_TYPE:
		{
			int numInt = *((short*)num);
			_itot_s(numInt, str, nLen, 10);
			return str;
		}
		case DATA_DBL_TYPE:
		case DATA_MON_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE:
		{
			USES_CONVERSION;
			char szBuffer [128];
			int	sign;
			int dec = 10;
			double	numDbl = *((double*)num); 
			sign = (numDbl > 0) ? 0 : 1;
			_fcvt_s(szBuffer, 128, numDbl, 0, &dec, &sign);
			return A2T(szBuffer);
		}
		case DATA_LNG_TYPE:
		{   
			long numLng = *((long*)num);
			_ltot_s(numLng, str, nLen, 10);
			return str;
		} 
		default: 
			return _T("");
	}	
} 
													 
//------------------------------------------------------------------------------
void Table::VKReturn()
{
	if (NoActiveColumn())
	{
		OnTableTitle ();
		return;
	}
	
	// column manage virtual key
	OnColumnTitle ();
}



//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//                                                                          //
//                 I N L I N E    S T A R T    D E F I N I T I O N          //
//                                                                          //
//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

int	Table::LastColumn	() const	{ return m_Columns.GetUpperBound(); }
int	Table::LastRow		() const	{ return m_Columns[0]->LastRow(); }
int	Table::RowsNumber	() const	{ return LastRow() + 1; }
int	Table::RowHeight	() const	{ return CellRect(0,0).Height(); }

CRect	Table::CellRect			(int nRow, int nCol) const	{ return m_Columns[nCol]->CellRect(nRow); }
CRect	Table::ColumnTitleRect	(int nCol) const			{ return m_Columns[nCol]->GetColumnTitleRect(); }
CRect	Table::ColumnCellsRect	(int nCol) const			{ return m_Columns[nCol]->GetColumnCellsRect(); }
CRect	Table::ColumnRect		(int nCol) const			{ return m_Columns[nCol]->GetColumnRect(); }
CRect	Table::TotalRect		(int nCol) const			{ return m_Columns[nCol]->TotalRect(); }

BOOL	Table::ColumnIsMultiRow		() const { return m_Columns[m_nActiveColumn]->IsMultiRow(); }
BOOL	Table::ColumnIsBitmap		() const { return m_Columns[m_nActiveColumn]->IsBitmap(); }
BOOL	Table::ColumnIsBarCode		() const { return m_Columns[m_nActiveColumn]->IsBarCode(); }
BOOL	Table::ColumnIsTextFile		() const { return m_Columns[m_nActiveColumn]->IsTextFile(); }
BOOL	Table::ColumnCanBeMultiRow	() const { return m_Columns[m_nActiveColumn]->CanBeMultiRow(); }
BOOL	Table::ColumnCanBeBitmap	() const { return m_Columns[m_nActiveColumn]->CanBeBitmap(); }
BOOL	Table::ColumnCanBeBarCode	() const { return m_Columns[m_nActiveColumn]->CanBeBarCode(); }
BOOL	Table::ColumnCanBeTextFile	() const { return m_Columns[m_nActiveColumn]->CanBeTextFile(); }
WORD	Table::GetActiveColumnId	() const { return m_Columns[m_nActiveColumn]->GetInternalID(); }
	
BOOL	Table::ColumnIsAnchorLeft		() const { return m_Columns[m_nActiveColumn]->IsAnchorLeft(); }
BOOL	Table::ColumnIsAnchorRight		() const { return m_Columns[m_nActiveColumn]->IsAnchorRight(); }
BOOL	Table::ColumnIsSplitter			() const { return m_Columns[m_nActiveColumn]->IsSplitter(); }
BOOL	Table::ColumnIsPinned			() const { return m_Columns[m_nActiveColumn]->IsPinned(); }
BOOL	Table::ColumnIsHiddenWhenEmpy	() const { return m_Columns[m_nActiveColumn]->IsHiddenWhenEmpy(); }
BOOL	Table::ColumnIsWidthOptimized	() const { return m_Columns[m_nActiveColumn]->IsWidthOptimized(); }

BOOL Table::CanVExpandRow			(int nRow, int height) const	{ return m_Columns[0]->CanVExpandRow (nRow, height); }
BOOL Table::CanHExpandColumn		(int nCol, int width) const		{ return m_Columns[nCol]->CanHExpandColumn (width); }
BOOL Table::CanVExpandColumnTitle	(int height) const				{ return m_Columns[0]->CanVExpandTitle (height); }
BOOL Table::CanVExpandTotal			(int height) const				{ return m_Columns[0]->CanVExpandTotal(height); }
BOOL Table::CanVExpandTitle			(int height) const				{ return ((m_TitleRect.Height() - height) > MINCELL_Y); }

void Table::SetObjCursor		()			{ SetObjCursor (m_nAction); }
BOOL Table::MovingObject		() const	{ return m_nAction == MOVE; }
BOOL Table::NoActiveColumn		() const	{ return (m_nActiveColumn  == NO_ACTIVE_COLUMN); }
BOOL Table::MoreThanOneColumn	() const	{ return m_Columns.GetSize() > 1; }

int Table::TitlesHeight		() const	{ return m_TitleRect.Height() + ColumnTitleRect(0).Height(); }
int	Table::NormalizedRow	() const	{ return (m_nActiveRow < 0) ? LastRow() : m_nActiveRow; }
int	Table::NormalizedColumn	() const	{ return (m_nActiveColumn == NO_ACTIVE_COLUMN) ? LastColumn() : m_nActiveColumn; }

//---------------------------------------------------------------------------
BOOL Table::HasTotal(int nCol) const 
{ return m_Columns[nCol]->HasTotal(); }

BOOL Table::HasTotal() const
{
	for (int i = 0; i <= m_Columns.GetUpperBound(); i++)
	{
		if (HasTotal(i))
		{
			return TRUE;
		}
	}
	return FALSE;
}

//---------------------------------------------------------------------------
BOOL Table::ActiveColumnCanHaveTotal	() const
{
	switch (m_pDocument->m_pEditorManager->GetDataType(GetActiveColumnId()).m_wType)
	{
		case DATA_STR_TYPE:
		case DATA_INT_TYPE:
		case DATA_LNG_TYPE:
		case DATA_DBL_TYPE:
		case DATA_MON_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE:
			return TRUE;
	}
	
	return FALSE;
}

//---------------------------------------------------------------------------
BOOL Table::Action (CPoint mouse_pos)
{
	m_nAction = TableAction(mouse_pos);

	return (m_nAction != LEAVE);
}

//---------------------------------------------------------------------------
CRect Table::NormalizedBaseRect(BOOL bCheckTitle/* = FALSE*/) const
{
	CRect rect (m_nTotalCounter ? m_BaseRect : m_BaseCellsRect);
	if (bCheckTitle && m_bHideTableTitle)
		rect.top += this->m_TitleRect.Height();

	return rect;
}

//---------------------------------------------------------------------------
CRect Table::NormalizedColumnRect(int nCol) const
{
	return HasTotal(nCol) ? ColumnRect(nCol) : ColumnCellsRect(nCol);
}

//------------------------------------------------------------------------------
void Table::SetColumnPen (const BorderPen& aPen, int nCol)
{
	m_Columns[nCol]->SetColumnPen (aPen);
}

//------------------------------------------------------------------------------
void Table::SetTotalPen (const BorderPen& aPen, int nCol)
{
	m_Columns[nCol]->SetTotalPen (aPen);
}

//------------------------------------------------------------------------------
void Table::SetColumnTitlePen (const BorderPen& aPen, int nCol)
{
	m_Columns[nCol]->SetColumnTitlePen (aPen);
}


//------------------------------------------------------------------------------
void Table::SetCellColor (COLORREF* pColors, int nRow, int nCol)
{
	m_Columns[nCol]->SetCellColor(pColors, nRow);
}

//------------------------------------------------------------------------------
void Table::SetColumnCellsColor(COLORREF* pColors, int nCol)
{
	m_Columns[nCol]->SetColumnCellsColor(pColors);
}

//------------------------------------------------------------------------------
void Table::SetColumnTitleColor (COLORREF* pColors, int nCol)
{
	m_Columns[nCol]->SetColumnTitleColor (pColors);
}

//------------------------------------------------------------------------------
void Table::SetColumnTitleTextColor(COLORREF color, int nCol)
{
	m_Columns[nCol]->SetColumnTitleTextColor(color);
}

//------------------------------------------------------------------------------
void Table::SetColumnTitleBkgColor(COLORREF color, int nCol)
{
	m_Columns[nCol]->SetColumnTitleBkgColor(color);
}

//------------------------------------------------------------------------------
void Table::SetColumnTitleBorderColor(COLORREF color, int nCol)
{
	m_Columns[nCol]->SetColumnTitleBorderColor(color);
}

//------------------------------------------------------------------------------
void Table::SetColumnTitleBorderWidth(int width, int nCol)
{
	m_Columns[nCol]->SetColumnTitleBorderWidth(width);
}

//------------------------------------------------------------------------------
void Table::SetSubTotalColor (COLORREF* pColors, int nCol)
{
	m_Columns[nCol]->SetSubTotalColor (pColors);
}

//------------------------------------------------------------------------------
void Table::SetTotalColor (COLORREF* pColors, int nCol)
{
	m_Columns[nCol]->SetTotalColor (pColors);
}

//------------------------------------------------------------------------------
void Table::SetTotalTextColor(COLORREF color, int nCol)
{
	m_Columns[nCol]->SetTotalTextColor(color);
}

//------------------------------------------------------------------------------
void Table::SetTotalBkgColor(COLORREF color, int nCol)
{
	m_Columns[nCol]->SetTotalBkgColor(color);
}

//------------------------------------------------------------------------------
void Table::SetTotalBorderColor(COLORREF color, int nCol)
{
	m_Columns[nCol]->SetTotalBorderColor(color);
}

//------------------------------------------------------------------------------
void Table::SetTotalBorderWidth(int width, int nCol)
{
	m_Columns[nCol]->SetTotalBorderWidth(width);
}

//------------------------------------------------------------------------------
void Table::SetCellAlign (AlignType wAlign, int nRow, int nCol)
{
	// esiste almeno una cella diversa dalle altre, quindi
	// l'attributo di colonna perde di significato (vedere
	// la sua gestione in OnColumnAlign())
	//
	//m_Columns[nCol]->m_bMixedColumnAlign = true;

	m_Columns[nCol]->SetCellAlign(wAlign, nRow);
}

//------------------------------------------------------------------------------
void Table::SetColumnAlign (AlignType wAlign, int nCol)
{
	m_Columns[nCol]->SetColumnAlign (wAlign);
}

//------------------------------------------------------------------------------
void Table::SetTotalAlign (AlignType wAlign, int nCol)
{
	m_Columns[nCol]->SetTotalAlign (wAlign);
}

//------------------------------------------------------------------------------
void Table::SetColumnTitleAlign (AlignType wAlign, int nCol)
{
	m_Columns[nCol]->SetColumnTitleAlign (wAlign);
}

//---------------------------------------------------------------------------
void Table::SetTableTitleAlign (AlignType wAlign)
{
	m_Title.SetAlign(wAlign);
}


//---------------------------------------------------------------------------
AlignType Table::GetTableTitleAlign () const
{
	return m_Title.GetAlign();
}

//------------------------------------------------------------------------------
void Table::SetCellFontIdx (FontIdx nIdx, int nRow, int nCol)
{
	// esiste almeno una cella diversa dalle altre, quindi
	// l'attributo di colonna perde di significato (vedere
	// la sua gestione in Table::OnColumnFontStyle())
	//
	//m_Columns[nCol]->m_bMixedColumnFont = true;

	m_Columns[nCol]->SetCellFontIdx(nIdx, nRow);
}

//------------------------------------------------------------------------------
void Table::SetColumnFontIdx (FontIdx nIdx, int nCol)
{
	m_Columns[nCol]->SetColumnFontIdx (nIdx);
}

//------------------------------------------------------------------------------
void Table::SetTotalFontIdx (FontIdx nIdx, int nCol)
{
	m_Columns[nCol]->SetTotalFontIdx (nIdx);
}

//------------------------------------------------------------------------------
void Table::SetColumnTitleFontIdx (FontIdx nIdx, int nCol)
{
	m_Columns[nCol]->SetColumnTitleFontIdx (nIdx);
}

//------------------------------------------------------------------------------
void Table::SetSubTotalFontIdx (FontIdx nIdx, int nCol)
{
	m_Columns[nCol]->SetSubTotalFontIdx (nIdx);
}


//---------------------------------------------------------------------------
void Table::SetTableTitleFontIdx (FontIdx nIdx)
{
	m_Title.SetFontIdx(nIdx);
}

//------------------------------------------------------------------------------
void Table::SetColumnFormatIdx (FormatIdx nIdx, int nCol)
{
	m_Columns[nCol]->SetColumnFormatIdx (nIdx);
}


//---------------------------------------------------------------------------
FontIdx Table::GetTableTitleFontIdx () const
{
	return m_Title.GetFontIdx();
}

//---------------------------------------------------------------------------
BorderPen Table::GetColumnPen (int nCol) const
{
	return m_Columns[nCol]->GetColumnPen();
}

//---------------------------------------------------------------------------
BorderPen Table::GetColumnTitlePen (int nCol) const
{
	return m_Columns[nCol]->GetColumnTitlePen();
}

//---------------------------------------------------------------------------
BorderPen Table::GetTotalPen (int nCol) const
{
	return m_Columns[nCol]->GetTotalPen();
}

//---------------------------------------------------------------------------
void Table::GetColumnTitleColor (COLORREF* pColors, int nCol) const
{
	m_Columns[nCol]->GetColumnTitleColor(pColors);
}

//---------------------------------------------------------------------------
void Table::GetSubTotalColor (COLORREF* pColors, int nCol) const
{
	m_Columns[nCol]->GetSubTotalColor(pColors);
}

//---------------------------------------------------------------------------
void Table::GetTotalColor (COLORREF* pColors, int nCol) const
{
	m_Columns[nCol]->GetTotalColor(pColors);
}

//---------------------------------------------------------------------------
void Table::GetCellColor (COLORREF* pColors, int nRow, int nCol) const
{
	m_Columns[nCol]->GetCellColor(pColors, nRow);
}


//---------------------------------------------------------------------------
AlignType Table::GetColumnTitleAlign (int nCol) const
{
	return m_Columns[nCol]->GetColumnTitleAlign();
}

//---------------------------------------------------------------------------
AlignType Table::GetColumnAlign (int nCol) const
{
	return m_Columns[nCol]->GetColumnAlign();
}

//---------------------------------------------------------------------------
AlignType Table::GetTotalAlign (int nCol) const
{
	return m_Columns[nCol]->GetTotalAlign();
}

//---------------------------------------------------------------------------
AlignType Table::GetCellAlign (int nRow, int nCol) const
{
	return m_Columns[nCol]->GetCellAlign(nRow);
}


//---------------------------------------------------------------------------
FontIdx Table::GetColumnTitleFontIdx (int nCol) const
{
	return m_Columns[nCol]->GetColumnTitleFontIdx();
}

//---------------------------------------------------------------------------
FontIdx Table::GetSubTotalFontIdx (int nCol) const
{
	return m_Columns[nCol]->GetSubTotalFontIdx();
}

//---------------------------------------------------------------------------
FontIdx Table::GetColumnFontIdx (int nCol) const
{
	return m_Columns[nCol]->GetColumnFontIdx();
}

//---------------------------------------------------------------------------
FontIdx Table::GetTotalFontIdx (int nCol) const
{
	return m_Columns[nCol]->GetTotalFontIdx();
}

//---------------------------------------------------------------------------
FontIdx Table::GetCellFontIdx (int nRow, int nCol) const
{
	return m_Columns[nCol]->GetCellFontIdx(nRow);
}

//---------------------------------------------------------------------------
FormatIdx Table::GetColumnFormatIdx (int nCol) const
{
	return m_Columns[nCol]->GetColumnFormatIdx();
}

//---------------------------------------------------------------------------
CString Table::GetColumnTitleText (int nCol) const
{
	return m_Columns[nCol]->m_Title.GetText();
}

//---------------------------------------------------------------------------
CString Table::GetColumnLocalizedTitleText(int nCol, CBaseDocument* pDocument) const
{
	return m_Columns[nCol]->m_Title.GetLocalizedText(pDocument);
}

//---------------------------------------------------------------------------
const TableColumn* Table::GetColumn (int nCol) const
{
	return m_Columns[nCol];
}

//---------------------------------------------------------------------------
TableColumn* Table::GetColumnByAlias (WORD nAlias, int* idx) const
{
	if (idx) *idx = -1;
	for (int i = 0; i <= m_Columns.GetUpperBound(); i++)
	{
		if (m_Columns[i]->GetInternalID() == nAlias)
		{
			if (idx) *idx = i;
			return m_Columns[i]; 
		}
	}
	return NULL;
}


//---------------------------------------------------------------------------
int Table::GetColumnIndexByAlias (WORD nAlias) const
{
	for (int i = 0; i <= m_Columns.GetUpperBound(); i++)
	{
		if (m_Columns[i]->GetInternalID() == nAlias)
		{
			return i; 
		}
	}
	return -1;
}

//---------------------------------------------------------------------------
int	Table::GetColumnIndexByPtr(TableColumn* pCol) const
{
	for (int i = 0; i <= m_Columns.GetUpperBound(); i++)
	{
		if (m_Columns[i] == pCol)
		{
			return i;
		}
	}
	return -1;
}

//---------------------------------------------------------------------------
CString Table::GetTableTitleText () const
{
	return m_Title.GetText();
}

//---------------------------------------------------------------------------
CString Table::GetTableLocalizedTitleText (CBaseDocument* pDocument) const
{
	return m_Title.GetLocalizedText(pDocument);
}

//---------------------------------------------------------------------------
TableCell*	Table::GetTableCell (int nRow, int nCol) const
{
	if
	(
		nCol < 0 || nCol > LastColumn() ||
		nRow < 0 || nRow > LastRow()
	)
		return NULL;
	
	return m_Columns[nCol]->m_Cells[nRow];
}

//---------------------------------------------------------------------------
TotalCell*	Table::GetTotalCell (int nCol) const
{
	if
	(
		nCol < 0 || nCol > LastColumn() 
	)
		return NULL;
	
	return m_Columns[nCol]->m_pTotalCell;
}

//---------------------------------------------------------------------------
TableCell* Table::GetCellFromID(int nRow, WORD wID) const
{
	if (nRow >= m_Columns[0]->m_Cells.GetSize())
	{
		//ASSERT(FALSE);
		return NULL;
	}

	for (int i = 0; i <= m_Columns.GetUpperBound(); i++)
	{
		if (m_Columns[i]->GetInternalID() == wID)
			return nRow == -1 
					? m_Columns[i]->m_pTotalCell
					: m_Columns[i]->m_Cells[nRow];
	}

	return NULL;
}

//---------------------------------------------------------------------------
BOOL Table::ExistChildID(WORD wID)
{
	for (int i = 0; i <= m_Columns.GetUpperBound(); i++)
	{
		if (m_Columns[i]->GetInternalID() == wID)
			return TRUE;
	}
	return FALSE;
}

//---------------------------------------------------------------------------
void Table::ClearCellDynamicAttributes(BOOL bBrowsed /*= TRUE*/, BOOL bSelected /*= TRUE*/, BOOL bError/* = TRUE*/) 
{
	if (bBrowsed)
	{
		m_bAlternateEasyview = TRUE;
		m_arAlternateEasyviewOnPage.RemoveAll();
	}
	for (int i = 0; i <= m_Columns.GetUpperBound(); i++)
	{
		m_Columns[i]->ClearCellDynamicAttributes(bBrowsed, bSelected, bError);
	}
}

//---------------------------------------------------------------------------
CString Table::GetDynamicColumnLocalizedTitleText(int c) 
{
	if (c < 0 || c > m_Columns.GetUpperBound())
		return _T("");
	TableColumn* pCol = m_Columns[c];

	if (pCol->m_pTitleExpr)
	{
		DataStr t;
		if (pCol->m_pTitleExpr->Eval(t))
		{
			pCol->m_Title.SetText(t);
			return pCol->m_Title.GetText();
		}
	}
	return pCol->m_Title.GetLocalizedText(m_pDocument);
}

//------------------------------------------------------------------------------
void Table::OnColumnDynamicFontStyle ()
{
	TableColumn* pCol = m_Columns[m_nActiveColumn];
	BOOL        bAllColumns = FALSE, bAborted = FALSE;

	//copia espressioni originarie, da ripristinare in caso di annullamento dell'overwrite
	Expression* saveExpr = pCol->m_pTextFontExpr 
									?	new Expression(*(pCol->m_pTextFontExpr))
									:	NULL;

	CDynamicFontStyleColumnDlg dialog(
								pCol->m_pTextFontExpr,
								&(m_pDocument->m_ViewSymbolTable),
								bAllColumns,
								m_pDocument
							);

	if (dialog.DoModal() != IDOK) 
	{
		SAFE_DELETE(pCol->m_pTextFontExpr);
		pCol->m_pTextFontExpr = saveExpr;
		saveExpr = NULL;
		return;
	}

	if (bAllColumns)
	{
		SetAllColumnDynamicExpr(m_nActiveColumn, TextFontExpr, pCol->m_pTextFontExpr, bAborted);
		if (bAborted)
		{	//ripristino espressioni iniziale, perche utente ha annullato l'overwrite su tutte le colonne
			SAFE_DELETE(pCol->m_pTextFontExpr);
			pCol->m_pTextFontExpr = saveExpr;
			saveExpr = NULL;
			return;
		}
		
		m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);
	}
	else
	{
		m_pDocument->InvalidateRect(ColumnCellsRect(m_nActiveColumn), m_bTransparent);
	}
	
	SAFE_DELETE (saveExpr);

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnColumnDynamicFormatStyle ()
{
	TableColumn* pCol = m_Columns[m_nActiveColumn];
	//copia espressioni originarie, da ripristinare in caso di annullamento dell'overwrite
	Expression* saveExpr = pCol->m_pCellFormatterExpr 
									?	new Expression(*(pCol->m_pCellFormatterExpr))
									:	NULL;

	CDynamicFormatStyleColumnDlg dialog(
								pCol->m_pCellFormatterExpr,
								&(m_pDocument->m_ViewSymbolTable),
								m_pDocument,
								pCol->GetDataType()
							);

	if (dialog.DoModal() != IDOK) 
	{
		SAFE_DELETE(pCol->m_pCellFormatterExpr);
		pCol->m_pCellFormatterExpr = saveExpr;
		saveExpr = NULL;
		return;
	}

	m_pDocument->InvalidateRect(ColumnCellsRect(m_nActiveColumn), m_bTransparent);
	
	SAFE_DELETE (saveExpr);

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnColumnDynamicBorders ()
{
	TableColumn* pCol = m_Columns[m_nActiveColumn];
	BOOL        bAllColumns = FALSE, bAborted = FALSE;

	//copia espressioni originarie, da ripristinare in caso di annullamento dell'overwrite
	Expression* saveExpr = pCol->m_pCellBordersExpr 
									?	new Expression(*(pCol->m_pCellBordersExpr))
									:	NULL;

	CDynamicCellBordersDlg dialog(
								pCol->m_pCellBordersExpr,
								&(m_pDocument->m_ViewSymbolTable),
								bAllColumns,
								m_pDocument
							);

	if (dialog.DoModal() != IDOK) 
	{
		SAFE_DELETE(pCol->m_pCellBordersExpr);
		pCol->m_pCellBordersExpr = saveExpr;
		saveExpr = NULL;
		return;
	}

	if (bAllColumns)
	{
		SetAllColumnDynamicExpr(m_nActiveColumn, CellBordersExpr,pCol->m_pCellBordersExpr, bAborted);
		if (bAborted)
		{	//ripristino espressioni iniziale, perche utente ha annullato l'overwrite su tutte le colonne
			SAFE_DELETE(pCol->m_pCellBordersExpr);
			pCol->m_pCellBordersExpr = saveExpr;
			saveExpr = NULL;
			return;
		}
		
		m_pDocument->InvalidateRect(BodyRect(), m_bTransparent);
	}
	else
	{
		m_pDocument->InvalidateRect(ColumnCellsRect(m_nActiveColumn), m_bTransparent);
	}
	
	SAFE_DELETE (saveExpr);

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::OnColumnSetWidth ()
{	
	TableColumn* pCol = m_Columns[m_nActiveColumn];

	int nNewWidth = pCol->Width();

	CWidthDlg   dialog (nNewWidth, this);
 
	if (dialog.DoModal() != IDOK) 
		return;
	
	ASSERT(pCol == m_Columns[m_nActiveColumn]);

	OnColumnSetWidth(pCol, nNewWidth, TRUE);
}

//------------------------------------------------------------------------------
void Table::OnColumnSetWidth(TableColumn* pCol, int nNewWidth, BOOL bRefreshUI, BOOL bSetActive)
{
	int nPrevWidth = pCol->Width();
	if (nNewWidth == nPrevWidth)
		return;

	nNewWidth -= nPrevWidth;	//se nWidth < 0 la si sta restringendo

	if (pCol->CanHExpandColumn(nNewWidth))
	{
		if (bRefreshUI) m_pDocument->InvalidateRect(m_BaseRect, FALSE);

		int nCol = GetColumnIndexByPtr(pCol);

		HExpandColumn(nCol, nNewWidth, bRefreshUI);	//ridimensiona la colonna e sposta le successive 

		if (bRefreshUI)
		{
			if(bSetActive)
				m_pDocument->m_pActiveRect->SetActive(pCol->GetColumnTitleRect());

			m_pDocument->InvalidateRect(m_BaseRect, TRUE);	//TODO manca lo shadow
			m_pDocument->UpdateWindow();
			m_pDocument->SetModifiedFlag();
		}
	}
}

//------------------------------------------------------------------------------
void Table::OnBringToFront (UINT nID)
{
	m_pDocument->OnBringToFront (nID);
}

//------------------------------------------------------------------------------
void Table::OnSendToBack ()
{
	m_pDocument->OnSendToBack (this);
}

//---------------------------------------------------------------------------
void Table::OnSetHeights ()
{
	int	nTableTitleHeight	= m_TitleRect.Height();
	int	nColumnTitleHeight  = ColumnTitleRect(0).Height();;
	int	nRowHeight			= CellRect(0,0).Height();
	int	nTotalHeight			= TotalRect(0).Height();

	CTableHeightDlg dlg(nTableTitleHeight, nColumnTitleHeight, nRowHeight, nTotalHeight, this);
	
	if (dlg.DoModal() == IDOK)
	{
		CRect rect = m_BaseRect;
		rect.right += m_nDropShadowHeight;
		rect.bottom += m_nDropShadowHeight;
		m_pDocument->InvalidateRect(rect, FALSE);

		// reconstruct table layout and Format contents
		RebuildTableSizes
		(
			CPoint(m_BaseRect.left, m_BaseRect.top),
			nTableTitleHeight,
			nColumnTitleHeight,
			nRowHeight,
			nTotalHeight
		);

		rect = m_BaseRect;
		rect.right += m_nDropShadowHeight;
		rect.bottom += m_nDropShadowHeight;
		m_pDocument->InvalidateRect(rect, TRUE);

		if (m_pDocument->m_pCurrentObj == this)
			m_pDocument->m_pActiveRect->SetActive (GetActiveRect());

		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//---------------------------------------------------------------------------
void Table::OnSetTableRows ()
{
	int	nRows = RowsNumber();

	CTableRowsDlg dlg(nRows, this);
	
	if (dlg.DoModal() == IDOK)
	{
		nRows = nRows - dlg.m_nPrevRows;

		CRect rect = m_BaseRect;
		rect.right += m_nDropShadowHeight;
		rect.bottom += m_nDropShadowHeight;
		m_pDocument->InvalidateRect(rect, FALSE);
		//----
		
		ModifyRowsNumber (nRows);

		//----
		rect = m_BaseRect;
		rect.right += m_nDropShadowHeight;
		rect.bottom += m_nDropShadowHeight;
		m_pDocument->InvalidateRect(rect, TRUE);

		if (m_pDocument->m_pCurrentObj == this)
			m_pDocument->m_pActiveRect->SetActive (GetActiveRect());

		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//---------------------------------------------------------------------------
void Table::OnSetDropShadow	()
{
	CDropShadowDlg dlg(m_crDropShadowColor, m_nDropShadowHeight);
	
	if (dlg.DoModal() == IDOK)
	{
		int nH = m_nDropShadowHeight;

		m_crDropShadowColor = (COLORREF) (long) dlg.m_Color;
		m_nDropShadowHeight = (short) dlg.m_Height;

		CRect rect = m_BaseCellsRect;
		rect.right += max(nH, m_nDropShadowHeight);
		rect.bottom += max(nH, m_nDropShadowHeight);

		m_pDocument->InvalidateRect(rect, TRUE);

		if (m_pDocument->m_pCurrentObj == this)
			m_pDocument->m_pActiveRect->SetActive (GetActiveRect());

		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//---------------------------------------------------------------------------
void Table::OnSetColumnTooltip	()
{
	CSetTooltipDlg dlg(m_Columns[m_nActiveColumn]->m_pCellTooltipExpr, &(m_pDocument->m_ViewSymbolTable));
	dlg.DoModal();
	
	m_pDocument->SetModifiedFlag();
}

//---------------------------------------------------------------------------
void Table::OnSetColumnTitleTooltip	()
{
	CSetTooltipDlg dlg(m_Columns[m_nActiveColumn]->m_pTitleTooltipExpr, &(m_pDocument->m_ViewSymbolTable));
	dlg.DoModal();

	m_pDocument->SetModifiedFlag();
}

//---------------------------------------------------------------------------
void Table::OnCopyActiveColumnAttributes	()
{
	CChooseObjectDlg   dialog(m_pDocument, this);

	if (dialog.DoModal() == IDOK)
	{
		m_Columns[m_nActiveColumn]->CopyAttributes (dialog.m_pSelected);

		m_pDocument->InvalidateRect(m_Columns[m_nActiveColumn]->GetColumnRect());

		if (m_pDocument->m_pCurrentObj == this)
			m_pDocument->m_pActiveRect->SetActive (GetActiveRect());

		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//---------------------------------------------------------------------------
void Table::OnCopyActiveColumnTitleAttributes	()
{
	CChooseObjectDlg   dialog(m_pDocument, this);

	if (dialog.DoModal() == IDOK)
	{
		m_Columns[m_nActiveColumn]->CopyHeaderAttributes (dialog.m_pSelected);

		m_pDocument->InvalidateRect(m_Columns[m_nActiveColumn]->GetColumnTitleRect());

		if (m_pDocument->m_pCurrentObj == this)
			m_pDocument->m_pActiveRect->SetActive (GetActiveRect());

		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//---------------------------------------------------------------------------
void Table::OnToggleTemplate	()
{
	m_bTemplate = !m_bTemplate;

	m_pDocument->SetModifiedFlag();
}

//---------------------------------------------------------------------------
void Table::OnColumnToggleTemplate	()
{
	TableColumn* pCol = m_Columns[m_nActiveColumn];
	pCol->m_bTemplate = !pCol->m_bTemplate;
	if (pCol->m_bTemplate)
		m_bTemplate = TRUE;
	m_pDocument->SetModifiedFlag();
}

//---------------------------------------------------------------------------
void Table::OnTableSetStyle	()
{
	CListStyleDlg dlg(m_pDocument, m_sStyleClass, this, this);
	if (dlg.DoModal() == IDOK && m_sStyleClass.CompareNoCase(dlg.m_sStyleClass))
	{
		if (dlg.m_sStyleClass.CompareNoCase(CWoormTemplate::s_sCustom_StyleName))
		{
			ClearStyle();
		}
		//RemoveStyle();

		m_sStyleClass = dlg.m_sStyleClass;
		m_pDocument->SetTemplateStyle(this);

		m_pDocument->InvalidateRect(m_BaseRect, TRUE);
		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//---------------------------------------------------------------------------
void Table::OnTableSetStyleName	()
{
	CSetNameDlg   dlg(m_pDocument, this);
	if (dlg.DoModal() == IDOK && !dlg.m_sName.IsEmpty())
	{
		m_sStyleClass = dlg.m_sName;
		m_pDefault = NULL;

		m_pDocument->SetModifiedFlag();
	}
}

//---------------------------------------------------------------------------
void Table::OnClearTableCustomStyles	()
{
	ClearStyle();

	m_pDocument->InvalidateRect(m_BaseRect, TRUE);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//---------------------------------------------------------------------------
void Table::OnColumnSetStyle	()
{
	TableColumn* pCol = m_Columns[m_nActiveColumn];
	CListStyleDlg dlg(m_pDocument, pCol->m_sStyleClass, pCol, this);
	if (dlg.DoModal() == IDOK && pCol->m_sStyleClass.CompareNoCase(dlg.m_sStyleClass))
	{
		if (dlg.m_sStyleClass.CompareNoCase(CWoormTemplate::s_sCustom_StyleName))
		{
			pCol->ClearStyle();
		}
		//pCol->RemoveStyle();

		pCol->m_sStyleClass = dlg.m_sStyleClass;
		m_pDocument->SetTemplateStyle(pCol);

		m_pDocument->InvalidateRect(pCol->m_ColumnRect, TRUE);
		m_pDocument->UpdateWindow();
		m_pDocument->SetModifiedFlag();
	}
}

//---------------------------------------------------------------------------
void Table::OnColumnSetStyleName	()
{
	TableColumn* pCol = m_Columns[m_nActiveColumn];
	
	CSetNameDlg   dlg(m_pDocument, this);
	if (dlg.DoModal() == IDOK && !dlg.m_sName.IsEmpty())
	{
		pCol->m_sStyleClass = dlg.m_sName;
		pCol->m_pDefault = NULL;

		m_pDocument->SetModifiedFlag();
	}
}

//---------------------------------------------------------------------------
void Table::OnClearColumnCustomStyles	()
{
	TableColumn* pCol = m_Columns[m_nActiveColumn];

	pCol->ClearStyle();

	m_pDocument->InvalidateRect(pCol->m_ColumnRect, TRUE);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//---------------------------------------------------------------------------
void	Table::OnClearAllCustomStyles ()
{
	for (int c = 0; c <= m_Columns.GetUpperBound(); c++)
	{
		TableColumn* pCol = m_Columns[c];

		pCol->ClearStyle();
	}

	OnClearTableCustomStyles();
}

//---------------------------------------------------------------------------
void	Table::RemoveAllStyles ()
{
	for (int c = 0; c <= m_Columns.GetUpperBound(); c++)
	{
		TableColumn* pCol = m_Columns[c];

		pCol->RemoveStyle();
	}
	RemoveStyle();
}

//---------------------------------------------------------------------------
void	Table::ClearAllStyles ()
{
	for (int c = 0; c <= m_Columns.GetUpperBound(); c++)
	{
		TableColumn* pCol = m_Columns[c];

		pCol->ClearStyle();
	}
	ClearStyle();
}

//---------------------------------------------------------------------------
void	Table::SetStyle (Table* pTplTable)
{
	RemoveStyle ();
	if (!pTplTable) return;
	m_pDefault = pTplTable;
	m_sStyleClass = pTplTable->m_sStyleClass;
	//---- ----

	if (m_bTransparent == FALSE)
		m_bTransparent = m_pDefault->m_bTransparent;

	if (m_Borders == TableBorders())
		m_Borders = m_pDefault->m_Borders;

	if (m_TitlePen == BorderPen())
		m_TitlePen = m_pDefault->m_TitlePen;

	if (!m_bHideTableTitle)
		m_bHideTableTitle = m_pDefault->m_bHideTableTitle;
	if (!m_bHideColumnsTitle)
		m_bHideColumnsTitle = m_pDefault->m_bHideColumnsTitle;
	
	if (!m_bAlternateBkgColorOnRow && !m_bAlternateBkgColorOnMultiLineRow && m_crAlternateBkgColorOnRow == AfxGetThemeManager()->GetAlternateColor())
	{
		m_bAlternateBkgColorOnRow			= m_pDefault->m_bAlternateBkgColorOnRow;
		m_bAlternateBkgColorOnMultiLineRow	= m_pDefault->m_bAlternateBkgColorOnMultiLineRow;
		m_crAlternateBkgColorOnRow				= m_pDefault->m_crAlternateBkgColorOnRow;
	}

	if (m_nDropShadowHeight == 0 && m_crDropShadowColor == 0)
	{
		m_nDropShadowHeight = m_pDefault->m_nDropShadowHeight;
		m_crDropShadowColor = m_pDefault->m_crDropShadowColor;
	}

	if (m_bFiscalEnd == FALSE)
		m_bFiscalEnd = m_pDefault->m_bFiscalEnd;

	if (m_Title.m_nAlign == DEFAULT_ALIGN)
		m_Title.m_nAlign = m_pDefault->m_Title.m_nAlign;

	BaseRect::SetTemplateFont (m_pDocument, m_Title.m_nFontIdx, m_pDefault->m_Title.m_nFontIdx, FNT_DEFAULT);

	if (m_Title.m_rgbTextColor == DEFAULT_TEXTCOLOR)
		m_Title.m_rgbTextColor = m_pDefault->m_Title.m_rgbTextColor;
	if (m_Title.m_rgbBkgColor == DEFAULT_BKGCOLOR)
		m_Title.m_rgbBkgColor = m_pDefault->m_Title.m_rgbBkgColor;
}

//---------------------------------------------------------------------------
void	Table::RemoveStyle ()
{
	if (!m_pDefault) return;
	//---- ----

	if (m_bTransparent == m_pDefault->m_bTransparent)
		m_bTransparent = FALSE;

	if (m_Borders == m_pDefault->m_Borders)
		m_Borders = TableBorders();

	if (m_TitlePen == m_pDefault->m_TitlePen)
		m_TitlePen = BorderPen();

	if (m_bHideTableTitle == m_pDefault->m_bHideTableTitle)
		m_bHideTableTitle = FALSE;
	if (m_bHideColumnsTitle == m_pDefault->m_bHideColumnsTitle)
		m_bHideColumnsTitle = FALSE;
	
	if (
			m_bAlternateBkgColorOnRow			== m_pDefault->m_bAlternateBkgColorOnRow &&
			m_bAlternateBkgColorOnMultiLineRow	== m_pDefault->m_bAlternateBkgColorOnMultiLineRow &&
			m_crAlternateBkgColorOnRow				== m_pDefault->m_crAlternateBkgColorOnRow
		)
	{
		m_bAlternateBkgColorOnRow = FALSE;
		m_bAlternateBkgColorOnMultiLineRow = FALSE;
		m_crAlternateBkgColorOnRow = AfxGetThemeManager()->GetAlternateColor();
	}

	if (m_nDropShadowHeight == m_pDefault->m_nDropShadowHeight &&
		m_crDropShadowColor == m_pDefault->m_crDropShadowColor)
	{
		m_nDropShadowHeight = 0;
		m_crDropShadowColor = 0;
	}

	if (m_bFiscalEnd == m_pDefault->m_bFiscalEnd)
		m_bFiscalEnd = FALSE;

	if (m_Title.m_nAlign == m_pDefault->m_Title.m_nAlign)
		m_Title.m_nAlign = DEFAULT_ALIGN;

	BaseRect::RemoveTemplateFont(m_pDocument,	m_Title.m_nFontIdx, m_pDefault->m_Title.m_nFontIdx, FNT_DEFAULT);

	if (m_Title.m_rgbTextColor == m_pDefault->m_Title.m_rgbTextColor)
		m_Title.m_rgbTextColor = DEFAULT_TEXTCOLOR;
	if (m_Title.m_rgbBkgColor == m_pDefault->m_Title.m_rgbBkgColor)
		m_Title.m_rgbBkgColor = DEFAULT_BKGCOLOR;

	//---- ----
	m_pDefault = NULL;
}

//---------------------------------------------------------------------------
void	Table::ClearStyle ()
{
	m_bTransparent 			= m_pDefault ? m_pDefault->m_bTransparent			: FALSE;

	m_Borders 				= m_pDefault ? m_pDefault->m_Borders				: TableBorders();
	m_TitlePen 				= m_pDefault ? m_pDefault->m_TitlePen				: BorderPen();

	m_bHideTableTitle 		= m_pDefault ? m_pDefault->m_bHideTableTitle		: FALSE;
	m_bHideColumnsTitle		= m_pDefault ? m_pDefault->m_bHideColumnsTitle		: FALSE;
	
	m_bAlternateBkgColorOnRow			= m_pDefault ? m_pDefault->m_bAlternateBkgColorOnRow		: FALSE;
	m_bAlternateBkgColorOnMultiLineRow	= m_pDefault ? m_pDefault->m_bAlternateBkgColorOnMultiLineRow	: FALSE;
	m_crAlternateBkgColorOnRow 				= m_pDefault ? m_pDefault->m_crAlternateBkgColorOnRow			: AfxGetThemeManager()->GetAlternateColor();

	m_nDropShadowHeight		= m_pDefault ? m_pDefault->m_nDropShadowHeight		: 0;
	m_crDropShadowColor		= m_pDefault ? m_pDefault->m_crDropShadowColor		: 0;

	m_bFiscalEnd			= m_pDefault ? m_pDefault->m_bFiscalEnd				: FALSE;

	m_Title.m_nAlign		= m_pDefault ? m_pDefault->m_Title.m_nAlign			: DEFAULT_ALIGN;

	BaseRect::ClearTemplateFont (m_pDocument, m_Title.m_nFontIdx, m_pDefault ? &(m_pDefault->m_Title.m_nFontIdx) : NULL, FNT_DEFAULT);
	//m_Title.m_nFontIdx		= m_pDefault ? m_pDefault->m_Title.m_nFontIdx		: AfxGetFontStyleTable()->GetFontIdx(FNT_DEFAULT);

	m_Title.m_rgbTextColor	= m_pDefault ? m_pDefault->m_Title.m_rgbTextColor	: DEFAULT_TEXTCOLOR;
	m_Title.m_rgbBkgColor	= m_pDefault ? m_pDefault->m_Title.m_rgbBkgColor	: DEFAULT_BKGCOLOR;
}

//---------------------------------------------------------------------------
BOOL	Table::IsTemplate() const
{
	if (m_bTemplate)
		return TRUE;

	for (int c = 0; c <= m_Columns.GetUpperBound(); c++)
	{
		TableColumn* pCol = m_Columns[c];
		if (pCol->m_bTemplate)
			return TRUE;
	}
	return FALSE;
}
//---------------------------------------------------------------------------
BOOL Table::IsTemplateOverridden() const
{
	//TODO: non gestibile
	//if (m_bTemplateOverridden)
	//	return TRUE;

	for (int c = 0; c <= m_Columns.GetUpperBound(); c++)
	{
		TableColumn* pCol = m_Columns[c];
		if (pCol->m_bTemplate && !pCol->m_bTemplateOverridden)
			return FALSE;
	}
	return TRUE;
}

//---------------------------------------------------------------------------
void Table::PurgeTemplateColumns()
{
	for (int nCol = 0; nCol <= m_Columns.GetUpperBound();)
	{
		//deve rimanere almeno una colonna
		if (m_Columns.GetSize() == 1)
			break;

		TableColumn* pColumn = m_Columns[nCol];
		if (!pColumn->m_bTemplate || pColumn->m_bTemplateOverridden)
		{
			//---- Extract from DeleteColumn(int)
			int             width   = pColumn->Width();
			CRect           old_rect= m_BaseRect;
			if (HasTotal(nCol)) m_nTotalCounter--;
			// remove columm from array and delete it
			m_Columns.RemoveAt(nCol);
			delete pColumn;

			// adjust size and position of left positioned columns and modify global rect
			// (i.e. m_BaseRect m_BaseCellsRect titleRec)
			LeftShiftColumn (nCol, width);
			//----
			continue;
		}
		nCol++;
	}
//#ifdef _DEBUG
//	if (m_Columns.GetSize() == 1)
//	{
//		TableColumn* pColumn = m_Columns[0];
//		if (!pColumn->m_bTemplate)
//			pColumn->m_bTemplate = TRUE;
//	}
//#else
//	(m_Columns[0])->m_bTemplate = TRUE;
//#endif
}

//---------------------------------------------------------------------------
void Table::MarkTemplateOverridden()
{
	if (m_bTemplate && m_pDefault)
	{
		m_pDefault->m_bTemplateOverridden = TRUE;
	}

	for (int nCol = 0; nCol <= m_Columns.GetUpperBound(); nCol++)
	{
		TableColumn* pColumn = m_Columns[nCol];
		if (pColumn->m_bTemplate && pColumn->m_pDefault) 
		{
			pColumn->m_pDefault->m_bTemplateOverridden = TRUE;
		}
	}
}

//---------------------------------------------------------------------------
void Table::MergeTemplateColumns()
{
	if (!m_bTemplate || !m_pDefault)
	{
		return;
	}

	for (int nCol = 0; nCol <= m_pDefault->m_Columns.GetUpperBound(); nCol++)
	{
		TableColumn* pColumn = m_pDefault->m_Columns[nCol];

		if (pColumn->m_bTemplate && !pColumn->m_bTemplateOverridden) //c'e' almeno una colonna
		{
			TableColumn* pNewTplCol = new TableColumn(*pColumn);
			pNewTplCol->m_wInternalID = m_pDocument->m_pEditorManager->GetNextId();
			pNewTplCol->m_pTable = this;

			pColumn->m_bTemplateOverridden = TRUE;

			int idx = m_Columns.Add (pNewTplCol);

			// must be done after creation of new column
			int width    = pNewTplCol->Width();
			int XOffset  = m_BaseRect.Width() - (pNewTplCol->GetColumnCellsRect().left - m_pDefault->m_BaseRect.left);

			// adjust table title and global size and shift
			m_TitleRect.right     += width;
			m_BaseCellsRect.right += width;
			m_BaseRect.right      += width;

			// move new column for width of previosus column
			m_Columns[idx]->HMoveColumn(XOffset);
		}
	}
}

//---------------------------------------------------------------------------
void Table::RenameAlias(int offset)
{
	m_wInternalID += offset;

	for (int nCol = 0; nCol <= m_Columns.GetUpperBound(); nCol++)
	{
		TableColumn* pColumn = m_Columns[nCol];

		pColumn->m_wInternalID += offset;
	}
}

//=============================================================================

void Table::GetAnchoredFields(CArray<BaseRect*, BaseRect*>&	arAnchoredFields)
{
	for (int c = 0; c <= m_Columns.GetUpperBound(); c++)
	{
		TableColumn* pCol = m_Columns[c];

		//NO! arAnchoredFields.Append(pCol->m_arAnchoredFields);	//Pertroppo Devo eliminare i duplicati (campi ancorati ad un range di colonne)
		for (int i = 0 ; i < pCol->m_arAnchoredFields.GetCount(); i++)
		{
			BaseRect* pObj = pCol->m_arAnchoredFields[i];

			int j = 0;
			for (; j < arAnchoredFields.GetCount(); j++)
			{
				if (pObj == arAnchoredFields[j])
					break;
			}
			if (j == arAnchoredFields.GetCount())
				arAnchoredFields.Add(pObj);
		}
	}
}

//---------------------------------------------------------------------------
void Table::RemoveAnchoredFields()
{
	for (int c = 0; c <= m_Columns.GetUpperBound(); c++)
	{
		TableColumn* pCol = m_Columns[c];

		pCol->m_arAnchoredFields.RemoveAll();
	}
}

//-----------------------------------------------------------------------------
void Table::OnLayoutCopyObject ()
{
	CChooseLayoutDlg   dlg(m_pDocument);
	if (dlg.DoModal() == IDOK)
	{
		m_pDocument->LayoutCopyObject(&(m_pDocument->GetObjects()), dlg.m_pSelectedLayout, this);
	}
}

//-----------------------------------------------------------------------------
void Table::OnLayoutMoveObject ()
{
	CChooseLayoutDlg   dlg(m_pDocument);
	if (dlg.DoModal() == IDOK)
	{
		m_pDocument->LayoutMoveObject(&(m_pDocument->GetObjects()),  dlg.m_pSelectedLayout, this);
	}
}

//=============================================================================

BOOL Table::CanDeleteField(LPCTSTR pszFieldName, CString& sLog) const
{
	if (m_pHideExpr && m_pHideExpr->HasMember(pszFieldName))
		return sLog = _TB("Table's hidden condition"), FALSE;

	for (int c = 0; c <= m_Columns.GetUpperBound(); c++)
	{
		TableColumn* pCol = m_Columns[c];

		if (!pCol->CanDeleteField (pszFieldName, sLog))
			return FALSE;
	}

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL Table::FillSeries(CBCGPChartSeries* pSeries, WORD nColumnID)
{
	TableColumn* pCol = this->GetColumnByAlias(nColumnID);
	if (!pCol)
		return FALSE;
	return pCol->FillSeries(pSeries, nColumnID);
}

//------------------------------------------------------------------------------
void Table::Redraw(BOOL repaint, int prevShadowHeight, int movedOf)
{
	CRect rect = m_BaseRect;
	rect.right += max(m_nDropShadowHeight, prevShadowHeight)+ abs(movedOf);
	rect.bottom += max(m_nDropShadowHeight, prevShadowHeight) + abs(movedOf);
	//rect.bottom 
	m_pDocument->InvalidateRect(rect, repaint);

	if (repaint && m_pDocument->m_pCurrentObj == this)
		m_pDocument->m_pActiveRect->SetActive(GetActiveRect());

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::MouseMove(CDC& cdc, CPoint point, CBCGPScrollView* pScrollView, BOOL bTransMode)
{
	//devo capire se sto cercando di muovere la tabella o spostando le colonne o ancora trasformando le colonne in fieldrect
	if (bTransMode && m_nAction == MOVE /*&& m_nActiveRow == ACTIVE_IS_TITLE*/ && m_nActiveColumn >= 0)
	{
		m_bTransMode = TRUE;

		if(m_nActiveRow == ACTIVE_IS_TITLE)
			MyClipCursor(cdc, pScrollView, AllColumnsTitleRect());
		else if(m_nActiveRow > 0)
			MyClipCursor(cdc, pScrollView, RowRect(m_nActiveRow));
	
		m_pDocument->GetWoormView()->ManageDragToTransform(point);	
		return;
	}

	if (m_bTransMode)
	{
		//avevo bloccato il mouse e ora lo sblocco
		ClipCursor(NULL);
		m_bTransMode = FALSE;
	}

	__super::MouseMove(cdc, point, pScrollView);
}
//=============================================================================
