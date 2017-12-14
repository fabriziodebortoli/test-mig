#include "stdafx.h" 

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TbOleDb\OledbMng.h>
#include <TbOleDb\Sqlconnect.h>
#include <TbOleDb\SqlAccessor.h>
#include <TbOleDb\SqlTable.h>
#include <TbGes\NumbererService.h>
#include <TbGes\EXTDOC.H>
#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbGenLibManaged\GlobalFunctions.h>

#include "RSTables.h"
#include "TBRowSecurityEnums.h"
#include "RSStructures.h"
#include "RSManager.h"
#include "CDRSGrants.h"
  
static const TCHAR szRowSecurityFileName[]	= _T("RowSecurityObjects.xml");
static const TCHAR szCrsRowSecurityFileName[]	= _T("RowSecurityObjects.crs");

static TCHAR szP1[] = _T("P1");
static TCHAR szP2[] = _T("P2");
static TCHAR szP3[] = _T("P3");
static TCHAR szP4[] = _T("P4");
static TCHAR szP5[] = _T("P5");
static TCHAR szP6[] = _T("P6");
static TCHAR szP7[] = _T("P7");
static TCHAR szRowID[] = _T("RowID");

//local function
//-----------------------------------------------------------------------------
BOOL ExistSubjectInArray(Array* pSubjects, int nSubjectID)
{
	if (!pSubjects)
		return FALSE;

	CSubjectCache* pSubjectCache = NULL;
	
	for (int i = 0; i <= pSubjects->GetUpperBound(); i++)
	{
		pSubjectCache = (CSubjectCache*)pSubjects->GetAt(i);
		if (pSubjectCache && pSubjectCache->m_SubjectID == nSubjectID)
			return TRUE;
	}

	return FALSE;
}

///////////////////////////////////////////////////////////////////////////////
//						CTableRowSecurityMng declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CTableRowSecurityMng::CTableRowSecurityMng(SqlSession* pSqlSession, RSProtectedTableInfo* pProtectedInfo)
	:
	m_pSqlSession(pSqlSession),
	m_pProtectedInfo(pProtectedInfo)
{	
}

//-----------------------------------------------------------------------------
CTableRowSecurityMng::~CTableRowSecurityMng()
{
	delete m_pProtectedInfo;
}

/*
Il filtraggio sul CurrentWorkerID deve essere fatto sul campo TBCreatedID del record della tabella master oppure sul campo WorkerID della tabella RS_SubjectsGrants
Dobbiamo tenere conto delle seguenti casistiche:

Caso 1: il SqlRecord passato come parametri rappresenta la master table dell'entità es OM_Clients
Per ogni entità (nell'esempio client) per cui la tabella è sotto protezione è necessario fare solo la join con la tabella RS_WorkersGrants utilizzando come alias WRS+nome entità
ed il filtraggio sul WorkerID che può essere chi ha inserito il record oppure chi ha un grant sulla tabella
from RS_SubjectsGrants WRSClient
where ....... (OM_Clients.TBCreatedID = CurrentWorkerID OR (WRSClient.RowSecurityID = OM_Clients.RowSecurityID AND WRSClient.EntityName='Client' AND WRSClient.WorkerID=CurrentWorkerID))

Caso 2: il SqlRecord passato come parametri non rappresenta la master table dell'entità es OM_Tasks
Per ogni entità (nell'esempio client) per cui la tabella è sotto protezione è necessario fare:
la join con la tabella che rappresenta la master table del'entità utilizzando MRS+nome entità come alias
la join con la tabella RS_WorkersGrants utilizzando come filtraggio la master table dell'entità

from ....., OM_Client MRSClient, RS_SubjectsGrants WRSClient
where ....... AND MRSClient.ClientCode = OM_Tasks.ClientCode AND ((MRSClient.TBCreatedID = CurrentWorkerID OR (WRSClient.RowSecurityID = MRSClient.RowSecurityID 
AND WRSClient.EntityName='Client'AND WRSClient.WorkerID=CurrentWorkerID))
*/
	
//-----------------------------------------------------------------------------
void CTableRowSecurityMng::AddRowSecurityFilters(SqlTable* pTable, SqlTableItem* pTableItem)
{
	if (!pTable || !pTableItem || !m_pProtectedInfo)
		return;

	CString strRSFilter;
	RSEntityTableInfo* pEntityTable = NULL;
	RSProtectedColumns* pRSColumns = NULL;
	for (int i =0; i <= m_pProtectedInfo->m_arProtectedInfo.GetUpperBound(); i++)
	{
		pEntityTable = (RSEntityTableInfo*)m_pProtectedInfo->m_arProtectedInfo.GetAt(i);
		if (pEntityTable && pEntityTable->m_pEntityInfo && pEntityTable->m_pEntityInfo->m_bUsed)
		{
			strRSFilter = pEntityTable->GetFilterText(pTable, pTableItem);
			if (!strRSFilter.IsEmpty())
			{
				if (!pTable->m_strFilter.IsEmpty())
					pTable->m_strFilter += _T(" AND ");
				pTable->m_strFilter += strRSFilter;
			}
		}
	}	
}

//-----------------------------------------------------------------------------
CString CTableRowSecurityMng::GetSelectGrantString(SqlTable* pTable)
{
	if (!pTable || !pTable->IsSelectGrantInformation() || !m_pProtectedInfo)
		return _T("");

	CString strFilterAdded;
	RSEntityTableInfo* pEntityTable = NULL;
	RSProtectedColumns* pRSColumns = NULL;
	for (int i =0; i <= m_pProtectedInfo->m_arProtectedInfo.GetUpperBound(); i++)
	{
		pEntityTable = (RSEntityTableInfo*)m_pProtectedInfo->m_arProtectedInfo.GetAt(i);
		//inserisco le informazioni di grant solo per l'entità per cui pTableItem->GetRecord() rappresenta la tabella master
		if (pEntityTable && pEntityTable->m_pEntityInfo && pEntityTable->m_pEntityInfo->m_bUsed && pEntityTable->m_pEntityInfo->m_MasterTableNamespace == *pTable->GetRecord()->GetNamespace())
			return pEntityTable->GetSelectGrantString(pTable);
	}
	return _T("");
}

//-----------------------------------------------------------------------------
void CTableRowSecurityMng::ValorizeRowSecurityParameters(SqlTable* pTable)
{ 
	DataLng* pRSSelectWorker = pTable->GetRowSecuritySelectWorker();	
	DataLng* pRSFilterWorker = pTable->GetRowSecurityFilterWorker();

	RSEntityTableInfo* pEntityTable = NULL;
	if (pRSSelectWorker || pRSFilterWorker)
	{
		for (int i =0; i <= m_pProtectedInfo->m_arProtectedInfo.GetUpperBound(); i++)
		{
			pEntityTable = (RSEntityTableInfo*)m_pProtectedInfo->m_arProtectedInfo.GetAt(i);
			if (pEntityTable && pEntityTable->m_pEntityInfo && pEntityTable->m_pEntityInfo->m_bUsed)
			{
				pEntityTable->ValorizeRowSecurityParameters(pTable);
				if (pTable->IsSelectGrantInformation() && pEntityTable->m_pEntityInfo->m_MasterTableNamespace == *pTable->GetRecord()->GetNamespace())
				{
					//parametro presente nella subquery nella select
					//l_CurrentWorkerGrantType = (select GrantType from RS_SubjectsGrants where WorkerID = ? and RowSecurityID = %s.RowSecurityID)"
					if (pTable->ExistParam(_T("SubRSWorkerID")))			
						pTable->SetParamValue(_T("SubRSWorkerID"), (pRSSelectWorker) ? *pRSSelectWorker : DataLng(AfxGetWorkerId())); 
				}
			}
		}		
	}	
}

//-----------------------------------------------------------------------------
BOOL CTableRowSecurityMng::CanCurrentWorkerUsesRecord(SqlRecord* pRecord, SqlTable* pTable)
{
	if (!pRecord)
		return TRUE;
	RowSecurityAddOnFields* pAddOnFields = (RowSecurityAddOnFields*)pRecord->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields));	
	if (pAddOnFields)
	{
		if (!pAddOnFields->f_IsProtected)
			return TRUE;
		//mi devo far dare anche la SqlTable
		//il controllo lo devo fare differente a seconda se è valorizzato o meno il dataobj GetRowSecuritySelectWorker
		DataLng* pRSSelectWorker = pTable->GetRowSecuritySelectWorker();	
		return	(pRSSelectWorker)
				? (pAddOnFields->l_SpecificWorkerGrantType == E_GRANT_TYPE_READ_ONLY || pAddOnFields->l_SpecificWorkerGrantType == E_GRANT_TYPE_READWRITE)
				: (pAddOnFields->l_CurrentWorkerGrantType == E_GRANT_TYPE_READ_ONLY || pAddOnFields->l_CurrentWorkerGrantType == E_GRANT_TYPE_READWRITE);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CTableRowSecurityMng::HideProtectedFields(SqlRecord* pRecord) 
{
	if (!pRecord)
		return;

	//mi devo far dare anche la SqlTable
	//il controllo lo devo fare differente a seconda se è valorizzato o meno il dataobj GetRowSecuritySelectWorker
	RowSecurityAddOnFields* pAddOnFields = (RowSecurityAddOnFields*)pRecord->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields));	

	BOOL bCanUseRec = !pAddOnFields || !pAddOnFields->f_IsProtected || pAddOnFields->l_CurrentWorkerGrantType == E_GRANT_TYPE_READ_ONLY || pAddOnFields->l_CurrentWorkerGrantType == E_GRANT_TYPE_READWRITE;
	for (int i = 0; i < pRecord->GetSize() - 1; i++)
	{
		SqlRecordItem* pItem = pRecord->GetAt(i);
		if (!pItem || pItem->IsSpecial())
			continue;
		pItem->GetDataObj()->SetPrivate(!bCanUseRec);
	}
}

///////////////////////////////////////////////////////////////////////////////
//						CEntitiesManager declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CEntitiesManager::CEntitiesManager(SqlSession* pSession)
	:
	m_pSqlSession(pSession) 
{
	m_pEntityArray = new CRSEntityArray();
}

//-----------------------------------------------------------------------------
CEntitiesManager::~CEntitiesManager()
{
	delete(m_pEntityArray);
}

//-----------------------------------------------------------------------------
void CEntitiesManager::LoadInfoFromXML(CXMLNode* pnNode)
{
	CXMLNode* pnChild = NULL;
	CXMLNodeChildsList* pnChilds = NULL;			
	if (pnChilds = pnNode->GetChilds())
	{
		for (int i =0; i < pnChilds->GetCount(); i++)
		{
			pnChild = pnChilds->GetAt(i);			
			if (pnChild)
			{
				RSEntityInfo* pEntityInfo = new RSEntityInfo();
				if (pEntityInfo->Parse(pnChild))
				{
					//l'entity è stata inserita temporaneamente dal parse del RSProtectedTableInfo
					//devo aggiornarne le informazioni 
					RSEntityInfo* pTemp = GetEntityInfo(pEntityInfo->m_strName, FALSE);
					if (pTemp)
					{
						pTemp->Assign(pEntityInfo);
						delete pEntityInfo;
					}
					else
						m_pEntityArray->Add(pEntityInfo);
				}
			}
		}
	}	
}

