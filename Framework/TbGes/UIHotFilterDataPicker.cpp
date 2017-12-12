
#include "stdafx.h"

#include <TbGes\UnpinnedTilesPane.h>
// Dbl
//#include <Items\Dbl\TItem.h>

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include "HotFilter.h"
#include "UIHotFilterDataPicker.h"
#include "UIHotFilterDataPicker.hjson"
#include "DataPickerRecordSet.h"

#ifdef _DEBUG
#undef THIS_FILE                                                        
static char THIS_FILE[] = __FILE__;     
#endif                                

//////////////////////////////////////////////////////////////////////////////
//							CHotFilterDataPickerFrame
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterDataPickerFrame, CBatchFrame)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotFilterDataPickerFrame, CBatchFrame)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------	
CHotFilterDataPickerFrame::CHotFilterDataPickerFrame()
	:
	CBatchFrame()
{
}

//-----------------------------------------------------------------------------
BOOL CHotFilterDataPickerFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	__super::OnCustomizeTabbedToolBar(pTabbedBar);

	CTBToolBar* pToolbar = pTabbedBar->FindToolBar(szToolbarNameMain);

	pToolbar->InsertButtonBefore
	(
		ID_HFLDATAPICKER_EXTRACT_DATA,
		_NS_TOOLBARBTN("HotFilterDataPickerExtractData"),
		TBIcon(szIconExtract, TOOLBAR),
		_TB("Extract"),
		ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN
	);

	pToolbar->InsertButtonBefore
	(
		ID_HFLDATAPICKER_UNDO_EXTRACTION,
		_NS_TOOLBARBTN("HotFilterDataPickerUndoExtraction"),
		TBIcon(szIconUndo, TOOLBAR),
		_TB("Undo"),
		ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN
	);

	pToolbar->InsertButtonBefore
	(
		ID_HFLDATAPICKER_COMPLETED,
		_NS_TOOLBARBTN("HotFilterDataPickerCompleted"),
		TBIcon(szIconSelect, TOOLBAR),
		_TB("Confirm"),
		ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN
	);

	pToolbar->RemoveButtonForID(ID_EXTDOC_REPORT);
	pToolbar->RemoveButtonForID(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN);
	pToolbar->RemoveButtonForID(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN);

	((HotFilterDataPicker*)GetDocument())->m_pUnpinnedTilesPane = CUnpinnedTilesPane::Create(this);

	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//					CHotFilterDataPickerTBGrid 
/////////////////////////////////////////////////////////////////////////////
//
//==============================================================================
IMPLEMENT_DYNCREATE(CHotFilterDataPickerTBGrid, CTBGridControl)

BEGIN_MESSAGE_MAP(CHotFilterDataPickerTBGrid, CTBGridControl)
	//{{AFX_MSG_MAP(CHotFilterDataPickerTBGrid)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//------------------------------------------------------------------------------
CHotFilterDataPickerTBGrid::CHotFilterDataPickerTBGrid(const CString sName)
	:
	CTBGridControl	(sName),
	m_pRecordSet	(NULL)
{
}

//------------------------------------------------------------------------------
CHotFilterDataPickerTBGrid::~CHotFilterDataPickerTBGrid()
{
}

//------------------------------------------------------------------------------
void CHotFilterDataPickerTBGrid::Customize()
{
	EnableVirtualMode();

	m_CachedItems.m_nCachePageCount = CACHE_PAGE_COUNT;
	m_CachedItems.m_nCachePageSize = CACHE_PAGE_SIZE;
}

//------------------------------------------------------------------------------
void CHotFilterDataPickerTBGrid::CreateGridLayout()
{
	__super::CreateAllColumns();
}

//------------------------------------------------------------------------------
void CHotFilterDataPickerTBGrid::SetRowsCount(int nRows)
{
	SetVirtualRows(nRows);

	CTileDialog* pTile = dynamic_cast<CTileDialog*>(GetParentForm());
	if (pTile)
	{
		pTile->SetTitle(cwsprintf(_TB("Result ({0-%d} found)"), nRows));
		pTile->UpdateTitleView();
	}
}

