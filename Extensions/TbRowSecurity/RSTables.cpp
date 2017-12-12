
#include "stdafx.h" 

#include "TBRowSecurityEnums.h"
#include "RSTables.h"

// Parameters
static TCHAR szP1[]	= _T("P1");
static TCHAR szP2[]	= _T("P2");
static TCHAR szP3[]	= _T("P3");
static TCHAR szP4[]	= _T("P4");

static TCHAR szParamResourceType[]	= _T("p2");
static TCHAR szParamResourceCode[]	= _T("p3");
static TCHAR szParamIsWorker[]		= _T("p4");
static TCHAR szParamWorkerID[]		= _T("p5");
static TCHAR szParamOfficeID[]		= _T("p6");

//=============================================================================
//							TRS_Configuration
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TRS_Configuration, SqlRecord)

//-----------------------------------------------------------------------------
TRS_Configuration::TRS_Configuration(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TRS_Configuration::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	(_NS_FLD("OfficeID"),		f_OfficeID);
		BIND_DATA	(_NS_FLD("UsedEntities"),	f_UsedEntries);
		BIND_DATA	(_NS_FLD("IsValid"),		f_IsValid);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TRS_Configuration::GetStaticName() { return _NS_TBL("RS_Configuration"); }

/////////////////////////////////////////////////////////////////////////////
//		TableUpdater :	class TURS_Configuration
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (TURS_Configuration, TableUpdater)

//------------------------------------------------------------------------------
TURS_Configuration::TURS_Configuration
	(
		CAbstractFormDoc* 	pDocument,	// = NULL
		CMessages* 			pMessages	// = NULL
	)													
	: 
	TableUpdater(RUNTIME_CLASS(TRS_Configuration), (CBaseDocument*)pDocument, pMessages)
{
}

//------------------------------------------------------------------------------
void TURS_Configuration::OnDefineQuery ()
{
	TRS_Configuration* pRec = GetRecord();
	m_pTable->SelectAll();
	
	m_pTable->AddParam			(szParamOfficeID, pRec->f_OfficeID);
	m_pTable->AddFilterColumn	(pRec->f_OfficeID);
}
	
//------------------------------------------------------------------------------
void TURS_Configuration::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szParamOfficeID, m_nOfficeID);
}

//------------------------------------------------------------------------------
BOOL TURS_Configuration::IsEmptyQuery()
{
	return m_nOfficeID < 0; // non uso IsEmpty perche' scarta anche lo zero!
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TURS_Configuration::FindRecord(const DataLng& aOfficeID, BOOL bLock)
{
	m_nOfficeID = aOfficeID;
	return TableUpdater::FindRecord(bLock);
}                                                

//=============================================================================
//							TRS_Subjects
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TRS_Subjects, SqlRecord)

//-----------------------------------------------------------------------------
TRS_Subjects::TRS_Subjects(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	f_ResourceType.SetUpperCase();
	f_ResourceCode.SetUpperCase();

	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------
TCHAR TRS_Subjects::szSubjectID[] = _NS_FLD("SubjectID");

//-----------------------------------------------------------------------------
void TRS_Subjects::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_AUTOINCREMENT	(szSubjectID,				f_SubjectID);
		BIND_DATA			(_NS_FLD("IsWorker"),		f_IsWorker);
		BIND_DATA			(_NS_FLD("ResourceType"),	f_ResourceType);
		BIND_DATA			(_NS_FLD("ResourceCode"),	f_ResourceCode); 
		BIND_DATA			(_NS_FLD("WorkerID"),		f_WorkerID);
		BIND_DATA			(_NS_FLD("Description"),	f_Description);
		// local
		//LOCAL_STR			(_NS_LFLD("Description"),	l_Description,	32);
		LOCAL_STR			(_NS_LFLD("WRLabel"),		l_WRLabel,		20);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TRS_Subjects::GetStaticName() { return _NS_TBL("RS_Subjects"); }

//-----------------------------------------------------------------------------
//CString TRS_Subjects::GetRecordDescription() const
//{
//	CString strTitle;
//	if (f_IsWorker)
//	{
//		CWorker* pWorker = 	AfxGetWorkersTable()->GetWorker(f_WorkerID);
//		strTitle = (pWorker) ? cwsprintf(_T("%s %s"), pWorker->GetName(), pWorker->GetLastName()) : cwsprintf(_T("%d"), f_WorkerID);
//	}
//	else
//		strTitle = cwsprintf(_T("%s %s"), f_ResourceType.Str(), f_ResourceCode.Str());
//	
//	return strTitle;
//}

/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRRS_SubjectsByWorkerID
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TRRS_SubjectsByWorkerID, TableReader)