//-----------------------------------------------------------------------------
void CEntitiesManager::SetUsedEntities(const CString& strUsedEntities)
{
	m_strUsedEntities = strUsedEntities;

	// se le entita' utilizzate sono vuote imposto d'ufficio
	// a tutte il valore m_bUsed = FALSE
	if (m_strUsedEntities.IsEmpty())
	{
		for (int i = 0; i <= m_pEntityArray->GetUpperBound(); i++)
			m_pEntityArray->GetAt(i)->m_bUsed = FALSE;
		return;
	}
	
	CStringArray arEntries;
	int curPos= 0;

	CString strToken= m_strUsedEntities.Tokenize(_T(";"), curPos);
	while (strToken != "")
	{
		strToken.Trim(); // faccio prima il Trim perche' dopo ogni ";" viene messo un blank
		arEntries.Add(strToken);
		strToken= m_strUsedEntities.Tokenize(_T(";"), curPos);
	}

	//setto le entity effettivamente utilizzati dall'azienda
	RSEntityInfo* pEntityInfo = NULL;
	BOOL bUsed;
	for (int i=0; i <= m_pEntityArray->GetUpperBound(); i++)
	{
		pEntityInfo = m_pEntityArray->GetAt(i);
		if (pEntityInfo)
		{
			bUsed = FALSE;
			for (int j=0; j <= arEntries.GetUpperBound(); j++)
			{
				if (pEntityInfo->m_strName.CompareNoCase(arEntries.GetAt(j)) == 0)
				{
					bUsed = TRUE;
					break;
				}
			}
			pEntityInfo->m_bUsed = bUsed;
		}
	}
}

//-----------------------------------------------------------------------------
void CEntitiesManager::SetProtectionInformation(const CString& strUsedEntities)
{
	//verifico le sole entità usate per la company corrente
	SetUsedEntities(strUsedEntities);
	
	//carico le informazioni relative alle addoncolumns e nel mentre marco le sole entità utilizzate
	//CAddColsTableDescription* pAddColsDescri = NULL;
	RSEntityInfo* pEntityInfo = NULL;

	for (int i = 0; i <= m_pEntityArray->GetUpperBound(); i++)
	{
		pEntityInfo = m_pEntityArray->GetAt(i);
		if (pEntityInfo)
		{
			// l'aggiunta della colonna RowSecurityID la faccio utilizzando la tecnica degli AddOnNewColumn ma visto che non so a priori il nome della tabella 
			// a cui aggiungere la colonna (dipende delle entità apportate dal programma gestionale) effettuo l'add utilizzando direttamente il metodo virtuale 
			// anzichè chiamare le macro e compilare il file AddOnDatabaseObjects.xml
		/*	pAddColsDescri = new CAddColsTableDescription();
			pAddColsDescri->SetNamespace(pEntityInfo->m_MasterTableNamespace);
			pAddColsDescri->SetNotLocalizedTitle(pAddColsDescri->GetName());

			CAlterTableDescription* pAlterDescription = new CAlterTableDescription();
			pAlterDescription->SetNamespace(CTBNamespace(_T("Library.Extensions.TBRowSecurity.TBRowSecurity")));
			pAlterDescription->SetNotLocalizedTitle(pAddColsDescri->GetName());
			pAddColsDescri->m_arAlterTables.Add(pAlterDescription);
			AfxGetApplicationContext()->GetObject<CAlterTableDescriptionArray>(&CApplicationContext::GetAddOnFieldsTable)->AddAddOnFieldOnTable(pAddColsDescri);
		*/	
			//Registro l'entity per l'autonumerazione del campo 		
			CTBNamespace aService (CTBNamespace::BEHAVIOUR, CAutoincrementService::GetStaticName());	
			if (!AfxGetBehavioursRegistry()->GetEntity(pEntityInfo->m_strAutonumberNamespace))
				AfxGetWritableBehavioursRegistry()->RegisterEntity(pEntityInfo->m_strAutonumberNamespace, aService.ToString(), cwsprintf(_T("%s RowSecurityID"), pEntityInfo->m_strName));			
		}
	}
}

//-----------------------------------------------------------------------------
CStringArray* CEntitiesManager::GetAllEntities() const
{
	CStringArray* pAllEntities = new CStringArray();
	RSEntityInfo* pEntityInfo = NULL;
	for (int i = 0; i <= m_pEntityArray->GetUpperBound(); i++)
	{
		pEntityInfo = m_pEntityArray->GetAt(i);
		if (pEntityInfo)
			pAllEntities->Add(pEntityInfo->m_strName);
	}

	return pAllEntities;
}