//------------------------------------------------------------------------------
void CHotFilterDataPickerTBGrid::ClearRowsCount()
{
	SetVirtualRows(0);
	CTileDialog* pTile = dynamic_cast<CTileDialog*>(GetParentForm());
	if (pTile)
	{
		pTile->SetTitle(_TB("Result"));
		pTile->UpdateTitleView();
	}
}

//------------------------------------------------------------------------------
void CHotFilterDataPickerTBGrid::SetCurrentRecord(int nRow)
{
	m_pRecordSet->FindData(nRow);
	m_pCurrentRecord =  m_pRecordSet->GetVirtualRow(nRow);
}

//------------------------------------------------------------------------------
BOOL CHotFilterDataPickerTBGrid::IsColumnReadOnly(int nColumn)
{
	// first column (selected) is editable
	return nColumn > 0;
}

//------------------------------------------------------------------------------
void CHotFilterDataPickerTBGrid::OnHeaderColumnClick (int nColumn)
{
	if (nColumn == 0)
	{
		HotFilterDataPicker* pDoc = (HotFilterDataPicker*)GetDocument();

		BOOL bSel = m_pRecordSet->SelectDeselectAllLines();
		bool bValue(bSel ? true : false);

		for (LONG r = m_pRecordSet->GetFirstBufferedRowNo(); r <= m_pRecordSet->GetLastBufferedRowNo(); r++)
		{
			CBCGPGridRow* pRow = GetVirtualRow(r);
			CBCGPGridCheckItem* pCheck = (CBCGPGridCheckItem*)pRow->GetItem(0);
			
			pCheck->SetValue(_variant_t(bValue));

			DataBool* pSel = pDoc->GetSelected(m_pRecordSet->GetVirtualRow(r));
			*pSel = bSel;
		}

	}
	else
		__super::OnHeaderColumnClick(nColumn);
}

//------------------------------------------------------------------------------
void  CHotFilterDataPickerTBGrid::OnItemChanged(CBCGPGridItem* pItem, int nRow, int nColumn)
{
	__super::OnItemChanged(pItem, nRow, nColumn);
	if (nColumn > 0)
		return;

	bool b(pItem->GetValue());

	m_pRecordSet->ChangeStatus(nRow, b ? TRUE : FALSE);
}

/////////////////////////////////////////////////////////////////////////////
//					CHotFilterDataPickerResultsTBGridTileDlg Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterDataPickerResultsTBGridTileDlg, CTileDialog)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotFilterDataPickerResultsTBGridTileDlg, CTileDialog)
	//{{AFX_MSG_MAP(CHotFilterDataPickerResultsTBGridTileDlg)		
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CHotFilterDataPickerResultsTBGridTileDlg::CHotFilterDataPickerResultsTBGridTileDlg()
	:     
	CTileDialog(_NS_TABDLG("CHotFilterDataPickerResultsTBGridTileDialog"),IDD_HFLDATAPICKER_RESULTS_TBGRID)
{
}

//-----------------------------------------------------------------------------
CHotFilterDataPickerResultsTBGridTileDlg::~CHotFilterDataPickerResultsTBGridTileDlg()
{
}

//-----------------------------------------------------------------------------
void CHotFilterDataPickerResultsTBGridTileDlg::BuildDataControlLinks()
{
	GetDocument()->m_pTBGridControl = (CHotFilterDataPickerTBGrid*)AddLinkGrid
	(
		IDC_HFLDATAPICKER_RESULTS_TBGRID,
		GetDocument()->m_pRecordSet,
		RUNTIME_CLASS(CHotFilterDataPickerTBGrid),
		_T("ResultsTBGrid")
	);
	GetDocument()->m_pTBGridControl->Attach(GetDocument()->m_pRecordSet);
}                         

