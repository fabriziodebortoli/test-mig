#include "stdafx.h"

#include "ItemsListTools.h"
#include "extdoc.h"

#include "UIHotFilter.h"
#include "UIHotFilter.hjson"
#include "HotFilterDataPicker.h"
#include "UIHotFilterDataPicker.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterTileObj		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CHotFilterTileObj, CTileDialog)

//----------------------------------------------------------------------------
CHotFilterTileObj::CHotFilterTileObj(const CString& sName, UINT nIDD/*=0*/)
    :
    CTileDialog		(sName, nIDD),
	m_pHotFilter	(NULL)
{
	SetTileStyle(AfxGetTileDialogStyleFilter());
	SetMinHeight(ORIGINAL);
}

//----------------------------------------------------------------------------
CHotFilterTileObj::~CHotFilterTileObj()
{
}

//-----------------------------------------------------------------------------
void CHotFilterTileObj::AttachOwner(CObject* pOwner)
{
	ASSERT(pOwner->IsKindOf(RUNTIME_CLASS(HotFilterObj)));

	m_pHotFilter = (HotFilterObj*)pOwner;
}

//-----------------------------------------------------------------------------
void CHotFilterTileObj::OnPinUnpin()
{
	m_pHotFilter->ResetCriteria();

	if (m_pHotFilter->GetDocument()) 
		m_pHotFilter->GetDocument()->UpdateDataView();
}

//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterRangeTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterRangeTile, CHotFilterTileObj)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotFilterRangeTile, CHotFilterTileObj)
	ON_EN_VALUE_CHANGED	(IDC_HFL_RANGE_EDIT_FROM,				OnFromChanged)
	ON_EN_VALUE_CHANGED	(IDC_HFL_RANGE_EDIT_TO,					OnToChanged)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CHotFilterRangeTile::CHotFilterRangeTile()
    :
    CHotFilterTileObj	(_NS_DLG("ERP.Core.Components.HotFilterRangeTile"), IDD_TD_HOTFILTER_RANGE)
{
}

//----------------------------------------------------------------------------
CHotFilterRangeTile::CHotFilterRangeTile(const CString& sName, int nIDD)
    :
    CHotFilterTileObj	(sName, nIDD)
{
}

//----------------------------------------------------------------------------
CHotFilterRangeTile::~CHotFilterRangeTile()
{
}

//-----------------------------------------------------------------------------
void CHotFilterRangeTile::AttachOwner(CObject* pOwner)
{
	ASSERT(pOwner->IsKindOf(RUNTIME_CLASS(HotFilterRange)));

	__super::AttachOwner(pOwner);
}

