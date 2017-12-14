
#include "stdafx.h"  

#include <TbOleDb\lentbl.h>

#include "TResources.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// Parameters
static TCHAR szP1	[]	= _T("p1");
static TCHAR szP2	[]	= _T("p2");
static TCHAR szP3	[]	= _T("p3");
static TCHAR szP4	[]	= _T("p4");

static TCHAR szParamResourceType		[]	= _T("p4");
static TCHAR szParamResourceCode		[]	= _T("p5");
static TCHAR szParamIsWorker			[]	= _T("p6");
static TCHAR szParamChildResourceType	[]	= _T("p7");
static TCHAR szParamChildResourceCode	[]	= _T("p8");
static TCHAR szParamChildWorkerID		[]	= _T("p9");

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TResources
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TResources, SqlRecord)

//-----------------------------------------------------------------------------
TResources::TResources(BOOL bCallInit)
	:
	SqlRecord 		(GetStaticName())
{                                            
	f_ResourceType.		SetUpperCase();
	f_ResourceCode.		SetUpperCase();
	f_CostCenter.		SetUpperCase();
	f_FederalState.		SetUpperCase();
	f_ISOCountryCode.	SetUpperCase();

	BindRecord();	
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TResources::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("ResourceType"),	f_ResourceType);
		BIND_DATA	(_NS_FLD("ResourceCode"),	f_ResourceCode);
		BIND_DATA	(_NS_FLD("Description"),	f_Description);
		BIND_DATA	(_NS_FLD("Manager"),		f_Manager);
		BIND_DATA	(_NS_FLD("Notes"),			f_Notes);
		BIND_DATA	(_NS_FLD("ImagePath"),		f_ImagePath);
		BIND_DATA	(_NS_FLD("CostCenter"),		f_CostCenter);
		BIND_DATA	(_NS_FLD("Disabled"), 		f_Disabled);
		BIND_DATA	(_NS_FLD("HideOnLayout"),	f_HideOnLayout);		
		BIND_DATA	(_NS_FLD("DomicilyAddress"),f_DomicilyAddress);
		BIND_DATA	(_NS_FLD("DomicilyCity"),	f_DomicilyCity);
		BIND_DATA	(_NS_FLD("DomicilyCounty"),	f_DomicilyCounty); 
		BIND_DATA	(_NS_FLD("DomicilyZip"),	f_DomicilyZip); 
		BIND_DATA	(_NS_FLD("DomicilyCountry"),f_DomicilyCountry); 
		BIND_DATA	(_NS_FLD("Telephone1"),		f_Telephone1);
		BIND_DATA	(_NS_FLD("Telephone2"),		f_Telephone2);
		BIND_DATA	(_NS_FLD("Telephone3"),		f_Telephone3);
		BIND_DATA	(_NS_FLD("Telephone4"),		f_Telephone4);
		BIND_DATA	(_NS_FLD("Email"),			f_Email);
		BIND_DATA	(_NS_FLD("URL"),			f_URL);
		BIND_DATA	(_NS_FLD("SkypeID"),		f_SkypeID);
		BIND_DATA	(_NS_FLD("Branch"),			f_Branch);
		BIND_DATA	(_NS_FLD("Latitude"),		f_Latitude);
		BIND_DATA	(_NS_FLD("Longitude"),		f_Longitude);
		BIND_DATA	(_NS_FLD("Address2"),		f_Address2);
		BIND_DATA	(_NS_FLD("StreetNo"),		f_StreetNo);
		BIND_DATA	(_NS_FLD("District"),		f_District);
		BIND_DATA	(_NS_FLD("FederalState"),	f_FederalState);
		BIND_DATA	(_NS_FLD("ISOCountryCode"),	f_ISOCountryCode);
		LOCAL_STR	(_NS_LFLD("ManagerDesc"),	l_ManagerDes,	LEN_RM_DESCRIPTION);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TResources::GetStaticName() { return _NS_TBL("RM_Resources"); }

/////////////////////////////////////////////////////////////////////////////
//	TableReader		###  Resources ###			TRResources
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TRResources, TableReader)

//------------------------------------------------------------------------------
TRResources::TRResources(CAbstractFormDoc* pDocument/* NULL */)
	: 
	TableReader(RUNTIME_CLASS(TResources), pDocument)
{}

