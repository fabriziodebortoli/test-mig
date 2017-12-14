#include "stdafx.h"

#include <TbGeneric\Critical.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGenlib\parsctrl.h>
#include <TbOleDb\SqlRec.h>
#include <TbOleDb\SqlTable.h>

#include "Dbt.h"
#include "HotLink.h"
#include "bodyedit.h"
#include "ExtDocAbstract.h"
#include "extdoc.hjson" //JSON AUTOMATIC UPDATE

#include "NumbererService.h"
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static TCHAR szP1[]  = _T("P1");
static TCHAR szP2[]  = _T("P2");

////////////////////////////////////////////////////////////////////////////////
///							CNumbererServiceObj
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CNumbererServiceObj::CNumbererServiceObj(SqlSession* pSession)
	:
	m_pDocument			(NULL),
	m_pSqlSession		(pSession),
	m_bOpenedInUpdate	(FALSE)
{
}

//-----------------------------------------------------------------------------	
CNumbererServiceObj::~CNumbererServiceObj()
{
}

//-----------------------------------------------------------------------------	
SqlSession* CNumbererServiceObj::GetSqlSession(bool bIsUpdatable /*FALSE*/)
{
	// se ho il documento lavoro con il documento
	if (m_pDocument)
		return bIsUpdatable ? m_pDocument->GetUpdatableSqlSession() : m_pDocument->GetReadOnlySqlSession();
	
	// se non ho una sessione di lavoro, posso lavorare con quella di default
	if (!m_pSqlSession && !bIsUpdatable)
		return AfxGetDefaultSqlSession();
	
	// se no devono avermela data i programmatori
	return m_pSqlSession;
}

// prima di fare l'assign devo capire se devo assegnare tutto il nuovo
// formato di numeratore oppure se devo preservare il suffisso
//-----------------------------------------------------------------------------	
void CNumbererServiceObj::AssignNumber(DataObj* pData, const DataLng& aNewNumber, DataDate* pDataDate /*NULL*/)
{
	if (!pData)
		return;

	CString sSource = aNewNumber.Str();

	// devo preoccuparmi di preservare il suffisso esistente
	int nStart = GetFormatMask().GetEditableZoneStart();
	if (nStart > 0 && pData->GetDataType() == DATA_STR_TYPE)
		sSource += pData->Str().Mid(nStart);
	
	// se ho l'anno bene altrimenti default
	pData->Assign(GetFormatMask().ApplyMask(sSource, TRUE, pDataDate ? pDataDate->Year() : 0));
}

//-----------------------------------------------------------------------------	
void CNumbererServiceObj::OnInitService()
{
	if (GetContext()->GetContextClass()->IsDerivedFrom(RUNTIME_CLASS(CAbstractFormDoc)))
		m_pDocument = (CAbstractFormDoc*) GetContext();
	else if (GetContext()->GetContextClass()->IsDerivedFrom(RUNTIME_CLASS(CWnd)))
	{
		CParsedCtrl* pCtrl = (CParsedCtrl*) GetContext();
		m_pDocument = (CAbstractFormDoc*) pCtrl->GetDocument();
	}
}

//-----------------------------------------------------------------------------	
CFormatMask	 CNumbererServiceObj::GetFormatMask()
{
	CFormatMask aMask;
	aMask.SetMask(GetFormatMaskField());

	if (AfxIsActivated(TBEXT_APP, XENGINE_ACT))
		aMask.SetSiteCode(AfxGetXEngineSiteCode());

	return aMask;
}

// default information request on how numberer is made
//-----------------------------------------------------------------------------	
bool CNumbererServiceObj::ReadInfo(IBehaviourRequest* pRequest)
{
	CNumbererRequest* pNumRequest = dynamic_cast<CNumbererRequest*>(pRequest);

	// se si tratta di una richiesta di informazioni le ritorno
	// anche al cambio di formmode deve ritornare al comportamento originale
	pNumRequest->SetNumberingDisabled(FALSE);
	pNumRequest->SetDatabaseNumberingDisabled(FALSE);
	pNumRequest->SetFormatMask(GetFormatMask());
	
	return true;
}

