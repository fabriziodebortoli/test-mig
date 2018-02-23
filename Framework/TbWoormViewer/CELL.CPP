
#include "stdafx.h"      

#include <TbNameSolver\PathFinder.h>
#include <TbGeneric\RdeProtocol.h>
#include <TbGenlib\baseapp.h>
#include <TbNameSolver\PathFinder.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include "woormini.h"
#include "woormdoc.h"
#include "woormvw.h"
#include "mclrdlg.h"
#include "cell.h"
#include "listdlg.h"
#include "column.h"
#include "rectobj.h"
#include "export.h"
#include "ExpExter.h"
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
//---------------------------------------------------------------------------
TableCell::TableCell (TableColumn* pColumn, CPoint ptOrigin, CSize szCellSize)
	:
	m_rectCell	(CRect(ptOrigin,szCellSize)),
	m_nCurrRow	(Table::ACTIVE_IS_TITLE),
	m_pColumn	(pColumn)
{
	m_Value.SetAlign	(DEFAULT_CELL_STRING_ALIGN);
	m_Value.SetFontIdx	(AfxGetFontStyleTable()->GetFontIdx(FNT_CELL_STRING));
	m_Value.SetTextColor(DEFAULT_CELL_FOREGROUND);
	m_Value.SetBkgColor	(DEFAULT_CELL_BACKGROUND);
}
   
//---------------------------------------------------------------------------
TableCell::TableCell	(TableCell& source, TableColumn* pColumn)
	:
	m_Value		(source.m_Value),
	m_rectCell	(source.m_rectCell),
	m_nCurrRow	(Table::ACTIVE_IS_TITLE),//should be source.m_nCurrRow?
	m_pColumn   (pColumn)
{
	m_Value.SetText(_T(""));
	m_Value.m_RDEdata.DisableData();
}	

//---------------------------------------------------------------------------
TableCell::TableCell (const TableCell& source)
	:
	m_Value		(source.m_Value),
	m_rectCell	(source.m_rectCell),
	m_nCurrRow	(source.m_nCurrRow),
	m_pColumn	(source.m_pColumn)
{
	m_arBrowsedOnPage		.Copy(source.m_arBrowsedOnPage);
	m_arSelectedOnPage		.Copy(source.m_arSelectedOnPage);
	m_arWrongOnPage			.Copy(source.m_arWrongOnPage);
	m_arTooltip				.Copy(source.m_arTooltip);
}

//------------------------------------------------------------------------------
TableCell::~TableCell	()
{
	ClearCellDynamicAttributes();
}

//------------------------------------------------------------------------------
BOOL TableCell::ClearCellDynamicAttributes(BOOL bBrowsed /*= TRUE*/, BOOL bSelected /*= TRUE*/, BOOL bWrong/* = TRUE*/) 
{
	int n = 0;
	if (bBrowsed) 
	{
		n += m_arBrowsedOnPage.GetSize();
		m_arBrowsedOnPage.RemoveAll();

	}
	if (bSelected) 
	{
		n += m_arSelectedOnPage.GetSize();
		m_arSelectedOnPage.RemoveAll();
	}
	if (bWrong) 
	{
		n += m_arWrongOnPage.GetSize();
		m_arWrongOnPage.RemoveAll();
	}
	m_arTooltip.RemoveAll();
	return n > 0;
}