//------------------------------------------------------------------------------
void TRResources::OnDefineQuery()
{   
	m_pTable->SelectAll			();
	m_pTable->AddFilterColumn	(GetRecord()->f_ResourceCode);
	m_pTable->AddParam			(szP1, GetRecord()->f_ResourceCode);
	m_pTable->AddFilterColumn	(GetRecord()->f_ResourceType);
	m_pTable->AddParam			(szP2, GetRecord()->f_ResourceType);
}
	
//------------------------------------------------------------------------------
void TRResources::OnPrepareQuery()
{
	m_pTable->SetParamValue(szP1,	m_ResourceCode);
	m_pTable->SetParamValue(szP2,	m_ResourceType);
}

//------------------------------------------------------------------------------
BOOL TRResources::IsEmptyQuery()
{
	return m_ResourceCode.IsEmpty();
}

//------------------------------------------------------------------------------
TableReader::FindResult TRResources::FindRecord(const DataStr& aResourceCode, const DataStr&	aResourceType)
{
	m_ResourceCode = aResourceCode; 
	m_ResourceType = aResourceType; 
	
	return TableReader::FindRecord();
}

//------------------------------------------------------------------------------
void TRResources::Clear()
{
	m_pRecord->Init();

	m_ResourceCode.Clear();
	m_ResourceType.Clear();

	if (m_pTable && m_pTable->IsOpen()) 
		OnPrepareQuery();
}

/////////////////////////////////////////////////////////////////////////////
//		TableUpdater :	class TUResources
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TUResources, TableUpdater)

//------------------------------------------------------------------------------
TUResources::TUResources
	(
		CAbstractFormDoc* 	pDocument,	// = NULL
		CMessages* 			pMessages	// = NULL
	)													
	: 
	TableUpdater(RUNTIME_CLASS(TResources), pDocument, pMessages)
{}

//------------------------------------------------------------------------------
void TUResources::OnDefineQuery()
{
	m_pTable->SelectAll			();
	m_pTable->AddFilterColumn	(GetRecord()->f_ResourceType);
	m_pTable->AddFilterColumn	(GetRecord()->f_ResourceCode);
	m_pTable->AddParam			(szParamResourceType,		GetRecord()->f_ResourceType);
	m_pTable->AddParam			(szParamResourceCode,		GetRecord()->f_ResourceCode);
}
	
//------------------------------------------------------------------------------
void TUResources::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamResourceType,	m_ResourceType);
	m_pTable->SetParamValue(szParamResourceCode,	m_ResourceCode);
}

//------------------------------------------------------------------------------
BOOL TUResources::IsEmptyQuery()
{
	return m_ResourceType.IsEmpty() && m_ResourceCode.IsEmpty(); 
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUResources::FindRecord(const DataStr& aResourceType, const DataStr& aResourceCode, BOOL bLock)
{
	m_ResourceType = aResourceType;
	m_ResourceCode = aResourceCode;
	
	return TableUpdater::FindRecord(bLock);
}                                                                           

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TResourcesDetails
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TResourcesDetails, SqlRecord)

//-----------------------------------------------------------------------------
TResourcesDetails::TResourcesDetails(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	f_ResourceType.			SetUpperCase();
	f_ResourceCode.			SetUpperCase();
	f_ChildResourceType.	SetUpperCase();
	f_ChildResourceCode.	SetUpperCase();

	BindRecord();	
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TResourcesDetails::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("ResourceType"),		f_ResourceType);
		BIND_DATA	(_NS_FLD("ResourceCode"),		f_ResourceCode);
		BIND_DATA	(_NS_FLD("IsWorker"),			f_IsWorker); 
		BIND_DATA	(_NS_FLD("ChildResourceType"),	f_ChildResourceType);
		BIND_DATA	(_NS_FLD("ChildResourceCode"),	f_ChildResourceCode); 
		BIND_DATA	(_NS_FLD("ChildWorkerID"),		f_ChildWorkerID);

		LOCAL_STR	(_NS_LFLD("WorkerDesc"),		l_WorkerDesc,	LEN_RM_DESCRIPTION);
		LOCAL_STR	(_NS_LFLD("ManagerDesc"),		l_ManagerDesc,	LEN_RM_DESCRIPTION);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TResourcesDetails::GetStaticName() { return _NS_TBL("RM_ResourcesDetails"); }

/////////////////////////////////////////////////////////////////////////////
//		TableUpdater :	class TUResourcesDetails
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TUResourcesDetails, TableUpdater)

