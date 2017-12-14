#include "stdafx.h"  

#include "TWorkers.h"
#include "TResources.h"
#include "RMEnums.h"

#include <TbOleDb\lentbl.h>

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
static TCHAR szParamWorkerID			[]	= _T("p10");

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TWorkers
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TWorkers, SqlRecord) 

//-----------------------------------------------------------------------------
TWorkers::TWorkers(BOOL bCallInit)
	:
	SqlRecord 	(GetStaticName()),	
	f_Gender	(E_GENDER_DEFAULT) 
{                                                    				
	f_CostCenter.		SetUpperCase();
	f_DomicilyISOCode.	SetUpperCase();
	f_FederalState.		SetUpperCase();

	BindRecord();	
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TWorkers::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("WorkerID"),				f_WorkerID);
		BIND_DATA	(_NS_FLD("LastName"),				f_LastName);
		BIND_DATA	(_NS_FLD("Name"),					f_Name);
		BIND_DATA	(_NS_FLD("Title"),					f_Title);
		BIND_DATA	(_NS_FLD("Gender"),					f_Gender);
		BIND_DATA	(_NS_FLD("DomicilyAddress"),		f_DomicilyAddress);
		BIND_DATA	(_NS_FLD("DomicilyCity"),			f_DomicilyCity);
		BIND_DATA	(_NS_FLD("DomicilyCounty"),			f_DomicilyCounty); 
		BIND_DATA	(_NS_FLD("DomicilyZip"),			f_DomicilyZip); 
		BIND_DATA	(_NS_FLD("DomicilyCountry"),		f_DomicilyCountry); 
		BIND_DATA	(_NS_FLD("DomicilyFC"),				f_DomicilyFC);
		BIND_DATA	(_NS_FLD("DomicilyISOCode"),		f_DomicilyISOCode);
		BIND_DATA	(_NS_FLD("Telephone1"),				f_Telephone1);
		BIND_DATA	(_NS_FLD("Telephone2"),				f_Telephone2);
		BIND_DATA	(_NS_FLD("Telephone3"),				f_Telephone3);
		BIND_DATA	(_NS_FLD("Telephone4"),				f_Telephone4);
		BIND_DATA	(_NS_FLD("Email"),					f_Email);
		BIND_DATA	(_NS_FLD("URL"),					f_URL);
		BIND_DATA	(_NS_FLD("SkypeID"),				f_SkypeID);
		BIND_DATA	(_NS_FLD("CostCenter"),				f_CostCenter);
		BIND_DATA	(_NS_FLD("HourlyCost"),				f_HourlyCost);
		BIND_DATA	(_NS_FLD("Notes"),					f_Notes);
		BIND_DATA	(_NS_FLD("DateOfBirth"),			f_DateOfBirth);
		BIND_DATA	(_NS_FLD("CityOfBirth"),			f_CityOfBirth);
		BIND_DATA	(_NS_FLD("CivilStatus"),			f_CivilStatus); 
		BIND_DATA	(_NS_FLD("RegisterNumber"),			f_RegisterNumber);
		BIND_DATA	(_NS_FLD("EmploymentDate"),			f_EmploymentDate);
		BIND_DATA	(_NS_FLD("ResignationDate"),		f_ResignationDate); 
		BIND_DATA	(_NS_FLD("ImagePath"),				f_ImagePath); 
		BIND_DATA	(_NS_FLD("Disabled"),				f_Disabled);
		BIND_DATA	(_NS_FLD("HideOnLayout"),			f_HideOnLayout);
		BIND_DATA	(_NS_FLD("Password"),				f_Password);
		BIND_DATA	(_NS_FLD("PasswordMustBeChanged"),	f_PasswordMustBeChanged);
		BIND_DATA	(_NS_FLD("PasswordCannotChange"),	f_PasswordCannotChange);
		BIND_DATA	(_NS_FLD("PasswordNeverExpire"),	f_PasswordNeverExpire);
		BIND_DATA	(_NS_FLD("PasswordNotRenewable"),	f_PasswordNotRenewable);
		BIND_DATA	(_NS_FLD("PasswordExpirationDate"), f_PasswordExpirationDate);
		BIND_DATA	(_NS_FLD("PasswordAttemptsNumber"),	f_PasswordAttemptsNumber);
		BIND_DATA	(_NS_FLD("CompanyLogin"),			f_CompanyLogin); 
		BIND_DATA	(_NS_FLD("Latitude"),				f_Latitude);
		BIND_DATA	(_NS_FLD("Longitude"),				f_Longitude);
		BIND_DATA	(_NS_FLD("PIN"),					f_PIN);
		BIND_DATA	(_NS_FLD("Branch"),					f_Branch);
		BIND_DATA	(_NS_FLD("Address2"),				f_Address2);
		BIND_DATA	(_NS_FLD("StreetNo"),				f_StreetNo);
		BIND_DATA	(_NS_FLD("District"),				f_District);
		BIND_DATA	(_NS_FLD("FederalState"),			f_FederalState);
		BIND_DATA	(_NS_FLD("IsRSEnabled"),			f_IsRSEnabled);

		LOCAL_STR	(_NS_LFLD("WorkerDesc"),			l_WorkerDesc, LEN_RM_DESCRIPTION);
		LOCAL_STR	(_NS_FLD("NameComplete"),			l_NameComplete, LEN_RM_DESCRI_MULTILINE);
		LOCAL_STR	(_NS_FLD("NameCompleteWithLastNameFirst"), l_NameCompleteWithLastNameFirst, LEN_RM_DESCRI_MULTILINE);

	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TWorkers::GetStaticName() { return _NS_TBL("RM_Workers"); }

