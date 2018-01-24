
#include "stdafx.h" 
#include <atldbcli.h> 

#include <TbGeneric\DataObj.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbNameSolver\TBNamespaces.h>

#include <TbOleDb\SqlRec.h>
#include <TbOleDb\SqlTable.h>
#include <TbOleDb\SqlCatalog.h>
#include <TbOleDb\SqlAccessor.h>
#include <TbOleDb\SqlObject.h>

#include <TbGes\DBT.h>
#include <TbGes\XMLGesInfo.h>

#include "AuditTables.h"
#include "AuditReportGenerator.h"
#include "SoapFunctions.h"

#include "AuditingManager.h"

static const TCHAR szLoginName[]		= _T("AU_LoginName");
static const TCHAR szOperationData[]	= _T("AU_OperationData");
static const TCHAR szOperationType[]	= _T("AU_OperationType");
static const TCHAR szNameSpaceID[]	= _T("AU_NameSpaceID");
static const TCHAR szAU_ID[]			= _T("AU_ID");



//============================================================================
//	general functions
//============================================================================

//-----------------------------------------------------------------------------
TB_EXPORT AuditingInterface* AFXAPI AfxGetAuditingInterface()
{ 
	return AfxGetLoginContext()->GetObject<AuditingInterface>(); 
}          

//-----------------------------------------------------------------------------
TB_EXPORT SqlConnection* AFXAPI AfxGetAuditingSqlConnection()
{
	return AfxGetAuditingInterface()->GetSqlConnection();
}

//-----------------------------------------------------------------------------
DataLng GetNextAutoincrementValue(SqlSession* pSqlSession, const CString& strTableName, const CString& strAutoIncCol)
{
	if (strAutoIncCol.IsEmpty() || !pSqlSession)
		return 0;

	SqlTable aTable(pSqlSession);		
	DataLng lMax = 0;
	TRY
	{
		//considero l'unica colonna autoincrementale del record
		aTable.Open();
		aTable.m_strSQL = cwsprintf( _T("SELECT MAX(%s) from %s"), strAutoIncCol, strTableName);
		aTable.Select(strAutoIncCol, &lMax, nEmptySqlRecIdx);
		aTable.Query();
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		if (aTable.IsOpen())
			aTable.Close();
		THROW_LAST();
	}
	END_CATCH

	return ++lMax;
}

///////////////////////////////////////////////////////////////////////////////
//								AuditingInterface declaration
///////////////////////////////////////////////////////////////////////////////
//

IMPLEMENT_DYNCREATE(AuditingInterface, CObject)

//-----------------------------------------------------------------------------
AuditingInterface::AuditingInterface()
:
	m_pSqlConnection	(NULL),
	m_pNSLookupMng		(NULL)
{
}

//-----------------------------------------------------------------------------
AuditingInterface::~AuditingInterface()
{
	CloseAuditing();
}

//-----------------------------------------------------------------------------
BOOL AuditingInterface::CloseAuditing()
{
	if (m_pNSLookupMng)
	{
		delete m_pNSLookupMng;
		m_pNSLookupMng = NULL;
	}

	return TRUE;
}