//------------------------------------------------------------------------------
TRRS_SubjectsByWorkerID::TRRS_SubjectsByWorkerID(CBaseDocument* pDocument /*= NULL*/)
	: 
	TableReader(RUNTIME_CLASS(TRS_Subjects), pDocument)
{}

//------------------------------------------------------------------------------
void TRRS_SubjectsByWorkerID::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn(GetRecord()->f_IsWorker);
	m_pTable->AddFilterColumn(GetRecord()->f_WorkerID);
	m_pTable->AddParam(szParamIsWorker,	GetRecord()->f_IsWorker);
	m_pTable->AddParam(szParamWorkerID,	GetRecord()->f_WorkerID);
}

//------------------------------------------------------------------------------
void TRRS_SubjectsByWorkerID::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamIsWorker, m_IsWorker);
	m_pTable->SetParamValue(szParamWorkerID, m_WorkerID);
}

//------------------------------------------------------------------------------
BOOL TRRS_SubjectsByWorkerID::IsEmptyQuery()
{
	return m_WorkerID.IsEmpty();
}

// FindRecord di un worker
//------------------------------------------------------------------------------
TableReader::FindResult	TRRS_SubjectsByWorkerID::FindRecord(const DataLng& aWorkerID)
{
	m_IsWorker = TRUE;
	m_WorkerID = aWorkerID;
	return TableReader::FindRecord();
}

/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRRS_SubjectsByResource
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TRRS_SubjectsByResource, TableReader)

//------------------------------------------------------------------------------
TRRS_SubjectsByResource::TRRS_SubjectsByResource(CBaseDocument* pDocument /*= NULL*/)
	: 
	TableReader(RUNTIME_CLASS(TRS_Subjects), pDocument)
{}

//------------------------------------------------------------------------------
void TRRS_SubjectsByResource::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn(GetRecord()->f_IsWorker);
	m_pTable->AddFilterColumn(GetRecord()->f_ResourceType);
	m_pTable->AddFilterColumn(GetRecord()->f_ResourceCode);
	m_pTable->AddParam(szParamIsWorker,	GetRecord()->f_IsWorker);
	m_pTable->AddParam(szParamResourceType,	GetRecord()->f_ResourceType);
	m_pTable->AddParam(szParamResourceCode,	GetRecord()->f_ResourceCode);
}

//------------------------------------------------------------------------------
void TRRS_SubjectsByResource::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamIsWorker, m_IsWorker);
	m_pTable->SetParamValue(szParamResourceType, m_ResourceType);
	m_pTable->SetParamValue(szParamResourceCode, m_ResourceCode);
}

//------------------------------------------------------------------------------
BOOL TRRS_SubjectsByResource::IsEmptyQuery()
{
	return (m_ResourceType.IsEmpty() && m_ResourceCode.IsEmpty());
}

// FindRecord di una risorsa
//------------------------------------------------------------------------------
TableReader::FindResult TRRS_SubjectsByResource::FindRecord(const DataStr& aResourceCode, const DataStr& aResourceType)
{
	m_IsWorker = FALSE;
	m_ResourceCode = aResourceCode;
	m_ResourceType = aResourceType;
	
	return TableReader::FindRecord();
}

//=============================================================================
//							TRS_SubjectsHierarchy
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TRS_SubjectsHierarchy, SqlRecord)

