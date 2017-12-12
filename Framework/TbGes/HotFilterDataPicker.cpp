#include "stdafx.h"

#include <TbOleDb\SqlAccessor.h>
#include <TbOleDb\Sqltable.h>

// Descrizione - tenere allineato con #include <Core\CoreLen.h>
#define LEN_DESCRIPTION 32			

// Locals
#include "HotFilter.h"
#include "UIHotFilterDataPicker.h"
#include "DataPickerRecordSet.h"
#include "HotFilterDataPicker.h"

#ifdef _DEBUG
#undef THIS_FILE                                                        
static char THIS_FILE[] = __FILE__;     
#endif                                

static TCHAR szSelectedColName	[]		= _T("Selected");


/////////////////////////////////////////////////////////////////////////////
// 				class HotFilterDataPicker Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HotFilterDataPicker, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(HotFilterDataPicker, CAbstractFormDoc)
	//{{AFX_MSG_MAP(HotFilterDataPicker)		
	ON_COMMAND				(ID_HFLDATAPICKER_EXTRACT_DATA,		OnExtractData)
	ON_UPDATE_COMMAND_UI	(ID_HFLDATAPICKER_EXTRACT_DATA,		OnUpdateExtractData)
	ON_COMMAND				(ID_HFLDATAPICKER_UNDO_EXTRACTION,	OnUndoExtraction)
	ON_UPDATE_COMMAND_UI	(ID_HFLDATAPICKER_UNDO_EXTRACTION,	OnUpdateUndoExtraction)
	ON_COMMAND				(ID_HFLDATAPICKER_COMPLETED,		OnCompleted)
	ON_UPDATE_COMMAND_UI	(ID_HFLDATAPICKER_COMPLETED,		OnEnableCompleted)
	ON_COMMAND				(ID_HFLDATAPICKER_SAVE_QUERY_MENU,	OnSaveQuery)
	ON_COMMAND				(ID_HFLDATAPICKER_SAVE_QUERY,		OnSaveQuery)
	ON_COMMAND				(ID_HFLDATAPICKER_SAVE_QUERY_AS,	OnSaveQueryAs)
	ON_COMMAND				(ID_HFLDATAPICKER_DELETE_QUERY,		OnDeleteQuery)
	ON_UPDATE_COMMAND_UI	(ID_HFLDATAPICKER_SAVE_QUERY_MENU,	OnEnableSaveQueryMenu)
	ON_UPDATE_COMMAND_UI	(ID_HFLDATAPICKER_SAVED_QUERIES,	OnEnableSaveQueryMenu)
	ON_EN_VALUE_CHANGED		(ID_HFLDATAPICKER_SAVED_QUERIES,	OnQueryChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
HotFilterDataPicker::HotFilterDataPicker()
	:
	m_bSelectAll			(FALSE),
	m_pHotFilter			(NULL),
	m_pRecordSet			(NULL),
	m_nLastIDC				(IDC_HFLDATAPICKER_COL_01),
	m_nSelectedRows			(0),
	m_pTBGridControl		(NULL),
	m_pUnpinnedTilesPane	(NULL),
	m_bCanDoExtractData		(TRUE),
	m_pFiltersPanel			(NULL)
{ 
	m_bBatch = TRUE;
	m_pLocalFields = new SqlRecordLocals(_T("HotFilter"));
}

//-----------------------------------------------------------------------------
HotFilterDataPicker::~HotFilterDataPicker() 
{
	SAFE_DELETE	(m_pRecordSet);
	
	m_pLocalFields->RemoveAll();
	SAFE_DELETE(m_pLocalFields);
}

//------------------------------------------------------------------------
BOOL HotFilterDataPicker::OnOpenDocument(LPCTSTR pParam)
{
	if (pParam)
		m_pHotFilter = (HotFilterRange*)GET_AUXINFO(pParam);

	return __super::OnOpenDocument(pParam);
}

//-----------------------------------------------------------------------------
BOOL HotFilterDataPicker::OnAttachData()
{     
	if (m_pHotFilter && !m_pHotFilter->m_strDataPickerFormTitle.IsEmpty())
	{
		SetFormTitle	(m_pHotFilter->m_strDataPickerFormTitle);
		SetFormName		(m_pHotFilter->m_strDataPickerFormTitle);
	}
	else
	{
		SetFormTitle	(_TB("Select Data"));
		SetFormName		(_TB("Select Data"));
	}

	m_pRecordSet = new DataPickerRecordSet(m_pHotFilter->GetRecordClass(), this);

	//@@TODO acceleratori?
	//SetDocAccel		(IDR_HFLDATAPICKER_AUX_ACCELERATOR);

	m_pRecordSet->GetRecord()->AddExtension(m_pLocalFields);

	m_pHotFilter->OnAttachDataPickerData(this);

	//if (m_pHotFilter->m_bAllowSavingQueries)
	//{
	//	m_SavedQueriesCombo.AttachQueryParser(m_pHotFilter->m_pQueryParser);
	//	m_pHotFilter->m_pQueryParser->Load(m_pHotFilter->m_CurrentQuery);
	//}

	m_pHotFilter->InitializeControls();
	
	m_pRecordSet->SetPreselectedItems(m_pHotFilter->m_PickedItemsList, m_pHotFilter->m_UnselectedItemsList);
	m_bSelectAll = m_pHotFilter->m_PickedItemsList.GetSize() == 0;

	return TRUE;
}

//--------------------------------------------------------------------------------------------
void HotFilterDataPicker::OnEnableCompleted(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_nSelectedRows > 0);
}

