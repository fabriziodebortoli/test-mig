#include "stdafx.h"


#include <TbOledb\sqltable.h>
#include <TbOledb\sqlcatalog.h>	
#include <TbOledb\TbExtensionsInterface.h>	
#include <TbNameSolver\ApplicationContext.h>

#include "dbt.h"
#include "dyndbt.h"
#include "extdoc.h"
#include "BODYEDIT.H"
#include "hotlink.h"
#include "DBTIterator.h"
#include "DbtTreeEdit.h"
#include "NumbererService.h"

//includere come ultimo include all'inizio del cpp  
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//----------------------------------------------------------------------------
#define DLY_NO_DELAYED_READ
#define DLY_BROWSE_DELAYED_READ
#define DLY_EDIT_DELAYED_READ
#define UNTOUCHED	0xFFFF

DataObj* GetDataObjFromName(SqlRecord* pRec, const CString& sFieldName)
{
	DataObj* pDataObj = pRec->GetDataObjFromName(sFieldName);
	if (!pDataObj)
		pDataObj = pRec->GetDataObjFromName(_T("l_") + sFieldName);//se è un campo local, la bind aggiunge l_
	return pDataObj;
}

HKLInfo* HKLInfo::Clone()
{
	HKLInfo* pNew = new HKLInfo;
	pNew->m_DescriptionField = m_DescriptionField;
	pNew->m_pHKL = m_pHKL;
	pNew->m_strKeyField = m_strKeyField;
	return pNew;
}

//=============================================================================
//			Class CMuteDataEventsProxy
//=============================================================================
class TB_EXPORT CMuteDataEventsProxy : public CDataEventsProxy
{
	DECLARE_DYNAMIC(CMuteDataEventsProxy)
	CMuteDataEventsProxy(CObservable* pTarget)
		: CDataEventsProxy(pTarget)
	{}

public:
	virtual void Signal(CObservable* pSender, EventType eType)
	{
		//do nothing
	}
};

IMPLEMENT_DYNAMIC(CMuteDataEventsProxy, CDataEventsProxy)

////////////////////////////////////////////////////////////////////////////////
//				class DBTArray definition
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
void DBTArray::RemoveDBT(DBTSlave* pDBTSlave)
{
	for (int i = GetUpperBound(); i >= 0; i--)
		if (GetAt(i) == pDBTSlave)
		{
			RemoveAt(i);
			break;
		}
}

//-----------------------------------------------------------------------------	
DBTSlave* DBTArray::GetBy(const CTBNamespace& aNs) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		DBTSlave* pDBTSlave = GetAt(i);
		if (pDBTSlave->GetNamespace() == aNs)
			return pDBTSlave;
	}

	return NULL;
}

////////////////////////////////////////////////////////////////////////////////
//				class DBTObject definition
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTObject, CObject)

//-----------------------------------------------------------------------------	
void DBTObject::Initialize(CRuntimeClass* pClass, CAbstractFormDoc* pDocument, const CString& sName)
{
	ASSERT(pDocument);
	ASSERT(pClass);

	CRuntimeClass* pModClass = pDocument->DispatchOnModifySqlRecordClass(this, sName, pClass);
	if (pModClass && pModClass->IsDerivedFrom(pClass))
	{
		pClass = pModClass;
	}
	else if (pClass != pModClass)
	{
		ASSERT(FALSE);
	}

	m_pRecord = (SqlRecord*)pClass->CreateObject();
	m_pOldRecord = (SqlRecord*)pClass->CreateObject();

	CreateAndInit(pDocument, sName);
}
//-----------------------------------------------------------------------------	
DBTObject::DBTObject()
	:
	m_bDBTErrorPending(FALSE),
	m_bNoDelete(FALSE),
	m_pXMLDBTInfo(NULL),
	m_pClientDocOwner(NULL),
	m_bUpdated(FALSE),
	m_bRecordable(TRUE),
	m_pFnOnCheckPrimaryKey(NULL)
{
}
//-----------------------------------------------------------------------------	
DBTObject::DBTObject(CRuntimeClass* pClass, CAbstractFormDoc* pDocument, const CString& sName)
	:
	m_bDBTErrorPending(FALSE),
	m_bNoDelete(FALSE),
	m_pXMLDBTInfo(NULL),
	m_pClientDocOwner(NULL),
	m_bUpdated(FALSE),
	m_bRecordable(TRUE),
	m_pFnOnCheckPrimaryKey(NULL)

{

	Initialize(pClass, pDocument, sName);
}

//-----------------------------------------------------------------------------	
DBTObject::DBTObject(const CString& sTableName, CAbstractFormDoc* pDocument, const CString& sName)
	:
	m_bDBTErrorPending(FALSE),
	m_bNoDelete(FALSE),
	m_pXMLDBTInfo(NULL),
	m_pClientDocOwner(NULL),
	m_bUpdated(FALSE),
	m_bRecordable(TRUE),
	m_pFnOnCheckPrimaryKey(NULL)
{
	ASSERT(pDocument);

	const CDbObjectDescription* pDescription = AfxGetDbObjectDescription(sTableName);
	int nType = TABLE_TYPE;

	if (pDescription)
		nType = pDescription->GetSqlRecType();

	m_pRecord = new DynamicSqlRecord(sTableName, NULL, nType);
	m_pOldRecord = new DynamicSqlRecord(sTableName, NULL, nType);
	CreateAndInit(pDocument, sName);
}

//-----------------------------------------------------------------------------	
DBTObject::DBTObject(SqlRecord* pRecord, CAbstractFormDoc* pDocument, const CString& sName)
	:
	m_bDBTErrorPending(FALSE),
	m_bNoDelete(FALSE),
	m_pXMLDBTInfo(NULL),
	m_pClientDocOwner(NULL),
	m_bUpdated(FALSE),
	m_bRecordable(TRUE),
	m_pFnOnCheckPrimaryKey(NULL)
{
	ASSERT(pDocument);

	m_pRecord = pRecord;
	m_pOldRecord = pRecord->Clone();

	CreateAndInit(pDocument, sName);
}
//-----------------------------------------------------------------------------	
DBTObject* DBTObject::GetDBTObject(SqlRecord* pRec)
{
	return (pRec == m_pRecord || pRec == m_pOldRecord) ? this : NULL;
}

//-----------------------------------------------------------------------------	
//[TBWebMethod(name = DBTObject_GetRecord, securityhidden=true, thiscall_method=true)]
SqlRecord* DBTObject::GetDBTRecord()
{
	return m_pRecord;
}

//-----------------------------------------------------------------------------	
//[TBWebMethod(name = DBTObject_GetTable, securityhidden=true, thiscall_method=true)]
SqlTable* DBTObject::GetDBTTable()
{
	return m_pTable;
}

//-----------------------------------------------------------------------------	
CString DBTObject::GetName() const
{
	CString sDBTName = const_cast<DBTObject*>(this)->GetNamespace().GetObjectName();
	if (sDBTName.IsEmpty())
		sDBTName = m_pRecord->GetTableName();
	return sDBTName;
}

//-----------------------------------------------------------------------------	
void DBTObject::CreateAndInit(CAbstractFormDoc* pDocument, const CString& sName)
{
	m_pDocument = pDocument;
	GetNamespace().AutoCompleteNamespace(CTBNamespace::DBT, sName, pDocument->GetNamespace());

	m_pTable = new SqlTable(m_pRecord, m_pDocument->GetUpdatableSqlSession());
	// se la connessione fosse diversa dalla default devo associare le nuove info di catalog
	// all'OldSqlRecord
	// m_pRecord invece é giá stato aggiornato poiché agganciato al SqlTable
	m_pOldRecord->SetConnection(m_pDocument->GetSqlConnection());

	// vuol dire che il DBT è costruito su una view. Non è possibile effettuare il salvataggio
	m_bDBTOnView = m_pRecord->m_nType != TABLE_TYPE;
	if (m_pDocument)
	{
		m_pDocument->RegisterDBT(this);
	}

	m_pDocument->DispatchOnAfterCreateAndInitDBT(this);

}

//-----------------------------------------------------------------------------	
DBTObject::~DBTObject()
{
	m_Handler.FireDisposing(this);

	if (m_pDocument)
	{
		ASSERT_VALID(m_pDocument);
		m_pDocument->DeregisterDBT(this);
	}

	ASSERT_VALID(m_pRecord);
	ASSERT_VALID(m_pOldRecord);
	ASSERT_VALID(m_pTable);

	if (m_pTable->IsOpen())
		m_pTable->Close();

	SAFE_DELETE(m_pTable);
	SAFE_DELETE(m_pRecord);
	SAFE_DELETE(m_pOldRecord);

	SAFE_DELETE(m_pXMLDBTInfo);
}

//-----------------------------------------------------------------------------	
BOOL DBTObject::IsActive()
{
	return TRUE;
}

//-----------------------------------------------------------------------------	
CClientDocArray* DBTObject::GetClientDocs()  const
{
	return m_pDocument ? m_pDocument->m_pClientDocs : NULL;
}


// Display Error Message if enabled and always return FALSE
//-----------------------------------------------------------------------------	
BOOL DBTObject::ErrorMessage(const CString &strMsg, SqlException* e, UINT nIDP)
{
	CString msg = strMsg;
	if (e) msg += _T("\n") + e->m_strError;

	ConditionalDisplayMessage(msg, MB_OK | MB_ICONSTOP, nIDP);

	TRACE(cwsprintf(_T("DBT::ErrorMessage in class {0-%s}"), CString(GetRuntimeClass()->m_lpszClassName)));

	return FALSE;
}

//-----------------------------------------------------------------------------	
/// Displays message if messaging is enabled, put the message
/// in document's m_pMessages otherwise
void DBTObject::ConditionalDisplayMessage(const CString &strMessage, UINT nID /*= MB_OK*/, UINT nIDHelp /*= (UINT)-1*/)
{
	if (!m_pDocument)
		AfxMessageBox(strMessage, nID, nIDHelp);
	else
		m_pDocument->Message(strMessage, nID, nIDHelp);
}


//-----------------------------------------------------------------------------	
BOOL DBTObject::SetError(const CString& strError)
{
	if (m_pDocument && m_pDocument->IsInUnattendedMode())
	{
		if (m_pDocument->m_pMessages)
			m_pDocument->m_pMessages->Add(strError);
		return TRUE;
	}

	if (!m_bDBTErrorPending)
		SetDBTError(strError);

	return TRUE;
}

//-----------------------------------------------------------------------------	
void DBTObject::SetDBTError(const CString& strError)
{
	m_strDBTError = strError;
}

//-----------------------------------------------------------------------------	
BOOL DBTObject::Open()
{
	ASSERT(m_pTable);
	ASSERT_VALID(m_pTable);
	ASSERT(!m_pTable->IsOpen());

	if (m_pTable->IsOpen())
		return FALSE;

	BOOL bOk = FALSE;
	TRY
	{
		m_pTable->Open(!m_bDBTOnView, GetCursorType());

	// verifico prima se il documento vuole sostituire la query del dbt
	if (!m_pDocument->OnChangeDBTDefineQuery(this, m_pTable))
		OnDefineQuery();

	// chiedo al documento (e ai suoi clientdoc) l'eventuale aggiunta di parametri alla query
	// del dbt
	m_pDocument->DispatchOnModifyDBTDefineQuery(this, m_pTable);

	if (!m_pDocument->OnChangeDBTPrepareQuery(this, m_pTable))
		OnPrepareQuery();
	m_pDocument->DispatchOnModifyDBTPrepareQuery(this, m_pTable);
	}

		CATCH(SqlException, e)
	{
		ConditionalDisplayMessage(e->m_strError);
		return FALSE;
	}
	END_CATCH

		return TRUE;
}

//-----------------------------------------------------------------------------	
void DBTObject::Close()
{
	if (m_pTable && m_pTable->IsOpen())
		m_pTable->Close();
}

//-----------------------------------------------------------------------------	
void DBTObject::Init()
{
	m_pRecord->Init();
	m_pOldRecord->Init();
}

//-----------------------------------------------------------------------------	
BOOL DBTObject::FindData(BOOL bPrepareOld)
{
	ASSERT(m_pTable);

	// inizializza i parametri per la query in modo conforme alla OnDefineQuery
	if (!m_pDocument->OnChangeDBTPrepareQuery(this, m_pTable))
		OnPrepareQuery();
	m_pDocument->DispatchOnModifyDBTPrepareQuery(this, m_pTable);

	BOOL bReturn = FALSE;

	TRY
	{
		m_pTable->Query();
		if (bPrepareOld) *m_pOldRecord = *m_pRecord;
		bReturn = TRUE;
	}
		CATCH(SqlException, e)
	{
		return ErrorMessage(cwsprintf(_TB("DBT {0-%s}: Query on table {1-%s} failed."), CString(GetRuntimeClass()->m_lpszClassName), m_pRecord->GetTableName()), e);
	}
	END_CATCH

		return bReturn && !m_pTable->IsEmpty();
}