//-----------------------------------------------------------------------------
void CHotFilterRangeTile::OnFromChanged()
{
	GetHotFilter()->m_RangeTo = GetHotFilter()->m_RangeFrom;

	GetHotFilter()->NotifyChanged(HFL_ELEMENT_FROM, m_hWnd);

	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void CHotFilterRangeTile::OnToChanged()
{
	GetHotFilter()->NotifyChanged(HFL_ELEMENT_TO, m_hWnd);
}

//----------------------------------------------------------------------------
void CHotFilterRangeTile::OnDisableControlsForBatch()
{
	GetHotFilter()->ManageReadOnly();
}

//----------------------------------------------------------------------------
void CHotFilterRangeTile::BuildDataControlLinks()
{
	// if customized, ctrl class must be a CStrEdit
	ASSERT(GetHotFilter()->m_pRangeCtrlClass->IsDerivedFrom(RUNTIME_CLASS(CStrEdit)));

	CParsedCtrl* pCtrl = AddLink
	(
		IDC_HFL_RANGE_EDIT_FROM,
		_NS_LNK("From"),
		NULL, &(GetHotFilter()->m_RangeFrom),
		GetHotFilter()->m_pRangeCtrlClass,
		GetHotFilter()->m_pHKLRangeFrom
	);
	pCtrl->SetCtrlStyle(pCtrl->GetCtrlStyle() | CTRL_STYLE_SHOW_FIRST);

	pCtrl = AddLink
	(
		IDC_HFL_RANGE_EDIT_TO,
		_NS_LNK("To"),
		NULL, &(GetHotFilter()->m_RangeTo),
		GetHotFilter()->m_pRangeCtrlClass,
		GetHotFilter()->m_pHKLRangeTo
	);
	pCtrl->SetCtrlStyle(pCtrl->GetCtrlStyle() | CTRL_STYLE_SHOW_LAST);
}

//////////////////////////////////////////////////////////////////////////////////////////
//						CHotFilterRangeWithSelectionTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterRangeWithSelectionTile, CHotFilterRangeTile)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotFilterRangeWithSelectionTile, CHotFilterRangeTile)
	ON_EN_VALUE_CHANGED	(IDC_HFL_RANGE_RADIO_ALL,				OnRadiobuttonChanged)
	ON_EN_VALUE_CHANGED	(IDC_HFL_RANGE_RADIO_RANGE,				OnRadiobuttonChanged)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CHotFilterRangeWithSelectionTile::CHotFilterRangeWithSelectionTile()
    :
    CHotFilterRangeTile	(_NS_DLG("ERP.Core.Components.HotFilterRangeWithSelectionTile"), IDD_TD_HOTFILTER_RANGE_WITH_SELECTION)
{
}

//----------------------------------------------------------------------------
CHotFilterRangeWithSelectionTile::CHotFilterRangeWithSelectionTile(const CString& sName, int nIDD)
    :
    CHotFilterRangeTile	(sName, nIDD)
{
}

//----------------------------------------------------------------------------
CHotFilterRangeWithSelectionTile::~CHotFilterRangeWithSelectionTile()
{
}

//-----------------------------------------------------------------------------
void CHotFilterRangeWithSelectionTile::AttachOwner(CObject* pOwner)
{
	__super::AttachOwner(pOwner);
}

//-----------------------------------------------------------------------------
void CHotFilterRangeWithSelectionTile::OnRadiobuttonChanged()
{
	GetHotFilter()->ManageReadOnly();

	GetHotFilter()->NotifyChanged(HFL_ELEMENT_RADIO, m_hWnd);

	GetDocument()->UpdateDataView();
}

//----------------------------------------------------------------------------
void CHotFilterRangeWithSelectionTile::BuildDataControlLinks()
{
	// if customized, ctrl class must be a CStrEdit
	ASSERT(GetHotFilter()->m_pRangeCtrlClass->IsDerivedFrom(RUNTIME_CLASS(CStrEdit)));

	__super::BuildDataControlLinks();

	AddLink
	(
		IDC_HFL_RANGE_RADIO_ALL,
		_NS_LNK("All"),
		NULL, &(GetHotFilter()->m_bAll),
		RUNTIME_CLASS(CBoolButton)
	);
	AddLink
	(
		IDC_HFL_RANGE_RADIO_RANGE,
		_NS_LNK("Range"),
		NULL, &(GetHotFilter()->m_bRange),
		RUNTIME_CLASS(CBoolButton)
	);
}

//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterRangeDateTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterRangeDateTile, CHotFilterTileObj)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotFilterRangeDateTile, CHotFilterTileObj)
	ON_EN_VALUE_CHANGED	(IDC_HFL_RANGE_EDIT_FROM,				OnFromChanged)
	ON_EN_VALUE_CHANGED	(IDC_HFL_RANGE_EDIT_TO,					OnToChanged)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CHotFilterRangeDateTile::CHotFilterRangeDateTile()
    :
    CHotFilterTileObj	(_NS_DLG("ERP.Core.Components.HotFilterRangeDateTile"), IDD_TD_HOTFILTER_RANGE_DATE)
{
}

//----------------------------------------------------------------------------
CHotFilterRangeDateTile::CHotFilterRangeDateTile(const CString& sName, int nIDD)
    :
    CHotFilterTileObj	(sName, nIDD)
{
}

//----------------------------------------------------------------------------
CHotFilterRangeDateTile::~CHotFilterRangeDateTile()
{
}

//-----------------------------------------------------------------------------
void CHotFilterRangeDateTile::AttachOwner(CObject* pOwner)
{
	ASSERT(pOwner->IsKindOf(RUNTIME_CLASS(HotFilterDateRange)));

	__super::AttachOwner(pOwner);

	if (GetHotFilter()->m_bDefaultCurrentMonth)
	{
		GetHotFilter()->m_RangeFrom	= DataDate(1, AfxGetApplicationDate().Month(), AfxGetApplicationDate().Year());
		GetHotFilter()->m_RangeTo	= DataDate(GetHotFilter()->m_RangeFrom.MonthDays(), AfxGetApplicationDate().Month(), AfxGetApplicationDate().Year());
	}
}

//-----------------------------------------------------------------------------
void CHotFilterRangeDateTile::OnFromChanged()
{
	if (!GetHotFilter()->m_RangeMin.IsEmpty() && GetHotFilter()->m_RangeFrom < GetHotFilter()->m_RangeMin)
		GetHotFilter()->m_RangeTo = GetHotFilter()->m_RangeMin;

	if (GetHotFilter()->m_bDefaultCurrentMonth)
		GetHotFilter()->m_RangeTo	= DataDate(GetHotFilter()->m_RangeFrom.MonthDays(), GetHotFilter()->m_RangeFrom.Month(), GetHotFilter()->m_RangeFrom.Year());
	else
		GetHotFilter()->m_RangeTo = GetHotFilter()->m_RangeFrom;

	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void CHotFilterRangeDateTile::OnToChanged()
{
	if (!GetHotFilter()->m_RangeMax.IsEmpty() && GetHotFilter()->m_RangeTo > GetHotFilter()->m_RangeMax)
			GetHotFilter()->m_RangeTo = GetHotFilter()->m_RangeMax;

	GetHotFilter()->NotifyChanged(HFL_ELEMENT_TO, m_hWnd);
}

//----------------------------------------------------------------------------
void CHotFilterRangeDateTile::OnDisableControlsForBatch()
{
	GetHotFilter()->ManageReadOnly();
}

//----------------------------------------------------------------------------
void CHotFilterRangeDateTile::BuildDataControlLinks()
{
	AddLink
	(
		IDC_HFL_RANGE_EDIT_FROM,
		_NS_LNK("From"),
		NULL, &(GetHotFilter()->m_RangeFrom),
		RUNTIME_CLASS(CDateEdit)
	);

	AddLink
	(
		IDC_HFL_RANGE_EDIT_TO,
		_NS_LNK("To"),
		NULL, &(GetHotFilter()->m_RangeTo),
		RUNTIME_CLASS(CDateEdit)
	);
}

//////////////////////////////////////////////////////////////////////////////////////////
//						CHotFilterDateRangeWithSelectionTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterDateRangeWithSelectionTile, CHotFilterRangeDateTile)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotFilterDateRangeWithSelectionTile, CHotFilterRangeDateTile)
	ON_EN_VALUE_CHANGED	(IDC_HFL_RANGE_RADIO_ALL,				OnRadiobuttonChanged)
	ON_EN_VALUE_CHANGED	(IDC_HFL_RANGE_RADIO_RANGE,				OnRadiobuttonChanged)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CHotFilterDateRangeWithSelectionTile::CHotFilterDateRangeWithSelectionTile()
    :
    CHotFilterRangeDateTile	(_NS_DLG("ERP.Core.Components.HotFilterDateRangeWithSelectionTile"), IDD_TD_HOTFILTER_RANGE_DATE_WITH_SELECTION)
{
}

//----------------------------------------------------------------------------
CHotFilterDateRangeWithSelectionTile::~CHotFilterDateRangeWithSelectionTile()
{
}

//-----------------------------------------------------------------------------
void CHotFilterDateRangeWithSelectionTile::AttachOwner(CObject* pOwner)
{
	__super::AttachOwner(pOwner);
}

//-----------------------------------------------------------------------------
void CHotFilterDateRangeWithSelectionTile::OnRadiobuttonChanged()
{
	if (GetHotFilter()->m_bRange && GetHotFilter()->m_bDefaultCurrentMonth)
	{
		GetHotFilter()->m_RangeFrom	= DataDate(1, AfxGetApplicationDate().Month(), AfxGetApplicationDate().Year());
		GetHotFilter()->m_RangeTo	= DataDate(GetHotFilter()->m_RangeFrom.MonthDays(), AfxGetApplicationDate().Month(), AfxGetApplicationDate().Year());
	}
	m_pHotFilter->ManageReadOnly();
	GetDocument()->UpdateDataView();
}

//----------------------------------------------------------------------------
void CHotFilterDateRangeWithSelectionTile::BuildDataControlLinks()
{
	__super::BuildDataControlLinks();

	AddLink
	(
		IDC_HFL_RANGE_RADIO_ALL,
		_NS_LNK("All"),
		NULL, &(GetHotFilter()->m_bAll),
		RUNTIME_CLASS(CBoolButton)
	);
	AddLink
	(
		IDC_HFL_RANGE_RADIO_RANGE,
		_NS_LNK("Range"),
		NULL, &(GetHotFilter()->m_bRange),
		RUNTIME_CLASS(CBoolButton)
	);
}

//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterPickerTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterPickerTile, CHotFilterRangeWithSelectionTile)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotFilterPickerTile, CHotFilterRangeWithSelectionTile)
	ON_EN_VALUE_CHANGED	(IDC_HFL_RANGE_RADIO_SELECTION,			OnRadiobuttonChanged)
	ON_EN_VALUE_CHANGED	(IDC_HFL_RANGE_COMBO,					OnQueryChanged)
	ON_BN_CLICKED		(IDC_HFL_RANGE_BUTTON_OPEN_SELECTION,	OnOpenSelection)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CHotFilterPickerTile::CHotFilterPickerTile()
    :
    CHotFilterRangeWithSelectionTile	(_NS_DLG("ERP.Core.Components.HotFilterPickerTile"), IDD_TD_HOTFILTER_PICKER)
{
	SetMinHeight(ORIGINAL);
}

//-----------------------------------------------------------------------------
void CHotFilterPickerTile::OnQueryChanged()
{
	//if (!m_pHotFilter->m_pQueryParser)
	//	return;

	//m_pHotFilter->m_pQueryParser->Load(m_pHotFilter->m_CurrentQuery);
	GetHotFilter()->NotifyChanged(HFL_ELEMENT_QUERY, m_hWnd);
}

//----------------------------------------------------------------------------
void CHotFilterPickerTile::OnOpenSelection()
{
	GetDocument()->GetNotValidView(TRUE);

	CBaseDocument* pDoc = AfxGetTbCmdManager()->RunDocument(_T("ERP.Core.Components.HotFilterDataPicker"), szDefaultViewMode, FALSE, GetDocument(), (LPAUXINFO)m_pHotFilter);
	
	if (!pDoc)
		return;

	//@@TODO usare i disposing handler
	GetDocument()->GetMasterFrame()->EnableWindow(FALSE);
	AfxGetTbCmdManager()->WaitDocumentEnd(pDoc);
	GetDocument()->GetMasterFrame()->EnableWindow(TRUE);
}

//----------------------------------------------------------------------------
void CHotFilterPickerTile::BuildDataControlLinks()
{
	__super::BuildDataControlLinks();

	AddLink
	(
		IDC_HFL_RANGE_RADIO_SELECTION,
		_NS_LNK("Selection"),
		NULL, &(GetHotFilter()->m_bSelection),
		RUNTIME_CLASS(CBoolButton)
	);

	AddLink(IDC_HFL_RANGE_BUTTON_OPEN_SELECTION, _NS_LNK("OpenSelection"), NULL, &(GetHotFilter()->m_bOpenSelection));

	if (GetHotFilter()->m_bAllowSavingQueries)
	{
		//CQueriesCombo* pQueryCombo = (CQueriesCombo* ) AddLink
		//	(
		//		IDC_HFL_RANGE_COMBO,
		//		_NS_LNK("Query"),
		//		NULL, &(m_pHotFilter->m_CurrentQuery),
		//		RUNTIME_CLASS(CQueriesCombo)
		//	);
		//pQueryCombo->AttachQueryParser(m_pHotFilter->m_pQueryParser);
	}
	else
	{
		CWnd* pWnd = GetDlgItem(IDC_HFL_RANGE_COMBO);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
	}
}

//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterListPopupTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterListPopupTile, CHotFilterTileObj)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotFilterListPopupTile, CHotFilterTileObj)
	ON_BN_CLICKED		(IDC_HFL_ITEMSLIST_BUTTON_OPEN_SELECTION,		OnOpenSelection)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CHotFilterListPopupTile::CHotFilterListPopupTile()
    :
    CHotFilterTileObj	(_NS_DLG("ERP.Core.Components.HotFilterListPopupTile"), IDD_TD_HOTFILTER_LIST_POPUP),
	m_pCItemsListEdit	(NULL)
{
	m_ItemsList.SetAllocSize(255);
}

//-----------------------------------------------------------------------------
void CHotFilterListPopupTile::AttachOwner(CObject* pOwner)
{
	ASSERT(pOwner->IsKindOf(RUNTIME_CLASS(HotFilterList)));

	__super::AttachOwner(pOwner);
}

//----------------------------------------------------------------------------
void CHotFilterListPopupTile::OnOpenSelection()
{
	ASSERT(m_pCItemsListEdit);
	if (!m_pCItemsListEdit)
		return;

	if (m_pCItemsListEdit->DoModal() == IDOK)
		GetHotFilter()->NotifyChanged(HFL_ELEMENT_ITEMLIST, m_hWnd);

	GetDocument()->UpdateDataView();
}

//----------------------------------------------------------------------------
void CHotFilterListPopupTile::BuildDataControlLinks()
{
	m_pCItemsListEdit = (CItemsListEdit*)
	AddLink
	(
		IDC_HFL_ITMLIST_LIST,
		_NS_LNK("ItemsList"),
		NULL, &(m_ItemsList),
		RUNTIME_CLASS(CItemsListEdit)
	);
	m_pCItemsListEdit->Attach(&GetHotFilter()->m_PickedItemsList, GetHotFilter()->m_pHKLItemsList);

	AddLink(IDC_HFL_ITEMSLIST_BUTTON_OPEN_SELECTION, _NS_LNK("OpenSelection"));
}

//-----------------------------------------------------------------------------
void CHotFilterListPopupTile::OnPinUnpin()
{
	m_ItemsList.Clear();
	__super::OnPinUnpin();
}

//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterListListboxTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterListListboxTile, CHotFilterTileObj)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotFilterListListboxTile, CHotFilterTileObj)
	ON_EN_VALUE_CHANGED	(IDC_HFL_ITMLIST_COMBO,					OnItemListChanged)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CHotFilterListListboxTile::CHotFilterListListboxTile()
    :
    CHotFilterTileObj	(_NS_DLG("ERP.Core.Components.HotFilterListListboxTile"), IDD_TD_HOTFILTER_LIST_LISTBOX),
	m_pCItemsListEdit	(NULL)
{
	m_ItemsList.SetAllocSize(255);
}

//-----------------------------------------------------------------------------
void CHotFilterListListboxTile::AttachOwner(CObject* pOwner)
{
	ASSERT(pOwner->IsKindOf(RUNTIME_CLASS(HotFilterList)));

	__super::AttachOwner(pOwner);
}

//----------------------------------------------------------------------------
void CHotFilterListListboxTile::BuildDataControlLinks()
{
	m_pCItemsListEdit = (CItemsListEdit*)
	AddLink
	(
		IDC_HFL_ITMLIST_LIST,
		_NS_LNK("ItemsList"),
		NULL, &(m_ItemsList),
		RUNTIME_CLASS(CItemsListEdit)
	);
	m_pCItemsListEdit->Attach(&GetHotFilter()->m_PickedItemsList, GetHotFilter()->m_pHKLItemsList);

	m_pCItemsListEdit->CreateItemsMSCombo
		(
			_T("ItemsListCombo"), 
			this, 
			IDC_HFL_ITMLIST_COMBO
		);
}

//-----------------------------------------------------------------------------
void CHotFilterListListboxTile::OnPinUnpin()
{
	m_ItemsList.Clear();
	__super::OnPinUnpin();
}

//-----------------------------------------------------------------------------
void CHotFilterListListboxTile::OnItemListChanged()
{
	GetHotFilter()->NotifyChanged(HFL_ELEMENT_ITEMLIST, m_hWnd);
}


//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterListCheckboxTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CHotFilterCheckListboxTile, CHotFilterTileObj)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotFilterCheckListboxTile, CHotFilterTileObj)
	ON_EN_VALUE_CHANGED(IDC_HFL_ITMLIST_COMBO, OnItemListChanged)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CHotFilterCheckListboxTile::CHotFilterCheckListboxTile()
	:
	CHotFilterTileObj		(_NS_DLG("ERP.Core.Components.HotFilterListCheckboxTile"), IDD_TD_HOTFILTER_LIST_CHECKBOX),
	m_pParsedCheckListBox	(NULL)
{
	//m_ItemsList.SetAllocSize(255);
}

//-----------------------------------------------------------------------------
void CHotFilterCheckListboxTile::AttachOwner(CObject* pOwner)
{
	ASSERT(pOwner->IsKindOf(RUNTIME_CLASS(HotFilterList)));

	__super::AttachOwner(pOwner);
}

//----------------------------------------------------------------------------
void CHotFilterCheckListboxTile::BuildDataControlLinks()
{
	m_pParsedCheckListBox = (CParsedCheckListBox*)
		AddLink
		(
			IDC_HOTFILTER_ITEMS_LBX,
			_NS_LNK("ListBox"),
			NULL,
			&GetHotFilter()->m_arItemsList,
			RUNTIME_CLASS(CParsedCheckListBox),
			GetHotFilter()->m_pHKLItemsList
		);
	//m_pCItemsListEdit = (CItemsListEdit*)
	//	AddLink
	//	(
	//		IDC_HFL_ITMLIST_LIST,
	//		_NS_LNK("ItemsList"),
	//		NULL, &(m_ItemsList),
	//		RUNTIME_CLASS(CItemsListEdit)
	//	);
	//m_pParsedCheckListBox->Attach(&GetHotFilter()->m_PickedItemsList, GetHotFilter()->m_pHKLItemsList);

	//m_pCItemsListEdit->CreateItemsMSCombo
	//(
	//	_T("ItemsListCombo"),
	//	this,
	//	IDC_HFL_ITMLIST_COMBO
	//);
}

//-----------------------------------------------------------------------------
void CHotFilterCheckListboxTile::OnPinUnpin()
{
	//m_ItemsList.Clear();
	__super::OnPinUnpin();
}

//-----------------------------------------------------------------------------
void CHotFilterCheckListboxTile::OnItemListChanged()
{
	GetHotFilter()->NotifyChanged(HFL_ELEMENT_ITEMLIST, m_hWnd);
}