//------------------------------------------------------------------------------
TUResourcesDetails::TUResourcesDetails
	(
		CAbstractFormDoc* 	pDocument,	// = NULL
		CMessages* 			pMessages	// = NULL
	)													
	: 
	TableUpdater(RUNTIME_CLASS(TResourcesDetails), pDocument, pMessages)
{
}

//------------------------------------------------------------------------------
void TUResourcesDetails::OnDefineQuery()
{
	m_pTable->SelectAll			();

	m_pTable->AddFilterColumn	(GetRecord()->f_ResourceType);
	m_pTable->AddFilterColumn	(GetRecord()->f_ResourceCode);
	m_pTable->AddFilterColumn	(GetRecord()->f_IsWorker);
	m_pTable->AddFilterColumn	(GetRecord()->f_ChildResourceType);
	m_pTable->AddFilterColumn	(GetRecord()->f_ChildResourceCode);
	m_pTable->AddFilterColumn	(GetRecord()->f_ChildWorkerID);

	m_pTable->AddParam			(szParamResourceType,		GetRecord()->f_ResourceType);
	m_pTable->AddParam			(szParamResourceCode,		GetRecord()->f_ResourceCode);
	m_pTable->AddParam			(szParamIsWorker,			GetRecord()->f_IsWorker);
	m_pTable->AddParam			(szParamChildResourceType,	GetRecord()->f_ChildResourceType);
	m_pTable->AddParam			(szParamChildResourceCode,	GetRecord()->f_ChildResourceCode);
	m_pTable->AddParam			(szParamChildWorkerID, GetRecord()->f_ChildWorkerID);
}
	
//------------------------------------------------------------------------------
void TUResourcesDetails::OnPrepareQuery()
{
	m_pTable->SetParamValue		(szParamResourceType,		m_ResourceType);
	m_pTable->SetParamValue		(szParamResourceCode,		m_ResourceCode);
	m_pTable->SetParamValue		(szParamIsWorker,			m_IsWorker);
	m_pTable->SetParamValue		(szParamChildResourceType,	m_ChildResourceType);

	if (m_ChildWorkerID.IsEmpty())
	{
		m_pTable->SetParamValue(szParamChildResourceCode,	m_ChildResourceCode);
		m_pTable->SetParamValue(szParamChildWorkerID,		DataLng(0));
	}
	else
	{
		m_pTable->SetParamValue(szParamChildResourceCode,	DataStr(_T("")));
		m_pTable->SetParamValue(szParamChildWorkerID,		m_ChildWorkerID);
	}
}

//------------------------------------------------------------------------------
BOOL TUResourcesDetails::IsEmptyQuery()
{
	return m_ResourceType.IsEmpty() && m_ResourceCode.IsEmpty(); 
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUResourcesDetails::FindRecord(const DataStr&	aResourceType,
														const DataStr&	aResourceCode, 
														const DataBool&	aIsWorker,
														const DataStr&	aChildResourceType, 
														const DataStr&	aChildResourceCode, 
														const DataLng&	aChildWorkerID, 
															BOOL		bLock)
{
	m_ResourceType		= aResourceType;
	m_ResourceCode		= aResourceCode;
	m_IsWorker			= aIsWorker;
	m_ChildResourceType = aChildResourceType;
	m_ChildResourceCode = aChildResourceCode;
	m_ChildWorkerID		= aChildWorkerID;
	
	return TableUpdater::FindRecord(bLock);
}                                                                           

//------------------------------------------------------------------------------
TableUpdater::FindResult TUResourcesDetails::FindRecord(const DataStr&	aResourceType,
														const DataStr&	aResourceCode, 
														const DataStr&	aChildResourceType, 
														const DataStr&	aChildResourceCode, 
															 BOOL		bLock)
{
	m_ResourceType		= aResourceType;
	m_ResourceCode		= aResourceCode;
	m_IsWorker			= FALSE;
	m_ChildResourceType = aChildResourceType;
	m_ChildResourceCode = aChildResourceCode;
	m_ChildWorkerID		= DataLng(0);
	
	return TableUpdater::FindRecord(bLock);
}                                                                           

//------------------------------------------------------------------------------
TableUpdater::FindResult TUResourcesDetails::FindRecord(const DataStr&	aResourceType,
														const DataStr&	aResourceCode, 
														const DataLng&	aChildWorkerID, 
															 BOOL		bLock)
{
	m_ResourceType		= aResourceType;
	m_ResourceCode		= aResourceCode;
	m_IsWorker			= TRUE;
	m_ChildResourceType = _T("");
	m_ChildResourceCode = DataStr(_T(""));
	m_ChildWorkerID		= aChildWorkerID;
	
	return TableUpdater::FindRecord(bLock);
} 

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TResourcesFields
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TResourcesFields, SqlRecord)