//-----------------------------------------------------------------------------
void CEntitiesManager::UpdateUsedEntities(const CString& strUsedEntries)
{
	//devo modificare il campo 
	TRS_Configuration aConfRec;
	SqlTable aTable(&aConfRec, m_pSqlSession);
	aTable.SetAutocommit();

	TRY
	{
		aTable.Open(TRUE);
		aTable.SelectAll();	
		//aTable.m_strFilter =  cwsprintf(_T("OfficeID = %s"), m_pSqlSession->m_pSqlConnection->NativeConvert(&DataLng(0)));
		aTable.Query();
		if (aTable.IsEmpty())
		{
			aTable.AddNew();
			aConfRec.f_OfficeID = 0;
		}
		else
			aTable.Edit();
		aConfRec.f_UsedEntries = strUsedEntries;
		aTable.Update();
		aTable.Close();

		SetUsedEntities(strUsedEntries);
	}
	CATCH(SqlException, e)
	{
		if (aTable.IsOpen())
			aTable.Close();
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
RSEntityInfo* CEntitiesManager::GetEntityInfo(const CString& strEntityName, BOOL bAddIfNotExist) 
{
	RSEntityInfo* pEntityInfo = GetEntityInfo(strEntityName);
	if (!pEntityInfo && bAddIfNotExist)
	{
		pEntityInfo = new RSEntityInfo();
		pEntityInfo->m_strName = strEntityName;
		m_pEntityArray->Add(pEntityInfo);
	}
	
	return pEntityInfo;
}

//-----------------------------------------------------------------------------
RSEntityInfo* CEntitiesManager::GetEntityInfo(const CString& strEntityName) 
{
	RSEntityInfo* pEntityInfo = NULL;
	for (int i = 0; i <= m_pEntityArray->GetUpperBound(); i++)
	{
		pEntityInfo = m_pEntityArray->GetAt(i);
		if (pEntityInfo && pEntityInfo->m_strName.CompareNoCase(strEntityName) == 0)
			return pEntityInfo;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
RSEntityInfo* CEntitiesManager::GetEntityInfo(const CTBNamespace& docNS)
{
	RSEntityInfo* pEntityInfo = NULL;
	for (int i = 0; i <= m_pEntityArray->GetUpperBound(); i++)
	{
		pEntityInfo = m_pEntityArray->GetAt(i);

		for (int r = 0; r < pEntityInfo->m_arrDocNamespace.GetSize(); r++)
		{
			if (pEntityInfo && pEntityInfo->m_arrDocNamespace.GetAt(r) == docNS)
				return pEntityInfo;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL CEntitiesManager::IsEntityDocument(const CTBNamespace& docNS)
{
	RSEntityInfo* pEntityInfo = NULL;
	for (int i = 0; i <= m_pEntityArray->GetUpperBound(); i++)
	{
		pEntityInfo = m_pEntityArray->GetAt(i);
		for (int r = 0; r < pEntityInfo->m_arrDocNamespace.GetSize(); r++)
		{
			if (pEntityInfo && pEntityInfo->m_arrDocNamespace.GetAt(r) == docNS)
				return TRUE;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CEntitiesManager::IsEntityMasterTable(const CTBNamespace& tableNS)
{
	//a partire dalla Runtime rendo il nome della tabella 
	CString strTableName = tableNS.GetObjectName();

	RSEntityInfo* pEntityInfo = NULL;	
	for (int i = 0; i <= m_pEntityArray->GetUpperBound(); i++)
	{
		pEntityInfo = m_pEntityArray->GetAt(i);
		if (pEntityInfo && pEntityInfo->m_strMasterTable.CollateNoCase(strTableName) == 0)
			return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CEntitiesManager::ValorizeRowSecurityID()
{	
	CAutoincrementService aAutoincrement(m_pSqlSession); //per valorizzare il campo RowSecurityID utilizzo il servizio di Autoincrement legato a ciascuna entità

	const SqlCatalogEntry* pCatalogEntry = NULL;
	SqlRecord*		pMasterRec = NULL;
	RSEntityInfo*	pEntityInfo = NULL;	

	for (int i = 0; i <= m_pEntityArray->GetUpperBound(); i++)
	{
		pEntityInfo = m_pEntityArray->GetAt(i);

		if (pEntityInfo && pEntityInfo->m_bUsed) // considero solo le entita' utilizzate
		{
			pCatalogEntry = m_pSqlSession->GetSqlConnection()->GetCatalogEntry(pEntityInfo->m_strMasterTable);
			pMasterRec = (pCatalogEntry) ? pCatalogEntry->CreateRecord() : NULL;
			DataObj* rowSecurityID = pMasterRec->GetDataObjFromColumnName(RowSecurityAddOnFields::s_sRowSecurityID);

			if (pMasterRec && rowSecurityID)
			{
				SqlTable aTable(pMasterRec, m_pSqlSession);
				aTable.SetAutocommit();

				TRY
				{
					aTable.Open(TRUE);
					aTable.SetSkipRowSecurity();
					aTable.Select(rowSecurityID); //Seleziono il solo campo RowSecurityID la selezione dei segmenti di chiave primaria avviene nella BuildSelect
					aTable.m_strFilter = cwsprintf(_T("%s is NULL OR %s <= 0"), RowSecurityAddOnFields::s_sRowSecurityID, RowSecurityAddOnFields::s_sRowSecurityID);
					aTable.Query();

					while (!aTable.IsEOF())
					{
						aTable.Edit();
						aAutoincrement.GetNextNumber(pEntityInfo->m_strAutonumberNamespace, rowSecurityID);
						aTable.Update();
						aTable.MoveNext();
					}
					aTable.Close();
					delete pMasterRec;
				}
				CATCH(SqlException, e)
				{
					if (aTable.IsOpen())
						aTable.Close();
					if (pMasterRec)
						delete pMasterRec;
					return FALSE; //?????????????
				}
				END_CATCH
			}
			else
				return FALSE;
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
SqlRecord* CEntitiesManager::GetEntityMasterRecord(const CString& strEntityName, const DataLng& nRowSecurityID)
{
	const SqlCatalogEntry* pCatalogEntry = NULL;
	SqlRecord*	pMasterRec = NULL;
	RSEntityInfo* pEntityInfo = GetEntityInfo(strEntityName);
	if (pEntityInfo)
	{
		pCatalogEntry = m_pSqlSession->GetSqlConnection()->GetCatalogEntry(pEntityInfo->m_strMasterTable);
		pMasterRec = (pCatalogEntry) ? pCatalogEntry->CreateRecord() : NULL;
		if (pMasterRec)
		{
			SqlTable aTable(pMasterRec, m_pSqlSession);
			TRY
			{
				aTable.Open();
				aTable.SetSkipRowSecurity();
				aTable.SelectAll();
				aTable.m_strFilter = cwsprintf(_T("%s = %s"), RowSecurityAddOnFields::s_sRowSecurityID, m_pSqlSession->GetSqlConnection()->NativeConvert(&nRowSecurityID));
				aTable.Query();
				aTable.Close();
			}
			CATCH(SqlException, e)
			{
				if (aTable.IsOpen())
					aTable.Close();
				return pMasterRec; 
			}
			END_CATCH			
		}
	}

	return pMasterRec;
}

///////////////////////////////////////////////////////////////////////////////
//						CSubjectsManager declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CSubjectsManager::CSubjectsManager(SqlSession* pSqlSession)
	: 
	m_pSqlSession(pSqlSession)	
{
	m_pSubjectsCacheArray = new CSubjectCacheArray();
	m_pOldSubjectsCacheArray = new CSubjectCacheArray();

	// leggo le informazioni presenti nella tabelle RS_Subjects e RS_SubjectsHierarchy
	LoadInformation();
}

//-----------------------------------------------------------------------------
CSubjectsManager::~CSubjectsManager()
{
	delete m_pSubjectsCacheArray;
	delete m_pOldSubjectsCacheArray;
}

//-----------------------------------------------------------------------------
CSubjectCache* CSubjectsManager::GetSubjectCache(int nSubjectID)
{ 
	return m_pSubjectsCacheArray->GetSubjectCache(nSubjectID); 
};

//-----------------------------------------------------------------------------
int CSubjectsManager::GetSubjectID(int nWorkerID)
{ 
	return m_pSubjectsCacheArray->GetSubjectID(nWorkerID); 
};

//-----------------------------------------------------------------------------
CSubjectCache* CSubjectsManager::GetSubjectCacheFromWorkerID(int nWorkerID) 
{ 
	return m_pSubjectsCacheArray->GetSubjectCacheFromWorkerID(nWorkerID); 
};

//-----------------------------------------------------------------------------
CSubjectCache* CSubjectsManager::GetSubjectCacheFromResource(const CString& resourceType, const CString& resourceCode) 
{ 
	return m_pSubjectsCacheArray->GetSubjectCacheFromResource(resourceType, resourceCode); 
};

//-----------------------------------------------------------------------------
CSubjectCache* CSubjectsManager::GetOldSubjectCache(int nSubjectID)
{ 
	return m_pOldSubjectsCacheArray->GetSubjectCache(nSubjectID); 
};

//-----------------------------------------------------------------------------
CSubjectCacheArray* CSubjectsManager::GetSubjectCacheArray() const
{ 
	return m_pSubjectsCacheArray; 
}

//-----------------------------------------------------------------------------
void CSubjectsManager::LoadInformation()
{
	LoadSubjects();
	LoadHierarchies();
}

// leggo le informazioni dalla tabella RS_Subjects e riempio la cache
//-----------------------------------------------------------------------------
void CSubjectsManager::LoadSubjects()
{
	// pulisco la cache e ricarico tutto
	m_pSubjectsCacheArray->RemoveAll();

	TRS_Subjects subjectRec;
	SqlTable table(&subjectRec, m_pSqlSession);

	TRY
	{	
		table.Open(FALSE, E_FAST_FORWARD_ONLY);
		table.Select(subjectRec.f_SubjectID);
		table.Select(subjectRec.f_IsWorker);		
		table.Select(subjectRec.f_ResourceType);		
		table.Select(subjectRec.f_ResourceCode);		
		table.Select(subjectRec.f_WorkerID);		
		table.Select(subjectRec.f_Description);	
		table.Query();

		while(!table.IsEOF())
		{
			m_pSubjectsCacheArray->Add(new CSubjectCache(&subjectRec));			
			table.MoveNext();
		}
		table.Close();
	}
	CATCH(SqlException, e)	
	{
		if (table.IsOpen()) table.Close();
		TRACE("%s\n", (LPCTSTR)e->m_strError);
	}
	END_CATCH
}

// leggo le informazioni dalla tabella RS_SubjectsHierarchy e riempio la cache
//-----------------------------------------------------------------------------
void CSubjectsManager::LoadHierarchies()
{
	m_nMaxLevel = 0;
	for (int i = 0; i <= m_pSubjectsCacheArray->GetUpperBound(); i++)
	{
		CSubjectCache* pSubjectCache = m_pSubjectsCacheArray->GetAt(i);
		if (pSubjectCache)
			pSubjectCache->LoadHierarchyInfo(m_pSqlSession, this);
	}
}

// creo la copia dell'array che contiene la cache dei subjects e relative hierarchies
//-----------------------------------------------------------------------------
void CSubjectsManager::CreateOldSubjectsCacheArray()
{
	m_pOldSubjectsCacheArray->RemoveAll();

	// copio i Subjects
	CSubjectCache* pSubjectCache = NULL;
	for (int i = 0; i <= m_pSubjectsCacheArray->GetUpperBound(); i++)
	{
		pSubjectCache = m_pSubjectsCacheArray->GetAt(i);
		if (pSubjectCache)
			m_pOldSubjectsCacheArray->Add(new CSubjectCache(*pSubjectCache));
	}

	// per ogni soggetto copiato precedentemente vado a copiare le sue hierarchies
	for (int i = 0; i <= m_pOldSubjectsCacheArray->GetUpperBound(); i++)
	{
		pSubjectCache = m_pOldSubjectsCacheArray->GetAt(i);
		if (pSubjectCache)
			pSubjectCache->CopyHierarchyInfo(m_pSubjectsCacheArray->GetSubjectCache(pSubjectCache->m_SubjectID), m_pOldSubjectsCacheArray);
	}
}

///////////////////////////////////////////////////////////////////////////////
//						CGrantsManager declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CGrantsManager::CGrantsManager(SqlSession* pSession, CSubjectsManager* pSubjectMng)
	: 
	m_pSqlSession(pSession),
	m_pSubjectsManager(pSubjectMng)
{
}

//-----------------------------------------------------------------------------
CGrantsManager::~CGrantsManager() 
{
}

//dato un worker restituisce tutti i subject che ereditano in modo implicito il grant 
//ovvero tutti i master del worker e tutti i suoi fratelli (NO VECCHIA RICHIESTA)
//ovvero tutti i suoi slave (di ogni livello)
//-----------------------------------------------------------------------------
Array* CGrantsManager::GetSubjectsToImplicitGrant(int nWorkerID)
{	
	Array* pSubjects = new Array();
	pSubjects->SetOwns(FALSE);
	CSubjectCache* pSubjectCache = m_pSubjectsManager->GetSubjectCacheFromWorkerID(nWorkerID);
	
	if (!pSubjectCache)
		return pSubjects;
		
	if (!ExistSubjectInArray(pSubjects, pSubjectCache->m_SubjectID))
		pSubjects->Add((CObject*)pSubjectCache);
	
	//considero i subjects slave di nWorkerID in modo tale da fornire anche a loro i grant
	CSubjectHierarchy* pSubjectHierachy = NULL;
	for (int i = 0; i <= pSubjectCache->m_pSlaveSubjects->GetUpperBound(); i++)
	{
		pSubjectHierachy = (CSubjectHierarchy*)pSubjectCache->m_pSlaveSubjects->GetAt(i);
		if (pSubjectHierachy)
		{
			if (!ExistSubjectInArray(pSubjects, pSubjectHierachy->m_pSubject->m_SubjectID))
				pSubjects->Add((CObject*)pSubjectHierachy->m_pSubject);
		}
	}

	return pSubjects;
}

//dato un subjectID di tipo risorsa i grants espliciti devono essere assegnati anche ai soli worker di livello 1 (???) facenti parte della risorsa
//-----------------------------------------------------------------------------
Array* CGrantsManager::GetSubjectsToExplictGrant(int nSubjectID)
{
	Array* pSubjects = new Array();
	CSubjectHierarchy* pSubjectHierachy = NULL;
	pSubjects->SetOwns(FALSE);
	CSubjectCache* pSubjectCache = m_pSubjectsManager->GetSubjectCache(nSubjectID);
	
	if (!pSubjectCache)
		return pSubjects;
		
	if (!ExistSubjectInArray(pSubjects, pSubjectCache->m_SubjectID))
		pSubjects->Add((CObject*)pSubjectCache);

	if (!pSubjectCache->IsWorker())
		for (int i = 0; i <= pSubjectCache->m_pSlaveSubjects->GetUpperBound(); i++)
		{
			pSubjectHierachy = (CSubjectHierarchy*)pSubjectCache->m_pSlaveSubjects->GetAt(i);
			if (pSubjectHierachy)
			{
				if (pSubjectHierachy->m_pSubject->IsWorker() && pSubjectHierachy->m_nrLevel == 1 && !ExistSubjectInArray(pSubjects, pSubjectHierachy->m_pSubject->m_SubjectID))
				pSubjects->Add((CObject*)pSubjectHierachy->m_pSubject);
			}
		}		
	return pSubjects;
}

//-----------------------------------------------------------------------------
Array* CGrantsManager::PutRecordUnderProtection(const CString& strEntityName, SqlRecord* pRecord)
{
	if (!pRecord || strEntityName.IsEmpty())
		return NULL;
	
	RowSecurityAddOnFields* pAddOnFields = (RowSecurityAddOnFields*)pRecord->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields));
	if (!pAddOnFields || pAddOnFields->f_RowSecurityID.IsEmpty() || strEntityName.IsEmpty())
		return NULL;
	
	Array* pSubjects = GetSubjectsToImplicitGrant(AfxGetLoggedSubject()->m_pResourceElement->m_WorkerID);
	Array* pGrantedSubjects = new Array();
	pGrantedSubjects->SetOwns(FALSE);
	
	TURS_SubjectsGrants subjectsGrantsTU;
	subjectsGrantsTU.SetSqlSession(m_pSqlSession);

	TURowSecurity rowSecurityTU(pRecord->GetRuntimeClass(), m_pSqlSession);
	TRS_SubjectsGrants* subjectGrantRec;
	RowSecurityAddOnFields* pAddOnFieldsToProtect = NULL;
	CSubjectCache* pSubjectCache = NULL;

	TRY
	{
		if (rowSecurityTU.FindRecord(pAddOnFields->f_RowSecurityID, TRUE) == TableUpdater::FOUND)
		{
			pAddOnFieldsToProtect = rowSecurityTU.GetRowSecurityAddOnFields();
			if (pAddOnFieldsToProtect)
			{
				m_pSqlSession->StartTransaction();				
				pAddOnFieldsToProtect->f_IsProtected = TRUE;
				rowSecurityTU.UpdateRecord();
		
				for (int i = 0; i <= pSubjects->GetUpperBound(); i++)
				{
					pSubjectCache = (CSubjectCache*)pSubjects->GetAt(i);
					if (!pSubjectCache->IsWorker())
						continue;
			
					if (subjectsGrantsTU.FindRecord(DataLng(pSubjectCache->m_SubjectID), DataStr(strEntityName), pAddOnFields->f_RowSecurityID, FALSE) == TableUpdater::NOT_FOUND)
					{
						subjectGrantRec = subjectsGrantsTU.GetRecord();
						subjectGrantRec->f_SubjectID = pSubjectCache->m_SubjectID;
						subjectGrantRec->f_WorkerID = pSubjectCache->GetWorkerID();
						subjectGrantRec->f_EntityName = strEntityName;
						subjectGrantRec->f_GrantType = E_GRANT_TYPE_READWRITE;
						subjectGrantRec->f_RowSecurityID = pAddOnFields->f_RowSecurityID;
						subjectGrantRec->f_Inherited = FALSE;
						subjectGrantRec->f_IsImplicit = FALSE;
						pGrantedSubjects->Add(pSubjectCache);
						subjectsGrantsTU.UpdateRecord();
					}			
				}
				m_pSqlSession->Commit();
			}
		}
	}
	CATCH(SqlException, e)	
	{
		m_pSqlSession->Abort();
		if (pSubjects)
			delete pSubjects; 
		if (pGrantedSubjects)
			delete pGrantedSubjects;
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		THROW(e);
	}
	END_CATCH

	if (pSubjects)
		delete pSubjects;
	
	return pGrantedSubjects;
}

//-----------------------------------------------------------------------------
void CGrantsManager::ModifyImplicitGrant(const CString& strEntityName, int rowSecurityID, int nOldWorkerID, int nNewWorkerID, CAbstractFormDoc* pDoc)
{
	CRSGrantsClientDoc* pClientDoc;
	if (pDoc && (pClientDoc = (CRSGrantsClientDoc*)pDoc->GetClientDoc(RUNTIME_CLASS(CRSGrantsClientDoc))))
	{
		pClientDoc->ModifyImplicitGrant(nOldWorkerID, nNewWorkerID);
		return;
	}
}

//-----------------------------------------------------------------------------
void CGrantsManager::SaveExplicitGrants(DBTEntitySubjectsGrants* pDBTEntitySubjectsGrants)
{
	if (!pDBTEntitySubjectsGrants || pDBTEntitySubjectsGrants->GetRecords()->GetUpperBound() < 0)
		return;

	TURS_SubjectsGrants aTUSubjectsGrants;
	aTUSubjectsGrants.SetSqlSession(m_pSqlSession);
	aTUSubjectsGrants.SetAutocommit();

	TRS_SubjectsGrants* pDBTSubjectGrantRec;
	TRS_SubjectsGrants* pSubjectGrantRec;
	TableUpdater::FindResult findResult;
	for(int i= 0; i <= pDBTEntitySubjectsGrants->GetUpperBound(); i++)
	{
		pDBTSubjectGrantRec = (TRS_SubjectsGrants*)pDBTEntitySubjectsGrants->GetRow(i);
		if (pDBTSubjectGrantRec)
		{
			TRY
			{
				findResult =  aTUSubjectsGrants.FindRecord(pDBTSubjectGrantRec->f_SubjectID, pDBTSubjectGrantRec->f_EntityName, pDBTSubjectGrantRec->f_RowSecurityID, TRUE);
				switch (findResult)
				{	
					case TableUpdater::FOUND:
						{
							//se il grant assegnato è di tipo E_GRANT_TYPE_DENY allora cancello il record dal db, visto che salvo solo i grant dati
							pSubjectGrantRec = aTUSubjectsGrants.GetRecord();

							if (pDBTSubjectGrantRec->f_GrantType == E_GRANT_TYPE_DENY)
								aTUSubjectsGrants.DeleteRecord();
							else
							{
								pSubjectGrantRec->f_GrantType = pDBTSubjectGrantRec->f_GrantType;
								pSubjectGrantRec->f_Inherited = pDBTSubjectGrantRec->f_Inherited;
								pSubjectGrantRec->f_IsImplicit = pDBTSubjectGrantRec->f_IsImplicit;
								aTUSubjectsGrants.UpdateRecord();
							}
							aTUSubjectsGrants.UnlockCurrent();
						}
						break;
					case TableUpdater::NOT_FOUND: 
						if (pDBTSubjectGrantRec->f_GrantType != E_GRANT_TYPE_DENY)
						{
							pSubjectGrantRec = aTUSubjectsGrants.GetRecord();
							pSubjectGrantRec->f_SubjectID = pDBTSubjectGrantRec->f_SubjectID;
							pSubjectGrantRec->f_WorkerID = pDBTSubjectGrantRec->f_WorkerID;
							pSubjectGrantRec->f_EntityName = pDBTSubjectGrantRec->f_EntityName;
							pSubjectGrantRec->f_GrantType = pDBTSubjectGrantRec->f_GrantType;
							pSubjectGrantRec->f_RowSecurityID =  pDBTSubjectGrantRec->f_RowSecurityID;
							pSubjectGrantRec->f_Inherited = pDBTSubjectGrantRec->f_Inherited;
							pSubjectGrantRec->f_IsImplicit = pDBTSubjectGrantRec->f_IsImplicit;;
							aTUSubjectsGrants.UpdateRecord(); 
						}
						break;
					case TableUpdater::LOCKED: break;		
				}
			}
			CATCH(SqlException, e)
			{
				aTUSubjectsGrants.UnlockAll();
				e->m_strError = cwsprintf(_TB("The error {0-%s} occurred saving the Row Security grants on SubjectID {1-%s} for entity {2-%s} and RowSecurityID {3-%s}"), 		
					e->m_strError, pDBTSubjectGrantRec->f_SubjectID.Str(), pDBTSubjectGrantRec->f_EntityName, pDBTSubjectGrantRec->f_RowSecurityID.Str());
				THROW(e);
			}
			END_CATCH		
		}
	}
}

//-----------------------------------------------------------------------------
void CGrantsManager::DeleteAllGrants(const CString& strEntityName, int rowSecurityID)
{
	CString strSQLText;
	strSQLText = cwsprintf(_T("DELETE FROM %s WHERE %s = %s AND %s = %s"),
		TRS_SubjectsGrants::GetStaticName(),
		TRS_SubjectsGrants::s_sEntityName,
		m_pSqlSession->m_pSqlConnection->NativeConvert(&DataStr(strEntityName)),
		TRS_SubjectsGrants::s_sRowSecurityID,
		m_pSqlSession->m_pSqlConnection->NativeConvert(&DataLng(rowSecurityID)));
	TRY
	{	
		//in questo la cancellazione fa parte della transazione del documento
		m_pSqlSession->GetSqlConnection()->ExecuteSQL(strSQLText, m_pSqlSession);
	
	}
		CATCH(SqlException, e)
	{
		m_pSqlSession->Abort();
		THROW(e);
	}
	END_CATCH
}

// query di esempio
// SELECT WorkerId,TitleCode,Name,LastName
// FROM OM_Workers 
// WHERE Disabled = 0 AND 
// WorkerID IN (SELECT DISTINCT W.WorkerId FROM OM_Workers W, OM_Masters M, RS_SubjectsGrants R 
// WHERE M.RowSecurityID = ? AND (M.IsProtected = '0' OR (M.RowSecurityID = R.RowSecurityID AND R.EntityName = 'CLIENT' AND M.RowSecurityID = R.RowSecurityID) AND W.WorkerId = R.WorkerID))
// ORDER BY WorkerId, Name,LastName

// metodo che aggiunge in coda alla strFilter di SqlTable un'ulteriore clausola di WHERE
// utilizzata per filtrare la combobox dei workers (HKLWorkersForRowSecurity)
// DE delle pratiche: in base al client indicato, le combobox devono contenere solo i workers 
// che hanno effettivamente un grant sul client
//-----------------------------------------------------------------------------
void CGrantsManager::AddFilterForEntity(SqlTable* pTable, const CString& strEntityName)
{
	RSEntityInfo* pInfo = AfxGetEntitiesManager()->GetEntityInfo(strEntityName);
	if (!pInfo || !pTable)
		return;

	DataLng aRowSecurityID;
	TRY
	{
		pTable->m_strFilter += cwsprintf(
			_TB(" AND RM_Workers.WorkerID IN (SELECT DISTINCT W.WorkerId FROM OM_Workers W, %s M LEFT OUTER JOIN RS_SubjectsGrants R ON M.RowSecurityID = R.RowSecurityID WHERE M.RowSecurityID = ? AND (M.IsProtected = '0' OR (M.RowSecurityID = R.RowSecurityID AND R.EntityName = %s AND M.RowSecurityID = R.RowSecurityID) AND W.WorkerId = R.WorkerID))"),
			pInfo->m_strMasterTable,
			m_pSqlSession->GetSqlConnection()->NativeConvert(&DataStr(strEntityName))
		);
	pTable->AddParam(szRowID, aRowSecurityID);

	}
		CATCH(SqlException, e)
	{
		THROW(e);
	}
	END_CATCH
}

// SetParamValue per l'hotlink HKLWorkersForRowSecurity
//-----------------------------------------------------------------------------
void CGrantsManager::SetRowSecurityIDParam(SqlTable* pTable, const DataLng& rowSecurityID)
{
	if (!pTable)
		return;

	pTable->SetParamValue(szRowID, rowSecurityID);
}

///////////////////////////////////////////////////////////////////////////////
//						CWorkerGrantRow declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CWorkerGrantRow::CWorkerGrantRow(CString sEntity, int nRowSecurityID)
	:
	m_Entity(sEntity),
	m_RowSecurityID(nRowSecurityID)
{
}

///////////////////////////////////////////////////////////////////////////////
//						CRSMaintenanceManager declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CRSMaintenanceManager::CRSMaintenanceManager
	(
	SqlSession*			pSession, 
	CSubjectsManager*	pSubjectsManager, 
	CGrantsManager*		pGrantsManager, 
	CEntitiesManager*	pEntitiesManager
	)
	: 
	m_pSqlSession		(pSession),
	m_pSubjectsManager	(pSubjectsManager),
	m_pGrantsManager	(pGrantsManager),
	m_pEntitiesManager	(pEntitiesManager)
{
	m_pOldHierarchyRowsArray			= new CRSHierarchyRowArray();
	m_pCurrentHierarchyRowsArray		= new CRSHierarchyRowArray();
	m_pOldRowsNotFoundInCurrentArray	= new CRSHierarchyRowArray();
	m_pCurrentRowsNotFoundInOldArray	= new CRSHierarchyRowArray();
	m_pSubjectsToRemoveArray			= new CSubjectCacheArray();
}

//-----------------------------------------------------------------------------
CRSMaintenanceManager::~CRSMaintenanceManager() 
{
	delete m_pOldHierarchyRowsArray;
	delete m_pCurrentHierarchyRowsArray;
	delete m_pOldRowsNotFoundInCurrentArray;
	delete m_pCurrentRowsNotFoundInOldArray;
	delete m_pSubjectsToRemoveArray;
}

// Manutenzione tabella RS_Subjects
// Cancellazione completa dei record della tabella RS_Subjects
//-----------------------------------------------------------------------------
void CRSMaintenanceManager::DeleteAllSubjects()
{
	DeleteAllFromTable(TRS_Subjects::GetStaticName());
}

// Manutenzione tabella RS_Subjects: inserimento/cancellazione subjects
//-----------------------------------------------------------------------------
void CRSMaintenanceManager::ManageSubjects(CRSResourcesArray* pResources)
{
	m_pSubjectsToRemoveArray->RemoveAll();

	TRS_Subjects aRecSubject;
	SqlTable aTblSubjects(&aRecSubject, m_pSqlSession);

	TRY
	{
		aTblSubjects.Open(TRUE);
		aTblSubjects.SelectAll();
		m_pSqlSession->StartTransaction();
		aTblSubjects.Query();

		if (aTblSubjects.GetRowSetCount())
		{
			while (!aTblSubjects.IsEOF())
			{
				// se nella tabella RS_Subjects ci sono dei record vado a cercare il corrispondente elemento nell'array delle risorse
				int pos = GetSubjectElementPos(aRecSubject, pResources);
				if (pos > -1)
					pResources->RemoveAt(pos); // se l'elemento esiste nell'array lo rimuovo, perche' esiste gia' in tabella
				else
					// se invece non trovo l'elemento nell'array si tratta di un "cadavere" nella tabella, non rimuovo il record 
					// nella RS_Subjects, ma mi tengo da parte le sue info
					// N.B. faccio una copia dell'oggetto CSubjectCache, perche' la chiamata poco qui sotto della LoadSubjects mi
					// pulisce la memoria e diventa un bad ptr! 
					// da capire se la new del nuovo oggetto si porta dietro anche gli slavesubjects!!!
					m_pSubjectsToRemoveArray->Add(new CSubjectCache(*m_pSubjectsManager->GetSubjectCache(aRecSubject.f_SubjectID)));

				aTblSubjects.MoveNext();
			}
		}

		// per tutti gli elementi rimasti nell'array faccio la Add del record nella tabella
		AddSubjects(aTblSubjects, aRecSubject, pResources);

		aTblSubjects.Close();
		m_pSqlSession->Commit();

		// alla fine ricarico le info dei subjects nella cache
		m_pSubjectsManager->LoadSubjects(); 
	}
	CATCH(SqlException, e)
	{
		if (aTblSubjects.IsOpen()) aTblSubjects.Close();
		m_pSqlSession->Abort();
		THROW(e);
	}
	END_CATCH
}

// Manutenzione tabella RS_Subjects: inserimento subjects
//-----------------------------------------------------------------------------
void CRSMaintenanceManager::AddSubjects(SqlTable& aTblSubjects, TRS_Subjects& aRecSubject, CRSResourcesArray* pResources)
{
	TRY
	{
		CRSResourceElement* elem;
		for (int i=0; i <= pResources->GetUpperBound(); i++)
		{
			aTblSubjects.AddNew();
			elem = pResources->GetAt(i);
			aRecSubject.f_IsWorker = elem->m_IsWorker;
			aRecSubject.f_ResourceType = elem->m_ResourceType;
			aRecSubject.f_ResourceCode = elem->m_ResourceCode;
			aRecSubject.f_WorkerID = elem->m_WorkerID;
			aRecSubject.f_Description = elem->m_Description;
			aTblSubjects.Update();
		}
	}
	CATCH(SqlException, e)
	{
		THROW(e);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
int CRSMaintenanceManager::GetSubjectElementPos(TRS_Subjects& aRecSubject, CRSResourcesArray* pResources)
{
	CRSResourceElement* elem;
	for (int i=0; i <= pResources->GetUpperBound(); i++)
	{
		elem = pResources->GetAt(i);
		if (aRecSubject.f_IsWorker)
		{
			if (elem->m_WorkerID == aRecSubject.f_WorkerID)
				return i;
		}
		else
		{
			if (
				elem->m_ResourceCode == aRecSubject.f_ResourceCode &&
				elem->m_ResourceType == aRecSubject.f_ResourceType 
				)
				return i;
		}
	}
	return -1;
}

// Manutenzione tabella RS_SubjectsHierarchy
//-----------------------------------------------------------------------------
void CRSMaintenanceManager::ManageHierarchies(CHierarchyArray* pHierarchies) 
{
	BOOL bResult = FALSE;

	TRY
	{
		bResult =	PurgeHierarchies() // elimino tutti i record dalla tabella
					&& 
					FillHierarchies(pHierarchies); // ricalcolo le hierarchies

		// se e' andato tutto a buon fine 
		// ricarico le info delle hierarchies nella cache
		if (bResult)
			m_pSubjectsManager->LoadHierarchies();
	}
	CATCH(SqlException, e)
	{
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		m_pSqlSession->Abort();
		THROW(e);
	}
	END_CATCH
}

// Eliminazione preventiva di tutti i record dalla tabella RS_SubjectsHierarchy
// previo salvataggio informazioni in una struttura in memoria 
// e nella tabella temporanea RS_TmpOldHierarchies
//-----------------------------------------------------------------------------
BOOL CRSMaintenanceManager::PurgeHierarchies()
{
	TRS_SubjectsHierarchy aRecH;
	SqlTable aTblH(&aRecH, m_pSqlSession);

	TRS_TmpOldHierarchies aRecTmp;
	SqlTable aTblTmp(&aRecTmp, m_pSqlSession);

	TRY
	{
		BOOL bExistHierarchyRows = ExistRowsInTable(TRS_SubjectsHierarchy::GetStaticName());
		BOOL bExistTmpRows = ExistRowsInTable(TRS_TmpOldHierarchies::GetStaticName());

		// se entrambe le tabelle sono vuote ritorno subito
		if (!bExistHierarchyRows && !bExistTmpRows)
			return TRUE;

		// se la tabella delle hierarchies e' vuota e nella temporanea ci sono dei record
		// si tratta di un caso anomalo: svuoto anche la tabella temporanea
		if (!bExistHierarchyRows && bExistTmpRows)
		{
			DeleteAllFromTable(TRS_TmpOldHierarchies::GetStaticName());
			return TRUE;
		}

		aTblH.Open();
		aTblH.Select(aRecH.f_MasterSubjectID);
		aTblH.Select(aRecH.f_SlaveSubjectID);
		aTblH.Select(aRecH.f_NrLevel);
		aTblH.Query();

		aTblTmp.Open(TRUE);
		aTblTmp.SelectAll();
		m_pSqlSession->StartTransaction();
		aTblTmp.Query();

		while(!aTblH.IsEOF()) 
		{
			// riempio anche la tabella temporanea con le old hierarchies (solo se la tabella e' vuota)
			if (!bExistTmpRows)
			{
				aTblTmp.AddNew();
				aRecTmp.f_MasterSubjectID = aRecH.f_MasterSubjectID;
				aRecTmp.f_SlaveSubjectID = aRecH.f_SlaveSubjectID;
				aRecTmp.f_NrLevel = aRecH.f_NrLevel;
				aTblTmp.Update();
			}
			aTblH.MoveNext();
		}

		m_pSqlSession->Commit();
		aTblH.Close();

		// rifaccio la query sulla temporanea
		aTblTmp.Query();
		m_pOldHierarchyRowsArray->RemoveAll();

		while(!aTblTmp.IsEOF()) 
		{
			// riempio una struttura con le informazioni delle old hierarchies, lette dalla tabella temporanea.
			// i suoi record infatti: 1) o sono stati appena inseriti, 2) o sono gia' esistenti
			m_pOldHierarchyRowsArray->Add(aRecTmp.f_MasterSubjectID, aRecTmp.f_SlaveSubjectID, aRecTmp.f_NrLevel);
			aTblTmp.MoveNext();
		}
		aTblTmp.Close();

		// alla fine elimino tutti i record dalla tabella RS_SubjectsHierarchy
		DeleteAllFromTable(TRS_SubjectsHierarchy::GetStaticName());
	}
	CATCH(SqlException, e)	
	{
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		if (aTblH.IsOpen()) aTblH.Close();
		if (aTblTmp.IsOpen()) aTblTmp.Close();
		m_pSqlSession->Abort();
		THROW(e);
	}
	END_CATCH

	return TRUE;
}

// Riempimento tabella RS_SubjectsHierarchy e relativa struttura in memoria
//-----------------------------------------------------------------------------
BOOL CRSMaintenanceManager::FillHierarchies(CHierarchyArray* pHierarchies)
{
	TRS_SubjectsHierarchy aRecH;
	SqlTable aTblH(&aRecH, m_pSqlSession);

	TRY
	{
		aTblH.Open(TRUE);
		aTblH.SelectAll();
		m_pSqlSession->StartTransaction();
		aTblH.Query();

		m_pCurrentHierarchyRowsArray->RemoveAll();

		CHierarchyElement* pElement;
		CSubjectCache* pMasterSubject;
		CSubjectCache* pChildSubject;

		for(int i=0; i <= pHierarchies->GetUpperBound(); i++)
		{
			pElement = pHierarchies->GetAt(i);

			pMasterSubject = (pElement->m_pMasterElement->m_IsWorker)
				? m_pSubjectsManager->GetSubjectCacheFromWorkerID(pElement->m_pMasterElement->m_WorkerID)
				: m_pSubjectsManager->GetSubjectCacheFromResource(pElement->m_pMasterElement->m_ResourceType, pElement->m_pMasterElement->m_ResourceCode);

			pChildSubject = (pElement->m_pSlaveElement->m_IsWorker)
				? m_pSubjectsManager->GetSubjectCacheFromWorkerID(pElement->m_pSlaveElement->m_WorkerID)
				: m_pSubjectsManager->GetSubjectCacheFromResource(pElement->m_pSlaveElement->m_ResourceType, pElement->m_pSlaveElement->m_ResourceCode);

			if (!pMasterSubject || !pChildSubject || pMasterSubject->m_SubjectID <= 0 || pChildSubject->m_SubjectID <= 0)
				continue;

			// aggiungo il nuovo record
			aTblH.AddNew();
			aRecH.f_MasterSubjectID = pMasterSubject->m_SubjectID;
			aRecH.f_SlaveSubjectID = pChildSubject->m_SubjectID;
			aRecH.f_NrLevel = pElement->m_nrLevel;
			aTblH.Update();

			m_pCurrentHierarchyRowsArray->Add(aRecH.f_MasterSubjectID, aRecH.f_SlaveSubjectID, aRecH.f_NrLevel);
		}
		
		m_pSqlSession->Commit();
		aTblH.Close();
	}
	CATCH(SqlException, e)
	{
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		if (aTblH.IsOpen()) aTblH.Close();
		m_pSqlSession->Abort();
		THROW(e);
	}
	END_CATCH
	
	return TRUE;
}

// metodo che ritorna se sono presenti o meno record nella tabella passata come parametro
//-----------------------------------------------------------------------------
BOOL CRSMaintenanceManager::ExistRowsInTable(const CString& strTableName)
{
	if (!m_pSqlSession || !m_pSqlSession->m_pSqlConnection)
		return FALSE;
	
	return !m_pSqlSession->m_pSqlConnection->IsEmptyTable(strTableName);
}

// metodo generico che cancella tutti i record nella tabella passata come parametro
//-----------------------------------------------------------------------------
BOOL CRSMaintenanceManager::DeleteAllFromTable(const CString& strTableName)
{
	TRY
	{
		m_pSqlSession->StartTransaction();
		m_pSqlSession->GetSqlConnection()->ExecuteSQL(cwsprintf(_T("DELETE FROM %s"), strTableName));
		m_pSqlSession->Commit();
	}
	CATCH(SqlException, e)
	{
		m_pSqlSession->Abort();
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		return FALSE;
	}
	END_CATCH

	return TRUE;
}

// Valorizza il campo RowSecurityID delle master table delle entità previste dalla RowSecurity
//-----------------------------------------------------------------------------
BOOL CRSMaintenanceManager::ValorizeRowSecurityID()
{
	return m_pEntitiesManager->ValorizeRowSecurityID();
}

// manutenzione dei dati nella tabella RS_SubjectsGrants, sulla base dei cambiamenti
// rilevati nelle hierarchies dei subjects (oldCache e currentCache)
//-----------------------------------------------------------------------------
BOOL CRSMaintenanceManager::ManageSubjectsGrants() 
{
	m_pOldRowsNotFoundInCurrentArray->RemoveAll();
	m_pCurrentRowsNotFoundInOldArray->RemoveAll();

	CompareHierarchyRowArray();

	BOOL bResult = TRUE;

	if (m_pOldRowsNotFoundInCurrentArray->GetSize() > 0)
		bResult = bResult && ManageOldSubjectsGrants(); // elimino i vecchi grant

	if (bResult && m_pCurrentRowsNotFoundInOldArray->GetSize() > 0)
		bResult = bResult && ManageNewSubjectsGrants(); // aggiungo i nuovi grant

	if (bResult && m_pSubjectsToRemoveArray->GetSize() > 0)
		RemoveGrantsForDeadSubjects();

	if (bResult) // alla fine dell'elaborazione elimino tutti i record dalla tabella RS_TmpOldHierarchies
		bResult = bResult && DeleteAllFromTable(TRS_TmpOldHierarchies::GetStaticName());

	return bResult;
}

// Esegue il confronto tra i dati presenti nella tabella RS_SubjectsHierarchy per individuare 
// i record uguali, che vengono marcati con il flag m_bVisited = TRUE
// Dopo analizzo le hierarchies dei record rimasti con m_bVisited = FALSE
//-----------------------------------------------------------------------------
void CRSMaintenanceManager::CompareHierarchyRowArray()
{
	CRSHierarchyRow* pOldRow = NULL;
	CRSHierarchyRow* pCurrentRow = NULL;

	// per ogni elemento delle current hierarchies vado a cercare il medesimo record
	// negli elementi nell'array delle oldhierarchies
	// se lo trovo segno gli elementi di entrambi gli array come m_bVisited = TRUE
	for (int i=0; i <= m_pCurrentHierarchyRowsArray->GetUpperBound(); i++)
	{
		pCurrentRow = m_pCurrentHierarchyRowsArray->GetAt(i);

		for (int y=0; y <= m_pOldHierarchyRowsArray->GetUpperBound(); y++)
		{
			pOldRow = m_pOldHierarchyRowsArray->GetAt(y);
			if (pCurrentRow->Match(pOldRow))
			{
				pOldRow->m_bVisited = pCurrentRow->m_bVisited = TRUE;
				break;
			}
		}
	}

	// analizzo le righe delle old hierarchies con m_bVisited = FALSE
	AnalyzeOldHierarchiesRows();
	// analizzo le righe delle current hierarchies con m_bVisited = FALSE
	AnalyzeCurrentHierarchiesRows();
}

// Analisi degli elementi non visitati appartenenti alle vecchie gerarchie
// per individuare gli slave e i master NON presenti nelle nuove gerarchie
// e li tengo da parte in un array apposito
//-----------------------------------------------------------------------------
void CRSMaintenanceManager::AnalyzeOldHierarchiesRows()
{
	CSubjectCache*		pMasterSubjectCache = NULL;
	CSubjectCache*		pSlaveSubjectCache = NULL;
	CSubjectHierarchy*	pSubjectHierarchy = NULL;
	CRSHierarchyRow*	pOldRow = NULL;
	CRSHierarchyRow*	pHierarchyRowChanged = NULL;

	// ora vado ad analizzare gli elementi rimasti con m_bVisited = FALSE
	// inizio dall'array delle old hierarchies
	for (int i=0; i <= m_pOldHierarchyRowsArray->GetUpperBound(); i++)
	{
		pOldRow = m_pOldHierarchyRowsArray->GetAt(i);

		if (pOldRow->m_bVisited)
			continue; // se il record e' visitato lo skippo

		// per ogni elemento non visitato leggo i suoi mastersubjects e i suoi slavesubjects dalla old cache
		pMasterSubjectCache = m_pSubjectsManager->GetOldSubjectCache(pOldRow->m_MasterSubjectID);
		pSlaveSubjectCache = m_pSubjectsManager->GetOldSubjectCache(pOldRow->m_SlaveSubjectID);

		// analizzo i mastersubjects e mi tengo da parte le righe che NON esistono nell'array delle current hierarchies
		if (pMasterSubjectCache && pMasterSubjectCache->m_pMasterSubjects && pMasterSubjectCache->m_pMasterSubjects->GetSize() > 0)
		{
			for (int y=0; y <= pMasterSubjectCache->m_pMasterSubjects->GetUpperBound(); y++)
			{
				pSubjectHierarchy = (CSubjectHierarchy*)pMasterSubjectCache->m_pMasterSubjects->GetAt(y);
				if (!pSubjectHierarchy) continue;
				pHierarchyRowChanged = m_pCurrentHierarchyRowsArray->GetElement(pSubjectHierarchy->m_pSubject->m_SubjectID, pOldRow->m_MasterSubjectID, pSubjectHierarchy->m_nrLevel);
				if (pHierarchyRowChanged) continue;
				// se non l'ho trovato lo tengo da parte
				m_pOldRowsNotFoundInCurrentArray->Add(pSubjectHierarchy->m_pSubject->m_SubjectID, pOldRow->m_MasterSubjectID, pSubjectHierarchy->m_nrLevel);
			}
		}
		if (pMasterSubjectCache && pMasterSubjectCache->m_pSlaveSubjects && pMasterSubjectCache->m_pSlaveSubjects->GetSize() > 0)
		{
			for (int y=0; y <= pMasterSubjectCache->m_pSlaveSubjects->GetUpperBound(); y++)
			{
				pSubjectHierarchy = (CSubjectHierarchy*)pMasterSubjectCache->m_pSlaveSubjects->GetAt(y);
				if (!pSubjectHierarchy) continue;
				pHierarchyRowChanged = m_pCurrentHierarchyRowsArray->GetElement(pOldRow->m_MasterSubjectID, pSubjectHierarchy->m_pSubject->m_SubjectID, pSubjectHierarchy->m_nrLevel);
				if (pHierarchyRowChanged) continue;
				// se non l'ho trovato lo tengo da parte
				m_pOldRowsNotFoundInCurrentArray->Add(pOldRow->m_MasterSubjectID, pSubjectHierarchy->m_pSubject->m_SubjectID, pSubjectHierarchy->m_nrLevel);
			}
		}

		// analizzo gli slavesubjects e mi tengo da parte le righe che NON esistono nell'array delle current hierarchies
		if (pSlaveSubjectCache && pSlaveSubjectCache->m_pMasterSubjects && pSlaveSubjectCache->m_pMasterSubjects->GetSize() > 0)
		{
			for (int y=0; y <= pSlaveSubjectCache->m_pMasterSubjects->GetUpperBound(); y++)
			{
				pSubjectHierarchy = (CSubjectHierarchy*)pSlaveSubjectCache->m_pMasterSubjects->GetAt(y);
				if (!pSubjectHierarchy) continue;
				pHierarchyRowChanged = m_pCurrentHierarchyRowsArray->GetElement(pSubjectHierarchy->m_pSubject->m_SubjectID, pOldRow->m_SlaveSubjectID, pSubjectHierarchy->m_nrLevel);
				if (pHierarchyRowChanged) continue;
				// se non l'ho trovato lo tengo da parte
				m_pOldRowsNotFoundInCurrentArray->Add(pSubjectHierarchy->m_pSubject->m_SubjectID, pOldRow->m_SlaveSubjectID, pSubjectHierarchy->m_nrLevel);
			}
		}
		if (pSlaveSubjectCache && pSlaveSubjectCache->m_pSlaveSubjects && pSlaveSubjectCache->m_pSlaveSubjects->GetSize() > 0)
		{
			for (int y=0; y <= pSlaveSubjectCache->m_pSlaveSubjects->GetUpperBound(); y++)
			{
				pSubjectHierarchy = (CSubjectHierarchy*)pSlaveSubjectCache->m_pSlaveSubjects->GetAt(y);
				if (!pSubjectHierarchy) continue;
				pHierarchyRowChanged = m_pCurrentHierarchyRowsArray->GetElement(pOldRow->m_SlaveSubjectID, pSubjectHierarchy->m_pSubject->m_SubjectID, pSubjectHierarchy->m_nrLevel);
				if (pHierarchyRowChanged) continue;
				// se non l'ho trovato lo tengo da parte
				m_pOldRowsNotFoundInCurrentArray->Add(pOldRow->m_SlaveSubjectID, pSubjectHierarchy->m_pSubject->m_SubjectID, pSubjectHierarchy->m_nrLevel);
			}
		}
	}
}

// Analisi degli elementi non visitati appartenenti alle nuove gerarchie
// per individuare gli slave e i master NON presenti nelle vecchie gerarchie
// e li tengo da parte in un array apposito
//-----------------------------------------------------------------------------
void CRSMaintenanceManager::AnalyzeCurrentHierarchiesRows()
{
	CRSHierarchyRow* pCurrentRow = NULL;

	// ora vado ad analizzare gli elementi rimasti con m_bVisited = FALSE
	// inizio dall'array delle current hierarchies
	for (int i=0; i <= m_pCurrentHierarchyRowsArray->GetUpperBound(); i++)
	{
		pCurrentRow = m_pCurrentHierarchyRowsArray->GetAt(i);

		if (pCurrentRow->m_bVisited)
			continue; // se il record e' visitato lo skippo

		m_pCurrentRowsNotFoundInOldArray->Add(pCurrentRow->m_MasterSubjectID, pCurrentRow->m_SlaveSubjectID, pCurrentRow->m_NrLevel);
	}
}

// nuova gestione per la manutenzione dei grants dei subjects
// non viene fatta distizione tra gli impliciti e gli espliciti
// non esiste piu' la propagazione tra fratelli di livello 1 e verso i padri in cascata
// quindi si tratta di eliminare i soli grant ereditati da una risorsa di livello 1
//-----------------------------------------------------------------------------
BOOL CRSMaintenanceManager::ManageOldSubjectsGrants()
{
	TRS_SubjectsGrants	aRecMasterSubjectGrants;
	SqlTable			aTblMasterSubjectGrants(&aRecMasterSubjectGrants, m_pSqlSession);

	TRS_SubjectsGrants	aRecSubjectsGrants;
	SqlTable			aTblSubjectsGrants(&aRecSubjectsGrants, m_pSqlSession);

	CRSHierarchyRow*	pRow			= NULL;
	CSubjectCache*		pMasterSubject	= NULL;
	CSubjectCache*		pSlaveSubject	= NULL;

	TRY
	{
		// SqlTable per individuare i grants della risorsa da eliminare
		aTblMasterSubjectGrants.Open();
		aTblMasterSubjectGrants.SelectAll();
		aTblMasterSubjectGrants.AddParam(szP5, aRecMasterSubjectGrants.f_SubjectID);	
		aTblMasterSubjectGrants.AddFilterColumn(aRecMasterSubjectGrants.f_SubjectID);

		// SqlTable per individuare i grants ereditati di uno specifico worker
		aTblSubjectsGrants.Open();
		aTblSubjectsGrants.SelectAll();
		aTblSubjectsGrants.AddParam(szP1, aRecSubjectsGrants.f_SubjectID);	
		aTblSubjectsGrants.AddParam(szP2, aRecSubjectsGrants.f_EntityName);	
		aTblSubjectsGrants.AddParam(szP3, aRecSubjectsGrants.f_RowSecurityID);
		aTblSubjectsGrants.AddParam(szP4, aRecSubjectsGrants.f_Inherited);
		aTblSubjectsGrants.AddFilterColumn(aRecSubjectsGrants.f_SubjectID);
		aTblSubjectsGrants.AddFilterColumn(aRecSubjectsGrants.f_EntityName);
		aTblSubjectsGrants.AddFilterColumn(aRecSubjectsGrants.f_RowSecurityID);
		aTblSubjectsGrants.AddFilterColumn(aRecSubjectsGrants.f_Inherited);

		// scorro gli elementi delle old hierarchies che non sono piu' presenti nelle current hierarchies
		for (int i=0; i <= m_pOldRowsNotFoundInCurrentArray->GetUpperBound(); i++)
		{
			pRow = m_pOldRowsNotFoundInCurrentArray->GetAt(i);
		
			pMasterSubject = m_pSubjectsManager->GetOldSubjectCache(pRow->m_MasterSubjectID);
			pSlaveSubject =  m_pSubjectsManager->GetOldSubjectCache(pRow->m_SlaveSubjectID);

			// se il master e' una risorsa ed il livello e' pari a 1
			if (!pMasterSubject->m_pResourceElement->m_IsWorker && pRow->m_NrLevel == 1)
			{
				if (pSlaveSubject->m_pResourceElement->m_IsWorker)
				{
					aTblMasterSubjectGrants.SetParamValue(szP5, DataLng(pMasterSubject->m_SubjectID));
					aTblMasterSubjectGrants.Query();

					while (!aTblMasterSubjectGrants.IsEOF())
					{
						aTblSubjectsGrants.SetParamValue(szP1, DataLng(pSlaveSubject->m_SubjectID));	
						aTblSubjectsGrants.SetParamValue(szP2, aRecMasterSubjectGrants.f_EntityName);	
						aTblSubjectsGrants.SetParamValue(szP3, aRecMasterSubjectGrants.f_RowSecurityID);
						aTblSubjectsGrants.SetParamValue(szP4, DataBool(TRUE));
						aTblSubjectsGrants.Query();

						while (!aTblSubjectsGrants.IsEOF())
						{
							//procedo ad eliminare il grant sul worker
							RemoveGrant(aRecSubjectsGrants.f_SubjectID, aRecSubjectsGrants.f_EntityName, aRecSubjectsGrants.f_RowSecurityID);
							aTblSubjectsGrants.MoveNext();
						}

						aTblMasterSubjectGrants.MoveNext();
					}
				}
			}
		}

		aTblSubjectsGrants.Close();
		aTblMasterSubjectGrants.Close();
	}
	CATCH(SqlException,e)
	{
		m_pSqlSession->Abort();
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		if (aTblSubjectsGrants.IsOpen()) aTblSubjectsGrants.Close();
		if (aTblMasterSubjectGrants.IsOpen()) aTblMasterSubjectGrants.Close();
		return FALSE;
	}
	END_CATCH

	return TRUE;
}

// nuova gestione per la manutenzione dei grants dei subjects
// non viene fatta distizione tra gli impliciti e gli espliciti
// non esiste piu' la propagazione tra fratelli di livello 1 e verso i padri in cascata
// quindi si tratta di aggiungere i soli grant ereditati da una risorsa di livello 1 (se non gia' attribuiti al worker)
//-----------------------------------------------------------------------------
BOOL CRSMaintenanceManager::ManageNewSubjectsGrants()
{
	TRS_SubjectsGrants	aRecSubjectsGrants;
	SqlTable			aTblSubjectsGrants(&aRecSubjectsGrants, m_pSqlSession);

	CSubjectCache*		pMasterSubject	= NULL;
	CSubjectCache*		pSlaveSubject	= NULL;
	CRSHierarchyRow*	pRow			= NULL;

	TRY
	{
		// SqlTable utilizzata per individuare i grants ereditati di una specifica risorsa
		aTblSubjectsGrants.Open();
		aTblSubjectsGrants.SelectAll();
		aTblSubjectsGrants.AddParam(szP1, aRecSubjectsGrants.f_SubjectID);	
		aTblSubjectsGrants.AddFilterColumn(aRecSubjectsGrants.f_SubjectID);

		// scorro gli elementi delle current hierarchies che non sono presenti nelle old hierarchies
		for (int i=0; i <= m_pCurrentRowsNotFoundInOldArray->GetUpperBound(); i++)
		{
			pRow = m_pCurrentRowsNotFoundInOldArray->GetAt(i);

			pMasterSubject = m_pSubjectsManager->GetSubjectCache(pRow->m_MasterSubjectID);
			pSlaveSubject =  m_pSubjectsManager->GetSubjectCache(pRow->m_SlaveSubjectID);
			
			// se il master e' una risorsa ed il livello e' pari a 1
			if (!pMasterSubject->m_pResourceElement->m_IsWorker && pRow->m_NrLevel == 1)
			{
				// se lo slave e' un worker 
				if (pSlaveSubject->m_pResourceElement->m_IsWorker)
				{
					// leggo i grants assegnati alla risorsa padre
					aTblSubjectsGrants.SetParamValue(szP1, DataLng(pMasterSubject->m_SubjectID));
					aTblSubjectsGrants.Query();

					while (!aTblSubjectsGrants.IsEOF())
					{
						//devo andare ad assegnare i grant della risorsa allo slave worker, solo se non gia' esistenti, con il flag IsInheritable = TRUE
						AddGrant(pSlaveSubject->m_SubjectID, aRecSubjectsGrants.f_EntityName, aRecSubjectsGrants.f_RowSecurityID, pSlaveSubject->GetWorkerID(), aRecSubjectsGrants.f_GrantType);
						aTblSubjectsGrants.MoveNext();
					}
				}
			}
		}

		aTblSubjectsGrants.Close();
	}
	CATCH(SqlException,e)
	{
		if (aTblSubjectsGrants.IsOpen()) aTblSubjectsGrants.Close();
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		return FALSE;
	}
	END_CATCH

	return TRUE;
}

// inserisce un grant con il flag IsInheritable = TRUE
//-----------------------------------------------------------------------------
void CRSMaintenanceManager::AddGrant(int nSubjectID, const CString& strEntityName, int nRowSecurityID, int nWorkerID, const DataEnum& eGrantType)
{
	TURS_SubjectsGrants subjectsGrantsTU;
	subjectsGrantsTU.SetSqlSession(m_pSqlSession);
	subjectsGrantsTU.SetAutocommit();

	TRS_SubjectsGrants* subjectGrantRec;

	TRY
	{
		if (subjectsGrantsTU.FindRecord(DataLng(nSubjectID), DataStr(strEntityName), DataLng(nRowSecurityID), FALSE) == TableUpdater::NOT_FOUND)
		{		
			subjectGrantRec = subjectsGrantsTU.GetRecord();
			subjectGrantRec->f_SubjectID		= nSubjectID;
			subjectGrantRec->f_EntityName		= strEntityName;
			subjectGrantRec->f_RowSecurityID	= nRowSecurityID;
			subjectGrantRec->f_GrantType		= eGrantType;
			subjectGrantRec->f_Inherited		= TRUE;
			subjectGrantRec->f_IsImplicit		= FALSE;
			subjectGrantRec->f_WorkerID			= nWorkerID;
			subjectsGrantsTU.UpdateRecord();
		}
	}
	CATCH(SqlException,e)
	{
		m_pSqlSession->Abort();
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		THROW(e);
	}
	END_CATCH
}

// elimina un grant non piu' valido
//-----------------------------------------------------------------------------
void CRSMaintenanceManager::RemoveGrant(int nSubjectID, const CString& strEntityName, int nRowSecurityID)
{
	TURS_SubjectsGrants subjectsGrantsTU;
	subjectsGrantsTU.SetSqlSession(m_pSqlSession);
	subjectsGrantsTU.SetAutocommit();

	TRY
	{
		if (subjectsGrantsTU.FindRecord(DataLng(nSubjectID), DataStr(strEntityName), DataLng(nRowSecurityID), TRUE) == TableUpdater::FOUND)
			subjectsGrantsTU.DeleteRecord();

		subjectsGrantsTU.UnlockAll();
	}
	CATCH(SqlException,e)
	{
		m_pSqlSession->Abort();
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		THROW(e);
	}
	END_CATCH
}

// elimina i grants per i subjects che sono stati eliminati dall'organigramma
// N.B. non mi posso basare sulla struttura delle gerarchie che uso per il ricalcolo 
// dei grants perche' i subject senza padri non vi compaiono!!!
//-----------------------------------------------------------------------------
void CRSMaintenanceManager::RemoveGrantsForDeadSubjects()
{
	TRS_SubjectsGrants	aRecSubjectToRemoveGrants;
	SqlTable			aTblSubjectToRemoveGrants(&aRecSubjectToRemoveGrants, m_pSqlSession);

	TRS_SubjectsGrants	aRecSubjectsGrants;
	SqlTable			aTblSubjectsGrants(&aRecSubjectsGrants, m_pSqlSession);

	CSubjectCache* pSubjectToRemove = NULL;
	CSubjectHierarchy* pSlaveSubject = NULL;
	CSubjectHierarchy* pMasterSubject = NULL;

	BOOL bDelete = TRUE;

	TRY
	{
		// SqlTable per individuare i grants della risorsa da eliminare
		aTblSubjectToRemoveGrants.Open();
		aTblSubjectToRemoveGrants.SelectAll();
		aTblSubjectToRemoveGrants.AddParam(szP5, aRecSubjectToRemoveGrants.f_SubjectID);	
		aTblSubjectToRemoveGrants.AddFilterColumn(aRecSubjectToRemoveGrants.f_SubjectID);

		// SqlTable per individuare i grants ereditati di uno specifico worker
		aTblSubjectsGrants.Open();
		aTblSubjectsGrants.SelectAll();
		aTblSubjectsGrants.AddParam(szP1, aRecSubjectsGrants.f_SubjectID);	
		aTblSubjectsGrants.AddParam(szP2, aRecSubjectsGrants.f_EntityName);	
		aTblSubjectsGrants.AddParam(szP3, aRecSubjectsGrants.f_RowSecurityID);
		aTblSubjectsGrants.AddParam(szP4, aRecSubjectsGrants.f_Inherited);
		aTblSubjectsGrants.AddFilterColumn(aRecSubjectsGrants.f_SubjectID);
		aTblSubjectsGrants.AddFilterColumn(aRecSubjectsGrants.f_EntityName);
		aTblSubjectsGrants.AddFilterColumn(aRecSubjectsGrants.f_RowSecurityID);
		aTblSubjectsGrants.AddFilterColumn(aRecSubjectsGrants.f_Inherited);

		for (int i=0; i <= m_pSubjectsToRemoveArray->GetUpperBound(); i++)
		{
			pSubjectToRemove = m_pSubjectsToRemoveArray->GetAt(i);
			if (!pSubjectToRemove) continue;

			// se il subject e' un worker elimino d'ufficio tutti i suoi grants
			if (pSubjectToRemove->IsWorker())
				DeleteGrantsForSubject(pSubjectToRemove->m_SubjectID);
			else
			{
				// se il subject da eliminare e' una risorsa ed ha dei figli devo:
				// 1. leggere le info dei suoi grants
				// 2. se ha dei figli devo prima rimuovere i loro grants
				// 3. poi elimino d'ufficio tutti i suoi grants
				if (pSubjectToRemove->m_pSlaveSubjects && pSubjectToRemove->m_pSlaveSubjects->GetSize() > 0)
				{
					aTblSubjectToRemoveGrants.SetParamValue(szP5, DataLng(pSubjectToRemove->m_SubjectID));
					aTblSubjectToRemoveGrants.Query();

					while (!aTblSubjectToRemoveGrants.IsEOF())
					{
						for (int j=0; j <= pSubjectToRemove->m_pSlaveSubjects->GetUpperBound(); j++)
						{
							pSlaveSubject = (CSubjectHierarchy*)pSubjectToRemove->m_pSlaveSubjects->GetAt(j);
							// skippo lo slave se il livello e' > 1 oppure se e' una risorsa
							if (pSlaveSubject->m_nrLevel > 1 || !pSlaveSubject->m_pSubject->IsWorker()) 
								continue;

							aTblSubjectsGrants.SetParamValue(szP1, DataLng(pSubjectToRemove->m_SubjectID));	
							aTblSubjectsGrants.SetParamValue(szP2, aRecSubjectToRemoveGrants.f_EntityName);	
							aTblSubjectsGrants.SetParamValue(szP3, aRecSubjectToRemoveGrants.f_RowSecurityID);
							aTblSubjectsGrants.SetParamValue(szP4, DataBool(TRUE));
							aTblSubjectsGrants.Query();

							while (!aTblSubjectsGrants.IsEOF())
							{
								//procedo ad eliminare il grant sul worker
								RemoveGrant(aRecSubjectsGrants.f_SubjectID, aRecSubjectsGrants.f_EntityName, aRecSubjectsGrants.f_RowSecurityID);
								aTblSubjectsGrants.MoveNext();
							}
						}

						aTblSubjectToRemoveGrants.MoveNext();
					}
				}

				DeleteGrantsForSubject(pSubjectToRemove->m_SubjectID);
			}

			// ora devo procedere ad eliminare il subject dalle anagrafiche RS_Subjects e RS_SubjectsHierarchy
			DeleteSubjectAndHierarchies(pSubjectToRemove->m_SubjectID);
		}

		aTblSubjectsGrants.Close();
		aTblSubjectToRemoveGrants.Close();
		
		// alla fine ricarico le info dei subjects e delle hierarchies nella cache
		m_pSubjectsManager->LoadInformation();
	}
	CATCH(SqlException,e)
	{
		m_pSqlSession->Abort();
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		if (aTblSubjectsGrants.IsOpen()) aTblSubjectsGrants.Close();
		if (aTblSubjectToRemoveGrants.IsOpen()) aTblSubjectToRemoveGrants.Close();
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void CRSMaintenanceManager::DeleteGrantsForSubject(int nSubjectID)
{
	CString strSQLText = cwsprintf
		(
			_T("DELETE FROM %s WHERE %s = %s"), 
			TRS_SubjectsGrants::GetStaticName(),
			TRS_SubjectsGrants::s_sSubjectID, 
			m_pSqlSession->m_pSqlConnection->NativeConvert(&DataLng(nSubjectID))
		);

	TRY
	{
		m_pSqlSession->StartTransaction();
		m_pSqlSession->GetSqlConnection()->ExecuteSQL(strSQLText);
		m_pSqlSession->Commit();
	}
	CATCH(SqlException, e)
	{
		m_pSqlSession->Abort();
		THROW(e);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void CRSMaintenanceManager::DeleteSubjectAndHierarchies(int nSubjectID)
{
	CString strDeleteSubject = cwsprintf
		(
			_T("DELETE FROM %s WHERE %s = %s"), 
			TRS_Subjects::GetStaticName(),
			TRS_Subjects::szSubjectID, 
			m_pSqlSession->m_pSqlConnection->NativeConvert(&DataLng(nSubjectID))
		);

	CString strDeleteHierarchies = cwsprintf
		(
			_T("DELETE FROM %s WHERE %s = %s OR %s = %s"), 
			TRS_SubjectsHierarchy::GetStaticName(),
			TRS_SubjectsHierarchy::szMasterSubjectID,
	 		m_pSqlSession->m_pSqlConnection->NativeConvert(&DataLng(nSubjectID)),
			TRS_SubjectsHierarchy::szSlaveSubjectID,
			m_pSqlSession->m_pSqlConnection->NativeConvert(&DataLng(nSubjectID))
		);

	TRY
	{
		m_pSqlSession->StartTransaction();
		m_pSqlSession->GetSqlConnection()->ExecuteSQL(strDeleteSubject);
		m_pSqlSession->GetSqlConnection()->ExecuteSQL(strDeleteHierarchies);
		m_pSqlSession->Commit();
	}
	CATCH(SqlException, e)
	{
		m_pSqlSession->Abort();
		THROW(e);
	}
	END_CATCH
}

///////////////////////////////////////////////////////////////////////////////
//					CRowSecurityManager declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRowSecurityManager, CObject)

//-----------------------------------------------------------------------------
CRowSecurityManager::CRowSecurityManager()
	:
	m_bIsValid(FALSE)
{
	m_pSqlSession			= AfxGetDefaultSqlConnection()->GetNewSqlSession();
	m_pEntitiesManager		= new CEntitiesManager(m_pSqlSession);
	m_pSubjectsManager		= new CSubjectsManager(m_pSqlSession);
	m_pGrantsManager		= new CGrantsManager(m_pSqlSession, m_pSubjectsManager);
	m_pMaintenanceManager	= new CRSMaintenanceManager(m_pSqlSession, m_pSubjectsManager, m_pGrantsManager, m_pEntitiesManager);
}

//-----------------------------------------------------------------------------
CRowSecurityManager::~CRowSecurityManager()
{
	// visto che il RowSecurityManager è un oggetto del CompanyContext, al momento della sua distruzione la connessione 
	// risulta già chiusa e distrutta e con essa tutte le sue sessioni ancora presenti
	//m_pSqlSession->Close();
	//delete m_pSqlSession;

	delete m_pEntitiesManager;
	delete m_pSubjectsManager;
	delete m_pGrantsManager;
	delete m_pMaintenanceManager;
}

//-----------------------------------------------------------------------------
void CRowSecurityManager::LoadSingleRowSecurityInfo(const CTBNamespace& aModuleNS)
{
	BOOL bCrsFile = FALSE;

	// prima carico il file con estensione .crs se esiste, altrimenti carico i file .xml
	CString strFileName = AfxGetPathFinder()->GetModuleObjectsPath(aModuleNS, CPathFinder::STANDARD) + SLASH_CHAR + szCrsRowSecurityFileName;
	CString crsString;

	if (ExistFile(strFileName))
	{
		crsString = OpenCrsFile(strFileName);
		if (crsString.IsEmpty()) return;
		bCrsFile = TRUE;
	}
	else
	{
		strFileName = AfxGetPathFinder()->GetModuleObjectsPath(aModuleNS, CPathFinder::STANDARD) + SLASH_CHAR + szRowSecurityFileName;
		if (!ExistFile(strFileName))
			return;
	}
	
	CLocalizableXMLDocument aXMLDefDoc(aModuleNS, AfxGetPathFinder());
	aXMLDefDoc.EnableMsgMode(FALSE);

	if (bCrsFile ? aXMLDefDoc.LoadXML(crsString) : aXMLDefDoc.LoadXMLFile(strFileName))
	{
		CXMLNode* pRoot = aXMLDefDoc.GetRoot();
		if (pRoot) 
		{			
			CXMLNode* pnNode;
			CXMLNode* pnChild = NULL;
			CXMLNodeChildsList* pnChilds = NULL;			
			//ci sono delle Entities
			if (pnNode = pRoot->GetChildByName(_T("Entities")))
				m_pEntitiesManager->LoadInfoFromXML(pnNode);				

			//ci sono delle tabelle da mettere sotto protezione
			//ci sono delle Entities
			if (pnNode = pRoot->GetChildByName(_T("Tables")))
			{
				if (pnChilds = pnNode->GetChilds())
				{
					for (int i =0; i < pnChilds->GetCount(); i++)
					{
						pnChild = pnChilds->GetAt(i);			
						if (pnChild)
						{
							RSProtectedTableInfo* pRSTable = new RSProtectedTableInfo();
							if (pRSTable->Parse(pnChild))
							{
								const SqlCatalogEntry* pSqlCatalogEntry = m_pSqlSession->GetSqlConnection()->GetCatalogEntry(pRSTable->m_strTableName);
								ASSERT(pSqlCatalogEntry);
								if (pSqlCatalogEntry)
									((SqlCatalogEntry*)pSqlCatalogEntry)->SetProtected(new CTableRowSecurityMng(m_pSqlSession, pRSTable));
							}
							else
								delete pRSTable;
						}
					}		
				}
			}
		}
		aXMLDefDoc.Close();
	}
}

//-----------------------------------------------------------------------------
void CRowSecurityManager::LoadProtectionInformation()
{
	//devo caricare le informazioni di RowSecurity indipendentemente dall'attivazione di un'applicazione/modulo 
	//vado direttamente sul FileSystem e non mi baso sulla struttura in memoria
	CPathFinder* pPathFinder = AfxGetPathFinder();
	CStringArray arApps;
	CString strAppName;
	CStringArray aAppModules;
	pPathFinder->GetCandidateApplications(&arApps); 

	for (int i=0; i <= arApps.GetUpperBound(); i++)
	{
		strAppName = arApps.GetAt(i);
		aAppModules.RemoveAll();
		if (!strAppName.IsEmpty())
		{
			AfxGetPathFinder()->GetCandidateModulesOfApp(strAppName, &aAppModules); 
			for (int i=0; i <= aAppModules.GetUpperBound(); i++)
			{
				CTBNamespace aModuleNS(CTBNamespace::MODULE, strAppName + CTBNamespace::GetSeparator() + aAppModules.GetAt (i));
				LoadSingleRowSecurityInfo(aModuleNS);
			}
		}				
	}
	//leggo da RS_Configuration
	TRS_Configuration aConfRec;
	SqlTable aTable(&aConfRec, m_pSqlSession);
	TRY
	{
		aTable.Open();
		aTable.SelectAll();
		aTable.Query();
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		if (aTable.IsOpen())
			aTable.Close();
		m_bIsValid = FALSE;
		return;
	}
	END_CATCH
		//se il worker corrente non è presente tra i subject allora invalido la rowsecurity in modo da far lanciare la procedura di configurazione
	m_bIsValid = aConfRec.f_IsValid && (AfxGetLoggedSubject() != NULL);
	m_pEntitiesManager->SetProtectionInformation(aConfRec.f_UsedEntries.GetString());
}

//--------------------------------------------------------------------------
void CRowSecurityManager::ModifyImplicitGrant(const CString& strEntityName, int rowSecurityID, int nOldWorkerID, int nNewWorkerID, CAbstractFormDoc* pDoc)
{
	TRY
	{
		m_pGrantsManager->ModifyImplicitGrant(strEntityName, rowSecurityID, nOldWorkerID, nNewWorkerID, pDoc);
	}
	CATCH(SqlException, e)	
	{
		throw(e);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
Array* CRowSecurityManager::PutRecordUnderProtection(const CString& strEntityName, SqlRecord* pRecord)
{
	TRY
	{
		return m_pGrantsManager->PutRecordUnderProtection(strEntityName, pRecord);
	}
	CATCH(SqlException, e)	
	{
		throw(e);
	}
	END_CATCH
}

// Esegue l'update della colonna IsValid sulla tabella RS_Configuration con il valore passato come parametro
//--------------------------------------------------------------------------
BOOL CRowSecurityManager::UpdateRSConfiguration(const BOOL& bSetValid)
{ 
	// prima estraggo i valori dell'unico record, che mi servono dopo per il TU
	DataLng nOfficeID;
	DataStr strUsedEntries;

	TRS_Configuration aConfRec;
	SqlTable aTable(&aConfRec, m_pSqlSession);
	TRY
	{
		aTable.Open();
		aTable.SelectAll();
		aTable.Query();
		nOfficeID = aConfRec.f_OfficeID;
		strUsedEntries = aConfRec.f_UsedEntries;
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		if (aTable.IsOpen()) aTable.Close();
			return FALSE;
	}
	END_CATCH

	// istanzio il TU e aggiorno l'IsValid
	TURS_Configuration rsConfigurationTU;
	rsConfigurationTU.SetSqlSession(m_pSqlSession);
	rsConfigurationTU.SetAutocommit(); 

	TRS_Configuration* record;

	TRY
	{
		if (rsConfigurationTU.FindRecord(DataLng(nOfficeID), FALSE) == TableUpdater::FOUND)
		{
			record = rsConfigurationTU.GetRecord();
			record->f_OfficeID = nOfficeID;
			record->f_UsedEntries = strUsedEntries;
			record->f_IsValid = bSetValid;
			rsConfigurationTU.UpdateRecord();
		}
	}
	CATCH(SqlException,e)
	{
		m_pSqlSession->Abort();
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		m_bIsValid = FALSE;
		return FALSE;
	}
	END_CATCH

	m_bIsValid = bSetValid;
	return TRUE;
}

//=========================================================================================
CRowSecurityManager* AFXAPI AfxGetRowSecurityManager()
{
	return (CRowSecurityManager*) AfxGetIRowSecurityManager();	
}

//=========================================================================================
CSubjectsManager* AFXAPI AfxGetSubjectsManager()
{
	CRowSecurityManager* pRowSecurityManager = AfxGetRowSecurityManager();	
	return (pRowSecurityManager) ? pRowSecurityManager->m_pSubjectsManager : NULL;
}

//=========================================================================================
 CGrantsManager* AFXAPI AfxGetGrantsManager()
 {
	CRowSecurityManager* pRowSecurityManager = AfxGetRowSecurityManager();	
	return (pRowSecurityManager) ? pRowSecurityManager->m_pGrantsManager : NULL;
 }

//=========================================================================================
CSubjectCache* AFXAPI AfxGetLoggedSubject()
{
	CSubjectsManager* pSubjectManager = AfxGetSubjectsManager();	
	return (pSubjectManager) ? pSubjectManager->GetSubjectCacheFromWorkerID(AfxGetWorkerId()) : NULL;
}

//=========================================================================================
CEntitiesManager* AFXAPI AfxGetEntitiesManager()
{
	CRowSecurityManager* pRowSecurityManager = AfxGetRowSecurityManager();	
	return (pRowSecurityManager) ? pRowSecurityManager->m_pEntitiesManager : NULL;
}