/////////////////////////////////////////////////////////////////////////////
//    Hotlink          ### HKLWorkers ###                       HKLWorkers
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (HKLWorkers, HotKeyLink)

//------------------------------------------------------------------------------
HKLWorkers::HKLWorkers() 
	: 
	HotKeyLink(RUNTIME_CLASS(TWorkers), _NS_DOC("Framework.TbResourcesMng.TbResourcesMng.Workers")),
	m_SelectionDisabled	(ACTIVE)
{
	m_WorkerID = 0;
}

//------------------------------------------------------------------------------
BOOL HKLWorkers::Customize(const DataObjArray& params)
{
	ASSERT(params.GetSize() == 1);
	ASSERT(params[0]->IsKindOf(RUNTIME_CLASS(DataInt)));

	DataInt Filter;
	Filter.Assign(*params[0]);

	if (Filter == 0)
		SetSelDisabled(ACTIVE);
	else
	{
		if (Filter == 1)
			SetSelDisabled(DISABLED);
		else
			SetSelDisabled(BOTH);
	}
	return TRUE;
}

//------------------------------------------------------------------------------
void HKLWorkers::OnPrepareAuxData() 
{	
	GetRecord()->l_NameComplete = GetNameComplete();
	GetRecord()->l_NameCompleteWithLastNameFirst = GetNameComplete(FALSE);

	__super::OnPrepareAuxData();
}

//------------------------------------------------------------------------------
HotKeyLink::FindResult HKLWorkers::FindRecord(DataObj* pData, BOOL bCallLink, BOOL bFromControl, BOOL bAllowRunningModeForInternalUse )
{
	FindResult res = __super::FindRecord(pData, bCallLink, bFromControl, bAllowRunningModeForInternalUse);
	GetRecord()->l_WorkerDesc = GetNameComplete();
	return res;
}
//------------------------------------------------------------------------------
void HKLWorkers::OnDefineQuery (SelectionType nQuerySelection)
{
	TResourcesDetails aRecResource;
	aRecResource.SetQualifier();

	m_pTable->SelectAll();
	ASSERT(GetDataObj()->IsKindOf(RUNTIME_CLASS(DataLng)));

	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->AddFilterColumn    (GetRecord()->f_WorkerID);
			m_pTable->AddParam           (szP1, GetRecord()->f_WorkerID);
			if (!m_ResourceType.IsEmpty()) m_pTable->m_strFilter += _T(" AND");
			break;

		case UPPER_BUTTON:
			m_pTable->AddFilterColumn    (GetRecord()->f_WorkerID);
			m_pTable->AddParam           (szP1, GetRecord()->f_WorkerID);

			DefineDisabled();

			if (!m_ResourceType.IsEmpty()) m_pTable->m_strFilter += _T(" AND");
			break;
		
		case COMBO_ACCESS:
		case LOWER_BUTTON:
			m_pTable->AddSortColumn      (GetRecord()->f_LastName);
			DefineDisabled();
			if (!m_ResourceType.IsEmpty() && m_SelectionDisabled != BOTH ) m_pTable->m_strFilter += _T(" AND");
			break;
	}

	if (!m_ResourceType.IsEmpty())
	{
		m_pTable->AddParam(szP3, aRecResource.f_ResourceType);
		m_pTable->AddParam(szP4, aRecResource.f_ResourceCode);

		m_pTable->m_strFilter += cwsprintf
		(
			_T(" %s IN (SELECT %s FROM %s WHERE %s = ? AND %s = ?) "), 
			(LPCTSTR) m_pTable->GetColumnName(&GetRecord()->f_WorkerID),
			(LPCTSTR) aRecResource.GetQualifiedColumnName(&aRecResource.f_ChildWorkerID),
			(LPCTSTR) aRecResource.GetTableName(),
			(LPCTSTR) aRecResource.GetQualifiedColumnName(&aRecResource.f_ResourceType),
			(LPCTSTR) aRecResource.GetQualifiedColumnName(&aRecResource.f_ResourceCode)
		); 
	}
}