//----------------------------------------------------------------------------------
DataBool* HotFilterDataPicker::GetSelected(SqlRecord* pRecord)
{
	return (DataBool*)pRecord->GetDataObjFromColumnName((LPCTSTR)szSelectedColName);
}

//-----------------------------------------------------------------------------
void HotFilterDataPicker::GetSelectedItems(DataObjArray& selectedItems, DataObjArray& unselectedItems)
{
	m_pRecordSet->GetSelectedItems(selectedItems, unselectedItems);
}

//-----------------------------------------------------------------------------
CRuntimeClass* DefaultCtrlClass(DataType aType)
{
	if (aType == DataType::String)			return RUNTIME_CLASS(CStrStatic);
	if (aType == DataType::Integer)			return RUNTIME_CLASS(CIntStatic);
	if (aType == DataType::Long)			return RUNTIME_CLASS(CLongStatic);
	if (aType == DataType::Double)			return RUNTIME_CLASS(CDoubleStatic);
	if (aType == DataType::Money)			return RUNTIME_CLASS(CMoneyStatic);
	if (aType == DataType::Quantity)		return RUNTIME_CLASS(CQuantityStatic);
	if (aType == DataType::Percent)			return RUNTIME_CLASS(CPercStatic);
	if (aType == DataType::Date)			return RUNTIME_CLASS(CDateStatic);
	if (aType == DataType::DateTime)		return RUNTIME_CLASS(CDateStatic);
	if (aType == DataType::Time)			return RUNTIME_CLASS(CTimeStatic);
	if (aType == DataType::ElapsedTime)		return RUNTIME_CLASS(CElapsedTimeStatic);
	if (aType == DataType::Bool)			return RUNTIME_CLASS(CBoolStatic);
	if (aType == DataType::Enum)			return RUNTIME_CLASS(CEnumStatic);
	if (aType == DataType::Guid)			return RUNTIME_CLASS(CGuidStatic);
	if (aType == DataType::Text)			return RUNTIME_CLASS(CTextStatic);

	ASSERT_TRACE(FALSE, "Unsupported type for grids");
	return NULL;
}

//-----------------------------------------------------------------------------
CTBGridColumnInfo* HotFilterDataPicker::AddColumn
	(
		const	CString&		sColumnName,
		const	CString&		sColumnTitle,
				DataObj*		pDataObj,
				CRuntimeClass*	pHKLClass, /*= NULL*/
				BOOL			bWithDescription /*= FALSE*/
	)
{
	ASSERT_TRACE(m_pTBGridControl, "This HotFilter has been configured to use BodyEdit, not TBGridControl");

	CTBGridColumnInfo* pInfo = m_pTBGridControl->AddColumn
		(
			sColumnName,
			pDataObj,
			sColumnTitle,
			0,
			pHKLClass
		);
	if (bWithDescription)
	{
		ASSERT_TRACE(pHKLClass, "Automatic description requires the HKL class");
		CTBGridColumnInfo* pDescri = AddColumn
			(
				sColumnName + _T("Description"),
				_T(""),
				DataType::String,
				LEN_DESCRIPTION
			);
		pDescri->AttachMasterColumn(pInfo);
	}
	return pInfo;
}