//-----------------------------------------------------------------------------	
bool CNumbererServiceObj::ReadNumber(IBehaviourRequest* pRequest)
{
	CNumbererRequest* pNumRequest = dynamic_cast<CNumbererRequest*>(pRequest);

	DataLng aNext;

	if	(
			(
				// il tipo di documento fa la differenza
				!m_pDocument || (m_pDocument->IsABatchDocument() && !m_pDocument->IsRunningAsADM())|| 
				m_pDocument->GetFormMode() == CBaseDocument::NEW ||
				(!pNumRequest->IsPrimaryKey() && m_pDocument->GetFormMode() == CBaseDocument::EDIT)
			)
			&& OnReadNextNumber(pNumRequest, aNext) 
			&& CanPerformNumbering(pNumRequest)
		)
	{
		AssignNumber(pNumRequest->GetData(), aNext);

		if (m_pDocument && !m_pDocument->IsABatchDocument())
		{
			// avviso documento e DBT che ho modificato il dato
			if (pRequest->GetOwner() && pRequest->GetOwner()->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
				((DBTSlaveBuffered*) pRequest->GetOwner())->SetModified(TRUE);
			
			m_pDocument->SetModifiedFlag(TRUE);
			pNumRequest->GetData()->SetUpdateView(TRUE);
		}

		return true;
	}
	return false;
}


//-----------------------------------------------------------------------------	
bool CNumbererServiceObj::WriteNumber(IBehaviourRequest* pRequest)
{
	CNumbererRequest* pNumRequest = dynamic_cast<CNumbererRequest*>(pRequest);

	DataLng aNext;

	if (!OnWriteNextNumber(pNumRequest, aNext))
		return false;

	// se è disabilitata non fa nulla ma non fa fallire la transazione
	if (!CanPerformNumbering(pNumRequest))
		return true;

	AssignNumber(pNumRequest->GetData(), aNext);

	if (m_pDocument && pNumRequest->IsPrimaryKey())
		m_pDocument->ForcePreparePrimaryKey();
	
	return true;
}

// Due parole sul senso dei vari campi disabled:
// 1) IBehaviourRequest::m_bEnabled abil/disabil-itazione della intera richiesta di servizio
//    viene gestito all'origine dai behaviours non facendo partire la chiamata al servizio
// 2) m_bIsNumberingDisabled: pilota lo stato di numerazione manuale e automatica
//	  e viene rinfrescato ad ogni cambio di FormMode di documento
// 3) m_Entities.f_Disabled è il parametro utente salvato su database da cui si riparte
//-----------------------------------------------------------------------------	
const bool CNumbererServiceObj::CanPerformNumbering(CNumbererRequest* pRequest) const
{
	return !*(pRequest->GetNumberingDisabled());
}

////////////////////////////////////////////////////////////////////////////////
///							TAutoincrementEntities
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TAutoincrementEntities, SqlRecord)

//-----------------------------------------------------------------------------
TAutoincrementEntities::TAutoincrementEntities(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TAutoincrementEntities::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	        (_NS_FLD("Entity"),			f_Entity);
		BIND_DATA	        (_NS_FLD("LastNumber"),		f_LastNumber);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TAutoincrementEntities::GetStaticName() {return _NS_TBL("TB_AutoincrementEntities");}

////////////////////////////////////////////////////////////////////////////////
///							CAutoincrementService
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CAutoincrementService, CNumbererServiceObj)

//-----------------------------------------------------------------------------	
BEGIN_BEHAVIOUR_EVENTMAP(CAutoincrementService)
	// eventi di documento
	ON_BEHAVIOUR_EVENT(bhe_LockDocumentForNew,	OnLockDocumentForNew)
	ON_BEHAVIOUR_EVENT(bhe_DeleteTransaction,	OnDeleteTransaction)
	ON_BEHAVIOUR_EVENT(bhe_BeforeEscape,		OnBeforeEscape)
	ON_BEHAVIOUR_EVENT(bhe_FormModeChanged,		OnFormModeChanged)
END_BEHAVIOUR_EVENTMAP()

//-----------------------------------------------------------------------------	
CAutoincrementService::CAutoincrementService(SqlSession* pSession)
	:
	CNumbererServiceObj		(pSession),
	m_pEntityTable		    (NULL)
{
}