//-----------------------------------------------------------------------------
TRS_SubjectsHierarchy::TRS_SubjectsHierarchy(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------
TCHAR TRS_SubjectsHierarchy::szMasterSubjectID[] = _NS_FLD("MasterSubjectID");
TCHAR TRS_SubjectsHierarchy::szSlaveSubjectID[] = _NS_FLD("SlaveSubjectID");

//-----------------------------------------------------------------------------
void TRS_SubjectsHierarchy::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA			(szMasterSubjectID,		f_MasterSubjectID);
		BIND_DATA			(szSlaveSubjectID,		f_SlaveSubjectID); 
		BIND_DATA			(_NS_FLD("NrLevel"),	f_NrLevel); 
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TRS_SubjectsHierarchy::GetStaticName() { return _NS_TBL("RS_SubjectsHierarchy"); }

//=============================================================================
//							TRS_TmpOldHierarchies
//=============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TRS_TmpOldHierarchies, SqlRecord)

//-----------------------------------------------------------------------------
TRS_TmpOldHierarchies::TRS_TmpOldHierarchies(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TRS_TmpOldHierarchies::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	(_NS_FLD("MasterSubjectID"),	f_MasterSubjectID);
		BIND_DATA	(_NS_FLD("SlaveSubjectID"),		f_SlaveSubjectID); 
		BIND_DATA	(_NS_FLD("NrLevel"),			f_NrLevel); 
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TRS_TmpOldHierarchies::GetStaticName() { return _NS_TBL("RS_TmpOldHierarchies"); }

//=============================================================================
//							TRS_SubjectsGrants
//=============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TRS_SubjectsGrants, SqlRecord)

//-----------------------------------------------------------------------------
TRS_SubjectsGrants::TRS_SubjectsGrants(BOOL bCallInit)
	:
	SqlRecord(GetStaticName()),
	f_GrantType(E_GRANT_TYPE_DEFAULT),
	f_Inherited(FALSE),
	f_IsImplicit(FALSE)
{
	f_EntityName.SetUpperCase();
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------
const CString&	TRS_SubjectsGrants::s_sSubjectID		= _NS_FLD("SubjectID");
const CString&	TRS_SubjectsGrants::s_sEntityName		= _NS_FLD("EntityName");
const CString&	TRS_SubjectsGrants::s_sRowSecurityID	= _NS_FLD("RowSecurityID");
const CString&	TRS_SubjectsGrants::s_sGrantType		= _NS_FLD("GrantType");
const CString&	TRS_SubjectsGrants::s_sInherited		= _NS_FLD("Inherited");
const CString&	TRS_SubjectsGrants::s_sIsImplicit		= _NS_FLD("IsImplicit");
const CString&	TRS_SubjectsGrants::s_sWorkerID			= _NS_FLD("WorkerID");

const CString&	TRS_SubjectsGrants::s_sCode				= _NS_LFLD("Code");
const CString&	TRS_SubjectsGrants::s_sDescription		= _NS_LFLD("Description");
const CString&	TRS_SubjectsGrants::s_sSelected			= _NS_LFLD("Selected");

//-----------------------------------------------------------------------------
void TRS_SubjectsGrants::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	(s_sSubjectID,				f_SubjectID);
		BIND_DATA	(s_sEntityName,				f_EntityName);			
		BIND_DATA	(s_sRowSecurityID,			f_RowSecurityID);
		BIND_DATA	(s_sGrantType,				f_GrantType);
		BIND_DATA	(s_sInherited,				f_Inherited);	
		BIND_DATA	(s_sIsImplicit,				f_IsImplicit);
		BIND_DATA	(s_sWorkerID,				f_WorkerID);

		// local
		LOCAL_STR	(s_sCode,					l_Code,			12);
		LOCAL_STR	(s_sDescription,			l_Description,	60);
		LOCAL_DATA	(s_sSelected,				l_Selected);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TRS_SubjectsGrants::GetStaticName() {return _NS_TBL("RS_SubjectsGrants");}

/////////////////////////////////////////////////////////////////////////////
//		TableUpdater :	class TURS_SubjectsGrants
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (TURS_SubjectsGrants, TableUpdater)

//------------------------------------------------------------------------------
TURS_SubjectsGrants::TURS_SubjectsGrants
	(
		CAbstractFormDoc* 	pDocument,	// = NULL
		CMessages* 			pMessages	// = NULL
	)													
	: 
	TableUpdater(RUNTIME_CLASS(TRS_SubjectsGrants), (CBaseDocument*)pDocument, pMessages)
{
}

//------------------------------------------------------------------------------
void TURS_SubjectsGrants::OnDefineQuery ()
{
	TRS_SubjectsGrants* pRec = GetRecord();
	m_pTable->SelectAll	();
	
	m_pTable->AddParam			(szP1, pRec->f_SubjectID);
	m_pTable->AddFilterColumn	(pRec->f_SubjectID);
	m_pTable->AddParam			(szP2, pRec->f_EntityName);
	m_pTable->AddFilterColumn	(pRec->f_EntityName);
	m_pTable->AddParam			(szP3, pRec->f_RowSecurityID);
	m_pTable->AddFilterColumn	(pRec->f_RowSecurityID);
}
	
//------------------------------------------------------------------------------
void TURS_SubjectsGrants::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szP1, m_nSubjectID);
	m_pTable->SetParamValue(szP2, m_strEntityName);
	m_pTable->SetParamValue(szP3, m_nRowSecurityID);
}

//------------------------------------------------------------------------------
BOOL TURS_SubjectsGrants::IsEmptyQuery()
{
	return m_nSubjectID.IsEmpty() || m_strEntityName.IsEmpty() || m_nRowSecurityID.IsEmpty();
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TURS_SubjectsGrants::FindRecord(const DataLng& aSubjectID, const DataStr& strEntityName, const DataLng& nRowSecurityID, BOOL bLock)
{
	m_nSubjectID = aSubjectID;
	m_strEntityName = strEntityName;
	m_nRowSecurityID = nRowSecurityID;

	return TableUpdater::FindRecord(bLock);
}                                                

//////////////////////////////////////////////////////////////////////////////
//             					RowSecurityAddOnFields
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------
IMPLEMENT_DYNCREATE(RowSecurityAddOnFields, SqlAddOnFieldsColumn) 

//-----------------------------------------------------------------------------
RowSecurityAddOnFields::RowSecurityAddOnFields()
	:
	f_IsProtected				(FALSE),
	l_CurrentWorkerGrantType	(E_GRANT_TYPE_DENY),
	l_SpecificWorkerGrantType	(E_GRANT_TYPE_DENY)
{
}

//-----------------------------------------------------------------
const CString& RowSecurityAddOnFields::s_sRowSecurityID				= _NS_FLD("RowSecurityID");
const CString& RowSecurityAddOnFields::s_sIsProtected				= _NS_FLD("IsProtected");
const CString& RowSecurityAddOnFields::s_sCurrentWorkerGrantType	= _NS_LFLD("CurrentWorkerGrantType");
const CString& RowSecurityAddOnFields::s_sSpecificWorkerGrantType	= _NS_LFLD("SpecificWorkerGrantType");

//-----------------------------------------------------------------
int RowSecurityAddOnFields::BindAddOnFields(int nStartPos /*=0*/)
{
	BEGIN_BIND_ADDON_FIELDS(nStartPos);

		BIND_ADDON_DATA	(s_sRowSecurityID,				f_RowSecurityID);
		BIND_ADDON_DATA	(s_sIsProtected,				f_IsProtected);

		LOCAL_ADDON_DATA(s_sCurrentWorkerGrantType,		l_CurrentWorkerGrantType);
		LOCAL_ADDON_DATA(s_sSpecificWorkerGrantType,	l_SpecificWorkerGrantType);
		
	END_BIND_ADDON_FIELDS();
}

static TCHAR szPKey[]			= _T("PKey");
static TCHAR szPDescription[]	= _T("PDescription");
static TCHAR szPRSID[]			= _T("PRSID");

/////////////////////////////////////////////////////////////////////////////
//	Hotlink	 HKLRowSecurity 				
/////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (HKLRowSecurity, HotKeyLink)

//------------------------------------------------------------------------------
HKLRowSecurity::HKLRowSecurity()
{
}

//------------------------------------------------------------------------------
HKLRowSecurity::HKLRowSecurity(CRuntimeClass* pRecClass, const CString& strDocNamespace) 
	: 
	HotKeyLink		(pRecClass, strDocNamespace),
	m_nRowSecurityID(-1)
{
}
	
//------------------------------------------------------------------------------
void HKLRowSecurity::OnDefineQuery (SelectionType nQuerySelection)
{
	m_pTable->SelectAll();

	DataLng* pRowSecurityID = (DataLng*)m_pRecord->GetDataObjFromColumnName(RowSecurityAddOnFields::s_sRowSecurityID);
	DataObj* pKeyField = m_pRecord->GetDataObjFromColumnName(m_strKeyField);
	DataObj* pDescriptionField = m_pRecord->GetDataObjFromColumnName(m_strDescriptionField);
	
	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->m_strFilter = cwsprintf(_T(" %s = ? OR %s = ?"),  m_strKeyField, RowSecurityAddOnFields::s_sRowSecurityID);
			m_pTable->AddParam	(szPKey, *pKeyField);
			m_pTable->AddParam	(szPRSID, *pRowSecurityID);
			break;
			
		case COMBO_ACCESS:
		case UPPER_BUTTON:
			m_pTable->AddSortColumn		(*pKeyField);
			m_pTable->AddFilterLike		(*pKeyField);
			m_pTable->AddParam			(szPKey, *pKeyField);
			break;

		case LOWER_BUTTON:
			m_pTable->AddSortColumn		(*pDescriptionField);
			m_pTable->AddFilterLike		(*pDescriptionField);
			m_pTable->AddParam			(szPDescription, *pDescriptionField);
			break;
	}
}

//------------------------------------------------------------------------------
void HKLRowSecurity::OnPrepareQuery (DataObj* pDataObj, SelectionType nQuerySelection)
{
	ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataStr)));
	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->SetParamValue(szPKey,	*pDataObj);
			m_pTable->SetParamValue(szPRSID, m_nRowSecurityID);
			break;
			
		case COMBO_ACCESS:
		case UPPER_BUTTON:
			m_pTable->SetParamLike(szPKey,	*pDataObj);
			break;

		case LOWER_BUTTON:
			m_pTable->SetParamLike(szPDescription,	*pDataObj);
			break;
	}
}


