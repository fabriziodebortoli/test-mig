#include "stdafx.h" 

#include <TbOleDb\Sqltable.h>
#include "TBDMSEnums.h"
#include "CommonObjects.h"
#include "TBRepositoryManager.h"
#include "BDDMSRepository.h"
#include "DMSSearchFilter.h"

////////////////////////////////////////////////////////////////////////////////
//	class TDMS_CollectionsFields implementation
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TDMS_Field, SqlRecord)

//-----------------------------------------------------------------------------
TDMS_Field::TDMS_Field(BOOL bCallInit  /* = TRUE */)
	:
	SqlRecord(GetStaticName(), AfxGetDMSSqlConnection())
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TDMS_Field::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_KEY(_NS_FLD("FieldName"),			f_FieldName);
		BIND_DATA(_NS_FLD("FieldDescription"),	f_FieldDescription);
		BIND_DATA(_NS_FLD("ValueType"),			f_ValueType);	
		//BIND_DATA(_NS_FLD("IsCategory"), f_IsCategory);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TDMS_Field::GetStaticName() { return _NS_TBL("DMS_Field"); }

////////////////////////////////////////////////////////////////////////////////
//	class TDMS_CollectionsFields implementation
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TDMS_CollectionsFields, SqlRecord)

//-----------------------------------------------------------------------------
TDMS_CollectionsFields::TDMS_CollectionsFields(BOOL bCallInit  /* = TRUE */)
	:
	SqlRecord(GetStaticName(), AfxGetDMSSqlConnection())
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TDMS_CollectionsFields::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_KEY(_NS_FLD("FieldName"),		f_FieldName);
		BIND_KEY(_NS_FLD("CollectionID"),	f_CollectionID);
		BIND_DATA(_NS_FLD("FieldGroup"),	f_GroupType);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TDMS_CollectionsFields::GetStaticName() { return _NS_TBL("DMS_CollectionsFields"); }

////////////////////////////////////////////////////////////////////////////////
//	class TDMS_CollectionsFields implementation
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TDMS_SearchFieldIndexes, SqlRecord)

//-----------------------------------------------------------------------------
TDMS_SearchFieldIndexes::TDMS_SearchFieldIndexes(BOOL bCallInit  /* = TRUE */)
	:
	SqlRecord(GetStaticName(), AfxGetDMSSqlConnection())
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TDMS_SearchFieldIndexes::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_KEY(_NS_FLD("SearchIndexID"),		f_SearchIndexID);	
		BIND_DATA(_NS_FLD("FieldName"),			f_FieldName);
		BIND_DATA(_NS_FLD("FieldValue"),		f_FieldValue);
		BIND_DATA(_NS_FLD("FormattedValue"),	f_FormattedValue);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TDMS_SearchFieldIndexes::GetStaticName() { return _NS_TBL("DMS_SearchFieldIndexes"); }

/////////////////////////////////////////////////////////////////////////////
//						class HKLDMSFields
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HKLDMSFields, HotKeyLink)

//------------------------------------------------------------------------------
HKLDMSFields::HKLDMSFields()
	: 
	HotKeyLink				(RUNTIME_CLASS(TDMS_Field), _NS_DOC(""), AfxGetDMSSqlConnection()),
	m_pCollectionID			(NULL),
	m_pCollectionsFieldsRec	(NULL)
{
	EnableAddOnFly(FALSE);
	MustExistData(TRUE);	
	
	m_pCollectionsFieldsRec = new TDMS_CollectionsFields();
}

//------------------------------------------------------------------------------
HKLDMSFields::~HKLDMSFields()
{
	if (m_pCollectionsFieldsRec)
		delete m_pCollectionsFieldsRec;
}

//------------------------------------------------------------------------------
void HKLDMSFields::FilterForCollectionID(DataLng* pCollectionID)
{
	m_pCollectionID = pCollectionID;
}