//-----------------------------------------------------------------------------
TResourcesFields::TResourcesFields(BOOL bCallInit)
	:
	SqlRecord		(GetStaticName())
{
	BindRecord();	
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TResourcesFields::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("ResourceType"),	f_ResourceType);
		BIND_DATA	(_NS_FLD("ResourceCode"),	f_ResourceCode);
		BIND_DATA	(_NS_FLD("Line"),			f_Line);
		BIND_DATA	(_NS_FLD("FieldName"),		f_FieldName);
		BIND_DATA	(_NS_FLD("FieldValue"),		f_FieldValue); 
		BIND_DATA	(_NS_FLD("Notes"),			f_Notes);
		BIND_DATA	(_NS_FLD("HideOnLayout"),	f_HideOnLayout);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TResourcesFields::GetStaticName() { return _NS_TBL("RM_ResourcesFields"); }

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TResourcesAbsences
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TResourcesAbsences, SqlRecord)

//-----------------------------------------------------------------------------
TResourcesAbsences::TResourcesAbsences(BOOL bCallInit)
	:
	SqlRecord		(GetStaticName())
{
	f_ResourceCode.	SetUpperCase();
	f_Reason.		SetUpperCase();

	f_StartingDate.	SetFullDate();
	f_EndingDate.	SetFullDate();

	BindRecord();	
	if (bCallInit) Init(); 
}

//-----------------------------------------------------------------------------
void TResourcesAbsences::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("ResourceType"),	f_ResourceType); 
		BIND_DATA	(_NS_FLD("ResourceCode"),	f_ResourceCode);
		BIND_DATA	(_NS_FLD("Reason"),			f_Reason);
		BIND_DATA	(_NS_FLD("StartingDate"),	f_StartingDate); 
		BIND_DATA	(_NS_FLD("EndingDate"),		f_EndingDate);
		BIND_DATA	(_NS_FLD("Manager"),		f_Manager);
		BIND_DATA	(_NS_FLD("Notes"),			f_Notes);

		LOCAL_STR	(_NS_LFLD("ManagerDesc"),	l_ManagerDesc,	LEN_RM_DESCRIPTION);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TResourcesAbsences::GetStaticName() { return _NS_TBL("RM_ResourcesAbsences"); }

/////////////////////////////////////////////////////////////////////////////
//	Hotlink					HKLResources
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HKLResources, HotKeyLink)

//------------------------------------------------------------------------------
HKLResources::HKLResources()
	: 
	HotKeyLink(RUNTIME_CLASS(TResources), _NS_DOC("Framework.TbResourcesMng.TbResourcesMng.Resources")),
	m_SelectionDisabled	(BOTH)
{
}

//------------------------------------------------------------------------------
void HKLResources::OnCallLink()
{
	GetRecord()->f_ResourceType	= m_CodeType;
}

//------------------------------------------------------------------------------
BOOL HKLResources::Customize(const DataObjArray& params)
{
	ASSERT(params.GetSize() >= 1);
	
	BOOL	bFilterFound = FALSE;
	DataInt Filter;
	DataStr CodeType;

	// primo parametro HKL: ResourceType
	if (params[0]->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		CodeType.Assign(*params[0]);
		SetCodeType(CodeType);
	}

	// secondo parametro HKL: Enabled (0 (Enabled) / 1 (Disabled) / 2 (Both))
	if (params[0]->IsKindOf(RUNTIME_CLASS(DataInt)) ||
		params.GetSize() == 2 && params[1]->IsKindOf(RUNTIME_CLASS(DataInt)))
	{
		Filter.Assign(*params[params.GetSize() == 1 ? 0 : 1]);
		bFilterFound = TRUE;
	}

	if (bFilterFound)
	{
		if (Filter == 0)
			SetSelDisabled(ACTIVE);
		else
		{
			if (Filter == 1)
				SetSelDisabled(DISABLED);
			else
				SetSelDisabled(BOTH);
		}
	}
	return TRUE;
}