// questo metodo viene chiamato se il database é posto sotto gestione auditing
// e dopo la ONDSNChange
// sul cambio del database aziendale, le connessioni esistenti vengono tutte 
// chiuse
//-----------------------------------------------------------------------------------
BOOL AuditingInterface::OpenAuditing()
{
	if (AfxGetDefaultSqlConnection() == NULL)
		return FALSE;

	USES_CONVERSION;	
	if (m_pNSLookupMng)
	{
		delete m_pNSLookupMng;
		m_pNSLookupMng = NULL;
	}

	// é il login manager che mi fornisce la login 
	m_pSqlConnection = AfxGetOleDbMng()->GetNewConnection(AfxGetLoginInfos()->m_strNonProviderCompanyConnectionString);

	//carico la mappa di lookup per la gestione dei namespaces
	if (m_pSqlConnection)
		m_pNSLookupMng = new NamespacesLookupMng();

		
	//PERASSO: il codice che segue e` stato rimosso e spostato in void CLoginThread::PostLoginInitializations(), 
	//dove chiamo la RefreshTraces dopo la OpenAuditing

	// nel caso di registrazione "anticipata" delle tabelle scatenata da fattori esterni, "forzo" la tracciatura
	// leggendo il catalog della connessione primaria (fix anomalia 14646)
	/*CStringArray arTables;
	AfxGetDefaultSqlConnection()->GetCatalog()->GetRegisteredTableNames(arTables);
	for (int i = 0; i < arTables.GetCount(); i++)
		SetAuditingManager(arTables[i]);*/

	s_pfCallTraced = &::SetAuditManagerFunction;
	
	return m_pSqlConnection != NULL;
}

///////////////////////////////////////////////////////////////////////////////
//								AuditingManager declaration
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
AuditingManager::AuditingManager(LPCTSTR lpszTableName)
:
	m_pAuditRec		(NULL),
	m_pAuxiliaryRec	(NULL),
	m_pAuditTable	(NULL),
	m_pAuxTable		(NULL),
	m_bValid		(TRUE),
	m_bAuxQuery		(TRUE),
	m_strTableName  (lpszTableName)
{
}

//-----------------------------------------------------------------------------
AuditingManager::~AuditingManager()
{
	if (m_pAuditTable)
	{
		if (m_pAuditTable->IsOpen())
			m_pAuditTable->Close();
		delete m_pAuditTable;
	}
	if (m_pAuditRec)
		delete m_pAuditRec;
	
	if (m_pAuxTable)
	{
		if (m_pAuxTable->IsOpen())
			m_pAuxTable->Close();
		delete m_pAuxTable;
	}
	if (m_pAuxiliaryRec)
		delete m_pAuxiliaryRec;
}

//-----------------------------------------------------------------------------
TAuditingRecord* AuditingManager::GetAuditRec()
{
	if (!m_pAuditRec && m_bValid) 
		CreateAuditingRecord();

	return m_pAuditRec;
}

//-----------------------------------------------------------------------------
BOOL AuditingManager::CreateAuditingRecord	()
{
	TB_LOCK_FOR_WRITE ();

	CString strTable = m_strTableName;
	strTable = szAUDIT + strTable;

	if (AfxGetAuditingSqlConnection()->GetDBMSType() == DBMS_ORACLE && strTable.GetLength() > 30)
		strTable = strTable.Left(30);

	m_pAuditRec = new TAuditingRecord(m_strTableName, strTable);
	m_bValid = m_pAuditRec->IsValid();
	if (!m_bValid)
		return FALSE;
	
	return TRUE;
}

//verifica che nel record che si sta x inserire|modificare|cancellare siano stati
//bindati tutti i campi previsti dalla tracciatura. Per i campi non presenti viene
//fatto un bind al volo
//-----------------------------------------------------------------------------
void AuditingManager::BindTracedColumns(SqlTable* pTable)
{
	TB_LOCK_FOR_WRITE();

	// to instantiate and validate record if not yet
	TAuditingRecord* pAuditRec = GetAuditRec();  
	if (!pAuditRec || !m_bValid)
		return;

	// i campi variabili della tabella di tracciatura iniziano alla posizione
	// m_nStartVarFieldsPos
	for (int j = pAuditRec->m_nStartVarFieldsPos; j <= pAuditRec->GetUpperBound(); j++)
	{
		BOOL bFound = FALSE;
		CString sColumnName = pAuditRec->GetColumnName(j);

		DataObj* pData = pTable->GetRecord()->GetDataObjFromColumnName(sColumnName);
		if (!pData)
			continue;

		for (int i = 0; i <= pTable->m_pColumnArray->GetUpperBound(); i++)
		{
			// confronto tra puntatori per verificare se il campo e` selezionato
			if (pTable->m_pColumnArray->GetDataObjAt(i) == pData)
			{
				bFound = TRUE;
				break;
			}
		}

		if (!bFound)
			pTable->Select(pTable->GetRecord()->GetDataObjFromColumnName(sColumnName));
	}
}