static TCHAR szP1[] = _T("P1");
static TCHAR szP2[] = _T("P2");
static TCHAR szP3[] = _T("P3");
//------------------------------------------------------------------------------
void HKLDMSFields::OnDefineQuery(SelectionType nQuerySelection)
{	
	GetRecord()->SetQualifier((m_pCollectionID && *m_pCollectionID > -1) ? _T("F") : _T(""));
	if (*m_pCollectionID > -1)
	{
		m_pCollectionsFieldsRec->SetQualifier(_T("S"));
		m_pTable->FromTable(GetRecord());
		m_pTable->FromTable(m_pCollectionsFieldsRec);
		m_pTable->AddParam(szP2, m_pCollectionsFieldsRec->f_CollectionID);
		m_pTable->AddFilterColumn(m_pCollectionsFieldsRec, m_pCollectionsFieldsRec->f_CollectionID);
		m_pTable->AddCompareColumn(GetRecord()->f_FieldName, m_pCollectionsFieldsRec, m_pCollectionsFieldsRec->f_FieldName);
	}
	
	m_pTable->SelectAll(GetRecord());
	
	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->AddFilterColumn(GetRecord()->f_FieldDescription);
			m_pTable->AddParam(szP1, GetRecord()->f_FieldDescription);
			break;
		   

		case UPPER_BUTTON:
		case LOWER_BUTTON:
			m_pTable->AddSortColumn(GetRecord()->f_FieldDescription);
			m_pTable->AddFilterLike(GetRecord()->f_FieldDescription);
			m_pTable->AddParam(szP1, GetRecord()->f_FieldDescription);
			break;
	}
}

//--------------------------------------------------------------------------------
void HKLDMSFields::OnPrepareQuery(DataObj* pDataObj, SelectionType nQuerySelection)
{
	ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataStr)));

	if (*m_pCollectionID > -1)
		m_pTable->SetParamValue(szP2, *m_pCollectionID);

	switch (nQuerySelection)
	{
	case DIRECT_ACCESS:
		m_pTable->SetParamValue(szP1, *pDataObj);
		break;

	case UPPER_BUTTON:
	case LOWER_BUTTON:
		m_pTable->SetParamLike(szP1, *pDataObj);
		break;
	}
}

/////////////////////////////////////////////////////////////////////////////
//						class HKLSearchFieldIndexes
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HKLSearchFieldIndexes, HotKeyLink)

//------------------------------------------------------------------------------
HKLSearchFieldIndexes::HKLSearchFieldIndexes()
	:
	HotKeyLink		(RUNTIME_CLASS(TDMS_SearchFieldIndexes), _NS_DOC(""), AfxGetDMSSqlConnection()),
	m_CollectionID	(-1)
{
	EnableAddOnFly(FALSE);
	MustExistData(TRUE);
}

//------------------------------------------------------------------------------
void HKLSearchFieldIndexes::OnDefineQuery(SelectionType nQuerySelection)
{
	m_pTable->SelectAll();

	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->AddFilterColumn(GetRecord()->f_FieldValue);
			m_pTable->AddParam(szP1, GetRecord()->f_FieldValue);
			if (!m_FieldName.IsEmpty())
			{
				m_pTable->AddFilterColumn(GetRecord()->f_FieldName);
				m_pTable->AddParam(szP2, GetRecord()->f_FieldName);
			}
			break;

		case UPPER_BUTTON:
		case LOWER_BUTTON:
			m_pTable->AddSortColumn(GetRecord()->f_FieldValue);
			m_pTable->AddFilterLike(GetRecord()->f_FieldValue);
			m_pTable->AddParam(szP1, GetRecord()->f_FieldValue);
			if (!m_FieldName.IsEmpty())
			{
				m_pTable->AddFilterColumn(GetRecord()->f_FieldName);
				m_pTable->AddParam(szP2, GetRecord()->f_FieldName);

			}
			break;
	}
}