//------------------------------------------------------------------------------
void HKLResources::OnDefineQuery(SelectionType nQuerySelection)
{
	m_pTable->SelectAll();
	
	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->AddParam			(szP1, GetRecord()->f_ResourceType);
			m_pTable->AddFilterColumn	(GetRecord()->f_ResourceType);
			m_pTable->AddParam			(szP2, GetRecord()->f_ResourceCode);
			m_pTable->AddFilterColumn	(GetRecord()->f_ResourceCode);
			break;
			
		case UPPER_BUTTON:
		case COMBO_ACCESS:
			m_pTable->AddParam			(szP1, GetRecord()->f_ResourceType);
			m_pTable->AddFilterColumn	(GetRecord()->f_ResourceType);
			m_pTable->AddParam			(szP2, GetRecord()->f_ResourceCode);
			m_pTable->AddFilterLike		(GetRecord()->f_ResourceCode);
			m_pTable->AddSortColumn		(GetRecord()->f_ResourceCode);
			DefineDisabled();
			break;

		case LOWER_BUTTON:
			m_pTable->AddParam			(szP1, GetRecord()->f_ResourceType);
			m_pTable->AddFilterColumn	(GetRecord()->f_ResourceType);
			m_pTable->AddParam			(szP2, GetRecord()->f_Description);
			m_pTable->AddFilterLike		(GetRecord()->f_Description);
			m_pTable->AddSortColumn		(GetRecord()->f_Description);
			DefineDisabled();
			break;
	}
}

//------------------------------------------------------------------------------
void HKLResources::DefineDisabled()
{ 
	switch(m_SelectionDisabled)
	{
		case ACTIVE:
		case DISABLED: 
			m_pTable->AddFilterColumn	(GetRecord()->f_Disabled);
			m_pTable->AddParam			(szP3, GetRecord()->f_Disabled);
			break;  
	}               
}

//------------------------------------------------------------------------------
void HKLResources::PrepareDisabled()
{
	DataBool bDisabled;

	switch(m_SelectionDisabled) 
	{
		case ACTIVE: 
			bDisabled = FALSE;
			m_pTable->SetParamValue(szP3, bDisabled);
			break;

		case DISABLED: 
			bDisabled = TRUE;
			m_pTable->SetParamValue(szP3, bDisabled);
			break;
	}               
}	

//------------------------------------------------------------------------------
void HKLResources::OnPrepareQuery(DataObj* pDataObj, SelectionType nQuerySelection)
{
	ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataStr)));

	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->SetParamValue(szP1, m_CodeType);
			m_pTable->SetParamValue(szP2, *pDataObj);
			break;
			
		case UPPER_BUTTON:
		case LOWER_BUTTON:
		case COMBO_ACCESS:
			m_pTable->SetParamValue(szP1, m_CodeType);
			m_pTable->SetParamLike(szP2, *pDataObj);
			PrepareDisabled();
			break;
	}

	if (!GetRecord())
		return;


}

//------------------------------------------------------------------------------
BOOL HKLResources::IsValid()
{
	if (!HotKeyLink::IsValid())
		return FALSE;

	// se il filtro non viene settato il record risulta sempre valido
	switch	(m_SelectionDisabled)
	{
		case ACTIVE:	return SetErrorString(!GetRecord()->f_Disabled ? _T("") : cwsprintf(_TB("Code {0-%s} is disabled. It is necessary to select an enable code."), GetRecord()->f_ResourceCode));
		case DISABLED:	return SetErrorString(GetRecord()->f_Disabled ? _T("") : cwsprintf(_TB("Code {0-%s} is enabled. It is necessary to select a disable code."), GetRecord()->f_ResourceCode));
	}

	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TResourceTypes
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TResourceTypes, SqlRecord)