//-----------------------------------------------------------------------------
DataObj* AuditingManager::FindValueForColumnName(const CString& strColumnName, SqlTable* pTable)
{
	if (!m_pAuxiliaryRec && GetAuditRec())
	{
		m_pAuxiliaryRec = AfxGetDefaultSqlConnection()->GetCatalogEntry(GetAuditRec()->GetTracedTableName())->CreateRecord();
		if (!m_pAuxiliaryRec)
			return NULL;
		m_pAuxiliaryRec->SetConnection(AfxGetAuditingSqlConnection());

		m_pAuxTable = new SqlTable(m_pAuxiliaryRec, AfxGetAuditingSqlConnection()->GetDefaultSqlSession());
		m_pAuxTable->Open();	
		m_pAuxTable->SelectAll();
				
		for (int nIdx = 0; nIdx <= m_pAuxiliaryRec->GetUpperBound(); nIdx++)
		{
			SqlRecordItem* pItem = m_pAuxiliaryRec->GetAt(nIdx);
			if (pItem && pItem->IsSpecial())
			{
				m_pAuxTable->AddFilterColumn(pItem->GetColumnName());
				m_pAuxTable->AddParam(pItem->GetColumnName(), *pItem->GetDataObj());
			}			
		}		
	}

	if (!m_pAuxTable || m_pAuxTable->m_pParamArray->GetSize() <= 0)
		return NULL;
	
	if (!m_bAuxQuery)
	{
		for (int nParam = 0; nParam <= m_pAuxTable->m_pParamArray->GetUpperBound(); nParam++)
		{	
			SqlBindingElem* pParam = m_pAuxTable->m_pParamArray->GetAt(nParam);		
			if (!pParam)
				return NULL;

			DataObj* pDataObj = pTable->m_pColumnArray->GetDataObj(m_pAuxTable->m_pParamArray->GetParamName(nParam));
			if (!pDataObj)
				return NULL;
			pParam->SetParamValue(*pDataObj);
		}
		m_pAuxTable->Query();
		m_bAuxQuery = TRUE;
	}

	return m_pAuxiliaryRec->GetDataObjFromColumnName(strColumnName);
}
//-----------------------------------------------------------------------------
void AuditingManager::AssignValues(int eType, SqlTable* pTable) 
{	
	m_pAuditTable->AddNew();
	
	for (int i = 0; i < GetAuditRec()->GetSize(); i++)
	{	
		CString strColumnName = GetAuditRec()->GetColumnName(i);
		DataObj* pAuditDataObj = GetAuditRec()->GetDataObjAt(i);
		// non considero la colonna szOperationData poiché nello schema della tabella 
		// al campo é stato assgnato il default GETDATE() that Returns the current system 
		// date and time in the Microsoft® SQL Server™ standard internal format for 
		// datetime values
		if (strColumnName.CompareNoCase(szOperationData) == 0)
			continue;

		if (strColumnName.CompareNoCase(szAU_ID) == 0)
		{
			DataLng aAutoInc(GetNextAutoincrementValue(pTable->m_pSqlSession, GetAuditRec()->GetTableName(), szAU_ID));
			((DataLng*)pAuditDataObj)->Assign(aAutoInc);
			continue;
		}

		if (strColumnName.CompareNoCase(szOperationType) == 0)
		{
			((DataInt*)pAuditDataObj)->Assign(eType);
			continue;
		}

		if (strColumnName.CompareNoCase(szLoginName) == 0)
		{
			((DataStr*)pAuditDataObj)->Assign(AfxGetLoginInfos()->m_strUserName);
			continue;
		}

		if (strColumnName.CompareNoCase(szNameSpaceID) == 0)
		{
			CTBNamespace* pNameSpace = (pTable->m_pSqlSession->m_pContext && pTable->m_pSqlSession->m_pContext->GetDocument()) 
									? &pTable->m_pSqlSession->m_pContext->GetDocument()->GetNamespace() 
									: NULL;
			((DataLng*)pAuditDataObj)->Assign((pNameSpace && pNameSpace->IsValid()) ? AfxGetAuditingInterface()->GetNamespaceID(pNameSpace) : 0);
			continue;
		}
		// cerco la colonna nelle colonne di cui ho fatto il binding (che sono sicuramente
		// i campi chiave e quelli inseriti sotto tracciatura)
		// per l'operazione di modifica inserisco i valori dei dati prima della modifica
		// per l'operazione di inserimento e cancellazione quelli attuali
		// per l'operazione di cambio chiave traccio prima i valori dei dati prima della modifica e poi quelli dopo
		
		DataObj* pRecDataObj = NULL;	 
		if (eType == AUDIT_UPDATE_OP || eType == AUDIT_DELETE_OP)
		{
			pRecDataObj = pTable->GetOldDataObj(strColumnName);
			if (!pRecDataObj && eType == AUDIT_UPDATE_OP)
				pRecDataObj = FindValueForColumnName(strColumnName, pTable);
		}
		else
			pRecDataObj = pTable->GetDataObj(strColumnName);

		if (pRecDataObj)
			pAuditDataObj->Assign(*pRecDataObj); 
		else
			pAuditDataObj->Clear();
	}
}

