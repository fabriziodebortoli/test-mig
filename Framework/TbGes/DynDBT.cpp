#include "stdafx.h"
#include "DynDBT.h"

IMPLEMENT_DYNAMIC(CPredicate, CObject)
IMPLEMENT_DYNAMIC(CFieldPredicate, CPredicate)
IMPLEMENT_DYNAMIC(CValuePredicate, CPredicate)
IMPLEMENT_DYNAMIC(CForeignKeyPredicate, CFieldPredicate)

//-----------------------------------------------------------------------------
CRuntimeClass* GetSqlRecordClass(CString sTableName)
{
	SqlCatalog* pCatalog = AfxGetSqlCatalog(AfxGetDefaultSqlConnection());
	if (!pCatalog)
		return NULL;

	POSITION pos;
	CString key;
	SqlCatalogEntry* pCatalogEntry;

	const CDbObjectDescription* pDescri = AfxGetDbObjectDescription(sTableName);

	for (pos = pCatalog->GetStartPosition(); pos != NULL;)
	{
		pCatalog->GetNextAssoc(pos, key, (CObject*&)pCatalogEntry);

		if (pCatalogEntry->m_strTableName.CompareNoCase(sTableName) != 0)
			continue;

		if (pDescri && pDescri->GetDeclarationType() < CDbObjectDescription::Dynamic)
		{
			CRuntimeClass* pClass = pCatalogEntry->GetSqlRecordClass();
			if (!pClass)
			{
				AfxGetTbCmdManager()->LoadNeededLibraries(pCatalogEntry->GetNamespace());
			}
			return pCatalogEntry->GetSqlRecordClass();
		}
		else
			return NULL;
	}

	return NULL;
}

IMPLEMENT_DYNAMIC(DynDBTMaster, DBTMaster)


//----------------------------------------------------------------------------
DynDBTMaster* DynDBTMaster::Create(const CString& sTableName, CAbstractFormDoc* pDocument, const CString& sName)
{
	CRuntimeClass* pClass = GetSqlRecordClass(sTableName);
	return pClass
		? new DynDBTMaster(pClass, pDocument, sName)			//SqlRecord tipizzato
		: new DynDBTMaster(sTableName, pDocument, sName);	//SqlRecord dinamico
}
//----------------------------------------------------------------------------
void DynDBTMaster::OnPrepareBrowser(SqlTable* pTable)
{
	pTable->SelectAll();
		
	if (m_pTable)
		pTable->m_strSort = m_pTable->m_strSort;
	int nParam = 0;
	m_BrowserQuery.OnDefineDynamicQuery(pTable, nParam);
	nParam = 0;
	m_BrowserQuery.OnPrepareDynamicQuery(pTable, NULL, nParam);

}
//----------------------------------------------------------------------------
void DynDBTMaster::OnDefineQuery()
{
	int nID = 0;
	m_Query.OnDefineDynamicQuery(m_pTable, nID);
	SqlRecord * pRecord = m_pTable->GetRecord();
	for (int i = 0; i < pRecord->GetCount(); i++)
	{
		SqlRecordItem* pItem = pRecord->GetAt(i);
		if (!pItem->IsSpecial())
			continue;
		nID++;

		DataObj* pDataObj = pItem->GetDataObj();

		m_pTable->AddFilterColumn(*pDataObj);
		m_pTable->AddParam(PARAM_PREFIX(nID), *pDataObj);

		// order by
		m_pTable->AddSortColumn(*pDataObj);
	}
	__super::OnDefineQuery();
}

//----------------------------------------------------------------------------
void DynDBTMaster::OnPrepareQuery()
{
	int nID = 0;
	m_Query.OnPrepareDynamicQuery(m_pTable, NULL, nID);
	SqlRecord * pRecord = m_pTable->GetRecord();
	for (int i = 0; i < pRecord->GetCount(); i++)
	{
		SqlRecordItem* pItem = pRecord->GetAt(i);
		if (!pItem->IsSpecial())
			continue;
		nID++;

		DataObj* pDataObj = pItem->GetDataObj();

		m_pTable->SetParamValue(PARAM_PREFIX(nID), *pDataObj);
	}
	__super::OnPrepareQuery();
}

//----------------------------------------------------------------------------
CPredicateContainer::~CPredicateContainer()
{
	for (int i = 0; i < m_arPredicates.GetCount(); i++)
		delete (m_arPredicates[i]);
}


//----------------------------------------------------------------------------
void CPredicateContainer::SplitQualifiedName(const CString& sQualified, CString& sTable, CString &sColumn)
{
	int idx = sQualified.Find(_T("."));
	if (idx != -1)
	{
		sTable = sQualified.Left(idx);
		sColumn = sQualified.Mid(idx);
	}
	else
	{
		sColumn = sQualified;
	}
}

