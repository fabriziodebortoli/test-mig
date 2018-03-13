#include "stdafx.h"

#include <TbOleDb\SqlAccessor.h>
#include <TbOleDb\SqlTable.h>

#include "UIHotFilterDataPicker.h"
#include "HotFilter.h"
#include "HotFilterManager.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

static TCHAR szParamFrom[] = _T("From");
static TCHAR szParamTo[] = _T("To");
static TCHAR szParamItm[] = _T("Itm");
static TCHAR szSettingsPicked[] = _T("_Picked");


//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterObj		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(HotFilterObj, CCmdTarget)

//-----------------------------------------------------------------------------
HotFilterObj::HotFilterObj(EHotFilterType type, CAbstractFormDoc* pDocument, HotFilterManager* pHotFilterManager, int nNotificationIDC /*= 0*/)
	:
	IDisposingSourceImpl(this),
	m_eHFType(type),
	m_pDocument(pDocument),
	m_pHotFilterManager(pHotFilterManager),
	m_pParentHFL(NULL),
	m_bSetSelected(FALSE),
	m_nNotificationIDC(nNotificationIDC),
	m_nLastAction(0),
	m_bMustExistData(FALSE),
	m_bEnableAddOnFly(FALSE),
	m_bIsReadOnly	(FALSE)
{
}

//-----------------------------------------------------------------------------
HotFilterObj::~HotFilterObj()
{
	m_PickedItemsList.RemoveAll();
	m_SelectedObj.RemoveAll();
}

//-----------------------------------------------------------------------------
void HotFilterObj::AddVar(const CString& sVarName, DataObj& aVar)
{
	ASSERT_VALID(this->m_pDocument);
	ASSERT(!this->m_strName.IsEmpty());

	CString s = this->m_strName + '_' + sVarName;
	m_pDocument->DeclareVariable(s, aVar);
}

//-----------------------------------------------------------------------------
void HotFilterObj::AttachNsHotlink(const HotLinkInfo* hklInfo)
{
	m_sNsHotlink = hklInfo->m_strNamespace;
	if (hklInfo->m_bEnableAddOnFly != B_UNDEFINED)
		m_bEnableAddOnFly = hklInfo->m_bEnableAddOnFly == B_TRUE ? TRUE : FALSE;

	if (hklInfo->m_bMustExistData != B_UNDEFINED)
		m_bMustExistData = hklInfo->m_bMustExistData == B_TRUE ? TRUE : FALSE;

	CreateHotlinks();
}

//-----------------------------------------------------------------------------
void HotFilterObj::InitItemsList(const DataObj& aValue)
{
	m_PickedItemsList.RemoveAll();
	m_PickedItemsList.Add(aValue.Clone());
	if (m_bSetSelected)
	{
		m_SelectedObj.RemoveAll();
		m_SelectedObj.Add(aValue.Clone());
	}

	OnInitItemsList();
}

//-----------------------------------------------------------------------------
void HotFilterObj::InitItemsList(const DataObjArray& aArrayValue)
{
	m_PickedItemsList.RemoveAll();
	m_PickedItemsList.Append(aArrayValue);
	if (m_bSetSelected)
	{
		m_SelectedObj.RemoveAll();
		m_SelectedObj.Append(aArrayValue);
	}
	OnInitItemsList();
}

//-----------------------------------------------------------------------------
const CTBNamespace& HotFilterObj::GetNamespace() const
{
	ASSERT_TRACE(this->m_Namespace.IsValid(), "GetNamespace HotFilterObj fails");
	return m_Namespace;
}

//-----------------------------------------------------------------------------
CString HotFilterObj::GetName()	const
{
	ASSERT(!m_strName.IsEmpty()); return m_strName;
}

//-----------------------------------------------------------------------------
void HotFilterObj::SetName(const CString& sName)
{
	m_strName = sName;
	if (m_pDocument)
		m_Namespace.AutoCompleteNamespace(CTBNamespace::HOTFILTER, sName, m_pDocument->GetNamespace());
}

//-----------------------------------------------------------------------------
void HotFilterObj::SetType(EHotFilterType type)
{
	//TODO aggiungere controlli di congruenza fra runtimeclass ed enumtype
	m_eHFType = type;
}