//-----------------------------------------------------------------------------
void AuditingManager::TraceOperation(int eType, SqlTable* pTable)
{
	if (
			!GetAuditRec() || // to instantiate and validate record if not yet
			!m_bValid || 
			!pTable ||
			!pTable->m_pSqlSession || !pTable->m_pSqlSession->GetSession()
		)
		return;


	//devo essere sullo stesso database su cui é stato attivato il sistema di auditing
	SqlConnection* pSqlConnection = pTable->m_pSqlConnection;
	if (
			!pSqlConnection	||
			pSqlConnection->GetDatabaseName().CompareNoCase(AfxGetAuditingSqlConnection()->GetDatabaseName()) ||
			pSqlConnection->GetDatabaseServerName().CompareNoCase(AfxGetAuditingSqlConnection()->GetDatabaseServerName()) ||
			pSqlConnection->GetDbmsName().CompareNoCase(AfxGetAuditingSqlConnection()->GetDbmsName())
		)
		return;	


	if (m_pAuditTable && (m_pAuditTable->m_pSqlSession != pTable->m_pSqlSession || !m_pAuditTable->IsOpen())) //bugfix #19508
	{
		if (m_pAuditTable->IsOpen())
			m_pAuditTable->Close();
		delete m_pAuditTable;
		m_pAuditTable =  NULL;
	}

	TRY
	{
		if (!m_pAuditTable)
		{
			m_pAuditTable = new SqlTable(GetAuditRec(), pTable->m_pSqlSession);
			m_pAuditTable->Open(TRUE);
		
		
			for (int i = 0; i < GetAuditRec()->GetSize(); i++)
			{
				// non considero la colonna szOperationData poiché nello schema della tabella 
				// al campo é stato assgnato il default GETDATE() that Returns the current system 
				// date and time in the Microsoft® SQL Server™ standard internal format for 
				// datetime values
				// per ORACLE invece si considera la funzione
				if (GetAuditRec()->GetColumnName(i).CompareNoCase(szOperationData) == 0)
					continue;
				m_pAuditTable->Select(GetAuditRec()->GetDataObjAt(i));
			}
		}
		
			
		m_pAuditTable->SetAutocommit(pTable->IsAutocommit());

		// inserisco l'operazione di tracciatura considerando i vecchi valori del record
		// nel caso di operazione di update e delete, i nuovi nel caso di insert 		
		AssignValues(eType, pTable);
		m_pAuditTable->Update();
		// se sono cambiati i campi chiave traccio prima l'operazione di 
		// di aggiornamento considerando i vecchi valori del record e 
		// successivamente l'operazione di cambio chiave considerando i nuovi valori
		if (eType == AUDIT_UPDATE_OP && pTable->m_bKeyChanged)
		{
			AssignValues(AUDIT_CHANGEKEY_OP, pTable);
			m_pAuditTable->Update();		
		}
	}

	CATCH(SqlException, e)
	{
		pTable->m_pSqlSession->AddMessage(e->m_strError, CDiagnostic::Info);
		if (m_pAuditTable && m_pAuditTable->IsOpen())
			m_pAuditTable->Close();
		delete m_pAuditTable;
		m_pAuditTable =  NULL;
		return;					
	}
	END_CATCH
}