//-----------------------------------------------------------------------------
TResourceTypes::TResourceTypes(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	f_ResourceType.SetUpperCase();

	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TResourceTypes::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA(_NS_FLD("ResourceType"),	f_ResourceType);
		BIND_DATA(_NS_FLD("Description"),	f_Description);
		BIND_DATA(_NS_FLD("ImagePath"),		f_ImagePath);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TResourceTypes::GetStaticName() { return _NS_TBL("RM_ResourceTypes"); }

/////////////////////////////////////////////////////////////////////////////
//	Hotlink						HKLResourceType 				
/////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HKLResourceTypes, HotKeyLink)

//------------------------------------------------------------------------------
HKLResourceTypes::HKLResourceTypes()
	:
	HotKeyLink(RUNTIME_CLASS(TResourceTypes), _NS_DOC("Framework.TbResourcesMng.TbResourcesMng.ResourceTypes"))
{
}

//------------------------------------------------------------------------------
void HKLResourceTypes::OnDefineQuery(SelectionType nQuerySelection)
{
	m_pTable->SelectAll();

	switch (nQuerySelection)
	{
	case DIRECT_ACCESS:
		m_pTable->AddFilterColumn(GetRecord()->f_ResourceType);
		m_pTable->AddParam(szP1, GetRecord()->f_ResourceType);
		break;
	
	case COMBO_ACCESS:
	case UPPER_BUTTON:
		m_pTable->AddSortColumn(GetRecord()->f_ResourceType);
		m_pTable->AddFilterLike(GetRecord()->f_ResourceType);
		m_pTable->AddParam(szP1, GetRecord()->f_ResourceType);
		break;

	case LOWER_BUTTON:
		m_pTable->AddSortColumn(GetRecord()->f_Description);
		m_pTable->AddFilterLike(GetRecord()->f_Description);
		m_pTable->AddParam(szP1, GetRecord()->f_Description);
		break;
	}
}

//------------------------------------------------------------------------------
void HKLResourceTypes::OnPrepareQuery(DataObj* pDataObj, SelectionType nQuerySelection)
{
	ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataStr)));
	switch (nQuerySelection)
	{
	case DIRECT_ACCESS:
		m_pTable->SetParamValue(szP1, *pDataObj);
		break;

	case COMBO_ACCESS:
	case UPPER_BUTTON:
	case LOWER_BUTTON:
		m_pTable->SetParamLike(szP1, *pDataObj);
		break;
	}
}

/////////////////////////////////////////////////////////////////////////////
//	TableReader				### ResourceType ###			TRResourceTypes
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(TRResourceTypes, TableReader)

//------------------------------------------------------------------------------
TRResourceTypes::TRResourceTypes(CAbstractFormDoc* pDocument /* NULL */)
	:
	TableReader(RUNTIME_CLASS(TResourceTypes), pDocument)
{
}

//------------------------------------------------------------------------------
void TRResourceTypes::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn(GetRecord()->f_ResourceType);
	m_pTable->AddParam(szP1, GetRecord()->f_ResourceType);
}

//------------------------------------------------------------------------------
void TRResourceTypes::OnPrepareQuery()
{
	m_pTable->SetParamValue(szP1, m_ResourceType);
}

//------------------------------------------------------------------------------
BOOL TRResourceTypes::IsEmptyQuery()
{
	return m_ResourceType.IsEmpty();
}

//------------------------------------------------------------------------------
TableReader::FindResult TRResourceTypes::FindRecord(const DataStr& aResourceType)
{
	m_ResourceType = aResourceType;

	return TableReader::FindRecord();
}

/////////////////////////////////////////////////////////////////////////////
//	RowSetReader		###  RRResourcesByType ###			RRResourcesByType
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(RRResourcesByType, RowsetReader)

//------------------------------------------------------------------------------
RRResourcesByType::RRResourcesByType(CAbstractFormDoc*	pDocument/* NULL */)
	:
	RowsetReader(RUNTIME_CLASS(TResources), pDocument)
{
}

//------------------------------------------------------------------------------
void RRResourcesByType::OnDefineQuery()
{
	m_pTable->SelectAll();
	m_pTable->AddFilterColumn(GetRecord()->f_ResourceType);
	m_pTable->AddParam(szP1, GetRecord()->f_ResourceType);
}

//------------------------------------------------------------------------------
void RRResourcesByType::OnPrepareQuery()
{
	m_pTable->SetParamValue(szP1, m_ResourceType);
}

//------------------------------------------------------------------------------
BOOL RRResourcesByType::IsEmptyQuery()
{
	return m_ResourceType.IsEmpty();
}

//------------------------------------------------------------------------------
TableReader::FindResult RRResourcesByType::FindRecord(const DataStr& aResourceType)
{
	m_ResourceType = aResourceType;

	return TableReader::FindRecord();
}