//-----------------------------------------------------------------------------	
BOOL DBTObject::AddNew(BOOL bInit /*=TRUE*/)
{
	ASSERT(m_pTable);

	CApplicationContext::MacroRecorderStatus pStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;

	TRY
	{
		if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

		m_pTable->AddNew(bInit);
	}
		CATCH(SqlException, e)
	{
		if (!m_bRecordable)
			AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

		return ErrorMessage(cwsprintf(_TB("DBT {0-%s}: Unable to add a new record in table {1-%s}."), CString(GetRuntimeClass()->m_lpszClassName), m_pRecord->GetTableName()), e);
	}
	END_CATCH

		if (!m_bRecordable)
			AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL DBTObject::Edit()
{
	ASSERT(m_pTable);

	if (m_pTable->NoCurrent())
		return ErrorMessage(_TB("Unable to edit the current document."), NULL);

	CApplicationContext::MacroRecorderStatus pStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;

	TRY
	{
		if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

		m_pTable->Edit();
	}
		CATCH(SqlException, e)
	{
		if (!m_bRecordable)
			AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

		return ErrorMessage(cwsprintf(_TB("DBT {0-%s}: Unable to edit record in table {1-%s}."), CString(GetRuntimeClass()->m_lpszClassName), m_pRecord->GetTableName()), e);
	}
	END_CATCH

		if (!m_bRecordable)
			AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

	return TRUE;
}

// Guardare l'esempio di delete di enroll\step4\addform.cpp
//-----------------------------------------------------------------------------	
BOOL DBTObject::Delete()
{
	ASSERT(m_pTable);

	if (m_pTable->IsEOF() || m_pTable->IsBOF())
		return ErrorMessage(_TB("Unable to delete the current document."), NULL);

	CApplicationContext::MacroRecorderStatus pStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;

	TRY
	{
		if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

	// delete non e` lecita in modo edit o addnew pertanto occorre uscire
	// da questi stati con l'istruzione sotto che non muove il current record.
	m_pTable->Move(SqlTable::E_MOVE_REFRESH);

	// cancellazione effettiva utilizzando i dati di chiave primaria del 
	// record prima di eventuali modifiche in data entry
	m_pTable->Delete(m_pOldRecord);
	}
		CATCH(SqlException, e)
	{
		if (!m_bRecordable)
			AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

		return ErrorMessage(cwsprintf(_TB("DBT {0-%s}: Deletion on table {1-%s} failed."), CString(GetRuntimeClass()->m_lpszClassName), m_pRecord->GetTableName()), e);
	}
	END_CATCH

		if (!m_bRecordable)
			AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL DBTObject::Update()
{
	ASSERT(m_pTable);

	CApplicationContext::MacroRecorderStatus pStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;

	TRY
	{
		if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

		m_bUpdated = FALSE;
		// Aggiorna il record con i dati di chiave primaria del record salvato
		// in fase di lettura. La chiave primaria del nuovo record potrebbe
		// essere stata cambiata in fase di editing , se abilitato
		if (m_pTable->Update(m_pOldRecord) == UPDATE_SUCCESS)
			m_bUpdated = TRUE;
	}
		CATCH(SqlException, e)
	{
		if (!m_bRecordable)
			AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

		return ErrorMessage(cwsprintf(_TB("DBT {0-%s}: Saving on table {1-%s} failed."), CString(GetRuntimeClass()->m_lpszClassName), m_pRecord->GetTableName()), e);
	}
	END_CATCH

		if (!m_bRecordable)
			AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL DBTObject::LoadXMLDBTInfo()
{
	CXMLDocInfo* pXMLDocInfo = NULL;
	if (!m_pDocument || !(pXMLDocInfo = m_pDocument->GetXMLDocInfo()))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (m_pXMLDBTInfo) // le informazioni sono state già caricate....
		return TRUE;

	m_pXMLDBTInfo = new CXMLDBTInfo;
	// il terzo parametro mi fa capire se è stato agganciato da un clientdoc
	if (pXMLDocInfo->LoadXMLDBTInfo(GetNamespace(), m_pXMLDBTInfo, GetClientDocOwner() ? GetClientDocOwner()->m_Namespace : CTBNamespace()))
	{
		return TRUE;
	}

	delete m_pXMLDBTInfo;
	m_pXMLDBTInfo = NULL;

	return FALSE;
}

//-----------------------------------------------------------------------------	
void DBTObject::InstantiateFromClientDoc(CClientDoc* pClientDoc)
{
	m_pClientDocOwner = pClientDoc;
	GetNamespace().AutoCompleteNamespace(CTBNamespace::DBT, GetNamespace().GetObjectName(), pClientDoc->m_Namespace);
}

//-----------------------------------------------------------------------------	
BOOL DBTObject::OnCheckPrimaryKey()
{
	// se ho un overload lo chiamo, altrimenti eseguo l'algoritmo di default
	if (m_pFnOnCheckPrimaryKey)
		return m_pFnOnCheckPrimaryKey(m_pRecord, -1) == NULL;

	return CheckPrimaryKey(m_pRecord) == NULL;
}

//-----------------------------------------------------------------------------	
DataObj* DBTObject::CheckPrimaryKey(SqlRecord* pRecord)
{
	SqlRecordItem* pItem;
	DataObj* pDataObj;
	const CUIntArray& aIndexes = pRecord->GetPrimaryKeyIndexes();
	for (int i = 0; i <= aIndexes.GetUpperBound(); i++)
	{
		UINT index = aIndexes[i];
		pItem = pRecord->GetAt(index);
		pDataObj = pItem->GetDataObj();
		if (pDataObj && pDataObj->GetDataType() != DataType::Bool && pDataObj->GetDataType().m_wType != DataType::Enum.m_wType && pItem->IsEmpty())
			return pDataObj;
	}

	return NULL;
}

//-----------------------------------------------------------------------------	
void DBTObject::SetOnCheckPrimaryKeyFunPtr(DATAOBJ_ROW_FUNC funPtr)
{
	m_pFnOnCheckPrimaryKey = funPtr;
}

//-----------------------------------------------------------------------------	
void DBTObject::OnPreparePrimaryKey()
{
}

//-----------------------------------------------------------------------------	
BOOL DBTObject::OnOkTransaction()
{
	return TRUE;
}

//-----------------------------------------------------------------------------	
void DBTObject::OnDisableControlsForAddNew()
{
}

//-----------------------------------------------------------------------------	
void DBTObject::OnDisableControlsAlways()
{

}

//-----------------------------------------------------------------------------	
void DBTObject::SetFindable()
{
	if (IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		return;

	if (m_pRecord && m_pDocument)
	{
		for (int i = 0; i < m_pRecord->GetSizeEx(); i++)
		{
			SqlRecordItem* pRecItm = m_pRecord->GetAt(i);
			if (!pRecItm->GetColumnInfo())
				continue;
			if (pRecItm->GetColumnInfo()->m_bVirtual)
				continue;

			DataObj* pDataObj = pRecItm->GetDataObj();
			if (!pDataObj)
				continue;

			CParsedCtrl* pCtrl = m_pDocument->GetLinkedParsedCtrl(pDataObj);
			if (!pCtrl)
			{
				pDataObj->SetFindable(FALSE);
				continue;
			}

			if (IsKindOf(RUNTIME_CLASS(DBTSlave)) && !pDataObj->IsEmpty())
				pDataObj->Clear();

			pDataObj->SetFindable(TRUE);
		}
	}
}

//-----------------------------------------------------------------------------	
void DBTObject::SetReadOnly(BOOL bReadOnly/* = TRUE*/)
{
	// modifico lo stato dei field del sqlRecord
	for (int nCol = 0; nCol <= m_pRecord->GetUpperBound(); nCol++)
		m_pRecord->GetDataObjAt(nCol)->SetReadOnly(bReadOnly);
}

//-----------------------------------------------------------------------------	
void DBTObject::SetAlwaysReadOnly(BOOL bReadOnly/* = TRUE*/)
{
	// modifico lo stato dei field del sqlRecord
	for (int nCol = 0; nCol <= m_pRecord->GetUpperBound(); nCol++)
		m_pRecord->GetDataObjAt(nCol)->SetAlwaysReadOnly(bReadOnly);
}

//-----------------------------------------------------------------------------	
CNumbererBinder* DBTObject::GetNumbererBinder()
{
	return m_pDocument ? m_pDocument->GetNumbererBinder() : NULL;
}

//-----------------------------------------------------------------------------	
CNumbererRequest* DBTObject::BindAutoincrement(DataObj* pDataBinding, const CString& sEntity)
{
	return GetNumbererBinder() ? GetNumbererBinder()->BindAutoincrement(this, pDataBinding, sEntity) : NULL;
}

//-----------------------------------------------------------------------------	
CNumbererRequest* DBTObject::BindAutonumber(DataObj* pDataBinding, const CString& sEntity, DataDate* pDataDate)
{
	if (!GetNumbererBinder())
		return NULL;

	CNumbererRequest* pRequest = new CNumbererRequest(this, pDataBinding, sEntity);

	CString sColName = GetRecord()->GetColumnName(pRequest->GetData());
	pRequest->SetIsPrimaryKey(GetRecord()->IsSpecial(pRequest->GetData()));

	if (GetOldRecord())
	{
		CString sColName = GetRecord()->GetColumnName(pRequest->GetData());
		pRequest->SetOldData(GetOldRecord()->GetDataObjFromColumnName(sColName));
	}
	else
	{
		TRACE(_T("Missing OldRecord"));
		ASSERT(FALSE);
	}

	CDateNumbererRequestParams* pParams = NULL;

	if (pDataDate)
	{
		pParams = new CDateNumbererRequestParams(pDataDate);
		pParams->SetDocDate(pDataDate);
		// If non exist old record, bind current record, because delete transaction use the old data
		if (GetOldRecord())
		{
			CString sColName = GetRecord()->GetColumnName(pDataDate);
			pParams->SetOldDocDate((DataDate*)GetOldRecord()->GetDataObjFromColumnName(sColName));
		}
		else
		{
			TRACE(_T("Missing OldRecord"));
			ASSERT(FALSE);
		}
	}

	GetNumbererBinder()->BindAutonumber(pRequest, pParams);
	return pRequest;
}

//--------------------------------------------------------------------------
BOOL DBTObject::PrepareSymbolTable(SymTable* pTable)
{
	if (!m_pRecord || m_pRecord->GetSizeEx() == 0)
		return FALSE;

	CString sTitle(m_pRecord->GetTableName());
	CString sDBTName(GetName());

	// table context symbols
	DataLng me((long)this);
	me.SetAsHandle();
	SymField* pField = new SymField(sDBTName /*+ m_pRecord->GetTableName()*/, DataType::Object, SpecialReportField::NO_INTERNAL_ID, &me);
	if (!sTitle.IsEmpty())
		pField->SetTitle(sTitle);
	pField->AddMethods(GetRuntimeClass(), AfxGetAddOnAppsTable()->GetMapWebClass());
	pTable->Add(pField);	pField->GetData()->SetValid(TRUE);

	sDBTName += '.';
	for (int i = 0; i < m_pRecord->GetSizeEx(); i++)
	{
		SqlRecordItem* pItem = m_pRecord->GetAt(i);
		//const SqlColumnInfo* pCol = pItem->GetColumnInfo();
		CString sName = sDBTName + pItem->GetColumnName(); // (pCol ? pCol->GetQualifiedColumnName() : pItem->GetColumnName());

		SymField* pF = new SymField(sName, pItem->GetDataObj()->GetDataType(), SpecialReportField::NO_INTERNAL_ID, pItem->GetDataObj(), FALSE);

		if (IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
			pF->SetProvider(dynamic_cast<DBTSlaveBuffered*>(this));	//Il DBT sarà il provider del dato tramite il metodo virtuale GetData

		if (GetDocument())
			GetDocument()->PrepareSymbolField(pF);

		pTable->Add(pF);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------	
void DBTSlave::OnPreparePrimaryKey()
{
	//Fa esattamente quello che fa il suo papa` ma non rimuovo il metodo perche`
	//nel papa` e` protetto mentre qui e` pubblico.
	__super::OnPreparePrimaryKey();
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DBTObject::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDBTObject");

	CObject::Dump(dc);
}

void DBTObject::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

////////////////////////////////////////////////////////////////////////////////
//				class DBTMaster definition
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTMaster, DBTObject)

//-----------------------------------------------------------------------------	
DBTMaster::DBTMaster
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument,
	const CString&		sName
)
	:
	DBTObject(pClass, pDocument, sName)
{
	ASSERT(pDocument);
	m_pDBTSlaves = new DBTArray;
	m_pTable->SetDBTMasterQuery();
}

//-----------------------------------------------------------------------------	
DBTMaster::DBTMaster
(
	const CString&		sTableName,
	CAbstractFormDoc*	pDocument,
	const CString&		sName
)
	:
	DBTObject(sTableName, pDocument, sName)
{
	ASSERT(pDocument);
	m_pDBTSlaves = new DBTArray;
	m_pTable->SetDBTMasterQuery();
}

//-----------------------------------------------------------------------------	
DBTMaster::DBTMaster
(
	SqlRecord*			pRecord,
	CAbstractFormDoc*	pDocument,
	const CString&		sName
)
	:
	DBTObject(pRecord, pDocument, sName)
{
	ASSERT(pDocument);
	m_pDBTSlaves = new DBTArray;
	m_pTable->SetDBTMasterQuery();
}

//-----------------------------------------------------------------------------	
DBTMaster::~DBTMaster()
{
	ASSERT(m_pDBTSlaves);

	// remove also attached DBTSlaves (MUST DO)	
	delete m_pDBTSlaves;
	m_pDBTSlaves = NULL;
}

//-----------------------------------------------------------------------------	
CString	DBTMaster::GetSortString() const
{
	return m_pTable->m_strSort;
}

//-----------------------------------------------------------------------------	
void DBTMaster::Attach(DBTSlave* pDBTSlave)
{
	ASSERT(m_pDBTSlaves);
	ASSERT(pDBTSlave);
	ASSERT(pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlave)));

	pDBTSlave->m_pDBTMaster = this;
	pDBTSlave->m_pMasterRecord = this->GetRecord();
	m_pDBTSlaves->Add(pDBTSlave);
}

//-----------------------------------------------------------------------------	
BOOL DBTMaster::IsValidRecordsSchema(CDiagnostic* pDiagnostic)
{
	BOOL bValid = m_pRecord->IsValid();

	if (!bValid)
		pDiagnostic->Add(cwsprintf(_TB("Table {0-%s} has not a valid table schema accordingly company database catalog. See next to display binding errors."), m_pRecord->GetTableName()));

	DBTIterator iterator(this);
	DBTSlave* pDbt;
	while (pDbt = (DBTSlave*)iterator.GetNextDbtObject())
	{
		if (!pDbt->m_pRecord->IsValid())
		{
			pDiagnostic->Add(cwsprintf(_TB("Table {0-%s} has not a valid table schema accordingly company database catalog. See next to display binding errors."), pDbt->m_pRecord->GetTableName()));
			bValid = FALSE;
		}
	}
	return bValid;
}

//-----------------------------------------------------------------------------	
void DBTMaster::PrepareFindQuery(SqlTable* pTable)
{
	OnPrepareFindQuery(pTable);
	m_pDocument->DispatchOnPrepareFindQuery(pTable);
}

//-----------------------------------------------------------------------------	
void DBTMaster::PrepareBrowser(SqlTable* pTable)
{
	OnPrepareBrowser(pTable);
	m_pDocument->DispatchOnPrepareBrowser(pTable);
}

//-----------------------------------------------------------------------------	
void DBTMaster::OnPrepareBrowser(SqlTable* pTable)
{
	//@@ TODO
	// in futuro quando sara' corretto il radar in modo da non riselezionare
	// tutto, verificare cosa mettere in questa routine
	if (pTable)
		pTable->SelectAll();
}

//-----------------------------------------------------------------------------	
void DBTMaster::OnPrepareForXImportExport(SqlTable* pTable)
{
	if (pTable)
		pTable->SelectAll();
}

//-----------------------------------------------------------------------------	
void DBTMaster::OnBeforeXMLExport()
{

}

//-----------------------------------------------------------------------------	
void DBTMaster::OnAfterXMLExport()
{

}

//-----------------------------------------------------------------------------	
BOOL DBTMaster::OnOkXMLExport()
{
	return TRUE;
}

//-----------------------------------------------------------------------------	
void DBTMaster::OnBeforeXMLImport()
{
}

//-----------------------------------------------------------------------------	
void DBTMaster::OnAfterXMLImport()
{
}

//-----------------------------------------------------------------------------	
BOOL DBTMaster::OnOkXMLImport()
{
	return TRUE;
}

//-----------------------------------------------------------------------------	
void DBTMaster::OnDefineQuery()
{

}

//-----------------------------------------------------------------------------	
void DBTMaster::OnPrepareQuery()
{
}

//-----------------------------------------------------------------------------	
void DBTMaster::ForcePreparePrimaryKey()
{
	DBTIterator iterator(this);
	DBTSlave* pDbt;
	while (pDbt = (DBTSlave*)iterator.GetNextDbtObject())
	{
		if (pDbt->GetTable()->IsOpen())
			pDbt->ForcePreparePrimaryKey();
	}
}


// Non sempre (anzi quasi mai) e' gestito il lock pessimistico
//-----------------------------------------------------------------------------	
BOOL DBTMaster::Open()
{
	TRY
	{
		if (!DBTObject::Open())
		return FALSE;
	}
		CATCH(SqlException, e)
	{
		return ErrorMessage(_TB("DBTMaster: Open failed. Query not executed."), NULL);
	}
	END_CATCH

		// try to open all attached slaves (close all opened if error)
		for (int i = 0; i <= m_pDBTSlaves->GetUpperBound(); i++)
		{
			if (m_pDBTSlaves->GetAt(i)->m_bOnlyDelete)
				continue;

			if (!m_pDBTSlaves->GetAt(i)->Open())
			{
				// Attenzione. Se il DBT e' usato solo per essere deletato
				// viene aperto solo durante la Delete
				for (i--; i >= 0; i--)
					if (!m_pDBTSlaves->GetAt(i)->m_bOnlyDelete)
						m_pDBTSlaves->GetAt(i)->Close();

				// Chiude il DBTMaster
				DBTObject::Close();
				return FALSE;
			}
		}

	return TRUE;
}

//-----------------------------------------------------------------------------	
void DBTMaster::Close()
{
	// if master is not open then avoid to close it and also
	// avoid to close slave (if bad master open, slaves are not opened)
	if (m_pTable && m_pTable->IsOpen())
	{
		// chiude gli slave attaccati e che non siano slave di sola
		// cancellazione (gestione rilassata della persistenza)
		//
		DBTIterator iterator(this);
		DBTSlave* pDbt;
		while (pDbt = (DBTSlave*)iterator.GetNextDbtObject())
		{
			if (!pDbt->m_bOnlyDelete)
				pDbt->Close();
		}
		// close master record yet!	
		DBTObject::Close();
	}
}

//-----------------------------------------------------------------------------	
void DBTMaster::OnEnableControlsForFind()
{

}

//-----------------------------------------------------------------------------	
void DBTMaster::OnDisableControlsForEdit()
{
	const CUIntArray& aIndexes = m_pRecord->GetPrimaryKeyIndexes();
	for (int j = 0; j <= aIndexes.GetUpperBound(); j++)
	{
		SqlRecordItem* pItem = m_pRecord->GetAt(aIndexes.GetAt(j));
		pItem->GetDataObj()->SetReadOnly(TRUE);
	}
}

//-----------------------------------------------------------------------------	
void DBTMaster::EnableControlsForFind()
{
	// it enables only master data that are currently visible and
	// only the data that are not declared findable by the developer
	SetFindable();

	if (m_pDocument && GetDBTSlaves() && m_pDocument->CanFindOnSlave())
	{
		for (int d = 0; d < GetDBTSlaves()->GetCount(); d++)
		{
			DBTSlave* pDBT = GetDBTSlaves()->GetAt(d);
			if (!pDBT || pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))) continue;

			// enum Master and Slaves Foreign keys to simulate inner join on subquery 
			SqlForeignKeysReader aFKReader;
			aFKReader.LoadForeignKeys
			(
				GetRecord()->GetTableName(),
				pDBT->GetRecord()->GetTableName(),
				m_pDocument->GetReadOnlySqlSession()
			);
			if (!aFKReader.GetSize())
				continue;

			pDBT->SetFindable();
		}
	}

	OnEnableControlsForFind();
	GetClientDocs()->OnEnableControlsForFind(this);

	DBTIterator iterator(this);
	DBTSlave* pDbt;
	while (pDbt = (DBTSlave*)iterator.GetNextDbtObject())
	{
		if (!pDbt->m_bOnlyDelete)
		{
			pDbt->OnEnableControlsForFind();
			GetClientDocs()->OnEnableControlsForFind(pDbt);
		}
	}
}

//-----------------------------------------------------------------------------	
void DBTMaster::DisableControlsForAddNew()
{
	OnDisableControlsForAddNew();
	GetClientDocs()->OnDisableControlsForAddNew(this);
	DBTIterator iterator(this);
	DBTSlave* pDbt;
	while (pDbt = (DBTSlave*)iterator.GetNextDbtObject())
	{
		if (!pDbt->m_bOnlyDelete)
		{
			pDbt->OnDisableControlsForAddNew();
			GetClientDocs()->OnDisableControlsForAddNew(pDbt);
		}
	}
}

//-----------------------------------------------------------------------------	
void DBTMaster::DisableControlsForEdit()
{
	OnDisableControlsForEdit();
	GetClientDocs()->OnDisableControlsForEdit(this);
	DBTIterator iterator(this);
	DBTSlave* pDbt;
	while (pDbt = (DBTSlave*)iterator.GetNextDbtObject())
	{
		if (!pDbt->m_bOnlyDelete)
		{
			pDbt->OnDisableControlsForEdit();
			GetClientDocs()->OnDisableControlsForEdit(pDbt);
		}
	}
}

//-----------------------------------------------------------------------------	
void DBTMaster::DisableControlsAlways()
{
	OnDisableControlsAlways();
	GetClientDocs()->OnDisableControlsAlways(this);
	DBTIterator iterator(this);
	DBTSlave* pDbt;
	while (pDbt = (DBTSlave*)iterator.GetNextDbtObject())
	{
		if (!pDbt->m_bOnlyDelete)
		{
			pDbt->OnDisableControlsAlways();
			GetClientDocs()->OnDisableControlsAlways(pDbt);
		}
	}
}

//-----------------------------------------------------------------------------	
void DBTMaster::Init()
{
	DBTObject::Init();
	InitSlaves();
}

//-----------------------------------------------------------------------------	
void DBTMaster::InitSlaves()
{
	// try to init all attached slaves		
	DBTIterator iterator(this);
	DBTSlave* pDbt;
	while (pDbt = (DBTSlave*)iterator.GetNextDbtObject())
		pDbt->Init();
}

//-----------------------------------------------------------------------------	
DBTObject* DBTMaster::GetDBTObject(SqlRecord* pRec)
{
	DBTObject* pDBT = __super::GetDBTObject(pRec);
	if (pDBT)
		return pDBT;

	for (int i = 0; i <= m_pDBTSlaves->GetUpperBound(); i++)
	{
		DBTSlave* pSlave = m_pDBTSlaves->GetAt(i);
		pDBT = pSlave->GetDBTObject(pRec);
		if (pDBT)
			return pDBT;
	}
	return NULL;
}

//-----------------------------------------------------------------------------	
void DBTMaster::GetJson(BOOL bWithChildren, CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound)
{
	jsonSerializer.OpenObject(GetName());

	SqlRecord *pRecord = GetRecord();
	if (pRecord)
	{
		pRecord->GetJson(jsonSerializer, bOnlyWebBound);
	}
	jsonSerializer.CloseObject();

	if (bWithChildren)
	{
		DBTArray* pSlaves = GetDBTSlaves();
		for (int i = 0; i < pSlaves->GetCount(); i++)
		{
			DBTSlave* pSlave = pSlaves->GetAt(i);
			pSlave->GetJson(bWithChildren, jsonSerializer, bOnlyWebBound);
		}
	}

}

//-----------------------------------------------------------------------------	
void DBTMaster::SetJson(BOOL bWithChildren, CJsonParser& jsonParser)
{
	if (jsonParser.BeginReadObject(GetName()))
	{
		SqlRecord *pRecord = GetRecord();
		if (pRecord)
			pRecord->SetJson(jsonParser);
		jsonParser.EndReadObject();
	}
	if (bWithChildren)
	{
		DBTArray* pSlaves = GetDBTSlaves();
		if (pSlaves->GetCount())
		{
			for (int i = 0; i < pSlaves->GetCount(); i++)
				pSlaves->GetAt(i)->SetJson(bWithChildren, jsonParser);
		}
	}

}


//-----------------------------------------------------------------------------	
BOOL DBTMaster::Exist()
{
	SqlRecord* pTmp = m_pRecord->Create();
	*pTmp = *m_pRecord;

	// inizializza i parametri per la query in modo conforme alla OnDefineQuery
	if (!m_pDocument->OnChangeDBTPrepareQuery(this, m_pTable))
		OnPrepareQuery();
	m_pDocument->DispatchOnModifyDBTPrepareQuery(this, m_pTable);

	BOOL bReturn = FALSE;

	TRY
	{
		m_pTable->Query();
		bReturn = TRUE;
	}
		CATCH(SqlException, e)
	{
		return ErrorMessage(cwsprintf(_TB("DBT {0-%s}: Query on table {1-%s} failed."), CString(GetRuntimeClass()->m_lpszClassName), m_pRecord->GetTableName()), e);
	}
	END_CATCH

		*m_pRecord = *pTmp;
	delete pTmp;

	return bReturn && !m_pTable->IsEmpty();
}
//-----------------------------------------------------------------------------	
BOOL DBTMaster::FindData(BOOL bPrepareOld)
{
	AfxGetApp()->BeginWaitCursor();
	InitSlaves();
	BOOL bOk = DBTObject::FindData(bPrepareOld);

	if (bOk)
		for (int i = 0; i <= m_pDBTSlaves->GetUpperBound(); i++)
		{
			DBTSlave* pSlave = m_pDBTSlaves->GetAt(i);
			if (pSlave->m_bOnlyDelete)
				continue;

			if (!pSlave->FindData(bPrepareOld) && bOk)
				bOk = FALSE;
		}

	AfxGetApp()->EndWaitCursor();
	return bOk;
}

//-----------------------------------------------------------------------------	
BOOL DBTMaster::AddNew(BOOL bInit /*=TRUE*/)
{
	if (!DBTObject::AddNew(bInit))
		return FALSE;

	// try to Append all attached slaves		
	BOOL bOk = TRUE;
	for (int i = 0; i <= m_pDBTSlaves->GetUpperBound(); i++)
	{
		DBTSlave* pSlave = m_pDBTSlaves->GetAt(i);
		if (pSlave->m_bOnlyDelete)
			continue;

		if (!pSlave->AddNew(bInit) && bOk)
			bOk = FALSE;
	}

	return bOk;
}

//-----------------------------------------------------------------------------	
BOOL DBTMaster::Edit()
{
	if (!DBTObject::Edit())
		return FALSE;

	// try to rewrite all attached slaves		
	BOOL bOk = TRUE;
	for (int i = 0; i <= m_pDBTSlaves->GetUpperBound(); i++)
	{
		DBTSlave* pSlave = m_pDBTSlaves->GetAt(i);
		if (pSlave->m_bOnlyDelete)
			continue;

		if (!pSlave->Edit() && bOk)
			bOk = FALSE;
	}

	return bOk;
}

//-----------------------------------------------------------------------------	
BOOL DBTMaster::Update()
{
	CString strSlaveTableUpdated;
	int nUpdatedSlave = 0;

	BOOL bOk = DBTObject::Update();
	if (!bOk)
		return FALSE;

	// try to rewrite all attached slaves		
	for (int i = 0; i <= m_pDBTSlaves->GetUpperBound(); i++)
	{
		DBTSlave* pSlave = m_pDBTSlaves->GetAt(i);
		if (pSlave->m_bOnlyDelete)
			continue;

		if (!pSlave->Update())
			bOk = FALSE;

		if (bOk && pSlave->m_bUpdated)
			nUpdatedSlave++;

		if (!bOk)
			return FALSE;
	}

	// @@AUDITING e TBModified
	// se la master non é stata modificata ma é stato modificato almeno un 
	// dbtslave allora traccio la modifica alla tabella del master
	// devo anche modificare il campo TBModified
	if (!m_bUpdated && nUpdatedSlave > 0)
	{
		m_pRecord->SetDirty();
		bOk = DBTObject::Edit();
		m_pTable->Update(m_pOldRecord, TRUE);
	}
	return bOk;
}

//-----------------------------------------------------------------------------	
BOOL DBTMaster::Delete()
{
	if (m_bDBTOnView) return TRUE;

	int nSlaveDeleted = 0;
	BOOL bOk = TRUE;

	// try to delete all current record for all attached slaves		
	// before deleting master Dbt
	for (int i = 0; i <= m_pDBTSlaves->GetUpperBound(); i++)
	{
		DBTSlave* pDBTSlave = m_pDBTSlaves->GetAt(i);
		ASSERT(pDBTSlave);

		//posso avere un dbt che devo visualizzare senza andare in edit ma lo devo cancellare
		if ((pDBTSlave->m_bDBTOnView && !pDBTSlave->m_bOnlyDelete) || pDBTSlave->m_bNoDelete)
			continue;

		// Gestisce la cancellazione di Slave connessi solo per mantenere
		// la persistenza relazionale, ma non gestiti in input
		if (pDBTSlave->m_bOnlyDelete)
		{
			if (
				pDBTSlave->Open() &&
				pDBTSlave->FindData() &&
				pDBTSlave->Delete()
				)
			{
				nSlaveDeleted++;
				pDBTSlave->Close();
			}
			else
			{
				pDBTSlave->Close();
				return FALSE;
			}
		}
		else
		{
			if (pDBTSlave->Delete())
				nSlaveDeleted++;
			else
				return FALSE;
		}

	}

	// DBTMaster viene deletato alla fine se tutto e' Ok ed inoltre
	// se non ne e' stata inibita la cancellazione (utile nelle relazioni
	// uno a molti gestite con semplice slave (esempio Clienti per articolo)
	if (bOk && !m_bNoDelete)
		bOk = DBTObject::Delete();

	//@@AUDITING
	// se é stato cancellato almeno un dbtslave allora traccio la modifica 
	// alla tabella del master
	if (m_bNoDelete && nSlaveDeleted > 0)
		m_pRecord->GetTableInfo()->GetSqlCatalogEntry()->TraceOperation(AUDIT_UPDATE_OP, m_pTable);

	return bOk;
}

//	Overridable :
//
//-----------------------------------------------------------------------------	
BOOL DBTMaster::CheckTransaction()
{
	if (m_bDBTOnView)
	{
		// Prima controlla la chiave primaria
		m_strDBTError = _TB("Unable to save the document. The master is built on a database view.");

		ConditionalDisplayMessage(m_strDBTError, MB_OK | MB_ICONSTOP);

		return FALSE;
	}

	// riprepara la chiave perche` possono essere stati modificati i dataobj
	// del master che fanno parte della chiave dello slave, permettendo inoltre
	// eventuali valorizzazioni definite dal programmatore e non chieste in input
	OnPreparePrimaryKey();
	GetClientDocs()->OnPreparePrimaryKey(this);

	// Prima controlla la chiave primaria
	m_strDBTError.Empty();
	if (!OnCheckPrimaryKey())
	{
		if (m_strDBTError.IsEmpty())
			m_strDBTError = _TB("Some main reference data are not complete.\r\nTransaction failed.");
		ConditionalDisplayMessage(m_strDBTError, MB_OK | MB_ICONSTOP);
		return FALSE;
	}

	// Devono essere ok anche tutte le sub transazioni di slaves		
	for (int i = 0; i <= m_pDBTSlaves->GetUpperBound(); i++)
	{
		DBTSlave* pSlave = m_pDBTSlaves->GetAt(i);
		if (pSlave->m_bOnlyDelete)
			continue;

		if (!pSlave->CheckTransaction())
			return FALSE;
	}

	return OnOkTransaction();
}

//-----------------------------------------------------------------------------	
BOOL DBTMaster::PrepareSymbolTable(SymTable* pTable)
{
	BOOL bOk = __super::PrepareSymbolTable(pTable);

	for (int i = 0; i <= m_pDBTSlaves->GetUpperBound(); i++)
	{
		DBTSlave* pSlave = m_pDBTSlaves->GetAt(i);

		pSlave->PrepareSymbolTable(pTable);
	}

	return bOk;
}

//-----------------------------------------------------------------------------	
void DBTMaster::SetNoPreloadStep()
{
	DBTIterator iterator(this);
	DBTSlave* pDbt;
	while (pDbt = (DBTSlave*)iterator.GetNextDbtObject())
		pDbt->SetPreloadStep(-1);
}

//-----------------------------------------------------------------------------	
void DBTMaster::GoInBrowseMode()
{
	DBTIterator iterator(this);
	DBTSlave* pDbt;
	while (pDbt = (DBTSlave*)iterator.GetNextDbtObject())
		pDbt->OnGoInBrowseMode();
}

//-----------------------------------------------------------------------------	
BOOL DBTMaster::LoadXMLDBTInfo()
{
	if (!DBTObject::LoadXMLDBTInfo())
		return FALSE;

	BOOL bOk = TRUE;
	//TODO @@BAUZI considerare gli slavable
	for (int i = 0; i <= m_pDBTSlaves->GetUpperBound(); i++)
	{
		DBTSlave* pSlave = m_pDBTSlaves->GetAt(i);
		if (pSlave->IsDBTOnView())
			continue;

		bOk = pSlave->LoadXMLDBTInfo() && bOk;
	}
	return bOk;
}

//-----------------------------------------------------------------------------	
//BOOL DBTMaster::Parse (CXMLNode* pNode, BOOL bWithAttributes/* = TRUE*/, BOOL bParseLocal/* = FALSE*/)
//{
//	ASSERT_VALID(m_pRecord);
//
//	return m_pRecord->Parse (pNode, bWithAttributes, bParseLocal);
//}

BOOL DBTMaster::UnParse(CXMLNode* pRootNode, BOOL bWithAttributes/* = TRUE*/, BOOL bUnParseLocal/* = FALSE*/, BOOL bUnParseSlavable/* = FALSE*/)
{
	ASSERT_VALID(m_pRecord);
	ASSERT_VALID(m_pRecord);

	CXMLNode* pParentNode = pRootNode->CreateNewChild(GetName());
	pParentNode->SetAttribute(L"type", L"DBTMaster");

	CXMLNode* pMasterNode = pParentNode->CreateNewChild(m_pRecord->GetTableName());
	m_pRecord->UnParse(pMasterNode, bWithAttributes, bUnParseLocal);

	for (int i = 0; i <= m_pDBTSlaves->GetUpperBound(); i++)
	{
		DBTSlave* pSlave = m_pDBTSlaves->GetAt(i);

		pSlave->UnParse(pParentNode, bWithAttributes, bUnParseLocal, bUnParseSlavable);
	}

	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DBTMaster::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDBTMaster");

	DBTObject::Dump(dc);
}

void DBTMaster::AssertValid() const
{
	DBTObject::AssertValid();
}
#endif //_DEBUG

////////////////////////////////////////////////////////////////////////////////
//				class DBTSlave definition
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNCREATE(DBTSlave, DBTObject)

//-----------------------------------------------------------------------------	
DBTSlave::DBTSlave()
	:
	m_bAllowEmpty(!ALLOW_EMPTY_BODY),
	m_bEmpty(TRUE),
	m_bOnlyDelete(FALSE),
	m_ReadType(NO_DELAYED),
	m_bLoaded(FALSE),
	m_pDBTMaster(NULL),
	m_pMasterRecord(NULL),
	m_bOnlyForRead(false)
{
}

//-----------------------------------------------------------------------------	
DBTSlave::DBTSlave
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument,
	const CString&		sName,
	BOOL				bAllowEmpty
)
	:
	DBTObject(pClass, pDocument, sName),
	m_bAllowEmpty(bAllowEmpty),
	m_bEmpty(TRUE),
	m_bOnlyDelete(FALSE),
	m_ReadType(NO_DELAYED),
	m_bLoaded(FALSE),
	m_pDBTMaster(NULL),
	m_pMasterRecord(NULL),
	m_bOnlyForRead(false)
{
}