//viene chiamata da XTech per effettuare l'esportazione dei documenti inseriti, cancellati e modificati da data a data
//-----------------------------------------------------------------------------
void AuditingManager::PrepareQuery
						(
							SqlTable*	pTable, 
							DataDate&	aDataFrom, 
							DataDate&	aDataTo, 
							int			eType
						)
{
	TB_LOCK_FOR_WRITE();

	if (
			!GetAuditRec() || // to instantiate and validate record if not yet
			!m_bValid ||
			!pTable || 
			!pTable->GetRecord() || 
			pTable->GetTableName().CompareNoCase(GetAuditRec()->m_strTracedTable)
		)
		return;

	GetAuditRec()->SetQualifier(_T("AU")); 
	pTable->FromTable(GetAuditRec()); 

	SqlRecord* pRecord = pTable->GetRecord();
	// inserisco nella where clause la join con i campi chiave della tabella posta
	// sotto tracciatura
	CString strKeySegment;
	int nPos = 0;	
	for (int i = 0; i < pRecord->GetTableInfo()->GetSqlUniqueColumns()->GetSize(); i++)
	{
		strKeySegment = pRecord->GetTableInfo()->GetSqlUniqueColumns()->GetAt(i);
		nPos = GetAuditRec()->Lookup(strKeySegment);
		if (nPos < 0)
			continue;
		if (!pTable->m_strFilter.IsEmpty())
			pTable->m_strFilter += _T(" AND ");
		pTable->m_strFilter += cwsprintf(
											_T("%s = %s"),
											pRecord->GetQualifiedColumnName(pRecord->Lookup(strKeySegment)),
											GetAuditRec()->GetQualifiedColumnName(nPos)
										);
	}

	//inserisco le date e il tipo di operazione scelta
	pTable->m_strFilter += cwsprintf(
										_T(" AND %s >= %s AND %s <= %s AND ("),
										GetAuditRec()->GetQualifiedColumnName(&GetAuditRec()->f_OperData),
										pTable->m_pSqlConnection->NativeConvert(&aDataFrom),
										GetAuditRec()->GetQualifiedColumnName(&GetAuditRec()->f_OperData),
										pTable->m_pSqlConnection->NativeConvert(&aDataTo)
									);
	CString strOperTypeFilter;
	if ((eType & AUDIT_INSERT_OP) == AUDIT_INSERT_OP)
		strOperTypeFilter += cwsprintf(	_T("%s = %s"),
										GetAuditRec()->GetQualifiedColumnName(&GetAuditRec()->f_OperType),
										pTable->m_pSqlConnection->NativeConvert(&DataInt(AUDIT_INSERT_OP))									
									  );
	if ((eType & AUDIT_UPDATE_OP) == AUDIT_UPDATE_OP)
	{
		if (!strOperTypeFilter.IsEmpty())
			strOperTypeFilter += _T(" OR ");
		strOperTypeFilter += cwsprintf(	_T("%s = %s"),
										GetAuditRec()->GetQualifiedColumnName(&GetAuditRec()->f_OperType),
										pTable->m_pSqlConnection->NativeConvert(&DataInt(AUDIT_UPDATE_OP))									
									  );
	}
	strOperTypeFilter += _T(")");
	pTable->m_strFilter += strOperTypeFilter;
	GetAuditRec()->SetQualifier(_T("")); 
}