//-----------------------------------------------------------------------------------
BOOL CHotFilterDataPickerResultsTBGridTileDlg::OnPrepareAuxData()
{
	GetDocument()->CreateGridLayout();
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//			Class CHotFilterDataPickerTileGrp Declaration & Implementation
/////////////////////////////////////////////////////////////////////////////
//

//-----------------------------------------------------------------------------
class TB_EXPORT CHotFilterDataPickerTileGrp : public CPinnedTilesTileGroup
{
	DECLARE_DYNCREATE(CHotFilterDataPickerTileGrp)

private:
	HotFilterDataPicker* GetDocument() { return (HotFilterDataPicker*) __super::GetDocument(); }

protected:
	virtual void Customize();
};

IMPLEMENT_DYNCREATE(CHotFilterDataPickerTileGrp, CPinnedTilesTileGroup)

//-----------------------------------------------------------------------------
void CHotFilterDataPickerTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);
	SetLayoutAlign(CLayoutContainer::STRETCH);

	AttachUnpinnedTilesPane(GetDocument()->m_pUnpinnedTilesPane);

	GetDocument()->m_pFiltersPanel = AddPanel(_T("Filters"), _TB("Filters"), CLayoutContainer::COLUMN, CLayoutContainer::BEGIN);
	GetDocument()->m_pFiltersPanel->SetTileStyle(AfxGetTileDialogStyleNormal());
	GetDocument()->m_pFiltersPanel->SetTileDialogStyle(AfxGetTileDialogStyleFilter());
	GetDocument()->m_pFiltersPanel->SetCollapsible(TRUE);
	//GetDocument()->GetHotFilter()->CustomizeDataPickerFilter(this, GetDocument()->m_pFiltersPanel);
	// no filters, hide the filter panel and unpinned pane
	if (GetTileDialogs()->GetSize() == 0)
	{
		GetDocument()->m_pFiltersPanel->SetPinnable(TRUE);
		GetDocument()->m_pFiltersPanel->SetPinned(FALSE);
		GetDocument()->m_pUnpinnedTilesPane->ShowWindow(SW_HIDE); // @@TODO non funziona!
	}
	else
		for (int d = 0; d <= GetTileDialogs()->GetUpperBound(); d++)
			GetDocument()->m_FilterTiles.Add(GetTileDialogs()->GetAt(d));

	AddTile
	(
		RUNTIME_CLASS(CHotFilterDataPickerResultsTBGridTileDlg), 
		IDD_HFLDATAPICKER_RESULTS_TBGRID, 
		_TB("Result"), 
		TILE_AUTOFILL
	);
}

//==============================================================================
//	CHotFilterDataPickerView
//==============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterDataPickerView, CMasterFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotFilterDataPickerView, CMasterFormView)
	//{{AFX_MSG_MAP(CHotFilterDataPickerView)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
HotFilterDataPicker* CHotFilterDataPickerView::GetDocument() const { return (HotFilterDataPicker*) m_pDocument; }

//-----------------------------------------------------------------------------
CHotFilterDataPickerView::CHotFilterDataPickerView()
	:
	CMasterFormView(_NS_VIEW("HotFilterView"),IDD_HFLDATAPICKER_VIEW)
{
	EnableLayout(TRUE);
}

//-----------------------------------------------------------------------------
void CHotFilterDataPickerView::BuildDataControlLinks()
{
	//GetDocument()->m_SavedQueriesCombo.FillListBox();

	m_pLayoutContainer->SetFlex(1);
	AddTileGroup
	(
		IDC_HFLDATAPICKER, 
		RUNTIME_CLASS(CHotFilterDataPickerTileGrp), 
		_NS_TILEGRP("HotFilterDataPickerTileGrp")
	);

}

//-----------------------------------------------------------------------------------
BOOL CHotFilterDataPickerView::OnPrepareAuxData()
{
	BOOL bSomePicked = GetDocument()->m_pHotFilter->m_PickedItemsList.GetSize() > 0 || GetDocument()->m_pHotFilter->m_UnselectedItemsList.GetSize() > 0 ;
	if (bSomePicked)
	{
		GetDocument()->PopulateGrid(FALSE);
		//as some data are already extracted and selected, the user must press "undo" to repeat the query
		GetDocument()->m_bCanDoExtractData = FALSE; 
		GetDocument()->SetFiltersEnabled(FALSE);
		GetDocument()->SetFiltersCollapsed(TRUE);
	}

	return TRUE;
}