//-----------------------------------------------------------------------------	
DBTSlave::DBTSlave
(
	const CString&		sTableName,
	CAbstractFormDoc*	pDocument,
	const CString&		sName,
	BOOL				bAllowEmpty
)
	:
	DBTObject(sTableName, pDocument, sName),
	m_bAllowEmpty(bAllowEmpty),
	m_bEmpty(TRUE),
	m_bOnlyDelete(FALSE),
	m_ReadType(NO_DELAYED),
	m_bLoaded(FALSE),
	m_pDBTMaster(NULL),
	m_pMasterRecord(NULL),
	m_bOnlyForRead(false)
{
}

//-----------------------------------------------------------------------------	
DBTSlave::DBTSlave
(
	SqlRecord*			pRecord,
	CAbstractFormDoc*	pDocument,
	const CString&		sName,
	BOOL				bAllowEmpty
)
	:
	DBTObject(pRecord, pDocument, sName),
	m_bAllowEmpty(bAllowEmpty),
	m_bEmpty(TRUE),
	m_bOnlyDelete(FALSE),
	m_ReadType(NO_DELAYED),
	m_bLoaded(FALSE),
	m_pDBTMaster(NULL),
	m_pMasterRecord(NULL),
	m_bOnlyForRead(false)
{
}

//-----------------------------------------------------------------------------	
BOOL DBTSlave::IsEmptyData()
{
	ASSERT(m_pRecord);
	return m_pRecord->IsEmpty();
}

// Funziona solo in BROWSE mode perche' negli altri casi viene comunque caricato
// anche se si e' specificato il DelayedRead
//-----------------------------------------------------------------------------	
BOOL DBTSlave::Reload(BOOL bIgnorePreloadStep /*= FALSE*/)
{
	if (
		m_bLoaded ||
		m_ReadType == NO_DELAYED ||
		!DelayedRead(m_pDocument->GetFormMode() != CBaseDocument::BROWSE) ||
		m_bOnlyDelete
		)
		return TRUE;

	// la find del master e' andata a buco (tabella vuota o documento 
	// non trovato) per cui inibisco il load dello slave che sicuramente
	// fallirebbe
	if (m_pDBTMaster && m_pDBTMaster->m_pTable->IsEmpty())
		return TRUE;

	int nOldPreloadStep = GetPreloadStep();
	if (bIgnorePreloadStep)
		SetPreloadStep(-1);	// forza la lettura di tutte le righe

	// in caso di edit devo ricaricare il buffer old
	BOOL bOk = LocalFindData(m_pDocument->GetFormMode() != CBaseDocument::BROWSE);

	if (bIgnorePreloadStep)
		SetPreloadStep(nOldPreloadStep);

	OnDisableControlsAlways();

	return bOk;

}

// bPrepareOld e' vero solo quando siamo in BROWSE mode			
//-----------------------------------------------------------------------------	
BOOL DBTSlave::DelayedRead(BOOL bPrepareOld)
{
	return
		(
			!bPrepareOld && // BROWSE mode
			(m_ReadType == BROWSE_DELAYED || m_ReadType == ALL_DELAYED)
			)
		||
		(
			m_pDocument->GetFormMode() == CBaseDocument::EDIT &&
			(m_ReadType == EDIT_DELAYED || m_ReadType == ALL_DELAYED)
			);
}

// bPrepareOld e' vero solo quando siamo in BROWSE mode			
//-----------------------------------------------------------------------------	
BOOL DBTSlave::FindData(BOOL bPrepareOld)
{
	// ripristina lo stato iniziale
	m_bEmpty = TRUE;

	// Bypassa la lettura in BROWSE mode per ottimizzare le presazioni.
	// E' compito nel programmatore leggere il dato nella OnPrepareAuxData 
	// della view o della TabDialog
	if (DelayedRead(bPrepareOld))
	{
		Init();
		m_bLoaded = FALSE;
		return TRUE;
	}

	return LocalFindData(bPrepareOld);
}

//-----------------------------------------------------------------------------	
BOOL DBTSlave::LocalFindData(BOOL bPrepareOld)
{
	m_bEmpty = !DBTObject::FindData(bPrepareOld);
	m_bLoaded = TRUE;

	// switch da eventuale stato di AddNew in stato di Edit
	if (m_bAllowEmpty && !m_bEmpty && !DBTObject::Edit())
		return FALSE;

	if (!m_bAllowEmpty && m_bEmpty)
		return ErrorMessage(_TB("Warning! A part of the document is empty."), NULL);

	return m_bAllowEmpty || !m_bEmpty;
}

// Nel caso degli slave normali e` possibile salvare un documento con lo slave
// vuoto. Quando si entra in correzione del documento occorre posizionare gli slave
// vuoti in condizione di AddNew onde permetterene l'eventuale l'aggiunta 
//-----------------------------------------------------------------------------	
BOOL DBTSlave::Edit()
{
	if (m_bAllowEmpty && m_bEmpty)
	{
		BOOL bOk = DBTObject::AddNew();
		Init();
		return bOk;
	}

	return DBTObject::Edit();
}

//-----------------------------------------------------------------------------	
BOOL DBTSlave::AddNewData()
{
	// Se si sta aggiungendo un nuovo insieme di DBT e il DBT slave e` vuoto e permette
	// di esserlo (relazione uno a uno con possibilita` di assenza della referenza)
	// allora evita di aggiungere i dati del DBT slave
	if (!IsEmptyData())
	{
		// Da al programmatore la possibilita` di preparare correttamente
		// la chiave primaria
		if (!m_pDocument->OnChangeDBTPrepareQuery(this, m_pTable))
			OnPrepareQuery();
		m_pDocument->DispatchOnModifyDBTPrepareQuery(this, m_pTable);

		return DBTObject::Update();
	}

	if (m_bAllowEmpty)
		return TRUE;

	// Non e` permesso avere uno slave vuoto
	return ErrorMessage(_TB("Warning! You cannot save the document if one part is empty."), NULL);
}

//-----------------------------------------------------------------------------	
BOOL DBTSlave::ModifyData()
{
	// Gestione del vuoto su data entry
	if (!IsEmptyData())
	{
		// Da al programmatore la possibilita` di preparare correttamente
		// i parametri di query per la chiave primaria
		if (!m_pDocument->OnChangeDBTPrepareQuery(this, m_pTable))
			OnPrepareQuery();
		m_pDocument->DispatchOnModifyDBTPrepareQuery(this, m_pTable);

		return DBTObject::Update();
	}

	// Non e` permesso avere uno slave vuoto
	if (!m_bAllowEmpty)
		return ErrorMessage(_TB("Warning! You cannot save the document if one part is empty."), NULL);

	// Record vuoto. Bisogna cancellare quello su file usando la chiave del
	// record caricato in ingresso e non il corrente che potrebbe avere la
	// chiave modificata da eventuale data entry abilitato sulla chiave primaria
	if (!m_pTable->NoCurrent())
		return Delete();

	// Non c'era su file e non e` stato inserito ed e` ammesso lo slave vuoto
	return TRUE;
}

// Il comportamento e` diversificato a seconda che sia vuoto o no lo slave
//-----------------------------------------------------------------------------	
BOOL DBTSlave::Update()
{
	if (m_bDBTOnView || !m_pTable->IsUpdatable()) return TRUE;

	m_bUpdated = FALSE;
	switch (m_pDocument->GetFormMode())
	{
	case CBaseDocument::NEW:
		if (AddNewData())
		{
			// se il DBT fosse "delayed", bisogna inibire eventuali "reload" dopo
			// la persistenza. In effetti e` come se fosse stata appena fatta la "reload"
			m_bLoaded = TRUE;
			return TRUE;
		}
		return FALSE;

	case CBaseDocument::EDIT:
		return m_bLoaded ? ModifyData() : TRUE;
	}

	ASSERT(FALSE);
	return FALSE;
}

// Cancella il DBT slave solo se era gia` stato ritrovato dalla FindData
//-----------------------------------------------------------------------------	
BOOL DBTSlave::Delete()
{
	if (m_bEmpty)
		return TRUE;

	return DBTObject::Delete();
}

//	NON Overridable :
//
//-----------------------------------------------------------------------------	
BOOL DBTSlave::CheckTransaction()
{
	if (m_bDBTOnView)	return TRUE;

	// Transazione non valida se lo slave e` vuoto e cio` non e` consentito
	if (!m_bAllowEmpty && IsEmptyData())
		return ErrorMessage(_TB("Warning! You cannot save the document if one part is empty."), NULL);

	// Non devo controllare coerenze di chiavi ed altro perche` non scrivo
	// il record su file
	if (IsEmptyData())
		return TRUE;

	// riprepara la chiave perche` possono essere stati modificati i dataobj
	// del master che fanno parte della chiave dello slave
	OnPreparePrimaryKey();
	GetClientDocs()->OnPreparePrimaryKey(this);

	// Prima controlla la chiave primaria
	m_strDBTError.Empty();
	BOOL bOk = OnCheckPrimaryKey();

	if (!bOk)
	{
		if (m_strDBTError.IsEmpty())
			m_strDBTError = _TB("Some extra reference data are not complete.");
		ConditionalDisplayMessage(m_strDBTError, MB_OK | MB_ICONSTOP);
	}

	// Da un canches al programmatore di controllare la chiave primaria
	return bOk  && OnOkTransaction();
}

//	NON Overridable :
//
//-----------------------------------------------------------------------------	
void DBTSlave::ForcePreparePrimaryKey()
{
	if (m_bDBTOnView || IsEmptyData())
		return;

	OnPreparePrimaryKey();
	GetClientDocs()->OnPreparePrimaryKey(this);
}

//-----------------------------------------------------------------------------	
void DBTSlave::OnDefineQuery()
{
	if (!m_pDBTMaster || !m_pTable || !m_pDBTMaster->GetRecord())
	{
		ASSERT(FALSE);
		TRACE1("DBTSlave::OnDefineQuery: missing DBTMaster record or m_pTable in DBT %s", GetNamespace().ToString());
		return;
	}

}

//-----------------------------------------------------------------------------	
void DBTSlave::OnPrepareQuery()
{
	if (!m_pDBTMaster || !m_pTable || !m_pDBTMaster->GetRecord())
	{
		ASSERT(FALSE);
		TRACE1("DBTSlave::OnPrepareQuery: missing DBTMaster record or m_pTable in DBT %s", GetNamespace().ToString());
		return;
	}
}

//-----------------------------------------------------------------------------	
//BOOL DBTSlave::Parse (CXMLNode* pNode, BOOL bWithAttributes/* = TRUE*/, BOOL bParseLocal/* = FALSE*/)
//{
//	ASSERT_VALID(m_pRecord);
//	return m_pRecord->Parse (pNode, bWithAttributes, bParseLocal);
//}

BOOL DBTSlave::UnParse(CXMLNode* pRootNode, BOOL bWithAttributes/* = TRUE*/, BOOL bUnParseLocal/* = FALSE*/, BOOL /*bUnParseSlavable = FALSE*/)
{
	ASSERT_VALID(m_pRecord);
	CXMLNode* pParentNode = pRootNode->CreateNewChild(GetName());

	CXMLNode* pNodeChild = pParentNode->CreateNewChild(m_pRecord->GetTableName());
	return m_pRecord->UnParse(pNodeChild, bWithAttributes, bUnParseLocal);
}


//-----------------------------------------------------------------------------	
void DBTSlave::GetJson(BOOL bWithChildren, CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound)
{
	jsonSerializer.OpenObject(GetName());
	SqlRecord *pRecord = GetRecord();
	if (pRecord)
	{
		pRecord->GetJson(jsonSerializer, bOnlyWebBound);
	}
	jsonSerializer.CloseObject();
}

//-----------------------------------------------------------------------------	
void DBTSlave::SetJson(BOOL bWithChildren, CJsonParser& jsonParser)
{
	if (jsonParser.BeginReadObject(GetName()))
	{
		SqlRecord *pRecord = GetRecord();
		if (pRecord)
			pRecord->SetJson(jsonParser);
		jsonParser.EndReadObject();
	}
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DBTSlave::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDBTSlave");

	DBTObject::Dump(dc);
}

void DBTSlave::AssertValid() const
{
	DBTObject::AssertValid();
}
#endif //_DEBUG


////////////////////////////////////////////////////////////////////////////////
//				class DBTSlaveBuffered definition
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNCREATE(DBTSlaveBuffered, DBTSlave)

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::DBTSlaveBuffered()
	:
	m_bCheckDuplicateKey(!CHECK_DUPLICATE_KEY),
	m_nCurrentRow(-1),
	m_nPreloadStep(15),
	m_bReadOnly(FALSE),
	m_bAlwaysReadOnly(FALSE),
	m_bModified(TRUE),
	m_pwNewRows(NULL),
	m_pwModifiedRows(NULL),
	m_pwDeletedRows(NULL),
	// Filtraggio
	m_bAllowFilter(FALSE),
	m_pAllRecords(NULL),
	m_pFreeList(NULL),
	m_pDefaultDuplicateKeyDataObj(NULL),
	m_bFindDataCalled(false),
	m_dwLatestModifyTime(0),
	m_pFnDuplicateKey(NULL)