//----------------------------------------------------------------------------------------------
void AuditingManager::AddSelect(SqlTable* pTable, const CString& strColumnName)
{
	for (int nPos = 0; nPos <= GetAuditRec()->GetUpperBound(); nPos++)
	{ 
		 if (GetAuditRec()->GetAt(nPos) && GetAuditRec()->GetAt(nPos)->GetColumnName().CompareNoCase(strColumnName) == 0)
		 {
			 // controllo anche l'esistenza nel record della tabella posta sotto tracciatura 
			 DataObj* pData = pTable->GetRecord()->GetDataObjFromColumnName(strColumnName);
			 if (pData)
				 pTable->Select(pData);
		 }
	}				
}

//----------------------------------------------------------------------------------------------
BOOL AuditingManager::PrepareDeletedQuery
						(
								SqlTable* pTable, 
								DBTMaster* pDBTMaster,
								DataDate& aDataFrom, 
								DataDate& aDataTo
						)
{
	TB_LOCK_FOR_WRITE();

	if (
			!GetAuditRec() || // to instantiate and validate record if not yet
			!m_bValid ||
			!pTable || 
			!pTable->GetRecord() || 
			pTable->GetTableName().CompareNoCase(GetAuditRec()->m_strTracedTable) ||
			!pDBTMaster ||
			!pDBTMaster->GetXMLDBTInfo()
		)
		return FALSE;

	pTable->FromTable(GetAuditRec()); 

	SqlRecord* pRecord = pTable->GetRecord();
	// nella select devo inserire i campi chiave e gli eventuali UK e FixedKey presenti
	// nel DBTMaster passato come argomento
	
	// campi chiave
	for (int i = 0; i < pRecord->GetTableInfo()->GetSqlUniqueColumns()->GetSize(); i++)
		AddSelect(pTable, pRecord->GetTableInfo()->GetSqlUniqueColumns()->GetAt(i));

	// nella SELECT devo considerare anche gli eventuali campi di Universal Keys
	if (pDBTMaster->GetXMLDBTInfo()->GetXMLUniversalKeyGroup())
	{
		CXMLUniversalKey* pUniversalKey = NULL;
		for (int nUk =0; nUk < pDBTMaster->GetXMLDBTInfo()->GetXMLUniversalKeyGroup()->GetSize(); nUk++)
		{
			pUniversalKey = pDBTMaster->GetXMLDBTInfo()->GetXMLUniversalKeyGroup()->GetAt(nUk);
			if (pUniversalKey && !pUniversalKey->GetName().IsEmpty())
			{
				for (int nSeg = 0; nSeg < pUniversalKey->GetSegmentNumber(); nSeg++)
					if (!pUniversalKey->GetSegmentAt(nSeg).IsEmpty())
						AddSelect(pTable, pUniversalKey->GetSegmentAt(nSeg));
			}
		}
	}

	// nella WHERE CLAUSE devo inserire gli eventuali campi fissi (FixedKeys)
	if (pDBTMaster->GetXMLDBTInfo()->GetXMLFixedKeyArray())
	{
		CXMLFixedKey* pFixedKey = NULL;
		DataObj* pDataObj = NULL;
		DataObj* pDataRec = NULL;
	
		for (int i =0; i < pDBTMaster->GetXMLDBTInfo()->GetXMLFixedKeyArray()->GetSize(); i++)
		{
			pFixedKey = pDBTMaster->GetXMLDBTInfo()->GetXMLFixedKeyArray()->GetAt(i);
			if (pFixedKey && !pFixedKey->GetName().IsEmpty() && !pFixedKey->GetValue().IsEmpty())
			{
				if (!pTable->m_strFilter.IsEmpty())
					pTable->m_strFilter += _T(" AND ");
				pDataRec = pRecord->GetDataObjFromColumnName(pFixedKey->GetName());
				pDataObj = DataObj::DataObjCreate(pDataRec->GetDataType());
				pDataObj->AssignFromXMLString(pFixedKey->GetValue());
				
				pTable->m_strFilter += cwsprintf(
													_T("%s = %s"),
													pFixedKey->GetName(),
													pTable->m_pSqlConnection->NativeConvert(pDataObj)
												 );
				delete pDataObj;
			}
		}
	}
	//inserisco le date e l'operazione di delete
	if (!pTable->m_strFilter.IsEmpty())
		pTable->m_strFilter += _T(" AND ");
	pTable->m_strFilter += cwsprintf(
										_T("%s >= %s AND %s <= %s AND %s = %s"),
										GetAuditRec()->GetQualifiedColumnName(&GetAuditRec()->f_OperData),
										pTable->m_pSqlConnection->NativeConvert(&aDataFrom),
										GetAuditRec()->GetQualifiedColumnName(&GetAuditRec()->f_OperData),
										pTable->m_pSqlConnection->NativeConvert(&aDataTo),
										GetAuditRec()->GetQualifiedColumnName(&GetAuditRec()->f_OperType),
										pTable->m_pSqlConnection->NativeConvert(&DataInt(AUDIT_DELETE_OP))									
									);	
	return TRUE;
}