//-----------------------------------------------------------------------------
void HotFilterObj::DefineQuerySelectionPicked
(
	SqlTable*	pTable,
	SqlRecord*	pRec,
	const	DataObj&	aColumn,
	CString		sOperator
)
{
	PreparePickedItemsList();

	if (m_PickedItemsList.GetSize() == 0)
		return; // void items list = all

	TRY
	{
		pRec->SetQualifier();

	//@@TODO ottimizzare in caso di 1 solo item selezionato

	if (!pTable->m_strFilter.IsEmpty())
		pTable->m_strFilter += _T(" ") + sOperator + _T(" ");
	pTable->m_strFilter += cwsprintf(_T("%s IN ("), pRec->GetQualifiedColumnName(&aColumn));

	for (int i = 0; i <= m_PickedItemsList.GetUpperBound(); i++)
	{
		pTable->m_strFilter += _T("?") + CString(i < m_PickedItemsList.GetUpperBound() ? _T(",") : _T(""));
		pTable->AddParam(cwsprintf(_T("%s%s%d"), GetName(), szParamItm, i), aColumn);
	}

	pTable->m_strFilter += _T(")");
	}
		CATCH(SqlException, e)
	{
		TCHAR msg[255];
		e->GetErrorMessage(msg, 255);
		ASSERT_TRACE1(FALSE, "Errors defining the HotFilter query:\n%s", msg);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void HotFilterObj::PrepareQuerySelectionPicked
(
	SqlTable*	pTable
)
{
	if (m_PickedItemsList.GetSize() == 0)
		return; // void items list = all

	TRY
	{
		//@@TODO ottimizzare in caso di 1 solo item selezionato
		for (int i = 0; i <= m_PickedItemsList.GetUpperBound(); i++)
		{
			DataObj* a = m_PickedItemsList.GetAt(i);
			pTable->SetParamValue(cwsprintf(_T("%s%s%d"), GetName(), szParamItm, i),*m_PickedItemsList.GetAt(i));
		}
	}
		CATCH(SqlException, e)
	{
		TCHAR msg[255];
		e->GetErrorMessage(msg, 255);
		ASSERT_TRACE1(FALSE, "Errors defining the HotFilter query:\n%s", msg);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void HotFilterObj::GetSettingsValues(CParameterInfo& aSettings)
{
	ASSERT_TRACE(GetDataObjClass() && GetDataObjClass()->IsDerivedFrom(RUNTIME_CLASS(DataObj)), "To automatic read/store settings the DataObj runtime class must be defined");
	if (!GetDataObjClass() && GetDataObjClass()->IsDerivedFrom(RUNTIME_CLASS(DataObj)))
		return;

	DataStr pickedItems = *(DataStr*)aSettings.GetSettingValue(GetInstanceName() + szSettingsPicked, DataStr(_T("")));

	m_PickedItemsList.RemoveAll();
	if (!pickedItems.IsEmpty())
	{
		int nCurrPos = 0;
		CString sItem = pickedItems.GetString().Tokenize(_T(";"), nCurrPos);
		while (sItem != "")
		{
			DataObj* pItem = (DataObj*)GetDataObjClass()->CreateObject();
			pItem->Assign(sItem);
			m_PickedItemsList.Add(pItem);
			sItem = pickedItems.GetString().Tokenize(_T(";"), nCurrPos);
		}
	}
}

//-----------------------------------------------------------------------------
void HotFilterObj::SetSettingsValues(CParameterInfo& aSettings)
{
	DataStr pickedItems;

	for (int i = 0; i <= m_PickedItemsList.GetUpperBound(); i++)
		pickedItems += (i == 0 ? _T("") : _T(";")) + m_PickedItemsList.GetAt(i)->FormatData();

	aSettings.SetSettingValue(GetInstanceName() + szSettingsPicked, pickedItems);
}

//-----------------------------------------------------------------------------
const DataStr& HotFilterObj::GetInstanceName()
{
	if (m_InstanceName.IsEmpty())
		m_InstanceName = (m_pParentHFL ? m_pParentHFL->GetInstanceName() + _T("_") : _T("")) + CString(GetRuntimeClass()->m_lpszClassName);

	return m_InstanceName;
}

//-----------------------------------------------------------------------------
void HotFilterObj::SetInstanceName(const DataStr& aPrefix)
{
	m_InstanceName = (m_pParentHFL ? m_pParentHFL->GetInstanceName() + _T("_") : _T(""));
	if (aPrefix.IsEmpty())
		m_InstanceName = m_InstanceName + CString(GetRuntimeClass()->m_lpszClassName);
	else
		m_InstanceName = m_InstanceName + aPrefix;
}
//-----------------------------------------------------------------------------
void HotFilterObj::ResetCriteria()
{
	OnResetCriteria();

	ManageReadOnly();
}

//-----------------------------------------------------------------------------
void HotFilterObj::InitializeHotFilter()
{
	if (GetDocument())
		GetDocument()->InitializeHotFilter(this);
}

//-----------------------------------------------------------------------------
void HotFilterObj::Customize()
{
	ASSERT_VALID(this);
	ASSERT_VALID(this->m_pDocument);
}

//-----------------------------------------------------------------------------
void HotFilterObj::SetNotificationIDC(UINT nIDC)
{
	m_nNotificationIDC = nIDC;
	m_sNotificationIDC = AfxGetTBResourcesMap()->DecodeID(TbResourceType::TbResources, nIDC).m_strName;
	if (m_sNotificationIDC.IsEmpty())
		m_sNotificationIDC = AfxGetTBResourcesMap()->DecodeID(TbResourceType::TbControls, nIDC).m_strName;
	ASSERT(!m_sNotificationIDC.IsEmpty());
}


//-----------------------------------------------------------------------------
void HotFilterObj::NotifyChanged(UINT nAction, HWND hwnd)
{
	if (m_nNotificationIDC == 0)
		return;

	m_nLastAction = nAction;

	GetDocument()->POST_WM_COMMAND(m_nNotificationIDC, EN_VALUE_CHANGED, hwnd)
}

//-----------------------------------------------------------------------------
BOOL HotFilterObj::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	CJsonResource res = AfxGetTBResourcesMap()->DecodeID(TbResourceType::TbControls, nID);
	if (res.m_strName.Find(m_sNotificationIDC) == 0)
	{
		CString sIdc = res.m_strName.Mid(m_sNotificationIDC.GetLength() + 1);
		__super::OnCmdMsg(AfxGetTBResourcesMap()->GetTbResourceID(sIdc, TbResourceType::TbControls), nCode, pExtra, pHandlerInfo);
	}
	return __super::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
}

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterRange		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HotFilterRange, HotFilterObj)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(HotFilterRange, HotFilterObj)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_RADIO_ALL, ManageReadOnly)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_RADIO_RANGE, ManageReadOnly)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_EDIT_FROM, OnFromChanged)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_EDIT_TO, OnToChanged)
END_MESSAGE_MAP()
//-----------------------------------------------------------------------------
HotFilterRange::HotFilterRange
(
	EHotFilterType type, CAbstractFormDoc*	pDocument, HotFilterManager* pHotFilterManager,
	CRuntimeClass*		pHKLClass,
	int					nNotificationIDC /*= 0*/
)
	:
	HotFilterObj(type, pDocument, pHotFilterManager, nNotificationIDC),
	m_bAll(TRUE),
	m_bAllowSavingQueries(FALSE),
	m_bOpenSelection(FALSE),
	m_bRange(FALSE),
	m_bSelection(FALSE),
	m_nHKLLinkedColumnIdx(-1),
	m_pHKLRangeFrom(NULL),
	m_pHKLRangeTo(NULL),
	//m_pQueryParser			(NULL),
	m_pRangeCtrlClass(RUNTIME_CLASS(CStrEdit)),
	m_Style(PICKER)
{
	m_RangeFrom.SetUpperCase();
	m_RangeTo.SetUpperCase();

	//m_CurrentQuery = PredefinedQuery::Default();
}

//-----------------------------------------------------------------------------
HotFilterRange::HotFilterRange()
	:
	HotFilterObj(EHotFilterType::HF_RANGE_SIMPLE, NULL, NULL),
	m_bAll(TRUE),
	m_bAllowSavingQueries(FALSE),
	m_bOpenSelection(FALSE),
	m_bRange(FALSE),
	m_bSelection(FALSE),
	m_nHKLLinkedColumnIdx(-1),
	m_pHKLRangeFrom(NULL),
	m_pHKLRangeTo(NULL),
	//m_pQueryParser			(NULL),
	m_pRangeCtrlClass(RUNTIME_CLASS(CStrEdit)),
	m_Style(PICKER)
{
	m_RangeFrom.SetUpperCase();
	m_RangeTo.SetUpperCase();

	//m_CurrentQuery = PredefinedQuery::Default();
}

//-----------------------------------------------------------------------------
HotFilterRange::~HotFilterRange()
{
	for (int i = 0; i <= m_DataPickerHotFilters.GetUpperBound(); i++)
		SAFE_DELETE(m_DataPickerHotFilters.GetAt(i));
	m_DataPickerHotFilters.RemoveAll();

	m_UnselectedItemsList.RemoveAll();

	for (int i = 0; i <= m_RangeColumns.GetUpperBound(); i++)
		SAFE_DELETE(m_RangeColumns.GetAt(i));
	m_RangeColumns.RemoveAll();
}