{
	m_pRecords = new RecordArray();
	m_pOldRecords = new RecordArray();
}
//-----------------------------------------------------------------------------	
DBTSlaveBuffered::DBTSlaveBuffered
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument,
	const CString&		sName,
	BOOL				bAllowEmpty,
	BOOL				bCheckDuplicateKey
)
	:
	DBTSlave(pClass, pDocument, sName, bAllowEmpty),
	m_bCheckDuplicateKey(bCheckDuplicateKey),
	m_nCurrentRow(-1),
	m_nPreloadStep(15),
	m_bReadOnly(FALSE),
	m_bAlwaysReadOnly(FALSE),
	m_bModified(TRUE),
	m_pwNewRows(NULL),
	m_pwModifiedRows(NULL),
	m_pwDeletedRows(NULL),
	// Filtraggio
	m_bAllowFilter(FALSE),
	m_pAllRecords(NULL),
	m_pFreeList(NULL),
	m_pDefaultDuplicateKeyDataObj(NULL),
	m_bFindDataCalled(false),
	m_dwLatestModifyTime(0),
	m_pFnDuplicateKey(NULL)
{
	m_pRecords = new RecordArray();
	m_pOldRecords = new RecordArray();
}

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::DBTSlaveBuffered
(
	const CString&		sTableName,
	CAbstractFormDoc*	pDocument,
	const CString&		sName,
	BOOL				bAllowEmpty,
	BOOL				bCheckDuplicateKey
)
	:
	DBTSlave(sTableName, pDocument, sName, bAllowEmpty),

	m_bCheckDuplicateKey(bCheckDuplicateKey),
	m_nCurrentRow(-1),
	m_nPreloadStep(15),
	m_bReadOnly(FALSE),
	m_bAlwaysReadOnly(FALSE),
	m_bModified(TRUE),
	m_pwNewRows(NULL),
	m_pwModifiedRows(NULL),
	m_pwDeletedRows(NULL),
	m_bAllowFilter(FALSE),
	m_pAllRecords(NULL),
	m_pFreeList(NULL),
	m_pDefaultDuplicateKeyDataObj(NULL),
	m_bFindDataCalled(false),
	m_dwLatestModifyTime(0),
	m_pFnDuplicateKey(NULL)
{
	m_pRecords = new RecordArray();
	m_pOldRecords = new RecordArray();
}

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::DBTSlaveBuffered
(
	SqlRecord*			pRecord,
	CAbstractFormDoc*	pDocument,
	const CString&		sName,
	BOOL				bAllowEmpty,
	BOOL				bCheckDuplicateKey
)
	:
	DBTSlave(pRecord, pDocument, sName, bAllowEmpty),

	m_bCheckDuplicateKey(bCheckDuplicateKey),
	m_nCurrentRow(-1),
	m_nPreloadStep(15),
	m_bReadOnly(FALSE),
	m_bAlwaysReadOnly(FALSE),
	m_bModified(TRUE),
	m_pwNewRows(NULL),
	m_pwModifiedRows(NULL),
	m_pwDeletedRows(NULL),
	m_bAllowFilter(FALSE),
	m_pAllRecords(NULL),
	m_pFreeList(NULL),
	m_pDefaultDuplicateKeyDataObj(NULL),
	m_bFindDataCalled(false),
	m_dwLatestModifyTime(0),
	m_pFnDuplicateKey(NULL)
{
	m_pRecords = new RecordArray();
	m_pOldRecords = new RecordArray();
}
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::AlignBodyEdits(BOOL bRefreshBody)
{
	CBodyEditPointers* parBodies = GetBodyEdits();
	if (!parBodies)
		return;
	for (int i = 0; i < parBodies->GetCount(); i++)
	{
		CBodyEdit *pBody = parBodies->GetPointerAt(i);
		if (!pBody)
			continue;
		ASSERT_VALID(pBody);

		pBody->m_SelRecords.RemoveAll();
		pBody->ResizeSelections();

		if (bRefreshBody)
		{
			pBody->RefreshBody();

			if (m_pRecords->GetSize() > 1 && pBody->m_OrdSelectedColumn.GetSize())
				pBody->Sort();

			if (m_pRecords->GetSize())
				pBody->SetCurrLine(0);
		}
		else
		{
			pBody->m_nCurrRecordIdx = -1;
		}
	}
}

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::~DBTSlaveBuffered()
{
	ClearSlaveDBTs();

	CBodyEditPointers* parBodies = GetBodyEdits();
	if (parBodies)
	{
		for (int i = 0; i < parBodies->GetCount(); i++)
		{
			CBodyEdit* pEdit = parBodies->GetPointerAt(i);
			if (pEdit && pEdit->GetDBT() == this)
				pEdit->OnSwitchDBT(NULL);
		}
	}
	for (int x = m_DBTSlaveData.GetUpperBound(); x >= 0; x--)
	{
		//la distruzione della slavedata implica la distruzione del prototipo slave
		//la distruzione del prototipo slave implica l'accesso alla slave data del parent (cioè quella che sto ora distruggendo) per vedere se ci sono bodyedit agganciati
		//quindi devo prima chiamare la delete (che prima chiama i distruttori e poi cancella l'oggetto)
		//e poi rimuovere l'oggetto
		delete m_DBTSlaveData[x];
		m_DBTSlaveData.RemoveAt(x);
	}

	SAFE_DELETE(m_pRecords);
	SAFE_DELETE(m_pOldRecords);

	SAFE_DELETE(m_pwNewRows)
		SAFE_DELETE(m_pwModifiedRows)
		SAFE_DELETE(m_pwDeletedRows)

		SAFE_DELETE(m_pAllRecords);

	SAFE_DELETE(m_pFreeList);
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::IsActive()
{
	const DBTObject* pMaster = GetMaster();
	if (!pMaster || !pMaster->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		return TRUE;
	DBTSlaveBuffered* pParent = (DBTSlaveBuffered*)pMaster;
	if (!pParent->IsActive())
		return FALSE;
	DBTSlaveMap* pData = pParent->GetDBTSlaveData(GetNamespace().GetObjectName());
	return pData ? pData->m_pCurrentDBTSlave == this : FALSE;
}
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::ClearSlaveDBTs()
{
	for (int x = 0; x < m_DBTSlaveData.GetCount(); x++)
	{
		DBTSlaveMap* pData = m_DBTSlaveData[x];
		SetCurrentDBTSlave(pData, pData->m_pDBTSlavePrototype);
		POSITION pos = pData->m_Slaves.GetStartPosition();
		while (pos)
		{

			SqlRecord* pKey = NULL;
			DBTSlave* pValue = NULL;
			pData->m_Slaves.GetNextAssoc(pos, pKey, pValue);
			delete pValue;
		}
		pData->m_Slaves.RemoveAll();
	}
	for (int i = 0; i < m_DeletedSlaves.GetCount(); i++)
		delete m_DeletedSlaves[i];
	m_DeletedSlaves.RemoveAll();

}
//-----------------------------------------------------------------------------	
SqlRecord* DBTSlaveBuffered::CreateRecord()
{
	if (m_pFreeList)
	{
		SqlRecord* pRec = m_pFreeList->RemoveLast();
		if (pRec)
		{
			pRec->Init();
			return pRec;
		}
	}

	// Instanzia un altro record e lo inserisce
	SqlRecord* pRec = m_pRecord->Create();
	pRec->SetConnection(m_pRecord->GetConnection());
	return pRec;
}

//-----------------------------------------------------------------------------	
SqlRecord* DBTSlaveBuffered::InsertNewRecord(int nRow)
{
	ASSERT(m_pRecords);
	ASSERT(nRow <= m_pRecords->GetUpperBound());

	// Instanzia un altro record e lo inserisce
	SqlRecord* pRec = CreateRecord();

	// vera e propria aggiunta del record appena creato
	//
	// N.B.: dato che pRec deriva da CObArray e` necessario fare il cast
	// esplicito per chiamare la corretta InsertAt(int, CObject*, int)
	//
	//Filtraggio
	if (m_bAllowFilter && m_pRecords != m_pAllRecords)
	{
		ASSERT_VALID(m_pAllRecords);
		int nFullRow = RemapIndexF2A(nRow);
		m_pAllRecords->InsertAt(nFullRow, (CObject*)pRec, 1);
	}
	m_pRecords->InsertAt(nRow, (CObject*)pRec, 1);

	InternalSetModified();

	//aggancio gli eventi per EasyBuilder
	AttachDataEventsProxy(pRec);

	// assegna ad ogni colonna lo stato del record template m_pRecord
	pRec->CopyAttribute(m_pRecord);

	PrepareDynamicColumns(pRec, FALSE);
	OnPrepareAuxColumns(pRec);
	GetClientDocs()->OnPrepareAuxColumns(this, pRec);
	OnRecordAdded(pRec, nRow);

	return pRec;
}
///Siccome in easybuilder si agganciano gli eventi al dataobj del prototipo (l'unico
///disponibile in design mode) ogni volta che aggiungo una riga devo agganciare
///gli eventi del prototipo al dataobj di riga
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::AttachDataEventsProxy(SqlRecord* pAddedRec)
{
	int sz = min(pAddedRec->GetSizeEx(), m_pRecord->GetSizeEx());
	for (int nCol = 0; nCol < sz; nCol++)
	{
		DataObj* pD = pAddedRec->GetDataObjAt(nCol);
		DataObj* pS = m_pRecord->GetDataObjAt(nCol);

		pD->AttachEvents(new CDataEventsProxy(pS));
	}
}
//-----------------------------------------------------------------------------	
SqlRecord* DBTSlaveBuffered::AddNewRecord(BOOL bCopyCurrent)
{
	ASSERT(m_pRecords);

	// crea un nuovo record dello stesso tipo del corrente e se del caso copia anche
	// i dati
	SqlRecord* pRec = CreateRecord();

	//Filtraggio
	if (m_bAllowFilter && m_pRecords != m_pAllRecords)
	{
		ASSERT_VALID(m_pAllRecords);
		m_pAllRecords->Add(pRec);
	}
	int nRow = m_pRecords->Add(pRec);

	InternalSetModified();

	//aggancio gli eventi per EasyBuilder
	AttachDataEventsProxy(pRec);

	if (bCopyCurrent)
	{
		*pRec = *m_pRecord;
		pRec->SetStorable();
	}

	// assegna ad ogni colonna lo stato del record template m_pRecord
	pRec->CopyAttribute(m_pRecord);

	PrepareDynamicColumns(pRec, FALSE);
	OnPrepareAuxColumns(pRec);
	GetClientDocs()->OnPrepareAuxColumns(this, pRec);

	OnRecordAdded(pRec, nRow);

	return pRec;
}

//-----------------------------------------------------------------------------	
SqlRecord* DBTSlaveBuffered::AddOldRecord()
{
	ASSERT(m_pRecords);

	SqlRecord* pRec = m_pRecord->Create();

	*pRec = *m_pRecord;
	m_pOldRecords->Add(pRec);

	OnPrepareOldAuxColumns(pRec);
	GetClientDocs()->OnPrepareOldAuxColumns(this, pRec);
	return pRec;
}

// Viene chiamata dal BodyEdit quando si muove oltre l'ultima righe e pertanto
// e' giusto non settarla Storable per impedire l'inserimento di righe vuote a
// fine pagina
//-----------------------------------------------------------------------------	
//[TBWebMethod(thiscall_method=true)]
SqlRecord* DBTSlaveBuffered::AddRecord()
{
	ASSERT(m_pRecords);

	int nRow = m_pRecords->GetUpperBound() + 1;

	if
		(
			!OnBeforeAddRow(nRow)
			||
			!GetClientDocs()->OnBeforeAddRow(this, nRow)
			)
	{
		return NULL;
	}

	CApplicationContext::MacroRecorderStatus pStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;

	if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

	// crea un nuovo record dello stesso tipo del corrente, eventualmente copia anche i dati
	SqlRecord* pRec = AddNewRecord(FALSE);

	nRow = m_pRecords->GetUpperBound();

	// riprepara la chiave perche` possono essere stati modificati i dataobj
	// del master che fanno parte della chiave dello slave
	OnPreparePrimaryKey(nRow, pRec);
	GetClientDocs()->OnPreparePrimaryKey(this, nRow, pRec);

	// da la possibilita` al programmatore di inizializzare dataobj di riga
	// che non sono parte di chiave primaria (inizializzati sopra) 
	OnPrepareRow(nRow, pRec);
	GetClientDocs()->OnPrepareRow(this, nRow, pRec);
	if (!this->IsKindOf(RUNTIME_CLASS(DBTTree)))
	{
		//per il DBTTree l'evento è posticipato al completamento dell'azione
		//in DBTTree::Insert ed InsertChild

		// Da la possibilita' al programmatore di sapere che e' stato aggiunto un 
		// record. Il programmatore decide cosa fare reimplementando opportunamente.
		// Di default non viene fatto nulla
		OnAfterAddRow(nRow, pRec);
		GetClientDocs()->OnAfterAddRow(this, nRow, pRec);
	}

	if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

	//Il dbt cambia, quindi anche il documento cambia: nel caso di bodyedit c++ era il body edit a impostare il documento modificato
	//nel caso di griglia c# invece abbiamo deciso che deve essere il dbt a modificarlo
	if (m_pDocument)
	{
		m_pDocument->SetModifiedFlag();
	}

	return pRec;
}

// Chiamata dal CBodyEdit per inserire una nuova riga
// Warning!! se si inserisce un record dall'esterno il record diventa proprieta`
// del DBT che lo deve anche cancellare.
//
// Come per la AddRecord non setta Storable al riga inserita (eventualmente in
// caso di default imposti dal programmatore e' lo stesso a dover settare Storable
// la riga onde permetterne il salvataggio)
//-----------------------------------------------------------------------------	
SqlRecord* DBTSlaveBuffered::InsertRecord(int nRow)
{
	// Su inserimento oltre l'ultima riga si comporta come in Add
	if (nRow > m_pRecords->GetUpperBound())
	{
		return AddRecord();
	}

	if
		(
			!OnBeforeInsertRow(nRow)
			||
			!GetClientDocs()->OnBeforeInsertRow(this, nRow)
			)
	{
		return NULL;
	}

	CApplicationContext::MacroRecorderStatus pStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;

	if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

	// Instanzia un altro record e lo inserisce
	SqlRecord* pRec = InsertNewRecord(nRow);

	// se intervengo "sopra" o sulla current row, ne aggiorno il valore
	if (nRow <= m_nCurrentRow)
		SetCurrentRow(m_nCurrentRow + 1);

	// riprepara la chiave perche` possono essere stati modificati i dataobj
	// del master che fanno parte della chiave dello slave
	OnPreparePrimaryKey(nRow, pRec);
	GetClientDocs()->OnPreparePrimaryKey(this, nRow, pRec);

	// da la possibilita` al programmatore di inizializzare dataobj di riga
	// che non sono parte di chiave primaria (inizializzati sopra) 
	OnPrepareRow(nRow, pRec);
	GetClientDocs()->OnPrepareRow(this, nRow, pRec);
	if (!this->IsKindOf(RUNTIME_CLASS(DBTTree)))
	{
		//per il DBTTree l'evento è posticipato al completamento dell'azione
		//in DBTTree::Insert ed InsertChild

		// Da la possibilita' al programmatore di sapere che e' stato aggiunto un 
		// record. Il programmatore decide cosa fare reimplementando opportunamente.
		// Di default non viene fatto nulla
		OnAfterInsertRow(nRow, pRec);
		GetClientDocs()->OnAfterInsertRow(this, nRow, pRec);
	}

	if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

	//Il dbt cambia, quindi anche il documento cambia: nel caso di bodyedit c++ era il body edit a impostare il documento modificato
	//nel caso di griglia c# invece abbiamo deciso che deve essere il dbt a modificarlo
	if (m_pDocument)
	{
		m_pDocument->SetModifiedFlag();
	}

	return pRec;
}

// called by CBodyEdit when overflow row occurred
// called by CBodyEdit when deleting a row
//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::DeleteRecord(int nRow)
{
	ASSERT(nRow <= m_pRecords->GetUpperBound());

	ASSERT_VALID(m_pRecords);

	// Da la possibilita' al programmatore di sapere che si sta' per eliminare un 
	// record. Il programmatore decide cosa fare reimplementando opportunamente.
	// Di default non viene fatto nulla
	if
		(
			!OnBeforeDeleteRow(nRow)
			||
			!GetClientDocs()->OnBeforeDeleteRow(this, nRow)
			)
	{
		return FALSE;
	}

	CApplicationContext::MacroRecorderStatus pStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;

	if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

	int nFullRow = nRow;
	if (m_bAllowFilter && m_pAllRecords != m_pRecords)
	{
		nFullRow = RemapIndexF2A(nRow);
	}
	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap* pData = m_DBTSlaveData[i];
		DBTSlave *pSubSlave = GetDBTSlave(nFullRow, pData);
		if (pSubSlave)
		{
			if (pSubSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
			{
				DBTSlaveBuffered* pSubBuffered = (DBTSlaveBuffered*)pSubSlave;
				for (int i = pSubBuffered->GetUpperBound(); i >= 0; i--)
					pSubBuffered->DeleteRecord(i);
			}
			VERIFY(pData->m_Slaves.RemoveKey(pSubSlave->m_pMasterRecord));
			pSubSlave->m_pMasterRecord = NULL;
			m_DeletedSlaves.Add(pSubSlave);
		}
	}
	if (m_bAllowFilter && m_pAllRecords != m_pRecords)
	{
		m_pAllRecords->RemoveAt(nFullRow);
	}
	m_pRecords->RemoveAt(nRow);

	InternalSetModified();

	// se intervengo "sopra" o sulla current row, ne aggiorno il valore
	if (nRow <= m_nCurrentRow)
	{
		SetCurrentRow(min(max(0, m_nCurrentRow - 1), m_pRecords->GetUpperBound()));
	}

	// da la possibilita' al programmatore di fare della post elaborazione
	// ad esempio ricalcoli di code fattura
	OnAfterDeleteRow(nRow);
	GetClientDocs()->OnAfterDeleteRow(this, nRow);

	if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

	//Il dbt cambia, quindi anche il documento cambia: nel caso di bodyedit c++ era il body edit a impostare il documento modificato
	//nel caso di griglia c# invece abbiamo deciso che deve essere il dbt a modificarlo
	if (m_pDocument)
	{
		m_pDocument->SetModifiedFlag();
	}

	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::DeleteUnfilteredRecord(int nRow)
{
	if (!m_pAllRecords)
		return DeleteRecord(nRow);

	ASSERT_VALID(m_pAllRecords);
	ASSERT(nRow <= m_pAllRecords->GetUpperBound());

	SqlRecord* pRecord = GetUnfilteredRow(nRow);
	ASSERT_VALID(pRecord);

	int pos = -1;
	if ((pos = m_pRecords->FindPtr(pRecord)) >= 0)
		return DeleteRecord(pos);

	// Di default non viene fatto nulla
	if
		(
			!OnBeforeDeleteRow(nRow)
			||
			!GetClientDocs()->OnBeforeDeleteRow(this, nRow)
			)
	{
		return FALSE;
	}

	CApplicationContext::MacroRecorderStatus pStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;

	if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap* pData = m_DBTSlaveData[i];
		DBTSlave *pSubSlave = GetDBTSlave(pRecord, pData);

		if (pSubSlave)
		{
			if (pSubSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
			{
				DBTSlaveBuffered* pSubBuffered = (DBTSlaveBuffered*)pSubSlave;
				for (int i = pSubBuffered->GetUpperBound(); i >= 0; i--)
					pSubBuffered->DeleteRecord(i);
			}
			VERIFY(pData->m_Slaves.RemoveKey(pSubSlave->m_pMasterRecord));
			pSubSlave->m_pMasterRecord = NULL;
			m_DeletedSlaves.Add(pSubSlave);
		}
	}

	m_pAllRecords->RemoveAt(nRow);

	InternalSetModified();

	// da la possibilita' al programmatore di fare della post elaborazione
	// ad esempio ricalcoli di code fattura
	OnAfterDeleteRow(nRow);
	GetClientDocs()->OnAfterDeleteRow(this, nRow);

	if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

	//Il dbt cambia, quindi anche il documento cambia: nel caso di bodyedit c++ era il body edit a impostare il documento modificato
	//nel caso di griglia c# invece abbiamo deciso che deve essere il dbt a modificarlo
	if (m_pDocument)
	{
		m_pDocument->SetModifiedFlag();
	}

	return TRUE;
}
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::RemoveAllRecords()
{
	if (!m_pRecords)
		return;

	for (int i = m_pRecords->GetUpperBound(); i >= 0; i--)
	{
		m_pRecords->RemoveAt(i);
	}
	//TODO da verificare se si può mettere qui; 
	//m_nCurrentRow = -1;
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::RemoveAll(BOOL bRemoveOld /*= FALSE*/)
{
	CApplicationContext::MacroRecorderStatus pStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;

	if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

	ClearSlaveDBTs();
	RemoveAllRecords();
	if (bRemoveOld)
		m_pOldRecords->RemoveAll();

	InternalSetModified();

	SetCurrentRow(-1);

	//Filtraggio
	if (m_bAllowFilter && m_pAllRecords) m_pAllRecords->RemoveAll();

	if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::IsDuplicateKey(SqlRecord* pRec1, SqlRecord* pRec2)
{
	const CUIntArray& aIndexes = pRec1->GetPrimaryKeyIndexes();
	for (int i = 0; i <= aIndexes.GetUpperBound(); i++)
	{
		UINT index = aIndexes[i];
		if (!pRec1->GetDataObjAt(index)->IsEqual(*pRec2->GetDataObjAt(index)))
			return FALSE;
	}

	if (m_pFnDuplicateKey)
		m_pDefaultDuplicateKeyDataObj = m_pFnDuplicateKey(pRec1, -1);

	return TRUE;
}


//-----------------------------------------------------------------------------	
int DBTSlaveBuffered::FindRecordIndex(SqlRecord* pRec)
{
	for (int i = 0; i <= m_pRecords->GetUpperBound(); i++)
	{
		SqlRecord* pCurrentRec = m_pRecords->GetAt(i);

		if (IsDuplicateKey(pRec, pCurrentRec))
		{
			return i;
		}
	}

	return -1;
}

// Il parametro di bCheckAll abilita in controllo di tutte le righe oppure
// ottimizza il controllo partendo sempre dalla corrente posizione a salire.
// Nel caso di chiamata da body la messaggistica non deve essere fatta localmente 
// ma demandata al body che la fara' al momento per lui opportuno
//-----------------------------------------------------------------------------	
DataObj* DBTSlaveBuffered::CheckRow(int nRow, BOOL bCheckAll/*= TRUE*/, BOOL bFromBody/*= TRUE*/)
{
	DataObj*	pBadData = NULL;	 // mean right data object
	SqlRecord*	pRec = GetRow(nRow);
	ASSERT_VALID(pRec);

	// se il record non e' buono non deve essere controllato
	if (!pRec || !pRec->IsStorable())
		return NULL;

	// se  provengo dal body edit rieseguo l'assegnazione delle chiavi
	// altrimenti questa è già stata fatta nel loop nella checkrecords
	if (bFromBody)
	{
		OnPreparePrimaryKey(nRow, pRec);
		GetClientDocs()->OnPreparePrimaryKey(this, nRow, pRec);
	}

	// Prima controlla la chiave primaria
	m_strDBTError.Empty();
	if ((pBadData = OnCheckPrimaryKey(nRow, pRec)) != NULL)
	{
		if (m_strDBTError.IsEmpty())
			m_strDBTError = _TB("Some extra reference data are not complete.");

		if (!bFromBody)
			ConditionalDisplayMessage(m_strDBTError, MB_OK | MB_ICONSTOP);

		return pBadData;
	}

	// check user defined rules
	m_strDBTError.Empty();
	if (
		(pBadData = OnCheckUserData(nRow)) != NULL ||
		(pBadData = GetClientDocs()->OnCheckUserData(this, nRow)) != NULL
		)
	{
		if (m_strDBTError.IsEmpty())
			m_strDBTError = _TB("Some extra data are not complete.");

		if (!bFromBody)
			ConditionalDisplayMessage(m_strDBTError, MB_OK | MB_ICONSTOP);

		return pBadData;
	}

	// search for duplicate key in all records
	if (m_bCheckDuplicateKey)
	{
		m_strDBTError.Empty();
		m_pDefaultDuplicateKeyDataObj = NULL;

		for (int i = (bCheckAll ? GetUpperBound() : nRow - 1); i >= 0; i--)
		{
			// non controlla se stessa oppure quelle non memorizzabili (non validate in qualche modo)
			if (i == nRow || !GetRow(i)->IsStorable())
				continue;

			// abilita la composizione del messaggio
			if (IsDuplicateKey(pRec, GetRow(i)) || UserIsDuplicateKey(pRec, GetRow(i)))
			{
				m_strDBTError = GetDuplicateKeyMsg(pRec);
				if (m_strDBTError.IsEmpty())
					m_strDBTError = _TB("The document contains duplicated reference data.");
				if (!bFromBody)
					ConditionalDisplayMessage(m_strDBTError, MB_OK | MB_ICONSTOP);
				else
					m_bDBTErrorPending = TRUE;

				// positionate BodyEdit on first dataobj in recordobj
				return GetDuplicateKeyPos(pRec);
			}
		}
	}

	return NULL;
}

// Serve ad indicare in quale dataObj occorre posizionare il fuoco (nella colonna
// del BodyEdit che controlla il DBT) in caso la chiave sia duplicata
//-----------------------------------------------------------------------------	
DataObj* DBTSlaveBuffered::GetDuplicateKeyPos(SqlRecord* pRec)
{
	DataObj* pDataObj = m_pDefaultDuplicateKeyDataObj ? m_pDefaultDuplicateKeyDataObj : pRec->GetDataObjAt(0);
	if (pDataObj)
		return pDataObj;

	return NULL;
}

//-----------------------------------------------------------------------------	
CString DBTSlaveBuffered::GetDuplicateKeyMsg(SqlRecord* pRec)
{
	CString pStrError = m_strDBTError.IsEmpty() ? _TB("The document contains duplicated reference data.") : m_strDBTError;
	if (!pStrError.IsEmpty())
		return pStrError;

	return _T("");
}

// Check all records for document coherence (unique keys, etc...)
// (also call CheckRow). User can add other body rules for validate them
//-----------------------------------------------------------------------------	
DataObj* DBTSlaveBuffered::CheckRecords(int& nRow, BOOL bFromBody/* = TRUE*/)
{
	DataObj* pDataObj = NULL;

	// se non provengo dal body edit rieseguo l'assegnazione delle chiavi
	// riprepara la chiave perche` possono essere stati modificati i dataobj
	// del master che fanno parte della chiave dello slave		
	if (!bFromBody)
		for (int i = 0, nRiga = 0; i <= GetUpperBound(); i++)
			if (GetRow(i)->IsStorable())
			{
				OnPreparePrimaryKey(nRiga, GetRow(i));
				GetClientDocs()->OnPreparePrimaryKey(this, nRiga, GetRow(i));
				nRiga++;
			}

	for (int i = GetUpperBound(); i >= 0; i--)
	{
		// se la riga non e' storable non devo fare alcun controllo
		if ((pDataObj = CheckRow(i, FALSE, bFromBody)) != NULL)
		{
			nRow = i;
			return pDataObj;
		}
	}

	// give a chance to user for other tests
	m_strDBTError.Empty();
	if (
		(pDataObj = OnCheckUserRecords(nRow)) != NULL ||
		(pDataObj = GetClientDocs()->OnCheckUserRecords(this, nRow)) != NULL
		)
	{
		if (m_strDBTError.IsEmpty())
			m_strDBTError = _TB("The document contains inconsistent data.");
		if (!bFromBody)
			ConditionalDisplayMessage(m_strDBTError, MB_OK | MB_ICONSTOP);

		return pDataObj;
	}

	return NULL;
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::CheckSlaveTransaction()
{
	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap *pData = m_DBTSlaveData[i];
		POSITION pos = pData->m_Slaves.GetStartPosition();
		while (pos)
		{
			SqlRecord* pKey = NULL;
			DBTSlave* pValue = NULL;
			pData->m_Slaves.GetNextAssoc(pos, pKey, pValue);
			if (pValue->m_pMasterRecord && pValue->m_pMasterRecord->IsStorable() && !pValue->CheckTransaction())
				return FALSE;
		}
	}
	return TRUE;
}
//	NON Overridable :
//
//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::CheckTransaction()
{
	if (m_bDBTOnView) return TRUE;

	int nRow = -1;
	if ((CheckRecords(nRow, FALSE) != NULL))
		return FALSE;
	if (!CheckSlaveTransaction())
		return FALSE;

	if (m_bAllowEmpty)
		return OnOkTransaction();

	// Non e' ammesso il body vuoto e trovo almeno una riga
	for (int i = 0; i <= GetUpperBound(); i++)
		if (GetRow(i)->IsStorable())
			return OnOkTransaction();

	// Body vuoto				
	return ErrorMessage(_TB("Warning! A part of the document is empty."), NULL);
}

//	NON Overridable :
//
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::ForcePreparePrimaryKey()
{
	if (m_bDBTOnView)
		return;

	for (int i = 0, nRiga = 0; i <= GetUpperBound(); i++)
		if (GetRow(i)->IsStorable())
		{
			OnPreparePrimaryKey(nRiga, GetRow(i));
			GetClientDocs()->OnPreparePrimaryKey(this, nRiga, GetRow(i));
			nRiga++;
		}
}

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::CompareStatus DBTSlaveBuffered::CompareRow(int nRow, int& nFoundRow)
{
	// Try first on same row (very probable) (best performance)
	if (nRow <= GetOldUpperBound() && IsDuplicateKey(GetOldRow(nRow), GetRow(nRow)))
	{
		// allow update
		GetOldRow(nRow)->SetModified();
		nFoundRow = nRow;

		// leave unchanged
		if (GetOldRow(nRow)->IsPhisicalEqual(*GetRow(nRow)))
			return DBTSlaveBuffered::EQUAL;

		// rewrite record		
		return DBTSlaveBuffered::MODIFIED;
	}

	// insert dicotomic search if needed
	for (nFoundRow = 0; nFoundRow <= GetOldUpperBound(); nFoundRow++)
	{
		// nRow position already tested first
		if (nFoundRow == nRow)
			continue;

		if (IsDuplicateKey(GetOldRow(nFoundRow), GetRow(nRow)))
		{
			GetOldRow(nFoundRow)->SetModified();

			if (GetOldRow(nFoundRow)->IsPhisicalEqual(*GetRow(nRow)))
				return DBTSlaveBuffered::EQUAL;

			return DBTSlaveBuffered::MODIFIED;
		}
	}

	nFoundRow = -1;
	return DBTSlaveBuffered::NEW_ROW;
}

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::CompareStatus DBTSlaveBuffered::CompareWithOldRow(int nRow, int& nFoundRow)
{
	// insert dicotomic search if needed
	for (nFoundRow = 0; nFoundRow <= GetOldUpperBound(); nFoundRow++)
	{
		if (IsDuplicateKey(GetOldRow(nFoundRow), GetRow(nRow)))
		{
			if (GetOldRow(nFoundRow)->IsPhisicalEqual(*GetRow(nRow)))
				return DBTSlaveBuffered::EQUAL;

			return DBTSlaveBuffered::MODIFIED;
		}
	}

	nFoundRow = -1;
	return DBTSlaveBuffered::NEW_ROW;
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::Init()
{
	DBTSlave::Init();

	// clear buffers and also remove pointed data
	ClearSlaveDBTs();

	RemoveAllRecords();
	m_pOldRecords->RemoveAll();

	InternalSetModified();

	SetCurrentRow(-1);

	//Filtraggio
	if (m_bAllowFilter && m_pAllRecords) m_pAllRecords->RemoveAll();
}
//-----------------------------------------------------------------------------
void DBTSlaveBuffered::PrepareDynamicColumns(BOOL bUpdateDescriptions)
{
	if (m_arHKLInfos.GetCount())
	{
		for (int j = 0; j < GetSize(); j++)
		{
			if (j == m_nCurrentRow)
				continue;
			SqlRecord* pRec = GetRow(j);
			PrepareDynamicColumns(pRec, bUpdateDescriptions);
		}
		//la riga corrente la faccio pe ultima, così un eventuale hotlink composito (unico per tutte le righe) mi rimane
		//posizionato correttamente
		if (m_nCurrentRow != -1)
		{
			SqlRecord* pRec = GetRow(m_nCurrentRow);
			PrepareDynamicColumns(pRec, bUpdateDescriptions);
		}
	}
}
//-----------------------------------------------------------------------------
void DBTSlaveBuffered::PrepareDynamicColumns(SqlRecord *pRec, BOOL bUpdateDescriptions)
{
	OnPrepareDynamicColumns(pRec);
	CMap<void*, void*, bool, bool> hotLinkFindCalled;
	for (int i = 0; i < m_arHKLInfos.GetCount(); i++)
	{
		hotLinkFindCalled[m_arHKLInfos[i]->m_pHKL] = false;
	}
	for (int i = 0; i < m_arHKLInfos.GetCount(); i++)
	{
		HKLInfo* pInfo = m_arHKLInfos[i];
		if (!pInfo->m_DescriptionField.IsEmpty())
		{
			DataObj* pKey = GetDataObjFromName(pRec, pInfo->m_strKeyField);
			if (pKey)
			{
				if (bUpdateDescriptions && pInfo->m_pHKL->FindNeeded(pKey, pRec))
				{
					pInfo->m_pHKL->OnPrepareForFind(pRec);
					pInfo->m_pHKL->FindRecord(pKey);
					hotLinkFindCalled[pInfo->m_pHKL] = true;
				}
				else
				{
					//se sono in fase di changing, l'hotlink è stato già findato, ma devo
					//cmq travasare il valore dei campi
					if (pKey == &AfxGetBaseApp()->GetChangingCtrlData())
						hotLinkFindCalled[pInfo->m_pHKL] = bUpdateDescriptions == TRUE;
				}

				CString sFieldName = pInfo->m_DescriptionField;
				CString sDescri = pInfo->m_pHKL->GetName() + _T("_") + sFieldName;
				DataObj* pSource = GetDataObjFromName(pInfo->m_pHKL->GetAttachedRecord(), sFieldName);
				if (!pSource)
				{
					ASSERT(FALSE);
					continue;
				}
				bool bDescriptionFieldCreated = false;
				DataObj* pTarget = GetDataObjFromName(pRec, sDescri);
				if (!pTarget)
				{
					pTarget = pSource->Clone();
					pTarget->Clear();
					pRec->BindDynamicDataObj(sDescri, *pTarget, pSource->GetColumnLen());
					bDescriptionFieldCreated = bUpdateDescriptions == TRUE;
				}

				if (bDescriptionFieldCreated || hotLinkFindCalled[pInfo->m_pHKL])
					pTarget->Assign(*pSource);

			}
		}
	}
}
//-----------------------------------------------------------------------------
void DBTSlaveBuffered::AddHotLinkKeyField(HotKeyLink* pHKL, const CString& sKeyField)
{
	for (int i = 0; i < m_arHKLInfos.GetCount(); i++)
	{
		HKLInfo* pInfo = m_arHKLInfos[i];
		if (pInfo->m_pHKL == pHKL)
		{
			if (pInfo->m_strKeyField.IsEmpty())
				pInfo->m_strKeyField = sKeyField;
			else
				ASSERT(pInfo->m_strKeyField == sKeyField);
		}
	}

	HKLInfo* pInfo = new HKLInfo;
	pInfo->m_pHKL = pHKL;
	pInfo->m_strKeyField = sKeyField;
	m_arHKLInfos.Add(pInfo);
}
//-----------------------------------------------------------------------------
void DBTSlaveBuffered::AddHotLinkDescriptionField(HotKeyLink* pHKL, const CString& sDescriField)
{
	CString sKeyField;
	for (int i = 0; i < m_arHKLInfos.GetCount(); i++)
	{
		HKLInfo* pInfo = m_arHKLInfos[i];
		if (pInfo->m_pHKL == pHKL)
		{
			if (pInfo->m_DescriptionField == sDescriField)
			{
				return;
			}
			if (pInfo->m_DescriptionField.IsEmpty())
			{
				pInfo->m_DescriptionField = sDescriField;
				return;
			}
			if (sKeyField.IsEmpty())
				sKeyField = pInfo->m_strKeyField;
			else
				ASSERT(sKeyField == pInfo->m_strKeyField);
		}
	}

	HKLInfo* pInfo = new HKLInfo;
	pInfo->m_pHKL = pHKL;
	pInfo->m_strKeyField = sKeyField;
	pInfo->m_DescriptionField = sDescriField;
	m_arHKLInfos.Add(pInfo);
}

//-----------------------------------------------------------------------------
DataObj* DBTSlaveBuffered::GetBindingData(const CString& strParentDataSource, const CString& strDataSource, CString& sFieldName, CString& sBindingName, bool &fromHKL)
{
	DataObj* pDataObj = NULL;
	int startField = strDataSource.Find(_T('.'));
	CString sOwnerName;
	if (startField > -1)
	{
		sOwnerName = strDataSource.Mid(0, startField);
		GetDocument()->TranslateDataSourceAlias(sOwnerName);
		sFieldName = strDataSource.Mid(startField + 1);
	}
	else
	{
		sFieldName = strDataSource;
		if (strParentDataSource.IsEmpty())
		{
			sOwnerName = GetName();
		}
		else
		{
			sOwnerName = strParentDataSource;
			GetDocument()->TranslateDataSourceAlias(sOwnerName);
		}
	}
	GetDocument()->TranslateFieldAlias(sOwnerName, sFieldName);

	if (sOwnerName.CompareNoCase(GetName()) == 0)
	{
		pDataObj = GetDataObjFromName(GetRecord(), sFieldName);
		sBindingName = GetRecord()->GetTableName() + _T("_") + sFieldName;

		fromHKL = false;
	}
	else //caso del campo di decodifica di hotlink
	{
		HotKeyLink* pHotLink = GetDocument()->GetHotLink(sOwnerName);
		ASSERT(pHotLink);
		if (pHotLink)
		{
			CString sDescri = sOwnerName + _T("_") + sFieldName;
			//prima controllo se c'è, magari creato da una precedente istanziazione di body edit
			pDataObj = GetRecord()->GetDataObjFromColumnName(sDescri);
			if (!pDataObj)
			{
				DataObj* pHKLObj = GetDataObjFromName(pHotLink->GetAttachedRecord(), sFieldName);
				if (pHKLObj)
				{
					//creo dinamicamente un campo local per ospitare la decodifica dell'hotlink
					pDataObj = pHKLObj->Clone();
					GetRecord()->BindDynamicDataObj(sDescri, *pDataObj, pHKLObj->GetColumnLen());
				}

			}
			sBindingName = pHotLink->GetAttachedRecord()->GetTableName() + _T("_") + sFieldName;
			if (pDataObj)
				AddHotLinkDescriptionField(pHotLink, sFieldName);

			fromHKL = true;
		}
	}
	return pDataObj;
}
//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::MoreRecords()	const
{
	return !m_pTable->IsEOF();
}

// Permette di continuare il caricamento dei succesivi n-record non caricati
// dalla find data. Valido solo in  browse mode per sincronizzarsi con lo 
// scroll  del BodyEdit.
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::LoadNextRecords()
{
	if (m_pDocument->GetFormMode() != CBaseDocument::BROWSE)
		return;

	if (!m_pTable)
		return;

	TRY
	{
		int nRecFound = 0;
		while (
			!m_pTable->IsEOF() &&
			(m_nPreloadStep < 0 || nRecFound < m_nPreloadStep)
			)
		{
			AddNewRecord(TRUE);
			nRecFound++;
			m_pTable->MoveNext();
		}
	}
		CATCH(CException, e)
	{
		TCHAR strErr[512];
		e->GetErrorMessage(strErr, 500);
		ErrorMessage
		(
			cwsprintf(
				_TB("DBTSlaveBuffered {0-%s} LoadNextRecords : exception of kind {1-%s}\n error message {2-%s}"),
				(LPCTSTR)CString(GetRuntimeClass()->m_lpszClassName),
				(LPCTSTR)CString(e->GetRuntimeClass()->m_lpszClassName),
				strErr
			),
			NULL
		);
	}
	END_CATCH
}

//---------------------------------------------------------------------------------------------------------------------
void DBTSlaveBuffered::SetDataOSLReadOnly(BOOL bReadOnly, int idx/* = -1*/, BOOL onlyLoaded /*= TRUE*/, int startRowIndex/* = 0*/)
{
	for (int i = startRowIndex; i < GetSize(); i++)
	{
		if (onlyLoaded && IsNewRow(i))
			continue;

		SqlRecord* pRec = m_pRecords->GetAt(i);

		pRec->SetDataOSLReadOnly(bReadOnly, idx);
	}
}

// Cerca di riempire un corpo se trova qualche cosa e segnala errore se trova
// qualcosa e siamo in inserimento. Il controllo e la gestione del corpo vuoto
// e` lasciata alla implementazione della classe padre.
//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::FindData(BOOL bPrepareOld)
{
	m_bFindDataCalled = true;

	ClearSlaveDBTs();//cancello eventuali DBT slave

	//Filtraggio
	RecordArray* pTemp = m_pRecords;
	int nOldPreloadStep = GetPreloadStep();
	if (m_bAllowFilter)
	{
		m_pRecords = m_pAllRecords;
		RemoveAllRecords();

		SetPreloadStep(-1);	// forza la lettura di tutte le righe
	}

	// ripristina lo stato iniziale
	m_bEmpty = TRUE;
	// Bypassa la lettura in BROWSE mode per ottimizzare le presazioni.
	// E' compito nel programmatore leggere il dato nella OnPrepareAuxData 
	// della view o della TabDialog.
	BOOL bOk = TRUE;
	if (DelayedRead(bPrepareOld))
	{
		Init();
		m_bLoaded = FALSE;
		bOk = TRUE;
	}
	else
		bOk = LocalFindData(bPrepareOld);

	//Filtraggio
	if (m_bAllowFilter)
	{
		SetPreloadStep(nOldPreloadStep);

		m_pRecords = pTemp;
		m_pRecords->Append(*m_pAllRecords);
	}

	OnFindData();

	return bOk;
}

// Cerca di riempire un corpo se trova qualche cosa e segnala errore se trova
// qualcosa e siamo in inserimento. Il controllo e la gestione del corpo vuoto
// e` lasciata alla implementazione della classe padre.
//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::LocalFindData(BOOL bPrepareOld)
{
	// Se non lo trova lo inizializza  ai valori di default
	if (!DBTSlave::LocalFindData(bPrepareOld))
	{
		Init();
		return FALSE;
	}

	// cancello il contenuto degli array
	if (m_pwNewRows)
		m_pwNewRows->RemoveAll();

	if (m_pwModifiedRows)
		m_pwModifiedRows->RemoveAll();

	if (m_pwDeletedRows)
		m_pwDeletedRows->RemoveAll();


	// Carica solo i primi 15 records e non bufferizza i dati precedenti
	// quando si sta' scorrendo in browse per minimizzare i tempi
	int nRecFound = 0;
	TRY
	{
		m_bLoaded = FALSE;
		while (
			!m_pTable->IsEOF() &&
			(
			bPrepareOld ||
			m_nPreloadStep < 0 ||
			nRecFound < m_nPreloadStep
			)
			)
		{
			AddNewRecord(TRUE);

			// carica i vecchi dati in caso di edit
			if (bPrepareOld)
				AddOldRecord();

			nRecFound++;
			m_pTable->MoveNext();
		}
		m_bLoaded = TRUE;

		// Sicronizzo la posizione per il BodyEdit
		if (IsEmpty())							SetCurrentRow(-1);
		else	if (m_nCurrentRow > GetUpperBound())	SetCurrentRow(0);
		else	if (m_nCurrentRow == -1)				SetCurrentRow(0);
	}
		CATCH(SqlException, e)
	{
		TCHAR strErr[512];
		e->GetErrorMessage(strErr, 500);

		return ErrorMessage
		(
			cwsprintf(
				_TB("DBTSlaveBuffered {0-%s} LocalFindData : query failed, body not found.\n {1-%s}"),
				(LPCTSTR)CString(GetRuntimeClass()->m_lpszClassName),
				strErr
			),
			NULL);
	}
	END_CATCH

		/*
			// Errore. Esiste un corpo ma non una testa (no Master):
			// l'unica possibilita` che cio` avvenga e` che` sia richiamata esplicitamente
			// la LocalFindData () per caricare dei record di uno SlaveBuffered, gestito da un
			// Master di un altro data entry, e si vogliano solamente visualizzare anche in stato
			// di NUOVO documento (per esempio nel momento in cui l'utente a composto sulla
			// form una chiave coerente). Il controllo e` quindi stato rilassato per accettare
			// la condizione in cui questo DBTSlaveBuffered sia in stato di ReadOnly.
			//
			if (nRecFound && m_pDocument->GetFormMode() == CBaseDocument::NEW && !IsReadOnly())
			return ErrorMessage(_TB("The document does not contain the main reference data."), NULL);
			*/
		return m_bLoaded;
}

// a differenza della AddNew del DBTObject non chiamo la AddNew del SqlTable.
// questo viene fatto dopo nella AddNewRow 
//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::AddNew(BOOL /*bInit = TRUE*/)
{
	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::Edit()
{
	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::AddNewRow(int nRow)
{
	ASSERT(m_pTable);
	ASSERT(nRow <= GetUpperBound());

	CApplicationContext::MacroRecorderStatus pStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;

	TRY
	{
		if (!m_bRecordable)
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

		m_pTable->AddNew();
		m_pTable->ModifyRecord(GetRow(nRow));
		m_pTable->Update();
	}
		CATCH(SqlException, e)
	{
		if (!m_bRecordable)
			AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

		return ErrorMessage(cwsprintf(_TB("DBTSlaveBuffered {0-%s}: Add record failed."), CString(GetRuntimeClass()->m_lpszClassName)), e);
	}
	END_CATCH

		if (!m_bRecordable)
			AfxGetApplicationContext()->m_MacroRecorderStatus = pStatus;

	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::Update()
{
	//Filtraggio
	RecordArray* pTemp = m_pRecords;
	if (m_bAllowFilter)
	{
		m_pRecords = m_pAllRecords;
	}
	BOOL bOk = TRUE;
	//prima cancello da DB tutti quelli che sono stati cancellati
	for (int i = m_DeletedSlaves.GetUpperBound(); i >= 0; i--)
	{
		DBTSlave* pValue = m_DeletedSlaves.GetAt(i);
		ASSERT(pValue->m_pMasterRecord == NULL);

		bOk = bOk & pValue->Delete();
		if (bOk)
		{
			m_DeletedSlaves.RemoveAt(i);
			delete pValue;
		}
	}
	bOk = bOk & DBTSlave::Update();

	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap* pData = m_DBTSlaveData[i];
		POSITION pos = pData->m_Slaves.GetStartPosition();
		while (pos)
		{
			SqlRecord* pKey = NULL;
			DBTSlave* pValue = NULL;
			pData->m_Slaves.GetNextAssoc(pos, pKey, pValue);

			ASSERT(pValue->m_pMasterRecord != NULL);
			if (pValue->m_pMasterRecord->IsStorable())
				bOk = bOk & pValue->Update();
			if (!bOk)
				break;
		}
	}
	//Filtraggio
	if (m_bAllowFilter) m_pRecords = pTemp;

	return bOk;
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::UpdateRows()
{
	TRY
	{
		int nStep = -1;

		for (int nOldRow = 0; nOldRow <= GetOldUpperBound(); nOldRow++)
		{
			nStep++;
			int nNewRow = m_pwModifiedRows->GetAt(nOldRow);
			if (nNewRow == UNTOUCHED)
				continue;
			m_pTable->Edit();
			m_pTable->ModifyRecord(GetRow(nNewRow));

			int nIdxOldRow = -1;
			SqlRecord* pOld = NULL;
			if (IsModifiedRow(nNewRow, nIdxOldRow) && nIdxOldRow >= 0)
				pOld = GetOldRow(nIdxOldRow);

			m_pTable->Update(pOld);
			nStep = 0;
		}

		return TRUE;
	}
		CATCH(SqlException, e)
	{
		return ErrorMessage(cwsprintf(_TB("DBTSlaveBuffered {0-%s}: Change record failed."), CString(GetRuntimeClass()->m_lpszClassName)), e);
	}
	END_CATCH
}


// elimina gli eventuali record che non sono piu` utilizzati (ad esempio
// sono diminuite le righe, o in caso di chiave non sequenziale numerica,
// un codice non e` piu` usato)
//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::DeleteUnusedRows()
{
	ASSERT(m_pTable);

	TRY
	{
		if (!m_pwDeletedRows)
		m_pwDeletedRows = new CDWordArray;
		else
			m_pwDeletedRows->RemoveAll();

	// cancellazione usando la KeyedDelete
	for (int nRow = 0; nRow <= GetOldUpperBound(); nRow++)
	{
		if (!GetOldRow(nRow)->IsModified())
		{
			m_pTable->ModifyRecord(GetOldRow(nRow));
			m_pTable->Delete(GetOldRow(nRow));
			m_pwDeletedRows->Add(nRow);
		}
	}
	}
		CATCH(SqlException, e)
	{
		return ErrorMessage(cwsprintf(_TB("DBTSlaveBuffered {0-%s}: Delete record failed."), CString(GetRuntimeClass()->m_lpszClassName)), e);
	}
	END_CATCH
		return TRUE;
}

// Bisogna testare la validita` della riga per non inserire le righe
// di corpo non validate. L'ultima riga e` presente ma non valida)
// perche` il body edit la alloca quando ci si muove oltre l'ultima riga
//
//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::AddNewData()
{
	TRY
	{
		// Loop utile anche alla ricostruzione della chiave primaria. Vedi ModifyData
		// (rinumera durante l'inserimento delle singole righe)
		//
		for (int i = 0, nRiga = 0; i <= GetUpperBound(); i++)
		if (GetRow(i)->IsStorable())
		{
			OnPreparePrimaryKey(nRiga, GetRow(i));
			GetClientDocs()->OnPreparePrimaryKey(this, nRiga, GetRow(i));
			nRiga++;

			if (!AddNewRow(i))
				return FALSE;
		}

		m_bUpdated = TRUE;
	}
		CATCH(SqlException, e)
	{
		return ErrorMessage(cwsprintf(_TB("DBTSlaveBuffered {0-%s}: Add record failed."), CString(GetRuntimeClass()->m_lpszClassName)), e);
	}
	END_CATCH
		return TRUE;
}


// Al primo errore si ferma demandando alla roolback il ripristino della
// situazione coerente ante piantata
//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::ModifyData()
{
	if (m_bAllowEmpty && m_bEmpty)
		return AddNewData();

	ASSERT(m_pTable);

	BOOL bOk = TRUE;
	BOOL nFoundRow = -1;

	if (!m_pwNewRows)
		m_pwNewRows = new CDWordArray;
	else
		m_pwNewRows->RemoveAll();

	if (!m_pwModifiedRows)
		m_pwModifiedRows = new CDWordArray;
	else
		m_pwModifiedRows->RemoveAll();


	// Prepara il vettore parallelo al buffer OLD (letto da disco) con
	// un tag fittizzio che indica che la riga non deve essere Updatata
	for (int nOldRow = 0; nOldRow <= GetOldUpperBound(); nOldRow++)
		m_pwModifiedRows->Add(UNTOUCHED);

	// Loop di ricostruzione della chiave primaria. Utile essenzialmente
	// per rinumerare automaticamente le righe a causa di possibili righe
	// non storabili che lascerebbero un buco.
	//
	for (int i = 0, nRiga = 0; i <= GetUpperBound(); i++)
	{
		SqlRecord *pRow = GetRow(i);
		if (pRow->IsStorable())
		{
			OnPreparePrimaryKey(nRiga, pRow);
			GetClientDocs()->OnPreparePrimaryKey(this, nRiga, pRow);
			nRiga++;
		}
	}
	for (int nRow = 0; nRow <= GetUpperBound(); nRow++)
	{
		// sono gestiti solo i record validati come Storable
		if (!GetRow(nRow)->IsStorable())
			continue;

		//determina cosa e` successo alla riga (modificata, aggiunta, uguale)
		switch (CompareRow(nRow, nFoundRow))
		{
		case NEW_ROW:
			m_pwNewRows->Add(nRow);
			break;

		case MODIFIED:
			m_pwModifiedRows->SetAt(nFoundRow, nRow);
			break;

		default:
			break;
		}
	}

	if (m_pwNewRows->GetUpperBound() >= 0)
		m_bUpdated = TRUE;
	else
	{
		for (int nOldRow = 0; nOldRow <= GetOldUpperBound(); nOldRow++)
		{
			if (
				m_pwModifiedRows->GetAt(nOldRow) != UNTOUCHED ||
				!GetOldRow(nOldRow)->IsModified()
				)
			{
				m_bUpdated = TRUE;
				break;
			}
		}
	}

	// Esegue le sole Update per tutte le righe che hanno mantenuto
	// la stessa chiave primaria, ma hanno cambiato valore
	if (!UpdateRows())
		return FALSE;

	// cancella le righe non piu' usate a causa di cancellazioni
	// esplicite o variazioni di dati di chiave primaria		
	if (!DeleteUnusedRows())
		return FALSE;

	// Aggiunge tutti le nuove righe inserite esplicitamente o variate nella
	// chiave primaria
	for (int j = 0; j <= m_pwNewRows->GetUpperBound(); j++)
		if (!AddNewRow(m_pwNewRows->GetAt(j)))
			return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::IsNewRow(int nRow) const
{
	if (!m_pwNewRows)
		return FALSE;

	for (int nIdx = 0; nIdx <= m_pwNewRows->GetUpperBound(); nIdx++)
		if (nRow == m_pwNewRows->GetAt(nIdx))
			return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::IsModifiedRow(int nRow, int& nOldRow) const
{
	if (!m_pwModifiedRows)
		return FALSE;

	for (int nIdx = 0; nIdx <= m_pwModifiedRows->GetUpperBound(); nIdx++)
		if (nRow == m_pwModifiedRows->GetAt(nIdx))
		{
			nOldRow = nIdx;
			return TRUE;
		}

	return FALSE;
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::IsDeletedRow(int nRow) const
{
	if (!m_pwDeletedRows)
		return FALSE;

	for (int nIdx = 0; nIdx <= m_pwDeletedRows->GetUpperBound(); nIdx++)
		if (nRow == m_pwDeletedRows->GetAt(nIdx))
			return TRUE;

	return FALSE;
}
//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::Delete()
{
	if (m_bEmpty)
		return TRUE;

	ASSERT(m_pTable);
	TRY
	{
		for (int i = m_DeletedSlaves.GetUpperBound(); i >= 0; i--)
		{
			DBTSlave* pSlave = m_DeletedSlaves[i];
			if (!pSlave->Delete())
				return FALSE;
			m_DeletedSlaves.RemoveAt(i);
			delete pSlave;
		}
	// cancellazione usando la KeyedDelete
	for (int nRow = 0; nRow <= GetOldUpperBound(); nRow++)
	{
		for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
		{
			BOOL bCreated = FALSE;
			DBTSlave* pSlave = GetDBTSlave(nRow, m_DBTSlaveData[i], FALSE);
			if (!pSlave)
			{
				pSlave = GetDBTSlave(nRow, m_DBTSlaveData[i], TRUE);
				bCreated = TRUE;
			}
			BOOL bError = pSlave && !pSlave->Delete();
			if (bCreated)
				DestroyDBTSlave(pSlave);
			if (bError)
				return FALSE;
		}
		m_pTable->ModifyRecord(GetOldRow(nRow));
		m_pTable->Delete(GetOldRow(nRow));
	}
	}
		CATCH(SqlException, e)
	{
		return ErrorMessage(cwsprintf(_TB("DBTSlaveBuffered {0-%s}: Delete body failed."), CString(GetRuntimeClass()->m_lpszClassName)), e);
	}
	END_CATCH

		return TRUE;
}

//-----------------------------------------------------------------------------	
SqlRecord* DBTSlaveBuffered::GetRow(int nRow) const
{
	ASSERT_VALID(m_pRecords);
	return (nRow >= 0 && nRow < m_pRecords->GetSize()) ? (SqlRecord*)m_pRecords->GetAt(nRow) : NULL;
}
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::SetCurrentDBTSlave(DBTSlaveMap* pData, DBTSlave* pSlave)
{
	if (!pData->m_pCurrentDBTSlave)
		return;

	DBTSlave* pCurrentDBTSlave = pData->m_pCurrentDBTSlave;
	pData->m_pCurrentDBTSlave = pSlave;//va fatta prima di assegnarlo al bodyedit per gestire correttamente i BEN_ROW_CHANGED

	if (pCurrentDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
	{
		DBTSlaveBuffered* pCurrentBuff = (DBTSlaveBuffered*)pCurrentDBTSlave;
		//non devo propagare lo stato di readonly ai figli, non è detto che siano legati!
		//non chiamo la SetReadOnly, perche' non devo toccare i dataobj (problema dell'impossibilita' di mettere
		//a readonly un campo nella OnDisableControlsForEdit
		((DBTSlaveBuffered*)pSlave)->m_bReadOnly = pCurrentBuff->m_bReadOnly;
		CBodyEditPointers* parBodies = pCurrentBuff->GetBodyEdits();
		if (parBodies && parBodies->GetCount())
		{
			for (int i = 0; i < parBodies->GetCount(); i++)
			{
				CBodyEdit* pEdit = parBodies->GetPointerAt(i);
				if (pEdit)
				{
					DBTSlaveBuffered* pNewSlave = (DBTSlaveBuffered*)pSlave;
					if (!IsActive())
						pNewSlave = pNewSlave ? pNewSlave->GetActiveSibling() : NULL;

					//il prototipo non può essere messo come dbt del bodyedit,
					//non posso metterci dentro dei dati!
					if (pNewSlave == pData->m_pDBTSlavePrototype)
					{
						pNewSlave = NULL;
					}

					pEdit->OnSwitchDBT(pNewSlave);
				}
			}
		}
	}

}
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::AlignDBTSlaveToCurrentRow()
{
	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap *pData = m_DBTSlaveData[i];
		DBTSlave* pSlave = GetDBTSlave(m_nCurrentRow, pData, TRUE);
		if (!pSlave)
			pSlave = pData->m_pDBTSlavePrototype;
		SetCurrentDBTSlave(pData, pSlave);
	}
}
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::SetCurrentRow(int nRow)
{
	SetCurrentRow(nRow, TRUE);
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::SetCurrentRowForValueChanged(int nRow)
{
	SetCurrentRow(nRow);
	CBodyEditPointers* parBodies = GetBodyEdits();
	if (parBodies)
	{
		for (int i = 0; i < parBodies->GetCount(); i++)
		{
			CBodyEdit* pEdit = parBodies->GetPointerAt(i);
			if (pEdit)
				pEdit->SetCurrLine(nRow);
		}
	}
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::SetCurrentRow(int nRow, BOOL bAlignCurrentSlave /*= TRUE*/)
{
	ASSERT_TRACE(nRow < m_pRecords->GetSize(), _T("DBTSlaveBuffered::SetCurrentRow oltre il massimo indice"));
	int r = min(nRow, m_pRecords->GetUpperBound());


	BOOL bChanged = r != m_nCurrentRow;

	m_nCurrentRow = r;

	if (bAlignCurrentSlave)
		AlignDBTSlaveToCurrentRow();

	if (bChanged)
	{
		OnSetCurrentRow();
		GetClientDocs()->OnSetCurrentRow(this);
	}
}

//----------------------------------------------------------------------------------
void DBTSlaveBuffered::DestroyDBTSlave(DBTSlave* pSlave)
{
	if (!pSlave) return;

	ASSERT(pSlave->m_pMasterRecord);
	DBTSlaveMap* pData = GetDBTSlaveData(pSlave->GetRuntimeClass());
	if (pSlave == pData->m_pCurrentDBTSlave)
		SetCurrentDBTSlave(pData, pData->m_pDBTSlavePrototype);
	DBTSlave* pSlave1 = NULL;
	if (!pData->m_Slaves.Lookup(pSlave->m_pMasterRecord, pSlave1))
	{
		ASSERT(FALSE);
		return;
	}
	ASSERT(pSlave == pSlave1);
	pData->m_Slaves.RemoveKey(pSlave->m_pMasterRecord);
	delete pSlave;
}

//----------------------------------------------------------------------------------
DBTSlave* DBTSlaveBuffered::CreateDBTSlave(DBTSlave* pPrototype)
{
	DBTSlave* pSlave = NULL;
	SqlRecord* pRecord = pPrototype->GetRecord();
	bool typed = pRecord->GetRuntimeClass() != RUNTIME_CLASS(SqlRecord) && pRecord->GetRuntimeClass() != RUNTIME_CLASS(DynamicSqlRecord);
	BOOL bBuffered = FALSE;
	if (pPrototype->GetRuntimeClass() == RUNTIME_CLASS(DBTSlave))
	{
		//passo di qui nel caso di Slave creato con EasyBuilder
		pSlave = typed
			? new DBTSlave(pRecord->GetRuntimeClass(), GetDocument(), pPrototype->GetNamespace().GetObjectName(), pPrototype->GetAllowEmpty())
			: new DBTSlave(pPrototype->GetRecord()->GetTableName(), GetDocument(), pPrototype->GetNamespace().GetObjectName(), pPrototype->GetAllowEmpty());
		pSlave->m_pDBTMaster = this;
	}
	else if (pPrototype->GetRuntimeClass() == RUNTIME_CLASS(DynDBTSlaveBuffered))
	{
		//passo di qui nel caso di Slave Buffered creato con EasyBuilder
		pSlave = typed
			? new DynDBTSlaveBuffered(pRecord->GetRuntimeClass(), GetDocument(), pPrototype->GetNamespace().GetObjectName(), pPrototype->GetAllowEmpty(), ((DBTSlaveBuffered*)pPrototype)->IsCheckingDuplicateKey())
			: new DynDBTSlaveBuffered(pRecord->GetTableName(), GetDocument(), pPrototype->GetNamespace().GetObjectName(), pPrototype->GetAllowEmpty(), ((DBTSlaveBuffered*)pPrototype)->IsCheckingDuplicateKey());
		pSlave->m_pDBTMaster = this;

		//CopyPredicates
		((DynDBTSlaveBuffered*)pSlave)->GetQuery()->CopyPredicates(((DynDBTSlaveBuffered*)pPrototype)->GetQuery()->GetPredicates());

		bBuffered = TRUE;
	}
	else
	{
		//passo di qui nel caso di DBT C++
		pSlave = (DBTSlave*)pPrototype->GetRuntimeClass()->CreateObject();
		pSlave->Initialize(pPrototype->GetRecord()->GetRuntimeClass(), pPrototype->GetDocument(), pPrototype->GetNamespace().GetObjectName());
		pSlave->m_pDBTMaster = this;
		//devo creare il prototipo 
		if (pSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		{
			TArray<HKLInfo>& prototypeInfos = ((DBTSlaveBuffered*)pPrototype)->m_arHKLInfos;
			TArray<HKLInfo>& slaveInfos = ((DBTSlaveBuffered*)pSlave)->m_arHKLInfos;
			for (int i = 0; i < prototypeInfos.GetCount(); i++)
			{
				slaveInfos.Add(prototypeInfos[i]->Clone());
			}
			((DBTSlaveBuffered*)pSlave)->PrepareDynamicColumns(FALSE);
			for (int i = 0; i < ((DBTSlaveBuffered*)pPrototype)->m_DBTSlaveData.GetSize(); i++)
			{
				DBTSlaveMap *pData = ((DBTSlaveBuffered*)pPrototype)->m_DBTSlaveData[i];
				((DBTSlaveBuffered*)pSlave)->Attach(CreateDBTSlave(pData->m_pDBTSlavePrototype));
			}
			bBuffered = TRUE;
		}
	}
	pSlave->SetAllowEmpty(pPrototype->GetAllowEmpty());
	pSlave->m_ReadType = pPrototype->m_ReadType;
	CAbstractFormDoc* pDoc = GetDocument();
	if (bBuffered && pDoc)
	{
		BOOL bReadOnly = pDoc->m_bBatch ? pDoc->m_bBatchRunning : pDoc->GetFormMode() == CBaseDocument::BROWSE;
		((DBTSlaveBuffered*)pSlave)->SetReadOnly(bReadOnly);

		//il dbt prototipo deve seguire le sorti di quello corrente
		//perché sono i suoi dataobj ad essere addlinkati nelle colonne
		DBTSlaveBuffered* pDBTPrototype = (DBTSlaveBuffered*)((DBTSlaveBuffered*)pSlave)->GetMainPrototype();
		if (pDBTPrototype)
			pDBTPrototype->SetReadOnly(bReadOnly);
	}
	pSlave->SetOnlyForRead(pPrototype->IsOnlyForRead());

	SqlRecord* pAddedRec = pSlave->GetRecord();
	SqlRecord* pPrototypeRecord = pRecord;

	int sz = min(pAddedRec->GetSizeEx(), pPrototypeRecord->GetSizeEx());
	for (int nCol = 0; nCol < sz; nCol++)
	{
		DataObj* pD = pAddedRec->GetDataObjAt(nCol);
		DataObj* pS = pPrototypeRecord->GetDataObjAt(nCol);

		pD->AttachEvents(new CMuteDataEventsProxy(pS));
	}
	return pSlave;
}

//----------------------------------------------------------------------------------
void  DBTSlaveBuffered::GetSlavesDBTS(CStringArray& arNames)
{
	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap* pData = m_DBTSlaveData.GetAt(i);

		arNames.Add(pData->m_pDBTSlavePrototype->GetNamespace().GetObjectName());
	}
}
//----------------------------------------------------------------------------------
DBTSlaveMap* DBTSlaveBuffered::GetDBTSlaveData(const CString& strDBTName)
{
	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap* pData = m_DBTSlaveData.GetAt(i);

		if (strDBTName.IsEmpty() || pData->m_pDBTSlavePrototype->GetNamespace().GetObjectName().CompareNoCase(strDBTName) == 0)
			return pData;
	}
	return NULL;
}
//----------------------------------------------------------------------------------
DBTSlaveMap* DBTSlaveBuffered::GetDBTSlaveData(const CRuntimeClass*pClass)
{
	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap* pData = m_DBTSlaveData.GetAt(i);

		if (pData->m_pDBTSlavePrototype->GetRuntimeClass() == pClass)
			return pData;
	}
	return NULL;
}
//----------------------------------------------------------------------------------
DBTSlave* DBTSlaveBuffered::GetCurrentDBTSlave(const CString& strDBTName)
{
	DBTSlaveMap* pData = GetDBTSlaveData(strDBTName);
	return pData ? pData->m_pCurrentDBTSlave : NULL;
}
//----------------------------------------------------------------------------------
DBTSlave* DBTSlaveBuffered::GetDBTSlave(const CString& strDBTName, int nRow, BOOL bForceCreate /*= FALSE*/)
{
	DBTSlaveMap* pData = GetDBTSlaveData(strDBTName);
	return pData
		? GetDBTSlave(nRow, pData, bForceCreate)
		: NULL;
}
//----------------------------------------------------------------------------------
DBTSlave* DBTSlaveBuffered::GetDBTSlave(const CString& strDBTName, SqlRecord* pRecordMaster, BOOL bForceCreate /*= FALSE*/)
{
	DBTSlaveMap* pData = GetDBTSlaveData(strDBTName);
	return pData
		? GetDBTSlave(pRecordMaster, pData, bForceCreate)
		: NULL;
}
//----------------------------------------------------------------------------------
DBTSlave* DBTSlaveBuffered::GetCurrentDBTSlave()
{
	return GetCurrentDBTSlave(_T(""));
}
//----------------------------------------------------------------------------------
DBTSlave* DBTSlaveBuffered::GetDBTSlave(int nRow, BOOL bForceCreate /*= FALSE*/)
{
	return GetDBTSlave(_T(""), nRow, bForceCreate);
}
//----------------------------------------------------------------------------------
DBTSlave* DBTSlaveBuffered::GetDBTSlave(SqlRecord* pRecordMaster, BOOL bForceCreate /*= FALSE*/)
{
	return GetDBTSlave(_T(""), pRecordMaster, bForceCreate);
}

//----------------------------------------------------------------------------------
DBTSlave* DBTSlaveBuffered::GetDBTSlave(SqlRecord* pRecordMaster, DBTSlaveMap* pData, BOOL bForceCreate /*= FALSE*/)
{
	if (!pRecordMaster)
		return NULL;
	if (!pData->m_pDBTSlavePrototype)
		return NULL;

	DBTSlave* pSlave = NULL;
	if (pData->m_Slaves.Lookup(pRecordMaster, pSlave))
	{
		ASSERT(pSlave->m_pMasterRecord);
		return pSlave;
	}

	if (!bForceCreate)
		return NULL;

	pSlave = CreateDBTSlave(pData->m_pDBTSlavePrototype);
	pSlave->m_pMasterRecord = pRecordMaster;
	pData->m_Slaves[pRecordMaster] = pSlave;
	VERIFY(pSlave->Open());
	if (m_bFindDataCalled) //solo se ho chiamato la FindData sul papà chiamo quella sul figlio
		VERIFY(pSlave->FindData());

	//se il dbt non dovra' essere risalvato, chiudo il cursore per non sprecare memoria
	if (pSlave->IsOnlyForRead())
		pSlave->Close();

	if (!pSlave->m_bOnlyDelete)
	{
		// normal processing in interctive mode
		switch (GetDocument()->GetFormMode())
		{
		case CBaseDocument::NEW:
			pSlave->OnDisableControlsForAddNew();
			GetClientDocs()->OnDisableControlsForAddNew(pSlave);
			pSlave->OnDisableControlsAlways();
			GetClientDocs()->OnDisableControlsAlways(pSlave);
			break;

		case CBaseDocument::EDIT:
			pSlave->OnDisableControlsForEdit();
			GetClientDocs()->OnDisableControlsForEdit(pSlave);
			pSlave->OnDisableControlsAlways();
			GetClientDocs()->OnDisableControlsAlways(pSlave);
			break;

		case CBaseDocument::FIND:
			pSlave->OnEnableControlsForFind();
			GetClientDocs()->OnEnableControlsForFind(pSlave);
			break;
		}
	}
	return pSlave;
}

//-----------------------------------------------------------------------------	
DBTSlave* DBTSlaveBuffered::GetDBTSlave(int nRow, DBTSlaveMap* pData, BOOL bForceCreate /*= FALSE*/)
{
	SqlRecord* pRecord = GetRow(nRow);
	return GetDBTSlave(pRecord, pData, bForceCreate);
}

//-----------------------------------------------------------------------------	
DBTObject* DBTSlaveBuffered::GetDBTObject(const CRuntimeClass* pDBTClass) const
{
	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap *pData = m_DBTSlaveData[i];
		if (!pData->m_pCurrentDBTSlave)
			continue;
		if (pData->m_pCurrentDBTSlave->IsKindOf(pDBTClass))
			return pData->m_pCurrentDBTSlave;
		if (pData->m_pCurrentDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		{
			DBTObject* pSub = ((DBTSlaveBuffered*)pData->m_pCurrentDBTSlave.operator DBTSlave *())->GetDBTObject(pDBTClass);
			if (pSub)
				return pSub;
		}

	}
	return NULL;
}
//-----------------------------------------------------------------------------	
DBTObject* DBTSlaveBuffered::GetDBTObject(const CString& sTableName) const
{
	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap *pData = m_DBTSlaveData[i];
		if (!pData->m_pCurrentDBTSlave)
			continue;
		if (pData->m_pCurrentDBTSlave->GetRecord()->GetTableName().CompareNoCase(sTableName) == 0)
			return pData->m_pCurrentDBTSlave;
		if (pData->m_pCurrentDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		{
			DBTObject* pSub = ((DBTSlaveBuffered*)pData->m_pCurrentDBTSlave.operator DBTSlave *())->GetDBTObject(sTableName);
			if (pSub)
				return pSub;
		}
	}
	return NULL;
}
//-----------------------------------------------------------------------------	
DBTObject*	DBTSlaveBuffered::GetDBTObject(const CTBNamespace& aNs) const
{
	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap *pData = m_DBTSlaveData[i];
		if (!pData->m_pCurrentDBTSlave)
			continue;
		if (pData->m_pCurrentDBTSlave->GetNamespace() == aNs)
			return pData->m_pCurrentDBTSlave;
		if (pData->m_pCurrentDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		{
			DBTObject* pSub = ((DBTSlaveBuffered*)pData->m_pCurrentDBTSlave.operator DBTSlave *())->GetDBTObject(aNs);
			if (pSub)
				return pSub;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::GetJson(BOOL bWithChildren, CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound)
{
	jsonSerializer.OpenArray(GetName());
	for (int i = 0; i < GetRowCount(); i++)
	{
		SqlRecord *pRecord = GetRow(i);
		jsonSerializer.OpenObject(i);
		pRecord->GetJson(jsonSerializer, bOnlyWebBound);
		jsonSerializer.CloseObject();
	}
	jsonSerializer.CloseArray();
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::SetJson(BOOL bWithChildren, CJsonParser& jsonParser)
{
	if (jsonParser.BeginReadArray(GetName()))
	{
		for (int i = 0; i < GetRowCount(); i++)
		{
			SqlRecord *pRecord = GetRow(i);
			if (jsonParser.BeginReadObject(i))
			{
				pRecord->SetJson(jsonParser);
				jsonParser.EndReadObject();
			}
		}
		jsonParser.EndReadObject();
	}
}

//-----------------------------------------------------------------------------	
DBTObject*	DBTSlaveBuffered::GetDBTByName(const CString& sDbtName) const
{
	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap *pData = m_DBTSlaveData[i];
		if (!pData->m_pCurrentDBTSlave)
			continue;
		if (pData->m_pCurrentDBTSlave->GetNamespace().GetObjectName().CompareNoCase(sDbtName) == 0)
			return pData->m_pCurrentDBTSlave;

		if (pData->m_pCurrentDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		{
			DBTObject* pSub = ((DBTSlaveBuffered*)pData->m_pCurrentDBTSlave.operator DBTSlave *())->GetDBTByName(sDbtName);
			if (pSub)
				return pSub;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------	
DBTObject* DBTSlaveBuffered::GetDBTObject(SqlRecord* pRec)
{
	DBTObject* pDBT = __super::GetDBTObject(pRec);
	if (pDBT)
		return pDBT;

	if (pRec == GetCurrentRow())
		return this;

	//for (int i = 0; i <= m_pRecords->GetUpperBound(); i++)
	//{
	//	if (pRec == m_pRecords->GetAt(i))
	//		return this;
	//}

	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap *pData = m_DBTSlaveData[i];

		if (!pData->m_pCurrentDBTSlave)
			continue;

		pDBT = pData->m_pCurrentDBTSlave->GetDBTObject(pRec);
		if (pDBT)
			return pDBT;
	}

	return NULL;
}
//-------------------------------------------------------------------------------
SqlRecord* DBTSlaveBuffered::GetCurrentMasterRecord(const CString& strDBTName)
{
	ASSERT(!strDBTName.IsEmpty() || m_DBTSlaveData.GetCount() <= 1);

	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap* pData = m_DBTSlaveData[i];
		if (!pData->m_pCurrentDBTSlave)
			continue;
		if (!strDBTName.IsEmpty() && pData->m_pCurrentDBTSlave->GetNamespace().GetObjectName().CompareNoCase(strDBTName) != 0)
			continue;
		return pData->m_pCurrentDBTSlave->m_pMasterRecord;
	}
	return NULL;
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::SetReadOnly(BOOL bReadOnly/* = TRUE*/, BOOL bRecursive /*= FALSE*/)
{
	if (m_bAlwaysReadOnly)
		return;
	// Global flag usato per gestire il readonly
	m_bReadOnly = bReadOnly;

	InternalSetModified();

	// modifica anche il record di buffer del generico DBTObject
	m_pRecord->SetReadOnly(bReadOnly);

	// modifica lo stato del nuovo buffer di riga
	RecordArray* parRecords = (m_bAllowFilter && m_pAllRecords) ? m_pAllRecords : m_pRecords;
	ASSERT_VALID(parRecords);
	for (int nRow = 0; nRow < parRecords->GetSize(); nRow++)
	{
		SqlRecord* pRec = parRecords->GetAt(nRow);

		pRec->SetReadOnly(bReadOnly);
	}

	if (bRecursive)
	{
		for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
		{
			DBTSlaveMap *pData = m_DBTSlaveData[i];
			if (!pData->m_pDBTSlavePrototype ||
				!pData->m_pDBTSlavePrototype->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
				continue;

			((DBTSlaveBuffered*)pData->m_pDBTSlavePrototype)->SetReadOnly(bReadOnly, TRUE);
			POSITION pos = pData->m_Slaves.GetStartPosition();
			while (pos)
			{
				SqlRecord* pKey = NULL;
				DBTSlave* pValue = NULL;
				pData->m_Slaves.GetNextAssoc(pos, pKey, pValue);
				((DBTSlaveBuffered*)pValue)->SetReadOnly(bReadOnly, TRUE);
			}

		}
	}
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::SetReadOnlyCurrentRow(BOOL bReadOnly/* = TRUE*/)
{
	SetReadOnlyRow(GetCurrentRowIdx(), bReadOnly);
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::SetReadOnlyRow(int nRow, BOOL bReadOnly/* = TRUE*/)
{
	SqlRecord* pRec = GetRow(nRow);

	pRec->SetReadOnly(bReadOnly);
}

// Per motivi di performance non controlla i dataobj all'interno ma solo il flag
// globale	
//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::IsModified() const
{
	if (m_bModified)
		return TRUE;

	// verifica anche il record di buffer del generico DBTObject
	if (m_pRecord->IsDataModified())
		return TRUE;

	// verifica lo stato delle righe
	RecordArray* parRecords = (m_bAllowFilter && m_pAllRecords) ? m_pAllRecords : m_pRecords;
	ASSERT_VALID(parRecords);
	for (int nRow = 0; nRow < parRecords->GetSize(); nRow++)
	{
		SqlRecord* pRec = parRecords->GetAt(nRow);

		if (pRec->IsDataModified())
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------	
DWORD DBTSlaveBuffered::GetLatestModifyTime()
{
	return m_dwLatestModifyTime;
}
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::InternalSetModified(BOOL bModified /*= TRUE*/)
{
	if (bModified)
		m_dwLatestModifyTime = GetTickCount();

	// Global flag usato per gestire il readonly
	m_bModified = bModified;

}
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::SetModified(BOOL bModified/* = TRUE*/)
{
	InternalSetModified(bModified);

	// modifica anche il record di buffer del generico DBTObject
	m_pRecord->SetDataModified(bModified);

	// modifica lo stato del nuovo buffer di riga
	RecordArray* parRecords = (m_bAllowFilter && m_pAllRecords) ? m_pAllRecords : m_pRecords;
	ASSERT_VALID(parRecords);
	for (int nRow = 0; nRow < parRecords->GetSize(); nRow++)
	{
		SqlRecord* pRec = parRecords->GetAt(nRow);

		pRec->SetDataModified(bModified);
	}
}

//-----------------------------------------------------------------------------
void DBTSlaveBuffered::LoadAllRecords()
{
	// carica tutti i dati in memoria!!
	while (MoreRecords())
		LoadNextRecords();
}

//-----------------------------------------------------------------------------
BOOL DBTSlaveBuffered::LoadMoreRows(int preloadStep)
{
	SetPreloadStep(preloadStep);
	if (!MoreRecords())
	{
		return FALSE;
	}

	LoadNextRecords();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DBTSlaveBuffered::ManualLoad()
{
	Close();
	RemoveAll();
	return Open() && FindData(FALSE);
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemorySort()
{
	if (m_pRecords == NULL)
		return FALSE;
	if (m_pRecords->GetSize() <= 1)
		return TRUE;

	if (m_pRecords->m_arSortedColumIndex.GetSize() == 0)
		return FALSE;
	//----

	m_pRecords->m_arFormatCtrls.RemoveAll();

	CBodyEditPointers* parBodies = GetBodyEdits();
	if (parBodies && parBodies->GetSize()/* == 1*/)
	{
		CBodyEdit *pBody = parBodies->GetPointerAt(0);
		if (pBody)
		{

			for (int i = 0; i < m_pRecords->m_arSortedColumIndex.GetSize(); i++)
			{
				int idx = m_pRecords->m_arSortedColumIndex.GetAt(i);

				ColumnInfo* pCol = pBody->GetVisibleColumnFromDataIdx(idx);

				if (
					pCol &&
					(
						pCol->GetControl()->IsKindOf(RUNTIME_CLASS(CDescriptionCombo))
						||
						pCol->GetControl()->IsKindOf(RUNTIME_CLASS(CBoolButton))
						||
						pCol->GetControl()->IsKindOf(RUNTIME_CLASS(CEnumCombo))
						||
						pCol->GetControl()->IsKindOf(RUNTIME_CLASS(CEnumStatic))
						||
						pCol->GetControl()->IsKindOf(RUNTIME_CLASS(CParsedStateImage))
						)
					)
				{
					ASSERT_VALID(pCol);
					TRACE((LPCTSTR)cwsprintf(L"\tMemorySort on column %s, %d\n", pCol->GetTitle(), idx));
					ASSERT(pCol->GetParsedCtrl()->GetDataType() == m_pRecords->GetAt(0)->GetDataObjAt(idx)->GetDataType());

					m_pRecords->m_arFormatCtrls.Add(pCol->GetParsedCtrl());
				}
				else
				{
					m_pRecords->m_arFormatCtrls.Add(NULL);
				}
			}
		}
	}

	//----
	m_pRecords->HeapSort();

	OnMemorySorted();

	return TRUE;
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemorySort(LPCTSTR szOrderBy)
{
	TRACE(L"MemorySort by %s\n", szOrderBy);

	if (m_pRecords == NULL)
		return FALSE;
	if (m_pRecords->GetSize() <= 1)
		return TRUE;

	if (!m_pRecords->SetOrderBy(szOrderBy))
		return FALSE;

	return MemorySort();
}

//--------------------------------------------------------------------------
void DBTSlaveBuffered::SuspendObservables()
{
	m_pAllRecords->EnableObservables(FALSE);
	m_pRecords->EnableObservables(FALSE);
}

//--------------------------------------------------------------------------
void DBTSlaveBuffered::ResumeObservables()
{
	m_pAllRecords->EnableObservables(TRUE);
	m_pRecords->EnableObservables(TRUE);
	// avviso il bodyedit di fare un giro di update unico
	for (int i = 0; i < m_arBodyPtr.GetSize(); i++)
	{
		CBodyEdit* pBody = m_arBodyPtr.GetPointerAt(i);
		if (pBody)
			pBody->UpdateFooters();
	}
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::CalcSum(int nIndex, DataObj& aValue) const
{
	ASSERT_VALID(m_pRecords);
	if (m_pRecords == NULL)
		return FALSE;
	return m_pRecords->CalcSum(nIndex, aValue);
}

BOOL DBTSlaveBuffered::CalcSum(const CString& sColumnName, DataObj& aValue) const
{
	ASSERT_VALID(m_pRecords);
	if (m_pRecords == NULL)
		return FALSE;
	return m_pRecords->CalcSum(sColumnName, aValue);
}

//-----------------------------------------------------------------------------	
DataObj* DBTSlaveBuffered::GetMinElem(const CString& sColumnName)
{
	ASSERT_VALID(m_pRecords);
	if (m_pRecords == NULL)
		return FALSE;
	return m_pRecords->GetMinElem(sColumnName);
}

DataObj* DBTSlaveBuffered::GetMaxElem(const CString& sColumnName)
{
	ASSERT_VALID(m_pRecords);
	if (m_pRecords == NULL)
		return FALSE;
	return m_pRecords->GetMaxElem(sColumnName);
}

//-----------------------------------------------------------------------------	
DataObj* DBTSlaveBuffered::GetData(const CString& sName, int nRow/*= -1*/)
{
	if (GetMaster())
	{
		if (GetMaster()->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		{
			DBTSlaveBuffered* pParent = (DBTSlaveBuffered*)GetMaster();

			DBTSlave* pCurrent = NULL;

			CString sDBTName = this->GetNamespace().GetObjectName();
			if (sDBTName.IsEmpty())
				pCurrent = pParent->GetCurrentDBTSlave(GetRuntimeClass());
			else
				pCurrent = pParent->GetCurrentDBTSlave(sDBTName);

			if (!pCurrent)
			{
				ASSERT_TRACE1(FALSE, "Non è stato trovato il dato relativo al campo %s\n", sName);
				return NULL;
			}
			if (!pCurrent->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
			{
				ASSERT_TRACE1(FALSE, "I DBTSlave slavable non sono supportati per ora (campo %s)\n", sName);
				return NULL;
			}
			return dynamic_cast<DBTSlaveBuffered*>(pCurrent)->GetData(sName, nRow);
		}
	}

	if (nRow == -1)
		nRow = this->m_nCurrentRow;

	if (nRow < 0 || nRow >= (m_pAllRecords ? m_pAllRecords : m_pRecords)->GetSize())
	{
		//ASSERT_TRACE(FALSE, "DBTSlaveBuffered::GetData called with wrong row number");
		return NULL;
	}

	SqlRecord* pRec = (m_pAllRecords ? m_pAllRecords : m_pRecords)->GetAt(nRow);
	ASSERT_VALID(pRec);

	int idx = sName.Find('.');
	DataObj* pData = pRec->GetDataObjFromColumnName(idx > 0 ? sName.Mid(idx + 1) : sName);
	ASSERT_VALID(pData);

	return pData;
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::PrepareSymbolTable(SymTable* pTable)
{
	BOOL bOk = __super::PrepareSymbolTable(pTable);
	if (!bOk)
		return FALSE;

	//---- ciclo sugli slavable
	for (int i = 0; i < m_DBTSlaveData.GetCount(); i++)
	{
		DBTSlaveMap* pData = m_DBTSlaveData.GetAt(i);
		ASSERT(pData);
		ASSERT_VALID(pData->m_pDBTSlavePrototype);

		if (!pData->m_pDBTSlavePrototype->PrepareSymbolTable(pTable))
			bOk = FALSE;
	}

	return bOk;
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::GetMatchRecords(RecordArray& ar, const CString& sExpr, SymTable* parent/*=NULL*/)
{
	if (m_pRecords->GetSize() == 0)
		return TRUE;

	SymTable table;
	if (!PrepareSymbolTable(&table))
		return FALSE;
	table.SetParent(parent);

	Parser parser(sExpr);
	Expression exprFilter(&table);
	if (!exprFilter.Parse(parser, DataType::Bool))
	{
		ASSERT_TRACE1(FALSE, "DBTSlaveBuffered::MatchRecords fails to parse %s\n", exprFilter.GetErrDescription());
		return FALSE;
	}

	//NO! ClearSlaveDBTs();
	int nSaveCurrentRow = m_nCurrentRow;

	for (int r = 0; r <= m_pRecords->GetUpperBound(); r++)
	{
		m_nCurrentRow = r;	//Fondamentale per la risoluzione del valore della colonna nella riga corrente
		//effettuata dal metodo virtuale GetData
		//----
		DataBool check;
		if (!exprFilter.Eval(check))
		{
			ASSERT_TRACE1(FALSE, "DBTSlaveBuffered::MemoryFilter fails to eval %s\n", exprFilter.GetErrDescription());
			return FALSE;
		}
		if (!check)
			continue;
		//----
		SqlRecord* pRec = m_pRecords->GetAt(r);
		ar.Add(pRec);
	}

	m_nCurrentRow = nSaveCurrentRow;
	return TRUE;
}

///////////////////////////////////////////////////////////////////////////////

BOOL DBTSlaveBuffered::IsAllowFilter()
{
	return m_bAllowFilter;
}

BOOL DBTSlaveBuffered::IsMemoryFilterActive() const
{
	return m_bAllowFilter && m_pAllRecords && m_arMemFilters.GetSize();
}

BOOL DBTSlaveBuffered::IsMemoryFiltered() const
{
	return IsMemoryFilterActive() && m_pAllRecords->GetUpperBound() != m_pRecords->GetUpperBound();
}

BOOL DBTSlaveBuffered::IsMemoryUIFilterActive(int nColumnDataIdx/* = -1*/) const
{
	for (int i = 0; i < m_arMemFilters.GetSize(); i++)
	{
		MemFilter* filter = (MemFilter*)m_arMemFilters[i];

		if (filter->m_bUI)
		{
			if (nColumnDataIdx == -1)
				return TRUE;
			if (filter->m_arColumnsDataIndex[0] == nColumnDataIdx)
				return TRUE;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::SetAllowFilter(BOOL bAllow)
{
	if (bAllow == m_bAllowFilter)
		return;

	if (bAllow)
	{
		ASSERT(m_pAllRecords == NULL);
		m_pAllRecords = new RecordArray();
		m_pRecords->SetOwns(FALSE);
		m_pAllRecords->Append(*m_pRecords);
	}
	else
	{
		//No! ClearSlaveDBTs();
		RemoveAllRecords(); m_nCurrentRow = -1;

		RecordArray* pTemp = m_pRecords;

		m_pRecords = m_pAllRecords;

		m_pAllRecords = NULL;

		SAFE_DELETE(pTemp);

		//----
		AlignBodyEdits(TRUE);
	}

	m_bAllowFilter = bAllow;
}

///////////////////////////////////////////////////////////////////////////////
DBTSlaveBuffered::MemFilter::MemFilter(DBTSlaveBuffered* pDBT)
	:
	m_pDBT(pDBT),
	m_eFilterType(FILTER_METHOD),
	m_bUI(FALSE),

	m_bAnd_Or(TRUE),
	m_eCmp(CMP_EQUAL),
	m_pCtrl(NULL),
	m_pCmpObj(NULL),
	m_pSymTable(NULL),
	m_pExpr(NULL)
{
}

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::MemFilter::MemFilter(DBTSlaveBuffered* pDBT, int nTop)
	:
	m_pDBT(pDBT),
	m_eFilterType(FILTER_TOP),
	m_bUI(FALSE),

	m_bAnd_Or(TRUE),
	m_eCmp(CMP_EQUAL),
	m_pCtrl(NULL),
	m_pCmpObj(NULL),
	m_pSymTable(NULL),
	m_pExpr(NULL),
	m_nTop(nTop)
{
}

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::MemFilter::MemFilter(DBTSlaveBuffered* pDBT, const CArray<int>& arColumnDataIndex, const DataObjArray& arFilterValue, BOOL bAnd_Or)
	:
	m_pDBT(pDBT),
	m_eFilterType(FILTER_DATAOBJS),
	m_bUI(FALSE),

	m_bAnd_Or(bAnd_Or),
	m_eCmp(CMP_EQUAL),
	m_pCtrl(NULL),
	m_pCmpObj(NULL),
	m_pSymTable(NULL),
	m_pExpr(NULL)
{
	m_arColumnsDataIndex.Append(arColumnDataIndex);
	m_arFilterValues.Append(arFilterValue);
}

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::MemFilter::MemFilter(DBTSlaveBuffered* pDBT, int nColumnDataIndex, DataObj* pFilterValue, ECompareType cmp)
	:
	m_pDBT(pDBT),
	m_eFilterType(FILTER_COLUMNDATA),
	m_bUI(FALSE),

	m_bAnd_Or(FALSE),
	m_eCmp(cmp),
	m_pCtrl(NULL),
	m_pCmpObj(pFilterValue),
	m_pSymTable(NULL),
	m_pExpr(NULL)
{
	m_arColumnsDataIndex.Add(nColumnDataIndex);
}

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::MemFilter::MemFilter(DBTSlaveBuffered* pDBT, int nColumnDataIndex, const CStringArray& arFilterStrValues, ECompareType eCmp, CParsedCtrl* pCtrl, DataObj* pCmpObj)
	:
	m_pDBT(pDBT),
	m_eFilterType(FILTER_STRINGS),
	m_bUI(TRUE),

	m_bAnd_Or(TRUE),
	m_eCmp(eCmp),
	m_pCtrl(pCtrl),
	m_pCmpObj(pCmpObj),
	m_pSymTable(NULL),
	m_pExpr(NULL)
{
	m_arColumnsDataIndex.Add(nColumnDataIndex);
	m_arFilterStrValues.Append(arFilterStrValues);
}

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::MemFilter::MemFilter(DBTSlaveBuffered* pDBT, Expression* pExpr, SymTable* pSymtable)
	:
	m_pDBT(pDBT),
	m_eFilterType(FILTER_EXPR),
	m_bUI(FALSE),

	m_bAnd_Or(TRUE),
	m_eCmp(CMP_EQUAL),
	m_pCtrl(NULL),
	m_pCmpObj(NULL),
	m_pSymTable(pSymtable),
	m_pExpr(pExpr)
{
}

//-----------------------------------------------------------------------------	
DBTSlaveBuffered::MemFilter::~MemFilter()
{
	SAFE_DELETE(m_pCmpObj); SAFE_DELETE(m_pExpr); SAFE_DELETE(m_pSymTable);
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemFilter::CheckDataObjs(SqlRecord* pRec)
{
	ASSERT(m_eFilterType == FILTER_DATAOBJS);

	BOOL bSkip = !m_bAnd_Or;

	for (int j = 0; j < m_arColumnsDataIndex.GetSize(); j++)
	{
		int nFieldIndex = m_arColumnsDataIndex[j];
		DataObj* pObj = pRec->GetDataObjAt(nFieldIndex);

		if (m_bAnd_Or)
		{
			if (*pObj != *m_arFilterValues[j])
			{
				bSkip = TRUE;
				break;
			}
		}
		else
		{
			if (*pObj == *m_arFilterValues[j])
			{
				bSkip = FALSE;
				break;
			}
		}

	}
	return bSkip ? FALSE : TRUE;
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemFilter::CheckStrings(SqlRecord* pRec)
{
	ASSERT(m_eFilterType == FILTER_STRINGS);
	ASSERT(m_arColumnsDataIndex.GetSize() == 1);

	return pRec->CompareFieldBy(m_arColumnsDataIndex[0], m_eCmp, m_arFilterStrValues, m_pCmpObj, TRUE, m_pCtrl);
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemFilter::CheckColumnData(SqlRecord* pRec)
{
	ASSERT(m_eFilterType == FILTER_COLUMNDATA);
	ASSERT(m_arColumnsDataIndex.GetSize() == 1);

	return pRec->CompareFieldBy(m_arColumnsDataIndex[0], m_eCmp, m_pCmpObj, TRUE);
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemFilter::CheckExpr(SqlRecord* pRec)
{
	ASSERT(m_eFilterType == FILTER_EXPR);

	DataBool check;
	if (!m_pExpr->Eval(check))
	{
		ASSERT_TRACE1(FALSE, "DBTSlaveBuffered::MemoryFilter fails to eval %s\n", m_pExpr->GetErrDescription());
		return TRUE;
	}
	return check;
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemFilter::CheckMethod(SqlRecord* pRec)
{
	return m_pDBT->CheckFilterMethod(pRec);
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemFilter::Check(SqlRecord* pRec)
{
	switch (m_eFilterType)
	{
	case FILTER_DATAOBJS:
		return CheckDataObjs(pRec);
	case FILTER_STRINGS:
		return CheckStrings(pRec);
	case FILTER_EXPR:
		return CheckExpr(pRec);
	case FILTER_METHOD:
		return CheckMethod(pRec);
	case FILTER_TOP:
		return m_pDBT->m_pRecords->GetSize() < m_nTop;
	case FILTER_COLUMNDATA:
		return CheckColumnData(pRec);
	default:
		ASSERT(FALSE);
	}
	return TRUE;	//non filtra
}

//--------------------------------------------------------------------------
void DBTSlaveBuffered::ApplyMemoryFilters()
{
	//NO! ClearSlaveDBTs();
	RemoveAllRecords(); m_nCurrentRow = -1;

	for (int r = 0; r <= m_pAllRecords->GetUpperBound(); r++)
	{
		m_nCurrentRow = r;	//Fondamentale per la risoluzione del valore della colonna nella riga corrente
		//effettuata dal metodo virtuale GetData
		SqlRecord* pRec = m_pAllRecords->GetAt(r);

		//----
		BOOL bCheck = TRUE;
		for (int i = 0; i < m_arMemFilters.GetSize(); i++)
		{
			MemFilter* filter = (MemFilter*)m_arMemFilters[i];

			bCheck = filter->Check(pRec);
			if (!bCheck)
			{
				if (filter->m_eFilterType == MemFilter::EMemFilterType::FILTER_TOP)
					return;
				break;
			}
		}
		if (!bCheck)
			continue;
		//----

		m_pRecords->Add(pRec);
	}

	m_nCurrentRow = m_pRecords->GetUpperBound();
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemoryFilter(BOOL bRefreshBody/* = TRUE*/, ERemoveMemFilter eRemoveFilters/* = REMOVE_FILTER_ALL*/, int nTop)
{
	if (!m_bAllowFilter) return FALSE;

	if (m_pAllRecords->GetSize() == 0)
		return TRUE;

	//----
	RemoveMemoryFilterAux(FALSE, eRemoveFilters);

	if (nTop > 0)
		m_arMemFilters.Add(new MemFilter(this, nTop));
	else
		m_arMemFilters.Add(new MemFilter(this));

	ApplyMemoryFilters();
	//----

	if (!bRefreshBody && m_pRecords->GetSize())
		SetCurrentRow(0);

	AlignBodyEdits(bRefreshBody);

	return TRUE;
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemoryFilter(const CString& sExpr, SymTable* parent/*=NULL*/, BOOL bRefreshBody/* = TRUE*/, ERemoveMemFilter eRemoveFilters)
{
	if (!m_bAllowFilter) return FALSE;

	if (m_pAllRecords->GetSize() == 0)
		return TRUE;

	SymTable* pTable = new SymTable;
	if (!PrepareSymbolTable(pTable))
		return FALSE;
	pTable->SetParent(parent);

	Parser parser(sExpr);
	Expression* pExprFilter = new Expression(pTable);
	if (!pExprFilter->Parse(parser, DataType::Bool))
	{
		ASSERT_TRACE1(FALSE, "DBTSlaveBuffered::MemoryFilter fails to parse %s\n", pExprFilter->GetErrDescription());
		return FALSE;
	}

	//----
	RemoveMemoryFilterAux(FALSE, eRemoveFilters);

	m_arMemFilters.Add(new MemFilter(this, pExprFilter, pTable));

	ApplyMemoryFilters();
	//----

	if (!bRefreshBody && m_pRecords->GetSize())
		SetCurrentRow(0);

	AlignBodyEdits(bRefreshBody);

	return TRUE;
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemoryFilter(const CString& sColumnName, DataObj* filterValue, BOOL bRefreshBody/* = TRUE*/, ERemoveMemFilter eRemoveFilters)
{
	CStringArray arColumnName; arColumnName.Add(sColumnName);
	DataObjArray arFilterValue; arFilterValue.SetOwns(FALSE);
	arFilterValue.Add(filterValue);

	return MemoryFilter(arColumnName, arFilterValue, /*bAnd_Or*/ TRUE, bRefreshBody, eRemoveFilters);
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemoryFilter(const CString& sColumnName, const DataObjArray& arFilterValue, BOOL bRefreshBody/* = TRUE*/, ERemoveMemFilter eRemoveFilters)
{
	CStringArray arColumnName;
	for (int i = 0; i < arFilterValue.GetSize(); i++)
		arColumnName.Add(sColumnName);

	return MemoryFilter(arColumnName, arFilterValue, FALSE, bRefreshBody, eRemoveFilters);
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemoryFilter(const CStringArray& arColumnName, const DataObjArray& arFilterValue, BOOL bAnd_Or/* = TRUE*/, BOOL bRefreshBody/* = TRUE*/, ERemoveMemFilter eRemoveFilters)
{
	if (!m_bAllowFilter)
		return FALSE;

	if (m_pAllRecords->GetSize() == 0)
		return TRUE;

	SqlRecord* pRec = m_pAllRecords->GetAt(0);
	if (pRec == NULL)
		return FALSE;

	if (arColumnName.GetSize() == 0)
		return FALSE;

	if (arColumnName.GetSize() != arFilterValue.GetSize())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CArray<int> arColumnDataIndex;
	for (int i = 0; i < arColumnName.GetSize(); i++)
	{
		int nFieldIndex = pRec->GetIndexFromColumnName(arColumnName[i]);
		if (nFieldIndex < 0)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		DataObj* pObj = pRec->GetDataObjAt(nFieldIndex);
		if (pObj == NULL)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		if (!DataType::IsCompatible(pObj->GetDataType(), arFilterValue[i]->GetDataType()))
		{
			ASSERT(FALSE);
			return FALSE;
		}

		arColumnDataIndex.Add(nFieldIndex);
	}

	return MemoryFilter(arColumnDataIndex, arFilterValue, bAnd_Or, bRefreshBody, eRemoveFilters);
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemoryFilter(const CArray<int>& arColumnDataIndex, const DataObjArray& arFilterValue, BOOL bAnd_Or/* = TRUE*/, BOOL bRefreshBody/* = TRUE*/, ERemoveMemFilter eRemoveFilters)
{
	if (!m_bAllowFilter)
		return FALSE;
	if (m_pAllRecords->GetSize() == 0)
		return TRUE;

	//----
	RemoveMemoryFilterAux(FALSE, eRemoveFilters);

	m_arMemFilters.Add(new MemFilter(this, arColumnDataIndex, arFilterValue, bAnd_Or));

	ApplyMemoryFilters();
	//----

	if (!bRefreshBody && m_pRecords->GetSize())
		SetCurrentRow(0);

	AlignBodyEdits(bRefreshBody);
	return TRUE;
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemoryFilter(int nFieldIndex, CString sFilterValue, ECompareType cmp/* = CMP_EQUAL*/, CParsedCtrl* pCtrl/* = NULL*/, BOOL bRefreshBody/* = TRUE*/, ERemoveMemFilter eRemoveFilters)
{
	CStringArray arFilterValues; arFilterValues.Add(sFilterValue);

	return MemoryFilter(nFieldIndex, arFilterValues, cmp, pCtrl, bRefreshBody, eRemoveFilters);
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemoryFilter(const CString& sColumnName, DataObj* filterValue, ECompareType cmp, BOOL bRefreshBody/* = TRUE*/, ERemoveMemFilter eRemoveFilters/* = REMOVE_FILTER_ALL*/)
{
	if (!m_bAllowFilter)
		return FALSE;

	if (m_pAllRecords->GetSize() == 0)
		return TRUE;

	SqlRecord* pRec = m_pAllRecords->GetAt(0);
	if (pRec == NULL)
		return FALSE;

	int nFieldIndex = pRec->GetIndexFromColumnName(sColumnName);
	if (nFieldIndex < 0)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	//----
	RemoveMemoryFilterAux(FALSE, eRemoveFilters);

	m_arMemFilters.Add(new MemFilter(this, nFieldIndex, filterValue->DataObjClone(), cmp));

	ApplyMemoryFilters();
	//----

	AlignBodyEdits(bRefreshBody);

	return TRUE;
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::MemoryFilter(int nFieldIndex, const CStringArray& arFilterStrValues, ECompareType eCmp/*= CMP_EQUAL*/, CParsedCtrl* pCtrl/* = NULL*/, BOOL bRefreshBody/* = TRUE*/, ERemoveMemFilter eRemoveFilters)
{
	if (!m_bAllowFilter)
		return FALSE;

	if (nFieldIndex < 0)
		return FALSE;

	if (m_pAllRecords->GetSize() == 0)
		return TRUE;

	SqlRecord* pRec = m_pAllRecords->GetAt(0);
	ASSERT_VALID(pRec);
	if (pRec == NULL)
		return FALSE;

	DataObj* pObj = pRec->GetDataObjAt(nFieldIndex);
	ASSERT_VALID(pObj);
	if (pObj == NULL)
		return FALSE;

	//----
	RemoveMemoryFilterAux(FALSE, eRemoveFilters);

	m_arMemFilters.Add(new MemFilter(this, nFieldIndex, arFilterStrValues, eCmp, pCtrl, pCtrl ? new DataStr() : pObj->DataObjClone()));

	ApplyMemoryFilters();
	//----

	AlignBodyEdits(TRUE);

	return TRUE;
}

//--------------------------------------------------------------------------
void DBTSlaveBuffered::RemoveMemoryFilterAux(BOOL bRefreshBody/* = TRUE*/, ERemoveMemFilter eRemoveFilters/* = REMOVE_FILTER_ALL*/, int nColumnDataIdx/* = -1*/)
{
	int nRemovedFilter = 0;
	if (!m_arMemFilters.GetSize())
		return;

	switch (eRemoveFilters)
	{
	case ERemoveMemFilter::REMOVE_FILTER_ALL:
		nRemovedFilter = m_arMemFilters.GetSize();
		m_arMemFilters.RemoveAll();
		break;

	case ERemoveMemFilter::REMOVE_FILTER_UI:
		for (int i = m_arMemFilters.GetUpperBound(); i >= 0; i--)
		{
			MemFilter* filter = (MemFilter*)m_arMemFilters[i];
			if (filter->m_bUI && (nColumnDataIdx < 0 || nColumnDataIdx == filter->m_arColumnsDataIndex[0]))
			{
				nRemovedFilter++;
				m_arMemFilters.RemoveAt(i);
			}
		}
		break;

	case ERemoveMemFilter::REMOVE_FILTER_NOT_UI:
		for (int i = m_arMemFilters.GetUpperBound(); i >= 0; i--)
		{
			MemFilter* filter = (MemFilter*)m_arMemFilters[i];
			if (!filter->m_bUI && (nColumnDataIdx < 0 || nColumnDataIdx == filter->m_arColumnsDataIndex[0]))
			{
				nRemovedFilter++;
				m_arMemFilters.RemoveAt(i);
			}
		}
		break;

	case ERemoveMemFilter::REMOVE_FILTER_NONE:
		return;
	}

	if (!nRemovedFilter)
		return;

	CBodyEditPointers* parBodies = GetBodyEdits();
	if (parBodies)
	{
		for (int i = 0; i < parBodies->GetCount(); i++)
		{
			CBodyEdit *pBody = parBodies->GetPointerAt(i);
			if (!pBody || !pBody->m_hWnd)
				continue;

			pBody->m_nCurrRecordIdx = -1;

			pBody->ResetSearchPoint(TRUE);	//sono cambiate le righe
		}
	}
}

//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::RemoveMemoryFilter(BOOL bRefreshBody/* = TRUE*/, ERemoveMemFilter eRemoveFilters/* = REMOVE_FILTER_ALL*/, int nColumnDataIdx/* = -1*/)
{
	RemoveMemoryFilterAux(bRefreshBody, eRemoveFilters, nColumnDataIdx);

	if (!m_bAllowFilter)
		return FALSE;
	ASSERT_VALID(m_pAllRecords);
	if (!m_pAllRecords)
		return FALSE;

	if (m_arMemFilters.GetSize())
	{
		ApplyMemoryFilters();
	}
	else
	{
		//NO! ClearSlaveDBTs();
		RemoveAllRecords(); m_nCurrentRow = -1;

		m_pRecords->Append(*m_pAllRecords);
	}
	//----

	if (!bRefreshBody && m_pRecords->GetSize())
		SetCurrentRow(0);

	AlignBodyEdits(bRefreshBody);
	return TRUE;
}

//-----------------------------------------------------------------------------
int DBTSlaveBuffered::RemapIndexA2F(const int nRow)
{
	if (!m_bAllowFilter || !m_pAllRecords)
		return nRow;

	ASSERT(m_pRecords != m_pAllRecords);

	if (nRow <= 0 || nRow > m_pAllRecords->GetUpperBound())
		return nRow;

	// leggo il puntatore al record 
	SqlRecord*	pFindRow = m_pAllRecords->GetAt(nRow);
	int	nIdx = -1;

	// cerco il record nell'array filtrato...
	for (int c = 0; c <= m_pRecords->GetUpperBound(); c++)
	{
		// lo trovo ??
		if (m_pRecords->GetAt(c) == pFindRow)
		{
			// rimappo la nRow sull'indice 'filtrato'
			nIdx = c;
			break;
		}
	}

	//ASSERT(nIdx != -1);
	return nIdx;
}

//-----------------------------------------------------------------------------
int DBTSlaveBuffered::RemapIndexF2A(const int nRow)
{
	if (!m_bAllowFilter || !m_pAllRecords)
		return nRow;

	ASSERT(m_pRecords != m_pAllRecords);

	ASSERT(nRow >= -1 && nRow <= m_pRecords->GetSize());
	if (nRow < 0 || nRow > m_pRecords->GetUpperBound())
		return m_pRecords->GetUpperBound() < 0 ? -1 : nRow;

	// leggo il puntatore al record 
	SqlRecord*	pFindRow = m_pRecords->GetAt(nRow);
	int	nIdx = -1;

	// cerco il record nell'array completo...
	for (int c = 0; c <= m_pAllRecords->GetUpperBound(); c++)
	{
		if (m_pAllRecords->GetAt(c) == pFindRow)
		{
			// rimappo la nRow sull'indice 'vero'
			nIdx = c;
			break;
		}
	}

	ASSERT(nIdx != -1);
	return nIdx;
}

//-----------------------------------------------------------------------------
void DBTSlaveBuffered::PrepareAllPrimaryKeys(int startRow/*=0*/)
{
	RecordArray* pRecords = m_pAllRecords ? m_pAllRecords : m_pRecords;
	for (int c = startRow; c <= pRecords->GetUpperBound(); c++)
	{
		OnPreparePrimaryKey(c, pRecords->GetAt(c));

		GetClientDocs()->OnPreparePrimaryKey(this, c, pRecords->GetAt(c));
	}
}

//-----------------------------------------------------------------------------	
DataObj* DBTSlaveBuffered::OnCheckPrimaryKey(int nRow, SqlRecord* pRecord)
{
	// se ho un overload lo chiamo, altrimenti eseguo l'algoritmo di default
	if (m_pFnOnCheckPrimaryKey)
		return (m_pFnOnCheckPrimaryKey(m_pRecord, nRow));

	DataObj* pDataObj = CheckPrimaryKey(pRecord);
	if (pDataObj)
		return pDataObj;

	return NULL;
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::SetDuplicateKeyFunPtr(DATAOBJ_ROW_FUNC funPtr)
{
	m_pFnDuplicateKey = funPtr;
}

//-----------------------------------------------------------------------------
void DBTSlaveBuffered::OnPreparePrimaryKey(int nRow, SqlRecord* pRec)
{
}
//-----------------------------------------------------------------------------
void DBTSlaveBuffered::OnPrepareDynamicColumns(SqlRecord* pRec)
{
}
//-----------------------------------------------------------------------------
void DBTSlaveBuffered::OnPrepareAuxColumns(SqlRecord* pRec)
{
}

//-----------------------------------------------------------------------------
void DBTSlaveBuffered::OnRecordAdded(SqlRecord* pRec, int nRow)
{
}

//-----------------------------------------------------------------------------
void DBTSlaveBuffered::OnPrepareOldAuxColumns(SqlRecord* pRec)
{
}

//-----------------------------------------------------------------------------
void DBTSlaveBuffered::OnSetCurrentRow()
{

}

//-----------------------------------------------------------------------------
void DBTSlaveBuffered::OnPrepareRow(int nRow, SqlRecord* pRec)
{
}

/* chiamata prima dell'aggiunta in fondo di una nuova riga */
//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::OnBeforeAddRow(int nRow)
{
	return TRUE;
}

/* chiamata dopo l'aggiunta in fondo di una nuova riga */
//-----------------------------------------------------------------------------
void DBTSlaveBuffered::OnAfterAddRow(int nRow, SqlRecord* pRec)
{
}

/* chiamata prima del (re)inserimento tra due righe */
//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::OnBeforeInsertRow(int nRow)
{
	return TRUE;
}

/* chiamata dopo l'inserimento tra due righe */
//-----------------------------------------------------------------------------
void DBTSlaveBuffered::OnAfterInsertRow(int nRow, SqlRecord* pRec)
{
}

/* chiamata prima di cancellare la riga */
//--------------------------------------------------------------------------
BOOL DBTSlaveBuffered::OnBeforeDeleteRow(int nRow)
{
	return TRUE;
}

/* chiamata dopo la rimozione effettiva della riga*/
//-----------------------------------------------------------------------------
void DBTSlaveBuffered::OnAfterDeleteRow(int nRow)
{
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::ReNumberColumn(DataObj* pObj, int startRow/* = 0*/)
{
	int nDataIdx = GetRecord()->GetIndexFromDataObj(pObj);
	ReNumberColumn(nDataIdx, startRow);
}

void DBTSlaveBuffered::ReNumberColumn(int nDataIdx, int startRow/* = 0*/)
{
	if (nDataIdx < 0)
	{
		ASSERT_TRACE(FALSE, "DBTSlaveBuffered::ReNumberColumn called on unknown column");
		return;
	}

#ifdef _DEBUG
	DataObj* pObj = GetRecord()->GetDataObjAt(nDataIdx);
	ASSERT_VALID(pObj);
	if (!pObj->IsKindOf(RUNTIME_CLASS(DataInt)) && !pObj->IsKindOf(RUNTIME_CLASS(DataLng)))
	{
		ASSERT_TRACE(FALSE, "DBTSlaveBuffered::ReNumberColumn called on column with wrong data type");
		return;
	}
#endif

	RecordArray* pRecords = m_pAllRecords ? m_pAllRecords : m_pRecords;
	for (int c = startRow; c < pRecords->GetUpperBound(); c++)
	{
		DataObj* po = (*pRecords)[c]->GetDataObjAt(nDataIdx);

		if (po->IsKindOf(RUNTIME_CLASS(DataInt)))
			((DataInt*)po)->Assign(c);
		else
			((DataLng*)po)->Assign(c);
	}
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::IsUniqueColumn(DataObj* pObj, int startRow/* = 0*/, int* r1/* = NULL*/, int* r2/* = NULL*/) const
{
	int nDataIdx = GetRecord()->GetIndexFromDataObj(pObj);
	return IsUniqueColumn(nDataIdx, startRow, r1, r2);
}

BOOL DBTSlaveBuffered::IsUniqueColumn(int nDataIdx, int startRow/* = 0*/, int* r1/* = NULL*/, int* r2/* = NULL*/) const
{
	if (nDataIdx < 0)
	{
		ASSERT_TRACE(FALSE, "DBTSlaveBuffered::IsUniqueColumn called on unknown column");
		return TRUE;
	}

	RecordArray* pRecords = m_pAllRecords ? m_pAllRecords : m_pRecords;
	for (int c = startRow; c < pRecords->GetUpperBound(); c++)
	{
		DataObj* p1 = (*pRecords)[c]->GetDataObjAt(nDataIdx);
		for (int j = c + 1; j < pRecords->GetSize(); j++)
		{
			if (p1->IsEqual(*(*pRecords)[j]->GetDataObjAt(nDataIdx)))
			{
				if (r1) *r1 = c;
				if (r2) *r2 = j;
				return FALSE;
			}
		}
	}
	return TRUE;
}


//-----------------------------------------------------------------------------	
DBTSlaveBuffered* DBTSlaveBuffered::GetActiveSibling()
{
	const DBTObject* pMaster = GetMaster();
	if (!pMaster || !pMaster->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		return this;
	DBTSlaveBuffered* pParent = (DBTSlaveBuffered*)pMaster;

	DBTSlaveBuffered* pUncle = pParent->GetActiveSibling();
	if (!pUncle)
		return NULL;
	DBTSlaveMap* pData = pUncle->GetDBTSlaveData(GetNamespace().GetObjectName());
	DBTSlaveBuffered* pActive = (DBTSlaveBuffered*)(pData ? pData->m_pCurrentDBTSlave.operator DBTSlave *() : NULL);
	if (pActive && pActive == pData->m_pDBTSlavePrototype)
		return NULL;
	return pActive;
}
//-----------------------------------------------------------------------------	
DBTSlave* DBTSlaveBuffered::GetMainPrototype()
{
	//ritorno il prototipo del dbt corrente.
	//esiste un dbt prototipo per ogni istanza del dbt master
	//bisogna ricordare quindi che se i dbt sono su tre livelli, per il terzo livello esistono più dbt prototipo
	//quello che conta è il primo, associato al prototipo del parent
	//quindi devo risalire la catena ricorsivamente in questo modo

	// 1 - prendo il master buffered
	const DBTObject* pMaster = GetMaster();
	if (!pMaster || !pMaster->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		return NULL;
	DBTSlaveBuffered* pFather = (DBTSlaveBuffered*)pMaster;
	// 2 - vedo se ad esso è associato un prototipo
	DBTSlaveBuffered* pUncleOrFather = (DBTSlaveBuffered*)pFather->GetMainPrototype();
	// 3 - se non c'è, vuol dire che il parent è il nodo radice, uso lui stesso come ancestor
	if (!pUncleOrFather)
		pUncleOrFather = pFather;

	//chiedo al mio parente di darmi i dati relativi a me
	DBTSlaveMap* pData = pUncleOrFather->GetDBTSlaveData(GetNamespace().GetObjectName());
	//ritorno il prototipo
	return pData ? pData->m_pDBTSlavePrototype : NULL;
}
//-----------------------------------------------------------------------------	
CBodyEditPointers* DBTSlaveBuffered::GetBodyEdits()
{
	//se non sono slave di slave buffered, allora detengo la lista dei bodyedit
	if (!m_pDBTMaster || !m_pDBTMaster->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		return &m_arBodyPtr;
	//altrimenti, siccome ho una serie di istanze di dbt, la lista è detenuta dal main prototype
	DBTSlaveBuffered* pProt = (DBTSlaveBuffered*)GetMainPrototype();
	return pProt ? &pProt->m_arBodyPtr : &m_arBodyPtr;
}
//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::AddBodyEdit(CBodyEdit* pBody)
{
	CBodyEditPointers* parBodies = GetBodyEdits();
	if (parBodies)
	{
		for (int i = 0; i < parBodies->GetCount(); i++)
			if (parBodies->GetPointerAt(i) == pBody)
				return;
		parBodies->AddPointer(pBody);
	}
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::Attach(DBTSlave* pDBTSlave)
{
	ASSERT(pDBTSlave);
	ASSERT(pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlave)));

	DBTSlaveMap* pData = new DBTSlaveMap();
	m_DBTSlaveData.Add(pData);

	pData->m_pCurrentDBTSlave = pDBTSlave;
	pData->m_pDBTSlavePrototype = pDBTSlave;
	pDBTSlave->m_pDBTMaster = this;
	pDBTSlave->m_pMasterRecord = this->GetRecord();
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::OnDefineQuery()
{
	if (!m_pDBTMaster || !m_pTable || !m_pDBTMaster->GetRecord())
	{
		ASSERT(FALSE);
		TRACE1("DBTSlaveBuffered::OnDefineQuery: missing DBTMaster record or m_pTable in DBT %s", GetNamespace().ToString());
		return;
	}
}

//-----------------------------------------------------------------------------	
void DBTSlaveBuffered::OnPrepareQuery()
{
	if (!m_pDBTMaster || !m_pTable || !m_pDBTMaster->GetRecord())
	{
		ASSERT(FALSE);
		TRACE1("DBTSlaveBuffered::OnPrepareQuery: missing DBTMaster record or m_pTable in DBT %s", GetNamespace().ToString());
		return;
	}
}

//-----------------------------------------------------------------------------	
int DBTSlaveBuffered::FindRecordIndex(const CString& sColumnName, const DataObj* pValue, int nStartPos/* = 0*/) const
{
	return m_pRecords->FindRecordIndex(sColumnName, pValue, nStartPos);
}

//-----------------------------------------------------------------------------	
SqlRecord* DBTSlaveBuffered::FindRecord(const CString& sColumnName, DataObj* pValue, int nStartPos/* = 0*/)
{
	return m_pRecords->FindRecord(sColumnName, pValue, nStartPos);
}

//-----------------------------------------------------------------------------	
SqlRecord* DBTSlaveBuffered::FindRecord(const CStringArray& arColumnName, const DataObjArray& arFilterValue, int nStartPos/*= 0*/)
{
	return m_pRecords->FindRecord(arColumnName, arFilterValue, nStartPos);
}

//-----------------------------------------------------------------------------	
BOOL DBTSlaveBuffered::Parse(const CString& filename, BOOL bWithAttributes/* = TRUE*/, BOOL bParseLocal/* = FALSE*/)
{
	ASSERT_VALID(m_pRecords);
	ASSERT_VALID(m_pRecord);
	return m_pRecords->Parse(this->m_pRecord->GetRuntimeClass(), filename, bWithAttributes, bParseLocal);
}

BOOL DBTSlaveBuffered::Parse(CXMLNode* pParentNode, BOOL bWithAttributes/* = TRUE*/, BOOL bParseLocal/* = FALSE*/)
{
	ASSERT_VALID(m_pRecords);
	ASSERT_VALID(m_pRecord);
	return m_pRecords->Parse(this->m_pRecord->GetRuntimeClass(), pParentNode, bWithAttributes, bParseLocal);
}

BOOL DBTSlaveBuffered::UnParse(CXMLNode* pRootNode, BOOL bWithAttributes/* = TRUE*/, BOOL bUnParseLocal/* = FALSE*/, BOOL bUnParseSlavable/* = FALSE*/)
{
	ASSERT_VALID(m_pRecords);
	ASSERT_VALID(m_pRecord);
	ASSERT_VALID(pRootNode);

	CXMLNode* pParentNode = pRootNode->CreateNewChild(GetName());
	pParentNode->SetAttribute(L"count", cwsprintf(L"%d", m_pRecords->GetSize()));
	pParentNode->SetAttribute(L"class", (LPCTSTR)CString(this->GetRuntimeClass()->m_lpszClassName));
	pParentNode->SetAttribute(L"addrs", cwsprintf(L"%d", (long)this));

	if (!bUnParseSlavable || m_DBTSlaveData.GetCount() == 0)
		return m_pRecords->UnParse(pParentNode, bWithAttributes, bUnParseLocal);

	for (int i = 0; i <= m_pRecords->GetUpperBound(); i++)
	{
		SqlRecord* pRec = m_pRecords->GetAt(i);
		ASSERT_VALID(pRec);

		CXMLNode* pNodeRec = pParentNode->CreateNewChild(pRec->GetTableName());

		pRec->UnParse(pNodeRec, bWithAttributes, bUnParseLocal);

		for (int s = 0; s < m_DBTSlaveData.GetCount(); s++)
		{
			DBTSlaveMap* pData = m_DBTSlaveData.GetAt(s);

			DBTSlave* pSlave = GetDBTSlave(pRec, pData, FALSE);
			if (pSlave)
			{
				pSlave->UnParse(pNodeRec, bWithAttributes, bUnParseLocal, bUnParseSlavable);
			}
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------	

//[TBWebMethod(name = DBTSlaveBuffered_GetRow, thiscall_method=true)]
SqlRecord* DBTSlaveBuffered::TbScriptGetRow(DataLng row)
{
	return GetRow((long)row);
}

//[TBWebMethod(name = DBTSlaveBuffered_GetCurrentRow, thiscall_method=true)]
SqlRecord* DBTSlaveBuffered::TbScriptGetCurrentRow()
{
	return GetCurrentRow();
}

//[TBWebMethod(name = DBTSlaveBuffered_SetCurrentRow, thiscall_method=true)]
void DBTSlaveBuffered::TbScriptSetCurrentRow(DataLng row)
{
	SetCurrentRow((long)row);
}

//[TBWebMethod(name = DBTSlaveBuffered_GetCurrentRowIndex, thiscall_method=true)]
DataLng DBTSlaveBuffered::TbScriptGetCurrentRowIdx()
{
	return DataLng(GetCurrentRowIdx());
}

//[TBWebMethod(name = DBTSlaveBuffered_GetSize, thiscall_method=true)]
DataLng DBTSlaveBuffered::TbScriptGetSize()
{
	return DataLng(GetSize());
}

//[TBWebMethod(name = DBTSlaveBuffered_InsertRecord, thiscall_method=true)]
SqlRecord* DBTSlaveBuffered::TbScriptInsertRecord(DataLng row)
{
	return InsertRecord((long)row);
}

//[TBWebMethod(name = DBTSlaveBuffered_DeleteRecord, thiscall_method=true)]
void DBTSlaveBuffered::TbScriptDeleteRecord(DataLng row)
{
	DeleteRecord((long)row);
}

//[TBWebMethod(name = DBTSlaveBuffered_RemoveAll, thiscall_method=true)]
void DBTSlaveBuffered::TbScriptRemoveAll()
{
	RemoveAll();
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DBTSlaveBuffered::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDBTSlaveBuffered");

	DBTSlave::Dump(dc);
}

void DBTSlaveBuffered::AssertValid() const
{
	DBTSlave::AssertValid();
}
#endif //_DEBUG

DBTSlave* DBTSlaveMap::GetSiblingPrototype()
{
	const DBTObject* pMaster = m_pDBTSlavePrototype->GetMaster();
	if (!pMaster || !pMaster->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		return m_pDBTSlavePrototype;
	DBTSlaveBuffered* pParent = (DBTSlaveBuffered*)pMaster;

	DBTSlaveMap* pData = pParent->GetDBTSlaveData(m_pDBTSlavePrototype->GetNamespace().GetObjectName());
	if (pData == this)
		return m_pDBTSlavePrototype;
	return pData->GetSiblingPrototype();
}

/////////////////////////////////////////////////////////////////////////////

CBodyEditPointers::~CBodyEditPointers()
{
	for (int i = 0; i < GetCount(); i++)
	{
		delete GetAt(i);
	}
}

//-----------------------------------------------------------------------------	
CBodyEdit* CBodyEditPointers::GetPointerAt(int i)
{
	TDisposablePtr<CBodyEdit>* ps = GetAt(i);
	if (ps && *ps)
		return GetAt(i)->operator->();
	return NULL;
}
//-----------------------------------------------------------------------------	
void CBodyEditPointers::AddPointer(CBodyEdit* pEdit)
{
	Add(new TDisposablePtr<CBodyEdit>(pEdit));
}