//----------------------------------------------------------------------------------------------
CTBNamespace* AuditingManager::CreateAuditingReport(CTBNamespace* pNamespace, CXMLFixedKeyArray* pFixedKey, BOOL bAllUsers, const CString& strUser)
{
	TB_LOCK_FOR_WRITE();

	// to instantiate and validate record if not yet
	if (!GetAuditRec() || !m_bValid)
		return NULL;

	ReportGenerator aReportGenerator(pNamespace, this, pFixedKey);
	return aReportGenerator.CreateReportFromTemplate(bAllUsers, strUser);
}


// serve per creare la tabella di lookup tra identificatore e namespace
///////////////////////////////////////////////////////////////////////////////
//								NamespacesLookupMng declaration
///////////////////////////////////////////////////////////////////////////////
NamespacesLookupMng::NamespacesLookupMng()
:
	m_pNamespaceRec	(NULL),
	m_pTable		(NULL),
	m_pSqlSession	(NULL)
{
	InitMap();
}

//-----------------------------------------------------------------------------
NamespacesLookupMng::~NamespacesLookupMng()
{
	if (m_pTable)
	{
		if (m_pTable->IsOpen())
			m_pTable->Close();
		delete m_pTable;
	}

	if (m_pSqlSession)
	{
		m_pSqlSession->Close();
		delete m_pSqlSession;
	}

	if (m_pNamespaceRec)
		delete m_pNamespaceRec;

// elimina gli elementi nella mappa
	POSITION	pos;
	CString		strKey;
	DataLng*	pDataID;

	for (pos = m_LookupMap.GetStartPosition(); pos != NULL;)
	{
		m_LookupMap.GetNextAssoc(pos, strKey, (CObject*&)pDataID);
		if (pDataID)
			delete pDataID;
	}
	m_LookupMap.RemoveAll();
}