//------------------------------------------------------------------------------
void HKLWorkers::OnPrepareQuery (DataObj* pDataObj, SelectionType nQuerySelection)
{
	if (m_WorkerID == 0)
		ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataLng)));
		
	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			if (m_WorkerID != 0)
				m_pTable->SetParamValue(szP1, m_WorkerID);
			else
				m_pTable->SetParamValue(szP1, *pDataObj);
			m_WorkerID = 0;
			break;

		case UPPER_BUTTON:
			if (m_WorkerID != 0)
				m_pTable->SetParamValue(szP1, m_WorkerID);
			else
				m_pTable->SetParamValue(szP1, *pDataObj);
			m_WorkerID = 0;

			PrepareDisabled();
			break;
				  
		case COMBO_ACCESS:
		case LOWER_BUTTON:
			PrepareDisabled();
			break;
	}

	if (!m_ResourceType.IsEmpty())
	{
		m_pTable->SetParamValue	(szP3, m_ResourceType);
		m_pTable->SetParamValue	(szP4, m_ResourceCode);
	}
}

//------------------------------------------------------------------------------
void HKLWorkers::SetResource(const DataStr& aResourceType, const DataStr& aResourceCode) 
{ 
	CloseTable();
	m_ResourceType = aResourceType; 
	m_ResourceCode = aResourceCode;
}

//------------------------------------------------------------------------------
void HKLWorkers::DefineDisabled()
{ 
	switch(m_SelectionDisabled)
	{
		case ACTIVE:
		case DISABLED: 
			m_pTable->AddFilterColumn	(GetRecord()->f_Disabled);
			m_pTable->AddParam			(szP2, GetRecord()->f_Disabled);
			break;  
	}               
}

//------------------------------------------------------------------------------
void HKLWorkers::PrepareDisabled()
{
	DataBool bDisabled;

	switch(m_SelectionDisabled) 
	{
		case ACTIVE: 
			bDisabled = FALSE;
			m_pTable->SetParamValue(szP2, bDisabled);
			break;

		case DISABLED: 
			bDisabled = TRUE;
			m_pTable->SetParamValue(szP2, bDisabled);
			break;
	}               
}