//-----------------------------------------------------------------------------
CTBGridColumnInfo* HotFilterDataPicker::AddColumn
	(
		const	CString&		sColumnName,
		const	CString&		sColumnTitle,
				DataType		aType,
				int				nLen
	)
{
	ASSERT_TRACE(m_pTBGridControl, "This HotFilter has been configured to use BodyEdit, not TBGridControl");

	SqlRecordItem* pItem = m_pLocalFields->GetItemByColumnName(sColumnName);
	if (!pItem)
		pItem = m_pLocalFields->AddLocalField(aType, sColumnName, nLen);

	return m_pTBGridControl->AddColumn
		(
			sColumnName, 
			pItem->GetDataObj(), 
			sColumnTitle,
			nLen
		);
}

//-----------------------------------------------------------------------------
void HotFilterDataPicker::CreateGridLayout()
{
	AddColumn
	(
		szSelectedColName,
		_TB("Sel."), 
		DataType::Bool, 
		0
	);
	m_pHotFilter->CustomizeDataPicker(m_pRecordSet->GetRecord(), this);
	m_pTBGridControl->CreateGridLayout();
}

//-----------------------------------------------------------------------------
void HotFilterDataPicker::PopulateGrid(BOOL bPreselect)
{
	m_pRecordSet->Execute(bPreselect);

	if (!bPreselect && m_pHotFilter->m_PickedItemsList.GetSize() > 0)
	{
		m_pRecordSet->PreselectItems(m_pHotFilter->m_PickedItemsList, m_pHotFilter->m_UnselectedItemsList);
		m_nSelectedRows = m_pHotFilter->m_PickedItemsList.GetSize();
	}
	else
		m_nSelectedRows = m_pRecordSet->GetSize() - m_pHotFilter->m_UnselectedItemsList.GetSize();
}

//-----------------------------------------------------------------------------
void HotFilterDataPicker::ClearGrid()
{
	if (m_pRecordSet->GetSize() == 0)
		return;

	m_pRecordSet->ClearGrid();
	m_nSelectedRows = 0;
}

//-----------------------------------------------------------------------------------------------
void HotFilterDataPicker::OnUpdateExtractData(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_bCanDoExtractData);
}

//-----------------------------------------------------------------------------
void HotFilterDataPicker::OnExtractData()
{  
	m_bSelectAll = TRUE;
	PopulateGrid(TRUE);

	//disable itself
	m_bCanDoExtractData = FALSE;

	SetFiltersEnabled(FALSE);
	SetFiltersCollapsed(TRUE);

	UpdateDataView();
}

//-----------------------------------------------------------------------------------------------
void HotFilterDataPicker::OnUndoExtraction()
{
	m_bCanDoExtractData = TRUE;
	m_bSelectAll = FALSE;
	ClearGrid();

	SetFiltersEnabled(TRUE);
	SetFiltersCollapsed(FALSE);

	UpdateDataView();
}

//-----------------------------------------------------------------------------------------------
void HotFilterDataPicker::OnUpdateUndoExtraction(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bCanDoExtractData);
}

//---------------------------------------------------------
void HotFilterDataPicker::OnCompleted()
{
	GetNotValidView(TRUE);

	if (m_pHotFilter)
		m_pHotFilter->DataPickerCompleted(this);

	GetMasterFrame()->PostMessage(WM_CLOSE);
}

//---------------------------------------------------------
void HotFilterDataPicker::DisableControlsForBatch()
{
	__super::DisableControlsForBatch();

	//for (int d = 0; d <= m_FilterTiles.GetUpperBound(); d++)
	//{
	//	if (!m_FilterTiles[d]->IsKindOf(RUNTIME_CLASS(CHotFilterRangeTile)))
	//		continue;
	//	((CHotFilterRangeTile*)m_FilterTiles[d])->OnDisableControlsForBatch();
	//}
}