//-----------------------------------------------------------------------------
void NamespacesLookupMng::InitMap()
{
	m_pNamespaceRec = new TNamespaces();
	m_pSqlSession = AfxGetAuditingSqlConnection()->GetNewSqlSession(AfxGetAuditingSqlConnection()->m_pContext);
	m_pTable = new SqlTable(m_pNamespaceRec, m_pSqlSession);
	TRY
	{
		m_pTable->Open(TRUE, E_FORWARD_ONLY);
		m_pTable->SelectAll();
		m_pTable->AddSortColumn(m_pNamespaceRec->f_ID);
		m_pTable->Query();
		
		while (!m_pTable->IsEOF())
		{
			CTBNamespace nsObject
				(
					(m_pNamespaceRec->f_Type == DOCUMENT_NSTYPE) ? CTBNamespace::DOCUMENT : CTBNamespace::REPORT,
					m_pNamespaceRec->f_Namespace.Str()
				);

			DataLng* pData = NULL;
			//BugFix# 19101
			CString strLowerKey = nsObject.ToString();
			strLowerKey.MakeLower();

			if (!m_LookupMap.Lookup(strLowerKey, (CObject*&)pData))
				m_LookupMap.SetAt(strLowerKey, (CObject*)new DataLng(m_pNamespaceRec->f_ID));

			m_pTable->MoveNext();
		}
	}

	CATCH(SqlException, e)
	{
		m_pTable->m_pSqlSession->ShowMessage(e->m_strError);
		if (m_pTable->IsOpen())
			m_pTable->Close();		
		return;					
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
DataLng NamespacesLookupMng::GetID(CTBNamespace* pNameSpace)
{
	TB_LOCK_FOR_WRITE();

	// controllo prima se il namespace é giá presente nella mappa di lookup
	DataLng* pData = NULL;
	
	//BugFix# 19101
	CString strLowerKey = pNameSpace->ToString();
	strLowerKey.MakeLower();

	if (m_LookupMap.Lookup(strLowerKey, (CObject*&)pData))
		return *pData;

	//inserisco il namespace prima nella tabella di database poi nella mappa in memoria
	//provo a fare un inserimento secco 	
	//devo inserire il namespace senza tipo e valorizzare il campo TypeNs con il tipo di namespace
	m_pNamespaceRec->Init();
	m_pNamespaceRec->f_Namespace = pNameSpace->ToUnparsedString();
	m_pNamespaceRec->f_Type = (pNameSpace->GetType() == CTBNamespace::DOCUMENT) ? DOCUMENT_NSTYPE : REPORT_NSTYPE;
    if (!m_pTable->NativeInsert())
	{ 
		//vedo se per caso il record é stato inserito poco prima
		SqlSession* pSqlSession = AfxGetAuditingSqlConnection()->GetNewSqlSession(AfxGetAuditingSqlConnection()->m_pContext);
		SqlTable aTable(m_pNamespaceRec, pSqlSession);
		TRY
		{
			aTable.Open(FALSE, E_FAST_FORWARD_ONLY);
			aTable.SelectAll();
			aTable.m_strFilter = (cwsprintf(_T(" %s = %s AND %s = %s"),
								  (LPCTSTR)m_pNamespaceRec->GetColumnName(&m_pNamespaceRec->f_Namespace),
								  AfxGetDefaultSqlConnection()->NativeConvert(&m_pNamespaceRec->f_Namespace),
								  (LPCTSTR)m_pNamespaceRec->GetColumnName(&m_pNamespaceRec->f_Type),
								  AfxGetDefaultSqlConnection()->NativeConvert(&m_pNamespaceRec->f_Type)
								 ));
			aTable.Query();
			BOOL bEOF = aTable.IsEOF();
			aTable.Close();
			pSqlSession->Close();
			delete pSqlSession;
			if (bEOF)
				//l'errore in fase di inserimento non era dovuto al record duplicato
				return DataLng(0l);		
		}

		CATCH(SqlException, e)
		{
			aTable.m_pSqlSession->ShowMessage(e->m_strError);
			if (aTable.IsOpen())
				aTable.Close();	
			pSqlSession->Close();
			delete pSqlSession;
			return DataLng(0l);
		}
		END_CATCH
	}
	
	pData = new DataLng(m_pNamespaceRec->f_ID);
	m_LookupMap.SetAt(strLowerKey, (CObject*)pData);
	
	return *pData;	
}