//------------------------------------------------------------------------------
BOOL HKLWorkers::IsValid()
{
	if (!HotKeyLink::IsValid())
		return FALSE;
	
	// se il filtro non viene settato il record risulta sempre valido
	switch	(m_SelectionDisabled)
	{
		case ACTIVE:	return SetErrorString(!GetRecord()->f_Disabled ? _T("") : cwsprintf(_TB("Worker {0-%s} is disabled. It is necessary to select an enable code."),(LPCTSTR)GetRecord()->f_LastName.GetString()));
		case DISABLED:	return SetErrorString(GetRecord()->f_Disabled ? _T("") : cwsprintf(_TB("Worker {0-%s} is enabled. It is necessary to select a disable code."),(LPCTSTR)GetRecord()->f_LastName.GetString()));
	}
	
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL HKLWorkers::ExistData (DataObj* pDataObj)
{
	if (pDataObj && pDataObj->IsEmpty())
		return TRUE;

	return HotKeyLink::ExistData(pDataObj);
}

//------------------------------------------------------------------------------ 
DataStr HKLWorkers::GetNameComplete(BOOL bNameFirst /* TRUE */) const
{ 
	CString strNameComplete;

	if (bNameFirst)
		strNameComplete = GetRecord()->f_Name + _T(" ") + GetRecord()->f_LastName;
	else 
		strNameComplete = GetRecord()->f_LastName + _T(" ") + GetRecord()->f_Name;

	return strNameComplete.Trim();
}

//------------------------------------------------------------------------------ 
CString HKLWorkers::GetHKLDescription () const 
{
	return GetNameComplete();
}

//------------------------------------------------------------------------------ 
DataStr HKLWorkers::GetAddressComplete() const
{ 
	DataStr aAddressComplete =
		GetRecord()->f_DomicilyAddress + _T("\n") + GetRecord()->f_DomicilyZip; 
																 
	if (!GetRecord()->f_DomicilyCity.IsEmpty())
		aAddressComplete += _T("-")  +  GetRecord()->f_DomicilyCity;
											   
	if (!GetRecord()->f_DomicilyCountry.IsEmpty())    
		aAddressComplete += _T(" (") +  GetRecord()->f_DomicilyCountry + _T(")");   

	  return aAddressComplete;
} 

/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRWorkers	
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (TRWorkers, TableReader)

//------------------------------------------------------------------------------
TRWorkers::TRWorkers(CAbstractFormDoc* pDocument /* NULL */)
	: 
	TableReader(RUNTIME_CLASS(TWorkers), pDocument)
{
}

//------------------------------------------------------------------------------
void TRWorkers::OnDefineQuery ()
{   
	TWorkers* pRec = GetRecord();
	m_pTable->SelectAll	();
	
	m_pTable->AddParam			(szP1, pRec->f_WorkerID);
	m_pTable->AddFilterColumn	(pRec->f_WorkerID);
}
	 
//------------------------------------------------------------------------------
void TRWorkers::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szP1, m_WorkerID);
}

//------------------------------------------------------------------------------ 
DataStr TRWorkers::GetNameComplete() 
{ 
	CString strNameComplete;

//	if (bNameFirst)
		strNameComplete = GetRecord()->f_Name + _T(" ") + GetRecord()->f_LastName;
	//else 
	//	strNameComplete = GetRecord()->f_LastName + _T(" ") + GetRecord()->f_Name;

	return strNameComplete.Trim();
}
//------------------------------------------------------------------------------
BOOL TRWorkers::IsEmptyQuery()
{
	return m_WorkerID.IsEmpty();
}

//------------------------------------------------------------------------------
TableReader::FindResult TRWorkers::FindRecord(DataLng& aWorkerID)
{
	m_WorkerID = aWorkerID;
	return TableReader::FindRecord();
}

//------------------------------------------------------------------------------
DataStr TRWorkers::GetWorker()
{
	return GetRecord()->f_Name + _T(" ") + GetRecord()->f_LastName;
}

/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRWorkersByLogin	
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TRWorkersByLogin, TableReader)

//------------------------------------------------------------------------------
TRWorkersByLogin::TRWorkersByLogin(CAbstractFormDoc* pDocument /* NULL */)
	:
	TableReader(RUNTIME_CLASS(TWorkers), pDocument),
	m_WorkerID(0)
{
}

//------------------------------------------------------------------------------
void TRWorkersByLogin::OnDefineQuery()
{
	TWorkers* pRec = GetRecord();

	m_pTable->SelectAll();

	m_pTable->AddParam(szP1, pRec->f_CompanyLogin);
	m_pTable->AddParam(szP3, pRec->f_WorkerID);

	m_pTable->AddFilterColumn(pRec->f_CompanyLogin);
	m_pTable->AddFilterColumn(pRec->f_WorkerID, _T("<>"));
}

//------------------------------------------------------------------------------
void TRWorkersByLogin::OnPrepareQuery()
{
	m_pTable->SetParamValue(szP1, m_CompanyLogin);
	m_pTable->SetParamValue(szP3, m_WorkerID);
}

//------------------------------------------------------------------------------
BOOL TRWorkersByLogin::IsEmptyQuery()
{
	return m_CompanyLogin.IsEmpty();
}