//-----------------------------------------------------------------------------	
CAutoincrementService::~CAutoincrementService()
{
	CloseTables();
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::OnBeforeEscape(IBehaviourRequest* pRequest)
{
	CNumbererRequest* pBaseRequest = dynamic_cast<CNumbererRequest*>(pRequest);

	if (!pRequest || !pBaseRequest->GetData())
		return false;

	// l'operazione di Undo in caso in New non usa gli old 
	if (m_pDocument && m_pDocument->GetFormMode() == CBaseDocument::NEW) 
		RestoreNumber(pBaseRequest, false); 
	return true;
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::OnFormModeChanged(IBehaviourRequest* pRequest)
{
	return ReadInfo(pRequest) ? ReadNumber (pRequest) : false; 
}

//-----------------------------------------------------------------------------	
void CAutoincrementService::OpenTables(bool bIsUpdatable /*FALSE*/)
{
	if (m_pEntityTable && m_pEntityTable->IsOpen())
	{
		// non devo aprire in update tutto ok
		if (!bIsUpdatable || m_bOpenedInUpdate)
			return;

		// devo assicurarmi di avere la SqlSession di update
		CloseTables();
	}

	m_pEntityTable = new SqlTable(&m_Entities, GetSqlSession(bIsUpdatable));
	m_pEntityTable->Open(bIsUpdatable);
	m_bOpenedInUpdate = bIsUpdatable;

	m_pEntityTable->SelectAll();

	// questa istruzione fa in modo che se non c'è la transazione aperta
	// si vada in AutoCommit. Comunque se c'è la transazione aperta vince la transazione.
	if (m_bOpenedInUpdate)
		m_pEntityTable->SetAutocommit(TRUE);

	m_pEntityTable->AddFilterColumn(m_Entities.f_Entity);
	m_pEntityTable->AddParam(szP1, m_Entities.f_Entity);
}

//-----------------------------------------------------------------------------	
void CAutoincrementService::CloseTables()
{
	if (m_pEntityTable && m_pEntityTable->IsOpen())
		m_pEntityTable->Close();

	SAFE_DELETE(m_pEntityTable)
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::IsCompatibleWith(IBehaviourRequest* pRequest)
{
	return	pRequest->GetReceiver()->GetClass() == GetRuntimeClass() &&
			pRequest->IsKindOf(RUNTIME_CLASS(CNumbererRequest)) &&
			(m_pEntityTable == NULL || ((CNumbererRequest*) pRequest)->GetEntity().ToString().CompareNoCase(m_Entities.f_Entity.Str()) == 0);
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::CanExecuteRequest(const BehaviourEvents& evnt, IBehaviourRequest* pRequest)
{
	// le basi
	if (!pRequest || !pRequest->IsKindOf(RUNTIME_CLASS(CNumbererRequest)))
		return false;

	bool bIsTransactionEvent = evnt == bhe_LockDocumentForNew || evnt == bhe_NewTransaction || evnt == bhe_DeleteTransaction;
	
	return !bIsTransactionEvent || m_pDocument;
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::ReadInfo(IBehaviourRequest* pRequest)
{
	CNumbererRequest* pNumRequest = (CNumbererRequest*) pRequest;
	if (!ReadInfo (pNumRequest, false, false))
		return false;

	// se si tratta di una richiesta di informazioni le ritorno
	// anche al cambio di formmode deve ritornare al comportamento originale
	pNumRequest->SetNumberingDisabled(FALSE);
	pNumRequest->SetDatabaseNumberingDisabled(FALSE);
	pNumRequest->SetFormatMask(GetFormatMask());
	
	return true;
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::OnDeleteTransaction(IBehaviourRequest* pRequest)
{
	RestoreNumber((CNumbererRequest*) pRequest, true); 
	// se non riesco a decrementare pace non faccio fallire la transazione
	return true;
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::OnLockDocumentForNew(IBehaviourRequest* pRequest)
{
	CNumbererRequest* pNumRequest = (CNumbererRequest*) pRequest;

	// se è chiave primaria devo farlo durante il lock per riallineare i dbt
	return WriteNumber (pRequest);
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::RestoreNumber(CNumbererRequest* pRequest, bool useOldData)
{
	// provo a prendere il numero dove riesco se non non transo nemmeno
	DataObj* pData = useOldData ? pRequest->GetOldData() : pRequest->GetData();
	if (!pData)
		return true;

	// inizializzo i dati della richiesta
	ReadInfo(pRequest, false, useOldData);
	long lNumber = GetFormatMask().GetNumberFromMask(pData->Str());
	DataStr aEntity (pRequest->GetEntity().ToString());

	// eseguo le query
	bool bOk = true;
	TRY
	{
		OpenTables(true);

		m_pEntityTable->SetParamValue(szP1, aEntity); 
		m_pEntityTable->Query();

		if (!CanPerformNumbering(pRequest) || m_pEntityTable->IsEmpty())
			return true;

		if (m_pEntityTable->LockCurrent(!AfxIsInUnattendedMode()))
		{
			if (lNumber == m_Entities.f_LastNumber)
				m_pEntityTable->Edit();
		}
		else
		{
			m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to lock record for entity {0-%s}"), aEntity.Str()),	CDiagnostic::Error);
			return false;
		}

		if (lNumber == m_Entities.f_LastNumber && m_Entities.f_LastNumber > 0)
		{
			lNumber =  --m_Entities.f_LastNumber;
			if (m_pEntityTable->Update()== UPDATE_FAILED)
			{
				m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to decrement last number for entity {0-%s}"), aEntity.Str()),	CDiagnostic::Error);
				bOk = false;
			}
		}

		m_pEntityTable->UnlockCurrent();

	}
	CATCH(SqlException, e)
	{
		m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to get entity number for {0-%s}"), aEntity.Str()), CDiagnostic::Error);
		return false;
	}
	END_CATCH

	return bOk;
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::OnReadNextNumber(CNumbererRequest* pRequest, DataLng& aNextNumber) 
{ 
	return WriteNextNumber(pRequest, aNextNumber);
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::OnWriteNextNumber(CNumbererRequest* pRequest, DataLng& aNextNumber)
{
	// l'Autoincrement ha già scritto
	aNextNumber.Assign(pRequest->GetData()->Str());
	return true;
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::ReadInfo(CNumbererRequest* pRequest, bool bForUpdate, bool useOldData)
{
	DataStr aEntity (pRequest->GetEntity().ToString());

	// se non devo scrivere ottimizzo un po' le letture 
	if (!bForUpdate && m_pEntityTable && m_Entities.f_Entity == aEntity)
		return true;
	

	TRY
	{
		OpenTables(bForUpdate);

		m_pEntityTable->SetParamValue(szP1, aEntity); 
		m_pEntityTable->Query();
	}
	CATCH(SqlException, e)
	{
		m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to read numberer info for entity {0-%s}"), pRequest->GetEntity().ToString()),	CDiagnostic::Error);
		return false;
	}
	END_CATCH

	return true;
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::ReadNextNumber(CNumbererRequest* pRequest, DataLng& aNextNumber)
{
	TRY
	{
		// forzo la rilettura della tabella
		m_Entities.f_Entity.Clear();
		if (!ReadInfo(pRequest, false, false))
			return false;

		if (!CanPerformNumbering(pRequest))
			return true;

		aNextNumber =  m_Entities.f_LastNumber + 1;
	}
	CATCH(SqlException, e)
	{
		m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to get next number for entity {0-%s}"), pRequest->GetEntity().ToString()),	CDiagnostic::Error);
		return false;
	}
	END_CATCH

	return true;
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::WriteNextNumber(CNumbererRequest* pRequest, DataLng& aNextNumber)
{
	DataStr aEntity (pRequest->GetEntity().ToString());
	bool bOk = TRUE;
	TRY
	{
		OpenTables(true);

		// prima provo a loccare il record
		m_Entities.f_Entity = aEntity;
		if (!m_pEntityTable->LockTableKey(&m_Entities))
		{
			m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to lock record for entity {0-%s}"), pRequest->GetEntity().ToString()),	CDiagnostic::Error);  
			return false;
		}

		m_pEntityTable->SetParamValue(szP1, aEntity); 
		m_pEntityTable->Query();

		if (!CanPerformNumbering(pRequest))
		{
			m_pEntityTable->UnlockTableKey(&m_Entities);
			return true;
		}

		// mi preparo a scrivere
		if (m_pEntityTable->IsEmpty())
		{
			m_pEntityTable->AddNew();
			m_Entities.f_Entity		   = aEntity;
		}
		else
			m_pEntityTable->Edit();

		aNextNumber =  ++m_Entities.f_LastNumber;
		if (m_pEntityTable->Update() == UPDATE_FAILED)
		{
			m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to update record for entity {0-%s}"), pRequest->GetEntity().ToString()), CDiagnostic::Error);
			bOk = false;
		}

		m_pEntityTable->UnlockTableKey(&m_Entities);
	}
	CATCH(SqlException, e)
	{
		m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to get entity number for {0-%s}"), pRequest->GetEntity().ToString()), CDiagnostic::Error);
		bOk = false;
	}
	END_CATCH

	return bOk;
}

//-----------------------------------------------------------------------------	
CString CAutoincrementService::GetFormatMaskField()
{
	return _T("");
}

// Due parole sul senso dei vari campi disabled:
// 1) IBehaviourRequest::m_bEnabled abil/disabil-itazione della intera richiesta di servizio
//    viene gestito all'origine dai behaviours non facendo partire la chiamata al servizio
// 2) m_bIsNumberingDisabled: pilota lo stato di numerazione manuale e automatica
//	  e viene rinfrescato ad ogni cambio di FormMode di documento
// 3) m_Entities.f_Disabled è il parametro utente salvato su database da cui si riparte
//-----------------------------------------------------------------------------	
const bool CAutoincrementService::CanPerformNumbering(CNumbererRequest* pRequest) const
{
	return __super::CanPerformNumbering(pRequest);
}

//-----------------------------------------------------------------------------	
bool CAutoincrementService::GetNextNumber(const CString& strEntity, DataObj* pDataObj)
{
	CNumbererRequest aRequest(this, pDataObj, strEntity);
	return ReadNumber(&aRequest);
}

////////////////////////////////////////////////////////////////////////////////
///							TAutonumberEntities
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TAutonumberEntities, SqlRecord)

//-----------------------------------------------------------------------------
TAutonumberEntities::TAutonumberEntities(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TAutonumberEntities::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	        (_NS_FLD("Entity"),			f_Entity);
		BIND_DATA	        (_NS_FLD("FormattedMask"),	f_FormattedMask);
		BIND_DATA	        (_NS_FLD("IsYearEntity"),	f_IsYearEntity);
		BIND_DATA	        (_NS_FLD("LastNumber"),		f_LastNumber);
		BIND_DATA	        (_NS_FLD("Disabled"),		f_Disabled);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TAutonumberEntities::GetStaticName() {return _NS_TBL("TB_AutonumberEntities");}

////////////////////////////////////////////////////////////////////////////////
///						TAutonumberEntitiesYears
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TAutonumberEntitiesYears, SqlRecord)

//-----------------------------------------------------------------------------
TAutonumberEntitiesYears::TAutonumberEntitiesYears(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TAutonumberEntitiesYears::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	(_NS_FLD("Entity"),		    f_Entity);
		BIND_DATA	(_NS_FLD("Year"),		    f_Year);
		BIND_DATA	(_NS_FLD("LastNumber"),	    f_LastNumber);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TAutonumberEntitiesYears::GetStaticName() {return _NS_TBL("TB_AutonumberEntitiesYears");}

////////////////////////////////////////////////////////////////////////////////
///							CAutonumberService
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CAutonumberService, CNumbererServiceObj)

//-----------------------------------------------------------------------------	
BEGIN_BEHAVIOUR_EVENTMAP(CAutonumberService)
	// eventi di documento
	ON_BEHAVIOUR_EVENT(bhe_FormModeChanged,		OnFormModeChanged)
	ON_BEHAVIOUR_EVENT(bhe_LockDocumentForNew,	OnLockDocumentForNew)
	ON_BEHAVIOUR_EVENT(bhe_DeleteTransaction,	OnDeleteTransaction)
END_BEHAVIOUR_EVENTMAP()

//-----------------------------------------------------------------------------	
CAutonumberService::CAutonumberService(SqlSession* pSession)
	:
	CNumbererServiceObj	(pSession),
	m_pEntityTable		(NULL),
	m_pEntityYearsTable	(NULL)
{
}

//-----------------------------------------------------------------------------	
CAutonumberService::~CAutonumberService()
{
	CloseTables();
}

//-----------------------------------------------------------------------------	
void CAutonumberService::OpenTables(bool bIsUpdatable /*FALSE*/)
{
	if (m_pEntityTable && m_pEntityTable->IsOpen())
	{
		// non devo aprire in update tutto ok
		if (!bIsUpdatable || m_bOpenedInUpdate)
			return;

		// devo assicurarmi di avere la SqlSession di update
		CloseTables();
	}

	m_pEntityTable = new SqlTable(&m_Entities, GetSqlSession(bIsUpdatable));
	m_pEntityTable->Open(bIsUpdatable);
	m_bOpenedInUpdate = bIsUpdatable;

	m_pEntityTable->SelectAll();

	// questa istruzione fa in modo che se non c'è la transazione aperta
	// si vada in AutoCommit. Comunque se c'è la transazione aperta vince la transazione.
	if (m_bOpenedInUpdate)
		m_pEntityTable->SetAutocommit(TRUE);

	m_pEntityTable->AddFilterColumn(m_Entities.f_Entity);
	m_pEntityTable->AddParam(szP1, m_Entities.f_Entity);
}

//-----------------------------------------------------------------------------	
void CAutonumberService::OpenYearsTables(bool bIsUpdatable /*FALSE*/)
{
	if (m_pEntityYearsTable && m_pEntityYearsTable->IsOpen())
	{
		// non devo aprire in update tutto ok
		if (!bIsUpdatable || m_bOpenedInUpdate)
			return;

		// devo assicurarmi di avere la SqlSession di update
		if (m_pEntityYearsTable)
			m_pEntityYearsTable->Close();
		SAFE_DELETE(m_pEntityYearsTable);
	}

	m_pEntityYearsTable = new SqlTable(&m_EntitiesYears, GetSqlSession(bIsUpdatable));
	m_pEntityYearsTable->Open(bIsUpdatable);
	m_pEntityYearsTable->SelectAll();
	// questa istruzione fa in modo che se non c'è la transazione aperta
	// si vada in AutoCommit. Comunque se c'è la transazione aperta vince la transazione.
	if (bIsUpdatable)
		m_pEntityYearsTable->SetAutocommit(TRUE);

	m_pEntityYearsTable->AddParam       (szP1,	m_EntitiesYears.f_Entity);
	m_pEntityYearsTable->AddFilterColumn(		m_EntitiesYears.f_Entity);
	m_pEntityYearsTable->AddParam       (szP2,	m_EntitiesYears.f_Year);
	m_pEntityYearsTable->AddFilterColumn(		m_EntitiesYears.f_Year);
}

//-----------------------------------------------------------------------------	
void CAutonumberService::CloseTables()
{
	if (m_pEntityYearsTable && m_pEntityYearsTable->IsOpen())
		m_pEntityYearsTable->Close();
	
	if (m_pEntityTable && m_pEntityTable->IsOpen())
		m_pEntityTable->Close();

	SAFE_DELETE(m_pEntityYearsTable)
	SAFE_DELETE(m_pEntityTable)
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::IsCompatibleWith(IBehaviourRequest* pRequest)
{
	return	pRequest->GetReceiver()->GetClass() == GetRuntimeClass() &&
			pRequest->IsKindOf(RUNTIME_CLASS(CNumbererRequest)) &&
			(m_pEntityTable == NULL || ((CNumbererRequest*) pRequest)->GetEntity().ToString().CompareNoCase(m_Entities.f_Entity.Str()) == 0);
}


//-----------------------------------------------------------------------------	
bool CAutonumberService::CanExecuteRequest(const BehaviourEvents& evnt, IBehaviourRequest* pRequest)
{
	// le basi
	if (!pRequest || !pRequest->IsKindOf(RUNTIME_CLASS(CNumbererRequest)))
		return false;

	bool bIsTransactionEvent = evnt == bhe_LockDocumentForNew || evnt == bhe_NewTransaction || evnt == bhe_DeleteTransaction;
	
	return !bIsTransactionEvent || m_pDocument;
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::ReadInfo(IBehaviourRequest* pRequest)
{
	CNumbererRequest* pNumRequest = (CNumbererRequest*) pRequest;
	if (!ReadInfo (pNumRequest, false, false))
		return false;

	// se si tratta di una richiesta di informazioni le ritorno
	// anche al cambio di formmode deve ritornare al comportamento originale
	pNumRequest->SetNumberingDisabled(m_Entities.f_Disabled);
	pNumRequest->SetDatabaseNumberingDisabled(m_Entities.f_Disabled);
	pNumRequest->SetFormatMask(GetFormatMask());
	
	return true;
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::OnFormModeChanged(IBehaviourRequest* pRequest)
{
	return ReadInfo(pRequest) ? ReadNumber (pRequest) : false; 
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::OnLockDocumentForNew(IBehaviourRequest* pRequest)
{
	CNumbererRequest* pNumRequest = (CNumbererRequest*) pRequest;

	// se è chiave primaria devo farlo durante il lock per riallineare i dbt
	return WriteNumber (pRequest);
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::OnDeleteTransaction(IBehaviourRequest* pRequest)
{
	RestoreNumber((CNumbererRequest*) pRequest, true); 
	// se non riesco a decrementare pace non faccio fallire la transazione
	return true;
}

//-----------------------------------------------------------------------------	
CDateNumbererRequestParams* CAutonumberService::GetDateParams(CNumbererRequest* pRequest)
{
	return (pRequest->GetParams() && pRequest->GetParams()->IsKindOf(RUNTIME_CLASS(CDateNumbererRequestParams))) ? (CDateNumbererRequestParams*) pRequest->GetParams() : NULL;
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::RestoreNumber(CNumbererRequest* pRequest, bool useOldData)
{
	// provo a prendere il numero dove riesco se non non transo nemmeno
	DataObj* pData = useOldData ? pRequest->GetOldData() : pRequest->GetData();
	if (!pData)
		return true;

	// inizializzo i dati della richiesta
	ReadInfo(pRequest, false, useOldData);
	long lNumber = GetFormatMask().GetNumberFromMask(pData->Str());
	DataStr aEntity (pRequest->GetEntity().ToString());

	// eseguo le query
	bool bOk = true;
	TRY
	{
		OpenTables(true);

		m_pEntityTable->SetParamValue(szP1, aEntity); 
		m_pEntityTable->Query();

		if (!CanPerformNumbering(pRequest) || m_pEntityTable->IsEmpty())
			return true;

		if (m_pEntityTable->LockCurrent(!AfxIsInUnattendedMode()))
		{
			if (!m_Entities.f_IsYearEntity && lNumber == m_Entities.f_LastNumber)
				m_pEntityTable->Edit();
		}
		else
		{
			m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to lock record for entity {0-%s}"), aEntity.Str()),	CDiagnostic::Error);
			return false;
		}

		if (m_Entities.f_IsYearEntity)
		{
			OpenYearsTables(true);

			CDateNumbererRequestParams* pDateParams = GetDateParams(pRequest);
			if	(
					pDateParams == NULL || 
					(!useOldData && pDateParams->GetDocDate() == NULL) || 
					(useOldData && pDateParams->GetOldDocDate() == NULL)
				)
			{
				ASSERT_TRACE (FALSE, "Missing date parameter in ReadNextNumber called with year numberer!!");
				return false;
			}

			DataInt nYear = useOldData ? pDateParams->GetOldDocDate()->Year() : pDateParams->GetDocDate()->Year(); 

			m_pEntityYearsTable->SetParamValue(szP1, aEntity); 
			m_pEntityYearsTable->SetParamValue(szP2, nYear);
			m_pEntityYearsTable->Query();

			if (!m_pEntityYearsTable->IsEmpty() && lNumber == m_EntitiesYears.f_LastNumber && m_EntitiesYears.f_LastNumber > 0)
			{
				m_pEntityYearsTable->Edit();
				lNumber =  --m_EntitiesYears.f_LastNumber;

				if (m_pEntityYearsTable->Update() == UPDATE_FAILED)
				{
					m_pEntityYearsTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to decrement last number for entity {0-%s}"), aEntity.Str()),	CDiagnostic::Error);
					bOk = false;
				}
			}
		}
		else if (lNumber == m_Entities.f_LastNumber && m_Entities.f_LastNumber > 0)
		{
			lNumber =  --m_Entities.f_LastNumber;
			if (m_pEntityTable->Update()== UPDATE_FAILED)
			{
				m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to decrement last number for entity {0-%s}"), aEntity.Str()),	CDiagnostic::Error);
				bOk = false;
			}
		}

		m_pEntityTable->UnlockCurrent();

	}
	CATCH(SqlException, e)
	{
		m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to get entity number for {0-%s}"), aEntity.Str()), CDiagnostic::Error);
		return false;
	}
	END_CATCH

	return bOk;
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::OnReadNextNumber(CNumbererRequest* pRequest, DataLng& aNextNumber) 
{ 
	return ReadNextNumber(pRequest, aNextNumber);
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::OnWriteNextNumber(CNumbererRequest* pRequest, DataLng& aNextNumber)
{
	return WriteNextNumber(pRequest, aNextNumber);
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::ReadInfo(CNumbererRequest* pRequest, bool bForUpdate, bool useOldData)
{
	DataStr aEntity (pRequest->GetEntity().ToString());

	// se non devo scrivere ottimizzo un po' le letture 
	if (!bForUpdate && m_pEntityTable && m_Entities.f_Entity == aEntity)
		return true;
	

	TRY
	{
		OpenTables(bForUpdate);

		m_pEntityTable->SetParamValue(szP1, aEntity); 
		m_pEntityTable->Query();

		if (!m_pEntityTable->IsEmpty() && m_Entities.f_IsYearEntity)
		{
			OpenYearsTables(false);

			
			CDateNumbererRequestParams* pDateParams = GetDateParams(pRequest);

			if	(
					pDateParams == NULL || 
					(!useOldData && pDateParams->GetDocDate() == NULL) || 
					(useOldData && pDateParams->GetOldDocDate() == NULL)
				)
			{
				ASSERT_TRACE (FALSE, "Missing date parameter in ReadNextNumber called with year numberer!!");
				return false;
			}

			DataInt nYear = useOldData ? pDateParams->GetOldDocDate()->Year() : pDateParams->GetDocDate()->Year(); 

			m_pEntityYearsTable->SetParamValue(szP1, aEntity); 
			m_pEntityYearsTable->SetParamValue(szP2, nYear);
			m_pEntityYearsTable->Query();

			if (m_pEntityYearsTable->IsEmpty())
			{
				m_EntitiesYears.f_Entity     = aEntity;
				m_EntitiesYears.f_Year	     = nYear;
				GetDataFromPreviousYear(&m_EntitiesYears);
			}
		}
	}
	CATCH(SqlException, e)
	{
		m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to read numberer info for entity {0-%s}"), pRequest->GetEntity().ToString()),	CDiagnostic::Error);
		return false;
	}
	END_CATCH

	return true;
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::ReadNextNumber(CNumbererRequest* pRequest, DataLng& aNextNumber)
{
	TRY
	{
		// forzo la rilettura della tabella
		m_Entities.f_Entity.Clear();
		if (!ReadInfo(pRequest, false, false))
			return false;

		if (!CanPerformNumbering(pRequest))
			return true;

		if (m_Entities.f_IsYearEntity)
		{
			OpenYearsTables(false);

			CDateNumbererRequestParams* pDateParams = GetDateParams(pRequest);
			if (pDateParams == NULL || pDateParams->GetDocDate() == NULL)
			{
				ASSERT_TRACE (FALSE, "Missing date parameter in ReadNextNumber called with year numberer!!");
				return false;
			}

			DataInt nYear = pDateParams->GetDocDate()->Year();

			m_pEntityYearsTable->SetParamValue(szP1, DataStr(pRequest->GetEntity().ToString())); 
			m_pEntityYearsTable->SetParamValue(szP2, nYear);
			m_pEntityYearsTable->Query();

			aNextNumber = m_EntitiesYears.f_LastNumber + 1;
		}
		else
			aNextNumber =  m_Entities.f_LastNumber + 1;
	}
	CATCH(SqlException, e)
	{
		m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to get next number for entity {0-%s}"), pRequest->GetEntity().ToString()),	CDiagnostic::Error);
		return false;
	}
	END_CATCH

	return true;
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::WriteNextNumber(CNumbererRequest* pRequest, DataLng& aNextNumber)
{
	DataStr aEntity (pRequest->GetEntity().ToString());
	bool bOk = TRUE;
	TRY
	{
		OpenTables(true);

		// prima provo a loccare il record
		m_Entities.f_Entity = aEntity;
		if (!m_pEntityTable->LockTableKey(&m_Entities))
		{
			m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to lock record for entity {0-%s}"), pRequest->GetEntity().ToString()),	CDiagnostic::Error);  
			return false;
		}

		m_pEntityTable->SetParamValue(szP1, aEntity); 
		m_pEntityTable->Query();

		if (!CanPerformNumbering(pRequest))
		{
			m_pEntityTable->UnlockTableKey(&m_Entities);
			return true;
		}

		// mi preparo a scrivere
		if (m_pEntityTable->IsEmpty())
		{
			m_pEntityTable->AddNew();
			m_Entities.f_Entity		   = aEntity;
		}
		else if (!m_Entities.f_IsYearEntity)
			m_pEntityTable->Edit();

		if (m_Entities.f_IsYearEntity)
		{
			OpenYearsTables(true);

			CDateNumbererRequestParams* pDateParams = GetDateParams(pRequest);
			if (pDateParams == NULL || pDateParams->GetDocDate() == NULL)
			{
				ASSERT_TRACE (FALSE, "Missing date parameter in ReadNextNumber called with year numberer!!");
				return false;
			}

			DataInt nYear = pDateParams->GetDocDate()->Year();			

			m_pEntityYearsTable->SetParamValue(szP1, aEntity); 
			m_pEntityYearsTable->SetParamValue(szP2, nYear);
			m_pEntityYearsTable->Query();

			if (m_pEntityYearsTable->IsEmpty())
			{
				m_pEntityYearsTable->AddNew();
				m_EntitiesYears.f_Entity     = aEntity;
				m_EntitiesYears.f_Year	     = nYear;
				GetDataFromPreviousYear(&m_EntitiesYears);
			}
			else
				m_pEntityYearsTable->Edit();

			aNextNumber = ++m_EntitiesYears.f_LastNumber;
			if (m_pEntityYearsTable->Update() == UPDATE_FAILED)
			{
				m_pEntityYearsTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to update record for entity {0-%s}"), pRequest->GetEntity().ToString()),	CDiagnostic::Error);
				bOk = false;
			}	
		}
		else
		{
			aNextNumber =  ++m_Entities.f_LastNumber;
			if (m_pEntityTable->Update() == UPDATE_FAILED)
			{
				m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to update record for entity {0-%s}"), pRequest->GetEntity().ToString()),	CDiagnostic::Error);
				bOk = false;
			}
		}

		m_pEntityTable->UnlockTableKey(&m_Entities);
	}
	CATCH(SqlException, e)
	{
		m_pEntityTable->GetDiagnostic()->Add(cwsprintf(_TB("Unable to get entity number for {0-%s}"), pRequest->GetEntity().ToString()), CDiagnostic::Error);
		bOk = false;
	}
	END_CATCH

	return bOk;
}

//-----------------------------------------------------------------------------	
void CAutonumberService::GetDataFromPreviousYear(TAutonumberEntitiesYears* pRecEntityYear)
{
	TAutonumberEntitiesYears	entitiesYears;
	SqlTable					entityYearsTable(&entitiesYears, GetSqlSession(false));

	TRY
	{
		entityYearsTable.Open();
		entityYearsTable.SelectAll();
		entityYearsTable.AddParam       (szP1,	entitiesYears.f_Entity);
		entityYearsTable.AddFilterColumn(		entitiesYears.f_Entity);
		entityYearsTable.AddParam       (szP2,	entitiesYears.f_Year);
		entityYearsTable.AddFilterColumn(		entitiesYears.f_Year);
		entityYearsTable.AddSortColumn  (		entitiesYears.f_Year, TRUE);
	
		entityYearsTable.SetParamValue(szP1,	pRecEntityYear->f_Entity); 
		entityYearsTable.SetParamValue(szP2,	pRecEntityYear->f_Year -1);

		entityYearsTable.Query();
	}
	CATCH(SqlException, e)
	{
		m_pEntityTable->GetDiagnostic()->Add(_TB("Error on GetDataFromPreviousYear"), CDiagnostic::Error);
	}
	END_CATCH

	if (entityYearsTable.IsOpen())
		entityYearsTable.Close();
}

//-----------------------------------------------------------------------------	
CString CAutonumberService::GetFormatMaskField()
{
	if (m_Entities.f_Disabled)
		return __T("");

	return m_Entities.f_FormattedMask;
}

// Due parole sul senso dei vari campi disabled:
// 1) IBehaviourRequest::m_bEnabled abil/disabil-itazione della intera richiesta di servizio
//    viene gestito all'origine dai behaviours non facendo partire la chiamata al servizio
// 2) m_bIsNumberingDisabled: pilota lo stato di numerazione manuale e automatica
//	  e viene rinfrescato ad ogni cambio di FormMode di documento
// 3) m_Entities.f_Disabled è il parametro utente salvato su database da cui si riparte
//-----------------------------------------------------------------------------	
const bool CAutonumberService::CanPerformNumbering(CNumbererRequest* pRequest) const
{
	if (__super::CanPerformNumbering(pRequest))
		return true;

	return !m_Entities.f_Disabled && __super::CanPerformNumbering(pRequest);
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::GetNextNumber(const CString& strEntity, DataObj* pDataObj, DataDate* pDataDate)
{
	CNumbererRequest aRequest(this, pDataObj, strEntity);
	return ReadNumber(&aRequest);
}

//-----------------------------------------------------------------------------	
bool CAutonumberService::SetNextNumber(const CString& strEntity, DataObj* pDataObj, DataDate* pDataDate)
{
	CNumbererRequest aRequest(this, pDataObj, strEntity);
	return WriteNumber(&aRequest);
}

////////////////////////////////////////////////////////////////////////////////
//								CNumbererBinder
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
CNumbererBinder::CNumbererBinder()
{
}

//-----------------------------------------------------------------------------	
void CNumbererBinder::BindAutoincrement(CNumbererRequest* pRequest)
{
	pRequest->SetIsPrimaryKey(TRUE);
	m_Requests.Add(pRequest);
}

//-----------------------------------------------------------------------------	
CNumbererRequest* CNumbererBinder::BindAutoincrement(CObject* pOwner, DataObj* pDataBinding, const CString& sEntity)
{
	CNumbererRequest* pRequest = new CNumbererRequest(pOwner, pDataBinding, sEntity);
	BindAutoincrement(pRequest);
	return pRequest;
}

// ---------------------------------------------------------------------------- -
void CNumbererBinder::BindAutonumber(CNumbererRequest* pRequest, CNumbererRequestParams* pParams /*NULL*/)
{
	pRequest->SetParams(pParams);
	m_Requests.Add(pRequest);
}

// ---------------------------------------------------------------------------- -
void CNumbererBinder::BindAutonumber(CObject* pOwner, DataObj* pDataBinding, const CString& sEntity)
{
	CNumbererRequest* pRequest = new CNumbererRequest(pOwner, pDataBinding, sEntity);
	BindAutonumber(pRequest);
}

//-----------------------------------------------------------------------------	
void CNumbererBinder::EnableNumberer(DataObj* pDataBinding, const BOOL bValue /*TRUE*/)
{
	for (int i = 0; i <= m_Requests.GetUpperBound(); i++)
	{
		IBehaviourRequest* pRequest = m_Requests.GetAt(i);
		if (
			pRequest->IsKindOf(RUNTIME_CLASS(CNumbererRequest)) &&
			((CNumbererRequest*)pRequest)->GetData() == pDataBinding
			)
			pRequest->SetEnabled(bValue);
	}
}