//--------------------------------------------------------------------------------
void HKLSearchFieldIndexes::OnPrepareQuery(DataObj* pDataObj, SelectionType nQuerySelection)
{
	ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataStr)));

	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->SetParamValue(szP1, *pDataObj);
			if (!m_FieldName.IsEmpty())
				m_pTable->SetParamValue(szP2, m_FieldName);
			break;

		case UPPER_BUTTON:
		case LOWER_BUTTON:
			m_pTable->SetParamLike(szP1, *pDataObj);
			if (!m_FieldName.IsEmpty())
				m_pTable->SetParamValue(szP2, m_FieldName);
			break;
	}
}

////////////////////////////////////////////////////////////////////////////////
//	class VSearchFieldCondition implementation: rappresenta un singolo filtro di ricerca
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VSearchFieldCondition, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VSearchFieldCondition::VSearchFieldCondition(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord(GetStaticName()),
	l_OperationType	(E_OPERATION_TYPE_DEFAULT),
	l_LogicOperator	(E_LOGIC_OPERATOR_TYPE_DEFAULT)
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void VSearchFieldCondition::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_KEY(_NS_FLD("VConditionRow"),		l_ConditionRow);
		LOCAL_DATA(_NS_FLD("VCollectionID"),	l_CollectionID);
		LOCAL_DATA(_NS_FLD("VSearchFieldID"),	l_SearchFieldID);
		LOCAL_STR(_NS_FLD("VCollectionName"),	l_CollectionName,	256);
		LOCAL_STR(_NS_FLD("VFieldName"),		l_FieldName,		80);
		LOCAL_STR(_NS_FLD("VFieldDescription"), l_FieldDescription, 256);
		LOCAL_DATA(_NS_FLD("VOperationType"),	l_OperationType);
		LOCAL_STR(_NS_FLD("VFormattedValue"),	l_FormattedValue,	512);
		LOCAL_STR(_NS_FLD("VFieldValue"),		l_FieldValue,		512);		
		LOCAL_STR(_NS_FLD("ValueType"),			l_ValueType,		50);
		LOCAL_DATA(_NS_FLD("VLogicOperator"),	l_LogicOperator);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
BOOL VSearchFieldCondition::IsValidCondition() const
{
	return (
				!l_FieldName.IsEmpty() &&
				((l_OperationType != E_OPERATION_LIKE && !l_SearchFieldID.IsEmpty()) || (l_OperationType == E_OPERATION_LIKE && !l_FormattedValue.IsEmpty()))
			);
}

//-----------------------------------------------------------------------------
LPCTSTR VSearchFieldCondition::GetStaticName() { return _NS_TBL("VSearchFieldCondition"); }

//////////////////////////////////////////////////////////////////////////////
//				DBTArchivedDocuments implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTSearchFieldsConditions, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTSearchFieldsConditions::DBTSearchFieldsConditions(CRuntimeClass* pClass, CAbstractFormDoc* pDocument)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("DBTSearchFieldsConditions"), ALLOW_EMPTY_BODY, FALSE)
{
}

//-----------------------------------------------------------------------------
DataObj* DBTSearchFieldsConditions::OnCheckPrimaryKey(int /*nRow*/, SqlRecord*)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTSearchFieldsConditions::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	((VSearchFieldCondition*)pSqlRec)->l_ConditionRow = nRow;
}

//-----------------------------------------------------------------------------
BOOL DBTSearchFieldsConditions::UserIsDuplicateKey(SqlRecord* pRec1, SqlRecord* pRec2)
{ 
	VSearchFieldCondition* pSearchField1 = (VSearchFieldCondition*)pRec1;
	VSearchFieldCondition* pSearchField2 = (VSearchFieldCondition*)pRec2;
	return pSearchField1->l_CollectionID == pSearchField2->l_CollectionID && 
   		   pSearchField1->l_SearchFieldID == pSearchField2->l_SearchFieldID &&
		   pSearchField1->l_OperationType == pSearchField2->l_OperationType;
}