//---------------------------------------------------------------------------------------------------
void HotFilterDataPicker::SetFiltersEnabled(BOOL bEnable)
{
	for (int d = 0; d <= m_FilterTiles.GetUpperBound(); d++)
	{
		m_FilterTiles[d]->Enable(bEnable);
		((CTileDialog*)m_FilterTiles[d])->EnableTileDialogControlLinks(bEnable);
	}
}

//-------------------------------------------------------------------------------------------------
void HotFilterDataPicker::SetFiltersCollapsed(BOOL bSet)
{
	if (!m_pFiltersPanel)
		return;

	m_pFiltersPanel->SetCollapsed(bSet);
}

//---------------------------------------------------------
void HotFilterDataPicker::RefreshQueryList(BOOL bReset /*= FALSE*/)
{
	//CWnd*	pCtrl	= GetWndCtrl(ID_HFLDATAPICKER_SAVED_QUERIES);
	////if (pCtrl && pCtrl->m_hWnd)
	////{
	//	if (bReset)
	//	{
	//		m_SavedQueriesCombo.SetNotResetAssociations(FALSE);
	//		m_SavedQueriesCombo.ResetAssociations();
	//		m_SavedQueriesCombo.SetNotResetAssociations();
	//	}
	//	m_SavedQueriesCombo.FillListBox(); 
	//	m_SavedQueriesCombo.UpdateCtrlView();
	////}
}

//---------------------------------------------------------
void HotFilterDataPicker::OnSaveQuery()
{
	//if (!m_pHotFilter->m_pQueryParser)
	//	return;

	//GetNotValidView(TRUE);
	//m_pHotFilter->m_pQueryParser->Save(GetMasterFrame(), m_pHotFilter->m_CurrentQuery);
	//RefreshQueryList();
}

//---------------------------------------------------------
void HotFilterDataPicker::OnSaveQueryAs()
{
	//if (!m_pHotFilter->m_pQueryParser)
	//	return;

	//GetNotValidView(TRUE);
	//m_pHotFilter->m_pQueryParser->SaveAs(GetMasterFrame(), m_pHotFilter->m_CurrentQuery);
	//RefreshQueryList();
}

//---------------------------------------------------------
void HotFilterDataPicker::OnDeleteQuery()
{
	//if (!m_pHotFilter->m_pQueryParser)
	//	return;

	//GetNotValidView(TRUE);
	//m_pHotFilter->m_pQueryParser->Delete(m_pHotFilter->m_CurrentQuery);
	//RefreshQueryList(TRUE);
}

//-----------------------------------------------------------------------------------
void HotFilterDataPicker::OnEnableSaveQueryMenu(CCmdUI* pCmdUI)
{
	//pCmdUI->Enable(m_pHotFilter->m_pQueryParser && m_bGridLocked);
}

//---------------------------------------------------------
void HotFilterDataPicker::OnQueryChanged()
{
	//if (!m_pHotFilter->m_pQueryParser)
	//	return;

	//m_pHotFilter->m_pQueryParser->Load(m_pHotFilter->m_CurrentQuery);
	//m_pHotFilter->InitializeControls();

	//ClearGrid();
	//if (m_pParsedPanel)
	//	m_pParsedPanel->OnDisableControlsForBatch();
	//UpdateDataView();
}

//-----------------------------------------------------------------------------
BOOL HotFilterDataPicker::OnToolbarDropDown(UINT nID, CMenu& aMenu)
{
	//if (nID != ID_HFLDATAPICKER_SAVE_QUERY_MENU)
	//	return FALSE;

	//aMenu.AppendMenu(MF_STRING,	ID_HFLDATAPICKER_SAVE_QUERY,	_TB("Save"));
	//aMenu.AppendMenu(MF_STRING,	ID_HFLDATAPICKER_SAVE_QUERY_AS,	_TB("Save as ..."));
	//aMenu.AppendMenu(MF_STRING,	ID_HFLDATAPICKER_DELETE_QUERY,	_TB("Delete"));

	return TRUE;
}