//-----------------------------------------------------------------------------
void HotFilterRange::OnFromChanged()
{
	if (m_RangeTo < m_RangeFrom)
		m_RangeTo = m_RangeFrom;

	NotifyChanged(IDC_HFL_RANGE_EDIT_FROM, GetDocument()->GetMasterFrame()->m_hWnd);

	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void HotFilterRange::OnToChanged()
{
	if (m_RangeTo < m_RangeFrom)
		m_RangeFrom = m_RangeTo;
	NotifyChanged(IDC_HFL_RANGE_EDIT_TO, GetDocument()->GetMasterFrame()->m_hWnd);
}
//-----------------------------------------------------------------------------
void HotFilterRange::Customize()
{
	__super::Customize();

	AddVar(L"All", m_bAll);
	AddVar(L"Range", m_bRange);
	AddVar(L"From", m_RangeFrom);
	AddVar(L"To", m_RangeTo);
}

//-----------------------------------------------------------------------------
void HotFilterRange::SetStyle(Style aStyle)
{
	m_Style = aStyle;

	if (m_Style == SIMPLE)
	{
		m_bAll = FALSE;
		m_bRange = TRUE;
	}
}

//-----------------------------------------------------------------------------
void HotFilterRange::AllowSavingQueries(const DataBool& bAllow/* = TRUE*/)
{
	//if (bAllow && !m_pQueryParser)
	//	m_pQueryParser = new HotFilterQueryParser(this);

	m_bAllowSavingQueries = bAllow;
}

//-----------------------------------------------------------------------------
CRuntimeClass* HotFilterRange::GetRecordClass() const
{
	ASSERT_TRACE(m_pHKLRangeFrom, "GetRecordClass called prior the Hotlinks are defined");
	return m_pHKLRangeFrom->GetAttachedRecord()->GetRuntimeClass();
}

//-----------------------------------------------------------------------------
CRuntimeClass* HotFilterRange::GetDataObjClass() const
{
	ASSERT_TRACE(m_pHKLRangeFrom, "GetDataObjClass called prior the Hotlinks are defined");
	return m_pHKLRangeFrom->GetDataObj()->GetRuntimeClass();
}

//-----------------------------------------------------------------------------
void HotFilterRange::CreateHotlinks()
{
	CTBNamespace nsHkl(CTBNamespace::NSObjectType::HOTLINK, m_sNsHotlink);
	if (!nsHkl.IsValid())
	{
		return;
	}
	CString sName = this->m_strName + _T("_HKLFrom");
	HotKeyLink* pHL = m_pDocument->GetHotLink(sName, nsHkl);
	if (!pHL)
	{
		return;
	}
	pHL->EnableAutoFind(FALSE);

	m_pHKLRangeFrom = pHL;
	m_pHKLRangeFrom->MustExistData(m_bMustExistData);
	m_pHKLRangeFrom->EnableAddOnFly(m_bEnableAddOnFly);


	sName = this->m_strName + _T("_HKLTo");
	pHL = m_pDocument->GetHotLink(sName, nsHkl);
	if (!pHL)
	{
		return;
	}
	pHL->EnableAutoFind(FALSE);

	m_pHKLRangeTo = pHL;
	m_pHKLRangeTo->MustExistData(m_bMustExistData);
	m_pHKLRangeTo->EnableAddOnFly(m_bEnableAddOnFly);

	m_nHKLLinkedColumnIdx = m_pHKLRangeFrom->GetDbFieldRecIndex(m_pHKLRangeFrom->GetAttachedRecord());
	ASSERT_TRACE(m_nHKLLinkedColumnIdx != -1, "Linked column not found in Hotlink record!");
}

//-----------------------------------------------------------------------------
void HotFilterRange::OnResetCriteria()
{
	if (m_Style == SIMPLE)
	{
		m_bAll = FALSE;
		m_bRange = TRUE;
	}
	else
	{
		m_bAll = TRUE;
		m_bRange = FALSE;
	}
	m_bSelection = FALSE;
	m_RangeFrom.Clear();
	m_RangeTo.Clear();

	m_PickedItemsList.RemoveAll();
	m_UnselectedItemsList.RemoveAll();
}

//-----------------------------------------------------------------------------
void HotFilterRange::ManageReadOnly()
{
	if (!m_bRange)
	{
		m_RangeFrom.Clear();
		m_RangeTo.Clear();
	}

	m_bAll.     SetAlwaysReadOnly(m_bIsReadOnly);
	m_bRange.   SetAlwaysReadOnly(m_bIsReadOnly);
	m_RangeFrom.SetAlwaysReadOnly(!m_bRange || m_bIsReadOnly);
	m_RangeTo.  SetAlwaysReadOnly(!m_bRange || m_bIsReadOnly);

	m_bOpenSelection = m_bSelection;
	m_CurrentQuery.SetAlwaysReadOnly(!m_bSelection);

	// if the radio changes and "selection" is no more selected, reset the datapicker filter criteria
	if (!m_bSelection)
	{
		ResetPickerCriteria();
		m_PickedItemsList.RemoveAll();
		m_UnselectedItemsList.RemoveAll();
	}
	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
BOOL HotFilterRange::CheckData()
{
	if (m_bRange && (m_RangeFrom.IsEmpty() || m_RangeTo.IsEmpty()))
	{
		m_pDocument->m_pMessages->Add(_TB("Incomplete range value"));
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void HotFilterRange::OnInitItemsList()
{
	m_bAll = m_PickedItemsList.IsEmpty() && m_UnselectedItemsList.IsEmpty();
	m_bSelection = !m_bAll;
	m_bRange = FALSE;
}

//-----------------------------------------------------------------------------
void HotFilterRange::DataPickerCompleted(HotFilterDataPicker* pDataPicker)
{
	pDataPicker->GetSelectedItems(m_PickedItemsList, m_UnselectedItemsList);
	OnDataPickerCompleted(pDataPicker);
	//NotifyChanged(HFL_ELEMENT_ITEMLIST, GetDocument()->GetMasterFrame()->m_hWnd);
}

//-----------------------------------------------------------------------------
BOOL HotFilterRange::IsEmptyQuery()
{
	if (m_bAll)
		return TRUE;

	if (m_bRange)
		return	m_RangeFrom.IsEmpty() && m_RangeTo.IsEmpty() ||
		m_RangeFrom == CParsedCtrl::Strings::FIRST() && m_RangeTo == CParsedCtrl::Strings::LAST();

	if (m_bSelection)
	{
		if (m_PickedItemsList.IsEmpty() && m_UnselectedItemsList.IsEmpty())
			return IsEmptyDataPickerSelectionQuery();
		else
			return FALSE;
	}

	ASSERT_TRACE(FALSE, "Undefined status for the HotFilter query!");
	return TRUE;
}

//-----------------------------------------------------------------------------
void HotFilterRange::DefineQuery
(
	SqlTable*	pTable,
	SqlRecord*	pRec,
	const	DataObj&	aColumn,
	CString		sOperator
)
{
	if (m_bAll)
		return;

	else if (m_bRange)
		DefineQueryRange(pTable, pRec, aColumn);

	else if (m_bSelection)
	{
		// empty picked list = all those extracted according to params of data picker panel, minus those 
		// of the unselected list, if any
		if (m_PickedItemsList.IsEmpty())
			DefineQuerySelectionByParams(pTable, pRec, aColumn, sOperator);
		else
			DefineQuerySelectionPicked(pTable, pRec, aColumn, sOperator);
	}
	else
	{
		ASSERT_TRACE(FALSE, "Unable to define the HotFilter query!");
	}
}

//-----------------------------------------------------------------------------
void HotFilterRange::DefineQueryRange
(
	SqlTable*	pTable,
	SqlRecord*	pRec,
	const	DataObj&	aColumn
)
{
	if (
		m_RangeFrom.IsEmpty() && m_RangeTo.IsEmpty() ||
		m_RangeFrom == CParsedCtrl::Strings::FIRST() && m_RangeTo == CParsedCtrl::Strings::LAST()
		)
		return; // range is void

	TRY
	{
		pRec->SetQualifier();
	//@@TODO GESTIRE CASI TIPO: xxx - ULTIMO o PRIMO - xxx
	pTable->AddBetweenColumn(pRec, aColumn);
	pTable->AddParam(CString(GetName() + szParamFrom),	aColumn);
	pTable->AddParam(CString(GetName() + szParamTo),	aColumn);
	}
		CATCH(SqlException, e)
	{
		TCHAR msg[255];
		e->GetErrorMessage(msg, 255);
		ASSERT_TRACE1(FALSE, "Errors defining the HotFilter query:\n%s", msg);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void HotFilterRange::DefineQuerySelectionByParams
(
	SqlTable*	pTable,
	SqlRecord*	pRec,
	const	DataObj&	aColumn,
	CString		sOperator
)
{
	if (IsEmptyDataPickerSelectionQuery() && m_UnselectedItemsList.IsEmpty())
		return;

	SqlRecord* pTblRec = (SqlRecord*)GetRecordClass()->CreateObject();
	pTblRec->SetQualifier();

	SqlTable aTbl(pTblRec, GetDocument()->GetReadOnlySqlSession());

	OnDefineDataPickerParamsQuery(GetInstanceName(), &aTbl, pTblRec);

	aTbl.ClearColumns();
	aTbl.Select(pTblRec->GetDataObjAt(m_nHKLLinkedColumnIdx));
	aTbl.BuildSelect();

	TRY
	{
		pRec->SetQualifier();

		if (!IsEmptyDataPickerSelectionQuery())
		{
			if (!pTable->m_strFilter.IsEmpty())
				pTable->m_strFilter += _T(" ") + sOperator + _T(" ");

			pTable->m_strFilter += cwsprintf(_T("%s IN (%s)"), pRec->GetQualifiedColumnName(&aColumn), aTbl.m_strSQL);

			for (int i = 0; i <= aTbl.m_pParamArray->GetUpperBound(); i++)
			{
				CString strParamName = aTbl.m_pParamArray->GetParamName(i);
				DataObj* pDataObj = aTbl.m_pParamArray->GetDataObjAt(i);
				pTable->AddParam(strParamName, *pDataObj);
			}
		}
		if (!m_UnselectedItemsList.IsEmpty())
		{
			if (!pTable->m_strFilter.IsEmpty())
				pTable->m_strFilter += _T(" ") + sOperator + _T(" ");

			pTable->m_strFilter += cwsprintf(_T("%s NOT IN ("), pRec->GetQualifiedColumnName(&aColumn));

			for (int i = 0; i <= m_UnselectedItemsList.GetUpperBound(); i++)
			{
				pTable->m_strFilter += _T("?") + CString(i < m_UnselectedItemsList.GetUpperBound() ? _T(",") : _T(""));
				pTable->AddParam(cwsprintf(_T("%s%s%d"), GetInstanceName().FormatData(), szParamItm, i), aColumn);
			}

			pTable->m_strFilter += _T(")");
		}
	}
		CATCH(SqlException, e)
	{
		TCHAR msg[255];
		e->GetErrorMessage(msg, 255);
		ASSERT_TRACE1(FALSE, "Errors defining the HotFilter query:\n%s", msg);
	}
	END_CATCH

		delete pTblRec;
}

//-----------------------------------------------------------------------------
void HotFilterRange::PrepareQuery
(
	SqlTable*	pTable
)
{
	if (m_bAll)
		return;

	else if (m_bRange)
		PrepareQueryRange(pTable);

	else if (m_bSelection)
	{
		// empty list = all those extracted according to params of data picker panel, minus those 
		// of the unselected list, if any
		if (m_PickedItemsList.IsEmpty())
			PrepareQuerySelectionByParams(pTable);
		else
			PrepareQuerySelectionPicked(pTable);
	}
	else
	{
		ASSERT_TRACE(FALSE, "Unable to prepare the HotFilter query!");
	}
}

//-----------------------------------------------------------------------------
void HotFilterRange::PrepareQueryRange
(
	SqlTable*	pTable
)
{
	if (
		m_RangeFrom.IsEmpty() && m_RangeTo.IsEmpty() ||
		m_RangeFrom == CParsedCtrl::Strings::FIRST() && m_RangeTo == CParsedCtrl::Strings::LAST()
		)
		return; // range is void

	TRY
	{
		pTable->SetParamValue(CString(GetName() + szParamFrom),	m_RangeFrom);
		pTable->SetParamValue(CString(GetName() + szParamTo),	m_RangeTo);
	}
		CATCH(SqlException, e)
	{
		TCHAR msg[255];
		e->GetErrorMessage(msg, 255);
		ASSERT_TRACE1(FALSE, "Errors preparing the HotFilter query:\n%s", msg);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void HotFilterRange::PrepareQuerySelectionByParams
(
	SqlTable*	pTable
)
{
	if (!IsEmptyDataPickerSelectionQuery())
		OnPrepareDataPickerParamsQuery(GetInstanceName(), pTable);

	if (!m_UnselectedItemsList.IsEmpty())
	{
		TRY
		{
			//@@TODO ottimizzare in caso di 1 solo item deselezionato
			for (int i = 0; i <= m_UnselectedItemsList.GetUpperBound(); i++)
			{
				DataObj* a = m_UnselectedItemsList.GetAt(i);
				pTable->SetParamValue(cwsprintf(_T("%s%s%d"), GetName(), szParamItm, i),*m_UnselectedItemsList.GetAt(i));
			}
		}
			CATCH(SqlException, e)
		{
			TCHAR msg[255];
			e->GetErrorMessage(msg, 255);
			ASSERT_TRACE1(FALSE, "Errors defining the HotFilter query:\n%s", msg);
		}
		END_CATCH
	}
}

//-----------------------------------------------------------------------------
void HotFilterRange::GetSettingsValues(CParameterInfo& aSettings)
{
	__super::GetSettingsValues(aSettings);

	m_bAll = m_PickedItemsList.IsEmpty() && m_UnselectedItemsList.IsEmpty();
	m_bSelection = !m_bAll;
	m_bRange = FALSE; //@@@TODO ammettere il salvataggio anche del range
}

//-----------------------------------------------------------------------------
HotFilterObj* HotFilterRange::AddDataPickerHotFilter
(
	HotFilterObj*	pHFL,
	const	CString&		strLinkedColumnName,
	const	CString&		strTileTitle,
	BOOL			bInitiallyUnpinned
)
{
	ASSERT_TRACE(pHFL->IsKindOf(RUNTIME_CLASS(HotFilterObj)), "Object must derive from HotFilterObj");

	pHFL->AttachHFLParent(this);
	m_DataPickerHotFilters.Add(new HotFilterInfo(pHFL, strLinkedColumnName, strTileTitle, bInitiallyUnpinned));

	return pHFL;
}

//-----------------------------------------------------------------------------
void HotFilterRange::OnDefineDataPickerParamsQuery
(
	const	CString&,
	SqlTable*	pTable,
	SqlRecord*	pRec
)
{
	for (int i = 0; i <= m_DataPickerHotFilters.GetUpperBound(); i++)
	{
		HotFilterInfo* pInfo = m_DataPickerHotFilters.GetAt(i);
		pInfo->m_pHFL->DefineQuery(pTable, pRec, *pRec->GetDataObjFromColumnName(pInfo->m_strLinkedColumnName));
	}
}

//-----------------------------------------------------------------------------
void HotFilterRange::OnPrepareDataPickerParamsQuery
(
	const	CString&,
	SqlTable*	pTable
)
{
	for (int i = 0; i <= m_DataPickerHotFilters.GetUpperBound(); i++)
	{
		HotFilterInfo* pInfo = m_DataPickerHotFilters.GetAt(i);
		pInfo->m_pHFL->PrepareQuery(pTable);
	}
}

//-----------------------------------------------------------------------------
BOOL HotFilterRange::IsEmptyDataPickerSelectionQuery()
{
	BOOL bOk = TRUE;
	for (int i = 0; i <= m_DataPickerHotFilters.GetUpperBound(); i++)
	{
		HotFilterInfo* pInfo = m_DataPickerHotFilters.GetAt(i);
		bOk &= pInfo->m_pHFL->IsEmptyQuery();
	}

	return bOk;
}

//-----------------------------------------------------------------------------
void HotFilterRange::ResetPickerCriteria()
{
	for (int i = 0; i <= m_DataPickerHotFilters.GetUpperBound(); i++)
	{
		HotFilterInfo* pInfo = m_DataPickerHotFilters.GetAt(i);
		pInfo->m_pHFL->ResetCriteria();
	}
}

//-----------------------------------------------------------------------------
void HotFilterRange::AddPickerColumn(CString strColumnName, CString strColumnTitle, CString strFieldName)
{
	HotFilterRangeElementColumn* pColumn = new HotFilterRangeElementColumn(strColumnName, strColumnTitle, strFieldName);

	m_RangeColumns.Add(pColumn);
}

//-----------------------------------------------------------------------------
void HotFilterRange::CustomizeDataPicker(SqlRecord* pRec, HotFilterDataPicker* pPicker)
{
	for (int i = 0; i <= m_RangeColumns.GetUpperBound(); i++)
	{
		HotFilterRangeElementColumn* pCol = m_RangeColumns.GetAt(i);

		DataObj* pDataObj = pRec->GetDataObjFromColumnName(pCol->m_strFieldName);

		pPicker->AddColumn
		(
			pCol->m_strColumnName,
			pCol->m_strColumnTitle,
			pDataObj
		);
	}
}

//--------------------------------------------------------------------------------------
BOOL HotFilterRange::OnBeforeBatchExecute()
{
	ASSERT_VALID(m_pDocument);
	BOOL bOK = TRUE;
	if (!GetRadioAll())
	{
		if (/*pHFLRange->GetRangeFrom().IsEmpty() || */GetRangeTo().IsEmpty())
		{
			m_pDocument->m_pMessages->Add(GetCaption() + _T(" - ") + _TB("Upper range value must be specified"));
			bOK = FALSE;
		}

		if (GetRangeFrom() > GetRangeTo())
		{
			m_pDocument->m_pMessages->Add(GetCaption() + _T(" - ") + _TB("Lower range value cannot be greater than upper range value"));
			bOK = FALSE;
		}
	}
	return bOK;
}

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterList		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HotFilterList, HotFilterObj)

//-----------------------------------------------------------------------------
HotFilterList::HotFilterList
(
	EHotFilterType type, CAbstractFormDoc*	pDocument, HotFilterManager* pHotFilterManager,
	CRuntimeClass*		pHKLClass,
	int					nNotificationIDC /*= 0*/
)
	:
	HotFilterObj(type, pDocument, pHotFilterManager, nNotificationIDC),
	m_nHKLLinkedColumnIdx(-1),
	m_pHKLItemsList(NULL),
	m_SelectionStyle(POPUP)
{
	ASSERT_VALID(pDocument);
	ASSERT_VALID(pHotFilterManager);
	ASSERT(pHKLClass);
}

//-----------------------------------------------------------------------------
HotFilterList::HotFilterList()
	:
	HotFilterObj(EHotFilterType::HF_CHECKLISTBOX, NULL, NULL)
{
}

//-----------------------------------------------------------------------------
HotFilterList::~HotFilterList()
{
}

//-----------------------------------------------------------------------------
void HotFilterList::Customize()
{
	__super::Customize();

	AddVar(L"List", m_arItemsList);

}

//-----------------------------------------------------------------------------
CRuntimeClass* HotFilterList::GetDataObjClass() const
{
	ASSERT_TRACE(m_pHKLItemsList, "GetDataObjClass called prior the Hotlinks are defined");
	return m_pHKLItemsList->GetDataObj()->GetRuntimeClass();
}

//-----------------------------------------------------------------------------
void HotFilterList::PreparePickedItemsList()
{
	if (m_PickedItemsList.GetSize() == 0 && m_arItemsList.GetSize())
		m_PickedItemsList = m_arItemsList.GetData();
}

//-----------------------------------------------------------------------------
void HotFilterList::CreateHotlinks()
{
	CTBNamespace nsHkl(CTBNamespace::NSObjectType::HOTLINK, m_sNsHotlink);
	if (!nsHkl.IsValid())
	{
		return;
	}

	CString sName = this->m_strName + L"_HKLList";
	HotKeyLink* pHL = m_pDocument->GetHotLink(sName, nsHkl);
	if (!pHL)
	{
		return;
	}
	pHL->EnableAutoFind(FALSE);
	m_pHKLItemsList = pHL;


	m_nHKLLinkedColumnIdx = m_pHKLItemsList->GetDbFieldRecIndex(m_pHKLItemsList->GetAttachedRecord());
	ASSERT_TRACE(m_nHKLLinkedColumnIdx != -1, "Linked column not found in Hotlink record!");
}

//-----------------------------------------------------------------------------
BOOL HotFilterList::IsEmptyQuery()
{
	m_PickedItemsList.RemoveAll();
	PreparePickedItemsList();
	return m_PickedItemsList.GetSize() == 0;
}

//-----------------------------------------------------------------------------
void HotFilterList::OnResetCriteria()
{
	m_PickedItemsList.RemoveAll();
	m_arItemsList.RemoveAll();
	m_arItemsList.Clear();

	m_arItemsList.SetModified();
	m_arItemsList.SetDirty();
	//DataStr sEmpty = L"";
	//m_arItemsList.SetBaseDataType(DataType::String);
	//m_arItemsList.Assign(sEmpty);
}

//-----------------------------------------------------------------------------
void HotFilterList::DefineQuery
(
	SqlTable*	pTable,
	SqlRecord*	pRec,
	const	DataObj&	aColumn,
	CString		sOperator
)
{
	DefineQuerySelectionPicked(pTable, pRec, aColumn);
}

//-----------------------------------------------------------------------------
void HotFilterList::PrepareQuery
(
	SqlTable*	pTable
)
{
	PrepareQuerySelectionPicked(pTable);
}

//-----------------------------------------------------------------------------
void HotFilterList::ManageReadOnly()
{
	m_arItemsList.SetAlwaysReadOnly(m_bIsReadOnly);
}

#pragma region HotFilterDateRange
//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterDateRange		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HotFilterDateRange, HotFilterObj)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(HotFilterDateRange, HotFilterObj)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_RADIO_ALL, ManageReadOnly)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_RADIO_RANGE, ManageReadOnly)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_EDIT_FROM, OnFromChanged)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_EDIT_TO, OnToChanged)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
HotFilterDateRange::HotFilterDateRange
(
	EHotFilterType type, CAbstractFormDoc*	pDocument, HotFilterManager* pHotFilterManager,
	SelectionStyle style, int nNotificationIDC /*= 0*/
)
	:
	HotFilterObj          (type, pDocument, pHotFilterManager, nNotificationIDC),
	m_bAll                (TRUE),
	m_bRange              (FALSE),
	m_RangeFrom           (DataDate::NULLDATE),
	m_RangeTo             (DataDate::NULLDATE),
	m_SelectionStyle      (style),
	m_bDefaultCurrentMonth(FALSE)
{
	if (style == SIMPLE)
	{
		m_bAll = FALSE;
		m_bRange = TRUE;
	}
}

//-----------------------------------------------------------------------------
HotFilterDateRange::HotFilterDateRange()
	:
	HotFilterObj          (EHotFilterType::HF_RANGE_SIMPLE, NULL, NULL),
	m_bAll                (TRUE),
	m_bRange              (FALSE),
	m_RangeFrom           (DataDate::NULLDATE),
	m_RangeTo             (DataDate::NULLDATE),
	m_SelectionStyle      (SelectionStyle::SIMPLE),
	m_bDefaultCurrentMonth(FALSE)
{
	//if (m_SelectionStyle == SIMPLE)
	//{
	//	m_bAll = FALSE;
	//	m_bRange = TRUE;
	//}
}

//-----------------------------------------------------------------------------
HotFilterDateRange::~HotFilterDateRange()
{
}

//-----------------------------------------------------------------------------
void HotFilterDateRange::Customize()
{
	__super::Customize();

	AddVar(L"All",	 m_bAll);
	AddVar(L"Range", m_bRange);
	AddVar(L"From",  m_RangeFrom);
	AddVar(L"To",	 m_RangeTo);
}

//-----------------------------------------------------------------------------
BOOL HotFilterDateRange::IsEmptyQuery()
{
	if (m_bAll)
		return TRUE;

	if (m_bRange)
		return	m_RangeFrom.IsEmpty() && m_RangeTo.IsEmpty() ||
		m_RangeFrom == DataDate::MINVALUE && m_RangeTo == DataDate::MAXVALUE;

	ASSERT_TRACE(FALSE, "Undefined status for the HotFilter query!");
	return TRUE;
}

//-----------------------------------------------------------------------------
void HotFilterDateRange::DefineQuery
(
	SqlTable*	pTable,
	SqlRecord*	pRec,
	const	DataObj&	aColumn,
	CString		sOperator /*= _T("AND")*/
)
{
	if (IsEmptyQuery())
		return;

	TRY
	{
		pRec->SetQualifier();
	//@@TODO GESTIRE CASI TIPO: xxx - ULTIMO o PRIMO - xxx
	pTable->AddBetweenColumn(pRec, aColumn);
	pTable->AddParam(CString(GetName() + szParamFrom),	aColumn);
	pTable->AddParam(CString(GetName() + szParamTo),	aColumn);
	}
		CATCH(SqlException, e)
	{
		TCHAR msg[255];
		e->GetErrorMessage(msg, 255);
		ASSERT_TRACE1(FALSE, "Errors defining the HotFilter query:\n%s", msg);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void HotFilterDateRange::PrepareQuery(SqlTable* pTable)
{
	if (IsEmptyQuery())
		return;

	TRY
	{
		pTable->SetParamValue(CString(GetName() + szParamFrom),	m_RangeFrom);
		pTable->SetParamValue(CString(GetName() + szParamTo),	m_RangeTo);
	}
		CATCH(SqlException, e)
	{
		TCHAR msg[255];
		e->GetErrorMessage(msg, 255);
		ASSERT_TRACE1(FALSE, "Errors preparing the HotFilter query:\n%s", msg);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
CRuntimeClass* HotFilterDateRange::GetDataObjClass() const
{
	return RUNTIME_CLASS(DataDate);
}

//-----------------------------------------------------------------------------
void HotFilterDateRange::OnResetCriteria()
{
	m_bAll = TRUE;
	m_bRange = FALSE;
	m_RangeFrom.Clear();
	m_RangeTo.Clear();

	ManageReadOnly();
}

//-----------------------------------------------------------------------------
void HotFilterDateRange::ManageReadOnly()
{
	if (!m_bRange)
	{
		m_RangeFrom.Clear();
		m_RangeTo.Clear();
	}

	m_bAll.		SetAlwaysReadOnly(m_bIsReadOnly);
	m_bRange.   SetAlwaysReadOnly(m_bIsReadOnly);
	m_RangeFrom.SetAlwaysReadOnly(!m_bRange || m_bIsReadOnly);
	m_RangeTo.  SetAlwaysReadOnly(!m_bRange || m_bIsReadOnly);

	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void HotFilterDateRange::OnFromChanged()
{
	if (m_RangeTo < m_RangeFrom)
		m_RangeTo = m_RangeFrom;

	//NotifyChanged(HFL_ELEMENT_FROM, m_hWnd);

	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void HotFilterDateRange::OnToChanged()
{
	if (m_RangeTo < m_RangeFrom)
		m_RangeFrom = m_RangeTo;
	//NotifyChanged(HFL_ELEMENT_TO, m_hWnd);
}

//-----------------------------------------------------------------------------
BOOL HotFilterDateRange::OnBeforeBatchExecute()
{
	ASSERT_VALID(m_pDocument);
	BOOL bOK = TRUE;
	if (!GetRadioAll())
	{
		if (GetRangeFrom().IsEmpty() || GetRangeTo().IsEmpty())
		{
			m_pDocument->m_pMessages->Add(GetCaption() + _T(" - ") + _TB("Upper range value must be specified"));
			bOK = FALSE;
		}

		if (GetRangeFrom() > GetRangeTo())
		{
			m_pDocument->m_pMessages->Add(GetCaption() + _T(" - ") + _TB("Lower range value cannot be greater than upper range value"));
			bOK = FALSE;
		}
	}
	return bOK;
}

#pragma endregion

#pragma region HotFilterIntRange
//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterIntRange		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HotFilterIntRange, HotFilterObj)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(HotFilterIntRange, HotFilterObj)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_RADIO_INT_ALL, ManageReadOnly)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_RADIO_INT_RANGE, ManageReadOnly)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_INT_FROM, OnFromChanged)
	ON_EN_VALUE_CHANGED(IDC_HFL_RANGE_INT_TO, OnToChanged)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
HotFilterIntRange::HotFilterIntRange
(
	EHotFilterType type, CAbstractFormDoc*	pDocument, HotFilterManager* pHotFilterManager,
	SelectionStyle style, int nNotificationIDC /*= 0*/
)
	:
	HotFilterObj    (type, pDocument, pHotFilterManager, nNotificationIDC),
	m_bAll          (TRUE),
	m_bRange        (FALSE),
	m_RangeFrom     (DataDate::NULLDATE),
	m_RangeTo       (DataDate::NULLDATE),
	m_SelectionStyle(style)
{
	//if (style == SIMPLE)
	//{
	//	m_bAll = FALSE;
	//	m_bRange = TRUE;
	//}
}

//-----------------------------------------------------------------------------
HotFilterIntRange::HotFilterIntRange()
	:
	HotFilterObj    (EHotFilterType::HF_RANGE_SIMPLE, NULL, NULL),
	m_bAll          (TRUE),
	m_bRange        (FALSE),
	m_RangeFrom     (DataDate::NULLDATE),
	m_RangeTo       (DataDate::NULLDATE),
	m_SelectionStyle(SelectionStyle::SIMPLE)
{
	//if (m_SelectionStyle == SIMPLE)
	//{
	//	m_bAll = FALSE;
	//	m_bRange = TRUE;
	//}
}

//-----------------------------------------------------------------------------
HotFilterIntRange::~HotFilterIntRange()
{
}

//-----------------------------------------------------------------------------
void HotFilterIntRange::Customize()
{
	__super::Customize();

	AddVar(L"All",   m_bAll);
	AddVar(L"Range", m_bRange);
	AddVar(L"From",  m_RangeFrom);
	AddVar(L"To",    m_RangeTo);
}

//-----------------------------------------------------------------------------
BOOL HotFilterIntRange::IsEmptyQuery()
{
	if (m_bAll)
		return TRUE;

	if (m_bRange)
		return	m_RangeFrom.IsEmpty() && m_RangeTo.IsEmpty() ||
		m_RangeFrom == DataInt::MINVALUE && m_RangeTo == DataInt::MAXVALUE;

	ASSERT_TRACE(FALSE, "Undefined status for the HotFilter query!");
	return TRUE;
}

//-----------------------------------------------------------------------------
void HotFilterIntRange::DefineQuery
(
	SqlTable*	pTable,
	SqlRecord*	pRec,
	const	DataObj&	aColumn,
	CString		sOperator /*= _T("AND")*/
)
{
	if (IsEmptyQuery())
		return;

	TRY
	{
		pRec->SetQualifier();
	//@@TODO GESTIRE CASI TIPO: xxx - ULTIMO o PRIMO - xxx
	pTable->AddBetweenColumn(pRec, aColumn);
	pTable->AddParam(CString(GetName() + szParamFrom),	aColumn);
	pTable->AddParam(CString(GetName() + szParamTo),	aColumn);
	}
		CATCH(SqlException, e)
	{
		TCHAR msg[255];
		e->GetErrorMessage(msg, 255);
		ASSERT_TRACE1(FALSE, "Errors defining the HotFilter query:\n%s", msg);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void HotFilterIntRange::PrepareQuery(SqlTable* pTable)
{
	if (IsEmptyQuery())
		return;

	TRY
	{
		pTable->SetParamValue(CString(GetName() + szParamFrom),	m_RangeFrom);
		pTable->SetParamValue(CString(GetName() + szParamTo),	m_RangeTo);
	}
		CATCH(SqlException, e)
	{
		TCHAR msg[255];
		e->GetErrorMessage(msg, 255);
		ASSERT_TRACE1(FALSE, "Errors preparing the HotFilter query:\n%s", msg);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
CRuntimeClass* HotFilterIntRange::GetDataObjClass() const
{
	return RUNTIME_CLASS(DataInt);
}

//-----------------------------------------------------------------------------
void HotFilterIntRange::OnResetCriteria()
{
	m_bAll = TRUE;
	m_bRange = FALSE;
	m_RangeFrom.Clear();
	m_RangeTo.Clear();

	ManageReadOnly();
}

//-----------------------------------------------------------------------------
void HotFilterIntRange::ManageReadOnly()
{
	if (!m_bRange)
	{
		m_RangeFrom.Clear();
		m_RangeTo.Clear();
	}

	m_bAll.     SetAlwaysReadOnly(m_bIsReadOnly);
	m_bRange.   SetAlwaysReadOnly(m_bIsReadOnly);
	m_RangeFrom.SetAlwaysReadOnly(!m_bRange || m_bIsReadOnly);
	m_RangeTo.  SetAlwaysReadOnly(!m_bRange || m_bIsReadOnly);

	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void HotFilterIntRange::OnFromChanged()
{
	if (m_RangeTo < m_RangeFrom)
		m_RangeTo = m_RangeFrom;

	//NotifyChanged(HFL_ELEMENT_FROM, m_hWnd);

	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void HotFilterIntRange::OnToChanged()
{
	if (m_RangeTo < m_RangeFrom)
		m_RangeFrom = m_RangeTo;
	//NotifyChanged(HFL_ELEMENT_TO, m_hWnd);
}

//-----------------------------------------------------------------------------
BOOL HotFilterIntRange::OnBeforeBatchExecute()
{
	ASSERT_VALID(m_pDocument);
	BOOL bOK = TRUE;
	if (!GetRadioAll())
	{
		if (GetRangeFrom().IsEmpty() && GetRangeTo().IsEmpty())
			return bOK;

		//if (GetRangeFrom().IsEmpty() || GetRangeTo().IsEmpty())
		//{
		//	m_pDocument->m_pMessages->Add(GetCaption() + _T(" - ") + _TB("Upper range value must be specified"));
		//	bOK = FALSE;
		//}

		if (GetRangeFrom() > GetRangeTo())
		{
			m_pDocument->m_pMessages->Add(GetCaption() + _T(" - ") + _TB("Lower range value cannot be greater than upper range value"));
			bOK = FALSE;
		}
	}
	return bOK;
}
#pragma endregion

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterCombo		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HotFilterCombo, HotFilterObj)

//-----------------------------------------------------------------------------
HotFilterCombo::HotFilterCombo
(
	EHotFilterType type, CAbstractFormDoc*	pDocument, HotFilterManager* pHotFilterManager,
	CRuntimeClass*		pHKLClass,
	int					nNotificationIDC /*= 0*/
)
	:
	HotFilterObj(type, pDocument, pHotFilterManager, nNotificationIDC),
	m_nHKLLinkedColumnIdx(-1),
	m_pHKLMSStrCombo(NULL),
	m_pCtrlMSStrCombo(NULL)
{

}

//-----------------------------------------------------------------------------
HotFilterCombo::HotFilterCombo()
	:
	HotFilterObj(EHotFilterType::HF_CHECKLISTBOX, NULL, NULL),
	m_nHKLLinkedColumnIdx(-1),
	m_pHKLMSStrCombo(NULL),
	m_pCtrlMSStrCombo(NULL)
{

}

//-----------------------------------------------------------------------------
HotFilterCombo::~HotFilterCombo()
{
	SAFE_DELETE(m_pHKLMSStrCombo);
}

//-----------------------------------------------------------------------------
CRuntimeClass* HotFilterCombo::GetDataObjClass() const
{
	ASSERT_TRACE(m_pHKLMSStrCombo, "GetDataObjClass called prior the Hotlinks are defined");
	return m_pHKLMSStrCombo->GetDataObj()->GetRuntimeClass();
}

//-----------------------------------------------------------------------------
void HotFilterCombo::CreateHotlinks()
{
	CTBNamespace nsHkl(CTBNamespace::NSObjectType::HOTLINK, m_sNsHotlink);
	if (!nsHkl.IsValid())
	{
		return;
	}

	HotKeyLink* pHL = dynamic_cast<HotKeyLink*>(AfxGetTbCmdManager()->RunHotlink(nsHkl));
	if (!pHL)
	{
		return;
	}

	//SAFE_DELETE(m_pHKLMSStrCombo);
	m_pHKLMSStrCombo = pHL;
	m_pHKLMSStrCombo->AttachDocument(m_pDocument);

	m_nHKLLinkedColumnIdx = m_pHKLMSStrCombo->GetDbFieldRecIndex(m_pHKLMSStrCombo->GetAttachedRecord());
	ASSERT_TRACE(m_nHKLLinkedColumnIdx != -1, "Linked column not found in Hotlink record!");
}

//-----------------------------------------------------------------------------
void HotFilterCombo::OnResetCriteria()
{
	m_PickedItemsList.RemoveAll();
	if (m_pCtrlMSStrCombo)
		m_pCtrlMSStrCombo->SetArrayValue(m_PickedItemsList);

	if (m_bSetSelected && m_pCtrlMSStrCombo)
	{
		m_SelectedObj.RemoveAll();
		m_pCtrlMSStrCombo->SetArrayValue(m_SelectedObj);
	}

	ManageReadOnly();
}

//-----------------------------------------------------------------------------
void HotFilterCombo::ManageReadOnly()
{
	//TODO
	//m_pCtrlMSStrCombo->SetAlwaysReadOnly()
}

//-----------------------------------------------------------------------------
void HotFilterCombo::OnInitItemsList()
{
	if (m_pCtrlMSStrCombo)
		m_pCtrlMSStrCombo->SetArrayValue(m_PickedItemsList);
	if (m_bSetSelected && m_pCtrlMSStrCombo)
	{
		m_pCtrlMSStrCombo->SetArrayValue(m_SelectedObj);
	}
}

//-----------------------------------------------------------------------------
void HotFilterCombo::AttachMSStrCombo(CMSStrCombo* pMSStrCombo)
{
	m_pCtrlMSStrCombo = pMSStrCombo;
	m_pCtrlMSStrCombo->SetArrayValue(m_PickedItemsList);
	if (m_bSetSelected)
		m_pCtrlMSStrCombo->SetArrayValue(m_PickedItemsList);
}

//-----------------------------------------------------------------------------
void HotFilterCombo::DoMSStrComboChanged()
{
	ASSERT_TRACE(m_pCtrlMSStrCombo, "HotFilter: MSStrCombo control not attached!");
	if (!m_pCtrlMSStrCombo)
		return;

	m_PickedItemsList.RemoveAll();
	if (!m_pCtrlMSStrCombo->IsSelectAll())
		m_pCtrlMSStrCombo->GetArrayValue(m_PickedItemsList);

	if (m_bSetSelected)
	{
		m_SelectedObj.RemoveAll();
		m_pCtrlMSStrCombo->GetArrayValue(m_SelectedObj);
	}

}

//-----------------------------------------------------------------------------
BOOL HotFilterCombo::IsEmptyQuery()
{
	return m_PickedItemsList.GetSize() == 0;
}

//-----------------------------------------------------------------------------
void HotFilterCombo::DefineQuery
(
	SqlTable*	pTable,
	SqlRecord*	pRec,
	const	DataObj&	aColumn,
	CString		sOperator /*= _T("AND")*/
)
{
	DefineQuerySelectionPicked(pTable, pRec, aColumn);
}

//-----------------------------------------------------------------------------
void HotFilterCombo::PrepareQuery
(
	SqlTable*	pTable
)
{
	PrepareQuerySelectionPicked(pTable);
}

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterDateRange		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(HotFilterArray, HotFilterObj)

//-----------------------------------------------------------------------------

HotFilterArray::HotFilterArray()
	:
	HotFilterObj(EHotFilterType::HF_ARRAY, NULL, 0)
{
}

///////////////////////////////////////////////////////////////////////////////