//------------------------------------------------------------------------------
TableReader::FindResult TRWorkersByLogin::FindRecord(DataStr& aCompanyLogin)
{
	m_CompanyLogin = aCompanyLogin;
	return TableReader::FindRecord();
}


/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRWorkersByName	
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (TRWorkersByName, TableReader)

//------------------------------------------------------------------------------
TRWorkersByName::TRWorkersByName(CAbstractFormDoc* pDocument /* NULL */)
	: 
	TableReader(RUNTIME_CLASS(TWorkers), pDocument),
	m_WorkerID(0)
{
}

//------------------------------------------------------------------------------
void TRWorkersByName::OnDefineQuery ()
{   
	TWorkers* pRec = GetRecord();

	m_pTable->SelectAll();
	
	m_pTable->AddParam			(szP1, pRec->f_Name);
	m_pTable->AddParam			(szP2, pRec->f_LastName);
	m_pTable->AddParam			(szP3, pRec->f_WorkerID);

	m_pTable->AddFilterColumn	(pRec->f_Name);
	m_pTable->AddFilterColumn	(pRec->f_LastName);
	m_pTable->AddFilterColumn	(pRec->f_WorkerID, _T("<>"));
}
	 
//------------------------------------------------------------------------------
void TRWorkersByName::OnPrepareQuery ()
{
	m_pTable->SetParamValue	(szP1, m_Name);
	m_pTable->SetParamValue	(szP2, m_LastName);
	m_pTable->SetParamValue	(szP3, m_WorkerID);
}

//------------------------------------------------------------------------------
BOOL TRWorkersByName::IsEmptyQuery()
{
	return (m_Name.IsEmpty() && m_LastName.IsEmpty());
}

//------------------------------------------------------------------------------
TableReader::FindResult TRWorkersByName::FindRecord(DataStr& aName, DataStr& aLastName)
{
	m_Name		= aName;
	m_LastName	= aLastName;
	return TableReader::FindRecord();
}

/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRWorkersByPIN	
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (TRWorkersByPIN, TableReader)

//------------------------------------------------------------------------------
TRWorkersByPIN::TRWorkersByPIN(CAbstractFormDoc* pDocument /* NULL */)
	: 
	TableReader(RUNTIME_CLASS(TWorkers), pDocument)
{
}

//------------------------------------------------------------------------------
void TRWorkersByPIN::OnDefineQuery ()
{   
	TWorkers* pRec = GetRecord();

	m_pTable->SelectAll();
	
	m_pTable->AddParam			(szP1, pRec->f_PIN);
	m_pTable->AddParam			(szP2, pRec->f_WorkerID);
	m_pTable->AddFilterColumn	(pRec->f_PIN);
	m_pTable->AddFilterColumn	(pRec->f_WorkerID, _T("<>"));
}
		 
//------------------------------------------------------------------------------
void TRWorkersByPIN::OnPrepareQuery ()
{
	m_pTable->SetParamValue	(szP1, m_PIN);
	m_pTable->SetParamValue	(szP2, m_WorkerID);
}

//------------------------------------------------------------------------------
BOOL TRWorkersByPIN::IsEmptyQuery()
{
	return (m_PIN.IsEmpty());
}

//------------------------------------------------------------------------------
TableReader::FindResult TRWorkersByPIN::FindRecord(DataStr& aPIN)
{
	m_PIN  = aPIN;
	return TableReader::FindRecord();
}

/////////////////////////////////////////////////////////////////////////////
//		TableUpdater :	class TUWorkers
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (TUWorkers, TableUpdater)

//------------------------------------------------------------------------------
TUWorkers::TUWorkers
	(
		CAbstractFormDoc* 	pDocument,	// = NULL
		CMessages* 			pMessages	// = NULL
	)													
	: 
	TableUpdater(RUNTIME_CLASS(TWorkers), pDocument, pMessages)
{
}

//------------------------------------------------------------------------------
void TUWorkers::OnDefineQuery ()
{
	TWorkers* pRec = GetRecord();
	m_pTable->SelectAll	();
	
	m_pTable->AddParam			(szP1, pRec->f_WorkerID);
	m_pTable->AddFilterColumn	(pRec->f_WorkerID);
}
	