//----------------------------------------------------------------------------
void CPredicateContainer::OnDefineDynamicQuery(SqlTable* pTable, int& nParam)
{
	pTable->SelectAll();
	SqlRecord * pRecord = pTable->GetRecord();
	for (int i = 0; i < m_arPredicates.GetCount(); i++)
	{
		CPredicate* pPredicate = m_arPredicates[i];

		nParam++;
		CString sTable, sColumn;
		SplitQualifiedName(pPredicate->m_sField, sTable, sColumn);

		// devo trovare il DataObj corretto che contiene il valore di query
		::DataObj* pDataObj = pRecord->GetDataObjFromColumnName(sColumn);
		if (!pDataObj)
		{
			ASSERT(FALSE);
			continue;
		}
		pTable->AddFilterColumn(*pDataObj, pPredicate->m_sOperator);
		pTable->AddParam(PARAM_PREFIX(nParam), *pDataObj);
	}
}
//----------------------------------------------------------------------------
void CPredicateContainer::OnPrepareDynamicQuery(SqlTable* pTable, SqlRecord* pMasterRecord, int& nParam)
{
	//iterate through predicates and set query values
	for (int i = 0; i < m_arPredicates.GetCount(); i++)
	{
		nParam++;
		CPredicate* pPredicate = m_arPredicates[i];
		::DataObj* pDataObj = NULL;
		if (pPredicate->IsKindOf(RUNTIME_CLASS(CForeignKeyPredicate)))
		{
			CForeignKeyPredicate* pFieldPredicate = (CForeignKeyPredicate*)pPredicate;

			CString sTable, sColumn;
			SplitQualifiedName(pFieldPredicate->m_sFieldParameter, sTable, sColumn);
			pDataObj = pMasterRecord->GetDataObjFromColumnName(sColumn);

			if (!pDataObj)
			{
				ASSERT(FALSE);
				continue;
			}
			pTable->SetParamValue(PARAM_PREFIX(nParam), *pDataObj);
		}
		else if (pPredicate->IsKindOf(RUNTIME_CLASS(CFieldPredicate)))
		{
			CFieldPredicate* pFieldPredicate = (CFieldPredicate*)pPredicate;
			
			CString sTable, sColumn;
			SplitQualifiedName(pFieldPredicate->m_sFieldParameter, sTable, sColumn);
			SqlRecord* pRecord = NULL;
			if (pMasterRecord && pMasterRecord->GetTableName() == sTable)
				pRecord = pMasterRecord;
			else
				pRecord = pTable->GetRecord();

			pDataObj = pRecord->GetDataObjFromColumnName(sColumn);

			if (!pDataObj)
			{
				ASSERT(FALSE);
				continue;
			}
			pTable->SetParamValue(PARAM_PREFIX(nParam), *pDataObj);
		}
		else if (pPredicate->IsKindOf(RUNTIME_CLASS(CValuePredicate)))
		{
			CValuePredicate* pValuePredicate = (CValuePredicate*)pPredicate;
			// devo trovare il DataObj corretto per sapere il tipo di dato
			CString sTable, sColumn;
			SplitQualifiedName(pValuePredicate->m_sField, sTable, sColumn);
			::DataObj* pTemplateDataObj = pTable->GetRecord()->GetDataObjFromColumnName(sColumn);

			if (!pTemplateDataObj)
			{
				ASSERT(FALSE);
				continue;
			}
			pDataObj = pTemplateDataObj->Clone();
			pDataObj->AssignFromXMLString(pValuePredicate->m_sFieldValue);
			pTable->SetParamValue(PARAM_PREFIX(nParam), *pDataObj);
			delete pDataObj;
		}
		
	}

}
//----------------------------------------------------------------------------
void CPredicateContainer::OnPrepareDynamicPrimaryKey(SqlRecord* pRecord, SqlRecord* pMasterRecord)
{
	// default primary key algorithm
	for (int i = 0; i < m_arPredicates.GetCount(); i++)
	{
		CPredicate* pPredicate = m_arPredicates[i]; 
		if (!pPredicate->IsKindOf(RUNTIME_CLASS(CForeignKeyPredicate)))
			continue;
		CForeignKeyPredicate* pForeign = (CForeignKeyPredicate*)pPredicate;
		DataObj* pDataObj = pRecord->GetDataObjFromName(pForeign->m_sField);
		if (!pDataObj)
		{
			ASSERT(FALSE);
			continue;
		}
		
		DataObj* pValueDataObj = pValueDataObj = pMasterRecord->GetDataObjFromColumnName(pForeign->m_sFieldParameter);
		
		if (!pValueDataObj)
		{
			ASSERT(FALSE);
			continue;
		}
		pDataObj->Assign(*pValueDataObj);

	}
}
//----------------------------------------------------------------------------
void CPredicateContainer::AddForeignKey(const CString& sPrimary, const CString& sForeign)
{
	m_arPredicates.Add(new CForeignKeyPredicate(sPrimary, sForeign));
}