/////////////////////////////////////////////////////////////////////////////
//	TableUpdater				TURowSecurity
/////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (TURowSecurity, TableUpdater)


//------------------------------------------------------------------------------
TURowSecurity::TURowSecurity(CRuntimeClass* pRecClass, SqlSession* pSession)													
	: 
	TableUpdater(pRecClass, NULL, NULL)
{
	SetSqlSession(pSession);
}

//------------------------------------------------------------------------------
void TURowSecurity::OnDefineQuery ()
{
	RowSecurityAddOnFields* pAddOnFields = (RowSecurityAddOnFields*)m_pRecord->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields));
	m_pTable->SelectAll			();
	m_pTable->AddFilterColumn	(pAddOnFields->f_RowSecurityID);
	m_pTable->AddParam			(_T("P1"),	pAddOnFields->f_RowSecurityID);;
}
	
//------------------------------------------------------------------------------
void TURowSecurity::OnPrepareQuery ()
{
	m_pTable->SetParamValue(_T("P1"),	m_RowSecurityID);
}

//------------------------------------------------------------------------------
BOOL TURowSecurity::IsEmptyQuery()
{
	return m_RowSecurityID.IsEmpty();
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TURowSecurity::FindRecord(const DataLng& nRowSecurityID, BOOL bLock)
{
	m_RowSecurityID		= nRowSecurityID;
	
	return TableUpdater::FindRecord(bLock);
}

//------------------------------------------------------------------------------
RowSecurityAddOnFields* TURowSecurity::GetRowSecurityAddOnFields()
{
	return (m_pRecord) ? (RowSecurityAddOnFields*)m_pRecord->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields)) : NULL;
}