//------------------------------------------------------------------------------
void TUWorkers::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szP1, m_WorkerID);
}

//------------------------------------------------------------------------------
BOOL TUWorkers::IsEmptyQuery()
{
	return m_WorkerID.IsEmpty();
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUWorkers::FindRecord(DataLng& aWorkerID, BOOL	bLock)
{
	m_WorkerID = aWorkerID;
	return TableUpdater::FindRecord(bLock);
}                                                                           

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TWorkersDetails
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TWorkersDetails, SqlRecord)

//-----------------------------------------------------------------------------
TWorkersDetails::TWorkersDetails(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	f_ChildResourceType.SetUpperCase();
	f_ChildResourceCode.SetUpperCase();

	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TWorkersDetails::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	(_NS_FLD("WorkerId"),			f_WorkerID);
		BIND_DATA	(_NS_FLD("IsWorker"),			f_IsWorker);
		BIND_DATA	(_NS_FLD("ChildResourceType"),	f_ChildResourceType);
		BIND_DATA	(_NS_FLD("ChildResourceCode"),	f_ChildResourceCode);
		BIND_DATA	(_NS_FLD("ChildWorkerId"),		f_ChildWorkerID);

		LOCAL_STR	(_NS_LFLD("WorkerDesc"),		l_WorkerDesc,	LEN_RM_DESCRIPTION);
		LOCAL_STR	(_NS_LFLD("ManagerDesc"),		l_ManagerDesc,	LEN_RM_DESCRIPTION);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TWorkersDetails::GetStaticName() { return _NS_TBL("RM_WorkersDetails"); }

/////////////////////////////////////////////////////////////////////////////
//		TableUpdater :	class TUWorkersDetails
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TUWorkersDetails, TableUpdater)

//------------------------------------------------------------------------------
TUWorkersDetails::TUWorkersDetails(CAbstractFormDoc* pDocument /* = NULL*/, CMessages* pMessages /* = NULL */)
	:
	TableUpdater(RUNTIME_CLASS(TWorkersDetails), pDocument, pMessages)
{
}

//------------------------------------------------------------------------------
void TUWorkersDetails::OnDefineQuery()
{
	m_pTable->SelectAll();
	m_pTable->AddFilterColumn(GetRecord()->f_WorkerID);
	m_pTable->AddFilterColumn(GetRecord()->f_IsWorker);
	m_pTable->AddFilterColumn(GetRecord()->f_ChildResourceType);
	m_pTable->AddFilterColumn(GetRecord()->f_ChildResourceCode);
	m_pTable->AddFilterColumn(GetRecord()->f_ChildWorkerID);

	m_pTable->AddParam(szParamWorkerID, GetRecord()->f_WorkerID);
	m_pTable->AddParam(szParamIsWorker, GetRecord()->f_IsWorker);
	m_pTable->AddParam(szParamChildResourceType, GetRecord()->f_ChildResourceType);
	m_pTable->AddParam(szParamChildResourceCode, GetRecord()->f_ChildResourceCode);
	m_pTable->AddParam(szParamChildWorkerID, GetRecord()->f_ChildWorkerID);
}

//------------------------------------------------------------------------------
void TUWorkersDetails::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamWorkerID, m_WorkerID);
	m_pTable->SetParamValue(szParamIsWorker, m_IsWorker);
	m_pTable->SetParamValue(szParamChildResourceType, m_ChildResourceType);

	if (m_ChildWorkerID.IsEmpty())
	{
		m_pTable->SetParamValue(szParamChildResourceCode, m_ChildResourceCode);
		m_pTable->SetParamValue(szParamChildWorkerID, DataLng(0));
	}
	else
	{
		m_pTable->SetParamValue(szParamChildResourceCode, DataStr(_T("")));
		m_pTable->SetParamValue(szParamChildWorkerID, m_ChildWorkerID);
	}
}