//----------------------------------------------------------------------------
void CPredicateContainer::AddFieldPredicate(const CString& sField, const CString& sValueField, const CString& sOperator)
{
	m_arPredicates.Add(new CFieldPredicate(sField, sValueField, sOperator));
}
//----------------------------------------------------------------------------
void CPredicateContainer::AddValuePredicate(const CString& sField, const CString& sValue, const CString& sOperator)
{
	m_arPredicates.Add(new CValuePredicate(sField, sValue, sOperator));
}

//---------------------------------------------------------------------------------------
void CPredicateContainer::CopyPredicates(CArray<CPredicate*>* pPredicates)
{
	ASSERT(m_arPredicates.GetCount() == 0);

	if (m_arPredicates.GetCount() > 0)
		return;

	for (int i = 0; i < pPredicates->GetCount(); i++)
	{
		if (pPredicates->GetAt(i)->IsKindOf(RUNTIME_CLASS(CForeignKeyPredicate)))
		{
			CForeignKeyPredicate* pObj = (CForeignKeyPredicate*)pPredicates->GetAt(i);
			AddForeignKey(pObj->m_sField, pObj->m_sFieldParameter);
		}
	}
}

IMPLEMENT_DYNAMIC(DynDBTSlave, DBTSlave)
//----------------------------------------------------------------------------
DynDBTSlave* DynDBTSlave::Create(const CString& sTableName, CAbstractFormDoc* pDocument, const CString& sName, BOOL bAllowEmpty)
{
	CRuntimeClass* pClass = GetSqlRecordClass(sTableName);
	return pClass
		? new DynDBTSlave(pClass, pDocument, sName, bAllowEmpty)			//SqlRecord tipizzato
		: new DynDBTSlave(sTableName, pDocument, sName, bAllowEmpty);	//SqlRecord dinamico
}
//----------------------------------------------------------------------------
void DynDBTSlave::OnDefineQuery()
{
	int nID = 0;
	m_Query.OnDefineDynamicQuery(m_pTable, nID);
	// order by
	SqlRecord * pRecord = m_pTable->GetRecord();
	for (int i = 0; i < pRecord->GetCount(); i++)
	{
		SqlRecordItem* pItem = pRecord->GetAt(i);
		if (!pItem->IsSpecial())
			continue;

		m_pTable->AddSortColumn(pItem->GetColumnName());
	}
	__super::OnDefineQuery();
}

//----------------------------------------------------------------------------
void DynDBTSlave::OnPrepareQuery()
{
	int nID = 0;
	m_Query.OnPrepareDynamicQuery(m_pTable, GetMasterRecord(), nID);
	__super::OnPrepareQuery();
}


//----------------------------------------------------------------------------
void DynDBTSlave::OnPreparePrimaryKey()
{
	m_Query.OnPrepareDynamicPrimaryKey(m_pRecord, GetMasterRecord());
	__super::OnPreparePrimaryKey();
}
IMPLEMENT_DYNAMIC(DynDBTSlaveBuffered, DBTSlaveBuffered)
//----------------------------------------------------------------------------
DynDBTSlaveBuffered* DynDBTSlaveBuffered::Create(const CString& sTableName, CAbstractFormDoc* pDocument, const CString& sName, BOOL bAllowEmpty, BOOL bCheckDuplicateKey)
{
	CRuntimeClass* pClass = GetSqlRecordClass(sTableName);
	return pClass
		? new DynDBTSlaveBuffered(pClass, pDocument, sName, bAllowEmpty, bCheckDuplicateKey)			//SqlRecord tipizzato
		: new DynDBTSlaveBuffered(sTableName, pDocument, sName, bAllowEmpty, bCheckDuplicateKey);	//SqlRecord dinamico
}
//----------------------------------------------------------------------------
void DynDBTSlaveBuffered::OnDefineQuery()
{
	int nID = 0;
	m_Query.OnDefineDynamicQuery(m_pTable, nID);

	// order by
	SqlRecord * pRecord = m_pTable->GetRecord();
	for (int i = 0; i < pRecord->GetCount(); i++)
	{
		SqlRecordItem* pItem = pRecord->GetAt(i);
		if (!pItem->IsSpecial())
			continue;

		m_pTable->AddSortColumn(pItem->GetColumnName());
	}
	__super::OnDefineQuery();
}

//----------------------------------------------------------------------------
void DynDBTSlaveBuffered::OnPrepareQuery()
{
	int nID = 0;
	m_Query.OnPrepareDynamicQuery(m_pTable, GetMasterRecord(), nID);
	__super::OnPrepareQuery();
}

//----------------------------------------------------------------------------
void DynDBTSlaveBuffered::OnPreparePrimaryKey()
{
	m_Query.OnPrepareDynamicPrimaryKey(m_pRecord, GetMasterRecord());
	__super::OnPreparePrimaryKey();
}