//------------------------------------------------------------------------------
BOOL TableCell::IsBrowsed(int page)
{
	for (int i=0; i < m_arBrowsedOnPage.GetSize(); i++)
		if (page == m_arBrowsedOnPage[i])
			return TRUE;
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL TableCell::IsWrong(int page)
{
	for (int i=0; i < m_arWrongOnPage.GetSize(); i++)
		if (page == m_arWrongOnPage[i])
			return TRUE;
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL TableCell::IsSelected(int page)
{
	for (int i=0; i < m_arSelectedOnPage.GetSize(); i++)
		if (page == m_arSelectedOnPage[i])
			return TRUE;
	return FALSE;
}

//------------------------------------------------------------------------------
CString TableCell::GetTooltip(int nPage) 
{
	DataStr sTip;
	if (m_pColumn->m_pCellTooltipExpr)
		m_pColumn->m_pCellTooltipExpr->Eval(sTip);

	if (nPage < 0 || nPage >= m_arTooltip.GetSize())
		return sTip;
	return sTip + '\n' + m_arTooltip[nPage];
}

//------------------------------------------------------------------------------
void TableCell::DrawBarCodeCell
	(
		CDC&		DC,
		CRect		inside,
		CWoormDocMng*	pDoc,
		CBarCode*	pBarCode,
		BOOL		bPreview
	)
{
	CString sErr;
	if (m_Value.GetText().IsEmpty())
	{
		m_Value.Draw (DC, inside, pDoc);
	}
	else if (!::CBarCode_Draw(pBarCode, DC, inside, pDoc, m_Value, sErr, m_nCurrRow, bPreview))
	{
		CString sV = m_Value.GetText();

		m_Value.SetText(sErr + _T(" [") + sV + ']');

		m_Value.Draw(DC, inside, pDoc);

		m_Value.SetText(sV);
	}
}

//---------------------------------------------------------------------------
void TableCell::DrawSubTotal
	(
		CDC&			DC,
		CRect			inside,
		CWoormDocMng*		pDoc,
		BasicTextStyle&	subTotal,
		BOOL			bOpaque
	)
{
	// copy standard m_Value
	Value buffer(m_Value);

	//TODO NON viene salvato nel wrm nella sezione Table-SubTotal 
	//buffer.m_nAlign = subTotal.GetAlign();

	if (
			m_pColumn &&
			!m_pColumn->m_pCellBordersExpr &&
			(
				m_pColumn->m_SubTotal.m_pBkgColorExpr || m_pColumn->m_SubTotal.m_pTextColorExpr ||
				m_pColumn->m_pTextFontExpr || m_pColumn->m_pCellFormatterExpr
			)
		)
	{
		if (m_nCurrRow >= 0 && m_nCurrRow < m_pColumn->m_Cells.GetSize())
			m_pColumn->GetDocument()->UpdateViewSymbolTable (m_pColumn->m_pTable, m_nCurrRow);
	}

	// change attribute
	buffer.SetFontIdx 	(subTotal.GetFontIdx	());
	
	// dynamic format style
	if (m_pColumn->m_pCellFormatterExpr)
	{
		Formatter* fmt = m_pColumn->GetCellDynamicFormatter();
		if (fmt)
		{
			buffer.FormatWithValue(fmt, m_pColumn->GetDocument());
		}
	}

	if (m_pColumn->m_SubTotal.m_pBkgColorExpr || m_pColumn->m_SubTotal.m_pTextColorExpr)
	{
		if (m_pColumn->m_SubTotal.m_pBkgColorExpr && !m_pColumn->m_SubTotal.m_pBkgColorExpr->IsEmpty())
		{
			DataLng color;
			if (m_pColumn->m_SubTotal.m_pBkgColorExpr->Eval(color))
				buffer.SetBkgColor	((COLORREF)(long)color);
			else
			{
				// in lettura facilitata inverto i colori della cella se non ha una colorazione dinamica
				if (m_pColumn->m_pTable->UseColorEasyview (m_nCurrRow) && subTotal.GetBkgColor() == DEFAULT_BKGCOLOR)
					buffer.SetBkgColor	(m_pColumn->m_pTable->m_crAlternateBkgColorOnRow);
				else
					buffer.SetBkgColor	(subTotal.GetBkgColor());
			}
		}

		BOOL bEvalTextColorOk = FALSE;
		if (m_pColumn->m_SubTotal.m_pTextColorExpr && !m_pColumn->m_SubTotal.m_pTextColorExpr->IsEmpty())
		{
			DataLng color;
			if (m_pColumn->m_SubTotal.m_pTextColorExpr->Eval(color))
			{
				bEvalTextColorOk = TRUE;
				buffer.SetTextColor((COLORREF)(long)color);
			}
		}
		if (!bEvalTextColorOk)
		{
			buffer.SetTextColor(subTotal.GetTextColor());
			if (subTotal.GetFontIdx() != FNT_ERROR && buffer.m_rgbTextColor == DEFAULT_TEXTCOLOR)
			{
				const FontStyle* pFontStyle = pDoc->m_pFontStyles->GetFontStyle(subTotal.GetFontIdx(), &(pDoc->GetNamespace()));
				if (pFontStyle && pFontStyle->GetColor() != DEFAULT_TEXTCOLOR)
					buffer.SetTextColor(pFontStyle->GetColor());
			}
		}
	}
	else //there is no expression, setting static color value 
	{
		buffer.SetTextColor	(subTotal.GetTextColor	());
		if (subTotal.GetFontIdx() != FNT_ERROR && buffer.m_rgbTextColor == DEFAULT_TEXTCOLOR)
		{
			const FontStyle* pFontStyle = pDoc->m_pFontStyles->GetFontStyle(subTotal.GetFontIdx(), &(pDoc->GetNamespace()));
			if (pFontStyle && pFontStyle->GetColor() != DEFAULT_TEXTCOLOR)
				buffer.SetTextColor(pFontStyle->GetColor());
		}

		// in lettura facilitata inverto i colori della cella se non ha una colorazione dinamica o custom
		if (m_pColumn->m_pTable->UseColorEasyview (m_nCurrRow) && subTotal.GetBkgColor() == DEFAULT_BKGCOLOR)
			buffer.SetBkgColor	(m_pColumn->m_pTable->m_crAlternateBkgColorOnRow);
		else
			buffer.SetBkgColor	(subTotal.GetBkgColor ());
	}
	
	int internalID = m_pColumn->GetInternalID();

	BOOL bBrowsed =  FALSE;
	BOOL bWrong =  FALSE;
	BOOL bSelected =  FALSE;
	BOOL bLastSelected =  FALSE;

	if (pDoc && pDoc->m_pRDEmanager && !pDoc->m_pRDEmanager->IsClosed())
	{
		int nPage	= pDoc->m_pRDEmanager->CurrPageRead();

		bBrowsed	= IsBrowsed(nPage);
		bWrong		= IsWrong(nPage);
		bSelected	= IsSelected(nPage);
		if (
			bSelected &&
			pDoc->m_findWordInfo.m_nCurrPage == nPage &&
			pDoc->m_findWordInfo.m_nCurrRow  == m_nCurrRow
			)
		{
			int nc = pDoc->m_findWordInfo.m_nCurrCol;

			CExportTableItem* pItem = pDoc->m_pExportData->GetExportTableItem();
			if (pItem && nc >= 0 && nc < pItem->m_SelectedCol.GetSize())
			{
				nc = pItem->m_SelectedCol[nc].m_ColIdx;
			}

			if (m_pColumn->m_pTable->GetColumnIndexByAlias(internalID) == nc)
				bLastSelected = TRUE;
		}
	}

	BOOL bShowLink = !DC.IsPrinting() && 
						m_pColumn->m_pTable->m_pDocument->GetEnableLink(m_pColumn->m_pTable, internalID, m_nCurrRow);
	
	int flags = bShowLink ? GenericText::LINKED : GenericText::NORMAL;
	if (bShowLink && bBrowsed)
		flags |= GenericText::BROWSED;
	if (bWrong)
		flags |= GenericText::WRONG;
	if (bSelected)
	{
		flags |= GenericText::SELECTED;
		if (bLastSelected)
		{
			flags |= GenericText::LAST_SELECTED;
		}
	}

	buffer.Draw (DC, inside, pDoc, bOpaque, flags);
}

//---------------------------------------------------------------------------
void TableCell::DrawNormalCell
	(
		CDC&		DC,
		CRect		inside,
		CWoormDocMng*	pDoc,
		BOOL		bOpaque,
		BOOL		bIsTextFile /* = FALSE*/
	)
{
	// copy standard m_Value
	Value buffer(m_Value);

	ASSERT(m_pColumn);

	if (
			m_pColumn->m_bVMergeEqualCell &&
			m_nCurrRow > 0 &&
			m_Value.m_RDEdata.IsValid() &&
			!m_Value.m_RDEdata.IsSubTotal() &&
			!m_Value.GetText().IsEmpty()
		)
	{
		if (
				m_pColumn->m_PreviousValue.m_RDEdata.IsValid() &&
				!m_pColumn->m_PreviousValue.m_RDEdata.IsSubTotal() &&
				m_pColumn->m_PreviousValue.GetText() == m_Value.GetText()
			) 
			buffer.SetText(L"");
	}

	// dynamic font style attribute
	FontIdx nCustomFontIdx = FNT_ERROR;
	if (m_pColumn->m_pTextFontExpr && !m_pColumn->m_pTextFontExpr->IsEmpty())
	{
		nCustomFontIdx = m_pColumn->GetCellDynamicFontIdx();
		if (nCustomFontIdx != FNT_ERROR)
			buffer.SetFontIdx (nCustomFontIdx);
	}

	// dynamic formatter style
	if (m_pColumn->m_pCellFormatterExpr && !m_pColumn->m_pCellFormatterExpr->IsEmpty())
	{
		Formatter* fmt = m_pColumn->GetCellDynamicFormatter();
		if (fmt)
		{
			buffer.FormatWithValue(fmt, m_pColumn->GetDocument());
		}
	}

	if (bIsTextFile)
	{
		CString strFileName = buffer.GetText();
		if (!strFileName.IsEmpty())
		{
			strFileName = AfxGetPathFinder()->FromNs2Path(strFileName, CTBNamespace::TEXT, CTBNamespace::FILE);

			CString strText;
			if (LoadLineTextFile (strFileName, strText))
				buffer.SetText (strText);
			else
				buffer.SetText (_T(""));
		}
	}

	// in lettura facilitata inverto i colori della cella se non ha una colorazione dinamica
	if (m_pColumn->m_pTable->UseColorEasyview (m_nCurrRow))
		buffer.SetBkgColor	(m_pColumn->m_pTable->m_crAlternateBkgColorOnRow);

	if (m_pColumn->m_pBkgColorExpr && !m_pColumn->m_pBkgColorExpr->IsEmpty())
		buffer.SetBkgColor	(GetValueBkgColor(buffer.GetBkgColor()));

	BOOL bEvalTextColorOk = FALSE;
	if (m_pColumn->m_pTextColorExpr && !m_pColumn->m_pTextColorExpr->IsEmpty())
	{
		buffer.SetTextColor(GetValueTextColor(&bEvalTextColorOk));
	}
	if (!bEvalTextColorOk && buffer.m_rgbTextColor == DEFAULT_TEXTCOLOR)
	{
		if (m_pColumn->m_pTextFontExpr && nCustomFontIdx != FNT_ERROR)
		{
			const FontStyle* pFontStyle = pDoc->m_pFontStyles->GetFontStyle(nCustomFontIdx, &(pDoc->GetNamespace()));
			if (pFontStyle && pFontStyle->GetColor() != DEFAULT_TEXTCOLOR)
				buffer.SetTextColor(pFontStyle->GetColor());
		}
		else if (m_pColumn->m_nFontIdx != FNT_ERROR)
		{
			const FontStyle* pFontStyle = pDoc->m_pFontStyles->GetFontStyle(m_pColumn->m_nFontIdx, &(pDoc->GetNamespace()));
			if (pFontStyle && pFontStyle->GetColor() != DEFAULT_TEXTCOLOR)
				buffer.SetTextColor(pFontStyle->GetColor());
		}
	}

	int internalID = m_pColumn->GetInternalID();

	BOOL bBrowsed =  FALSE;
	BOOL bWrong =  FALSE;
	BOOL bSelected =  FALSE;
	BOOL bLastSelected =  FALSE;

	if (pDoc && pDoc->m_pRDEmanager && !pDoc->m_pRDEmanager->IsClosed())
	{
		int nPage	= pDoc->m_pRDEmanager->CurrPageRead();

		bBrowsed	= IsBrowsed	(nPage);
		bWrong		= IsWrong	(nPage);
		bSelected	= IsSelected(nPage);
		if (
			bSelected &&
			pDoc->m_findWordInfo.m_nCurrPage == nPage &&
			pDoc->m_findWordInfo.m_nCurrRow  == m_nCurrRow
			)
		{
			int nc = pDoc->m_findWordInfo.m_nCurrCol;

			CExportTableItem* pItem = pDoc->m_pExportData->GetExportTableItem();
			if (pItem && nc >= 0 && nc < pItem->m_SelectedCol.GetSize())
			{
				nc = pItem->m_SelectedCol[nc].m_ColIdx;
			}

			if (m_pColumn->m_pTable->GetColumnIndexByAlias(internalID) == nc)
				bLastSelected = TRUE;
		}
	}

	BOOL bShowLink = !DC.IsPrinting() &&
					 m_pColumn->GetDocument()->GetEnableLink(m_pColumn->m_pTable, internalID, m_nCurrRow);
	
	int flags = bShowLink ? GenericText::LINKED : GenericText::NORMAL;
	if (bShowLink && bBrowsed)
		flags |= GenericText::BROWSED;
	if (bWrong)
		flags |= GenericText::WRONG;
	if (bSelected)
	{
		flags |= GenericText::SELECTED;
		if (bLastSelected)
		{
			flags |= GenericText::LAST_SELECTED;
		}
	}

	buffer.Draw (DC, inside, pDoc, bOpaque, flags, CSize(0, 0), this->m_pColumn->m_bMiniHtml);
}

//---------------------------------------------------------------------------
void TableCell::Draw
	(
		CDC&			DC,
		BOOL			bPreview,
		CRect			invalid,
		CWoormDocMng*		pDoc,
		BasicTextStyle&	subTotal,
		CellBorders&	borders,
		BOOL			bNoBorders, 
		CObject*		pGraphic,
		BOOL			bMarkExportable,
		BOOL			bOpaque,
		BOOL  			bIsExpTitles /*= FALSE*/,
		BOOL			bIsTextFile /*= FALSE*/
	)
{
	CRect rect = m_rectCell;
	ScaleRect(rect, DC);

	// change font style attribute
	ASSERT(m_pColumn);
	if (/*m_pColumn && */m_pColumn->m_pCellBordersExpr)
	{
		Borders b(borders);
		if (m_pColumn->GetCellDynamicBorders(b))
		{
			((Borders&)borders) = b;
		}
	}

	// invalid is already in device point
	if (DC.IsPrinting() || rect.IntersectRect(rect, invalid))
	{
		CRect inside = InsideRect (DC, m_rectCell, *borders.m_pColumnPen, borders, bNoBorders);

		if (borders.m_pRowSepPen && borders.bottom && !bNoBorders)
		{
			Borders b(borders);
			b.bottom = FALSE;
			if (b.Exists())
				DrawBorders (DC, m_rectCell, *borders.m_pColumnPen, &b, FALSE);
			b.bottom = TRUE; b.left = b.right = b.top = FALSE;
			DrawBorders (DC, m_rectCell, *borders.m_pRowSepPen, &b, FALSE);

			inside.bottom += borders.m_pColumnPen->GetWidth() - borders.m_pRowSepPen->GetWidth();
		}
		else
		{
			DrawBorders (DC, m_rectCell, *borders.m_pColumnPen, &borders, bNoBorders);
		}

		// if there are no data use default cell background only
		if (!m_Value.m_RDEdata.IsEnabled())	
		{
			DrawNormalCell	(DC, inside, pDoc, bOpaque);

			if (bMarkExportable)
				GenericDrawObj::DrawForExport(DC, m_rectCell, GetValueTextColor (), bIsExpTitles);

			return;
		}

		if (pGraphic == NULL)
		{
			BOOL bSelected =  FALSE;
			if (pDoc && pDoc->m_pRDEmanager && !pDoc->m_pRDEmanager->IsClosed())
			{
				bSelected = IsSelected(pDoc->m_pRDEmanager->CurrPageRead());
			}

			if (m_Value.m_RDEdata.IsSubTotal())
				DrawSubTotal	(DC, inside, pDoc, subTotal, bOpaque);
			else
				DrawNormalCell	(DC, inside, pDoc, bOpaque, bIsTextFile);

			if (bMarkExportable && !bSelected)
				GenericDrawObj::DrawForExport(DC, m_rectCell, GetValueTextColor (), bIsExpTitles);
            
			return;
		}

		if (pGraphic->IsKindOf(RUNTIME_CLASS(CBarCode)))
		{
			DrawBarCodeCell	(DC, inside, pDoc, (CBarCode*) pGraphic, bPreview);
			if (bMarkExportable)
				GenericDrawObj::DrawForExport(DC, m_rectCell, GetValueTextColor (), bIsExpTitles);
		}
		else if (pGraphic->IsKindOf(RUNTIME_CLASS(CTBPicture)))
		{
			CString sFileName(m_Value.GetText());
			sFileName.Trim();

			if (!sFileName.IsEmpty())
			{
				CString sPart;
				int idx = sFileName.ReverseFind(':');
				if (idx > -1)
				{
					sPart = sFileName.Mid(idx + 1);
					sFileName = sFileName.Left(idx);
				}

				CTBPicture* pBitmap = (CTBPicture*)pGraphic;
				if (pBitmap && pBitmap->ReadFile(sFileName, TRUE, TRUE))
				{
					if (!sPart.IsEmpty())
					{
						CString sPartTop, sPartBottom;
						int idx = sPart.ReverseFind(',');
						if (idx > -1)
						{
							sPartTop = sPart.Left(idx);
							sPartBottom = sPart.Mid(idx + 1);
						
							//int h = pBitmap->GetHeight();
							int w = pBitmap->GetWidth();

							int top = atoi((CStringA)sPartTop);
							int bottom = atoi((CStringA)sPartBottom);

							CRect cutted(0, top, w, bottom);
							GenericDrawObj::DrawBitmapWithFitMode(DC, inside, pBitmap, !bOpaque, m_Value.m_rgbBkgColor, m_Value.GetAlign(), TRUE, cutted);
							return;
						}
					}

					GenericDrawObj::DrawBitmapWithFitMode(DC, inside, pBitmap, !bOpaque, m_Value.m_rgbBkgColor, m_Value.GetAlign(), FALSE, inside/*non usato*/);
					return;
				}
			}
			m_Value.Draw(DC, inside, pDoc, bOpaque);
		}
	}
}

//------------------------------------------------------------------------------
COLORREF TableCell::GetValueTextColor (BOOL* pbEvalOk) const
{
	ASSERT(m_pColumn);
	if (pbEvalOk) *pbEvalOk = FALSE;
	if (m_pColumn->m_pTextColorExpr)
	{
		DataLng color;
		if (m_pColumn->m_pTextColorExpr->Eval(color))
		{
			if (pbEvalOk) *pbEvalOk = TRUE;
			return (COLORREF)(long) color;
		}
	}
	return m_Value.GetTextColor();
}

//------------------------------------------------------------------------------
COLORREF TableCell::GetValueBkgColor (COLORREF cr) const
{
	ASSERT(m_pColumn);
	if (m_pColumn->m_pBkgColorExpr)
	{
		DataLng color;
		if (m_pColumn->m_pBkgColorExpr->Eval(color))
		{
			return (COLORREF)(long) color;
		}
	}
	return cr; //m_Value.GetBkgColor();
}

//------------------------------------------------------------------------------
void TableCell::GetCellColor (COLORREF* rgbColors) const
{
	rgbColors[CCellColorsDlg::VALUE]		= m_Value.GetTextColor();
	rgbColors[CCellColorsDlg::BACKGROUND]	= m_Value.GetBkgColor();
}

//------------------------------------------------------------------------------
void TableCell::SetCellColor (COLORREF* rgbColors)
{
	m_Value.SetTextColor (rgbColors[CCellColorsDlg::VALUE]);
	m_Value.SetBkgColor	(rgbColors[CCellColorsDlg::BACKGROUND]);
}

//------------------------------------------------------------------------------
void TableCell::Redraw()
{
	/*IMPLEMENTARE SE SERVE, guarda Table::OnCellColor*/
	m_pColumn->GetTable()->m_pDocument->InvalidateRect(m_rectCell, TRUE);
	m_pColumn->GetTable()->m_pDocument->UpdateWindow();
	m_pColumn->GetTable()->m_pDocument->SetModifiedFlag(TRUE);
}

//==============================================================================
//			Class TotalCell  implementation
//==============================================================================

//---------------------------------------------------------------------------
TotalCell::TotalCell (TableColumn* pCol, CPoint ptOrigin, CSize szCellSize)
	:
	TableCell (pCol, ptOrigin, szCellSize)
{
	m_Value.SetAlign	(DEFAULT_TOTAL_STRING_ALIGN);
	m_Value.SetFontIdx	(AfxGetFontStyleTable()->GetFontIdx(FNT_TOTAL_STRING));
	m_Value.SetTextColor(DEFAULT_TOTAL_FOREGROUND);
	m_Value.SetBkgColor	(DEFAULT_TOTAL_BACKGROUND);
	m_nCurrRow			= Table::ACTIVE_IS_TOTAL;
}
   

//---------------------------------------------------------------------------
TotalCell::TotalCell (TotalCell& source, TableColumn* pColumn)
	:
	TableCell (source, pColumn)
{
}

//---------------------------------------------------------------------------
TotalCell::TotalCell (const TotalCell& source)
	:
	TableCell	(source),
	m_TotalPen	(source.m_TotalPen)
{
}

//---------------------------------------------------------------------------
void TotalCell::DrawTotal
	(
		CDC&		DC,
		CRect		invalid,
		CWoormDocMng*	pDoc,
		Borders		borders,
		BOOL		bNoBorders,
		BOOL		bOpaque
	)
{
	CRect rect = m_rectCell;
	ScaleRect(rect, DC);
	// invalid is already in device point
	if (DC.IsPrinting() || rect.IntersectRect(rect, invalid))
	{
		DrawDropShadow(DC);

		DrawBorders	(DC, m_rectCell, m_TotalPen, &borders, bNoBorders);
        CRect inside = InsideRect (DC, m_rectCell, m_TotalPen, borders, bNoBorders);
		
		ASSERT(m_pColumn);
		if (
				m_pColumn->m_pTotalTextColorExpr ||
				m_pColumn->m_pTotalBkgColorExpr ||
				m_pColumn->m_pCellFormatterExpr
		)
			m_pColumn->GetDocument()->UpdateViewSymbolTable ();

		Value buffer(m_Value);

		// dynamic format style
		if (m_pColumn->m_pCellFormatterExpr)
		{
			Formatter* fmt = m_pColumn->GetCellDynamicFormatter();
			if (fmt)
			{
				buffer.FormatWithValue(fmt, m_pColumn->GetDocument());
			}
		}

		buffer.SetBkgColor	(GetValueBkgColor());
		buffer.SetTextColor	(GetValueTextColor());
		buffer.Draw (DC, inside, pDoc, bOpaque);
    }
}

//---------------------------------------------------------------------------
void TotalCell::DrawDropShadow(CDC& DC)
{
	m_pColumn->m_pTable->DrawDropShadow(DC, m_rectCell, m_pColumn->m_pTable->m_nDropShadowHeight, m_pColumn->m_pTable->m_crDropShadowColor);
}

//---------------------------------------------------------------------------
void TotalCell::HideTotal
	(
		CDC&	DC,
		CRect	invalid,
		Borders	borders,
		BOOL	bNoBorders,
		BOOL	bNextColumnHasTotal,
		int		nNextVisibleColumn
	)
{
	CRect rect = m_rectCell;
	ScaleRect(rect, DC);
	// invalid is already in device point
	if (DC.IsPrinting() || rect.IntersectRect(rect, invalid))
	{
		BorderPen* pen = &m_TotalPen;

		borders.right = borders.right && bNextColumnHasTotal;
		if (borders.right && nNextVisibleColumn > 0)
		{
			TableColumn* pCol = m_pColumn->m_pTable->m_Columns[nNextVisibleColumn];
			if (pCol && pCol->m_pTotalCell)
				pen = &(pCol->m_pTotalCell->m_TotalPen);
		}
		DrawBorders (DC, m_rectCell, *pen, &borders, bNoBorders);
    }
}

//------------------------------------------------------------------------------
void TotalCell::GetTotalColor (COLORREF* rgbColors) const
{
	rgbColors[CTotalColorsDlg::VALUE]		= m_Value.GetTextColor();
	rgbColors[CTotalColorsDlg::BACKGROUND]	= m_Value.GetBkgColor();
	rgbColors[CTotalColorsDlg::BORDER]		= m_TotalPen.GetColor();
}

//------------------------------------------------------------------------------
void TotalCell::SetTotalColor (COLORREF* rgbColors)
{
	m_Value.SetTextColor(rgbColors[CTotalColorsDlg::VALUE]);
	m_Value.SetBkgColor	(rgbColors[CTotalColorsDlg::BACKGROUND]);
	m_TotalPen.SetColor	(rgbColors[CTotalColorsDlg::BORDER]);
}

//------------------------------------------------------------------------------
void TotalCell::GetCellColor (COLORREF* rgbColors) const
{
	rgbColors[CTotalColorsDlg::VALUE]		= m_Value.GetTextColor();
	rgbColors[CTotalColorsDlg::BACKGROUND]	= m_Value.GetBkgColor();
}


//------------------------------------------------------------------------------
void TotalCell::SetCellColor (COLORREF* rgbColors)
{
	m_Value.SetTextColor	(rgbColors[CTotalColorsDlg::VALUE]);
	m_Value.SetBkgColor		(rgbColors[CTotalColorsDlg::BACKGROUND]);
}

//------------------------------------------------------------------------------
COLORREF TotalCell::GetValueTextColor () const
{
	if (m_pColumn && m_pColumn->m_pTotalTextColorExpr)
	{
		DataLng color;
		if (m_pColumn->m_pTotalTextColorExpr->Eval(color))
		{
			return (COLORREF)(long) color;
		}
	}
	return m_Value.GetTextColor();
}

//------------------------------------------------------------------------------
COLORREF TotalCell::GetValueBkgColor () const
{
	if (m_pColumn && m_pColumn->m_pTotalBkgColorExpr)
	{
		DataLng color;
		if (m_pColumn->m_pTotalBkgColorExpr->Eval(color))
		{
			return (COLORREF)(long) color;
		}
	}
	return m_Value.GetBkgColor();
}

//------------------------------------------------------------------------------
void TotalCell::Redraw()
{
	//E' stato scelto di reimplementare questo metodo virtuale (già presente nella classe parent), per invalidare tutta la riga dei totali.
	//Questo perchè altrimenti non veniva disegnato il bordo a sx della cella del totale.
	m_pColumn->GetTable()->m_pDocument->InvalidateRect(m_pColumn->GetTable()->AllTotalsRect(), TRUE);
	m_pColumn->GetTable()->m_pDocument->UpdateWindow();
	m_pColumn->GetTable()->m_pDocument->SetModifiedFlag(TRUE);
}