//------------------------------------------------------------------------------
BOOL TUWorkersDetails::IsEmptyQuery()
{
	return m_WorkerID.IsEmpty();
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUWorkersDetails::FindRecord(	const DataLng&	aWorkerID,
														const DataBool&	aIsWorker,
														const DataStr&	aChildResourceType,
														const DataStr&	aChildResourceCode,
														const DataLng&	aChildWorkerID,
														BOOL			bLock)
{
	m_WorkerID = aWorkerID;
	m_IsWorker = aIsWorker;
	m_ChildResourceType = aChildResourceType;
	m_ChildResourceCode = aChildResourceCode;
	m_ChildWorkerID = aChildWorkerID;

	return TableUpdater::FindRecord(bLock);
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUWorkersDetails::FindRecord(	const DataLng&	aWorkerID,
														const DataStr&	aChildResourceType,
														const DataStr&	aChildResourceCode,
														BOOL			bLock)
{
	m_WorkerID = aWorkerID;
	m_IsWorker = FALSE;
	m_ChildResourceType = aChildResourceType;
	m_ChildResourceCode = aChildResourceCode;
	m_ChildWorkerID = DataLng(0);

	return TableUpdater::FindRecord(bLock);
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUWorkersDetails::FindRecord(	const DataLng&	aWorkerID,
														const DataLng&	aChildWorkerID,
														BOOL			bLock)
{
	m_WorkerID = aWorkerID;
	m_IsWorker = TRUE;
	m_ChildResourceType = _T("");
	m_ChildResourceCode = DataStr(_T(""));
	m_ChildWorkerID = aChildWorkerID;

	return TableUpdater::FindRecord(bLock);
}

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord			 TWorkersFields
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TWorkersFields, SqlRecord) 

//-----------------------------------------------------------------------------
TWorkersFields::TWorkersFields(BOOL bCallInit)
	:
	SqlRecord	(GetStaticName()),
	f_WorkerID	(0)	
{
	BindRecord();	
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TWorkersFields::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("WorkerID"),		f_WorkerID);
		BIND_DATA	(_NS_FLD("Line"),			f_Line);
		BIND_DATA	(_NS_FLD("FieldName"),		f_FieldName);
		BIND_DATA	(_NS_FLD("FieldValue"),		f_FieldValue); 
		BIND_DATA	(_NS_FLD("Notes"),			f_Notes);
		BIND_DATA	(_NS_FLD("HideOnLayout"),	f_HideOnLayout);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TWorkersFields::GetStaticName() { return _NS_TBL("RM_WorkersFields"); }

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord			TWorkersArrangement
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TWorkersArrangement, SqlRecord) 

//-----------------------------------------------------------------------------
TWorkersArrangement::TWorkersArrangement(BOOL bCallInit)
	:
	SqlRecord	(GetStaticName()),
	f_WorkerID	(0)	
{
	f_Arrangement.SetUpperCase(); 

	BindRecord();	
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TWorkersArrangement::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("WorkerID"),			f_WorkerID);
		BIND_DATA	(_NS_FLD("Line"),				f_Line);
		BIND_DATA	(_NS_FLD("Arrangement"),		f_Arrangement);
		BIND_DATA	(_NS_FLD("ArrangementLevel"),	f_ArrangementLevel); 
		BIND_DATA	(_NS_FLD("BasicPay"),			f_BasicPay);
		BIND_DATA	(_NS_FLD("TotalPay"),			f_TotalPay);
		BIND_DATA	(_NS_FLD("FromDate"),			f_FromDate);
		BIND_DATA	(_NS_FLD("ToDate"),				f_ToDate);
		BIND_DATA	(_NS_FLD("Notes"),				f_Notes);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TWorkersArrangement::GetStaticName() { return _NS_TBL("RM_WorkersArrangements"); }

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord			 TWorkersAbsences
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TWorkersAbsences, SqlRecord)

//-----------------------------------------------------------------------------
TWorkersAbsences::TWorkersAbsences(BOOL bCallInit)
	:
	SqlRecord	(GetStaticName()),
	f_WorkerID	(0)	
{
	f_Reason.		SetUpperCase(); 
	f_StartingDate.	SetFullDate();
	f_EndingDate.	SetFullDate();

	BindRecord();	
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TWorkersAbsences::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("WorkerID"),		f_WorkerID);
		BIND_DATA	(_NS_FLD("Reason"),			f_Reason);
		BIND_DATA	(_NS_FLD("StartingDate"),	f_StartingDate);
		BIND_DATA	(_NS_FLD("EndingDate"),		f_EndingDate); 
		BIND_DATA	(_NS_FLD("Manager"),		f_Manager);
		BIND_DATA	(_NS_FLD("Notes"),			f_Notes);

		LOCAL_STR	(_NS_LFLD("ManagerDesc"),	l_ManagerDes,	LEN_RM_DESCRIPTION);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TWorkersAbsences::GetStaticName() { return _NS_TBL("RM_WorkersAbsences"); }

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TArrangements
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TArrangements, SqlRecord) 