//-----------------------------------------------------------------------------
CString DBTSearchFieldsConditions::GetDuplicateKeyMsg(SqlRecord* pRec)
{
	ASSERT(pRec->IsKindOf(RUNTIME_CLASS(VSearchFieldCondition)));
	VSearchFieldCondition* pSearchField = (VSearchFieldCondition*)pRec;

	return cwsprintf(_TB("The condition {0-%s} {1-%s} {2-%s} already exists:\r\n choose a different field or value or operation."), 
						(LPCTSTR)pSearchField->l_FieldDescription.Str(),
						(LPCTSTR)pSearchField->l_OperationType.Str(),
						(LPCTSTR)pSearchField->l_FormattedValue.Str()					
					);
}

//-----------------------------------------------------------------------------
void DBTSearchFieldsConditions::OnPrepareRow(int /*nRow*/, SqlRecord* pSqlRec)
{
	VSearchFieldCondition* pSearchFieldCondRec = (VSearchFieldCondition*)pSqlRec;
	pSearchFieldCondRec->l_CollectionID = GetDocument()->m_CollectionID;
	pSearchFieldCondRec->l_OperationType = E_OPERATION_EQUAL;
	pSearchFieldCondRec->l_LogicOperator = E_LOGIC_OPERATOR_AND;
}

///////////////////////////////////////////////////////////////////////////////
//						CSearchFilter implementation
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSearchFilter, CObject)

//-----------------------------------------------------------------------------
CSearchFilter::CSearchFilter()
	:
	m_wSearchLocation			(1),
	m_pDBTSearchFieldsConditions(NULL),
	m_pCachedSearchFieldsArray	(NULL)
{
	m_StartDate = DataDate(1, AfxGetApplicationMonth(), AfxGetApplicationYear());
	m_EndDate = AfxGetApplicationDate();
	Clear();
}

//-----------------------------------------------------------------------------
CSearchFilter::~CSearchFilter()
{
	delete m_pCachedSearchFieldsArray;
}

//-----------------------------------------------------------------------------
void CSearchFilter::Clear()
{
	m_arWorkers.RemoveAll();

	m_TopDocsNumber		= 50;
	m_CollectionID		= -1;;
	m_DocExtensionType	= szAllFiles;
	m_StartDate			= DataDate(1, AfxGetApplicationMonth(), AfxGetApplicationYear());
	m_EndDate			= AfxGetApplicationDate();
	m_wSearchLocation	= SearchLocation::All;

	m_FreeTag.Clear();

	if (m_pCachedSearchFieldsArray)
		m_pCachedSearchFieldsArray->RemoveAll();
}

//-----------------------------------------------------------------------------
void CSearchFilter::CreateSearchFieldsConditions()
{
	if (m_pCachedSearchFieldsArray)
		m_pCachedSearchFieldsArray->RemoveAll();
	else
		m_pCachedSearchFieldsArray = new RecordArray();

	if (!m_pDBTSearchFieldsConditions)
		return;

	// devo travasare le righe del DBT nella cache 
	VSearchFieldCondition* pSearchFieldRec;
	for (int i = 0; i <= m_pDBTSearchFieldsConditions->GetUpperBound(); i++)
	{
		pSearchFieldRec = (VSearchFieldCondition*)m_pDBTSearchFieldsConditions->GetRow(i);
		VSearchFieldCondition* pNewRec = (VSearchFieldCondition*)m_pDBTSearchFieldsConditions->CreateRecord();
		*pNewRec = *pSearchFieldRec;
		m_pCachedSearchFieldsArray->Add(pNewRec);
	}
}

//-----------------------------------------------------------------------------
void CSearchFilter::RestoreFilters()
{
	if (!m_pDBTSearchFieldsConditions || !m_pCachedSearchFieldsArray)
		return;

	// devo travasare le righe della cache nel DBT
	VSearchFieldCondition* pSearchFieldRec;
	for (int i = 0; i <= m_pCachedSearchFieldsArray->GetUpperBound(); i++)
	{
		pSearchFieldRec = (VSearchFieldCondition*)m_pCachedSearchFieldsArray->GetAt(i);
		VSearchFieldCondition* pNewRec = (VSearchFieldCondition*)m_pDBTSearchFieldsConditions->AddRecord();
		*pNewRec = *pSearchFieldRec;
	}
}