//-----------------------------------------------------------------------------
TArrangements::TArrangements(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	f_Arrangements.SetUpperCase(); 
	f_WorkingHours.SetAsTime();

	BindRecord();	
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TArrangements::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("Arrangement"),		f_Arrangements);
		BIND_DATA	(_NS_FLD("Description"),		f_Description);
		BIND_DATA	(_NS_FLD("ArrangementLevel"),	f_ArrangementLevel);
		BIND_DATA	(_NS_FLD("BasicPay"),			f_BasicPay);
		BIND_DATA	(_NS_FLD("TotalPay"),			f_TotalPay);
		BIND_DATA	(_NS_FLD("WorkingHours"),		f_WorkingHours);
		BIND_DATA	(_NS_FLD("Notes"), 				f_Notes);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TArrangements::GetStaticName() { return _NS_TBL("RM_Arrangements"); }

////////////////////////////////////////////////////////////////////////////
//	Hotlink					### Arrangements ###			Arrangements
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (HKLArrangements, HotKeyLink)

//------------------------------------------------------------------------------
HKLArrangements::HKLArrangements() 
	: 
	HotKeyLink(RUNTIME_CLASS(TArrangements), _NS_DOC("Framework.TbResourcesMng.TbResourcesMng.Arrangements"))
{
}
	
//------------------------------------------------------------------------------
void HKLArrangements::OnDefineQuery (SelectionType nQuerySelection)
{
	m_pTable->SelectAll();

	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->AddFilterColumn	(GetRecord()->f_Arrangements);
			m_pTable->AddParam			(szP1, GetRecord()->f_Arrangements);
			break;
			
		case UPPER_BUTTON:
			m_pTable->AddSortColumn		(GetRecord()->f_Arrangements);
			m_pTable->AddFilterLike		(GetRecord()->f_Arrangements);
			m_pTable->AddParam			(szP1, GetRecord()->f_Arrangements);
			break;

		case LOWER_BUTTON:
			m_pTable->AddSortColumn		(GetRecord()->f_Description);
			m_pTable->AddFilterLike		(GetRecord()->f_Description);
			m_pTable->AddParam			(szP1, GetRecord()->f_Description);
			break;
	}
}

//------------------------------------------------------------------------------
void HKLArrangements::OnPrepareQuery (DataObj* pDataObj, SelectionType nQuerySelection)
{
	ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataStr)));

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

	if (!GetRecord())
		return;
}

//----------------------------------------------------------------------------
//[TBWebMethod(name = GetLoggedWorkerID )]
///<summary>
///Logged Worker's ID
///</summary>
///<returns>Return currently logged Worker's ID</returns>
DataLng GetLoggedWorkerID()
{
	return AfxGetWorkerId();
}

//----------------------------------------------------------------------------
//[TBWebMethod(name = GetWorkerName )]
///<summary>
///Name of Worker
///</summary>
///<param name="WorkerID">The ID of Worker</param>
///<returns>Return name and last name concatenated</returns>
DataStr GetWorkerName(DataLng WorkerID)
{
	TRWorkers aTRWorkers(NULL);
	aTRWorkers.SetSqlSession(AfxGetDefaultSqlSession());
	if (WorkerID.IsEmpty())
		 WorkerID = AfxGetWorkerId();
	aTRWorkers.FindRecord(WorkerID);
	return aTRWorkers.GetWorker();
}

//----------------------------------------------------------------------------
//[TBWebMethod(name = GetLoggedWorkerName )]
///<summary>
///Name of Worker
///</summary>
///<param name="WorkerID">The ID of Worker</param>
///<returns>Return name and last name concatenated</returns>
DataStr GetLoggedWorkerName()
{
	TRWorkers aTRWorkers(NULL);
	aTRWorkers.SetSqlSession(AfxGetDefaultSqlSession());
	aTRWorkers.FindRecord(DataLng(AfxGetWorkerId()));
	return aTRWorkers.GetWorker();
}
